Public Class punchOutGrpSites

    Private m_arr As ArrayList = Nothing
    Private m_grpId As String = ""
    Private m_grpName As String = ""

    Public Sub New(ByVal grpId As String)
        m_grpId = grpId
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return m_grpId
        End Get
    End Property

    Public Property [Name]() As String
        Get
            Return m_grpName
        End Get
        Set(ByVal Value As String)
            m_grpName = Value
        End Set
    End Property

    Public ReadOnly Property Sites() As ArrayList
        Get
            If (m_arr Is Nothing) Then
                m_arr = New ArrayList
            End If
            Return m_arr
        End Get
    End Property

    Public ReadOnly Property easySiteIdSearch() As String
        Get
            Dim s As String = ""
            If Not (m_arr Is Nothing) Then
                For Each site As punchOutGrpSite In m_arr
                    s &= site.Id & "~"
                Next
            End If
            Return s
        End Get
    End Property

End Class

Public Class punchOutGrpSite

    Private m_siteId As String = ""
    Private m_siteName As String = ""
    Private m_siteCustId As String = ""

    Public Sub New(ByVal siteId As String)
        m_siteId = siteId
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return m_siteId
        End Get
    End Property

    Public Property [Name]() As String
        Get
            Return m_siteName
        End Get
        Set(ByVal Value As String)
            m_siteName = Value
        End Set
    End Property

    Public Property CustomerId() As String
        Get
            Return m_siteCustId
        End Get
        Set(ByVal Value As String)
            m_siteName = Value
        End Set
    End Property

End Class
