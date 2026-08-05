Imports System.IO
Imports System.IO.Compression
Imports System.Xml.Linq
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Builds the final output zip from scratch: every scanned file is
    ''' copied from the source (zip or folder, via ISourceReader) into the
    ''' output archive at its resolved Target path, and Manifest.dsx /
    ''' Supplement.dsx are added at the root.
    '''
    ''' Rebuilding from scratch (rather than copying an existing zip and
    ''' patching it) works identically for zip and folder sources, and
    ''' correctly relocates files when a "Content/" prefix had to be added
    ''' (the MissingContentPrefix case) — copying the original zip verbatim
    ''' would have left the actual entries out of sync with what the
    ''' Manifest declares.
    ''' </summary>
    Public Class PackageWriter

        Public Sub WritePackage(reader As ISourceReader, files As List(Of ScannedFile),
                                 outputZipPath As String, manifest As XDocument, supplement As XDocument)

            If File.Exists(outputZipPath) Then File.Delete(outputZipPath)

            Using outputArchive = ZipFile.Open(outputZipPath, ZipArchiveMode.Create)
                For Each f In files
                    Dim entry = outputArchive.CreateEntry(f.Target)
                    Using sourceStream = reader.OpenRead(f.RelativePath)
                        Using entryStream = entry.Open()
                            sourceStream.CopyTo(entryStream)
                        End Using
                    End Using
                Next

                AddXmlEntry(outputArchive, "Manifest.dsx", manifest)
                AddXmlEntry(outputArchive, "Supplement.dsx", supplement)
            End Using
        End Sub

        Private Sub AddXmlEntry(archive As ZipArchive, entryName As String, doc As XDocument)
            Dim entry = archive.CreateEntry(entryName)
            Using stream = entry.Open()
                doc.Save(stream)
            End Using
        End Sub

    End Class

End Namespace
