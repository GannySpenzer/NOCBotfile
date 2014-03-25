Imports System.data.SqlClient
Public Class SQLDBAccess

    Public Shared Function GetSQLAdapter(ByVal p_strQuery As String, ByVal connection As SqlConnection) As DataSet

        Try
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.open()
            Dim dataAdapter As SqlDataAdapter = _
                    New SqlDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

            dataAdapter.Fill(UserdataSet)
            connection.close()
            Return UserdataSet
        Catch objException As SqlException
            MsgBox(objException.ToString, MsgBoxStyle.Critical)
        End Try

    End Function

    Public Shared Function GetSQLScalar(ByVal p_strQuery As String, ByVal connection As SqlConnection) As String

        Try
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()

            Dim strResults As String = Command.ExecuteScalar()

            If IsDBNull(strResults) Then
                GetSQLScalar = "0"
            Else
                GetSQLScalar = strResults
            End If

            connection.Close()

        Catch objException As SqlException
            MsgBox(objException.ToString, MsgBoxStyle.Critical)
        End Try
    End Function


    Public Shared Function GetSQLReader(ByVal p_strQuery As String, ByVal connection As SqlConnection) As SqlDataReader
        Try
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            GetSQLReader = Command.ExecuteReader(CommandBehavior.CloseConnection)
        Catch objException As SqlException
            MsgBox(objException.ToString, MsgBoxStyle.Critical)
        End Try

    End Function

    Public Shared Function ExecNonQuerySQL(ByVal p_strQuery As String, ByVal connection As SqlConnection) As Integer

        Dim rowsAffected As Integer

        Try
            Dim Command = New SqlCommand(p_strQuery, connection)
             
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            connection.Close()
            Return rowsAffected
        Catch objException As SqlException
            MsgBox(objException.ToString, MsgBoxStyle.Critical)
        End Try

    End Function

End Class
