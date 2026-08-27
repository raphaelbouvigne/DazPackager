Imports System.Text.RegularExpressions
Imports System.IO.Compression
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Scans a source zip file and detects its structure (presence or
    ''' absence of a root "Content/" folder, recognized Daz folders, etc.).
    ''' </summary>
    Public Class ZipScanner
        Implements ISourceScanner

        Private ReadOnly KnownDazFolders As String() = {
            "data", "People", "Runtime", "Environments", "Props",
            "Scenes", "Scripts", "Camera Presets", "Light Presets",
            "Render Presets", "Shader Presets", "Materials", "Lights",
            "Templates", "Textures"
        }

        Public Function Scan(sourcePath As String) As ScanResult Implements ISourceScanner.Scan
            Dim files As New List(Of ScannedFile)
            Dim hasContentRoot As Boolean
            Dim hasExistingManifest As Boolean
            Dim hasExistingSupplement As Boolean

            Using archive = ZipFile.OpenRead(sourcePath)
                hasContentRoot = archive.Entries.Any(
                    Function(e) e.FullName.Replace("\"c, "/"c).StartsWith("Content/", StringComparison.OrdinalIgnoreCase))

                For Each entry In archive.Entries
                    ' A "folder" entry has an empty Name (it ends with "/")
                    If String.IsNullOrEmpty(entry.Name) Then Continue For

                    Dim relPath = entry.FullName.Replace("\"c, "/"c)

                    ' We flag existing root .dsx files without adding them to the
                    ' list of files to install: it's up to the caller to decide
                    ' what to do (abort or overwrite).
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
            End Using

            Dim status As ScanStatus
			' Join them for the regex patterns
			Dim folderChoices As String = String.Join("|", KnownDazFolders)

            If hasContentRoot Then
                status = ScanStatus.OK
            ElseIf files.Any(Function(f) KnownDazFolders.Any(Function(k) f.RelativePath.StartsWith(k & "/", StringComparison.OrdinalIgnoreCase))) Then
                status = ScanStatus.MissingContentPrefix
            ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "^[^/]+/Content/(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) AndAlso files.All(Function(f) f.RelativePath.StartsWith(f.RelativePath.Split("/"c)(0) & "/Content/", StringComparison.OrdinalIgnoreCase)) Then
                status = ScanStatus.NestedContentPrefix
            ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) Then
                ' Deep WrongContentPrefix: find the first occurrence of a DAZ folder in any file to determine the root prefix
                Dim sampleFile = files.FirstOrDefault(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase))
                Dim match = Regex.Match(sampleFile.RelativePath, "^.*?(?=(?:" & folderChoices & ")/)", RegexOptions.IgnoreCase)
                Dim detectedPrefix As String = match.Value ' e.g., "G9-V9 Poses/My DAZ 3D Library/"
                
                ' Strict check: all files must reside under this exact deep prefix to avoid junk
                If files.All(Function(f) f.RelativePath.StartsWith(detectedPrefix, StringComparison.OrdinalIgnoreCase)) Then
                    status = ScanStatus.WrongContentPrefix
                Else
                    status = ScanStatus.UnrecognizedStructure
                End If
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
