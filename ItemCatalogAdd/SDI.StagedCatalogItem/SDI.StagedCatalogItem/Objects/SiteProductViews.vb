Imports System.Data.OleDb
Imports System.Data.SqlClient


Public Class SiteProductViews

    Private m_arr As Hashtable = Nothing

    Public Sub New()

    End Sub

    Public Sub New(ByVal oraCN As OleDbConnection, _
                   ByVal qry As String)
        BuildSiteProductViewList(oraCN, qry)
    End Sub

    Public ReadOnly Property Item As Hashtable
        Get
            If (m_arr Is Nothing) Then
                m_arr = New Hashtable
            End If
            Return m_arr
        End Get
    End Property

    Public Sub BuildSiteProductViewList(ByVal oraCN As OleDbConnection, _
                                        ByVal qry As String)
        Dim cmd As OleDbCommand = oraCN.CreateCommand
        cmd.CommandText = qry
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDbDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sIsaSiteId As String = ""
            Dim nProductViewId As Integer = 0

            While rdr.Read
                sIsaSiteId = ""
                Try
                    sIsaSiteId = CStr(rdr("ISA_SITE_ID"))
                Catch ex As Exception
                End Try

                nProductViewId = 0
                Try
                    nProductViewId = CInt(rdr("PRODUCT_VIEW_ID"))
                Catch ex As Exception
                End Try

                If Not Me.Item.ContainsKey(sIsaSiteId) Then
                    Me.Item.Add(sIsaSiteId, nProductViewId)
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
