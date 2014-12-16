Public Class Alert
    Public Function Say(ByVal Message As String) As String
        ' Format string properly
        Message = Message.Replace("'", "\'")
        Message = Message.Replace(Convert.ToChar(10), "\n")
        Message = Message.Replace(Convert.ToChar(13), "")
        ' Display as JavaScript alert
        'ltlAlert.Text
        Dim strMessage As String = "alert('" & Message & "')"
        Return strMessage
    End Function

End Class
