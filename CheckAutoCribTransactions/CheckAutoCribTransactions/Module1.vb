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

        Return bResult

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
