# [RASM_API_MICONVEXHULL]

`MIConvexHull` owns the kernel's typed-result computational-geometry primitive for the convex hull, Delaunay triangulation, and Voronoi diagram over one point set. The N-dimensional Quickhull and dedicated 2D path share generic `TVertex: IVertex` or `IVertex2D` vertex contracts and `TFace: ConvexFace` or `TCell: TriangulationCell` connectivity contracts, so kernel payloads remain on their native vertex and face types. `ConvexHullCreationResult` carries the `ConvexHullCreationResultOutcome` discriminant and `ErrorMessage`, which the boundary folds into `Fin` or `Validation`. The central pin is both the direct kernel reference and the downstream transitive floor for Materials' `InteractionDiagram`, which assembles N-M-M structural-capacity vertices into a closed onion hull.

## [01]-[PACKAGE_SURFACE]

- Package: `MIConvexHull`
- License: MIT; DesignEngrLab; `designengrlab.github.io/MIConvexHull`; nuspec `licenseUrl github.com/DesignEngrLab/MIConvexHull/.../LICENSE.txt`
- Assembly: `MIConvexHull.dll`
- Namespace: `MIConvexHull`
- Target: `lib/netstandard1.0` and `lib/netstandard2.0`; the `net10.0` consumer binds the `netstandard2.0` asset, and both target frameworks expose the same public surface
- Asset: pure-managed AnyCPU runtime library with no native runtime or package dependencies; its dimension-agnostic Quickhull carries a specialized monotone-chain 2D path
- ABI: generic `TVertex: IVertex`, `TFace: ConvexFace<TVertex, TFace>, new()`, and `TCell: TriangulationCell<...>, new()` contracts over `IList<TVertex>` and `double[]`
- Rail: typed-result convex hull, Delaunay, and Voronoi

## [02]-[PUBLIC_TYPES]

The public vocabulary comprises static entrypoints, vertex contracts, typed results, hulls, cells, and Voronoi edges. Public allocation plumbing such as `FaceConnector`, `ConvexFaceInternal`, `ObjectManager`, and `IndexBuffer` remains outside the consumer surface.

| [INDEX] | [SYMBOL]                                   | [KIND]        | [ROLE]                  |
| :-----: | :----------------------------------------- | :------------ | :---------------------- |
|  [01]   | `ConvexHull`                               | static class  | hull entrypoint         |
|  [02]   | `Triangulation`                            | static class  | Delaunay and Voronoi    |
|  [03]   | `VoronoiMesh`                              | static class  | Voronoi entrypoint      |
|  [04]   | `IVertex`                                  | interface     | N-dimensional point     |
|  [05]   | `IVertex2D`                                | interface     | planar point            |
|  [06]   | `DefaultVertex`                            | adapter       | raw coordinates         |
|  [07]   | `DefaultVertex2D`                          | adapter       | raw planar coordinates  |
|  [08]   | `ConvexHullCreationResult<TVertex, TFace>` | result        | hull outcome            |
|  [09]   | `ConvexHullCreationResult<TVertex>`        | result        | planar hull outcome     |
|  [10]   | `ConvexHull<TVertex, TFace>`               | model         | faceted hull            |
|  [11]   | `ConvexHull<TVertex>`                      | model         | ordered planar hull     |
|  [12]   | `ConvexFace<TVertex, TFace>`               | abstract face | hull facet              |
|  [13]   | `TriangulationCell<TVertex, TCell>`        | abstract cell | Delaunay simplex        |
|  [14]   | `DefaultTriangulationCell<TVertex>`        | cell          | default simplex         |
|  [15]   | `VoronoiEdge<TVertex, TCell>`              | edge          | dual adjacency          |
|  [16]   | `VoronoiMesh<TVertex, TCell, TEdge>`       | model         | dual graph              |
|  [17]   | `ConvexHullCreationResultOutcome`          | enum          | result discriminant     |
|  [18]   | `ConvexHullGenerationException`            | exception     | exceptional hull result |

[TYPE_CONTRACTS]:

- `IVertex` exposes `double[] Position`, while `IVertex2D` exposes `double X` and `double Y` for the planar fast path.
- `DefaultVertex` exposes `double[] Position`; `DefaultVertex2D` accepts `double[]` or `double x, double y` when the consumer has no native vertex type.
- `ConvexHullCreationResult<TVertex, TFace>` carries `Result`, `Outcome`, and `ErrorMessage`; the planar specialization returns `IList<TVertex>` through `Result`.
- `ConvexHull<TVertex, TFace>` exposes `Points` and faceted `Faces`, while `ConvexHull<TVertex>` exposes the ordered boundary through `Points`.
- `ConvexFace<TVertex, TFace>` exposes `TFace[] Adjacency`, `TVertex[] Vertices`, and `double[] Normal`; it is the base for `DefaultConvexFace` and `TriangulationCell`.
- `TriangulationCell<TVertex, TCell>` models a triangle in 2D or a tetrahedron in 3D with vertices and adjacency.
- `VoronoiEdge<TVertex, TCell>` exposes `Source` and `Target`; `VoronoiMesh<TVertex, TCell, TEdge>` exposes cell `Vertices` and `Edges`.
- `ConvexHullCreationResultOutcome` includes `Success`, `DimensionSmallerTwo`, `DimensionTwoWrongMethod`, `NotEnoughVerticesForDimension`, `NonUniformDimension`, `DegenerateData`, and `UnknownError`.
- `ConvexHullGenerationException` carries `Error` and `ErrorMessage` for the exceptional path.

[ENTRYPOINT_FAMILIES]:

- `ConvexHull` exposes `Create<TVertex, TFace>`, `Create<TVertex>`, `Create(IList<double[]>)`, `Create2D<TVertex>`, and `Create2D(IList<double[]>)`.
- `Triangulation` exposes `CreateDelaunay<TVertex>`, `CreateDelaunay<TVertex, TFace>`, `CreateDelaunay(IList<double[]>)`, and the `CreateVoronoi` overload family.
- `VoronoiMesh` exposes `Create<TVertex, TCell, TEdge>`, `Create<TVertex>`, and `Create(IList<double[]>)`.

## [03]-[CONVEX_HULL]

`ConvexHull.Create` returns an N-dimensional hull whose `Faces` carry `Vertices`, `Adjacency`, and outward `Normal`; `ConvexHull.Create2D` returns an ordered boundary polygon through `Result.Points`. The N-dimensional entrypoint rejects 2D input with `DimensionTwoWrongMethod`. The `tolerance` parameter defaults to `1E-10` as the coplanarity threshold, and every entrypoint returns a `ConvexHullCreationResult` whose `Outcome` precedes access to `Result`.

| [INDEX] | [SURFACE]                    | [INPUT]           | [ROLE]                  |
| :-----: | :--------------------------- | :---------------- | :---------------------- |
|  [01]   | `Create<TVertex, TFace>`     | `IList<TVertex>`  | custom-face N-D hull    |
|  [02]   | `Create<TVertex>`            | `IList<TVertex>`  | default-face N-D hull   |
|  [03]   | `Create`                     | `IList<double[]>` | raw-coordinate N-D hull |
|  [04]   | `Create2D<TVertex>`          | `IList<TVertex>`  | typed planar hull       |
|  [05]   | `Create2D`                   | `IList<double[]>` | raw-coordinate 2D hull  |
|  [06]   | `ConvexHull<TVertex, TFace>` | result            | points and faces        |
|  [07]   | `ConvexHull<TVertex>`        | result            | ordered boundary        |

[HULL_SIGNATURES]:

- `ConvexHull.Create<TVertex, TFace>(IList<TVertex> data, double tolerance = 1E-10)` returns `ConvexHullCreationResult<TVertex, TFace>` for `TFace: ConvexFace`.
- `ConvexHull.Create<TVertex>(IList<TVertex> data, double tolerance = 1E-10)` returns `ConvexHullCreationResult<TVertex, DefaultConvexFace<TVertex>>`.
- `ConvexHull.Create(IList<double[]> data, double tolerance = 1E-10)` returns `ConvexHullCreationResult<DefaultVertex, DefaultConvexFace<DefaultVertex>>`.
- `ConvexHull.Create2D<TVertex>(IList<TVertex> data, double tolerance = 1E-10)` returns `ConvexHullCreationResult<TVertex>` where `TVertex: IVertex2D, new()`.
- `ConvexHull.Create2D(IList<double[]> data, double tolerance = 1E-10)` returns `ConvexHullCreationResult<DefaultVertex2D>`.

## [04]-[DELAUNAY_AND_VORONOI]

[DELAUNAY]:

`Triangulation.CreateDelaunay` returns an N-dimensional point-cloud cell complex. Its `Cells` are triangles in 2D or tetrahedra in 3D, and every `TriangulationCell: ConvexFace` carries `Vertices` and `Adjacency`. `PlaneDistanceTolerance` defaults to `1E-10` as the cospherical threshold. This point-cloud triangulation does not fill a contour.

| [INDEX] | [SURFACE]                        | [INPUT]           | [ROLE]               |
| :-----: | :------------------------------- | :---------------- | :------------------- |
|  [01]   | `CreateDelaunay<TVertex>`        | `IList<TVertex>`  | default-cell complex |
|  [02]   | `CreateDelaunay<TVertex, TFace>` | `IList<TVertex>`  | custom-cell complex  |
|  [03]   | `CreateDelaunay`                 | `IList<double[]>` | raw-coordinate cloud |
|  [04]   | `ITriangulation<TVertex, TCell>` | result            | simplex collection   |

[DELAUNAY_SIGNATURES]:

- `Triangulation.CreateDelaunay<TVertex>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)` returns `ITriangulation<TVertex, DefaultTriangulationCell<TVertex>>`.
- `Triangulation.CreateDelaunay<TVertex, TFace>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)` accepts `TFace: TriangulationCell` for domain data on each simplex.
- `Triangulation.CreateDelaunay(IList<double[]> data, double PlaneDistanceTolerance = 1E-10)` consumes raw coordinates.
- `ITriangulation<TVertex, TCell>.Cells` returns `IEnumerable<TCell>` whose vertex and adjacency arrays expose mesh connectivity.

[VORONOI]:

`VoronoiMesh.Create` and `Triangulation.CreateVoronoi` expose the Delaunay dual. `Vertices` are Delaunay cells whose circumcenters are Voronoi vertices, while `Edges` are `VoronoiEdge` adjacencies from `Source` to `Target`.

| [INDEX] | [SURFACE]                            | [INPUT]           | [ROLE]               |
| :-----: | :----------------------------------- | :---------------- | :------------------- |
|  [01]   | `Create<TVertex>`                    | `IList<TVertex>`  | default dual         |
|  [02]   | `Create<TVertex, TCell>`             | `IList<TVertex>`  | custom-cell dual     |
|  [03]   | `Create<TVertex, TCell, TEdge>`      | `IList<TVertex>`  | custom-edge dual     |
|  [04]   | `Create`                             | `IList<double[]>` | raw-coordinate dual  |
|  [05]   | `Triangulation.CreateVoronoi`        | overload family   | alternate entrypoint |
|  [06]   | `VoronoiMesh<TVertex, TCell, TEdge>` | result            | cells and edges      |

[VORONOI_SIGNATURES]:

- `VoronoiMesh.Create<TVertex>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)` returns `VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<...>>`.
- `VoronoiMesh.Create<TVertex, TCell, TEdge>(...)`, `Create<TVertex, TCell>(...)`, and `Create(IList<double[]>, ...)` expose custom-cell, custom-edge, and raw-coordinate overloads.
- `Triangulation.CreateVoronoi<...>(...)` exposes the same overload family and result as `VoronoiMesh.Create`.
- `VoronoiMesh<TVertex, TCell, TEdge>.Vertices` returns `IEnumerable<TCell>`; `.Edges` returns `IEnumerable<TEdge>` whose `Source` and `Target` form a Delaunay-cell pair.

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:

- Representation: every entrypoint is generic over `IVertex`, `IVertex2D`, and a face or cell type; the consumer's native type remains the hull or cell vertex, so payload and connectivity survive through `Faces`, `Cells`, and `Edges`.
- Result: `ConvexHull.Create` and `Create2D` return `ConvexHullCreationResult` with `Outcome` and `ErrorMessage`; the kernel folds the outcome into `Fin` or `Validation` at the boundary.
- Dimensionality: `Create` rejects 2D with `DimensionTwoWrongMethod`; `Create2D` returns the ordered hull-boundary polygon, while `CreateDelaunay` and `CreateVoronoi` remain dimension-agnostic.
- Numeric profile: coordinates are `double[]`; `tolerance` and `PlaneDistanceTolerance` default to `1E-10` for coplanarity or cospherical classification.
- Robustness: the double-precision Quickhull reports degenerate or near-cospherical input as `DegenerateData` instead of an exact result.
- Boundary: the object-model API uses `IList<TVertex>` and `double[]`, without `System.Numerics` generic math or `Span`-first kernels.

[LOCAL_ADMISSION]:

- `MIConvexHull` is the kernel's `[COMPUTATIONAL_GEOMETRY]` owner and direct `PackageReference` for convex hull, Delaunay, and Voronoi.
- The central pin is also the Materials `InteractionDiagram` transitive floor for assembling N-M-M structural-capacity vertices into a closed onion hull.
- The kernel implements `IVertex` on its point type or adapts `Rasm.Spatial`, carries its index and payload on the vertex or custom face, and reads connectivity from `Faces`, `Cells`, or `Edges`.
- Typed vertices remain native instead of round-tripping through `double[]`.
- `ConvexHullCreationResult.Outcome` folds into the kernel's `Fin` or `Validation` boundary rail before `Result` is read.
- `Create2D` owns planar-section and interaction-curve hulls because the N-dimensional `Create` returns `DimensionTwoWrongMethod` for 2D input.

[STACKING_LAW]:

- Vendored NURBS engine: `Parametric/nurbs` carries no hull concern, so `MIConvexHull` remains the kernel hull owner.
- Kernel Delaunay: `Meshing/delaunay` owns constrained Delaunay over planar straight-line graphs, including boundary and hole recovery over the exact-predicate floor and Ruppert-style quality refinement; `MIConvexHull` owns unconstrained N-dimensional point-cloud Delaunay without boundary constraints or quality refinement.
- Spatial queries: `Supercluster.KDTree` answers nearest-neighbour queries over a fixed cloud without connectivity; `CreateDelaunay` returns the cloud's cell complex.
- Polygon fill: `Meshing/delaunay` and `Meshing/arrangement` own winding-classified bounded-area fill; `CreateDelaunay` connects unbounded scattered points without a boundary.
- Voronoi: the `Meshing/delaunay` dual and `Meshing/mesh` restricted power diagram own constrained and restricted exact lanes; `VoronoiMesh` owns the N-dimensional Delaunay-dual cell and edge graph.

[RAIL_LAW]:

- Package: `MIConvexHull`
- Owns: typed-result N-dimensional Quickhull through `ConvexHull.Create<TVertex, TFace>`, `Create<TVertex>`, and `Create(IList<double[]>)`
- Owns: typed-result planar monotone-chain hulls through `Create2D<TVertex>` and `Create2D(IList<double[]>)`
- Owns: N-dimensional Delaunay cell complexes through `Triangulation.CreateDelaunay`
- Owns: Delaunay-dual Voronoi graphs through `VoronoiMesh.Create` and `Triangulation.CreateVoronoi`
- Owns: the `IVertex` and `IVertex2D` point contracts with `DefaultVertex` and `DefaultVertex2D` adapters
- Owns: `ConvexFace`, `TriangulationCell`, and `VoronoiEdge` connectivity through `Adjacency`, `Vertices`, and `Normal`
- Owns: the `ConvexHullCreationResult` and `ConvexHullCreationResultOutcome` typed error rail
- Owns: the kernel's `[COMPUTATIONAL_GEOMETRY]` concern and the Materials `InteractionDiagram` transitive floor under one central pin
- Accept: N-dimensional or 2D convex hulls, N-dimensional Delaunay triangulations, and N-dimensional Voronoi diagrams over `IVertex` or `IVertex2D` point sets
- Accept: domain payload on typed vertices, connectivity through `Faces`, `Cells`, or `Edges`, and `ConvexHullCreationResult.Outcome` folds into `Fin` or `Validation`
- Accept: closed-onion-hull assembly of structural-capacity vertices downstream in Materials
- Reject: `ConvexHull.Create` for 2D data; `Create2D` owns the planar path after `DimensionTwoWrongMethod`
- Reject: reading a possibly null `Result` before its `Outcome`, or using `ConvexHullGenerationException` in place of the typed-result fold
- Reject: a second hull owner beside the typed-result `Create` family
- Reject: constrained 2D meshes, which belong to `Meshing/delaunay`, and 2D edge-clipped Voronoi, which belongs to `api-sharpvoronoilib`
- Reject: consumer use of `FaceConnector`, `ObjectManager`, `IndexBuffer`, or `ConvexFaceInternal`
- Reject: robust or exact arithmetic; near-degenerate input returns `DegenerateData` and escalates to the kernel's exact-predicate ladder
