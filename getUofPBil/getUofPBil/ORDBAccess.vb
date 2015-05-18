Imports System.Data.OleDb
Public Class ORDBAccess
    Public Shared Function GetAdapter(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As DataSet

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

            dataAdapter.Fill(UserdataSet)
            connection.close()
            Return UserdataSet
        Catch objException As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & objException.ToString)
            Console.WriteLine("")

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
            Console.WriteLine("")
            Console.WriteLine("***error - " & objException.ToString)
            Console.WriteLine("")
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
            Console.WriteLine("")
            Console.WriteLine("***error - " & objException.ToString)
            Console.WriteLine("")
        End Try
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As Integer

        Dim rowsAffected As Integer

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            'connection.open()
            rowsAffected = Command.ExecuteNonQuery()
            'connection.close()
            Return rowsAffected
        Catch objException As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & objException.ToString)
            Console.WriteLine("")
        End Try
    End Function

End Class
