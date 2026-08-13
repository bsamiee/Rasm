# [RASM_CLOUD]

`VectorCloud` owns the point-cloud union under one admission that deduplicates by tolerance, renormalizes mass, and carries a copy-safe shared native index per cluster. `VectorCloudMetric` folds every cloud measurement behind one `Project<TOut>`, each row naming the `CloudKernel` fold that answers it, so a new cloud capability lands as a metric row, a hull-kind row, or a shape column.

`CloudKernel.CovarianceOf` is the corpus's one covariance fold, composing `Domain/stats.md` `SampleMoment` into a `matrix.md` `SymmetricMatrix` every PCA consumer reads.

## [01]-[INDEX]

- [02]-[VECTOR_CLOUD]: `VectorCloud` folds every cloud case under tolerance-dedup admission with the lazy cluster index and closest-vertex probe.
- [03]-[CLOUD_METRICS]: `VectorCloudMetric` projects every measurement through one `Project<TOut>` over the kernel folds.
- [04]-[HULL]: `CloudHullKind` rails native convex, faceted, planar, and Delaunay-filtered concave hulls into typed receipts.
- [05]-[VORONOI_COMPLEX]: `CloudVoronoiCell` decomposes a cluster cloud into its 3D dual cells, skeleton, and bound census.

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
using MIConvexHull;
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
                // One-dispatch-site law holds corpus-wide: the sequence arms ride AtomProjection.Values, so no
                // page-local reflection-branching helper stands beside the sanctioned dispatch site.
                Seq<Vector3d> vs => AtomProjection.Values<Vector3d, TOut>(values: vs, key: key, owner: typeof(VectorCloudMetric)),
                Seq<double> ds => AtomProjection.Values<double, TOut>(values: ds, key: key, owner: typeof(VectorCloudMetric)),
                Seq<Plane> ps => AtomProjection.Values<Plane, TOut>(values: ps, key: key, owner: typeof(VectorCloudMetric)),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
        };
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
internal static partial class CloudKernel {
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
        from eigen in stats.Cov.DecomposeEigenDetailed(key: key).Bind(receipt => receipt.PairsIn(expected: EigenOrder.DescendingMagnitude, key: key))
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

- Owner: `CloudHullKind` names the hull species, `FootprintWrapper` the 2D fallback a rejected 3D hull degrades to; concave columns `Alpha` and `Lambda` derive from the cluster's mean spacing when the caller supplies neither. `CloudFoldStatus` is the cloud-decomposition outcome both this rail and the `[05]` dual publish, and `CloudHullRejection` keys the typed cause off the MIConvexHull ordinals.
- Entry: `ComputeHullDetailed` is cluster-only, and every declared kind computes, so `CloudFoldStatus` discriminates outcome alone.
- Auto: `Convex3D` routes native through `Mesh.CreateConvexHull3D` behind a coplanar preflight, duplicating the mesh out of its `using` window; `ConvexFootprint2D` and `FootprintWrapper` fit the PCA plane, run `PolylineCurve.CreateConvexHull2d`, verify containment within tolerance, and mesh via `Mesh.CreateFromClosedPolyline`. `AlphaShape` keeps every triangle whose circumradius stays within `Alpha`; `ConcaveOutline` erodes the longest boundary edge while it exceeds `Lambda` and removal preserves regularity, abandoning no vertex and leaving the boundary a single simple cycle. `Faceted3D` and `IndexedFootprint2D` are the index-preserving twins of the two host routes — the host `Mesh` and `PolylineCurve` answer geometry and drop which cluster vertex each output came from, so the typed rows keep facet adjacency, per-facet outward normals, and the cluster index every downstream census and dual keys on.
- Packages: RhinoCommon (native convex hull, plane fitting, polyline meshing), MIConvexHull (`Triangulation.CreateDelaunay<CloudVertex, CloudCell>`, `ConvexHull.Create<CloudVertex, CloudFace>` with `ConvexHull.Faces`/`ConvexFace.Adjacency`/`.Normal`, `ConvexHull.Create2D<CloudPlanarVertex>`, `ConvexHullCreationResult.Outcome`/`.Result` — the index-carrying `IVertex`/`IVertex2D` and circumsphere-carrying `TriangulationCell` generics, tolerance threaded from the admitted policy), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new hull species is one kind row and one arm in the hull fold, or one filter predicate over the shared Delaunay fold; a new concave criterion is one policy column; a new rejection cause is one `CloudHullRejection` row keyed off its package ordinal.
- Boundary: both concave kinds share ONE Delaunay fold over `MIConvexHull`'s complex, the filter predicate their only difference; `Triangulation.CreateDelaunay` is the one foreign-exception seam on this rail, funneled through `key.Catch` into `Rejected` evidence whose `Rejection` column reads `ConvexHullGenerationException.Error` — the same outcome vocabulary, so a caught degeneracy and a returned one name the cause identically. `ConvexHull.*` catches internally and returns that outcome instead of throwing, so `Faceted3D` and `IndexedFootprint2D` gate `Outcome` ahead of `Result` on the `Fin` fold and take no `Catch`. This rail owns the native-first host, index-preserving, and concave hull kinds; the predicate-exact hull fold homes at `Meshing/delaunay` `LowerHull`, and `SolidOf` is the one volume-and-centroid producer both this rail and `[05]` read.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new(key: 0);
    public static readonly CloudHullKind ConvexFootprint2D = new(key: 1);
    public static readonly CloudHullKind ConcaveOutline = new(key: 2);
    public static readonly CloudHullKind AlphaShape = new(key: 3);
    public static readonly CloudHullKind FootprintWrapper = new(key: 4);
    public static readonly CloudHullKind Faceted3D = new(key: 5);
    public static readonly CloudHullKind IndexedFootprint2D = new(key: 6);
}

// One outcome vocabulary for every cloud decomposition on this page — the hull rail and the [05] dual share the
// admission path, the receipt shape, and the consumer, so a second Completed/Rejected pair would be one concept twice.
[SmartEnum<int>]
public sealed partial class CloudFoldStatus {
    public static readonly CloudFoldStatus Completed = new(key: 0);
    public static readonly CloudFoldStatus Rejected = new(key: 1);
}

// Keys MIRROR the package ordinals, so the seam admits (int)result.Outcome and a roster the package grows lands as
// one row rather than a silently-unmapped literal; Success is the Fin success arm and mints no row.
[SmartEnum<int>]
public sealed partial class CloudHullRejection {
    public static readonly CloudHullRejection LowDimension = new(key: (int)ConvexHullCreationResultOutcome.DimensionSmallerTwo);
    public static readonly CloudHullRejection PlanarRoute = new(key: (int)ConvexHullCreationResultOutcome.DimensionTwoWrongMethod);
    public static readonly CloudHullRejection SparseInput = new(key: (int)ConvexHullCreationResultOutcome.NotEnoughVerticesForDimension);
    public static readonly CloudHullRejection MixedDimension = new(key: (int)ConvexHullCreationResultOutcome.NonUniformDimension);
    public static readonly CloudHullRejection Degenerate = new(key: (int)ConvexHullCreationResultOutcome.DegenerateData);
    public static readonly CloudHullRejection Unknown = new(key: (int)ConvexHullCreationResultOutcome.UnknownError);
    internal static Fin<Option<CloudHullRejection>> Of(ConvexHullCreationResultOutcome outcome, Op key) =>
        outcome is ConvexHullCreationResultOutcome.Success ? Fin.Succ(Option<CloudHullRejection>.None)
        : TryGet((int)outcome, out CloudHullRejection? row) ? Fin.Succ(Some(row!))
        : Fin.Fail<Option<CloudHullRejection>>(key.InvalidResult());
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

// One facet row per typed hull face: the cluster INDEX per corner, the neighbour ordinal across each ridge, and the
// outward unit normal — the three facts the host `Mesh` route drops on the way out.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudFacet(Arr<int> Vertices, Arr<int> Adjacency, Vector3d Normal) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Vertices.Count, floor: 3),
        ValidityClaim.Of(Vertices.ForAll(static v => v >= 0)),
        ValidityClaim.CountExactly(count: Adjacency.Count, expected: Vertices.Count),
        // A convex hull is a closed manifold, so every ridge is shared by exactly two facets and the package fills
        // every slot; a -1 here is a torn hull, not a boundary — the Delaunay complex is where nulls are lawful.
        ValidityClaim.Of(Adjacency.ForAll(static a => a >= 0)),
        ValidityClaim.Finite(Normal));
}

// Volume, centroid, and facets of one convex body — the shared product SolidOf mints for a hull kind and for a
// bounded dual cell alike, so neither re-derives the divergence fold.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudSolid(double Volume, Point3d Centroid, Seq<CloudFacet> Facets) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(Volume),
        ValidityClaim.Finite(Centroid),
        ValidityClaim.CountAtLeast(count: Facets.Count, floor: 4),
        ValidityClaim.Of(Facets.ForAll(static f => f.IsValid)));
}

// Every count an arm may not take rides an Option: a native route measures no surviving triangle, a Delaunay-filtered
// route no facet, and a 3D route no containment rejection, so a 0 here always spells a fold that ran and found none.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullReceipt(
    CloudHullKind Kind, CloudFoldStatus Status, double Tolerance, double AngleTolerance,
    int InputCount, int OutputVertexCount, Option<int> FacetCount, Option<int> SurvivingTriangleCount,
    Option<int> ContainmentRejectedCount, Option<double> PlanarityDeviation, Option<double> EffectiveAlpha,
    Option<double> EffectiveLambda, Option<CloudHullRejection> Rejection,
    bool CoplanarRejected, bool NativeRouted, bool Fallback) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(Tolerance),
        ValidityClaim.Nonnegative(AngleTolerance),
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        ValidityClaim.Of(OutputVertexCount >= 0),
        ValidityClaim.Of(FacetCount.Map(static c => c >= 0).IfNone(true)),
        ValidityClaim.Of(SurvivingTriangleCount.Map(static c => c >= 0).IfNone(true)),
        ValidityClaim.Of(ContainmentRejectedCount.Map(static c => c >= 0).IfNone(true)),
        ValidityClaim.Of(PlanarityDeviation.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveAlpha.Map(static a => ValidityClaim.Positive(a).Holds).IfNone(true)),
        ValidityClaim.Of(EffectiveLambda.Map(static l => ValidityClaim.Positive(l).Holds).IfNone(true)),
        // Only the typed-outcome ConvexHull.* rows name a cause, and a cause rides the Rejected arm alone.
        ValidityClaim.Of(Rejection.IsNone || Status.Equals(CloudFoldStatus.Rejected)),
        ValidityClaim.Of(!Status.Equals(CloudFoldStatus.Completed) || OutputVertexCount >= 3));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(Option<Mesh> Mesh, Option<CloudSolid> Solid, CloudHullReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudHullResult self = this;
        return AtomProjection.Rows<CloudHullResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudHullReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<CloudSolid>(() => self.Solid.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(CloudSolid)))),
            ProjectionRow.Of<Seq<CloudFacet>>(() => self.Solid.Map(static solid => solid.Facets)
                .ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(Seq<CloudFacet>)))),
            ProjectionRow.Of<Mesh>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(Mesh)))
                .Bind(mesh => key.AcceptValue(value: mesh))),
            ProjectionRow.Of<VectorCloud>(() => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(VectorCloud)))
                .Bind(mesh => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context, key: key))));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// CloudKernel concave fold: project(PCA plane) -> CloudVertex{Index, Position} ->
//   Triangulation.CreateDelaunay<CloudVertex, CloudCell>(PlaneDistanceTolerance: policy.Tolerance.Value)
//   -> Circumsphere sweep -> filter(cells by cluster INDEX + per-cell circumradius) -> boundary(one-incident edges)
//   -> orient CCW -> lift -> Mesh.CreateFromClosedPolyline
// Vertices CARRY the cluster index — cell.Vertices[j].Index reads it directly, because a position re-match is
// unsafe on a deduplicated index-stable cluster — and the threaded tolerance ends the package-default 1e-10
// degeneracy verdict that judged metres and millimetres identically. The cell caches the circumsphere the alpha
// filter otherwise recomputes per triangle, and ConcaveOutline's erosion keys on the same indices.
// Faceted3D fold: CloudVertex -> SolidOf -> CloudSolid{Volume, Centroid, Facets} with no mesh leg.
// IndexedFootprint2D fold: project(PCA plane) -> CloudPlanarVertex{Index, X, Y} ->
//   ConvexHull.Create2D<CloudPlanarVertex>(tolerance: policy.Tolerance.Value) -> gate Outcome -> lift the returned
//   boundary order back through the plane. Create2D returns the CALLER's own instances in hull order, so the cluster
//   index survives where PolylineCurve.CreateConvexHull2d hands back bare coordinates a position re-match must guess.
internal sealed class CloudVertex(int index, double[] position) : IVertex {
    public int Index { get; } = index;
    public double[] Position { get; } = position;
}

// The package allocates every FACE, CELL, and EDGE carrier itself and fills the inherited columns, so each is a
// settable-property class satisfying the new() bound; a VERTEX carrier is the caller's own instance surviving the
// hull, and the planar new() is a declaration-site bound the monotone-chain path never exercises — its object
// initializer at the one projection site is the only construction, so the zero-coordinate default never mints.
internal sealed class CloudFace : ConvexFace<CloudVertex, CloudFace>;

internal sealed class CloudPlanarVertex : IVertex2D {
    internal int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
}

internal sealed class CloudCell : TriangulationCell<CloudVertex, CloudCell> {
    // One Circumsphere sweep writes both columns for EVERY cell before any filter or dual reads them; a degenerate
    // simplex leaves both None, so an alpha filter never admits a cell on a fabricated 0.0 radius.
    internal Option<Point3d> Circumcenter { get; set; }
    internal Option<double> Circumradius { get; set; }
    // A null adjacency slot is the hull facet opposite that vertex — lawful on a Delaunay complex, torn on a hull.
    internal bool Boundary => Array.Exists(array: Adjacency, match: static face => face is null);
}

internal static partial class CloudKernel {
    // ConvexHull.* carries a typed outcome and never throws, so the gate reads Outcome ahead of Result and this fold
    // stays off key.Catch; ConvexFace.Normal is the outward unit normal, so each facet's tetrahedron over the interior
    // anchor carries its own sign and the divergence fold needs no winding convention.
    internal static Fin<Option<CloudSolid>> SolidOf(Point3d[] points, double tolerance, Op key) {
        ConvexHullCreationResult<CloudVertex, CloudFace> hull = ConvexHull.Create<CloudVertex, CloudFace>(
            data: [.. points.Select(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z]))],
            tolerance: tolerance);
        return CloudHullRejection.Of(outcome: hull.Outcome, key: key).Bind(rejection => rejection.IsSome
            ? Fin.Succ(Option<CloudSolid>.None)
            : Accumulate(anchor: points.Aggregate(Point3d.Origin, static (sum, p) => sum + p) / points.Length,
                faces: toSeq(hull.Result.Faces), key: key));
    }

    private static Fin<Option<CloudSolid>> Accumulate(Point3d anchor, Seq<CloudFace> faces, Op key) {
        (double volume, Vector3d moment) = faces.Fold((Volume: 0.0, Moment: Vector3d.Zero), (acc, face) => {
            (Vector3d u, Vector3d v, Vector3d w) = (PointOf(face.Vertices[0]) - anchor, PointOf(face.Vertices[1]) - anchor, PointOf(face.Vertices[2]) - anchor);
            double tet = Math.Abs(Vector3d.CrossProduct(a: v - u, b: w - u) * u) / 6.0;
            return (acc.Volume + tet, acc.Moment + (tet * 0.25 * (u + v + w)));
        });
        // CloudFace overrides no equality, so the default comparer IS reference identity and the ordinal map turns
        // each Adjacency reference into the facet index CloudFacet publishes.
        Dictionary<CloudFace, int> ordinal = faces.Map(static (face, index) => (Face: face, Index: index))
            .ToDictionary(static row => row.Face, static row => row.Index);
        CloudSolid solid = new(Volume: volume, Centroid: anchor + (moment / volume),
            Facets: faces.Map(face => new CloudFacet(
                Vertices: new Arr<int>([.. face.Vertices.Select(static corner => corner.Index)]),
                Adjacency: new Arr<int>([.. face.Adjacency.Select(neighbor => neighbor is null ? -1 : ordinal[neighbor])]),
                Normal: new Vector3d(x: face.Normal[0], y: face.Normal[1], z: face.Normal[2]))));
        // A vanishing volume is a flat point set the outcome gate let through, so it answers None; a solid that fails
        // its own evidence is a torn hull and faults, because no caller can act on a half-closed body.
        return volume <= EpsilonPolicy.ZeroTolerance ? Fin.Succ(Option<CloudSolid>.None)
            : solid.IsValid ? Fin.Succ(Some(solid))
            : Fin.Fail<Option<CloudSolid>>(key.InvalidResult());
    }

    internal static Point3d PointOf(CloudVertex vertex) => new(x: vertex.Position[0], y: vertex.Position[1], z: vertex.Position[2]);
}
```

## [05]-[VORONOI_COMPLEX]

- Owner: `CloudVoronoiCell` is the 3D dual cell over a cluster cloud — one row per site — and `CloudCellBound` is the vocabulary deciding which measures that row carries, so an unbounded or degenerate cell publishes `None` where a bounded one publishes volume, centroid, and extent.
- Entry: `ComputeVoronoiDetailed` is cluster-only and consumes the already-admitted `CloudHullPolicy`, reading `Tolerance` alone as the `PlaneDistanceTolerance` the dual threads; `Alpha`, `Lambda`, and `AngleTolerance` are concave-hull columns this fold never reads. `NaturalNeighborWeights` is the Sibson stolen-volume fold — two duals, sites then sites-plus-query, each neighbour weighted by the bounded volume the insertion steals, normalized to sum one — the one weight source the `Meshing/reconstruct` evaluator composes.
- Auto: each Delaunay cell IS a Voronoi vertex, so the circumsphere sweep over `VoronoiMesh.Vertices` mints the whole vertex array once and `VoronoiEdge.Source`/`.Target` read the cell pair whose circumcenters bound one dual edge — the 1-skeleton falls out as `Skeleton` with no second traversal. Bound classification derives structurally, never by proximity heuristic: `SolidOf` over the sites answers the site hull in the same pass that answers `HullVolume`, and a site on that hull owns an open cell because the Voronoi region of a hull vertex extends to infinity. A bounded cell's geometry is the convex hull of its incident circumcenters, so `SolidOf` answers its volume and centroid too and the `[04]` faceted row is this band's measurement kernel.
- Receipt: `CloudVoronoiReceipt` carries the tolerance and input count on both arms and the whole tally inside `Option<CloudVoronoiCensus>`, so a rejected fold publishes no zero-filled counts; `BoundedVolumeTotal` never exceeds `HullVolume`, and that ordering is the census's own conservation claim. `ConvexHullGenerationException.Error` is the same `ConvexHullCreationResultOutcome` the typed-result family publishes, so the rejected arm reads its cause off the caught exception into the same `CloudHullRejection` column the `[04]` rows fill.
- Packages: MIConvexHull (`VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>`, `VoronoiMesh.Vertices`/`.Edges`, `VoronoiEdge.Source`/`.Target`, `ConvexHull.Create<CloudVertex, CloudFace>` through `SolidOf`), RhinoCommon, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new per-cell measure is one `Option` column on `CloudVoronoiCell` plus its arm in the bounded fold; a new bound species is one `CloudCellBound` row; a new census tally is one column on `CloudVoronoiCensus`.
- Boundary: this band owns the 3D cell decomposition alone — 2D border-clipped point-site Voronoi homes at `Meshing/delaunay` `Tessellation.VoronoiDual`, whose bounded-cell overload is the predicate-exact planar peer, and `Meshing/offset` reads that owner for the medial locus. `VoronoiMesh.Create` returns the bare complex and throws `ConvexHullGenerationException` on degenerate input, so this fold keeps the page's `key.Catch` → `Rejected` funnel where the `ConvexHull.*` rows read their typed outcome instead. Natural-neighbour interpolation reads `Volume` from here and fits nothing; the admitting minter is `Meshing/reconstruct`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudCellBound {
    public static readonly CloudCellBound Bounded = new(key: 0);
    public static readonly CloudCellBound Unbounded = new(key: 1);
    public static readonly CloudCellBound Degenerate = new(key: 2);
}

internal sealed class CloudVoronoiEdge : VoronoiEdge<CloudVertex, CloudCell> {
    // Source and Target are the Delaunay-cell pair whose circumcenters bound one dual edge; the segment is the
    // derived read, and a degenerate endpoint answers None rather than a Line through an unmeasured corner.
    internal Option<Line> Dual =>
        from tail in Source.Circumcenter
        from head in Target.Circumcenter
        select new Line(from: tail, to: head);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Site is the cluster index the seed came in on, Vertices indexes the result's compacted dual-vertex array, and
// Neighbors carries the natural-neighbour sites — the stolen-volume weight set an interpolant reads Volume against.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiCell(
    int Site, Point3d Seed, CloudCellBound Bound, Arr<int> Vertices, Arr<int> Neighbors,
    Option<double> Volume, Option<Point3d> Centroid, Option<double> Extent) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Site >= 0),
        ValidityClaim.Finite(Seed),
        ValidityClaim.CountAtLeast(count: Vertices.Count, floor: 1),
        ValidityClaim.Of(Vertices.ForAll(static v => v >= 0)),
        ValidityClaim.Of(Neighbors.ForAll(neighbor => neighbor >= 0 && neighbor != Site)),
        // Bounded is the ONE measuring arm: the other two rows carry None by structure, and a 0.0 volume on an open
        // cell would spell a measurement no fold took.
        ValidityClaim.Of(Bound.Equals(CloudCellBound.Bounded)
            ? Volume.Map(static v => ValidityClaim.Positive(v).Holds).IfNone(false)
              && Centroid.Map(static c => ValidityClaim.Finite(c).Holds).IfNone(false)
              && Extent.Map(static e => ValidityClaim.Positive(e).Holds).IfNone(false)
            : Volume.IsNone && Centroid.IsNone && Extent.IsNone));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
// DualVertexCount tallies Delaunay cells and UnmeasuredVertexCount the degenerate simplices among them, so the two
// answer the dual's VERTEX plane while the three cell counts answer its SITE plane — one name per plane, never one
// count read two ways. MeasuredVertexCount is the published Vertices array length every index column addresses.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiCensus(
    int DualVertexCount, int UnmeasuredVertexCount, int DualEdgeCount, int SkeletonEdgeCount,
    int BoundedCellCount, int UnboundedCellCount, int DegenerateCellCount,
    Option<double> BoundedVolumeTotal, Option<double> HullVolume) : IValidityEvidence {
    internal int MeasuredVertexCount => DualVertexCount - UnmeasuredVertexCount;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: DualVertexCount, floor: 1),
        ValidityClaim.Of(UnmeasuredVertexCount >= 0 && UnmeasuredVertexCount <= DualVertexCount),
        ValidityClaim.Of(DualEdgeCount >= 0 && SkeletonEdgeCount >= 0 && SkeletonEdgeCount <= DualEdgeCount),
        ValidityClaim.Of(BoundedCellCount >= 0 && UnboundedCellCount >= 0 && DegenerateCellCount >= 0),
        // Every bounded cell measured, so the totals move together and neither stands without the other.
        ValidityClaim.Of(BoundedVolumeTotal.IsSome == (BoundedCellCount > 0)),
        // The bounded cells tile a subset of the site hull, so their volume never exceeds it.
        ValidityClaim.Of((BoundedVolumeTotal.Case, HullVolume.Case) switch {
            (double bounded, double hull) => ValidityClaim.Positive(hull).Holds && ValidityClaim.Ordered(lower: bounded, upper: hull).Holds,
            _ => BoundedVolumeTotal.IsNone,
        }));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiReceipt(
    CloudFoldStatus Status, double PlaneDistanceTolerance, int InputCount,
    Option<CloudHullRejection> Rejection, Option<CloudVoronoiCensus> Census) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(PlaneDistanceTolerance),
        // Four affinely independent sites are the floor for a 3D complex; fewer is an admission refusal, not a dual.
        ValidityClaim.CountAtLeast(count: InputCount, floor: 4),
        // The census IS the completion evidence: a rejected fold measured nothing and publishes no tallies at all.
        ValidityClaim.Of(Status.Equals(CloudFoldStatus.Completed) == Census.IsSome),
        ValidityClaim.Of(Rejection.IsNone || Status.Equals(CloudFoldStatus.Rejected)),
        ValidityClaim.Of(Census.Map(static c => ValidityClaim.Evidence(c).Holds).IfNone(true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiResult(
    Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton, CloudVoronoiReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudVoronoiResult self = this;
        return AtomProjection.Rows<CloudVoronoiResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudVoronoiReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<Seq<CloudVoronoiCell>>(() => Fin.Succ(self.Cells)),
            ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(toSeq(self.Skeleton.AsIterable()
                .Select(edge => new Line(from: self.Vertices[edge.Tail], to: self.Vertices[edge.Head]))))),
            ProjectionRow.Of<VectorCloud>(() => VectorCloud.Cluster(
                points: toSeq(self.Vertices.AsIterable()), context: context, key: key)));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class CloudKernel {
    internal static Fin<CloudVoronoiResult> ComputeVoronoiDetailed(VectorCloud.ClusterCase cluster, CloudHullPolicy policy, Op key) =>
        from _ in guard(cluster.Vertices.Count >= 4, key.InvalidInput()).ToFin()
        let sites = cluster.Vertices.Map(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z])).ToArray()
        // The dual family returns the bare complex and THROWS on degenerate input — the one foreign-exception seam
        // this band owns — so the Catch funnels into a census-free Rejected receipt, never a zero-filled one.
        from result in key.Catch(() => CensusOf(
                sites: sites, tolerance: policy.Tolerance.Value, key: key,
                complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(
                    data: sites, PlaneDistanceTolerance: policy.Tolerance.Value)))
            // Only the package's OWN degeneracy throw degrades to a Rejected receipt, and it carries the same typed
            // outcome the ConvexHull.* result publishes, so the funnel names the cause; every other fault — an
            // unmapped outcome, an invalid census — stays a fault, because neither is a statement about the input.
            .BindFail(fault => fault is { Exception.Case: ConvexHullGenerationException generated }
                ? CloudHullRejection.Of(outcome: generated.Error, key: key).Map(cause => new CloudVoronoiResult(
                    Cells: Seq<CloudVoronoiCell>.Empty, Vertices: Arr<Point3d>.Empty, Skeleton: Arr<(int Tail, int Head)>.Empty,
                    Receipt: new CloudVoronoiReceipt(Status: CloudFoldStatus.Rejected,
                        PlaneDistanceTolerance: policy.Tolerance.Value, InputCount: sites.Length,
                        Rejection: cause, Census: None)))
                : Fin.Fail<CloudVoronoiResult>(fault))
        select result;

    // Sibson natural-neighbour coordinates by stolen volume — TWO dual folds, sites alone then sites plus the
    // query, and each neighbour's weight is the volume its cell LOSES to the inserted site, normalized to sum one.
    // The exact-support law rules the refusals: a query whose inserted cell is not Bounded lies outside or on the
    // sample hull where the interpolant is undefined, and a neighbour whose loss is unmeasurable — an open cell in
    // either fold — marks the same boundary from the inside; both refuse typed, never an extrapolated or
    // zero-filled weight. The positive filter absorbs float-noise on a grazing neighbour; the reconstruct.md
    // partition gate reconciles the normalized set downstream. Degenerate builds fail typed here — a Sibson read
    // over a degenerate site set is a refusal, never a Rejected receipt product.
    internal static Fin<Seq<(int Site, double Weight)>> NaturalNeighborWeights(Seq<Point3d> sites, Point3d query, double tolerance, Op key) {
        CloudVertex[] before = sites.Map(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z])).ToArray();
        CloudVertex[] after = [.. before, new CloudVertex(index: before.Length, position: [query.X, query.Y, query.Z])];
        return from _ in guard(sites.Count >= 4, key.InvalidInput()).ToFin()
               from first in key.Catch(() => CensusOf(sites: before, tolerance: tolerance, key: key,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(data: before, PlaneDistanceTolerance: tolerance)))
               from second in key.Catch(() => CensusOf(sites: after, tolerance: tolerance, key: key,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(data: after, PlaneDistanceTolerance: tolerance)))
               from inserted in second.Cells.Find(cell => cell.Site == before.Length).ToFin(key.InvalidInput())
               from _support in guard(inserted.Bound.Equals(CloudCellBound.Bounded), key.InvalidInput()).ToFin()
               from losses in toSeq(inserted.Neighbors.AsIterable().Filter(site => site != before.Length))
                   .TraverseM(site => VolumeLoss(first: first, second: second, site: site).ToFin(key.InvalidInput())).As()
               let positive = losses.Filter(static row => row.Loss > 0.0)
               let total = positive.Sum(static row => row.Loss)
               from _unity in guard(positive.Count >= 1 && double.IsFinite(total) && total > 0.0, key.InvalidResult()).ToFin()
               select positive.Map(row => (row.Site, Weight: row.Loss / total));
    }

    private static Option<(int Site, double Loss)> VolumeLoss(CloudVoronoiResult first, CloudVoronoiResult second, int site) =>
        from was in first.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
        from now in second.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
        select (Site: site, Loss: was - now);

    private static Fin<CloudVoronoiResult> CensusOf(
        CloudVertex[] sites, VoronoiMesh<CloudVertex, CloudCell, CloudVoronoiEdge> complex, double tolerance, Op key) {
        CloudCell[] cells = [.. complex.Vertices];
        foreach (CloudCell cell in cells) {
            (cell.Circumcenter, cell.Circumradius) = Circumsphere(cell: cell);
        }
        return from hull in SolidOf(points: [.. sites.Select(PointOf)], tolerance: tolerance, key: key)
               // A site on the site hull owns an open cell, so the hull's own facet corners ARE the unbounded set —
               // structural, never a proximity heuristic, and free because HullVolume already paid for the hull.
               let open = hull.Map(static solid => solid.Facets.Fold(Set<int>.Empty,
                   static (acc, facet) => facet.Vertices.Fold(acc, static (set, corner) => set.Add(corner)))).IfNone(Set<int>.Empty)
               from rows in CellRows(cells: cells, complex: complex, open: open, sites: sites, tolerance: tolerance, key: key)
               let census = new CloudVoronoiCensus(
                   DualVertexCount: cells.Length, UnmeasuredVertexCount: cells.Length - rows.Vertices.Count,
                   DualEdgeCount: complex.Edges.Count(), SkeletonEdgeCount: rows.Skeleton.Count,
                   BoundedCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Bounded)),
                   UnboundedCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Unbounded)),
                   DegenerateCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Degenerate)),
                   BoundedVolumeTotal: rows.Cells.Bind(static c => c.Volume.ToSeq()) is { IsEmpty: false } measured
                       ? Some(measured.Fold(0.0, static (acc, volume) => acc + volume))
                       : Option<double>.None,
                   HullVolume: hull.Map(static solid => solid.Volume))
               from verified in census.IsValid ? Fin.Succ(census) : Fin.Fail<CloudVoronoiCensus>(key.InvalidResult())
               select new CloudVoronoiResult(Cells: rows.Cells, Vertices: rows.Vertices, Skeleton: rows.Skeleton,
                   Receipt: new CloudVoronoiReceipt(Status: CloudFoldStatus.Completed,
                       PlaneDistanceTolerance: tolerance, InputCount: sites.Length, Census: Some(verified)));
    }

    private static Fin<(Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton)> CellRows(
        CloudCell[] cells, VoronoiMesh<CloudVertex, CloudCell, CloudVoronoiEdge> complex, Set<int> open,
        CloudVertex[] sites, double tolerance, Op key) { /* Cells ARE the Voronoi vertices, so ONE pass over cells
        maps each MEASURED cell to its slot in the published Vertices array and drops the unmeasured ones — every
        index column downstream addresses that compacted array, and the drop count is the census's
        UnmeasuredVertexCount. The same pass builds each site's incident-corner list and its natural-neighbour set
        from the three co-cell sites — the Delaunay star and the dual cell corner set together. Each row takes
        Degenerate when any incident circumcenter is None, Unbounded when the site sits in `open`, and otherwise
        SolidOf over its corner circumcenters, answering Volume, Centroid, and the corner-farthest Extent at once; a
        SolidOf answering None on an interior site downgrades that row to Degenerate rather than publishing a zero.
        Skeleton keeps only the VoronoiMesh.Edges whose Source and Target both measured, so a dropped edge always
        traces to an already-counted unmeasured vertex. */
        return default!;
    }

    // Closed-form circumsphere of a simplex: the squared-edge-weighted cross-product sum over twice the simplex
    // determinant. A coplanar tetrahedron falls out as a vanishing determinant and answers None on BOTH columns, so
    // no reader ever sees a fabricated centre or a zero radius that an alpha filter would silently admit.
    private static (Option<Point3d> Center, Option<double> Radius) Circumsphere(CloudCell cell) {
        if (cell.Vertices is not [CloudVertex c0, CloudVertex c1, CloudVertex c2, CloudVertex c3]) {
            return (None, None);
        }
        Point3d anchor = PointOf(c0);
        (Vector3d u, Vector3d v, Vector3d w) = (PointOf(c1) - anchor, PointOf(c2) - anchor, PointOf(c3) - anchor);
        double twice = 2.0 * (u * Vector3d.CrossProduct(a: v, b: w));
        if (Math.Abs(twice) <= EpsilonPolicy.ZeroTolerance) {
            return (None, None);
        }
        Vector3d offset = ((u.SquareLength * Vector3d.CrossProduct(a: v, b: w))
            + (v.SquareLength * Vector3d.CrossProduct(a: w, b: u))
            + (w.SquareLength * Vector3d.CrossProduct(a: u, b: v))) / twice;
        return offset.IsValid ? (Some(anchor + offset), Some(offset.Length)) : (None, None);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
