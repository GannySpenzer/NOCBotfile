Public Class myIdDesc

    Public Sub New()

    End Sub

    Public Sub New(ByVal sId As String, ByVal sDesc As String)
        m_id = sId
        m_desc = sDesc
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

    Private m_desc As String = ""

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal Value As String)
            m_desc = Value
        End Set
    End Property

End Class
