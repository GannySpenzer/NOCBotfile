Imports System
Imports System.IO
Imports System.Diagnostics
Imports System.Data.SqlClient


Public Class appLoggerDB

    Implements IDisposable
    Implements IApplicationLogger

    Private Const sqlCN_default_provider As String = ""
    Private Const sqlCN_default_creden As String = "User ID=i.print;Password=webp0501"
    Private Const sqlCN_default_DB As String = "Data Source=DEXPROD4;Initial Catalog=webLog;"

    Private m_sqlCNstring As String = "" & _
                                      sqlCN_default_provider & _
                                      sqlCN_default_creden & _
                                      sqlCN_default_DB & _
                                      ""
    Private m_cn As SqlConnection = Nothing
    Private m_logLevel As TraceLevel = TraceLevel.Error
    Private m_logCategory As String = "--"

    Private m_user As String = ""
    Private m_userBU As String = ""
    Private m_userName As String = ""
    Private m_userType As String = ""
    Private m_isaLogPage As String = ""
    Private m_isaLogURL As String = ""
    Private m_isaLogIP As String = ""
    Private m_isaLogBrowser As String = ""
    Private m_isaLogCookies As String = ""
    Private m_isaLogJavaScript As String = ""
    Private m_isaLogJavaApplet As String = ""
    Private m_isaLogVBScript As String = ""
    Private m_isaLogActiveX As String = ""
    Private m_isaLogPlatform As String = ""


    Public Sub New()
        m_cn = New SqlConnection(connectionString:=m_sqlCNstring)
    End Sub

    Public Sub New(ByVal logLevel As System.Diagnostics.TraceLevel)
        m_logLevel = logLevel
        m_cn = New SqlConnection(connectionString:=m_sqlCNstring)
    End Sub

    Public Sub New(ByVal logLevel As System.Diagnostics.TraceLevel, _
                   ByVal sqlCNstring As String)
        m_logLevel = logLevel
        m_sqlCNstring = sqlCNstring
        m_cn = New SqlConnection(connectionString:=m_sqlCNstring)
    End Sub

    Public Sub New(ByVal logLevel As System.Diagnostics.TraceLevel, _
                   ByVal logCategory As String, _
                   ByVal sqlCNstring As String)
        m_logLevel = logLevel
        m_sqlCNstring = sqlCNstring
        m_cn = New SqlConnection(connectionString:=m_sqlCNstring)
    End Sub

    ' NOTE:
    ' (1) provide connection string to use (intended for SQL server use)
    ' OR (2) provide connection object (SQL server)

    Public Property sqlCNstring() As String
        Get
            Return m_sqlCNstring
        End Get
        Set(ByVal Value As String)
            m_sqlCNstring = Value
            m_cn = New SqlConnection(connectionString:=m_sqlCNstring)
        End Set
    End Property

    Public Property LogCategory() As String
        Get
            Return m_logCategory
        End Get
        Set(ByVal Value As String)
            m_logCategory = Value
        End Set
    End Property

    Public Property UserId() As String
        Get
            Return m_user
        End Get
        Set(ByVal Value As String)
            m_user = Value
        End Set
    End Property

    Public Property UserBU() As String
        Get
            Return m_userBU
        End Get
        Set(ByVal Value As String)
            m_userBU = Value
        End Set
    End Property

#Region " IDisposable Implementations "

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            If Not (m_cn Is Nothing) Then
                If Not (m_cn.State = ConnectionState.Closed) Then
                    m_cn.Close()
                End If
                m_cn.Dispose()
            End If
        Catch ex As Exception
        Finally
            m_cn = Nothing
        End Try
    End Sub

#End Region

#Region " IApplicationLogger Implementations "

    Public Sub WriteLog(ByVal msg As String, ByVal logAs As System.Diagnostics.TraceLevel) Implements IApplicationLogger.WriteLog
        AppendLog(msg, logAs)
    End Sub

    Public Sub WriteVerboseLog(ByVal msg As String) Implements IApplicationLogger.WriteVerboseLog
        AppendLog(msg, System.Diagnostics.TraceLevel.Verbose)
    End Sub

    Public Sub WriteInformationLog(ByVal msg As String) Implements IApplicationLogger.WriteInformationLog
        AppendLog(msg, System.Diagnostics.TraceLevel.Info)
    End Sub

    Public Sub WriteWarningLog(ByVal msg As String) Implements IApplicationLogger.WriteWarningLog
        AppendLog(msg, System.Diagnostics.TraceLevel.Warning)
    End Sub

    Public Sub WriteErrorLog(ByVal msg As String) Implements IApplicationLogger.WriteErrorLog
        AppendLog(msg, System.Diagnostics.TraceLevel.Error)
    End Sub

    Public ReadOnly Property LoggingLevel() As System.Diagnostics.TraceLevel Implements IApplicationLogger.LoggingLevel
        Get
            Return m_logLevel
        End Get
    End Property

#End Region

    Private Sub AppendLog(ByVal msg As String, ByVal msgLevel As System.Diagnostics.TraceLevel)
        Dim rtn As String = "appLoggerDB.AppendLog"
        Dim bIsWriteLog As Boolean = True
        If Not (msgLevel = TraceLevel.Off) Then
            Select Case m_logLevel
                Case TraceLevel.Verbose
                    ' write everything ... except "off" of course
                    bIsWriteLog = True
                Case TraceLevel.Info
                    ' write anything that is NOT VERBOSE
                    bIsWriteLog = Not (msgLevel = TraceLevel.Verbose)
                Case TraceLevel.Warning
                    ' write warning and error logs
                    bIsWriteLog = (msgLevel = TraceLevel.Warning Or msgLevel = TraceLevel.Error)
                Case TraceLevel.Error
                    ' write error logs only
                    bIsWriteLog = (msgLevel = TraceLevel.Error)
                Case Else
                    ' do nothing
                    '   logging is OFF
                    bIsWriteLog = False
            End Select
        Else
            ' well, do nothing??? why would somebody in their right mind wants to log something
            '   and gives a traceLevel=OFF!?!?!!
            bIsWriteLog = False
        End If
        If bIsWriteLog Then

            Dim sql As New SQLBuilder(sql:="" & _
                                           "INSERT INTO [m_log] " & vbCrLf & _
                                           "(" & vbCrLf & _
                                           " [k_loglevel] ,[k_logcategory] ,[s_server] ,[s_user] ,[s_userBU] " & vbCrLf & _
                                           ",[s_userName] ,[s_userType] ,[s_isaLogPage] ,[s_isaLogURL] ,[s_isaLogIP] " & vbCrLf & _
                                           ",[s_isaLogBrowser] ,[s_isaLogCookies] ,[s_isaLogJavaScript] ,[s_isaLogJavaApplet] " & vbCrLf & _
                                           ",[s_isaLogVBScript] ,[s_isaLogActiveX] ,[s_isaLogPlatform] ,[t_message] " & vbCrLf & _
                                           ") " & vbCrLf & _
                                           "VALUES " & vbCrLf & _
                                           "(" & vbCrLf & _
                                           " ':k_loglevel' ,':k_logcategory' ,':s_server' ,':s_user' ,':s_userBU' " & vbCrLf & _
                                           ",':s_userName' ,':s_userType' ,':s_isaLogPage' ,':s_isaLogURL' ,':s_isaLogIP' " & vbCrLf & _
                                           ",':s_isaLogBrowser' ,':s_isaLogCookies' ,':s_isaLogJavaScript' ,':s_isaLogJavaApplet' " & vbCrLf & _
                                           ",':s_isaLogVBScript' ,':s_isaLogActiveX' ,':s_isaLogPlatform' ,':t_message' " & vbCrLf & _
                                           ")" & vbCrLf & _
                                           "")

            sql.Parameters.Add(":k_loglevel", msgLevel.ToString)
            sql.Parameters.Add(":k_logcategory", CStr(IIf(m_logCategory.Trim.Length > 0, m_logCategory.Trim, " ")))
            sql.Parameters.Add(":s_server", CStr(IIf(True, "true", "false")))
            sql.Parameters.Add(":s_user", "")
            sql.Parameters.Add(":s_userBU", "")
            sql.Parameters.Add(":s_userName", "")
            sql.Parameters.Add(":s_userType", "")
            sql.Parameters.Add(":s_isaLogPage", "")
            sql.Parameters.Add(":s_isaLogURL", "")
            sql.Parameters.Add(":s_isaLogIP", "")
            sql.Parameters.Add(":s_isaLogBrowser", "")
            sql.Parameters.Add(":s_isaLogCookies", "")
            sql.Parameters.Add(":s_isaLogJavaScript", "")
            sql.Parameters.Add(":s_isaLogJavaApplet", "")
            sql.Parameters.Add(":s_isaLogVBScript", "")
            sql.Parameters.Add(":s_isaLogActiveX", "")
            sql.Parameters.Add(":s_isaLogPlatform", "")
            sql.Parameters.Add(":t_message", CStr(IIf(msg.Trim.Length > 0, msg.Trim, "")))

            m_cn.Open()

            Dim cmd As SqlCommand = m_cn.CreateCommand

            cmd.CommandText = sql.ToString
            cmd.CommandType = CommandType.Text
            'cmd.CommandTimeout = 10 - use default of 30 secs

            sql = Nothing

            Dim ret As Integer = cmd.ExecuteNonQuery

            m_cn.Close()

        End If
    End Sub

End Class
