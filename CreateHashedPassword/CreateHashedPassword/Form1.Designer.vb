﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtSource = New System.Windows.Forms.TextBox()
        Me.txtResult = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(28, 47)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(180, 23)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Password must start with INNEW"
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(288, 47)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(252, 23)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Make sure results are strictly alpha-numeric"
        '
        'txtSource
        '
        Me.txtSource.Location = New System.Drawing.Point(31, 86)
        Me.txtSource.Name = "txtSource"
        Me.txtSource.Size = New System.Drawing.Size(177, 20)
        Me.txtSource.TabIndex = 2
        '
        'txtResult
        '
        Me.txtResult.Location = New System.Drawing.Point(291, 86)
        Me.txtResult.Name = "txtResult"
        Me.txtResult.Size = New System.Drawing.Size(249, 20)
        Me.txtResult.TabIndex = 3
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(235, 187)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(125, 23)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "Create Hash"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(564, 261)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.txtResult)
        Me.Controls.Add(Me.txtSource)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "Create Password"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtSource As System.Windows.Forms.TextBox
    Friend WithEvents txtResult As System.Windows.Forms.TextBox
    Friend WithEvents Button1 As System.Windows.Forms.Button

End Class
