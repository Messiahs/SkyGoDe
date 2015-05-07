Public Class VideoInfo
    Public ID As String
    Public Title As String
    Public [Alias] As String
    Public Title2 As String
    Public Airdate As String
    Public Description As String
    Public VideoUrl As String
    Public coverSmall_localPath As String
    Public coverSmall_url As String
    Public coverBig_localPath As String
    Public coverBig_url As String
    Public backgroundImage_localPath As String
    Public backgroundImage_url As String
    Public HasDetails As Boolean

    Public Sub VideoInfo()
        Title = String.Empty
        [Alias] = String.Empty
        Title2 = String.Empty
        Description = String.Empty
        VideoUrl = String.Empty
        HasDetails = True
        Airdate = String.Empty
    End Sub

    Public Sub New()
        ID = "-1"
        Title = String.Empty
        [Alias] = String.Empty
        Title2 = String.Empty
        Description = String.Empty
        VideoUrl = String.Empty
        HasDetails = True
    End Sub

End Class
