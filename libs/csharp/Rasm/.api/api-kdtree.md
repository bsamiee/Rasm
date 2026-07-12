# [RASM_API_KDTREE]

`Supercluster.KDTree.Net` (the `micampbell` modernized fork — assembly `KDTree.dll`, namespace `SuperClusterKDTree`, NOT `Supercluster.KDTree.Net`) is the kernel's GENERIC, array-backed, exact-k-NN kd-tree: a static-balanced binary spatial tree over `INumber<TDimension>` points serving exact `NearestNeighbors` (k-nearest) and `RadialSearch` (radius / k-within-radius). It is the LOW-DIMENSIONAL POINT-NEAREST leaf that the kernel's primitive broad-phase (SAH-BVH + Morton octree, `Spatial/index`) serves poorly — point clouds, sample sets, the ICP correspondence step — and is ADDITIVE to that broad-phase, never a replacement for primitive overlap queries. The fork is `net8.0` and fully generic over `System.Numerics` generic-math: `TDimension: INumber<TDimension>, IMinMaxValue<TDimension>` (so a tree binds `double`/`float`/`Half`/`decimal`/`ddouble` coordinates), `TNode` is an arbitrary per-point payload, and a `DistanceMetrics` enum (Manhattan/Euclidean/Chebyshev/Cosine) or a custom `Func` metric drives the search. The kernel maps `Rasm.Spatial` points → `IReadOnlyList<TDimension>` AT THE BOUNDARY and carries the cloud's `Rasm` index/payload as `TNode`. Pure-managed AnyCPU, ZERO dependencies, osx-arm64-safe.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Supercluster.KDTree.Net`
- package: `Supercluster.KDTree.Net` (nuspec `<id>`); assembly + namespace use the SHORT name `KDTree` / `SuperClusterKDTree`
- license: MIT (`micampbell` fork of SuperClusterInc; `github.com/micampbell/Supercluster.KDTree`; nuspec `<license type="expression">MIT</license>`)
- assembly: `KDTree.dll` (ships `KDTree.xml`) — the package id and the assembly name DIFFER (`Supercluster.KDTree.Net` package → `KDTree.dll` / `namespace SuperClusterKDTree`); a `using Supercluster.KDTree;` is WRONG
- namespace: `SuperClusterKDTree` (`KDTree` static factory, `KDTree<TDimension,TPriority,TNode>` tree, `HyperRect<T>`, `DistanceMetrics`); `SuperClusterKDTree.Utilities` (`BoundedPriorityList`, `nth_element` selection — internal-grade helpers)
- target: single `lib/net8.0` only — NO multi-target fallback ambiguity; the `net10.0` consumer binds the one `net8.0` asset forward (the `INumber<T>` generic-math surface requires net7+, so net8.0 is the real floor — there is no `netstandard` fallback)
- asset: pure-managed runtime library, AnyCPU, NO native runtime and ZERO package dependencies (`<group targetFramework="net8.0" />` empty); the tree stores flat `IList<IReadOnlyList<TDimension>>` point + `IList<TNode>` node arrays (the "array-backed" balanced-tree layout)
- abi: full `System.Numerics` generic-math — `TDimension: INumber<TDimension>, IMinMaxValue<TDimension>` and `TPriority: INumber<TPriority>, IMinMaxValue<TPriority>`; a tree over `double`/`float`/`Half`/`decimal`/`ddouble` (`api-doubledouble`) coordinates binds with NO adapter
- rail: exact low-dimensional point k-NN / radius search

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the factory + the tree + the metric vocabulary (`SuperClusterKDTree`)
- rail: point k-NN

The tree is generic in THREE parameters — `TDimension` (coordinate scalar), `TPriority` (distance/priority scalar, usually = `TDimension`), `TNode` (per-point payload). `KDTree<TDimension,TPriority,TNode>` carries the `NearestNeighbors` and `RadialSearch` queries plus the flat point and node arrays under `TDimension: IComparable<TDimension>, IMinMaxValue<TDimension>` and `TPriority: INumber<TPriority>, IMinMaxValue<TPriority>`. The `DistanceMetrics` enum selects `ManhattanDistance` (L1), `EuclideanDistance` (L2-SQUARED — no sqrt), `ChebyshevDistance` (L∞), or `CosineDistance`; the static `KDTree.Create` factory wires the selected metric so a consumer never hand-writes the `Func`. `HyperRect<T>` owns the internal node split region through `static Infinite(int dimensions, T pos∞, T neg∞)`, `GetClosestPoint(IReadOnlyList<T>)`, and `Clone()`.

| [INDEX] | [SYMBOL]                             | [ROLE]             | [CAPABILITY]         |
| :-----: | :----------------------------------- | :----------------- | :------------------- |
|  [01]   | `KDTree`                             | static factory     | metric-bound build   |
|  [02]   | `KDTree<TDimension,TPriority,TNode>` | balanced kd-tree   | exact point search   |
|  [03]   | `DistanceMetrics`                    | enum               | metric selection     |
|  [04]   | `HyperRect<T>`                       | bounding rectangle | split-region pruning |

## [03]-[CONSTRUCTION]

[CONSTRUCTION_SCOPE]: building the tree (the factory vs the raw constructor)
- rail: point k-NN

The static `KDTree.Create<TDimension,TNode>(IList<IReadOnlyList<TDimension>> points, IList<TNode> nodes, DistanceMetrics distanceMetric)` returns `KDTree<TDimension,TDimension,TNode>`, binds the built-in metric, and infers `TPriority = TDimension`. The raw `KDTree<TDimension,TPriority,TNode>(int dimensions, ICollection<IReadOnlyList<TDimension>> points, IEnumerable<TNode> nodes, Func<…,…,TPriority> metric, TDimension searchWindowMinValue=default, TDimension searchWindowMaxValue=default)` constructor owns custom metrics and search-window clamps. The `KDTree<…>(int dimensions, IEnumerable<IReadOnlyList<TDimension>> points, int pointsCount, IEnumerable<TNode> nodes, Func<…> metric, …)` overload accepts a lazy point source without a materialized count. CONSTRUCTION IS THE INDEXING COST: the tree balances once at build (median split) and is then immutable for queries; rebuild on point-set change rather than mutating.

| [INDEX] | [SURFACE]                   | [KIND]      | [CAPABILITY]             |
| :-----: | :-------------------------- | :---------- | :----------------------- |
|  [01]   | `KDTree.Create`             | factory     | built-in metric          |
|  [02]   | `KDTree(…, ICollection, …)` | constructor | custom metric            |
|  [03]   | `KDTree(…, IEnumerable, …)` | constructor | lazy point source        |
|  [04]   | `Count`                     | property    | indexed point count      |
|  [05]   | `Dimensions`                | property    | fixed dimensionality     |
|  [06]   | `InternalPointArray`        | property    | balanced point storage   |
|  [07]   | `InternalNodeArray`         | property    | parallel payload storage |
|  [08]   | `Metric`                    | property    | swappable metric         |

## [04]-[QUERY]

[QUERY_SCOPE]: exact k-nearest and radius search
- rail: point k-NN

`NearestNeighbors(IReadOnlyList<TDimension> point, int numNeighbors)` and `RadialSearch(IReadOnlyList<TDimension> center, TPriority radius, int numNeighbors=-1)` return `IEnumerable<(IReadOnlyList<TDimension> point, TNode payload)>`, so each hit carries the consumer's point and payload directly. `NearestNeighbors` is exact k-NN; `RadialSearch` returns every hit within `radius` when `numNeighbors=-1` and otherwise applies the k-cap. Under `EuclideanDistance`, priority and `radius` use SQUARED distance because the metric skips the sqrt — pass `r²`, not `r`. Each static metric accepts `<TDimension>(IReadOnlyList<TDimension> x, IReadOnlyList<TDimension> y)`.

| [INDEX] | [SURFACE]                  | [KIND]   | [CAPABILITY]         |
| :-----: | :------------------------- | :------- | :------------------- |
|  [01]   | `NearestNeighbors`         | instance | exact k-nearest hits |
|  [02]   | `RadialSearch`             | instance | radius-bounded hits  |
|  [03]   | `KDTree.EuclideanDistance` | static   | squared L2 distance  |
|  [04]   | `KDTree.ManhattanDistance` | static   | L1 distance          |
|  [05]   | `KDTree.ChebyshevDistance` | static   | L∞ distance          |
|  [06]   | `KDTree.CosineDistance`    | static   | cosine distance      |

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: a static-balanced binary tree stored as FLAT arrays (`InternalPointArray` points + `InternalNodeArray` parallel payloads) — the "array-backed" layout, with implicit children at `2i+1`/`2i+2`. A point is an `IReadOnlyList<TDimension>` (any dimensionality fixed at build via `Dimensions`), NOT a fixed `(x,y,z)` struct — the tree is GENERIC N-dimensional, not 3D-specialized.
- generic math: `TDimension: INumber<TDimension>, IMinMaxValue<TDimension>` and `TPriority: INumber<TPriority>, IMinMaxValue<TPriority>` — the tree binds any `System.Numerics` scalar (`double`/`float`/`Half`/`decimal`/`ddouble`) with no adapter; `IMinMaxValue` supplies the ±∞ split-region sentinels via `HyperRect<T>.Infinite`.
- immutability: the tree balances ONCE at construction (median split) and is immutable for queries; a point-set change means a rebuild, not an insert/delete — there is no incremental update API.
- metric semantics: `EuclideanDistance` returns SQUARED L2 (skips the sqrt for speed), so a `RadialSearch` radius and any priority comparison under that metric is in squared units; `ManhattanDistance`=L1, `ChebyshevDistance`=L∞, `CosineDistance`=cosine. A custom `Func<IReadOnlyList<T>,IReadOnlyList<T>,TPriority>` metric is settable on `Metric`.

[LOCAL_ADMISSION]:
- this kd-tree is the DISCRETE POINT-NEAREST leaf feeding `Solving/fit` (MLESAC primitive-fit, normal estimation via local k-NN PCA) and the `registration/ICP` correspondence step — the per-iteration "nearest source point to each target point" query over a static cloud.
- it is ADDITIVE to, NOT a replacement for, the kernel's primitive broad-phase: the SAH-BVH + Morton octree (`Spatial/index`) owns primitive (triangle/curve/AABB) overlap and ray queries; this kd-tree owns POINT-cloud k-NN/radius. A query is routed by SHAPE — point set → kd-tree, primitive set → BVH/octree — never duplicated.
- the kernel maps `Rasm.Spatial` points → `IReadOnlyList<TDimension>` AT THE BOUNDARY and carries the cloud's `Rasm` index or payload as `TNode`, recovering it from the `(point, payload)` query tuple; the kd-tree never holds a kernel type directly.
- prefer `KDTree.Create(points, nodes, DistanceMetrics.EuclideanDistance)` over the raw constructor unless a custom metric or search window is genuinely needed; do NOT hand-write the Euclidean `Func` the enum already wires.

[STACKING_LAW]:
- vs the kernel BVH/octree (`Spatial/index`, kernel-authored): the kd-tree and the BVH/octree are DISJOINT acceleration owners partitioned by query shape — point k-NN/radius vs primitive overlap/ray. Never re-implement a point-cloud k-NN on the BVH (a BVH is the wrong structure for low-dim point-nearest) nor a primitive broad-phase on the kd-tree.
- vs the vendored NURBS engine (`Parametric/nurbs`): the engine's `ClosestParameter` is the PARAMETRIC (continuous, Newton-on-one-carrier) nearest point; this kd-tree is the DISCRETE (sampled point cloud) nearest point. A dense closest-point query over many sampled curve/surface points routes through the kd-tree (the `Parametric/surface` batch pullback seeds exactly this way); the single-carrier continuous projection stays on the engine.
- vs MIConvexHull (`api-miconvexhull`): MIConvexHull's `Triangulation.CreateDelaunay` produces a Delaunay cell complex (connectivity); the kd-tree produces nearest-neighbour QUERIES (no connectivity). For "k nearest of a fixed cloud" use the kd-tree; for "triangulate the cloud" use MIConvexHull — they answer different questions over the same points.
- vs DoubleDouble (`api-doubledouble`): because `TDimension: INumber<TDimension>`, a kd-tree over `ddouble` coordinates binds directly — a precision-critical point cloud (degenerate near-coincident points) can index at 106-bit with no specialized path, the `ddouble` distance computed through the same generic metric.
- vs the upstream `Supercluster.KDTree` lineage: this fork's type is `SuperClusterKDTree.KDTree<TDimension,TPriority,TNode>` (arity-3, namespace `SuperClusterKDTree`, assembly `KDTree.dll`) — the ORIGINAL `Supercluster.KDTree` namespace names the upstream arity-1 tree some ecosystem packages bundle as source, a different fully-qualified type. Reach this fork's arity-3 tree only as `using SuperClusterKDTree;`; `using Supercluster.KDTree;` names the wrong namespace for this package.

[RAIL_LAW]:
- Package: `Supercluster.KDTree.Net` (assembly `KDTree.dll`, namespace `SuperClusterKDTree`)
- Owns: the generic N-dimensional exact-k-NN kd-tree — `KDTree.Create<TDimension,TNode>(points, nodes, DistanceMetrics)` factory and the raw `KDTree<TDimension,TPriority,TNode>` constructors, the `NearestNeighbors(point, k)` exact k-nearest and `RadialSearch(center, radius, k=-1)` radius queries returning `(coordinate, payload)` tuples, the `DistanceMetrics` enum (Manhattan/Euclidean²/Chebyshev/Cosine) + the static distance functions, and the `HyperRect<T>` split-region primitive — all over `INumber<TDimension>` coordinates with an arbitrary `TNode` payload.
- Accept: exact k-NN / radius search over a STATIC low-dimensional point cloud (sample sets, normal-estimation neighbourhoods, ICP correspondence); a `Rasm.Spatial` cloud mapped to `IReadOnlyList<TDimension>` with the `Rasm` index/payload carried as `TNode`; a `ddouble`/`float`/`Half` coordinate type bound through the generic-math constraint; the built-in metric wired via `Create`.
- Reject: using the package name `Supercluster.KDTree.Net` as the namespace or `using Supercluster.KDTree;` (the namespace is `SuperClusterKDTree`, the assembly `KDTree.dll` — the `Supercluster.KDTree` namespace belongs to the upstream arity-1 lineage, a different FQN); treating it as a primitive (triangle/curve/AABB) broad-phase or ray-query structure (that is the kernel BVH/octree — the kd-tree is POINT-only); expecting incremental insert/delete (the tree is build-once immutable — rebuild on change); passing an un-squared radius under `EuclideanDistance` (that metric is squared-L2); hand-writing a Euclidean `Func` the `DistanceMetrics` enum already supplies; re-implementing curve/surface continuous closest-point on the kd-tree (that is the vendored `Parametric/nurbs` engine's `ClosestParameter`).
