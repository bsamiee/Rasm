# [RASM_RHINO_API_RHINOCOMMON_FILE_PLUGINS]

`Rhino.PlugIns` owns the file-type registration that binds a package into the native file-open and file-save dialog dispatch. `FileImportPlugIn` and `FileExportPlugIn` declare their handled extensions through a `FileTypeList`, and Rhino routes a matching open or save to the plug-in's `ReadFile` or `WriteFile` keyed on the registered file-type index. This dispatch path stands apart from the `File3dm` archive reads and the direct engine invocations a package drives itself.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.PlugIns`, `Rhino.FileIO` (`FileType`)
- asset: `RhinoCommon.dll` — the in-process managed host assembly, verified by direct decompile and the McNeel developer catalog
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dispatch bases and registration carriers
- rail: host

| [INDEX] | [SYMBOL]           | [KIND]           | [CAPABILITY]                                                                |
| :-----: | :----------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `FileImportPlugIn` | abstract plug-in | import dispatch base; declares handled extensions and reads a matched file  |
|  [02]   | `FileExportPlugIn` | abstract plug-in | export dispatch base; declares handled extensions and writes a matched file |
|  [03]   | `FileTypeList`     | class            | ordered file-type registration list populated during `AddFileTypes`         |
|  [04]   | `FileType`         | class            | single extension and description pair (`Rhino.FileIO`)                      |
|  [05]   | `FileReadOptions`  | class            | host read-dispatch mode passed into import                                  |
|  [06]   | `FileWriteOptions` | class            | host write-dispatch mode passed into export                                 |
|  [07]   | `WriteFileResult`  | enum             | export outcome reported back to the host dialog                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: extension registration
- rail: host

- `FileTypeList.FileTypeList(string description, string extension)` — construct a registration list seeded with one type
- `FileTypeList.AddFileType(string description, string extension) : int` — register a type, returning the dispatch index
- `FileTypeList.AddFileType(string description, string extension, bool showOptionsButtonInFileDialog) : int` — register with a dialog options button
- `FileTypeList.AddFileType(string description, string extension1, string extension2) : int` — register a type spanning two extensions
- `FileTypeList.AddFileType(string description, IEnumerable<string> extensions, bool showOptionsButtonInFileDialog) : int` — register a type spanning many extensions
- `FileType.FileType(string extension, string description)` — construct one extension/description pair
- `FileType.Description : string` / `FileType.Extension : string` — the registered identity

[ENTRYPOINT_SCOPE]: dispatch-mode carriers
- rail: host

- `FileReadOptions.FileReadOptions()` — public construction for package-driven reads; `Dispose()` releases the native carrier
- `FileReadOptions.ImportMode` / `OpenMode` / `NewMode` / `InsertMode` / `ImportReferenceMode` / `BatchMode : bool` (get/set) — the host intent axes an engine consults for merge-versus-open behavior
- `FileReadOptions.UseScaleGeometry` / `ScaleGeometry : bool` (get/set) — unit-scale participation for the incoming payload
- `FileReadOptions.OptionsDictionary : ArchivableDictionary` — the per-format option payload the dialog lane threads

[ENTRYPOINT_SCOPE]: plug-in dispatch contract
- rail: host

- `FileImportPlugIn.AddFileTypes(FileTypeList list, FileReadOptions options) : void` — populate the list Rhino shows in the open dialog
- `FileImportPlugIn.ReadFile(string filename, int index, RhinoDoc doc, FileReadOptions options) : Result` — read the file the user chose for the registered `index`
- `FileExportPlugIn.AddFileTypes(FileTypeList list, FileWriteOptions options) : void` — populate the list Rhino shows in the save dialog
- `FileExportPlugIn.WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) : WriteFileResult` — write the file for the registered `index`

## [04]-[IMPLEMENTATION_LAW]

[FILE_PLUGINS_TOPOLOGY]:
- registration is declarative: `AddFileTypes` populates the `FileTypeList`, and each `AddFileType` returns the index the later `ReadFile`/`WriteFile` receives to identify which registered type the user selected
- dispatch is host-driven: Rhino matches the dialog extension to a registered `FileType` and invokes the plug-in's keyed `ReadFile`/`WriteFile`; the plug-in reads the selected type from the index, never by re-parsing the path
- `FileReadOptions`/`FileWriteOptions` carry the host dispatch mode — import against open, selected against all — into the transfer, and `WriteFileResult` reports the export outcome back to the dialog

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the `ReadFile` `Result` and `WriteFile` `WriteFileResult` fold to `Fin<A>`, and the registered-index-to-handler binding is a `HashMap` lookup
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the registered file-type roster wraps as a keyed `SmartEnum` indexed by the `AddFileType` return, collapsing the extension, description, and index triple into one owner the dispatch switches on

[LOCAL_ADMISSION]:
- a package binds into the native file dialog through one `FileImportPlugIn`/`FileExportPlugIn` owner declaring its `FileTypeList`
- the registered index is the single dispatch key; `ReadFile`/`WriteFile` discriminate on it, never on a re-parsed extension string
- direct `File3dm` and engine invocation stays the package-driven path, separate from dialog dispatch

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: native file-dialog registration and dispatch for import and export plug-ins
- Accept: extension registration, host-driven keyed read and write
- Reject: standalone archive reads, direct engine conversion, document identity
