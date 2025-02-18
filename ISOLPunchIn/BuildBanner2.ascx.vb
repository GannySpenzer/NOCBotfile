Class BuildBanner2
    Inherits System.Web.UI.UserControl

    Public FormTitle As String
    Public DisplayTitle As String
    Public HOME As String
    Public TABS As String
    Public sFields As String = ""
    Public sTables As String = ""
    Public sWhere As String = ""
    Public strccfirst As String = "bob.dougherty"
    Public strcclast As String = "sdi.com"
    Public strmailto As String

    Public HOME1 As String
    Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal
    Protected WithEvents LinkPassword As System.Web.UI.WebControls.HyperLink
    Public CARTIMG As String
#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Or _
                Request.ServerVariables("HTTP_HOST").ToString().ToUpper.Substring(0, 6) = "CPTEST" Then
            strmailto = "mailto:" & strccfirst & "@" & strcclast & "?subject=ISOL%20ver%202.0%20issue"
        Else
            strmailto = "mailto:" & Session("SITEEMAIL") & "?bcc=" & strccfirst & "@" & strcclast
        End If
        If Not Page.IsPostBack Then


        End If
        HOME = Request.QueryString("HOME")
        Dim intItems As Integer

    End Sub

End Class

