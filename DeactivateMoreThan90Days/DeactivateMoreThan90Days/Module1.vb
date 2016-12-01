Imports System
Imports System.Data
Imports System.Web
Imports System.Xml
Imports System.Data.OleDb
Imports System.Web.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter
Imports System.IO

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing
    Private m_timeStamp As String = Now.ToString

    Dim rootDir As String = "C:\Program Files (x86)\SDI\DeactivateNotActive90Days"
    Dim logpath As String = "C:\Program Files (x86)\SDI\DeactivateNotActive90Days\Logs\DeactivNotActive90Days" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Sub Main()

        Dim rtn As String = "DeactvNotActv90Days.Main"

        Console.WriteLine("Start of 'DEACTIVATE - Not Active 90 Days' process")
        Console.WriteLine("")

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
            connectOR = New OleDbConnection(cnString)
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
        Dim strLogFileName As String = "DeactivNotActive90Days" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            'logpath = sLogPath & "\DeactivNotActive90Days" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
            logpath = sLogPath & "\" & strLogFileName
        Else
            logpath = "C:\Program Files (x86)\SDI\DeactivateNotActive90Days\Logs\" & strLogFileName
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        Dim strEmailAddress As String = ""
        Dim strSqlString As String = ""
        Dim strAccount As String = ""
        Dim strUserName As String = ""

        Dim ds As New DataSet
        Dim bError As Boolean = False
        Dim rowsUpdated As Integer = 0

        If Not bError Then

            Console.WriteLine("Started Update part of 'DEACTIVATE - Not Active 90 Days' process")
            Console.WriteLine("")

            'get the list and send e-mails
            strSqlString = "SELECT ISA_EMPLOYEE_ID,ACTIVE_STATUS,LAST_ACTIVITY,ISA_EMPLOYEE_EMAIL, ISA_USER_NAME FROM PS_ISA_USERS_TBL " & vbCrLf & _
                "WHERE (LAST_ACTIVITY IS NULL AND ACTIVE_STATUS = 'A') OR " & vbCrLf & _
                "(LAST_ACTIVITY IS NOT NULL AND ACTIVE_STATUS = 'A' AND LAST_ACTIVITY < (SYSDATE - 90))" & vbCrLf & _
                ""

            Try

                ds = ORDBAccess.GetAdapter(strSqlString, connectOR)
                If ds Is Nothing Then
                    bError = True
                    m_logger.WriteErrorLog(rtn & " :: Error reading source table PS_ISA_USERS_TBL - e-mails were not sent")
                    m_logger.WriteErrorLog("Call to function 'ORDBAccess.GetAdapter' returned 'Nothing'")
                End If
            Catch ex21 As Exception
                m_logger.WriteErrorLog(rtn & " :: Error reading source table PS_ISA_USERS_TBL - e-mails were not sent")
                m_logger.WriteErrorLog("ERROR MSG: " & ex21.Message)
                m_logger.WriteErrorLog("SQL String: " & strSqlString)
                Try
                    connectOR.Close()
                Catch ex As Exception

                End Try
                bError = True
            End Try

            If Not bError Then

                If ds Is Nothing Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")

                Else

                    If ds.Tables.Count = 0 Then
                        m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")

                    Else

                        If ds.Tables(0).Rows.Count < 1 Then
                            m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")

                        End If
                    End If
                End If  '  ds Is Nothing - nothing to process

                Dim intX As Integer = 0
                Dim intNotSent As Integer = 0

                If Not bError Then

                    Dim I As Integer = 0
                    If Not ds Is Nothing Then
                        If ds.Tables.Count > 0 Then
                            If ds.Tables(0).Rows.Count > 0 Then

                                m_logger.WriteVerboseLog(rtn & " :: Total number of rows: " & ds.Tables(0).Rows.Count.ToString())
                                'deactivate here

                                ' log verbose DB connection string
                                m_logger.WriteInformationLog(rtn & " :: Start of 'DEACTIVATE - Not Active 90 Days' process.")
                                m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

                                'deactivate - get rows afftected
                                strSqlString = "UPDATE PS_ISA_USERS_TBL SET ACTIVE_STATUS = 'I' " & vbCrLf & _
                                    "WHERE (LAST_ACTIVITY IS NULL AND ACTIVE_STATUS = 'A') OR " & vbCrLf & _
                                    "(LAST_ACTIVITY IS NOT NULL AND ACTIVE_STATUS = 'A' AND LAST_ACTIVITY < (SYSDATE - 90))" & vbCrLf & _
                                    ""
                                Try
                                    rowsUpdated = ORDBAccess.ExecNonQuery(strSqlString, connectOR)
                                    If rowsUpdated = 0 Then
                                        m_logger.WriteErrorLog(rtn & " :: Error updating table PS_ISA_USERS_TBL - ACTIVE_STATUS was not updated")
                                        m_logger.WriteErrorLog("Function'ORDBAccess.ExecNonQuery' returned 'rowsUpdated = 0' ")
                                        m_logger.WriteErrorLog("SQL String: " & strSqlString)

                                        bError = True
                                    Else
                                        m_logger.WriteVerboseLog(rtn & " :: Total number of rows updated: " & rowsUpdated.ToString())
                                    End If
                                    Try
                                        connectOR.Close()
                                    Catch ex As Exception

                                    End Try
                                Catch ex18 As Exception
                                    m_logger.WriteErrorLog(rtn & " :: Error updating table PS_ISA_USERS_TBL - ACTIVE_STATUS was not updated")
                                    m_logger.WriteErrorLog("ERROR MSG: " & ex18.Message)
                                    m_logger.WriteErrorLog("SQL String: " & strSqlString)
                                    Try
                                        connectOR.Close()
                                    Catch ex As Exception

                                    End Try
                                    bError = True
                                End Try

                                If Not bError Then

                                    Console.WriteLine("Started Send E-mails part of 'DEACTIVATE - Not Active 90 Days' process")
                                    Console.WriteLine("")

                                    m_logger.WriteVerboseLog(rtn & " :: Starting Send Emails process ")
                                    Dim bSend As Boolean = True
                                    For I = 0 To ds.Tables(0).Rows.Count - 1

                                        strEmailAddress = ""
                                        Try
                                            strEmailAddress = CStr(ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))
                                        Catch ex As Exception
                                            strEmailAddress = ""
                                        End Try

                                        strAccount = ""
                                        Try
                                            strAccount = CStr(ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID"))
                                        Catch ex As Exception
                                            strAccount = ""
                                        End Try

                                        strUserName = ""
                                        Try
                                            strUserName = CStr(ds.Tables(0).Rows(I).Item("ISA_USER_NAME"))
                                        Catch ex As Exception
                                            strUserName = "<No Name>"
                                        End Try

                                        If Trim(strAccount) <> "" And Trim(strEmailAddress) <> "" Then
                                            strAccount = Trim(strAccount)
                                            strEmailAddress = Trim(strEmailAddress)
                                            If LCase(strEmailAddress) = "pete.doyle@sdi.com" Or LCase(strEmailAddress) = "customer.service@sdi.com" _
                                                Or LCase(strEmailAddress) = "erwin.bautista@sdi.com" Or LCase(strEmailAddress) = "bob.dougherty@isacs.com" _
                                                Or LCase(strEmailAddress) = "bob.dougherty@sdi.com" Then
                                                'send nothing
                                                intNotSent = intNotSent + 1
                                                m_logger.WriteVerboseLog(rtn & " :: Email WAS NOT sent to: " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                                            Else
                                                bSend = True
                                                'If intX > 4 Then
                                                '    SendEmail(strEmailAddress, strAccount, bSend)
                                                'End If
                                                SendEmail(strEmailAddress, strAccount, bSend)
                                                If bSend Then
                                                    m_logger.WriteVerboseLog(rtn & " :: Email sent to: " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                                                    intX = intX + 1
                                                    'If intX = 2 Then
                                                    '    Exit For
                                                    'End If
                                                Else
                                                    intNotSent = intNotSent + 1
                                                    m_logger.WriteVerboseLog(rtn & " :: Email WAS NOT sent to (tried to send): " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                                                End If
                                            End If ' old wrong e-mail address
                                        Else
                                            intNotSent = intNotSent + 1
                                            m_logger.WriteVerboseLog(rtn & " :: Email WAS NOT sent to: " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                                        End If  '  If Trim(strAccount) <> "" And Trim(strEmailAddress) <> "" Then

                                    Next  '  For I = 0 To ds.Tables(0).Rows.Count - 1
                                    m_logger.WriteVerboseLog(rtn & " :: Total number of emails sent: " & intX.ToString())
                                    m_logger.WriteVerboseLog(rtn & " :: Total number of emails NOT sent: " & intNotSent.ToString())

                                End If  '   If Not bError Then - inner # 3

                            End If  '  If ds.Tables(0).Rows.Count > 0 Then

                        End If
                    End If
                   
                End If '  If Not bError Then - Inner # 2

            End If  '  If Not bError Then - Inner # 1

        End If  '  If Not bError Then

        If bError Then
            'send Error e-mail
            SendErrorEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending 'DEACTIVATE - Not Active 90 Days' process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

        SendEmailHelpDesk(strLogFileName, logpath)

    End Sub

    Private Sub SendErrorEmail()

        Dim rtn As String = "Module1.SendErrorEmail"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "vitaly.rovensky@sdi.com"
        email.Cc = " "
        email.Bcc = "webdev@sdi.com"

        'The subject of the email
        email.Subject = "'DEACTIVATE - Not Active 90 Days' process Error(s)"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'DEACTIVATE - Not Active 90 Days' process has completed with errors. Please, review Log file.</td></tr></table>"

        'Send the email and handle any error that occurs
        Dim bSend As Boolean = False
        Try
            SendEmail1(email)
            bSend = True
        Catch ex As Exception
            bSend = False
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Private Sub SendEmail(ByVal strEmailAddress As String, ByVal strAccount As String, ByRef bSend As Boolean)

        Dim rtn As String = "Module1.SendEmail"

        Dim strSource As String = ""
        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = strEmailAddress
        '' for testing
        'email.To = "vitaly.rovensky@sdi.com;michael.randall@sdi.com"
        email.Cc = " "
        email.Bcc = " "

        strSource = ""
        'Try
        '    strSource = My.Settings("onErrorEmail_BCC").ToString.Trim
        '    If Trim(strSource) = "" Then
        '        strSource = "webdev@sdi.com"
        '    End If
        'Catch ex As Exception
        '    strSource = "webdev@sdi.com"
        'End Try
        'If Trim(strSource) <> "" Then
        '    email.Bcc = strSource
        'End If

        'The subject of the email
        email.Subject = " (Test for Mike) Your SDiExchange Account has been deactivated due to 90 days inactivity"

        Try
            strSource = My.Settings("onError_emailSubject").ToString.Trim
            If Trim(strSource) = "" Then
                strSource = "Your SDiExchange Account has been deactivated due to 90 days inactivity"
            End If
        Catch ex As Exception
            strSource = "Your SDiExchange Account has been deactivated due to more than 90 days inactivity"
        End Try
        If Trim(strSource) <> "" Then
            email.Subject = strSource
        End If

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>Due to more than 90 days of inactivity your <b>SDiExchange</b> Account: <b>" & strAccount & "</b>, - has been deactivated. </td></tr>" & _
            "<tr><td>You can have your account reactivated by contacting the HelpDesk @ 215-633-1900  Option 7.</td></tr>" & _
            "</table>" & vbCrLf
        email.Body &= "<HR width='100%' SIZE='1'>" & vbCrLf
        email.Body &= "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

        'Send the email and handle any error that occurs
        Try
            SendEmail1(email)
            bSend = True
            
        Catch ex As Exception
            bSend = False
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent for the Account: " & strAccount & " ; " & strEmailAddress & "")
        End If
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try
            'If Trim(mailer.Cc) = "" Then
            mailer.Cc = " "
            'End If
            'If Trim(mailer.Bcc) = "" Then
            mailer.Bcc = " "
            'End If
            SendLogger(mailer.Subject, mailer.Body, "DEACTVNOTACTV90DAYS", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            SDIEmailService.EmailUtilityServices(MailType, "TechSupport@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

    Private Sub SendEmailHelpDesk(ByVal strAttachedFileName As String, ByVal strFileUrl As String)

        Dim rtn As String = "Module1.SendEmailHelpDesk"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "Tony.Smith@sdi.com"
        email.Cc = " "
        email.Bcc = "webdev@sdi.com"

        'The subject of the email
        email.Subject = "'DEACTIVATE - Not Active 90 Days' process Summary"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>Please, see attached Summary for the 'DEACTIVATE - Not Active 90 Days' process.</td></tr></table>"

        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim Attachments As New List(Of String)
        Attachments.Add(strAttachedFileName)
        MailAttachmentName = Attachments.ToArray()
        MailAttachmentbytes.Add(New System.Net.WebClient().DownloadData(strFileUrl))


        'Send the email and handle any error that occurs
        Dim bSend As Boolean = False
        Try
            SendLoggerAllParams(email.Subject, email.Body, "DEACTVNOTACTV90DAYS", "Mail", email.To, " ", email.Bcc, MailAttachmentName, MailAttachmentbytes.ToArray())
            bSend = True
        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " :: Email to Help Desk was not sent. Error: " & ex.Message)
            bSend = True
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: bSend is False - the email to Help Desk was not sent.")
        End If
    End Sub

    Public Sub SendLoggerAllParams(ByVal subject As String, ByVal body As String, ByVal messageType As String, _
                ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, _
                ByVal AttachmentName() As String, ByVal Attachmentbytes()() As Byte)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()

            SDIEmailService.EmailUtilityServices(MailType, "TechSupport@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, AttachmentName, Attachmentbytes)

        Catch ex As Exception

        End Try
    End Sub

End Module
