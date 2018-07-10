Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter


Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\AmazonSdiDirectIn"
    Dim logpath As String = "C:\AmazonSdiDirectIn\LOGS\UpdAmazonShipNoticeXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\AmazonSdiDirectIn\LOGS\ErredSQLsAmazonShipNotice" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")
    Dim strOverride As String
    Dim bolWarning As Boolean = False
    Dim strSiteBu As String = "ISA00"
    Dim strVendorId As String = "0000039777"  '  "0000041491"  '   "0000039777"  '  AMAZON ID
    Dim strUserId As String = "ASNVEND"

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

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
            logpath = sLogPath & "\UpdAmazonShipNoticeXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        'process received info
        Call ProcessAmazonShipNoticeInfo()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try
        Try
            myLoggr1.Dispose()
        Catch ex As Exception
        Finally
            myLoggr1 = Nothing
        End Try

    End Sub

    Private Sub ProcessAmazonShipNoticeInfo()

        Dim rtn As String = "AmazonShipNotice.ProcessAmazonShipNoticeInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start Amazon SDI Direct process Ship Notice XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of Amazon SDI Direct process Ship Notice Inbound")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "\\ims\SDIWebProcessorsXMLFiles\ShipNoticeAmazon"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "\\ims\SDIWebProcessorsXMLFiles\ShipNoticeAmazon"
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
                    If aSrcFiles(I).Name.Length > Len("ShipNotice") - 1 Then
                        strSrcFileName = UCase(aSrcFiles(I).Name)
                        If strSrcFileName.StartsWith("SHIPNOTICE") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\AmazonSdiDirectIn\XMLIN\ShipNoticeAmazon\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\XMLIN\ShipNoticeAmazon\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\AmazonSdiDirectIn\XMLIN\ShipNoticeAmazon\ " & "...")
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
        '// Amazon SDI Direct process Ship Notice: copy of Supplier Portal ASN process
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = AmazonSDIDirectShipNotc()
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

        m_logger.WriteInformationLog(rtn & " :: End of Amazon SDI Direct process Ship Notice")

    End Sub

    Private Function AmazonSDIDirectShipNotc() As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "AmazonShipNotice.AmazonSDIDirectShipNotc"

        Console.WriteLine("Start of the Amazon SDI Direct process Ship Notice: copy of Supplier Portal ASN process")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start of the Amazon SDI Direct process Ship Notice: copy of Supplier Portal ASN process")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\AmazonSdiDirectIn\XMLIN\ShipNoticeAmazon\")
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
                    'If I = 1 Then ' for testing only
                    '    Exit For
                    'End If
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
                            File.Move(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\ShipNoticeAmazon\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML\ShipNoticeAmazon\ folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try

                    root = xmlRequest.DocumentElement
                    Dim strTestMy1 As String = ""
                    If Trim(strXMLError) = "" Then
                        If root.ChildNodes.Count > 0 Then
                            If root.FirstChild Is Nothing Then
                                strXMLError = "Empty XML file"
                            ElseIf root.LastChild.Name.ToUpper = "REQUEST" Then
                                'strTestMy1 = root.LastChild.Name.ToUpper
                                'strTestMy1 = root.FirstChild.Name.ToUpper
                                strXMLError = ""
                            Else
                                strXMLError = "Missing last XML Element"
                            End If
                        Else
                            strXMLError = "Empty XML file"
                        End If  '  If root.ChildNodes.Count > 0 Then
                    End If  '  If Trim(strXMLError) = "" Then ' 1st

                    'start parsing XML file
                    If Trim(strXMLError) = "" Then

                        Dim nodeOrdConf As XmlNode = root.LastChild()

                        If nodeOrdConf.ChildNodes.Count > 0 Then
                            Dim strFirstChildName As String = nodeOrdConf.FirstChild.Name.ToUpper
                            'SHIPNOTICEREQUEST
                            Select Case strFirstChildName
                                Case "SHIPNOTICEREQUEST"
                                    Dim iCnt As Integer = 0
                                    Dim j1 As Integer = 0
                                    Dim strOrderNum As String = ""
                                    Dim strOrderDate As String = ""
                                    Dim strCarrierName As String = ""
                                    Dim strTrackngNumber As String = ""
                                    Dim arrLineNums(0) As String
                                    arrLineNums(0) = ""
                                    Dim arrLineQtys(0) As String
                                    arrLineQtys(0) = ""
                                    Dim arrUoms(0) As String
                                    arrUoms(0) = ""
                                    Dim strDelvrDate As String = ""
                                    Dim strShipmntId As String = ""
                                    Dim strShipmntDate As String = ""
                                    'parse and process
                                    Dim nodeConfReqst As XmlNode = nodeOrdConf.FirstChild() ' "SHIPNOTICEREQUEST"
                                    If nodeConfReqst.ChildNodes.Count > 0 Then
                                        j1 = 0
                                        ReDim arrLineNums(0)
                                        arrLineNums(0) = ""
                                        ReDim arrLineQtys(0)
                                        arrLineQtys(0) = ""
                                        ReDim arrUoms(0)
                                        arrUoms(0) = ""
                                        For iCnt = 0 To nodeConfReqst.ChildNodes.Count - 1
                                            Dim strNodeName1 As String = UCase(nodeConfReqst.ChildNodes(iCnt).Name)
                                            'header info
                                            If strNodeName1 = "SHIPNOTICEHEADER" Then
                                                'get all attributes
                                                Dim nodeShipNoticeHeadr As XmlNode = nodeConfReqst.ChildNodes(iCnt)  ' "SHIPNOTICEHEADER"
                                                If nodeShipNoticeHeadr.Attributes.Count > 0 Then
                                                    For Each attrib As XmlAttribute In nodeShipNoticeHeadr.Attributes()
                                                        If UCase(attrib.Name) = "DELIVERYDATE" Then
                                                            strDelvrDate = attrib.Value
                                                        End If
                                                        If UCase(attrib.Name) = "SHIPMENTID" Then
                                                            strShipmntId = attrib.Value
                                                        End If
                                                        If UCase(attrib.Name) = "SHIPMENTDATE" Then
                                                            strShipmntDate = attrib.Value
                                                        End If
                                                    Next  '  For Each attrib As XmlAttribute In nodeShipNoticeHeadr.Attributes()
                                                End If  '  If nodeShipNoticeHeadr.Attributes.Count > 0 Then
                                            End If  '  If strNodeName1 = "SHIPNOTICEHEADER" Then

                                            'header info
                                            If strNodeName1 = "SHIPCONTROL" Then
                                                Dim nodeShipControlMy As XmlNode = nodeConfReqst.ChildNodes(iCnt)  '  "SHIPCONTROL"
                                                If nodeShipControlMy.ChildNodes.Count > 0 Then
                                                    Dim strChldNodeNm As String = ""
                                                    For Each ChildItmNode As XmlNode In nodeShipControlMy.ChildNodes()
                                                        strChldNodeNm = UCase(ChildItmNode.Name)
                                                        Select Case strChldNodeNm
                                                            Case "CARRIERIDENTIFIER"
                                                                strCarrierName = ChildItmNode.InnerText
                                                            Case "SHIPMENTIDENTIFIER"
                                                                strTrackngNumber = ChildItmNode.InnerText
                                                            Case Else
                                                                'do nothing
                                                        End Select
                                                    Next  '  For Each ChildItmNode As XmlNode In nodeShipControlMy.ChildNodes()
                                                End If  '  If nodeShipControlMy.ChildNodes.Count > 0 Then
                                            End If  '  If strNodeName1 = "SHIPCONTROL" Then

                                            If strNodeName1 = "SHIPNOTICEPORTION" Then
                                                Dim nodeShipNoticeMy As XmlNode = nodeConfReqst.ChildNodes(iCnt)  '  "SHIPNOTICEPORTION"
                                                If nodeShipNoticeMy.ChildNodes.Count > 0 Then
                                                    Dim strChldNodeName1 As String = ""
                                                    For Each ChildItemNode As XmlNode In nodeShipNoticeMy.ChildNodes()
                                                        strChldNodeName1 = ChildItemNode.Name
                                                        Select Case strChldNodeName1

                                                            Case "OrderReference"  'order header info
                                                                'get all attributes
                                                                If ChildItemNode.Attributes.Count > 0 Then
                                                                    For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                        If UCase(attrib.Name) = "ORDERID" Then
                                                                            strOrderNum = attrib.Value
                                                                        End If
                                                                        If UCase(attrib.Name) = "ORDERDATE" Then
                                                                            strOrderDate = attrib.Value
                                                                        End If
                                                                    Next  '  For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                Else
                                                                    strXMLError = "Empty node 'OrderReference'"
                                                                End If  '  If ChildItemNode.Attributes.Count > 0 Then
                                                            Case "ShipNoticeItem"   'line info - could be repeated several times
                                                                'get all attributes
                                                                If ChildItemNode.Attributes.Count > 0 Then
                                                                    'Dim bOK As Boolean = True
                                                                    For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                        If UCase(attrib.Name) = "QUANTITY" Then
                                                                            If j1 = 0 Then
                                                                            Else
                                                                                ReDim Preserve arrLineQtys(j1)
                                                                            End If
                                                                            arrLineQtys(j1) = attrib.Value
                                                                        End If
                                                                        If UCase(attrib.Name) = "LINENUMBER" Then
                                                                            If j1 = 0 Then
                                                                            Else
                                                                                ReDim Preserve arrLineNums(j1)
                                                                            End If
                                                                            arrLineNums(j1) = attrib.Value
                                                                        End If
                                                                    Next  '  For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                    'get all childs
                                                                    If ChildItemNode.ChildNodes.Count > 0 Then
                                                                        For Each ItemNode As XmlNode In ChildItemNode.ChildNodes()
                                                                            If UCase(ItemNode.Name) = "UNITOFMEASURE" Then
                                                                                If j1 = 0 Then
                                                                                Else
                                                                                    ReDim Preserve arrUoms(j1)
                                                                                End If
                                                                                arrUoms(j1) = ItemNode.InnerText
                                                                            End If
                                                                        Next  '  For Each ItemNode As XmlNode In ChildItemNode.ChildNodes()
                                                                    End If
                                                                    If Trim(arrUoms(j1)) = "" Then
                                                                        arrUoms(j1) = "EA"
                                                                    End If
                                                                    If Trim(arrLineQtys(j1)) <> "" And Trim(arrLineNums(j1)) <> "" Then
                                                                        j1 = j1 + 1
                                                                    End If
                                                                End If  '  If ChildItemNode.Attributes.Count > 0 Then

                                                            Case Else
                                                                'do nothing
                                                        End Select  '  Select Case strChldNodeName1

                                                    Next  '  For Each ChildItemNode As XmlNode In nodeShipNoticeMy.ChildNodes()
                                                    If j1 = 0 Then
                                                        'empty line items - get out
                                                        strXMLError = "Empty node(s) 'ShipNoticeItem'"
                                                    End If
                                                Else
                                                    strXMLError = "Empty node 'ShipNoticePortion'"
                                                End If  '  If nodeShipNoticeMy.ChildNodes.Count > 0 Then
                                            End If  '  If strNodeName1 = "SHIPNOTICEPORTION" Then

                                        Next  '  For iCnt = 0 To nodeConfReqst.ChildNodes.Count - 1

                                        If Trim(strXMLError) = "" Then
                                            'write to Oracle tables - we have strOrderNum, strOrderDate and arrLineNums(j1) etc.

                                            Dim intNextReceiver As Int64 = 0
                                            Dim strNextReceiver As String = ""
                                            Dim intLines As Integer = 0
                                            Dim decQty As Decimal = 0
                                            Dim strSmallSite As String = ""
                                            Dim strASNSite As String = ""
                                            Dim ReceivingWebServiceAlreadyRun As Boolean = False
                                            Dim strMessageText As String = ""

                                            'get Vendor ID based on strOrderNum
                                            strVendorId = GetVendorId(strOrderNum)

                                            strSmallSite = "Y"  '  getSmallSiteFlag(strOrderNum, strSiteBu)
                                            strASNSite = getASNFlag(strOrderNum, strSiteBu)

                                            ' here is how we create receipts

                                            'Small Site     ASN    
                                            '_______________________
                                            'Y              <>Y             Creates an Xref comments record
                                            'Y              Y                  RECEIVER is created when the site does receiving
                                            'N              Y               Creates an receiver record 
                                            'N              <>Y                Creates an xref comment record
                                            '_______________________

                                            'will use code from clsReceiver.vb in SDiExchange
                                            Dim arrParamdtgLine As ArrayList
                                            Dim trnsactSession As OleDbTransaction = Nothing
                                            connectOR.Open()
                                            trnsactSession = connectOR.BeginTransaction
                                            Dim strUserIdN1 As String = "ASNVEN2"  ' use here instead of strUserId
                                            Dim strTariffCode As String = ""
                                            Dim strItemWeight As String = ""
                                            Dim strShipToId As String = " "
                                            strShipToId = getshipto(strOrderNum, strVendorId)
                                            If Trim(strShipToId) = "" Then
                                                strShipToId = " "
                                            End If

                                            If Trim(strShipmntId) = "" Then
                                                strShipmntId = " "
                                            End If
                                            If Trim(strShipmntId) = "0" Then
                                                strShipmntId = " "
                                            End If
                                            'get order info - from Page_Load code
                                            Dim POdataSet As DataSet = New DataSet()
                                            POdataSet = getpoinfo(strOrderNum, strVendorId, strSiteBu, strSmallSite)

                                            Try
                                                If Not POdataSet Is Nothing Then
                                                    If POdataSet.Tables.Count > 0 Then
                                                        If POdataSet.Tables(0).Rows.Count > 0 Then
                                                            
                                                            Dim iMyCnt As Integer = 0
                                                            Dim iLoc1 As Integer = -1
                                                            Dim strErsAction As String = POdataSet.Tables(0).Rows(0).Item("ERS_ACTION")
                                                            Dim strVendLoc As String = POdataSet.Tables(0).Rows(0).Item("VNDR_LOC")

                                                            If Not ReceivingWebServiceAlreadyRun Then
                                                                Dim bSucceessRcv As Boolean = False
                                                                strMessageText = ""
                                                                strNextReceiver = ""

                                                                bSucceessRcv = CallReceiverWebService(strSiteBu, strOrderNum, arrLineQtys, arrLineNums, strMessageText, strNextReceiver)
                                                                'bSucceessRcv = True
                                                                'strNextReceiver = "0004964709"

                                                                If Not bSucceessRcv Then
                                                                    trnsactSession.Rollback()
                                                                    connectOR.Close()
                                                                    'write error message
                                                                    strXMLError = "Error from web service call. Error message: " & strMessageText

                                                                Else
                                                                    ReceivingWebServiceAlreadyRun = True
                                                                End If
                                                            End If ' If Not ReceivingWebServiceAlreadyRun Then

                                                            If Trim(strXMLError) = "" Then

                                                                For iMyCnt = 0 To POdataSet.Tables(0).Rows.Count - 1
                                                                    iLoc1 = -1
                                                                    If arrLineNums.Contains(POdataSet.Tables(0).Rows(iMyCnt).Item("LINE_NBR")) Then '  check is line exists in Shipping Notice
                                                                        iLoc1 = Array.IndexOf(arrLineNums, POdataSet.Tables(0).Rows(iMyCnt).Item("LINE_NBR").ToString())

                                                                        If strSmallSite = "N" And strASNSite <> "Y" Then
                                                                            createXrefComment(POdataSet.Tables(0).Rows(iMyCnt).Item("LINE_NBR"), strOrderNum, strSiteBu, POdataSet.Tables(0).Rows(iMyCnt).Item("SCHED_NBR"), strUserId)

                                                                        ElseIf strSmallSite = "Y" And strASNSite <> "Y" Then

                                                                            createXrefComment(POdataSet.Tables(0).Rows(iMyCnt).Item("LINE_NBR"), strOrderNum, strSiteBu, POdataSet.Tables(0).Rows(iMyCnt).Item("SCHED_NBR"), strUserId)
                                                                            createASNShipped(POdataSet.Tables(0).Rows(iMyCnt).Item("LINE_NBR"), strOrderNum, POdataSet.Tables(0).Rows(iMyCnt).Item("QTY_PO"), strSiteBu, POdataSet.Tables(0).Rows(iMyCnt).Item("SCHED_NBR"), strUserId, strOrderDate, POdataSet.Tables(0).Rows(iMyCnt).Item("UNIT_OF_MEASURE"), strTrackngNumber, strCarrierName)
                                                                        Else

                                                                            intLines = intLines + 1
                                                                            Dim truth As Boolean = True
                                                                            
                                                                            Dim row As DataRow = POdataSet.Tables(0).Rows(iMyCnt)
                                                                            Dim strMy21Error As String = " "

                                                                            arrParamdtgLine = initializeParamsArrRadGrid(row, strOrderNum, strVendorId, strUserIdN1, strCarrierName, strTrackngNumber, strShipmntId, strSiteBu)

                                                                            truth = createReceiver92(iMyCnt, strNextReceiver, intLines, arrParamdtgLine, strErsAction, "R", trnsactSession, connectOR, " ", strMy21Error)
                                                                            If Not truth Then
                                                                                strXMLError = " subroutine 'createReceiver92' returned False. Error: " & strMy21Error
                                                                                trnsactSession.Rollback()
                                                                                trnsactSession = Nothing
                                                                                connectOR.Close()
                                                                                
                                                                            End If
                                                                        End If  '  based on strSmallSite And strASNSite

                                                                    End If  '   If arrLineNums.Contains(iMyCnt.ToString()) Then

                                                                Next  '  For iMyCnt = 0 To POdataSet.Tables(0).Rows.Count - 1

                                                            End If  ' If Trim(strXMLError) = "" Then - after web service call

                                                        Else
                                                            strXMLError = "Empty POdataSet 1st"
                                                        End If
                                                    Else
                                                        strXMLError = "Empty POdataSet 2nd"
                                                    End If
                                                Else
                                                    strXMLError = "Empty POdataSet 3rd"
                                                End If
                                            Catch exPoDs As Exception
                                                strXMLError = "Error in POdataSet for Order No: " & strOrderNum & vbCrLf & _
                                                    "Error: " & exPoDs.Message
                                            End Try

                                            If Trim(strXMLError) = "" Then
                                                'close connection/commit
                                                Try
                                                    trnsactSession.Commit()
                                                    trnsactSession = Nothing
                                                    connectOR.Close()
                                                Catch ex As Exception
                                                    trnsactSession = Nothing
                                                    Try
                                                        connectOR.Close()
                                                    Catch

                                                    End Try
                                                    connectOR = Nothing
                                                End Try
                                            End If
                                            
                                        End If  '  If Trim(strXMLError) = "" Then  '  3rd

                                    Else
                                        strXMLError = "Empty node 'ShipNoticeRequest'"
                                    End If  '  If nodeConfReqst.ChildNodes.Count > 0 Then
                                Case Else
                                    strXMLError = "Unexpected node name: " & strFirstChildName
                            End Select  '  Select Case strFirstChildName
                        Else
                            strXMLError = "Empty node 'Request'"
                        End If  ' If nodeOrdConf.ChildNodes.Count > 0 Then
                    End If  '  If Trim(strXMLError) = "" Then ' 2nd
                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bolError Then
                        bolError = True
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
                        File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\ShipNoticeAmazon\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\BadXML\ShipNoticeAmazon\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\XMLINProcessed\ShipNoticeAmazon\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\XMLINProcessed\ShipNoticeAmazon\" & aFiles(I).Name)

                    End If
                Next  '  For I = 0 To aFiles.Length - 1
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
            File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\ShipNoticeAmazon\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\BadXML\ShipNoticeAmazon\" & aFiles(I).Name)

            Return True

        End Try

        Return bolError

    End Function

    Public Function CallReceiverWebService(ByVal selectedBU As String, ByVal strPO As String,
                    ByVal arrQtys() As String, ByVal arrLineNumbers() As String,
                    ByRef strMessageText As String, ByRef ReceiverID As String) As Boolean

        System.Net.ServicePointManager.CertificatePolicy = New AlwaysIgnoreCertPolicy
        System.Net.ServicePointManager.SecurityProtocol = 3072

        ''ticket 120072 new receiving webservice 20170721 Yury ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim callISA_Receiving As SDI_ISA_RECEIPT.ISA_RECEIPT = New SDI_ISA_RECEIPT.ISA_RECEIPT
        Dim setISA_Receiving As New SDI_ISA_RECEIPT.ISA_PORCH_SDIEX_TypeShape
        Dim setISA_Receiving_L As New SDI_ISA_RECEIPT.ISA_PORCL_SDIEX_TypeShape
        Try

            Dim I As Integer

            With setISA_Receiving
                .BUSINESS_UNIT_PO = selectedBU
                .PO_ID = strPO
            End With
            Dim iRowIndex As Integer = 0 'ticket 120072 20170721

            For I = 0 To arrQtys.Count - 1

                setISA_Receiving_L = New SDI_ISA_RECEIPT.ISA_PORCL_SDIEX_TypeShape

                With setISA_Receiving_L
                    .BUSINESS_UNIT_PO = selectedBU
                    .PO_ID = strPO
                    .LINE_NBR = arrLineNumbers(I)
                    .QTY_PO = arrQtys(I)
                End With

                ReDim Preserve setISA_Receiving.ISA_PORCL_SDIEX(iRowIndex)
                setISA_Receiving.ISA_PORCL_SDIEX(iRowIndex) = setISA_Receiving_L

                iRowIndex += 1

            Next
            
            Dim respISAReceiving As SDI_ISA_RECEIPT.ISA_RCV_EX_RSP_TypeShape = callISA_Receiving.CallISA_RECEIPT(setISA_Receiving)
            strMessageText = respISAReceiving.messageText.ToString
            ReceiverID = respISAReceiving.receiverId.ToString
            If InStr(UCase(strMessageText), "SUCCESS") = 0 Then
                CallReceiverWebService = False
            Else
                CallReceiverWebService = True
            End If
        Catch ex As Exception
            CallReceiverWebService = False
        Finally
            callISA_Receiving.Dispose()
        End Try

    End Function

    Public Function initializeParamsArrRadGrid(ByVal row As DataRow, ByVal strPO As String, _
                    ByVal strVendor As String, _
                    ByVal strUserid As String, ByVal strCarrierName As String, ByVal strTrackngNumber As String, _
                    ByVal strShipmntId As String, ByVal strSiteBu As String, _
                    Optional ByVal strTariffCode As String = "", Optional ByVal strItemWeight As String = "") As ArrayList

        'strSQLString = "SELECT A.ERS_ACTION, A.VNDR_LOC," & vbCrLf & _
        '         " TO_CHAR(A.PO_DT,'MM-DD-YYYY') as PODT, B.LINE_NBR," & vbCrLf & _
        '         " A.BUSINESS_UNIT as PO_BU," & vbCrLf & _
        '         " C.SCHED_NBR," & vbCrLf & _
        '         " B.INV_ITEM_ID," & vbCrLf & _
        '         " B.MFG_ITM_ID," & vbCrLf & _
        '         " B.ITM_ID_VNDR," & vbCrLf & _
        '         " B.DESCR254_MIXED," & vbCrLf & _
        '         " C.QTY_PO," & vbCrLf & _
        '         " B.UNIT_OF_MEASURE," & vbCrLf & _
        '         " D.PRICE_PO, D.SHIPTO_ID," & vbCrLf & _
        '         " D.MERCHANDISE_AMT," & vbCrLf & _
        '         " B.MFG_ID," & vbCrLf & _
        '         " B.INV_STOCK_TYPE," & vbCrLf & _
        '         " ' ' as ISA_ASN_SHIP_DT," & vbCrLf & _
        '         " ' ' as ISA_ASN_TRACK_NO," & vbCrLf & _
        '         " ' ' as ISA_ASN_SHIP_VIA," & vbCrLf & _
        '         " ' ' as ISA_ASN_SHIP_VIA_ID," & vbCrLf & _
        '         " D.CURRENCY_CD AS CURRENCY_CD," & vbCrLf & _
        '         " D.CURRENCY_CD AS SHIP_CURRENCY," & vbCrLf & _
        '         " D.CURRENCY_CD_BASE AS SHIP_BASE_CURRENCY," & vbCrLf & _
        '         " C.CURRENCY_CD AS DIST_CURRENCY," & vbCrLf & _
        '         " C.CURRENCY_CD_BASE AS DIST_BASE_CURRENCY, " & vbCrLf & _
        '         " C.QTY_PO as hQTYPO," & vbCrLf
        '    QTY_LN_ACCPT, hQTYLNACCPT," & vbCrLf & _
        '" ' ' as BUSINESS_UNIT," & vbCrLf & _
        '" ' ' as RECEIVER_ID," & vbCrLf & _
        '" ' ' as RECV_LN_NBR," & vbCrLf & _
        '" G.ISA_SHIP_ID," & vbCrLf

        'strSQLString = strSQLString & " D.MERCHANDISE_AMT as hMERCHANDISEAMT" & vbCrLf & _

        Dim arrdtgline As New ArrayList

        Try

            arrdtgline.Insert(0, Decimal.Parse(row.Item("QTY_PO")))
            arrdtgline.Insert(1, strTrackngNumber)  'Trim(CType(item.FindControl("txtTrckno"), TextBox).Text.ToUpper))
            arrdtgline.Insert(2, strCarrierName)  '  CType(item.FindControl("cmbShipvia"), DropDownList).SelectedValue.ToUpper)
            arrdtgline.Insert(3, " ")
            arrdtgline.Insert(4, UCase(row.Item("DESCR254_MIXED")))

            arrdtgline.Insert(5, UCase(row.Item("MFG_ID")))
            arrdtgline.Insert(6, UCase(row.Item("LINE_NBR")))
            arrdtgline.Insert(7, UCase(row.Item("SCHED_NBR")))
            arrdtgline.Insert(8, strPO)
            arrdtgline.Insert(9, UCase(row.Item("INV_ITEM_ID")))
            arrdtgline.Insert(10, UCase(row.Item("INV_STOCK_TYPE")))
            arrdtgline.Insert(11, UCase(row.Item("hQTYLNACCPT")))
            arrdtgline.Insert(12, UCase(row.Item("hQtyPO")))
            arrdtgline.Insert(13, UCase(row.Item("UNIT_OF_MEASURE")))
            arrdtgline.Insert(14, strVendor)
            arrdtgline.Insert(15, UCase(row.Item("SHIPTO_ID")))  '  strShiptoID)
            arrdtgline.Insert(16, strUserid)
            arrdtgline.Insert(17, strTariffCode)
            arrdtgline.Insert(18, UCase(row.Item("MFG_ITM_ID")))
            arrdtgline.Insert(19, strItemWeight)

            ' PO business unit - strSiteBu
            arrdtgline.Insert(20, strSiteBu)  ' item("PO_BU").Text.ToUpper)
            
            ' po base currency and currency - default to USD
            arrdtgline.Insert(21, "USD")
            arrdtgline.Insert(22, "USD")
            arrdtgline.Insert(23, "USD")
            arrdtgline.Insert(24, "USD")
            arrdtgline.Insert(25, "USD")
            ' currency_cd
            Dim curr1 As String = ""
            Dim currency As String = ""
            Try

                currency = UCase(row.Item("CURRENCY_CD"))  ' item("CURRENCY_CD").Text.Trim.ToUpper
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(21) = currency
                End If
            Catch ex As Exception
            End Try
            ' ship currency
            curr1 = ""
            currency = ""
            Try

                currency = UCase(row.Item("SHIP_CURRENCY"))  ' item("SHIP_CURRENCY").Text.Trim.ToUpper
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(22) = currency
                End If
            Catch ex As Exception
            End Try
            ' ship base currency
            curr1 = ""
            currency = ""
            Try

                currency = UCase(row.Item("SHIP_BASE_CURRENCY"))  ' item("SHIP_BASE_CURRENCY").Text.Trim.ToUpper
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(23) = currency
                End If
            Catch ex As Exception
            End Try
            ' distrib currency
            curr1 = ""
            currency = ""
            Try

                currency = UCase(row.Item("DIST_CURRENCY"))  ' item("DIST_CURRENCY").Text.Trim.ToUpper
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(24) = currency
                End If
            Catch ex As Exception
            End Try
            ' distrib base currency
            curr1 = ""
            currency = ""
            Try

                currency = UCase(row.Item("DIST_BASE_CURRENCY"))  ' item("DIST_BASE_CURRENCY").Text.Trim.ToUpper
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(25) = currency
                End If
            Catch ex As Exception
            End Try

            Dim sMyReqid As String = " "
            'Try
            '    'sMyReqid = item("REQ_ID").Text.ToUpper
            '    If sMyReqid Is Nothing Then
            '        sMyReqid = ""
            '    Else
            '        ' check for HTML space code
            '        sMyReqid = UCase(sMyReqid)
            '        If sMyReqid.IndexOf("NBSP") > -1 Then
            '            sMyReqid = ""
            '        End If
            '    End If
            'Catch ex As Exception
            '    sMyReqid = ""
            'End Try
            arrdtgline.Insert(26, sMyReqid)

            Dim sMyReqLnNbr As String = " "
            'Try
            '    'sMyReqLnNbr = item("REQ_LINE_NBR").Text.ToUpper
            '    If sMyReqLnNbr Is Nothing Then
            '        sMyReqLnNbr = ""
            '    Else
            '        ' check for HTML space code
            '        sMyReqLnNbr = UCase(sMyReqLnNbr)
            '        If sMyReqLnNbr.IndexOf("NBSP") > -1 Then
            '            sMyReqLnNbr = ""
            '        End If
            '    End If
            'Catch ex As Exception
            '    sMyReqLnNbr = ""
            'End Try
            arrdtgline.Insert(27, sMyReqLnNbr)

        Catch ex As Exception
        End Try

        Return arrdtgline

    End Function

    Public Sub UpdateIntfcLTblStatusQtyRecv(ByVal sMyReqid As String, ByVal sMyReqLnNbr As String, ByVal strShipQtyStatus As String, _
                                ByVal decMyQtyRecv As Decimal)
        Dim sMyOrderLineStatus As String = ""
        Dim strError As String = ""

        Try
            If Trim(sMyReqid) <> "" And Trim(sMyReqLnNbr) <> "" Then

                Select Case strShipQtyStatus
                    Case "1"  '  fully received
                        sMyOrderLineStatus = "RCF"
                    Case "3"   '  partially received
                        sMyOrderLineStatus = "RCP"
                    Case Else
                        sMyOrderLineStatus = "RCF"
                End Select

                If Trim(sMyOrderLineStatus) <> "" Then

                    Dim strSqlStrng As String = ""
                    strSqlStrng = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET QTY_RECEIVED = " & decMyQtyRecv.ToString() & ", ISA_LINE_STATUS = '" & sMyOrderLineStatus & "'" & vbCrLf & _
                                  "WHERE ORDER_NO = '" & sMyReqid & "' AND ISA_INTFC_LN = " & sMyReqLnNbr & ""
                    Dim intRowsAffctd As Integer = 0
                    Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
                    intRowsAffctd = ORDBAccess.ExecNonQuery(strSqlStrng, myConnect)

                End If  '  Trim(sMyOrderLineStatus) <> ""
            End If  '  Trim(sMyReqid) <> "" And Trim(sMyReqLnNbr) <> ""
        Catch ex As Exception

        End Try

    End Sub

    Private Function GetVendorId(ByVal strOrderNum As String) As String
        Dim strVendorIdByOrder As String = ""
        Dim strSQLString As String = ""
        Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)

        strSQLString = "select VENDOR_ID from SYSADM8.PS_PO_DISPATCHED where PO_ID = '" & strOrderNum & "'" & vbCrLf
        Try
            strVendorIdByOrder = ORDBAccess.GetScalar(strSQLString, myConnect)
        Catch ex As Exception
            strVendorIdByOrder = "0000039777"
        End Try

        If Trim(strVendorIdByOrder) = "" Then
            strVendorIdByOrder = "0000039777"
        End If
        Return strVendorIdByOrder
    End Function

    Public Function createReceiver92(ByVal I As Integer, _
                                    ByVal strReceiver As String, _
                                    ByVal intLine As Integer, _
                                    ByVal arrLineParams As ArrayList, _
                                    ByVal strPOERSAction As String, _
                                    ByVal strReceivingType As String, _
                                    ByRef trnsactSession As OleDbTransaction, _
                                    ByRef connection As OleDbConnection, _
                                    Optional ByVal strSerialNums As String = " ", _
                                    Optional ByRef sErrorMessage As String = "2E1") As Boolean

        createReceiver92 = True
        Dim sMyErrorString21 As String = ""

        Dim dteNowd As Date = Now().ToString("d")
        Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
        Dim dteNowy As String = Now().ToString("yyyy-M-d")
        Dim decQty As Decimal = Convert.ToDecimal(arrLineParams(0))
        Dim strTrckNo As String = arrLineParams(1)
        Dim strShipvia As String = arrLineParams(2)
        Dim sMyReqid As String = arrLineParams(26)
        Dim sMyReqLnNbr As String = arrLineParams(27)
        Dim decMyQtyRecv As Decimal = 0

        Dim strShpDt As Date

        strShpDt = Now().ToString("d")

        Dim strShpDttx As String = arrLineParams(3)

        Dim rowsaffected As Integer
        Dim strsql As String
        Dim strCatID As String = " "
        Dim strDescr As String = Replace(Replace(arrLineParams(4), "'", ""), "´", "")
        Dim strInspectCD As String = " "
        Dim strMfgID = Replace(Replace(arrLineParams(5), "'", ""), "´", "")
        Dim strMfgItmID = Replace(Replace(arrLineParams(18), "'", ""), "´", "")
        Dim strBUin As String = " "
        Dim strBUam As String = " "
        Dim strBUgl As String = " "
        Dim strBUpc As String = " "
        Dim strBUpo As String = " "
        Dim strBURItemType As String
        Dim decBURMarkup As Decimal
        Dim decBURPricePO As Decimal
        Dim decBURPrice As Decimal
        Dim bConvertRateApplied As Boolean = False
        Dim strBURbuIN As String
        Dim strDSNetworkCode As String
        Dim strDuedate As String = ""
        Dim strDuedt As Date
        Dim intPoLinenum As Integer = Convert.ToInt32(arrLineParams(6))
        Dim intPOSchedNbr As Integer = Convert.ToInt32(arrLineParams(7))
        Dim decQtyPO As Decimal = 0.0
        Dim decMerchAmtBse As Decimal = 0.0
        Dim decMerchAmtPOBse As Decimal = 0.0
        Dim decMerchandiseAmt As Decimal = 0.0
        Dim decMerchandiseAmtPo As Decimal = 0.0
        Dim decNSTKMarkup As Decimal
        Dim decNSTKPrice As Decimal = 0.0
        Dim strNSTKOrdGrp As String
        Dim strNSTKItemType As String
        Dim intOPSequence As Integer = 0
        Dim decPricePO As Decimal = 0.0
        Dim decPricePOBse As Decimal = 0.0
        Dim strProductionID As String = " "
        Dim intSchedNum As Integer = Convert.ToInt32(arrLineParams(7))
        Dim strShipQtyStatus As String
        Dim strAccount As String = " "
        Dim strAnalysisType As String = " "
        Dim strCapNum As String = " "
        Dim intCapSeq As String = 0
        Dim strChartfieldStatus As String = " "
        Dim strDistribType As String = " "
        Dim strDstAcctType As String = " "
        Dim strEmpID As String = " "
        Dim strFinancialAssetSW As String = " "
        Dim intPODistLineNum As Integer = 0
        Dim strProfileID As String = " "
        Dim strReqID As String = " "
        Dim strResourceCategory As String = " "
        Dim strResourceSubCat As String = " "
        Dim strResourceType As String = " "
        Dim strStatisticsCode As String = " "
        Dim decStatisticAmount As Decimal = 0.0
        Dim decTaxCDSutPct As Decimal = 0.0
        Dim decTaxCDVatPct As Decimal = 0.0
        Dim strDeptID As String = " "
        Dim strProduct As String = " "
        Dim strProjectID As String = " "
        Dim strAffiliate As String = " "
        Dim decCost As Decimal
        Dim strMFGName As String = " "
        Dim strModel As String = " "
        Dim strRoutingID As String = " "

        Dim strRecvStatus As String
        Dim stritemUOM As String
        Dim strpoUOM As String = Replace(Replace(arrLineParams(13), "'", ""), "´", "")
        Dim strConvertRate As Decimal
        Dim strConvertToStk As Decimal
        Dim strConvertToPO As Decimal
        Dim strInvItemID As String = Replace(Replace(arrLineParams(9), "'", ""), "´", "")
        Dim strStockType As String = Replace(Replace(arrLineParams(10), "'", ""), "´", "")
        Dim strPO As String = Replace(Replace(arrLineParams(8), "'", ""), "´", "")
        Dim strPOBU As String = "ISA00"
        
        ' business unit of PO from grid
        Try
            strPOBU = Replace(Replace(CStr(arrLineParams(20)), "'", ""), "´", "")
        Catch ex As Exception
        End Try
        Dim strVendor As String = Replace(Replace(arrLineParams(14), "'", ""), "´", "")
        Dim strShipTo As String = Replace(Replace(arrLineParams(15), "'", ""), "´", "")
        Dim strUserID As String = Replace(Replace(arrLineParams(16), "'", ""), "´", "")
        Dim strTariff As String = Replace(Replace(arrLineParams(17), "'", ""), "´", "")
        Dim strItemWeight As String = Replace(Replace(arrLineParams(19), "'", ""), "´", "")

        Dim decQtyLnAccpt As Decimal

        If arrLineParams(11) = "&nbsp;" Then
            decQtyLnAccpt = 0
        Else
            If IsNumeric(arrLineParams(11)) Then
                decQtyLnAccpt = Convert.ToDecimal(arrLineParams(11))
            Else
                decQtyLnAccpt = 0
            End If

        End If

        'Getting To_Currency value based on Business Unit
        Dim strqryToCurr As String = ""
        Dim strdistBaseCurrency As String = ""

        strqryToCurr = "SELECT CURRENCY_CD" & vbCrLf & _
                " FROM ps_bus_unit_tbl_pm" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & strPOBU & "'"
        Dim drBaseCurrency As OleDbDataReader = ORDBAccess.GetReaderTrans(strqryToCurr, connection, trnsactSession)  '   ORDBData.GetReader(strqryToCurr)
        Dim m_weblogstring1 As String = ""  '  CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
        If m_weblogstring1 = "true" Then
            'WebLogOpenConn()
        End If
        If drBaseCurrency.HasRows() = True Then
            If drBaseCurrency.Read() Then
                strdistBaseCurrency = drBaseCurrency.Item("CURRENCY_CD")
            Else
                strdistBaseCurrency = ""
            End If
        Else
            strdistBaseCurrency = ""
        End If
        drBaseCurrency.Close()
        If m_weblogstring1 = "true" Then
            'WebLogCloseConn()
        End If

        'Getting From_Currency value based on Business Unit
        Dim strdistCurrency As String
        Dim strqryFromCurr As String
        strqryFromCurr = "SELECT CURRENCY_CD" & vbCrLf & _
                    " FROM SYSADM8.PS_PO_LINE_SHIP " & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                    " AND PO_ID = '" & strPO & "'" & vbCrLf & _
                    " AND rownum = 1"
        Dim drCurrency As OleDbDataReader = ORDBAccess.GetReaderTrans(strqryFromCurr, connection, trnsactSession)  '   ORDBData.GetReader(strqryFromCurr)
        If m_weblogstring1 = "true" Then
            'WebLogOpenConn()
        End If
        If drCurrency.HasRows() = True Then
            If drCurrency.Read() Then
                strdistCurrency = drCurrency.Item("CURRENCY_CD")
            Else
                strdistCurrency = ""
            End If
        Else
            strdistCurrency = ""
        End If
        drCurrency.Close()
        If m_weblogstring1 = "true" Then
            'WebLogCloseConn()
        End If

        ' currency codes
        '   - erwin 2010.03.02
        Dim shipCurrency As String = "USD"
        Dim shipBaseCurrency As String = "USD"

        Dim distCurrency As String = strdistCurrency
        Dim distBaseCurrency As String = strdistBaseCurrency
        Dim currency As String = ""
        ' ship currency
        Try
            currency = CStr(arrLineParams(22)).Trim.ToUpper
            If (currency Is Nothing) Then
                currency = ""
            Else
                ' check for HTML space code
                If currency.IndexOf("NBSP") > -1 Then
                    currency = ""
                End If
            End If
            If currency.Length > 0 Then
                shipCurrency = currency
            End If
        Catch ex As Exception
        End Try
        ' ship base currency
        Try
            currency = CStr(arrLineParams(23)).Trim.ToUpper
            If (currency Is Nothing) Then
                currency = ""
            Else
                ' check for HTML space code
                If currency.IndexOf("NBSP") > -1 Then
                    currency = ""
                End If
            End If
            If currency.Length > 0 Then
                shipBaseCurrency = currency
            End If
        Catch ex As Exception
        End Try

        strRecvStatus = strReceivingType
        If (decQty + decQtyLnAccpt) = _
            (Convert.ToDecimal(arrLineParams(12))) Then
            'strRecvStatus = "R"
            strShipQtyStatus = "1"
        Else
            'strRecvStatus = "R"
            If (decQty + decQtyLnAccpt) < _
            (Convert.ToDecimal(arrLineParams(12))) Then
                strShipQtyStatus = "3"
            Else
                strShipQtyStatus = "2"
            End If
        End If
        decMyQtyRecv = decQty + decQtyLnAccpt

        strsql = "SELECT B.UNIT_MEASURE_STD" & vbCrLf & _
                " FROM PS_PURCH_ITEM_ATTR A, PS_MASTER_ITEM_TBL B" & vbCrLf & _
                " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                " AND A.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                " AND A.SETID = B.SETID" & vbCrLf & _
                " AND A.INV_ITEM_ID = B.INV_ITEM_ID"

        Dim dtrPURCHITEMReader As OleDbDataReader = ORDBAccess.GetReaderTrans(strsql, connection, trnsactSession)  '   ORDBData.GetReader(strsql)
        ' Log whenever connection is established with the DB: Vijay - 2/15/2013
        Dim m_weblogstring As String = ""  '  CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
        If m_weblogstring = "true" Then
            'WebLogOpenConn()
        End If
        If (dtrPURCHITEMReader.Read()) Then
            stritemUOM = dtrPURCHITEMReader.Item("UNIT_MEASURE_STD")
        Else
            stritemUOM = strpoUOM
        End If
        dtrPURCHITEMReader.Close()
        'Log whenever connection is closed with the DB: Vijay - 2/15/2013
        If m_weblogstring = "true" Then
            'WebLogCloseConn()
        End If

        If stritemUOM <> strpoUOM Then
            strsql = "SELECT CONVERSION_RATE" & vbCrLf & _
                " FROM PS_ITM_VNDR_UOM" & vbCrLf & _
                " WHERE SETID = 'MAIN1'" & vbCrLf & _
                " AND INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                " AND VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                " AND VENDOR_ID = '" & strVendor & "'" & vbCrLf & _
                " AND UNIT_OF_MEASURE = '" & strpoUOM & "'"

            Dim dtrITMVNDRReader As OleDbDataReader = ORDBAccess.GetReaderTrans(strsql, connection, trnsactSession)  '    ORDBData.GetReader(strsql)
            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
            If m_weblogstring = "true" Then
                'WebLogOpenConn()
            End If
            If (dtrITMVNDRReader.Read()) Then
                strConvertRate = dtrITMVNDRReader.Item("CONVERSION_RATE")
            Else
                strConvertRate = 0
            End If
            dtrITMVNDRReader.Close()
            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
            If m_weblogstring = "true" Then
                'WebLogCloseConn()
            End If
            If strConvertRate = 0 Then
                strsql = "SELECT A.CONVERSION_RATE" & vbCrLf & _
                    " FROM PS_UNITS_CVT_TBL A" & vbCrLf & _
                    " WHERE A.UNIT_OF_MEASURE = '" & strpoUOM & "'" & vbCrLf & _
                    " AND A.UNIT_OF_MEASURE_TO = '" & stritemUOM & "'"
                Dim dtrUNITSCVTReader As OleDbDataReader = ORDBAccess.GetReaderTrans(strsql, connection, trnsactSession)  '   ORDBData.GetReader(strsql)
                ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                If m_weblogstring = "true" Then
                    'WebLogOpenConn()
                End If
                If (dtrUNITSCVTReader.Read) Then
                    strConvertRate = dtrUNITSCVTReader.Item("CONVERSION_RATE")
                    strConvertToStk = strConvertRate
                    strConvertToPO = 1.0
                    dtrUNITSCVTReader.Close()
                    'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                    If m_weblogstring = "true" Then
                        'WebLogCloseConn()
                    End If
                Else
                    dtrUNITSCVTReader.Close()
                    'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                    If m_weblogstring = "true" Then
                        'WebLogCloseConn()
                    End If
                    strsql = "SELECT A.CONVERSION_RATE" & vbCrLf & _
                    " FROM PS_UNITS_CVT_TBL A" & vbCrLf & _
                    " WHERE A.UNIT_OF_MEASURE = '" & stritemUOM & "'" & vbCrLf & _
                    " AND A.UNIT_OF_MEASURE_TO = '" & strpoUOM & "'"
                    Dim dtrUNITSCVTReader2 As OleDbDataReader = ORDBAccess.GetReaderTrans(strsql, connection, trnsactSession)  '   
                    ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                    If m_weblogstring = "true" Then
                        'WebLogOpenConn()
                    End If
                    If (dtrUNITSCVTReader2.Read) Then
                        strConvertRate = 1 / dtrUNITSCVTReader2.Item("CONVERSION_RATE")
                        strConvertToStk = strConvertRate
                        strConvertToPO = 1.0

                    Else
                        strConvertRate = 1.0
                        strConvertToStk = 1.0
                        strConvertToPO = 1.0
                    End If
                    dtrUNITSCVTReader2.Close()
                    'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                    If m_weblogstring = "true" Then
                        'WebLogCloseConn()
                    End If
                End If
            Else
                strConvertToStk = strConvertRate
                strConvertToPO = 1.0
            End If

        Else
            strConvertRate = 1.0
            strConvertToStk = 1.0
            strConvertToPO = 1.0
        End If

        If strConvertToStk = 0 Then
            strConvertToStk = 1.0
        End If

        Dim sUserId As String = "ASNVEN2"

        strRecvStatus = "R"

        If Not connection.State = ConnectionState.Open Then
            connection.Open()
        End If
        Dim command = New OleDbCommand(strsql, connection)
        command.Transaction = trnsactSession

        If strTrckNo = "" Then
            strTrckNo = " "
        End If

        strsql = "INSERT INTO sysadm8.PS_ISA_RECV_LN_ASN" & vbCrLf & _
        " (BUSINESS_UNIT, RECEIVER_ID," & vbCrLf & _
        " RECV_LN_NBR, ISA_ASN_SHIP_DT," & vbCrLf & _
        " ISA_ASN_TRACK_NO, ISA_ASN_SHIP_VIA," & vbCrLf & _
        " LASTUPDDTTM)" & vbCrLf & _
        " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
        " TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf & _
        " '" & strTrckNo.ToUpper & "'," & vbCrLf & _
        " '" & strShipvia & "'," & vbCrLf & _
        " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'))"

        command = New OleDbCommand(strsql, connection)
        command.Transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            createReceiver92 = False
            sMyErrorString21 = "subroutine 'createReceiver92', PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
            sMyErrorString21 &= vbCrLf & " SQL String: " & strsql

            Return False
            Exit Function
        End If

        Call UpdateIntfcLTblStatusQtyRecv(sMyReqid, sMyReqLnNbr, strShipQtyStatus, decMyQtyRecv)

        createReceiver92 = True

    End Function

    Private Function initializeParamsArrForDataRow(ByVal dtgPO As DataRow, _
                                        ByVal strPO As String, _
                                        ByVal strVendor As String, _
                                        ByVal strShiptoID As String, _
                                        ByVal strTrackingNo As String, _
                                        ByVal strCarrierName As String, _
                                        ByVal strPoQty As String, _
                                        ByVal strShipDate As String, _
                                        ByVal strUserid As String, Optional ByVal strTariffCode As String = "", _
                                        Optional ByVal strItemWeight As String = "") As ArrayList

        Try
            Dim arrdtgline As New ArrayList

            'SELECT A.ERS_ACTION, A.VNDR_LOC," & vbCrLf & _
            '" TO_CHAR(A.PO_DT,'MM-DD-YYYY') as PODT, B.LINE_NBR," & vbCrLf & _
            '" A.BUSINESS_UNIT as PO_BU," & vbCrLf & _
            '" C.SCHED_NBR," & vbCrLf & _
            '" B.INV_ITEM_ID," & vbCrLf & _
            '" B.MFG_ITM_ID," & vbCrLf & _
            '" B.ITM_ID_VNDR," & vbCrLf & _
            '" B.DESCR254_MIXED," & vbCrLf & _
            '" C.QTY_PO," & vbCrLf & _
            '" B.UNIT_OF_MEASURE," & vbCrLf & _
            '" D.PRICE_PO, D.SHIPTO_ID," & vbCrLf & _
            '" D.MERCHANDISE_AMT," & vbCrLf & _
            '" B.MFG_ID," & vbCrLf & _
            ' " B.INV_STOCK_TYPE," & vbCrLf & _
            '" ' ' as ISA_ASN_SHIP_DT," & vbCrLf & _
            '" ' ' as ISA_ASN_TRACK_NO," & vbCrLf & _
            '" ' ' as ISA_ASN_SHIP_VIA," & vbCrLf & _
            '" ' ' as ISA_ASN_SHIP_VIA_ID," & vbCrLf & _
            '" D.CURRENCY_CD AS CURRENCY_CD," & vbCrLf & _
            '" D.CURRENCY_CD AS SHIP_CURRENCY," & vbCrLf & _
            '" D.CURRENCY_CD_BASE AS SHIP_BASE_CURRENCY," & vbCrLf & _
            '" C.CURRENCY_CD AS DIST_CURRENCY," & vbCrLf & _
            '" C.CURRENCY_CD_BASE AS DIST_BASE_CURRENCY, " & vbCrLf & _
            '" C.QTY_PO as hQTYPO," & vbCrLf
            '       " G.QTY_LN_ACCPT," & vbCrLf & _
            '" G.QTY_LN_ACCPT as hQTYLNACCPT," & vbCrLf & _
            '" G.BUSINESS_UNIT," & vbCrLf & _
            '" G.RECEIVER_ID," & vbCrLf & _
            '" G.RECV_LN_NBR," & vbCrLf & _
            '" ' ' as ISA_SHIP_ID," & vbCrLf
            '  D.MERCHANDISE_AMT as hMERCHANDISEAMT
            arrdtgline.Insert(0, strPoQty)  '  dtgPO.Item("QTY_PO").Text.ToUpper)  '  Decimal.Parse(txtQTY.Text))
            arrdtgline.Insert(1, strTrackingNo)  '  Trim(CType(item.FindControl("txtTrckno"), TextBox).Text.ToUpper))
            arrdtgline.Insert(2, strCarrierName)  '  CType(item.FindControl("cmbShipvia"), DropDownList).SelectedValue.ToUpper)

            arrdtgline.Insert(3, strShipDate)  '  dtgPO.Item("ISA_ASN_SHIP_DT").Text.ToUpper)
            Try
                arrdtgline.Insert(4, UCase(dtgPO.Item("DESCR254_MIXED")))  '  Trim(CType(item.FindControl("lblDescr254Mixed"), Label).Text.ToUpper)))
            Catch ex As Exception

                arrdtgline.Insert(4, " ")

            End Try
            arrdtgline.Insert(5, UCase(dtgPO.Item("MFG_ID")))
            arrdtgline.Insert(6, UCase(dtgPO.Item("LINE_NBR")))
            arrdtgline.Insert(7, UCase(dtgPO.Item("SCHED_NBR")))
            arrdtgline.Insert(8, strPO)
            arrdtgline.Insert(9, UCase(dtgPO.Item("INV_ITEM_ID")))
            arrdtgline.Insert(10, UCase(dtgPO.Item("INV_STOCK_TYPE")))
            Try
                arrdtgline.Insert(11, UCase(dtgPO.Item("hQTYLNACCPT")))
            Catch ex As Exception
                arrdtgline.Insert(11, "0")
            End Try

            Try
                arrdtgline.Insert(12, UCase(dtgPO.Item("hQtyPO")))
            Catch ex As Exception
                arrdtgline.Insert(12, "0")
            End Try
            arrdtgline.Insert(13, UCase(dtgPO.Item("UNIT_OF_MEASURE")))
            arrdtgline.Insert(14, strVendor)
            arrdtgline.Insert(15, strShiptoID)
            arrdtgline.Insert(16, strUserid)
            'Try
            '    arrdtgline.Insert(17, Trim(CType(item.FindControl("txtTariffCode"), TextBox).Text.ToUpper))
            'Catch ex As Exception
            arrdtgline.Insert(17, strTariffCode)
            'End Try

            arrdtgline.Insert(18, UCase(dtgPO.Item("MFG_ITM_ID")))
            'Try
            '    arrdtgline.Insert(19, Trim(CType(item.FindControl("txtItemWeight"), TextBox).Text.ToUpper))
            'Catch ex As Exception
            arrdtgline.Insert(19, strItemWeight)
            'End Try

            ' PO business unit
            Try
                arrdtgline.Insert(20, UCase(dtgPO.Item("PO_BU")))
            Catch ex As Exception

                arrdtgline.Insert(20, "ISA00")

            End Try

            ' po base currency and currency - default to USD
            arrdtgline.Insert(21, "USD")
            arrdtgline.Insert(22, "USD")
            arrdtgline.Insert(23, "USD")
            arrdtgline.Insert(24, "USD")
            arrdtgline.Insert(25, "USD")
            ' currency_cd
            Dim curr1 As String = ""
            Dim currency As String = ""
            Try
                'currency = dtgPO.Cells(44).Text.Trim.ToUpper
                currency = UCase(dtgPO.Item("CURRENCY_CD"))
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(21) = currency
                End If
            Catch ex As Exception
            End Try
            ' ship currency
            curr1 = ""
            currency = ""
            Try
                'currency = dtgPO.Cells(45).Text.Trim.ToUpper
                currency = UCase(dtgPO.Item("SHIP_CURRENCY"))
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(22) = currency
                End If
            Catch ex As Exception
            End Try
            ' ship base currency
            curr1 = ""
            currency = ""
            Try
                'currency = dtgPO.Cells(46).Text.Trim.ToUpper
                currency = UCase(dtgPO.Item("SHIP_BASE_CURRENCY"))
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(23) = currency
                End If
            Catch ex As Exception
            End Try
            ' distrib currency
            curr1 = ""
            currency = ""
            Try
                'currency = dtgPO.Cells(47).Text.Trim.ToUpper
                currency = UCase(dtgPO.Item("DIST_CURRENCY"))
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(24) = currency
                End If
            Catch ex As Exception
            End Try
            ' distrib base currency
            curr1 = ""
            currency = ""
            Try
                'currency = dtgPO.Cells(48).Text.Trim.ToUpper
                currency = UCase(dtgPO.Item("DIST_BASE_CURRENCY"))
                If currency Is Nothing Then
                    currency = ""
                Else
                    ' check for HTML space code
                    curr1 = UCase(currency)
                    If curr1.IndexOf("NBSP") > -1 Then
                        currency = ""
                    End If
                End If
                If currency.Length > 0 Then
                    arrdtgline(25) = currency
                End If
            Catch ex As Exception
            End Try

            Dim sMyReqid As String = ""
            Try
                sMyReqid = UCase(dtgPO.Item("REQ_ID"))
                If sMyReqid Is Nothing Then
                    sMyReqid = ""
                Else
                    ' check for HTML space code
                    sMyReqid = UCase(sMyReqid)
                    If sMyReqid.IndexOf("NBSP") > -1 Then
                        sMyReqid = ""
                    End If
                End If
            Catch ex As Exception
                sMyReqid = ""
            End Try
            arrdtgline.Insert(26, sMyReqid)

            Dim sMyReqLnNbr As String = ""
            Try
                sMyReqLnNbr = UCase(dtgPO.Item("REQ_LINE_NBR"))
                If sMyReqLnNbr Is Nothing Then
                    sMyReqLnNbr = ""
                Else
                    ' check for HTML space code
                    sMyReqLnNbr = UCase(sMyReqLnNbr)
                    If sMyReqLnNbr.IndexOf("NBSP") > -1 Then
                        sMyReqLnNbr = ""
                    End If
                End If
            Catch ex As Exception
                sMyReqLnNbr = ""
            End Try
            arrdtgline.Insert(27, sMyReqLnNbr)

            Return arrdtgline
        Catch ex As Exception
        End Try
    End Function

    Private Function createReceiverHeader(ByVal strNextReceiver As String, _
                                    ByVal strOperid As String, _
                                    ByVal strShiptoID As String, _
                                    ByVal strVendor As String, _
                                    ByVal strPOERSAction As String, _
                                    ByVal strvndrLoc As String, _
                                    ByVal strSiteBu As String, _
                                    ByRef trnsactSession As OleDbTransaction, _
                                    ByRef connection As OleDbConnection, _
                                    Optional ByVal strCustomerShipping As String = " ") As Boolean

        Dim strsql As String
        Dim rowsaffected As Integer
        Dim strERSAction As String
        Dim strERSINVStatus As String
        Dim strDFLTPackslip As String

        If strPOERSAction = "V" Then
            strERSAction = "Y"
            strERSINVStatus = "N"
            strDFLTPackslip = strNextReceiver
        Else
            strERSAction = "N"
            strERSINVStatus = "A"
            strDFLTPackslip = " "
        End If
        Dim dteNowd As Date = Now().ToString("d")
        Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
        ' INTFC_INVENTORY is always set to 'Y' pfd 
        strsql = "INSERT INTO PS_RECV_HDR" & vbCrLf & _
                " (BUSINESS_UNIT,RECEIVER_ID,ASNTRANSID,ASNQUEUEINSTANCE," & vbCrLf & _
                "BILL_OF_LADING, CARRIER_ID, CONTAINER_ID, " & vbCrLf & _
                "COUNTRY_SHIP_FROM,DFLT_PACKSLIP_NO,ERS_ACTION," & vbCrLf & _
                "ERS_INV_STATUS,ERS_OVERRIDE_OPT,HOLD_ASSET," & vbCrLf & _
                "HOLD_INVENTORY,IN_PROCESS_FLG,INTFC_ASSET,INTFC_INVENTORY," & vbCrLf & _
                "INTFC_MG,MATCH_PROCESS_FLG,MATCH_STATUS_RECV," & vbCrLf & _
                "OPRID,ORIGIN,PO_RECEIPT_FLG,PORT_OF_UNLOADING," & vbCrLf & _
                "PROCESS_INSTANCE, RECEIPT_DT, RECV_SOURCE, RECV_STATUS, " & vbCrLf & _
                "SHIPMENT_NO,SHIPTO_ID,VENDOR_ID,VENDOR_SETID,VNDR_LOC," & vbCrLf & _
                "POST_STATUS_RECV,OPRID_MODIFIED_BY,LAST_DTTM_UPDATE)" & vbCrLf & _
                " VALUES ('" & strSiteBu & "','" & strNextReceiver & "'," & vbCrLf & _
                "' ',0,' ',' ',' ','USA'," & vbCrLf & _
                "'" & strDFLTPackslip & "','" & strERSAction & "','" & strERSINVStatus & "',' ','N','N','N','N','Y','N','N','N'," & vbCrLf & _
                "'" & strOperid & "','INB','Y',' ',0," & vbCrLf & _
                "TO_DATE('" & dteNowd & "','MM-DD-YYYY'),'3','R','" & strCustomerShipping & "'," & vbCrLf & _
                "'" & strShiptoID & "','" & strVendor & "'," & vbCrLf & _
                "'MAIN1','" & strvndrLoc & "',' ','ASNOPER'," & vbCrLf & _
                "TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'))"

        Dim command = New OleDbCommand(strsql, connection)
        command.Transaction = trnsactSession
        If Not connection.State = ConnectionState.Open Then
            connection.Open()
        End If
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            createReceiverHeader = False

            Return False
            Exit Function
        End If
        createReceiverHeader = True
    End Function

    Private Function getNextReceiver(ByVal strSiteBu As String) As Long
        Dim connMy1 As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
        Dim strsql As String
        strsql = "Update ps_bus_unit_tbl_pm Set" & vbCrLf & _
                "recv_id_last_used = recv_id_last_used + 1" & vbCrLf & _
                "where business_unit = '" & strSiteBu & "'"

        Dim rowsaffected As Integer = ORDBAccess.ExecNonQuery(strsql, connMy1)
        If rowsaffected = 0 Then
            Return 0
        End If

        strsql = "select recv_id_last_used" & vbCrLf & _
        "from ps_bus_unit_tbl_pm" & vbCrLf & _
        "where business_unit = '" & strSiteBu & "'"

        Try
            getNextReceiver = CType(ORDBAccess.GetScalar(strsql, connMy1), Long)
        Catch objException As Exception
            Return 0
        End Try
    End Function

    Private Sub createXrefComment(ByVal strPoLineNum As String, ByVal strPoId As String, ByVal strPoSiteBu As String, _
                ByVal strSchedLineNum As String, ByVal strOperId As String)
        Try
            Dim connMy2 As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
            Dim strsql As String = ""
            Dim rowsaffected As Integer = 0
            Dim intPoLinenum As Integer = CType(strPoLineNum, Integer)
            Dim intSchedNum As Integer = CType(strSchedLineNum, Integer)
            Dim strProblemCode As String = "SH"
            Dim strDESCR60 As String = "Expeditor - Shipped"

            strsql = "INSERT INTO PS_ISA_XPD_COMMENT" & vbCrLf & _
                " (BUSINESS_UNIT, PO_ID," & vbCrLf & _
                " LINE_NBR, SCHED_NBR, " & vbCrLf & _
                " ISA_PROBLEM_CODE, NOTES_1000," & vbCrLf & _
                " OPRID," & vbCrLf & _
                " DTTM_STAMP ) " & vbCrLf & _
                " VALUES ('" & strPoSiteBu & "', '" & strPoId & "'," & vbCrLf & _
                " " & intPoLinenum & ", " & intSchedNum & "," & vbCrLf & _
                " '" & strProblemCode & "'," & vbCrLf & _
                " '" & strDESCR60 & "'," & vbCrLf & _
                " '" & strOperId.ToUpper & "'" & vbCrLf & _
                ",SYSDATE )" & vbCrLf

            rowsaffected = ORDBAccess.ExecNonQuery(strsql, connMy2)

        Catch ex As Exception

        End Try
    End Sub

    Private Function getpoinfo(ByVal ponum As String, ByVal vendor As String, ByVal strPOSiteBu As String, ByVal strSmallSite As String) As DataSet

        Dim strSQLString As String

        strSQLString = "SELECT A.ERS_ACTION, A.VNDR_LOC," & vbCrLf & _
                    " TO_CHAR(A.PO_DT,'MM-DD-YYYY') as PODT, B.LINE_NBR," & vbCrLf & _
                    " A.BUSINESS_UNIT as PO_BU," & vbCrLf & _
                    " C.SCHED_NBR," & vbCrLf & _
                    " BP.INV_ITEM_ID," & vbCrLf & _
                    " B.MFG_ITM_ID," & vbCrLf & _
                    " B.ITM_ID_VNDR," & vbCrLf & _
                    " B.DESCR254_MIXED," & vbCrLf & _
                    " C.QTY_PO," & vbCrLf & _
                    " B.UNIT_OF_MEASURE," & vbCrLf & _
                    " D.PRICE_PO, D.SHIPTO_ID," & vbCrLf & _
                    " D.MERCHANDISE_AMT," & vbCrLf & _
                    " B.MFG_ID," & vbCrLf & _
                    " BP.INV_STOCK_TYPE," & vbCrLf & _
                    " ' ' as ISA_ASN_SHIP_DT," & vbCrLf & _
                    " ' ' as ISA_ASN_TRACK_NO," & vbCrLf & _
                    " ' ' as ISA_ASN_SHIP_VIA," & vbCrLf & _
                    " ' ' as ISA_ASN_SHIP_VIA_ID," & vbCrLf & _
                    " D.CURRENCY_CD AS CURRENCY_CD," & vbCrLf & _
                    " D.CURRENCY_CD AS SHIP_CURRENCY," & vbCrLf & _
                    " D.CURRENCY_CD_BASE AS SHIP_BASE_CURRENCY," & vbCrLf & _
                    " C.CURRENCY_CD AS DIST_CURRENCY," & vbCrLf & _
                    " C.CURRENCY_CD_BASE AS DIST_BASE_CURRENCY, " & vbCrLf & _
                    " C.QTY_PO as hQTYPO," & vbCrLf

        strSQLString = strSQLString & " (SELECT SUM(D1.QTY_SH_NETRCV_VUOM) FROM " & vbCrLf & _
        " sysadm8.PS_RECV_LN_SHIP D1" & vbCrLf & _
        " WHERE " & vbCrLf & _
        " D.BUSINESS_UNIT = D1.BUSINESS_UNIT_PO AND " & vbCrLf & _
        " D.PO_ID = D1.PO_ID AND " & vbCrLf & _
        " D.LINE_NBR = D1.LINE_NBR AND " & vbCrLf & _
        " D.SCHED_NBR = D1.SCHED_NBR AND " & vbCrLf & _
        " D1.RECV_SHIP_STATUS <> 'X' " & vbCrLf & _
        " ) as QTY_LN_ACCPT," & vbCrLf & _
        " (SELECT SUM(D1.QTY_SH_NETRCV_VUOM) FROM " & vbCrLf & _
        " sysadm8.PS_RECV_LN_SHIP D1 " & vbCrLf & _
        " WHERE " & vbCrLf & _
        " D.BUSINESS_UNIT = D1.BUSINESS_UNIT_PO AND " & vbCrLf & _
        " D.PO_ID = D1.PO_ID AND " & vbCrLf & _
        " D.LINE_NBR = D1.LINE_NBR AND " & vbCrLf & _
        " D.SCHED_NBR = D1.SCHED_NBR AND " & vbCrLf & _
        " D1.RECV_SHIP_STATUS <> 'X'" & vbCrLf & _
        " ) as hQTYLNACCPT," & vbCrLf & _
        " ' ' as BUSINESS_UNIT," & vbCrLf & _
        " ' ' as RECEIVER_ID," & vbCrLf & _
        " ' ' as RECV_LN_NBR," & vbCrLf & _
        " G.ISA_SHIP_ID," & vbCrLf

        strSQLString = strSQLString & " D.MERCHANDISE_AMT as hMERCHANDISEAMT" & vbCrLf & _
                    " FROM PS_PO_HDR A," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB C," & vbCrLf & _
                    " PS_PO_LINE_SHIP D," & vbCrLf & _
                    " PS_PO_LINE B, SYSADM8.PS_ISA_PO_LINE BP, " & vbCrLf

        If strSmallSite = "Y" Then
            strSQLString = strSQLString & " PS_ISA_ASN_SHIPPED G" & vbCrLf
        Else
            strSQLString = strSQLString & " PS_RECV_LN G" & vbCrLf
        End If

        'Yury 20170228 Ticket 111816 Added C.BUSINESS_UNIT = G.BUSINESS_UNIT (+) comparison to WHERE
        'fix for missing PS_ISA_PO_LINE data (BP) - added outer joins (+)
        strSQLString = strSQLString & " WHERE A.BUSINESS_UNIT = '" & strPOSiteBu & "'" & vbCrLf & _
                    " AND A.PO_ID = '" & ponum & "'" & vbCrLf & _
                    " AND A.VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.VENDOR_ID = '" & vendor & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf & _
                    " AND B.PO_ID = C.PO_ID" & vbCrLf & _
                    " AND B.CANCEL_STATUS != 'X'" & vbCrLf & _
                    " AND B.LINE_NBR = C.LINE_NBR" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.PO_ID = D.PO_ID" & vbCrLf & _
                    " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf & _
                    " AND C.SCHED_NBR = D.SCHED_NBR" & vbCrLf & _
                    " AND D.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = G.BUSINESS_UNIT (+)" & vbCrLf & _
                    " AND D.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND D.LINE_NBR = B.LINE_NBR" & vbCrLf & _
                    " AND C.PO_ID = G.PO_ID(+)" & vbCrLf & _
                    " AND B.BUSINESS_UNIT = BP.BUSINESS_UNIT (+)" & vbCrLf & _
                    " AND B.PO_ID = BP.PO_ID (+)" & vbCrLf & _
                    " AND B.LINE_NBR = BP.LINE_NBR (+)" & vbCrLf & _
                    " AND C.LINE_NBR = G.LINE_NBR(+)" & vbCrLf
        If Not strSmallSite = "Y" Then
            strSQLString = strSQLString & _
                    " AND  C.QTY_PO <> G.QTY_LN_REJCT(+)" & vbCrLf & _
                    " AND  'X' <> G.RECV_LN_STATUS(+)" & vbCrLf
        End If
        strSQLString = strSQLString & _
                    " ORDER BY B.LINE_NBR, C.SCHED_NBR"

        Dim POdataSet As DataSet = New DataSet()
        Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
        Try
            POdataSet = ORDBAccess.GetAdapter(strSQLString, myConnect)

            Return POdataSet
        Catch objException As Exception
            Return Nothing
        End Try


    End Function

    Private Function getshipto(ByVal ponum As String, ByVal vendor As String) As String
        Dim strSQLString As String = ""
        Dim strBUs As String = "ISA00"
        Dim strShipToId As String = " "

        strSQLString = "SELECT A.SHIPTO_ID," & vbCrLf & _
                " B.DESCR, B.ADDRESS1, B.ADDRESS2, B.ADDRESS3, B.ADDRESS4," & vbCrLf & _
                " B.CITY, B.STATE, B.POSTAL" & vbCrLf & _
                " FROM PS_PO_LINE_SHIP A, PS_LOCATION_TBL B, PS_PO_HDR C" & vbCrLf & _
                " WHERE C.BUSINESS_UNIT IN ('" & strBUs & "')" & vbCrLf & _
                " AND C.PO_ID = '" & ponum & "'" & vbCrLf & _
                " AND C.VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                " AND C.VENDOR_ID = '" & vendor & "'" & vbCrLf & _
                " AND C.BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf & _
                " AND C.PO_ID = A.PO_ID" & vbCrLf & _
                " AND B.EFFDT =" & vbCrLf & _
                " (SELECT MAX(B_ED.EFFDT) FROM PS_LOCATION_TBL B_ED" & vbCrLf & _
                " WHERE(B.SETID = B_ED.SETID)" & vbCrLf & _
                " AND B.LOCATION = B_ED.LOCATION" & vbCrLf & _
                " AND B_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                " AND A.SHIPTO_SETID = B.SETID" & vbCrLf & _
                " AND A.SHIPTO_ID = B.LOCATION" & vbCrLf & _
                " AND B.EFF_STATUS = 'A'" & vbCrLf & _
                " and ROWNUM=1" & vbCrLf
        Try
            Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
            Dim dtrPOds As DataSet = ORDBAccess.GetAdapter(strSQLString, myConnect)
            If Not dtrPOds Is Nothing Then
                If dtrPOds.Tables.Count > 0 Then
                    If dtrPOds.Tables(0).Rows.Count > 0 Then
                        strShipToId = dtrPOds.Tables(0).Rows(0).Item("SHIPTO_ID")

                    End If
                End If
            End If
        Catch objException As Exception
            strShipToId = " "
        End Try

        If Trim(strShipToId) = "" Then
            strShipToId = " "
        End If

        Return strShipToId

    End Function

    Private Sub createASNShipped(ByVal strPoLineNum As String, ByVal strPoId As String, ByVal strLineQty As String, _
                ByVal strPoSiteBu As String, ByVal strSchedLineNum As String, ByVal strOperId As String, ByVal strOrderDate As String, _
                ByVal strpoUOM As String, ByVal strTrckNo As String, ByVal strShipvia As String)
        Try
            Dim connMy2 As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
            Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
            'Dim item As GridDataItem = dtgPO.Items(I)
            'Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
            'Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
            'Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            'Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)

            Dim strsql As String = ""
            Dim rowsaffected As Integer = 0
            Dim intPoLinenum As Integer = CType(strPoLineNum, Integer)
            Dim intSchedNum As Integer = CType(strSchedLineNum, Integer)
            'Dim strpoUOM As String = "LBS"   '  item("UNIT_OF_MEASURE").Text  '  UOM
            Dim decQty As Decimal = CType(strLineQty, Decimal)
            'Dim strTrckNo As String = "00989"  '  CType(txtTrckno, TextBox).Text.ToUpper  '  ShipControl/ShipmentIdentifier
            'Dim strShipvia As String = "Test 2nd "  '  CType(cmbShipvia, DropDownList).SelectedValue  '  ShipControl/CarrierIdentifier
            Dim strShpDt As Date = strOrderDate  '  CType(dpDeliveredDate, RadDatePicker).SelectedDate  ' OrderDate
            Dim strShpDttx As String = strOrderDate  '  CType(dpDeliveredDate, RadDatePicker).SelectedDate  ' OrderDate
            'If Not CType(txtCommon, TextBox).Text = " " And _
            '    Not CType(txtCommon, TextBox).Text = "" Then
            '    strShipvia = strShipvia & "#" & Replace(CType(txtCommon, TextBox).Text.ToUpper, "'", "''")
            'End If
            If strTrckNo = "" Then
                strTrckNo = " "
            End If

            strsql = "SELECT A.PO_ID, A.ISA_SHIP_ID" & vbCrLf & _
                    " FROM PS_ISA_ASN_SHIPPED A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                    " AND A.PO_ID = '" & strPoId & "'" & vbCrLf & _
                    " AND A.LINE_NBR = " & intPoLinenum & vbCrLf & _
                    " AND A.SCHED_NBR = " & intSchedNum & vbCrLf & _
                    " AND A.QTY_LN_ACCPT_VUOM = 0" & vbCrLf

            Dim objReader As OleDbDataReader = ORDBAccess.GetReader(strsql, connMy2)

            If objReader.Read() Then
                strsql = "UPDATE PS_ISA_ASN_SHIPPED" & vbCrLf & _
                        " SET QTY_LN_ACCPT_VUOM = " & decQty & "," & vbCrLf & _
                        " ISA_ASN_TRACK_NO = '" & strTrckNo.ToUpper & "'," & vbCrLf & _
                        " ISA_ASN_SHIP_VIA = '" & strShipvia & "'," & vbCrLf & _
                        " ISA_ASN_SHIP_DT = TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf & _
                        " OPRID = '" & strOperId & "'," & vbCrLf & _
                        " OPRID_MODIFIED_BY = 'ASNOPER'," & vbCrLf & _
                        " DATETIME_ADDED = TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS')" & vbCrLf & _
                        " WHERE BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                        " AND PO_ID = '" & strPoId & "'" & vbCrLf & _
                        " AND LINE_NBR = " & intPoLinenum & vbCrLf & _
                        " AND SCHED_NBR = " & intSchedNum & vbCrLf & _
                        " AND QTY_LN_ACCPT_VUOM = 0" & vbCrLf
            Else
                strsql = "INSERT INTO PS_ISA_ASN_SHIPPED" & vbCrLf & _
                    " (BUSINESS_UNIT, PO_ID," & vbCrLf & _
                    " LINE_NBR, SCHED_NBR, " & vbCrLf & _
                    " ISA_SHIP_ID, QTY_LN_ACCPT_VUOM," & vbCrLf & _
                    " UNIT_OF_MEASURE," & vbCrLf & _
                    " ISA_ASN_TRACK_NO," & vbCrLf & _
                    " ISA_ASN_SHIP_VIA," & vbCrLf & _
                    " ISA_ASN_SHIP_DT," & vbCrLf & _
                    " OPRID, OPRID_MODIFIED_BY," & vbCrLf & _
                    " DATETIME_ADDED)" & vbCrLf & _
                    " VALUES ('" & strPoSiteBu & "', '" & strPoId & "'," & vbCrLf & _
                    " " & intPoLinenum & ", " & intSchedNum & "," & vbCrLf & _
                    " SEQ_ISA_SHIP_ID_PK.NEXTVAL," & decQty & "," & vbCrLf & _
                    " '" & strpoUOM & "'," & vbCrLf & _
                    " '" & strTrckNo.ToUpper & "'," & vbCrLf & _
                    " '" & strShipvia & "'," & vbCrLf & _
                    " TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf & _
                    "'" & strOperId & "','ASNOPER'," & vbCrLf & _
                    " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'))"
            End If

            rowsaffected = ORDBAccess.ExecNonQuery(strsql, connMy2)

            objReader.Close()

        Catch ex As Exception

        End Try
    End Sub

    Private Function getSmallSiteFlag(ByVal strpo As String, ByVal strPoSiteBu As String) As String

        Dim connMy2 As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
        Dim strSQLstring As String = ""

        strSQLstring = "SELECT B.ISA_SMALLSITE_FLAG" & vbCrLf & _
                        " FROM PS_PO_LINE_SHIP A," & vbCrLf & _
                        " PS_ISA_SDR_BU_LOC B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                        " AND A.PO_ID = '" & strpo & "'" & vbCrLf & _
                        " AND A.SHIPTO_SETID = B.SETID" & vbCrLf & _
                        " AND A.SHIPTO_ID = B.LOCATION" & vbCrLf & _
                        " AND  rownum = '1'" & vbCrLf

        getSmallSiteFlag = ORDBAccess.GetScalar(strSQLstring, connMy2)

    End Function

    Private Function getASNFlag(ByVal strpo As String, ByVal strPoSiteBu As String) As String

        Dim connMy2 As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
        Dim strSQLstring As String

        'strSQLstring = "SELECT B.ISA_ASN_SITE" & vbCrLf & _
        '                " FROM PS_PO_LINE_SHIP A," & vbCrLf & _
        '                " PS_ISA_SDR_BU_LOC B" & vbCrLf & _
        '                " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
        '                " AND A.PO_ID = '" & strpo & "'" & vbCrLf & _
        '                " AND A.SHIPTO_SETID = B.SETID" & vbCrLf & _
        '                " AND A.SHIPTO_ID = B.LOCATION" & vbCrLf & _
        '                " AND  rownum = '1'" & vbCrLf

        strSQLstring = "SELECT B.ISA_ASN_RECEIPT As ISA_ASN_SITE" & vbCrLf & _
                        " FROM PS_PO_LINE_SHIP A," & vbCrLf & _
                        " PS_ISA_ENTERPRISE B," & vbCrLf & _
                        " PS_PO_LINE_DISTRIB D " & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                        " AND A.PO_ID = '" & strpo & "'" & vbCrLf & _
                        " AND A.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                        " AND D.PO_ID = A.PO_ID" & vbCrLf & _
                        " AND A.SHIPTO_SETID = B.SETID" & vbCrLf & _
                        " AND D.DEPTID = B.DEPTID"

        getASNFlag = ORDBAccess.GetScalar(strSQLstring, connMy2)

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "AmazonShipNotice.SendEmail"

        'The email address of the recipient. 
        Dim strEmailTo As String = ""
        strEmailTo = "vitaly.rovensky@sdi.com"
        If bIsSendOut Then
            If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                strEmailTo = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
            End If
        End If

        Dim strEmailCc As String = " "

        Dim strEmailBcc As String = "webdev@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_BCC")) <> "") Then
            strEmailBcc = CStr(My.Settings(propertyName:="onErrorEmail_BCC")).Trim
        End If

        Dim strEmailSubject As String = ""
        strEmailSubject = " (TEST) Amazon SDI Direct process Ship Notice Error(s)"

        Dim strEmailBody As String = ""
        strEmailBody &= "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >Amazon SDI Direct process Ship Notice Error(s)</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>Amazon SDI Direct process Ship Notice has completed with error(s)"
        ''If bolWarning = True Then
        ''    email.Body &= "warnings."
        ''Else
        ''    email.Body &= "errors."
        ''End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Try

            sInfoErr &= " XML file name(s) are below. Please review Logs.</td></tr>"
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
            strEmailBody &= sInfoErr
        Catch ex As Exception

            strEmailBody &= " Please review Logs.</td></tr>"
        End Try

        strEmailBody &= "</table>"

        strEmailBody &= "&nbsp;<br>Sincerely,<br>&nbsp;<br>SDI Customer Care<br>&nbsp;<br></p></div><BR>"
        strEmailBody &= "<br />"


        Dim sApp As String = "" & _
                             System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).Name & _
                             " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                            ""
        Try
            strEmailBody &= "" & _
                          "<p style=""text-align:right;font-size:10px"">" & _
                          sApp & _
                          "</p>" & _
                          ""
        Catch ex As Exception
        End Try

        strEmailBody &= "" & _
                    "<HR width='100%' SIZE='1'>" & _
                    "<img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' />"
        strEmailBody &= "<br><P><CENTER><SPAN style='FONT-SIZE: 12pt'><SPAN style='FONT-SIZE: 12pt'><FONT color=teal size=2>The information in this communication, including any attachments, is the property of SDI, Inc,&nbsp;</SPAN>is intended only for the addressee and may contain confidential, proprietary, and/or privileged material. Any review, retransmission, dissemination or other use of, or taking of any action in reliance upon, this information by persons or entities other than the intended recipient is prohibited. If you received this in error, please immediately contact the sender by replying to this email and delete the material from all computers.</FONT></SPAN></CENTER></P>"
        strEmailBody &= "</body></html>"

        ''Try
        ''    email.Attachments.Add(New System.Web.Mail.MailAttachment(filename:=sErrLogPath))
        ''Catch ex As Exception
        ''End Try

        Dim int1 As Integer = 0

        Try
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles() As String = Split(m_arrXMLErrFiles, ",")
                    If arrErrFiles.Length > 0 Then
                        m_logger.WriteInformationLog(rtn & " :: erroneous xml file count = " & arrErrFiles.Length.ToString)
                        'For int1 = 0 To arrErrFiles.Length - 1
                        '    Dim myFileName2 As String = "C:\CytecMxmIn\BadXML\PurchReqs\" & arrErrFiles(int1)
                        '    'email.Attachments.Add(New System.Web.Mail.MailAttachment(myFileName2))
                        'Next
                    End If
                End If
            End If
        Catch ex As Exception

        End Try

        Dim bSend As Boolean = False
        Try

            SendLogger(strEmailSubject, strEmailBody, "AMAZONSHIPNOTCEIN", "Mail", strEmailTo, strEmailCc, strEmailBcc)
            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
        Catch ex As Exception

        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
