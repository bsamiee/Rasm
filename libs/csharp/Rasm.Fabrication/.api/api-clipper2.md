# [RASM_FABRICATION_API_CLIPPER2]

`Clipper2Lib` supplies 2D polygon Boolean operations, path offsetting, rectangle clipping, Minkowski sum/difference, point-in-polygon testing, path simplification, triangulation, and coordinate-conversion utilities for the fabrication toolpath, nesting, and offset owners. The primary entry points are the static `Clipper` facade, the stateful `Clipper64` and `ClipperD` engines for subject/clip workflow, and `ClipperOffset` for inward/outward path inflation with configurable join and end styles. Coordinate geometry resolves through `int64`-native `Point64`/`Path64`/`Rect64` types and `double`-native `PointD`/`PathD`/`RectD` types; `ClipperD` scales doubles to int64 internally at caller-specified decimal precision.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Clipper2`
- package: `Clipper2`
- assembly: `Clipper2Lib`
- namespace: `Clipper2Lib`
- asset: runtime library
- rail: fabrication

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate primitives
- rail: fabrication

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]    | [CAPABILITY]                       |
| :-----: | :-------- | :--------------- | :--------------------------------- |
|   [1]   | `Point64` | int64 point      | integer coordinate geometry        |
|   [2]   | `PointD`  | double point     | floating-point coordinate geometry |
|   [3]   | `Rect64`  | int64 rectangle  | bounding box and rect-clip input   |
|   [4]   | `RectD`   | double rectangle | bounding box in double coordinates |

[PUBLIC_TYPE_SCOPE]: path collections
- rail: fabrication

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :-------- | :-------------- | :----------------------------------- |
|   [1]   | `Path64`  | `List<Point64>` | single polygon or open path (int64)  |
|   [2]   | `Paths64` | `List<Path64>`  | polygon set (int64)                  |
|   [3]   | `PathD`   | `List<PointD>`  | single polygon or open path (double) |
|   [4]   | `PathsD`  | `List<PathD>`   | polygon set (double)                 |

[PUBLIC_TYPE_SCOPE]: clipping enumerations
- rail: fabrication

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :--------- | :------------ | :------------------------------ |
|   [1]   | `ClipType` | clip enum     | Boolean operation selector      |
|   [2]   | `FillRule` | fill enum     | winding / even-odd fill policy  |
|   [3]   | `PathType` | path enum     | subject vs clip role assignment |
|   [4]   | `JoinType` | join enum     | offset corner join style        |
|   [5]   | `EndType`  | end enum      | open-path end cap style         |

[PUBLIC_TYPE_SCOPE]: clipping engines
- rail: fabrication

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                           |
| :-----: | :-------------- | :-------------- | :------------------------------------- |
|   [1]   | `Clipper`       | static facade   | one-shot Boolean and offset operations |
|   [2]   | `Clipper64`     | stateful engine | int64 subject/clip workflow            |
|   [3]   | `ClipperD`      | stateful engine | double subject/clip workflow           |
|   [4]   | `ClipperOffset` | offset engine   | polygon and open-path inflation        |
|   [5]   | `ClipperBase`   | abstract base   | shared scan-line engine state          |

[PUBLIC_TYPE_SCOPE]: poly-tree result carriers
- rail: fabrication

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]      | [CAPABILITY]                        |
| :-----: | :------------- | :----------------- | :---------------------------------- |
|   [1]   | `PolyTree64`   | tree root (int64)  | nested polygon result (outer/holes) |
|   [2]   | `PolyPath64`   | tree node (int64)  | one polygon node with child holes   |
|   [3]   | `PolyTreeD`    | tree root (double) | nested polygon result (double)      |
|   [4]   | `PolyPathD`    | tree node (double) | one polygon node with child holes   |
|   [5]   | `PolyPathBase` | abstract base      | shared node enumeration and depth   |

[PUBLIC_TYPE_SCOPE]: result and error types
- rail: fabrication

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :--------------------- | :------------ | :-------------------------------- |
|   [1]   | `PointInPolygonResult` | result enum   | inside / on-boundary / outside    |
|   [2]   | `TriangulateResult`    | result enum   | triangulation success / failure   |
|   [3]   | `ClipperLibException`  | exception     | domain error from clipping engine |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static Boolean operations — `Clipper` facade
- rail: fabrication

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]   | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------- | :--------------- | :---------------------------- |
|   [1]   | `Intersect(Paths64, Paths64, FillRule)`             | one-shot clip    | int64 intersection            |
|   [2]   | `Intersect(PathsD, PathsD, FillRule, precision)`    | one-shot clip    | double intersection           |
|   [3]   | `Union(Paths64, FillRule)`                          | one-shot clip    | int64 self-union              |
|   [4]   | `Union(Paths64, Paths64, FillRule)`                 | one-shot clip    | int64 union                   |
|   [5]   | `Union(PathsD, FillRule)`                           | one-shot clip    | double self-union             |
|   [6]   | `Union(PathsD, PathsD, FillRule, precision)`        | one-shot clip    | double union                  |
|   [7]   | `Difference(Paths64, Paths64, FillRule)`            | one-shot clip    | int64 difference              |
|   [8]   | `Difference(PathsD, PathsD, FillRule, precision)`   | one-shot clip    | double difference             |
|   [9]   | `Xor(Paths64, Paths64, FillRule)`                   | one-shot clip    | int64 exclusive-or            |
|  [10]   | `Xor(PathsD, PathsD, FillRule, precision)`          | one-shot clip    | double exclusive-or           |
|  [11]   | `BooleanOp(ClipType, Paths64?, Paths64?, FillRule)` | one-shot clip    | int64 generic Boolean         |
|  [12]   | `BooleanOp(ClipType, PathsD, PathsD?, FillRule, p)` | one-shot clip    | double generic Boolean        |
|  [13]   | `BooleanOp(.. PolyTree64, FillRule)`                | tree-output clip | int64 Boolean into PolyTree64 |
|  [14]   | `BooleanOp(.. PolyTreeD, FillRule, precision)`      | tree-output clip | double Boolean into PolyTreeD |

[ENTRYPOINT_SCOPE]: static offset and geometry — `Clipper` facade
- rail: fabrication

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `InflatePaths(Paths64, delta, JoinType, EndType, …)`      | offset         | int64 polygon/open-path inflation  |
|   [2]   | `InflatePaths(PathsD, delta, JoinType, EndType, …)`       | offset         | double polygon/open-path inflation |
|   [3]   | `RectClip(Rect64, Paths64)`                               | rect clip      | int64 paths clipped to rect        |
|   [4]   | `RectClip(Rect64, Path64)`                                | rect clip      | int64 single path clipped to rect  |
|   [5]   | `RectClip(RectD, PathsD, precision)`                      | rect clip      | double paths clipped to rect       |
|   [6]   | `RectClipLines(Rect64, Paths64)`                          | rect clip      | int64 open paths clipped to rect   |
|   [7]   | `RectClipLines(RectD, PathsD, precision)`                 | rect clip      | double open paths clipped to rect  |
|   [8]   | `MinkowskiSum(Path64, Path64, isClosed)`                  | morphology     | int64 Minkowski sum                |
|   [9]   | `MinkowskiSum(PathD, PathD, isClosed)`                    | morphology     | double Minkowski sum               |
|  [10]   | `MinkowskiDiff(Path64, Path64, isClosed)`                 | morphology     | int64 Minkowski difference         |
|  [11]   | `MinkowskiDiff(PathD, PathD, isClosed)`                   | morphology     | double Minkowski difference        |
|  [12]   | `Ellipse(Point64, radiusX, radiusY, steps)`               | geometry       | int64 ellipse/circle path          |
|  [13]   | `Ellipse(PointD, radiusX, radiusY, steps)`                | geometry       | double ellipse/circle path         |
|  [14]   | `Triangulate(Paths64, out Paths64, useDelaunay)`          | triangulation  | int64 Delaunay triangulation       |
|  [15]   | `Triangulate(PathsD, decPlaces, out PathsD, useDelaunay)` | triangulation  | double Delaunay triangulation      |

[ENTRYPOINT_SCOPE]: static path measurement and analysis — `Clipper` facade
- rail: fabrication

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `Area(Path64)` / `Area(Paths64)`              | measurement    | signed area (positive = CCW)     |
|   [2]   | `Area(PathD)` / `Area(PathsD)`                | measurement    | signed area (double)             |
|   [3]   | `IsPositive(Path64)` / `IsPositive(PathD)`    | orientation    | CCW winding test                 |
|   [4]   | `GetBounds(Path64)` / `GetBounds(Paths64)`    | bounds         | int64 axis-aligned bounding box  |
|   [5]   | `GetBounds(PathD)` / `GetBounds(PathsD)`      | bounds         | double axis-aligned bounding box |
|   [6]   | `PointInPolygon(Point64, Path64)`             | containment    | inside / on / outside test       |
|   [7]   | `PointInPolygon(PointD, PathD, precision)`    | containment    | double containment test          |
|   [8]   | `RamerDouglasPeucker(Path64, epsilon)`        | simplification | int64 polyline simplification    |
|   [9]   | `RamerDouglasPeucker(PathD, epsilon)`         | simplification | double polyline simplification   |
|  [10]   | `SimplifyPath(Path64, epsilon, isClosedPath)` | simplification | int64 collinear simplification   |
|  [11]   | `SimplifyPath(PathD, epsilon, isClosedPath)`  | simplification | double collinear simplification  |
|  [12]   | `TrimCollinear(Path64, isOpen)`               | cleanup        | int64 collinear point removal    |
|  [13]   | `TrimCollinear(PathD, precision, isOpen)`     | cleanup        | double collinear point removal   |
|  [14]   | `StripDuplicates(Path64, isClosedPath)`       | cleanup        | consecutive duplicate removal    |

[ENTRYPOINT_SCOPE]: static path conversion and transform — `Clipper` facade
- rail: fabrication

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------------- |
|   [1]   | `ScalePath(Path64, scale)` / `ScalePaths(…)`                  | conversion     | int64 coordinate scaling            |
|   [2]   | `ScalePath64(PathD, scale)` / `ScalePaths64(…)`               | conversion     | double-to-int64 path conversion     |
|   [3]   | `ScalePathD(Path64, scale)` / `ScalePathsD(…)`                | conversion     | int64-to-double path conversion     |
|   [4]   | `TranslatePath(Path64, dx, dy)`                               | transform      | integer path translation            |
|   [5]   | `TranslatePath(PathD, dx, dy)`                                | transform      | double path translation             |
|   [6]   | `ReversePath(Path64)` / `ReversePath(PathD)`                  | transform      | winding reversal                    |
|   [7]   | `MakePath(int[])` / `MakePath(long[])` / `MakePath(double[])` | factory        | interleaved-array path construction |
|   [8]   | `PolyTreeToPaths64(PolyTree64)`                               | tree flatten   | nested result to flat Paths64       |
|   [9]   | `PolyTreeToPathsD(PolyTreeD)`                                 | tree flatten   | nested result to flat PathsD        |

[ENTRYPOINT_SCOPE]: stateful clipping — `Clipper64` engine
- rail: fabrication

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `AddSubject(Paths64)`                                       | input          | closed subject paths         |
|   [2]   | `AddOpenSubject(Paths64)`                                   | input          | open subject paths           |
|   [3]   | `AddClip(Paths64)`                                          | input          | clip paths                   |
|   [4]   | `Execute(ClipType, FillRule, Paths64 closed)`               | operation      | closed-only int64 result     |
|   [5]   | `Execute(ClipType, FillRule, Paths64 closed, Paths64 open)` | operation      | closed + open int64 result   |
|   [6]   | `Execute(ClipType, FillRule, PolyTree64)`                   | operation      | tree-structured int64 result |
|   [7]   | `Execute(ClipType, FillRule, PolyTree64, Paths64 open)`     | operation      | tree + open int64 result     |
|   [8]   | `PreserveCollinear` / `ReverseSolution`                     | property       | engine behavior flags        |

[ENTRYPOINT_SCOPE]: stateful clipping — `ClipperD` engine
- rail: fabrication

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `ClipperD(roundingDecimalPrecision = 2)`                  | constructor    | precision 1–8 decimal places  |
|   [2]   | `AddSubject(PathD)` / `AddSubject(PathsD)`                | input          | closed subject paths (double) |
|   [3]   | `AddOpenSubject(PathD)` / `AddOpenSubject(PathsD)`        | input          | open subject paths (double)   |
|   [4]   | `AddClip(PathD)` / `AddClip(PathsD)`                      | input          | clip paths (double)           |
|   [5]   | `Execute(ClipType, FillRule, PathsD closed)`              | operation      | closed-only double result     |
|   [6]   | `Execute(ClipType, FillRule, PathsD closed, PathsD open)` | operation      | closed + open double result   |
|   [7]   | `Execute(ClipType, FillRule, PolyTreeD)`                  | operation      | tree-structured double result |
|   [8]   | `Execute(ClipType, FillRule, PolyTreeD, PathsD open)`     | operation      | tree + open double result     |

[ENTRYPOINT_SCOPE]: offset engine — `ClipperOffset`
- rail: fabrication

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------ | :------------- | :-------------------------------------- |
|   [1]   | `ClipperOffset(miterLimit, arcTolerance, …)`            | constructor    | offset engine with join and arc options |
|   [2]   | `AddPath(Path64, JoinType, EndType)`                    | input          | single path registration                |
|   [3]   | `AddPaths(Paths64, JoinType, EndType)`                  | input          | multi-path registration                 |
|   [4]   | `Execute(double delta, Paths64 solution)`               | operation      | inflate to flat result                  |
|   [5]   | `Execute(double delta, PolyTree64 solutionTree)`        | operation      | inflate to tree result                  |
|   [6]   | `Execute(DeltaCallback64 callback, Paths64 solution)`   | operation      | per-vertex variable delta inflate       |
|   [7]   | `Clear()`                                               | reset          | discard registered path groups          |
|   [8]   | `ArcTolerance` / `MiterLimit`                           | property       | arc and miter precision tuning          |
|   [9]   | `MergeGroups` / `PreserveCollinear` / `ReverseSolution` | property       | merge and winding behavior flags        |
|  [10]   | `DeltaCallback` (delegate `DeltaCallback64`)            | property       | per-point variable offset callback      |

## [4]-[IMPLEMENTATION_LAW]

[COORDINATE_TOPOLOGY]:
- `int64` path: `Point64.X` and `Point64.Y` are `long`; coordinates must be pre-scaled before clipping when originating from floating-point geometry
- `double` path: `PointD.x` / `PointD.y` are `double`; `ClipperD` internally scales by `10^precision` before executing and descales the result
- `ClipperD` precision range: integers `−8` through `8`; values outside this range raise `ClipperLibException`
- `FillRule.EvenOdd`: alternating fill by crossing count; `NonZero`: non-zero winding; `Positive`: positive-winding areas only; `Negative`: negative-winding areas only
- `ClipType.NoClip`: returns subject unchanged; `Intersection`, `Union`, `Difference`, `Xor` are the four Boolean operations
- `JoinType.Miter`: sharp corners up to `MiterLimit`; `Square`: flat angled cap; `Bevel`: flat cut across; `Round`: arc
- `EndType.Polygon`: closed path offset; `Joined`: open path treated as closed join; `Butt`: flat perpendicular end; `Square`: extended square end; `Round`: semicircular end
- `Rect64` layout: `left`, `top`, `right`, `bottom` as `long`; `IsEmpty` returns true when `bottom <= top` or `right <= left`

[LOCAL_ADMISSION]:
- Use `Clipper` static methods for one-shot operations; prefer `Clipper64` or `ClipperD` stateful engines only when re-using subject/clip state across multiple `Execute` calls
- Scale floating-point geometry to int64 via `Clipper.ScalePaths64(paths, scale)` where `scale = Math.Pow(10, precision)` before passing to `Clipper64`; descale results with `Clipper.ScalePathsD`
- `ClipperOffset.Execute` writes directly into the caller-supplied `Paths64` or `PolyTree64`; clear or allocate a fresh container before each call
- `PolyTree64` / `PolyTreeD` carry outer/hole nesting; flatten to `Paths64`/`PathsD` via `Clipper.PolyTreeToPaths64` / `Clipper.PolyTreeToPathsD` when nesting structure is not required
- `DeltaCallback64` receives the current path, path normals, current vertex index, and previous vertex index; return the signed offset delta for that vertex

[RAIL_LAW]:
- Package: `Clipper2`
- Owns: 2D polygon Boolean operations, path offsetting, rect clipping, Minkowski operations, triangulation, and path utilities
- Accept: `Path64`/`Paths64` for int64 geometry; `PathD`/`PathsD` with explicit precision for double geometry
- Reject: hand-rolled polygon clipping, manual coordinate scaling without using `Clipper.ScalePaths64`/`ScalePathsD`, and direct manipulation of `ClipperBase` internals
