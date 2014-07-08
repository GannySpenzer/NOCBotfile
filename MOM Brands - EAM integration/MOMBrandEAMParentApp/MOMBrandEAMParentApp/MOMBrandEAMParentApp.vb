Public Class MOMBrandEAMParentApp

    Implements SDI.HTTP_SOAP_INTFC.IParentApp

    Private m_logger As SDI.ApplicationLogger.appLogger = Nothing
    Private m_emailer As SDI.EmailNotifier.smtpNotifier = Nothing
    Private m_name As String = ""
    Private m_version As String = ""
    Private m_customerId As String = ""
    Private m_targetURL As String = ""
    Private m_oraCNString1 As String = ""
    Private m_tempPath As String = ""
    Private m_userId As String = ""
    Private m_password As String = ""
    Private m_attribs As Hashtable = Nothing

    Private m_logPath As String = ""
    Private m_logFile As String = ""
    Private m_logLevel As System.Diagnostics.TraceLevel = TraceLevel.Off

    Public Sub New()
        MyBase.New()
        InitMembers()
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
        Dim sLogFilenameId As String = "EAMProcess"
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

        ' don't create an instance here ... wait until child request instance of the logger object
        '   - erwin
        '' logger
        'm_logger = New SDI.ApplicationLogger.appLogger(sLogFile, logLevel)

        ' URL
        m_targetURL = ""
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="target_URL")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_targetURL = s
        End If

        ' customer identifier
        m_customerId = ""
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="customerIdentifier")).Trim.ToUpper
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_customerId = s
        End If

        ' oracle connection string #1
        m_oraCNString1 = ""
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="oraCnString1")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_oraCNString1 = s
        End If

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

        ' user id/password
        m_userId = ""
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="username")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_userId = s
        End If

        m_password = ""
        s = ""
        Try
            s = CStr(My.Settings(propertyName:="password")).Trim
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            m_password = s
        End If

    End Sub

#Region " SDI.HTTP_SOAP_INTFC.IParentApp implementation "

    Public ReadOnly Property EmailNotifier As EmailNotifier.IEmailNotifier Implements HTTP_SOAP_INTFC.IParentApp.EmailNotifier
        Get
            If (m_emailer Is Nothing) Then
                m_emailer = New SDI.EmailNotifier.smtpNotifier
            End If
            Return m_emailer
        End Get
    End Property

    Public ReadOnly Property Logger As ApplicationLogger.IApplicationLogger Implements HTTP_SOAP_INTFC.IParentApp.Logger
        Get
            If (m_logger Is Nothing) Then
                m_logger = New SDI.ApplicationLogger.appLogger(m_logFile, m_logLevel)
            End If
            Return m_logger
        End Get
    End Property

    Public ReadOnly Property TargetURL As String Implements HTTP_SOAP_INTFC.IParentApp.TargetURL
        Get
            Return m_targetURL
        End Get
    End Property

    Public ReadOnly Property Version As String Implements HTTP_SOAP_INTFC.IParentApp.Version
        Get
            Return m_version
        End Get
    End Property

    Public ReadOnly Property [Name] As String Implements HTTP_SOAP_INTFC.IParentApp.Name
        Get
            Return m_name
        End Get
    End Property

    Public ReadOnly Property [CustomerIdentifier] As String Implements HTTP_SOAP_INTFC.IParentApp.CustomerIdentifier
        Get
            Return m_customerId
        End Get
    End Property

    Public ReadOnly Property [OracleConnectionString1] As String Implements HTTP_SOAP_INTFC.IParentApp.OracleConnectionString1
        Get
            Return m_oraCNString1
        End Get
    End Property

    Public ReadOnly Property [TemporaryFilePath] As String Implements HTTP_SOAP_INTFC.IParentApp.TemporaryFilePath
        Get
            Return m_tempPath
        End Get
    End Property

    Public ReadOnly Property [Username] As String Implements HTTP_SOAP_INTFC.IParentApp.Username
        Get
            Return m_userId
        End Get
    End Property

    Public ReadOnly Property [Password] As String Implements HTTP_SOAP_INTFC.IParentApp.Password
        Get
            Return m_password
        End Get
    End Property

    Public ReadOnly Property [Attributes] As Hashtable Implements HTTP_SOAP_INTFC.IParentApp.Attributes
        Get
            If (m_attribs Is Nothing) Then
                m_attribs = New Hashtable
            End If
            Return m_attribs
        End Get
    End Property

#End Region

End Class
