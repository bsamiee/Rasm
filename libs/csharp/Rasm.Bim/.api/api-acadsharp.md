# [RASM_BIM_API_ACADSHARP]

`ACadSharp` is a pure-managed, MIT-licensed, AnyCPU IL library (multi-targeted `netstandard2.0`/`netstandard2.1` through `net9.0`, resolved here as the `net9.0` facade and consumed under `net10`) reading and writing AutoCAD DXF (ASCII and binary) and DWG (AC1012/R13 through AC1032) files; the BIM exchange folder consumes its READ surface to admit DWG/DXF drawings into the canonical `ImportedGeometry` triangle-soup at the `Exchange/import#IMPORT_RAIL` `BimIo.AcadGeometry` arm, the `acad-sharp` codec the `Exchange/format#FORMAT_AXIS` `Dwg` row carries in-process rather than the `native-companion` two-hop. The dense rail is single-pass: route by the `Dwg`/`Dxf` `InterchangeFormat` extension row, hand a filename to `CadReaderFactory.CreateReader` (or a stream to the format-specific `DxfReader.Read`/`DwgReader.Read`) under a tuned `DwgReaderConfiguration`/`DxfReaderConfiguration` (`Failsafe`, `KeepUnknownEntities`, `ReadSummaryInfo`), fold the reader's `OnNotification` `NotificationType` stream into the `BimIo.Boundary` degradation log, traverse `CadDocument.Entities` (`= ModelSpace.Entities`), discriminate the mesh-bearing families, and for every `Insert` flatten the nested block through `Insert.Explode` (or bake `entity.ApplyTransform(insert.GetTransform)`) so placed `Face3D`/`Mesh` arrive already in WCS — never a hand-rolled `InsertPoint`/scale matrix, and never a hand-sniffed `AC10xx`/`AutoCAD Binary DXF` byte sentinel that `CadReaderFactory.GetFileFormat`/`DxfReader.IsBinary` already own. The Bim consumption is the mesh-bearing entity surface (`Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh`/`Insert`-referenced meshes), distinct from the `Rasm.Fabrication` consumption of the 2D profile entity surface (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) into the `Loop` vocabulary — both folders read one `CadDocument` model root, each projecting the entity families its domain owns. No native asset and no RID burden — the package is managed IL, ALC-safe, osx-arm64-safe, and coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path; the package is already consumed by `Rasm.Fabrication` and `Rasm.AppUi` and centrally pinned at `ACadSharp `. The `CSMath` value/transform algebra (`XYZ`/`XY`/`Matrix4`/`Transform`/`BoundingBox`) and `CSUtilities` ride the dependency closure inside the one assembly resolution.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- license: `MIT`
- assembly: `ACadSharp` (`lib/net9.0/ACadSharp.dll`; multi-target `netstandard2.0`+)
- namespace: `ACadSharp`
- namespace: `ACadSharp.Entities`
- namespace: `ACadSharp.IO`
- namespace: `ACadSharp.Tables` (`BlockRecord`, `Layer`, table roots)
- namespace: `CSMath` (bundled value/transform algebra)
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facades, configuration, and the notification/progress rail
- rail: geometry

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                 |
| :-----: | :------------------------- | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `CadReaderFactory`         | static dispatch | extension-routed `CreateReader`/`GetFileFormat` format facade                |
|  [02]   | `CadFileFormat`            | enum            | `DWG`/`DXF`/`Unknown` format discriminant                                    |
|  [03]   | `DxfReader`                | static reader   | DXF ASCII+binary read; `IsBinary` sniff; `IDisposable`                       |
|  [04]   | `DwgReader`                | static reader   | DWG AC1012–AC1032 read; `ReadPreview`/`ReadSummaryInfo`/`ReadHeader`         |
|  [05]   | `ICadReader`               | reader contract | `OnNotification`/`OnProgress` events, `Read()`/`ReadHeader()`, `IDisposable` |
|  [06]   | `CadReaderConfiguration`   | base config     | `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` knobs      |
|  [07]   | `DwgReaderConfiguration`   | DWG config      | adds `CrcCheck`, `ReadSummaryInfo` (default true)                            |
|  [08]   | `DxfReaderConfiguration`   | DXF config      | adds `ClearCache` (default true), `CreateDefaults`                           |
|  [09]   | `NotificationEventHandler` | delegate        | `NotificationEventArgs` carrier — `Message`, `NotificationType`, `Exception` |
|  [10]   | `NotificationType`         | enum            | `NotImplemented`/`None`/`NotSupported`/`Warning`/`Error` severity            |
|  [11]   | `ProgressEventHandler`     | delegate        | `(object sender, ProgressEventArgs e)` read-progress stream                  |
|  [12]   | `ProgressEventArgs`        | progress args   | `Stage` (`ReadStage`) + `Current` (`CadObjectData`) per progress event       |

[PUBLIC_TYPE_SCOPE]: document root and tables — the `CadDocument` model
- rail: geometry

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :--------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `CadDocument`    | model root    | `Entities`, `ModelSpace`/`PaperSpace`, `BlockRecords`, `Header`, `SummaryInfo`, `Layers` |
|  [02]   | `BlockRecord`    | block table   | named entity container — `Entities` is the nested-block geometry an `Insert` references  |
|  [03]   | `CadHeader`      | header        | drawing units/version metadata behind `CadDocument.Header`                               |
|  [04]   | `CadSummaryInfo` | summary       | title/author metadata (DWG gated by `DwgReaderConfiguration.ReadSummaryInfo`)            |

[PUBLIC_TYPE_SCOPE]: mesh-bearing entity types — the Bim triangle-soup surface
- rail: geometry

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                                                                                    |
| :-----: | :----------------- | :--------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Mesh`             | SubDMesh         | `AcDbSubDMesh` — `Vertices` `List<XYZ>`, `Faces` `List<int[]>`, `Edges`, `SubdivisionLevel`     |
|  [02]   | `Mesh.Edge`        | per-edge struct  | `Start`/`End` 0-based vertex index, optional `Crease` (`double?`)                               |
|  [03]   | `Face3D`           | 3D face          | `3DFACE` tri/quad — `FirstCorner`..`FourthCorner` (`XYZ` WCS), `Flags` (`InvisibleEdgeFlags`)   |
|  [04]   | `PolyfaceMesh`     | polyface mesh    | `Polyline<VertexFaceMesh>` — `Vertices` pool + `Faces` `VertexFaceRecord` records               |
|  [05]   | `VertexFaceMesh`   | polyface vertex  | a `Vertex` carrying the `Location` `XYZ` of one polyface corner                                 |
|  [06]   | `VertexFaceRecord` | polyface face    | 1-based signed `Index1`..`Index4` (`short`; negative = hidden edge, 0 = unused)                 |
|  [07]   | `PolygonMesh`      | M×N surface mesh | `Polyline<PolygonMeshVertex>` — `MVertexCount`×`NVertexCount` grid, `M`/`NSmoothSurfaceDensity` |
|  [08]   | `Insert`           | block reference  | placed/arrayed nested-block reference — flattened via `Explode`/`GetTransform`                  |

[PUBLIC_TYPE_SCOPE]: entity base and CSMath value/transform algebra
- rail: geometry

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Entity`      | entity base   | graphic style, package-owned TRS bakes, `GetBoundingBox()` — see [01]                              |
|  [02]   | `XYZ`         | 3D vector     | `X`/`Y`/`Z` + indexer, `AxisX/Y/Z`/`Zero`, `+`/`-`/`*`/`/`, `Cross`/`FindNormal`/`GetAngle`        |
|  [03]   | `XY`          | 2D point      | `X`/`Y` doubles; explicit cast to/from `XYZ`                                                       |
|  [04]   | `Transform`   | TRS transform | `Matrix` (`Matrix4`), `Translation`/`Scale`/`EulerRotation`, `ApplyTransform(XYZ)`, `TryDecompose` |
|  [05]   | `Matrix4`     | 4×4 matrix    | the affine matrix `Transform.Matrix`/`Insert.GetTransform` composes                                |
|  [06]   | `BoundingBox` | AABB          | `Entity.GetBoundingBox()` extent for soup-bounds accumulation                                      |

- [01]-[ENTITY_BASE]: `Layer`/`Color`/`LineWeight`/`Transparency`/`Material`/`IsInvisible` graphic props; package-owned bakes `ApplyTransform(Transform)`/`ApplyTranslation`/`ApplyRotation(axis,θ)`/`ApplyScaling(scale[,origin])`; `GetBoundingBox()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format dispatch and file/stream read — `CadReaderFactory` / `DxfReader` / `DwgReader`; every read/create overload takes a trailing `NotificationEventHandler = null`
- rail: geometry

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `CadReaderFactory.CreateReader(string filename)`                 | format dispatch | extension-routed `ICadReader` (DWG/DXF)           |
|  [02]   | `CadReaderFactory.GetFileFormat(string filename)`                | format probe    | `CadFileFormat.{DWG,DXF,Unknown}` from extension  |
|  [03]   | `DxfReader.IsBinary(Stream stream, bool resetPos)`               | binary sniff    | ascii-vs-binary DXF sniff (`IsBinary(string)`)    |
|  [04]   | `DxfReader.Read(Stream stream)`                                  | static read     | DXF stream → `CadDocument` (binary auto-detected) |
|  [05]   | `DxfReader.Read(string filename, DxfReaderConfiguration)`        | static read     | DXF file read under a tuned config                |
|  [06]   | `DwgReader.Read(Stream stream)`                                  | static read     | DWG stream → `CadDocument` (no-config overload)   |
|  [07]   | `DwgReader.Read(Stream stream, DwgReaderConfiguration)`          | static read     | DWG stream read under a tuned config              |
|  [08]   | `DwgReader.Read(string filename)`                                | static read     | DWG file read by path → `CadDocument`             |
|  [09]   | `DwgReader.ReadSummaryInfo()` / `ReadPreview()` / `ReadHeader()` | partial read    | summary/preview/header without full entity parse  |
|  [10]   | `DxfReader.ReadEntities()` / `ReadTables()`                      | partial read    | section-scoped DXF read → `List<Entity>`          |
|  [11]   | `new DxfReader(Stream)` / `new DwgReader(Stream)`                | instance read   | `ICadReader.Read()` with the `OnProgress` event   |

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → mesh-bearing entities
- rail: geometry

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]                        | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------- | :------------------------------------ | :---------------------------------------- |
|  [01]   | `CadDocument.Entities` (= `ModelSpace.Entities`)    | `CadObjectCollection<Entity>`         | top-level model-space entity set          |
|  [02]   | `CadDocument.BlockRecords` / `BlockRecord.Entities` | block table                           | nested-block geometry set                 |
|  [03]   | `Mesh.Vertices` / `Mesh.Faces`                      | `List<XYZ>` / `List<int[]>`           | SubDMesh verts + 0-based n-gon faces      |
|  [04]   | `Face3D.FirstCorner`..`FourthCorner`                | `XYZ` (WCS)                           | 3DFACE corners (4th==3rd → tri)           |
|  [05]   | `PolyfaceMesh.Vertices` / `PolyfaceMesh.Faces`      | `VertexFaceMesh` / `VertexFaceRecord` | polyface pool + signed-index face records |

[ENTRYPOINT_SCOPE]: block-reference flattening — `Insert` placement, the canonical no-hand-roll path (members on `Insert`)
- rail: geometry

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]        | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------- | :-------------------- | :-------------------------------------------------------- |
|  [01]   | `Explode()`                                        | `IEnumerable<Entity>` | resolved block → placed entities already in WCS           |
|  [02]   | `GetTransform()`                                   | `Transform`           | placement `Transform` (`Matrix4`) for `ApplyTransform`    |
|  [03]   | `Block` / `InsertPoint`                            | `BlockRecord` / `XYZ` | referenced block record + WCS origin                      |
|  [04]   | `XScale`/`YScale`/`ZScale` / `Rotation` / `Normal` | placement             | per-axis scale/rotation/OCS normal for `GetTransform`     |
|  [05]   | `ColumnCount`/`RowCount`                           | MInsert grid          | `MINSERT` array row/column counts                         |
|  [06]   | `ColumnSpacing`/`RowSpacing` / `IsMultiple`        | MInsert grid          | grid spacing; `IsMultiple` discriminates a grid placement |

## [04]-[IMPLEMENTATION_LAW]

[BIM_MESH_ADMISSION]:
- The Bim arm reads ONLY the mesh-bearing entities into the canonical `ImportedGeometry` triangle-soup: `Mesh` (the `Vertices` `List<XYZ>` plus the `Faces` `List<int[]>` 0-based n-gon index list, each face fan-triangulated `(face[0], face[k], face[k+1])`), `Face3D` (the `FirstCorner`..`FourthCorner` `XYZ` quad, fan-triangulated, the fourth corner equal to the third signaling a triangle), `PolyfaceMesh` (the `Vertices` `VertexFaceMesh` pool indexed by the `Faces` `VertexFaceRecord` 1-based signed `Index1`..`Index4`, a negative index marking a hidden edge and a zero index marking an unused slot, so a quad with `Index4==0` is a triangle), `PolygonMesh` (the M×N surface vertex grid), and the `Insert`-referenced mesh set.
- An `Insert` is flattened through `Insert.Explode()` returning the block's entities already placed in WCS, OR by baking the placement with `entity.ApplyTransform(insert.GetTransform())` against the resolved `Insert.Block.Entities` — never a hand-rolled `InsertPoint`/`XScale`/`YScale`/`ZScale`/`Rotation`/`Normal` matrix; `CSMath.Transform` (`Insert.GetTransform`) owns the OCS→WCS composition and `Entity.ApplyTransform` owns the per-entity bake. A `MINSERT` array (`IsMultiple`, `ColumnCount`/`RowCount` > 1) replicates the block across the `ColumnSpacing`/`RowSpacing` grid; `Explode` yields every instance.
- A `Line`/`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline` 2D profile entity is the `Rasm.Fabrication` `Ingress/profile#PROFILE_IMPORT` concern (sampled into the `Loop` vocabulary), not a triangle-soup contributor — the Bim arm skips it; admitting one drawing into two folders is correct, each projecting its owned entity families from one `CadDocument`.

[FORMAT_DISPATCH]:
- Format selection is `CadReaderFactory.GetFileFormat`/`CreateReader` (extension→`CadFileFormat`→`ICadReader`) for the filename path, and the format-specific `DxfReader.Read`/`DwgReader.Read` for the stream path routed by the `Dwg`/`Dxf` `InterchangeFormat` extension set. The DXF ascii-vs-binary split is `DxfReader.IsBinary(stream)` (and the reader auto-detects it internally), and the DWG version is read from the file header — the Bim arm does NOT hand-sniff the `AC10xx` / `0\nSECTION` / `AutoCAD Binary DXF` byte sentinels the package already owns.
- The DWG read floor is `AC1012` (R13): `DwgFileHeader.CreateFileHeader` throws `CadNotSupportedException(version)` for `AC1009` (R11/R12) and earlier, and supports `AC1012`/`AC1014`/`AC1015`/`AC1018`/`AC1021`/`AC1024`/`AC1027`/`AC1032`. DWG `SummaryInfo` is itself gated at `AC1018` (`ReadSummaryInfo` short-circuits below it).
- A mesh-only ingress that needs neither header nor non-graphical objects reads the DXF ENTITIES section alone through the instance `DxfReader.ReadEntities()` (`List<Entity>`) rather than the full `Read()` — the section-scoped read skips the header/objects/classes parse, then the same mesh-family discrimination folds the entity list onto the soup. The DWG path has no section-scoped entity read (`DwgReader` partial reads are header/preview/summary only), so DWG always takes the full `Read()`.

[BOUNDARY_AND_NOTIFICATION]:
- The reader is configured at the boundary: `CadReaderConfiguration.Failsafe` (default true) keeps recoverable section/entity errors as `OnNotification` events rather than throws; `KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` (default false) drop proxy/unknown objects the soup never reads; `DwgReaderConfiguration.ReadSummaryInfo` is set false when only geometry is needed; `DxfReaderConfiguration.ClearCache`/`CreateDefaults` tune DXF section reuse. The `BimIo.Boundary` subscribes `ICadReader.OnNotification` and folds the `NotificationEventArgs` stream — `NotificationType` severity (`Warning`/`Error`/`NotSupported`/`NotImplemented`), `Message`, and the optional carried `Exception` (the recovered defect's detail under `Failsafe`) — into its degradation log without aborting the read; the `OnProgress` (`ProgressEventArgs` — `Stage` `ReadStage`, `Current` `CadObjectData`, no fraction) stream rides the instance readers only (`new DxfReader(Stream)`/`new DwgReader(Stream)`, never the static `Read` facades) and fires the `Model/observability#HOOK_RAIL` `rasm.bim.exchange.progress` observe point when a composition mounts `BimHooks`.
- The `DxfReader.Read`/`DwgReader.Read` facade (and `CadReaderFactory.CreateReader().Read()`) THROWS on a malformed/unreadable file (`CadNotSupportedException`, DXF/DWG parse exceptions) — the Bim arm wraps the call in the `BimIo.Boundary` funnel and lowers the caught exception to `BimFault.ModelRejected` ONCE at admission (the reader exception never escapes the boundary); under `Failsafe` a non-fatal defect arrives as an `OnNotification` event, not a throw.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DWG/DXF format dispatch and stream/file read into the `CadDocument` model, the reader configuration/notification rail, and the mesh-bearing entity surface the `Exchange/import#IMPORT_RAIL` `BimIo.AcadGeometry` arm folds into `ImportedGeometry` triangle-soup
- Accept: a DWG/DXF stream or filename routed through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config; the `Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh`/`Insert`-referenced entities folded onto the canonical triangle-soup; the `Insert` placement flattened through `Explode`/`GetTransform`/`Entity.ApplyTransform`; `NotificationType` events folded into the boundary log
- Reject: writing DWG/DXF from the Bim folder (`DxfWriter`/`DwgWriter`/`SvgWriter` are out of scope — the codec is read-only ingress; `Rasm.AppUi` owns the managed ACadSharp write leg), a `CadDocument` or an ACadSharp entity type escaping the `BimIo` boundary into a domain signature, admitting a second managed CAD library (`netDxf`) for the same row where `ACadSharp` supersedes it, a reader exception escaping the boundary unlowered, hand-rolling format/binary detection or an `Insert` placement matrix the package already owns, and re-reading the 2D profile entities the `Rasm.Fabrication` boundary owns
