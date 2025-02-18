Public Class orderLine

    Public Sub New()
        InitMembers()
    End Sub

    Public [ParentOrder] As orderHdr
    Public [LineNo] As Int32
    Public [ItemId] As String
    Public [ItemDesc] As String
    Public [ItemQuantity] As Decimal
    Public [ShippedDate] As String
    Public [DemandLineNo] As Int32
    Public [ReceiverId] As String
    Public [ReceiverLineNo] As Int32
    Public [OrderIntLineNo] As Int32
    Public [WorkOrderNo] As String

    Private Sub InitMembers()
        Me.ParentOrder = Nothing
        Me.LineNo = 0
        Me.ItemId = ""
        Me.ItemDesc = ""
        Me.ItemQuantity = 0
        Me.ShippedDate = ""
        Me.DemandLineNo = 0
        Me.ReceiverId = ""
        Me.ReceiverLineNo = 0
        Me.OrderIntLineNo = 0
        Me.WorkOrderNo = ""
    End Sub

End Class
