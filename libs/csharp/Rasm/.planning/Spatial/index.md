# [RASM_SPATIAL_INDEX]

`Spatial.Apply` folds every broad-phase modality onto one `Fin<SpatialAnswer>` rail over a `SpatialIndex` `[Union]` whose kernels differ only in build partition strategy and share one frozen `NodeStore`, so query, refit, and wire read that store kernel-agnostically. This owner serves predicate-exact primitive-bounds broad phase alone.

`SpatialIndex` computes on raw primitive coordinates, never a unit-bearing quantity type. `NodeWalk` is this page's ONE hierarchy traversal owner and every query composes it: the monotone arm projects the child links onto a QuikGraph delegate graph and reads one breadth-first walk, while the state-threading arm is the named span kernel the four walks whose graph is not fixed per walk share. `NodeVerdict` is the one pruning vocabulary both arms read. `BuildPolicy` and every scalar it carries admit through the `Numerics/atoms` `Band` rows, so an inadmissible policy is unrepresentable and no consumer re-gates one. `Wire` is the one cross-package egress and carries raw arrays alone, so `Rasm.Compute` decodes with no Compute type entering this owner; `Rasm.Persistence` content-addresses the frozen `NodeStore` itself, and this owner mints no second store.

## [01]-[INDEX]

- [02]-[NODE_WALK]: `NodeWalk` owns every hierarchy descent, the monotone arm on QuikGraph and the state-threading arm named.
- [03]-[SPATIAL_INDEX]: `Spatial.Apply` folds every broad-phase op over the shared node store.

## [02]-[NODE_WALK]

- Owner: `NodeWalk` is the page's ONE traversal owner over a `NodeStore`; `NodeVerdict` rows are the per-node decision every query answers — `Prune` withholds the node and its whole subtree, `Absorb` visits the node and withholds its children, `Descend` visits and admits them. Both arms read the same three rows: `Reach` hands them to the graph container, `Descend` reads them off fold state.
- Law: subtree pruning is exact BECAUSE child bounds nest inside parent bounds — a verdict that is a pure function of the node is MONOTONE, so "reachable under the verdict" and "visited by a pruning descent" are the same node set and `Reach` may hand the decision to a graph container. Monotone is a property of the verdict over ONE walk, not over a query batch: a verdict parameterized by a per-item probe is monotone per item and re-roots a fresh graph per item, which is the cost the state-threading arm exists to refuse.
- Auto: `Reach` projects the store's child links onto `GraphExtensions.ToDelegateVertexAndEdgeListGraph` — the delegate withholds every child of a `Prune` or `Absorb` node, so the filter IS the adjacency and no second container materializes — then runs one `BreadthFirstSearchAlgorithm` under an `EdgeRecorderObserver`, whose `Edges` in visit order are the discovered nodes. Verdict memo answers each node once for the two questions the walk asks of it (its own children, and its parent's child filter), so a predicate never runs twice per walk.
- Exemption: `Descend` keeps ONE `Stack<TCursor>` frontier and is the named span kernel; `Reach`'s `NodeVerdict?[] seen` memo is build-kernel state for exactly one walk and never leaves it. `Descend` exists because four walks refuse `BreadthFirstSearchAlgorithm` and `DepthFirstSearchAlgorithm` alike: the ray and k-nearest walks tighten their admission bound from traversal state, so the filtered graph is not a fixed graph and no `IQueue<int>` carries the domain incumbent; the winding walk's verdict is a function of the QUERY POINT, so a graph arm builds and colours one whole-store graph per point where the descent visits only the nodes the multipole criterion admits; the dual walk's cursor is a node PAIR over two stores, and both catalogued lazy containers demand a surface the product has not got — `ToDelegateVertexAndEdgeListGraph` takes an `IEnumerable<TVertex>` both searches colour whole at `Initialize`, which materializes the |L|x|R| product, and `ToDelegateBidirectionalIncidenceGraph` demands in-edges a descent never reads.
- Growth: a new monotone query is one verdict function through `Reach`; a new state-threading query is one `step` through `Descend`; neither mints a frontier.
- Packages: QuikGraph (`GraphExtensions.ToDelegateVertexAndEdgeListGraph`, `BreadthFirstSearchAlgorithm`, `EdgeRecorderObserver`, `SEdge`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NodeVerdict {
    public static readonly NodeVerdict Prune = new(key: 0, offersChildren: false, visits: false);
    public static readonly NodeVerdict Absorb = new(key: 1, offersChildren: false, visits: true);
    public static readonly NodeVerdict Descend = new(key: 2, offersChildren: true, visits: true);

    internal bool OffersChildren { get; }
    internal bool Visits { get; }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class NodeWalk {
    internal static Seq<int> Reach(NodeStore store, Func<int, NodeVerdict> verdict) {
        // `NodeVerdict` is a class, so the array's own default IS the unfilled slot — a parallel `bool[]` re-derived
        // a fact the memo already carried, at two allocations and two writes where one suffices.
        NodeVerdict?[] seen = new NodeVerdict?[store.Count];
        NodeVerdict Decide(int node) => seen[node] ??= verdict(node);
        bool OutEdges(int node, out IEnumerable<SEdge<int>> edges) {
            edges = Decide(node: node).OffersChildren
                ? Enumerable.Range(store.FirstChild[node], store.ChildCount[node])
                    .Where(child => Decide(node: child).Visits)
                    .Select(child => new SEdge<int>(source: node, target: child))
                : [];
            return true;
        }
        if (!Decide(node: 0).Visits) { return []; }
        BreadthFirstSearchAlgorithm<int, SEdge<int>> walk = new(
            visitedGraph: GraphExtensions.ToDelegateVertexAndEdgeListGraph<int, SEdge<int>>(
                vertices: Enumerable.Range(0, store.Count), tryGetOutEdges: OutEdges));
        EdgeRecorderObserver<int, SEdge<int>> discovered = new();
        using (discovered.Attach(algorithm: walk)) {
            walk.Compute(root: 0);
        }
        return toSeq(discovered.Edges.Select(static edge => edge.Target).Prepend(0));
    }

    internal static TState Descend<TCursor, TState>(TCursor root, TState seed, Func<TState, TCursor, Stack<TCursor>, TState> step) {
        Stack<TCursor> frontier = new();
        frontier.Push(root);
        TState state = seed;
        while (frontier.Count > 0) {
            state = step(state, frontier.Pop(), frontier);
        }
        return state;
    }
}
```

## [03]-[SPATIAL_INDEX]

- Owner: `SpatialKind` rows own kernel selection, each row carrying its own builder over the shared `NodeStore` and each built case carrying the row that built it, so a refit rebuilds through its own kernel; `SpatialIndex.ClosestOnTriangle` is the one point-triangle refinement every consumer of this page's candidate prune reads, foot and distance leaving together.
- Receipt: `QueryResult` carries every query verdict, and the index itself is the registered validity evidence, so this owner mints no receipt type.
- Law: `BuildPolicy.Of` is the ONE admission — every scalar enters through a `Numerics/atoms` owner, so `Band.Count`'s closed floor of one is the authority that makes a zero leaf size, depth, bucket count, or parallel floor unrepresentable and the `IsAdmitted` bool it replaced carried nothing the band does not. `Canonical`'s figures each carry their provenance on site, because a band authorizes a RANGE and never a value. The packed-field ceiling DERIVES from `ChildShift`, both seated on `BuildPolicy` so the wire layout has ONE authority the projection reads, and `RefitGrowth` carries the degradation limit as a positive FRACTION above unity so a limit at or below one is unrepresentable rather than guarded. Far-field cut and every degeneracy floor on this page read `EpsilonPolicy.ZeroTolerance`, the branch's one degeneracy anchor — the ray slab included, its probe unitized at admission so the comparison shares that anchor's scale — so no page literal states either.
- Output: `RangeHits` and `SlabHits` publish LEVEL order — the monotone walk's own discovery order, the owner's published order for a hit set (`DIGEST_OVER_UNORDERED_CONTAINER`); `Nearest` publishes ascending distance, `Pairs` the dual descent's own order, and `Field` the query-point order the caller supplied.
- Exemption: the build kernels keep mutable accumulators for exactly one build — the octree's `List<(int Lo, int Hi)> runs` per cell, the agglomerative round's `nearest` and survivor arrays, the pooled `Arena`, bucket, bin, and partition spans, and the `Compact` visit/map arrays — and every one dies at `Freeze`; k-nearest selection composes the `Rasm.Domain` `Ranked` cell under `ExtremumDirection.Minimum`, whose `Bound` is the walk's pruning threshold, so the page holds no `PriorityQueue` of its own.
- Packages: RhinoCommon through `Rasm.Numerics`, `Rasm.Domain`, QuikGraph, CommunityToolkit.HighPerformance (`SpanOwner`/`MemoryOwner` the pooled arenas, `Span2D<T>.GetRowSpan` every strided plane read, `ParallelHelper.For` the PLOC sweep and the leaf refit), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new kernel is one `SpatialKind` row over the shared `NodeStore` and one `Builder` column on the case it mints, a new query one `SpatialQuery` case with its `QueryKind` row and `Query` arm, a new op one `SpatialOp` case and `Apply` arm, a new knob one `BuildPolicy` column; a new node layout is one `SpatialIndex` case, admitted only by charter amendment.
- Boundary: every failure routes the one `Fin` rail — `GeometryFault` on the geometry channel, `key.InvalidInput()` on the admission channel; point k-NN and radius over a bare point set route `neighbors.md`. `NodeLinkProjection` is the producer of the clash node-link branch golden `tests/csharp/README.md` `[09]-[SNAPSHOTS]` registers — producer and every decoder are C#, so the wire binds no peer runtime and earns no `tests/contracts/` corpus seat.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new("bvh", SpatialIndex.BuildBvh);
    public static readonly SpatialKind Octree = new("octree", SpatialIndex.BuildOctree);
    public static readonly SpatialKind Agglomerative = new("agglomerative", SpatialIndex.BuildAgglomerative);

    // The builder returns the RAIL: an arena that runs out of nodes is a typed refusal the entry already carries,
    // never an unguarded span write past the rented capacity.
    [UseDelegateFromConstructor]
    public partial Fin<SpatialIndex> Build(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueryKind {
    public static readonly QueryKind Range = new("range");
    public static readonly QueryKind Ray = new("ray");
    public static readonly QueryKind Nearest = new("nearest");
    public static readonly QueryKind Overlap = new("overlap");
    public static readonly QueryKind SelfOverlap = new("self-overlap");
    public static readonly QueryKind Winding = new("winding");
    public static readonly QueryKind Slab = new("slab");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BuildPolicy {
    private BuildPolicy(Dimension leafSize, Dimension maxDepth, Dimension sahBuckets, PositiveMagnitude refitGrowth, Dimension parallelFloor) =>
        (LeafSize, MaxDepth, SahBuckets, RefitGrowth, ParallelFloor) = (leafSize, maxDepth, sahBuckets, refitGrowth, parallelFloor);

    // The shift IS the wire layout, and the ceiling DERIVES from it — a hand-asserted copy of the same width let a
    // moved shift leave the ceiling silently wrong, admitting nodes whose leaf count then overflowed the child
    // field. Neither is a tuning bound and no `Band` row owns either; `NodeLinkProjection` reads both from here.
    public const int ChildShift = 21;
    public const int PackedCountMax = (1 << ChildShift) - 1;

    public Dimension LeafSize { get; }
    public Dimension MaxDepth { get; }
    public Dimension SahBuckets { get; }
    // Working figures pending measurement: RefitGrowth 0.6 calibrates at the refit-vs-rebuild query crossover on a
    // deforming scene; ParallelFloor 4096 at the partitioned-refit break-even under `ParallelHelper.For`.
    public PositiveMagnitude RefitGrowth { get; }
    public Dimension ParallelFloor { get; }
    public double RefitDegradationLimit => 1.0 + RefitGrowth.Value;

    [BoundaryAdapter]
    public static Fin<BuildPolicy> Of(int leafSize, int maxDepth, int sahBuckets, double refitGrowth, int parallelFloor, Op? key = null) {
        Op op = key.OrDefault();
        return from leaf in op.AcceptValidated<Dimension>(candidate: leafSize)
               from depth in op.AcceptValidated<Dimension>(candidate: maxDepth)
               from buckets in op.AcceptValidated<Dimension>(candidate: sahBuckets)
               from growth in op.AcceptValidated<PositiveMagnitude>(candidate: refitGrowth)
               from floor in op.AcceptValidated<Dimension>(candidate: parallelFloor)
               // Single buckets admit no split plane at all, and a leaf wider than the packed field cannot be wired.
               from _ in guard(leaf.Value <= PackedCountMax && buckets.Value > 1, op.InvalidInput()).ToFin()
               select new BuildPolicy(leafSize: leaf, maxDepth: depth, sahBuckets: buckets, refitGrowth: growth, parallelFloor: floor);
    }

    // Canonical row literals are constants inside every band, so the generated `Create` is total here; `Of` is the
    // ONE runtime admission and the only path a caller-supplied number takes. Each figure carries its provenance,
    // because the bands authorize the RANGE and never the value:
    //   leafSize    4 — PBRT 3e §4.3 maxPrimsInNode, the leaf width past which SAH traversal stops paying.
    //   maxDepth   32 — the conventional BVH ceiling; independent of MortonDepth, which caps the octree alone.
    //   sahBuckets 12 — PBRT 3e §4.3.2 nBuckets, the bucket count the SAH sweep is published against.
    //   refitGrowth/parallelFloor — UNMEASURED working defaults (a 60% SAH degradation before rebuild, a 4096-node
    //   partition floor), owed the measurement tracked at `[04]-[RESEARCH]` `[REFIT_BAND]`/`[PARALLEL_FLOOR]`.
    public static readonly BuildPolicy Canonical = new(
        leafSize: Dimension.Create(value: 4), maxDepth: Dimension.Create(value: 32), sahBuckets: Dimension.Create(value: 12),
        refitGrowth: PositiveMagnitude.Create(value: 0.6), parallelFloor: Dimension.Create(value: 4096));
}

public sealed record NodeStore(
    int Count,
    float[] BoundsMin,
    float[] BoundsMax,
    int[] FirstChild,
    int[] ChildCount,
    int[] LeafStart,
    int[] LeafCount,
    int[] Order) {
    // The bound arrays are ONE `(Count, 3)` plane each, so every read addresses a row and the `3 * node + axis`
    // arithmetic the substrate supplies never appears by hand.
    internal Span2D<float> Lower => BoundsMin.AsSpan2D(height: Count, width: 3);
    internal Span2D<float> Upper => BoundsMax.AsSpan2D(height: Count, width: 3);

    public BoundingBox Bound(int node) {
        ReadOnlySpan<float> lo = Lower.GetRowSpan(node), hi = Upper.GetRowSpan(node);
        return new(new Point3d(lo[0], lo[1], lo[2]), new Point3d(hi[0], hi[1], hi[2]));
    }

    internal bool Leaf(int node) => LeafCount[node] > 0;
    internal IEnumerable<int> Primitives(int node) =>
        Enumerable.Range(LeafStart[node], LeafCount[node]).Select(slot => Order[slot]);
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
    // Unordered-pair enumeration within ONE index — the clash broad phase over a single model; the dual walk's
    // self filter emits each pair once, so a downstream interference union never re-keys a second registry.
    public sealed record SelfOverlap(double Tolerance) : SpatialQuery;
    // The soup is a TRIANGLE roster, not a flat point array with an implicit 3x stride held together by prose: the
    // arity claim becomes a count comparison, the six stride expressions delete, and `BetaSquared` derives from the
    // magnitude the caller actually states rather than admitting a pre-squared number no band guards.
    public sealed record Winding(Arr<Point3d> Queries, Arr<(Point3d A, Point3d B, Point3d C)> Triangles, PositiveMagnitude Beta) : SpatialQuery;
    // Rank-2 lattice instances ARE the plane slab the slice fold defers to; the rank-3 instance serves voxel sweeps.
    public sealed record Slab(CellLattice Grid, int Layer) : SpatialQuery;

    public QueryKind Kind =>
        Switch(
            range: static _ => QueryKind.Range,
            ray: static _ => QueryKind.Ray,
            nearest: static _ => QueryKind.Nearest,
            overlap: static _ => QueryKind.Overlap,
            selfOverlap: static _ => QueryKind.SelfOverlap,
            winding: static _ => QueryKind.Winding,
            slab: static _ => QueryKind.Slab);
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

    public sealed record Bvh(NodeStore Store, BoundingBox[] Primitives, double BuildCost, BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;
    public sealed record LinearOctree(NodeStore Store, BoundingBox[] Primitives, BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;

    // Positional case synthesis overrides these abstract get/init columns; a same-name-param base Switch suppresses that synthesis and self-recurses.
    public abstract NodeStore Store { get; init; }
    public abstract BoundingBox[] Primitives { get; init; }
    public abstract BuildPolicy Policy { get; init; }
    // `Builder` is DATA on both cases, so a rebuild reads the row that built it rather than a hand-asserted constant
    // one case carried and the other re-stated — the assertion diverges the first time an octree variant lands.
    public abstract SpatialKind Builder { get; init; }

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Store.Count, floor: 1),
        ValidityClaim.CountExactly(count: Store.BoundsMin.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.BoundsMax.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.Order.Length, expected: Primitives.Length),
        TensorPrimitives.IsFiniteAll<float>(Store.BoundsMin) && TensorPrimitives.IsFiniteAll<float>(Store.BoundsMax),
        Links(Store));

    // --- [ADMISSION]
    // Clone detaches the admitted set, so a frozen index never aliases a caller-mutable array.
    internal static Fin<BoundingBox[]> Admit(BoundingBox[] primitives) {
        if (primitives.Length == 0)
            return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, None, "empty"));
        for (int i = 0; i < primitives.Length; i++)
            if (!primitives[i].IsValid)
                return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, i, "non-finite-bound"));
        return Fin.Succ((BoundingBox[])primitives.Clone());
    }

    internal static Point3d[] Centroids(BoundingBox[] boxes) =>
        Array.ConvertAll(boxes, static box => 0.5 * (box.Min + box.Max));

    // --- [BUILD]
    // The build rents ONE pooled scratch per fold — bucket counts, bucket bounds, and the partition buffer — and
    // threads spans into `BestSah` and `StablePartition`, so three arrays per internal node and one per partition
    // call leave the hottest path in the page. A refused arena write lifts onto the `Fin` rail the entry holds.
    internal static Fin<SpatialIndex> BuildBvh(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        int buckets = policy.SahBuckets.Value;
        using Arena arena = Arena.Rent(boxes.Length, depthCeiling: 2);
        using SpanOwner<int> counts = SpanOwner<int>.Allocate(3 * buckets);
        using SpanOwner<BoundingBox> bins = SpanOwner<BoundingBox>.Allocate(3 * buckets);
        using SpanOwner<int> scratch = SpanOwner<int>.Allocate(boxes.Length);
        int[] order = Enumerable.Range(0, boxes.Length).ToArray();
        int next = 1;
        bool Partition(int node, int lo, int hi, int depth) {
            BoundingBox bound = Union(boxes, order, lo, hi);
            int count = hi - lo;
            if (count <= policy.LeafSize.Value || depth >= policy.MaxDepth.Value) {
                return arena.Write(node, bound, 0, 0, lo, count);
            }
            BoundingBox centroidBound = CentroidBound(centroids, order, lo, hi);
            (int axis, double cost, int splitBucket) = BestSah(boxes, centroids, order, lo, hi, bound, centroidBound, buckets, counts.Span, bins.Span);
            if (cost >= count) {
                return arena.Write(node, bound, 0, 0, lo, count);
            }
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            int mid = StablePartition(order, lo, hi, scratch.Span, idx =>
                (int)(buckets * (Axis(centroids[idx], axis) - Axis(centroidBound.Min, axis)) / Math.Max(extent, EpsilonPolicy.ZeroTolerance)) <= splitBucket);
            mid = mid == lo || mid == hi ? (lo + hi) / 2 : mid;
            int firstChild = next;
            next += 2;
            return arena.Write(node, bound, firstChild, 2, -1, 0)
                && Partition(firstChild, lo, mid, depth + 1)
                && Partition(firstChild + 1, mid, hi, depth + 1);
        }
        if (!Partition(0, 0, boxes.Length, 0)) { return Overflowed(); }
        NodeStore store = arena.Freeze(next, order);
        return Fin.Succ((SpatialIndex)new Bvh(store, boxes, AggregateSahCost(store), policy, SpatialKind.Bvh));
    }

    internal static Fin<SpatialIndex> BuildOctree(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        BoundingBox root = Union(boxes);
        (uint[] codes, int[] order) = MortonOrder(boxes.Length, centroids, root);
        // The depth ceiling this kernel honours IS the arena's capacity derivation, so the two cannot drift.
        int ceiling = Math.Min(policy.MaxDepth.Value, MortonDepth);
        using Arena arena = Arena.Rent(boxes.Length, depthCeiling: ceiling);
        int next = 1;
        bool Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
            int count = hi - lo;
            if (count <= policy.LeafSize.Value || depth >= ceiling) {
                return arena.Write(node, Union(boxes, order, lo, hi), 0, 0, lo, count);
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
            if (!arena.Write(node, bound, firstChild, runs.Count, -1, 0)) { return false; }
            for (int c = 0; c < runs.Count; c++)
                if (!Cell(firstChild + c, runs[c].Lo, runs[c].Hi, depth + 1, Union(boxes, order, runs[c].Lo, runs[c].Hi))) { return false; }
            return true;
        }
        if (!Cell(0, 0, boxes.Length, 0, root)) { return Overflowed(); }
        return Fin.Succ((SpatialIndex)new LinearOctree(arena.Freeze(next, order), boxes, policy, SpatialKind.Octree));
    }

    // Agglomerative PLOC as PLOC: one parallel nearest sweep per ROUND, then EVERY mutually-nearest pair merges in
    // one linear pass into a fresh survivor array — O(n x window) per round over O(log n) rounds. The quadratic form
    // re-scanned the whole window per candidate, merged the FIRST mutual pair it found, and paid an O(n) list
    // removal per merge; its `j < 0` fallback additionally merged two arbitrary nodes with no evidence recorded,
    // where a round that merges nothing is now a typed refusal.
    internal static Fin<SpatialIndex> BuildAgglomerative(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        int n = boxes.Length;
        BoundingBox rootBound = Union(boxes);
        (_, int[] order) = MortonOrder(n, centroids, rootBound);
        int capacity = Math.Max(1, (2 * n) - 1);
        int[] childA = new int[capacity];
        int[] childB = new int[capacity];
        BoundingBox[] bound = new BoundingBox[capacity];
        int[] leafSlot = new int[capacity];
        for (int i = 0; i < n; i++) { bound[i] = boxes[order[i]]; leafSlot[i] = i; }
        (int Node, BoundingBox Bound)[] live = [.. Enumerable.Range(0, n).Select(i => (Node: i, Bound: bound[i]))];
        int next = n;
        int window = policy.SahBuckets.Value;
        while (live.Length > 1) {
            int[] nearest = new int[live.Length];
            ParallelHelper.For(0, live.Length, in new NearestSweep(live, window, nearest), policy.ParallelFloor.Value);
            (int Node, BoundingBox Bound)[] survivors = new (int, BoundingBox)[live.Length];
            int kept = 0;
            for (int k = 0; k < live.Length; k++) {
                int partner = nearest[k];
                bool mutual = partner >= 0 && nearest[partner] == k;
                if (mutual && k > partner) { continue; }                                  // the partner emits the merge
                if (!mutual) { survivors[kept++] = live[k]; continue; }
                BoundingBox merged = live[k].Bound; merged.Union(live[partner].Bound);
                int parent = next++;
                (childA[parent], childB[parent], bound[parent], leafSlot[parent]) = (live[k].Node, live[partner].Node, merged, -1);
                survivors[kept++] = (parent, merged);
            }
            if (kept == live.Length) {
                return Fin.Fail<SpatialIndex>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, None, "ploc-no-mutual-pair"));
            }
            live = survivors[..kept];
        }
        NodeStore store = Compact(live[0].Node, next, childA, childB, bound, leafSlot, order);
        return Fin.Succ((SpatialIndex)new Bvh(store, boxes, AggregateSahCost(store), policy, SpatialKind.Agglomerative));
    }

    // The page's own `IAction` idiom: one struct carrying the round's roster, its window, and its destination, so
    // the sweep allocates nothing and the merge pass reads a filled array rather than re-scanning per candidate.
    readonly struct NearestSweep((int Node, BoundingBox Bound)[] live, int window, int[] nearest) : IAction {
        public void Invoke(int i) {
            int best = -1;
            double bestArea = double.MaxValue;
            for (int j = Math.Max(0, i - window); j <= Math.Min(live.Length - 1, i + window); j++) {
                if (j == i) { continue; }
                BoundingBox merged = live[i].Bound;
                merged.Union(live[j].Bound);
                if (merged.Area < bestArea) { (bestArea, best) = (merged.Area, j); }
            }
            nearest[i] = best;
        }
    }

    static Fin<SpatialIndex> Overflowed() =>
        Fin.Fail<SpatialIndex>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, None, "node-capacity"));

    // BFS visit order gives each internal node's children CONSECUTIVE slots, satisfying the contiguous child-range and parent-before-child laws.
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
        // Capacity is EXACT here — the arena is sized by the visited census — so no write can refuse.
        using Arena arena = new(count);
        for (int node = 0; node < count; node++) {
            int old = visit[node];
            _ = leafSlot[old] >= 0
                ? arena.Write(node, bound[old], 0, 0, leafSlot[old], 1)
                : arena.Write(node, bound[old], map[childA[old]], 2, -1, 0);
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

    // AggregateSahCost scores the whole tree root-normalized; BestSah scores one candidate split against its leaf cost — distinct metrics over the same area.
    static double AggregateSahCost(NodeStore store) {
        int count = store.Count;
        using SpanOwner<float> extent = SpanOwner<float>.Allocate(3 * count);
        using SpanOwner<float> area = SpanOwner<float>.Allocate(count);
        using SpanOwner<float> weight = SpanOwner<float>.Allocate(count);
        TensorPrimitives.Subtract<float>(store.BoundsMax.AsSpan(0, 3 * count), store.BoundsMin.AsSpan(0, 3 * count), extent.Span);
        // The extent is a `(count, 3)` plane, so the surface-area fold reads ROWS and the `3 * node + axis`
        // arithmetic never appears in the one member that scores the whole tree.
        Span2D<float> spans = extent.Span.AsSpan2D(height: count, width: 3);
        Span<float> sa = area.Span, w = weight.Span;
        for (int node = 0; node < count; node++) {
            ReadOnlySpan<float> d = spans.GetRowSpan(node);
            sa[node] = 2f * ((d[0] * d[1]) + (d[1] * d[2]) + (d[2] * d[0]));
            w[node] = store.LeafCount[node] > 0 ? store.LeafCount[node] : 0.125f * store.ChildCount[node];
        }
        return TensorPrimitives.Dot<float>(w, sa) / Math.Max(sa[0], (float)EpsilonPolicy.ZeroTolerance);
    }

    // Both scratch spans arrive RENTED from the build fold and are re-sliced per axis, so the three arrays a
    // per-node mint paid for are gone; the two `Enumerable.Range` sequences bought nothing over the loops they
    // replaced and this body sits inside the build kernel the `[03]` Exemption names.
    static (int Axis, double Cost, int Bucket) BestSah(BoundingBox[] boxes, Point3d[] centroids, int[] order, int lo, int hi,
        BoundingBox bound, BoundingBox centroidBound, int buckets, Span<int> counts, Span<BoundingBox> bins) {
        (int bestAxis, double bestCost, int bestBucket) = (0, double.MaxValue, 0);
        for (int axis = 0; axis < 3; axis++) {
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            if (extent <= EpsilonPolicy.ZeroTolerance) { continue; }
            Span<int> lane = counts.Slice(axis * buckets, buckets);
            Span<BoundingBox> boxLane = bins.Slice(axis * buckets, buckets);
            for (int b = 0; b < buckets; b++) { (lane[b], boxLane[b]) = (0, BoundingBox.Empty); }
            for (int i = lo; i < hi; i++) {
                int bucket = Math.Min(buckets - 1, (int)(buckets * (Axis(centroids[order[i]], axis) - Axis(centroidBound.Min, axis)) / extent));
                lane[bucket]++;
                boxLane[bucket].Union(boxes[order[i]]);
            }
            for (int split = 0; split < buckets - 1; split++) {
                (BoundingBox lBox, int lCount) = Accumulate(boxLane, lane, 0, split + 1);
                (BoundingBox rBox, int rCount) = Accumulate(boxLane, lane, split + 1, buckets);
                double cost = 0.125 + (((lCount * lBox.Area) + (rCount * rBox.Area)) / Math.Max(bound.Area, EpsilonPolicy.ZeroTolerance));
                if (cost < bestCost) { (bestAxis, bestCost, bestBucket) = (axis, cost, split); }
            }
        }
        return (bestAxis, bestCost, bestBucket);
    }

    static (BoundingBox Box, int Count) Accumulate(Span<BoundingBox> bins, Span<int> counts, int from, int to) {
        BoundingBox box = BoundingBox.Empty;
        int count = 0;
        for (int b = from; b < to; b++) { box.Union(bins[b]); count += counts[b]; }
        return (box, count);
    }

    // --- [QUERY]
    internal Fin<QueryResult> Query(SpatialQuery probe, Op key) {
        SpatialIndex self = this;
        return probe.Switch(
            state: (Self: self, Key: key),
            range: static (s, q) =>
                guard(q.Box.IsValid && q.Ball.Match(static ball => ball.IsValid, static () => true), s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Hits(RangeHits(s.Self.Store, s.Self.Primitives, q))),
            // The probe UNITIZES at admission, so `MaxT` is a length and the slab's parallel-axis floor compares a
            // direction component against the page's one degeneracy anchor on a scale it actually shares.
            ray: static (s, q) =>
                guard(q.Probe.Direction.Length > 0.0 && double.IsFinite(q.MaxT) && q.MaxT > 0.0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)RayNearest(s.Self.Store, s.Self.Primitives,
                        q with { Probe = new Ray3d(q.Probe.Position, UnitDirection(q.Probe.Direction)) })),
            nearest: static (s, q) =>
                guard(q.K > 0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Nearest(KNearest(s.Self.Store, s.Self.Primitives, q))),
            // The self arm passes its OWN filter rather than letting the pair walk re-derive the modality from store
            // reference identity: a legitimate cross query against the same instance was silently self-filtered.
            overlap: static (s, q) =>
                guard(double.IsFinite(q.Tolerance) && q.Tolerance >= 0.0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Pairs(OverlapPairs(s.Self, q.Other, q.Tolerance, static (_, _) => true))),
            selfOverlap: static (s, q) =>
                guard(double.IsFinite(q.Tolerance) && q.Tolerance >= 0.0, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Pairs(OverlapPairs(s.Self, s.Self, q.Tolerance, static (a, b) => a < b))),
            // A COUNT defect reports as a count fault: the soup's triangle roster and the index's primitive roster
            // are one arity, and a kind mismatch named the wrong axis entirely.
            winding: static (s, q) =>
                q.Triangles.Count != s.Self.Primitives.Length
                    ? Fin.Fail<QueryResult>(new GeometryFault.IndexMismatch(EntityKind.Face, s.Self.Primitives.Length, q.Triangles.Count))
                    : guard(q.Queries.Count > 0, s.Key.InvalidInput()).ToFin()
                        .Map(_ => (QueryResult)new QueryResult.Field(Winding(s.Self.Store, q))),
            slab: static (s, q) =>
                guard(q.Layer >= 0 && q.Layer < q.Grid.Layers.Value, s.Key.InvalidInput()).ToFin()
                    .Map(_ => (QueryResult)new QueryResult.Hits(SlabHits(s.Self.Store, s.Self.Primitives, q))));
    }

    // Plane-slab broad phase: a box crosses layer L iff the box's LOCAL Z range under the lattice inverse affine
    // crosses [L, L+1] — exact for a rotated or sheared lattice, because local Z is affine in the corner and an
    // AABB's extremal value of an affine functional is centre ± Σ|coefficient|·halfExtent, allocation-free.
    static Seq<int> SlabHits(NodeStore store, BoundingBox[] primitives, SpatialQuery.Slab slab) =>
        LeafHits(store: store, verdict: node => CrossesLayer(store.Bound(node), slab.Grid, slab.Layer) ? NodeVerdict.Descend : NodeVerdict.Prune,
            admit: prim => CrossesLayer(primitives[prim], slab.Grid, slab.Layer));

    static bool CrossesLayer(BoundingBox box, CellLattice grid, int layer) {
        Transform w = grid.WorldToIndex;
        Point3d centre = 0.5 * (box.Min + box.Max);
        Vector3d half = 0.5 * (box.Max - box.Min);
        double z = (w.M20 * centre.X) + (w.M21 * centre.Y) + (w.M22 * centre.Z) + w.M23;
        double reach = (Math.Abs(value: w.M20) * half.X) + (Math.Abs(value: w.M21) * half.Y) + (Math.Abs(value: w.M22) * half.Z);
        return z - reach <= layer + 1.0 && z + reach >= layer;
    }

    static Seq<int> RangeHits(NodeStore store, BoundingBox[] primitives, SpatialQuery.Range range) =>
        LeafHits(store: store, verdict: node => Intersects(store.Bound(node), range.Box) ? NodeVerdict.Descend : NodeVerdict.Prune,
            admit: prim => Intersects(primitives[prim], range.Box)
                && range.Ball.Match(ball => SphereHits(primitives[prim], ball), static () => true));

    // Shared monotone shape runs one `Reach` walk, then the admitted leaves' primitives filtered by the caller's
    // own per-primitive test. Both hit queries differ ONLY in those two predicates.
    static Seq<int> LeafHits(NodeStore store, Func<int, NodeVerdict> verdict, Func<int, bool> admit) =>
        NodeWalk.Reach(store: store, verdict: verdict)
            .Filter(store.Leaf)
            .Bind(node => toSeq(store.Primitives(node).Where(admit)));

    // Reverse index order is child-before-parent: one bottom-up moment pass per evaluation feeds every query point.
    static double[] Winding(NodeStore store, SpatialQuery.Winding query) {
        (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(store, query.Triangles);
        double betaSquared = query.Beta.Value * query.Beta.Value;
        return [.. query.Queries.AsIterable().Select(point => WindingAt(store, query, betaSquared, dipole, weighted, area, point))];
    }

    static (Vector3d[] Dipole, Point3d[] Weighted, double[] Area) Moments(NodeStore store, Arr<(Point3d A, Point3d B, Point3d C)> triangles) {
        Vector3d[] dipole = new Vector3d[store.Count];
        Point3d[] weighted = new Point3d[store.Count];
        double[] area = new double[store.Count];
        for (int node = store.Count - 1; node >= 0; node--)
            if (store.Leaf(node))
                foreach (int tri in store.Primitives(node)) {
                    (Point3d a, Point3d b, Point3d c) = triangles[tri];
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

    // Span-kernel arm: the multipole verdict is a function of the QUERY POINT, so the graph `Reach` would colour is a
    // fresh whole-store graph per point — the refused operator is named here, and the same NodeVerdict rows drive
    // this descent, `Absorb` naming exactly the far node whose subtree collapses into one dipole term.
    static double WindingAt(NodeStore store, SpatialQuery.Winding query, double betaSquared, Vector3d[] dipole, Point3d[] weighted, double[] area, Point3d point) =>
        NodeWalk.Descend(root: 0, seed: 0.0, step: (total, node, frontier) => {
            NodeVerdict verdict = Multipole(store, betaSquared, node, point);
            if (verdict.OffersChildren) {
                for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            }
            return total + (verdict.Equals(NodeVerdict.Absorb)
                ? FarField(dipole: dipole[node], weighted: weighted[node], area: area[node], point: point)
                : store.Leaf(node)
                    ? store.Primitives(node).Sum(tri => FourPiInverse * SolidAngle(
                        query.Triangles[tri].A, query.Triangles[tri].B, query.Triangles[tri].C, point))
                    : 0.0);
        });

    static NodeVerdict Multipole(NodeStore store, double betaSquared, int node, Point3d point) {
        if (store.Leaf(node)) { return NodeVerdict.Descend; }
        BoundingBox bound = store.Bound(node);
        double radius = 0.5 * (bound.Max - bound.Min).Length;
        double distance = (0.5 * (bound.Min + bound.Max)).DistanceTo(point);
        return radius > 0.0 && distance * distance > betaSquared * radius * radius ? NodeVerdict.Absorb : NodeVerdict.Descend;
    }

    static double FarField(Vector3d dipole, Point3d weighted, double area, Point3d point) {
        if (area <= EpsilonPolicy.ZeroTolerance) { return 0.0; }
        Vector3d r = (weighted / area) - point;
        double len = r.Length;
        return len > EpsilonPolicy.ZeroTolerance ? FourPiInverse * (dipole * r) / (len * len * len) : 0.0;
    }

    static double SolidAngle(Point3d a, Point3d b, Point3d c, Point3d p) {
        Vector3d ra = a - p, rb = b - p, rc = c - p;
        double la = ra.Length, lb = rb.Length, lc = rc.Length;
        double numerator = ra * Vector3d.CrossProduct(rb, rc);
        double denominator = la * lb * lc + (ra * rb) * lc + (rb * rc) * la + (rc * ra) * lb;
        return 2.0 * Math.Atan2(numerator, denominator);
    }

    // Span-kernel arm: `best` tightens as the walk proceeds, so the admission is not a pure function of the node and
    // no `BreadthFirstSearchAlgorithm` frontier carries it — the refused operator is named here, not elsewhere.
    // The incumbent rides an OPTION through the whole descent — a `-1` seed threaded to the exit and lifted only at
    // the boundary spelled absence as a sentinel every intermediate read had to remember to test.
    static QueryResult.RayHit RayNearest(NodeStore store, BoundingBox[] primitives, SpatialQuery.Ray ray) {
        (double best, Option<int> hit) = NodeWalk.Descend(root: 0, seed: (Best: ray.MaxT, Hit: Option<int>.None), step: (state, node, frontier) => {
            if (!Slab(store.Bound(node), ray.Probe, state.Best, out _)) { return state; }
            if (store.Leaf(node)) {
                foreach (int prim in store.Primitives(node)) {
                    if (Slab(primitives[prim], ray.Probe, state.Best, out double t) && t < state.Best) { state = (t, Some(prim)); }
                }
                return state;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            return state;
        });
        return new QueryResult.RayHit(hit, hit.IsSome ? best : ray.MaxT);
    }

    // Span-kernel arm. Leaf distance is the exact DOUBLE primitive-box distance (0 inside); a centroid metric
    // bounds nothing. QuikGraph carries no event queue and `IQueue<int>` selects a frontier without reading the
    // incumbent worst, so a Dijkstra over a nearest-neighbour heap is a fiction and stands refused. The bounded
    // selection is the `Rasm.Domain` `Ranked` cell under `ExtremumDirection.Minimum` — its `Bound` IS the pruning
    // threshold, an `Option` where the local heap's `Worst()` spelled absence as a `double.MaxValue` sentinel —
    // and `Drain` answers ascending distance, so the page holds no heap and no final sort of its own.
    static Seq<int> KNearest(NodeStore store, BoundingBox[] primitives, SpatialQuery.Nearest knn) {
        Ranked<int, double> nearest = new(knn.K, ExtremumDirection.Minimum);
        _ = NodeWalk.Descend(root: 0, seed: unit, step: (state, node, frontier) => {
            double lower = store.Bound(node).ClosestPoint(knn.Query).DistanceTo(knn.Query);
            if (nearest.Bound.Match(bound => lower > bound, static () => false)) { return state; }
            if (store.Leaf(node)) {
                foreach (int prim in store.Primitives(node)) {
                    nearest.Offer(prim, primitives[prim].ClosestPoint(knn.Query).DistanceTo(knn.Query));
                }
                return state;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            return state;
        });
        return nearest.Drain();
    }

    // THE point-triangle closest refinement behind every BVH candidate prune, Ericson's Voronoi-region ladder: three
    // vertex regions, three edge regions, then the barycentric interior. Foot and distance leave TOGETHER because the
    // distance IS query.DistanceTo(foot) — publishing one alone forced each consumer to re-derive the other, which is
    // exactly how two transcriptions of this ladder came to sit beside two copies of this same broad phase.
    public static (Point3d Foot, double Distance) ClosestOnTriangle(Point3d query, Point3d a, Point3d b, Point3d c) {
        Vector3d ab = b - a, ac = c - a, ap = query - a;
        double d1 = ab * ap, d2 = ac * ap;
        if (d1 <= 0.0 && d2 <= 0.0) { return Foot(query, a); }
        Vector3d bp = query - b;
        double d3 = ab * bp, d4 = ac * bp;
        if (d3 >= 0.0 && d4 <= d3) { return Foot(query, b); }
        double vc = (d1 * d4) - (d3 * d2);
        if (vc <= 0.0 && d1 >= 0.0 && d3 <= 0.0) { return Foot(query, a + ((d1 / (d1 - d3)) * ab)); }
        Vector3d cp = query - c;
        double d5 = ab * cp, d6 = ac * cp;
        if (d6 >= 0.0 && d5 <= d6) { return Foot(query, c); }
        double vb = (d5 * d2) - (d1 * d6);
        if (vb <= 0.0 && d2 >= 0.0 && d6 <= 0.0) { return Foot(query, a + ((d2 / (d2 - d6)) * ac)); }
        double va = (d3 * d6) - (d5 * d4);
        if (va <= 0.0 && (d4 - d3) >= 0.0 && (d5 - d6) >= 0.0) { return Foot(query, b + (((d4 - d3) / ((d4 - d3) + (d5 - d6))) * (c - b))); }
        double denom = 1.0 / (va + vb + vc);
        return Foot(query, a + ((vb * denom) * ab) + ((vc * denom) * ac));

        static (Point3d Foot, double Distance) Foot(Point3d query, Point3d at) => (at, query.DistanceTo(at));
    }

    // Span-kernel arm: the cursor is a node PAIR over two stores, so the vertex set is the |L|x|R| product a
    // `ToDelegateVertexAndEdgeListGraph` would colour whole at `Initialize`. The modality arrives as `admit` from
    // the QUERY case that already carries it — reference identity re-derived a discriminant the union holds, and a
    // legitimate cross query against one instance got a self filter the caller never asked for.
    static Seq<(int Left, int Right)> OverlapPairs(SpatialIndex left, SpatialIndex right, double tolerance, Func<int, int, bool> admit) {
        (NodeStore ls, BoundingBox[] lp) = (left.Store, left.Primitives);
        (NodeStore rs, BoundingBox[] rp) = (right.Store, right.Primitives);
        return NodeWalk.Descend(root: (L: 0, R: 0), seed: Seq<(int, int)>(), step: (pairs, cursor, frontier) => {
            (int l, int r) = cursor;
            if (!Intersects(Inflate(ls.Bound(l), tolerance), rs.Bound(r))) { return pairs; }
            (bool lLeaf, bool rLeaf) = (ls.Leaf(l), rs.Leaf(r));
            if (lLeaf && rLeaf) {
                foreach (int pa in ls.Primitives(l))
                    foreach (int pb in rs.Primitives(r))
                        if (admit(pa, pb) && Intersects(Inflate(lp[pa], tolerance), rp[pb])) { pairs = pairs.Add((pa, pb)); }
                return pairs;
            }
            if (rLeaf || (!lLeaf && ls.Bound(l).Diagonal.Length >= rs.Bound(r).Diagonal.Length)) {
                for (int c = 0; c < ls.ChildCount[l]; c++) { frontier.Push((ls.FirstChild[l] + c, r)); }
            } else {
                for (int c = 0; c < rs.ChildCount[r]; c++) { frontier.Push((l, rs.FirstChild[r] + c)); }
            }
            return pairs;
        });
    }

    // --- [REFIT]
    // Refit is persistent: fresh bound arrays over the shared topology, so a published index is never mutated.
    internal Fin<SpatialIndex> Refit(BoundingBox[] revised) =>
        revised.Length != Primitives.Length
            ? Fin.Fail<SpatialIndex>(new GeometryFault.IndexMismatch(EntityKind.Face, Primitives.Length, revised.Length))
            : Admit(revised).Bind(Rebound);

    Fin<SpatialIndex> Rebound(BoundingBox[] updated) {
        NodeStore store = Store;
        (float[] min, float[] max) = (new float[3 * store.Count], new float[3 * store.Count]);
        LeafRefit leaves = new(store, updated, min, max);
        ParallelHelper.For(0, store.Count, in leaves, Policy.ParallelFloor.Value);
        // Reverse index order is child-before-parent; both planes address by ROW, so the bottom-up union reads a
        // child's three lanes contiguously and no site re-derives an offset.
        Span2D<float> lower = min.AsSpan2D(height: store.Count, width: 3), upper = max.AsSpan2D(height: store.Count, width: 3);
        for (int node = store.Count - 1; node >= 0; node--) {
            if (store.Leaf(node)) { continue; }
            int first = store.FirstChild[node];
            Span<float> lo = lower.GetRowSpan(node), hi = upper.GetRowSpan(node);
            for (int axis = 0; axis < 3; axis++) { (lo[axis], hi[axis]) = (float.MaxValue, float.MinValue); }
            for (int c = 0; c < store.ChildCount[node]; c++) {
                ReadOnlySpan<float> childLo = lower.GetRowSpan(first + c), childHi = upper.GetRowSpan(first + c);
                for (int axis = 0; axis < 3; axis++) {
                    (lo[axis], hi[axis]) = (Math.Min(lo[axis], childLo[axis]), Math.Max(hi[axis], childHi[axis]));
                }
            }
        }
        NodeStore refitted = store with { BoundsMin = min, BoundsMax = max };
        // ONE arm now, because both cases carry the row that built them: a degraded tree rebuilds through its own
        // `Builder` whatever the kernel, and the `Bvh`-only rebuild the missing column forced is gone.
        double cost = AggregateSahCost(refitted);
        return Switch<Fin<SpatialIndex>>(
            bvh: b => cost > b.Policy.RefitDegradationLimit * b.BuildCost
                ? b.Builder.Build(updated, Centroids(updated), b.Policy)
                : Fin.Succ((SpatialIndex)(b with { Primitives = updated, Store = refitted })),
            linearOctree: o => Fin.Succ((SpatialIndex)(o with { Primitives = updated, Store = refitted })));
    }

    readonly struct LeafRefit(NodeStore store, BoundingBox[] boxes, float[] min, float[] max) : IAction {
        public void Invoke(int node) {
            if (!store.Leaf(node)) { return; }
            BoundingBox bound = LeafBound(store, boxes, node);
            Span<float> lo = min.AsSpan2D(height: store.Count, width: 3).GetRowSpan(node);
            Span<float> hi = max.AsSpan2D(height: store.Count, width: 3).GetRowSpan(node);
            (lo[0], lo[1], lo[2]) = (Down(bound.Min.X), Down(bound.Min.Y), Down(bound.Min.Z));
            (hi[0], hi[1], hi[2]) = (Up(bound.Max.X), Up(bound.Max.Y), Up(bound.Max.Z));
        }
    }

    // --- [WIRE]
    // Leaf descriptors pack a TAIL-RELATIVE LeafStart'; bounds copy the store's already-outward-rounded floats, so no second rounding site exists.
    // Framing law: the tuple concatenates Bounds THEN Nodes little-endian — 6*Count float32, then Count + Order.Length int64. Decoders recover the
    // split by bounds.Length / 6, so any harness that flattens the tuple to one stream owes this order and nothing else reconstructs it.
    // Branch golden [NODE_LINK_GOLDEN] over eight BoundingBox primitives — (0,0,0)->(1,1,1), (0.5,0,0)->(1.5,1,1), (2,0,0)->(3,1,1),
    // (2.5,0,0)->(3.5,1,1), (10,0,0)->(11,1,1), (10.5,0,0)->(11.5,1,1), (12,0,0)->(13,1,1), (12.5,0,0)->(13.5,1,1) — built through
    // Spatial.Apply(new SpatialOp.Build(SpatialKind.Bvh, primitives, BuildPolicy.Canonical)) and projected through SpatialOp.Wire:
    //   Store.Count == 3; Order is the identity permutation; Nodes[0] = 2097154, Nodes[1] = -5, Nodes[2] = -8388613; tail [0,1,2,3,4,5,6,7];
    //   stream length 160 bytes (18 float32 = 72, 11 int64 = 88), little-endian, Bounds THEN Nodes:
    //   bounds 010000800100008001000080010058410100803F0100803F010000800100008001000080010060400100803F0100803FFFFF1F410100008001000080010058410100803F0100803F
    //   nodes  0200200000000000FBFFFFFFFFFFFFFFFBFF7FFFFFFFFFFF00000000000000000100000000000000020000000000000003000000000000000400000000000000050000000000000006000000000000000700000000000000
    // Regenerate when the node-link layout, BuildPolicy.Canonical, or the pinned 8-box input changes.
    internal static Fin<(float[] Bounds, long[] Nodes)> NodeLinkProjection(NodeStore store, Op key) {
        for (int node = 0; node < store.Count; node++)
            if (store.LeafCount[node] > BuildPolicy.PackedCountMax || store.ChildCount[node] > BuildPolicy.PackedCountMax)
                return Fin.Fail<(float[] Bounds, long[] Nodes)>(key.InvalidInput());

        int count = store.Count;
        float[] bounds = new float[6 * count];
        long[] nodes = new long[count + store.Order.Length];
        int tail = count;
        Span2D<float> wire = bounds.AsSpan2D(height: count, width: 6);
        for (int node = 0; node < count; node++) {
            Span<float> row = wire.GetRowSpan(node);
            store.Lower.GetRowSpan(node).CopyTo(row[..3]);
            store.Upper.GetRowSpan(node).CopyTo(row[3..]);
            if (store.Leaf(node)) {
                nodes[node] = -(((long)(tail - count) << BuildPolicy.ChildShift) | (uint)store.LeafCount[node]) - 1;
                foreach (int prim in store.Primitives(node)) { nodes[tail++] = prim; }
            } else {
                nodes[node] = ((long)store.FirstChild[node] << BuildPolicy.ChildShift) | (uint)store.ChildCount[node];
            }
        }
        return Fin.Succ((bounds, nodes));
    }

    // Expand10 spreads TEN bits per axis, so a deeper octree walk would read shift bits the code never wrote.
    const int MortonDepth = 10;
    const double FourPiInverse = 0.25 / Math.PI;

    // --- [KERNELS]
    // Bounds narrow OUTWARD at this one write seam; Freeze copies the used prefix and the pooled memory dies with the build.
    sealed class Arena(int capacity) : IDisposable {
        readonly MemoryOwner<float> mins = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<float> maxs = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<int> firstChild = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> childCount = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafStart = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafCount = MemoryOwner<int>.Allocate(capacity);

        // Capacity DERIVES from the depth ceiling the caller's kernel actually honours: a degenerate chain under a
        // ceiling of d costs at most (d + 2)n nodes, so the octree passes min(MaxDepth, MortonDepth) and the binary
        // build passes 2 for its 2n-1 bound. A hand `12x` factor was the octree's ceiling asserted as a literal.
        internal static Arena Rent(int primitives, int depthCeiling) => new(Math.Max(1, ((depthCeiling + 2) * primitives) + 1));

        // A refused write is EVIDENCE the build lifts onto its `Fin` rail, never an unguarded span store past the
        // rented capacity — `BuildOctree` grows its node count by the run census with no ceiling of its own.
        internal bool Write(int node, BoundingBox box, int first, int children, int start, int count) {
            if (node >= capacity) { return false; }
            Span2D<float> lower = mins.Span.AsSpan2D(height: capacity, width: 3), upper = maxs.Span.AsSpan2D(height: capacity, width: 3);
            Span<float> row = lower.GetRowSpan(node), peak = upper.GetRowSpan(node);
            (row[0], row[1], row[2]) = (Down(box.Min.X), Down(box.Min.Y), Down(box.Min.Z));
            (peak[0], peak[1], peak[2]) = (Up(box.Max.X), Up(box.Max.Y), Up(box.Max.Z));
            (firstChild.Span[node], childCount.Span[node], leafStart.Span[node], leafCount.Span[node]) = (first, children, start, count);
            return true;
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

    // Down(v) <= v <= Up(v) for finite double, so a float node bound never falsely prunes and leaf tests re-read the double boxes.
    static float Down(double value) => float.BitDecrement((float)value);
    static float Up(double value) => float.BitIncrement((float)value);

    static bool Links(NodeStore store) {
        for (int node = 0; node < store.Count; node++) {
            bool leaf = store.Leaf(node);
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
        foreach (int prim in store.Primitives(node)) box.Union(boxes[prim]);
        return box;
    }

    // Host Inflate mutates in place; the by-value copy makes this the pure form.
    static BoundingBox Inflate(BoundingBox box, double tolerance) {
        box.Inflate(tolerance);
        return box;
    }

    static double Axis(Point3d p, int axis) => axis == 0 ? p.X : axis == 1 ? p.Y : p.Z;
    static double Axis(Vector3d v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    // Host `Unitize` mutates in place; the by-value copy makes this the pure form, and the admission gate already
    // proved the length positive so the return is total.
    static Vector3d UnitDirection(Vector3d direction) {
        _ = direction.Unitize();
        return direction;
    }

    static bool Intersects(BoundingBox a, BoundingBox b) =>
        a.Min.X <= b.Max.X && a.Max.X >= b.Min.X && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y && a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;

    static bool SphereHits(BoundingBox box, Sphere ball) => box.ClosestPoint(ball.Center).DistanceTo(ball.Center) <= ball.Radius;

    static bool Slab(BoundingBox box, Ray3d ray, double max, out double t) {
        double tMin = 0.0, tMax = max;
        for (int axis = 0; axis < 3; axis++) {
            double origin = Axis(ray.Position, axis), dir = Axis(ray.Direction, axis);
            double lo = Axis(box.Min, axis), hi = Axis(box.Max, axis);
            if (Math.Abs(dir) < EpsilonPolicy.ZeroTolerance) {
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
        span <= EpsilonPolicy.ZeroTolerance ? 0u : (uint)Math.Clamp((int)(1023.0 * (value - min) / span), 0, 1023);

    // The right-side buffer is the build's ONE rented scratch, sliced per call, so a partition no longer mints an
    // array per internal node on the hottest path in the page.
    static int StablePartition(int[] order, int lo, int hi, Span<int> scratch, Func<int, bool> onLeft) {
        Span<int> buffer = scratch[..(hi - lo)];
        int write = lo, b = 0;
        for (int i = lo; i < hi; i++) { if (onLeft(order[i])) { order[write++] = order[i]; } else { buffer[b++] = order[i]; } }
        buffer[..b].CopyTo(order.AsSpan(write, b));
        return write;
    }
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class Spatial {
    [BoundaryAdapter]
    public static Fin<SpatialAnswer> Apply(SpatialOp op, Op? key = null) {
        Op minted = key.OrDefault();
        return op.Switch(
            state: minted,
            build: static (k, b) =>
                from boxes in SpatialIndex.Admit(b.Primitives)
                from built in b.Kind.Build(boxes, SpatialIndex.Centroids(boxes), b.Policy)
                from _ in guard(built.IsValid, k.InvalidResult()).ToFin()
                select (SpatialAnswer)new SpatialAnswer.Index(built),
            refit: static (k, r) =>
                from refitted in r.Index.Refit(r.Updated)
                from _ in guard(refitted.IsValid, k.InvalidResult()).ToFin()
                select (SpatialAnswer)new SpatialAnswer.Index(refitted),
            query: static (k, q) => q.Index.Query(q.Probe, k).Map(static r => (SpatialAnswer)new SpatialAnswer.Result(r)),
            wire: static (k, w) => SpatialIndex.NodeLinkProjection(w.Index.Store, k)
                .Map(static t => (SpatialAnswer)new SpatialAnswer.Wire(t.Bounds, t.Nodes)));
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
