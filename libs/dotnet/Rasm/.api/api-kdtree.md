# [RASM_API_KDTREE]

`Supercluster.KDTree.Net` owns the kernel's generic, array-backed exact-k-NN kd-tree: a build-once balanced binary tree over `INumber<TDimension>` coordinates serving `NearestNeighbors` k-nearest and `RadialSearch` radius queries under a `DistanceMetrics`-selected or custom `Func` metric. It is the discrete point-nearest leaf for static point clouds, feeding the fit and registration owners.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the factory, the tree, and the metric vocabulary

`KDTree<TDimension,TPriority,TNode>` is generic over `TDimension` (coordinate scalar), `TPriority` (distance scalar, usually `TDimension`), and `TNode` (per-point payload).

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `KDTree`                             | class         | static metric-bound build factory                |
|  [02]   | `KDTree<TDimension,TPriority,TNode>` | class         | balanced tree, exact point search                |
|  [03]   | `DistanceMetrics`                    | enum          | Manhattan/Euclidean²/Chebyshev exact · Cosine no |

- `DistanceMetrics` rosters FOUR rows and only the three Lp rows are sound through this structure. Pruning drops a subtree by `Metric(rect.GetClosestPoint(target), target)` against the incumbent worst priority — valid for a coordinate-monotone metric alone. `Cosine` is scale-invariant, so a point inside a rejected halfspace sits arbitrarily nearer in cosine terms and the result set is APPROXIMATE, not the exact set the `[01]` summary line promises.

## [02]-[CONSTRUCTION]

[CONSTRUCTION_SCOPE]: the factory, the raw constructors, and the built tree's state

`KDTree.Create(IList<IReadOnlyList<TDimension>>, IList<TNode>, DistanceMetrics) -> KDTree<TDimension,TDimension,TNode>` wires a built-in metric and infers `TPriority = TDimension`. Two raw constructors own a custom `Func` metric and optional `searchWindow` min/max clamps — one over an `ICollection` point set, one over a lazy `IEnumerable` with an explicit `pointsCount`.

- `Create` reads `points[0].Count` for the dimensionality, so an EMPTY point set throws a raw `ArgumentOutOfRangeException` before any metric runs; a count floor is the caller's admission gate.
- `DistanceMetrics` is the FACTORY's argument and nothing else — `Create` switches it into a lambda once and stores the result. The `Metric` property is a `Func<IReadOnlyList<TDimension>, IReadOnlyList<TDimension>, TPriority>`, so a metric VALUE is one of the `KDTree` statics as a method group; assigning the enum member there is a type error, and the two spellings are not interchangeable.

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

## [03]-[QUERY]

[QUERY_SCOPE]: exact k-nearest, radius search, and the static distance functions

`NearestNeighbors(point, numNeighbors)` and `RadialSearch(center, radius, numNeighbors = -1)` return `IEnumerable<(IReadOnlyList<TDimension>, TNode)>`, each hit carrying its coordinate and payload. `RadialSearch` returns every hit inside `radius` at `numNeighbors = -1` and otherwise caps at that count. The cap parameter is spelled `numNeighbors` on both, so a named argument spelling it `k` does not compile.

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `NearestNeighbors`         | instance | exact k-nearest hits under an Lp metric                |
|  [02]   | `RadialSearch`             | instance | radius-bounded hits under an Lp metric                 |
|  [03]   | `KDTree.EuclideanDistance` | static   | squared L2 distance                                    |
|  [04]   | `KDTree.ManhattanDistance` | static   | L1 distance                                            |
|  [05]   | `KDTree.ChebyshevDistance` | static   | L∞ distance                                            |
|  [06]   | `KDTree.CosineDistance`    | static   | cosine — prune-unsound, `double`-narrowed, sentinelled |

- The three Lp statics bound `TDimension : INumber<TDimension>` alone and stay in `TDimension` end to end; `CosineDistance` additionally demands `IMinMaxValue<TDimension>` for the sentinel it returns, so the narrower bound is itself the tell.
- `CosineDistance` breaks the generic contract twice over. Its tail reads `TDimension.CreateChecked(double.Sqrt(double.CreateChecked(zero2 * zero3)))`, so the norm product round-trips through `double` whatever `TDimension` is and a `ddouble` cosine tree silently answers at 53 bits; and it returns `TDimension.One` on a zero dot product and `TDimension.One + TDimension.One` on a zero-norm operand — sentinel verdicts, not distances, so the priority ordering a search folds over is not a metric ordering. Neither defect touches the three Lp statics.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- representation: a static-balanced binary tree stored as flat parallel arrays (`InternalPointArray`, `InternalNodeArray`) with implicit children at `2i+1`/`2i+2`; a point is an `IReadOnlyList<TDimension>` of any dimensionality fixed at build via `Dimensions`, so the tree is generic N-dimensional.
- generic math: the `Create` factory binds `TDimension: INumber<TDimension>, IMinMaxValue<TDimension>`; the raw tree relaxes to `TDimension: IComparable<TDimension>, IMinMaxValue<TDimension>` with `TPriority: INumber<TPriority>, IMinMaxValue<TPriority>`, and `IMinMaxValue` supplies the ±∞ split-region sentinels.
- immutability: the tree balances once at construction by median split and is then read-only; a point-set change is a rebuild.
- metric semantics: `EuclideanDistance` returns squared L2 (skips the sqrt), so a `RadialSearch` radius and priority comparison under it are squared units; `Metric` accepts a custom `Func<IReadOnlyList<T>,IReadOnlyList<T>,TPriority>`.
- prune soundness: exactness is a property of the METRIC, not of the structure. Hyperrect pruning compares a metric value at the rejected region's closest point against the incumbent worst, so any metric a custom `Func` supplies must be coordinate-monotone — non-decreasing as a coordinate moves away from the target — or the search degrades to approximate with no signal. `Metric` is settable on a BUILT tree, so this is the one contract a metric swap owes.

[STACKING]:
- `MIConvexHull`(`.api/api-miconvexhull.md`): `Triangulation.CreateDelaunay` yields a cell complex (connectivity) over the same cloud, this tree yields nearest-neighbour queries (no connectivity) — a fixed-cloud k-NN routes here, a triangulation there.
- `DoubleDouble`(`.api/api-doubledouble.md`): `ddouble` coordinates bind straight through the `INumber<TDimension>` constraint, so a near-coincident precision-critical cloud indexes at 106-bit through the same generic metric — the three Lp statics ALONE, each of which stays in `TDimension` end to end. `CosineDistance` narrows through `double` regardless of the coordinate type, so it reads 53 bits off a 106-bit cloud and this binding does not survive that row. The binding's precondition is a 106-bit coordinate SOURCE: widening `TDimension` over a cloud whose coordinates arrived as `double` recovers no precision the input never carried and moves no prune verdict, because the hyperrect bounds derive from those same coordinates. A `Point3d`-carried cloud is therefore outside this binding by ABI, and degeneracy on such a cloud escalates to the exact-predicate ladder, never to a wider tree.
- within-lib: the kernel BVH/octree (`Spatial/index`) and the NURBS `ClosestParameter` (`Parametric/nurbs`) are disjoint acceleration owners by query shape — this tree owns discrete point k-NN and radius, the BVH/octree primitive overlap and ray, the engine continuous single-carrier parametric projection.

[LOCAL_ADMISSION]:
- admitted for `Solving/fit` (MLESAC primitive-fit, normal estimation via local k-NN PCA) and the `registration/ICP` per-iteration nearest-source query over a static cloud.
- `Rasm.Spatial` points map to `IReadOnlyList<TDimension>` at the boundary with the `Rasm` index or payload carried as `TNode`, recovered from the `(point, payload)` tuple; the tree never holds a kernel type.
- `KDTree.Create(points, nodes, DistanceMetrics.EuclideanDistance)` is the admitted build; the raw constructor is reserved for a custom metric or search window.
