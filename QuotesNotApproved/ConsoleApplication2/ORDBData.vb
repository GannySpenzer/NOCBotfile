Imports System.Data.OleDb
Imports System.Web.UI.Page
Imports System.Web.HttpApplication
Imports System.Configuration.ConfigurationSettings

Public Class ORDBData

    Public Shared Function GetAdapter(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
       ' Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
        Try
            Dim connection = New OleDbConnection(("Provider=MSDAORA.1;Password=EINTERNET;User ID=EINTERNET;Data Source=prod"))
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

            dataAdapter.Fill(UserdataSet)
            connection.close()
            Return UserdataSet
        Catch objException As Exception
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
        End Try

    End Function

    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
        Try
            Dim connection = New OleDbConnection(DbUrl)
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

            dataAdapter.Fill(UserdataSet)
            connection.close()
            Return UserdataSet
        Catch objException As Exception
            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            currentApp.Response.Write("<hr>")
            currentApp.Response.Write("<li>  The database may be down for routine maintenance.")
            currentApp.Response.Write(" Please try again in a few minutes...<br>")
            currentApp.Response.Write("Click <a href='default.aspx'>HERE</a> to return to login page.")
            currentApp.Response.End()
        End Try

    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String) As OleDbDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
        Try
            Dim connection = New OleDbConnection(DbUrl)
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception
            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            currentApp.Response.Write("<hr>")
            currentApp.Response.Write("<li>Message: " & objException.Message)
            currentApp.Response.Write("<li>Source: " & objException.Source)
            currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            currentApp.Response.Write("<li>SQL: " & p_strQuery)
            currentApp.Response.End()
        End Try

    End Function

    Public Shared Function GetScalar(ByVal p_strQuery As String) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
        Dim strReturn As String
        Try
            Dim connection = New OleDbConnection(DbUrl)
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            strReturn = Command.ExecuteScalar()
            connection.close()
            Return strReturn
        Catch objException As Exception
            If strReturn Is Nothing Then
                Return ""
            End If
            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            currentApp.Response.Write("<hr>")
            currentApp.Response.Write("<li>Message: " & objException.Message)
            currentApp.Response.Write("<li>Source: " & objException.Source)
            currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            currentApp.Response.Write("<li>SQL: " & p_strQuery)
            currentApp.Response.End()
        End Try
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String) As Integer

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

        Dim rowsAffected As Integer

        Try
            Dim connection = New OleDbConnection(DbUrl)
            Dim Command = New OleDbCommand(p_strQuery, connection)
            connection.open()
            rowsAffected = Command.ExecuteNonQuery()
            connection.close()
            Return rowsAffected
        Catch objException As Exception
            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            currentApp.Response.Write("<hr>")
            currentApp.Response.Write("<li>Message: " & objException.Message)
            currentApp.Response.Write("<li>Source: " & objException.Source)
            currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            currentApp.Response.Write("<li>SQL: " & p_strQuery)
            currentApp.Response.End()
        End Try

    End Function


    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return System.Configuration.ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property



End Class
