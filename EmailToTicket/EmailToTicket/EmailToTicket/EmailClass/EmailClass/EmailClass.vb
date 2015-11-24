Imports System.Net.Mail
Imports System.Collections.Specialized
Imports System.Configuration


Public Class EmailClass

    Private m_emlAlert As System.Net.Mail.MailMessage
    Private m_EmailTo As String
    Private m_EmailBCC As String
    Private m_EmailText As String
    Private m_EmailSubject As String
    Private m_EmailAttachmentPath As String

    Public Sub New()
        EmailTo = ""
        EmailBCC = ""
        EmailSubject = ""
        EmailText = ""
        EmailAttachmentPath = ""
    End Sub
    Public Sub New(ByVal sEmaiTo As String)

        EmailTo = sEmaiTo
        EmailBCC = ""
        EmailSubject = ""
        EmailText = ""
        EmailAttachmentPath = ""

    End Sub

    Public Sub New(ByVal sEmaiTo As String, _
                  ByVal sEmailBCC As String)

        EmailTo = sEmaiTo
        EmailBCC = sEmailBCC
        EmailSubject = ""
        EmailText = ""
        EmailAttachmentPath = ""

    End Sub

    Public Sub New(ByVal sEmaiTo As String, _
               ByVal sEmailBCC As String, _
               ByVal sEmailSubject As String)

        EmailTo = sEmaiTo
        EmailBCC = sEmailBCC
        EmailSubject = sEmailSubject
        EmailText = ""
        EmailAttachmentPath = ""

    End Sub

    Public Sub New(ByVal sEmaiTo As String, _
               ByVal sEmailBCC As String, _
               ByVal sEmailSubject As String, _
               ByVal sEmailText As String)

        EmailTo = sEmaiTo
        EmailBCC = sEmailBCC
        EmailSubject = sEmailSubject
        EmailText = sEmailText
        EmailAttachmentPath = ""
    End Sub
    Public Sub New(ByVal sEmaiTo As String, _
               ByVal sEmailBCC As String, _
               ByVal sEmailSubject As String, _
               ByVal sEmailText As String, _
               ByVal sEmailAttachmentPath As String)

        EmailTo = sEmaiTo
        EmailBCC = sEmailBCC
        EmailSubject = sEmailSubject
        EmailText = sEmailText
        EmailAttachmentPath = sEmailAttachmentPath
    End Sub


    Public Property EmailTo() As String
        Get
            EmailTo = m_EmailTo
        End Get
        Set(ByVal value As String)
            m_EmailTo = value
        End Set
    End Property

    Public Property EmailBCC() As String
        Get
            EmailBCC = m_EmailBCC
        End Get
        Set(ByVal value As String)
            m_EmailBCC = value
        End Set
    End Property

    Public Property EmailText() As String
        Get
            EmailText = m_EmailText
        End Get
        Set(ByVal value As String)
            m_EmailText = value
        End Set
    End Property


    Public Property EmailSubject() As String
        Get
            EmailSubject = m_EmailSubject
        End Get
        Set(ByVal value As String)
            m_EmailSubject = value
        End Set
    End Property
    Public Property EmailAttachmentPath() As String
        Get
            EmailAttachmentPath = m_EmailAttachmentPath
        End Get
        Set(ByVal value As String)
            m_EmailAttachmentPath = value
        End Set
    End Property

    Public Function SendEmail() As Boolean
        Dim bReturn As Boolean = True

        Dim eml As New System.Net.Mail.MailMessage
        Dim emlNew As New System.Net.Mail.MailMessage
        Dim fromAddress As System.Net.Mail.MailAddress
        Dim saEmail As Array


        Try

            fromAddress = New System.Net.Mail.MailAddress("service.notification@sdi.com", "service.notification@sdi.com")
            eml.From = fromAddress

            Try
                If EmailSubject.Length > 0 Then
                    eml.Subject = EmailSubject.Trim
                Else

                End If

            Catch ex As Exception
                bReturn = False

            End Try

            ' get email address list on whom will receives this notification
            Try
                saEmail = EmailTo.ToString.Split(";")
                For iCount As Integer = 0 To saEmail.GetUpperBound(0)
                    If saEmail(iCount).ToString.Length = 0 Then
                        Exit For
                    Else
                        eml.To.Add(saEmail(iCount).ToString.Trim())
                    End If
                Next
            Catch ex As Exception
                bReturn = False

            End Try

            If EmailBCC.Trim.Length > 0 Then
                Try
                    saEmail = EmailBCC.ToString.Split(";")
                    For iCount As Integer = 0 To saEmail.GetUpperBound(0)
                        If saEmail(iCount).ToString.Length = 0 Then
                            Exit For
                        Else
                            eml.Bcc.Add(saEmail(iCount).ToString.Trim())
                        End If
                    Next
                Catch ex As Exception
                    bReturn = False

                End Try
            End If

            eml.Body = EmailText
            eml.Priority = MailPriority.High
            Try
                If EmailAttachmentPath.Length > 0 Then
                    Dim itemAttachment As New System.Net.Mail.Attachment(EmailAttachmentPath)

                    eml.Attachments.Add(itemAttachment)

                End If
            Catch ex As Exception
                eml.Subject = "Error adding Attachment: " & EmailSubject
                eml.Body = "Error adding attachment for this email " & ex.Message & vbCrLf & _
                           eml.Body
            End Try
            Debug.Print(eml.To.ToString)

            Dim myClient As New System.Net.Mail.SmtpClient


#If DEBUG Then
            myClient.Host = "localhost"
            myClient.Port = 25
            myClient.UseDefaultCredentials = True
            myClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            myClient.PickupDirectoryLocation = "c:\smtp"
#Else
            'production only
            myClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis

#End If

            myClient.Send(eml)

            myClient = Nothing
            eml = Nothing
        Catch ex As Exception
            bReturn = False

        End Try

        Return bReturn
    End Function

End Class
