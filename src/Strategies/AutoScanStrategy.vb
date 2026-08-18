Imports System.Text.RegularExpressions
Imports DazPackager.Models

Namespace Strategies

    ''' <summary>
    ''' Default strategy: the target path is deduced automatically from the
    ''' zip structure, with no manual input required.
    ''' </summary>
    Public Class AutoScanStrategy
        Implements IFolderMappingStrategy

        Public Function ResolveTargets(scan As ScanResult) As List(Of ScannedFile) _
            Implements IFolderMappingStrategy.ResolveTargets

            Select Case scan.Status
                Case ScanStatus.OK
                    For Each f In scan.Files
                        f.Target = f.RelativePath
                    Next

                Case ScanStatus.MissingContentPrefix
                    For Each f In scan.Files
                        f.Target = "Content/" & f.RelativePath
                    Next
					
				Case ScanStatus.WrongContentPrefix
					For Each f In scan.Files
						' Replaces any folder name like "My library/" or "Content creator/" to "Content/"
						f.Target = Regex.Replace(f.RelativePath, "^[^/]+/", "Content/")
					Next

                Case ScanStatus.UnrecognizedStructure
                    Throw New InvalidOperationException(
                        "Unrecognized zip structure: no 'Content/' folder or standard Daz subfolder (data/, People/, Runtime/...) found at the root.")
            End Select

            Return scan.Files
        End Function

    End Class

End Namespace
