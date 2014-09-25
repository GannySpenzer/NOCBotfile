Public Class SiteProductView

    Private m_businessUnit As String = ""
    Private m_siteId As String = ""
    Private m_productViewId As Integer = 0
    Private m_id As Integer = 0
    Private m_siteItemPrefix As String = ""
    Private m_companyShortName As String = ""
    Private m_siteIndicatorFlag As String = ""
    Private m_catalogId As Integer = 0

    Public Sub New()

    End Sub

    Public Sub New(ByVal businessUnit As String, _
                    ByVal siteId As String, _
                    ByVal productViewId As Integer, _
                    ByVal id As Integer, _
                    ByVal siteItemPrefix As String, _
                    ByVal siteIndicatorFlag As String, _
                    ByVal catalogId As Integer)
        m_businessUnit = businessUnit
        m_siteId = siteId
        m_productViewId = productViewId
        m_id = id
        m_siteItemPrefix = siteItemPrefix
        m_siteIndicatorFlag = siteIndicatorFlag
        m_catalogId = catalogId
    End Sub

    Public Property BusinessUnit As String
        Get
            Return m_businessUnit
        End Get
        Set(value As String)
            m_businessUnit = value
        End Set
    End Property

    Public Property SiteId As String
        Get
            Return m_siteId
        End Get
        Set(value As String)
            m_siteId = value
        End Set
    End Property

    Public Property ProductViewId As Integer
        Get
            Return m_productViewId
        End Get
        Set(value As Integer)
            m_productViewId = value
        End Set
    End Property

    Public Property Id As Integer
        Get
            Return m_id
        End Get
        Set(value As Integer)
            m_id = value
        End Set
    End Property

    Public Property SiteItemPrefix As String
        Get
            Return m_siteItemPrefix
        End Get
        Set(value As String)
            m_siteItemPrefix = value
        End Set
    End Property

    Public Property SiteIndicatorFlag As String
        Get
            Return m_siteIndicatorFlag
        End Get
        Set(value As String)
            m_siteIndicatorFlag = value
        End Set
    End Property

    Public Property SiteShortName As String
        Get
            Return m_companyShortName
        End Get
        Set(value As String)
            m_companyShortName = value
        End Set
    End Property

    Public Property CatalogId As Integer
        Get
            Return m_catalogId
        End Get
        Set(value As Integer)
            m_catalogId = value
        End Set
    End Property

End Class
