# [RASM_FABRICATION_API_CAVALIERCONTOURS]

`CavalierContours` owns arc-native (bulge) 2D polyline algebra — offset, closed-polyline Boolean, containment, measure, and spatial indexing. A circular arc rides as one `PlineVertex<T>` pair carrying `Bulge = tan(theta/4)`, so the offset and Boolean engine runs in exact arc-space where the line-only `Clipper2` (`api-clipper2`) cannot. It produces the kerf, lead-arc, and morphed-spiral adaptive-clearing paths in arc-space, retiring the post-hoc `Clipper2`-offset-then-`g3.BiArcFit2`-refit on the `Toolpath` and `Posting` arc rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CavalierContours`
- package: `CavalierContours` (`ISC`, oberbichler)
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
- note: `StaticAABB2DIndex<T>` is a flatbush packed-Hilbert R-tree built once from a polyline's segment AABBs; the offset and Boolean engines consume it to prune the self-intersection scan. `Core` structs are the value-type math floor.

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
- note: `Shape<T>` is the multi-loop owner — CCW outer loops and CW hole loops offset together with island topology preserved, the form a pocket-with-islands clearing toolpath needs.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]    |
| :-----: | :---------------------- | :--------------- | :-------------- |
|  [01]   | `Shape<T>`              | multi-loop owner | topology offset |
|  [02]   | `OffsetLoop<T>`         | loop carrier     | parent lineage  |
|  [03]   | `ShapeOffsetOptions<T>` | options record   | offset policy   |
|  [04]   | `IndexedPolyline<T>`    | indexed loop     | cached polyline |

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
|  [21]   | `PlineSeg.SegMidpoint`                            | static   | exact arc midpoint           |
|  [22]   | `PlineSeg.SegTangentVector`                       | static   | exact arc tangent            |
|  [23]   | `PlineSeg.SegArcRadiusAndCenter`                  | static   | exact arc frame              |

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- each vertex is `(X, Y, Bulge)` where `Bulge = tan(theta/4)` of the arc to the next vertex: `0` straight, `> 0` sweeps CCW, `< 0` sweeps CW, `|Bulge| == 1` a semicircle. One constant-radius arc is one vertex pair — the whole reason to choose this owner over `Clipper2` for an arc-walled profile.
- instantiate `Polyline<double>` and `Vector2<double>`; the engine assumes no float width internally, so a `Half`/`float` `T` is type-legal but `double` is the fabrication rail.
- a closed CCW polyline has positive `Area()` and `CounterClockwise` `Orientation()`; the offset sign follows, so a positive offset of a CCW closed loop inflates outward. `Orientation()` returns `Open` for an open polyline.
- `WindingNumber`, `ClosestPoint`, `Area`, and `PathLength` integrate over the true arc; only `ArcsToApproxLines(errorDistance)` chord-approximates, the explicit bridge when a line-only consumer cannot accept bulge.

[LOCAL_ADMISSION]:
- build a profile as `Polyline<double>` carrying bulge straight from the `Ingress/profile` `ACadSharp` arc/`Spline` entities, one `PlineVertex` pair per arc.
- offset through the single-call `PlineOffset.ParallelOffset<Polyline<double>, double>(src, offset, options) -> List<Polyline<double>>`; a single inward offset of a concave loop returns several loops. `CreateRawOffsetPolyline` and `SlicesFromRawOffset` serve only an adaptive-clearing walk needing the untrimmed intermediate.
- difference through `PlineBoolean.PolylineBoolean<Polyline<double>, double>(p1, p2, op, options)` keyed on `BooleanOp { Or, And, Not, Xor }`; `Not` is the kerf-inflated arc-space remnant the `Nesting/nfp` `Remnant` producer consumes.
- clear a pocket with islands through `Shape<T>.FromPlines(loops).ParallelOffset(offset, ShapeOffsetOptions<T>)`, offsetting CCW outer and CW hole loops together; a per-loop `PlineOffset.ParallelOffset` loses the hole nesting.
- build the `StaticAABB2DIndex<T>` once via `CreateAabbIndex()` and thread it through `PlineOffsetOptions<T>.AabbIndex` / `PlineBooleanOptions<T>.Pline1AabbIndex`, reusing it across the repeated inward offsets of an adaptive-clearing pass.
- traverse the index with a `ref struct V : IQueryVisitor` (`VisitQuery<V>`/`VisitNeighbors<V>`) inside the offset and nesting sweep, where the query runs per candidate position; the `Func<int,bool>` overload is the convenience form.
- sample lead-in and feed points through `FindPointAtPathLength(targetPathLength)`, whose result carries the segment index and accumulated length as true arc parameters.

[STACKING]:
- `Clipper2` (`api-clipper2`, substrate): its `PathD` Boolean and `ClipperOffset` line offset own pure-polygon clip and Minkowski-NFP; the arc/line bridge in either direction is `ArcsToApproxLines(errorDistance)` to a `PathD`, and a `PathD` result refits to arcs only when the source was line-only. An arc-walled profile stays here and skips the refit.
- `geometry3Sharp` (`api-geometry3sharp`): `g3.BiArcFit2` refits a genuinely line-sourced path to biarcs; a bulge-carried offset skips it, so `g3.BiArcFit2` owns only that residual case.
- `SharpVoronoiLib` (`api-sharpvoronoilib`): owns the point-Voronoi spine; this surface consumes the `Toolpath/skeleton` spine to drive the morphed-spiral offset and computes no medial axis.
- `Posting/program`: `PlineVertex<T>.Bulge` maps to a `G2`/`G3` arc move — center and radius derive from the vertex pair, and `Move.ArcCenter` reads straight from the segment with no refit.
- `Nesting/nfp`: `PlineBoolean` `Not` mints the kerf-inflated `Remnant` in arc-space; the remnant's bulge threads into the next pass's `StaticAABB2DIndex` placement scan.
- kernel: `Core.Vector2<double>` and `AABB<double>` boundary-map to `Rasm` `Point3d`/`Vector3d` (z-dropped) and the `Geometry2D` box at the `Polyline<double>` ⇄ `Loop` seam, bulge preserved into the `Loop` arc-segment.

[RAIL_LAW]:
- Package: `CavalierContours`
- Owns: arc-native (bulge) 2D polyline parallel offset, closed-polyline Boolean, containment and winding, closest-point, arc-aware area/path-length/extents, arc-length sampling, arc-to-line densification, and the flatbush `StaticAABB2DIndex` over open, closed, and self-intersecting polylines.
- Accept: a `Polyline<double>` carrying real `Bulge` from the `ACadSharp` arc ingest; the static `PlineOffset`/`PlineBoolean` slice pipelines with a once-built `StaticAABB2DIndex` threaded through the options; the `ref struct` `IQueryVisitor` for the hot index loop.
- Reject: densifying an arc to a line fan at ingest; re-implementing offset or Boolean on `Clipper2` for an arc-walled profile; a `g3.BiArcFit2` refit of a bulge-carried path; a hand-rolled O(n²) self-intersection scan beside the `StaticAABB2DIndex`; a non-`double` `T` on the fabrication rail; a medial-axis expectation this owner does not carry.
