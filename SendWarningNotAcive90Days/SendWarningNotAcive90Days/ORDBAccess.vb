Imports System.Data.OleDb
Public Class ORDBAccess
    Public Shared Function GetAdapter(ByVal p_strQuery As String, ByVal connection As OleDbConnection, Optional ByRef strError As String = "") As DataSet

        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            dataAdapter.Fill(UserdataSet)
            connection.Close()
            Return UserdataSet
        Catch objException As Exception
            strError = objException.ToString()
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            Return Nothing
        End Try

    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As OleDbDataReader
        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

    End Function

    Public Shared Function GetScalar(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As String
        Try

            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim strReturn As String
            strReturn = Command.ExecuteScalar()
            connection.Close()
            Return strReturn
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As Integer

        Dim rowsAffected As Integer

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            connection.Close()
            Return rowsAffected
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try
    End Function

End Class
