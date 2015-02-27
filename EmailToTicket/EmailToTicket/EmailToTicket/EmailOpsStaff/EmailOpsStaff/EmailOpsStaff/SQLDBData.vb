Imports System.Data.SqlClient
Imports System.Web.UI.Page
Imports System.Web

Imports EmailOpsStaff.WebPartnerFunctions.WebPSharedFunc

Public Class SQLDBData

    Public Shared Function GetSQLAdapter(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Or _
        '    currentApp.Session("CPVersion") Is Nothing Then
        If currentApp.Session("UtilCatalog") = "UtilCatalog" Then
            connString = DbSQLUrl
        Else
            connString = DbSQLUrl1
        End If


        'End If
        Dim connection = New SqlConnection(connString)
        'Dim connection = New SqlConnection(DbSQLUrl)
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
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try

    End Function

    Public Shared Function GetProdSQLAdapter(ByVal p_strQuery As String) As DataSet

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim connection = New SqlConnection(ProdSQLUrl)
        'Dim connection = New SqlConnection(DbSQLRepUrl)

        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Then
        connString = DbSQLRepUrl
        'Else
        'connString = DbSQLRepUrl
        'End If
        Dim connection = New SqlConnection(connString)

        Try

            'if we change the session variable to point to a particular catalogue for testing - remember to change it back
            ' connection.connectionstring = getRepDBConnectString(connection.connectionstring, currentApp.Session("CPLUSDB"))
            'connection.connectionstring = getRepDBConnectString(connection.connectionstring, "755784_1")
            '755784_1
            Dim Command = New SqlCommand(p_strQuery, connection)
            Command.commandtimeout = 120
            connection.open()
            Dim dataAdapter As SqlDataAdapter = _
                    New SqlDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet

            dataAdapter.Fill(UserdataSet)
            connection.close()
            Return UserdataSet
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            '  sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx?msg=" & objException.Message)

        End Try

    End Function

    Public Shared Function GetSQLReader(ByVal p_strQuery As String) As SqlDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim connection = New SqlConnection(DbSQLUrl)
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Or _
        'currentApp.Session("CPVersion") Is Nothing Then
        connString = DbSQLUrl1
        'Else
        'connString = DbSQLUrl
        'End If
        Dim connection = New SqlConnection(connString)

        Try
            
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As SqlDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            '  sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try

    End Function

    Public Shared Function GetProdSQLReader(ByVal p_strQuery As String) As SqlDataReader
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim connection = New SqlConnection(DbSQLRepUrl)
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Or _
        'currentApp.Session("CPVersion") Is Nothing Then
        connString = DbSQLRepUrl1
        'Else
        'connString = DbSQLRepUrl
        'End If
        Dim connection = New SqlConnection(connString)

        Try
            
            '   connection.connectionstring = getRepDBConnectString(connection.connectionstring, currentApp.Session("CPLUSDB"))
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As SqlDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try

    End Function

    Public Shared Function GetSQLScalar(ByVal p_strQuery As String) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim connection = New SqlConnection(DbSQLUrl)
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Or _
        'currentApp.Session("CPVersion") Is Nothing Then
        connString = DbSQLUrl1
        'Else
        'connString = DbSQLUrl
        'End If
        Dim connection = New SqlConnection(connString)
        Try
            
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim strReturn As String
            strReturn = Command.ExecuteScalar()
            connection.close()
            Return strReturn
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try
    End Function

    
    Public Shared Function GetProdSQLScalar(ByVal p_strQuery As String) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim connection = New SqlConnection(DbSQLRepUrl)
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Then
        connString = DbSQLRepUrl
        'Else
        'connString = DbSQLRepUrl
        'End If
        Dim connection = New SqlConnection(connString)
        Try
            
            'connection.connectionstring = getRepDBConnectString(connection.connectionstring, currentApp.Session("CPLUSDB"))
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim strReturn As String
            strReturn = Command.ExecuteScalar()
            connection.close()
            Return strReturn
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            ' sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try
    End Function

    Public Shared Function GetProdSQLScalarSPC(ByVal p_strQuery As String) As String
        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim connection = New SqlConnection(DbSQLRepUrl1)
        Try

            '   connection.connectionstring = getRepDBConnectString(connection.connectionstring, currentApp.Session("CPLUSDB"))
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim strReturn As String
            strReturn = Command.ExecuteScalar()
            connection.close()
            Return strReturn
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try
    End Function

    Public Shared Function ExecNonQuerySQL(ByVal p_strQuery As String) As Integer

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim rowsAffected As Integer
        'Dim connection = New SqlConnection(DbSQLUrl)
        Dim connString As String
        'If currentApp.Session("CPVersion") = "Ver1.3" Then
        connString = DbSQLUrl1
        'Else
        'connString = DbSQLUrl
        'End If
        Dim connection = New SqlConnection(connString)

        Try
            
            Dim Command = New SqlCommand(p_strQuery, connection)
            connection.open()
            rowsAffected = Command.ExecuteNonQuery()
            connection.close()
            Return rowsAffected
        Catch objException As SqlException
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
            connection.close()
            '  sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), p_strQuery)
            currentApp.Response.Redirect("DBErrorPage.aspx")

        End Try

    End Function

    'Public Shared Function getRepDBConnectString(ByVal strConnectString, ByVal strCatalogID) As String

    '    'Gives us a reference to the current asp.net 
    '    'application executing the method.
    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    Dim DBfile As String
    '    Dim strdb As String
    '    ' temporary code until version 1.3 is in production
    '    ' the productio sql server database is control by this function and the 
    '    ' web.config file
    '    'If currentApp.Session("CPLUSSERVER") = "http://contentplus.isacs.com:8080/" Then
    '    'If currentApp.Session("CPVersion") = "Ver1.2" Then

    '    '    If currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
    '    '        DBfile = "C:\BOBD\cplus\db.properties"
    '    '        strdb = Insiteonline.WebPartnerFunctions.WebPSharedFunc.getCPlusProddb(DBfile, currentApp.Session("CplusDB"))
    '    '        getRepDBConnectString = "server=contentplus;uid=einternet;pwd=einternet;initial catalog='" & strdb & "'"
    '    '    ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper.Substring(0, 6) = "CPTEST" Then
    '    '        DBfile = "e:\Apache Tomcat 4.0\webapps\ContentPlus\WEB-INF\classes\com\eplus\contentplus\db.properties"
    '    '        strdb = Insiteonline.WebPartnerFunctions.WebPSharedFunc.getCPlusProddb(DBfile, currentApp.Session("CplusDB"))
    '    '        'ProdSQLUrl = "server=cptest;uid=einternet;pwd=einternet;initial catalog='" & strDB & "'"
    '    '        getRepDBConnectString = "server=contentplus;uid=einternet;pwd=einternet;initial catalog='" & strdb & "'"
    '    '    ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper.Substring(0, 8) = "DEXTEST4" Then
    '    '        DBfile = currentApp.Server.MapPath("") & "\db.properties"
    '    '        strdb = Insiteonline.WebPartnerFunctions.WebPSharedFunc.getCPlusProddb(DBfile, currentApp.Session("CplusDB"))
    '    '        'ProdSQLUrl = "server=cptest;uid=einternet;pwd=einternet;initial catalog='" & strDB & "'"
    '    '        getRepDBConnectString = "server=contentplus;uid=einternet;pwd=einternet;initial catalog='" & strdb & "'"
    '    '    Else
    '    '        DBfile = "c:\Program Files\Apache Tomcat 4.0\webapps\ContentPlus\WEB-INF\classes\com\eplus\contentplus\db.properties"
    '    '        strdb = Insiteonline.WebPartnerFunctions.WebPSharedFunc.getCPlusProddb(DBfile, currentApp.Session("CplusDB"))
    '    '        getRepDBConnectString = "server=contentplus;uid=einternet;pwd=einternet;initial catalog='" & strdb & "'"
    '    '    End If
    '    'Else
    '        DBfile = currentApp.Server.MapPath("") & "\DBOverRide\dbOverRide.txt"
    '    strdb = InsiteonlineReplaceMagic.WebPartnerFunctions.WebPSharedFunc.getCPlusProddbOverride(DBfile, currentApp.Session("CplusDB"))

    '        If Trim(strdb) = "" Then
    '            'If Trim(strCatalogID) = "" Then
    '            '    strCatalogID = "755784"
    '            'End If
    '            getRepDBConnectString = strConnectString & strCatalogID
    '        Else
    '            getRepDBConnectString = strConnectString & strdb
    '        End If

    '    'End If
    'End Function

    Public Shared ReadOnly Property DbSQLUrl() As String

        Get
          '  Return ConfigurationSettings.AppSettings("SQLDBconString")
        End Get
    End Property

    Public Shared ReadOnly Property DbSQLUrl1() As String

        Get
            ' Return ConfigurationSettings.AppSettings("SQLDBconString1")
        End Get
    End Property

    Public Shared ReadOnly Property DbSQLRepUrl() As String

        Get
            'Return ConfigurationSettings.AppSettings("SQLDBReplicateConString")
        End Get
    End Property

    Public Shared ReadOnly Property DbSQLRepUrl1() As String

        Get
            '  Return ConfigurationSettings.AppSettings("SQLDBReplicateConString1")
        End Get
    End Property

End Class
