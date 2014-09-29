Imports System.Data.OleDb
Imports System.Data.SqlClient

Public Class itemFactory

    ' prevent from being instantiated
    Private Sub New()

    End Sub

    Public Shared Function CreateItemInstance(ByVal psItemId As String, _
                                              ByVal psItemStatus As String, _
                                              ByVal psItemDescription As String, _
                                              ByVal catalogItemId As Integer, _
                                              ByVal siteId As String, _
                                              ByVal catalogItemId_cpJunction As Integer, _
                                              ByVal uom As String, _
                                              ByVal mfg As String, _
                                              ByVal mfgPartNumber As String, _
                                              Optional ByVal catalogCustomerItemId As String = Nothing, _
                                              Optional ByVal catalogCustomerItemPrefix As String = Nothing) As item
        Dim boItem As New item

        boItem.PS_ItemId = psItemId
        boItem.PS_Status = psItemStatus
        boItem.PS_ItemDescription = psItemDescription
        boItem.Catalog_ItemId = catalogItemId
        boItem.PS_ManufacturerId = mfg
        boItem.PS_ManufacturerPartNumber = mfgPartNumber

        If Not (catalogCustomerItemPrefix Is Nothing) Then
            boItem.Catalog_CustomerItemPrefix = catalogCustomerItemPrefix
        End If

        If Not (catalogCustomerItemId Is Nothing) Then
            boItem.Catalog_CustomerItemId = catalogCustomerItemId
        End If

        boItem.PS_UnitOfMeasure = uom
        boItem.SiteId = siteId

        ' check the "commodity code" in cp_junction, and this wins over whatever was in ps_inv_items table
        If (catalogItemId_cpJunction > 0) Then
            boItem.Catalog_ItemId = catalogItemId_cpJunction
        End If

        Return (boItem)
    End Function

    Public Shared Function GetItemCatalogId(ByVal oItem As item, _
                                            ByVal oCatalogProductViews As CatalogProductViews, _
                                            ByVal oSiteProductViews As SiteProductViews, _
                                            ByVal sSiteIdForPrefix As String, _
                                            ByVal sSiteIndicatorForPrefix As String) As Integer
        Dim nCatalogId As Integer = oItem.CatalogId

        ' given the existing item's site ID (if available), identify the product view ID and then the catalog ID
        If (nCatalogId = 0) Then
            If (oItem.SiteId.Length > 0) Then
                If oSiteProductViews.Item.ContainsKey(oItem.SiteId) Then
                    ' get the product view ID
                    Dim nProductViewId As Integer = CInt(oSiteProductViews.Item(oItem.SiteId))
                    ' get the catalog ID based on that product view
                    nCatalogId = CInt(oCatalogProductViews.Item(nProductViewId))
                End If
            End If
        End If

        ' if we cannot identify the catalog ID using method above since the site ID was not provided via original item's attributes
        '       use the given site ID via item prefix value
        If (nCatalogId = 0) Then
            If (oItem.Catalog_CustomerItemPrefix.Length > 0) And (sSiteIdForPrefix.Length > 0) Then
                ' get the product view ID
                Dim nProductViewId As Integer = CInt(oSiteProductViews.Item(sSiteIdForPrefix))
                ' get the catalog ID based on that product view
                nCatalogId = CInt(oCatalogProductViews.Item(nProductViewId))
            End If
        End If

        Return (nCatalogId)
    End Function

    Public Shared Function GetProductViewId(ByVal oItem As item, _
                                            ByVal oCatalogProductViews As CatalogProductViews, _
                                            ByVal oSiteProductViews As SiteProductViews, _
                                            ByVal sSiteIdForPrefix As String, _
                                            ByVal sSiteIndicatorForPrefix As String) As Integer
        Dim nProductViewId As Integer = oItem.Catalog_ProductViewId

        ' (1) check for the site Id
        '       and use that to figure out the product view ID if available
        If (nProductViewId = 0) Then
            If (oItem.SiteId.Length > 0) Then
                If oSiteProductViews.Item.ContainsKey(oItem.SiteId) Then
                    nProductViewId = CInt(oSiteProductViews.Item(oItem.SiteId))
                End If
            End If
        End If

        ' (2) if we cannot figure out the product view id for this item because the site ID is not available,
        '       use the customer item ID (item prefix and all) to trace back to site (isa_enterprise table) and grab the product view ID from there
        If (nProductViewId = 0) Then
            If (oItem.Catalog_CustomerItemPrefix.Length > 0) And (sSiteIdForPrefix.Length > 0) Then
                nProductViewId = CInt(oSiteProductViews.Item(sSiteIdForPrefix))
            End If
        End If

        Return (nProductViewId)
    End Function

    Public Shared Function GetSiteId(ByVal oItem As item, _
                                     ByVal oCatalogProductViews As CatalogProductViews, _
                                     ByVal oSiteProductViews As SiteProductViews, _
                                     ByVal sSiteIdForPrefix As String, _
                                     ByVal sSiteIndicatorForPrefix As String) As String
        Dim sSiteId As String = oItem.SiteId

        ' if the site ID was not provided from original item attribute, use the site ID identified using item prefix
        If (sSiteId.Length = 0) Then
            If (oItem.Catalog_CustomerItemPrefix.Length > 0) And (sSiteIdForPrefix.Length > 0) Then
                sSiteId = sSiteIdForPrefix
            End If
        End If

        Return (sSiteId)
    End Function

End Class
