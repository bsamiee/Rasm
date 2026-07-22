# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` reads DXF (ASCII and binary) and DWG (AC1014 through AC1032) into a `CadDocument`, and fabrication consumes only that read surface to admit 2D profiles into the canonical `Loop` vocabulary. Every bulge, arc, circle, ellipse, spline span, and nested block tessellates through the ACadSharp-owned sampler and `Insert.Explode`, never hand-rolled trigonometry or NURBS. Resilient read routes recoverable corruption to the notification stream and completes; a hard reader throw lowers to `GeometryFault.DegenerateInput`. `Rasm.AppUi` owns the drafting write leg; this boundary is read-only.

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

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `Arc.CreateFromBulge(XY, XY, double) -> Arc`                     | static   | mint bulge arc           |
|  [02]   | `Arc.GetCenter(XY, XY, double) -> XY`                            | static   | center without radius    |
|  [03]   | `Arc.GetCenter(XY, XY, double, out double) -> XY`                | static   | center and radius        |
|  [04]   | `Arc.GetEndVertices(out XYZ, out XYZ)`                           | instance | WCS endpoints            |
|  [05]   | `Arc.PolygonalVertexes(int) -> List<XYZ>`                        | instance | sample arc               |
|  [06]   | `Circle.PolygonalVertexes(int) -> List<XYZ>`                     | instance | sample circle            |
|  [07]   | `Circle.GetBoundingBox() -> BoundingBox`                         | instance | circle extent            |
|  [08]   | `Arc.GetBoundingBox() -> BoundingBox`                            | instance | arc extent               |
|  [09]   | `Ellipse.PolygonalVertexes(int) -> List<XYZ>`                    | instance | sample ellipse           |
|  [10]   | `Ellipse.IsFullEllipse -> bool`                                  | property | closure discriminator    |
|  [11]   | `Spline.PolygonalVertexes(int) -> List<XYZ>`                     | instance | sample native NURBS      |
|  [12]   | `Spline.TryPolygonalVertexes(int, out List<XYZ>) -> bool`        | instance | probe tessellator        |
|  [13]   | `Spline.PointOnSpline(double) -> XYZ`                            | instance | evaluate spline point    |
|  [14]   | `Spline.TryPointOnSpline(double, out XYZ) -> bool`               | instance | probe spline point       |
|  [15]   | `Spline.UpdateFromFitPoints(uint) -> bool`                       | instance | rebuild fit-point spline |
|  [16]   | `Insert.Explode() -> IEnumerable<Entity>`                        | instance | flatten placed block     |
|  [17]   | `Insert.GetTransform() -> Transform`                             | instance | composed affine          |
|  [18]   | `Insert.ApplyTransform(Transform) -> void`                       | instance | apply affine in place    |
|  [19]   | `Matrix3.ArbitraryAxis(XYZ) -> Matrix3`                          | static   | OCS-to-WCS basis         |
|  [20]   | `Matrix3.RotationZ(double) -> Matrix3`                           | static   | in-plane rotation        |
|  [21]   | `Matrix3.operator *(Matrix3, XYZ) -> XYZ`                        | operator | apply basis to a point   |
|  [22]   | `Matrix3.Transpose() -> Matrix3`                                 | instance | invert orthonormal basis |
|  [23]   | `Hatch.Paths -> List<BoundaryPath>`                              | property | enumerate hatch loops    |
|  [24]   | `Hatch.BoundaryPath.Edges -> ObservableCollection<Edge>`         | property | typed edge leaves        |
|  [25]   | `Hatch.BoundaryPath.GetPoints(int) -> IEnumerable<XYZ>`          | instance | sample one boundary      |
|  [26]   | `Hatch.BoundaryPath.Edge.Type -> EdgeType`                       | property | discriminate edges       |
|  [27]   | `Hatch.BoundaryPath.Line.Start` / `.End` -> `XY`                 | property | OCS line endpoints       |
|  [28]   | `Hatch.BoundaryPath.Arc.Center` / `.Radius` -> `XY`/`double`     | property | OCS circle               |
|  [29]   | `Hatch.BoundaryPath.Arc.StartAngle` / `.EndAngle` -> `double`    | property | angular interval         |
|  [30]   | `Hatch.BoundaryPath.Arc.CounterClockWise -> bool`                | property | sweep sense              |
|  [31]   | `Hatch.BoundaryPath.Ellipse.PolygonalVertexes(int) -> List<XYZ>` | instance | conic sampling           |
|  [32]   | `Hatch.BoundaryPath.Polyline.Vertices` / `.Bulges` / `.IsClosed` | property | bulge path               |
|  [33]   | `Hatch.BoundaryPath.Spline.PolygonalVertexes(int) -> List<XYZ>`  | instance | spline sampling          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Coordinate frames: `LwPolyline.Vertex.Location` (OCS at `LwPolyline.Elevation`), `Circle.Center`, `Arc.Center`, `Hatch.BoundaryPath.Arc.Center`, and the `Circle`/`Arc` `PolygonalVertexes` output are OCS values; `Ellipse.Center`, `Point.Location`, `Insert.InsertPoint`, `Arc.GetEndVertices`, and the `Ellipse`/`Spline` `PolygonalVertexes` output publish WCS.
- `Matrix3.ArbitraryAxis(entity.Normal) * ocsPoint` is the package-owned OCS-to-WCS map; the package composes `ArbitraryAxis(...).Transpose()` for the inverse in `Hatch.BoundaryPath.Arc(Circle)`, and a mirrored extrusion (`Normal.Z < 0`) inverts in-plane arc sense, so a bulge carried across the frame multiplies by `Math.Sign(normal.Z)`.
- Resilient read: `CadReaderConfiguration.Failsafe` defaults `true`, so recoverable corruption routes to `NotificationEventArgs(NotificationType, Message, Exception)` and the read completes; a hard reader throw lowers once to `GeometryFault.DegenerateInput` and the reader exception never escapes.
- Block recursion binds on an ancestor set keyed on `BlockRecord.Handle`, so a self-referencing block terminates.
- `Insert.Explode()` enumerates `Block.Entities` once under one `GetTransform()` affine and does NOT replicate the `RowCount`/`ColumnCount` MINSERT array; a consumer needing every occurrence expands the grid itself, offsetting each replica by `Matrix3.ArbitraryAxis(Normal) * Matrix3.RotationZ(Rotation)` on the spacing vector.
- `Insert.Explode()` rewrites a block-nested `Circle` into an `Ellipse` carrying `RadiusRatio = 1.0` and `MajorAxisEndPoint = XYZ.AxisX * Radius`; `Arc` survives as `Arc`, so a consumer preserving exact circles re-reads a unit-ratio full ellipse rather than sampling it.

[STACKING]:
- `CavalierContours`(`.api/api-cavaliercontours.md`): `LwPolyline.Vertex.Bulge` and `Polyline2D` vertices carry `Bulge = tan(theta/4)` of the arc's included angle, the identical convention `PlineVertex<T>.Bulge` binds, so a bulge profile crosses to arc-native `Polyline<double>` for exact offset and Boolean without a line-densified fan.
- `PolygonAlgebra` / `ArcAlgebra`: sampled entity vertices and exploded block children enter as `Loop` boundary atoms; `ArcAlgebra.Densify` is the sole bulge-to-line bridge, and every `CadDocument` and ACadSharp entity terminates inside the profile-import owner.

[LOCAL_ADMISSION]:
- Admitted profiles: `LwPolyline`, `Polyline2D`, `Line`, `Arc`, `Circle`, `Ellipse`, `Spline`, and `Insert` block references flattened through `Insert.Explode()`.
- `LwPolyline.Vertices` is `List<Vertex>` and `LwPolyline.Elevation`/`Normal` carry the OCS Z and extrusion direction the 2D `Location` omits, so reading `Location` alone flattens every polyline onto Z zero; `Polyline2D.Vertices` is a `SeqendCollection<Vertex2D>` whose `Location` is a plain `XYZ`.
- Reader config: `DxfReader.Read(path, DxfReaderConfiguration, notification)` and the `DwgReader` peer carry explicit config; `CadReaderConfiguration` owns `Failsafe`/`KeepUnknownEntities`/`KeepUnknownNonGraphicalObjects`, `DxfReaderConfiguration` adds `ClearCache`/`CreateDefaults`, and `DwgReaderConfiguration` adds `CrcCheck`/`ReadSummaryInfo`.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DXF/DWG file read into the `CadDocument` model and the 2D profile entity surface the profile-import boundary tessellates into `Loop` sets.
- Accept: file path or stream input, `NotificationEventHandler`, `OnNotification`/`OnProgress`, and optional `DxfReaderConfiguration`/`DwgReaderConfiguration`; `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Ellipse`/`Spline` sampled through `CreateFromBulge`/`PolygonalVertexes`; `Insert` flattened through `Insert.Explode()` and the package transform composer.
- Reject: DXF/DWG write from this folder (the AppUi drafting write leg owns CAD write); `CadDocument` or ACadSharp entity types escaping into sibling kernels; hand-rolled bulge, NURBS, or `Insert` transform where ACadSharp owns `Arc.CreateFromBulge`/`Spline.PolygonalVertexes`/`Insert.Explode()`/`GetTransform()`; `Vertex2D.Pt(XYZ)` or an assumption that `Read` always throws on bad input.
