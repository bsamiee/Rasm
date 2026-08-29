# [RASM_CLOUD]

`VectorCloud` owns the point-cloud union under one admission that deduplicates by tolerance, renormalizes mass, and owns one native index per cluster by reference. `VectorCloudMetric` folds every cloud measurement behind one `Project<TOut>`, each row naming the `CloudKernel` fold that answers it, so a new cloud capability lands as a metric row, a hull-kind row, or a shape column.

`CloudKernel.CovarianceOf` is the corpus's one covariance fold, composing `Domain/stats.md` `SampleMoment` into a `matrix.md` `SymmetricMatrix` every PCA consumer reads. Deduplication posture is a `CloudDedup` row carrying its own point-equivalence body, and the mass-conservation floor reads `ToleranceLane.Conservation` from the cloud's own `Context`, so neither is a page literal nor a boolean a reader re-interprets.

## [01]-[INDEX]

- [02]-[VECTOR_CLOUD]: `VectorCloud` folds every cloud case under tolerance-dedup admission; `Cluster` builds the native index at construction, `Release()`/`Dispose` free it once, and `ClosestVertex` probes it.
- [03]-[CLOUD_METRICS]: `VectorCloudMetric` projects every measurement through one `Project<TOut>` over the kernel folds.
- [04]-[HULL]: `CloudHullKind` folds native convex, faceted, planar, and Delaunay-filtered concave hulls into typed outcomes.
- [05]-[VORONOI_COMPLEX]: `CloudVoronoiCell` decomposes a cluster cloud into its 3D dual cells, skeleton, and bound census.

## [02]-[VECTOR_CLOUD]

- Owner: `VectorCloud` mints one case per cloud modality, mass an `Option` column on `ClusterCase`, so a weighted cluster is that case rather than a case of its own; `Vertices` and `Tolerance` are abstract ROOT columns every case answers, so a policy owner threading a `Context` and a consumer walking positions each read one member instead of switching for a fact all three cases carry.
- Cases: `CloudDedup` rows are the admission posture — `Merge` collapses coincident points under the policy tolerance, `Preserve` keeps every input position index-stable — and each row carries its own equivalence body, so re-admission selects a row rather than flipping a flag whose meaning the reader must re-derive.
- Auto: cluster admission is the ONE dedup-and-renormalize fold, emitting `OriginalToUnique` — the input-index→unique-index map every external per-point array re-indexes through to survive deduplication.
- Packages: RhinoCommon (native point cloud, polyline closure, self-intersection), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new cloud modality is one union case, one factory, and its metric-adapter arms; a new admission rule is one policy column; a new dedup posture is one `CloudDedup` row.
- Boundary: admission runs ONCE at the factory, so every kernel fold below consumes admitted vertices without re-validating and re-admission runs under `CloudDedup.Preserve` to keep vertices index-stable; native `PointCloud` and `PolylineCurve` reads are the platform boundary, held inside their lease windows under `key.Catch`; `Dispose` releases the cluster's native index once under its lock, and every later `UseIndex` refuses with `InvalidContext`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Threading;
using MIConvexHull;
using Rasm.Domain;
using Rasm.Numerics;
using CloudCell = MIConvexHull.DefaultTriangulationCell<Rasm.Spatial.CloudVertex>;
using CloudEdge = MIConvexHull.VoronoiEdge<Rasm.Spatial.CloudVertex, MIConvexHull.DefaultTriangulationCell<Rasm.Spatial.CloudVertex>>;
using CloudFace = MIConvexHull.DefaultConvexFace<Rasm.Spatial.CloudVertex>;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CloudDedup {
    public static readonly CloudDedup Merge = new(
        equivalent: static (left, right, tolerance) => tolerance.Match(
            Some: t => left.EpsilonEquals(other: right, epsilon: t.Value),
            None: () => left == right));
    public static readonly CloudDedup Preserve = new(equivalent: static (_, _, _) => false);

    [UseDelegateFromConstructor]
    internal partial bool Equivalent(Point3d left, Point3d right, Option<PositiveMagnitude> tolerance);
}

[Union]
public abstract partial class VectorCloud : IDisposable {
    private VectorCloud() { }
    public sealed class RingCase : VectorCloud { internal RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) { this.Vertices = Vertices; this.Native = Native; this.Tolerance = Tolerance; } public override Seq<Point3d> Vertices { get; } public Polyline Native { get; } public override Context Tolerance { get; } }
    public sealed class PolylineCase : VectorCloud { internal PolylineCase(Seq<Point3d> Vertices, Context Tolerance) { this.Vertices = Vertices; this.Tolerance = Tolerance; } public override Seq<Point3d> Vertices { get; } public override Context Tolerance { get; } }
    public sealed class ClusterCase : VectorCloud {
        private readonly Lock gate = new();
        private readonly Lease<PointCloud> index;
        private bool disposed;
        internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass, Lease<PointCloud> Indexed, CloudAdmission Admission) { this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass; index = Indexed; this.Admission = Admission; }
        public override Seq<Point3d> Vertices { get; }
        public override Context Tolerance { get; }
        public Option<Arr<double>> Mass { get; }
        public CloudAdmission Admission { get; }

        internal Fin<T> UseIndex<T>(Func<PointCloud, Fin<T>> project) {
            lock (gate) return disposed ? Fin.Fail<T>(new KernelFault.InvalidContext()) : project(index.Resource);
        }

        internal Fin<ClosestHit> ClosestVertex(Point3d sample) =>
            UseIndex(project: indexed => Try.lift(() => indexed.ClosestPoint(testPoint: sample) switch {
                    int idx when idx >= 0 && idx < Vertices.Count => Acceptance.Value(value: ClosestHit.At(
                        target: sample, point: indexed.PointAt(index: idx),
                        component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: idx)))),
                    _ => Fin.Fail<ClosestHit>(error: new KernelFault.InvalidResult()),
                }).Run().Bind(static inner => inner));

        internal Unit Release() {
            lock (gate) {
                if (!disposed) { disposed = true; _ = index.Dispose(); }
            }
            return unit;
        }
    }

    public abstract Seq<Point3d> Vertices { get; }
    public abstract Context Tolerance { get; }

    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context) =>
        from admitted in AdmitPoints(points: points, context: context, minimum: 3)
        let closure = admitted.Context.For(lane: ToleranceLane.Closure).Value
        let closed = admitted.Points.Count > 1 && admitted.Points[0].EpsilonEquals(other: admitted.Points[^1], epsilon: closure)
        let vertices = closed ? admitted.Points.Init : admitted.Points
        from _ in guard(vertices.Count >= 3, new KernelFault.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from ringClosed in guard(native.IsValid && native.IsClosedWithinTolerance(closure) && native.SegmentCount >= 3, new KernelFault.InvalidInput())
        from simple in Optional(native.ToPolylineCurve()).ToFin(new KernelFault.InvalidResult())
            .Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (admitted.Context, admitted.Key),
                project: static (s, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: s.Context.For(lane: ToleranceLane.Join).Value))
                    .ToFin(new KernelFault.InvalidResult())
                    .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(new KernelFault.InvalidInput()))))
        select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: admitted.Context);

    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context) =>
        AdmitPoints(points: points, context: context, minimum: 2)
            .Map(static a => (VectorCloud)new PolylineCase(Vertices: a.Points, Tolerance: a.Context));

    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Option<CloudAdmissionPolicy> admission = default, Option<Arr<double>> mass = default) =>
        from admitted in AdmitPoints(points: points, context: context, minimum: 1)
        from policy in admission.Match(
            Some: candidate => candidate.Admit(),
            None: () => CloudAdmissionPolicy.Of(context: admitted.Context))
        from fold in CloudKernel.AdmitCluster(points: admitted.Points, mass: mass, policy: policy)
        from indexed in Try.lift(() => {
            PointCloud native = [];
            native.AddRange(points: fold.Points.AsIterable());
            return Fin.Succ(native);
        }).Run().Bind(static inner => inner)
        select (VectorCloud)new ClusterCase(Vertices: fold.Points, Tolerance: admitted.Context, Mass: fold.Mass, Indexed: new Lease<PointCloud>.Owned(Value: indexed), Admission: fold.Admission);

    internal Fin<VectorCloud> Admit() => Switch(
        ringCase: static (ring) => Ring(points: ring.Vertices, context: ring.Tolerance),
        polylineCase: static (poly) => Polyline(points: poly.Vertices, context: poly.Tolerance),
        clusterCase: static (cluster) =>
            from policy in CloudAdmissionPolicy.Of(context: cluster.Tolerance)
            from readmitted in Cluster(points: cluster.Vertices, context: cluster.Tolerance,
                admission: Some(policy with { Dedup = CloudDedup.Preserve }), mass: cluster.Mass)
            select readmitted);

    public void Dispose() => Switch(
        ringCase: static _ => { },
        polylineCase: static _ => { },
        clusterCase: static cluster => { _ = cluster.Release(); });

    private static Fin<(Seq<Point3d> Points, Context Context)> AdmitPoints(Seq<Point3d> points, Context context, int minimum) {
        return from model in Optional(context).ToFin(new KernelFault.MissingContext())
               from _ in points.TraverseM(p => Acceptance.Value(value: p)).As()
               from count in guard(points.Count >= minimum, new KernelFault.InvalidInput())
               select (Points: points, Context: model);
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionPolicy(
    CloudDedup Dedup, Option<PositiveMagnitude> Tolerance, PositiveMagnitude ConservationTolerance) {
    internal static Fin<CloudAdmissionPolicy> Of(Context context, Option<PositiveMagnitude> tolerance = default) =>
        from conservation in FactoryBridge.Accept<PositiveMagnitude>(candidate: context.For(lane: ToleranceLane.Conservation).Value)
        select new CloudAdmissionPolicy(Dedup: CloudDedup.Merge, Tolerance: tolerance, ConservationTolerance: conservation);
    internal Fin<CloudAdmissionPolicy> Admit() {
        CloudAdmissionPolicy self = this;
        return guard(ValidityClaim.All(
                self.Tolerance.Map(static tolerance => ValidityClaim.Positive(tolerance.Value).Holds).IfNone(true),
                ValidityClaim.Positive(self.ConservationTolerance.Value)), new KernelFault.InvalidInput())
            .ToFin().Map(_ => self);
    }
    internal bool Equivalent(Point3d left, Point3d right) => Dedup.Equivalent(left: left, right: right, tolerance: Tolerance);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmission(
    int InputCount, int OutputCount, int InputDuplicateCoordinateCount,
    Option<PositiveMagnitude> Tolerance, PositiveMagnitude ConservationTolerance, CloudDedup Dedup, Arr<int> OriginalToUnique,
    Option<(double Input, double Merged, double Output)> Mass) : IValidityEvidence {
    public bool IsValid {
        get {
            CloudAdmission self = this;
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(count: InputCount, floor: 1),
                InputDuplicateCoordinateCount >= 0 && OutputCount <= InputCount,
                OutputCount == InputCount || Dedup.Equals(CloudDedup.Merge),
                ValidityClaim.CountExactly(count: OriginalToUnique.Count, expected: InputCount),
                OriginalToUnique.ForAll(i => i >= 0 && i < self.OutputCount),
                Mass.Match(
                    Some: totals => ValidityClaim.All(
                        ValidityClaim.Nonnegative(totals.Input),
                        ValidityClaim.Nonnegative(totals.Merged),
                        ValidityClaim.Nonnegative(totals.Output),
                        Math.Abs(totals.Input - totals.Merged) <= self.ConservationTolerance.Value * Math.Max(1.0, Math.Abs(totals.Input)),
                        Math.Abs(1.0 - totals.Output) <= self.ConservationTolerance.Value),
                    None: static () => true));
        }
    }
}
```

## [03]-[CLOUD_METRICS]

- Owner: `VectorCloudMetric` mints one row per measurement behind ONE `Project<TOut>`, each row a single declaration line naming its fold and its admissible cloud cases; neighborhood-backed rows thread the `neighbors.md` `NeighborhoodPolicy` itself, so `Project<TOut>` resolves its `Option<NeighborhoodPolicy>` against the cloud's own `Context` and admits it ahead of the case-compatibility and output gates — the one policy admission every metric caller shares.
- Entry: the row builders — `Ring`, `All`, `Chain`, `Cluster` — are the whole declaration surface: each fixes the case admission and adapts its fold to the erased measure column, so a row is one line and no arm re-states a case test.
- Auto: `PrincipalFrameOf` builds the frame from the two dominant eigenvectors, and ring orientation reads `ClosedCurveOrientation` against the fitted plane to sign the normal CCW-positive. Skewness is the worst normalized interior-angle deviation from the regular-polygon ideal, compactness `4πA/P²`, moment anisotropy the in-plane principal-moment ratio; chain rows are pure folds over unitized tangents, prefix-sum arc length, and turning-angle curvature. `PlanarWindingOf` takes the query point, so it is a kernel entry rather than a metric row, and a winding consumer composes it with the CCW-signed `RingNormalOf` normal — a sign-arbitrary best-fit-plane normal flips the winding integer. `Shape` answers one `VectorCloudShape` per cloud case, never a per-case sibling record.
- Packages: RhinoCommon (area mass properties, plane fitting, polyline geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measurement is ONE row through the matching builder; a new cloud case extends the builders' adapt arms; a policy knob is one column on `NeighborhoodPolicy`.
- Boundary: neighborhood-backed rows delegate to `neighbors.md`, the fold living on that substrate while the metric row is its cloud-facing name and its census returns unchanged; `AreaMassProperties` and `PolylineCurve` natives stay inside their lease windows; `PlanarWinding` names the 2D ring fold, held distinct from the 3D solid-angle GWN family `reconstruct.md` owns.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(measure: static (c, k) => CloudKernel.RingNormalOf(ring: c)),
        Area = Ring(measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => Acceptance.Value(value: props.Area))),
        Perimeter = Ring(measure: static (c, k) => Acceptance.Value(value: c.Native.Length)),
        EdgeAspect = Ring(measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance)),
        Skewness = Ring(measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c)),
        Compactness = Ring(measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c)),
        MomentAnisotropy = Ring(measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c)),
        RadiiOfGyration = Ring(measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => Acceptance.Value(value: props.CentroidCoordinatesRadiiOfGyration))),
        AreaError = Ring(measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => Acceptance.Value(value: props.AreaError))),
        CentroidError = Ring(measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => Acceptance.Value(value: props.CentroidError)));
    public static readonly VectorCloudMetric Centroid = All(measure: static (c, k) => CloudKernel.CentroidOf(cloud: c)),
        BestFitPlane = All(measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c)),
        PrincipalAxes = All(measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c)),
        PrincipalFrame = All(measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c)),
        Shape = All(measure: static (c, k) => CloudKernel.ShapeOf(cloud: c));
    public static readonly VectorCloudMetric BishopFrames = Chain(measure: static (c, k) => NeighborKernel.BishopChain(cloud: c)),
        TangentFlow = Chain(measure: static (cloud, k) => CloudKernel.TangentFlowOf(points: cloud.Vertices)),
        CumulativeArcLength = Chain(measure: static (cloud, k) => CloudKernel.CumulativeArcLengthOf(points: cloud.Vertices)),
        EdgeCurvatures = Chain(measure: static (cloud, k) => CloudKernel.EdgeCurvaturesOf(points: cloud.Vertices)),
        OpenLength = Chain(measure: static (cloud, k) => CloudKernel.OpenLengthOf(points: cloud.Vertices));
    public static readonly VectorCloudMetric Covariance = Cluster(measure: static (c, _, k) => CloudKernel.CovarianceOf(cluster: c).Map(static v => v.Cov)),
        PrincipalDirection = Cluster(measure: static (c, _, k) => CloudKernel.PrincipalStatsOf(cluster: c).Bind(s => Acceptance.Value(value: CloudKernel.AsVector3d(v: s.Eigen[0].Eigenvector)))),
        Spread = Cluster(measure: static (c, _, k) => CloudKernel.PrincipalStatsOf(cluster: c)
            .Bind(s => Acceptance.Value(value: new Vector3d(s.Eigen[0].Eigenvalue, s.Eigen[1].Eigenvalue, s.Eigen[2].Eigenvalue)))),
        OrientedNormals = Cluster(measure: static (c, p, k) => NeighborKernel.OrientNormals(cluster: c, policy: p)),
        PrincipalCurvature = Cluster(measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p)),
        Curvedness = Cluster(measure: static (c, p, k) => NeighborKernel.Project(CurvatureAxis.Curvedness, cluster: c, policy: p)),
        ShapeIndex = Cluster(measure: static (c, p, k) => NeighborKernel.Project(CurvatureAxis.Shape, cluster: c, policy: p)),
        Admission = Cluster(measure: static (c, _, k) => Fin.Succ(c.Admission)),
        Neighborhood = Cluster(measure: static (c, p, k) => NeighborKernel.GraphOf(index: new NeighborIndex.CloudCase(Source: c), needles: [.. c.Vertices.AsIterable()], policy: p).Map(static graph => graph.Census)),
        CurvatureCensus = Cluster(measure: static (c, p, k) => NeighborKernel.PrincipalCurvatures(cluster: c, policy: p).Map(static r => r.Census));

    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, NeighborhoodPolicy policy);

    private static VectorCloudMetric Ring<TValue>(Func<VectorCloud.RingCase, Fin<TValue>> measure);
    private static VectorCloudMetric All<TValue>(Func<VectorCloud, Fin<TValue>> measure);
    private static VectorCloudMetric Chain<TValue>(Func<VectorCloud, Fin<TValue>> measure);
    private static VectorCloudMetric Cluster<TValue>(Func<VectorCloud.ClusterCase, NeighborhoodPolicy, Fin<TValue>> measure);

    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Option<NeighborhoodPolicy> policy) =>
        from active in policy.Match(
            Some: candidate => candidate.Admit(),
            None: () => NeighborhoodPolicy.Of(context: cloud.Tolerance))
        from output in (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
            (false, _) => Fin.Fail<TOut>(new KernelFault.Unsupported(InputType: cloud.GetType(), OutputType: typeof(TOut))),
            (_, false) => Fin.Fail<TOut>(new KernelFault.Unsupported(InputType: typeof(VectorCloudMetric), OutputType: typeof(TOut))),
            _ => Measure(cloud: cloud, policy: active).Bind(value => value switch {
                Seq<Vector3d> values => ResultProjection.Values<Vector3d, TOut>(values: values, owner: typeof(VectorCloudMetric)),
                Seq<double> values => ResultProjection.Values<double, TOut>(values: values, owner: typeof(VectorCloudMetric)),
                Seq<Plane> values => ResultProjection.Values<Plane, TOut>(values: values, owner: typeof(VectorCloudMetric)),
                _ => Acceptance.Value(value: value).Map(static admitted => (TOut)admitted),
            })
        }
        select output;
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
        Seq<Point3d> points, Option<Arr<double>> mass, CloudAdmissionPolicy policy);

    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Option<Arr<double>> mass) =>
        from moment in SampleMoment.Of(rows: points.Map(static p => Seq(p.X, p.Y, p.Z)),
            weights: mass.Map(static m => toSeq(m.AsIterable())))
        from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: moment.UpperCovariance)
        select (Mean: AsVector3d(v: moment.Mean), Cov: cov);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster) =>
        cluster.Mass.Match(
            Some: mass => MassOf(mass: mass, count: cluster.Vertices.Count)
                .Bind(normalized => CovarianceOf(points: cluster.Vertices, mass: Some(normalized))),
            None: () => CovarianceOf(points: cluster.Vertices, mass: Option<Arr<double>>.None));
    internal static Fin<Arr<double>> MassOf(VectorCloud.ClusterCase cluster) =>
        MassOf(mass: cluster.Mass.IfNone(() => new Arr<double>([.. Enumerable.Repeat(1.0 / cluster.Vertices.Count, cluster.Vertices.Count)])), count: cluster.Vertices.Count);
    internal static Fin<Arr<double>> MassOf(Arr<double> mass, int count) =>
        from _ in guard(mass.Count == count && mass.ForAll(static w => double.IsFinite(w) && w > 0.0), new KernelFault.InvalidInput())
        from total in mass.Fold(0.0, static (s, w) => s + w) switch {
            double sum when double.IsFinite(sum) && sum > 0.0 => Fin.Succ(sum),
            _ => Fin.Fail<double>(new KernelFault.InvalidInput()),
        }
        select new Arr<double>([.. mass.AsIterable().Select(w => w / total)]);

    internal static Fin<(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen)>
        PrincipalStatsOf(VectorCloud.ClusterCase cluster) =>
        from stats in CovarianceOf(cluster: cluster)
        from eigen in stats.Cov.DecomposeEigenDetailed().Map(static solved => solved.Pairs)
        from full in eigen.Count >= 3
            ? Fin.Succ((Mean: stats.Mean, Eigen: eigen))
            : Fin.Fail<(Vector3d, Seq<(double, Arr<double>)>)>(new KernelFault.InvalidResult())
        select full;
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);

    internal static Fin<T> WithMassProperties<T>(VectorCloud.RingCase ring, Func< AreaMassProperties, Fin<T>> project);
    internal static Fin<Vector3d> RingNormalOf(VectorCloud.RingCase ring);
    internal static Fin<double> EdgeAspectOf(Polyline native, Context context);
    internal static Fin<double> RingSkewnessOf(VectorCloud.RingCase ring);
    internal static Fin<double> RingCompactnessOf(VectorCloud.RingCase ring);
    internal static Fin<double> RingMomentAnisotropyOf(VectorCloud.RingCase ring);

    internal static Fin<Point3d> CentroidOf(VectorCloud cloud);
    internal static Fin<Plane> BestFitPlaneOf(VectorCloud cloud);
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalAxesOf(VectorCloud cloud);
    internal static Fin<Plane> PrincipalFrameOf(VectorCloud cloud);
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud);

    internal static Fin<Seq<Vector3d>> TangentFlowOf(Seq<Point3d> points);
    internal static Fin<Seq<double>> CumulativeArcLengthOf(Seq<Point3d> points);
    internal static Fin<Seq<double>> EdgeCurvaturesOf(Seq<Point3d> points);
    internal static Fin<double> OpenLengthOf(Seq<Point3d> points);

    internal static Fin<int> PlanarWindingOf(Seq<Point3d> ring, Vector3d planeNormal, Point3d query) =>
        ring.Count < 3
            ? Fin.Fail<int>(new KernelFault.InvalidInput())
            : Acceptance.Value(value: (int)Math.Round(
                ring.Map((v, i) => (V0: v - query, V1: ring[(i + 1) % ring.Count] - query))
                    .Fold(0.0, (sum, pair) => sum + Vector3d.VectorAngle(v1: pair.V0, v2: pair.V1, vNormal: planeNormal)) / (2.0 * Math.PI),
                MidpointRounding.ToEven));
}
```

## [04]-[HULL]

- Owner: `CloudHullKind` names the hull species — a rejected 3D hull degrades to `ConvexFootprint2D` under `HullRoute.Fallback`, the route carrying that provenance rather than a second kind row; concave columns `Alpha` and `Lambda` derive from the cluster's mean spacing when the caller supplies neither. `CloudHullRejection` keys the typed refusal off the MIConvexHull ordinals, and its presence on the result IS the completion state.
- Cases: `HullRoute` rows are the route evidence the outcome carries as a `CapabilitySet` — `Coplanar` says the 3D preflight refused the input, `Native` that a host route answered, `Fallback` that the 2D footprint stood in for a refused 3D hull. They combine under a legal-corner law (a coplanar rejection always rides beside a fallback), which is why they are one set rather than three independent flags open to inconsistent pairing.
- Entry: `ComputeHullDetailed` is cluster-only, and every declared kind computes, so `Rejection` presence discriminates outcome alone.
- Auto: `Convex3D` routes native through `Mesh.CreateConvexHull3D` behind a coplanar preflight, duplicating the mesh out of its `using` window; `ConvexFootprint2D` — reached directly or as the 3D fallback arm — fits the PCA plane, run `PolylineCurve.CreateConvexHull2d`, verify containment within tolerance, and mesh via `Mesh.CreateFromClosedPolyline`. `AlphaShape` keeps every triangle whose circumradius stays within `Alpha`; `ConcaveOutline` erodes the longest boundary edge while it exceeds `Lambda` and removal preserves regularity, abandoning no vertex and leaving the boundary a single simple cycle. `Faceted3D` and `IndexedFootprint2D` are the index-preserving twins of the two host routes — the host `Mesh` and `PolylineCurve` answer geometry and drop which cluster vertex each output came from, so the typed rows keep facet adjacency, per-facet outward normals, and the cluster index every downstream census and dual keys on.
- Packages: RhinoCommon (native convex hull, plane fitting, polyline meshing), MIConvexHull (`Triangulation.CreateDelaunay<CloudVertex, CloudCell>`, `ConvexHull.Create<CloudVertex>` with `ConvexHull.Faces`/`ConvexFace.Adjacency`/`.Normal`, `ConvexHull.Create2D<CloudVertex>`, `ConvexHullCreationResult.Outcome`/`.Result` — one index-carrying `IVertex`/`IVertex2D` vertex over the package default face and cell carriers, tolerance threaded from the admitted policy), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Law: `CloudHullPolicy` is the AUTHORITY for every threshold the fold ran under and `CloudHullResult.Tolerance`/`AngleTolerance` are its published copy, carried so a consumer reading a stored result need not hold the policy that produced it; a present `Rejection` is the one refusal spelling, and the claim fold ties the native route and the vertex floor to its absence rather than to a second status column.
- Growth: a new hull species is one kind row and one arm in the hull fold, or one filter predicate over the shared Delaunay fold; a new concave criterion is one policy column; a new rejection cause is one `CloudHullRejection` row keyed off its package ordinal; a new route fact is one `HullRoute` row.
- Boundary: both concave kinds share ONE Delaunay fold over `MIConvexHull`'s complex, the filter predicate their only difference; `Triangulation.CreateDelaunay` is the foreign-exception boundary on this owner and `key.Catch` preserves its exact exceptional `Error`. `ConvexHull.*` instead returns a typed outcome, so `Faceted3D` and `IndexedFootprint2D` gate `Outcome` ahead of `Result` and publish `CloudHullRejection` without a capture. This family owns the native-first host, index-preserving, and concave hull kinds; the predicate-exact hull fold homes at `Meshing/delaunay` `LowerHull`, and `SolidOf` is the one volume-and-centroid producer both this family and `[05]` read.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new();
    public static readonly CloudHullKind ConvexFootprint2D = new();
    public static readonly CloudHullKind ConcaveOutline = new();
    public static readonly CloudHullKind AlphaShape = new();
    public static readonly CloudHullKind Faceted3D = new();
    public static readonly CloudHullKind IndexedFootprint2D = new();
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
    internal static Fin<Option<CloudHullRejection>> Of(ConvexHullCreationResultOutcome outcome) =>
        outcome is ConvexHullCreationResultOutcome.Success ? Fin.Succ(Option<CloudHullRejection>.None)
        : FactoryBridge.Row<int, CloudHullRejection>((int)outcome).Map(Some).MapFail(_ => new KernelFault.InvalidResult());
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullPolicy(
    PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
    Option<PositiveMagnitude> Alpha, Option<PositiveMagnitude> Lambda) {
    internal static Fin<CloudHullPolicy> AdmitOrDefault(Option<CloudHullPolicy> policy, Context context) {
        (double tolerance, double angle, Option<PositiveMagnitude> alpha, Option<PositiveMagnitude> lambda) = policy.Match(
            Some: static candidate => (candidate.Tolerance.Value, candidate.AngleTolerance.Value, candidate.Alpha, candidate.Lambda),
            None: () => (context.For(lane: ToleranceLane.Deviation).Value, context.Angle.Value, Option<PositiveMagnitude>.None, Option<PositiveMagnitude>.None));
        return from admittedTolerance in FactoryBridge.Accept<PositiveMagnitude>(candidate: tolerance)
               from admittedAngle in FactoryBridge.Accept<VectorAngle>(candidate: angle)
               from admittedAlpha in AdmitMagnitude(value: alpha)
               from admittedLambda in AdmitMagnitude(value: lambda)
               select new CloudHullPolicy(Tolerance: admittedTolerance, AngleTolerance: admittedAngle, Alpha: admittedAlpha, Lambda: admittedLambda);
    }
    private static Fin<Option<PositiveMagnitude>> AdmitMagnitude(Option<PositiveMagnitude> value) =>
        value.TraverseM(magnitude => FactoryBridge.Accept<PositiveMagnitude>(candidate: magnitude.Value)).As();
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
    CloudHullKind Kind, PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
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
        HullRoute.Law.Admit(held: Route).IsSucc,
        Rejection.IsSome || Route.Admits(capability: HullRoute.Native),
        Rejection.IsSome || OutputVertexCount >= 3,
        Mesh.IsSome || Solid.IsSome || Rejection.IsSome);

    internal Fin<TOut> Project<TOut>(Context context) {
        CloudHullResult self = this;
        return ResultProjection.Rows<CloudHullResult, TOut>(self: self,
            ProjectionRow.Of<CloudSolid>(() => self.Solid.ToFin(new KernelFault.Unsupported(InputType: typeof(CloudHullResult), OutputType: typeof(CloudSolid)))),
            ProjectionRow.Of<Seq<CloudFacet>>(() => self.Solid.Map(static solid => solid.Facets)
                .ToFin(new KernelFault.Unsupported(InputType: typeof(CloudHullResult), OutputType: typeof(Seq<CloudFacet>)))),
            ProjectionRow.Of<Mesh>(() => self.Mesh.ToFin(new KernelFault.Unsupported(InputType: typeof(CloudHullResult), OutputType: typeof(Mesh)))
                .Bind(mesh => Acceptance.Value(value: mesh))),
            ProjectionRow.Of<VectorCloud>(() => self.Mesh.ToFin(new KernelFault.Unsupported(InputType: typeof(CloudHullResult), OutputType: typeof(VectorCloud)))
                .Bind(mesh => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context))));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal sealed class CloudVertex : IVertex, IVertex2D {
    public CloudVertex() : this(index: 0, x: 0.0, y: 0.0) { }
    internal CloudVertex(int index, double x, double y, double z = 0.0) {
        Index = index; Position = [x, y, z];
    }
    public int Index { get; }
    public double[] Position { get; }
    public double X => Position[0];
    public double Y => Position[1];
}

internal static partial class CloudKernel {
    internal static Fin<CloudHullResult> ComputeHullDetailed(
        VectorCloud.ClusterCase cluster, CloudHullKind kind, CloudHullPolicy policy);

    internal static Fin<Option<CloudSolid>> SolidOf(Point3d[] points, double tolerance) {
        ConvexHullCreationResult<CloudVertex, CloudFace> hull = ConvexHull.Create<CloudVertex>(
            data: [.. points.Select(static (p, i) => new CloudVertex(index: i, x: p.X, y: p.Y, z: p.Z))],
            tolerance: tolerance);
        return CloudHullRejection.Of(outcome: hull.Outcome).Bind(rejection => {
            if (rejection.IsSome) return Fin.Succ(Option<CloudSolid>.None);
            Point3d anchor = points.Aggregate(Point3d.Origin, static (sum, p) => sum + p) / points.Length;
            Seq<CloudFace> faces = toSeq(hull.Result.Faces);
            (double volume, Vector3d moment) = faces.Fold(
                (Volume: 0.0, Moment: Vector3d.Zero), (acc, face) => {
                    (Vector3d u, Vector3d v, Vector3d w) = (PointOf(face.Vertices[0]) - anchor,
                        PointOf(face.Vertices[1]) - anchor, PointOf(face.Vertices[2]) - anchor);
                    double tet = Math.Abs(Vector3d.CrossProduct(a: v - u, b: w - u) * u) / 6.0;
                    return (acc.Volume + tet, acc.Moment + (tet * 0.25 * (u + v + w)));
                });
            if (volume <= tolerance * tolerance * tolerance) return Fin.Succ(Option<CloudSolid>.None);
            FrozenDictionary<CloudFace, int> ordinal = faces.Map(static (face, index) => (Face: face, Index: index))
                .ToFrozenDictionary(static row => row.Face, static row => row.Index);
            return faces.Traverse(face => toSeq(face.Adjacency)
                    .Traverse(neighbor => Optional(neighbor).Map(present => ordinal[present]))
                    .Map(slots => new CloudFacet(
                        Vertices: new Arr<int>([.. face.Vertices.Select(static corner => corner.Index)]),
                        Adjacency: new Arr<int>([.. slots]),
                        Normal: new Vector3d(x: face.Normal[0], y: face.Normal[1], z: face.Normal[2]))))
                .ToFin(new KernelFault.InvalidResult())
                .Bind(facets => new CloudSolid(Volume: volume, Centroid: anchor + (moment / volume), Facets: facets) switch {
                    CloudSolid solid when solid.IsValid => Fin.Succ(Some(solid)),
                    _ => Fin.Fail<Option<CloudSolid>>(new KernelFault.InvalidResult()),
                });
        });
    }

    internal static Point3d PointOf(CloudVertex vertex) => new(x: vertex.Position[0], y: vertex.Position[1], z: vertex.Position[2]);
}
```

## [05]-[VORONOI_COMPLEX]

- Owner: `CloudVoronoiCell` is the 3D dual cell over a cluster cloud — one row per site — and its nested `Bound` union carries the measures boundedness licenses — `Bounded` alone holds volume, centroid, and extent, `Unbounded` and `Degenerate` hold none — so an impossible partial measurement is unrepresentable and no reader re-derives the relation.
- Entry: `CloudVoronoiResult.Of(VectorCloud, Option<PositiveMagnitude>)` is the one public 3D-dual entry — cluster-only, admitting the deviation-lane tolerance once as the `PositiveMagnitude` it threads as `PlaneDistanceTolerance`; `CloudKernel.CensusOf` takes that admitted value unchanged and derives one `epsilon` for MIConvexHull and the numeric kernels; `CloudHullPolicy`'s angle, alpha, and lambda columns describe hull algorithms this fold cannot execute, so none of them reaches it. `NaturalNeighborField` is the Sibson stolen-volume owner — `Of` mints the base dual once and retains only its admitted vertices and site-keyed bounded volumes behind a private constructor, and each `Weights` query pays only the inserted-site dual and the volume-loss fold against that table, so an interpolant over M queries neither rebuilds the unchanging half nor re-mints a vertex M times — the one weight source the `Meshing/reconstruct` evaluator composes.
- Auto: each Delaunay cell IS a Voronoi vertex, so ONE circumsphere sweep over `VoronoiMesh.Vertices` mints the measured sphere table — a `FrozenDictionary` over the cells that HAVE a sphere, where a missing key IS degeneracy — and each `VoronoiEdge.Source`/`.Target` pair projects inside `CellRows` to the `Line` between the two centers that table holds, absent when either cell is unmeasured, so the 1-skeleton falls out as `Skeleton` with no second traversal and no reader can reach a dual before the sweep that measures it. Bound classification derives structurally, never by proximity heuristic: `SolidOf` over the sites answers the site hull in the same pass that answers `HullVolume`, and a site on that hull owns an open cell because the Voronoi region of a hull vertex extends to infinity. Bounded cells take the convex hull of their incident circumcenters as geometry, so `SolidOf` answers volume and centroid too and the `[04]` faceted row is this band's measurement kernel.
- Law: `CloudVoronoiCensus` carries the tolerance and input count beside the completed census; `BoundedVolumeTotal` never exceeds `HullVolume`, and that ordering is the census's own conservation claim.
- Packages: MIConvexHull (`VoronoiMesh.Create<CloudVertex, CloudCell>`, `VoronoiMesh.Vertices`/`.Edges`, `VoronoiEdge.Source`/`.Target`, `ConvexHull.Create<CloudVertex>` through `SolidOf`), RhinoCommon, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new per-cell measure is one column on `Bound.Bounded` with its arm in the bounded fold; a new bound species is one `Bound` case every generated `Switch` then demands; a new census tally is one column on `CloudVoronoiCensus`; a new interpolant over one site set is one member on `NaturalNeighborField`, sharing the baseline volumes and vertices it already holds.
- Boundary: this band owns the 3D cell decomposition alone — 2D border-clipped point-site Voronoi homes at `Meshing/delaunay` `Tessellation.VoronoiDual`, whose bounded-cell overload is the predicate-exact planar peer, and `Meshing/offset` reads that owner for the medial locus. `VoronoiMesh.Create` returns the bare complex and throws on degenerate input, so `Try.lift` keeps that exact exceptional `Error` on the failure result; the `ConvexHull.*` APIs instead return a typed outcome and alone publish `CloudHullRejection`. Natural-neighbour interpolation reads `Bound.Bounded.Volume` from here and fits nothing; the admitting minter is `Meshing/reconstruct`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly partial record struct CloudVoronoiCell(
    int Site, Point3d Seed, CloudVoronoiCell.Bound Measure, Arr<int> Vertices, Arr<int> Neighbors) : IValidityEvidence {
    [Union]
    public abstract partial record Bound {
        private Bound() { }
        public sealed record Bounded(double Volume, Point3d Centroid, double Extent) : Bound;
        public sealed record Unbounded : Bound;
        public sealed record Degenerate : Bound;
    }

    public bool IsValid {
        get {
            CloudVoronoiCell self = this;
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(count: Site, floor: 0),
                ValidityClaim.Finite(Seed),
                ValidityClaim.CountAtLeast(count: Vertices.Count, floor: 1),
                Vertices.ForAll(static v => v >= 0),
                Neighbors.ForAll(neighbor => neighbor >= 0 && neighbor != self.Site),
                Measure.Switch(
                    bounded: static measured => ValidityClaim.All(
                        ValidityClaim.Positive(measured.Volume),
                        ValidityClaim.Finite(measured.Centroid),
                        ValidityClaim.Positive(measured.Extent)),
                    unbounded: static _ => true,
                    degenerate: static _ => true));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudVoronoiCensus(
    PositiveMagnitude PlaneDistanceTolerance, int InputCount, int DualVertexCount, int UnmeasuredVertexCount, int DualEdgeCount, int SkeletonEdgeCount,
    int BoundedCellCount, int UnboundedCellCount, int DegenerateCellCount,
    Option<double> BoundedVolumeTotal, Option<double> HullVolume) : IValidityEvidence {
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

    internal Fin<TOut> Project<TOut>(Context context) {
        CloudVoronoiResult self = this;
        return ResultProjection.Rows<CloudVoronoiResult, TOut>(self: self,
            ProjectionRow.Of<CloudVoronoiCensus>(() => Fin.Succ(self.Census)),
            ProjectionRow.Of<Seq<CloudVoronoiCell>>(() => Fin.Succ(self.Cells)),
            ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(toSeq(self.Skeleton.AsIterable()
                .Select(edge => new Line(from: self.Vertices[edge.Tail], to: self.Vertices[edge.Head]))))),
            ProjectionRow.Of<VectorCloud>(() => VectorCloud.Cluster(
                points: toSeq(self.Vertices.AsIterable()), context: context)));
    }

    public static Fin<CloudVoronoiResult> Of(
        VectorCloud source, Option<PositiveMagnitude> tolerance = default) {
        return from cloud in Admit.Need(value: source)
               from cluster in cloud is VectorCloud.ClusterCase active
                   ? Fin.Succ(active)
                   : Fin.Fail<VectorCloud.ClusterCase>(new KernelFault.Unsupported(InputType: cloud.GetType(), OutputType: typeof(CloudVoronoiResult)))
               from distance in tolerance.Match(
                   Some: static supplied => Fin.Succ(supplied),
                   None: () => FactoryBridge.Accept<PositiveMagnitude>(candidate: cluster.Tolerance.For(lane: ToleranceLane.Deviation).Value))
               from _ in guard(cluster.Vertices.Count >= 4, new KernelFault.InvalidInput()).ToFin()
               let sites = cluster.Vertices.Map(static (point, index) => new CloudVertex(index: index, x: point.X, y: point.Y, z: point.Z)).ToArray()
               from result in Try.lift(() => CloudKernel.CensusOf(
                   sites: sites, tolerance: distance,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: sites, PlaneDistanceTolerance: distance.Value))).Run().Bind(static inner => inner)
               select result;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class CloudKernel {
    internal static Fin<CloudVoronoiResult> CensusOf(
        CloudVertex[] sites, VoronoiMesh<CloudVertex, CloudCell, CloudEdge> complex, PositiveMagnitude tolerance) {
        double epsilon = tolerance.Value;
        CloudCell[] cells = [.. complex.Vertices];
        FrozenDictionary<CloudCell, (Point3d Center, double Radius)> spheres = cells
            .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: epsilon)))
            .Where(static row => row.Sphere.IsSome)
            .ToFrozenDictionary(static row => row.Cell,
                static row => row.Sphere.IfNone(default((Point3d Center, double Radius))));
        return from hull in SolidOf(points: [.. sites.Select(PointOf)], tolerance: epsilon)
               let open = hull.Map(static solid => solid.Facets.Fold(Set<int>(),
                   static (acc, facet) => facet.Vertices.Fold(acc, static (set, corner) => set.Add(corner)))).IfNone(Set<int>())
               from rows in CellRows(cells: cells, complex: complex, spheres: spheres, open: open, sites: sites, tolerance: epsilon)
               let tally = rows.Cells.Fold((Bounded: 0, Unbounded: 0, Degenerate: 0, Volume: 0.0), static (acc, cell) => cell.Measure.Switch(
                   state: acc,
                   bounded: static (t, measured) => (t.Bounded + 1, t.Unbounded, t.Degenerate, t.Volume + measured.Volume),
                   unbounded: static (t, _) => (t.Bounded, t.Unbounded + 1, t.Degenerate, t.Volume),
                   degenerate: static (t, _) => (t.Bounded, t.Unbounded, t.Degenerate + 1, t.Volume)))
               let census = new CloudVoronoiCensus(
                   PlaneDistanceTolerance: tolerance, InputCount: sites.Length, DualVertexCount: cells.Length, UnmeasuredVertexCount: cells.Length - rows.Vertices.Count,
                   DualEdgeCount: complex.Edges.Count(), SkeletonEdgeCount: rows.Skeleton.Count,
                   BoundedCellCount: tally.Bounded, UnboundedCellCount: tally.Unbounded, DegenerateCellCount: tally.Degenerate,
                   BoundedVolumeTotal: tally.Bounded > 0 ? Some(tally.Volume) : Option<double>.None,
                   HullVolume: hull.Map(static solid => solid.Volume))
               from verified in census.IsValid ? Fin.Succ(census) : Fin.Fail<CloudVoronoiCensus>(new KernelFault.InvalidResult())
               from verifiedResult in new CloudVoronoiResult(Cells: rows.Cells, Vertices: rows.Vertices, Skeleton: rows.Skeleton,
                       Census: verified) switch {
                   CloudVoronoiResult whole when whole.IsValid => Fin.Succ(whole),
                   _ => Fin.Fail<CloudVoronoiResult>(new KernelFault.InvalidResult()),
               }
               select verifiedResult;
    }

    private static Fin<(Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton)> CellRows(
        CloudCell[] cells, VoronoiMesh<CloudVertex, CloudCell, CloudEdge> complex,
        FrozenDictionary<CloudCell, (Point3d Center, double Radius)> spheres, Set<int> open,
        CloudVertex[] sites, double tolerance);

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

// --- [NATURAL_NEIGHBORS]
public sealed class NaturalNeighborField {
    private readonly FrozenDictionary<int, double> volumes;
    private readonly CloudVertex[] sites;
    private readonly PositiveMagnitude tolerance;
    private NaturalNeighborField(FrozenDictionary<int, double> volumes, CloudVertex[] sites, PositiveMagnitude tolerance) {
        this.volumes = volumes; this.sites = sites; this.tolerance = tolerance;
    }

    internal static Fin<NaturalNeighborField> Of(Seq<Point3d> sites, PositiveMagnitude tolerance) {
        CloudVertex[] seeds = sites.Map(static (p, i) => new CloudVertex(index: i, x: p.X, y: p.Y, z: p.Z)).ToArray();
        return from _ in guard(sites.Count >= 4, new KernelFault.InvalidInput()).ToFin()
               from dual in Try.lift(() => CloudKernel.CensusOf(sites: seeds, tolerance: tolerance,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: seeds, PlaneDistanceTolerance: tolerance.Value))).Run().Bind(static inner => inner)
               let volumes = dual.Cells.Bind(cell => cell.Measure.Switch(
                   bounded: measured => Seq((Site: cell.Site, Volume: measured.Volume)),
                   unbounded: static _ => Seq<(int Site, double Volume)>(),
                   degenerate: static _ => Seq<(int Site, double Volume)>()))
                   .ToFrozenDictionary(static row => row.Site, static row => row.Volume)
               select new NaturalNeighborField(volumes: volumes, sites: seeds, tolerance: tolerance);
    }

    internal Fin<Seq<(int Site, double Weight)>> Weights(Point3d query) {
        CloudVertex[] after = [
            .. sites,
            new CloudVertex(index: sites.Length, x: query.X, y: query.Y, z: query.Z)];
        return from inserted in Try.lift(() => CloudKernel.CensusOf(sites: after, tolerance: tolerance,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: after, PlaneDistanceTolerance: tolerance.Value))).Run().Bind(static inner => inner)
               from cell in inserted.Cells.Find(row => row.Site == sites.Length).ToFin(new KernelFault.InvalidInput())
               from _support in cell.Measure.Switch(
                   bounded: static _ => Fin.Succ(unit),
                   unbounded: _ => Fin.Fail<Unit>(new KernelFault.InvalidInput()),
                   degenerate: _ => Fin.Fail<Unit>(new KernelFault.InvalidInput()))
               from losses in toSeq(cell.Neighbors.AsIterable().Filter(site => site != sites.Length))
                   .TraverseM(site =>
                       (from was in volumes.TryGetValue(site, out double volume) ? Some(volume) : Option<double>.None
                        from now in inserted.Cells.Find(row => row.Site == site).Bind(row => row.Measure.Switch(
                            bounded: static measured => Some(measured.Volume),
                            unbounded: static _ => Option<double>.None,
                            degenerate: static _ => Option<double>.None))
                        select (Site: site, Loss: was - now)).ToFin(new KernelFault.InvalidInput())).As()
               let positive = losses.Filter(static row => row.Loss > 0.0)
               let total = positive.Sum(static row => row.Loss)
               from _unity in guard(positive.Count >= 1 && double.IsFinite(total) && total > 0.0, new KernelFault.InvalidResult())
               select positive.Map(row => (row.Site, Weight: row.Loss / total));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
