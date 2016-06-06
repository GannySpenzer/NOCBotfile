Public Class orderLineInfo

    Public Sub New()

    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = ""
            If Not (m_parent Is Nothing) Then
                s &= m_parent.Id & "." & m_lineNo.ToString(Format:="000")
            Else
                s &= (New orderShipmentInfo).Id & "." & m_lineNo.ToString(Format:="000")
            End If
            Return (s)
        End Get
    End Property

    Private m_parent As orderShipmentInfo = Nothing

    Public Property [Parent]() As orderShipmentInfo
        Get
            Return m_parent
        End Get
        Set(ByVal Value As orderShipmentInfo)
            m_parent = Value
        End Set
    End Property

    Private m_lineNo As Integer = 0

    Public Property [LineNo]() As Integer
        Get
            Return m_lineNo
        End Get
        Set(ByVal Value As Integer)
            m_lineNo = Value
        End Set
    End Property

    Private m_qtyPicked As Decimal = 0

    Public Property [QuantityPicked]() As Decimal
        Get
            Return m_qtyPicked
        End Get
        Set(ByVal Value As Decimal)
            m_qtyPicked = Value
        End Set
    End Property

    Private m_shipDT As String = ""

    Public Property [ShipDateTime]() As String
        Get
            Return m_shipDT
        End Get
        Set(ByVal Value As String)
            m_shipDT = Value
        End Set
    End Property

    Private m_workOrderNo As String = ""

    Public Property [WorkOrderNo]() As String
        Get
            Return m_workOrderNo
        End Get
        Set(ByVal Value As String)
            m_workOrderNo = Value
        End Set
    End Property

End Class
