#Region "Imports"
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MediaPortal.GUI.Library
Imports MediaPortal.Dialogs
Imports MediaPortal.Profile
Imports MediaPortal.Configuration
Imports MediaPortal.InputDevices
Imports System.ComponentModel
Imports System.Net
Imports SkyGoDe.PluginConfiguration
Imports SkyGoDe.JSONClasses
Imports System.Collections.Concurrent
Imports System.Drawing
Imports System.Reflection
Imports System.Web.Script.Serialization
Imports System.Collections
Imports System.Runtime.InteropServices
Imports System.Windows.Dispatcher
#End Region


Namespace SkyGoDe
    <PluginIcons("icon.png", "icon_deactivated.png")> _
    Public Class GUIMain
        Inherits GUIWindow
        Implements ISetupForm

#Region "Facade ViewModes"
        Protected currentView As GUIFacadeControl.Layout = GUIFacadeControl.Layout.List
#End Region

#Region "Skin Controls"
        <SkinControlAttribute(50)> _
        Protected GUI_facadeView As GUIFacadeControl = Nothing
#End Region

#Region "lokale variablen"
        Private t_readOrDeleteImages As Thread
        Private t_getRandomBgImages As Thread
        Private t_readLocalChannelList As Thread
        Private bw As BackgroundWorker
        Private firstLoadDone As Boolean = False
        Private initDone As Boolean = False
        Private Shared lastSelectedItemID As String = ""

#End Region

#Region "MediaPortal"

        Public Function Author() As String Implements ISetupForm.Author
            Return "Messiahs"
        End Function
        Public Overrides Property GetID() As Integer
            Get
                Return PluginConfiguration.WindowId
            End Get
            Set(ByVal value As Integer)
            End Set
        End Property
        ' Get Windows-ID
        Public Function GetWindowId() As Integer Implements ISetupForm.GetWindowId
            Return PluginConfiguration.WindowId
        End Function
        ' Returns the name of the plugin which is shown in the plugin menu
        Public Function PluginName() As String Implements ISetupForm.PluginName
            Return PluginConfiguration.Plugin_Name
        End Function
        ' Returns the description of the plugin is shown in the plugin menu
        Public Function Description() As String Implements ISetupForm.Description
            Return "on demand streams from SkyGo.de"
        End Function
        ' show the setup dialog
        Public Sub ShowPlugin() Implements ISetupForm.ShowPlugin
            Dim frmConfig As Form = New frmConfiguration
            frmConfig.ShowDialog()
        End Sub
        ' Indicates whether plugin can be enabled/disabled
        Public Function CanEnable() As Boolean Implements ISetupForm.CanEnable
            Return True
        End Function
        ' Indicates if plugin is enabled by default;
        Public Function DefaultEnabled() As Boolean Implements ISetupForm.DefaultEnabled
            Return True
        End Function
        ' indicates if a plugin has it's own setup screen
        Public Function HasSetup() As Boolean Implements ISetupForm.HasSetup
            Return True
        End Function
        ''' <summary>
        ''' If the plugin should have it's own button on the main menu of Mediaportal then it
        ''' should return true to this method, otherwise if it should not be on home
        ''' it should return false
        ''' </summary>
        ''' <param name="strButtonText">text the button should have</param>
        ''' <param name="strButtonImage">image for the button, or empty for default</param>
        ''' <param name="strButtonImageFocus">image for the button, or empty for default</param>
        ''' <param name="strPictureImage">subpicture for the button or empty for none</param>
        ''' <returns>true : plugin needs it's own button on home
        ''' false : plugin does not need it's own button on home</returns>
        Public Function GetHome(ByRef strButtonText As String, _
                                ByRef strButtonImage As String, _
                                ByRef strButtonImageFocus As String, _
                                ByRef strPictureImage As String) As Boolean Implements ISetupForm.GetHome
            strButtonText = PluginName()
            ' don't use PluginConfiguration.Instance here -> GetHome is already called when MediaPortal starts up into HomeScreen
            ' and we don't want to load all sites and config at that moment already
            strButtonImage = String.Empty
            strButtonImageFocus = String.Empty
            strPictureImage = folder_Thumbs_Plugin_defaultFile
            Return True
        End Function

        Public Sub New()
            MyBase.New()
        End Sub

#End Region

#Region "init and background tasks"
        Public Overrides Function Init() As Boolean 'when MP starts
            Dim result As Boolean = Load(GUIGraphicsContext.Skin + "\SkyGoDe.xml")

            'start background init
            bw = New BackgroundWorker()
            bw.WorkerSupportsCancellation = False
            bw.WorkerReportsProgress = False

            AddHandler bw.DoWork, AddressOf bw_DoWork
            bw.RunWorkerAsync() 'fire and forget

            Log.Debug("SkyGoDe: init done")
            Return result
        End Function

        Private Sub bw_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
            Try
                ' The default connection limit is 2 in .Net on most platforms! This means downloading two files will block all other WebRequests.
                System.Net.ServicePointManager.DefaultConnectionLimit = 16

                userSetting = loadSettings()
                initPlugin()

            Catch ex As Exception
                Log.Error("SkyGoDe: bw_DoWork Error: " & ex.Message)
            End Try
        End Sub

        Protected Overrides Sub OnPageLoad() 'when Plugin called
            ' let animations run
            MyBase.OnPageLoad()

            'pageStore Clear because resume from Standby
            pageStore = New Dictionary(Of String, Page)

            If Not initDone Then 'initPlugin already done on firstStart 
                initPlugin()

                'wait till tasks are finished
                t_readOrDeleteImages.Join()
                t_getRandomBgImages.Join()
                t_readLocalChannelList.Join()
            End If

            GUIPropertyManager.SetProperty("#SkyGoDe.HeaderLabel", String.Empty)

            'Dummy value -> only for Screen Headers /Load firstPage
            Dim category As New Category("0", "skygode", "skygode", category.onClickType.startMenue)
            displayStatic_listing(category)

            setRandomBackgroundImage()

        End Sub

        Protected Overrides Sub OnPageDestroy(newWindowId As Integer)
            If GUIWindowManager.ActiveWindow = PluginConfiguration.WindowId Then ' My main Window
                pageStore.Clear()
                firstLoadDone = False
                initDone = False
            End If

            MyBase.OnPageDestroy(newWindowId)
        End Sub

        Private Sub initPlugin()

            'get and delete cover
            t_readLocalChannelList = New Thread(AddressOf Me.readLocalChannelList)
            t_readLocalChannelList.IsBackground = True
            t_readLocalChannelList.Start()

            'get and delete cover
            t_readOrDeleteImages = New Thread(AddressOf ImageDownloader.readOrDeleteImages)
            t_readOrDeleteImages.IsBackground = True
            t_readOrDeleteImages.Start()

            'get random background images
            t_getRandomBgImages = New Thread(AddressOf ImageDownloader.getRandomBgImages)
            t_getRandomBgImages.IsBackground = True
            t_getRandomBgImages.Start()
            initDone = True
        End Sub

#End Region

        Private Sub LoadAndShow(ByVal actionType As Category.onClickType, ByVal category As Category)
            Dim menuPage As New Page

            GUIWaitCursor.Init()
            GUIWaitCursor.Show()

            'do the Download
            Try
                menuPage = GetWebData(actionType, category)
                If menuPage.state = Workstate.complete Then
                    If tasks.Count > 0 Then

                        '29.03.2015    tasks.First.Wait(5000, cts.Token)
                        '    Task.WaitAll(tasks.ToArray(), 5000, cts.Token)
                        tasks.Last.Wait(5000, cts.Token)  'wait for the first added image 

                        '   Dim TaskArray As Array
                        '  Dim xx As Array = tasks.ToArray()
                        '  For Each t In tasks
                        '   If Not t.IsCompleted And Not t.IsCanceled Then
                        'TaskArray.add(t)
                        ' xx.
                        '  End If
                        '   Next

                        '   Log.Debug("SkyGoDe: tasks.First.Id: " & tasks.First.Id)
                        Log.Debug("SkyGoDe: tasks.Last.Id: " & tasks.Last.Id)
                    End If

                    If Not cts.IsCancellationRequested Then
                        showCategoryMenu(menuPage, menuPage.selectedListItemIndex)
                        storeMenuPage(menuPage)
                    End If
                End If

            Catch ex As AggregateException
            Catch ex As OperationCanceledException
                Log.Debug("SkyGoDe: Thread aborted ")
            Catch ex As Exception
                Message2Gui(ex.Message)
                Log.Error("SkyGoDe: LoadAndShow Error: " & ex.Message)
            Finally
                GUIWaitCursor.Hide()
            End Try
        End Sub

        Private Sub storeMenuPage(ByVal menuPage As Page, Optional ByVal nextPage As Boolean = False)
            If pageStore.Count > 0 AndAlso menuPage.alias = pageStore.Last.Key Then  ' Category.Alias = "nextPage" And pageStore.ContainsKey(menuPage.alias) Then 'Category.Alias = "nextPage" ''  prüfen auf Status oder eigene Sektion
                If nextPage Then
                    'remove all old entrys
                    menuPage.category = New BindingList(Of Category)

                    'write back stored items from pageStore to page
                    For Each category In pageStore(menuPage.alias).category
                        menuPage.category.Add(category)
                    Next
                Else
                    'add updated menuPage to pageStore
                    pageStore.Remove(menuPage.alias)
                    pageStore.Add(menuPage.alias, menuPage)
                End If

            Else
                'add whole menuPage to pageStore
                pageStore.Add(menuPage.alias, menuPage)
            End If
        End Sub

        Private Function getStatic_listing(ByVal category As Category) As Page
            Dim menuPage As New Page

            menuPage.state = Workstate.working
            menuPage.name = category.Label
            menuPage.alias = category.Alias
            menuPage.type = State.categories

            Select Case category.onClick

                Case category.onClickType.startMenue
                    menuPage.category.Add(New Category("1", "Filme", "filme", category.onClickType.FilmMenue))
                    menuPage.category.Add(New Category("2", "Serien", "serien", category.onClickType.SerieMenue))
                    menuPage.category.Add(New Category("3", "Dokus", "doku", category.onClickType.DokuMenue))
                    If Not userSetting.PlugIn_SkyGoDe_DisableMenue_Lifestyle Then
                        menuPage.category.Add(New Category("4", "Lifestyle", "lifestyle", category.onClickType.LifestyleMenue))
                    End If
                    If Not userSetting.PlugIn_SkyGoDe_DisableMenue_LiveChannels Then
                        menuPage.category.Add(New Category("6", "Live-Channels", "liveevents", category.onClickType.LiveEvent_Listing))
                    End If
                    If Not userSetting.PlugIn_SkyGoDe_DisableMenue_Snap Then
                        menuPage.category.Add(New Category("8", "Snap", "Snap", category.onClickType.SnapMenue))
                    End If

                Case category.onClickType.FilmMenue
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/newreleases/64_p1.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesFilm", onClick:=category.onClickType.Film_Listing, url:="film/newreleases/64.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/mostwatched/32.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedFilm", onClick:=category.onClickType.Film_Listing, url:="film/mostwatched/32.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/lastchance/63_p1.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Letzte Chance", [alias]:="LastChanceFilm", onClick:=category.onClickType.Film_Listing, url:="film/lastchance/63.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/all/227/header.json
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/all/227/A_p1.json
                    'http://www.skygo.sky.de/sg/multiplatform/ipad/json/automatic_listing/film/all/227/D.json
                    menuPage.category.Add(New Category(itemId:="4", label:="Alle Filme", [alias]:="AllFilmByLexic", onClick:=category.onClickType.FilmLetter_Listing, url:="film/all/227/header.json"))

                Case category.onClickType.SerieMenue
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/newreleases/2_p1.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesSeries", onClick:=category.onClickType.Episode_Listing, url:="series/newreleases/2.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/mostwatched/62.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedSeries", onClick:=category.onClickType.Episode_Listing, url:="series/mostwatched/62.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/lastchance/60_p1.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Letzte Chance", [alias]:="LastChanceSeries", onClick:=category.onClickType.Episode_Listing, url:="series/lastchance/60.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/all/221/header.json
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/all/221/A_p1.json
                    menuPage.category.Add(New Category(itemId:="4", label:="Alle Serien", [alias]:="AllSeriesByLexic", onClick:=category.onClickType.SerieLetter_Listing, url:="series/all/221/header.json"))

                Case category.onClickType.DokuMenue
                    menuPage.category.Add(New Category(itemId:="1", label:="Filme", [alias]:="dokufilme", onClick:=category.onClickType.DokuFilmMenue))
                    menuPage.category.Add(New Category(itemId:="2", label:="Serien", [alias]:="dokuserien", onClick:=category.onClickType.DokuSerieMenue))

                Case category.onClickType.DokuFilmMenue
                    menuPage.name = "Dokus/Filme"
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/newreleases/7_p1.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesFilm", onClick:=category.onClickType.DokuFilm_Listing, url:="film/newreleases/7.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/mostwatched/232.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedFilm", onClick:=category.onClickType.DokuFilm_Listing, url:="film/mostwatched/232.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/lastchance/101_p1.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Letzte Chance", [alias]:="LastChanceFilm", onClick:=category.onClickType.DokuFilm_Listing, url:="film/lastchance/101.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/all/211/header.json
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/film/all/211/A_p1.json
                    menuPage.category.Add(New Category(itemId:="4", label:="Alle Filme", [alias]:="AllDokuFilmByLexic", onClick:=category.onClickType.DokuFilmLetter_Listing, url:="film/all/211/header.json"))

                Case category.onClickType.DokuSerieMenue
                    menuPage.name = "Dokus/Serien"
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/newreleases/13_p1.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesSeries", onClick:=category.onClickType.DokuEpisode_Listing, url:="series/newreleases/13.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/mostwatched/106.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedSeries", onClick:=category.onClickType.DokuEpisode_Listing, url:="series/mostwatched/106.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/lastchance/102_p1.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Letzte Chance", [alias]:="LastChanceSeries", onClick:=category.onClickType.DokuEpisode_Listing, url:="series/lastchance/102.json"))
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/all/205/header.json
                    'http://www.skygo.sky.de/sg/multiplatform/web/json/automatic_listing/series/all/205/A_p1.json
                    menuPage.category.Add(New Category(itemId:="4", label:="Alle Serien", [alias]:="AllDokuSeriesByLexic", onClick:=category.onClickType.DokuSerieLetter_Listing, url:="series/all/205/header.json"))

                Case category.onClickType.LifestyleMenue
                    menuPage.name = "Lifestyle"
                    'http://www.skygo.sky.de//sg/multiplatform/ipad/json/automatic_listing/series/newreleases/309.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesSeries", onClick:=category.onClickType.LifestyleEpisode_Listing, url:="series/newreleases/309.json"))
                    'http://www.skygo.sky.de//sg/multiplatform/ipad/json/automatic_listing/series/mostwatched/324.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedSeries", onClick:=category.onClickType.LifestyleEpisode_Listing, url:="series/mostwatched/324.json"))
                    'http://www.skygo.sky.de//sg/multiplatform/ipad/json/automatic_listing/series/lastchance/310.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Letzte Chance", [alias]:="LastChanceSeries", onClick:=category.onClickType.LifestyleEpisode_Listing, url:="series/lastchance/310.json"))
                    'http://www.skygo.sky.de//sg/multiplatform/ipad/json/automatic_listing/series/all/314/header.json
                    menuPage.category.Add(New Category(itemId:="4", label:="Alle Serien", [alias]:="AllLifestyleSeriesByLexic", onClick:=category.onClickType.LifestyleSerieLetter_Listing, url:="series/all/314/header.json"))

                Case category.onClickType.SnapMenue
                    menuPage.category.Add(New Category(itemId:="1", label:="Filme", [alias]:="snapfilme", onClick:=category.onClickType.SnapFilmMenue))
                    menuPage.category.Add(New Category(itemId:="2", label:="Serien", [alias]:="snapserien", onClick:=category.onClickType.SnapSerieMenue))

                Case category.onClickType.SnapFilmMenue
                    menuPage.name = "Snap/Filme"
                    'http://www.skygo.sky.de/snap/multiplatform/ipad/json/automatic_listing/film/newreleases/listing.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Neustarts", [alias]:="NewReleasesFilmSNAP", onClick:=category.onClickType.SnapFilm_Listing, url:="film/newreleases/listing.json"))
                    'http://www.skygo.sky.de/snap/multiplatform/web/json/automatic_listing/film/mostwatched/listing.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Meistgesehen", [alias]:="MostWatchedFilmSNAP", onClick:=category.onClickType.SnapFilm_Listing, url:="film/mostwatched/listing.json"))
                    'http://www.skygo.sky.de/snap/multiplatform/web/json/automatic_listing/film/all/header.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Alle Filme", [alias]:="AllSnapFilmByLexic", onClick:=category.onClickType.SnapFilmLetter_Listing, url:="film/all/header.json"))

                Case category.onClickType.SnapSerieMenue
                    menuPage.name = "Snap/Serien"
                    'http://www.skygo.sky.de/snap/multiplatform/ipad/json/automatic_listing/series/mostwatched/listing.json
                    menuPage.category.Add(New Category(itemId:="1", label:="Meistgesehen", [alias]:="MostWatchedSeriesSNAP", onClick:=category.onClickType.SnapEpisode_Listing, url:="series/mostwatched/listing.json"))
                    'http://www.skygo.sky.de/snap/multiplatform/ipad/json/automatic_listing/series/lastchance/listing.json
                    menuPage.category.Add(New Category(itemId:="2", label:="Letzte Chance", [alias]:="LastChanceSeriesSNAP", onClick:=category.onClickType.SnapEpisode_Listing, url:="series/lastchance/listing.json"))
                    'http://www.skygo.sky.de/snap/multiplatform/web/json/automatic_listing/series/all/header.json
                    menuPage.category.Add(New Category(itemId:="3", label:="Alle Serien", [alias]:="AllSnapSeriesByLexic", onClick:=category.onClickType.SnapSerieLetter_Listing, url:="series/all/header.json"))


                Case Else
                    Message2Gui("Error in Function getStaticMenuItems")
            End Select

            menuPage.state = Workstate.complete
            Return menuPage
        End Function

        Private Sub displayStatic_listing(ByVal category As Category)
            Dim menuPage As New Page

            menuPage = getStatic_listing(category)

            If menuPage.state = Workstate.complete Then
                showCategoryMenu(menuPage)
                storeMenuPage(menuPage)
            End If
        End Sub

        Private Sub DisplayEpisodeListing(ByVal actionType As Category.onClickType, ByVal category As Category)
            Dim menuPage As New Page

            If Not category.SubCategories Is Nothing Then
                menuPage.state = Workstate.working
                menuPage.alias = category.ItemId.ToString
                menuPage.type = State.serie

                Dim onClick As Category.onClickType

                If category.onClick = PluginConfiguration.Category.onClickType.Episode_Listing Then
                    onClick = category.onClickType.Episode
                ElseIf category.onClick = PluginConfiguration.Category.onClickType.DokuEpisode_Listing Then
                    onClick = category.onClickType.DokuEpisode
                ElseIf category.onClick = PluginConfiguration.Category.onClickType.SnapEpisode_Listing Then
                    onClick = category.onClickType.SnapEpisode
                Else
                    Stop
                End If

                For Each cat In category.SubCategories
                    menuPage.category.Add(New Category(itemId:=cat.ItemId, label:=cat.Label, [alias]:=cat.ItemId.ToString, onClick:=onClick, url:=cat.Url, iconImage:=cat.IconImage, thumbnailImage:=cat.ThumbnailImage, backgroundImage:=cat.backgroundImage, backgroundImage_url:=cat.backgroundImage_url, descriptionType:=cat.descriptionType, description:=cat.description))
                Next
                menuPage.state = Workstate.complete
            End If

            If menuPage.state = Workstate.complete Then
                showCategoryMenu(menuPage)
                storeMenuPage(menuPage)
            End If
        End Sub

        Private Sub DisplayJSonListing(ByVal actionType As Category.onClickType, ByVal category As Category)
            ' Use our factory to run a task. 
            Dim t As Task = factory.StartNew(Sub()
                                                 LoadAndShow(actionType, category)
                                             End Sub)
        End Sub

        Public Sub DisplayPreviousCategoryMenu(Optional ByVal selectListItemIndex As Integer = -1) 'Optional ByVal bolfocusAliasName As Boolean = False

            cts.Cancel() 'should be on first position

            If tasks.Count > 0 Then
                Try
                    Task.WaitAny(tasks.ToArray(), 5000)

                    Dim t As Task = factory.StartNew(Sub()
                                                         'Remove all unfinished downloads from imageStore
                                                         Dim itemsToRemove = (From pair In ListOfExistingCovers Where pair.Value.Length > 0 Select pair.Key).ToArray()
                                                         For Each item As String In itemsToRemove
                                                             ListOfExistingCovers.Remove(item)
                                                         Next
                                                     End Sub)
                    'clear tasks
                    SyncLock (tasks)
                        tasks = New ConcurrentBag(Of Task)
                    End SyncLock

                Catch ex As AggregateException
                    GUIWaitCursor.Hide()
                Catch ex As Exception
                    Log.Error("SkyGoDe: DisplayPreviousCategoryMenu Error: " & ex.Message)
                End Try
            End If

            'get Key to focus
            If pageStore.Count > 1 Then
                Dim menuPage As New Page
                Dim currentMenueKey As String = pageStore.ElementAt(pageStore.Count - 1).Key

                'remove last one
                pageStore.Remove(currentMenueKey)

                'get last key
                Dim newCurrentMenueKey As String = pageStore.ElementAt(pageStore.Count - 1).Key

                menuPage = pageStore.Item(newCurrentMenueKey)
                If menuPage.state = Workstate.complete Then
                    showCategoryMenu(menuPage, selectListItemIndex)
                    If menuPage.type = State.categories Then
                        setRandomBackgroundImage()
                    End If

                End If
            Else
                firstLoadDone = False
                initDone = False
                OnPreviousWindow()  'reset and close
            End If

        End Sub

        Private Sub showCategoryMenu(ByVal menuPage As Page, Optional ByVal selectListItemIndex As Integer = -1)
            Log.Debug("SkyGoDe: showCategoryMenu")
            GUIControl.ClearControl(GetID, GUI_facadeView.GetID)
            GUIPropertyManager.SetProperty("#SkyGoDe.state", menuPage.type.ToString)

            'add entry "back"
            GUI_facadeView.Add(New Category("back", "..", "back"))

            For Each listItem As Category In menuPage.category
                GUI_facadeView.Add(listItem)
            Next

            '  If menuPage.HasNextPage Then
            ' GUI_facadeView.Add(New Category(itemId:="99999", label:="nächste Seite", [alias]:="nextPage", onClick:=Category.onClickType.Film_Listing, url:=menuPage.Url, iconImage:="SkyGoDe\Icons\NextPage.png", thumbnailImage:="SkyGoDe\Icons\NextPage.png"))
            '  End If

            If Not String.IsNullOrEmpty(menuPage.name) Then
                Log.Debug("SkyGoDe: showCategoryMenu menuPage.name: " & menuPage.name)
                If menuPage.name = "skygode" Then
                    GUIPropertyManager.SetProperty("#header.label", Plugin_Name)
                    GUIPropertyManager.SetProperty("#SkyGoDe.HeaderLabel", String.Empty)
                    GUIPropertyManager.SetProperty("#SkyGoDe.HeaderImage", folder_Thumbs_Plugin_logoFile)
                Else
                    GUIPropertyManager.SetProperty("#SkyGoDe.HeaderLabel", menuPage.name)
                End If
            End If

            'get index to select item /needed for nextPage
            If selectListItemIndex > -1 Then
                selectListItemIndex = selectListItemIndex
            ElseIf menuPage.category.Count >= menuPage.selectedListItemIndex And menuPage.selectedListItemIndex > -1 Then
                selectListItemIndex = menuPage.selectedListItemIndex
            Else
                selectListItemIndex = 1 'default index to select (first call)
            End If

            If menuPage.category.Count > 0 Then
                GUIPropertyManager.SetProperty("#itemcount", CInt(menuPage.category.Count).ToString())
                GUI_facadeView.CurrentLayout = currentView ' explicitly set the view (fixes bug that facadeView.list isn't working at startup
                GUI_facadeView.SelectedListItemIndex = selectListItemIndex
            ElseIf menuPage.category.Count = 0 Then
                Log.Debug("SkyGoDe: showCategoryMenu no Items found")
                Message2Gui("no Items found", "ShowCategoryMenu") 'toDo hier ggf. falscher Thread
            Else
                Message2Gui("Error on showCategoryMenu")
            End If

        End Sub

#Region "Events"
        Public Overrides Sub OnAction(action As MediaPortal.GUI.Library.Action)
            Select Case action.wID
                Case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU
                    DisplayPreviousCategoryMenu(0) 'back & focus first item 
                    Return
            End Select

            MyBase.OnAction(action)
        End Sub

        Friend Shared Sub OnSelectedItem(ByVal item As GUIListItem, ByVal parent As GUIControl)
            Try
                Dim selectedItem As Category = DirectCast(item, Category)

                '   If String.IsNullOrWhiteSpace(lastSelectedItemID) Then '12.04.2015 hier Probleme für den ersten Eintrag
                'lastSelectedItemID = selectedItem.ItemId
                ' GUIPropertyManager.SetProperty("#SkyGoDe.desc", String.Empty)
                ' GUIPropertyManager.SetProperty("#selecteditem", String.Empty)
                ' Else
                If selectedItem.ItemId <> lastSelectedItemID Then
                    'neu
                    'cts.Cancel() 'should be on first position
                    'clear tasks
                    '  If tasks.Count > 0 Then
                    '   SyncLock (tasks)
                    '    tasks = New ConcurrentBag(Of Task)
                    ' End SyncLock
                    'End If

                    '  cts = New CancellationTokenSource
                    'stop
                    lastSelectedItemID = selectedItem.ItemId
                    If selectedItem.Alias = "back" Then
                        GUIPropertyManager.SetProperty("#SkyGoDe.desc", String.Empty)
                        GUIPropertyManager.SetProperty("#selecteditem", String.Empty)
                    Else
                        Dim backgroundImagePath As String = String.Empty
                        If Not String.IsNullOrWhiteSpace(selectedItem.backgroundImage_url) And Not String.IsNullOrWhiteSpace(selectedItem.backgroundImage) AndAlso Not ListOfExistingBackdrops.ContainsKey(selectedItem.backgroundImage) Then
                            'download background image
                            ListOfExistingBackdrops(selectedItem.backgroundImage) = "" 'first add this pic
                            downloadBackdrops(selectedItem.backgroundImage, selectedItem.backgroundImage_url, "background") 'toDo cancel request if selectedItem changed
                        End If

                        ' because of image download -> query selectedItem is equal with lastSelectedItemID
                        If selectedItem.ItemId = lastSelectedItemID Then
                            If Not String.IsNullOrWhiteSpace(selectedItem.backgroundImage) Then
                                backgroundImagePath = selectedItem.backgroundImage
                                '   Dim x = GUIPropertyManager.GetProperty("#SkyGoDe.Backdrop")
                                GUIPropertyManager.SetProperty("#SkyGoDe.Backdrop", String.Empty)
                                GUIPropertyManager.SetProperty("#SkyGoDe.Backdrop", backgroundImagePath) 'selectedItem.backgroundImage
                            End If

                            '06.03.
                            '  If x = 1 Then
                            '   GUIPropertyManager.SetProperty("#SkyGoDe.Backdrop", backgroundImagePath)
                            '   GUIPropertyManager.SetProperty("#SkyGoDe.display1", "show")
                            '   GUIPropertyManager.SetProperty("#SkyGoDe.display2", "hide")
                            '    GUIControl.ShowControl(PluginConfiguration.WindowId, 15)
                            ' GUIControl.HideControl(PluginConfiguration.WindowId, 16)
                            '  x = 2
                            ' Else
                            '  GUIPropertyManager.SetProperty("#SkyGoDe.Backdrop2", backgroundImagePath)
                            '   GUIControl.ShowControl(PluginConfiguration.WindowId, 16)
                            '  GUIControl.HideControl(PluginConfiguration.WindowId, 15)
                            '  x = 1
                            'End If
                        End If

                        If selectedItem.onClick = Category.onClickType.Episode Or _
                            selectedItem.onClick = Category.onClickType.DokuEpisode Or _
                            selectedItem.onClick = Category.onClickType.SnapEpisode Then

                            If selectedItem.descriptionType <> "detail" And selectedItem.descriptionType <> "loading" Then
                                selectedItem.descriptionType = "loading"
                                'get episode description
                                'http://www.skygo.sky.de/sg/multiplatform/ipad/json/details/asset/120703.json
                                Dim categoryStruct As ResponseStruct
                                categoryStruct = SkyGoDeWeb.GetWebAdditionalData(selectedItem.onClick, selectedItem)

                                If categoryStruct.returnCode = ResponseCode.Ok Then
                                    selectedItem = categoryStruct.category
                                    If pageStore.Count > 0 Then
                                        Dim page As Page = pageStore.ElementAt(pageStore.Count - 2).Value

                                        For i As Integer = 0 To pageStore.ElementAt(pageStore.Count - 2).Value.category.Count - 1
                                            Try
                                                If Not pageStore.ElementAt(pageStore.Count - 2).Value.category.Item(i).SubCategories Is Nothing Then
                                                    For x As Integer = 0 To pageStore.ElementAt(pageStore.Count - 2).Value.category.Item(i).SubCategories.Count - 1
                                                        If pageStore.ElementAt(pageStore.Count - 2).Value.category.Item(i).SubCategories.Item(x).Alias = selectedItem.ItemId.ToString Then
                                                            pageStore.ElementAt(pageStore.Count - 2).Value.category.Item(i).SubCategories.Item(x).description = selectedItem.description
                                                            pageStore.ElementAt(pageStore.Count - 2).Value.category.Item(i).SubCategories.Item(x).descriptionType = selectedItem.descriptionType
                                                        End If
                                                    Next
                                                End If
                                            Catch ex As Exception
                                                Stop
                                            End Try
                                        Next
                                    End If
                                Else
                                    Message2Gui(categoryStruct.string, "OnItemSelected")
                                End If
                            End If
                        End If

                        If selectedItem.ItemId = lastSelectedItemID Then
                            GUIPropertyManager.SetProperty("#SkyGoDe.desc", selectedItem.description)
                        End If
                    End If

                End If
            Catch ex As Exception
                Stop
            End Try

        End Sub

        <STAThread> _
        Protected Overrides Sub OnClicked(ByVal controlId As Integer, _
                                          ByVal control As GUIControl, _
                                          ByVal actionType As  _
                                          MediaPortal.GUI.Library.Action.ActionType)

            cts.Cancel() 'should be on first position
            'clear tasks
            If tasks.Count > 0 Then
                SyncLock (tasks)
                    tasks = New ConcurrentBag(Of Task)
                End SyncLock
            End If

            cts = New CancellationTokenSource
            lastSelectedItemID = ""

            If control Is GUI_facadeView Then
                If actionType = MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM Then
                    Dim facadeControl As GUIFacadeControl = DirectCast(control, MediaPortal.GUI.Library.GUIFacadeControl)
                    Dim selectedItem As Category = DirectCast(facadeControl.SelectedListItem, Category)

                    If selectedItem.Alias <> "back" Then
                        selectedItem.ListItemIndex = facadeControl.SelectedListItemIndex 'store ItemIndex in category -> focus item
                        UpdateCurrentPage(selectedItem.ListItemIndex)
                    End If

                    If selectedItem.Alias = "back" Then
                        DisplayPreviousCategoryMenu()

                    ElseIf selectedItem.onClick = Category.onClickType.startMenue _
                        Or selectedItem.onClick = Category.onClickType.FilmMenue _
                        Or selectedItem.onClick = Category.onClickType.SerieMenue _
                        Or selectedItem.onClick = Category.onClickType.DokuMenue _
                        Or selectedItem.onClick = Category.onClickType.DokuFilmMenue _
                        Or selectedItem.onClick = Category.onClickType.DokuSerieMenue _
                        Or selectedItem.onClick = Category.onClickType.LifestyleMenue _
                        Or selectedItem.onClick = Category.onClickType.SnapMenue _
                        Or selectedItem.onClick = Category.onClickType.SnapFilmMenue _
                        Or selectedItem.onClick = Category.onClickType.SnapSerieMenue Then
                        displayStatic_listing(selectedItem) 'Display StaticMenue

                    ElseIf selectedItem.onClick = Category.onClickType.FilmLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.SerieLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.DokuFilmLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.DokuSerieLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.LifestyleSerieLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.SnapFilmLetter_Listing _
                        Or selectedItem.onClick = Category.onClickType.SnapSerieLetter_Listing Then
                        DisplayJSonListing(selectedItem.onClick, selectedItem)

                    ElseIf selectedItem.onClick = Category.onClickType.Film_Listing _
                        Or selectedItem.onClick = Category.onClickType.Serie_Listing _
                        Or selectedItem.onClick = Category.onClickType.DokuFilm_Listing _
                        Or selectedItem.onClick = Category.onClickType.DokuSerie_Listing _
                        Or selectedItem.onClick = Category.onClickType.LifestyleSerie_Listing _
                        Or selectedItem.onClick = Category.onClickType.SnapFilm_Listing _
                        Or selectedItem.onClick = Category.onClickType.SnapSerie_Listing Then
                        DisplayJSonListing(selectedItem.onClick, selectedItem)

                    ElseIf selectedItem.onClick = Category.onClickType.Episode_Listing _
                        Or selectedItem.onClick = Category.onClickType.DokuEpisode_Listing _
                        Or selectedItem.onClick = Category.onClickType.LifestyleEpisode_Listing _
                        Or selectedItem.onClick = Category.onClickType.SnapEpisode_Listing Then
                        If Not selectedItem.SubCategories Is Nothing AndAlso selectedItem.SubCategories.Count > 0 Then
                            'Episode already stored
                            DisplayEpisodeListing(selectedItem.onClick, selectedItem)
                        Else
                            DisplayJSonListing(selectedItem.onClick, selectedItem)
                        End If

                    ElseIf selectedItem.onClick = Category.onClickType.Staffel_Listing Or _
                        selectedItem.onClick = Category.onClickType.DokuStaffel_Listing Or _
                        selectedItem.onClick = Category.onClickType.LifestyleStaffel_Listing Or _
                        selectedItem.onClick = Category.onClickType.SnapStaffel_Listing Then
                        DisplayJSonListing(selectedItem.onClick, selectedItem)

                    ElseIf selectedItem.onClick = Category.onClickType.Film Or _
                        selectedItem.onClick = Category.onClickType.Episode Or _
                        selectedItem.onClick = Category.onClickType.DokuFilm Or _
                        selectedItem.onClick = Category.onClickType.DokuEpisode Or _
                        selectedItem.onClick = Category.onClickType.LifestyleEpisode Or _
                        selectedItem.onClick = Category.onClickType.SnapFilm Or _
                        selectedItem.onClick = Category.onClickType.SnapEpisode Or _
                        selectedItem.onClick = Category.onClickType.LiveStream Then

                        Dim loginResponse As New LoginResponseStruct
                        loginResponse = doLogin()
                        If loginResponse.returnCode = LoginReturnCode.Ok Then
                            Dim categoryStruct As New ResponseStruct
                            Dim startupArgs As New List(Of String)

                            If Not selectedItem.ItemId = "0" Then
                                categoryStruct = SkyGoDeWeb.GetWebAdditionalData(selectedItem.onClick, selectedItem)
                                selectedItem = categoryStruct.category
                            Else
                                categoryStruct.returnCode = ResponseCode.Ok
                            End If

                            If categoryStruct.returnCode = ResponseCode.Ok Then
                                If Not String.IsNullOrEmpty(selectedItem.Url_Manifest) Then
                                    startupArgs.Add("url_Manifest=" & selectedItem.Url_Manifest)
                                    startupArgs.Add("filePath_backdrop=" & selectedItem.backgroundImage)
                                    startupArgs.Add("sessionId=" & _SLPlayerStartupArgs.sessionId)
                                    startupArgs.Add("apixId=" & selectedItem.apix_id)

                                    If String.IsNullOrEmpty(selectedItem.product) Then
                                        selectedItem.product = "BW"
                                    End If
                                    startupArgs.Add("product=" & selectedItem.product)
                                    'stop playing content
                                    Call stopInternalMPPlayer()
                                    Call createSLPlayerStartupArgs(startupArgs)
                                    Call startSilverlightPlayer(selectedItem.Url)
                                Else
                                    Message2Gui("Manifest-Url konnte nicht ermittelt werden.", "OnClicked")
                                End If
                            Else
                                Message2Gui(categoryStruct.string, "OnClicked")
                            End If
                        ElseIf loginResponse.returnCode = LoginReturnCode.Error Then
                            Message2Gui(loginResponse.string, "Login error")
                        End If

                    ElseIf selectedItem.onClick = Category.onClickType.LiveEvent_Listing Then
                        DisplayJSonListing(selectedItem.onClick, selectedItem)
                    Else
                        Message2Gui("Error in function: OnClicked " & selectedItem.onClick.ToString)
                    End If

                    If selectedItem.onClick = Category.onClickType.startMenue Or _
                        selectedItem.onClick = Category.onClickType.FilmMenue Or _
                        selectedItem.onClick = Category.onClickType.SerieMenue Or _
                        selectedItem.onClick = Category.onClickType.DokuMenue Or _
                        selectedItem.onClick = Category.onClickType.DokuFilmMenue Or _
                        selectedItem.onClick = Category.onClickType.DokuSerieMenue Or _
                        selectedItem.onClick = Category.onClickType.LifestyleMenue Or _
                        selectedItem.onClick = Category.onClickType.SnapMenue Or _
                        selectedItem.onClick = Category.onClickType.SnapFilmMenue Or _
                        selectedItem.onClick = Category.onClickType.SnapSerieMenue Or _
                        selectedItem.onClick = Category.onClickType.DokuFilmLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.DokuSerieLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.LifestyleSerieLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.SnapFilmLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.SnapSerieLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.FilmLetter_Listing Or _
                        selectedItem.onClick = Category.onClickType.SerieLetter_Listing Then
                        setRandomBackgroundImage()
                    End If

                End If
            End If

            '   MyBase.OnClicked(controlId, control, actionType)
        End Sub

        Private Function stopInternalMPPlayer() As Boolean

            If g_Player.Playing Then
                g_Player.Stop()
            End If

            Return True
        End Function

        Private Function createSLPlayerStartupArgs(ByVal startupArgs As List(Of String)) As Boolean
            Dim sw As IO.StreamWriter = Nothing

            Try
                sw = New StreamWriter(filePath_StartupArgs, False, System.Text.Encoding.UTF8)
                For Each str In startupArgs
                    sw.WriteLine(str)
                Next
            Catch ex As Exception

            Finally
                If Not sw Is Nothing Then
                    sw.Close()
                End If
            End Try

            Return True
        End Function

        Private Function readLocalChannelList() As Dictionary(Of String, JSONChannelListClass.ChannelList)
            Dim strResponse As String
            strResponse = readFile(filePath_ChannelList)
            localChannelList = parseJsonLocalChannelList(strResponse)

            Return localChannelList
        End Function

        Private Function readFile(ByVal filePath As String) As String
            Dim strResponse As String = ""
            If File.Exists(filePath) Then
                Using sr As New StreamReader(filePath, System.Text.Encoding.UTF8)
                    While Not (sr.EndOfStream)
                        strResponse = sr.ReadToEnd()
                    End While
                End Using
            Else
                Call Message2Gui("Datei:" & Environment.NewLine & filePath & Environment.NewLine & "nicht gefunden.")
            End If

            Return strResponse
        End Function

        Private Sub startSilverlightPlayer(ByVal url_manifest As String)

            If File.Exists(folder_ProgramFiles_sllauncherFile) Then 'Sllancher exists
                If File.Exists(folder_Plugin_Windows_SkyGoDe_SLPlayerFile) Then 'Player exists
                    Dim args As String = String.Format(" /emulate:""{0}"" /origin:{1} /overwrite", folder_Plugin_Windows_SkyGoDe_SLPlayerFile, "http://localhost")

                    SuspendMP(True)
                    '    Dim myD1 As New SuspendMPDelegate(AddressOf SuspendMP)
                    '   myD1.Invoke(True)


                    '  Dim t As Task = Task.Factory.StartNew(Sub()
                    Dim process = New Process
                    Dim StartInfo As ProcessStartInfo = New ProcessStartInfo(folder_ProgramFiles_sllauncherFile)
                    process.EnableRaisingEvents = True
                    process.StartInfo.CreateNoWindow = False
                    process.StartInfo.RedirectStandardError = True
                    process.StartInfo.UseShellExecute = False
                    process.StartInfo.Arguments = args
                    process.StartInfo.FileName = folder_ProgramFiles_sllauncherFile
                    '      process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized '19.03.2015
                    Try
                        process.Start()
                        process.PriorityClass = ProcessPriorityClass.High
                        process.WaitForExit()
                        If process.ExitCode > 0 Then
                            Stop
                        End If
                        SuspendMP(False)
                        '   Dim myD2 As New SuspendMPDelegate(AddressOf SuspendMP)
                        '   myD2.Invoke(False)

                    Catch ex As Exception
                        Stop
                    End Try
                Else
                    Message2Gui("Error: File '" & folder_Plugin_Windows_SkyGoDe_SLPlayerFile & "' nicht gefunden.")
                End If
            Else
                Message2Gui("Error: File '" & folder_ProgramFiles_sllauncherFile & "' nicht gefunden.")
            End If
        End Sub

        Friend Shared Sub setRandomBackgroundImage()
            Dim backgroundImagePath As String = ""

            If backgroundImagePathStore.Count > 0 Then
                backgroundImagePath = backgroundImagePathStore.ElementAt(0)
                backgroundImagePathStore.Add(backgroundImagePath)
                backgroundImagePathStore.RemoveAt(0)
            End If

            If String.IsNullOrWhiteSpace(backgroundImagePath) Then
                backgroundImagePath = folder_Thumbs_Plugin
            End If

            GUIPropertyManager.SetProperty("#SkyGoDe.Backdrop", backgroundImagePath)
        End Sub
#End Region

        Public Function doLogin() As LoginResponseStruct
            Dim loginResponseStruct As LoginResponseStruct = SkyGoDeWeb.MainLoginHandler(userSetting.PlugIn_SkyGoDe_UserID, userSetting.PlugIn_SkyGoDe_PIN)

            Return loginResponseStruct
        End Function

        Public Sub SuspendMP(ByVal suspend As Boolean)
            Dim retval = False
            If suspend Then
                InputDevices.Stop()

                Dim pos As New Drawing.Point
                pos.X = 8000
                pos.Y = 0
                Cursor.Position = pos

                If userSetting.MP_alwaysontop Then
                    Dim hwndStartButton = NativeMethods.FindWindow("Button", "Start")
                    If hwndStartButton <> IntPtr.Zero Then
                        NativeMethods.ShowWindow(hwndStartButton, NativeMethods.ShowWindowCommands.Hide)
                        '   NativeMethods.ShowWindow(hwndStartButton, NativeMethods.ShowWindowCommands.Minimize)
                    End If

                    Dim TaskBarHwnd As IntPtr = NativeMethods.FindWindow("Shell_traywnd", "")
                    If TaskBarHwnd <> IntPtr.Zero Then
                        NativeMethods.ShowWindow(TaskBarHwnd, NativeMethods.ShowWindowCommands.Hide)
                        '   NativeMethods.ShowWindow(TaskBarHwnd, NativeMethods.ShowWindowCommands.Minimize)
                    End If
                End If


                GUIGraphicsContext.IsFullScreenVideo = True
                ' GUIGraphicsContext.IsPlaying = True
                '  GUIGraphicsContext.IsPlayingVideo = True

                GUIGraphicsContext.BlankScreen = True
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING

                Dim currProcess As Process
                currProcess = System.Diagnostics.Process.GetCurrentProcess
                currProcess.PriorityClass = ProcessPriorityClass.BelowNormal

            Else
                ' reset sleeping Mediaportal 
                Dim currProcess As Process
                currProcess = System.Diagnostics.Process.GetCurrentProcess
                currProcess.PriorityClass = userSetting.MP_ThreadPriority 'set old ThreadPriority

                InputDevices.Init()

                'If MP was configured 'alwaysontop' -> reset z-order & focus
                If userSetting.MP_alwaysontop Then
                    Try
                        Dim hwndStartButton = NativeMethods.FindWindow("Button", "Start")
                        If hwndStartButton <> IntPtr.Zero Then
                            NativeMethods.ShowWindow(hwndStartButton, NativeMethods.ShowWindowCommands.Minimize)
                            NativeMethods.ShowWindow(hwndStartButton, NativeMethods.ShowWindowCommands.Restore)
                        End If

                        Dim TaskBarHwnd As IntPtr = NativeMethods.FindWindow("Shell_traywnd", "")
                        If TaskBarHwnd <> IntPtr.Zero Then
                            NativeMethods.ShowWindow(TaskBarHwnd, NativeMethods.ShowWindowCommands.Restore)
                            NativeMethods.ShowWindow(TaskBarHwnd, NativeMethods.ShowWindowCommands.Minimize)
                        End If

                        Dim hwnd As IntPtr = GUIGraphicsContext.form.Handle
                        If hwnd <> IntPtr.Zero Then
                            NativeMethods.BringWindowToTop(hwnd)
                            NativeMethods.ShowWindow(hwnd, NativeMethods.ShowWindowCommands.Normal)
                        End If
                    Catch ex As Exception
                        Stop
                    End Try
                End If

                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING
                GUIGraphicsContext.IsFullScreenVideo = False
                ' GUIGraphicsContext.IsPlaying = False
                '  GUIGraphicsContext.IsPlayingVideo = False
                GUIGraphicsContext.BlankScreen = False
                GUIGraphicsContext.ResetLastActivity()
            End If

        End Sub

        Public Sub UpdateCurrentPage(ByVal index As Integer)
            If pageStore.Count > 0 Then
                pageStore.Item(pageStore.ElementAt(pageStore.Count - 1).Key).selectedListItemIndex = index
            End If
        End Sub

        <STAThread>
        Public Shared Sub Message2Gui(ByVal errorMsg As String, Optional ByVal title As String = "")
            Dim dlg_notify As GUIDialogNotify = CType(GUIWindowManager.GetWindow(CType(GUIWindow.Window.WINDOW_DIALOG_NOTIFY, Integer)), GUIDialogNotify)
            dlg_notify.Reset()
            dlg_notify.SetHeading(Plugin_Name)
            dlg_notify.SetText(errorMsg)
            dlg_notify.DoModal(GUIWindowManager.ActiveWindow)
        End Sub

    End Class

   
End Namespace
