Imports System.IO

Namespace Services

    ''' <summary>
    ''' Opens a read stream for a given file, identified by its path
    ''' relative to the source root, regardless of whether that source is
    ''' a zip archive or an extracted folder.
    ''' </summary>
    Public Interface ISourceReader
        Function OpenRead(relativePath As String) As Stream
    End Interface

End Namespace
