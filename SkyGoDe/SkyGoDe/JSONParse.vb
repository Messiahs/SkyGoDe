Imports SkyGoDe.JSONClasses
Imports SkyGoDe.PluginConfiguration
Imports System.Web.Script.Serialization
Imports System.IO
Imports MediaPortal.GUI.Library

Namespace SkyGoDe
    Module JSONParse
        Friend Function parseJsonLetterResponse(ByVal strResponse As String, ByVal category As Category) As Page
            Dim menuPage As New Page

            If Not strResponse Is Nothing Then
                '/thanks to http://json2csharp.com/ &   http://jsonutils.com/
                Dim DataSet As JSONLettersClass.JSONObject = New JavaScriptSerializer().Deserialize(Of JSONLettersClass.JSONObject)(strResponse)
                If Not DataSet Is Nothing Then
                    Dim i As Integer

                    menuPage.alias = category.Alias
                    menuPage.Url = category.Url

                    For Each item As JSONLettersClass.Letter In DataSet.letters.letter
                        If item.linkable Then
                            i += 1
                            If category.onClick = category.onClickType.FilmLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.Film_Listing, "film/all/227/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.SerieLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.Serie_Listing, "series/all/221/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.DokuFilmLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.DokuFilm_Listing, "film/all/211/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.DokuSerieLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.DokuSerie_Listing, "series/all/205/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.LifestyleSerieLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.LifestyleSerie_Listing, "series/all/314/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.SnapFilmLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.SnapFilm_Listing, "film/all/" & item.content.ToString & ".json"))
                            ElseIf category.onClick = category.onClickType.SnapSerieLetter_Listing Then
                                menuPage.category.Add(New Category(i.ToString, item.content.ToString, menuPage.alias & "/" & item.content.ToString, category.onClickType.SnapSerie_Listing, "series/all/" & item.content.ToString & ".json"))
                            Else
                                Stop
                            End If
                        End If
                    Next
                End If
            End If

            Return menuPage
        End Function

        Friend Function parseJsonSerieResponse(ByVal strResponse As String, ByVal strUrl As String, ByVal category As Category) As Page
            Dim menuPage As New Page

            Try
                If Not strResponse Is Nothing Then
                    Dim DataSet As JSONClass.SerieObject = New JavaScriptSerializer().Deserialize(Of JSONClass.SerieObject)(strResponse) 'JSONSerieClass.JSONObject
                    If Not DataSet Is Nothing Then
                        Dim BackdropImage2Download As New Dictionary(Of String, String)

                        menuPage.state = Workstate.working
                        menuPage.type = State.categories
                        menuPage.Url = strUrl

                        Dim JSONSerieItem As JSONClass.Serie = DataSet.serieRecap.serie
                        If Not JSONSerieItem Is Nothing Then
                            Dim serie As New SerieInfo

                            'Page header
                            menuPage.alias = JSONSerieItem.id.ToString
                            menuPage.type = State.serie
                            menuPage.Url = strUrl

                            serie.ID = JSONSerieItem.id
                            serie.Title = JSONSerieItem.title
                            serie.Alias = JSONSerieItem.id.ToString
                            serie.Apix_ID = JSONSerieItem.apix_id
                            serie.Title2 = JSONSerieItem.original_title
                            serie.Description = JSONSerieItem.synopsis
                            serie.VideoUrl = JSONSerieItem.webvod_canonical_url

                            'getImagePaths 
                            Dim imagePaths As ImagePaths
                            imagePaths = parseImage(JSONSerieItem.dvd_cover, JSONSerieItem.main_picture)

                            serie.coverSmall_localPath = imagePaths.coverSmall_localPath
                            serie.coverSmall_url = imagePaths.coverSmall_url
                            serie.coverBig_localPath = imagePaths.cover_big_localPath
                            serie.coverBig_url = imagePaths.cover_big_url

                            If Not String.IsNullOrEmpty(imagePaths.backgroundImage_localPath) Then
                                serie.backgroundImage_localPath = imagePaths.backgroundImage_localPath
                                serie.backgroundImage_url = imagePaths.backgroundImage_url
                            Else
                                serie.backgroundImage_localPath = String.Empty
                                serie.backgroundImage_url = String.Empty
                            End If

                            'Download Covers
                            If Not ListOfExistingCovers.ContainsKey(serie.coverSmall_localPath) Then
                                ListOfExistingCovers.Add(serie.coverSmall_localPath, serie.coverSmall_url) 'keine 100% Lösung, da WebResponse ggf. auf einen Fehler läuft 
                                downloadPic(serie.coverSmall_localPath, serie.coverSmall_url, "cover")
                            End If

                            If Not ListOfExistingBackdrops.ContainsKey(serie.backgroundImage_localPath) And Not BackdropImage2Download.ContainsKey(serie.backgroundImage_localPath) Then
                                BackdropImage2Download.Add(serie.backgroundImage_localPath, serie.backgroundImage_url)
                            End If

                            'now get Seasons
                            'season selbst, hat keine brauchbaren Bilder
                            For Each JSONSeason As JSONClass.Season In JSONSerieItem.seasons.season
                                Dim staffel As New SerieInfo.season
                                staffel.ID = JSONSeason.id
                                staffel.Title = "Staffel: " & JSONSeason.nr.ToString
                                staffel.Alias = "Staffel" & JSONSeason.nr.ToString

                                'getImagePaths 
                                'now get Episodes
                                'all episodes have the same pic, but different jpg names
                                'so just read only first entry
                                Dim dvd_cover As New JSONClass.PictureInfo

                                If Not String.IsNullOrEmpty(JSONSeason.dvd_cover.file) Then
                                    imagePaths = parseImage(JSONSeason.dvd_cover, JSONSeason.main_picture)
                                ElseIf JSONSeason.episodes.episode.Count > 0 Then
                                    dvd_cover.file = JSONSeason.episodes.episode(0).dvd_cover.file
                                    dvd_cover.path = JSONSeason.episodes.episode(0).dvd_cover.path
                                    imagePaths = parseImage(dvd_cover, JSONSeason.main_picture)
                                Else
                                    Stop
                                End If

                                staffel.coverSmall_url = imagePaths.coverSmall_url
                                staffel.coverSmall_localPath = imagePaths.coverSmall_localPath
                                staffel.coverBig_url = imagePaths.cover_big_url
                                staffel.coverBig_localPath = imagePaths.cover_big_localPath

                                'Background aus Serie übernehmen, da "season.main_picture" nocht gefüllt ist
                                staffel.backgroundImage_localPath = serie.backgroundImage_localPath
                                staffel.backgroundImage_url = serie.backgroundImage_url

                                For Each JSONEpisode As JSONClass.Asset In JSONSeason.episodes.episode
                                    Dim episode As New SerieInfo.episode
                                    episode.ID = JSONEpisode.id
                                    episode.Title = JSONEpisode.title & " (S" & JSONEpisode.season_nr & "E" & JSONEpisode.episode_nr & ")"
                                    episode.Alias = JSONEpisode.id.ToString
                                    episode.VideoUrl = JSONEpisode.webvod_canonical_url

                                    'episode hat keine Cover 
                                    If Not String.IsNullOrWhiteSpace(JSONSeason.dvd_cover.file) Then
                                        imagePaths = parseImage(JSONSeason.dvd_cover, JSONEpisode.main_picture)
                                    ElseIf Not String.IsNullOrWhiteSpace(JSONEpisode.dvd_cover.file) Then
                                        imagePaths = parseImage(JSONEpisode.dvd_cover, JSONEpisode.main_picture)
                                    Else
                                        Stop
                                    End If


                                    If String.IsNullOrEmpty(imagePaths.backgroundImage_localPath) Then
                                        imagePaths.backgroundImage_localPath = staffel.backgroundImage_localPath
                                    End If

                                    If Not ListOfExistingBackdrops.ContainsKey(imagePaths.backgroundImage_localPath) And Not BackdropImage2Download.ContainsKey(imagePaths.backgroundImage_localPath) Then
                                        BackdropImage2Download.Add(imagePaths.backgroundImage_localPath, imagePaths.backgroundImage_url)
                                    End If

                                    episode.coverSmall_localPath = imagePaths.coverSmall_localPath
                                    episode.coverSmall_url = imagePaths.coverSmall_url
                                    episode.coverBig_localPath = imagePaths.cover_big_localPath
                                    episode.coverBig_url = imagePaths.cover_big_url
                                    episode.backgroundImage_localPath = imagePaths.backgroundImage_localPath
                                    episode.backgroundImage_url = imagePaths.backgroundImage_url

                                    staffel.Episodes.Add(episode)
                                Next

                                'take first Images from Episode for season
                                If staffel.Episodes.Count > 0 Then
                                    If String.IsNullOrWhiteSpace(staffel.coverSmall_localPath) Then
                                        staffel.coverSmall_localPath = staffel.Episodes(0).coverSmall_localPath
                                        staffel.coverSmall_url = staffel.Episodes(0).coverSmall_url
                                    End If

                                    If String.IsNullOrWhiteSpace(staffel.coverBig_localPath) Then
                                        staffel.coverBig_localPath = staffel.Episodes(0).coverBig_localPath
                                        staffel.coverBig_url = staffel.Episodes(0).coverBig_url
                                    End If
                                    staffel.backgroundImage_localPath = serie.backgroundImage_localPath
                                    staffel.backgroundImage_url = serie.backgroundImage_url

                                    Log.Debug("SkyGoDe: parseJsonSerieResponse staffel.coverBig_localPath2: " & staffel.coverBig_localPath)
                                End If

                                If Not ListOfExistingCovers.ContainsKey(staffel.coverSmall_localPath) Then
                                    ListOfExistingCovers.Add(staffel.coverSmall_localPath, staffel.coverSmall_url)
                                    'hier 29.03.2015
                                    downloadPic(staffel.coverSmall_localPath, staffel.coverSmall_url, "cover")
                                End If

                                serie.Seasons.Add(staffel)

                                Dim onClick As Category.onClickType
                                If category.onClick = PluginConfiguration.Category.onClickType.DokuStaffel_Listing Then
                                    onClick = category.onClickType.DokuEpisode_Listing
                                ElseIf category.onClick = PluginConfiguration.Category.onClickType.SnapStaffel_Listing Then
                                    onClick = category.onClickType.SnapEpisode_Listing
                                Else
                                    onClick = category.onClickType.Episode_Listing
                                End If

                                Dim staffelCategory As Category = New Category(itemId:=staffel.ID, label:=staffel.Title, [alias]:=staffel.ID.ToString, onClick:=onClick, iconImage:=staffel.coverSmall_localPath, thumbnailImage:=staffel.coverBig_localPath, backgroundImage:=staffel.backgroundImage_localPath, backgroundImage_url:=staffel.backgroundImage_url, descriptionType:="serie", description:=serie.Description, subCategories:=New List(Of Category))

                                'now add episodes
                                For Each episode In staffel.Episodes
                                    'no episode description available here, so set type to serie and use serie description as Default
                                    staffelCategory.SubCategories.Add(New Category(itemId:=episode.ID, label:=episode.Title, [alias]:=episode.ID.ToString, onClick:=category.onClickType.Episode, iconImage:=episode.coverSmall_localPath, thumbnailImage:=episode.coverBig_localPath, backgroundImage:=episode.backgroundImage_localPath, backgroundImage_url:=episode.backgroundImage_url, descriptionType:="serie", description:=JSONSerieItem.synopsis))
                                Next
                                menuPage.category.Add(New Category(itemId:=staffel.ID, label:=staffel.Title, [alias]:=staffel.ID.ToString, onClick:=onClick, iconImage:=staffel.coverSmall_localPath, thumbnailImage:=staffel.coverBig_localPath, backgroundImage:=staffel.backgroundImage_localPath, backgroundImage_url:=staffel.backgroundImage_url, descriptionType:=staffelCategory.descriptionType, description:=serie.Description, subCategories:=staffelCategory.SubCategories))
                            Next
                            menuPage.state = Workstate.complete
                        End If
                    End If

                End If
            Catch ex As Exception
                menuPage.state = Workstate.error
            End Try

            Return menuPage
        End Function

        Friend Function parseJsonVideoResponse(ByVal strResponse As String, ByVal strUrl As String, ByVal category As Category) As Page
            Dim menuPage As New Page

            If Not strResponse Is Nothing Then

                Dim DataSet As JSONClass.VideoListingObject = New JavaScriptSerializer().Deserialize(Of JSONClass.VideoListingObject)(strResponse) 'JSONVideoClass.JSONObject

                If Not DataSet Is Nothing Then
                    If Not DataSet.listing Is Nothing Then
                        menuPage.alias = DataSet.listing.type & "/" & DataSet.listing.currentLetter
                        menuPage.currPage = DataSet.listing.currPage
                        menuPage.maxPage = DataSet.listing.pages
                    End If

                    menuPage.Url = strUrl
                    If category.Alias = "nextPage" Then
                        menuPage.selectedListItemIndex = category.ListItemIndex
                    End If
                    If (menuPage.currPage < menuPage.maxPage) Then
                        menuPage.HasNextPage = True
                        menuPage.nextPage = menuPage.currPage + 1
                    End If
                End If

                'get collection
                Dim JSONitems As JSONClass.Asset() = DataSet.listing.asset_listing.asset

                If Not JSONitems Is Nothing Then
                    Dim BackdropImage2Download As New Dictionary(Of String, String)

                    For Each JSONitem As JSONClass.Asset In JSONitems
                        Dim video As New VideoInfo

                        video.ID = JSONitem.id
                        video.Description = JSONitem.synopsis
                        video.VideoUrl = JSONitem.webvod_canonical_url
                        video.Title2 = JSONitem.original_title

                        'benutze Cover der Serie, da Verweis bei der Episode jeweils unterschiedl. Name besitzt, aber gleiches Bild liefern
                        If Not String.IsNullOrWhiteSpace(JSONitem.serie_id) Then
                            JSONitem.dvd_cover.file = "dvd_cover_" & JSONitem.serie_id.ToString & ".jpg"
                            JSONitem.dvd_cover.path = JSONitem.dvd_cover.path + "/series"
                        End If

                        'getImagePaths 
                        Dim imagePaths As ImagePaths = parseImage(JSONitem.dvd_cover, JSONitem.main_picture)
                        video.coverSmall_localPath = imagePaths.coverSmall_localPath
                        video.coverSmall_url = imagePaths.coverSmall_url
                        video.coverBig_localPath = imagePaths.cover_big_localPath
                        video.coverBig_url = imagePaths.cover_big_url
                        video.backgroundImage_localPath = imagePaths.backgroundImage_localPath
                        video.backgroundImage_url = imagePaths.backgroundImage_url

                        If Not ListOfExistingCovers.ContainsKey(video.coverSmall_localPath) Then
                            ListOfExistingCovers.Add(video.coverSmall_localPath, video.coverSmall_url)
                            downloadPic(video.coverSmall_localPath, video.coverSmall_url, "cover")
                        End If

                        If Not ListOfExistingBackdrops.ContainsKey(video.backgroundImage_localPath) And Not BackdropImage2Download.ContainsKey(video.backgroundImage_localPath) Then
                            BackdropImage2Download.Add(video.backgroundImage_localPath, video.backgroundImage_url)
                        End If

                        Dim onClick As Category.onClickType

                        If category.onClick = PluginConfiguration.Category.onClickType.Film_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.Film
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.Serie_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.Staffel_Listing
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.Episode_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.Episode

                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.DokuFilm_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.DokuFilm
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.DokuSerie_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.DokuStaffel_Listing
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.DokuEpisode_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.DokuEpisode

                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.LifestyleSerie_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.LifestyleStaffel_Listing
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.LifestyleEpisode_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.LifestyleEpisode

                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.SnapFilm_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.SnapFilm
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.SnapSerie_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.SnapStaffel_Listing
                        ElseIf category.onClick = PluginConfiguration.Category.onClickType.SnapEpisode_Listing Then
                            onClick = PluginConfiguration.Category.onClickType.SnapEpisode

                        Else
                            Stop
                        End If

                        If JSONitem.asset_type = "Episode" Then
                            video.Title = JSONitem.serie_title & " (S" & JSONitem.season_nr & "E" & JSONitem.episode_nr & ")" '31.01. gechecked
                            video.Description = JSONitem.synopsis
                        ElseIf JSONitem.asset_type = "Film" Then
                            video.Title = JSONitem.title
                        ElseIf JSONitem.asset_type = "Series" Then
                            video.Title = JSONitem.title
                        Else
                            Stop
                        End If

                        menuPage.category.Add(New Category(itemId:=video.ID, label:=video.Title, [alias]:=JSONitem.id.ToString, onClick:=onClick, url:=video.VideoUrl, iconImage:=video.coverSmall_localPath, thumbnailImage:=video.coverBig_localPath, backgroundImage:=video.backgroundImage_localPath, backgroundImage_url:=video.backgroundImage_url, description:=video.Description))
                    Next

                End If
            End If

            Return menuPage
        End Function

        Friend Function parseJsonLocalChannelList(ByVal strResponse As String) As Dictionary(Of String, JSONChannelListClass.ChannelList) 'List
            localChannelList.Clear()

            If Not strResponse Is Nothing Then
                Dim DataSet As JSONChannelListClass.JSONObject = New JavaScriptSerializer().Deserialize(Of JSONChannelListClass.JSONObject)(strResponse)
                If Not DataSet Is Nothing Then

                    For Each item As JSONChannelListClass.ChannelList In DataSet.channelList
                        If item.enable Then
                            localChannelList.Add(item.id, item)
                        End If
                    Next
                End If
            End If

            Return localChannelList
        End Function

        Friend Function parseJsonLiveEventListResponse(ByVal strResponse As String, ByVal category As Category) As Page
            Dim menuPage As New Page
            Dim sortedList As New SortedList(Of Integer, Category)

            If Not strResponse Is Nothing Then
                Try
                    Dim dtNow As DateTime = DateTime.Now
                    Dim DataSet As Dictionary(Of String, List(Of JSONLiveEventListClass.LiveEventList))
                    DataSet = New JavaScriptSerializer().Deserialize(Of Dictionary(Of String, List(Of JSONLiveEventListClass.LiveEventList)))(strResponse)

                    menuPage.name = category.Label
                    menuPage.alias = category.Alias
                    menuPage.Url = category.Url

                    For Each channel As JSONChannelListClass.ChannelList In localChannelList.Values
                        ' Dim channelName As String = channel.name

                        If DataSet.ContainsKey(channel.id) Then
                            'search events 4 current channel
                            Dim channelEvents As List(Of JSONLiveEventListClass.LiveEventList) = DataSet.Item(channel.id)
                            Dim oldEventStart As DateTime = #1/1/2000 1:00:00 AM#
                            Dim oldEventEnd As DateTime = #1/1/2000 1:00:00 AM#

                            For Each channelEvent As JSONLiveEventListClass.LiveEventList In channelEvents
                                Dim dtEventStart As DateTime = CDate(channelEvent.startDate & " " & channelEvent.startTime)
                                Dim dtEventEnd As DateTime = CDate(channelEvent.endDate & " " & channelEvent.endTime)
                                Dim resultStart As Integer = DateTime.Compare(dtEventStart, dtNow)
                                Dim resultEnd As Integer = DateTime.Compare(dtEventEnd, dtNow)
                 
                                If resultStart < 0 Then
                                    'start des events ist früher als jetzt
                                    If resultEnd < 0 Then
                                        'ende  des events ist früher als jetzt
                                    Else
                                        'Dieses Event ist nun für das Menue interessant
                                        Dim url_Manifest As String
                                        Dim event_id As String = ""
                                        Dim itemId As String
                                        Dim apix_id As String = ""
                                        Dim coverBig_url As String = ""
                                        Dim coverSmall_url As String = ""
                                        Dim description As String = ""
                                        'http://www.skygo.sky.de/epgd/sg/ipad/eventDetail/84289457/118/
                                        Dim url_eventDetail As String = url_skygode_eventDetail & channelEvent.id & "/" & channel.id & "/"
                                        Dim JSONEventDetail As JSONEventDetailClass.JSONObject

                                        JSONEventDetail = GetWebEventDetailData(url_eventDetail)

                                        If JSONEventDetail.returnCode = ResponseCode.Ok Then

                                            description = channelEvent.startTime & " - " & channelEvent.endTime & Environment.NewLine & channelEvent.subtitle
                                            If Not String.IsNullOrWhiteSpace(JSONEventDetail.detailTxt) Then
                                                description = description & Environment.NewLine & Environment.NewLine & JSONEventDetail.detailTxt
                                            End If

                                            If Not String.IsNullOrWhiteSpace(channel.logo) Then
                                                If File.Exists(folder_Thumbs_Plugin_Icons_Channels & channel.logo) Then
                                                    coverSmall_url = folder_Thumbs_Plugin_Icons_Channels & channel.logo
                                                End If
                                            End If

                                            If Not String.IsNullOrWhiteSpace(JSONEventDetail.imageUrl) Then
                                                coverBig_url = url_skygode & JSONEventDetail.imageUrl
                                            End If

                                            If String.IsNullOrWhiteSpace(channel.mediaurl) Then
                                                url_Manifest = ""
                                            Else
                                                url_Manifest = channel.mediaurl
                                            End If

                                            If Not String.IsNullOrWhiteSpace(JSONEventDetail.assetid) Then
                                                itemId = JSONEventDetail.assetid.ToString
                                            Else
                                                itemId = "0"
                                            End If

                                            If Not String.IsNullOrWhiteSpace(JSONEventDetail.assetid) Then
                                                apix_id = channelEvent.id
                                                event_id = channel.id
                                            ElseIf Not String.IsNullOrWhiteSpace(channel.skygoid) Then
                                                apix_id = channel.skygoid
                                            Else
                                                apix_id = ""
                                            End If

                                            'Workaround Start
                                            'weil SkyGo teilweise für die gleiche Zeit und den gleichen Channel 2 Sendungen über JSON sendet
                                            If DateTime.Compare(oldEventStart, dtEventStart) = 0 Or DateTime.Compare(oldEventEnd, dtEventEnd) = 0 Then
                                                'remove already stored entry
                                                sortedList.Remove(sortedList.Count - 1)
                                            End If

                                            oldEventStart = dtEventStart
                                            oldEventEnd = dtEventEnd
                                            'Workaround Ende

                                            If Not String.IsNullOrWhiteSpace(apix_id) Then
                                                sortedList.Add(sortedList.Count, New Category(itemId:=itemId, label:=channel.name, [alias]:=menuPage.alias & "/" & channelEvent.title.ToString, onClick:=category.onClickType.LiveStream, event_id:=event_id, apix_id:=apix_id, url_Manifest:=url_Manifest, thumbnailImage:=coverBig_url, iconImage:=coverSmall_url, descriptionType:="detail", description:=description))
                                            End If

                                        End If
                                    End If

                                End If
                            Next

                        End If

                    Next

                    For Each item In sortedList
                        menuPage.category.Add(item.Value)
                    Next
                Catch ex As Exception
                    menuPage.state = Workstate.error
                End Try
            End If

            Return menuPage
        End Function

        Friend Function parseJsonChannelListResponse(ByVal strResponse As String, ByVal category As Category) As Page
            Dim menuPage As New Page

            If Not strResponse Is Nothing Then
                Dim DataSet As JSONChannelListClass.JSONObject = New JavaScriptSerializer().Deserialize(Of JSONChannelListClass.JSONObject)(strResponse)
                If Not DataSet Is Nothing Then
                    menuPage.alias = category.Alias
                    menuPage.Url = category.Url
                    menuPage.type = State.categories
                    Dim dic_channelList As New Dictionary(Of String, JSONChannelListClass.ChannelList)

                    For Each item As JSONChannelListClass.ChannelList In DataSet.channelList
                        If Not (String.IsNullOrEmpty(item.mediaurl)) Then

                            If Not dic_channelList.ContainsKey(item.mediaurl) Then
                                dic_channelList.Add(item.mediaurl, item)
                                Dim coverBig_url As String = url_skygode & item.logo
                                menuPage.category.Add(New Category(itemId:=item.id, label:=item.name.ToString, [alias]:=menuPage.alias & "/" & item.name.ToString, iconImage:=coverBig_url, thumbnailImage:=coverBig_url, onClick:=category.onClickType.LiveStream, url_Manifest:=item.mediaurl))
                            End If
                        End If
                    Next
                End If
            End If

            Return menuPage
        End Function

        Public Function parseImage(ByVal JsonPic As JSONClass.PictureInfo, ByVal JsonPicList As JSONClass.PictureInfos) As ImagePaths
            Dim imagePaths As New ImagePaths

            'get list items
            If Not String.IsNullOrWhiteSpace(JsonPic.file) Then
                imagePaths.coverSmall_url = url_skygode & JsonPic.path & "/" & JsonPic.file
                imagePaths.coverSmall_localPath = Path.Combine(folder_Thumbs_Plugin_Cache_SkyGo_Covers, JsonPic.file)

                'use list items as cover (left side/thumbnailImage)
                imagePaths.cover_big_url = imagePaths.coverSmall_url
                imagePaths.cover_big_localPath = imagePaths.coverSmall_localPath
            End If

            'get background
            If JsonPicList.picture.Count = 0 Then
                imagePaths.backgroundImage_url = ""
                imagePaths.backgroundImage_localPath = ""
            Else
                imagePaths.backgroundImage_url = url_skygode & JsonPicList.picture(JsonPicList.picture.Count - 1).path & "/" & JsonPicList.picture(JsonPicList.picture.Count - 1).file
                imagePaths.backgroundImage_localPath = Path.Combine(folder_Thumbs_Plugin_Cache_SkyGo_Backdrops, JsonPicList.picture(JsonPicList.picture.Count - 1).file)
            End If

            Return imagePaths
        End Function

    End Module
End Namespace

