Imports System.IO
Imports System.Text

Namespace Utils

    ''' <summary>
    ''' A TextWriter that duplicates everything written to it across two
    ''' underlying writers — used to mirror console output into a log file
    ''' without changing any existing Console.WriteLine call site.
    ''' </summary>
    Public Class MultiTextWriter
        Inherits TextWriter

        Private ReadOnly _first As TextWriter
        Private ReadOnly _second As TextWriter

        Public Sub New(first As TextWriter, second As TextWriter)
            _first = first
            _second = second
        End Sub

        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return _first.Encoding
            End Get
        End Property

        Public Overrides Sub Write(value As Char)
            _first.Write(value)
            _second.Write(value)
        End Sub

        Public Overrides Sub Write(value As String)
            _first.Write(value)
            _second.Write(value)
        End Sub

        Public Overrides Sub WriteLine(value As String)
            _first.WriteLine(value)
            _second.WriteLine(value)
        End Sub

        Public Overrides Sub WriteLine()
            _first.WriteLine()
            _second.WriteLine()
        End Sub

        Public Overrides Sub Flush()
            _first.Flush()
            _second.Flush()
        End Sub

    End Class

End Namespace