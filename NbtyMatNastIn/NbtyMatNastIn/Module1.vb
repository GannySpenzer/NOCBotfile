Imports System
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

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\NBTYMatMast"
    Dim logpath As String = "C:\NBTYMatMast\LOGS\NbtyMatMastXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\NBTYMatMast\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG")

    Dim strOverride As String = ""
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG"
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
            logpath = sLogPath & "\NbtyMatMastXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
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

                Call ProceesNbtyMatMastXmlInInfo(sPlantCode)

            Next
        End If


        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Sub ProceesNbtyMatMastXmlInInfo(ByVal sPlantCode As String)

        Dim rtn As String = "NbtyMatMastXmlIn.ProceesNbtyMatMastXmlInInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start NBTY Mat. Mast. XML in for: " & sPlantCode)
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of NBTY Mat. Mast. Inbound for: " & sPlantCode)

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "C:\NBTYMatMast\XMLIN_SRC"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "C:\NBTYMatMast\XMLIN_SRC"
        End Try
        sInputDir = sInputDir & "\" & sPlantCode

        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer = 0

        Try
            If aSrcFiles.Length > 0 Then
                Dim bFound As Boolean = False
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > Len("cmrpt_sdipe") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("cmrpt_sdipe") Then
                            bFound = True
                            File.Copy(aSrcFiles(I).FullName, "C:\NBTYMatMast\XMLIN\" & sPlantCode & "\" & aSrcFiles(I).Name, True)
                            'File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " copied to " & "C:\NBTYMatMast\XMLIN\" & sPlantCode & "\" & aSrcFiles(I).Name)

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
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\NBTYMatMast\XMLIN\" & sPlantCode & "\" & " ...")
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
        '// CytecMatMastIn Processing: inbound to the Oracle table SYSADM8.PS_ISA_NBTY_IMP_MM ...
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = GetNbtyMatMastXmlIn(sPlantCode)
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

    Private Function GetNbtyMatMastXmlIn(ByVal sPlantCode As String) As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "NbtyMatMastXmlIn.GetNbtyMatMastXmlIn." & sPlantCode

        Console.WriteLine("Start Insert of NBTY Mat. Mast. in SYSADM8.PS_ISA_NBTY_IMP_MM for: " & sPlantCode)
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of NBTY Mat. Mast. in SYSADM8.PS_ISA_NBTY_IMP_MM for: " & sPlantCode)

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\NBTYMatMast\XMLIN\" & sPlantCode & "\")
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
                            File.Move(aFiles(I).FullName, "C:\NBTYMatMast\BadXML\" & sPlantCode & "\" & aFiles(I).Name)
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
                        ElseIf root.LastChild.Name.ToLower() = "cmrpt_sdipe" Then
                            strXMLError = ""
                        Else
                            strXMLError = "Missing last XML Element for: " & sPlantCode
                        End If

                        Dim rowsaffected As Integer = 0
                        If Trim(strXMLError) = "" Then
                            If root.ChildNodes.Count > 0 Then
                                'code to delete previous info based on Plant Code - sPlantCode
                                sPlantCode = UCase(sPlantCode)
                                Dim strDeleteString As String = "DELETE SYSADM8.PS_ISA_NBTY_IMP_MM WHERE UPPER(PLANT) = '" & sPlantCode & "'"
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
                                Dim strInvItemId As String = ""
                                Dim strPrimFldName As String = ""
                                Dim strDescr254 As String = ""
                                Dim strStatusId As String = ""
                                Dim strPlant As String = ""
                                Dim strQtyOnHand As String = ""
                                Dim decQtyOnHand As Decimal = 0
                                Dim strUom As String = ""
                                Dim strUnitCost As String = ""
                                Dim decUnitCost As Decimal = 0
                                Dim strIsaCustBin As String = ""
                                Dim strReordPoint As String = ""
                                Dim decReordPoint As Decimal = 0
                                Dim strQtyMax As String = ""
                                Dim decQtyMax As Decimal = 0
                                Dim strDemandQty As String = ""
                                Dim decDemandQty As Decimal = 0
                                Dim strMfgDesc As String = ""
                                Dim strMfgId As String = ""
                                Dim strMfgItemId As String = ""
                                Dim strHazmClassCd As String = ""
                                Dim strClientCritSpare As String = ""
                                Dim strPrefMfg As String = ""
                                Dim strPrimaBranch As String = ""
                                Dim strStkType As String = ""
                                Dim strUtilizCd As String = ""

                                Dim iCnt As Integer = 0
                                For iCnt = 0 To root.ChildNodes.Count - 1
                                    If LCase(root.ChildNodes(iCnt).Name) = "cmrpt_sdipe" Then
                                        Dim nodeMxItem As XmlNode = root.ChildNodes(iCnt)
                                        'bLineError = False
                                        If nodeMxItem.ChildNodes.Count > 0 Then
                                            strInvItemId = " "
                                            strPrimFldName = " "
                                            strDescr254 = " "
                                            strStatusId = " "
                                            strPlant = " "
                                            strQtyOnHand = " "
                                            decQtyOnHand = 0
                                            strUom = " "
                                            strUnitCost = " "
                                            decUnitCost = 0
                                            strIsaCustBin = " "
                                            strReordPoint = " "
                                            decReordPoint = 0
                                            strQtyMax = " "
                                            decQtyMax = 0
                                            strDemandQty = " "
                                            decDemandQty = 0
                                            strMfgDesc = " "
                                            strMfgId = " "
                                            strMfgItemId = " "
                                            strHazmClassCd = " "
                                            strClientCritSpare = " "
                                            strPrefMfg = " "
                                            strPrimaBranch = " "
                                            strStkType = " "
                                            strUtilizCd = " "

                                            Dim iItemMM As Integer = 0
                                            Dim strNodeName As String = ""
                                            For iItemMM = 0 To nodeMxItem.ChildNodes.Count - 1
                                                strNodeName = LCase(nodeMxItem.ChildNodes(iItemMM).Name)
                                                Select Case strNodeName
                                                    Case "cmrpt_sdipe.part_id"  ' strInvItemId  ' 18
                                                        Try
                                                            strInvItemId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strInvItemId) = "" Then
                                                                strInvItemId = " "
                                                            Else
                                                                strInvItemId = Trim(strInvItemId)
                                                                strInvItemId = Replace(Replace(strInvItemId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strInvItemId)) > 18 Then
                                                                strInvItemId = Microsoft.VisualBasic.Left(strInvItemId, 18)
                                                            End If
                                                        Catch ex As Exception
                                                            strInvItemId = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.pkey"  '  strPrimFldName ' 18
                                                        Try
                                                            strPrimFldName = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strPrimFldName) = "" Then
                                                                strPrimFldName = " "
                                                            Else
                                                                strPrimFldName = Trim(strPrimFldName)
                                                                strPrimFldName = Replace(Replace(strPrimFldName, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strPrimFldName)) > 18 Then
                                                                strPrimFldName = Microsoft.VisualBasic.Left(strPrimFldName, 18)
                                                            End If
                                                        Catch ex As Exception
                                                            strPrimFldName = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.part_descr"  ' strDescr254  '  254
                                                        Try
                                                            strDescr254 = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strDescr254) = "" Then
                                                                strDescr254 = " "
                                                            Else
                                                                strDescr254 = Trim(strDescr254)
                                                                strDescr254 = Replace(Replace(strDescr254, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strDescr254)) > 254 Then
                                                                strDescr254 = Microsoft.VisualBasic.Left(strDescr254, 254)
                                                            End If
                                                        Catch ex As Exception
                                                            strDescr254 = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.status"  '  strStatusId  '  30
                                                        Try
                                                            strStatusId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strStatusId) = "" Then
                                                                strStatusId = " "
                                                            Else
                                                                strStatusId = Trim(strStatusId)
                                                                strStatusId = Replace(Replace(strStatusId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strStatusId)) > 30 Then
                                                                strStatusId = Microsoft.VisualBasic.Left(strStatusId, 30)
                                                            End If
                                                        Catch ex As Exception
                                                            strStatusId = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.plant"  '  strPlant  '  30
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
                                                    Case "cmrpt_sdipe.qty_onhand" ' strQtyOnHand ' 15.4
                                                        Try
                                                            strQtyOnHand = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strQtyOnHand) = "" Then
                                                                decQtyOnHand = 0
                                                            Else
                                                                strQtyOnHand = Trim(strQtyOnHand)
                                                                If IsNumeric(strQtyOnHand) Then
                                                                    decQtyOnHand = CType(strQtyOnHand, Decimal)
                                                                    decQtyOnHand = Math.Round(decQtyOnHand, 4)
                                                                Else
                                                                    decQtyOnHand = 0
                                                                End If
                                                            End If
                                                            
                                                        Catch ex As Exception
                                                            decQtyOnHand = 0
                                                        End Try
                                                    Case "cmrpt_sdipe.iss_uom"  '  strUom  ' 3
                                                        Try
                                                            strUom = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strUom) = "" Then
                                                                strUom = " "
                                                            Else
                                                                strUom = Trim(strUom)
                                                            End If
                                                            If Len(Trim(strUom)) > 3 Then
                                                                strUom = Microsoft.VisualBasic.Left(strUom, 3)
                                                            End If
                                                        Catch ex As Exception
                                                            strUom = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.unit_cost"  '  strUnitCost ' 14.4
                                                        Try
                                                            strUnitCost = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strUnitCost) = "" Then
                                                                decUnitCost = 0
                                                            Else
                                                                strUnitCost = Trim(strUnitCost)
                                                                If IsNumeric(strUnitCost) Then
                                                                    decUnitCost = CType(strUnitCost, Decimal)
                                                                    decUnitCost = Math.Round(decUnitCost, 4)
                                                                Else
                                                                    decUnitCost = 0
                                                                End If
                                                            End If

                                                        Catch ex As Exception
                                                            decUnitCost = 0
                                                        End Try
                                                    Case "cmrpt_sdipe.location"  '  StrIsaCustBin ' 15
                                                        Try
                                                            strIsaCustBin = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strIsaCustBin) = "" Then
                                                                strIsaCustBin = " "
                                                            Else
                                                                strIsaCustBin = Trim(strIsaCustBin)
                                                                strIsaCustBin = Replace(Replace(strIsaCustBin, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strIsaCustBin)) > 15 Then
                                                                strIsaCustBin = Microsoft.VisualBasic.Left(strIsaCustBin, 15)
                                                            End If
                                                        Catch ex As Exception
                                                            strIsaCustBin = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.reorder"  ' strReordPoint ' 15.4
                                                        Try
                                                            strReordPoint = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strReordPoint) = "" Then
                                                                decReordPoint = 0
                                                            Else
                                                                strReordPoint = Trim(strReordPoint)
                                                                If IsNumeric(strReordPoint) Then
                                                                    decReordPoint = CType(strReordPoint, Decimal)
                                                                    decReordPoint = Math.Round(decReordPoint, 4)
                                                                Else
                                                                    decReordPoint = 0
                                                                End If
                                                            End If

                                                        Catch ex As Exception
                                                            decReordPoint = 0
                                                        End Try
                                                    Case "cmrpt_sdipe.qty_maximum"  ' strQtyMax ' 15.4
                                                        Try
                                                            strQtyMax = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strQtyMax) = "" Then
                                                                decQtyMax = 0
                                                            Else
                                                                strQtyMax = Trim(strQtyMax)
                                                                If IsNumeric(strQtyMax) Then
                                                                    decQtyMax = CType(strQtyMax, Decimal)
                                                                    decQtyMax = Math.Round(decQtyMax, 4)
                                                                Else
                                                                    decQtyMax = 0
                                                                End If
                                                            End If

                                                        Catch ex As Exception
                                                            decQtyMax = 0
                                                        End Try
                                                    Case "cmrpt_sdipe.qty_request"  ' strDemandQty ' 15.4
                                                        Try
                                                            strDemandQty = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strDemandQty) = "" Then
                                                                decDemandQty = 0
                                                            Else
                                                                strDemandQty = Trim(strDemandQty)
                                                                If IsNumeric(strDemandQty) Then
                                                                    decDemandQty = CType(strDemandQty, Decimal)
                                                                    decDemandQty = Math.Round(decDemandQty, 4)
                                                                Else
                                                                    decDemandQty = 0
                                                                End If
                                                            End If

                                                        Catch ex As Exception
                                                            decDemandQty = 0
                                                        End Try
                                                    Case "cmrpt_sdipe.mfg_desc"  '  strMfgDesc  ' 150
                                                        Try
                                                            strMfgDesc = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strMfgDesc) = "" Then
                                                                strMfgDesc = " "
                                                            Else
                                                                strMfgDesc = Trim(strMfgDesc)
                                                                strMfgDesc = Replace(Replace(strMfgDesc, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strMfgDesc)) > 150 Then
                                                                strMfgDesc = Microsoft.VisualBasic.Left(strMfgDesc, 150)
                                                            End If
                                                        Catch ex As Exception
                                                            strMfgDesc = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.mfg_id"  ' strMfgId ' 50
                                                        Try
                                                            strMfgId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strMfgId) = "" Then
                                                                strMfgId = " "
                                                            Else
                                                                strMfgId = Trim(strMfgId)
                                                                strMfgId = Replace(Replace(strMfgId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strMfgId)) > 50 Then
                                                                strMfgId = Microsoft.VisualBasic.Left(strMfgId, 50)
                                                            End If
                                                        Catch ex As Exception
                                                            strMfgId = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.mfg_itm_id"  ' strMfgItemId ' 50
                                                        Try
                                                            strMfgItemId = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strMfgItemId) = "" Then
                                                                strMfgItemId = " "
                                                            Else
                                                                strMfgItemId = Trim(strMfgItemId)
                                                                strMfgItemId = Replace(Replace(strMfgItemId, "&", "&amp;"), "'", "&apos;")
                                                            End If
                                                            If Len(Trim(strMfgItemId)) > 50 Then
                                                                strMfgItemId = Microsoft.VisualBasic.Left(strMfgItemId, 50)
                                                            End If
                                                        Catch ex As Exception
                                                            strMfgItemId = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.hazmat"  ' strHazmClassCd ' 4
                                                        Try
                                                            strHazmClassCd = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strHazmClassCd) = "" Then
                                                                strHazmClassCd = " "
                                                            Else
                                                                strHazmClassCd = Trim(strHazmClassCd)
                                                            End If
                                                            If Len(Trim(strHazmClassCd)) > 4 Then
                                                                strHazmClassCd = Microsoft.VisualBasic.Left(strHazmClassCd, 4)
                                                            End If
                                                        Catch ex As Exception
                                                            strHazmClassCd = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.critical_spare"  ' strClientCritSpare ' 3
                                                        Try
                                                            strClientCritSpare = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strClientCritSpare) = "" Then
                                                                strClientCritSpare = " "
                                                            Else
                                                                strClientCritSpare = Trim(strClientCritSpare)
                                                            End If
                                                            If Len(Trim(strClientCritSpare)) > 3 Then
                                                                strClientCritSpare = Microsoft.VisualBasic.Left(strClientCritSpare, 3)
                                                            End If
                                                        Catch ex As Exception
                                                            strClientCritSpare = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.primary_mfg"  ' strPrefMfg  ' 1
                                                        Try
                                                            strPrefMfg = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strPrefMfg) = "" Then
                                                                strPrefMfg = " "
                                                            Else
                                                                strPrefMfg = Trim(strPrefMfg)
                                                            End If
                                                            If Len(Trim(strPrefMfg)) > 1 Then
                                                                strPrefMfg = Microsoft.VisualBasic.Left(strPrefMfg, 1)
                                                            End If
                                                        Catch ex As Exception
                                                            strPrefMfg = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.primary_location"  ' strPrimaBranch ' 1 
                                                        Try
                                                            strPrimaBranch = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strPrimaBranch) = "" Then
                                                                strPrimaBranch = " "
                                                            Else
                                                                strPrimaBranch = Trim(strPrimaBranch)
                                                            End If
                                                            If Len(Trim(strPrimaBranch)) > 1 Then
                                                                strPrimaBranch = Microsoft.VisualBasic.Left(strPrimaBranch, 1)
                                                            End If
                                                        Catch ex As Exception
                                                            strPrimaBranch = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.stock"  ' strStkType  ' 1
                                                        Try
                                                            strStkType = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strStkType) = "" Then
                                                                strStkType = " "
                                                            Else
                                                                strStkType = Trim(strStkType)
                                                            End If
                                                            If Len(Trim(strStkType)) > 1 Then
                                                                strStkType = Microsoft.VisualBasic.Left(strStkType, 1)
                                                            End If
                                                        Catch ex As Exception
                                                            strStkType = " "
                                                        End Try
                                                    Case "cmrpt_sdipe.abc"  '  strUtilizCd  '  4
                                                        Try
                                                            strUtilizCd = nodeMxItem.ChildNodes(iItemMM).InnerText
                                                            If Trim(strUtilizCd) = "" Then
                                                                strUtilizCd = " "
                                                            Else
                                                                strUtilizCd = Trim(strUtilizCd)
                                                            End If
                                                            If Len(Trim(strUtilizCd)) > 1 Then
                                                                strUtilizCd = Microsoft.VisualBasic.Left(strUtilizCd, 1)
                                                            End If
                                                        Catch ex As Exception
                                                            strUtilizCd = " "
                                                        End Try
                                                    Case Else
                                                        'do nothing
                                                End Select
                                            Next  '  For iItemMM = 0 To nodeMxItem.ChildNodes.Count - 1

                                            'collected all info - starting insert
                                            rowsaffected = 0
                                            Dim strSQLstring As String = ""
                                            strSQLstring = "INSERT INTO SYSADM8.PS_ISA_NBTY_IMP_MM (INV_ITEM_ID,PRIMARY_FIELDNAME,DESCR254,STATUS_ID" & vbCrLf & _
                                                ",PLANT,QTY_ONHAND,UNIT_OF_MEASURE,UNIT_COST" & vbCrLf & _
                                                ",ISA_CUST_BIN,REORDER_POINT,QTY_MAXIMUM,DEMAND_QTY" & vbCrLf & _
                                                ",MFG_DESCR,MFG_ID,MFG_ITM_ID,HAZ_CLASS_CD" & vbCrLf & _
                                                ",CLIENT_CRIT_SPARE,PREFERRED_MFG,PRIMARY_BRANCH,STOCK_TYPE,UTILIZ_CD)" & vbCrLf & _
                                                " VALUES ('" & strInvItemId & "','" & strPrimFldName & "','" & strDescr254 & "','" & strStatusId & "'" & vbCrLf & _
                                                ",'" & strPlant & "'," & decQtyOnHand & ",'" & strUom & "'," & decUnitCost & "" & vbCrLf & _
                                                ",'" & strIsaCustBin & "'," & decReordPoint & "," & decQtyMax & "," & decDemandQty & "" & vbCrLf & _
                                                ",'" & strMfgDesc & "','" & strMfgId & "','" & strMfgItemId & "','" & strHazmClassCd & "'" & vbCrLf & _
                                                ",'" & strClientCritSpare & "','" & strPrefMfg & "','" & strPrimaBranch & "','" & strStkType & "','" & strUtilizCd & "')" & vbCrLf & _
                                                ""


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
                                            'end insert line
                                        End If  '  nodeMxItem.ChildNodes.Count > 0
                                    End If  '  LCase(root.ChildNodes(iCnt).Name) = "cmrpt_sdipe"
                                Next  '  For iCnt = 0 To root.ChildNodes.Count - 1
                                myLoggr1.WriteInformationLog(rtn & " :: Number of lines in this file: " & root.ChildNodes.Count)
                                m_logger.WriteInformationLog(rtn & " :: Number of lines in this file: " & root.ChildNodes.Count)
                            End If  ' root.ChildNodes.Count > 0
                        End If '  Trim(strXMLError) = "" ' inner if
                    End If  '  Trim(strXMLError) = ""

                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bolError Or bLineError Then
                        If Trim(m_arrXMLErrFiles) = "" Then
                            m_arrXMLErrFiles = aFiles(I).Name
                        Else
                            m_arrXMLErrFiles &= "," & aFiles(I).Name
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
                        File.Copy(aFiles(I).FullName, "C:\NBTYMatMast\BadXML\" & sPlantCode & "\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\NBTYMatMast\BadXML\" & sPlantCode & "\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\NBTYMatMast\XMLINProcessed\" & sPlantCode & "\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\NBTYMatMast\XMLINProcessed\" & sPlantCode & "\" & aFiles(I).Name)

                    End If

                Next  '  For I = 0 To aFiles.Length - 1
                connectOR.Close()

            End If  '  aFiles.Length > 0

        Catch ex As Exception
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = aFiles(I).Name
            Else
                m_arrXMLErrFiles &= "," & aFiles(I).Name
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
            File.Copy(aFiles(I).FullName, "C:\NBTYMatMast\BadXML\" & sPlantCode & "\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\NBTYMatMast\BadXML\" & sPlantCode & "\" & aFiles(I).Name)

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

        Dim rtn As String = "NbtyMatMastXmlIn.SendEmail"
        Dim email As New System.Web.Mail.MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_From")) <> "") Then
            email.From = CStr(My.Settings(propertyName:="onErrorEmail_From")).Trim
        End If

        'The email address of the recipient. 
        email.To = "vitaly.rovensky@sdi.com"
        If bIsSendOut Then
            If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                email.To = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
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
        email.Body &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >NbtyMatMastXmlIn Error</span></center>&nbsp;&nbsp;"

        email.Body &= "<table><tr><td>NbtyMatMastXmlIn has completed with "
        If bolWarning = True Then
            email.Body &= "warnings,"
            email.Subject = " (Test) NbtyMatMastXmlIn Warning"
        Else
            email.Body &= "errors;"
            email.Subject = " (Test) NbtyMatMastXmlIn Error"
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

        Try
            email.Attachments.Add(New System.Web.Mail.MailAttachment(filename:=sErrLogPath))
        Catch ex As Exception
        End Try

        Dim int1 As Integer = 0

        Try
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles() As String = Split(m_arrXMLErrFiles, ",")
                    If arrErrFiles.Length > 0 Then
                        m_logger.WriteInformationLog(rtn & " :: erroneous xml file count = " & arrErrFiles.Length.ToString)
                        For int1 = 0 To arrErrFiles.Length - 1
                            Dim myFileName2 As String = "C:\NBTYMatMast\BadXML\" & arrErrFiles(int1)
                            email.Attachments.Add(New System.Web.Mail.MailAttachment(myFileName2))
                        Next
                    End If
                End If
            End If
        Catch ex As Exception

        End Try

        Dim bSend As Boolean = False
        Try

            SendEmail1(email)
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

            SendLogger(mailer.Subject, mailer.Body, "NBTYMATMASTIN", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(subject, "SDIExchADMIN@sdi.com", EmailTo, "", EmailBcc, "N", body, m_CN)

        Catch ex As Exception

        End Try
    End Sub

End Module
