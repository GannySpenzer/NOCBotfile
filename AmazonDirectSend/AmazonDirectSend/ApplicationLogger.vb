Imports System
Imports System.IO
Imports System.Diagnostics


Public Class ApplicationLogger

    Implements IDisposable

    Private m_logFileName As String = "log.txt"
    Private m_logPath As String = "C:\"
    Private m_logFile As String = m_logPath & m_logFileName
    Private m_logLevel As TraceLevel = TraceLevel.Error


    Public Sub New(Optional ByVal logPathFilename As String = Nothing, _
                   Optional ByVal logLevel As System.Diagnostics.TraceLevel = TraceLevel.Error)
        ' always get the folder for the executing assembly and
        '   use the same folder as default location for our log file
        m_logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & _
                    "\" & m_logFileName

        ' override default if specified path/filename
        If Not (logPathFilename Is Nothing) Then
            If logPathFilename.Length > 0 Then
                m_logFile = logPathFilename
            End If
        End If
        m_logLevel = logLevel

        ' parse into path & filename
        '   .GetDirectoryName() DOES NOT include the last "\" within the path string
        m_logPath = Path.GetDirectoryName(m_logFile)
        m_logFileName = Path.GetFileName(m_logFile)
    End Sub

    Public ReadOnly Property LogFileFullName() As String
        Get
            Return m_logFile
        End Get
    End Property

    Public ReadOnly Property LogFilePath() As String
        Get
            Return m_logPath
        End Get
    End Property

    Public ReadOnly Property LogFileExtensionName() As String
        Get
            Return m_logFileName
        End Get
    End Property

    Public Sub WriteLog(ByVal msg As String, ByVal logAs As System.Diagnostics.TraceLevel)
        AppendLog(msg, logAs)
    End Sub

    Public Sub WriteVerboseLog(ByVal msg As String)
        AppendLog(msg, System.Diagnostics.TraceLevel.Verbose)
    End Sub

    Public Sub WriteInformationLog(ByVal msg As String)
        AppendLog(msg, System.Diagnostics.TraceLevel.Info)
    End Sub

    Public Sub WriteWarningLog(ByVal msg As String)
        AppendLog(msg, System.Diagnostics.TraceLevel.Warning)
    End Sub

    Public Sub WriteErrorLog(ByVal msg As String)
        AppendLog(msg, System.Diagnostics.TraceLevel.Error)
    End Sub

#Region " IDisposable Implementations "

    Public Sub Dispose() Implements System.IDisposable.Dispose

    End Sub

#End Region

    Private Sub AppendLog(ByVal msg As String, ByVal msgLevel As System.Diagnostics.TraceLevel)
        Dim rtn As String = "ApplicationLogger.AppendLog"
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

            If Directory.Exists(Path:=m_logPath) Then
                Dim bIsAppend As Boolean = False
                ' check the log file.
                '   if file does not exist, create.
                If Not File.Exists(Path:=m_logFile) Then
                    Try
                        Dim fs As System.IO.FileStream = File.Create(Path:=m_logFile)
                        fs.Flush()
                        fs.Close()
                        fs = Nothing
                        bIsAppend = True
                    Catch ex As Exception
                        bIsAppend = False
                        Throw New ApplicationException(message:=rtn & " :: " & ex.Message, innerException:=ex)
                    End Try
                Else
                    ' file exist, so OK to append log
                    bIsAppend = True
                End If
                ' append the log
                If bIsAppend Then
                    Try
                        Dim dtFormat As String = "MM/dd/yyyy HH:mm:ss.fff"
                        Dim sLog As String = "" & _
                                             msgLevel.ToString() & vbTab & _
                                             Now.ToString(Format:=dtFormat) & vbTab & _
                                             msg & _
                                             ""
                        Dim sw As New StreamWriter(Path:=m_logFile, append:=True)
                        sw.WriteLine(sLog)
                        sw.Flush()
                        sw.Close()
                        sw = Nothing
                    Catch ex As Exception
                        Throw New ApplicationException(message:=rtn & " :: " & ex.message, innerException:=ex)
                    End Try
                End If
            Else
                Throw New ApplicationException(message:=rtn & " :: Path does not exists -> [" & m_logPath & "]")
            End If

        End If
    End Sub

End Class
