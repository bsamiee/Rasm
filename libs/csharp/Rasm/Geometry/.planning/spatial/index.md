# [RASM_SPATIAL_INDEX]

ONE polymorphic `SpatialIndex` owner that closes the broad-phase acceleration concern over a `[Union]` of two first-principles author-kernels — a SAH-partitioned bounding-volume hierarchy (`Bvh`) and a Morton-order linear octree (`LinearOctree`) — built over ONE flat `NodeStore` value layout so a query, a refit, and the Compute-seam projection read the same struct-of-arrays node memory regardless of which kernel produced it. The page owns the `SpatialKind` discriminant (binding the sibling-owned `GeometryKeyPolicy` string-key comparer), the `NodeStore` SoA node memory, the `SpatialIndex` `[Union]` with its `Build`/`Refit` rail and `Query` fold, the `SpatialQuery` `[Union]` query algebra (`Nearest`/`Range`/`Ray`/`Overlap`) with the typed `QueryResult` carrier, and the `ToAcceleration` projection that hands the flat node store to the Compute `solver#CLASH_AND_TWIN` `AccelerationStructure` consumer without re-minting a second acceleration structure.

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
- Boundary: the index is the ONE polymorphic `SpatialIndex` `[Union]` and a `BvhTree`/`OctreeIndex` sibling-class family each carrying its own `Intersect`/`Search`/`Nearest` surface is the named density defect collapsed here onto one union over one `NodeStore` — the two kernels differ ONLY in their `Build` partition strategy, never in their traversal, so `Query`/`Refit` live on the union base and read the shared store kernel-agnostically over a uniform child range; the `Builders` `FrozenDictionary` is the single kernel-selection data table and a `SpatialKind kind switch` arm cascade in `Build` is the deleted form; `SpatialQuery` is the closed query algebra and a `QueryBox`/`QuerySphere`/`QueryRay`/`QueryKnn` method family on `SpatialIndex` is the rejected form — one `Query(SpatialQuery)` fold discriminates by case value; the `NodeStore` is struct-of-arrays (`float[] BoundsMin`, `float[] BoundsMax`, `int[] FirstChild`, `int[] ChildCount`, `int[] LeafStart`, `int[] LeafCount`) so a node walk is cache-coherent and every internal node — binary BVH node or up-to-eight-way octree cell — addresses its children as one contiguous `[FirstChild, FirstChild+ChildCount)` run, never two lossy `Left`/`Right` links that would drop an octree's middle octants; `ToAcceleration` returns the Compute `AccelerationStructure` `[Union]` value DIRECTLY (composing the settled cross-lane vocabulary, not minting a parallel `Acceleration` record) and a domain-local re-derivation of the Compute clash structure or a Compute-local re-build of this index is the rejected double-owner form; the Morton kernel spreads each normalized 10-bit axis coordinate through `Expand10` (the portable magic-number bit-interleave) and a hand-rolled per-bit loop where the portable magic-number spread or `Bmi2.X64.ParallelBitDeposit` already owns the operation is the named defect; the index composes RhinoCommon `BoundingBox`/`Ray3d` directly through the `Vectors` substrate and a domain-local `Aabb`/`Ray` re-mint is the deleted form; the slab ray test, the SAH cost scan, and the k-NN heap operate on raw primitive doubles inside the kernel because coordinates are the domain's native scalar (a coordinate is not a unit-bearing quantity), and a unit-carrying type in a traversal signature is the seam violation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Compute.Solver;                                          // AccelerationStructure — settled cross-lane seam vocabulary, composed never re-minted
using Rasm.Geometry;                                                // GeometryKeyPolicy — the one ordinal string-key comparer, owned at faults#FAULT_BAND
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
// Struct-of-arrays flat node memory: one allocation set every kernel writes and every traversal/refit/seam reads.
// A node is internal when LeafCount[i] == 0 — its children occupy the contiguous run [FirstChild, FirstChild+ChildCount):
// a binary BVH node has ChildCount==2, an octree cell up to 8, so no middle octant is ever dropped by a two-link scheme.
// A leaf carries LeafStart/LeafCount into the primitive-order array (FirstChild/ChildCount are 0 on a leaf).
public sealed record NodeStore(
    int Count,           // emitted node count; the arrays are over-allocated to the worst case, Count is the live prefix
    float[] BoundsMin,   // length 3*capacity, axis-major per node
    float[] BoundsMax,
    int[] FirstChild,
    int[] ChildCount,
    int[] LeafStart,
    int[] LeafCount,
    int[] Order) {       // primitive indices in build order; leaves slice [LeafStart, LeafStart+LeafCount)
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

    public sealed record Range(BoundingBox Box, Option<Sphere> Ball) : SpatialQuery;       // box descent, optional sphere refinement
    public sealed record Ray(Ray3d Ray, double MaxT) : SpatialQuery;                       // front-to-back nearest hit
    public sealed record Nearest(Point3d Query, int K) : SpatialQuery;                     // best-first k-NN
    public sealed record Overlap(SpatialIndex Other, double Tolerance) : SpatialQuery;     // tandem broad-phase candidate pairs
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The package GeometryFault union (band 2400) is owned at faults#FAULT_BAND; the spatial-index-relevant cases are referenced here by their real shape.
// GeometryFault.DegenerateInput(string)  -> 2401  (empty/non-finite primitive set)
// GeometryFault.IndexMismatch(string)    -> 2402  (refit primitive-count mismatch)

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

    // --- [BUILD] --------------------------------------------------------------------------
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

    // SAH-BVH: recursive top-down partition minimizing surface-area-heuristic cost over bucketed centroid splits.
    // An internal node reserves its two child slots contiguously (firstChild, firstChild+1) BEFORE recursing, so the
    // immediate children are adjacent and the [FirstChild, FirstChild+ChildCount) range the traversal walks is dense.
    static SpatialIndex BuildBvh(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        var order = Enumerable.Range(0, boxes.Length).ToArray();
        var store = Allocate(boxes.Length);
        int next = 1;                                              // node 0 is the root, reserved by the first Partition call
        void Partition(int node, int lo, int hi) {
            BoundingBox bound = Union(boxes, order, lo, hi);
            int count = hi - lo;
            if (count <= policy.LeafSize) {
                store.Write(node, bound, 0, 0, lo, count);
                return;
            }
            BoundingBox centroidBound = CentroidBound(centroids, order, lo, hi);
            (int axis, double cost, int splitBucket) = BestSah(boxes, centroids, order, lo, hi, bound, centroidBound, policy.SahBuckets);
            if (cost >= count) {                                   // leaf cheaper than any split
                store.Write(node, bound, 0, 0, lo, count);
                return;
            }
            double extent = Axis(centroidBound.Max, axis) - Axis(centroidBound.Min, axis);
            int mid = StablePartition(order, lo, hi, idx =>
                (int)(policy.SahBuckets * (Axis(centroids[idx], axis) - Axis(centroidBound.Min, axis)) / Math.Max(extent, double.Epsilon)) <= splitBucket);
            mid = mid == lo || mid == hi ? (lo + hi) / 2 : mid;     // guard a degenerate all-one-side bucket
            int firstChild = next;                                  // reserve the two child slots adjacently, then recurse
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

    // Linear (Morton) octree: sort centroids by 30-bit Z-order code, build the cell hierarchy from sorted radix runs.
    // Each internal cell reserves a contiguous slot run for ALL its 1–8 octant children (none dropped) before recursing.
    static SpatialIndex BuildOctree(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy) {
        BoundingBox root = Union(boxes, Enumerable.Range(0, boxes.Length).ToArray(), 0, boxes.Length);
        Vector3d span = root.Max - root.Min;
        uint[] codes = Array.ConvertAll(centroids, c => Morton(
            Normalize(c.X, root.Min.X, span.X), Normalize(c.Y, root.Min.Y, span.Y), Normalize(c.Z, root.Min.Z, span.Z)));
        var order = Enumerable.Range(0, boxes.Length).ToArray();
        Array.Sort(codes, order);                                  // one keyed sort: codes ascending, same permutation applied to order
        var store = Allocate(boxes.Length);
        int next = 1;                                              // node 0 is the root cell, reserved by the first Cell call
        void Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
            int count = hi - lo;
            if (count <= policy.LeafSize || depth >= Math.Min(policy.MaxDepth, 10)) {
                store.Write(node, Union(boxes, order, lo, hi), 0, 0, lo, count);
                return;
            }
            int shift = 3 * (9 - depth);                            // top octant nibble at this depth (10 levels of 3 bits)
            var runs = new List<(int Lo, int Hi)>(8);               // one entry per occupied octant — every run is retained
            int runStart = lo;
            for (int i = lo + 1; i <= hi; i++) {
                bool boundary = i == hi || ((codes[i] >> shift) & 0x7) != ((codes[runStart] >> shift) & 0x7);
                if (boundary) { runs.Add((runStart, i)); runStart = i; }
            }
            int firstChild = next;                                  // reserve a dense slot run for every occupied octant
            next += runs.Count;
            store.Write(node, bound, firstChild, runs.Count, -1, 0);
            for (int c = 0; c < runs.Count; c++)
                Cell(firstChild + c, runs[c].Lo, runs[c].Hi, depth + 1, Union(boxes, order, runs[c].Lo, runs[c].Hi));
        }
        Cell(0, 0, boxes.Length, 0, root);
        return new LinearOctree(store with { Count = next, Order = order }, boxes, centroids, root, Math.Min(policy.MaxDepth, 10));
    }

    // --- [QUERY] --------------------------------------------------------------------------
    public QueryResult Query(SpatialQuery query) =>
        query switch {
            SpatialQuery.Range range => new QueryResult.Hits(RangeHits(Store, Primitives, range)),
            SpatialQuery.Ray ray => RayNearest(Store, Primitives, ray),
            SpatialQuery.Nearest knn => new QueryResult.Nearest(KNearest(Store, Centroids(Primitives), knn)),
            SpatialQuery.Overlap overlap => new QueryResult.Pairs(OverlapPairs(this, overlap.Other, overlap.Tolerance)),
            _ => new QueryResult.Hits(Seq<int>()),
        };

    // Box/sphere descent: push the root, pop nodes whose bound intersects the region, collect intersecting leaf primitives.
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

    // Front-to-back slab test: descend ordered by entry distance, return the nearest primitive hit parameter.
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

    // Best-first k-NN: a size-K bounded max-heap over centroid distance (PriorityQueue keyed by NEGATED distance, so the
    // queue head is the current FARTHEST kept neighbour). The worst kept distance is read in O(1) at the head — Worst below
    // peeks it without ever re-scanning UnorderedItems — and a node prunes when its bound lower-distance exceeds that worst.
    static Seq<int> KNearest(NodeStore store, Point3d[] centroids, SpatialQuery.Nearest knn) {
        var heap = new PriorityQueue<int, double>();
        var stack = new Stack<int>();
        stack.Push(0);
        double Worst() => heap.TryPeek(out _, out double p) ? -p : double.MaxValue;   // O(1) head read, never an UnorderedItems scan
        while (stack.Count > 0) {
            int node = stack.Pop();
            double lower = store.Bound(node).ClosestPoint(knn.Query).DistanceTo(knn.Query);
            if (heap.Count >= knn.K && lower > Worst()) continue;
            if (store.LeafCount[node] > 0) {
                for (int s = 0; s < store.LeafCount[node]; s++) {
                    int prim = store.Order[store.LeafStart[node] + s];
                    double d = centroids[prim].DistanceTo(knn.Query);
                    if (heap.Count < knn.K) heap.Enqueue(prim, -d);
                    else heap.EnqueueDequeue(prim, -d);            // bounded insert against the head worst, O(1) read + O(log K)
                }
                continue;
            }
            for (int c = 0; c < store.ChildCount[node]; c++) stack.Push(store.FirstChild[node] + c);
        }
        return toSeq(heap.UnorderedItems.OrderBy(static e => -e.Priority).Select(static e => e.Element));
    }

    // Tandem broad-phase: walk both node stacks together, collect leaf-pair candidates whose bounds intersect within tolerance.
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

    // --- [REFIT] --------------------------------------------------------------------------
    // Topology-stable: re-bound leaves from updated primitive AABBs, propagate merged child bounds up the stored child range.
    public Fin<SpatialIndex> Refit(ReadOnlySpan<BoundingBox> updated) {
        if (updated.Length != Primitives.Length)
            return Fin.Fail<SpatialIndex>(GeometryFault.IndexMismatch($"refit:{updated.Length}!={Primitives.Length}"));
        var boxes = updated.ToArray();
        NodeStore store = Store;
        for (int node = store.NodeCount - 1; node >= 0; node--) {  // children precede parents in build emission order
            BoundingBox bound = store.LeafCount[node] > 0
                ? LeafBound(store, boxes, node)
                : ChildBound(store, node);
            store.Write(node, bound, store.FirstChild[node], store.ChildCount[node], store.LeafStart[node], store.LeafCount[node]);
        }
        return Fin.Succ(Switch<SpatialIndex>(
            bvh: b => b with { Primitives = boxes },
            linearOctree: o => o with { Primitives = boxes, Centroids = Centroids(boxes) }));
    }

    // Merge the bounds of every child in the contiguous [FirstChild, FirstChild+ChildCount) range of an internal node.
    static BoundingBox ChildBound(NodeStore store, int node) {
        var box = BoundingBox.Empty;
        for (int c = 0; c < store.ChildCount[node]; c++) box.Union(store.Bound(store.FirstChild[node] + c));
        return box;
    }

    // --- [ACCELERATION_SEAM] --------------------------------------------------------------
    // ToAcceleration returns the Compute solver#CLASH_AND_TWIN AccelerationStructure union value DIRECTLY
    // (the settled cross-lane vocabulary, never a parallel local record). The byte layout is dictated by the consumer decode:
    // ClashScale.BvhPairs reads nodeCount = Bounds.Length/6 and slices Bounds[i*6 .. i*6+6] as one node's [minX,minY,minZ,
    // maxX,maxY,maxZ], then treats Nodes.Span[i] as a flat PRIMITIVE id (triangles.Slice(Nodes[i]*9, 9)) over an O(N^2)
    // ALL-PAIRS leaf scan — the consumer carries no internal/leaf distinction and decodes no node-link hierarchy. The only
    // layout that round-trips against that decode is one entry PER LEAF PRIMITIVE: each primitive's AABB as 6 interleaved
    // floats and Nodes[i] == the primitive id. This is emitted exactly so; the hierarchy this owner builds is therefore NOT
    // consumed by the current Compute side, which is the live cross-lane contract conflict the RESEARCH [CLASH_SEAM] item
    // names (the consumer accelerates nothing). Resolving it requires the Compute owner to expose a node-link decode that
    // walks [FirstChild, FirstChild+ChildCount) — an edit to solver/lane.md, OUT OF THIS PAGE'S WRITE-SCOPE —
    // so the projection stays byte-faithful to the present consumer while the seam holds pending that arbitration.
    public AccelerationStructure ToAcceleration() {
        (float[] bounds, long[] nodes) = LeafProjection(Store);
        return Switch<AccelerationStructure>(
            bvh: b => new AccelerationStructure.Bvh(bounds.AsMemory(), nodes.AsMemory(), b.LeafSize),
            linearOctree: o => new AccelerationStructure.Octree(
                new[] { (float)o.Root.Min.X, (float)o.Root.Min.Y, (float)o.Root.Min.Z }.AsMemory(),
                Diagonal(o.Root), nodes.AsMemory(), o.MaxDepth));
    }

    // One interleaved 6-float AABB and one primitive id per leaf primitive, in Order traversal — the flat per-primitive
    // stream ClashScale.BvhPairs/OctreePairs decode (Bounds.Length/6 nodes, Nodes[i] the triangle id at triangles[id*9]).
    static (float[] Bounds, long[] Nodes) LeafProjection(NodeStore store) {
        int count = store.Order.Length;
        var bounds = new float[6 * count];
        var nodes = new long[count];
        for (int node = 0; node < store.NodeCount; node++) {
            if (store.LeafCount[node] == 0) continue;
            for (int s = 0; s < store.LeafCount[node]; s++) {
                int prim = store.Order[store.LeafStart[node] + s];
                BoundingBox leaf = store.Bound(node);
                int b = 6 * prim;
                (bounds[b], bounds[b + 1], bounds[b + 2]) = ((float)leaf.Min.X, (float)leaf.Min.Y, (float)leaf.Min.Z);
                (bounds[b + 3], bounds[b + 4], bounds[b + 5]) = ((float)leaf.Max.X, (float)leaf.Max.Y, (float)leaf.Max.Z);
                nodes[prim] = prim;
            }
        }
        return (bounds, nodes);
    }

    // --- [KERNELS] ------------------------------------------------------------------------
    // Worst case bounds every kernel: a binary BVH is 2N-1; a single-child Morton-octree chain adds up to one internal
    // node per level (capped at 10) per leaf group, so 12N+1 dominates both and the dense child runs never overflow.
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

    // Ray/AABB slab intersection: returns the entry parameter t within [0, max].
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

    // 30-bit Morton interleave: spread each normalized 10-bit axis coordinate then weave x|y<<1|z<<2.
    static uint Morton(uint x, uint y, uint z) => Expand10(x) | (Expand10(y) << 1) | (Expand10(z) << 2);

    static uint Expand10(uint v) {                                 // place each of 10 low bits into every third slot
        v &= 0x3FF;
        v = (v | (v << 16)) & 0x030000FF;
        v = (v | (v << 8)) & 0x0300F00F;
        v = (v | (v << 4)) & 0x030C30C3;
        v = (v | (v << 2)) & 0x09249249;
        return v;
    }

    static uint Normalize(double value, double min, double span) =>
        span <= double.Epsilon ? 0u : (uint)Math.Clamp((int)(1023.0 * (value - min) / span), 0, 1023);

    // In-place stable partition by predicate; returns the split index.
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
- [CLASH_SEAM] (LIVE CONTRADICTION, not a byte probe): `ToAcceleration` returns the Compute `solver#CLASH_AND_TWIN` `AccelerationStructure` union value directly. The Compute consumer decodes a FLAT per-primitive layout incompatible with a hierarchy: `ClashScale.BvhPairs` reads `nodeCount = Bounds.Length/6` and slices `Bounds[i*6 .. i*6+6]` as one node's interleaved `[min,max]`, then treats `Nodes.Span[i]` as a PRIMITIVE id (`triangles.Slice(Nodes[i]*9, 9)`) over an O(N²) all-pairs leaf scan with NO internal/leaf distinction and NO node-link decode — the consumer accelerates nothing and cannot walk a `FirstChild`/`ChildCount` hierarchy. The page's projection is therefore emitted in the ONLY layout that round-trips against that decode (one interleaved 6-float AABB and one `Nodes[i] == primitiveId` per leaf primitive, `LeafProjection`), so `Bounds.Length/6 == primitiveCount` and `Slice(i*6,6)` is primitive `i`'s box exactly. The unresolved item is NOT a bit layout — it is a bidirectional contract conflict: the hierarchy this owner builds is not consumed Compute-side, and consuming it requires the Compute owner to expose a node-link decode walking `[FirstChild, FirstChild+ChildCount)`, an edit to `solver/lane.md` OUTSIDE this page's write-scope. The two pages cannot both finalize until one owner's node-link contract wins; flagged for cross-page arbitration. The validation harness is a shared FROZEN golden-bytes fixture both packages assert (the geometry domain's `ToAcceleration` emits, Compute `ClashScale.BvhPairs` decodes, the fixture proves the round-trip), and the fixture must exist and pass before the seam is settled.

## [4]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface.

| [INDEX] | [AXIS/CONCERN]   | [OWNER]        | [KIND]                                                                      | [RAIL]                                       | [CASES] |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------------------- | :------------------------------------------- | :-----: |
|   [2]   | Spatial index    | `SpatialIndex` | `[Union]` (`Bvh`/`LinearOctree`) over one `NodeStore` + `Build`/`Refit`/`ToAcceleration` | `SpatialIndex.Build → Fin<SpatialIndex>`     |    2    |
|   [2a]  | Spatial query    | `SpatialQuery` | `[Union]` (`Nearest`/`Range`/`Ray`/`Overlap`) folded by one `Query`         | `SpatialIndex.Query → QueryResult` (pure, total) |    4    |

The two build kernels (SAH-BVH, Morton linear octree), the four query bodies (range descent, front-to-back ray slab, best-first k-NN, tandem overlap), and the topology-stable `Refit` are transcription-complete pure-managed fences over one `NodeStore` SoA layout with a contiguous `[FirstChild, FirstChild+ChildCount)` child range that loses no octree octant. The `Query` fold is pure, total, and seam-independent. The `[CLASH_SEAM]` (RESEARCH) is the one open item: a LIVE cross-lane contract conflict requiring a `solver/lane.md` edit OUTSIDE this page's write-scope, resolved only after the cross-page node-link contract is arbitrated and the two-sided golden-bytes fixture passes.
