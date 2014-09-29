Public Class item

    Private m_ps_itemId As String = ""
    Private m_ps_itemStatus As String = ""
    Private m_ps_itemDesc As String = ""
    Private m_ps_uom As String = ""
    Private m_mfg As String = ""
    Private m_mfg_partNumber As String = ""

    Private m_cplus_customerItemId As String = ""
    Private m_cplus_itemPrefix As String = ""
    Private m_cplus_classId As Integer = 0
    Private m_cplus_itemId As Integer = 0

    Private m_siteId As String = ""   
    Private m_productViewId As Integer = 0
    Private m_catalogId As Integer = 0

    Public Sub New()

    End Sub

    Public Property PS_ItemId As String
        Get
            Return m_ps_itemId
        End Get
        Set(value As String)
            m_ps_itemId = value
        End Set
    End Property

    Public Property Catalog_CustomerItemPrefix As String
        Get
            Return m_cplus_itemPrefix
        End Get
        Set(value As String)
            m_cplus_itemPrefix = value
        End Set
    End Property

    Public Property Catalog_CustomerItemId As String
        Get
            Return m_cplus_customerItemId
        End Get
        Set(value As String)
            m_cplus_customerItemId = value
        End Set
    End Property

    Public Property PS_Status As String
        Get
            Return m_ps_itemStatus
        End Get
        Set(value As String)
            m_ps_itemStatus = value
        End Set
    End Property

    Public Property PS_UnitOfMeasure As String
        Get
            Return m_ps_uom
        End Get
        Set(value As String)
            m_ps_uom = value
        End Set
    End Property

    Public Property PS_ManufacturerId As String
        Get
            Return m_mfg
        End Get
        Set(value As String)
            m_mfg = value
        End Set
    End Property

    Public Property PS_ManufacturerPartNumber As String
        Get
            Return m_mfg_partNumber
        End Get
        Set(value As String)
            m_mfg_partNumber = value
        End Set
    End Property

    Public ReadOnly Property IsActive As Boolean
        Get
            Return Me.IsItemActive()
        End Get
    End Property

    Private Function IsItemActive() As Boolean
        Dim bIsActive As Boolean = False
        If Not (m_ps_itemStatus Is Nothing) Then
            Select Case m_ps_itemStatus
                Case "A"
                    bIsActive = True
                Case "I"
                    bIsActive = False
                Case Else
                    bIsActive = False
            End Select
        End If
        Return (bIsActive)
    End Function

    Public Property PS_ItemDescription As String
        Get
            Return m_ps_itemDesc
        End Get
        Set(value As String)
            m_ps_itemDesc = value
        End Set
    End Property

    Public Property Catalog_ItemId As Integer
        Get
            Return m_cplus_itemId
        End Get
        Set(value As Integer)
            m_cplus_itemId = value
        End Set
    End Property

    Public Property Catalog_ClassId As Integer
        Get
            Return m_cplus_classId
        End Get
        Set(value As Integer)
            m_cplus_classId = value
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

    Public Property Catalog_ProductViewId As Integer
        Get
            Return m_productViewId
        End Get
        Set(value As Integer)
            m_productViewId = value
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
