
Partial Class _Default
    Inherits System.Web.UI.Page

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        'Response.Write("<input type=""hidden"" id=""signature"" name=""signature"" value=""" + security.sign(parameters) + """/>")
    End Sub
End Class
