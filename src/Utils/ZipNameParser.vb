Imports System.Text.RegularExpressions

Namespace Utils

    ''' <summary>
    ''' Result of analyzing a zip file name.
    ''' </summary>
    Public Class ParsedZipName
        Public Property ProductId As String
        ' Note: "Variant" is a reserved keyword in VB.NET (legacy VB6/VBA type), hence "VariantNumber".
        Public Property VariantNumber As String
        Public Property ShortName As String
        Public Property SuggestedProductName As String
        Public Property IsRecognized As Boolean
    End Class

    ''' <summary>
    ''' Attempts to recognize the "IM{id}-{variant}_{name}.zip" naming
    ''' convention used by some Daz3D/DIM exports, and derive a readable
    ''' product name from it. Never blocks: if the name isn't recognized,
    ''' returns a fallback based on the raw file name.
    ''' </summary>
    Public Module ZipNameParser

        Private ReadOnly Pattern As New Regex(
            "^IM(?<id>\d+)-(?<variant>\d+)_(?<name>.+)$",
            RegexOptions.IgnoreCase)

        Public Function Parse(zipFileName As String) As ParsedZipName
            Dim nameWithoutExt = IO.Path.GetFileNameWithoutExtension(zipFileName)
            Dim match = Pattern.Match(nameWithoutExt)

            If Not match.Success Then
                Return New ParsedZipName With {
                    .IsRecognized = False,
                    .SuggestedProductName = nameWithoutExt
                }
            End If

            Dim rawName = match.Groups("name").Value
            Dim humanized = HumanizeProductName(rawName)

            Return New ParsedZipName With {
                .ProductId = match.Groups("id").Value,
                .VariantNumber = match.Groups("variant").Value,
                .ShortName = rawName,
                .SuggestedProductName = humanized,
                .IsRecognized = True
            }
        End Function

        ''' <summary>
        ''' Turns "Death9HD" into "Death 9 HD": inserts a space at
        ''' lowercase/uppercase, letter/digit and digit/uppercase transitions.
        ''' </summary>
        Private Function HumanizeProductName(raw As String) As String
            Dim spaced = Regex.Replace(raw, "(?<=[a-z])(?=[A-Z])", " ")
            spaced = Regex.Replace(spaced, "(?<=[A-Za-z])(?=\d)", " ")
            spaced = Regex.Replace(spaced, "(?<=\d)(?=[A-Z])", " ")
            spaced = spaced.Replace("_", " ").Replace("-", " ")
            Return Regex.Replace(spaced, "\s+", " ").Trim()
        End Function

    End Module

End Namespace
