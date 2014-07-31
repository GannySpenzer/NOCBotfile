Module TruckLoadingAutoDispatch

    Private m_logger As SDI.ApplicationLogger.appLogger = Nothing
    Private m_emailer As SDI.EmailNotifier.smtpNotifier = Nothing

    Private m_name As String = ""
    Private m_version As String = ""
    Private m_tempPath As String = ""

    Private m_logPath As String = ""
    Private m_logFile As String = ""
    Private m_logLevel As System.Diagnostics.TraceLevel = TraceLevel.Off

    Sub Main()

        InitMembers()

        m_logger.WriteInformationLog("starting " & m_name & " v" & m_version)

        Dim arrAutoDispatchSites As New ArrayList

        '
        ' get lists of B/U for auto-dispatching of loaded containers
        '
        Dim s As String = ""
        Dim s2 As String() = New String() {}

        Try
            s = CStr(My.Settings("AutoDispatchSite")).Trim.ToUpper
        Catch ex As Exception
        End Try
        If (s Is Nothing) Then
            s = ""
        End If
        If (s.Length > 0) Then
            s2 = s.Split(","c)
        End If

        If (s2.Length > 0) Then
            For Each s In s2
                If (s.Trim.Length > 0) Then
                    arrAutoDispatchSites.Add(s)
                End If
            Next
        End If

        '
        ' process all "loaded" containers and dispatch for auto-dispatch sites
        '

        If (arrAutoDispatchSites.Count > 0) Then

            Dim oraCN As OleDb.OleDbConnection = Nothing
            oraCN = New OleDb.OleDbConnection(My.Settings("oraCNString"))

            m_logger.WriteVerboseLog("using database : " & oraCN.DataSource)

            oraCN.Open()

            Dim cmd As OleDb.OleDbCommand = Nothing
            Dim sb As System.Text.StringBuilder = Nothing
            Dim sr As System.IO.StreamReader = Nothing
            Dim i As Integer = 0

            sr = New System.IO.StreamReader(CStr(My.Settings("SQL_AutoDispatchBU")))
            s = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            For Each sSiteId As String In arrAutoDispatchSites

                m_logger.WriteVerboseLog("auto-dispatching loaded containers for site : " & sSiteId)

                sb = New System.Text.StringBuilder
                sb.AppendFormat(s, sSiteId)
                s = sb.ToString
                sb = Nothing

                cmd = oraCN.CreateCommand
                cmd.CommandText = s
                cmd.CommandType = CommandType.Text

                Try
                    i = cmd.ExecuteNonQuery
                    m_logger.WriteVerboseLog("updated " & i.ToString("####,###,##0") & " record(s).")
                Catch ex As Exception
                    m_logger.WriteErrorLog("ERROR : " & ex.ToString)
                End Try

                Try
                    cmd.Dispose()
                Catch ex As Exception
                End Try
                cmd = Nothing

            Next

            Try
                oraCN.Close()
            Catch ex As Exception
            End Try
            Try
                oraCN.Dispose()
            Catch ex As Exception
            End Try
            oraCN = Nothing

        End If

        arrAutoDispatchSites = Nothing

        m_logger.WriteInformationLog("finished executing " & m_name)

        m_logger = Nothing
        m_emailer = Nothing

    End Sub

    Private Sub InitMembers()

        ' assembly name
        m_name = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName

        ' assembly version
        m_version = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

        Dim s As String = ""

        ' log level
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="appLogLevel")).Trim.ToUpper
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            If (s.IndexOf("VERB") > -1) Then
                m_logLevel = TraceLevel.Verbose
            ElseIf (s.IndexOf("INFO") > -1) Or (s.IndexOf("INFORMATION") > -1) Then
                m_logLevel = TraceLevel.Info
            ElseIf (s.IndexOf("WARNING") > -1) Or (s.IndexOf("WARN") > -1) Then
                m_logLevel = TraceLevel.Warning
            ElseIf (s.IndexOf("ERROR") > -1) Then
                m_logLevel = TraceLevel.Error
            ElseIf (s.IndexOf("OFF") > -1) Then
                m_logLevel = TraceLevel.Off
            Else
                ' don't change default
            End If
        End If

        ' log path
        '   default to location of executing assembly on \Logs folder
        m_logPath = m_name.Substring(0, m_name.LastIndexOf("\"c)) & "\Logs"
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="appLogPath")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_logPath = s
        End If
        While (m_logPath.Length > 0) And (m_logPath.Trim.LastIndexOf("\"c) = (m_logPath.Trim.Length - 1))
            m_logPath = m_logPath.Trim.TrimEnd("\"c)
        End While
        If Not System.IO.Directory.Exists(m_logPath) Then
            System.IO.Directory.CreateDirectory(m_logPath)
        End If

        ' log filename ID
        Dim sLogFilenameId As String = "X"
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="appLogFilenameId")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            sLogFilenameId = s
        End If

        ' log file
        m_logFile = m_logPath & "\" & sLogFilenameId & "_" & Now.ToString("yyyyMMdd") & Now.GetHashCode.ToString & ".log"

        ' logger
        m_logger = New SDI.ApplicationLogger.appLogger(m_logFile, m_logLevel)

        ' emailer
        m_emailer = New SDI.EmailNotifier.smtpNotifier()

        '   (1) TO
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="onError_NotifyTO")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_emailer.TO = s
        End If

        '   (2) FROM
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="onError_NotifyFROM")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_emailer.FROM = s
        End If

        '   (3) CC
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="onError_NotifyCC")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_emailer.CC = s
        End If

        '   (4) BCC
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="onError_NotifyBCC")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_emailer.BCC = s
        End If

        '   (5) SUBJECT
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="onError_NotifySUBJECT")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_emailer.SUBJECT = s
        End If

        ' application temporary file path
        '   default to location of executing assembly on \Logs folder
        m_tempPath = m_name.Substring(0, m_name.LastIndexOf("\"c)) & "\Logs"
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="appTempPath")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_tempPath = s
        End If
        While (m_tempPath.Length > 0) And (m_tempPath.Trim.LastIndexOf("\"c) = (m_tempPath.Trim.Length - 1))
            m_tempPath = m_tempPath.Trim.TrimEnd("\"c)
        End While
        If Not System.IO.Directory.Exists(m_tempPath) Then
            System.IO.Directory.CreateDirectory(m_tempPath)
        End If

    End Sub

End Module
