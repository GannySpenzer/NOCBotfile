Imports System.IO


Public Class LoggerClass
    Private g_TextStream As Scripting.TextStream
    Private sLogPath As String
    Private mlngLongestItemLen As Integer

    Public Function SetLogPath(ByVal sPath As String) As Boolean

        sLogPath = sPath


    End Function
    Public Sub Log_Event(ByVal sMessage As String)

        g_TextStream.WriteLine(sMessage)

    End Sub

    Public Sub Open_Log_file()

        Dim fsoLogFile As New Scripting.FileSystemObject

        Try

            'If Not fsoLogFile.FolderExists(sLogPath) Then
            '    fsoLogFile.CreateFolder(sLogPath)
            'End If

            g_TextStream = fsoLogFile.OpenTextFile(sLogPath & "\" & "CytecPO_PDF_" & Format(Now, "ddMMMyyyy") & ".log", Scripting.IOMode.ForAppending, True)
            g_TextStream.WriteLine("Log File Opened at " & Format(Now, "dd-MMM-yy hh:mm"))

        Catch
            Err.Clear()
        End Try

        Exit Sub

    End Sub
    Public Sub Open_Log_file(ByVal sFilePrefix As String)

        Dim fsoLogFile As New Scripting.FileSystemObject

        Try

            If Not fsoLogFile.FolderExists(sLogPath) Then
                fsoLogFile.CreateFolder(sLogPath)
            End If

            g_TextStream = fsoLogFile.OpenTextFile(sLogPath & "\" & sFilePrefix & Format(Now, "ddMMMyyyyhhmm") & ".log", Scripting.IOMode.ForAppending, True)
            g_TextStream.WriteLine("****************************************************" & vbCrLf & _
                                   "Log File Opened at " & Format(Now, "dd-MMM-yy hh:mm"))

        Catch
            Err.Clear()
        End Try

        Exit Sub

    End Sub

    Public Sub Close_Log_file()

        g_TextStream.WriteLine("Log File Closed at " & Format(Now, "dd-MMM-yy hh:mm") & vbCrLf & _
                               "****************************************************")
        g_TextStream.Close()

    End Sub

    Public Sub New()

    End Sub
    Public Sub New(ByVal sPath As String)

        sLogPath = sPath

    End Sub
End Class
