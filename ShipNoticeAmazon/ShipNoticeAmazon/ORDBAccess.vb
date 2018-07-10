Imports System.Data.OleDb
Public Class ORDBAccess
    Public Shared Function GetAdapter(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As DataSet

        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            If Not connection.State = ConnectionState.Open Then
                connection.Open()
            End If

            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            dataAdapter.Fill(UserdataSet)
            connection.close()
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            UserdataSet = Nothing
        End Try
        Return UserdataSet

    End Function

    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return My.Settings("oraCNString1").ToString.Trim
        End Get
    End Property

    Public Shared Function GetReader(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As OleDbDataReader
        Dim datareader As OleDbDataReader = Nothing
        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            If Not connection.State = ConnectionState.Open Then
                connection.Open()
            End If
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            datareader = Nothing
        End Try
        Return datareader

    End Function

    Public Shared Function GetReaderTrans(ByVal p_strQuery As String, ByRef connection As OleDbConnection, ByRef trnsactSession As OleDbTransaction) As OleDbDataReader
        Dim datareader As OleDbDataReader = Nothing
        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            If Not connection.State = ConnectionState.Open Then
                connection.Open()
            End If
            Command.Transaction = trnsactSession
            datareader = Command.ExecuteReader()
        Catch objException As Exception
            
            datareader = Nothing
        End Try
        Return datareader

    End Function

    Public Shared Function GetScalar(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As String
        Dim strReturn As String = ""
        Try

            Dim Command = New OleDbCommand(p_strQuery, connection)
            If Not connection.State = ConnectionState.Open Then
                connection.Open()
            End If
            strReturn = Command.ExecuteScalar()
            connection.Close()
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            strReturn = ""
        End Try
        Return strReturn
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, ByVal connection As OleDbConnection) As Integer

        Dim rowsAffected As Integer

        Try
            Dim Command = New OleDbCommand(p_strQuery, connection)
            If Not connection.State = ConnectionState.Open Then
                connection.Open()
            End If
            rowsAffected = Command.ExecuteNonQuery()
            connection.Close()
            Return rowsAffected
        Catch objException As Exception
            Try
                connection.Close()
            Catch ex As Exception

            End Try
            Return 0
        End Try
    End Function

End Class
