Imports Microsoft.SilverlightMediaFramework.Plugins.Primitives
Imports Microsoft.SilverlightMediaFramework.Core.Media
Imports Microsoft.SilverlightMediaFramework.Core
Imports Microsoft.SilverlightMediaFramework.Plugins
Imports System.Windows.Media
Imports System.Windows.Threading
Imports System.Windows.Interop
Imports System.IO
Imports System.Text
Imports System.Windows.Media.Imaging
Imports SLPlayer.PlayerPage
Imports System.Threading
Imports SLPlayer.NativeMethods
Imports System.Linq

Partial Public Class PlayerPage
    Inherits UserControl

    Public Shared StartupArgs As New StructStartupArgs
    Private WithEvents _adaptivePlugin As IAdaptiveMediaPlugin
    Friend MessageTimer As New DispatcherTimer
    Dim maxVideoBitrate As Long
    Dim minVideoBitrate As Long
    Dim videoBitrateList As List(Of Long) = New List(Of Long)
    Dim maxBitrateLimiterControl As MaxBitrateLimiterControl
    Dim bitrateGraphControl As BitrateGraphControl

    Public Structure StructStartupArgs
        Dim url_Manifest As String
        Dim filePath_backdrop As String
        Dim args As List(Of String)
    End Structure
    Public Structure StructResolution
        Dim Height As Integer
        Dim Width As Integer
        Dim isStandard As Boolean
    End Structure
    Public Sub New()
        InitializeComponent()
    End Sub

    Friend Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs) '1.
        StartupArgs = GetStartupArgsFile("StartupArgs.cfg")
        setSLPlayerBackdropImage(StartupArgs.filePath_backdrop)
        preventSleep(True)
        If Not String.IsNullOrWhiteSpace(StartupArgs.url_Manifest) Then
            AddNewPlayItem(StartupArgs.url_Manifest)
        Else
            Message2Gui("ungültige Manifest-URL aus StartupArgs.cfg", "SL Player Load")
        End If

    End Sub

    Private Sub AddNewPlayItem(ByVal ItemUri As String) '2.
        If Not String.IsNullOrEmpty(ItemUri) Then
            'Create a new PlayList Item
            Dim item As PlaylistItem = New PlaylistItem()

            ' Test a full fledged manual acquirer
            ' Set the LicenseAcquirer of the MediaElement to the custom License Acquirer defined
            Try
                item.MediaSource = New Uri(ItemUri)
                item.LicenseAcquirer = New ManualLicenseAcquirer(Me)
                item.LicenseAcquirer.ChallengeCustomData = String.Join("&", StartupArgs.args.ToArray)
                item.ChunkDownloadStrategy = ChunkDownloadStrategy.AsNeeded
                item.FrameRate = 50
                item.VideoHeight = 576
                item.VideoWidth = 720
                item.VideoStretchMode = Stretch.Uniform
                item.DeliveryMethod = Microsoft.SilverlightMediaFramework.Plugins.Primitives.DeliveryMethods.AdaptiveStreaming
            Catch ex As Exception
                Message2Gui(ex.Message.ToString, "AddNewPlayItem")
            End Try

            'Add PlaylistItem to the Media playlist
            sMFPlayer.ChunkDownloadStrategy = ChunkDownloadStrategy.AsNeeded
            sMFPlayer.AllowFullScreenPinning = True

            sMFPlayer.MediaTitleContent = "SkyGoDe"
            '   sMFPlayer.Playlist.Clear()
            sMFPlayer.Playlist.Add(item)
            sMFPlayer.AutoPlay = True

        End If
    End Sub
    Private Sub sMFPlayer_MediaPluginRegistered(sender As Object, e As CustomEventArgs(Of IMediaPlugin)) Handles sMFPlayer.MediaPluginRegistered '3.
        _adaptivePlugin = DirectCast(e.Value, IAdaptiveMediaPlugin)
        _adaptivePlugin.ChunkDownloadStrategy = ChunkDownloadStrategy.AsNeeded

        AddHandler _adaptivePlugin.PluginLoaded, AddressOf Me.MediaPlugin_OnPluginLoadad
        AddHandler _adaptivePlugin.PluginLoadFailed, AddressOf Me.MediaPlugin_OnPluginLoadFailed

    End Sub

    Private Sub MediaPlugin_OnPluginLoadFailed(obj As IPlugin, e As Exception)
        Call Message2Gui("Fehler beim laden des Plugin's", "OnPluginLoadFailed")
    End Sub

    Private Sub MediaPlugin_OnPluginLoadad(obj As IPlugin) '4. wird nach PluginLoaded aufgerufen
        'https://smf.codeplex.com/discussions/355646
        Dim mediaPlugin As IMediaPlugin = CType(obj, IMediaPlugin)

        If mediaPlugin Is Nothing Then
            Message2Gui("Fehler bei MediaPlugin", "OnPluginLoadad")
        Else
            mediaPlugin.BufferingTime = New TimeSpan(0, 0, 2)
        End If
    End Sub

    Private Sub adaptivePlugin_OnManifestReady(sender As Object) Handles _adaptivePlugin.ManifestReady '5.
        Dim VideoSize As Size

        For Each segment As ISegment In _adaptivePlugin.Segments
            Dim streams As List(Of IMediaStream) = segment.AvailableStreams
            For Each stream As IMediaStream In streams
                If stream.Type = StreamType.Video Then
                    Dim videoTracks As List(Of IMediaTrack) = stream.AvailableTracks

                    For Each videoTrack As IMediaTrack In videoTracks
                        If videoTrack.Bitrate > maxVideoBitrate Then
                            videoBitrateList.Add(videoTrack.Bitrate)
                            VideoSize = videoTrack.Resolution
                        End If
                    Next
                    '  ElseIf stream.Type = StreamType.Audio Then
                    ' Dim audioTracks As List(Of IMediaTrack) = stream.AvailableTracks
                    '  For Each audioTrack As IMediaTrack In audioTracks
                    '   Dim lang As String = audioTrack.Language
                    '  Dim lan As String = audioTrack.Language
                    '  _adaptivePlugin.DownloadStreamData(audioTrack)
                    '     Dim lang As String = audioTrack.ParentStream.Language
                    '   If lang = "eng" Then
                    'Dim xx As List(Of StreamMetadata) = sMFPlayer.AvailableAudioStreams
                    '  Stop
                    '  End If
                    '  Next
                End If
            Next
        Next

        If videoBitrateList.Count > 2 Then
            minVideoBitrate = videoBitrateList(videoBitrateList.Count - 2)
            maxVideoBitrate = videoBitrateList(videoBitrateList.Count - 1)
        End If

        If minVideoBitrate = 0 Or maxVideoBitrate = 0 Then
            minVideoBitrate = 162000
            maxVideoBitrate = 2430000
        End If

        _adaptivePlugin.EnableGPUAcceleration = True
        _adaptivePlugin.SetVideoBitrateRange(minVideoBitrate, maxVideoBitrate, True)

        sMFPlayer.PlayerGraphVisibility = FeatureVisibility.Visible

        'unten links
        bitrateGraphControl = New BitrateGraphControl
        bitrateGraphControl.AvailableBitrates = videoBitrateList
        bitrateGraphControl.MaximumPlayableBitrate = maxVideoBitrate

        'oben rechts
        maxBitrateLimiterControl = New MaxBitrateLimiterControl
        maxBitrateLimiterControl.AvailableBitrates = videoBitrateList
        maxBitrateLimiterControl.DownloadBitrate = maxVideoBitrate
        maxBitrateLimiterControl.UpdateLayout()

        sMFPlayer.PlayerGraphVisibility = FeatureVisibility.Disabled
    End Sub

    Private Sub preventSleep(ByVal bol As Boolean)
        'force no sleep and no display turn off
        If bol Then
            NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED Or EXECUTION_STATE.ES_CONTINUOUS Or EXECUTION_STATE.ES_DISPLAY_REQUIRED)
        Else
            NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED)
        End If
    End Sub

    Private Sub sMFPlayer_MediaOpened(sender As Object, e As EventArgs) Handles sMFPlayer.MediaOpened '7.
        Backdrop.Visibility = Windows.Visibility.Collapsed
        Backdrop.Source = Nothing
        sMFPlayer.Visibility = Windows.Visibility.Visible
        _adaptivePlugin.EnableGPUAcceleration = True

        If Not bitrateGraphControl Is Nothing Then
            bitrateGraphControl.MaximumPlayableBitrate = sMFPlayer.MaximumPlaybackBitrate
        End If
    End Sub

    Private Sub sMFPlayer_MediaFailed(ByVal sender As Object, ByVal e As CustomEventArgs(Of System.Exception)) Handles sMFPlayer.MediaFailed
        sMFPlayer.Visibility = Windows.Visibility.Visible

        Dim errorMessage As String = ""

        If Not e.Value Is Nothing Then
            If e.Value.Message.ToString.Contains("6007 ") Then
                errorMessage = "Session timed out or user already logged in."
            Else
                errorMessage = e.Value.Message.ToString
            End If
        Else
            errorMessage = "Fehler beim öffnen des Mediums"
        End If

        If Not String.IsNullOrWhiteSpace(errorMessage) Then
            Call Message2Gui(errorMessage, "MediaFailed")
        End If

    End Sub

    Private Sub sMFPlayer_MediaEnded(sender As Object, e As EventArgs) Handles sMFPlayer.MediaEnded
        Call close_Player()
    End Sub

    Friend Sub close_Player()
        preventSleep(False)
        sMFPlayer.Stop()
        sMFPlayer.Dispose()
        App.Current.MainWindow.Close()
    End Sub

    Private Sub sMFPlayer_KeyDown(sender As Object, e As KeyEventArgs) Handles sMFPlayer.KeyDown
        System.Diagnostics.Debug.WriteLine("SLPlayer KeyDown Key:" & e.Key.ToString & " PlatformKeyCode:" & e.PlatformKeyCode)
        Select Case e.Key
            Case Key.None
                Exit Select
            Case Key.F4, Key.Escape, Key.Back, Key.Unknown And e.PlatformKeyCode = 178 'close player
                Call close_Player()
                Exit Select
            Case Key.M, Key.Unknown And e.PlatformKeyCode = 173 'mute
                sMFPlayer.IsMuted = Not sMFPlayer.IsMuted
            Case Key.P, Key.Space, Key.Unknown And (e.PlatformKeyCode = 250 Or e.PlatformKeyCode = 19 Or e.PlatformKeyCode = 179 Or e.PlatformKeyCode = 166) 'playPause
                Call playPause_Player()
            Case Key.D0, Key.Unknown And e.PlatformKeyCode = 175 'VolumeUp
                If sMFPlayer.IsMuted Then
                    sMFPlayer.IsMuted = False
                End If
                sMFPlayer.VolumeLevel = sMFPlayer.VolumeLevel + 0.1
            Case Key.Subtract, Key.Unknown And e.PlatformKeyCode = 174 'VolumeDown
                If sMFPlayer.IsMuted Then
                    sMFPlayer.IsMuted = False
                End If
                sMFPlayer.VolumeLevel = sMFPlayer.VolumeLevel - 0.1
            Case Key.B, Key.F5 'rewind back
                sMFPlayer.StartRewind()
            Case Key.F, Key.F6 'Forward
                sMFPlayer.StartFastForward()
                If sMFPlayer.PlaySpeedState = PlaySpeedState.FastForwarding Then
                    sMFPlayer.StartFastForward()
                End If
            Case Key.F10 'slowMow
                If sMFPlayer.PlaySpeedState = PlaySpeedState.SlowMotion Then
                    sMFPlayer.StopSlowMotion()
                Else
                    sMFPlayer.StartSlowMotion()
                End If
            Case Key.Right, Key.PageUp, Key.Unknown And e.PlatformKeyCode = 176  'step forward MediaNextTrack
                Call stepForward_Player(1)
            Case Key.Left, Key.PageDown, Key.Unknown And e.PlatformKeyCode = 177  'step backward MediaPreviousTrack
                Call stepForward_Player(-1)
            Case Key.Up 'Big step forward
                Call stepForward_Player(5)
            Case Key.Down 'Big step backward
                Call stepForward_Player(-5)
            Case Key.C, Key.Unknown And e.PlatformKeyCode = 91 'show/hide ControlStrip
                sMFPlayer.IsControlStripVisible = Not sMFPlayer.IsControlStripVisible
            Case Key.S, Key.NumPad2
                If Not sMFPlayer.CurrentPlaylistItem Is Nothing Then
                    Select Case _adaptivePlugin.Stretch
                        Case Stretch.Fill
                            _adaptivePlugin.Stretch = Stretch.None
                        Case Stretch.None
                            _adaptivePlugin.Stretch = Stretch.Uniform
                        Case Stretch.Uniform
                            Dim targetResolution As StructResolution = getResolution(_adaptivePlugin.NaturalVideoSize.Width, _adaptivePlugin.NaturalVideoSize.Height)

                            If Not targetResolution.isStandard Then
                                Dim multiplicator As Double = Math.Round(App.Current.Host.Content.ActualWidth) / targetResolution.Width '  '720 = 2,6666
                                Dim displayHeight As Double = Math.Round(targetResolution.Height * multiplicator) '   540 *2,6 = 1404
                                Dim displayWidth As Double = Math.Round(targetResolution.Width * multiplicator)

                                Dim realDisplayHeight As Double = targetResolution.Height * multiplicator ' 540 * 2 = 1080

                                Dim newResolution As StructResolution
                                newResolution = getResolution(displayWidth, displayHeight)

                                If Not newResolution.isStandard Then
                                    realDisplayHeight = targetResolution.Height * Math.Floor(multiplicator)
                                    Dim margTopDiff As Integer = (displayHeight - realDisplayHeight) / 2

                                    sMFPlayer.Margin = New Thickness(0, -margTopDiff, 0, 0)
                                End If
                            End If

                            _adaptivePlugin.Stretch = Stretch.UniformToFill

                        Case Stretch.UniformToFill
                            sMFPlayer.Margin = New Thickness(0)
                            _adaptivePlugin.Stretch = Stretch.Fill

                        Case Else
                            Stop
                    End Select
                End If
            Case Key.A, Key.NumPad1 'change audio language
                If sMFPlayer.AvailableAudioStreams.Count > 1 Then
                    Dim availableAudioStreams As List(Of StreamMetadata) = sMFPlayer.AvailableAudioStreams
                    Dim selectedAudioStream As StreamMetadata = sMFPlayer.SelectedAudioStream
                    For i = 0 To sMFPlayer.AvailableAudioStreams.Count - 1
                        If sMFPlayer.SelectedAudioStream.Id = sMFPlayer.AvailableAudioStreams(i).Id Then
                            If i < sMFPlayer.AvailableAudioStreams.Count - 1 Then
                                sMFPlayer.SelectedAudioStream = sMFPlayer.AvailableAudioStreams(i + 1)
                                Call Message2Gui("Audio: -> " & sMFPlayer.AvailableAudioStreams(i + 1).Language, "Audio Language", False)
                                Exit For
                            Else
                                sMFPlayer.SelectedAudioStream = sMFPlayer.AvailableAudioStreams(0)
                                Call Message2Gui("Audio: -> " & sMFPlayer.AvailableAudioStreams(0).Language, "Audio Language", False)
                                Exit For
                            End If
                        End If
                    Next
                End If

            Case Key.NumPad9
                Application.Current.Host.Content.IsFullScreen = Not Application.Current.Host.Content.IsFullScreen

            Case Key.F12
                If Not sMFPlayer.PlayerGraphVisibility = FeatureVisibility.Visible Then
                    sMFPlayer.PlayerGraphVisibility = FeatureVisibility.Visible
                Else
                    sMFPlayer.PlayerGraphVisibility = FeatureVisibility.Disabled
                End If
        End Select
    End Sub

    Private Function getResolution(ByVal width As Long, ByVal height As Long) As StructResolution
        Dim res As New StructResolution

        Select Case width
            Case Is < 480
                res.Width = 353
                res.Height = 576
            Case Is < 544
                res.Width = 480
                res.Height = 576
            Case Is < 720
                res.Width = 544
                res.Height = 576
            Case Is < 768
                res.Width = 720
                res.Height = 576
            Case Is < 960
                res.Width = 768
                res.Height = 576
            Case Is < 1024
                res.Width = 960
                res.Height = 720
            Case Is < 1280
                res.Width = 1024
                res.Height = 576
            Case Is < 1440
                res.Width = 1280
                res.Height = 720
            Case Is < 1920
                res.Width = 1440
                res.Height = 1080
            Case Else
                res.Width = 1920
                res.Height = 1080
        End Select

        res.isStandard = False

        If res.Width = width Then
            If res.Height = height Then
                res.isStandard = True
            End If
        End If

        Return res
    End Function

    Private Sub stepForward_Player(ByVal minutes As Integer)
        Dim curPosition As TimeSpan = sMFPlayer.PlaybackPosition
        Dim ts As TimeSpan = New TimeSpan(0, 0, minutes, 0, 0)
        Dim newPosition As TimeSpan

        newPosition = curPosition.Add(ts)
        If newPosition < TimeSpan.Zero Then
            newPosition = New TimeSpan(0, 0, 0, 0, 0)
        End If
        sMFPlayer.SeekToPosition(newPosition)
    End Sub

    Private Sub playPause_Player()
        If sMFPlayer.PlayState = MediaPluginState.Playing Or Not sMFPlayer.PlayState = MediaPluginState.Paused Then
            sMFPlayer.Pause()
        Else
            sMFPlayer.Play()
        End If
    End Sub

    Private Function GetStartupArgsFile(ByVal filename As String) As StructStartupArgs
        Dim contents As String = Nothing

        If (Application.Current.HasElevatedPermissions) Then
            Dim myDocuments As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Dim filePath_StartupArgs As String = Path.Combine(Path.GetTempPath(), filename)
            If Not File.Exists(filePath_StartupArgs) Then
                filePath_StartupArgs = Path.Combine(Path.GetDirectoryName(Application.Current.Host.Source.LocalPath), filename)
            End If

            If File.Exists(filePath_StartupArgs) Then
                Dim strline As String = ""

                StartupArgs.args = New List(Of String)

                Dim sr As StreamReader = New StreamReader(filePath_StartupArgs, Encoding.UTF8)
                While Not (sr.EndOfStream)
                    strline = sr.ReadLine()
                    Dim items() As String = strline.Split("=")

                    If items.Length = 2 Then
                        If items(0).ToString = "url_Manifest" Then
                            StartupArgs.url_Manifest = items(1).ToString
                        ElseIf items(0).ToString = "filePath_backdrop" Then
                            StartupArgs.filePath_backdrop = items(1).ToString
                        Else
                            StartupArgs.args.Add(strline)
                        End If
                    End If
                End While
            Else
                Call Message2Gui("Datei 'StartupArgs.cfg' nicht gefunden." & Environment.NewLine & filePath_StartupArgs, "Error")
            End If
        Else
            Call Message2Gui("fehlende Berechtigung für Silverlight -> ElevatedPermissions", "Error")
        End If

        Return StartupArgs
    End Function

    Private Sub setSLPlayerBackdropImage(ByVal filePath_backdrop As String)
        Dim ret As Boolean
        Backdrop.Visibility = Windows.Visibility.Visible

        If Not String.IsNullOrEmpty(filePath_backdrop) Then
            If (Application.Current.HasElevatedPermissions) Then
                'pgn and jpg only
                If File.Exists(filePath_backdrop) Then
                    Backdrop.Source = New BitmapImage(New Uri(New Uri(filePath_backdrop).AbsoluteUri))
                    ret = True
                End If
            End If
        End If

        'fallback
        If Not ret Then
            filePath_backdrop = "Images/sky_go_05.png"
            Backdrop.Source = New BitmapImage(New Uri(filePath_backdrop, UriKind.Relative))
        End If
    End Sub

    Friend Sub Message2Gui(ByVal msgText As String, ByVal title As String, Optional closePlayer As Boolean = True)
        MessageTimer.Interval = TimeSpan.FromSeconds(5)
        MessageTimer.Start()

        TextGrid.Visibility = Windows.Visibility.Visible
        TextMessage.Text = title & Environment.NewLine & msgText
        TextMessage.Visibility = Windows.Visibility.Visible

        If closePlayer Then
            AddHandler MessageTimer.Tick, AddressOf Me.timerStop_closePlayer
        Else
            AddHandler MessageTimer.Tick, AddressOf Me.hide_TextMessage
        End If

    End Sub

    Private Sub hide_TextMessage(ByVal sender As Object, ByVal e As EventArgs)
        TextGrid.Visibility = Windows.Visibility.Collapsed
        TextMessage.Text = ""
        TextMessage.Visibility = Windows.Visibility.Collapsed
        MessageTimer.Stop()
        RemoveHandler MessageTimer.Tick, AddressOf Me.hide_TextMessage
    End Sub

    Private Sub timerStop_closePlayer(ByVal sender As Object, ByVal e As EventArgs)
        MessageTimer.Stop()
        RemoveHandler MessageTimer.Tick, AddressOf Me.timerStop_closePlayer
        close_Player()
    End Sub

End Class






