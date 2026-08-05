Imports System.IO
Imports DazPackager.Models
Imports DazPackager.Services
Imports DazPackager.Strategies
Imports DazPackager.Utils

Module Program

    Private Enum ProcessResult
        Success
        Skipped
        Failed
    End Enum

    Sub Main(args As String())
        If args.Length < 1 Then
            PrintUsage()
            Return
        End If

        Dim autoYes = args.Any(Function(a) a.Equals("--yes", StringComparison.OrdinalIgnoreCase) OrElse
                                            a.Equals("-y", StringComparison.OrdinalIgnoreCase))

        Dim batchIndex = Array.FindIndex(args, Function(a) a.Equals("--batch", StringComparison.OrdinalIgnoreCase) OrElse
                                                            a.Equals("-b", StringComparison.OrdinalIgnoreCase))

        If batchIndex >= 0 Then
            If batchIndex + 1 >= args.Length Then
                Console.WriteLine("Error: --batch requires a folder path argument.")
                Environment.Exit(1)
                Return
            End If
            RunBatch(args(batchIndex + 1), autoYes)
            Return
        End If

        ' Single-item mode: separate the --yes flag from positional arguments
        ' so it doesn't interfere with reading <path> / [product_name].
        Dim positionalArgs = args.Where(Function(a) Not a.Equals("--yes", StringComparison.OrdinalIgnoreCase) AndAlso
                                                     Not a.Equals("-y", StringComparison.OrdinalIgnoreCase)).ToArray()

        If positionalArgs.Length < 1 Then
            PrintUsage()
            Return
        End If

        ' Dragging several files/folders onto the .exe at once (Windows) passes
        ' every dropped path as its own argument. If the second positional
        ' argument is itself an existing file or folder, this is clearly a
        ' multi-drop rather than "<path> <product_name>" — process every
        ' dropped item individually instead of misreading arg 2 as a name.
        If positionalArgs.Length > 1 AndAlso
           (File.Exists(positionalArgs(1)) OrElse Directory.Exists(positionalArgs(1))) Then
            RunAdHocMultiDrop(positionalArgs, autoYes)
            Return
        End If

        Dim sourcePath = positionalArgs(0)
        Dim userProductName = If(positionalArgs.Length > 1, positionalArgs(1), Nothing)

        If Not File.Exists(sourcePath) AndAlso Not Directory.Exists(sourcePath) Then
            Console.WriteLine($"Error: file or folder not found: {sourcePath}")
            Environment.Exit(1)
            Return
        End If

        Dim result = ProcessSingleItem(sourcePath, userProductName, autoYes)
        If result = ProcessResult.Failed Then Environment.Exit(1)
    End Sub

    Private Sub PrintUsage()
        Console.WriteLine("DazPackager - generates a Manifest.dsx and Supplement.dsx to make a zip installable via Daz Install Manager")
        Console.WriteLine()
        Console.WriteLine("Single item:")
        Console.WriteLine("  DazPackager.exe <path_to_zip_or_folder> [product_name] [--yes]")
        Console.WriteLine("    <path_to_zip_or_folder>  Path to a source .zip file OR an already-extracted product folder")
        Console.WriteLine("    [product_name]           Optional. If omitted, a name is suggested from the source name.")
        Console.WriteLine("    --yes / -y               Optional. Automatically overwrites existing .dsx files without asking.")
        Console.WriteLine()
        Console.WriteLine("Batch mode (process every .zip and subfolder found directly inside a folder):")
        Console.WriteLine("  DazPackager.exe --batch <folder> [--yes]")
        Console.WriteLine("    Product names are always auto-suggested in batch mode (no manual naming per item).")
        Console.WriteLine()
        Console.WriteLine("Tip (Windows): dragging one or more .zip files / folders directly onto DazPackager.exe works too.")
        Console.WriteLine()
        Console.WriteLine("(Running from source instead of the published .exe? Use: dotnet run --project src -- <arguments>)")
    End Sub

    ''' <summary>
    ''' Processes several explicit paths at once — typically several files
    ''' and/or folders dropped together onto the .exe. Unlike --batch, this
    ''' doesn't scan a container folder: the items themselves were already
    ''' picked by the person (via drag &amp; drop). No manual product name is
    ''' used here either, for the same reason as --batch: one name can't
    ''' apply to several different products at once.
    ''' </summary>
    Private Sub RunAdHocMultiDrop(items As String(), autoYes As Boolean)
        Console.WriteLine($"{items.Length} item(s) dropped, processing each one...")
        Console.WriteLine()

        Dim succeeded As New List(Of String)
        Dim skipped As New List(Of String)
        Dim failed As New List(Of String)

        For Each item In items
            If Not File.Exists(item) AndAlso Not Directory.Exists(item) Then
                Console.WriteLine($"--- {item} ---")
                Console.WriteLine("Error: not a valid .zip file or folder, skipping.")
                failed.Add(item)
                Console.WriteLine()
                Continue For
            End If

            Console.WriteLine($"--- {Path.GetFileName(item.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))} ---")

            Dim result = ProcessSingleItem(item, Nothing, autoYes)
            Select Case result
                Case ProcessResult.Success
                    succeeded.Add(item)
                Case ProcessResult.Skipped
                    skipped.Add(item)
                Case ProcessResult.Failed
                    failed.Add(item)
            End Select

            Console.WriteLine()
        Next

        Console.WriteLine("=== Summary ===")
        Console.WriteLine($"Succeeded: {succeeded.Count}")
        Console.WriteLine($"Skipped: {skipped.Count}")
        Console.WriteLine($"Failed: {failed.Count}")
    End Sub

    ''' <summary>
    ''' Processes every .zip file and every subfolder found directly inside
    ''' batchFolder as an independent item, continuing even if one item
    ''' fails, and prints a summary at the end. No manual product name is
    ''' ever used in this mode — it's always auto-suggested per item.
    ''' </summary>
    Private Sub RunBatch(batchFolder As String, autoYes As Boolean)
        If Not Directory.Exists(batchFolder) Then
            Console.WriteLine($"Error: folder not found: {batchFolder}")
            Environment.Exit(1)
            Return
        End If

        Dim items As New List(Of String)
        items.AddRange(Directory.GetFiles(batchFolder, "*.zip", SearchOption.TopDirectoryOnly))
        items.AddRange(Directory.GetDirectories(batchFolder, "*", SearchOption.TopDirectoryOnly))

        If items.Count = 0 Then
            Console.WriteLine("No .zip file or subfolder found to process in this folder.")
            Return
        End If

        Console.WriteLine($"Batch mode: {items.Count} item(s) found in {batchFolder}")
        Console.WriteLine()

        Dim succeeded As New List(Of String)
        Dim skipped As New List(Of String)
        Dim failed As New List(Of String)

        For Each item In items
            Console.WriteLine($"--- {Path.GetFileName(item.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))} ---")

            Dim result = ProcessSingleItem(item, Nothing, autoYes)
            Select Case result
                Case ProcessResult.Success
                    succeeded.Add(item)
                Case ProcessResult.Skipped
                    skipped.Add(item)
                Case ProcessResult.Failed
                    failed.Add(item)
            End Select

            Console.WriteLine()
        Next

        Console.WriteLine("=== Batch summary ===")
        Console.WriteLine($"Succeeded: {succeeded.Count}")
        Console.WriteLine($"Skipped: {skipped.Count}")
        Console.WriteLine($"Failed: {failed.Count}")

        If failed.Count > 0 Then
            Console.WriteLine("Failed items:")
            For Each f In failed
                Console.WriteLine($"  - {Path.GetFileName(f.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}")
            Next
        End If
    End Sub

    ''' <summary>
    ''' Runs the full packaging pipeline for a single source (a .zip file
    ''' or an extracted folder). Shared by single-item mode and batch mode.
    ''' </summary>
    Private Function ProcessSingleItem(sourcePath As String, userProductName As String, autoYes As Boolean) As ProcessResult
        Dim trimmedPath = sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim isFolder = Directory.Exists(trimmedPath)
        Dim isZip = Not isFolder AndAlso File.Exists(trimmedPath) AndAlso
                    Path.GetExtension(trimmedPath).Equals(".zip", StringComparison.OrdinalIgnoreCase)

        If Not isFolder AndAlso Not isZip Then
            Console.WriteLine($"Error: not a valid .zip file or folder: {sourcePath}")
            Return ProcessResult.Failed
        End If

        Try
            Console.WriteLine($"Scanning {Path.GetFileName(trimmedPath)}...")

            Dim scanner As ISourceScanner = If(isFolder, CType(New FolderScanner(), ISourceScanner), New ZipScanner())
            Dim scanResult = scanner.Scan(trimmedPath)

            If scanResult.HasExistingDsxFiles Then
                Dim found As New List(Of String)
                If scanResult.HasExistingManifest Then found.Add("Manifest.dsx")
                If scanResult.HasExistingSupplement Then found.Add("Supplement.dsx")

                Console.WriteLine($"The file(s) {String.Join(" and ", found)} already seem to be present in this source.")

                If Not autoYes Then
                    If Not PromptOverwriteChoice() Then
                        Console.WriteLine("Operation skipped. No file was modified.")
                        Return ProcessResult.Skipped
                    End If
                Else
                    Console.WriteLine("(--yes provided: overwriting automatically.)")
                End If
            End If

            Dim strategy As IFolderMappingStrategy = New AutoScanStrategy()
            Dim resolvedFiles = strategy.ResolveTargets(scanResult)

            Console.WriteLine($"{resolvedFiles.Count} file(s) detected.")

            Select Case scanResult.Status
                Case ScanStatus.MissingContentPrefix
                    Console.WriteLine("Note: 'Content/' folder missing at the root, prefix added automatically.")
            End Select

            Dim nameForParsing = Path.GetFileName(trimmedPath)
            Dim parsed = ZipNameParser.Parse(nameForParsing)

            Dim productName As String
            If Not String.IsNullOrWhiteSpace(userProductName) Then
                productName = userProductName
            Else
                productName = parsed.SuggestedProductName
            End If

            If parsed.IsRecognized Then
                Console.WriteLine($"Suggested product name: '{parsed.SuggestedProductName}' (detected product ID: {parsed.ProductId})")
            Else
                Console.WriteLine("Naming convention not recognized in the source name.")
            End If

            Dim globalId = GlobalIdGenerator.Generate()
            Console.WriteLine($"Generated GlobalID: {globalId}")

            Dim manifestDoc = New ManifestBuilder().Build(globalId, resolvedFiles)

            Dim product As New ProductInfo With {.Name = productName, .Tags = ""}
            Dim supplementDoc = New SupplementBuilder().Build(product)

            Dim outputFileName = OutputFileNamer.BuildOutputFileName(
                nameForParsing, parsed, userProvidedProductName:=userProductName)

            If Not parsed.IsRecognized Then
                Console.WriteLine($"No 'IM{{id}}-{{variant}}_' prefix found in the source name; DIM requires one to accept the package, so a synthetic one was generated: {outputFileName}")
            End If

            Dim parentDir = Path.GetDirectoryName(Path.GetFullPath(trimmedPath))
            Dim outputPath = Path.Combine(parentDir, outputFileName)

            Dim writer As New PackageWriter()

            If isFolder Then
                Dim reader As New FolderSourceReader(trimmedPath)
                writer.WritePackage(reader, resolvedFiles, outputPath, manifestDoc, supplementDoc)
            Else
                Using reader As New ZipSourceReader(trimmedPath)
                    writer.WritePackage(reader, resolvedFiles, outputPath, manifestDoc, supplementDoc)
                End Using
            End If

            Console.WriteLine()
            Console.WriteLine($"Package generated: {outputPath}")
            Console.WriteLine("Drop it into the folder watched by Daz Install Manager so it shows up ready to install in your content library.")

            Return ProcessResult.Success

        Catch ex As InvalidOperationException
            Console.WriteLine($"Error: {ex.Message}")
            Return ProcessResult.Failed
        Catch ex As Exception
            Console.WriteLine($"Unexpected error: {ex.Message}")
            Return ProcessResult.Failed
        End Try
    End Function

    ''' <summary>
    ''' Asks the user whether to abort (1) or continue and overwrite the
    ''' existing .dsx files (2). Keeps asking until a valid answer is given.
    ''' Returns True for "continue / overwrite".
    ''' </summary>
    Private Function PromptOverwriteChoice() As Boolean
        Do
            Console.WriteLine("Skip the operation (1) or continue and overwrite these files (2)?")
            Console.Write("Your choice [1/2]: ")
            Dim input = Console.ReadLine()

            Select Case input?.Trim()
                Case "1"
                    Return False
                Case "2"
                    Return True
                Case Else
                    Console.WriteLine("Unrecognized answer, please enter 1 or 2.")
            End Select
        Loop
    End Function

End Module
