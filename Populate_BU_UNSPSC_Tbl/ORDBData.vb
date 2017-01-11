Imports System.Data.OleDb

Public Class ORDBData

    Public Shared Function UnilogGetAdapter(ByVal p_strQuery As String) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

            dataAdapter.Fill(UserdataSet)
            dataAdapter.Dispose()
            Command.Dispose()
            connection.Dispose()
            connection.Close()
            Return UserdataSet

        Catch objException As Exception
            connection.Dispose()
            connection.Close()
            Throw objException
        End Try

    End Function

    Public Shared ReadOnly Property UnilogDbUrl() As String
        Get
            Return My.Settings.UnilogDbUrl
        End Get
    End Property

    Public Shared ReadOnly Property DbUrl As String
        Get
            Return My.Settings.DbUrl
        End Get
    End Property

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String) As Integer
        Dim rowsAffected As Integer = 0
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)

        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            Command.Dispose()
            connection.Dispose()
            connection.Close()

        Catch objException As Exception
            connection.Dispose()
            connection.Close()
            Throw objException
        End Try

        Return rowsAffected
    End Function

    Public Shared Function ExecuteNonQuery(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
                strSQLstring As String, Optional ByRef strError As String = "") As Integer

        Dim iRowsaffected As Integer = 0

        Dim cmd As OleDbCommand = New OleDbCommand(strSQLstring, connection)
        Try
            cmd.CommandTimeout = 120
            cmd.Transaction = trnsactSession
            iRowsaffected = cmd.ExecuteNonQuery()
            cmd.Dispose()
        Catch exNonQ As Exception
            iRowsaffected = 0
            Try
                cmd.Dispose()
            Catch ex As Exception

            End Try
            strError = exNonQ.Message
        End Try

        Return iRowsaffected
    End Function
End Class
