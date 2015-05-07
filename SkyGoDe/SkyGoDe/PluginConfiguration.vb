Imports MediaPortal.GUI.Library
Imports System.ComponentModel
Imports MediaPortal.Configuration
Imports System.Collections.Concurrent
Imports System.IO
Imports System.Threading

Namespace SkyGoDe
    Public Class PluginConfiguration

#Region "global var's"
        'default's
        Friend Const Plugin_Name As String = "Skygo.de"
        Friend Const cfg_SectionName As String = "SkyGoDe"
        Friend Const WindowId As Integer = 14755

        'local path's
        Friend Shared folder_Thumbs As String = Config.GetFolder(Config.Dir.Thumbs) & "\"
        Friend Shared folder_Thumbs_Plugin As String = folder_Thumbs & "SkyGoDe\"
        Friend Shared folder_Thumbs_Plugin_logoFile As String = folder_Thumbs_Plugin & "logo.png"
        Friend Shared folder_Thumbs_Plugin_defaultFile As String = folder_Thumbs_Plugin & "hover_SkyGoDe.png" 'default.jpg"
        Friend Shared folder_Thumbs_Plugin_BackgroundImage As String = folder_Thumbs_Plugin & "Backgrounds\"
        Friend Shared folder_Thumbs_Plugin_Icons As String = folder_Thumbs_Plugin & "Icons\"
        Friend Shared folder_Thumbs_Plugin_Icons_Channels As String = folder_Thumbs_Plugin_Icons & "channels\"
        Friend Shared folder_Thumbs_Plugin_Cache As String = folder_Thumbs_Plugin & "Cache\"
        Friend Shared folder_Thumbs_Plugin_Cache_SkyGo As String = folder_Thumbs_Plugin_Cache & "SkyGoDe\"
        Friend Shared folder_Thumbs_Plugin_Cache_SkyGo_Covers As String = folder_Thumbs_Plugin_Cache_SkyGo & "Covers\"
        Friend Shared folder_Thumbs_Plugin_Cache_SkyGo_Backdrops As String = folder_Thumbs_Plugin_Cache_SkyGo & "Backdrops\"
        Friend Shared folder_Plugin_Windows As String = Config.GetFolder(Config.Dir.Plugins) & "\Windows\"
        Friend Shared folder_Plugin_Windows_SkyGoDe As String = folder_Plugin_Windows & "SkyGoDe\"
        Friend Shared folder_Plugin_Windows_SkyGoDe_SLPlayerFile As String = folder_Plugin_Windows_SkyGoDe & "SLPlayer.xap"
        Friend Shared folder_ProgramFiles_sllauncherFile As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) & "\Microsoft Silverlight\", "sllauncher.exe")
        Friend Shared filePath_StartupArgs As String = Path.Combine(Path.GetTempPath(), "StartupArgs.cfg")
        Friend Shared filePath_ChannelList As String = Path.Combine(folder_Plugin_Windows_SkyGoDe, "ChannelList.json")

        'web vars
        Friend Const url_skygode As String = "http://www.skygo.sky.de/"
        Friend Const url_skygode_channellist As String = url_skygode & "epgd/sg/web/channelList"
        Friend Const url_skygode_eventList As String = url_skygode & "epgd/sg/web/eventList/"
        Friend Const url_skygode_eventDetail As String = url_skygode & "epgd/sg/ipad/eventDetail/"
        Friend Const url_skygode_json As String = url_skygode & "[SkyGoOrSnap]/multiplatform/ipad/json/"
        Friend Const url_skygode_json_automaticlisting As String = url_skygode_json & "automatic_listing/"
        Friend Const url_skygode_json_detailsSeries As String = url_skygode_json & "details/series/"
        Friend Const url_skygode_json_detailsAsset As String = url_skygode_json & "details/asset/"
        Friend Const UserAgent As String = "Dalvik/2.1.0 (Linux; U; Android 5.0; Nexus 7 Build/LRX21P)"

        'important stores
        Friend Shared pageStore As New Dictionary(Of String, Page)
        Friend Shared backgroundImagePathStore As New List(Of String)
        Friend Shared ListOfExistingCovers As New Dictionary(Of String, String)
        Friend Shared ListOfExistingBackdrops As New Dictionary(Of String, String)
        Friend Shared localChannelList As New Dictionary(Of String, JSONClasses.JSONChannelListClass.ChannelList)
        Friend Shared factory As New Threading.Tasks.TaskFactory()
        Friend Shared tasks As New ConcurrentBag(Of Threading.Tasks.Task)()
        Friend Shared cts As New CancellationTokenSource
#End Region

#Region "MediaPortal.xml attribute names"
        Public Shared userSetting As New UserSettings
        Public Shared _SLPlayerStartupArgs As New SLPlayerStartupArgs

        Public Class UserSettings
            Friend MP_alwaysontop As Boolean
            Friend MP_ThreadPriority As ProcessPriorityClass
            Friend PlugIn_SkyGoDe_UserID As String
            Friend PlugIn_SkyGoDe_PIN As String
            Friend PlugIn_SkyGoDe_FSKPIN As String
            Friend PlugIn_SkyGoDe_Timeout As Integer
            Friend PlugIn_SkyGoDe_ImageCacheDays As Integer = 31

            Friend PlugIn_SkyGoDe_DisableMenue_Lifestyle As Boolean
            Friend PlugIn_SkyGoDe_DisableMenue_LiveChannels As Boolean
            Friend PlugIn_SkyGoDe_DisableMenue_Snap As Boolean

            Friend Success As Boolean
        End Class

        Public Enum ResponseCode
            Init = -1
            Ok = 0
            NotFound = 100
            [Error] = 99
        End Enum

        Public Class ResponseStruct
            Friend [string] As String
            Friend category As Category
            Friend returnCode As ResponseCode = ResponseCode.Init
        End Class

        Public Class SLPlayerStartupArgs
            Friend sessionId As String
            Friend Success As Boolean
        End Class

        Public Class ImagePaths
            ' list items (DVD_cover_small)
            Friend coverSmall_url As String = ""
            Friend coverSmall_localPath As String = ""
            ' cover (DVD_cover_big)
            Friend cover_big_url As String = ""
            Friend cover_big_localPath As String = ""
            'backdrop
            Friend backgroundImage_url As String = ""
            Friend backgroundImage_localPath As String = ""
        End Class

        Friend Shared Function loadSettings() As UserSettings

            Try
                Using xmlReader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml"))
                    Dim encryptedPIN As String
                    Dim encryptedFSKPIN As String

                    Dim alwaysontop As String = xmlReader.GetValueAsString("general", "alwaysontop", "no")
                    If alwaysontop.ToLower = "yes" Then
                        userSetting.MP_alwaysontop = True
                    Else
                        userSetting.MP_alwaysontop = False
                    End If

                    Dim threadPriority As String = xmlReader.GetValueAsString("general", "ThreadPriority", "Normal")

                    Select Case threadPriority
                        Case "AboveNormal"
                            userSetting.MP_ThreadPriority = ProcessPriorityClass.AboveNormal
                        Case "BelowNormal"
                            userSetting.MP_ThreadPriority = ProcessPriorityClass.BelowNormal
                        Case "High"
                            userSetting.MP_ThreadPriority = ProcessPriorityClass.High
                        Case Else
                            userSetting.MP_ThreadPriority = ProcessPriorityClass.Normal
                    End Select


                    'read settings from xml 
                    userSetting.PlugIn_SkyGoDe_UserID = xmlReader.GetValueAsString(PluginConfiguration.cfg_SectionName, "SkyGoDe_UserID", "0123456789")
                    userSetting.PlugIn_SkyGoDe_Timeout = xmlReader.GetValueAsInt(PluginConfiguration.cfg_SectionName, "SkyGoDe_Timeout", 5)
                    userSetting.PlugIn_SkyGoDe_ImageCacheDays = xmlReader.GetValueAsInt(PluginConfiguration.cfg_SectionName, "SkyGoDe_ImageCacheDays", 60)

                    'Menue
                    userSetting.PlugIn_SkyGoDe_DisableMenue_Lifestyle = xmlReader.GetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_Lifestyle", False)
                    userSetting.PlugIn_SkyGoDe_DisableMenue_LiveChannels = xmlReader.GetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_LiveChannels", False)
                    userSetting.PlugIn_SkyGoDe_DisableMenue_Snap = xmlReader.GetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_Snap", False)

                    encryptedPIN = xmlReader.GetValueAsString(PluginConfiguration.cfg_SectionName, "SkyGoDe_PIN", String.Empty)
                    encryptedFSKPIN = xmlReader.GetValueAsString(PluginConfiguration.cfg_SectionName, "SkyGoDe_FSKPIN", String.Empty)

                    If Not encryptedPIN Is String.Empty Then
                        userSetting.PlugIn_SkyGoDe_PIN = decryptString(encryptedPIN)
                    End If

                    If Not encryptedFSKPIN Is String.Empty Then
                        userSetting.PlugIn_SkyGoDe_FSKPIN = decryptString(encryptedFSKPIN)
                    End If
                End Using

                userSetting.Success = True
            Catch ex As Exception
                userSetting.Success = False
                Log.Error("SkyGoDe: loadSettings Error " & ex.Message)
            End Try

            Return userSetting
        End Function

        Friend Shared Function SaveSettings(ByVal userSetting As UserSettings) As UserSettings

            Using xmlReader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml"))
                Dim encryptedPIN As String
                xmlReader.SetValue(PluginConfiguration.cfg_SectionName, "SkyGoDe_UserID", userSetting.PlugIn_SkyGoDe_UserID)

                ' Encrypt the password
                encryptedPIN = encryptString(userSetting.PlugIn_SkyGoDe_PIN)
                xmlReader.SetValue(PluginConfiguration.cfg_SectionName, "SkyGoDe_PIN", encryptedPIN)

                encryptedPIN = encryptString(userSetting.PlugIn_SkyGoDe_FSKPIN)
                xmlReader.SetValue(PluginConfiguration.cfg_SectionName, "SkyGoDe_FSKPIN", encryptedPIN)

                xmlReader.SetValue(PluginConfiguration.cfg_SectionName, "SkyGoDe_Timeout", userSetting.PlugIn_SkyGoDe_Timeout)
                xmlReader.SetValue(PluginConfiguration.cfg_SectionName, "SkyGoDe_ImageCacheDays", userSetting.PlugIn_SkyGoDe_ImageCacheDays)

                'Menue
                xmlReader.SetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_Lifestyle", userSetting.PlugIn_SkyGoDe_DisableMenue_Lifestyle)
                xmlReader.SetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_LiveChannels", userSetting.PlugIn_SkyGoDe_DisableMenue_LiveChannels)
                xmlReader.SetValueAsBool(PluginConfiguration.cfg_SectionName, "SkyGoDe_DisableMenue_Snap", userSetting.PlugIn_SkyGoDe_DisableMenue_Snap)

            End Using

            Return userSetting
        End Function

        Friend Shared Function encryptString(ByVal decrypted As String) As String
            Dim Crypto As EncryptDecrypt = New EncryptDecrypt()
            Dim encrypted As String = String.Empty

            Try
                encrypted = Crypto.Encrypt(decrypted)
            Catch ex As Exception
                encrypted = String.Empty
                Log.Error("SkyGoDe: Could not encrypt setting string!")
            End Try

            Return encrypted
        End Function

        Friend Shared Function decryptString(ByVal encrypted As String) As String
            Dim decrypted As String = String.Empty

            Dim Crypto As EncryptDecrypt = New EncryptDecrypt()
            Try
                decrypted = Crypto.Decrypt(encrypted)
            Catch ex As Exception
                decrypted = String.Empty '29.03.2015
                Log.Error("SkyGoDe: Could not decrypt config string!")
            End Try

            Return decrypted
        End Function
#End Region

        Public Enum Workstate
            init = 0
            working = 5
            [error] = 7
            complete = 9
        End Enum

        Public Enum State
            categories = 1
            videos = 2
            serie = 3
            livestream = 6
        End Enum

        Public Class Page
            Public name As String
            Public [alias] As String
            Public type As State
            Public currPage As Integer
            Public nextPage As Integer
            Public maxPage As Integer
            Public HasNextPage As Boolean
            Public Url As String
            Public selectedListItemIndex As Integer
            Public category As New BindingList(Of Category)
            Public state As Workstate

            Sub New()
                name = String.Empty
                currPage = 1
                nextPage = 1
                maxPage = 1
                selectedListItemIndex = -1 'no item selected at this time
                HasNextPage = False
                state = Workstate.init
                Url = String.Empty
            End Sub
        End Class

        Public Class Category
            Inherits GUIListItem
            Friend Shadows ItemId As String
            Friend countID As Integer
            Friend [Alias] As String
            Friend Url As String
            Friend Url_Manifest As String
            Friend event_id As String
            Friend apix_id As String
            Friend onClick As onClickType
            Friend descriptionType As String
            Friend description As String
            Friend backgroundImage As String
            Friend backgroundImage_url As String
            Friend product As String
            Friend ListItemIndex As Integer
            Friend nextPage As Integer
            Friend SubCategories As List(Of Category)

            Public Enum onClickType
                startMenue = 100

                'Menue
                FilmMenue = 110
                SerieMenue = 120
                DokuMenue = 130
                LifestyleMenue = 140
                SnapMenue = 150
                DokuFilmMenue = 160
                DokuSerieMenue = 170
                SnapFilmMenue = 180
                SnapSerieMenue = 190

                'Listing
                Film_Listing = 200
                Serie_Listing = 210
                Staffel_Listing = 220
                Episode_Listing = 230
                DokuFilm_Listing = 240
                DokuSerie_Listing = 250
                DokuStaffel_Listing = 260
                DokuEpisode_Listing = 270
                LiveEvent_Listing = 280
                LifestyleSerie_Listing = 290
                LifestyleStaffel_Listing = 300
                LifestyleEpisode_Listing = 310
                SnapFilm_Listing = 320
                SnapSerie_Listing = 330
                SnapStaffel_Listing = 340
                SnapEpisode_Listing = 350

                'Letter_Listing
                FilmLetter_Listing = 500
                SerieLetter_Listing = 510
                DokuFilmLetter_Listing = 520
                DokuSerieLetter_Listing = 530
                LifestyleSerieLetter_Listing = 540
                SnapFilmLetter_Listing = 550
                SnapSerieLetter_Listing = 560

                'Type of Video
                Film = 800
                DokuFilm = 810
                SnapFilm = 820
                LiveStream = 830
                Episode = 840
                DokuEpisode = 850
                LifestyleEpisode = 860
                SnapEpisode = 870
            End Enum

            Friend Sub New(ByVal itemId As String, _
                        ByVal label As String, _
                        ByVal [alias] As String, _
                        Optional ByVal onClick As onClickType = onClickType.startMenue, _
                        Optional ByVal url As String = "", _
                        Optional ByVal url_Manifest As String = "", _
                        Optional ByVal event_id As String = "", _
                        Optional ByVal apix_id As String = "", _
                        Optional ByVal iconImage As String = "", _
                        Optional ByVal thumbnailImage As String = "", _
                        Optional ByVal backgroundImage As String = "", _
                        Optional ByVal backgroundImage_url As String = "", _
                        Optional ByVal descriptionType As String = "detail", _
                        Optional ByVal description As String = "", _
                        Optional ByVal subCategories As List(Of Category) = Nothing)

                Me.ItemId = itemId
                Me.Label = label
                Me.[Alias] = [alias]
                Me.onClick = onClick
                Me.event_id = event_id
                Me.apix_id = apix_id
                Me.Url = url
                Me.Url_Manifest = url_Manifest
                Me.IsFolder = True 'must be before SetDefaultIcons
                Me.nextPage = 1
                Me.descriptionType = descriptionType
                Me.description = description
                AddHandler Me.OnItemSelected, AddressOf SkyGoDe.GUIMain.OnSelectedItem

                If Not (String.IsNullOrEmpty(iconImage)) Then
                    Me.IconImage = iconImage 'Icon in list items
                Else
                    MediaPortal.Util.Utils.SetDefaultIcons(Me)
                End If

                Me.ThumbnailImage = thumbnailImage 'Cover (left side)
                Me.backgroundImage = backgroundImage
                Me.backgroundImage_url = backgroundImage_url

                If Not subCategories Is Nothing Then
                    Me.SubCategories = New List(Of Category)
                    Me.SubCategories = subCategories
                End If
            End Sub

        End Class
    End Class

End Namespace

