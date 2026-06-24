# [RASM_BIM_API_ACADSHARP]

`ACadSharp` is a pure-managed, MIT-licensed, AnyCPU IL library (multi-targeted `netstandard2.0`/`netstandard2.1` through `net9.0`, resolved here as the `net9.0` facade and consumed under `net10`) reading and writing AutoCAD DXF (ASCII and binary) and DWG (AC1012/R13 through AC1032) files; the BIM exchange folder consumes its READ surface to admit DWG/DXF drawings into the canonical `ImportedGeometry` triangle-soup at the `Exchange/import#IMPORT_RAIL` `BimIo.AcadGeometry` arm, the `acad-sharp` codec the `Exchange/format#FORMAT_AXIS` `Dwg` row carries in-process rather than the `native-companion` two-hop. The dense rail is single-pass: route by the `Dwg`/`Dxf` `InterchangeFormat` extension row, hand a filename to `CadReaderFactory.CreateReader` (or a stream to the format-specific `DxfReader.Read`/`DwgReader.Read`) under a tuned `DwgReaderConfiguration`/`DxfReaderConfiguration` (`Failsafe`, `KeepUnknownEntities`, `ReadSummaryInfo`), fold the reader's `OnNotification` `NotificationType` stream into the `BimIo.Boundary` degradation log, traverse `CadDocument.Entities` (`= ModelSpace.Entities`), discriminate the mesh-bearing families, and for every `Insert` flatten the nested block through `Insert.Explode()` (or bake `entity.ApplyTransform(insert.GetTransform())`) so placed `Face3D`/`Mesh` arrive already in WCS — never a hand-rolled `InsertPoint`/scale matrix, and never a hand-sniffed `AC10xx`/`AutoCAD Binary DXF` byte sentinel that `CadReaderFactory.GetFileFormat`/`DxfReader.IsBinary` already own. The Bim consumption is the mesh-bearing entity surface (`Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh`/`Insert`-referenced meshes), distinct from the `Rasm.Fabrication` consumption of the 2D profile entity surface (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) into the `Loop` vocabulary — both folders read one `CadDocument` model root, each projecting the entity families its domain owns. No native asset and no RID burden — the package is managed IL, ALC-safe, osx-arm64-safe, and coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path; the package is already consumed by `Rasm.Fabrication` and `Rasm.AppUi` and centrally pinned at `ACadSharp 3.6.29`. The `CSMath` value/transform algebra (`XYZ`/`XY`/`Matrix4`/`Transform`/`BoundingBox`) and `CSUtilities` ride the dependency closure inside the one assembly resolution.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- version: `3.6.29`
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

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :------------------------ | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `CadReaderFactory`        | static dispatch    | extension-routed `CreateReader`/`GetFileFormat` — the one format-dispatch facade    |
|  [02]   | `CadFileFormat`           | enum               | `DWG` / `DXF` / `Unknown` — the routed format discriminant                          |
|  [03]   | `DxfReader`               | static reader      | DXF (ASCII + binary) read; `IsBinary` ascii/binary sniff; `IDisposable` instance    |
|  [04]   | `DwgReader`               | static reader      | DWG (AC1012–AC1032) read; `ReadPreview`/`ReadSummaryInfo`/`ReadHeader`              |
|  [05]   | `ICadReader`              | reader contract    | `OnNotification`/`OnProgress` events + `Read()`/`ReadHeader()`, `IDisposable`       |
|  [06]   | `CadReaderConfiguration`  | base config        | `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` degrade knobs      |
|  [07]   | `DwgReaderConfiguration`  | DWG config         | adds `CrcCheck`, `ReadSummaryInfo` (default true)                                   |
|  [08]   | `DxfReaderConfiguration`  | DXF config         | adds `ClearCache` (default true), `CreateDefaults`                                  |
|  [09]   | `NotificationEventHandler`| delegate           | the boundary subscribes for `NotificationEventArgs` (message + `NotificationType`)  |
|  [10]   | `NotificationType`        | enum               | `NotImplemented`/`None`/`NotSupported`/`Warning`/`Error` severity                   |
|  [11]   | `ProgressEventHandler`    | delegate           | `ProgressEventArgs` read-progress stream (optional)                                 |

[PUBLIC_TYPE_SCOPE]: document root and tables — the `CadDocument` model
- rail: geometry

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `CadDocument`       | model root    | the read drawing — `Entities`, `ModelSpace`/`PaperSpace`, `BlockRecords`, `Header`, `SummaryInfo`, `Layers` |
|  [02]   | `BlockRecord`       | block table   | a named entity container — `Entities` is the nested-block geometry an `Insert` references |
|  [03]   | `CadHeader`         | header        | drawing units/version metadata behind `CadDocument.Header`                     |
|  [04]   | `CadSummaryInfo`    | summary       | title/author metadata (DWG read gated by `DwgReaderConfiguration.ReadSummaryInfo`) |

[PUBLIC_TYPE_SCOPE]: mesh-bearing entity types — the Bim triangle-soup surface
- rail: geometry

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :----------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Mesh`              | SubDMesh           | `AcDbSubDMesh` — `Vertices` `List<XYZ>` + `Faces` `List<int[]>` (0-based n-gon) + `Edges`; `SubdivisionLevel` |
|  [02]   | `Mesh.Edge`         | per-edge struct    | `Start`/`End` 0-based vertex index plus optional `Crease` (`double?`)         |
|  [03]   | `Face3D`            | 3D face            | `3DFACE` triangle/quad — `FirstCorner`..`FourthCorner` (`XYZ` WCS) + `Flags` (`InvisibleEdgeFlags`) |
|  [04]   | `PolyfaceMesh`      | legacy polyface    | `Polyline<VertexFaceMesh>` — `Vertices` `SeqendCollection<VertexFaceMesh>`, `Faces` `CadObjectCollection<VertexFaceRecord>` |
|  [05]   | `VertexFaceMesh`    | polyface vertex    | a `Vertex` carrying the `Location` `XYZ` of one polyface corner                |
|  [06]   | `VertexFaceRecord`  | polyface face      | 1-based signed `Index1`..`Index4` (`short`; negative = hidden edge, 0 = unused) |
|  [07]   | `PolygonMesh`       | M×N surface mesh   | `Polyline<PolygonMeshVertex>` row/column surface mesh (`SmoothSurfaceType`)    |
|  [08]   | `Insert`            | block reference    | placed/arrayed nested-block reference — flattened via `Explode`/`GetTransform` |

[PUBLIC_TYPE_SCOPE]: entity base and CSMath value/transform algebra
- rail: geometry

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :-------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Entity`          | entity base     | every entity: `Layer`/`Color`/`LineWeight`/`Transparency` + `ApplyTransform(Transform)` + `GetBoundingBox()` |
|  [02]   | `XYZ`             | 3D vector       | `X`/`Y`/`Z` doubles, indexer, `AxisX`/`AxisY`/`AxisZ`/`Zero`, `+`/`-`/`*`/`/` ops, `Cross`/`FindNormal`/`GetAngle` |
|  [03]   | `XY`              | 2D point        | `X`/`Y` doubles; explicit cast to/from `XYZ`                                   |
|  [04]   | `Transform`       | TRS transform   | `Matrix` (`Matrix4`), `Translation`/`Scale`/`EulerRotation`, `ApplyTransform(XYZ)`, `TryDecompose` |
|  [05]   | `Matrix4`         | 4×4 matrix      | the affine matrix `Transform.Matrix`/`Insert.GetTransform` composes            |
|  [06]   | `BoundingBox`     | AABB            | `Entity.GetBoundingBox()` extent for soup-bounds accumulation                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format dispatch and file/stream read — `CadReaderFactory` / `DxfReader` / `DwgReader`
- rail: geometry

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :--------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `CadReaderFactory.CreateReader(string filename, NotificationEventHandler = null)`  | format dispatch  | extension-routed `ICadReader` (DWG→`DwgReader`, DXF→`DxfReader`)    |
|  [02]   | `CadReaderFactory.GetFileFormat(string filename)`                                  | format probe     | `CadFileFormat.{DWG,DXF,Unknown}` from the extension               |
|  [03]   | `DxfReader.IsBinary(Stream stream, bool resetPos = false)`                          | binary sniff     | static ascii-vs-binary DXF detection (also `IsBinary(string)`)     |
|  [04]   | `DxfReader.Read(Stream stream, NotificationEventHandler = null)`                    | static read      | read a DXF stream (binary auto-detected) into a `CadDocument`      |
|  [05]   | `DxfReader.Read(string filename, DxfReaderConfiguration, NotificationEventHandler = null)` | static read | read a DXF file under a tuned config                              |
|  [06]   | `DwgReader.Read(Stream stream, DwgReaderConfiguration, NotificationEventHandler = null)` | static read  | read a DWG stream under a tuned config (stream+config overload)    |
|  [07]   | `DwgReader.Read(string filename, NotificationEventHandler = null)`                  | static read      | read a DWG file by path into a `CadDocument`                       |
|  [08]   | `DwgReader.ReadSummaryInfo()` / `ReadPreview()` / `ReadHeader()`                    | partial read     | summary/preview/header without a full entity parse (instance)      |

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → mesh-bearing entities
- rail: geometry

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]              | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------- | :-------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities` (= `ModelSpace.Entities`) | `CadObjectCollection<Entity>` | the top-level model-space entity set                                  |
|  [02]   | `CadDocument.BlockRecords` / `BlockRecord.Entities` | block table              | the nested-block geometry an `Insert.Block` references                  |
|  [03]   | `Mesh.Vertices` / `Mesh.Faces`                  | `List<XYZ>` / `List<int[]>` | the SubDMesh vertex list and 0-based n-gon face index list (fan-triangulated) |
|  [04]   | `Face3D.FirstCorner`..`FourthCorner`            | `XYZ` (WCS)                 | the 3DFACE corners (fourth == third signals a triangle)                 |
|  [05]   | `PolyfaceMesh.Vertices` / `PolyfaceMesh.Faces`  | `VertexFaceMesh` / `VertexFaceRecord` | the polyface vertex pool + 1-based signed-index face records  |

[ENTRYPOINT_SCOPE]: block-reference flattening — `Insert` placement, the canonical no-hand-roll path
- rail: geometry

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `Insert.Explode()`                              | `IEnumerable<Entity>` | the resolved block expanded into placed entities already in WCS   |
|  [02]   | `Insert.GetTransform()`                         | `Transform`     | the full placement transform (`Matrix4`) the soup applies via `Entity.ApplyTransform` |
|  [03]   | `Insert.Block` / `Insert.InsertPoint`           | `BlockRecord` / `XYZ` | the referenced block record and WCS origin                          |
|  [04]   | `Insert.XScale`/`YScale`/`ZScale` / `Rotation` / `Normal` | placement | per-axis scale, rotation, and OCS normal `GetTransform` composes         |
|  [05]   | `Insert.ColumnCount`/`RowCount`/`ColumnSpacing`/`RowSpacing` / `IsMultiple` | MInsert grid | the array (`MINSERT`) replication; `IsMultiple` discriminates a grid placement |

## [04]-[IMPLEMENTATION_LAW]

[BIM_MESH_ADMISSION]:
- The Bim arm reads ONLY the mesh-bearing entities into the canonical `ImportedGeometry` triangle-soup: `Mesh` (the `Vertices` `List<XYZ>` plus the `Faces` `List<int[]>` 0-based n-gon index list, each face fan-triangulated `(face[0], face[k], face[k+1])`), `Face3D` (the `FirstCorner`..`FourthCorner` `XYZ` quad, fan-triangulated, the fourth corner equal to the third signaling a triangle), `PolyfaceMesh` (the `Vertices` `VertexFaceMesh` pool indexed by the `Faces` `VertexFaceRecord` 1-based signed `Index1`..`Index4`, a negative index marking a hidden edge and a zero index marking an unused slot, so a quad with `Index4==0` is a triangle), `PolygonMesh` (the M×N surface vertex grid), and the `Insert`-referenced mesh set.
- An `Insert` is flattened through `Insert.Explode()` returning the block's entities already placed in WCS, OR by baking the placement with `entity.ApplyTransform(insert.GetTransform())` against the resolved `Insert.Block.Entities` — never a hand-rolled `InsertPoint`/`XScale`/`YScale`/`ZScale`/`Rotation`/`Normal` matrix; `CSMath.Transform` (`Insert.GetTransform`) owns the OCS→WCS composition and `Entity.ApplyTransform` owns the per-entity bake. A `MINSERT` array (`IsMultiple`, `ColumnCount`/`RowCount` > 1) replicates the block across the `ColumnSpacing`/`RowSpacing` grid; `Explode` yields every instance.
- A `Line`/`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline` 2D profile entity is the `Rasm.Fabrication` `Polygon/import#PROFILE_IMPORT` concern (sampled into the `Loop` vocabulary), not a triangle-soup contributor — the Bim arm skips it; admitting one drawing into two folders is correct, each projecting its owned entity families from one `CadDocument`.

[FORMAT_DISPATCH]:
- Format selection is `CadReaderFactory.GetFileFormat`/`CreateReader` (extension→`CadFileFormat`→`ICadReader`) for the filename path, and the format-specific `DxfReader.Read`/`DwgReader.Read` for the stream path routed by the `Dwg`/`Dxf` `InterchangeFormat` extension set. The DXF ascii-vs-binary split is `DxfReader.IsBinary(stream)` (and the reader auto-detects it internally), and the DWG version is read from the file header — the Bim arm does NOT hand-sniff the `AC10xx` / `0\nSECTION` / `AutoCAD Binary DXF` byte sentinels the package already owns.
- The DWG read floor is `AC1012` (R13): `DwgReader` throws `CadNotSupportedException` for `AC1009` (R11/R12) and earlier, and supports `AC1012`/`AC1014`/`AC1015`/`AC1018`/`AC1021`/`AC1024`/`AC1027`/`AC1032`.

[BOUNDARY_AND_NOTIFICATION]:
- The reader is configured at the boundary: `CadReaderConfiguration.Failsafe` (default true) keeps recoverable section/entity errors as `OnNotification` events rather than throws; `KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` (default false) drop proxy/unknown objects the soup never reads; `DwgReaderConfiguration.ReadSummaryInfo` is set false when only geometry is needed; `DxfReaderConfiguration.ClearCache`/`CreateDefaults` tune DXF section reuse. The `BimIo.Boundary` subscribes `ICadReader.OnNotification` and folds the `NotificationType` (`Warning`/`Error`/`NotSupported`) stream into its degradation log without aborting the read.
- The `DxfReader.Read`/`DwgReader.Read` facade (and `CadReaderFactory.CreateReader().Read()`) THROWS on a malformed/unreadable file (`CadNotSupportedException`, DXF/DWG parse exceptions) — the Bim arm wraps the call in the `BimIo.Boundary` funnel and lowers the caught exception to `BimFault.ModelRejected` ONCE at admission (the reader exception never escapes the boundary); under `Failsafe` a non-fatal defect arrives as an `OnNotification` event, not a throw.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DWG/DXF format dispatch and stream/file read into the `CadDocument` model, the reader configuration/notification rail, and the mesh-bearing entity surface the `Exchange/import#IMPORT_RAIL` `BimIo.AcadGeometry` arm folds into `ImportedGeometry` triangle-soup
- Accept: a DWG/DXF stream or filename routed through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config; the `Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh`/`Insert`-referenced entities folded onto the canonical triangle-soup; the `Insert` placement flattened through `Explode`/`GetTransform`/`Entity.ApplyTransform`; `NotificationType` events folded into the boundary log
- Reject: writing DWG/DXF from the Bim folder (`DxfWriter`/`DwgWriter`/`SvgWriter` are out of scope — the codec is read-only ingress; Rhino owns the host-bound native write), a `CadDocument` or an ACadSharp entity type escaping the `BimIo` boundary into a domain signature, admitting a second managed CAD library (`netDxf`) for the same row where `ACadSharp` supersedes it, a reader exception escaping the boundary unlowered, hand-rolling format/binary detection or an `Insert` placement matrix the package already owns, and re-reading the 2D profile entities the `Rasm.Fabrication` boundary owns
