# [RASM_API_KDTREE]

`Supercluster.KDTree.Net` (the `micampbell` modernized fork — assembly `KDTree.dll`, namespace
`SuperClusterKDTree`, NOT `Supercluster.KDTree.Net`) is the kernel's GENERIC, array-backed,
exact-k-NN kd-tree: a static-balanced binary spatial tree over `INumber<TDimension>` points
serving exact `NearestNeighbors` (k-nearest) and `RadialSearch` (radius / k-within-radius). It
is the LOW-DIMENSIONAL POINT-NEAREST leaf that the kernel's primitive broad-phase
(SAH-BVH + Morton octree, `Spatial/index`) serves poorly — point clouds, sample sets, the ICP
correspondence step — and is ADDITIVE to that broad-phase, never a replacement for primitive
overlap queries. The fork is `net8.0` and fully generic over `System.Numerics` generic-math:
`TDimension : INumber<TDimension>, IMinMaxValue<TDimension>` (so a tree binds `double`/`float`/
`Half`/`decimal`/`ddouble` coordinates), `TNode` is an arbitrary per-point payload, and a
`DistanceMetrics` enum (Manhattan/Euclidean/Chebyshev/Cosine) or a custom `Func` metric drives
the search. The kernel maps `Rasm.Vectors` points → `IReadOnlyList<TDimension>` AT THE BOUNDARY
and carries the cloud's `Rasm` index/payload as `TNode`. Pure-managed AnyCPU, ZERO dependencies,
osx-arm64-safe.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Supercluster.KDTree.Net`
- package: `Supercluster.KDTree.Net` (nuspec `<id>`); assembly + namespace use the SHORT name `KDTree` / `SuperClusterKDTree`
- version: `1.0.22` (`+45735b1c…` informational)
- license: MIT (`micampbell` fork of SuperClusterInc; `github.com/micampbell/Supercluster.KDTree`; nuspec `<license type="expression">MIT</license>`)
- assembly: `KDTree.dll` (ships `KDTree.xml`) — the package id and the assembly name DIFFER (`Supercluster.KDTree.Net` package → `KDTree.dll` / `namespace SuperClusterKDTree`); a `using Supercluster.KDTree;` is WRONG
- namespace: `SuperClusterKDTree` (`KDTree` static factory, `KDTree<TDimension,TPriority,TNode>` tree, `HyperRect<T>`, `DistanceMetrics`); `SuperClusterKDTree.Utilities` (`BoundedPriorityList`, `nth_element` selection — internal-grade helpers)
- target: single `lib/net8.0` only — NO multi-target fallback ambiguity; the `net10.0` consumer binds the one `net8.0` asset forward (the `INumber<T>` generic-math surface requires net7+, so net8.0 is the real floor — there is no `netstandard` fallback)
- asset: pure-managed runtime library, AnyCPU, NO native runtime and ZERO package dependencies (`<group targetFramework="net8.0" />` empty); the tree stores flat `IList<IReadOnlyList<TDimension>>` point + `IList<TNode>` node arrays (the "array-backed" balanced-tree layout)
- abi: full `System.Numerics` generic-math — `TDimension : INumber<TDimension>, IMinMaxValue<TDimension>` and `TPriority : INumber<TPriority>, IMinMaxValue<TPriority>`; a tree over `double`/`float`/`Half`/`decimal`/`ddouble` (`api-doubledouble`) coordinates binds with NO adapter
- rail: exact low-dimensional point k-NN / radius search

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the factory + the tree + the metric vocabulary (`SuperClusterKDTree`)
- rail: point k-NN

The tree is generic in THREE parameters — `TDimension` (coordinate scalar), `TPriority`
(distance/priority scalar, usually = `TDimension`), `TNode` (per-point payload). The
`DistanceMetrics` enum + the static distance functions are the metric vocabulary; the static
`KDTree.Create` factory is the ergonomic entrypoint that wires the metric from the enum so a
consumer never hand-writes the `Func`.

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]                       | [CAPABILITY]                                                                                          |
| :-----: | :---------------------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `KDTree` (static)                         | the ergonomic factory + metrics      | `static Create<TDimension,TNode>(points, nodes, DistanceMetrics)`; the four static distance functions |
|  [02]   | `KDTree<TDimension,TPriority,TNode>`      | the balanced kd-tree                 | `where TDimension : IComparable<TDimension>, IMinMaxValue<TDimension>` and `TPriority : INumber<TPriority>, IMinMaxValue<TPriority>`; carries `NearestNeighbors`/`RadialSearch` + the flat point/node arrays |
|  [03]   | `DistanceMetrics` (enum)                  | the built-in metric selector         | `ManhattanDistance` (L1) / `EuclideanDistance` (L2-SQUARED — no sqrt) / `ChebyshevDistance` (L∞) / `CosineDistance` |
|  [04]   | `HyperRect<T>`                            | N-dim bounding rectangle             | `static Infinite(int dimensions, T pos∞, T neg∞)`; `GetClosestPoint(IReadOnlyList<T>)`; `Clone()` — the internal node split-region primitive |

## [03]-[CONSTRUCTION]

[CONSTRUCTION_SCOPE]: building the tree (the factory vs the raw constructor)
- rail: point k-NN

The static `KDTree.Create` is the PREFERRED entrypoint — it binds the metric from the
`DistanceMetrics` enum and infers `TPriority = TDimension`. The raw constructor is for a custom
metric `Func` or a non-default search window. CONSTRUCTION IS THE INDEXING COST: the tree
balances once at build (median split) and is then immutable for queries; rebuild on point-set
change rather than mutating.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `KDTree.Create<TDimension,TNode>(IList<IReadOnlyList<TDimension>> points, IList<TNode> nodes, DistanceMetrics distanceMetric)` (→ `KDTree<TDimension,TDimension,TNode>`) | static factory | the ergonomic build — points + parallel payloads + a built-in metric; `TPriority` = `TDimension` |
|  [02]   | `new KDTree<TDimension,TPriority,TNode>(int dimensions, ICollection<IReadOnlyList<TDimension>> points, IEnumerable<TNode> nodes, Func<…,…,TPriority> metric, TDimension searchWindowMinValue=default, TDimension searchWindowMaxValue=default)` | constructor | full control — explicit dimensionality + a CUSTOM metric `Func` + a search-window clamp |
|  [03]   | `new KDTree<…>(int dimensions, IEnumerable<IReadOnlyList<TDimension>> points, int pointsCount, IEnumerable<TNode> nodes, Func<…> metric, …)` | constructor | the streaming overload — `pointsCount` lets a lazy `IEnumerable` build without a materialized count |
|  [04]   | `Count` / `Dimensions` (props)                                            | instance       | the indexed point count and the fixed dimensionality                 |
|  [05]   | `InternalPointArray` (`IList<IReadOnlyList<TDimension>>`) / `InternalNodeArray` (`IList<TNode>`) / `Metric` (settable `Func`) | instance | the flat balanced-tree storage views + a swappable metric delegate   |

## [04]-[QUERY]

[QUERY_SCOPE]: exact k-nearest and radius search
- rail: point k-NN

Both queries return `IEnumerable<(IReadOnlyList<TDimension> point, TNode payload)>` — the hit
POINT paired with its `TNode` payload, so the consumer recovers its own index/data directly.
`NearestNeighbors` is exact k-NN; `RadialSearch` is radius-bounded with an optional k-cap
(`numNeighbors=-1` = all within radius). NOTE: when the metric is `EuclideanDistance`, the
priority/`radius` is in SQUARED distance (the metric skips the sqrt) — pass `r²`, not `r`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `NearestNeighbors(IReadOnlyList<TDimension> point, int numNeighbors)` (→ `IEnumerable<(IReadOnlyList<TDimension>, TNode)>`) | instance | exact k-nearest neighbours to `point`, each as `(coordinate, payload)` |
|  [02]   | `RadialSearch(IReadOnlyList<TDimension> center, TPriority radius, int numNeighbors=-1)` (→ `IEnumerable<(IReadOnlyList<TDimension>, TNode)>`) | instance | all points within `radius` of `center` (or the `numNeighbors` nearest within it); `radius` is SQUARED for the Euclidean metric |
|  [03]   | `KDTree.EuclideanDistance` / `ManhattanDistance` / `ChebyshevDistance` / `CosineDistance` `<TDimension>(IReadOnlyList<TDimension> x, IReadOnlyList<TDimension> y)` | static | the four metric functions — Euclidean returns the SQUARED L2 distance (no sqrt) for speed |

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: a static-balanced binary tree stored as FLAT arrays (`InternalPointArray` points + `InternalNodeArray` parallel payloads) — the "array-backed" layout, with implicit children at `2i+1`/`2i+2`. A point is an `IReadOnlyList<TDimension>` (any dimensionality fixed at build via `Dimensions`), NOT a fixed `(x,y,z)` struct — the tree is GENERIC N-dimensional, not 3D-specialized.
- generic math: `TDimension : INumber<TDimension>, IMinMaxValue<TDimension>` and `TPriority : INumber<TPriority>, IMinMaxValue<TPriority>` — the tree binds any `System.Numerics` scalar (`double`/`float`/`Half`/`decimal`/`ddouble`) with no adapter; `IMinMaxValue` supplies the ±∞ split-region sentinels via `HyperRect<T>.Infinite`.
- immutability: the tree balances ONCE at construction (median split) and is immutable for queries; a point-set change means a rebuild, not an insert/delete — there is no incremental update API.
- metric semantics: `EuclideanDistance` returns SQUARED L2 (skips the sqrt for speed), so a `RadialSearch` radius and any priority comparison under that metric is in squared units; `ManhattanDistance`=L1, `ChebyshevDistance`=L∞, `CosineDistance`=cosine. A custom `Func<IReadOnlyList<T>,IReadOnlyList<T>,TPriority>` metric is settable on `Metric`.

[LOCAL_ADMISSION]:
- this kd-tree is the DISCRETE POINT-NEAREST leaf feeding `Solving/fit` (MLESAC primitive-fit, normal estimation via local k-NN PCA) and the `registration/ICP` correspondence step — the per-iteration "nearest source point to each target point" query over a static cloud.
- it is ADDITIVE to, NOT a replacement for, the kernel's primitive broad-phase: the SAH-BVH + Morton octree (`Spatial/index`) owns primitive (triangle/curve/AABB) overlap and ray queries; this kd-tree owns POINT-cloud k-NN/radius. A query is routed by SHAPE — point set → kd-tree, primitive set → BVH/octree — never duplicated.
- the kernel maps `Rasm.Vectors` points → `IReadOnlyList<TDimension>` AT THE BOUNDARY and carries the cloud's `Rasm` index or payload as `TNode`, recovering it from the `(point, payload)` query tuple; the kd-tree never holds a `Rasm.Vectors` type directly.
- prefer `KDTree.Create(points, nodes, DistanceMetrics.EuclideanDistance)` over the raw constructor unless a custom metric or search window is genuinely needed; do NOT hand-write the Euclidean `Func` the enum already wires.

[STACKING_LAW]:
- vs the kernel BVH/octree (`Spatial/index`, kernel-authored): the kd-tree and the BVH/octree are DISJOINT acceleration owners partitioned by query shape — point k-NN/radius vs primitive overlap/ray. Never re-implement a point-cloud k-NN on the BVH (a BVH is the wrong structure for low-dim point-nearest) nor a primitive broad-phase on the kd-tree.
- vs GShark (`api-gshark`): GShark `NurbsBase.ClosestPoint`/`ClosestParameter` is the PARAMETRIC (continuous, Newton-on-one-curve) nearest point; this kd-tree is the DISCRETE (sampled point cloud) nearest point. A dense closest-point query over many sampled curve/surface points routes through the kd-tree; the single-curve continuous projection stays in GShark.
- vs MIConvexHull (`api-miconvexhull`): MIConvexHull's `Triangulation.CreateDelaunay` produces a Delaunay cell complex (connectivity); the kd-tree produces nearest-neighbour QUERIES (no connectivity). For "k nearest of a fixed cloud" use the kd-tree; for "triangulate the cloud" use MIConvexHull — they answer different questions over the same points.
- vs DoubleDouble (`api-doubledouble`): because `TDimension : INumber<TDimension>`, a kd-tree over `ddouble` coordinates binds directly — a precision-critical point cloud (degenerate near-coincident points) can index at 106-bit with no specialized path, the `ddouble` distance computed through the same generic metric.
- vs the upstream `Supercluster.KDTree` lineage: this fork's type is `SuperClusterKDTree.KDTree<TDimension,TPriority,TNode>` (arity-3, namespace `SuperClusterKDTree`, assembly `KDTree.dll`) — the ORIGINAL `Supercluster.KDTree` namespace names the upstream arity-1 tree some ecosystem packages bundle as source, a different fully-qualified type. Reach this fork's arity-3 tree only as `using SuperClusterKDTree;`; `using Supercluster.KDTree;` names the wrong namespace for this package.

[RAIL_LAW]:
- Package: `Supercluster.KDTree.Net` (assembly `KDTree.dll`, namespace `SuperClusterKDTree`)
- Owns: the generic N-dimensional exact-k-NN kd-tree — `KDTree.Create<TDimension,TNode>(points, nodes, DistanceMetrics)` factory and the raw `KDTree<TDimension,TPriority,TNode>` constructors, the `NearestNeighbors(point, k)` exact k-nearest and `RadialSearch(center, radius, k=-1)` radius queries returning `(coordinate, payload)` tuples, the `DistanceMetrics` enum (Manhattan/Euclidean²/Chebyshev/Cosine) + the static distance functions, and the `HyperRect<T>` split-region primitive — all over `INumber<TDimension>` coordinates with an arbitrary `TNode` payload.
- Accept: exact k-NN / radius search over a STATIC low-dimensional point cloud (sample sets, normal-estimation neighbourhoods, ICP correspondence); a `Rasm.Vectors` cloud mapped to `IReadOnlyList<TDimension>` with the `Rasm` index/payload carried as `TNode`; a `ddouble`/`float`/`Half` coordinate type bound through the generic-math constraint; the built-in metric wired via `Create`.
- Reject: using the package name `Supercluster.KDTree.Net` as the namespace or `using Supercluster.KDTree;` (the namespace is `SuperClusterKDTree`, the assembly `KDTree.dll` — the `Supercluster.KDTree` namespace belongs to the upstream arity-1 lineage, a different FQN); treating it as a primitive (triangle/curve/AABB) broad-phase or ray-query structure (that is the kernel BVH/octree — the kd-tree is POINT-only); expecting incremental insert/delete (the tree is build-once immutable — rebuild on change); passing an un-squared radius under `EuclideanDistance` (that metric is squared-L2); hand-writing a Euclidean `Func` the `DistanceMetrics` enum already supplies; re-implementing curve/surface continuous closest-point on the kd-tree (that is GShark's parametric `ClosestPoint`).
