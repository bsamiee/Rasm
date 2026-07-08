# [RASM_FABRICATION_API_SHARPVORONOILIB]

`SharpVoronoiLib` (RudyTheDev) is the pure-managed, MIT-licensed 2D Fortune's-sweepline point-site Voronoi engine, fronted by the stateful `VoronoiPlane` orchestrator: it tessellates a site set into a bordered diagram (`VoronoiEdge`/`VoronoiPoint`/per-site cell), CLIPS edges to a rectangular border and synthesizes the four border edges + corner points (`BorderEdgeGeneration`), runs Lloyd's RELAXATION in place (`Relax` — iterate centroid-resite + re-tessellate), MERGES adjacent cells under a caller `VoronoiSiteMergeQuery` predicate (`MergeSites`), generates random uniform/Gaussian site clouds (`GenerateRandomSites`), and answers nearest-site queries via a bundled `Supercluster.KDTree` (`GetNearestSiteTo`, `NearestSiteLookupMethod.KDTree`). The diagram is fully NAVIGABLE: each `VoronoiSite` exposes its `Edges`/`Points`, the clockwise-wound `ClockwiseEdges`/`ClockwisePoints`/`ClockwiseEdgesWound`, its `Neighbours`, `Centroid`, and `Closed` flag; each `VoronoiEdge` exposes `Start`/`End`/`Mid`, its `Left`/`Right` sites, `Length`, `Neighbours`, and `CommonPointWith`. The package is OWNED SOLELY by `Rasm.Fabrication` (`Rasm.Fabrication.csproj`, `extern alias Voronoi`); the `Rasm` geometry kernel holds NO reference (its `Rasm.csproj` is clean). This catalog serves the Fabrication CAM rail, where it is the dedicated point-site tessellation primitive behind the `Toolpath/partition` Voronoi region decomposition — Fortune sites + Lloyd relaxation, spiral-pocket seed centroids, even-spacing decomposition, and Lloyd-relaxed stipple/engrave/pen-plot paths.

`SharpVoronoiLib` is POINT-SITE ONLY: it computes the Voronoi of a set of point sites, NOT the polygon medial axis / segment Voronoi — the medial/straight-skeleton concern is the KERNEL's owner (`Meshing/offset`+`Meshing/skeleton` clearance family, K1/K2), never re-routed here (`Toolpath/skeleton` keeps only the trochoidal WALK over the kernel clearance field). It supersedes the stale `VoronoiLib 0.1.0` predecessor. The clipped-bordered 2D diagram stays inside the Fabrication CAM rail: a relaxed/region-partitioned cell set feeds `Clipper2`-clipped pocketing toolpaths, and `CavalierContours` arc-native offsetting of the cell boundaries — `SharpVoronoiLib` produces the partition, the offset/Boolean substrate machines it. Distinct from the kernel's `MIConvexHull` (the N-D Voronoi-as-Delaunay-dual, unbordered) and `Triangle`'s `StandardVoronoi`/`BoundedVoronoi` (the Voronoi dual of a constrained mesh): `SharpVoronoiLib` is the standalone, border-clipped, relaxable 2D point-site diagram — the only one of the three with native edge clipping + Lloyd relaxation + cell merging.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpVoronoiLib`
- package: `SharpVoronoiLib`
- license: `MIT` ("MIT License, various authors"; `licenseUrl` `github.com/RudyTheDev/SharpVoronoiLib/blob/main/License.md`) — NOT ISC
- assembly: `SharpVoronoiLib`
- namespaces: `SharpVoronoiLib` (the diagram + orchestrator + strategies), `SharpVoronoiLib.Exceptions` (the typed faults), `Supercluster.KDTree` + `Supercluster.KDTree.Utilities` (the BUNDLED kd-tree powering `NearestSiteLookupMethod.KDTree` — vendored INTO this assembly, not a separate dependency)
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe, osx-arm64-safe)
- frameworks: multi-target `net10.0` + `net9.0` + `netstandard2.1` + `netstandard2.0` (four `lib/` assets); the `net10.0` consumer binds the REAL `lib/net10.0/SharpVoronoiLib.dll` — the `[API_TFM_RESOLUTION]` hazard does NOT apply (the consumer-bound TFM is the primary, and the `netstandard2.0` surface is verified IDENTICAL, so even a fallback resolve carries no signature drift)
- dependencies: NONE (the kd-tree is vendored in-assembly) — zero transitive closure
- owner: `Rasm.Fabrication.csproj` SOLE (`extern alias Voronoi`); the kernel `Rasm.csproj` holds no reference — the point-site diagram is Fabrication-only, the polygon medial axis stays the kernel's
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the orchestrator and the diagram graph
- rail: fabrication

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :------------------ | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `VoronoiPlane`      | stateful orchestrator | the primary entry: holds `MinX`/`MinY`/`MaxX`/`MaxY` border + `Sites`/`Edges`/`Points` lists + `DuplicateCount`; drives `SetSites`/`GenerateRandomSites` -> `Tessellate` -> `Relax`/`MergeSites`/`GetNearestSiteTo` |
|  [02]   | `VoronoiSite`       | cell / input site   | `: IEquatable` — both the INPUT seed (ctor `(x, y)`) and the OUTPUT cell: `X`/`Y`, `Edges`, `ClockwiseEdges`, `ClockwiseEdgesWound` (`WoundVoronoiEdge`), `Points`, `ClockwisePoints`, `Neighbours`, `Centroid`, `Closed`, `LiesOnEdge`/`LiesOnCorner` (degenerate-site flags), `Contains(x,y)` |
|  [03]   | `VoronoiEdge`       | cell boundary edge  | `: IEquatable` — `VoronoiPoint Start`/`End`/`Mid`, `VoronoiSite? Left`/`Right` (the two sites this edge separates; null on a border edge's outer side), `Length`, `Neighbours`, `CommonPointWith(other)` (the shared vertex of two edges) |
|  [04]   | `VoronoiPoint`      | vertex              | `: IEquatable` — `double X`/`Y`, `PointBorderLocation BorderLocation` (which border the vertex sits on, or `NotOnBorder`), `Sites` (the cells meeting at this vertex) |
|  [05]   | `WoundVoronoiEdge`  | wound edge          | `readonly struct` — `VoronoiEdge Edge` + `bool Flipped`, with `Start`/`End` re-oriented to the consistent clockwise winding; the element of `VoronoiSite.ClockwiseEdgesWound` for building a closed CW cell polygon directly |

[PUBLIC_TYPE_SCOPE]: tessellation / generation / lookup vocabulary (enums)
- rail: fabrication

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                                  |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `BorderEdgeGeneration`    | tessellate enum | `DoNotMakeBorderEdges` (open cells at the border, no synthesized edges) / `MakeBorderEdges` (clip cells to the border rect AND synthesize the four border edges + corner points — closed cells) |
|  [02]   | `PointGenerationMethod`   | random enum   | `Uniform` / `Gaussian` — the `GenerateRandomSites` distribution                              |
|  [03]   | `NearestSiteLookupMethod` | lookup enum   | `BruteForce` (O(n) scan) / `KDTree` (the bundled `Supercluster.KDTree`, O(log n) — the default for `GetNearestSiteTo`) |
|  [04]   | `PointBorderLocation`     | border enum   | `NotOnBorder` (-1), then `BottomLeft`, `Left`, `TopLeft`, `Top`, `TopRight`, `Right`, `BottomRight`, `Bottom` — `VoronoiPoint.BorderLocation`, the clockwise border-position classification |
|  [05]   | `VoronoiSiteMergeDecision`| merge enum    | `DontMerge` / `MergeIntoSite1` / `MergeIntoSite2` — the return of a `VoronoiSiteMergeQuery` |

[PUBLIC_TYPE_SCOPE]: pluggable strategies — generation, RNG, merging, nearest-site
- rail: fabrication
- note: the strategy interfaces let a consumer inject custom site generation, determinism (seeded RNG for reproducible toolpaths), or cell-merge logic. The merge-algorithm interface itself (`ISiteMergingAlgorithm`) is INTERNAL — only the concrete `GenericSiteMergingAlgorithm` is public; merging is normally driven through `VoronoiPlane.MergeSites(VoronoiSiteMergeQuery)`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `IPointGenerationAlgorithm`       | generation contract | the custom site-cloud generator `GenerateRandomSites(amount, algorithm, random)` accepts |
|  [02]   | `IRandomNumberGenerator`          | RNG contract       | the determinism seam — `double NextDouble()`                               |
|  [03]   | `SeededRandomNumberGenerator`     | RNG impl           | `(int seed)` — the REPRODUCIBLE generator for deterministic stipple/engrave site clouds |
|  [04]   | `ProvidedRandomNumberGenerator`   | RNG impl           | `(Random random)` — wraps a supplied `System.Random`                       |
|  [05]   | `VoronoiSiteMergeQuery` (delegate)| merge predicate    | `VoronoiSiteMergeDecision VoronoiSiteMergeQuery(VoronoiSite site1, VoronoiSite site2)` — the per-adjacent-pair merge decision `MergeSites` folds over |
|  [06]   | `GenericSiteMergingAlgorithm`     | merge impl         | `MergeSites(List<VoronoiSite> sites, List<VoronoiEdge> edges, VoronoiSiteMergeQuery mergeQuery)` — the standalone merge engine (the `VoronoiPlane.MergeSites` shorthand wraps it) |
|  [07]   | `INearestSiteLookup` / `BruteForceNearestSiteLookup` / `KDTreeNearestSiteLookup` | lookup contract+impls | the nearest-site strategy behind `GetNearestSiteTo` |
|  [08]   | `VoronoiEdgeComparer` / `VoronoiPointComparer` / `VoronoiSiteComparer` | equality comparers | `IEqualityComparer` singletons (`.Instance`) for dedup/set membership of edges/points/sites |
|  [09]   | `VoronoiLibValues`                | constant           | `const double epsilon = 1E-12` — the package's coordinate-comparison epsilon |

[PUBLIC_TYPE_SCOPE]: typed faults — `SharpVoronoiLib.Exceptions`
- rail: fabrication

| [INDEX] | [SYMBOL]                                          | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | `VoronoiNotTessellatedException`                  | thrown when a navigation property is read before `Tessellate` ran      |
|  [02]   | `VoronoiDoesntHaveSitesException`                 | thrown when `Tessellate` runs with no sites set                        |
|  [03]   | `VoronoiSiteNotClosedException`                   | thrown when a closed-cell operation hits an open (unbordered) cell     |
|  [04]   | `VoronoiSiteSkippedAsDuplicateException`          | a coincident-site skip (also reflected in `VoronoiPlane.DuplicateCount`) |
|  [05]   | `VoronoiRandomPointGenerationEncounteredTooManyInvalidSites` / `VoronoiSiteGenerationProducedNull` / `VoronoiSiteGenerationProducedWrongCount` | random/custom-generation contract faults |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the stateful pipeline — `VoronoiPlane`
- rail: fabrication
- note: the canonical flow is `new VoronoiPlane(minX, minY, maxX, maxY)` -> `SetSites(sites)` or `GenerateRandomSites(...)` -> `Tessellate(BorderEdgeGeneration)` -> read `Sites`/`Edges`/`Points` and navigate each cell -> optionally `Relax(...)` / `MergeSites(...)`. Navigation properties throw `VoronoiNotTessellatedException` before `Tessellate`.

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `VoronoiPlane(double minX, double minY, double maxX, double maxY)`                                                | constructor    | bind the diagram border rectangle (the clip extent + the synthesized border) |
|  [02]   | `SetSites(List<VoronoiSite> sites)`                                                                                | input          | set the point-site seeds explicitly                                          |
|  [03]   | `GenerateRandomSites(int amount, PointGenerationMethod method = Uniform, IRandomNumberGenerator? random = null)`   | input          | generate a random uniform/Gaussian site cloud (pass `SeededRandomNumberGenerator` for reproducibility) |
|  [04]   | `GenerateRandomSites(int amount, IPointGenerationAlgorithm algorithm, IRandomNumberGenerator? random = null)`      | input          | generate via a custom generation strategy                                    |
|  [05]   | `Tessellate(BorderEdgeGeneration borderGeneration = MakeBorderEdges)`                                              | tessellate     | run the Fortune sweep, clip to the border, synthesize border edges/corners; returns `List<VoronoiEdge>` and populates `Sites`/`Points` |
|  [06]   | `Relax(int iterations = 1, float strength = 1f, bool reTessellate = true)`                                         | relaxation     | Lloyd's relaxation: move each site toward its cell `Centroid` by `strength`, optionally re-tessellate per iteration — the even-spacing / stipple regularizer |
|  [07]   | `MergeSites(VoronoiSiteMergeQuery mergeQuery)`                                                                     | merge          | fold the merge predicate over adjacent cell pairs, merging per `VoronoiSiteMergeDecision` — region coalescing/region-growing |
|  [08]   | `GetNearestSiteTo(double x, double y, NearestSiteLookupMethod lookupMethod = KDTree)`                              | query          | nearest-cell lookup via the bundled kd-tree (or brute force) — point-to-region assignment |
|  [09]   | `static TessellateOnce(List<VoronoiSite> sites, minX, minY, maxX, maxY, BorderEdgeGeneration = MakeBorderEdges)`   | one-shot       | the stateless one-call tessellation of an explicit site set                  |
|  [10]   | `static TessellateRandomSitesOnce(int numberOfSites, minX, minY, maxX, maxY, BorderEdgeGeneration = MakeBorderEdges)` | one-shot    | the stateless one-call random-site tessellation                              |

[ENTRYPOINT_SCOPE]: cell navigation — `VoronoiSite` / `VoronoiEdge`
- rail: fabrication
- note: after `Tessellate`, walk each `VoronoiSite` as a closed cell polygon for toolpath/region generation.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :----------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `VoronoiSite.ClockwisePoints`              | cell polygon   | the cell's boundary vertices in clockwise order — the closed-polygon ring for a pocketing/region contour |
|  [02]   | `VoronoiSite.ClockwiseEdgesWound`          | cell edges     | the cell's edges as `WoundVoronoiEdge` (consistently CW-oriented) — the closed edge cycle without re-deriving winding |
|  [03]   | `VoronoiSite.Centroid` / `Contains(x,y)`   | cell geometry  | the cell centroid (the Lloyd target / spiral-pocket seed) and the point-in-cell test |
|  [04]   | `VoronoiSite.Neighbours`                   | adjacency      | the adjacent cells (sharing an edge) — region-adjacency graph for traversal-order sequencing |
|  [05]   | `VoronoiSite.Closed`                       | cell status    | true when the cell is fully bounded (border-clipped or interior); false on an open border cell under `DoNotMakeBorderEdges` |
|  [06]   | `VoronoiEdge.Start`/`End`/`Mid`/`Length`/`Left`/`Right`/`CommonPointWith` | edge geometry | the edge endpoints/midpoint/length, the two separated sites, and the shared-vertex query — the medial-segment data for even-spacing decomposition |

## [04]-[IMPLEMENTATION_LAW]

[DIAGRAM_TOPOLOGY]:
- Coordinates are `double`; the engine is a FLOATING-POINT Fortune sweep with `epsilon = 1E-12` coincidence tolerance. Coincident sites are SKIPPED (counted in `VoronoiPlane.DuplicateCount`, optionally surfacing `VoronoiSiteSkippedAsDuplicateException`), so the consumer pre-dedups or reads `DuplicateCount` to reconcile the input/output site counts.
- `BorderEdgeGeneration.MakeBorderEdges` is the DEFAULT and the one the CAM rail wants: it clips every cell to the `[MinX,MinY]..[MaxX,MaxY]` rectangle and synthesizes the four border edges + corner `VoronoiPoint`s so EVERY cell is a closed polygon (`Closed == true`) directly machinable. `DoNotMakeBorderEdges` leaves boundary cells open (infinite-ray cells truncated, no synthesized border) — use it only when the border is handled downstream.
- `VoronoiPoint.BorderLocation` classifies each vertex's border position (`NotOnBorder` for interior vertices, else the clockwise `BottomLeft..Bottom` slot); a cell's `ClockwisePoints` interleaves interior Voronoi vertices and border/corner vertices into the closed ring.
- `Relax` mutates the plane IN PLACE (re-sites toward centroids and, by default, re-tessellates each iteration); capture the returned edge list per iteration if intermediate states matter. `strength` in [0,1] damps the move (1 = full centroid snap).

[STACKING_LAW]:
- Feeds the Fabrication CAM substrate: a tessellated/relaxed/region-merged cell set is the PARTITION; the per-cell `ClockwisePoints` ring becomes a `Clipper2` `Path64`/`PathD` (scale to int64 at the owner `Precision.Digits`) for pocketing-toolpath Boolean/clip, or a `CavalierContours` polyline for arc-native offsetting of the cell boundaries. `SharpVoronoiLib` produces the regions; the offset/Boolean owners machine them. Spiral-pocket toolpaths seed at `VoronoiSite.Centroid`; even-spacing engrave decomposes along `VoronoiEdge` medial segments; Lloyd-relaxed (`Relax`) sites give regular stipple/pen-plot point fields.
- POINT-SITE BOUNDARY: `SharpVoronoiLib` does NOT compute the polygon medial axis / segment Voronoi — given a polygon, it Voronois the polygon's VERTICES, not its EDGES. The straight-skeleton / segment-Voronoi concern stays in the in-folder `Toolpath/skeleton` author-kernel; never approximate the medial axis by sampling polygon edges into point sites here.
- Disjoint from the kernel `MIConvexHull` and `Triangle` Voronoi: `MIConvexHull.CreateVoronoi` is the N-D Voronoi-as-Delaunay-dual with NO border clipping and NO Lloyd relaxation. `Triangle`'s `StandardVoronoi`/`BoundedVoronoi` is the Voronoi dual OF a constrained Delaunay mesh (half-edge DCEL). `SharpVoronoiLib` is the only owner of the standalone, BORDER-CLIPPED, RELAXABLE, MERGEABLE 2D point-site diagram with native nearest-site query — the CAM tessellation primitive. A region partition needing border closure + relaxation uses this; a Voronoi dual of an existing mesh uses `Triangle`'s.

[RAIL_LAW]:
- Package: `SharpVoronoiLib` (assembly `SharpVoronoiLib`, with a vendored `Supercluster.KDTree`)
- Owns: the 2D Fortune's-sweepline point-site Voronoi diagram with border clipping + synthesized border edges (`Tessellate`/`BorderEdgeGeneration`), Lloyd's relaxation (`Relax`), cell merging under a caller predicate (`MergeSites`/`VoronoiSiteMergeQuery`), random uniform/Gaussian site generation with a seeded-RNG determinism seam, kd-tree nearest-site query (`GetNearestSiteTo`), and the fully-navigable cell/edge/vertex graph — all in the `double` domain
- Accept: the CAM tessellation concern — toolpath region partitioning, spiral-pocket seed centroids, even-spacing region decomposition, and Lloyd-relaxed stipple/engrave/pen-plot site fields, with cells handed to `Clipper2`/`CavalierContours` for machining
- Reject: any polygon medial-axis / segment-Voronoi / straight-skeleton use (point-site only — the medial concern is the KERNEL's `Meshing/offset`+`Meshing/skeleton` owner, K1/K2); reaching for the stale `VoronoiLib 0.1.0` predecessor (superseded); duplicating the N-D Voronoi-dual (`MIConvexHull` owns it) or the constrained-mesh Voronoi dual (`Triangle`'s `StandardVoronoi`/`BoundedVoronoi` owns it); reading a `VoronoiSite`/`VoronoiEdge`/`VoronoiPoint` navigation property before `Tessellate` (throws `VoronoiNotTessellatedException`); and citing the license as ISC (it is MIT)
