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
            ElseIf files.Any(Function(f) KnownDazFolders.Any(
                Function(k) f.RelativePath.StartsWith(k & "/", StringComparison.OrdinalIgnoreCase))) Then
                status = ScanStatus.MissingContentPrefix
            ElseIf files.Any(Function(f) 
					' The folder at root is not Content, but it has known Daz folders inside
					Dim pattern As String = "^[^/]+/(?:" & folderChoices & ")/"
					Return Regex.IsMatch(f.RelativePath, pattern, RegexOptions.IgnoreCase)
				End Function) Then
                status = ScanStatus.WrongContentPrefix
            ElseIf files.Any(Function(f)
                    ' If the Content folder is deeper, like "Whatever/Content/" 
                    Dim patternNested As String = "^[^/]+/Content/(?:" & folderChoices & ")/"
                    Return Regex.IsMatch(f.RelativePath, patternNested, RegexOptions.IgnoreCase)
                End Function) AndAlso files.All(Function(f)
                    ' But fails if there are any files outside the Content folder, like preview.txt. I know, there are HasExistingManifest and HasExistingSupplement cases in nested cases, but I don't want to handle it currently
                    Return f.RelativePath.StartsWith(f.RelativePath.Split("/"c)(0) & "/Content/", StringComparison.OrdinalIgnoreCase)
                End Function) Then
                status = ScanStatus.NestedContentPrefix
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
