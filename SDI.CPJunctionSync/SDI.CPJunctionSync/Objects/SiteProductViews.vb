Imports System.Data.SqlClient

Public Class SiteProductViews

    Public Sub New()

    End Sub

    Public Sub New(ByVal sqlCN As SqlConnection, _
                   ByVal qry As String, _
                   ByVal catalogProductViewList As CatalogProductViews)
        BuildCatalogProductViewIdReferenceList(sqlCN, qry, catalogProductViewList)
    End Sub

    Private m_arr As Hashtable = Nothing

    Public ReadOnly Property Item As Hashtable
        Get
            If (m_arr Is Nothing) Then
                m_arr = New Hashtable
            End If
            Return m_arr
        End Get
    End Property

    Public Function GetSite(ByVal siteId As String) As ArrayList
        Dim arr As New ArrayList
        Dim oSite As SiteProductView = Nothing
        If Me.Item.Count > 0 Then
            Dim oEnumerator As IDictionaryEnumerator = Me.Item.GetEnumerator
            While oEnumerator.MoveNext
                oSite = CType(oEnumerator.Value, SiteProductView)
                If (oSite.SiteId = siteId) Then
                    arr.Add(oSite)
                End If
            End While
        End If
        Return (arr)
    End Function

    Public Function GetSite(ByVal productViewId As Integer) As ArrayList
        Dim arr As New ArrayList
        Dim oSite As SiteProductView = Nothing
        If Me.Item.Count > 0 Then
            Dim oEnumerator As IDictionaryEnumerator = Me.Item.GetEnumerator
            While oEnumerator.MoveNext
                oSite = CType(oEnumerator.Value, SiteProductView)
                If (oSite.ProductViewId = productViewId) Then
                    arr.Add(oSite)
                End If
            End While
        End If
        Return (arr)
    End Function

    Public Sub BuildCatalogProductViewIdReferenceList(ByVal sqlCN As SqlConnection, _
                                                      ByVal qry As String, _
                                                      ByVal catalogProductViewList As CatalogProductViews)
        Dim cmd As SqlCommand = sqlCN.CreateCommand
        cmd.CommandText = qry
        cmd.CommandType = CommandType.Text

        Dim rdr As SqlDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sBusinessUnit As String = ""
            Dim sSiteId As String = ""
            Dim nProductViewId As Integer = 0
            Dim nId As Integer = 0
            Dim sSiteItemPrefix As String = ""
            Dim sCompanyShortName As String = ""
            Dim sSiteIndicatorFlag As String = ""
            Dim nCatalogId As Integer = 0

            Dim oSite As SiteProductView = Nothing

            While rdr.Read
                oSite = Nothing

                sBusinessUnit = ""
                Try
                    sBusinessUnit = CStr(rdr("isa_business_unit")).Trim.ToUpper
                Catch ex As Exception
                End Try

                sSiteId = ""
                Try
                    sSiteId = CStr(rdr("isa_site_id")).Trim.ToUpper
                Catch ex As Exception
                End Try

                nProductViewId = 0
                Try
                    nProductViewId = CInt(rdr("PRODUCT_VIEW_ID"))
                Catch ex As Exception
                End Try

                nId = 0
                Try
                    nId = CInt(rdr("ID"))
                Catch ex As Exception
                End Try

                sSiteItemPrefix = ""
                Try
                    sSiteItemPrefix = CStr(rdr("PREFIX")).Trim.ToUpper
                Catch ex As Exception
                End Try

                sCompanyShortName = ""
                Try
                    sCompanyShortName = CStr(rdr("ISA_COMPANY_ID")).Trim.ToUpper
                Catch ex As Exception
                End Try

                sSiteIndicatorFlag = ""
                Try
                    sSiteIndicatorFlag = CStr(rdr("SITE_INDICATOR_FLAG")).Trim.ToUpper
                Catch ex As Exception
                End Try

                nCatalogId = 0
                Try
                    nCatalogId = CInt(catalogProductViewList.Item(nProductViewId))
                Catch ex As Exception
                End Try

                oSite = New SiteProductView(sBusinessUnit, _
                                            sSiteId, _
                                            nProductViewId, _
                                            nId, _
                                            sSiteItemPrefix, _
                                            sSiteIndicatorFlag, _
                                            nCatalogId)
                oSite.SiteShortName = sCompanyShortName

                If Not Me.Item.ContainsKey(nId) Then
                    Me.Item.Add(nId, oSite)
                End If

                oSite = Nothing
            End While

            Try
                rdr.Close()
            Catch ex As Exception
            End Try
        End If

        rdr = Nothing

        Try
            cmd.Dispose()
        Catch ex As Exception
        End Try
        cmd = Nothing
    End Sub

End Class
