Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web
Imports System.Text
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Xml

Module Module1

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\Program Files (x86)\SDI\CheckDeluxeDelivery"
    Dim logpath As String = "C:\Program Files (x86)\SDI\CheckDeluxeDelivery\LOGS\CheckDeluxeDelivery" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=PROD")
    'Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")

    Sub Main()

        Console.WriteLine("Started to check deluxe delivery status - sub main")
        Console.WriteLine("")

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Started check deluxe delivery status at " & Now().ToString())

        m_xmlConfig = New XmlDocument
        m_xmlConfig.Load(filename:=m_configFile)

        Dim bWasDeluxeProcessedToday As Boolean = False
        bWasDeluxeProcessedToday = CheckDeluxe()

        If Not bWasDeluxeProcessedToday Then
            'holiday or something wrong - send email
            'SendDeluxeEmail()
            SendErrEmail()
        End If

        objStreamWriter.WriteLine("End of check deluxe delivery at " & Now().ToString())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function CheckDeluxe() As Boolean
        Dim bResult As Boolean = False

        Dim strSQLstring As String
        ' selct all customers with AutoCrib installed
        'strSQLstring = "select distinct ship_cntr_id from sdix_deluxe_delivered where trunc(add_dttm) = trunc(sysdate)"
        strSQLstring = "select count(1) from sdix_deluxe_delivered where trunc(add_dttm) = trunc(sysdate)"

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
        Dim countDeluxe As Integer
        Try
            countDeluxe = CInt(Command.ExecuteScalar)
        Catch ex As Exception
            objStreamWriter.WriteLine("error selecting from sdix_deluxe_delivered table")
            objStreamWriter.WriteLine("         " & ex.Message)
        Finally
            connectOR.Close()
        End Try

        objStreamWriter.WriteLine(countDeluxe.ToString & " containers/rows processed in sdix_deluxe_delivered table on " & Now().ToString())
        If countDeluxe = 0 Then
            connectOR.Close()
            Return False
        Else
            Return True
        End If

        'connectOR.Open()
        'Dim dataAdapter As OleDbDataAdapter = _
        '            New OleDbDataAdapter(Command)
        'Dim ds As System.Data.DataSet = New System.Data.DataSet

        'Try
        '    dataAdapter.Fill(ds)
        '    connectOR.Close()
        'Catch ex As Exception
        '    objStreamWriter.WriteLine("error selecting from sdix_deluxe_delivered table")
        '    objStreamWriter.WriteLine("         " & ex.Message)
        '    connectOR.Close()
        'End Try

        'objStreamWriter.WriteLine(ds.Tables(0).Rows.Count & " containers/rows processed in sdix_deluxe_delivered table on " & Now().ToString())
        'If ds.Tables(0).Rows.Count = 0 Then
        '    connectOR.Close()
        '    Return False
        'Else
        '    Return True
        'End If

    End Function

    'Private Function CheckAndSendCustEmail(ByVal dr As DataRow, Optional ByVal bDo As Boolean = False) As Boolean
    '    Dim bReslt As Boolean = False
    '    '  ISA_COMPANY_ID " & dr.Item("ISA_COMPANY_ID") & "   My.Settings("oraCNString1").ToString.Trim

    '    Dim strSQLstring As String
    '    'Dim strFirstName As String
    '    'Dim strLastName As String
    '    Dim strBusUnitMy As String = dr.Item("isa_business_unit")

    '    strSQLstring = "select * from SYSADM8.ps_isa_autocrb_trx " & vbCrLf & _
    '                " where business_unit='" & strBusUnitMy & "' " & vbCrLf & _
    '                " AND INV_ITEM_ID != ' ' and trunc(dt_timestamp, 'DDD') > trunc(sysdate - 2, 'DDD')"

    '    Dim dDate As Date = CDate(Now())
    '    Try
    '        If bDo Then
    '            objStreamWriter.WriteLine(" Sample SQL string: " & strSQLstring)
    '        End If
    '        Dim Command1 As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)

    '        Dim dataAdapter1 As OleDbDataAdapter = _
    '                    New OleDbDataAdapter(Command1)
    '        Dim ds1 As System.Data.DataSet = New System.Data.DataSet
    '        'objStreamWriter.WriteLine("Place1")
    '        dataAdapter1.Fill(ds1)
    '        'Try
    '        '    objStreamWriter.WriteLine("Place2 " & ds1.Tables.Count)
    '        'Catch ex As Exception
    '        '    objStreamWriter.WriteLine("Error: Place2")
    '        'End Try
    '        'Try
    '        '    objStreamWriter.WriteLine("Place2-A " & ds1.Tables(0).Rows.Count)
    '        'Catch ex As Exception
    '        '    objStreamWriter.WriteLine("Error: Place2-A")
    '        'End Try

    '        If Not ds1 Is Nothing Then
    '            If ds1.Tables.Count > 0 Then
    '                Dim iMy As Integer = ds1.Tables(0).Rows.Count

    '                If iMy = 0 Then
    '                    'send e-mail

    '                    Dim Mailer As MailMessage = New MailMessage
    '                    'objStreamWriter.WriteLine("Place3")

    '                    Dim xmlEmailElement As XmlElement = Nothing

    '                    ' get "email" node of the configuration file
    '                    If Not (m_xmlConfig("configuration")("email") Is Nothing) Then
    '                        xmlEmailElement = m_xmlConfig("configuration")("email")
    '                    End If
    '                    Dim strTo As String = ""

    '                    ' get additional recipient (as TO) email addresses
    '                    If Not (xmlEmailElement("additionalTo").ChildNodes Is Nothing) Then
    '                        If xmlEmailElement("additionalTo").ChildNodes.Count > 0 Then
    '                            For Each toItem As XmlNode In xmlEmailElement("additionalTo").ChildNodes
    '                                If toItem.Name = "toItem" And Not (toItem.Attributes("addy").InnerText Is Nothing) Then
    '                                    If toItem.Attributes("addy").InnerText.Trim.Length > 0 Then
    '                                        strTo += toItem.Attributes("addy").InnerText.Trim & ";"
    '                                    End If
    '                                End If
    '                            Next
    '                        End If
    '                    End If
    '                    If Trim(strTo) = "" Then
    '                        strTo = "Donna.Ciampoli@sdi.com;"
    '                    End If

    '                    ' add extra To based on BU
    '                    Dim strExtraToBasedOnBusUnit As String = ""
    '                    'strExtraToBasedOnBusUnit = GetExtraToBasedBusUnit(strBusUnitMy)

    '                    If Trim(strExtraToBasedOnBusUnit) <> "" Then
    '                        strTo = strTo & strExtraToBasedOnBusUnit
    '                    End If

    '                    Dim strBcc As String = ""

    '                    ' get additional recipient (as BCC) email addresses
    '                    If Not (xmlEmailElement("additionalBcc").ChildNodes Is Nothing) Then
    '                        If xmlEmailElement("additionalBcc").ChildNodes.Count > 0 Then
    '                            For Each bccItem As XmlNode In xmlEmailElement("additionalBcc").ChildNodes
    '                                If bccItem.Name = "bccItem" And Not (bccItem.Attributes("addy").InnerText Is Nothing) Then
    '                                    If bccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
    '                                        strBcc += bccItem.Attributes("addy").InnerText.Trim & ";"
    '                                    End If
    '                                End If
    '                            Next
    '                        End If
    '                    End If
    '                    If Trim(strBcc) = "" Then
    '                        strBcc = "WebDev@sdi.com"
    '                    End If

    '                    With Mailer

    '                        .From = "TechSupport@sdi.com"
    '                        .To = strTo    '  "vitaly.rovensky@sdi.com"
    '                        If connectOR.DataSource.ToUpper = "RPTG" Or _
    '                                connectOR.DataSource.ToUpper = "SNBX" Or _
    '                                connectOR.DataSource.ToUpper = "STAR" Or _
    '                                connectOR.DataSource.ToUpper = "DEVL" Then

    '                            .To = "DoNotSendRPTG@sdi.com"
    '                        End If
    '                        .Bcc = strBcc

    '                        dDate = dDate.AddDays(-1)

    '                        .Subject = "No Autocrib Transaction for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString()
    '                        .Priority = MailPriority.High
    '                        .BodyFormat = MailFormat.Html
    '                        .Body = "<html><body><table><tr><td>No transactions processed for site " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") via autocrib web services on " & dDate.ToShortDateString() & "</td></tr>"


    '                    End With
    '                    'objStreamWriter.WriteLine("Place4")

    '                    bReslt = SendEmail1(Mailer)
    '                    'objStreamWriter.WriteLine("Place5")
    '                    If bReslt = False Then
    '                        objStreamWriter.WriteLine("  Error sending email to: " & Mailer.To & ", from: " & Mailer.From & ", - at " & Now().ToString())
    '                    Else
    '                        objStreamWriter.WriteLine("E-mail was sent successfully for " & dr.Item("ISA_COMPANY_ID") & " BU: " & strBusUnitMy & "")
    '                    End If
    '                Else
    '                    If ds1.Tables(0).Rows.Count > 0 Then
    '                        bReslt = True
    '                        objStreamWriter.WriteLine("There are " & ds1.Tables(0).Rows.Count & " transactions for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
    '                    End If
    '                End If
    '            Else
    '                objStreamWriter.WriteLine("No data from SYSADM8.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
    '            End If
    '        Else
    '            objStreamWriter.WriteLine("No data from SYSADM8.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
    '        End If
    '    Catch ex As Exception
    '        bReslt = False
    '        objStreamWriter.WriteLine("Error retrieving data from SYSADM8.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") on " & dDate.ToShortDateString())
    '        objStreamWriter.WriteLine("Error: " & ex.Message)
    '    End Try

    '    Return bReslt
    'End Function

    'Private Sub SendDeluxeEmail()
    '    Dim bReslt As Boolean = False
    '    Dim Mailer As MailMessage = New MailMessage

    '    With Mailer

    '        .From = "TechSupport@sdi.com"
    '        .To = "yury.arkadin@sdi.com"
    '        'If connectOR.DataSource.ToUpper = "RPTG" Or _
    '        '        connectOR.DataSource.ToUpper = "SNBX" Or _
    '        '        connectOR.DataSource.ToUpper = "STAR" Or _
    '        '        connectOR.DataSource.ToUpper = "DEVL" Then

    '        '    .To = "DoNotSendRPTG@sdi.com"
    '        'End If
    '        .Bcc = "yury.arkadin@sdi.com; vitaly.rovensky@sdi.com"

    '        'dDate = dDate.AddDays(-1)

    '        .Subject = "No Deluxe Delivery transactions for " & Now().ToString()
    '        .Priority = MailPriority.High
    '        .BodyFormat = MailFormat.Html
    '        '.Body = "<html><body><table><tr><td>No transactions processed for site " & dr.Item("ISA_COMPANY_ID") & " (BU: " & strBusUnitMy & ") via autocrib web services on " & dDate.ToShortDateString() & "</td></tr>"
    '        .Body = "<html><body><table><tr><td>No Deluxe deliveries were processed on " & Now().ToString() & "</td></tr>"


    '    End With
    '    'objStreamWriter.WriteLine("Place4")

    '    Try
    '        SendEmail1(Mailer)
    '        objStreamWriter.WriteLine("Check Deluxe E-mail with warning was sent successfully")

    '        'objStreamWriter.WriteLine("Place5")
    '    Catch ex As Exception
    '        objStreamWriter.WriteLine("Check Deluxe E-Mail Error sending on " & Now().ToString())
    '    End Try

    'End Sub

    Private Sub SendErrEmail()

        Dim email As New MailMessage
        email.From = "TechSupport@sdi.com"
        'email.To = "michael.marrinan@sdi.com; rashmi.gupta@sdi.com; ron.fijalkowski@sdi.com; scott.doyle@sdi.com; wenjia.zhang@sdi.com; brian.akom@sdi.com"
        Dim stest As String = My.Settings("EMailList").ToString.Trim
        If connectOR.DataSource.ToUpper = "RPTG" Then
            email.To = "webdev@sdi.com"
        Else
            email.To = My.Settings("EMailList").ToString.Trim
        End If

        email.Subject = "Check Deluxe Delivery Warning - No Deluxe Delivery transactions on " & Now().ToShortDateString()
        email.Priority = MailPriority.High
        email.BodyFormat = MailFormat.Html
        email.Body = "<html><body><table><tr><td>No Deluxe Delivery transactions on " & Now().ToShortDateString() & ".  Check log.</td></tr>"
        email.Cc = "webdev@sdi.com"
        email.Bcc = ""

        'Send the email and handle any error that occurs
        Try
            'SmtpMail.Send(email)
            SendEmail1(email)
            objStreamWriter.WriteLine("Warning Email sent at " & Now().ToString())
        Catch
            objStreamWriter.WriteLine("     Error - an Error email was not sent")
        End Try

    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "CHECKDELUXENYC", "Mail", mailer.To, "", mailer.Bcc, mailer.From)
            'SendLogger(mailer.Subject, mailer.Body, "CHECKAUTOCRIBTRANS", "Mail", mailer.To, "", mailer.Bcc, mailer.From)

        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - an Error email was not sent.  Error: " & ex.ToString)
        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, _
               ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, ByVal EmailFrom As String)
        Try
            Dim SDIEmailService As SDIEmailUtility.EmailServices = New SDIEmailUtility.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            'Dim objException As String
            'Dim objExceptionTrace As String

            SDIEmailService.EmailUtilityServices(MailType, EmailFrom, EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())
            ' '   http://ims.sdi.com:8913/SDIEmailSvc/EmailServices.asmx
        Catch ex As Exception

        End Try
    End Sub

End Module
