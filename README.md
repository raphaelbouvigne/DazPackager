# DazPackager

Automatically generates the `Manifest.dsx` and `Supplement.dsx` files needed to make a `.zip` installable via **Daz Install Manager (DIM)** — useful for any content purchased outside the Daz3D store (Renderosity, RenderHub, independent creators, etc.).

## Why this project?

Many Daz Studio assets purchased outside Daz3D come as a "raw" `.zip` (just the `Content/`, `data/`, `Runtime/`... folders) without the metadata Install Manager expects. As a result, they can't be dropped into DIM — you have to extract them manually into your Content Library.

DazPackager scans the zip, generates the two XML files DIM expects, and produces a copy of the zip ready to be dropped into the folder Install Manager watches.

## Features

- 🔍 Automatic scan of the source structure (no manual path entry)
- 📁 Works on both `.zip` files **and** already-extracted folders
- 🗂️ Batch mode: process every `.zip` and subfolder inside a given folder in one run
- 🧩 Generates `Manifest.dsx` (file list + unique GlobalID)
- 🏷️ Generates `Supplement.dsx` (product metadata)
- ✨ Automatically suggests a readable product name from the source name
- 🖥️ Cross-platform (Windows, Linux, macOS) via .NET 8
- 📦 Never modifies the original zip or folder — always writes a new output file
- ⚠️ Asks before overwriting `.dsx` files that already exist in the source

## Expected structure

For best results, your source (zip file or folder) should contain a `Content/` folder at its root:

```
MyProduct.zip            (or a MyProduct/ folder, same rule)
└── Content/
    ├── data/
    ├── People/
    └── Runtime/
```

**If `Content/` is missing** but the tool recognizes standard Daz folders (`data/`, `People/`, `Runtime/`...) directly at the root, it automatically adds the required prefix.

**If no recognizable structure is found** (rare case), the tool stops and reports the error instead of generating a potentially broken package.

## Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later

### Build

```bash
git clone https://github.com/<your-account>/DazPackager.git
cd DazPackager
dotnet build
```

### Run

```bash
dotnet run --project src -- "path/to/MyProduct.zip"
```

Or with a manually forced product name:

```bash
dotnet run --project src -- "path/to/MyProduct.zip" "My Awesome Product"
```

### Publishing a standalone .exe

#### Custom icon

Drop an `icon.ico` file into `src/` (next to `DazPackager.vbproj`) before publishing — the project already references it (`ApplicationIcon`) and will pick it up automatically if present. No icon is required for `dotnet build`/`dotnet run` to work; it's only applied when the `.exe` is built.

#### Single-file executable (DazPackager.dll bundled into the .exe — .NET runtime still required on the machine)

`PublishSingleFile` and `SelfContained` are already set in `DazPackager.vbproj`, so you only need to pick a target platform with `-r`:

```bash
dotnet publish src -c Release -r win-x64 -o publish/win-x64
```

This produces one `DazPackager.exe` in `publish/win-x64/` with `DazPackager.dll` (and its small dependency assemblies) bundled into it — no loose `.dll` sitting next to the `.exe`, and you invoke it directly as `DazPackager.exe ...` instead of `dotnet DazPackager.dll ...`. It stays small (a few MB) because the .NET runtime itself is **not** embedded — the target machine needs the [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed (not the full SDK, just the runtime).

Swap `win-x64` for `linux-x64`, `osx-x64`, or `osx-arm64` (adjusting the output folder accordingly) to publish for another platform — cross-compilation works out of the box, you don't need to run the command on that OS.

If you'd rather have a fully standalone `.exe` that needs nothing installed on the target machine at all (at the cost of a much bigger file, since the .NET runtime itself gets embedded too), override the project defaults on the command line: `dotnet publish src -c Release -r win-x64 --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64`.

## Usage

The examples below use the published `DazPackager.exe` (see [Publishing a standalone .exe](#publishing-a-standalone-exe)). Running from source during development works the same way via `dotnet run --project src -- <arguments>`.

### Single item (a .zip file or an extracted folder)

```bash
DazPackager.exe <path_to_zip_or_folder> [product_name] [--yes]
```

| Argument | Required | Description |
|---|---|---|
| `path_to_zip_or_folder` | Yes | Path to a source `.zip` file, or to an already-extracted product folder |
| `product_name` | No | Product name to use in `Supplement.dsx`. If omitted, a name is automatically suggested from the source name. |
| `--yes` / `-y` | No | Automatically overwrites existing `.dsx` files without asking for confirmation (handy for batch processing / scripting). |

Quick example, with a product name that contains spaces (use quotes around both the path and the product name):

```bash
DazPackager.exe "your zip file name.zip" "your product name" -y
```

Works the same way with an extracted folder instead of a zip:

```bash
DazPackager.exe "path/to/MyExtractedProduct" "your product name" -y
```

### Example

```bash
$ DazPackager.exe "rks maxine for genesis 9.zip"

Scanning rks maxine for genesis 9.zip...
124 file(s) detected.
Naming convention not recognized in the source file name.
Generated GlobalID: 8f3a1c2e-4b5d-4e6f-9a1b-2c3d4e5f6a7b
No 'IM{id}-{variant}_' prefix found in the source file name; DIM requires one to accept the package, so a synthetic one was generated: IM91234567-01_RksMaxineForGenesis9DIM.zip

Package generated: IM91234567-01_RksMaxineForGenesis9DIM.zip
Drop it into the folder watched by Daz Install Manager so it shows up ready to install in your content library.
```

Just drop the generated `.zip` file into the download folder watched by Install Manager (`Content Library Download Path`, configurable in DIM's preferences).

### Batch mode

Process every `.zip` file and every subfolder found directly inside a given folder, in one run:

```bash
DazPackager.exe --batch "path/to/folder/full/of/products" --yes
```

- Product names are **always auto-suggested** in batch mode — there's no way to pass a manual `product_name` per item, since one run covers many items.
- If one item fails (unrecognized structure, corrupt zip, etc.), the batch continues with the next item rather than stopping. A summary of successes, skips, and failures is printed at the end.
- `--yes` is strongly recommended in batch mode — without it, the tool will stop and prompt you individually for every item that already contains a `Manifest.dsx`/`Supplement.dsx`.
- Only the direct children of the given folder are processed (not nested subfolders inside them) — each child is expected to be one product, either as a `.zip` or as an already-extracted folder.

### Drag & drop (Windows)

Once published as an `.exe` (see [Publishing a standalone .exe](#publishing-a-standalone-exe)), you can drop files directly onto it instead of using the command line:

- **One file or folder**: drop it onto `DazPackager.exe` — same result as running the tool with that single path.
- **Several files/folders at once**: drop them all together onto `DazPackager.exe` — each one is processed independently (auto-suggested product name, no manual naming, since a single drop can't target one specific item).

This relies on how Windows launches an `.exe` when you drop files on it (each dropped path becomes a separate command-line argument) — no extra setup needed.

### If a Manifest.dsx / Supplement.dsx already exists in the source

If the source (zip or folder) already contains these files (for example, if you rerun the tool on something you already prepared), DazPackager asks what to do:

```
The file(s) Manifest.dsx and Supplement.dsx already seem to be present in this source.
Skip the operation (1) or continue and overwrite these files (2)?
Your choice [1/2]:
```

- **1**: the operation is canceled/skipped, nothing is modified.
- **2**: the existing `.dsx` files are overwritten with a freshly generated version.

Pass `--yes` as an argument to overwrite automatically without a prompt (useful in scripts and required for smooth batch runs).

## How it works

1. **Scan**: reads the source's file tree (zip entries or folder contents), detects its structure (`Content/` present or not, existing `.dsx` files)
2. **GlobalID generation**: a unique identifier (UUID) that identifies the product
3. **Manifest.dsx generation**: lists every file with its resolved target path
4. **Supplement.dsx generation**: product metadata (name, install type)
5. **Assembly**: a new output zip is built from scratch, copying every file to its resolved target path and adding both `.dsx` files at the root — this works identically for zip and folder sources, and correctly relocates files if a `Content/` prefix had to be added

## Known limitations

- The `Manifest.dsx` / `Supplement.dsx` format isn't officially documented by Daz3D — this tool is based on reverse-engineering real packages. Additional fields (figure compatibility, version, dependencies) exist in some official packages and aren't generated here yet.
- Designed for zips that are already properly organized by the seller (complete `Content/` structure). Fine-grained per-folder mapping customization isn't available yet, but the project's architecture is ready for it (see `IFolderMappingStrategy`).

## The "IM{id}-{variant}_" file name prefix

Real-world testing (confirmed on packages from RenderHub) showed that **DIM requires the zip file name itself to start with an `IM{id}-{variant}_` prefix** to recognize it as installable at all — regardless of what a correct `Manifest.dsx`/`Supplement.dsx` declare inside. A zip named `rks zena for genesis 9.zip`, or even a cleaned-up `rkszenaforgenesis9.zip`, was rejected by DIM until renamed with that prefix.

DazPackager handles this automatically:
- If the source zip's file name already follows that convention (e.g. official Daz3D re-exports), the same ID and variant are reused, along with its own short name as-is.
- Otherwise, a synthetic 8-digit ID is generated deterministically from the source file name (same input → same ID every time, so reprocessing a zip produces a stable output name), in a numeric range chosen to avoid colliding with real Daz3D catalog IDs.

The readable part of the name follows these casing rules:
- **Auto-suggested** (no `product_name` argument given): each word is capitalized and spaces are removed — e.g. `rks maxine for genesis 9` → `RksMaxineForGenesis9`.
- **User-provided** (`product_name` argument given): the exact capitalization you typed is kept, only spaces are removed.

Example: `rks maxine for genesis 9.zip` → `IM9xxxxxxx-01_RksMaxineForGenesis9DIM.zip`

This synthetic ID has no relation to Daz3D's actual product catalog — it exists solely to satisfy DIM's local file name check.

> **Note**: manual testing showed DIM also accepts longer IDs (e.g. a 9-digit ID in `IM900090092-01_...`), so the exact digit count doesn't appear to be strictly enforced. The 8-digit format used here is simply what's been validated as reliable.

## Roadmap

- ✅ Zip source support
- ✅ Extracted folder source support
- ✅ Batch mode (process a whole folder of products at once)
- ✅ Drag & drop support (Windows): drop one or more `.zip`/folders directly onto the `.exe`
- ✅ Custom application icon
- ✅ Self-contained single-file publish (no .NET runtime install required)

DazPackager is intentionally kept as a lightweight command-line tool rather than a full GUI application. Daz Studio users are generally comfortable with the command line, and a CLI tool is easy to script, batch, or wrap in your own front-end (a `.bat`/shell script, a scheduled task, a small GUI of your own, etc.) — no GUI framework needed to get value out of it.

## Contributing

Feedback, bug reports, and pull requests are welcome — especially if you have access to other real-world `Manifest.dsx` / `Supplement.dsx` examples that reveal fields or conventions not covered here.

## License

MIT — see [LICENSE](LICENSE). You're free to use, modify, and redistribute this tool, including in commercial projects, as long as the copyright notice is kept.

## Disclaimer

This tool lets you package content you already own so it can be installed via Daz Install Manager. It does not bypass any protection or licensing system — make sure you only use this software with content you have legally acquired.
