Public Class PrintedPickedOrderLines

    Public Sub New()

    End Sub

    Private m_arrLines As ArrayList = Nothing

    Public ReadOnly Property Items() As ArrayList
        Get
            If (m_arrLines Is Nothing) Then
                m_arrLines = New ArrayList
            End If
            Return m_arrLines
        End Get
    End Property

End Class

Public Class PrintedPickedOrderLine

    'ORDER_NO
    'ORDER_INT_LINE_NO
    'SCHED_LINE_NO
    'INV_ITEM_ID
    'SHIP_CNTR_ID
    'DEMAND_SOURCE

    Private m_orderNo As String = ""
    Private m_orderLineNo As Integer = -1
    Private m_schedLineNo As Integer = -1
    Private m_invItemId As String = ""
    Private m_containerId As String = ""
    Private m_demandSource As String = ""

    Public Sub New()

    End Sub

    Public Sub New(ByVal orderNo As String, _
                   ByVal orderLineNo As Integer, _
                   ByVal schedLineNo As Integer, _
                   ByVal invItemId As String, _
                   ByVal containerId As String, _
                   ByVal demandSource As String)
        m_orderNo = orderNo
        m_orderLineNo = orderLineNo
        m_schedLineNo = schedLineNo
        m_invItemId = invItemId
        m_containerId = containerId
        m_demandSource = demandSource
    End Sub

    Public Property OrderNo() As String
        Get
            Return m_orderNo
        End Get
        Set(ByVal Value As String)
            m_orderNo = Value
        End Set
    End Property

    Public Property OrderLineNo() As Integer
        Get
            Return m_orderLineNo
        End Get
        Set(ByVal Value As Integer)
            m_orderLineNo = Value
        End Set
    End Property

    Public Property SchedLineNo() As Integer
        Get
            Return m_schedLineNo
        End Get
        Set(ByVal Value As Integer)
            m_schedLineNo = Value
        End Set
    End Property

    Public Property InvItemId() As String
        Get
            Return m_invItemId
        End Get
        Set(ByVal Value As String)
            m_invItemId = Value
        End Set
    End Property

    Public Property ContainerId() As String
        Get
            Return m_containerId
        End Get
        Set(ByVal Value As String)
            m_containerId = Value
        End Set
    End Property

    Public Property DemandSource() As String
        Get
            Return m_demandSource
        End Get
        Set(ByVal Value As String)
            m_demandSource = Value
        End Set
    End Property

End Class
