# [RASM_NEIGHBORS]

THE neighborhood substrate: ONE `NeighborIndex` `[Union]` over every proximity-index modality — the native `RTree` tiers (point array, point cloud, mesh faces, inserted bounds) and the static `Supercluster.KDTree.Net` kd-tree tier — behind ONE `Query(NeighborQuery)` dispatch, plus the `NeighborKernel` operation surface that owns every per-point neighborhood fold in the corpus: batch kNN/radius graphs with typed receipts, neighborhood PCA, covariance normal estimation with Hoppe-DeRose MST orientation, quadric-fit principal curvature with Koenderink classification, and the ONE rotation-minimizing-frame chain. The retired corpus carried three parallel `RTree` wrappers (cloud metrics, ICP correspondence, power-diagram seeding) and a fourth in the analysis layer — all four collapse onto this substrate; `cloud.md` metric rows, `register.md` correspondences, `sample.md`/`mesh.md` seeding, and the `Analysis/query.md` search/overlap/point-pair cases are consumers of these folds, never re-implementations.

This page ABSORBS the retired analysis spatial family — `SpatialIndex` (Points/PointCloud/MeshFaces/FromBounds), `SpatialProbe` (Nearest/Within), `SpatialHit`/`SpatialPair` — as query modalities: box and sphere searches are `NeighborQuery` cases, tree-against-tree overlap is a case, probe-driven point-pairs are a case, and the hit/pair carriers land as `NeighborHit`/`NeighborPair`. It also owns RMF GENERATION: `atoms.md` `VectorFrame.Chain` DELEGATES to `NeighborKernel.BishopChain` here — one Wang double-reflection body in the corpus (`Direction.ParallelTransport` is the atoms-owned APPLICATION fold over caller-supplied frames, a different altitude that carries no reflection body). The settled `Rasm.Spatial` index (first-principles SAH-BVH + Morton octree over primitive AABBs) is a DIFFERENT altitude by standing decision: it owns predicate-exact primitive broad-phase; this substrate owns the Rhino-native and static-point neighborhood tier — the two coexist under the host-capture law and neither re-implements the other.

## [01]-[INDEX]

- [02]-[NEIGHBOR_INDEX]: the five-case index union; the query algebra; hit/pair carriers; the batch kNN/radius graph with its receipt.
- [03]-[NEIGHBORHOOD_FOLDS]: policy; PCA samples; normal estimation + MST orientation; principal curvature + classification.
- [04]-[BISHOP_CHAIN]: the one rotation-minimizing-frame owner.

## [02]-[NEIGHBOR_INDEX]

- Owner: `NeighborIndex` `[Union]` — `Cloud` (a `VectorCloud.ClusterCase` riding its lazy native `PointCloud` index), `Points` (`RTree.CreateFromPointArray` over an admitted `Point3d[]`), `MeshFaces` (`RTree.CreateMeshFaceTree` — face-id hits), `Bounds` (an `RTree` built by `Insert(box, elementId)` over admitted `BoundingBox` extents), `Static` (a `SuperClusterKDTree.KDTree<double, double, int>` built once over an immutable point set — the exact-kNN tier for repeated queries over a frozen cloud, `register.md`'s per-iteration correspondence backend). `NeighborQuery` `[Union]` — `Nearest(int K)` / `Radius(PositiveMagnitude R, Option<Dimension> Cap)` / `Box(BoundingBox)` / `Ball(Sphere)` / `Overlaps(NeighborIndex Other, double Tolerance)` / `Pairs(Seq<Point3d> Needles, NeighborQuery Probe)`. `NeighborHit(int Id)` and `NeighborPair(int A, int B)` are the id carriers.
- Entry: `public static Fin<NeighborIndex> Of(NeighborSource source, Op? key = null)` — one admitting factory over a `NeighborSource` `[Union]` (cluster / points / mesh / bounds / static-points) so index species is a case value, never five factory names; `internal Fin<NeighborAnswer> Query(NeighborQuery query, Point3d anchor, Op key, CancellationToken cancel = default)` — the ONE dispatch: `anchor` is read only by the `Nearest`/`Radius` arms (volume, overlap, and pair cases carry their own geometry and ignore it), and `cancel` is the cooperative token the callback capsule rides (`Analysis/query.md` pre-gates on `Env` cancellation today and threads the token when mid-traversal cancel matters; `default` elsewhere). `NeighborAnswer` `[Union]` carries `Hits(Seq<NeighborHit> Values)` / `PairsFound(Seq<NeighborPair> Values)` / `Graph(NeighborhoodGraph Value)` — case and field names are the `Analysis/query.md` `ProjectAnswer` binding, frozen by that consumer.
- Auto: the native search cases run inside the ONE callback capsule — `RTree.Search(box|sphere, callback)` and `RTree.SearchOverlaps(treeA, treeB, tolerance, callback)` mutate a caller-owned buffer through an `EventHandler<RTreeEventArgs>` that reads `args.Id`/`args.IdB` and sets `args.Cancel` from the cooperative token; the capsule sorts hits (id order) and pairs (lexicographic) before emission so results are deterministic regardless of tree traversal order; a volume or overlap query over the `Cloud`/`Static` tiers mints its `RTree` (`CreatePointCloudTree`/`CreateFromPointArray`) inside a `Lease<RTree>.Owned` window that dies with the search, while the tree-carrying tiers run on the case-owned handle. Batch kNN/radius over hay×needles routes the static forms — `RTree.Point3dKNeighbors(hayPoints, needlePts, amount)` / `RTree.Point3dClosestPoints(hayPoints, needlePts, limitDistance)` on point arrays, `RTree.PointCloudKNeighbors(pointcloud, needlePts, amount)` / `RTree.PointCloudClosestPoints(pointcloud, needlePts, limitDistance)` on the cloud tier — each an `IEnumerable<int[]>` leased (`as IDisposable`) for the read window; radius batches re-rank by squared distance and truncate to the policy cap; a kNN request clamps `k` to the hay population before the query, so the receipt's `RequestedNeighborCount ≤ InputCount` conservation term holds by construction. The `Static` tier queries `NearestNeighbors(point, k)` / `RadialSearch(center, r², cap)` — the Euclidean metric is SQUARED-distance, so the radius squares at this seam and nowhere else. `Pairs` validates needles, runs the probe per needle, and emits sorted `(needle, source)` pairs — the absorbed point-pair modality; its probe is anchor-driven BY LAW (`Nearest`/`Radius` only — exactly the absorbed `SpatialProbe` two-case vocabulary): a volume, overlap, or nested `Pairs` probe carries its own geometry, ignores the needle, and refuses with `key.InvalidInput()` rather than degenerating into per-needle duplicates or unbounded recursion.
- Receipt: `NeighborhoodGraph(int[][] Ids, NeighborhoodReceipt Receipt)` — the batch answer every per-point fold consumes; `NeighborhoodReceipt` carries input/query/requested counts, the `NeighborSearchBackend` row (`RTreeKnn`/`RTreeRadius`/`KdTreeKnn`/`KdTreeRadius`), radius evidence, self-inclusion, empty/out-of-range/duplicate counts, and min/max/mean returned counts — `IValidityEvidence`, `IsValid` one `ValidityClaim.All` fold declaring the cross-field terms (`RequestedNeighborCount ≤ InputCount`, zero out-of-range, zero duplicates, `RadiusLimited == Radius.IsSome`).
- Packages: RhinoCommon (`RTree` full surface — `CreateFromPointArray`/`CreatePointCloudTree`/`CreateMeshFaceTree`/`Insert`/`Search`/`SearchOverlaps`/`Point3dKNeighbors`/`Point3dClosestPoints`/`PointCloudKNeighbors`/`PointCloudClosestPoints`, `RTreeEventArgs.Id`/`IdB`/`Cancel`), Supercluster.KDTree.Net (`SuperClusterKDTree.KDTree` — assembly `KDTree.dll`, namespace `SuperClusterKDTree`, `KDTree.Create(points, payloads, DistanceMetrics.EuclideanDistance)`, `NearestNeighbors`, `RadialSearch`; build-once immutable, rebuild on point-set change), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new index species is one `NeighborIndex` case + one `NeighborSource` case + its query arms; a new query shape is one `NeighborQuery` case + one dispatch arm; a new backend is one `NeighborSearchBackend` row — never a parallel wrapper class per consumer.
- Boundary: the RTree callback capsule is the named platform seam — the mutating buffer, the `args.Cancel` write, and the `IDisposable` lease on the batch enumerable never escape it; a consumer-local `RTree` construction (the retired cloud/align/mesh triplication) is the killed form — every kNN in the corpus reads `NeighborhoodGraph`; the native `RTree` handle a case carries dies WITH the case (finalizer-backed, mirroring the `ClusterCase` index-memo law) — a consumer needing deterministic release wraps the whole index in `Lease<T>.Owned`, and a case-level `Dispose` member is the rejected half-ownership; the kd-tree squared-radius conversion happens ONCE at the `Static` arm and an unsquared radius passed through is the named silent-wrong-result defect; `using SuperClusterKDTree;` is the only correct namespace (the package id is not the namespace); the settled `Rasm.Spatial.SpatialIndex` is never wrapped, seeded, or re-implemented here — primitive AABB broad-phase routes there by standing decision.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using SuperClusterKDTree;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NeighborSearchBackend {
    public static readonly NeighborSearchBackend RTreeKnn = new(key: 0);
    public static readonly NeighborSearchBackend RTreeRadius = new(key: 1);
    public static readonly NeighborSearchBackend KdTreeKnn = new(key: 2);
    public static readonly NeighborSearchBackend KdTreeRadius = new(key: 3);
}

[Union]
public abstract partial record NeighborQuery {
    private NeighborQuery() { }
    public sealed record NearestCase(int K) : NeighborQuery;
    public sealed record RadiusCase(PositiveMagnitude R, Option<Dimension> Cap) : NeighborQuery;
    public sealed record BoxCase(BoundingBox Bounds) : NeighborQuery;
    public sealed record BallCase(Sphere Ball) : NeighborQuery;
    public sealed record OverlapsCase(NeighborIndex Other, double Tolerance) : NeighborQuery;
    public sealed record PairsCase(Seq<Point3d> Needles, NeighborQuery Probe) : NeighborQuery;
    public static Fin<NeighborQuery> Nearest(int k, Op? key = null) =>
        guard(k > 0, key.OrDefault().InvalidInput()).ToFin().Map(_ => (NeighborQuery)new NearestCase(K: k));
    public static Fin<NeighborQuery> Radius(double r, Option<int> cap = default, Op? key = null) =>
        from magnitude in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: r)
        from bound in cap.Match(Some: c => key.OrDefault().AcceptValidated<Dimension>(candidate: c).Map(Some), None: static () => Fin.Succ(Option<Dimension>.None))
        select (NeighborQuery)new RadiusCase(R: magnitude, Cap: bound);
}

[Union]
public abstract partial record NeighborSource {
    private NeighborSource() { }
    public sealed record ClusterCase(VectorCloud.ClusterCase Cloud) : NeighborSource;
    public sealed record PointsCase(Seq<Point3d> Values) : NeighborSource;
    public sealed record MeshCase(Mesh Source) : NeighborSource;
    public sealed record BoundsCase(Seq<BoundingBox> Boxes) : NeighborSource;
    public sealed record StaticCase(Seq<Point3d> Values) : NeighborSource;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborHit(int Id) : IValidityEvidence { public bool IsValid => Id >= 0; }

// A/B span two id spaces (treeA/treeB, needle-ordinal/source) — identity inequality is not an invariant.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborPair(int A, int B) : IValidityEvidence { public bool IsValid => A >= 0 && B >= 0; }

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodReceipt(
    int InputCount, int QueryCount, int RequestedNeighborCount, NeighborSearchBackend SearchBackend,
    bool RadiusLimited, Option<double> Radius, bool SelfNeighborIncluded,
    int EmptyNeighborhoodCount, int OutOfRangeIndexCount, int DuplicateIndexCount,
    int MinReturnedCount, int MaxReturnedCount, double MeanReturnedCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(InputCount >= 0 && QueryCount >= 0 && RequestedNeighborCount >= 0 && EmptyNeighborhoodCount >= 0),
        ValidityClaim.Of(RequestedNeighborCount <= InputCount),
        ValidityClaim.CountExactly(count: OutOfRangeIndexCount, expected: 0),
        ValidityClaim.CountExactly(count: DuplicateIndexCount, expected: 0),
        ValidityClaim.Of(MinReturnedCount >= 0 && MinReturnedCount <= MaxReturnedCount),
        ValidityClaim.Nonnegative(MeanReturnedCount),
        ValidityClaim.Of(Radius.Map(static r => ValidityClaim.Positive(r).Holds).IfNone(true)),
        ValidityClaim.Of(RadiusLimited == Radius.IsSome));
}

public readonly record struct NeighborhoodGraph(int[][] Ids, NeighborhoodReceipt Receipt);

// Case and field names frozen by Analysis/query.md ProjectAnswer: Hits.Values / PairsFound.Values.
[Union]
public abstract partial record NeighborAnswer {
    private NeighborAnswer() { }
    public sealed record Hits(Seq<NeighborHit> Values) : NeighborAnswer;
    public sealed record PairsFound(Seq<NeighborPair> Values) : NeighborAnswer;
    public sealed record Graph(NeighborhoodGraph Value) : NeighborAnswer;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record NeighborIndex {
    private NeighborIndex() { }
    public sealed record CloudCase(VectorCloud.ClusterCase Source) : NeighborIndex;
    public sealed record PointsCase(Point3d[] Hay, RTree Tree) : NeighborIndex;
    public sealed record MeshFacesCase(Mesh Source, RTree Tree) : NeighborIndex;
    public sealed record BoundsCase(RTree Tree, int Count) : NeighborIndex;
    public sealed record StaticCase(KDTree<double, double, int> Tree, Point3d[] Points) : NeighborIndex;

    public static Fin<NeighborIndex> Of(NeighborSource source, Op? key = null) {
        Op op = key.OrDefault();
        return source.Switch(
            state: op,
            clusterCase: static (k, c) => Fin.Succ((NeighborIndex)new CloudCase(Source: c.Cloud)),
            pointsCase: static (k, p) =>
                from hay in p.Values.TraverseM(v => k.AcceptValue(value: v)).As().Map(static vs => vs.ToArray())
                from tree in Optional(RTree.CreateFromPointArray(points: hay)).ToFin(k.InvalidResult())
                select (NeighborIndex)new PointsCase(Hay: hay, Tree: tree),
            meshCase: static (k, m) =>
                from valid in guard(m.Source.IsValid, k.InvalidInput())
                from tree in Optional(RTree.CreateMeshFaceTree(mesh: m.Source)).ToFin(k.InvalidResult())
                select (NeighborIndex)new MeshFacesCase(Source: m.Source, Tree: tree),
            boundsCase: static (k, b) => b.Boxes
                .Map(static (box, index) => (Box: box, Index: index))
                .Fold(Fin.Succ(new RTree()), (acc, item) => acc.Bind(tree =>
                    item.Box.IsValid && tree.Insert(box: item.Box, elementId: item.Index)
                        ? Fin.Succ(tree)
                        : new Lease<RTree>.Owned(Value: tree).Use(_ => Fin.Fail<RTree>(k.InvalidResult()))))
                .Map(tree => (NeighborIndex)new BoundsCase(Tree: tree, Count: b.Boxes.Count)),
            staticCase: static (k, s) =>
                from points in s.Values.TraverseM(v => k.AcceptValue(value: v)).As().Map(static vs => vs.ToArray())
                let coordinates = points.Select(IReadOnlyList<double> (p) => [p.X, p.Y, p.Z]).ToArray()
                let payloads = Enumerable.Range(0, points.Length).ToArray()
                select (NeighborIndex)new StaticCase(
                    Tree: KDTree.Create(coordinates, payloads, DistanceMetrics.EuclideanDistance), Points: points));
    }

    // anchor: read by Nearest/Radius arms only; cancel: rides args.Cancel inside the capsule (default = never).
    internal Fin<NeighborAnswer> Query(NeighborQuery query, Point3d anchor, Op key, CancellationToken cancel = default) {
        NeighborIndex self = this;
        return cancel.IsCancellationRequested
            ? Fin.Fail<NeighborAnswer>(error: new Fault.Cancelled())
            : query.Switch(
                state: (Self: self, Anchor: anchor, Key: key, Cancel: cancel),
                nearestCase: static (s, q) =>
                    from _ in s.Key.AcceptValue(value: s.Anchor)
                    from graph in NeighborKernel.GraphOf(index: s.Self, needles: [s.Anchor], count: Some(q.K), radius: Option<double>.None, key: s.Key)
                    select (NeighborAnswer)new NeighborAnswer.Graph(Value: graph),
                radiusCase: static (s, q) =>
                    from _ in s.Key.AcceptValue(value: s.Anchor)
                    from graph in NeighborKernel.GraphOf(index: s.Self, needles: [s.Anchor], count: q.Cap.Map(static c => c.Value), radius: Some(q.R.Value), key: s.Key)
                    select (NeighborAnswer)new NeighborAnswer.Graph(Value: graph),
                boxCase: static (s, q) =>
                    from _ in guard(q.Bounds.IsValid, s.Key.InvalidInput()).ToFin()
                    from hits in s.Self.WithTree(key: s.Key, run: tree => SearchCapsule<NeighborHit>(
                        run: buffer => tree.Search(box: q.Bounds, callback: (sender, args) => { buffer.Add(new NeighborHit(Id: args.Id)); args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.Id.CompareTo(right.Id), cancel: s.Cancel, key: s.Key))
                    select (NeighborAnswer)new NeighborAnswer.Hits(Values: hits),
                ballCase: static (s, q) =>
                    from _ in guard(q.Ball.IsValid, s.Key.InvalidInput()).ToFin()
                    from hits in s.Self.WithTree(key: s.Key, run: tree => SearchCapsule<NeighborHit>(
                        run: buffer => tree.Search(sphere: q.Ball, callback: (sender, args) => { buffer.Add(new NeighborHit(Id: args.Id)); args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.Id.CompareTo(right.Id), cancel: s.Cancel, key: s.Key))
                    select (NeighborAnswer)new NeighborAnswer.Hits(Values: hits),
                overlapsCase: static (s, q) =>
                    from _ in guard(double.IsFinite(q.Tolerance) && q.Tolerance >= 0.0, s.Key.InvalidInput()).ToFin()
                    from pairs in s.Self.WithTree(key: s.Key, run: mine => q.Other.WithTree(key: s.Key, run: theirs => SearchCapsule<NeighborPair>(
                        run: buffer => RTree.SearchOverlaps(treeA: mine, treeB: theirs, tolerance: q.Tolerance,
                            callback: (sender, args) => { buffer.Add(new NeighborPair(A: args.Id, B: args.IdB)); args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.A != right.A ? left.A.CompareTo(right.A) : left.B.CompareTo(right.B), cancel: s.Cancel, key: s.Key)))
                    select (NeighborAnswer)new NeighborAnswer.PairsFound(Values: pairs),
                pairsCase: static (s, q) =>
                    from needles in q.Needles.TraverseM(v => s.Key.AcceptValue(value: v)).As().Map(static vs => vs.ToArray())
                    from graph in q.Probe switch {
                        // The pair-probe refusal LAW (Analysis/query.md defers here): only the anchor-driven probes
                        // ride the needle; a volume/overlap/nested-Pairs probe carries its own geometry and refuses.
                        NeighborQuery.NearestCase n => NeighborKernel.GraphOf(index: s.Self, needles: needles, count: Some(n.K), radius: Option<double>.None, key: s.Key),
                        NeighborQuery.RadiusCase r => NeighborKernel.GraphOf(index: s.Self, needles: needles, count: r.Cap.Map(static c => c.Value), radius: Some(r.R.Value), key: s.Key),
                        _ => Fin.Fail<NeighborhoodGraph>(s.Key.InvalidInput()),
                    }
                    let pairs = toSeq(graph.Ids.SelectMany(static (row, needle) => row.Select(id => new NeighborPair(A: needle, B: id)))
                        .OrderBy(static p => p.A).ThenBy(static p => p.B))
                    select (NeighborAnswer)new NeighborAnswer.PairsFound(Values: pairs));
    }

    // Tree window: held tiers run on the case-owned RTree; Cloud/Static mint one for the search and it dies
    // with the lease — the case-level Dispose stays the rejected half-ownership.
    private Fin<TOut> WithTree<TOut>(Op key, Func<RTree, Fin<TOut>> run) => Switch(
        state: (Key: key, Run: run),
        cloudCase: static (s, c) => Optional(RTree.CreatePointCloudTree(cloud: c.Source.Indexed)).ToFin(s.Key.InvalidResult())
            .Bind(tree => new Lease<RTree>.Owned(Value: tree).Use(s.Run)),
        pointsCase: static (s, p) => s.Run(p.Tree),
        meshFacesCase: static (s, m) => s.Run(m.Tree),
        boundsCase: static (s, b) => s.Run(b.Tree),
        staticCase: static (s, t) => Optional(RTree.CreateFromPointArray(points: t.Points)).ToFin(s.Key.InvalidResult())
            .Bind(tree => new Lease<RTree>.Owned(Value: tree).Use(s.Run)));

    // The callback capsule: the ONE mutating-buffer seam every native search runs through; Query's token sources it.
    private static Fin<Seq<TItem>> SearchCapsule<TItem>(Func<List<TItem>, bool> run, Comparison<TItem> order, CancellationToken cancel, Op key) {
        List<TItem> buffer = [];
        bool completed = run(buffer);
        buffer.Sort(comparison: order);
        return (completed, cancel.IsCancellationRequested) switch {
            (_, true) => Fin.Fail<Seq<TItem>>(error: new Fault.Cancelled()),
            (true, _) => Fin.Succ(toSeq(buffer)),
            _ => Fin.Fail<Seq<TItem>>(error: key.InvalidResult()),
        };
    }
}
```

## [03]-[NEIGHBORHOOD_FOLDS]

- Owner: `NeighborhoodPolicy` (`NeighborCount: Dimension`, `Radius: Option<PositiveMagnitude>`, `EigenGapTolerance`, `FitResidualTolerance`, `SphereLikenessBand: UnitInterval` — the ONE policy record every neighborhood fold threads; `Default(key)` derives the canonical row: 10 neighbors, no radius, `1e-8` gap, `1e-4` residual, `0.35` band); the `NeighborKernel` static operation surface (the `CloudKernel`/`MeshKernel` family name).
- Entry: `NeighborKernel.GraphOf(index, needles, policy, key) → Fin<NeighborhoodGraph>` (the batch spine every fold reads; its raw `count`/`radius` overload is the `Query` Nearest/Radius/Pairs altitude — one body, no policy floor); `PcaOf(cluster, policy, key) → Fin<NeighborhoodPcaResult>`; `EstimateNormals(cluster, policy, key) → Fin<Vector3d[]>` (plus the graph-threaded internal overload the orientation fold reuses); `OrientNormals(cluster, policy, key) → Fin<Seq<Vector3d>>`; `PrincipalCurvatures(cluster, policy, key) → Fin<CurvatureResult>`; `Curvedness`/`ShapeIndex` scalar projections; `ReceiptOf(cluster, policy, key) → Fin<NeighborhoodReceipt>`.
- Auto: per-point PCA reads the graph row, folds the neighborhood through `CloudKernel.CovarianceOf` → `DecomposeEigen`, clamps eigenvalues to the floor `max(EigenGapTolerance, εₘ½)`, and emits `NeighborhoodPcaSample` (point, neighbor count, reconstituted covariance, normal = third eigenvector, raw/clamped spectra, rank, clamp count) — the GICP precision-field input `register.md` consumes. Normal ESTIMATION is the PCA normal gated on the eigen gap; normal ORIENTATION is Hoppe-DeRose over the kNN graph mined through QuikGraph — build one `UndirectedGraph<int, SEdge<int>>` from the graph rows, take `MinimumSpanningTreePrim(edgeWeights: e => 1.0 − |n[e.Source]·n[e.Target]|)`, and propagate sign by BFS over the MST adjacency flipping any child whose dot against its parent is negative; forest roots (disconnected clusters) each orient independently. The hand-rolled O(n²) Prim scan the retired source carried is deleted — the MST is the package's. PRINCIPAL CURVATURE fits the quadric `n ≈ a·u² + b·uv + c·v² + d·u + e·v + f` in the PCA tangent frame through `matrix.md` `Matrix.LeastSquaresDetailed` (six columns, full-rank + finite-residual gated), reads the shape operator `[[2a, b],[b, 2c]]` through `SymmetricMatrix.DecomposeEigen`, lifts eigenvectors back through the tangent axes, and classifies: per-sample attempts partition into rank-rejected / residual-rejected / accepted; `Curvedness = √((k₁²+k₂²)/2)`; `ShapeIndex = (2/π)·atan2(k₁+k₂, k₁−k₂)` (Koenderink-van Doorn, sign-collapsed at the umbilic floor); the range fold buckets samples plane/sphere/saddle/mixed under the tolerance with the sphere-likeness band as a policy column (default `0.35` of the dominant magnitude), and the whole-cloud `CurvatureRangeKind` is unanimous-or-`Mixed`.
- Receipt: `NeighborhoodPcaReceipt` (counts, rank/eigen clamp evidence, floor, nested `NeighborhoodReceipt`); `CurvatureReceipt` (counts, rank/residual rejection split, mean/max residual, tolerances, nested neighborhood + range receipts); `CurvatureRangeReceipt` (bucket counts + k₁/k₂/Gaussian/mean/shape-index extents + tolerance); each `IValidityEvidence` with `IsValid` one `ValidityClaim.All` fold declaring its conservation terms (`Accepted + Rejected == Input`, bucket sums, nested-receipt count agreement via `ValidityClaim.Evidence`) once.
- Packages: QuikGraph (`UndirectedGraph<TVertex,TEdge>`, `SEdge<int>`, `AlgorithmExtensions.MinimumSpanningTreePrim(edgeWeights)`), RhinoCommon, LanguageExt.Core.
- Growth: a new per-point measurement is one fold over the SAME `NeighborhoodGraph` spine + its receipt columns; a new classification band is one policy column; a new orientation strategy is one arm beside the MST fold.
- Boundary: every fold reads `GraphOf` — a fold constructing its own tree is the tri-plication this page kills; the quadric solve routes `matrix.md` owners and a raw-MathNet reach here is the named bypass defect; QuikGraph owns the MST and a hand-rolled Prim/Kruskal is the deleted form; the curvature entry gates `NeighborCount >= 6` before any solve (six quadric unknowns — `NeighborhoodPolicy.Admit` floors at 3 for PCA alone); the graph spine refuses the `MeshFaces`/`Bounds` tiers (`Unsupported`) — face and inserted-box ids are not point neighborhoods; eigen clamping is evidence, never silent — clamp counts ride the receipts.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodPolicy(Dimension NeighborCount, Option<PositiveMagnitude> Radius, PositiveMagnitude EigenGapTolerance, PositiveMagnitude FitResidualTolerance, UnitInterval SphereLikenessBand) {
    internal static Fin<NeighborhoodPolicy> Default(Op key) =>
        from count in key.AcceptValidated<Dimension>(candidate: 10)
        from gap in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8)
        from residual in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-4)
        from band in key.AcceptValidated<UnitInterval>(candidate: 0.35)
        select new NeighborhoodPolicy(NeighborCount: count, Radius: None, EigenGapTolerance: gap, FitResidualTolerance: residual, SphereLikenessBand: band);
    internal Fin<NeighborhoodPolicy> Admit(Op key) {
        NeighborhoodPolicy self = this;
        return guard(self.NeighborCount.Value >= 3, key.InvalidInput()).ToFin().Map(_ => self);
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodPcaSample(
    int Index, Point3d Point, int NeighborCount, SymmetricMatrix Covariance, Vector3d Normal,
    Arr<double> RawEigenvalues, Arr<double> ClampedEigenvalues, int Rank, int EigenClampCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Index >= 0 && NeighborCount >= 0 && Rank is >= 0 and <= 3 && EigenClampCount >= 0),
        ValidityClaim.Finite(Point),
        ValidityClaim.Finite(Normal),
        ValidityClaim.Evidence(Covariance),
        ValidityClaim.CountExactly(count: RawEigenvalues.Count, expected: 3),
        ValidityClaim.CountExactly(count: ClampedEigenvalues.Count, expected: 3),
        ValidityClaim.Of(ClampedEigenvalues.ForAll(static v => ValidityClaim.Positive(v).Holds)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodPcaReceipt(
    int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
    int RankClampCount, int EigenClampCount, double EigenClampFloor, NeighborhoodReceipt Neighborhood) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(AcceptedSampleCount >= 0 && RejectedSampleCount >= 0 && RankClampCount >= 0 && EigenClampCount >= 0),
        ValidityClaim.CountExactly(count: AcceptedSampleCount + RejectedSampleCount, expected: InputCount),
        ValidityClaim.Positive(EigenClampFloor),
        ValidityClaim.Evidence(Neighborhood),
        ValidityClaim.CountExactly(count: Neighborhood.InputCount, expected: InputCount));
}

public readonly record struct NeighborhoodPcaResult(Seq<NeighborhoodPcaSample> Samples, NeighborhoodPcaReceipt Receipt);

[SmartEnum<int>]
public sealed partial class CurvatureRangeKind {
    public static readonly CurvatureRangeKind Empty = new(key: 0);
    public static readonly CurvatureRangeKind Plane = new(key: 1);
    public static readonly CurvatureRangeKind Sphere = new(key: 2);
    public static readonly CurvatureRangeKind Saddle = new(key: 3);
    public static readonly CurvatureRangeKind Mixed = new(key: 4);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureSample(
    int Index, Point3d Point, double K1, double K2, Direction E1, Direction E2, double Residual, int NeighborCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Index >= 0),
        ValidityClaim.Finite(Point),
        ValidityClaim.Finite(K1),
        ValidityClaim.Finite(K2),
        ValidityClaim.Of(E1.IsValid && E2.IsValid),
        ValidityClaim.Nonnegative(Residual),
        ValidityClaim.CountAtLeast(count: NeighborCount, floor: 6));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureReceipt(
    int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
    int RankRejectedCount, int ResidualRejectedCount, double MeanResidual, double MaxResidual,
    double EigenGapTolerance, double FitResidualTolerance, double SphereLikenessBand,
    NeighborhoodReceipt Neighborhood, CurvatureRangeReceipt Range) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: AcceptedSampleCount + RejectedSampleCount, expected: InputCount),
        ValidityClaim.CountExactly(count: RankRejectedCount + ResidualRejectedCount, expected: RejectedSampleCount),
        ValidityClaim.Nonnegative(MeanResidual),
        ValidityClaim.Ordered(lower: MeanResidual, upper: MaxResidual),
        ValidityClaim.Positive(EigenGapTolerance),
        ValidityClaim.Positive(FitResidualTolerance),
        ValidityClaim.UnitInterval(SphereLikenessBand),
        ValidityClaim.Evidence(Neighborhood),
        ValidityClaim.Evidence(Range),
        ValidityClaim.CountExactly(count: Range.AcceptedSampleCount, expected: AcceptedSampleCount));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureRangeReceipt(
    int AcceptedSampleCount, CurvatureRangeKind Kind, int PlaneLikeCount, int SphereLikeCount, int SaddleLikeCount, int MixedCount,
    double MinK1, double MaxK1, double MinK2, double MaxK2, double MinGaussian, double MaxGaussian,
    double MinMean, double MaxMean, double MinShapeIndex, double MaxShapeIndex, double Tolerance) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: PlaneLikeCount + SphereLikeCount + SaddleLikeCount + MixedCount, expected: AcceptedSampleCount),
        ValidityClaim.Of(AcceptedSampleCount == 0 || (
            ValidityClaim.Ordered(lower: MinK1, upper: MaxK1).Holds
            && ValidityClaim.Ordered(lower: MinK2, upper: MaxK2).Holds
            && ValidityClaim.Ordered(lower: MinGaussian, upper: MaxGaussian).Holds
            && ValidityClaim.Ordered(lower: MinMean, upper: MaxMean).Holds
            && ValidityClaim.Ordered(lower: MinShapeIndex, upper: MaxShapeIndex).Holds)),
        ValidityClaim.Nonnegative(Tolerance));
}

public readonly record struct CurvatureResult(Seq<CurvatureSample> Samples, CurvatureReceipt Receipt);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class NeighborKernel {
    internal static Fin<NeighborhoodGraph> GraphOf(NeighborIndex index, Point3d[] needles, NeighborhoodPolicy policy, Op key) =>
        policy.Admit(key: key).Bind(admitted => GraphOf(index: index, needles: needles,
            count: Some(admitted.NeighborCount.Value), radius: admitted.Radius.Map(static r => r.Value), key: key));

    // The ONE hay×needles batch body: kNN when radius is None (k pre-clamped to the hay population so the
    // receipt conservation holds by construction), radius otherwise (re-ranked by squared distance,
    // cap-truncated). Query's Nearest/Radius/Pairs arms ride this raw altitude; the folds ride the policy overload.
    internal static Fin<NeighborhoodGraph> GraphOf(NeighborIndex index, Point3d[] needles, Option<int> count, Option<double> radius, Op key) =>
        from gate in guard(needles.Length > 0
            && count.Map(static k => k > 0).IfNone(true)
            && radius.Map(static r => double.IsFinite(r) && r > 0.0).IfNone(count.IsSome), key.InvalidInput()).ToFin()
        from graph in index.Switch(
            state: (Needles: needles, Count: count, Radius: radius, Key: key),
            cloudCase: static (s, c) => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: c.Source.Vertices.Count, hayAt: i => c.Source.Vertices[i],
                knnBackend: NeighborSearchBackend.RTreeKnn, radiusBackend: NeighborSearchBackend.RTreeRadius,
                knn: k => RTree.PointCloudKNeighbors(pointcloud: c.Source.Indexed, needlePts: s.Needles, amount: k),
                radial: (r, _) => RTree.PointCloudClosestPoints(pointcloud: c.Source.Indexed, needlePts: s.Needles, limitDistance: r)),
            pointsCase: static (s, p) => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: p.Hay.Length, hayAt: i => p.Hay[i],
                knnBackend: NeighborSearchBackend.RTreeKnn, radiusBackend: NeighborSearchBackend.RTreeRadius,
                knn: k => RTree.Point3dKNeighbors(hayPoints: p.Hay, needlePts: s.Needles, amount: k),
                radial: (r, _) => RTree.Point3dClosestPoints(hayPoints: p.Hay, needlePts: s.Needles, limitDistance: r)),
            // Face and inserted-box ids are not point neighborhoods — the graph spine refuses these tiers.
            meshFacesCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(s.Key.Unsupported(geometryType: typeof(NeighborIndex.MeshFacesCase), outputType: typeof(NeighborhoodGraph))),
            boundsCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(s.Key.Unsupported(geometryType: typeof(NeighborIndex.BoundsCase), outputType: typeof(NeighborhoodGraph))),
            staticCase: static (s, t) => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: t.Points.Length, hayAt: i => t.Points[i],
                knnBackend: NeighborSearchBackend.KdTreeKnn, radiusBackend: NeighborSearchBackend.KdTreeRadius,
                knn: k => s.Needles.Select(needle => t.Tree.NearestNeighbors(point: Coordinate(needle), numNeighbors: k).Select(static hit => hit.Item2).ToArray()),
                // The kd-tree Euclidean metric is SQUARED distance — the radius squares HERE and nowhere else.
                radial: (r, cap) => s.Needles.Select(needle => t.Tree.RadialSearch(center: Coordinate(needle), radius: r * r, numNeighbors: cap).Select(static hit => hit.Item2).ToArray())))
        select graph;

    internal static Fin<Seq<Vector3d>> OrientNormals(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
        from graph in GraphOf(index: new NeighborIndex.CloudCase(Source: cluster), needles: [.. cluster.Vertices.AsIterable()], policy: policy, key: key)
        from normals in EstimateNormals(cluster: cluster, graph: graph, policy: policy, key: key)
        from oriented in key.Catch(() => {
            UndirectedGraph<int, SEdge<int>> knn = new(allowParallelEdges: false);
            _ = knn.AddVertexRange(Enumerable.Range(0, normals.Length));
            foreach ((int[] row, int i) in graph.Ids.Select(static (row, i) => (row, i)))
                foreach (int j in row.Where(j => j >= 0 && j < normals.Length && j != i))
                    _ = knn.AddEdge(new SEdge<int>(i, j));
            // Hoppe-DeRose: MST under weight 1-|n_i.n_j| (QuikGraph Prim), then BFS sign propagation per forest root.
            IEnumerable<SEdge<int>> mst = knn.MinimumSpanningTreePrim(edgeWeights: e => 1.0 - Math.Abs(normals[e.Source] * normals[e.Target]));
            return key.Accept(values: PropagateSigns(normals: normals, mstEdges: mst));
        })
        select oriented;

    internal static Fin<CurvatureResult> PrincipalCurvatures(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
        // Six quadric unknowns — the entry gates NeighborCount >= 6 before any solve (Admit alone floors at 3).
        from _ in guard(policy.NeighborCount.Value >= 6, key.InvalidInput()).ToFin()
        from graph in GraphOf(index: new NeighborIndex.CloudCase(Source: cluster), needles: [.. cluster.Vertices.AsIterable()], policy: policy, key: key)
        from attempts in toSeq(graph.Ids.Select(static (row, index) => (Row: row, Index: index)))
            .TraverseM(vertex => AttemptOf(cluster: cluster, index: vertex.Index, row: vertex.Row, policy: policy, key: key)).As()
        let accepted = attempts.Bind(static a => a.Sample.ToSeq())
        let rankRejected = attempts.Count(static a => a.Sample.IsNone && a.RankRejected)
        let residualRejected = attempts.Count(static a => a.Sample.IsNone && !a.RankRejected)
        let receipt = new CurvatureReceipt(
            InputCount: cluster.Vertices.Count, RequestedNeighborCount: policy.NeighborCount.Value,
            AcceptedSampleCount: accepted.Count, RejectedSampleCount: rankRejected + residualRejected,
            RankRejectedCount: rankRejected, ResidualRejectedCount: residualRejected,
            MeanResidual: accepted.IsEmpty ? 0.0 : accepted.Sum(static s => s.Residual) / accepted.Count,
            MaxResidual: accepted.IsEmpty ? 0.0 : accepted.Max(static s => s.Residual),
            EigenGapTolerance: policy.EigenGapTolerance.Value, FitResidualTolerance: policy.FitResidualTolerance.Value,
            SphereLikenessBand: policy.SphereLikenessBand.Value, Neighborhood: graph.Receipt,
            Range: RangeOf(samples: accepted, band: policy.SphereLikenessBand.Value))
        from result in receipt.IsValid
            ? Fin.Succ(new CurvatureResult(Samples: accepted, Receipt: receipt))
            : Fin.Fail<CurvatureResult>(key.InvalidResult())
        select result;

    internal static Fin<Seq<double>> Curvedness(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
        PrincipalCurvatures(cluster: cluster, policy: policy, key: key)
            .Map(r => r.Samples.Map(static s => Math.Sqrt(0.5 * ((s.K1 * s.K1) + (s.K2 * s.K2)))));
    internal static Fin<Seq<double>> ShapeIndex(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
        PrincipalCurvatures(cluster: cluster, policy: policy, key: key)
            .Map(r => r.Samples.Map(static s => Math.Abs(s.K1 - s.K2) < EpsilonPolicy.SqrtEpsilon
                ? (double)Math.Sign(s.K1 + s.K2)
                : 2.0 / Math.PI * Math.Atan2(s.K1 + s.K2, s.K1 - s.K2)));

    private static Fin<NeighborhoodGraph> Batch(Point3d[] needles, Option<int> count, Option<double> radius, Op key,
        int hayCount, Func<int, Point3d> hayAt, NeighborSearchBackend knnBackend, NeighborSearchBackend radiusBackend,
        Func<int, IEnumerable<int[]>> knn, Func<double, int, IEnumerable<int[]>> radial) =>
        guard(hayCount > 0, key.InvalidInput()).ToFin().Bind(_ => key.Catch(() => {
            int requested = Math.Min(count.IfNone(hayCount), hayCount);
            IEnumerable<int[]> batch = radius.Match(Some: r => radial(r, requested), None: () => knn(requested));
            using IDisposable? window = batch as IDisposable;   // the native batch lease — the read window
            int[][] ids = radius.IsSome
                ? [.. batch.Select((row, i) => row.OrderBy(id => needles[i].DistanceToSquared(hayAt(id))).Take(requested).ToArray())]
                : [.. batch];
            int[] returned = [.. ids.Select(static row => row.Length)];
            NeighborhoodReceipt receipt = new(
                InputCount: hayCount, QueryCount: needles.Length, RequestedNeighborCount: requested,
                SearchBackend: radius.IsSome ? radiusBackend : knnBackend, RadiusLimited: radius.IsSome, Radius: radius,
                SelfNeighborIncluded: needles.Length == hayCount && ids.Where(static (row, i) => row.Contains(i)).Any(),
                EmptyNeighborhoodCount: returned.Count(static n => n == 0),
                OutOfRangeIndexCount: ids.Sum(row => row.Count(id => id < 0 || id >= hayCount)),
                DuplicateIndexCount: ids.Sum(static row => row.Length - row.Distinct().Count()),
                MinReturnedCount: returned.Min(), MaxReturnedCount: returned.Max(), MeanReturnedCount: returned.Average());
            return ids.Length == needles.Length && receipt.IsValid
                ? Fin.Succ(new NeighborhoodGraph(Ids: ids, Receipt: receipt))
                : Fin.Fail<NeighborhoodGraph>(key.InvalidResult());
        }));

    private static Fin<QuadricAttempt> AttemptOf(VectorCloud.ClusterCase cluster, int index, int[] row, NeighborhoodPolicy policy, Op key) =>
        row.Length < 6
            // Fewer than six equations can never be full-rank for the six quadric unknowns.
            ? Fin.Succ(new QuadricAttempt(Sample: None, RankRejected: true))
            : from stats in CloudKernel.CovarianceOf(points: toSeq(row.Select(id => cluster.Vertices[id])), mass: Option<Arr<double>>.None, key: key)
              from eigen in stats.Cov.DecomposeEigen(key: key)   // |λ| descending: [0]/[1] span the tangent, [2] is the normal
              let frame = (U: AxisOf(eigen[0].Eigenvector), V: AxisOf(eigen[1].Eigenvector), N: AxisOf(eigen[2].Eigenvector))
              let center = cluster.Vertices[index]
              let local = row.Select(id => cluster.Vertices[id] - center).Select(d => (U: d * frame.U, V: d * frame.V, N: d * frame.N)).ToArray()
              from rows in key.AcceptValidated<Dimension>(candidate: local.Length)
              from cols in key.AcceptValidated<Dimension>(candidate: 6)
              from design in Matrix.Of(rows: rows, cols: cols, entries: new Arr<double>([.. local.SelectMany(static q => (double[])[q.U * q.U, q.U * q.V, q.V * q.V, q.U, q.V, 1.0])]), key: key)
              from attempt in design.LeastSquaresDetailed(rhs: new Arr<double>([.. local.Select(static q => q.N)]), key: key).Match(
                  Succ: fit => !fit.Stop.IsUsable
                      ? Fin.Succ(new QuadricAttempt(Sample: None, RankRejected: true))
                      : fit.Residual > policy.FitResidualTolerance.Value
                          ? Fin.Succ(new QuadricAttempt(Sample: None, RankRejected: false))
                          : SampleOf(index: index, point: center, frame: (frame.U, frame.V), fit: fit, neighborCount: row.Length, context: cluster.Tolerance, key: key)
                              .Map(static sample => new QuadricAttempt(Sample: Some(sample), RankRejected: false)),
                  // A refused solve (degenerate neighborhood, non-finite garbage) partitions, never aborts the cloud.
                  Fail: _ => Fin.Succ(new QuadricAttempt(Sample: None, RankRejected: true)))
              select attempt;

    // Shape operator [[2a, b],[b, 2c]] of the fitted quadric; eigenpairs re-ordered by VALUE (k1 ≥ k2 —
    // Koenderink), axes lifted through the PCA tangent basis and admitted as Directions.
    private static Fin<CurvatureSample> SampleOf(int index, Point3d point, (Vector3d U, Vector3d V) frame, SolveReceipt fit, int neighborCount, Context context, Op key) =>
        from dim in key.AcceptValidated<Dimension>(candidate: 2)
        from shape in SymmetricMatrix.Of(dim: dim, upper: new Arr<double>([2.0 * fit.Solution[0], fit.Solution[1], 2.0 * fit.Solution[2]]), key: key)
        from pairs in shape.DecomposeEigen(key: key)
        let ordered = pairs[0].Eigenvalue >= pairs[1].Eigenvalue ? (Max: pairs[0], Min: pairs[1]) : (Max: pairs[1], Min: pairs[0])
        from e1 in Direction.Of(value: (ordered.Max.Eigenvector[0] * frame.U) + (ordered.Max.Eigenvector[1] * frame.V), context: context, key: key)
        from e2 in Direction.Of(value: (ordered.Min.Eigenvector[0] * frame.U) + (ordered.Min.Eigenvector[1] * frame.V), context: context, key: key)
        select new CurvatureSample(Index: index, Point: point, K1: ordered.Max.Eigenvalue, K2: ordered.Min.Eigenvalue, E1: e1, E2: e2, Residual: fit.Residual, NeighborCount: neighborCount);

    // Range classification: the zero-curvature floor is EpsilonPolicy.SqrtEpsilon (the ShapeIndex umbilic
    // floor); sphere-likeness is |k1−k2| within the policy band of the dominant magnitude.
    private static CurvatureRangeReceipt RangeOf(Seq<CurvatureSample> samples, double band) {
        Seq<CurvatureRangeKind> kinds = samples.Map(s => ClassOf(sample: s, band: band));
        (int plane, int sphere, int saddle) = (
            kinds.Count(static k => k.Equals(CurvatureRangeKind.Plane)),
            kinds.Count(static k => k.Equals(CurvatureRangeKind.Sphere)),
            kinds.Count(static k => k.Equals(CurvatureRangeKind.Saddle)));
        double MinOf(Func<CurvatureSample, double> f) => samples.IsEmpty ? 0.0 : samples.Min(f);
        double MaxOf(Func<CurvatureSample, double> f) => samples.IsEmpty ? 0.0 : samples.Max(f);
        static double ShapeOf(CurvatureSample s) => Math.Abs(s.K1 - s.K2) < EpsilonPolicy.SqrtEpsilon
            ? (double)Math.Sign(s.K1 + s.K2)
            : 2.0 / Math.PI * Math.Atan2(s.K1 + s.K2, s.K1 - s.K2);
        return new CurvatureRangeReceipt(
            AcceptedSampleCount: samples.Count,
            Kind: samples.IsEmpty ? CurvatureRangeKind.Empty
                : plane == samples.Count ? CurvatureRangeKind.Plane
                : sphere == samples.Count ? CurvatureRangeKind.Sphere
                : saddle == samples.Count ? CurvatureRangeKind.Saddle
                : CurvatureRangeKind.Mixed,
            PlaneLikeCount: plane, SphereLikeCount: sphere, SaddleLikeCount: saddle, MixedCount: samples.Count - plane - sphere - saddle,
            MinK1: MinOf(static s => s.K1), MaxK1: MaxOf(static s => s.K1),
            MinK2: MinOf(static s => s.K2), MaxK2: MaxOf(static s => s.K2),
            MinGaussian: MinOf(static s => s.K1 * s.K2), MaxGaussian: MaxOf(static s => s.K1 * s.K2),
            MinMean: MinOf(static s => 0.5 * (s.K1 + s.K2)), MaxMean: MaxOf(static s => 0.5 * (s.K1 + s.K2)),
            MinShapeIndex: MinOf(ShapeOf), MaxShapeIndex: MaxOf(ShapeOf), Tolerance: EpsilonPolicy.SqrtEpsilon);
    }

    private static CurvatureRangeKind ClassOf(CurvatureSample sample, double band) =>
        Math.Abs(sample.K1) <= EpsilonPolicy.SqrtEpsilon && Math.Abs(sample.K2) <= EpsilonPolicy.SqrtEpsilon ? CurvatureRangeKind.Plane
        : Math.Abs(sample.K1 - sample.K2) <= band * Math.Max(Math.Abs(sample.K1), Math.Abs(sample.K2)) ? CurvatureRangeKind.Sphere
        : sample.K1 > EpsilonPolicy.SqrtEpsilon && sample.K2 < -EpsilonPolicy.SqrtEpsilon ? CurvatureRangeKind.Saddle
        : CurvatureRangeKind.Mixed;

    private static Vector3d AxisOf(Arr<double> eigenvector) => new(x: eigenvector[0], y: eigenvector[1], z: eigenvector[2]);
    private static IReadOnlyList<double> Coordinate(Point3d point) => [point.X, point.Y, point.Z];

    private readonly record struct QuadricAttempt(Option<CurvatureSample> Sample, bool RankRejected);
}
```

## [04]-[BISHOP_CHAIN]

- Owner: `NeighborKernel.BishopChain` — the ONE rotation-minimizing-frame body (Wang et al. double reflection) in the corpus: `atoms.md` `VectorFrame.Chain` delegates here (the atoms fence binds `NeighborKernel.BishopChain` by name); the `cloud.md` `BishopFrames` metric row names it; `Parametric/projections.md` curve-frame sweeps compose it. `atoms.md` `Direction.ParallelTransport(Seq<Plane>)` transports a direction through frames a CALLER supplies — the application fold, never a second generator.
- Entry: `internal static Fin<Seq<Plane>> BishopChain(VectorCloud cloud, Op key)` — ring case seeds from the oriented ring normal, polyline case from `VectorFrame.SeedPerpendicular` on the first tangent, cluster case refuses (`Unsupported` — a cluster has no chain order); and the point-form `BishopChain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op key)`.
- Auto: the chain seeds an initial frame (tangent-orthogonalized seed normal), then folds each step through the double reflection — reflect the previous reference and tangent across the chord bisector plane, reflect again across the new tangent's bisector — which is the discretely rotation-minimizing transport; degenerate segments reuse the prior tangent, tiny reflection axes pass the vector through unchanged. Closed chains redistribute the holonomy: the angular defect between the transported final frame and the seed frame spreads as `−residual·i/count` per frame about each local tangent, so the closed chain meets itself with zero twist seam.
- Receipt: none — the chain is a pure fold; a degenerate chain faults with the step's evidence.
- Growth: a new transport flavor (e.g. frame interpolation weights) is a policy argument on the fold, never a second chain body.
- Boundary: this is THE RMF generator — the retired corpus carried the double-reflection body in the cloud kernel while the atoms file re-derived transport frames per call; the generator collapses here, `VectorFrame.Chain` keeps only the delegating member, and `Direction.ParallelTransport` stays the atoms-owned application fold over given frames; per-frame construction admits through `VectorFrame.Of` so every emitted plane is an orthonormal admitted frame, never a raw plane assembly.

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]           | [OWNER]                                    | [KIND]                                                      | [RAIL]                                        | [CASES] |
| :-----: | :----------------------- | :----------------------------------------- | :----------------------------------------------------------- | :--------------------------------------------- | :-----: |
|  [01]   | Index species            | `NeighborIndex`                            | `[Union]` Cloud/Points/MeshFaces/Bounds/Static                | `Of → Fin<NeighborIndex>`                      |    5    |
|  [02]   | Query modality           | `NeighborQuery`                            | `[Union]` Nearest/Radius/Box/Ball/Overlaps/Pairs              | `Query → Fin<NeighborAnswer>`                  |    6    |
|  [03]   | Batch neighborhood spine | `NeighborKernel.GraphOf`                   | hay×needles kNN/radius fold + receipt                         | `→ Fin<NeighborhoodGraph>`                     |    —    |
|  [04]   | Per-point PCA            | `NeighborhoodPcaSample`/`Result`/`Receipt` | clamped-spectrum evidence rows over the graph spine           | `PcaOf → Fin<NeighborhoodPcaResult>`           |    —    |
|  [05]   | Normal orientation       | `NeighborKernel.OrientNormals`             | QuikGraph Prim MST + BFS sign propagation                     | `→ Fin<Seq<Vector3d>>`                         |    —    |
|  [06]   | Principal curvature      | `CurvatureResult`/`CurvatureRangeKind`     | quadric fit + Koenderink classification + range fold          | `PrincipalCurvatures → Fin<CurvatureResult>`   |    5    |
|  [07]   | Rotation-minimizing frame| `NeighborKernel.BishopChain`               | Wang double reflection + closed-chain twist redistribution    | `→ Fin<Seq<Plane>>`                            |    —    |
