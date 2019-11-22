
Imports System
Imports System.Linq
Imports System.Data
Imports System.Data.OleDb
Imports System.Web.UI.Page
Imports System.Xml
Imports System.Web
Imports System.Web.Mail
Imports System.Configuration

Public Class ORDBData

    Public Shared Function GetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, Optional ByVal bThrowBackError As Boolean = False) As DataSet


        'Gives us a reference to the current asp.net 
        'application executing the method.
        ' Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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
            'connection.close()
            'Dim errorMessage As String = objException.Message
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetAdapter. User supposed to see DBErrorPage.aspx")
            'Dim sMsg66 As String = LCase(objException.ToString)
            'If bGoToErrPage Then
            '    Call ProcessError(sMsg66)
            'ElseIf bThrowBackError Then
            '    Throw objException

            'Else
            Return Nothing
            'End If

        End Try

    End Function

    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        '   Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

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
            'connection.close()
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetAdapterSpc. User supposed to see DBErrorPage.aspx")
            'Dim sMsg66 As String = LCase(objException.ToString)
            'Call ProcessError(sMsg66)

        End Try

    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
            'Dim sMsg66 As String = LCase(objException.ToString)
            'Call ProcessError(sMsg66)
            'Dim sMsg66 As String = LCase(objException.ToString)
            'If bGoToErrPage Then
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
            '    Call ProcessError(sMsg66)
            'Else
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User will not see DBErrorPage.aspx")
            'End If
        End Try

    End Function

    Public Shared Sub ProcessError(ByVal sMsg66 As String)

        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        If Not currentApp.Session("USERID") Is Nothing Then
            If (sMsg66.Contains("table does not exist")) Or (sMsg66.Contains("table or view does not exist")) Then
                'SPECIAL PROCESSING
                currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                currentApp.Response.Write("<hr>")
                currentApp.Response.Write("<li> Please try again in a few moments...If the problem persist, please contact your SDI site contact or ")
                currentApp.Response.Write("<li> call SDI Information Technology at 215-633-1900 option 7 to report the problem.")
                currentApp.Response.Write("<li>  ")
                currentApp.Response.Write("<li>  We apologize for the inconvenience.")
                currentApp.Response.Write("<li>______________________________________________________________________________________________________")
                currentApp.Response.Write("<li>  ")
                currentApp.Response.End()
            Else
                Try
                    currentApp.Response.Redirect("//" & currentApp.Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/DBErrorPage.aspx?HOME=N")
                Catch ex As Exception
                    ' just catching: to avoid 'Thread being aborted' error message
                End Try
            End If
        Else
            'ERROR DURING LOGIN - SPECIAL PROCESSING
            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            currentApp.Response.Write("<hr>")
            currentApp.Response.Write("<li> Please try again in a few moments...If the problem persist, please contact your SDI site contact or ")
            currentApp.Response.Write("<li> call SDI Information Technology at 215-633-1900 option 7 to report the problem.")
            currentApp.Response.Write("<li>  ")
            currentApp.Response.Write("<li>  We apologize for the inconvenience.")
            currentApp.Response.Write("<li>______________________________________________________________________________________________________")
            currentApp.Response.Write("<li>  ")
            currentApp.Response.End()
        End If
    End Sub

    Public Shared Function GetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        '  Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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

                'connection.close()
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetScalar. User supposed to see DBErrorPage.aspx")
                'Dim sMsg66 As String = LCase(objException.ToString)
                'If bGoToErrPage Then
                '    Call ProcessError(sMsg66)
                'End If
            End Try

        End Try

        Return strReturn

    End Function

    Public Shared Function ExecNonQueryWithTransaction(ByVal p_strQuery As String) As Integer
        Dim trnsactSession As OleDbTransaction = Nothing
        Dim connection As OleDbConnection = New OleDbConnection(ORDBData.DbUrl)
        Dim rowsAffected As Integer = 0

        Try

            connection.Open()
            trnsactSession = connection.BeginTransaction
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.Transaction = trnsactSession
            Command.CommandTimeout = 120
            rowsAffected = Command.ExecuteNonQuery()
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try

            trnsactSession.Commit()
            connection.Close()
            trnsactSession = Nothing
            connection = Nothing
        Catch objException As Exception
            rowsAffected = 0
            Try
                trnsactSession.Rollback()
                connection.Close()
                trnsactSession = Nothing
                connection = Nothing
            Catch ex As Exception

            End Try
        End Try

        Return rowsAffected

    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As Integer

        'Gives us a reference to the current asp.net 
        'application executing the method.
        '   Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

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
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, ExecNonQuery. User supposed to see DBErrorPage.aspx")
            'Dim sMsg66 As String = LCase(objException.ToString)
            'If bGoToErrPage Then
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, ExecNonQuery. User supposed to see DBErrorPage.aspx")
            '    Call ProcessError(sMsg66)
            'Else
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, ExecNonQuery. User will not see DBErrorPage.aspx")
            'End If
        End Try

        Return rowsAffected
    End Function

    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property

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

    'Public Shared Function UnilogGetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As DataSet
    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
    '    Try

    '        Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
    '        Command.CommandTimeout = 120
    '        connection.Open()
    '        Dim dataAdapter As OleDbDataAdapter = _
    '                New OleDbDataAdapter(Command)

    '        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

    '        dataAdapter.Fill(UserdataSet)
    '        Try
    '            dataAdapter.Dispose()
    '        Catch ex As Exception

    '        End Try
    '        Try
    '            Command.Dispose()
    '        Catch ex As Exception

    '        End Try
    '        Try
    '            connection.Dispose()
    '            connection.Close()
    '        Catch ex As Exception

    '        End Try
    '        'connection.close()
    '        Return UserdataSet
    '    Catch objException As Exception
    '        Try
    '            connection.Dispose()
    '            connection.Close()
    '        Catch ex As Exception

    '        End Try
    '        'connection.close()
    '        sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetAdapter. User supposed to see DBErrorPage.aspx")
    '        Dim sMsg66 As String = LCase(objException.ToString)
    '        If bGoToErrPage Then
    '            Call ProcessError(sMsg66)
    '        Else
    '            Return Nothing
    '        End If

    '    End Try
    'End Function

    'Public Shared Function UnilogGetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    Dim strReturn As String = ""
    '    Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
    '    Try

    '        Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
    '        Command.CommandTimeout = 120
    '        connection.Open()
    '        Try
    '            strReturn = CType(Command.ExecuteScalar(), String)
    '        Catch ex32 As Exception
    '            strReturn = ""
    '        End Try
    '        If strReturn Is Nothing Then
    '            strReturn = ""
    '        End If
    '        Try
    '            Command.Dispose()
    '        Catch ex1 As Exception

    '        End Try
    '        Try
    '            connection.Close()
    '            connection.Dispose()
    '        Catch ex2 As Exception

    '        End Try
    '        'connection.close()
    '    Catch objException As Exception
    '        strReturn = ""

    '        Try
    '            connection.Close()
    '            connection.Dispose()
    '        Catch ex As Exception

    '            'connection.close()
    '            sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetScalar. User supposed to see DBErrorPage.aspx")
    '            Dim sMsg66 As String = LCase(objException.ToString)
    '            If bGoToErrPage Then
    '                Call ProcessError(sMsg66)
    '            End If
    '        End Try

    '    End Try

    '    Return strReturn
    'End Function

    'Public Shared Function UnilogGetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader
    '    'Gives us a reference to the current asp.net 
    '    'application executing the method.
    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
    '    Try

    '        Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
    '        Command.CommandTimeout = 120
    '        connection.Open()
    '        Dim datareader As OleDbDataReader
    '        datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
    '        Return datareader
    '    Catch objException As Exception
    '        Try
    '            connection.Close()
    '            connection.Dispose()
    '        Catch ex As Exception

    '        End Try
    '        'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
    '        'Dim sMsg66 As String = LCase(objException.ToString)
    '        'Call ProcessError(sMsg66)
    '        Dim sMsg66 As String = LCase(objException.ToString)
    '        If bGoToErrPage Then
    '            sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
    '            Call ProcessError(sMsg66)
    '        Else
    '            sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User will not see DBErrorPage.aspx")
    '        End If
    '    End Try

    'End Function

    'Public Shared ReadOnly Property UnilogDbUrl As String
    '    Get
    '        Return GetUnilogConnectString()  '  ConfigurationSettings.AppSettings("UnilogconString")
    '    End Get
    'End Property

    Public Shared Sub sendErrorEmail(ByVal strMessage As String, ByVal strMobile As String, ByVal strURL As String, ByVal strSQL As String)

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim Mailer As MailMessage = New MailMessage
        Dim strccfirst As String = System.Configuration.ConfigurationManager.AppSettings("MailToName") '  "michael.randall"
        Dim strccfirst2 As String = "3025591705"
        Dim strcclast As String = "sdi.com"
        Dim strcclast2 As String = "vtext.com"
        Mailer.From = "SDIExchADMIN@SDI.com"

        Dim strComeFrom As String = "Unknown"
        Try
            strComeFrom = System.Configuration.ConfigurationSettings.AppSettings("serverId").Trim
        Catch ex As Exception
            strComeFrom = "Unknown"
        End Try
        strSQL += ". Data Source = " & Right(DbUrl, 4) & vbCrLf & _
            "BU: " & currentApp.Session("BUSUNIT") & ". Server Id: " & strComeFrom
        Dim bSendToMeOnly As Boolean = False
        Dim strHasNothing As String = "Session('USERID')  Is Nothing" & vbCrLf & _
            "Session('SDIEMP')  Is Nothing."
        If strMessage.Contains(strHasNothing) Then
            bSendToMeOnly = True
        End If

        Dim strHasInvaldViewState As String = "Invalid viewstate."
        If strMessage.Contains(strHasInvaldViewState) Then
            bSendToMeOnly = True
        End If

        Dim strHasWebResource As String = "/WebResource.axd"
        If strURL.Contains(strHasWebResource) Then
            bSendToMeOnly = True
        End If

        Dim strCollectionWasModified As String = "Collection was modified; enumeration operation may not execute."
        If strMessage.Contains(strCollectionWasModified) Then
            bSendToMeOnly = True
        End If

        Dim strMailtoList1 As String = strccfirst & "@" & strcclast & ";" & System.Configuration.ConfigurationManager.AppSettings("MailToList")
        Try
            If LCase(strMessage).Contains("transaction or savepoint rollback required") Then
                ' add Brian Akom and Wenjia to a Mailer.to list
                strMailtoList1 = strMailtoList1 & ";Brian.Akom@sdi.com;Wenjia.Zhang@sdi.com"
            End If
        Catch ex As Exception

        End Try

        If bSendToMeOnly Then
            Mailer.To = "webdev@sdi.com"
        Else
            Mailer.To = strMailtoList1
        End If

        Mailer.Subject = strURL  '   & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr")
        If strMobile = "YES" And _
            Not Trim(currentApp.Request.ServerVariables("remote_addr")) = "" And _
            Not currentApp.Request.ServerVariables("remote_addr") = "65.220.75.12" And _
            Not currentApp.Request.ServerVariables("remote_addr") = "127.0.0.1" And _
            Mailer.Cc = strccfirst2 & "@" & strcclast2 Then
        End If
        'Dim strSQLString As String

        Dim strbodydetl As String
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >" & strMessage & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr") & _
                                "&nbsp;" & currentApp.Session("USERID") & "&nbsp;" & strSQL
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "</div>"

        Mailer.Body = strbodydetl

        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()

        Try
            SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From.ToString(), Mailer.To.ToString(), Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception
            Try
                SDIEmailService.EmailUtilityServices("Mail", Mailer.From.ToString(), Mailer.To.ToString(), Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())

                Dim exceptionString As String
                Dim serverName As String = String.Empty
                If Not currentApp.Server.MachineName Is Nothing Then
                    serverName = currentApp.Server.MachineName
                Else
                    serverName = strComeFrom
                End If
                exceptionString = "Exception Message - " + ex.Message + "<br />" + "Exception Trace - " + ex.StackTrace + "<br />"
                SDIEmailService.EmailUtilityServices("Mail", Mailer.From.ToString(), Mailer.To.ToString(), "Error in Email Utility - " + serverName, String.Empty, String.Empty, exceptionString, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())
            Catch exs1 As Exception
            End Try
        End Try

    End Sub

    Public Shared Function GetWebAppName1() As String
        Dim sReturn As String = ""
        Dim sWebAppName As String = ""
        Try
            sWebAppName = Convert.ToString(ConfigurationSettings.AppSettings("WebAppName"))
            If sWebAppName Is Nothing Then
                sWebAppName = ""
            End If
        Catch ex As Exception
            sWebAppName = ""
        End Try
        If sWebAppName = "" Then
            sReturn = ""
        Else
            sReturn = "/" & sWebAppName
        End If
        Return sReturn
    End Function
End Class
