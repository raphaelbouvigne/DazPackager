Imports System.IO
Imports System.Text.RegularExpressions
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Scans an already-extracted folder (as opposed to a zip file) and
    ''' detects its structure, using the same rules as ZipScanner.
    ''' </summary>
    Public Class FolderScanner
        Implements ISourceScanner

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
            Dim folderChoices As String = KnownDazFolders.Pattern

            If hasContentRoot Then
                status = ScanStatus.OK
            ElseIf files.Any(Function(f) KnownDazFolders.Folders.Any(Function(k) f.RelativePath.StartsWith(k & "/", StringComparison.OrdinalIgnoreCase))) Then
                status = ScanStatus.MissingContentPrefix
            ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "^[^/]+/Content/(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) Then
                Dim nestedFile = files.First(Function(f) Regex.IsMatch(f.RelativePath, "^[^/]+/Content/(?:" & folderChoices & ")/", RegexOptions.IgnoreCase))
                Dim nestedRoot As String = nestedFile.RelativePath.Split("/"c)(0) & "/"

                If files.All(Function(f) f.RelativePath.StartsWith(nestedRoot, StringComparison.OrdinalIgnoreCase) OrElse
                                          Not f.RelativePath.Contains("/"c)) Then
                    status = ScanStatus.NestedContentPrefix
                Else
                    status = ScanStatus.UnrecognizedStructure
                End If
            ElseIf files.Any(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase)) Then
                Dim sampleFile = files.First(Function(f) Regex.IsMatch(f.RelativePath, "(?:^|/)(?:" & folderChoices & ")/", RegexOptions.IgnoreCase))
                Dim deepRoot As String = sampleFile.RelativePath.Split("/"c)(0) & "/"

                If files.All(Function(f) f.RelativePath.StartsWith(deepRoot, StringComparison.OrdinalIgnoreCase) OrElse
                                          Not f.RelativePath.Contains("/"c)) Then
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