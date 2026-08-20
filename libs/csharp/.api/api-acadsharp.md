# [RASM_API_ACADSHARP]

`ACadSharp` owns the managed AutoCAD drawing wire — DWG (AC1012/R13 through AC1032) and DXF (ASCII and binary) — over one `CadDocument` model root, read and write. Three folders partition one document model: `Rasm.Bim` folds the mesh-bearing entity families — flattened to WCS through `Insert` explosion — into the canonical `ImportedGeometry` triangle-soup, `Rasm.Fabrication` projects the 2D-profile entities into `Loop` values and the annotation entities into markings, and `Rasm.AppUi` holds CAD WRITE authority alone, folding one authored `CadDocument` to DWG, DXF, and SVG through `DwgWriter`/`DxfWriter`/`SvgWriter`. Every bulge, conic, spline span, and nested block tessellates through the package sampler and `Insert.Explode`, never hand-rolled trigonometry or NURBS.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp` (MIT)
- assembly: `ACadSharp` (`lib/net10.0/ACadSharp.dll`)
- namespace: `ACadSharp`, `ACadSharp.Entities`, `ACadSharp.Tables`, `ACadSharp.IO`, `ACadSharp.IO.SVG`, `ACadSharp.Header`, `ACadSharp.Types.Units`, `CSMath`
- asset: pure-managed AnyCPU IL (depends `CSMath`, `CSUtilities`) — no native asset, no RID burden, ALC-safe, coexists with the Rhino-native host-bound file I/O
- rail: geometry — CAD read at the Bim and Fabrication boundaries, drafting write at AppUi

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facades, configuration, and the notification/progress rail

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `CadReaderFactory`         | class         | extension-routed `CreateReader`/`GetFileFormat` format facade                |
|  [02]   | `CadFileFormat`            | enum          | `DWG`/`DXF`/`Unknown` format discriminant                                    |
|  [03]   | `DxfReader`                | class         | `CadReaderBase<DxfReaderConfiguration>` — DXF ASCII+binary read; `IsBinary`  |
|  [04]   | `DwgReader`                | class         | `CadReaderBase<DwgReaderConfiguration>` — DWG AC1012–AC1032 read             |
|  [05]   | `ICadReader`               | interface     | `OnNotification`/`OnProgress` events, `Read()`/`ReadHeader()`, `IDisposable` |
|  [06]   | `CadReaderConfiguration`   | class         | `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects` knobs      |
|  [07]   | `DwgReaderConfiguration`   | class         | adds `CrcCheck`, `ReadSummaryInfo` (default true)                            |
|  [08]   | `DxfReaderConfiguration`   | class         | adds `ClearCache` (default true), `CreateDefaults`                           |
|  [09]   | `NotificationEventHandler` | delegate      | `NotificationEventArgs` carrier — `Message`, `NotificationType`, `Exception` |
|  [10]   | `NotificationType`         | enum          | `NotImplemented=-1`/`None`/`NotSupported`/`Warning`/`Error` severity         |
|  [11]   | `ProgressEventHandler`     | delegate      | `(object sender, ProgressEventArgs e)` read-progress stream                  |
|  [12]   | `ProgressEventArgs`        | class         | `Stage` (`ReadStage`) + `Current` (`CadObjectData`) per progress event       |
|  [13]   | `ReadStage`                | enum          | TWO members — `Read` then `Build`; no count or total accompanies either      |

[PUBLIC_TYPE_SCOPE]: the `CadDocument` model root, tables, and collections

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `CadDocument`                     | class         | `Entities`, `ModelSpace`/`PaperSpace`, `BlockRecords`, `Header`, `SummaryInfo`      |
|  [02]   | `BlockRecord`                     | class         | nested-block container — `Entities` holds the geometry an `Insert` references       |
|  [03]   | `CadHeader`                       | class         | drawing units/version metadata; `InsUnits : UnitsType` the `$INSUNITS` slot         |
|  [04]   | `ACadSharp.Types.Units.UnitsType` | enum          | the insertion-unit vocabulary of `InsUnits`                                         |
|  [05]   | `CadSummaryInfo`                  | class         | title/author metadata (DWG gated by `DwgReaderConfiguration.ReadSummaryInfo`)       |
|  [06]   | `CadObjectCollection<T>`          | collection    | `IEnumerable<T>` + `Count`/`this[int]` (NOT `List<T>`) — enumerate via `toSeq`      |
|  [07]   | `CadDocument.Layers`              | layer table   | `LayersTable` — complete declared-layer census, including layers with zero entities |
|  [08]   | `CadSystemVariable`               | class         | drawing header variable                                                             |
|  [09]   | `ACadVersion`                     | enum          | drawing format version; the DWG write version-policy row — write-relevant members `AC1021` (R2007), `AC1027` (R2013), `AC1032` (R2018), the roster `CadVersionPolicy` curates |

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
|  [09]   | `ModelerGeometry`  | abstract      | the ACIS-payload BASE — `Solid3D`, `Region`, `CadBody` derive; no managed tessellator exists    |

- [09]-[ACIS_FAMILY]: `ModelerGeometry : Entity` is the ONE base every solid-modelling entity sits under, so a partition that must decline ACIS payloads matches the base and a sibling the package adds later declines with it; matching the leaf names instead misses `CadBody` outright (there is no `Body` or `Surface` type under this name) and misses every future leaf silently.

[PUBLIC_TYPE_SCOPE]: 2D profile entity types (`ACadSharp.Entities`); `IPolyline`/`ICurve` discriminate a tessellation arm over multiple concrete leaves

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [CAPABILITY]                                                                          |
| :-----: | :---------------------- | :------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `IPolyline`             | shape discriminator | `IEnumerable<IVertex> Vertices`/`IsClosed` — the `LwPolyline`+`Polyline2D` union arm  |
|  [02]   | `ICurve`                | shape discriminator | the `Circle`/`Arc` curve contract carrying `PolygonalVertexes`                        |
|  [03]   | `LwPolyline`            | lightweight poly    | `LwPolyline : Entity, IPolyline` — closed/open bulge polyline, primary 2D profile     |
|  [04]   | `LwPolyline.Vertex`     | per-vertex          | `Location: XY`/`Bulge: double`/`StartWidth`/`EndWidth` of the lightweight poly        |
|  [05]   | `Polyline2D`            | 2D polyline         | `Polyline<Vertex2D>` — the base publishes `Normal : XYZ` (= `AxisZ`) + `Elevation`    |
|  [06]   | `Vertex2D`              | per-vertex          | `Vertex : IVertex` — `Location : XYZ` OCS when 2D, WCS when 3D; `Bulge`/`StartWidth`  |
|  [07]   | `Line`                  | line segment        | straight `StartPoint`→`EndPoint` segment                                              |
|  [08]   | `Arc`                   | circular arc        | `Arc : Circle` partial-sweep arc                                                      |
|  [09]   | `Circle`                | full circle         | `Circle : Entity, ICurve` — `Center`/`Radius`/`Normal`/`Thickness` closed circle      |
|  [10]   | `Ellipse`               | conic curve         | partial or full ellipse with native polygonal sampling                                |
|  [11]   | `Spline`                | NURBS spline        | `Spline : Entity` — native-tessellated control-point/knot spline                      |
|  [12]   | `Insert`                | block reference     | placed/arrayed nested-block reference with package-owned explode + transform          |
|  [13]   | `Entity` base           | provenance          | `Layer : Tables.Layer` + `Color : Color` — `.Name`/`.Index` (ACI)                     |
|  [14]   | `CadObject` base        | identity            | `Handle : ulong` (`0` when unowned) + `Document`/`Owner`/`ExtendedData`               |
|  [15]   | `Point`                 | marking entity      | `Location : XYZ` (WCS) + `Rotation`/`Normal`/`Thickness` — pierce and drill marks     |
|  [16]   | `Hatch`                 | filled region       | `Paths : List<BoundaryPath>` + `Elevation`/`Normal`/`IsSolid`/`Pattern`/`SeedPoints`  |
|  [17]   | `Hatch.BoundaryPath`    | loop container      | `Edges : ObservableCollection<Edge>` + `Entities`/`IsPolyline`/`UpdateEdges()`        |
|  [18]   | `BoundaryPath.Edge`     | edge base           | `abstract EdgeType Type` + `Clone()`/`GetBoundingBox()`/`ToEntity()` on every leaf    |
|  [19]   | `BoundaryPath.Line`     | line edge           | `Start`/`End : XY` (OCS) + `ToSegment2D()` — `EdgeType.Line`                          |
|  [20]   | `BoundaryPath.Arc`      | circular edge       | `Center : XY`/`Radius`/`StartAngle`/`EndAngle`/`CounterClockWise` + `ToArc2D()`       |
|  [21]   | `BoundaryPath.Ellipse`  | elliptic edge       | `Center`/`MajorAxisEndPoint : XY`/`RadiusRatio` + derived `MajorAxis`/`Rotation`      |
|  [22]   | `BoundaryPath.Polyline` | polyline edge       | `Vertices : List<XYZ>` (x, y, BULGE in Z) + `IsClosed` — `EdgeType.Polyline`          |
|  [23]   | `BoundaryPath.Spline`   | spline edge         | `ControlPoints`/`Knots`/`FitPoints`/`Degree`/`IsRational`/`IsPeriodic`/tangents       |
|  [24]   | `BoundaryPath.EdgeType` | enum                | `Polyline`/`Line`/`CircularArc`/`EllipticArc`/`Spline` — the leaf discriminator       |
|  [25]   | `IText`                 | shape discriminator | `Height`/`Value`/`Style`/`InsertPoint`/`AlignmentPoint` settable, `Rotation` get-only |

[PUBLIC_TYPE_SCOPE]: annotation entity types (`ACadSharp.Entities`); `IText` discriminates one arm over `TextEntity` and `MText`, and `AttributeEntity` reaches it through `TextEntity`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :-------------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `TextEntity`                | single-line     | `TextEntity : Entity, IText` — the TEXT entity carrying one `Value` string          |
|  [02]   | `MText`                     | multiline       | `MText : Entity, IText` — the MTEXT entity carrying a formatted, wrapped body       |
|  [03]   | `MText.TextColumnData`      | column layout   | nested `ColumnType`/`ColumnCount`/`Width`/`Gutter`/`Heights` column record          |
|  [04]   | `AttributeBase`             | placed base     | `AttributeBase : TextEntity` — `Tag`/`Flags`/`IsLocked`/`MText`/`AttributeType`     |
|  [05]   | `AttributeEntity`           | block attribute | `AttributeEntity : AttributeBase` — the ATTRIB placed value hanging off an `Insert` |
|  [06]   | `AttributeDefinition`       | block template  | the block-record attribute template `Insert.UpdateAttributes` matches by tag        |
|  [07]   | `Tables.TextStyle`          | table entry     | `TextStyle : TableEntry` — carries the style `Name` a marking records               |
|  [08]   | `AttributeFlags`            | flags enum      | `[Flags]` `None=0`/`Hidden=1`/`Constant=2`/`Verify=4`/`Preset=8`                    |
|  [09]   | `AttributeType`             | enum            | `SingleLine=1`/`MultiLine=2`/`ConstantMultiLine=4` — body-location discriminator    |
|  [10]   | `TextHorizontalAlignment`   | enum `short`    | `Left`/`Center`/`Right`/`Aligned`/`Middle`/`Fit` — two rows stretch a run           |
|  [11]   | `TextVerticalAlignmentType` | enum `short`    | `Baseline`/`Bottom`/`Middle`/`Top` — baseline stays distinct from bottom            |
|  [12]   | `AttachmentPointType`       | enum `short`    | `TopLeft=1` through `BottomRight=9` — the MTEXT 3x3 attachment grid                 |
|  [13]   | `TextMirrorFlag`            | flags enum      | `Backward`/`Upsidedown` mirror state `ApplyTransform` reads and clears              |
|  [14]   | `SeqendCollection<T>`       | collection      | `IEnumerable<T>` seqend-terminated collection carrying `Insert.Attributes`          |

[PUBLIC_TYPE_SCOPE]: entity base and `CSMath` value/transform algebra

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Entity`      | class         | graphic style, package-owned TRS bakes, `GetBoundingBox()` — see [01]                              |
|  [02]   | `XYZ`         | struct        | `X`/`Y`/`Z` + indexer, `AxisX/Y/Z`/`Zero`, `+`/`-`/`*`/`/`, `Cross`/`FindNormal`/`GetAngle`        |
|  [03]   | `XY`          | struct        | `X`/`Y` doubles; explicit cast to/from `XYZ`                                                       |
|  [04]   | `Transform`   | class         | `Matrix` (`Matrix4`), `Translation`/`Scale`/`EulerRotation`, `ApplyTransform(XYZ)`, `TryDecompose` |
|  [05]   | `Matrix4`     | struct        | the affine matrix `Transform.Matrix`/`Insert.GetTransform` composes                                |
|  [06]   | `Matrix3`     | struct        | `ArbitraryAxis`/`RotationZ`/`Transpose` OCS basis algebra — see the sampling entrypoints           |
|  [07]   | `BoundingBox` | struct        | `Entity.GetBoundingBox()` extent for soup-bounds accumulation                                      |

- [01]-[ENTITY_BASE]: `Layer`/`Color`/`LineWeight`/`Transparency`/`Material`/`IsInvisible` graphic props; package-owned bakes `ApplyTransform(Transform)`/`ApplyTranslation`/`ApplyRotation(axis,θ)`/`ApplyScaling(scale[,origin])`; `GetBoundingBox()`.
- [01]-[HANDLE]: `Entity : CadObject`, so every entity carries the `ulong Handle` the drawing keys it by beside `Owner`, `Document`, `ObjectType`, and `ExtendedData` — the one stable identity a degrade row names an undecodable entity with.

[PUBLIC_TYPE_SCOPE]: write authoring surface — style vocabulary, tables, and the WRITE-IO family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [CAPABILITY]                        |
| :-----: | :----------------------- | :----------------- | :---------------------------------- |
|  [01]   | `Color`                  | color value        | ACI and true-color                  |
|  [02]   | `Transparency`           | transparency value | alpha channel                       |
|  [03]   | `LineWeightType`         | lineweight enum    | pen weight vocabulary               |
|  [04]   | `ObjectType`             | object type enum   | entity discriminant                 |
|  [05]   | `Dimension`              | annotation entity  | dimension family root               |
|  [06]   | `Viewport`               | viewport entity    | paper-space viewport                |
|  [07]   | `Tables.Layer`           | table entry        | layer definition                    |
|  [08]   | `Tables.LineType`        | table entry        | linetype definition                 |
|  [09]   | `LineType.Segment`       | pattern row        | dash, gap, or dot                   |
|  [10]   | `Tables.DimensionStyle`  | table entry        | dimension style                     |
|  [11]   | `DwgWriter`              | IO writer          | DWG emit entry                      |
|  [12]   | `DxfWriter`              | IO writer          | DXF emit entry, binary or text      |
|  [13]   | `SvgWriter`              | IO writer          | SVG emit entry (`ACadSharp.IO.SVG`) |
|  [14]   | `DxfWriterConfiguration` | writer config      | DXF write options                   |

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

- instance read subscribes `OnNotification` and `OnProgress` before `.Read()`; a static read takes the optional `NotificationEventHandler` inline.

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` to entities

`CadDocument.Entities` aliases `ModelSpace.Entities`, `ModelSpace` aliases `BlockRecords["*Model_Space"]`, and a `BlockRecord` stays nested until an `Insert` resolves it.

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

[ENTRYPOINT_SCOPE]: curve sampling, bulge-to-arc factories, block flatten, and hatch boundaries (ACadSharp-owned tessellation)

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Arc.CreateFromBulge(XY, XY, double) -> Arc`                     | static   | mint bulge arc               |
|  [02]   | `Arc.GetCenter(XY, XY, double) -> XY`                            | static   | center without radius        |
|  [03]   | `Arc.GetCenter(XY, XY, double, out double) -> XY`                | static   | center and radius            |
|  [04]   | `Arc.GetEndVertices(out XYZ, out XYZ)`                           | instance | WCS endpoints                |
|  [05]   | `Arc.Sweep -> double`                                            | property | NEGATED included angle       |
|  [06]   | `Arc.PolygonalVertexes(int) -> List<XYZ>`                        | instance | sample arc                   |
|  [07]   | `Circle.PolygonalVertexes(int) -> List<XYZ>`                     | instance | sample circle                |
|  [08]   | `Circle.GetBoundingBox() -> BoundingBox`                         | instance | circle extent                |
|  [09]   | `Arc.GetBoundingBox() -> BoundingBox`                            | instance | arc extent                   |
|  [10]   | `Ellipse.PolygonalVertexes(int) -> List<XYZ>`                    | instance | sample ellipse               |
|  [11]   | `Ellipse.IsFullEllipse -> bool`                                  | property | closure discriminator        |
|  [12]   | `Ellipse.RadiusRatio` / `.MajorAxisEndPoint` -> `double`/`XYZ`   | property | conic form and major axis    |
|  [13]   | `Spline.PolygonalVertexes(int) -> List<XYZ>`                     | instance | sample native NURBS          |
|  [14]   | `Spline.TryPolygonalVertexes(int, out List<XYZ>) -> bool`        | instance | probe tessellator            |
|  [15]   | `Spline.PointOnSpline(double) -> XYZ`                            | instance | evaluate spline point        |
|  [16]   | `Spline.TryPointOnSpline(double, out XYZ) -> bool`               | instance | probe spline point           |
|  [17]   | `Spline.UpdateFromFitPoints(uint iterationLimit = 255) -> bool`  | instance | rebuild fit-point spline     |
|  [18]   | `Insert.Explode() -> IEnumerable<Entity>`                        | instance | flatten placed block         |
|  [19]   | `Insert.GetTransform() -> Transform`                             | instance | composed affine              |
|  [20]   | `Insert.ApplyTransform(Transform) -> void`                       | instance | apply affine in place        |
|  [21]   | `Insert.Block -> Tables.BlockRecord`                             | property | referenced block record      |
|  [22]   | `Insert.RowCount` / `.ColumnCount` -> `ushort`                   | property | MINSERT array extent         |
|  [23]   | `Insert.RowSpacing` / `.ColumnSpacing` -> `double`               | property | MINSERT array pitch          |
|  [24]   | `Insert.Rotation` / `.Normal` -> `double`/`XYZ`                  | property | placement frame              |
|  [25]   | `Insert.IsMultiple -> bool`                                      | property | MINSERT discriminator        |
|  [26]   | `Matrix3.ArbitraryAxis(XYZ) -> Matrix3`                          | static   | OCS-to-WCS basis             |
|  [27]   | `Matrix3.RotationZ(double) -> Matrix3`                           | static   | in-plane rotation            |
|  [28]   | `Matrix3.operator *(Matrix3, XYZ) -> XYZ`                        | operator | apply basis to a point       |
|  [29]   | `Matrix3.Transpose() -> Matrix3`                                 | instance | invert orthonormal basis     |
|  [30]   | `Hatch.Paths -> List<BoundaryPath>`                              | property | enumerate hatch loops        |
|  [31]   | `Hatch.Elevation` / `.Normal` -> `double`/`XYZ`                  | property | OCS plane of every edge leaf |
|  [32]   | `Hatch.BoundaryPath.Edges -> ObservableCollection<Edge>`         | property | typed edge leaves            |
|  [33]   | `Hatch.BoundaryPath.GetPoints(int) -> IEnumerable<XYZ>`          | instance | sample one boundary          |
|  [34]   | `Hatch.BoundaryPath.Edge.Type -> EdgeType`                       | property | discriminate edges           |
|  [35]   | `Hatch.BoundaryPath.Line.Start` / `.End` -> `XY`                 | property | OCS line endpoints           |
|  [36]   | `Hatch.BoundaryPath.Arc.Center` / `.Radius` -> `XY`/`double`     | property | OCS circle                   |
|  [37]   | `Hatch.BoundaryPath.Arc.StartAngle` / `.EndAngle` -> `double`    | property | angular interval             |
|  [38]   | `Hatch.BoundaryPath.Arc.CounterClockWise -> bool`                | property | sweep sense                  |
|  [39]   | `Hatch.BoundaryPath.Ellipse.PolygonalVertexes(int) -> List<XYZ>` | instance | conic sampling               |
|  [40]   | `Hatch.BoundaryPath.Polyline.Vertices` / `.Bulges` / `.IsClosed` | property | bulge path                   |
|  [41]   | `Hatch.BoundaryPath.Spline.PolygonalVertexes(int) -> List<XYZ>`  | instance | spline sampling              |

[ENTRYPOINT_SCOPE]: annotation content, placement, and typography (`ACadSharp.Entities`)

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `IText.Value` / `.Height` -> `string`/`double`              | property | content and glyph height      |
|  [02]   | `IText.InsertPoint` / `.AlignmentPoint` -> `XYZ`            | property | the two placed points         |
|  [03]   | `IText.Rotation -> double`                                  | property | get-only on the interface     |
|  [04]   | `IText.Style -> Tables.TextStyle`                           | property | style entry, `.Name` its key  |
|  [05]   | `TextEntity.HorizontalAlignment -> TextHorizontalAlignment` | property | justification and run stretch |
|  [06]   | `TextEntity.VerticalAlignment -> TextVerticalAlignmentType` | property | vertical datum, virtual       |
|  [07]   | `TextEntity.ObliqueAngle` / `.WidthFactor` -> `double`      | property | slant and width scaling       |
|  [08]   | `TextEntity.Mirror -> TextMirrorFlag`                       | property | backward/upside-down state    |
|  [09]   | `MText.PlainText -> string`                                 | property | format codes stripped         |
|  [10]   | `MText.GetPlainTextLines() -> string[]`                     | instance | stripped body split to lines  |
|  [11]   | `MText.GetTextLines() -> string[]`                          | instance | raw body split to lines       |
|  [12]   | `MText.AttachmentPoint -> AttachmentPointType`              | property | 3x3 attachment grid cell      |
|  [13]   | `MText.RectangleWidth` / `.RectangleHeight` -> `double`     | property | wrap column and box height    |
|  [14]   | `MText.LineSpacing` / `.LineSpacingStyle`                   | property | spacing factor and style      |
|  [15]   | `MText.HasColumns` / `.ColumnData -> TextColumnData`        | property | column layout discriminator   |
|  [16]   | `AttributeBase.Tag -> string`                               | property | the attribute lookup key      |
|  [17]   | `AttributeBase.AttributeType -> AttributeType`              | property | where the body lives          |
|  [18]   | `AttributeBase.MText -> MText`                              | property | multiline body, NULLABLE      |
|  [19]   | `AttributeBase.Flags -> AttributeFlags`                     | property | hidden/constant/verify/preset |
|  [20]   | `AttributeBase.IsLocked -> bool`                            | property | position-lock state           |
|  [21]   | `Insert.HasAttributes -> bool`                              | property | `Attributes.Any()` shorthand  |
|  [22]   | `Insert.Attributes -> SeqendCollection<AttributeEntity>`    | property | the PLACED attribute values   |
|  [23]   | `Insert.UpdateAttributes() -> void`                         | instance | re-sync against definitions   |
|  [24]   | `TextEntity.ApplyTransform(Transform)` / `MText` peer       | instance | transform one annotation      |

[ENTRYPOINT_SCOPE]: WRITE operations — one `CadDocument` fold, three format writers

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `DwgWriter.Write(string\|Stream, CadDocument)`       | static   | one-call DWG emit                 |
|  [02]   | `DxfWriter.Write(string\|Stream, CadDocument, bool)` | static   | one-call DXF emit, binary or text |
|  [03]   | `new DwgWriter(string\|Stream, CadDocument)`         | ctor     | configured DWG emit               |
|  [04]   | `new DxfWriter(string\|Stream, CadDocument, bool)`   | ctor     | configured DXF emit               |
|  [05]   | `new SvgWriter(string\|Stream, CadDocument)`         | ctor     | configured SVG emit               |
|  [06]   | `LineType.AddSegment(Segment)`                       | instance | append ordered dash pattern       |
|  [07]   | `CadDocument.LineTypes.Continuous`                   | property | the REGISTERED solid entry        |
|  [08]   | `Table<T>[string name]`                              | indexer  | registered entry by name          |

- Static `Write`: a trailing optional `<Format>WriterConfiguration?` overrides `.Configuration`, and a `NotificationEventHandler?` takes the warning/error sink.
- `DxfWriter`: `bool binary` selects binary or ASCII DXF — the ctor fixes it, the static overload takes it per-call; `.IsBinary` reads it back.
- `SvgWriter`: `SvgConfiguration` exposes `LineWeightRatio` and `DefaultLineWeight`, settable before `Write()`.
- `LineType.AddSegment`: signed `Segment.Length` encodes dash (positive), space (negative), dot (zero).
- `LineType.ByLayer`/`ByBlock`/`Continuous` and `Layer.Default`/`Defpoints` are FACTORY properties minting a fresh unregistered entry per read; a document built through `new CadDocument()` already seats those defaults, so a layer binds `doc.LineTypes.Continuous` (the table's own registered accessor) and a per-read static seats duplicate table rows the writers reject.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One read folds format-route → reader → `CadDocument` → entity-family discrimination: the `Dwg`/`Dxf` extension selects `CadReaderFactory.CreateReader` (filename) or `DxfReader.Read`/`DwgReader.Read` (stream) under a tuned `*ReaderConfiguration`, and `CadDocument.Entities` traverses to the families each consuming boundary admits.
- `CadReaderFactory.GetFileFormat`/`DxfReader.IsBinary` own format and ASCII/binary detection, and a mesh- or profile-only ingress reads the DXF ENTITIES section alone via `DxfReader.ReadEntities()`; DWG carries no section-scoped entity read and takes the full `Read()`.
- Resilient read: `CadReaderConfiguration.Failsafe` defaults `true`, so recoverable corruption routes to `NotificationEventArgs(NotificationType, Message, Exception)` and the read completes; a hard reader throw lowers once at the consuming boundary and never escapes.
- Coordinate frames: `LwPolyline.Vertex.Location` (OCS at `LwPolyline.Elevation`), `Vertex2D.Location` (OCS at `Polyline<T>.Elevation`, whose base also publishes `Normal`), `Circle.Center`, `Arc.Center`, `Hatch.BoundaryPath.Arc.Center`, and the `Circle`/`Arc` `PolygonalVertexes` output are OCS values; `Ellipse.Center`, `Point.Location`, `Insert.InsertPoint`, `Arc.GetEndVertices`, and the `Ellipse`/`Spline` `PolygonalVertexes` output publish WCS.
- `Matrix3.ArbitraryAxis(entity.Normal) * ocsPoint` is the package-owned OCS-to-WCS map; the package composes `ArbitraryAxis(...).Transpose()` for the inverse in `Hatch.BoundaryPath.Arc(Circle)`, and a mirrored extrusion (`Normal.Z < 0`) inverts in-plane arc sense, so a bulge carried across the frame multiplies by `Math.Sign(normal.Z)`; `CSMath.Transform` owns the full OCS→WCS placement composition an `Insert` bakes.
- Block recursion binds on an ancestor set keyed on `BlockRecord.Handle`, so a self-referencing block terminates.
- `Insert.Explode()` enumerates `Block.Entities` once under one `GetTransform()` affine and does NOT replicate the `RowCount`/`ColumnCount` MINSERT array; a consumer needing every occurrence expands the grid itself, offsetting each replica by `Matrix3.ArbitraryAxis(Normal) * Matrix3.RotationZ(Rotation)` on the spacing vector.
- `Insert.Explode()` rewrites a block-nested `Circle` into an `Ellipse` carrying `RadiusRatio = 1.0` and `MajorAxisEndPoint = XYZ.AxisX * Radius`; `Arc` survives as `Arc`, so a consumer preserving exact circles re-reads a unit-ratio full ellipse rather than sampling it.
- `Insert.Explode()` enumerates `Block.Entities` ALONE and never yields `Insert.Attributes`; `Insert.ApplyTransform` transforms those attributes on a separate trailing loop, so the package treats a placed attribute as an independently transformed entity. `Explode()` alone therefore drops every ATTRIB — the part marks, heat numbers, and shop tags a drawing carries — with no notification and no throw, so a flatten enumerates the attribute collection beside the exploded children or loses that content whole.
- `AttributeEntity : AttributeBase : TextEntity`, so a type-pattern arm matching `TextEntity` first captures every ATTRIB and strands its `Tag`; the derived arm precedes the base arm, and `AttributeBase.MText` is nullable, carrying the body only for the `MultiLine` and `ConstantMultiLine` `AttributeType` rows.
- `MText.Rotation` is DERIVED, not stored — `new XY(AlignmentPoint.X, AlignmentPoint.Y).GetAngle()` — and `IText.Rotation` is get-only, so an MTEXT left on the default `AlignmentPoint = XYZ.AxisX` reads zero rotation and no setter exists to correct it.
- `Spline.PointOnSpline(double t)` normalizes `t` over `[0,1]` across the knot span, throws `ArgumentOutOfRangeException` outside it, and nudges `t == 1.0` down by `double.Epsilon`, so the terminal sample lands epsilon short of the end knot. `TryPointOnSpline` swallows EVERY exception and assigns `XYZ.NaN` on failure, so the `bool` return is the only safe gate — a caller reading the out parameter alone admits NaN coordinates that survive downstream folds.
- `TextEntity.Height` throws `ArgumentOutOfRangeException` on assignment at or below zero while the getter is total, so reading annotation height needs no guard and authoring one does.
- Every emit folds one `CadDocument` graph through `CadWriterBase<T>`, the three writers differing only by target format; the output DWG/DXF version is a policy row over `ACadVersion`, and the DWG+DXF write leg is two rows on one emit axis over the single document, never a second model.

[STACKING]:
- `CavalierContours`(`Rasm.Fabrication/.api/api-cavaliercontours.md`): `LwPolyline.Vertex.Bulge` and `Polyline2D` vertices carry `Bulge = tan(theta/4)` of the arc's included angle, the identical convention `PlineVertex<T>.Bulge` binds, so a bulge profile crosses to arc-native `Polyline<double>` for exact offset and Boolean without a line-densified fan.
- `OcctNet.Wrapper`(`Rasm.Fabrication/.api/api-occtnet-wrapper.md`): the 2D complement — `ACadSharp` admits 2D profiles (polylines, arcs into `Loop`), OCCT admits 3D solids (STEP/IGES into B-rep into mesh); a 3D STEP solid tessellates then planar-sections to 2D loops, a 2D DXF profile goes straight to `Loop`, neither duplicating the other.
- Bim consumer anchor: `AcadReader.Read` folds the mesh-family entities (`Mesh`/`Face3D`/`PolyfaceMesh`/`PolygonMesh` under `Insert` explosion) off `CadDocument.Entities` into the `ImportedGeometry` triangle-soup, subscribing `ICadReader.OnNotification` into its degradation log and firing the `BimPoint.ExchangeProgress` observe point off the instance readers' `OnProgress` — the in-process codec the `Dwg`/`Dxf` `InterchangeFormat` rows carry against the native-companion two-hop.
- Fabrication consumer anchor: sampled entity vertices and exploded block children enter `PolygonAlgebra`/`ArcAlgebra` as `Loop` boundary atoms — `ArcAlgebra.Densify` is the sole bulge-to-line bridge — and every `CadDocument` and ACadSharp entity terminates inside the profile-import owner.
- AppUi consumer anchor: `Render/drafting.md` composes the DWG+DXF two-format write leg over one `CadDocument` populated from `ACadSharp.Entities` and `ACadSharp.Tables`; the write leg rides the `api-drafting-export.md` catalogue beside `DocumentFormat.OpenXml`.

[LOCAL_ADMISSION]:
- `Rasm.Bim` READ: a DWG/DXF stream or filename enters through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config, then folds onto `ImportedGeometry` in `Exchange/import`; `DxfReader.Read`/`DwgReader.Read` throwing on a malformed file lowers to `BimFault.ModelRejected` once at the `BimIo` boundary.
- `Rasm.Fabrication` READ: admitted profiles are `LwPolyline`, `Polyline2D`, `Line`, `Arc`, `Circle`, `Ellipse`, `Spline`, and `Insert` block references flattened through `Insert.Explode()` beside the placed `Insert.Attributes`; admitted annotation is `Point`, `TextEntity`, `MText`, and `AttributeEntity`, each lowering its content, placement, height, style name, and justification into the profile owner's own marking vocabulary — the provider justification, attachment, stretch, and attribute-flag enums re-close at that lowering arm and none survives into a receipt. A hard reader throw lowers to `GeometryFault.DegenerateInput`. `LwPolyline.Vertices` is `List<Vertex>` and `LwPolyline.Elevation`/`Normal` carry the OCS Z and extrusion direction the 2D `Location` omits, so reading `Location` alone flattens every polyline onto Z zero; `Polyline2D.Vertices` is a `SeqendCollection<Vertex2D>` whose `Location` is a plain `XYZ`.
- `Rasm.AppUi` WRITE: AppUi emits a CAD file, never opens one; entity construction flows through the typed entity constructor then collection `Add`, and the output version is an `ACadVersion` policy row.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DWG/DXF format dispatch and stream/file read into the `CadDocument` model, the reader configuration/notification rail, the mesh-bearing entity surface Bim folds into triangle-soup, the 2D-profile and annotation surface Fabrication tessellates into `Loop` sets and markings, and DWG/DXF/SVG CAD WRITE over one authored `CadDocument` at AppUi
- Accept: a DWG/DXF stream or filename through `CadReaderFactory`/`DxfReader`/`DwgReader` under a tuned config; entity families sampled through `CreateFromBulge`/`PolygonalVertexes`/`TryPointOnSpline` and flattened through `Insert.Explode()`/`GetTransform()`/`ApplyTransform` beside `Insert.Attributes`; `NotificationType` events folded into the boundary log; export flowing through `DwgWriter`/`DxfWriter`/`SvgWriter` WRITE entry points
- Reject: a `CadDocument` or ACadSharp entity type escaping a boundary into a domain signature; a second managed CAD library or document model where `ACadSharp` covers the row; a reader exception escaping a boundary unlowered; hand-rolled format/binary detection, bulge, NURBS, or `Insert` transform algebra the package owns; MTEXT format-code parsing where `PlainText`/`GetPlainTextLines()` own the stripped body; a CAD write outside AppUi or a CAD read inside it; raw entity re-reads across folders — each partition projects only the entity families its domain owns
