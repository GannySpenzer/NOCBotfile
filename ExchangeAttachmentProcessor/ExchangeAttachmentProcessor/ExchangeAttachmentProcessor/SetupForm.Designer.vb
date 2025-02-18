<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SetupForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnSave = New System.Windows.Forms.Button
        Me.btnLoad = New System.Windows.Forms.Button
        Me.btnPrevious = New System.Windows.Forms.Button
        Me.btnNext = New System.Windows.Forms.Button
        Me.btnNew = New System.Windows.Forms.Button
        Me.btnDelete = New System.Windows.Forms.Button
        Me.TabControl1 = New System.Windows.Forms.TabControl
        Me.General = New System.Windows.Forms.TabPage
        Me.chkUseLastRunTime = New System.Windows.Forms.CheckBox
        Me.txtURL = New System.Windows.Forms.TextBox
        Me.lblURL = New System.Windows.Forms.Label
        Me.txtEmail = New System.Windows.Forms.TextBox
        Me.txtUserName = New System.Windows.Forms.TextBox
        Me.txtPassword = New System.Windows.Forms.TextBox
        Me.txtSourceFolder = New System.Windows.Forms.TextBox
        Me.lblPassword = New System.Windows.Forms.Label
        Me.lblUserName = New System.Windows.Forms.Label
        Me.lblSourceFolder = New System.Windows.Forms.Label
        Me.lblEmail = New System.Windows.Forms.Label
        Me.chkEnabled = New System.Windows.Forms.CheckBox
        Me.lblRunName = New System.Windows.Forms.Label
        Me.txtLastRun = New System.Windows.Forms.TextBox
        Me.txtRunName = New System.Windows.Forms.TextBox
        Me.Processing = New System.Windows.Forms.TabPage
        Me.txtReviewedFolder = New System.Windows.Forms.TextBox
        Me.lblReviewFolder = New System.Windows.Forms.Label
        Me.btnFolderBrowse = New System.Windows.Forms.Button
        Me.txtFileTypes = New System.Windows.Forms.TextBox
        Me.lblFileTypes = New System.Windows.Forms.Label
        Me.chkUseFileFilter = New System.Windows.Forms.CheckBox
        Me.txtSaveToFolder = New System.Windows.Forms.TextBox
        Me.lblSaveToFolder = New System.Windows.Forms.Label
        Me.chkSaveAttachment = New System.Windows.Forms.CheckBox
        Me.txtProcessedFolder = New System.Windows.Forms.TextBox
        Me.txtProcessedParent = New System.Windows.Forms.TextBox
        Me.chkMoveProcessed = New System.Windows.Forms.CheckBox
        Me.lblProcessedFolder = New System.Windows.Forms.Label
        Me.lblProcessParent = New System.Windows.Forms.Label
        Me.FileRenaming = New System.Windows.Forms.TabPage
        Me.lblDateFormat = New System.Windows.Forms.Label
        Me.txtDateFormat = New System.Windows.Forms.TextBox
        Me.chkRemoveRE = New System.Windows.Forms.CheckBox
        Me.lblDupFileHandler = New System.Windows.Forms.Label
        Me.cboDuplicateFile = New System.Windows.Forms.ComboBox
        Me.chkCreateDateAsMessageDate = New System.Windows.Forms.CheckBox
        Me.txtRenamingFormat = New System.Windows.Forms.TextBox
        Me.lblRenamingFormat = New System.Windows.Forms.Label
        Me.chkRenameFile = New System.Windows.Forms.CheckBox
        Me.EmailFilter = New System.Windows.Forms.TabPage
        Me.grpboxFilter = New System.Windows.Forms.GroupBox
        Me.lblKBGreater = New System.Windows.Forms.Label
        Me.lblKBLess = New System.Windows.Forms.Label
        Me.txtSubjectFilter = New System.Windows.Forms.TextBox
        Me.txtFilenameFilter = New System.Windows.Forms.TextBox
        Me.txtAttSizeGreaterFilter = New System.Windows.Forms.TextBox
        Me.txtAttSizeLessFilter = New System.Windows.Forms.TextBox
        Me.chkFilterSizeLess = New System.Windows.Forms.CheckBox
        Me.chkFilterSizeGreater = New System.Windows.Forms.CheckBox
        Me.chkFilterFileName = New System.Windows.Forms.CheckBox
        Me.chkFilterSubject = New System.Windows.Forms.CheckBox
        Me.rbtnEmailFilter = New System.Windows.Forms.RadioButton
        Me.rbtnProcessAll = New System.Windows.Forms.RadioButton
        Me.Print = New System.Windows.Forms.TabPage
        Me.cboNoAttachment = New System.Windows.Forms.ComboBox
        Me.lblNoAttachment = New System.Windows.Forms.Label
        Me.chkPrintAttachment = New System.Windows.Forms.CheckBox
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog
        Me.ChkSaveEmailBody = New System.Windows.Forms.CheckBox
        Me.TabControl1.SuspendLayout()
        Me.General.SuspendLayout()
        Me.Processing.SuspendLayout()
        Me.FileRenaming.SuspendLayout()
        Me.EmailFilter.SuspendLayout()
        Me.grpboxFilter.SuspendLayout()
        Me.Print.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.Enabled = False
        Me.btnSave.Location = New System.Drawing.Point(244, 434)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 18
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnLoad
        '
        Me.btnLoad.Enabled = False
        Me.btnLoad.Location = New System.Drawing.Point(125, 433)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(75, 23)
        Me.btnLoad.TabIndex = 17
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'btnPrevious
        '
        Me.btnPrevious.Location = New System.Drawing.Point(55, 463)
        Me.btnPrevious.Name = "btnPrevious"
        Me.btnPrevious.Size = New System.Drawing.Size(75, 23)
        Me.btnPrevious.TabIndex = 19
        Me.btnPrevious.Text = "Previous"
        Me.btnPrevious.UseVisualStyleBackColor = True
        '
        'btnNext
        '
        Me.btnNext.Location = New System.Drawing.Point(139, 463)
        Me.btnNext.Name = "btnNext"
        Me.btnNext.Size = New System.Drawing.Size(75, 23)
        Me.btnNext.TabIndex = 20
        Me.btnNext.Text = "Next"
        Me.btnNext.UseVisualStyleBackColor = True
        '
        'btnNew
        '
        Me.btnNew.Location = New System.Drawing.Point(223, 463)
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(75, 23)
        Me.btnNew.TabIndex = 21
        Me.btnNew.Text = "New"
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        Me.btnDelete.Location = New System.Drawing.Point(307, 463)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(75, 23)
        Me.btnDelete.TabIndex = 22
        Me.btnDelete.Text = "Delete"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.General)
        Me.TabControl1.Controls.Add(Me.Processing)
        Me.TabControl1.Controls.Add(Me.FileRenaming)
        Me.TabControl1.Controls.Add(Me.EmailFilter)
        Me.TabControl1.Controls.Add(Me.Print)
        Me.TabControl1.Location = New System.Drawing.Point(28, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(406, 403)
        Me.TabControl1.TabIndex = 8
        '
        'General
        '
        Me.General.Controls.Add(Me.chkUseLastRunTime)
        Me.General.Controls.Add(Me.txtURL)
        Me.General.Controls.Add(Me.lblURL)
        Me.General.Controls.Add(Me.txtEmail)
        Me.General.Controls.Add(Me.txtUserName)
        Me.General.Controls.Add(Me.txtPassword)
        Me.General.Controls.Add(Me.txtSourceFolder)
        Me.General.Controls.Add(Me.lblPassword)
        Me.General.Controls.Add(Me.lblUserName)
        Me.General.Controls.Add(Me.lblSourceFolder)
        Me.General.Controls.Add(Me.lblEmail)
        Me.General.Controls.Add(Me.chkEnabled)
        Me.General.Controls.Add(Me.lblRunName)
        Me.General.Controls.Add(Me.txtLastRun)
        Me.General.Controls.Add(Me.txtRunName)
        Me.General.Location = New System.Drawing.Point(4, 22)
        Me.General.Name = "General"
        Me.General.Padding = New System.Windows.Forms.Padding(3)
        Me.General.Size = New System.Drawing.Size(398, 377)
        Me.General.TabIndex = 0
        Me.General.Text = "General"
        Me.General.UseVisualStyleBackColor = True
        '
        'chkUseLastRunTime
        '
        Me.chkUseLastRunTime.AutoSize = True
        Me.chkUseLastRunTime.Checked = True
        Me.chkUseLastRunTime.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseLastRunTime.Location = New System.Drawing.Point(26, 90)
        Me.chkUseLastRunTime.Name = "chkUseLastRunTime"
        Me.chkUseLastRunTime.Size = New System.Drawing.Size(165, 30)
        Me.chkUseLastRunTime.TabIndex = 40
        Me.chkUseLastRunTime.Text = "Only process Messages" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Recieved after Last RunTime"
        Me.chkUseLastRunTime.UseVisualStyleBackColor = True
        '
        'txtURL
        '
        Me.txtURL.Location = New System.Drawing.Point(202, 134)
        Me.txtURL.Name = "txtURL"
        Me.txtURL.ReadOnly = True
        Me.txtURL.Size = New System.Drawing.Size(149, 20)
        Me.txtURL.TabIndex = 3
        '
        'lblURL
        '
        Me.lblURL.AutoSize = True
        Me.lblURL.Location = New System.Drawing.Point(111, 137)
        Me.lblURL.Name = "lblURL"
        Me.lblURL.Size = New System.Drawing.Size(80, 13)
        Me.lblURL.TabIndex = 39
        Me.lblURL.Text = "Exchange URL"
        '
        'txtEmail
        '
        Me.txtEmail.Location = New System.Drawing.Point(201, 168)
        Me.txtEmail.Name = "txtEmail"
        Me.txtEmail.Size = New System.Drawing.Size(150, 20)
        Me.txtEmail.TabIndex = 4
        '
        'txtUserName
        '
        Me.txtUserName.Location = New System.Drawing.Point(201, 202)
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New System.Drawing.Size(150, 20)
        Me.txtUserName.TabIndex = 5
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(201, 236)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(150, 20)
        Me.txtPassword.TabIndex = 6
        '
        'txtSourceFolder
        '
        Me.txtSourceFolder.Location = New System.Drawing.Point(201, 270)
        Me.txtSourceFolder.Name = "txtSourceFolder"
        Me.txtSourceFolder.Size = New System.Drawing.Size(150, 20)
        Me.txtSourceFolder.TabIndex = 7
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New System.Drawing.Point(138, 240)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(53, 13)
        Me.lblPassword.TabIndex = 34
        Me.lblPassword.Text = "Password"
        '
        'lblUserName
        '
        Me.lblUserName.AutoSize = True
        Me.lblUserName.Location = New System.Drawing.Point(131, 204)
        Me.lblUserName.Name = "lblUserName"
        Me.lblUserName.Size = New System.Drawing.Size(60, 13)
        Me.lblUserName.TabIndex = 33
        Me.lblUserName.Text = "User Name"
        '
        'lblSourceFolder
        '
        Me.lblSourceFolder.AutoSize = True
        Me.lblSourceFolder.Location = New System.Drawing.Point(118, 273)
        Me.lblSourceFolder.Name = "lblSourceFolder"
        Me.lblSourceFolder.Size = New System.Drawing.Size(73, 13)
        Me.lblSourceFolder.TabIndex = 32
        Me.lblSourceFolder.Text = "Source Folder"
        '
        'lblEmail
        '
        Me.lblEmail.AutoSize = True
        Me.lblEmail.Location = New System.Drawing.Point(118, 170)
        Me.lblEmail.Name = "lblEmail"
        Me.lblEmail.Size = New System.Drawing.Size(73, 13)
        Me.lblEmail.TabIndex = 31
        Me.lblEmail.Text = "Email Address"
        '
        'chkEnabled
        '
        Me.chkEnabled.AutoSize = True
        Me.chkEnabled.Location = New System.Drawing.Point(39, 27)
        Me.chkEnabled.Name = "chkEnabled"
        Me.chkEnabled.Size = New System.Drawing.Size(65, 17)
        Me.chkEnabled.TabIndex = 0
        Me.chkEnabled.Text = "Enabled"
        Me.chkEnabled.UseVisualStyleBackColor = True
        '
        'lblRunName
        '
        Me.lblRunName.AutoSize = True
        Me.lblRunName.Location = New System.Drawing.Point(120, 29)
        Me.lblRunName.Name = "lblRunName"
        Me.lblRunName.Size = New System.Drawing.Size(58, 13)
        Me.lblRunName.TabIndex = 28
        Me.lblRunName.Text = "Run Name"
        '
        'txtLastRun
        '
        Me.txtLastRun.Location = New System.Drawing.Point(201, 100)
        Me.txtLastRun.Name = "txtLastRun"
        Me.txtLastRun.ReadOnly = True
        Me.txtLastRun.Size = New System.Drawing.Size(150, 20)
        Me.txtLastRun.TabIndex = 2
        '
        'txtRunName
        '
        Me.txtRunName.Location = New System.Drawing.Point(201, 25)
        Me.txtRunName.Name = "txtRunName"
        Me.txtRunName.Size = New System.Drawing.Size(150, 20)
        Me.txtRunName.TabIndex = 1
        '
        'Processing
        '
        Me.Processing.Controls.Add(Me.ChkSaveEmailBody)
        Me.Processing.Controls.Add(Me.txtReviewedFolder)
        Me.Processing.Controls.Add(Me.lblReviewFolder)
        Me.Processing.Controls.Add(Me.btnFolderBrowse)
        Me.Processing.Controls.Add(Me.txtFileTypes)
        Me.Processing.Controls.Add(Me.lblFileTypes)
        Me.Processing.Controls.Add(Me.chkUseFileFilter)
        Me.Processing.Controls.Add(Me.txtSaveToFolder)
        Me.Processing.Controls.Add(Me.lblSaveToFolder)
        Me.Processing.Controls.Add(Me.chkSaveAttachment)
        Me.Processing.Controls.Add(Me.txtProcessedFolder)
        Me.Processing.Controls.Add(Me.txtProcessedParent)
        Me.Processing.Controls.Add(Me.chkMoveProcessed)
        Me.Processing.Controls.Add(Me.lblProcessedFolder)
        Me.Processing.Controls.Add(Me.lblProcessParent)
        Me.Processing.Location = New System.Drawing.Point(4, 22)
        Me.Processing.Name = "Processing"
        Me.Processing.Padding = New System.Windows.Forms.Padding(3)
        Me.Processing.Size = New System.Drawing.Size(398, 377)
        Me.Processing.TabIndex = 1
        Me.Processing.Text = "Processing"
        Me.Processing.UseVisualStyleBackColor = True
        '
        'txtReviewedFolder
        '
        Me.txtReviewedFolder.Location = New System.Drawing.Point(167, 113)
        Me.txtReviewedFolder.Name = "txtReviewedFolder"
        Me.txtReviewedFolder.Size = New System.Drawing.Size(213, 20)
        Me.txtReviewedFolder.TabIndex = 40
        '
        'lblReviewFolder
        '
        Me.lblReviewFolder.AutoSize = True
        Me.lblReviewFolder.Location = New System.Drawing.Point(25, 117)
        Me.lblReviewFolder.Name = "lblReviewFolder"
        Me.lblReviewFolder.Size = New System.Drawing.Size(119, 13)
        Me.lblReviewFolder.TabIndex = 39
        Me.lblReviewFolder.Text = "To Be Reviewed Folder"
        '
        'btnFolderBrowse
        '
        Me.btnFolderBrowse.Location = New System.Drawing.Point(69, 226)
        Me.btnFolderBrowse.Name = "btnFolderBrowse"
        Me.btnFolderBrowse.Size = New System.Drawing.Size(72, 23)
        Me.btnFolderBrowse.TabIndex = 13
        Me.btnFolderBrowse.Text = "Browse"
        Me.btnFolderBrowse.UseVisualStyleBackColor = True
        '
        'txtFileTypes
        '
        Me.txtFileTypes.Location = New System.Drawing.Point(166, 310)
        Me.txtFileTypes.Name = "txtFileTypes"
        Me.txtFileTypes.Size = New System.Drawing.Size(213, 20)
        Me.txtFileTypes.TabIndex = 16
        '
        'lblFileTypes
        '
        Me.lblFileTypes.AutoSize = True
        Me.lblFileTypes.Location = New System.Drawing.Point(89, 317)
        Me.lblFileTypes.Name = "lblFileTypes"
        Me.lblFileTypes.Size = New System.Drawing.Size(55, 13)
        Me.lblFileTypes.TabIndex = 38
        Me.lblFileTypes.Text = "File Types"
        '
        'chkUseFileFilter
        '
        Me.chkUseFileFilter.AutoSize = True
        Me.chkUseFileFilter.Location = New System.Drawing.Point(3, 294)
        Me.chkUseFileFilter.Name = "chkUseFileFilter"
        Me.chkUseFileFilter.Size = New System.Drawing.Size(130, 17)
        Me.chkUseFileFilter.TabIndex = 15
        Me.chkUseFileFilter.Text = "Use File Type Filtering"
        Me.chkUseFileFilter.UseVisualStyleBackColor = True
        '
        'txtSaveToFolder
        '
        Me.txtSaveToFolder.Location = New System.Drawing.Point(167, 186)
        Me.txtSaveToFolder.Multiline = True
        Me.txtSaveToFolder.Name = "txtSaveToFolder"
        Me.txtSaveToFolder.Size = New System.Drawing.Size(213, 105)
        Me.txtSaveToFolder.TabIndex = 14
        '
        'lblSaveToFolder
        '
        Me.lblSaveToFolder.AutoSize = True
        Me.lblSaveToFolder.Location = New System.Drawing.Point(63, 206)
        Me.lblSaveToFolder.Name = "lblSaveToFolder"
        Me.lblSaveToFolder.Size = New System.Drawing.Size(80, 13)
        Me.lblSaveToFolder.TabIndex = 35
        Me.lblSaveToFolder.Text = "Save To Folder"
        '
        'chkSaveAttachment
        '
        Me.chkSaveAttachment.AutoSize = True
        Me.chkSaveAttachment.Checked = True
        Me.chkSaveAttachment.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSaveAttachment.Location = New System.Drawing.Point(5, 186)
        Me.chkSaveAttachment.Name = "chkSaveAttachment"
        Me.chkSaveAttachment.Size = New System.Drawing.Size(160, 17)
        Me.chkSaveAttachment.TabIndex = 12
        Me.chkSaveAttachment.Text = "Save Attachements to folder"
        Me.chkSaveAttachment.UseVisualStyleBackColor = True
        '
        'txtProcessedFolder
        '
        Me.txtProcessedFolder.Location = New System.Drawing.Point(167, 83)
        Me.txtProcessedFolder.Name = "txtProcessedFolder"
        Me.txtProcessedFolder.Size = New System.Drawing.Size(213, 20)
        Me.txtProcessedFolder.TabIndex = 11
        '
        'txtProcessedParent
        '
        Me.txtProcessedParent.Location = New System.Drawing.Point(167, 55)
        Me.txtProcessedParent.Name = "txtProcessedParent"
        Me.txtProcessedParent.Size = New System.Drawing.Size(213, 20)
        Me.txtProcessedParent.TabIndex = 10
        '
        'chkMoveProcessed
        '
        Me.chkMoveProcessed.AutoSize = True
        Me.chkMoveProcessed.Checked = True
        Me.chkMoveProcessed.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkMoveProcessed.Location = New System.Drawing.Point(6, 23)
        Me.chkMoveProcessed.Name = "chkMoveProcessed"
        Me.chkMoveProcessed.Size = New System.Drawing.Size(139, 17)
        Me.chkMoveProcessed.TabIndex = 9
        Me.chkMoveProcessed.Text = "Move Processed Emails"
        Me.chkMoveProcessed.UseVisualStyleBackColor = True
        '
        'lblProcessedFolder
        '
        Me.lblProcessedFolder.AutoSize = True
        Me.lblProcessedFolder.Location = New System.Drawing.Point(55, 87)
        Me.lblProcessedFolder.Name = "lblProcessedFolder"
        Me.lblProcessedFolder.Size = New System.Drawing.Size(89, 13)
        Me.lblProcessedFolder.TabIndex = 30
        Me.lblProcessedFolder.Text = "Processed Folder"
        '
        'lblProcessParent
        '
        Me.lblProcessParent.AutoSize = True
        Me.lblProcessParent.Location = New System.Drawing.Point(33, 62)
        Me.lblProcessParent.Name = "lblProcessParent"
        Me.lblProcessParent.Size = New System.Drawing.Size(111, 13)
        Me.lblProcessParent.TabIndex = 29
        Me.lblProcessParent.Text = "Process Folder Parent"
        '
        'FileRenaming
        '
        Me.FileRenaming.Controls.Add(Me.lblDateFormat)
        Me.FileRenaming.Controls.Add(Me.txtDateFormat)
        Me.FileRenaming.Controls.Add(Me.chkRemoveRE)
        Me.FileRenaming.Controls.Add(Me.lblDupFileHandler)
        Me.FileRenaming.Controls.Add(Me.cboDuplicateFile)
        Me.FileRenaming.Controls.Add(Me.chkCreateDateAsMessageDate)
        Me.FileRenaming.Controls.Add(Me.txtRenamingFormat)
        Me.FileRenaming.Controls.Add(Me.lblRenamingFormat)
        Me.FileRenaming.Controls.Add(Me.chkRenameFile)
        Me.FileRenaming.Location = New System.Drawing.Point(4, 22)
        Me.FileRenaming.Name = "FileRenaming"
        Me.FileRenaming.Size = New System.Drawing.Size(398, 377)
        Me.FileRenaming.TabIndex = 2
        Me.FileRenaming.Text = "File Renaming"
        Me.FileRenaming.UseVisualStyleBackColor = True
        '
        'lblDateFormat
        '
        Me.lblDateFormat.AutoSize = True
        Me.lblDateFormat.Location = New System.Drawing.Point(41, 86)
        Me.lblDateFormat.Name = "lblDateFormat"
        Me.lblDateFormat.Size = New System.Drawing.Size(65, 13)
        Me.lblDateFormat.TabIndex = 8
        Me.lblDateFormat.Text = "Date Format"
        '
        'txtDateFormat
        '
        Me.txtDateFormat.Location = New System.Drawing.Point(112, 82)
        Me.txtDateFormat.Name = "txtDateFormat"
        Me.txtDateFormat.Size = New System.Drawing.Size(270, 20)
        Me.txtDateFormat.TabIndex = 7
        '
        'chkRemoveRE
        '
        Me.chkRemoveRE.AutoSize = True
        Me.chkRemoveRE.Checked = True
        Me.chkRemoveRE.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRemoveRE.Location = New System.Drawing.Point(16, 113)
        Me.chkRemoveRE.Name = "chkRemoveRE"
        Me.chkRemoveRE.Size = New System.Drawing.Size(189, 17)
        Me.chkRemoveRE.TabIndex = 6
        Me.chkRemoveRE.Text = "Remove RE: && FWD: from Subject"
        Me.chkRemoveRE.UseVisualStyleBackColor = True
        '
        'lblDupFileHandler
        '
        Me.lblDupFileHandler.AutoSize = True
        Me.lblDupFileHandler.Location = New System.Drawing.Point(16, 152)
        Me.lblDupFileHandler.Name = "lblDupFileHandler"
        Me.lblDupFileHandler.Size = New System.Drawing.Size(83, 13)
        Me.lblDupFileHandler.TabIndex = 5
        Me.lblDupFileHandler.Text = "If the File Exists "
        '
        'cboDuplicateFile
        '
        Me.cboDuplicateFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDuplicateFile.FormattingEnabled = True
        Me.cboDuplicateFile.Items.AddRange(New Object() {"", "Append unique number to end of filename if file exists", "Overwrite existing files with the same name", "Do Not Save if file already exists with the same name"})
        Me.cboDuplicateFile.Location = New System.Drawing.Point(112, 152)
        Me.cboDuplicateFile.Name = "cboDuplicateFile"
        Me.cboDuplicateFile.Size = New System.Drawing.Size(270, 21)
        Me.cboDuplicateFile.TabIndex = 4
        '
        'chkCreateDateAsMessageDate
        '
        Me.chkCreateDateAsMessageDate.AutoSize = True
        Me.chkCreateDateAsMessageDate.Checked = True
        Me.chkCreateDateAsMessageDate.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCreateDateAsMessageDate.Location = New System.Drawing.Point(16, 194)
        Me.chkCreateDateAsMessageDate.Name = "chkCreateDateAsMessageDate"
        Me.chkCreateDateAsMessageDate.Size = New System.Drawing.Size(204, 17)
        Me.chkCreateDateAsMessageDate.TabIndex = 3
        Me.chkCreateDateAsMessageDate.Text = "Set file create date as message date?"
        Me.chkCreateDateAsMessageDate.UseVisualStyleBackColor = True
        '
        'txtRenamingFormat
        '
        Me.txtRenamingFormat.Location = New System.Drawing.Point(112, 56)
        Me.txtRenamingFormat.Name = "txtRenamingFormat"
        Me.txtRenamingFormat.Size = New System.Drawing.Size(270, 20)
        Me.txtRenamingFormat.TabIndex = 2
        '
        'lblRenamingFormat
        '
        Me.lblRenamingFormat.AutoSize = True
        Me.lblRenamingFormat.Location = New System.Drawing.Point(16, 60)
        Me.lblRenamingFormat.Name = "lblRenamingFormat"
        Me.lblRenamingFormat.Size = New System.Drawing.Size(90, 13)
        Me.lblRenamingFormat.TabIndex = 1
        Me.lblRenamingFormat.Text = "Renaming Format"
        '
        'chkRenameFile
        '
        Me.chkRenameFile.AutoSize = True
        Me.chkRenameFile.Location = New System.Drawing.Point(16, 20)
        Me.chkRenameFile.Name = "chkRenameFile"
        Me.chkRenameFile.Size = New System.Drawing.Size(85, 17)
        Me.chkRenameFile.TabIndex = 0
        Me.chkRenameFile.Text = "Rename File"
        Me.chkRenameFile.UseVisualStyleBackColor = True
        '
        'EmailFilter
        '
        Me.EmailFilter.Controls.Add(Me.grpboxFilter)
        Me.EmailFilter.Controls.Add(Me.rbtnEmailFilter)
        Me.EmailFilter.Controls.Add(Me.rbtnProcessAll)
        Me.EmailFilter.Location = New System.Drawing.Point(4, 22)
        Me.EmailFilter.Name = "EmailFilter"
        Me.EmailFilter.Size = New System.Drawing.Size(398, 377)
        Me.EmailFilter.TabIndex = 3
        Me.EmailFilter.Text = "Email Filtering"
        Me.EmailFilter.UseVisualStyleBackColor = True
        '
        'grpboxFilter
        '
        Me.grpboxFilter.Controls.Add(Me.lblKBGreater)
        Me.grpboxFilter.Controls.Add(Me.lblKBLess)
        Me.grpboxFilter.Controls.Add(Me.txtSubjectFilter)
        Me.grpboxFilter.Controls.Add(Me.txtFilenameFilter)
        Me.grpboxFilter.Controls.Add(Me.txtAttSizeGreaterFilter)
        Me.grpboxFilter.Controls.Add(Me.txtAttSizeLessFilter)
        Me.grpboxFilter.Controls.Add(Me.chkFilterSizeLess)
        Me.grpboxFilter.Controls.Add(Me.chkFilterSizeGreater)
        Me.grpboxFilter.Controls.Add(Me.chkFilterFileName)
        Me.grpboxFilter.Controls.Add(Me.chkFilterSubject)
        Me.grpboxFilter.Location = New System.Drawing.Point(11, 69)
        Me.grpboxFilter.Name = "grpboxFilter"
        Me.grpboxFilter.Size = New System.Drawing.Size(375, 280)
        Me.grpboxFilter.TabIndex = 2
        Me.grpboxFilter.TabStop = False
        '
        'lblKBGreater
        '
        Me.lblKBGreater.AutoSize = True
        Me.lblKBGreater.Location = New System.Drawing.Point(341, 92)
        Me.lblKBGreater.Name = "lblKBGreater"
        Me.lblKBGreater.Size = New System.Drawing.Size(21, 13)
        Me.lblKBGreater.TabIndex = 9
        Me.lblKBGreater.Text = "KB"
        '
        'lblKBLess
        '
        Me.lblKBLess.AutoSize = True
        Me.lblKBLess.Location = New System.Drawing.Point(341, 125)
        Me.lblKBLess.Name = "lblKBLess"
        Me.lblKBLess.Size = New System.Drawing.Size(21, 13)
        Me.lblKBLess.TabIndex = 8
        Me.lblKBLess.Text = "KB"
        '
        'txtSubjectFilter
        '
        Me.txtSubjectFilter.Location = New System.Drawing.Point(191, 27)
        Me.txtSubjectFilter.Name = "txtSubjectFilter"
        Me.txtSubjectFilter.Size = New System.Drawing.Size(178, 20)
        Me.txtSubjectFilter.TabIndex = 7
        '
        'txtFilenameFilter
        '
        Me.txtFilenameFilter.Location = New System.Drawing.Point(191, 58)
        Me.txtFilenameFilter.Name = "txtFilenameFilter"
        Me.txtFilenameFilter.Size = New System.Drawing.Size(178, 20)
        Me.txtFilenameFilter.TabIndex = 6
        '
        'txtAttSizeGreaterFilter
        '
        Me.txtAttSizeGreaterFilter.Location = New System.Drawing.Point(191, 89)
        Me.txtAttSizeGreaterFilter.Name = "txtAttSizeGreaterFilter"
        Me.txtAttSizeGreaterFilter.Size = New System.Drawing.Size(148, 20)
        Me.txtAttSizeGreaterFilter.TabIndex = 5
        '
        'txtAttSizeLessFilter
        '
        Me.txtAttSizeLessFilter.Location = New System.Drawing.Point(191, 121)
        Me.txtAttSizeLessFilter.Name = "txtAttSizeLessFilter"
        Me.txtAttSizeLessFilter.Size = New System.Drawing.Size(148, 20)
        Me.txtAttSizeLessFilter.TabIndex = 4
        '
        'chkFilterSizeLess
        '
        Me.chkFilterSizeLess.AutoSize = True
        Me.chkFilterSizeLess.Location = New System.Drawing.Point(11, 124)
        Me.chkFilterSizeLess.Name = "chkFilterSizeLess"
        Me.chkFilterSizeLess.Size = New System.Drawing.Size(171, 17)
        Me.chkFilterSizeLess.TabIndex = 3
        Me.chkFilterSizeLess.Text = "If Attachment size is less than: "
        Me.chkFilterSizeLess.UseVisualStyleBackColor = True
        '
        'chkFilterSizeGreater
        '
        Me.chkFilterSizeGreater.AutoSize = True
        Me.chkFilterSizeGreater.Location = New System.Drawing.Point(11, 92)
        Me.chkFilterSizeGreater.Name = "chkFilterSizeGreater"
        Me.chkFilterSizeGreater.Size = New System.Drawing.Size(186, 17)
        Me.chkFilterSizeGreater.TabIndex = 2
        Me.chkFilterSizeGreater.Text = "If Attachment size is greater than: "
        Me.chkFilterSizeGreater.UseVisualStyleBackColor = True
        '
        'chkFilterFileName
        '
        Me.chkFilterFileName.AutoSize = True
        Me.chkFilterFileName.Location = New System.Drawing.Point(11, 61)
        Me.chkFilterFileName.Name = "chkFilterFileName"
        Me.chkFilterFileName.Size = New System.Drawing.Size(177, 17)
        Me.chkFilterFileName.TabIndex = 1
        Me.chkFilterFileName.Text = "If Attachment filename contains:"
        Me.chkFilterFileName.UseVisualStyleBackColor = True
        '
        'chkFilterSubject
        '
        Me.chkFilterSubject.AutoSize = True
        Me.chkFilterSubject.Location = New System.Drawing.Point(11, 30)
        Me.chkFilterSubject.Name = "chkFilterSubject"
        Me.chkFilterSubject.Size = New System.Drawing.Size(115, 17)
        Me.chkFilterSubject.TabIndex = 0
        Me.chkFilterSubject.Text = "If Subject Contains"
        Me.chkFilterSubject.UseVisualStyleBackColor = True
        '
        'rbtnEmailFilter
        '
        Me.rbtnEmailFilter.AutoSize = True
        Me.rbtnEmailFilter.Location = New System.Drawing.Point(8, 39)
        Me.rbtnEmailFilter.Name = "rbtnEmailFilter"
        Me.rbtnEmailFilter.Size = New System.Drawing.Size(169, 17)
        Me.rbtnEmailFilter.TabIndex = 1
        Me.rbtnEmailFilter.Text = "Process Only those that match"
        Me.rbtnEmailFilter.UseVisualStyleBackColor = True
        '
        'rbtnProcessAll
        '
        Me.rbtnProcessAll.AutoSize = True
        Me.rbtnProcessAll.Checked = True
        Me.rbtnProcessAll.Location = New System.Drawing.Point(8, 16)
        Me.rbtnProcessAll.Name = "rbtnProcessAll"
        Me.rbtnProcessAll.Size = New System.Drawing.Size(110, 17)
        Me.rbtnProcessAll.TabIndex = 0
        Me.rbtnProcessAll.TabStop = True
        Me.rbtnProcessAll.Text = "Process All Emails"
        Me.rbtnProcessAll.UseVisualStyleBackColor = True
        '
        'Print
        '
        Me.Print.Controls.Add(Me.cboNoAttachment)
        Me.Print.Controls.Add(Me.lblNoAttachment)
        Me.Print.Controls.Add(Me.chkPrintAttachment)
        Me.Print.Location = New System.Drawing.Point(4, 22)
        Me.Print.Name = "Print"
        Me.Print.Size = New System.Drawing.Size(398, 377)
        Me.Print.TabIndex = 4
        Me.Print.Text = "Printing Options"
        Me.Print.UseVisualStyleBackColor = True
        '
        'cboNoAttachment
        '
        Me.cboNoAttachment.FormattingEnabled = True
        Me.cboNoAttachment.Items.AddRange(New Object() {"", "Do nothing and mark email as processed", "Do nothing and mark email for review", "Print email message"})
        Me.cboNoAttachment.Location = New System.Drawing.Point(138, 54)
        Me.cboNoAttachment.Name = "cboNoAttachment"
        Me.cboNoAttachment.Size = New System.Drawing.Size(199, 21)
        Me.cboNoAttachment.TabIndex = 2
        '
        'lblNoAttachment
        '
        Me.lblNoAttachment.Location = New System.Drawing.Point(27, 49)
        Me.lblNoAttachment.Name = "lblNoAttachment"
        Me.lblNoAttachment.Size = New System.Drawing.Size(105, 30)
        Me.lblNoAttachment.TabIndex = 1
        Me.lblNoAttachment.Text = "If email does not have an attachment"
        '
        'chkPrintAttachment
        '
        Me.chkPrintAttachment.AutoSize = True
        Me.chkPrintAttachment.Location = New System.Drawing.Point(23, 23)
        Me.chkPrintAttachment.Name = "chkPrintAttachment"
        Me.chkPrintAttachment.Size = New System.Drawing.Size(109, 17)
        Me.chkPrintAttachment.TabIndex = 0
        Me.chkPrintAttachment.Text = "Print Attachments"
        Me.chkPrintAttachment.UseVisualStyleBackColor = True
        '
        'ChkSaveEmailBody
        '
        Me.ChkSaveEmailBody.AutoSize = True
        Me.ChkSaveEmailBody.Location = New System.Drawing.Point(3, 151)
        Me.ChkSaveEmailBody.Name = "ChkSaveEmailBody"
        Me.ChkSaveEmailBody.Size = New System.Drawing.Size(106, 17)
        Me.ChkSaveEmailBody.TabIndex = 41
        Me.ChkSaveEmailBody.Text = "Save Email Body"
        Me.ChkSaveEmailBody.UseVisualStyleBackColor = True
        '
        'SetupForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(468, 498)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.btnPrevious)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.btnSave)
        Me.Name = "SetupForm"
        Me.Text = "Setup"
        Me.TabControl1.ResumeLayout(False)
        Me.General.ResumeLayout(False)
        Me.General.PerformLayout()
        Me.Processing.ResumeLayout(False)
        Me.Processing.PerformLayout()
        Me.FileRenaming.ResumeLayout(False)
        Me.FileRenaming.PerformLayout()
        Me.EmailFilter.ResumeLayout(False)
        Me.EmailFilter.PerformLayout()
        Me.grpboxFilter.ResumeLayout(False)
        Me.grpboxFilter.PerformLayout()
        Me.Print.ResumeLayout(False)
        Me.Print.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents btnPrevious As System.Windows.Forms.Button
    Friend WithEvents btnNext As System.Windows.Forms.Button
    Friend WithEvents btnNew As System.Windows.Forms.Button
    Friend WithEvents btnDelete As System.Windows.Forms.Button
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents General As System.Windows.Forms.TabPage
    Friend WithEvents txtURL As System.Windows.Forms.TextBox
    Friend WithEvents lblURL As System.Windows.Forms.Label
    Friend WithEvents txtEmail As System.Windows.Forms.TextBox
    Friend WithEvents txtUserName As System.Windows.Forms.TextBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtSourceFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents lblUserName As System.Windows.Forms.Label
    Friend WithEvents lblSourceFolder As System.Windows.Forms.Label
    Friend WithEvents lblEmail As System.Windows.Forms.Label
    Friend WithEvents chkEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents lblRunName As System.Windows.Forms.Label
    Friend WithEvents txtLastRun As System.Windows.Forms.TextBox
    Friend WithEvents txtRunName As System.Windows.Forms.TextBox
    Friend WithEvents Processing As System.Windows.Forms.TabPage
    Friend WithEvents lblFileTypes As System.Windows.Forms.Label
    Friend WithEvents chkUseFileFilter As System.Windows.Forms.CheckBox
    Friend WithEvents txtSaveToFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblSaveToFolder As System.Windows.Forms.Label
    Friend WithEvents chkSaveAttachment As System.Windows.Forms.CheckBox
    Friend WithEvents txtProcessedFolder As System.Windows.Forms.TextBox
    Friend WithEvents txtProcessedParent As System.Windows.Forms.TextBox
    Friend WithEvents chkMoveProcessed As System.Windows.Forms.CheckBox
    Friend WithEvents lblProcessedFolder As System.Windows.Forms.Label
    Friend WithEvents lblProcessParent As System.Windows.Forms.Label
    Friend WithEvents txtFileTypes As System.Windows.Forms.TextBox
    Friend WithEvents btnFolderBrowse As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents FileRenaming As System.Windows.Forms.TabPage
    Friend WithEvents lblRenamingFormat As System.Windows.Forms.Label
    Friend WithEvents chkRenameFile As System.Windows.Forms.CheckBox
    Friend WithEvents txtRenamingFormat As System.Windows.Forms.TextBox
    Friend WithEvents chkCreateDateAsMessageDate As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseLastRunTime As System.Windows.Forms.CheckBox
    Friend WithEvents cboDuplicateFile As System.Windows.Forms.ComboBox
    Friend WithEvents lblDupFileHandler As System.Windows.Forms.Label
    Friend WithEvents chkRemoveRE As System.Windows.Forms.CheckBox
    Friend WithEvents EmailFilter As System.Windows.Forms.TabPage
    Friend WithEvents rbtnEmailFilter As System.Windows.Forms.RadioButton
    Friend WithEvents rbtnProcessAll As System.Windows.Forms.RadioButton
    Friend WithEvents grpboxFilter As System.Windows.Forms.GroupBox
    Friend WithEvents chkFilterSizeLess As System.Windows.Forms.CheckBox
    Friend WithEvents chkFilterSizeGreater As System.Windows.Forms.CheckBox
    Friend WithEvents chkFilterFileName As System.Windows.Forms.CheckBox
    Friend WithEvents chkFilterSubject As System.Windows.Forms.CheckBox
    Friend WithEvents txtSubjectFilter As System.Windows.Forms.TextBox
    Friend WithEvents txtFilenameFilter As System.Windows.Forms.TextBox
    Friend WithEvents txtAttSizeGreaterFilter As System.Windows.Forms.TextBox
    Friend WithEvents txtAttSizeLessFilter As System.Windows.Forms.TextBox
    Friend WithEvents lblKBGreater As System.Windows.Forms.Label
    Friend WithEvents lblKBLess As System.Windows.Forms.Label
    Friend WithEvents Print As System.Windows.Forms.TabPage
    Friend WithEvents lblNoAttachment As System.Windows.Forms.Label
    Friend WithEvents chkPrintAttachment As System.Windows.Forms.CheckBox
    Friend WithEvents cboNoAttachment As System.Windows.Forms.ComboBox
    Friend WithEvents txtReviewedFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblReviewFolder As System.Windows.Forms.Label
    Friend WithEvents lblDateFormat As System.Windows.Forms.Label
    Friend WithEvents txtDateFormat As System.Windows.Forms.TextBox
    Friend WithEvents ChkSaveEmailBody As System.Windows.Forms.CheckBox
End Class
