Imports System.Configuration
Imports System.Data.SqlClient

Public Class SQLDBData

    Public Shared Function GetProdSQLScalarSPC3(ByVal p_strQuery As String) As String
        Dim strReturn As String = ""
        Dim connection As SqlConnection = New SqlConnection(DbSQLUrl1)
        Try

            Dim Command As SqlCommand = New SqlCommand(p_strQuery, connection)
            connection.Open()
            strReturn = Command.ExecuteScalar()
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try

        Catch objException As SqlException
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

        Return strReturn
    End Function

    Public Shared Function ExecNonQuerySQLSPC3(ByVal p_strQuery As String) As Integer
        Dim rowsAffected As Integer = 0
        Dim connString As String
        connString = DbSQLUrl1
        Dim connection As SqlConnection = New SqlConnection(connString)

        Try

            Dim Command As SqlCommand = New SqlCommand(p_strQuery, connection)
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try

        Catch objException As SqlException
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
        End Try

        Return rowsAffected
    End Function

    Public Shared Function GetSQLAdapterDazzle(ByVal p_strQuery As String) As DataSet
        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet
        Dim connString As String
        connString = DbSQLUrl1

        Dim connection As SqlConnection = New SqlConnection(connString)
        Try

            Dim Command As SqlCommand = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim dataAdapter As SqlDataAdapter = New SqlDataAdapter(Command)

            dataAdapter.Fill(UserdataSet)
            Try
                dataAdapter.Dispose()
            Catch ex As Exception

            End Try
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
        Catch objException As SqlException
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try

        End Try

        Return UserdataSet
    End Function

    Public Shared ReadOnly Property DbSQLUrl1() As String
        Get
            If My.Settings.ProductionMode.ToUpper = "TRUE" Then
                Return My.Settings.sqlCNStringPROD
            Else
                Return My.Settings.sqlCNString
            End If
        End Get
    End Property
End Class
