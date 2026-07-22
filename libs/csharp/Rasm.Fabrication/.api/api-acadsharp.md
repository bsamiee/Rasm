# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` is the fabrication read-side CAD owner for AutoCAD DXF (ASCII and binary) and DWG (AC1014 through AC1032); `Rasm.AppUi` owns the two-format ACadSharp drafting write leg. Fabrication consumes only the read surface to admit external 2D part profiles into the canonical `Loop` vocabulary at the profile-import boundary. At the consumer floor (`net10.0` binds the package's highest compatible `lib/net9.0` asset — the `net9.0` dependency group is EMPTY, so the read path drags zero transitive package weight; the netstandard/net48 `System.Memory`/`System.Text.Encoding.CodePages` deps never reach this consumer) the surface splits across four namespaces the boundary `using`s explicitly: the reader facades and the notification/config rail live in `ACadSharp.IO`, the document root and tables in `ACadSharp`/`ACadSharp.Tables`, every profile entity in `ACadSharp.Entities`, and the coordinate/transform primitives in `CSMath`. The reader contract is the `ICadReader : IDisposable` interface (`Read()`/`ReadHeader()` plus `OnNotification`/`OnProgress` events) behind the static `DxfReader.Read`/`DwgReader.Read` facades and the `new DxfReader(filename, notification)`/`.Read()` instance form, all returning a `CadDocument` model root whose `Entities` (`=> ModelSpace.Entities`), `ModelSpace`, and `BlockRecords` carry the drawing geometry. Closed 2D profiles arrive as `LwPolyline` (an `IPolyline` whose `Vertex` carries a `CSMath.XY Location` plus a `double Bulge` — the tangent of one quarter the arc's included angle), `Polyline2D` (`Polyline<Vertex2D> : IPolyline`), `Line`, `Arc`/`Circle` (`Circle : Entity, ICurve`; `Arc : Circle`), `Ellipse`, and `Spline` entities, and nested-block profiles arrive through `Insert` block references; the boundary tessellates each bulge/arc/circle/ellipse/spline span to `Loop` vertices through the ACadSharp-owned `Arc.CreateFromBulge`/`PolygonalVertexes`/`Spline.PolygonalVertexes` curve sampler at the owner's chord-precision knob, never a hand-rolled bulge trigonometry or a hand-rolled NURBS de Boor, and flattens each `Insert` through the package-owned `Insert.Explode()`/`Insert.GetTransform()` rather than a hand-built OCS-to-WCS matrix. The read is RESILIENT by default (`CadReaderConfiguration.Failsafe = true`): recoverable corruption rides the `OnNotification` event as a structured `(NotificationType, Message, Exception)` record and the read completes with the recoverable entities, so a hard reader throw is the unrecoverable-structural case the boundary lowers ONCE to `GeometryFault.DegenerateInput` — the warning stream is the resilient rail, the throw is the floor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- license: MIT (expression)
- assembly: `ACadSharp`
- consumer-bound asset: `lib/net9.0` (highest TFM compatible with the `net10.0` consumer floor; no `lib/net10.0` ships)
- transitive deps at this floor: NONE (the `net9.0` dependency group is empty; `System.Memory`/`System.Text.Encoding.CodePages` are netstandard/net48-only and never bind here)
- namespaces: `ACadSharp.IO` (readers, config, notification), `ACadSharp` (`CadDocument`), `ACadSharp.Tables` (`BlockRecord`), `ACadSharp.Entities` (every profile entity + `IPolyline`/`ICurve`/`IVertex`), `CSMath` (`XY`/`XYZ`/`Transform`/`Matrix4`)
- asset: pure-managed AnyCPU IL (ALC-safe, coexists with the Rhino-native host-bound file I/O the architecture keeps)
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader contract, document root, notification rail (`ACadSharp.IO` / `ACadSharp`)
- rail: fabrication

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
- rail: fabrication

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities`     | entity sequence | `CadObjectCollection<Entity>` — flat model-space access (`=> ModelSpace.Entities`)  |
|  [02]   | `CadDocument.ModelSpace`   | block record    | `BlockRecord` (`=> BlockRecords["*Model_Space"]`) carrying `Entities`               |
|  [03]   | `CadDocument.BlockRecords` | block table     | `BlockRecordsTable` — the block-record table for nested blocks                      |
|  [04]   | `BlockRecord.Entities`     | entity sequence | `CadObjectCollection<Entity>` — a named block's geometry (resolved per `Insert`)    |
|  [05]   | `CadObjectCollection<T>`   | collection      | `IEnumerable<T>` + `Count`/`this[int]` (NOT `List<T>`) — enumerate via `toSeq`      |
|  [06]   | `CadDocument.Layers`       | layer table     | `LayersTable` — complete declared-layer census, including layers with zero entities |

[PUBLIC_TYPE_SCOPE]: 2D profile entity types and their polymorphic discriminators (`ACadSharp.Entities`)
- rail: fabrication

Per-type member spelling is the row-owned axis below: `LwPolyline`/`Polyline2D`/`Arc`/`Circle` resolve in `[VERTEX_ACCESS]`, `Ellipse` and `Spline` in the sampling surface, and `Insert` in `[BLOCK_TRAVERSAL]`. Each `[CAPABILITY]` cell carries the boundary role only. `IPolyline` and `ICurve` are shape discriminators for a tessellation arm spanning multiple concrete leaves.

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

[PUBLIC_TYPE_SCOPE]: curve sampling, bulge-to-arc factories, and block flatten (ACadSharp-owned, the tessellation the boundary composes)
- rail: fabrication

`PolygonalVertexes(int precision)` samples curves to the requested segment count. Circular curves return OCS vertices, and ellipse and spline rows return WCS vertices. `Arc.GetBoundingBox()` samples through `PolygonalVertexes(256)`. `Insert.Explode()` resolves `Block.Entities` and applies each child's placement transform.

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]                    | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------- | :------------------------------- | :-------------------------- |
|  [01]   | `Arc.CreateFromBulge(XY p1, XY p2, double bulge)`                  | static → `Arc`                   | mint bulge arc              |
|  [02]   | `Arc.GetCenter(XY start, XY end, double bulge)`                    | static → `XY`                    | center without radius       |
|  [03]   | `Arc.GetCenter(XY start, XY end, double bulge, out double radius)` | static → `XY`                    | center and radius           |
|  [04]   | `Arc.GetEndVertices(out XYZ start, out XYZ end)`                   | instance                         | return WCS endpoints        |
|  [05]   | `Arc.PolygonalVertexes(int precision)`                             | instance → `List<XYZ>`           | sample arc                  |
|  [06]   | `Circle.PolygonalVertexes(int precision)`                          | instance → `List<XYZ>`           | sample circle               |
|  [07]   | `Circle.GetBoundingBox()`                                          | instance → `BoundingBox`         | sample circle extent        |
|  [08]   | `Arc.GetBoundingBox()`                                             | instance → `BoundingBox`         | sample arc extent           |
|  [09]   | `Ellipse.PolygonalVertexes(int precision)`                         | instance → `List<XYZ>`           | sample ellipse              |
|  [10]   | `Ellipse.IsFullEllipse`                                            | instance → `bool`                | closure discriminator       |
|  [11]   | `Spline.PolygonalVertexes(int precision)`                          | instance → `List<XYZ>`           | sample native NURBS         |
|  [12]   | `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)`        | instance → `bool`                | probe native tessellator    |
|  [13]   | `Spline.PointOnSpline(double t)`                                   | instance                         | evaluate spline point       |
|  [14]   | `Spline.TryPointOnSpline(double t, out XYZ)`                       | instance                         | probe spline point          |
|  [15]   | `Spline.UpdateFromFitPoints(uint iterationLimit = 255)`            | instance → `bool`                | rebuild fit-point spline    |
|  [16]   | `Insert.Explode()`                                                 | instance → `IEnumerable<Entity>` | flatten placed block        |
|  [17]   | `Insert.GetTransform()`                                            | instance → `Transform`           | return composed affine      |
|  [18]   | `Insert.ApplyTransform(Transform)`                                 | instance → `Transform`           | apply composed affine       |
|  [19]   | `CSMath.Matrix3.ArbitraryAxis(XYZ zAxis)`                          | static → `Matrix3`               | OCS-to-WCS basis            |
|  [20]   | `CSMath.Matrix3.RotationZ(double angle)`                           | static → `Matrix3`               | in-plane rotation           |
|  [21]   | `CSMath.Matrix3.operator *(Matrix3, XYZ)`                          | operator → `XYZ`                 | apply basis to a point      |
|  [22]   | `CSMath.Matrix3.Transpose()`                                       | instance → `Matrix3`             | invert an orthonormal basis |
|  [23]   | `Hatch.Paths`                                                      | instance → `List<BoundaryPath>`  | enumerate hatch loops       |
|  [24]   | `Hatch.BoundaryPath.Edges`                                         | instance → edge collection       | preserve typed edge leaves  |
|  [25]   | `Hatch.BoundaryPath.GetPoints(int precision = 256)`                | instance → `IEnumerable<XYZ>`    | sample one closed boundary  |
|  [26]   | `Hatch.BoundaryPath.Edge.Type`                                     | instance → `EdgeType`            | discriminate edge leaves    |
|  [27]   | `Hatch.BoundaryPath.Line.Start` / `.End`                           | instance → `XY`                  | exact OCS line endpoints    |
|  [28]   | `Hatch.BoundaryPath.Arc.Center` / `.Radius`                        | instance → `XY` / `double`       | exact OCS circle            |
|  [29]   | `Hatch.BoundaryPath.Arc.StartAngle` / `.EndAngle`                  | instance → `double`              | exact angular interval      |
|  [30]   | `Hatch.BoundaryPath.Arc.CounterClockWise`                          | instance → `bool`                | exact sweep sense           |
|  [31]   | `Hatch.BoundaryPath.Ellipse.PolygonalVertexes(int)`                | instance → `List<XYZ>`           | provider conic sampling     |
|  [32]   | `Hatch.BoundaryPath.Polyline.Vertices` / `.Bulges` / `.IsClosed`   | instance collection + flag       | exact bulge path            |
|  [33]   | `Hatch.BoundaryPath.Spline.PolygonalVertexes(int)`                 | instance → `List<XYZ>`           | provider spline sampling    |

[FRAME_LAW]:
- OCS coordinates: `LwPolyline.Vertex.Location` (`XY`, documented "in OCS") at `LwPolyline.Elevation`, `Circle.Center`, `Arc.Center`, and `Hatch.BoundaryPath.Arc.Center` are OCS values; `Circle.PolygonalVertexes` and `Arc.PolygonalVertexes` return OCS vertices.
- WCS coordinates: `Ellipse.Center`, `Ellipse.MajorAxisEndPoint`, `Point.Location`, `Insert.InsertPoint`, `Arc.GetEndVertices`, `Ellipse.PolygonalVertexes`, and `Spline.PolygonalVertexes` publish WCS directly.
- Bridge: `Matrix3.ArbitraryAxis(entity.Normal) * ocsPoint` is the package-owned OCS-to-WCS map (the package itself composes `ArbitraryAxis(...).Transpose()` for the inverse in `Hatch.BoundaryPath.Arc(Circle)`); a hand-rolled axis derivation or a sign flip standing in for it is the rejected form.
- Sense: a mirrored extrusion direction (`Normal.Z < 0`) inverts in-plane arc sense, so a bulge value carried across the frame multiplies by `Math.Sign(normal.Z)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read — `DxfReader` / `DwgReader` facades (`ACadSharp.IO`)
- rail: fabrication

`DxfReader` and `DwgReader` file reads return `CadDocument`; instance reads subscribe `OnNotification` and `OnProgress` before `.Read()`.

| [INDEX] | [KEY]           | [SURFACE]                   | [ENTRY_FAMILY] | [CAPABILITY]       |
| :-----: | :-------------- | :-------------------------- | :------------- | :----------------- |
|  [01]   | `DXF_PATH`      | `DxfReader.Read`            | static read    | read DXF file      |
|  [02]   | `DXF_STREAM`    | `DxfReader.Read`            | static read    | read DXF stream    |
|  [03]   | `DXF_CONFIG`    | `DxfReader.Read`            | static read    | configured DXF     |
|  [04]   | `DXF_INSTANCE`  | `DxfReader.Read`            | instance read  | disposable read    |
|  [05]   | `DWG_PATH`      | `DwgReader.Read`            | static read    | read DWG file      |
|  [06]   | `DWG_CONFIG`    | `DwgReader.Read`            | static read    | configured DWG     |
|  [07]   | `DXF_BINARY`    | `DxfReader.IsBinary`        | static probe   | classify DXF form  |
|  [08]   | `READ_HEADER`   | `ICadReader.ReadHeader`     | partial read   | read header        |
|  [09]   | `READ_TABLES`   | `DxfReader.ReadTables`      | partial read   | read tables        |
|  [10]   | `READ_ENTITIES` | `DxfReader.ReadEntities`    | partial read   | read entities      |
|  [11]   | `DWG_PREVIEW`   | `DwgReader.ReadPreview`     | DWG metadata   | read preview image |
|  [12]   | `DWG_SUMMARY`   | `DwgReader.ReadSummaryInfo` | DWG metadata   | read summary info  |

[FILE_READ_SIGNATURES]:
- `DXF_PATH`: `DxfReader.Read(string filename, NotificationEventHandler notification = null)` returns `CadDocument`; notification is optional.
- `DXF_STREAM`: `DxfReader.Read(Stream stream, NotificationEventHandler notification = null)` returns `CadDocument`.
- `DXF_CONFIG`: `DxfReader.Read(string filename, DxfReaderConfiguration configuration, NotificationEventHandler notification = null)` carries `Failsafe` and cache knobs.
- `DXF_INSTANCE`: `new DxfReader(string filename, NotificationEventHandler notification = null)` followed by `.Read()`.
- `DWG_PATH`: `DwgReader.Read(string filename, NotificationEventHandler notification = null)` returns `CadDocument`.
- `DWG_CONFIG`: `DwgReader.Read(string filename, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)` carries `CrcCheck` and `ReadSummaryInfo`.
- `DXF_BINARY`: `DxfReader.IsBinary(string filename)` or `DxfReader.IsBinary(Stream, bool resetPos)` discriminates ASCII and binary DXF before the read.
- `READ_HEADER`: `ICadReader.ReadHeader()` reads the header without a full document read.
- `READ_TABLES`: `DxfReader.ReadTables()` reads tables without a full document read.
- `READ_ENTITIES`: `DxfReader.ReadEntities()` reads entities without a full document read.
- `DWG_PREVIEW`: `DwgReader.ReadPreview()` reads the embedded preview image without a full document read.
- `DWG_SUMMARY`: `DwgReader.ReadSummaryInfo()` reads the summary-info block without a full document read.

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → top-level model-space entities
- rail: fabrication

`CadDocument.Entities` aliases `ModelSpace.Entities`, and `CadDocument.ModelSpace` aliases `BlockRecords["*Model_Space"]`. `BlockRecord.Entities` remains nested until an `Insert` resolves it.

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY]  | [CAPABILITY]         |
| :-----: | :----------------------- | :-------------- | :------------------- |
|  [01]   | `CadDocument.Entities`   | entity sequence | top-level entity set |
|  [02]   | `CadDocument.ModelSpace` | block record    | model-space block    |
|  [03]   | `BlockRecord.Entities`   | entity sequence | nested-block entity  |

## [04]-[RATIFIED]

[READER_RAIL]:
- Surface: `ICadReader : IDisposable` owns `Read()`/`ReadHeader()` plus the `OnNotification` and `OnProgress` events. The canonical boundary call is `DxfReader.Read(path, notification)` or `DwgReader.Read(path, notification)`.
- Resilience: `CadReaderConfiguration.Failsafe` is `true` by default, so recoverable corruption routes to `NotificationEventArgs(NotificationType, Message, Exception)` while the read completes. `Failsafe=false` promotes recoverable defects to throws at the same boundary.
- Boundary: a hard reader throw lowers once to `GeometryFault.DegenerateInput`; the reader exception never escapes, and the subscribed notification stream folds into the admission receipt.

[VERTEX_ACCESS]:
- Lightweight polyline: `LwPolyline.Vertices` is `List<Vertex>`; `Vertex.Location` is `CSMath.XY`, `Vertex.Bulge` is the arc-column value, and `LwPolyline.IsClosed` owns closure. `LwPolyline.Elevation` carries the OCS Z the 2D vertices omit, `LwPolyline.Normal` (default `XYZ.AxisZ`) carries the extrusion direction, and `ConstantWidth`/`Thickness`/`Flags` carry the remaining polyline state — reading `Location` alone flattens every polyline onto Z zero.
- Polyline2D: `Polyline2D.Vertices` is `SeqendCollection<Vertex2D>`; `Vertex2D.Location` is a plain `XYZ` property and there is no `Pt(XYZ)` provider member.
- Curves: `Arc : Circle` carries `Center`, `Radius`, `StartAngle`, `EndAngle`, `Sweep`, and `GetEndVertices`; `Circle : Entity, ICurve` carries `Center`, `Radius`, `Normal`, and `Thickness`.
- Sampler: `Arc.CreateFromBulge(XY p1, XY p2, double bulge)`, `Arc.PolygonalVertexes(int precision)`, and `Circle.PolygonalVertexes(int precision)` own bulge, arc, and circle tessellation.

[SPLINE_SAMPLER]:
- Surface: `Spline.PolygonalVertexes(int precision)` returns `List<XYZ>` in WCS, and `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)` is the non-throwing probe.
- Fit path: `ControlPoints`, `Knots`, `Weights`, `Degree`, and `FitPoints` are read-side properties; `UpdateFromFitPoints(uint iterationLimit = 255)` rebuilds fit-point-only splines before sampling.
- Boundary: `PointOnSpline(double t)` and `TryPointOnSpline(double t, out XYZ)` own single-point evaluation; profile import never hand-rolls de Boor or control-polygon sampling.

[ELLIPSE_SAMPLER]:
- Surface: `Ellipse.PolygonalVertexes(int precision)` returns WCS `List<XYZ>`, and `Ellipse.IsFullEllipse` owns closure.
- Boundary: profile import composes both members; no conic sampler or closure inference survives beside them.

[BLOCK_TRAVERSAL]:
- Model space: `CadDocument.Entities` is `ModelSpace.Entities` and does not flatten nested blocks.
- Insert: `Insert.Block` references `BlockRecord.Entities`; `InsertPoint`, `XScale`, `YScale`, `ZScale`, `Rotation`, and `Normal` carry placement, while `RowCount`, `ColumnCount`, `RowSpacing`, `ColumnSpacing`, and `IsMultiple` carry the MINSERT array.
- Flatten: `Insert.Explode()` resolves `Block.Entities` and applies placement per child in one package call; `Insert.GetTransform()` and `Insert.ApplyTransform(Transform)` expose the same affine.
- Trap: `Explode()` enumerates `Block.Entities` exactly once under the single `GetTransform()` affine — it does NOT replicate the `RowCount`/`ColumnCount` array, so a consumer that needs every MINSERT occurrence expands the row/column grid itself, offsetting each replica by `Matrix3.ArbitraryAxis(Normal) * Matrix3.RotationZ(Rotation)` applied to the spacing vector.
- Trap: `Explode()` rewrites a block-nested `Circle` into an `Ellipse` carrying `RadiusRatio = 1.0` and `MajorAxisEndPoint = XYZ.AxisX * Radius`; exact circle identity does not survive the call, so a consumer preserving exact arcs re-reads a unit-ratio full ellipse as a circle rather than sampling it. `Arc` survives as `Arc`.
- Boundary: profile import recurses over the exploded child entities under an ancestor set keyed on `BlockRecord.Handle`, because a self-referencing block otherwise recurses without bound; a hand-built `Insert` matrix is the rejected form.

[CONFIG_KNOBS]:
- Surface: `DxfReader.Read(path, DxfReaderConfiguration, notification)` and `DwgReader.Read(path, DwgReaderConfiguration, notification)` own explicit reader configuration.
- Shared knobs: `CadReaderConfiguration` carries `Failsafe`, `KeepUnknownEntities`, and `KeepUnknownNonGraphicalObjects`.
- Format knobs: `DxfReaderConfiguration` adds `ClearCache` and `CreateDefaults`; `DwgReaderConfiguration` adds `CrcCheck` and `ReadSummaryInfo`.

[RAIL_LAW]:
- Package: `ACadSharp`, MIT, consumer-bound `lib/net9.0`, zero transitive deps at this floor
- Owns: DXF/DWG file read into the `CadDocument` model and the 2D profile entity surface the profile-import boundary tessellates into `Loop` sets
- Accept: file path or stream input, `ACadSharp.IO.NotificationEventHandler`, `OnNotification`, `OnProgress`, and optional `DxfReaderConfiguration` or `DwgReaderConfiguration`.
- Accept: `LwPolyline`, `Polyline2D`, `Line`, `Arc`, `Circle`, `Ellipse`, and `Spline` profile entities sampled through `CreateFromBulge` and `PolygonalVertexes`.
- Accept: `Insert` block references flattened through `Insert.Explode()` and the package transform composer.
- Reject: DWG/DXF write from this folder; the AppUi two-format ACadSharp drafting WRITE leg owns CAD write, and this boundary is read-only profile ingress.
- Reject: `CadDocument` or ACadSharp entity types escaping into sibling kernels.
- Reject: hand-rolled bulge, NURBS, or `Insert` transform logic where ACadSharp owns `Arc.CreateFromBulge`, `Spline.PolygonalVertexes`, `Insert.Explode()`, `Insert.GetTransform()`, and `Insert.ApplyTransform`.
- Reject: `Vertex2D.Pt(XYZ)`, reader exception escape, or an assumption that `Read` always throws on bad input.
