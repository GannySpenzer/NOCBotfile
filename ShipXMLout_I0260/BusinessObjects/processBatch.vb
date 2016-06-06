Public Class processBatch

    Public Sub New()

    End Sub

    '// collection of orderShipmentInfo object type
    Private m_arr As ArrayList = Nothing

    Public ReadOnly Property [Orders]() As ArrayList
        Get
            If (m_arr Is Nothing) Then
                m_arr = New ArrayList
            End If
            Return m_arr
        End Get
    End Property

    Public ReadOnly Property [OrderNoList]() As String
        Get
            Dim s As String = ""
            If Me.Orders.Count > 0 Then
                For Each o As orderShipmentInfo In Me.Orders
                    s &= o.Id & "~"
                Next
            End If
            Return (s)
        End Get
    End Property

    Public Function GetOrder(ByVal orderNo As String) As orderShipmentInfo
        Dim o As orderShipmentInfo = Nothing
        If Me.Orders.Count > 0 Then
            For Each i As orderShipmentInfo In Me.Orders
                If i.OrderNo = orderNo Then
                    o = i
                    Exit For
                End If
            Next
        End If
        Return (o)
    End Function

End Class
