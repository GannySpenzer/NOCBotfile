Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Xml
Imports System.Collections.Generic

Module Module1

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\SendCustEmail"
    Dim logpath As String = "C:\SendCustEmail\LOGS\SendCustEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")
    '     PS_ISA_OUTBND_EML
    'ISA_EMAIL_ID                              NOT NULL NUMBER(38)
    'DATETIME_ADDED                                     DATE
    'EMAIL_SUBJECT_LONG                        NOT NULL VARCHAR2(80)
    'ISA_EMAIL_FROM                            NOT NULL VARCHAR2(254
    'ISA_EMAIL_TO                              NOT NULL VARCHAR2(254
    'ISA_EMAIL_CC                              NOT NULL VARCHAR2(254
    'ISA_EMAIL_BCC                             NOT NULL VARCHAR2(254
    'ISA_EMAIL_DFL_HEAD                        NOT NULL VARCHAR2(1)
    'ISA_EMAIL_TXT_FILE                        NOT NULL VARCHAR2(100
    'ISA_STATUS                                NOT NULL VARCHAR2(1)
    'EMAIL_DATETIME                                     DATE
    'EMAIL_TEXTLONG                                     LONG

    ' SDIXEMAIL

    'EMAILKEY      NOT NULL NUMBER(12)     -  ISA_EMAIL_ID  
    'EMAILFROM              VARCHAR2(60)   - ISA_EMAIL_FROM 
    'EMAILTO                VARCHAR2(250)  - ISA_EMAIL_TO
    'EMAILSUBJECT           VARCHAR2(250)   - EMAIL_SUBJECT_LONG
    'EMAILCC                VARCHAR2(250)  - ISA_EMAIL_CC
    'EMAILBCC               VARCHAR2(250)  - ISA_EMAIL_BCC 
    'EMAILBODYPATH          VARCHAR2(250)  
    'EMAILTYPE              VARCHAR2(20)   
    'EMAILRESENDID          VARCHAR2(12)   
    'DT_TIMESTAMP  NOT NULL TIMESTAMP(6)    - DATETIME_ADDED
    'OPRID                  VARCHAR2(8)    
    'EMAILBODY              VARCHAR2(4000) 
    'BATCH_PRINT            VARCHAR2(1)    
    'BATCH_DTTM             DATE          -  EMAIL_DATETIME

    Sub Main()

        Console.WriteLine("Start Sending Customer Emails")
        Console.WriteLine("")

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnString = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG"
        End Try
        If Trim(cnString) <> "" Then
            cnString = Trim(cnString)
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            Try
                connectOR = New OleDbConnection(cnString)
            Catch ex As Exception
                connectOR = New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")
            End Try
        End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Starting send Customer emails out " & Now())

        Dim bolError As Boolean = buildSendCustEmailOut()

        If bolError = True Then
            SendErrEmail()
        End If
        objStreamWriter.WriteLine("End of send Customer Emails out " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function buildSendCustEmailOut() As Boolean

        Dim strSQLstring As String

        strSQLstring = "SELECT TO_CHAR(A.DATETIME_ADDED,'YYYY-MM-DD-HH24.MI.SS.""000000""') as ADD_DATE," & vbCrLf & _
                    " A.EMAIL_SUBJECT_LONG, A.ISA_EMAIL_FROM," & vbCrLf & _
                    " A.ISA_EMAIL_TO, A.ISA_EMAIL_CC, A.ISA_EMAIL_BCC," & vbCrLf & _
                    " A.ISA_EMAIL_DFL_HEAD, ISA_EMAIL_TXT_FILE," & vbCrLf & _
                    " A.ISA_STATUS," & vbCrLf & _
                    " A.EMAIL_TEXTLONG" & vbCrLf & _
                    " FROM PS_ISA_OUTBND_EML A" & vbCrLf & _
                    " WHERE A.EMAIL_DATETIME Is NULL" & vbCrLf

        Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("     error selecting from sendCustEmails PS_ISA_OUTBND_EML table")
            objStreamWriter.WriteLine("         " & ex.Message)
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("  sendCustEmails select emails = 0")
            connectOR.Close()
            Return False
        End If

        'insert into the PS_ISA_ORDSTAT_EML table

        Dim I As Integer
        'Dim bolEmailSent As Boolean

        connectOR.Open()
        For I = 0 To ds.Tables(0).Rows.Count - 1
            buildSendCustEmailOut = sendCustEmail(ds.Tables(0).Rows(I))
            If buildSendCustEmailOut = False Then
                buildSendCustEmailOut = updateSendEmailTbl(ds.Tables(0).Rows(I))
            End If

        Next
        objStreamWriter.WriteLine("  sendCustEmails send selected emails = " & ds.Tables(0).Rows.Count)
        connectOR.Close()

    End Function

    Private Function sendCustEmail(ByVal dr As DataRow) As Boolean

        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        Dim strbodyfoot As String = ""
        'Dim txtBody As String
        'Dim txtHdr As String
        'Dim txtMsg As String
        'Dim bolSelectItem As Boolean
        Dim strSQLstring As String = ""
        Dim strFirstName As String = " "
        Dim strLastName As String = " "
        Dim strEmailList As String = dr.Item("ISA_EMAIL_TO")
        Dim strEmailTo() As String = Split(strEmailList, ";")
        Dim intTotal As Integer = strEmailTo.Length()
        Dim intCnt As Integer = 0
        Dim strEmailEmpName As String = ""
        Dim strEmailEmpEmail As String = ""

        For intCnt = 0 To intTotal - 1

            strSQLstring = "SELECT A.FIRST_NAME_SRCH, A.LAST_NAME_SRCH" & vbCrLf & _
                        " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                        " WHERE UPPER(A.ISA_EMPLOYEE_EMAIL) = UPPER('" & strEmailTo(intCnt) & "')" & vbCrLf & _
                        " AND ROWNUM < 2"

            Dim drName As OleDbDataReader
            Dim command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
            drName = command.ExecuteReader()
            If drName.Read Then
                strFirstName = drName.Item("FIRST_NAME_SRCH")
                strLastName = drName.Item("LAST_NAME_SRCH")
            End If
            drName.Close()

            If Trim(strEmailEmpName) = "" Then
                strEmailEmpName = strFirstName & " " & strLastName
            Else
                strEmailEmpName &= ", " & strFirstName & " " & strLastName
            End If
        Next  '  For intCnt = 0 To intTotal - 1

        strEmailEmpEmail = strEmailList
        Dim Mailer As MailMessage = New MailMessage
        'Mailer.From = "Insiteonline@SDI.com"
        Mailer.From = Trim(dr.Item("ISA_EMAIL_FROM"))
        If Convert.ToString(dr.Item("ISA_EMAIL_CC")).Length = 1 Then
            Mailer.Cc = ""
        Else
            Mailer.Cc = Trim(dr.Item("ISA_EMAIL_CC"))
        End If
        If Convert.ToString(dr.Item("ISA_EMAIL_BCC")).Length = 1 Then
            Mailer.Bcc = ""
        Else
            Mailer.Bcc = Trim(dr.Item("ISA_EMAIL_BCC"))
        End If
        Dim strAddBCC As String = getAddBCC()
        If Not Trim(strAddBCC) = "" Then
            If Mailer.Bcc = "" Then
                Mailer.Bcc = strAddBCC
            Else
                If Not InStr(Mailer.Bcc, strAddBCC) Then
                    Mailer.Bcc = "; " & strAddBCC
                End If
            End If
        End If

        If dr.Item("ISA_EMAIL_DFL_HEAD") = "Y" Then
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center>" & vbCrLf
            strbodyhead = strbodyhead & "<center><span >" & dr.Item("EMAIL_SUBJECT_LONG") & "</span></center>"
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        End If

        If Trim(dr.Item("EMAIL_TEXTLONG")) = "" And _
            Trim(dr.Item("ISA_EMAIL_TXT_FILE")) = "" Then
            objStreamWriter.WriteLine("  error no email message - " & strEmailEmpEmail & " for " & strEmailEmpName & " at " & Now())
            Return True
        End If
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        If dr.Item("ISA_EMAIL_DFL_HEAD") = "Y" Then
            If Not Trim(strEmailEmpName) = "" Then
                strbodydetl = strbodydetl & "<p>Hello " & strEmailEmpName & ",<br></p>"
            End If
        End If

        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        If Not Trim(dr.Item("EMAIL_TEXTLONG")) = "" Then
            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dr.Item("EMAIL_TEXTLONG") & "</TD></TR>"
        Else
            ' get external file
            Dim strExtMessage As String = getExternalMessageFile(dr.Item("ISA_EMAIL_TXT_FILE"))
            If Trim(strExtMessage) = "" Then
                objStreamWriter.WriteLine("  error no email message - " & strEmailEmpEmail & " for " & strEmailEmpName & " at " & Now())
                Return True
            Else
                strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & strExtMessage & "</TD></TR>"
            End If

        End If

        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl & "</TABLE>" & vbCrLf

        strbodydetl = strbodydetl & "&nbsp;<br>"
        If dr.Item("ISA_EMAIL_DFL_HEAD") = "Y" Then
            strbodydetl = strbodydetl & "Sincerely,<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "SDI Customer Care<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
        End If

        strbodydetl = strbodydetl & "</p>"
        strbodydetl = strbodydetl & "</div>"

        strbodyfoot = "<BR><P><CENTER><SPAN style='FONT-SIZE: 12pt'><SPAN style='FONT-SIZE: 12pt'>" & _
                    "<FONT color=teal size=2>The information in this communication, including any attachments," & _
                    " is the property of SDI," & _
                    " Inc,&nbsp;</SPAN>is intended only for the addressee and may contain" & _
                    " confidential, proprietary, and/or privileged material." & _
                    " Any review, retransmission, dissemination or other use of," & _
                    " or taking of any action in reliance upon, this information by" & _
                    " persons or entities other than the intended recipient is prohibited." & _
                    " If you received this in error, please immediately contact the" & _
                    " sender by replying to this email and delete the material from" & _
                    " all computers.</FONT></SPAN></CENTER></P>"

        Mailer.Body = strbodyhead & strbodydetl & strbodyfoot

        If Trim(dr.Item("EMAIL_SUBJECT_LONG")) = "" Then
            Mailer.Subject = "Email from SDI, INC"
        Else
            Mailer.Subject = dr.Item("EMAIL_SUBJECT_LONG")
        End If

        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

        If dr.Item("ISA_STATUS") = "T" Then
            Mailer.To = Trim(dr.Item("ISA_EMAIL_BCC"))
        ElseIf connectOR.DataSource.ToUpper = "RPTG" Or _
                connectOR.DataSource.ToUpper = "PLGR" Or _
                connectOR.DataSource.ToUpper = "STAR" Or _
                connectOR.DataSource.ToUpper = "DEVL" Then
            Mailer.To = "WEBDEV@sdi.com"
            Mailer.Subject = " (Test Run) - " & Mailer.Subject
        Else
            Mailer.To = strEmailEmpEmail
        End If
        sendCustEmail = sendemail(Mailer)
        If sendCustEmail = False Then
            objStreamWriter.WriteLine("  email sent to email " & Mailer.To & " from " & Mailer.From & " at " & Now())
        End If

    End Function

    Private Sub SendErrEmail()

        Dim email As New MailMessage
        email.From = "TechSupport@sdi.com"
        email.To = "webdev@sdi.com"
        email.Subject = "Sent Cust Email OUT Error"
        email.Priority = MailPriority.High
        email.BodyFormat = MailFormat.Html
        email.Body = "<html><body><table><tr><td>sendCustEmails has completed with errors, review log.</td></tr>"
        email.Bcc = "vitaly.rovensky@sdi.com"

        'Send the email and handle any error that occurs
        Try
            'SmtpMail.Send(email) - maybe this  ?? (Just change to NETMAIL)

            SendEmail1(email)
        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

    Private Function sendemail(ByVal mailer As MailMessage) As Boolean

        Try
            
            SendEmail1(mailer)
        Catch ex As Exception

        End Try
    End Function

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try
            ''SendLogger(mailer.Subject, mailer.Body, "SENDCUSTEMAILS", "Mail", mailer.To, "", mailer.Bcc, mailer.From)

            ' here a newer version of UpdEmailOut

            ''UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, "", "", "N", mailer.Body, connectOR)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - in the sendemail to customer SUB")
        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, _
                   ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, ByVal EmailFrom As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            'Dim objException As String
            'Dim objExceptionTrace As String

            SDIEmailService.EmailUtilityServices(MailType, EmailFrom, EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())
            ' '   http://ims.sdi.com:8913/SDIEmailSvc/EmailServices.asmx

            ''  http://sdiwebsrv:8011/SDIEmailSvcProd/EmailServices.asmx
        Catch ex As Exception

        End Try
    End Sub

    Private Function updateSendEmailTbl(ByVal dr As DataRow) As Boolean

        Dim strSQLstring As String
        Dim rowsaffected As Integer
        strSQLstring = "UPDATE PS_ISA_OUTBND_EML" & vbCrLf & _
                       " SET EMAIL_DATETIME = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                       " ISA_STATUS = 'S'" & vbCrLf & _
                       " WHERE TO_CHAR(DATETIME_ADDED,'YYYY-MM-DD-HH24.MI.SS.""000000""') = '" & dr.Item("ADD_DATE") & "'" & vbCrLf & _
                       " AND ISA_EMAIL_TO = '" & dr.Item("ISA_EMAIL_TO") & "'"

        Dim Command1 As OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectOR)
        Try
            rowsaffected = Command1.ExecuteNonQuery
            If rowsaffected = 0 Then
                objStreamWriter.WriteLine("**")
                objStreamWriter.WriteLine("     Error - rowsaffected = 0 for table PS_ISA_OUTBND_EML tbl for order " & dr.Item("ADD_DATE") & " " & dr.Item("ISA_EMAIL_TO"))
                objStreamWriter.WriteLine("  Sql statement: " & strSQLstring)
                objStreamWriter.WriteLine("**")
                updateSendEmailTbl = True
            End If
        Catch OleDBExp As OleDbException
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - updating PS_ISA_OUTBND_EML tbl for order " & dr.Item("ADD_DATE") & " " & dr.Item("ISA_EMAIL_TO"))
            objStreamWriter.WriteLine("          " & OleDBExp.Message)
            objStreamWriter.WriteLine("**")
            updateSendEmailTbl = True
        End Try
        Command1.Dispose()
    End Function

    Private Function getAddBCC() As String
        Dim BCCFile As String = rootDir & "\EmailAddBCC.txt"
        Dim reader As TextReader
        If File.Exists(BCCFile) Then
            reader = File.OpenText(BCCFile)
        Else
            objStreamWriter.WriteLine("  EmailAddBCC.txt does not exist")
            Return ""
        End If
        Dim readerline As String
        'Dim I As Integer
        Try
            readerline = reader.ReadLine()
            reader.Close()
            objStreamWriter.WriteLine("  EmailAddBCC.txt email = " & readerline)

            Return readerline

        Catch ex As Exception
            objStreamWriter.WriteLine("  EmailAddBCC.txt exp = " & ex.Message)
        End Try
        reader.Close()
        Return ""

    End Function

    Private Function getExternalMessageFile(ByVal strFilePath As String) As String
        ' this is where we are going to call the web service
        ' the web service determines where the overflow email resides
        ' emails > 3999 characters get copied to a text file.
        ' Before the load balancer, it went to the server where ISOL resided.
        ' Now we'll have three servers, so we will let IIS determine where the text files reside.
        ' The web service is named SDI_load_balance_IO.
        ' call webservice 
        Try
            Dim myloadbalance As loadBalance_March2018.SDI_loadbalance_IO = New loadBalance_March2018.SDI_loadbalance_IO
            'Dim myloadbalance As loadbalance2.SDI_loadbalance_IO = New loadbalance2.SDI_loadbalance_IO

            Dim ExtMsgFile As String = strFilePath
            Dim readerline As String
            readerline = myloadbalance.Stat_Change_Email_Send(ExtMsgFile)
            Return readerline
        Catch ex As Exception

            Return ""
        End Try

        'Dim ExtMsgFile As String = strFilePath
        'Dim reader As TextReader
        'If File.Exists(ExtMsgFile) Then
        '    reader = File.OpenText(ExtMsgFile)
        'Else
        '    objStreamWriter.WriteLine("  " & ExtMsgFile & " does not exist")
        '    Return ""
        'End If
        'Dim readerline As String
        'Dim I As Integer
        'Try
        '    While reader.Peek <> -1
        '        readerline = readerline & reader.ReadLine()
        '    End While

        '    reader.Close()
        '    Return readerline
        'Catch ex As Exception
        '    objStreamWriter.WriteLine("  " & ExtMsgFile & " - " & ex.Message)
        'End Try
        'reader.Close()
        'Return ""

    End Function

End Module
