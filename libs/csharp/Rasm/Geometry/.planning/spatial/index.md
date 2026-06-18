# [RASM_SPATIAL_INDEX]

ONE polymorphic `SpatialIndex` owner that closes the broad-phase acceleration concern over a `[Union]` of two first-principles author-kernels — a SAH-partitioned bounding-volume hierarchy (`Bvh`) and a Morton-order linear octree (`LinearOctree`) — built over ONE flat `NodeStore` value layout so a query, a refit, and the Compute-seam projection read the same struct-of-arrays node memory regardless of which kernel produced it. The page owns the `SpatialKind` discriminant (binding the sibling-owned `GeometryKeyPolicy` string-key comparer), the `NodeStore` SoA node memory, the `SpatialIndex` `[Union]` with its `Build`/`Refit` rail and `Query` fold, the `SpatialQuery` `[Union]` query algebra (`Nearest`/`Range`/`Ray`/`Overlap`) with the typed `QueryResult` carrier, and the `ToAcceleration` projection that hands the flat node store to the `Compute/solver/clash#CLASH_AND_TWIN` `AccelerationStructure` consumer without re-minting a second acceleration structure.

The index composes RhinoCommon `BoundingBox`/`Point3d`/`Vector3d`/`Ray3d`/`Sphere`/`Line` through the `Vectors` substrate as settled vocabulary — read, compose, never re-mint — and operates on raw primitive coordinates because the index is a geometric-coordinate structure, not a unit-bearing quantity surface. The only cross-package egress is `ToAcceleration`, returning the Compute `AccelerationStructure` `[Union]` value directly; a domain-local acceleration-structure record duplicating that union's field shape is the rejected double-owner form. The immutable `NodeStore` is the hash-friendly record the Persistence blob lane content-addresses by reference; the geometry domain computes no hash and mints no second store.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                          |
| :-----: | :------------ | :----------------------------------------------------------------------------- |
|   [1]   | SPATIAL_INDEX | `SpatialIndex` Bvh/LinearOctree union over one `NodeStore`; SAH/Morton build bodies; `SpatialQuery` Nearest/Range/Ray/Overlap algebra; `Refit`; `ToAcceleration` Compute seam |

## [2]-[SPATIAL_INDEX]

- Owner: `SpatialKind` `[SmartEnum<string>]` index discriminant (`bvh`/`octree`) binding the sibling-owned `GeometryKeyPolicy` (`faults/faults#FAULT_BAND`) as its string-key comparer; `NodeStore` the struct-of-arrays flat node memory (per-node AABB min/max, child range, leaf primitive range) every kernel writes and every traversal reads; `SpatialIndex` `[Union]` `Bvh`/`LinearOctree` carrying that one store plus the primitive `BoundingBox[]`/`Point3d[]` payload; `SpatialQuery` `[Union]` `Nearest`/`Range`/`Ray`/`Overlap` the query algebra; `QueryResult` the typed hit carrier; `ToAcceleration` the Compute-seam projection returning the Compute `AccelerationStructure` union directly.
- Cases: `SpatialKind` rows `bvh` · `octree` (2); `SpatialIndex` cases `Bvh` · `LinearOctree` (2); `SpatialQuery` cases `Nearest` · `Range` · `Ray` · `Overlap` (4).
- Entry: `public static Fin<SpatialIndex> Build(SpatialKind kind, ReadOnlySpan<BoundingBox> primitives, BuildPolicy policy)` — the ONE build entrypoint discriminating by `SpatialKind` value, `Fin<T>` routing a band-2400 `GeometryFault.DegenerateInput` when the primitive set is empty or carries a non-finite bound; `public QueryResult Query(SpatialQuery query)` is the ONE pure total query fold dispatching `Nearest`/`Range`/`Ray`/`Overlap` over the index ADT (a query is total over a built index — no rail); `public Fin<SpatialIndex> Refit(ReadOnlySpan<BoundingBox> updated)` re-bounds the existing node topology in place for an incremental `ClashScale` rebuild without a full re-partition, `Fin<T>` aborting on a primitive-count mismatch.
- Auto: `Build` reads the `Builders` `FrozenDictionary` keyed by `SpatialKind` so the kernel selection is a data-table row, never a `kind switch` cascade — the SAH-BVH row recursively partitions on the minimum surface-area-heuristic cost split over the centroid axis emitting a binary node tree, the linear-octree row sorts primitive centroids by 30-bit Morton code (10 bits per axis via bit-spreading) then builds the cell hierarchy from the sorted radix runs as an N-way node whose 1–8 children occupy a contiguous `[FirstChild, FirstChild+ChildCount)` run in the store; every kernel writes the SAME `NodeStore` SoA layout — an internal node stores `FirstChild`/`ChildCount` (the `Left`/`Right` columns are repurposed as `FirstChild`/`ChildCount` so a binary node is the `ChildCount == 2` special case and an octree node carries up to eight in one contiguous range) — so `Query` and `Refit` are kernel-agnostic and traverse the full child range, never just two links. The `Query` fold lowers every query case to one traversal over the shared `NodeStore` AABB stack walking the whole `[FirstChild, FirstChild+ChildCount)` child range at each internal node: `Range` is a box/sphere descent collecting leaf primitives whose bound intersects the query region; `Ray` is a slab-test descent ordered front-to-back returning the nearest primitive hit `t`; `Nearest` is a best-first k-NN descent over a `PriorityQueue` worst-distance bound read in O(1) at the queue head by `Peek`, pruned by the node-bound lower distance; `Overlap` walks two indices' node stacks in tandem collecting leaf-pair candidates whose bounds intersect within tolerance (the broad-phase clash candidate set `ClashScale` confirms). `Refit` re-bounds leaves from the updated primitive AABBs then propagates merged child bounds up the stored child range — topology-stable, no re-sort.
- Receipt: none on the query rail — a query verdict is the typed `QueryResult` carrier (hit ids, ray `t`, k-NN ordered ids, overlap pairs), not an evidence record; the build/refit rail returns the index itself, and the index's `NodeStore` IS the hash-friendly immutable record the Persistence blob lane content-addresses.
- Packages: Rhino.Inside / RhinoCommon (`BoundingBox`/`Point3d`/`Vector3d`/`Ray3d`/`Sphere`/`Line` via `Vectors`), Rasm.Compute.Solver (`AccelerationStructure` seam union — composed, never re-minted), `Rasm.Geometry` (`GeometryKeyPolicy` string-key comparer — composed, never re-minted), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `PriorityQueue<TElement,TPriority>`, `ReadOnlyMemory<T>`)
- Growth: a new acceleration kernel is one `SpatialKind` row plus one `Builders` `FrozenDictionary` row plus one `SpatialIndex` union case writing the shared `NodeStore` — never a parallel index class with a duplicated query surface (a third kernel is admitted only by a charter amendment, never widened silently from this leaf page); a new query shape is one `SpatialQuery` case plus one `Query`-fold arm over the same store; a new build knob is one column on `BuildPolicy`; zero new surface.
- Boundary: the index is the ONE polymorphic `SpatialIndex` `[Union]` and a `BvhTree`/`OctreeIndex` sibling-class family each carrying its own `Intersect`/`Search`/`Nearest` surface is the named density defect collapsed here onto one union over one `NodeStore` — the two kernels differ ONLY in their `Build` partition strategy, never in their traversal, so `Query`/`Refit` live on the union base and read the shared store kernel-agnostically over a uniform child range; the `Builders` `FrozenDictionary` is the single kernel-selection data table and a `SpatialKind kind switch` arm cascade in `Build` is the deleted form; `SpatialQuery` is the closed query algebra and a `QueryBox`/`QuerySphere`/`QueryRay`/`QueryKnn` method family on `SpatialIndex` is the rejected form — one `Query(SpatialQuery)` fold discriminates by case value; the `NodeStore` is struct-of-arrays (`float[] BoundsMin`, `float[] BoundsMax`, `int[] FirstChild`, `int[] ChildCount`, `int[] LeafStart`, `int[] LeafCount`) so a node walk is cache-coherent and every internal node — binary BVH node or up-to-eight-way octree cell — addresses its children as one contiguous `[FirstChild, FirstChild+ChildCount)` run, never two lossy `Left`/`Right` links that drop an octree's middle octants; `ToAcceleration` returns the Compute `AccelerationStructure` `[Union]` value DIRECTLY (composing the settled cross-lane vocabulary, not minting a parallel `Acceleration` record) carrying the FROZEN node-link wire the `Compute/solver/clash#CLASH_AND_TWIN` `ClashScale.BvhPairs` traversal decodes — `Bounds` is `6·NodeCount` little-endian `float32` as one interleaved `[minX,minY,minZ,maxX,maxY,maxZ]` AABB per node in node-index order (root = node 0), and `Nodes` is `NodeCount + primitiveCount` little-endian `int64` where `Nodes[node]` for `node < NodeCount` is the per-node descriptor (non-negative internal node packs `(FirstChild << 21) | ChildCount`; negative leaf node packs `-(((LeafStart' << 21) | LeafCount)) - 1` where `LeafStart'` indexes the primitive-id tail) and `Nodes[NodeCount + LeafStart' + s]` for `s ∈ [0, LeafCount)` is the leaf primitive id — so `NodeCount == Bounds.Length/6` and a Compute walk descends the contiguous `[FirstChild, FirstChild+ChildCount)` child range; a domain-local re-derivation of the Compute clash structure or a Compute-local re-build of this index is the rejected double-owner form; the Morton kernel spreads each normalized 10-bit axis coordinate through `Expand10` (the portable magic-number bit-interleave) and a hand-rolled per-bit loop where the portable magic-number spread or `Bmi2.X64.ParallelBitDeposit` already owns the operation is the named defect; the index composes RhinoCommon `BoundingBox`/`Ray3d` directly through the `Vectors` substrate and a domain-local `Aabb`/`Ray` re-mint is the deleted form; the slab ray test, the SAH cost scan, and the k-NN heap operate on raw primitive doubles inside the kernel because coordinates are the domain's native scalar (a coordinate is not a unit-bearing quantity), and a unit-carrying type in a traversal signature is the seam violation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Compute.Solver;
using Rasm.Geometry;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Geometry.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<GeometryKeyPolicy, string>]
[KeyMemberComparer<GeometryKeyPolicy, string>]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new("bvh");
    public static readonly SpatialKind Octree = new("octree");
}

// --- [CONSTANTS] --------------------------------------------------------------------------
public sealed record BuildPolicy(int LeafSize, int MaxDepth, int SahBuckets) {
    public static readonly BuildPolicy Canonical = new(LeafSize: 4, MaxDepth: 32, SahBuckets: 12);
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
    public int NodeCount => Count;

    public BoundingBox Bound(int node) =>
        new(new Point3d(BoundsMin[3 * node], BoundsMin[3 * node + 1], BoundsMin[3 * node + 2]),
            new Point3d(BoundsMax[3 * node], BoundsMax[3 * node + 1], BoundsMax[3 * node + 2]));

    internal void Write(int node, BoundingBox box, int firstChild, int childCount, int leafStart, int leafCount) {
        (BoundsMin[3 * node], BoundsMin[3 * node + 1], BoundsMin[3 * node + 2]) = (Single(box.Min.X), Single(box.Min.Y), Single(box.Min.Z));
        (BoundsMax[3 * node], BoundsMax[3 * node + 1], BoundsMax[3 * node + 2]) = (Single(box.Max.X), Single(box.Max.Y), Single(box.Max.Z));
        (FirstChild[node], ChildCount[node], LeafStart[node], LeafCount[node]) = (firstChild, childCount, leafStart, leafCount);
    }

    static float Single(double value) => (float)value;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueryResult {
    private QueryResult() { }

    public sealed record Hits(Seq<int> Ids) : QueryResult;
    public sealed record RayHit(Option<int> Id, double T) : QueryResult;
    public sealed record Nearest(Seq<int> Ordered) : QueryResult;
    public sealed record Pairs(Seq<(int Left, int Right)> Overlaps) : QueryResult;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialQuery {
    private SpatialQuery() { }

    public sealed record Range(BoundingBox Box, Option<Sphere> Ball) : SpatialQuery;
    public sealed record Ray(Ray3d Ray, double MaxT) : SpatialQuery;
    public sealed record Nearest(Point3d Query, int K) : SpatialQuery;
    public sealed record Overlap(SpatialIndex Other, double Tolerance) : SpatialQuery;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialIndex {
    private SpatialIndex() { }

    public sealed record Bvh(NodeStore Store, BoundingBox[] Primitives, int LeafSize) : SpatialIndex;
    public sealed record LinearOctree(NodeStore Store, BoundingBox[] Primitives, Point3d[] Centroids, BoundingBox Root, int MaxDepth) : SpatialIndex;

    public SpatialKind Kind =>
        Switch(
            bvh: static _ => SpatialKind.Bvh,
            linearOctree: static _ => SpatialKind.Octree);

    NodeStore Store =>
        Switch(
            bvh: static b => b.Store,
            linearOctree: static o => o.Store);

    BoundingBox[] Primitives =>
        Switch(
            bvh: static b => b.Primitives,
            linearOctree: static o => o.Primitives);

    // --- [BUILD]
    static readonly FrozenDictionary<SpatialKind, Func<BoundingBox[], Point3d[], BuildPolicy, SpatialIndex>> Builders =
        new (SpatialKind Kind, Func<BoundingBox[], Point3d[], BuildPolicy, SpatialIndex> Build)[] {
            (SpatialKind.Bvh, static (boxes, centroids, policy) => BuildBvh(boxes, centroids, policy)),
            (SpatialKind.Octree, static (boxes, centroids, policy) => BuildOctree(boxes, centroids, policy)),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Build);

    public static Fin<SpatialIndex> Build(SpatialKind kind, ReadOnlySpan<BoundingBox> primitives, BuildPolicy policy) {
        if (primitives.Length == 0)
            return Fin.Fail<SpatialIndex>(GeometryFault.DegenerateInput("spatial-build:empty"));
        var boxes = primitives.ToArray();
        return toSeq(boxes).ForAll(static box => box.IsValid)
            ? Builders.TryGetValue(kind, out var build)
                ? Fin.Succ(build(boxes, Centroids(boxes), policy))
                : Fin.Fail<SpatialIndex>(GeometryFault.DegenerateInput($"spatial-kind-miss:{kind.Key}"))
            : Fin.Fail<SpatialIndex>(GeometryFault.DegenerateInput("spatial-build:non-finite-bound"));
    }

    static Point3d[] Centroids(BoundingBox[] boxes) =>
        Array.ConvertAll(boxes, static box => 0.5 * (box.Min + box.Max));

    static SpatialIndex BuildBvh(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        var order = Enumerable.Range(0, boxes.Length).ToArray();
        var store = Allocate(boxes.Length);
        int next = 1;
        void Partition(int node, int lo, int hi) {
            BoundingBox bound = Union(boxes, order, lo, hi);
            int count = hi - lo;
            if (count <= policy.LeafSize) {
                store.Write(node, bound, 0, 0, lo, count);
                return;
            }
            BoundingBox centroidBound = CentroidBound(centroids, order, lo, hi);
            (int axis, double cost, int splitBucket) = BestSah(boxes, centroids, order, lo, hi, bound, centroidBound, policy.SahBuckets);
            if (cost >= count) {
                store.Write(node, bound, 0, 0, lo, count);
                return;
            }
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            int mid = StablePartition(order, lo, hi, idx =>
                (int)(policy.SahBuckets * (Axis(centroids[idx], axis) - Axis(centroidBound.Min, axis)) / Math.Max(extent, double.Epsilon)) <= splitBucket);
            mid = mid == lo || mid == hi ? (lo + hi) / 2 : mid;
            int firstChild = next;
            next += 2;
            store.Write(node, bound, firstChild, 2, -1, 0);
            Partition(firstChild, lo, mid);
            Partition(firstChild + 1, mid, hi);
        }
        Partition(0, 0, boxes.Length);
        return new Bvh(store with { Count = next, Order = order }, boxes, policy.LeafSize);
    }

    static (int Axis, double Cost, int Bucket) BestSah(BoundingBox[] boxes, Point3d[] centroids, int[] order, int lo, int hi, BoundingBox bound, BoundingBox centroidBound, int buckets) =>
        toSeq(Enumerable.Range(0, 3)).Fold((Axis: 0, Cost: double.MaxValue, Bucket: 0), (best, axis) => {
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            if (extent <= double.Epsilon) return best;
            var counts = new int[buckets];
            var bins = new BoundingBox[buckets];
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
        var box = BoundingBox.Empty;
        int count = 0;
        for (int b = from; b < to; b++) { box.Union(bins[b]); count += counts[b]; }
        return (box, count);
    }

    static SpatialIndex BuildOctree(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        BoundingBox root = Union(boxes, Enumerable.Range(0, boxes.Length).ToArray(), 0, boxes.Length);
        Vector3d span = root.Max - root.Min;
        uint[] codes = Array.ConvertAll(centroids, c => Morton(
            Normalize(c.X, root.Min.X, span.X), Normalize(c.Y, root.Min.Y, span.Y), Normalize(c.Z, root.Min.Z, span.Z)));
        var order = Enumerable.Range(0, boxes.Length).ToArray();
        Array.Sort(codes, order);
        var store = Allocate(boxes.Length);
        int next = 1;
        void Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
            int count = hi - lo;
            if (count <= policy.LeafSize || depth >= Math.Min(policy.MaxDepth, 10)) {
                store.Write(node, Union(boxes, order, lo, hi), 0, 0, lo, count);
                return;
            }
            int shift = 3 * (9 - depth);
            var runs = new List<(int Lo, int Hi)>(8);
            int runStart = lo;
            for (int i = lo + 1; i <= hi; i++) {
                bool boundary = i == hi || ((codes[i] >> shift) & 0x7) != ((codes[runStart] >> shift) & 0x7);
                if (boundary) { runs.Add((runStart, i)); runStart = i; }
            }
            int firstChild = next;
            next += runs.Count;
            store.Write(node, bound, firstChild, runs.Count, -1, 0);
            for (int c = 0; c < runs.Count; c++)
                Cell(firstChild + c, runs[c].Lo, runs[c].Hi, depth + 1, Union(boxes, order, runs[c].Lo, runs[c].Hi));
        }
        Cell(0, 0, boxes.Length, 0, root);
        return new LinearOctree(store with { Count = next, Order = order }, boxes, centroids, root, Math.Min(policy.MaxDepth, 10));
    }

    // --- [QUERY]
    public QueryResult Query(SpatialQuery query) =>
        query switch {
            SpatialQuery.Range range => new QueryResult.Hits(RangeHits(Store, Primitives, range)),
            SpatialQuery.Ray ray => RayNearest(Store, Primitives, ray),
            SpatialQuery.Nearest knn => new QueryResult.Nearest(KNearest(Store, Centroids(Primitives), knn)),
            SpatialQuery.Overlap overlap => new QueryResult.Pairs(OverlapPairs(this, overlap.Other, overlap.Tolerance)),
            _ => new QueryResult.Hits(Seq<int>()),
        };

    static Seq<int> RangeHits(NodeStore store, BoundingBox[] primitives, SpatialQuery.Range range) {
        var hits = Seq<int>();
        var stack = new Stack<int>();
        stack.Push(0);
        while (stack.Count > 0) {
            int node = stack.Pop();
            BoundingBox bound = store.Bound(node);
            if (!Intersects(bound, range.Box)) continue;
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
        var stack = new Stack<int>();
        stack.Push(0);
        double best = ray.MaxT;
        int hit = -1;
        while (stack.Count > 0) {
            int node = stack.Pop();
            if (!Slab(store.Bound(node), ray.Ray, best, out _)) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    if (Slab(primitives[prim], ray.Ray, best, out double t) && t < best) { best = t; hit = prim; }
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return new QueryResult.RayHit(hit >= 0 ? Some(hit) : None, hit >= 0 ? best : ray.MaxT);
    }

    static Seq<int> KNearest(NodeStore store, Point3d[] centroids, SpatialQuery.Nearest knn) {
        var heap = new PriorityQueue<int, double>();
        var stack = new Stack<int>();
        stack.Push(0);
        double Worst() => heap.TryPeek(out _, out double p) ? -p : double.MaxValue;
        while (stack.Count > 0) {
            int node = stack.Pop();
            double lower = store.Bound(node).ClosestPoint(knn.Query).DistanceTo(knn.Query);
            if (heap.Count >= knn.K && lower > Worst()) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    double d = centroids[prim].DistanceTo(knn.Query);
                    if (heap.Count < knn.K) heap.Enqueue(prim, -d);
                    else heap.EnqueueDequeue(prim, -d);
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return toSeq(heap.UnorderedItems.OrderBy(static e => -e.Priority).Select(static e => e.Element));
    }

    static Seq<(int Left, int Right)> OverlapPairs(SpatialIndex left, SpatialIndex right, double tolerance) {
        (NodeStore ls, BoundingBox[] lp) = (left.Store, left.Primitives);
        (NodeStore rs, BoundingBox[] rp) = (right.Store, right.Primitives);
        var pairs = Seq<(int, int)>();
        var stack = new Stack<(int L, int R)>();
        stack.Push((0, 0));
        while (stack.Count > 0) {
            (int l, int r) = stack.Pop();
            if (!Intersects(Inflate(ls.Bound(l), tolerance), rs.Bound(r))) continue;
            bool lLeaf = ls.LeafCount[l] > 0, rLeaf = rs.LeafCount[r] > 0;
            if (lLeaf && rLeaf) {
                for (int a = 0; a < ls.LeafCount[l]; a++)
                    for (int b = 0; b < rs.LeafCount[r]; b++) {
                        int pa = ls.Order[ls.LeafStart[l] + a], pb = rs.Order[rs.LeafStart[r] + b];
                        if (Intersects(Inflate(lp[pa], tolerance), rp[pb])) pairs = pairs.Add((pa, pb));
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
    public Fin<SpatialIndex> Refit(ReadOnlySpan<BoundingBox> updated) {
        if (updated.Length != Primitives.Length)
            return Fin.Fail<SpatialIndex>(GeometryFault.IndexMismatch($"refit:{updated.Length}!={Primitives.Length}"));
        var boxes = updated.ToArray();
        NodeStore store = Store;
        for (int node = store.NodeCount - 1; node >= 0; node--) {
            BoundingBox bound = store.LeafCount[node] > 0
                ? LeafBound(store, boxes, node)
                : ChildBound(store, node);
            store.Write(node, bound, store.FirstChild[node], store.ChildCount[node], store.LeafStart[node], store.LeafCount[node]);
        }
        return Fin.Succ(Switch<SpatialIndex>(
            bvh: b => b with { Primitives = boxes },
            linearOctree: o => o with { Primitives = boxes, Centroids = Centroids(boxes) }));
    }

    static BoundingBox ChildBound(NodeStore store, int node) {
        var box = BoundingBox.Empty;
        for (int c = 0; c < store.ChildCount[node]; c++) box.Union(store.Bound(store.FirstChild[node] + c));
        return box;
    }

    // --- [ACCELERATION_SEAM]
    public AccelerationStructure ToAcceleration() {
        (float[] bounds, long[] nodes) = NodeLinkProjection(Store);
        return Switch<AccelerationStructure>(
            bvh: b => new AccelerationStructure.Bvh(bounds.AsMemory(), nodes.AsMemory(), b.LeafSize),
            linearOctree: o => new AccelerationStructure.Octree(
                new[] { (float)o.Root.Min.X, (float)o.Root.Min.Y, (float)o.Root.Min.Z }.AsMemory(),
                Diagonal(o.Root), nodes.AsMemory(), o.MaxDepth));
    }

    static (float[] Bounds, long[] Nodes) NodeLinkProjection(NodeStore store) {
        int nodeCount = store.NodeCount;
        var bounds = new float[6 * nodeCount];
        var nodes = new long[nodeCount + store.Order.Length];
        int tail = nodeCount;
        for (int node = 0; node < nodeCount; node++) {
            BoundingBox box = store.Bound(node);
            int b = 6 * node;
            (bounds[b], bounds[b + 1], bounds[b + 2]) = ((float)box.Min.X, (float)box.Min.Y, (float)box.Min.Z);
            (bounds[b + 3], bounds[b + 4], bounds[b + 5]) = ((float)box.Max.X, (float)box.Max.Y, (float)box.Max.Z);
            if (store.LeafCount[node] > 0) {
                nodes[node] = -(((long)tail << ChildShift) | (uint)store.LeafCount[node]) - 1;
                for (int s = 0; s < store.LeafCount[node]; s++)
                    nodes[tail++] = store.Order[store.LeafStart[node] + s];
            } else {
                nodes[node] = ((long)store.FirstChild[node] << ChildShift) | (uint)store.ChildCount[node];
            }
        }
        return (bounds, nodes);
    }

    const int ChildShift = 21;

    // --- [KERNELS]
    static NodeStore Allocate(int primitiveCount) {
        int capacity = Math.Max(1, 12 * primitiveCount + 1);
        return new NodeStore(0, new float[3 * capacity], new float[3 * capacity], new int[capacity], new int[capacity], new int[capacity], new int[capacity], new int[primitiveCount]);
    }

    static BoundingBox Union(BoundingBox[] boxes, int[] order, int lo, int hi) {
        var box = BoundingBox.Empty;
        for (int i = lo; i < hi; i++) box.Union(boxes[order[i]]);
        return box;
    }

    static BoundingBox CentroidBound(Point3d[] centroids, int[] order, int lo, int hi) {
        var box = BoundingBox.Empty;
        for (int i = lo; i < hi; i++) box.Union(centroids[order[i]]);
        return box;
    }

    static BoundingBox LeafBound(NodeStore store, BoundingBox[] boxes, int node) {
        var box = BoundingBox.Empty;
        for (int s = 0; s < store.LeafCount[node]; s++) box.Union(boxes[store.Order[store.LeafStart[node] + s]]);
        return box;
    }

    static BoundingBox Inflate(BoundingBox box, double tolerance) =>
        new(box.Min - new Vector3d(tolerance, tolerance, tolerance), box.Max + new Vector3d(tolerance, tolerance, tolerance));

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
        var buffer = new int[hi - lo];
        int b = 0;
        for (int i = lo; i < hi; i++) if (onLeft(order[i])) order[write++] = order[i]; else buffer[b++] = order[i];
        Array.Copy(buffer, 0, order, write, b);
        return write;
    }
}
```

## [3]-[RESEARCH]

- [SAH_PARTITION] — the `BuildBvh` body is the bucketed surface-area-heuristic partition — `BestSah` bins centroids into `BuildPolicy.SahBuckets` buckets per axis, scans the `buckets-1` split planes accumulating left/right bounds and counts, and minimizes `0.125 + (lCount·SA(lBox) + rCount·SA(rBox)) / SA(node)`; a split is taken only when its cost undercuts the leaf cost (`count`), the degenerate all-one-side bucket falls back to the median, internal nodes reserve their two child slots contiguously before recursing so the `[FirstChild, FirstChild+ChildCount)` range is dense, and `RhinoCommon` `BoundingBox.Union`/`SurfaceArea`-equivalent (`(d.X·d.Y + d.Y·d.Z + d.Z·d.X)·2`) are pure-managed — no host probe; the kernel is transcription-complete and pure-managed.
- [MORTON_OCTREE] — the `BuildOctree` body normalizes each centroid axis to a 10-bit grid, interleaves through the portable magic-number `Expand10` bit-spread (no `Bmi2.X64.ParallelBitDeposit` host dependence so the build is RID-agnostic), does ONE keyed sort (`Array.Sort(codes, order)` permutes `order` by the Morton key in a single pass), and builds the cell hierarchy by splitting each sorted run on the depth-indexed octant nibble (`3·(9-depth)` shift, 10 levels) — every occupied octant (1–8) is retained as a child in a contiguous reserved slot run, so no middle octant is dropped and the traversal walks the full child range; the linear-octree contract — sort-by-Morton then radix-run subdivision into a dense child range — is the entire correctness claim and is pure-managed.
- [CLASH_SEAM] — `ToAcceleration` returns the `Compute/solver/clash#CLASH_AND_TWIN` `AccelerationStructure` union value directly carrying the FROZEN node-link wire `NodeLinkProjection` emits and `ClashScale.BvhPairs` decodes by a proper BVH descent over the contiguous `[FirstChild, FirstChild+ChildCount)` child range — the node-link hierarchy is the canonical contract (O(N log N)-capable), the prior flat per-primitive O(N²) all-pairs decode is the deleted form. Canonical layout: `Bounds` = `6·NodeCount` little-endian `float32`, node `i`'s AABB at `Bounds[i·6 .. i·6+6)` as `[minX,minY,minZ,maxX,maxY,maxZ]`, node-index order with the SAH-BVH/Morton-octree root at node 0; `Nodes` = `NodeCount + primitiveCount` little-endian `int64` split as a `NodeCount`-long descriptor block followed by a primitive-id tail — `Nodes[node]` non-negative is an INTERNAL node packing `(FirstChild << 21) | ChildCount` (binary BVH node is `ChildCount == 2`, octree cell is `ChildCount ∈ [1,8]`), `Nodes[node]` negative is a LEAF packing `-(((LeafStart' << 21) | LeafCount)) - 1` where `LeafStart'` is the zero-based offset into the tail and `Nodes[NodeCount + LeafStart' + s]` for `s ∈ [0, LeafCount)` is the leaf primitive id (the `Order`-permuted primitive index). `NodeCount == Bounds.Length/6`; the 21-bit `ChildShift` field admits up to `2²¹−1` nodes and the same `2²¹−1` tail offset, sufficient for the `12·primitiveCount+1` node-store capacity bound.
- [CLASH_GOLDEN] (frozen two-sided fixture, both pages assert byte-identical): a single canonical input — a fixed 8-primitive `BoundingBox[]` set built with `BuildPolicy.Canonical` through `SpatialIndex.Build(SpatialKind.Bvh, …)` — projects through `ToAcceleration` to one `AccelerationStructure.Bvh` whose `Bounds`/`Nodes` are serialized little-endian into a frozen golden byte sequence checked into the shared test corpus; the Rasm geometry spec asserts `NodeLinkProjection` emits exactly those bytes, the `Compute/solver/clash#CLASH_GOLDEN` spec asserts `ClashScale.BvhPairs` decodes those same bytes into the agreed clash-pair set, and the round-trip (Rasm emits → Compute decodes → confirmed clash count) is the seam-settled signal; the fixture must exist and pass on BOTH sides before the seam finalizes.

## [4]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface.

| [INDEX] | [AXIS/CONCERN]   | [OWNER]        | [KIND]                                                                      | [RAIL]                                       | [CASES] |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------------------- | :------------------------------------------- | :-----: |
|   [2]   | Spatial index    | `SpatialIndex` | `[Union]` (`Bvh`/`LinearOctree`) over one `NodeStore` + `Build`/`Refit`/`ToAcceleration` | `SpatialIndex.Build → Fin<SpatialIndex>`     |    2    |
|   [2a]  | Spatial query    | `SpatialQuery` | `[Union]` (`Nearest`/`Range`/`Ray`/`Overlap`) folded by one `Query`         | `SpatialIndex.Query → QueryResult` (pure, total) |    4    |

The two build kernels (SAH-BVH, Morton linear octree), the four query bodies (range descent, front-to-back ray slab, best-first k-NN, tandem overlap), and the topology-stable `Refit` are transcription-complete pure-managed fences over one `NodeStore` SoA layout with a contiguous `[FirstChild, FirstChild+ChildCount)` child range that loses no octree octant. The `Query` fold is pure, total, and seam-independent. The `[CLASH_SEAM]` is settled: `ToAcceleration`/`NodeLinkProjection` emit the FROZEN node-link wire (per-node interleaved AABB + `(FirstChild << 21) | ChildCount` descriptor with a leaf primitive-id tail) the `Compute/solver/clash#CLASH_AND_TWIN` `ClashScale.BvhPairs` traversal descends over the same contiguous child range, and the `[CLASH_GOLDEN]` two-sided frozen golden-bytes fixture is the byte-identical round-trip both pages assert before the seam finalizes.
