Imports SkyGoDe
Imports SkyGoDe.PluginConfiguration
Imports System.Windows.Forms
Imports System.IO

Public Class frmConfiguration
    Private Sub Configuration_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Icon = My.Resources.SkyGo_icon

        'check if folders exists or create them
        Directory.CreateDirectory(folder_Thumbs_Plugin)
        Directory.CreateDirectory(folder_Thumbs_Plugin_Cache)
        Directory.CreateDirectory(folder_Thumbs_Plugin_Cache_SkyGo)
        Directory.CreateDirectory(folder_Thumbs_Plugin_Cache_SkyGo_Covers)
        Directory.CreateDirectory(folder_Thumbs_Plugin_Cache_SkyGo_Backdrops)

        userSetting = loadSettings()

        If userSetting.Success Then
            Me.tbSkyGoUserID.Text = userSetting.PlugIn_SkyGoDe_UserID
            Me.tbSkyGoPIN.Text = userSetting.PlugIn_SkyGoDe_PIN
            Me.tbSkyGoFSKPIN.Text = userSetting.PlugIn_SkyGoDe_FSKPIN
            Me.tbSkyGoTimeout.Text = userSetting.PlugIn_SkyGoDe_Timeout.ToString
            Me.tbSkyGoImageCacheDays.Text = userSetting.PlugIn_SkyGoDe_ImageCacheDays.ToString

            Me.cbDisableMenue_Lifestyle.Checked = userSetting.PlugIn_SkyGoDe_DisableMenue_Lifestyle
            Me.cbDisableMenue_LiveChannels.Checked = userSetting.PlugIn_SkyGoDe_DisableMenue_LiveChannels
            Me.cbDisableMenue_Snap.Checked = userSetting.PlugIn_SkyGoDe_DisableMenue_Snap

        End If

    End Sub

    Private Function checkSettings() As Boolean
        If Me.tbSkyGoUserID.Text.Length < 4 Then
            MessageBox.Show("Bitte die Sky Kundennummer eingeben.", "UserID", MessageBoxButtons.OK)
            Return False
        End If

        If Me.tbSkyGoPIN.Text.Length <> 4 Then
            MessageBox.Show("Bitte die PIN für SKY eingeben.", "PIN", MessageBoxButtons.OK)
            Return False
        End If

        If Me.tbSkyGoFSKPIN.Text.Length <> 4 Then
            MessageBox.Show("Bitte die FSKPIN für SKY eingeben.", "FSKPIN", MessageBoxButtons.OK)
            Return False
        End If

        If String.IsNullOrEmpty(Me.tbSkyGoTimeout.Text) Then
            MessageBox.Show("Bitte ein Timeout für SKY eingeben.", "Timeout", MessageBoxButtons.OK)
            Return False
        End If

        If String.IsNullOrEmpty(Me.tbSkyGoImageCacheDays.Text) Then
            MessageBox.Show("Bitte einen Wert bei 'image cache' eingeben.", "image cache", MessageBoxButtons.OK)
            Return False
        End If

        Return True
    End Function

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If checkSettings() Then
            userSetting.PlugIn_SkyGoDe_UserID = Me.tbSkyGoUserID.Text
            userSetting.PlugIn_SkyGoDe_PIN = Me.tbSkyGoPIN.Text
            userSetting.PlugIn_SkyGoDe_FSKPIN = Me.tbSkyGoFSKPIN.Text
            userSetting.PlugIn_SkyGoDe_Timeout = CInt(Me.tbSkyGoTimeout.Text)
            userSetting.PlugIn_SkyGoDe_ImageCacheDays = CInt(Me.tbSkyGoImageCacheDays.Text)

            'Tab Menue
            userSetting.PlugIn_SkyGoDe_DisableMenue_Lifestyle = Me.cbDisableMenue_Lifestyle.Checked
            userSetting.PlugIn_SkyGoDe_DisableMenue_LiveChannels = Me.cbDisableMenue_LiveChannels.Checked
            userSetting.PlugIn_SkyGoDe_DisableMenue_Snap = Me.cbDisableMenue_Snap.Checked

            SaveSettings(userSetting)
            Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub


End Class





