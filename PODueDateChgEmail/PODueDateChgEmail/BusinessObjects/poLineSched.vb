Public Class poLineSched

    Private m_poLn As poLine = Nothing

    Public Sub New()

    End Sub

    Public Sub New(ByVal parentPOLine As poLine)
        m_poLn = parentPOLine
    End Sub

    Public Sub New(ByVal parentPOLine As poLine, ByVal schedNo As Integer)
        m_poLn = parentPOLine
        m_poSchedLineNo = schedNo
    End Sub

    Public Property ParentPOLine() As poLine
        Get
            Return m_poLn
        End Get
        Set(ByVal Value As poLine)
            m_poLn = Value
        End Set
    End Property

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = ""
            If Not (m_poLn Is Nothing) Then
                s &= m_poLn.Id & "." & Me.PurchaseOrderLineScheduleNo.ToString(Format:="00000")
            Else
                s &= (New poLine).Id & "." & Me.PurchaseOrderLineScheduleNo.ToString(Format:="00000")
            End If
            Return (id)
        End Get

    End Property

    Public m_Employee_ID As String = ""

    Public Property [EmployeeID]() As String
        Get
            Return m_Employee_ID
        End Get
        Set(ByVal Value As String)
            m_Employee_ID = Value
        End Set
    End Property

    Private m_inventoryBU As String = ""

    Public Property InventoryBusinessUnit() As String
        Get
            Return m_inventoryBU
        End Get
        Set(ByVal Value As String)
            m_inventoryBU = Value
        End Set
    End Property

    Private m_poSchedLineNo As Integer = -1

    Public Property [PurchaseOrderLineScheduleNo]() As Integer
        Get
            Return m_poSchedLineNo
        End Get
        Set(ByVal Value As Integer)
            m_poSchedLineNo = Value
        End Set
    End Property

    Private m_poDueDT As String = ""

    Public Property [PurchaseOrderDueDate]() As String
        Get
            Return m_poDueDT
        End Get
        Set(ByVal Value As String)
            m_poDueDT = Value
        End Set
    End Property

    Private m_originalPromisedDate As String = ""

    Public Property [OriginalPromisedDate]() As String
        Get
            Return m_originalPromisedDate
        End Get
        Set(ByVal Value As String)
            m_originalPromisedDate = Value
        End Set
    End Property

    Private m_orderNo As String = ""

    Public Property [OrderNo]() As String
        Get
            Return m_orderNo
        End Get
        Set(ByVal Value As String)
            m_orderNo = Value
        End Set
    End Property

    Private m_orderLineNo As Integer = -1

    Public Property [OrderLineNo]() As Integer
        Get
            Return m_orderLineNo
        End Get
        Set(ByVal Value As Integer)
            m_orderLineNo = Value
        End Set
    End Property

End Class
