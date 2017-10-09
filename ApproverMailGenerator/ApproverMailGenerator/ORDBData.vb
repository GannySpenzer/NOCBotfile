Imports System.Data.OleDb
Imports System.Configuration
Public Class ORDBData
    Public Shared Function GetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim strReturn As String = ""
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Try
                strReturn = CType(Command.ExecuteScalar(), String)
            Catch ex32 As Exception
                strReturn = ""
            End Try
            If strReturn Is Nothing Then
                strReturn = ""
            End If
            Try
                Command.Dispose()
            Catch ex1 As Exception

            End Try
            Try
                connection.Close()
                connection.Dispose()
            Catch ex2 As Exception

            End Try
            'connection.close()
        Catch objException As Exception
            strReturn = ""
            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception
            End Try
        End Try

        Return strReturn
    End Function

    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet
        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

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
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property

    Public Shared Function GetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception
            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception

            End Try
        End Try

    End Function

    Public Shared Function GetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, Optional ByVal bThrowBackError As Boolean = False) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

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
            'connection.close()
            Return UserdataSet
        Catch objException As Exception
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As Integer
        Dim rowsAffected As Integer = 0
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)

        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
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
            'connection.close()
        Catch objException As Exception
            rowsAffected = 0
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

        Return rowsAffected
    End Function

    Public Shared Function ExecuteNonQuery(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
                                           strSQLstring As String, ByRef rowsaffected As Integer, ByRef exError As Exception) As Boolean
        Dim bSuccessful As Boolean = False

        Try
            Dim cmd As OleDbCommand = New OleDbCommand(strSQLstring, connection)
            cmd.CommandTimeout = 120
            cmd.Transaction = trnsactSession
            rowsaffected = cmd.ExecuteNonQuery()
            bSuccessful = True
        Catch ex As Exception
            bSuccessful = False
            exError = ex
        End Try

        Return bSuccessful
    End Function
End Class
