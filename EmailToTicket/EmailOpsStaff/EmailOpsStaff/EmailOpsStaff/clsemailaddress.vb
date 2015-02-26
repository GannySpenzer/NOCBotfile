Imports Microsoft.Exchange.WebServices.Data
Imports Microsoft.Exchange.WebServices.Autodiscover.UserSettingName
Imports XMLSerialization
Public Class Clsemailaddress
    Private strsender As String
    Public ReadOnly Property Sender() As EmailAddress
        Get
            Return strsender
        End Get
    End Property
    Public Sub New(ByVal instance As EmailMessage, ByVal value As EmailAddress, ByVal strsender As String)
        value = instance.Sender
        ' EmailMessage.Bind(Microsoft.Exchange.WebServices.Data.ExchangeService,Microsoft.Exchange.WebServices.Data.WellKnownFolderName))

        'ItemSchema.
        Dim returnValue As EmailMessage
        Dim service As ExchangeService
        Dim id As ItemId
        Dim propertySet As PropertySet
        'Microsoft.Exchange.WebServices.Data.ItemSchema 

        returnValue = EmailMessage.Bind(service, id, propertySet)

        returnValue = EmailMessage.Bind(service, _
         id, propertySet)

        strsender = value.ToString
    End Sub
    ''Declaration

    ''Usage
    'Dim instance As EmailMessage
    'Dim value As EmailAddress

    '        value = instance.Sender

    '        instance.Sender = value

End Class
