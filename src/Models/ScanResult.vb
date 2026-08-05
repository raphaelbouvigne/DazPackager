Namespace Models

    ''' <summary>
    ''' Status of the structure detected in the zip file, used to determine
    ''' how (or whether) target paths can be resolved.
    ''' </summary>
    Public Enum ScanStatus
        ''' <summary>A "Content/" folder is present at the root of the zip.</summary>
        OK
        ''' <summary>No "Content/" folder, but recognized Daz folders (data/, People/, Runtime/...) at the root.</summary>
        MissingContentPrefix
        ''' <summary>No recognizable Daz structure found.</summary>
        UnrecognizedStructure
    End Enum

    Public Class ScanResult
        Public Property Files As List(Of ScannedFile)
        Public Property Status As ScanStatus

        ''' <summary>A Manifest.dsx and/or Supplement.dsx already exist at the root of the source zip.</summary>
        Public Property HasExistingManifest As Boolean
        Public Property HasExistingSupplement As Boolean

        Public ReadOnly Property HasExistingDsxFiles As Boolean
            Get
                Return HasExistingManifest OrElse HasExistingSupplement
            End Get
        End Property
    End Class

End Namespace
