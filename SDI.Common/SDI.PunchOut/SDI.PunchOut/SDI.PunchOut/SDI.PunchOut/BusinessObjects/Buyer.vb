Public Class Buyer

    Private m_buyerCookie As String = ""
    Private m_email As String = ""
    Private m_name As String = ""
    Private m_costCenter As String = ""

    Public Sub New()

    End Sub

    Public Property SessionCookie() As String
        Get
            Return m_buyerCookie
        End Get
        Set(ByVal Value As String)
            m_buyerCookie = Value
        End Set
    End Property

    Public Property EMail() As String
        Get
            Return m_email
        End Get
        Set(ByVal Value As String)
            m_email = Value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal Value As String)
            m_name = Value
        End Set
    End Property

    Public Property CostCenter() As String
        Get
            Return m_costCenter
        End Get
        Set(ByVal Value As String)
            m_costCenter = Value
        End Set
    End Property

End Class
