Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
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
    'Private m_POConfirm_Logger As appLogger = Nothing
    Private m_POConfirm_LoggerN1 As appLogger = Nothing

    Dim rootDir As String = "C:\LMIn"
    Dim logpath As String = "C:\LMIn\LOGS\LMInPOToFLAT" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\LMIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sPOConfirmPath As String = "C:\LMIn\SFTP\POConfrm\POConfirm" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sPoConfirmText As String = ""
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
    Dim strOverride As String
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR"
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
            logpath = sLogPath & "\LMInPOToFLAT" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' initialize logs 

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)

        Call ProceesLyonsPOInfo()

        myLoggr1.WriteErrorLog("End of processing - " & Now.ToString())

        ' destroy logger object (Error logs)
        Try
            myLoggr1.Dispose()
        Catch ex As Exception
        Finally
            myLoggr1 = Nothing
        End Try

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Sub ProceesLyonsPOInfo()

        Dim rtn As String = "LyonsPoProcessing.ProceesLyonsPOInfo"
        Dim bError As Boolean = False

        m_logger.WriteInformationLog("Start of processing - " & Now.ToString())
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        Dim strInfoForLog As String = "" ' NEED TO CHANGE logpath TO REFLECT 531 or 532
        strInfoForLog = "Start of processing - " & Now.ToString() & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                    " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                   rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]" & _
                                   ""

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\LYONS"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\LYONS"
        End Try
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)

        Dim strFiles As String = ""
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer

        Try
            If aSrcFiles.Length > 0 Then
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > Len("LYONS_XML") - 1 Then
                        If aSrcFiles(I).Name.StartsWith("LYONS_XML") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\LMIn\XMLIN\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\LMIn\XMLIN\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\LMIn\XMLIN\ " & "...")
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

        Dim bolError As Boolean = False

        Try
            bolError = GetLyonsPOIn()
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

        m_logger.WriteInformationLog(rtn & " :: End of Lyons From PO To Flat File")

    End Sub

    Private Function GetLyonsPOIn() As Boolean

        Dim rtn As String = "LyonsPoProcessing.GetLyonsPOIn"
        Dim bolError As Boolean = False

        m_logger.WriteInformationLog(rtn & " :: Start XML Parsing for Lyons From PO To Flat File")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\LMIn\XMLIN\")
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
                        
                        myLoggr1.WriteErrorLog(rtn & " :: Error loading XML: " & exLoad.Message.ToString & " in file " & aFiles(I).Name)
                        strXMLError = rtn & " :: Error loading XML: " & exLoad.ToString
                        bErrorLoadXML = True
                        bLineError = True
                        Try
                            File.Move(aFiles(I).FullName, "C:\LMIn\BadXML\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML folder): " & ex24.Message & " in file " & aFiles(I).Name)
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End Try

                    sPoConfirmText = ""
                    sPOConfirmPath = ""
                    Dim strReqId As String = ""

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

                        End If  '  If Trim(strXMLError) = "" Then  ' 1st level inner

                        If Trim(strXMLError) = "" Then
                            Dim strOrigOrderQuoteNumber As String = " "
                            ' do not have right now - skip
                            ' <ADD CODE LATER>
                            If Trim(strOrigOrderQuoteNumber) = "" Then
                                strOrigOrderQuoteNumber = " "
                            End If

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
                                        Dim strSdiBusUnit As String = ""
                                        Dim strLyonsPlantName As String = ""

                                        Dim strQuoteID As String = " "
                                        Dim arrQuoteLineNum(0) As String
                                        arrQuoteLineNum(0) = ""
                                        'define arrays for line info
                                        Dim arrLineNums(0) As String
                                        arrLineNums(0) = ""
                                        Dim arrDueDates(0) As String
                                        arrDueDates(0) = ""
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
                                        Dim arrVendor(0) As String
                                        arrVendor(0) = ""
                                        Dim arrVendPartNum(0) As String
                                        arrVendPartNum(0) = ""
                                        Dim arrMfgId(0) As String
                                        arrMfgId(0) = ""
                                        Dim arrMfgPartNum(0) As String
                                        arrMfgPartNum(0) = ""
                                        Dim arrUOMs(0) As String
                                        arrUOMs(0) = ""
                                        Dim arrTax(0) As String
                                        arrTax(0) = ""
                                        Dim arrMatGroupTax(0) As String
                                        arrMatGroupTax(0) = ""

                                        If nodeOrdRequest.ChildNodes.Count > 0 Then
                                            j1 = 0
                                            ReDim arrLineNums(0)
                                            arrLineNums(0) = ""
                                            ReDim arrDueDates(0)
                                            arrDueDates(0) = ""
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
                                            ReDim arrUOMs(0)
                                            arrUOMs(0) = ""
                                            ReDim arrTax(0)
                                            arrTax(0) = ""
                                            ReDim arrMatGroupTax(0)
                                            arrMatGroupTax(0) = ""

                                            ReDim arrVendor(0)
                                            arrVendor(0) = ""
                                            ReDim arrVendPartNum(0)
                                            arrVendPartNum(0) = ""
                                            ReDim arrMfgId(0)
                                            arrMfgId(0) = ""
                                            ReDim arrMfgPartNum(0)
                                            arrMfgPartNum(0) = ""
                                            ReDim arrQuoteLineNum(0)
                                            arrQuoteLineNum(0) = ""

                                            For iCtr = 0 To nodeOrdRequest.ChildNodes.Count - 1
                                                strChildNodeName = UCase(nodeOrdRequest.ChildNodes(iCtr).Name)
                                                Dim nodeOrdrRefr As XmlNode = nodeOrdRequest.ChildNodes(iCtr)
                                                'get Header info from XML
                                                If strChildNodeName = "ORDERREQUESTHEADER" Then
                                                    'get info from Attributes
                                                    If nodeOrdrRefr.Attributes.Count > 0 Then
                                                        For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                            'get order number from Attributes
                                                            If UCase(attrib.Name) = "ORDERID" Then
                                                                strOrderNum = attrib.Value

                                                            End If  ' If UCase(attrib.Name) = "ORDERID" Then
                                                        Next ' For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                    End If  '  If nodeOrdrRefr.Attributes.Count > 0 Then
                                                    ' get SDI Bus. Unit
                                                    If nodeOrdrRefr.ChildNodes.Count > 0 Then
                                                        Dim nodeBusUnitName As System.Xml.XmlNode = nodeOrdrRefr.SelectSingleNode("ShipTo/Address/Name")
                                                        If Not (nodeBusUnitName Is Nothing) Then
                                                            Try
                                                                strLyonsPlantName = UCase(nodeBusUnitName.InnerText)
                                                                If Trim(strLyonsPlantName) <> "" Then
                                                                    If Len(strLyonsPlantName) > 3 Then
                                                                        If strLyonsPlantName.Contains("WEST") Then
                                                                            strSdiBusUnit = "I0531"
                                                                        ElseIf strLyonsPlantName.Contains("EAST") Then
                                                                            strSdiBusUnit = "I0532"
                                                                        End If
                                                                    End If
                                                                End If

                                                            Catch ex As Exception

                                                            End Try
                                                        End If  '  If Not (nodeBusUnitName Is Nothing) Then
                                                    End If  '  If nodeOrdrRefr.ChildNodes.Count > 0 Then

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
                                                            ' code for Due Date 
                                                            If UCase(attrib.Name) = "REQUESTEDDELIVERYDATE" Then
                                                                If j1 = 0 Then
                                                                Else
                                                                    ReDim Preserve arrDueDates(j1)
                                                                End If
                                                                arrDueDates(j1) = attrib.Value
                                                            End If
                                                        Next  '  For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                    End If  '  If nodeOrdrRefr.Attributes.Count > 0 Then

                                                    If nodeOrdrRefr.ChildNodes.Count > 0 Then
                                                        Dim strLineItemChildName As String = ""
                                                        Dim strNodeChildItemIdName As String = ""
                                                        Dim strNodeChildMoneyName As String = ""

                                                        'get Item numbers, price, currency and descriptoin 
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
                                                                Case "TAX"
                                                                    If j1 = 0 Then
                                                                    Else
                                                                        ReDim Preserve arrTax(j1)
                                                                    End If
                                                                    arrTax(j1) = ChildItemNode.InnerText
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
                                                                                                        ReDim Preserve arrVendor(j1)
                                                                                                        ReDim Preserve arrVendPartNum(j1)
                                                                                                        ReDim Preserve arrMfgId(j1)
                                                                                                        ReDim Preserve arrMfgPartNum(j1)
                                                                                                        ReDim Preserve arrQuoteLineNum(j1)
                                                                                                    End If
                                                                                                    arrUnitPrice(j1) = NodeChildIUnitPrice.InnerText
                                                                                                    arrVendor(j1) = " "
                                                                                                    arrVendPartNum(j1) = " "
                                                                                                    arrMfgId(j1) = " "
                                                                                                    arrMfgPartNum(j1) = " "
                                                                                                    arrQuoteLineNum(j1) = " "
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

                                                                                Case "UNITOFMEASURE"
                                                                                    If j1 = 0 Then
                                                                                    Else
                                                                                        ReDim Preserve arrUOMs(j1)
                                                                                    End If
                                                                                    arrUOMs(j1) = NodeChildItemID.InnerText




                                                                                    '    'below is the code for the Charge Code (Material Group for KLA)
                                                                                    '    '<ManufacturerPartID>33</ManufacturerPartID> - their field
                                                                                    '    ' ISA_CUST_CHARGE_CD - our Interface line field
                                                                                    'Case "MANUFACTURERPARTID"
                                                                                    '    If j1 = 0 Then
                                                                                    '    Else
                                                                                    '        ReDim Preserve arrMatGroupTax(j1)
                                                                                    '    End If
                                                                                    '    arrMatGroupTax(j1) = NodeChildItemID.InnerText



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
                                            m_logger.WriteInformationLog(rtn & " :: Start creating Flat File ")
                                           
                                            Dim writerLnsMagn As StreamWriter
                                            Dim strPathForLM As String = ""
                                            Dim strSrchFileName As String = ""
                                            Dim bIsThereAnyData As Boolean = False
                                            Dim iLM As Integer = 0

                                            If arrUnitPrice.Length > 0 Then
                                                If Trim(arrUnitPrice(0)) <> "" Then
                                                    bIsThereAnyData = True
                                                    'create Flat file only if data supplied
                                                    strSdiBusUnit = UCase(Trim(strSdiBusUnit))

                                                    strSrchFileName = strSdiBusUnit & "_" & strOrderNum.ToUpper & "_" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

                                                    strPathForLM = "C:\LMIn\FlatFile\" & strSrchFileName
                                                    writerLnsMagn = New StreamWriter(strPathForLM)

                                                    ' need: strQuoteID, arrQuoteLineNum
                                                    For iLM = 0 To arrUnitPrice.Length - 1
                                                        'add headers
                                                        writerLnsMagn.WriteLine("BUSUNIT" & "~" & "POID" & "~" & "POLINENUM" & "~" & "QUOTEID" & "~" & "QUOTELINENUM" & "~" & "PRODUCT_ID" & "~" & "DESCRIPTION" & "~" & "QUANTITY" & "~" & "UOM" & "~" & "ITEM_PRICE" & "~" & "CURRENCY" & "~" & "VENDOR" & "~" & "VENDORPARTNUM" & "~" & "MANFID" & "~" & "MANFPARTNUM" & "~" & "UNSPSC" & "~" & "DUEDATE" & "~" & "TAX")
                                                        'add data
                                                        writerLnsMagn.WriteLine(strSdiBusUnit & "~" & strOrderNum & "~" & arrLineNums(iLM) & "~" & strQuoteID & "~" & arrQuoteLineNum(iLM) & "~" & arrSupplPartIDs(iLM) & "~" & arrDescr(iLM) & "~" & arrLineQtys(iLM) & "~" & arrUOMs(iLM) & "~" & arrUnitPrice(iLM) & "~" & arrCurrency(iLM) & "~" & arrVendor(iLM) & "~" & arrVendPartNum(iLM) & "~" & arrMfgId(iLM) & "~" & arrMfgPartNum(iLM) & "~" & " " & "~" & arrDueDates(iLM) & "~" & arrTax(iLM))

                                                    Next  ' For iLM = 0 To Len(arrUnitPrice) - 1

                                                    writerLnsMagn.Flush()
                                                    writerLnsMagn.Dispose()
                                                    writerLnsMagn = Nothing

                                                End If
                                            End If

                                            If Not bIsThereAnyData Then
                                                'send error email
                                                strXMLError = "Error - no data sent. File name: " & aFiles(I).Name
                                            End If
                                            
                                        End If '  If nodeOrdRequest.ChildNodes.Count > 0 Then - node "ORDERREQUEST"
                                    Case Else
                                        'do nothing
                                End Select   '  Select Case strFirstChildName    ' ORDERREQUEST

                            End If  '  If nodeOrdConf.ChildNodes.Count > 0 Then

                        End If  '  If Trim(strXMLError) = "" Then  ' 1st level inner 2nd time

                    End If  '  If Trim(strXMLError) = "" Then ' - outer

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
                            File.Copy(aFiles(I).FullName, "C:\LMIn\BadXML\" & aFiles(I).Name, True)
                            File.Delete(aFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\LMIn\BadXML\" & aFiles(I).Name)
                        End If

                        If Trim(strXMLError) <> "" Then

                        Else
                            strXMLError = "File is not processed OK"
                        End If
                        
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\LMIn\XMLINProcessed\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\LMIn\XMLINProcessed\" & aFiles(I).Name)
                        
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
            File.Copy(aFiles(I).FullName, "C:\LMIn\BadXML\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\LMIn\BadXML\" & aFiles(I).Name)

            bolError = True
        End Try

        Return bolError

    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "LyonsPoProcessing.SendEmail"
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

        Dim strAddSDILogo As String = ""
        strEmailBody = ""
        strAddSDILogo = "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >Lyons From PO To Flat File</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>Lyons From PO To Flat File has completed with "
        If bolWarning = True Then
            strEmailBody &= "warnings,"
            strEmailSubject = " Lyons From PO To Flat File Warning"
        Else
            strEmailBody &= "errors;"
            strEmailSubject = " Lyons From PO To Flat File Error"
        End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Dim strErrListForSFTP As String = ""
        Try

            sInfoErr &= " XML file name(s) are below.</td></tr>"
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
                    Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
                    If arrErrFiles1.Length > 0 Then
                        For i1 As Integer = 0 To arrErrFiles1.Length - 1
                            sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
                            strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
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

        Dim strEmailBodyEnd As String = "</body></html>"

        strEmailBody = strAddSDILogo & strEmailBody
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

            SendLogger(strEmailSubject, strEmailBody, "LYONSPOTOFLAT", "Mail", strEmailTo, strEmailCC, strEmailBcc)
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
