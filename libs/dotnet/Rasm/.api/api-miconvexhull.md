# [RASM_API_MICONVEXHULL]

`MIConvexHull` owns the kernel's typed-result computational geometry: the convex hull, Delaunay triangulation, and Voronoi diagram over one point set. Its N-D Quickhull and dedicated 2D monotone-chain path stay generic over the consumer's `IVertex`/`IVertex2D` and face or cell type, so native vertex payload and connectivity survive the hull.

## [01]-[PUBLIC_TYPES]

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
|  [09]   | `ConvexHullCreationResult<TVertex>`        | class          | planar-hull outcome carrier, `TVertex` `new()`    |
|  [10]   | `ConvexHull<TVertex, TFace>`               | class          | faceted hull (`Points`, `Faces`)                  |
|  [11]   | `ConvexHull<TVertex>`                      | class          | planar boundary (`Points`), `TVertex` `new()`     |
|  [12]   | `ConvexFace<TVertex, TFace>`               | abstract class | hull facet (`Adjacency`, `Vertices`, `Normal`)    |
|  [13]   | `DefaultConvexFace<TVertex>`               | class          | default facet the untyped-face rows return        |
|  [14]   | `TriangulationCell<TVertex, TCell>`        | abstract class | Delaunay simplex                                  |
|  [15]   | `DefaultTriangulationCell<TVertex>`        | class          | default simplex cell                              |
|  [16]   | `VoronoiEdge<TVertex, TCell>`              | class          | dual adjacency (`Source`, `Target`)               |
|  [17]   | `VoronoiMesh<TVertex, TCell, TEdge>`       | class          | dual graph (`Vertices`, `Edges`)                  |
|  [18]   | `ITriangulation<TVertex, TCell>`           | interface      | cell-complex read surface (`Cells`)               |
|  [19]   | `DelaunayTriangulation<TVertex, TCell>`    | class          | the `ITriangulation` implementation               |
|  [20]   | `ConvexHullCreationResultOutcome`          | enum           | result discriminant                               |
|  [21]   | `ConvexHullGenerationException`            | class          | exceptional hull result (`Error`, `ErrorMessage`) |

[TYPE_CONTRACTS]:
- `IVertex` carries `double[] Position`; `IVertex2D` carries `double X`/`double Y`; `DefaultVertex2D` admits `double[]` or `(double x, double y)`.
- `ConvexHullCreationResult` carries `Result`, `Outcome`, and `ErrorMessage`; the planar specialization returns `IList<TVertex>` through `Result`.
- Planar family types constrain `TVertex : IVertex2D, new()` on the RESULT and HULL types as well as on `Create2D`, so the parameterless-constructor obligation reaches a consumer that only names `ConvexHull<TVertex>` in a signature and never calls the factory.
- `VoronoiEdge` declares both a parameterless and a `(TCell source, TCell target)` constructor, and `Source`/`Target` are `internal set` — a consumer subclass reads the endpoints and never writes them, so a derived edge carries derived reads alone.
- `ConvexHullGenerationException.Error` is a `ConvexHullCreationResultOutcome`, so the throwing and returning families answer degeneracy in ONE vocabulary and a `catch` recovers the same typed cause a result publishes.
- `[ConvexHullCreationResultOutcome]`: `Success` `DimensionSmallerTwo` `DimensionTwoWrongMethod` `NotEnoughVerticesForDimension` `NonUniformDimension` `DegenerateData` `UnknownError`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static hull, Delaunay, and Voronoi factories over `IList<TVertex>` or raw `IList<double[]>`.

EVERY row takes a SECOND parameter — a trailing `double` defaulting to `1E-10`, the coplanarity or cospherical threshold — spelled `tolerance` on the `ConvexHull` rows and `PlaneDistanceTolerance` on the `Triangulation` and `VoronoiMesh` rows, so a caller threads its own domain epsilon positionally or by either name and never re-scales its input to reach the default. `CreateVoronoi` and `VoronoiMesh.Create` share one overload family and one result type. `[CTOR_BOUND]` names the type parameters carrying a `new()` constraint beside their base-type bound.

| [INDEX] | [SURFACE]                                                           | [CTOR_BOUND] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------ | :----------- | :-------------------------------- |
|  [01]   | `ConvexHull.Create<TVertex, TFace>(IList<TVertex>, tolerance)`      | TFace        | custom-face N-D hull              |
|  [02]   | `ConvexHull.Create<TVertex>(IList<TVertex>, tolerance)`             | —            | default-face N-D hull             |
|  [03]   | `ConvexHull.Create(IList<double[]>, tolerance)`                     | —            | raw-coordinate N-D hull           |
|  [04]   | `ConvexHull.Create2D<TVertex>(IList<TVertex>, tolerance)`           | TVertex      | typed planar monotone-chain hull  |
|  [05]   | `ConvexHull.Create2D(IList<double[]>, tolerance)`                   | —            | raw-coordinate planar hull        |
|  [06]   | `Triangulation.CreateDelaunay<TVertex>(IList<TVertex>, tol)`        | —            | default-cell N-D Delaunay complex |
|  [07]   | `Triangulation.CreateDelaunay<TVertex, TFace>(IList<TVertex>, tol)` | TFace        | custom-cell Delaunay complex      |
|  [08]   | `Triangulation.CreateDelaunay(IList<double[]>, tol)`                | —            | raw-coordinate Delaunay complex   |
|  [09]   | `Triangulation.CreateVoronoi<TVertex, TCell, TEdge>(IList<…>, tol)` | TCell TEdge  | Delaunay-dual (VoronoiMesh alias) |
|  [10]   | `VoronoiMesh.Create<TVertex>(IList<TVertex>, tol)`                  | —            | default-cell dual                 |
|  [11]   | `VoronoiMesh.Create<TVertex, TCell>(IList<TVertex>, tol)`           | TCell        | custom-cell dual                  |
|  [12]   | `VoronoiMesh.Create<TVertex, TCell, TEdge>(IList<TVertex>, tol)`    | TCell TEdge  | custom-edge dual                  |
|  [13]   | `VoronoiMesh.Create(IList<double[]>, tol)`                          | —            | raw-coordinate dual               |

- `TVertex` bounds to `IVertex` on every N-D row and to `IVertex2D, new()` on `Create2D<TVertex>`; `TFace` bounds to `ConvexFace<TVertex, TFace>, new()` on the hull rows and `TriangulationCell<TVertex, TFace>, new()` on the Delaunay rows; `TCell` to `TriangulationCell<TVertex, TCell>, new()` and `TEdge` to `VoronoiEdge<TVertex, TCell>, new()`. That obligation is a compile error for the shape a kernel writer reaches for first — a custom face, cell, or edge whose primary constructor carries required parameters — so each is a settable-property class the library fills after allocating it.
- A struct satisfies `new()` structurally, so the planar bound admits a `readonly record struct` vertex and then mints a zero-coordinate ghost through `default`; the disqualification is that default-hostility, not the constraint.
- Default-face and default-cell rows are not untyped: `Create<TVertex>` returns `ConvexHullCreationResult<TVertex, DefaultConvexFace<TVertex>>`, `CreateDelaunay<TVertex>` returns `ITriangulation<TVertex, DefaultTriangulationCell<TVertex>>`, and the default duals close over `VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>`, so a caller naming the result type spells the default face or cell rather than a bare `TVertex`.
- `TriangulationCell` derives from `ConvexFace`, so a Delaunay cell carries `Vertices` and `Adjacency`; `ITriangulation<TVertex, TCell>.Cells` enumerates the simplices, and `VoronoiEdge.Source`/`Target` are the Delaunay-cell pair whose circumcenters are the Voronoi vertices.
- `Adjacency` nullability splits by family: a Delaunay cell leaves the slot NULL opposite each hull facet — the dual builder skips exactly those, so an unbounded region contributes no edge — while a closed convex hull fills every slot, making a null there a torn hull rather than a boundary.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every entrypoint is generic over the consumer's vertex and face or cell type, so native payload and connectivity survive through `Faces`, `Cells`, and `Edges` without a `double[]` round-trip.
- FAILURE MODE SPLITS BY FAMILY, and it is the first fact a consumer needs. `ConvexHull.Create`/`Create2D` catch `ConvexHullGenerationException` AND every other exception internally, so they never throw for domain input and always return the typed outcome; `Triangulation.CreateDelaunay`/`CreateVoronoi` and `VoronoiMesh.Create` catch nothing and PROPAGATE. So the hull family folds `Outcome` straight onto the rail with no exception seam, while the triangulation and dual families need one. Both leave `data == null` throwing `ArgumentNullException`, which is a caller defect and not a domain outcome.
- `Outcome` gates `Result`: a possibly-null `Result` reads only after `Outcome` reports `Success`, and the outcome folds into `Fin`/`Validation` at the boundary. On the throwing families the caught `ConvexHullGenerationException.Error` is that same outcome, so the exception seam recovers a typed cause rather than a flattened message.
- `CreateDelaunay` over an EMPTY set returns an empty complex instead of throwing, so a count floor is the caller's admission gate, never an inferred exception.
- Coordinates are double-precision `double[]`; the Quickhull is inexact, reporting near-cospherical or degenerate input as `DegenerateData`. Tolerance is the caller's LEVER over exactly that verdict — a domain epsilon threaded there moves the coplanarity band instead of leaving the `1E-10` default to decide against unscaled model coordinates.
- The face, cell, and edge types are library-allocated: the algorithm materializes each and fills the inherited properties, so they are mutable-by-construction carriers, never immutable records the caller hands in fully formed. The VERTEX type is not — every hull and complex returns the caller's own instances, so the planar `new()` is a declaration-site bound the monotone-chain path never exercises and consumer payload on a vertex survives by reference identity.

[STACKING]:
- `Supercluster.KDTree`(`.api/api-kdtree.md`): the kd-tree answers exact k-NN and radius queries over a fixed cloud without connectivity while `CreateDelaunay` returns that cloud's cell complex — nearest-neighbour lookup composes beside topological connectivity over one point set.
- kernel fold: kernel code implements `IVertex`/`IVertex2D` on its point type or adapts `Rasm.Spatial`, carries index and payload on the vertex or a custom `ConvexFace`, and reads connectivity from `Faces`, `Cells`, or `Edges`. `Spatial/cloud` seats the whole carrier set — `CloudVertex`/`CloudPlanarVertex` carry the cluster index, `CloudCell` the circumsphere columns, `CloudFace` the facet adjacency and outward normal, `CloudVoronoiEdge` the dual segment — and `CloudHullRejection` keys the outcome vocabulary off the package ordinals so the returning and throwing families report one cause.

[LOCAL_ADMISSION]:
- `MIConvexHull` is the kernel `[COMPUTATIONAL_GEOMETRY]` owner and direct `PackageReference`, and `Create2D` owns the planar-section and interaction-curve hulls the N-D `Create` rejects — `Spatial/cloud` `IndexedFootprint2D` is that route, kept beside the host `PolylineCurve.CreateConvexHull2d` row because the typed path returns the caller's own instances and the host path returns bare coordinates.
