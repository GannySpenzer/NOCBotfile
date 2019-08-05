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
Imports System.Net.Mail

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

        'get root dir
        Dim strMyRoot As String = "C:\SendCustEmail"
        Try
            strMyRoot = My.Settings("rootDirApp").ToString.Trim
        Catch ex As Exception
            strMyRoot = "C:\SendCustEmail"
        End Try
        If Trim(strMyRoot) <> "" Then
            rootDir = Trim(strMyRoot)
            logpath = rootDir & "\LOGS\SendCustEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Starting send Customer emails out " & Now())
        Dim strDbName As String = Right(cnString, 4)
        objStreamWriter.WriteLine("Database Name: " & strDbName)

        Dim bolError As Boolean = buildSendCustEmailOut()

        If bolError Then
            SendErrEmail()
        End If
        objStreamWriter.WriteLine("End of send Customer Emails out " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function buildSendCustEmailOut() As Boolean

        Dim strSQLstring As String = ""
        Dim bErrorExists As Boolean = False

        'strSQLstring = "SELECT TO_CHAR(A.DATETIME_ADDED,'YYYY-MM-DD-HH24.MI.SS.""000000""') as ADD_DATE," & vbCrLf & _
        '            " A.EMAIL_SUBJECT_LONG, A.ISA_EMAIL_FROM," & vbCrLf & _
        '            " A.ISA_EMAIL_TO, A.ISA_EMAIL_CC, A.ISA_EMAIL_BCC," & vbCrLf & _
        '            " A.ISA_EMAIL_DFL_HEAD, ISA_EMAIL_TXT_FILE," & vbCrLf & _
        '            " A.ISA_STATUS," & vbCrLf & _
        '            " A.EMAIL_TEXTLONG" & vbCrLf & _
        '            " FROM PS_ISA_OUTBND_EML A" & vbCrLf & _
        '            " WHERE A.EMAIL_DATETIME Is NULL" & vbCrLf

        strSQLstring = "SELECT * FROM SDIXEMAIL where BATCH_PRINT = 'Y'"

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

        Dim I As Integer = 0
        Dim bLineResult As Boolean = False
        Dim bAreThereBadEmails As Boolean = False
        Dim bNeedToSendErrorEmail As Boolean = False

        connectOR.Open()
        For I = 0 To ds.Tables(0).Rows.Count - 1
            bAreThereBadEmails = False
            bLineResult = sendCustEmail(ds.Tables(0).Rows(I), bAreThereBadEmails)
            If Not bLineResult Then
                bErrorExists = True
            End If
            If bAreThereBadEmails Then
                bNeedToSendErrorEmail = True
            End If
            If bLineResult Then
                bLineResult = updateSendEmailTbl(ds.Tables(0).Rows(I))
                If Not bLineResult Then
                    bErrorExists = True
                End If
            End If

        Next
        objStreamWriter.WriteLine("  sendCustEmails send selected emails = " & ds.Tables(0).Rows.Count)
        connectOR.Close()

        If Not bErrorExists Then
            If bNeedToSendErrorEmail Then
                bErrorExists = True
            End If
        End If
        Return bErrorExists

    End Function

    Private Function sendCustEmail(ByVal dr As DataRow, Optional ByRef bAreThereBadEmails As Boolean = False) As Boolean

        ' SDIXEMAIL

        'EMAILKEY      NOT NULL NUMBER(12)     -  ISA_EMAIL_ID  
        'EMAILFROM              VARCHAR2(60)   - ISA_EMAIL_FROM 
        'EMAILTO                VARCHAR2(250)  - ISA_EMAIL_TO
        'EMAILSUBJECT           VARCHAR2(250)   - EMAIL_SUBJECT_LONG
        'EMAILCC                VARCHAR2(250)  - ISA_EMAIL_CC
        'EMAILBCC               VARCHAR2(250)  - ISA_EMAIL_BCC 
        'EMAILBODYPATH          VARCHAR2(250)  - ISA_EMAIL_TXT_FILE --> filepath 
        'EMAILTYPE              VARCHAR2(20)   - <EMAILTYPE> - newer param - like "MATSTOCK"
        'EMAILRESENDID - not required -  VARCHAR2(12)   
        'DT_TIMESTAMP  NOT NULL TIMESTAMP(6)    - DATETIME_ADDED --> CURRENT_TIMESTAMP or Now()
        'OPRID                  VARCHAR2(8)   - newer param --> like "VROV1" 
        'EMAILBODY              VARCHAR2(4000)  - EMAIL_TEXTLONG  --> email body
        'BATCH_PRINT --> 'Y'    VARCHAR2(1)  - change to 'N' after emailed - in SendCustEmails
        'BATCH_DTTM - not required here -   DATE  - in SendCustEmails: EMAIL_DATETIME --> CURRENT_TIMESTAMP 

        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        Dim strbodyfoot As String = ""

        Dim strSQLstring As String = ""
        Dim strFirstName As String = " "
        Dim strLastName As String = " "
        Dim strEmailList As String = dr.Item("EMAILTO")  '   dr.Item("ISA_EMAIL_TO")
        strEmailList = Replace(strEmailList, ",", ";")
        Dim strEmailTo() As String = Split(strEmailList, ";")
        Dim intTotal As Integer = strEmailTo.Length()
        Dim intCnt As Integer = 0
        Dim strEmailEmpName As String = ""
        Dim strEmailEmpEmail As String = ""
        Dim strBadEmailsList As String = ""
        Dim strTimeStamp As String = ""  '  dr.Item("DT_TIMESTAMP")
        Try
            strTimeStamp = CType(dr.Item("DT_TIMESTAMP").ToString, String)
        Catch ex As Exception
            strTimeStamp = Now.ToString & "  ( current time )"
        End Try

        Dim eMail As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage()

        Dim strSingleEmail As String = ""
        For intCnt = 0 To intTotal - 1
            If Trim(strEmailTo(intCnt)) <> "" Then
                strSingleEmail = Trim(strEmailTo(intCnt))
                strSingleEmail = Replace(strSingleEmail, ":", "")
                If IsValidSingleAdd(strSingleEmail) Then
                    eMail.To.Add(strSingleEmail)

                    strFirstName = ""
                    strLastName = ""
                    strSQLstring = "SELECT A.FIRST_NAME_SRCH, A.LAST_NAME_SRCH" & vbCrLf & _
                                " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                                " WHERE UPPER(A.ISA_EMPLOYEE_EMAIL) = UPPER('" & strSingleEmail & "')" & vbCrLf & _
                                " AND ROWNUM < 2"

                    Dim drName As OleDbDataReader
                    Dim command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
                    drName = command.ExecuteReader()
                    If drName.Read Then
                        strFirstName = drName.Item("FIRST_NAME_SRCH")
                        strLastName = drName.Item("LAST_NAME_SRCH")
                    End If
                    drName.Close()

                    If (Trim(strFirstName) <> "") Or (Trim(strLastName) <> "") Then
                        If Trim(strEmailEmpName) = "" Then
                            strEmailEmpName = Trim(strFirstName) & " " & Trim(strLastName)
                        Else
                            strEmailEmpName &= ", " & Trim(strFirstName) & " " & Trim(strLastName)
                        End If

                    End If

                Else
                    ' bad email address - need to find out for whom
                    If Trim(strBadEmailsList) = "" Then
                        strBadEmailsList = strSingleEmail
                    Else
                        strBadEmailsList += " ; " & strSingleEmail
                    End If
                End If  '  If IsValidSingleAdd(Trim(strEmailTo(intCnt))) Then

            End If  '  If Trim(strEmailTo(intCnt)) <> "" Then

        Next  '  For intCnt = 0 To intTotal - 1

        If Trim(strBadEmailsList) <> "" Then
            objStreamWriter.WriteLine("  Bad email(s) for EmailKey: " & dr.Item("EMAILKEY") & ". Bad email(s) list:" & Trim(strBadEmailsList))
            bAreThereBadEmails = True
        End If

        strEmailEmpEmail = strEmailList

        'dr.Item("EMAILFROM") 
        eMail.From = New MailAddress(dr.Item("EMAILFROM"))  '  New MailAddress("SDIExchange@sdi.com", "SDI Exchange")

        eMail.Bcc.Add(New MailAddress("webdev@sdi.com", "WEBDEV Group"))

        'If Trim(dr.Item("EMAILBCC")) = "" Then 
        '    Mailer.Bcc = "webdev@sdi.com"
        'Else
        '    Mailer.Bcc = Trim(dr.Item("EMAILBCC")) 
        'End If
        'Dim strAddBCC As String = getAddBCC()
        'If Not Trim(strAddBCC) = "" Then
        '    If Mailer.Bcc = "" Then
        '        Mailer.Bcc = strAddBCC
        '    Else
        '        If Not InStr(Mailer.Bcc, strAddBCC) Then
        '            Mailer.Bcc = "; " & strAddBCC
        '        End If
        '    End If
        'End If

        If Trim(dr.Item("EMAILBODY")) = "" And Trim(dr.Item("EMAILBODYPATH")) = "" Then  '  If Trim(dr.Item("EMAIL_TEXTLONG")) = "" And Trim(dr.Item("ISA_EMAIL_TXT_FILE")) = "" Then
            objStreamWriter.WriteLine("  error no email message - " & strEmailEmpEmail & " for " & strEmailEmpName & " at " & Now())
            Return True
        End If
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"

        'If Not Trim(strEmailEmpName) = "" Then
        '    strbodydetl = strbodydetl & "<p>Hello " & strEmailEmpName & ",<br></p>"
        'End If

        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        
        If Not Trim(dr.Item("EMAILBODY")) = "" Then  '  If Not Trim(dr.Item("EMAIL_TEXTLONG")) = "" Then
            'strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dr.Item("EMAIL_TEXTLONG") & "</TD></TR>"
            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dr.Item("EMAILBODY") & "</TD></TR>"
        Else
            ' get external file
            Dim strExtMessage As String = getExternalMessageFile(dr.Item("EMAILBODYPATH"))
            If Trim(strExtMessage) = "" Then
                objStreamWriter.WriteLine("  error in 'getExternalMessageFile' no email message - " & strEmailEmpEmail & " for " & strEmailEmpName & " at " & Now())
                Return True
            Else
                strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & strExtMessage & "</TD></TR>"
            End If

        End If

        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & "Email record was created: " & strTimeStamp & "</TD></TR>"
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl & "</TABLE>" & vbCrLf

        strbodydetl = strbodydetl & "&nbsp;<br>"

        'strbodydetl = strbodydetl & "Sincerely,<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        'strbodydetl = strbodydetl & "SDI Customer Care<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"

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

        'Mailer.Body = strbodyhead & strbodydetl '   & strbodyfoot
        eMail.Body = strbodyhead & strbodydetl

        If Trim(dr.Item("EMAILSUBJECT")) = "" Then   '  If Trim(dr.Item("EMAIL_SUBJECT_LONG")) = "" Then
            eMail.Subject = "Email from SDI, INC"
        Else
            'Mailer.Subject = dr.Item("EMAIL_SUBJECT_LONG")
            eMail.Subject = dr.Item("EMAILSUBJECT")
        End If
        Dim strSubj911 As String = ""
        Dim strList911 As String = ""
        Dim i2 As Integer = 0

        Try
            strSubj911 = My.Settings("Subject911").ToString.Trim
        Catch ex As Exception
            strSubj911 = "911 Needs Immediate Attention"
        End Try

        If eMail.Subject.Contains(strSubj911) Then
            Try
                strList911 = My.Settings("List911").ToString.Trim
            Catch ex As Exception
                strList911 = "Scott.Doyle@sdi.com;Kelly.Kleinfelder@sdi.com"
            End Try
            If Trim(strList911) <> "" Then
                strList911 = Trim(strList911)
                Dim arrList911() As String = Split(strList911, ";")
                If arrList911.Length > 0 Then
                    For i2 = 0 To arrList911.Length - 1
                        If Trim(arrList911(i2)) <> "" Then
                            eMail.To.Add(arrList911(i2))
                        End If
                    Next
                    'eMail.Subject = " (Test Run) " & eMail.Subject
                End If
            End If
        End If
        Dim strEmailToList As String = eMail.To.ToString()

        If connectOR.DataSource.ToUpper = "RPTG" Or _
                connectOR.DataSource.ToUpper = "PLGR" Or _
                connectOR.DataSource.ToUpper = "STAR" Or _
                connectOR.DataSource.ToUpper = "DEVL" Then

            eMail.To.Clear()
            eMail.To.Add("webdev@sdi.com")
            eMail.To.Add("SDIportalsupport@avasoft.com")
            eMail.Subject = " (Test Run) - " & eMail.Subject
            eMail.Body = "Email.To - " & strEmailToList & eMail.Body
        Else
            
            If Trim(strEmailEmpName) = "" Then
                objStreamWriter.WriteLine("  email address problem for EmailKey: " & dr.Item("EMAILKEY") & " - e-mail address doesn't exist in Users table")
                'bAreThereBadEmails = True
            End If

        End If

        eMail.IsBodyHtml = True

        sendCustEmail = sendemail(eMail)

        If sendCustEmail Then
            objStreamWriter.WriteLine("  email sent for EmailKey: " & dr.Item("EMAILKEY") & " - with Subject: " & eMail.Subject & " - at " & Now())
        End If

    End Function

    Private Function IsValidSingleAdd(ByVal SingleEmailAdd As String) As Boolean
        Dim bValid As Boolean = False
        Try
            Dim strPart1 As String = ""
            If Trim(SingleEmailAdd) <> "" Then
                SingleEmailAdd = Trim(SingleEmailAdd)
                Dim cParts As String() = SingleEmailAdd.Split(CType("@", Char))

                If cParts.Length = 2 Then
                    bValid = (cParts(0).Length > 0 And cParts(1).Length > 0)
                    If bValid Then
                        strPart1 = Trim(cParts(1))
                        If strPart1.Contains(".") Then
                            bValid = True
                        Else
                            bValid = False
                        End If
                    End If
                Else
                    bValid = False
                End If
            Else
                bValid = False
            End If

        Catch ex As Exception
            bValid = False
        End Try

        Return bValid

    End Function

    Private Sub SendErrEmail()

        'Dim email As New System.Web.Mail.MailMessage
        'email.From = "TechSupport@sdi.com"
        'email.To = "webdev@sdi.com"
        'email.Subject = "Sent Cust Email OUT Error"

        'email.BodyFormat = MailFormat.Html
        'email.Body = "<html><body><table><tr><td>sendCustEmails has completed with errors, review log.</td></tr>"
        'email.Bcc = "webdev@sdi.com"

        Dim eMailNet As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage()

        eMailNet.From = New MailAddress("TechSupport@sdi.com", "Tech Support")  '  "TechSupport@sdi.com"
        eMailNet.To.Add(New MailAddress("webdev@sdi.com", "WEBDEV"))
        eMailNet.Subject = "Sent Cust Email OUT Error"
        eMailNet.Body = "<html><body><table><tr><td>sendCustEmails has completed with errors, review log.</td></tr>"
        eMailNet.IsBodyHtml = True

        'Send the email and handle any error that occurs
        Try
           
            sendemail(eMailNet)

        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - the email was not sent: " & ex.Message)
        End Try

    End Sub

    Private Function sendemail(ByVal mailer As System.Net.Mail.MailMessage) As Boolean
        Dim bRetrn As Boolean = True
        Dim strSmtpServerStr As String = "mail.sdi.com"
        Try
            strSmtpServerStr = My.Settings("SMTPServerString").ToString.Trim
        Catch ex As Exception
            strSmtpServerStr = "mail.sdi.com"
        End Try
        Dim strPortForSmtp As String = "25"
        Dim intPortSMTP As Integer = 25
        Try
            strPortForSmtp = My.Settings("PortSMTPServer").ToString.Trim
            If Trim(strPortForSmtp) <> "" Then
                strPortForSmtp = Trim(strPortForSmtp)
                If IsNumeric(strPortForSmtp) Then
                    intPortSMTP = CType(strPortForSmtp, Integer)
                Else
                    intPortSMTP = 25
                End If
            Else
                intPortSMTP = 25
            End If
        Catch ex As Exception
            strPortForSmtp = "25"
            intPortSMTP = 25
        End Try
        Try

            Dim clientSMTP As SmtpClient  '   = New SmtpClient(strSmtpServerStr, intPortSMTP)
            Try
                clientSMTP = New SmtpClient(strSmtpServerStr, intPortSMTP)

            Catch ex As Exception
                clientSMTP = New SmtpClient("mail.sdi.com", 25)
            End Try

            clientSMTP.Send(mailer)

        Catch ex As Exception
            bRetrn = False
            objStreamWriter.WriteLine("     Error - the email was not sent: " & ex.Message)
        End Try

        Return bRetrn

    End Function

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try
            ''SendLogger(mailer.Subject, mailer.Body, "SENDCUSTEMAILS", "Mail", mailer.To, "", mailer.Bcc, mailer.From)

            'SmtpMail.Send(email) - maybe this  ?? (Just change to NETMAIL)

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

        ' SDIXEMAIL

        'EMAILKEY      NOT NULL NUMBER(12)     -  ISA_EMAIL_ID  
        'EMAILFROM              VARCHAR2(60)   - ISA_EMAIL_FROM 
        'EMAILTO                VARCHAR2(250)  - ISA_EMAIL_TO
        'EMAILSUBJECT           VARCHAR2(250)   - EMAIL_SUBJECT_LONG
        'EMAILCC                VARCHAR2(250)  - ISA_EMAIL_CC
        'EMAILBCC               VARCHAR2(250)  - ISA_EMAIL_BCC 
        'EMAILBODYPATH          VARCHAR2(250)  - ISA_EMAIL_TXT_FILE --> filepath 
        'EMAILTYPE              VARCHAR2(20)   - <EMAILTYPE> - newer param - like "MATSTOCK"
        'EMAILRESENDID - not required -  VARCHAR2(12)   
        'DT_TIMESTAMP  NOT NULL TIMESTAMP(6)    - DATETIME_ADDED --> CURRENT_TIMESTAMP or Now()
        'OPRID                  VARCHAR2(8)   - newer param --> like "VROV1" 
        'EMAILBODY              VARCHAR2(4000)  - EMAIL_TEXTLONG  --> email body
        'BATCH_PRINT --> 'Y'    VARCHAR2(1)  - change to 'N' after emailed - in SendCustEmails
        'BATCH_DTTM - not required here -   DATE  - in SendCustEmails: EMAIL_DATETIME --> CURRENT_TIMESTAMP 

        Dim strSQLstring As String = ""
        Dim rowsaffected As Integer = 0

        strSQLstring = "UPDATE SDIXEMAIL" & vbCrLf & _
                       " SET BATCH_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                       " BATCH_PRINT = 'N'" & vbCrLf & _
                       " WHERE EMAILKEY = " & dr.Item("EMAILKEY") & "" 

        Dim Command1 As OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectOR)
        updateSendEmailTbl = True
        Try
            rowsaffected = Command1.ExecuteNonQuery
            If rowsaffected = 0 Then
                objStreamWriter.WriteLine("**")
                objStreamWriter.WriteLine("     Error - rowsaffected = 0 for table SDIXEMAIL tbl for EMAILKEY " & dr.Item("EMAILKEY"))
                objStreamWriter.WriteLine("  Sql statement: " & strSQLstring)
                objStreamWriter.WriteLine("**")
                updateSendEmailTbl = False
            End If
        Catch OleDBExp As OleDbException
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - updating SDIXEMAIL tbl for EMAILKEY " & dr.Item("EMAILKEY"))
            objStreamWriter.WriteLine("          " & OleDBExp.Message)
            objStreamWriter.WriteLine("**")
            updateSendEmailTbl = False
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
            Try
                reader.Close()
            Catch exErr As Exception

            End Try
        End Try

        Return ""

    End Function

    Private Function getExternalMessageFile(ByVal strFilePath As String) As String
        '' this is where we are going to call the web service
        '' the web service determines where the overflow email resides
        '' emails > 3999 characters get copied to a text file.
        '' Before the load balancer, it went to the server where ISOL resided.
        '' Now we'll have three servers, so we will let IIS determine where the text files reside.
        '' The web service is named SDI_load_balance_IO.
        '' call webservice 
        'Try
        '    Dim myloadbalance As loadBalance_March2018.SDI_loadbalance_IO = New loadBalance_March2018.SDI_loadbalance_IO
        '    'Dim myloadbalance As loadbalance2.SDI_loadbalance_IO = New loadbalance2.SDI_loadbalance_IO

        '    Dim ExtMsgFile As String = strFilePath
        '    Dim readerline As String
        '    readerline = myloadbalance.Stat_Change_Email_Send(ExtMsgFile)
        '    Return readerline
        'Catch ex As Exception

        '    Return ""
        'End Try

        Dim ExtMsgFile As String = strFilePath
        Dim reader As TextReader
        If File.Exists(ExtMsgFile) Then
            reader = File.OpenText(ExtMsgFile)
        Else
            objStreamWriter.WriteLine("  " & ExtMsgFile & " does not exist")
            Return ""
        End If
        Dim readerline As String = ""
        'Dim I As Integer
        Try
            While reader.Peek <> -1
                readerline = readerline & reader.ReadLine()
            End While

            reader.Close()
            Return readerline
        Catch ex As Exception
            objStreamWriter.WriteLine("Error in 'getExternalMessageFile' - " & ExtMsgFile & " - " & ex.Message)
            Try
                reader.Close()
            Catch exError As Exception

            End Try
        End Try

        Return ""

    End Function

End Module
