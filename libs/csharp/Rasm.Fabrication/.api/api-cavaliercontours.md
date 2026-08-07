# [RASM_FABRICATION_API_CAVALIERCONTOURS]

`CavalierContours` owns arc-native (bulge) 2D polyline algebra — offset, closed-polyline Boolean, containment, measure, and spatial indexing. Each circular arc rides as one `PlineVertex<T>` pair carrying `Bulge = tan(theta/4)`, so the offset and Boolean engine runs in exact arc-space where the line-only `Clipper2` (`api-clipper2`) cannot. It produces the kerf, lead-arc, and morphed-spiral adaptive-clearing paths in arc-space, retiring the post-hoc `Clipper2`-offset-then-`g3.BiArcFit2`-refit on the `Toolpath` and `Posting` arc rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CavalierContours`
- package: `CavalierContours` (ISC)
- assembly: `CavalierContours`
- namespace: `CavalierContours.Polyline`, `.Shape`, `.Spatial`, `.Core`
- asset: pure-managed AnyCPU IL, multi-target `net10.0`/`net8.0`, ALC-safe, zero package dependencies
- generic: `T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>` — the `System.Numerics` generic-math floor, instantiated `double`
- rail: fabrication — arc-native `Polygon` offset and Boolean, the `Clipper2` line-space peer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: polyline carriers (`CavalierContours.Polyline`)
- note: a carrier is an ordered `PlineVertex<T>` list with `IsClosed`; `Polyline<T>` is the mutable owner, `PlineView<T>`/`PlineViewData<T>` the zero-copy slices the offset and Boolean pipelines read.
- vertex: `PlineVertex<T>(X, Y, Bulge)` mints a bulge vertex; `WithBulge(T)`, `FromVector2(Vector2<T>, T)`, `FromSlice(ReadOnlySpan<T>)`, `Pos()`, and bulge-sign predicates project it.

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
- note: offset and Boolean are static slice-pipeline facades consuming an `IPlineSource<T>` and an options record; each options and result record is one row.
- config: `PlineOffsetOptions<T>` carries `AabbIndex`, `HandleSelfIntersects`, `PosEqualEps`, `SliceJoinEps`, `OffsetDistEps`; `PlineBooleanOptions<T>` carries `Pline1AabbIndex`, `PosEqualEps`, `CollapsedAreaEps`.
- verdicts: `BooleanResultInfo` carries `InvalidInput`, `Pline1InsidePline2`, `Pline2InsidePline1`, `Disjoint`, `Overlapping`, `Intersected`; `PlineContainsResult` carries the same set minus `Overlapping`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]        |
| :-----: | :----------------------------- | :------------- | :------------------ |
|  [01]   | `PlineOffset`                  | static facade  | parallel offset     |
|  [02]   | `PlineBoolean`                 | static facade  | polyline Boolean    |
|  [03]   | `PlineContains`                | static facade  | pair containment    |
|  [04]   | `PlineIntersects`              | static facade  | intersection visits |
|  [05]   | `BooleanOp`                    | enum           | set operation       |
|  [06]   | `PlineContainsResult`          | enum           | containment verdict |
|  [07]   | `BooleanResultInfo`            | enum           | Boolean relation    |
|  [08]   | `PlineOffsetOptions<T>`        | options record | offset policy       |
|  [09]   | `PlineBooleanOptions<T>`       | options record | Boolean policy      |
|  [10]   | `PlineContainsOptions<T>`      | options record | containment policy  |
|  [11]   | `FindIntersectsOptions<T>`     | options record | intersection policy |
|  [12]   | `PlineSelfIntersectOptions<T>` | options record | self-scan policy    |
|  [13]   | `BooleanResult<O,T>`           | result carrier | Boolean result      |
|  [14]   | `BooleanResultPline<O,T>`      | result carrier | result loop         |
|  [15]   | `ClosestPointResult<T>`        | result struct  | closest projection  |

[PUBLIC_TYPE_SCOPE]: spatial index and geometry primitives (`CavalierContours.Spatial`, `.Core`)
- note: `StaticAABB2DIndex<T>` is a flatbush packed-Hilbert R-tree built once from a polyline's segment AABBs; the offset and Boolean engines consume it to prune the self-intersection scan. Both visitor contracts bind a plain `struct`, never a `ref struct`. `Core` structs are the value-type math floor.
- intersections: every `*Intr<T>` result carries a private constructor and is minted only by its static `*Intersection` facade, so the facade row is the whole reachable surface. Each verdict rides `Kind` over a `byte` enum the pair-shape decides — `CircleCircleIntrKind { NoIntersect, TangentIntersect, TwoIntersects, Overlapping }` answering `Point1`/`Point2`, `LineCircleIntrKind { NoIntersect, TangentIntersect, TwoIntersects }` answering the line parameters `T0`/`T1`, `LineLineIntrKind { NoIntersect, TrueIntersect, Overlapping, FalseIntersect }` answering `Seg1T`/`Seg2T`/`Seg2T1` — so a circle form returns points and a line form returns parameters the caller lifts through the segment.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]       | [CAPABILITY]      |
| :-----: | :---------------------------- | :------------------ | :---------------- |
|  [01]   | `StaticAABB2DIndex<T>`        | spatial index       | segment lookup    |
|  [02]   | `StaticAABB2DIndexBuilder<T>` | index builder       | staged build      |
|  [03]   | `IQueryVisitor`               | struct contract     | query callback    |
|  [04]   | `INeighborVisitor<T>`         | struct contract     | neighbor callback |
|  [05]   | `DelegateQueryVisitor`        | adapter struct      | query adapter     |
|  [06]   | `DelegateNeighborVisitor<T>`  | adapter struct      | neighbor adapter  |
|  [07]   | `Vector2<T>`                  | readonly struct     | vector algebra    |
|  [08]   | `AABB<T>`                     | readonly struct     | axis-aligned box  |
|  [09]   | `CircleCircleIntr<T>`         | intersection struct | circle pair       |
|  [10]   | `LineCircleIntr<T>`           | intersection struct | line and circle   |
|  [11]   | `LineLineIntr<T>`             | intersection struct | line pair         |
|  [12]   | `CircleCircleIntersection`    | static facade       | circle-pair solve |
|  [13]   | `LineCircleIntersection`      | static facade       | line-circle solve |
|  [14]   | `LineLineIntersection`        | static facade       | line-pair solve   |
|  [15]   | `PlineSeg`                    | static facade       | segment algebra   |
|  [16]   | `PlineSegIntersection`        | static facade       | arc-aware solve   |
|  [17]   | `PlineSegIntr<T>`             | intersection struct | segment pair      |
|  [18]   | `SplitResult<T>`              | readonly struct     | split vertex pair |

[PUBLIC_TYPE_SCOPE]: multi-loop shape offset (`CavalierContours.Shape`)
- note: `Shape<T>` is the multi-loop owner — CCW outer loops and CW hole loops offset together with island topology preserved, the form a pocket-with-islands clearing toolpath needs.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]    |
| :-----: | :---------------------- | :--------------- | :-------------- |
|  [01]   | `Shape<T>`              | multi-loop owner | topology offset |
|  [02]   | `OffsetLoop<T>`         | loop carrier     | parent lineage  |
|  [03]   | `ShapeOffsetOptions<T>` | options record   | offset policy   |
|  [04]   | `IndexedPolyline<T>`    | indexed loop     | loop plus index |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polyline construction and mutation — `Polyline<T>`, with `CreateFrom` materializers over any `IPlineSourceMut<T>`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `Polyline<T>(bool)`                               | ctor     | empty polyline               |
|  [02]   | `Polyline<T>(int, bool)`                          | ctor     | reserved polyline            |
|  [03]   | `Polyline<T>(IEnumerable, bool)`                  | ctor     | populated polyline           |
|  [04]   | `AddVertex(PlineVertex<T>)`                       | instance | vertex append                |
|  [05]   | `Add(T, T, T)`                                    | instance | coordinate append            |
|  [06]   | `InsertVertex(int, PlineVertex<T>)`               | instance | vertex insertion             |
|  [07]   | `SetVertex(int, PlineVertex<T>)`                  | instance | vertex replacement           |
|  [08]   | `Remove(int)`                                     | instance | vertex removal               |
|  [09]   | `this[int]`                                       | property | indexed vertex               |
|  [10]   | `Get(int)`                                        | instance | indexed vertex               |
|  [11]   | `VertexCount`                                     | property | vertex count                 |
|  [12]   | `IsClosed`                                        | property | closure state                |
|  [13]   | `SetIsClosed(bool)`                               | instance | closure mutation             |
|  [14]   | `Clear()`                                         | instance | vertex clearing              |
|  [15]   | `ExtendVertexes(IEnumerable)`                     | instance | bulk append                  |
|  [16]   | `AddOrReplaceVertex(PlineVertex<T>, T)`           | instance | deduplicated append          |
|  [17]   | `InvertDirection()`                               | instance | winding reversal             |
|  [18]   | `ExtendRemoveRepeat(IPlineSource<T>, T)`          | instance | deduplicated merge           |
|  [19]   | `CreateFrom<O,T>(IPlineSource<T>)`                | factory  | source materialization       |
|  [20]   | `CreateFromRemoveRepeat<O,T>(IPlineSource<T>, T)` | factory  | deduplicated materialization |

[ENTRYPOINT_SCOPE]: segment primitives and exact intersection — `PlineSeg` over one `PlineVertex<T>` pair, and the `CavalierContours.Core` facades under it
- shape: static, every member generic on the same `T`; a segment IS the vertex pair `(v1, v2)` and `v1.Bulge` alone decides line or arc, so no member takes a shape flag
- note: these are the sub-polyline floor — a corridor, kerf-corner, lead-arc trim, or arc-space NFP contact test reaches a single segment here rather than densifying the loop or running a whole-polyline Boolean.

| [INDEX] | [SURFACE]                                                                                            | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `PlineSeg.SegArcRadiusAndCenter(v1, v2) -> (T Radius, Vector2<T> Center)`                            | exact arc frame               |
|  [02]   | `PlineSeg.SegMidpoint(v1, v2) -> Vector2<T>`                                                         | exact arc midpoint            |
|  [03]   | `PlineSeg.SegTangentVector(v1, v2, Vector2<T> pointOnSeg) -> Vector2<T>`                             | exact tangent at a point      |
|  [04]   | `PlineSeg.SegClosestPoint(v1, v2, Vector2<T> point, T epsilon) -> Vector2<T>`                        | nearest point on one segment  |
|  [05]   | `PlineSeg.SegLength(v1, v2) -> T`                                                                    | exact arc or chord length     |
|  [06]   | `PlineSeg.SegBoundingBox(v1, v2) -> AABB<T>`                                                         | exact swept-arc box           |
|  [07]   | `PlineSeg.SegFastApproxBoundingBox(v1, v2) -> AABB<T>`                                               | conservative bulge-offset box |
|  [08]   | `PlineSeg.SegSplitAtPoint(v1, v2, Vector2<T> pointOnSeg, T posEqualEps) -> SplitResult<T>`           | bulge-preserving split        |
|  [09]   | `PlineSegIntersection.Intersect(v1, v2, u1, u2, T posEqualEps) -> PlineSegIntr<T>`                   | arc-aware segment-pair solve  |
|  [10]   | `CircleCircleIntersection.Intersect(T r1, Vector2<T> c1, T r2, Vector2<T> c2, T epsilon)`            | exact circle-pair solve       |
|  [11]   | `LineCircleIntersection.Intersect(Vector2<T> p0, Vector2<T> p1, T radius, Vector2<T> center, T eps)` | exact line-circle solve       |
|  [12]   | `LineLineIntersection.Intersect(Vector2<T> v1, Vector2<T> v2, Vector2<T> u1, Vector2<T> u2, T eps)`  | exact line-pair solve         |

- `PlineSeg.SegFastApproxBoundingBox`: bounds an arc by offsetting the chord box by the bulge rather than sweeping it, so it is a superset of `SegBoundingBox` and the form `CreateApproxAabbIndex()` builds; a containment verdict reads the exact box.
- `PlineSegIntersection.Intersect`: `SplitResult<T>` carries `UpdatedStart` and `SplitVertex`, and `PlineSegIntr<T>` carries `Kind` with `Point1`/`Point2` under `PlineSegIntrKind { NoIntersect, TangentIntersect, OneIntersect, TwoIntersects, OverlappingLines, OverlappingArcs }`. The two overlap cases are verdicts a boolean intersect test cannot represent — a keepout corridor riding a boundary arc reads `OverlappingArcs`, never a crossing — so a `Kind` fold discriminates all six and never collapses to a hit predicate.

[ENTRYPOINT_SCOPE]: measure, query, and arc handling — extension methods on `IPlineSource<T>`, applying uniformly to `Polyline<T>`, `PlineView<T>`, and any custom source
- shape: instance (extensions on `IPlineSource<T>`)

| [INDEX] | [SURFACE]                                                | [CAPABILITY]          |
| :-----: | :------------------------------------------------------- | :-------------------- |
|  [01]   | `Area()`                                                 | signed arc area       |
|  [02]   | `PathLength()`                                           | arc path length       |
|  [03]   | `Extents() -> AABB<T>?`                                  | bounding box          |
|  [04]   | `Orientation()`                                          | winding verdict       |
|  [05]   | `WindingNumber(Vector2<T>)`                              | point winding         |
|  [06]   | `ClosestPoint(Vector2<T>, T) -> ClosestPointResult<T>?`  | nearest segment point |
|  [07]   | `CreateAabbIndex()`                                      | exact segment index   |
|  [08]   | `CreateApproxAabbIndex()`                                | approximate index     |
|  [09]   | `ArcsToApproxLines(T) -> Polyline<T>`                    | tolerance chords      |
|  [10]   | `FindPointAtPathLength(T) -> (bool, int, Vector2<T>, T)` | arc-length sample     |
|  [11]   | `RemoveRedundant(T) -> Polyline<T>?`                     | collinear removal     |
|  [12]   | `RemoveRepeatPos(T) -> Polyline<T>?`                     | duplicate removal     |
|  [13]   | `RotateStart(int, Vector2<T>, T) -> Polyline<T>?`        | seam rotation         |
|  [14]   | `IterSegments()`                                         | segment pairs         |
|  [15]   | `IterVertexes()`                                         | vertices              |
|  [16]   | `IterSegmentIndexes()`                                   | segment indexes       |
|  [17]   | `SegmentCount()`                                         | segment count         |
|  [18]   | `NextWrappingIndex(int)`                                 | forward index         |
|  [19]   | `PrevWrappingIndex(int)`                                 | reverse index         |
|  [20]   | `FwdWrappingDist(int, int)`                              | wrapping distance     |
|  [21]   | `FuzzyEq(IPlineSource<T>)`                               | fuzzy equality        |
|  [22]   | `FuzzyEqEps(IPlineSource<T>, T)`                         | epsilon equality      |
|  [23]   | `IsEmpty()`                                              | emptiness             |
|  [24]   | `Last() -> PlineVertex<T>?`                              | terminal vertex       |

- `ClosestPointResult<T>`: `SegStartIndex`, `SegPoint`, `Distance`; a multi-loop nearest fold ranks on `Distance` and keeps the owning loop ordinal beside the result.

[ENTRYPOINT_SCOPE]: parallel offset — `PlineOffset`, a two-stage slice pipeline (raw untrimmed offset, then trim against the original via the AABB index) whose raw stages an adaptive-clearing walk consumes directly
- shape: static

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]       |
| :-----: | :-------------------------------------------------------------------------- | :----------------- |
|  [01]   | `ParallelOffset<O,T>(IPlineSource<T>, T, PlineOffsetOptions<T>) -> List<O>` | finished offset    |
|  [02]   | `CreateRawOffsetPolyline<O,T>(IPlineSource<T>, T, T) -> O`                  | untrimmed polyline |
|  [03]   | `CreateUntrimmedRawOffsetSegs<T>(IPlineSource<T>, T)`                       | offset primitives  |
|  [04]   | `SlicesFromRawOffset<T>(...) -> List<PlineViewData<T>>`                     | valid slices       |
|  [05]   | `PointValidForOffset<T>(...) -> bool`                                       | collision test     |

[ENTRYPOINT_SCOPE]: Boolean and containment — `PlineBoolean` / `PlineContains` / `PlineIntersects`, a slice-and-stitch pipeline keyed on `BooleanOp` operating in exact arc-space
- shape: static

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------- | :-------------------- |
|  [01]   | `PolylineBoolean<O,T>(src, src, BooleanOp, opts) -> BooleanResult<O,T>`      | finished Boolean      |
|  [02]   | `FindIntersects<T>(...) -> PlineIntersectsCollection<T>`                     | pair intersections    |
|  [03]   | `ProcessForBoolean<T>(...) -> ProcessForBooleanResult<T>`                    | intersection topology |
|  [04]   | `SliceAtIntersects<T>(...)`                                                  | intersected slices    |
|  [05]   | `PruneSlices<T>(...) -> PrunedSlices<T>`                                     | selected slices       |
|  [06]   | `StitchSlicesIntoClosedPolylines<O,T>(...) -> List<BooleanResultPline<O,T>>` | closed result loops   |
|  [07]   | `PolylineContains<T>(...) -> PlineContainsResult`                            | containment verdict   |
|  [08]   | `VisitLocalSelfIntersects<T>(...)`                                           | local scan            |
|  [09]   | `VisitGlobalSelfIntersects<T>(...)`                                          | indexed global scan   |
|  [10]   | `AllSelfIntersectsAsBasic<T>(...) -> List<PlineBasicIntersect<T>>`           | materialized scan     |

[ENTRYPOINT_SCOPE]: multi-loop shape offset — `Shape<T>` over `IndexedPolyline<T>` loops, each loop caching its own index so a repeated offset pass rebuilds none
- shape: instance unless marked; `Shape<T>` splits CCW outer from CW hole loops and holds one index over both sets
- note: `IndexedPolyline<T>` pairs a `Polyline<T>` with the index its constructor derives through `CreateApproxAabbIndex()`; both members are settable, so a mutated polyline re-binds its own index rather than minting a second carrier.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `IndexedPolyline<T>(Polyline<T>)`                                                    | ctor, approx-index pairing   |
|  [02]   | `IndexedPolyline<T>.Polyline` / `.SpatialIndex`                                      | settable loop and index      |
|  [03]   | `IndexedPolyline<T>.ParallelOffsetForShape(T, ShapeOffsetOptions<T>)`                | self-intersect-free offset   |
|  [04]   | `Shape<T>.FromPlines(IEnumerable<Polyline<T>>)`                                      | static, winding partition    |
|  [05]   | `Shape<T>.Empty()`                                                                   | static, empty shape          |
|  [06]   | `Shape<T>(List<IndexedPolyline<T>>, List<IndexedPolyline<T>>, StaticAABB2DIndex<T>)` | ctor, pre-indexed shape      |
|  [07]   | `Shape<T>.CcwPlines` / `.CwPlines` / `.PlinesIndex`                                  | outer, hole, and shape index |
|  [08]   | `Shape<T>.ParallelOffset(T, ShapeOffsetOptions<T>)`                                  | island-preserving offset     |
|  [09]   | `Shape<T>.CreateOffsetLoopsWithIndex(T, ShapeOffsetOptions<T>)`                      | raw loops plus their index   |
|  [10]   | `Shape<T>.FindIntersectsBetweenOffsetLoops(...)`                                     | inter-loop slice points      |
|  [11]   | `OffsetLoop<T>.ParentLoopIdx` / `.IndexedPline`                                      | lineage and indexed loop     |
|  [12]   | `ShapeOffsetOptions<T>(T, T, T)`                                                     | ctor, three epsilon rows     |

[ENTRYPOINT_SCOPE]: index traversal and visitor callbacks — `StaticAABB2DIndex<T>` with the two visitor contracts it dispatches on
- shape: instance; the visitor rides `ref V` and every callback returns `bool` — `true` continues the descent, `false` stops it
- note: the generic slot is `where V : struct` with NO `allows ref struct`, so a visitor is a plain `struct`; a `ref struct` visitor is a compile rejection at the call, not a lifetime choice.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------ | :------------------------ |
|  [01]   | `IQueryVisitor.Visit(int indexPos) -> bool`                         | the query callback        |
|  [02]   | `INeighborVisitor<T>.Visit(int indexPos, T distSquared) -> bool`    | the neighbor callback     |
|  [03]   | `DelegateQueryVisitor(Func<int, bool>)`                             | ctor, delegate adapter    |
|  [04]   | `VisitQuery<V>(T, T, T, T, ref V) -> bool`                          | pruned box descent        |
|  [05]   | `VisitQueryWithStack<V>(T, T, T, T, ref V, List<int>) -> bool`      | descent over a lent stack |
|  [06]   | `VisitQuery(T, T, T, T, Func<int, bool>) -> bool`                   | delegate box descent      |
|  [07]   | `VisitNeighbors<V>(T, T, ref V) -> bool`                            | nearest-first walk        |
|  [08]   | `VisitNeighborsWithQueue<V>(T, T, ref V, PriorityQueue<…>) -> bool` | walk over a lent queue    |
|  [09]   | `Query(T, T, T, T) -> List<int>` / `QueryIter(...)`                 | materialized box hits     |
|  [10]   | `Bounds` / `Count` / `NodeSize` / `ItemBoxes` / `ItemIndices`       | index shape and spans     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- each vertex is `(X, Y, Bulge)` where `Bulge = tan(theta/4)` of the arc to the next vertex: `0` straight, `> 0` sweeps CCW, `< 0` sweeps CW, `|Bulge| == 1` a semicircle. One constant-radius arc is one vertex pair — the whole reason to choose this owner over `Clipper2` for an arc-walled profile.
- instantiate `Polyline<double>` and `Vector2<double>`; the engine assumes no float width internally, so a `Half`/`float` `T` is type-legal but `double` is the fabrication rail.
- closed CCW polylines carry positive `Area()` and `CounterClockwise` `Orientation()`; the offset sign follows, so a positive offset of a CCW closed loop inflates outward. `Orientation()` returns `Open` for an open polyline.
- `WindingNumber`, `ClosestPoint`, `Area`, and `PathLength` integrate over the true arc; only `ArcsToApproxLines(errorDistance)` chord-approximates, the explicit bridge when a line-only consumer cannot accept bulge.

[LOCAL_ADMISSION]:
- build a profile as `Polyline<double>` carrying bulge straight from the `Ingress/profile` `ACadSharp` arc/`Spline` entities, one `PlineVertex` pair per arc.
- offset through the single-call `PlineOffset.ParallelOffset<Polyline<double>, double>(src, offset, options) -> List<Polyline<double>>`; a single inward offset of a concave loop returns several loops. `CreateRawOffsetPolyline` and `SlicesFromRawOffset` serve only an adaptive-clearing walk needing the untrimmed intermediate.
- difference through `PlineBoolean.PolylineBoolean<Polyline<double>, double>(p1, p2, op, options)` keyed on `BooleanOp { Or, And, Not, Xor }`; `Not` is the kerf-inflated arc-space remnant the `Nesting/nfp` `Remnant` producer consumes.
- clear a pocket with islands through `Shape<T>.FromPlines(loops).ParallelOffset(offset, ShapeOffsetOptions<T>)`, offsetting CCW outer and CW hole loops together; a per-loop `PlineOffset.ParallelOffset` loses the hole nesting. Each loop travels as an `IndexedPolyline<T>` carrying its own approximate index, so a multi-pass adaptive walk re-offsets through `ParallelOffsetForShape` with no index rebuild, and `OffsetLoop<T>.ParentLoopIdx` is the island lineage the next pass reads.
- build the `StaticAABB2DIndex<T>` once via `CreateAabbIndex()` and thread it through `PlineOffsetOptions<T>.AabbIndex` / `PlineBooleanOptions<T>.Pline1AabbIndex`, reusing it across the repeated inward offsets of an adaptive-clearing pass.
- traverse the index with a plain `readonly struct V : IQueryVisitor` whose `bool Visit(int indexPos)` returns `false` at the first blocker, so the descent stops inside the tree rather than materializing the candidates behind it; the visitor slot is `where V : struct` with no `allows ref struct`, so a `ref struct` visitor is a compile rejection and a span-carrying probe copies into owned fields first.
- lend the traversal state: `VisitQueryWithStack`/`VisitNeighborsWithQueue` take the caller's own `List<int>`/`PriorityQueue`, so one pooled stack serves every test in a pass, while the `Func<int,bool>` overload and `DelegateQueryVisitor` are the allocating convenience forms.
- sample lead-in and feed points through `FindPointAtPathLength(targetPathLength)`, whose result carries the segment index and accumulated length as true arc parameters.

[STACKING]:
- `Clipper2` (`api-clipper2`, substrate): its `PathD` Boolean and `ClipperOffset` line offset own pure-polygon clip and Minkowski-NFP; the arc/line bridge in either direction is `ArcsToApproxLines(errorDistance)` to a `PathD`, and a `PathD` result refits to arcs only when the source was line-only. Arc-walled profiles stay here and skip the refit.
- `geometry3Sharp`(`libs/csharp/.api/api-geometry3sharp.md`): `g3.BiArcFit2` refits a genuinely line-sourced path to biarcs; a bulge-carried offset skips it, so `g3.BiArcFit2` owns only that residual case.
- `Posting/program`: `PlineVertex<T>.Bulge` maps to a `G2`/`G3` arc move — center and radius derive from the vertex pair, and `Move.ArcCenter` reads straight from the segment with no refit.
- `Nesting/nfp`: `PlineBoolean` `Not` mints the kerf-inflated `Remnant` in arc-space; the remnant's bulge threads into the next pass's `StaticAABB2DIndex` placement scan.
- kernel: `Core.Vector2<double>` and `AABB<double>` boundary-map to `Rasm` `Point3d`/`Vector3d` (z-dropped) and the `Geometry2D` box at the `Polyline<double>` ⇄ `Loop` seam, bulge preserved into the `Loop` arc-segment.

[RAIL_LAW]:
- Package: `CavalierContours`
- Owns: arc-native (bulge) 2D polyline parallel offset, closed-polyline Boolean, containment and winding, closest-point, arc-aware area/path-length/extents, arc-length sampling, arc-to-line densification, the exact segment algebra and six-verdict segment-pair intersection under `PlineSeg`, and the flatbush `StaticAABB2DIndex` over open, closed, and self-intersecting polylines.
- Accept: a `Polyline<double>` carrying real `Bulge` from the `ACadSharp` arc ingest; the static `PlineOffset`/`PlineBoolean` slice pipelines with a once-built `StaticAABB2DIndex` threaded through the options; a plain-`struct` `IQueryVisitor` for the hot index loop; the `PlineSeg` primitives where a test reaches one segment.
- Reject: densifying an arc to a line fan at ingest; re-implementing offset or Boolean on `Clipper2` for an arc-walled profile; a `g3.BiArcFit2` refit of a bulge-carried path; a hand-rolled O(n²) self-intersection scan beside the `StaticAABB2DIndex`; a hand-rolled segment-pair test beside `PlineSegIntersection`, or a boolean hit predicate that erases its overlap verdicts; a non-`double` `T` on the fabrication rail; a medial-axis expectation this owner does not carry.
