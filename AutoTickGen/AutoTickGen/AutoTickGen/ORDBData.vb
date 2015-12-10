Imports System.Data.OleDb
Imports System.Configuration

Public Class ORDBData

    Public Shared Function GetAdapter(ByVal p_strQuery As String) As DataSet
        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)

        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)

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

        Catch objException As Exception
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
        End Try

        Return UserdataSet

    End Function

    Public Shared ReadOnly Property DbUrl() As String
        Get
            If My.Settings.ProductionMode.ToUpper = "TRUE" Then
                Return My.Settings.oraCNStringPROD
            Else
                Return My.Settings.oraCNString 'MSDAORA.1 'OraOLEDB.Oracle.1
            End If
        End Get
    End Property
End Class
