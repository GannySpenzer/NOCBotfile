﻿Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

'*************************************************************************************************************************
' new code to insert NBTY work orders in SYSADM8.PS_NLNK2_OBJCT_VAL (to be used in Shopping Cart WO validation) - VR 08/21/2019
'*************************************************************************************************************************
Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\NbtyWorkOrder"
    Dim logpath As String = "C:\NbtyWorkOrder\LOGS\NbtyWrkOrdXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\NbtyWorkOrder\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")

    Dim strOverride As String = ""
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String
    Private strDbName As String = ""

    Sub Main()

        ' Production source share: \\solaris2\PSSHARE\efi\NBTY\inbound 
        ' starting times: daily every hour at 09, 24, 39, 54

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG"
        Try
            cnStringORA = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnStringORA = ""
        End Try
        If (cnStringORA.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnStringORA)
            strDbName = cnStringORA.Substring(Len(cnStringORA) - 4)
        Else
            strDbName = "PROD"
        End If
        '   (2) root directory
        Dim rDir As String = ""
        Try
            rDir = My.Settings("rootDir").ToString.Trim
        Catch ex As Exception
        End Try
        If (rDir.Length > 0) Then
            rootDir = rDir
        End If
        '   (3) log path/file
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\NbtyWrkOrdXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
            sErrLogPath = sLogPath & "\NbtyWrkOrdXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & "421" & ".txt"
        End If

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Verbose)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        'process received info
        Dim sPlantList As String = "NE_10MFG,NE_90PKG,NE_105MFG,NE_815PKG,NE_4320WHS"
        Try
            sPlantList = My.Settings("Plant_List").ToString.Trim
        Catch ex As Exception
            sPlantList = "NE_10MFG,NE_90PKG,NE_105MFG,NE_815PKG,NE_4320WHS"
        End Try
        Dim arrPlantList() As String = Split(sPlantList, ",")

        Dim iM1 As Integer = 0
        Dim sPlantCode As String = ""
        If arrPlantList.Length > 0 Then
            For iM1 = 0 To arrPlantList.Length - 1
                sPlantCode = arrPlantList(iM1)

                Call ProceesNbtyWrkOrdXmlInInfo(sPlantCode)

            Next
        End If

        ' new code to insert NBTY work orders in SYSADM8.PS_NLNK2_OBJCT_VAL (to be used in Shopping Cart WO validation)
        Dim strPlantListForSql As String = sPlantList
        strPlantListForSql = Replace(strPlantListForSql, ",", "','")
        strPlantListForSql = "('" & strPlantListForSql & "')"

        'SDI-52594 Deleting NBTY WO in SYSADM8.PS_NLNK2_OBJCT_VAL[Change by Vishalini]
        Dim rowsaffected1 As Integer = 0
        Dim strDeleteString As String = "Delete SYSADM8.PS_NLNK2_OBJCT_VAL where CUST_ID = 'NBTY' AND ISA_OBJ_KEY = 'WORKORDER' AND ISA_ATTRVAL1 IN " & strPlantListForSql & ""
        Try

            Dim Command = New OleDbCommand(strDeleteString, connectOR)

            If Not connectOR.State = ConnectionState.Open Then
                connectOR.Open()
            End If
            rowsaffected1 = Command.ExecuteNonQuery()

            m_logger.WriteErrorLog(rtn & " ::  ")
            m_logger.WriteErrorLog(rtn & " :: Number of Work Orders deleted: " & rowsaffected1.ToString())
            myLoggr1.WriteErrorLog(rtn & " ::  ")
            myLoggr1.WriteErrorLog(rtn & " :: Number of Work Orders deleted: " & rowsaffected1.ToString())
            myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strDeleteString)
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error while trying to DELETE INTO SYSADM8.PS_NLNK2_OBJCT_VAL: " & ex.ToString())
        End Try
        Dim strSqlForWOInsert As String = ""
        strSqlForWOInsert = " INSERT INTO SYSADM8.PS_NLNK2_OBJCT_VAL(CUST_ID,ISA_OBJ_KEY,ISA_OBJECTID,DT_TIMESTAMP,ISA_ATTRLBL1,ISA_ATTRVAL1,ISA_ATTRLBL2, " & vbCrLf &
            " ISA_ATTRVAL2,ISA_ATTRLBL3,ISA_ATTRVAL3,ISA_ATTRLBL4,ISA_ATTRVAL4,ISA_ATTRLBL5,ISA_ATTRVAL5) " & vbCrLf &
            " SELECT DISTINCT 'NBTY' AS CUST_ID,'WORKORDER' AS ISA_OBJ_KEY,A.ISA_WORK_ORDER_NO AS ISA_OBJECTID," & vbCrLf &
            " TO_TIMESTAMP('" & Now.ToString("MM/dd/yyyy hh:mm:ss:ff5 tt") & "', 'MM/DD/YYYY HH:MI:SS:ff5 AM') AS DT_TIMESTAMP,'PLANT' AS ISA_ATTRLBL1," & vbCrLf &
            " A.PLANT AS ISA_ATTRVAL1, ' ' AS ISA_ATTRLBL2, ' ' AS ISA_ATTRVAL2, " & vbCrLf &
            " ' ' AS ISA_ATTRLBL3, ' ' AS ISA_ATTRVAL3, ' ' AS ISA_ATTRLBL4, ' ' AS ISA_ATTRVAL4,' ' AS ISA_ATTRLBL5, ' ' AS ISA_ATTRVAL5" & vbCrLf &
            " FROM SYSADM8.PS_ISA_NB_WOVAL A WHERE A.PLANT IN " & strPlantListForSql & " AND " & vbCrLf &
            " NOT EXISTS (  SELECT 'X' " & vbCrLf &
            " FROM SYSADM8.PS_NLNK2_OBJCT_VAL B " & vbCrLf &
            " WHERE B.CUST_ID = 'NBTY' AND B.ISA_OBJ_KEY = 'WORKORDER' AND B.ISA_ATTRLBL1 = 'PLANT' " & vbCrLf &
            " AND B.ISA_OBJECTID = A.ISA_WORK_ORDER_NO AND B.ISA_ATTRVAL1 = A.PLANT  )" & vbCrLf &
            " "

        If Not connectOR.State = ConnectionState.Open Then
            connectOR.Open()
        End If

        Dim rowsaffected As Integer = 0
        Try

            Dim Command As OleDbCommand = New OleDbCommand(strSqlForWOInsert, connectOR)
            Command.CommandTimeout = 120

            rowsaffected = Command.ExecuteNonQuery()

            m_logger.WriteErrorLog(rtn & " ::  ")
            m_logger.WriteErrorLog(rtn & " :: Number of Work Orders inserted: " & rowsaffected.ToString())
            myLoggr1.WriteErrorLog(rtn & " ::  ")
            myLoggr1.WriteErrorLog(rtn & " :: Number of Work Orders inserted: " & rowsaffected.ToString())
            myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSqlForWOInsert)

            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
        Catch exInsrt As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error while trying to INSERT INTO SYSADM8.PS_NLNK2_OBJCT_VAL: " & exInsrt.ToString())
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
        End Try

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Sub ProceesNbtyWrkOrdXmlInInfo(ByVal sPlantCode As String)

        Dim rtn As String = "NbtyWrkOrdXmlIn.ProceesNbtyWrkOrdXmlInInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start NBTY Work Ord. XML in for: " & sPlantCode)
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of NBTY Work Ord. Inbound for: " & sPlantCode)

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "C:\NbtyWorkOrder\XMLIN_SRC"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "C:\NbtyWorkOrder\XMLIN_SRC"
        End Try
        sInputDir = sInputDir & "\" & sPlantCode

        Dim dirInfo As DirectoryInfo = Nothing  '  New DirectoryInfo(sInputDir)

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = Nothing   '  dirInfo.GetFiles(strFiles)

        Try
            dirInfo = New DirectoryInfo(sInputDir)
            strFiles = "*.XML"
            aSrcFiles = dirInfo.GetFiles(strFiles)
        Catch exDirInf As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error retrieving Directory Info from: " & sInputDir)
            myLoggr1.WriteErrorLog(rtn & " :: " & exDirInf.ToString)
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = rtn & " :: Error retrieving Directory Info from: " & sInputDir
            Else
                m_arrXMLErrFiles &= "," & rtn & " :: Error retrieving Directory Info from: " & sInputDir
            End If
            Dim strErrDirInfo As String = exDirInf.ToString
            If Len(strErrDirInfo) > 250 Then
                strErrDirInfo = Microsoft.VisualBasic.Left(strErrDirInfo, 250)
            End If
            If Trim(m_arrErrorsList) = "" Then
                m_arrErrorsList = strErrDirInfo
            Else
                m_arrErrorsList &= "," & strErrDirInfo
            End If
            ' send error e-mail
            SendEmail(True)
            Exit Sub
        End Try

        Dim I As Integer = 0

        Try
            If aSrcFiles.Length > 0 Then
                Dim bFound As Boolean = False
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > Len("cmrpt_sdi_wo") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("cmrpt_sdi_wo") Then
                            bFound = True
                            File.Copy(aSrcFiles(I).FullName, rootDir & "\XMLIN\" & sPlantCode & "\" & aSrcFiles(I).Name, True)
                            'File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " copied to " & rootDir & "\XMLIN\" & sPlantCode & "\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
                If Not bFound Then
                    m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
                End If
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to " & rootDir & "\XMLIN\" & sPlantCode & "\" & " ...")
            myLoggr1.WriteErrorLog(rtn & " :: " & ex.ToString)
            bError = True
            Dim strXMLError As String = ex.Message
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = aSrcFiles(I).FullName
            Else
                m_arrXMLErrFiles &= "," & aSrcFiles(I).FullName
            End If
            If Trim(strXMLError) <> "" Then
                If Len(strXMLError) > 250 Then
                    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                End If
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = strXMLError
                Else
                    m_arrErrorsList &= "," & strXMLError
                End If
            Else
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = "No Error Description"
                Else
                    m_arrErrorsList &= "," & "No Error Description"
                End If
            End If
        End Try

        If bError Then
            SendEmail(True)
        End If

        '// ***
        '// CytecMatMastIn Processing: inbound to the Oracle table SYSADM8.PS_ISA_NB_WOVAL ...
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = GetNbtyWrkOrdXmlIn(sPlantCode)
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: " & ex.ToString)
            bolError = True
        End Try

        If bolError = True Or bolWarning = True Then
            SendEmail(True)
        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        m_logger.WriteInformationLog(rtn & " :: End of NBTY Work Ord. IN Process")

    End Sub

    Private Function GetNbtyWrkOrdXmlIn(ByVal sPlantCode As String) As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "NbtyWrkOrdXmlIn.GetNbtyWrkOrdXmlIn." & sPlantCode

        Console.WriteLine("Start Insert of NBTY Work Ord. in SYSADM8.PS_ISA_NB_WOVAL for: " & sPlantCode)
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of NBTY Work Ord. in SYSADM8.PS_ISA_NB_WOVAL for: " & sPlantCode)

        Dim dirInfo As DirectoryInfo = New DirectoryInfo(rootDir & "\XMLIN\" & sPlantCode & "\")
        Dim strFiles As String = ""
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bLineError As Boolean = False

        Dim xmlRequest As New XmlDocument

        Dim I As Integer

        strFiles = "*.XML"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim root As XmlElement

        Dim sXMLFilename As String = ""

        Try
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
            If aFiles.Length > 0 Then

                connectOR.Open()

                For I = 0 To aFiles.Length - 1
                    sXMLFilename = aFiles(I).Name
                    bLineError = False
                    bolError = False
                    strXMLError = ""
                    m_logger.WriteInformationLog(rtn & " :: Start File " & aFiles(I).Name)
                    'here where we load the xml into memory
                    sr = New System.IO.StreamReader(aFiles(I).FullName)
                    XMLContent = sr.ReadToEnd()
                    XMLContent = Replace(XMLContent, "&", "&amp;")
                    'XMLContent = Replace(XMLContent, "'", "&#39;")
                    'XMLContent = Replace(XMLContent, """", "&quot;")
                    sr.Close()

                    Try
                        xmlRequest.LoadXml(XMLContent)
                    Catch exLoad As Exception
                        Console.WriteLine("")
                        Console.WriteLine("***error - " & exLoad.ToString)
                        Console.WriteLine("")
                        myLoggr1.WriteErrorLog(rtn & " :: Error " & exLoad.Message.ToString & " in file " & aFiles(I).Name)
                        strXMLError = exLoad.ToString
                        bolError = True
                        bLineError = True
                        Try
                            File.Move(aFiles(I).FullName, rootDir & "\BadXML\" & sPlantCode & "\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try

                    If Trim(strXMLError) = "" Then
                        root = xmlRequest.DocumentElement

                        If root.FirstChild Is Nothing Then
                            strXMLError = "empty XML file for: " & sPlantCode
                        ElseIf root.LastChild.Name.ToLower() = "cmrpt_sdi_wo" Then
                            strXMLError = ""
                        Else
                            strXMLError = "Missing last XML Element for: " & sPlantCode
                        End If

                        Dim rowsaffected As Integer = 0
                        If Trim(strXMLError) = "" Then
                            If root.ChildNodes.Count > 0 Then
                                'code to delete previous info based on Plant Code - sPlantCode
                                sPlantCode = UCase(sPlantCode)
                                Dim strDeleteString As String = "DELETE SYSADM8.PS_ISA_NB_WOVAL WHERE UPPER(PLANT) = '" & sPlantCode & "'"
                                Try

                                    Dim Command = New OleDbCommand(strDeleteString, connectOR)

                                    If Not connectOR.State = ConnectionState.Open Then
                                        connectOR.Open()
                                    End If
                                    rowsaffected = Command.ExecuteNonQuery()

                                Catch ex As Exception

                                    myLoggr1.WriteErrorLog(rtn & " :: Error deleting: " & ex.Message & " for the file: " & aFiles(I).Name)
                                End Try
                                'End code to delete
                                Dim strPlant As String = ""
                                Dim strWorkOrdNum As String = ""
                                Dim strAssetId As String = ""
                                Dim strIsaUnloadPt As String = ""
                                Dim strIsaWoStatus As String = ""
                                Dim strIsaCustChrcd As String = ""
                                Dim strWhouseId As String = ""
                                Dim strWoDescr As String = ""
                                Dim strWoTypeId As String = ""
                                Dim strPriortyId As String = ""
                                Dim strSuperName As String = ""
                                Dim strSuperId As String = ""
                                Dim strSchedDate As String = ""
                                Dim dtSchedDate As DateTime = Nothing
                                Dim strAddDate As String = ""
                                Dim dtAddDate As DateTime = Nothing
                                Dim strProcdId As String = ""

                                Dim iCnt As Integer = 0
                                For iCnt = 0 To root.ChildNodes.Count - 1
                                    If LCase(root.ChildNodes(iCnt).Name) = "cmrpt_sdi_wo" Then
                                        Dim nodeMxItem As XmlNode = root.ChildNodes(iCnt)
                                        'bLineError = False
                                        If nodeMxItem.ChildNodes.Count > 0 Then
                                            strPlant = " "
                                            strWorkOrdNum = " "
                                            strAssetId = " "
                                            strIsaUnloadPt = " "
                                            strIsaWoStatus = " "
                                            strIsaCustChrcd = " "
                                            strWhouseId = " "
                                            strWoDescr = " "
                                            strWoTypeId = " "
                                            strPriortyId = " "
                                            strSuperName = " "
                                            strSuperId = " "
                                            strSchedDate = " "
                                            dtSchedDate = Nothing
                                            strAddDate = " "
                                            dtAddDate = Nothing
                                            strProcdId = " "

                                            Dim iItemMM As Integer = 0
                                            Dim strNodeName As String = ""
                                            For iItemMM = 0 To nodeMxItem.ChildNodes.Count - 1
                                                strNodeName = LCase(nodeMxItem.ChildNodes(iItemMM).Name)
                                                Select Case strNodeName
                                                    Case "cmrpt_sdi_wo.plant"
                                                        Try
                                                            strPlant = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strPlant) = "" Then
                                                                strPlant = " "
                                                            Else
                                                                strPlant = Trim(strPlant)
                                                                strPlant = Replace(Replace(strPlant, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strPlant)) > 30 Then
                                                                strPlant = Microsoft.VisualBasic.Left(strPlant, 30)
                                                            End If
                                                        Catch ex As Exception
                                                            strPlant = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.wo_id"
                                                        Try
                                                            strWorkOrdNum = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strWorkOrdNum) = "" Then
                                                                strWorkOrdNum = " "
                                                            Else
                                                                strWorkOrdNum = Trim(strWorkOrdNum)
                                                                strWorkOrdNum = Replace(Replace(strWorkOrdNum, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strWorkOrdNum)) > 20 Then
                                                                strWorkOrdNum = Microsoft.VisualBasic.Left(strWorkOrdNum, 20)
                                                            End If
                                                        Catch ex As Exception
                                                            strWorkOrdNum = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.asset_id"
                                                        Try
                                                            strAssetId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strAssetId) = "" Then
                                                                strAssetId = " "
                                                            Else
                                                                strAssetId = Trim(strAssetId)
                                                                strAssetId = Replace(Replace(strAssetId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strAssetId)) > 40 Then
                                                                strAssetId = Microsoft.VisualBasic.Left(strAssetId, 40)
                                                            End If
                                                        Catch ex As Exception
                                                            strAssetId = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.bldg_id"
                                                        Try
                                                            strIsaUnloadPt = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strIsaUnloadPt) = "" Then
                                                                strIsaUnloadPt = " "
                                                            Else
                                                                strIsaUnloadPt = Trim(strIsaUnloadPt)
                                                                strIsaUnloadPt = Replace(Replace(strIsaUnloadPt, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strIsaUnloadPt)) > 60 Then
                                                                strIsaUnloadPt = Microsoft.VisualBasic.Left(strIsaUnloadPt, 60)
                                                            End If
                                                        Catch ex As Exception
                                                            strIsaUnloadPt = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.status_id"
                                                        Try
                                                            strIsaWoStatus = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strIsaWoStatus) = "" Then
                                                                strIsaWoStatus = " "
                                                            Else
                                                                strIsaWoStatus = Trim(strIsaWoStatus)
                                                                strIsaWoStatus = Replace(Replace(strIsaWoStatus, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strIsaWoStatus)) > 10 Then
                                                                strIsaWoStatus = Microsoft.VisualBasic.Left(strIsaWoStatus, 10)
                                                            End If
                                                        Catch ex As Exception
                                                            strIsaWoStatus = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.gl_id"
                                                        Try
                                                            strIsaCustChrcd = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strIsaCustChrcd) = "" Then
                                                                strIsaCustChrcd = " "
                                                            Else
                                                                strIsaCustChrcd = Trim(strIsaCustChrcd)
                                                                strIsaCustChrcd = Replace(Replace(strIsaCustChrcd, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strIsaCustChrcd)) > 60 Then
                                                                strIsaCustChrcd = Microsoft.VisualBasic.Left(strIsaCustChrcd, 60)
                                                            End If
                                                        Catch ex As Exception
                                                            strIsaCustChrcd = " "
                                                        End Try
                                                        If Trim(strIsaCustChrcd) = "" Then
                                                            strIsaCustChrcd = " "
                                                        End If
                                                    Case "cmrpt_sdi_wo.warehouse_id"
                                                        Try
                                                            strWhouseId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strWhouseId) = "" Then
                                                                strWhouseId = " "
                                                            Else
                                                                strWhouseId = Trim(strWhouseId)
                                                                strWhouseId = Replace(Replace(strWhouseId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strWhouseId)) > 30 Then
                                                                strWhouseId = Microsoft.VisualBasic.Left(strWhouseId, 30)
                                                            End If
                                                        Catch ex As Exception
                                                            strWhouseId = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.wo_descr"
                                                        Try
                                                            strWoDescr = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strWoDescr) = "" Then
                                                                strWoDescr = " "
                                                            Else
                                                                strWoDescr = Trim(strWoDescr)
                                                                strWoDescr = Replace(Replace(strWoDescr, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strWoDescr)) > 254 Then
                                                                strWoDescr = Microsoft.VisualBasic.Left(strWoDescr, 254)
                                                            End If
                                                        Catch ex As Exception
                                                            strWoDescr = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.work_type_id"
                                                        Try
                                                            strWoTypeId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strWoTypeId) = "" Then
                                                                strWoTypeId = " "
                                                            Else
                                                                strWoTypeId = Trim(strWoTypeId)
                                                            End If
                                                            If Len(Trim(strWoTypeId)) > 5 Then
                                                                strWoTypeId = Microsoft.VisualBasic.Left(strWoTypeId, 5)
                                                            End If
                                                        Catch ex As Exception
                                                            strWoTypeId = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.priority_id"
                                                        Try
                                                            strPriortyId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strPriortyId) = "" Then
                                                                strPriortyId = " "
                                                            Else
                                                                strPriortyId = Trim(strPriortyId)
                                                            End If
                                                            If Len(Trim(strPriortyId)) > 3 Then
                                                                strPriortyId = Microsoft.VisualBasic.Left(strPriortyId, 3)
                                                            End If
                                                        Catch ex As Exception
                                                            strPriortyId = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.supvr_name"
                                                        Try
                                                            strSuperName = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strSuperName) = "" Then
                                                                strSuperName = " "
                                                            Else
                                                                strSuperName = Trim(strSuperName)
                                                                strSuperName = Replace(Replace(strSuperName, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strSuperName)) > 40 Then
                                                                strSuperName = Microsoft.VisualBasic.Left(strSuperName, 40)
                                                            End If
                                                        Catch ex As Exception
                                                            strSuperName = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.supvr_id"
                                                        Try
                                                            strSuperId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strSuperId) = "" Then
                                                                strSuperId = " "
                                                            Else
                                                                strSuperId = Trim(strSuperId)
                                                                strSuperId = Replace(Replace(strSuperId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strSuperId)) > 30 Then
                                                                strSuperId = Microsoft.VisualBasic.Left(strSuperId, 30)
                                                            End If
                                                        Catch ex As Exception
                                                            strSuperId = " "
                                                        End Try
                                                    Case "cmrpt_sdi_wo.date_sched"
                                                        Try
                                                            strSchedDate = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strSchedDate) = "" Then
                                                                dtSchedDate = Nothing
                                                            Else
                                                                strSchedDate = Trim(strSchedDate)
                                                                If IsDate(strSchedDate) Then
                                                                    dtSchedDate = CType(strSchedDate, DateTime)
                                                                Else
                                                                    dtSchedDate = Nothing
                                                                End If
                                                            End If
                                                        Catch ex As Exception
                                                            dtSchedDate = Nothing
                                                        End Try
                                                    Case "cmrpt_sdi_wo.date_added"
                                                        Try
                                                            strAddDate = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strAddDate) = "" Then
                                                                dtAddDate = Nothing
                                                            Else
                                                                strAddDate = Trim(strAddDate)
                                                                If IsDate(strAddDate) Then
                                                                    dtAddDate = CType(strAddDate, DateTime)
                                                                Else
                                                                    dtAddDate = Nothing
                                                                End If
                                                            End If
                                                        Catch ex As Exception
                                                            dtAddDate = Nothing
                                                        End Try
                                                    Case "cmrpt_sdi_wo.procedure_id"
                                                        Try
                                                            strProcdId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strProcdId) = "" Then
                                                                strProcdId = " "
                                                            Else
                                                                strProcdId = Trim(strProcdId)
                                                                strProcdId = Replace(Replace(strProcdId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strProcdId)) > 20 Then
                                                                strProcdId = Microsoft.VisualBasic.Left(strProcdId, 20)
                                                            End If
                                                        Catch ex As Exception
                                                            strProcdId = " "
                                                        End Try
                                                    Case Else
                                                        'DO NOTHING
                                                End Select  '  strNodeName

                                            Next  '  For iItemMM = 0 To nodeMxItem.ChildNodes.Count - 1

                                            'collected all info - starting insert 
                                            rowsaffected = 0
                                            Dim strSQLstring As String = ""
                                            strSQLstring = "INSERT INTO SYSADM8.PS_ISA_NB_WOVAL (PLANT,ISA_WORK_ORDER_NO,ISA_ASSET_ID,ISA_UNLOADING_PT" & vbCrLf & _
                                                ",ISA_WO_STATUS,ISA_CUST_CHARGE_CD,WAREHOUSE_ID,WO_DESCR" & vbCrLf & _
                                                ",WO_TYPE,PRIORITY_CD,SUPERVISORS_NAME,SUPERVISOR" & vbCrLf & _
                                                ",SCHED_DTTM,ADD_DTTM,ISA_PROCEDURE_ID) " & vbCrLf & _
                                                "VALUES('" & strPlant & "','" & strWorkOrdNum & "','" & strAssetId & "','" & strIsaUnloadPt & "'" & vbCrLf & _
                                                ",'" & strIsaWoStatus & "','" & strIsaCustChrcd & "','" & strWhouseId & "','" & strWoDescr & "'" & vbCrLf & _
                                                ",'" & strWoTypeId & "','" & strPriortyId & "','" & strSuperName & "','" & strSuperId & "'" & vbCrLf
                                            If dtSchedDate = Nothing Then
                                                strSQLstring = strSQLstring & ",NULL" & vbCrLf
                                            Else
                                                strSQLstring = strSQLstring & ",TO_TIMESTAMP('" & dtSchedDate.ToString("MM/dd/yyyy hh:mm:ss:ff5 tt") & "', 'MM/DD/YYYY HH:MI:SS:ff5 AM')"
                                            End If
                                            If dtAddDate = Nothing Then
                                                strSQLstring = strSQLstring & ",NULL" & vbCrLf
                                            Else
                                                strSQLstring = strSQLstring & ",TO_TIMESTAMP('" & dtAddDate.ToString("MM/dd/yyyy hh:mm:ss:ff5 tt") & "', 'MM/DD/YYYY HH:MI:SS:ff5 AM')"
                                            End If
                                            strSQLstring = strSQLstring & "" & _
                                                ",'" & strProcdId & "')"


                                            Try

                                                Dim Command = New OleDbCommand(strSQLstring, connectOR)

                                                If Not connectOR.State = ConnectionState.Open Then
                                                    connectOR.Open()
                                                End If
                                                rowsaffected = Command.ExecuteNonQuery()

                                                If rowsaffected = 0 Then
                                                    myLoggr1.WriteErrorLog(rtn & " :: Error while inserting: 'rowsaffected = 0' for the file: " & aFiles(I).Name & " and line number: " & (iCnt + 1).ToString())
                                                    myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                    bLineError = True
                                                    If Trim(strXMLError) = "" Then
                                                        strXMLError = sPlantCode & ": Check log file: error on line number(s): " & (iCnt + 1).ToString()
                                                    Else
                                                        strXMLError = strXMLError & ", " & (iCnt + 1).ToString()
                                                    End If
                                                End If
                                            Catch ex As Exception
                                                myLoggr1.WriteErrorLog(rtn & " :: Error inserting: " & ex.Message & " for the file: " & aFiles(I).Name & " and line number: " & (iCnt + 1).ToString())
                                                myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                bLineError = True
                                                If Trim(strXMLError) = "" Then
                                                    strXMLError = sPlantCode & ": Check log file: error on line number(s): " & (iCnt + 1).ToString()
                                                Else
                                                    strXMLError = strXMLError & ", " & (iCnt + 1).ToString()
                                                End If
                                            End Try
                                        End If  '  nodeMxItem.ChildNodes.Count > 0 ==> "cmrpt_sdi_wo" has childs 
                                    End If  '  LCase(root.ChildNodes(iCnt).Name) = "cmrpt_sdi_wo"  
                                Next  '  For iCnt = 0 To root.ChildNodes.Count - 1 
                                myLoggr1.WriteInformationLog(rtn & " :: Number of lines in this file: " & root.ChildNodes.Count)
                                m_logger.WriteInformationLog(rtn & " :: Number of lines in this file: " & root.ChildNodes.Count)
                            End If  '  root.ChildNodes.Count > 0
                        End If ' Trim(strXMLError) = ""  '  inner if 
                    End If  ' Trim(strXMLError) = "" 

                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bolError Or bLineError Then
                        If Trim(m_arrXMLErrFiles) = "" Then
                            m_arrXMLErrFiles = aFiles(I).Name & " for: " & sPlantCode
                        Else
                            m_arrXMLErrFiles &= "," & aFiles(I).Name & " for: " & sPlantCode
                        End If
                        If Trim(strXMLError) <> "" Then
                            If Len(strXMLError) > 250 Then
                                strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                            End If
                            If Trim(m_arrErrorsList) = "" Then
                                m_arrErrorsList = strXMLError
                            Else
                                m_arrErrorsList &= "," & strXMLError
                            End If
                        Else
                            If Trim(m_arrErrorsList) = "" Then
                                m_arrErrorsList = "Check Log for the Error Description"
                            Else
                                m_arrErrorsList &= "," & "Check Log for the Error Description"
                            End If
                        End If
                        'move file to BadXML folder
                        File.Copy(aFiles(I).FullName, rootDir & "\BadXML\" & sPlantCode & "\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXML\" & sPlantCode & "\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, rootDir & "\XMLINProcessed\" & sPlantCode & "\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\XMLINProcessed\" & sPlantCode & "\" & aFiles(I).Name)

                    End If
                Next  '  For I = 0 To aFiles.Length - 1
                connectOR.Close()

            End If  '  aFiles.Length > 0
        Catch ex As Exception
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = aFiles(I).Name & " for: " & sPlantCode
            Else
                m_arrXMLErrFiles &= "," & aFiles(I).Name & " for: " & sPlantCode
            End If
            If Trim(strXMLError) <> "" Then
                If Len(strXMLError) > 250 Then
                    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                End If
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = strXMLError
                Else
                    m_arrErrorsList &= "," & strXMLError
                End If
            Else
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = "Check Log for the Error Description"
                Else
                    m_arrErrorsList &= "," & "Check Log for the Error Description"
                End If
            End If
            'move file to BadXML folder
            File.Copy(aFiles(I).FullName, rootDir & "\BadXML\" & sPlantCode & "\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXML\" & sPlantCode & "\" & aFiles(I).Name)

            Return True
        End Try

        If Not bolError Then
            If bLineError Then
                bolError = True
            End If
        End If

        Return bolError

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "NbtyWrkOrdXmlIn.SendEmail"
        Dim email As New System.Web.Mail.MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_From")) <> "") Then
            email.From = CStr(My.Settings(propertyName:="onErrorEmail_From")).Trim
        End If

        'The email address of the recipient. 
        If bIsSendOut Then
            If strDbName = "PROD" Then

                If bIsSendOut Then
                    email.To = "vitaly.rovensky@sdi.com;"
                    If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                        email.To = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
                    End If
                End If

            Else
                email.To = "vitaly.rovensky@sdi.com"
            End If

        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_CC")) <> "") Then
            email.Cc = CStr(My.Settings(propertyName:="onErrorEmail_CC")).Trim
        Else
            email.Cc = ""
        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_BCC")) <> "") Then
            email.Bcc = CStr(My.Settings(propertyName:="onErrorEmail_BCC")).Trim
        End If

        'The Priority attached and displayed for the email
        email.Priority = System.Web.Mail.MailPriority.High
        'myEmail.Priority = Mail.MailPriority.High

        email.BodyFormat = System.Web.Mail.MailFormat.Html

        email.Body = ""
        email.Body &= "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        email.Body &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >NbtyWrkOrdXmlIn Error</span></center>&nbsp;&nbsp;"

        email.Body &= "<table><tr><td>NbtyWrkOrdXmlIn has completed with "
        If strDbName = "PROD" Then
            email.Subject = "NbtyWrkOrdXmlIn "
        Else
            email.Subject = " -- TEST -- NbtyWrkOrdXmlIn "
        End If
        If bolWarning = True Then
            email.Body &= "warnings,"
            email.Subject &= "Warning"
        Else
            email.Body &= "errors;"
            email.Subject &= "Error"
        End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Try

            sInfoErr &= " XML file name(s) are below.</td></tr>"   '  " review log.</td></tr></table>"
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
                    Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
                    If arrErrFiles1.Length > 0 Then
                        For i1 As Integer = 0 To arrErrFiles1.Length - 1
                            sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
                        Next
                    End If
                End If
            End If
            email.Body &= sInfoErr
        Catch ex As Exception

            email.Body &= " review log.</td></tr>"
        End Try

        email.Body &= "</table>"

        email.Body &= "&nbsp;<br>Sincerely,<br>&nbsp;<br>SDI Customer Care<br>&nbsp;<br></p></div><BR>"
        email.Body &= "<br />"


        Dim sApp As String = "" & _
                             System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).Name & _
                             " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                            ""
        Try
            email.Body &= "" & _
                          "<p style=""text-align:right;font-size:10px"">" & _
                          sApp & _
                          "</p>" & _
                          ""
        Catch ex As Exception
        End Try

        email.Body &= "" & _
                    "<HR width='100%' SIZE='1'>" & _
                    "<img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' />"
        email.Body &= "<br><P><CENTER><SPAN style='FONT-SIZE: 12pt'><SPAN style='FONT-SIZE: 12pt'><FONT color=teal size=2>The information in this communication, including any attachments, is the property of SDI, Inc,&nbsp;</SPAN>is intended only for the addressee and may contain confidential, proprietary, and/or privileged material. Any review, retransmission, dissemination or other use of, or taking of any action in reliance upon, this information by persons or entities other than the intended recipient is prohibited. If you received this in error, please immediately contact the sender by replying to this email and delete the material from all computers.</FONT></SPAN></CENTER></P>"
        email.Body &= "</body></html>"

        Dim bSend As Boolean = False
        Try
            If bIsSendOut Then
                SendEmail1(email)
            End If

            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
        Catch ex As Exception

        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, mailer.Cc, mailer.Bcc, "N", mailer.Body, connectOR)

            SendLogger(mailer.Subject, mailer.Body, "NBTYWORKORDIN", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "TechSupport@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(subject, "SDIExchADMIN@sdi.com", EmailTo, "", EmailBcc, "N", body, m_CN)

        Catch ex As Exception

        End Try
    End Sub

End Module
