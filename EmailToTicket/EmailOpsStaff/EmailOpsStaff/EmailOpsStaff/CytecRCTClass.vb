Imports Microsoft.Exchange.WebServices.Data

Public Class CytecRCTClass
    Inherits ExchangeBaseClass


    Public Sub New()
        MailboxName = "CytecRCT@sdi.com"
        SourceFolderName = WellKnownFolderName.Inbox
        ProcessedParentFolder = WellKnownFolderName.Inbox
        ProcessedFolder = "Processed"
        UserName = "pdfuser"
        Password = "0c17c08C"

    End Sub
    Public Sub New(ByVal runInfo As runInformation)
        MailboxName = runInfo.EmailAddress
        If runInfo.ProcessedFolderEnabled Then
            SourceFolderName = WellKnownFolderName.Inbox
            ProcessedParentFolder = WellKnownFolderName.Inbox
            ProcessedFolder = "Processed"
        Else
            'insert last runtime
        End If

        UserName = runInfo.UserName
        Password = runInfo.Password

    End Sub
End Class
