# [RASM_API_CAVALIERCONTOURS]

`CavalierContours` (oberbichler, ns `CavalierContours.*`) is the arc-aware 2D polyline
parallel-offset and boolean engine — a C# port of the Rust `cavalier_contours` library, generic
over `T : IFloatingPointIeee754<T>, IMinMaxValue<T>` (modern .NET generic math: works at `double`,
`float`, or any IEEE float). Its defining property is that vertices carry a **bulge** (`Bulge =
tan(theta/4)` of the arc between this vertex and the next), so a polyline natively represents
mixed line/arc segments and the offset/boolean operations stay in ARC-SPACE — no line-densify,
no arc re-fit. `PlineOffset.ParallelOffset` generates self-intersection-handled inset/outset
loops; `PlineBoolean.PolylineBoolean` does closed-polyline Or/And/Not/Xor; `Shape<T>.ParallelOffset`
offsets a multi-loop shape (outer + holes) coherently; and `StaticAABB2DIndex<T>` is a packed
Hilbert-R-tree (flatbush-style) spatial index reused across operations. This is the kernel's
arc-domain 2D offset/boolean owner; the integer-exact STRAIGHT-segment boolean/offset stays with
`Clipper2`, and exact 3D mesh CSG stays with the authored `Meshing/arrangement`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CavalierContours`
- package: `CavalierContours`
- version: `1.0.0`
- license: `ISC` (permissive)
- assembly: `CavalierContours` (multi-target `net10.0` + `net8.0`; the `lib/net10.0/CavalierContours.dll` is the consumer-bound asset)
- namespaces: `CavalierContours.Core` (math/intersection), `.Polyline` (polyline + offset/boolean), `.Shape` (multi-loop), `.Spatial` (R-tree index)
- deps: none (pure-managed AnyCPU, osx-arm64-clean; uses BCL `generic math` + `PriorityQueue`/`ReadOnlySpan`)
- owners: `libs/csharp/Rasm/Rasm.csproj` (kernel owner — `[COMPUTATIONAL_GEOMETRY]`), `libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj` (kerf/lead-arc + adaptive-clearing, over the one central pin)
- rail: arc-aware 2D offset / closed-polyline boolean

## [02]-[POLYLINE_MODEL]

[POLYLINE_MODEL_SCOPE]: the bulge-carrying polyline, its vertex, and the spatial index — every type is generic over the float `T`
- rail: geometry

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `PlineVertex<T>`          | `readonly struct (T X, Y, Bulge)` | one vertex; `Bulge = tan(theta/4)` is the arc to the NEXT vertex (`0` = straight) |
|  [02]   | `Polyline<T>`             | `class : IPlineSourceMut<T>` | the open/closed bulge-polyline; `this[int]`, `AddVertex`/`InsertVertex`/`SetVertex`, `IsClosed`, per-vertex `ulong` user-data |
|  [03]   | `IPlineSource<T>`/`IPlineSourceMut<T>` | `interface` | the read / read-write polyline contract every op accepts (so any backing store qualifies) |
|  [04]   | `Vector2<T>`              | `static` + `readonly struct` | the 2D point (`Vector2.New<T>(x,y)`); `PlineVertex<T>.Pos()` projects to it |
|  [05]   | `AABB<T>`                 | `readonly struct (MinX,MinY,MaxX,MaxY)` | axis-aligned bbox; `OverlapsAABB`/`ContainsAABB`/`Overlaps`              |
|  [06]   | `StaticAABB2DIndex<T>`    | `class`       | immutable packed Hilbert-R-tree over segment bboxes; `Query`/`VisitQuery`/`VisitNeighbors` |
|  [07]   | `Shape<T>`                | `class`       | a multi-loop region (outer CCW + holes CW); `ParallelOffset` offsets all loops coherently |
|  [08]   | `BooleanOp`               | `enum : byte` | `Or` (union) / `And` (intersection) / `Not` (difference) / `Xor`         |
|  [09]   | `PlineOrientation`        | `enum : byte` | `Open` / `Clockwise` / `CounterClockwise` — orientation drives offset direction |

`Polyline<T>` is built by `new Polyline<T>(isClosed:true)` + `AddVertex(new PlineVertex<T>(x,y,bulge))`, or the `PlineSourceExtensions.Add(x,y,bulge)` extension. `PlineVertex<T>` exposes
`WithBulge`, `BulgeIsZero/Pos/Neg`, `FromVector2(v, bulge)`, `FromSlice(ReadOnlySpan<T>)`. The
`Vectors` typed 2D vocabulary boundary-maps to `Vector2<T>` and a bulge value at the seam; bulge
is the only concept the kernel's straight-segment paths lack, so a `Path64` (Clipper2) → bulge
polyline conversion adds the arc parameter, never drops one.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the offset, boolean, and query verbs — the algorithm cores plus the `IPlineSource<T>` extension surface
- rail: offset / boolean / query

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `PlineOffset.ParallelOffset<O,T>(IPlineSource<T> pline, T offset, PlineOffsetOptions<T> opts)` → `List<O>` | offset         | arc-space parallel offset; positive/negative `offset`, self-intersection-handled, splits into multiple result loops |
|  [02]   | `Shape<T>.ParallelOffset(T offset, ShapeOffsetOptions<T> opts)` → `Shape<T>`                      | offset (region) | coherent multi-loop (outer+holes) offset — the pocket/island case        |
|  [03]   | `PlineBoolean.PolylineBoolean<O,T>(IPlineSource<T> p1, IPlineSource<T> p2, BooleanOp op, PlineBooleanOptions<T> opts)` → `BooleanResult<O,T>` | boolean        | closed-polyline Or/And/Not/Xor in arc-space; result carries the stitched loops |
|  [04]   | `pline.CreateAabbIndex()` / `CreateApproxAabbIndex()` → `StaticAABB2DIndex<T>`                    | index (ext)    | build the segment R-tree once; pass it back via `opts.AabbIndex` to skip a rebuild |
|  [05]   | `pline.WindingNumber(Vector2<T> point)` → `int` / `pline.ClosestPoint(point, eps)` → `ClosestPointResult<T>?` | query (ext)    | containment winding number / nearest-point-on-polyline (seg index + point + distance) |
|  [06]   | `pline.Area()` / `PathLength()` / `Orientation()` / `Extents()`                                   | measure (ext)  | signed area (arc-exact), arc-length, CW/CCW, bbox — all bulge-aware       |
|  [07]   | `pline.ArcsToApproxLines(T errorDistance)` → `Polyline<T>` / `pline.IterSegments()`               | convert (ext)  | flatten arcs to a chord-error line polyline (the Clipper2/mesh hand-off), or iterate `(V1,V2)` segment pairs |
|  [08]   | `PlineBoolean.FindIntersects<T>(p1, p2, FindIntersectsOptions<T>)` → `PlineIntersectsCollection<T>` | intersect      | the raw intersection set (basic + overlapping) the boolean is built on    |

The high-level consumer verbs are the static `PlineOffset.ParallelOffset` /
`PlineBoolean.PolylineBoolean` and `Shape<T>.ParallelOffset`; the query/measure/convert verbs are
extension methods on `IPlineSource<T>` (`PlineSourceExtensions`). The boolean is decomposed into
public stages (`ProcessForBoolean` → `SliceAtIntersects` → `PruneSlices` → `StitchSlicesIntoClosedPolylines`) so a caller can intercept the slice set, but the kernel uses the one-shot `PolylineBoolean`.

## [04]-[IMPLEMENTATION_LAW]

[BULGE_ALGEBRA]:
- `Bulge = tan(theta/4)` where `theta` is the signed sweep of the arc from this vertex to the next: `0` = straight segment, `+` = CCW arc, `-` = CW arc; magnitude `1` = semicircle — so one polyline losslessly mixes lines and arcs and every op stays arc-exact
- this is the whole point of routing here instead of `Clipper2`: Clipper2's offset emits line approximations of round joins, which a CAM toolpath would have to re-fit to arcs (via `g3.BiArcFit2`); CavalierContours generates the offset arcs directly, so kerf/lead-in/lead-out and morphed-spiral clearing passes come out as native G-code arcs
- `ArcsToApproxLines(errorDistance)` is the deliberate down-conversion to a chord-error line polyline when the consumer (a `Clipper2` boolean, a mesh lift, a renderer) cannot take arcs — the kernel calls it explicitly, it is never implicit

[OFFSET_TOPOLOGY]:
- `ParallelOffset` handles self-intersection of the raw offset (`PlineOffsetOptions.HandleSelfIntersects`): it slices the raw offset at self-intersections, prunes invalid slices against the original via the AABB index, and stitches the valid arcs — a concave inset that would fold over splits into multiple disjoint result loops (hence `List<O>`)
- `PlineOffsetOptions<T>` knobs: `AabbIndex` (pre-built index reuse), `HandleSelfIntersects`, `PosEqualEps`/`SliceJoinEps`/`OffsetDistEps` (the three geometric tolerances); offset sign follows orientation — a CCW closed polyline offset by `+delta` grows
- `Shape<T>.ParallelOffset` extends this to a region: outer and hole loops offset in their correct directions and re-pair, the multi-pocket adaptive-clearing primitive

[BOOLEAN_TOPOLOGY]:
- `PolylineBoolean(p1, p2, op, opts)` requires both inputs CLOSED; it finds all intersections (`FindIntersects`), classifies basic vs overlapping, slices both polylines at the intersection set, prunes by the `BooleanOp` (`Or`/`And`/`Not`/`Xor`) using winding/containment, and stitches the surviving slices via an `IStitchSelector` (`OrAndStitchSelector`/`NotXorStitchSelector`) into closed result loops
- `BooleanResult<O,T>` carries the resulting positive/negative loops; coincident/overlapping edges are handled as a distinct slice class (not an epsilon fudge), so exact touchings resolve deterministically
- `PosEqualEps` is the single point-equality tolerance threading the whole pipeline (offset and boolean) — a kernel policy value, not a per-call literal

[SPATIAL_INDEX]:
- `StaticAABB2DIndex<T>` is an immutable flatbush-style packed Hilbert R-tree over segment bounding boxes: `Query(minX,minY,maxX,maxY)→List<int>` / `QueryIter` (allocation-light), `VisitQuery(…, Func<int,bool>)` / `VisitQueryWithStack` (caller-owned stack, zero-alloc), and `VisitNeighbors<V>(x,y, ref V)` / `VisitNeighborsWithQueue` (k-NN with a caller-owned `PriorityQueue`) — the `ref struct`-visitor overloads (`IQueryVisitor`/`INeighborVisitor<T>`) are the hot-path, zero-allocation query forms
- it is built ONCE per polyline (`CreateAabbIndex` exact / `CreateApproxAabbIndex` faster) and reused across offset + boolean via the options' `AabbIndex`; this is g3's `DMeshAABBTree3` and the kernel's BVH analog for the 2D segment domain

[INTEGRATION_STACK]:
- `Rasm.Fabrication` `Toolpath/kinematics`/clearing: this is the primary consumer — kerf compensation, lead-in/lead-out arcs, and morphed-spiral/adaptive-clearing passes generate in arc-space here instead of as line-densified `Clipper2` offsets re-fit to arcs via `g3.BiArcFit2`; the `StaticAABB2DIndex` reused across a pass
- kernel `Meshing/offset#OFFSET`: the arc-exact 2D offset leg routes to `ParallelOffset`/`Shape.ParallelOffset` where the result must stay arc-domain; the predicate-exact wavefront medial/skeleton (Aichholzer-Aurenhammer) stays author-kernel over `Orient2D`, and the straight-segment/integer offset stays with `Clipper2` — three distinct offset owners by domain (arc / exact-wavefront / integer-straight)
- the `[COMPUTATIONAL_GEOMETRY]` leaves do not overlap: `CavalierContours` (arc-aware float offset+boolean), `Clipper2` (integer-exact straight offset+boolean + ear-clip triangulate), `MIConvexHull` (N-D hull/Delaunay/Voronoi), `Triangle` (2D constrained-Delaunay quality mesh), `SharpVoronoiLib` (point-site Fortune Voronoi), `LibTessDotNet` (winding-rule polygon fill), `geometry3Sharp` (dense `DMesh3` substrate) — a concern lands on exactly one
- the `Vectors` 2D vocabulary meets `Vector2<T>` + bulge at the seam; an arc result hands to `Clipper2`/mesh consumers only after an explicit `ArcsToApproxLines`, and a `Clipper2` straight result lifts to a bulge polyline by adding `Bulge = 0`

[LOCAL_ADMISSION]:
- arc/bulge-preserving 2D offset and closed-polyline boolean route HERE; do NOT line-densify-then-`Clipper2`-then-re-fit when the deliverable is arc-domain
- straight-segment integer-exact boolean/offset/triangulation stays with `Clipper2`; 3D mesh CSG stays with `Meshing/arrangement`; this owner is float-domain 2D arcs only
- the AABB index is built once and reused via the options; the geometric tolerances (`PosEqualEps`/`SliceJoinEps`/`OffsetDistEps`) are kernel policy values, not per-call literals
- the generic `T` is pinned to the kernel's working float (`double`) at the boundary; the polyline backing store can be any `IPlineSourceMut<T>`, so a custom store qualifies without a copy

[RAIL_LAW]:
- Package: `CavalierContours`
- Owns: arc-aware (bulge) 2D polyline parallel offset (single + multi-loop `Shape`), closed-polyline boolean (`Or`/`And`/`Not`/`Xor`), polyline winding/closest-point/area/length/orientation queries, and the `StaticAABB2DIndex` packed-R-tree over segments — all generic over `T : IFloatingPointIeee754<T>`
- Accept: an `IPlineSource<T>` bulge polyline (or a `Shape<T>`) plus the operation's options (offset distance / `BooleanOp` / tolerances)
- Reject: a hand-rolled arc-offset; a line-densify-then-`Clipper2`-then-`BiArcFit2` round-trip where arc-space offset is available; using this for straight-segment integer boolean (use `Clipper2`) or 3D mesh CSG (use `Meshing/arrangement`); a second 2D-offset owner beside the (arc / exact-wavefront / integer) split; rebuilding the AABB index per operation
