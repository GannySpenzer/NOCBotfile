Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Xml

Module Module1

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\Program Files (x86)\SDI\CheckAutoCribTrans"
    Dim logpath As String = "C:\Program Files (x86)\SDI\CheckAutoCribTrans\LOGS\CheckAutoCribTransOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG")
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

        Console.WriteLine("Started to check Auto Crib Transactions ")
        Console.WriteLine("")

        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Started to check Auto Crib Transactions " & Now())

        m_xmlConfig = New XmlDocument
        m_xmlConfig.Load(filename:=m_configFile)

        Dim bolError As Boolean = False
        bolError = CheckAutoCribTrans()

        If bolError Then
            SendErrEmail()
        End If

        objStreamWriter.WriteLine("End of Auto Crib Transactions Email send " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function CheckAutoCribTrans() As Boolean
        Dim bResult As Boolean = False

        Dim strSQLstring As String
        ' selct all customers with AutoCrib installed
        strSQLstring = "select * from ps_isa_enterprise where isa_autocrib_db > ' '"

        Dim cnString As String = ""
        Try

            ' retrieve the source DB connection string to use
            If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                cnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
            End If
        Catch ex As Exception
            cnString = ""
        End Try

        If Trim(cnString) <> "" Then
            connectOR.ConnectionString = cnString
        End If
        Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet

        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("     error selecting from ps_isa_enterprise table")
            objStreamWriter.WriteLine("         " & ex.Message)
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine(" 0 rows selected from ps_isa_enterprise table")
            connectOR.Close()
            Return False
        Else
            'check how many trans for each selected BU
            Dim I As Integer

            objStreamWriter.WriteLine(ds.Tables(0).Rows.Count & "  rows selected from ps_isa_enterprise table")
            Try
                connectOR.Open()
            Catch ex As Exception
                objStreamWriter.WriteLine(" Error on connectOR.Open() ")
            End Try
            Dim bOK As Boolean = False
            For I = 0 To ds.Tables(0).Rows.Count - 1
                'objStreamWriter.WriteLine(" Going to check row: " & I.ToString())
                If I = 0 Then
                    bOK = CheckAndSendCustEmail(ds.Tables(0).Rows(I), True)
                Else
                    bOK = CheckAndSendCustEmail(ds.Tables(0).Rows(I))
                End If

                If Not bOK Then
                    bResult = True
                End If
            Next
            Try
                connectOR.Close()
            Catch ex As Exception
                objStreamWriter.WriteLine(" Error on connectOR.Close() ")
            End Try
        End If

        Return bResult

    End Function

    Private Function CheckAndSendCustEmail(ByVal dr As DataRow, Optional ByVal bDo As Boolean = False) As Boolean
        Dim bReslt As Boolean = False
        '  ISA_COMPANY_ID " & dr.Item("ISA_COMPANY_ID") & "

        Dim strSQLstring As String
        'Dim strFirstName As String
        'Dim strLastName As String
        Dim strBusUnitMy As String = dr.Item("isa_business_unit")

        strSQLstring = "select * from SYSADM.ps_isa_autocrb_trx " & vbCrLf & _
                    " where business_unit='" & strBusUnitMy & "' " & vbCrLf & _
                    " AND INV_ITEM_ID != ' ' and trunc(dt_timestamp, 'DDD') > trunc(sysdate - 2, 'DDD')"

        Dim dDate As Date = CDate(Now())
        Try
            If bDo Then
                objStreamWriter.WriteLine(" Sample SQL string: " & strSQLstring)
            End If
            Dim Command1 As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)

            Dim dataAdapter1 As OleDbDataAdapter = _
                        New OleDbDataAdapter(Command1)
            Dim ds1 As System.Data.DataSet = New System.Data.DataSet
            'objStreamWriter.WriteLine("Place1")
            dataAdapter1.Fill(ds1)
            'Try
            '    objStreamWriter.WriteLine("Place2 " & ds1.Tables.Count)
            'Catch ex As Exception
            '    objStreamWriter.WriteLine("Error: Place2")
            'End Try
            'Try
            '    objStreamWriter.WriteLine("Place2-A " & ds1.Tables(0).Rows.Count)
            'Catch ex As Exception
            '    objStreamWriter.WriteLine("Error: Place2-A")
            'End Try

            If Not ds1 Is Nothing Then
                If ds1.Tables.Count > 0 Then
                    Dim iMy As Integer = ds1.Tables(0).Rows.Count

                    If iMy = 0 Then
                        'send e-mail

                        Dim Mailer As MailMessage = New MailMessage
                        'objStreamWriter.WriteLine("Place3")

                        Dim xmlEmailElement As XmlElement = Nothing

                        ' get "email" node of the configuration file
                        If Not (m_xmlConfig("configuration")("email") Is Nothing) Then
                            xmlEmailElement = m_xmlConfig("configuration")("email")
                        End If
                        Dim strTo As String = ""

                        ' get additional recipient (as TO) email addresses
                        If Not (xmlEmailElement("additionalTo").ChildNodes Is Nothing) Then
                            If xmlEmailElement("additionalTo").ChildNodes.Count > 0 Then
                                For Each toItem As XmlNode In xmlEmailElement("additionalTo").ChildNodes
                                    If toItem.Name = "toItem" And Not (toItem.Attributes("addy").InnerText Is Nothing) Then
                                        If toItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                            strTo += toItem.Attributes("addy").InnerText.Trim & ";"
                                        End If
                                    End If
                                Next
                            End If
                        End If
                        If Trim(strTo) = "" Then
                            strTo = "Donna.Ciampoli@sdi.com;"
                        End If

                        ' add extra To based on BU
                        Dim strExtraToBasedOnBusUnit As String = ""
                        strExtraToBasedOnBusUnit = GetExtraToBasedBusUnit(strBusUnitMy)

                        If Trim(strExtraToBasedOnBusUnit) <> "" Then
                            strTo = strTo & strExtraToBasedOnBusUnit
                        End If

                        Dim strBcc As String = ""

                        ' get additional recipient (as BCC) email addresses
                        If Not (xmlEmailElement("additionalBcc").ChildNodes Is Nothing) Then
                            If xmlEmailElement("additionalBcc").ChildNodes.Count > 0 Then
                                For Each bccItem As XmlNode In xmlEmailElement("additionalBcc").ChildNodes
                                    If bccItem.Name = "bccItem" And Not (bccItem.Attributes("addy").InnerText Is Nothing) Then
                                        If bccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                            strBcc += bccItem.Attributes("addy").InnerText.Trim & ";"
                                        End If
                                    End If
                                Next
                            End If
                        End If
                        If Trim(strBcc) = "" Then
                            strBcc = "WebDev@sdi.com"
                        End If

                        With Mailer

                            .From = "TechSupport@sdi.com"
                            .To = strTo    '  "vitaly.rovensky@sdi.com"
                            If connectOR.DataSource.ToUpper = "RPTG" Or _
                                    connectOR.DataSource.ToUpper = "SNBX" Or _
                                    connectOR.DataSource.ToUpper = "DEVL" Then

                                .To = "DoNotSendRPTG@sdi.com"
                            End If
                            .Bcc = strBcc

                            dDate = dDate.AddDays(-1)

                            .Subject = "No Autocrib Transaction for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString()
                            .Priority = MailPriority.High
                            .BodyFormat = MailFormat.Html
                            .Body = "<html><body><table><tr><td>No transactions processed for site " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") via autocrib web services on " & dDate.ToShortDateString() & "</td></tr>"


                        End With
                        'objStreamWriter.WriteLine("Place4")

                        bReslt = sendemail(Mailer)
                        'objStreamWriter.WriteLine("Place5")
                        If bReslt = False Then
                            objStreamWriter.WriteLine("  Error sending email to: " & Mailer.To & ", from: " & Mailer.From & ", - at " & Now().ToString())
                        Else
                            objStreamWriter.WriteLine("E-mail was sent successfully for " & dr.Item("ISA_COMPANY_ID") & " BU: " & strBusUnitMy & "")
                        End If
                    Else
                        If ds1.Tables(0).Rows.Count > 0 Then
                            bReslt = True
                            objStreamWriter.WriteLine("There are " & ds1.Tables(0).Rows.Count & " transactions for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
                        End If
                    End If
                Else
                    objStreamWriter.WriteLine("No data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
                End If
            Else
                objStreamWriter.WriteLine("No data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
            End If
        Catch ex As Exception
            bReslt = False
            objStreamWriter.WriteLine("Error retrieving data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
            objStreamWriter.WriteLine("Error: " & ex.Message)
        End Try

        Return bReslt
    End Function

    Private Function GetExtraToBasedBusUnit(ByVal strBusUnitMy As String) As String
        Dim strExtraTo As String = ""
        Dim strMySettngsValue As String = ""
        If Trim(strBusUnitMy) <> "" Then
            strBusUnitMy = UCase(Trim(strBusUnitMy))
            Select Case strBusUnitMy
                Case "I0906", "I0907", "I0908"
                    Try
                        strMySettngsValue = My.Settings("BU_I090X_VISTEON").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0278"
                    Try
                        strMySettngsValue = My.Settings("BU_I0278").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0405"
                    Try
                        strMySettngsValue = My.Settings("BU_I0405").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0277"
                    Try
                        strMySettngsValue = My.Settings("BU_I0277").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0440"
                    Try
                        strMySettngsValue = My.Settings("BU_I0440").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0450"
                    Try
                        strMySettngsValue = My.Settings("BU_I0450").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case "I0469"
                    Try
                        strMySettngsValue = My.Settings("BU_I0469").ToString.Trim
                    Catch ex As Exception
                        strMySettngsValue = ""
                    End Try
                    If Trim(strMySettngsValue) <> "" Then
                        strExtraTo = strMySettngsValue
                    Else
                        strExtraTo = ""
                    End If
                Case Else
                    strExtraTo = ""
            End Select
        End If  '  Trim(strBusUnitMy) <> ""

        Return strExtraTo
    End Function

    Private Function sendemail(ByVal mailer As MailMessage) As Boolean
        Dim bNoError As Boolean = False
        Try
            'SmtpMail.Send(mailer)

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, "", "", "N", mailer.Body, connectOR)

            SendEmail1(mailer)

            bNoError = True
        Catch ex As Exception
            bNoError = False
        End Try
        Return bNoError
    End Function

    Private Sub SendErrEmail()

        Dim email As New MailMessage
        email.From = "TechSupport@sdi.com"
        email.To = "vitaly.rovensky@sdi.com"

        email.Subject = "Auto Crib Transactions Email OUT Error"
        email.Priority = MailPriority.High
        email.BodyFormat = MailFormat.Html
        email.Body = "<html><body><table><tr><td>CheckAutoCribTransactions has completed with errors, review log.</td></tr>"
        email.Bcc = "vitaly.rovensky@sdi.com"

        'Send the email and handle any error that occurs
        Try
            'SmtpMail.Send(email)
            SendEmail1(email)
        Catch
            objStreamWriter.WriteLine("     Error - an Error email was not sent")
        End Try

    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try
            'SendLogger(eml.Subject, eml.Body, "QUOTEAPPROVAL", "Mail", eml.To, eml.Cc, eml.Bcc)
            SendLogger(mailer.Subject, mailer.Body, "CHECKAUTOCRIBTRANS", "Mail", mailer.To, "", mailer.Bcc, mailer.From)

            '' old code
            'UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, "", "", "N", mailer.Body, connectOR)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, _
                   ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, ByVal EmailFrom As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            SDIEmailService.EmailUtilityServices(MailType, EmailFrom, EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())
            ' '   http://ims.sdi.com:8913/SDIEmailSvc/EmailServices.asmx
        Catch ex As Exception

        End Try
    End Sub

End Module
