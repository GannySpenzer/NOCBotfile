Public Class myItem

    Public Sub New()

    End Sub

    Public Sub New(ByVal [id] As String, ByVal [name] As String)
        m_id = id
        m_name = name
    End Sub

    Private m_id As String = ""

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Private m_name As String = ""

    Public Property [Name]() As String
        Get
            Return m_name
        End Get
        Set(ByVal Value As String)
            m_name = Value
        End Set
    End Property

End Class
