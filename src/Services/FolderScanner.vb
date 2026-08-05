Imports System.IO
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Scans an already-extracted folder (as opposed to a zip file) and
    ''' detects its structure, using the same rules as ZipScanner.
    ''' </summary>
    Public Class FolderScanner
        Implements ISourceScanner

        Private ReadOnly KnownDazFolders As String() = {
            "data", "People", "Runtime", "Environments", "Props",
            "Scenes", "Scripts", "Camera Presets", "Light Presets",
            "Render Presets", "Shader Presets", "Materials", "Lights",
            "Templates", "Textures"
        }

        Public Function Scan(sourcePath As String) As ScanResult Implements ISourceScanner.Scan
            Dim files As New List(Of ScannedFile)
            Dim hasExistingManifest As Boolean
            Dim hasExistingSupplement As Boolean

            Dim hasContentRoot = Directory.GetDirectories(sourcePath).Any(
                Function(d) Path.GetFileName(d).Equals("Content", StringComparison.OrdinalIgnoreCase))

            For Each fullPath In Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                Dim relPath = Path.GetRelativePath(sourcePath, fullPath).Replace("\"c, "/"c)

                If relPath.Equals("Manifest.dsx", StringComparison.OrdinalIgnoreCase) Then
                    hasExistingManifest = True
                    Continue For
                End If
                If relPath.Equals("Supplement.dsx", StringComparison.OrdinalIgnoreCase) Then
                    hasExistingSupplement = True
                    Continue For
                End If

                files.Add(New ScannedFile With {.RelativePath = relPath})
            Next

            Dim status As ScanStatus

            If hasContentRoot Then
                status = ScanStatus.OK
            ElseIf files.Any(Function(f) KnownDazFolders.Any(
                Function(k) f.RelativePath.StartsWith(k & "/", StringComparison.OrdinalIgnoreCase))) Then
                status = ScanStatus.MissingContentPrefix
            Else
                status = ScanStatus.UnrecognizedStructure
            End If

            Return New ScanResult With {
                .Files = files,
                .Status = status,
                .HasExistingManifest = hasExistingManifest,
                .HasExistingSupplement = hasExistingSupplement
            }
        End Function

    End Class

End Namespace
