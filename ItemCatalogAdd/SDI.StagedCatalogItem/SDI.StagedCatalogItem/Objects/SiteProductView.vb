Public Class SiteProductView

    Private m_isaSiteId As String = ""
    Private m_productViewId As Integer = 0

    Public Sub New()

    End Sub

    Public Sub New(ByVal sIsaSiteId As String, _
                   ByVal nProductViewId As Integer)
        m_isaSiteId = sIsaSiteId
        m_productViewId = nProductViewId
    End Sub

    Public Property IsaSiteId As String
        Get
            Return m_isaSiteId
        End Get
        Set(value As String)
            m_isaSiteId = value
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
