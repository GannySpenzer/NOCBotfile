Imports System.IO
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
    Dim rootDir As String = "C:\CheckAutoCribTrans"
    Dim logpath As String = "C:\CheckAutoCribTrans\LOGS\CheckAutoCribTransOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=RPTG")
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

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Send Auto Crib Transactions emails out " & Now())

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
            For I = 0 To ds.Tables(0).Rows.Count - 1
                'objStreamWriter.WriteLine(" Going to check row: " & I.ToString())
                CheckAndSendCustEmail(ds.Tables(0).Rows(I))
            Next
        End If

        Return bResult

    End Function

    Private Function CheckAndSendCustEmail(ByVal dr As DataRow) As Boolean
        Dim bReslt As Boolean = False
        '  ISA_COMPANY_ID " & dr.Item("ISA_COMPANY_ID") & "

        Dim strSQLstring As String
        'Dim strFirstName As String
        'Dim strLastName As String

        strSQLstring = "select * from SYSADM.ps_isa_autocrb_trx " & vbCrLf & _
                    " where business_unit='" & dr.Item("isa_business_unit") & "' " & vbCrLf & _
                    " and trunc(dt_timestamp, 'DDD') > trunc(sysdate - 2, 'DDD')"

        Dim dDate As Date = CDate(Now())
        Try
            objStreamWriter.WriteLine(" strSQLstring: " & strSQLstring)
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
                    If ds1.Tables(0).Rows.Count = 0 Then
                        'send e-mail

                        Dim Mailer As MailMessage = New MailMessage
                        'objStreamWriter.WriteLine("Place3")

                        With Mailer

                            .From = "TechSupport@sdi.com"
                            .To = "vitaly.rovensky@sdi.com"
                            If connectOR.DataSource.ToUpper = "RPTG" Or _
                                    connectOR.DataSource.ToUpper = "SNBX" Or _
                                    connectOR.DataSource.ToUpper = "DEVL" Then

                                .To = "DoNotSendRPTG@sdi.com"
                            End If

                            dDate = dDate.AddDays(-1)

                            .Subject = "No Autocrib Transaction for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") on " & dDate.ToShortDateString()
                            .Priority = MailPriority.High
                            .BodyFormat = MailFormat.Html
                            .Body = "<html><body><table><tr><td>No transactions processed for site " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") via autocrib web services on " & dDate.ToShortDateString() & "</td></tr>"


                        End With
                        'objStreamWriter.WriteLine("Place4")

                        bReslt = sendemail(Mailer)
                        'objStreamWriter.WriteLine("Place5")
                        If bReslt = False Then
                            objStreamWriter.WriteLine("  Error sending email to: " & Mailer.To & ", from: " & Mailer.From & ", - at " & Now().ToString())
                        Else
                            objStreamWriter.WriteLine("E-mail was sent successfully for " & dr.Item("ISA_COMPANY_ID") & " BU: " & dr.Item("isa_business_unit") & "")
                        End If
                    Else
                        If ds1.Tables(0).Rows.Count > 0 Then
                            objStreamWriter.WriteLine("There are " & ds1.Tables(0).Rows.Count & " transactions for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") on " & dDate.ToShortDateString())
                        End If
                    End If
                Else
                    objStreamWriter.WriteLine("No data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") on " & dDate.ToShortDateString())
                End If
            Else
                objStreamWriter.WriteLine("No data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") on " & dDate.ToShortDateString())
            End If
        Catch ex As Exception
            bReslt = False
            objStreamWriter.WriteLine("Error retrieving data from SYSADM.ps_isa_autocrb_trx for " & dr.Item("ISA_COMPANY_ID") & " (BU: " & dr.Item("isa_business_unit") & ") on " & dDate.ToShortDateString())
            objStreamWriter.WriteLine("Error: " & ex.Message)
        End Try

        Return bReslt
    End Function

    Private Function sendemail(ByVal mailer As MailMessage) As Boolean
        Dim bNoError As Boolean = False
        Try
            SmtpMail.Send(mailer)

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, "", "", "N", mailer.Body, connectOR)

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

        'Send the email and handle any error that occurs
        Try
            SmtpMail.Send(email)
        Catch
            objStreamWriter.WriteLine("     Error - an Error email was not sent")
        End Try

    End Sub

End Module
