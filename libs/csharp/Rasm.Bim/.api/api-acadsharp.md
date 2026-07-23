# [RASM_BIM_API_ACADSHARP]

`ACadSharp` mints the managed read surface for AutoCAD DWG (AC1012/R13 through AC1032) and DXF (ASCII and binary) drawings, folding each file into one `CadDocument` model root. Its mesh-bearing entity families cross the `BimIo` boundary — flattened to WCS through `Insert` explosion — into the canonical `ImportedGeometry` triangle-soup, the in-process codec the `Dwg`/`Dxf` `InterchangeFormat` rows carry against the native-companion two-hop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp` (MIT)
- assembly: `ACadSharp` (`lib/net9.0/ACadSharp.dll`)
- namespace: `ACadSharp`, `ACadSharp.Entities`, `ACadSharp.IO`, `ACadSharp.Tables`, `CSMath`
- asset: pure-managed AnyCPU IL — no native asset, no RID burden, ALC-safe
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facades, configuration, and the notification/progress rail

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `CadReaderFactory`         | class         | extension-routed `CreateReader`/`GetFileFormat` format facade                |
|  [02]   | `CadFileFormat`            | enum          | `DWG`/`DXF`/`Unknown` format discriminant                                    |
|  [03]   | `DxfReader`                | class         | DXF ASCII+binary read; `IsBinary` sniff; `IDisposable`                       |
|  [04]   | `DwgReader`                | class         | DWG AC1012–AC1032 read; `ReadPreview`/`ReadSummaryInfo`/`ReadHeader`         |
|  [05]   | `ICadReader`               | interface     | `OnNotification`/`OnProgress` events, `Read()`/`ReadHeader()`, `IDisposable` |
|  [06]   | `CadReaderConfiguration`   | class         | `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` knobs      |
|  [07]   | `DwgReaderConfiguration`   | class         | adds `CrcCheck`, `ReadSummaryInfo` (default true)                            |
|  [08]   | `DxfReaderConfiguration`   | class         | adds `ClearCache` (default true), `CreateDefaults`                           |
|  [09]   | `NotificationEventHandler` | delegate      | `NotificationEventArgs` carrier — `Message`, `NotificationType`, `Exception` |
|  [10]   | `NotificationType`         | enum          | `NotImplemented`/`None`/`NotSupported`/`Warning`/`Error` severity            |
|  [11]   | `ProgressEventHandler`     | delegate      | `(object sender, ProgressEventArgs e)` read-progress stream                  |
|  [12]   | `ProgressEventArgs`        | class         | `Stage` (`ReadStage`) + `Current` (`CadObjectData`) per progress event       |

[PUBLIC_TYPE_SCOPE]: the `CadDocument` model root and tables

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :--------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `CadDocument`    | class         | `Entities`, `ModelSpace`/`PaperSpace`, `BlockRecords`, `Header`, `SummaryInfo`, `Layers` |
|  [02]   | `BlockRecord`    | class         | named entity container — `Entities` is the nested-block geometry an `Insert` references  |
|  [03]   | `CadHeader`      | class         | drawing units/version metadata behind `CadDocument.Header`                               |
|  [04]   | `CadSummaryInfo` | class         | title/author metadata (DWG gated by `DwgReaderConfiguration.ReadSummaryInfo`)            |

[PUBLIC_TYPE_SCOPE]: mesh-bearing entity types — the Bim triangle-soup surface

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                                    |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Mesh`             | class         | `AcDbSubDMesh` — `Vertices` `List<XYZ>`, `Faces` `List<int[]>`, `Edges`, `SubdivisionLevel`     |
|  [02]   | `Mesh.Edge`        | struct        | `Start`/`End` 0-based vertex index, optional `Crease` (`double?`)                               |
|  [03]   | `Face3D`           | class         | `3DFACE` tri/quad — `FirstCorner`..`FourthCorner` (`XYZ` WCS), `Flags` (`InvisibleEdgeFlags`)   |
|  [04]   | `PolyfaceMesh`     | class         | `Polyline<VertexFaceMesh>` — `Vertices` pool + `Faces` `VertexFaceRecord` records               |
|  [05]   | `VertexFaceMesh`   | class         | a `Vertex` carrying the `Location` `XYZ` of one polyface corner                                 |
|  [06]   | `VertexFaceRecord` | class         | 1-based signed `Index1`..`Index4` (`short`; negative = hidden edge, 0 = unused)                 |
|  [07]   | `PolygonMesh`      | class         | `Polyline<PolygonMeshVertex>` — `MVertexCount`×`NVertexCount` grid, `M`/`NSmoothSurfaceDensity` |
|  [08]   | `Insert`           | class         | placed/arrayed nested-block reference — flattened via `Explode`/`GetTransform`                  |

[PUBLIC_TYPE_SCOPE]: entity base and `CSMath` value/transform algebra

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Entity`      | class         | graphic style, package-owned TRS bakes, `GetBoundingBox()` — see [01]                              |
|  [02]   | `XYZ`         | struct        | `X`/`Y`/`Z` + indexer, `AxisX/Y/Z`/`Zero`, `+`/`-`/`*`/`/`, `Cross`/`FindNormal`/`GetAngle`        |
|  [03]   | `XY`          | struct        | `X`/`Y` doubles; explicit cast to/from `XYZ`                                                       |
|  [04]   | `Transform`   | class         | `Matrix` (`Matrix4`), `Translation`/`Scale`/`EulerRotation`, `ApplyTransform(XYZ)`, `TryDecompose` |
|  [05]   | `Matrix4`     | struct        | the affine matrix `Transform.Matrix`/`Insert.GetTransform` composes                                |
|  [06]   | `BoundingBox` | struct        | `Entity.GetBoundingBox()` extent for soup-bounds accumulation                                      |

- [01]-[ENTITY_BASE]: `Layer`/`Color`/`LineWeight`/`Transparency`/`Material`/`IsInvisible` graphic props; package-owned bakes `ApplyTransform(Transform)`/`ApplyTranslation`/`ApplyRotation(axis,θ)`/`ApplyScaling(scale[,origin])`; `GetBoundingBox()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format dispatch and file/stream read — every read/create overload takes a trailing `NotificationEventHandler = null`

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `CadReaderFactory.CreateReader(string)`                          | factory  | extension-routed `ICadReader` (DWG/DXF)          |
|  [02]   | `CadReaderFactory.GetFileFormat(string)`                         | static   | `CadFileFormat.{DWG,DXF,Unknown}` from extension |
|  [03]   | `DxfReader.IsBinary(Stream, bool)`                               | static   | ASCII-vs-binary DXF sniff (`IsBinary(string)`)   |
|  [04]   | `DxfReader.Read(Stream)`                                         | static   | DXF stream → `CadDocument` (binary auto-detect)  |
|  [05]   | `DxfReader.Read(string, DxfReaderConfiguration)`                 | static   | DXF file read under a tuned config               |
|  [06]   | `DwgReader.Read(Stream)`                                         | static   | DWG stream → `CadDocument` (no-config overload)  |
|  [07]   | `DwgReader.Read(Stream, DwgReaderConfiguration)`                 | static   | DWG stream read under a tuned config             |
|  [08]   | `DwgReader.Read(string)`                                         | static   | DWG file read by path → `CadDocument`            |
|  [09]   | `DwgReader.ReadSummaryInfo()` / `ReadPreview()` / `ReadHeader()` | instance | summary/preview/header without full entity parse |
|  [10]   | `DxfReader.ReadEntities()` / `ReadTables()`                      | instance | section-scoped DXF read → `List<Entity>`         |
|  [11]   | `new DxfReader(Stream)` / `new DwgReader(Stream)`                | ctor     | `ICadReader.Read()` with the `OnProgress` event  |

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` to mesh-bearing entities

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `CadDocument.Entities -> CadObjectCollection<Entity>` | property | top-level entity set (= `ModelSpace.Entities`)    |
|  [02]   | `CadDocument.BlockRecords` / `BlockRecord.Entities`   | property | nested-block geometry set                         |
|  [03]   | `Mesh.Vertices` / `Mesh.Faces`                        | property | `List<XYZ>` verts + `List<int[]>` 0-based n-gon   |
|  [04]   | `Face3D.FirstCorner`..`FourthCorner`                  | property | 3DFACE corners `XYZ` WCS (4th==3rd → tri)         |
|  [05]   | `PolyfaceMesh.Vertices` / `.Faces`                    | property | `VertexFaceMesh` pool + signed-index face records |

[ENTRYPOINT_SCOPE]: block-reference flattening — `Insert` placement, the canonical no-hand-roll path (members on `Insert`)

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Explode()`                                        | instance | resolved block → placed entities in WCS (`IEnumerable`)   |
|  [02]   | `GetTransform()`                                   | instance | placement `Transform` (`Matrix4`) for `ApplyTransform`    |
|  [03]   | `Block` / `InsertPoint`                            | property | `BlockRecord` reference + `XYZ` WCS origin                |
|  [04]   | `XScale`/`YScale`/`ZScale` / `Rotation` / `Normal` | property | per-axis scale, rotation, OCS normal for `GetTransform`   |
|  [05]   | `ColumnCount` / `RowCount`                         | property | `MINSERT` array row/column counts                         |
|  [06]   | `ColumnSpacing` / `RowSpacing` / `IsMultiple`      | property | grid spacing; `IsMultiple` discriminates a grid placement |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One read folds format-route → reader → `CadDocument` → mesh-family discrimination → WCS soup: the `Dwg`/`Dxf` extension selects `CadReaderFactory.CreateReader` (filename) or `DxfReader.Read`/`DwgReader.Read` (stream) under a tuned `*ReaderConfiguration`, and `CadDocument.Entities` traverses to the families the `ImportedGeometry` triangle-soup admits — `Mesh`, `Face3D`, `PolyfaceMesh`, `PolygonMesh`, and `Insert`-referenced meshes.
- Every `Insert` flattens through `Insert.Explode()` or `entity.ApplyTransform(insert.GetTransform())` so placed geometry arrives in WCS, `CSMath.Transform` owning the OCS→WCS composition; a `MINSERT` array (`IsMultiple`, `Column`/`RowCount` > 1) replicates across the `Column`/`RowSpacing` grid that `Explode` unrolls.
- `CadReaderFactory.GetFileFormat`/`DxfReader.IsBinary` own format and ASCII/binary detection, and a mesh-only ingress reads the DXF ENTITIES section alone via `DxfReader.ReadEntities()`; DWG carries no section-scoped entity read and takes the full `Read()`.

[STACKING]:
- (none): `CadDocument`/entity outputs feed the `BimIo` boundary directly, composing with no sibling `.api` at the member level; the format table dispatches DWG/DXF here as a peer codec, never a member hand-off.
- `BimIo.AcadGeometry`: folds the mesh-family entities off `CadDocument.Entities` into the `ImportedGeometry` triangle-soup, subscribing `ICadReader.OnNotification` into its degradation log and firing the `BimHooks` progress observe point off the instance readers' `OnProgress`.
- `Rasm.Fabrication`: reads the SAME `CadDocument` projecting the 2D-profile families (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) into its `Loop` vocabulary — one drawing admitted into two folders, each projecting the entity families its domain owns.

[LOCAL_ADMISSION]:
- A DWG/DXF stream or filename enters through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config, then folds onto `ImportedGeometry` in `Exchange/import`; `CadReaderConfiguration.Failsafe` (default true) keeps recoverable section/entity defects as `OnNotification` events, and `DxfReader.Read`/`DwgReader.Read` throwing on a malformed file lowers to `BimFault.ModelRejected` once at the `BimIo` boundary.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DWG/DXF format dispatch and stream/file read into the `CadDocument` model, the reader configuration/notification rail, and the mesh-bearing entity surface `BimIo.AcadGeometry` folds into `ImportedGeometry` triangle-soup
- Accept: a DWG/DXF stream or filename through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config; the `Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh`/`Insert`-referenced entities folded onto the soup with `Insert` placement flattened through `Explode`/`GetTransform`/`ApplyTransform`; `NotificationType` events folded into the boundary log
- Reject: writing DWG/DXF from Bim (`Rasm.AppUi` owns the managed write leg); a `CadDocument` or ACadSharp entity type escaping the `BimIo` boundary into a domain signature; a second managed CAD library where `ACadSharp` covers the row; a reader exception escaping the boundary unlowered; hand-rolled format/binary detection or an `Insert` placement matrix the package owns; and re-reading the 2D-profile entities `Rasm.Fabrication` owns
