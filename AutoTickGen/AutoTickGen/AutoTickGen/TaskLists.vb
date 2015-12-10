Imports System.Xml.Serialization

Public Class TaskLists
    Inherits SerializableData

    <XmlIgnore()> Public Items As New ArrayList()
    Public Property NewTicketInformation As TicketInfo()
        Get
            Dim infoArray(Items.Count - 1) As TicketInfo
            Items.CopyTo(infoArray)
            Return infoArray
        End Get
        Set(ByVal value As TicketInfo())
            Items.Clear()
            If Not value Is Nothing Then
                Dim TicketInfo As TicketInfo
                For Each TicketInfo In value
                    Items.Add(TicketInfo)
                Next
            End If
        End Set
    End Property
End Class
