Public Class SetupForm

    Public runControl As runControl
    Private _currentRunControlIndex As Integer
    


    ReadOnly Property CurrentRunInformation() As runInformation
        Get
            Return runControl.Items(CurrentRunControlIndex - 1)
        End Get
    End Property
    Property CurrentRunControlIndex() As Integer
        Get
            Return _currentRunControlIndex
        End Get
        Set(ByVal value As Integer)
            _currentRunControlIndex = value
            PopulateFormFromRunInformation(CurrentRunInformation)

        End Set
    End Property
    Public ReadOnly Property DataFileName() As String
        Get
            Dim folder As String
            folder = Environment.CurrentDirectory
            Return folder & "\RunControl.xml"
        End Get
    End Property
    Public Sub SaveChanges()
        runControl.Save(DataFileName)

    End Sub
    Private Sub UpdateCurrent()
        PopulateRunInfoFromForm(CurrentRunInformation)
    End Sub
    Private Sub SetupForm_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        UpdateCurrent()
        SaveChanges()
    End Sub
    Private Sub SetupForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        runControl = runControl.Load(DataFileName, GetType(runControl))
        If runControl.Items.Count = 0 Then runControl.AddRunInformation()
        CurrentRunControlIndex = 1

    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim runInformation As New runInformation
        PopulateRunInfoFromForm(runInformation)
        Dim filename As String = DataFileName
        runInformation.Save(filename)
        'msgbox("Information has been saved to " & filename)
    End Sub

    Public Sub PopulateRunInfoFromForm(ByVal runInformation As runInformation)
        Dim sTempPassword As String

        If chkEnabled.Checked Then
            runInformation.Enabled = True
        Else
            runInformation.Enabled = False
        End If

        runInformation.RunName = txtRunName.Text

        If String.IsNullOrEmpty(txtLastRun.Text) Then
            runInformation.LastRunTime = Now.AddYears(-1)
        Else
            runInformation.LastRunTime = txtLastRun.Text
        End If
        If String.IsNullOrEmpty(txtURL.Text) Then
            runInformation.ExchangeURL = "https://mail.sdi.com/EWS/Exchange.asmx"
        Else
            runInformation.ExchangeURL = txtURL.Text
        End If
        runInformation.EmailAddress = txtEmail.Text
        runInformation.SourceFolder = txtSourceFolder.Text

        'If chkMoveProcessed.Checked Then
        '    runInformation.ProcessedFolderEnabled = "ENABLED"
        'Else
        '    runInformation.ProcessedFolderEnabled = "DISABLED"
        'End If
        runInformation.ProcessedFolderEnabled = chkMoveProcessed.Checked
        runInformation.ProcessedFolderParent = txtProcessedParent.Text
        runInformation.ProcessedFolder = txtProcessedFolder.Text
        runInformation.ReviewedFolder = txtReviewedFolder.Text
        runInformation.UserName = txtUserName.Text
        txtPassword.Text = txtPassword.Text.Trim

        If txtPassword.Text <> "" Then
            Dim clsPassword As New PasswordClass
            sTempPassword = txtPassword.Text
            sTempPassword = clsPassword.EncryptPassword(sTempPassword)
        Else
            sTempPassword = ""
        End If
        runInformation.Password = sTempPassword
        runInformation.SaveToFolderEnabled = chkSaveAttachment.Checked
        runInformation.SaveToFolder = txtSaveToFolder.Text

        runInformation.UseLastRunFilter = chkUseLastRunTime.Checked

        runInformation.UseFileTypeFilter = chkUseFileFilter.Checked
        runInformation.FileTypes = txtFileTypes.Text
        runInformation.RenameFile = chkRenameFile.Checked
        runInformation.RenamingFormat = txtRenamingFormat.Text
        runInformation.RenamingDateFormat = txtDateFormat.Text
        runInformation.CreateDateAsMessageDate = chkCreateDateAsMessageDate.Checked
        runInformation.DuplicateFileHandler = cboDuplicateFile.SelectedIndex
        runInformation.RemoveRE = chkRemoveRE.Checked

        runInformation.FilterEmails = rbtnEmailFilter.Checked
        runInformation.SubjectFilterEnabled = chkFilterSubject.Checked
        runInformation.SubjectFilter = txtSubjectFilter.Text
        runInformation.AttachmentNameFilterEnabled = chkFilterFileName.Checked
        runInformation.AttachmentNameFilter = txtFilenameFilter.Text
        runInformation.AttachmentSizeLargerEnabled = chkFilterSizeGreater.Checked
        runInformation.AttachmentSizeLargerFilter = txtAttSizeGreaterFilter.Text
        runInformation.AttachmentSizeSmallEnabled = chkFilterSizeLess.Checked
        runInformation.AttachmentSizeSmallFilter = txtAttSizeLessFilter.Text
        runInformation.PrintAttachments = chkPrintAttachment.Checked
        runInformation.NoAttachmentHandler = cboNoAttachment.SelectedIndex


    End Sub

    Private Sub btnLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        Dim newRunInformation As runInformation = runInformation.Load(DataFileName, GetType(runInformation))

        PopulateFormFromRunInformation(newRunInformation)

    End Sub
    Public Sub PopulateFormFromRunInformation(ByVal newRunInfo As runInformation)
        Dim sTempPassword As String
        chkEnabled.Checked = newRunInfo.Enabled

        txtRunName.Text = newRunInfo.RunName

        If newRunInfo.LastRunTime = "1/1/0001 12:00:00 AM" Then
            txtLastRun.Text = ""
        Else
            txtLastRun.Text = newRunInfo.LastRunTime
        End If
        txtURL.Text = newRunInfo.ExchangeURL
        txtEmail.Text = newRunInfo.EmailAddress
        txtSourceFolder.Text = newRunInfo.SourceFolder
        chkMoveProcessed.Checked = newRunInfo.ProcessedFolderEnabled
        'If newRunInfo.ProcessedFolderEnabled = "ENABLED" Then
        '    chkMoveProcessed.Checked = True
        'Else
        '    chkMoveProcessed.Checked = False
        'End If

        txtProcessedParent.Text = newRunInfo.ProcessedFolderParent
        txtProcessedFolder.Text = newRunInfo.ProcessedFolder
        txtReviewedFolder.Text = newRunInfo.ReviewedFolder
        txtUserName.Text = newRunInfo.UserName
        If newRunInfo.Password Is Nothing Then
            txtPassword.Text = newRunInfo.Password
        Else
            sTempPassword = newRunInfo.Password.Trim

            If sTempPassword <> "" Then
                Dim clsPassword As New PasswordClass
                sTempPassword = clsPassword.DecryptPassword(sTempPassword)

            Else
                sTempPassword = ""
            End If
            txtPassword.Text = sTempPassword
        End If

        chkSaveAttachment.Checked = newRunInfo.SaveToFolderEnabled
        txtSaveToFolder.Text = newRunInfo.SaveToFolder
        chkUseLastRunTime.Checked = newRunInfo.UseLastRunFilter
        chkUseFileFilter.Checked = newRunInfo.UseFileTypeFilter
        txtFileTypes.Text = newRunInfo.FileTypes
        chkRenameFile.Checked = newRunInfo.RenameFile
        txtRenamingFormat.Text = newRunInfo.RenamingFormat
        txtDateFormat.Text = newRunInfo.RenamingDateFormat
        chkCreateDateAsMessageDate.Checked = newRunInfo.CreateDateAsMessageDate
        cboDuplicateFile.SelectedIndex = newRunInfo.DuplicateFileHandler
        chkRemoveRE.Checked = newRunInfo.RemoveRE

        If newRunInfo.FilterEmails Then
            rbtnProcessAll.Checked = False
            rbtnEmailFilter.Checked = True
        Else
            rbtnProcessAll.Checked = True
            rbtnEmailFilter.Checked = False
        End If
        chkFilterSubject.Checked = newRunInfo.SubjectFilterEnabled
        txtSubjectFilter.Text = newRunInfo.SubjectFilter
        chkFilterFileName.Checked = newRunInfo.AttachmentNameFilterEnabled
        txtFilenameFilter.Text = newRunInfo.AttachmentNameFilter
        chkFilterSizeGreater.Checked = newRunInfo.AttachmentSizeLargerEnabled
        txtAttSizeGreaterFilter.Text = newRunInfo.AttachmentSizeLargerFilter
        chkFilterSizeLess.Checked = newRunInfo.AttachmentSizeSmallEnabled
        txtAttSizeLessFilter.Text = newRunInfo.AttachmentSizeSmallFilter
        chkPrintAttachment.Checked = newRunInfo.PrintAttachments
        cboNoAttachment.SelectedIndex = newRunInfo.NoAttachmentHandler


    End Sub
    Public Function AddNewRun() As runInformation
        Dim newInfo As runInformation = runControl.AddRunInformation
        CurrentRunControlIndex = runControl.Items.Count
        Return newInfo
    End Function

    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNew.Click
        AddNewRun()
    End Sub

    Private Sub btnPrevious_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrevious.Click
        MovePrevious()
    End Sub

    Private Sub btnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNext.Click
        MoveNext()
    End Sub
    Public Sub MoveNext()
        Dim newIndex As Integer = CurrentRunControlIndex + 1
        If newIndex > runControl.Items.Count Then newIndex = 1
        UpdateCurrent()
        CurrentRunControlIndex = newIndex
    End Sub
    Public Sub MovePrevious()
        Dim newIndex As Integer = CurrentRunControlIndex - 1
        If newIndex = 0 Then newIndex = runControl.Items.Count
        UpdateCurrent()
        CurrentRunControlIndex = newIndex
    End Sub
    Public Sub DeleteRun(ByVal Index As Integer)
        runControl.Items.RemoveAt(Index - 1)
        If runControl.Items.Count = 0 Then
            runControl.AddRunInformation()
        Else
            If Index > runControl.Items.Count Then
                Index = runControl.Items.Count
            End If
        End If
        CurrentRunControlIndex = Index

    End Sub
    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        DeleteRun(CurrentRunControlIndex)
    End Sub

    Private Sub chkMoveProcessed_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If chkMoveProcessed.Checked Then
            txtProcessedParent.Enabled = True
            txtProcessedFolder.Enabled = True
        Else
            txtProcessedParent.Enabled = False
            txtProcessedFolder.Enabled = False
        End If
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub chkSaveAttachment_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If chkSaveAttachment.Checked Then
            txtSaveToFolder.Enabled = True
        Else
            txtSaveToFolder.Enabled = False
        End If
    End Sub

    Private Sub btnFolderBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFolderBrowse.Click
        Dim dialogResult As Windows.Forms.DialogResult

        dialogResult = FolderBrowserDialog1.ShowDialog(Me)
        If dialogResult = Windows.Forms.DialogResult.OK Then
            txtSaveToFolder.Text = FolderBrowserDialog1.SelectedPath
        End If

    End Sub

    Private Sub cboDuplicateFile_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboDuplicateFile.SelectedIndexChanged

    End Sub

    Private Sub txtSourceFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSourceFolder.TextChanged

    End Sub

    Private Sub rbtnEmailFilter_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnEmailFilter.CheckedChanged
        If rbtnEmailFilter.Checked Then
            grpboxFilter.Enabled = True
        Else
            grpboxFilter.Enabled = False
        End If
    End Sub

    Private Sub chkMoveProcessed_CheckedChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMoveProcessed.CheckedChanged

    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtReviewedFolder.TextChanged

    End Sub
End Class