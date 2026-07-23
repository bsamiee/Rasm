# [RASM_FABRICATION_API_SHARPVORONOILIB]

`SharpVoronoiLib` owns the pure-managed 2D Fortune's-sweepline point-site Voronoi engine behind the stateful `VoronoiPlane`: it tessellates a site set into a border-clipped diagram, runs in-place Lloyd relaxation and predicate cell merging, generates uniform or Gaussian site clouds, and answers nearest-site queries through a vendored kd-tree. It Voronois point sites alone — never the polygon medial axis or segment Voronoi — and feeds the Fabrication CAM partition rail, decomposing a stock region into closed machinable cells for `Clipper2` and `CavalierContours` offsetting.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpVoronoiLib`
- package: `SharpVoronoiLib` (MIT)
- assembly: `SharpVoronoiLib`
- namespaces: `SharpVoronoiLib`, `SharpVoronoiLib.Exceptions`, `Supercluster.KDTree`, `Supercluster.KDTree.Utilities`
- asset: pure-managed AnyCPU IL, multi-target `net10.0`/`net9.0`/`netstandard2.1`/`netstandard2.0`; the consumer binds `lib/net10.0/SharpVoronoiLib.dll`. Zero package dependencies — the `Supercluster.KDTree` behind `NearestSiteLookupMethod.KDTree` is vendored in-assembly
- owner: `Rasm.Fabrication.csproj` (`extern alias Voronoi`)
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the orchestrator and the navigable diagram graph

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]         | [CAPABILITY]               |
| :-----: | :----------------- | :-------------------- | :------------------------- |
|  [01]   | `VoronoiPlane`     | stateful orchestrator | bordered diagram lifecycle |
|  [02]   | `VoronoiSite`      | site and cell         | navigable cell graph       |
|  [03]   | `VoronoiEdge`      | boundary edge         | cell boundary topology     |
|  [04]   | `VoronoiPoint`     | vertex                | border-classified vertex   |
|  [05]   | `WoundVoronoiEdge` | readonly struct       | clockwise cell traversal   |

- `VoronoiPlane` read-state: `MinX` `MinY` `MaxX` `MaxY` `Sites` `Edges` `Points` `DuplicateCount` — a navigation property throws before `Tessellate`.
- `VoronoiSite` (`IEquatable`, ctor `(double, double)` seeds one cell): `X` `Y` `Edges` `Points` `Centroid` `Closed` `Contains(double,double)` `ClockwiseEdges` `ClockwiseEdgesWound` `ClockwisePoints` `Neighbours` `LiesOnEdge` `LiesOnCorner`.
- `VoronoiEdge` (`IEquatable`; a border edge carries one null outer site): `Start` `End` `Mid` `Length` `Left` `Right` `Neighbours` `CommonPointWith(VoronoiEdge)`.
- `VoronoiPoint` (`IEquatable`): `X` `Y` `BorderLocation` `Sites` `Edges`.
- `WoundVoronoiEdge` orients an edge clockwise for `VoronoiSite.ClockwiseEdgesWound`: `Edge` `Flipped` `Start` `End`.

[PUBLIC_TYPE_SCOPE]: tessellation, generation, and lookup vocabulary

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]        |
| :-----: | :------------------------- | :-------------- | :------------------ |
|  [01]   | `BorderEdgeGeneration`     | tessellate enum | border construction |
|  [02]   | `PointGenerationMethod`    | random enum     | site distribution   |
|  [03]   | `NearestSiteLookupMethod`  | lookup enum     | lookup strategy     |
|  [04]   | `PointBorderLocation`      | border enum     | border position     |
|  [05]   | `VoronoiSiteMergeDecision` | merge enum      | merge direction     |

- `BorderEdgeGeneration`: `MakeBorderEdges` `DoNotMakeBorderEdges` — `MakeBorderEdges` (default) clips every cell and synthesizes the four border edges and corner points; `DoNotMakeBorderEdges` leaves boundary cells open.
- `PointGenerationMethod`: `Uniform` `Gaussian` — the `GenerateRandomSites` distribution.
- `NearestSiteLookupMethod`: `KDTree` `BruteForce` — `KDTree` (default) queries the vendored index in O(log n); `BruteForce` scans O(n).
- `PointBorderLocation`: `NotOnBorder` (-1) `BottomLeft` `Left` `TopLeft` `Top` `TopRight` `Right` `BottomRight` `Bottom` — the clockwise `VoronoiPoint.BorderLocation` classification.
- `VoronoiSiteMergeDecision`: `DontMerge` `MergeIntoSite1` `MergeIntoSite2` — the `VoronoiSiteMergeQuery` verdict.

[PUBLIC_TYPE_SCOPE]: pluggable strategies — generation, RNG, merging, nearest-site

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]         | [CAPABILITY]       |
| :-----: | :------------------------------ | :-------------------- | :----------------- |
|  [01]   | `IPointGenerationAlgorithm`     | generation contract   | custom site clouds |
|  [02]   | `IRandomNumberGenerator`        | RNG contract          | deterministic draw |
|  [03]   | `SeededRandomNumberGenerator`   | RNG implementation    | seeded draws       |
|  [04]   | `ProvidedRandomNumberGenerator` | RNG implementation    | supplied RNG       |
|  [05]   | `VoronoiSiteMergeQuery`         | merge delegate        | pairwise decision  |
|  [06]   | `GenericSiteMergingAlgorithm`   | merge implementation  | standalone merging |
|  [07]   | `INearestSiteLookup`            | lookup contract       | nearest-site query |
|  [08]   | `BruteForceNearestSiteLookup`   | lookup implementation | linear lookup      |
|  [09]   | `KDTreeNearestSiteLookup`       | lookup implementation | indexed lookup     |
|  [10]   | `VoronoiEdgeComparer`           | equality comparer     | edge equality      |
|  [11]   | `VoronoiPointComparer`          | equality comparer     | point equality     |
|  [12]   | `VoronoiSiteComparer`           | equality comparer     | site equality      |
|  [13]   | `VoronoiLibValues`              | constant owner        | coordinate epsilon |

- `IPointGenerationAlgorithm.Generate(double, double, double, double, int, IRandomNumberGenerator?)` returns `List<VoronoiSite>`, backing the custom-generator `GenerateRandomSites` overload.
- `IRandomNumberGenerator.NextDouble()` is the determinism seam; `SeededRandomNumberGenerator(int)` yields reproducible stipple/engrave clouds and `ProvidedRandomNumberGenerator(Random)` wraps a supplied `System.Random`.
- `VoronoiSiteMergeQuery` is the delegate `VoronoiSiteMergeDecision(VoronoiSite, VoronoiSite)` deciding each adjacent pair; `GenericSiteMergingAlgorithm.MergeSites(List<VoronoiSite>, List<VoronoiEdge>, VoronoiSiteMergeQuery)` is the standalone form `VoronoiPlane.MergeSites` wraps, the `ISiteMergingAlgorithm` contract staying internal.
- `VoronoiEdgeComparer`, `VoronoiPointComparer`, and `VoronoiSiteComparer` expose `IEqualityComparer` singletons through `.Instance`.
- `VoronoiLibValues.epsilon` is `1E-12`, the coordinate-comparison tolerance.

[PUBLIC_TYPE_SCOPE]: typed faults — `SharpVoronoiLib.Exceptions`

| [INDEX] | [SYMBOL]                                                     | [FAULT]                     |
| :-----: | :----------------------------------------------------------- | :-------------------------- |
|  [01]   | `VoronoiNotTessellatedException`                             | pre-tessellation navigation |
|  [02]   | `VoronoiDoesntHaveSitesException`                            | empty tessellation input    |
|  [03]   | `VoronoiSiteNotClosedException`                              | open-cell operation         |
|  [04]   | `VoronoiSiteSkippedAsDuplicateException`                     | coincident-site skip        |
|  [05]   | `VoronoiRandomPointGenerationEncounteredTooManyInvalidSites` | generation exhaustion       |
|  [06]   | `VoronoiSiteGenerationProducedNull`                          | null generator output       |
|  [07]   | `VoronoiSiteGenerationProducedWrongCount`                    | generator count mismatch    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the stateful `VoronoiPlane` pipeline — construct on the clip rectangle, seed via `SetSites`/`GenerateRandomSites`, `Tessellate`, then `Relax`/`MergeSites`/`GetNearestSiteTo`; a navigation property throws `VoronoiNotTessellatedException` before `Tessellate`. Every method below returns a `List<VoronoiEdge>` (`MergeSites`/`SetSites` return the site list); `TessellateOnce`/`TessellateRandomSitesOnce` take the border as four `minX, minY, maxX, maxY` doubles.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `VoronoiPlane(double, double, double, double)`               | ctor     | bind the clip/border rect    |
|  [02]   | `SetSites(List<VoronoiSite>)`                                | instance | set explicit point seeds     |
|  [03]   | `GenerateRandomSites(int, PointGenerationMethod)`            | instance | uniform/Gaussian site cloud  |
|  [04]   | `GenerateRandomSites(int, IPointGenerationAlgorithm)`        | instance | custom-generator site cloud  |
|  [05]   | `Tessellate(BorderEdgeGeneration)`                           | instance | Fortune sweep + border clip  |
|  [06]   | `Relax(int, float, bool)`                                    | instance | in-place Lloyd relaxation    |
|  [07]   | `MergeSites(VoronoiSiteMergeQuery)`                          | instance | predicate-driven cell merge  |
|  [08]   | `GetNearestSiteTo(double, double, NearestSiteLookupMethod)`  | instance | assign nearest site          |
|  [09]   | `TessellateOnce(List<VoronoiSite>, …, BorderEdgeGeneration)` | static   | one-shot explicit tessellate |
|  [10]   | `TessellateRandomSitesOnce(int, …, BorderEdgeGeneration)`    | static   | one-shot random tessellate   |

- `GenerateRandomSites` both overloads accept a trailing `IRandomNumberGenerator?` (default null); a `SeededRandomNumberGenerator` makes the cloud reproducible.
- `Relax(iterations, strength, reTessellate)` moves each site toward `Centroid` by `strength` in (0,1] (1 = full centroid snap) and, by default, re-tessellates each iteration.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Coordinates are `double`; the engine is a floating-point Fortune sweep with `1E-12` coincidence tolerance. Coincident sites skip, counting into `VoronoiPlane.DuplicateCount` (optionally surfacing `VoronoiSiteSkippedAsDuplicateException`), so a consumer pre-dedups or reads `DuplicateCount` to reconcile input against output site counts.
- `MakeBorderEdges` clips every cell to the `[MinX,MinY]..[MaxX,MaxY]` rectangle and synthesizes the four border edges and corner `VoronoiPoint`s, closing every cell (`Closed == true`) into a directly machinable polygon; `DoNotMakeBorderEdges` truncates infinite-ray cells and leaves boundary cells open, reserved for a downstream border owner.
- `VoronoiPoint.BorderLocation` classifies each vertex (`NotOnBorder` interior, else the clockwise `BottomLeft..Bottom` slot); a cell's `ClockwisePoints` interleaves interior Voronoi vertices and border/corner vertices into one closed ring.
- `Relax` mutates the plane in place and re-tessellates by default; capture the returned edge list per iteration when an intermediate state matters.

[STACKING]:
- `Clipper2` (`api-clipper2`): each `ClockwisePoints` ring becomes a `Path64`/`PathD` scaled to int64 at `Precision.Digits` for clipped pocketing Boolean and offset.
- `CavalierContours` (`api-cavaliercontours`): a cell ring becomes a bulge `Polyline<double>` for arc-native offsetting of the cell boundary.
- Fabrication CAM partition: a tessellated, relaxed, or merged cell set is the toolpath partition — `VoronoiSite.Centroid` seeds spiral-pocket toolpaths, `VoronoiEdge` medial segments drive even-spacing engrave decomposition, and Lloyd-relaxed sites lay regular stipple, engrave, and pen-plot fields.

[LOCAL_ADMISSION]:
- A region partition needing border closure and relaxation binds `SharpVoronoiLib`; given a polygon it Voronois the vertices, so an edge sampled into point sites to fake a medial axis is a defect — the medial-axis, straight-skeleton, and segment-Voronoi concern stays the kernel's `Meshing/offset`+`Meshing/skeleton` clearance family (K1/K2) that `Toolpath/skeleton` walks.
- `MIConvexHull.CreateVoronoi` binds the unbordered N-D Voronoi-as-Delaunay dual; `Triangle.StandardVoronoi`/`BoundedVoronoi` bind the half-edge Voronoi dual of a constrained mesh.

[RAIL_LAW]:
- Package: `SharpVoronoiLib`
- Owns: the border-clipped, relaxable, mergeable 2D point-site Voronoi diagram — Fortune tessellation with synthesized border edges, Lloyd `Relax`, predicate `MergeSites`, uniform/Gaussian generation with a seeded-RNG determinism seam, kd-tree `GetNearestSiteTo`, and the fully-navigable cell/edge/vertex graph, all in `double`
- Accept: the CAM tessellation concern — toolpath region partitioning, spiral-pocket seed centroids, even-spacing region decomposition, and Lloyd-relaxed site fields, cells handed to `Clipper2`/`CavalierContours` for machining
- Reject: sampling polygon edges into point sites to approximate a medial axis; duplicating the N-D Voronoi dual (`MIConvexHull` owns it) or the constrained-mesh dual (`Triangle` owns it); reading a `VoronoiSite`/`VoronoiEdge`/`VoronoiPoint` navigation property before `Tessellate`
