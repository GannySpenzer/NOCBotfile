Public Class QuotedNStkItemCollection
    Inherits System.Collections.CollectionBase

    Public Function Add(ByVal oItem As QuotedNStkItem) As Integer
        Dim nIdx As Integer = MyBase.List.Add(oItem)
        CType(MyBase.List.Item(nIdx), QuotedNStkItem).IndexInCollection = nIdx
        Return nIdx
    End Function

    Public Sub Remove(ByVal oItem As QuotedNStkItem)
        MyBase.List.Remove(oItem)
    End Sub

    Public Function IndexOf(ByVal oItem As QuotedNStkItem) As Integer
        Return MyBase.List.IndexOf(oItem)
    End Function

    Default Public Property Item(ByVal nIndex As Integer) As QuotedNStkItem
        Get
            Return CType(MyBase.List.Item(nIndex), QuotedNStkItem)
        End Get
        Set(ByVal Value As QuotedNStkItem)
            MyBase.List.Item(nIndex) = Value
        End Set
    End Property
End Class
