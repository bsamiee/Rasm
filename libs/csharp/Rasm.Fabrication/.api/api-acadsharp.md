# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` is a pure-managed AnyCPU IL-only library reading and writing AutoCAD DXF (ASCII and binary) and DWG (AC1014 through AC1032) files; the fabrication folder consumes only its READ surface to admit external 2D part profiles into the canonical `Loop` vocabulary at the `Polygon/import#PROFILE_IMPORT` boundary. The primary entry points are the static `DxfReader.Read`/`DwgReader.Read` facades and the `new DxfReader(filename, notification)`/`.Read()` instance form, all returning a `CadDocument` model root whose `Entities` (an alias for `ModelSpace.Entities`), `ModelSpace`, and `BlockRecords` collections carry the drawing geometry. Closed 2D profiles arrive as `LwPolyline` (a lightweight polyline whose `Vertex` carries a `CSMath.XY Location` plus a `double Bulge` — the tangent of one quarter the arc's included angle), `Polyline2D`, `Line`, `Arc`, `Circle`, and `Spline` entities, and nested-block profiles arrive through `Insert` block references; the boundary tessellates each bulge/arc/circle/spline span to `Loop` vertices through the ACadSharp-owned `Arc.CreateFromBulge`/`Arc.PolygonalVertexes`/`Circle.PolygonalVertexes`/`Spline.PolygonalVertexes` curve sampler at the owner's chord-precision knob, never a hand-rolled bulge trigonometry or a hand-rolled NURBS de Boor. No native asset and no RID burden — the package is managed IL, ALC-safe, and coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- assembly: `ACadSharp`
- namespace: `ACadSharp`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden)
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and reader facades
- rail: fabrication

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `CadDocument`              | model root    | the read drawing — entities, blocks, tables |
|  [02]   | `DxfReader`                | static reader | DXF (ASCII + binary) file read              |
|  [03]   | `DwgReader`                | static reader | DWG (AC1014–AC1032) file read               |
|  [04]   | `NotificationEventHandler` | callback      | read-progress / warning notification sink   |

[PUBLIC_TYPE_SCOPE]: entity collections on the document
- rail: fabrication

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------- | :-------------- | :--------------------------------------- |
|  [01]   | `Entities`     | entity sequence | all model-space entities (flat access)   |
|  [02]   | `ModelSpace`   | block record    | the model-space block and its entities   |
|  [03]   | `BlockRecords` | block table     | the block-record table for nested blocks |

[PUBLIC_TYPE_SCOPE]: 2D profile entity types
- rail: fabrication

The per-type member spelling is the row-owned axis below: `LwPolyline`/`Polyline2D`/`Arc`/`Circle` resolve in `[VERTEX_ACCESS]`, `Spline` in `[SPLINE_SAMPLER]`, and `Insert` in `[BLOCK_TRAVERSAL]`. The `[CAPABILITY]` cell carries the boundary role only.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `LwPolyline`        | lightweight poly | closed/open bulge polyline — primary 2D profile    |
|  [02]   | `LwPolyline.Vertex` | per-vertex       | `Location`/`Bulge` vertex of the lightweight poly  |
|  [03]   | `Polyline2D`        | 2D polyline      | `Seqend`-collection bulge polyline (non-`List`)    |
|  [04]   | `Line`              | line segment     | straight `StartPoint`→`EndPoint` segment           |
|  [05]   | `Arc`               | circular arc     | `Arc : Circle` partial-sweep arc                   |
|  [06]   | `Circle`            | full circle      | `Center`/`Radius` closed circle                    |
|  [07]   | `Spline`            | NURBS spline     | native-tessellated control-point/knot spline       |
|  [08]   | `Insert`            | block reference  | placed/arrayed nested-block reference              |

[PUBLIC_TYPE_SCOPE]: curve sampling and bulge-to-arc factories (ACadSharp-owned, the tessellation the boundary composes)
- rail: fabrication

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]          | [CAPABILITY]                                                                     |
| :-----: | :----------------------------------------------------------------- | :--------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Arc.CreateFromBulge(XY p1, XY p2, double bulge)`                  | static factory         | the bulge-segment-to-`Arc` mint the README names — the boundary's bulge→arc path |
|  [02]   | `Arc.GetCenter(XY start, XY end, double bulge, out double radius)` | static                 | the bulge arc's center + radius without minting the `Arc`                        |
|  [03]   | `Arc.PolygonalVertexes(int precision)`                             | instance → `List<XYZ>` | sample the arc to `precision` segments in OCS — the chord-precision knob         |
|  [04]   | `Circle.PolygonalVertexes(int precision)`                          | instance → `List<XYZ>` | sample the full circle to `precision` segments in OCS                            |
|  [05]   | `Spline.PolygonalVertexes(int precision)`                          | instance → `List<XYZ>` | native NURBS tessellator — sample the spline to `precision` chord vertices in WCS |
|  [06]   | `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)`        | instance → `bool`      | the non-throwing tessellate probe (false on an unevaluable/degenerate spline)    |
|  [07]   | `Spline.PointOnSpline(double t)`                                   | instance → `XYZ`       | evaluate one parametric point on the spline at `t`                               |
|  [08]   | `Spline.UpdateFromFitPoints(uint iterationLimit = 255)`           | instance → `bool`      | rebuild `ControlPoints`/`Knots`/`Weights` from `FitPoints` before sampling       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read — `DxfReader` / `DwgReader` facades
- rail: fabrication

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `DxfReader.Read(string filename, NotificationEventHandler notification = null)`                   | static read    | read a DXF file into a `CadDocument`; notification is OPTIONAL |
|  [02]   | `DxfReader.Read(Stream stream, NotificationEventHandler notification = null)`                     | static read    | read a DXF stream into a `CadDocument`                         |
|  [03]   | `DxfReader.Read(string filename, DxfReaderConfiguration configuration, NotificationEventHandler notification = null)` | static read    | read with an explicit reader configuration                     |
|  [04]   | `new DxfReader(string filename, NotificationEventHandler notification = null)` + `.Read()`                     | instance read  | the disposable instance-reader form beside the static facade   |
|  [05]   | `DwgReader.Read(string filename, NotificationEventHandler notification = null)`                   | static read    | read a DWG file into a `CadDocument`                           |

[ENTRYPOINT_SCOPE]: document traversal — `CadDocument` → top-level model-space entities
- rail: fabrication

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]  | [CAPABILITY]                                                                                       |
| :-----: | :---------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `CadDocument.Entities` (= `ModelSpace.Entities`)            | entity sequence | `CadObjectCollection<Entity>` — the boundary's top-level entity set |
|  [02]   | `CadDocument.ModelSpace` (= `BlockRecords["*Model_Space"]`) | block record    | the model-space block carrying `Entities`                                                          |
|  [03]   | `BlockRecord.Entities`                                      | entity sequence | a named block's entities — read ONLY when an `Insert` reference is resolved (not auto-flattened)   |

## [04]-[RATIFIED]

- [READER_OVERLOADS] The reader exposes both a static facade and a disposable instance form; the `NotificationEventHandler notification = null` parameter is OPTIONAL on every overload, so the boundary passes a warning sink or omits it. The canonical boundary call is the static `DxfReader.Read(path, notification)` (and `DwgReader.Read(path, notification)` for DWG), with the instance `new DxfReader(path, notification)` + `using`/`.Read()` form available when the reader handle must be disposed deterministically. The `Read` facade THROWS on a malformed/unreadable file — the boundary wraps the call in a try/catch and lowers the caught exception to `GeometryFault.DegenerateInput` ONCE at admission (the reader exception never escapes the boundary).
- [VERTEX_ACCESS] `LwPolyline.Vertices` is a `List<Vertex>` whose `Vertex` carries `Location` (`CSMath.XY` with `.X`/`.Y` doubles) and `Bulge` (`double`, the tangent of one quarter the included angle, sign = arc direction); closure is `LwPolyline.IsClosed` (a `Flags.HasFlag(Closed)` projection off `LwPolylineFlags`), and `LwPolyline.Elevation: double` plus `Normal: XYZ` seat the 2D profile on its OCS Z-plane. `Polyline2D` is `Polyline<Vertex2D>` whose `Vertices` is a `SeqendCollection<Vertex2D>` (a `CadObjectCollection<Vertex2D>` subclass: `IEnumerable<Vertex2D>` with `Count`/`this[int]`, NOT a `List<T>` — so a consumer enumerates it through `toSeq`, never a `List`-only member), with `IsClosed` projected off `Flags`; `Vertex2D : ACadSharp.Entities.Vertex` carries `Location: XYZ` (read through the `Pt(XYZ)` overload) and a `Bulge: double`. `Arc : Circle` inherits `Center` (`XYZ`) and `Radius` (`double`) and adds `StartAngle`/`EndAngle`/`Sweep` (radians); `Circle` carries `Center`/`Radius`/`Normal`. ACadSharp SHIPS the bulge-to-arc factory the README names — `Arc.CreateFromBulge(XY p1, XY p2, double bulge)` (and `Arc.GetCenter(start, end, bulge, out radius)` for the center/radius without minting) — and the curve sampler `Arc.PolygonalVertexes(int precision)` / `Circle.PolygonalVertexes(int precision)` returning `List<XYZ>` in object-coordinate space, so the boundary tessellates every bulge/arc/circle span through the package-owned sampler at one `precision` knob, never a hand-rolled bulge trigonometry. A zero `Bulge` is a straight segment kept as the raw `Location`; a non-zero `Bulge` mints the arc through `CreateFromBulge` and samples it.
- [SPLINE_SAMPLER] ACadSharp SHIPS a native NURBS tessellator, so the boundary samples the spline through the package surface and never a hand-rolled in-folder de Boor or control-polygon walk. `Spline.PolygonalVertexes(int precision)` returns the chord-sampled `List<XYZ>` in WCS at the `precision` knob — the same chord-precision knob the `Arc`/`Circle` samplers carry — and `Spline.TryPolygonalVertexes(int precision, out List<XYZ>)` is the non-throwing probe that returns `false` on an unevaluable spline (the boundary lowers a `false` to `GeometryFault.DegenerateInput`). The control-point/knot data (`ControlPoints`, `Knots`, `Weights`, `Degree`) plus `IsClosed`/`IsPeriodic` are read-side properties; a spline authored from `FitPoints` only is rebuilt through `UpdateFromFitPoints(iterationLimit)` before sampling. `PointOnSpline(double t)`/`TryPointOnSpline(double t, out XYZ)` evaluate a single parametric point when a non-uniform sampling is needed.
- [BLOCK_TRAVERSAL] `CadDocument.Entities` is exactly `ModelSpace.Entities` and does NOT flatten nested blocks: an `Insert` entity references its resolved `BlockRecord` directly through `Insert.Block` (the DXF code-2 name resolves to the live `BlockRecord`, whose entities are `Block.Entities` — a `CadObjectCollection<Entity>`) and carries its own placement transform that must compose explicitly. The `Insert` placement is `InsertPoint: XYZ` (the block origin in WCS), `XScale`/`YScale`/`ZScale: double` (the per-axis scale), `Rotation: double` (radians about `Normal`), and `Normal: XYZ` (the OCS axis); an arrayed `Insert` additionally carries `ColumnCount`/`RowCount: ushort` and `ColumnSpacing`/`RowSpacing: double`. The boundary admits ONLY the top-level `ModelSpace.Entities` closed profiles this campaign; a nested-block `Insert`-flattening arm (reading `Insert.Block.Entities`, composing the `InsertPoint`/scale/`Rotation` transform per array cell, then recursing `Admit`) is the one forward growth arm, not a phase-1 admission. `Insert.ApplyTransform` is the package-owned transform composer the flattening arm reuses rather than hand-building the OCS-to-WCS matrix.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DXF/DWG file read into the `CadDocument` model and the 2D profile entity surface the `Polygon/import#PROFILE_IMPORT` boundary tessellates into `Loop` sets
- Accept: a file path (or stream) to a DXF/DWG, the optional `NotificationEventHandler` warning sink, the `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Spline` closed-profile entities sampled through the ACadSharp-owned `CreateFromBulge`/`PolygonalVertexes` curve surface, and the `Insert` block reference whose resolved `Block.Entities` flatten through the composed `InsertPoint`/scale/`Rotation` placement transform
- Reject: writing DWG/DXF from this folder (Rhino owns the host-bound native write; this boundary is read-only profile ingress), a `CadDocument` or an ACadSharp entity type escaping the boundary into a sibling kernel signature (a foreign CAD entity crosses into the canonical `Loop` at the one boundary, never travels the interior), a hand-rolled bulge-to-arc trigonometry or a hand-rolled NURBS de Boor where the package owns `Arc.CreateFromBulge`/`PolygonalVertexes`/`Spline.PolygonalVertexes`, a hand-built OCS-to-WCS `Insert` matrix where the package owns `Insert.ApplyTransform`, and a reader exception escaping the boundary unlowered (the `Read` throw lowers to `GeometryFault.DegenerateInput` once at admission)
