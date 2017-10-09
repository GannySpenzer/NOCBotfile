<Serializable()> _
Public Class sdiCurrency

    Public Sub New()

    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String)
        m_Id = id
        m_desc = desc
    End Sub

    Private m_Id As String = "USD"

    Public Property [Id]() As String
        Get
            Return m_Id
        End Get
        Set(ByVal value As String)
            m_Id = value
        End Set
    End Property

    Private m_symbol As String = "$"

    Public Property [Symbol]() As String
        Get
            Return m_symbol
        End Get
        Set(ByVal value As String)
            m_symbol = value
        End Set
    End Property

    Private m_desc As String = ""

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal value As String)
            m_desc = value
        End Set
    End Property

    Private m_shortDesc As String = ""

    Public Property [ShortDescription]() As String
        Get
            Return m_shortDesc
        End Get
        Set(ByVal value As String)
            m_shortDesc = value
        End Set
    End Property

    Private m_country As String = ""

    Public Property [Country]() As String
        Get
            Return m_country
        End Get
        Set(ByVal value As String)
            m_country = value
        End Set
    End Property

    Private m_bIsKnownCurrency As Boolean = False

    Public Property [IsKnownCurrency]() As Boolean
        Get
            Return m_bIsKnownCurrency
        End Get
        Set(ByVal value As Boolean)
            m_bIsKnownCurrency = value
        End Set
    End Property

End Class
