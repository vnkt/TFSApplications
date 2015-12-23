<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
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
        Me.lblVersionLabel = New System.Windows.Forms.Label()
        Me.txtVersion = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'lblVersionLabel
        '
        Me.lblVersionLabel.AutoSize = True
        Me.lblVersionLabel.Location = New System.Drawing.Point(12, 9)
        Me.lblVersionLabel.Name = "lblVersionLabel"
        Me.lblVersionLabel.Size = New System.Drawing.Size(97, 13)
        Me.lblVersionLabel.TabIndex = 0
        Me.lblVersionLabel.Text = "Application Version"
        '
        'txtVersion
        '
        Me.txtVersion.Location = New System.Drawing.Point(115, 6)
        Me.txtVersion.Name = "txtVersion"
        Me.txtVersion.ReadOnly = True
        Me.txtVersion.Size = New System.Drawing.Size(287, 20)
        Me.txtVersion.TabIndex = 1
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(414, 37)
        Me.Controls.Add(Me.txtVersion)
        Me.Controls.Add(Me.lblVersionLabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmMain"
        Me.Text = "ClickOnce Application"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblVersionLabel As System.Windows.Forms.Label
    Friend WithEvents txtVersion As System.Windows.Forms.TextBox

End Class
