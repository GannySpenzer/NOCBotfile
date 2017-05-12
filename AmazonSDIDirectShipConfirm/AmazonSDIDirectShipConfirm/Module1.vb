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
    Dim logpath As String = "C:\AmazonSdiDirectIn\LOGS\UpdAmazonOrdrConfXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\AmazonSdiDirectIn\LOGS\ErredSQLsAmazonOrdrConf" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")
    Dim strOverride As String
    Dim bolWarning As Boolean = False
    Dim strVendorId As String = "0000039777"  '  "0000041491"  '   "0000039777"  '  AMAZON ID
    Dim strSiteBu As String = "ISA00"

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
            logpath = sLogPath & "\UpdAmazonOrdrConfXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        'process received info
        Call ProcessAmazonShipConfInfo()

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

    Private Sub ProcessAmazonShipConfInfo()

        Dim rtn As String = "AmazonOrdrConfirm.ProcessAmazonShipConfInfo"
        Dim bError As Boolean = False

        Console.WriteLine("Start Amazon SDI Direct process Order Confirm XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Process of Amazon SDI Direct process Order Confirm Inbound")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim sInputDir As String = "\\ims\SDIWebProcessorsXMLFiles\OrderConfirmAmazon"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = "\\ims\SDIWebProcessorsXMLFiles\OrderConfirmAmazon"
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
                    If aSrcFiles(I).Name.Length > Len("OrderConfirm") - 1 Then
                        strSrcFileName = UCase(aSrcFiles(I).Name)
                        If strSrcFileName.StartsWith("ORDERCONFIRM") Then
                            File.Copy(aSrcFiles(I).FullName, "C:\AmazonSdiDirectIn\XMLIN\OrderConfirm\" & aSrcFiles(I).Name, True)
                            File.Delete(aSrcFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\XMLIN\OrderConfirm\" & aSrcFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\AmazonSdiDirectIn\XMLIN\OrderConfirm\ " & "...")
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
        '// Amazon SDI Direct process Order Confirm: copy of Supplier Portal PO Confirmation process
        '// ***

        Dim bolError As Boolean = False

        Try
            bolError = AmazonSDIDirectOrdrConfirm()
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

        m_logger.WriteInformationLog(rtn & " :: End of Amazon SDI Direct process Order Confirm")

    End Sub

    Private Function AmazonSDIDirectOrdrConfirm() As Boolean
        Dim bolError As Boolean = False
        Dim rtn As String = "AmazonOrdrConfirm.AmazonSDIDirectOrdrConfirm"

        Console.WriteLine("Start of the Amazon SDI Direct process Order Confirm: copy of Supplier Portal PO Confirmation process")
        Console.WriteLine("")

        m_logger.WriteInformationLog(rtn & " :: Start of the Amazon SDI Direct process Order Confirm: copy of Supplier Portal PO Confirmation process")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("C:\AmazonSdiDirectIn\XMLIN\OrderConfirm\")
        Dim strFiles As String = ""
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bLineError As Boolean = False
        Dim strErrorFromHeaderComments As String = ""
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
                            File.Move(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\OrderConfirm\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex24 As Exception
                            myLoggr1.WriteErrorLog(rtn & " :: ** ")
                            myLoggr1.WriteErrorLog(rtn & " :: Error (moving file to BadXML\OrderConfirm\ folder): " & ex24.Message & " in file " & aFiles(I).Name)
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
                    End If  '  If Trim(strXMLError) = "" Then

                    'start parsing XML file
                    If Trim(strXMLError) = "" Then

                        Dim nodeOrdConf As XmlNode = root.LastChild()

                        If nodeOrdConf.ChildNodes.Count > 0 Then
                            Dim strFirstChildName As String = nodeOrdConf.FirstChild.Name.ToUpper
                            ' CONFIRMATIONREQUEST
                            Select Case strFirstChildName
                                Case "CONFIRMATIONREQUEST"
                                    Dim iCnt As Integer = 0
                                    Dim j1 As Integer = 0
                                    Dim arrLineNums(0) As String
                                    arrLineNums(0) = ""
                                    Dim arrLineQtys(0) As String
                                    arrLineQtys(0) = ""
                                    Dim arrUoms(0) As String
                                    arrUoms(0) = ""
                                    Dim strOrderNum As String = ""
                                    Dim strOrderDate As String = ""
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
                                        strErrorFromHeaderComments = ""
                                        For iCnt = 0 To nodeConfReqst.ChildNodes.Count - 1
                                            Dim strNodeName1 As String = UCase(nodeConfReqst.ChildNodes(iCnt).Name)
                                            'header info
                                            If strNodeName1 = "CONFIRMATIONHEADER" Then
                                                'get <comments> node - this is order problem description
                                                Dim nodeConfHeader As XmlNode = nodeConfReqst.ChildNodes(iCnt)
                                                ' it could be NOT present - so Try/Catch
                                                Try
                                                    If nodeConfHeader.ChildNodes.Count > 0 Then
                                                        Dim strChldNodeNameCF1 As String = ""
                                                        For Each ChildItemNode As XmlNode In nodeConfHeader.ChildNodes()
                                                            strChldNodeNameCF1 = UCase(ChildItemNode.Name)
                                                            Select Case strChldNodeNameCF1
                                                                Case "COMMENTS"
                                                                    strErrorFromHeaderComments = ChildItemNode.InnerText
                                                                    Exit For
                                                                Case Else
                                                                    strErrorFromHeaderComments = ""
                                                            End Select
                                                        Next  '  For Each ChildItemNode As XmlNode In nodeConfHeader.ChildNodes()
                                                    End If  '  If nodeConfHeader.ChildNodes.Count > 0 Then
                                                Catch ex As Exception
                                                    strErrorFromHeaderComments = ""
                                                End Try
                                            End If
                                            If strNodeName1 = "ORDERREFERENCE" Then
                                                Dim nodeOrdrRefr As XmlNode = nodeConfReqst.ChildNodes(iCnt)
                                                If nodeOrdrRefr.Attributes.Count > 0 Then
                                                    'get attribute orderID value
                                                    For Each attrib As XmlAttribute In nodeOrdrRefr.Attributes()
                                                        If UCase(attrib.Name) = "ORDERID" Then
                                                            strOrderNum = attrib.Value
                                                        End If
                                                        If UCase(attrib.Name) = "ORDERDATE" Then
                                                            strOrderDate = attrib.Value
                                                        End If
                                                    Next
                                                End If
                                            End If  '  If strNodeName1 = "ORDERREFERENCE" Then

                                            'line info
                                            If strNodeName1 = "CONFIRMATIONITEM" Then
                                                Dim nodeConfItem1 As XmlNode = nodeConfReqst.ChildNodes(iCnt)
                                                Dim bContinue As Boolean = True
                                                If nodeConfItem1.ChildNodes.Count > 0 Then
                                                    'get Status type ("detail"), UOM, 
                                                    Dim strChldNodeName1 As String = ""
                                                    For Each ChildItemNode As XmlNode In nodeConfItem1.ChildNodes()
                                                        strChldNodeName1 = ChildItemNode.Name
                                                        Select Case strChldNodeName1
                                                            Case "ConfirmationStatus"
                                                                If ChildItemNode.Attributes.Count > 0 Then
                                                                    'get attribute type value
                                                                    For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                        If UCase(attrib.Name) = "TYPE" Then
                                                                            If UCase(attrib.Value) = "DETAIL" Then
                                                                            Else
                                                                                bContinue = False
                                                                                strXMLError = strXMLError & "'ConfirmationStatus' node 'Type' attribute is not 'Detail' (value is '" & attrib.Value & "') for Order No: " & strOrderNum & " ; " & vbCrLf
                                                                                strErrorFromHeaderComments = Replace(strErrorFromHeaderComments, ",", " ")
                                                                                If Trim(strErrorFromHeaderComments) <> "" Then
                                                                                    strErrorFromHeaderComments = Trim(strErrorFromHeaderComments)
                                                                                    strXMLError = strXMLError & " Error: " & strErrorFromHeaderComments & " ; " & vbCrLf
                                                                                End If
                                                                                Exit For
                                                                            End If
                                                                        End If
                                                                    Next
                                                                Else
                                                                    bContinue = False
                                                                    Exit For
                                                                End If
                                                                If bContinue Then
                                                                    '' get order confirmID
                                                                    'If ChildItemNode.ChildNodes.Count > 0 Then
                                                                    '    Dim strConfStatusChldname As String = ""
                                                                    '    For Each ConfStatusChild In ChildItemNode
                                                                    '        strConfStatusChldname = ConfStatusChild.Name
                                                                    '        Select Case strConfStatusChldname
                                                                    '            Case "Comments"
                                                                    '             ' if attribute type="confirmID"
                                                                    '             ' <confirmID> = ConfStatusChild.InnerText
                                                                    '            Case Else
                                                                    '                'do nothing
                                                                    '        End Select
                                                                    '    Next  '  For Each ConfStatusChild In ChildItemNode
                                                                    '    If Not bContinue Then
                                                                    '        Exit For ' this line is rejected
                                                                    '    End If
                                                                    'Else
                                                                    '    bContinue = False
                                                                    '    Exit For
                                                                    'End If
                                                                End If

                                                            Case "UnitOfMeasure"
                                                                If j1 = 0 Then
                                                                Else
                                                                    ReDim Preserve arrUoms(j1)
                                                                End If
                                                                arrUoms(j1) = ChildItemNode.InnerText
                                                            Case Else
                                                                'do nothing
                                                        End Select
                                                    Next  '  For Each ChildItemNode As XmlNode In nodeConfItem1.ChildNodes()
                                                Else
                                                    bContinue = False
                                                End If  '  If nodeConfItem1.ChildNodes.Count > 0 Then
                                                If bContinue Then
                                                    'get line # & Qty and put them in arrays
                                                    If nodeConfItem1.Attributes.Count > 0 Then
                                                        For Each attrib As XmlAttribute In nodeConfItem1.Attributes()
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
                                                        Next  '  For Each attrib As XmlAttribute In nodeConfItem1.Attributes()
                                                    Else
                                                        bContinue = False
                                                    End If  '  If nodeConfItem1.Attributes.Count > 0 Then
                                                    If bContinue Then
                                                        j1 = j1 + 1
                                                    End If
                                                Else
                                                    strXMLError = strXMLError & "'ConfirmationStatus' node doesn't have all necessary info for Order No: " & strOrderNum & vbCrLf
                                                End If  '   If bContinue Then
                                            End If  '  If strNodeName1 = "CONFIRMATIONITEM" Then
                                        Next  '  For iCnt = 0 To nodeConfReqst.ChildNodes.Count - 1
                                        If j1 = 0 Then
                                            'empty line items - get out
                                            strXMLError = strXMLError & "Empty/rejected 'CONFIRMATIONITEM' for Order No: " & strOrderNum & vbCrLf

                                        End If
                                        myLoggr1.WriteErrorLog(rtn & " :: " & strXMLError)
                                        If Trim(strXMLError) = "" Then
                                            'writing to Oracle tables here
                                            Dim intQueueInst As Integer
                                            intQueueInst = getNextQueueInst()
                                            If intQueueInst = 0 Then
                                                strXMLError = "Error getting queue number for Order No: " & strOrderNum
                                            Else
                                                ' continue writing to Oracle tables

                                                'get Vendor ID based on strOrderNum
                                                strVendorId = GetVendorId(strOrderNum)

                                                Dim bolErrM1 As Boolean = False
                                                bolErrM1 = createECConfirmation(intQueueInst, strVendorId, strOrderNum, strSiteBu, strOrderDate, arrLineNums, arrLineQtys, arrUoms)
                                                If bolErrM1 Then
                                                    strXMLError = "Error updating EC tables for Order No: " & strOrderNum
                                                Else
                                                    strXMLError = ""
                                                End If
                                            End If
                                        End If
                                    Else
                                        strXMLError = "Empty node 'ConfirmationRequest'"
                                    End If  '  If nodeConfReqst.ChildNodes.Count > 0 Then
                                Case Else
                                    strXMLError = "Unexpected node name: " & strFirstChildName
                            End Select
                        Else
                            strXMLError = "Empty node 'Request'"
                        End If '  If nodeOrdConf.ChildNodes.Count > 0 Then
                    End If  ' If Trim(strXMLError) = "" Then
                    ' if there's an error, capture the filename of the XML and corresponding error message
                    If Trim(strXMLError) <> "" Or bolError Then
                        bolError = True
                        If Trim(m_arrXMLErrFiles) = "" Then
                            m_arrXMLErrFiles = aFiles(I).Name
                        Else
                            m_arrXMLErrFiles &= "," & aFiles(I).Name
                        End If
                        If Trim(strXMLError) <> "" Then
                            'If Len(strXMLError) > 250 Then
                            '    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                            'End If
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
                        File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\OrderConfirm\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\BadXML\OrderConfirm\" & aFiles(I).Name)
                    Else

                        'move file to XMLInProcessed folder
                        File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\XMLINProcessed\OrderConfirm\" & aFiles(I).Name, True)
                        File.Delete(aFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\XMLINProcessed\OrderConfirm\" & aFiles(I).Name)

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
                'If Len(strXMLError) > 250 Then
                '    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                'End If
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
            File.Copy(aFiles(I).FullName, "C:\AmazonSdiDirectIn\BadXML\OrderConfirm\" & aFiles(I).Name, True)
            File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\AmazonSdiDirectIn\BadXML\OrderConfirm\" & aFiles(I).Name)

            Return True

        End Try

        Return bolError

    End Function

    Private Function GetVendorId(ByVal strOrderNum As String) As String
        Dim strVendorIdByOrder As String = ""
        Dim strSQLString As String = ""

        strSQLString = "select VENDOR_ID from SYSADM.PS_PO_DISPATCHED where PO_ID = '" & strOrderNum & "'" & vbCrLf
        Try
            strVendorIdByOrder = ORDBAccess.GetScalar(strSQLString, connectOR)
        Catch ex As Exception
            strVendorIdByOrder = "0000039777"
        End Try

        If Trim(strVendorIdByOrder) = "" Then
            strVendorIdByOrder = "0000039777"
        End If
        Return strVendorIdByOrder
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
            Dim dtrPOds As DataSet = ORDBAccess.GetAdapter(strSQLString, connectOR)
            strShipToId = dtrPOds.Tables(0).Rows(0).Item(0)
            
        Catch objException As Exception
            Return " "

        End Try

        Return strShipToId

    End Function

    Private Function getpoinfo(ByVal ponum As String, ByVal vendor As String, ByVal strPoSiteBu As String) As DataSet

        Dim strSQLString As String

        strSQLString = "SELECT TO_CHAR(A.PO_DT,'MM-DD-YYYY') as PODT, B.LINE_NBR," & vbCrLf & _
                    " C.SCHED_NBR," & vbCrLf & _
                    " B.INV_ITEM_ID," & vbCrLf & _
                    " B.MFG_ITM_ID," & vbCrLf & _
                    " B.ITM_ID_VNDR," & vbCrLf & _
                    " B.DESCR254_MIXED," & vbCrLf & _
                    " C.QTY_PO," & vbCrLf & _
                    " B.UNIT_OF_MEASURE," & vbCrLf & _
                    " D.PRICE_PO," & vbCrLf & _
                    " TO_CHAR(D.DUE_DT,'MM/DD/YYYY') as DUE_DT," & vbCrLf & _
                    " B.MFG_ID," & vbCrLf & _
                    " D.MERCHANDISE_AMT," & vbCrLf & _
                    " D.CURRENCY_CD," & vbCrLf & _
                    " D.FREIGHT_TERMS," & vbCrLf & _
                    " D.SHIP_TYPE_ID," & vbCrLf & _
                    " A.PO_REF," & vbCrLf & _
                    " A.PYMNT_TERMS_CD," & vbCrLf & _
                    " A.CNTCT_SEQ_NUM," & vbCrLf & _
                    " A.BILL_LOCATION," & vbCrLf & _
                    " A.TAX_EXEMPT," & vbCrLf & _
                    " A.TAX_EXEMPT_ID," & vbCrLf & _
                    " A.CURRENCY_CD as CURRENCY_CD_HDR," & vbCrLf & _
                    " A.RT_TYPE," & vbCrLf & _
                    " F.UPC_ID," & vbCrLf & _
                    " X.IN_PROCESS_FLG," & vbCrLf & _
                    " C.REQ_ID, C.REQ_LINE_NBR,A.BUYER_ID" & vbCrLf & _
                    " FROM PS_PO_HDR A," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB C," & vbCrLf & _
                    " PS_PO_LINE_SHIP D," & vbCrLf & _
                    " PS_PO_LINE B," & vbCrLf & _
                    " PS_INV_ITEMS F," & vbCrLf & _
                    " PS_ISA_QUICK_REQ_H X" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                    " AND A.PO_ID = '" & ponum & "'" & vbCrLf & _
                    " AND A.VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.VENDOR_ID = '" & vendor & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf & _
                    " AND B.PO_ID = C.PO_ID" & vbCrLf & _
                    " AND B.CANCEL_STATUS != 'X'" & vbCrLf & _
                    " AND C.DISTRIB_LN_STATUS != 'X'" & vbCrLf & _
                    " AND B.LINE_NBR = C.LINE_NBR" & vbCrLf & _
                    " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                            " FROM SYSADM.PS_PO_LINE_EC LEC" & vbCrLf & _
                            " WHERE LEC.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                            " AND C.PO_ID = LEC.PO_ID" & vbCrLf & _
                            " AND C.LINE_NBR = LEC.LINE_NBR)" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.PO_ID = D.PO_ID" & vbCrLf & _
                    " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf & _
                    " AND C.SCHED_NBR = D.SCHED_NBR" & vbCrLf & _
                    " AND D.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND D.PO_ID = B.PO_ID" & vbCrLf & _
                    " AND D.LINE_NBR = B.LINE_NBR" & vbCrLf & _
                    " AND B.ITM_SETID = F.SETID" & vbCrLf & _
                    " AND B.INV_ITEM_ID = F.INV_ITEM_ID" & vbCrLf & _
                    " AND F.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED" & vbCrLf & _
                    " WHERE F.SETID = B_ED.SETID" & vbCrLf & _
                    " AND F.INV_ITEM_ID = B_ED.INV_ITEM_ID" & vbCrLf & _
                    " AND B_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND '" & strPoSiteBu & "' = X.BUSINESS_UNIT(+)" & vbCrLf & _
                    " AND C.REQ_ID = X.REQ_ID(+)" & vbCrLf & _
                    " UNION" & vbCrLf & _
                    " SELECT TO_CHAR(G.PO_DT,'MM-DD-YYYY') as PODT,J.LINE_NBR," & vbCrLf & _
                    " H.SCHED_NBR," & vbCrLf & _
                    " J.INV_ITEM_ID," & vbCrLf & _
                    " J.MFG_ITM_ID," & vbCrLf & _
                    " J.ITM_ID_VNDR," & vbCrLf & _
                    " J.DESCR254_MIXED," & vbCrLf & _
                    " H.QTY_PO," & vbCrLf & _
                    " J.UNIT_OF_MEASURE," & vbCrLf & _
                    " I.PRICE_PO," & vbCrLf & _
                    " TO_CHAR(I.DUE_DT,'MM/DD/YYYY') as DUE_DT," & vbCrLf & _
                    " J.MFG_ID," & vbCrLf & _
                    " I.MERCHANDISE_AMT," & vbCrLf & _
                    " I.CURRENCY_CD," & vbCrLf & _
                    " I.FREIGHT_TERMS," & vbCrLf & _
                    " I.SHIP_TYPE_ID," & vbCrLf & _
                    " G.PO_REF," & vbCrLf & _
                    " G.PYMNT_TERMS_CD," & vbCrLf & _
                    " G.CNTCT_SEQ_NUM," & vbCrLf & _
                    " G.BILL_LOCATION," & vbCrLf & _
                    " G.TAX_EXEMPT," & vbCrLf & _
                    " G.TAX_EXEMPT_ID," & vbCrLf & _
                    " G.CURRENCY_CD as CURRENCY_CD_HDR," & vbCrLf & _
                    " G.RT_TYPE," & vbCrLf & _
                    " ' ' as UPC_ID," & vbCrLf & _
                    " Y.IN_PROCESS_FLG," & vbCrLf & _
                    " H.REQ_ID, H.REQ_LINE_NBR,G.BUYER_ID" & vbCrLf & _
                    " FROM PS_PO_HDR G," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB H," & vbCrLf & _
                    " PS_PO_LINE_SHIP I," & vbCrLf & _
                    " PS_PO_LINE J," & vbCrLf & _
                    " PS_ISA_QUICK_REQ_H Y" & vbCrLf & _
                    " WHERE G.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                    " AND G.PO_ID = '" & ponum & "'" & vbCrLf & _
                    " AND G.VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                    " AND G.VENDOR_ID = '" & vendor & "'" & vbCrLf & _
                    " AND G.BUSINESS_UNIT = J.BUSINESS_UNIT" & vbCrLf & _
                    " AND G.PO_ID = J.PO_ID" & vbCrLf & _
                    " AND J.BUSINESS_UNIT = H.BUSINESS_UNIT" & vbCrLf & _
                    " AND J.PO_ID = H.PO_ID" & vbCrLf & _
                    " AND J.CANCEL_STATUS != 'X'" & vbCrLf & _
                    " AND H.DISTRIB_LN_STATUS != 'X'" & vbCrLf & _
                    " AND J.LINE_NBR = H.LINE_NBR" & vbCrLf & _
                    " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                            " FROM SYSADM.PS_PO_LINE_EC LEC" & vbCrLf & _
                            " WHERE LEC.BUSINESS_UNIT = '" & strPoSiteBu & "'" & vbCrLf & _
                            " AND H.PO_ID = LEC.PO_ID" & vbCrLf & _
                            " AND H.LINE_NBR = LEC.LINE_NBR)" & vbCrLf & _
                    " AND H.BUSINESS_UNIT = I.BUSINESS_UNIT" & vbCrLf & _
                    " AND H.PO_ID = I.PO_ID" & vbCrLf & _
                    " AND H.LINE_NBR = I.LINE_NBR" & vbCrLf & _
                    " AND H.SCHED_NBR = I.SCHED_NBR" & vbCrLf & _
                    " AND I.BUSINESS_UNIT = J.BUSINESS_UNIT" & vbCrLf & _
                    " AND I.PO_ID = J.PO_ID" & vbCrLf & _
                    " AND I.LINE_NBR = J.LINE_NBR" & vbCrLf & _
                    " AND NOT EXISTS (SELECT 'X' FROM PS_INV_ITEMS K" & vbCrLf & _
                    " WHERE J.ITM_SETID = K.SETID" & vbCrLf & _
                    " AND J.INV_ITEM_ID = K.INV_ITEM_ID" & vbCrLf & _
                    " AND K.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED" & vbCrLf & _
                    " WHERE K.SETID = B_ED.SETID" & vbCrLf & _
                    " AND K.INV_ITEM_ID = B_ED.INV_ITEM_ID" & vbCrLf & _
                    " AND B_ED.EFFDT <= SYSDATE))" & vbCrLf & _
                    " AND '" & strPoSiteBu & "' = Y.BUSINESS_UNIT(+)" & vbCrLf & _
                    " AND H.REQ_ID = Y.REQ_ID(+)" & vbCrLf & _
                    " ORDER BY LINE_NBR, SCHED_NBR"

        Try
            Dim POdataSet As DataSet = ORDBAccess.GetAdapter(strSQLString, connectOR)
            Return POdataSet
        Catch objException As Exception
            Return Nothing
        End Try


    End Function

    Private Function createECConfirmation(ByVal intQueueInst As Integer, ByVal strVendorId As String, ByVal strPoId As String, _
                ByVal strPoSiteBu As String, ByVal strOrderDate As String, ByVal arrLineNums() As String, _
                ByVal arrLineQtys() As String, ByVal arrUoms() As String) As Boolean

        Dim bolUpdateFailed As Boolean = False
        Try
            Dim I As Integer = 0
            Dim strSQLstring As String = ""
            Dim strStatus As String = ""
            Dim bolChanged As Boolean = False
            Dim strDescr As String = ""
            Dim strMfgID As String = ""
            Dim strMfgItemID As String = ""
            Dim strVndItemID As String = ""
            Dim strInvItemID As String = ""
            Dim strUPCID As String = ""
            Dim decQTY As Decimal
            Dim decPrice As Decimal
            Dim strDueDt As Date
            Dim strDueDtOld As Date
            Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
            Dim dteNowd As Date = Now().ToString("d")
            Dim rowsaffected As Integer
            Dim strShipToId As String = " "
            Dim strLineNum As String = ""
            Dim strUom1 As String = ""
            Dim strSchedNum As String = ""
            Dim strCurrCd As String = ""
            Dim strFrtTerms As String = ""
            Dim strShipTypeId As String = ""

            strShipToId = getshipto(strPoId, strVendorId)

            strStatus = "AD"

            strSQLstring = "INSERT INTO PS_ECQUEUE" & vbCrLf & _
                        " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf & _
                        " ECBUSDOCID, ECQUEUESTATUS, ECACTIONCD," & vbCrLf & _
                        " ECDRIVERDTTM, ECAPPDTTM, BUSINESS_UNIT," & vbCrLf & _
                        " ECENTITYCD_BU, ECCUSTVNDRVAL, ECENTITYCD_EXT," & vbCrLf & _
                        " EC_QUEUE_CNTL_NBR)" & vbCrLf & _
                        " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf & _
                        " " & intQueueInst & ", 'L', ' '," & vbCrLf & _
                        " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'), '','" & strPoSiteBu & "'," & vbCrLf & _
                        " 'POBU', '" & strVendorId & "', 'VNDR'," & vbCrLf & _
                        " ' ')"
            rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
            If rowsaffected = 0 Then
                bolUpdateFailed = True
            End If

            Dim dsPOdata As DataSet = Nothing

            Dim bLineExists As Boolean = False
            If Not bolUpdateFailed Then
                dsPOdata = getpoinfo(strPoId, strVendorId, strPoSiteBu)
                Dim bIsDataPresent As Boolean = False
                If Not dsPOdata Is Nothing Then
                    If dsPOdata.Tables.Count > 0 Then
                        If dsPOdata.Tables(0).Rows.Count > 0 Then
                            bIsDataPresent = True
                        End If
                    End If
                End If

                If bIsDataPresent Then
                    ' "SELECT TO_CHAR(A.PO_DT,'MM-DD-YYYY') as PODT, B.LINE_NBR," & vbCrLf & _
                    '" C.SCHED_NBR," & vbCrLf & _
                    '" B.INV_ITEM_ID," & vbCrLf & _
                    '" B.MFG_ITM_ID," & vbCrLf & _
                    '" B.ITM_ID_VNDR," & vbCrLf & _
                    '" B.DESCR254_MIXED," & vbCrLf & _
                    '" C.QTY_PO," & vbCrLf & _
                    '" B.UNIT_OF_MEASURE," & vbCrLf & _
                    '" D.PRICE_PO," & vbCrLf & _
                    '" TO_CHAR(D.DUE_DT,'MM/DD/YYYY') as DUE_DT," & vbCrLf & _
                    '" B.MFG_ID," & vbCrLf & _
                    '" D.MERCHANDISE_AMT," & vbCrLf & _
                    '" D.CURRENCY_CD," & vbCrLf & _
                    '" D.FREIGHT_TERMS," & vbCrLf & _
                    '" D.SHIP_TYPE_ID," & vbCrLf & _
                    '" A.PO_REF," & vbCrLf & _
                    '" A.PYMNT_TERMS_CD," & vbCrLf & _
                    '" A.CNTCT_SEQ_NUM," & vbCrLf & _
                    '" A.BILL_LOCATION," & vbCrLf & _
                    '" A.TAX_EXEMPT," & vbCrLf & _
                    '" A.TAX_EXEMPT_ID," & vbCrLf & _
                    '" A.CURRENCY_CD as CURRENCY_CD_HDR," & vbCrLf & _
                    '" A.RT_TYPE," & vbCrLf & _
                    '" F.UPC_ID," & vbCrLf & _
                    '" X.IN_PROCESS_FLG," & vbCrLf & _
                    '" C.REQ_ID, C.REQ_LINE_NBR,A.BUYER_ID" & vbCrLf & _

                    Dim strQtyMy3 As String = ""
                    Dim strPriceMy3 As String = ""
                    Dim iLoc1 As Integer = -1
                    For I = 0 To dsPOdata.Tables(0).Rows.Count - 1
                        iLoc1 = -1

                        strFrtTerms = dsPOdata.Tables(0).Rows(I).Item("FREIGHT_TERMS")
                        strShipTypeId = dsPOdata.Tables(0).Rows(I).Item("SHIP_TYPE_ID")
                        strSchedNum = dsPOdata.Tables(0).Rows(I).Item("SCHED_NBR")
                        strCurrCd = dsPOdata.Tables(0).Rows(I).Item("CURRENCY_CD")
                        strLineNum = dsPOdata.Tables(0).Rows(I).Item("LINE_NBR")
                        strUom1 = dsPOdata.Tables(0).Rows(I).Item("UNIT_OF_MEASURE")
                        strMfgItemID = dsPOdata.Tables(0).Rows(I).Item("MFG_ITM_ID")
                        strVndItemID = dsPOdata.Tables(0).Rows(I).Item("ITM_ID_VNDR")
                        strInvItemID = dsPOdata.Tables(0).Rows(I).Item("INV_ITEM_ID")
                        strUPCID = dsPOdata.Tables(0).Rows(I).Item("UPC_ID")

                        If arrLineNums.Contains(strLineNum) Then
                            bLineExists = True
                            iLoc1 = Array.IndexOf(arrLineNums, strLineNum)

                            If iLoc1 > -1 Then
                                strQtyMy3 = arrLineQtys(iLoc1)
                                strUom1 = arrUoms(iLoc1)
                            Else
                                strQtyMy3 = dsPOdata.Tables(0).Rows(I).Item("QTY_PO")
                            End If
                            'strQtyMy3 = dsPOdata.Tables(0).Rows(I).Item("QTY_PO")
                            If Trim(strQtyMy3) = "" Then
                                decQTY = 0
                            Else
                                strQtyMy3 = Trim(strQtyMy3)
                                If IsNumeric(strQtyMy3) Then
                                    decQTY = CType(strQtyMy3, Decimal)
                                Else
                                    decQTY = 0
                                End If
                            End If

                            strPriceMy3 = dsPOdata.Tables(0).Rows(I).Item("PRICE_PO")
                            If Trim(strPriceMy3) = "" Then
                                decPrice = 0
                            Else
                                strPriceMy3 = Trim(strPriceMy3)
                                If IsNumeric(strPriceMy3) Then
                                    decPrice = CType(strPriceMy3, Decimal)
                                Else
                                    decPrice = 0
                                End If
                            End If

                            strDueDt = strOrderDate
                            strDueDtOld = dsPOdata.Tables(0).Rows(I).Item("DUE_DT")

                            strDescr = dsPOdata.Tables(0).Rows(I).Item("DESCR254_MIXED")
                            strDescr = Replace(strDescr, "'", "")
                            strVndItemID = Replace(strVndItemID, "'", "")
                            strInvItemID = Replace(strInvItemID, "'", "")
                            strMfgID = dsPOdata.Tables(0).Rows(I).Item("MFG_ID")
                            strMfgID = Replace(strMfgID, "'", "")
                            strMfgItemID = Replace(strMfgItemID, "'", "")
                            strUPCID = Replace(strUPCID, "'", "")

                            If Trim(strDescr) = "" Then
                                strDescr = " "
                            End If
                            If Trim(strMfgID) = "" Then
                                strMfgID = " "
                            End If
                            If Trim(strMfgItemID) = "" Then
                                strMfgItemID = " "
                            End If
                            If Trim(strVndItemID) = "" Then
                                strVndItemID = " "
                            End If
                            If Trim(strInvItemID) = "" Then
                                strInvItemID = " "
                            End If
                            If Trim(strUPCID) = "" Then
                                strUPCID = " "
                            End If

                            strSQLstring = "INSERT INTO PS_PO_LINE_EC" & vbCrLf & _
                                    " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf & _
                                    " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf & _
                                    " INV_ITEM_ID, DESCR254_MIXED, ITM_ID_VNDR," & vbCrLf & _
                                    " VNDR_CATALOG_ID, UNIT_OF_MEASURE, MFG_ID," & vbCrLf & _
                                    " CNTRCT_ID, CNTRCT_LINE_NBR, RFQ_ID," & vbCrLf & _
                                    " RFQ_LINE_NBR, ACK_STATUS, MFG_ITM_ID," & vbCrLf & _
                                    " UPC_ID)" & vbCrLf & _
                                    " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf & _
                                    " '" & strPoSiteBu & "', '" & strPoId & "', '" & strLineNum & "'," & vbCrLf & _
                                    " '" & strInvItemID & "'," & vbCrLf & _
                                    " '" & strDescr.ToUpper & "'," & vbCrLf & _
                                    " '" & strVndItemID & "'," & vbCrLf & _
                                    " ' '," & vbCrLf & _
                                    " '" & strUom1 & "'," & vbCrLf & _
                                    " '" & strMfgID & "'," & vbCrLf & _
                                    " ' ',0,' '," & vbCrLf & _
                                    " 0,'" & strStatus & "'," & vbCrLf & _
                                    " '" & strMfgItemID & "'," & vbCrLf & _
                                    " '" & strUPCID & " ')" & vbCrLf

                            rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                            If rowsaffected = 0 Then
                                bolUpdateFailed = True
                            End If

                            strSQLstring = "INSERT INTO PS_PO_LINE_SHIP_EC" & vbCrLf & _
                                    " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf & _
                                    " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf & _
                                    " SCHED_NBR, PRICE_PO, DUE_DT," & vbCrLf & _
                                    " DUE_TIME, SHIPTO_ID, QTY_PO," & vbCrLf & _
                                    " MERCHANDISE_AMT, CURRENCY_CD, FREIGHT_TERMS," & vbCrLf & _
                                    " SHIP_TYPE_ID)" & vbCrLf & _
                                    " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf & _
                                    " '" & strPoSiteBu & "', '" & strPoId & "', '" & strLineNum & "'," & vbCrLf & _
                                    " '" & strSchedNum & "'," & vbCrLf & _
                                    " " & decPrice & "," & vbCrLf & _
                                    " TO_DATE('" & strDueDt & "','MM/DD/YYYY')," & vbCrLf & _
                                    " '', '" & strShipToId & "'," & vbCrLf & _
                                    " " & decQTY & "," & vbCrLf & _
                                    " " & (decQTY * decPrice) & "," & vbCrLf & _
                                    " '" & strCurrCd & "'," & vbCrLf & _
                                    " '" & strFrtTerms & "'," & vbCrLf & _
                                    " '" & strShipTypeId & "')"

                            rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                            If rowsaffected = 0 Then
                                bolUpdateFailed = True
                            End If

                        End If  '  If arrLineNums.Contains(strLineNum) Then

                    Next  '  For I = 0 To dsPOdata.Tables(0).Rows.Count - 1

                    If bLineExists Then
                        Dim strPoRef As String = dsPOdata.Tables(0).Rows(0).Item("PO_REF")
                        Dim strPYMNT_TERMS_CD As String = dsPOdata.Tables(0).Rows(0).Item("PYMNT_TERMS_CD")
                        Dim strCNTCT_SEQ_NUM As String = dsPOdata.Tables(0).Rows(0).Item("CNTCT_SEQ_NUM")
                        Dim strBILL_LOCATION As String = dsPOdata.Tables(0).Rows(0).Item("BILL_LOCATION")
                        Dim strTAX_EXEMPT As String = dsPOdata.Tables(0).Rows(0).Item("TAX_EXEMPT")
                        Dim strTAX_EXEMPT_ID As String = dsPOdata.Tables(0).Rows(0).Item("TAX_EXEMPT_ID")
                        Dim strCURRENCY_CD_HDR As String = dsPOdata.Tables(0).Rows(0).Item("CURRENCY_CD_HDR")
                        Dim strRT_TYPE As String = dsPOdata.Tables(0).Rows(0).Item("RT_TYPE")

                        strSQLstring = "INSERT INTO PS_PO_HDR_EC" & vbCrLf & _
                                    " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf & _
                                    " BUSINESS_UNIT, PO_ID, CHNG_ORD_BATCH," & vbCrLf & _
                                    " PO_REF, PYMNT_TERMS_CD, CNTCT_SEQ_NUM," & vbCrLf & _
                                    " BILL_LOCATION, TAX_EXEMPT, TAX_EXEMPT_ID," & vbCrLf & _
                                    " CURRENCY_CD, RT_TYPE, ACKNOWLEGE_DT," & vbCrLf & _
                                    " ACK_STATUS, ACK_RECEIVED_DT, REVIEWED," & vbCrLf & _
                                    " REVIEWED_DT, OPRID)" & vbCrLf & _
                                    " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf & _
                                    " '" & strPoSiteBu & "', '" & strPoId & "',0," & vbCrLf & _
                                    " '" & strPoRef & "'," & vbCrLf & _
                                    " '" & strPYMNT_TERMS_CD & "'," & vbCrLf & _
                                    " '" & strCNTCT_SEQ_NUM & "'," & vbCrLf & _
                                    " '" & strBILL_LOCATION & "'," & vbCrLf & _
                                    " '" & strTAX_EXEMPT & "'," & vbCrLf & _
                                    " '" & strTAX_EXEMPT_ID & "'," & vbCrLf & _
                                    " '" & strCURRENCY_CD_HDR & "'," & vbCrLf & _
                                    " '" & strRT_TYPE & "'," & vbCrLf & _
                                    " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf & _
                                    " '" & strStatus & "'," & vbCrLf & _
                                    " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf & _
                                    " 'N','','ASNVEND')"

                        rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
                        If rowsaffected = 0 Then
                            bolUpdateFailed = True
                        End If

                    End If  '  If bLineExists Then
                Else
                    bolUpdateFailed = True
                End If  '  If bIsDataPresent Then

            End If  '  If Not bolUpdateFailed Then

            If Not bolUpdateFailed Then
                If Not bLineExists Then
                    bolUpdateFailed = True
                End If
            End If

            If bolUpdateFailed Then
                Try
                    strSQLstring = "DELETE FROM PS_ECQUEUE" & vbCrLf & _
                                    " WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                                    " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf & _
                                    " AND ECTRANSINOUTSW = 'I'" & vbCrLf

                    rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)

                Catch ex As Exception

                End Try

                Try
                    strSQLstring = "DELETE FROM PS_PO_LINE_EC" & vbCrLf & _
                         " WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                         " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf & _
                         " AND ECTRANSINOUTSW = 'I'" & vbCrLf


                    rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)

                Catch ex As Exception

                End Try

                Try
                    strSQLstring = "DELETE FROM PS_PO_LINE_SHIP_EC" & vbCrLf & _
                         " WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                         " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf & _
                         " AND ECTRANSINOUTSW = 'I'" & vbCrLf


                    rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)

                Catch ex As Exception

                End Try

                Try
                    strSQLstring = "DELETE FROM PS_PO_HDR_EC" & vbCrLf & _
                         " WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                         " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf & _
                         " AND ECTRANSINOUTSW = 'I'" & vbCrLf

                    rowsaffected = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)

                Catch ex As Exception

                End Try

            End If

        Catch exAll As Exception
            bolUpdateFailed = True
        End Try

        Return bolUpdateFailed
    End Function

    Private Function getNextQueueInst() As Integer
        Dim strsql As String

        strsql = "Update PS_ECQUEUEINST Set" & vbCrLf & _
                    "ECQUEUEINSTANCE = ECQUEUEINSTANCE + 1" & vbCrLf & _
                    "WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                    "AND ECTRANSINOUTSW = 'I'"

        Dim rowsaffected As Integer = ORDBAccess.ExecNonQuery(strsql, connectOR)
        If rowsaffected = 0 Then
            Return 0
        End If
        strsql = "SELECT ECQUEUEINSTANCE" & vbCrLf & _
                    "FROM PS_ECQUEUEINST" & vbCrLf & _
                    "WHERE ECTRANSID = 'PO ACK'" & vbCrLf & _
                    "AND ECTRANSINOUTSW = 'I'"
        Try
            getNextQueueInst = ORDBAccess.GetScalar(strsql, connectOR)
        Catch objException As Exception
            Return 0
        End Try
    End Function

    Private Sub SendEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "AmazonOrdrConfirm.SendEmail"

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

        Dim strEmailSubject As String = "Amazon SDI Direct process Order Confirm Error(s)"
        Dim strDbname As String = ""
        strDbname = Right(connectOR.ConnectionString, 4)
        If UCase(strDbname) = "STAR" Or UCase(strDbname) = "DEVL" Or UCase(strDbname) = "RPTG" Or UCase(strDbname) = "PLGR" Then
            strEmailSubject = " (TEST) Amazon SDI Direct process Order Confirm Error(s)"
        End If
        
        Dim strEmailBody As String = ""
        strEmailBody &= "<html><body><img src='https://www.sdiexchange.com/images/sdi_logo2017_yellow.jpg' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >Amazon SDI Direct process Order Confirm Error(s)</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>Amazon SDI Direct process Order Confirm has completed with error(s)"
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
            
            SendLogger(strEmailSubject, strEmailBody, "AMAZONORDRCONFIRMIN", "Mail", strEmailTo, strEmailCc, strEmailBcc)
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
