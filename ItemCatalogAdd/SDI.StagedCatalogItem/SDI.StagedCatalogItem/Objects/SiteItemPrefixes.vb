Imports System.Data.OleDb
Imports System.Data.SqlClient


Public Class SiteItemPrefixes

    Private m_arr As Hashtable = Nothing

    Public Sub New()

    End Sub

    Public Sub New(ByVal sqlCN As SqlConnection, _
                   ByVal qry As String)
        BuildSiteItemPrefixList(sqlCN, qry)
    End Sub

    Public ReadOnly Property Item As Hashtable
        Get
            If (m_arr Is Nothing) Then
                m_arr = New Hashtable
            End If
            Return m_arr
        End Get
    End Property

    Public Sub BuildSiteItemPrefixList(ByVal sqlCN As SqlConnection, _
                                       ByVal qry As String)
        Dim cmd As SqlCommand = sqlCN.CreateCommand
        cmd.CommandText = qry
        cmd.CommandType = CommandType.Text

        Dim rdr As SqlDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sIsaSiteId As String = ""
            Dim sItemPrefix As String = ""
            Dim sIndicatorFlag As String = ""

            While rdr.Read
                sIsaSiteId = ""
                Try
                    sIsaSiteId = CStr(rdr("ISA_SITE_ID"))
                Catch ex As Exception
                End Try

                sItemPrefix = ""
                Try
                    sItemPrefix = CStr(rdr("ITEM_PREFIX")).Trim.ToUpper
                Catch ex As Exception
                End Try

                sIndicatorFlag = ""
                Try
                    sIndicatorFlag = CStr(rdr("SITE_INDICATOR_FLAG")).Trim.ToUpper
                Catch ex As Exception
                End Try

                Dim sKey As String = sItemPrefix & "." & sIsaSiteId

                If Not Me.Item.ContainsKey(sKey) Then
                    Me.Item.Add(sKey, New String() {sItemPrefix, sIsaSiteId, sIndicatorFlag})
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
