# [RASM_RHINO_API_RHINOCOMMON_FILEIO]

`File3dm` owns standalone `.3dm` archives outside a live document: filtered and metadata-only reads, table mutation, write policy, nullable byte serialization, and preview ownership. Direct format engines accept typed option carriers for document-attached interchange, `FilePdf` owns vector page authoring, and `RhinoDoc` owns import, export, save, template, and general write lifecycles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`

- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.FileIO`, `Rhino.DocObjects` (`EarthAnchorPoint`), `Rhino` (`RhinoDoc` document-attached I/O)
- asset: `RhinoCommon.dll` — the in-process managed host assembly
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

Direct engines receive their typed option carrier directly; `RhinoDoc.Import`/`Export`/`ExportSelected` instead receive `ArchivableDictionary?`, and only option types that declare `ToDictionary()` project into that lane. `FileObjReadOptions(FileReadOptions)`, `FileObjWriteOptions(FileWriteOptions)`, and `FilePlyWriteOptions(FileWriteOptions)` consume the shared host carriers without declaring `ToDictionary()`. Exchange-relevant knobs include `File3dsWriteOptions.SaveViews`/`SaveLights`; the AI/EPS model-scale fields; the OBJ and PLY grouping, material, normal, and mesh fields; the DWG surface/color fields; `FileStlWriteOptions.BinaryFile`; the FBX object/view/light/normal fields; the SketchUp grouping field; the VRML/X3DV texture-coordinate and normal fields; the text/CSV column fields; the glTF Draco family; `FileUsdWriteOptions.ForceMeshes`/`IncludeUserStrings`/`BlockHandling`/`ModelName`; and `FileXamlWriteOptions.UseExistingRenderMeshes`.

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

- runtime-nullable `public static File3dm? File3dm.Read(string path)` / `Read(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter)`; a missing path throws `FileNotFoundException`, while a native read failure returns `null`
- runtime-nullable `public static File3dm? File3dm.ReadWithLog(string path, out string errorLog)` / `ReadWithLog(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter, out string errorLog)`
- runtime-nullable `public static File3dm? File3dm.FromByteArray(byte[] bytes)`
- `public static string File3dm.ReadNotes(string path)` / `public static int ReadArchiveVersion(string path)`
- `public static bool File3dm.ReadRevisionHistory(string path, out string createdBy, out string lastEditedBy, out int revision, out DateTime createdOn, out DateTime lastEditedOn)`
- runtime-nullable `public static EarthAnchorPoint? File3dm.ReadEarthAnchorPoint(string path)`; the caller owns the returned disposable value
- `public static void File3dm.ReadApplicationData(string path, out string applicationName, out string applicationUrl, out string applicationDetails)`
- `public static ViewInfo[] File3dm.ReadPageViews(string path)` returns an empty array when the file is absent or the native read fails
- runtime-nullable `public static Bitmap? File3dm.ReadPreviewImage(string path)` / `public static DimensionStyle[]? ReadDimensionStyles(string path)`; an absent path throws for both

[ENTRYPOINT_SCOPE]: archive writes, serialization, and validity

- rail: host

- `public static bool File3dm.WriteOneObject(string path, GeometryBase geometry)` / `public static bool WriteMultipleObjects(string path, IEnumerable<GeometryBase> geometry)` create standalone minimal archives
- `public bool File3dm.Write(string path, int version)` / `public bool Write(string path, File3dmWriteOptions? options)`; `null` options create defaults
- `public bool File3dm.WriteWithLog(string path, int version, out string errorLog)` / `public bool WriteWithLog(string path, File3dmWriteOptions? options, out string errorLog)`
- runtime-nullable `public byte[]? File3dm.ToByteArray()` / `public byte[]? ToByteArray(File3dmWriteOptions? options)`; native serialization failure returns `null`
- `[Obsolete]` `File3dm.IsValid` returns true, and `[Obsolete]` `Audit`/`Polish` do not provide archive validation; `public bool CommonObject.IsValidWithLog(out string log)` is the per-object validity surface
- runtime-nullable `public Bitmap? File3dm.GetPreviewImage()` / `public void SetPreviewImage(Bitmap? image)`; the returned bitmap is caller-owned, `null` clears the stored preview, and a non-null image is copied into native storage during the call

[ENTRYPOINT_SCOPE]: archive tables and write options

- rail: host

- `File3dm.Settings : File3dmSettings` / `Manifest : ManifestTable` / `Objects : File3dmObjectTable`
- `File3dmSettings.ModelUnits : LengthUnit` / `PageUnits : LengthUnit` preserve built-in or custom unit identity and meters-per-unit scale; `ModelUnitSystem`/`PageUnitSystem : UnitSystem` are the enum-only projections
- `File3dm.AllLayers : File3dmLayerTable` / `AllMaterials : File3dmMaterialTable` / `AllGroups : File3dmGroupTable` / `AllInstanceDefinitions : File3dmInstanceDefinitionTable`
- `File3dm.AllViews : File3dmViewTable` / `AllNamedViews : File3dmViewTable` / `EmbeddedFiles : File3dmEmbeddedFiles`
- `File3dm.RenderMaterials : File3dmRenderMaterials` / `RenderEnvironments : File3dmRenderEnvironments` / `RenderTextures : File3dmRenderTextures`
- `File3dm.Strings : File3dmStringTable` — `SetString(string section, string entry, string value)` / `Delete(string section, string entry)`
- `File3dm.Notes : File3dmNotes` (get/set) — `File3dmNotes.Notes : string` writes through the parent archive when attached, beside `IsVisible`/`IsHtml`
- `File3dm.ArchiveVersion : int` / `Revision : int` / `CreatedBy`/`LastEditedBy : string` / `Created`/`LastEdited : DateTime` — the in-memory header identity
- `File3dm.ApplicationName`/`ApplicationUrl`/`ApplicationDetails : string` / `EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable) / `AllDimStyles : File3dmDimStyleTable`
- `File3dm.TableTypeFilter` values: `StartSection`/`Properties`/`Settings`/`Bitmap`/`TextureMapping`/`Material`/`Linetype`/`Layer`/`Group`/`Font`/`Dimstyle`/`Light`/`Hatchpattern`/`SectionStyle`/`Markup`/`PageViewGroup`/`InstanceDefinition`/`ObjectTable`/`Historyrecord`/`UserTable`
- `public override void File3dmObjectTable.Add(File3dmObject item)` duplicates a model component and throws `NotSupportedException` when native addition fails
- `public Guid File3dmObjectTable.Add(GeometryBase item, ObjectAttributes? attributes)` dispatches supported geometry kinds and throws `NotSupportedException` for an unsupported kind; no one-argument polymorphic `Add(GeometryBase)` exists
- typed object-table overloads include `AddPoint`, `AddCurve`, `AddExtrusion`, `AddMesh`, `AddBrep`, and `AddSubD`, each with a no-attributes overload and an `ObjectAttributes?` overload returning `Guid`
- runtime-nullable `public ViewInfo? File3dmViewTable.FindName(string name)` / `public void Add(ViewInfo item)` / `public bool Delete(ViewInfo item)`
- `public string File3dmEmbeddedFile.Filename { get; }` / `public bool SaveToFile(string filename)`
- `public int File3dmWriteOptions.Version { get; set; }` admits `0` or `[2, RhinoApp.ExeVersion]`; the constructor defaults to `RhinoApp.ExeVersion`. `public bool SaveUserData { get; set; }` defaults true
- `public void File3dmWriteOptions.EnableRenderMeshes(ObjectType objectType, bool enable)` / `public void EnableAnalysisMeshes(ObjectType objectType, bool enable)` mutate the per-kind flags; render meshes principally apply to brep, extrusion, and SubD, while analysis meshes additionally apply to mesh

[ENTRYPOINT_SCOPE]: document-attached exchange

- rail: host

- `public bool RhinoDoc.Import(string filePath)` / `public bool Import(string filePath, ArchivableDictionary? options)`; `public bool Export(string filePath)` / `public bool Export(string filePath, ArchivableDictionary? options)`; `public bool ExportSelected(string filePath)` / `public bool ExportSelected(string filePath, ArchivableDictionary? options)`
- `public bool RhinoDoc.WriteFile(string path, FileWriteOptions options)` is the general writer with plug-in, locking, temporary-file, and backup handling; `public bool Write3dmFile(string path, FileWriteOptions options)` writes `.3dm` without changing document identity
- `public bool RhinoDoc.Save()` writes `RhinoDoc.Path` and throws `InvalidOperationException` when it is empty
- `public bool RhinoDoc.SaveAs(string file3dmPath)` / `SaveAs(string file3dmPath, int version)` / `SaveAs(string file3dmPath, int version, bool saveSmall, bool saveTextures, bool saveGeometryOnly, bool savePluginData)` / `SaveAs(string file3dmPath, int version, bool saveSmall, bool saveTextures, bool saveGeometryOnly, bool savePluginData, bool useCompression)` update document path on success
- `public bool RhinoDoc.SaveAsTemplate(string file3dmTemplatePath)` / `SaveAsTemplate(string file3dmTemplatePath, int version)` preserve document path and require a `.3dm` extension
- `FileWriteOptions`: `UpdateDocumentPath`, `WriteSelectedObjectsOnly`, `IncludeRenderMeshes`, `IncludePreviewImage`, `IncludeBitmapTable`, `IncludeHistory`, `SuppressDialogBoxes`, `SuppressAllInput`, `WriteGeometryOnly`, `WriteUserData`, `CreateBackupFiles`, `CreateOtherBackupFiles`, and `UseCompression` are public mutable booleans; `WriteAsTemplate` is get-only

[ENTRYPOINT_SCOPE]: direct format engines

- rail: host

- `public static WriteFileResult FileObj.Write(string filename, RhinoDoc doc, FileObjWriteOptions options)` / `public static bool FileObj.Read(string filename, RhinoDoc doc, FileObjReadOptions options)`
- `public static bool File3ds.Write(string path, RhinoDoc doc, File3dsWriteOptions options)` / `public static bool File3ds.Read(string path, RhinoDoc doc, File3dsReadOptions options)`
- `public static bool FileAi.Write(string path, RhinoDoc doc, FileAiWriteOptions options)` / `public static bool FileAi.Read(string path, RhinoDoc doc, FileAiReadOptions options)`
- `public static bool FileDwg.Write(string path, RhinoDoc doc, FileDwgWriteOptions options)` / `public static bool FileDwg.Read(string path, RhinoDoc doc, FileDwgReadOptions options)`
- `public static bool FileFbx.Write(string path, RhinoDoc doc, FileFbxWriteOptions options)` / `public static bool FileFbx.Read(string path, RhinoDoc doc, FileFbxReadOptions options)`
- `public static WriteFileResult FilePly.Write(string filename, RhinoDoc doc, FilePlyWriteOptions options)` / `public static bool FilePly.Read(string path, RhinoDoc doc, FilePlyReadOptions options)`
- `public static bool FileSkp.Write(string filename, RhinoDoc doc, FileSkpWriteOptions options)` / `public static bool FileSkp.Read(string path, RhinoDoc doc, FileSkpReadOptions options)`
- `public static bool FileStl.Write(string path, RhinoDoc doc, FileStlWriteOptions options)` / `public static bool FileStl.Read(string path, RhinoDoc doc, FileStlReadOptions options)`
- `public static bool FileStp.Write(string filename, RhinoDoc doc, FileStpWriteOptions options)` / `public static bool FileStp.Read(string path, RhinoDoc doc, FileStpReadOptions options)`
- `public static bool FileGltf.Write(string filename, RhinoDoc doc, FileGltfWriteOptions options)` / `public static bool FileIgs.Write(string path, RhinoDoc doc, FileIgsWriteOptions options)`
- `public static bool FileNwd.Write(string path, RhinoDoc doc, FileNwdWriteOptions options)` / `public static bool FileUsd.Write(string path, RhinoDoc doc, FileUsdWriteOptions options)` / `public static bool FileX_T.Write(string filename, RhinoDoc doc, FileX_TWriteOptions options)`
- `public static bool FileSvg.Read(string path, RhinoDoc doc, FileSvgReadOptions options)`

[ENTRYPOINT_SCOPE]: PDF page authoring

- rail: host

- runtime-nullable `public static FilePdf? FilePdf.Create()`; the PDF plug-in lookup can return `null`
- `public abstract int FilePdf.AddPage(ViewCaptureSettings settings)` / `public abstract int AddPage(int widthInDots, int heightInDots, int dotsPerInch)`
- `public abstract void FilePdf.DrawText(int pageNumber, string text, double x, double y, float heightPoints, Font onfont, Color fillColor, Color strokeColor, float strokeWidth, float angleDegrees, TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment)`
- `public abstract void FilePdf.DrawPolyline(int pageNumber, PointF[] polyline, Color fillColor, Color strokeColor, float strokeWidth)`
- `public void FilePdf.DrawLine(int pageNumber, PointF from, PointF to, Color strokeColor, float strokeWidth)`
- `public abstract void FilePdf.DrawBitmap(int pageNumber, Bitmap bitmap, float left, float top, float width, float height, float rotationInDegrees)`
- `public bool FilePdf.LayersAsOptionalContentGroups { get; set; }` / runtime-nullable `public static event EventHandler<FilePdfEventArgs>? PreWrite`
- `public static PrintedPageDefinition[] FilePdf.GetCustomPages()` / `public static void SetCustomPages(IEnumerable<PrintedPageDefinition>? pages)`; `null` clears the custom-page set
- `public abstract void FilePdf.Write(string filename)` / `public abstract void Write(Stream stream)` / `public static bool Read(string path, RhinoDoc doc, FilePdfReadOptions options)`
- `FilePdfReadOptions.PreserveModelScale : bool` / `RhinoScale : double` / `PdfUnits : FilePdfReadOptions.PDF_UNITS` / `PDFScale : double` / `ImportFillsAsHatches : bool` / `LoadText : bool`

## [04]-[IMPLEMENTATION_LAW]

[FILEIO_TOPOLOGY]:

- `File3dm` is document-free: it reads, mutates, serializes, and writes the archive without opening a `RhinoDoc`, and `TableTypeFilter`/`ObjectTypeFilter` bound a partial read to the required tables and object kinds
- the metadata reads (`ReadNotes`, `ReadArchiveVersion`, `ReadRevisionHistory`, `ReadEarthAnchorPoint`, `ReadApplicationData`, `ReadPreviewImage`) touch the header without materializing geometry
- byte serialization through `FromByteArray`/`ToByteArray` is runtime-nullable in both directions, and a consumer admits the value before hashing or dereferencing it
- every direct engine takes a live `RhinoDoc` plus a typed `*Options` carrier and returns a `bool` or `WriteFileResult`; the roster is the one conversion surface, and format selection lives in which engine and options a caller picks, never a re-parsed extension string
- `FilePdf` authors vector pages directly, groups layers as optional content, and stamps every page through the static `PreWrite` hook before `Write` commits the document

[STACKING]:

- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): nullable archive/bitmap/style/byte returns and engine `bool`/`WriteFileResult` outcomes fold to `Option<T>`/`Fin<T>`, while nonempty `ReadWithLog`/`WriteWithLog`/`IsValidWithLog` diagnostics remain typed evidence or fault detail
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
