Imports System.Data.OleDb
Imports System.IO
Imports System.Xml
Imports System.Xml.XPath
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Web
Imports System.Text
Imports System.Net
Imports System.Net.Mail
Imports SDI.ApplicationLogger

Public Class UpdEmailOut

    Public Shared Sub UpdEmailOut(ByVal strSubject As String, ByVal strFrom As String, _
                    ByVal strTo As String, ByVal strCC As String, _
                    ByVal strBCC As String, ByVal strDflHead As String, _
                    ByVal strMessage As String, _
                    ByVal connection As OleDbConnection, Optional ByVal sMailPriority As String = "0")

        Dim myloadbalance As loadbalance.SDI_loadbalance_IO = New loadbalance.SDI_loadbalance_IO

        Dim objStreamWriterLogs As StreamWriter
        Dim myLoggr1 As appLogger = Nothing

        Dim rootDir As String = "C:\UpdEmailOut"
        Dim logpathAppLog As String = "C:\UpdEmailOut\LOGS\AppLoggerLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        Dim filepath As String = "C:\UpdEmailOut\FILES\UpdEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        Dim logpath As String = "C:\UpdEmailOut\LOGS\UpdEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

        objStreamWriterLogs = File.CreateText(logpath)
        myLoggr1 = New SDI.ApplicationLogger.appLogger(logpathAppLog, TraceLevel.Off)

        'Dim m_xmlConfig As XmlDocument
        'Dim m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\UpdEmailOutSettingConfig.xml"
        'm_xmlConfig = New XmlDocument
        'm_xmlConfig.Load(filename:=m_configFile)
        Dim strSubj1 As String = "Email from SDiExchange"
        Dim strFrom1 As String = "SDIExchange@SDI.com"
        'Dim xmlEmailElement As XmlElement = Nothing

        myLoggr1.WriteInformationLog("Start UpdEmailOut")

        '' get "email" node of the configuration file
        'If Not (m_xmlConfig("configuration")("email") Is Nothing) Then
        '    xmlEmailElement = m_xmlConfig("configuration")("email")
        'End If

        'If Not (xmlEmailElement Is Nothing) Then
        '    Try
        '        If Not (xmlEmailElement("defaultFrom").Attributes("addy").InnerText Is Nothing) Then
        '            strFrom1 = xmlEmailElement("defaultFrom").Attributes("addy").InnerText.Trim
        '        End If
        '    Catch ex As Exception
        '        strFrom1 = "SDIExchange@SDI.com"
        '    End Try

        '    Try
        '        If Not (xmlEmailElement("defaultSubject").Attributes("addy").InnerText Is Nothing) Then
        '            strSubj1 = xmlEmailElement("defaultSubject").Attributes("addy").InnerText.Trim
        '        End If
        '    Catch ex As Exception
        '        strSubj1 = "Email from SDiExchange"
        '    End Try
        'End If

        myLoggr1.WriteInformationLog("End Default")

        If Trim(strSubject) = "" Then
            strSubject = strSubj1

        End If
        If Trim(strFrom) = "" Then
            strFrom = strFrom1
        End If
        If Trim(strTo) = "" Then
            strTo = " "
        End If
        If Trim(strCC) = "" Then
            strCC = " "
        End If
        If Trim(strBCC) = "" Then
            strBCC = "webdev@sdi.com"
        End If
        If Trim(strDflHead) = "" Then
            strDflHead = "Y"
        End If
        Dim strSQLstring As String

        strSQLstring = "INSERT INTO PS_ISA_OUTBND_EML" & vbCrLf & _
            " ( ISA_EMAIL_ID, DATETIME_ADDED, EMAIL_SUBJECT_LONG, ISA_EMAIL_FROM," & vbCrLf & _
            " ISA_EMAIL_TO, ISA_EMAIL_CC, ISA_EMAIL_BCC," & vbCrLf & _
            " ISA_EMAIL_DFL_HEAD, ISA_EMAIL_TXT_FILE," & vbCrLf & _
            " ISA_STATUS, EMAIL_DATETIME, EMAIL_TEXTLONG)" & vbCrLf & _
            " VALUES (SEQ_ISA_EMAIL_ID_PK.NEXTVAL," & vbCrLf & _
            " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
            " '" & strSubject & "'," & vbCrLf & _
            " '" & strFrom & "'," & vbCrLf & _
            " '" & strTo & "'," & vbCrLf & _
            " '" & strCC & "'," & vbCrLf & _
            " '" & strBCC & "'," & vbCrLf & _
            " '" & strDflHead & "',"

        Dim sDatetimeAdded As String = ""
        Dim sIsaEmailTo As String = strTo
        Dim sEmailTextLong As String = " "
        Dim sIsaEmailTextFile As String = " "
        If strMessage.Length > 3999 Then
            strSQLstring = strSQLstring & " '" & filepath & "'," & vbCrLf & _
            " ' '," & vbCrLf & _
            " ''," & vbCrLf & _
            " ' ')"

            sIsaEmailTextFile = filepath
        Else
            strSQLstring = strSQLstring & " ' '," & vbCrLf & _
            " ' '," & vbCrLf & _
            " ''," & vbCrLf & _
            " '" & Replace(strMessage, "'", "''") & "')"

            sEmailTextLong = Replace(strMessage, "'", "''")
        End If
        Dim rowsaffected As Integer
        Try
            Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connection)
            If Not connection Is Nothing AndAlso ((connection.State And ConnectionState.Open) = ConnectionState.Open) Then
                connection.Close()
            End If

            connection.Open()
            rowsaffected = Command.ExecuteNonQuery()
            connection.Close()

            myLoggr1.WriteInformationLog("End 1st write to DB")

        Catch objException As Exception
            objStreamWriterLogs.WriteLine("Send emails out " & Now())
            objStreamWriterLogs.WriteLine("    error - " & _
                strSubject & " from " & strFrom & " to " & strTo & _
                objException.Message)
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End Try
        '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        ' right here we have to call the web service
        ' to copy to the the server that will house the text file 
        ' the reason is that we have a load balancer that will have three servers
        ' playing the balancing game and if we put it on the c: drive of the server the user is on 
        ' we will have a one in three chance of picking up that email
        ' so we need to direct the text file to one repository server - probably dazzle...
        ' we want to control this by going into IIS and determining where the Web Service is pointing
        Try
            If strMessage.Length > 3999 Then
                ' call webservice 

                Dim myloadbalance_string As String
                myloadbalance_string = myloadbalance.Stat_Change_Email_Copy(filepath, strMessage)

            End If

        Catch objException As Exception
            objStreamWriterLogs.WriteLine("Send emails out " & Now())
            objStreamWriterLogs.WriteLine("    error - " & _
                strSubject & " from " & strFrom & " to " & strTo & _
                objException.Message)
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End Try

        'VR 12/10/2014 Sending E-mail and marking record in database as sent

        Dim strbodyhead As String = ""
        Dim strbodydetl As String
        Dim strbodyfoot As String

        If Not connection Is Nothing AndAlso ((connection.State And ConnectionState.Open) = ConnectionState.Open) Then
            connection.Close()
        End If

        connection.Open()

        Dim strFirstName As String = ""
        Dim strLastName As String = ""

        strSQLstring = "SELECT A.FIRST_NAME_SRCH, A.LAST_NAME_SRCH" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                    " WHERE UPPER(A.ISA_EMPLOYEE_EMAIL) = UPPER('" & strTo & "')" & vbCrLf & _
                    " AND ROWNUM < 2"

        Try
            Dim drName As OleDbDataReader
            Dim command As OleDbCommand = New OleDbCommand(strSQLstring, connection)
            drName = command.ExecuteReader()
            If drName.Read Then
                strFirstName = drName.Item("FIRST_NAME_SRCH")
                strLastName = drName.Item("LAST_NAME_SRCH")
            Else
                strFirstName = " "
                strLastName = " "
            End If
            drName.Close()
        Catch ex As Exception
            strFirstName = " "
            strLastName = " "
        End Try

        Dim sName As String = ""
        If Trim(strFirstName) <> "" Or Trim(strLastName) <> "" Then
            sName = strFirstName & " " & strLastName
        Else
            sName = "   "
        End If

        myLoggr1.WriteInformationLog("End read name: " & sName)

        Dim myMailer As MailMessage = New MailMessage

        myMailer.From = New MailAddress(strFrom, sName)
        myMailer.To.Add(strTo)
        Dim iCtr As Integer = 0

        myLoggr1.WriteInformationLog("End add To")
        If Trim(strCC) <> "" Then
            Dim arrCC() As String = Split(strCC, ";")
            For iCtr = 0 To arrCC.Length - 1
                Try
                    myMailer.CC.Add(arrCC(iCtr))
                Catch ex30 As Exception

                End Try
            Next
        End If

        myLoggr1.WriteInformationLog("End add CC")
        If Trim(strBCC) <> "" Then
            Dim arrBcc() As String = Split(strBCC, ";")
            For iCtr = 0 To arrBcc.Length - 1
                Try
                    myMailer.Bcc.Add(arrBcc(iCtr))
                Catch ex32 As Exception

                End Try
            Next
        Else
        End If

        myLoggr1.WriteInformationLog("End add BCC")
        Dim strAddBCC As String = getAddBCC(rootDir, objStreamWriterLogs)
        If Not Trim(strAddBCC) = "" Then
            Dim arrAddBcc() As String = Split(strAddBCC, ";")
            For iCtr = 0 To arrAddBcc.Length - 1
                Try
                    myMailer.Bcc.Add(arrAddBcc(iCtr))
                Catch ex32 As Exception

                End Try
            Next

            'If Trim(strBCC) = "" Then
            'Else
            '    'If Not InStr(strAddBCC, Trim(strBCC)) Then  '  If Not InStr(Trim(strBCC), strAddBCC) Then
            '    '    myMailer.Bcc.Add(Trim(strBCC))  '  myMailer.Bcc.Add(strAddBCC)
            '    'End If
            'End If
        End If

        myLoggr1.WriteInformationLog("End add  BCC with getAddBCC")
        If Trim(strDflHead) = "Y" Then
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center>" & vbCrLf
            strbodyhead = strbodyhead & "<center><span >" & strSubject & "</span></center>"
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        End If

        Dim strEmailEmpEmail As String = strTo
        If Trim(sEmailTextLong) = "" And _
            Trim(sIsaEmailTextFile) = "" Then
            objStreamWriterLogs.WriteLine("  Error no email message - " & strEmailEmpEmail & " for " & sName & " at " & Now())
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End If
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"

        If Trim(strDflHead) = "Y" Then
            If Not Trim(sName) = "" Then
                strbodydetl = strbodydetl & "<p>Hello " & sName & ",<br></p>"
            End If
        End If

        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        If Not Trim(sEmailTextLong) = "" Then
            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & sEmailTextLong & "</TD></TR>"
        Else
            ' get external file
            Dim strExtMessage As String = getExternalMessageFile(sIsaEmailTextFile)
            If Trim(strExtMessage) = "" Then
                objStreamWriterLogs.WriteLine("  Error: empty email message - " & strEmailEmpEmail & " for " & sName & " at " & Now())
                objStreamWriterLogs.Flush()
                objStreamWriterLogs.Close()
                connection.Close()
                Exit Sub
            Else
                strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & strExtMessage & "</TD></TR>"
            End If

        End If

        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl & "</TABLE>" & vbCrLf

        strbodydetl = strbodydetl & "&nbsp;<br>"
        If Trim(strDflHead) = "Y" Then
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

        myMailer.Body = strbodyhead & strbodydetl & strbodyfoot

        myLoggr1.WriteInformationLog("End add Body")

        'myMailer.To.Add(strEmailEmpEmail)  '  this is for testing ONLY!
        If connection.DataSource.ToUpper = "RPTG" Or _
                connection.DataSource.ToUpper = "SNBX" Or _
                connection.DataSource.ToUpper = "STAR" Or _
                connection.DataSource.ToUpper = "DEVL" Then
            myMailer.To.Add("DoNotSendRPTG@sdi.com")
        Else
            myMailer.To.Add(strEmailEmpEmail)
        End If

        If Trim(strSubject) = "" Then
            myMailer.Subject = "Email from SDIExchange"
        Else
            myMailer.Subject = strSubject
        End If
        If InStr(myMailer.Subject, "In-Site?") Then
            myMailer.Subject = Replace(myMailer.Subject, "In-Site?", "In-Site®")
        End If

        myMailer.IsBodyHtml = True
        myMailer.Priority = MailPriority.Normal
        Try
            Select Case sMailPriority
                Case "0"
                    myMailer.Priority = MailPriority.Normal
                Case "1"
                    myMailer.Priority = MailPriority.Low
                Case "2"
                    myMailer.Priority = MailPriority.High
                Case Else
                    myMailer.Priority = MailPriority.Normal
            End Select
        Catch ex As Exception
            myMailer.Priority = MailPriority.Normal
        End Try
        Dim bResult As Boolean = False

        myLoggr1.WriteInformationLog("End prepare Mailer")
        Try
            bResult = mySendemail1(myMailer, objStreamWriterLogs)

            myLoggr1.WriteInformationLog("End send mail")
        Catch ex As Exception
            bResult = False
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End Try

        If bResult Then
            'marking record in database as sent
            updateSendEmailTbl(strTo, "", connection, objStreamWriterLogs)
            myLoggr1.WriteInformationLog("End update mail record as sent")
        Else
            'ERROR is recorded in 'mySendemail1' function
        End If

        objStreamWriterLogs.Flush()
        objStreamWriterLogs.Close()
        connection.Close()
    End Sub

    Public Shared Function updateSendEmailTbl(ByVal sIsaEmailTo As String, ByVal sAddDate As String, _
                ByVal connection As OleDbConnection, _
                ByRef objStreamWriter As StreamWriter) As Boolean

        Dim strSQLstringSrc As String

        strSQLstringSrc = "SELECT TO_CHAR(A.DATETIME_ADDED,'YYYY-MM-DD-HH24.MI.SS.""000000""') as ADD_DATE," & vbCrLf & _
                    " A.EMAIL_SUBJECT_LONG, A.ISA_EMAIL_FROM," & vbCrLf & _
                    " A.ISA_EMAIL_TO, A.ISA_EMAIL_CC, A.ISA_EMAIL_BCC," & vbCrLf & _
                    " A.ISA_EMAIL_DFL_HEAD, ISA_EMAIL_TXT_FILE," & vbCrLf & _
                    " A.ISA_STATUS," & vbCrLf & _
                    " A.EMAIL_TEXTLONG" & vbCrLf & _
                    " FROM PS_ISA_OUTBND_EML A" & vbCrLf & _
                    " WHERE A.EMAIL_DATETIME Is NULL AND ISA_EMAIL_TO = '" & sIsaEmailTo & "'" & vbCrLf

        Dim CommandSrc As OleDbCommand
        CommandSrc = New OleDbCommand(strSQLstringSrc, connection)

        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(CommandSrc)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
        Catch ex As Exception
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - getting info from PS_ISA_OUTBND_EML tbl for the line with A.EMAIL_DATETIME Is NULL and ISA_EMAIL_TO: " & sIsaEmailTo)
            objStreamWriter.WriteLine("          " & ex.Message)
            objStreamWriter.WriteLine("**")
            Return True
        End Try
        Try
            dataAdapter.Dispose()
            CommandSrc.Dispose()
        Catch ex As Exception

        End Try
        sAddDate = ""
        If Not ds Is Nothing Then
            If ds.Tables.Count > 0 Then
                If ds.Tables(0).Rows.Count > 0 Then
                    sAddDate = ds.Tables(0).Rows(0).Item("ADD_DATE")
                End If
            End If
        End If
        If Trim(sAddDate) = "" Then
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - cannot get ADD_DATE info (sAddDate is empty) from PS_ISA_OUTBND_EML tbl for the line with A.EMAIL_DATETIME Is NULL and ISA_EMAIL_TO: " & sIsaEmailTo)
            objStreamWriter.WriteLine("**")
            Return True
        End If
        Dim strSQLstring As String  '  dr.Item("ADD_DATE")   dr.Item("ISA_EMAIL_TO")
        Dim rowsaffected As Integer
        strSQLstring = "UPDATE PS_ISA_OUTBND_EML" & vbCrLf & _
                       " SET EMAIL_DATETIME = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                       " ISA_STATUS = 'S'" & vbCrLf & _
                       " WHERE TO_CHAR(DATETIME_ADDED,'YYYY-MM-DD-HH24.MI.SS.""000000""') = '" & sAddDate & "'" & vbCrLf & _
                       " AND ISA_EMAIL_TO = '" & sIsaEmailTo & "'"

        Dim Command1 As OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connection)
        Try
            rowsaffected = Command1.ExecuteNonQuery
            If rowsaffected = 0 Then
                objStreamWriter.WriteLine("**")
                objStreamWriter.WriteLine("     rowsaffected = 0, PS_ISA_OUTBND_EML tbl for the line with ADD_DATE: " & sAddDate & " and ISA_EMAIL_TO: " & sIsaEmailTo)
                objStreamWriter.WriteLine("**")
                updateSendEmailTbl = True
            End If
        Catch OleDBExp As OleDbException
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - updating PS_ISA_OUTBND_EML tbl for the line with ADD_DATE: " & sAddDate & " and ISA_EMAIL_TO: " & sIsaEmailTo)
            objStreamWriter.WriteLine("          " & OleDBExp.Message)
            objStreamWriter.WriteLine("**")
            updateSendEmailTbl = True
        End Try
        Command1.Dispose()
    End Function

    Public Shared Function getExternalMessageFile(ByVal strFilePath As String) As String
        ' this is where we are going to call the web service
        ' the web service determines where the overflow email resides
        ' emails > 3999 characters get copied to a text file.
        ' Before the load balancer, it went to the server where ISOL resided.
        ' Now we'll have three servers, so we will let IIS determine where the text files reside.
        ' The web service is named SDI_load_balance_IO.
        ' call webservice 
        Try
            Dim myloadbalance As loadbalance.SDI_loadbalance_IO = New loadbalance.SDI_loadbalance_IO
            Dim ExtMsgFile As String = (strFilePath)
            Dim readerline As String
            readerline = myloadbalance.Stat_Change_Email_Send(ExtMsgFile)
            Return readerline
        Catch ex As Exception
            'Dazzle is probably down so we can't grab the text file.
            Return ""
        End Try

    End Function

    Public Shared Sub UpdEmailOutNoCon(ByVal strSubject As String, ByVal strFrom As String, _
                    ByVal strTo As String, ByVal strCC As String, _
                    ByVal strBCC As String, ByVal strDflHead As String, _
                    ByVal strMessage As String)
        Dim connection As OleDbConnection = New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
        UpdEmailOut(strSubject, strFrom, strTo, strCC, strBCC, strDflHead, strMessage, connection)

    End Sub

    Public Sub New()

    End Sub

    Public Shared Function getAddBCC(ByVal rootDir As String, ByRef objStreamWriter As StreamWriter) As String

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
            objStreamWriter.WriteLine("  EmailAddBCC.txt error: " & ex.Message)
        End Try
        reader.Close()
        Return ""

    End Function

    Public Shared Function mySendemail1(ByVal mailer As MailMessage, ByRef objStreamWriter As StreamWriter) As Boolean
        Dim bResult As Boolean = False
        
        Dim mySmtpClient As SmtpClient = New SmtpClient("SDIMBXHYBRID.isacs.com")  '  New SmtpClient("SDIMBX01.isacs.com")
        Try
            mySmtpClient.Send(mailer)
            bResult = True
            Try
                objStreamWriter.WriteLine(" E-mail sent successfully")
            Catch ex1 As Exception
            End Try
        Catch ex As Exception
            bResult = False
            objStreamWriter.WriteLine("     Error - in the sendemail to customer Function (mySendemail1)")
        End Try

        Return bResult

    End Function

    Private Sub SendErrEmail(ByRef objStreamWriter As StreamWriter)

        Dim email As New MailMessage
        email.From = New MailAddress("TechSupport@sdi.com", "TechSupport")  '   = "TechSupport@sdi.com"
        email.To.Add("erwin.bautista@sdi.com")

        email.Subject = "UpdEmailOut Error"
        email.Priority = MailPriority.High
        email.IsBodyHtml = True
        email.Body = "<html><body><table><tr><td>Email sending by UpdEmailOut has completed with errors, review log.</td></tr>"

        'Send the email and handle any error that occurs
        Dim mySmtpClient As New SmtpClient
        Try
            mySmtpClient.Send(email)
        Catch
            objStreamWriter.WriteLine("     Error when trying to send 'Err E-mail' - the email was not sent")
        End Try

    End Sub

End Class

