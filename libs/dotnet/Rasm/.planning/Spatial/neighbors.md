# [RASM_NEIGHBORS]

`NeighborIndex` and `NeighborKernel` own the Rhino-native and frozen-point neighborhood substrate; every proximity consumer routes its index, query, and per-point fold through these owners.

Tolerances arrive from `Domain/context` lanes rather than page literals: the eigen-gap floor reads `ToleranceLane.Svd` and the quadric residual floor `ToleranceLane.Residual`, so a model that widens either widens it here without a second policy. Ring posture crosses as the one `isClosed` discriminant `VectorFrame.Chain` (`Numerics/atoms`) threads down, so every chain fold reads a single declared fact rather than re-deriving it per call.

## [01]-[INDEX]

- [02]-[NEIGHBOR_INDEX]: `NeighborIndex` admits every index species and `Query` dispatches the whole algebra onto one answer type.
- [03]-[NEIGHBORHOOD_FOLDS]: `NeighborKernel` folds PCA, oriented normals, and principal curvature over one batch graph spine.
- [04]-[BISHOP_CHAIN]: `BishopChain` generates every point-chain rotation-minimizing frame.

## [02]-[NEIGHBOR_INDEX]

- Owner: `NeighborIndex` owns every index species as a case; its `PointsCase` kd-tree tier serves exact repeated kNN over a frozen cloud, the `register.md` correspondence backend, and builds its native `RTree` only inside the `Lease<RTree>.Owned` window a box, sphere, or overlap query opens; the distance metric rides the QUERY as a `NeighborMetric` row, and `PointsCase` holds ONE frozen tree per row — built at admission with the row's `DistanceMetrics` bound by `KDTree.Create`, never mutated, so two concurrent queries under different metrics cannot race a shared `Tree.Metric` field — with each row carrying its own search-radius transform as a delegate column rather than a flag the reader re-interprets.
- Entry: `Of` admits every source and `Query` is the one dispatch; box and sphere validity gate inside their executing arms, a `PairsCase` probe narrows through `SwitchPartially` to the graph-producing cases with every other probe refused by the one `@default:` arm, and every admitted bound — nearest count, radius, cap, overlap band — crosses as its value object.
- Auto: `SearchCapsule` owns every native search and sorts hits and pairs before emission, keeping a result deterministic regardless of tree traversal order.
- Exemption: `SearchCapsule`'s `List<TItem>` is the named native-callback buffer — the RTree callbacks append during the platform's own sweep and no persistent carrier can receive them mid-callback; it freezes to `Seq` before it leaves.
- Packages: RhinoCommon (`RTree`), Supercluster.KDTree.Net (`KDTree`, `DistanceMetrics`), LanguageExt.Core, Thinktecture.Runtime.Extensions (`[ValueObject<T>]`/`[ComplexValueObject]` admission, `[SmartEnum]`/`[Union]` vocabularies), BCL inbox (`FrozenDictionary`).
- Growth: a new index species is one `NeighborIndex` case with its `NeighborSource` case and query arms; a new query is one `NeighborQuery` case and dispatch arm; a new coordinate-monotone metric is one `NeighborMetric` row carrying its own radius transform, and the `PointsCase` build folds it into one more frozen tree unasked.
- Boundary: `SearchCapsule` confines every platform mutation and native lease; every kNN in the corpus reads `NeighborhoodGraph`, and deterministic index release wraps the index in `Lease<T>.Owned`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using QuikGraph;
using QuikGraph.Algorithms.MinimumSpanningTree;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Numerics;
using SuperClusterKDTree;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class NeighborMetric {
    public static readonly NeighborMetric Euclidean = new(metric: DistanceMetrics.EuclideanDistance, searchRadius: static r => r * r);
    public static readonly NeighborMetric Manhattan = new(metric: DistanceMetrics.ManhattanDistance, searchRadius: static r => r);
    public static readonly NeighborMetric Chebyshev = new(metric: DistanceMetrics.ChebyshevDistance, searchRadius: static r => r);
    internal DistanceMetrics Metric { get; }
    [UseDelegateFromConstructor] internal partial double SearchRadius(double r);
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record NeighborQuery {
    private NeighborQuery() { }
    public sealed record NearestCase(Dimension Count, NeighborMetric Metric) : NeighborQuery;
    public sealed record RadiusCase(PositiveMagnitude R, Option<Dimension> Cap, NeighborMetric Metric) : NeighborQuery;
    public sealed record BoxCase(BoundingBox Bounds) : NeighborQuery;
    public sealed record BallCase(Sphere Ball) : NeighborQuery;
    public sealed record OverlapsCase(NeighborIndex Other, Tolerance Band) : NeighborQuery;
    public sealed record PairsCase(Seq<Point3d> Needles, NeighborQuery Probe) : NeighborQuery;
    public static Fin<NeighborQuery> Nearest(int k, Option<NeighborMetric> metric = default) =>
        FactoryBridge.Accept<Dimension>(k)
            .Map(count => (NeighborQuery)new NearestCase(count, metric.IfNone(NeighborMetric.Euclidean)));
    public static Fin<NeighborQuery> Radius(double r, Option<int> cap = default, Option<NeighborMetric> metric = default) =>
        from magnitude in FactoryBridge.Accept<PositiveMagnitude>(candidate: r)
        from bound in cap.TraverseM(c => FactoryBridge.Accept<Dimension>(candidate: c)).As()
        select (NeighborQuery)new RadiusCase(R: magnitude, Cap: bound, Metric: metric.IfNone(NeighborMetric.Euclidean));
}

[Union]
public abstract partial record NeighborSource {
    private NeighborSource() { }
    public sealed record ClusterCase(VectorCloud.ClusterCase Cloud) : NeighborSource;
    public sealed record PointsCase(Seq<Point3d> Values) : NeighborSource;
    public sealed record MeshCase(Mesh Source) : NeighborSource;
    public sealed record BoundsCase(Seq<BoundingBox> Boxes) : NeighborSource;
}

[ValueObject<int>(KeyMemberName = "Id", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct NeighborHit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError("NeighborHit id must be >= 0.");
}

[ComplexValueObject]
public sealed partial class NeighborPair {
    public int A { get; }
    public int B { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int a, ref int b) =>
        validationError = a >= 0 && b >= 0 ? null : new ValidationError("NeighborPair ordinals must be >= 0.");
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodCensus(
    int InputCount, int QueryCount, int RequestedNeighborCount, bool UsesKdTree,
    Option<PositiveMagnitude> Radius,
    int EmptyNeighborhoodCount, int OutOfRangeIndexCount, int DuplicateIndexCount,
    Stat<Scalar> Returned) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        InputCount >= 0 && QueryCount >= 0 && RequestedNeighborCount >= 0 && EmptyNeighborhoodCount >= 0,
        RequestedNeighborCount <= InputCount,
        ValidityClaim.CountExactly(count: OutOfRangeIndexCount, expected: 0),
        ValidityClaim.CountExactly(count: DuplicateIndexCount, expected: 0),
        ValidityClaim.Evidence(Some(Returned)),
        ValidityClaim.CountExactly(count: Returned.Count, expected: QueryCount),
        ValidityClaim.Nonnegative(Returned.Minimum.To()));
}

public readonly record struct NeighborhoodGraph(int[][] Ids, NeighborhoodCensus Census);

[Union]
public abstract partial record NeighborAnswer {
    private NeighborAnswer() { }
    public sealed record Hits(Seq<NeighborHit> Values) : NeighborAnswer;
    public sealed record PairsFound(Seq<NeighborPair> Values) : NeighborAnswer;
    public sealed record Graph(NeighborhoodGraph Value) : NeighborAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record NeighborIndex {
    private NeighborIndex() { }
    public sealed record CloudCase(VectorCloud.ClusterCase Source) : NeighborIndex;
    public sealed record PointsCase(
        Point3d[] Points,
        FrozenDictionary<NeighborMetric, KDTree<double, double, int>> Trees) : NeighborIndex;
    public sealed record MeshFacesCase(Mesh Source, RTree Tree) : NeighborIndex;
    public sealed record BoundsCase(RTree Tree, int Count) : NeighborIndex;

    public static Fin<NeighborIndex> Of(NeighborSource source) {
        return source.Switch(
            state: op,
            clusterCase: static (k, c) => Fin.Succ((NeighborIndex)new CloudCase(Source: c.Cloud)),
            pointsCase: static (k, p) =>
                from points in p.Values.TraverseM(v => Acceptance.Value(v)).As().Map(static vs => vs.ToArray())
                from _ in guard(points.Length > 0, new KernelFault.InvalidInput())
                let coordinates = points.Select(IReadOnlyList<double> (v) => [v.X, v.Y, v.Z]).ToArray()
                let payloads = Enumerable.Range(0, points.Length).ToArray()
                from trees in Try.lift(() => Fin.Succ(NeighborMetric.Items.ToFrozenDictionary(
                    static row => row, row => KDTree.Create(coordinates, payloads, row.Metric)))).Run().Bind(static inner => inner)
                select (NeighborIndex)new PointsCase(points, trees),
            meshCase: static (k, m) =>
                from valid in guard(m.Source.IsValid, new KernelFault.InvalidInput())
                from tree in Optional(RTree.CreateMeshFaceTree(mesh: m.Source)).ToFin(new KernelFault.InvalidResult())
                select (NeighborIndex)new MeshFacesCase(Source: m.Source, Tree: tree),
            boundsCase: static (k, b) => b.Boxes
                .Map(static (box, index) => (Box: box, Index: index))
                .Fold(Fin.Succ(new RTree()), (acc, item) => acc.Bind(tree =>
                    item.Box.IsValid && tree.Insert(box: item.Box, elementId: item.Index)
                        ? Fin.Succ(tree)
                        : new Lease<RTree>.Owned(Value: tree).Use(_ => Fin.Fail<RTree>(new KernelFault.InvalidResult()))))
                .Map(tree => (NeighborIndex)new BoundsCase(Tree: tree, Count: b.Boxes.Count)));
    }

    internal Fin<NeighborAnswer> Query(NeighborQuery query, Point3d anchor, CancellationToken cancel = default) {
        NeighborIndex self = this;
        return cancel.IsCancellationRequested
            ? Fin.Fail<NeighborAnswer>(error: Errors.Cancelled)
            : query.Switch(
                state: (Self: self, Anchor: anchor, Cancel: cancel),
                nearestCase: static (s, q) =>
                    from _ in Acceptance.Value(value: s.Anchor)
                    from graph in NeighborKernel.GraphOf(index: s.Self, needles: [s.Anchor], count: Some(q.Count), radius: Option<PositiveMagnitude>.None, metric: Some(q.Metric))
                    select (NeighborAnswer)new NeighborAnswer.Graph(Value: graph),
                radiusCase: static (s, q) =>
                    from _ in Acceptance.Value(value: s.Anchor)
                    from graph in NeighborKernel.GraphOf(index: s.Self, needles: [s.Anchor], count: q.Cap, radius: Some(q.R), metric: Some(q.Metric))
                    select (NeighborAnswer)new NeighborAnswer.Graph(Value: graph),
                boxCase: static (s, q) =>
                    from _ in guard(q.Bounds.IsValid, new KernelFault.InvalidInput()).ToFin()
                    from hits in s.Self.WithTree(run: tree => SearchCapsule<NeighborHit>(
                        run: buffer => tree.Search(box: q.Bounds, callback: (sender, args) => { if (NeighborHit.TryCreate(args.Id, out NeighborHit hit)) { buffer.Add(hit); } args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.Id.CompareTo(right.Id), cancel: s.Cancel))
                    select (NeighborAnswer)new NeighborAnswer.Hits(Values: hits),
                ballCase: static (s, q) =>
                    from _ in guard(q.Ball.IsValid, new KernelFault.InvalidInput()).ToFin()
                    from hits in s.Self.WithTree(run: tree => SearchCapsule<NeighborHit>(
                        run: buffer => tree.Search(sphere: q.Ball, callback: (sender, args) => { if (NeighborHit.TryCreate(args.Id, out NeighborHit hit)) { buffer.Add(hit); } args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.Id.CompareTo(right.Id), cancel: s.Cancel))
                    select (NeighborAnswer)new NeighborAnswer.Hits(Values: hits),
                overlapsCase: static (s, q) =>
                    from pairs in s.Self.WithTree(run: mine => q.Other.WithTree(run: theirs => SearchCapsule<NeighborPair>(
                        run: buffer => RTree.SearchOverlaps(treeA: mine, treeB: theirs, tolerance: q.Band.Value,
                            callback: (sender, args) => { if (NeighborPair.TryCreate(args.Id, args.IdB, out NeighborPair? pair)) { buffer.Add(pair!); } args.Cancel = s.Cancel.IsCancellationRequested; }),
                        order: static (left, right) => left.A != right.A ? left.A.CompareTo(right.A) : left.B.CompareTo(right.B), cancel: s.Cancel)))
                    select (NeighborAnswer)new NeighborAnswer.PairsFound(Values: pairs),
                pairsCase: static (s, q) =>
                    from needles in q.Needles.TraverseM(v => Acceptance.Value(value: v)).As().Map(static vs => vs.ToArray())
                    from graph in q.Probe.SwitchPartially(
                        state: (s.Self, Needles: needles, s.Key),
                        @default: static (p, _) => Fin.Fail<NeighborhoodGraph>(new KernelFault.InvalidInput()),
                        nearestCase: static (p, n) => NeighborKernel.GraphOf(index: p.Self, needles: p.Needles, count: Some(n.Count), radius: Option<PositiveMagnitude>.None, metric: Some(n.Metric)),
                        radiusCase: static (p, r) => NeighborKernel.GraphOf(index: p.Self, needles: p.Needles, count: r.Cap, radius: Some(r.R), metric: Some(r.Metric)))
                    let pairs = toSeq(graph.Ids
                        .SelectMany(static (row, needle) => row.Select(id => NeighborPair.Create(needle, id)))
                        .OrderBy(static p => p.A).ThenBy(static p => p.B))
                    select (NeighborAnswer)new NeighborAnswer.PairsFound(Values: pairs));
    }

    private Fin<TOut> WithTree<TOut>(Func<RTree, Fin<TOut>> run) => Switch(
        state: (Key: key, Run: run),
        cloudCase: static (s, c) => c.Source.UseIndex(project: cloud =>
            Optional(RTree.CreatePointCloudTree(cloud: cloud)).ToFin(new KernelFault.InvalidResult())
                .Bind(tree => new Lease<RTree>.Owned(Value: tree).Use(s.Run))),
        pointsCase: static (s, p) => Optional(RTree.CreateFromPointArray(p.Points)).ToFin(new KernelFault.InvalidResult())
            .Bind(tree => new Lease<RTree>.Owned(tree).Use(s.Run)),
        meshFacesCase: static (s, m) => s.Run(m.Tree),
        boundsCase: static (s, b) => s.Run(b.Tree));

    private static Fin<Seq<TItem>> SearchCapsule<TItem>(Func<List<TItem>, bool> run, Comparison<TItem> order, CancellationToken cancel) {
        List<TItem> buffer = [];
        bool completed = run(buffer);
        buffer.Sort(comparison: order);
        return (completed, cancel.IsCancellationRequested) switch {
            (_, true) => Fin.Fail<Seq<TItem>>(error: Errors.Cancelled),
            (true, _) => Fin.Succ(toSeq(buffer)),
            _ => Fin.Fail<Seq<TItem>>(error: new KernelFault.InvalidResult()),
        };
    }
}
```

## [03]-[NEIGHBORHOOD_FOLDS]

- Owner: `NeighborKernel` owns every per-point measurement, and `NeighborhoodPolicy` is the one record each fold threads.
- Entry: `GraphOf` is the batch spine; `PcaOf`, `EstimateNormals`, `OrientNormals`, and `PrincipalCurvatures` fold per point over it, and a bare neighborhood census is `GraphOf(...).Map(graph => graph.Census)` at the caller.
- Auto: per-point PCA clamps eigenvalues to the floor and emits the sample `register.md` reads as its GICP precision field; normal orientation runs Hoppe-DeRose over the minimum spanning FOREST of the kNN graph — Kruskal, because a sampled cloud's kNN graph is routinely disconnected and Prim would leave every non-root component unoriented — propagating sign along ONE depth-first walk of that forest; principal curvature routes its quadric solve to the `matrix.md` owners. `CurvatureAxis` owns every derived curvature scalar as a projection row, so `Project` and the range bands are one fold over that vocabulary and each formula has exactly one site; the aggregate `CurvatureRange.Kind` derives from the tally as an `Option`, absent over zero accepted samples, never a stored row.
- Exemption: `OrientNormals` holds ONE `Try.lift` span window over the whole Hoppe-DeRose leg — the two `AddVertexRange` seeds fill by mutation because that is the container's own admission surface, and the sign fold runs on a `Vector3d[]` scratch because `Arr<A>.SetItem` copies its backing array, which makes one propagation pass quadratic; the window freezes to `Seq` through `key.Accept` before anything leaves.
- Law: `NeighborhoodPolicy.Of` reads its two numeric floors from `Domain/context` lanes — `Svd` for the eigen gap and `Residual` for the quadric fit — so neither is a page literal. `SphereLikenessBand` stays a declared `UnitInterval` because a classification band measures shape similarity, not numeric agreement, and no tolerance lane owns it. A non-Euclidean metric is a `PointsCase` capability: the `CloudCase` arm of `GraphOf` refuses it as `Unsupported` inside its own dispatch arm, so no type probe over `NeighborIndex` runs ahead of the fold.
- Packages: QuikGraph (`UndirectedGraph`, `AddEdgeRange`, `MinimumSpanningTreeKruskal`, `AdjacencyGraph`, `DepthFirstSearchAlgorithm`, `EdgeRecorderObserver`), `Rasm.Domain` (`Stat<Scalar>`, the ONE moment owner every census spread reads), RhinoCommon, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new per-point measurement is one fold over the `NeighborhoodGraph` spine with its census columns; a new derived curvature scalar is one `CurvatureAxis` row that joins every band set unasked; a new curvature classification is one `CurvatureRangeKind` row carrying its own `Admits` body, which the tally fold counts unasked; a new quadric refusal cause is one `QuadricAttempt` case and one census arm; a new orientation strategy is one arm beside the MST fold.
- Boundary: every measure an arm may not take rides an `Option` — the residual summary and the whole band set are absent rather than zero-filled, so a census never reads as a perfect fit over samples that failed to solve; a self-neighbour census enters only with explicit needle-to-hay correspondence evidence, never a count heuristic. Moments and extrema come off `Domain/stats` `Stat<Scalar>`, the branch's ONE moment owner, so no reducer roster re-derives the recurrence here.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodPolicy(Dimension NeighborCount, Option<PositiveMagnitude> Radius, PositiveMagnitude EigenGapTolerance, PositiveMagnitude FitResidualTolerance, UnitInterval SphereLikenessBand) {
    internal static Fin<NeighborhoodPolicy> Of(Context context, Option<Dimension> neighbors = default, Option<PositiveMagnitude> radius = default) =>
        from count in neighbors.Match(Some: Fin.Succ, None: () => FactoryBridge.Accept<Dimension>(candidate: 10))
        from gap in FactoryBridge.Accept<PositiveMagnitude>(candidate: context.For(lane: ToleranceLane.Svd).Value)
        from residual in FactoryBridge.Accept<PositiveMagnitude>(candidate: context.For(lane: ToleranceLane.Residual).Value)
        from band in FactoryBridge.Accept<UnitInterval>(candidate: 0.35)
        select new NeighborhoodPolicy(NeighborCount: count, Radius: radius, EigenGapTolerance: gap, FitResidualTolerance: residual, SphereLikenessBand: band);
    internal Fin<NeighborhoodPolicy> Admit() {
        NeighborhoodPolicy self = this;
        return guard(self.NeighborCount.Value >= 3, new KernelFault.InvalidInput()).ToFin().Map(_ => self);
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct NeighborhoodPcaSample(
    int Index, Point3d Point, int NeighborCount, SymmetricMatrix Covariance, Vector3d Normal,
    Arr<double> RawEigenvalues, Arr<double> ClampedEigenvalues, int Rank, int EigenClampCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Index >= 0 && NeighborCount >= 0 && Rank is >= 0 and <= 3 && EigenClampCount >= 0,
        ValidityClaim.Finite(Point),
        ValidityClaim.Finite(Normal),
        ValidityClaim.Evidence(Covariance),
        ValidityClaim.CountExactly(count: RawEigenvalues.Count, expected: 3),
        ValidityClaim.CountExactly(count: ClampedEigenvalues.Count, expected: 3),
        ClampedEigenvalues.ForAll(static v => ValidityClaim.Positive(v).Holds));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PcaCensus(
    int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
    int RankClampCount, int EigenClampCount, double EigenClampFloor, NeighborhoodCensus Neighborhood) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        AcceptedSampleCount >= 0 && RejectedSampleCount >= 0 && RankClampCount >= 0 && EigenClampCount >= 0,
        ValidityClaim.CountExactly(count: AcceptedSampleCount + RejectedSampleCount, expected: Neighborhood.InputCount),
        ValidityClaim.Positive(EigenClampFloor),
        ValidityClaim.Evidence(Neighborhood));
}

public readonly record struct NeighborhoodPcaResult(Seq<NeighborhoodPcaSample> Samples, PcaCensus Census);

[SmartEnum]
public sealed partial class CurvatureRangeKind {
    public static readonly CurvatureRangeKind Plane = new(
        admits: static (sample, _) => Math.Abs(sample.K1) <= EpsilonPolicy.SqrtEpsilon && Math.Abs(sample.K2) <= EpsilonPolicy.SqrtEpsilon);
    public static readonly CurvatureRangeKind Sphere = new(
        admits: static (sample, band) => Math.Abs(sample.K1 - sample.K2) <= band * Math.Max(Math.Abs(sample.K1), Math.Abs(sample.K2)));
    public static readonly CurvatureRangeKind Saddle = new(
        admits: static (sample, _) => sample.K1 > EpsilonPolicy.SqrtEpsilon && sample.K2 < -EpsilonPolicy.SqrtEpsilon);
    public static readonly CurvatureRangeKind Mixed = new(admits: static (_, _) => true);

    [UseDelegateFromConstructor] internal partial bool Admits(CurvatureSample sample, double band);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureSample(
    int Index, Point3d Point, double K1, double K2, Direction E1, Direction E2, double Residual, int NeighborCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Index, floor: 0),
        ValidityClaim.Finite(Point),
        ValidityClaim.Finite(K1),
        ValidityClaim.Finite(K2),
        E1.IsValid && E2.IsValid,
        ValidityClaim.Nonnegative(Residual),
        ValidityClaim.CountAtLeast(count: NeighborCount, floor: NeighborKernel.QuadricUnknowns));
}

[SmartEnum]
public sealed partial class CurvatureAxis {
    public static readonly CurvatureAxis Principal = new(project: static s => s.K1);
    public static readonly CurvatureAxis Secondary = new(project: static s => s.K2);
    public static readonly CurvatureAxis Gaussian = new(project: static s => s.K1 * s.K2);
    public static readonly CurvatureAxis Mean = new(project: static s => 0.5 * (s.K1 + s.K2));
    public static readonly CurvatureAxis Curvedness = new(project: static s => Math.Sqrt(0.5 * ((s.K1 * s.K1) + (s.K2 * s.K2))));
    public static readonly CurvatureAxis Shape = new(project: static s => Math.Abs(s.K1 - s.K2) < EpsilonPolicy.SqrtEpsilon
        ? (double)Math.Sign(s.K1 + s.K2)
        : 2.0 / Math.PI * Math.Atan2(s.K1 + s.K2, s.K1 - s.K2));
    [UseDelegateFromConstructor] internal partial double Project(CurvatureSample sample);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureCensus(
    int RequestedNeighborCount, int RankRejectedCount, int ResidualRejectedCount, int SolveRejectedCount,
    Option<Stat<Scalar>> Residuals,
    double EigenGapTolerance, double FitResidualTolerance, double SphereLikenessBand,
    NeighborhoodCensus Neighborhood, CurvatureRange Range) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: Range.AcceptedSampleCount + RankRejectedCount + ResidualRejectedCount + SolveRejectedCount, expected: Neighborhood.InputCount),
        Residuals.IsSome == (Range.AcceptedSampleCount > 0),
        Residuals.Map(spread => ValidityClaim.Evidence(Some(spread)).Holds
            && ValidityClaim.Nonnegative(spread.Minimum.To()).Holds
            && ValidityClaim.CountExactly(count: spread.Count, expected: Range.AcceptedSampleCount).Holds).IfNone(true),
        ValidityClaim.Positive(EigenGapTolerance),
        ValidityClaim.Positive(FitResidualTolerance),
        ValidityClaim.UnitInterval(SphereLikenessBand),
        ValidityClaim.Evidence(Neighborhood),
        ValidityClaim.Evidence(Range));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureRange(
    int AcceptedSampleCount, int PlaneLikeCount, int SphereLikeCount,
    int SaddleLikeCount, int MixedCount, Option<Arr<(CurvatureAxis Axis, Stat<Scalar> Spread)>> Bands, double Tolerance) : IValidityEvidence {
    public Option<CurvatureRangeKind> Kind => AcceptedSampleCount switch {
        0 => None,
        int n when PlaneLikeCount == n => Some(CurvatureRangeKind.Plane),
        int n when SphereLikeCount == n => Some(CurvatureRangeKind.Sphere),
        int n when SaddleLikeCount == n => Some(CurvatureRangeKind.Saddle),
        _ => Some(CurvatureRangeKind.Mixed),
    };
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: PlaneLikeCount + SphereLikeCount + SaddleLikeCount + MixedCount, expected: AcceptedSampleCount),
        Bands.IsSome == (AcceptedSampleCount > 0),
        Bands.Map(static bands => bands.Count == CurvatureAxis.Items.Count && bands.ForAll(static band => ValidityClaim.Evidence(Some(band.Spread)).Holds)).IfNone(true),
        ValidityClaim.Nonnegative(Tolerance));
}

public readonly record struct CurvatureResult(Seq<CurvatureSample> Samples, CurvatureCensus Census);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class NeighborKernel {
    internal static Fin<NeighborhoodGraph> GraphOf(NeighborIndex index, Point3d[] needles, NeighborhoodPolicy policy) =>
        policy.Admit().Bind(admitted => GraphOf(index: index, needles: needles,
            count: Some(admitted.NeighborCount), radius: admitted.Radius));

    internal static Fin<NeighborhoodGraph> GraphOf(
        NeighborIndex index, Point3d[] needles, Option<Dimension> count, Option<PositiveMagnitude> radius,
        Option<NeighborMetric> metric = default) =>
        from _ in guard(needles.Length > 0 && (count.IsSome || radius.IsSome), new KernelFault.InvalidInput()).ToFin()
        from graph in index.Switch(
            state: (Needles: needles, Count: count, Radius: radius, Metric: metric),
            cloudCase: static (s, c) =>
                guard(s.Metric.IfNone(NeighborMetric.Euclidean) == NeighborMetric.Euclidean, new KernelFault.Unsupported(InputType: typeof(NeighborIndex.CloudCase), OutputType: typeof(NeighborMetric))).ToFin()
                    .Bind(_ => c.Source.UseIndex(project: cloud => Batch(needles: s.Needles, count: s.Count, radius: s.Radius,
                        hayCount: c.Source.Vertices.Count, hayAt: i => c.Source.Vertices[i], usesKdTree: false,
                        knn: k => RTree.PointCloudKNeighbors(pointcloud: cloud, needlePts: s.Needles, amount: k),
                        radial: (r, _) => RTree.PointCloudClosestPoints(pointcloud: cloud, needlePts: s.Needles, limitDistance: r)))),
            pointsCase: static (s, p) => {
                NeighborMetric row = s.Metric.IfNone(NeighborMetric.Euclidean);
                KDTree<double, double, int> tree = p.Trees[row];
                return Batch(needles: s.Needles, count: s.Count, radius: s.Radius,
                    hayCount: p.Points.Length, hayAt: i => p.Points[i], usesKdTree: true,
                    knn: k => s.Needles.Select(needle => tree.NearestNeighbors(point: Coordinate(needle), numNeighbors: k).Select(static hit => hit.Item2).ToArray()),
                    radial: (r, cap) => s.Needles.Select(needle => tree.RadialSearch(center: Coordinate(needle), radius: row.SearchRadius(r), numNeighbors: cap).Select(static hit => hit.Item2).ToArray()));
            },
            meshFacesCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(new KernelFault.Unsupported(InputType: typeof(NeighborIndex.MeshFacesCase), OutputType: typeof(NeighborhoodGraph))),
            boundsCase: static (s, _) => Fin.Fail<NeighborhoodGraph>(new KernelFault.Unsupported(InputType: typeof(NeighborIndex.BoundsCase), OutputType: typeof(NeighborhoodGraph))))
        select graph;

    internal static Fin<NeighborhoodPcaResult> PcaOf(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy);

    internal static Fin<Arr<Vector3d>> EstimateNormals(VectorCloud.ClusterCase cluster, NeighborhoodGraph graph, NeighborhoodPolicy policy);

    internal static Fin<Seq<Vector3d>> OrientNormals(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy) =>
        from graph in GraphOf(index: new NeighborIndex.CloudCase(Source: cluster), needles: [.. cluster.Vertices.AsIterable()], policy: policy)
        from normals in EstimateNormals(cluster: cluster, graph: graph, policy: policy)
        from oriented in Try.lift(() => {
            UndirectedGraph<int, SEdge<int>> knn = new(allowParallelEdges: false);
            _ = knn.AddVertexRange(Enumerable.Range(0, normals.Count));
            _ = knn.AddEdgeRange(graph.Ids.SelectMany((row, i) =>
                row.Where(j => j >= 0 && j < normals.Count && j != i).Select(j => new SEdge<int>(i, j))));
            AdjacencyGraph<int, SEdge<int>> tree = new(allowParallelEdges: false);
            _ = tree.AddVertexRange(Enumerable.Range(0, normals.Count));
            _ = tree.AddEdgeRange(knn
                .MinimumSpanningTreeKruskal(edgeWeights: e => 1.0 - Math.Abs(normals[e.Source] * normals[e.Target]))
                .SelectMany(static e => (SEdge<int>[])[new(e.Source, e.Target), new(e.Target, e.Source)]));
            DepthFirstSearchAlgorithm<int, SEdge<int>> walk = new(tree) { ProcessAllComponents = true };
            EdgeRecorderObserver<int, SEdge<int>> visited = new();
            using (visited.Attach(algorithm: walk)) { walk.Compute(); }
            Vector3d[] field = [.. normals.AsIterable()];
            foreach (SEdge<int> edge in visited.Edges) {
                if (field[edge.Source] * field[edge.Target] < 0.0) { field[edge.Target] = -field[edge.Target]; }
            }
            return Acceptance.Rows(values: toSeq(field));
        }).Run().Bind(static inner => inner)
        select oriented;

    internal static Fin<CurvatureResult> PrincipalCurvatures(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy) =>
        from _ in guard(policy.NeighborCount.Value >= QuadricUnknowns, new KernelFault.InvalidInput()).ToFin()
        from graph in GraphOf(index: new NeighborIndex.CloudCase(Source: cluster), needles: [.. cluster.Vertices.AsIterable()], policy: policy)
        from attempts in toSeq(graph.Ids.Select(static (row, index) => (Row: row, Index: index)))
            .TraverseM(vertex => AttemptOf(cluster: cluster, index: vertex.Index, row: vertex.Row, policy: policy)).As()
        let census = attempts.Fold((Accepted: Seq<CurvatureSample>(), Rank: 0, Residual: 0, Solve: 0), static (held, attempt) => attempt.Switch(
            state: held,
            fitted: static (h, f) => (h.Accepted.Add(f.Sample), h.Rank, h.Residual, h.Solve),
            rankRefused: static (h, _) => (h.Accepted, h.Rank + 1, h.Residual, h.Solve),
            residualRefused: static (h, _) => (h.Accepted, h.Rank, h.Residual + 1, h.Solve),
            solveRefused: static (h, _) => (h.Accepted, h.Rank, h.Residual, h.Solve + 1)))
        from residuals in census.Accepted.IsEmpty
            ? Fin.Succ(Option<Stat<Scalar>>.None)
            : Stat<Scalar>.Of(values: census.Accepted.Map(static s => (Scalar)s.Residual)).Map(Some)
        from range in RangeOf(samples: census.Accepted, band: policy.SphereLikenessBand.Value)
        let tally = new CurvatureCensus(
            RequestedNeighborCount: policy.NeighborCount.Value,
            RankRejectedCount: census.Rank, ResidualRejectedCount: census.Residual, SolveRejectedCount: census.Solve,
            Residuals: residuals,
            EigenGapTolerance: policy.EigenGapTolerance.Value, FitResidualTolerance: policy.FitResidualTolerance.Value,
            SphereLikenessBand: policy.SphereLikenessBand.Value, Neighborhood: graph.Census, Range: range)
        from result in tally.IsValid
            ? Fin.Succ(new CurvatureResult(Samples: census.Accepted, Census: tally))
            : Fin.Fail<CurvatureResult>(new KernelFault.InvalidResult())
        select result;

    internal static Fin<Seq<double>> Project(
        CurvatureAxis axis, VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy) =>
        PrincipalCurvatures(cluster, policy, key).Map(r => r.Samples.Map(axis.Project));

    private static Fin<NeighborhoodGraph> Batch(Point3d[] needles, Option<Dimension> count, Option<PositiveMagnitude> radius,
        int hayCount, Func<int, Point3d> hayAt, bool usesKdTree,
        Func<int, IEnumerable<int[]>> knn, Func<double, int, IEnumerable<int[]>> radial) =>
        guard(hayCount > 0, new KernelFault.InvalidInput()).ToFin().Bind(_ => Try.lift(() => {
            int requested = Math.Min(count.Map(static c => c.Value).IfNone(hayCount), hayCount);
            IEnumerable<int[]> batch = radius.Match(
                Some: r => radial(r.Value, requested), None: () => knn(requested));
            using IDisposable? window = batch as IDisposable;
            int[][] ids = radius.IsSome
                ? [.. batch.Select((row, i) => row.OrderBy(id => needles[i].DistanceToSquared(hayAt(id))).Take(requested).ToArray())]
                : [.. batch];
            double[] returned = [.. ids.Select(static row => (double)row.Length)];
            return Stat<Scalar>.Of(plane: returned).Bind(spread => {
                NeighborhoodCensus census = new(
                    InputCount: hayCount, QueryCount: needles.Length, RequestedNeighborCount: requested,
                    UsesKdTree: usesKdTree, Radius: radius,
                    EmptyNeighborhoodCount: returned.Count(static n => n == 0.0),
                    OutOfRangeIndexCount: ids.Sum(row => row.Count(id => id < 0 || id >= hayCount)),
                    DuplicateIndexCount: ids.Sum(static row => row.Length - row.Distinct().Count()),
                    Returned: spread);
                return ids.Length == needles.Length && census.IsValid
                    ? Fin.Succ(new NeighborhoodGraph(Ids: ids, Census: census))
                    : Fin.Fail<NeighborhoodGraph>(new KernelFault.InvalidResult());
            });
        }).Run().Bind(static inner => inner));

    internal const int QuadricUnknowns = 6;

    private static Fin<QuadricAttempt> AttemptOf(VectorCloud.ClusterCase cluster, int index, int[] row, NeighborhoodPolicy policy) =>
        row.Length < QuadricUnknowns
            ? Fin.Succ((QuadricAttempt)new QuadricAttempt.RankRefused())
            : from stats in CloudKernel.CovarianceOf(points: toSeq(row.Select(id => cluster.Vertices[id])), mass: Option<Arr<double>>.None)
              from eigen in stats.Cov.DecomposeEigenDetailed().Bind(solved => solved.PairsIn(expected: EigenOrder.DescendingMagnitude))
              let frame = (U: AxisOf(eigen[0].Eigenvector), V: AxisOf(eigen[1].Eigenvector), N: AxisOf(eigen[2].Eigenvector))
              let center = cluster.Vertices[index]
              let local = row.Select(id => cluster.Vertices[id] - center).Select(d => (U: d * frame.U, V: d * frame.V, N: d * frame.N)).ToArray()
              from rows in FactoryBridge.Accept<Dimension>(candidate: local.Length)
              from cols in FactoryBridge.Accept<Dimension>(candidate: QuadricUnknowns)
              from design in Matrix.Of(rows: rows, cols: cols, entries: new Arr<double>([.. local.SelectMany(static q => (double[])[q.U * q.U, q.U * q.V, q.V * q.V, q.U, q.V, 1.0])]))
              from attempt in design.LeastSquaresDetailed(rhs: new Arr<double>([.. local.Select(static q => q.N)])).Match(
                  Succ: fit => !fit.Stop.IsUsable
                      ? Fin.Succ((QuadricAttempt)new QuadricAttempt.RankRefused())
                      : fit.Residual > policy.FitResidualTolerance.Value
                          ? Fin.Succ((QuadricAttempt)new QuadricAttempt.ResidualRefused(Residual: fit.Residual))
                          : SampleOf(index: index, point: center, frame: (frame.U, frame.V), fit: fit, neighborCount: row.Length, context: cluster.Tolerance)
                              .Map(static sample => (QuadricAttempt)new QuadricAttempt.Fitted(Sample: sample)),
                  Fail: cause => Fin.Succ((QuadricAttempt)new QuadricAttempt.SolveRefused(Cause: cause)))
              select attempt;

    private static Fin<CurvatureSample> SampleOf(int index, Point3d point, (Vector3d U, Vector3d V) frame, LinearSolution fit, int neighborCount, Context context) =>
        from dim in FactoryBridge.Accept<Dimension>(candidate: 2)
        from shape in SymmetricMatrix.Of(dim: dim, upper: new Arr<double>([2.0 * fit.Solution[0], fit.Solution[1], 2.0 * fit.Solution[2]]))
        from pairs in shape.DecomposeEigenDetailed().Map(static solved => solved.Pairs)
        let ordered = pairs[0].Eigenvalue >= pairs[1].Eigenvalue ? (Max: pairs[0], Min: pairs[1]) : (Max: pairs[1], Min: pairs[0])
        from e1 in Direction.Of(value: (ordered.Max.Eigenvector[0] * frame.U) + (ordered.Max.Eigenvector[1] * frame.V), context: context)
        from e2 in Direction.Of(value: (ordered.Min.Eigenvector[0] * frame.U) + (ordered.Min.Eigenvector[1] * frame.V), context: context)
        select new CurvatureSample(Index: index, Point: point, K1: ordered.Max.Eigenvalue, K2: ordered.Min.Eigenvalue, E1: e1, E2: e2, Residual: fit.Residual, NeighborCount: neighborCount);

    private static Fin<CurvatureRange> RangeOf(Seq<CurvatureSample> samples, double band) {
        HashMap<CurvatureRangeKind, int> tally = samples.Fold(HashMap<CurvatureRangeKind, int>(),
            (held, sample) => held.AddOrUpdate(CurvatureRangeKind.Items.First(row => row.Admits(sample, band)), static n => n + 1, 1));
        int Counted(CurvatureRangeKind row) => tally.Find(row).IfNone(0);
        return (samples.IsEmpty
                ? Fin.Succ(Option<Arr<(CurvatureAxis Axis, Stat<Scalar> Spread)>>.None)
                : CurvatureAxis.Items.AsIterable().Traverse(axis =>
                        Stat<Scalar>.Of(values: samples.Map(sample => (Scalar)axis.Project(sample: sample)))
                            .Map(spread => (Axis: axis, Spread: spread)))
                    .Map(bands => Some(new Arr<(CurvatureAxis Axis, Stat<Scalar> Spread)>([.. bands]))))
            .Map(bands => new CurvatureRange(
                AcceptedSampleCount: samples.Count,
                PlaneLikeCount: Counted(CurvatureRangeKind.Plane), SphereLikeCount: Counted(CurvatureRangeKind.Sphere),
                SaddleLikeCount: Counted(CurvatureRangeKind.Saddle), MixedCount: Counted(CurvatureRangeKind.Mixed),
                Bands: bands, Tolerance: EpsilonPolicy.SqrtEpsilon));
    }

    private static Vector3d AxisOf(Arr<double> eigenvector) => new(x: eigenvector[0], y: eigenvector[1], z: eigenvector[2]);
    private static IReadOnlyList<double> Coordinate(Point3d point) => [point.X, point.Y, point.Z];

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    private abstract partial record QuadricAttempt {
        private QuadricAttempt() { }
        public sealed record Fitted(CurvatureSample Sample) : QuadricAttempt;
        public sealed record RankRefused : QuadricAttempt;
        public sealed record ResidualRefused(double Residual) : QuadricAttempt;
        public sealed record SolveRefused(Error Cause) : QuadricAttempt;
    }
}
```

## [04]-[BISHOP_CHAIN]

- Owner: `NeighborKernel.BishopChain` mints the one point-chain rotation-minimizing-frame body that `VectorFrame.Chain` delegates to.
- Law: ring posture is the one `bool isClosed` discriminant `VectorFrame.Chain` threads down — a payloadless two-case type over the same binary fact is the forbidden second spelling, so the boolean column owns the posture and each `VectorCloud` arm states it exactly once.
- Exemption: the double-reflection walk is a named span kernel — each step's reference vector is the previous step's product, so no fold or traversal owner carries it and the tangent/reference arrays stay mutable for exactly that pass.
- Growth: a new transport flavor is one policy argument on this fold.
- Boundary: every emitted plane admits through `VectorFrame.Of`; `Direction.ParallelTransport` applies caller-supplied frames, and parametric-curve sweeps route `Parametric/curve.md` `PerpendicularFrames`.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class NeighborKernel {
    internal static Fin<Seq<Plane>> BishopChain(VectorCloud cloud) => cloud.Switch(
        state: key,
        ringCase: static (k, r) =>
            from seed in Direction.Of(value: VectorFrame.NewellNormal(ring: r.Vertices.ToArray()), context: r.Tolerance, key: k)
            from chain in BishopChain(points: r.Vertices, initialNormal: seed, isClosed: true, context: r.Tolerance, key: k)
            select chain,
        polylineCase: static (k, p) =>
            from _ in guard(p.Vertices.Count >= 2, new KernelFault.InvalidInput()).ToFin()
            from seed in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: p.Vertices[1] - p.Vertices[0]), context: p.Tolerance, key: k)
            from chain in BishopChain(points: p.Vertices, initialNormal: seed, isClosed: false, context: p.Tolerance, key: k)
            select chain,
        clusterCase: static (k, _) => Fin.Fail<Seq<Plane>>(new KernelFault.Unsupported(InputType: typeof(VectorCloud.ClusterCase), OutputType: typeof(Seq<Plane>))));

    internal static Fin<Seq<Plane>> BishopChain(Seq<Point3d> points, Direction initialNormal, bool isClosed, Context context) =>
        from _ in guard(points.Count >= 2, new KernelFault.InvalidInput()).ToFin()
        from columns in Try.lift(() => {
            Point3d[] p = [.. points];
            double step = context.For(lane: ToleranceLane.Collapse).Value;
            double floor = step * step;
            var tangents = new Vector3d[p.Length];
            Vector3d prior = p[1] - p[0];
            for (int i = 0; i < p.Length; i++) {
                Vector3d advance = i < p.Length - 1 ? p[i + 1] - p[i] : isClosed ? p[0] - p[i] : prior;
                tangents[i] = advance.IsTiny(step) ? prior : advance;
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
            if (isClosed) {
                Vector3d returned = Transported(reference: reference[^1], tangent: tangents[^1], next: tangents[0], chord: p[0] - p[^1], floor: floor);
                double residual = Math.Atan2(Vector3d.CrossProduct(a: reference[0], b: returned) * tangents[0], reference[0] * returned);
                for (int i = 1; i < p.Length; i++) {
                    _ = reference[i].Rotate(angleRadians: -residual * i / p.Length, rotationAxis: tangents[i]);
                }
            }
            return Fin.Succ((Points: p, Tangents: tangents, References: reference));
        }).Run().Bind(static inner => inner)
        from frames in toSeq(Enumerable.Range(0, columns.Points.Length))
            .TraverseM(i => VectorFrame.Of(origin: columns.Points[i], normal: columns.Tangents[i],
                xHint: Some(columns.References[i]), context: context).Map(static frame => frame.Value)).As()
        select frames;

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
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
