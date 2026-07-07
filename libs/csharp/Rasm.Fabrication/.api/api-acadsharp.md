# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` is the fabrication read-side CAD owner for AutoCAD DXF (ASCII and binary) and DWG (AC1014 through AC1032); `Rasm.AppUi` owns the two-format ACadSharp drafting write leg. Fabrication consumes only the read surface to admit external 2D part profiles into the canonical `Loop` vocabulary at the `Ingress/profile#PROFILE_IMPORT` boundary. At the consumer floor (`net10.0` binds the package's highest compatible `lib/net9.0` asset — the `net9.0` dependency group is EMPTY, so the read path drags zero transitive package weight; the netstandard/net48 `System.Memory`/`System.Text.Encoding.CodePages` deps never reach this consumer) the surface splits across four namespaces the boundary `using`s explicitly: the reader facades and the notification/config rail live in `ACadSharp.IO`, the document root and tables in `ACadSharp`/`ACadSharp.Tables`, every profile entity in `ACadSharp.Entities`, and the coordinate/transform primitives in `CSMath`. The reader contract is the `ICadReader : IDisposable` interface (`Read()`/`ReadHeader()` plus `OnNotification`/`OnProgress` events) behind the static `DxfReader.Read`/`DwgReader.Read` facades and the `new DxfReader(filename, notification)`/`.Read()` instance form, all returning a `CadDocument` model root whose `Entities` (`=> ModelSpace.Entities`), `ModelSpace`, and `BlockRecords` carry the drawing geometry. Closed 2D profiles arrive as `LwPolyline` (an `IPolyline` whose `Vertex` carries a `CSMath.XY Location` plus a `double Bulge` — the tangent of one quarter the arc's included angle), `Polyline2D` (`Polyline<Vertex2D> : IPolyline`), `Line`, `Arc`/`Circle` (`Circle : Entity, ICurve`; `Arc : Circle`), and `Spline` entities, and nested-block profiles arrive through `Insert` block references; the boundary tessellates each bulge/arc/circle/spline span to `Loop` vertices through the ACadSharp-owned `Arc.CreateFromBulge`/`PolygonalVertexes`/`Spline.PolygonalVertexes` curve sampler at the owner's chord-precision knob, never a hand-rolled bulge trigonometry or a hand-rolled NURBS de Boor, and flattens each `Insert` through the package-owned `Insert.Explode()`/`Insert.GetTransform()` rather than a hand-built OCS-to-WCS matrix. The read is RESILIENT by default (`CadReaderConfiguration.Failsafe = true`): recoverable corruption rides the `OnNotification` event as a structured `(NotificationType, Message, Exception)` record and the read completes with the recoverable entities, so a hard reader throw is the unrecoverable-structural case the boundary lowers ONCE to `GeometryFault.DegenerateInput` — the warning stream is the resilient rail, the throw is the floor.

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

[PUBLIC_TYPE_SCOPE]: entity collections on the document
- rail: fabrication

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                       |
| :-----: | :------------------------- | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities`     | entity sequence | `CadObjectCollection<Entity>` — flat model-space access (`=> ModelSpace.Entities`) |
|  [02]   | `CadDocument.ModelSpace`   | block record    | `BlockRecord` (`=> BlockRecords["*Model_Space"]`) carrying `Entities`              |
|  [03]   | `CadDocument.BlockRecords` | block table     | `BlockRecordsTable` — the block-record table for nested blocks                     |
|  [04]   | `BlockRecord.Entities`     | entity sequence | `CadObjectCollection<Entity>` — a named block's geometry (resolved per `Insert`)   |
|  [05]   | `CadObjectCollection<T>`   | collection      | `IEnumerable<T>` + `Count`/`this[int]` (NOT `List<T>`) — enumerate via `toSeq`     |

[PUBLIC_TYPE_SCOPE]: 2D profile entity types and their polymorphic discriminators (`ACadSharp.Entities`)
- rail: fabrication

The per-type member spelling is the row-owned axis below: `LwPolyline`/`Polyline2D`/`Arc`/`Circle` resolve in `[VERTEX_ACCESS]`, `Spline` in `[SPLINE_SAMPLER]`, and `Insert` in `[BLOCK_TRAVERSAL]`. The `[CAPABILITY]` cell carries the boundary role only. `IPolyline`/`ICurve` are the shape discriminators the `Admit` switch may match on instead of the concrete leaf when one tessellation arm spans both polyline forms.

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
|  [10]   | `Spline`            | NURBS spline        | `Spline : Entity` — native-tessellated control-point/knot spline                     |
|  [11]   | `Insert`            | block reference     | placed/arrayed nested-block reference with package-owned explode + transform         |

[PUBLIC_TYPE_SCOPE]: curve sampling, bulge-to-arc factories, and block flatten (ACadSharp-owned, the tessellation the boundary composes)
- rail: fabrication

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY]                    | [CAPABILITY]                                                                                                            |
| :-----: | :----------------------------------------------------------------------- | :------------------------------- | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Arc.CreateFromBulge(XY p1, XY p2, double bulge)`                        | static → `Arc`                   | the bulge-segment-to-`Arc` mint the README names — the boundary's bulge→arc path                                        |
|  [02]   | `Arc.GetCenter(XY start, XY end, double bulge)`                          | static → `XY`                    | the bulge arc's center directly (radius-free 3-arg form)                                                                |
|  [03]   | `Arc.GetCenter(XY start, XY end, double bulge, out double radius)`       | static → `XY`                    | the bulge arc's center + radius without minting the `Arc`                                                               |
|  [04]   | `Arc.GetEndVertices(out XYZ start, out XYZ end)`                         | instance                         | the arc's two endpoint vertices in WCS without sampling                                                                 |
|  [05]   | `Arc.PolygonalVertexes(int precision)`                                   | instance → `List<XYZ>`           | sample the arc to `precision` segments in OCS — the chord-precision knob (override)                                     |
|  [06]   | `Circle.PolygonalVertexes(int precision)`                                | instance → `List<XYZ>`           | sample the full circle to `precision` segments in OCS (virtual base sampler)                                            |
|  [07]   | `Circle.GetBoundingBox()` / `Arc.GetBoundingBox()`                       | instance → `BoundingBox`         | the sampled extent (`Arc` overrides via `PolygonalVertexes(256)`)                                                       |
|  [08]   | `Spline.PolygonalVertexes(int precision)`                                | instance → `List<XYZ>`           | native NURBS tessellator — sample the spline to `precision` chord vertices in WCS                                       |
|  [09]   | `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)`              | instance → `bool`                | the non-throwing tessellate probe (false on an unevaluable/degenerate spline)                                           |
|  [10]   | `Spline.PointOnSpline(double t)` / `TryPointOnSpline(double t, out XYZ)` | instance                         | evaluate one parametric point on the spline at `t` (throwing + probe forms)                                             |
|  [11]   | `Spline.UpdateFromFitPoints(uint iterationLimit = 255)`                  | instance → `bool`                | rebuild `ControlPoints`/`Knots`/`Weights` from `FitPoints` before sampling                                              |
|  [12]   | `Insert.Explode()`                                                       | instance → `IEnumerable<Entity>` | the package-owned block flattener — resolves `Block.Entities` AND applies the placement transform per child in one call |
|  [13]   | `Insert.GetTransform()` / `Insert.ApplyTransform(Transform)`             | instance → `Transform`           | the composed OCS-to-WCS affine (`CSMath.Transform`) the flatten reuses                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read — `DxfReader` / `DwgReader` facades (`ACadSharp.IO`)
- rail: fabrication

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `DxfReader.Read(string filename, NotificationEventHandler notification = null)`                                       | static read    | read a DXF file into a `CadDocument`; notification is OPTIONAL                      |
|  [02]   | `DxfReader.Read(Stream stream, NotificationEventHandler notification = null)`                                         | static read    | read a DXF stream into a `CadDocument`                                              |
|  [03]   | `DxfReader.Read(string filename, DxfReaderConfiguration configuration, NotificationEventHandler notification = null)` | static read    | read with an explicit reader configuration (`Failsafe`/cache knobs)                 |
|  [04]   | `new DxfReader(string filename, NotificationEventHandler notification = null)` + `.Read()`                            | instance read  | the disposable instance-reader form (subscribe `OnNotification`/`OnProgress` first) |
|  [05]   | `DwgReader.Read(string filename, NotificationEventHandler notification = null)`                                       | static read    | read a DWG file into a `CadDocument`                                                |
|  [06]   | `DwgReader.Read(string filename, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)` | static read    | DWG read with `CrcCheck`/`ReadSummaryInfo` knobs                                    |
|  [07]   | `DxfReader.IsBinary(string filename)` / `DxfReader.IsBinary(Stream, bool resetPos)`                                   | static probe   | discriminate ASCII vs binary DXF before committing the read                         |
|  [08]   | `ICadReader.ReadHeader()` / `DxfReader.ReadTables()` / `DxfReader.ReadEntities()`                                     | partial read   | header-only / table-only / entity-only reads beside the full `Read()`               |
|  [09]   | `DwgReader.ReadPreview()` / `DwgReader.ReadSummaryInfo()`                                                             | DWG metadata   | the embedded preview image and summary-info block without a full read               |

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → top-level model-space entities
- rail: fabrication

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [CAPABILITY]                                                                                     |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities` (`=> ModelSpace.Entities`)            | entity sequence | `CadObjectCollection<Entity>` — the boundary's top-level entity set                              |
|  [02]   | `CadDocument.ModelSpace` (`=> BlockRecords["*Model_Space"]`) | block record    | the model-space `BlockRecord` carrying `Entities`                                                |
|  [03]   | `BlockRecord.Entities`                                       | entity sequence | a named block's entities — read ONLY when an `Insert` reference is resolved (not auto-flattened) |

## [04]-[RATIFIED]

[READER_RAIL]:
- Surface: `ICadReader : IDisposable` owns `Read()`/`ReadHeader()` plus the `OnNotification` and `OnProgress` events. The canonical boundary call is `DxfReader.Read(path, notification)` or `DwgReader.Read(path, notification)`.
- Resilience: `CadReaderConfiguration.Failsafe` is `true` by default, so recoverable corruption routes to `NotificationEventArgs(NotificationType, Message, Exception)` while the read completes. `Failsafe=false` promotes recoverable defects to throws at the same boundary.
- Boundary: a hard reader throw lowers once to `GeometryFault.DegenerateInput`; the reader exception never escapes, and the subscribed notification stream folds into the admission receipt.

[VERTEX_ACCESS]:
- Lightweight polyline: `LwPolyline.Vertices` is `List<Vertex>`; `Vertex.Location` is `CSMath.XY`, `Vertex.Bulge` is the arc-column value, and `LwPolyline.IsClosed` owns closure.
- Polyline2D: `Polyline2D.Vertices` is `SeqendCollection<Vertex2D>`; `Vertex2D.Location` is a plain `XYZ` property and there is no `Pt(XYZ)` provider member.
- Curves: `Arc : Circle` carries `Center`, `Radius`, `StartAngle`, `EndAngle`, `Sweep`, and `GetEndVertices`; `Circle : Entity, ICurve` carries `Center`, `Radius`, `Normal`, and `Thickness`.
- Sampler: `Arc.CreateFromBulge(XY p1, XY p2, double bulge)`, `Arc.PolygonalVertexes(int precision)`, and `Circle.PolygonalVertexes(int precision)` own bulge, arc, and circle tessellation.

[SPLINE_SAMPLER]:
- Surface: `Spline.PolygonalVertexes(int precision)` returns `List<XYZ>` in WCS, and `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)` is the non-throwing probe.
- Fit path: `ControlPoints`, `Knots`, `Weights`, `Degree`, and `FitPoints` are read-side properties; `UpdateFromFitPoints(uint iterationLimit = 255)` rebuilds fit-point-only splines before sampling.
- Boundary: `PointOnSpline(double t)` and `TryPointOnSpline(double t, out XYZ)` own single-point evaluation; profile import never hand-rolls de Boor or control-polygon sampling.

[BLOCK_TRAVERSAL]:
- Model space: `CadDocument.Entities` is `ModelSpace.Entities` and does not flatten nested blocks.
- Insert: `Insert.Block` references `BlockRecord.Entities`; `InsertPoint`, `XScale`, `YScale`, `ZScale`, `Rotation`, and `Normal` carry placement.
- Flatten: `Insert.Explode()` resolves `Block.Entities` and applies placement per child in one package call; `Insert.GetTransform()` and `Insert.ApplyTransform(Transform)` expose the same affine.
- Boundary: profile import recurses over the exploded child entities; a hand-built `Insert` matrix is the rejected form.

[CONFIG_KNOBS]:
- Surface: `DxfReader.Read(path, DxfReaderConfiguration, notification)` and `DwgReader.Read(path, DwgReaderConfiguration, notification)` own explicit reader configuration.
- Shared knobs: `CadReaderConfiguration` carries `Failsafe`, `KeepUnknownEntities`, and `KeepUnknownNonGraphicalObjects`.
- Format knobs: `DxfReaderConfiguration` adds `ClearCache` and `CreateDefaults`; `DwgReaderConfiguration` adds `CrcCheck` and `ReadSummaryInfo`.

[RAIL_LAW]:
- Package: `ACadSharp`, MIT, consumer-bound `lib/net9.0`, zero transitive deps at this floor
- Owns: DXF/DWG file read into the `CadDocument` model and the 2D profile entity surface the `Ingress/profile#PROFILE_IMPORT` boundary tessellates into `Loop` sets
- Accept: file path or stream input, `ACadSharp.IO.NotificationEventHandler`, `OnNotification`, `OnProgress`, and optional `DxfReaderConfiguration` or `DwgReaderConfiguration`.
- Accept: `LwPolyline`, `Polyline2D`, `Line`, `Arc`, `Circle`, and `Spline` profile entities sampled through `CreateFromBulge` and `PolygonalVertexes`.
- Accept: `Insert` block references flattened through `Insert.Explode()` and the package transform composer.
- Reject: DWG/DXF write from this folder; the AppUi two-format ACadSharp drafting WRITE leg owns CAD write, and this boundary is read-only profile ingress.
- Reject: `CadDocument` or ACadSharp entity types escaping into sibling kernels.
- Reject: hand-rolled bulge, NURBS, or `Insert` transform logic where ACadSharp owns `Arc.CreateFromBulge`, `Spline.PolygonalVertexes`, `Insert.Explode()`, `Insert.GetTransform()`, and `Insert.ApplyTransform`.
- Reject: `Vertex2D.Pt(XYZ)`, reader exception escape, or an assumption that `Read` always throws on bad input.
