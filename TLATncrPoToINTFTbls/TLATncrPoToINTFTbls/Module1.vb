Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
'Imports System.Web.Mail
'Imports System.Web.UI
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\KLATencorIn"
    Dim logpath As String = "C:\KLATencorIn\LOGS\KLATencorPOInToINTF" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\KLATencorIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
    Dim strOverride As String
    Dim bolWarning As Boolean = False

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
            logpath = sLogPath & "\KLATencorPOInToINTF" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' initialize logs

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("Start of processing - " & Now.ToString())
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        'process received info
        Call ProceesKLATencorPOInfo()

        myLoggr1.WriteErrorLog("End of processing - " & Now.ToString())

        ' destroy logger object
        Try
            m_logger.Dispose()
            myLoggr1.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
            myLoggr1 = Nothing
        End Try

    End Sub

    Sub ProceesKLATencorPOInfo()

        Dim rtn As String = "KLATencorFromPOToINTF.ProceesKLATencorPOInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start KLA-Tencor From PO To INTF tables")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Start KLATencor From PO To INTF tables")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***
        ' for Production: \\sdixbatch\C$\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\KLATENCOR

        Dim sInputDir As String = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\KLATENCOR"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\KLATENCOR"
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
                    If aSrcFiles(I).Name.Length > Len("KLATENCOR_XML") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("KLATENCOR_XML") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\KLATencorIn\XMLIN\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\KLATencorIn\XMLIN\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\KLATencorIn\XMLIN\ " & "...")
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
        '// KLATencor From PO To INTF tables Processing: 
        '// inbound to the Oracle table SYSADM8.PS_ISA_ORD_INTF_HD and SYSADM8.PS_ISA_ORD_INTF_LN ...
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = GetKlaTencorPOIn()
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

        m_logger.WriteInformationLog(rtn & " :: End of KLA-Tencor From PO To INTF tables")

    End Sub

    Private Function GetKlaTencorPOIn() As Boolean

        Dim rtn As String = "KLATencorFromPOToINTF.GetKlaTencorPOIn"
        Dim bolError As Boolean = False

        Console.WriteLine("Start Insert of KLA-Tencor From PO in SYSADM8.PS_ISA_ORD_INTF_HD and SYSADM8.PS_ISA_ORD_INTF_LN")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start Insert of KLA-Tencor From PO in SYSADM8.PS_ISA_ORD_INTF_HD and SYSADM8.PS_ISA_ORD_INTF_LN")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\KLATencorIn\XMLIN\")
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
            bolError = False
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

                    Dim bErrorLoadXML As Boolean = False
                    Try
                        xmlRequest.LoadXml(XMLContent)
                    Catch exLoad As Exception
                        Console.WriteLine("")
                        Console.WriteLine("***error - " & exLoad.ToString)
                        Console.WriteLine("")
                        myLoggr1.WriteErrorLog(rtn & " :: Error loading XML: " & exLoad.Message.ToString & " in file " & aFiles(I).Name)
                        strXMLError = rtn & " :: Error loading XML: " & exLoad.ToString
                        bErrorLoadXML = True
                        bLineError = True
                        Try
                            File.Move(aFiles(I).FullName, "C:\KLATencorIn\BadXML\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try

                    If Trim(strXMLError) = "" Then

                        root = xmlRequest.DocumentElement
                        Dim strTestMy1 As String = ""
                        If Trim(strXMLError) = "" Then
                            If root.ChildNodes.Count > 0 Then
                                If root.FirstChild Is Nothing Then
                                    strXMLError = "Empty XML file"
                                    'bolError = True
                                    bLineError = True
                                ElseIf root.LastChild.Name.ToUpper = "REQUEST" Then
                                    'strTestMy1 = root.LastChild.Name.ToUpper
                                    'strTestMy1 = root.FirstChild.Name.ToUpper
                                    strXMLError = ""
                                Else
                                    strXMLError = "Missing last XML Element"
                                    'bolError = True
                                    bLineError = True
                                End If
                            Else
                                strXMLError = "Empty XML file"
                                'bolError = True
                                bLineError = True
                            End If  '  If root.ChildNodes.Count > 0 Then
                        End If  '  If Trim(strXMLError) = "" Then

                        'start parsing XML file
                        m_logger.WriteInformationLog(rtn & " :: Start parsing XML file ")
                        If Trim(strXMLError) = "" Then

                            Dim nodeOrdConf As XmlNode = root.LastChild()  '  node "REQUEST" 

                            If nodeOrdConf.ChildNodes.Count > 0 Then
                                Dim strFirstChildName As String = nodeOrdConf.FirstChild.Name.ToUpper
                                ' ORDERREQUEST
                                Select Case strFirstChildName
                                    Case "ORDERREQUEST"
                                        Dim nodeOrdRequest As XmlNode = nodeOrdConf.FirstChild()
                                        'check each node in "ORDERREQUEST"
                                        Dim iCtr As Integer = 0
                                        Dim j1 As Integer = 0
                                        Dim strChildNodeName As String = ""
                                        Dim strOrderNum As String = ""
                                        'define arrays for line info
                                        Dim arrLineNums(0) As String
                                        arrLineNums(0) = ""
                                        Dim arrLineQtys(0) As String
                                        arrLineQtys(0) = ""
                                        Dim arrSupplPartIDs(0) As String
                                        arrSupplPartIDs(0) = ""
                                        Dim arrAuxSupplPartIDs(0) As String
                                        arrAuxSupplPartIDs(0) = ""
                                        Dim arrUnitPrice(0) As String
                                        arrUnitPrice(0) = ""
                                        Dim arrCurrency(0) As String
                                        arrCurrency(0) = ""
                                        Dim arrDescr(0) As String
                                        arrDescr(0) = ""

                                        If nodeOrdRequest.ChildNodes.Count > 0 Then
                                            j1 = 0
                                            ReDim arrLineNums(0)
                                            arrLineNums(0) = ""
                                            ReDim arrLineQtys(0)
                                            arrLineQtys(0) = ""
                                            ReDim arrSupplPartIDs(0)
                                            arrSupplPartIDs(0) = ""
                                            ReDim arrAuxSupplPartIDs(0)
                                            arrAuxSupplPartIDs(0) = ""
                                            ReDim arrUnitPrice(0)
                                            arrUnitPrice(0) = ""
                                            ReDim arrCurrency(0)
                                            arrCurrency(0) = ""
                                            ReDim arrDescr(0)
                                            arrDescr(0) = ""

                                            For iCtr = 0 To nodeOrdRequest.ChildNodes.Count - 1
                                                strChildNodeName = UCase(nodeOrdRequest.ChildNodes(iCtr).Name)
                                                Dim nodeOrdrRefr As XmlNode = nodeOrdRequest.ChildNodes(iCtr)
                                                'get Header info from XML
                                                If strChildNodeName = "ORDERREQUESTHEADER" Then
                                                    'get order number from Attributes
                                                    If nodeOrdrRefr.Attributes.Count > 0 Then
                                                        For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                            If UCase(attrib.Name) = "ORDERID" Then
                                                                strOrderNum = attrib.Value
                                                            End If
                                                        Next  '  For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                    End If

                                                End If  '  If strChildNodeName = "ORDERREQUESTHEADER" Then
                                                'get Line Item info
                                                If strChildNodeName = "ITEMOUT" Then
                                                    'get Line number and Qty from Attributes
                                                    If nodeOrdrRefr.Attributes.Count > 0 Then
                                                        For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
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
                                                        Next  '  For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                    End If

                                                    If nodeOrdrRefr.ChildNodes.Count > 0 Then
                                                        Dim strLineItemChildName As String = ""
                                                        Dim strNodeChildItemIdName As String = ""
                                                        Dim strNodeChildMoneyName As String = ""

                                                        'get Item numbers, price, currency and descriptoin - like in punch out
                                                        For Each ChildItemNode As XmlNode In nodeOrdrRefr.ChildNodes()  '  for the node  "ITEMOUT" 
                                                            strLineItemChildName = UCase(ChildItemNode.Name)
                                                            Select Case strLineItemChildName
                                                                Case "ITEMID"
                                                                    If ChildItemNode.ChildNodes.Count > 0 Then
                                                                        ' get item IDs: Supplier Part Id and Aux. Supplier Part Id
                                                                        For Each NodeChildItemID As XmlNode In ChildItemNode.ChildNodes()
                                                                            strNodeChildItemIdName = UCase(NodeChildItemID.Name)
                                                                            Select Case strNodeChildItemIdName
                                                                                Case "SUPPLIERPARTID"
                                                                                    If j1 = 0 Then
                                                                                    Else
                                                                                        ReDim Preserve arrSupplPartIDs(j1)
                                                                                    End If
                                                                                    arrSupplPartIDs(j1) = NodeChildItemID.InnerText
                                                                                Case "SUPPLIERPARTAUXILIARYID"  '  SupplierPartAuxiliaryID
                                                                                    If j1 = 0 Then
                                                                                    Else
                                                                                        ReDim Preserve arrAuxSupplPartIDs(j1)
                                                                                    End If
                                                                                    arrAuxSupplPartIDs(j1) = NodeChildItemID.InnerText
                                                                                Case Else
                                                                                    'do nothing
                                                                            End Select
                                                                        Next  '  For Each NodeChildItemID As XmlNode In ChildItemNode.ChildNodes()
                                                                    End If  ' If ChildItemNode.ChildNodes.Count > 0 Then  -  ITEMID
                                                                Case "ITEMDETAIL"
                                                                    If ChildItemNode.ChildNodes.Count > 0 Then
                                                                        'get currency, price and description
                                                                        For Each NodeChildItemID As XmlNode In ChildItemNode.ChildNodes()
                                                                            strNodeChildItemIdName = UCase(NodeChildItemID.Name)
                                                                            Select Case strNodeChildItemIdName
                                                                                Case "UNITPRICE"
                                                                                    If NodeChildItemID.ChildNodes.Count > 0 Then
                                                                                        'node MONEY
                                                                                        For Each NodeChildIUnitPrice As XmlNode In NodeChildItemID.ChildNodes()
                                                                                            strNodeChildMoneyName = UCase(NodeChildIUnitPrice.Name)
                                                                                            Select Case strNodeChildMoneyName
                                                                                                Case "MONEY"
                                                                                                    If NodeChildIUnitPrice.Attributes.Count > 0 Then
                                                                                                        'get currency attribute
                                                                                                        For Each attrib As XmlAttribute In NodeChildIUnitPrice.Attributes()
                                                                                                            If UCase(attrib.Name) = "CURRENCY" Then
                                                                                                                If j1 = 0 Then
                                                                                                                Else
                                                                                                                    ReDim Preserve arrCurrency(j1)
                                                                                                                End If
                                                                                                                arrCurrency(j1) = attrib.Value
                                                                                                            End If
                                                                                                        Next  '  For Each attrib As XmlAttribute In NodeChildIUnitPrice.Attributes()
                                                                                                    End If
                                                                                                    If j1 = 0 Then
                                                                                                    Else
                                                                                                        ReDim Preserve arrUnitPrice(j1)
                                                                                                    End If
                                                                                                    arrUnitPrice(j1) = NodeChildIUnitPrice.InnerText
                                                                                                Case Else
                                                                                                    'do nothing
                                                                                            End Select

                                                                                        Next  '  For Each NodeChildIUnitPrice As XmlNode In NodeChildItemID.ChildNodes() - node MONEY
                                                                                    End If
                                                                                    
                                                                                Case "DESCRIPTION"
                                                                                    If j1 = 0 Then
                                                                                    Else
                                                                                        ReDim Preserve arrDescr(j1)
                                                                                    End If
                                                                                    arrDescr(j1) = NodeChildItemID.InnerText
                                                                                Case Else
                                                                                    'do nothing
                                                                            End Select
                                                                        Next  '  For Each NodeChildItemID As XmlNode In ChildItemNode.ChildNodes()
                                                                    End If  ' If ChildItemNode.ChildNodes.Count > 0 Then  -  ITEMDETAIL
                                                                Case Else
                                                                    'do nothing
                                                            End Select

                                                        Next  '  For Each ChildItemNode As XmlNode In nodeOrdrRefr.ChildNodes() - node  "ITEMOUT" 
                                                        j1 = j1 + 1

                                                    End If  ' If nodeOrdrRefr.ChildNodes.Count > 0 Then
                                                End If  '  If strChildNodeName = "ITEMOUT" Then
                                            Next  '  For iCtr = 0 To nodeOrdRequest.ChildNodes.Count - 1
                                            'finished parsing XML for one order
                                            m_logger.WriteInformationLog(rtn & " :: Finished parsing XML file for one order ")

                                            'start creating Interface record
                                            m_logger.WriteInformationLog(rtn & " :: Start creating Interface records ")
                                            Dim strSqlString As String = ""
                                            Dim strReqId As String = ""
                                           
                                            If Trim(strOrderNum) <> "" Then
                                                If arrDescr.Length > 0 And arrUnitPrice.Length > 0 And arrCurrency.Length > 0 And arrSupplPartIDs.Length > 0 And arrAuxSupplPartIDs.Length > 0 And arrLineNums.Length > 0 And arrLineQtys.Length > 0 Then
                                                    'get new Order No.
                                                    strReqId = getOrdreqID()

                                                    Dim i3 As Integer = 0
                                                    Dim cmd As OleDbCommand = Nothing

                                                    connectOR.Open()
                                                    Dim trnsactSession As OleDbTransaction = connectOR.BeginTransaction

                                                    Try
                                                        'header Insert string
                                                        strSqlString = "INSERT INTO SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf & _
                                                        "( " & vbCrLf & _
                                                        " BUSINESS_UNIT_OM " & vbCrLf & _
                                                        ",ORDER_NO " & vbCrLf & _
                                                        ",BILL_TO_CUST_ID " & vbCrLf & _
                                                        ",ORIGIN " & vbCrLf & _
                                                        ",SOURCE_ID " & vbCrLf & _
                                                        ",ORDER_DATE " & vbCrLf & _
                                                        ",ORDER_STATUS " & vbCrLf & _
                                                        ",REQUESTOR_ID " & vbCrLf & _
                                                        ",ISA_ORDER_TYPE " & vbCrLf & _
                                                        ",ADD_DTTM " & vbCrLf & _
                                                        ",LASTUPDDTTM " & vbCrLf & _
                                                        ") " & vbCrLf & _
                                                        "VALUES " & vbCrLf & _
                                                        "( " & vbCrLf & _
                                                        " 'I0515' " & vbCrLf & _
                                                        ",'" & strReqId & "' " & vbCrLf & _
                                                        ",'90589' " & vbCrLf & _
                                                        ",'IOL' " & vbCrLf & _
                                                        ",' ' " & vbCrLf & _
                                                        ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                                                        ",'OPN' " & vbCrLf & _
                                                        ",' ' " & vbCrLf & _
                                                        ",' ' " & vbCrLf & _
                                                        ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                                                        ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                                                        ") " & vbCrLf & _
                                                        ""

                                                        cmd = connectOR.CreateCommand
                                                        cmd.CommandText = strSqlString
                                                        cmd.CommandType = CommandType.Text
                                                        cmd.Transaction = trnsactSession
                                                        i3 = cmd.ExecuteNonQuery()
                                                        cmd = Nothing
                                                        If i3 > 0 Then
                                                            m_logger.WriteInformationLog(rtn & " :: Created Interface Header record with Order number: " & strReqId)
                                                        Else
                                                            'error
                                                            m_logger.WriteInformationLog(rtn & " :: Rollback. Error - rows affected <= 0 for Interface Header record with Order number: " & strReqId)
                                                            strXMLError &= rtn & " :: Rollback. Error - rows affected <= 0 for Interface Line record with Order number: " & strReqId

                                                            'bolError = True
                                                            bLineError = True
                                                            Exit For
                                                        End If
                                                        If Trim(strXMLError) = "" Then
                                                            Dim bIsAtLeastOneLineOK As Boolean = False
                                                            'create INTF_LN records based on arrSupplPartIDs(iLn) values
                                                            Dim iLn As Integer = 0
                                                            Dim strTAX_EXEMPT As String = My.Settings("TaxExempt").ToString.Trim
                                                            For iLn = 0 To arrLineNums.Length - 1

                                                                ' needed info: UnitPrice, UOM, ChargeCode for this part: arrSupplPartIDs(iLn)
                                                                Dim strUnitPrice As String = " "
                                                                Dim strStdUom As String = "EA"
                                                                Dim strPartChargeCode As String = " "

                                                                strSqlString = "SELECT A.INV_ITEM_ID, A.UNIT_MEASURE_STD, B.ISA_CUST_CHARGE_CD, C.PRICE FROM PS_MASTER_ITEM_TBL A, PS_ISA_MASTER_ITEM B, SYSADM8.PS_ISA_SDIEX_PRICE C " & vbCrLf & _
                                                                    "WHERE A.INV_ITEM_ID = '" & arrSupplPartIDs(iLn) & "' AND A.INV_ITEM_ID = B.INV_ITEM_ID AND A.INV_ITEM_ID = C.INV_ITEM_ID" & vbCrLf & _
                                                                    "" & vbCrLf & _
                                                                    "" & vbCrLf & _
                                                                    ""
                                                                cmd = connectOR.CreateCommand
                                                                cmd.CommandText = strSqlString
                                                                cmd.CommandType = CommandType.Text
                                                                cmd.Transaction = trnsactSession
                                                                'get reader
                                                                Dim rdr As OleDbDataReader = cmd.ExecuteReader()
                                                                If Not (rdr Is Nothing) Then
                                                                    If rdr.Read Then
                                                                        'get fields for INTF_LN
                                                                        strStdUom = CType(rdr("UNIT_MEASURE_STD"), String)
                                                                        strPartChargeCode = CType(rdr("ISA_CUST_CHARGE_CD"), String)
                                                                        strUnitPrice = CType(rdr("PRICE"), String)
                                                                    Else
                                                                        'error
                                                                        strXMLError += "Error retrieving Part info for: " & arrSupplPartIDs(iLn) & vbCrLf
                                                                        'do not process this file
                                                                        m_logger.WriteInformationLog(rtn & " :: Rollback. Error description: " & strXMLError)
                                                                        strXMLError &= rtn & " :: Rollback. Error description: " & strXMLError
                                                                        'bolError = True
                                                                        bLineError = True
                                                                        Exit For
                                                                    End If
                                                                End If
                                                                rdr.Close()
                                                                rdr = Nothing
                                                                cmd = Nothing

                                                                m_logger.WriteInformationLog(rtn & " :: Retrieved part info for: " & arrSupplPartIDs(iLn))
                                                                'insert interface line string

                                                                'get INTF insert line SQL

                                                                ' Due Date 
                                                                Dim dDueDate As Date = DateAdd(DateInterval.Day, 7, DateTime.Now.Date)
                                                                Dim StrDueDate As String = dDueDate.ToString()
                                                                Try
                                                                    StrDueDate = "TO_DATE('" & StrDueDate & "', 'MM/DD/YYYY HH:MI:SS AM')"
                                                                Catch ex As Exception
                                                                    StrDueDate = ""
                                                                End Try

                                                                strSqlString = "INSERT INTO SYSADM8.PS_ISA_ORD_INTF_LN (BUSINESS_UNIT_OM,ORDER_NO,ISA_INTFC_LN,BUSINESS_UNIT_IN,BUSINESS_UNIT_PO,REF_ORDER_NO,REF_LINE_NBR,SHIP_TO_CUST_ID,OPRID_APPROVED_BY,ITM_SETID," & _
                                                                    "INV_ITEM_ID,CONTRACT_NUM,CONTRACT_LINE_NUM2,TXN_CURRENCY_CD,CURRENCY_CD_BASE,PRICE_VNDR,ISA_SELL_PRICE," & _
                                                                    "UNIT_OF_MEASURE,QTY_REQUESTED,QTY_ORDERED_BASE,ISA_LINE_STATUS,CHANGE_FLAG,TAX_EXEMPT,TAX_EXEMPT_CERT," & _
                                                                    "TAX_GROUP,ISA_EMPLOYEE_ID,ISA_PRIORITY_FLAG,NEEDS_REPAIR_SW,PRIORITY_NBR,VENDOR_SETID,VENDOR_ID," & _
                                                                    "ITM_ID_VNDR,MFG_ID,ISA_MFG_FREEFORM,MFG_ITM_ID,ISA_NONCAT_KEY,INSPECT_CD,DESCR254,ISA_UNLOADING_PT," & _
                                                                    "DELIVERED_TO,PROCESS_INSTANCE,CUSTOMER_PO,ISA_CUST_PO_LINE,ACCOUNT,ISA_WORK_ORDER_NO,ISA_ACTIVITY_NBR,ISA_MACHINE_NO," & _
                                                                    "NETWORK_ID,ISA_WBS_ELMNT,PROJECT_ID,ISA_CUST_CHARGE_CD,ISA_QUOTE_REF,ISA_INTFC_LN_TYPE,ADD_DTTM,OPRID_ENTERED_BY," &
                                                                    "SHIPTO_ID,LOCATION2,QTY_RECEIVED,QTY_SHIPPED,KIT_PRESENT_FLG,ISA_KIT_ID,LINE_FIELD_C6,RFQ_IND,ISA_PICK_COMPLETE," & _
                                                                    "ISA_SUGGESTED_VNDR,STORAGE_AREA,STOR_LEVEL_1,STOR_LEVEL_2,STOR_LEVEL_3,STOR_LEVEL_4,HAZARDOUS_SW,ISA_ITEM_CAT," & _
                                                                    "DELIVERED_FLG,ISA_LANE_ID,BUYER_ID,SERIAL_ID,USER_CHAR1,USER_CHAR2,USER_CHAR3,USER_AMT1,USER_AMT2,USER_AMT3,USER1," & _
                                                                    "ISA_USER1,ISA_USER2,ISA_USER3,ISA_USER4,ISA_USER5,ERROR_FLAG,OPRID_MODIFIED_BY,ISA_STOP_NBR"

                                                                If Not String.IsNullOrEmpty(StrDueDate) Then
                                                                    strSqlString = strSqlString + ",ISA_REQUIRED_BY_DT"
                                                                End If
                                                                strSqlString = strSqlString + ")"

                                                                'default values
                                                                Dim strWorkOrder As String = " "
                                                                Dim strMachineNo As String = " "
                                                                Dim strVendorID As String = " "
                                                                Dim strVendorITMID As String = " "
                                                                Dim strPULINEFIELD As String = " "
                                                                Dim strRfqInd As String = " "
                                                                Dim strAddToCtlg As String = " "
                                                                Dim strMfdID As String = " "
                                                                Dim strMfdName As String = " "
                                                                Dim strMfgPartNumber As String = " "
                                                                Dim strDefaultEmpID As String = "TESTERJ1"
                                                                Dim strShipto As String = "L0515-01"
                                                                Dim strDefaultUserId As String = "TESTERJ1" ' special test user created in SDiExchange: JACK TESTER Superuser with email: vitaly.rovensky@sdi.com

                                                                If Trim(strUnitPrice) = "" Then
                                                                    strUnitPrice = "0"
                                                                End If
                                                                If Trim(strTAX_EXEMPT) = "" Then
                                                                    strTAX_EXEMPT = " "
                                                                End If

                                                                strSqlString = strSqlString & " VALUES ('I0515','" & strReqId & "'," & (iLn + 1) & ",' ',' ',' ',0,'90589',' ','MAIN1'," & _
                                                                "'" & arrSupplPartIDs(iLn) & "',' ',0,' ',' ','" & arrUnitPrice(iLn) & "'," & strUnitPrice & "," & _
                                                                "'" & strStdUom & "'," & arrLineQtys(iLn) & ",0,'NEW',' ','" & strTAX_EXEMPT & "',' '," & _
                                                                "' ','" & strDefaultEmpID & "', ' ',' ',0,'MAIN1','" & strVendorID & "'," & _
                                                                "'" & strVendorITMID & "','" & strMfdID & "','" & strMfdName & "','" & strMfgPartNumber & "',' ','N','" & arrDescr(iLn) & "',' '," & _
                                                                "' ',0,'" & strOrderNum & "','" & arrLineNums(iLn) & "',' ','" & strWorkOrder & "',' ','" & strMachineNo & "'," & _
                                                                "' ',' ',' ','" & strPartChargeCode & "',' ',' ',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'),'" & strDefaultUserId & "'," & _
                                                                "'" & strShipto & "',' ',0,0,' ',' ','" & strPULINEFIELD & "','" & strRfqInd & "',' '," & _
                                                                "' ',' ',' ',' ',' ',' ',' ',' '," & _
                                                                "' ',0,' ',' ',' ',' ',' ',0,0,0,'" & strAddToCtlg & "',' ',' ',' ',' ','" & arrAuxSupplPartIDs(iLn) & "',' ',' ',0"

                                                                If Not String.IsNullOrEmpty(StrDueDate) Then
                                                                    strSqlString = strSqlString + "," & StrDueDate
                                                                End If
                                                                strSqlString = strSqlString + ")"

                                                                cmd = connectOR.CreateCommand
                                                                cmd.CommandText = strSqlString
                                                                cmd.CommandType = CommandType.Text
                                                                cmd.Transaction = trnsactSession
                                                                i3 = cmd.ExecuteNonQuery
                                                                cmd = Nothing
                                                                If i3 > 0 Then
                                                                    m_logger.WriteInformationLog(rtn & " :: Created Interface Line record with Order number: " & strReqId & " for line number: " & (iLn + 1).ToString())
                                                                    bIsAtLeastOneLineOK = True
                                                                Else
                                                                    'error
                                                                    m_logger.WriteInformationLog(rtn & " :: Rollback. Error - rows affected <= 0 for Interface Line record with Order number: " & strReqId & " for line number: " & (iLn + 1).ToString())
                                                                    strXMLError &= rtn & " :: Rollback. Error - rows affected <= 0 for Interface Line record with Order number: " & strReqId & " for line number: " & (iLn + 1).ToString()
                                                                    'bolError = True
                                                                    bLineError = True
                                                                    Exit For
                                                                End If
                                                            Next  ' For iLn = 0 To arrLineNums.Length - 1

                                                            'If bIsAtLeastOneLineOK Then
                                                            '    'created INTF records. Need to record problems with lines if exists
                                                            'Else
                                                            '    'rollback
                                                            '    bolError = True
                                                            '    strXMLError &= rtn & " :: Rollback. All lines in the current order are erred out"
                                                            'End If
                                                        End If  '  If Trim(strXMLError) = "" Then - for line Items

                                                        If Trim(strXMLError) = "" Then
                                                            'Commit transaction
                                                            trnsactSession.Commit()
                                                            connectOR.Close()
                                                            trnsactSession = Nothing
                                                            m_logger.WriteInformationLog(rtn & " :: Finished Interface records creation for: " & aFiles(I).Name)
                                                        Else
                                                            'rollback
                                                            trnsactSession.Rollback()
                                                            connectOR.Close()
                                                            trnsactSession = Nothing
                                                            m_logger.WriteInformationLog(rtn & " :: Rollback. Error Description: " & strXMLError)
                                                            'bolError = True
                                                            bLineError = True
                                                        End If

                                                    Catch exTrans As Exception
                                                        'rollback
                                                        strXMLError &= " Error creating INTF records. Rolling back for: " & aFiles(I).Name & ". Error: " & exTrans.Message()
                                                        trnsactSession.Rollback()
                                                        connectOR.Close()
                                                        trnsactSession = Nothing
                                                        'bolError = True
                                                        bLineError = True
                                                        m_logger.WriteInformationLog(rtn & " :: Rollback. Error Description: " & strXMLError)
                                                    End Try

                                                Else
                                                    strXMLError &= " Some or all line info is missing for this file: " & aFiles(I).Name
                                                    m_logger.WriteInformationLog(rtn & " :: Error Description: " & strXMLError)

                                                    'bolError = True
                                                    bLineError = True
                                                End If
                                            Else
                                                strXMLError &= " Order Number is missing for this file: " & aFiles(I).Name
                                                m_logger.WriteInformationLog(rtn & " :: Error Description: " & strXMLError)

                                                'bolError = True
                                                bLineError = True
                                            End If
                                        End If  '  If nodeOrdRequest.ChildNodes.Count > 0 Then - node "ORDERREQUEST"
                                    Case Else
                                        'do nothing
                                End Select
                            End If  '  If nodeOrdConf.ChildNodes.Count > 0 Then
                        End If  '  If Trim(strXMLError) = "" Then - inner loop
                    End If ' If Trim(strXMLError) = "" Then - outer

                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bLineError Then
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
                        'move file to BadXML folder - if not moved already
                        If Not bErrorLoadXML Then
                            File.Copy(aFiles(I).FullName, "C:\KLATencorIn\BadXML\" & aFiles(I).Name, True)
                            File.Delete(aFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\KLATencorIn\BadXML\" & aFiles(I).Name)
                        End If
                        
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\KLATencorIn\XMLINProcessed\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\KLATencorIn\XMLINProcessed\" & aFiles(I).Name)

                    End If

                    If bLineError Then
                        bolError = True
                    End If

                Next  '  For I = 0 To aFiles.Length - 1

            End If  '  If aFiles.Length > 0 Then
        Catch ex As Exception
            strXMLError = ex.ToString()
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
            File.Copy(aFiles(I).FullName, "C:\KLATencorIn\BadXML\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\KLATencorIn\BadXML\" & aFiles(I).Name)

            bolError = True
        End Try

        Return bolError

    End Function

    Public Function UpdateToLineTable() As Integer
        '' those values should be available in UNILOG Search for I0515
        'System.Net.ServicePointManager.CertificatePolicy = New AlwaysIgnoreCertPolicy
        'System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
        'Dim urlAddress As String = "https://ora-linux-01.sdi.com/cimm2bcentral/productHunter/v3/515"
        'urlAddress = My.Settings("UnlgPath").ToString.Trim

        'Dim userName As String = My.Settings("UnlgSrchUser").ToString.Trim
        'Dim password As String = My.Settings("UnlgSrchPswd").ToString.Trim



        ' '' get line item info based on arrSupplPartIDs(iLn) values
        ' '' same way as in ItemDetailNew.aspx screen: by calling Unilog web service
        'urlAddress = urlAddress & "/product/search?query=customerPartnumber_515:""" & arrSupplPartIDs(iLn) & """"

        'Try
        '    Dim req As WebRequest = WebRequest.Create(urlAddress)
        '    req.Method = "GET"
        '    req.Credentials = New NetworkCredential(userName, password)
        '    req.UseDefaultCredentials = False
        '    Dim basicAuthBase641 As String = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", userName, password)))
        '    req.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641))
        '    Dim resp As HttpWebResponse = TryCast(req.GetResponse(), HttpWebResponse)
        '    If resp.StatusCode = HttpStatusCode.OK Then
        '        Using respStream As Stream = resp.GetResponseStream()
        '            Dim reader As New StreamReader(respStream, Encoding.UTF8)
        '            response = reader.ReadToEnd()
        '        End Using
        '    Else
        '        response = "Failed"
        '        strXMLError &= " response Failed."
        '        'rolling back
        '        trnsactSession.Rollback()
        '        connectOR.Close()
        '        trnsactSession = Nothing
        '        Exit For
        '    End If
        'Catch exRspns As Exception
        '    response = "Failed"
        '    strXMLError &= "Unilog Response Error: " & exRspns.Message
        '    'rolling back
        '    trnsactSession.Rollback()
        '    connectOR.Close()
        '    trnsactSession = Nothing
        '    Exit For
        'End Try

        ''parse sring 'response', get evrything in Data Table
        'Dim dtSearchResults As DataTable
        'dtSearchResults = ParseUnilogJsonValue(response, "515")

        Dim StrQuery As String = ""
        Dim trnsactSession As OleDbTransaction = Nothing
        Dim bSuccess As Boolean = False
        Dim exError As Exception
        Dim rowAffeted As Integer = 0
        Try
            connectOR.Open()
            trnsactSession = connectOR.BeginTransaction

            '' Due Date 
            'Dim StrDueDate As String
            'Try
            '    StrDueDate = "TO_DATE('" & DueDate & "', 'MM/DD/YYYY HH:MI:SS AM')"
            'Catch ex As Exception
            '    StrDueDate = ""
            'End Try

            'StrQuery = "INSERT INTO SYSADM8.PS_ISA_ORD_INTF_LN (BUSINESS_UNIT_OM,ORDER_NO,ISA_INTFC_LN,BUSINESS_UNIT_IN,BUSINESS_UNIT_PO,REF_ORDER_NO,REF_LINE_NBR,SHIP_TO_CUST_ID,OPRID_APPROVED_BY,ITM_SETID," & _
            '    "INV_ITEM_ID,CONTRACT_NUM,CONTRACT_LINE_NUM2,TXN_CURRENCY_CD,CURRENCY_CD_BASE,PRICE_VNDR,ISA_SELL_PRICE," & _
            '    "UNIT_OF_MEASURE,QTY_REQUESTED,QTY_ORDERED_BASE,ISA_LINE_STATUS,CHANGE_FLAG,TAX_EXEMPT,TAX_EXEMPT_CERT," & _
            '    "TAX_GROUP,ISA_EMPLOYEE_ID,ISA_PRIORITY_FLAG,NEEDS_REPAIR_SW,PRIORITY_NBR,VENDOR_SETID,VENDOR_ID," & _
            '    "ITM_ID_VNDR,MFG_ID,ISA_MFG_FREEFORM,MFG_ITM_ID,ISA_NONCAT_KEY,INSPECT_CD,DESCR254,ISA_UNLOADING_PT," & _
            '    "DELIVERED_TO,PROCESS_INSTANCE,CUSTOMER_PO,ISA_CUST_PO_LINE,ACCOUNT,ISA_WORK_ORDER_NO,ISA_ACTIVITY_NBR,ISA_MACHINE_NO," & _
            '    "NETWORK_ID,ISA_WBS_ELMNT,PROJECT_ID,ISA_CUST_CHARGE_CD,ISA_QUOTE_REF,ISA_INTFC_LN_TYPE,ADD_DTTM,OPRID_ENTERED_BY," &
            '    "SHIPTO_ID,LOCATION2,QTY_RECEIVED,QTY_SHIPPED,KIT_PRESENT_FLG,ISA_KIT_ID,LINE_FIELD_C6,RFQ_IND,ISA_PICK_COMPLETE," & _
            '    "ISA_SUGGESTED_VNDR,STORAGE_AREA,STOR_LEVEL_1,STOR_LEVEL_2,STOR_LEVEL_3,STOR_LEVEL_4,HAZARDOUS_SW,ISA_ITEM_CAT," & _
            '    "DELIVERED_FLG,ISA_LANE_ID,BUYER_ID,SERIAL_ID,USER_CHAR1,USER_CHAR2,USER_CHAR3,USER_AMT1,USER_AMT2,USER_AMT3,USER1," & _
            '    "ISA_USER1,ISA_USER2,ISA_USER3,ISA_USER4,ISA_USER5,ERROR_FLAG,OPRID_MODIFIED_BY,ISA_STOP_NBR"

            'If Not String.IsNullOrEmpty(StrDueDate) Then
            '    StrQuery = StrQuery + ",ISA_REQUIRED_BY_DT"
            'End If
            'StrQuery = StrQuery + ")"


            'StrQuery = StrQuery & "VALUES ('I0515','" & strReqId & "','" & (iLn + 1).ToString() & "',' ',' ',' ',0,'90589',' ','MAIN1'," & _
            '"'" & ItemID & "',' ',0,' ',' ','" & PricePO & "','" & UnitPrice & "'," & _
            '"'" & UOM & "'," & Qty & ",0,'" & LineStatus & "',' ','" & TAX_EXEMPT & "',' '," & _
            '"' ','" & EmpID & "', '" & Priority & "',' ',0,'MAIN1','" & VendorID & "'," & _
            '"'" & VendorITMID & "','" & MfdID & "','" & MfdName & "','" & PartNumber & "',' ','N','" & Description & "',' '," & _
            '"' ',0,'" & CustPO & "','" & CustPOLN & "',' ','" & WorkOrder & "',' ','" & MachineNo & "'," & _
            '"' ',' ',' ','" & ChargeCode & "',' ',' ',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'),'" & HttpContext.Current.Session("USERID").ToString() & "'," & _
            '"'" & SHIPID & "',' ',0,0,' ',' ','" & PULINEFIELD & "','" & ReqID & "',' '," & _
            '"' ',' ',' ',' ',' ',' ',' ',' '," & _
            '"' ',0,' ',' ',' ',' ',' ',0,0,0,'" & User & "',' ',' ',' ',' ','" & VendorAuxITMID & "',' ',' ',0"

            'If Not String.IsNullOrEmpty(StrDueDate) Then
            '    StrQuery = StrQuery + "," & StrDueDate
            'End If
            'StrQuery = StrQuery + ")"


            'If ORDBData.ExecuteNonQuery(trnsactSession, connection, StrQuery, rowAffeted, exError) Then
            '    trnsactSession.Commit()
            'Else
            '    bSuccess = False
            '    trnsactSession.Rollback()
            'End If
            Return rowAffeted
        Catch ex As Exception
            'Dim sMyErrorString As String = String.Empty
            'If Not ex Is Nothing Then
            '    sMyErrorString = ex.ToString
            'End If
            'sMyErrorString += sMyErrorString & _
            '           "Error in clsShoppingcart -> UpdateToLineTable" & vbCrLf & _
            '           "UserID = " & HttpContext.Current.Session("USERID") & ", Business Unit = " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID")
            'SendSDiExchErrorMail(sMyErrorString)
            Return rowAffeted
        Finally
            'connection.Close()
            'trnsactSession = Nothing
            'connection = Nothing
        End Try
    End Function

    Private Function ParseUnilogJsonValue(response As String, catalogId As String, Optional ByVal bTreeFlag As Boolean = False) As DataTable
        Dim dt As New DataTable
        Dim dtCM As New DataTable
        Dim origCatalogID As String = catalogId

        If Trim(response) <> "" Then
            response = Trim(response)
        Else
            Return dt
        End If

        Try

        Catch ex As Exception

        End Try

        Return dt

    End Function

    Private Function getOrdreqID() As String

        Dim strreqid As String = ""
        Dim strReqNo As String = ""
        Dim strOrderNo As String = ""
        Dim strOrdIntfcH As String = ""
        Dim bolReqExist As Boolean
        bolReqExist = True
        Dim bolOrderExist As Boolean = True
        Dim bolOrdNoIntfcH As Boolean = True
        Dim bolExist As Boolean = True
        Dim strBusunit As String = "I0515"
        Dim strSiteBu As String = "ISA00"

        Dim i As Integer = 0
        Dim cmd As OleDbCommand = Nothing

        connectOR.Open()
        'Dim trnsactSession As OleDbTransaction = connectOR.BeginTransaction
        Do Until bolExist = False

            Dim reqSelSQL As String = ""

            reqSelSQL = "select SYSADM8.isa_next_ord('" & strBusunit & "') from dual"
            Try
                cmd = connectOR.CreateCommand
                cmd.CommandText = reqSelSQL
                cmd.CommandType = CommandType.Text
                'm_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
                strreqid = cmd.ExecuteScalar
                cmd = Nothing
            Catch ex As Exception
                strreqid = "0"
            End Try

            'check PS_REQ_HDR
            Dim strSQLstring As String = "SELECT A.REQ_ID" & vbCrLf & _
            " FROM PS_REQ_HDR A" & vbCrLf & _
            " WHERE A.BUSINESS_UNIT = '" & strSiteBu & "'" & vbCrLf & _
            " AND A.REQ_ID = '" & strreqid & "'"

            cmd = connectOR.CreateCommand
            cmd.CommandText = strSQLstring
            cmd.CommandType = CommandType.Text
            'm_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
            strReqNo = cmd.ExecuteScalar
            cmd = Nothing
            If strReqNo = strreqid Then
                bolReqExist = True
            Else
                bolReqExist = False
            End If

            'check PS_ORD_HEADER
            Dim strSQLstringOrd As String = "SELECT A.ORDER_NO" & vbCrLf & _
            " FROM PS_ORD_HEADER A" & vbCrLf & _
            " WHERE A.BUSINESS_UNIT = '" & strBusunit & "'" & vbCrLf & _
            " AND A.ORDER_NO = '" & strreqid & "'"

            cmd = connectOR.CreateCommand
            cmd.CommandText = strSQLstringOrd
            cmd.CommandType = CommandType.Text
            'm_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
            strOrderNo = cmd.ExecuteScalar
            cmd = Nothing

            If strOrderNo = strreqid Then
                bolOrderExist = True
            Else
                bolOrderExist = False
            End If

            ' check PS_ISA_ORD_INTFC_H (BUSINESS_UNIT_OM and ORDER_NO)
            Dim strSQLstringIntfcH As String = "SELECT A.ORDER_NO" & vbCrLf & _
            " FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf & _
            " WHERE A.BUSINESS_UNIT_OM = '" & strBusunit & "'" & vbCrLf & _
            " AND A.ORDER_NO = '" & strreqid & "'"

            cmd = connectOR.CreateCommand
            cmd.CommandText = strSQLstringIntfcH
            cmd.CommandType = CommandType.Text
            'm_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
            strOrdIntfcH = cmd.ExecuteScalar
            cmd = Nothing

            If strOrdIntfcH = strreqid Then
                bolOrdNoIntfcH = True
            Else
                bolOrdNoIntfcH = False
            End If

            If (bolReqExist = False) And (bolOrderExist = False) And (bolOrdNoIntfcH = False) Then
                bolExist = False
            End If

        Loop

        connectOR.Close()

        Return strreqid

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "KLATencorFromPOToINTF.SendEmail"
        Dim strEmailFrom As String = ""
        Dim strEmailTo As String = ""
        Dim strEmailCC As String = ""
        Dim strEmailBcc As String = ""
        Dim strEmailBody As String = ""
        Dim strEmailSubject As String = ""

        'The email address of the sender
        strEmailFrom = "TechSupport@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_From")) <> "") Then
            strEmailFrom = CStr(My.Settings(propertyName:="onErrorEmail_From")).Trim
        End If

        'The email address of the recipient. 
        strEmailTo = "vitaly.rovensky@sdi.com"
        If bIsSendOut Then
            If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                strEmailTo = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
            End If
        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_CC")) <> "") Then
            strEmailCC = CStr(My.Settings(propertyName:="onErrorEmail_CC")).Trim
        Else
            strEmailCC = ""
        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_BCC")) <> "") Then
            strEmailBcc = CStr(My.Settings(propertyName:="onErrorEmail_BCC")).Trim
        Else
            strEmailBcc = "webdev@sdi.com"
        End If

        strEmailBody = ""
        strEmailBody &= "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >KLA-Tencor From PO To INTF Error</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>KLA-Tencor From PO To INTF tables has completed with "
        If bolWarning = True Then
            strEmailBody &= "warnings,"
            strEmailSubject = " KLA-Tencor From PO To INTF tables Warning"
        Else
            strEmailBody &= "errors;"
            strEmailSubject = " KLA-Tencor From PO To INTF tables Error"
        End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Try

            sInfoErr &= " XML file name(s) are below.</td></tr>"
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

            strEmailBody &= " review log.</td></tr>"
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

        If connectOR.DataSource.ToUpper.IndexOf("RPTG") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("DEVL") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("STAR") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("PLGR") > -1 Then
            strEmailTo = "WEBDEV@sdi.com"
            strEmailSubject = " (test run) " & strEmailSubject
        End If

        Dim bSend As Boolean = False
        Try

            SendLogger(strEmailSubject, strEmailBody, "KLATENCORTOINTF", "Mail", strEmailTo, strEmailCC, strEmailBcc)
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


Public Class AlwaysIgnoreCertPolicy
    Implements ICertificatePolicy
    Public Function CheckValidationResult(ByVal srvPoint As ServicePoint, _
                                          ByVal cert As X509Certificate, ByVal request As WebRequest, _
                                          ByVal certificateProblem As Integer) As Boolean Implements ICertificatePolicy.CheckValidationResult
        ' return TRUE to force the certificate to be accepted.
        Return True
    End Function
End Class

