Imports System.IO
Imports System.Data
Imports System.Data.OleDb
'Imports System.Web.Mail
Imports System.Web
Imports System.Text
'Imports System.Web.Services
'Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Xml

Module Module1

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\Program Files (x86)\SDI\CheckDeluxeDelivery"
    Dim logpath As String = "C:\Program Files (x86)\SDI\CheckDeluxeDelivery\LOGS\CheckDeluxeDelivery" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=PROD")

    Sub Main()

        'Console.WriteLine("Started to check Auto Crib Transactions ")
        'Console.WriteLine("")

        'objStreamWriter = File.CreateText(logpath)
        'objStreamWriter.WriteLine("Started to check Auto Crib Transactions " & Now())

        'm_xmlConfig = New XmlDocument
        'm_xmlConfig.Load(filename:=m_configFile)

        'Dim bolError As Boolean = False
        'bolError = CheckAutoCribTrans()

        'If bolError Then
        '    SendErrEmail()
        'End If

        'objStreamWriter.WriteLine("End of Auto Crib Transactions Email send " & Now())

        'objStreamWriter.Flush()
        'objStreamWriter.Close()


    End Sub

    'Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

    '    Try

    '        SendLogger(mailer.Subject, mailer.Body, "CHECKDELUXENYC", "Mail", mailer.To, mailer.Cc, mailer.Bcc, mailer.From)

    '    Catch ex As Exception

    '    End Try
    'End Sub

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
