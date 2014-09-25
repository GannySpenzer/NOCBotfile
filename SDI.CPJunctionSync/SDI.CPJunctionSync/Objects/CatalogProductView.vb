Public Class CatalogProductView

    Private m_catalogId As Integer = 0
    Private m_productViewId As Integer = 0

    Public Sub New()

    End Sub

    Public Sub New(ByVal catalogId As Integer, _
                   ByVal productViewId As Integer)
        m_catalogId = catalogId
        m_productViewId = productViewId
    End Sub

    Public Property CatalogId As Integer
        Get
            Return m_catalogId
        End Get
        Set(value As Integer)
            m_catalogId = value
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

End Class
