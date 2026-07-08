# [RASM_API_CLIPPER2]

`Clipper2Lib` supplies 2D polygon Boolean operations, path offsetting, rectangle clipping, Minkowski sum/difference, point-in-polygon testing, path simplification, and coordinate-conversion utilities — cross-cutting planar-path substrate the strata share at the float production plane: `Rasm.Fabrication` drives the toolpath, nesting, and offset owners, and `Rasm.Compute` runs the circulation corridor-offset and occupant-area algebra. The public `Triangulate`/`TriangulateResult` surface is author-flagged buggy and stays out of every consumer. The primary entry points are the static `Clipper` facade, the stateful `Clipper64`/`ClipperD` engines for subject/clip workflow, and `ClipperOffset` for inward/outward inflation with configurable join and end styles. Coordinate geometry resolves through `int64`-native `Point64`/`Path64`/`Rect64` and `double`-native `PointD`/`PathD`/`RectD`; `ClipperD` scales doubles to int64 internally at caller-specified decimal precision. The kernel `Rasm` stays the exact-geometry owner — Clipper2 is the float production-plane algebra, never a second exact rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Clipper2`
- package: `Clipper2`
- license: `BSL-1.0` (Boost-1.0)
- assembly: `Clipper2Lib`
- namespace: `Clipper2Lib`
- asset: pure-managed AnyCPU IL, single-target `netstandard2.0` (no native asset, no RID burden, ALC-safe); the `net10.0` consumer binds the lone `lib/netstandard2.0/Clipper2Lib.dll`
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate primitives
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------- |:--------------- |:--------------------------------- |
| [01] | `Point64` | int64 point | integer coordinate geometry |
| [02] | `PointD` | double point | floating-point coordinate geometry |
| [03] | `Rect64` | int64 rectangle | bounding box and rect-clip input |
| [04] | `RectD` | double rectangle | bounding box in double coordinates |

[PUBLIC_TYPE_SCOPE]: path collections
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------- |:-------------- |:----------------------------------- |
| [01] | `Path64` | `List<Point64>` | single polygon or open path (int64) |
| [02] | `Paths64` | `List<Path64>` | polygon set (int64) |
| [03] | `PathD` | `List<PointD>` | single polygon or open path (double) |
| [04] | `PathsD` | `List<PathD>` | polygon set (double) |

[PUBLIC_TYPE_SCOPE]: clipping enumerations
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:--------- |:------------ |:------------------------------ |
| [01] | `ClipType` | clip enum | Boolean operation selector |
| [02] | `FillRule` | fill enum | winding / even-odd fill policy |
| [03] | `PathType` | path enum | subject vs clip role assignment |
| [04] | `JoinType` | join enum | offset corner join style |
| [05] | `EndType` | end enum | open-path end cap style |

[PUBLIC_TYPE_SCOPE]: clipping engines
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------------- |:-------------- |:------------------------------------- |
| [01] | `Clipper` | static facade | one-shot Boolean and offset operations |
| [02] | `Clipper64` | stateful engine | int64 subject/clip workflow |
| [03] | `ClipperD` | stateful engine | double subject/clip workflow |
| [04] | `ClipperOffset` | offset engine | polygon and open-path inflation |
| [05] | `ClipperBase` | abstract base | shared input API (`AddSubject`/`AddOpenSubject`/`AddClip`/`Clear`/`GetBounds`) and `PreserveCollinear`/`ReverseSolution` flags both engines inherit |
| [06] | `Minkowski` | static facade | precision-bearing Minkowski sum/diff |
| [07] | `ReuseableDataContainer64` | int64 reuse cache | precomputed subject/clip vertex+local-minima structure shared across many `Clipper64.Execute` clips |

[PUBLIC_TYPE_SCOPE]: poly-tree result carriers
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------- |:----------------- |:---------------------------------- |
| [01] | `PolyTree64` | tree root (int64) | nested polygon result (outer/holes) |
| [02] | `PolyPath64` | tree node (int64) | one polygon node with child holes |
| [03] | `PolyTreeD` | tree root (double) | nested polygon result (double) |
| [04] | `PolyPathD` | tree node (double) | one polygon node with child holes |
| [05] | `PolyPathBase` | abstract base | shared node enumeration and depth |

[PUBLIC_TYPE_SCOPE]: result and error types
- rail: geometry

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------- |:------------ |:-------------------------------- |
| [01] | `PointInPolygonResult` | result enum | inside / on-boundary / outside |
| [02] | `TriangulateResult` | result enum | triangulation success / failure |
| [03] | `ClipperLibException` | exception | domain error from clipping engine |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static Boolean operations — `Clipper` facade
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:-------------------------------------------------- |:--------------- |:---------------------------- |
| [01] | `Intersect(Paths64, Paths64, FillRule)` | one-shot clip | int64 intersection |
| [02] | `Intersect(PathsD, PathsD, FillRule, precision)` | one-shot clip | double intersection |
| [03] | `Union(Paths64, FillRule)` | one-shot clip | int64 self-union |
| [04] | `Union(Paths64, Paths64, FillRule)` | one-shot clip | int64 union |
| [05] | `Union(PathsD, FillRule)` | one-shot clip | double self-union |
| [06] | `Union(PathsD, PathsD, FillRule, precision)` | one-shot clip | double union |
| [07] | `Difference(Paths64, Paths64, FillRule)` | one-shot clip | int64 difference |
| [08] | `Difference(PathsD, PathsD, FillRule, precision)` | one-shot clip | double difference |
| [09] | `Xor(Paths64, Paths64, FillRule)` | one-shot clip | int64 exclusive-or |
| [10] | `Xor(PathsD, PathsD, FillRule, precision)` | one-shot clip | double exclusive-or |
| [11] | `BooleanOp(ClipType, Paths64?, Paths64?, FillRule)` | one-shot clip | int64 generic Boolean |
| [12] | `BooleanOp(ClipType, PathsD, PathsD?, FillRule, p)` | one-shot clip | double generic Boolean |
| [13] | `BooleanOp(.. PolyTree64, FillRule)` | tree-output clip | int64 Boolean into PolyTree64 |
| [14] | `BooleanOp(.. PolyTreeD, FillRule, precision)` | tree-output clip | double Boolean into PolyTreeD |

[ENTRYPOINT_SCOPE]: static offset and geometry — `Clipper` facade
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:-------------------------------------------------------- |:------------- |:-------------------------------------------------- |
| [01] | `InflatePaths(Paths64, delta, JoinType, EndType, …)` | offset | int64 polygon/open-path inflation |
| [02] | `InflatePaths(PathsD, delta, JoinType, EndType, …)` | offset | double polygon/open-path inflation |
| [03] | `RectClip(Rect64, Paths64)` | rect clip | int64 paths clipped to rect |
| [04] | `RectClip(Rect64, Path64)` | rect clip | int64 single path clipped to rect |
| [05] | `RectClip(RectD, PathsD, precision)` | rect clip | double paths clipped to rect |
| [06] | `RectClipLines(Rect64, Paths64)` | rect clip | int64 open paths clipped to rect |
| [07] | `RectClipLines(RectD, PathsD, precision)` | rect clip | double open paths clipped to rect |
| [08] | `MinkowskiSum(Path64, Path64, isClosed)` | morphology | int64 Minkowski sum |
| [09] | `MinkowskiSum(PathD, PathD, isClosed)` | morphology | double Minkowski sum (precision fixed at 2) |
| [10] | `MinkowskiDiff(Path64, Path64, isClosed)` | morphology | int64 Minkowski difference |
| [11] | `MinkowskiDiff(PathD, PathD, isClosed)` | morphology | double Minkowski difference (precision fixed at 2) |
| [12] | `Ellipse(Point64, radiusX, radiusY, steps)` | geometry | int64 ellipse/circle path |
| [13] | `Ellipse(PointD, radiusX, radiusY, steps)` | geometry | double ellipse/circle path |
| [14] | `Triangulate(Paths64, out Paths64, useDelaunay)` | triangulation | int64 Delaunay triangulation `[!]` buggy, kept out |
| [15] | `Triangulate(PathsD, decPlaces, out PathsD, useDelaunay)` | triangulation | double Delaunay triangulation `[!]` buggy, kept out |

[ENTRYPOINT_SCOPE]: precision-bearing Minkowski — `Minkowski` facade
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------------------------------------------- |:------------- |:---------------------------------------------- |
| [01] | `Sum(Path64 pattern, Path64 path, isClosed)` | morphology | int64 Minkowski sum |
| [02] | `Sum(PathD pattern, PathD path, isClosed, decimalPlaces)` | morphology | double Minkowski sum at caller precision |
| [03] | `Diff(Path64 pattern, Path64 path, isClosed)` | morphology | int64 Minkowski difference |
| [04] | `Diff(PathD pattern, PathD path, isClosed, decimalPlaces)` | morphology | double Minkowski difference at caller precision |

[ENTRYPOINT_SCOPE]: static path measurement and analysis — `Clipper` facade
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:------------- |:------------------------------- |
| [01] | `Area(Path64)` / `Area(Paths64)` | measurement | signed area (positive = CCW) |
| [02] | `Area(PathD)` / `Area(PathsD)` | measurement | signed area (double) |
| [03] | `IsPositive(Path64)` / `IsPositive(PathD)` | orientation | CCW winding test |
| [04] | `GetBounds(Path64)` / `GetBounds(Paths64)` | bounds | int64 axis-aligned bounding box |
| [05] | `GetBounds(PathD)` / `GetBounds(PathsD)` | bounds | double axis-aligned bounding box |
| [06] | `PointInPolygon(Point64, Path64)` | containment | inside / on / outside test |
| [07] | `PointInPolygon(PointD, PathD, precision)` | containment | double containment test |
| [08] | `RamerDouglasPeucker(Path64, epsilon)` | simplification | int64 polyline simplification |
| [09] | `RamerDouglasPeucker(PathD, epsilon)` | simplification | double polyline simplification |
| [10] | `SimplifyPath(Path64, epsilon, isClosedPath)` | simplification | int64 collinear simplification |
| [11] | `SimplifyPath(PathD, epsilon, isClosedPath)` | simplification | double collinear simplification |
| [12] | `SimplifyPaths(Paths64, epsilon, isClosedPath)` | simplification | int64 path-set simplification |
| [13] | `SimplifyPaths(PathsD, epsilon, isClosedPath)` | simplification | double path-set simplification |
| [14] | `TrimCollinear(Path64, isOpen)` | cleanup | int64 collinear point removal |
| [15] | `TrimCollinear(PathD, precision, isOpen)` | cleanup | double collinear point removal |
| [16] | `StripDuplicates(Path64, isClosedPath)` | cleanup | consecutive duplicate removal |

[ENTRYPOINT_SCOPE]: static path conversion and transform — `Clipper` facade
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------ |:------------- |:---------------------------------- |
| [01] | `ScalePath(Path64, scale)` / `ScalePaths(…)` | conversion | int64 coordinate scaling |
| [02] | `ScalePath64(PathD, scale)` / `ScalePaths64(…)` | conversion | double-to-int64 path conversion |
| [03] | `ScalePathD(Path64, scale)` / `ScalePathsD(…)` | conversion | int64-to-double path conversion |
| [04] | `TranslatePath(Path64, dx, dy)` | transform | integer path translation |
| [05] | `TranslatePath(PathD, dx, dy)` | transform | double path translation |
| [06] | `ReversePath(Path64)` / `ReversePath(PathD)` | transform | winding reversal |
| [07] | `MakePath(int[])` / `MakePath(long[])` / `MakePath(double[])` | factory | interleaved-array path construction |
| [08] | `PolyTreeToPaths64(PolyTree64)` | tree flatten | nested result to flat Paths64 |
| [09] | `PolyTreeToPathsD(PolyTreeD)` | tree flatten | nested result to flat PathsD |

[ENTRYPOINT_SCOPE]: stateful clipping — `Clipper64` engine
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:---------------------------------------------------------- |:------------- |:--------------------------- |
| [01] | `AddSubject(Paths64)` | input | closed subject paths |
| [02] | `AddOpenSubject(Paths64)` | input | open subject paths |
| [03] | `AddClip(Paths64)` | input | clip paths |
| [04] | `AddReuseableData(ReuseableDataContainer64)` | input | inject a precomputed subject/clip structure (reuse across many clips) |
| [05] | `Execute(ClipType, FillRule, Paths64 closed)` | operation | closed-only int64 result |
| [06] | `Execute(ClipType, FillRule, Paths64 closed, Paths64 open)` | operation | closed + open int64 result |
| [07] | `Execute(ClipType, FillRule, PolyTree64)` | operation | tree-structured int64 result |
| [08] | `Execute(ClipType, FillRule, PolyTree64, Paths64 open)` | operation | tree + open int64 result |
| [09] | `PreserveCollinear` / `ReverseSolution` | property | engine behavior flags (inherited from `ClipperBase`) |

[ENTRYPOINT_SCOPE]: stateful clipping — `ClipperD` engine
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:-------------------------------------------------------- |:------------- |:---------------------------- |
| [01] | `ClipperD(roundingDecimalPrecision = 2)` | constructor | precision 1–8 decimal places |
| [02] | `AddSubject(PathD)` / `AddSubject(PathsD)` | input | closed subject paths (double) |
| [03] | `AddOpenSubject(PathD)` / `AddOpenSubject(PathsD)` | input | open subject paths (double) |
| [04] | `AddClip(PathD)` / `AddClip(PathsD)` | input | clip paths (double) |
| [05] | `Execute(ClipType, FillRule, PathsD closed)` | operation | closed-only double result |
| [06] | `Execute(ClipType, FillRule, PathsD closed, PathsD open)` | operation | closed + open double result |
| [07] | `Execute(ClipType, FillRule, PolyTreeD)` | operation | tree-structured double result |
| [08] | `Execute(ClipType, FillRule, PolyTreeD, PathsD open)` | operation | tree + open double result |

[ENTRYPOINT_SCOPE]: reusable subject/clip precompute — `ReuseableDataContainer64`
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------------------------------- |:------------- |:------------------------------------------------------------- |
| [01] | `ReuseableDataContainer64()` | constructor | empty reusable input container (int64 rail only — no `ReuseableDataContainerD`) |
| [02] | `AddPaths(Paths64, PathType pt, bool isOpen)` | input | precompute the vertex/local-minima structure for a subject or clip path set ONCE |
| [03] | `Clear()` | reset | discard the precomputed structure |
| [04] | `Clipper64.AddReuseableData(container)` | consume | feed the precomputed structure into a fresh `Clipper64` so a recurring subject/clip skips re-tessellation per `Execute` |

[ENTRYPOINT_SCOPE]: offset engine — `ClipperOffset`
- rail: geometry

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------ |:------------- |:-------------------------------------- |
| [01] | `ClipperOffset(miterLimit, arcTolerance, …)` | constructor | offset engine with join and arc options |
| [02] | `AddPath(Path64, JoinType, EndType)` | input | single path registration |
| [03] | `AddPaths(Paths64, JoinType, EndType)` | input | multi-path registration |
| [04] | `Execute(double delta, Paths64 solution)` | operation | inflate to flat result |
| [05] | `Execute(double delta, PolyTree64 solutionTree)` | operation | inflate to tree result |
| [06] | `Execute(DeltaCallback64 callback, Paths64 solution)` | operation | per-vertex variable delta inflate |
| [07] | `Clear()` | reset | discard registered path groups |
| [08] | `ArcTolerance` / `MiterLimit` | property | arc and miter precision tuning |
| [09] | `MergeGroups` / `PreserveCollinear` / `ReverseSolution` | property | merge and winding behavior flags |
| [10] | `DeltaCallback` (delegate `DeltaCallback64`) | property | per-point variable offset callback |

## [04]-[IMPLEMENTATION_LAW]

[COORDINATE_TOPOLOGY]:
- `int64` path: `Point64.X` and `Point64.Y` are `long`; coordinates must be pre-scaled before clipping when originating from floating-point geometry
- `double` path: `PointD.x` / `PointD.y` are `double`; `ClipperD` internally scales by `10^precision` before executing and descales the result
- `ClipperD` precision range: integers `−8` through `8`; values outside this range raise `ClipperLibException`
- `FillRule.EvenOdd`: alternating fill by crossing count; `NonZero`: non-zero winding; `Positive`: positive-winding areas only; `Negative`: negative-winding areas only
- `ClipType.NoClip`: returns subject unchanged; `Intersection`, `Union`, `Difference`, `Xor` are the four Boolean operations
- `JoinType.Miter`: sharp corners up to `MiterLimit`; `Square`: flat angled cap; `Bevel`: flat cut across; `Round`: arc
- `EndType.Polygon`: closed path offset; `Joined`: open path treated as closed join; `Butt`: flat perpendicular end; `Square`: extended square end; `Round`: semicircular end
- `Rect64` layout: `left`, `top`, `right`, `bottom` as `long`; `IsEmpty` returns true when `bottom <= top` or `right <= left`

[STACK_INTEGRATION]:
- `Rasm.Fabrication` (toolpath/nest/offset): the `ClipperOffset` inflation drives kerf/tool-radius offset, the Boolean facade the sheet-layout clip, and `Minkowski.Sum`/`Diff` the no-fit-polygon nesting kernel; `ReuseableDataContainer64` precomputes one recurring part-set's scanbeam structure ONCE and folds the per-position `Clipper64.Execute` over it (the placement-scan reuse rail), routing NFP construction through the deeper `Minkowski.Sum`/`Diff(PathD, PathD, isClosed, decimalPlaces)` facade at the owner's precision, never the precision-dropping shorthand.
- `Rasm.Compute` circulation (`[V12]`): the `Analysis/circulation` egress runner composes `InflatePaths`/`ClipperOffset` for corridor-width offset and clearance envelopes, the Boolean facade for occupant-area intersection, and `Area`/`PointInPolygon` for occupant-load computation — the planar-path side of the egress discipline, the flow/topology algebra being `QuikGraph`/`Google.OrTools.Graph`'s.

[LOCAL_ADMISSION]:
- Use `Clipper` static methods for one-shot operations; prefer `Clipper64`/`ClipperD` stateful engines only when re-using subject/clip state across multiple `Execute` calls.
- Scale floating-point geometry to int64 via `Clipper.ScalePaths64(paths, scale)` where `scale = Math.Pow(10, precision)` before passing to `Clipper64`; descale results with `Clipper.ScalePathsD`.
- `ClipperOffset.Execute` writes directly into the caller-supplied `Paths64`/`PolyTree64`; clear or allocate a fresh container before each call.
- `PolyTree64`/`PolyTreeD` carry outer/hole nesting; flatten via `Clipper.PolyTreeToPaths64`/`PolyTreeToPathsD` when nesting structure is not required.
- `DeltaCallback64` receives the current path, path normals, current and previous vertex indices; return the signed offset delta for that vertex.
- `Clipper.Triangulate`/`TriangulateResult` are public surface but stay OUT of every consumer: the triangulation module is author-flagged buggy (open infinite-loop defects in the internal `Delaunay` kernel); route triangulation through the kernel triangulation owner instead.

[RAIL_LAW]:
- Package: `Clipper2` (Clipper2Lib, BSL-1.0, pure-managed netstandard2.0)
- Owns: 2D polygon Boolean operations, path offsetting, rect clipping, Minkowski operations, and path utilities (the buggy `Triangulate` surface excluded)
- Accept: `Path64`/`Paths64` for int64 geometry; `PathD`/`PathsD` with explicit precision for double geometry — the fabrication toolpath/nest/offset plane and the circulation corridor-offset/occupant-area plane
- Reject: hand-rolled polygon clipping, manual coordinate scaling without `Clipper.ScalePaths64`/`ScalePathsD`, direct `ClipperBase` internals, and the EXACT-geometry concern the kernel `Rasm` owns (Clipper2 is the float production plane, never a second exact rail)
