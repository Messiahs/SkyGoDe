Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Runtime.Serialization.Formatters.Binary
Imports SkyGoDe.PluginConfiguration
Imports SkyGoDe.JSONClasses
Imports SkyGoDe.JSONParse
Imports System.Web.Script.Serialization
Imports System.Collections
Imports System.Threading.Tasks
Imports MediaPortal.GUI.Library
Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices
Imports System.Reflection

Namespace SkyGoDe

    Public Enum LoginReturnCode
        Init = -1
        Ok = 0
        AlreadyLoggedIn = 10
        [Error] = 99
        TryAgain = 150
    End Enum

    Public Class LoginResponseStruct
        Friend [string] As String
        Friend returnCode As LoginReturnCode = LoginReturnCode.Init
    End Class

    Public Enum ProcessState
        Init = 0
        loginOK = 110
        loginTryAgain = 150
        loginAlreadyLoggedIn = 151
        loginError = 190

    End Enum

    Module SkyGoDeWeb
        Dim _urlLogin As String = "https://www.skygo.sky.de/SILK/services/public/session/login?version=12354&platform=web&product=SG&[UserIdType]=[UserId]&password=[PIN]&remMe=true&callback=_jqjsp&_[Seconds]="
        Dim _urlKillSession As String = "https://www.skygo.sky.de/SILK/services/public/session/kill/web?version=12354&platform=web&product=SG&callback=_jqjsp&_[Seconds]="

        Friend Function MainLoginHandler(ByVal UserID As String, ByVal PIN As String) As LoginResponseStruct
            Dim response As ResponseStruct
            Dim loginResponse As New LoginResponseStruct

            response = Login_RequestLogin(UserID, PIN)

            If response.returnCode = ResponseCode.Ok Then
                loginResponse = parseLoginResponse(response.string)
                ' 12.04. ganzer Block von unten hoch kopiert
                If loginResponse.returnCode = LoginReturnCode.Error Then 'neu 12.04.2015
                    loginResponse.returnCode = LoginReturnCode.Error
                End If

                If loginResponse.returnCode = LoginReturnCode.AlreadyLoggedIn Then
                    response = Login_KillSession(UserID, PIN)
                    If response.returnCode = ResponseCode.Ok Then
                        loginResponse = parseLoginResponse(response.string)
                    End If
                End If

                If loginResponse.returnCode = LoginReturnCode.TryAgain Then
                    response = Login_RequestLogin(UserID, PIN)
                    If response.returnCode = ResponseCode.Ok Then
                        loginResponse = parseLoginResponse(response.string)
                    End If
                End If

            ElseIf response.returnCode = ResponseCode.Error Then
                loginResponse.returnCode = LoginReturnCode.Error
                loginResponse.string = response.string
            End If

            Return loginResponse
        End Function

        Private Function Login_RequestLogin(ByVal UserID As String, ByVal PIN As String) As ResponseStruct 'String
            Dim response As New ResponseStruct
            Dim Seconds As String = DateTime.Now.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds.ToString("##########")
            Dim _cookieContainer As CookieContainer = New CookieContainer()

            Dim strUrl As String
            If Regex.IsMatch(UserID, "^[0-9 ]+$") Then
                strUrl = _urlLogin.Replace("[UserIdType]", "customerCode")
            Else
                strUrl = _urlLogin.Replace("[UserIdType]", "email")
            End If

            strUrl = strUrl.Replace("[UserId]", UserID)
            strUrl = strUrl.Replace("[PIN]", PIN)
            strUrl = strUrl.Replace("[Seconds]", Seconds)

            Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(strUrl), HttpWebRequest)
            httpRequest = buildHeader(httpRequest)
            httpRequest.CookieContainer = _cookieContainer

            Try
                Using httpResponse As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
                    If _cookieContainer.Count > 0 Then
                        Dim Container As New CookieContainer
                        Container.Add(New Uri("https://www.skygo.sky.de"), httpResponse.Cookies)
                        SaveCookies(Container)
                    End If

                    Using reader As New StreamReader(httpResponse.GetResponseStream())
                        response.string = reader.ReadToEnd()
                    End Using

                    response.returnCode = ResponseCode.Ok
                End Using

            Catch ex As Exception
                response.string = ex.Message
                response.returnCode = ResponseCode.Error
            End Try

            Return response
        End Function

        Private Function Login_KillSession(ByVal user As String, ByVal password As String) As ResponseStruct
            Dim response As New ResponseStruct
            Dim _cookieContainer As CookieContainer = New CookieContainer()
            Dim Seconds As String = DateTime.Now.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds.ToString("##########")
            Dim strUrl As String = _urlKillSession.Replace("[Seconds]", Seconds)

            Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(strUrl), HttpWebRequest)
            httpRequest = buildHeader(httpRequest)

            Dim cookContainer As CookieContainer
            Dim cookCollection As CookieCollection
            cookContainer = LoadCookies()
            cookCollection = cookContainer.GetCookies(New Uri("https://www.skygo.sky.de"))

            For Each cook As Cookie In cookCollection
                Dim bRet As Boolean = NativeMethods.InternetSetCookie("https://www.skygo.sky.de/", cook.Name, cook.Value)
            Next

            httpRequest.CookieContainer = cookContainer

            Try
                Using httpResponse As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
                    If _cookieContainer.Count > 0 Then
                        Dim Container As New CookieContainer
                        Container.Add(httpResponse.Cookies)
                    End If

                    Using reader As New StreamReader(httpResponse.GetResponseStream())
                        response.string = reader.ReadToEnd()
                    End Using

                    response.returnCode = ResponseCode.Ok
                End Using

            Catch ex As Exception
                response.string = ex.Message
                response.returnCode = ResponseCode.Error
            End Try

            Return response
        End Function

        Private Function buildHeader(ByVal httpRequest As HttpWebRequest) As HttpWebRequest
            Dim byteArray() As Byte = Encoding.UTF8.GetBytes("")
            httpRequest.Method = "GET"
            httpRequest.Accept = "*/*"
            httpRequest.UserAgent = "Dalvik/2.1.0 (Linux; U; Android 5.0; Nexus 7 Build/LRX21P)"
            httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "de-DE,de;q=0.8,en-US;q=0.6,en;q=0.4,fr;q=0.2")
            httpRequest.ContentLength = byteArray.Length
            httpRequest.Timeout = 5000 'don't wait longer than 5 seconds for an image

            Return httpRequest
        End Function

        Public Sub SaveCookies(ByVal cookContainer As CookieContainer)
            Dim fileFullPath As String = Path.Combine(Path.GetTempPath(), "SkyGoGer.tmp")
            Using fs As FileStream = File.Create(fileFullPath)
                Try
                    Dim formatter As New BinaryFormatter()
                    formatter.Serialize(fs, cookContainer)
                    ' For Each cook As Cookie In cookContainer.GetCookies(New Uri("https://www.skygo.sky.de"))
                    ' Debug.Print("----------Save Cookies------------------ ")
                    '  Debug.Print("Cookie: " & cook.Name)
                    '  Debug.Print("Value: " & cook.Value)
                    '  Debug.Print("Expires: " & cook.Expires.ToString)
                    ' Debug.Print("Expired: " & cook.Expired)
                    '  Debug.Print("Domain: " & cook.Domain)
                    '  Debug.Print("Secure: " & cook.Secure)
                    ' Next
                Catch e As Exception
                    Stop
                End Try
            End Using
        End Sub

        Public Function LoadCookies() As CookieContainer
            Dim cookContainer As New CookieContainer

            Dim fileFullPath As String = Path.Combine(Path.GetTempPath(), "SkyGoGer.tmp")
            If System.IO.File.Exists(fileFullPath) Then
                Try
                    Using fs As FileStream = File.Open(fileFullPath, FileMode.Open)
                        Dim formatter As New BinaryFormatter()
                        cookContainer = DirectCast(formatter.Deserialize(fs), CookieContainer)
                        '  For Each cook As Cookie In cookContainer.GetCookies(New Uri("https://www.skygo.sky.de"))
                        'Debug.Print("----------Load Cookies------------------ ")
                        '  Debug.Print("Cookie: " & cook.Name)
                        '  Debug.Print("Value: " & cook.Value)
                        '  Debug.Print("Expires: " & cook.Expires.ToString)
                        '  Debug.Print("Expired: " & cook.Expired)
                        '  Debug.Print("Domain: " & cook.Domain)
                        '  Debug.Print("Secure: " & cook.Secure)
                        '  Next

                    End Using
                Catch e As Exception
                    Stop
                End Try
            End If

            Return cookContainer
        End Function

        Private Function parseLoginResponse(ByVal response As String) As LoginResponseStruct
            Dim loginResponse As New LoginResponseStruct
            Dim responseArr As String() = response.Replace("_jqjsp({", "").Split(CChar(","))

            If response.Contains("<p>The document has moved <a href=""http://www.skygo.sky.de/wartung_error.html") Then
                loginResponse.returnCode = LoginReturnCode.Error
                loginResponse.string = "Aufgrund von Wartungsarbeiten steht Ihnen Sky Go aktuell nicht zur Verfügung."
            Else
                For Each element As String In responseArr
                    Dim responseArr1 As String() = element.ToString.Split(CChar(":"))
                    If responseArr1(0).Contains("resultCode") Then
                        Select Case responseArr1(1)
                            Case """T_100""" 'Login ok
                                If responseArr.Length > 5 Then 'logon
                                    loginResponse.returnCode = LoginReturnCode.Ok
                                Else 'logoff/kill OK -> lets try again
                                    loginResponse.returnCode = LoginReturnCode.TryAgain
                                End If

                                Exit For
                            Case """T_206""" 'User already logged in
                                loginResponse.returnCode = LoginReturnCode.AlreadyLoggedIn
                                Exit For
                            Case """S_217""" 'Sessionfehler SkySessionId null
                                loginResponse.returnCode = LoginReturnCode.Error
                                Exit For
                            Case """S_218""" 'Customer Code not found in SilkCache
                                loginResponse.returnCode = LoginReturnCode.Error
                                Exit For

                            Case """S_236""" 'Android Geräte-ID fehlt
                                loginResponse.returnCode = LoginReturnCode.Error
                                Exit For
                            Case """999""" '-> allg. Fehler bei der Anmeldung (SkyGo hat Probleme)
                                loginResponse.returnCode = LoginReturnCode.Error
                                loginResponse.string = "techn. Fehler bei der Anmeldung (Wartungsarbeiten)"
                                Exit For
                            Case Else '999' -> techn. fehler ' keine FSk eingegeben 
                                'http://www.skygo.sky.de//sg/multiplatform/android_tablet/json/editorial/silkMessages.json
                                loginResponse.returnCode = LoginReturnCode.Error
                                Exit For
                        End Select
                    ElseIf responseArr1(0).Contains("skygoSessionId") Then
                        'get skygoSessionId
                        _SLPlayerStartupArgs.sessionId = responseArr1(1).Replace("""", "")
                    End If
                Next
            End If
            Return loginResponse
        End Function

        Friend Function UpdateUrl(ByVal Url As String, ByVal actionType As Category.onClickType) As String
            'Update Url with Snap or SkyGo ?
            Dim SkyGoOrSnap As String

            If actionType = Category.onClickType.SnapFilm Or _
               actionType = Category.onClickType.SnapEpisode Or _
               actionType = Category.onClickType.SnapFilm_Listing Or _
               actionType = Category.onClickType.SnapSerie_Listing Or _
               actionType = Category.onClickType.SnapStaffel_Listing Or _
               actionType = Category.onClickType.SnapEpisode_Listing Or _
               actionType = Category.onClickType.SnapFilmLetter_Listing Or _
               actionType = Category.onClickType.SnapSerieLetter_Listing Then
                SkyGoOrSnap = "snap"
            Else
                SkyGoOrSnap = "sg"
            End If

            Url = Url.Replace("[SkyGoOrSnap]", SkyGoOrSnap)

            Return Url
        End Function
        Friend Function GetWebData(ByVal actionType As Category.onClickType, ByVal category As Category) As Page
            Dim menuPage As New Page
            Dim strUrl As String = category.Url
            Dim newUrl As String

            menuPage.state = Workstate.working

            If category.Alias = "nextPage" Then
                menuPage = pageStore.ElementAt(pageStore.Count - 1).Value
            End If

            If actionType = category.onClickType.Staffel_Listing _
                Or actionType = category.onClickType.DokuStaffel_Listing _
                Or actionType = category.onClickType.LifestyleStaffel_Listing _
                Or actionType = category.onClickType.SnapStaffel_Listing Then
                newUrl = url_skygode_json_detailsSeries & category.ItemId & "_global.json"
            ElseIf actionType = category.onClickType.LiveEvent_Listing Then
                Dim channelId As New ArrayList
                For Each key As String In localChannelList.Keys
                    channelId.Add(key)
                Next
                Dim allChannelIDs As String = String.Join(",", channelId.ToArray)
                Dim strDatum As String = String.Format(DateTime.Today.ToString("d"), "dd.MM.yyyy")
                newUrl = url_skygode_eventList & strDatum & "/" & allChannelIDs & "/"
                '  ElseIf actionType = PluginConfiguration.Category.onClickType.SnapFilm_Listing Or _
                '  actionType = PluginConfiguration.Category.onClickType.SnapSerie_Listing Then
                '  newUrl = url_skygode_Snap_automaticlisting & strUrl.Replace("[page]", Convert.ToString(menuPage.nextPage))
            Else
                newUrl = url_skygode_json_automaticlisting & strUrl.Replace("[page]", Convert.ToString(menuPage.nextPage))
            End If

            newUrl = UpdateUrl(newUrl, actionType)

            Dim response As ResponseStruct = loadWebData(newUrl)

            If response.returnCode = ResponseCode.Ok Then
                If actionType = category.onClickType.FilmLetter_Listing _
                    Or actionType = category.onClickType.SerieLetter_Listing _
                    Or actionType = category.onClickType.DokuFilmLetter_Listing _
                    Or actionType = category.onClickType.DokuSerieLetter_Listing _
                    Or actionType = category.onClickType.LifestyleSerieLetter_Listing _
                    Or actionType = category.onClickType.SnapFilmLetter_Listing _
                    Or actionType = category.onClickType.SnapSerieLetter_Listing Then
                    menuPage = parseJsonLetterResponse(response.string, category)
                    menuPage.type = State.categories
                ElseIf actionType = category.onClickType.Film_Listing _
                    Or actionType = category.onClickType.Serie_Listing _
                    Or actionType = category.onClickType.Episode_Listing _
                    Or actionType = category.onClickType.DokuFilm_Listing _
                    Or actionType = category.onClickType.DokuSerie_Listing _
                    Or actionType = category.onClickType.DokuEpisode_Listing _
                    Or actionType = category.onClickType.LifestyleSerie_Listing _
                    Or actionType = category.onClickType.LifestyleEpisode_Listing _
                    Or actionType = category.onClickType.SnapFilm_Listing _
                    Or actionType = category.onClickType.SnapSerie_Listing _
                    Or actionType = category.onClickType.SnapEpisode_Listing Then
                    menuPage = parseJsonVideoResponse(response.string, strUrl, category)
                    menuPage.type = State.videos
                ElseIf actionType = category.onClickType.Staffel_Listing _
                    Or actionType = category.onClickType.DokuStaffel_Listing _
                    Or actionType = category.onClickType.LifestyleStaffel_Listing _
                    Or actionType = category.onClickType.SnapStaffel_Listing Then
                    menuPage = parseJsonSerieResponse(response.string, strUrl, category)
                    menuPage.type = State.serie
                ElseIf actionType = category.onClickType.LiveEvent_Listing Then
                    menuPage = parseJsonLiveEventListResponse(response.string, category)
                    menuPage.type = State.livestream
                Else
                    Stop
                End If
            End If

            If Not cts.IsCancellationRequested And response.returnCode = ResponseCode.Ok Then
                menuPage.state = Workstate.complete
            End If

            Return menuPage
        End Function

        Friend Function GetWebAdditionalData(ByVal actionType As Category.onClickType, ByVal category As Category) As ResponseStruct
            Dim strUrl As String = category.Url
            Dim newUrl As String = ""
            Dim categoryStruct As New ResponseStruct
            Dim response As ResponseStruct

            If actionType = category.onClickType.Film _
                Or actionType = category.onClickType.Episode _
                Or actionType = category.onClickType.DokuFilm _
                Or actionType = category.onClickType.DokuEpisode _
                Or actionType = category.onClickType.LifestyleEpisode _
                Or actionType = category.onClickType.SnapFilm _
                Or actionType = category.onClickType.SnapEpisode _
                Or actionType = category.onClickType.LiveStream Then
                'http://www.skygo.sky.de/sg/multiplatform/ipad/json/details/asset/120703.json
                newUrl = url_skygode_json_detailsAsset & category.ItemId & ".json"
            Else
                Stop
            End If

            newUrl = UpdateUrl(newUrl, actionType)

            response = loadWebData(newUrl)

            If response.returnCode = ResponseCode.Ok Then
                Dim DataSet As JSONClass.AdditionalDataObject = New JavaScriptSerializer().Deserialize(Of JSONClass.AdditionalDataObject)(response.string)

                If Not DataSet Is Nothing Then
                    category.description = DataSet.asset.synopsis
                    category.descriptionType = "detail"
                    category.apix_id = DataSet.asset.event_id.ToString
                    category.product = DataSet.product

                    If actionType = category.onClickType.Film _
                        Or actionType = category.onClickType.Episode _
                        Or actionType = category.onClickType.DokuFilm _
                        Or actionType = category.onClickType.DokuEpisode _
                        Or actionType = category.onClickType.LifestyleEpisode _
                        Or actionType = category.onClickType.SnapFilm _
                        Or actionType = category.onClickType.SnapEpisode Then
                        category.Url_Manifest = DataSet.asset.ms_media_url
                        category.event_id = DataSet.asset.event_id.ToString

                    ElseIf actionType = category.onClickType.LiveStream Then
                        category.Url_Manifest = DataSet.asset.media_url
                        category.event_id = DataSet.asset.id.ToString
                    Else
                        Stop
                    End If

                    categoryStruct.category = category
                    categoryStruct.returnCode = ResponseCode.Ok
                End If
            Else
                categoryStruct.returnCode = ResponseCode.Error
                categoryStruct.string = response.string
            End If

            Return categoryStruct
        End Function

        Friend Function GetWebEventDetailData(ByVal strUrl As String) As JSONEventDetailClass.JSONObject
            Dim EventDetail As New JSONClasses.JSONEventDetailClass
            Dim JSONEventDetail As New JSONEventDetailClass.JSONObject

            Dim response As ResponseStruct = loadWebData(strUrl)

            If response.returnCode = ResponseCode.Ok Then
                Dim DataSet As JSONEventDetailClass.JSONObject = New JavaScriptSerializer().Deserialize(Of JSONEventDetailClass.JSONObject)(response.string)

                If Not DataSet Is Nothing Then 'asset
                    JSONEventDetail.assetid = DataSet.assetid
                    JSONEventDetail.detailPage = DataSet.detailPage
                    JSONEventDetail.detailTxt = DataSet.detailTxt
                    JSONEventDetail.imageUrl = DataSet.imageUrl
                    JSONEventDetail.cmsid = DataSet.cmsid
                    JSONEventDetail.license = DataSet.license
                    JSONEventDetail.returnCode = ResponseCode.Ok
                Else
                    JSONEventDetail.returnCode = ResponseCode.Error
                End If
            Else
                JSONEventDetail.returnCode = response.returnCode
                SkyGoDe.GUIMain.Message2Gui(response.string, "GetWebEventDetailData")
            End If

            Return JSONEventDetail
        End Function

        Friend Function loadWebData(ByVal strUrl As String) As ResponseStruct
            Dim response As New ResponseStruct
            Dim client As WebClient = New WebClient()
            client.Encoding = System.Text.Encoding.UTF8
            If Not String.IsNullOrWhiteSpace(strUrl) Then
                Try
                    response.string = client.DownloadString(New Uri(strUrl))
                    response.string = response.string.Replace("\u0026", "&")
                    response.string = System.Web.HttpUtility.HtmlDecode(response.string)
                    response.returnCode = ResponseCode.Ok
                Catch ex As Exception
                    response.string = ex.Message
                    response.returnCode = ResponseCode.Error
                End Try
            End If

            Return response
        End Function

        Private Sub downloadPics(ByVal imageList As Dictionary(Of String, String), ByVal imageType As String)
            For Each imagePair As KeyValuePair(Of String, String) In imageList
                If Not cts.IsCancellationRequested Then
                    downloadPic(imagePair.Key, imagePair.Value, imageType)
                End If
            Next
        End Sub

        Friend Sub downloadPic(ByVal path As String, ByVal url As String, ByVal imageType As String)
            Try
                Dim t As Task = factory.StartNew(Sub()
                                                     Try
                                                         If cts.Token.IsCancellationRequested Then
                                                             cts.Token.ThrowIfCancellationRequested()
                                                         Else
                                                             Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
                                                             httpRequest = buildHeader(httpRequest)

                                                             Using httpResponse As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
                                                                 If Not httpRequest Is Nothing Then
                                                                     Dim responseStream As System.IO.Stream = httpResponse.GetResponseStream()
                                                                     If Not responseStream.CanRead Then
                                                                         Stop
                                                                     End If
                                                                     Dim image As System.Drawing.Image = System.Drawing.Image.FromStream(responseStream, True, True)
                                                                     image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg)

                                                                     'delete webPage, so we know image is downloaded
                                                                     If imageType = "cover" Then
                                                                         ListOfExistingCovers.Item(path) = ""
                                                                     End If

                                                                     image.Dispose()
                                                                     responseStream.Dispose()
                                                                 End If
                                                             End Using
                                                         End If

                                                     Catch ex As System.Net.WebException
                                                     Catch ex As System.OperationCanceledException
                                                     Catch ex As System.ArgumentException 'falls kein pic zurückgeliefert wird
                                                         'remove path from existing store list
                                                         If ListOfExistingCovers.ContainsKey(path) Then
                                                             If imageType = "cover" Then
                                                                 ListOfExistingCovers.Remove(path)
                                                             Else
                                                                 Stop
                                                             End If

                                                         End If
                                                     Catch ex As Exception
                                                         Stop
                                                         '   Message2Gui(ex.Message)
                                                     End Try

                                                 End Sub, cts.Token)
                tasks.Add(t)
                ' Log.Debug("SkyGoDe: t.Id:" & t.Id)
                Log.Debug("SkyGoDe: downloadPic t.Id: " & t.Id & " url:" & url & "  type:" & imageType & " path: " & path)
            Catch ex As Exception
                Stop
                '  Message2Gui(ex.Message)
            End Try

        End Sub

        Friend Sub downloadBackdrops(ByVal path As String, ByVal url As String, ByVal imageType As String)
            Try
                Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
                httpRequest = buildHeader(httpRequest)

                Using httpResponse As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
                    If Not httpRequest Is Nothing Then
                        Dim responseStream As System.IO.Stream = httpResponse.GetResponseStream()
                        Dim image As System.Drawing.Image = System.Drawing.Image.FromStream(responseStream, True, True)
                        image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg)
                        image.Dispose()
                        responseStream.Dispose()
                    End If
                End Using

            Catch ex As System.Net.WebException
            Catch ex As System.OperationCanceledException
            Catch ex As System.ArgumentException 'falls kein pic zurückgeliefert wird
                'remove path from existing store list
                If ListOfExistingCovers.ContainsKey(path) Then
                    If imageType = "cover" Then
                        ListOfExistingBackdrops.Remove(path)
                    Else
                        Stop
                    End If
                End If
            Catch ex As Exception
                SkyGoDe.GUIMain.Message2Gui(ex.Message, "downloadBackdrops")
            End Try

        End Sub
    End Module
End Namespace

