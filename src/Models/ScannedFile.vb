Namespace Models

    ''' <summary>
    ''' Represents a file found in the source zip, with its original path
    ''' and its final target path (resolved by an IFolderMappingStrategy).
    ''' </summary>
    Public Class ScannedFile
        ''' <summary>Relative path of the file as it appears in the source zip.</summary>
        Public Property RelativePath As String

        ''' <summary>Final path that will be written to the VALUE attribute in Manifest.dsx.</summary>
        Public Property Target As String
    End Class

End Namespace
