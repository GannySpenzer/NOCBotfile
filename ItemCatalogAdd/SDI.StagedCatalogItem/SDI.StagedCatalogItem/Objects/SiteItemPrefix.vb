Public Class SiteItemPrefix

    Private m_isaSiteId As String = ""
    Private m_itemPrefix As String = ""

    Public Sub New()

    End Sub

    Public Sub New(ByVal sIsaSiteId As String, _
                   ByVal sItemPrefix As String)
        m_isaSiteId = sIsaSiteId
        m_itemPrefix = sItemPrefix
    End Sub

    Public Property ISA_SITE_ID As String
        Get
            Return m_isaSiteId
        End Get
        Set(value As String)
            m_isaSiteId = value
        End Set
    End Property

    Public Property ITEM_PREFIX As String
        Get
            Return m_itemPrefix
        End Get
        Set(value As String)
            m_itemPrefix = value
        End Set
    End Property

End Class
