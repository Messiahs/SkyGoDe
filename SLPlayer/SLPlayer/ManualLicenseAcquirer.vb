Imports System.IO
Imports System.Windows.Threading
' makes license request explicitly
Public Class ManualLicenseAcquirer
    Inherits LicenseAcquirer

    Private WithEvents _playerPage As PlayerPage
    Private challengeString As String
    Private tmr As New DispatcherTimer
    Private request As HttpWebRequest
    Delegate Sub Message2GuiDelegate(myArg1 As String, myArg2 As String)

    Public Sub New(ByVal playerPage As PlayerPage)
        MyBase.New()
        _playerPage = playerPage
    End Sub

    Private Sub TimerFired(sender As Object, e As EventArgs)
        If Not request.HaveResponse Then
            request.Abort()
        End If

        tmr.Stop()

    End Sub
    ' The default implementation of OnAcquireLicense calls into the MediaElement to acquire a
    ' license. It is called when the Media pipeline is building a topology and will be raised
    ' before MediaOpened is raised.
    Protected Overrides Sub OnAcquireLicense(ByVal licenseChallenge As System.IO.Stream, ByVal licenseServerUri As Uri)
        Dim sr As StreamReader = New StreamReader(licenseChallenge)

        challengeString = sr.ReadToEnd

        ' Need to resolve the URI for the License Server -- make sure it is correct
        ' and store that correct URI as resolvedLicenseServerUri.
        Dim resolvedLicenseServerUri As Uri
        If (LicenseServerUriOverride Is Nothing) Then
            resolvedLicenseServerUri = licenseServerUri
        Else
            resolvedLicenseServerUri = LicenseServerUriOverride
        End If

        'timeout
        tmr = New DispatcherTimer()
        tmr.Interval = TimeSpan.FromSeconds(5)
        AddHandler tmr.Tick, AddressOf Me.TimerFired

        ' Make a HttpWebRequest to the License Server.
        Try
            request = CType(WebRequest.Create(resolvedLicenseServerUri), HttpWebRequest)
            ' The headers below are necessary so that error handling and redirects are handled 
            ' properly via the Silverlight client.
            request.Method = "POST"
            request.ContentType = "application/xml"
            request.Headers("msprdrm_server_redirect_compat") = "false"
            request.Headers("msprdrm_server_exception_compat") = "false"
            request.Headers("UserAgent") = "Dalvik/2.1.0 (Linux; U; Android 5.0; Nexus 7 Build/LRX21P)"

            'Initiate getting request stream  
            Dim asyncResult As IAsyncResult = request.BeginGetRequestStream(New AsyncCallback(AddressOf RequestStreamCallback), request)
            tmr.Start()
        Catch ex As Exception
            Stop
        End Try
    End Sub

    ' This method is called when the asynchronous operation completes.
    Private Sub RequestStreamCallback(ByVal ar As IAsyncResult)
        Dim request As HttpWebRequest = CType(ar.AsyncState, HttpWebRequest)
        ' populate request stream  
        request.ContentType = "text/xml"
        Dim requestStream As Stream = request.EndGetRequestStream(ar)
        Dim streamWriter As StreamWriter = New StreamWriter(requestStream, System.Text.Encoding.UTF8)
        streamWriter.Write(challengeString)
        streamWriter.Close()
        ' Make async call for response  
        request.BeginGetResponse(New AsyncCallback(AddressOf ResponseCallback), request)
    End Sub

    Private Sub ResponseCallback(ByVal ar As IAsyncResult)
        request = CType(ar.AsyncState, HttpWebRequest)

        If request.HaveResponse Then
            Dim response As WebResponse = request.EndGetResponse(ar)
            Dim responseStream As IO.Stream = response.GetResponseStream
            'get license body        
            Dim sr As StreamReader = New StreamReader(responseStream, System.Text.Encoding.UTF8)
            Dim xmlString As String = sr.ReadToEnd()

            Try
                Dim xmlDocument As XDocument = XDocument.Parse(xmlString)
                Dim rootElement As XElement = xmlDocument.Root.Element("{http://schemas.xmlsoap.org/soap/envelope/}Body")
                Dim faultElement As XElement = rootElement.Element("{http://schemas.xmlsoap.org/soap/envelope/}Fault")

                If Not faultElement Is Nothing Then
                    Dim Params(1) As Object
                    Params(1) = "OnAcquireLicense"
                    Dim faultstring As XElement = faultElement.Element("faultstring")
                    Dim exceptionNode As XElement = faultElement.Element("detail").Element("Exception")
                    Dim statusCode As XElement = exceptionNode.Element("StatusCode")
                    Dim customData As XElement = exceptionNode.Element("CustomData")
                    Dim serviceId As XElement = exceptionNode.Element("ServiceId")
                    Dim redirectUrl As XElement = exceptionNode.Element("RedirectUrl")

                    'http://playready.directtaps.net/pr/doc/devicecore/servererrors.html
                    If (faultstring.Value.StartsWith("System.Web.Services.Protocols.SoapException: Domain Required")) Then
                        If statusCode.Value = "0x8004c605" Then
                            Params(0) = "Dieser PC ist noch nicht aktiviert." & Environment.NewLine & "Zur Aktivierung einen Film über die normale 'SkyGo.de Homepage' starten."
                        ElseIf Not String.IsNullOrWhiteSpace(customData.Value) Then
                            Params(0) = customData.Value
                        Else
                            Params(0) = "Es ist ein unbekannter Fehler aufgetreten (1)."
                        End If
                    ElseIf faultstring.Value.StartsWith("System.Web.Services.Protocols.SoapException: Service Specific") Then
                        'ggf. Sitzung abgelaufen
                        If (customData.Value.Length <> 0) Then
                            Params(0) = customData.Value
                        Else
                            Params(0) = "Es ist ein unbekannter Fehler aufgetreten (2)."
                        End If
                    Else
                        Params(0) = "Es ist ein unbekannter Fehler aufgetreten (3)."
                    End If
                    _playerPage.Dispatcher.BeginInvoke(New Message2GuiDelegate(AddressOf _playerPage.Message2Gui), Params)
                Else
                    'alles OK
                    responseStream.Seek(0, SeekOrigin.Begin)
                    SetLicenseResponse(response.GetResponseStream)
                End If

            Catch exception As System.Exception
                Stop
            End Try
        Else
            Dim Params(1) As Object
            Params(0) = "Zeitüberschreitung während Verbindungsaufbau."
            Params(1) = "OnAcquireLicense"
            _playerPage.Dispatcher.BeginInvoke(New Message2GuiDelegate(AddressOf _playerPage.Message2Gui), Params)
        End If

    End Sub

End Class
