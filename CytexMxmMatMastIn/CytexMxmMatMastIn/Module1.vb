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

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdMatMastCytecXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
    Dim strOverride As String
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL"
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
            logpath = sLogPath & "\UpdMatMastCytecXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        'Dim cnStringSQL As String = "server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'"
        'Try
        '    cnStringSQL = My.Settings("sqlCNString1").ToString.Trim
        'Catch ex As Exception
        '    cnStringSQL = ""
        'End Try
        'If (cnStringSQL.Length > 0) Then
        '    ' drop current connection and re-create
        '    Try
        '        connectSQL.Dispose()
        '    Catch ex As Exception
        '    End Try
        '    connectSQL = Nothing
        '    connectSQL = New SqlClient.SqlConnection(cnStringSQL)
        'End If

        ' initialize logs

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        'process received info
        Call ProceesCytecMxmMatMastInfo()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Sub ProceesCytecMxmMatMastInfo()

        Dim rtn As String = "CytecMxmMatMast.ProceesCytecMxmMatMastInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start CytecMxm Mat. Mast. XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of CytecMxm Mat. Mast. Inbound")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")
        'm_logger.WriteVerboseLog(rtn & " :: (SQL Server)connection string : [" & connectSQL.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "\\ims\SDIWebProcessorsXMLFiles"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "\\ims\SDIWebProcessorsXMLFiles"
        End Try
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer

        Try
            If aSrcFiles.Length > 0 Then
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > Len("CYTECMXM_MM_XML") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("CYTECMXM_MM_XML") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\CytecMxmIn\XMLIN\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLIN\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\CytecMxmIn\XMLIN\ " & "...")
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
        '// CytecMatMastIn Processing: inbound to the Oracle table SYSADM8.PS_ISA_MXM_ITEM_IN ...
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = GetCytecMatMastIn()
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

        m_logger.WriteInformationLog(rtn & " :: End of CytecMxm Mat. Mast. IN Process")

    End Sub

    Private Function ProcessItemInterface(ByVal nodeMatMastReq As XmlNode, ByVal strFileName As String, ByRef bolError As Boolean) As String
        Dim strXMLError As String = ""
        'Dim bolError As Boolean = False
        Dim rtn As String = "CytecMxmMatMast.ProcessItemInterface"

        m_logger.WriteInformationLog(rtn & " :: Start ProcessItemInterface")

        ' get existing node CONTENT
        Dim strCustId As String = "CYTEC"
        Dim strInvItemId As String = " "
        ' timestamp
        Dim strEffStatus As String = "A"
        Dim strPrefrdMsg As String = "Y"
        Dim strUtilizCd As String = " "
        Dim strClientCritSpare As String = " "
        Dim strIsaItemNotes As String = " "
        Dim strInvItemType As String = " "
        Dim strTaxFlag As String = " "
        Dim strProcessFlag As String = "N"
        Dim lngProcessInstance As Long = 0
        'Dim - Date_Processed - timestamp - NULL

        Dim nodeSITEID As XmlNode = Nothing
        Dim nodeItemNum As XmlNode = Nothing
        Dim nodeCommodGrp As XmlNode = Nothing
        Dim nodeDescr As XmlNode = Nothing
        Dim nodeInvStockType As XmlNode = Nothing
        Dim nodeUOM As XmlNode = Nothing
        Dim nodeUom_po As XmlNode = Nothing
        Dim nodeIsaCustMfg As XmlNode = Nothing
        Dim nodeMfgItmId As XmlNode = Nothing
        Dim nodeIsaBinId As XmlNode = Nothing
        Dim nodeReorderPoint As XmlNode = Nothing
        Dim nodeQtyMax As XmlNode = Nothing
        Dim nodeReorderQty As XmlNode = Nothing
        Dim nodeStdLead As XmlNode = Nothing
        Dim nodeStorArea As XmlNode = Nothing
        Dim nodeStdCost As XmlNode = Nothing
        Dim nodeComments2000 As XmlNode = Nothing
        Dim nodeInvItemType As XmlNode = Nothing

        If nodeMatMastReq.ChildNodes.Count > 0 Then
            Dim strSiteId As String = ""
            Dim strItemNum As String = ""
            Dim strCommodGrp As String = ""
            Dim strDescr1 As String = ""
            Dim strInvStockType As String = ""
            Dim strUOM As String = ""
            Dim strUom_Po As String = ""
            Dim strIsaCustMfg As String = ""
            Dim strMfgItmId As String = ""
            Dim strIsaBinId As String = ""
            Dim strReorderPoint As String = ""
            Dim decReorderPoint As Decimal
            Dim strQtyMax As String = ""
            Dim decQtyMax As Decimal
            Dim strReorderQty As String = ""
            Dim decReorderQty As Decimal
            Dim strStdLead As String = ""
            Dim intStdLead As Integer
            Dim strStorArea As String = ""
            Dim strStdCost As String = ""
            Dim decStdCost As Decimal
            Dim strComments2000 As String = ""

            Dim iCnt As Integer = 0
            For iCnt = 0 To nodeMatMastReq.ChildNodes.Count - 1
                If UCase(nodeMatMastReq.ChildNodes(iCnt).Name) = "MXITEM" Then  '   "MMITEMIN" Then
                    Dim nodeMxItem As XmlNode = nodeMatMastReq.ChildNodes(iCnt)
                    If nodeMxItem.ChildNodes.Count > 0 Then
                        Dim iMxItem As Integer = 0
                        For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1
                            If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "ITEM" Then
                                Dim nodeItemMM As XmlNode = nodeMxItem.ChildNodes(iMxItem)
                                If nodeItemMM.ChildNodes.Count > 0 Then
                                    nodeSITEID = Nothing
                                    nodeItemNum = Nothing
                                    nodeCommodGrp = Nothing
                                    nodeDescr = Nothing
                                    nodeInvStockType = Nothing
                                    nodeUOM = Nothing
                                    nodeUom_po = Nothing
                                    nodeIsaCustMfg = Nothing
                                    nodeMfgItmId = Nothing
                                    nodeIsaBinId = Nothing
                                    nodeReorderPoint = Nothing
                                    nodeQtyMax = Nothing
                                    nodeReorderQty = Nothing
                                    nodeStdLead = Nothing
                                    nodeStorArea = Nothing
                                    nodeStdCost = Nothing
                                    nodeComments2000 = Nothing
                                    nodeInvItemType = Nothing

                                    strSiteId = ""
                                    strItemNum = ""
                                    strCommodGrp = ""
                                    strDescr1 = ""
                                    strInvStockType = ""
                                    strUOM = ""
                                    strUom_Po = ""
                                    strIsaCustMfg = ""
                                    strMfgItmId = ""
                                    strIsaBinId = ""
                                    decReorderPoint = 0
                                    strReorderPoint = ""
                                    decQtyMax = 0
                                    strQtyMax = ""
                                    decReorderQty = 0
                                    strReorderQty = ""
                                    intStdLead = 0
                                    strStdLead = ""
                                    strStorArea = ""
                                    decStdCost = 0
                                    strStdCost = ""
                                    strComments2000 = ""
                                    strInvItemType = ""

                                    Dim iItemMM As Integer = 0
                                    Dim strNodeName As String = ""
                                    For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                        strNodeName = UCase(nodeItemMM.ChildNodes(iItemMM).Name)
                                        Select Case strNodeName
                                            Case "STOCKTYPE"
                                                Try
                                                    strInvItemType = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strInvItemType) = "" Then
                                                        strInvItemType = " "
                                                    Else
                                                        strInvItemType = Trim(strInvItemType)
                                                    End If
                                                    If Len(Trim(strInvItemType)) > 5 Then
                                                        strInvItemType = Microsoft.VisualBasic.Left(strInvItemType, 5)
                                                    End If
                                                Catch ex As Exception
                                                    strInvItemType = " "
                                                End Try
                                            Case "DESCRIPTION_LONGDESCRIPTION"
                                                'strComments2000
                                                Try
                                                    strComments2000 = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strComments2000) = "" Then
                                                        strComments2000 = " "
                                                    Else
                                                        strComments2000 = Trim(strComments2000)
                                                    End If
                                                    If Len(Trim(strComments2000)) > 2000 Then
                                                        strComments2000 = Microsoft.VisualBasic.Left(strComments2000, 2000)
                                                    End If
                                                Catch ex As Exception
                                                    strComments2000 = " "
                                                End Try

                                            Case "ITEMNUM"
                                                'strItemNum
                                                Try
                                                    strItemNum = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strItemNum) = "" Then
                                                        strItemNum = " "
                                                    Else
                                                        strItemNum = Trim(strItemNum)
                                                    End If
                                                    If Len(Trim(strItemNum)) > 18 Then
                                                        strItemNum = Microsoft.VisualBasic.Left(strItemNum, 18)
                                                    End If
                                                Catch ex As Exception
                                                    strItemNum = " "
                                                End Try

                                            Case "ORDERUNIT"
                                                Try
                                                    strUom_Po = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strUom_Po) = "" Then
                                                        strUom_Po = " "
                                                    Else
                                                        strUom_Po = Trim(strUom_Po)
                                                    End If
                                                    If Len(Trim(strUom_Po)) > 3 Then
                                                        strUom_Po = Microsoft.VisualBasic.Left(strUom_Po, 3)
                                                    End If
                                                Catch ex As Exception
                                                    strUom_Po = " "
                                                End Try

                                            Case "ISSUEUNIT"  '  DESCRIPTION
                                                Try
                                                    strUOM = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strUOM) = "" Then
                                                        strUOM = " "
                                                    Else
                                                        strUOM = Trim(strUOM)
                                                    End If
                                                    If Len(Trim(strUOM)) > 3 Then
                                                        strUOM = Microsoft.VisualBasic.Left(strUOM, 3)
                                                    End If
                                                Catch ex As Exception
                                                    strUOM = " "
                                                End Try

                                            Case "DESCRIPTION"
                                                Try
                                                    strDescr1 = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strDescr1) = "" Then
                                                        strDescr1 = " "
                                                    Else
                                                        strDescr1 = Trim(strDescr1)
                                                    End If
                                                    If Len(Trim(strDescr1)) > 254 Then
                                                        strDescr1 = Microsoft.VisualBasic.Left(strDescr1, 254)
                                                    End If
                                                Catch ex As Exception
                                                    strDescr1 = " "
                                                End Try

                                            Case "COMMODITYGROUP"
                                                Try
                                                    strCommodGrp = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strCommodGrp) = "" Then
                                                        strCommodGrp = " "
                                                    Else
                                                        strCommodGrp = Trim(strCommodGrp)
                                                    End If
                                                    If Len(Trim(strCommodGrp)) > 5 Then
                                                        strCommodGrp = Microsoft.VisualBasic.Left(strCommodGrp, 5)
                                                    End If
                                                Catch ex As Exception
                                                    strCommodGrp = " "
                                                End Try

                                            Case Else
                                                'do nothing
                                        End Select
                                       

                                    Next  ' every node in <ITEM>  ' For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                    ' defaults - values not in ITEM interface
                                    strSiteId = "PC"
                                    decStdCost = 0
                                    strStorArea = " "
                                    intStdLead = 0
                                    decReorderQty = 0
                                    decQtyMax = 0
                                    decReorderPoint = 0
                                    strIsaBinId = " "
                                    strMfgItmId = " "
                                    strIsaCustMfg = " "
                                    strInvStockType = " "

                                    'collected all info - starting insert
                                    Dim rowsaffected As Integer = 0
                                    Dim strSQLstring As String = ""
                                    If Trim(strItemNum) <> "" And Trim(strSiteId) <> "" Then
                                        strSQLstring = "INSERT INTO SYSADM8.PS_ISA_MXM_ITEM_IN (CUST_ID,PLANT,ISA_ITEM,INV_ITEM_ID,DT_TIMESTAMP,CATEGORY_ID,IM_CFFT,EFF_STATUS" & vbCrLf & _
                                            ",INV_STOCK_TYPE,ISA_CUSTOMER_MFG,MFG_ITM_ID,UNIT_OF_MEASURE,UOM_PO,PREFERRED_MFG,ISA_BIN_ID,REORDER_POINT" & vbCrLf & _
                                            ",QTY_MAXIMUM,REORDER_QTY,STANDARD_COST,STD_LEAD,UTILIZ_CD,CLIENT_CRIT_SPARE,ISA_ITEM_NOTES,INV_ITEM_TYPE" & vbCrLf & _
                                            ",TAX_FLAG,STORAGE_AREA,PROCESS_FLAG,PROCESS_INSTANCE,DATE_PROCESSED,COMMENTS_2000,INTFC_REC_TYPE) " & vbCrLf & _
                                            "VALUES('" & strCustId & "','" & strSiteId & "','" & strItemNum & "','" & strInvItemId & "',CURRENT_TIMESTAMP,'" & strCommodGrp & "','" & strDescr1 & "','A'" & vbCrLf & _
                                            ",'" & strInvStockType & "','" & strIsaCustMfg & "','" & strMfgItmId & "','" & strUOM & "','" & strUom_Po & "','Y','" & strIsaBinId & "'," & decReorderPoint & "" & vbCrLf & _
                                            "," & decQtyMax & "," & decReorderQty & "," & decStdCost & "," & intStdLead & ",' ',' ',' ','" & strInvItemType & "'" & vbCrLf & _
                                            ",' ','" & strStorArea & "','N',0,NULL,'" & strComments2000 & "','ITEM')"

                                        Try
                                            rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                                            If rowsaffected = 0 Then
                                                myLoggr1.WriteErrorLog(rtn & " :: Error while inserting: 'rowsaffected = 0' for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                                myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                bolError = True
                                            End If
                                        Catch ex As Exception
                                            myLoggr1.WriteErrorLog(rtn & " :: Error inserting: " & ex.Message & " for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                            myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                            bolError = True
                                        End Try

                                    Else
                                        'empty line
                                        myLoggr1.WriteErrorLog(rtn & " :: Error: fields PLANT,ISA_ITEM are empty for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                    End If
                                End If
                            End If
                        Next '  nodeMxItem.ChildNodes(iMxItem) - each <ITEM>
                    End If  '  nodeMxItem.ChildNodes.Count > 0   -  <MXITEM>

                End If   '  UCase(nodeMatMastReq.ChildNodes(iCnt).Name) = "MXITEM"
            Next  '  ChildNodes(iCnt) 
        End If  '  nodeMatMastReq.ChildNodes.Count > 0

        Return strXMLError

    End Function

    Private Function ProcessInventoryInterface(ByVal nodeMatMastReq As XmlNode, ByVal strFileName As String, ByRef bolError As Boolean) As String
        Dim strXMLError As String = ""
        'Dim bolError As Boolean = False
        Dim rtn As String = "CytecMxmMatMast.ProcessInventoryInterface"

        m_logger.WriteInformationLog(rtn & " :: Start ProcessInventoryInterface")

        ' get existing node CONTENT
        Dim strCustId As String = "CYTEC"
        Dim strInvItemId As String = " "
        ' timestamp
        Dim strEffStatus As String = "A"
        Dim strPrefrdMsg As String = "Y"
        Dim strUtilizCd As String = " "
        Dim strClientCritSpare As String = " "
        Dim strIsaItemNotes As String = " "
        Dim strInvItemType As String = " "
        Dim strTaxFlag As String = " "
        Dim strProcessFlag As String = "N"
        Dim lngProcessInstance As Long = 0
        'Dim - Date_Processed - timestamp - NULL

        Dim nodeSITEID As XmlNode = Nothing
        Dim nodeItemNum As XmlNode = Nothing
        Dim nodeCommodGrp As XmlNode = Nothing
        Dim nodeDescr As XmlNode = Nothing
        Dim nodeInvStockType As XmlNode = Nothing
        Dim nodeUOM As XmlNode = Nothing
        Dim nodeUom_po As XmlNode = Nothing
        Dim nodeIsaCustMfg As XmlNode = Nothing
        Dim nodeMfgItmId As XmlNode = Nothing
        Dim nodeIsaBinId As XmlNode = Nothing
        Dim nodeReorderPoint As XmlNode = Nothing
        Dim nodeQtyMax As XmlNode = Nothing
        Dim nodeReorderQty As XmlNode = Nothing
        Dim nodeStdLead As XmlNode = Nothing
        Dim nodeStorArea As XmlNode = Nothing
        Dim nodeStdCost As XmlNode = Nothing
        Dim nodeComments2000 As XmlNode = Nothing
        Dim nodeInvItemType As XmlNode = Nothing

        If nodeMatMastReq.ChildNodes.Count > 0 Then
            Dim strSiteId As String = ""
            Dim strItemNum As String = ""
            Dim strCommodGrp As String = ""
            Dim strDescr1 As String = ""
            Dim strInvStockType As String = ""
            Dim strUOM As String = ""
            Dim strUom_Po As String = ""
            Dim strIsaCustMfg As String = ""
            Dim strMfgItmId As String = ""
            Dim strIsaBinId As String = ""
            Dim strReorderPoint As String = ""
            Dim decReorderPoint As Decimal
            Dim strQtyMax As String = ""
            Dim decQtyMax As Decimal
            Dim strReorderQty As String = ""
            Dim decReorderQty As Decimal
            Dim strStdLead As String = ""
            Dim intStdLead As Integer
            Dim strStorArea As String = ""
            Dim strStdCost As String = ""
            Dim decStdCost As Decimal
            Dim strComments2000 As String = ""

            Dim iCnt As Integer = 0
            For iCnt = 0 To nodeMatMastReq.ChildNodes.Count - 1
                If UCase(nodeMatMastReq.ChildNodes(iCnt).Name) = "MXINVENTORY" Then
                    Dim nodeMxItem As XmlNode = nodeMatMastReq.ChildNodes(iCnt)
                    If nodeMxItem.ChildNodes.Count > 0 Then
                        Dim iMxItem As Integer = 0
                        For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1
                            If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "INVENTORY" Then
                                Dim nodeItemMM As XmlNode = nodeMxItem.ChildNodes(iMxItem)
                                If nodeItemMM.ChildNodes.Count > 0 Then
                                    nodeSITEID = Nothing
                                    nodeItemNum = Nothing
                                    nodeCommodGrp = Nothing
                                    nodeDescr = Nothing
                                    nodeInvStockType = Nothing
                                    nodeUOM = Nothing
                                    nodeUom_po = Nothing
                                    nodeIsaCustMfg = Nothing
                                    nodeMfgItmId = Nothing
                                    nodeIsaBinId = Nothing
                                    nodeReorderPoint = Nothing
                                    nodeQtyMax = Nothing
                                    nodeReorderQty = Nothing
                                    nodeStdLead = Nothing
                                    nodeStorArea = Nothing
                                    nodeStdCost = Nothing
                                    nodeComments2000 = Nothing
                                    nodeInvItemType = Nothing

                                    strSiteId = ""
                                    strItemNum = ""
                                    strCommodGrp = ""
                                    strDescr1 = ""
                                    strInvStockType = ""
                                    strUOM = ""
                                    strUom_Po = ""
                                    strIsaCustMfg = ""
                                    strMfgItmId = ""
                                    strIsaBinId = ""
                                    decReorderPoint = 0
                                    strReorderPoint = ""
                                    decQtyMax = 0
                                    strQtyMax = ""
                                    decReorderQty = 0
                                    strReorderQty = ""
                                    intStdLead = 0
                                    strStdLead = ""
                                    strStorArea = ""
                                    decStdCost = 0
                                    strStdCost = ""
                                    strComments2000 = ""
                                    strInvItemType = ""

                                    Dim iItemMM As Integer = 0
                                    Dim strNodeName As String = ""
                                    For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                        strNodeName = UCase(nodeItemMM.ChildNodes(iItemMM).Name)
                                        Select Case strNodeName
                                            Case "INVCOST"
                                                Dim iInvCost As Integer = 0
                                                Dim nodeInvcost As XmlNode = nodeItemMM.ChildNodes(iItemMM)
                                                Dim strNodeInvCostName As String = ""
                                                For iInvCost = 0 To nodeInvcost.ChildNodes.Count - 1

                                                    strNodeInvCostName = UCase(nodeInvcost.ChildNodes(iInvCost).Name)
                                                    Select Case strNodeInvCostName
                                                        Case "STDCOST"
                                                            Try
                                                                strStdCost = nodeInvcost.ChildNodes(iInvCost).InnerText
                                                                If Trim(strStdCost) = "" Then
                                                                    decStdCost = 0
                                                                Else
                                                                    If IsNumeric(strStdCost) Then
                                                                        decStdCost = CType(strStdCost, Decimal)
                                                                        decStdCost = Math.Round(decStdCost, 2)
                                                                    Else
                                                                        decStdCost = 0
                                                                    End If
                                                                End If
                                                            Catch ex As Exception
                                                                decStdCost = 0
                                                            End Try
                                                        Case Else
                                                            'do nothing
                                                    End Select
                                                Next

                                            Case "LOCATION"
                                                Try
                                                    strStorArea = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strStorArea) = "" Then
                                                        strStorArea = " "
                                                    Else
                                                        strStorArea = Trim(strStorArea)
                                                    End If
                                                    If Len(Trim(strStorArea)) > 5 Then
                                                        strStorArea = Microsoft.VisualBasic.Left(strStorArea, 5)
                                                    End If
                                                Catch ex As Exception
                                                    strStorArea = " "
                                                End Try
                                            Case "ORDERQTY" ' MAXLEVEL
                                                Try
                                                    strReorderQty = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strReorderQty) = "" Then
                                                        decReorderQty = 0
                                                    Else
                                                        If IsNumeric(strReorderQty) Then
                                                            decReorderQty = CType(strReorderQty, Decimal)
                                                            decReorderQty = Math.Round(decReorderQty, 4)
                                                        Else
                                                            decReorderQty = 0
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    decReorderQty = 0
                                                End Try
                                            Case "DELIVERYTIME"
                                                Try
                                                    strStdLead = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strStdLead) = "" Then
                                                        intStdLead = 0
                                                    Else
                                                        If IsNumeric(strStdLead) Then
                                                            intStdLead = CType(strStdLead, Integer)
                                                        Else
                                                            intStdLead = 0
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    intStdLead = 0
                                                End Try

                                            Case "ITEMNUM"
                                                Try
                                                    strItemNum = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strItemNum) = "" Then
                                                        strItemNum = " "
                                                    Else
                                                        strItemNum = Trim(strItemNum)
                                                    End If
                                                    If Len(Trim(strItemNum)) > 18 Then
                                                        strItemNum = Microsoft.VisualBasic.Left(strItemNum, 18)
                                                    End If
                                                Catch ex As Exception
                                                    strItemNum = " "
                                                End Try

                                            Case "MAXLEVEL"
                                                Try
                                                    strQtyMax = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strQtyMax) = "" Then
                                                        decQtyMax = 0
                                                    Else
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

                                            Case "MINLEVEL"
                                                Try
                                                    strReorderPoint = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strReorderPoint) = "" Then
                                                        decReorderPoint = 0
                                                    Else
                                                        If IsNumeric(strReorderPoint) Then
                                                            decReorderPoint = CType(strReorderPoint, Decimal)
                                                            decReorderPoint = Math.Round(decReorderPoint, 4)
                                                        Else
                                                            decReorderPoint = 0
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    decReorderPoint = 0
                                                End Try

                                            Case "MODELNUM"
                                                Try
                                                    strMfgItmId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strMfgItmId) = "" Then
                                                        strMfgItmId = " "
                                                    Else
                                                        strMfgItmId = Trim(strMfgItmId)
                                                    End If
                                                    If Len(Trim(strMfgItmId)) > 50 Then
                                                        strMfgItmId = Microsoft.VisualBasic.Left(strMfgItmId, 50)
                                                    End If
                                                Catch ex As Exception
                                                    strMfgItmId = " "
                                                End Try

                                            Case "BINNUM"
                                                Try
                                                    strIsaBinId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strIsaBinId) = "" Then
                                                        strIsaBinId = " "
                                                    Else
                                                        strIsaBinId = Trim(strIsaBinId)
                                                    End If
                                                    If Len(Trim(strIsaBinId)) > 30 Then
                                                        strIsaBinId = Microsoft.VisualBasic.Left(strIsaBinId, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strIsaBinId = " "
                                                End Try

                                            Case "MANUFACTURER"
                                                Try
                                                    strIsaCustMfg = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strIsaCustMfg) = "" Then
                                                        strIsaCustMfg = " "
                                                    Else
                                                        strIsaCustMfg = Trim(strIsaCustMfg)
                                                    End If
                                                    If Len(Trim(strIsaCustMfg)) > 40 Then
                                                        strIsaCustMfg = Microsoft.VisualBasic.Left(strIsaCustMfg, 40)
                                                    End If
                                                Catch ex As Exception
                                                    strIsaCustMfg = " "
                                                End Try

                                            Case "CATEGORY"
                                                Try
                                                    strInvStockType = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strInvStockType) = "" Then
                                                        strInvStockType = " "
                                                    Else
                                                        strInvStockType = Trim(strInvStockType)
                                                    End If
                                                    If Len(Trim(strInvStockType)) > 4 Then
                                                        strInvStockType = Microsoft.VisualBasic.Left(strInvStockType, 4)
                                                    End If
                                                Catch ex As Exception
                                                    strInvStockType = " "
                                                End Try

                                            Case "ORDERUNIT"
                                                Try
                                                    strUom_Po = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strUom_Po) = "" Then
                                                        strUom_Po = " "
                                                    Else
                                                        strUom_Po = Trim(strUom_Po)
                                                    End If
                                                    If Len(Trim(strUom_Po)) > 3 Then
                                                        strUom_Po = Microsoft.VisualBasic.Left(strUom_Po, 3)
                                                    End If
                                                Catch ex As Exception
                                                    strUom_Po = " "
                                                End Try

                                            Case "ISSUEUNIT" ' 
                                                Try
                                                    strUOM = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strUOM) = "" Then
                                                        strUOM = " "
                                                    Else
                                                        strUOM = Trim(strUOM)
                                                    End If
                                                    If Len(Trim(strUOM)) > 3 Then
                                                        strUOM = Microsoft.VisualBasic.Left(strUOM, 3)
                                                    End If
                                                Catch ex As Exception
                                                    strUOM = " "
                                                End Try

                                            Case "SITEID" ' 
                                                Try
                                                    strSiteId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strSiteId) = "" Then
                                                        strSiteId = " "
                                                    Else
                                                        strSiteId = Trim(strSiteId)
                                                    End If
                                                    If Len(Trim(strSiteId)) > 30 Then
                                                        strSiteId = Microsoft.VisualBasic.Left(strSiteId, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strSiteId = " "
                                                End Try

                                            Case Else
                                                'do nothing
                                        End Select
                                        
                                    Next  ' every node in <INVENTORY>  ' For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                    ' defaults - values not in INVENTORY interface
                                    strComments2000 = " "
                                    strInvItemType = " "
                                    strDescr1 = " "
                                    strCommodGrp = " "

                                    'collected all info - starting insert
                                    Dim rowsaffected As Integer = 0
                                    Dim strSQLstring As String = ""
                                    If Trim(strItemNum) <> "" And Trim(strSiteId) <> "" Then
                                        strSQLstring = "INSERT INTO SYSADM8.PS_ISA_MXM_ITEM_IN (CUST_ID,PLANT,ISA_ITEM,INV_ITEM_ID,DT_TIMESTAMP,CATEGORY_ID,IM_CFFT,EFF_STATUS" & vbCrLf & _
                                            ",INV_STOCK_TYPE,ISA_CUSTOMER_MFG,MFG_ITM_ID,UNIT_OF_MEASURE,UOM_PO,PREFERRED_MFG,ISA_BIN_ID,REORDER_POINT" & vbCrLf & _
                                            ",QTY_MAXIMUM,REORDER_QTY,STANDARD_COST,STD_LEAD,UTILIZ_CD,CLIENT_CRIT_SPARE,ISA_ITEM_NOTES,INV_ITEM_TYPE" & vbCrLf & _
                                            ",TAX_FLAG,STORAGE_AREA,PROCESS_FLAG,PROCESS_INSTANCE,DATE_PROCESSED,COMMENTS_2000,INTFC_REC_TYPE) " & vbCrLf & _
                                            "VALUES('" & strCustId & "','" & strSiteId & "','" & strItemNum & "','" & strInvItemId & "',CURRENT_TIMESTAMP,'" & strCommodGrp & "','" & strDescr1 & "','A'" & vbCrLf & _
                                            ",'" & strInvStockType & "','" & strIsaCustMfg & "','" & strMfgItmId & "','" & strUOM & "','" & strUom_Po & "','Y','" & strIsaBinId & "'," & decReorderPoint & "" & vbCrLf & _
                                            "," & decQtyMax & "," & decReorderQty & "," & decStdCost & "," & intStdLead & ",' ',' ',' ','" & strInvItemType & "'" & vbCrLf & _
                                            ",' ','" & strStorArea & "','N',0,NULL,'" & strComments2000 & "','INVENTORY')"

                                        Try
                                            rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                                            If rowsaffected = 0 Then
                                                myLoggr1.WriteErrorLog(rtn & " :: Error while inserting: 'rowsaffected = 0' for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                                myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                bolError = True
                                            End If
                                        Catch ex As Exception
                                            myLoggr1.WriteErrorLog(rtn & " :: Error inserting: " & ex.Message & " for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                            myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                            bolError = True
                                        End Try

                                    Else
                                        'empty line
                                        myLoggr1.WriteErrorLog(rtn & " :: Error: fields PLANT,ISA_ITEM are empty for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                    End If
                                End If
                            End If
                        Next '  nodeMxItem.ChildNodes(iMxItem) - each <INVENTORY>
                    End If  '  nodeMxItem.ChildNodes.Count > 0   -  <MXINVENTORY>

                End If   '  UCase(nodeMatMastReq.ChildNodes(iCnt).Name) = "MXINVENTORY"
            Next  '  ChildNodes(iCnt) 
        End If  '  nodeMatMastReq.ChildNodes.Count > 0

        Return strXMLError

    End Function

    Private Function GetCytecMatMastIn() As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "CytecMxmMatMast.GetCytecMatMastIn"

        Console.WriteLine("Start Insert of CytecMxm Mat. Mast. in SYSADM8.PS_ISA_MXM_ITEM_IN")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of CytecMxm Mat. Mast. in SYSADM8.PS_ISA_MXM_ITEM_IN")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\CytecMxmIn\XMLIN\")
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
                For I = 0 To aFiles.Length - 1
                    sXMLFilename = aFiles(I).Name
                    bLineError = False
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
                            File.Move(aFiles(I).FullName, "C:\INTFCXML\BadXML\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try

                    '<setting name="inputDirectory" serializeAs="String">
                    '    <value>\\ims\SDIWebProcessorsXMLFiles</value>
                    '</setting>

                    '    <value>C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles</value>

                    If Trim(strXMLError) = "" Then
                        root = xmlRequest.DocumentElement

                        If root.FirstChild Is Nothing Then
                            strXMLError = "empty XML file"
                        ElseIf root.LastChild.Name.ToUpper = "CONTENT" Then
                            strXMLError = ""
                        Else
                            strXMLError = "Missing last XML Element"
                        End If

                        If Trim(strXMLError) = "" Then
                            Dim strInterfaceType As String = UCase(xmlRequest.ChildNodes(1).Name())
                            '  MXINVENTORYInterface MXINVENTORYINTERFACE
                            '  MXITEMInterface MXITEMINTERFACE

                            'Dim nodeMatMastReq As XmlNode = xmlRequest.SelectSingleNode(xpath:="//Content")
                            Dim nodeMatMastReq As XmlNode = root.LastChild()
                            Select Case strInterfaceType
                                Case "MXITEMINTERFACE"
                                    strXMLError = ProcessItemInterface(nodeMatMastReq, aFiles(I).Name, bolError)
                                Case "MXINVENTORYINTERFACE"
                                    strXMLError = ProcessInventoryInterface(nodeMatMastReq, aFiles(I).Name, bolError)
                                Case Else
                                    strXMLError = "Unknown Interface type: " & strInterfaceType
                            End Select

                        End If ' Trim(strXMLError) = "" ' inner if
                    End If ' Trim(strXMLError) = ""

                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bolError Then
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
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\XMLINProcessed\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLINProcessed\" & aFiles(I).Name)

                    End If
                Next ' aFiles(I)

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
            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\" & aFiles(I).Name)

            Return True
        End Try

        Return bolError

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "CytecMxmMatMast.SendEmail"
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
        email.Body &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >CytecMxmMatMast Error</span></center>&nbsp;&nbsp;"

        email.Body &= "<table><tr><td>CytecMxmMatMast has completed with "
        If bolWarning = True Then
            email.Body &= "warnings,"
            email.Subject = "CytecMxmMatMast Warning"
        Else
            email.Body &= "errors;"
            email.Subject = "CytecMxmMatMast Error"
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
                            Dim myFileName2 As String = "C:\CytecMxmIn\BadXML\" & arrErrFiles(int1)
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

            SendLogger(mailer.Subject, mailer.Body, "CYTECMXMMATMASTIN", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

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
