Imports Microsoft.SilverlightMediaFramework.Core
Imports System.IO
Imports System.Windows.Resources
Imports System.Text
Imports SLPlayer.PlayerPage
Imports System.Windows.Interop
Imports System.Threading

Partial Public Class App
    Inherits Application
    Private WithEvents playerPage As PlayerPage

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Application_Startup(ByVal o As Object, ByVal e As StartupEventArgs) Handles Me.Startup
        '    If Application.Current.InstallState = InstallState.NotInstalled Then
        Me.RootVisual = New InstallPage()
        '  Else
        playerPage = New PlayerPage
        Me.RootVisual = playerPage
        Me.RootVisual.Dispatcher.BeginInvoke(New Action(AddressOf PlayerPage_Loaded)) 'Wichtig
        '  End If
    End Sub

    Private Sub PlayerPage_Loaded()
        If (Application.Current.IsRunningOutOfBrowser AndAlso Application.Current.HasElevatedPermissions) Then
            Application.Current.Host.Content.FullScreenOptions = FullScreenOptions.StaysFullScreenWhenUnfocused
            'next line opens second window... that's the way Silverlight oob works
            Application.Current.Host.Content.IsFullScreen = True
        End If
    End Sub

    Friend Sub App_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException
        e.Handled = True 'Wichtig, sonst WSOF (WhiteScreenOfDeath) bei unhandledExceptions
        Try
            Dim Params(1) As Object
            Params(0) = e.ExceptionObject.ToString()
            Params(1) = "UnhandledException"
            _playerPage.Dispatcher.BeginInvoke(New ManualLicenseAcquirer.Message2GuiDelegate(AddressOf _playerPage.Message2Gui), Params)
        Catch ex As Exception

        End Try
    End Sub

End Class
