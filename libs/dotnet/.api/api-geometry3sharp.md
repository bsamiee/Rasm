# [RASM_API_GEOMETRY3SHARP]

`geometry3Sharp` serves two disjoint surfaces of one pure-managed distribution. The mesh-text leg owns OBJ/STL/OFF/G3 triangle-mesh decode: the `StandardMeshReader` extension-dispatching facade over per-format `MeshFormatReader` handlers, the `DMesh3` refcounted indexed-mesh carrier with per-vertex normal/color/UV channels, and the `IMeshBuilder`/`DMesh3Builder` accumulator the readers drive — the managed-import leg of the `Rasm.Bim` `MeshText` interchange codec (PLY and 3MF carry no reader here). The biarc leg owns `g3.BiArcFit2`, the line-sourced biarc fitter folding a line-only chord run into two `G1`-continuous `Arc2d`/`Segment2d` spans that emit as `Rasm.Fabrication` `G2`/`G3` arc moves with no downstream refit. Neither leg reads the other's types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp` (Boost-1.0)
- assembly: `geometry3Sharp`
- namespace: `g3`
- asset: pure-managed AnyCPU IL, multi-target `netstandard2.0`/`net45` (no native asset, no RID burden), ALC-safe, zero package dependencies; the `net10.0` consumer binds `lib/netstandard2.0/geometry3Sharp.dll`
- rail: geometry — mesh-text decode and line-sourced biarc fit

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facade and dispatch

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `StandardMeshReader`   | class         | extension-dispatched read into an `IMeshBuilder`; OBJ/STL/OFF/G3 by default   |
|  [02]   | `MeshFormatReader`     | interface     | per-format handler: `SupportedExtensions` plus `ReadFile` over file or stream |
|  [03]   | `OBJFormatReader`      | class         | OBJ reader; `SupportedExtensions = ["obj"]`                                   |
|  [04]   | `STLFormatReader`      | class         | binary and ASCII STL reader; `["stl"]`                                        |
|  [05]   | `OFFFormatReader`      | class         | OFF reader; `["off"]`                                                         |
|  [06]   | `BinaryG3FormatReader` | class         | native binary-G3 reader; `["g3mesh"]`                                         |

[PUBLIC_TYPE_SCOPE]: mesh model and builder

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                                                                   |
| :-----: | :---------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `DMesh3`          | class          | indexed triangle mesh; refcounted vertex/triangle/edge pools, vtx normals      |
|  [02]   | `DMesh3Builder`   | class          | `IMeshBuilder` accumulator owning `List<DMesh3> Meshes` and material rows      |
|  [03]   | `IMeshBuilder`    | interface      | append-mesh/vertex/triangle/material surface the format readers drive          |
|  [04]   | `NewVertexInfo`   | mutable struct | per-vertex bundle: position, normal, color, UV with presence flags             |
|  [05]   | `GenericMaterial` | abstract class | `name`/`id` base; the OBJ MTL parse builds `OBJMaterial` under `ReadMaterials` |

[PUBLIC_TYPE_SCOPE]: vector and index value types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                                                         |
| :-----: | :--------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Vector3d` | mutable struct | `double x`, `y`, `z`; the canonical position type                                    |
|  [02]   | `Vector3f` | mutable struct | `float x`, `y`, `z`; normal/color type; `AxisX`/`AxisY`/`AxisZ`/`Zero`/`One` anchors |
|  [03]   | `Vector2f` | mutable struct | `float x`, `y`; the UV type                                                          |
|  [04]   | `Index3i`  | mutable struct | `int a`, `b`, `c`; a triangle's three vertex indices                                 |

- `Vector3f.AxisZ`: default up-normal a projection substitutes when a mesh carries no normal channel.

[PUBLIC_TYPE_SCOPE]: read result and options

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `IOReadResult` | struct        | `IOCode code`, `string message`; `IOReadResult.Ok` the success value            |
|  [02]   | `IOCode`       | enum          | result status; `Ok=0` plus parse/format/access error rows                       |
|  [03]   | `ReadOptions`  | class         | `ReadMaterials` flag, `CustomFlags`; `ReadOptions.Defaults` reads geometry only |

[IO_CODE_CASES]: `IOCode`
- `Ok=0` `FileAccessError=1` `UnknownFormatError=2` `FormatNotSupportedError=3` `InvalidFilenameError=4`
- `FileParsingError=100` `GarbageDataError=101` `GenericReaderError=102` `GenericReaderWarning=103` `WriterError=200` `ComputingInWorkerThread=1000`

[PUBLIC_TYPE_SCOPE]: biarc fit and curve primitives

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :---------- | :------------ | :----------------------------- |
|  [01]   | `BiArcFit2` | fitter class  | point/tangent biarc fitting    |
|  [02]   | `Arc2d`     | curve class   | circular span carrier          |
|  [03]   | `Segment2d` | curve struct  | degenerate straight span       |
|  [04]   | `Vector2d`  | vector struct | point and unit tangent carrier |

[PUBLIC_TYPE_SCOPE]: `BiArcFit2` members

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d)`                        | constructor   | endpoint/tangent fit  |
|  [02]   | `BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d, double)`                | constructor   | explicit fit distance |
|  [03]   | `Arc2d Arc1` / `Arc2`                                                      | field         | fitted arc spans      |
|  [04]   | `bool Arc1IsSegment` / `Arc2IsSegment`                                     | field         | degenerate span flags |
|  [05]   | `Segment2d Segment1` / `Segment2`                                          | field         | fitted segment spans  |
|  [06]   | `Vector2d Point1` / `Point2`                                               | field         | input endpoints       |
|  [07]   | `Vector2d Tangent1` / `Tangent2`                                           | field         | input tangents        |
|  [08]   | `double FitD1` / `FitD2`                                                   | field         | solved fit distances  |
|  [09]   | `double Epsilon`                                                           | field         | fit tolerance         |
|  [10]   | `List<IParametricCurve2d> Curves` / `IParametricCurve2d Curve1` / `Curve2` | property      | polymorphic span list |
|  [11]   | `Distance(Vector2d) -> double` / `NearestPoint(Vector2d) -> Vector2d`      | method        | fit-error query       |

[PUBLIC_TYPE_SCOPE]: `Arc2d`, `Segment2d`, and `Vector2d` members

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `Arc2d(Vector2d, double, double, double)`                                         | constructor   | center/radius arc     |
|  [02]   | `Arc2d(Vector2d, Vector2d, Vector2d)`                                             | constructor   | center/endpoints arc  |
|  [03]   | `Vector2d Arc2d.Center` / `double Radius`                                         | field         | center and radius     |
|  [04]   | `double Arc2d.AngleStartDeg` / `AngleEndDeg`                                      | field         | sweep angles          |
|  [05]   | `bool Arc2d.IsReversed`                                                           | field         | direction selection   |
|  [06]   | `Arc2d.SampleT(double) -> Vector2d`                                               | method        | parameter sample      |
|  [07]   | `Vector2d Arc2d.P0` / `P1`                                                        | property      | endpoints             |
|  [08]   | `double Arc2d.ArcLength`                                                          | property      | arc length            |
|  [09]   | `Arc2d.SampleArcLength(double)` / `Segment2d.SampleArcLength(double) -> Vector2d` | method        | distance sample       |
|  [10]   | `Segment2d(Vector2d, Vector2d)`                                                   | constructor   | endpoint segment      |
|  [11]   | `Vector2d Segment2d.P0` / `P1`                                                    | property      | segment endpoints     |
|  [12]   | `Vector2d Segment2d.Center` / `Direction` / `double Extent`                       | field         | center-direction span |
|  [13]   | `double Segment2d.Length`                                                         | property      | segment length        |
|  [14]   | `Vector2d(double, double)`                                                        | constructor   | point/tangent builder |
|  [15]   | `double Vector2d.x` / `y`                                                         | field         | component fields      |
|  [16]   | `Vector2d Vector2d.Normalized`                                                    | property      | unit-copy tangent     |
|  [17]   | `Vector2d.Normalize(double) -> double`                                            | method        | in-place unit scale   |
|  [18]   | `double Vector2d.Length` / `LengthSquared`                                        | property      | magnitude             |
|  [19]   | `Vector2d.Dot(Vector2d)` / `AngleD(Vector2d) -> double`                           | method        | tangent comparison    |
|  [20]   | `Vector2d.Distance(Vector2d)` / `DistanceSquared(Vector2d) -> double`             | method        | point deviation       |
|  [21]   | `Vector2d Vector2d.Perp` / `UnitPerp`                                             | property      | perpendicular vectors |
|  [22]   | `static Vector2d Vector2d.Zero` / `AxisX` / `AxisY`                               | field         | origin and axes       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `StandardMeshReader` — read and dispatch

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `StandardMeshReader(bool)`                            | ctor     | registers OBJ/STL/OFF/G3 handlers by default         |
|  [02]   | `MeshBuilder`                                         | property | the `IMeshBuilder` sink; defaults to `DMesh3Builder` |
|  [03]   | `Read(Stream, string, ReadOptions)`                   | instance | in-memory read; returns `IOReadResult`               |
|  [04]   | `Read(string, ReadOptions)`                           | instance | path-dispatched read; returns `IOReadResult`         |
|  [05]   | `SupportsFormat(string)`                              | instance | `true` when a registered handler claims the ext      |
|  [06]   | `AddFormatHandler(MeshFormatReader)`                  | instance | adds a handler; throws on duplicate extension        |
|  [07]   | `ReadMesh(Stream, string)`                            | static   | first `DMesh3` or `null` on non-`Ok`                 |
|  [08]   | `ReadMesh(string)`                                    | static   | path overload; first `DMesh3` or `null`              |
|  [09]   | `ReadFile(Stream, string, ReadOptions, IMeshBuilder)` | static   | reads into a caller-supplied builder                 |
|  [10]   | `ReadFile(string, ReadOptions, IMeshBuilder)`         | static   | path overload into a supplied builder                |
|  [11]   | `warningEvent` (`ParsingMessagesHandler`)             | event    | per-parse warning callback                           |

[ENTRYPOINT_SCOPE]: `MeshFormatReader` — per-format handler

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SupportedExtensions`                                                 | property | `List<string>` of bare extensions, no dot |
|  [02]   | `ReadFile(Stream, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | instance | parses the stream into the builder        |
|  [03]   | `ReadFile(string, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | instance | parses the file into the builder          |

[ENTRYPOINT_SCOPE]: `DMesh3` — mesh read accessors

| [INDEX] | [SURFACE]              | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `VertexCount`          | property | live refcounted vertex count                                    |
|  [02]   | `TriangleCount`        | property | live triangle count                                             |
|  [03]   | `HasVertexNormals`     | property | `true` when the normal channel is allocated (`normals != null`) |
|  [04]   | `GetVertex(int)`       | instance | `Vector3d` position                                             |
|  [05]   | `GetVertexf(int)`      | instance | `Vector3f` position                                             |
|  [06]   | `GetVertexNormal(int)` | instance | `Vector3f` normal                                               |
|  [07]   | `GetTriangle(int)`     | instance | `Index3i` of three vertex indices                               |
|  [08]   | `GetTriNormal(int)`    | instance | `Vector3d` face normal                                          |
|  [09]   | `VertexIndices()`      | instance | `IEnumerable<int>` over live vertex ids                         |
|  [10]   | `TriangleIndices()`    | instance | `IEnumerable<int>` over live triangle ids                       |
|  [11]   | `IsVertex(int)`        | instance | `true` when the slot is a live vertex                           |
|  [12]   | `IsTriangle(int)`      | instance | `true` when the slot is a live triangle                         |

[ENTRYPOINT_SCOPE]: `DMesh3Builder` — accumulation

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `Meshes`                                | property | `List<DMesh3>` the readers populate                               |
|  [02]   | `AppendNewMesh(bool, bool, bool, bool)` | instance | normals/colors/UVs/face-group presence flags                      |
|  [03]   | `AppendVertex(double, double, double)`  | instance | appends to the active mesh; returns vertex id                     |
|  [04]   | `AppendTriangle(int, int, int, int)`    | instance | appends a grouped triangle; returns triangle id                   |
|  [05]   | `Materials`                             | property | `List<GenericMaterial>` from the MTL parse                        |
|  [06]   | `SupportsMetaData`                      | property | `true`; `AppendMetaData(string, object)` stores per-mesh metadata |

[ENTRYPOINT_SCOPE]: line-sourced biarc fit

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------ | :------- | :----------------- |
|  [01]   | `new Vector2d(double, double).Normalized`                     | ctor     | unit tangent input |
|  [02]   | `new BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d)`       | ctor     | biarc solve        |
|  [03]   | `BiArcFit2.Distance(Vector2d) -> double`                      | instance | tolerance gate     |
|  [04]   | `Arc2d.SampleT(double)` / `Arc2d.Center` / `Arc2d.IsReversed` | instance | `G2`/`G3` mapping  |
|  [05]   | `Segment2d.P1`                                                | property | `G1` fallback      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `StandardMeshReader` registers `OBJFormatReader`, `STLFormatReader`, `OFFFormatReader`, and `BinaryG3FormatReader` by default, dispatching on case-insensitive bare extension through each handler's `SupportedExtensions`; `AddFormatHandler` throws on a duplicate extension.
- `STLFormatReader` reads binary and ASCII STL; `OBJFormatReader` materializes MTL rows as `OBJMaterial : GenericMaterial` into `DMesh3Builder.Materials` only under `ReadOptions.ReadMaterials`.
- `DMesh3` pools are refcounted and sparse, so `VertexCount`/`TriangleCount` are live counts and projection iterates `VertexIndices()`/`TriangleIndices()`, never a dense `0..Count` loop; `HasVertexNormals` reads `normals != null` on the live carrier while the `IMesh` view returns constant `false`.
- `BiArcFit2(p1, t1, p2, t2)` fits two spans meeting at a `G1`-continuous junction; `Arc1IsSegment` and `Arc2IsSegment` select the `Segment2d` fallback per span, exposed uniformly through `Curves` / `Curve1` / `Curve2` as `IParametricCurve2d`.
- Input tangents are unit-length via `Vector2d.Normalized`; a zero tangent routes to the straight-span path before fitting.
- Arc endpoints read through `SampleT(0.0)` and `SampleT(1.0)`; the G-code `I`/`J` offset is `Center - SampleT(0.0)`, `IsReversed == true` maps to clockwise `G2`, `false` to counter-clockwise `G3`, and `AngleEndDeg - AngleStartDeg` (signed by `IsReversed`) is the included-angle source feeding `Move.Circular.SweepRadians`.
- `SampleArcLength(a)` samples by distance along the fitted span, so tab and micro-bridge spacing follows arc length rather than parameter position.

[STACKING]:
- `Openize.Drako`(`Rasm.Bim/.api/api-openize-drako.md`): the boundary's projected vertex/normal/index triple loads `PointAttribute.Wrap(AttributeType, Span<Vector3>)` as the `KHR_draco_mesh_compression` encode intake, with no second projection.
- `Alimer.Bindings.MeshOptimizer`(`api-alimer-meshoptimizer.md`): the same triple passes as `ReadOnlySpan<TVertex>` into the meshopt remap/optimize and `EncodeVertexBuffer<TVertex>` compression leg.
- `SharpGLTF`(`api-sharpgltf.md`): a decoded `DMesh3` re-emits through the glTF schema and toolkit builders as the export counterpart to the decode leg.
- `CavalierContours`(`Rasm.Fabrication/.api/api-cavaliercontours.md`): its `PlineVertex.Bulge` maps directly to a `G2`/`G3` move, so a bulge-carrying offset, lead-arc, or adaptive-spiral loop emits arcs without a refit; `g3.BiArcFit2` fires only on a genuinely line-sourced chord run — a line-only path densified through `ArcsToApproxLines` then clipped, or a kernel mesh-section chain — and stays the sole biarc owner for that residual.
- `OcctNet.Wrapper`(`Rasm.Fabrication/.api/api-occtnet-wrapper.md`): the `OcctMesh` triangle soup crosses through `Vertices` and `TriangleIndices` to the mesh owner — only the admitted mesh crosses, never the live `OcctShape` handle.
- Bim consumer anchor: `StandardMeshReader.Read(Stream, string, ReadOptions.Defaults)` folds one `MeshText` `InterchangeCodec` row keyed by bare extension into a `DMesh3Builder`, then the import boundary projects `DMesh3` to the canonical triangle-soup; `IOReadResult.code != IOCode.Ok` maps to the owner's codec refusal, while `warningEvent` callbacks append provider diagnostics alongside success and never become terminal faults.
- Fabrication consumer anchor: `Posting/program` and `Toolpath/motion` drive arc emit from the fitted span — `Arc2d.Center - SampleT(0.0)` is the `Move.ArcCenter` `I`/`J` offset, `IsReversed` selects `G2` vs `G3`, `Arc2d.AngleEndDeg - AngleStartDeg` signed by `IsReversed` supplies the required `Move.Circular.SweepRadians`, `Segment2d.P1` the `G1` fallback, `SampleArcLength` the tab and lead-point spacing.

[LOCAL_ADMISSION]:
- The mesh-text leg enters at the `Rasm.Bim` import boundary alone: read through `StandardMeshReader.Read(stream, extension, ReadOptions.Defaults)` into a `DMesh3Builder`, gating on `IOReadResult.code == IOCode.Ok`; `ReadMesh(stream, extension)` returns the first `DMesh3` or `null` for a one-shot, and projection iterates `VertexIndices()`/`TriangleIndices()`. `ReadInvariantCulture` defaults `true`, so the float parse stays locale-independent.
- The biarc leg enters at the `Rasm.Fabrication` toolpath rail alone: feed `BiArcFit2` a `Vector2d` point/tangent pair with tangents pre-normalized through `Normalized` — the two-arg constructor solves the symmetric fit, the explicit-`d1` constructor pins the first arc distance — and gate emission with `BiArcFit2.Distance(p)` against `BiarcPolicy.FitTolerance`; an over-tolerance fit falls back to chorded `G1` output. Reach `g3.BiArcFit2` only for a line-sourced kernel mesh-section chain; an arc-native loop reads `Geometry2D/arcs` through `CavalierContours` directly.

[RAIL_LAW]:
- Package: `geometry3Sharp`
- Owns: pure-managed OBJ/STL/OFF/G3 triangle-mesh-text decode into the `DMesh3` carrier, and line-sourced biarc fitting — two `G1`-continuous `Arc2d`/`Segment2d` spans from a point/tangent pair, with `SampleT`/`SampleArcLength` arc reads and `Distance` fit-error query
- Accept: `Stream` or file input through `StandardMeshReader.Read`/`ReadMesh` keyed by a bare-extension discriminant driving an `IMeshBuilder` sink; a unit-normalized `Vector2d` point/tangent pair from a genuinely line-only chord run, the `Arc2d`/`Segment2d` frame read straight for `G2`/`G3` emit
- Reject: a hand-rolled STL/OBJ tokenizer, a per-format reader family beside the `MeshFormatReader` dispatch, the in-package writer family whose export leg the glTF rail owns, fitting a path already carrying bulge through the `CavalierContours` offset, a hand-rolled biarc solver beside `BiArcFit2`, admitting `DMeshAABBTree3`, `MeshSignedDistanceGrid`, `Remesher`, mesh-Boolean surfaces, or the `geometry4Sharp` fork into the fabrication rail
