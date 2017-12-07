Imports System.Data.OleDb
Imports System.Configuration

Module SdiUserMail
    Sub Main()
        UserMail.SendMailSDIUser()
    End Sub
End Module

Public Class UserMail
    Private StrFrom As String = ""
    Public Property [FromAddress]() As String
        Get
            Return StrFrom
        End Get
        Set(value As String)
            StrFrom = value
        End Set
    End Property
    Private StrTo As String = ""
    Public Property [ToAddress]() As String
        Get
            Return StrTo
        End Get
        Set(value As String)
            StrTo = value
        End Set
    End Property
    Private StrCC As String = ""
    Public Property [CcAddress]() As String
        Get
            Return StrCC
        End Get
        Set(value As String)
            StrCC = value
        End Set
    End Property
    Private StrBcc As String = ""
    Public Property [BccAddress]() As String
        Get
            Return StrBcc
        End Get
        Set(value As String)
            StrBcc = value
        End Set
    End Property
    Private StrSubject As String = ""
    Public Property [Subject]() As String
        Get
            Return StrSubject
        End Get
        Set(value As String)
            StrSubject = value
        End Set
    End Property
    Private StrBody As String = ""
    Public Property [MailBody]() As String
        Get
            Return StrBody
        End Get
        Set(value As String)
            StrBody = value
        End Set
    End Property
    Public Shared Sub SendMailSDIUser()
        Try
            '  Dim UserList As String() = {"AVACORP1", "MRANDALL"}
            Dim DS As DataSet = GetUserList()
            Dim Mail As New UserMail
            Mail.MailBody = MailContent()
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            For Each row As DataRow In DS.Tables(0).Rows
                Mail.FromAddress = "SDIExchange@SDI.com"
                Mail.ToAddress = Convert.ToString(row("ISA_EMPLOYEE_EMAIL")).Trim()
                Mail.CcAddress = "webdev@sdi.com;SDIportalsupport@avasoft.com"
                Mail.BccAddress = ""
                Mail.Subject = "SDI user mail - Testing"
                SDIEmailService.EmailUtilityServices("Mail", Mail.FromAddress, Mail.ToAddress, Mail.Subject, Mail.CcAddress, Mail.BccAddress, Mail.MailBody, "Mail for SDIUser", MailAttachmentName, MailAttachmentbytes.ToArray())
            Next
        Catch ex As Exception

        End Try
    End Sub
    Public Shared Function GetUserList() As DataSet
        Dim DS As DataSet = Nothing
        Try
            Dim StrQry As String = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID IN ('AVACORP1','MRANDALL') AND ISA_EMPLOYEE_EMAIL <> '@'"
            DS = ORDBData.GetAdapterSpc(StrQry)
            Return DS
        Catch ex As Exception
            Return DS
        End Try
    End Function
    Public Shared Function MailContent() As String
        Dim StrMailBody As String = ""
        Try
            Dim strbodyhead As String = ""
            Dim strbody As String = ""
            Dim strFooter As String = ""

            strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style=padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center></td></tr></tbody></table>" & vbCrLf
            strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

            strbody = "New email utility services to send mail for bunch of SDI user ID ---> AVASOFT Testing"

            strFooter = strFooter & "&nbsp;<br>"
            strFooter = strFooter & "&nbsp;<br>"
            strFooter = strFooter & "&nbsp;<br>"
            strFooter = strFooter & "Sincerely,<br>"
            strFooter = strFooter & "&nbsp;<br>"
            strFooter = strFooter & "SDI Customer Care<br>"
            strFooter = strFooter & "&nbsp;<br>"
            strFooter = strFooter & "<HR width='100%' SIZE='1'>" & vbCrLf
            strFooter = strFooter & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

            StrMailBody = strbodyhead & strbody & strFooter
            Return StrMailBody
        Catch ex As Exception
            Return StrMailBody
        End Try
    End Function
End Class

Public Class ORDBData
    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet
            dataAdapter.Fill(UserdataSet)
            Try
                dataAdapter.Dispose()
            Catch ex As Exception
            End Try
            Try
                Command.Dispose()
            Catch ex As Exception
            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
            Return UserdataSet
        Catch objException As Exception
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
        End Try
    End Function
    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property
End Class

