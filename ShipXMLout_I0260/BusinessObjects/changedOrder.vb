Public Class changedOrder

    Public Sub New()

    End Sub

    Public Sub New(ByVal bu As String, ByVal orderNo As String)
        m_bu = bu
        m_orderNo = orderNo
    End Sub

    Private m_orderNo As String = ""

    Public Property [OrderNo]() As String
        Get
            Return m_orderNo
        End Get
        Set(ByVal Value As String)
            m_orderNo = Value
        End Set
    End Property

    Private m_bu As String = ""

    Public Property [BusinessUnit]() As String
        Get
            Return m_bu
        End Get
        Set(ByVal Value As String)
            m_bu = Value
        End Set
    End Property

End Class
