using Foundation.CSharp.Analyzers.Contracts;
using MathNet.Numerics.Statistics;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudCurvatureRangeKind {
    public static readonly CloudCurvatureRangeKind Empty = new(key: 0);
    public static readonly CloudCurvatureRangeKind Plane = new(key: 1);
    public static readonly CloudCurvatureRangeKind Sphere = new(key: 2);
    public static readonly CloudCurvatureRangeKind Saddle = new(key: 3);
    public static readonly CloudCurvatureRangeKind Mixed = new(key: 4);
}

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
    public static readonly CloudHullStatus Unsupported = new(key: 1);
    public static readonly CloudHullStatus Rejected = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class CloudNeighborhoodSearchBackend {
    public static readonly CloudNeighborhoodSearchBackend RhinoPointCloudKnn = new(key: 0);
    public static readonly CloudNeighborhoodSearchBackend RhinoPointCloudRadius = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SinkhornNumericStatus {
    public static readonly SinkhornNumericStatus FiniteAccepted = new(key: 0);
    public static readonly SinkhornNumericStatus UnderflowFloored = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SinkhornResidualKind {
    public static readonly SinkhornResidualKind MarginalMass = new(key: 0);
    public static readonly SinkhornResidualKind ScalingChange = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SinkhornStopKind {
    public static readonly SinkhornStopKind BalancedMarginalsConverged = new(key: 0, converged: true);
    public static readonly SinkhornStopKind RelaxedScalingConverged = new(key: 1, converged: true);
    public static readonly SinkhornStopKind BalancedMarginalsStoppedWithoutConvergence = new(key: 2, converged: false);
    public static readonly SinkhornStopKind RelaxedScalingStoppedWithoutConvergence = new(key: 3, converged: false);
    public bool Converged { get; }
}

[Union]
public abstract partial record VectorCloud {
    private VectorCloud() { }
    public sealed record RingCase : VectorCloud { internal RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) { this.Vertices = Vertices; this.Native = Native; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Polyline Native { get; } public Context Tolerance { get; } }
    public sealed record PolylineCase : VectorCloud { internal PolylineCase(Seq<Point3d> Vertices, Context Tolerance) { this.Vertices = Vertices; this.Tolerance = Tolerance; } public Seq<Point3d> Vertices { get; } public Context Tolerance { get; } }
    public sealed record ClusterCase : VectorCloud {
        internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass = default, CloudAdmissionReceipt Admission = default) { this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass; this.Admission = Admission.IsValid ? Admission : new CloudAdmissionReceipt(InputCount: Vertices.Count, OutputCount: Vertices.Count, InputDuplicateCoordinateCount: 0, MergedCoordinateCount: 0, Tolerance: 0.0, Deduplicated: false, OriginalToUnique: new Arr<int>([.. Enumerable.Range(start: 0, count: Vertices.Count)]), MassInputTotal: Mass.Map(static values => Enumerable.Sum(source: values.AsIterable())), MassOutputTotal: Mass.Map(static values => Enumerable.Sum(source: values.AsIterable()))); }
        public Seq<Point3d> Vertices { get; }
        public Context Tolerance { get; }
        public Option<Arr<double>> Mass { get; }
        public CloudAdmissionReceipt Admission { get; }
        private static readonly ConditionalWeakTable<ClusterCase, PointCloud> CloudCache = [];
        internal PointCloud Indexed => CloudCache.GetValue(key: this, createValueCallback: static c => {
            PointCloud pc = [];
            pc.AddRange(points: c.Vertices.AsIterable());
            return pc;
        });
        internal Fin<ClosestHit> ClosestVertex(Point3d sample, Op key) =>
            Indexed.ClosestPoint(testPoint: sample) switch {
                int idx when idx >= 0 && idx < Vertices.Count => key.AcceptValue(value: ClosestHit.At(
                    target: sample,
                    point: Indexed.PointAt(index: idx),
                    component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: idx)))),
                _ => Fin.Fail<ClosestHit>(error: key.InvalidResult()),
            };
        internal Fin<Seq<int>> WithinRadius(Point3d sample, double radius, Op key) =>
            new Sphere(center: sample, radius: radius) switch {
                { IsValid: false } => Fin.Fail<Seq<int>>(error: key.InvalidInput()),
                Sphere ball => key.Catch(() => {
                    IEnumerable<int[]> found = RTree.PointCloudClosestPoints(pointcloud: Indexed, needlePts: [sample], limitDistance: ball.Radius);
                    using IDisposable? lease = found as IDisposable;
                    return key.Accept(values: found.FirstOrDefault(defaultValue: []).Where(i => i >= 0 && i < Vertices.Count));
                }),
            };
    }
    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 1)
        let closed = admitted.Points.Count > 1 && admitted.Points[0].EpsilonEquals(other: admitted.Points[admitted.Points.Count - 1], epsilon: admitted.Context.Absolute.Value)
        let vertices = closed ? admitted.Points.Init : admitted.Points
        from _ in guard(vertices.Count >= 3, admitted.Key.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from __ in guard(native.IsValid && native.IsClosedWithinTolerance(admitted.Context.Absolute.Value) && native.SegmentCount >= 3, admitted.Key.InvalidInput())
        from ___ in Optional(native.ToPolylineCurve()).ToFin(admitted.Key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (admitted.Context, admitted.Key), project: static (state, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: state.Context.Absolute.Value)).ToFin(state.Key.InvalidResult()).Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(state.Key.InvalidInput()))))
        select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: admitted.Context);
    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) =>
        AdmitPoints(points: points, context: context, key: key, minimum: 2)
            .Map(static admitted => (VectorCloud)new PolylineCase(Vertices: admitted.Points, Tolerance: admitted.Context));
    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, CloudAdmissionPolicy? admission = null, Op? key = null) =>
        AdmitCluster(points: points, mass: None, context: context, key: key, policy: admission.HasValue ? Some(admission.Value) : Option<CloudAdmissionPolicy>.None)
            .Map(static admitted => (VectorCloud)new ClusterCase(Vertices: admitted.Points, Tolerance: admitted.Context, Mass: admitted.Mass, Admission: admitted.Receipt));
    public static Fin<VectorCloud> WeightedCluster(Seq<Point3d> points, Seq<double> mass, Context context, CloudAdmissionPolicy? admission = null, Op? key = null) =>
        AdmitCluster(points: points, mass: Some(new Arr<double>([.. mass.AsIterable()])), context: context, key: key, policy: admission.HasValue ? Some(admission.Value) : Option<CloudAdmissionPolicy>.None)
            .Map(static admitted => (VectorCloud)new ClusterCase(Vertices: admitted.Points, Tolerance: admitted.Context, Mass: admitted.Mass, Admission: admitted.Receipt));
    internal Fin<VectorCloud> Admit(Op key) =>
        this switch {
            RingCase ring => Ring(points: ring.Vertices, context: ring.Tolerance, key: key),
            PolylineCase polyline => Polyline(points: polyline.Vertices, context: polyline.Tolerance, key: key),
            ClusterCase cluster => from admitted in AdmitCluster(points: cluster.Vertices, mass: cluster.Mass, context: cluster.Tolerance, key: key, policy: Some(CloudAdmissionPolicy.Default with { Deduplicate = false }))
                                   select (VectorCloud)new ClusterCase(Vertices: admitted.Points, Tolerance: admitted.Context, Mass: admitted.Mass, Admission: admitted.Receipt),
            _ => Fin.Fail<VectorCloud>(key.InvalidInput()),
        };
    internal static Fin<VectorCloud> Admit(VectorCloud value, Op key) =>
        FieldNabla.NotNull(value: value, key: key).Bind(cloud => cloud.Admit(key: key));
    private static Fin<(Seq<Point3d> Points, Context Context, Op Key)> AdmitPoints(Seq<Point3d> points, Context context, Op? key, int minimum) =>
        key.OrDefault() switch {
            Op op => from model in FieldNabla.NotNull(value: context, error: op.MissingContext())
                     from finite in FieldNabla.AllFinite(points: points, key: op)
                     from count in FieldNabla.CountAtLeast(count: points.Count, minimum: minimum, key: op)
                     select (Points: points, Context: model, Key: op),
        };
    private static Fin<(Seq<Point3d> Points, Context Context, Op Key, Option<Arr<double>> Mass, CloudAdmissionReceipt Receipt)> AdmitCluster(Seq<Point3d> points, Option<Arr<double>> mass, Context context, Op? key, Option<CloudAdmissionPolicy> policy) =>
        key.OrDefault() switch {
            Op op => from model in FieldNabla.NotNull(value: context, error: op.MissingContext())
                     from finite in FieldNabla.AllFinite(points: points, key: op)
                     from count in FieldNabla.CountAtLeast(count: points.Count, minimum: 1, key: op)
                     from active in policy.IfNone(CloudAdmissionPolicy.Default).Admit(key: op)
                     from admitted in CloudKernel.AdmitCluster(points: points, mass: mass, policy: active, key: op)
                     select (admitted.Points, model, op, admitted.Mass, admitted.Receipt),
        };
}

[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0, measure: static (c, k) => CloudKernel.RingNormalOf(ring: c, key: k)), Area = Ring(key: 1, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.Area), key: k)), Perimeter = Ring(key: 2, measure: static (c, k) => k.AcceptValue(value: c.Native.Length)), EdgeAspect = Ring(key: 3, measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance, key: k)), Skewness = Ring(key: 4, measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c, key: k)), Compactness = Ring(key: 5, measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c, key: k)), MomentAnisotropy = Ring(key: 6, measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c, key: k));
    public static readonly VectorCloudMetric RadiiOfGyration = Ring(key: 7, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k)), AreaError = Ring(key: 8, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.AreaError), key: k)), CentroidError = Ring(key: 9, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidError), key: k));
    public static readonly VectorCloudMetric Centroid = All(key: 10, measure: static (c, k) => CloudKernel.CentroidOf(cloud: c, key: k)), BestFitPlane = All(key: 11, measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c, key: k)), PrincipalAxes = All(key: 12, measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c, key: k)), PrincipalFrame = All(key: 13, measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c, key: k)), Shape = All(key: 14, measure: static (c, k) => CloudKernel.ShapeOf(cloud: c, key: k));
    public static readonly VectorCloudMetric BishopFrames = Cases(key: 15, admitsCase: static cloud => cloud is VectorCloud.PolylineCase or VectorCloud.RingCase, measure: static (cloud, k) => CloudKernel.BishopFramesOf(cloud: cloud, key: k)), TangentFlow = Poly(key: 16, measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k)), CumulativeArcLength = Poly(key: 17, measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k)), EdgeCurvatures = Poly(key: 18, measure: static (pts, k) => CloudKernel.EdgeCurvaturesOf(points: pts, key: k)), OpenLength = Poly(key: 19, measure: static (pts, k) => CloudKernel.OpenLengthOf(points: pts, key: k));
    public static readonly VectorCloudMetric Covariance = Cluster(key: 20, measure: static (cluster, k) => CloudKernel.CovarianceOf(cluster: cluster, key: k).Map(static v => v.Cov)), PrincipalDirection = Cluster(key: 21, measure: static (cluster, k) => CloudKernel.PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => k.AcceptValue(value: CloudKernel.AsVector3d(v: stats.Eigen[0].Eigenvector)))), Spread = Cluster(key: 22, measure: static (cluster, k) => CloudKernel.PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => k.AcceptValue(value: stats.Spread))), OrientedNormals = Cluster(key: 23, measure: static (cloud, policy, k) => CloudKernel.OrientNormalsViaMst(cloud: cloud, policy: policy.Neighborhood, key: k)), PrincipalCurvature = Cluster(key: 24, measure: static (cluster, policy, k) => CloudKernel.PrincipalCurvaturesOf(cluster: cluster, policy: policy.Neighborhood, key: k)), Curvedness = Cluster(key: 25, measure: static (cluster, policy, k) => CloudKernel.CurvednessOf(cluster: cluster, policy: policy.Neighborhood, key: k)), ShapeIndex = Cluster(key: 26, measure: static (cluster, policy, k) => CloudKernel.ShapeIndexOf(cluster: cluster, policy: policy.Neighborhood, key: k));
    public static readonly VectorCloudMetric Admission = Cluster(key: 27, measure: static (cluster, k) => Fin.Succ(cluster.Admission)), Neighborhood = Cluster(key: 28, measure: static (cluster, policy, k) => CloudKernel.NeighborhoodReceiptOf(cluster: cluster, policy: policy.Neighborhood, key: k)), CurvatureReceipt = Cluster(key: 29, measure: static (cluster, policy, k) => CloudKernel.PrincipalCurvaturesOf(cluster: cluster, policy: policy.Neighborhood, key: k).Map(static result => result.Receipt));
    private static VectorCloudMetric Of<TCase, TOut>(int key, Func<VectorCloud, TCase?> adapt, Func<TCase, CloudMetricPolicy, Op, Fin<TOut>> measure) where TCase : class =>
        new(key: key, output: typeof(TOut), admitsCase: cloud => adapt(cloud) is not null,
            measure: (cloud, policy, k) => adapt(cloud) is TCase value ? measure(value, policy, k).Map(static v => (object)v!) : Fin.Fail<object>(k.InvalidInput()));
    private static VectorCloudMetric All<TOut>(int key, Func<VectorCloud, Op, Fin<TOut>> measure) => Of(key: key, adapt: static cloud => cloud, measure: (cloud, _, k) => measure(cloud, k));
    private static VectorCloudMetric Ring<TOut>(int key, Func<VectorCloud.RingCase, Op, Fin<TOut>> measure) => Of(key: key, adapt: static cloud => cloud as VectorCloud.RingCase, measure: (ring, _, k) => measure(ring, k));
    private static VectorCloudMetric Cases<TOut>(int key, Func<VectorCloud, bool> admitsCase, Func<VectorCloud, Op, Fin<TOut>> measure) => Of(key: key, adapt: cloud => admitsCase(cloud) ? cloud : null, measure: (cloud, _, k) => measure(cloud, k));
    private static VectorCloudMetric Poly<TOut>(int key, Func<Seq<Point3d>, Op, Fin<TOut>> measure) => Of(key: key, adapt: static cloud => cloud as VectorCloud.PolylineCase, measure: (poly, _, k) => measure(poly.Vertices, k));
    private static VectorCloudMetric Cluster<TOut>(int key, Func<VectorCloud.ClusterCase, Op, Fin<TOut>> measure) => Of(key: key, adapt: static cloud => cloud as VectorCloud.ClusterCase, measure: (cluster, _, k) => measure(cluster, k));
    private static VectorCloudMetric Cluster<TOut>(int key, Func<VectorCloud.ClusterCase, CloudMetricPolicy, Op, Fin<TOut>> measure) => Of(key: key, adapt: static cloud => cloud as VectorCloud.ClusterCase, measure: measure);
    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, CloudMetricPolicy policy, Op key);
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        from policy in CloudMetricPolicy.AdmitOrDefault(policy: None, key: key)
        from output in Project<TOut>(cloud: cloud, policy: policy, key: key)
        select output;
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, CloudMetricPolicy policy, Op key) =>
        (AdmitsCase(cloud: cloud), Output.Equals(typeof(TOut))) switch {
            (true, true) => Measure(cloud: cloud, policy: policy, key: key).Bind(value => value switch {
                Seq<Vector3d> vectors => ProjectSeq<Vector3d, TOut>(values: vectors, key: key),
                Seq<double> scalars => ProjectSeq<double, TOut>(values: scalars, key: key),
                Seq<Plane> planes => ProjectSeq<Plane, TOut>(values: planes, key: key),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
        };
    private static Fin<TOut> ProjectSeq<TItem, TOut>(Seq<TItem> values, Op key) =>
        values.TraverseM(value => key.AcceptValue(value: value)).As().Map(static valid => (TOut)(object)valid);
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionPolicy(bool Deduplicate, Option<PositiveMagnitude> Tolerance) {
    internal static CloudAdmissionPolicy Default => new(Deduplicate: true, Tolerance: None);
    internal double ToleranceValue => Tolerance.Match(Some: static value => value.Value, None: static () => 0.0);
    internal Fin<CloudAdmissionPolicy> Admit(Op key) {
        CloudAdmissionPolicy self = this;
        return self.Tolerance switch {
            { IsSome: true, Case: PositiveMagnitude active } => FieldNabla.Positive(value: active, key: key).Map(_ => self),
            _ => Fin.Succ(self),
        };
    }
    internal bool Equivalent(Point3d left, Point3d right) {
        Option<PositiveMagnitude> tolerance = Tolerance;
        return tolerance switch {
            { IsSome: true, Case: PositiveMagnitude active } => left.EpsilonEquals(other: right, epsilon: active.Value),
            _ => left == right,
        };
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudAdmissionReceipt(int InputCount, int OutputCount, int InputDuplicateCoordinateCount, int MergedCoordinateCount, double Tolerance, bool Deduplicated, Arr<int> OriginalToUnique, Option<double> MassInputTotal, Option<double> MassOutputTotal) : IValidityEvidence {
    internal const double ConservationEps = 1.0e-8;
    bool IValidityEvidence.IsValid => IsValid;
    internal static bool MassConserved(double inputTotal, double outputTotal) =>
        Math.Abs(value: inputTotal - outputTotal) <= ConservationEps * Math.Max(val1: 1.0, val2: Math.Abs(value: inputTotal));
    internal bool IsValid {
        get {
            int outputCount = OutputCount;
            Option<double> inputTotal = MassInputTotal, outputTotal = MassOutputTotal;
            bool inputValid = inputTotal.IsNone || (RhinoMath.IsValidDouble(x: inputTotal.IfNone(0.0)) && inputTotal.IfNone(0.0) > 0.0);
            bool outputValid = outputTotal.IsNone || (RhinoMath.IsValidDouble(x: outputTotal.IfNone(0.0)) && outputTotal.IfNone(0.0) > 0.0);
            bool conserved = inputTotal.IsNone || outputTotal.IsNone || MassConserved(inputTotal: inputTotal.IfNone(0.0), outputTotal: outputTotal.IfNone(0.0));
            return InputCount > 0
                && outputCount > 0
                && InputDuplicateCoordinateCount >= 0
                && MergedCoordinateCount >= 0
                && outputCount + MergedCoordinateCount == InputCount
                && Deduplicated == (MergedCoordinateCount > 0)
                && OriginalToUnique.Count == InputCount
                && OriginalToUnique.ForAll(index => index >= 0 && index < outputCount)
                && RhinoMath.IsValidDouble(x: Tolerance)
                && Tolerance >= 0.0
                && inputValid
                && outputValid
                && conserved;
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudNeighborhoodPolicy(Dimension NeighborCount, Option<PositiveMagnitude> Radius, PositiveMagnitude EigenGapTolerance, PositiveMagnitude FitResidualTolerance) {
    internal static Fin<CloudNeighborhoodPolicy> Default(Op key) =>
        from count in key.AcceptValidated<Dimension>(candidate: 10)
        from eigenGap in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8)
        from residual in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-4)
        select new CloudNeighborhoodPolicy(NeighborCount: count, Radius: None, EigenGapTolerance: eigenGap, FitResidualTolerance: residual);
    internal Fin<CloudNeighborhoodPolicy> Admit(Op key) {
        CloudNeighborhoodPolicy self = this;
        return from count in FieldNabla.Dimension(value: self.NeighborCount, key: key)
               from minimum in guard(self.NeighborCount.Value >= 3, key.InvalidInput())
               from radius in self.Radius.Match(Some: value => FieldNabla.Positive(value: value, key: key), None: static () => Fin.Succ(unit))
               from gap in FieldNabla.Positive(value: self.EigenGapTolerance, key: key)
               from residual in FieldNabla.Positive(value: self.FitResidualTolerance, key: key)
               select self;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudNeighborhoodReceipt(int InputCount, int QueryCount, int RequestedNeighborCount, CloudNeighborhoodSearchBackend SearchBackend, bool RadiusLimited, Option<double> Radius, bool NativeIndexRouted, bool SelfNeighborIncluded, int EmptyNeighborhoodCount, int OutOfRangeIndexCount, int DuplicateIndexCount, int DuplicateCoordinateCount, int MinReturnedCount, int MaxReturnedCount, double MeanReturnedCount) : IValidityEvidence {
    bool IValidityEvidence.IsValid => IsValid;
    internal bool IsValid {
        get {
            Option<double> radius = Radius;
            return SearchBackend is not null
                && InputCount > 0
                && QueryCount > 0
                && RequestedNeighborCount > 0
                && EmptyNeighborhoodCount >= 0 && OutOfRangeIndexCount >= 0 && DuplicateIndexCount >= 0 && DuplicateCoordinateCount >= 0 && MinReturnedCount >= 0 && MaxReturnedCount >= 0
                && QueryCount >= EmptyNeighborhoodCount
                && RequestedNeighborCount <= InputCount
                && OutOfRangeIndexCount == 0
                && DuplicateIndexCount == 0
                && MinReturnedCount <= MaxReturnedCount
                && RhinoMath.IsValidDouble(x: MeanReturnedCount)
                && MeanReturnedCount >= 0.0
                && (radius.IsNone || (RhinoMath.IsValidDouble(x: radius.IfNone(0.0)) && radius.IfNone(0.0) > 0.0))
                && RadiusLimited == radius.IsSome;
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudMetricPolicy(CloudNeighborhoodPolicy Neighborhood) {
    internal static Fin<CloudMetricPolicy> AdmitOrDefault(Option<CloudMetricPolicy> policy, Op key) =>
        policy.Match(
            Some: active => active.Admit(key: key),
            None: () => CloudNeighborhoodPolicy.Default(key: key).Map(static neighborhood => new CloudMetricPolicy(Neighborhood: neighborhood)));
    internal Fin<CloudMetricPolicy> Admit(Op key) =>
        Neighborhood.Admit(key: key).Map(static neighborhood => new CloudMetricPolicy(Neighborhood: neighborhood));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudNeighborhoodPcaReceipt(int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount, int RankClampCount, int EigenClampCount, double EigenClampFloor, CloudNeighborhoodReceipt Neighborhood) {
    internal bool IsValid =>
        InputCount >= 0 && RequestedNeighborCount >= 0 && AcceptedSampleCount >= 0 && RejectedSampleCount >= 0 && RankClampCount >= 0 && EigenClampCount >= 0
        && AcceptedSampleCount + RejectedSampleCount == InputCount
        && RankClampCount <= AcceptedSampleCount
        && EigenClampCount <= AcceptedSampleCount * 3
        && RhinoMath.IsValidDouble(x: EigenClampFloor)
        && EigenClampFloor > 0.0
        && Neighborhood.IsValid
        && Neighborhood.InputCount == InputCount
        && Neighborhood.RequestedNeighborCount == RequestedNeighborCount;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudNeighborhoodPcaResult(Seq<CloudNeighborhoodPcaSample> Samples, CloudNeighborhoodPcaReceipt Receipt) {
    internal bool IsValid =>
        Receipt.IsValid
        && Samples.Count == Receipt.AcceptedSampleCount
        && Samples.ForAll(static sample => sample.IsValid);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudNeighborhoodPcaSample(int Index, Point3d Point, int NeighborCount, SymmetricMatrix Covariance, Vector3d Normal, Arr<double> RawEigenvalues, Arr<double> ClampedEigenvalues, int Rank, int EigenClampCount) {
    internal bool IsValid =>
        Index >= 0
        && Point.IsValid
        && NeighborCount >= 3
        && Covariance.IsValid
        && Normal.IsValid
        && !Normal.IsTiny()
        && RawEigenvalues.Count == 3
        && ClampedEigenvalues.Count == 3
        && RawEigenvalues.ForAll(RhinoMath.IsValidDouble)
        && ClampedEigenvalues.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0)
        && Rank is >= 0 and <= 3
        && EigenClampCount is >= 0 and <= 3;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCurvatureRangeReceipt(int AcceptedSampleCount, CloudCurvatureRangeKind Kind, int PlaneLikeCount, int SphereLikeCount, int SaddleLikeCount, int MixedCount, double MinK1, double MaxK1, double MinK2, double MaxK2, double MinGaussian, double MaxGaussian, double MinMean, double MaxMean, double MinShapeIndex, double MaxShapeIndex, double Tolerance) {
    internal bool IsValid =>
        Kind is not null
        && AcceptedSampleCount >= 0 && PlaneLikeCount >= 0 && SphereLikeCount >= 0 && SaddleLikeCount >= 0 && MixedCount >= 0
        && PlaneLikeCount + SphereLikeCount + SaddleLikeCount + MixedCount == AcceptedSampleCount
        && RhinoMath.IsValidDouble(x: MinK1) && RhinoMath.IsValidDouble(x: MaxK1) && RhinoMath.IsValidDouble(x: MinK2) && RhinoMath.IsValidDouble(x: MaxK2) && RhinoMath.IsValidDouble(x: MinGaussian) && RhinoMath.IsValidDouble(x: MaxGaussian) && RhinoMath.IsValidDouble(x: MinMean) && RhinoMath.IsValidDouble(x: MaxMean) && RhinoMath.IsValidDouble(x: MinShapeIndex) && RhinoMath.IsValidDouble(x: MaxShapeIndex) && RhinoMath.IsValidDouble(x: Tolerance)
        && MinK1 <= MaxK1
        && MinK2 <= MaxK2
        && MinGaussian <= MaxGaussian
        && MinMean <= MaxMean
        && MinShapeIndex <= MaxShapeIndex
        && Tolerance > 0.0;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCurvatureReceipt(int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount, int RankRejectedCount, int ResidualRejectedCount, double MeanResidual, double MaxResidual, double EigenGapTolerance, double FitResidualTolerance, CloudNeighborhoodReceipt Neighborhood, CloudCurvatureRangeReceipt Range) : IValidityEvidence {
    public bool SelfNeighborIncluded => Neighborhood.SelfNeighborIncluded;
    public bool NativeIndexRouted => Neighborhood.NativeIndexRouted;
    public bool RadiusLimited => Neighborhood.RadiusLimited;
    public CloudNeighborhoodSearchBackend SearchBackend => Neighborhood.SearchBackend;
    bool IValidityEvidence.IsValid => IsValid;
    internal bool IsValid =>
        InputCount >= 0 && RequestedNeighborCount >= 0 && AcceptedSampleCount >= 0 && RejectedSampleCount >= 0 && RankRejectedCount >= 0 && ResidualRejectedCount >= 0
        && AcceptedSampleCount + RejectedSampleCount == InputCount
        && RankRejectedCount + ResidualRejectedCount == RejectedSampleCount
        && RhinoMath.IsValidDouble(x: MeanResidual) && RhinoMath.IsValidDouble(x: MaxResidual) && RhinoMath.IsValidDouble(x: EigenGapTolerance) && RhinoMath.IsValidDouble(x: FitResidualTolerance)
        && EigenGapTolerance > 0.0
        && FitResidualTolerance > 0.0
        && Neighborhood.IsValid
        && Range.IsValid
        && Neighborhood.RequestedNeighborCount == RequestedNeighborCount
        && Range.AcceptedSampleCount == AcceptedSampleCount;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCurvatureResult(Seq<CloudCurvatureSample> Samples, CloudCurvatureReceipt Receipt) : IValidityEvidence {
    bool IValidityEvidence.IsValid => IsValid;
    internal bool IsValid =>
        Receipt.IsValid
        && Samples.Count == Receipt.AcceptedSampleCount
        && Samples.ForAll(static sample => sample.IsValid);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCurvatureSample(int Index, Point3d Point, double K1, double K2, Direction E1, Direction E2, double Residual, int NeighborCount) {
    internal bool IsValid =>
        Index >= 0
        && Point.IsValid && E1.IsValid && E2.IsValid && RhinoMath.IsValidDouble(x: K1) && RhinoMath.IsValidDouble(x: K2) && RhinoMath.IsValidDouble(x: Residual)
        && Residual >= 0.0
        && NeighborCount >= 6;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullPolicy(PositiveMagnitude Tolerance, VectorAngle AngleTolerance) {
    internal static Fin<CloudHullPolicy> AdmitOrDefault(Option<CloudHullPolicy> policy, Context context, Op key) =>
        policy.Match(
            Some: active => active.Admit(key: key),
            None: () => from tolerance in key.AcceptValidated<PositiveMagnitude>(candidate: context.Absolute.Value)
                        from angle in key.AcceptValidated<VectorAngle>(candidate: context.Angle.Value)
                        select new CloudHullPolicy(Tolerance: tolerance, AngleTolerance: angle));
    internal Fin<CloudHullPolicy> Admit(Op key) {
        CloudHullPolicy self = this;
        return from tolerance in FieldNabla.Positive(value: self.Tolerance, key: key)
               from angle in key.AcceptValidated<VectorAngle>(candidate: self.AngleTolerance.Value)
               from range in guard(self.AngleTolerance.Value is > 0.0 and < Math.PI, key.InvalidInput())
               select self;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullReceipt(CloudHullKind Kind, CloudHullStatus Status, double Tolerance, double AngleTolerance, int InputCount, int OutputVertexCount, int NativeFacetCount, Option<double> PlanarityDeviation, bool CoplanarRejected, int ContainmentRejectedCount, bool NativeRouted, bool Fallback);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(Option<Mesh> Mesh, CloudHullReceipt Receipt) {
    internal static CloudHullResult Completed(CloudHullKind kind, CloudHullPolicy policy, int inputCount, Mesh mesh, Option<double> planarityDeviation, bool fallback, int nativeFacetCount = 0) =>
        new(Mesh: Some(mesh), Receipt: ReceiptOf(kind: kind, status: CloudHullStatus.Completed, policy: policy, inputCount: inputCount, outputVertexCount: mesh.Vertices.Count, nativeFacetCount: nativeFacetCount <= 0 ? mesh.Faces.Count : nativeFacetCount, planarityDeviation: planarityDeviation, coplanarRejected: false, containmentRejectedCount: 0, nativeRouted: true, fallback: fallback));
    internal static CloudHullResult Rejected(CloudHullKind kind, CloudHullPolicy policy, int inputCount, Option<double> planarityDeviation, bool coplanarRejected, int containmentRejectedCount, bool nativeRouted, bool fallback) =>
        new(Mesh: Option<Mesh>.None, Receipt: ReceiptOf(kind: kind, status: CloudHullStatus.Rejected, policy: policy, inputCount: inputCount, outputVertexCount: 0, nativeFacetCount: 0, planarityDeviation: planarityDeviation, coplanarRejected: coplanarRejected, containmentRejectedCount: containmentRejectedCount, nativeRouted: nativeRouted, fallback: fallback));
    internal static CloudHullResult Unsupported(CloudHullKind kind, CloudHullPolicy policy, int inputCount) =>
        new(Mesh: Option<Mesh>.None, Receipt: ReceiptOf(kind: kind, status: CloudHullStatus.Unsupported, policy: policy, inputCount: inputCount, outputVertexCount: 0, nativeFacetCount: 0, planarityDeviation: None, coplanarRejected: false, containmentRejectedCount: 0, nativeRouted: false, fallback: false));
    private static CloudHullReceipt ReceiptOf(CloudHullKind kind, CloudHullStatus status, CloudHullPolicy policy, int inputCount, int outputVertexCount, int nativeFacetCount, Option<double> planarityDeviation, bool coplanarRejected, int containmentRejectedCount, bool nativeRouted, bool fallback) =>
        new(Kind: kind, Status: status, Tolerance: policy.Tolerance.Value, AngleTolerance: policy.AngleTolerance.Value, InputCount: inputCount, OutputVertexCount: outputVertexCount, NativeFacetCount: nativeFacetCount, PlanarityDeviation: planarityDeviation, CoplanarRejected: coplanarRejected, ContainmentRejectedCount: containmentRejectedCount, NativeRouted: nativeRouted, Fallback: fallback);
    internal Fin<TOut> Project<TOut>(Context context, Op key) {
        CloudHullResult self = this;
        return AtomProjection.Rows<CloudHullResult, TOut>(self: self, key: key,
            new ProjectionRow(typeof(CloudHullReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(Mesh), () => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(Mesh))).Bind(mesh => key.AcceptValue(value: mesh).Map(static value => (object)value))),
            new ProjectionRow(typeof(VectorCloud), () => self.Mesh.ToFin(key.Unsupported(geometryType: typeof(CloudHullResult), outputType: typeof(VectorCloud))).Bind(mesh => VectorCloud.Cluster(
                points: toSeq(mesh.Vertices.AsIterable().Select(static vertex => (Point3d)vertex)),
                context: context,
                key: key).Map(static value => (object)value))));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudTransportPolicy(PositiveMagnitude Regularization, Dimension MaxIterations, bool Debiased, Option<PositiveMagnitude> MassRelaxation, PositiveMagnitude ConvergenceTolerance, PositiveMagnitude CouplingCutoff) {
    public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, bool debiased = false, double? massRelaxation = null, double? convergenceTolerance = null, double? couplingCutoff = null, Op? key = null) {
        Op op = key.OrDefault();
        return from reg in op.AcceptValidated<PositiveMagnitude>(candidate: regularization)
               from cap in op.AcceptValidated<Dimension>(candidate: maxIterations)
               from relax in massRelaxation is double lambda
                   ? op.AcceptValidated<PositiveMagnitude>(candidate: lambda).Map(Some)
                   : Fin.Succ(Option<PositiveMagnitude>.None)
               from tolerance in op.AcceptValidated<PositiveMagnitude>(candidate: convergenceTolerance ?? 1.0e-8)
               from cutoff in op.AcceptValidated<PositiveMagnitude>(candidate: couplingCutoff ?? 1.0e-8)
               from policy in new CloudTransportPolicy(Regularization: reg, MaxIterations: cap, Debiased: debiased, MassRelaxation: relax, ConvergenceTolerance: tolerance, CouplingCutoff: cutoff).Admit(key: op)
               select policy;
    }
    internal Fin<CloudTransportPolicy> Admit(Op key) {
        CloudTransportPolicy self = this;
        return from regularization in FieldNabla.Positive(value: self.Regularization, key: key)
               from iterations in FieldNabla.Dimension(value: self.MaxIterations, key: key)
               from tolerance in FieldNabla.Positive(value: self.ConvergenceTolerance, key: key)
               from cutoff in FieldNabla.Positive(value: self.CouplingCutoff, key: key)
               from relaxation in self.MassRelaxation.Match(Some: value => FieldNabla.Positive(value: value, key: key), None: static () => Fin.Succ(unit))
               select self;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondence(int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint, Vector3d Residual, double Distance, double SquaredDistance, Option<double> SourceMass, Option<double> TargetMass, Option<double> CouplingMass, Option<double> Confidence);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondenceSet(Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount, int NonZeroCount, double TotalMass, double Rmse, double MedianDistance, double MaxDistance, double Quantile90, double Quantile95, int CoveredSourceCount = 0, int CoveredTargetCount = 0, double RetainedSourceMass = 0.0, double RetainedTargetMass = 0.0) {
    public static CloudCorrespondenceSet Of(Seq<CloudCorrespondence> items, int sourceCount, int targetCount) {
        CloudCorrespondence[] ordered = [.. items.AsIterable()]; double totalMass = ordered.Sum(static item => item.CouplingMass.IfNone(item.SourceMass.IfNone(0.0))); double[] distances = [.. ordered.Select(static item => item.Distance).Order()];
        double denominator = totalMass > RhinoMath.ZeroTolerance ? totalMass : ordered.Length, squared = ordered.Sum(item => item.SquaredDistance * (totalMass > RhinoMath.ZeroTolerance ? item.CouplingMass.IfNone(item.SourceMass.IfNone(0.0)) : 1.0));
        return new CloudCorrespondenceSet(Items: items, SourceCount: sourceCount, TargetCount: targetCount, NonZeroCount: ordered.Length, TotalMass: totalMass, Rmse: ordered.Length > 0 ? Math.Sqrt(d: squared / denominator) : 0.0, MedianDistance: QuantileOrZero(sorted: distances, tau: 0.5), MaxDistance: distances.Length > 0 ? distances[^1] : 0.0, Quantile90: QuantileOrZero(sorted: distances, tau: 0.9), Quantile95: QuantileOrZero(sorted: distances, tau: 0.95), CoveredSourceCount: ordered.Select(static item => item.SourceIndex).Distinct().Count(), CoveredTargetCount: ordered.Select(static item => item.TargetIndex).Distinct().Count());
    }
    internal static CloudCorrespondenceSet Of(Seq<CloudCorrespondence> items, int sourceCount, int targetCount, double[] rowMass, double[] columnMass) {
        CloudCorrespondenceSet basic = Of(items: items, sourceCount: sourceCount, targetCount: targetCount);
        return basic with {
            CoveredSourceCount = rowMass.Count(static mass => mass > RhinoMath.ZeroTolerance),
            CoveredTargetCount = columnMass.Count(static mass => mass > RhinoMath.ZeroTolerance),
            RetainedSourceMass = rowMass.Sum(),
            RetainedTargetMass = columnMass.Sum(),
        };
    }
    private static double QuantileOrZero(double[] sorted, double tau) =>
        sorted.Length == 0 ? 0.0 : SortedArrayStatistics.Quantile(data: sorted, tau: tau);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance, double Regularization, Option<double> MassRelaxation, double ConvergenceTolerance, double CouplingCutoff, bool Debiased, SinkhornResidualKind ResidualKind, SinkhornNumericStatus NumericStatus, double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop, double CouplingMass, int NonZeroCouplings, Option<double> MinPositiveCoupling, Option<double> MaxCoupling, CloudCorrespondenceSet Correspondences);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCloudShape(Option<Vector3d> Normal, Option<double> SignedArea, Option<double> Area, Option<double> Perimeter, Option<double> EdgeAspect, Option<double> Skewness, Option<double> PlanarityDeviation, Option<double> Compactness, Option<double> MomentAnisotropy, Option<Vector3d> RadiiOfGyration, Option<double> AreaError, Option<Vector3d> CentroidError, Option<Plane> BestFitPlane, Option<bool> Convex, Option<CurveOrientation> Orientation, Option<double> OpenLength, Option<Vector3d> Spread, Point3d Centroid, Plane PrincipalFrame, Seq<(double Moment, Vector3d Axis)> PrincipalAxes) : IValidityEvidence {
    internal VectorCloudShape(Point3d centroid, Plane principalFrame, Seq<(double Moment, Vector3d Axis)> principalAxes)
        : this(Normal: None, SignedArea: None, Area: None, Perimeter: None, EdgeAspect: None, Skewness: None, PlanarityDeviation: None, Compactness: None, MomentAnisotropy: None, RadiiOfGyration: None, AreaError: None, CentroidError: None, BestFitPlane: None, Convex: None, Orientation: None, OpenLength: None, Spread: None, Centroid: centroid, PrincipalFrame: principalFrame, PrincipalAxes: principalAxes) { }
    bool IValidityEvidence.IsValid => IsValid;
    internal bool IsValid =>
        Centroid.IsValid
        && PrincipalFrame.IsValid
        && PrincipalAxes.ForAll(static axis => OpAcceptance.ValidityOf(source: axis).IfNone(noneValue: false))
        && Normal.Map(static v => v.IsValid && !v.IsTiny()).IfNone(noneValue: true)
        && SignedArea.Map(static v => RhinoMath.IsValidDouble(x: v)).IfNone(noneValue: true)
        && Area.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(noneValue: true)
        && Perimeter.Map(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0).IfNone(noneValue: true)
        && EdgeAspect.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(noneValue: true)
        && Skewness.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(noneValue: true)
        && PlanarityDeviation.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(noneValue: true)
        && Compactness.Map(static v => RhinoMath.IsValidDouble(x: v) && v is >= 0.0 and <= 1.0).IfNone(noneValue: true)
        && MomentAnisotropy.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(noneValue: true)
        && RadiiOfGyration.Map(static v => v.IsValid).IfNone(noneValue: true)
        && AreaError.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(noneValue: true)
        && CentroidError.Map(static v => v.IsValid).IfNone(noneValue: true)
        && BestFitPlane.Map(static v => v.IsValid).IfNone(noneValue: true)
        && Orientation.Map(static v => v is CurveOrientation.Clockwise or CurveOrientation.CounterClockwise).IfNone(noneValue: true)
        && OpenLength.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(noneValue: true)
        && Spread.Map(static v => v.IsValid).IfNone(noneValue: true);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    internal static Fin<(Seq<Point3d> Points, Option<Arr<double>> Mass, CloudAdmissionReceipt Receipt)> AdmitCluster(Seq<Point3d> points, Option<Arr<double>> mass, CloudAdmissionPolicy policy, Op key) {
        Point3d[] source = [.. points.AsIterable()];
        double[] inputMass = mass.Match(Some: values => values.AsIterable().ToArray(), None: () => [.. Enumerable.Repeat(element: 1.0, count: source.Length)]);
        if (FieldNabla.PositiveFiniteWeights(weights: inputMass, count: source.Length, key: key).IsFail) return Fin.Fail<(Seq<Point3d>, Option<Arr<double>>, CloudAdmissionReceipt)>(key.InvalidInput());
        List<Point3d> unique = [];
        List<double> outputMass = [];
        int[] map = new int[source.Length];
        for (int i = 0; i < source.Length; i++) {
            int target = policy.Deduplicate ? unique.FindIndex(point => policy.Equivalent(left: point, right: source[i])) : -1;
            if (target < 0) {
                target = unique.Count;
                unique.Add(item: source[i]);
                outputMass.Add(item: 0.0);
            }
            map[i] = target;
            outputMass[target] += inputMass[i];
        }
        double inputTotal = inputMass.Sum();
        double outputTotal = outputMass.Sum();
        if (!RhinoMath.IsValidDouble(x: inputTotal) || inputTotal <= RhinoMath.ZeroTolerance || !CloudAdmissionReceipt.MassConserved(inputTotal: inputTotal, outputTotal: outputTotal)) return Fin.Fail<(Seq<Point3d>, Option<Arr<double>>, CloudAdmissionReceipt)>(key.InvalidResult());
        Arr<double> normalized = new([.. outputMass.Select(value => value / outputTotal)]);
        bool carriesMass = mass.IsSome || unique.Count != source.Length;
        int inputDuplicateCoordinates = DuplicateCoordinateCount(points: source);
        int mergedCoordinates = source.Length - unique.Count;
        CloudAdmissionReceipt receipt = new(InputCount: source.Length, OutputCount: unique.Count, InputDuplicateCoordinateCount: inputDuplicateCoordinates, MergedCoordinateCount: mergedCoordinates, Tolerance: policy.ToleranceValue, Deduplicated: policy.Deduplicate && mergedCoordinates != 0, OriginalToUnique: new Arr<int>(map), MassInputTotal: Some(inputTotal), MassOutputTotal: Some(outputTotal));
        return receipt.IsValid
            ? Fin.Succ((Points: toSeq(unique), Mass: carriesMass ? Some(normalized) : Option<Arr<double>>.None, Receipt: receipt))
            : Fin.Fail<(Seq<Point3d>, Option<Arr<double>>, CloudAdmissionReceipt)>(key.InvalidResult());
    }
    internal static Fin<Arr<double>> MassOf(VectorCloud.ClusterCase cluster, Op key) =>
        MassOf(mass: cluster.Mass.IfNone(new Arr<double>([.. Enumerable.Repeat(element: 1.0 / cluster.Vertices.Count, count: cluster.Vertices.Count)])), count: cluster.Vertices.Count, key: key);
    internal static Fin<Arr<double>> MassOf(Arr<double> mass, int count, Op key) =>
        from _ in FieldNabla.PositiveFiniteWeights(weights: [.. mass.AsIterable()], count: count, key: key)
        from total in mass.Fold(initialState: 0.0, f: static (sum, value) => sum + value) switch {
            double sum when RhinoMath.IsValidDouble(x: sum) && sum > RhinoMath.ZeroTolerance => Fin.Succ(sum),
            _ => Fin.Fail<double>(key.InvalidInput()),
        }
        select new Arr<double>([.. mass.AsIterable().Select(value => value / total)]);
    private static int DuplicateCoordinateCount(Point3d[] points) =>
        points.Length - points.GroupBy(static point => point).Count();
    // --- [COVARIANCE] -------------------------------------------------------------------
    internal static Fin<(Arr<double> Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Arr<double>> points, Dimension dimension, Op key) =>
        SampleMoment.Of(rows: points, dimension: dimension.Value, key: key)
            .Bind(moment => SymmetricMatrix.Of(dim: dimension, upper: moment.UpperCovariance, key: key)
                .Map(cov => (moment.Mean, Cov: cov)));
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, mass: None, key: key);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster, Op key) =>
        from mass in MassOf(cluster: cluster, key: key) from stats in CovarianceOf(points: cluster.Vertices, mass: Some(mass), key: key) select stats;
    private static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Option<Arr<double>> mass, Op key) =>
        from moment in SampleMoment.Of(
            rows: points.Map(static point => new Arr<double>([point.X, point.Y, point.Z])),
            dimension: 3,
            key: key,
            weights: mass.IsSome ? mass : Some(new Arr<double>([.. Enumerable.Repeat(element: 1.0, count: points.Count)])))
        from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: moment.Dimension), upper: moment.UpperCovariance, key: key)
        select (Mean: AsVector3d(v: moment.Mean), Cov: cov);
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);
    internal sealed record PrincipalStats(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen) {
        internal Seq<(double Moment, Vector3d Axis)> Axes => Eigen.Map(static pair => (Moment: pair.Eigenvalue, Axis: AsVector3d(v: pair.Eigenvector)));
        internal Vector3d Spread => new(Eigen[0].Eigenvalue, Eigen[1].Eigenvalue, Eigen[2].Eigenvalue);
    }
    private static Fin<PrincipalStats> PrincipalStatsOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, key: key).Bind(stats => stats.Cov.DecomposeEigen(key: key).Bind(eigen => PrincipalStatsOf(mean: stats.Mean, eigen: eigen, key: key)));
    internal static Fin<PrincipalStats> PrincipalStatsOf(VectorCloud.ClusterCase cluster, Op key) =>
        CovarianceOf(cluster: cluster, key: key).Bind(stats => stats.Cov.DecomposeEigen(key: key).Bind(eigen => PrincipalStatsOf(mean: stats.Mean, eigen: eigen, key: key)));
    private static Fin<PrincipalStats> PrincipalStatsOf(Vector3d mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, Op key) =>
        eigen.Count >= 3 ? Fin.Succ(new PrincipalStats(Mean: mean, Eigen: eigen)) : Fin.Fail<PrincipalStats>(key.InvalidResult());
    private static Fin<Plane> PrincipalFrameOf(PrincipalStats stats, Context context, Op key) =>
        VectorFrame.Of(origin: (Point3d)stats.Mean, normal: AsVector3d(v: stats.Eigen[2].Eigenvector), xHint: Some(AsVector3d(v: stats.Eigen[0].Eigenvector)), context: context, key: key)
            .Bind(frame => frame.Project<Plane>(key: key));

    // --- [BISHOP] -----------------------------------------------------------------------
    internal static Fin<Seq<Plane>> BishopFramesOf(VectorCloud cloud, Op key) =>
        cloud.Switch(
            state: key,
            ringCase: static (k, ring) => RingNormalOf(ring: ring, key: k).Bind(normal => Direction.Of(value: normal, context: ring.Tolerance, key: k)).Bind(initialNormal => BishopChainOf(points: ring.Vertices, initialNormal: initialNormal, closed: true, context: ring.Tolerance, key: k)),
            polylineCase: static (k, polyline) => polyline.Vertices.Count < 2
                ? Fin.Fail<Seq<Plane>>(k.InvalidInput())
                : Direction.Of(value: VectorFrame.SeedPerpendicular(axis: polyline.Vertices[1] - polyline.Vertices[0]), context: polyline.Tolerance, key: k).Bind(initialNormal => BishopChainOf(points: polyline.Vertices, initialNormal: initialNormal, closed: false, context: polyline.Tolerance, key: k)),
            clusterCase: static (k, c) => Fin.Fail<Seq<Plane>>(k.Unsupported(geometryType: c.GetType(), outputType: typeof(Seq<Plane>))));
    internal static Fin<Seq<Plane>> BishopChainOf(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op key) =>
        points.Count switch {
            < 2 => Fin.Fail<Seq<Plane>>(key.InvalidInput()),
            _ => InitialBishop(points: points, initialNormal: initialNormal, context: context, key: key)
                .Bind(initial => toSeq(Enumerable.Range(start: 1, count: points.Count - 1)).Fold(initialState: Fin.Succ((Frames: Seq(initial.Frame), initial.R, initial.T)), f: (acc, i) => acc.Bind(state => StepBishop(state: state, points: points, index: i, context: context, key: key))))
                .Bind(result => closed ? RedistributeClosureTwist(frames: result.Frames, key: key) : Fin.Succ(result.Frames)),
        };
    private static Fin<(Plane Frame, Vector3d R, Vector3d T)> InitialBishop(Seq<Point3d> points, Direction initialNormal, Context context, Op key) {
        Vector3d t0 = points[1] - points[0];
        bool unitized = t0.Unitize();
        Vector3d r = initialNormal.Value - (initialNormal.Value * t0 * t0);
        _ = r.Unitize();
        return (unitized, r.IsValid && !r.IsTiny()) switch {
            (true, true) => VectorFrame.Of(origin: points[0], normal: t0, xHint: Some(r), context: context, key: key)
                .Map(frame => (Frame: frame.Value, R: r, T: t0)),
            (false, _) => Fin.Fail<(Plane, Vector3d, Vector3d)>(key.InvalidInput()),
            _ => Fin.Fail<(Plane, Vector3d, Vector3d)>(key.InvalidResult()),
        };
    }
    private static Fin<(Seq<Plane> Frames, Vector3d R, Vector3d T)> StepBishop(
        (Seq<Plane> Frames, Vector3d R, Vector3d T) state, Seq<Point3d> points, int index, Context context, Op key) {
        Vector3d tCurrRaw = index < points.Count - 1 ? points[index + 1] - points[index] : state.T;
        return tCurrRaw.Unitize() switch {
            false when index < points.Count - 1 => Fin.Fail<(Seq<Plane>, Vector3d, Vector3d)>(key.InvalidInput()),
            _ => DoubleReflect(rPrev: state.R, tPrev: state.T, tCurr: tCurrRaw) switch {
                Vector3d rNew when rNew.IsValid && !rNew.IsTiny() => VectorFrame.Of(origin: points[index], normal: tCurrRaw, xHint: Some(rNew), context: context, key: key).Map(frame => (Frames: state.Frames.Add(frame.Value), R: rNew, T: tCurrRaw)),
                _ => Fin.Fail<(Seq<Plane>, Vector3d, Vector3d)>(key.InvalidResult()),
            },
        };
    }
    internal static Vector3d DoubleReflect(Vector3d rPrev, Vector3d tPrev, Vector3d tCurr) =>
        (tCurr - tPrev) switch {
            Vector3d v1 when v1.IsTiny() => rPrev,
            Vector3d v1 => (RL: ReflectAcross(value: rPrev, axis: v1), TL: ReflectAcross(value: tPrev, axis: v1)) switch { (Vector3d, Vector3d) step => ReflectAcross(value: step.RL, axis: tCurr - step.TL) },
        };
    private static Vector3d ReflectAcross(Vector3d value, Vector3d axis) =>
        axis.IsTiny() ? value : value - (2.0 / (axis * axis) * (axis * value) * axis);
    private static Fin<Seq<Plane>> RedistributeClosureTwist(Seq<Plane> frames, Op key) {
        Plane last = frames[frames.Count - 1];
        Vector3d xRef = frames[0].XAxis - (frames[0].XAxis * last.ZAxis * last.ZAxis);
        _ = xRef.Unitize();
        double residual = Vector3d.VectorAngle(v1: last.XAxis, v2: xRef, vNormal: last.ZAxis);
        int count = frames.Count;
        return frames.Count switch {
            < 2 => Fin.Succ(frames),
            _ => frames.Map((p, i) => {
                Plane rotated = p;
                _ = rotated.Rotate(angle: -residual * i / count, axis: rotated.ZAxis);
                return rotated;
            }).TraverseM(p => key.AcceptValue(value: p)).As(),
        };
    }

    // --- [WINDING] ----------------------------------------------------------------------
    internal static Fin<int> PlanarWindingOf(Seq<Point3d> ring, Vector3d planeNormal, Point3d query, Op key) =>
        ring.Count switch {
            < 3 => Fin.Fail<int>(key.InvalidInput()),
            _ => key.AcceptValue(value: (int)Math.Round(ring.Map((v, i) => (V0: v - query, V1: ring[(i + 1) % ring.Count] - query))
                .Fold(initialState: 0.0, f: (sum, pair) => sum + Vector3d.VectorAngle(v1: pair.V0, v2: pair.V1, vNormal: planeNormal)) / RhinoMath.TwoPI, MidpointRounding.ToEven)),
        };
    internal static Fin<TOut> Winding<TOut>(VectorCloud cloud, Point3d query, Op key) =>
        cloud switch {
            VectorCloud.RingCase ring =>
                from normal in RingNormalOf(ring: ring, key: key)
                from winding in PlanarWindingOf(ring: ring.Vertices, planeNormal: normal, query: query, key: key)
                from output in AtomProjection.Self<int, TOut>(value: winding, key: key, owner: typeof(VectorCloud.RingCase))
                select output,
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(int))),
        };
    // --- [SHAPE_QUERIES] ----------------------------------------------------------------
    private static Fin<T> CloudCases<T>(VectorCloud cloud, Op key, Func<VectorCloud.RingCase, Op, Fin<T>> ring, Func<VectorCloud.PolylineCase, Op, Fin<T>> polyline, Func<VectorCloud.ClusterCase, Op, Fin<T>> cluster) =>
        cloud switch { VectorCloud.RingCase c => ring(arg1: c, arg2: key), VectorCloud.PolylineCase c => polyline(arg1: c, arg2: key), VectorCloud.ClusterCase c => cluster(arg1: c, arg2: key), _ => Fin.Fail<T>(key.InvalidInput()) };
    internal static Fin<Point3d> CentroidOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key,
            ring: static (ring, k) => WithMassProperties(ring: ring, project: static (op, props) => op.AcceptValue(value: props.Centroid), key: k),
            polyline: static (polyline, k) => CovarianceOf(points: polyline.Vertices, key: k).Bind(stats => k.AcceptValue(value: (Point3d)stats.Mean)),
            cluster: static (cluster, k) => CovarianceOf(cluster: cluster, key: k).Bind(stats => k.AcceptValue(value: (Point3d)stats.Mean)));
    internal static Fin<Plane> BestFitPlaneOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key,
            ring: static (ring, k) => BestFitOf(points: ring.Vertices, key: k).Map(static fit => fit.Plane),
            polyline: static (polyline, k) => BestFitOf(points: polyline.Vertices, key: k).Map(static fit => fit.Plane),
            cluster: static (cluster, k) => BestFitOf(points: cluster.Vertices, key: k).Map(static fit => fit.Plane));
    internal static Fin<Seq<Vector3d>> PrincipalAxesOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key,
            ring: static (ring, k) => WithMassProperties(ring: ring, project: static (op, props) => AxesOf(mass: props, key: op).Bind(axes => AxisVectorsOf(axes: axes, key: op)), key: k),
            polyline: static (polyline, k) => PrincipalStatsOf(points: polyline.Vertices, key: k).Bind(stats => AxisVectorsOf(axes: stats.Axes, key: k)),
            cluster: static (cluster, k) => PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => AxisVectorsOf(axes: stats.Axes, key: k)));
    internal static Fin<Plane> PrincipalFrameOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key,
            ring: static (ring, k) => WithRingCurve(ring: ring, project: static (state, curve) =>
                from oriented in RingOrientationOf(curve: curve, context: state.Context, key: state.Key)
                from frame in WithMassPropertiesInternal(curve: curve, context: state.Context, project: static ((Context Context, Op Key, Vector3d Normal) s, AreaMassProperties mass) =>
                    from axes in AxesOf(mass: mass, key: s.Key)
                    from principal in RingPrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: s.Normal, context: s.Context, key: s.Key)
                    select principal, state: (state.Context, state.Key, oriented.Normal), key: state.Key)
                select frame, key: k),
            polyline: static (polyline, k) => PrincipalStatsOf(points: polyline.Vertices, key: k).Bind(stats => PrincipalFrameOf(stats: stats, context: polyline.Tolerance, key: k)),
            cluster: static (cluster, k) => PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => PrincipalFrameOf(stats: stats, context: cluster.Tolerance, key: k)));
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key, ring: static (ring, k) => RingShapeOf(ring: ring, key: k),
            polyline: static (polyline, k) => PointSetShapeOf(points: polyline.Vertices, context: polyline.Tolerance, forPolyline: true, principalStats: PrincipalStatsOf(points: polyline.Vertices, key: k), key: k),
            cluster: static (cluster, k) => PointSetShapeOf(points: cluster.Vertices, context: cluster.Tolerance, forPolyline: false, principalStats: PrincipalStatsOf(cluster: cluster, key: k), key: k));
    // --- [RING_METRICS] -----------------------------------------------------------------
    internal static Fin<Vector3d> RingNormalOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            RingOrientationOf(curve: curve, context: state.Context, key: state.Key).Map(static o => o.Normal), key: key);
    internal static Fin<double> RingCompactnessOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) => WithMassPropertiesInternal(
            curve: curve, context: state.Context,
            project: static (s, props) => CompactnessOf(area: props.Area, perimeter: s.Native.Length, key: s.Key),
            state: state, key: state.Key), key: key);
    internal static Fin<double> RingMomentAnisotropyOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            from oriented in RingOrientationOf(curve: curve, context: state.Context, key: state.Key)
            from value in WithMassPropertiesInternal(curve: curve, context: state.Context, project: static ((Context Context, Op Key, Vector3d Normal) s, AreaMassProperties mass) =>
                from axes in AxesOf(mass: mass, key: s.Key)
                from anisotropy in MomentAnisotropyOf(axes: axes, normal: s.Normal, context: s.Context, key: s.Key)
                select anisotropy, state: (state.Context, state.Key, oriented.Normal), key: state.Key)
            select value, key: key);
    internal static Fin<double> RingSkewnessOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            from oriented in RingOrientationOf(curve: curve, context: state.Context, key: state.Key)
            from skewness in SkewnessOf(points: state.Vertices, normal: oriented.Normal, key: state.Key)
            select skewness, key: key);
    private static Fin<VectorCloudShape> RingShapeOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) => WithMassPropertiesInternal(
            curve: curve, context: state.Context,
            project: static (s, mass) =>
                from oriented in RingOrientationOf(curve: s.Curve, context: s.Context, key: s.Key)
                from fit in BestFitOf(points: s.Vertices, key: s.Key)
                from edgeAspect in EdgeAspectOf(native: s.Native, context: s.Context, key: s.Key)
                from skewness in SkewnessOf(points: s.Vertices, normal: oriented.Normal, key: s.Key)
                from axes in AxesOf(mass: mass, key: s.Key)
                from compactness in CompactnessOf(area: mass.Area, perimeter: s.Native.Length, key: s.Key)
                from anisotropy in MomentAnisotropyOf(axes: axes, normal: oriented.Normal, context: s.Context, key: s.Key)
                from radii in s.Key.AcceptValue(value: mass.CentroidCoordinatesRadiiOfGyration)
                from areaError in s.Key.AcceptValue(value: mass.AreaError)
                from centroidError in s.Key.AcceptValue(value: mass.CentroidError)
                from principal in RingPrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: oriented.Normal, context: s.Context, key: s.Key)
                let shape = new VectorCloudShape(Normal: Some(oriented.Normal), SignedArea: Some(oriented.Orientation == CurveOrientation.CounterClockwise ? mass.Area : -mass.Area), Area: Some(mass.Area), Perimeter: Some(s.Native.Length), EdgeAspect: Some(edgeAspect), Skewness: Some(skewness), PlanarityDeviation: Some(fit.Deviation), Compactness: Some(compactness), MomentAnisotropy: Some(anisotropy), RadiiOfGyration: Some(radii), AreaError: Some(areaError), CentroidError: Some(centroidError), BestFitPlane: Some(fit.Plane), Convex: Some(s.Native.IsConvexLoop(strictlyConvex: false)), Orientation: Some(oriented.Orientation), OpenLength: None, Spread: None, Centroid: mass.Centroid, PrincipalFrame: principal, PrincipalAxes: axes)
                from valid in ValidShape(shape: shape, key: s.Key)
                select valid,
            state: (state.Vertices, state.Native, state.Context, state.Key, Curve: curve),
            key: state.Key), key: key);
    private static Fin<VectorCloudShape> PointSetShapeOf(Seq<Point3d> points, Context context, bool forPolyline, Fin<PrincipalStats> principalStats, Op key) =>
        from openLen in forPolyline ? OpenLengthOf(points: points, key: key).Map(Some) : Fin.Succ(Option<double>.None)
        from fit in forPolyline
            ? BestFitOf(points: points, key: key).Map(static f => (Plane: Some(f.Plane), Deviation: Some(f.Deviation)))
            : Fin.Succ((Plane: Option<Plane>.None, Deviation: Option<double>.None))
        from stats in principalStats
        from principal in PrincipalFrameOf(stats: stats, context: context, key: key)
        let shape = new VectorCloudShape(centroid: (Point3d)stats.Mean, principalFrame: principal, principalAxes: stats.Axes) with { PlanarityDeviation = fit.Deviation, BestFitPlane = fit.Plane, OpenLength = openLen, Spread = Some(stats.Spread) }
        from valid in ValidShape(shape: shape, key: key)
        select valid;
    private static Fin<VectorCloudShape> ValidShape(VectorCloudShape shape, Op key) =>
        shape.IsValid ? Fin.Succ(shape) : Fin.Fail<VectorCloudShape>(key.InvalidResult());
    private static Fin<(Plane Plane, double Deviation)> BestFitOf(Seq<Point3d> points, Op key) =>
        (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
            (PlaneFitResult.Success, { IsValid: true } valid) => from acceptedPlane in key.AcceptValue(value: valid) from acceptedDeviation in key.AcceptValue(value: deviation) select (Plane: acceptedPlane, Deviation: acceptedDeviation),
            _ => Fin.Fail<(Plane Plane, double Deviation)>(error: key.InvalidResult()),
        };
    internal static Fin<double> CompactnessOf(double area, double perimeter, Op key) =>
        from validArea in key.AcceptValue(value: area) from validPerimeter in key.AcceptValue(value: perimeter) from compactness in validPerimeter > RhinoMath.ZeroTolerance ? key.AcceptValue(value: 4.0 * Math.PI * validArea / (validPerimeter * validPerimeter)) : Fin.Fail<double>(error: key.InvalidResult()) select compactness;
    internal static Fin<double> EdgeAspectOf(Polyline native, Context context, Op key) {
        double tolerance = context.Absolute.Value;
        return Optional(native.GetSegments()).ToFin(key.InvalidResult()).Bind(segments => toSeq(segments).Map(static segment => segment.Length) switch {
            Seq<double> lengths when !lengths.IsEmpty && lengths.ForAll(length => RhinoMath.IsValidDouble(x: length) && length > tolerance) =>
                lengths.Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0), f: static (range, length) => (Min: Math.Min(val1: range.Min, val2: length), Max: Math.Max(val1: range.Max, val2: length))) switch {
                    (Min: double min, Max: double max) when min > tolerance && max >= min => key.AcceptValue(value: max / min),
                    _ => Fin.Fail<double>(error: key.InvalidResult()),
                },
            _ => Fin.Fail<double>(error: key.InvalidResult()),
        });
    }
    private static Fin<double> SkewnessOf(Seq<Point3d> points, Vector3d normal, Op key) =>
        points.Count switch {
            int count when count >= 3 => points.Map((point, index) => (
                    Previous: points[(index + count - 1) % count] - point,
                    Next: points[(index + 1) % count] - point,
                    Normal: normal))
                .Map(static vectors => Vector3d.VectorAngle(a: vectors.Previous, b: vectors.Next) switch {
                    double angle when Vector3d.CrossProduct(a: vectors.Previous, b: vectors.Next) * vectors.Normal > 0.0 => RhinoMath.TwoPI - angle,
                    double angle => angle,
                })
                .Fold(initialState: Fin.Succ((Max: 0.0, Ideal: (count - 2) * Math.PI / count, Key: key)), f: static (state, angle) => state.Bind(s => s.Key.AcceptValidated<VectorAngle>(candidate: angle)
                    .Map(a => (Max: Math.Max(val1: s.Max, val2: Math.Max(val1: (a.Value - s.Ideal) / (Math.PI - s.Ideal), val2: (s.Ideal - a.Value) / s.Ideal)), s.Ideal, s.Key))))
                .Map(static state => state.Max),
            _ => Fin.Fail<double>(error: key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> AxesOf(AreaMassProperties mass, Op key) =>
        (mass.CentroidCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis), Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))) switch {
            (true, Seq<(double Moment, Vector3d Axis)> axes) => axes.TraverseM(axis => key.AcceptValue(value: axis)).As(),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(error: key.InvalidResult()),
        };
    private static Fin<Seq<Vector3d>> AxisVectorsOf(Seq<(double Moment, Vector3d Axis)> axes, Op key) =>
        axes.Map(static axis => axis.Axis).TraverseM(axis => key.AcceptValue(value: axis)).As();
    private static Fin<double> MomentAnisotropyOf(Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        InPlaneAxesOf(axes: axes, normal: normal, context: context, key: key)
            .Bind(chosen => chosen switch {
                Seq<(double Moment, Vector3d Axis)> value when value.Count == 2 => from a in key.AcceptValue(value: value[0].Moment) from b in key.AcceptValue(value: value[1].Moment) from _ in guard(a >= 0.0 && b >= 0.0, key.InvalidResult()) let ratio = Math.Max(val1: a, val2: b) / Math.Max(val1: RhinoMath.ZeroTolerance, val2: Math.Min(val1: a, val2: b)) from anisotropy in ratio >= 1.0 ? key.AcceptValue(value: ratio) : Fin.Fail<double>(error: key.InvalidResult()) select anisotropy,
                _ => Fin.Fail<double>(error: key.InvalidResult()),
            });
    private static Fin<Plane> RingPrincipalFrameOf(Point3d centroid, Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        InPlaneAxesOf(axes: axes, normal: normal, context: context, key: key)
            .Bind(chosen => chosen switch {
                Seq<(double Moment, Vector3d Axis)> value when value.Count == 2 => VectorFrame.Of(origin: centroid, normal: normal, xHint: Some(value[0].Axis), context: context, key: key).Bind(frame => frame.Project<Plane>(key: key)),
                _ => Fin.Fail<Plane>(error: key.InvalidResult()),
            });
    private static Fin<Seq<(double Moment, Vector3d Axis)>> InPlaneAxesOf(Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        axes.TraverseM(axis => Direction.Of(value: axis.Axis, context: context, key: key).Map(direction => (axis.Moment, Axis: direction.Value, Score: Math.Abs(direction.Value * normal)))).As()
            .Bind(valid => toSeq(valid.AsIterable().OrderBy(static axis => axis.Score)).Take(2) switch {
                Seq<(double Moment, Vector3d Axis, double Score)> chosen when chosen.Count == 2 =>
                    chosen.Map(static axis => (axis.Moment, axis.Axis)).TraverseM(axis => key.AcceptValue(value: axis)).As(),
                _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(error: key.InvalidResult()),
            });
    private static Fin<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)> RingOrientationOf(PolylineCurve curve, Context context, Op key) =>
        (curve.TryGetPlane(plane: out Plane plane, tolerance: context.Absolute.Value), plane) switch {
            (true, { IsValid: true } frame) => curve.ClosedCurveOrientation(plane: frame) switch {
                CurveOrientation.Clockwise => Direction.Of(value: -frame.Normal, context: context, key: key).Map(normal => (Plane: frame, Normal: normal.Value, Orientation: CurveOrientation.Clockwise)),
                CurveOrientation.CounterClockwise => Direction.Of(value: frame.Normal, context: context, key: key).Map(normal => (Plane: frame, Normal: normal.Value, Orientation: CurveOrientation.CounterClockwise)),
                _ => Fin.Fail<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)>(key.InvalidResult()),
            },
            _ => Fin.Fail<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)>(key.InvalidResult()),
        };
    private static Fin<TResult> WithRingCurve<TResult>(VectorCloud.RingCase ring, Func<(Seq<Point3d> Vertices, Polyline Native, Context Context, Op Key), PolylineCurve, Fin<TResult>> project, Op key) =>
        Optional(ring.Native.ToPolylineCurve()).ToFin(key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (ring.Vertices, ring.Native, Context: ring.Tolerance, Key: key), project: project));
    internal static Fin<TResult> WithMassPropertiesInternal<TState, TResult>(PolylineCurve curve, Context context, Func<TState, AreaMassProperties, Fin<TResult>> project, TState state, Op key) =>
        Optional(AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: context.Absolute.Value)).ToFin(key.InvalidResult())
            .Bind(props => new Lease<AreaMassProperties>.Owned(Value: props).Use(state: state, project: project));
    internal static Fin<TResult> WithMassProperties<TResult>(VectorCloud.RingCase ring, Func<Op, AreaMassProperties, Fin<TResult>> project, Op key) =>
        WithRingCurve(ring: ring, project: (state, curve) => WithMassPropertiesInternal(
            curve: curve, context: state.Context, project: project, state: state.Key, key: state.Key), key: key);

    // --- [POLYLINE_METRICS] -------------------------------------------------------------
    internal static Fin<Seq<Vector3d>> TangentFlowOf(Seq<Point3d> points, Op key) =>
        points.Count switch {
            < 2 => Fin.Fail<Seq<Vector3d>>(key.InvalidInput()),
            _ => toSeq(Enumerable.Range(start: 0, count: points.Count - 1))
                .Map(i => (points[i + 1] - points[i]) switch {
                    Vector3d raw when raw.Unitize() && raw.IsValid && !raw.IsTiny() => Fin.Succ(raw),
                    _ => Fin.Fail<Vector3d>(key.InvalidResult()),
                })
                .TraverseM(t => t.Bind(value => key.AcceptValue(value: value))).As(),
        };
    internal static Fin<Seq<double>> CumulativeArcLengthOf(Seq<Point3d> points, Op key) =>
        SegmentLengthsOf(points: points, key: key).Map(static lengths => lengths.Fold(initialState: (Trail: Seq(0.0), Cumulative: 0.0), f: static (state, length) => (Trail: state.Trail.Add(state.Cumulative + length), Cumulative: state.Cumulative + length)).Trail);
    internal static Fin<Seq<double>> EdgeCurvaturesOf(Seq<Point3d> points, Op key) =>
        points.Count switch {
            < 3 => Fin.Fail<Seq<double>>(key.InvalidInput()),
            _ => toSeq(Enumerable.Range(start: 1, count: points.Count - 2))
                .Map(i => (E0: points[i] - points[i - 1], E1: points[i + 1] - points[i]) switch {
                    (Vector3d E0, Vector3d E1) edges when (edges.E0.Length + edges.E1.Length) * 0.5 > RhinoMath.ZeroTolerance =>
                        Vector3d.VectorAngle(a: edges.E0, b: edges.E1) / ((edges.E0.Length + edges.E1.Length) * 0.5),
                    _ => 0.0,
                })
                .TraverseM(c => key.AcceptValue(value: c)).As(),
        };
    internal static Fin<double> OpenLengthOf(Seq<Point3d> points, Op key) =>
        SegmentLengthsOf(points: points, key: key).Bind(lengths => key.AcceptValue(value: lengths.Fold(initialState: 0.0, f: static (sum, length) => sum + length)));
    private static Fin<Seq<double>> SegmentLengthsOf(Seq<Point3d> points, Op key) =>
        points.Count switch {
            < 2 => Fin.Fail<Seq<double>>(key.InvalidInput()),
            _ => toSeq(Enumerable.Range(start: 0, count: points.Count - 1)).Map(i => points[i + 1].DistanceTo(other: points[i])).TraverseM(length => key.AcceptValue(value: length)).As(),
        };
    // --- [HULL] -----------------------------------------------------------------------------
    internal static Fin<CloudHullResult> ComputeHullDetailed(VectorCloud source, CloudHullKind kind, CloudHullPolicy policy, Op key) =>
        source switch {
            VectorCloud.ClusterCase cluster => kind switch {
                CloudHullKind k when k.Equals(CloudHullKind.Convex3D) => ConvexHullOf(cluster: cluster, policy: policy, key: key),
                CloudHullKind k when k.Equals(CloudHullKind.ConvexFootprint2D) || k.Equals(CloudHullKind.FootprintWrapper) => ConvexFootprintOf(cluster: cluster, kind: kind, policy: policy, key: key),
                CloudHullKind k when k.Equals(CloudHullKind.ConcaveOutline) || k.Equals(CloudHullKind.AlphaShape) => Fin.Succ(CloudHullResult.Unsupported(kind: kind, policy: policy, inputCount: cluster.Vertices.Count)),
                _ => Fin.Fail<CloudHullResult>(key.Unsupported(geometryType: typeof(CloudHullKind), outputType: typeof(CloudHullResult))),
            },
            _ => Fin.Fail<CloudHullResult>(error: key.Unsupported(geometryType: source.GetType(), outputType: typeof(CloudHullResult))),
        };
    private static Fin<CloudHullResult> ConvexHullOf(VectorCloud.ClusterCase cluster, CloudHullPolicy policy, Op key) {
        if (cluster.Vertices.Count < 4) return Fin.Succ(CloudHullResult.Rejected(kind: CloudHullKind.Convex3D, policy: policy, inputCount: cluster.Vertices.Count, planarityDeviation: None, coplanarRejected: false, containmentRejectedCount: cluster.Vertices.Count, nativeRouted: true, fallback: false));
        if (Point3d.ArePointsCoplanar(points: cluster.Vertices.AsIterable(), tolerance: policy.Tolerance.Value)) return Fin.Succ(CloudHullResult.Rejected(kind: CloudHullKind.Convex3D, policy: policy, inputCount: cluster.Vertices.Count, planarityDeviation: Some(0.0), coplanarRejected: true, containmentRejectedCount: 0, nativeRouted: true, fallback: false));
        using Mesh? hull = Mesh.CreateConvexHull3D(points: cluster.Vertices.AsIterable(), hullFacets: out int[][] facets, tolerance: policy.Tolerance.Value, angleTolerance: policy.AngleTolerance.Value);
        return hull is { IsValid: true } ? key.AcceptValue(value: hull.DuplicateMesh()).Map(mesh => CloudHullResult.Completed(kind: CloudHullKind.Convex3D, policy: policy, inputCount: cluster.Vertices.Count, mesh: mesh, planarityDeviation: None, fallback: false, nativeFacetCount: facets.Length)) : Fin.Fail<CloudHullResult>(error: key.InvalidResult());
    }
    private static Fin<CloudHullResult> ConvexFootprintOf(VectorCloud.ClusterCase cluster, CloudHullKind kind, CloudHullPolicy policy, Op key) =>
        cluster.Vertices.Count < 3
            ? Fin.Succ(CloudHullResult.Rejected(kind: kind, policy: policy, inputCount: cluster.Vertices.Count, planarityDeviation: None, coplanarRejected: false, containmentRejectedCount: cluster.Vertices.Count, nativeRouted: false, fallback: kind.Equals(CloudHullKind.FootprintWrapper)))
            : from fit in BestFitOf(points: cluster.Vertices, key: key)
              let hull = ConvexHull2D(points: cluster.Vertices, plane: fit.Plane)
              let containmentRejected = hull.Count < 3 ? cluster.Vertices.Count : FootprintContainmentRejectedCount(points: cluster.Vertices, hull: hull, plane: fit.Plane, tolerance: policy.Tolerance.Value)
              from result in containmentRejected == 0
                  ? MeshFromFootprint(points: hull, key: key).Map(mesh => CloudHullResult.Completed(kind: kind, policy: policy, inputCount: cluster.Vertices.Count, mesh: mesh, planarityDeviation: Some(fit.Deviation), fallback: kind.Equals(CloudHullKind.FootprintWrapper)))
                  : Fin.Succ(CloudHullResult.Rejected(kind: kind, policy: policy, inputCount: cluster.Vertices.Count, planarityDeviation: Some(fit.Deviation), coplanarRejected: false, containmentRejectedCount: containmentRejected, nativeRouted: true, fallback: kind.Equals(CloudHullKind.FootprintWrapper)))
              select result;
    private static Seq<Point3d> ConvexHull2D(Seq<Point3d> points, Plane plane) {
        Point3d[] source = [.. points.AsIterable()];
        Point2d[] projected = [.. source.Select(point => {
            _ = plane.ClosestParameter(testPoint: point, s: out double x, t: out double y);
            return new Point2d(x: x, y: y);
        })];
        using PolylineCurve? hull = PolylineCurve.CreateConvexHull2d(points: projected, hullIndices: out int[] indices);
        return hull is not { IsValid: true } || indices.Length < 4 ? Seq<Point3d>() : toSeq(indices.Take(count: indices.Length - 1).Where(i => i >= 0 && i < projected.Length).Select(i => plane.PointAt(u: projected[i].X, v: projected[i].Y)));
    }
    private static int FootprintContainmentRejectedCount(Seq<Point3d> points, Seq<Point3d> hull, Plane plane, double tolerance) {
        if (hull.Count < 3) return points.Count;
        using PolylineCurve? boundary = new Polyline([.. hull.AsIterable(), hull[index: 0]]).ToPolylineCurve();
        return boundary is not { IsValid: true }
            ? points.Count
            : points.AsIterable().Count(point => boundary.Contains(testPoint: point, plane: plane, tolerance: tolerance) == PointContainment.Outside);
    }
    private static Fin<Mesh> MeshFromFootprint(Seq<Point3d> points, Op key) {
        if (points.Count < 3) return Fin.Fail<Mesh>(key.InvalidResult());
        using Mesh? mesh = Mesh.CreateFromClosedPolyline(polyline: [.. Enumerable.Append(source: points.AsIterable(), element: points[index: 0])]);
        return mesh is { IsValid: true }
            ? key.AcceptValue(value: mesh.DuplicateMesh())
            : Fin.Fail<Mesh>(key.InvalidResult());
    }
    // --- [NEIGHBORHOODS] ---------------------------------------------------------------------
    internal readonly record struct CloudNeighborhoodGraph(int[][] Ids, CloudNeighborhoodReceipt Receipt);
    internal static Fin<CloudNeighborhoodReceipt> NeighborhoodReceiptOf(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) =>
        cluster.Vertices.Count < 1
            ? Fin.Fail<CloudNeighborhoodReceipt>(key.InvalidInput())
            : from graph in NeighborhoodGraphOf(pointcloud: cluster.Indexed, needlePts: [.. cluster.Vertices.AsIterable()], policy: policy, key: key)
              select graph.Receipt;
    // --- [NORMAL_ESTIMATION] -----------------------------------------------------------------
    internal static Fin<Vector3d[]> EstimateNormalsViaCovariance(VectorCloud.ClusterCase target, Op key) =>
        from policy in CloudNeighborhoodPolicy.Default(key: key)
        from graph in EstimateNormalGraph(target: target, policy: policy, key: key)
        select graph.Normals;
    internal static Fin<(Vector3d[] Normals, CloudNeighborhoodGraph Graph)> EstimateNormalGraph(VectorCloud.ClusterCase target, CloudNeighborhoodPolicy policy, Op key) =>
        target.Vertices.AsIterable().ToArray() switch {
            Point3d[] points when points.Length >= 3 =>
                from graph in NeighborhoodGraphOf(pointcloud: target.Indexed, needlePts: points, policy: policy, key: key)
                from normals in EstimateNormalsFromPoints(points: points, key: key, policy: Some(policy), neighborhoodOf: Some<Func<int, Fin<Seq<Point3d>>>>(i => NeighborhoodOf(pointcloud: target.Indexed, pointCount: points.Length, ids: graph.Ids.Length > i ? graph.Ids[i] : [], key: key)))
                select (Normals: normals, Graph: graph),
            _ => Fin.Fail<(Vector3d[], CloudNeighborhoodGraph)>(key.InvalidInput()),
        };
    private static Fin<CloudNeighborhoodGraph> NeighborhoodGraphOf(PointCloud pointcloud, Point3d[] needlePts, CloudNeighborhoodPolicy policy, Op key) {
        int amount = Math.Min(val1: policy.NeighborCount.Value, val2: pointcloud.Count);
        double radius = policy.Radius.Match(Some: static value => value.Value, None: static () => 0.0);
        Fin<int[][]> found = policy.Radius.IsSome
            ? RadiusNeighborhoodIdsOf(pointcloud: pointcloud, needlePts: needlePts, radius: radius, amount: amount, key: key)
            : key.Catch(() => {
                IEnumerable<int[]> ids = RTree.PointCloudKNeighbors(pointcloud: pointcloud, needlePts: needlePts, amount: amount);
                using IDisposable? lease = ids as IDisposable;
                return Fin.Succ<int[][]>([.. ids]);
            });
        return from activePolicy in policy.Admit(key: key)
               from ids in pointcloud.Count > 0 && needlePts.Length > 0 && amount > 0 ? found : Fin.Fail<int[][]>(key.InvalidInput())
               let counts = ids.Select(static row => row.Length).ToArray()
               let outOfRange = ids.Sum(row => row.Count(i => i < 0 || i >= pointcloud.Count))
               let duplicateIndex = ids.Sum(static row => row.Length - row.Distinct().Count())
               let duplicateCoordinate = DuplicateCoordinateCount(points: [.. Enumerable.Range(start: 0, count: pointcloud.Count).Select(pointcloud.PointAt)])
               let receipt = new CloudNeighborhoodReceipt(
                   InputCount: pointcloud.Count,
                   QueryCount: needlePts.Length,
                   RequestedNeighborCount: amount,
                   SearchBackend: activePolicy.Radius.IsSome ? CloudNeighborhoodSearchBackend.RhinoPointCloudRadius : CloudNeighborhoodSearchBackend.RhinoPointCloudKnn,
                   RadiusLimited: activePolicy.Radius.IsSome,
                   Radius: activePolicy.Radius.Map(static value => value.Value),
                   NativeIndexRouted: true,
                   SelfNeighborIncluded: SelfNeighborIncluded(ids: ids),
                   EmptyNeighborhoodCount: counts.Count(static count => count == 0),
                   OutOfRangeIndexCount: outOfRange,
                   DuplicateIndexCount: duplicateIndex,
                   DuplicateCoordinateCount: duplicateCoordinate,
                   MinReturnedCount: counts.Length == 0 ? 0 : counts.Min(),
                   MaxReturnedCount: counts.Length == 0 ? 0 : counts.Max(),
                   MeanReturnedCount: counts.Length == 0 ? 0.0 : counts.Average())
               from _ in receipt.IsValid ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
               select new CloudNeighborhoodGraph(Ids: ids, Receipt: receipt);
    }
    private static Fin<int[][]> RadiusNeighborhoodIdsOf(PointCloud pointcloud, Point3d[] needlePts, double radius, int amount, Op key) =>
        key.Catch(() => {
            IEnumerable<int[]> found = RTree.PointCloudClosestPoints(pointcloud: pointcloud, needlePts: needlePts, limitDistance: radius);
            using IDisposable? lease = found as IDisposable;
            return Fin.Succ<int[][]>([.. found.Select((within, needle) => within
                .Where(i => i >= 0 && i < pointcloud.Count)
                .OrderBy(i => needlePts[needle].DistanceToSquared(other: pointcloud.PointAt(index: i)))
                .Take(count: amount)
                .ToArray())]);
        });
    private static Fin<Seq<Point3d>> NeighborhoodOf(PointCloud pointcloud, int pointCount, int[] ids, Op key) {
        Seq<Point3d> neighborhood = toSeq(ids.Where(i => i >= 0 && i < pointCount).Select(pointcloud.PointAt));
        return ids.Length == 0 || neighborhood.IsEmpty
            ? Fin.Fail<Seq<Point3d>>(key.InvalidResult())
            : Fin.Succ(neighborhood);
    }
    private static bool SelfNeighborIncluded(int[][] ids) =>
        ids.Length > 0 && Enumerable.Range(start: 0, count: ids.Length).All(i => ids[i].Contains(i));
    internal static Fin<CloudNeighborhoodPcaResult> NeighborhoodPcaOf(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) {
        int n = cluster.Vertices.Count;
        int requested = Math.Min(val1: policy.NeighborCount.Value, val2: n);
        if (n < 3 || requested < 3) return Fin.Fail<CloudNeighborhoodPcaResult>(key.InvalidInput());
        Point3d[] points = [.. cluster.Vertices.AsIterable()];
        return NeighborhoodGraphOf(pointcloud: cluster.Indexed, needlePts: points, policy: policy, key: key).Bind(graph => {
            List<CloudNeighborhoodPcaSample> samples = [];
            int rejected = 0;
            for (int i = 0; i < n; i++) {
                Fin<CloudNeighborhoodPcaSample> sample = NeighborhoodPcaSampleOf(pointcloud: cluster.Indexed, pointCount: points.Length, point: points[i], ids: graph.Ids.Length > i ? graph.Ids[i] : [], index: i, policy: policy, key: key);
                _ = sample.Match(Succ: value => { samples.Add(item: value); return unit; }, Fail: _ => { rejected++; return unit; });
            }
            int rankClamp = samples.Count(static sample => sample.Rank < 3);
            int eigenClamp = samples.Sum(static sample => sample.EigenClampCount);
            double floor = EigenClampFloor(policy: policy);
            CloudNeighborhoodPcaReceipt receipt = new(InputCount: n, RequestedNeighborCount: requested, AcceptedSampleCount: samples.Count, RejectedSampleCount: rejected, RankClampCount: rankClamp, EigenClampCount: eigenClamp, EigenClampFloor: floor, Neighborhood: graph.Receipt);
            CloudNeighborhoodPcaResult result = new(Samples: toSeq(samples), Receipt: receipt);
            return result.IsValid && samples.Count == n ? Fin.Succ(result) : Fin.Fail<CloudNeighborhoodPcaResult>(key.InvalidResult());
        });
    }
    private static Fin<CloudNeighborhoodPcaSample> NeighborhoodPcaSampleOf(PointCloud pointcloud, int pointCount, Point3d point, int[] ids, int index, CloudNeighborhoodPolicy policy, Op key) =>
        from neighborhood in NeighborhoodOf(pointcloud: pointcloud, pointCount: pointCount, ids: ids, key: key)
        from _ in neighborhood.Count >= 3 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from stats in CovarianceOf(points: neighborhood, key: key)
        from eigen in stats.Cov.DecomposeEigen(key: key)
        from sample in NeighborhoodPcaSampleOf(point: point, index: index, neighborCount: neighborhood.Count, eigen: eigen, policy: policy, key: key)
        select sample;
    private static Fin<CloudNeighborhoodPcaSample> NeighborhoodPcaSampleOf(Point3d point, int index, int neighborCount, Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, CloudNeighborhoodPolicy policy, Op key) {
        double floor = EigenClampFloor(policy: policy);
        return eigen.Count < 3
            ? Fin.Fail<CloudNeighborhoodPcaSample>(key.InvalidResult())
            : from vectors in toSeq(Enumerable.Range(start: 0, count: 3))
                .TraverseM(i => Direction.Of(value: AsVector3d(v: eigen[index: i].Eigenvector), tolerance: RhinoMath.ZeroTolerance, key: key)
                    .Map(direction => (Raw: Math.Max(val1: 0.0, val2: eigen[index: i].Eigenvalue), Axis: direction.Value)))
                .As()
              let raw = vectors.Map(static item => item.Raw)
              let clamped = raw.Map(value => Math.Max(val1: value, val2: floor))
              let covariance = NeighborhoodCovarianceOf(axes: vectors.Map(static item => item.Axis), eigenvalues: clamped)
              from cov in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: covariance, key: key)
              let sample = new CloudNeighborhoodPcaSample(
                  Index: index,
                  Point: point,
                  NeighborCount: neighborCount,
                  Covariance: cov,
                  Normal: vectors[index: 2].Axis,
                  RawEigenvalues: new Arr<double>([.. raw.AsIterable()]),
                  ClampedEigenvalues: new Arr<double>([.. clamped.AsIterable()]),
                  Rank: raw.Count(value => value > floor),
                  EigenClampCount: raw.Count(value => value <= floor))
              from valid in sample.IsValid ? Fin.Succ(sample) : Fin.Fail<CloudNeighborhoodPcaSample>(key.InvalidResult())
              select valid;
    }
    private static Arr<double> NeighborhoodCovarianceOf(Seq<Vector3d> axes, Seq<double> eigenvalues) {
        double[] upper = new double[6];
        for (int e = 0; e < 3; e++) {
            Vector3d axis = axes[index: e];
            double lambda = eigenvalues[index: e];
            upper[0] += lambda * axis.X * axis.X; upper[1] += lambda * axis.X * axis.Y; upper[2] += lambda * axis.X * axis.Z;
            upper[3] += lambda * axis.Y * axis.Y; upper[4] += lambda * axis.Y * axis.Z; upper[5] += lambda * axis.Z * axis.Z;
        }
        return new Arr<double>(upper);
    }
    private static double EigenClampFloor(CloudNeighborhoodPolicy policy) =>
        Math.Max(val1: policy.EigenGapTolerance.Value, val2: RhinoMath.SqrtEpsilon);
    // --- [NORMAL_ORIENTATION] ---------------------------------------------------------------
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992: kNN MST weighted by 1 - |n_i dot n_j|.
    internal static Fin<Seq<Vector3d>> OrientNormalsViaMst(VectorCloud cloud, CloudNeighborhoodPolicy policy, Op key) =>
        cloud switch {
            VectorCloud.ClusterCase cluster => OrientClusterNormals(cluster: cluster, policy: policy, key: key),
            _ => Fin.Fail<Seq<Vector3d>>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(Seq<Vector3d>))),
        };
    private static Fin<Seq<Vector3d>> OrientClusterNormals(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) {
        int n = cluster.Vertices.Count;
        return n == 0
            ? Fin.Fail<Seq<Vector3d>>(key.InvalidInput())
            : EstimateNormalGraph(target: cluster, policy: policy, key: key).Bind(((Vector3d[] Normals, CloudNeighborhoodGraph Graph) graph) => {
                bool[] visited = new bool[n];
                double[] bestWeight = [.. Enumerable.Repeat(element: double.PositiveInfinity, count: n)];
                int[] parent = [.. Enumerable.Repeat(element: -1, count: n)];
                for (int root = 0; root < n; root++) {
                    if (visited[root]) continue;
                    bestWeight[root] = 0.0;
                    for (int step = 0; step < n; step++) {
                        int curr = -1; double best = double.PositiveInfinity;
                        for (int i = 0; i < n; i++)
                            if (!visited[i] && bestWeight[i] < best) { best = bestWeight[i]; curr = i; }
                        if (curr < 0 || double.IsPositiveInfinity(best)) break;
                        visited[curr] = true;
                        if (parent[curr] >= 0 && graph.Normals[parent[curr]] * graph.Normals[curr] < 0.0) graph.Normals[curr] = -graph.Normals[curr];
                        int[] neighbors = graph.Graph.Ids.Length > curr ? graph.Graph.Ids[curr] : [];
                        for (int e = 0; e < neighbors.Length; e++) {
                            int j = neighbors[e];
                            if (j < 0 || j >= n || visited[j] || curr == j) continue;
                            double weight = 1.0 - Math.Abs(value: graph.Normals[curr] * graph.Normals[j]);
                            if (weight < bestWeight[j]) { bestWeight[j] = weight; parent[j] = curr; }
                        }
                    }
                }
                return toSeq(graph.Normals).TraverseM(normal => key.AcceptValue(value: normal)).As();
            });
    }
    internal static Fin<Vector3d[]> EstimateNormalsFromPoints(Point3d[] points, Op key, Option<CloudNeighborhoodPolicy> policy = default, Option<Func<int, Fin<Seq<Point3d>>>> neighborhoodOf = default) =>
        from active in policy.Match(Some: static value => Fin.Succ(value), None: () => CloudNeighborhoodPolicy.Default(key: key))
        from normals in EstimateNormalsOf(points: points, neighborhoodOf: neighborhoodOf.IfNone(() => BatchedNeighborhoods(points: points, policy: active, key: key)), policy: active, key: key)
        select normals;
    private static Func<int, Fin<Seq<Point3d>>> BatchedNeighborhoods(Point3d[] points, CloudNeighborhoodPolicy policy, Op key) {
        PointCloud cloud = [];
        cloud.AddRange(points: points);
        Fin<CloudNeighborhoodGraph> graph = NeighborhoodGraphOf(pointcloud: cloud, needlePts: points, policy: policy, key: key);
        return i => graph.Bind(g => NeighborhoodOf(pointcloud: cloud, pointCount: points.Length, ids: g.Ids.Length > i ? g.Ids[i] : [], key: key));
    }
    private static Fin<Vector3d[]> EstimateNormalsOf(Point3d[] points, Func<int, Fin<Seq<Point3d>>> neighborhoodOf, CloudNeighborhoodPolicy policy, Op key) {
        int n = points.Length;
        return n < 3
            ? Fin.Fail<Vector3d[]>(key.InvalidInput())
            : toSeq(Enumerable.Range(start: 0, count: n)).TraverseM(i =>
                from neighborhood in neighborhoodOf(arg: i)
                from normal in neighborhood.Count < 3
                    ? Fin.Fail<Vector3d>(key.InvalidInput())
                    : from stats in CovarianceOf(points: neighborhood, key: key)
                      from eigen in stats.Cov.DecomposeEigen(key: key)
                      from _ in guard(eigen.Count >= 3 && eigen[index: 1].Eigenvalue > policy.EigenGapTolerance.Value, key.InvalidResult())
                      let raw = AsVector3d(v: eigen[index: 2].Eigenvector)
                      from direction in Direction.Of(value: raw, tolerance: RhinoMath.ZeroTolerance, key: key)
                      select direction.Value
                select normal).As().Map(static normals => normals.AsIterable().ToArray());
    }

    // --- [PRINCIPAL_CURVATURE] -------------------------------------------------------------
    // Local quadric fit in PCA tangent frame; II = [[2a, b], [b, 2c]] yields signed curvatures.
    // Umbilic band: |k1|-|k2| within 35% of the dominant magnitude classifies a sample sphere-like.
    private const double SphereLikenessBand = 0.35;
    internal static Fin<CloudCurvatureResult> PrincipalCurvaturesOf(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) {
        int n = cluster.Vertices.Count;
        int requested = Math.Min(val1: policy.NeighborCount.Value, val2: n);
        if (n < 6 || requested < 6) return Fin.Fail<CloudCurvatureResult>(error: key.InvalidInput());
        Point3d[] points = [.. cluster.Vertices.AsIterable()];
        return NeighborhoodGraphOf(pointcloud: cluster.Indexed, needlePts: points, policy: policy, key: key).Bind(graph => {
            List<CloudCurvatureSample> samples = [];
            int rankRejected = 0;
            int residualRejected = 0;
            for (int i = 0; i < n; i++) {
                CurvatureAttempt attempt = CurvatureAttemptOf(cluster: cluster, points: points, neighborhoods: graph.Ids, index: i, policy: policy, key: key);
                CloudCurvatureSample sample = attempt.Sample.Match(Some: static value => value, None: static () => default);
                if (attempt.Sample.IsSome) samples.Add(item: sample);
                rankRejected += attempt.RankRejected ? 1 : 0;
                residualRejected += attempt.ResidualRejected ? 1 : 0;
            }
            double meanResidual = samples.Count == 0 ? 0.0 : samples.Average(static sample => sample.Residual);
            double maxResidual = samples.Count == 0 ? 0.0 : samples.Max(static sample => sample.Residual);
            CloudCurvatureReceipt receipt = new(InputCount: n, RequestedNeighborCount: requested, AcceptedSampleCount: samples.Count, RejectedSampleCount: n - samples.Count, RankRejectedCount: rankRejected, ResidualRejectedCount: residualRejected, MeanResidual: meanResidual, MaxResidual: maxResidual, EigenGapTolerance: policy.EigenGapTolerance.Value, FitResidualTolerance: policy.FitResidualTolerance.Value, Neighborhood: graph.Receipt, Range: CurvatureRangeOf(samples: samples, tolerance: Math.Max(val1: policy.FitResidualTolerance.Value, val2: policy.EigenGapTolerance.Value)));
            CloudCurvatureResult result = new(Samples: toSeq(samples), Receipt: receipt);
            return result.IsValid && samples.Count > 0 ? Fin.Succ(result) : Fin.Fail<CloudCurvatureResult>(key.InvalidResult());
        });
    }
    private static Fin<Seq<double>> CurvatureScalars(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key, Func<CloudCurvatureSample, double> project) =>
        PrincipalCurvaturesOf(cluster: cluster, policy: policy, key: key).Map(curvatures => toSeq(curvatures.Samples.AsIterable().Select(project)));
    internal static Fin<Seq<double>> CurvednessOf(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) =>
        CurvatureScalars(cluster: cluster, policy: policy, key: key, project: static c => Math.Sqrt(d: 0.5 * ((c.K1 * c.K1) + (c.K2 * c.K2))));
    // Koenderink-van Doorn 1992 shape index in [-1, 1]: cup, trough, saddle, ridge, cap.
    internal static Fin<Seq<double>> ShapeIndexOf(VectorCloud.ClusterCase cluster, CloudNeighborhoodPolicy policy, Op key) =>
        CurvatureScalars(cluster: cluster, policy: policy, key: key, project: ShapeIndexOf);
    private readonly record struct CurvatureAttempt(Option<CloudCurvatureSample> Sample, bool RankRejected, bool ResidualRejected) {
        internal static readonly CurvatureAttempt Rank = new(Sample: None, RankRejected: true, ResidualRejected: false);
        internal static readonly CurvatureAttempt Residual = new(Sample: None, RankRejected: false, ResidualRejected: true);
        internal static CurvatureAttempt Accepted(CloudCurvatureSample sample) => new(Sample: Some(sample), RankRejected: false, ResidualRejected: false);
    }
    private static CurvatureAttempt CurvatureAttemptOf(VectorCloud.ClusterCase cluster, Point3d[] points, int[][] neighborhoods, int index, CloudNeighborhoodPolicy policy, Op key) =>
        NeighborhoodOf(pointcloud: cluster.Indexed, pointCount: points.Length, ids: neighborhoods.Length > index ? neighborhoods[index] : [], key: key).Match(
            Succ: neighborhood => CurvatureAttemptOf(point: points[index], index: index, neighborhood: neighborhood, policy: policy, key: key),
            Fail: static _ => CurvatureAttempt.Rank);
    private static CurvatureAttempt CurvatureAttemptOf(Point3d point, int index, Seq<Point3d> neighborhood, CloudNeighborhoodPolicy policy, Op key) =>
        neighborhood.Count < 6 ? CurvatureAttempt.Rank : PrincipalStatsOf(points: neighborhood, key: key).Match(
            Succ: stats => stats.Eigen.Count >= 3 && stats.Eigen[index: 1].Eigenvalue > policy.EigenGapTolerance.Value
                ? CurvatureAttemptFromStats(point: point, index: index, neighborhood: neighborhood, stats: stats, policy: policy, key: key)
                : CurvatureAttempt.Rank,
            Fail: static _ => CurvatureAttempt.Rank);
    private static CurvatureAttempt CurvatureAttemptFromStats(Point3d point, int index, Seq<Point3d> neighborhood, PrincipalStats stats, CloudNeighborhoodPolicy policy, Op key) =>
        (from normal in Direction.Of(value: AsVector3d(v: stats.Eigen[index: 2].Eigenvector), tolerance: RhinoMath.ZeroTolerance, key: key)
         from uAxis in Direction.Of(value: AsVector3d(v: stats.Eigen[index: 0].Eigenvector), tolerance: RhinoMath.ZeroTolerance, key: key)
         from vAxis in Direction.Of(value: Vector3d.CrossProduct(a: normal.Value, b: uAxis.Value), tolerance: RhinoMath.ZeroTolerance, key: key)
         from fit in QuadraticFit(center: point, neighborhood: neighborhood, uAxis: uAxis.Value, vAxis: vAxis.Value, normal: normal.Value, key: key)
         select (normal, uAxis, vAxis, fit)).Match(
            Succ: state => state.fit.Receipt.Residual > policy.FitResidualTolerance.Value
                ? CurvatureAttempt.Residual
                : ShapeOperatorEigen(a: state.fit.A, b: state.fit.B, c: state.fit.C, uAxis: state.uAxis.Value, vAxis: state.vAxis.Value, key: key).Match(
                    Succ: output => CurvatureAttempt.Accepted(new CloudCurvatureSample(Index: index, Point: point, K1: output.K1, K2: output.K2, E1: output.E1, E2: output.E2, Residual: state.fit.Receipt.Residual, NeighborCount: neighborhood.Count)),
                    Fail: static _ => CurvatureAttempt.Rank),
            Fail: static _ => CurvatureAttempt.Rank);
    private static Fin<(double A, double B, double C, SolveReceipt Receipt)> QuadraticFit(Point3d center, Seq<Point3d> neighborhood, Vector3d uAxis, Vector3d vAxis, Vector3d normal, Op key) {
        int m = neighborhood.Count;
        double[] designFlat = new double[m * 6];
        double[] rhs = new double[m];
        for (int i = 0; i < m; i++) {
            Vector3d offset = neighborhood[index: i] - center;
            double u = offset * uAxis; double v = offset * vAxis; double n = offset * normal;
            int row = i * 6;
            designFlat[row + 0] = u * u; designFlat[row + 1] = u * v; designFlat[row + 2] = v * v;
            designFlat[row + 3] = u; designFlat[row + 4] = v; designFlat[row + 5] = 1.0;
            rhs[i] = n;
        }
        return from design in Matrix.Of(rows: Dimension.Create(value: m), cols: Dimension.Create(value: 6), entries: new Arr<double>(designFlat), key: key)
               from receipt in design.LeastSquaresDetailed(rhs: new Arr<double>(rhs), key: key)
               from _ in guard(
                   receipt.FullRank.IfNone(noneValue: false)
                   && receipt.Solution.Count == 6
                   && receipt.Solution.ForAll(RhinoMath.IsValidDouble)
                   && RhinoMath.IsValidDouble(x: receipt.Residual),
                   key.InvalidResult())
               select (A: receipt.Solution[0], B: receipt.Solution[1], C: receipt.Solution[2], Receipt: receipt);
    }
    private static Fin<(double K1, double K2, Direction E1, Direction E2)> ShapeOperatorEigen(double a, double b, double c, Vector3d uAxis, Vector3d vAxis, Op key) {
        return from matrix in SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [2.0 * a, b, 2.0 * c], key: key)
               from eigen in matrix.DecomposeEigen(key: key)
               let ordered = toSeq(eigen.AsIterable().OrderByDescending(static pair => pair.Eigenvalue))
               from _ in guard(ordered.Count >= 2, key.InvalidResult())
               from e1 in Direction.Of(value: LiftEigenvector(vector: ordered[index: 0].Eigenvector, uAxis: uAxis, vAxis: vAxis), tolerance: RhinoMath.ZeroTolerance, key: key)
               from e2 in Direction.Of(value: LiftEigenvector(vector: ordered[index: 1].Eigenvector, uAxis: uAxis, vAxis: vAxis), tolerance: RhinoMath.ZeroTolerance, key: key)
               select (K1: ordered[index: 0].Eigenvalue, K2: ordered[index: 1].Eigenvalue, E1: e1, E2: e2);

        static Vector3d LiftEigenvector(Arr<double> vector, Vector3d uAxis, Vector3d vAxis) =>
            vector.Count >= 2 ? (vector[index: 0] * uAxis) + (vector[index: 1] * vAxis) : Vector3d.Unset;
    }
    private static CloudCurvatureRangeReceipt CurvatureRangeOf(List<CloudCurvatureSample> samples, double tolerance) {
        if (samples.Count == 0)
            return new CloudCurvatureRangeReceipt(AcceptedSampleCount: 0, Kind: CloudCurvatureRangeKind.Empty, PlaneLikeCount: 0, SphereLikeCount: 0, SaddleLikeCount: 0, MixedCount: 0, MinK1: 0.0, MaxK1: 0.0, MinK2: 0.0, MaxK2: 0.0, MinGaussian: 0.0, MaxGaussian: 0.0, MinMean: 0.0, MaxMean: 0.0, MinShapeIndex: 0.0, MaxShapeIndex: 0.0, Tolerance: tolerance);
        double[] k1 = [.. samples.Select(static sample => sample.K1)];
        double[] k2 = [.. samples.Select(static sample => sample.K2)];
        double[] gaussian = [.. samples.Select(static sample => sample.K1 * sample.K2)];
        double[] mean = [.. samples.Select(static sample => 0.5 * (sample.K1 + sample.K2))];
        double[] shape = [.. samples.Select(ShapeIndexOf)];
        int plane = 0, sphere = 0, saddle = 0, mixed = 0;
        foreach (CloudCurvatureSample sample in samples) {
            double maxAbs = Math.Max(val1: Math.Abs(value: sample.K1), val2: Math.Abs(value: sample.K2));
            double gaussianValue = sample.K1 * sample.K2;
            if (maxAbs <= tolerance) plane++;
            else if (gaussianValue > tolerance * tolerance && Math.Abs(value: Math.Abs(value: sample.K1) - Math.Abs(value: sample.K2)) <= Math.Max(val1: tolerance, val2: SphereLikenessBand * maxAbs)) sphere++;
            else if (gaussianValue < -(tolerance * tolerance)) saddle++;
            else mixed++;
        }
        CloudCurvatureRangeKind kind = (plane, sphere, saddle, mixed) switch {
            (int p, 0, 0, 0) when p == samples.Count => CloudCurvatureRangeKind.Plane,
            (0, int s, 0, 0) when s == samples.Count => CloudCurvatureRangeKind.Sphere,
            (0, 0, int s, 0) when s == samples.Count => CloudCurvatureRangeKind.Saddle,
            _ => CloudCurvatureRangeKind.Mixed,
        };
        return new CloudCurvatureRangeReceipt(AcceptedSampleCount: samples.Count, Kind: kind, PlaneLikeCount: plane, SphereLikeCount: sphere, SaddleLikeCount: saddle, MixedCount: mixed, MinK1: k1.Min(), MaxK1: k1.Max(), MinK2: k2.Min(), MaxK2: k2.Max(), MinGaussian: gaussian.Min(), MaxGaussian: gaussian.Max(), MinMean: mean.Min(), MaxMean: mean.Max(), MinShapeIndex: shape.Min(), MaxShapeIndex: shape.Max(), Tolerance: tolerance);
    }
    private static double ShapeIndexOf(CloudCurvatureSample sample) =>
        (Diff: sample.K1 - sample.K2, Sum: sample.K1 + sample.K2) switch { (double diff, double sum) => Math.Abs(value: diff) < RhinoMath.SqrtEpsilon ? Math.Sign(value: sum) : 2.0 / Math.PI * Math.Atan2(y: sum, x: diff) };
    // --- [TRANSPORT] ------------------------------------------------------------------------
    // Below this log-argument double-precision Math.Exp underflows to 0 (ln of smallest positive double ~ -745).
    private const double LogUnderflowFloor = -745.0;
    internal static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op key) =>
        from activePolicy in policy.Admit(key: key)
        from output in (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => SinkhornCluster<TOut>(source: src, target: tgt, policy: activePolicy, key: key),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> SinkhornCluster<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, CloudTransportPolicy policy, Op key) =>
        source.Vertices.Count < 1 || target.Vertices.Count < 1
            ? Fin.Fail<TOut>(error: key.InvalidInput())
            : from sourceMass in MassOf(cluster: source, key: key)
              from targetMass in MassOf(cluster: target, key: key)
              from plan in SinkhornOt(source: source.Vertices, target: target.Vertices, sourceMass: sourceMass, targetMass: targetMass, policy: policy, key: key)
              from bias in policy.Debiased
                  ? from sourceBias in SinkhornOt(source: source.Vertices, target: source.Vertices, sourceMass: sourceMass, targetMass: sourceMass, policy: policy, key: key) from targetBias in SinkhornOt(source: target.Vertices, target: target.Vertices, sourceMass: targetMass, targetMass: targetMass, policy: policy, key: key) select (Source: Some(sourceBias.Distance), Target: Some(targetBias.Distance), Distance: plan.Distance - (0.5 * sourceBias.Distance) - (0.5 * targetBias.Distance))
                  : key.AcceptValue(value: plan.Distance).Map(distance => (Source: Option<double>.None, Target: Option<double>.None, Distance: distance))
              from output in plan.Project<TOut>(source: source, target: target, distance: bias.Distance, sourceBias: bias.Source, targetBias: bias.Target, policy: policy, key: key)
              select output;
    private sealed record SinkhornPlan(double Distance, DenseMatrixD Coupling, double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop, double ConvergenceTolerance, double CouplingCutoff, bool UnderflowFloored) {
        internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance, Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
            SinkhornPlan self = this;
            return AtomProjection.Rows<SinkhornPlan, TOut>(self: self, key: key, owner: typeof(VectorCloud),
                new ProjectionRow(typeof(double), () => key.AcceptValue(value: distance).Map(static d => (object)d)),
                new ProjectionRow(typeof(SinkhornReceipt), () => self.ReceiptOf(source: source, target: target, distance: distance, sourceBias: sourceBias, targetBias: targetBias, policy: policy, key: key).Map(static receipt => (object)receipt)),
                new ProjectionRow(typeof(CloudCorrespondenceSet), () => CouplingCorrespondences(source: source, target: target, coupling: self.Coupling, couplingCutoff: self.CouplingCutoff, key: key).Map(static correspondences => (object)correspondences)),
                new ProjectionRow(typeof(Matrix), () => MatrixKernel.FromMathNet(m: self.Coupling, rows: Dimension.Create(value: self.Coupling.RowCount), cols: Dimension.Create(value: self.Coupling.ColumnCount)) switch {
                    Matrix matrix when matrix.IsValid => Fin.Succ<object>(matrix),
                    _ => Fin.Fail<object>(error: key.InvalidResult()),
                }),
                new ProjectionRow(typeof(VectorCloud), () => toSeq(Enumerable.Range(start: 0, count: source.Vertices.Count)).TraverseM(i => self.Coupling.Row(i).Sum() switch {
                    double mass when mass > RhinoMath.ZeroTolerance => Fin.Succ((Point: Point3d.Origin + (toSeq(Enumerable.Range(start: 0, count: target.Vertices.Count)).Fold(initialState: Vector3d.Zero, f: (sum, j) => sum + (self.Coupling[i, j] * (Vector3d)target.Vertices[index: j])) / mass), Mass: mass)),
                    _ => Fin.Fail<(Point3d Point, double Mass)>(key.InvalidResult()),
                }).As().Bind(transported => VectorCloud.WeightedCluster(points: transported.Map(static item => item.Point), mass: transported.Map(static item => item.Mass), context: source.Tolerance, key: key).Map(static cloud => (object)cloud))));
        }
        private Fin<SinkhornReceipt> ReceiptOf(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance, Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
            (double couplingMass, int nonZeroCouplings, double minPositiveCoupling, double maxCoupling) = Coupling.Enumerate().Aggregate(
                seed: (Total: 0.0, Nonzero: 0, MinPositive: double.PositiveInfinity, Max: 0.0),
                func: (s, value) => value > CouplingCutoff ? (s.Total + value, s.Nonzero + 1, Math.Min(val1: s.MinPositive, val2: value), Math.Max(val1: s.Max, val2: value)) : s);
            return CouplingCorrespondences(source: source, target: target, coupling: Coupling, couplingCutoff: CouplingCutoff, key: key).Map(correspondences => new SinkhornReceipt(
                Distance: distance, RawDistance: Some(Distance), SourceBiasDistance: sourceBias, TargetBiasDistance: targetBias, Regularization: policy.Regularization.Value, MassRelaxation: policy.MassRelaxation.Map(static value => value.Value), ConvergenceTolerance: ConvergenceTolerance, CouplingCutoff: CouplingCutoff, Debiased: policy.Debiased,
                ResidualKind: policy.MassRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass, NumericStatus: UnderflowFloored ? SinkhornNumericStatus.UnderflowFloored : SinkhornNumericStatus.FiniteAccepted,
                SourceConvergenceResidual: SourceConvergenceResidual, TargetConvergenceResidual: TargetConvergenceResidual, Iterations: Iterations, Stop: Stop, CouplingMass: couplingMass, NonZeroCouplings: nonZeroCouplings, MinPositiveCoupling: nonZeroCouplings > 0 ? Some(minPositiveCoupling) : Option<double>.None, MaxCoupling: nonZeroCouplings > 0 ? Some(maxCoupling) : Option<double>.None, Correspondences: correspondences));
        }
    }
    private static Fin<SinkhornPlan> SinkhornOt(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
        int m = source.Count; int n = target.Count;
        double reg = policy.Regularization.Value;
        int maxIter = policy.MaxIterations.Value;
        if (FieldNabla.PositiveFiniteWeights(weights: [.. sourceMass.AsIterable()], count: m, key: key).IsFail || FieldNabla.PositiveFiniteWeights(weights: [.. targetMass.AsIterable()], count: n, key: key).IsFail)
            return Fin.Fail<SinkhornPlan>(key.InvalidInput());
        DenseMatrixD logKernel = DenseMatrixD.OfRowArrays(source.AsIterable().Select(src => target.AsIterable().Select(tgt => -src.DistanceToSquared(other: tgt) / reg).ToArray()));
        DenseVectorD logU = DenseVectorD.Create(m, 0.0);
        DenseVectorD logV = DenseVectorD.Create(n, 0.0);
        DenseVectorD logA = DenseVectorD.OfEnumerable(sourceMass.AsIterable().Select(static value => Math.Log(d: value)));
        DenseVectorD logB = DenseVectorD.OfEnumerable(targetMass.AsIterable().Select(static value => Math.Log(d: value)));
        double exponent = policy.MassRelaxation.Match(Some: lambda => lambda.Value / (lambda.Value + reg), None: () => 1.0);
        bool balanced = policy.MassRelaxation.IsNone;
        double sourceConvergenceResidual = double.PositiveInfinity;
        double targetConvergenceResidual = double.PositiveInfinity;
        int iterations = 0;
        for (int iter = 0; iter < maxIter && Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) > policy.ConvergenceTolerance.Value; iter++) {
            iterations = iter + 1;
            double[] previousU = [.. logU];
            double[] previousV = [.. logV];
            for (int i = 0; i < m; i++) {
                double rowNormalizer = LogSumExp(count: n, valueAt: j => logKernel[i, j] + logV[j]);
                logU[i] = exponent * (logA[i] - rowNormalizer);
            }
            for (int j = 0; j < n; j++) {
                double columnNormalizer = LogSumExp(count: m, valueAt: i => logKernel[i, j] + logU[i]);
                logV[j] = exponent * (logB[j] - columnNormalizer);
            }
            (sourceConvergenceResidual, targetConvergenceResidual) = balanced
                ? MarginalResiduals(logKernel: logKernel, logU: logU, logV: logV, sourceMass: sourceMass, targetMass: targetMass)
                : ScalingResiduals(previousU: previousU, logU: logU, previousV: previousV, logV: logV);
        }
        bool underflowFloored = false;
        DenseMatrixD coupling = DenseMatrixD.OfRowArrays(Enumerable.Range(start: 0, count: m).Select(i => Enumerable.Range(start: 0, count: n).Select(j => (logU[i] + logKernel[i, j] + logV[j]) switch {
            double logValue when logValue < LogUnderflowFloor => (underflowFloored = true, 0.0).Item2,
            double logValue => Math.Exp(d: logValue),
        }).ToArray()));
        double dist = -reg * coupling.PointwiseMultiply(logKernel).Enumerate().Sum();
        bool numeric = RhinoMath.IsValidDouble(x: dist) && RhinoMath.IsValidDouble(x: sourceConvergenceResidual) && RhinoMath.IsValidDouble(x: targetConvergenceResidual) && coupling.Enumerate().All(RhinoMath.IsValidDouble);
        bool converged = Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) <= policy.ConvergenceTolerance.Value;
        return numeric
            ? Fin.Succ(new SinkhornPlan(Distance: dist, Coupling: coupling, SourceConvergenceResidual: sourceConvergenceResidual, TargetConvergenceResidual: targetConvergenceResidual, Iterations: iterations, Stop: (balanced, converged) switch {
                (true, true) => SinkhornStopKind.BalancedMarginalsConverged,
                (true, false) => SinkhornStopKind.BalancedMarginalsStoppedWithoutConvergence,
                (false, true) => SinkhornStopKind.RelaxedScalingConverged,
                (false, false) => SinkhornStopKind.RelaxedScalingStoppedWithoutConvergence,
            }, ConvergenceTolerance: policy.ConvergenceTolerance.Value, CouplingCutoff: policy.CouplingCutoff.Value, UnderflowFloored: underflowFloored))
            : Fin.Fail<SinkhornPlan>(key.InvalidResult());
    }
    private static double LogSumExp(int count, Func<int, double> valueAt) =>
        Enumerable.Range(start: 0, count: count).Max(valueAt) switch {
            double.NegativeInfinity => double.NegativeInfinity,
            double max => max + Math.Log(d: Enumerable.Range(start: 0, count: count).Sum(i => Math.Exp(d: valueAt(arg: i) - max))),
        };
    private static double ExpFloored(double logValue) => logValue < LogUnderflowFloor ? 0.0 : Math.Exp(d: logValue);
    private static (double Source, double Target) MarginalResiduals(DenseMatrixD logKernel, DenseVectorD logU, DenseVectorD logV, Arr<double> sourceMass, Arr<double> targetMass) =>
        (Source: Enumerable.Range(start: 0, count: logKernel.RowCount).Max(i => Math.Abs(value: ExpFloored(logValue: logU[i] + LogSumExp(count: logKernel.ColumnCount, valueAt: j => logKernel[i, j] + logV[j])) - sourceMass[index: i])),
            Target: Enumerable.Range(start: 0, count: logKernel.ColumnCount).Max(j => Math.Abs(value: ExpFloored(logValue: logV[j] + LogSumExp(count: logKernel.RowCount, valueAt: i => logKernel[i, j] + logU[i])) - targetMass[index: j])));
    private static (double Source, double Target) ScalingResiduals(double[] previousU, DenseVectorD logU, double[] previousV, DenseVectorD logV) =>
        (Source: Enumerable.Range(start: 0, count: logU.Count).Max(i => Math.Abs(value: logU[i] - previousU[i])),
            Target: Enumerable.Range(start: 0, count: logV.Count).Max(i => Math.Abs(value: logV[i] - previousV[i])));
    internal static Fin<CloudCorrespondenceSet> CouplingCorrespondences(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, DenseMatrixD coupling, double couplingCutoff, Op key) =>
        from sourceMass in MassOf(cluster: source, key: key)
        from targetMass in MassOf(cluster: target, key: key)
        let retained = coupling.Map(value => value > couplingCutoff ? value : 0.0)
        let rowMass = retained.RowSums().ToArray()
        let columnMass = retained.ColumnSums().ToArray()
        from correspondences in coupling.RowCount == source.Vertices.Count && coupling.ColumnCount == target.Vertices.Count
            ? Fin.Succ(CloudCorrespondenceSet.Of(items: toSeq(
                from i in Enumerable.Range(start: 0, count: source.Vertices.Count)
                from j in Enumerable.Range(start: 0, count: target.Vertices.Count)
                let mass = coupling[i, j]
                where mass > couplingCutoff
                let residual = target.Vertices[index: j] - source.Vertices[index: i]
                let squared = residual.SquareLength
                let confidence = Math.Min(val1: 1.0, val2: mass / Math.Max(val1: sourceMass[index: i], val2: targetMass[index: j]))
                select new CloudCorrespondence(SourceIndex: i, TargetIndex: j, SourcePoint: source.Vertices[index: i], TargetPoint: target.Vertices[index: j], Residual: residual, Distance: Math.Sqrt(d: squared), SquaredDistance: squared, SourceMass: Some(sourceMass[index: i]), TargetMass: Some(targetMass[index: j]), CouplingMass: Some(mass), Confidence: RhinoMath.IsValidDouble(x: confidence) ? Some(confidence) : Option<double>.None)), sourceCount: source.Vertices.Count, targetCount: target.Vertices.Count, rowMass: rowMass, columnMass: columnMass))
            : Fin.Fail<CloudCorrespondenceSet>(key.InvalidResult())
        select correspondences;

}
