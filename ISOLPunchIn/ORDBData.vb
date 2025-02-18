Imports System.Data.OleDb
Imports System.Web.UI.Page

Public Class ORDBData

    Public Shared Function GetAdapter(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection
        Try
            connection = New OleDbConnection(DbUrl)
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

            dataAdapter.Fill(UserdataSet)

            Try
                dataAdapter.Dispose()
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try

            'connection.Close()
            Return UserdataSet
        Catch objException As Exception
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()

            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)

            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            currentApp.Response.Redirect("DBErrorPage.aspx")
            'Throw New ApplicationException(objException.ToString)
        End Try

    End Function

    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection
        Try
            connection = New OleDbConnection(DbUrl)
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

            dataAdapter.Fill(UserdataSet)

            Try
                dataAdapter.Dispose()
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            'connection.Close()
            Return UserdataSet
        Catch objException As Exception
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>  The database may be down for routine maintenance.")
            'currentApp.Response.Write(" Please try again in a few minutes...<br>")
            'currentApp.Response.Write("Click <a href='default.aspx'>HERE</a> to return to login page.")
            'currentApp.Response.End()
            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            currentApp.Response.Redirect("DBErrorPage.aspx?HOME=N")
            'Throw New ApplicationException(objException.ToString)
        End Try

    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String) As OleDbDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection
        Try
            connection = New OleDbConnection(DbUrl)
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            currentApp.Response.Redirect("DBErrorPage.aspx?HOME=N")
            'Throw New ApplicationException(objException.ToString)
        End Try

    End Function

    Public Shared Function GetScalar(ByVal p_strQuery As String) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim strReturn As String
        Dim connection As OleDbConnection
        Try
            connection = New OleDbConnection(DbUrl)
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
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
            'connection.Close()
            Return strReturn
        Catch objException As Exception
            If strReturn Is Nothing Then
                Return ""
            End If
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            currentApp.Response.Redirect("DBErrorPage.aspx?HOME=N")
            'Throw New ApplicationException(objException.ToString)
        End Try
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String) As Integer

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim rowsAffected As Integer

        Dim connection As OleDbConnection
        Try
            connection = New OleDbConnection(DbUrl)
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
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
            'connection.Close()
            Return rowsAffected
        Catch objException As Exception
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            currentApp.Response.Redirect("DBErrorPage.aspx?HOME=N")
            'Throw New ApplicationException(objException.ToString)
        End Try

    End Function


    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property

    Public Shared Sub sendemail(ByVal mailer As System.Web.Mail.MailMessage)

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Try
            'If currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
            '    SmtpMail.SmtpServer = "127.0.0.1"
            'End If
            System.Web.Mail.SmtpMail.Send(mailer)
        Catch ex As Exception
            currentApp.Response.Write(("The following exception occurred: " + ex.ToString()))
            sendErrorEmail(ex.ToString, "NO", currentApp.Request.ServerVariables("URL"), "sendemail")
            'check the InnerException
            While Not (ex.InnerException Is Nothing)
                currentApp.Response.Write("--------------------------------")
                currentApp.Response.Write(("The following InnerException reported: " + ex.InnerException.ToString()))
                ex = ex.InnerException
            End While
        End Try
    End Sub

    Public Shared Sub sendErrorEmail(ByVal strMessage As String, ByVal strMobile As String, ByVal strURL As String, ByVal strSQL As String)

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim Mailer = New System.Web.Mail.MailMessage
        Dim strccfirst As String = "bob.dougherty"
        Dim strccfirst2 As String = "3025591705"
        Dim strcclast As String = "sdi.com"
        Dim strcclast2 As String = "mmode.com"
        Mailer.From = "Insiteonline.isacs.com"
        Mailer.to = strccfirst & "@" & strcclast
        Mailer.subject = strURL
        If strMobile = "YES" And _
            Not Trim(currentApp.Request.ServerVariables("remote_addr")) = "" And _
            Not currentApp.Request.ServerVariables("remote_addr") = "65.220.75.12" And _
            Not currentApp.Request.ServerVariables("remote_addr") = "127.0.0.1" And _
            Mailer.cc = strccfirst2 & "@" & strcclast2 Then
        End If
        'Dim strSQLString As String

        Dim strbodydetl As String = ""
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >" & strMessage & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr") & _
                                "&nbsp;" & currentApp.Session("USERID") & "&nbsp;" & strSQL
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "</div>"

        Mailer.body = strbodydetl

        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
        'SmtpMail.Send(Mailer)
        sendemail(Mailer)

    End Sub



End Class
