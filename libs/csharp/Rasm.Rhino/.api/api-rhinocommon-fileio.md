# [RASM_RHINO_API_RHINOCOMMON_FILEIO]

`File3dm` owns the standalone `.3dm` archive off any live document — filtered and metadata-only reads, write options, byte serialization, validation, audit, and preview. The direct format engines own single-call conversion across the interchange roster, each keyed on a typed options carrier, and `FilePdf` owns vector page authoring with optional-content groups and a static pre-write stamp. `RhinoDoc` adds the document-attached import, export, and save operations that drive the live model through those same engines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.FileIO`, `Rhino.DocObjects` (`EarthAnchorPoint`), `Rhino` (`RhinoDoc` document-attached I/O)
- asset: `RhinoCommon.dll` — the in-process managed host assembly, verified by direct decompile
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the standalone archive
- rail: host

| [INDEX] | [SYMBOL]                         | [KIND]     | [CAPABILITY]                                                             |
| :-----: | :------------------------------- | :--------- | :----------------------------------------------------------------------- |
|  [01]   | `File3dm`                        | class      | document-free `.3dm` archive; filtered, metadata, byte, and table access |
|  [02]   | `File3dmWriteOptions`            | class      | archive write policy; version, user data, render and analysis meshes     |
|  [03]   | `File3dmSettings`                | class      | archive-level document settings                                          |
|  [04]   | `File3dm.TableTypeFilter`        | flags enum | bounds a partial read to the required tables                             |
|  [05]   | `File3dm.ObjectTypeFilter`       | flags enum | bounds a partial read to the required object kinds                       |
|  [06]   | `ManifestTable`                  | table      | archive component manifest                                               |
|  [07]   | `File3dmObjectTable`             | table      | archive object collection                                                |
|  [08]   | `File3dmLayerTable`              | table      | archive layer collection                                                 |
|  [09]   | `File3dmMaterialTable`           | table      | archive material collection                                              |
|  [10]   | `File3dmGroupTable`              | table      | archive group collection                                                 |
|  [11]   | `File3dmInstanceDefinitionTable` | table      | archive block definitions read by the block catalog                      |
|  [12]   | `File3dmViewTable`               | table      | archive model and named views                                            |
|  [13]   | `File3dmEmbeddedFiles`           | table      | files embedded in the archive                                            |
|  [14]   | `File3dmRenderMaterials`         | table      | archive render materials                                                 |
|  [15]   | `File3dmRenderEnvironments`      | table      | archive render environments                                              |
|  [16]   | `File3dmRenderTextures`          | table      | archive render textures                                                  |
|  [17]   | `EarthAnchorPoint`               | class      | earth-to-model georeference read from the header                         |

[PUBLIC_TYPE_SCOPE]: the direct format-engine roster
- rail: host

| [INDEX] | [SYMBOL]               | [DIRECTION]      | [CAPABILITY]                                                |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `FileObj`              | read + write     | Wavefront OBJ mesh interchange                              |
|  [02]   | `File3ds`              | read + write     | 3D Studio mesh interchange                                  |
|  [03]   | `FileAi`               | read + write     | Adobe Illustrator vector interchange                        |
|  [04]   | `FileDwg`              | read + write     | AutoCAD DWG/DXF interchange                                 |
|  [05]   | `FileFbx`              | read + write     | Autodesk FBX interchange                                    |
|  [06]   | `FileGltf`             | write            | glTF asset export                                           |
|  [07]   | `FileIgs`              | write            | IGES CAD export                                             |
|  [08]   | `FileNwd`              | write            | Navisworks export                                           |
|  [09]   | `FilePly`              | read + write     | Stanford PLY point/mesh interchange                         |
|  [10]   | `FileSkp`              | read + write     | SketchUp interchange                                        |
|  [11]   | `FileStl`              | read + write     | STL mesh interchange                                        |
|  [12]   | `FileStp`              | read + write     | STEP CAD interchange                                        |
|  [13]   | `FileUsd`              | write            | Universal Scene Description export                          |
|  [14]   | `FileSvg`              | read             | SVG vector import                                           |
|  [15]   | `FileX_T`              | write            | Parasolid export                                            |
|  [16]   | `File3mf`              | write            | 3MF additive-manufacturing export                           |
|  [17]   | `FileAmf`              | write            | AMF additive-manufacturing export                           |
|  [18]   | `FileCd`               | write            | CD export                                                   |
|  [19]   | `FileDgn`              | read             | MicroStation DGN import                                     |
|  [20]   | `FileDst`              | read             | DST import                                                  |
|  [21]   | `FileEps`              | read             | Encapsulated PostScript vector import                       |
|  [22]   | `FileGHS`              | read             | GHS import                                                  |
|  [23]   | `FileGts`              | write            | GTS surface export                                          |
|  [24]   | `FileLwo`              | read + write     | LightWave object interchange                                |
|  [25]   | `FilePov`              | write            | POV-Ray scene export                                        |
|  [26]   | `FileSat`              | write            | ACIS SAT export                                             |
|  [27]   | `FileSlc`              | write            | SLC slice export                                            |
|  [28]   | `FileSW`               | read             | SolidWorks part/assembly import                             |
|  [29]   | `FileUdo`              | write            | UDO export                                                  |
|  [30]   | `FileVda`              | write            | VDA-FS export                                               |
|  [31]   | `FileVrml`             | write            | VRML/WRL export                                             |
|  [32]   | `FileX3dv`             | write            | X3DV export                                                 |
|  [33]   | `FileRaw`              | read + write     | RAW triangle interchange                                    |
|  [34]   | `FileTxt`              | read + write     | delimited text interchange                                  |
|  [35]   | `FileCsv`              | write            | CSV property/measurement export                             |
|  [36]   | `FileXamlWriteOptions` | write dictionary | XAML export options; `ToDictionary` feeds `RhinoDoc.Export` |
|  [37]   | `WriteFileResult`      | enum             | direct-engine write outcome                                 |

Every `File*ReadOptions`/`File*WriteOptions` carrier exposes `ToDictionary() : ArchivableDictionary`, the payload the document-attached `RhinoDoc.Import`/`Export`/`ExportSelected` lane consumes; each engine above pairs `Read(string, RhinoDoc, File*ReadOptions) : bool` and/or `Write(string, RhinoDoc, File*WriteOptions) : bool` in the `FileObj`/`FilePly` shape (`FileObj`/`FilePly` writes return `WriteFileResult`). Option members driven by exchange policy: `File3dsWriteOptions.SaveViews`/`SaveLights`; `FileAiReadOptions.PreserveModelScale`/`RhinoScale`/`AiScale`/`AiUnits`; `FileAiWriteOptions.PreserveModelScale`/`OrderLayers`/`RhinoScale`/`AIScale`/`AiUnits`; `FileEpsReadOptions.PreserveModelScale`/`RhinoScale`/`EpsScale`/`EpsUnits`; `FileObjWriteOptions(FileWriteOptions)` with `ObjectType`/`ExportObjectNames`/`ExportGroupNameLayerNames`/`ExportMaterialDefinitions`/`UseDisplayColorForMaterial`/`ExportTcs`/`ExportNormals`/`UseRenderMeshes`/`SortObjGroups`/`MergeNestedGroupingNames`; `FilePlyWriteOptions(FileWriteOptions)` with `ExportASCII`/`ExportDoubles`/`ExportNormals`/`ExportColors`/`ExportMaterial`; `FileDwgWriteOptions.FullLayerPath`/`ExportSurfacesAs`/`UseLWPolylines`/`ColorMethod`/`UseColor`; `FileStlWriteOptions.BinaryFile`; `FileFbxWriteOptions.SaveObjectsAs`/`SaveViews`/`SaveLights`/`SaveVertexNormals`; `FileSkpWriteOptions.GroupObjects`; `FileVrmlWriteOptions`/`FileX3dvWriteOptions` `ExportTextureCoordinates`/`ExportVertexNormals`; `FileTxtWriteOptions.SurroundWithDoubleQuotes`; `FileCsvWriteOptions` header/layer/group/object/measurement column flags; `FileGltfWriteOptions` material/layer/normal flags plus the `UseDracoCompression`/`DracoCompressionLevel`/`DracoQuantizationBits*` family; `FileUsdWriteOptions.ForceMeshes`/`IncludeUserStrings`/`BlockHandling` (`USDExportBlockHandling`)/`DefaultLayer`/`ModelName`; `FileXamlWriteOptions.UseExistingRenderMeshes`.

[PUBLIC_TYPE_SCOPE]: PDF page authoring and document-attached I/O
- rail: host

| [INDEX] | [SYMBOL]                | [KIND]     | [CAPABILITY]                                                               |
| :-----: | :---------------------- | :--------- | :------------------------------------------------------------------------- |
|  [01]   | `FilePdf`               | class      | vector PDF page generation, drawing, and optional-content groups           |
|  [02]   | `FilePdfReadOptions`    | class      | PDF import scale, unit, hatch, and text policy                             |
|  [03]   | `FilePdfEventArgs`      | event args | payload for the static `PreWrite` stamp hook                               |
|  [04]   | `PrintedPageDefinition` | class      | custom printed-page definition for `GetCustomPages`/`SetCustomPages`       |
|  [05]   | `FileWriteOptions`      | class      | document write policy for `RhinoDoc.WriteFile`                             |
|  [06]   | `ViewCaptureSettings`   | class      | page capture input to `FilePdf.AddPage`; the display catalog owns the type |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive reads — filtered, metadata, and byte
- rail: host

- `File3dm.Read(string path) : File3dm` / `Read(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter) : File3dm`
- `File3dm.ReadWithLog(string path, out string errorLog) : File3dm` / `ReadWithLog(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter, out string errorLog) : File3dm`
- `File3dm.FromByteArray(byte[] bytes) : File3dm` — the in-memory archive read
- `File3dm.ReadNotes(string path) : string` / `ReadArchiveVersion(string path) : int`
- `File3dm.ReadRevisionHistory(string path, out string createdBy, out string lastEditedBy, out int revision, out DateTime createdOn, out DateTime lastEditedOn) : bool`
- `File3dm.ReadEarthAnchorPoint(string path) : EarthAnchorPoint` — georeference without materializing geometry
- `File3dm.ReadApplicationData(string path, out string applicationName, out string applicationUrl, out string applicationDetails) : void`
- `File3dm.ReadPageViews(string path) : ViewInfo[]` / `ReadPreviewImage(string path) : Bitmap` / `ReadDimensionStyles(string path) : DimensionStyle[]`

[ENTRYPOINT_SCOPE]: archive writes, serialization, and validity
- rail: host

- `File3dm.WriteOneObject(string path, GeometryBase geometry) : bool` / `WriteMultipleObjects(string path, IEnumerable<GeometryBase> geometry) : bool`
- `File3dm.Write(string path, File3dmWriteOptions options) : bool` / `WriteWithLog(string path, File3dmWriteOptions options, out string errorLog) : bool`
- `File3dm.ToByteArray(File3dmWriteOptions options) : byte[]` — the in-memory archive write feeding the content key
- `File3dm.IsValid`/`Audit`/`Polish` are `[Obsolete]` husks — `IsValid` returns true unconditionally and `Audit` repairs nothing; per-object validity is `CommonObject.IsValidWithLog(out string log) : bool` folded over each `File3dmObject.Geometry`
- `File3dm.GetPreviewImage() : Bitmap` / `SetPreviewImage(Bitmap image) : void`

[ENTRYPOINT_SCOPE]: archive tables and write options
- rail: host

- `File3dm.Settings : File3dmSettings` / `Manifest : ManifestTable` / `Objects : File3dmObjectTable`
- `File3dm.AllLayers : File3dmLayerTable` / `AllMaterials : File3dmMaterialTable` / `AllGroups : File3dmGroupTable` / `AllInstanceDefinitions : File3dmInstanceDefinitionTable`
- `File3dm.AllViews : File3dmViewTable` / `AllNamedViews : File3dmViewTable` / `EmbeddedFiles : File3dmEmbeddedFiles`
- `File3dm.RenderMaterials : File3dmRenderMaterials` / `RenderEnvironments : File3dmRenderEnvironments` / `RenderTextures : File3dmRenderTextures`
- `File3dm.Strings : File3dmStringTable` — `SetString(string section, string entry, string value)` / `Delete(string section, string entry)`
- `File3dm.Notes : File3dmNotes` (get/set) — `File3dmNotes.Notes : string` writes through the parent archive when attached, beside `IsVisible`/`IsHtml`
- `File3dm.ArchiveVersion : int` / `Revision : int` / `CreatedBy`/`LastEditedBy : string` / `Created`/`LastEdited : DateTime` — the in-memory header identity
- `File3dm.ApplicationName`/`ApplicationUrl`/`ApplicationDetails : string` / `EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable) / `AllDimStyles : File3dmDimStyleTable`
- `File3dm.TableTypeFilter` values: `StartSection`/`Properties`/`Settings`/`Bitmap`/`TextureMapping`/`Material`/`Linetype`/`Layer`/`Group`/`Font`/`Dimstyle`/`Light`/`Hatchpattern`/`SectionStyle`/`Markup`/`PageViewGroup`/`InstanceDefinition`/`ObjectTable`/`Historyrecord`/`UserTable`
- `File3dmViewTable.FindName(string name) : ViewInfo` / `Add(ViewInfo)` / `Delete(ViewInfo)`
- `File3dmEmbeddedFile.Filename : string` / `SaveToFile(string path) : bool`
- `File3dmWriteOptions.Version : int` / `SaveUserData : bool`
- `File3dmWriteOptions.EnableRenderMeshes(ObjectType objectType, bool enable) : void` / `EnableAnalysisMeshes(ObjectType objectType, bool enable) : void`

[ENTRYPOINT_SCOPE]: document-attached exchange
- rail: host

- `RhinoDoc.Import(string filePath, ArchivableDictionary options) : bool` / `Export(string filePath, ArchivableDictionary options) : bool` / `ExportSelected(string filePath, ArchivableDictionary options) : bool`
- `RhinoDoc.WriteFile(string path, FileWriteOptions options) : bool` / `Write3dmFile(string path, FileWriteOptions options) : bool`
- `RhinoDoc.Save() : bool` / `SaveAsTemplate(string fileName) : bool`
- `RhinoDoc.SaveAs(string file3dmPath, int version, bool saveSmall, bool saveTextures, bool saveGeometryOnly, bool savePluginData, bool useCompression) : bool`
- `FileWriteOptions.WriteGeometryOnly` / `WriteUserData` / `IncludeRenderMeshes : bool`

[ENTRYPOINT_SCOPE]: direct format engines
- rail: host

- `FileObj.Write(string filename, RhinoDoc doc, FileObjWriteOptions options) : WriteFileResult` / `FileObj.Read(string filename, RhinoDoc doc, FileObjReadOptions options) : bool`
- `File3ds.Write(string path, RhinoDoc doc, File3dsWriteOptions options) : bool` / `File3ds.Read(string path, RhinoDoc doc, File3dsReadOptions options) : bool`
- `FileAi.Write(string path, RhinoDoc doc, FileAiWriteOptions options) : bool` / `FileAi.Read(string path, RhinoDoc doc, FileAiReadOptions options) : bool`
- `FileDwg.Write(string path, RhinoDoc doc, FileDwgWriteOptions options) : bool` / `FileDwg.Read(string path, RhinoDoc doc, FileDwgReadOptions options) : bool`
- `FileFbx.Write(string path, RhinoDoc doc, FileFbxWriteOptions options) : bool` / `FileFbx.Read(string path, RhinoDoc doc, FileFbxReadOptions options) : bool`
- `FilePly.Write(string filename, RhinoDoc doc, FilePlyWriteOptions options) : WriteFileResult` / `FilePly.Read(string path, RhinoDoc doc, FilePlyReadOptions options) : bool`
- `FileSkp.Write(string filename, RhinoDoc doc, FileSkpWriteOptions options) : bool` / `FileSkp.Read(string filename, RhinoDoc doc, FileSkpReadOptions options) : bool`
- `FileStl.Write(string path, RhinoDoc doc, FileStlWriteOptions options) : bool` / `FileStl.Read(string path, RhinoDoc doc, FileStlReadOptions options) : bool`
- `FileStp.Write(string filename, RhinoDoc doc, FileStpWriteOptions options) : bool` / `FileStp.Read(string path, RhinoDoc doc, FileStpReadOptions options) : bool`
- `FileGltf.Write(string filename, RhinoDoc doc, FileGltfWriteOptions options) : bool` / `FileIgs.Write(string path, RhinoDoc doc, FileIgsWriteOptions options) : bool`
- `FileNwd.Write(string path, RhinoDoc doc, FileNwdWriteOptions options) : bool` / `FileUsd.Write(string path, RhinoDoc doc, FileUsdWriteOptions options) : bool` / `FileX_T.Write(string filename, RhinoDoc doc, FileX_TWriteOptions options) : bool`
- `FileSvg.Read(string path, RhinoDoc doc, FileSvgReadOptions options) : bool`

[ENTRYPOINT_SCOPE]: PDF page authoring
- rail: host

- `FilePdf.Create() : FilePdf` — the page-document constructor
- `FilePdf.AddPage(ViewCaptureSettings settings) : int` / `AddPage(int widthInDots, int heightInDots, int dotsPerInch) : int`
- `FilePdf.DrawText(int pageNumber, string text, double x, double y, float heightPoints, Font onfont, Color fillColor, Color strokeColor, float strokeWidth, float angleDegrees, TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment) : void`
- `FilePdf.DrawPolyline(int pageNumber, PointF[] polyline, Color fillColor, Color strokeColor, float strokeWidth) : void`
- `FilePdf.DrawLine(int pageNumber, PointF from, PointF to, Color strokeColor, float strokeWidth) : void`
- `FilePdf.DrawBitmap(int pageNumber, Bitmap bitmap, float left, float top, float width, float height, float rotationInDegrees) : void`
- `FilePdf.LayersAsOptionalContentGroups : bool` / `PreWrite : EventHandler<FilePdfEventArgs>`
- `FilePdf.GetCustomPages() : PrintedPageDefinition[]` / `SetCustomPages(IEnumerable<PrintedPageDefinition> pages) : void`
- `FilePdf.Write(string filename) : void` / `Write(Stream stream) : void` / `Read(string path, RhinoDoc doc, FilePdfReadOptions options) : bool`
- `FilePdfReadOptions.PreserveModelScale : bool` / `RhinoScale : double` / `PdfUnits : FilePdfReadOptions.PDF_UNITS` / `PDFScale : double` / `ImportFillsAsHatches : bool` / `LoadText : bool`

## [04]-[IMPLEMENTATION_LAW]

[FILEIO_TOPOLOGY]:
- `File3dm` is document-free: it reads and writes the archive without opening a `RhinoDoc`, and `TableTypeFilter`/`ObjectTypeFilter` bound a partial read to the tables and object kinds a consumer needs
- the metadata reads (`ReadNotes`, `ReadArchiveVersion`, `ReadRevisionHistory`, `ReadEarthAnchorPoint`, `ReadApplicationData`, `ReadPreviewImage`) touch the header without materializing geometry
- byte serialization through `FromByteArray`/`ToByteArray` is the in-memory archive path the persistence artifact index keys on
- every direct engine takes a live `RhinoDoc` plus a typed `*Options` carrier and returns a `bool` or `WriteFileResult`; the roster is the one conversion surface, and format selection lives in which engine and options a caller picks, never a re-parsed extension string
- `FilePdf` authors vector pages directly, groups layers as optional content, and stamps every page through the static `PreWrite` hook before `Write` commits the document

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the engine `bool`/`WriteFileResult` returns fold to `Fin<A>`, and the out-string diagnostics of `ReadWithLog`, `WriteWithLog`, and `IsValidWithLog` carry into `Error`
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the format-engine roster wraps as a `Union` format vocabulary, `TableTypeFilter`/`ObjectTypeFilter` wrap as flag `SmartEnum` read policy, and the archive write version wraps as a `ValueObject`
- `Hashing`(`libs/csharp/.api/api-hashing.md`): `File3dm.ToByteArray` feeds the `XxHash128` content key the persistence artifact index dedupes archives on

[LOCAL_ADMISSION]:
- standalone archive work enters through `File3dm`; document-attached I/O enters through the `RhinoDoc` operations, and the two paths never fork the same read
- direct conversion enters through the owning engine's `Write`/`Read` with its typed options carrier
- PDF page authoring enters through `FilePdf.Create` then the page and draw surface

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the standalone `.3dm` archive, direct format conversion, PDF page authoring, document-attached exchange
- Accept: filtered, metadata, and byte archive access, typed-option conversion, vector PDF authoring
- Reject: document identity and table mutation, block-definition graph depth, native file-dialog registration
