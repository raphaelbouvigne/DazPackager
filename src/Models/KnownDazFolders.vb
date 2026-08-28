Namespace Models

    ''' <summary>
    ''' Central list of Daz3D folder names recognized by the scanners
    ''' and mapping strategies (data/, People/, Runtime/, etc.).
    ''' </summary>
    Public Module KnownDazFolders

        Public ReadOnly Folders As String() = {
            "data", "People", "Runtime", "Environments", "Props",
            "Scenes", "Scripts", "Camera Presets", "Light Presets",
            "Render Presets", "Shader Presets", "Materials", "Lights",
            "Templates", "Textures"
        }

        Public ReadOnly Pattern As String = String.Join("|", Folders)

    End Module

End Namespace
