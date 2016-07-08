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
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdCytecPurchReqsXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\ErredSQLsPurchReqs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
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
            logpath = sLogPath & "\UpdCytecPurchReqsXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        'process received info
        Call ProceesCytecMxmPurchReqsInfo()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Sub ProceesCytecMxmPurchReqsInfo()

        Dim rtn As String = "CytecMxmPurcReqs.ProceesCytecMxmPurchReqsInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start CytecMxm Purchase Reqs XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of CytecMxm Purchase Reqs Inbound")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "\\ims\SDIWebProcessorsXMLFiles\PurchReqs"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "\\ims\SDIWebProcessorsXMLFiles\PurchReqs"
        End Try
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer

        Dim strSrcFileName As String = ""
        Try
            If aSrcFiles.Length > 0 Then
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > Len("CYTECMXM_PREQS_XML") - 1 Then
                        strSrcFileName = UCase(aSrcFiles(I).Name)
                        If strSrcFileName.StartsWith("CYTECMXM_PREQS_XML") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\CytecMxmIn\XMLIN\PurchReqs\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLIN\PurchReqs\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\CytecMxmIn\XMLIN\PurchReqs\ " & "...")
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
            bolError = GetCytecPurchReqsIn()
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

        m_logger.WriteInformationLog(rtn & " :: End of CytecMxm Purchase Reqs IN Process")

    End Sub

    Private Function GetCytecPurchReqsIn() As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "CytecMxmPurchReqs.GetCytecPurchReqsIn"

        Console.WriteLine("Start Insert of CytecMxm Purchase Reqs in PS_ISA_MXM_REQ_IN ")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of CytecMxm Purchase Reqs in PS_ISA_MXM_REQ_IN ")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\CytecMxmIn\XMLIN\PurchReqs\")
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
                            File.Move(aFiles(I).FullName, "C:\INTFCXML\BadXML\PurchReqs\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML\PurchReqs\ folder): " & ex24.Message & " in file " & aFiles(I).Name)
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
                            '' MXPRINTERFACE

                            Dim nodeStkReservReq As XmlNode = root.LastChild()
                            Select Case strInterfaceType
                                Case "MXPRINTERFACE"
                                    strXMLError = ProcessPrPrlineInterface(nodeStkReservReq, aFiles(I).Name, dtCreationDateTime, bolError)
                                    
                                Case Else
                                    strXMLError = "Unknown Interface type: " & strInterfaceType
                            End Select

                        End If  '  inner If Trim(strXMLError) = "" Then
                    End If   '  If Trim(strXMLError) = "" Then

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
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\PurchReqs\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\PurchReqs\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\XMLINProcessed\PurchReqs\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLINProcessed\PurchReqs\" & aFiles(I).Name)

                    End If
                Next  '  For I = 0 To aFiles.Length - 1

            Else  '  If aFiles.Length = 0 Then

            End If  '  If aFiles.Length > 0 Then

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
            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\PurchReqs\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\BadXML\PurchReqs\" & aFiles(I).Name)

            Return True

        End Try

        Return bolError

    End Function

    Private Function ProcessPrPrlineInterface(ByVal nodeStkReservReq As XmlNode, ByVal strFileName As String, _
                    ByVal dtCreationDateTime As DateTime, ByRef bolError As String) As String
        Dim strXMLError As String = ""
        Dim rtn As String = "CytecMxmPurchReqs.ProcessPrPrlineInterface"

        m_logger.WriteInformationLog(rtn & " :: Start ProcessPrPrlineInterface")

        ' get existing node CONTENT
        Dim strCustId As String = "CYTEC"
        Dim strProcessedFlag As String = "N"
        Dim strInvItemId As String = " "
        Dim decProcInstnce As Decimal = 0  '  PROCESS_INSTANCE
        Dim dtProcessDttm As DateTime = Nothing  ' NULL
        Dim strReqId As String = " "
        Dim intReqLineNo As Integer = 0
        Dim strActivityId As String = " "
        Dim strWorkOrderType As String = " "

        If nodeStkReservReq.ChildNodes.Count > 0 Then '  Content

            Dim strSiteId As String = ""
            Dim strIsaCustReq As String = ""
            Dim strReqStatus As String = ""
            Dim strIsaCustPriority As String = ""
            Dim strIsaUnloadingPt As String = ""
            Dim strBuyerId As String = ""
            Dim strOrdHdrNote As String = ""
            Dim strShiptoAttnTo As String = ""
            Dim strRequestorId As String = ""
            Dim strStatusDate As String = ""
            Dim dtStatusDate As DateTime = Nothing
            'PRLINE nodes - arrays
            Dim arrIntfcType(0) As String ' "MATERIAL"  '  "ITEM"
            arrIntfcType(0) = ""
            Dim arrIsaItem(0) As String
            arrIsaItem(0) = ""
            Dim arrIsaReqLineNum(0) As Integer
            arrIsaReqLineNum(0) = 0
            Dim arrIsaWorkOrdNum(0) As String
            arrIsaWorkOrdNum(0) = ""
            Dim arrUOM(0) As String
            arrUOM(0) = ""
            Dim arrDueDate(0) As DateTime
            arrDueDate(0) = Nothing
            Dim arrQtyRequested(0) As Decimal
            arrQtyRequested(0) = 0
            Dim arrEmplId(0) As String
            arrEmplId(0) = ""
            Dim arrIsaMachineNum(0) As String
            arrIsaMachineNum(0) = ""
            Dim arrDescr254(0) As String
            arrDescr254(0) = ""
            Dim arrIsaCustChargeCd(0) As String
            arrIsaCustChargeCd(0) = ""
            Dim arrManuf(0) As String
            arrManuf(0) = ""
            Dim arrManufItemId(0) As String
            arrManufItemId(0) = ""
            Dim arrPriceReq(0) As Decimal
            arrPriceReq(0) = 0
            Dim arrEnteredBy(0) As String
            arrEnteredBy(0) = ""
            Dim arrEnteredDate(0) As DateTime
            arrEnteredDate(0) = Nothing
            Dim arrMrNum(0) As String
            arrMrNum(0) = ""
            Dim arrMrLineNum(0) As Integer
            arrMrLineNum(0) = 0

            Dim iCnt As Integer = 0
            For iCnt = 0 To nodeStkReservReq.ChildNodes.Count - 1
                ''  MXPRInterface/Content/MXPR/PR/PRLINE/PRCOST  
                If UCase(nodeStkReservReq.ChildNodes(iCnt).Name) = "MXPR" Then
                    Dim nodeMxItem As XmlNode = nodeStkReservReq.ChildNodes(iCnt)
                    If nodeMxItem.ChildNodes.Count > 0 Then
                        Dim iNumOfPrlineNodes As Integer = 0
                        Dim iMxItem As Integer = 0
                        For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1
                            If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "PR" Then
                                Dim nodeItemMM As XmlNode = nodeMxItem.ChildNodes(iMxItem)
                                If nodeItemMM.ChildNodes.Count > 0 Then
                                    'reset number of PRLINE nodes
                                    iNumOfPrlineNodes = 0
                                    ' those are nodes of PR with special node PRLINE
                                    strSiteId = ""
                                    strIsaCustReq = ""
                                    strReqStatus = ""
                                    strIsaCustPriority = ""
                                    strIsaUnloadingPt = ""
                                    strBuyerId = ""
                                    strOrdHdrNote = ""
                                    strShiptoAttnTo = ""
                                    strRequestorId = ""
                                    strStatusDate = ""
                                    dtStatusDate = Nothing
                                    'PRLINE nodes - arrays
                                    ReDim arrIsaItem(0)
                                    arrIsaItem(0) = ""
                                    ReDim arrIntfcType(0)
                                    arrIntfcType(0) = ""
                                    ReDim arrIsaReqLineNum(0)
                                    arrIsaReqLineNum(0) = 0
                                    ReDim arrIsaWorkOrdNum(0)
                                    arrIsaWorkOrdNum(0) = ""
                                    ReDim arrUOM(0)
                                    arrUOM(0) = ""
                                    ReDim arrDueDate(0)
                                    arrDueDate(0) = Nothing
                                    ReDim arrQtyRequested(0)
                                    arrQtyRequested(0) = 0
                                    ReDim arrEmplId(0)
                                    arrEmplId(0) = ""
                                    ReDim arrIsaMachineNum(0)
                                    arrIsaMachineNum(0) = ""
                                    ReDim arrDescr254(0)
                                    arrDescr254(0) = ""
                                    ReDim arrIsaCustChargeCd(0)
                                    arrIsaCustChargeCd(0) = ""
                                    ReDim arrManuf(0)
                                    arrManuf(0) = ""
                                    ReDim arrManufItemId(0)
                                    arrManufItemId(0) = ""
                                    ReDim arrPriceReq(0)
                                    arrPriceReq(0) = 0
                                    ReDim arrEnteredBy(0)
                                    arrEnteredBy(0) = ""
                                    ReDim arrEnteredDate(0)
                                    arrEnteredDate(0) = Nothing
                                    ReDim arrMrNum(0)
                                    arrMrNum(0) = ""
                                    ReDim arrMrLineNum(0)
                                    arrMrLineNum(0) = 0

                                    Dim iItemMM As Integer = 0
                                    Dim strNodeName As String = ""
                                    For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1  '  nodes of PR

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

                                            Case "PRNUM"  '  ISA_CUST_REQ  '  10  '  strIsaCustReq
                                                Try
                                                    strIsaCustReq = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strIsaCustReq) = "" Then
                                                        strIsaCustReq = " "
                                                    Else
                                                        strIsaCustReq = Trim(strIsaCustReq)
                                                    End If
                                                    If Len(Trim(strIsaCustReq)) > 10 Then
                                                        strIsaCustReq = Microsoft.VisualBasic.Left(strIsaCustReq, 10)
                                                    End If
                                                Catch ex As Exception
                                                    strIsaCustReq = " "
                                                End Try

                                            Case "STATUS"  '  REQ_STATUS  '  4  '  strReqStatus
                                                Try
                                                    strReqStatus = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strReqStatus) = "" Then
                                                        strReqStatus = " "
                                                    Else
                                                        strReqStatus = Trim(strReqStatus)
                                                    End If
                                                    If Len(Trim(strReqStatus)) > 4 Then
                                                        strReqStatus = Microsoft.VisualBasic.Left(strReqStatus, 4)
                                                    End If
                                                Catch ex As Exception
                                                    strReqStatus = " "
                                                End Try

                                            Case "PRIORITY"  '  ISA_CUST_PRIORITY  '  10  '  strIsaCustPriority
                                                Try
                                                    strIsaCustPriority = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strIsaCustPriority) = "" Then
                                                        strIsaCustPriority = " "
                                                    Else
                                                        strIsaCustPriority = Trim(strIsaCustPriority)
                                                    End If
                                                    If Len(Trim(strIsaCustPriority)) > 10 Then
                                                        strIsaCustPriority = Microsoft.VisualBasic.Left(strIsaCustPriority, 10)
                                                    End If
                                                Catch ex As Exception
                                                    strIsaCustPriority = " "
                                                End Try

                                            Case "SHIPTO"  ' ISA_UNLOADING_PT  '  60  ' strIsaUnloadingPt
                                                Try
                                                    strIsaUnloadingPt = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strIsaUnloadingPt) = "" Then
                                                        strIsaUnloadingPt = " "
                                                    Else
                                                        strIsaUnloadingPt = Trim(strIsaUnloadingPt)
                                                    End If
                                                    If Len(Trim(strIsaUnloadingPt)) > 60 Then
                                                        strIsaUnloadingPt = Microsoft.VisualBasic.Left(strIsaUnloadingPt, 60)
                                                    End If
                                                Catch ex As Exception
                                                    strIsaUnloadingPt = " "
                                                End Try

                                            Case "BUYER"  '   BUYER_ID  '   30   '  strBuyerId
                                                Try
                                                    strBuyerId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strBuyerId) = "" Then
                                                        strBuyerId = " "
                                                    Else
                                                        strBuyerId = Trim(strBuyerId)
                                                    End If
                                                    If Len(Trim(strBuyerId)) > 30 Then
                                                        strBuyerId = Microsoft.VisualBasic.Left(strBuyerId, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strBuyerId = " "
                                                End Try

                                            Case "DESCRIPTION"  ' ORD_HDR_NOTE  '  254  '  strOrdHdrNote
                                                Try
                                                    strOrdHdrNote = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strOrdHdrNote) = "" Then
                                                        strOrdHdrNote = " "
                                                    Else
                                                        strOrdHdrNote = Trim(strOrdHdrNote)
                                                    End If
                                                    If Len(Trim(strOrdHdrNote)) > 254 Then
                                                        strOrdHdrNote = Microsoft.VisualBasic.Left(strOrdHdrNote, 254)
                                                    End If
                                                Catch ex As Exception
                                                    strOrdHdrNote = " "
                                                End Try

                                            Case "SHIPTOATTN"  '  SHIPTO_ATTN_TO ' 30  ' strShiptoAttnTo
                                                Try
                                                    strShiptoAttnTo = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strShiptoAttnTo) = "" Then
                                                        strShiptoAttnTo = " "
                                                    Else
                                                        strShiptoAttnTo = Trim(strShiptoAttnTo)
                                                    End If
                                                    If Len(Trim(strShiptoAttnTo)) > 30 Then
                                                        strShiptoAttnTo = Microsoft.VisualBasic.Left(strShiptoAttnTo, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strShiptoAttnTo = " "
                                                End Try

                                            Case "REQUESTEDBY"  ' REQUESTOR_ID  ' 30  '  strRequestorId
                                                Try
                                                    strRequestorId = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strRequestorId) = "" Then
                                                        strRequestorId = " "
                                                    Else
                                                        strRequestorId = Trim(strRequestorId)
                                                    End If
                                                    If Len(Trim(strRequestorId)) > 30 Then
                                                        strRequestorId = Microsoft.VisualBasic.Left(strRequestorId, 30)
                                                    End If
                                                Catch ex As Exception
                                                    strRequestorId = " "
                                                End Try

                                            Case "STATUSDATE"   '  STATUS_DT ' DATE  ' strStatusDate   '  dtStatusDate
                                                Try
                                                    strStatusDate = nodeItemMM.ChildNodes(iItemMM).InnerText
                                                    If Trim(strStatusDate) = "" Then
                                                        dtStatusDate = Nothing
                                                    Else
                                                        strStatusDate = Trim(strStatusDate)
                                                        If IsDate(strStatusDate) Then
                                                            dtStatusDate = CType(strStatusDate, DateTime)
                                                        Else
                                                            dtStatusDate = Nothing
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    dtStatusDate = Nothing
                                                End Try

                                            Case "PRLINE"
                                                'process all nodes of the PRLINE
                                                Dim nodePrline As XmlNode = nodeItemMM.ChildNodes(iItemMM)
                                                Dim iPrline As Integer = 0
                                                Dim strPrlineNodeName As String = ""
                                                If nodePrline.ChildNodes.Count > 0 Then
                                                    iNumOfPrlineNodes += 1
                                                    Dim strNodeInnerText As String = ""
                                                    Dim intNodeInnerText As Integer = 0
                                                    Dim dtNodeInnerText As DateTime = Nothing
                                                    Dim decNodeInnerText As Decimal = 0

                                                    For iPrline = 0 To nodePrline.ChildNodes.Count - 1
                                                        strPrlineNodeName = UCase(nodePrline.ChildNodes(iPrline).Name)
                                                        strNodeInnerText = ""
                                                        intNodeInnerText = 0
                                                        dtNodeInnerText = Nothing
                                                        decNodeInnerText = 0

                                                        Select Case strPrlineNodeName
                                                            Case "ITEMNUM"  ' ISA_ITEM  '  18  ' arrIsaItem()
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrIsaItem(iNumOfPrlineNodes - 1)
                                                                    ReDim Preserve arrIntfcType(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 18 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 18)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrIsaItem(iNumOfPrlineNodes - 1) = strNodeInnerText
                                                                ' for ITEMNUM only
                                                                If Trim(strNodeInnerText) = "" Then
                                                                    arrIntfcType(iNumOfPrlineNodes - 1) = "MATERIAL"
                                                                Else
                                                                    arrIntfcType(iNumOfPrlineNodes - 1) = "ITEM"
                                                                End If

                                                            Case "PRLINENUM"  ' ISA_REQ_LINE_NBR  '  int 0 '  arrIsaReqLineNum
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrIsaReqLineNum(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        intNodeInnerText = 0
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsNumeric(strNodeInnerText) Then
                                                                            intNodeInnerText = CType(strNodeInnerText, Integer)
                                                                        Else
                                                                            intNodeInnerText = 0
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    intNodeInnerText = 0
                                                                End Try
                                                                arrIsaReqLineNum(iNumOfPrlineNodes - 1) = intNodeInnerText

                                                            Case "REFWO"  '  ISA_WORK_ORDER_NO '  20 ' arrIsaWorkOrdNum
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrIsaWorkOrdNum(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 20 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 20)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrIsaWorkOrdNum(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "ORDERUNIT"  ' UNIT_OF_MEASURE ' 3 ' arrUOM
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrUOM(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 3 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 3)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrUOM(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "REQDELIVERYDATE"  ' DUE_DATE ' DATE '  arrDueDate
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrDueDate(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        dtNodeInnerText = Nothing
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsDate(strNodeInnerText) Then
                                                                            dtNodeInnerText = CType(strNodeInnerText, DateTime)
                                                                        Else
                                                                            dtNodeInnerText = Nothing
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    dtNodeInnerText = Nothing
                                                                End Try
                                                                arrDueDate(iNumOfPrlineNodes - 1) = dtNodeInnerText

                                                            Case "ORDERQTY"  ' QTY_REQUESTED '  Dec 4  '  arrQtyRequested
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrQtyRequested(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        decNodeInnerText = 0
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsNumeric(strNodeInnerText) Then
                                                                            decNodeInnerText = CType(strNodeInnerText, Decimal)
                                                                            decNodeInnerText = Math.Round(decNodeInnerText, 4)
                                                                        Else
                                                                            decNodeInnerText = 0
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    decNodeInnerText = 0
                                                                End Try
                                                                arrQtyRequested(iNumOfPrlineNodes - 1) = decNodeInnerText

                                                            Case "REQUESTEDBY"  ' EMPLID ' 11 ' arrEmplId
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrEmplId(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 11 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 11)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrEmplId(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "ASSETNUM"  ' ISA_MACHINE_NO '  20 '  arrIsaMachineNum
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrIsaMachineNum(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 20 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 20)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrIsaMachineNum(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "DESCRIPTION"  ' DESCR254_MIXED ' 254 ' arrDescr254
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrDescr254(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 254 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 254)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrDescr254(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "GLDEBITACCT"  ' ISA_CUST_CHARGE_CD ' 40  ' arrIsaCustChargeCd
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrIsaCustChargeCd(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 40 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 40)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrIsaCustChargeCd(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "MANUFACTURER"  ' ISA_MFG_FREEFORM ' 30 ' arrManuf
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrManuf(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 30 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 30)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrManuf(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "MODELNUM"  ' MFG_ITM_ID ' 50 ' arrManufItemId
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrManufItemId(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 50 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 50)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrManufItemId(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "UNITCOST"  ' PRICE_REQ ' Dec 0 ' arrPriceReq
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrPriceReq(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        decNodeInnerText = 0
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsNumeric(strNodeInnerText) Then
                                                                            decNodeInnerText = CType(strNodeInnerText, Decimal)
                                                                            decNodeInnerText = Math.Round(decNodeInnerText, 5)
                                                                        Else
                                                                            decNodeInnerText = 0
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    decNodeInnerText = 0
                                                                End Try
                                                                arrPriceReq(iNumOfPrlineNodes - 1) = decNodeInnerText

                                                            Case "ENTERBY"  ' ENTERED_BY ' 30 ' arrEnteredBy
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrEnteredBy(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 30 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 30)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrEnteredBy(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "ENTERDATE"  ' ENTERED_DT ' DATE ' arrEnteredDate
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrEnteredDate(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        dtNodeInnerText = Nothing
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsDate(strNodeInnerText) Then
                                                                            dtNodeInnerText = CType(strNodeInnerText, DateTime)
                                                                        Else
                                                                            dtNodeInnerText = Nothing
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    dtNodeInnerText = Nothing
                                                                End Try
                                                                arrEnteredDate(iNumOfPrlineNodes - 1) = dtNodeInnerText

                                                            Case "MRNUM"  '  strMrNum  '  ISA_REQUEST_ID ' 10
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrMrNum(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        strNodeInnerText = " "
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                    End If
                                                                    If Len(Trim(strNodeInnerText)) > 10 Then
                                                                        strNodeInnerText = Microsoft.VisualBasic.Left(strNodeInnerText, 10)
                                                                    End If
                                                                Catch ex As Exception
                                                                    strNodeInnerText = " "
                                                                End Try
                                                                arrMrNum(iNumOfPrlineNodes - 1) = strNodeInnerText

                                                            Case "MRLINENUM"    '  ISA_REQUEST_LN_NBR  '  Int 0
                                                                If iNumOfPrlineNodes > 1 Then
                                                                    ReDim Preserve arrMrLineNum(iNumOfPrlineNodes - 1)
                                                                End If
                                                                Try
                                                                    strNodeInnerText = nodePrline.ChildNodes(iPrline).InnerText
                                                                    If Trim(strNodeInnerText) = "" Then
                                                                        intNodeInnerText = 0
                                                                    Else
                                                                        strNodeInnerText = Trim(strNodeInnerText)
                                                                        If IsNumeric(strNodeInnerText) Then
                                                                            intNodeInnerText = CType(strNodeInnerText, Integer)
                                                                        Else
                                                                            intNodeInnerText = 0
                                                                        End If
                                                                    End If
                                                                Catch ex As Exception
                                                                    intNodeInnerText = 0
                                                                End Try
                                                                arrMrLineNum(iNumOfPrlineNodes - 1) = intNodeInnerText

                                                            Case Else
                                                                'do nothing
                                                        End Select   '  PRLINE
                                                    Next  '  For iPrline = 0 To nodePrline.ChildNodes.Count - 1

                                                End If  '  If nodePrline.ChildNodes.Count > 0 Then
                                            Case Else
                                                ' do nothing
                                        End Select   '  PR
                                    Next  '  For iItemMM = 0 To nodeItemMM.ChildNodes.Count - 1   '  nodes of PR

                                    'collected all info - starting insert
                                    Dim rowsaffected As Integer = 0
                                    Dim strSQLstring As String = ""
                                    ' one insert for each PRLINE
                                    If iNumOfPrlineNodes > 0 Then
                                        Dim iCnt1 As Integer = 0
                                        For iCnt1 = 0 To iNumOfPrlineNodes - 1
                                            strSQLstring = "INSERT INTO SYSADM8.PS_ISA_MXM_REQ_IN (CUST_ID,PLANT,ISA_ITEM,INV_ITEM_ID," & vbCrLf & _
                                                "PROCESS_INSTANCE,PROCESS_FLAG,PROCESS_DTTM,DTTM_CREATED,REQ_ID,REQ_LINE_NBR," & vbCrLf & _
                                                "ISA_CUST_REQ,ISA_REQ_LINE_NBR,ISA_WORK_ORDER_NO,REQ_STATUS,WORK_ORDER_TYPE," & vbCrLf & _
                                                "ISA_CUST_PRIORITY,ACTIVITY_ID,UNIT_OF_MEASURE,ISA_CUSTOMER_UOM,DUE_DATE," & vbCrLf & _
                                                "ISA_UNLOADING_PT,QTY_REQUESTED,EMPLID,ISA_MACHINE_NO,DESCR254_MIXED," & vbCrLf & _
                                                "ISA_CUST_CHARGE_CD,BUYER_ID,ORD_HDR_NOTE,SHIPTO_ATTN_TO,REQUESTOR_ID," & vbCrLf & _
                                                "STATUS_DT," & vbCrLf & _
                                                "ISA_MFG_FREEFORM,MFG_ITM_ID,PRICE_REQ,ENTERED_BY," & vbCrLf & _
                                                "ENTERED_DT,INTFC_REC_TYPE" & vbCrLf & _
                                                ",ISA_REQUEST_ID,ISA_REQUEST_LN_NBR) " & vbCrLf & _
                                                "VALUES ('" & strCustId & "','" & strSiteId & "','" & arrIsaItem(iCnt1) & "','" & strInvItemId & "'," & vbCrLf & _
                                                "0,'N',NULL,TO_DATE('" & dtCreationDateTime & "', 'MM/DD/YYYY HH:MI:SS AM'),' ',0," & vbCrLf & _
                                                "'" & strIsaCustReq & "','" & arrIsaReqLineNum(iCnt1) & "','" & arrIsaWorkOrdNum(iCnt1) & "','" & strReqStatus & "',' '," & vbCrLf & _
                                                "'" & strIsaCustPriority & "',' ',' ','" & arrUOM(iCnt1) & "'," & vbCrLf
                                            If arrDueDate(iCnt1) = Nothing Then
                                                strSQLstring = strSQLstring & "NULL,"
                                            Else
                                                strSQLstring = strSQLstring & "TO_DATE('" & arrDueDate(iCnt1) & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                            End If
                                            strSQLstring = strSQLstring & "" & vbCrLf & _
                                                "'" & strIsaUnloadingPt & "'," & arrQtyRequested(iCnt1) & ",'" & arrEmplId(iCnt1) & "','" & arrIsaMachineNum(iCnt1) & "','" & arrDescr254(iCnt1) & "'," & vbCrLf & _
                                                "'" & arrIsaCustChargeCd(iCnt1) & "','" & strBuyerId & "','" & strOrdHdrNote & "','" & strShiptoAttnTo & "','" & strRequestorId & "'," & vbCrLf
                                            If dtStatusDate = Nothing Then
                                                strSQLstring = strSQLstring & "NULL,"
                                            Else
                                                strSQLstring = strSQLstring & "TO_DATE('" & dtStatusDate & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                            End If
                                            strSQLstring = strSQLstring & "" & vbCrLf & _
                                            "'" & arrManuf(iCnt1) & "','" & arrManufItemId(iCnt1) & "'," & arrPriceReq(iCnt1) & ",'" & arrEnteredBy(iCnt1) & "'," & vbCrLf
                                            If arrEnteredDate(iCnt1) = Nothing Then
                                                strSQLstring = strSQLstring & "NULL,"
                                            Else
                                                strSQLstring = strSQLstring & "TO_DATE('" & arrEnteredDate(iCnt1) & "', 'MM/DD/YYYY HH:MI:SS AM'),"
                                            End If
                                            strSQLstring = strSQLstring & "" & vbCrLf & _
                                            "'" & arrIntfcType(iCnt1) & "'" & vbCrLf & _
                                            ",'" & arrMrNum(iCnt1) & "'," & arrMrLineNum(iCnt1) & ")"

                                            Try
                                                rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                                                If rowsaffected = 0 Then
                                                    myLoggr1.WriteErrorLog(rtn & " :: Error while inserting: 'rowsaffected = 0' for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                                    strXMLError = rtn & " :: Error while inserting: 'rowsaffected = 0' for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString()
                                                    myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                    bolError = True
                                                End If
                                            Catch ex As Exception
                                                myLoggr1.WriteErrorLog(rtn & " :: Error inserting: " & ex.Message & " for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString())
                                                strXMLError = rtn & " :: Error inserting: " & ex.Message & " for the file: " & strFileName & " and item number (0 based): " & iCnt.ToString()
                                                myLoggr1.WriteInformationLog(rtn & " :: SQL String: " & strSQLstring)
                                                bolError = True
                                            End Try
                                        Next

                                    End If
                                End If  '  If nodeItemMM.ChildNodes.Count > 0 Then

                            End If  '  If UCase(nodeMxItem.ChildNodes(iMxItem).Name) = "PR" Then
                        Next  '  For iMxItem = 0 To nodeMxItem.ChildNodes.Count - 1

                    End If  '  If nodeMxItem.ChildNodes.Count > 0 Then
                End If  '  If UCase(nodeStkReservReq.ChildNodes(iCnt).Name) = "MXPR" Then
            Next  '  For iCnt = 0 To nodeStkReservReq.ChildNodes.Count - 1

        End If  '  If nodeStkReservReq.ChildNodes.Count > 0 Then '  Content

        Return strXMLError

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "CytecMxmPurchReqs.SendEmail"
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
        email.Body &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >CytecMxmPurchReqsIn Error</span></center>&nbsp;&nbsp;"

        email.Body &= "<table><tr><td>CytecMxmPurchOrders has completed with "
        If bolWarning = True Then
            email.Body &= "warnings,"
            email.Subject = " CytecMxmPurchReqsIn Warning"
        Else
            email.Body &= "errors;"
            email.Subject = " CytecMxmPurchReqsIn Error"
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
                            Dim myFileName2 As String = "C:\CytecMxmIn\BadXML\PurchReqs\" & arrErrFiles(int1)
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

            SendLogger(mailer.Subject, mailer.Body, "CYTECMXMPURCHREQSIN", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

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
