# [RASM_API_MICONVEXHULL]

`MIConvexHull` is the kernel's TYPED-RESULT computational-geometry primitive for three coupled
constructions over the same point set — the CONVEX HULL (N-dimensional Quickhull + a dedicated
2D fast path), the DELAUNAY triangulation, and the VORONOI diagram (the Delaunay dual). It is
the kernel's `[COMPUTATIONAL_GEOMETRY]` owner per the strata law: a single central pin that
DOUBLES as the Rasm-kernel direct reference AND the downstream transitive floor for Materials'
`InteractionDiagram` (which assembles the N-M-M structural-capacity vertices into a closed onion
hull) — one owner, never two. The surface is generic over a `TVertex : IVertex` (N-dim
`double[] Position`) / `IVertex2D` (`X`/`Y`) contract and a `TFace : ConvexFace`/`TCell :
TriangulationCell` cell type, so the kernel carries its own `Rasm` payload AS the vertex/face
type and recovers connectivity (`Adjacency`/`Vertices`/`Normal`) directly. CRITICALLY it returns
a `ConvexHullCreationResult` carrying a `ConvexHullCreationResultOutcome` discriminant + an
`ErrorMessage` — a TYPED ERROR RAIL for degenerate/insufficient/non-uniform input, NOT an
exception path — so the kernel folds it into a `Fin`/`Validation` at the boundary. Pure-managed
AnyCPU, zero dependencies, MIT, osx-arm64-safe.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MIConvexHull`
- package: `MIConvexHull`
- version: `1.1.19.1019`
- license: MIT (DesignEngrLab; `designengrlab.github.io/MIConvexHull`; nuspec `licenseUrl github.com/DesignEngrLab/MIConvexHull/.../LICENSE.txt`)
- assembly: `MIConvexHull.dll`
- namespace: `MIConvexHull` (all types — `ConvexHull`/`Triangulation`/`VoronoiMesh` static entrypoints, the generic result/face/cell types, the `IVertex`/`IVertex2D` contracts)
- target: MULTI-TARGET `lib/netstandard1.0` AND `lib/netstandard2.0` — the `net10.0` consumer binds the `netstandard2.0` asset (verified surface decompiled from `netstandard2.0`); the two TFMs expose the SAME public surface (the floor difference is BCL, not API), so there is no consumer-TFM signature drift here
- asset: pure-managed runtime library, AnyCPU, NO native runtime and ZERO package dependencies (both TFM groups empty); the algorithm is dimension-agnostic Quickhull with a specialized monotone-chain 2D path
- abi: generic over `TVertex : IVertex` (the N-dim `double[] Position` contract) and `TFace : ConvexFace<TVertex,TFace>, new()` / `TCell : TriangulationCell<…>, new()`; NO `System.Numerics` generic-math (coordinates are `double[]`), NO `Span`-first kernels — `IList<TVertex>`/`double[]` object-model API
- rail: typed-result convex hull / Delaunay / Voronoi

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the three static entrypoints + the vertex/face/result vocabulary
- rail: convex hull / Delaunay / Voronoi

Three static entry classes (`ConvexHull`/`Triangulation`/`VoronoiMesh`) each polymorphic over
the generality of the vertex/cell type; the `IVertex`/`IVertex2D` contracts the consumer's own
type implements (or the `DefaultVertex`/`DefaultVertex2D` for raw `double[]`); the
`ConvexHullCreationResult` typed-result carrier; and the connectivity types `ConvexFace`/
`TriangulationCell`/`VoronoiEdge`. The package exposes its internal `FaceConnector`/
`ConvexFaceInternal`/`ObjectManager`/`IndexBuffer` allocation plumbing as public — none is
consumer surface.

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]                       | [CAPABILITY]                                                                                          |
| :-----: | :---------------------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `ConvexHull` (static)                     | the hull entrypoint                  | `Create<TVertex,TFace>` / `Create<TVertex>` / `Create(IList<double[]>)` (ND Quickhull) + `Create2D<TVertex>` / `Create2D(IList<double[]>)` (2D fast path) |
|  [02]   | `Triangulation` (static)                  | the Delaunay entrypoint              | `CreateDelaunay<TVertex>` / `CreateDelaunay<TVertex,TFace>` / `CreateDelaunay(IList<double[]>)` + `CreateVoronoi<…>` overloads |
|  [03]   | `VoronoiMesh` (static)                    | the Voronoi entrypoint               | `Create<TVertex,TCell,TEdge>` / `Create<TVertex>` / `Create(IList<double[]>)` — the Delaunay dual as cells + edges |
|  [04]   | `IVertex` / `IVertex2D`                   | the point contracts                  | `IVertex` = `double[] Position` (N-dim); `IVertex2D` = `double X` + `double Y` (the 2D fast-path contract) |
|  [05]   | `DefaultVertex` / `DefaultVertex2D`       | the raw-coordinate adapters          | `DefaultVertex` (`double[] Position`); `DefaultVertex2D(double[]/double x,double y)` — when the consumer has no own vertex type |
|  [06]   | `ConvexHullCreationResult<TVertex,TFace>` / `<TVertex>` | the TYPED-RESULT carrier  | `Result` (the `ConvexHull<…>` / `IList<TVertex>`) + `Outcome` (`ConvexHullCreationResultOutcome`) + `ErrorMessage` — the error rail |
|  [07]   | `ConvexHull<TVertex,TFace>` / `<TVertex>` | the hull object                      | ND: `Points` + `Faces` (the hull facets); 2D: `Points` (the ORDERED hull-boundary polygon) |
|  [08]   | `ConvexFace<TVertex,TFace>` (abstract)    | a hull facet / Delaunay cell base    | `Adjacency` (`TFace[]`) + `Vertices` (`TVertex[]`) + `Normal` (`double[]`); base of `DefaultConvexFace`/`TriangulationCell` |
|  [09]   | `TriangulationCell<TVertex,TCell>` (abstract) / `DefaultTriangulationCell` | a Delaunay simplex | `: ConvexFace` — a Delaunay cell (triangle in 2D, tetrahedron in 3D) with its vertex set + adjacency |
|  [10]   | `VoronoiEdge<TVertex,TCell>` / `VoronoiMesh<…>` | the Voronoi dual graph          | `VoronoiEdge` = `Source`/`Target` cell pair; `VoronoiMesh` = `Vertices` (cells) + `Edges` |
|  [11]   | `ConvexHullCreationResultOutcome` (enum)  | the result discriminant              | `Success`/`DimensionSmallerTwo`/`DimensionTwoWrongMethod`/`NotEnoughVerticesForDimension`/`NonUniformDimension`/`DegenerateData`/`UnknownError` |
|  [12]   | `ConvexHullGenerationException`            | the exceptional outcome              | carries `Error` (`…Outcome`) + `ErrorMessage` — thrown only by the legacy throwing path; prefer the `Result.Outcome` fold |

## [03]-[CONVEX_HULL]

[HULL_SCOPE]: N-dimensional Quickhull + the 2D fast path (typed-result)
- rail: convex hull

The ND `Create` returns a hull object with `Faces` (each `ConvexFace` carrying `Vertices`,
`Adjacency`, and the outward `Normal`); the 2D `Create2D` returns the ORDERED hull-boundary
polygon (`Result.Points` is the boundary loop). USE `Create2D` FOR 2D — the ND `Create` rejects
2D input with `DimensionTwoWrongMethod`. `tolerance` (default `1E-10`) is the coplanarity
threshold. EVERY entrypoint returns a `ConvexHullCreationResult`; read `.Outcome` before `.Result`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `ConvexHull.Create<TVertex,TFace>(IList<TVertex> data, double tolerance=1E-10)` (→ `ConvexHullCreationResult<TVertex,TFace>`) | static | ND hull with a CUSTOM face type `TFace : ConvexFace` (carry domain data on the facet) |
|  [02]   | `ConvexHull.Create<TVertex>(IList<TVertex> data, double tolerance=1E-10)` (→ `…Result<TVertex, DefaultConvexFace<TVertex>>`) | static | ND hull with the default facet type — the common case for a custom `IVertex` |
|  [03]   | `ConvexHull.Create(IList<double[]> data, double tolerance=1E-10)` (→ `…Result<DefaultVertex, DefaultConvexFace<DefaultVertex>>`) | static | ND hull straight from raw `double[]` coordinates (no custom vertex type) |
|  [04]   | `ConvexHull.Create2D<TVertex>(IList<TVertex> data, double tolerance=1E-10)` (→ `ConvexHullCreationResult<TVertex>` where `TVertex : IVertex2D, new()`) | static | the 2D monotone-chain fast path — `Result` is the ORDERED hull-boundary polygon |
|  [05]   | `ConvexHull.Create2D(IList<double[]> data, double tolerance=1E-10)` (→ `…Result<DefaultVertex2D>`) | static | 2D hull from raw `double[]` 2-vectors                                |
|  [06]   | `ConvexHull<TVertex,TFace>.Points` / `.Faces` (ND) ; `ConvexHull<TVertex>.Points` (2D) | result accessor | the hull vertices; the hull FACETS (ND, each with `Adjacency`/`Normal`) / the ORDERED boundary loop (2D) |

## [04]-[DELAUNAY_AND_VORONOI]

[DELAUNAY_SCOPE]: `Triangulation.CreateDelaunay` — the N-dim Delaunay complex
- rail: Delaunay triangulation

The Delaunay triangulation as a cell complex — `Cells` are the simplices (triangles in 2D,
tetrahedra in 3D), each a `TriangulationCell : ConvexFace` carrying its `Vertices` + `Adjacency`.
`PlaneDistanceTolerance` (default `1E-10`) is the cospherical threshold. This is a POINT-CLOUD
triangulation (connectivity of scattered points), DISTINCT from a contour fill.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Triangulation.CreateDelaunay<TVertex>(IList<TVertex> data, double PlaneDistanceTolerance=1E-10)` (→ `ITriangulation<TVertex, DefaultTriangulationCell<TVertex>>`) | static | Delaunay complex over a custom `IVertex` with the default cell type   |
|  [02]   | `Triangulation.CreateDelaunay<TVertex,TFace>(IList<TVertex> data, double PlaneDistanceTolerance=1E-10)` | static | Delaunay with a CUSTOM cell type `TFace : TriangulationCell` (carry data on the simplex) |
|  [03]   | `Triangulation.CreateDelaunay(IList<double[]> data, double PlaneDistanceTolerance=1E-10)` | static    | Delaunay straight from raw `double[]` coordinates                    |
|  [04]   | `ITriangulation<TVertex,TCell>.Cells` (`IEnumerable<TCell>`)               | result accessor | the simplices; each `TCell.Vertices` + `TCell.Adjacency` give the mesh connectivity |

[VORONOI_SCOPE]: `VoronoiMesh.Create` / `Triangulation.CreateVoronoi` — the Delaunay dual
- rail: Voronoi diagram

The Voronoi diagram as the dual graph of the Delaunay — `Vertices` are the Delaunay cells (whose
circumcenters are the Voronoi vertices), `Edges` are the `VoronoiEdge` `Source`→`Target` cell
adjacencies. Reachable via either `VoronoiMesh.Create` or `Triangulation.CreateVoronoi`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `VoronoiMesh.Create<TVertex>(IList<TVertex> data, double PlaneDistanceTolerance=1E-10)` (→ `VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<…>>`) | static | the Voronoi diagram over a custom `IVertex` with default cell/edge types |
|  [02]   | `VoronoiMesh.Create<TVertex,TCell,TEdge>(…)` / `Create<TVertex,TCell>(…)` / `Create(IList<double[]>, …)` | static | the custom-cell/edge and raw-`double[]` overloads |
|  [03]   | `Triangulation.CreateVoronoi<…>(…)` (the same overload family on the `Triangulation` class) | static     | the alternate entrypoint — identical result to `VoronoiMesh.Create`  |
|  [04]   | `VoronoiMesh<…>.Vertices` (`IEnumerable<TCell>`) / `.Edges` (`IEnumerable<TEdge>`) ; `VoronoiEdge.Source` / `.Target` | result accessor | the Voronoi cells + the adjacency edges (each a Delaunay-cell pair) |

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: every entrypoint is GENERIC over the vertex contract — `IVertex` (N-dim `double[] Position`) or `IVertex2D` (`double X`/`Y` for the 2D fast path) — and over a face/cell type (`ConvexFace<TVertex,TFace>` with `Adjacency`/`Vertices`/`Normal`, or `TriangulationCell : ConvexFace`). The consumer's OWN type implements `IVertex` and IS the hull/cell vertex, so domain payload rides through and connectivity is recovered from `Faces`/`Cells`/`Edges`.
- typed-result rail: `ConvexHull.Create*` returns a `ConvexHullCreationResult` carrying `Outcome` (`ConvexHullCreationResultOutcome` = `Success`/`DimensionSmallerTwo`/`DimensionTwoWrongMethod`/`NotEnoughVerticesForDimension`/`NonUniformDimension`/`DegenerateData`/`UnknownError`) + `ErrorMessage` — a TYPED ERROR RAIL, NOT an exception. The kernel reads `.Outcome` and folds to `Fin`/`Validation` at the boundary; `ConvexHullGenerationException` is the legacy throwing path, avoided in favour of the result fold.
- dimensionality discipline: `Create` is N-dimensional but REJECTS 2D with `DimensionTwoWrongMethod` — 2D MUST use `Create2D` (the monotone-chain fast path), whose `Result.Points` is the ORDERED hull-boundary polygon (vs ND `Faces`). `CreateDelaunay`/`CreateVoronoi` are dimension-agnostic.
- numeric profile: coordinates are `double[]` (NO `System.Numerics` generic-math); `tolerance`/`PlaneDistanceTolerance` (default `1E-10`) is the coplanarity/cospherical threshold. The algorithm is `double`-precision Quickhull — degenerate/near-cospherical input surfaces as `DegenerateData`, not a robust exact result.

[LOCAL_ADMISSION]:
- MIConvexHull is the kernel's `[COMPUTATIONAL_GEOMETRY]` owner for convex-hull / Delaunay / Voronoi (Rasm.csproj Computational Geometry group, DIRECT `PackageReference`). It is PRIMARILY owned by the kernel per the strata law; Materials' `InteractionDiagram` consumes the SAME single central pin downstream (assembling the N-M-M structural-capacity vertices into a closed onion hull), so the central pin doubles as the kernel reference AND the Materials transitive floor — never a second owner.
- the kernel implements `IVertex` on (or adapts `Rasm.Vectors` →) its point type, carries the `Rasm` index/payload on the vertex or a custom `TFace : ConvexFace`, and recovers connectivity from `Faces`/`Cells`/`Edges` — it never round-trips through `double[]` when a typed vertex is available.
- ALWAYS fold `ConvexHullCreationResult.Outcome` into the kernel's `Fin`/`Validation` rail at the boundary; do NOT call the throwing `ConvexHullGenerationException` path or ignore the outcome and read a possibly-null `Result`.
- USE `Create2D` for 2D hulls (the ND `Create` errors with `DimensionTwoWrongMethod`); this is the planar-section / interaction-curve hull case.

[STACKING_LAW]:
- vs GShark (`api-gshark`): GShark ships its OWN `GShark.Geometry.ConvexHull.GenerateHull` (3D incremental, mutable `ref` out-params, `Point3`-valued) — the kernel does NOT use it; feeding it would force a GShark value-vocabulary leak. MIConvexHull's typed-result `Create` (consumer `IVertex` + `Outcome` rail) is the kernel hull owner; GShark's hull stays internal to GShark.
- vs the kernel `Meshing/delaunay`: the kernel-authored `Tessellation` owns the CONSTRAINED Delaunay of a planar straight-line graph (boundary + hole segment recovery over the exact-predicate floor; Ruppert-style quality refinement is its growth axis); MIConvexHull owns the UNCONSTRAINED point-cloud Delaunay (no boundary constraint, no quality refinement, but N-dimensional). Constrained-2D → `Meshing/delaunay`; unconstrained point-cloud / ND → MIConvexHull. They are complementary computational-geometry owners, not duplicates.
- vs Supercluster.KDTree (`api-kdtree`): the kd-tree answers nearest-neighbour QUERIES over a fixed cloud (no connectivity); MIConvexHull's `CreateDelaunay` produces the connectivity (the cell complex). "k nearest of a cloud" → kd-tree; "triangulate the cloud" → MIConvexHull — different questions over the same points.
- vs the kernel fill owners (`Meshing/delaunay` + `Meshing/arrangement`): polygon-AREA fill (bounded region, winding-classified) is the kernel's exact `Tessellation`/`PlanarOverlay` lane; MIConvexHull's `CreateDelaunay` is the point-cloud Delaunay (the connectivity of scattered points, no boundary). "Triangulate the area inside this polygon" → the kernel fill lane; "connect these scattered points into a Delaunay complex" → MIConvexHull. Both say "Delaunay" but over different inputs (bounded polygon area vs unbounded point set) — never interchangeable.
- vs the kernel Voronoi owners (`Meshing/delaunay` dual + `Meshing/mesh` restricted power diagram): the constrained Voronoi dual and the restricted Laguerre diagram are kernel-authored exact lanes; MIConvexHull's `VoronoiMesh` is the N-dimensional Delaunay-dual Voronoi as an abstract cell/edge graph, reached only where the ND dual is the question.

[RAIL_LAW]:
- Package: `MIConvexHull`
- Owns: the typed-result computational-geometry triad — `ConvexHull.Create<TVertex,TFace>`/`Create<TVertex>`/`Create(IList<double[]>)` (ND Quickhull) and `Create2D<TVertex>`/`Create2D(…)` (2D monotone-chain fast path), `Triangulation.CreateDelaunay<…>` (N-dim Delaunay cell complex), `VoronoiMesh.Create<…>` / `Triangulation.CreateVoronoi<…>` (the Delaunay-dual Voronoi), the `IVertex`/`IVertex2D` point contracts + `DefaultVertex`/`DefaultVertex2D` adapters, the `ConvexFace`/`TriangulationCell`/`VoronoiEdge` connectivity types (`Adjacency`/`Vertices`/`Normal`), and the `ConvexHullCreationResult` + `ConvexHullCreationResultOutcome` typed error rail — the kernel's `[COMPUTATIONAL_GEOMETRY]` owner and the Materials `InteractionDiagram` transitive floor under ONE central pin.
- Accept: N-dim or 2D convex hull, N-dim Delaunay triangulation, and N-dim Voronoi over a point set whose vertex type implements `IVertex`/`IVertex2D` (carrying the `Rasm` payload); reading connectivity from `Faces`/`Cells`/`Edges`; folding `ConvexHullCreationResult.Outcome` into the kernel `Fin`/`Validation`; the closed-onion-hull assembly of structural-capacity vertices (Materials downstream of the same pin).
- Reject: calling the ND `Create` on 2D data (it errors `DimensionTwoWrongMethod` — use `Create2D`); ignoring `Result.Outcome` and reading a possibly-null `Result`, or taking the throwing `ConvexHullGenerationException` path instead of the outcome fold; consuming GShark's internal `ConvexHull.GenerateHull` as the kernel hull (use this typed-result `Create`); using MIConvexHull for a CONSTRAINED 2D mesh (that is the kernel `Meshing/delaunay`) or a 2D edge-clipped Voronoi (that is `api-sharpvoronoilib`); touching the package's public-but-internal `FaceConnector`/`ObjectManager`/`IndexBuffer`/`ConvexFaceInternal` plumbing; expecting robust/exact arithmetic (it is `double`-precision Quickhull — near-degenerate input is `DegenerateData`, escalate to the kernel's exact-predicate ladder where robustness is required).
