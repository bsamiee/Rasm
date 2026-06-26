# [RASM_API_SHARPVORONOILIB]

`SharpVoronoiLib` computes a 2D point-site Voronoi diagram by Fortune's sweep-line algorithm
inside an axis-aligned bounding box, with edge clipping to the box, border-edge closure (so
every cell is a closed polygon), the Delaunay dual read off each edge's `Left`/`Right` site
pair, and Lloyd's relaxation. The whole pipeline is driven by ONE orchestrator class —
`VoronoiPlane` — whose enum knobs (`BorderEdgeGeneration`, `NearestSiteLookupMethod`,
`PointGenerationMethod`) select the strategy; the per-stage `ITessellationAlgorithm`/
`IBorderClippingAlgorithm`/`IBorderClosingAlgorithm`/`IRelaxationAlgorithm`/`ISiteMergingAlgorithm`/
`INearestSiteLookup` interfaces are `internal`, so a consumer composes by enum/delegate, NOT by
re-implementing the sweep. It is the CAM tessellation primitive for the Geometry `Drawing`/
`Meshing` Voronoi leg: a Fortune diagram on a clean point set, its cells walked clockwise as
closed polygons (`VoronoiSite.ClockwiseCell`), its dual edges feeding a Delaunay graph, and its
centroids feeding Lloyd-relaxed point distribution. It is POINT-SITE ONLY — there is no
segment-Voronoi / medial-axis here (that is the in-house `OffsetOp` over the `Tessellation`
dual); it is `double`-domain (NOT predicate-exact); and the `1.2.0` assembly BUNDLES public
copies of a `Supercluster.KDTree.KDTree<TNode>` / `BoundedPriorityList<TElement>` kd-tree under
the `Supercluster.KDTree` namespace — DISTINCT in both namespace and generic arity from the
standalone `SuperClusterKDTree.KDTree<…>` that `api-kdtree` admits, so the two COEXIST with no
collision (a namespace-discipline seam, below).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpVoronoiLib`
- package: `SharpVoronoiLib`
- version: `1.2.0`
- license: public-domain lineage (RudyTheDev; the "VoronoiLib" successor) — the canonical terms are the repo `License.md` (`github.com/RudyTheDev/SharpVoronoiLib/blob/main/License.md`); the nuspec carries a `licenseUrl`, not an SPDX `license` expression
- assembly: `SharpVoronoiLib.dll`
- namespace: `SharpVoronoiLib` (the `VoronoiPlane` orchestrator + `VoronoiSite`/`VoronoiEdge`/`VoronoiPoint`/`WoundVoronoiEdge` model + the enums and the two public strategy interfaces); `SharpVoronoiLib.Exceptions` (the typed faults); `Supercluster.KDTree` + `Supercluster.KDTree.Utilities` (the BUNDLED kd-tree under the ORIGINAL `Supercluster.KDTree` namespace — NOT the standalone fork's `SuperClusterKDTree`, see ABI)
- target: multi-target (`lib/net10.0`, `lib/net9.0`, `lib/netstandard2.1`, `lib/netstandard2.0`); the `net10.0` consumer binds `lib/net10.0` EXACTLY — the chosen asset differs from `netstandard2.0` ONLY by nullable reference-type annotations (`IRandomNumberGenerator?`, `VoronoiSite?`, `VoronoiEdge?`), so the net10.0 bind is what gives the consumer the nullable contract on `Left`/`Right`/`LiesOnEdge`
- asset: pure-managed runtime library, AnyCPU, no native runtime and ZERO external package dependencies; `osx-arm64`-safe
- abi: plain reference types — `VoronoiPlane` is a `class` with `List<T>` collections; the model types are `class : IEquatable<T>` with `==`/`!=`; NO `System.Numerics` / generic-math contract. The BUNDLED `Supercluster.KDTree.KDTree<TNode>` (arity-1) and `Supercluster.KDTree.BoundedPriorityList<TElement>` are PUBLIC in this assembly under the `Supercluster.KDTree` namespace; the standalone `Supercluster.KDTree.Net` (`api-kdtree`) renamed ITS type to `SuperClusterKDTree.KDTree<TDimension,TPriority,TNode>` (arity-3, namespace `SuperClusterKDTree`), so the two are DIFFERENT fully-qualified types and COEXIST with NO same-FQN collision and NO `extern alias` — the only discipline is to not `using Supercluster.KDTree;` reach the bundled copy when the standalone's `SuperClusterKDTree` is wanted (see STACKING_LAW)
- rail: 2D point-site Voronoi tessellation (`double`-domain CAM primitive)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the orchestrator + the diagram model (namespace `SharpVoronoiLib`)
- rail: 2D point-site Voronoi tessellation

`VoronoiPlane` owns the diagram lifecycle (set sites -> tessellate -> relax/merge/query) and
holds the resulting `Sites`/`Edges`/`Points`. The model types form the cell complex:
`VoronoiSite` is an input point that grows a cell, `VoronoiEdge` a cell boundary segment
carrying its two adjacent sites (the Delaunay dual), `VoronoiPoint` a cell-boundary vertex.
The strategy interfaces below are `internal` EXCEPT `IPointGenerationAlgorithm` and
`IRandomNumberGenerator`, the only two pluggable extension points.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]                  | [CAPABILITY]                                                                                          |
| :-----: | :------------------------ | :------------------------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `VoronoiPlane`            | the diagram orchestrator        | `class`; bounded by `(minX,minY,maxX,maxY)`; `SetSites`/`Tessellate`/`Relax`/`MergeSites`/`GetNearestSiteTo` + the `Sites`/`Edges`/`Points` results — the single entry point |
|  [02]   | `VoronoiSite`             | an input point + its cell       | `class : IEquatable<…>`; `X`/`Y`; `Cell`/`ClockwiseCell`/`ClockwiseEdgesWound` (closed cell polygon), `Neighbours` (adjacent sites), `Points`/`ClockwisePoints`, `Centroid` (Lloyd target), `Closed` |
|  [03]   | `VoronoiEdge`             | a cell-boundary segment + dual  | `class : IEquatable<…>`; `Start`/`End`/`Mid` (`VoronoiPoint`), `Left`/`Right` (`VoronoiSite?` — the DELAUNAY DUAL: the two sites this edge separates), `Length`, `Neighbours`, `CommonPointWith(edge)` |
|  [04]   | `VoronoiPoint`            | a cell-boundary vertex          | `class : IEquatable<…>`; `X`/`Y`; `BorderLocation` (`PointBorderLocation`), `Edges`/`Sites` incident lists |
|  [05]   | `WoundVoronoiEdge`        | an oriented cell edge           | the clockwise-wound edge variant returned by `VoronoiSite.ClockwiseEdgesWound` (edge + traversal direction for a consistent CCW/CW polygon walk) |
|  [06]   | `BorderEdgeGeneration`    | border-closure knob             | `enum`; `MakeBorderEdges` (close every cell against the box) / `DoNotMakeBorderEdges` (open boundary cells) |
|  [07]   | `NearestSiteLookupMethod` | point-location strategy knob    | `enum`; `KDTree` (default, O(log n) via the bundled kd-tree) / `BruteForce` (O(n)) |
|  [08]   | `PointGenerationMethod`   | random-site distribution knob   | `enum`; `Uniform` / `Gaussian` — the built-in `GenerateRandomSites` distributions |
|  [09]   | `PointBorderLocation`     | vertex border-edge classifier   | `enum`; `NotOnBorder`/`BottomLeft`/`Left`/`TopLeft`/`Top`/`TopRight`/`Right`/`BottomRight`/`Bottom` — where a `VoronoiPoint` sits on the box |
|  [10]   | `VoronoiSiteMergeQuery`   | site-merge decision delegate    | `delegate VoronoiSiteMergeDecision(VoronoiSite site1, VoronoiSite site2)`; returns `DontMerge`/`MergeIntoSite1`/`MergeIntoSite2` — the predicate `MergeSites` applies pairwise |
|  [11]   | `IPointGenerationAlgorithm` / `IRandomNumberGenerator` | PUBLIC extension points | the only consumer-implementable strategies — a custom point distribution and a custom (e.g. seeded/deterministic) RNG for `GenerateRandomSites` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `VoronoiPlane` — diagram construction, relaxation, query
- rail: 2D point-site Voronoi tessellation

The lifecycle is: construct over a box, `SetSites` (or `GenerateRandomSites`), `Tessellate`
(producing the clipped + closed edge list), then optionally `Relax` (Lloyd), `MergeSites`, or
`GetNearestSiteTo`. After tessellation the cell complex is walked through the model types. The
`static` `TessellateOnce`/`TessellateRandomSitesOnce` are the one-shot convenience forms that
skip the stateful instance.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new VoronoiPlane(double minX, double minY, double maxX, double maxY)`      | constructor    | the bounded plane — every cell is clipped to this box |
|  [02]   | `VoronoiPlane.SetSites(List<VoronoiSite>)`                                  | instance       | install the input point set (the cell-growing seeds) |
|  [03]   | `VoronoiPlane.GenerateRandomSites(int amount, PointGenerationMethod = Uniform, IRandomNumberGenerator? = null)` / `(int, IPointGenerationAlgorithm, IRandomNumberGenerator?)` | instance | generate + install N random sites (`Uniform`/`Gaussian`, or a custom distribution + RNG) |
|  [04]   | `VoronoiPlane.Tessellate(BorderEdgeGeneration = MakeBorderEdges)`           | instance       | run Fortune's sweep + clip + border-close; returns `List<VoronoiEdge>` and populates `Sites`/`Edges`/`Points` |
|  [05]   | `VoronoiPlane.Relax(int iterations = 1, float strength = 1f, bool reTessellate = true)` | instance | Lloyd's relaxation — move each site toward its cell `Centroid` and (by default) re-tessellate; `strength` interpolates the move |
|  [06]   | `VoronoiPlane.MergeSites(VoronoiSiteMergeQuery)`                            | instance       | pairwise-merge sites under the decision delegate (collapse near-duplicate or co-cell seeds) |
|  [07]   | `VoronoiPlane.GetNearestSiteTo(double x, double y, NearestSiteLookupMethod = KDTree)` | instance | nearest-site point location (kd-tree default, brute-force fallback) — the cell a query point falls in |
|  [08]   | `VoronoiPlane.Sites` / `Edges` / `Points`                                  | instance prop  | the `List<VoronoiSite>` / `List<VoronoiEdge>` / `List<VoronoiPoint>` result graph after `Tessellate` |
|  [09]   | `VoronoiPlane.MinX` / `MinY` / `MaxX` / `MaxY` / `DuplicateCount` / `Tesselated` | instance prop | the box bounds, the count of sites dropped as duplicates, and the tessellated-yet flag |
|  [10]   | `VoronoiPlane.TessellateOnce(List<VoronoiSite>, double minX, minY, maxX, maxY, BorderEdgeGeneration = MakeBorderEdges)` / `TessellateRandomSitesOnce(int numberOfSites, …)` | static | one-shot stateless tessellation (skip the instance lifecycle) |

[ENTRYPOINT_SCOPE]: `VoronoiSite` — the cell + adjacency surface (the Delaunay dual graph)
- rail: 2D point-site Voronoi tessellation

After tessellation each `VoronoiSite` exposes its cell as ordered edge/point loops and its
adjacent sites. `ClockwiseCell`/`ClockwisePoints`/`ClockwiseEdgesWound` give a consistently-
wound closed polygon (the input to a fill/offset/mesh consumer); `Neighbours` IS the Delaunay
adjacency (two sites are Delaunay-adjacent iff their cells share an edge); `Centroid` is the
Lloyd-relaxation target.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new VoronoiSite(double x, double y)` / `VoronoiSite.X` / `Y`               | constructor / prop | the seed point |
|  [02]   | `VoronoiSite.Cell` (= `Edges`) / `ClockwiseCell` (= `ClockwiseEdges`) / `ClockwiseEdgesWound` | instance prop | the cell boundary as an unordered / clockwise-ordered / clockwise-wound edge loop — the closed cell polygon |
|  [03]   | `VoronoiSite.Points` / `ClockwisePoints`                                    | instance prop  | the cell's corner vertices (unordered / clockwise) — the polygon ring for a fill/mesh consumer |
|  [04]   | `VoronoiSite.Neighbours`                                                    | instance prop  | the adjacent `VoronoiSite`s — the DELAUNAY DUAL adjacency (cells sharing an edge) |
|  [05]   | `VoronoiSite.Centroid`                                                      | instance prop  | the cell centroid (`VoronoiPoint`) — the Lloyd-relaxation move target |
|  [06]   | `VoronoiSite.LiesOnEdge` / `LiesOnCorner` / `Closed` / `Contains(double x, double y)` | instance | degenerate-placement flags (site on a cell edge/corner), closed-cell flag, point-in-cell test |

[ENTRYPOINT_SCOPE]: `VoronoiEdge` / `VoronoiPoint` — the boundary geometry
- rail: 2D point-site Voronoi tessellation

A `VoronoiEdge` carries the two sites it separates in `Left`/`Right` — reading both across the
edge list reconstructs the Delaunay edge set directly. `VoronoiPoint.BorderLocation` flags
which box border a vertex was clipped/closed onto.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `VoronoiEdge.Start` / `End` / `Mid`                                         | instance prop  | the edge endpoints + midpoint (`VoronoiPoint`) |
|  [02]   | `VoronoiEdge.Left` / `Right`                                                | instance prop  | the two `VoronoiSite?` this edge separates — the DELAUNAY DUAL edge (null on a border-only edge) |
|  [03]   | `VoronoiEdge.Length` / `Neighbours` / `CommonPointWith(VoronoiEdge)`        | instance       | edge length, edge-adjacency, shared endpoint between two edges (cell-traversal step) |
|  [04]   | `VoronoiPoint.X` / `Y` / `BorderLocation` / `Edges` / `Sites`              | instance prop  | the vertex coordinate, its box-border classification, and the incident edges/sites |

[ENTRYPOINT_SCOPE]: `SharpVoronoiLib.Exceptions` — the typed fault surface
- rail: 2D point-site Voronoi tessellation

The pipeline raises specific exception types rather than a generic failure — a boundary capsule
catches these and maps them onto the Geometry result rail.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `VoronoiNotTessellatedException` / `VoronoiDoesntHaveSitesException`        | accessing the diagram before `Tessellate` / tessellating with no sites |
|  [02]   | `VoronoiSiteNotClosedException` / `VoronoiSiteSkippedAsDuplicateException`  | a cell could not be closed / a duplicate site was dropped |
|  [03]   | `VoronoiRandomPointGenerationEncounteredTooManyInvalidSites` / `VoronoiSiteGenerationProducedNull` / `VoronoiSiteGenerationProducedWrongCount` | `GenerateRandomSites` / a custom `IPointGenerationAlgorithm` produced an invalid result |

## [04]-[IMPLEMENTATION_LAW]

[TESSELLATION_PROFILE]:
- algorithm: Fortune's sweep-line — O(n log n) for n sites — inside the `(minX,minY,maxX,maxY)` box; the diagram is ALWAYS clipped to the box and (under `MakeBorderEdges`) every cell is closed against it, so there are no infinite/open cells in the result the way a raw Fortune output would have.
- domain: `double` arithmetic with an internal epsilon (`EpsilonUtils`) for the sweep-event comparisons — this is NOT a predicate-exact construction; for a degeneracy-robust diagram on a near-collinear/cocircular point set the in-house `Meshing/delaunay` (predicate floor) owns the exact path, and this library is the fast `double`-domain CAM tessellator.
- single entry: `VoronoiPlane` is the only orchestrator; the per-stage `ITessellationAlgorithm`/`IBorderClippingAlgorithm`/`IBorderClosingAlgorithm`/`IRelaxationAlgorithm`/`ISiteMergingAlgorithm`/`INearestSiteLookup` strategies are `internal`, so a consumer NEVER re-implements the sweep/clip/close — it selects behavior through the `BorderEdgeGeneration`/`NearestSiteLookupMethod`/`PointGenerationMethod` enums and the `VoronoiSiteMergeQuery` delegate, and extends only the two PUBLIC `IPointGenerationAlgorithm`/`IRandomNumberGenerator` seams.
- Delaunay dual: the dual is not a separate construction — it is read off the result. Each `VoronoiEdge.Left`/`Right` names the two sites the edge separates (a Delaunay edge), and `VoronoiSite.Neighbours` is the adjacency; the Delaunay triangulation is derived from the diagram, not computed twice.

[LOCAL_ADMISSION]:
- `SharpVoronoiLib` backs the Geometry `Drawing`/`Meshing` Voronoi leg: a Fortune diagram on a clean `double` point set, cells walked as closed polygons via `VoronoiSite.ClockwiseCell`/`ClockwisePoints`, centroids driving Lloyd point relaxation via `Relax`, and the Delaunay dual via edge `Left`/`Right`. It is the CAM tessellation primitive PRIMARILY OWNED by the Rasm geometry kernel (per the README roster), additive beside `MIConvexHull` (which also computes a Voronoi but as a convex-hull lift) and the in-house exact `Meshing/delaunay`.
- it is POINT-SITE ONLY — there is NO segment Voronoi / medial axis here; the medial-axis read-off is the in-house `OffsetOp` over the `Tessellation` dual, and a segment-input diagram is out of scope. Do not reach for this library for a medial-axis or a straight-skeleton.
- `GetNearestSiteTo` + the cell loops give point-location-into-cell directly; a consumer that already holds the diagram does not need a separate spatial index for which-cell queries (the `NearestSiteLookupMethod.KDTree` path uses the bundled kd-tree).

[STACKING_LAW]:
- vs the bundled `Supercluster.KDTree` (`api-kdtree`): `SharpVoronoiLib 1.2.0` ships PUBLIC copies of `Supercluster.KDTree.KDTree<TNode>` (arity-1) and `Supercluster.KDTree.BoundedPriorityList<TElement>` inside `SharpVoronoiLib.dll` under the ORIGINAL `Supercluster.KDTree` namespace (a source-included kd-tree backing `NearestSiteLookupMethod.KDTree` via the internal `KDTreeNearestSiteLookup`). The standalone `Supercluster.KDTree.Net` admitted by `api-kdtree` is the `micampbell` fork that RENAMED its namespace+type to `SuperClusterKDTree.KDTree<TDimension,TPriority,TNode>` (arity-3, assembly `KDTree.dll`) — so the bundled `Supercluster.KDTree.KDTree` and the standalone `SuperClusterKDTree.KDTree` are DIFFERENT fully-qualified types (different namespace AND different generic arity) on the compile graph. There is NO same-FQN collision and NO `extern alias` is required: referencing both assemblies is clean. The only discipline is reach-by-name — the consumer's OWN k-NN (`Processing/fit`, registration) goes through `using SuperClusterKDTree;` on the standalone arity-3 tree, while `SharpVoronoiLib`'s bundled arity-1 copy serves only its own `GetNearestSiteTo` and is never named by the consumer (a `using Supercluster.KDTree;` would resolve the bundled copy, not the standalone). They never meet at a call site.
- vs MIConvexHull (`api-miconvexhull`): `MIConvexHull` builds a Voronoi as the dual of a Delaunay obtained by lifting points to a paraboloid and taking the lower convex hull — N-dimensional, no bounding box, infinite cells. `SharpVoronoiLib` is 2D-only, box-clipped, border-closed, with explicit cell/edge/site adjacency and Lloyd relaxation. The two are complementary: `MIConvexHull` for the N-D hull/Delaunay/Voronoi algebra, `SharpVoronoiLib` for a 2D box-bounded closed-cell CAM diagram with relaxation — never collapse onto one when the design needs both the N-D dual and the 2D closed-cell walk.
- vs Triangle's `StandardVoronoi` (`api-triangle`): `Triangle.NET` ALSO yields a Voronoi — `StandardVoronoi`/`BoundedVoronoi` derive it as a DCEL half-edge dual of its constrained-Delaunay triangulation (topological, keyed off the triangle mesh, a by-product of meshing). `SharpVoronoiLib` is the point-site Fortune diagram with explicit site/edge geometry, box-clipped CLOSED cells, and Lloyd `Relax`. The three Voronoi-producing owners split by what they primarily build: `SharpVoronoiLib` when the closed Voronoi CELLS (clockwise-wound polygons + relaxation) are the goal, `Triangle` when the triangle MESH is the goal and the Voronoi is a free DCEL dual, `MIConvexHull` when the N-D Delaunay-dual algebra is the goal — a Voronoi concern lands on exactly one by which output drives the design.
- vs the in-house exact Delaunay (`Meshing/delaunay`): the in-house Bowyer-Watson runs on the predicate-exact `Orient2D`/`InCircle` floor and is degeneracy-robust; `SharpVoronoiLib` is `double`-domain with an epsilon. For a Voronoi/Delaunay whose COMBINATORIAL structure must survive near-cocircular inputs, the exact path owns it; `SharpVoronoiLib` is the fast tessellation where a `double` epsilon is acceptable. The two are precision tiers of the same diagram, not redundant owners.
- output shape: `VoronoiSite.ClockwisePoints` is a closed CCW/CW polygon ring per cell, directly the input shape a fill (`Drawing/view`), pack (`Drawing/pack`), or mesh consumer expects — no re-winding step needed since the library already provides the wound order.

[RAIL_LAW]:
- Package: `SharpVoronoiLib`
- Owns: 2D point-site Voronoi tessellation — `VoronoiPlane` (Fortune's sweep + box clipping + border closure + Lloyd `Relax` + site `MergeSites` + `GetNearestSiteTo` point location), the `VoronoiSite`/`VoronoiEdge`/`VoronoiPoint` cell complex with clockwise-wound closed cells and the Delaunay dual on edge `Left`/`Right` + site `Neighbours`, the `BorderEdgeGeneration`/`NearestSiteLookupMethod`/`PointGenerationMethod` knobs, and the typed `SharpVoronoiLib.Exceptions` fault surface
- Accept: a `double`-domain 2D Voronoi on a clean box-bounded point set; cell polygons via `ClockwiseCell`/`ClockwisePoints` feeding a fill/pack/mesh consumer; the Delaunay adjacency via `Neighbours`/edge `Left`/`Right`; Lloyd point relaxation via `Relax`; which-cell point location via `GetNearestSiteTo`; a custom distribution/RNG via the PUBLIC `IPointGenerationAlgorithm`/`IRandomNumberGenerator`
- Reject: a segment-Voronoi / medial-axis / straight-skeleton request (point-site only — the in-house `OffsetOp`/`Tessellation` dual owns the medial axis); a degeneracy-EXACT diagram (this is `double`+epsilon — the in-house exact `Meshing/delaunay` owns the robust combinatorial structure); re-implementing the `internal` tessellation/clipping/closing strategies (compose the enums + delegate, extend only the two public seams); `using Supercluster.KDTree;` when the standalone `api-kdtree` arity-3 `SuperClusterKDTree.KDTree<…>` is wanted (that namespace resolves THIS assembly's bundled arity-1 copy — the two are distinct FQNs, no collision and no alias, but reach the right one by name); treating `VoronoiPlane.Sites`/`Edges` as valid before `Tessellate` (raises `VoronoiNotTessellatedException`)
