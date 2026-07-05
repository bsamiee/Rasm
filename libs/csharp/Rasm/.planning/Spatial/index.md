# [RASM_SPATIAL_INDEX]

ONE polymorphic broad-phase owner behind ONE entry: `Fin<SpatialAnswer> Spatial.Apply(SpatialOp, Op? key = null)` folds a closed `SpatialOp` `[Union]` — `Build(SpatialKind, BoundingBox[], BuildPolicy)` · `Refit` · `Query(SpatialQuery)` · `Wire` — over a `SpatialIndex` `[Union]` of two first-principles author-kernels, a SAH-partitioned bounding-volume hierarchy (`Bvh`) and a Morton-order linear octree (`LinearOctree`), built over ONE frozen `NodeStore` struct-of-arrays layout so every query, refit, and wire projection reads the same node memory regardless of which kernel produced it. The kernel selection is a `[UseDelegateFromConstructor]` behavior column on the `SpatialKind` `[SmartEnum<string>]` (`bvh` · `octree` · `agglomerative` — the agglomerative bottom-up builder writes the `Bvh` case), the query algebra is the `SpatialQuery` `[Union]` (`Range`/`Ray`/`Nearest`/`Overlap`/`Winding`) with its `QueryKind` discriminant and typed `QueryResult` carrier, and the entry is total over the `Fin` rail: a malformed op parameter routes `key.InvalidInput()`, a degenerate primitive set routes `GeometryFault.DegenerateInput` 2400, a refit count mismatch routes `IndexMismatch` 2401, and a query the index cannot serve routes the typed `KindMismatch(SpatialKind, QueryKind)` 2402 — zero throws, zero silent-empty verdicts.

The index composes RhinoCommon `BoundingBox`/`Point3d`/`Vector3d`/`Ray3d`/`Sphere` through the `Rasm.Numerics` substrate as settled vocabulary — read, compose, never re-mint — and operates on raw primitive coordinates because the index is a geometric-coordinate structure, not a unit-bearing quantity surface. Node bounds narrow `double → float` OUTWARD at the one store-write seam (`float.BitDecrement` on every min, `float.BitIncrement` on every max, post-cast) so a conservative node bound can never falsely prune a true hit; exact confirmation always re-tests the original `double` primitive boxes at the leaves. The only cross-package egress is the `Wire` case returning the frozen node-array wire `(float[] Bounds, long[] Nodes)` over `NodeLinkProjection` — `Rasm.Compute` decodes it, and no Compute type appears in this fence (a `using Rasm.Compute.*` here is the deleted upward strata edge). The frozen `NodeStore` is the hash-friendly record the Persistence blob lane content-addresses by reference; the geometry domain computes no hash and mints no second store. Point k-NN and radius search over a bare point set route the landed `neighbors.md` `StaticCase` kd-tree tier — the excised `PointCloud` arm is the deleted duplicate — and this owner serves PRIMITIVE bounds exclusively.

## [01]-[INDEX]

- [01]-[SPATIAL_INDEX]: ONE `Spatial.Apply(SpatialOp, Op?)` entry; `SpatialIndex` Bvh/LinearOctree union over one frozen `NodeStore`; three `SpatialKind` builder rows (SAH-BVH/Morton-octree/agglomerative PLOC); `SpatialQuery` Range/Ray/Nearest/Overlap/Winding algebra; degradation-keyed refit; the `Wire` node-array seam with the harness-gated `CLASH_GOLDEN` fixture.

## [02]-[SPATIAL_INDEX]

- Owner: `SpatialKind` `[SmartEnum<string>]` the builder discriminant (`bvh`/`octree`/`agglomerative`) binding the shipped `ComparerAccessors.StringOrdinal` as its string-key comparer and carrying the `[UseDelegateFromConstructor]` `Build` behavior column — the kernel selection is a row delegate on the vocabulary, so the `Builders` dictionary and the `kind switch` cascade are both deleted forms; `QueryKind` `[SmartEnum<string>]` the query-modality discriminant (`range`/`ray`/`nearest`/`overlap`/`winding`) the `KindMismatch` 2402 payload names; `BuildPolicy` the one policy row (leaf size, depth, SAH buckets, refit degradation limit, parallel floor); `NodeStore` the frozen struct-of-arrays node memory (per-node outward-rounded `float` AABB min/max, contiguous child range, leaf primitive range, the `Order` permutation) every kernel freezes and every traversal reads; `SpatialIndex` `[Union]` `Bvh`/`LinearOctree` carrying that one store plus the primitive `BoundingBox[]` payload, registered into the validity oracle as `IValidityEvidence` with one `ValidityClaim.All` fold; `SpatialQuery` `[Union]` the query algebra with the typed `QueryResult` carrier; `SpatialOp`/`SpatialAnswer` the request/answer pair; `Spatial` the static entry surface owning the ONE `Apply` fold.
- Cases: `SpatialKind` rows `bvh` · `octree` · `agglomerative` (3 — the agglomerative bottom-up builder is a third partition strategy over the SAME `Bvh` union case and `NodeStore` layout, carried by the `Bvh.Builder` discriminant column, never a parallel index class); `SpatialIndex` cases `Bvh` · `LinearOctree` (2 — agglomerative writes the `Bvh` case); `SpatialQuery` cases `Range` · `Ray` · `Nearest` · `Overlap` · `Winding` (5 — the generalized-winding-number query is a `SpatialQuery` case over the SAME store, never a new structure); `SpatialOp` cases `Build` · `Refit` · `Query` · `Wire` (4); `SpatialAnswer` cases `Index` · `Result` · `Wire` (3).
- Entry: `public static Fin<SpatialAnswer> Spatial.Apply(SpatialOp op, Op? key = null)` — the ONE entry over every modality, discriminating on the op case value; `Build` admits the primitive set once (`GeometryFault.DegenerateInput(Kind.BoundingBox, index, witness)` 2400 on an empty set or the first non-finite bound — the `Rasm.Domain` `Kind` vocabulary, the payload type the `faults.md` signature names), dispatches the `SpatialKind.Build` row, and gates the frozen result through its `IValidityEvidence` fold; `Refit` re-bounds the existing node topology persistently (`GeometryFault.IndexMismatch(EntityKind.Face, expected, actual)` 2401 on a primitive-count mismatch — the index primitives are face-level bounds in every declared consumer — and the revised set re-enters the SAME admission gate, so a non-finite revision faults `DegenerateInput` with its index) and re-gates validity; `Query` folds the `SpatialQuery` case over the shared store, routing `key.InvalidInput()` for a malformed query scalar and `GeometryFault.KindMismatch(kind, query)` 2402 when the query payload cannot bind the index content (a `Winding` whose triangle array does not cover the primitive set); `Wire` emits the frozen `(float[] Bounds, long[] Nodes)` node-array wire. The multi-static `Build`/`Query`/`Refit`/`ToAcceleration` surface is the collapsed form — those bodies survive as `internal` members the one entry composes.
- Auto: the `Build` arm reads the `SpatialKind.Build` `[UseDelegateFromConstructor]` row — the SAH-BVH row recursively partitions on the minimum surface-area-heuristic cost split over the centroid axis into a binary node tree, the linear-octree row sorts primitive centroids by 30-bit Morton code (10 bits per axis via `Expand10` bit-spreading) then builds the cell hierarchy from the sorted radix runs as an N-way node whose 1–8 children occupy a contiguous `[FirstChild, FirstChild+ChildCount)` run, and the agglomerative row is Morton-presorted locally-ordered clustering (mutually-nearest merge by minimum merged surface area inside a `SahBuckets`-wide window) whose merge forest a breadth-first compaction re-indexes into the SAME dense parent-before-child store — every kernel stages node rows in a pooled `MemoryOwner<T>` arena and FREEZES the used prefix into exact-size arrays, so the durable `NodeStore` carries no over-allocated tail and the pooled arena dies with the build. Every store write narrows bounds OUTWARD (`float.BitDecrement(min)` / `float.BitIncrement(max)` post-cast) so node descent pruning is conservative by construction; leaf verdicts re-test the original `double` boxes. The `Query` fold lowers every case to one traversal over the shared store walking the whole `[FirstChild, FirstChild+ChildCount)` child range: `Range` is a box/sphere descent collecting leaf primitives whose exact bound intersects the region; `Ray` is a slab-test descent returning the nearest primitive hit `t`; `Nearest` is a best-first k-NN descent over a `PriorityQueue` worst-distance bound, pruned by the conservative node-bound lower distance with exact `double` primitive-box distance at the leaves; `Overlap` walks two stores in tandem collecting leaf-pair candidates within tolerance (the broad-phase clash candidate set; the self-overlap modality — one index on both sides — dedups to `pa < pb`, never a reflexive or mirrored pair); `Winding` is the Barill/Jacobson hierarchical generalized-winding-number descent over per-node dipole far-field moments cached by ONE bottom-up pass per evaluation and read by EVERY query point in the batch. `Refit` is PERSISTENT: a parallel leaf pass (`ParallelHelper.For` over a struct `IAction`, each leaf writing its disjoint node slot in fresh bound arrays) then one reverse-order internal propagation over the parent-before-child index law, freezing a new `NodeStore` that SHARES the immutable topology arrays and replaces only the bound arrays — the input index is never mutated; the refitted SAH cost (`TensorPrimitives.Subtract` extents + one `TensorPrimitives.Dot` weight·area reduction) gates against the `Bvh.BuildCost` baseline, and past `BuildPolicy.RefitDegradationLimit × BuildCost` the refit rebuilds fully through the SAME `SpatialKind.Build` row — deterministic, so an incremental edit session is reproducible.
- Receipt: none on the query rail — a query verdict is the typed `QueryResult` carrier (hit ids, ray `t`, k-NN ordered ids, overlap pairs, per-query-point winding field); the build/refit rail returns the index itself, and the index IS the registered evidence — `SpatialIndex : IValidityEvidence` declares one `ValidityClaim.All` fold (node count floor, exact array coverage, `TensorPrimitives.IsFiniteAll` over both bound arrays, parent-before-child link ranges, `Order` covering the primitive count) the `Apply` build/refit arms gate before emission.
- Packages: Rhino.Inside / RhinoCommon (`BoundingBox`/`Point3d`/`Vector3d`/`Ray3d`/`Sphere` via `Rasm.Numerics`), `Rasm.Domain` (`Op` key rail + `ValidityClaim`/`IValidityEvidence` + the `Kind` taxonomy row in the `DegenerateInput` payload — landed, composed), `Rasm.Numerics` (`GeometryFault` band 2400, `Numerics/faults.md`), `Rasm.Spatial` (`EntityKind` — the `IndexMismatch` payload discriminant only), CommunityToolkit.HighPerformance (`MemoryOwner<T>`/`SpanOwner<T>` pooled build arena, `ParallelHelper` + `IAction` partition-disjoint leaf refit), System.Numerics.Tensors (`TensorPrimitives.Subtract`/`Dot`/`IsFiniteAll` — non-exact float span reductions only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`PriorityQueue<TElement,TPriority>`, `Stack<T>`, `List<T>`).
- Growth: a new acceleration kernel is one `SpatialKind` row carrying its `Build` delegate and writing the shared `NodeStore` (a wholly new layout would be one `SpatialIndex` union case, admitted only by a charter amendment, never widened silently from this leaf page); a new query shape is one `SpatialQuery` case + one `QueryKind` row + one `Query`-fold arm over the same store; a new op modality is one `SpatialOp` case + one `Apply` arm; a new build/refit knob is one column on `BuildPolicy`; zero new surface.
- Boundary: `Spatial.Apply` is the ONE public entry and the retired `Build`/`Query`/`Refit`/`ToAcceleration` multi-static surface is the collapsed form; the two kernels differ ONLY in their build partition strategy, never in their traversal, so `Query`/`Refit`/`Wire` read the shared store kernel-agnostically over a uniform contiguous child range — two lossy `Left`/`Right` links that drop an octree's middle octants are the deleted form; the `NodeStore` is FROZEN exact-size struct-of-arrays (`float[] BoundsMin`, `float[] BoundsMax`, `int[] FirstChild`, `int[] ChildCount`, `int[] LeafStart`, `int[] LeafCount`, `int[] Order`) and a build-arena array retained oversized into the durable record is the deleted waste — pooled staging freezes to exact prefixes; bounds narrowing rounds OUTWARD at the one write seam and a round-to-nearest `(float)` cast in a node bound is the named false-negative-prune defect this rebuild deletes (the wire emits the already-outward-rounded store floats directly, so no second rounding site exists); `Refit` is persistent — fresh bound arrays over shared topology arrays — and an in-place mutation of a published index is the deleted aliasing defect; the `Wire` case returns `(float[] Bounds, long[] Nodes)` raw arrays at the seam and `Rasm.Compute` decodes them (`ClashScale.NodeLinkPairs`) — returning a Compute-owned type from this kernel is the deleted upward strata edge, and a domain-local record duplicating the wire shape is the rejected double-owner form; point k-NN/radius over a bare point set routes the landed `neighbors.md` `StaticCase` kd-tree tier by standing decision (this page's excised `PointCloud`/`Supercluster.KDTree` arm is the deleted duplicate — neighbors owns the point tier, this owner the primitive tier, and neither re-implements the other); every failure routes the `Fin` rail — a thrown accessor, a silent-empty query verdict, and a swallowed kind mismatch are all deleted forms, with `GeometryFault.<Case>(...).ToError()` the band-2400 channel and `key.InvalidInput()` the admission channel (the two-family seam); the Morton kernel spreads each normalized 10-bit axis through the portable `Expand10` magic-number interleave so the build is RID-agnostic; the slab ray test, SAH cost scan, and k-NN heap operate on raw primitive doubles inside the kernel because coordinates are the domain's native scalar, and a unit-carrying type in a traversal signature is the seam violation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new("bvh", SpatialIndex.BuildBvh);
    public static readonly SpatialKind Octree = new("octree", SpatialIndex.BuildOctree);
    public static readonly SpatialKind Agglomerative = new("agglomerative", SpatialIndex.BuildAgglomerative);

    // The kernel-selection behavior column: the row IS the builder, so the FrozenDictionary
    // table and the kind-switch cascade are both deleted forms.
    [UseDelegateFromConstructor]
    public partial SpatialIndex Build(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueryKind {
    public static readonly QueryKind Range = new("range");
    public static readonly QueryKind Ray = new("ray");
    public static readonly QueryKind Nearest = new("nearest");
    public static readonly QueryKind Overlap = new("overlap");
    public static readonly QueryKind Winding = new("winding");
}

// --- [CONSTANTS] --------------------------------------------------------------------------
public sealed record BuildPolicy(int LeafSize, int MaxDepth, int SahBuckets, double RefitDegradationLimit, int ParallelFloor) {
    public static readonly BuildPolicy Canonical =
        new(LeafSize: 4, MaxDepth: 32, SahBuckets: 12, RefitDegradationLimit: 1.6, ParallelFloor: 4096);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Frozen exact-size SoA node memory: bounds are OUTWARD-rounded float32, topology arrays are
// immutable after freeze, and a persistent refit shares them while replacing only the bounds.
public sealed record NodeStore(
    int Count,
    float[] BoundsMin,
    float[] BoundsMax,
    int[] FirstChild,
    int[] ChildCount,
    int[] LeafStart,
    int[] LeafCount,
    int[] Order) {
    public BoundingBox Bound(int node) =>
        new(new Point3d(BoundsMin[3 * node], BoundsMin[3 * node + 1], BoundsMin[3 * node + 2]),
            new Point3d(BoundsMax[3 * node], BoundsMax[3 * node + 1], BoundsMax[3 * node + 2]));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueryResult {
    private QueryResult() { }

    public sealed record Hits(Seq<int> Ids) : QueryResult;
    public sealed record RayHit(Option<int> Id, double T) : QueryResult;
    public sealed record Nearest(Seq<int> Ordered) : QueryResult;
    public sealed record Pairs(Seq<(int Left, int Right)> Overlaps) : QueryResult;
    public sealed record Field(double[] Values) : QueryResult;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialQuery {
    private SpatialQuery() { }

    public sealed record Range(BoundingBox Box, Option<Sphere> Ball) : SpatialQuery;
    public sealed record Ray(Ray3d Probe, double MaxT) : SpatialQuery;
    public sealed record Nearest(Point3d Query, int K) : SpatialQuery;
    public sealed record Overlap(SpatialIndex Other, double Tolerance) : SpatialQuery;
    public sealed record Winding(Point3d[] Queries, Point3d[] Triangles, double BetaSquared) : SpatialQuery;

    public QueryKind Kind =>
        Switch(
            range: static _ => QueryKind.Range,
            ray: static _ => QueryKind.Ray,
            nearest: static _ => QueryKind.Nearest,
            overlap: static _ => QueryKind.Overlap,
            winding: static _ => QueryKind.Winding);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialOp {
    private SpatialOp() { }

    public sealed record Build(SpatialKind Kind, BoundingBox[] Primitives, BuildPolicy Policy) : SpatialOp;
    public sealed record Refit(SpatialIndex Index, BoundingBox[] Updated) : SpatialOp;
    public sealed record Query(SpatialIndex Index, SpatialQuery Probe) : SpatialOp;
    public sealed record Wire(SpatialIndex Index) : SpatialOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialAnswer {
    private SpatialAnswer() { }

    public sealed record Index(SpatialIndex Value) : SpatialAnswer;
    public sealed record Result(QueryResult Value) : SpatialAnswer;
    public sealed record Wire(float[] Bounds, long[] Nodes) : SpatialAnswer;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialIndex : IValidityEvidence {
    private SpatialIndex() { }

    public sealed record Bvh(NodeStore Store, BoundingBox[] Primitives, int LeafSize, double BuildCost, BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;
    public sealed record LinearOctree(NodeStore Store, BoundingBox[] Primitives, Point3d[] Centroids, BoundingBox Root, BuildPolicy Policy) : SpatialIndex;

    // Union-wide columns as abstract get/init overridden by positional case synthesis — a base
    // computed Switch under a same-name case param suppresses synthesis and self-recurses.
    public abstract NodeStore Store { get; init; }
    public abstract BoundingBox[] Primitives { get; init; }
    public abstract BuildPolicy Policy { get; init; }

    public SpatialKind Kind =>
        Switch(
            bvh: static b => b.Builder,
            linearOctree: static _ => SpatialKind.Octree);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Store.Count, floor: 1),
        ValidityClaim.CountExactly(count: Store.BoundsMin.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.BoundsMax.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.Order.Length, expected: Primitives.Length),
        ValidityClaim.Of(TensorPrimitives.IsFiniteAll<float>(Store.BoundsMin) && TensorPrimitives.IsFiniteAll<float>(Store.BoundsMax)),
        ValidityClaim.Of(Links(Store)));

    // --- [ADMISSION]
    // Admitted ONCE with a detaching copy: the frozen index never aliases a caller-mutable array.
    internal static Fin<BoundingBox[]> Admit(BoundingBox[] primitives) {
        if (primitives.Length == 0)
            return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, -1, "empty").ToError());
        for (int i = 0; i < primitives.Length; i++)
            if (!primitives[i].IsValid)
                return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, i, "non-finite-bound").ToError());
        return Fin.Succ((BoundingBox[])primitives.Clone());
    }

    internal static Point3d[] Centroids(BoundingBox[] boxes) =>
        Array.ConvertAll(boxes, static box => 0.5 * (box.Min + box.Max));

    // --- [BUILD]
    internal static SpatialIndex BuildBvh(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        using Arena arena = Arena.Rent(boxes.Length);
        int[] order = Enumerable.Range(0, boxes.Length).ToArray();
        int next = 1;
        void Partition(int node, int lo, int hi, int depth) {
            BoundingBox bound = Union(boxes, order, lo, hi);
            int count = hi - lo;
            if (count <= policy.LeafSize || depth >= policy.MaxDepth) {
                arena.Write(node, bound, 0, 0, lo, count);
                return;
            }
            BoundingBox centroidBound = CentroidBound(centroids, order, lo, hi);
            (int axis, double cost, int splitBucket) = BestSah(boxes, centroids, order, lo, hi, bound, centroidBound, policy.SahBuckets);
            if (cost >= count) {
                arena.Write(node, bound, 0, 0, lo, count);
                return;
            }
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            int mid = StablePartition(order, lo, hi, idx =>
                (int)(policy.SahBuckets * (Axis(centroids[idx], axis) - Axis(centroidBound.Min, axis)) / Math.Max(extent, double.Epsilon)) <= splitBucket);
            mid = mid == lo || mid == hi ? (lo + hi) / 2 : mid;
            int firstChild = next;
            next += 2;
            arena.Write(node, bound, firstChild, 2, -1, 0);
            Partition(firstChild, lo, mid, depth + 1);
            Partition(firstChild + 1, mid, hi, depth + 1);
        }
        Partition(0, 0, boxes.Length, 0);
        NodeStore store = arena.Freeze(next, order);
        return new Bvh(store, boxes, policy.LeafSize, SahCost(store), policy, SpatialKind.Bvh);
    }

    internal static SpatialIndex BuildOctree(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        BoundingBox root = Union(boxes);
        (uint[] codes, int[] order) = MortonOrder(boxes.Length, centroids, root);
        using Arena arena = Arena.Rent(boxes.Length);
        int next = 1;
        void Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
            int count = hi - lo;
            if (count <= policy.LeafSize || depth >= Math.Min(policy.MaxDepth, MortonDepth)) {
                arena.Write(node, Union(boxes, order, lo, hi), 0, 0, lo, count);
                return;
            }
            int shift = 3 * (MortonDepth - 1 - depth);
            List<(int Lo, int Hi)> runs = new(8);
            int runStart = lo;
            for (int i = lo + 1; i <= hi; i++) {
                bool boundary = i == hi || ((codes[i] >> shift) & 0x7) != ((codes[runStart] >> shift) & 0x7);
                if (boundary) { runs.Add((runStart, i)); runStart = i; }
            }
            int firstChild = next;
            next += runs.Count;
            arena.Write(node, bound, firstChild, runs.Count, -1, 0);
            for (int c = 0; c < runs.Count; c++)
                Cell(firstChild + c, runs[c].Lo, runs[c].Hi, depth + 1, Union(boxes, order, runs[c].Lo, runs[c].Hi));
        }
        Cell(0, 0, boxes.Length, 0, root);
        return new LinearOctree(arena.Freeze(next, order), boxes, centroids, root, policy);
    }

    // Agglomerative PLOC: Morton presort, windowed mutually-nearest merges recorded as a parent
    // forest, then ONE breadth-first compaction emitting the dense parent-before-child store —
    // the swap-based relink (which corrupted inbound child references) is the deleted form.
    internal static SpatialIndex BuildAgglomerative(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        int n = boxes.Length;
        BoundingBox rootBound = Union(boxes);
        (_, int[] order) = MortonOrder(n, centroids, rootBound);
        int capacity = Math.Max(1, 2 * n - 1);
        int[] childA = new int[capacity];
        int[] childB = new int[capacity];
        BoundingBox[] bound = new BoundingBox[capacity];
        int[] leafSlot = new int[capacity];
        for (int i = 0; i < n; i++) { bound[i] = boxes[order[i]]; leafSlot[i] = i; }
        List<(int Node, BoundingBox Bound)> live = new(n);
        for (int i = 0; i < n; i++) live.Add((i, bound[i]));
        int next = n;
        int Nearest(int i) {
            int best = -1; double bestArea = double.MaxValue;
            for (int j = Math.Max(0, i - policy.SahBuckets); j <= Math.Min(live.Count - 1, i + policy.SahBuckets); j++) {
                if (j == i) continue;
                BoundingBox merged = live[i].Bound; merged.Union(live[j].Bound);
                double area = SurfaceArea(merged);
                if (area < bestArea) { bestArea = area; best = j; }
            }
            return best;
        }
        bool Mutual(int i, int j) => Nearest(j) == i;
        while (live.Count > 1) {
            int i = 0, j = Nearest(0);
            for (int k = 1; k < live.Count; k++) { int m = Nearest(k); if (m >= 0 && Mutual(k, m)) { i = k; j = m; break; } }
            if (j < 0) j = i == 0 ? 1 : 0;
            (int lo, int hi) = i < j ? (i, j) : (j, i);
            BoundingBox merged = live[lo].Bound; merged.Union(live[hi].Bound);
            int parent = next++;
            (childA[parent], childB[parent], bound[parent], leafSlot[parent]) = (live[lo].Node, live[hi].Node, merged, -1);
            live[lo] = (parent, merged);
            live.RemoveAt(hi);
        }
        NodeStore store = Compact(live[0].Node, next, childA, childB, bound, leafSlot, order);
        return new Bvh(store, boxes, policy.LeafSize, SahCost(store), policy, SpatialKind.Agglomerative);
    }

    // BFS compaction: visit order assigns each internal node's children CONSECUTIVE new slots,
    // so the frozen store satisfies the contiguous child-range and parent-before-child laws.
    static NodeStore Compact(int root, int total, int[] childA, int[] childB, BoundingBox[] bound, int[] leafSlot, int[] order) {
        int[] visit = new int[total];
        int[] map = new int[total];
        int count = 0;
        visit[count] = root; map[root] = count++;
        for (int head = 0; head < count; head++) {
            int old = visit[head];
            if (leafSlot[old] >= 0) continue;
            (map[childA[old]], visit[count]) = (count, childA[old]); count++;
            (map[childB[old]], visit[count]) = (count, childB[old]); count++;
        }
        using Arena arena = new(count);
        for (int node = 0; node < count; node++) {
            int old = visit[node];
            if (leafSlot[old] >= 0) arena.Write(node, bound[old], 0, 0, leafSlot[old], 1);
            else arena.Write(node, bound[old], map[childA[old]], 2, -1, 0);
        }
        return arena.Freeze(count, order);
    }

    static (uint[] Codes, int[] Order) MortonOrder(int count, Point3d[] centroids, BoundingBox root) {
        Vector3d span = root.Max - root.Min;
        uint[] codes = Array.ConvertAll(centroids, c => Morton(
            Normalize(c.X, root.Min.X, span.X), Normalize(c.Y, root.Min.Y, span.Y), Normalize(c.Z, root.Min.Z, span.Z)));
        int[] order = Enumerable.Range(0, count).ToArray();
        Array.Sort(codes, order);
        return (codes, order);
    }

    // SAH tree cost: SIMD extent subtraction + one Dot reduction over per-node weight·area,
    // root-normalized; internal nodes weight the 0.125 traversal constant, leaves the count.
    static double SahCost(NodeStore store) {
        int count = store.Count;
        using SpanOwner<float> extent = SpanOwner<float>.Allocate(3 * count);
        using SpanOwner<float> area = SpanOwner<float>.Allocate(count);
        using SpanOwner<float> weight = SpanOwner<float>.Allocate(count);
        TensorPrimitives.Subtract<float>(store.BoundsMax.AsSpan(0, 3 * count), store.BoundsMin.AsSpan(0, 3 * count), extent.Span);
        Span<float> d = extent.Span, sa = area.Span, w = weight.Span;
        for (int node = 0; node < count; node++) {
            int b = 3 * node;
            sa[node] = 2f * ((d[b] * d[b + 1]) + (d[b + 1] * d[b + 2]) + (d[b + 2] * d[b]));
            w[node] = store.LeafCount[node] > 0 ? store.LeafCount[node] : 0.125f * store.ChildCount[node];
        }
        return TensorPrimitives.Dot<float>(w, sa) / Math.Max(sa[0], float.Epsilon);
    }

    static (int Axis, double Cost, int Bucket) BestSah(BoundingBox[] boxes, Point3d[] centroids, int[] order, int lo, int hi, BoundingBox bound, BoundingBox centroidBound, int buckets) =>
        toSeq(Enumerable.Range(0, 3)).Fold((Axis: 0, Cost: double.MaxValue, Bucket: 0), (best, axis) => {
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            if (extent <= double.Epsilon) return best;
            int[] counts = new int[buckets];
            BoundingBox[] bins = new BoundingBox[buckets];
            for (int b = 0; b < buckets; b++) bins[b] = BoundingBox.Empty;
            for (int i = lo; i < hi; i++) {
                int bucket = Math.Min(buckets - 1, (int)(buckets * (Axis(centroids[order[i]], axis) - Axis(centroidBound.Min, axis)) / extent));
                counts[bucket]++;
                bins[bucket].Union(boxes[order[i]]);
            }
            return toSeq(Enumerable.Range(0, buckets - 1)).Fold(best, (acc, split) => {
                (BoundingBox lBox, int lCount) = Accumulate(bins, counts, 0, split + 1);
                (BoundingBox rBox, int rCount) = Accumulate(bins, counts, split + 1, buckets);
                double cost = 0.125 + (lCount * SurfaceArea(lBox) + rCount * SurfaceArea(rBox)) / Math.Max(SurfaceArea(bound), double.Epsilon);
                return cost < acc.Cost ? (axis, cost, split) : acc;
            });
        });

    static (BoundingBox Box, int Count) Accumulate(BoundingBox[] bins, int[] counts, int from, int to) {
        BoundingBox box = BoundingBox.Empty;
        int count = 0;
        for (int b = from; b < to; b++) { box.Union(bins[b]); count += counts[b]; }
        return (box, count);
    }

    // --- [QUERY]
    // Total over {Bvh, LinearOctree} × the five query cases: a malformed scalar routes the Op
    // admission rail, a payload the index content cannot bind routes the typed KindMismatch —
    // the silent-empty default arm is the deleted form.
    internal Fin<QueryResult> Query(SpatialQuery probe, Op key) {
        SpatialIndex self = this;
        return probe.Switch(
            state: (Self: self, Key: key),
            range: static (s, q) =>
                guard(q.Box.IsValid && q.Ball.Match(static ball => ball.IsValid, static () => true), s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Hits(RangeHits(s.Self.Store, s.Self.Primitives, q))),
            ray: static (s, q) =>
                guard(q.Probe.Direction.Length > 0.0 && double.IsFinite(q.MaxT) && q.MaxT > 0.0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)RayNearest(s.Self.Store, s.Self.Primitives, q)),
            nearest: static (s, q) =>
                guard(q.K > 0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Nearest(KNearest(s.Self.Store, s.Self.Primitives, q))),
            overlap: static (s, q) =>
                guard(double.IsFinite(q.Tolerance) && q.Tolerance >= 0.0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Pairs(OverlapPairs(s.Self, q.Other, q.Tolerance))),
            winding: static (s, q) =>
                q.Triangles.Length != 3 * s.Self.Primitives.Length
                    ? Fin.Fail<QueryResult>(new GeometryFault.KindMismatch(s.Self.Kind, QueryKind.Winding).ToError())
                    : guard(q.Queries.Length > 0 && double.IsFinite(q.BetaSquared) && q.BetaSquared > 0.0, s.Key.InvalidInput()).ToFin()
                        .Map(_ => (QueryResult)new QueryResult.Field(Winding(s.Self.Store, q))));
    }

    // ONE bottom-up moment pass per evaluation (reverse index order = child-before-parent), then
    // EVERY query point reads the cached node moments — re-walking a subtree per far-field visit
    // is the deleted form that priced the "accelerated" descent at the exact sum's own cost.
    static double[] Winding(NodeStore store, SpatialQuery.Winding query) {
        (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(store, query.Triangles);
        return Array.ConvertAll(query.Queries, point => WindingAt(store, query, dipole, weighted, area, point));
    }

    static (Vector3d[] Dipole, Point3d[] Weighted, double[] Area) Moments(NodeStore store, Point3d[] triangles) {
        Vector3d[] dipole = new Vector3d[store.Count];
        Point3d[] weighted = new Point3d[store.Count];
        double[] area = new double[store.Count];
        for (int node = store.Count - 1; node >= 0; node--)
            if (store.LeafCount[node] > 0)
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int tri = store.Order[store.LeafStart[node] + s];
                    Point3d a = triangles[3 * tri], b = triangles[3 * tri + 1], c = triangles[3 * tri + 2];
                    Vector3d normal = 0.5 * Vector3d.CrossProduct(b - a, c - a);
                    double weight = normal.Length;
                    dipole[node] += normal;
                    weighted[node] += weight * ((a + b + c) / 3.0);
                    area[node] += weight;
                }
            else
                for (int c = 0; c < store.ChildCount[node]; c++) {
                    int child = store.FirstChild[node] + c;
                    dipole[node] += dipole[child];
                    weighted[node] += weighted[child];
                    area[node] += area[child];
                }
        return (dipole, weighted, area);
    }

    static double WindingAt(NodeStore store, SpatialQuery.Winding query, Vector3d[] dipole, Point3d[] weighted, double[] area, Point3d point) {
        const double FourPiInverse = 0.079577471545947667884441881686257;
        double total = 0.0;
        Stack<int> stack = new();
        stack.Push(0);
        while (stack.Count > 0) {
            int node = stack.Pop();
            BoundingBox bound = store.Bound(node);
            Point3d centre = 0.5 * (bound.Min + bound.Max);
            double radius = 0.5 * (bound.Max - bound.Min).Length;
            double distance = centre.DistanceTo(point);
            bool leaf = store.LeafCount[node] > 0;
            if (!leaf && radius > 0.0 && distance * distance > query.BetaSquared * radius * radius) {
                if (area[node] > 1e-18) {
                    Vector3d r = (weighted[node] / area[node]) - point;
                    double len = r.Length;
                    total += len > 1e-18 ? FourPiInverse * (dipole[node] * r) / (len * len * len) : 0.0;
                }
                continue;
            }
            if (leaf) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int tri = store.Order[store.LeafStart[node] + s];
                    total += FourPiInverse * SolidAngle(query.Triangles[3 * tri], query.Triangles[3 * tri + 1], query.Triangles[3 * tri + 2], point);
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return total;
    }

    static double SolidAngle(Point3d a, Point3d b, Point3d c, Point3d p) {
        Vector3d ra = a - p, rb = b - p, rc = c - p;
        double la = ra.Length, lb = rb.Length, lc = rc.Length;
        double numerator = ra * Vector3d.CrossProduct(rb, rc);
        double denominator = la * lb * lc + (ra * rb) * lc + (rb * rc) * la + (rc * ra) * lb;
        return 2.0 * Math.Atan2(numerator, denominator);
    }

    static Seq<int> RangeHits(NodeStore store, BoundingBox[] primitives, SpatialQuery.Range range) {
        Seq<int> hits = [];
        Stack<int> stack = new();
        stack.Push(0);
        while (stack.Count > 0) {
            int node = stack.Pop();
            if (!Intersects(store.Bound(node), range.Box)) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    if (Intersects(primitives[prim], range.Box) && range.Ball.Match(ball => SphereHits(primitives[prim], ball), () => true))
                        hits = hits.Add(prim);
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return hits;
    }

    static QueryResult.RayHit RayNearest(NodeStore store, BoundingBox[] primitives, SpatialQuery.Ray ray) {
        Stack<int> stack = new();
        stack.Push(0);
        double best = ray.MaxT;
        int hit = -1;
        while (stack.Count > 0) {
            int node = stack.Pop();
            if (!Slab(store.Bound(node), ray.Probe, best, out _)) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    if (Slab(primitives[prim], ray.Probe, best, out double t) && t < best) { best = t; hit = prim; }
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return new QueryResult.RayHit(hit >= 0 ? Some(hit) : None, hit >= 0 ? best : ray.MaxT);
    }

    // Leaf distance = exact DOUBLE primitive-box distance (0 inside), so the k-NN verdict obeys
    // the leaves-re-test-the-double-boxes law; a centroid metric bounds nothing and is deleted.
    static Seq<int> KNearest(NodeStore store, BoundingBox[] primitives, SpatialQuery.Nearest knn) {
        PriorityQueue<int, double> heap = new();
        Stack<int> stack = new();
        stack.Push(0);
        double Worst() => heap.TryPeek(out _, out double p) ? -p : double.MaxValue;
        while (stack.Count > 0) {
            int node = stack.Pop();
            double lower = store.Bound(node).ClosestPoint(knn.Query).DistanceTo(knn.Query);
            if (heap.Count >= knn.K && lower > Worst()) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    double d = primitives[prim].ClosestPoint(knn.Query).DistanceTo(knn.Query);
                    if (heap.Count < knn.K) heap.Enqueue(prim, -d);
                    else heap.EnqueueDequeue(prim, -d);
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return toSeq(heap.UnorderedItems.OrderBy(static e => -e.Priority).Select(static e => e.Element));
    }

    // Self-overlap (ONE index on both sides, store reference-identical) dedups to pa < pb —
    // no reflexive pair, no mirrored duplicate; the modality is the value, never a knob.
    static Seq<(int Left, int Right)> OverlapPairs(SpatialIndex left, SpatialIndex right, double tolerance) {
        (NodeStore ls, BoundingBox[] lp) = (left.Store, left.Primitives);
        (NodeStore rs, BoundingBox[] rp) = (right.Store, right.Primitives);
        bool self = ReferenceEquals(ls, rs);
        Seq<(int, int)> pairs = [];
        Stack<(int L, int R)> stack = new();
        stack.Push((0, 0));
        while (stack.Count > 0) {
            (int l, int r) = stack.Pop();
            if (!Intersects(Inflate(ls.Bound(l), tolerance), rs.Bound(r))) continue;
            bool lLeaf = ls.LeafCount[l] > 0, rLeaf = rs.LeafCount[r] > 0;
            if (lLeaf && rLeaf) {
                for (int a = 0; a < ls.LeafCount[l]; a++)
                    for (int b = 0; b < rs.LeafCount[r]; b++) {
                        int pa = ls.Order[ls.LeafStart[l] + a], pb = rs.Order[rs.LeafStart[r] + b];
                        if ((!self || pa < pb) && Intersects(Inflate(lp[pa], tolerance), rp[pb])) pairs = pairs.Add((pa, pb));
                    }
            } else if (rLeaf || (!lLeaf && Diagonal(ls.Bound(l)) >= Diagonal(rs.Bound(r)))) {
                for (int c = 0; c < ls.ChildCount[l]; c++) stack.Push((ls.FirstChild[l] + c, r));
            } else {
                for (int c = 0; c < rs.ChildCount[r]; c++) stack.Push((l, rs.FirstChild[r] + c));
            }
        }
        return pairs;
    }

    // --- [REFIT]
    // Persistent refit: parallel leaf re-bound into FRESH arrays (partition-disjoint slots), one
    // reverse parent-before-child propagation, topology arrays shared — the input index is never
    // mutated; revised bounds re-enter through Admit (typed DegenerateInput, detached array), and
    // the SAH degradation gate rebuilds through the SAME SpatialKind.Build row.
    internal Fin<SpatialIndex> Refit(BoundingBox[] revised) =>
        revised.Length != Primitives.Length
            ? Fin.Fail<SpatialIndex>(new GeometryFault.IndexMismatch(EntityKind.Face, Primitives.Length, revised.Length).ToError())
            : Admit(revised).Map(Rebound);

    SpatialIndex Rebound(BoundingBox[] updated) {
        NodeStore store = Store;
        (float[] min, float[] max) = (new float[3 * store.Count], new float[3 * store.Count]);
        LeafRefit leaves = new(store, updated, min, max);
        ParallelHelper.For(0, store.Count, in leaves, Policy.ParallelFloor);
        for (int node = store.Count - 1; node >= 0; node--) {
            if (store.LeafCount[node] > 0) continue;
            int first = store.FirstChild[node];
            for (int axis = 0; axis < 3; axis++) { min[3 * node + axis] = float.MaxValue; max[3 * node + axis] = float.MinValue; }
            for (int c = 0; c < store.ChildCount[node]; c++)
                for (int axis = 0; axis < 3; axis++) {
                    min[3 * node + axis] = Math.Min(min[3 * node + axis], min[3 * (first + c) + axis]);
                    max[3 * node + axis] = Math.Max(max[3 * node + axis], max[3 * (first + c) + axis]);
                }
        }
        NodeStore refitted = store with { BoundsMin = min, BoundsMax = max };
        return Switch<SpatialIndex>(
            bvh: b => SahCost(refitted) > b.Policy.RefitDegradationLimit * b.BuildCost
                ? b.Builder.Build(updated, Centroids(updated), b.Policy)
                : b with { Primitives = updated, Store = refitted },
            linearOctree: o => o with { Primitives = updated, Centroids = Centroids(updated), Root = Union(updated), Store = refitted });
    }

    readonly struct LeafRefit(NodeStore store, BoundingBox[] boxes, float[] min, float[] max) : IAction {
        public void Invoke(int node) {
            if (store.LeafCount[node] == 0) return;
            BoundingBox bound = LeafBound(store, boxes, node);
            (min[3 * node], min[3 * node + 1], min[3 * node + 2]) = (Down(bound.Min.X), Down(bound.Min.Y), Down(bound.Min.Z));
            (max[3 * node], max[3 * node + 1], max[3 * node + 2]) = (Up(bound.Max.X), Up(bound.Max.Y), Up(bound.Max.Z));
        }
    }

    // --- [WIRE]
    // The frozen node-link wire: per-node interleaved AABB straight from the outward-rounded
    // store floats (no second rounding site), descriptor block packing (FirstChild<<21)|Count
    // for internals and -(((LeafStart'<<21)|LeafCount))-1 for leaves with LeafStart' the
    // ZERO-BASED tail offset, then the Order-permuted primitive-id tail.
    internal static (float[] Bounds, long[] Nodes) NodeLinkProjection(NodeStore store) {
        int count = store.Count;
        float[] bounds = new float[6 * count];
        long[] nodes = new long[count + store.Order.Length];
        int tail = count;
        for (int node = 0; node < count; node++) {
            store.BoundsMin.AsSpan(3 * node, 3).CopyTo(bounds.AsSpan(6 * node, 3));
            store.BoundsMax.AsSpan(3 * node, 3).CopyTo(bounds.AsSpan((6 * node) + 3, 3));
            if (store.LeafCount[node] > 0) {
                nodes[node] = -(((long)(tail - count) << ChildShift) | (uint)store.LeafCount[node]) - 1;
                for (int s = 0; s < store.LeafCount[node]; s++)
                    nodes[tail++] = store.Order[store.LeafStart[node] + s];
            } else {
                nodes[node] = ((long)store.FirstChild[node] << ChildShift) | (uint)store.ChildCount[node];
            }
        }
        return (bounds, nodes);
    }

    const int ChildShift = 21;
    const int MortonDepth = 10;

    // --- [KERNELS]
    // Pooled build arena: node rows stage in MemoryOwner<T> rented memory, bounds narrow
    // OUTWARD at this one write seam, and Freeze copies the used prefix into the exact-size
    // frozen NodeStore — the pooled memory dies with the build.
    sealed class Arena(int capacity) : IDisposable {
        readonly MemoryOwner<float> mins = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<float> maxs = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<int> firstChild = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> childCount = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafStart = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafCount = MemoryOwner<int>.Allocate(capacity);

        internal static Arena Rent(int primitives) => new(Math.Max(1, 12 * primitives + 1));

        internal void Write(int node, BoundingBox box, int first, int children, int start, int count) {
            Span<float> lo = mins.Span, hi = maxs.Span;
            (lo[3 * node], lo[3 * node + 1], lo[3 * node + 2]) = (Down(box.Min.X), Down(box.Min.Y), Down(box.Min.Z));
            (hi[3 * node], hi[3 * node + 1], hi[3 * node + 2]) = (Up(box.Max.X), Up(box.Max.Y), Up(box.Max.Z));
            (firstChild.Span[node], childCount.Span[node], leafStart.Span[node], leafCount.Span[node]) = (first, children, start, count);
        }

        internal NodeStore Freeze(int count, int[] order) => new(count,
            mins.Span[..(3 * count)].ToArray(), maxs.Span[..(3 * count)].ToArray(),
            firstChild.Span[..count].ToArray(), childCount.Span[..count].ToArray(),
            leafStart.Span[..count].ToArray(), leafCount.Span[..count].ToArray(), order);

        public void Dispose() {
            mins.Dispose(); maxs.Dispose(); firstChild.Dispose();
            childCount.Dispose(); leafStart.Dispose(); leafCount.Dispose();
        }
    }

    // Outward narrowing: Down(v) <= v and Up(v) >= v for every finite double, so a float node
    // bound can never falsely prune a true hit; exact leaf tests re-read the double boxes.
    static float Down(double value) => float.BitDecrement((float)value);
    static float Up(double value) => float.BitIncrement((float)value);

    static bool Links(NodeStore store) {
        for (int node = 0; node < store.Count; node++) {
            bool leaf = store.LeafCount[node] > 0;
            if (leaf && (store.LeafStart[node] < 0 || store.LeafStart[node] + store.LeafCount[node] > store.Order.Length)) return false;
            if (!leaf && store.ChildCount[node] > 0 && (store.FirstChild[node] <= node || store.FirstChild[node] + store.ChildCount[node] > store.Count)) return false;
        }
        return true;
    }

    static BoundingBox Union(BoundingBox[] boxes, int[] order, int lo, int hi) {
        BoundingBox box = BoundingBox.Empty;
        for (int i = lo; i < hi; i++) box.Union(boxes[order[i]]);
        return box;
    }

    static BoundingBox Union(ReadOnlySpan<BoundingBox> boxes) {
        BoundingBox box = BoundingBox.Empty;
        foreach (BoundingBox b in boxes) box.Union(b);
        return box;
    }

    static BoundingBox CentroidBound(Point3d[] centroids, int[] order, int lo, int hi) {
        BoundingBox box = BoundingBox.Empty;
        for (int i = lo; i < hi; i++) box.Union(centroids[order[i]]);
        return box;
    }

    static BoundingBox LeafBound(NodeStore store, BoundingBox[] boxes, int node) {
        BoundingBox box = BoundingBox.Empty;
        for (int s = 0; s < store.LeafCount[node]; s++) box.Union(boxes[store.Order[store.LeafStart[node] + s]]);
        return box;
    }

    // Host in-place Inflate over the struct COPY — the pure form of the owned member.
    static BoundingBox Inflate(BoundingBox box, double tolerance) {
        box.Inflate(tolerance);
        return box;
    }

    static double SurfaceArea(BoundingBox box) {
        Vector3d d = box.Max - box.Min;
        return d.IsZero ? 0.0 : 2.0 * (d.X * d.Y + d.Y * d.Z + d.Z * d.X);
    }

    static double Diagonal(BoundingBox box) => (box.Max - box.Min).Length;
    static double Axis(Point3d p, int axis) => axis == 0 ? p.X : axis == 1 ? p.Y : p.Z;
    static double Axis(Vector3d v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    static bool Intersects(BoundingBox a, BoundingBox b) =>
        a.Min.X <= b.Max.X && a.Max.X >= b.Min.X && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y && a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;

    static bool SphereHits(BoundingBox box, Sphere ball) => box.ClosestPoint(ball.Center).DistanceTo(ball.Center) <= ball.Radius;

    static bool Slab(BoundingBox box, Ray3d ray, double max, out double t) {
        double tMin = 0.0, tMax = max;
        for (int axis = 0; axis < 3; axis++) {
            double origin = Axis(ray.Position, axis), dir = Axis(ray.Direction, axis);
            double lo = Axis(box.Min, axis), hi = Axis(box.Max, axis);
            if (Math.Abs(dir) < 1e-12) {
                if (origin < lo || origin > hi) { t = double.MaxValue; return false; }
                continue;
            }
            double inv = 1.0 / dir;
            double near = (lo - origin) * inv, far = (hi - origin) * inv;
            if (near > far) (near, far) = (far, near);
            tMin = Math.Max(tMin, near);
            tMax = Math.Min(tMax, far);
            if (tMin > tMax) { t = double.MaxValue; return false; }
        }
        t = tMin;
        return true;
    }

    static uint Morton(uint x, uint y, uint z) => Expand10(x) | (Expand10(y) << 1) | (Expand10(z) << 2);

    static uint Expand10(uint v) {
        v &= 0x3FF;
        v = (v | (v << 16)) & 0x030000FF;
        v = (v | (v << 8)) & 0x0300F00F;
        v = (v | (v << 4)) & 0x030C30C3;
        v = (v | (v << 2)) & 0x09249249;
        return v;
    }

    static uint Normalize(double value, double min, double span) =>
        span <= double.Epsilon ? 0u : (uint)Math.Clamp((int)(1023.0 * (value - min) / span), 0, 1023);

    static int StablePartition(int[] order, int lo, int hi, Func<int, bool> onLeft) {
        int write = lo;
        int[] buffer = new int[hi - lo];
        int b = 0;
        for (int i = lo; i < hi; i++) if (onLeft(order[i])) order[write++] = order[i]; else buffer[b++] = order[i];
        Array.Copy(buffer, 0, order, write, b);
        return write;
    }
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class Spatial {
    public static Fin<SpatialAnswer> Apply(SpatialOp op, Op? key = null) {
        Op minted = key.OrDefault();
        return op.Switch(
            state: minted,
            build: static (k, b) =>
                from boxes in SpatialIndex.Admit(b.Primitives)
                let built = b.Kind.Build(boxes, SpatialIndex.Centroids(boxes), b.Policy)
                from _ in guard(built.IsValid, k.InvalidResult()).ToFin()
                select (SpatialAnswer)new SpatialAnswer.Index(built),
            refit: static (k, r) =>
                from refitted in r.Index.Refit(r.Updated)
                from _ in guard(refitted.IsValid, k.InvalidResult()).ToFin()
                select (SpatialAnswer)new SpatialAnswer.Index(refitted),
            query: static (k, q) => q.Index.Query(q.Probe, k).Map(static r => (SpatialAnswer)new SpatialAnswer.Result(r)),
            wire: static (_, w) => Fin.Succ(SpatialIndex.NodeLinkProjection(w.Index.Store))
                .Map(static t => (SpatialAnswer)new SpatialAnswer.Wire(t.Bounds, t.Nodes)));
    }
}
```

## [03]-[RESEARCH]

- [SAH_PARTITION] — the `BuildBvh` body is the bucketed surface-area-heuristic partition — `BestSah` bins centroids into `BuildPolicy.SahBuckets` buckets per axis, scans the `buckets-1` split planes accumulating left/right bounds and counts, and minimizes `0.125 + (lCount·SA(lBox) + rCount·SA(rBox)) / SA(node)`; a split is taken only when its cost undercuts the leaf cost (`count`), the degenerate all-one-side bucket falls back to the median, `BuildPolicy.MaxDepth` forces a leaf so an adversarial one-off-the-end split chain can never overrun the recursion stack (the same depth cap the octree consumes — one policy column, both kernels), internal nodes reserve their two child slots contiguously before recursing so the `[FirstChild, FirstChild+ChildCount)` range is dense and parent-before-child, and the node rows stage in the pooled `Arena` (capacity `12·primitiveCount+1`) before `Freeze` copies the used prefix into the exact-size frozen `NodeStore` — the kernel is transcription-complete and pure-managed, no host probe.
- [MORTON_OCTREE] — the `BuildOctree` body normalizes each centroid axis to a 10-bit grid, interleaves through the portable magic-number `Expand10` bit-spread (no `Bmi2.X64.ParallelBitDeposit` host dependence so the build is RID-agnostic), does ONE keyed sort (`Array.Sort(codes, order)` permutes `order` by the Morton key in a single pass), and builds the cell hierarchy by splitting each sorted run on the depth-indexed octant nibble (`3·(MortonDepth-1-depth)` shift, `MortonDepth = 10` levels) — every occupied octant (1–8) is retained as a child in a contiguous reserved slot run, so no middle octant is dropped and the traversal walks the full child range; the linear-octree contract — sort-by-Morton then radix-run subdivision into a dense child range — is the entire correctness claim and is pure-managed.
- [AGGLOMERATIVE_BUILD] — `BuildAgglomerative` is the bottom-up locally-ordered clustering builder beside the top-down SAH: Morton-presort the centroids by the SAME 30-bit Z-order bit-spread, seed every primitive as a single-primitive leaf, then iteratively merge the mutually-nearest cluster pair (`Nearest`/`Mutual` over a `SahBuckets`-wide sliding window by minimum merged surface area — the field-standard PLOC nearest-neighbour agglomeration) RECORDING each merge as a parent row in a `(childA, childB, bound)` forest, re-inserting the parent at the lower live slot so Morton locality survives the merge sequence. One breadth-first `Compact` pass then re-indexes the reachable forest into the SAME dense SoA `NodeStore` the SAH/octree kernels emit — BFS visit order assigns every internal node's two children CONSECUTIVE new slots, so the contiguous `[FirstChild, FirstChild+ChildCount)` and parent-before-child laws hold by construction and the root lands at node 0 with no swap. The retired swap-based `RelinkPair`/`Swap`/`Reroot` relink is the deleted form: exchanging two node rows in place left every inbound `FirstChild` reference and every `live`-list node id pointing at relocated content — a silent store corruption the recorded-forest + compaction shape makes structurally impossible. The agglomerative tree's higher quality on clustered, incrementally-mutated clash geometry is the value claim, and it is one `SpatialKind.Agglomerative` row writing the `Bvh` case via the `Bvh.Builder` column, never a parallel index class.
- [GENERALIZED_WINDING] — the `Winding` query is the fast hierarchical generalized-winding-number evaluation (Barill/Jacobson tree-based GWN) over the EXISTING store, returning a robust per-query-point inside/outside field over defective triangle soups regardless of holes, self-intersection, or non-manifold defects — a `SpatialQuery.Winding` case folded by the same `Query`, never a new structure, and BATCH-shaped by construction: `Queries` is a `Point3d[]` (a single point is a 1-length array — the modality is the value's shape), because the consuming `fields.md` `SignedDistanceFromMesh` → `reconstruct.md` GWN chain evaluates a FIELD of sample points. `Moments` runs ONE bottom-up pass per evaluation (reverse index order — every builder emits parent-before-child, so children resolve before their parent) caching per-node area-weighted dipole, weighted-centroid, and area arrays in O(n); each `WindingAt` descent then reads the cached moment O(1) at every far-field cut — the per-visit subtree re-walk is the deleted form whose far-field branch cost equalled the exact per-triangle sum it claimed to avoid, an acceleration in name only. The descent takes the single far-field dipole term `(dipole · r) / (4π |r|³)` when the query point clears the `β`-scaled node radius (`radius > 0 && distance² > βSquared · radius²`, the `BetaSquared` accuracy knob), otherwise recurses, and at a leaf sums the EXACT per-triangle solid angle via the numerically-stable `atan2` half-angle form (a degenerate zero-radius internal node never takes the far-field branch and descends to its exact leaves; a zero-area far-field node contributes nothing). Each field value is `~1` strictly inside, `~0` strictly outside, continuous across defects. GWN SINGLE-OWNER ruling: this page owns the accelerated winding query; the landed `fields.md` `SignedDistanceFromMesh` → `reconstruct.md` GWN chain COMPOSES it — the ONE distance-field lane (the Fabrication distance-field ingress), and a second GWN or SDF evaluator beside it is the deleted duplicate. A `Winding` payload that does not cover the index (`Triangles.Length != 3·primitiveCount`) routes the typed `KindMismatch(kind, QueryKind.Winding)` 2402 — never a silent partial sum; an empty `Queries` routes `key.InvalidInput()`. The tier-2 law-matrix asserts the GWN converges to the brute-force per-triangle sum as `βSquared → ∞` and the inside/outside classification matches a known-watertight reference; no host probe.
- [DEGRADATION_REFIT] — the degradation-keyed `Refit` is topology-stable PERSISTENT re-bounding plus a deterministic rebuild trigger: the parallel leaf pass (`ParallelHelper.For` over the `LeafRefit` struct `IAction`, floored by `BuildPolicy.ParallelFloor`, each leaf writing its disjoint slot in fresh outward-rounded bound arrays) is followed by one reverse-index internal propagation — sound because every builder emits parent-before-child numbering — and the refitted store SHARES the immutable topology arrays while replacing only the bounds, so the input index survives unmutated; the revised boxes re-enter through the SAME `Admit` seam the build uses, so a non-finite revision faults `DegenerateInput` WITH its index before any store write and the caller's array is detached, never aliased. `SahCost` reads the refitted tree cost through one `TensorPrimitives.Subtract` extent pass and one `TensorPrimitives.Dot` weight·area reduction (internal nodes weighted by the `0.125` traversal constant, leaves by primitive count — the same metric `BestSah` minimizes, root-normalized) against the `Bvh.BuildCost` baseline frozen at build; past `BuildPolicy.RefitDegradationLimit × BuildCost` the refit rebuilds fully through the SAME `SpatialKind.Build` row the index was built with. The trigger is a pure function of the frozen baseline and the refitted bounds, so a long incremental edit session is reproducible — refit until the SAH quality degrades past the limit, then rebuild — and the `Wire` projection is unchanged by either path (the rebuild is transparent to the seam). The tier-2 property: `Refit` followed by `Query` returns the IDENTICAL result set to a fresh `Build` over the updated boxes, and the rebuild trigger fires deterministically iff the refitted cost crosses the limit.
- [CLASH_SEAM] — the `Wire` op case emits the FROZEN node-link wire `(float[] Bounds, long[] Nodes)` over `NodeLinkProjection`, and the `Rasm.Compute` `ClashScale.NodeLinkPairs` traversal decodes it by a proper descent over the contiguous `[FirstChild, FirstChild+ChildCount)` child range — the kernel emits raw arrays and Compute decodes (the retired `ToAcceleration` member returning the Compute `AccelerationStructure` union was the upward strata edge this rebuild deletes; the reverse Compute-decodes-kernel direction is settled at the counterpart ledger). Canonical layout: `Bounds` = `6·NodeCount` little-endian `float32`, node `i`'s AABB at `Bounds[i·6 .. i·6+6)` as `[minX,minY,minZ,maxX,maxY,maxZ]` copied DIRECTLY from the outward-rounded store floats (node-index order, root = node 0 — no second rounding site exists on the wire); `Nodes` = `NodeCount + primitiveCount` little-endian `int64` split as a `NodeCount`-long descriptor block followed by a primitive-id tail — `Nodes[node]` non-negative is an INTERNAL node packing `(FirstChild << 21) | ChildCount` (binary BVH node is `ChildCount == 2`, octree cell is `ChildCount ∈ [1,8]`), `Nodes[node]` negative is a LEAF packing `-(((LeafStart' << 21) | LeafCount)) - 1` where `LeafStart'` is the ZERO-BASED offset into the tail and `Nodes[NodeCount + LeafStart' + s]` for `s ∈ [0, LeafCount)` is the leaf primitive id (the `Order`-permuted primitive index). `NodeCount == Bounds.Length/6`; the low 21-bit field caps `ChildCount`/`LeafCount` at `2²¹−1` while the high field carries `FirstChild`/`LeafStart'` to `2⁴²−1` — ample for the frozen exact node count (`≤ 2·primitiveCount − 1` for the binary builders, `≤ MortonDepth·primitiveCount + 1` for the octree) and for any leaf width the builders emit. The retired projection packed the ABSOLUTE tail position where its own decode law and fixture assume the tail-relative `LeafStart'` — the tail-relative pack is the corrected, fixture-consistent form.
- [CLASH_GOLDEN] (two-sided fixture — HARNESS-GATED: the byte stream re-derives under the outward-rounding law and is asserted REAL only after the one-time `NodeLinkProjection` harness reproof on both sides): the canonical input is the FROZEN 8-primitive `BoundingBox(min,max)` set, two X-separated clusters of four unit cubes — `(0,0,0)→(1,1,1)` · `(0.5,0,0)→(1.5,1,1)` · `(2,0,0)→(3,1,1)` · `(2.5,0,0)→(3.5,1,1)` · `(10,0,0)→(11,1,1)` · `(10.5,0,0)→(11.5,1,1)` · `(12,0,0)→(13,1,1)` · `(12.5,0,0)→(13.5,1,1)` — built with `BuildPolicy.Canonical` (`LeafSize: 4`) through `Spatial.Apply(new SpatialOp.Build(SpatialKind.Bvh, …))`. The build is deterministic and its TOPOLOGY is pinned: `BestSah` selects axis X, bins primitives `{0,1,2,3}` left and `{4,5,6,7}` right, the split cost `≈ 2.41` undercuts the leaf cost `8`, `StablePartition` keeps `Order` the identity `[0..7]`, and the frozen outcome is `NodeCount == 3` (root node 0 internal, node 1 left leaf, node 2 right leaf) with node bounds the OUTWARD-rounded float form of `node0 = (0,0,0)→(13.5,1,1)`, `node1 = (0,0,0)→(3.5,1,1)`, `node2 = (10,0,0)→(13.5,1,1)` (each min one `float.BitDecrement` below, each max one `float.BitIncrement` above the exact cast). The descriptor block is pinned exactly: `Nodes[0] = (1 << 21) | 2 = 2097154`, `Nodes[1] = -(((0 << 21) | 4)) - 1 = -5`, `Nodes[2] = -(((4 << 21) | 4)) - 1 = -8388613`, tail `Nodes[3..11) = [0,1,2,3,4,5,6,7]`; the 160-byte contiguous `Bounds`-then-`Nodes` little-endian stream is byte-derivable from those facts plus the outward-rounding law, and the harness freezes it once before either side cites the bytes REAL. The `Rasm.Compute` side decodes the same stream over the contiguous child range to the PINNED clash-pair set `{(0,1),(2,3),(4,5),(6,7)}` (4 pairs — every pair intra-leaf; the X-cluster separation, six full units wide, dominates the one-ULP outward inflation, so no cross-leaf candidate appears and the hierarchical descent stays strictly cheaper than the deleted O(N²) all-pairs form); the round-trip (kernel emits → Compute decodes → 4-pair set confirmed) is the seam-settled signal.

## [04]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface.

| [INDEX] | [AXIS/CONCERN] | [OWNER]        | [KIND]                                                                                                                                                | [RAIL]                                          | [CASES] |
| :-----: | :------------- | :------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ | :-----: |
|  [01]   | Entry          | `Spatial`/`SpatialOp` | `[Union]` (`Build`/`Refit`/`Query`/`Wire`) folded by ONE `Apply` with `Op? key` threading                                                        | `Spatial.Apply → Fin<SpatialAnswer>`               |    4    |
|  [02]   | Builder rows   | `SpatialKind`  | `[SmartEnum<string>]` (`bvh`/`octree`/`agglomerative`) + `[UseDelegateFromConstructor]` `Build` behavior column                                          | row delegate → `SpatialIndex` (interior)           |    3    |
|  [03]   | Spatial index  | `SpatialIndex` | `[Union]` (`Bvh`/`LinearOctree`) over one frozen `NodeStore`, `IValidityEvidence` registered, persistent degradation-keyed refit                         | via `Apply` (`Build`/`Refit` arms, validity-gated) |    2    |
|  [04]   | Spatial query  | `SpatialQuery` | `[Union]` (`Range`/`Ray`/`Nearest`/`Overlap`/`Winding`) + `QueryKind` discriminant, folded by one `Query`                                                | via `Apply` (`Query` arm) → `QueryResult`          |    5    |
|  [05]   | Answer         | `SpatialAnswer`| `[Union]` (`Index`/`Result`/`Wire`) — the wire case carries the frozen `(float[], long[])` node arrays                                                   | carrier (returned in the `Apply` rail)             |    3    |

The three build kernels (SAH-BVH top-down, Morton linear octree, agglomerative PLOC with BFS compaction), the five query bodies (range descent, front-to-back ray slab, best-first k-NN, tandem overlap, hierarchical GWN), and the persistent degradation-keyed refit are transcription-complete pure-managed fences over one frozen `NodeStore` SoA layout with a contiguous parent-before-child `[FirstChild, FirstChild+ChildCount)` child range that loses no octree octant and rides every agglomerative-merged node unchanged. Bounds narrow OUTWARD once at the arena write seam, the wire copies those floats verbatim, and every failure — degenerate admission, count mismatch, kind mismatch, malformed scalar — routes the ONE `Fin<SpatialAnswer>` rail through the band-2400 `GeometryFault` cases or the `Op` admission vocabulary. The `[CLASH_SEAM]` layout is settled; the `[CLASH_GOLDEN]` fixture is pinned in topology, descriptor block, and pair set, with the byte stream HARNESS-GATED for its one-time reproof under the outward-rounding law before either package cites the bytes REAL.
