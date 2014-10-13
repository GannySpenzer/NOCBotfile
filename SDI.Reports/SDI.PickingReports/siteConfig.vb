Public Class siteConfig

    Private m_siteId As String = ""
    Private m_arr As New Hashtable


    Public Sub New()

    End Sub

    Public Sub New(ByVal siteId As String)
        m_siteId = siteId
    End Sub

    Public Property SiteId() As String
        Get
            Return m_siteId
        End Get
        Set(ByVal Value As String)
            m_siteId = Value
        End Set
    End Property

    Public ReadOnly Property KeyValue() As Hashtable
        Get
            If (m_arr Is Nothing) Then
                m_arr = New Hashtable
            End If
            Return m_arr
        End Get
    End Property

End Class
