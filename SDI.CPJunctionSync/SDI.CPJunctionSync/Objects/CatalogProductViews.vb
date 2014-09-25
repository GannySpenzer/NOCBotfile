Imports System.Data.SqlClient


Public Class CatalogProductViews

    Public Sub New()

    End Sub

    Public Sub New(ByVal sqlCPlusCN As SqlConnection, _
                   ByVal qry As String)
        BuildCatalogProductViewIdReferenceList(sqlCPlusCN, qry)
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

    Public Sub BuildCatalogProductViewIdReferenceList(ByVal sqlCPlusCN As SqlConnection, _
                                                      ByVal qry As String)
        Dim cmd As SqlCommand = sqlCPlusCN.CreateCommand
        cmd.CommandText = qry
        cmd.CommandType = CommandType.Text

        Dim rdr As SqlDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim nCatalogId As Integer = 0
            Dim nProductViewId As Integer = 0

            While rdr.Read
                nCatalogId = 0
                Try
                    nCatalogId = CInt(rdr("CATALOG_ID"))
                Catch ex As Exception
                End Try

                nProductViewId = 0
                Try
                    nProductViewId = CInt(rdr("PRODUCT_VIEW_ID"))
                Catch ex As Exception
                End Try

                If Not Me.Item.ContainsKey(nProductViewId) Then
                    Me.Item.Add(nProductViewId, nCatalogId)
                End If
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
