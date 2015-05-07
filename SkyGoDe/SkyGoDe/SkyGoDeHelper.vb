Imports Microsoft.Win32
Namespace SkyGoDe
    Module SkyGoDeHelper
        Private regPath As String = Registry.CurrentUser.ToString() + "\SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\"
        Private Const regKeyNameEmulation As String = "FEATURE_BROWSER_EMULATION"
        Private Const regValueIeVersion As String = "10000"

        Friend Sub SetIEVersion()
            ' Set a flag in the registry to make this browser run as if it was IE 10
            ' Value reference: http://msdn.microsoft.com/en-us/library/ee330730%28v=VS.85%29.aspx
            ' IDOC Reference:  http://msdn.microsoft.com/en-us/library/ms535242%28v=vs.85%29.aspx
            Registry.SetValue(regPath + regKeyNameEmulation, Process.GetCurrentProcess().ProcessName + ".exe", regValueIeVersion, RegistryValueKind.DWord)
        End Sub

        Friend Sub RemoveIEVersion()
            Dim key = Registry.CurrentUser.OpenSubKey(regPath.Substring(1), True)
            key.DeleteValue(Process.GetCurrentProcess().ProcessName + ".exe", False)
        End Sub

        Friend Function UriCheck(ByVal UriString As String) As Boolean
            Try
                Dim Uri As Uri = New Uri(UriString)
                UriCheck = True
            Catch ex As Exception
                UriCheck = False
            End Try

            Return UriCheck
        End Function

    End Module
End Namespace

