<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SiteUpload
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
        Me.components = New System.ComponentModel.Container()
        Me.cmbSites = New System.Windows.Forms.ComboBox()
        Me.btnGo = New System.Windows.Forms.Button()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.txtSource = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.cmbServer = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnOpenSite = New System.Windows.Forms.Button()
        Me.btnWebConfig = New System.Windows.Forms.Button()
        Me.txtWebConfig = New System.Windows.Forms.TextBox()
        Me.lblWebConfigSrv = New System.Windows.Forms.Label()
        Me.btnTable = New System.Windows.Forms.Button()
        Me.lblSiteDesc = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.lblMessage = New System.Windows.Forms.TextBox()
        Me.txtDescription = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'cmbSites
        '
        Me.cmbSites.FormattingEnabled = True
        Me.cmbSites.Location = New System.Drawing.Point(26, 89)
        Me.cmbSites.Name = "cmbSites"
        Me.cmbSites.Size = New System.Drawing.Size(473, 21)
        Me.cmbSites.TabIndex = 0
        '
        'btnGo
        '
        Me.btnGo.Location = New System.Drawing.Point(362, 319)
        Me.btnGo.Name = "btnGo"
        Me.btnGo.Size = New System.Drawing.Size(218, 76)
        Me.btnGo.TabIndex = 1
        Me.btnGo.Text = "GO"
        Me.btnGo.UseVisualStyleBackColor = True
        '
        'txtSource
        '
        Me.txtSource.Location = New System.Drawing.Point(26, 150)
        Me.txtSource.Name = "txtSource"
        Me.txtSource.Size = New System.Drawing.Size(473, 20)
        Me.txtSource.TabIndex = 2
        Me.ToolTip1.SetToolTip(Me.txtSource, "Double click on source directory to open File Explorer")
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(23, 134)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(89, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Source Directory:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(23, 73)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(60, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Upload To:"
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'btnBrowse
        '
        Me.btnBrowse.Location = New System.Drawing.Point(505, 149)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(75, 21)
        Me.btnBrowse.TabIndex = 5
        Me.btnBrowse.Text = "Browse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'cmbServer
        '
        Me.cmbServer.FormattingEnabled = True
        Me.cmbServer.Location = New System.Drawing.Point(26, 35)
        Me.cmbServer.Name = "cmbServer"
        Me.cmbServer.Size = New System.Drawing.Size(121, 21)
        Me.cmbServer.TabIndex = 6
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(23, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(41, 13)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Server:"
        '
        'btnOpenSite
        '
        Me.btnOpenSite.Location = New System.Drawing.Point(505, 87)
        Me.btnOpenSite.Name = "btnOpenSite"
        Me.btnOpenSite.Size = New System.Drawing.Size(75, 23)
        Me.btnOpenSite.TabIndex = 10
        Me.btnOpenSite.Text = "Test Site"
        Me.btnOpenSite.UseVisualStyleBackColor = True
        '
        'btnWebConfig
        '
        Me.btnWebConfig.Location = New System.Drawing.Point(505, 195)
        Me.btnWebConfig.Name = "btnWebConfig"
        Me.btnWebConfig.Size = New System.Drawing.Size(75, 23)
        Me.btnWebConfig.TabIndex = 11
        Me.btnWebConfig.Text = "Browse"
        Me.btnWebConfig.UseVisualStyleBackColor = True
        '
        'txtWebConfig
        '
        Me.txtWebConfig.Location = New System.Drawing.Point(26, 198)
        Me.txtWebConfig.Name = "txtWebConfig"
        Me.txtWebConfig.Size = New System.Drawing.Size(473, 20)
        Me.txtWebConfig.TabIndex = 12
        Me.ToolTip1.SetToolTip(Me.txtWebConfig, "Double click on web.config directory to open File Explorer")
        '
        'lblWebConfigSrv
        '
        Me.lblWebConfigSrv.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWebConfigSrv.Location = New System.Drawing.Point(302, 224)
        Me.lblWebConfigSrv.Name = "lblWebConfigSrv"
        Me.lblWebConfigSrv.Size = New System.Drawing.Size(197, 17)
        Me.lblWebConfigSrv.TabIndex = 13
        Me.lblWebConfigSrv.Text = "server"
        Me.lblWebConfigSrv.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'btnTable
        '
        Me.btnTable.Location = New System.Drawing.Point(454, 33)
        Me.btnTable.Name = "btnTable"
        Me.btnTable.Size = New System.Drawing.Size(126, 23)
        Me.btnTable.TabIndex = 14
        Me.btnTable.Text = "View Oracle Table"
        Me.btnTable.UseVisualStyleBackColor = True
        '
        'lblSiteDesc
        '
        Me.lblSiteDesc.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSiteDesc.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblSiteDesc.Location = New System.Drawing.Point(149, 113)
        Me.lblSiteDesc.Name = "lblSiteDesc"
        Me.lblSiteDesc.Size = New System.Drawing.Size(350, 17)
        Me.lblSiteDesc.TabIndex = 15
        Me.lblSiteDesc.Text = "site description"
        Me.lblSiteDesc.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(23, 182)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(111, 13)
        Me.Label5.TabIndex = 17
        Me.Label5.Text = "Web.Config Directory:"
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(26, 284)
        Me.lblMessage.Multiline = True
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.lblMessage.Size = New System.Drawing.Size(317, 111)
        Me.lblMessage.TabIndex = 18
        '
        'txtDescription
        '
        Me.txtDescription.Location = New System.Drawing.Point(362, 284)
        Me.txtDescription.Name = "txtDescription"
        Me.txtDescription.Size = New System.Drawing.Size(218, 20)
        Me.txtDescription.TabIndex = 19
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(359, 265)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(139, 13)
        Me.Label4.TabIndex = 20
        Me.Label4.Text = "Enter Description (Optional):"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(25, 265)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(51, 13)
        Me.Label6.TabIndex = 21
        Me.Label6.Text = "Progress:"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(28, 419)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(554, 23)
        Me.ProgressBar1.TabIndex = 22
        '
        'BackgroundWorker1
        '
        Me.BackgroundWorker1.WorkerReportsProgress = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(182, 238)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 23
        Me.Button1.Text = "test"
        Me.Button1.UseVisualStyleBackColor = True
        Me.Button1.Visible = False
        '
        'Timer1
        '
        Me.Timer1.Interval = 5000
        '
        'lblProgress
        '
        Me.lblProgress.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProgress.Location = New System.Drawing.Point(155, 451)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(427, 17)
        Me.lblProgress.TabIndex = 24
        Me.lblProgress.Text = "File 0 of 0 processed"
        Me.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblProgress.Visible = False
        '
        'SiteUpload
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(607, 477)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.lblSiteDesc)
        Me.Controls.Add(Me.btnTable)
        Me.Controls.Add(Me.lblWebConfigSrv)
        Me.Controls.Add(Me.txtWebConfig)
        Me.Controls.Add(Me.btnWebConfig)
        Me.Controls.Add(Me.btnOpenSite)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.cmbServer)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSource)
        Me.Controls.Add(Me.btnGo)
        Me.Controls.Add(Me.cmbSites)
        Me.Name = "SiteUpload"
        Me.Text = "Site Upload Utility"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmbSites As System.Windows.Forms.ComboBox
    Friend WithEvents btnGo As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents txtSource As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents cmbServer As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnOpenSite As System.Windows.Forms.Button
    Friend WithEvents btnWebConfig As System.Windows.Forms.Button
    Friend WithEvents txtWebConfig As System.Windows.Forms.TextBox
    Friend WithEvents lblWebConfigSrv As System.Windows.Forms.Label
    Friend WithEvents btnTable As System.Windows.Forms.Button
    Friend WithEvents lblSiteDesc As System.Windows.Forms.Label
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents lblMessage As System.Windows.Forms.TextBox
    Friend WithEvents txtDescription As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents lblProgress As System.Windows.Forms.Label

End Class
