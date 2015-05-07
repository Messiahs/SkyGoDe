Imports System.IO
Imports System.Threading
Imports SkyGoDe.PluginConfiguration
Imports System.Net
Imports System.Drawing
Imports System.ComponentModel
Imports MediaPortal.GUI.Library

Namespace SkyGoDe
    Public Class ImageDownloader

#Region "sub's called by backgroundworker"

        Friend Shared Sub readOrDeleteImages()
            Dim files As FileInfo()
            Dim keepdate As DateTime = DateTime.Now.AddDays(-CDbl(userSetting.PlugIn_SkyGoDe_ImageCacheDays))

            'erase Image store
            ListOfExistingCovers.Clear()
            ListOfExistingBackdrops.Clear()

            Try
                'getorDelete Covers
                If IO.Directory.Exists(folder_Thumbs_Plugin_Cache_SkyGo_Covers) Then
                    files = New DirectoryInfo(folder_Thumbs_Plugin_Cache_SkyGo_Covers).GetFiles("*.*")
                    For Each file As FileInfo In files
                        If file.LastWriteTime <= keepdate Then
                            file.Delete()
                        Else
                            ListOfExistingCovers.Add(file.FullName, "")
                        End If
                    Next
                End If

                'getorDelete Background
                If IO.Directory.Exists(folder_Thumbs_Plugin_Cache_SkyGo_Backdrops) Then
                    files = New DirectoryInfo(folder_Thumbs_Plugin_Cache_SkyGo_Backdrops).GetFiles("*.*")

                    For Each file As FileInfo In files
                        If file.LastWriteTime <= keepdate Then
                            file.Delete()
                        Else
                            ListOfExistingBackdrops.Add(file.FullName, "")
                        End If
                    Next
                End If

            Catch ex As Exception
                Log.Error("SkyGoDe: error readOrDeleteImages: " & ex.Message)
            End Try
        End Sub

        Public Shared Sub deletefiles(ByVal files As FileInfo())
            Dim keepdate As DateTime = DateTime.Now.AddDays(-CDbl(userSetting.PlugIn_SkyGoDe_ImageCacheDays))

            For Each file As FileInfo In files
                If file.LastWriteTime <= keepdate Then
                    file.Delete()
                End If
            Next
        End Sub

        Public Shared Sub getRandomBgImages()
            'read all files form Skygode/Background-Path and store them for later use
            Try
                Dim dir As DirectoryInfo = New IO.DirectoryInfo(folder_Thumbs_Plugin_BackgroundImage)
                Dim fileInfos As IO.FileInfo() = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                Dim fileInfo As IO.FileInfo

                For Each fileInfo In fileInfos.OrderBy(Function(i) i.Name)
                    If fileInfo.Extension = ".jpg" Or fileInfo.Extension = ".png" Then
                        backgroundImagePathStore.Add(fileInfo.FullName)
                    End If
                Next
            Catch ex As Exception
                Log.Error("SkyGoDe: error getRandomBgImages: " & ex.Message)
            End Try
        End Sub
#End Region

    End Class
End Namespace

