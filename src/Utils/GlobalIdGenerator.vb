Namespace Utils

    Public Module GlobalIdGenerator

        ''' <summary>Generates a unique identifier in UUID format, as observed in real DIM Manifest.dsx files.</summary>
        Public Function Generate() As String
            Return Guid.NewGuid().ToString()
        End Function

    End Module

End Namespace
