using Foundation.CSharp.Analyzers.Contracts;
using MathNet.Numerics.Statistics;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0, measure: static (c, k) => CloudKernel.RingNormalOf(ring: c, key: k)), Area = Ring(key: 1, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.Area), key: k)), Perimeter = Ring(key: 2, measure: static (c, k) => k.AcceptValue(value: c.Native.Length)), EdgeAspect = Ring(key: 3, measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance, key: k)), Skewness = Ring(key: 4, measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c, key: k)), Compactness = Ring(key: 5, measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c, key: k)), MomentAnisotropy = Ring(key: 6, measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c, key: k));
    public static readonly VectorCloudMetric RadiiOfGyration = Ring(key: 7, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k)), AreaError = Ring(key: 8, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.AreaError), key: k)), CentroidError = Ring(key: 9, measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidError), key: k));
    public static readonly VectorCloudMetric Centroid = All(key: 10, measure: static (c, k) => CloudKernel.CentroidOf(cloud: c, key: k)), BestFitPlane = All(key: 11, measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c, key: k)), PrincipalAxes = All(key: 12, measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c, key: k)), PrincipalFrame = All(key: 13, measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c, key: k)), Shape = All(key: 14, measure: static (c, k) => CloudKernel.ShapeOf(cloud: c, key: k));
    public static readonly VectorCloudMetric BishopFrames = Cases(key: 15, admitsCase: static cloud => cloud is VectorCloud.PolylineCase or VectorCloud.RingCase, measure: static (cloud, k) => CloudKernel.BishopFramesOf(cloud: cloud, key: k)), TangentFlow = Poly(key: 16, measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k)), CumulativeArcLength = Poly(key: 17, measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k)), EdgeCurvatures = Poly(key: 18, measure: static (pts, k) => CloudKernel.EdgeCurvaturesOf(points: pts, key: k)), OpenLength = Poly(key: 19, measure: static (pts, k) => CloudKernel.OpenLengthOf(points: pts, key: k));
    public static readonly VectorCloudMetric Covariance = Cluster(key: 20, measure: static (cluster, k) => CloudKernel.CovarianceOf(cluster: cluster, key: k).Map(static v => v.Cov)), PrincipalDirection = Cluster(key: 21, measure: static (cluster, k) => CloudKernel.PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => k.AcceptValue(value: CloudKernel.AsVector3d(v: stats.Eigen[0].Eigenvector)))), Spread = Cluster(key: 22, measure: static (cluster, k) => CloudKernel.PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => k.AcceptValue(value: stats.Spread))), OrientedNormals = Cluster(key: 23, measure: static (cloud, k) => CloudKernel.OrientNormalsViaMst(cloud: cloud, key: k)), PrincipalCurvature = Cluster(key: 24, measure: static (cluster, k) => CloudKernel.PrincipalCurvaturesOf(cluster: cluster, key: k)), Curvedness = Cluster(key: 25, measure: static (cluster, k) => CloudKernel.CurvednessOf(cluster: cluster, key: k)), ShapeIndex = Cluster(key: 26, measure: static (cluster, k) => CloudKernel.ShapeIndexOf(cluster: cluster, key: k));
    private static VectorCloudMetric Ring<TOut>(int key, Func<VectorCloud.RingCase, Op, Fin<TOut>> measure) => new(key: key, output: typeof(TOut), admitsCase: static cloud => cloud is VectorCloud.RingCase, measure: (cloud, k) => measure((VectorCloud.RingCase)cloud, k).Map(static v => (object)v!));
    private static VectorCloudMetric Cases<TOut>(int key, Func<VectorCloud, bool> admitsCase, Func<VectorCloud, Op, Fin<TOut>> measure) => new(key: key, output: typeof(TOut), admitsCase: admitsCase, measure: (cloud, k) => measure(cloud, k).Map(static v => (object)v!));
    private static VectorCloudMetric All<TOut>(int key, Func<VectorCloud, Op, Fin<TOut>> measure) => new(key: key, output: typeof(TOut), admitsCase: static _ => true, measure: (cloud, k) => measure(cloud, k).Map(static v => (object)v!));
    private static VectorCloudMetric Poly<TOut>(int key, Func<Seq<Point3d>, Op, Fin<TOut>> measure) => new(key: key, output: typeof(TOut), admitsCase: static cloud => cloud is VectorCloud.PolylineCase, measure: (cloud, k) => measure(((VectorCloud.PolylineCase)cloud).Vertices, k).Map(static v => (object)v!));
    private static VectorCloudMetric Cluster<TOut>(int key, Func<VectorCloud.ClusterCase, Op, Fin<TOut>> measure) => new(key: key, output: typeof(TOut), admitsCase: static cloud => cloud is VectorCloud.ClusterCase, measure: (cloud, k) => measure((VectorCloud.ClusterCase)cloud, k).Map(static v => (object)v!));
    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, Op key);
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        (AdmitsCase(cloud: cloud), Output.Equals(typeof(TOut))) switch {
            (true, true) => Measure(cloud: cloud, key: key).Bind(value => value switch {
                Seq<Vector3d> vectors => ProjectSeq<Vector3d, TOut>(values: vectors, key: key),
                Seq<double> scalars => ProjectSeq<double, TOut>(values: scalars, key: key),
                Seq<Plane> planes => ProjectSeq<Plane, TOut>(values: planes, key: key),
                Seq<(double K1, double K2, Direction E1, Direction E2)> curvatures => curvatures.TraverseM(c => from k1 in key.AcceptValue(value: c.K1) from k2 in key.AcceptValue(value: c.K2) from e1 in key.AcceptValue(value: c.E1.Value) from e2 in key.AcceptValue(value: c.E2.Value) select (k1, k2, c.E1, c.E2)).As().Map(static valid => (TOut)(object)valid),
                VectorCloudShape shape when shape.IsValid => Fin.Succ((TOut)(object)shape),
                VectorCloudShape => Fin.Fail<TOut>(key.InvalidResult()),
                SymmetricMatrix matrix when matrix.IsValid => Fin.Succ((TOut)value),
                SymmetricMatrix => Fin.Fail<TOut>(key.InvalidResult()),
                _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
            }),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
        };
    private static Fin<TOut> ProjectSeq<TItem, TOut>(Seq<TItem> values, Op key) =>
        values.TraverseM(value => key.AcceptValue(value: value)).As().Map(static valid => (TOut)(object)valid);
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SinkhornStopKind { public static readonly SinkhornStopKind BalancedMarginalsConverged = new(key: 0), RelaxedScalingConverged = new(key: 1); }
[SmartEnum<int>]
public sealed partial class SinkhornResidualKind { public static readonly SinkhornResidualKind MarginalMass = new(key: 0), ScalingChange = new(key: 1); }
[SmartEnum<int>]
public sealed partial class SinkhornNumericStatus { public static readonly SinkhornNumericStatus FiniteAccepted = new(key: 0); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance, double Regularization, Option<double> MassRelaxation, bool Debiased, SinkhornResidualKind ResidualKind, SinkhornNumericStatus NumericStatus, double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop, double CouplingMass, int NonZeroCouplings, Option<double> MinPositiveCoupling, Option<double> MaxCoupling, CloudCorrespondenceSet Correspondences);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondence(int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint, Vector3d Residual, double Distance, double SquaredDistance, Option<double> SourceMass, Option<double> TargetMass, Option<double> CouplingMass, Option<double> Confidence);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondenceSet(Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount, int NonZeroCount, double TotalMass, double Rmse, double MedianDistance, double MaxDistance, double Quantile90, double Quantile95) {
    public static CloudCorrespondenceSet Of(Seq<CloudCorrespondence> items, int sourceCount, int targetCount) {
        CloudCorrespondence[] ordered = [.. items.AsIterable()]; double totalMass = ordered.Sum(static item => item.CouplingMass.IfNone(item.SourceMass.IfNone(0.0))); double[] distances = [.. ordered.Select(static item => item.Distance).Order()];
        double denominator = totalMass > RhinoMath.ZeroTolerance ? totalMass : ordered.Length, squared = ordered.Sum(item => item.SquaredDistance * (totalMass > RhinoMath.ZeroTolerance ? item.CouplingMass.IfNone(item.SourceMass.IfNone(0.0)) : 1.0));
        return new CloudCorrespondenceSet(Items: items, SourceCount: sourceCount, TargetCount: targetCount, NonZeroCount: ordered.Length, TotalMass: totalMass, Rmse: ordered.Length > 0 ? Math.Sqrt(d: squared / denominator) : 0.0, MedianDistance: QuantileOrZero(sorted: distances, tau: 0.5), MaxDistance: distances.Length > 0 ? distances[^1] : 0.0, Quantile90: QuantileOrZero(sorted: distances, tau: 0.9), Quantile95: QuantileOrZero(sorted: distances, tau: 0.95));
    }
    private static double QuantileOrZero(double[] sorted, double tau) =>
        sorted.Length == 0 ? 0.0 : SortedArrayStatistics.Quantile(data: sorted, tau: tau);
}
[SmartEnum<int>]
public sealed partial class CloudHullKind { public static readonly CloudHullKind Convex3D = new(key: 0), ConvexFootprint2D = new(key: 1), ConcaveOutline = new(key: 2), AlphaShape = new(key: 3), FootprintWrapper = new(key: 4); }
[SmartEnum<int>]
public sealed partial class CloudHullStatus { public static readonly CloudHullStatus Completed = new(key: 0), Unsupported = new(key: 1); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullReceipt(CloudHullKind Kind, CloudHullStatus Status, int InputCount, int OutputVertexCount, bool NativeRouted, bool Fallback);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudHullResult(Option<Mesh> Mesh, CloudHullReceipt Receipt);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCloudShape(Option<Vector3d> Normal, Option<double> SignedArea, Option<double> Area, Option<double> Perimeter, Option<double> EdgeAspect, Option<double> Skewness, Option<double> PlanarityDeviation, Option<double> Compactness, Option<double> MomentAnisotropy, Option<Vector3d> RadiiOfGyration, Option<double> AreaError, Option<Vector3d> CentroidError, Option<Plane> BestFitPlane, Option<bool> Convex, Option<CurveOrientation> Orientation, Option<double> OpenLength, Option<Vector3d> Spread, Point3d Centroid, Plane PrincipalFrame, Seq<(double Moment, Vector3d Axis)> PrincipalAxes) {
    internal VectorCloudShape(Point3d centroid, Plane principalFrame, Seq<(double Moment, Vector3d Axis)> principalAxes)
        : this(Normal: None, SignedArea: None, Area: None, Perimeter: None, EdgeAspect: None, Skewness: None, PlanarityDeviation: None, Compactness: None, MomentAnisotropy: None, RadiiOfGyration: None, AreaError: None, CentroidError: None, BestFitPlane: None, Convex: None, Orientation: None, OpenLength: None, Spread: None, Centroid: centroid, PrincipalFrame: principalFrame, PrincipalAxes: principalAxes) { }
    internal bool IsValid =>
        Centroid.IsValid
        && PrincipalFrame.IsValid
        && PrincipalAxes.ForAll(static axis => OpAcceptance.ValidityOf(source: axis).IfNone(false))
        && new[] { Normal.Map(static v => v.IsValid && !v.IsTiny()).IfNone(true), SignedArea.Map(static v => RhinoMath.IsValidDouble(x: v)).IfNone(true), Area.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true), Perimeter.Map(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0).IfNone(true),
            EdgeAspect.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(true), Skewness.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true), PlanarityDeviation.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true), Compactness.Map(static v => RhinoMath.IsValidDouble(x: v) && v is >= 0.0 and <= 1.0).IfNone(true), MomentAnisotropy.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(true), RadiiOfGyration.Map(static v => v.IsValid).IfNone(true), AreaError.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true), CentroidError.Map(static v => v.IsValid).IfNone(true),
            BestFitPlane.Map(static v => v.IsValid).IfNone(true), Orientation.Map(static v => v is CurveOrientation.Clockwise or CurveOrientation.CounterClockwise).IfNone(true), OpenLength.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true), Spread.Map(static v => v.IsValid).IfNone(true) }.All(static valid => valid);
}
[Union]
public abstract partial record VectorCloud {
    private VectorCloud() { }
    public sealed record RingCase(Seq<Point3d> Vertices, Polyline Native, Context Tolerance) : VectorCloud;
    public sealed record PolylineCase(Seq<Point3d> Vertices, Context Tolerance) : VectorCloud;
    public sealed record ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass = default) : VectorCloud {
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
                Sphere ball => key.Accept(values: RTree.PointCloudClosestPoints(pointcloud: Indexed, needlePts: [sample], limitDistance: ball.Radius)
                    .FirstOrDefault(defaultValue: []).Where(i => i >= 0 && i < Vertices.Count)),
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
    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Op? key = null) =>
        AdmitPoints(points: points, context: context, key: key, minimum: 1)
            .Map(static admitted => (VectorCloud)new ClusterCase(Vertices: admitted.Points, Tolerance: admitted.Context));
    public static Fin<VectorCloud> WeightedCluster(Seq<Point3d> points, Seq<double> mass, Context context, Op? key = null) =>
        from admitted in AdmitPoints(points: points, context: context, key: key, minimum: 1) from normalized in CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: admitted.Points.Count, key: admitted.Key) select (VectorCloud)new ClusterCase(Vertices: admitted.Points, Tolerance: admitted.Context, Mass: Some(normalized));
    private static Fin<(Seq<Point3d> Points, Context Context, Op Key)> AdmitPoints(Seq<Point3d> points, Context context, Op? key, int minimum) =>
        key.OrDefault() switch {
            Op op => from model in Optional(context).ToFin(op.MissingContext())
                     from valid in points.Traverse(point => AdmitPoint(point: point, key: op)).As().ToFin()
                     from _ in guard(valid.Count >= minimum, op.InvalidInput())
                     select (Points: valid, Context: model, Key: op),
        };
    private static Validation<Error, Point3d> AdmitPoint(Point3d point, Op key) =>
        (RhinoMath.IsValidDouble(x: point.X) && RhinoMath.IsValidDouble(x: point.Y) && RhinoMath.IsValidDouble(x: point.Z)) switch { true => Success<Error, Point3d>(value: point), false => Fail<Error, Point3d>(value: key.InvalidInput()) };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    private const int NormalEstimationNeighbors = 10;
    internal static Fin<Arr<double>> MassOf(VectorCloud.ClusterCase cluster, Op key) =>
        MassOf(mass: cluster.Mass.IfNone(new Arr<double>([.. Enumerable.Repeat(element: 1.0 / cluster.Vertices.Count, count: cluster.Vertices.Count)])), count: cluster.Vertices.Count, key: key);
    internal static Fin<Arr<double>> MassOf(Arr<double> mass, int count, Op key) =>
        (mass.Count, mass.Fold(initialState: 0.0, f: static (total, value) => total + value)) switch {
            (int length, double total) when length == count && mass.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0) && RhinoMath.IsValidDouble(x: total) && total > RhinoMath.ZeroTolerance =>
                Fin.Succ(new Arr<double>([.. mass.AsIterable().Select(value => value / total)])),
            _ => Fin.Fail<Arr<double>>(key.InvalidInput()),
        };
    private static Arr<double> MassOf(VectorCloud.ClusterCase cloud) =>
        cloud.Mass.IfNone(new Arr<double>([.. Enumerable.Repeat(element: 1.0 / cloud.Vertices.Count, count: cloud.Vertices.Count)]));

    // --- [COVARIANCE] -------------------------------------------------------------------
    internal static Fin<(Arr<double> Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Arr<double>> points, Dimension dimension, Op key) =>
        SampleMoment.Of(rows: points, dimension: dimension.Value, key: key)
            .Bind(moment => SymmetricMatrix.Of(dim: dimension, upper: moment.UpperCovariance, key: key)
                .Map(cov => (moment.Mean, Cov: cov)));
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(
            points: points,
            mass: new Arr<double>([.. Enumerable.Repeat(element: 1.0, count: points.Count)]),
            key: key);
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(VectorCloud.ClusterCase cluster, Op key) =>
        from mass in MassOf(cluster: cluster, key: key) from stats in CovarianceOf(points: cluster.Vertices, mass: mass, key: key) select stats;
    private static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Arr<double> mass, Op key) =>
        from moment in SampleMoment.Of(
            rows: points.Map(static point => new Arr<double>([point.X, point.Y, point.Z])),
            dimension: 3,
            key: key,
            weights: Some(mass))
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
            Vector3d v1 => (RL: ReflectAcross(value: rPrev, axis: v1), TL: ReflectAcross(value: tPrev, axis: v1)) switch { (Vector3d, Vector3d) step => ReflectAcross(value: step.Item1, axis: tCurr - step.Item2) },
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
                .Fold(initialState: 0.0, f: (sum, pair) => sum + Vector3d.VectorAngle(v1: pair.V0, v2: pair.V1, vNormal: planeNormal)) / RhinoMath.TwoPI)),
        };
    internal static Fin<TOut> Winding<TOut>(VectorCloud cloud, Point3d query, Op key) =>
        cloud switch {
            VectorCloud.RingCase ring =>
                from normal in RingNormalOf(ring: ring, key: key)
                from winding in PlanarWindingOf(ring: ring.Vertices, planeNormal: normal, query: query, key: key)
                from output in typeof(TOut) == typeof(int)
                    ? Fin.Succ((TOut)(object)winding)
                    : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloud.RingCase), outputType: typeof(TOut)))
                select output,
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(int))),
        };
    // --- [DISPATCH] ---------------------------------------------------------------------
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
                from frame in WithMassPropertiesInternal(curve: curve, context: state.Context, project: static (s, mass) => AxesOf(mass: mass, key: s.Key)
                    .Bind(axes => RingPrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: s.Normal, context: s.Context, key: s.Key)), state: (state.Context, state.Key, oriented.Normal), key: state.Key)
                select frame, key: k),
            polyline: static (polyline, k) => PrincipalStatsOf(points: polyline.Vertices, key: k).Bind(stats => PrincipalFrameOf(stats: stats, context: polyline.Tolerance, key: k)),
            cluster: static (cluster, k) => PrincipalStatsOf(cluster: cluster, key: k).Bind(stats => PrincipalFrameOf(stats: stats, context: cluster.Tolerance, key: k)));
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud, Op key) =>
        CloudCases(cloud: cloud, key: key, ring: static (ring, k) => RingShapeOf(ring: ring, key: k),
            polyline: static (polyline, k) => PointSetShapeOf(points: polyline.Vertices, context: polyline.Tolerance, forPolyline: true, principalStats: PrincipalStatsOf(points: polyline.Vertices, key: k), key: k),
            cluster: static (cluster, k) => PointSetShapeOf(points: cluster.Vertices, context: cluster.Tolerance, forPolyline: false, principalStats: PrincipalStatsOf(cluster: cluster, key: k), key: k));
    // --- [RING] -------------------------------------------------------------------------
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
            from value in WithMassPropertiesInternal(curve: curve, context: state.Context, project: static (s, mass) => AxesOf(mass: mass, key: s.Key)
                .Bind(axes => MomentAnisotropyOf(axes: axes, normal: s.Normal, context: s.Context, key: s.Key)), state: (state.Context, state.Key, oriented.Normal), key: state.Key)
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
    internal static Fin<CloudHullResult> ComputeHullDetailed(VectorCloud source, CloudHullKind kind, Context context, Op key) =>
        source switch {
            VectorCloud.ClusterCase cluster => kind switch {
                CloudHullKind k when k.Equals(CloudHullKind.Convex3D) => ConvexHullOf(cluster: cluster, context: context, key: key),
                CloudHullKind k when k.Equals(CloudHullKind.ConvexFootprint2D) || k.Equals(CloudHullKind.FootprintWrapper) => ConvexFootprintOf(cluster: cluster, kind: kind, key: key),
                CloudHullKind k when k.Equals(CloudHullKind.ConcaveOutline) || k.Equals(CloudHullKind.AlphaShape) => Fin.Succ(new CloudHullResult(Mesh: Option<Mesh>.None, Receipt: new CloudHullReceipt(Kind: kind, Status: CloudHullStatus.Unsupported, InputCount: cluster.Vertices.Count, OutputVertexCount: 0, NativeRouted: false, Fallback: false))),
                _ => Fin.Fail<CloudHullResult>(key.Unsupported(geometryType: typeof(CloudHullKind), outputType: typeof(CloudHullResult))),
            },
            _ => Fin.Fail<CloudHullResult>(error: key.Unsupported(geometryType: source.GetType(), outputType: typeof(CloudHullResult))),
        };
    private static Fin<CloudHullResult> ConvexHullOf(VectorCloud.ClusterCase cluster, Context context, Op key) {
        if (cluster.Vertices.Count < 4 || Point3d.ArePointsCoplanar(points: cluster.Vertices.AsIterable(), tolerance: context.Absolute.Value)) return Fin.Fail<CloudHullResult>(error: key.InvalidInput());
        using Mesh? hull = Mesh.CreateConvexHull3D(points: cluster.Vertices.AsIterable(), hullFacets: out _, tolerance: context.Absolute.Value, angleTolerance: context.Angle.Value);
        return hull is { IsValid: true } ? key.AcceptValue(value: hull.DuplicateMesh()).Map(mesh => new CloudHullResult(Mesh: Some(mesh), Receipt: HullReceipt(kind: CloudHullKind.Convex3D, status: CloudHullStatus.Completed, inputCount: cluster.Vertices.Count, outputVertexCount: mesh.Vertices.Count, nativeRouted: true, fallback: false))) : Fin.Fail<CloudHullResult>(error: key.InvalidResult());
    }
    private static Fin<CloudHullResult> ConvexFootprintOf(VectorCloud.ClusterCase cluster, CloudHullKind kind, Op key) =>
        cluster.Vertices.Count < 3
            ? Fin.Fail<CloudHullResult>(key.InvalidInput())
            : from fit in BestFitOf(points: cluster.Vertices, key: key)
              let hull = ConvexHull2D(points: cluster.Vertices, plane: fit.Plane)
              from mesh in MeshFromFootprint(points: hull, key: key)
              select new CloudHullResult(Mesh: Some(mesh), Receipt: HullReceipt(kind: kind, status: CloudHullStatus.Completed, inputCount: cluster.Vertices.Count, outputVertexCount: mesh.Vertices.Count, nativeRouted: false, fallback: kind.Equals(CloudHullKind.FootprintWrapper)));
    private static CloudHullReceipt HullReceipt(CloudHullKind kind, CloudHullStatus status, int inputCount, int outputVertexCount, bool nativeRouted, bool fallback) =>
        new(Kind: kind, Status: status, InputCount: inputCount, OutputVertexCount: outputVertexCount, NativeRouted: nativeRouted, Fallback: fallback);
    private static Seq<Point3d> ConvexHull2D(Seq<Point3d> points, Plane plane) {
        (Point3d Point, double X, double Y)[] sorted = [.. points.AsIterable()
            .Select(point => {
                _ = plane.ClosestParameter(testPoint: point, s: out double x, t: out double y);
                return (Point: point, X: x, Y: y);
            })
            .OrderBy(static p => p.X)
            .ThenBy(static p => p.Y)];
        return toSeq(ConvexChain(points: sorted).SkipLast(count: 1)
            .Concat(ConvexChain(points: sorted.Reverse()).SkipLast(count: 1))
            .Select(static p => p.Point));
    }
    private static List<(Point3d Point, double X, double Y)> ConvexChain(IEnumerable<(Point3d Point, double X, double Y)> points) =>
        points.Aggregate(seed: new List<(Point3d Point, double X, double Y)>(), func: static (chain, point) => {
            while (chain.Count >= 2 && Cross(o: chain[^2], a: chain[^1], b: point) <= 0.0) chain.RemoveAt(index: chain.Count - 1);
            chain.Add(item: point);
            return chain;
        });
    private static double Cross((Point3d Point, double X, double Y) o, (Point3d Point, double X, double Y) a, (Point3d Point, double X, double Y) b) =>
        ((a.X - o.X) * (b.Y - o.Y)) - ((a.Y - o.Y) * (b.X - o.X));
    [System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "The created Mesh is returned as the successful payload and becomes caller-owned.")]
    private static Fin<Mesh> MeshFromFootprint(Seq<Point3d> points, Op key) =>
        points.Count < 3
            ? Fin.Fail<Mesh>(key.InvalidResult())
            : Optional(Mesh.CreateFromClosedPolyline(polyline: [.. Enumerable.Append(source: points.AsIterable(), element: points[index: 0])])).ToFin(key.InvalidResult())
                .Bind(mesh => new Lease<Mesh>.Owned(Value: mesh).Use(state: key, project: static (op, active) => active.IsValid ? op.AcceptValue(value: active.DuplicateMesh()) : Fin.Fail<Mesh>(op.InvalidResult())));
    // --- [NORMAL_ESTIMATION] -----------------------------------------------------------------
    internal static Fin<Vector3d[]> EstimateNormalsViaCovariance(VectorCloud.ClusterCase target, Op key) =>
        EstimateNormalGraph(target: target, key: key).Map(static graph => graph.Normals);
    internal static Fin<(Vector3d[] Normals, int[][] Neighbors)> EstimateNormalGraph(VectorCloud.ClusterCase target, Op key) =>
        target.Vertices.AsIterable().ToArray() switch {
            Point3d[] points when points.Length >= 3 => RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: points, amount: Math.Min(val1: NormalEstimationNeighbors, val2: points.Length)).ToArray() switch {
                int[][] neighborhoods => EstimateNormalsFromPoints(points: points, neighborhoodOf: i => NeighborhoodOf(points: points, ids: neighborhoods.Length > i ? neighborhoods[i] : [], key: key), key: key)
                    .Map(normals => (Normals: normals, Neighbors: neighborhoods)),
            },
            _ => Fin.Fail<(Vector3d[], int[][])>(key.InvalidInput()),
        };
    // --- [NORMAL_ORIENTATION] ---------------------------------------------------------------
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992: kNN MST weighted by 1 - |n_i dot n_j|.
    internal static Fin<Seq<Vector3d>> OrientNormalsViaMst(VectorCloud cloud, Op key) =>
        cloud switch {
            VectorCloud.ClusterCase cluster => OrientClusterNormals(cluster: cluster, key: key),
            _ => Fin.Fail<Seq<Vector3d>>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(Seq<Vector3d>))),
        };
    private static Fin<Seq<Vector3d>> OrientClusterNormals(VectorCloud.ClusterCase cluster, Op key) {
        int n = cluster.Vertices.Count;
        return n == 0
            ? Fin.Fail<Seq<Vector3d>>(key.InvalidInput())
            : EstimateNormalGraph(target: cluster, key: key).Bind(((Vector3d[] Normals, int[][] Neighbors) graph) => {
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
                        int[] neighbors = graph.Neighbors.Length > curr ? graph.Neighbors[curr] : [];
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
    internal static Fin<Vector3d[]> EstimateNormalsFromPoints(Point3d[] points, Op key) =>
        EstimateNormalsFromPoints(points: points, neighborhoodOf: BatchedNeighborhoods(points: points, key: key), key: key);
    private static Func<int, Fin<Seq<Point3d>>> BatchedNeighborhoods(Point3d[] points, Op key) {
        PointCloud cloud = [];
        cloud.AddRange(points: points);
        int k = Math.Min(val1: NormalEstimationNeighbors, val2: points.Length);
        int[][] ids = [.. RTree.PointCloudKNeighbors(pointcloud: cloud, needlePts: points, amount: k)];
        return i => NeighborhoodOf(points: points, ids: ids.Length > i ? ids[i] : [], key: key);
    }
    private static Fin<Seq<Point3d>> NeighborhoodOf(Point3d[] points, int[] ids, Op key) {
        Seq<Point3d> neighborhood = toSeq(ids.Where(i => i >= 0 && i < points.Length).Select(i => points[i]));
        return ids.Length == 0 || neighborhood.IsEmpty
            ? Fin.Fail<Seq<Point3d>>(key.InvalidResult())
            : Fin.Succ(neighborhood);
    }
    private static Fin<Vector3d[]> EstimateNormalsFromPoints(Point3d[] points, Func<int, Fin<Seq<Point3d>>> neighborhoodOf, Op key) {
        int n = points.Length;
        return n < 3
            ? Fin.Fail<Vector3d[]>(key.InvalidInput())
            : toSeq(Enumerable.Range(start: 0, count: n)).TraverseM(i =>
                from neighborhood in neighborhoodOf(arg: i)
                from normal in neighborhood.Count < 3
                    ? Fin.Fail<Vector3d>(key.InvalidInput())
                    : from stats in CovarianceOf(points: neighborhood, key: key)
                      from eigen in stats.Cov.DecomposeEigen(key: key)
                      from _ in eigen.Count >= 3 && eigen[index: 1].Eigenvalue > RhinoMath.SqrtEpsilon
                          ? Fin.Succ(unit)
                          : Fin.Fail<Unit>(key.InvalidResult())
                      let raw = AsVector3d(v: eigen[index: 2].Eigenvector)
                      from direction in Direction.Of(value: raw, tolerance: RhinoMath.ZeroTolerance, key: key)
                      select direction.Value
                select normal).As().Map(static normals => normals.AsIterable().ToArray());
    }

    // --- [TRANSPORT] ------------------------------------------------------------------------
    internal static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, double regularization, int maxIterations, bool debiased, Option<PositiveMagnitude> massRelaxation, Op key) =>
            (source, target) switch {
                (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => SinkhornCluster<TOut>(source: src, target: tgt, regularization: regularization, maxIterations: maxIterations, debiased: debiased, massRelaxation: massRelaxation, key: key),
                _ => Fin.Fail<TOut>(key.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))),
            };
    private static Fin<TOut> SinkhornCluster<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double regularization, int maxIterations, bool debiased, Option<PositiveMagnitude> massRelaxation, Op key) =>
        source.Vertices.Count < 1 || target.Vertices.Count < 1
            ? Fin.Fail<TOut>(error: key.InvalidInput())
            : from sourceMass in MassOf(cluster: source, key: key)
              from targetMass in MassOf(cluster: target, key: key)
              from plan in SinkhornOt(source: source.Vertices, target: target.Vertices, sourceMass: sourceMass, targetMass: targetMass, reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key)
              from bias in debiased
                  ? from sourceBias in SinkhornOt(source: source.Vertices, target: source.Vertices, sourceMass: sourceMass, targetMass: sourceMass, reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key) from targetBias in SinkhornOt(source: target.Vertices, target: target.Vertices, sourceMass: targetMass, targetMass: targetMass, reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key) select (Source: Some(sourceBias.Distance), Target: Some(targetBias.Distance), Distance: plan.Distance - (0.5 * sourceBias.Distance) - (0.5 * targetBias.Distance))
                  : key.AcceptValue(value: plan.Distance).Map(distance => (Source: Option<double>.None, Target: Option<double>.None, Distance: distance))
              from output in plan.Project<TOut>(source: source, target: target, distance: bias.Distance, sourceBias: bias.Source, targetBias: bias.Target, regularization: regularization, debiased: debiased, massRelaxation: massRelaxation, key: key)
              select output;
    private sealed record SinkhornPlan(double Distance, DenseMatrixD Coupling, double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop) {
        internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance, Option<double> sourceBias, Option<double> targetBias, double regularization, bool debiased, Option<PositiveMagnitude> massRelaxation, Op key) =>
            typeof(TOut) switch {
                Type t when t == typeof(double) => key.AcceptValue(value: distance).Map(static d => (TOut)(object)d),
                Type t when t == typeof(SinkhornReceipt) => Fin.Succ((TOut)(object)ReceiptOf(source: source, target: target, distance: distance, sourceBias: sourceBias, targetBias: targetBias, regularization: regularization, debiased: debiased, massRelaxation: massRelaxation)),
                Type t when t == typeof(CloudCorrespondenceSet) => Fin.Succ((TOut)(object)CouplingCorrespondences(source: source, target: target, coupling: Coupling)),
                Type t when t == typeof(Matrix) => key.AcceptValue(value: MatrixKernel.FromMathNet(m: Coupling, rows: Dimension.Create(value: Coupling.RowCount), cols: Dimension.Create(value: Coupling.ColumnCount))).Map(static matrix => (TOut)(object)matrix),
                Type t when t == typeof(VectorCloud) => toSeq(Enumerable.Range(start: 0, count: source.Vertices.Count)).TraverseM(i => Coupling.Row(i).Sum() switch {
                    double mass when mass > RhinoMath.ZeroTolerance => Fin.Succ((Point: Point3d.Origin + (toSeq(Enumerable.Range(start: 0, count: target.Vertices.Count)).Fold(initialState: Vector3d.Zero, f: (sum, j) => sum + (Coupling[i, j] * (Vector3d)target.Vertices[index: j])) / mass), Mass: mass)),
                    _ => Fin.Fail<(Point3d Point, double Mass)>(key.InvalidResult()),
                }).As().Bind(transported => VectorCloud.WeightedCluster(points: transported.Map(static item => item.Point), mass: transported.Map(static item => item.Mass), context: source.Tolerance, key: key).Map(static cloud => (TOut)(object)cloud)),
                _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloud), outputType: typeof(TOut))),
            };
        private SinkhornReceipt ReceiptOf(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance, Option<double> sourceBias, Option<double> targetBias, double regularization, bool debiased, Option<PositiveMagnitude> massRelaxation) {
            (double couplingMass, int nonZeroCouplings, double minPositiveCoupling, double maxCoupling) = Coupling.Enumerate().Aggregate(
                seed: (Total: 0.0, Nonzero: 0, MinPositive: double.PositiveInfinity, Max: 0.0),
                func: static (s, value) => value > RhinoMath.ZeroTolerance ? (s.Total + value, s.Nonzero + 1, Math.Min(val1: s.MinPositive, val2: value), Math.Max(val1: s.Max, val2: value)) : (s.Total + value, s.Nonzero, s.MinPositive, s.Max));
            return new SinkhornReceipt(
                Distance: distance, RawDistance: Some(Distance), SourceBiasDistance: sourceBias, TargetBiasDistance: targetBias, Regularization: regularization, MassRelaxation: massRelaxation.Map(static value => value.Value), Debiased: debiased,
                ResidualKind: massRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass, NumericStatus: SinkhornNumericStatus.FiniteAccepted,
                SourceConvergenceResidual: SourceConvergenceResidual, TargetConvergenceResidual: TargetConvergenceResidual, Iterations: Iterations, Stop: Stop, CouplingMass: couplingMass, NonZeroCouplings: nonZeroCouplings, MinPositiveCoupling: nonZeroCouplings > 0 ? Some(minPositiveCoupling) : Option<double>.None, MaxCoupling: nonZeroCouplings > 0 ? Some(maxCoupling) : Option<double>.None, Correspondences: CouplingCorrespondences(source: source, target: target, coupling: Coupling));
        }
    }
    private static Fin<SinkhornPlan> SinkhornOt(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, double reg, int maxIter, Option<PositiveMagnitude> massRelaxation, Op key) {
        int m = source.Count; int n = target.Count;
        if (sourceMass.Count != m || targetMass.Count != n || sourceMass.Exists(static value => !RhinoMath.IsValidDouble(x: value) || value <= 0.0) || targetMass.Exists(static value => !RhinoMath.IsValidDouble(x: value) || value <= 0.0) || !RhinoMath.IsValidDouble(x: reg) || reg <= 0.0 || maxIter < 1)
            return Fin.Fail<SinkhornPlan>(key.InvalidInput());
        DenseMatrixD logKernel = DenseMatrixD.OfRowArrays(source.AsIterable().Select(src => target.AsIterable().Select(tgt => -src.DistanceToSquared(other: tgt) / reg).ToArray()));
        DenseVectorD logU = DenseVectorD.Create(m, 0.0);
        DenseVectorD logV = DenseVectorD.Create(n, 0.0);
        DenseVectorD logA = DenseVectorD.OfEnumerable(sourceMass.AsIterable().Select(static value => Math.Log(d: value)));
        DenseVectorD logB = DenseVectorD.OfEnumerable(targetMass.AsIterable().Select(static value => Math.Log(d: value)));
        double exponent = massRelaxation.Match(Some: lambda => lambda.Value / (lambda.Value + reg), None: () => 1.0);
        bool balanced = massRelaxation.IsNone;
        double sourceConvergenceResidual = double.PositiveInfinity;
        double targetConvergenceResidual = double.PositiveInfinity;
        int iterations = 0;
        for (int iter = 0; iter < maxIter && Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) > RhinoMath.SqrtEpsilon; iter++) {
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
        DenseMatrixD coupling = DenseMatrixD.OfRowArrays(Enumerable.Range(start: 0, count: m).Select(i => Enumerable.Range(start: 0, count: n).Select(j => (logU[i] + logKernel[i, j] + logV[j]) switch { double value when value < -745.0 => 0.0, double value => Math.Exp(d: value) }).ToArray()));
        double dist = -reg * coupling.PointwiseMultiply(logKernel).Enumerate().Sum();
        bool numeric = RhinoMath.IsValidDouble(x: dist) && coupling.Enumerate().All(RhinoMath.IsValidDouble);
        return numeric && Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) <= RhinoMath.SqrtEpsilon
            ? Fin.Succ(new SinkhornPlan(Distance: dist, Coupling: coupling, SourceConvergenceResidual: sourceConvergenceResidual, TargetConvergenceResidual: targetConvergenceResidual, Iterations: iterations, Stop: balanced ? SinkhornStopKind.BalancedMarginalsConverged : SinkhornStopKind.RelaxedScalingConverged))
            : Fin.Fail<SinkhornPlan>(key.InvalidResult());
    }
    private static double LogSumExp(int count, Func<int, double> valueAt) =>
        Enumerable.Range(start: 0, count: count).Max(valueAt) switch {
            double.NegativeInfinity => double.NegativeInfinity,
            double max => max + Math.Log(d: Enumerable.Range(start: 0, count: count).Sum(i => Math.Exp(d: valueAt(arg: i) - max))),
        };
    private static (double Source, double Target) MarginalResiduals(DenseMatrixD logKernel, DenseVectorD logU, DenseVectorD logV, Arr<double> sourceMass, Arr<double> targetMass) {
        DenseMatrixD coupling = DenseMatrixD.OfRowArrays(Enumerable.Range(start: 0, count: logKernel.RowCount).Select(i => Enumerable.Range(start: 0, count: logKernel.ColumnCount).Select(j => Math.Exp(d: logU[i] + logKernel[i, j] + logV[j])).ToArray()));
        return (
            Source: Enumerable.Range(start: 0, count: coupling.RowCount).Max(i => Math.Abs(value: coupling.Row(i).Sum() - sourceMass[index: i])),
            Target: Enumerable.Range(start: 0, count: coupling.ColumnCount).Max(j => Math.Abs(value: coupling.Column(j).Sum() - targetMass[index: j])));
    }
    private static (double Source, double Target) ScalingResiduals(double[] previousU, DenseVectorD logU, double[] previousV, DenseVectorD logV) =>
        (Source: Enumerable.Range(start: 0, count: logU.Count).Max(i => Math.Abs(value: logU[i] - previousU[i])),
            Target: Enumerable.Range(start: 0, count: logV.Count).Max(i => Math.Abs(value: logV[i] - previousV[i])));
    internal static CloudCorrespondenceSet CouplingCorrespondences(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, DenseMatrixD coupling) =>
        (SourceMass: MassOf(cloud: source), TargetMass: MassOf(cloud: target)) switch {
            (Arr<double> sourceMass, Arr<double> targetMass) => CloudCorrespondenceSet.Of(items: toSeq(
                from i in Enumerable.Range(start: 0, count: source.Vertices.Count)
                from j in Enumerable.Range(start: 0, count: target.Vertices.Count)
                let mass = coupling[i, j]
                where mass > RhinoMath.ZeroTolerance
                let residual = target.Vertices[index: j] - source.Vertices[index: i]
                let squared = residual.SquareLength
                select new CloudCorrespondence(SourceIndex: i, TargetIndex: j, SourcePoint: source.Vertices[index: i], TargetPoint: target.Vertices[index: j], Residual: residual, Distance: Math.Sqrt(d: squared), SquaredDistance: squared, SourceMass: Some(sourceMass[index: i]), TargetMass: Some(targetMass[index: j]), CouplingMass: Some(mass), Confidence: Option<double>.None)), sourceCount: source.Vertices.Count, targetCount: target.Vertices.Count),
        };
    // --- [PRINCIPAL_CURVATURE] -------------------------------------------------------------
    // Local quadric fit in PCA tangent frame; II = [[2a, b], [b, 2c]] yields signed curvatures.
    internal static Fin<Seq<(double K1, double K2, Direction E1, Direction E2)>> PrincipalCurvaturesOf(VectorCloud.ClusterCase cluster, Op key) =>
        (cluster.Vertices.Count, cluster.Vertices.AsIterable().ToArray()) switch {
            (int n, Point3d[] points) when n >= 6 => RTree.PointCloudKNeighbors(pointcloud: cluster.Indexed, needlePts: points, amount: Math.Min(val1: NormalEstimationNeighbors, val2: n)).ToArray() switch {
                int[][] neighborhoods => toSeq(Enumerable.Range(start: 0, count: n)).TraverseM(i =>
                    from neighborhood in NeighborhoodOf(points: points, ids: neighborhoods.Length > i ? neighborhoods[i] : [], key: key)
                    from stats in neighborhood.Count < 6 ? Fin.Fail<PrincipalStats>(error: key.InvalidInput()) : PrincipalStatsOf(points: neighborhood, key: key)
                    from normal in Direction.Of(value: AsVector3d(v: stats.Eigen[index: 2].Eigenvector), tolerance: RhinoMath.ZeroTolerance, key: key)
                    from uAxis in Direction.Of(value: AsVector3d(v: stats.Eigen[index: 0].Eigenvector), tolerance: RhinoMath.ZeroTolerance, key: key)
                    from vAxis in Direction.Of(value: Vector3d.CrossProduct(a: normal.Value, b: uAxis.Value), tolerance: RhinoMath.ZeroTolerance, key: key)
                    from fit in QuadraticFit(center: points[i], neighborhood: neighborhood, uAxis: uAxis.Value, vAxis: vAxis.Value, normal: normal.Value, key: key)
                    from output in ShapeOperatorEigen(a: fit.A, b: fit.B, c: fit.C, uAxis: uAxis.Value, vAxis: vAxis.Value, key: key)
                    select output).As(),
            },
            _ => Fin.Fail<Seq<(double, double, Direction, Direction)>>(error: key.InvalidInput()),
        };
    private static Fin<(double A, double B, double C)> QuadraticFit(Point3d center, Seq<Point3d> neighborhood, Vector3d uAxis, Vector3d vAxis, Vector3d normal, Op key) {
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
        return Matrix.Of(rows: Dimension.Create(value: m), cols: Dimension.Create(value: 6), entries: new Arr<double>(designFlat), key: key)
            .Bind(design => design.LeastSquaresDetailed(rhs: new Arr<double>(rhs), key: key))
            .Bind(receipt => receipt.FullRank.IfNone(false)
                && receipt.Solution.Count == 6
                && receipt.Solution.ForAll(RhinoMath.IsValidDouble)
                ? Fin.Succ((A: receipt.Solution[0], B: receipt.Solution[1], C: receipt.Solution[2]))
                : Fin.Fail<(double A, double B, double C)>(key.InvalidResult()));
    }
    // Closed-form 2x2 symmetric eigen-decomposition. II = [[2a, b], [b, 2c]] -> (k1, k2, e1, e2)
    // with e1, e2 lifted into world via (uAxis, vAxis).
    private static Fin<(double K1, double K2, Direction E1, Direction E2)> ShapeOperatorEigen(double a, double b, double c, Vector3d uAxis, Vector3d vAxis, Op key) {
        double s11 = 2.0 * a; double s22 = 2.0 * c; double s12 = b;
        double trace = s11 + s22, disc = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (trace * trace) - (4.0 * ((s11 * s22) - (s12 * s12)))));
        double k1 = 0.5 * (trace + disc), k2 = 0.5 * (trace - disc);
        double angle = Math.Abs(value: s12) > RhinoMath.SqrtEpsilon
            ? Math.Atan2(y: k1 - s11, x: s12)
            : s22 > s11 ? Math.PI / 2.0 : 0.0;
        Vector3d e1World = (Math.Cos(d: angle) * uAxis) + (Math.Sin(a: angle) * vAxis);
        Vector3d e2World = (-Math.Sin(a: angle) * uAxis) + (Math.Cos(d: angle) * vAxis);
        return from e1 in Direction.Of(value: e1World, tolerance: RhinoMath.ZeroTolerance, key: key)
               from e2 in Direction.Of(value: e2World, tolerance: RhinoMath.ZeroTolerance, key: key)
               select (K1: k1, K2: k2, E1: e1, E2: e2);
    }
    private static Fin<Seq<double>> CurvatureScalars(VectorCloud.ClusterCase cluster, Op key, Func<(double K1, double K2, Direction E1, Direction E2), double> project) =>
        PrincipalCurvaturesOf(cluster: cluster, key: key).Map(curvatures => toSeq(curvatures.AsIterable().Select(project)));
    internal static Fin<Seq<double>> CurvednessOf(VectorCloud.ClusterCase cluster, Op key) =>
        CurvatureScalars(cluster: cluster, key: key, project: static c => Math.Sqrt(d: 0.5 * ((c.K1 * c.K1) + (c.K2 * c.K2))));
    // Koenderink-van Doorn 1992 shape index in [-1, 1]: cup, trough, saddle, ridge, cap.
    internal static Fin<Seq<double>> ShapeIndexOf(VectorCloud.ClusterCase cluster, Op key) =>
        CurvatureScalars(cluster: cluster, key: key, project: static c => (Diff: c.K1 - c.K2, Sum: c.K1 + c.K2) switch { (double diff, double sum) => Math.Abs(value: diff) < RhinoMath.SqrtEpsilon ? Math.Sign(value: sum) : 2.0 / Math.PI * Math.Atan2(y: sum, x: diff) });
}
