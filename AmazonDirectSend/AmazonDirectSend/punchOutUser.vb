Public Class punchOutUser

    Public Sub New()

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

    Private m_type As String = ""
    Public Property [Type]() As String
        Get
            Return m_type
        End Get
        Set(ByVal Value As String)
            m_type = Value
        End Set
    End Property

    Private m_bu As String = ""
    Public Property BusinessUnit() As String
        Get
            Return m_bu
        End Get
        Set(ByVal Value As String)
            m_bu = Value
        End Set
    End Property

    Private m_siteBU As String = ""
    Public Property SiteBusinessUnit() As String
        Get
            Return m_siteBU
        End Get
        Set(ByVal Value As String)
            m_siteBU = Value
        End Set
    End Property

End Class
