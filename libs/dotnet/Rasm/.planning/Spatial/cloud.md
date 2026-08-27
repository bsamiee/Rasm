# [RASM_CLOUD]

`VectorCloud` owns the point-cloud union under one admission that deduplicates by tolerance, renormalizes mass, and carries a copy-safe shared native index per cluster. `VectorCloudMetric` folds every cloud measurement behind one `Project<TOut>`, each row naming the `CloudKernel` fold that answers it, so a new cloud capability lands as a metric row, a hull-kind row, or a shape column.

`CloudKernel.CovarianceOf` is the corpus's one covariance fold, composing `Domain/stats.md` `SampleMoment` into a `matrix.md` `SymmetricMatrix` every PCA consumer reads. Deduplication posture is a `CloudDedup` row carrying its own point-equivalence body, and the mass-conservation floor reads `ToleranceLane.Conservation` from the cloud's own `Context`, so neither is a page literal nor a boolean a reader re-interprets.

## [01]-[INDEX]

- [02]-[VECTOR_CLOUD]: `VectorCloud` folds every cloud case under tolerance-dedup admission with the lazy cluster index and closest-vertex probe.
- [03]-[CLOUD_METRICS]: `VectorCloudMetric` projects every measurement through one `Project<TOut>` over the kernel folds.
- [04]-[HULL]: `CloudHullKind` folds native convex, faceted, planar, and Delaunay-filtered concave hulls into typed outcomes.
- [05]-[VORONOI_COMPLEX]: `CloudVoronoiCell` decomposes a cluster cloud into its 3D dual cells, skeleton, and bound census.

## [02]-[VECTOR_CLOUD]

- Owner: `VectorCloud` mints one case per cloud modality, mass an `Option` column on `ClusterCase`, so a weighted cluster is that case rather than a case of its own; `Vertices` and `Tolerance` are abstract ROOT columns every case answers, so a policy owner threading a `Context` and a consumer walking positions each read one member instead of switching for a fact all three cases carry.
- Cases: `CloudDedup` rows are the admission posture — `Merge` collapses coincident points under the policy tolerance, `Preserve` keeps every input position index-stable — and each row carries its own equivalence body, so re-admission selects a row rather than flipping a flag whose meaning the reader must re-derive.
- Auto: cluster admission is the ONE dedup-and-renormalize fold, emitting `OriginalToUnique` — the input-index→unique-index map every external per-point array re-indexes through to survive deduplication.
- Packages: RhinoCommon (native point cloud, polyline closure, self-intersection), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new cloud modality is one union case, one factory, and its metric-adapter arms; a new admission rule is one policy column; a new dedup posture is one `CloudDedup` row.
- Boundary: admission runs ONCE at the factory, so every kernel fold below consumes admitted vertices without re-validating and re-admission runs under `CloudDedup.Preserve` to keep vertices index-stable; native `PointCloud` and `PolylineCurve` reads are the platform boundary, held inside their lease windows under `key.Catch`; `Dispose` releases one shared cluster extent, so copies stay safe while a rehydrated cloud owns its own.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Threading;
using MIConvexHull;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudDedup {
    public static readonly CloudDedup Merge = new(key: 0,
        equivalent: static (left, right, tolerance) => tolerance.Match(
            Some: t => left.EpsilonEquals(other: right, epsilon: t.Value),
            None: () => left == right));
    public static readonly CloudDedup Preserve = new(key: 1, equivalent: static (_, _, _) => false);

    [UseDelegateFromConstructor]
    internal partial bool Equivalent(Point3d left, Point3d right, Option<PositiveMagnitude> tolerance);
}

[Union]
public abstract partial record VectorCloud : IDisposable {
    private VectorCloud() { }
    public sealed record RingCase : VectorCloud { internal RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) { this.Vertices = Vertices; this.Native = Native; this.Tolerance = Tolerance; } public override Seq<Point3d> Vertices { get; } public Polyline Native { get; } public override Context Tolerance { get; } }
    public sealed record PolylineCase : VectorCloud { internal PolylineCase(Seq<Point3d> Vertices, Context Tolerance) { this.Vertices = Vertices; this.Tolerance = Tolerance; } public override Seq<Point3d> Vertices { get; } public override Context Tolerance { get; } }
    public sealed record ClusterCase : VectorCloud {
        internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass, Lease<PointCloud> Indexed, CloudAdmission Admission) { this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass; Index = new IndexHandle(lease: Indexed); this.Admission = Admission; }
        private ClusterCase(ClusterCase original) : base(original) { Vertices = original.Vertices; Tolerance = original.Tolerance; Mass = original.Mass; Index = original.Index.Copy(); Admission = original.Admission; }
        public override Seq<Point3d> Vertices { get; }
        public override Context Tolerance { get; }
        public Option<Arr<double>> Mass { get; }
        private IndexHandle Index { get; }
        public CloudAdmission Admission { get; }

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

    public abstract Seq<Point3d> Vertices { get; }
    public abstract Context Tolerance { get; }

    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 3)
        let closure = admitted.Context.For(lane: ToleranceLane.Closure).Value
        let closed = admitted.Points.Count > 1 && admitted.Points[0].EpsilonEquals(other: admitted.Points[^1], epsilon: closure)
        let vertices = closed ? admitted.Points.Init : admitted.Points
        from _ in guard(vertices.Count >= 3, admitted.Key.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from ringClosed in guard(native.IsValid && native.IsClosedWithinTolerance(closure) && native.SegmentCount >= 3, admitted.Key.InvalidInput())
        from simple in Optional(native.ToPolylineCurve()).ToFin(admitted.Key.InvalidResult())
            .Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (admitted.Context, admitted.Key),
                project: static (s, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: s.Context.For(lane: ToleranceLane.Join).Value))
                    .ToFin(s.Key.InvalidResult())
                    .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(s.Key.InvalidInput()))))
        select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: admitted.Context);

    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) =>
        AdmitPoints(points: points, context: context, key: key, minimum: 2)
            .Map(static a => (VectorCloud)new PolylineCase(Vertices: a.Points, Tolerance: a.Context));

    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Option<CloudAdmissionPolicy> admission = default, Option<Arr<double>> mass = default, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 1)
        from policy in admission.Match(
            Some: candidate => candidate.Admit(key: admitted.Key),
            None: () => CloudAdmissionPolicy.Of(context: admitted.Context, key: admitted.Key))
        from fold in CloudKernel.AdmitCluster(points: admitted.Points, mass: mass, policy: policy, key: admitted.Key)
        from indexed in admitted.Key.Catch(() => {
            PointCloud native = [];
            native.AddRange(points: fold.Points.AsIterable());
            return Fin.Succ(native);
        })
        select (VectorCloud)new ClusterCase(Vertices: fold.Points, Tolerance: admitted.Context, Mass: fold.Mass, Indexed: new Lease<PointCloud>.Owned(Value: indexed), Admission: fold.Admission);

    internal Fin<VectorCloud> Admit(Op key) => Switch(
        state: key,
        ringCase: static (op, ring) => Ring(points: ring.Vertices, context: ring.Tolerance, key: op),
        polylineCase: static (op, poly) => Polyline(points: poly.Vertices, context: poly.Tolerance, key: op),
        clusterCase: static (op, cluster) =>
            from policy in CloudAdmissionPolicy.Of(context: cluster.Tolerance, key: op)
            from readmitted in Cluster(points: cluster.Vertices, context: cluster.Tolerance,
                admission: Some(policy with { Dedup = CloudDedup.Preserve }), mass: cluster.Mass, key: op)
            select readmitted);

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

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionPolicy(
    CloudDedup Dedup, Option<PositiveMagnitude> Tolerance, PositiveMagnitude ConservationTolerance) {
    internal static Fin<CloudAdmissionPolicy> Of(Context context, Op key, Option<PositiveMagnitude> tolerance = default) =>
        from conservation in key.AcceptValidated<PositiveMagnitude>(candidate: context.For(lane: ToleranceLane.Conservation).Value)
        select new CloudAdmissionPolicy(Dedup: CloudDedup.Merge, Tolerance: tolerance, ConservationTolerance: conservation);
    internal Fin<CloudAdmissionPolicy> Admit(Op key) {
        CloudAdmissionPolicy self = this;
        return guard(ValidityClaim.All(
                self.Tolerance.Map(static tolerance => ValidityClaim.Positive(tolerance.Value).Holds).IfNone(true),
                ValidityClaim.Positive(self.ConservationTolerance.Value)), key.InvalidInput())
            .ToFin().Map(_ => self);
    }
    internal bool Equivalent(Point3d left, Point3d right) => Dedup.Equivalent(left: left, right: right, tolerance: Tolerance);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmission(
    int InputCount, int OutputCount, int InputDuplicateCoordinateCount, int MergedCoordinateCount,
    Option<PositiveMagnitude> Tolerance, PositiveMagnitude ConservationTolerance, CloudDedup Dedup, Arr<int> OriginalToUnique,
    Option<double> MassInputTotal, Option<double> MassMergedTotal, Option<double> MassOutputTotal) : IValidityEvidence {
    internal static bool MassConserved(double input, double output, double tolerance) =>
        Math.Abs(input - output) <= tolerance * Math.Max(1.0, Math.Abs(input));
    internal static bool MassNormalized(double output, double tolerance) =>
        Math.Abs(1.0 - output) <= tolerance;
    internal static bool MassAdmitted(double total) => double.IsFinite(total) && total >= 0.0;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        InputDuplicateCoordinateCount >= 0 && MergedCoordinateCount >= 0,
        ValidityClaim.CountExactly(count: OutputCount + MergedCoordinateCount, expected: InputCount),
        MergedCoordinateCount == 0 || Dedup.Equals(CloudDedup.Merge),
        ValidityClaim.CountExactly(count: OriginalToUnique.Count, expected: InputCount),
        OriginalToUnique.ForAll(i => i >= 0 && i < OutputCount),
        (MassInputTotal.Case, MassMergedTotal.Case, MassOutputTotal.Case) switch {
            (double input, double merged, double output) =>
                MassAdmitted(total: input) && MassAdmitted(total: merged) && MassAdmitted(total: output)
                && MassConserved(input: input, output: merged, tolerance: ConservationTolerance.Value)
                && MassNormalized(output: output, tolerance: ConservationTolerance.Value),
            _ => MassInputTotal.IsNone && MassMergedTotal.IsNone && MassOutputTotal.IsNone,
        });
}
```

## [03]-[CLOUD_METRICS]

- Owner: `VectorCloudMetric` mints one row per measurement behind ONE `Project<TOut>`, each row a single declaration line naming its fold and its admissible cloud cases; `CloudMetricPolicy` wraps the `neighbors.md` `NeighborhoodPolicy` as the ONE policy record neighborhood-backed rows thread.
- Entry: the five row builders — `Ring`, `All`, `Chain`, `Poly`, `Cluster` — are the whole declaration surface: each fixes the case admission and adapts its fold to the erased measure column, so a row is one line and no arm re-states a case test.
- Auto: `PrincipalFrameOf` builds the frame from the two dominant eigenvectors, and ring orientation reads `ClosedCurveOrientation` against the fitted plane to sign the normal CCW-positive. Skewness is the worst normalized interior-angle deviation from the regular-polygon ideal, compactness `4πA/P²`, moment anisotropy the in-plane principal-moment ratio; chain rows are pure folds over unitized tangents, prefix-sum arc length, and turning-angle curvature. `PlanarWindingOf` takes the query point, so it is a kernel entry rather than a metric row, and the `intent.md` `WindingCase` composes it with the CCW-signed `RingNormalOf` normal — a sign-arbitrary best-fit-plane normal flips the winding integer. `Shape` answers one `VectorCloudShape` per cloud case, never a per-case sibling record.
- Packages: RhinoCommon (area mass properties, plane fitting, polyline geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measurement is ONE row through the matching builder; a new cloud case extends the builders' adapt arms; a policy knob is one column on `CloudMetricPolicy`.
- Boundary: neighborhood-backed rows delegate to `neighbors.md`, the fold living on that substrate while the metric row is its cloud-facing name and its census returns unchanged; `AreaMassProperties` and `PolylineCurve` natives stay inside their lease windows; `PlanarWinding` names the 2D ring fold, held distinct from the 3D solid-angle GWN family `reconstruct.md` owns.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
        Neighborhood = Cluster(key: 28, measure: static (c, p, k) => NeighborKernel.CensusOf(cluster: c, policy: p.Neighborhood, key: k)),
        CurvatureCensus = Cluster(key: 29, measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p.Neighborhood, key: k).Map(static r => r.Census));

    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, CloudMetricPolicy policy, Op key);

    private static VectorCloudMetric Ring<TValue>(int key, Func<VectorCloud.RingCase, Op, Fin<TValue>> measure);
    private static VectorCloudMetric All<TValue>(int key, Func<VectorCloud, Op, Fin<TValue>> measure);
    private static VectorCloudMetric Chain<TValue>(int key, Func<VectorCloud, Op, Fin<TValue>> measure);
    private static VectorCloudMetric Poly<TValue>(int key, Func<Seq<Point3d>, Op, Fin<TValue>> measure);
    private static VectorCloudMetric Cluster<TValue>(int key, Func<VectorCloud.ClusterCase, Op, Fin<TValue>> measure);
    private static VectorCloudMetric Cluster<TValue>(int key, Func<VectorCloud.ClusterCase, CloudMetricPolicy, Op, Fin<TValue>> measure);

    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        CloudMetricPolicy.AdmitOrDefault(policy: None, context: cloud.Tolerance, key: key)
            .Bind(policy => Project<TOut>(cloud: cloud, policy: policy, key: key));
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, CloudMetricPolicy policy, Op key) =>
        (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
            (false, _) => Fin.Fail<TOut>(error: key.Unsupported(inputType: cloud.GetType(), outputType: typeof(TOut))),
            (_, false) => Fin.Fail<TOut>(error: key.Unsupported(inputType: typeof(VectorCloudMetric), outputType: typeof(TOut))),
            _ => Measure(cloud: cloud, policy: policy, key: key).Bind(value => value switch {
                Seq<Vector3d> vs => ResultProjection.Values<Vector3d, TOut>(values: vs, key: key, owner: typeof(VectorCloudMetric)),
                Seq<double> ds => ResultProjection.Values<double, TOut>(values: ds, key: key, owner: typeof(VectorCloudMetric)),
                Seq<Plane> ps => ResultProjection.Values<Plane, TOut>(values: ps, key: key, owner: typeof(VectorCloudMetric)),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudMetricPolicy(NeighborhoodPolicy Neighborhood) {
    internal static Fin<CloudMetricPolicy> AdmitOrDefault(Option<CloudMetricPolicy> policy, Context context, Op key) =>
        policy.Match(Some: p => p.Neighborhood.Admit(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)),
                     None: () => NeighborhoodPolicy.Of(context: context, key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)));
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
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
        PrincipalFrame.IsValid,
        ValidityClaim.CountExactly(count: PrincipalAxes.Count, expected: 3),
        Area.Map(static a => ValidityClaim.Nonnegative(a).Holds).IfNone(true),
        Perimeter.Map(static p => ValidityClaim.Nonnegative(p).Holds).IfNone(true),
        Compactness.Map(static c => ValidityClaim.UnitInterval(c).Holds).IfNone(true),
        OpenLength.Map(static l => ValidityClaim.Nonnegative(l).Holds).IfNone(true),
        BestFitPlane.Map(static p => p.IsValid).IfNone(true));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class CloudKernel {
    internal static Fin<(Seq<Point3d> Points, Option<Arr<double>> Mass, CloudAdmission Admission)> AdmitCluster(
        Seq<Point3d> points, Option<Arr<double>> mass, CloudAdmissionPolicy policy, Op key);

    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Option<Arr<double>> mass, Op key) =>
        from moment in SampleMoment.Of(rows: points.Map(static p => Seq(p.X, p.Y, p.Z)), key: key,
            weights: mass.Map(static m => toSeq(m.AsIterable())))
        from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: moment.UpperCovariance, key: key)
        select (Mean: AsVector3d(v: moment.Mean), Cov: cov);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster, Op key) =>
        cluster.Mass.Match(
            Some: mass => MassOf(mass: mass, count: cluster.Vertices.Count, key: key)
                .Bind(normalized => CovarianceOf(points: cluster.Vertices, mass: Some(normalized), key: key)),
            None: () => CovarianceOf(points: cluster.Vertices, mass: Option<Arr<double>>.None, key: key));
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
        from eigen in stats.Cov.DecomposeEigenDetailed(key: key).Bind(solved => solved.PairsIn(expected: EigenOrder.DescendingMagnitude, key: key))
        from full in eigen.Count >= 3
            ? Fin.Succ(new PrincipalStats(Mean: stats.Mean, Eigen: eigen))
            : Fin.Fail<PrincipalStats>(key.InvalidResult())
        select full;
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);

    internal static Fin<T> WithMassProperties<T>(VectorCloud.RingCase ring, Func<Op, AreaMassProperties, Fin<T>> project, Op key);
    internal static Fin<Vector3d> RingNormalOf(VectorCloud.RingCase ring, Op key);
    internal static Fin<double> EdgeAspectOf(Polyline native, Context context, Op key);
    internal static Fin<double> RingSkewnessOf(VectorCloud.RingCase ring, Op key);
    internal static Fin<double> RingCompactnessOf(VectorCloud.RingCase ring, Op key);
    internal static Fin<double> RingMomentAnisotropyOf(VectorCloud.RingCase ring, Op key);

    internal static Fin<Point3d> CentroidOf(VectorCloud cloud, Op key);
    internal static Fin<Plane> BestFitPlaneOf(VectorCloud cloud, Op key);
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalAxesOf(VectorCloud cloud, Op key);
    internal static Fin<Plane> PrincipalFrameOf(VectorCloud cloud, Op key);
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud, Op key);

    internal static Fin<Seq<Vector3d>> TangentFlowOf(Seq<Point3d> points, Op key);
    internal static Fin<Seq<double>> CumulativeArcLengthOf(Seq<Point3d> points, Op key);
    internal static Fin<Seq<double>> EdgeCurvaturesOf(Seq<Point3d> points, Op key);
    internal static Fin<double> OpenLengthOf(Seq<Point3d> points, Op key);

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

- Owner: `CloudHullKind` names the hull species, `FootprintWrapper` the 2D fallback a rejected 3D hull degrades to; concave columns `Alpha` and `Lambda` derive from the cluster's mean spacing when the caller supplies neither. `CloudFoldStatus` is the returned-status outcome this hull owner publishes, and `CloudHullRejection` keys its typed refusal off the MIConvexHull ordinals.
- Cases: `HullRoute` rows are the route evidence the outcome carries as a `CapabilitySet` — `Coplanar` says the 3D preflight refused the input, `Native` that a host route answered, `Fallback` that the 2D wrapper stood in. They combine under a legal-corner law (a coplanar rejection always rides beside a fallback), which is why they are one set rather than three independent flags open to inconsistent pairing.
- Entry: `ComputeHullDetailed` is cluster-only, and every declared kind computes, so `CloudFoldStatus` discriminates outcome alone.
- Auto: `Convex3D` routes native through `Mesh.CreateConvexHull3D` behind a coplanar preflight, duplicating the mesh out of its `using` window; `ConvexFootprint2D` and `FootprintWrapper` fit the PCA plane, run `PolylineCurve.CreateConvexHull2d`, verify containment within tolerance, and mesh via `Mesh.CreateFromClosedPolyline`. `AlphaShape` keeps every triangle whose circumradius stays within `Alpha`; `ConcaveOutline` erodes the longest boundary edge while it exceeds `Lambda` and removal preserves regularity, abandoning no vertex and leaving the boundary a single simple cycle. `Faceted3D` and `IndexedFootprint2D` are the index-preserving twins of the two host routes — the host `Mesh` and `PolylineCurve` answer geometry and drop which cluster vertex each output came from, so the typed rows keep facet adjacency, per-facet outward normals, and the cluster index every downstream census and dual keys on.
- Packages: RhinoCommon (native convex hull, plane fitting, polyline meshing), MIConvexHull (`Triangulation.CreateDelaunay<CloudVertex, CloudCell>`, `ConvexHull.Create<CloudVertex, CloudFace>` with `ConvexHull.Faces`/`ConvexFace.Adjacency`/`.Normal`, `ConvexHull.Create2D<CloudPlanarVertex>`, `ConvexHullCreationResult.Outcome`/`.Result` — the index-carrying `IVertex`/`IVertex2D` and circumsphere-carrying `TriangulationCell` generics, tolerance threaded from the admitted policy), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Law: `CloudHullPolicy` is the AUTHORITY for every threshold the fold ran under and `CloudHullResult.Tolerance`/`AngleTolerance` are its published copy, carried so a consumer reading a stored result need not hold the policy that produced it; `Rejection` and `Status` mirror each other by the same rule, and the claim fold states that pairing rather than leaving two readers to agree by convention.
- Growth: a new hull species is one kind row and one arm in the hull fold, or one filter predicate over the shared Delaunay fold; a new concave criterion is one policy column; a new rejection cause is one `CloudHullRejection` row keyed off its package ordinal; a new route fact is one `HullRoute` row.
- Boundary: both concave kinds share ONE Delaunay fold over `MIConvexHull`'s complex, the filter predicate their only difference; `Triangulation.CreateDelaunay` is the foreign-exception boundary on this owner and `key.Catch` preserves its exact exceptional `Error`. `ConvexHull.*` instead returns a typed outcome, so `Faceted3D` and `IndexedFootprint2D` gate `Outcome` ahead of `Result` and publish `CloudHullRejection` without a capture. This family owns the native-first host, index-preserving, and concave hull kinds; the predicate-exact hull fold homes at `Meshing/delaunay` `LowerHull`, and `SolidOf` is the one volume-and-centroid producer both this family and `[05]` read.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<int>]
public sealed partial class CloudFoldStatus {
    public static readonly CloudFoldStatus Completed = new(key: 0);
    public static readonly CloudFoldStatus Rejected = new(key: 1);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HullRoute : ICapability<HullRoute> {
    public static readonly HullRoute Coplanar = new(key: "coplanar", rank: 0);
    public static readonly HullRoute Native = new(key: "native", rank: 1);
    public static readonly HullRoute Fallback = new(key: "fallback", rank: 2);
    public int Rank { get; }
    internal static readonly CapabilityLaw<HullRoute> Law = new(Legal: Seq(
        CapabilitySet<HullRoute>.None,
        CapabilitySet<HullRoute>.Of(Native),
        CapabilitySet<HullRoute>.Of(Native, Fallback),
        CapabilitySet<HullRoute>.Of(Coplanar, Native, Fallback)));
}

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
        : key.Row<int, CloudHullRejection>((int)outcome).Map(Some).MapFail(_ => key.InvalidResult());
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullPolicy(
    PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
    Option<PositiveMagnitude> Alpha, Option<PositiveMagnitude> Lambda) {
    internal static Fin<CloudHullPolicy> AdmitOrDefault(Option<CloudHullPolicy> policy, Context context, Op key) {
        (double tolerance, double angle, Option<PositiveMagnitude> alpha, Option<PositiveMagnitude> lambda) = policy.Match(
            Some: static candidate => (candidate.Tolerance.Value, candidate.AngleTolerance.Value, candidate.Alpha, candidate.Lambda),
            None: () => (context.For(lane: ToleranceLane.Deviation).Value, context.Angle.Value, Option<PositiveMagnitude>.None, Option<PositiveMagnitude>.None));
        return from admittedTolerance in key.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from admittedAngle in key.AcceptValidated<VectorAngle>(candidate: angle)
               from admittedAlpha in AdmitMagnitude(value: alpha, key: key)
               from admittedLambda in AdmitMagnitude(value: lambda, key: key)
               select new CloudHullPolicy(Tolerance: admittedTolerance, AngleTolerance: admittedAngle, Alpha: admittedAlpha, Lambda: admittedLambda);
    }
    private static Fin<Option<PositiveMagnitude>> AdmitMagnitude(Option<PositiveMagnitude> value, Op key) =>
        value.TraverseM(magnitude => key.AcceptValidated<PositiveMagnitude>(candidate: magnitude.Value)).As();
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudFacet(Arr<int> Vertices, Arr<int> Adjacency, Vector3d Normal) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Vertices.Count, floor: 3),
        Vertices.ForAll(static v => v >= 0),
        ValidityClaim.CountExactly(count: Adjacency.Count, expected: Vertices.Count),
        Adjacency.ForAll(static a => a >= 0),
        ValidityClaim.Finite(Normal));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudSolid(double Volume, Point3d Centroid, Seq<CloudFacet> Facets) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(Volume),
        ValidityClaim.Finite(Centroid),
        ValidityClaim.CountAtLeast(count: Facets.Count, floor: 4),
        Facets.ForAll(static f => f.IsValid));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(
    Option<Mesh> Mesh, Option<CloudSolid> Solid,
    CloudHullKind Kind, CloudFoldStatus Status, PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
    int InputCount, int OutputVertexCount, Option<int> FacetCount, Option<int> SurvivingTriangleCount,
    Option<int> ContainmentRejectedCount, Option<double> PlanarityDeviation, Option<PositiveMagnitude> EffectiveAlpha,
    Option<PositiveMagnitude> EffectiveLambda, Option<CloudHullRejection> Rejection,
    CapabilitySet<HullRoute> Route) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Solid.Map(static solid => solid.IsValid).IfNone(true),
        ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
        OutputVertexCount >= 0,
        FacetCount.Map(static c => c >= 0).IfNone(true),
        SurvivingTriangleCount.Map(static c => c >= 0).IfNone(true),
        ContainmentRejectedCount.Map(static c => c >= 0).IfNone(true),
        PlanarityDeviation.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(true),
        Rejection.IsNone || Status.Equals(CloudFoldStatus.Rejected),
        HullRoute.Law.Admit(held: Route).IsSucc,
        !Status.Equals(CloudFoldStatus.Completed) || Route.Admits(capability: HullRoute.Native),
        !Status.Equals(CloudFoldStatus.Completed) || OutputVertexCount >= 3,
        Mesh.IsSome || Solid.IsSome || Status.Equals(CloudFoldStatus.Rejected));

    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudHullResult self = this;
        return ResultProjection.Rows<CloudHullResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudSolid>(() => self.Solid.ToFin(key.Unsupported(inputType: typeof(CloudHullResult), outputType: typeof(CloudSolid)))),
            ProjectionRow.Of<Seq<CloudFacet>>(() => self.Solid.Map(static solid => solid.Facets)
                .ToFin(key.Unsupported(inputType: typeof(CloudHullResult), outputType: typeof(Seq<CloudFacet>)))),
            ProjectionRow.Of<Mesh>(() => self.Mesh.ToFin(key.Unsupported(inputType: typeof(CloudHullResult), outputType: typeof(Mesh)))
                .Bind(mesh => key.AcceptValue(value: mesh))),
            ProjectionRow.Of<VectorCloud>(() => self.Mesh.ToFin(key.Unsupported(inputType: typeof(CloudHullResult), outputType: typeof(VectorCloud)))
                .Bind(mesh => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context, key: key))));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal sealed class CloudVertex(int index, double[] position) : IVertex {
    public int Index { get; } = index;
    public double[] Position { get; } = position;
}

internal sealed class CloudFace : ConvexFace<CloudVertex, CloudFace>;

internal sealed class CloudPlanarVertex : IVertex2D {
    internal int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
}

internal sealed class CloudCell : TriangulationCell<CloudVertex, CloudCell> {
    internal bool Boundary => Array.Exists(array: Adjacency, match: static face => face is null);
}

internal readonly record struct CellSpheres(FrozenDictionary<CloudCell, (Point3d Center, double Radius)> Measured) {
    internal Option<Line> Dual(CloudVoronoiEdge edge) =>
        from tail in Measured.TryGetValue(key: edge.Source, value: out (Point3d Center, double Radius) from) ? Some(from.Center) : Option<Point3d>.None
        from head in Measured.TryGetValue(key: edge.Target, value: out (Point3d Center, double Radius) to) ? Some(to.Center) : Option<Point3d>.None
        select new Line(from: tail, to: head);
}

internal static partial class CloudKernel {
    internal static Fin<CloudHullResult> ComputeHullDetailed(
        VectorCloud.ClusterCase cluster, CloudHullKind kind, CloudHullPolicy policy, Op key);

    internal static Fin<Option<CloudSolid>> SolidOf(Point3d[] points, double tolerance, Op key) {
        ConvexHullCreationResult<CloudVertex, CloudFace> hull = ConvexHull.Create<CloudVertex, CloudFace>(
            data: [.. points.Select(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z]))],
            tolerance: tolerance);
        return CloudHullRejection.Of(outcome: hull.Outcome, key: key).Bind(rejection => rejection.IsSome
            ? Fin.Succ(Option<CloudSolid>.None)
            : Accumulate(anchor: points.Aggregate(Point3d.Origin, static (sum, p) => sum + p) / points.Length,
                faces: toSeq(hull.Result.Faces), tolerance: tolerance, key: key));
    }

    private static Fin<Option<CloudSolid>> Accumulate(Point3d anchor, Seq<CloudFace> faces, double tolerance, Op key) {
        (double volume, Vector3d moment) = faces.Fold((Volume: 0.0, Moment: Vector3d.Zero), (acc, face) => {
            (Vector3d u, Vector3d v, Vector3d w) = (PointOf(face.Vertices[0]) - anchor, PointOf(face.Vertices[1]) - anchor, PointOf(face.Vertices[2]) - anchor);
            double tet = Math.Abs(Vector3d.CrossProduct(a: v - u, b: w - u) * u) / 6.0;
            return (acc.Volume + tet, acc.Moment + (tet * 0.25 * (u + v + w)));
        });
        if (volume <= tolerance * tolerance * tolerance) { return Fin.Succ(Option<CloudSolid>.None); }
        FrozenDictionary<CloudFace, int> ordinal = faces.Map(static (face, index) => (Face: face, Index: index))
            .ToFrozenDictionary(static row => row.Face, static row => row.Index);
        return faces.Traverse(face => toSeq(face.Adjacency)
                .Traverse(neighbor => Optional(neighbor).Map(present => ordinal[present]))
                .Map(slots => new CloudFacet(
                    Vertices: new Arr<int>([.. face.Vertices.Select(static corner => corner.Index)]),
                    Adjacency: new Arr<int>([.. slots]),
                    Normal: new Vector3d(x: face.Normal[0], y: face.Normal[1], z: face.Normal[2]))))
            .ToFin(key.InvalidResult())
            .Bind(facets => new CloudSolid(Volume: volume, Centroid: anchor + (moment / volume), Facets: facets) switch {
                CloudSolid solid when solid.IsValid => Fin.Succ(Some(solid)),
                _ => Fin.Fail<Option<CloudSolid>>(key.InvalidResult()),
            });
    }

    internal static Point3d PointOf(CloudVertex vertex) => new(x: vertex.Position[0], y: vertex.Position[1], z: vertex.Position[2]);
}
```

## [05]-[VORONOI_COMPLEX]

- Owner: `CloudVoronoiCell` is the 3D dual cell over a cluster cloud — one row per site — and `CloudCellBound` is the vocabulary deciding which measures that row carries, so an unbounded or degenerate cell publishes `None` where a bounded one publishes volume, centroid, and extent.
- Entry: `ComputeVoronoiDetailed` is cluster-only and consumes the already-admitted `CloudHullPolicy`, reading `Tolerance` alone as the `PlaneDistanceTolerance` the dual threads; `Alpha`, `Lambda`, and `AngleTolerance` are concave-hull columns this fold never reads. `NaturalNeighborField` is the Sibson stolen-volume owner — the BASE dual mints once at `Of` and each `Weights` query pays only the inserted-site dual and the volume-loss fold against it, so an interpolant over M queries no longer rebuilds the unchanging half M times — the one weight source the `Meshing/reconstruct` evaluator composes.
- Auto: each Delaunay cell IS a Voronoi vertex, so ONE circumsphere sweep over `VoronoiMesh.Vertices` mints `CellSpheres` — a map over the cells that HAVE a sphere, where a missing key IS degeneracy — and `VoronoiEdge.Source`/`.Target` read the cell pair whose circumcenters bound one dual edge, so the 1-skeleton falls out as `Skeleton` with no second traversal and no reader can reach a dual before the sweep that measures it. Bound classification derives structurally, never by proximity heuristic: `SolidOf` over the sites answers the site hull in the same pass that answers `HullVolume`, and a site on that hull owns an open cell because the Voronoi region of a hull vertex extends to infinity. Bounded cells take the convex hull of their incident circumcenters as geometry, so `SolidOf` answers volume and centroid too and the `[04]` faceted row is this band's measurement kernel.
- Law: `CloudVoronoiCensus` carries the tolerance and input count beside the completed census; `BoundedVolumeTotal` never exceeds `HullVolume`, and that ordering is the census's own conservation claim.
- Packages: MIConvexHull (`VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>`, `VoronoiMesh.Vertices`/`.Edges`, `VoronoiEdge.Source`/`.Target`, `ConvexHull.Create<CloudVertex, CloudFace>` through `SolidOf`), RhinoCommon, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new per-cell measure is one `Option` column on `CloudVoronoiCell` with its arm in the bounded fold; a new bound species is one `CloudCellBound` row; a new census tally is one column on `CloudVoronoiCensus`; a new interpolant over one site set is one member on `NaturalNeighborField`, sharing the base dual it already holds.
- Boundary: this band owns the 3D cell decomposition alone — 2D border-clipped point-site Voronoi homes at `Meshing/delaunay` `Tessellation.VoronoiDual`, whose bounded-cell overload is the predicate-exact planar peer, and `Meshing/offset` reads that owner for the medial locus. `VoronoiMesh.Create` returns the bare complex and throws on degenerate input, so `Op.Catch` keeps that exact exceptional `Error` on the failure result; the `ConvexHull.*` APIs instead return a typed outcome and alone publish `CloudHullRejection`. Natural-neighbour interpolation reads `Volume` from here and fits nothing; the admitting minter is `Meshing/reconstruct`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudCellBound {
    public static readonly CloudCellBound Bounded = new(key: 0);
    public static readonly CloudCellBound Unbounded = new(key: 1);
    public static readonly CloudCellBound Degenerate = new(key: 2);
}

internal sealed class CloudVoronoiEdge : VoronoiEdge<CloudVertex, CloudCell>;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiCell(
    int Site, Point3d Seed, CloudCellBound Bound, Arr<int> Vertices, Arr<int> Neighbors,
    Option<double> Volume, Option<Point3d> Centroid, Option<double> Extent) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Site, floor: 0),
        ValidityClaim.Finite(Seed),
        ValidityClaim.CountAtLeast(count: Vertices.Count, floor: 1),
        Vertices.ForAll(static v => v >= 0),
        Neighbors.ForAll(neighbor => neighbor >= 0 && neighbor != Site),
        Bound.Equals(CloudCellBound.Bounded)
            ? Volume.Exists(static v => ValidityClaim.Positive(v).Holds)
              && Centroid.Exists(static c => ValidityClaim.Finite(c).Holds)
              && Extent.Exists(static e => ValidityClaim.Positive(e).Holds)
            : Volume.IsNone && Centroid.IsNone && Extent.IsNone);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiCensus(
    PositiveMagnitude PlaneDistanceTolerance, int InputCount, int DualVertexCount, int UnmeasuredVertexCount, int DualEdgeCount, int SkeletonEdgeCount,
    int BoundedCellCount, int UnboundedCellCount, int DegenerateCellCount,
    Option<double> BoundedVolumeTotal, Option<double> HullVolume) : IValidityEvidence {
    internal int MeasuredVertexCount => DualVertexCount - UnmeasuredVertexCount;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: InputCount, floor: 4),
        ValidityClaim.CountAtLeast(count: DualVertexCount, floor: 1),
        UnmeasuredVertexCount >= 0 && UnmeasuredVertexCount <= DualVertexCount,
        DualEdgeCount >= 0 && SkeletonEdgeCount >= 0 && SkeletonEdgeCount <= DualEdgeCount,
        BoundedCellCount >= 0 && UnboundedCellCount >= 0 && DegenerateCellCount >= 0,
        BoundedVolumeTotal.IsSome == (BoundedCellCount > 0),
        (BoundedVolumeTotal.Case, HullVolume.Case) switch {
            (double bounded, double hull) => ValidityClaim.Positive(hull).Holds && ValidityClaim.Ordered(lower: bounded, upper: hull).Holds,
            _ => BoundedVolumeTotal.IsNone,
        });
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiResult(
    Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton, CloudVoronoiCensus Census) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Cells.ForAll(cell => cell.IsValid
            && cell.Vertices.ForAll(vertex => vertex < Vertices.Count)
            && cell.Neighbors.ForAll(neighbor => neighbor < Cells.Count)),
        Skeleton.ForAll(edge => edge.Tail < Vertices.Count && edge.Head < Vertices.Count),
        ValidityClaim.Evidence(Some(Census)));

    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudVoronoiResult self = this;
        return ResultProjection.Rows<CloudVoronoiResult, TOut>(self: self, key: key,
            ProjectionRow.Of<CloudVoronoiCensus>(() => Fin.Succ(self.Census)),
            ProjectionRow.Of<Seq<CloudVoronoiCell>>(() => Fin.Succ(self.Cells)),
            ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(toSeq(self.Skeleton.AsIterable()
                .Select(edge => new Line(from: self.Vertices[edge.Tail], to: self.Vertices[edge.Head]))))),
            ProjectionRow.Of<VectorCloud>(() => VectorCloud.Cluster(
                points: toSeq(self.Vertices.AsIterable()), context: context, key: key)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class CloudKernel {
    internal static Fin<CloudVoronoiResult> ComputeVoronoiDetailed(VectorCloud.ClusterCase cluster, CloudHullPolicy policy, Op key) =>
        from _ in guard(cluster.Vertices.Count >= 4, key.InvalidInput()).ToFin()
        let sites = cluster.Vertices.Map(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z])).ToArray()
        from result in key.Catch(() => CensusOf(
                sites: sites, tolerance: policy.Tolerance.Value, key: key,
                complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(
                    data: sites, PlaneDistanceTolerance: policy.Tolerance.Value)))
        select result;

    internal static Option<(int Site, double Loss)> VolumeLoss(CloudVoronoiResult first, CloudVoronoiResult second, int site) =>
        from was in first.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
        from now in second.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
        select (Site: site, Loss: was - now);

    internal static Fin<CloudVoronoiResult> CensusOf(
        CloudVertex[] sites, VoronoiMesh<CloudVertex, CloudCell, CloudVoronoiEdge> complex, double tolerance, Op key) {
        CloudCell[] cells = [.. complex.Vertices];
        CellSpheres spheres = new(Measured: cells
            .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: tolerance)))
            .Where(static row => row.Sphere.IsSome)
            .ToFrozenDictionary(static row => row.Cell, static row => row.Sphere.IfNone(default((Point3d, double)))));
        return from admitted in key.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from hull in SolidOf(points: [.. sites.Select(PointOf)], tolerance: tolerance, key: key)
               let open = hull.Map(static solid => solid.Facets.Fold(Set<int>.Empty,
                   static (acc, facet) => facet.Vertices.Fold(acc, static (set, corner) => set.Add(corner)))).IfNone(Set<int>.Empty)
               from rows in CellRows(cells: cells, complex: complex, spheres: spheres, open: open, sites: sites, tolerance: tolerance, key: key)
               let census = new CloudVoronoiCensus(
                   PlaneDistanceTolerance: admitted, InputCount: sites.Length, DualVertexCount: cells.Length, UnmeasuredVertexCount: cells.Length - rows.Vertices.Count,
                   DualEdgeCount: complex.Edges.Count(), SkeletonEdgeCount: rows.Skeleton.Count,
                   BoundedCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Bounded)),
                   UnboundedCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Unbounded)),
                   DegenerateCellCount: rows.Cells.Count(static c => c.Bound.Equals(CloudCellBound.Degenerate)),
                   BoundedVolumeTotal: rows.Cells.Bind(static c => c.Volume.ToSeq()) is { IsEmpty: false } measured
                       ? Some(measured.Fold(0.0, static (acc, volume) => acc + volume))
                       : Option<double>.None,
                   HullVolume: hull.Map(static solid => solid.Volume))
               from verified in census.IsValid ? Fin.Succ(census) : Fin.Fail<CloudVoronoiCensus>(key.InvalidResult())
               from verifiedResult in new CloudVoronoiResult(Cells: rows.Cells, Vertices: rows.Vertices, Skeleton: rows.Skeleton,
                       Census: verified) switch {
                   CloudVoronoiResult whole when whole.IsValid => Fin.Succ(whole),
                   _ => Fin.Fail<CloudVoronoiResult>(key.InvalidResult()),
               }
               select verifiedResult;
    }

    private static Fin<(Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton)> CellRows(
        CloudCell[] cells, VoronoiMesh<CloudVertex, CloudCell, CloudVoronoiEdge> complex, CellSpheres spheres, Set<int> open,
        CloudVertex[] sites, double tolerance, Op key);

    private static Option<(Point3d Center, double Radius)> Circumsphere(CloudCell cell, double tolerance) {
        if (cell.Vertices is not [CloudVertex c0, CloudVertex c1, CloudVertex c2, CloudVertex c3]) {
            return Option<(Point3d, double)>.None;
        }
        Point3d anchor = PointOf(c0);
        (Vector3d u, Vector3d v, Vector3d w) = (PointOf(c1) - anchor, PointOf(c2) - anchor, PointOf(c3) - anchor);
        double twice = 2.0 * (u * Vector3d.CrossProduct(a: v, b: w));
        if (Math.Abs(twice) <= tolerance * u.Length * v.Length * w.Length) {
            return Option<(Point3d, double)>.None;
        }
        Vector3d offset = ((u.SquareLength * Vector3d.CrossProduct(a: v, b: w))
            + (v.SquareLength * Vector3d.CrossProduct(a: w, b: u))
            + (w.SquareLength * Vector3d.CrossProduct(a: u, b: v))) / twice;
        return Some((Center: anchor + offset, Radius: offset.Length));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NaturalNeighborField(CloudVoronoiResult Base, Arr<Point3d> Sites, double Tolerance) {
    internal static Fin<NaturalNeighborField> Of(Seq<Point3d> sites, double tolerance, Op key) {
        CloudVertex[] seeds = sites.Map(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z])).ToArray();
        return from _ in guard(sites.Count >= 4, key.InvalidInput()).ToFin()
               from dual in key.Catch(() => CloudKernel.CensusOf(sites: seeds, tolerance: tolerance, key: key,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(data: seeds, PlaneDistanceTolerance: tolerance)))
               select new NaturalNeighborField(Base: dual, Sites: new Arr<Point3d>([.. sites]), Tolerance: tolerance);
    }

    internal Fin<Seq<(int Site, double Weight)>> Weights(Point3d query, Op key) {
        NaturalNeighborField self = this;
        CloudVertex[] after = [
            .. self.Sites.AsIterable().Select(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z])),
            new CloudVertex(index: self.Sites.Count, position: [query.X, query.Y, query.Z])];
        return from inserted in key.Catch(() => CloudKernel.CensusOf(sites: after, tolerance: self.Tolerance, key: key,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(data: after, PlaneDistanceTolerance: self.Tolerance)))
               from cell in inserted.Cells.Find(row => row.Site == self.Sites.Count).ToFin(key.InvalidInput())
               from _support in guard(cell.Bound.Equals(CloudCellBound.Bounded), key.InvalidInput()).ToFin()
               from losses in toSeq(cell.Neighbors.AsIterable().Filter(site => site != self.Sites.Count))
                   .TraverseM(site => CloudKernel.VolumeLoss(first: self.Base, second: inserted, site: site).ToFin(key.InvalidInput())).As()
               let positive = losses.Filter(static row => row.Loss > 0.0)
               let total = positive.Sum(static row => row.Loss)
               from _unity in guard(positive.Count >= 1 && double.IsFinite(total) && total > 0.0, key.InvalidResult()).ToFin()
               select positive.Map(row => (row.Site, Weight: row.Loss / total));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
