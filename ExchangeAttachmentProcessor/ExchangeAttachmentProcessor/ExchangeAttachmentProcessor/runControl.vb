Imports System.Xml.Serialization
Imports XMLSerialization

Public Class runControl
    Inherits SerializableData

    <XmlIgnore()> Public Items As New ArrayList()

    Public Function AddRunInformation() As runInformation
        Dim newRunInfo As New runInformation()

        Items.Add(newRunInfo)

        Return newRunInfo

    End Function
    Public Property runsInformation() As runInformation()
        Get
            Dim infoArray(Items.Count - 1) As runInformation
            Items.CopyTo(infoArray)
            Return infoArray
        End Get
        Set(ByVal value As runInformation())
            Items.Clear()
            If Not value Is Nothing Then
                Dim runInformation As runInformation
                For Each runInformation In value
                    Items.Add(runInformation)
                Next
            End If
        End Set
    End Property
End Class
