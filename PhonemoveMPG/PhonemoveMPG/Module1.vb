Imports System.Xml
Imports System.IO
Imports System.Web.Mail
Imports Microsoft.JScript
Imports SDI.ApplicationLogger
Imports UpdEmailOut


Module Module1
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "\\lenka\c$\Phone_MP3"
    Dim sdmDIR As String = "\SDM"
    Dim CorpDir As String = "\Corp"
    Dim strMonth As String = getMonth()
    Dim Monthpath_SDM As String = "\" & strMonth & " "
    Dim Monthpath_corp As String = "\" & strMonth & " "
    Dim logpath As String = rootDir & "\LOGS\PhoneMoveLogs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New System.Data.OleDb.OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
    Dim strOverride As String
    Private m_logger As appLogger = Nothing
    Sub Main()
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Info
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
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        ' initialize(log)
        'logpath = (rootDir & "\LOGS")
        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        Console.WriteLine("Start MPG_Phone out")
        Console.WriteLine("")

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\" & sdmDIR, FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\" & sdmDIR)
        End If


        If Dir(rootDir & "\" & CorpDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\" & CorpDir)
        End If
        If Dir(rootDir & "\" & sdmDIR & Monthpath_SDM, FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\" & sdmDIR & Monthpath_SDM)
        End If
        If Dir(rootDir & "\" & CorpDir & Monthpath_corp, FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\" & CorpDir & Monthpath_corp)
        End If


        objStreamWriter = File.CreateText(Monthpath_SDM)
        objStreamWriter.WriteLine("  Update of SP Processing MP3 out " & Now())

        Dim bolError As Boolean = getOutputFiles()

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

        ' // At th
    End Sub
    Private Function getOutputFiles()
        Dim rtn As String = "ProcessOutMP3.getOutputFiles"
        Dim I As Integer
        Dim MSG_LOG As String = " "

        '  Dim dirInfo3 As DirectoryInfo = New DirectoryInfo("\\Instprd2\PSSHARE\efi\I0256\outbound\IOH")
        Dim dirInfo3 As DirectoryInfo = New DirectoryInfo("\\lenka\c$\inetpub\ftp.voxnet")
        Dim dirInfo4 As DirectoryInfo = New DirectoryInfo("\\lenka\c$\inetpub\ftp.sdm.recordings")
         

        Dim strFiles As String = "*.mp3"
        Dim strfiles1 As String = "*.wav"
        Dim aFiles3 As FileInfo() = dirInfo3.GetFiles(strFiles)
        Dim aFiles4 As FileInfo() = dirInfo4.GetFiles(strfiles1)
         

        If aFiles3.Length = 0 Then
            objStreamWriter.WriteLine(" No MPG files to send")
        Else
            For I = 0 To aFiles3.Length - 1
                '"Phone_MP3_Folder" & Now.Month & "   "
                If Not File.Exists("\\lenka\c$\Phone_MP3\Corp\" & Monthpath_corp & "" & aFiles3(I).Name) Then
                    Try
                        File.Move(aFiles3(I).FullName, rootDir & CorpDir & Trim(Monthpath_corp) & "\" & aFiles3(I).Name)
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
        If aFiles4.Length = 0 Then
            objStreamWriter.WriteLine(" No sdmMPG files to send")
        Else
            For I = 0 To aFiles4.Length - 1
                If Not File.Exists("\\lenka\c$\Phone_MP3\SDM\" & Monthpath_corp & "" & aFiles4(I).Name) Then
                    Try

                        File.Move(aFiles4(I).FullName, rootDir & sdmDIR & Trim(Monthpath_SDM) & "\" & aFiles4(I).Name)
                        objStreamWriter.WriteLine(" File " & aFiles4(I).Name & " has been copied")
                        m_logger.WriteVerboseLog(MSG_LOG & " ::endded SENDING FILE " & aFiles3(I).FullName & " has been copied")
                    Catch ex As Exception
                        objStreamWriter.WriteLine("   copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " ::copy file Error " & ex.Message.ToString & " in file " & aFiles4(I).Name)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                        m_logger.WriteErrorLog(rtn & " :: getOutputFiles.")
                    End Try
                End If
            Next
        End If
        

    End Function

    Private Sub SendEmail()

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "bob.dougherty@sdi.com"

        'The subject of the email
        email.Subject = "UNCC XML OUT Error"


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
            ' m_logger.WriteErrorLog(rtn & " ::the email was not sent Error " & ex.Message.ToString)

        End Try
    End Sub
    Function getMonth()
        Dim StrPesentMonth As String = Now.Month.ToString
        Dim strReturnMonth As String = " "
        Select Case StrPesentMonth
            Case "1"
                strReturnMonth = "January"
            Case "2"
                strReturnMonth = "February"
            Case "3"
                strReturnMonth = "March"
            Case "4"
                strReturnMonth = "April"
            Case "5"
                strReturnMonth = "May"
            Case "6"
                strReturnMonth = "June"
            Case "7"
                strReturnMonth = "July"
            Case "8"
                strReturnMonth = "August"
            Case "9"
                strReturnMonth = "September"
            Case "10"
                strReturnMonth = "October"
            Case "11"
                strReturnMonth = "November"
            Case "12"
                strReturnMonth = "December"

        End Select

        Return strReturnMonth

    End Function
End Module
