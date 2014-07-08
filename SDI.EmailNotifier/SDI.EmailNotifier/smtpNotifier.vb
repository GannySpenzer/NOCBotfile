Imports System.Net

Public Class smtpNotifier

    Implements IEmailNotifier

    Private m_to As String
    Private m_from As String
    Private m_cc As String
    Private m_bcc As String
    Private m_subject As String
    Private m_body As String
    Private m_lastSendProcessMsg As String
    Private m_lastSendProcessState As Enums.LastSendProcessStatus

    Public Sub New()
        MyBase.New()
        InitMembers()
    End Sub

    Private Sub InitMembers()
        m_to = ""
        m_from = ""
        m_cc = ""
        m_bcc = ""
        m_subject = ""
        m_body = ""
        m_lastSendProcessMsg = ""
        m_lastSendProcessState = Enums.LastSendProcessStatus.Unknown
    End Sub

#Region " IEmailNotifier implementation "

    Public Property BCC As String Implements IEmailNotifier.BCC
        Get
            Return m_bcc
        End Get
        Set(value As String)
            m_bcc = value
        End Set
    End Property

    Public Property BODY As String Implements IEmailNotifier.BODY
        Get
            Return m_body
        End Get
        Set(value As String)
            m_body = value
        End Set
    End Property

    Public Property CC As String Implements IEmailNotifier.CC
        Get
            Return m_cc
        End Get
        Set(value As String)
            m_cc = value
        End Set
    End Property

    Public Property FROM As String Implements IEmailNotifier.FROM
        Get
            Return m_from
        End Get
        Set(value As String)
            m_from = value
        End Set
    End Property

    Public ReadOnly Property LastSendProcessMessage As String Implements IEmailNotifier.LastSendProcessMessage
        Get
            Return m_lastSendProcessMsg
        End Get
    End Property

    Public ReadOnly Property LastSendProcessState As Enums.LastSendProcessStatus Implements IEmailNotifier.LastSendProcessState
        Get
            Return m_lastSendProcessState
        End Get
    End Property

    Public Sub Send() Implements IEmailNotifier.Send

        Dim srv As Mail.SmtpClient = Nothing
        Dim msg As Mail.MailMessage = Nothing

        ' smtp server
        srv = New Mail.SmtpClient
        srv.Host = "localhost"
        srv.Port = 25
        srv.UseDefaultCredentials = True
        srv.DeliveryMethod = Mail.SmtpDeliveryMethod.Network

        ' message
        msg = New Mail.MailMessage
        If (m_to.Trim.Length > 0) Then
            msg.To.Add(addresses:=m_to)
        Else
            Throw New ApplicationException(message:="Invalid recipient. Specify a valid email [TO] address.")
        End If
        If (m_from.Trim.Length > 0) Then
            msg.From = New Mail.MailAddress(address:=m_from)
        Else
            Throw New ApplicationException(message:="Invalid sender. Specify a valid email [FROM] address.")
        End If
        If (m_cc.Trim.Length > 0) Then
            msg.CC.Add(addresses:=m_cc)
        End If
        If (m_bcc.Trim.Length > 0) Then
            msg.Bcc.Add(addresses:=m_bcc)
        End If
        If (m_subject.Trim.Length > 0) Then
            msg.Subject = m_subject
        End If
        If (m_body.Trim.Length > 0) Then
            msg.Body = m_body
        End If
        msg.IsBodyHtml = True

        Dim bEmailSent As Boolean = False

        ' sending
        Try
            srv.Send(message:=msg)
            bEmailSent = True
            ' set "last send process" state/msg
            m_lastSendProcessMsg = "send successful"
            m_lastSendProcessState = Enums.LastSendProcessStatus.Successful
        Catch ex As Exception
            bEmailSent = False
            m_lastSendProcessMsg = "unable to send email : " & ex.ToString
            m_lastSendProcessState = Enums.LastSendProcessStatus.Error
        End Try

        ' clean-up
        Try
            msg.Dispose()
        Catch ex As Exception
        Finally
            msg = Nothing
        End Try

        Try
            srv.Dispose()
        Catch ex As Exception
        Finally
            srv = Nothing
        End Try

    End Sub

    Public Property SUBJECT As String Implements IEmailNotifier.SUBJECT
        Get
            Return m_subject
        End Get
        Set(value As String)
            m_subject = value
        End Set
    End Property

    Public Property [TO] As String Implements IEmailNotifier.TO
        Get
            Return m_to
        End Get
        Set(value As String)
            m_to = value
        End Set
    End Property

#End Region

End Class
