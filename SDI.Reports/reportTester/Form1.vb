Imports SDI

Public Class Form1

    Inherits System.Windows.Forms.Form
    Implements SDI.ApplicationLogger.IApplicationLogger

    Private Const oraCN_default_provider As String = "Provider=MSDAORA.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=PROD"



#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button
        Me.TextBox1 = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(8, 8)
        Me.Button1.Name = "Button1"
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Button1"
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(8, 40)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBox1.Size = New System.Drawing.Size(880, 424)
        Me.TextBox1.TabIndex = 1
        Me.TextBox1.Text = ""
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(896, 478)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click

        Me.TextBox1.Text = ""

        Dim oPicking As New SDI.PickingReports.ReportGenerator

        oPicking.BusinessUnit = "I0260"  '  "I0469"
        oPicking.OrderNo = "NY00301727"  '  N230003224"
        oPicking.oraCNstring = oraCN_default_provider & _
                               oraCN_default_creden & _
                               oraCN_default_DB

        oPicking.Logger = Me
        'oPicking.ExecutionPath = "C:\Projects\SDI.Reports\SDI.PickingReports\bin\"
        oPicking.GenerateReports(PickingReports.ReportGenerator.ePickingReportType.packingSlipRegPick)
        'oPicking.GenerateReports(PickingReports.ReportGenerator.ePickingReportType.shippingDocRegPick)
        oPicking = Nothing

    End Sub

#Region " IApplicationLogger Implementation "

    Public ReadOnly Property LoggingLevel() As System.Diagnostics.TraceLevel Implements ApplicationLogger.IApplicationLogger.LoggingLevel
        Get
            Return TraceLevel.Verbose
        End Get
    End Property

    Public Sub LogError(ByVal msg As String) Implements ApplicationLogger.IApplicationLogger.WriteErrorLog
        Me.TextBox1.Text &= "ERRO: " & msg & vbCrLf
    End Sub

    Public Sub LogInfo(ByVal msg As String) Implements ApplicationLogger.IApplicationLogger.WriteInformationLog
        Me.TextBox1.Text &= "INFO: " & msg & vbCrLf
    End Sub

    Public Sub WriteLog(ByVal msg As String, ByVal logAs As System.Diagnostics.TraceLevel) Implements ApplicationLogger.IApplicationLogger.WriteLog
        Me.TextBox1.Text &= "????: " & msg & vbCrLf
    End Sub

    Public Sub LogVerbose(ByVal msg As String) Implements ApplicationLogger.IApplicationLogger.WriteVerboseLog
        Me.TextBox1.Text &= "VERB: " & msg & vbCrLf
    End Sub

    Public Sub LogWarning(ByVal msg As String) Implements ApplicationLogger.IApplicationLogger.WriteWarningLog
        Me.TextBox1.Text &= "WARN: " & msg & vbCrLf
    End Sub

#End Region

End Class
