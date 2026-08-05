Imports System.Xml.Linq
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Generates the Manifest.dsx XML document, matching the format
    ''' observed in real DIM packages (VALUE / ACTION / TARGET attribute order).
    ''' </summary>
    Public Class ManifestBuilder

        Public Function Build(globalId As String, files As List(Of ScannedFile)) As XDocument
            Dim root As New XElement("DAZInstallManifest",
                New XAttribute("VERSION", "0.1"),
                New XElement("GlobalID", New XAttribute("VALUE", globalId))
            )

            For Each f In files
                root.Add(New XElement("File",
                    New XAttribute("VALUE", f.Target),
                    New XAttribute("ACTION", "Install"),
                    New XAttribute("TARGET", "Content")
                ))
            Next

            Return New XDocument(New XDeclaration("1.0", "UTF-8", Nothing), root)
        End Function

    End Class

End Namespace
