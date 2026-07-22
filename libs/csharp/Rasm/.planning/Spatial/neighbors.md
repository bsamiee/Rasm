# [RASM_NEIGHBORS]

`NeighborIndex` and `NeighborKernel` own the Rhino-native and static-point neighborhood substrate; every proximity consumer routes its index, query, and per-point fold through these owners.

## [01]-[INDEX]

- [02]-[NEIGHBOR_INDEX]: `NeighborIndex` admits every index species and `Query` dispatches the whole algebra onto one answer rail.
- [03]-[NEIGHBORHOOD_FOLDS]: `NeighborKernel` folds PCA, oriented normals, and principal curvature over one batch graph spine.
- [04]-[BISHOP_CHAIN]: `BishopChain` generates every point-chain rotation-minimizing frame.

## [02]-[NEIGHBOR_INDEX]

- Owner: `NeighborIndex` owns every index species as a case; its `Static` kd-tree tier serves exact repeated kNN over a frozen cloud, the `register.md` correspondence backend.
- Entry: `Of` admits every source and `Query` is the one dispatch; `SearchProbe` admits box and sphere validity at the build seam, ahead of execution.
- Auto: `SearchCapsule` owns every native search and sorts hits and pairs before emission, keeping a result deterministic regardless of tree traversal order.
- Packages: RhinoCommon (`RTree`), Supercluster.KDTree.Net (`KDTree`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new index species is one `NeighborIndex` case with its `NeighborSource` case and query arms; a new query is one `NeighborQuery` case and dispatch arm; a new backend is one `NeighborSearchBackend` row.
- Boundary: `SearchCapsule` confines every platform mutation and native lease; every kNN in the corpus reads `NeighborhoodGraph`, and deterministic index release wraps the index in `Lease<T>.Owned`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
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

    internal Fin<(NeighborQuery Query, Point3d Anchor)> SearchProbe(Op key) => this switch {
        BoxCase { Bounds: var bounds } when bounds.IsValid => Fin.Succ((this, bounds.Center)),
        BallCase { Ball: var ball } when ball.IsValid => Fin.Succ((this, ball.Center)),
        _ => Fin.Fail<(NeighborQuery, Point3d)>(key.InvalidInput()),
    };
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

// A/B span two id spaces (treeA/treeB, needle/source) — A != B is not an invariant.
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

// Case and field names are frozen by the Analysis/query.md ProjectAnswer binding.
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
                        NeighborQuery.NearestCase n => NeighborKernel.GraphOf(index: s.Self, needles: needles, count: Some(n.K), radius: Option<double>.None, key: s.Key),
                        NeighborQuery.RadiusCase r => NeighborKernel.GraphOf(index: s.Self, needles: needles, count: r.Cap.Map(static c => c.Value), radius: Some(r.R.Value), key: s.Key),
                        _ => Fin.Fail<NeighborhoodGraph>(s.Key.InvalidInput()),
                    }
                    let pairs = toSeq(graph.Ids.SelectMany(static (row, needle) => row.Select(id => new NeighborPair(A: needle, B: id)))
                        .OrderBy(static p => p.A).ThenBy(static p => p.B))
                    select (NeighborAnswer)new NeighborAnswer.PairsFound(Values: pairs));
    }

    private Fin<TOut> WithTree<TOut>(Op key, Func<RTree, Fin<TOut>> run) => Switch(
        state: (Key: key, Run: run),
        cloudCase: static (s, c) => c.Source.UseIndex(key: s.Key, project: cloud =>
            Optional(RTree.CreatePointCloudTree(cloud: cloud)).ToFin(s.Key.InvalidResult())
                .Bind(tree => new Lease<RTree>.Owned(Value: tree).Use(s.Run))),
        pointsCase: static (s, p) => s.Run(p.Tree),
        meshFacesCase: static (s, m) => s.Run(m.Tree),
        boundsCase: static (s, b) => s.Run(b.Tree),
        staticCase: static (s, t) => Optional(RTree.CreateFromPointArray(points: t.Points)).ToFin(s.Key.InvalidResult())
            .Bind(tree => new Lease<RTree>.Owned(Value: tree).Use(s.Run)));

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

- Owner: `NeighborKernel` owns every per-point measurement, and `NeighborhoodPolicy` is the one record each fold threads.
- Entry: `GraphOf` is the batch spine; `PcaOf`, `EstimateNormals`, `OrientNormals`, `PrincipalCurvatures`, `Curvedness`, `ShapeIndex`, and `ReceiptOf` fold per point over it.
- Auto: per-point PCA clamps eigenvalues to the floor and emits the sample `register.md` reads as its GICP precision field; normal orientation runs Hoppe-DeRose, propagating sign by BFS per forest root; principal curvature routes its quadric solve to the `matrix.md` owners.
- Packages: QuikGraph (`MinimumSpanningTreePrim`), RhinoCommon, LanguageExt.Core.
- Growth: a new per-point measurement is one fold over the `NeighborhoodGraph` spine with its receipt columns; a new classification band is one policy column; a new orientation strategy is one arm beside the MST fold.

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
internal static partial class NeighborKernel {
    internal static Fin<NeighborhoodGraph> GraphOf(NeighborIndex index, Point3d[] needles, NeighborhoodPolicy policy, Op key) =>
        policy.Admit(key: key).Bind(admitted => GraphOf(index: index, needles: needles,
            count: Some(admitted.NeighborCount.Value), radius: admitted.Radius.Map(static r => r.Value), key: key));

    internal static Fin<NeighborhoodGraph> GraphOf(NeighborIndex index, Point3d[] needles, Option<int> count, Option<double> radius, Op key) =>
        from gate in guard(needles.Length > 0
            && count.Map(static k => k > 0).IfNone(true)
            && radius.Map(static r => double.IsFinite(r) && r > 0.0).IfNone(count.IsSome), key.InvalidInput()).ToFin()
        from graph in index.Switch(
            state: (Needles: needles, Count: count, Radius: radius, Key: key),
            cloudCase: static (s, c) => c.Source.UseIndex(key: s.Key, project: cloud => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: c.Source.Vertices.Count, hayAt: i => c.Source.Vertices[i],
                knnBackend: NeighborSearchBackend.RTreeKnn, radiusBackend: NeighborSearchBackend.RTreeRadius,
                knn: k => RTree.PointCloudKNeighbors(pointcloud: cloud, needlePts: s.Needles, amount: k),
                radial: (r, _) => RTree.PointCloudClosestPoints(pointcloud: cloud, needlePts: s.Needles, limitDistance: r))),
            pointsCase: static (s, p) => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: p.Hay.Length, hayAt: i => p.Hay[i],
                knnBackend: NeighborSearchBackend.RTreeKnn, radiusBackend: NeighborSearchBackend.RTreeRadius,
                knn: k => RTree.Point3dKNeighbors(hayPoints: p.Hay, needlePts: s.Needles, amount: k),
                radial: (r, _) => RTree.Point3dClosestPoints(hayPoints: p.Hay, needlePts: s.Needles, limitDistance: r)),
            meshFacesCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(s.Key.Unsupported(geometryType: typeof(NeighborIndex.MeshFacesCase), outputType: typeof(NeighborhoodGraph))),
            boundsCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(s.Key.Unsupported(geometryType: typeof(NeighborIndex.BoundsCase), outputType: typeof(NeighborhoodGraph))),
            staticCase: static (s, t) => Batch(needles: s.Needles, count: s.Count, radius: s.Radius, key: s.Key,
                hayCount: t.Points.Length, hayAt: i => t.Points[i],
                knnBackend: NeighborSearchBackend.KdTreeKnn, radiusBackend: NeighborSearchBackend.KdTreeRadius,
                knn: k => s.Needles.Select(needle => t.Tree.NearestNeighbors(point: Coordinate(needle), numNeighbors: k).Select(static hit => hit.Item2).ToArray()),
                // KD-tree Euclidean distance is squared; radius squares only here.
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
            IEnumerable<SEdge<int>> mst = knn.MinimumSpanningTreePrim(edgeWeights: e => 1.0 - Math.Abs(normals[e.Source] * normals[e.Target]));
            return key.Accept(values: PropagateSigns(normals: normals, mstEdges: mst));
        })
        select oriented;

    internal static Fin<CurvatureResult> PrincipalCurvatures(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
        // Six quadric unknowns — fewer than six equations can never be full-rank.
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
                  // A refused solve partitions the cloud, never aborts it.
                  Fail: _ => Fin.Succ(new QuadricAttempt(Sample: None, RankRejected: true)))
              select attempt;

    // Shape operator [[2a,b],[b,2c]] of the fitted quadric; eigenpairs order by value (k1 ≥ k2, Koenderink).
    private static Fin<CurvatureSample> SampleOf(int index, Point3d point, (Vector3d U, Vector3d V) frame, SolveReceipt fit, int neighborCount, Context context, Op key) =>
        from dim in key.AcceptValidated<Dimension>(candidate: 2)
        from shape in SymmetricMatrix.Of(dim: dim, upper: new Arr<double>([2.0 * fit.Solution[0], fit.Solution[1], 2.0 * fit.Solution[2]]), key: key)
        from pairs in shape.DecomposeEigen(key: key)
        let ordered = pairs[0].Eigenvalue >= pairs[1].Eigenvalue ? (Max: pairs[0], Min: pairs[1]) : (Max: pairs[1], Min: pairs[0])
        from e1 in Direction.Of(value: (ordered.Max.Eigenvector[0] * frame.U) + (ordered.Max.Eigenvector[1] * frame.V), context: context, key: key)
        from e2 in Direction.Of(value: (ordered.Min.Eigenvector[0] * frame.U) + (ordered.Min.Eigenvector[1] * frame.V), context: context, key: key)
        select new CurvatureSample(Index: index, Point: point, K1: ordered.Max.Eigenvalue, K2: ordered.Min.Eigenvalue, E1: e1, E2: e2, Residual: fit.Residual, NeighborCount: neighborCount);

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

- Owner: `NeighborKernel.BishopChain` mints the one point-chain rotation-minimizing-frame body that `VectorFrame.Chain` delegates to.
- Growth: a new transport flavor is one policy argument on this fold.
- Boundary: every emitted plane admits through `VectorFrame.Of`; `Direction.ParallelTransport` applies caller-supplied frames, and parametric-curve sweeps route `Parametric/curve.md` `PerpendicularFrames`.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class NeighborKernel {
    internal static Fin<Seq<Plane>> BishopChain(VectorCloud cloud, Op key) => cloud.Switch(
        state: key,
        ringCase: static (k, r) =>
            from seed in Direction.Of(value: NewellNormal(vertices: r.Vertices), context: r.Tolerance, key: k)
            from chain in BishopChain(points: r.Vertices, initialNormal: seed, closed: true, context: r.Tolerance, key: k)
            select chain,
        polylineCase: static (k, p) =>
            from _ in guard(p.Vertices.Count >= 2, k.InvalidInput()).ToFin()
            from seed in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: p.Vertices[1] - p.Vertices[0]), context: p.Tolerance, key: k)
            from chain in BishopChain(points: p.Vertices, initialNormal: seed, closed: false, context: p.Tolerance, key: k)
            select chain,
        // A cluster carries no chain order; transport over an unordered set is undefined.
        clusterCase: static (k, _) => Fin.Fail<Seq<Plane>>(k.Unsupported(geometryType: typeof(VectorCloud.ClusterCase), outputType: typeof(Seq<Plane>))));

    internal static Fin<Seq<Plane>> BishopChain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op key) =>
        from _ in guard(points.Count >= 2, key.InvalidInput()).ToFin()
        from columns in key.Catch(() => {
            Point3d[] p = [.. points];
            double floor = context.Absolute.Value * context.Absolute.Value;
            var tangents = new Vector3d[p.Length];
            Vector3d prior = p[1] - p[0];
            for (int i = 0; i < p.Length; i++) {
                Vector3d step = i < p.Length - 1 ? p[i + 1] - p[i] : closed ? p[0] - p[i] : prior;
                tangents[i] = step.IsTiny(context.Absolute.Value) ? prior : step;
                prior = tangents[i];
                _ = tangents[i].Unitize();
            }
            var reference = new Vector3d[p.Length];
            reference[0] = initialNormal.Value - (tangents[0] * (initialNormal.Value * tangents[0]));
            if (!reference[0].Unitize()) {
                reference[0] = VectorFrame.SeedPerpendicular(axis: tangents[0]);
            }
            for (int i = 0; i < p.Length - 1; i++) {
                reference[i + 1] = Transported(reference: reference[i], tangent: tangents[i], next: tangents[i + 1], chord: p[i + 1] - p[i], floor: floor);
            }
            if (closed) {   // holonomy: spread the closing defect as −residual·i/count per tangent, closing with zero twist seam
                Vector3d returned = Transported(reference: reference[^1], tangent: tangents[^1], next: tangents[0], chord: p[0] - p[^1], floor: floor);
                double residual = Math.Atan2(Vector3d.CrossProduct(a: reference[0], b: returned) * tangents[0], reference[0] * returned);
                for (int i = 1; i < p.Length; i++) {
                    _ = reference[i].Rotate(angleRadians: -residual * i / p.Length, rotationAxis: tangents[i]);
                }
            }
            return Fin.Succ((Points: p, Tangents: tangents, References: reference));
        })
        from frames in toSeq(Enumerable.Range(0, columns.Points.Length))
            .TraverseM(i => VectorFrame.Of(origin: columns.Points[i], normal: columns.Tangents[i],
                xHint: Some(columns.References[i]), context: context, key: key).Map(static frame => frame.Value)).As()
        select frames;

    // Double reflection (Wang et al.) — the discretely rotation-minimizing transport.
    private static Vector3d Transported(Vector3d reference, Vector3d tangent, Vector3d next, Vector3d chord, double floor) {
        double c1 = chord * chord;
        (Vector3d rl, Vector3d tl) = c1 <= floor
            ? (reference, tangent)
            : (reference - (2.0 / c1 * (chord * reference) * chord), tangent - (2.0 / c1 * (chord * tangent) * chord));
        Vector3d axis = tl + next;
        double c2 = axis * axis;
        Vector3d transported = c2 <= floor ? rl : rl - (2.0 / c2 * (axis * rl) * axis);
        _ = transported.Unitize();
        return transported;
    }

    // Newell area-vector fold — orientation-true for any simple planar loop.
    private static Vector3d NewellNormal(Seq<Point3d> vertices) {
        Vector3d normal = Vector3d.Zero;
        for (int i = 0; i < vertices.Count; i++) {
            (Point3d a, Point3d b) = (vertices[i], vertices[(i + 1) % vertices.Count]);
            normal += new Vector3d(x: (a.Y - b.Y) * (a.Z + b.Z), y: (a.Z - b.Z) * (a.X + b.X), z: (a.X - b.X) * (a.Y + b.Y));
        }
        return normal;
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
