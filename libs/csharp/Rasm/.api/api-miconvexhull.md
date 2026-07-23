# [RASM_API_MICONVEXHULL]

`MIConvexHull` owns the kernel's typed-result computational geometry: the convex hull, Delaunay triangulation, and Voronoi diagram over one point set. Its N-D Quickhull and dedicated 2D monotone-chain path stay generic over the consumer's `IVertex`/`IVertex2D` and face or cell type, so native vertex payload and connectivity survive the hull.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MIConvexHull`
- package: `MIConvexHull` (MIT)
- assembly: `MIConvexHull.dll`
- namespace: `MIConvexHull`
- target: `netstandard2.0`, the `net10.0` consumer's bound asset
- asset: pure-managed AnyCPU, no native runtime or package dependency
- abi: generic `TVertex: IVertex`, `TFace: ConvexFace<TVertex, TFace>, new()`, `TCell: TriangulationCell<…>, new()` over `IList<TVertex>` and `double[]`
- rail: typed-result convex hull, Delaunay, and Voronoi

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: static entrypoints, vertex contracts, typed results, hulls, cells, and Voronoi edges.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :----------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `ConvexHull`                               | static class   | N-D and 2D hull factory                           |
|  [02]   | `Triangulation`                            | static class   | Delaunay and Voronoi factory                      |
|  [03]   | `VoronoiMesh`                              | static class   | Voronoi-dual factory                              |
|  [04]   | `IVertex`                                  | interface      | N-dimensional point contract                      |
|  [05]   | `IVertex2D`                                | interface      | planar point contract                             |
|  [06]   | `DefaultVertex`                            | class          | raw-coordinate N-D vertex                         |
|  [07]   | `DefaultVertex2D`                          | struct         | raw-coordinate planar vertex                      |
|  [08]   | `ConvexHullCreationResult<TVertex, TFace>` | class          | faceted-hull outcome carrier                      |
|  [09]   | `ConvexHullCreationResult<TVertex>`        | class          | planar-hull outcome carrier                       |
|  [10]   | `ConvexHull<TVertex, TFace>`               | class          | faceted hull (`Points`, `Faces`)                  |
|  [11]   | `ConvexHull<TVertex>`                      | class          | ordered planar boundary (`Points`)                |
|  [12]   | `ConvexFace<TVertex, TFace>`               | abstract class | hull facet (`Adjacency`, `Vertices`, `Normal`)    |
|  [13]   | `TriangulationCell<TVertex, TCell>`        | abstract class | Delaunay simplex                                  |
|  [14]   | `DefaultTriangulationCell<TVertex>`        | class          | default simplex cell                              |
|  [15]   | `VoronoiEdge<TVertex, TCell>`              | class          | dual adjacency (`Source`, `Target`)               |
|  [16]   | `VoronoiMesh<TVertex, TCell, TEdge>`       | class          | dual graph (`Vertices`, `Edges`)                  |
|  [17]   | `ConvexHullCreationResultOutcome`          | enum           | result discriminant                               |
|  [18]   | `ConvexHullGenerationException`            | class          | exceptional hull result (`Error`, `ErrorMessage`) |

[TYPE_CONTRACTS]:
- `IVertex` carries `double[] Position`; `IVertex2D` carries `double X`/`double Y`; `DefaultVertex2D` admits `double[]` or `(double x, double y)`.
- `ConvexHullCreationResult` carries `Result`, `Outcome`, and `ErrorMessage`; the planar specialization returns `IList<TVertex>` through `Result`.
- `[ConvexHullCreationResultOutcome]`: `Success` `DimensionSmallerTwo` `DimensionTwoWrongMethod` `NotEnoughVerticesForDimension` `NonUniformDimension` `DegenerateData` `UnknownError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static hull, Delaunay, and Voronoi factories over `IList<TVertex>` or raw `IList<double[]>`.

`CreateVoronoi` and `VoronoiMesh.Create` share one overload family and result; `tolerance` and `PlaneDistanceTolerance` default to `1E-10`, the coplanarity or cospherical threshold.

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `ConvexHull.Create<TVertex, TFace>(IList<TVertex>)`                  | static  | custom-face N-D hull              |
|  [02]   | `ConvexHull.Create<TVertex>(IList<TVertex>)`                         | static  | default-face N-D hull             |
|  [03]   | `ConvexHull.Create(IList<double[]>)`                                 | static  | raw-coordinate N-D hull           |
|  [04]   | `ConvexHull.Create2D<TVertex>(IList<TVertex>)`                       | static  | typed planar monotone-chain hull  |
|  [05]   | `ConvexHull.Create2D(IList<double[]>)`                               | static  | raw-coordinate planar hull        |
|  [06]   | `Triangulation.CreateDelaunay<TVertex>(IList<TVertex>)`              | static  | default-cell N-D Delaunay complex |
|  [07]   | `Triangulation.CreateDelaunay<TVertex, TFace>(IList<TVertex>)`       | static  | custom-cell Delaunay complex      |
|  [08]   | `Triangulation.CreateDelaunay(IList<double[]>)`                      | static  | raw-coordinate Delaunay complex   |
|  [09]   | `Triangulation.CreateVoronoi<TVertex, TCell, TEdge>(IList<TVertex>)` | static  | Delaunay-dual (VoronoiMesh alias) |
|  [10]   | `VoronoiMesh.Create<TVertex>(IList<TVertex>)`                        | static  | default-cell dual                 |
|  [11]   | `VoronoiMesh.Create<TVertex, TCell>(IList<TVertex>)`                 | static  | custom-cell dual                  |
|  [12]   | `VoronoiMesh.Create<TVertex, TCell, TEdge>(IList<TVertex>)`          | static  | custom-edge dual                  |
|  [13]   | `VoronoiMesh.Create(IList<double[]>)`                                | static  | raw-coordinate dual               |

- `TriangulationCell` derives from `ConvexFace`, so a Delaunay cell carries `Vertices` and `Adjacency`; `ITriangulation<TVertex, TCell>.Cells` enumerates the simplices, and `VoronoiEdge.Source`/`Target` are the Delaunay-cell pair whose circumcenters are the Voronoi vertices.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every entrypoint is generic over the consumer's vertex and face or cell type, so native payload and connectivity survive through `Faces`, `Cells`, and `Edges` without a `double[]` round-trip.
- `Outcome` gates `Result`: a possibly-null `Result` reads only after `Outcome` reports `Success`, and the outcome folds into `Fin`/`Validation` at the boundary, never through `ConvexHullGenerationException`.
- Coordinates are double-precision `double[]`; the Quickhull is inexact, reporting near-cospherical or degenerate input as `DegenerateData`.

[STACKING]:
- `Supercluster.KDTree`(`.api/api-kdtree.md`): the kd-tree answers exact k-NN and radius queries over a fixed cloud without connectivity while `CreateDelaunay` returns that cloud's cell complex — nearest-neighbour lookup composes beside topological connectivity over one point set.
- kernel fold: kernel code implements `IVertex`/`IVertex2D` on its point type or adapts `Rasm.Spatial`, carries index and payload on the vertex or a custom `ConvexFace`, and reads connectivity from `Faces`, `Cells`, or `Edges`.

[LOCAL_ADMISSION]:
- `MIConvexHull` is the kernel `[COMPUTATIONAL_GEOMETRY]` owner and direct `PackageReference`, and `Create2D` owns the planar-section and interaction-curve hulls the N-D `Create` rejects.

[RAIL_LAW]:
- Package: `MIConvexHull`
- Owns: unconstrained N-D and planar convex hulls (`Create`/`Create2D`), N-D Delaunay cell complexes (`CreateDelaunay`), Delaunay-dual Voronoi graphs (`VoronoiMesh.Create`/`CreateVoronoi`), the `IVertex`/`IVertex2D` contracts, and the `ConvexHullCreationResult`/`ConvexHullCreationResultOutcome` typed error rail.
- Accept: N-D or 2D hulls, N-D Delaunay complexes, and N-D Voronoi diagrams over `IVertex`/`IVertex2D` sets with domain payload on typed vertices and connectivity through `Faces`, `Cells`, or `Edges`.
- Accept: Materials' `InteractionDiagram` welding `(N, My, Mz)` capacity points into the closed-onion capacity hull through `ConvexHull.Create<ConvexHullVertex, ConvexHullFace>`.
- Reject: `Create` for 2D data; `Create2D` owns the planar path after `DimensionTwoWrongMethod`.
- Reject: reading `Result` before `Outcome`, or `ConvexHullGenerationException` in place of the typed-result fold.
- Reject: a second hull owner, constrained 2D meshes (`Meshing/delaunay`), and 2D border-clipped point-site Voronoi (`SharpVoronoiLib`, Fabrication).
- Reject: consumer use of `FaceConnector`, `ObjectManager`, `IndexBuffer`, or `ConvexFaceInternal`.
- Reject: robust or exact arithmetic; near-degenerate input returns `DegenerateData` and escalates to the kernel exact-predicate ladder.
