# [RASM_SPATIAL_INDEX]

`SpatialIndex` owns every broad-phase modality as a typed operation — static `Build`, instance `Refit`, one input-shape-dispatched `Query`, and `Wire` — each landing its own `Fin` result over one sealed `SpatialIndex` whose `SpatialKind` kernels differ only in build partition strategy and share one frozen `NodeStore`, so query, refit, and wire read that store kernel-agnostically. This owner serves predicate-exact primitive-bounds broad phase alone.

`SpatialIndex` computes on raw primitive coordinates, never a unit-bearing quantity type. `NodeWalk` is this page's ONE hierarchy traversal owner and every query composes it: the monotone arm projects the child links onto a QuikGraph delegate graph and reads one breadth-first walk, while the state-threading arm is the named span kernel the four walks whose graph is not fixed per walk share. `NodeVerdict` is the one pruning vocabulary both arms read. `BuildPolicy` and every scalar it carries admit through the `Numerics/atoms` `Band` rows, so an inadmissible policy is unrepresentable and no consumer re-gates one. `Wire` is the one cross-package egress and carries raw arrays alone, so `Rasm.Compute` decodes with no Compute type entering this owner; `Rasm.Persistence` content-addresses the frozen `NodeStore` itself, and this owner mints no second store.

## [01]-[INDEX]

- [02]-[NODE_WALK]: `NodeWalk` owns every hierarchy descent, the monotone arm on QuikGraph and the state-threading arm named.
- [03]-[SPATIAL_INDEX]: `SpatialIndex` lands every broad-phase op as a typed operation over the shared node store.

## [02]-[NODE_WALK]

- Owner: `NodeWalk` is the page's ONE traversal owner over a `NodeStore`; `NodeVerdict` rows are the per-node decision every query answers — `Prune` withholds the node and its whole subtree, `Absorb` visits the node and withholds its children, `Descend` visits and admits them. Both arms read the same three rows: `Reach` hands them to the graph container, `Descend` reads them off fold state.
- Law: subtree pruning is exact BECAUSE child bounds nest inside parent bounds — a verdict that is a pure function of the node is MONOTONE, so "reachable under the verdict" and "visited by a pruning descent" are the same node set and `Reach` may hand the decision to a graph container. Monotone is a property of the verdict over ONE walk, not over a query batch: a verdict parameterized by a per-item probe is monotone per item and re-roots a fresh graph per item, which is the cost the state-threading arm exists to refuse.
- Auto: `Reach` projects the store's child links onto `GraphExtensions.ToDelegateVertexAndEdgeListGraph` — the delegate withholds every child of a `Prune` or `Absorb` node, so the filter IS the adjacency and no second container materializes — then runs one `BreadthFirstSearchAlgorithm` under an `EdgeRecorderObserver`, whose `Edges` in visit order are the discovered nodes. Verdict memo answers each node once for the two questions the walk asks of it (its own children, and its parent's child filter), so a predicate never runs twice per walk.
- Exemption: `Descend` keeps ONE `Stack<TCursor>` frontier and is the named span kernel; `Reach`'s `NodeVerdict?[] seen` memo is build-kernel state for exactly one walk and never leaves it. `Descend` exists because four walks refuse `BreadthFirstSearchAlgorithm` and `DepthFirstSearchAlgorithm` alike: the ray and k-nearest walks tighten their admission bound from traversal state, so the filtered graph is not a fixed graph and no `IQueue<int>` carries the domain incumbent; the winding walk's verdict is a function of the QUERY POINT, so a graph arm builds and colours one whole-store graph per point where the descent visits only the nodes the multipole criterion admits; the dual walk's cursor is a node PAIR over two stores, and both catalogued lazy containers demand a surface the product has not got — `ToDelegateVertexAndEdgeListGraph` takes an `IEnumerable<TVertex>` both searches colour whole at `Initialize`, which materializes the |L|x|R| product, and `ToDelegateBidirectionalIncidenceGraph` demands in-edges a descent never reads.
- Growth: a new monotone query is one verdict function through `Reach`; a new state-threading query is one `step` through `Descend`; neither mints a frontier.
- Packages: QuikGraph (`GraphExtensions.ToDelegateVertexAndEdgeListGraph`, `BreadthFirstSearchAlgorithm`, `EdgeRecorderObserver`, `SEdge`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class NodeVerdict {
    internal static readonly NodeVerdict Prune = new(offersChildren: false, visits: false);
    internal static readonly NodeVerdict Absorb = new(offersChildren: false, visits: true);
    internal static readonly NodeVerdict Descend = new(offersChildren: true, visits: true);

    internal bool OffersChildren { get; }
    internal bool Visits { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NodeWalk {
    internal static Seq<int> Reach(NodeStore store, Func<int, NodeVerdict> verdict) {
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

- Owner: `SpatialKind` rows own kernel selection, each row carrying its own builder over the shared `NodeStore` and each built index carrying the row that built it beside an optional SAH baseline only the BVH-shaped kernels mint, so a refit rebuilds through its own kernel when that baseline degrades; `SpatialIndex.ClosestOnTriangle` is the one point-triangle refinement every consumer of this page's candidate prune reads, foot and distance leaving together.
- Output: each `Query` overload answers its own typed result — a hit `Seq<int>`, a ray `(Option<int> Id, double T)`, an overlap-pair `Seq`, or a winding `double[]` — and the index itself is the registered validity evidence.
- Law: `BuildPolicy.Of` is the ONE admission — every scalar enters through a `Numerics/atoms` owner, so `Band.Count`'s closed floor of one is the authority that makes a zero leaf size, depth, bucket count, or parallel floor unrepresentable; the five independent admissions accumulate through the tuple `Apply` before the packed-count and bucket-floor relation sequences on the admitted values. `Canonical`'s figures each carry their provenance on site, because a band authorizes a RANGE and never a value. The packed-field ceiling DERIVES from `ChildShift`, both seated on `BuildPolicy` so the wire layout has ONE authority the projection reads, and `RefitGrowth` carries the degradation limit as a positive FRACTION above unity so a limit at or below one is unrepresentable rather than guarded. Far-field cut and every degeneracy floor on this page read `EpsilonPolicy.ZeroTolerance`, the branch's one degeneracy anchor — the ray slab included, its probe unitized at admission so the comparison shares that anchor's scale — so no page literal states either.
- Output: the box and slab `Query` arms publish LEVEL order — the monotone walk's own discovery order, the owner's published order for a hit set (`DIGEST_OVER_UNORDERED_CONTAINER`); the k-nearest arm publishes ascending distance, the overlap arms the dual descent's own order, and the winding arm the query-point order the caller supplied.
- Exemption: the build kernels keep mutable accumulators for exactly one build — the BVH's `(Node, Lo, Hi, Depth)` range frontier, the octree's `List<(int Lo, int Hi)> runs` per cell, the agglomerative round's `nearest` and survivor arrays, the pooled `Arena`, bucket, bin, and partition spans, and the `Compact` visit/map arrays — and every one dies at `Freeze`. The BVH partition is an explicit range stack, never a recursive local function or a `NodeWalk.Descend` step, because its bucket, bin, and partition scratch are `SpanOwner<T>` rentals — a `ref struct` no local function or lambda may capture (`CS8175`) — so the split loop reads `.Span` off the rentals in the frame that owns them, and the octree's `Cell` stays recursive because it captures the class-typed `Arena` and arrays alone; k-nearest selection composes the `Rasm.Domain` `Ranked` cell under `ExtremumDirection.Minimum`, whose `Bound` is the walk's pruning threshold, so the page holds no `PriorityQueue` of its own.
- Packages: RhinoCommon through `Rasm.Numerics`, `Rasm.Domain`, QuikGraph, CommunityToolkit.HighPerformance (`SpanOwner`/`MemoryOwner` the pooled arenas, `Span2D<T>.GetRowSpan` every strided plane read, `ParallelHelper.For` the PLOC sweep and the leaf refit), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new kernel is one `SpatialKind` row over the shared `NodeStore`, carried as the index's `Kind`, a new query one `Query` overload discriminating on its input shape, a new op one typed member on `SpatialIndex`, a new knob one `BuildPolicy` column; a new node layout is one `NodeStore` shape change, admitted only by charter amendment.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new(SpatialIndex.BuildBvh);
    public static readonly SpatialKind Octree = new(SpatialIndex.BuildOctree);
    public static readonly SpatialKind Agglomerative = new(SpatialIndex.BuildAgglomerative);

    [UseDelegateFromConstructor]
    internal partial Fin<SpatialIndex> Build(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BuildPolicy {
    private BuildPolicy(Dimension leafSize, Dimension maxDepth, Dimension sahBuckets, PositiveMagnitude refitGrowth, Dimension parallelFloor) =>
        (LeafSize, MaxDepth, SahBuckets, RefitGrowth, ParallelFloor) = (leafSize, maxDepth, sahBuckets, refitGrowth, parallelFloor);

    public const int ChildShift = 21;
    public const int PackedCountMax = (1 << ChildShift) - 1;

    public Dimension LeafSize { get; }
    public Dimension MaxDepth { get; }
    public Dimension SahBuckets { get; }
    public PositiveMagnitude RefitGrowth { get; }
    public Dimension ParallelFloor { get; }

    public static Fin<BuildPolicy> Of(int leafSize, int maxDepth, int sahBuckets, double refitGrowth, int parallelFloor) {
        return (FactoryBridge.Accept<Dimension>(leafSize).ToValidation(),
                FactoryBridge.Accept<Dimension>(maxDepth).ToValidation(),
                FactoryBridge.Accept<Dimension>(sahBuckets).ToValidation(),
                FactoryBridge.Accept<PositiveMagnitude>(refitGrowth).ToValidation(),
                FactoryBridge.Accept<Dimension>(parallelFloor).ToValidation())
            .Apply((leaf, depth, buckets, growth, floor) => new BuildPolicy(leaf, depth, buckets, growth, floor)).As().ToFin()
            .Bind(policy => guard(policy.LeafSize.Value <= PackedCountMax && policy.SahBuckets.Value > 1, new KernelFault.InvalidInput()).ToFin()
                .Map(_ => policy));
    }

    public static readonly BuildPolicy Canonical = new(
        leafSize: Dimension.Create(value: 4), maxDepth: Dimension.Create(value: 32), sahBuckets: Dimension.Create(value: 12),
        refitGrowth: PositiveMagnitude.Create(value: 0.6), parallelFloor: Dimension.Create(value: 4096));
}

public sealed record NodeStore(
    float[] BoundsMin,
    float[] BoundsMax,
    int[] FirstChild,
    int[] ChildCount,
    int[] LeafStart,
    int[] LeafCount,
    int[] Order) {
    public int Count => FirstChild.Length;

    internal Span2D<float> Lower => BoundsMin.AsSpan2D(height: Count, width: 3);
    internal Span2D<float> Upper => BoundsMax.AsSpan2D(height: Count, width: 3);

    internal BoundingBox Bound(int node) {
        ReadOnlySpan<float> lo = Lower.GetRowSpan(node), hi = Upper.GetRowSpan(node);
        return new(new Point3d(lo[0], lo[1], lo[2]), new Point3d(hi[0], hi[1], hi[2]));
    }

    internal bool Leaf(int node) => LeafCount[node] > 0;
    internal IEnumerable<int> Primitives(int node) =>
        Enumerable.Range(LeafStart[node], LeafCount[node]).Select(slot => Order[slot]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class SpatialIndex : IValidityEvidence {
    private SpatialIndex(NodeStore store, BoundingBox[] primitives, BuildPolicy policy, SpatialKind kind, Option<double> baseline) =>
        (Store, Primitives, Policy, Kind, Baseline) = (store, primitives, policy, kind, baseline);

    public NodeStore Store { get; }
    BoundingBox[] Primitives { get; }
    BuildPolicy Policy { get; }
    SpatialKind Kind { get; }
    Option<double> Baseline { get; }

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Store.Count, floor: 1),
        ValidityClaim.CountExactly(count: Store.BoundsMin.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.BoundsMax.Length, expected: 3 * Store.Count),
        ValidityClaim.CountExactly(count: Store.ChildCount.Length, expected: Store.Count),
        ValidityClaim.CountExactly(count: Store.LeafStart.Length, expected: Store.Count),
        ValidityClaim.CountExactly(count: Store.LeafCount.Length, expected: Store.Count),
        ValidityClaim.CountExactly(count: Store.Order.Length, expected: Primitives.Length),
        Store.Order.All(primitive => (uint)primitive < (uint)Primitives.Length)
            && Store.Order.Distinct().Count() == Primitives.Length,
        TensorPrimitives.IsFiniteAll<float>(Store.BoundsMin) && TensorPrimitives.IsFiniteAll<float>(Store.BoundsMax),
        Links(Store));

    // --- [ADMISSION]
    internal static Fin<BoundingBox[]> Admit(BoundingBox[] primitives) {
        if (primitives.Length == 0)
            return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, None, "empty"));
        for (int i = 0; i < primitives.Length; i++)
            if (!primitives[i].IsValid)
                return Fin.Fail<BoundingBox[]>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.BoundingBox, i, "non-finite-bound"));
        return Fin.Succ((BoundingBox[])primitives.Clone());
    }

    internal static Point3d[] Centroids(BoundingBox[] boxes) =>
        System.Array.ConvertAll(boxes, static box => 0.5 * (box.Min + box.Max));

    // --- [BUILD]
    public static Fin<SpatialIndex> Build(SpatialKind kind, BoundingBox[] primitives, BuildPolicy policy) {
        return from boxes in Admit(primitives)
               from built in kind.Build(boxes, Centroids(boxes), policy)
               from _ in guard(built.IsValid, new KernelFault.InvalidResult())
               select built;
    }

    internal static Fin<SpatialIndex> BuildBvh(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        int buckets = policy.SahBuckets.Value;
        using Arena arena = Arena.Rent(boxes.Length, depthCeiling: 2);
        using SpanOwner<int> counts = SpanOwner<int>.Allocate(3 * buckets);
        using SpanOwner<BoundingBox> bins = SpanOwner<BoundingBox>.Allocate(3 * buckets);
        using SpanOwner<int> scratch = SpanOwner<int>.Allocate(boxes.Length);
        int[] order = Enumerable.Range(0, boxes.Length).ToArray();
        int next = 1;
        Stack<(int Node, int Lo, int Hi, int Depth)> frontier = new();
        frontier.Push((0, 0, boxes.Length, 0));
        while (frontier.Count > 0) {
            (int node, int lo, int hi, int depth) = frontier.Pop();
            BoundingBox bound = Union(boxes, order, lo, hi);
            int count = hi - lo;
            if (count <= policy.LeafSize.Value || depth >= policy.MaxDepth.Value) {
                if (!arena.Write(node, bound, 0, 0, lo, count)) { return Overflowed(); }
                continue;
            }
            BoundingBox centroidBound = BoundingBox.Empty;
            for (int i = lo; i < hi; i++) { centroidBound.Union(centroids[order[i]]); }
            (int axis, double cost, int splitBucket) = BestSah(boxes, centroids, order, lo, hi, bound, centroidBound, buckets, counts.Span, bins.Span);
            if (cost >= count) {
                if (!arena.Write(node, bound, 0, 0, lo, count)) { return Overflowed(); }
                continue;
            }
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            Span<int> partition = scratch.Span[..count];
            int mid = lo, right = 0;
            for (int i = lo; i < hi; i++) {
                int primitive = order[i];
                int bucket = (int)(buckets * (Axis(centroids[primitive], axis) - Axis(centroidBound.Min, axis)) / Math.Max(extent, EpsilonPolicy.ZeroTolerance));
                if (bucket <= splitBucket) { order[mid++] = primitive; }
                else { partition[right++] = primitive; }
            }
            partition[..right].CopyTo(order.AsSpan(mid, right));
            mid = mid == lo || mid == hi ? (lo + hi) / 2 : mid;
            int firstChild = next;
            next += 2;
            if (!arena.Write(node, bound, firstChild, 2, -1, 0)) { return Overflowed(); }
            frontier.Push((firstChild + 1, mid, hi, depth + 1));
            frontier.Push((firstChild, lo, mid, depth + 1));
        }
        NodeStore store = arena.Freeze(next, order);
        return Fin.Succ(new SpatialIndex(store, boxes, policy, SpatialKind.Bvh, Some(AggregateSahCost(store))));
    }

    internal static Fin<SpatialIndex> BuildOctree(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        BoundingBox root = Union(boxes);
        (uint[] codes, int[] order) = MortonOrder(boxes.Length, centroids, root);
        int ceiling = Math.Min(policy.MaxDepth.Value, MortonDepth);
        using Arena arena = Arena.Rent(boxes.Length, depthCeiling: ceiling);
        int next = 1;
        bool Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
            int count = hi - lo;
            if (count <= policy.LeafSize.Value || depth >= ceiling) {
                return arena.Write(node, bound, 0, 0, lo, count);
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
        return Fin.Succ(new SpatialIndex(arena.Freeze(next, order), boxes, policy, SpatialKind.Octree, None));
    }

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
                if (mutual && k > partner) { continue; }
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
        return Fin.Succ(new SpatialIndex(store, boxes, policy, SpatialKind.Agglomerative, Some(AggregateSahCost(store))));
    }

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
            _ = leafSlot[old] >= 0
                ? arena.Write(node, bound[old], 0, 0, leafSlot[old], 1)
                : arena.Write(node, bound[old], map[childA[old]], 2, -1, 0);
        }
        return arena.Freeze(count, order);
    }

    static (uint[] Codes, int[] Order) MortonOrder(int count, Point3d[] centroids, BoundingBox root) {
        Vector3d span = root.Max - root.Min;
        uint[] codes = System.Array.ConvertAll(centroids, c =>
            Expand10(Normalize(c.X, root.Min.X, span.X))
            | (Expand10(Normalize(c.Y, root.Min.Y, span.Y)) << 1)
            | (Expand10(Normalize(c.Z, root.Min.Z, span.Z)) << 2));
        int[] order = Enumerable.Range(0, count).ToArray();
        System.Array.Sort(codes, order);
        return (codes, order);
    }

    static double AggregateSahCost(NodeStore store) {
        int count = store.Count;
        using SpanOwner<float> extent = SpanOwner<float>.Allocate(3 * count);
        using SpanOwner<float> area = SpanOwner<float>.Allocate(count);
        using SpanOwner<float> weight = SpanOwner<float>.Allocate(count);
        TensorPrimitives.Subtract<float>(store.BoundsMax.AsSpan(0, 3 * count), store.BoundsMin.AsSpan(0, 3 * count), extent.Span);
        Span2D<float> spans = extent.Span.AsSpan2D(height: count, width: 3);
        Span<float> sa = area.Span, w = weight.Span;
        for (int node = 0; node < count; node++) {
            ReadOnlySpan<float> d = spans.GetRowSpan(node);
            sa[node] = 2f * ((d[0] * d[1]) + (d[1] * d[2]) + (d[2] * d[0]));
            w[node] = store.LeafCount[node] > 0 ? store.LeafCount[node] : 0.125f * store.ChildCount[node];
        }
        return TensorPrimitives.Dot<float>(w, sa) / Math.Max(sa[0], (float)EpsilonPolicy.ZeroTolerance);
    }

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
    public Fin<Seq<int>> Query(BoundingBox box, Option<Sphere> ball = default) {
        return guard(box.IsValid && ball.Match(static sphere => sphere.IsValid, static () => true), new KernelFault.InvalidInput()).ToFin()
            .Map(_ => LeafHits(Store, node => BoundingBox.Intersection(Store.Bound(node), box).IsValid ? NodeVerdict.Descend : NodeVerdict.Prune,
                primitive => BoundingBox.Intersection(Primitives[primitive], box).IsValid
                    && ball.Match(sphere => Primitives[primitive].ClosestPoint(sphere.Center).DistanceTo(sphere.Center) <= sphere.Radius, static () => true)));
    }

    public Fin<(Option<int> Id, double T)> Query(Ray3d ray, double maxT) {
        Vector3d direction = ray.Direction;
        return guard(ray.Position.IsValid && direction.IsValid && direction.Unitize() && double.IsFinite(maxT) && maxT > 0.0, new KernelFault.InvalidInput()).ToFin()
            .Map(_ => RayNearest(Store, Primitives, new Ray3d(ray.Position, direction), maxT));
    }

    public Fin<Seq<int>> Query(Point3d point, int count) {
        return guard(point.IsValid && count > 0, new KernelFault.InvalidInput()).ToFin()
            .Map(_ => KNearest(Store, Primitives, point, count));
    }

    public Fin<Seq<(int Left, int Right)>> Query(SpatialIndex other, double tolerance) {
        return guard(double.IsFinite(tolerance) && tolerance >= 0.0, new KernelFault.InvalidInput()).ToFin()
            .Map(_ => OverlapPairs(this, other, tolerance, static (_, _) => true));
    }

    public Fin<Seq<(int Left, int Right)>> Query(double tolerance) {
        return guard(double.IsFinite(tolerance) && tolerance >= 0.0, new KernelFault.InvalidInput()).ToFin()
            .Map(_ => OverlapPairs(this, this, tolerance, static (left, right) => left < right));
    }

    public Fin<double[]> Query(Arr<Point3d> points, Arr<(Point3d A, Point3d B, Point3d C)> triangles, PositiveMagnitude betaSquared) {
        return triangles.Count != Primitives.Length
            ? Fin.Fail<double[]>(new GeometryFault.PrimitiveCountMismatch(Primitives.Length, triangles.Count))
            : guard(points.Count > 0, new KernelFault.InvalidInput()).ToFin().Map<double[]>(_ => {
                (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(Store, triangles);
                return [.. points.AsIterable().Select(point => WindingAt(Store, triangles, betaSquared.Value, dipole, weighted, area, point))];
            });
    }

    public Fin<Seq<int>> Query(CellLattice grid, int layer) {
        return guard(layer >= 0 && layer < grid.Layers.Value, new KernelFault.InvalidInput()).ToFin()
            .Map(_ => LeafHits(Store, node => CrossesLayer(Store.Bound(node), grid, layer) ? NodeVerdict.Descend : NodeVerdict.Prune,
                primitive => CrossesLayer(Primitives[primitive], grid, layer)));
    }

    static bool CrossesLayer(BoundingBox box, CellLattice grid, int layer) {
        Transform w = grid.WorldToIndex;
        Point3d centre = 0.5 * (box.Min + box.Max);
        Vector3d half = 0.5 * (box.Max - box.Min);
        double z = (w.M20 * centre.X) + (w.M21 * centre.Y) + (w.M22 * centre.Z) + w.M23;
        double reach = (Math.Abs(value: w.M20) * half.X) + (Math.Abs(value: w.M21) * half.Y) + (Math.Abs(value: w.M22) * half.Z);
        return z - reach <= layer + 1.0 && z + reach >= layer;
    }

    static Seq<int> LeafHits(NodeStore store, Func<int, NodeVerdict> verdict, Func<int, bool> admit) =>
        NodeWalk.Reach(store: store, verdict: verdict)
            .Filter(store.Leaf)
            .Bind(node => toSeq(store.Primitives(node).Where(admit)));

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

    static double WindingAt(NodeStore store, Arr<(Point3d A, Point3d B, Point3d C)> triangles, double betaSquared, Vector3d[] dipole, Point3d[] weighted, double[] area, Point3d point) =>
        NodeWalk.Descend(root: 0, seed: 0.0, step: (total, node, frontier) => {
            NodeVerdict verdict = Multipole(store, betaSquared, node, point);
            if (verdict.OffersChildren) {
                for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            }
            return total + (verdict == NodeVerdict.Absorb
                ? FarField(dipole: dipole[node], weighted: weighted[node], area: area[node], point: point)
                : store.Leaf(node)
                    ? store.Primitives(node).Sum(tri => FourPiInverse * SolidAngle(
                        triangles[tri].A, triangles[tri].B, triangles[tri].C, point))
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

    static (Option<int> Id, double T) RayNearest(NodeStore store, BoundingBox[] primitives, Ray3d ray, double maxT) {
        (double best, Option<int> hit) = NodeWalk.Descend(root: 0, seed: (Best: maxT, Hit: Option<int>.None), step: (state, node, frontier) => {
            if (!Slab(store.Bound(node), ray, state.Best, out _)) { return state; }
            if (store.Leaf(node)) {
                foreach (int prim in store.Primitives(node)) {
                    if (Slab(primitives[prim], ray, state.Best, out double t) && t < state.Best) { state = (t, Some(prim)); }
                }
                return state;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            return state;
        });
        return (hit, hit.IsSome ? best : maxT);
    }

    static Seq<int> KNearest(NodeStore store, BoundingBox[] primitives, Point3d point, int count) {
        Ranked<int, double> nearest = new(count, ExtremumDirection.Minimum);
        _ = NodeWalk.Descend(root: 0, seed: unit, step: (state, node, frontier) => {
            double lower = store.Bound(node).ClosestPoint(point).DistanceTo(point);
            if (nearest.Bound.Match(bound => lower > bound, static () => false)) { return state; }
            if (store.Leaf(node)) {
                foreach (int prim in store.Primitives(node)) {
                    nearest.Offer(prim, primitives[prim].ClosestPoint(point).DistanceTo(point));
                }
                return state;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) { frontier.Push(store.FirstChild[node] + c); }
            return state;
        });
        return nearest.Drain();
    }

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

    static Seq<(int Left, int Right)> OverlapPairs(SpatialIndex left, SpatialIndex right, double tolerance, Func<int, int, bool> admit) {
        (NodeStore ls, BoundingBox[] lp) = (left.Store, left.Primitives);
        (NodeStore rs, BoundingBox[] rp) = (right.Store, right.Primitives);
        return NodeWalk.Descend(root: (L: 0, R: 0), seed: Seq<(int, int)>(), step: (pairs, cursor, frontier) => {
            (int l, int r) = cursor;
            if (!BoundingBox.Intersection(Inflate(ls.Bound(l), tolerance), rs.Bound(r)).IsValid) { return pairs; }
            (bool lLeaf, bool rLeaf) = (ls.Leaf(l), rs.Leaf(r));
            if (lLeaf && rLeaf) {
                foreach (int pa in ls.Primitives(l))
                    foreach (int pb in rs.Primitives(r))
                        if (admit(pa, pb) && BoundingBox.Intersection(Inflate(lp[pa], tolerance), rp[pb]).IsValid) { pairs = pairs.Add((pa, pb)); }
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
    public Fin<SpatialIndex> Refit(BoundingBox[] revised) {
        Fin<SpatialIndex> result = revised.Length != Primitives.Length
            ? Fin.Fail<SpatialIndex>(new GeometryFault.PrimitiveCountMismatch(Primitives.Length, revised.Length))
            : Admit(revised).Bind(Rebound);
        return result.Bind(index => guard(index.IsValid, new KernelFault.InvalidResult()).ToFin().Map(_ => index));
    }

    Fin<SpatialIndex> Rebound(BoundingBox[] updated) {
        NodeStore store = Store;
        (float[] min, float[] max) = (new float[3 * store.Count], new float[3 * store.Count]);
        LeafRefit leaves = new(store, updated, min, max);
        ParallelHelper.For(0, store.Count, in leaves, Policy.ParallelFloor.Value);
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
        return Baseline.Match(
            Some: prior => AggregateSahCost(refitted) > (1.0 + Policy.RefitGrowth.Value) * prior
                ? Kind.Build(updated, Centroids(updated), Policy)
                : Fin.Succ(new SpatialIndex(refitted, updated, Policy, Kind, Baseline)),
            None: () => Fin.Succ(new SpatialIndex(refitted, updated, Policy, Kind, None)));
    }

    readonly struct LeafRefit(NodeStore store, BoundingBox[] boxes, float[] min, float[] max) : IAction {
        public void Invoke(int node) {
            if (!store.Leaf(node)) { return; }
            BoundingBox bound = BoundingBox.Empty;
            foreach (int primitive in store.Primitives(node)) { bound.Union(boxes[primitive]); }
            Span<float> lo = min.AsSpan2D(height: store.Count, width: 3).GetRowSpan(node);
            Span<float> hi = max.AsSpan2D(height: store.Count, width: 3).GetRowSpan(node);
            (lo[0], lo[1], lo[2]) = (Down(bound.Min.X), Down(bound.Min.Y), Down(bound.Min.Z));
            (hi[0], hi[1], hi[2]) = (Up(bound.Max.X), Up(bound.Max.Y), Up(bound.Max.Z));
        }
    }

    // --- [WIRE]
    public Fin<(float[] Bounds, long[] Nodes)> Wire() {
        for (int node = 0; node < Store.Count; node++)
            if (Store.LeafCount[node] > BuildPolicy.PackedCountMax || Store.ChildCount[node] > BuildPolicy.PackedCountMax)
                return Fin.Fail<(float[] Bounds, long[] Nodes)>(new KernelFault.InvalidInput());

        int count = Store.Count;
        float[] bounds = new float[6 * count];
        long[] nodes = new long[count + Store.Order.Length];
        int tail = count;
        Span2D<float> wire = bounds.AsSpan2D(height: count, width: 6);
        for (int node = 0; node < count; node++) {
            Span<float> row = wire.GetRowSpan(node);
            Store.Lower.GetRowSpan(node).CopyTo(row[..3]);
            Store.Upper.GetRowSpan(node).CopyTo(row[3..]);
            if (Store.Leaf(node)) {
                nodes[node] = -(((long)(tail - count) << BuildPolicy.ChildShift) | (uint)Store.LeafCount[node]) - 1;
                foreach (int prim in Store.Primitives(node)) { nodes[tail++] = prim; }
            } else {
                nodes[node] = ((long)Store.FirstChild[node] << BuildPolicy.ChildShift) | (uint)Store.ChildCount[node];
            }
        }
        return Fin.Succ((bounds, nodes));
    }

    const int MortonDepth = 10;
    const double FourPiInverse = 0.25 / Math.PI;

    // --- [KERNELS]
    sealed class Arena(int capacity) : IDisposable {
        readonly MemoryOwner<float> mins = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<float> maxs = MemoryOwner<float>.Allocate(3 * capacity);
        readonly MemoryOwner<int> firstChild = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> childCount = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafStart = MemoryOwner<int>.Allocate(capacity);
        readonly MemoryOwner<int> leafCount = MemoryOwner<int>.Allocate(capacity);

        internal static Arena Rent(int primitives, int depthCeiling) => new(Math.Max(1, ((depthCeiling + 2) * primitives) + 1));

        internal bool Write(int node, BoundingBox box, int first, int children, int start, int count) {
            if (node >= capacity) { return false; }
            Span2D<float> lower = mins.Span.AsSpan2D(height: capacity, width: 3), upper = maxs.Span.AsSpan2D(height: capacity, width: 3);
            Span<float> row = lower.GetRowSpan(node), peak = upper.GetRowSpan(node);
            (row[0], row[1], row[2]) = (Down(box.Min.X), Down(box.Min.Y), Down(box.Min.Z));
            (peak[0], peak[1], peak[2]) = (Up(box.Max.X), Up(box.Max.Y), Up(box.Max.Z));
            (firstChild.Span[node], childCount.Span[node], leafStart.Span[node], leafCount.Span[node]) = (first, children, start, count);
            return true;
        }

        internal NodeStore Freeze(int count, int[] order) => new(
            mins.Span[..(3 * count)].ToArray(), maxs.Span[..(3 * count)].ToArray(),
            firstChild.Span[..count].ToArray(), childCount.Span[..count].ToArray(),
            leafStart.Span[..count].ToArray(), leafCount.Span[..count].ToArray(), order);

        public void Dispose() {
            mins.Dispose(); maxs.Dispose(); firstChild.Dispose();
            childCount.Dispose(); leafStart.Dispose(); leafCount.Dispose();
        }
    }

    static float Down(double value) => float.BitDecrement((float)value);
    static float Up(double value) => float.BitIncrement((float)value);

    static bool Links(NodeStore store) {
        if (store.ChildCount.Length != store.Count || store.LeafStart.Length != store.Count
            || store.LeafCount.Length != store.Count) { return false; }
        for (int node = 0; node < store.Count; node++) {
            bool leaf = store.LeafCount[node] > 0;
            if (leaf
                    ? store.ChildCount[node] != 0 || store.FirstChild[node] != 0
                    : store.ChildCount[node] == 0 || store.LeafStart[node] != -1) { return false; }
            if (leaf && (store.LeafStart[node] < 0
                    || store.LeafCount[node] > store.Order.Length - store.LeafStart[node])) { return false; }
            if (!leaf && (store.FirstChild[node] <= node
                    || store.ChildCount[node] > store.Count - store.FirstChild[node])) { return false; }
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

    static BoundingBox Inflate(BoundingBox box, double tolerance) {
        box.Inflate(tolerance);
        return box;
    }

    static double Axis(Point3d p, int axis) => axis == 0 ? p.X : axis == 1 ? p.Y : p.Z;
    static double Axis(Vector3d v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

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
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
