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

    Dim rootDir As String = "C:\Program Files (x86)\SDI\WarningNotActive90Days"
    Dim logpath As String = "C:\Program Files (x86)\SDI\WarningNotActive90Days\Logs\WarningNotActive90Days" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start of 'WARNING - Not Active 90 Days' process")
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
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\WarningNotActive90Days" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        ' log verbose DB connection string
        m_logger.WriteInformationLog(rtn & " :: Start of 'WARNING - Not Active 90 Days' process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        'get the list
        Dim strEmailAddress As String = ""
        Dim strSqlString As String = ""
        Dim strAccount As String = ""
        Dim strUserName As String = ""
        strSqlString = "SELECT ISA_EMPLOYEE_ID,ACTIVE_STATUS,LAST_ACTIVITY,ISA_EMPLOYEE_EMAIL, ISA_USER_NAME FROM PS_ISA_USERS_TBL " & vbCrLf & _
            "WHERE (LAST_ACTIVITY IS NULL AND ACTIVE_STATUS = 'A') OR " & vbCrLf & _
            "(LAST_ACTIVITY IS NOT NULL AND ACTIVE_STATUS = 'A' AND LAST_ACTIVITY < (SYSDATE - 90)) ORDER BY ISA_USER_NAME" & vbCrLf & _
            ""
        Dim ds As New DataSet
        Dim bError As Boolean = False

        Try

            ds = ORDBAccess.GetAdapter(strSqlString, connectOR)

        Catch ex21 As Exception
            m_logger.WriteErrorLog(rtn & " :: Error reading source table PS_ISA_USERS_TBL - e-mails were not sent")
            m_logger.WriteErrorLog("ERROR MSG: " & ex21.Message)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            bError = True
        End Try

        If Not bError Then

            If ds Is Nothing Then
                m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")
                bError = True
            Else

                If ds.Tables.Count = 0 Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")
                    bError = True
                Else

                    If ds.Tables(0).Rows.Count < 1 Then
                        m_logger.WriteWarningLog(rtn & " :: Warning - no Not Active Customers to process at this time")
                        bError = True
                    End If
                End If
            End If  '  ds Is Nothing - no Cytec Maximo POs to process

            Dim intX As Integer = 0
            Dim intNotSent As Integer = 0
            If Not bError Then
                Dim I As Integer = 0
                Dim rowsaffected As Integer = 0
                If ds.Tables(0).Rows.Count > 0 Then
                    Dim bSend As Boolean = True
                    For I = 0 To ds.Tables(0).Rows.Count - 1
                        'If I > 5235 Then ' processing what is not processed
                        bSend = True
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
                            If LCase(strEmailAddress) = "pete.doyle@sdi.com" Or LCase(strEmailAddress) = "customer.service@sdi.com" Or LCase(strEmailAddress) = "erwin.bautista@sdi.com" Or LCase(strEmailAddress) = "bob.dougherty@isacs.com" Then
                                'do nothing
                                intNotSent = intNotSent + 1
                                m_logger.WriteVerboseLog(rtn & " :: Email WAS NOT sent to: " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                            Else
                                bSend = True
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
                            End If ' not existing e-mails
                        Else
                            intNotSent = intNotSent + 1
                            m_logger.WriteVerboseLog(rtn & " :: Email WAS NOT sent to: " & strEmailAddress & " ; Account ID: " & strAccount & " ; User Name: " & strUserName)
                        End If  '  If Trim(strAccount) <> "" And Trim(strEmailAddress) <> "" Then

                        'End If  ' If I > 5635 Then
                        
                    Next  '  For I = 0 To ds.Tables(0).Rows.Count - 1
                    m_logger.WriteVerboseLog(rtn & " :: Total number of emails sent: " & intX.ToString())
                    m_logger.WriteVerboseLog(rtn & " :: Total number of emails NOT sent: " & intNotSent.ToString())
                End If  '  If ds.Tables(0).Rows.Count > 0 Then
            End If  ' If Not bError Then - inner
            
        End If ' If Not bError Then

        If bError Then
            'send Error e-mail
            SendErrorEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending 'WARNING - Not Active 90 Days' process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

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
        email.Subject = "'WARNING - Not Active 90 Days' process Error(s)"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'WARNING - Not Active 90 Days' process has completed with errors. Please, review Log file.</td></tr></table>"

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
        email.Bcc = "webdev@sdi.com"

        strSource = ""
        Try
            strSource = My.Settings("onErrorEmail_BCC").ToString.Trim
            If Trim(strSource) = "" Then
                strSource = "webdev@sdi.com"
            End If
        Catch ex As Exception
            strSource = "webdev@sdi.com"
        End Try
        If Trim(strSource) <> "" Then
            email.Bcc = strSource
        End If

        'The subject of the email
        email.Subject = "WARNING! Your SDiExchange Account about to Expire"

        Try
            strSource = My.Settings("onError_emailSubject").ToString.Trim
            If Trim(strSource) = "" Then
                strSource = "WARNING! Your SDiExchange Account about to Expire"
            End If
        Catch ex As Exception
            strSource = "WARNING! Your SDiExchange Account about to Expire"
        End Try
        If Trim(strSource) <> "" Then
            email.Subject = strSource
        End If

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>WARNING! Your SDiExchange Account: <b>" & strAccount & "</b>, - has been inactive </td></tr>" & _
            "<tr><td> for more than 90 days and <b>will be deactivated on or about 1st day of the next month</b>. </td></tr>" & _
            "<tr><td>You can avoid deactivation by logging into your SDiExchange Account.</td></tr>" & _
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
            m_logger.WriteErrorLog(rtn & " :: Error - the email was NOT sent for the Account: " & strAccount & " ; " & strEmailAddress & "")
        End If
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try
            ' for testing
            mailer.Cc = " "
            mailer.Bcc = " "
            SendLogger(mailer.Subject, mailer.Body, "WRNNGNOTACTVE90DAYS", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "TechSupport@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
