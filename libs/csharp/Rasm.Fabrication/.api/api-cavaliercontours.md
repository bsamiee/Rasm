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
- `Polyline<T>` implements `IPlineSourceMut<T>` with an indexer, `AddVertex`, `InsertVertex`, `Remove`, `Get`, `SetVertex`, `SetIsClosed`, `ExtendVertexes`, and `ulong UserData`. Its constructors accept no arguments, `bool isClosed`, capacity plus closure, or vertices plus closure.
- `PlineVertex<T>` exposes `X`, `Y`, `Bulge`, `Pos()`, `WithBulge(T)`, `FromVector2(Vector2<T>, T)`, `FromSlice(ReadOnlySpan<T>)`, bulge-sign predicates, and fuzzy equality.
- `IPlineSource<T>` exposes `VertexCount`, `IsClosed`, `Get(int)`, and `UserDataValues`; `IPlineSourceMut<T>` adds the mutable operations that `CreateFrom<O,T>` targets.
- `PlineView<T>` and `PlineViewData<T>` carry zero-copy source slices for the offset and Boolean pipelines.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]      |
| :-----: | :------------------- | :-------------- | :---------------- |
|  [01]   | `Polyline<T>`        | mutable owner   | polyline storage  |
|  [02]   | `PlineVertex<T>`     | readonly struct | bulge vertex      |
|  [03]   | `IPlineSource<T>`    | read contract   | source projection |
|  [04]   | `IPlineSourceMut<T>` | write contract  | source mutation   |
|  [05]   | `PlineView<T>`       | slice view      | source slice      |
|  [06]   | `PlineViewData<T>`   | slice data      | slice coordinates |
|  [07]   | `PlineOrientation`   | enum            | winding verdict   |

[PUBLIC_TYPE_SCOPE]: offset and Boolean facades (`CavalierContours.Polyline`)

- rail: fabrication
- note: the offset/Boolean engines are static slice-pipeline facades, not instance methods on `Polyline<T>`; they consume an `IPlineSource<T>` plus an options record and emit `Polyline<T>`/slice lists. `BooleanOp` selects the set operation.
- `PlineOffset` composes `ParallelOffset<O,T>` from `CreateRawOffsetPolyline` and `SlicesFromRawOffset`; `PlineBoolean` composes `PolylineBoolean<O,T>` from `ProcessForBoolean`, `PruneSlices`, `StitchSlicesIntoClosedPolylines`, and `FindIntersects`.
- `PlineContains` returns pairwise `PlineContainsResult`; `PlineIntersects` exposes `VisitLocalSelfIntersects` and `VisitGlobalSelfIntersects`.
- `PlineOffsetOptions<T>` carries `HandleSelfIntersects`, `PosEqualEps`, `SliceJoinEps`, `OffsetDistEps`, and `AabbIndex`; `PlineBooleanOptions<T>` carries `PosEqualEps`, `CollapsedAreaEps`, and `Pline1AabbIndex`.
- `PlineContainsOptions<T>`, `FindIntersectsOptions<T>`, and `PlineSelfIntersectOptions<T>` inject per-facade epsilon and index policy.
- `BooleanResult<O,T>` exposes `PosPlines`, `NegPlines`, and `ResultInfo`; each `BooleanResultPline<O,T>` exposes `Pline` and `Subslices` provenance.

`BooleanOp` carries `Or`, `And`, `Not`, and `Xor`; `PlineContainsResult` carries `InvalidInput`, `Pline1InsidePline2`, `Pline2InsidePline1`, `Disjoint`, and `Intersected`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]        |
| :-----: | :----------------------------- | :------------- | :------------------ |
|  [01]   | `PlineOffset`                  | static facade  | parallel offset     |
|  [02]   | `PlineBoolean`                 | static facade  | polyline Boolean    |
|  [03]   | `PlineContains`                | static facade  | pair containment    |
|  [04]   | `PlineIntersects`              | static facade  | intersection visits |
|  [05]   | `BooleanOp`                    | enum           | set operation       |
|  [06]   | `PlineContainsResult`          | enum           | containment verdict |
|  [07]   | `PlineOffsetOptions<T>`        | options record | offset policy       |
|  [08]   | `PlineBooleanOptions<T>`       | options record | Boolean policy      |
|  [09]   | `PlineContainsOptions<T>`      | options record | containment policy  |
|  [10]   | `FindIntersectsOptions<T>`     | options record | intersection policy |
|  [11]   | `PlineSelfIntersectOptions<T>` | options record | self-scan policy    |
|  [12]   | `BooleanResult<O,T>`           | result carrier | Boolean result      |
|  [13]   | `BooleanResultPline<O,T>`      | result carrier | result loop         |
|  [14]   | `ClosestPointResult<T>`        | result struct  | closest projection  |

[PUBLIC_TYPE_SCOPE]: spatial index and geometry primitives (`CavalierContours.Spatial`, `.Core`)

- rail: fabrication
- note: `StaticAABB2DIndex<T>` is a flatbush (packed Hilbert R-tree) built once from a polyline's segment AABBs; the offset/Boolean engines consume it to prune the self-intersection scan. The `Core` structs are the value-type math floor.
- `StaticAABB2DIndex<T>` exposes `Query`, `QueryIter`, `VisitQuery`, `VisitQueryWithStack`, `VisitNeighbors`, `VisitNeighborsWithQueue`, `Bounds`, `Count`, `ItemBoxes`, and `ItemIndices`; `StaticAABB2DIndexBuilder<T>` stages `Add(minX,minY,maxX,maxY)` before `Build()`.
- `VisitQuery<V>` constrains `V` to a `struct` visitor; the delegate adapters wrap `Func<int,bool>` for non-generic traversal.
- `Vector2<T>` carries `X`, `Y`, arithmetic, dot, and cross operations; `AABB<T>` carries `MinX`, `MinY`, `MaxX`, `MaxY`, and `Extents`.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]       | [CAPABILITY]      |
| :-----: | :---------------------------- | :------------------ | :---------------- |
|  [01]   | `StaticAABB2DIndex<T>`        | spatial index       | segment lookup    |
|  [02]   | `StaticAABB2DIndexBuilder<T>` | index builder       | staged build      |
|  [03]   | `IQueryVisitor`               | ref-struct visitor  | query callback    |
|  [04]   | `INeighborVisitor<T>`         | ref-struct visitor  | neighbor callback |
|  [05]   | `DelegateQueryVisitor`        | adapter struct      | query adapter     |
|  [06]   | `DelegateNeighborVisitor<T>`  | adapter struct      | neighbor adapter  |
|  [07]   | `Vector2<T>`                  | readonly struct     | vector algebra    |
|  [08]   | `AABB<T>`                     | readonly struct     | axis-aligned box  |
|  [09]   | `CircleCircleIntr`            | intersection struct | circle pair       |
|  [10]   | `LineCircleIntr`              | intersection struct | line and circle   |
|  [11]   | `LineLineIntr`                | intersection struct | line pair         |

[PUBLIC_TYPE_SCOPE]: multi-loop shape offset (`CavalierContours.Shape`)

- rail: fabrication
- note: `Shape` is the higher-level multi-loop owner — a set of CCW outer loops and CW hole loops offset together with island/hole topology preserved, the form a real pocket-with-islands clearing toolpath needs.
- `Shape<T>` exposes `CcwPlines`, `CwPlines`, `PlinesIndex`, `FromPlines(IEnumerable<Polyline<T>>)`, `Empty()`, and `ParallelOffset(T, ShapeOffsetOptions<T>)`.
- `OffsetLoop<T>` carries `ParentLoopIdx` and `IndexedPline`; `ShapeOffsetOptions<T>` carries `PosEqualEps`, `OffsetDistEps`, and `SliceJoinEps` through its three-value constructor.
- `IndexedPolyline<T>` pairs `Polyline` with a prebuilt `SpatialIndex`; the shape fold traverses both winding lists and projects each cached polyline back to a `Loop`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]    |
| :-----: | :---------------------- | :--------------- | :-------------- |
|  [01]   | `Shape<T>`              | multi-loop owner | topology offset |
|  [02]   | `OffsetLoop<T>`         | loop carrier     | parent lineage  |
|  [03]   | `ShapeOffsetOptions<T>` | options record   | offset policy   |
|  [04]   | `IndexedPolyline<T>`    | indexed loop     | cached polyline |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polyline construction and mutation — `Polyline<T>`

- rail: fabrication
- The constructors accept closure alone, capacity plus closure, or `IEnumerable<PlineVertex<T>>` plus closure; `CreateFrom<O,T>` and `CreateFromRemoveRepeat<O,T>` materialize any `IPlineSourceMut<T>` with a parameterless constructor.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CAPABILITY]                 |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `Polyline<T>(bool)`                             | constructor    | empty polyline               |
|  [02]   | `Polyline<T>(int, bool)`                        | constructor    | reserved polyline            |
|  [03]   | `Polyline<T>(IEnumerable, bool)`                | constructor    | populated polyline           |
|  [04]   | `AddVertex(PlineVertex<T>)`                     | input          | vertex append                |
|  [05]   | `Add(T, T, T)`                                  | input          | coordinate append            |
|  [06]   | `InsertVertex(int, PlineVertex<T>)`             | edit           | vertex insertion             |
|  [07]   | `SetVertex(int, PlineVertex<T>)`                | edit           | vertex replacement           |
|  [08]   | `Remove(int)`                                   | edit           | vertex removal               |
|  [09]   | `this[int]`                                     | read           | indexed vertex               |
|  [10]   | `Get(int)`                                      | read           | indexed vertex               |
|  [11]   | `VertexCount`                                   | read           | vertex count                 |
|  [12]   | `IsClosed`                                      | read           | closure state                |
|  [13]   | `SetIsClosed(bool)`                             | edit           | closure mutation             |
|  [14]   | `Clear()`                                       | edit           | vertex clearing              |
|  [15]   | `ExtendVertexes(IEnumerable)`                   | edit           | bulk append                  |
|  [16]   | `AddOrReplaceVertex(self, vertex, posEqualEps)` | edit           | deduplicated append          |
|  [17]   | `InvertDirection(self)`                         | edit           | winding reversal             |
|  [18]   | `ExtendRemoveRepeat(self, other, eps)`          | edit           | deduplicated merge           |
|  [19]   | `CreateFrom<O,T>(source)`                       | factory        | source materialization       |
|  [20]   | `CreateFromRemoveRepeat<O,T>(source, eps)`      | factory        | deduplicated materialization |

[ENTRYPOINT_SCOPE]: measure, query, and arc handling — `PlineSourceExtensions` (extension methods on `IPlineSource<T>`)

- rail: fabrication
- note: this is the bulk of the consumer surface — every measurement, containment, index build, and arc densification is an extension on the read interface, so they apply uniformly to `Polyline<T>`, `PlineView<T>`, and any custom `IPlineSource<T>`.
- `ClosestPoint(Vector2<T>, T)` returns `ClosestPointResult<T>?`; `Extents()` returns `AABB<T>?`; `FindPointAtPathLength(T)` returns `(bool, int SegIndex, Vector2<T>, T AccLength)`.
- `CreateAabbIndex()` and `CreateApproxAabbIndex()` return `StaticAABB2DIndex<T>`; `ArcsToApproxLines(T)` returns a tolerance-bounded `Polyline<T>` chord projection.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------------------ | :------------- | :-------------------- |
|  [01]   | `Area()`                              | measure        | signed arc area       |
|  [02]   | `PathLength()`                        | measure        | arc path length       |
|  [03]   | `Extents()`                           | measure        | bounding box          |
|  [04]   | `Orientation()`                       | measure        | winding verdict       |
|  [05]   | `WindingNumber(Vector2<T>)`           | containment    | point winding         |
|  [06]   | `ClosestPoint(Vector2<T>, T)`         | projection     | nearest segment point |
|  [07]   | `CreateAabbIndex()`                   | index          | exact segment index   |
|  [08]   | `CreateApproxAabbIndex()`             | index          | approximate index     |
|  [09]   | `ArcsToApproxLines(T)`                | densify        | tolerance chords      |
|  [10]   | `FindPointAtPathLength(T)`            | sample         | arc-length sample     |
|  [11]   | `RemoveRedundant(self, posEqualEps)`  | clean          | collinear removal     |
|  [12]   | `RemoveRepeatPos(posEqualEps)`        | clean          | duplicate removal     |
|  [13]   | `RotateStart(startIndex, point, eps)` | clean          | seam rotation         |
|  [14]   | `IterSegments()`                      | enumerate      | segment pairs         |
|  [15]   | `IterVertexes()`                      | enumerate      | vertices              |
|  [16]   | `IterSegmentIndexes()`                | enumerate      | segment indexes       |
|  [17]   | `SegmentCount()`                      | topology       | segment count         |
|  [18]   | `NextWrappingIndex(i)`                | topology       | forward index         |
|  [19]   | `PrevWrappingIndex(i)`                | topology       | reverse index         |
|  [20]   | `FwdWrappingDist(s, e)`               | topology       | wrapping distance     |
|  [21]   | `FuzzyEq(other)`                      | compare        | fuzzy equality        |
|  [22]   | `FuzzyEqEps(other, eps)`              | compare        | epsilon equality      |
|  [23]   | `IsEmpty()`                           | compare        | emptiness             |
|  [24]   | `Last()`                              | read           | terminal vertex       |

[ENTRYPOINT_SCOPE]: parallel offset — `PlineOffset`

- rail: fabrication
- note: `ParallelOffset<O,T>` is the single-call high-level entry returning the finished (trimmed, arc-preserving) offset polylines; internally it is a two-stage slice pipeline (raw un-trimmed offset → trim against the original via the AABB index) and the raw stages are exposed for an adaptive-clearing walk that needs the intermediate. A single inward offset can split a concave loop into SEVERAL result loops — hence `List<O>`.
- `ParallelOffset<O,T>(IPlineSource<T>, T, PlineOffsetOptions<T>)` returns `List<O>` where `O : IPlineSourceMut<T>, new()`.
- `CreateRawOffsetPolyline<O,T>(IPlineSource<T>, T, T)` produces the untrimmed polyline; `CreateUntrimmedRawOffsetSegs<T>(IPlineSource<T>, T)` returns `List<RawPlineOffsetSeg<T>>`.
- `SlicesFromRawOffset<T>(orig, rawOffset, origIndex, offset, options)` returns `List<PlineViewData<T>>`; `PointValidForOffset<T>(polyline, offset, aabbIndex, point, queryStack, posEqualEps, offsetTol)` tests trim candidates.

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [CAPABILITY]       |
| :-----: | :-------------------------------- | :------------- | :----------------- |
|  [01]   | `ParallelOffset<O,T>`             | high-level     | finished offset    |
|  [02]   | `CreateRawOffsetPolyline<O,T>`    | stage one      | untrimmed polyline |
|  [03]   | `CreateUntrimmedRawOffsetSegs<T>` | stage one      | offset primitives  |
|  [04]   | `SlicesFromRawOffset<T>`          | stage two      | valid slices       |
|  [05]   | `PointValidForOffset<T>`          | predicate      | collision test     |

[ENTRYPOINT_SCOPE]: Boolean and containment — `PlineBoolean` / `PlineContains` / `PlineIntersects`

- rail: fabrication
- note: the closed-polyline Boolean is a slice-and-stitch pipeline keyed on `BooleanOp`; it operates in exact arc-space, so a union of two arc-walled pockets keeps its fillet arcs rather than chord-approximating them.
- `PolylineBoolean<O,T>(IPlineSource<T>, IPlineSource<T>, BooleanOp, PlineBooleanOptions<T>)` returns `BooleanResult<O,T>` where `O : IPlineSourceMut<T>, new()`.
- `FindIntersects<T>(pline1, pline2, options)` returns `PlineIntersectsCollection<T>`; `ProcessForBoolean<T>(pline1, pline2, pline1AabbIndex, posEqualEps)` returns `ProcessForBooleanResult<T>`.
- `SliceAtIntersects<T>(...)` and `PruneSlices<T>(p1, p2, info, BooleanOp, eps)` produce `PrunedSlices<T>`; `StitchSlicesIntoClosedPolylines<O,T>(slices, src1, src2, IStitchSelector, eps, collapsedAreaEps?)` returns `List<BooleanResultPline<O,T>>`.
- `PolylineContains<T>(pline1, pline2, options)` returns `PlineContainsResult`; the self-intersection visitors scan locally or globally through the supplied index.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------------------- | :------------- | :-------------------- |
|  [01]   | `PolylineBoolean<O,T>`                 | high-level     | finished Boolean      |
|  [02]   | `FindIntersects<T>`                    | intersect      | pair intersections    |
|  [03]   | `ProcessForBoolean<T>`                 | stage one      | intersection topology |
|  [04]   | `SliceAtIntersects<T>`                 | stage two      | intersected slices    |
|  [05]   | `PruneSlices<T>`                       | stage two      | selected slices       |
|  [06]   | `StitchSlicesIntoClosedPolylines<O,T>` | stage three    | closed result loops   |
|  [07]   | `PolylineContains<T>`                  | containment    | containment verdict   |
|  [08]   | `VisitLocalSelfIntersects<T>`          | self-intersect | local scan            |
|  [09]   | `VisitGlobalSelfIntersects<T>`         | self-intersect | indexed global scan   |

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
