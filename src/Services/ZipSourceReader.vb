Imports System.IO
Imports System.IO.Compression

Namespace Services

    ''' <summary>
    ''' Reads files directly out of a source zip archive, keeping it open
    ''' for the duration of the packaging operation. Dispose to release it.
    ''' </summary>
    Public Class ZipSourceReader
        Implements ISourceReader, IDisposable

        Private ReadOnly archive As ZipArchive

        Public Sub New(zipPath As String)
            archive = ZipFile.OpenRead(zipPath)
        End Sub

        Public Function OpenRead(relativePath As String) As Stream Implements ISourceReader.OpenRead
            Dim entry = archive.GetEntry(relativePath)
            If entry Is Nothing Then
                Throw New FileNotFoundException($"Entry not found in source zip: {relativePath}")
            End If
            Return entry.Open()
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            archive.Dispose()
        End Sub

    End Class

End Namespace
