# [RASM_CLOUD]

`VectorCloud` owns the point-cloud union under one admission that deduplicates by tolerance, renormalizes mass, and carries a copy-safe shared native index per cluster. `VectorCloudMetric` folds every cloud measurement behind one `Project<TOut>`, each row naming the `CloudKernel` fold that answers it. A new cloud capability lands as a metric row, a hull-kind row, or a shape column.

`CloudKernel.CovarianceOf` is the corpus's one covariance fold, composing `Domain/stats.md` `SampleMoment` into a `matrix.md` `SymmetricMatrix` every PCA consumer reads.

## [01]-[INDEX]

- [02]-[VECTOR_CLOUD]: `VectorCloud` folds every cloud case under tolerance-dedup admission with the lazy cluster index and closest-vertex probe.
- [03]-[CLOUD_METRICS]: `VectorCloudMetric` projects every measurement through one `Project<TOut>` over the kernel folds.
- [04]-[HULL]: `CloudHullKind` rails native convex and Delaunay-filtered concave hulls into typed receipts.

## [02]-[VECTOR_CLOUD]

- Owner: `VectorCloud` mints one case per cloud modality, mass an `Option` column on `ClusterCase`, so a weighted cluster is that case rather than a case of its own.
- Auto: cluster admission is the ONE dedup-and-renormalize fold, emitting `OriginalToUnique` — the input-index→unique-index map every external per-point array re-indexes through to survive deduplication.
- Packages: RhinoCommon (native point cloud, polyline closure, self-intersection), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new cloud modality is one union case, one factory, and its metric-adapter arms; a new admission rule is one policy column.
- Boundary: admission runs ONCE at the factory, so every kernel fold below consumes admitted vertices without re-validating and re-admission runs with dedup off to keep vertices index-stable; native `PointCloud` and `PolylineCurve` reads are the platform seam, held inside their lease windows under `key.Catch`; `Dispose` releases one shared cluster extent, so copies stay safe while a rehydrated cloud owns its own.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record VectorCloud : IDisposable {
    private VectorCloud() { }
    public sealed record RingCase : VectorCloud { internal RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) { this.Vertices = Vertices; this.Native = Native; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Polyline Native { get; } public Context Tolerance { get; } }
    public sealed record PolylineCase : VectorCloud { internal PolylineCase(Seq<Point3d> Vertices, Context Tolerance) { this.Vertices = Vertices; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Context Tolerance { get; } }
    public sealed record ClusterCase : VectorCloud {
        internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass, Lease<PointCloud> Indexed, CloudAdmissionReceipt Admission) { this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass; Index = new IndexHandle(lease: Indexed); this.Admission = Admission; }
        private ClusterCase(ClusterCase original) : base(original) { Vertices = original.Vertices; Tolerance = original.Tolerance; Mass = original.Mass; Index = original.Index.Copy(); Admission = original.Admission; }
        public Seq<Point3d> Vertices { get; }
        public Context Tolerance { get; }
        public Option<Arr<double>> Mass { get; }
        private IndexHandle Index { get; }
        public CloudAdmissionReceipt Admission { get; }

        internal Fin<T> UseIndex<T>(Op key, Func<PointCloud, Fin<T>> project) =>
            Index.Use(key: key, project: project);

        internal Fin<ClosestHit> ClosestVertex(Point3d sample, Op key) =>
            UseIndex(key: key, project: indexed => key.Catch(() => indexed.ClosestPoint(testPoint: sample) switch {
                    int idx when idx >= 0 && idx < Vertices.Count => key.AcceptValue(value: ClosestHit.At(
                        target: sample, point: indexed.PointAt(index: idx),
                        component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: idx)))),
                    _ => Fin.Fail<ClosestHit>(error: key.InvalidResult()),
                }));

        internal Unit Release() => Index.Release();

        private sealed class IndexHandle : IEquatable<IndexHandle> {
            private readonly SharedIndex owner;
            private int disposed;
            internal IndexHandle(Lease<PointCloud> lease) { owner = new SharedIndex(lease: lease); }
            private IndexHandle(SharedIndex owner, bool live) { this.owner = owner; disposed = live ? 0 : 1; }
            internal IndexHandle Copy() {
                if (Volatile.Read(location: ref disposed) != 0 || !owner.TryRetain()) return new IndexHandle(owner: owner, live: false);
                return new IndexHandle(owner: owner, live: true);
            }
            internal Fin<T> Use<T>(Op key, Func<PointCloud, Fin<T>> project) =>
                Volatile.Read(location: ref disposed) == 0
                    ? owner.Use(key: key, project: project)
                    : Fin.Fail<T>(key.InvalidContext());
            internal Unit Release() => Interlocked.Exchange(location1: ref disposed, value: 1) == 0 ? owner.Release() : unit;
            public bool Equals(IndexHandle? other) => other is not null && ReferenceEquals(objA: owner, objB: other.owner);
            public override bool Equals(object? obj) => obj is IndexHandle other && Equals(other: other);
            public override int GetHashCode() => RuntimeHelpers.GetHashCode(o: owner);
        }

        private sealed class SharedIndex(Lease<PointCloud> lease) {
            private int references = 1;
            internal bool TryRetain() {
                while (true) {
                    int current = Volatile.Read(location: ref references);
                    if (current <= 0 || current == int.MaxValue) return false;
                    if (Interlocked.CompareExchange(location1: ref references, value: current + 1, comparand: current) == current) return true;
                }
            }
            internal Fin<T> Use<T>(Op key, Func<PointCloud, Fin<T>> project) {
                if (!TryRetain()) return Fin.Fail<T>(key.InvalidContext());
                try { return project(arg: lease.Resource); }
                finally { _ = Release(); }
            }
            internal Unit Release() {
                if (Interlocked.Decrement(location: ref references) == 0) _ = lease.Dispose();
                return unit;
            }
        }
    }

    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 3)
        let closed = admitted.Points.Count > 1 && admitted.Points[0].EpsilonEquals(other: admitted.Points[^1], epsilon: admitted.Context.Absolute.Value)
        let vertices = closed ? admitted.Points.Init : admitted.Points
        from _ in guard(vertices.Count >= 3, admitted.Key.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from closure in guard(native.IsValid && native.IsClosedWithinTolerance(admitted.Context.Absolute.Value) && native.SegmentCount >= 3, admitted.Key.InvalidInput())
        from simple in Optional(native.ToPolylineCurve()).ToFin(admitted.Key.InvalidResult())
            .Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (admitted.Context, admitted.Key),
                project: static (s, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: s.Context.Absolute.Value))
                    .ToFin(s.Key.InvalidResult())
                    .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(s.Key.InvalidInput()))))
        select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: admitted.Context);

    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) =>
        AdmitPoints(points: points, context: context, key: key, minimum: 2)
            .Map(static a => (VectorCloud)new PolylineCase(Vertices: a.Points, Tolerance: a.Context));

    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Option<CloudAdmissionPolicy> admission = default, Option<Seq<double>> mass = default, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 1)
        from policy in admission.IfNone(CloudAdmissionPolicy.Default).Admit(key: admitted.Key)
        from fold in CloudKernel.AdmitCluster(points: admitted.Points, mass: mass.Map(static m => new Arr<double>([.. m.AsIterable()])), policy: policy, key: admitted.Key)
        from indexed in admitted.Key.Catch(() => {
            PointCloud native = [];
            native.AddRange(points: fold.Points.AsIterable());
            return Fin.Succ(native);
        })
        select (VectorCloud)new ClusterCase(Vertices: fold.Points, Tolerance: admitted.Context, Mass: fold.Mass, Indexed: new Lease<PointCloud>.Owned(Value: indexed), Admission: fold.Receipt);

    internal Fin<VectorCloud> Admit(Op key) => Switch(
        state: key,
        ringCase: static (op, ring) => Ring(points: ring.Vertices, context: ring.Tolerance, key: op),
        polylineCase: static (op, poly) => Polyline(points: poly.Vertices, context: poly.Tolerance, key: op),
        clusterCase: static (op, cluster) => Cluster(points: cluster.Vertices, context: cluster.Tolerance,
            admission: Some(CloudAdmissionPolicy.Default with { Deduplicate = false }),
            mass: cluster.Mass.Map(static values => toSeq(values.AsIterable())), key: op));

    [BoundaryAdapter]
    public void Dispose() => Switch(
        ringCase: static _ => { },
        polylineCase: static _ => { },
        clusterCase: static cluster => { _ = cluster.Release(); });

    private static Fin<(Seq<Point3d> Points, Context Context, Op Key)> AdmitPoints(Seq<Point3d> points, Context context, Op? key, int minimum) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from _ in points.TraverseM(p => op.AcceptValue(value: p)).As()
               from count in guard(points.Count >= minimum, op.InvalidInput())
               select (Points: points, Context: model, Key: op);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionPolicy(
    bool Deduplicate, Option<PositiveMagnitude> Tolerance, PositiveMagnitude ConservationTolerance) {
    internal static CloudAdmissionPolicy Default => new(
        Deduplicate: true, Tolerance: None, ConservationTolerance: PositiveMagnitude.Create(value: 1.0e-8));
    internal Fin<CloudAdmissionPolicy> Admit(Op key) {
        CloudAdmissionPolicy self = this;
        return guard(ValidityClaim.All(
                ValidityClaim.Of(self.Tolerance.Map(static tolerance => ValidityClaim.Positive(tolerance.Value).Holds).IfNone(true)),
                ValidityClaim.Positive(self.ConservationTolerance.Value)), key.InvalidInput())
            .ToFin().Map(_ => self);
    }
    internal bool Equivalent(Point3d left, Point3d right) => Tolerance switch {
        { IsSome: true, Case: PositiveMagnitude t } => left.EpsilonEquals(other: right, epsilon: t.Value),
        _ => left == right,
    };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionReceipt(
    int InputCount, int OutputCount, int InputDuplicateCoordinateCount, int MergedCoordinateCount,
    double Tolerance, double ConservationTolerance, bool Deduplicated, Arr<int> OriginalToUnique,
    Option<double> MassInputTotal, Option<double> MassMergedTotal, Option<double> MassOutputTotal) : IValidityEvidence {
    internal static bool MassConserved(double input, double output, double tolerance) =>
        Math.Abs(input - output) <= tolerance * Math.Max(1.0, Math.Abs(input));
    internal static bool MassNormalized(double output, double tolerance) =>
        Math.Abs(1.0 - output) <= tolerance;
    internal static bool MassAdmitted(double total) => double.IsFinite(total) && total >= 0.0;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        ValidityClaim.Nonnegative(Tolerance),
        ValidityClaim.Positive(ConservationTolerance),
        ValidityClaim.Of(InputDuplicateCoordinateCount >= 0 && MergedCoordinateCount >= 0),
        ValidityClaim.CountExactly(count: OutputCount + MergedCoordinateCount, expected: InputCount),
        ValidityClaim.Of(MergedCoordinateCount == 0 || Deduplicated),
        ValidityClaim.CountExactly(count: OriginalToUnique.Count, expected: InputCount),
        ValidityClaim.Of(OriginalToUnique.ForAll(i => i >= 0 && i < OutputCount)),
        ValidityClaim.Of((MassInputTotal.Case, MassMergedTotal.Case, MassOutputTotal.Case) switch {
            (double input, double merged, double output) =>
                MassAdmitted(total: input) && MassAdmitted(total: merged) && MassAdmitted(total: output)
                && MassConserved(input: input, output: merged, tolerance: ConservationTolerance)
                && MassNormalized(output: output, tolerance: ConservationTolerance),
            _ => MassInputTotal.IsNone && MassMergedTotal.IsNone && MassOutputTotal.IsNone,
        }));
}
```

## [03]-[CLOUD_METRICS]

- Owner: `VectorCloudMetric` mints one row per measurement behind ONE `Project<TOut>`, each row a single declaration line naming its fold and its admissible cloud cases; `CloudMetricPolicy` wraps the `neighbors.md` `NeighborhoodPolicy` as the ONE policy record neighborhood-backed rows thread.
- Auto: `PrincipalFrameOf` builds the frame from the two dominant eigenvectors, and ring orientation reads `ClosedCurveOrientation` against the fitted plane to sign the normal CCW-positive. Skewness is the worst normalized interior-angle deviation from the regular-polygon ideal, compactness `4πA/P²`, moment anisotropy the in-plane principal-moment ratio; chain rows are pure folds over unitized tangents, prefix-sum arc length, and turning-angle curvature. `PlanarWindingOf` takes the query point, so it is a kernel entry rather than a metric row, and the `intent.md` `WindingCase` composes it with the CCW-signed `RingNormalOf` normal — a sign-arbitrary best-fit-plane normal flips the winding integer. `Shape` answers one `VectorCloudShape` per cloud case, never a per-case sibling record.
- Packages: RhinoCommon (area mass properties, plane fitting, polyline geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measurement is ONE row through the matching builder; a new cloud case extends the builders' adapt arms; a policy knob is one column on `CloudMetricPolicy`.
- Boundary: neighborhood-backed rows delegate to `neighbors.md`, the fold living on that substrate while the metric row is its cloud-facing name and its receipt returns unchanged; `AreaMassProperties` and `PolylineCurve` natives stay inside their lease windows; `PlanarWinding` names the 2D ring fold, held distinct from the 3D solid-angle GWN family `reconstruct.md` owns.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0, measure: static (c, k) => CloudKernel.RingNormalOf(ring: c, key: k)),
        Area = Ring(key: 1, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.Area), key: k)),
        Perimeter = Ring(key: 2, measure: static (c, k) => k.AcceptValue(value: c.Native.Length)),
        EdgeAspect = Ring(key: 3, measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance, key: k)),
        Skewness = Ring(key: 4, measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c, key: k)),
        Compactness = Ring(key: 5, measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c, key: k)),
        MomentAnisotropy = Ring(key: 6, measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c, key: k)),
        RadiiOfGyration = Ring(key: 7, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k)),
        AreaError = Ring(key: 8, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.AreaError), key: k)),
        CentroidError = Ring(key: 9, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidError), key: k));
    public static readonly VectorCloudMetric Centroid = All(key: 10, measure: static (c, k) => CloudKernel.CentroidOf(cloud: c, key: k)),
        BestFitPlane = All(key: 11, measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c, key: k)),
        PrincipalAxes = All(key: 12, measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c, key: k)),
        PrincipalFrame = All(key: 13, measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c, key: k)),
        Shape = All(key: 14, measure: static (c, k) => CloudKernel.ShapeOf(cloud: c, key: k));
    public static readonly VectorCloudMetric BishopFrames = Chain(key: 15, measure: static (c, k) => NeighborKernel.BishopChain(cloud: c, key: k)),
        TangentFlow = Poly(key: 16, measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k)),
        CumulativeArcLength = Poly(key: 17, measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k)),
        EdgeCurvatures = Poly(key: 18, measure: static (pts, k) => CloudKernel.EdgeCurvaturesOf(points: pts, key: k)),
        OpenLength = Poly(key: 19, measure: static (pts, k) => CloudKernel.OpenLengthOf(points: pts, key: k));
    public static readonly VectorCloudMetric Covariance = Cluster(key: 20, measure: static (c, k) => CloudKernel.CovarianceOf(cluster: c, key: k).Map(static v => v.Cov)),
        PrincipalDirection = Cluster(key: 21, measure: static (c, k) => CloudKernel.PrincipalStatsOf(cluster: c, key: k).Bind(s => k.AcceptValue(value: CloudKernel.AsVector3d(v: s.Eigen[0].Eigenvector)))),
        Spread = Cluster(key: 22, measure: static (c, k) => CloudKernel.PrincipalStatsOf(cluster: c, key: k).Bind(s => k.AcceptValue(value: s.Spread))),
        OrientedNormals = Cluster(key: 23, measure: static (c, p, k) => NeighborKernel.OrientNormals(cluster: c, policy: p.Neighborhood, key: k)),
        PrincipalCurvature = Cluster(key: 24, measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p.Neighborhood, key: k)),
        Curvedness = Cluster(key: 25, measure: static (c, p, k) => NeighborKernel.Curvedness(cluster: c, policy: p.Neighborhood, key: k)),
        ShapeIndex = Cluster(key: 26, measure: static (c, p, k) => NeighborKernel.ShapeIndex(cluster: c, policy: p.Neighborhood, key: k)),
        Admission = Cluster(key: 27, measure: static (c, k) => Fin.Succ(c.Admission)),
        Neighborhood = Cluster(key: 28, measure: static (c, p, k) => NeighborKernel.ReceiptOf(cluster: c, policy: p.Neighborhood, key: k)),
        CurvatureReceipt = Cluster(key: 29, measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p.Neighborhood, key: k).Map(static r => r.Receipt));

    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, CloudMetricPolicy policy, Op key);

    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        CloudMetricPolicy.AdmitOrDefault(policy: None, key: key).Bind(policy => Project<TOut>(cloud: cloud, policy: policy, key: key));
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, CloudMetricPolicy policy, Op key) =>
        (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
            (false, _) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
            (_, false) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloudMetric), outputType: typeof(TOut))),
            _ => Measure(cloud: cloud, policy: policy, key: key).Bind(value => value switch {
                Seq<Vector3d> vs => ProjectSeq<Vector3d, TOut>(values: vs, key: key),
                Seq<double> ds => ProjectSeq<double, TOut>(values: ds, key: key),
                Seq<Plane> ps => ProjectSeq<Plane, TOut>(values: ps, key: key),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
        };
    private static Fin<TOut> ProjectSeq<TItem, TOut>(Seq<TItem> values, Op key) =>
        values.TraverseM(v => key.AcceptValue(value: v)).As().Map(static valid => (TOut)(object)valid);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudMetricPolicy(NeighborhoodPolicy Neighborhood) {
    internal static Fin<CloudMetricPolicy> AdmitOrDefault(Option<CloudMetricPolicy> policy, Op key) =>
        policy.Match(Some: p => p.Neighborhood.Admit(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)),
                     None: () => NeighborhoodPolicy.Default(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)));
}

// --- [MODELS] -----------------------------------------------------------------------------
// Field set is a cross-page contract: Analysis/inspect.md embeds VectorCloudShape inside MeshFaceShape.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCloudShape(
    Point3d Centroid, Plane PrincipalFrame, Seq<(double Moment, Vector3d Axis)> PrincipalAxes,
    Option<Vector3d> Normal, Option<double> SignedArea, Option<double> Area, Option<double> Perimeter,
    Option<double> EdgeAspect, Option<double> Skewness, Option<double> PlanarityDeviation,
    Option<double> Compactness, Option<double> MomentAnisotropy, Option<Vector3d> RadiiOfGyration,
    Option<double> AreaError, Option<Vector3d> CentroidError, Option<Plane> BestFitPlane,
    Option<bool> Convex, Option<CurveOrientation> Orientation, Option<double> OpenLength,
    Option<Vector3d> Spread) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Centroid),
        ValidityClaim.Of(PrincipalFrame.IsValid),
        ValidityClaim.CountExactly(count: PrincipalAxes.Count, expected: 3),
        ValidityClaim.Of(Area.Map(static a => ValidityClaim.Nonnegative(a).Holds).IfNone(true)),
        ValidityClaim.Of(Perimeter.Map(static p => ValidityClaim.Nonnegative(p).Holds).IfNone(true)),
        ValidityClaim.Of(Compactness.Map(static c => ValidityClaim.UnitInterval(c).Holds).IfNone(true)),
        ValidityClaim.Of(OpenLength.Map(static l => ValidityClaim.Nonnegative(l).Holds).IfNone(true)),
        ValidityClaim.Of(BestFitPlane.Map(static p => p.IsValid).IfNone(true)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Option<Arr<double>> mass, Op key) =>
        from moment in SampleMoment.Of(
            rows: points.Map(static p => new Arr<double>([p.X, p.Y, p.Z])), dimension: 3, key: key,
            weights: mass) // None rides SampleMoment's unweighted arm; a unit-weight array here is redundant.
        from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: moment.UpperCovariance, key: key)
        select (Mean: AsVector3d(v: moment.Mean), Cov: cov);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster, Op key) =>
        from mass in MassOf(cluster: cluster, key: key)
        from stats in CovarianceOf(points: cluster.Vertices, mass: Some(mass), key: key)
        select stats;
    internal static Fin<Arr<double>> MassOf(VectorCloud.ClusterCase cluster, Op key) =>
        MassOf(mass: cluster.Mass.IfNone(() => new Arr<double>([.. Enumerable.Repeat(1.0 / cluster.Vertices.Count, cluster.Vertices.Count)])), count: cluster.Vertices.Count, key: key);
    internal static Fin<Arr<double>> MassOf(Arr<double> mass, int count, Op key) =>
        from _ in guard(mass.Count == count && mass.ForAll(static w => double.IsFinite(w) && w > 0.0), key.InvalidInput())
        from total in mass.Fold(0.0, static (s, w) => s + w) switch {
            double sum when double.IsFinite(sum) && sum > 0.0 => Fin.Succ(sum),
            _ => Fin.Fail<double>(key.InvalidInput()),
        }
        select new Arr<double>([.. mass.AsIterable().Select(w => w / total)]);

    internal sealed record PrincipalStats(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen) {
        internal Seq<(double Moment, Vector3d Axis)> Axes => Eigen.Map(static p => (Moment: p.Eigenvalue, Axis: AsVector3d(v: p.Eigenvector)));
        internal Vector3d Spread => new(Eigen[0].Eigenvalue, Eigen[1].Eigenvalue, Eigen[2].Eigenvalue);
    }
    internal static Fin<PrincipalStats> PrincipalStatsOf(VectorCloud.ClusterCase cluster, Op key) =>
        from stats in CovarianceOf(cluster: cluster, key: key)
        from eigen in stats.Cov.DecomposeEigen(key: key)
        from full in eigen.Count >= 3
            ? Fin.Succ(new PrincipalStats(Mean: stats.Mean, Eigen: eigen))
            : Fin.Fail<PrincipalStats>(key.InvalidResult())
        select full;
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);

    internal static Fin<int> PlanarWindingOf(Seq<Point3d> ring, Vector3d planeNormal, Point3d query, Op key) =>
        ring.Count < 3
            ? Fin.Fail<int>(key.InvalidInput())
            : key.AcceptValue(value: (int)Math.Round(
                ring.Map((v, i) => (V0: v - query, V1: ring[(i + 1) % ring.Count] - query))
                    .Fold(0.0, (sum, pair) => sum + Vector3d.VectorAngle(v1: pair.V0, v2: pair.V1, vNormal: planeNormal)) / (2.0 * Math.PI),
                MidpointRounding.ToEven));
}
```

## [04]-[HULL]

- Owner: `CloudHullKind` names the hull species, `FootprintWrapper` the 2D fallback a rejected 3D hull degrades to; concave columns `Alpha` and `Lambda` derive from the cluster's mean spacing when the caller supplies neither.
- Entry: `ComputeHullDetailed` is cluster-only, and every declared kind computes, so `CloudHullStatus` discriminates outcome alone.
- Auto: `Convex3D` routes native through `Mesh.CreateConvexHull3D` behind a coplanar preflight, duplicating the mesh out of its `using` window; `ConvexFootprint2D` and `FootprintWrapper` fit the PCA plane, run `PolylineCurve.CreateConvexHull2d`, verify containment within tolerance, and mesh via `Mesh.CreateFromClosedPolyline`. `AlphaShape` keeps every triangle whose circumradius stays within `Alpha`; `ConcaveOutline` erodes the longest boundary edge while it exceeds `Lambda` and removal preserves regularity, abandoning no vertex and leaving the boundary a single simple cycle.
- Packages: RhinoCommon (native convex hull, plane fitting, polyline meshing), MIConvexHull (`Triangulation.CreateDelaunay<DefaultVertex>`; planar rows enter as `DefaultVertex{ Position }` per its `IVertex` contract), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new hull species is one kind row and one arm in the hull fold, or one filter predicate over the shared Delaunay fold; a new concave criterion is one policy column.
- Boundary: both concave kinds share ONE Delaunay fold over `MIConvexHull`'s complex, the filter predicate their only difference; `Triangulation.CreateDelaunay` is the one foreign-exception seam on this page, funneled through `key.Catch` into `Rejected` evidence. This rail owns the native-first host and concave hull kinds; the predicate-exact envelope fold homes at `Meshing/delaunay` `LowerHull`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new(key: 0);
    public static readonly CloudHullKind ConvexFootprint2D = new(key: 1);
    public static readonly CloudHullKind ConcaveOutline = new(key: 2);
    public static readonly CloudHullKind AlphaShape = new(key: 3);
    public static readonly CloudHullKind FootprintWrapper = new(key: 4);
}

[SmartEnum<int>]
public sealed partial class CloudHullStatus {
    public static readonly CloudHullStatus Completed = new(key: 0);
    public static readonly CloudHullStatus Rejected = new(key: 1);
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullPolicy(
    PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
    Option<PositiveMagnitude> Alpha, Option<PositiveMagnitude> Lambda) {
    internal static Fin<CloudHullPolicy> AdmitOrDefault(Option<CloudHullPolicy> policy, Context context, Op key) {
        (double tolerance, double angle, Option<PositiveMagnitude> alpha, Option<PositiveMagnitude> lambda) = policy.Match(
            Some: static candidate => (candidate.Tolerance.Value, candidate.AngleTolerance.Value, candidate.Alpha, candidate.Lambda),
            None: () => (context.Absolute.Value, context.Angle.Value, Option<PositiveMagnitude>.None, Option<PositiveMagnitude>.None));
        return from admittedTolerance in key.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from admittedAngle in key.AcceptValidated<VectorAngle>(candidate: angle)
               from admittedAlpha in AdmitMagnitude(value: alpha, key: key)
               from admittedLambda in AdmitMagnitude(value: lambda, key: key)
               select new CloudHullPolicy(Tolerance: admittedTolerance, AngleTolerance: admittedAngle, Alpha: admittedAlpha, Lambda: admittedLambda);
    }
    private static Fin<Option<PositiveMagnitude>> AdmitMagnitude(Option<PositiveMagnitude> value, Op key) =>
        value.Match(
            Some: magnitude => key.AcceptValidated<PositiveMagnitude>(candidate: magnitude.Value).Map(static admitted => Some(admitted)),
            None: static () => Fin.Succ(Option<PositiveMagnitude>.None));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullReceipt(
    CloudHullKind Kind, CloudHullStatus Status, double Tolerance, double AngleTolerance,
    int InputCount, int OutputVertexCount, int NativeFacetCount, int SurvivingTriangleCount,
    Option<double> PlanarityDeviation, Option<double> EffectiveAlpha, Option<double> EffectiveLambda,
    bool CoplanarRejected, int ContainmentRejectedCount, bool NativeRouted, bool Fallback) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(Tolerance),
        ValidityClaim.Nonnegative(AngleTolerance),
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        ValidityClaim.Of(OutputVertexCount >= 0 && NativeFacetCount >= 0 && SurvivingTriangleCount >= 0 && ContainmentRejectedCount >= 0),
        ValidityClaim.Of(PlanarityDeviation.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveAlpha.Map(static a => ValidityClaim.Positive(a).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveLambda.Map(static l => ValidityClaim.Positive(l).Holds).IfNone(true)),
        ValidityClaim.Of(!Status.Equals(CloudHullStatus.Completed) || OutputVertexCount >= 3));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(Option<Mesh> Mesh, CloudHullReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudHullResult self = this;
        return AtomProjection.Rows<CloudHullResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudHullReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<Mesh>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(Mesh)))
                .Bind(mesh => key.AcceptValue(value: mesh))),
            ProjectionRow.Of<VectorCloud>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(VectorCloud)))
                .Bind(mesh => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context, key: key))));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// CloudKernel concave fold: project(PCA plane) -> DefaultVertex{Position} -> Triangulation.CreateDelaunay
//   -> filter(cells) -> boundary(one-incident edges) -> orient CCW -> lift -> Mesh.CreateFromClosedPolyline
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
