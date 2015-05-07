<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmConfiguration
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.cbDisableMenue_Snap = New System.Windows.Forms.CheckBox()
        Me.cbDisableMenue_LiveChannels = New System.Windows.Forms.CheckBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.cbDisableMenue_Lifestyle = New System.Windows.Forms.CheckBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.tbSkyGoImageCacheDays = New System.Windows.Forms.NumericUpDown()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.tbSkyGoTimeout = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.tbSkyGoFSKPIN = New System.Windows.Forms.TextBox()
        Me.tbSkyGoPIN = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.tbSkyGoUserID = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Menue = New System.Windows.Forms.TabControl()
        Me.TabPage3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        CType(Me.tbSkyGoImageCacheDays, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbSkyGoTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Menue.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(507, 276)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 2
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(426, 276)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(75, 23)
        Me.btnCancel.TabIndex = 3
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.GroupBox2)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage3.Size = New System.Drawing.Size(566, 245)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "about"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.Label9)
        Me.GroupBox2.Location = New System.Drawing.Point(60, 53)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(446, 139)
        Me.GroupBox2.TabIndex = 1
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "about SkyGoDe Plugin"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(6, 29)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(375, 13)
        Me.Label8.TabIndex = 1
        Me.Label8.Text = "This plugin enables the playing of on demand content from the Skygo website."
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(6, 59)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(149, 13)
        Me.Label9.TabIndex = 0
        Me.Label9.Text = "Version 0.97 RC / 09.04.2015"
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.cbDisableMenue_Snap)
        Me.TabPage2.Controls.Add(Me.cbDisableMenue_LiveChannels)
        Me.TabPage2.Controls.Add(Me.Label11)
        Me.TabPage2.Controls.Add(Me.cbDisableMenue_Lifestyle)
        Me.TabPage2.Controls.Add(Me.Label10)
        Me.TabPage2.Controls.Add(Me.Label2)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(566, 245)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Menue"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'cbDisableMenue_Snap
        '
        Me.cbDisableMenue_Snap.AutoSize = True
        Me.cbDisableMenue_Snap.Location = New System.Drawing.Point(154, 92)
        Me.cbDisableMenue_Snap.Name = "cbDisableMenue_Snap"
        Me.cbDisableMenue_Snap.Size = New System.Drawing.Size(15, 14)
        Me.cbDisableMenue_Snap.TabIndex = 3
        Me.cbDisableMenue_Snap.UseVisualStyleBackColor = True
        '
        'cbDisableMenue_LiveChannels
        '
        Me.cbDisableMenue_LiveChannels.AutoSize = True
        Me.cbDisableMenue_LiveChannels.Location = New System.Drawing.Point(154, 69)
        Me.cbDisableMenue_LiveChannels.Name = "cbDisableMenue_LiveChannels"
        Me.cbDisableMenue_LiveChannels.Size = New System.Drawing.Size(15, 14)
        Me.cbDisableMenue_LiveChannels.TabIndex = 2
        Me.cbDisableMenue_LiveChannels.UseVisualStyleBackColor = True
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(61, 70)
        Me.Label11.Name = "Label11"
        Me.Label11.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label11.Size = New System.Drawing.Size(77, 13)
        Me.Label11.TabIndex = 8
        Me.Label11.Text = "Live-Channels:"
        '
        'cbDisableMenue_Lifestyle
        '
        Me.cbDisableMenue_Lifestyle.AutoSize = True
        Me.cbDisableMenue_Lifestyle.Location = New System.Drawing.Point(154, 47)
        Me.cbDisableMenue_Lifestyle.Name = "cbDisableMenue_Lifestyle"
        Me.cbDisableMenue_Lifestyle.Size = New System.Drawing.Size(15, 14)
        Me.cbDisableMenue_Lifestyle.TabIndex = 1
        Me.cbDisableMenue_Lifestyle.UseVisualStyleBackColor = True
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(61, 93)
        Me.Label10.Name = "Label10"
        Me.Label10.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label10.Size = New System.Drawing.Size(35, 13)
        Me.Label10.TabIndex = 3
        Me.Label10.Text = "Snap:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 18)
        Me.Label2.Name = "Label2"
        Me.Label2.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label2.Size = New System.Drawing.Size(106, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "disable menu entries:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(61, 46)
        Me.Label1.Name = "Label1"
        Me.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label1.Size = New System.Drawing.Size(48, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Lifestyle:"
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.tbSkyGoImageCacheDays)
        Me.TabPage1.Controls.Add(Me.Label7)
        Me.TabPage1.Controls.Add(Me.tbSkyGoTimeout)
        Me.TabPage1.Controls.Add(Me.Label6)
        Me.TabPage1.Controls.Add(Me.tbSkyGoFSKPIN)
        Me.TabPage1.Controls.Add(Me.tbSkyGoPIN)
        Me.TabPage1.Controls.Add(Me.Label5)
        Me.TabPage1.Controls.Add(Me.Label4)
        Me.TabPage1.Controls.Add(Me.tbSkyGoUserID)
        Me.TabPage1.Controls.Add(Me.Label3)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.TabPage1.Size = New System.Drawing.Size(566, 245)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Settings"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'tbSkyGoImageCacheDays
        '
        Me.tbSkyGoImageCacheDays.Location = New System.Drawing.Point(228, 159)
        Me.tbSkyGoImageCacheDays.Maximum = New Decimal(New Integer() {120, 0, 0, 0})
        Me.tbSkyGoImageCacheDays.Name = "tbSkyGoImageCacheDays"
        Me.tbSkyGoImageCacheDays.Size = New System.Drawing.Size(153, 20)
        Me.tbSkyGoImageCacheDays.TabIndex = 9
        Me.tbSkyGoImageCacheDays.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(16, 166)
        Me.Label7.Name = "Label7"
        Me.Label7.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label7.Size = New System.Drawing.Size(100, 13)
        Me.Label7.TabIndex = 8
        Me.Label7.Text = "Image cache (days)"
        '
        'tbSkyGoTimeout
        '
        Me.tbSkyGoTimeout.Location = New System.Drawing.Point(228, 136)
        Me.tbSkyGoTimeout.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.tbSkyGoTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.tbSkyGoTimeout.Name = "tbSkyGoTimeout"
        Me.tbSkyGoTimeout.Size = New System.Drawing.Size(153, 20)
        Me.tbSkyGoTimeout.TabIndex = 7
        Me.tbSkyGoTimeout.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(16, 136)
        Me.Label6.Name = "Label6"
        Me.Label6.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label6.Size = New System.Drawing.Size(71, 13)
        Me.Label6.TabIndex = 6
        Me.Label6.Text = "Timeout (sek)"
        '
        'tbSkyGoFSKPIN
        '
        Me.tbSkyGoFSKPIN.Location = New System.Drawing.Point(228, 91)
        Me.tbSkyGoFSKPIN.Name = "tbSkyGoFSKPIN"
        Me.tbSkyGoFSKPIN.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.tbSkyGoFSKPIN.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.tbSkyGoFSKPIN.Size = New System.Drawing.Size(153, 20)
        Me.tbSkyGoFSKPIN.TabIndex = 3
        Me.tbSkyGoFSKPIN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'tbSkyGoPIN
        '
        Me.tbSkyGoPIN.Location = New System.Drawing.Point(228, 54)
        Me.tbSkyGoPIN.Name = "tbSkyGoPIN"
        Me.tbSkyGoPIN.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.tbSkyGoPIN.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.tbSkyGoPIN.Size = New System.Drawing.Size(153, 20)
        Me.tbSkyGoPIN.TabIndex = 2
        Me.tbSkyGoPIN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(16, 93)
        Me.Label5.Name = "Label5"
        Me.Label5.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label5.Size = New System.Drawing.Size(89, 13)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "FSKPIN (4-stellig)"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 56)
        Me.Label4.Name = "Label4"
        Me.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label4.Size = New System.Drawing.Size(69, 13)
        Me.Label4.TabIndex = 2
        Me.Label4.Text = "PIN (4-stellig)"
        '
        'tbSkyGoUserID
        '
        Me.tbSkyGoUserID.Location = New System.Drawing.Point(228, 19)
        Me.tbSkyGoUserID.Name = "tbSkyGoUserID"
        Me.tbSkyGoUserID.Size = New System.Drawing.Size(153, 20)
        Me.tbSkyGoUserID.TabIndex = 1
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(16, 22)
        Me.Label3.Name = "Label3"
        Me.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label3.Size = New System.Drawing.Size(162, 13)
        Me.Label3.TabIndex = 0
        Me.Label3.Text = "Kundennummer / E-Mail-Adresse"
        '
        'Menue
        '
        Me.Menue.Controls.Add(Me.TabPage1)
        Me.Menue.Controls.Add(Me.TabPage2)
        Me.Menue.Controls.Add(Me.TabPage3)
        Me.Menue.Location = New System.Drawing.Point(12, 2)
        Me.Menue.Name = "Menue"
        Me.Menue.SelectedIndex = 0
        Me.Menue.Size = New System.Drawing.Size(574, 271)
        Me.Menue.TabIndex = 1
        '
        'frmConfiguration
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(599, 307)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.Menue)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmConfiguration"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SkyGo"
        Me.TopMost = True
        Me.TabPage3.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        CType(Me.tbSkyGoImageCacheDays, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbSkyGoTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Menue.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents tbSkyGoImageCacheDays As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents tbSkyGoTimeout As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents tbSkyGoFSKPIN As System.Windows.Forms.TextBox
    Friend WithEvents tbSkyGoPIN As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents tbSkyGoUserID As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Menue As System.Windows.Forms.TabControl
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents cbDisableMenue_Lifestyle As System.Windows.Forms.CheckBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents cbDisableMenue_Snap As System.Windows.Forms.CheckBox
    Friend WithEvents cbDisableMenue_LiveChannels As System.Windows.Forms.CheckBox
End Class



