# [RASM_SPATIAL_INDEX]

`Spatial.Apply` folds every broad-phase modality onto one `Fin<SpatialAnswer>` rail over a `SpatialIndex` `[Union]` whose kernels differ only in build partition strategy and share one frozen `NodeStore`, so query, refit, and wire read that store kernel-agnostically. This owner serves predicate-exact primitive-bounds broad phase alone.

`SpatialIndex` computes on raw primitive coordinates, never a unit-bearing quantity type. `Wire` is the one cross-package egress and carries raw arrays alone, so `Rasm.Compute` decodes with no Compute type entering this owner; `Rasm.Persistence` content-addresses the frozen `NodeStore` itself, and this owner mints no second store.

## [01]-[INDEX]

- [02]-[SPATIAL_INDEX]: `Spatial.Apply` folds every broad-phase op over the shared node store.

## [02]-[SPATIAL_INDEX]

- Owner: `SpatialKind` rows own kernel selection, each row carrying its own builder over the shared `NodeStore`.
- Receipt: `QueryResult` carries every query verdict, and the index itself is the registered validity evidence, so this owner mints no receipt type.
- Packages: `RhinoCommon` through `Rasm.Numerics`, `Rasm.Domain`, CommunityToolkit.HighPerformance, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new kernel is one `SpatialKind` row over the shared `NodeStore`, a new query one `SpatialQuery` case with its `QueryKind` row and `Query` arm, a new op one `SpatialOp` case and `Apply` arm, a new knob one `BuildPolicy` column; a new node layout is one `SpatialIndex` case, admitted only by charter amendment.
- Boundary: every failure routes the one `Fin` rail — `GeometryFault` on the geometry channel, `key.InvalidInput()` on the admission channel; point k-NN and radius over a bare point set route `neighbors.md`.

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
    public const int PackedCountMax = (1 << 21) - 1;
    public static readonly BuildPolicy Canonical =
        new(LeafSize: 4, MaxDepth: 32, SahBuckets: 12, RefitDegradationLimit: 1.6, ParallelFloor: 4096);

    public bool IsAdmitted =>
        LeafSize is > 0 and <= PackedCountMax
        && MaxDepth > 0
        && SahBuckets > 1
        && double.IsFinite(RefitDegradationLimit) && RefitDegradationLimit > 1.0
        && ParallelFloor > 0;
}

// --- [MODELS] -----------------------------------------------------------------------------
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

    public sealed record Bvh(NodeStore Store, BoundingBox[] Primitives, double BuildCost, BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;
    public sealed record LinearOctree(NodeStore Store, BoundingBox[] Primitives, BuildPolicy Policy) : SpatialIndex;

    // Positional case synthesis overrides these abstract get/init columns; a same-name-param base Switch suppresses that synthesis and self-recurses.
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
    // Clone detaches the admitted set, so a frozen index never aliases a caller-mutable array.
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
        return new Bvh(store, boxes, AggregateSahCost(store), policy, SpatialKind.Bvh);
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
        return new LinearOctree(arena.Freeze(next, order), boxes, policy);
    }

    // Agglomerative PLOC: Morton presort, then windowed mutually-nearest merges recorded as a parent forest.
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
                double area = merged.Area;
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
        return new Bvh(store, boxes, AggregateSahCost(store), policy, SpatialKind.Agglomerative);
    }

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

    // AggregateSahCost scores the whole tree root-normalized; BestSah scores one candidate split against its leaf cost — distinct metrics over the same area.
    static double AggregateSahCost(NodeStore store) {
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
                double cost = 0.125 + (lCount * lBox.Area + rCount * rBox.Area) / Math.Max(bound.Area, double.Epsilon);
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

    // Reverse index order is child-before-parent: one bottom-up moment pass per evaluation feeds every query point.
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

    // Leaf distance is the exact DOUBLE primitive-box distance (0 inside); a centroid metric bounds nothing.
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

    // Store reference-identity IS the self-overlap discriminant — the modality is a value, never a knob.
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
            } else if (rLeaf || (!lLeaf && ls.Bound(l).Diagonal.Length >= rs.Bound(r).Diagonal.Length)) {
                for (int c = 0; c < ls.ChildCount[l]; c++) stack.Push((ls.FirstChild[l] + c, r));
            } else {
                for (int c = 0; c < rs.ChildCount[r]; c++) stack.Push((l, rs.FirstChild[r] + c));
            }
        }
        return pairs;
    }

    // --- [REFIT]
    // Refit is persistent: fresh bound arrays over the shared topology, so a published index is never mutated.
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
            bvh: b => AggregateSahCost(refitted) > b.Policy.RefitDegradationLimit * b.BuildCost
                ? b.Builder.Build(updated, Centroids(updated), b.Policy)
                : b with { Primitives = updated, Store = refitted },
            linearOctree: o => o with { Primitives = updated, Store = refitted });
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
    // A leaf descriptor packs a TAIL-RELATIVE LeafStart'; bounds copy the store's already-outward-rounded floats, so no second rounding site exists.
    internal static Fin<(float[] Bounds, long[] Nodes)> NodeLinkProjection(NodeStore store, Op key) {
        for (int node = 0; node < store.Count; node++)
            if (store.LeafCount[node] > BuildPolicy.PackedCountMax || store.ChildCount[node] > BuildPolicy.PackedCountMax)
                return Fin.Fail<(float[] Bounds, long[] Nodes)>(key.InvalidInput());

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
        return Fin.Succ((bounds, nodes));
    }

    const int ChildShift = 21;
    const int MortonDepth = 10;

    // --- [KERNELS]
    // Bounds narrow OUTWARD at this one write seam; Freeze copies the used prefix and the pooled memory dies with the build.
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

    // Down(v) <= v <= Up(v) for finite double, so a float node bound never falsely prunes and leaf tests re-read the double boxes.
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

    // Host Inflate mutates in place; the by-value copy makes this the pure form.
    static BoundingBox Inflate(BoundingBox box, double tolerance) {
        box.Inflate(tolerance);
        return box;
    }

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
                from _ in guard(b.Policy.IsAdmitted, k.InvalidInput()).ToFin()
                from boxes in SpatialIndex.Admit(b.Primitives)
                let built = b.Kind.Build(boxes, SpatialIndex.Centroids(boxes), b.Policy)
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
