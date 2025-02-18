Public Class punchInUser

    Public [UserId] As String
    Public [UserName] As String
    Public [EmailAddress] As String
    Public [CompanyId] As String
    Public [CompanyPassword] As String
    Public [HookURL] As String
    Public [Workorder] As String


    Public Sub New()
        InitMembers()
    End Sub

    Private Sub InitMembers()
        Me.UserId = ""
        Me.UserName = ""
        Me.EmailAddress = ""
        Me.CompanyId = ""
        Me.CompanyPassword = ""
        Me.HookURL = ""
        Me.Workorder = ""
    End Sub

End Class
