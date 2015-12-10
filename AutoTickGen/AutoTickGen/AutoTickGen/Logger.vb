Imports System.IO

Public Class Logger
    Private m_sLogFileSpec As String

    Public Sub New(ByVal sLogPath As String, sFilePrefix As String)
        Try
            If Not System.IO.Directory.Exists(sLogPath) Then
                System.IO.Directory.CreateDirectory(sLogPath)
            End If

            m_sLogFileSpec = sLogPath & "\" & sFilePrefix & Now.ToString("_yyyyMMdd_HHmmtt") & ".log"

        Catch ex As Exception
        End Try
    End Sub

    Public Sub WriteLine(sMessage As String)
        Dim sw As New StreamWriter(File.Open(m_sLogFileSpec, FileMode.Append))

        Dim sLogLine As String = Now.ToString("yyyyMMdd HH:mm:sstt") & ControlChars.Tab & sMessage

        sw.WriteLine(sLogLine)
        sw.Flush()
        sw.Close()
    End Sub

End Class
