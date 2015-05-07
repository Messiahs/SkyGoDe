Public Class SerieInfo

    Public Class episode
        Public ID As String
        Public Title As String
        Public [Alias] As String
        Public coverSmall_localPath As String
        Public coverSmall_url As String
        Public coverBig_localPath As String
        Public coverBig_url As String
        Public backgroundImage_localPath As String
        Public backgroundImage_url As String
        Public Description As String
        Public VideoUrl As String
    End Class

    Public Class season
        Public ID As String
        Public Title As String
        Public [Alias] As String
        Public coverSmall_localPath As String
        Public coverSmall_url As String
        Public coverBig_localPath As String
        Public coverBig_url As String
        Public backgroundImage_localPath As String
        Public backgroundImage_url As String
        Public Description As String
        Public VideoUrl As String
        Public Episodes As New List(Of episode)
    End Class

    Public ID As String
    Public Title As String
    Public [Alias] As String
    Public Apix_ID As String
    Public Title2 As String
    Public coverSmall_localPath As String
    Public coverSmall_url As String
    Public coverBig_localPath As String
    Public coverBig_url As String
    Public backgroundImage_localPath As String
    Public backgroundImage_url As String
    Public Description As String
    Public VideoUrl As String
    Public Seasons As New List(Of season)

    Public Sub New()
        ID = "-1"
        Title = String.Empty
        Description = String.Empty
        VideoUrl = String.Empty
    End Sub

End Class
