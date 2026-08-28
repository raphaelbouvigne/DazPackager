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
			Dim folderChoices As String = KnownDazFolders.Pattern

			If hasContentRoot Then
				status = ScanStatus.OK
			ElseIf files.Any(Function(f) KnownDazFolders.Folders.Any(Function(k) f.RelativePath.StartsWith(k & "/", StringComparison.OrdinalIgnoreCase))) Then
				status = ScanStatus.MissingContentPrefix
			ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "^[^/]+/Content/(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) Then
				' NestedContentPrefix: a "Content/" folder exists one level deep.
				' All files must share the same top-level folder, but siblings of
				' Content/ (Documentation/, License/, etc.) are tolerated.
				Dim nestedFile = files.First(Function(f) Regex.IsMatch(f.RelativePath, "^[^/]+/Content/(?:" & folderChoices & ")/", RegexOptions.IgnoreCase))
				Dim nestedRoot As String = nestedFile.RelativePath.Split("/"c)(0) & "/"

				If files.All(Function(f) f.RelativePath.StartsWith(nestedRoot, StringComparison.OrdinalIgnoreCase)) Then
					status = ScanStatus.NestedContentPrefix
				Else
					status = ScanStatus.UnrecognizedStructure
				End If
			ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) Then
				' Deep WrongContentPrefix: known Daz folders exist, but nested inside
				' a custom-named folder with no "Content/" folder present.
				Dim sampleFile = files.First(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase))
				Dim deepRoot As String = sampleFile.RelativePath.Split("/"c)(0) & "/"

				If files.All(Function(f) f.RelativePath.StartsWith(deepRoot, StringComparison.OrdinalIgnoreCase)) Then
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
