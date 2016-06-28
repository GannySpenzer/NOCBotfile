Imports System.Data.OleDb
Public Class ORDBAccess
    Public Shared Function GetAdapter(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As DataSet

        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            dataAdapter.Fill(UserdataSet)
            connection.close()
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            Return Nothing
        End Try

        Return UserdataSet

    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As OleDbDataReader
        Dim datareader As OleDbDataReader
        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception

            Try
                connection.Close()
            Catch ex As Exception

            End Try
            Return Nothing
        End Try

    End Function

    Public Shared Function GetScalar(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As String
        Dim strReturn As String
        Try

            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            strReturn = Command.ExecuteScalar()
            connection.Close()
        Catch objException As Exception
            strReturn = ""
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

        Return strReturn

    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As Integer

        Dim rowsAffected As Integer = 0

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            connection.Close()
            Return rowsAffected
        Catch objException As Exception
            rowsAffected = 0
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

        Return rowsAffected

    End Function

End Class
