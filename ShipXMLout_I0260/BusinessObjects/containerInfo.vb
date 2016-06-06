Public Class containerInfo

    Public Sub New()

    End Sub

    Public Sub New(ByVal parent As orderShipmentInfo, _
                   ByVal containerId As String, _
                   ByVal workOrderNo As String)
        m_parent = parent
        m_containerId = containerId
        m_workOrderNo = workOrderNo
    End Sub

    Public Sub New(ByVal containerId As String, _
                   ByVal workOrderNo As String)
        m_containerId = containerId
        m_workOrderNo = workOrderNo
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = ""
            If Not (m_parent Is Nothing) Then
                s &= m_parent.Id & "." & m_containerId
            Else
                s &= (New orderShipmentInfo).Id & "." & Me.ContainerId
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

    Private m_containerId As String

    Public Property [ContainerId]() As String
        Get
            Return m_containerId
        End Get
        Set(ByVal Value As String)
            m_containerId = Value
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

    Private m_shipDT As String = ""

    Public Property ShipDateTime() As String
        Get
            Return m_shipDT
        End Get
        Set(ByVal Value As String)
            m_shipDT = Value
        End Set
    End Property

End Class
