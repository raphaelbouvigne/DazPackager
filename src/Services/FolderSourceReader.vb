Imports System.IO

Namespace Services

    ''' <summary>
    ''' Reads files directly out of an extracted folder on disk.
    ''' </summary>
    Public Class FolderSourceReader
        Implements ISourceReader

        Private ReadOnly rootPath As String

        Public Sub New(rootPath As String)
            Me.rootPath = rootPath
        End Sub

        Public Function OpenRead(relativePath As String) As Stream Implements ISourceReader.OpenRead
            Dim fullPath = Path.Combine(rootPath, relativePath.Replace("/"c, Path.DirectorySeparatorChar))
            Return File.OpenRead(fullPath)
        End Function

    End Class

End Namespace
