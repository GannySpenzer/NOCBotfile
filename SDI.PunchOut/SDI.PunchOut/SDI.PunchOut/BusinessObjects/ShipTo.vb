Public Class ShipTo

    Private m_addressId As String = ""
    Private m_addressName As String = ""
    Private m_name As String = ""
    Private m_deliverTo As String = ""
    Private m_street As String = ""
    Private m_city As String = ""
    Private m_state As String = ""
    Private m_postalCode As String = ""
    Private m_country As String = "United States"
    Private m_countryCode As String = "US"
    Private m_xmlLanguage As String = "en-US"

    Public Sub New()

    End Sub

    Public Property AddressId() As String
        Get
            Return m_addressId
        End Get
        Set(ByVal Value As String)
            m_addressId = Value
        End Set
    End Property

    Public Property AddressName() As String
        Get
            Return m_addressName
        End Get
        Set(ByVal Value As String)
            m_addressName = Value
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

    Public Property DeliverTo() As String
        Get
            Return m_deliverTo
        End Get
        Set(ByVal Value As String)
            m_deliverTo = Value
        End Set
    End Property

    Public Property Street() As String
        Get
            Return m_street
        End Get
        Set(ByVal Value As String)
            m_street = Value
        End Set
    End Property

    Public Property City() As String
        Get
            Return m_city
        End Get
        Set(ByVal Value As String)
            m_city = Value
        End Set
    End Property

    Public Property State() As String
        Get
            Return m_state
        End Get
        Set(ByVal Value As String)
            m_state = Value
        End Set
    End Property

    Public Property ZIPCode() As String
        Get
            Return m_postalCode
        End Get
        Set(ByVal Value As String)
            m_postalCode = Value
        End Set
    End Property

    Public Property Country() As String
        Get
            Return m_country
        End Get
        Set(ByVal Value As String)
            m_country = Value
        End Set
    End Property

    Public Property CountryCode() As String
        Get
            Return m_countryCode
        End Get
        Set(ByVal Value As String)
            m_countryCode = Value
        End Set
    End Property

    Public Property LanguageCode() As String
        Get
            Return m_xmlLanguage
        End Get
        Set(ByVal Value As String)
            m_xmlLanguage = Value
        End Set
    End Property

End Class
