Imports System
Imports System.Linq
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml
Imports System.Configuration

Public Class ORDBData

    Public Shared Function GetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, Optional ByVal bThrowBackError As Boolean = False) As DataSet


        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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
            Dim errorMessage As String = objException.Message
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetAdapter. User supposed to see DBErrorPage.aspx")
            Dim sMsg66 As String = LCase(objException.ToString)
            If bGoToErrPage Then
                'Call ProcessError(sMsg66)
            ElseIf bThrowBackError Then
                Throw objException

            Else
                Return Nothing
            End If

        End Try

    End Function

    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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
            Dim sMsg66 As String = LCase(objException.ToString)
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
            Dim sMsg66 As String = LCase(objException.ToString)
            If bGoToErrPage Then
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
                'Call ProcessError(sMsg66)
            Else
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User will not see DBErrorPage.aspx")
            End If
        End Try

    End Function

    'Public Shared Sub ProcessError(ByVal sMsg66 As String)

    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    If Not currentApp.Session("USERID") Is Nothing Then
    '        If (sMsg66.Contains("table does not exist")) Or (sMsg66.Contains("table or view does not exist")) Then
    '            'SPECIAL PROCESSING
    '            currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
    '            currentApp.Response.Write("<hr>")
    '            currentApp.Response.Write("<li> Please try again in a few moments...If the problem persist, please contact your SDI site contact or ")
    '            currentApp.Response.Write("<li> call SDI Information Technology at 215-633-1900 option 7 to report the problem.")
    '            currentApp.Response.Write("<li>  ")
    '            currentApp.Response.Write("<li>  We apologize for the inconvenience.")
    '            currentApp.Response.Write("<li>______________________________________________________________________________________________________")
    '            currentApp.Response.Write("<li>  ")
    '            currentApp.Response.End()
    '        Else
    '            Try
    '                currentApp.Response.Redirect("//" & currentApp.Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/DBErrorPage.aspx?HOME=N")
    '            Catch ex As Exception
    '                ' just catching: to avoid 'Thread being aborted' error message
    '            End Try
    '        End If
    '    Else
    '        'ERROR DURING LOGIN - SPECIAL PROCESSING
    '        currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
    '        currentApp.Response.Write("<hr>")
    '        currentApp.Response.Write("<li> Please try again in a few moments...If the problem persist, please contact your SDI site contact or ")
    '        currentApp.Response.Write("<li> call SDI Information Technology at 215-633-1900 option 7 to report the problem.")
    '        currentApp.Response.Write("<li>  ")
    '        currentApp.Response.Write("<li>  We apologize for the inconvenience.")
    '        currentApp.Response.Write("<li>______________________________________________________________________________________________________")
    '        currentApp.Response.Write("<li>  ")
    '        currentApp.Response.End()
    '    End If
    'End Sub

    Public Shared Function GetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
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
                Dim sMsg66 As String = LCase(objException.ToString)
                If bGoToErrPage Then
                    'Call ProcessError(sMsg66)
                End If
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

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, _
                Optional bSendEmail As Boolean = True) As Integer

        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

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
            Dim sMsg66 As String = LCase(objException.ToString)
            If bGoToErrPage Then
                If bSendEmail Then
                    'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, ExecNonQuery. User supposed to see DBErrorPage.aspx")
                End If
                'Call ProcessError(sMsg66)
                'Else
                '    If bSendEmail Then
                '        'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, ExecNonQuery. User will not see DBErrorPage.aspx")
                '    End If
            End If
        End Try

        Return rowsAffected
    End Function


    Public Shared ReadOnly Property DbUrl() As String
        Get
            'Return ConfigurationSettings.AppSettings("OLEDBconString")
            'Return System.Configuration.AppSettingsReader("OLEDBconString")
            Return System.Configuration.ConfigurationSettings.AppSettings("OLEDBconString")

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

    Public Shared Function UnilogGetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As DataSet
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
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
            'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetAdapter. User supposed to see DBErrorPage.aspx")
            Dim sMsg66 As String = LCase(objException.ToString)
            If bGoToErrPage Then
                'Call ProcessError(sMsg66)
            Else
                Return Nothing
            End If

        End Try
    End Function

    Public Shared Function UnilogGetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim strReturn As String = ""
        Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
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
                Dim sMsg66 As String = LCase(objException.ToString)
                If bGoToErrPage Then
                    'Call ProcessError(sMsg66)
                End If
            End Try

        End Try

        Return strReturn
    End Function

    Public Shared Function UnilogGetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection As OleDbConnection = New OleDbConnection(UnilogDbUrl)
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
            Dim sMsg66 As String = LCase(objException.ToString)
            If bGoToErrPage Then
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
                'Call ProcessError(sMsg66)
            Else
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User will not see DBErrorPage.aspx")
            End If
        End Try

    End Function

    Public Shared ReadOnly Property UnilogDbUrl As String
        Get
            Return GetUnilogConnectString()  '  ConfigurationSettings.AppSettings("UnilogconString")
        End Get
    End Property

    Public Shared Function GetUnilogConnectString() As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim strConnString As String = ""
        Dim strXMLDir As String = ""
        Try
            strXMLDir = CType(ConfigurationSettings.AppSettings("AppConfigPath"), String)
            If Not strXMLDir Is Nothing Then
                strXMLDir = Trim(strXMLDir)
            Else
                strXMLDir = "C:\inetpub\wwwroot\SdiExchangeConfig"
            End If
        Catch ex As Exception
            strXMLDir = "C:\inetpub\wwwroot\SdiExchangeConfig"
        End Try
        If Trim(strXMLDir) <> "" Then
            strXMLDir = strXMLDir & "\configSetting.xml"
        Else
            strXMLDir = "C:\inetpub\wwwroot\SdiExchangeConfig\configSetting.xml"
        End If

        Dim m_xmlConfig As System.Xml.XmlDocument = New System.Xml.XmlDocument
        m_xmlConfig.Load(strXMLDir)

        Try
            ' retrieve the source DB connection string to use
            If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                strConnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
            Else
                strConnString = "Provider=OraOLEDB.Oracle.1;Password=Sd1UniProd;User Id=sdiprod;Data Source=unip1.sdi.com"
            End If
        Catch ex As Exception
            strConnString = "Provider=OraOLEDB.Oracle.1;Password=Sd1UniProd;User Id=sdiprod;Data Source=unip1.sdi.com"
        End Try

        Return strConnString
    End Function
End Class
