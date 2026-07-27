# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` reads DXF (ASCII and binary) and DWG (AC1014 through AC1032) into a `CadDocument`, and fabrication consumes only that read surface to admit 2D profiles as `Loop` values and annotation as markings. Every bulge, conic, spline span, and nested block tessellates through the package sampler and `Insert.Explode`, never hand-rolled trigonometry or NURBS. Resilient read routes recoverable corruption to the notification stream and completes; a hard reader throw lowers to `GeometryFault.DegenerateInput`. `Rasm.AppUi` owns the drafting write leg; this boundary is read-only.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp` (MIT)
- assembly: `ACadSharp`
- namespaces: `ACadSharp.IO` (readers, config, notification), `ACadSharp` (`CadDocument`), `ACadSharp.Tables` (`BlockRecord`), `ACadSharp.Entities` (profile entities + `IPolyline`/`ICurve`/`IVertex`), `CSMath` (`XY`/`XYZ`/`Transform`/`Matrix3`)
- asset: pure-managed AnyCPU IL, ALC-safe, coexists with the Rhino-native host-bound file I/O
- rail: fabrication read-side CAD profile ingress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader contract, document root, notification rail (`ACadSharp.IO` / `ACadSharp`)

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------- | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ACadSharp.IO.ICadReader`               | reader contract | `IDisposable` — `Read()`/`ReadHeader()` + `OnNotification`/`OnProgress`      |
|  [02]   | `ACadSharp.IO.DxfReader`                | static reader   | `CadReaderBase<DxfReaderConfiguration>` — DXF (ASCII + binary) read          |
|  [03]   | `ACadSharp.IO.DwgReader`                | static reader   | `CadReaderBase<DwgReaderConfiguration>` — DWG (AC1014–AC1032) read           |
|  [04]   | `ACadSharp.IO.NotificationEventHandler` | delegate        | `void(object sender, NotificationEventArgs e)` — the structured warning sink |
|  [05]   | `ACadSharp.IO.NotificationEventArgs`    | event payload   | `Message`/`NotificationType`/`Exception` — the recoverable-error record      |
|  [06]   | `ACadSharp.IO.NotificationType`         | enum            | `NotImplemented=-1`, `None`, `Warning`, `Error` — the severity discriminator |
|  [07]   | `ACadSharp.CadDocument`                 | model root      | the read drawing — `Entities`/`ModelSpace`/`BlockRecords`/`Header`           |
|  [08]   | `ACadSharp.Header.CadHeader`            | header record   | `InsUnits : UnitsType` — the `$INSUNITS` insertion-unit slot                 |
|  [09]   | `ACadSharp.Types.Units.UnitsType`       | enum            | the insertion-unit vocabulary of `InsUnits`                                  |

[PUBLIC_TYPE_SCOPE]: entity collections on the document

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities`     | entity sequence | `CadObjectCollection<Entity>` — flat model-space access (`=> ModelSpace.Entities`)  |
|  [02]   | `CadDocument.ModelSpace`   | block record    | `BlockRecord` (`=> BlockRecords["*Model_Space"]`) carrying `Entities`               |
|  [03]   | `CadDocument.BlockRecords` | block table     | `BlockRecordsTable` — the block-record table for nested blocks                      |
|  [04]   | `BlockRecord.Entities`     | entity sequence | `CadObjectCollection<Entity>` — a named block's geometry (resolved per `Insert`)    |
|  [05]   | `CadObjectCollection<T>`   | collection      | `IEnumerable<T>` + `Count`/`this[int]` (NOT `List<T>`) — enumerate via `toSeq`      |
|  [06]   | `CadDocument.Layers`       | layer table     | `LayersTable` — complete declared-layer census, including layers with zero entities |

[PUBLIC_TYPE_SCOPE]: 2D profile entity types (`ACadSharp.Entities`); `IPolyline`/`ICurve` discriminate a tessellation arm over multiple concrete leaves

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]       | [CAPABILITY]                                                                         |
| :-----: | :------------------ | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `IPolyline`         | shape discriminator | `IEnumerable<IVertex> Vertices`/`IsClosed` — the `LwPolyline`+`Polyline2D` union arm |
|  [02]   | `ICurve`            | shape discriminator | the `Circle`/`Arc` curve contract carrying `PolygonalVertexes`                       |
|  [03]   | `LwPolyline`        | lightweight poly    | `LwPolyline : Entity, IPolyline` — closed/open bulge polyline, primary 2D profile    |
|  [04]   | `LwPolyline.Vertex` | per-vertex          | `Location: XY`/`Bulge: double`/`StartWidth`/`EndWidth` of the lightweight poly       |
|  [05]   | `Polyline2D`        | 2D polyline         | `Polyline<Vertex2D> : IPolyline` — `Seqend`-collection bulge polyline (non-`List`)   |
|  [06]   | `Vertex2D`          | per-vertex          | `Vertex2D : Vertex` — `Location: XYZ`/`Bulge: double` (NO `Pt` overload)             |
|  [07]   | `Line`              | line segment        | straight `StartPoint`→`EndPoint` segment                                             |
|  [08]   | `Arc`               | circular arc        | `Arc : Circle` partial-sweep arc                                                     |
|  [09]   | `Circle`            | full circle         | `Circle : Entity, ICurve` — `Center`/`Radius`/`Normal`/`Thickness` closed circle     |
|  [10]   | `Ellipse`           | conic curve         | partial or full ellipse with native polygonal sampling                               |
|  [11]   | `Spline`            | NURBS spline        | `Spline : Entity` — native-tessellated control-point/knot spline                     |
|  [12]   | `Insert`            | block reference     | placed/arrayed nested-block reference with package-owned explode + transform         |
|  [13]   | `Entity` base       | provenance          | `Layer : Tables.Layer` + `Color : Color` — `.Name`/`.Index` (ACI)                    |
|  [14]   | `CadObject` base    | identity            | `Handle : ulong` (`0` when unowned) + `Document`/`Owner`/`ExtendedData`              |
|  [15]   | `Point`             | marking entity      | `Location : XYZ` (WCS) + `Rotation`/`Normal`/`Thickness` — pierce and drill marks    |
|  [16]   | `Hatch`             | filled region       | nested `BoundaryPath` rows whose `Edge` leaves discriminate on `EdgeType`            |
|  [17]   | `IText`             | shape discriminator | `Height`/`Value`/`Style`/`InsertPoint`/`AlignmentPoint` settable, `Rotation` get-only |

[PUBLIC_TYPE_SCOPE]: annotation entity types (`ACadSharp.Entities`); `IText` discriminates one arm over `TextEntity` and `MText`, and `AttributeEntity` reaches it through `TextEntity`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                                      |
| :-----: | :-------------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `TextEntity`                | single-line     | `TextEntity : Entity, IText` — the TEXT entity carrying one `Value` string         |
|  [02]   | `MText`                     | multiline       | `MText : Entity, IText` — the MTEXT entity carrying a formatted, wrapped body      |
|  [03]   | `MText.TextColumnData`      | column layout   | nested `ColumnType`/`ColumnCount`/`Width`/`Gutter`/`Heights` column record         |
|  [04]   | `AttributeBase`             | placed base     | `AttributeBase : TextEntity` — `Tag`/`Flags`/`IsLocked`/`MText`/`AttributeType`    |
|  [05]   | `AttributeEntity`           | block attribute | `AttributeEntity : AttributeBase` — the ATTRIB placed value hanging off an `Insert` |
|  [06]   | `AttributeDefinition`       | block template  | the block-record attribute template `Insert.UpdateAttributes` matches by tag       |
|  [07]   | `Tables.TextStyle`          | table entry     | `TextStyle : TableEntry` — carries the style `Name` a marking records              |
|  [08]   | `AttributeFlags`            | flags enum      | `[Flags]` `None=0`/`Hidden=1`/`Constant=2`/`Verify=4`/`Preset=8`                   |
|  [09]   | `AttributeType`             | enum            | `SingleLine=1`/`MultiLine=2`/`ConstantMultiLine=4` — body-location discriminator   |
|  [10]   | `TextHorizontalAlignment`   | enum `short`    | `Left`/`Center`/`Right`/`Aligned`/`Middle`/`Fit` — two rows stretch a run          |
|  [11]   | `TextVerticalAlignmentType` | enum `short`    | `Baseline`/`Bottom`/`Middle`/`Top` — baseline stays distinct from bottom           |
|  [12]   | `AttachmentPointType`       | enum `short`    | `TopLeft=1` through `BottomRight=9` — the MTEXT 3x3 attachment grid                |
|  [13]   | `TextMirrorFlag`            | flags enum      | `Backward`/`Upsidedown` mirror state `ApplyTransform` reads and clears             |
|  [14]   | `SeqendCollection<T>`       | collection      | `IEnumerable<T>` seqend-terminated collection carrying `Insert.Attributes`         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read — `DxfReader` / `DwgReader` facades (`ACadSharp.IO`)

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]    |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------- |
|  [01]   | `DxfReader.Read(string, NotificationEventHandler?) -> CadDocument` | static   | read DXF path   |
|  [02]   | `DxfReader.Read(Stream, NotificationEventHandler?) -> CadDocument` | static   | read DXF stream |
|  [03]   | `DxfReader.Read(string, DxfReaderConfiguration, …) -> CadDocument` | static   | configured DXF  |
|  [04]   | `new DxfReader(string, NotificationEventHandler?)` + `.Read()`     | ctor     | disposable read |
|  [05]   | `DwgReader.Read(string, NotificationEventHandler?) -> CadDocument` | static   | read DWG path   |
|  [06]   | `DwgReader.Read(string, DwgReaderConfiguration, …) -> CadDocument` | static   | configured DWG  |
|  [07]   | `DxfReader.IsBinary(string)` / `(Stream, bool) -> bool`            | static   | classify DXF    |
|  [08]   | `ICadReader.ReadHeader() -> CadHeader`                             | instance | header only     |
|  [09]   | `DxfReader.ReadTables() -> CadDocument`                            | instance | tables only     |
|  [10]   | `DxfReader.ReadEntities() -> List<Entity>`                         | instance | entities only   |
|  [11]   | `DwgReader.ReadPreview() -> DwgPreview`                            | instance | preview image   |
|  [12]   | `DwgReader.ReadSummaryInfo() -> CadSummaryInfo`                    | instance | summary info    |

- instance read subscribes `OnNotification` and `OnProgress` before `.Read()`; a static read takes the optional `NotificationEventHandler` inline.

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → top-level model-space entities

`CadDocument.Entities` aliases `ModelSpace.Entities`, `ModelSpace` aliases `BlockRecords["*Model_Space"]`, and a `BlockRecord` stays nested until an `Insert` resolves it.

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]         |
| :-----: | :---------------------------------------------------- | :------- | :------------------- |
|  [01]   | `CadDocument.Entities -> CadObjectCollection<Entity>` | property | top-level entity set |
|  [02]   | `CadDocument.ModelSpace -> BlockRecord`               | property | model-space block    |
|  [03]   | `BlockRecord.Entities -> CadObjectCollection<Entity>` | property | nested-block entity  |

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

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `IText.Value` / `.Height` -> `string`/`double`               | property | content and glyph height      |
|  [02]   | `IText.InsertPoint` / `.AlignmentPoint` -> `XYZ`             | property | the two placed points         |
|  [03]   | `IText.Rotation -> double`                                   | property | get-only on the interface     |
|  [04]   | `IText.Style -> Tables.TextStyle`                            | property | style entry, `.Name` its key  |
|  [05]   | `TextEntity.HorizontalAlignment -> TextHorizontalAlignment`  | property | justification and run stretch |
|  [06]   | `TextEntity.VerticalAlignment -> TextVerticalAlignmentType`  | property | vertical datum, virtual       |
|  [07]   | `TextEntity.ObliqueAngle` / `.WidthFactor` -> `double`       | property | slant and width scaling       |
|  [08]   | `TextEntity.Mirror -> TextMirrorFlag`                        | property | backward/upside-down state    |
|  [09]   | `MText.PlainText -> string`                                  | property | format codes stripped         |
|  [10]   | `MText.GetPlainTextLines() -> string[]`                      | instance | stripped body split to lines  |
|  [11]   | `MText.GetTextLines() -> string[]`                           | instance | raw body split to lines       |
|  [12]   | `MText.AttachmentPoint -> AttachmentPointType`               | property | 3x3 attachment grid cell      |
|  [13]   | `MText.RectangleWidth` / `.RectangleHeight` -> `double`      | property | wrap column and box height    |
|  [14]   | `MText.LineSpacing` / `.LineSpacingStyle`                    | property | spacing factor and style      |
|  [15]   | `MText.HasColumns` / `.ColumnData -> TextColumnData`         | property | column layout discriminator   |
|  [16]   | `AttributeBase.Tag -> string`                                | property | the attribute lookup key      |
|  [17]   | `AttributeBase.AttributeType -> AttributeType`               | property | where the body lives          |
|  [18]   | `AttributeBase.MText -> MText`                               | property | multiline body, NULLABLE      |
|  [19]   | `AttributeBase.Flags -> AttributeFlags`                      | property | hidden/constant/verify/preset |
|  [20]   | `AttributeBase.IsLocked -> bool`                             | property | position-lock state           |
|  [21]   | `Insert.HasAttributes -> bool`                               | property | `Attributes.Any()` shorthand  |
|  [22]   | `Insert.Attributes -> SeqendCollection<AttributeEntity>`     | property | the PLACED attribute values   |
|  [23]   | `Insert.UpdateAttributes() -> void`                          | instance | re-sync against definitions   |
|  [24]   | `TextEntity.ApplyTransform(Transform)` / `MText` peer        | instance | transform one annotation      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Coordinate frames: `LwPolyline.Vertex.Location` (OCS at `LwPolyline.Elevation`), `Circle.Center`, `Arc.Center`, `Hatch.BoundaryPath.Arc.Center`, and the `Circle`/`Arc` `PolygonalVertexes` output are OCS values; `Ellipse.Center`, `Point.Location`, `Insert.InsertPoint`, `Arc.GetEndVertices`, and the `Ellipse`/`Spline` `PolygonalVertexes` output publish WCS.
- `Matrix3.ArbitraryAxis(entity.Normal) * ocsPoint` is the package-owned OCS-to-WCS map; the package composes `ArbitraryAxis(...).Transpose()` for the inverse in `Hatch.BoundaryPath.Arc(Circle)`, and a mirrored extrusion (`Normal.Z < 0`) inverts in-plane arc sense, so a bulge carried across the frame multiplies by `Math.Sign(normal.Z)`.
- Resilient read: `CadReaderConfiguration.Failsafe` defaults `true`, so recoverable corruption routes to `NotificationEventArgs(NotificationType, Message, Exception)` and the read completes; a hard reader throw lowers once to `GeometryFault.DegenerateInput` and the reader exception never escapes.
- Block recursion binds on an ancestor set keyed on `BlockRecord.Handle`, so a self-referencing block terminates.
- `Insert.Explode()` enumerates `Block.Entities` once under one `GetTransform()` affine and does NOT replicate the `RowCount`/`ColumnCount` MINSERT array; a consumer needing every occurrence expands the grid itself, offsetting each replica by `Matrix3.ArbitraryAxis(Normal) * Matrix3.RotationZ(Rotation)` on the spacing vector.
- `Insert.Explode()` rewrites a block-nested `Circle` into an `Ellipse` carrying `RadiusRatio = 1.0` and `MajorAxisEndPoint = XYZ.AxisX * Radius`; `Arc` survives as `Arc`, so a consumer preserving exact circles re-reads a unit-ratio full ellipse rather than sampling it.
- `Insert.Explode()` enumerates `Block.Entities` ALONE and never yields `Insert.Attributes`; `Insert.ApplyTransform` transforms those attributes on a separate trailing loop, so the package treats a placed attribute as an independently transformed entity. `Explode()` alone therefore drops every ATTRIB — the part marks, heat numbers, and shop tags a drawing carries — with no notification and no throw, so a flatten enumerates the attribute collection beside the exploded children or loses that content whole.
- `AttributeEntity : AttributeBase : TextEntity`, so a type-pattern arm matching `TextEntity` first captures every ATTRIB and strands its `Tag`; the derived arm precedes the base arm, and `AttributeBase.MText` is nullable, carrying the body only for the `MultiLine` and `ConstantMultiLine` `AttributeType` rows.
- `MText.Rotation` is DERIVED, not stored — `new XY(AlignmentPoint.X, AlignmentPoint.Y).GetAngle()` — and `IText.Rotation` is get-only, so an MTEXT left on the default `AlignmentPoint = XYZ.AxisX` reads zero rotation and no setter exists to correct it.
- `Spline.PointOnSpline(double t)` normalizes `t` over `[0,1]` across the knot span, throws `ArgumentOutOfRangeException` outside it, and nudges `t == 1.0` down by `double.Epsilon`, so the terminal sample lands epsilon short of the end knot. `TryPointOnSpline` swallows EVERY exception and assigns `XYZ.NaN` on failure, so the `bool` return is the only safe gate — a caller reading the out parameter alone admits NaN coordinates that survive downstream folds.
- `TextEntity.Height` throws `ArgumentOutOfRangeException` on assignment at or below zero while the getter is total, so reading annotation height needs no guard and authoring one does.

[STACKING]:
- `CavalierContours`(`.api/api-cavaliercontours.md`): `LwPolyline.Vertex.Bulge` and `Polyline2D` vertices carry `Bulge = tan(theta/4)` of the arc's included angle, the identical convention `PlineVertex<T>.Bulge` binds, so a bulge profile crosses to arc-native `Polyline<double>` for exact offset and Boolean without a line-densified fan.
- `PolygonAlgebra` / `ArcAlgebra`: sampled entity vertices and exploded block children enter as `Loop` boundary atoms; `ArcAlgebra.Densify` is the sole bulge-to-line bridge, and every `CadDocument` and ACadSharp entity terminates inside the profile-import owner.

[LOCAL_ADMISSION]:
- Admitted profiles: `LwPolyline`, `Polyline2D`, `Line`, `Arc`, `Circle`, `Ellipse`, `Spline`, and `Insert` block references flattened through `Insert.Explode()` beside the placed `Insert.Attributes`.
- Admitted annotation: `Point`, `TextEntity`, `MText`, and `AttributeEntity`, each lowering its content, placement, height, style name, and justification into the profile owner's own marking vocabulary; the provider justification, attachment, stretch, and attribute-flag enums re-close at that lowering arm and none survives into a receipt.
- `LwPolyline.Vertices` is `List<Vertex>` and `LwPolyline.Elevation`/`Normal` carry the OCS Z and extrusion direction the 2D `Location` omits, so reading `Location` alone flattens every polyline onto Z zero; `Polyline2D.Vertices` is a `SeqendCollection<Vertex2D>` whose `Location` is a plain `XYZ`.
- Reader config: `DxfReader.Read(path, DxfReaderConfiguration, notification)` and the `DwgReader` peer carry explicit config; `CadReaderConfiguration` owns `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects`, `DxfReaderConfiguration` adds `ClearCache`/`CreateDefaults`, and `DwgReaderConfiguration` adds `CrcCheck`/`ReadSummaryInfo`.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DXF/DWG file read into the `CadDocument` model, the 2D profile entity surface the profile-import boundary tessellates into `Loop` sets, and the annotation surface that boundary lowers into markings.
- Accept: file path or stream input, `NotificationEventHandler`, `OnNotification`/`OnProgress`, and optional `DxfReaderConfiguration`/`DwgReaderConfiguration`; `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Ellipse`/`Spline` sampled through `CreateFromBulge`/`PolygonalVertexes`/`TryPointOnSpline`; `Insert` flattened through `Insert.Explode()` and the package transform composer beside its own `Attributes`; `Point`/`TextEntity`/`MText`/`AttributeEntity` read through `IText` and the `AttributeBase` tag surface.
- Reject: DXF/DWG write from this folder (the AppUi drafting write leg owns CAD write); `CadDocument` or ACadSharp entity types escaping into sibling kernels; hand-rolled bulge, NURBS, or `Insert` transform where ACadSharp owns `Arc.CreateFromBulge`/`Spline.PolygonalVertexes`/`Insert.Explode()`/`GetTransform()`; a hand-rolled de Boor evaluation where `PointOnSpline` owns parametric sampling; MTEXT format-code parsing where `PlainText` and `GetPlainTextLines()` own the stripped body; `Vertex2D.Pt(XYZ)` or an assumption that `Read` always throws on bad input.
