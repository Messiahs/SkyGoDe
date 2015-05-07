Partial Public Class InstallPage
    Inherits UserControl

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs) 'Handles button1.Click
        If Application.Current.InstallState = InstallState.NotInstalled Then
            Dim ret As Boolean = Application.Current.Install()
        End If
    End Sub
End Class