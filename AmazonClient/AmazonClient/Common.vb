Imports System
Imports System.Data
Imports System.Data.OleDb


Public NotInheritable Class Common

    ''' <summary>
    '''    prevent this class from being instantiated from outside
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    '''    loads/reads the specified SQL statement from a file
    ''' </summary>
    Public Shared Function LoadPathFile(ByVal pathFile As String) As String
        Dim rtn As String = "myCommon.LoadPathFile"
        Dim sQry As String = ""
        Try
            If pathFile.Trim.Length > 0 Then
                ' parse into path & filename
                '   .GetDirectoryName() DOES NOT include the last "\" within the path string
                Dim sqlPath As String = System.IO.Path.GetDirectoryName(pathFile)
                Dim sqlFileName As String = System.IO.Path.GetFileName(pathFile)
                ' check path/file and read
                If System.IO.Directory.Exists(Path:=sqlPath) Then
                    Dim bIsAppend As Boolean = False
                    ' check the log file if exists
                    If System.IO.File.Exists(path:=pathFile) Then
                        Dim rdr As New System.IO.StreamReader(path:=pathFile)
                        sQry = rdr.ReadToEnd
                        rdr.Close()
                        rdr = Nothing
                    Else
                        ' send an error here!
                        '   path/file was not found
                    End If
                Else
                    ' send an error here!
                    '   path was not found
                    'Throw New ApplicationException(message:=rtn & " :: Path does not exists -> [" & m_logPath & "]")
                End If
            Else
                ' send an error here!
                '   no path/file was specified
            End If
        Catch ex As Exception
            Throw New ApplicationException(rtn & "::" & ex.Message, ex)
        End Try
        Return sQry
    End Function

    ''' <summary>
    '''    clean up a given string by ...
    '''         * removing cr and/or lf and combination
    '''         * removing pre/post spaces
    ''' </summary>
    Public Shared Function RemoveCrLf(ByVal s As String) As String
        Dim ret As String = s
        If Not (ret Is Nothing) Then
            If ret.Length > 0 Then
                ret = ret.Replace(vbCrLf, " ")
                ret = ret.Replace(vbCr, " ")
                ret = ret.Replace(vbLf, " ")
                ret = ret.Trim
            End If
        End If
        Return ret
    End Function

    ' ''' <summary>
    ' '''    writes the msg as log entry using the supplied application logger instance
    ' ''' </summary>
    'Public Shared Sub WriteLog(ByVal appLogger As SDI.PunchOut.ApplicationLogger, ByVal msg As String, ByVal level As System.Diagnostics.TraceLevel)
    '    If Not (appLogger Is Nothing) Then
    '        appLogger.WriteLog(msg, level)
    '    End If
    'End Sub

End Class
