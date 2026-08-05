Imports DazPackager.Models

Namespace Strategies

    ''' <summary>
    ''' Determines the final target path (TARGET) of each scanned file.
    ''' V1: fully automatic resolution (AutoScanStrategy).
    ''' Designed for the future: a manual per-folder mapping strategy could
    ''' implement this same interface without changing anything else in
    ''' the program.
    ''' </summary>
    Public Interface IFolderMappingStrategy
        Function ResolveTargets(scan As ScanResult) As List(Of ScannedFile)
    End Interface

End Namespace
