Partial Public Class DBErrorPage
    Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object

    Protected WithEvents lblError As System.Web.UI.WebControls.Label
    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim strDBErrorMsgPath As String
        Dim strDescription As String
        strDBErrorMsgPath = Server.MapPath("") & "\DBDownMsg\DBErrorMsg.txt"

        strDescription = ISOLPunchIn.ISOLPunchINFunc.getDBDownMsg(strDBErrorMsgPath)
        If Trim(strDescription) = "" And _
            Not Request.QueryString("MSG") Is Nothing Then
            strDescription = Request.QueryString("MSG")
        End If
        If Trim(strDescription) = "" Then
            strDescription = "<DIV style='DISPLAY: inline; FONT-FAMILY: Arial; HEIGHT: 15px'" & _
            "ms_positioning='FlowLayout'>The Insiteonline Database is down or the application has encountered an error." & _
            "  Please try again in a few moments...If the problem persist," & _
            " please contact your SDI site contact or call the SDI Customer Service at 888-435-7734" & _
            " to report the problem." & _
            "  We apologize for the inconvenience.</DIV><br>" & vbCrLf
        Else
            strDescription = "<DIV style='DISPLAY: inline; FONT-FAMILY: Arial; HEIGHT: 15px'" & _
            "ms_positioning='FlowLayout'>" & strDescription & "</DIV><br>"
        End If
        lblError.Text = strDescription

    End Sub

End Class
