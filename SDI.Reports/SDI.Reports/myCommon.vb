Imports System
Imports System.Data
Imports System.Data.OleDb


Friend NotInheritable Class myCommon

    ''' <summary>
    '''    prevent this class from being instantiated from outside
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    '''    loads/reads the specified SQL statement from a file
    ''' </summary>
    Public Shared Function LoadQuery(ByVal qryPathFile As String) As String
        Dim sQry As String = ""
        If qryPathFile.Trim.Length > 0 Then
            ' parse into path & filename
            '   .GetDirectoryName() DOES NOT include the last "\" within the path string
            Dim sqlPath As String = System.IO.Path.GetDirectoryName(qryPathFile)
            Dim sqlFileName As String = System.IO.Path.GetFileName(qryPathFile)
            ' check path/file and read
            If System.IO.Directory.Exists(Path:=sqlPath) Then
                Dim bIsAppend As Boolean = False
                ' check the log file if exists
                If System.IO.File.Exists(path:=qryPathFile) Then
                    Dim rdr As New System.IO.StreamReader(path:=qryPathFile)
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
        Return sQry
    End Function

End Class
