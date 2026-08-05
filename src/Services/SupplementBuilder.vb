Imports System.Xml.Linq
Imports DazPackager.Models

Namespace Services

    ''' <summary>
    ''' Generates the Supplement.dsx XML document. No GlobalID here: the
    ''' association with Manifest.dsx is made by their simple co-presence
    ''' at the root of the zip, not by a shared ID.
    ''' </summary>
    Public Class SupplementBuilder

        Public Function Build(product As ProductInfo) As XDocument
            Dim root As New XElement("ProductSupplement",
                New XAttribute("VERSION", "0.1"),
                New XElement("ProductName", New XAttribute("VALUE", product.Name)),
                New XElement("InstallTypes", New XAttribute("VALUE", "Content"))
            )

            If Not String.IsNullOrWhiteSpace(product.Tags) Then
                root.Add(New XElement("ProductTags", New XAttribute("VALUE", product.Tags)))
            End If

            Return New XDocument(New XDeclaration("1.0", "UTF-8", Nothing), root)
        End Function

    End Class

End Namespace
