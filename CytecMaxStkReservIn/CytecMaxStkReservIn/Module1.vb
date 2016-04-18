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
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdCytecStkReservXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\ErredSQLsStkReserv" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")
    'Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
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
            logpath = sLogPath & "\UpdCytecStkReservXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        'process received info
        Call ProceesCytecMxmStkReservInfo()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Sub ProceesCytecMxmStkReservInfo()

        Dim rtn As String = "CytecMxmStkReserv.ProceesCytecMxmStkReservInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start CytecMxm Stock Reserv. XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of CytecMxm Stock Reserv. Inbound")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "\\ims\SDIWebProcessorsXMLFiles\StockReserv"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "\\ims\SDIWebProcessorsXMLFiles\StockReserv"
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
                    If aSrcFiles(I).Name.Length > Len("CYTECMXM_SR_XML") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("CYTECMXM_SR_XML") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\CytecMxmIn\XMLIN\StockReserv\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLIN\StockReserv\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\CytecMxmIn\XMLIN\StockReserv\ " & "...")
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
        '// CytecMxmStkReserv Processing: inbound to the Oracle table PS_ISA_MXM_RSV ...
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = GetCytecStkReservIn()
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

        m_logger.WriteInformationLog(rtn & " :: End of CytecMxm Stock Reserv. IN Process")


    End Sub

    Private Function ProcessInvResInterface(ByVal nodeStkReservReq As XmlNode, ByVal strFileName As String, ByVal dtCreationDateTime As DateTime, ByRef bolError As String) As String
        Dim strXMLError As String = ""
        Dim rtn As String = "CytecMxmStkReserv.ProcessInvResInterface"

        m_logger.WriteInformationLog(rtn & " :: Start ProcessInvResInterface")

        ' get existing node CONTENT
        Dim strCustId As String = "CYTEC"
        Dim strProcessedFlag As String = "N"
        Dim strIsaItem As String = ""
        Dim strIntfcType As String = "INVRESERVE"
        Dim decProcInstnce As Decimal = 0  '  PROCESS_INSTANCE
        Dim strOrderNo As String = " "
        Dim intOrderLineNo As Integer = 0

        If nodeStkReservReq.ChildNodes.Count > 0 Then

            Dim strWorkOrderStatus As String = ""
            Dim strSiteId As String = ""
            Dim strWorkOrderNum As String = ""
            Dim strReqstOrderRef As String = ""
            Dim strWoPrior As String = ""
            Dim intWoPrior As Integer = 0
            Dim strWoType As String = ""
            Dim strActivityId As String = ""
            Dim strDueDate As String = ""
            Dim dtDueDate As DateTime
            Dim strShiptoName As String = ""
            Dim strInvItemId As String = ""
            Dim strQtyReqst As String = ""
            Dim decQtyReqst As Decimal = 0
            Dim strEmplId As String = ""
            Dim strMachineNum As String = ""
            Dim strUnloadingPoint As String = ""
            Dim strEnteredDate As String = ""
            Dim dtEnteredDate As DateTime

            Dim iCnt As Integer = 0
            For iCnt = 0 To nodeStkReservReq.ChildNodes.Count - 1
                If UCase(nodeStkReservReq.ChildNodes(iCnt).Name) = "MXINVRES" Then
                    Dim nodeMxItem As XmlNode = nodeStkReservReq.ChildNodes(iCnt)
                    If nodeMxItem.ChildNodes.Count > 0 Then
                        Dim iMxItem As Integer = 0
                        For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1
                            If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "INVRESERVE" Then
                                Dim nodeItemMM As XmlNode = nodeMxItem.ChildNodes(iMxItem)
                                If nodeItemMM.ChildNodes.Count > 0 Then
                                    strSiteId = ""
                                    strWorkOrderNum = ""
                                    strReqstOrderRef = ""
                                    strWorkOrderStatus = ""
                                    strWoPrior = ""
                                    intWoPrior = 0
                                    strWoType = ""
                                    strActivityId = ""
                                    strDueDate = ""
                                    dtDueDate = Nothing
                                    strShiptoName = ""
                                    strInvItemId = ""
                                    strQtyReqst = ""
                                    decQtyReqst = 0
                                    strEmplId = ""
                                    strMachineNum = ""
                                    strUnloadingPoint = ""
                                    strEnteredDate = ""
                                    dtEnteredDate = Nothing

                                    Dim iItemMM As Integer = 0
                                    Dim strNodeName As String = ""
                                    For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                        strNodeName = UCase(nodeItemMM.ChildNodes(iItemMM).Name)
                                        Select Case strNodeName
                                            Case "SITEID"
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

                                            Case "WONUM" '  ISA_WORK_ORDER_NO  ' 20
                                                Try  '  strWorkOrderNum
                                                    strWorkOrderNum = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strWorkOrderNum) = "" Then
                                                        strWorkOrderNum = " "
                                                    Else
                                                        strWorkOrderNum = Trim(strWorkOrderNum)
                                                    End If
                                                    If Len(Trim(strWorkOrderNum)) > 20 Then
                                                        strWorkOrderNum = Microsoft.VisualBasic.Left(strWorkOrderNum, 20)
                                                    End If
                                                Catch ex As Exception
                                                    strWorkOrderNum = " "
                                                End Try

                                            Case "REQUESTNUM"  '  ORDER_REF ' 20
                                                Try  '  strReqstOrderRef
                                                    strReqstOrderRef = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strReqstOrderRef) = "" Then
                                                        strReqstOrderRef = " "
                                                    Else
                                                        strReqstOrderRef = Trim(strReqstOrderRef)
                                                    End If
                                                    If Len(Trim(strWorkOrderNum)) > 20 Then
                                                        strReqstOrderRef = Microsoft.VisualBasic.Left(strReqstOrderRef, 20)
                                                    End If
                                                Catch ex As Exception
                                                    strReqstOrderRef = " "
                                                End Try

                                            Case "REQUIREDDATE"  '  DUE_DT '  DATE
                                                Try  ' strDueDate
                                                    strDueDate = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strDueDate) = "" Then
                                                        dtDueDate = Nothing
                                                    Else
                                                        strDueDate = Trim(strDueDate)
                                                    End If
                                                    If IsDate(strDueDate) Then
                                                        dtDueDate = CType(strDueDate, DateTime)
                                                    End If
                                                Catch ex As Exception
                                                    dtDueDate = Nothing
                                                End Try

                                            Case "ISSUETO"  ' SHIPTO_ATTN_TO  '  30
                                                Try  '  strShiptoName
                                                    strShiptoName = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strShiptoName) = "" Then
                                                        strShiptoName = " "
                                                    Else
                                                        strShiptoName = Trim(strShiptoName)
                                                    End If
                                                    If Len(Trim(strShiptoName)) > 30 Then
                                                        strShiptoName = Microsoft.VisualBasic.Left(strShiptoName, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strShiptoName = " "
                                                End Try

                                            Case "ITEMNUM"  ' INV_ITEM_ID  '  18
                                                Try  '  strInvItemId
                                                    strInvItemId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strInvItemId) = "" Then
                                                        strInvItemId = " "
                                                    Else
                                                        strInvItemId = Trim(strInvItemId)
                                                    End If
                                                    If Len(Trim(strInvItemId)) > 18 Then
                                                        strInvItemId = Microsoft.VisualBasic.Left(strInvItemId, 18)
                                                    End If
                                                Catch ex As Exception
                                                    strInvItemId = " "
                                                End Try

                                            Case "RESERVEDQTY"  ' QTY_REQUESTED '  0 Decimal
                                                Try  '  strQtyReqst
                                                    strQtyReqst = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strQtyReqst) = "" Then
                                                        decQtyReqst = 0
                                                    Else
                                                        strQtyReqst = Trim(strQtyReqst)
                                                    End If
                                                    If IsNumeric(strQtyReqst) Then
                                                        decQtyReqst = CType(strQtyReqst, Decimal)
                                                    Else
                                                        decQtyReqst = 0
                                                    End If
                                                Catch ex As Exception
                                                    decQtyReqst = 0
                                                End Try

                                            Case "REQUESTEDBY"  ' EMPLID  '  11
                                                Try  '  strEmplId
                                                    strEmplId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strEmplId) = "" Then
                                                        strEmplId = " "
                                                    Else
                                                        strEmplId = Trim(strEmplId)
                                                    End If
                                                    If Len(Trim(strEmplId)) > 11 Then
                                                        strEmplId = Microsoft.VisualBasic.Left(strEmplId, 11)
                                                    End If
                                                Catch ex As Exception
                                                    strEmplId = " "
                                                End Try

                                            Case "ASSETNUM"  ' ISA_MACHINE_NO  '  20
                                                Try  '  strMachineNum
                                                    strMachineNum = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strMachineNum) = "" Then
                                                        strMachineNum = " "
                                                    Else
                                                        strMachineNum = Trim(strMachineNum)
                                                    End If
                                                    If Len(Trim(strMachineNum)) > 20 Then
                                                        strMachineNum = Microsoft.VisualBasic.Left(strMachineNum, 20)
                                                    End If
                                                Catch ex As Exception
                                                    strMachineNum = " "
                                                End Try

                                            Case "DELLOCATION"  ' ISA_UNLOADING_PT ' 60 
                                                Try '  strUnloadingPoint
                                                    strUnloadingPoint = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strUnloadingPoint) = "" Then
                                                        strUnloadingPoint = " "
                                                    Else
                                                        strUnloadingPoint = Trim(strUnloadingPoint)
                                                    End If
                                                    If Len(Trim(strUnloadingPoint)) > 60 Then
                                                        strUnloadingPoint = Microsoft.VisualBasic.Left(strUnloadingPoint, 60)
                                                    End If
                                                Catch ex As Exception
                                                    strUnloadingPoint = " "
                                                End Try

                                            Case "REQUESTEDDATE"  '  ENTERED_DT  '  DATE
                                                Try  '  strEnteredDate
                                                    strEnteredDate = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strEnteredDate) = "" Then
                                                        dtEnteredDate = Nothing
                                                    Else
                                                        strEnteredDate = Trim(strEnteredDate)
                                                    End If
                                                    If IsDate(strEnteredDate) Then
                                                        dtEnteredDate = CType(strEnteredDate, DateTime)
                                                    End If
                                                Catch ex As Exception
                                                    dtEnteredDate = Nothing
                                                End Try

                                            Case Else
                                                'do nothing
                                        End Select
                                    Next

                                    ' defaults - values not in INVRESERV interface
                                    strWorkOrderStatus = " "
                                    intWoPrior = 0
                                    strWoType = " "
                                    strActivityId = " "
                                    strIsaItem = " "

                                    'collected all info - starting insert
                                    Dim rowsaffected As Integer = 0
                                    Dim strSQLstring As String = ""
                                    strSQLstring = "insert into SYSADM8.PS_ISA_MXM_RSV_IN (CUST_ID,PLANT,ISA_ITEM,INV_ITEM_ID,PROCESS_INSTANCE,PROCESS_FLAG,PROCESS_DTTM,DTTM_CREATED,"
                                    strSQLstring = strSQLstring & "ORDER_NO,ORDER_INT_LINE_NO,ISA_WORK_ORDER_NO,ORDER_REF,WORK_ORDER_STATUS,"
                                    strSQLstring = strSQLstring & "PRIORITY,WORK_ORDER_TYPE,ACTIVITY_ID,DUE_DT,SHIPTO_ATTN_TO,QTY_REQUESTED,"
                                    strSQLstring = strSQLstring & "EMPLID,ISA_MACHINE_NO,ISA_UNLOADING_PT,ENTERED_DT,INTFC_REC_TYPE) VALUES ("
                                    strSQLstring = strSQLstring & "'" & strCustId & "','" & strSiteId & "','" & strInvItemId & "','" & strIsaItem & "'," & decProcInstnce & ",'" & strProcessedFlag & "',NULL,TO_DATE('" & dtCreationDateTime & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    strSQLstring = strSQLstring & "'" & strOrderNo & "'," & intOrderLineNo & ",'" & strWorkOrderNum & "','" & strReqstOrderRef & "','" & strWorkOrderStatus & "',"
                                    strSQLstring = strSQLstring & "" & intWoPrior & ",'" & strWoType & "','" & strActivityId & "',"
                                    If dtDueDate = Nothing Then
                                        strSQLstring = strSQLstring & "NULL,"
                                    Else
                                        strSQLstring = strSQLstring & "TO_DATE('" & dtDueDate & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    End If
                                    strSQLstring = strSQLstring & "'" & strShiptoName & "'," & decQtyReqst & ","
                                    strSQLstring = strSQLstring & "'" & strEmplId & "','" & strMachineNum & "','" & strUnloadingPoint & "',"
                                    If dtEnteredDate = Nothing Then
                                        strSQLstring = strSQLstring & "NULL,"
                                    Else
                                        strSQLstring = strSQLstring & "TO_DATE('" & dtEnteredDate & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    End If
                                    strSQLstring = strSQLstring & "'INVRESERVE')"

                                    If Trim(strInvItemId) <> "" And Trim(strSiteId) <> "" And decQtyReqst > 0 Then
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
                                        myLoggr1.WriteErrorLog(rtn & " :: Error: one of the fields PLANT,ISA_WORK_ORDER_NO,INV_ITEM_ID is empty or no Qty > 0 for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                    End If

                                End If ' If nodeItemMM.ChildNodes.Count > 0 Then

                            End If ' If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "INVRESERVE" Then
                        Next

                    End If  '  If nodeMxItem.ChildNodes.Count > 0 Then
                End If
            Next
        End If ' If nodeStkReservReq.ChildNodes.Count > 0 Then

        Return strXMLError
    End Function

    Private Function ProcessWoInterface(ByVal nodeStkReservReq As XmlNode, ByVal strFileName As String, ByVal dtCreationDateTime As DateTime, ByRef bolError As String) As String
        Dim strXMLError As String = ""
        Dim rtn As String = "CytecMxmStkReserv.ProcessWoInterface"

        m_logger.WriteInformationLog(rtn & " :: Start ProcessWoInterface")

        ' get existing node CONTENT
        Dim strCustId As String = "CYTEC"
        Dim strProcessedFlag As String = "N"
        Dim strIsaItem As String = ""
        Dim strIntfcType As String = "WORKORDER"
        Dim decProcInstnce As Decimal = 0  '  PROCESS_INSTANCE
        Dim strOrderNo As String = " "
        Dim intOrderLineNo As Integer = 0

        If nodeStkReservReq.ChildNodes.Count > 0 Then
            Dim strWorkOrderNum As String = ""
            Dim strSiteId As String = ""
            Dim strReqstOrderRef As String = ""
            Dim strWorkOrderStatus As String = ""
            Dim strWoPrior As String = ""
            Dim intWoPrior As Integer = 0
            Dim strWoType As String = ""
            Dim strActivityId As String = ""
            Dim strDueDate As String = ""
            Dim dtDueDate As DateTime
            Dim strShiptoName As String = ""
            Dim strInvItemId As String = ""
            Dim decQtyReqst As Decimal = 0
            Dim strEmplId As String = ""
            Dim strMachineNum As String = ""
            Dim strUnloadingPoint As String = ""
            Dim dtEnteredDate As DateTime

            Dim iCnt As Integer = 0
            For iCnt = 0 To nodeStkReservReq.ChildNodes.Count - 1
                If UCase(nodeStkReservReq.ChildNodes(iCnt).Name) = "MXWO" Then
                    Dim nodeMxItem As XmlNode = nodeStkReservReq.ChildNodes(iCnt)
                    If nodeMxItem.ChildNodes.Count > 0 Then
                        Dim iMxItem As Integer = 0
                        For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1
                            If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "WORKORDER" Then
                                Dim nodeItemMM As XmlNode = nodeMxItem.ChildNodes(iMxItem)
                                If nodeItemMM.ChildNodes.Count > 0 Then
                                    strSiteId = ""
                                    strWorkOrderNum = ""
                                    strReqstOrderRef = ""
                                    strWorkOrderStatus = ""
                                    strWoPrior = ""
                                    intWoPrior = 0
                                    strWoType = ""
                                    strActivityId = ""
                                    strDueDate = ""
                                    'dtDueDate ???
                                    strShiptoName = ""
                                    strInvItemId = ""
                                    decQtyReqst = 0
                                    strEmplId = ""
                                    strMachineNum = ""
                                    strUnloadingPoint = ""

                                    Dim iItemMM As Integer = 0
                                    Dim strNodeName As String = ""
                                    For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1

                                        strNodeName = UCase(nodeItemMM.ChildNodes(iItemMM).Name)
                                        Select Case strNodeName
                                            Case "SITEID"  '  PLANT
                                                Try  '  strWorkOrderNum
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

                                            Case "WONUM"  '  ISA_WORK_ORDER_NO  ' 20
                                                Try  '  strWorkOrderNum
                                                    strWorkOrderNum = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strWorkOrderNum) = "" Then
                                                        strWorkOrderNum = " "
                                                    Else
                                                        strWorkOrderNum = Trim(strWorkOrderNum)
                                                    End If
                                                    If Len(Trim(strWorkOrderNum)) > 20 Then
                                                        strWorkOrderNum = Microsoft.VisualBasic.Left(strWorkOrderNum, 20)
                                                    End If
                                                Catch ex As Exception
                                                    strWorkOrderNum = " "
                                                End Try

                                            Case "STATUS"  '  WORK_ORDER_STATUS  '  1
                                                Try  '  strWorkOrderStatus
                                                    strWorkOrderStatus = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strWorkOrderStatus) = "" Then
                                                        strWorkOrderStatus = " "
                                                    Else
                                                        strWorkOrderStatus = Trim(strWorkOrderStatus)
                                                    End If
                                                    If Len(Trim(strWorkOrderStatus)) > 1 Then
                                                        strWorkOrderStatus = Microsoft.VisualBasic.Left(strWorkOrderStatus, 1)
                                                    End If
                                                Catch ex As Exception
                                                    strWorkOrderStatus = " "
                                                End Try

                                            Case "WOPRIORITY"  '  PRIORITY  '  0  '  Integer
                                                Try
                                                    strWoPrior = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strWoPrior) = "" Then
                                                        intWoPrior = 0
                                                    Else
                                                        If IsNumeric(strWoPrior) Then
                                                            intWoPrior = CType(strWoPrior, Integer)
                                                        Else
                                                            intWoPrior = 0
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    intWoPrior = 0
                                                End Try

                                            Case "WORKTYPE"  ' WORK_ORDER_TYPE  ' 5
                                                Try  '  strWoType
                                                    strWoType = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strWoType) = "" Then
                                                        strWoType = " "
                                                    Else
                                                        strWoType = Trim(strWoType)
                                                    End If
                                                    If Len(Trim(strWoType)) > 5 Then
                                                        strWoType = Microsoft.VisualBasic.Left(strWoType, 5)
                                                    End If
                                                Catch ex As Exception
                                                    strWoType = " "
                                                End Try

                                            Case "TASKID"  '  ACTIVITY_ID  ' 15
                                                Try  '  strActivityId
                                                    strActivityId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strActivityId) = "" Then
                                                        strActivityId = " "
                                                    Else
                                                        strActivityId = Trim(strActivityId)
                                                    End If
                                                    If Len(Trim(strActivityId)) > 15 Then
                                                        strActivityId = Microsoft.VisualBasic.Left(strActivityId, 15)
                                                    End If
                                                Catch ex As Exception
                                                    strActivityId = " "
                                                End Try

                                            Case ""

                                            Case ""

                                            Case Else
                                                'do nothing
                                        End Select
                                    Next

                                    ' defaults - values not in WO interface
                                    strReqstOrderRef = " "
                                    dtDueDate = Nothing
                                    strShiptoName = " "
                                    strInvItemId = " "
                                    decQtyReqst = 0
                                    strEmplId = " "
                                    strMachineNum = " "
                                    strUnloadingPoint = " "
                                    dtEnteredDate = Nothing
                                    strIsaItem = " "

                                    'collected all info - starting insert
                                    Dim rowsaffected As Integer = 0
                                    Dim strSQLstring As String = ""
                                    strSQLstring = "insert into SYSADM8.PS_ISA_MXM_RSV_IN (CUST_ID,PLANT,ISA_ITEM,INV_ITEM_ID,PROCESS_INSTANCE,PROCESS_FLAG,PROCESS_DTTM,DTTM_CREATED,"
                                    strSQLstring = strSQLstring & "ORDER_NO,ORDER_INT_LINE_NO,ISA_WORK_ORDER_NO,ORDER_REF,WORK_ORDER_STATUS,"
                                    strSQLstring = strSQLstring & "PRIORITY,WORK_ORDER_TYPE,ACTIVITY_ID,DUE_DT,SHIPTO_ATTN_TO,QTY_REQUESTED,"
                                    strSQLstring = strSQLstring & "EMPLID,ISA_MACHINE_NO,ISA_UNLOADING_PT,ENTERED_DT,INTFC_REC_TYPE) VALUES ("
                                    strSQLstring = strSQLstring & "'" & strCustId & "','" & strSiteId & "','" & strInvItemId & "','" & strIsaItem & "'," & decProcInstnce & ",'" & strProcessedFlag & "',NULL,TO_DATE('" & dtCreationDateTime & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    strSQLstring = strSQLstring & "'" & strOrderNo & "'," & intOrderLineNo & ",'" & strWorkOrderNum & "','" & strReqstOrderRef & "','" & strWorkOrderStatus & "',"
                                    strSQLstring = strSQLstring & "" & intWoPrior & ",'" & strWoType & "','" & strActivityId & "',"
                                    If dtDueDate = Nothing Then
                                        strSQLstring = strSQLstring & "NULL,"
                                    Else
                                        strSQLstring = strSQLstring & "TO_DATE('" & dtDueDate & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    End If
                                    strSQLstring = strSQLstring & "'" & strShiptoName & "'," & decQtyReqst & ","
                                    strSQLstring = strSQLstring & "'" & strEmplId & "','" & strMachineNum & "','" & strUnloadingPoint & "',"
                                    If dtEnteredDate = Nothing Then
                                        strSQLstring = strSQLstring & "NULL,"
                                    Else
                                        strSQLstring = strSQLstring & "TO_DATE('" & dtEnteredDate & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                    End If
                                    strSQLstring = strSQLstring & "'WORKORDER')"

                                    If Trim(strWorkOrderNum) <> "" And Trim(strSiteId) <> "" Then
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
                                        myLoggr1.WriteErrorLog(rtn & " :: Error: one of the fields PLANT,ISA_WORK_ORDER_NO is empty for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                    End If

                                End If ' If nodeItemMM.ChildNodes.Count > 0 Then
                            End If ' If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "WORKORDER" Then
                        Next

                    End If  '  If nodeMxItem.ChildNodes.Count > 0 Then
                End If
            Next
        End If ' If nodeStkReservReq.ChildNodes.Count > 0 Then

        Return strXMLError
    End Function

    Private Function GetCytecStkReservIn() As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "CytecMxmStkReserv.GetCytecStkReservIn"

        Console.WriteLine("Start Insert of CytecMxm Stock Reserv. in PS_ISA_MXM_RSV ")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of CytecMxm Stock Reserv. in PS_ISA_MXM_RSV")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\CytecMxmIn\XMLIN\StockReserv\")
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
                            File.Move(aFiles(I).FullName, "C:\INTFCXML\BadXML\StockReserv\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML\StockReserv\ folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try
                    
                    Dim strCreationDateTime As String = ""
                    Dim strHeaderChildNodeName As String = ""
                    Dim dtCreationDateTime As DateTime = Now()

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
                            'get CreationDateTime
                            Dim nodeHeader As XmlNode = root.FirstChild()
                            Dim intHeader As Integer = 0
                            If nodeHeader.ChildNodes.Count > 0 Then
                                For intHeader = 0 To nodeHeader.ChildNodes.Count - 1
                                    strHeaderChildNodeName = UCase(nodeHeader.ChildNodes(intHeader).Name)
                                    Select Case strHeaderChildNodeName
                                        Case "CREATIONDATETIME"
                                            strCreationDateTime = nodeHeader.ChildNodes(intHeader).InnerText
                                        Case Else
                                            'do nothing
                                    End Select
                                    If Trim(strCreationDateTime) <> "" Then
                                        strCreationDateTime = Trim(strCreationDateTime)
                                        Exit For
                                    End If
                                Next

                                If Trim(strCreationDateTime) <> "" Then
                                    'convert to DateTime
                                    If IsDate(strCreationDateTime) Then
                                        dtCreationDateTime = CType(strCreationDateTime, DateTime)
                                        'Dim strDate As String = ""
                                        'Dim strTime As String = ""
                                        'strDate = dtCreationDateTime.ToLongDateString()
                                        'strTime = dtCreationDateTime.ToLongTimeString()
                                    End If
                                End If

                            End If ' If nodeHeader.ChildNodes.Count > 0 Then

                            Dim strInterfaceType As String = UCase(xmlRequest.ChildNodes(1).Name())
                            ''  MXINVRESINTERFACE
                            ''  MXWOINTERFACE

                            Dim nodeStkReservReq As XmlNode = root.LastChild()
                            Select Case strInterfaceType
                                Case "MXINVRESINTERFACE"
                                    strXMLError = ProcessInvResInterface(nodeStkReservReq, aFiles(I).Name, dtCreationDateTime, bolError)
                                Case "MXWOINTERFACE"
                                    strXMLError = ProcessWoInterface(nodeStkReservReq, aFiles(I).Name, dtCreationDateTime, bolError)
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
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\StockReserv\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\StockReserv\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\XMLINProcessed\StockReserv\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLINProcessed\StockReserv\" & aFiles(I).Name)

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
            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\StockReserv\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\StockReserv\" & aFiles(I).Name)

            Return True
        End Try

        Return bolError

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "CytecMxmStkReserv.SendEmail"
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
        email.Body &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >CytecMxmMatMast Error</span></center>&nbsp;&nbsp;"

        email.Body &= "<table><tr><td>CytecMxmStkReserv has completed with "
        If bolWarning = True Then
            email.Body &= "warnings,"
            email.Subject = " (TEST) CytecMxmStkReserv Warning"
        Else
            email.Body &= "errors;"
            email.Subject = " (TEST) CytecMxmStkReserv Error"
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
            'Dim objException As String
            'Dim objExceptionTrace As String

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(subject, "SDIExchADMIN@sdi.com", EmailTo, "", EmailBcc, "N", body, m_CN)

        Catch ex As Exception

        End Try
    End Sub

End Module
