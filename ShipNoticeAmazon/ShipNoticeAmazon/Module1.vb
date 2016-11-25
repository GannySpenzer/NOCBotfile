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
                                    Dim arrLineNums(0) As String
                                    arrLineNums(0) = ""
                                    'parse and process
                                    Dim nodeConfReqst As XmlNode = nodeOrdConf.FirstChild()
                                    If nodeConfReqst.ChildNodes.Count > 0 Then
                                        j1 = 0
                                        ReDim arrLineNums(0)
                                        arrLineNums(0) = ""
                                        For iCnt = 0 To nodeConfReqst.ChildNodes.Count - 1
                                            Dim strNodeName1 As String = UCase(nodeConfReqst.ChildNodes(iCnt).Name)
                                            'header info
                                            If strNodeName1 = "SHIPNOTICEHEADER" Then
                                                'do nothing: right now we are not using this info
                                            End If  '  If strNodeName1 = "SHIPNOTICEHEADER" Then

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
                                                                    For Each attrib As XmlAttribute In ChildItemNode.Attributes()
                                                                        If UCase(attrib.Name) = "LINENUMBER" Then
                                                                            If j1 = 0 Then
                                                                            Else
                                                                                ReDim Preserve arrLineNums(j1)
                                                                            End If
                                                                            arrLineNums(j1) = attrib.Value
                                                                            j1 = j1 + 1
                                                                        End If
                                                                    Next  '  For Each attrib As XmlAttribute In ChildItemNode.Attributes()
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
                                            'write to Oracle tables - we have strOrderNum, strOrderDate and arrLineNums(j1)
                                            'NOT DONE YET
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
