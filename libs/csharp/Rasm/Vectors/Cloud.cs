using Foundation.CSharp.Analyzers.Contracts;
using MathNet.Numerics.Statistics;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0, output: typeof(Vector3d), measure: static (c, k) => CloudKernel.RingNormalOf(ring: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Area = Ring(key: 1, output: typeof(double), measure: static (c, k) => CloudKernel.RingAreaOf(ring: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Perimeter = Ring(key: 2, output: typeof(double), measure: static (c, k) => k.AcceptValue(value: c.Native.Length).Map(static v => (object)v));
    public static readonly VectorCloudMetric EdgeAspect = Ring(key: 3, output: typeof(double), measure: static (c, k) => CloudKernel.EdgeAspectOf(native: c.Native, context: c.Tolerance, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Skewness = Ring(key: 4, output: typeof(double), measure: static (c, k) => CloudKernel.RingSkewnessOf(ring: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Compactness = Ring(key: 5, output: typeof(double), measure: static (c, k) => CloudKernel.RingCompactnessOf(ring: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric MomentAnisotropy = Ring(key: 6, output: typeof(double), measure: static (c, k) => CloudKernel.RingMomentAnisotropyOf(ring: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric RadiiOfGyration = Ring(key: 7, output: typeof(Vector3d), measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric AreaError = Ring(key: 8, output: typeof(double), measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.AreaError), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric CentroidError = Ring(key: 9, output: typeof(Vector3d), measure: static (c, k) => CloudKernel.WithMassProperties(ring: c, project: static (op, props) => op.AcceptValue(value: props.CentroidError), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Centroid = All(key: 10, output: typeof(Point3d), measure: static (c, k) => CloudKernel.CentroidOf(cloud: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric BestFitPlane = All(key: 11, output: typeof(Plane), measure: static (c, k) => CloudKernel.BestFitPlaneOf(cloud: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric PrincipalAxes = All(key: 12, output: typeof(Seq<Vector3d>), measure: static (c, k) => CloudKernel.PrincipalAxesOf(cloud: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric PrincipalFrame = All(key: 13, output: typeof(Plane), measure: static (c, k) => CloudKernel.PrincipalFrameOf(cloud: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Shape = All(key: 14, output: typeof(VectorCloudShape), measure: static (c, k) => CloudKernel.ShapeOf(cloud: c, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric BishopFrames = new(key: 15, output: typeof(Seq<Plane>),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase or VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.BishopFramesOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric TangentFlow = Poly(key: 16, output: typeof(Seq<Vector3d>), measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric CumulativeArcLength = Poly(key: 17, output: typeof(Seq<double>), measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric EdgeCurvatures = Poly(key: 18, output: typeof(Seq<double>), measure: static (pts, k) => CloudKernel.EdgeCurvaturesOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric OpenLength = Poly(key: 19, output: typeof(double), measure: static (pts, k) => CloudKernel.OpenLengthOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Covariance = Cluster(key: 20, output: typeof(SymmetricMatrix), measure: static (pts, k) => CloudKernel.CovarianceOf(points: pts, key: k).Map(static v => (object)v.Cov));
    public static readonly VectorCloudMetric PrincipalDirection = Cluster(key: 21, output: typeof(Vector3d), measure: static (pts, k) => CloudKernel.PrincipalDirectionOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Spread = Cluster(key: 22, output: typeof(Vector3d), measure: static (pts, k) => CloudKernel.SpreadOf(points: pts, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric OrientedNormals = new(key: 23, output: typeof(Seq<Vector3d>),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.OrientNormalsViaMst(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric PrincipalCurvature = new(key: 24, output: typeof(Seq<(double K1, double K2, Direction E1, Direction E2)>),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.PrincipalCurvaturesOf(cluster: (VectorCloud.ClusterCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Curvedness = new(key: 25, output: typeof(Seq<double>),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.CurvednessOf(cluster: (VectorCloud.ClusterCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric ShapeIndex = new(key: 26, output: typeof(Seq<double>),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.ShapeIndexOf(cluster: (VectorCloud.ClusterCase)cloud, key: k).Map(static v => (object)v));
    private static VectorCloudMetric Ring(int key, Type output, Func<VectorCloud.RingCase, Op, Fin<object>> measure) =>
        new(key: key, output: output, admitsCase: static cloud => cloud is VectorCloud.RingCase, measure: (cloud, k) => measure((VectorCloud.RingCase)cloud, k));
    private static VectorCloudMetric All(int key, Type output, Func<VectorCloud, Op, Fin<object>> measure) =>
        new(key: key, output: output, admitsCase: static _ => true, measure: measure);
    private static VectorCloudMetric Poly(int key, Type output, Func<Seq<Point3d>, Op, Fin<object>> measure) =>
        new(key: key, output: output, admitsCase: static cloud => cloud is VectorCloud.PolylineCase, measure: (cloud, k) => measure(((VectorCloud.PolylineCase)cloud).Vertices, k));
    private static VectorCloudMetric Cluster(int key, Type output, Func<Seq<Point3d>, Op, Fin<object>> measure) =>
        new(key: key, output: output, admitsCase: static cloud => cloud is VectorCloud.ClusterCase, measure: (cloud, k) => measure(((VectorCloud.ClusterCase)cloud).Vertices, k));
    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial bool AdmitsCase(VectorCloud cloud);
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorCloud cloud, Op key);
    internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
        AdmitsCase(cloud: cloud) switch {
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
            true => Output.Equals(typeof(TOut)) switch {
                true => Measure(cloud: cloud, key: key).Bind(value => value switch {
                    Seq<Vector3d> vectors => vectors.TraverseM(v => key.AcceptValue(value: v)).As().Map(static valid => (TOut)(object)valid),
                    Seq<double> scalars => scalars.TraverseM(s => key.AcceptValue(value: s)).As().Map(static valid => (TOut)(object)valid),
                    Seq<Plane> planes => planes.TraverseM(p => key.AcceptValue(value: p)).As().Map(static valid => (TOut)(object)valid),
                    Seq<(double K1, double K2, Direction E1, Direction E2)> curvatures => curvatures.TraverseM(c =>
                        from k1 in key.AcceptValue(value: c.K1)
                        from k2 in key.AcceptValue(value: c.K2)
                        from e1 in key.AcceptValue(value: c.E1.Value)
                        from e2 in key.AcceptValue(value: c.E2.Value)
                        select (k1, k2, c.E1, c.E2)).As().Map(static valid => (TOut)(object)valid),
                    VectorCloudShape shape when shape.IsValid => Fin.Succ((TOut)(object)shape),
                    VectorCloudShape => Fin.Fail<TOut>(key.InvalidResult()),
                    SymmetricMatrix matrix when matrix.IsValid => Fin.Succ((TOut)value),
                    SymmetricMatrix => Fin.Fail<TOut>(key.InvalidResult()),
                    _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
                }),
                false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
            },
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SinkhornStopKind {
    public static readonly SinkhornStopKind BalancedMarginalsConverged = new(key: 0);
    public static readonly SinkhornStopKind RelaxedScalingConverged = new(key: 1);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(
    double Distance,
    double SourceConvergenceResidual,
    double TargetConvergenceResidual,
    int Iterations,
    SinkhornStopKind Stop);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCloudShape(
    Option<Vector3d> Normal,
    Option<double> SignedArea,
    Option<double> Area,
    Option<double> Perimeter,
    Option<double> EdgeAspect,
    Option<double> Skewness,
    Option<double> PlanarityDeviation,
    Option<double> Compactness,
    Option<double> MomentAnisotropy,
    Option<Vector3d> RadiiOfGyration,
    Option<double> AreaError,
    Option<Vector3d> CentroidError,
    Option<Plane> BestFitPlane,
    Option<bool> Convex,
    Option<CurveOrientation> Orientation,
    Option<double> OpenLength,
    Option<Vector3d> Spread,
    Point3d Centroid,
    Plane PrincipalFrame,
    Seq<(double Moment, Vector3d Axis)> PrincipalAxes) {
    internal bool IsValid =>
        Centroid.IsValid
        && PrincipalFrame.IsValid
        && PrincipalAxes.ForAll(static axis => OpAcceptance.ValidityOf(source: axis).IfNone(false))
        && Normal.Map(static v => v.IsValid && !v.IsTiny()).IfNone(true)
        && SignedArea.Map(static v => RhinoMath.IsValidDouble(x: v)).IfNone(true)
        && Area.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true)
        && Perimeter.Map(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0).IfNone(true)
        && EdgeAspect.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(true)
        && Skewness.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true)
        && PlanarityDeviation.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true)
        && Compactness.Map(static v => RhinoMath.IsValidDouble(x: v) && v is >= 0.0 and <= 1.0).IfNone(true)
        && MomentAnisotropy.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 1.0).IfNone(true)
        && RadiiOfGyration.Map(static v => v.IsValid).IfNone(true)
        && AreaError.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true)
        && CentroidError.Map(static v => v.IsValid).IfNone(true)
        && BestFitPlane.Map(static v => v.IsValid).IfNone(true)
        && Orientation.Map(static v => v is CurveOrientation.Clockwise or CurveOrientation.CounterClockwise).IfNone(true)
        && OpenLength.Map(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0).IfNone(true)
        && Spread.Map(static v => v.IsValid).IfNone(true);
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
        internal Fin<Seq<int>> WithinRadius(Point3d sample, double radius, Op key) {
            Sphere ball = new(center: sample, radius: radius);
            return ball.IsValid switch {
                false => Fin.Fail<Seq<int>>(error: key.InvalidInput()),
                true => key.AcceptValue(value: toSeq(
                    RTree.PointCloudClosestPoints(pointcloud: Indexed, needlePts: [sample], limitDistance: ball.Radius)
                        .FirstOrDefault(defaultValue: [])
                        .Where(i => i >= 0 && i < Vertices.Count))),
            };
        }
    }
    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => AdmitPoint(point: point, key: op)).As().ToFin()
               let closed = valid.Count > 1 && valid[0].EpsilonEquals(other: valid[valid.Count - 1], epsilon: model.Absolute.Value)
               let vertices = closed ? valid.Init : valid
               from _ in guard(vertices.Count >= 3, op.InvalidInput())
               let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
               from __ in guard(native.IsValid && native.IsClosedWithinTolerance(model.Absolute.Value) && native.SegmentCount >= 3, op.InvalidInput())
               from ___ in Optional(native.ToPolylineCurve()).ToFin(op.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(
                   state: (Context: model, Key: op),
                   project: static (state, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: state.Context.Absolute.Value)).ToFin(state.Key.InvalidResult())
                       .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(state.Key.InvalidInput()))))
               select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: model);
    }
    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => AdmitPoint(point: point, key: op)).As().ToFin()
               from _ in guard(valid.Count >= 2, op.InvalidInput())
               select (VectorCloud)new PolylineCase(Vertices: valid, Tolerance: model);
    }
    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => AdmitPoint(point: point, key: op)).As().ToFin()
               from _ in guard(valid.Count >= 1, op.InvalidInput())
               select (VectorCloud)new ClusterCase(Vertices: valid, Tolerance: model);
    }
    public static Fin<VectorCloud> WeightedCluster(Seq<Point3d> points, Seq<double> mass, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => AdmitPoint(point: point, key: op)).As().ToFin()
               from _ in guard(valid.Count >= 1, op.InvalidInput())
               from normalized in NormalizeMass(mass: mass, count: valid.Count, key: op)
               select (VectorCloud)new ClusterCase(Vertices: valid, Tolerance: model, Mass: Some(normalized));
    }
    private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, int count, Op key) {
        double[] values = [.. mass.AsIterable()];
        double total = values.Sum();
        return values.Length == count
            && values.All(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0)
            && RhinoMath.IsValidDouble(x: total)
            && total > RhinoMath.ZeroTolerance
            ? Fin.Succ(new Arr<double>([.. values.Select(value => value / total)]))
            : Fin.Fail<Arr<double>>(key.InvalidInput());
    }
    private static Validation<Error, Point3d> AdmitPoint(Point3d point, Op key) =>
        RhinoMath.IsValidDouble(x: point.X) && RhinoMath.IsValidDouble(x: point.Y) && RhinoMath.IsValidDouble(x: point.Z)
            ? Success<Error, Point3d>(value: point)
            : Fail<Error, Point3d>(value: key.InvalidInput());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    private const int NormalEstimationNeighbors = 10;
    private static Arr<double> MassOf(VectorCloud.ClusterCase cloud) =>
        cloud.Mass.IfNone(new Arr<double>([.. Enumerable.Repeat(element: 1.0 / cloud.Vertices.Count, count: cloud.Vertices.Count)]));

    // --- [RAILS] ------------------------------------------------------------------------
    internal static Fin<T> WithCaseRails<T>(
        VectorCloud cloud,
        Func<VectorCloud.RingCase, Op, Fin<T>> ringRail,
        Func<Seq<Point3d>, Context, Op, Fin<T>> pointRail,
        Op key) =>
        cloud.Switch(
            state: (RingRail: ringRail, PointRail: pointRail, Key: key),
            ringCase: static (s, c) => s.RingRail(arg1: c, arg2: s.Key),
            polylineCase: static (s, c) => s.PointRail(arg1: c.Vertices, arg2: c.Tolerance, arg3: s.Key),
            clusterCase: static (s, c) => s.PointRail(arg1: c.Vertices, arg2: c.Tolerance, arg3: s.Key));

    // --- [COVARIANCE] -------------------------------------------------------------------
    internal static Fin<(Arr<double> Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Arr<double>> points, Dimension dimension, Op key) {
        int dim = dimension.Value;
        Arr<Arr<double>> rows = [.. points.AsIterable()];
        return rows.IsEmpty || rows.Exists(point => point.Count != dim || !point.ForAll(RhinoMath.IsValidDouble))
            ? Fin.Fail<(Arr<double>, SymmetricMatrix)>(key.InvalidInput())
            : key.Catch(() => {
                double[][] columns = [.. Enumerable.Range(start: 0, count: dim).Select(j => rows.Map(row => row[j]).ToArray())];
                double[] mean = [.. columns.Select(static values => ArrayStatistics.Mean(data: values))];
                double[] upper = [.. Enumerable.Range(start: 0, count: dim)
                    .SelectMany(i => Enumerable.Range(start: i, count: dim - i).Select(j => ArrayStatistics.PopulationCovariance(population1: columns[i], population2: columns[j])))];
                return SymmetricMatrix.Of(dim: dimension, upper: new Arr<double>(upper), key: key)
                    .Map(cov => (Mean: new Arr<double>(mean), Cov: cov));
            });
    }
    internal static Fin<(Vector3d Mean, SymmetricMatrix Cov)> CovarianceOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(
            points: points.Map(static p => new Arr<double>([p.X, p.Y, p.Z])),
            dimension: Dimension.Create(value: 3),
            key: key)
            .Map(static result => (AsVector3d(v: result.Mean), result.Cov));
    internal static Vector3d AsVector3d(Arr<double> v) => new(x: v[0], y: v[1], z: v[2]);

    // --- [BISHOP] -----------------------------------------------------------------------
    internal static Fin<Seq<Plane>> BishopFramesOf(VectorCloud cloud, Op key) =>
        cloud.Switch(
            state: key,
            ringCase: static (k, ring) => RingNormalOf(ring: ring, key: k)
                .Bind(normal => Direction.Of(value: normal, context: ring.Tolerance, key: k))
                .Bind(initialNormal => BishopChainOf(points: ring.Vertices, initialNormal: initialNormal, closed: true, context: ring.Tolerance, key: k)),
            polylineCase: static (k, polyline) => polyline.Vertices.Count < 2
                ? Fin.Fail<Seq<Plane>>(k.InvalidInput())
                : Direction.Of(value: VectorFrame.SeedPerpendicular(axis: polyline.Vertices[1] - polyline.Vertices[0]), context: polyline.Tolerance, key: k)
                    .Bind(initialNormal => BishopChainOf(points: polyline.Vertices, initialNormal: initialNormal, closed: false, context: polyline.Tolerance, key: k)),
            clusterCase: static (k, c) => Fin.Fail<Seq<Plane>>(k.Unsupported(geometryType: c.GetType(), outputType: typeof(Seq<Plane>))));
    internal static Fin<Seq<Plane>> BishopChainOf(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op key) =>
        points.Count switch {
            < 2 => Fin.Fail<Seq<Plane>>(key.InvalidInput()),
            _ => InitialBishop(points: points, initialNormal: initialNormal, context: context, key: key)
                .Bind(initial => toSeq(Enumerable.Range(start: 1, count: points.Count - 1)).Fold(
                    initialState: Fin.Succ((Frames: Seq(initial.Frame), initial.R, initial.T)),
                    f: (acc, i) => acc.Bind(state => StepBishop(state: state, points: points, index: i, context: context, key: key))))
                .Bind(result => closed
                    ? RedistributeClosureTwist(frames: result.Frames, key: key)
                    : Fin.Succ(result.Frames)),
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
                Vector3d rNew when rNew.IsValid && !rNew.IsTiny() =>
                    VectorFrame.Of(origin: points[index], normal: tCurrRaw, xHint: Some(rNew), context: context, key: key)
                        .Map(frame => (Frames: state.Frames.Add(frame.Value), R: rNew, T: tCurrRaw)),
                _ => Fin.Fail<(Seq<Plane>, Vector3d, Vector3d)>(key.InvalidResult()),
            },
        };
    }
    internal static Vector3d DoubleReflect(Vector3d rPrev, Vector3d tPrev, Vector3d tCurr) =>
        (tCurr - tPrev) switch {
            Vector3d v1 when v1.IsTiny() => rPrev,
            Vector3d v1 => (RL: ReflectAcross(value: rPrev, axis: v1), TL: ReflectAcross(value: tPrev, axis: v1)) switch {
                (Vector3d, Vector3d) step => ReflectAcross(value: step.Item1, axis: tCurr - step.Item2),
            },
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
    internal static Fin<Point3d> CentroidOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => WithMassProperties(ring: ring, project: static (op, props) => op.AcceptValue(value: props.Centroid), key: k),
            pointRail: static (points, _, k) => CovarianceOf(points: points, key: k).Bind(stats => k.AcceptValue(value: (Point3d)stats.Mean)));
    internal static Fin<Plane> BestFitPlaneOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => BestFitOf(points: ring.Vertices, key: k).Map(static fit => fit.Plane),
            pointRail: static (points, _, k) => BestFitOf(points: points, key: k).Map(static fit => fit.Plane));
    internal static Fin<Seq<Vector3d>> PrincipalAxesOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => WithMassProperties(ring: ring,
                project: static (op, props) => AxesOf(mass: props, key: op).Bind(axes => axes.Map(static a => a.Axis).TraverseM(a => op.AcceptValue(value: a)).As()),
                key: k),
            pointRail: static (points, _, k) => CovarianceOf(points: points, key: k)
                .Bind(stats => stats.Cov.DecomposeEigen(key: k))
                .Bind(eigen => eigen.Map(static p => AsVector3d(v: p.Eigenvector)).TraverseM(v => k.AcceptValue(value: v)).As()));
    internal static Fin<Plane> PrincipalFrameOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => WithRingCurve(ring: ring, project: static (state, curve) =>
                from oriented in RingOrientationOf(curve: curve, context: state.Context, key: state.Key)
                from frame in WithMassPropertiesInternal(
                    curve: curve, context: state.Context,
                    project: static (s, mass) => AxesOf(mass: mass, key: s.Key)
                        .Bind(axes => RingPrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: s.Normal, context: s.Context, key: s.Key)),
                    state: (state.Context, state.Key, oriented.Normal),
                    key: state.Key)
                select frame, key: k),
            pointRail: static (points, ctx, k) => CovarianceOf(points: points, key: k)
                .Bind(stats => stats.Cov.DecomposeEigen(key: k)
                    .Bind(eigen => VectorFrame.Of(origin: (Point3d)stats.Mean, normal: AsVector3d(v: eigen[2].Eigenvector), xHint: Some(AsVector3d(v: eigen[0].Eigenvector)), context: ctx, key: k))
                    .Bind(frame => frame.Project<Plane>(key: k))));
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud, Op key) =>
        cloud.Switch(
            state: key,
            ringCase: static (k, ring) => RingShapeOf(ring: ring, key: k),
            polylineCase: static (k, polyline) => PointSetShapeOf(points: polyline.Vertices, context: polyline.Tolerance, forPolyline: true, key: k),
            clusterCase: static (k, cluster) => PointSetShapeOf(points: cluster.Vertices, context: cluster.Tolerance, forPolyline: false, key: k));
    internal static Fin<Vector3d> PrincipalDirectionOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, key: key)
            .Bind(stats => stats.Cov.DecomposeEigen(key: key))
            .Bind(eigen => key.AcceptValue(value: AsVector3d(v: eigen[0].Eigenvector)));
    internal static Fin<Vector3d> SpreadOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, key: key)
            .Bind(stats => stats.Cov.DecomposeEigen(key: key))
            .Bind(eigen => key.AcceptValue(value: new Vector3d(eigen[0].Eigenvalue, eigen[1].Eigenvalue, eigen[2].Eigenvalue)));

    // --- [RING] -------------------------------------------------------------------------
    internal static Fin<Vector3d> RingNormalOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            RingOrientationOf(curve: curve, context: state.Context, key: state.Key).Map(static o => o.Normal), key: key);
    internal static Fin<double> RingAreaOf(VectorCloud.RingCase ring, Op key) =>
        WithMassProperties(ring: ring, project: static (op, props) => op.AcceptValue(value: props.Area), key: key);
    internal static Fin<double> RingCompactnessOf(VectorCloud.RingCase ring, Op key) {
        double perimeter = ring.Native.Length;
        return RingAreaOf(ring: ring, key: key).Bind(area => CompactnessOf(area: area, perimeter: perimeter, key: key));
    }
    internal static Fin<double> RingMomentAnisotropyOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            from oriented in RingOrientationOf(curve: curve, context: state.Context, key: state.Key)
            from value in WithMassPropertiesInternal(
                curve: curve, context: state.Context,
                project: static (s, mass) => AxesOf(mass: mass, key: s.Key)
                    .Bind(axes => MomentAnisotropyOf(axes: axes, normal: s.Normal, context: s.Context, key: s.Key)),
                state: (state.Context, state.Key, oriented.Normal),
                key: state.Key)
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
                let shape = new VectorCloudShape(
                    Normal: Some(oriented.Normal),
                    SignedArea: Some(oriented.Orientation == CurveOrientation.CounterClockwise ? mass.Area : -mass.Area),
                    Area: Some(mass.Area),
                    Perimeter: Some(s.Native.Length),
                    EdgeAspect: Some(edgeAspect),
                    Skewness: Some(skewness),
                    PlanarityDeviation: Some(fit.Deviation),
                    Compactness: Some(compactness),
                    MomentAnisotropy: Some(anisotropy),
                    RadiiOfGyration: Some(radii),
                    AreaError: Some(areaError),
                    CentroidError: Some(centroidError),
                    BestFitPlane: Some(fit.Plane),
                    Convex: Some(s.Native.IsConvexLoop(strictlyConvex: false)),
                    Orientation: Some(oriented.Orientation),
                    OpenLength: None,
                    Spread: None,
                    Centroid: mass.Centroid,
                    PrincipalFrame: principal,
                    PrincipalAxes: axes)
                from valid in shape.IsValid ? Fin.Succ(shape) : Fin.Fail<VectorCloudShape>(s.Key.InvalidResult())
                select valid,
            state: (state.Vertices, state.Native, state.Context, state.Key, Curve: curve),
            key: state.Key), key: key);
    private static Fin<VectorCloudShape> PointSetShapeOf(Seq<Point3d> points, Context context, bool forPolyline, Op key) =>
        from openLen in forPolyline ? OpenLengthOf(points: points, key: key).Map(Some) : Fin.Succ(Option<double>.None)
        from fit in forPolyline
            ? BestFitOf(points: points, key: key).Map(static f => (Plane: Some(f.Plane), Deviation: Some(f.Deviation)))
            : Fin.Succ((Plane: Option<Plane>.None, Deviation: Option<double>.None))
        from stats in CovarianceOf(points: points, key: key)
        from eigen in stats.Cov.DecomposeEigen(key: key)
        let axes = eigen.Map(static p => (Moment: p.Eigenvalue, Axis: AsVector3d(v: p.Eigenvector)))
        from principal in VectorFrame.Of(origin: (Point3d)stats.Mean, normal: AsVector3d(v: eigen[2].Eigenvector), xHint: Some(AsVector3d(v: eigen[0].Eigenvector)), context: context, key: key)
            .Bind(frame => frame.Project<Plane>(key: key))
        let shape = new VectorCloudShape(
            Normal: None, SignedArea: None, Area: None, Perimeter: None,
            EdgeAspect: None, Skewness: None,
            PlanarityDeviation: fit.Deviation,
            Compactness: None, MomentAnisotropy: None,
            RadiiOfGyration: None, AreaError: None, CentroidError: None,
            BestFitPlane: fit.Plane,
            Convex: None, Orientation: None,
            OpenLength: openLen,
            Spread: Some(new Vector3d(eigen[0].Eigenvalue, eigen[1].Eigenvalue, eigen[2].Eigenvalue)),
            Centroid: (Point3d)stats.Mean,
            PrincipalFrame: principal,
            PrincipalAxes: axes)
        from valid in shape.IsValid ? Fin.Succ(shape) : Fin.Fail<VectorCloudShape>(key.InvalidResult())
        select valid;
    private static Fin<(Plane Plane, double Deviation)> BestFitOf(Seq<Point3d> points, Op key) =>
        (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
            (PlaneFitResult.Success, { IsValid: true } valid) =>
                from acceptedPlane in key.AcceptValue(value: valid)
                from acceptedDeviation in key.AcceptValue(value: deviation)
                select (Plane: acceptedPlane, Deviation: acceptedDeviation),
            _ => Fin.Fail<(Plane Plane, double Deviation)>(error: key.InvalidResult()),
        };
    private static Fin<double> CompactnessOf(double area, double perimeter, Op key) =>
        from validArea in key.AcceptValue(value: area)
        from validPerimeter in key.AcceptValue(value: perimeter)
        from compactness in validPerimeter > RhinoMath.ZeroTolerance
            ? key.AcceptValue(value: 4.0 * Math.PI * validArea / (validPerimeter * validPerimeter))
            : Fin.Fail<double>(error: key.InvalidResult())
        select compactness;
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
    private static Fin<double> MomentAnisotropyOf(Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        InPlaneAxesOf(axes: axes, normal: normal, context: context, key: key)
            .Bind(chosen => chosen switch {
                Seq<(double Moment, Vector3d Axis)> value when value.Count == 2 =>
                    from a in key.AcceptValue(value: value[0].Moment)
                    from b in key.AcceptValue(value: value[1].Moment)
                    from _ in guard(a >= 0.0 && b >= 0.0, key.InvalidResult())
                    let ratio = Math.Max(val1: a, val2: b) / Math.Max(val1: RhinoMath.ZeroTolerance, val2: Math.Min(val1: a, val2: b))
                    from anisotropy in ratio >= 1.0 ? key.AcceptValue(value: ratio) : Fin.Fail<double>(error: key.InvalidResult())
                    select anisotropy,
                _ => Fin.Fail<double>(error: key.InvalidResult()),
            });
    private static Fin<Plane> RingPrincipalFrameOf(Point3d centroid, Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        InPlaneAxesOf(axes: axes, normal: normal, context: context, key: key)
            .Bind(chosen => chosen switch {
                Seq<(double Moment, Vector3d Axis)> value when value.Count == 2 =>
                    VectorFrame.Of(origin: centroid, normal: normal, xHint: Some(value[0].Axis), context: context, key: key)
                        .Bind(frame => frame.Project<Plane>(key: key)),
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
        points.Count switch {
            < 2 => Fin.Fail<Seq<double>>(key.InvalidInput()),
            _ => points.Fold(
                initialState: (Trail: Seq(0.0), Cumulative: 0.0, Prev: Option<Point3d>.None),
                f: static (state, p) => state.Prev.Match(
                    None: () => (state.Trail, Cumulative: 0.0, Prev: Some(p)),
                    Some: prev => (state.Cumulative + p.DistanceTo(other: prev)) switch {
                        double advanced => (Trail: state.Trail.Add(advanced), Cumulative: advanced, Prev: Some(p)),
                    })).Trail.TraverseM(d => key.AcceptValue(value: d)).As(),
        };
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
        points.Count switch {
            < 2 => Fin.Fail<double>(key.InvalidInput()),
            _ => key.AcceptValue(value: toSeq(Enumerable.Range(start: 0, count: points.Count - 1))
                .Fold(initialState: 0.0, f: (sum, i) => sum + points[i + 1].DistanceTo(other: points[i]))),
        };

    // --- [HULL] -----------------------------------------------------------------------------
    internal static Fin<Mesh> ComputeHull(VectorCloud source, Context context, Op key) =>
        source switch {
            VectorCloud.ClusterCase cluster => ConvexHullOf(cluster: cluster, context: context, key: key),
            _ => Fin.Fail<Mesh>(error: key.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
        };
    private static Fin<Mesh> ConvexHullOf(VectorCloud.ClusterCase cluster, Context context, Op key) {
        if (cluster.Vertices.Count < 4 || Point3d.ArePointsCoplanar(points: cluster.Vertices.AsIterable(), tolerance: context.Absolute.Value))
            return Fin.Fail<Mesh>(error: key.InvalidInput());
        using Mesh? hull = Mesh.CreateConvexHull3D(points: cluster.Vertices.AsIterable(), hullFacets: out _, tolerance: context.Absolute.Value, angleTolerance: context.Angle.Value);
        return hull is not { IsValid: true }
            ? Fin.Fail<Mesh>(error: key.InvalidResult())
            : Fin.Succ(hull.DuplicateMesh());
    }

    // --- [NORMAL_ESTIMATION] -----------------------------------------------------------------
    internal static Fin<Vector3d[]> EstimateNormalsViaCovariance(VectorCloud.ClusterCase target, Op key) =>
        EstimateNormalGraph(target: target, key: key).Map(static graph => graph.Normals);
    internal static Fin<(Vector3d[] Normals, int[][] Neighbors)> EstimateNormalGraph(VectorCloud.ClusterCase target, Op key) {
        Point3d[] points = [.. target.Vertices.AsIterable()];
        if (points.Length < 3) return Fin.Fail<(Vector3d[], int[][])>(key.InvalidInput());
        int k = Math.Min(val1: NormalEstimationNeighbors, val2: points.Length);
        int[][] neighborhoods = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: points, amount: k)];
        return EstimateNormalsFromPoints(points: points, neighborhoodOf: i => NeighborhoodOf(points: points, ids: neighborhoods.Length > i ? neighborhoods[i] : [], key: key), key: key)
            .Map(normals => (Normals: normals, Neighbors: neighborhoods));
    }
    // --- [NORMAL_ORIENTATION] ---------------------------------------------------------------
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992: build minimum spanning tree over k-nearest
    // neighbour graph weighted by 1 − |n_i · n_j|; flip normals to consistently agree along MST.
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
    private static Fin<TOut> SinkhornCluster<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double regularization, int maxIterations, bool debiased, Option<PositiveMagnitude> massRelaxation, Op key) {
        return source.Vertices.Count < 1 || target.Vertices.Count < 1
            ? Fin.Fail<TOut>(error: key.InvalidInput())
            : (from plan in SinkhornOt(source: source.Vertices, target: target.Vertices, sourceMass: MassOf(source), targetMass: MassOf(target), reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key)
               from distance in debiased
                    ? from sourceBias in SinkhornOt(source: source.Vertices, target: source.Vertices, sourceMass: MassOf(source), targetMass: MassOf(source), reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key)
                      from targetBias in SinkhornOt(source: target.Vertices, target: target.Vertices, sourceMass: MassOf(target), targetMass: MassOf(target), reg: regularization, maxIter: maxIterations, massRelaxation: massRelaxation, key: key)
                      select plan.Distance - (0.5 * sourceBias.Distance) - (0.5 * targetBias.Distance)
                    : key.AcceptValue(value: plan.Distance)
               from output in ProjectSinkhorn<TOut>(source: source, target: target, plan: plan, distance: distance, key: key)
               select output);
    }
    private static Fin<TOut> ProjectSinkhorn<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, SinkhornPlan plan, double distance, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(double) => key.AcceptValue(value: distance).Map(static d => (TOut)(object)d),
            Type t when t == typeof(SinkhornReceipt) => Fin.Succ((TOut)(object)new SinkhornReceipt(
                Distance: distance,
                SourceConvergenceResidual: plan.SourceConvergenceResidual,
                TargetConvergenceResidual: plan.TargetConvergenceResidual,
                Iterations: plan.Iterations,
                Stop: plan.Stop)),
            Type t when t == typeof(Matrix) => ProjectCoupling<TOut>(plan: plan, key: key),
            Type t when t == typeof(VectorCloud) => ProjectTransportedCloud<TOut>(source: source, target: target, plan: plan, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloud), outputType: typeof(TOut))),
        };
    private sealed record SinkhornPlan(double Distance, DenseMatrixD Coupling, double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop);
    private static Fin<SinkhornPlan> SinkhornOt(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, double reg, int maxIter, Option<PositiveMagnitude> massRelaxation, Op key) {
        int m = source.Count; int n = target.Count;
        if (sourceMass.Count != m
            || targetMass.Count != n
            || sourceMass.Exists(static value => !RhinoMath.IsValidDouble(x: value) || value <= 0.0)
            || targetMass.Exists(static value => !RhinoMath.IsValidDouble(x: value) || value <= 0.0)
            || !RhinoMath.IsValidDouble(x: reg)
            || reg <= 0.0
            || maxIter < 1)
            return Fin.Fail<SinkhornPlan>(key.InvalidInput());
        DenseMatrixD cost = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        DenseMatrixD logKernel = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) {
                double d2 = source[index: i].DistanceToSquared(other: target[index: j]);
                cost[i, j] = d2;
                logKernel[i, j] = -d2 / reg;
            }
        DenseVectorD logU = DenseVectorD.Create(m, 0.0);
        DenseVectorD logV = DenseVectorD.Create(n, 0.0);
        DenseVectorD logA = DenseVectorD.OfEnumerable(sourceMass.AsIterable().Select(static value => Math.Log(d: value)));
        DenseVectorD logB = DenseVectorD.OfEnumerable(targetMass.AsIterable().Select(static value => Math.Log(d: value)));
        double exponent = massRelaxation.Match(Some: lambda => lambda.Value / (lambda.Value + reg), None: () => 1.0);
        bool balanced = massRelaxation.IsNone;
        double sourceConvergenceResidual = double.PositiveInfinity;
        double targetConvergenceResidual = double.PositiveInfinity;
        int iterations = 0;
        for (int iter = 0; iter < maxIter; iter++) {
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
            if (Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) <= RhinoMath.SqrtEpsilon) break;
        }
        double dist = 0.0;
        DenseMatrixD coupling = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) {
                double logCoupling = logU[i] + logKernel[i, j] + logV[j];
                coupling[i, j] = logCoupling < -745.0 ? 0.0 : Math.Exp(d: logCoupling);
                dist += coupling[i, j] * cost[i, j];
            }
        bool numeric = RhinoMath.IsValidDouble(x: dist) && coupling.Enumerate().All(RhinoMath.IsValidDouble);
        return numeric && Math.Max(val1: sourceConvergenceResidual, val2: targetConvergenceResidual) <= RhinoMath.SqrtEpsilon
            ? Fin.Succ(new SinkhornPlan(
                Distance: dist,
                Coupling: coupling,
                SourceConvergenceResidual: sourceConvergenceResidual,
                TargetConvergenceResidual: targetConvergenceResidual,
                Iterations: iterations,
                Stop: balanced ? SinkhornStopKind.BalancedMarginalsConverged : SinkhornStopKind.RelaxedScalingConverged))
            : Fin.Fail<SinkhornPlan>(key.InvalidResult());
    }
    private static double LogSumExp(int count, Func<int, double> valueAt) {
        double max = double.NegativeInfinity;
        for (int i = 0; i < count; i++) max = Math.Max(val1: max, val2: valueAt(arg: i));
        if (double.IsNegativeInfinity(d: max)) return max;
        double sum = 0.0;
        for (int i = 0; i < count; i++) sum += Math.Exp(d: valueAt(arg: i) - max);
        return max + Math.Log(d: sum);
    }
    private static (double Source, double Target) MarginalResiduals(DenseMatrixD logKernel, DenseVectorD logU, DenseVectorD logV, Arr<double> sourceMass, Arr<double> targetMass) {
        double sourceResidual = 0.0;
        double targetResidual = 0.0;
        for (int i = 0; i < logKernel.RowCount; i++) {
            double row = 0.0;
            for (int j = 0; j < logKernel.ColumnCount; j++) row += Math.Exp(d: logU[i] + logKernel[i, j] + logV[j]);
            sourceResidual = Math.Max(val1: sourceResidual, val2: Math.Abs(value: row - sourceMass[index: i]));
        }
        for (int j = 0; j < logKernel.ColumnCount; j++) {
            double col = 0.0;
            for (int i = 0; i < logKernel.RowCount; i++) col += Math.Exp(d: logU[i] + logKernel[i, j] + logV[j]);
            targetResidual = Math.Max(val1: targetResidual, val2: Math.Abs(value: col - targetMass[index: j]));
        }
        return (Source: sourceResidual, Target: targetResidual);
    }
    private static (double Source, double Target) ScalingResiduals(double[] previousU, DenseVectorD logU, double[] previousV, DenseVectorD logV) =>
        (Source: Enumerable.Range(start: 0, count: logU.Count).Max(i => Math.Abs(value: logU[i] - previousU[i])),
            Target: Enumerable.Range(start: 0, count: logV.Count).Max(i => Math.Abs(value: logV[i] - previousV[i])));
    private static Fin<TOut> ProjectCoupling<TOut>(SinkhornPlan plan, Op key) {
        Dimension rows = Dimension.Create(value: plan.Coupling.RowCount);
        Dimension cols = Dimension.Create(value: plan.Coupling.ColumnCount);
        return key.AcceptValue(value: MatrixKernel.FromMathNet(m: plan.Coupling, rows: rows, cols: cols))
            .Map(static matrix => (TOut)(object)matrix);
    }
    private static Fin<TOut> ProjectTransportedCloud<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, SinkhornPlan plan, Op key) {
        Point3d[] transported = new Point3d[source.Vertices.Count];
        for (int i = 0; i < source.Vertices.Count; i++) {
            double mass = plan.Coupling.Row(i).Sum();
            if (mass <= RhinoMath.ZeroTolerance) return Fin.Fail<TOut>(key.InvalidResult());
            Vector3d sum = Vector3d.Zero;
            for (int j = 0; j < target.Vertices.Count; j++) sum += plan.Coupling[i, j] * (Vector3d)target.Vertices[index: j];
            transported[i] = Point3d.Origin + (sum / mass);
        }
        return VectorCloud.Cluster(points: toSeq(transported), context: source.Tolerance, key: key)
            .Map(static cloud => (TOut)(object)cloud);
    }

    // --- [PRINCIPAL_CURVATURE] -------------------------------------------------------------
    // Local quadric fit z = a u^2 + b u v + c v^2 + d u + e v + f in PCA tangent frame; shape
    // operator at the centre is II = [[2a, b], [b, 2c]] and its symmetric eigen-decomposition
    // yields signed principal curvatures (k1, k2) with tangent directions (e1, e2).
    internal static Fin<Seq<(double K1, double K2, Direction E1, Direction E2)>> PrincipalCurvaturesOf(VectorCloud.ClusterCase cluster, Op key) {
        int n = cluster.Vertices.Count;
        if (n < 6) return Fin.Fail<Seq<(double, double, Direction, Direction)>>(error: key.InvalidInput());
        Point3d[] points = [.. cluster.Vertices.AsIterable()];
        int neighborCount = Math.Min(val1: NormalEstimationNeighbors, val2: n);
        int[][] neighborhoods = [.. RTree.PointCloudKNeighbors(pointcloud: cluster.Indexed, needlePts: points, amount: neighborCount)];
        return toSeq(Enumerable.Range(start: 0, count: n)).TraverseM(i =>
            from neighborhood in NeighborhoodOf(points: points, ids: neighborhoods.Length > i ? neighborhoods[i] : [], key: key)
            from fit in FitPrincipalCurvature(center: points[i], neighborhood: neighborhood, key: key)
            select fit).As();
    }

    private static Fin<(double K1, double K2, Direction E1, Direction E2)> FitPrincipalCurvature(Point3d center, Seq<Point3d> neighborhood, Op key) =>
        neighborhood.Count < 6
            ? Fin.Fail<(double, double, Direction, Direction)>(error: key.InvalidInput())
            : CovarianceOf(points: neighborhood, key: key)
                .Bind(stats => stats.Cov.DecomposeEigen(key: key))
                .Bind(eigen => eigen.Count < 3
                    ? Fin.Fail<Seq<(double Eigenvalue, Arr<double> Eigenvector)>>(error: key.InvalidResult())
                    : Fin.Succ(eigen))
                .Bind(eigen => DecomposeCurvature(center: center, neighborhood: neighborhood, eigen: eigen, key: key));

    private static Fin<(double K1, double K2, Direction E1, Direction E2)> DecomposeCurvature(
        Point3d center, Seq<Point3d> neighborhood,
        Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, Op key) {
        Vector3d normalRaw = AsVector3d(v: eigen[index: 2].Eigenvector);
        Vector3d uRaw = AsVector3d(v: eigen[index: 0].Eigenvector);
        return from normal in Direction.Of(value: normalRaw, tolerance: RhinoMath.ZeroTolerance, key: key)
               from uAxis in Direction.Of(value: uRaw, tolerance: RhinoMath.ZeroTolerance, key: key)
               from vAxis in Direction.Of(value: Vector3d.CrossProduct(a: normal.Value, b: uAxis.Value), tolerance: RhinoMath.ZeroTolerance, key: key)
               from fit in QuadraticFit(center: center, neighborhood: neighborhood, uAxis: uAxis.Value, vAxis: vAxis.Value, normal: normal.Value, key: key)
               from output in ShapeOperatorEigen(a: fit.A, b: fit.B, c: fit.C, uAxis: uAxis.Value, vAxis: vAxis.Value, key: key)
               select output;
    }

    private static Fin<(double A, double B, double C)> QuadraticFit(Point3d center, Seq<Point3d> neighborhood, Vector3d uAxis, Vector3d vAxis, Vector3d normal, Op key) {
        int m = neighborhood.Count;
        double[] designFlat = new double[m * 6];
        double[] rhs = new double[m];
        for (int i = 0; i < m; i++) {
            Vector3d offset = neighborhood[index: i] - center;
            double u = offset * uAxis; double v = offset * vAxis; double n = offset * normal;
            designFlat[(i * 6) + 0] = u * u;
            designFlat[(i * 6) + 1] = u * v;
            designFlat[(i * 6) + 2] = v * v;
            designFlat[(i * 6) + 3] = u;
            designFlat[(i * 6) + 4] = v;
            designFlat[(i * 6) + 5] = 1.0;
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
        double trace = s11 + s22;
        double disc = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (trace * trace) - (4.0 * ((s11 * s22) - (s12 * s12)))));
        double k1 = 0.5 * (trace + disc);
        double k2 = 0.5 * (trace - disc);
        double angle = Math.Abs(value: s12) > RhinoMath.SqrtEpsilon
            ? Math.Atan2(y: k1 - s11, x: s12)
            : s22 > s11 ? Math.PI / 2.0 : 0.0;
        Vector3d e1World = (Math.Cos(d: angle) * uAxis) + (Math.Sin(a: angle) * vAxis);
        Vector3d e2World = (-Math.Sin(a: angle) * uAxis) + (Math.Cos(d: angle) * vAxis);
        return from e1 in Direction.Of(value: e1World, tolerance: RhinoMath.ZeroTolerance, key: key)
               from e2 in Direction.Of(value: e2World, tolerance: RhinoMath.ZeroTolerance, key: key)
               select (K1: k1, K2: k2, E1: e1, E2: e2);
    }

    internal static Fin<Seq<double>> CurvednessOf(VectorCloud.ClusterCase cluster, Op key) =>
        PrincipalCurvaturesOf(cluster: cluster, key: key)
            .Map(static curvatures => toSeq(curvatures.AsIterable().Select(static c => Math.Sqrt(d: 0.5 * ((c.K1 * c.K1) + (c.K2 * c.K2))))));

    // Koenderink-van Doorn 1992 shape index: (2/pi) * atan2(k1+k2, k1-k2) in [-1, 1].
    // Maps geometric type: -1 = cup, -0.5 = trough, 0 = saddle, 0.5 = ridge, 1 = cap.
    internal static Fin<Seq<double>> ShapeIndexOf(VectorCloud.ClusterCase cluster, Op key) =>
        PrincipalCurvaturesOf(cluster: cluster, key: key)
            .Map(static curvatures => toSeq(curvatures.AsIterable().Select(static c => {
                double diff = c.K1 - c.K2;
                double sum = c.K1 + c.K2;
                return Math.Abs(value: diff) < RhinoMath.SqrtEpsilon
                    ? Math.Sign(value: sum)
                    : 2.0 / Math.PI * Math.Atan2(y: sum, x: diff);
            })));
}
