Imports System.Xml
Imports System.IO
Imports System.Web.Mail
Imports Microsoft.JScript
Imports SDI.ApplicationLogger

Module Module1

    Private m_logger As appLogger = Nothing

    '
    ' NOTE: 
    '   These string will be overridden by the values in the app.config and then from command prompt param (if exists)
    '   - erwin
    '

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\INTFCXML"
    Dim logpath As String = "C:\INTFCXML\LOGS\UpdINTFCXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim logpath1 As String = "C:\INTFCXML\LOGS\UpdINTFCXMLloggerOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New System.Data.OleDb.OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
    Dim strOverride As String

    Private m_unixServer_IOH As String = "\\Instprd2\PSSHARE\efi\I0256\outbound\IOH"
    Private m_unixServer_ITM As String = "\\Instprd2\PSSHARE\efi\I0256\outbound\ITM"
    Private m_unixServer_REQCST As String = "\\Instprd2\PSSHARE\efi\I0256\outbound\REQCST"

    Private m_onError_emailFrom As String = "TechSupport@sdi.com"
    Private m_onError_emailTo As String = "michael.randall@sdi.com;vitaly.rovensky@sdi.com;"
    Private m_onError_emailSubject As String = "UNCC XML OUT Error"

    Private m_url_archibus_uncc_edu As String = "http://archibus.uncc.edu:8080/webtier/receivexml.jsp"

    Sub Main()

        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Info

        '
        ' read configuration values ...
        '   -- 3/19/2014 erwin
        '
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
            logpath = sLogPath & "\UpdINTFCXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
            logpath1 = sLogPath & "\UpdINTFCXMLloggerOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
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
        '   (5) unix server folder values
        Dim srvFolder As String = ""
        Try
            srvFolder = My.Settings("unixServer_IOH").ToString.Trim
        Catch ex As Exception
        End Try
        If (srvFolder.Length > 0) Then
            m_unixServer_IOH = srvFolder
        End If
        srvFolder = ""
        Try
            srvFolder = My.Settings("unixServer_ITM").ToString.Trim
        Catch ex As Exception
        End Try
        If (srvFolder.Length > 0) Then
            m_unixServer_ITM = srvFolder
        End If
        srvFolder = ""
        Try
            srvFolder = My.Settings("unixServer_REQCST").ToString.Trim
        Catch ex As Exception
        End Try
        If (srvFolder.Length > 0) Then
            m_unixServer_REQCST = srvFolder
        End If
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
        '   (7) archibus url
        Dim sUrl As String = ""
        Try
            sUrl = My.Settings("url_archibus_uncc_edu").ToString.Trim
        Catch ex As Exception
        End Try
        If (sUrl.Length > 0) Then
            m_url_archibus_uncc_edu = sUrl
        End If
        '   END : 3/19/2014 erwin

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
                                     " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        Console.WriteLine("Start UNCC XML out")
        Console.WriteLine("")

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        If Dir(rootDir & "\XMLIN", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLIN")
        End If
        If Dir(rootDir & "\XMLOUT", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLOUT")
        End If
        If Dir(rootDir & "\XMLINProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLINProcessed")
        End If
        If Dir(rootDir & "\XMLOUTProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLOUTProcessed")
        End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("  Update of SP Processing XML out " & Now())

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

        objStreamWriter.Flush()
        objStreamWriter.Close()
        Return      '// Returning from Main would terminate console application.

        ' // At this point console application is no more.


    End Sub

    Private Function getXMLOut() As Boolean
        Dim rtn As String = "ProcessOutXML.getXMLOut"
        Console.WriteLine("Start send of UNCC XML out")
        Console.WriteLine("")

        'put code here to go get the XML files
        getOutputFiles()

        m_logger.WriteVerboseLog(rtn & " : looking at XMLOUT folder ...")

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("c:\INTFCXML\XMLOUT\")
        Dim strFiles As String = ""
        Dim sr As System.IO.StreamReader = Nothing
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bolError As Boolean = False

        Dim xmlRequest As New XmlDocument

        Dim I As Integer = 0
        Dim strItems As String = ""
        Dim intItems As Integer = 0
        Dim MSG_LOG As String = " "

        strFiles = "*.XML"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)

        If aFiles.Length = 0 Then
            objStreamWriter.WriteLine(" No XML files to send")
            MSG_LOG = " No XML files to send"
            m_logger.WriteVerboseLog(MSG_LOG & " No XML files to send")
            Return False
        End If
        Dim XMLhttp As Object
        Dim xmlDoc2 As Object

        ''as of Ray dinello Please update server to fmbld07.uncc.edu.7-16-2008

        ''Dim strURL As String = "http://fmbld07.uncc.edu:8080/webtier/receivexml.jsp"
        'Dim strURL As String = "http://archibus.uncc.edu:8080/webtier/receivexml.jsp"

        ''Dim strURL As String = "http://PPLANT17NT.uncc.edu:8080/webtier/receivexml.jsp"
        ''Dim strURL As String = "http://fmbld05.uncc.edu:8080/webtier/receivexml.jsp"
        ''Dim strURL As String = "http://fmbld07.uncc.edu:8080/webtier/receivexml.jsp"

        ''Dim strURL As String = "http://200.61.48.171/webtier/receivexml.jsp"

        Dim strURL As String = m_url_archibus_uncc_edu
        Dim Response_Doc As String

        m_logger.WriteVerboseLog(rtn & " : started sending file (if any) ...")

        Try
            For I = 0 To aFiles.Length - 1
                objStreamWriter.WriteLine(" Start sending File " & aFiles(I).Name)
                m_logger.WriteVerboseLog(MSG_LOG & " ::Start SENDING FILE " & aFiles(I).FullName)
                sr = New System.IO.StreamReader(aFiles(I).FullName)
                XMLContent = sr.ReadToEnd()
                sr.Close()

                Try
                    xmlRequest.LoadXml(XMLContent)
                Catch ex As Exception
                    Console.WriteLine("")
                    Console.WriteLine("***error - " & ex.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     load out file Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                    strXMLError = ex.ToString
                    m_logger.WriteErrorLog(rtn & " :load out Error " & strXMLError & " in file " & aFiles(I).Name)
                    m_logger.WriteErrorLog(rtn & " :: " & ex.Message.ToString)

                End Try


                If Trim(strXMLError) = "" Then
                    XMLhttp = CreateObject("MSXML2.ServerXMLHTTP")
                    XMLhttp.Open("POST", strURL, False)
                    XMLhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8")
                    XMLhttp.setTimeouts(10000, 120000, 60000, 60000)
                    XMLhttp.Send("requestMessage=" & Microsoft.JScript.GlobalObject.encodeURIComponent(xmlRequest.InnerXml))

                    '-----------------------------------------------------------------------
                    ' Get XML response from Extranet
                    '-----------------------------------------------------------------------
                    xmlDoc2 = CreateObject("MSXML2.DOMDocument")
                    xmlDoc2.setProperty("ServerHTTPRequest", True)
                    xmlDoc2.async = False
                    xmlDoc2.LoadXML(XMLhttp.ResponseXML.xml)

                    Response_Doc = XMLhttp.responseXML.xml

                    '-----------------------------------------------------------------------
                    ' Parse the XML response from Intercall Extranet
                    '-----------------------------------------------------------------------

                    Dim xmlResponse As New XmlDocument
                    Try
                        If Trim(Response_Doc) = "" Then
                            objStreamWriter.WriteLine("     empty response from file " & aFiles(I).Name)
                        Else
                            objStreamWriter.WriteLine(Response_Doc)
                            xmlResponse.LoadXml(Response_Doc)
                        End If
                    Catch ex As Exception
                        Console.WriteLine("")
                        Console.WriteLine("***error - " & ex.ToString)
                        Console.WriteLine("")
                        objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                        strXMLError = ex.ToString
                        m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & aFiles(I).Name)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                    End Try

                    '-----------------------------------------------------------------------
                    ' Get server status
                    '-----------------------------------------------------------------------

                    If Trim(strXMLError) = "" Then
                        Try
                            File.Copy(aFiles(I).FullName, "C:\INTFCXML\XMLOUTProcessed\" & aFiles(I).Name, True)
                            objStreamWriter.WriteLine(" End - File " & aFiles(I).Name & " has been moved")
                            m_logger.WriteVerboseLog(MSG_LOG & " ::endded SENDING FILE " & aFiles(I).FullName & " has been moved")
                        Catch ex As Exception
                            objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                            m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & aFiles(I).Name)
                            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        End Try
                        File.Delete(aFiles(I).FullName)
                    Else
                        objStreamWriter.WriteLine("**")
                        objStreamWriter.WriteLine("     Error " & strXMLError & " in file " & aFiles(I).Name)
                        objStreamWriter.WriteLine("**")
                        m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & aFiles(I).Name)
                        'm_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        bolError = True
                    End If

                End If

                strXMLError = ""
            Next
        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & aFiles(I).Name)
            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

            strXMLError = ex.ToString
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error " & strXMLError & " in file " & aFiles(I).Name)
            objStreamWriter.WriteLine("**")
            bolError = True
        End Try

        Return bolError

    End Function

    Private Sub getOutputFiles()
        Dim rtn As String = "ProcessOutXML.getOutputFiles"
        Dim I As Integer = 0
        Dim MSG_LOG As String = " "

        ''Wenjia's Unix server..... you may want to check with hime if htere was a problem with the database and the xml's are not goiung out to UNCC
        'Dim dirInfo3 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\IOH")
        'Dim dirInfo4 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\ITM")
        'Dim dirInfo5 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\REQCST")

        m_logger.WriteVerboseLog(rtn & " : looking at source folder for XML files (ie., IOH, ITM, REQCST) ...")

        'Dim dirInfo3 As DirectoryInfo = New DirectoryInfo(m_unixServer_IOH)
        Dim dirInfo3 As DirectoryInfo = Nothing
        Try
            dirInfo3 = New DirectoryInfo(m_unixServer_IOH)
        Catch ex As Exception
        End Try
        'Dim dirInfo4 As DirectoryInfo = New DirectoryInfo(m_unixServer_ITM)
        Dim dirInfo4 As DirectoryInfo = Nothing
        Try
            dirInfo4 = New DirectoryInfo(m_unixServer_ITM)
        Catch ex As Exception
        End Try
        'Dim dirInfo5 As DirectoryInfo = New DirectoryInfo(m_unixServer_REQCST)
        Dim dirInfo5 As DirectoryInfo = Nothing
        Try
            dirInfo5 = New DirectoryInfo(m_unixServer_REQCST)
        Catch ex As Exception
        End Try

        m_logger.WriteVerboseLog(rtn & " : retrieving XML files (ie., IOH, ITM, REQCST) ...")

        Dim strFiles As String = "*.XML"

        'Dim aFiles3 As FileInfo() = dirInfo3.GetFiles(strFiles)
        Dim aFiles3 As FileInfo() = Nothing
        If Not (dirInfo3 Is Nothing) Then
            Try
                aFiles3 = dirInfo3.GetFiles(strFiles)
            Catch ex As Exception
            End Try
        End If
        'Dim aFiles4 As FileInfo() = dirInfo4.GetFiles(strFiles)
        Dim aFiles4 As FileInfo() = Nothing
        If Not (dirInfo4 Is Nothing) Then
            Try
                aFiles4 = dirInfo4.GetFiles(strFiles)
            Catch ex As Exception
            End Try
        End If
        'Dim aFiles5 As FileInfo() = dirInfo5.GetFiles(strFiles)
        Dim aFiles5 As FileInfo() = Nothing
        If Not (dirInfo5 Is Nothing) Then
            Try
                aFiles5 = dirInfo5.GetFiles(strFiles)
            Catch ex As Exception
            End Try
        End If

        Dim bProcessFolder As Boolean = False

        If (aFiles3 Is Nothing) Then
            m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from IOH folder")
            objStreamWriter.WriteLine(" unable to obtain XML file list from IOH folder")
        Else
            If aFiles3.Length = 0 Then
                m_logger.WriteInformationLog(rtn & " : No IOH XML files to send")
                objStreamWriter.WriteLine(" No IOH XML files to send")
            Else
                bProcessFolder = True
            End If
        End If

        If bProcessFolder Then
            For I = 0 To aFiles3.Length - 1
                If Not File.Exists("c:\INTFCXML\XMLOUTProcessed\" & aFiles3(I).Name) Then
                    Try
                        File.Copy(aFiles3(I).FullName, "C:\INTFCXML\XMLOUT\" & aFiles3(I).Name, True)
                        objStreamWriter.WriteLine(" File " & aFiles3(I).Name & " has been copied")
                        m_logger.WriteVerboseLog(MSG_LOG & " ::endded SENDING FILE " & aFiles3(I).FullName & " has been copied")
                    Catch ex As Exception
                        objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles3(I).Name)
                        m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                        m_logger.WriteErrorLog(rtn & " :: getOutputFiles.")
                    End Try
                End If
            Next
        End If

        bProcessFolder = False

        If (aFiles4 Is Nothing) Then
            m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from ITM folder")
            objStreamWriter.WriteLine(" unable to obtain XML file list from ITM folder")
        Else
            If aFiles4.Length = 0 Then
                m_logger.WriteInformationLog(rtn & " : No ITM XML files to send")
                objStreamWriter.WriteLine(" No ITM XML files to send")
            Else
                bProcessFolder = True
            End If
        End If

        If bProcessFolder Then
            For I = 0 To aFiles4.Length - 1
                If Not File.Exists("c:\INTFCXML\XMLOUTProcessed\" & aFiles4(I).Name) Then
                    Try
                        File.Copy(aFiles4(I).FullName, "C:\INTFCXML\XMLOUT\" & aFiles4(I).Name, True)
                        objStreamWriter.WriteLine(" File " & aFiles4(I).Name & " has been copied")
                        m_logger.WriteVerboseLog(" File " & aFiles4(I).Name & " has been copied")
                    Catch ex As Exception
                        objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                        m_logger.WriteErrorLog(rtn & " :: getOutputFiles.")
                    End Try
                End If
            Next
        End If

        bProcessFolder = False

        If (aFiles5 Is Nothing) Then
            m_logger.WriteInformationLog(rtn & " : unable to obtain XML file list from REQCST folder")
            objStreamWriter.WriteLine(" unable to obtain XML file list from REQCST folder")
        Else
            If aFiles5.Length = 0 Then
                m_logger.WriteInformationLog(rtn & " : No ITM XML files to send")
                objStreamWriter.WriteLine(" No REQCST XML files to send")
            Else
                bProcessFolder = True
            End If
        End If

        If bProcessFolder Then
            For I = 0 To aFiles5.Length - 1
                If Not File.Exists("c:\INTFCXML\XMLOUTProcessed\" & aFiles5(I).Name) Then
                    Try
                        File.Copy(aFiles5(I).FullName, "C:\INTFCXML\XMLOUT\" & aFiles5(I).Name, True)
                        objStreamWriter.WriteLine(" File " & aFiles5(I).Name & " has been copied")
                        m_logger.WriteVerboseLog(" File " & aFiles5(I).Name & " has been copied")
                    Catch ex As Exception
                        objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles5(I).Name)
                        m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString & " :: getOutputFiles.")

                    End Try
                End If
            Next
        End If

    End Sub

    Private Sub SendEmail()

        Dim email As New MailMessage

        ''The email address of the sender
        'email.From = "TechSupport@sdi.com"
        email.From = m_onError_emailFrom

        ''The email address of the recipient. 
        'email.To = "bob.dougherty@sdi.com"
        email.To = m_onError_emailTo

        ''The subject of the email
        'email.Subject = "UNCC XML OUT Error"
        email.Subject = m_onError_emailSubject

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>UNCC has completed with errors, review log.</td></tr>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Dim rtn As String = "SendEmail"
        Try
            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - the email was not sent")
            m_logger.WriteErrorLog(rtn & " ::the email was not sent Error " & ex.Message.ToString)

        End Try

    End Sub

End Module
