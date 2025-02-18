Public Class stringEncoder

    '// prevent the world from creating an instance of this class
    Private Sub New()

    End Sub

    Public Shared Function escapeString(ByVal s As String) As String
        Dim str1 As System.Text.StringBuilder = New System.Text.StringBuilder(s)
        If str1.Length > 0 Then
            ' pair "single quote"
            str1 = str1.Replace("''", "&psqu;")
            ' single "single quote"
            str1 = str1.Replace("'", "&squo;")
            ' pair "double quote"
            str1 = str1.Replace("""" & """", "&pdqu;")
            ' single "double quote"
            str1 = str1.Replace("""", "&dquo;")
            ' amp
            str1 = str1.Replace("&", "&amp;")
        End If
        ' return
        Return (str1.ToString)
    End Function

    Public Shared Function decodeString(ByVal s As String) As String
        Dim str1 As System.Text.StringBuilder = New System.Text.StringBuilder(s)
        If str1.Length > 0 Then
            ' amp
            str1 = str1.Replace("&amp;", "&")
            ' single "double quote"
            str1 = str1.Replace("&dquo;", """")
            ' pair "double quote"
            str1 = str1.Replace("&pdqu;", """" & """")
            ' single "single quote"
            str1 = str1.Replace("&squo;", "'")
            ' pair "single quote"
            str1 = str1.Replace("&psqu;", "''")
        End If
        ' return
        Return (str1.ToString)
    End Function

    Public Shared Function formStringForSQL(ByVal sIn As String, Optional ByVal nMaxLength As Integer = 0) As String
        Dim sOut As System.Text.StringBuilder = New System.Text.StringBuilder(sIn)
        If sOut.Length > 0 Then
            ' search for any "encoded" single "single quote" and replace with a pair of "single quote"
            sOut = sOut.Replace("&squo;", "&psqu;")
            ' search for any "encoded" single "double quote" and replace with a pair of "double quote"
            sOut = sOut.Replace("&dquo;", "&pdqu;")
        End If
        ' un-escape the string
        sOut = New System.Text.StringBuilder(stringEncoder.decodeString(sOut.ToString))
        ' check returning maximum length
        '   if maximum length is Zero (0), return the whole string
        If nMaxLength = 0 Then
            nMaxLength = sOut.Length
        End If
        ' return ONLY maximum chars
        If sOut.Length > nMaxLength Then
            sOut = New System.Text.StringBuilder(sOut.ToString.Substring(0, nMaxLength))
        End If
        Return (sOut.ToString)
    End Function

End Class
