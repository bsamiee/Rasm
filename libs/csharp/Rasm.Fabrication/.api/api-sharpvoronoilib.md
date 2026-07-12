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

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]         | [CAPABILITY]               |
| :-----: | :----------------- | :-------------------- | :------------------------- |
|  [01]   | `VoronoiPlane`     | stateful orchestrator | bordered diagram lifecycle |
|  [02]   | `VoronoiSite`      | site and cell         | navigable cell graph       |
|  [03]   | `VoronoiEdge`      | boundary edge         | cell boundary topology     |
|  [04]   | `VoronoiPoint`     | vertex                | border-classified vertex   |
|  [05]   | `WoundVoronoiEdge` | wound edge            | clockwise cell traversal   |

[VoronoiPlane]:

- State: `MinX`, `MinY`, `MaxX`, `MaxY`, `Sites`, `Edges`, `Points`, and `DuplicateCount`
- Flow: `SetSites` or `GenerateRandomSites` -> `Tessellate` -> `Relax`, `MergeSites`, or `GetNearestSiteTo`

[VoronoiSite]:

- Contract: `IEquatable`; constructor `(x, y)` creates an input seed that tessellation expands into a cell
- Geometry: `X`, `Y`, `Edges`, `Points`, `Centroid`, `Contains(x,y)`, and `Closed`
- Winding: `ClockwiseEdges`, `ClockwiseEdgesWound`, and `ClockwisePoints`
- Topology: `Neighbours`, `LiesOnEdge`, and `LiesOnCorner`

[VoronoiEdge]:

- Contract: `IEquatable`
- Geometry: `Start`, `End`, `Mid`, and `Length`
- Topology: `Left`, `Right`, `Neighbours`, and `CommonPointWith(other)`; a border edge has a null outer site

[VoronoiPoint]:

- Contract: `IEquatable`
- Geometry: `X`, `Y`, `BorderLocation`, and `Sites`; `NotOnBorder` identifies an interior vertex

[WoundVoronoiEdge]:

- Shape: `readonly struct` carrying `Edge` and `Flipped`
- Winding: `Start` and `End` orient the edge clockwise for `VoronoiSite.ClockwiseEdgesWound`

[PUBLIC_TYPE_SCOPE]: tessellation / generation / lookup vocabulary (enums)
- rail: fabrication

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]        |
| :-----: | :------------------------- | :-------------- | :------------------ |
|  [01]   | `BorderEdgeGeneration`     | tessellate enum | border construction |
|  [02]   | `PointGenerationMethod`    | random enum     | site distribution   |
|  [03]   | `NearestSiteLookupMethod`  | lookup enum     | lookup strategy     |
|  [04]   | `PointBorderLocation`      | border enum     | border position     |
|  [05]   | `VoronoiSiteMergeDecision` | merge enum      | merge direction     |

[BorderEdgeGeneration]:

- Values: `DoNotMakeBorderEdges` and `MakeBorderEdges`
- Effect: `MakeBorderEdges` clips cells and synthesizes the four border edges plus corner points; `DoNotMakeBorderEdges` leaves border cells open

[PointGenerationMethod]:

- Values: `Uniform` and `Gaussian`
- Consumer: `GenerateRandomSites`

[NearestSiteLookupMethod]:

- Values: `BruteForce` and `KDTree`
- Cost: `BruteForce` scans in O(n); the default `KDTree` uses bundled `Supercluster.KDTree` in O(log n)

[PointBorderLocation]:

- Values: `NotOnBorder` (-1), `BottomLeft`, `Left`, `TopLeft`, `Top`, `TopRight`, `Right`, `BottomRight`, and `Bottom`
- Consumer: `VoronoiPoint.BorderLocation` classifies positions clockwise

[VoronoiSiteMergeDecision]:

- Values: `DontMerge`, `MergeIntoSite1`, and `MergeIntoSite2`
- Consumer: `VoronoiSiteMergeQuery`

[PUBLIC_TYPE_SCOPE]: pluggable strategies — generation, RNG, merging, nearest-site
- rail: fabrication
- note: the strategy interfaces let a consumer inject custom site generation, determinism (seeded RNG for reproducible toolpaths), or cell-merge logic. The merge-algorithm interface itself (`ISiteMergingAlgorithm`) is INTERNAL — only the concrete `GenericSiteMergingAlgorithm` is public; merging is normally driven through `VoronoiPlane.MergeSites(VoronoiSiteMergeQuery)`.

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

[STRATEGY_DETAIL]:
- `IPointGenerationAlgorithm`: `GenerateRandomSites(amount, algorithm, random)` accepts the custom site-cloud generator
- `IRandomNumberGenerator`: `double NextDouble()` owns the deterministic draw seam
- `SeededRandomNumberGenerator`: `(int seed)` produces reproducible stipple and engrave site clouds
- `ProvidedRandomNumberGenerator`: `(Random random)` wraps a supplied `System.Random`
- `VoronoiSiteMergeQuery`: `VoronoiSiteMergeDecision VoronoiSiteMergeQuery(VoronoiSite site1, VoronoiSite site2)` decides each adjacent pair
- `GenericSiteMergingAlgorithm`: `MergeSites(List<VoronoiSite> sites, List<VoronoiEdge> edges, VoronoiSiteMergeQuery mergeQuery)` runs standalone merging; `VoronoiPlane.MergeSites` wraps it
- `INearestSiteLookup`, `BruteForceNearestSiteLookup`, and `KDTreeNearestSiteLookup`: provide the strategy behind `GetNearestSiteTo`
- `VoronoiEdgeComparer`, `VoronoiPointComparer`, and `VoronoiSiteComparer`: expose `IEqualityComparer` singletons through `.Instance`
- `VoronoiLibValues`: `const double epsilon = 1E-12` defines coordinate-comparison tolerance

[PUBLIC_TYPE_SCOPE]: typed faults — `SharpVoronoiLib.Exceptions`
- rail: fabrication

| [INDEX] | [SYMBOL]                                                     | [FAULT]                     |
| :-----: | :----------------------------------------------------------- | :-------------------------- |
|  [01]   | `VoronoiNotTessellatedException`                             | pre-tessellation navigation |
|  [02]   | `VoronoiDoesntHaveSitesException`                            | empty tessellation input    |
|  [03]   | `VoronoiSiteNotClosedException`                              | open-cell operation         |
|  [04]   | `VoronoiSiteSkippedAsDuplicateException`                     | coincident-site skip        |
|  [05]   | `VoronoiRandomPointGenerationEncounteredTooManyInvalidSites` | generation exhaustion       |
|  [06]   | `VoronoiSiteGenerationProducedNull`                          | null generator output       |
|  [07]   | `VoronoiSiteGenerationProducedWrongCount`                    | generator count mismatch    |

`VoronoiPlane.DuplicateCount` also records coincident-site skips.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the stateful pipeline — `VoronoiPlane`
- rail: fabrication
- note: the canonical flow is `new VoronoiPlane(minX, minY, maxX, maxY)` -> `SetSites(sites)` or `GenerateRandomSites(...)` -> `Tessellate(BorderEdgeGeneration)` -> read `Sites`/`Edges`/`Points` and navigate each cell -> optionally `Relax(...)` / `MergeSites(...)`. Navigation properties throw `VoronoiNotTessellatedException` before `Tessellate`.

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [CAPABILITY]            |
| :-----: | :------------------------------- | :------------- | :---------------------- |
|  [01]   | `VoronoiPlane`                   | constructor    | bind border             |
|  [02]   | `SetSites`                       | input          | set explicit sites      |
|  [03]   | `GenerateRandomSites(method)`    | input          | generate site cloud     |
|  [04]   | `GenerateRandomSites(algorithm)` | input          | invoke custom generator |
|  [05]   | `Tessellate`                     | tessellate     | build diagram           |
|  [06]   | `Relax`                          | relaxation     | regularize sites        |
|  [07]   | `MergeSites`                     | merge          | coalesce regions        |
|  [08]   | `GetNearestSiteTo`               | query          | assign nearest site     |
|  [09]   | `TessellateOnce`                 | one-shot       | tessellate site set     |
|  [10]   | `TessellateRandomSitesOnce`      | one-shot       | tessellate random sites |

[VORONOI_PLANE]:
- Signature: `VoronoiPlane(double minX, double minY, double maxX, double maxY)`
- Effect: Binds the clipping and synthesized-border rectangle

[SET_SITES]:
- Signature: `SetSites(List<VoronoiSite> sites)`
- Effect: Sets the explicit point-site seeds

[GENERATE_RANDOM_SITES_METHOD]:
- Signature: `GenerateRandomSites(int amount, PointGenerationMethod method = Uniform, IRandomNumberGenerator? random = null)`
- Effect: Generates a uniform or Gaussian site cloud; `SeededRandomNumberGenerator` makes the result reproducible

[GENERATE_RANDOM_SITES_ALGORITHM]:
- Signature: `GenerateRandomSites(int amount, IPointGenerationAlgorithm algorithm, IRandomNumberGenerator? random = null)`
- Effect: Delegates site creation to a custom generation strategy

[TESSELLATE]:
- Signature: `Tessellate(BorderEdgeGeneration borderGeneration = MakeBorderEdges)`
- Effect: Runs the Fortune sweep, clips to the border, synthesizes border edges and corners, returns `List<VoronoiEdge>`, and populates `Sites` and `Points`

[RELAX]:
- Signature: `Relax(int iterations = 1, float strength = 1f, bool reTessellate = true)`
- Effect: Moves each site toward `Centroid` by `strength` and optionally re-tessellates each iteration

[MERGE_SITES]:
- Signature: `MergeSites(VoronoiSiteMergeQuery mergeQuery)`
- Effect: Folds the predicate over adjacent cells according to `VoronoiSiteMergeDecision`

[GET_NEAREST_SITE_TO]:
- Signature: `GetNearestSiteTo(double x, double y, NearestSiteLookupMethod lookupMethod = KDTree)`
- Effect: Assigns a point through the bundled kd-tree or a brute-force scan

[TESSELLATE_ONCE]:
- Signature: `static TessellateOnce(List<VoronoiSite> sites, minX, minY, maxX, maxY, BorderEdgeGeneration = MakeBorderEdges)`
- Effect: Tessellates an explicit site set without retaining an orchestrator

[TESSELLATE_RANDOM_SITES_ONCE]:
- Signature: `static TessellateRandomSitesOnce(int numberOfSites, minX, minY, maxX, maxY, BorderEdgeGeneration = MakeBorderEdges)`
- Effect: Tessellates a generated site set without retaining an orchestrator

[ENTRYPOINT_SCOPE]: cell navigation — `VoronoiSite` / `VoronoiEdge`
- rail: fabrication
- note: after `Tessellate`, walk each `VoronoiSite` as a closed cell polygon for toolpath/region generation.

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [CAPABILITY]          |
| :-----: | :-------------------------------- | :------------- | :-------------------- |
|  [01]   | `VoronoiSite.ClockwisePoints`     | cell polygon   | clockwise vertex ring |
|  [02]   | `VoronoiSite.ClockwiseEdgesWound` | cell edges     | clockwise edge cycle  |
|  [03]   | `VoronoiSite.Centroid`            | cell geometry  | Lloyd target          |
|  [04]   | `VoronoiSite.Contains(x,y)`       | cell geometry  | point-in-cell test    |
|  [05]   | `VoronoiSite.Neighbours`          | adjacency      | region graph          |
|  [06]   | `VoronoiSite.Closed`              | cell status    | boundary closure      |
|  [07]   | `VoronoiEdge.Start`               | edge geometry  | first endpoint        |
|  [08]   | `VoronoiEdge.End`                 | edge geometry  | second endpoint       |
|  [09]   | `VoronoiEdge.Mid`                 | edge geometry  | midpoint              |
|  [10]   | `VoronoiEdge.Length`              | edge geometry  | segment length        |
|  [11]   | `VoronoiEdge.Left`                | edge topology  | left site             |
|  [12]   | `VoronoiEdge.Right`               | edge topology  | right site            |
|  [13]   | `VoronoiEdge.CommonPointWith`     | edge topology  | shared vertex         |

`ClockwisePoints` yields the pocketing contour, and `ClockwiseEdgesWound` avoids re-deriving winding. `Centroid` seeds Lloyd relaxation and spiral pockets. `Neighbours` drives traversal order. `Closed` is false for an open border cell under `DoNotMakeBorderEdges`. Edge geometry supplies medial segments for even-spacing decomposition, and `Left` plus `Right` identify the separated sites.

## [04]-[IMPLEMENTATION_LAW]

[DIAGRAM_TOPOLOGY]:
- Coordinates are `double`; the engine is a FLOATING-POINT Fortune sweep with `epsilon = 1E-12` coincidence tolerance. Coincident sites are SKIPPED (counted in `VoronoiPlane.DuplicateCount`, optionally surfacing `VoronoiSiteSkippedAsDuplicateException`), so the consumer pre-dedups or reads `DuplicateCount` to reconcile the input/output site counts.
- `BorderEdgeGeneration.MakeBorderEdges` is the DEFAULT and the one the CAM rail wants: it clips every cell to the `[MinX,MinY]..[MaxX,MaxY]` rectangle and synthesizes the four border edges + corner `VoronoiPoint`s so EVERY cell is a closed polygon (`Closed == true`) directly machinable. `DoNotMakeBorderEdges` leaves boundary cells open (infinite-ray cells truncated, no synthesized border) — use it only when the border is handled downstream.
- `VoronoiPoint.BorderLocation` classifies each vertex's border position (`NotOnBorder` for interior vertices, else the clockwise `BottomLeft..Bottom` slot); a cell's `ClockwisePoints` interleaves interior Voronoi vertices and border/corner vertices into the closed ring.
- `Relax` mutates the plane IN PLACE (re-sites toward centroids and, by default, re-tessellates each iteration); capture the returned edge list per iteration if intermediate states matter. `strength` in [0,1] damps the move (1 = full centroid snap).

[STACKING_LAW]:
- A tessellated, relaxed, or region-merged cell set is the Fabrication CAM partition.
- Each `ClockwisePoints` ring becomes a `Clipper2` `Path64` or `PathD`, scaled to int64 at `Precision.Digits`, or a `CavalierContours` polyline for arc-native offsetting.
- `SharpVoronoiLib` produces regions, and the offset and Boolean owners machine them.
- Spiral-pocket toolpaths seed at `VoronoiSite.Centroid`, and even-spacing engrave decomposes along `VoronoiEdge` medial segments.
- Lloyd-relaxed sites provide regular stipple, engrave, and pen-plot fields.
- POINT-SITE BOUNDARY: `SharpVoronoiLib` does NOT compute the polygon medial axis / segment Voronoi — given a polygon, it Voronois the polygon's VERTICES, not its EDGES. The straight-skeleton / segment-Voronoi concern stays in the in-folder `Toolpath/skeleton` author-kernel; never approximate the medial axis by sampling polygon edges into point sites here.
- `MIConvexHull.CreateVoronoi` owns the N-D Voronoi-as-Delaunay dual without border clipping or Lloyd relaxation.
- `Triangle.StandardVoronoi` and `Triangle.BoundedVoronoi` own the half-edge Voronoi dual of a constrained Delaunay mesh.
- `SharpVoronoiLib` owns the border-clipped, relaxable, mergeable 2D point-site diagram with native nearest-site lookup.
- A region partition requiring border closure and relaxation uses `SharpVoronoiLib`; an existing mesh requiring its Voronoi dual uses `Triangle`.

[RAIL_LAW]:
- Package: `SharpVoronoiLib` (assembly `SharpVoronoiLib`, with a vendored `Supercluster.KDTree`)
- Owns: the 2D Fortune's-sweepline point-site Voronoi diagram with border clipping + synthesized border edges (`Tessellate`/`BorderEdgeGeneration`), Lloyd's relaxation (`Relax`), cell merging under a caller predicate (`MergeSites`/`VoronoiSiteMergeQuery`), random uniform/Gaussian site generation with a seeded-RNG determinism seam, kd-tree nearest-site query (`GetNearestSiteTo`), and the fully-navigable cell/edge/vertex graph — all in the `double` domain
- Accept: the CAM tessellation concern — toolpath region partitioning, spiral-pocket seed centroids, even-spacing region decomposition, and Lloyd-relaxed stipple/engrave/pen-plot site fields, with cells handed to `Clipper2`/`CavalierContours` for machining
- Reject: any polygon medial-axis / segment-Voronoi / straight-skeleton use (point-site only — the medial concern is the KERNEL's `Meshing/offset`+`Meshing/skeleton` owner, K1/K2); reaching for the stale `VoronoiLib 0.1.0` predecessor (superseded); duplicating the N-D Voronoi-dual (`MIConvexHull` owns it) or the constrained-mesh Voronoi dual (`Triangle`'s `StandardVoronoi`/`BoundedVoronoi` owns it); reading a `VoronoiSite`/`VoronoiEdge`/`VoronoiPoint` navigation property before `Tessellate` (throws `VoronoiNotTessellatedException`); and citing the license as ISC (it is MIT)
