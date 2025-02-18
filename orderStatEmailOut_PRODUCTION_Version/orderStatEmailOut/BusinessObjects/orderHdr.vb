Public Class orderHdr

    Public Sub New()
        InitMembers()
    End Sub

    Public [BusinessUnit] As String
    Public [OrderNo] As String
    Public [RecipientId] As String
    Public [RecipientName] As String
    Public [RecipientEmailAddress] As String
    Public [ShipToLocation] As String
    Public [IsFabOrder] As Boolean
    Public [ShipToLocationName] As String

    '// collection of orderLine object type
    Private m_arrOrderLines As New ArrayList

    Public ReadOnly Property [OrderLines]() As ArrayList
        Get
            Return (m_arrOrderLines)
        End Get
    End Property

    Private Sub InitMembers()
        Me.BusinessUnit = ""
        Me.OrderNo = ""
        Me.RecipientId = ""
        Me.RecipientName = ""
        Me.RecipientEmailAddress = ""
        Me.ShipToLocation = ""
        Me.IsFabOrder = False
        Me.ShipToLocationName = ""
    End Sub

    Public Function isOrderFullyShipped() As Boolean
        Dim bIsAllOrderLinesShipped As Boolean = False
        If Me.OrderLines.Count > 0 Then
            bIsAllOrderLinesShipped = True
            For Each o As orderLine In Me.OrderLines
                If o.ItemQuantity = 0 Or _
                   o.ShippedDate.Trim.Length = 0 Then
                    bIsAllOrderLinesShipped = False
                    Exit For
                End If
            Next
        End If
        Return (bIsAllOrderLinesShipped)
    End Function

End Class
