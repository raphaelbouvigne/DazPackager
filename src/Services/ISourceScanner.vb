Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Scans a content source (a zip file or an already-extracted folder)
    ''' and reports its structure. ZipScanner and FolderScanner both
    ''' implement this so the rest of the pipeline doesn't need to know
    ''' which kind of source it's dealing with.
    ''' </summary>
    Public Interface ISourceScanner
        Function Scan(sourcePath As String) As ScanResult
    End Interface

End Namespace
