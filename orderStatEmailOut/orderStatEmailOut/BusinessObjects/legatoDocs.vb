Imports System.Data
Imports System.Data.SqlClient


Public Class legatoDocs

    Public Overloads Shared Function IsScannedDocExists(ByVal orderNo As String, _
                                                        ByVal sqlCNstring As String) As Boolean
        Dim rtn As String = "legatoScannedDoc.IsScannedDocExists:orderNo,sqlCNstring"
        Dim bIsScannedDocExists As Boolean = False
        Try

            orderNo = orderNo.Trim.ToUpper

            Dim cn As New SqlConnection(sqlCNstring)
            cn.Open()
            If cn.State = ConnectionState.Open Then
                Dim sql As String = "" & _
                                    "SELECT COUNT(1) AS RECS " & vbCrLf & _
                                    "FROM OTG..NYCSCANQRY " & vbCrLf & _
                                    "WHERE ORDER_NO = '" & orderNo & "' " & vbCrLf & _
                                    ""
                Dim cmd As SqlCommand = cn.CreateCommand
                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text
                Dim recs As Integer = CInt(cmd.ExecuteScalar)
                bIsScannedDocExists = (recs > 0)
                Try
                    cmd.Dispose()
                Catch ex As Exception
                Finally
                    cmd = Nothing
                End Try
                Try
                    cn.Close()
                Catch ex As Exception
                End Try
            Else       'If cn.State = ConnectionState.Open Then
                ' unable to open connection
                '    throw as exception
                Throw New ApplicationException(message:="unable to open connection : " & sqlCNstring & "")
            End If     'If cn.State = ConnectionState.Open Then
            Try
                cn.Dispose()
            Catch ex As Exception
            Finally
                cn = Nothing
            End Try

        Catch ex As Exception
            bIsScannedDocExists = False
            Throw New ApplicationException(rtn & "::" & ex.ToString)
        End Try
        Return (bIsScannedDocExists)
    End Function


End Class
