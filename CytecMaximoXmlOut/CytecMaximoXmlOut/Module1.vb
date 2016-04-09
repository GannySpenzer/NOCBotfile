Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Web
Imports System.Xml
Imports System.IO
Imports System.Web.Mail
Imports Microsoft.jsCRIPT
Imports SDI.ApplicationLogger

Module Module1

    Private m_logger As appLogger = Nothing

    '
    ' NOTE: 
    '   These string will be overridden by the values in the app.config and then from command prompt param (if exists)
    '   - erwin
    '

    'Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\CytecMxmIn"
    'Dim logpath As String = "C:\CytecMxmIn\LOGS\CytecMaxXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim logpath1 As String = "C:\CytecMxmIn\LOGS\CytecMaxXMLloggerOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New System.Data.OleDb.OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")
    Dim strOverride As String

    'Private m_unixServer_IOH As String = "\\solaris2\PSSHARE\efi\I0256\outbound\IOH"
    'Private m_unixServer_ITM As String = "\\solaris2\PSSHARE\efi\I0256\outbound\ITM"
    'Private m_unixServer_REQCST As String = "\\solaris2\PSSHARE\efi\I0256\outbound\REQCST"

    Private m_onError_emailFrom As String = "TechSupport@sdi.com"
    Private m_onError_emailTo As String = "michael.randall@sdi.com;vitaly.rovensky@sdi.com;"
    Private m_onError_emailSubject As String = "Cytec Maximo XML OUT Error"

    Private m_Url_Cytec_Maximo As String = "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"

    '   "http://archibus.uncc.edu:8080/webtier/receivexml.jsp"

    Sub Main()

        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
        End Try
        If (cnString.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDb.OleDbConnection(cnString)
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
            'logpath = sLogPath & "\CytecMaxXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
            logpath1 = sLogPath & "\CytecMaxXMLloggerOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If
        '   (4) log level
        Dim sLogLevel As String = ""
        Try
            sLogLevel = My.Settings("logLevel").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogLevel.Length > 0) Then
            If (sLogLevel.IndexOf("VERB") > -1) Then
                logLevel = TraceLevel.Verbose
            ElseIf (sLogLevel.IndexOf("INFO") > -1) Or (sLogLevel.IndexOf("INFORMATION") > -1) Then
                logLevel = TraceLevel.Info
            ElseIf (sLogLevel.IndexOf("WARNING") > -1) Or (sLogLevel.IndexOf("WARN") > -1) Then
                logLevel = TraceLevel.Warning
            ElseIf (sLogLevel.IndexOf("ERROR") > -1) Then
                logLevel = TraceLevel.Error
            ElseIf (sLogLevel.IndexOf("OFF") > -1) Then
                logLevel = TraceLevel.Off
            Else
                ' don't change default
            End If
        End If
        ''   (5) unix server folder values
        'Dim srvFolder As String = ""
        'Try
        '    srvFolder = My.Settings("unixServer_IOH").ToString.Trim
        'Catch ex As Exception
        'End Try
        'If (srvFolder.Length > 0) Then
        '    m_unixServer_IOH = srvFolder
        'End If
        'srvFolder = ""
        'Try
        '    srvFolder = My.Settings("unixServer_ITM").ToString.Trim
        'Catch ex As Exception
        'End Try
        'If (srvFolder.Length > 0) Then
        '    m_unixServer_ITM = srvFolder
        'End If
        'srvFolder = ""
        'Try
        '    srvFolder = My.Settings("unixServer_REQCST").ToString.Trim
        'Catch ex As Exception
        'End Try
        'If (srvFolder.Length > 0) Then
        '    m_unixServer_REQCST = srvFolder
        'End If
        '   (6) "on error" send email values
        Dim sOnErrEmail As String = ""
        Try
            sOnErrEmail = My.Settings("onError_emailFrom").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailFrom = sOnErrEmail
        End If
        sOnErrEmail = ""
        Try
            sOnErrEmail = My.Settings("onError_emailTo").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailTo = sOnErrEmail
        End If
        sOnErrEmail = ""
        Try
            sOnErrEmail = My.Settings("onError_emailSubject").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailSubject = sOnErrEmail
        End If
        '   (7)  url
        Dim sUrl As String = ""
        Try
            sUrl = My.Settings("Url_Cytec_Maximo").ToString.Trim
        Catch ex As Exception
        End Try
        If (sUrl.Length > 0) Then
            m_Url_Cytec_Maximo = sUrl
        End If

        '
        ' command prompt parameters
        '   always win over app.config file since
        '
        Dim param As String() = Command.Split(" ")
        Dim arr As New ArrayList
        For Each s As String In param
            If s.Trim.Length > 0 Then
                arr.Add(s.Trim.ToUpper)
            End If
        Next
        If (arr.IndexOf("/LOG=VERBOSE") > -1) Then
            logLevel = TraceLevel.Verbose
        ElseIf (arr.IndexOf("/LOG=INFO") > -1) Or (arr.IndexOf("/LOG=INFORMATION") > -1) Then
            logLevel = TraceLevel.Info
        ElseIf (arr.IndexOf("/LOG=WARNING") > -1) Or (arr.IndexOf("/LOG=WARN") > -1) Then
            logLevel = TraceLevel.Warning
        ElseIf (arr.IndexOf("/LOG=ERROR") > -1) Then
            logLevel = TraceLevel.Error
        ElseIf (arr.IndexOf("/LOG=OFF") > -1) Or (arr.IndexOf("/LOG=FALSE") > -1) Then
            logLevel = TraceLevel.Off
        Else
            ' don't change default
        End If
        ' check/enable strOverride (whatever this is)
        If (arr.IndexOf("/OVERRIDE=Y") > -1) Then
            strOverride = "Y"
        ElseIf System.Diagnostics.Debugger.IsAttached Then
            strOverride = "Y"
        End If
        arr = Nothing
        param = Nothing

        ' initialize log
        m_logger = New appLogger(logpath1, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        Console.WriteLine("Start Cytec Maximo XML out")
        Console.WriteLine("")

        'objStreamWriter = File.CreateText(logpath)
        'objStreamWriter.WriteLine("  Cytec Maximo Processing XML out was started at: " & Now())
        m_logger.WriteInformationLog("  Cytec Maximo Processing XML out was started at: " & Now())

        Dim bolError As Boolean = getXMLOut()

        If bolError = True Then
            SendEmail()
        End If
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

        Return      '// Returning from Main would terminate console application.

        ' // At this point console application is no more.


    End Sub

    Private Function getXMLOut() As Boolean
        Dim rtn As String = "Cytec Maximo Process XML Out.getXMLOut"
        Console.WriteLine("Start send of Cytec Maximo XML out")
        Console.WriteLine("")

        ''put code here to go get the XML files
        'getOutputFiles()

        m_logger.WriteVerboseLog(rtn & " : looking at XMLOUT folder ...")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("c:\CytecMxmIn\XMLOUT\")
        Dim strFiles As String = ""
        Dim sr As System.IO.StreamReader = Nothing
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bolError As Boolean = False

        Dim xmlRequest As New XmlDocument

        Dim I As Integer = 0
        Dim strItems As String = ""
        Dim intItems As Integer = 0

        strFiles = "*.XML"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)

        If aFiles.Length = 0 Then
            'objStreamWriter.WriteLine(" No XML files to send")
            m_logger.WriteVerboseLog(" No XML files to send")
            Return False
        End If

        m_logger.WriteVerboseLog(rtn & " :: started sending file (if any) ...")

        Try
            For I = 0 To aFiles.Length - 1
                'objStreamWriter.WriteLine(" Start sending File " & aFiles(I).Name)
                m_logger.WriteVerboseLog(" Start SENDING FILE " & aFiles(I).FullName)
                sr = New System.IO.StreamReader(aFiles(I).FullName)
                XMLContent = sr.ReadToEnd()
                sr.Close()

                Try
                    xmlRequest.LoadXml(XMLContent)
                Catch ex As Exception
                    Console.WriteLine("")
                    Console.WriteLine("***error - " & ex.ToString)
                    Console.WriteLine("")
                    strXMLError = ex.ToString
                    m_logger.WriteErrorLog(rtn & " :: load out Error " & strXMLError & " in file " & aFiles(I).Name)

                End Try

                Dim Response_Doc As String
                If Trim(strXMLError) = "" Then

                    Response_Doc = SendOut(xmlRequest.InnerXml)
                    'Response_Doc = Send1(xmlRequest.InnerXml)


                    '-----------------------------------------------------------------------
                    ' Parse the XML response from Intercall Extranet
                    '-----------------------------------------------------------------------

                    Dim xmlResponse As New XmlDocument
                    Try
                        If Trim(Response_Doc) = "" Then
                            m_logger.WriteVerboseLog("     empty response from file " & aFiles(I).Name)
                        Else
                            m_logger.WriteVerboseLog(Response_Doc)
                            xmlResponse.LoadXml(Response_Doc)
                            ' error trapping code
                            If Response_Doc.Contains("<soapenv:Fault>") Or Response_Doc.Contains("</faultcode>") Or _
                                        Response_Doc.Contains("</faultstring>") Then
                                bolError = True
                                strXMLError = "<soapenv:Fault>"
                            End If
                        End If
                    Catch ex As Exception
                        Console.WriteLine("")
                        Console.WriteLine("***error - " & ex.ToString)
                        Console.WriteLine("")
                        strXMLError = ex.ToString
                        m_logger.WriteErrorLog(rtn & " ::Error: " & strXMLError & " in file " & aFiles(I).Name)

                    End Try

                    '-----------------------------------------------------------------------
                    ' Get server status
                    '-----------------------------------------------------------------------

                    If Trim(strXMLError) = "" Then
                        Try
                            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\XMLOUTProcessed\" & aFiles(I).Name, True)
                            m_logger.WriteVerboseLog(" End - FILE " & aFiles(I).FullName & " has been moved")
                            File.Delete(aFiles(I).FullName)
                        Catch ex As Exception

                            m_logger.WriteErrorLog(rtn & "  copy file Error " & strXMLError & " in file " & aFiles(I).Name)
                            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        End Try
                    Else
                        Try
                            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\BadXML\" & aFiles(I).Name, True)
                            m_logger.WriteVerboseLog(" Error - FILE: " & aFiles(I).FullName & " - has been moved to BadXML directory")
                            File.Delete(aFiles(I).FullName)
                        Catch ex As Exception

                            m_logger.WriteErrorLog(rtn & " ::  moved to BadXML directory file Error: " & strXMLError & " in file " & aFiles(I).Name)
                            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        End Try
                        bolError = True
                    End If

                End If

                strXMLError = ""
            Next
        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & aFiles(I).Name)
            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

            strXMLError = ex.ToString
            
            bolError = True
        End Try

        Return bolError

    End Function

    Private Function SendOut(ByVal strInnerXml As String) As String
        Dim Response_Doc As String = ""
        Dim XMLhttp As Object
        Dim xmlDoc2 As Object

        XMLhttp = CreateObject("MSXML2.ServerXMLHTTP")
        XMLhttp.Open("POST", m_Url_Cytec_Maximo, False)
        'XMLhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8")
        XMLhttp.setRequestHeader("Content-Type", "text/xml; charset=UTF-8")
        XMLhttp.setRequestHeader("SOAPAction", "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx")
        XMLhttp.setTimeouts(10000, 120000, 60000, 60000)
        'XMLhttp.Send("requestMessage=" & Microsoft.JScript.GlobalObject.encodeURIComponent(xmlRequest.InnerXml))
        XMLhttp.Send(strInnerXml)

        '-----------------------------------------------------------------------
        ' Get XML response from Extranet
        '-----------------------------------------------------------------------
        xmlDoc2 = CreateObject("MSXML2.DOMDocument")
        xmlDoc2.setProperty("ServerHTTPRequest", True)
        xmlDoc2.async = False
        xmlDoc2.LoadXML(XMLhttp.ResponseXML.xml)

        Response_Doc = XMLhttp.responseXML.xml

        Return Response_Doc

    End Function

    Private Function Send1(ByRef strBox1 As String) As String

        'Dim strBox2 As String = ""   '  my test URL: "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"   ' not seen outside: "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"   '  
        '' "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '  

        Dim sHttpResponse As String = ""
        Dim httpSession As New easyHttp

        httpSession.URL = "http://164.84.80.105:9075/meaweb/services/MXINVISSUEInterface"  '   "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"  '  "http://164.84.80.105:9075/meaweb/services/MXINVISSUEInterface"  '    "http://localhost/SDIWebProcessors/XmlInSDI.aspx"   '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"  '   "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '    "https://www.amazon.com/eprocurement/punchout"  '    "https://supplydev.hajoca.com/hajomid/eclipse.ecl"
        httpSession.DataToPost = strBox1
        httpSession.ContentType = "text/xml; charset=utf-8"
        httpSession.Method = easyHttp.HTTPMethod.HTTP_POST
        httpSession.IgnoreServerCertificate = True
        'httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")
        httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")

        sHttpResponse = ""

        Try
            'sHttpResponse = httpSession.SendAsBytes
            sHttpResponse = httpSession.SendAsString
        Catch ex As Exception
            sHttpResponse &= ex.Message
        End Try

        httpSession = Nothing

        Return sHttpResponse

    End Function


    'Private Sub getOutputFiles()
    '    Dim rtn As String = "ProcessOutXML.getOutputFiles"
    '    Dim I As Integer = 0
    '    Dim MSG_LOG As String = " "

    '    ''Wenjia's Unix server..... you may want to check with hime if htere was a problem with the database and the xml's are not goiung out to UNCC
    '    'Dim dirInfo3 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\IOH")
    '    'Dim dirInfo4 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\ITM")
    '    'Dim dirInfo5 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\REQCST")

    '    m_logger.WriteVerboseLog(rtn & " : looking at source folder for XML files (ie., IOH, ITM, REQCST) ...")

    '    'Dim dirInfo3 As DirectoryInfo = New DirectoryInfo(m_unixServer_IOH)
    '    Dim dirInfo3 As DirectoryInfo = Nothing
    '    Try
    '        dirInfo3 = New DirectoryInfo(m_unixServer_IOH)
    '    Catch ex As Exception
    '    End Try
    '    'Dim dirInfo4 As DirectoryInfo = New DirectoryInfo(m_unixServer_ITM)
    '    Dim dirInfo4 As DirectoryInfo = Nothing
    '    Try
    '        dirInfo4 = New DirectoryInfo(m_unixServer_ITM)
    '    Catch ex As Exception
    '    End Try
    '    'Dim dirInfo5 As DirectoryInfo = New DirectoryInfo(m_unixServer_REQCST)
    '    Dim dirInfo5 As DirectoryInfo = Nothing
    '    Try
    '        dirInfo5 = New DirectoryInfo(m_unixServer_REQCST)
    '    Catch ex As Exception
    '    End Try

    '    m_logger.WriteVerboseLog(rtn & " : retrieving XML files (ie., IOH, ITM, REQCST) ...")

    '    Dim strFiles As String = "*.XML"

    '    'Dim aFiles3 As FileInfo() = dirInfo3.GetFiles(strFiles)
    '    Dim aFiles3 As FileInfo() = Nothing
    '    If Not (dirInfo3 Is Nothing) Then
    '        Try
    '            aFiles3 = dirInfo3.GetFiles(strFiles)
    '        Catch ex As Exception
    '        End Try
    '    End If
    '    'Dim aFiles4 As FileInfo() = dirInfo4.GetFiles(strFiles)
    '    Dim aFiles4 As FileInfo() = Nothing
    '    If Not (dirInfo4 Is Nothing) Then
    '        Try
    '            aFiles4 = dirInfo4.GetFiles(strFiles)
    '        Catch ex As Exception
    '        End Try
    '    End If
    '    'Dim aFiles5 As FileInfo() = dirInfo5.GetFiles(strFiles)
    '    Dim aFiles5 As FileInfo() = Nothing
    '    If Not (dirInfo5 Is Nothing) Then
    '        Try
    '            aFiles5 = dirInfo5.GetFiles(strFiles)
    '        Catch ex As Exception
    '        End Try
    '    End If

    '    Dim bProcessFolder As Boolean = False

    '    If (aFiles3 Is Nothing) Then
    '        m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from IOH folder")
    '        objStreamWriter.WriteLine(" unable to obtain XML file list from IOH folder")
    '    Else
    '        If aFiles3.Length = 0 Then
    '            m_logger.WriteInformationLog(rtn & " : No IOH XML files to send")
    '            objStreamWriter.WriteLine(" No IOH XML files to send")
    '        Else
    '            bProcessFolder = True
    '        End If
    '    End If

    '    If bProcessFolder Then
    '        For I = 0 To aFiles3.Length - 1
    '            If Not File.Exists("c:\CytecMxmIn\XMLOUTProcessed\" & aFiles3(I).Name) Then
    '                Try
    '                    File.Copy(aFiles3(I).FullName, "C:\CytecMxmIn\XMLOUT\" & aFiles3(I).Name, True)
    '                    objStreamWriter.WriteLine(" File " & aFiles3(I).Name & " has been copied")
    '                    m_logger.WriteVerboseLog(MSG_LOG & " ::endded SENDING FILE " & aFiles3(I).FullName & " has been copied")
    '                Catch ex As Exception
    '                    objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles3(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
    '                    m_logger.WriteErrorLog(rtn & " :: getOutputFiles.")
    '                End Try
    '            End If
    '        Next
    '    End If

    '    bProcessFolder = False

    '    If (aFiles4 Is Nothing) Then
    '        m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from ITM folder")
    '        objStreamWriter.WriteLine(" unable to obtain XML file list from ITM folder")
    '    Else
    '        If aFiles4.Length = 0 Then
    '            m_logger.WriteInformationLog(rtn & " : No ITM XML files to send")
    '            objStreamWriter.WriteLine(" No ITM XML files to send")
    '        Else
    '            bProcessFolder = True
    '        End If
    '    End If

    '    If bProcessFolder Then
    '        For I = 0 To aFiles4.Length - 1
    '            If Not File.Exists("c:\CytecMxmIn\XMLOUTProcessed\" & aFiles4(I).Name) Then
    '                Try
    '                    File.Copy(aFiles4(I).FullName, "C:\CytecMxmIn\XMLOUT\" & aFiles4(I).Name, True)
    '                    objStreamWriter.WriteLine(" File " & aFiles4(I).Name & " has been copied")
    '                    m_logger.WriteVerboseLog(" File " & aFiles4(I).Name & " has been copied")
    '                Catch ex As Exception
    '                    objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
    '                    m_logger.WriteErrorLog(rtn & " :: getOutputFiles.")
    '                End Try
    '            End If
    '        Next
    '    End If

    '    bProcessFolder = False

    '    If (aFiles5 Is Nothing) Then
    '        m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from REQCST folder")
    '        objStreamWriter.WriteLine(" unable to obtain XML file list from REQCST folder")
    '    Else
    '        If aFiles5.Length = 0 Then
    '            m_logger.WriteInformationLog(rtn & " : No ITM XML files to send")
    '            objStreamWriter.WriteLine(" No REQCST XML files to send")
    '        Else
    '            bProcessFolder = True
    '        End If
    '    End If

    '    If bProcessFolder Then
    '        For I = 0 To aFiles5.Length - 1
    '            If Not File.Exists("c:\CytecMxmIn\XMLOUTProcessed\" & aFiles5(I).Name) Then
    '                Try
    '                    File.Copy(aFiles5(I).FullName, "C:\CytecMxmIn\XMLOUT\" & aFiles5(I).Name, True)
    '                    objStreamWriter.WriteLine(" File " & aFiles5(I).Name & " has been copied")
    '                    m_logger.WriteVerboseLog(" File " & aFiles5(I).Name & " has been copied")
    '                Catch ex As Exception
    '                    objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles5(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
    '                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString & " :: getOutputFiles.")

    '                End Try
    '            End If
    '        Next
    '    End If

    'End Sub

    Private Sub SendEmail()

        Dim email As New MailMessage

        ''The email address of the sender
        email.From = m_onError_emailFrom

        ''The email address of the recipient. 
        email.To = m_onError_emailTo

        ''The subject of the email
        email.Subject = m_onError_emailSubject

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>Cytec Maximo XML OUT has completed with errors, review log.</td></tr>"

        'Send the email and handle any error that occurs
        Dim rtn As String = "SendEmail"
        Try
            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "webdev@sdi.com", "Y", email.Body, connectOR)
        Catch ex As Exception
            'objStreamWriter.WriteLine("     Error - the email was not sent")
            m_logger.WriteErrorLog(rtn & " :: the email was not sent. Error: " & ex.Message.ToString)

        End Try

    End Sub

End Module
