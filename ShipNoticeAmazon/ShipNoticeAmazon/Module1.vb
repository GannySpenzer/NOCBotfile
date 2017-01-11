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
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")
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
                                    Dim nodeConfReqst As XmlNode = nodeOrdConf.FirstChild()
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
                                                Dim nodeShipNoticeHeadr As XmlNode = nodeConfReqst.ChildNodes(iCnt)
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
                                                Dim nodeShipControlMy As XmlNode = nodeConfReqst.ChildNodes(iCnt)
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
                                                Dim nodeShipNoticeMy As XmlNode = nodeConfReqst.ChildNodes(iCnt)
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

                                            'get Vendor ID based on strOrderNum
                                            strVendorId = GetVendorId(strOrderNum)

                                            strSmallSite = getSmallSiteFlag(strOrderNum, strSiteBu)
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
                                                            Dim iMyCnt As Integer = 0
                                                            Dim iLoc1 As Integer = -1
                                                            Dim strErsAction As String = POdataSet.Tables(0).Rows(0).Item("ERS_ACTION")
                                                            Dim strVendLoc As String = POdataSet.Tables(0).Rows(0).Item("VNDR_LOC")
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
                                                                        ' use clsReceiver from SDiExchange
                                                                        intLines = intLines + 1
                                                                        Dim truth As Boolean = True
                                                                        If intLines = 1 Then
                                                                            'get next receiver number
                                                                            intNextReceiver = getNextReceiver(strSiteBu) + 10000000000
                                                                            strNextReceiver = Right(CType(intNextReceiver, String), 10)
                                                                            'create header  
                                                                            If Trim(strVendLoc) = "" Then
                                                                                strVendLoc = "1"
                                                                            End If
                                                                            truth = createReceiverHeader(strNextReceiver, strUserIdN1, strShipToId, strVendorId, strErsAction, strVendLoc, strSiteBu, trnsactSession, connectOR, strShipmntId)
                                                                            If Not truth Then
                                                                                strXMLError = "Error creating Receiver Header for Order No: " & strOrderNum

                                                                                trnsactSession.Rollback()
                                                                                trnsactSession = Nothing
                                                                                connectOR.Close()

                                                                            End If

                                                                        End If  '  If intLines = 1 Then

                                                                        If truth Then
                                                                            'process line info
                                                                            Dim dataRow1 As DataRow = POdataSet.Tables(0).Rows(iMyCnt)
                                                                            ' get Ship To ID for the current line
                                                                            Dim strShiptoIdLine As String = ""
                                                                            Try
                                                                                strShiptoIdLine = POdataSet.Tables(0).Rows(iMyCnt).Item("SHIPTO_ID")
                                                                            Catch ex As Exception
                                                                                strShiptoIdLine = strShipToId
                                                                            End Try

                                                                            Dim strPoQty As String = ""
                                                                            If iLoc1 > -1 Then
                                                                                strPoQty = arrLineQtys(iLoc1)
                                                                            Else
                                                                                strPoQty = POdataSet.Tables(0).Rows(iMyCnt).Item("QTY_PO")
                                                                            End If

                                                                            arrParamdtgLine = initializeParamsArrForDataRow(dataRow1, strOrderNum, strVendorId, strShiptoIdLine, strTrackngNumber, strCarrierName, strPoQty, strShipmntDate, strUserIdN1, strTariffCode, strItemWeight)

                                                                            Dim strMy21Error As String = " "

                                                                            Try

                                                                                truth = createReceiver(I, strNextReceiver, intLines, arrParamdtgLine, strErsAction, "R", trnsactSession, connectOR, " ", strMy21Error)

                                                                                If Not truth Then
                                                                                    strXMLError = " Error in 'createReceiver' (Not truth).Tech. info: " & strMy21Error

                                                                                    trnsactSession.Rollback()
                                                                                    trnsactSession = Nothing
                                                                                    connectOR.Close()
                                                                                End If
                                                                            Catch ex As Exception
                                                                                strXMLError = " Error setting up Receiver Record ('createReceiver' area). Tech. info: " & strMy21Error & _
                                                                                    "Error: " & ex.Message

                                                                                trnsactSession.Rollback()
                                                                                trnsactSession = Nothing
                                                                                connectOR.Close()
                                                                            End Try
                                                                        End If  '  If truth Then

                                                                    End If  '  based on strSmallSite And strASNSite

                                                                End If  '   If arrLineNums.Contains(iMyCnt.ToString()) Then

                                                            Next  '  For iMyCnt = 0 To POdataSet.Tables(0).Rows.Count - 1

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

    Private Function GetVendorId(ByVal strOrderNum As String) As String
        Dim strVendorIdByOrder As String = ""
        Dim strSQLString As String = ""
        Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)

        strSQLString = "select VENDOR_ID from SYSADM.PS_PO_DISPATCHED where PO_ID = '" & strOrderNum & "'" & vbCrLf
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

    Private Function createReceiver(ByVal I As Integer, _
                                            ByVal strReceiver As String, _
                                            ByVal intLine As Integer, _
                                            ByVal arrLineParams As ArrayList, _
                                            ByVal strPOERSAction As String, _
                                            ByVal strReceivingType As String, _
                                            ByRef trnsactSession As OleDbTransaction, _
                                            ByRef connection As OleDbConnection, _
                                            Optional ByVal strSerialNums As String = " ", _
                                            Optional ByRef sErrorMessage As String = "2E1") As Boolean

        createReceiver = True
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
        If UCase(arrLineParams(3)) = "&NBSP;" Then
            strShpDt = Now().ToString("d")
        Else
            strShpDt = arrLineParams(3)
        End If
        Dim strShpDttx As String = arrLineParams(3)

        Dim rowsaffected As Integer
        Dim strsql As String
        Dim strCatID As String = " "
        Dim strDescr As String = Replace(arrLineParams(4), "'", "")
        Dim strInspectCD As String = " "
        Dim strMfgID = arrLineParams(5)
        Dim strMfgItmID = arrLineParams(18)
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
        Dim strpoUOM As String = arrLineParams(13)
        Dim strConvertRate As Decimal
        Dim strConvertToStk As Decimal
        Dim strConvertToPO As Decimal
        Dim strInvItemID As String = arrLineParams(9)
        Dim strStockType As String = arrLineParams(10)
        Dim strPO As String = arrLineParams(8)
        Dim strPOBU As String = ""
        Dim myConnect As OleDbConnection = New OleDbConnection(ORDBAccess.DbUrl)
       
        Try
            strPOBU = CStr(arrLineParams(20))
        Catch ex As Exception
            strPOBU = "ISA00"
        End Try
        Dim strVendor As String = arrLineParams(14)
        Dim strShipTo As String = arrLineParams(15)
        Dim strUserID As String = arrLineParams(16)
        Dim strTariff As String = arrLineParams(17)
        Dim strItemWeight As String = arrLineParams(19)

        Dim sServer As String
        sServer = "RPT"  '  currentApp.Session("WEBSITEID")

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
        Dim strqryToCurr As String
        Dim strdistBaseCurrency As String

        strqryToCurr = "SELECT CURRENCY_CD" & vbCrLf & _
                " FROM ps_bus_unit_tbl_pm" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & strPOBU & "'"
        Dim drBaseCurrency As OleDbDataReader = ORDBAccess.GetReader(strqryToCurr, myConnect)
        
        If drBaseCurrency.HasRows() = True Then
            If (drBaseCurrency.Read()) Then
                strdistBaseCurrency = drBaseCurrency.Item("CURRENCY_CD")
            Else
                strdistBaseCurrency = ""
            End If
        Else
            strdistBaseCurrency = ""
        End If
        drBaseCurrency.Close()

        'Getting From_Currency value based on Business Unit
        Dim strdistCurrency As String
        Dim strqryFromCurr As String
        strqryFromCurr = "SELECT CURRENCY_CD" & vbCrLf & _
                    " FROM SYSADM.PS_PO_LINE_SHIP " & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                    " AND PO_ID = '" & strPO & "'" & vbCrLf & _
                    " AND rownum = 1"
        Dim drCurrency As OleDbDataReader = ORDBAccess.GetReader(strqryFromCurr, myConnect)
        
        If drCurrency.HasRows() = True Then
            If (drCurrency.Read()) Then
                strdistCurrency = drCurrency.Item("CURRENCY_CD")
            Else
                strdistCurrency = ""
            End If
        Else
            strdistCurrency = ""
        End If
        drCurrency.Close()
        
        Dim shipCurrency As String = "USD"
        Dim shipBaseCurrency As String = "USD"
        'Dim distCurrency As String = "USD"
        'Dim distBaseCurrency As String = "USD"
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
            strShipQtyStatus = "1"
        Else
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

        Dim dtrPURCHITEMReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
       
        If (dtrPURCHITEMReader.Read()) Then
            stritemUOM = dtrPURCHITEMReader.Item("UNIT_MEASURE_STD")
        Else
            stritemUOM = strpoUOM
        End If
        dtrPURCHITEMReader.Close()
        
        If stritemUOM <> strpoUOM Then
            strsql = "SELECT CONVERSION_RATE" & vbCrLf & _
                " FROM PS_ITM_VNDR_UOM" & vbCrLf & _
                " WHERE SETID = 'MAIN1'" & vbCrLf & _
                " AND INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                " AND VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                " AND VENDOR_ID = '" & strVendor & "'" & vbCrLf & _
                " AND UNIT_OF_MEASURE = '" & strpoUOM & "'"

            Dim dtrITMVNDRReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
            
            If (dtrITMVNDRReader.Read()) Then
                strConvertRate = dtrITMVNDRReader.Item("CONVERSION_RATE")
            Else
                strConvertRate = 0
            End If
            dtrITMVNDRReader.Close()
            
            If strConvertRate = 0 Then
                strsql = "SELECT A.CONVERSION_RATE" & vbCrLf & _
                    " FROM PS_UNITS_CVT_TBL A" & vbCrLf & _
                    " WHERE A.UNIT_OF_MEASURE = '" & strpoUOM & "'" & vbCrLf & _
                    " AND A.UNIT_OF_MEASURE_TO = '" & stritemUOM & "'"
                Dim dtrUNITSCVTReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
                
                If (dtrUNITSCVTReader.Read) Then
                    strConvertRate = dtrUNITSCVTReader.Item("CONVERSION_RATE")
                    strConvertToStk = strConvertRate
                    strConvertToPO = 1.0
                    dtrUNITSCVTReader.Close()
                    
                Else
                    dtrUNITSCVTReader.Close()
                    
                    strsql = "SELECT A.CONVERSION_RATE" & vbCrLf & _
                    " FROM PS_UNITS_CVT_TBL A" & vbCrLf & _
                    " WHERE A.UNIT_OF_MEASURE = '" & stritemUOM & "'" & vbCrLf & _
                    " AND A.UNIT_OF_MEASURE_TO = '" & strpoUOM & "'"
                    Dim dtrUNITSCVTReader2 As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
                   
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
        strsql = "SELECT BUSINESS_UNIT_PO, PO_ID, LINE_NBR," & vbCrLf & _
                " SCHED_NBR, BUSINESS_UNIT_IN, CATEGORY_ID, CB_SELECT_PO," & vbCrLf & _
                " CONFIG_CODE, COUNTRY_IST_ORIGIN, CURRENCY_CD," & vbCrLf & _
                " CURRENCY_CD_BASE, DESCR254_MIXED,	DETAILS_PB," & vbCrLf & _
                " DISTRIB_MTHD_FLG, DUE_DT," & vbCrLf & _
                " ERS_ACTION," & vbCrLf & _
                " INSPECT_CD, INV_ITEM_ID, IST_TXN_FLG, ITM_SETID," & vbCrLf & _
                " MANUFACTURER, MERCH_AMT_BSE, MERCH_AMT_PO_BSE," & vbCrLf & _
                " MERCHANDISE_AMT_PO, MFG_ID, MFG_ITM_ID, MODEL," & vbCrLf & _
                " OP_SEQUENCE, TO_CHAR(ORIG_PROM_DT,'YYYY-MM-DD')," & vbCrLf & _
                " TO_CHAR(PO_DT,'YYYY-MM-DD'), PRICE_PO, PRICE_PO_BSE," & vbCrLf & _
                " PRODUCTION_ID, QTY_PO, QTY_PRIOR_RECEIPT," & vbCrLf & _
                " QTY_RECV_TOL_PCT, ROUTING_ID, SHIPTO_ID," & vbCrLf & _
                " SHIPTO_SETID, SHIP_TYPE_ID, UNIT_OF_MEASURE," & vbCrLf & _
                " VENDOR_ID, VENDOR_SETID, VNDR_LOC, PO_DIST_LINE_NUM," & vbCrLf & _
                " ACCOUNT, ACTIVITY_ID, ANALYSIS_TYPE, BUSINESS_UNIT_AM," & vbCrLf & _
                " BUSINESS_UNIT_GL, BUSINESS_UNIT_PC, CAP_NUM," & vbCrLf & _
                " CAP_SEQUENCE, CHARTFIELD_STATUS, DISTRIB_TYPE," & vbCrLf & _
                " DST_ACCT_TYPE, EMPLID, FINANCIAL_ASSET_SW," & vbCrLf & _
                " LOCATION, PROFILE_ID, REQ_ID, RESOURCE_CATEGORY," & vbCrLf & _
                " RESOURCE_SUB_CAT, RESOURCE_TYPE, STATISTIC_AMOUNT," & vbCrLf & _
                " STATISTICS_CODE, TAX_CD_SUT_PCT, TAX_CD_VAT_PCT," & vbCrLf & _
                " DEPTID, PRODUCT, PROJECT_ID, AFFILIATE" & vbCrLf & _
                " FROM PS_ISA_RCVPOINF_VW" & vbCrLf & _
                " WHERE BUSINESS_UNIT_PO='" & strPOBU & "'" & vbCrLf & _
                " AND SHIPTO_ID='" & strShipTo & "'" & vbCrLf & _
                " AND PO_ID='" & strPO & "'" & vbCrLf & _
                " AND LINE_NBR = '" & intPoLinenum & "'" & vbCrLf & _
                " AND SCHED_NBR = '" & intPOSchedNbr & "'"
        '   - erwin 2009.02.18
        '" WHERE BUSINESS_UNIT_PO='" & currentApp.Session("SITEBU") & "'" & vbCrLf & _

        Dim dtrRCVPOINFReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
        
        Dim sUserId As String = ""
        'Try
        '    If currentApp.Session("SDIEMP") = "V" Then
        '        ' from Vendor Portal
        '        sUserId = CType(currentApp.Session("OPERID"), String) & " - from Supplier Portal"
        '    Else
        '        ' from Customer protal
        '        sUserId = CType(currentApp.Session("USERID"), String)
        '    End If
        'Catch ex As Exception
        '    sUserId = "Unknown User (Error while retrieving)"
        'End Try
        Dim strTimeStr As String = Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        If (dtrRCVPOINFReader.Read()) Then
            strCatID = dtrRCVPOINFReader.Item("CATEGORY_ID")
            strInspectCD = dtrRCVPOINFReader.Item("INSPECT_CD")
            strMfgID = Replace(dtrRCVPOINFReader.Item("MFG_ID"), "'", "")
            strMfgItmID = Replace(dtrRCVPOINFReader.Item("MFG_ITM_ID"), "'", "")
            strBUin = dtrRCVPOINFReader.Item("BUSINESS_UNIT_IN")
            strBUam = dtrRCVPOINFReader.Item("BUSINESS_UNIT_AM")
            strBUgl = dtrRCVPOINFReader.Item("BUSINESS_UNIT_GL")
            strBUpc = dtrRCVPOINFReader.Item("BUSINESS_UNIT_PC")
            strBUpo = dtrRCVPOINFReader.Item("BUSINESS_UNIT_PO")
            strDuedt = dtrRCVPOINFReader.Item("DUE_DT")
            strDuedate = strDuedt.ToString("yyyy-M-d")
            decMerchAmtBse = (dtrRCVPOINFReader.Item("PRICE_PO") * decQty)
            'decMerchAmtPOBse = dtrRCVPOINFReader.Item("MERCH_AMT_PO_BSE")
            decMerchAmtPOBse = (dtrRCVPOINFReader.Item("PRICE_PO_BSE") * decQty)
            decMerchandiseAmt = (dtrRCVPOINFReader.Item("PRICE_PO") * decQty)
            decMerchandiseAmtPo = dtrRCVPOINFReader.Item("MERCHANDISE_AMT_PO")
            decQtyPO = dtrRCVPOINFReader.Item("QTY_PO")
            intOPSequence = dtrRCVPOINFReader.Item("OP_SEQUENCE")
            decPricePO = dtrRCVPOINFReader.Item("PRICE_PO")
            decPricePOBse = dtrRCVPOINFReader.Item("PRICE_PO_BSE")
            strProductionID = dtrRCVPOINFReader.Item("PRODUCTION_ID")
            strAccount = dtrRCVPOINFReader.Item("ACCOUNT")
            strAnalysisType = dtrRCVPOINFReader.Item("ANALYSIS_TYPE")
            strCapNum = dtrRCVPOINFReader.Item("CAP_NUM")
            intCapSeq = dtrRCVPOINFReader.Item("CAP_SEQUENCE")
            strChartfieldStatus = dtrRCVPOINFReader.Item("CHARTFIELD_STATUS")
            strDistribType = dtrRCVPOINFReader.Item("DISTRIB_TYPE")
            strDstAcctType = dtrRCVPOINFReader.Item("DST_ACCT_TYPE")
            strEmpID = dtrRCVPOINFReader.Item("EMPLID")
            strFinancialAssetSW = dtrRCVPOINFReader.Item("FINANCIAL_ASSET_SW")
            intPODistLineNum = dtrRCVPOINFReader.Item("PO_DIST_LINE_NUM")
            strProfileID = dtrRCVPOINFReader.Item("PROFILE_ID")
            strReqID = dtrRCVPOINFReader.Item("REQ_ID")
            strResourceCategory = dtrRCVPOINFReader.Item("RESOURCE_CATEGORY")
            strResourceSubCat = dtrRCVPOINFReader.Item("RESOURCE_SUB_CAT")
            strResourceType = dtrRCVPOINFReader.Item("RESOURCE_TYPE")
            strStatisticsCode = dtrRCVPOINFReader.Item("STATISTICS_CODE")
            decStatisticAmount = dtrRCVPOINFReader.Item("STATISTIC_AMOUNT")
            decTaxCDSutPct = dtrRCVPOINFReader.Item("TAX_CD_SUT_PCT")
            decTaxCDVatPct = dtrRCVPOINFReader.Item("TAX_CD_VAT_PCT")
            strDeptID = dtrRCVPOINFReader.Item("DEPTID")
            strProduct = dtrRCVPOINFReader.Item("PRODUCT")
            strProjectID = dtrRCVPOINFReader.Item("PROJECT_ID")
            strAffiliate = dtrRCVPOINFReader.Item("AFFILIATE")
            strModel = dtrRCVPOINFReader.Item("MODEL")
            strRoutingID = dtrRCVPOINFReader.Item("ROUTING_ID")
            decCost = dtrRCVPOINFReader.Item("MERCHANDISE_AMT_PO")
        Else

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 1. "
                sMyErrorString21 = "Place # 1, subroutine 'createReceiver', class 'clsReceiver.vb'. Time: " & strTimeStr & " ; " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer & " ; "
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            dtrRCVPOINFReader.Close()
            createReceiver = False
            'createReceiver = "Error reading PS_ISA_RCVPOINF_VW"
            Return False
            Exit Function
        End If
        '' if CST00 ste the recv_ln_status = 'R'  per Donna/Natashia 09.28.2009
        '' Session("CST00") is used by the hard-coded menu.
        '' Privilege LOANERTOOL is used by the DB-driven menu.
        '' Include a test for both privileges during the switch from the
        '' hard-coded menu to the DB-driven menu.
        'If currentApp.Session("CST00") Or _
        '    clsAccessPrivileges.IsPrivilegeOn(currentApp.Session("USERID"), currentApp.Session("BUSUNIT"), _
        '                                      clsAccessPrivileges.UserPrivsEnum.LoanerTools) Then
        '    strRecvStatus = "R"
        'End If

        ' code for Inspection Process - VR 04/22/2016
        Dim strInspectStatus As String = "C"  ' field INSPECT_STATUS in the table PS_RECV_LN
        Dim strShipDateStatus As String = "1"
        ''test
        'strInspectCD = "Y"
        'strRoutingID = "P02003"
        '' end test
        Select Case strInspectCD
            Case "Y"
                strInspectStatus = "I"
                strShipDateStatus = "2"
            Case Else
                strInspectStatus = "C"
                strShipDateStatus = "1"
        End Select
        'end code for Inspection Process

        strsql = "INSERT INTO PS_RECV_LN" & vbCrLf & _
                " (BUSINESS_UNIT,RECEIVER_ID,RECV_LN_NBR,ASNTRANSID," & vbCrLf & _
                " ASNQUEUEINSTANCE,ASN_SEQ_NBR,ASSET_INV_STATUS," & vbCrLf & _
                " BILL_OF_LADING,BUSINESS_UNIT_PO,CATEGORY_ID," & vbCrLf & _
                " CONFIG_CODE,CONVERSION_RATE,CONVERT_TO_STK," & vbCrLf & _
                " CONVERT_TO_PO,DESCR254_MIXED,INSPECT_CD,INSPECT_DTTM," & vbCrLf & _
                " INSPECT_STATUS,INV_ITEM_ID,ITM_SETID,LINE_NBR," & vbCrLf & _
                " LOT_CONTROL,LOT_STATUS,MFG_ID,OPRID,PO_ID," & vbCrLf & _
                " PROCESS_INSTANCE,QTY_LN_ASSET_SUOM,QTY_LN_ACCPT," & vbCrLf & _
                " QTY_LN_ACCPT_SUOM,QTY_LN_ACCPT_VUOM,QTY_LN_INSPD," & vbCrLf & _
                " QTY_LN_INSPD_SUOM,QTY_LN_INSPD_VUOM,QTY_LN_INV_SUOM," & vbCrLf & _
                " QTY_LN_PKSLP_VUOM,QTY_LN_RECVD,QTY_LN_RECVD_SUOM," & vbCrLf & _
                " QTY_LN_RECVD_VUOM,QTY_LN_REJCT,QTY_LN_REJCT_SUOM," & vbCrLf & _
                " QTY_LN_REJCT_VUOM,RECEIPT_UM,RECEIVE_UOM,RECV_LN_STATUS," & vbCrLf & _
                " RECV_STOCK_UOM,SERIAL_CONTROL,SERIAL_STATUS," & vbCrLf & _
                " UNIT_MEASURE_STD)" & vbCrLf & _
                " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & ",' '," & vbCrLf & _
                " 0,0,'C'," & vbCrLf & _
                " ' ','" & strPOBU & "','" & strCatID & "'," & vbCrLf & _
                " ' '," & strConvertRate & "," & strConvertToStk & "," & vbCrLf & _
                " " & strConvertToPO & ",'" & strDescr & "','" & strInspectCD & "',''," & vbCrLf & _
                " '" & strInspectStatus & "','" & strInvItemID & "','MAIN1'," & intPoLinenum & "," & vbCrLf & _
                " 'N','C','" & strMfgID & "','" & strUserID & "','" & strPO & "'," & vbCrLf & _
                " 0,0," & Decimal.Round(decQty, 4) & "," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToStk), 4) & "," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToPO), 4) & "," & vbCrLf & _
                " 0.0000, 0.0000,0.0000," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToStk), 4) & "," & vbCrLf & _
                " 0.0000," & Decimal.Round((decQty * strConvertToPO), 4) & "," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToStk), 4) & "," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToPO), 4) & "," & vbCrLf & _
                " 0.0000,0.0000, 0.0000," & vbCrLf & _
                " '" & strpoUOM & "','" & strpoUOM & "','" & strRecvStatus & "'," & vbCrLf & _
                " '" & stritemUOM & "','N','C'," & vbCrLf & _
                " '" & stritemUOM & "')"
        '   - erwin 2009.02.18
        '" VALUES('" & currentApp.Session("SITEBU") & "','" & strReceiver & "'," & intLine & ",' '," & vbCrLf & _
        '" ' ','" & currentApp.Session("SITEBU") & "','" & strCatID & "'," & vbCrLf & _
        'rowsaffected = ExecNonQuery(strsql)
        Dim command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 2. "
                sMyErrorString21 = "Place # 2, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            ' createReceiver = "Error Updating PS_RECV_LN"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        'rowsaffected = ExecNonQuery(strsql)


        Dim strDFLTPackslip As String
        If strPOERSAction = "V" Then
            strDFLTPackslip = strReceiver
        Else
            strDFLTPackslip = " "
        End If

        strsql = "" & _
"INSERT INTO PS_RECV_LN_SHIP" & vbCrLf & _
"(" & vbCrLf & _
" BUSINESS_UNIT " & vbCrLf & _
",RECEIVER_ID " & vbCrLf & _
",RECV_LN_NBR " & vbCrLf & _
",RECV_SHIP_SEQ_NBR " & vbCrLf & _
",BUSINESS_UNIT_IN " & vbCrLf & _
",BUSINESS_UNIT_PO " & vbCrLf & _
",COUNTRY_IST_ORIGIN " & vbCrLf & _
",CURRENCY_CD " & vbCrLf & _
",CURRENCY_CD_BASE " & vbCrLf & _
",DISTRIB_MTHD_FLG " & vbCrLf & _
",DUE_DT " & vbCrLf & _
",DUE_TIME " & vbCrLf & _
",INSPECT_DTTM " & vbCrLf & _
",INSPECT_STATUS " & vbCrLf & _
",IST_DISTRIB_STATUS " & vbCrLf & _
",LINE_NBR " & vbCrLf & _
",MATCH_LINE_FLG " & vbCrLf & _
",MERCH_AMT_BSE " & vbCrLf & _
",MERCH_AMT_PO_BSE " & vbCrLf & _
",MERCHANDISE_AMT " & vbCrLf & _
",MERCHANDISE_AMT_PO " & vbCrLf & _
",OP_SEQUENCE " & vbCrLf & _
",PACKSLIP_NO " & vbCrLf & _
",PO_ID " & vbCrLf & _
",PRICE_PO " & vbCrLf & _
",PRICE_PO_BSE " & vbCrLf & _
",PROCESS_INSTANCE " & vbCrLf & _
",PRODUCTION_ID " & vbCrLf & _
",QTY_SH_ACCPT " & vbCrLf & _
",QTY_SH_ACCPT_SUOM " & vbCrLf & _
",QTY_SH_ACCPT_VUOM " & vbCrLf & _
",QTY_SH_INSPD " & vbCrLf & _
",QTY_SH_INSPD_SUOM " & vbCrLf & _
",QTY_SH_INSPD_VUOM " & vbCrLf & _
",QTY_SH_RECVD " & vbCrLf & _
",QTY_SH_RECVD_SUOM " & vbCrLf & _
",QTY_SH_RECVD_VUOM " & vbCrLf & _
",QTY_SH_REJCT " & vbCrLf & _
",QTY_SH_REJCT_SUOM " & vbCrLf & _
",QTY_SH_REJCT_VUOM " & vbCrLf & _
",REJECT_ACTION " & vbCrLf & _
",REJECT_REASON " & vbCrLf & _
",RMA_ID " & vbCrLf & _
",RMA_LINE_NBR " & vbCrLf & _
",RECV_LN_MATCH_FLG " & vbCrLf & _
",RECV_SHIP_STATUS " & vbCrLf & _
",REPLACEMENT_FLG " & vbCrLf & _
",SCHED_NBR " & vbCrLf & _
",SHIP_DATE_STATUS " & vbCrLf & _
",SHIP_QTY_STATUS " & vbCrLf & _
",SHIPTO_ID " & vbCrLf & _
")" & vbCrLf & _
"VALUES " & vbCrLf & _
"(" & vbCrLf & _
" '" & strPOBU & "' " & vbCrLf & _
",'" & strReceiver & "' " & vbCrLf & _
"," & intLine & " " & vbCrLf & _
",1 " & vbCrLf & _
",'" & strBUin & "' " & vbCrLf & _
",'" & strPOBU & "' " & vbCrLf & _
",' ' " & vbCrLf & _
",'" & shipCurrency & "' " & vbCrLf & _
",'" & shipBaseCurrency & "' " & vbCrLf & _
",'Q' " & vbCrLf & _
",TO_DATE('" & strDuedate & "','YYYY-MM-DD') " & vbCrLf & _
",NULL " & vbCrLf & _
",NULL " & vbCrLf & _
",'" & strInspectStatus & "' " & vbCrLf & _
",'I' " & vbCrLf & _
"," & intPoLinenum & " " & vbCrLf & _
",'Y' " & vbCrLf & _
"," & decMerchAmtBse & " " & vbCrLf & _
"," & decMerchAmtPOBse & " " & vbCrLf & _
"," & decMerchandiseAmt & " " & vbCrLf & _
"," & decMerchandiseAmtPo & " " & vbCrLf & _
"," & intOPSequence & " " & vbCrLf & _
",'" & strDFLTPackslip & "' " & vbCrLf & _
",'" & strPO & "' " & vbCrLf & _
"," & decPricePO & " " & vbCrLf & _
"," & decPricePOBse & " " & vbCrLf & _
",0 " & vbCrLf & _
",'" & strProductionID & "' " & vbCrLf & _
"," & Decimal.Round(decQty, 4) & " " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToStk), 4) & " " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToPO), 4) & " " & vbCrLf & _
",0 " & vbCrLf & _
",0.0000 " & vbCrLf & _
",0.0000 " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToStk), 4) & " " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToPO), 4) & " " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToStk), 4) & " " & vbCrLf & _
",0 " & vbCrLf & _
",0.0000 " & vbCrLf & _
",0.0000 " & vbCrLf & _
",' ' " & vbCrLf & _
",' ' " & vbCrLf & _
",' ' " & vbCrLf & _
",0 " & vbCrLf & _
",'N' " & vbCrLf & _
",'R' " & vbCrLf & _
",'NA' " & vbCrLf & _
"," & intSchedNum & " " & vbCrLf & _
",'" & strShipDateStatus & "' " & vbCrLf & _
",'" & strShipQtyStatus & "' " & vbCrLf & _
",'" & strShipTo & "' " & vbCrLf & _
")" & vbCrLf & _
                 ""

        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 3. "
                sMyErrorString21 = "Place # 3, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            'createReceiver = "Error Updating PS_RECV_LN_SHIP"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        ''rowsaffected = ExecNonQuery(strsql)
        'If rowsaffected = 0 Then
        '    createReceiver = "Error Updating PS_RECV_LN_SHIP"
        '    Exit Function
        'End If
        ' if CST00 ste the recv_ds_status = 'R'  per Donna/Natashia 09.28.2009
        Dim strRecvDSStatus As String = " "

        '' Session("CST00") is used by the hard-coded menu.
        '' Privilege LOANERTOOL is used by the DB-driven menu.
        '' Include a test for both privileges during the switch from the
        '' hard-coded menu to the DB-driven menu.
        'If currentApp.Session("CST00") Or _
        '    clsAccessPrivileges.IsPrivilegeOn(currentApp.Session("USERID"), currentApp.Session("BUSUNIT"), _
        '                                      clsAccessPrivileges.UserPrivsEnum.LoanerTools) Then
        '    strRecvDSStatus = "R"
        'Else

        'End If
        'strsql = "INSERT INTO PS_RECV_LN_DISTRIB" & vbCrLf & _
        '        " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
        '        " 1,1,'" & strAccount & "'," & vbCrLf & _
        '        " ' ','" & strAnalysisType & "','" & strBUam & "'," & vbCrLf & _
        '        " '" & strBUgl & "','" & strBUin & "','" & strBUpc & "'," & vbCrLf & _
        '        " '" & strBUpo & "','" & strCapNum & "'," & intCapSeq & "," & vbCrLf & _
        '        " '" & strChartfieldStatus & "','N'," & vbCrLf & _
        '        " 'USD','USD',TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf & _
        '        " 'N',' ','" & strDistribType & "'," & vbCrLf & _
        '        " '" & strDstAcctType & "','" & strEmpID & "','" & strFinancialAssetSW & "'," & vbCrLf & _
        '        " 0," & intPoLinenum & ",'" & strShipTo & "'," & vbCrLf & _
        '        " " & decMerchAmtBse & "," & decMerchAmtPOBse & "," & vbCrLf & _
        '        " " & decMerchandiseAmt & "," & vbCrLf & _
        '        " " & decMerchandiseAmtPo & ",0," & intPODistLineNum & "," & vbCrLf & _
        '        " '" & strPO & "',0,'" & strProfileID & "'," & vbCrLf & _
        '        " " & Decimal.Round((decQty * strConvertToStk), 4) & "," & vbCrLf & _
        '        " " & Decimal.Round((decQty * strConvertToPO), 4) & "," & vbCrLf & _
        '        " " & Decimal.Round(decQtyPO, 4) & "," & vbCrLf & _
        '        " 0,0," & vbCrLf & _
        '        " '" & strRecvDSStatus & "','" & strReqID & "','" & strResourceCategory & "','" & strResourceSubCat & "'," & vbCrLf & _
        '        " '" & strResourceType & "',' '," & intSchedNum & "," & vbCrLf & _
        '        " '" & strStatisticsCode & "'," & decStatisticAmount & "," & decTaxCDSutPct & "," & vbCrLf & _
        '        " " & decTaxCDVatPct & ",'" & strDeptID & "','" & strProduct & "'," & vbCrLf & _
        '        " '" & strProjectID & "','" & strAffiliate & "')"
        ''   - erwin 2009.02.18
        ''" VALUES('" & currentApp.Session("SITEBU") & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _

        'Calculation to get Merchandise Base PO Amount
        Dim strsqlqry2 As String
        Dim calcMerBasePOAmt As Decimal
        Dim CalcdecMerchAmtBse As Decimal
        Dim calcdecMerchandiseAmtPo As Decimal
        Dim rateMultiplier As Decimal
        Dim rateDivisor As Decimal
        Dim todayDate As DateTime = DateTime.Today.ToString()
        Dim formattedtodayDate As String = String.Format(todayDate.ToString("dd-MMM-yyyy"))
        rateMultiplier = 1
        rateDivisor = 1

        If distCurrency <> distBaseCurrency Then


            strsqlqry2 = "SELECT RATE_MULT, RATE_DIV" & vbCrLf & _
                " FROM (select * from SYSADM8.PS_RT_RATE_TBL A" & vbCrLf & _
                " WHERE FROM_CUR = '" & strdistCurrency & "'" & vbCrLf & _
                " AND TO_CUR = '" & strdistBaseCurrency & "'" & vbCrLf & _
                " AND EFFDT = (SELECT MAX(A_ED.EFFDT) FROM SYSADM8.PS_RT_RATE_TBL A_ED WHERE A.RT_RATE_INDEX = A_ED.RT_RATE_INDEX AND A.TERM = A_ED.TERM AND A.TO_CUR = A_ED.TO_CUR AND A.FROM_CUR = A_ED.FROM_CUR AND A.RT_TYPE = A_ED.RT_TYPE AND A_ED.EFFDT <= TO_DATE('" & formattedtodayDate & "', 'DD-MON-YYYY'))) WHERE ROWNUM = 1  ORDER BY EFFDT DESC"



            Dim drRateMulDiv As OleDbDataReader = ORDBAccess.GetReader(strsqlqry2, myConnect)

            If drRateMulDiv IsNot Nothing AndAlso drRateMulDiv.HasRows Then
                If (drRateMulDiv.Read()) Then
                    rateMultiplier = drRateMulDiv.Item("RATE_MULT")
                    rateDivisor = drRateMulDiv.Item("RATE_DIV")
                End If

                drRateMulDiv.Close()
                
                CalcdecMerchAmtBse = decMerchAmtBse * rateMultiplier / rateDivisor
                calcMerBasePOAmt = decMerchAmtPOBse * rateMultiplier / rateDivisor
                ' converting back - because it came converted (and it should not be)
                calcdecMerchandiseAmtPo = decMerchandiseAmtPo * rateDivisor / rateMultiplier
            Else
                CalcdecMerchAmtBse = decMerchAmtBse
                calcMerBasePOAmt = decMerchAmtPOBse
                calcdecMerchandiseAmtPo = decMerchandiseAmtPo
            End If
            drRateMulDiv.Close()
        Else
            CalcdecMerchAmtBse = decMerchAmtBse
            calcMerBasePOAmt = decMerchAmtPOBse
            calcdecMerchandiseAmtPo = decMerchandiseAmtPo
        End If

        strsql = "" & _
"INSERT INTO PS_RECV_LN_DISTRIB" & vbCrLf & _
"(" & vbCrLf & _
" BUSINESS_UNIT " & vbCrLf & _
",RECEIVER_ID " & vbCrLf & _
",RECV_LN_NBR " & vbCrLf & _
",RECV_SHIP_SEQ_NBR " & vbCrLf & _
",DISTRIB_LINE_NUM " & vbCrLf & _
",ACCOUNT " & vbCrLf & _
",ACTIVITY_ID " & vbCrLf & _
",ANALYSIS_TYPE " & vbCrLf & _
",BUSINESS_UNIT_AM " & vbCrLf & _
",BUSINESS_UNIT_GL " & vbCrLf & _
",BUSINESS_UNIT_IN " & vbCrLf & _
",BUSINESS_UNIT_PC " & vbCrLf & _
",BUSINESS_UNIT_PO " & vbCrLf & _
",CAP_NUM " & vbCrLf & _
",CAP_SEQUENCE " & vbCrLf & _
",CHARTFIELD_STATUS " & vbCrLf & _
",COSTED_FLAG " & vbCrLf & _
",CURRENCY_CD " & vbCrLf & _
",CURRENCY_CD_BASE " & vbCrLf & _
",DELIVERED_DT " & vbCrLf & _
",DELIVERED_FLG " & vbCrLf & _
",DELIVERED_TO " & vbCrLf & _
",DISTRIB_TYPE " & vbCrLf & _
",DST_ACCT_TYPE " & vbCrLf & _
",EMPLID " & vbCrLf & _
",FINANCIAL_ASSET_SW " & vbCrLf & _
",FREIGHT_PERCENT " & vbCrLf & _
",LINE_NBR " & vbCrLf & _
",LOCATION " & vbCrLf & _
",MERCH_AMT_BSE " & vbCrLf & _
",MERCH_AMT_PO_BSE " & vbCrLf & _
",MERCHANDISE_AMT " & vbCrLf & _
",MERCHANDISE_AMT_PO " & vbCrLf & _
",MOV_DS_ACCPT_SUOM " & vbCrLf & _
",PO_DIST_LINE_NUM " & vbCrLf & _
",PO_ID " & vbCrLf & _
",PROCESS_INSTANCE " & vbCrLf & _
",PROFILE_ID " & vbCrLf & _
",QTY_DS_ACCPT_SUOM " & vbCrLf & _
",QTY_DS_ACCPT_VUOM " & vbCrLf & _
",QTY_PO " & vbCrLf & _
",RATE_DIV " & vbCrLf & _
",RATE_MULT " & vbCrLf & _
",RECV_DS_STATUS " & vbCrLf & _
",REQ_ID " & vbCrLf & _
",RESOURCE_CATEGORY " & vbCrLf & _
",RESOURCE_SUB_CAT " & vbCrLf & _
",RESOURCE_TYPE " & vbCrLf & _
",RT_TYPE " & vbCrLf & _
",SCHED_NBR " & vbCrLf & _
",STATISTICS_CODE " & vbCrLf & _
",STATISTIC_AMOUNT " & vbCrLf & _
",TAX_CD_SUT_PCT " & vbCrLf & _
",TAX_CD_VAT_PCT " & vbCrLf & _
",DEPTID " & vbCrLf & _
",PRODUCT " & vbCrLf & _
",PROJECT_ID " & vbCrLf & _
",AFFILIATE " & vbCrLf & _
")" & vbCrLf & _
"VALUES " & vbCrLf & _
"(" & vbCrLf & _
" '" & strPOBU & "' " & vbCrLf & _
",'" & strReceiver & "' " & vbCrLf & _
"," & intLine & " " & vbCrLf & _
",1 " & vbCrLf & _
",1 " & vbCrLf & _
",'" & strAccount & "' " & vbCrLf & _
",' ' " & vbCrLf & _
",'" & strAnalysisType & "' " & vbCrLf & _
",'" & strBUam & "' " & vbCrLf & _
",'" & strBUgl & "' " & vbCrLf & _
",'" & strBUin & "' " & vbCrLf & _
",'" & strBUpc & "' " & vbCrLf & _
",'" & strBUpo & "' " & vbCrLf & _
",'" & strCapNum & "' " & vbCrLf & _
"," & intCapSeq & " " & vbCrLf & _
",'" & strChartfieldStatus & "' " & vbCrLf & _
",'N' " & vbCrLf & _
",'" & distCurrency & "' " & vbCrLf & _
",'" & distBaseCurrency & "' " & vbCrLf & _
",TO_DATE('" & strShpDt & "','MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
",'N' " & vbCrLf & _
",' ' " & vbCrLf & _
",'" & strDistribType & "' " & vbCrLf & _
",'" & strDstAcctType & "' " & vbCrLf & _
",'" & strEmpID & "' " & vbCrLf & _
",'" & strFinancialAssetSW & "' " & vbCrLf & _
",0 " & vbCrLf & _
"," & intPoLinenum & " " & vbCrLf & _
",'" & strShipTo & "' " & vbCrLf & _
"," & CalcdecMerchAmtBse & " " & vbCrLf & _
"," & decMerchAmtPOBse & " " & vbCrLf & _
"," & decMerchandiseAmt & " " & vbCrLf & _
"," & calcdecMerchandiseAmtPo & " " & vbCrLf & _
",0 " & vbCrLf & _
"," & intPODistLineNum & " " & vbCrLf & _
",'" & strPO & "' " & vbCrLf & _
",0 " & vbCrLf & _
",'" & strProfileID & "' " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToStk), 4) & " " & vbCrLf & _
"," & Decimal.Round((decQty * strConvertToPO), 4) & " " & vbCrLf & _
"," & Decimal.Round(decQtyPO, 4) & " " & vbCrLf & _
",'" & rateDivisor & "' " & vbCrLf & _
",'" & rateMultiplier & "' " & vbCrLf & _
",'" & strRecvDSStatus & "' " & vbCrLf & _
",'" & strReqID & "' " & vbCrLf & _
",'" & strResourceCategory & "' " & vbCrLf & _
",'" & strResourceSubCat & "' " & vbCrLf & _
",'" & strResourceType & "' " & vbCrLf & _
",' ' " & vbCrLf & _
"," & intSchedNum & " " & vbCrLf & _
",'" & strStatisticsCode & "' " & vbCrLf & _
"," & decStatisticAmount & " " & vbCrLf & _
"," & decTaxCDSutPct & " " & vbCrLf & _
"," & decTaxCDVatPct & " " & vbCrLf & _
",'" & strDeptID & "' " & vbCrLf & _
",'" & strProduct & "' " & vbCrLf & _
",'" & strProjectID & "' " & vbCrLf & _
",'" & strAffiliate & "' " & vbCrLf & _
")" & vbCrLf & _
                 ""
        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 4. "
                sMyErrorString21 = "Place # 4, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            'createReceiver = "Error Updating PS_RECV_LN_DISTRIB"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        
        strsql = "SELECT DESCR" & vbCrLf & _
                " FROM PS_MANUFACTURER" & vbCrLf & _
                " WHERE SETID = 'MAIN1'" & vbCrLf & _
                " and MFG_ID = '" & strMfgID & "'"

        Dim dtrMFGNameReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
       
        If (dtrMFGNameReader.Read()) Then
            'strMFGName = dtrMFGNameReader.Item("DESCR")
            strMFGName = Replace(dtrMFGNameReader.Item("DESCR"), "’", "")
            strMFGName = Replace(dtrMFGNameReader.Item("DESCR"), "'", "")
        Else
            strMFGName = "UNKNOWN"
        End If
        dtrMFGNameReader.Close()

        If Trim(strSerialNums) = "" Then
            strsql = "INSERT INTO PS_RECV_LN_ASSET" & vbCrLf & _
                    " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
                    " 1,1,1," & vbCrLf & _
                    " " & decPricePO & ",' ','N'," & vbCrLf & _
                    " '" & strBUam & " ','" & strBUin & "',' '," & vbCrLf & _
                    " " & decMerchandiseAmt & ",'USD'," & vbCrLf & _
                    " ' ', substr('" & strDescr & "',0,30)," & vbCrLf & _
                    " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS')," & vbCrLf & _
                    " '" & strFinancialAssetSW & "',' ',' '," & vbCrLf & _
                    " ' ',' ',' '," & vbCrLf & _
                    " '" & strInvItemID & "','MAIN1','" & strShipTo & "'," & vbCrLf & _
                    " ' ','" & strMFGName & "'," & vbCrLf & _
                    " '" & strModel & "',0,0," & vbCrLf & _
                    " " & decPricePO & ",0,'" & strProfileID & "',0," & vbCrLf & _
                    " 0," & Decimal.Round((decQty * strConvertToStk), 4) & ",'O'," & vbCrLf & _
                    " ' ',' ',' '," & vbCrLf & _
                    " ' ',' ',' '," & vbCrLf & _
                    " ' ',' ','" & strVendor & "')"

            command = New OleDbCommand(strsql, connection)

            command.transaction = trnsactSession
            rowsaffected = command.ExecuteNonQuery()
            If rowsaffected = 0 Then

                If sErrorMessage = "2E1" Then
                Else
                    sErrorMessage = "Place # 5. "
                    sMyErrorString21 = "Place # 5, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                        "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                        ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                    sMyErrorString21 &= vbCrLf & " server=" & sServer
                    sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                    'SendSDiExchErrorMail(sMyErrorString21)
                End If
                createReceiver = False
                'createReceiver = "Error Updating PS_RECV_LN_ASSET"
                'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
                Return False
                Exit Function
            End If
        Else ' processing Serial NOs
            'Dim sSerialNums() As String = Split(strSerialNums, clsSerialItem.SerialIDDelimiter)
            'Dim i3 As Integer = 0
            'Dim strSerNum As String = ""
            'If sSerialNums.Length > 0 Then
            '    For i3 = 0 To sSerialNums.Length - 1
            '        strSerNum = sSerialNums(i3)
            '        'process each serial number by creating separate lines with diff sequence numbers (i3 + 1)
            '        strsql = "INSERT INTO PS_RECV_LN_ASSET" & vbCrLf & _
            '                " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
            '                " " & (i3 + 1).ToString() & ",1,1," & vbCrLf & _
            '                " " & decPricePO & ",' ','N'," & vbCrLf & _
            '                " '" & strBUam & " ','" & strBUin & "',' '," & vbCrLf & _
            '                " " & decMerchandiseAmt & ",'USD'," & vbCrLf & _
            '                " ' ', substr('" & strDescr & "',0,30)," & vbCrLf & _
            '                " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS')," & vbCrLf & _
            '                " '" & strFinancialAssetSW & "',' ',' '," & vbCrLf & _
            '                " ' ',' ',' '," & vbCrLf & _
            '                " '" & strInvItemID & "','MAIN1','" & strShipTo & "'," & vbCrLf & _
            '                " ' ','" & strMFGName & "'," & vbCrLf & _
            '                " '" & strModel & "',0,0," & vbCrLf & _
            '                " " & decPricePO & ",0,'" & strProfileID & "',0," & vbCrLf & _
            '                " 0," & Decimal.Round((decQty * strConvertToStk), 4) & ",'O'," & vbCrLf & _
            '                " '" & strSerNum & "',' ',' '," & vbCrLf & _
            '                " ' ',' ',' '," & vbCrLf & _
            '                " ' ',' ','" & strVendor & "')"

            '        command = New OleDbCommand(strsql, connection)

            '        command.transaction = trnsactSession
            '        rowsaffected = command.ExecuteNonQuery()
            '        If rowsaffected = 0 Then

            '            If sErrorMessage = "2E1" Then
            '            Else
            '                sErrorMessage = "Place # 6. "
            '                sMyErrorString21 = "Place # 6, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
            '                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
            '                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
            '                sMyErrorString21 &= vbCrLf & " server=" & sServer
            '                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
            '                SendSDiExchErrorMail(sMyErrorString21)
            '            End If
            '            createReceiver = False
            '            'createReceiver = "Error Updating PS_RECV_LN_ASSET"
            '            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            '            Return False
            '            Exit Function
            '        End If
            '    Next
            'End If
        End If  '  no Serial NOs
        
        Dim intOpTimeStd As Integer = 1
        Dim intOpTimeStdTotal As Integer = 1
        Dim strOpInstr As String = " "

        If Trim(strInspectCD) = "Y" Then
            'GET OP_TIME_STD and SUM(OP_TIME_STD)
            Dim strSQLstringInsp As String = "SELECT A.OP_TIME_STD,A.OP_INSTRUCTIONS,A.EFFDT FROM SYSADM.PS_ROUTING_LN A WHERE A.ROUTING_ID = '" & strRoutingID & "' AND EFFDT = " & vbCrLf & _
                    " (SELECT MAX(EFFDT) FROM SYSADM.PS_ROUTING_LN B " & vbCrLf & _
                    " WHERE A.ROUTING_ID = B.ROUTING_ID " & vbCrLf & _
                    " AND B.EFFDT <= SYSDATE)"

            Dim UserdataSetInsp As System.Data.DataSet = New System.Data.DataSet()
            Dim iMy2 As Integer = 0
            Try
                UserdataSetInsp = ORDBAccess.GetAdapter(strSQLstringInsp, myConnect)
                If Not UserdataSetInsp Is Nothing Then
                    If UserdataSetInsp.Tables.Count > 0 Then
                        If UserdataSetInsp.Tables(0).Rows.Count > 0 Then
                            If Not IsDBNull(UserdataSetInsp.Tables(0).Rows(0).Item("EFFDT")) Then
                                If IsDate(UserdataSetInsp.Tables(0).Rows(0).Item("EFFDT")) Then
                                    Dim dtEffDate As Date = CType(UserdataSetInsp.Tables(0).Rows(0).Item("EFFDT"), Date)
                                    Dim itDiff As Long = DateDiff(DateInterval.Day, dtEffDate, Now())
                                    If itDiff >= 0 Then  '  UserdataSetInsp.Tables(0).Rows(0)
                                        If Not UserdataSetInsp.Tables(0).Rows(0).Item("OP_TIME_STD") Is Nothing Then
                                            If Trim(UserdataSetInsp.Tables(0).Rows(0).Item("OP_TIME_STD")) <> "" Then
                                                If IsNumeric(Trim(UserdataSetInsp.Tables(0).Rows(0).Item("OP_TIME_STD"))) Then
                                                    intOpTimeStd = CType(UserdataSetInsp.Tables(0).Rows(0).Item("OP_TIME_STD"), Integer)

                                                End If
                                            End If
                                        End If
                                        If Not UserdataSetInsp.Tables(0).Rows(0).Item("OP_INSTRUCTIONS") Is Nothing Then
                                            If Trim(UserdataSetInsp.Tables(0).Rows(0).Item("OP_INSTRUCTIONS")) <> "" Then
                                                strOpInstr = Trim(UserdataSetInsp.Tables(0).Rows(0).Item("OP_INSTRUCTIONS"))

                                            End If
                                        End If
                                        intOpTimeStdTotal = intOpTimeStd
                                        If UserdataSetInsp.Tables(0).Rows.Count > 1 Then
                                            For iMy2 = 1 To UserdataSetInsp.Tables(0).Rows.Count - 1
                                                If Not UserdataSetInsp.Tables(0).Rows(iMy2).Item("OP_TIME_STD") Is Nothing Then
                                                    If Trim(UserdataSetInsp.Tables(0).Rows(iMy2).Item("OP_TIME_STD")) <> "" Then
                                                        If IsNumeric(Trim(UserdataSetInsp.Tables(0).Rows(iMy2).Item("OP_TIME_STD"))) Then
                                                            intOpTimeStdTotal += CType(UserdataSetInsp.Tables(0).Rows(iMy2).Item("OP_TIME_STD"), Integer)

                                                        End If
                                                    End If
                                                End If
                                            Next  '  For iMy2 = 0 To UserdataSetInsp.Tables(0).Rows.Count - 1
                                        End If  '  If UserdataSetInsp.Tables(0).Rows.Count > 1 Then

                                    Else
                                        intOpTimeStd = 1
                                        intOpTimeStdTotal = 1
                                        strOpInstr = " "
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                intOpTimeStd = 1
                intOpTimeStdTotal = 1
                strOpInstr = " "
            End Try

        Else
            intOpTimeStd = 0
            intOpTimeStdTotal = 0
            strOpInstr = " "
        End If  '  If Trim(strInspectCD) = "Y" Then

        strsql = "INSERT INTO PS_RECV_LN_INSP" & vbCrLf & _
                " (BUSINESS_UNIT,RECEIVER_ID,RECV_LN_NBR," & vbCrLf & _
                " OP_SEQ,ROUTING_SETID,ROUTING_ID," & vbCrLf & _
                " OP_TIME_STD,OP_TIME_CD,OP_TIME_STD_TTL," & vbCrLf & _
                " OP_TIME_ACT_TTL,PROCESS_INSTANCE,OP_INSTRUCTIONS)" & vbCrLf & _
                " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
                " 1,' ','" & strRoutingID & "'," & vbCrLf & _
                " " & intOpTimeStd & ",'P'," & intOpTimeStdTotal & "," & vbCrLf & _
                " 0,0,'" & strOpInstr & "')"

        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 7. "
                sMyErrorString21 = "Place # 7, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            'createReceiver = "Error Updating PS_RECV_LN_ASSET"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        
        If strTrckNo = "" Then
            strTrckNo = " "
        End If

        strsql = "INSERT INTO PS_ISA_RECV_LN_ASN" & vbCrLf & _
        " (BUSINESS_UNIT, RECEIVER_ID," & vbCrLf & _
        " RECV_LN_NBR, ISA_ASN_SHIP_DT," & vbCrLf & _
        " ISA_ASN_TRACK_NO, ISA_ASN_SHIP_VIA," & vbCrLf & _
        " LASTUPDDTTM)" & vbCrLf & _
        " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
        " TO_DATE('" & strShpDt & "','MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
        " '" & strTrckNo.ToUpper & "'," & vbCrLf & _
        " '" & strShipvia & "'," & vbCrLf & _
        " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'))"
        '   - erwin 2009.02.18
        '" VALUES('" & currentApp.Session("SITEBU") & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 8. "
                sMyErrorString21 = "Place # 8, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            'createReceiver = "Error Updating PS_RECV_LN_ASSET"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        
        Dim strRecvToBI As String = strReceivingType
        If strReceivingType = "R" Then
            strRecvToBI = "N"
        End If

        'Get billing price for the PS_ISA_NSTK_FSTK table if item ORO, ONCE, or FSTK

        If strStockType = "ORO" Or strStockType = "ONCE" Or strStockType = "FSTK" Then

            strsql = "SELECT B.ISA_SELL_PRICE" & vbCrLf & _
                        " FROM PS_PO_LINE_DISTRIB A, PS_REQ_LINE B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                        " AND A.PO_ID = '" & strPO & "'" & vbCrLf & _
                        " AND A.LINE_NBR = " & intPoLinenum & vbCrLf & _
                        " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                        " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
                        " AND B.INV_ITEM_ID = ' '" & vbCrLf & _
                        " AND A.REQ_LINE_NBR = B.LINE_NBR" & vbCrLf

            Dim dtrNSTKReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
            
            If (dtrNSTKReader.Read()) Then
                decNSTKPrice = dtrNSTKReader.Item("ISA_SELL_PRICE")
                dtrNSTKReader.Close()
                
            Else
                dtrNSTKReader.Close()
               
            End If

            'If the price is still = zero then check for fixed price

            If decNSTKPrice = 0.0 Then
                strsql = "SELECT A.LIST_PRICE" & vbCrLf & _
                            " FROM PS_PROD_PRICE A" & vbCrLf & _
                            " WHERE A.EFFDT = " & vbCrLf & _
                            " (SELECT MAX(A_ED.EFFDT) FROM PS_PROD_PRICE A_ED" & vbCrLf & _
                            " WHERE(A.SETID = A_ED.SETID)" & vbCrLf & _
                            " AND A.PRODUCT_ID = A_ED.PRODUCT_ID" & vbCrLf & _
                            " AND A.UNIT_OF_MEASURE = A_ED.UNIT_OF_MEASURE" & vbCrLf & _
                            " AND A.BUSINESS_UNIT_IN = A_ED.BUSINESS_UNIT_IN" & vbCrLf & _
                            " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                            " AND A.PRODUCT_ID = '" & strInvItemID & "'" & vbCrLf & _
                            " AND A.UNIT_OF_MEASURE = '" & strpoUOM & "'" & vbCrLf & _
                            " AND A.EFF_STATUS = 'A'" & vbCrLf & _
                            " AND ROWNUM < 2" & vbCrLf

                '" AND A.BUSINESS_UNIT_IN = ' '" & vbCrLf

                dtrNSTKReader = ORDBAccess.GetReader(strsql, myConnect)
                
                If (dtrNSTKReader.Read()) Then
                    decNSTKPrice = dtrNSTKReader.Item("LIST_PRICE")
                    dtrNSTKReader.Close()
                    
                Else
                    dtrNSTKReader.Close()
                   
                End If
            End If
            'If the price is still = zero then check get markup

            If decNSTKPrice = 0.0 Then
                strsql = "SELECT A.INV_ITEM_TYPE, A.INV_STOCK_TYPE," & vbCrLf & _
                    " A.BUSINESS_UNIT_OM, A.BUSINESS_UNIT_IN" & vbCrLf & _
                    " FROM PS_PO_LINE A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                    " AND A.PO_ID = '" & strPO & "'" & vbCrLf & _
                    " AND A.LINE_NBR = " & intPoLinenum
                '   - erwin 2009.02.18
                '" WHERE A.BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _

                dtrNSTKReader = ORDBAccess.GetReader(strsql, myConnect)
                
                If (dtrNSTKReader.Read()) Then
                    strsql = "SELECT A.ISA_MARKUP_RATE" & vbCrLf & _
                            " FROM PS_ISA_PRICE_RULE A" & vbCrLf & _
                            " WHERE A.BUSINESS_UNIT = '" & dtrNSTKReader.Item("BUSINESS_UNIT_OM") & "'" & vbCrLf & _
                            " AND A.SHIP_FROM_BU = '" & dtrNSTKReader.Item("BUSINESS_UNIT_IN") & "'" & vbCrLf & _
                            " AND A.ORDER_GRP = '" & dtrNSTKReader.Item("INV_STOCK_TYPE") & "'" & vbCrLf & _
                            " AND A.INV_ITEM_TYPE = '" & dtrNSTKReader.Item("INV_ITEM_TYPE") & "'"

                    Try
                        decNSTKMarkup = ORDBAccess.GetScalar(strsql, myConnect)
                    Catch ex As Exception
                        decNSTKMarkup = 0.0
                    End Try
                    decNSTKPrice = (((decNSTKMarkup / 100) * decPricePO) + decPricePO)
                    dtrNSTKReader.Close()
                    
                Else
                    decNSTKPrice = decPricePO
                    dtrNSTKReader.Close()
                   
                End If
            End If

            ' add rate conversion
            decNSTKPrice = decNSTKPrice * rateMultiplier / rateDivisor
            Dim decNetUnitPrice As Decimal = Decimal.Round((decNSTKPrice / strConvertToStk), 4)
            Dim decConvQty As Decimal = Decimal.Round((decQty * strConvertToStk), 4)

            If strStockType = "ORO" Then
                If Trim(strInvItemID) <> "" Then
                    'do not convert - per Scott 10/30/2015 - till they change their PS program. After they change this code should be commented out
                    decNetUnitPrice = Decimal.Round((decNSTKPrice), 4)
                    decConvQty = Decimal.Round((decQty), 4)
                End If
            End If
            strsql = "INSERT INTO PS_ISA_NSTK_FSTK" & vbCrLf & _
                    " (BUSINESS_UNIT, RECEIVER_ID, RECV_LN_NBR," & vbCrLf & _
                    " RTV_ID, RTV_LN_NBR, INV_ITEM_ID," & vbCrLf & _
                    " QTY_LN_ACCPT_VUOM, QTY_AM_RETRN_VUOM, BUSINESS_UNIT_PO," & vbCrLf & _
                    " PO_ID, LINE_NBR, INTFC_ID," & vbCrLf & _
                    " INTFC_LINE_NUM, ISA_LINE_TYPE, ISA_RECIEVED_TO_BI," & vbCrLf & _
                    " ISA_RTV_ACTION, OPRID," & vbCrLf & _
                    " DTTIME_ADDED," & vbCrLf & _
                    " GL_ENTRY_CREATED, NET_UNIT_PRICE)" & vbCrLf & _
                    " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
                    " ' ',0,'" & strInvItemID & "'," & vbCrLf & _
                    " " & decConvQty & ",0,'" & strBUpo & "'," & vbCrLf & _
                    " '" & strPO & "'," & intPoLinenum & ",0," & vbCrLf & _
                    " 0,'R','" & strRecvToBI & "'," & vbCrLf & _
                    " 'N','" & strUserID & "'," & vbCrLf & _
                    " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS')," & vbCrLf & _
                    " 'N'," & decNetUnitPrice & ")"
            ' multiplied decQty by strConvertToStk - per Scott Doyle request VR 08/29/2015
            ' changed decNSTKPrice to Decimal.Round((decNSTKPrice / strConvertToStk), 4) - per Scott Doyle request VR 08/29/2015

            '   - erwin 2009.02.18
            '" VALUES('" & currentApp.Session("SITEBU") & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _

            'DTTIME_ADDED - replaces DATE_ADDED when new table structure is moved to PROD
            command = New OleDbCommand(strsql, connection)
            command.transaction = trnsactSession
            rowsaffected = command.ExecuteNonQuery()
            If rowsaffected = 0 Then

                If sErrorMessage = "2E1" Then
                Else
                    sErrorMessage = "Place # 9. "
                    sMyErrorString21 = "Place # 9, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                        "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                        ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                    sMyErrorString21 &= vbCrLf & " server=" & sServer
                    sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                    'SendSDiExchErrorMail(sMyErrorString21)
                End If
                createReceiver = False
                'createReceiver = "Error Updating PS_ISA_NSTK_FSTK"
                'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
                Return False
                Exit Function
            End If

            Try
                Dim strNetUnitPrice As String = decNetUnitPrice.ToString()
                Dim strPOPrice As String = Decimal.Round(decPricePO, 4).ToString()
                Dim strNstkPrice As String = Decimal.Round(decNSTKPrice, 4).ToString()
                Dim strConvToStk As String = Decimal.Round(strConvertToStk, 1).ToString()
                'WriteAuditRecord(trnsactSession, connection, currentApp, decQty, strPO, intPoLinenum.ToString & " " & decConvQty.ToString() & " Qty", strInvItemID, " ORO success", "PO " & strPOPrice & " " & strConvToStk, "NSTK " & strNstkPrice, " Net " & strNetUnitPrice)

            Catch exORO32 As Exception

            End Try
        End If

        dtrRCVPOINFReader.Close()


        strsql = "update PS_PO_HDR" & vbCrLf & _
                    " set RECV_STATUS = 'P'" & vbCrLf & _
                    " where BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                    " and PO_ID = '" & strPO & "'" & vbCrLf & _
                    " and RECV_STATUS = 'N'"
        '   - erwin 2009.02.18
        '" where BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _
        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()


        ' update tariff code if it exists
        If Not Trim(strTariff) = "" Or _
            Trim(strItemWeight) = "" Then
            Dim bolExists As Boolean = False
            Dim dteNow As String = Now().ToString("MM-dd-yyyy")
            Dim strTranslItemID As String = strInvItemID

            'If Trim(strTranslItemID) = "" Then
            '    strTranslItemID = "NONSTOCK"
            '    'Dim objMfgTranslate As New clsItemTranslate(strTranslItemID, strMfgID, strMfgItmID)
            '    Dim objMfgTranslate As New clsItemTranslate(strTranslItemID, _
            '                                                strMfgID, _
            '                                                strMfgItmID, _
            '                                                connection, _
            '                                                trnsactSession)
            '    If Not objMfgTranslate.ItemTranslate Is Nothing Then
            '        strTranslItemID = objMfgTranslate.InvItemID
            '        bolExists = True
            '    End If
            'Else
            '    Dim objItemTranslate As New clsItemTranslate(strInvItemID)
            '    If Not objItemTranslate.ItemTranslate Is Nothing Then
            '        bolExists = True
            '    End If
            'End If

            If strTariff = "" Then
                strTariff = " "
            End If
            If strItemWeight = "" Then
                strItemWeight = "0"
            ElseIf Not IsNumeric(strItemWeight) Then
                strItemWeight = "0"
            Else
                strItemWeight = FormatNumber(Convert.ToDecimal(strItemWeight), 4)
            End If

            If bolExists = False Then
                strsql = "INSERT INTO PS_ISA_ITEM_TRANSL" & vbCrLf & _
                            "(SETID, INV_ITEM_ID, MFG_ID, MFG_ITM_ID, ISA_TARIFF_CODE, INV_ITEM_WEIGHT," & vbCrLf & _
                            " ISA_ITEM_TRANSLATE, LAST_UPDATE_DT)" & vbCrLf & _
                            " VALUES ('MAIN1', '" & strTranslItemID & "'," & vbCrLf & _
                            " '" & strMfgID & "', '" & strMfgItmID & "'," & vbCrLf & _
                            " '" & strTariff & "', '" & Replace(strItemWeight, ",", "") & "', ' '," & vbCrLf & _
                            " TO_DATE('" & dteNow & "', 'MM/DD/YYYY'))"
            ElseIf strTranslItemID = "NONSTOCK" Then
                strsql = "UPDATE PS_ISA_ITEM_TRANSL" & vbCrLf & _
                            " SET ISA_TARIFF_CODE = '" & strTariff & "'," & vbCrLf & _
                            " INV_ITEM_WEIGHT = '" & Replace(strItemWeight, ",", "") & "'," & vbCrLf & _
                            " LAST_UPDATE_DT = TO_DATE('" & dteNow & "', 'MM/DD/YYYY')" & vbCrLf & _
                            " WHERE INV_ITEM_ID = '" & strTranslItemID & "'" & vbCrLf & _
                            " AND MFG_ID = '" & strMfgID & "'" & vbCrLf & _
                            " AND MFG_ITM_ID = '" & strMfgItmID & "'"
            Else
                strsql = "UPDATE PS_ISA_ITEM_TRANSL" & vbCrLf & _
                            " SET ISA_TARIFF_CODE = '" & strTariff & "'," & vbCrLf & _
                            " INV_ITEM_WEIGHT = '" & Replace(strItemWeight, ",", "") & "'," & vbCrLf & _
                            " LAST_UPDATE_DT = TO_DATE('" & dteNow & "', 'MM/DD/YYYY')" & vbCrLf & _
                            " WHERE INV_ITEM_ID = '" & strTranslItemID & "'"
            End If
            command = New OleDbCommand(strsql, connection)
            command.transaction = trnsactSession
            ' this was done to ignore any exception for this table
            '   what's happening is that the item being inserted into PS_ISA_ITEM_TRANSL, might already be
            '   existing but have not been committed yet (due to transaction) BUT blows up with unique constraint if tried
            '   to get re-inserted again - erwin
            'rowsaffected = command.ExecuteNonQuery()
            Try
                rowsaffected = command.ExecuteNonQuery()
                rowsaffected = 1
            Catch ex As Exception
                ' just ignore
                rowsaffected = 1
            End Try
            If rowsaffected = 0 Then

                If sErrorMessage = "2E1" Then
                Else
                    sErrorMessage = "Place # 10. "
                    sMyErrorString21 = "Place # 10, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                        "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                        ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                    sMyErrorString21 &= vbCrLf & " server=" & sServer
                    sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                    'SendSDiExchErrorMail(sMyErrorString21)
                End If
                createReceiver = False
                ' createReceiver = "Error Updating PS_ISA_ITEM_TRANSL"
                'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
                Return False
                Exit Function
            End If

        End If

        If strStockType = "ORO" Or strStockType = "ONCE" Or strStockType = "FSTK" Then
            ' in this case we shouldn't even try to process current item as not Stock Item
            ' (shouldn't even try to INSERT INTO ps_isa_bur_tbl)

            ''code to update table PS_ISA_ORD_INTFC_L fields ISA_ORDER_STATUS and QTY_SHIPPED based on this receiver
            'Call UpdateIntfcLTblStatusQtyRecv(sMyReqid, sMyReqLnNbr, strShipQtyStatus, decMyQtyRecv)

            Exit Function
        End If

        strsql = "SELECT B.BUSINESS_UNIT_IN, A.PRICE_PO" & vbCrLf & _
                    " FROM PS_PO_LINE_SHIP A, PS_PO_LINE_DISTRIB B" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT = '" & strPOBU & "'" & vbCrLf & _
                    " AND A.PO_ID = '" & strPO & "'" & vbCrLf & _
                    " AND A.LINE_NBR = " & intPoLinenum & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND A.LINE_NBR = B.LINE_NBR" & vbCrLf & _
                    " AND A.SCHED_NBR = B.SCHED_NBR" & vbCrLf & _
                    " AND B.BUSINESS_UNIT_IN LIKE 'C%'"
        '   - erwin 2009.02.18
        '" WHERE A.BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _

        Dim dtrBURReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
        
        If (dtrBURReader.Read()) Then
            strBURbuIN = dtrBURReader.Item("BUSINESS_UNIT_IN")
            decBURPricePO = dtrBURReader.Item("PRICE_PO")
            If strConvertRate > 0 Then
                decBURPricePO = decBURPricePO / strConvertRate
            End If
            dtrBURReader.Close()
           
        Else
            dtrBURReader.Close()
           

            'WriteAuditRecord(trnsactSession, connection, currentApp, decQty, strPO, intPoLinenum.ToString, strInvItemID, " dtrBURReader Else")

            ''code to update table PS_ISA_ORD_INTFC_L fields ISA_ORDER_STATUS and QTY_SHIPPED based on this receiver
            'Call UpdateIntfcLTblStatusQtyRecv(sMyReqid, sMyReqLnNbr, strShipQtyStatus, decMyQtyRecv)

            Exit Function
        End If

        strsql = "Select A.LIST_PRICE" & vbCrLf & _
                    " FROM PS_PROD_PRICE A" & vbCrLf & _
                    " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.PRODUCT_ID = '" & strInvItemID & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT_IN = '" & strBURbuIN & "'" & vbCrLf & _
                    " AND A.EFFDT = " & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_PROD_PRICE A_ED" & vbCrLf & _
                    " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                    " AND A.PRODUCT_ID = A_ED.PRODUCT_ID" & vbCrLf & _
                    " AND A.UNIT_OF_MEASURE = A_ED.UNIT_OF_MEASURE" & vbCrLf & _
                    " AND A.BUSINESS_UNIT_IN = A_ED.BUSINESS_UNIT_IN" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND A.EFF_STATUS = 'A'" & vbCrLf

        Dim dtrBURPriceReader As OleDbDataReader = ORDBAccess.GetReader(strsql, myConnect)
        
        If (dtrBURPriceReader.Read()) Then
            decBURPrice = dtrBURPriceReader.Item("LIST_PRICE")
            dtrBURPriceReader.Close()
           
        Else
            dtrBURPriceReader.Close()
            
            strsql = "SELECT SUBSTR(A.DS_NETWORK_CODE,2)" & vbCrLf & _
                    " FROM PS_DS_NETDET_TBL A" & vbCrLf & _
                    " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT_IN = '" & strBURbuIN & "'"
            strDSNetworkCode = "I" & ORDBAccess.GetScalar(strsql, myConnect)

            strsql = "SELECT A.INV_ITEM_TYPE" & vbCrLf & _
                    " FROM PS_INV_ITEMS A" & vbCrLf & _
                    " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                    " AND A.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
                    " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                    " AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)"
            strBURItemType = ORDBAccess.GetScalar(strsql, myConnect)
            If strBURItemType Is Nothing Then
                strBURItemType = "XXX"
            End If

            strsql = "SELECT A.ISA_MARKUP_RATE" & vbCrLf & _
                    " FROM PS_ISA_PRICE_RULE A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT = '" & strDSNetworkCode & "'" & vbCrLf & _
                    " AND A.SHIP_FROM_BU = '" & strBURbuIN & "'" & vbCrLf & _
                    " AND A.ORDER_GRP = 'CSTK'" & vbCrLf & _
                    " AND A.INV_ITEM_TYPE = '" & strBURItemType & "'"

            decBURMarkup = 0
            Try
                Dim sBURMarkUp As String = ORDBAccess.GetScalar(strsql, myConnect)
                If Not sBURMarkUp Is Nothing Then
                    If Trim(sBURMarkUp) <> "" Then
                        If IsNumeric(sBURMarkUp) Then
                            decBURMarkup = CType(sBURMarkUp, Decimal)
                        End If
                    End If
                End If
            Catch ex As Exception
                decBURMarkup = 0
            End Try
            'decBURMarkup = ORDBData.GetScalar(strsql)

            decBURPrice = (((decBURMarkup / 100) * decBURPricePO) + decBURPricePO)
            bConvertRateApplied = True
        End If
        ' add rate conversion  
        decBURPrice = decBURPrice * rateMultiplier / rateDivisor

        strsql = "INSERT INTO ps_isa_bur_tbl" & vbCrLf & _
                " (BUSINESS_UNIT, RECEIVER_ID, RECV_LN_NBR," & vbCrLf & _
                " RTV_ID, RTV_LN_NBR, INV_ITEM_ID," & vbCrLf & _
                " QTY_LN_ACCPT_SUOM, QTY_LN_RETRN_SUOM," & vbCrLf & _
                " BUSINESS_UNIT_PO, PO_ID, LINE_NBR," & vbCrLf & _
                " INTFC_ID, INTFC_LINE_NUM, ISA_LINE_TYPE," & vbCrLf & _
                " ISA_RECIEVED_TO_BI, ISA_RTV_ACTION, OPRID," & vbCrLf & _
                " DATE_ADDED, NET_UNIT_PRICE, BUSINESS_UNIT_IN)" & vbCrLf & _
                " VALUES('" & strPOBU & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
                " ' ', 0, '" & strInvItemID & "'," & vbCrLf & _
                " " & Decimal.Round((decQty * strConvertToStk), 4) & ", 0," & vbCrLf & _
                " '" & strPOBU & "', '" & strPO & "', " & intPoLinenum & "," & vbCrLf & _
                " 0, 0, 'R'," & vbCrLf & _
                " '" & strRecvToBI & "', ' ', '" & strUserID & "'," & vbCrLf & _
                " TO_DATE('" & dteNowy & "','YYYY-MM-DD')," & vbCrLf

        '  & _
        '  " " & Decimal.Round((decBURPrice / strConvertToStk), 4) & ", '" & strBURbuIN & "')"
        ' cnanged decBURPrice to Decimal.Round((decBURPrice / strConvertToStk), 4) - per Scott Doyle request - VR 08/29/2015
        If bConvertRateApplied Then
            strsql = strsql & " " & decBURPrice & ", '" & strBURbuIN & "')"
        Else
            strsql = strsql & " " & Decimal.Round((decBURPrice / strConvertToStk), 4) & ", '" & strBURbuIN & "')"
        End If

        '   - erwin 2009.02.18
        '" VALUES('" & currentApp.Session("SITEBU") & "','" & strReceiver & "'," & intLine & "," & vbCrLf & _
        '" '" & currentApp.Session("SITEBU") & "', '" & strPO & "', " & intPoLinenum & "," & vbCrLf & _
        command = New OleDbCommand(strsql, connection)
        command.transaction = trnsactSession
        rowsaffected = command.ExecuteNonQuery()
        If rowsaffected = 0 Then

            If sErrorMessage = "2E1" Then
            Else
                sErrorMessage = "Place # 11. "
                sMyErrorString21 = "Place # 11, subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf & _
                    "User ID: = " & sUserId & " ;BUSINESS_UNIT_PO = " & strPOBU & " ;SHIPTO_ID = " & strShipTo & vbCrLf & _
                    ";PO_ID = " & strPO & " ;LINE_NBR = " & intPoLinenum.ToString() & " ;SCHED_NBR = " & intPOSchedNbr.ToString()
                sMyErrorString21 &= vbCrLf & " server=" & sServer
                sMyErrorString21 &= vbCrLf & " strsql=" & strsql
                'SendSDiExchErrorMail(sMyErrorString21)
            End If
            createReceiver = False
            'createReceiver = "Error Updating ps_isa_bur_tbl"
            'currentApp.Response.Redirect("~/DBErrorPage.aspx?HOME=N")
            Return False
            Exit Function
        End If
        
        Dim strNetUnitPrice1 As String = decBURPrice.ToString()
        Dim strPOPrice1 As String = Decimal.Round(decPricePO, 4).ToString()
        Dim strBurPriceConverted1 As String = Decimal.Round(decBURPricePO, 4).ToString()
        Dim strConvToStk1 As String = Decimal.Round(strConvertToStk, 1).ToString()
        'WriteAuditRecord(trnsactSession, connection, currentApp, decQty, strPO, intPoLinenum.ToString & " " & Decimal.Round((decQty * strConvertToStk), 4).ToString() & " Qty", strInvItemID, " STK End - Success", "PO " & strPOPrice1 & " " & strConvToStk1, "BUR Conv: " & strBurPriceConverted1, " BUR: " & strNetUnitPrice1)

        ''code to update table PS_ISA_ORD_INTFC_L fields ISA_ORDER_STATUS and QTY_SHIPPED based on this receiver
        'Call UpdateIntfcLTblStatusQtyRecv(sMyReqid, sMyReqLnNbr, strShipQtyStatus, decMyQtyRecv)

        createReceiver = True
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
                    " B.INV_ITEM_ID," & vbCrLf & _
                    " B.MFG_ITM_ID," & vbCrLf & _
                    " B.ITM_ID_VNDR," & vbCrLf & _
                    " B.DESCR254_MIXED," & vbCrLf & _
                    " C.QTY_PO," & vbCrLf & _
                    " B.UNIT_OF_MEASURE," & vbCrLf & _
                    " D.PRICE_PO, D.SHIPTO_ID," & vbCrLf & _
                    " D.MERCHANDISE_AMT," & vbCrLf & _
                    " B.MFG_ID," & vbCrLf & _
                    " B.INV_STOCK_TYPE," & vbCrLf & _
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
        If strSmallSite = "Y" Then
            strSQLString = strSQLString & " G.QTY_LN_ACCPT_VUOM as QTY_LN_ACCPT," & vbCrLf & _
                    " G.QTY_LN_ACCPT_VUOM as hQTYLNACCPT," & vbCrLf & _
                    " ' ' as BUSINESS_UNIT," & vbCrLf & _
                    " ' ' as RECEIVER_ID," & vbCrLf & _
                    " ' ' as RECV_LN_NBR," & vbCrLf & _
                    " G.ISA_SHIP_ID," & vbCrLf
        Else
            strSQLString = strSQLString & " G.QTY_LN_ACCPT," & vbCrLf & _
                    " G.QTY_LN_ACCPT as hQTYLNACCPT," & vbCrLf & _
                    " G.BUSINESS_UNIT," & vbCrLf & _
                    " G.RECEIVER_ID," & vbCrLf & _
                    " G.RECV_LN_NBR," & vbCrLf & _
                    " ' ' as ISA_SHIP_ID," & vbCrLf
        End If
        strSQLString = strSQLString & " D.MERCHANDISE_AMT as hMERCHANDISEAMT" & vbCrLf & _
                    " FROM PS_PO_HDR A," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB C," & vbCrLf & _
                    " PS_PO_LINE_SHIP D," & vbCrLf & _
                    " PS_PO_LINE B," & vbCrLf

        If strSmallSite = "Y" Then
            strSQLString = strSQLString & " PS_ISA_ASN_SHIPPED G" & vbCrLf
        Else
            strSQLString = strSQLString & " PS_RECV_LN G" & vbCrLf
        End If

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
                    " AND D.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND D.LINE_NBR = B.LINE_NBR" & vbCrLf & _
                    " AND C.PO_ID = G.PO_ID(+)" & vbCrLf & _
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
        Dim strSQLString As String
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

        strSQLstring = "SELECT B.ISA_ASN_SITE" & vbCrLf & _
                        " FROM PS_PO_LINE_SHIP A," & vbCrLf & _
                        " PS_ISA_SDR_BU_LOC B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                        " AND A.PO_ID = '" & strpo & "'" & vbCrLf & _
                        " AND A.SHIPTO_SETID = B.SETID" & vbCrLf & _
                        " AND A.SHIPTO_ID = B.LOCATION" & vbCrLf & _
                        " AND  rownum = '1'" & vbCrLf

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
