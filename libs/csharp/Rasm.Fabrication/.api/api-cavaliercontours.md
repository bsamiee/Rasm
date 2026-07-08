# [RASM_FABRICATION_API_CAVALIERCONTOURS]

`CavalierContours` is the arc-native (bulge) 2D polyline algebra owner — the C# port of the Rust `cavalier_contours` library — supplying parallel offset, closed-polyline Boolean (union/intersect/difference/XOR), containment/winding, closest-point, path-length/area/extents, arc-to-line densification, and a flatbush `StaticAABB2DIndex` spatial index over open, closed, AND self-intersecting polylines whose vertices carry `Bulge = tan(theta/4)` so a circular-arc segment is ONE vertex pair, not a line-densified fan. Every surface is generic over `T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>` (instantiate `double`, never a hand-bound float), so the offset/Boolean engine runs in exact arc-space rather than the line-only `Clipper2` substrate (`api-clipper2`). It is the kerf/lead-arc + morphed-spiral adaptive-clearing producer that retires the post-hoc `Clipper2`-offset-then-`g3.BiArcFit2`-refit on the `Toolpath/motion`/`skeleton` and `Posting/program` arc rails. The primary entry points are the `Polyline<T>` mutable owner, the `PlineSourceExtensions` query/measure surface, the static `PlineOffset`/`PlineBoolean`/`PlineContains`/`PlineIntersects` facades, and the multi-loop `Shape` offset.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CavalierContours`
- package: `CavalierContours`
- license: `ISC` (oberbichler/CavalierContours; permissive, no reciprocity)
- assembly: `CavalierContours`
- namespace: `CavalierContours.Polyline`, `.Shape`, `.Spatial`, `.Core`
- asset: pure-managed AnyCPU IL, multi-target `lib/net10.0` + `lib/net8.0` (no native asset, no RID burden, ALC-safe); the `net10.0` consumer binds the `lib/net10.0/CavalierContours.dll`. Zero package dependencies
- generic floor: every public type/method is generic over `T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>` — the `System.Numerics` generic-math constraint set, so `Polyline<double>` is the canonical instantiation and the engine is float-width-agnostic
- rail: fabrication (`Polygon` arc-offset; the `Clipper2` Boolean/offset substrate's arc-native peer, NOT a duplicate)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: polyline carriers (`CavalierContours.Polyline`)
- rail: fabrication
- note: a polyline is a vertex list where each `PlineVertex<T>` carries `(X, Y, Bulge)`; `Bulge = tan(theta/4)` of the arc from this vertex to the next (`0` = straight segment, sign = sweep direction), so one constant-radius arc is a single vertex pair. `IsClosed` wraps the last segment back to the first.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                                  |
| :-----: | :--------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Polyline<T>`          | mutable owner      | the concrete `IPlineSourceMut<T>` — indexer, `AddVertex`/`InsertVertex`/`Remove`/`Get`/`SetVertex`, `IsClosed`/`SetIsClosed`, `ExtendVertexes`, ctors `()`/`(bool isClosed)`/`(int capacity, bool isClosed)`/`(IEnumerable<PlineVertex<T>>, bool isClosed)`, and a `ulong` `UserData` attribution channel |
|  [02]   | `PlineVertex<T>`       | readonly struct    | `(X, Y, Bulge)` vertex; `Pos()` → `Vector2<T>`, `WithBulge(T)`, `FromVector2(Vector2<T>, T bulge)`, `FromSlice(ReadOnlySpan<T>)`, `BulgeIsZero/Pos/Neg`, `FuzzyEq`/`FuzzyEqEps` |
|  [03]   | `IPlineSource<T>`      | read contract      | `VertexCount`/`IsClosed`/`Get(int)`/`UserDataValues` — the read view every facade and extension accepts |
|  [04]   | `IPlineSourceMut<T>`   | write contract     | `: IPlineSource<T>` + `SetVertex`/`InsertVertex`/`Remove`/`AddVertex`/`SetIsClosed`/`Clear`/`ExtendVertexes` — the generic mutable target `CreateFrom<O,T>` builds |
|  [05]   | `PlineView<T>` / `PlineViewData<T>` | slice view | a zero-copy sub-polyline view over a source plus an index range (the offset/Boolean slice carrier) |
|  [06]   | `PlineOrientation`     | enum               | `Open` / `Clockwise` / `CounterClockwise` — the signed-area winding verdict |

[PUBLIC_TYPE_SCOPE]: offset and Boolean facades (`CavalierContours.Polyline`)
- rail: fabrication
- note: the offset/Boolean engines are static slice-pipeline facades, not instance methods on `Polyline<T>`; they consume an `IPlineSource<T>` plus an options record and emit `Polyline<T>`/slice lists. `BooleanOp` selects the set operation.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `PlineOffset`             | static facade   | the high-level `ParallelOffset<O,T>(src, offset, options)` → `List<O>` plus the raw slice pipeline (`CreateRawOffsetPolyline`/`SlicesFromRawOffset`) it composes |
|  [02]   | `PlineBoolean`            | static facade   | the high-level `PolylineBoolean<O,T>(p1, p2, BooleanOp, options)` → `BooleanResult<O,T>` plus the raw slice pipeline (`ProcessForBoolean`/`PruneSlices`/`StitchSlicesIntoClosedPolylines`/`FindIntersects`) it composes |
|  [03]   | `PlineContains`           | static facade   | `PolylineContains` → `PlineContainsResult` pairwise containment          |
|  [04]   | `PlineIntersects`         | static facade   | self- and pairwise-intersection visitors (`VisitLocalSelfIntersects`/`VisitGlobalSelfIntersects`) |
|  [05]   | `BooleanOp`               | enum            | `Or` / `And` / `Not` / `Xor` — the closed-polyline set operation         |
|  [06]   | `PlineContainsResult`     | enum            | `InvalidInput` / `Pline1InsidePline2` / `Pline2InsidePline1` / `Disjoint` / `Intersected` |
|  [07]   | `PlineOffsetOptions<T>`   | options record  | `HandleSelfIntersects`, `PosEqualEps`, `SliceJoinEps`, `OffsetDistEps`, the pre-built `AabbIndex` |
|  [08]   | `PlineBooleanOptions<T>`  | options record  | `PosEqualEps`, `CollapsedAreaEps`, the pre-built `Pline1AabbIndex`        |
|  [09]   | `PlineContainsOptions<T>` / `FindIntersectsOptions<T>` / `PlineSelfIntersectOptions<T>` | options record | the per-facade epsilon + index injection knobs |
|  [10]   | `BooleanResult<O,T>` / `BooleanResultPline<O,T>` | result carrier | the Boolean output — `BooleanResult.PosPlines`/`NegPlines : List<BooleanResultPline<O,T>>` (the kept / subtracted result loops; the positive list is `PosPlines`, never a `Positive` accessor) plus `ResultInfo : BooleanResultInfo`; each `BooleanResultPline` carries `.Pline : O` (the result polyline the fold reads) and `.Subslices : List<BooleanPlineSlice<T>>` provenance |
|  [11]   | `ClosestPointResult<T>`   | result struct   | `SegIndex`, `SegClosestPoint`, `Distance` — the `ClosestPoint` projection |

[PUBLIC_TYPE_SCOPE]: spatial index and geometry primitives (`CavalierContours.Spatial`, `.Core`)
- rail: fabrication
- note: `StaticAABB2DIndex<T>` is a flatbush (packed Hilbert R-tree) built once from a polyline's segment AABBs; the offset/Boolean engines consume it to prune the self-intersection scan. The `Core` structs are the value-type math floor.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `StaticAABB2DIndex<T>`         | spatial index   | flatbush over segment AABBs — `Query`/`QueryIter`/`VisitQuery`/`VisitQueryWithStack`/`VisitNeighbors`/`VisitNeighborsWithQueue`, `Bounds`/`Count`/`ItemBoxes`/`ItemIndices` spans |
|  [02]   | `StaticAABB2DIndexBuilder<T>`  | index builder   | the staged `Add(minX,minY,maxX,maxY)` → `Build()` constructor (the extensions wrap it) |
|  [03]   | `IQueryVisitor` / `INeighborVisitor<T>` | ref-struct visitor | the allocation-free `ref struct` query/neighbor callback contract (`VisitQuery<V>` is generic-constrained to a `struct` visitor) |
|  [04]   | `DelegateQueryVisitor` / `DelegateNeighborVisitor<T>` | adapter struct | the `Func<int,bool>`-wrapping visitor for the non-generic `VisitQuery(...)` overload |
|  [05]   | `Vector2<T>`                   | readonly struct | the 2D point/vector value type the whole library speaks (`X`/`Y`, arithmetic, dot/cross) |
|  [06]   | `AABB<T>`                      | readonly struct | the axis-aligned box (`MinX`/`MinY`/`MaxX`/`MaxY`) `Extents` and the index emit |
|  [07]   | `CircleCircleIntr` / `LineCircleIntr` / `LineLineIntr` | intersection struct | the `Core` analytic segment/arc intersection results the engine folds over |

[PUBLIC_TYPE_SCOPE]: multi-loop shape offset (`CavalierContours.Shape`)
- rail: fabrication
- note: `Shape` is the higher-level multi-loop owner — a set of CCW outer loops and CW hole loops offset together with island/hole topology preserved, the form a real pocket-with-islands clearing toolpath needs.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :---------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `Shape<T>`              | multi-loop owner | `CcwPlines`/`CwPlines : List<IndexedPolyline<T>>` + `PlinesIndex`; `FromPlines(IEnumerable<Polyline<T>>)`/`Empty()`; `ParallelOffset(T offset, ShapeOffsetOptions<T>)` → `Shape<T>` offsetting all loops together with CCW-outer/CW-hole nesting preserved |
|  [02]   | `OffsetLoop<T>`         | loop carrier    | `{ ParentLoopIdx : int, IndexedPline : IndexedPolyline<T> }` — one offset loop tagged with its parent for the shape's hole topology |
|  [03]   | `ShapeOffsetOptions<T>` | options record  | `PosEqualEps`/`OffsetDistEps`/`SliceJoinEps`; ctor `(posEqualEps, offsetDistEps, sliceJoinEps)` |
|  [04]   | `IndexedPolyline<T>`    | indexed loop    | a `Polyline<T>` paired with its prebuilt `StaticAABB2DIndex<T>` (the shape's per-loop cache) — `.Polyline : Polyline<T>` and `.SpatialIndex : StaticAABB2DIndex<T>`; the offset fold iterates `Shape<T>.CcwPlines`/`CwPlines` and reads each `IndexedPolyline.Polyline` back to a `Loop` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polyline construction and mutation — `Polyline<T>`
- rail: fabrication

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `new Polyline<T>(bool isClosed)` / `(int capacity, bool isClosed)` | constructor    | an empty open/closed polyline                          |
|  [02]   | `new Polyline<T>(IEnumerable<PlineVertex<T>> vertexes, bool isClosed)` | constructor | a polyline from a vertex sequence                     |
|  [03]   | `AddVertex(PlineVertex<T>)` / `pline.Add(T x, T y, T bulge)` (ext) | input          | append a `(x, y, bulge)` vertex                         |
|  [04]   | `InsertVertex(int, PlineVertex<T>)` / `SetVertex(int, …)` / `Remove(int)` | edit    | positional vertex edit                                  |
|  [05]   | `this[int index]` / `Get(int)` / `VertexCount` / `IsClosed`        | read           | indexer + count + closed flag                           |
|  [06]   | `SetIsClosed(bool)` / `Clear()` / `ExtendVertexes(IEnumerable<…>)` | edit           | close/open, clear, bulk-append                          |
|  [07]   | `PlineSourceExtensions.AddOrReplaceVertex(self, vertex, posEqualEps)` / `InvertDirection(self)` / `ExtendRemoveRepeat(self, other, eps)` | edit | dedup-aware append, winding flip, dedup-merge extend |
|  [08]   | `PlineSourceExtensions.CreateFrom<O,T>(IPlineSource<T>)` / `CreateFromRemoveRepeat<O,T>(src, eps)` | factory | materialize any `IPlineSourceMut<T>` from a source view |

[ENTRYPOINT_SCOPE]: measure, query, and arc handling — `PlineSourceExtensions` (extension methods on `IPlineSource<T>`)
- rail: fabrication
- note: this is the bulk of the consumer surface — every measurement, containment, index build, and arc densification is an extension on the read interface, so they apply uniformly to `Polyline<T>`, `PlineView<T>`, and any custom `IPlineSource<T>`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Area()` / `PathLength()` / `Extents()` → `AABB<T>?`               | measure        | signed area (arc-aware), arc-aware perimeter, bounding box |
|  [02]   | `Orientation()` → `PlineOrientation`                               | measure        | `Open`/`Clockwise`/`CounterClockwise` winding verdict   |
|  [03]   | `WindingNumber(Vector2<T> point)`                                 | containment    | arc-aware winding number for point-in-polygon           |
|  [04]   | `ClosestPoint(Vector2<T> point, T posEqualEps)` → `ClosestPointResult<T>?` | projection | nearest point on the (arc-aware) polyline + its segment |
|  [05]   | `CreateAabbIndex()` / `CreateApproxAabbIndex()` → `StaticAABB2DIndex<T>` | index | exact / approximate segment-AABB flatbush for the offset/Boolean scan |
|  [06]   | `ArcsToApproxLines(T errorDistance)` → `Polyline<T>`               | densify        | tessellate every arc segment to a chord polyline within tolerance (the bridge to a line-only consumer) |
|  [07]   | `FindPointAtPathLength(T targetPathLength)` → `(bool, int SegIndex, Vector2<T>, T AccLength)` | sample | arc-length parameterization — the lead-in/feed-sampling point |
|  [08]   | `RemoveRedundant(self, posEqualEps)` / `RemoveRepeatPos(posEqualEps)` / `RotateStart(startIndex, point, eps)` | clean | collinear/duplicate removal, seam rotation |
|  [09]   | `IterSegments()` / `IterVertexes()` / `IterSegmentIndexes()`      | enumerate      | `(V1, V2)` segment pairs / vertices / `(i, j)` index pairs |
|  [10]   | `SegmentCount()` / `NextWrappingIndex(i)` / `PrevWrappingIndex(i)` / `FwdWrappingDist(s, e)` | topology | wrap-aware segment indexing for closed polylines |
|  [11]   | `FuzzyEq(other)` / `FuzzyEqEps(other, eps)` / `IsEmpty()` / `Last()` | compare      | structural equality and emptiness                       |

[ENTRYPOINT_SCOPE]: parallel offset — `PlineOffset`
- rail: fabrication
- note: `ParallelOffset<O,T>` is the single-call high-level entry returning the finished (trimmed, arc-preserving) offset polylines; internally it is a two-stage slice pipeline (raw un-trimmed offset → trim against the original via the AABB index) and the raw stages are exposed for an adaptive-clearing walk that needs the intermediate. A single inward offset can split a concave loop into SEVERAL result loops — hence `List<O>`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `PlineOffset.ParallelOffset<O,T>(IPlineSource<T> polyline, T offset, PlineOffsetOptions<T> options)` where `O : IPlineSourceMut<T>, new()` → `List<O>` | offset (high-level) | the finished parallel offset (positive = outward for a CCW loop); the single consumer entry |
|  [02]   | `PlineOffset.CreateRawOffsetPolyline<O,T>(IPlineSource<T> polyline, T offset, T posEqualEps)` | offset stage 1 | the raw un-trimmed parallel offset (arc-preserving), for the adaptive walk |
|  [03]   | `PlineOffset.CreateUntrimmedRawOffsetSegs<T>(IPlineSource<T> polyline, T offset)` → `List<RawPlineOffsetSeg<T>>` | offset stage 1 | the raw per-segment offset primitives |
|  [04]   | `PlineOffset.SlicesFromRawOffset<T>(orig, rawOffset, origIndex, offset, PlineOffsetOptions<T>)` → `List<PlineViewData<T>>` | offset stage 2 | trim the raw offset to valid slices against the original |
|  [05]   | `PlineOffset.PointValidForOffset<T>(polyline, offset, aabbIndex, point, queryStack, posEqualEps, offsetTol)` | offset predicate | per-point self-collision test the trim consults |

[ENTRYPOINT_SCOPE]: Boolean and containment — `PlineBoolean` / `PlineContains` / `PlineIntersects`
- rail: fabrication
- note: the closed-polyline Boolean is a slice-and-stitch pipeline keyed on `BooleanOp`; it operates in exact arc-space, so a union of two arc-walled pockets keeps its fillet arcs rather than chord-approximating them.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `PlineBoolean.PolylineBoolean<O,T>(IPlineSource<T> pline1, IPlineSource<T> pline2, BooleanOp operation, PlineBooleanOptions<T> options)` where `O : IPlineSourceMut<T>, new()` → `BooleanResult<O,T>` | Boolean (high-level) | the finished closed-polyline Boolean (positive + negative result loops); the single consumer entry |
|  [02]   | `PlineBoolean.FindIntersects<T>(pline1, pline2, FindIntersectsOptions<T>)` → `PlineIntersectsCollection<T>` | intersect | all basic + overlapping intersections between two polylines |
|  [03]   | `PlineBoolean.ProcessForBoolean<T>(pline1, pline2, pline1AabbIndex, posEqualEps)` → `ProcessForBooleanResult<T>` | Boolean stage 1 | resolve the intersection topology for the Boolean |
|  [04]   | `PlineBoolean.SliceAtIntersects<T>(...)` / `PruneSlices<T>(p1, p2, info, BooleanOp, eps)` → `PrunedSlices<T>` | Boolean stage 2 | slice at intersections, keep the slices the op selects |
|  [05]   | `PlineBoolean.StitchSlicesIntoClosedPolylines<O,T>(slices, src1, src2, IStitchSelector, eps, collapsedAreaEps?)` → `List<BooleanResultPline<O,T>>` | Boolean stage 3 | re-stitch kept slices into closed result polylines |
|  [06]   | `PlineContains.PolylineContains<T>(pline1, pline2, PlineContainsOptions<T>)` → `PlineContainsResult` | containment | the five-way containment/disjoint/intersect verdict |
|  [07]   | `PlineIntersects.VisitLocalSelfIntersects<T>(polyline, IPlineIntersectVisitor<T>, eps)` / `VisitGlobalSelfIntersects<T>(polyline, aabbIndex, visitor, eps)` | self-intersect | local-only and global (index-accelerated) self-intersection scan |

## [04]-[IMPLEMENTATION_LAW]

[POLYLINE_TOPOLOGY]:
- a polyline is `IsClosed` + an ordered `PlineVertex<T>` list; each vertex is `(X, Y, Bulge)` where `Bulge = tan(theta/4)` of the arc to the NEXT vertex — `Bulge == 0` is a straight segment, `Bulge > 0` sweeps CCW, `Bulge < 0` sweeps CW, and `|Bulge| == 1` is a semicircle. ONE constant-radius arc is one vertex pair; this is the whole reason to choose `CavalierContours` over `Clipper2` for an arc-walled profile
- the generic constraint is `T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>` — instantiate `Polyline<double>` and `Vector2<double>`; the library never assumes `double` internally, so a `Half`/`float` consumer is type-legal but `double` is the fabrication instantiation
- `Orientation()` returns `Open` for an open polyline; a closed CCW polyline has positive `Area()` and `CounterClockwise` orientation — the offset sign convention follows this (positive offset of a CCW closed loop inflates outward)
- `WindingNumber`/`ClosestPoint`/`Area`/`PathLength` are all ARC-AWARE — they integrate over the true arc, not a chord approximation; only `ArcsToApproxLines(errorDistance)` deliberately chord-approximates, and it is the explicit bridge when a downstream consumer (a line-only `Clipper2` op, a mesh tessellator) cannot accept bulge

[LOCAL_ADMISSION]:
- build a profile as `Polyline<double>` carrying bulge directly from the `Ingress/profile` `ACadSharp` arc/`Spline` entities (an arc DXF entity → one `PlineVertex` pair with the bulge computed from its sweep), never densifying the arc to a line fan at ingest
- the offset is the single-call static `PlineOffset.ParallelOffset<Polyline<double>, double>(src, offset, options)` → `List<Polyline<double>>` (a single inward offset of a concave loop legitimately returns several loops); reach for the raw `CreateRawOffsetPolyline` → `SlicesFromRawOffset` stages ONLY when an adaptive-clearing walk needs the un-trimmed intermediate — do NOT hand-spell the pipeline for an ordinary offset
- the closed-polyline Boolean is the single-call `PlineBoolean.PolylineBoolean<Polyline<double>, double>(p1, p2, BooleanOp.Not, options)` → `BooleanResult<…>` keyed on `BooleanOp { Or, And, Not, Xor }`; `Not` is the kerf-inflated remnant difference the `Nesting/nfp` `Remnant` lineage producer needs in arc-space (the three-stage `ProcessForBoolean` → `PruneSlices` → `StitchSlicesIntoClosedPolylines` is the internal decomposition, not the call-site shape)
- a pocket with islands is the multi-loop `Shape<T>.FromPlines(loops).ParallelOffset(offset, ShapeOffsetOptions<T>)` → `Shape<T>` — it offsets the CCW outer and CW hole loops together preserving the island topology, the form a real pocket-clearing toolpath needs (a per-loop `PlineOffset.ParallelOffset` loses the hole nesting)
- the `StaticAABB2DIndex<T>` is built ONCE per polyline via `CreateAabbIndex()` and threaded into the `PlineOffsetOptions<T>.AabbIndex` / `PlineBooleanOptions<T>.Pline1AabbIndex` so the self-intersection scan is index-pruned, not O(n²); reuse the index across the repeated inward offsets of an adaptive-clearing pass
- the `VisitQuery<V>`/`VisitNeighbors<V>` index traversals take a `ref struct V : IQueryVisitor` for an allocation-free hot loop; the `Func<int,bool>` overload is the convenience form — prefer the ref-struct visitor inside the offset/nesting sweep where the query runs per candidate position
- arc-length sampling for lead-in placement and feed-point emission is `FindPointAtPathLength(targetPathLength)`, never a manual chord walk; the result carries the segment index and accumulated length so the posting biarc refit reads the true arc parameter

[INTEGRATION_STACK]:
- `Polygon` offset substrate: this is the ARC-NATIVE peer of the line-only `Clipper2` (`api-clipper2`) — `Clipper2` owns int64/`PathD` Boolean + `ClipperOffset` line offset and Minkowski; `CavalierContours` owns the bulge-carrying offset/Boolean `Clipper2` structurally CANNOT express (a `Clipper2` offset emits straight segments + an arc-approximating fan at corners, never a constant-radius arc segment). They are NOT a duplicate: route a kerf-comp arc, a lead-arc, or a morphed-spiral adaptive pass through `CavalierContours` in arc-space; route a pure-polygon clip/Minkowski-NFP construction through `Clipper2`. The bridge in EITHER direction is `ArcsToApproxLines(errorDistance)` → `Clipper2` `PathD`, and a `Clipper2` `PathD` result re-fit to arcs via `g3.BiArcFit2` (`api-geometry3sharp`) ONLY when the source was line-only — when the source carries bulge, stay in `CavalierContours` and skip the refit entirely (the retirement the README records)
- `g3.BiArcFit2` retirement: the `Toolpath/motion`/`skeleton` lead-arc and adaptive-clearing rails previously offset with `Clipper2` then refit the polyline to biarcs with `geometry3Sharp` `g3.BiArcFit2`; with `CavalierContours` carrying bulge through the offset, the lead-arc and spiral passes are produced in exact arc-space and the `g3.BiArcFit2` refit is needed ONLY on a genuinely line-sourced path — `g3.BiArcFit2` stays the SOLE biarc owner for that residual case, never re-implemented here
- `Posting/program` arc emit: a `PlineVertex<T>.Bulge` maps directly to a `G2`/`G3` arc move — the bulge IS `tan(theta/4)`, so the arc center and radius derive from the vertex pair without a refit; the `Move.ArcCenter` the posting biarc refit carries is read straight from the `CavalierContours` segment rather than re-fitting a densified path
- `Toolpath/skeleton` boundary: `CavalierContours` is point-and-segment offset/Boolean, NOT a medial-axis/straight-skeleton solver — the straight-skeleton concern stays the in-folder `Toolpath/skeleton` author-kernel (and the `SharpVoronoiLib` point-Voronoi owner, `api-sharpvoronoilib`); `CavalierContours` consumes the skeleton's spine to drive the morphed-spiral offset, it does not compute the spine
- `Nesting/nfp` remnant: the kerf-inflated `Remnant` Boolean-difference in arc-space is `PlineBoolean` `Not` — the cut part offset by the kerf and differenced from the stock, keeping arc walls; the resulting remnant carries its bulge into the next nesting pass's `StaticAABB2DIndex` placement scan
- kernel atoms: a `CavalierContours.Core.Vector2<double>` and `AABB<double>` map at the boundary to the kernel `Rasm` `Point3d`/`Vector3d` (z-dropped) and the `Geometry2D` box — plan-cs boundary-maps at the `Polyline<double>` ⇄ `Loop` seam, the bulge preserved into the canonical `Loop` arc-segment representation rather than flattened

[RAIL_LAW]:
- Package: `CavalierContours` (ISC, pure-managed `lib/net10.0` AnyCPU IL, zero deps; generic over `T: IFloatingPointIeee754<T>, IMinMaxValue<T>`, instantiated `double`)
- Owns: arc-native (bulge) 2D polyline parallel offset, closed-polyline Boolean (`Or`/`And`/`Not`/`Xor`), containment/winding, closest-point, arc-aware area/path-length/extents, arc-length sampling, arc-to-line densification, and the flatbush `StaticAABB2DIndex` over open/closed/self-intersecting polylines
- Accept: a `Polyline<double>` carrying real `Bulge` from the `ACadSharp` arc ingest; the static `PlineOffset`/`PlineBoolean` slice pipelines with a once-built `StaticAABB2DIndex` threaded through the options; the `BooleanOp` set op; the ref-struct `IQueryVisitor` for the hot index loop
- Reject: densifying an arc to a line fan at ingest when the source carries bulge (defeats the entire reason to use this library); re-implementing the offset/Boolean on `Clipper2` for an arc-walled profile; a `g3.BiArcFit2` refit of a path that was bulge-carrying through the offset (the retired post-hoc rail); a hand-rolled O(n²) self-intersection scan beside the `StaticAABB2DIndex`; instantiating a non-`double` `T` for the fabrication rail; treating this as a medial-axis solver (it has none — that stays `Toolpath/skeleton`)
