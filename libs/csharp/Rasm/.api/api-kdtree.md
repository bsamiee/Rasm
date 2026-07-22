# [RASM_API_KDTREE]

`Supercluster.KDTree.Net` owns the kernel's generic, array-backed exact-k-NN kd-tree: a build-once balanced binary tree over `INumber<TDimension>` coordinates serving `NearestNeighbors` k-nearest and `RadialSearch` radius queries under a `DistanceMetrics`-selected or custom `Func` metric. It is the discrete point-nearest leaf for static point clouds, feeding the fit and registration rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Supercluster.KDTree.Net`
- package: `Supercluster.KDTree.Net` (MIT, `micampbell` fork of SuperClusterInc)
- assembly: `KDTree.dll` — the package id differs from assembly and namespace; the import is `using SuperClusterKDTree;`
- namespace: `SuperClusterKDTree`
- target: `net8.0` only, bound forward by the `net10.0` consumer
- asset: pure-managed AnyCPU, zero package dependencies
- abi: `System.Numerics` generic math over `INumber`/`IMinMaxValue`, binding any conforming scalar coordinate with no adapter
- rail: exact low-dimensional point k-NN and radius search

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the factory, the tree, and the metric vocabulary

`KDTree<TDimension,TPriority,TNode>` is generic over `TDimension` (coordinate scalar), `TPriority` (distance scalar, usually `TDimension`), and `TNode` (per-point payload).

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `KDTree`                             | class         | static metric-bound build factory     |
|  [02]   | `KDTree<TDimension,TPriority,TNode>` | class         | balanced tree, exact point search     |
|  [03]   | `DistanceMetrics`                    | enum          | Manhattan/Euclidean²/Chebyshev/Cosine |

## [03]-[CONSTRUCTION]

[CONSTRUCTION_SCOPE]: the factory, the raw constructors, and the built tree's state

`KDTree.Create(IList<IReadOnlyList<TDimension>>, IList<TNode>, DistanceMetrics) -> KDTree<TDimension,TDimension,TNode>` wires a built-in metric and infers `TPriority = TDimension`. Two raw constructors own a custom `Func` metric and optional `searchWindow` min/max clamps — one over an `ICollection` point set, one over a lazy `IEnumerable` with an explicit `pointsCount`.

| [INDEX] | [SURFACE]                   | [SHAPE]  | [CAPABILITY]             |
| :-----: | :-------------------------- | :------- | :----------------------- |
|  [01]   | `KDTree.Create`             | factory  | built-in metric          |
|  [02]   | `KDTree(…, ICollection, …)` | ctor     | custom metric            |
|  [03]   | `KDTree(…, IEnumerable, …)` | ctor     | lazy point source        |
|  [04]   | `Count`                     | property | indexed point count      |
|  [05]   | `Dimensions`                | property | fixed dimensionality     |
|  [06]   | `InternalPointArray`        | property | balanced point storage   |
|  [07]   | `InternalNodeArray`         | property | parallel payload storage |
|  [08]   | `Metric`                    | property | swappable metric         |

## [04]-[QUERY]

[QUERY_SCOPE]: exact k-nearest, radius search, and the static distance functions

`NearestNeighbors(point, k)` and `RadialSearch(center, radius, k=-1)` return `IEnumerable<(IReadOnlyList<TDimension>, TNode)>`, each hit carrying its coordinate and payload. `RadialSearch` returns every hit inside `radius` at `k=-1` and otherwise caps at `k`.

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]         |
| :-----: | :------------------------- | :------- | :------------------- |
|  [01]   | `NearestNeighbors`         | instance | exact k-nearest hits |
|  [02]   | `RadialSearch`             | instance | radius-bounded hits  |
|  [03]   | `KDTree.EuclideanDistance` | static   | squared L2 distance  |
|  [04]   | `KDTree.ManhattanDistance` | static   | L1 distance          |
|  [05]   | `KDTree.ChebyshevDistance` | static   | L∞ distance          |
|  [06]   | `KDTree.CosineDistance`    | static   | cosine distance      |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- representation: a static-balanced binary tree stored as flat parallel arrays (`InternalPointArray`, `InternalNodeArray`) with implicit children at `2i+1`/`2i+2`; a point is an `IReadOnlyList<TDimension>` of any dimensionality fixed at build via `Dimensions`, so the tree is generic N-dimensional.
- generic math: the `Create` factory binds `TDimension: INumber<TDimension>, IMinMaxValue<TDimension>`; the raw tree relaxes to `TDimension: IComparable<TDimension>, IMinMaxValue<TDimension>` with `TPriority: INumber<TPriority>, IMinMaxValue<TPriority>`, and `IMinMaxValue` supplies the ±∞ split-region sentinels.
- immutability: the tree balances once at construction by median split and is then read-only; a point-set change is a rebuild.
- metric semantics: `EuclideanDistance` returns squared L2 (skips the sqrt), so a `RadialSearch` radius and priority comparison under it are squared units; `Metric` accepts a custom `Func<IReadOnlyList<T>,IReadOnlyList<T>,TPriority>`.

[STACKING]:
- `MIConvexHull`(`.api/api-miconvexhull.md`): `Triangulation.CreateDelaunay` yields a cell complex (connectivity) over the same cloud, this tree yields nearest-neighbour queries (no connectivity) — a fixed-cloud k-NN routes here, a triangulation there.
- `DoubleDouble`(`.api/api-doubledouble.md`): `ddouble` coordinates bind straight through the `INumber<TDimension>` constraint, so a near-coincident precision-critical cloud indexes at 106-bit through the same generic metric.
- within-lib: the kernel BVH/octree (`Spatial/index`) and the NURBS `ClosestParameter` (`Parametric/nurbs`) are disjoint acceleration owners by query shape — this tree owns discrete point k-NN and radius, the BVH/octree primitive overlap and ray, the engine continuous single-carrier parametric projection.

[LOCAL_ADMISSION]:
- admitted for `Solving/fit` (MLESAC primitive-fit, normal estimation via local k-NN PCA) and the `registration/ICP` per-iteration nearest-source query over a static cloud.
- `Rasm.Spatial` points map to `IReadOnlyList<TDimension>` at the boundary with the `Rasm` index or payload carried as `TNode`, recovered from the `(point, payload)` tuple; the tree never holds a kernel type.
- `KDTree.Create(points, nodes, DistanceMetrics.EuclideanDistance)` is the admitted build; the raw constructor is reserved for a custom metric or search window.

[RAIL_LAW]:
- Package: `Supercluster.KDTree.Net`
- Owns: the generic N-dimensional exact-k-NN kd-tree over `INumber<TDimension>` coordinates and an arbitrary `TNode` payload — build-once balanced tree, k-nearest and radius queries returning `(coordinate, payload)`, built-in or custom metric.
- Accept: exact k-NN or radius search over a static low-dimensional cloud (sample sets, normal-estimation neighbourhoods, ICP correspondence); a `Rasm.Spatial` cloud mapped to `IReadOnlyList<TDimension>` with the index or payload as `TNode`; a `ddouble`/`float`/`Half` coordinate bound through generic math.
- Reject: treating it as a primitive broad-phase or ray-query structure (the kernel BVH/octree owns those); expecting incremental insert or delete (build-once, rebuild on change); passing an un-squared radius under `EuclideanDistance`; hand-writing a Euclidean `Func` the enum already wires; re-implementing continuous curve/surface closest-point (the `Parametric/nurbs` engine's `ClosestParameter` owns it).
