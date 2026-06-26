# [RASM_API_CLIPPER2]

`Clipper2` is the integer-exact 2D polygon-boolean and parallel-offset engine: the Vatti
sweep-line clips arbitrary self-intersecting/holey path sets under the four `ClipType`
operations (`Intersection`/`Union`/`Difference`/`Xor`) and the four `FillRule` winding rules
(`EvenOdd`/`NonZero`/`Positive`/`Negative`), and the angular-offset kernel inflates/deflates
paths under `JoinType` x `EndType`. All clipping runs in the `Point64` integer lattice so the
result is sign-exact (no float-domain robustness loss) — the float `PathD`/`ClipperD` surface is
a scale-to-integer-then-clip-then-scale-back wrapper around the same `long` core. This build is
the richer Clipper2 variant that also ships `Minkowski` sum/difference, ear-clip `Triangulate`
(Delaunay-flippable), `RectClip` viewport clipping, `RamerDouglasPeucker`/`SimplifyPath`, and
`PointInPolygon` on the same path types. It is the planar-cell boolean the Geometry kernel
composes where the result must close exactly in 2D; the predicate-exact 3D mesh boolean stays
with `Meshing/arrangement` (own GWN cell complex over implicit points), and arc-aware (bulge)
2D offset stays with `CavalierContours`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Clipper2`
- package: `Clipper2`
- version: `2.0.0`
- license: `BSL-1.0` (Boost Software License 1.0, `License.txt`; angusj/Clipper2)
- assembly: `Clipper2Lib` (single-target `netstandard2.0`; the one `lib/netstandard2.0/Clipper2Lib.dll` binds forward under the `net10.0` consumer)
- namespace: `Clipper2Lib`
- deps: none (pure-managed AnyCPU, osx-arm64-clean, ALC/IL-only)
- owners: `libs/csharp/Rasm/Rasm.csproj` (kernel owner), `libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj` (downstream consumer of the one central pin)
- rail: planar-boolean / parallel-offset

## [02]-[PATH_MODEL]

[PATH_MODEL_SCOPE]: the integer/float dual path lattice — every operation is mirrored over a `64` (integer) and `D` (double) type pair
- rail: geometry

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :--------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Point64`  | `struct (long X, Y)`        | the exact integer vertex; `+`/`-`/`==`, ctor from `PointD` with a scale  |
|  [02]   | `PointD`   | `struct (double x, y)`      | the float vertex; round-trips to `Point64` via a power-of-ten scale       |
|  [03]   | `Path64`   | `class : List<Point64>`     | one open/closed polyline of integer vertices                              |
|  [04]   | `Paths64`  | `class : List<Path64>`      | a path set (outer + holes + islands) — the unit every boolean consumes    |
|  [05]   | `PathD`/`PathsD` | `class : List<PointD>`/`List<PathD>` | the float mirror; precision lives in the op's `int precision = 2` arg |
|  [06]   | `Rect64`/`RectD` | `struct (l,t,r,b)`    | AABB; `Contains`/`Intersects`/`MidPoint`/`AsPath`, the `RectClip` window  |
|  [07]   | `PolyTree64`/`PolyTreeD` | `class : PolyPath64`  | the nesting result tree (`IsHole`/`Level`/`Count`/`Area`, `IEnumerable`)  |

`Path64 : List<Point64>` and `Paths64 : List<Path64>` — paths ARE BCL lists, so a kernel
`Span<Point64>`/`ReadOnlySpan` of the typed vector vocabulary materializes a path by `new
Path64(IEnumerable<Point64>)` and reads back by index with zero adapter type. `Clipper.MakePath(long[]|int[]|double[])` builds a path from a flat coordinate array. The scale between the
`Vectors` world-coordinate `double` and the `Point64` lattice is the caller's quantization
policy (`ScalePath`/`ScalePaths`/`ScalePath64`/`ScalePathD`), never a hidden default.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: two surfaces — the `Clipper` static facade (one-shot ops, the bulk of the consumer API) and the stateful `Clipper64`/`ClipperD`/`ClipperOffset` engines (multi-input, polytree, reuse)
- rail: boolean / offset

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `Clipper.BooleanOp(ClipType, Paths64? subject, Paths64? clip, FillRule)` → `Paths64`         | boolean (one-shot) | the polymorphic boolean; `Intersect`/`Union`/`Difference`/`Xor` are named shims over it |
|  [02]   | `Clipper.BooleanOp(ClipType, Paths64?, Paths64?, PolyTree64, FillRule)`                      | boolean → tree | the nesting-preserving overload writing a `PolyTree64` (hole/island hierarchy) |
|  [03]   | `Clipper.InflatePaths(Paths64, double delta, JoinType, EndType, double miterLimit=2, double arcTolerance=0)` → `Paths64` | offset (one-shot) | uniform parallel offset; `delta>0` inflate, `delta<0` deflate              |
|  [04]   | `Clipper.MinkowskiSum/MinkowskiDiff(Path64 pattern, Path64 path, bool isClosed)` → `Paths64` | minkowski      | convolution sum/difference — sweep a kernel along a path (tool footprint)   |
|  [05]   | `Clipper.RectClip(Rect64, Paths64)` / `RectClipLines(Rect64, Paths64)` → `Paths64`           | rect-clip      | fast Sutherland-Hodgman viewport clip (closed polygons / open lines)        |
|  [06]   | `Clipper.Triangulate(Paths64, out Paths64 solution, bool useDelaunay=true)` → `TriangulateResult` | triangulate    | ear-clip triangulation with optional Delaunay edge-flip; result enum guards |
|  [07]   | `Clipper64.AddSubject/AddOpenSubject/AddClip(Paths64)` + `Execute(ClipType, FillRule, …)`    | engine (stateful) | multi-subject/clip accumulation, open+closed split solutions, polytree    |
|  [08]   | `ClipperOffset.AddPath(s)(…, JoinType, EndType)` + `Execute(double delta, Paths64|PolyTree64)` | offset (stateful) | grouped multi-path offset, polytree output, the `DeltaCallback64` hook     |

The static `Clipper` facade is the kernel's normal entry — it constructs a throwaway engine per
call. Drop to `Clipper64`/`ClipperD` when one solve must mix several subject and clip sets,
split open and closed solution paths, or emit a `PolyTree64`; drop to `ClipperOffset` for grouped
or variable offset.

## [04]-[IMPLEMENTATION_LAW]

[CLIP_TOPOLOGY]:
- the boolean core is integer-domain (`Point64`/`long`): the Vatti sweep is sign-exact, so collinear/coincident edges and exact touchings resolve deterministically with no epsilon — this is why the kernel routes 2D closed-cell booleans here rather than re-deriving them in `double`
- the float surface (`PathD`/`ClipperD`/`InflatePaths(PathsD,…)`) carries an `int precision` (default `2`, i.e. ×100): it scales to `Point64`, clips, and scales back — precision above ~8 risks `long` overflow on large coordinates, so the kernel quantizes deliberately rather than maxing precision
- `FillRule` selects the winding test: `NonZero` for solid orientation-consistent geometry, `EvenOdd` for raw self-intersecting fill, `Positive`/`Negative` for signed-area selection; `Clipper.IsPositive(path)` and `Clipper.Area(path|paths)` report orientation and signed area
- `PolyTree64` is the only output that preserves the outer/hole/island nesting (`PolyPathBase.IsHole`/`Level`/`Count` + per-node `Area()`); a flat `Paths64` result loses parent/child — `Clipper.PolyTreeToPaths64(tree)` flattens when nesting is not needed

[OFFSET_TOPOLOGY]:
- `JoinType` ∈ `{Miter, Square, Bevel, Round}` controls convex corner joins (`miterLimit` caps the miter spike); `EndType` ∈ `{Polygon, Joined, Butt, Square, Round}` controls open-path ends — `Polygon` closes the path before offsetting, the open ends apply only to open input
- `arcTolerance` (integer units) sets the round-join/round-cap flattening chord error; `0` derives a default from `delta` — a tighter tolerance emits more, finer segments
- `ClipperOffset.DeltaCallback64(Path64 path, PathD path_norms, int currPt, int prevPt) → double` is the per-vertex variable-offset hook: it returns the local `delta` at each point, so a single offset pass produces a non-uniform (tapered/profiled) inset — the Vatti-exact alternative to N uniform offsets stitched together
- offset emits straight-line approximations of round joins; arcs are NOT preserved — when the toolpath must stay in arc-space (bulge), the kernel routes to `CavalierContours` instead of re-fitting these line segments

[REUSE_AND_QUERY]:
- `ReuseableDataContainer64.AddPaths(Paths64, PathType, bool isOpen)` pre-bins an immutable clip/subject set; `Clipper64.AddReuseableData(container)` then reuses it across many `Execute` calls without re-decomposing — the batch path when one fixed clip mask is applied to a stream of subjects
- `Clipper.PointInPolygon(Point64, Path64)` → `PointInPolygonResult` (`IsOn`/`IsInside`/`IsOutside`) is the exact even-odd containment test; `Clipper.RamerDouglasPeucker`/`SimplifyPath(s)` decimate a path to an `epsilon` band; `Clipper.TrimCollinear` drops redundant collinear vertices; `Clipper.Ellipse(center, rx, ry, steps)` mints a polygonized ellipse
- `Triangulate(Paths64, out Paths64, useDelaunay)` returns a `TriangulateResult` (`success`/`fail`/`noPolygons`/`pathsIntersect`) — a self-intersecting input is rejected, not silently mistriangulated

[INTEGRATION_STACK]:
- `Meshing/arrangement#PLANAR_OVERLAY`: a 2D planar overlay/cell-complex over polygon inputs uses `Clipper64` + `PolyTree64` for the exact integer cell decomposition; the kept/classified cells fold into the `Arrangement` carrier, while the 3D mesh-boolean leg keeps its own GWN-over-implicit-point classifier — Clipper2 owns ONLY the planar (z-projected) closed-cell boolean
- `Meshing/offset#OFFSET` and `#ASSEMBLE`: line-domain straight offsets (`InflatePaths`/`ClipperOffset` with the `DeltaCallback64` profile) and loop assembly route through Clipper2 when the offset need not be arc-exact; the wavefront medial/skeleton leg stays author-kernel, and arc-aware offset hands to `CavalierContours`
- `Drawing/pack#FILL` and `Drawing/view#SECTION`: section/outline fill and pack-region clipping use `RectClip` + `Union` + `Triangulate`; the polygon-triangulation seam has THREE distinct owners by input + rule — `Clipper.Triangulate` owns the integer-exact EAR-CLIP with optional Delaunay edge-flip on an already-SIMPLE `Paths64` (it rejects self-intersection as `pathsIntersect`), `LibTessDotNet` (`api-libtess`) owns the arbitrary WINDING-RULE combine that resolves self-intersecting/overlapping/holey contours in one pass, and `Triangle` (`api-triangle`) owns the constrained-DELAUNAY quality mesh (angle/area bounds, Steiner refinement) on a clean PSLG; a clean boolean result already in the lattice → `Clipper.Triangulate`, a messy self-intersecting fill → `LibTessDotNet`, an FEM-grade mesh → `Triangle`
- the kernel's `Vectors` typed point vocabulary materializes a `Path64`/`Paths64` directly (they are `List<Point64>`), and the boolean result feeds back into `Meshing/delaunay`'s `Tessellation` or the `geometry3Sharp` `DMesh3` 2D->3D lift — the `[COMPUTATIONAL_GEOMETRY]` leaves do not overlap on one concern: `Clipper2` (integer-exact straight boolean/offset + ear-clip triangulate), `CavalierContours` (arc/bulge offset+boolean), `MIConvexHull` (N-D hull/Delaunay/Voronoi), `Triangle` (2D constrained-Delaunay quality mesh), `SharpVoronoiLib` (point-site Fortune Voronoi), `LibTessDotNet` (winding-rule polygon fill), and `geometry3Sharp` (dense `DMesh3` substrate)

[LOCAL_ADMISSION]:
- callers quantize world `double` coordinates to the `Point64` lattice through an explicit `ScalePaths`/`MakePath` policy; the integer scale factor is a kernel policy value, never a per-call literal
- closed-cell 2D booleans that must close exactly route here; do NOT re-derive a planar boolean in `double`, and do NOT use the float `ClipperD` surface where integer input is already available
- arc/bulge-preserving offset is `CavalierContours`, NOT a re-fit of these line-offset segments; 3D mesh boolean is `Meshing/arrangement`, NOT a per-slice Clipper stack
- the offset segment count is governed by `arcTolerance`, the boolean precision by the integer scale + `int precision` — both are deliberate kernel knobs, not maxed defaults

[RAIL_LAW]:
- Package: `Clipper2`
- Owns: integer-exact 2D polygon boolean (`ClipType` × `FillRule`), uniform + variable straight-line parallel offset (`JoinType` × `EndType`), Minkowski sum/difference, rect-clip, ear-clip triangulation, RDP simplify, point-in-polygon
- Accept: a `Paths64`/`PathsD` path set with a `FillRule`, or a path + `JoinType`/`EndType` + `delta` for offset
- Reject: a hand-rolled Vatti/Greiner-Hormann clipper; a float-domain boolean where the integer lattice closes exactly; an arc-offset re-fit of these line segments (use `CavalierContours`); a 3D mesh boolean through per-slice 2D clipping (use `Meshing/arrangement`); a second polygon-boolean owner beside this one
