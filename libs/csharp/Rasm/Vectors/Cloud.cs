using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SymmetricMatrix3d(double M00, double M01, double M02, double M11, double M12, double M22) {
    public bool IsValid =>
        RhinoMath.IsValidDouble(M00) && RhinoMath.IsValidDouble(M01) && RhinoMath.IsValidDouble(M02) &&
        RhinoMath.IsValidDouble(M11) && RhinoMath.IsValidDouble(M12) && RhinoMath.IsValidDouble(M22);
}

[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = new(key: 0, output: typeof(Vector3d),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.RingNormalOf(ring: (VectorCloud.RingCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Area = new(key: 1, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.RingAreaOf(ring: (VectorCloud.RingCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Perimeter = new(key: 2, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => k.AcceptValue(value: ((VectorCloud.RingCase)cloud).Native.Length).Map(static v => (object)v));
    public static readonly VectorCloudMetric EdgeAspect = new(key: 3, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.EdgeAspectOf(native: ((VectorCloud.RingCase)cloud).Native, context: ((VectorCloud.RingCase)cloud).Tolerance, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Skewness = new(key: 4, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.RingSkewnessOf(ring: (VectorCloud.RingCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Compactness = new(key: 5, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.RingCompactnessOf(ring: (VectorCloud.RingCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric MomentAnisotropy = new(key: 6, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.RingMomentAnisotropyOf(ring: (VectorCloud.RingCase)cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric RadiiOfGyration = new(key: 7, output: typeof(Vector3d),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.WithMassProperties(ring: (VectorCloud.RingCase)cloud, project: static (props, op) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric AreaError = new(key: 8, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.WithMassProperties(ring: (VectorCloud.RingCase)cloud, project: static (props, op) => op.AcceptValue(value: props.AreaError), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric CentroidError = new(key: 9, output: typeof(Vector3d),
        admitsCase: static cloud => cloud is VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.WithMassProperties(ring: (VectorCloud.RingCase)cloud, project: static (props, op) => op.AcceptValue(value: props.CentroidError), key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Centroid = new(key: 10, output: typeof(Point3d),
        admitsCase: static _ => true,
        measure: static (cloud, k) => CloudKernel.CentroidOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric BestFitPlane = new(key: 11, output: typeof(Plane),
        admitsCase: static _ => true,
        measure: static (cloud, k) => CloudKernel.BestFitPlaneOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric PrincipalAxes = new(key: 12, output: typeof(Seq<Vector3d>),
        admitsCase: static _ => true,
        measure: static (cloud, k) => CloudKernel.PrincipalAxesOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric PrincipalFrame = new(key: 13, output: typeof(Plane),
        admitsCase: static _ => true,
        measure: static (cloud, k) => CloudKernel.PrincipalFrameOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Shape = new(key: 14, output: typeof(VectorCloudShape),
        admitsCase: static _ => true,
        measure: static (cloud, k) => CloudKernel.ShapeOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric BishopFrames = new(key: 15, output: typeof(Seq<Plane>),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase or VectorCloud.RingCase,
        measure: static (cloud, k) => CloudKernel.BishopFramesOf(cloud: cloud, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric TangentFlow = new(key: 16, output: typeof(Seq<Vector3d>),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase,
        measure: static (cloud, k) => CloudKernel.TangentFlowOf(points: ((VectorCloud.PolylineCase)cloud).Vertices, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric CumulativeArcLength = new(key: 17, output: typeof(Seq<double>),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase,
        measure: static (cloud, k) => CloudKernel.CumulativeArcLengthOf(points: ((VectorCloud.PolylineCase)cloud).Vertices, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric EdgeCurvatures = new(key: 18, output: typeof(Seq<double>),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase,
        measure: static (cloud, k) => CloudKernel.EdgeCurvaturesOf(points: ((VectorCloud.PolylineCase)cloud).Vertices, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric OpenLength = new(key: 19, output: typeof(double),
        admitsCase: static cloud => cloud is VectorCloud.PolylineCase,
        measure: static (cloud, k) => CloudKernel.OpenLengthOf(points: ((VectorCloud.PolylineCase)cloud).Vertices, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Covariance = new(key: 20, output: typeof(SymmetricMatrix3d),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.CovarianceOf(points: ((VectorCloud.ClusterCase)cloud).Vertices, key: k).Map(static v => (object)v.Cov));
    public static readonly VectorCloudMetric PrincipalDirection = new(key: 21, output: typeof(Vector3d),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.PrincipalDirectionOf(points: ((VectorCloud.ClusterCase)cloud).Vertices, key: k).Map(static v => (object)v));
    public static readonly VectorCloudMetric Spread = new(key: 22, output: typeof(Vector3d),
        admitsCase: static cloud => cloud is VectorCloud.ClusterCase,
        measure: static (cloud, k) => CloudKernel.SpreadOf(points: ((VectorCloud.ClusterCase)cloud).Vertices, key: k).Map(static v => (object)v));
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
                    VectorCloudShape shape when shape.IsValid => Fin.Succ((TOut)(object)shape),
                    VectorCloudShape => Fin.Fail<TOut>(key.InvalidResult()),
                    SymmetricMatrix3d matrix when matrix.IsValid => Fin.Succ((TOut)value),
                    SymmetricMatrix3d => Fin.Fail<TOut>(key.InvalidResult()),
                    _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
                }),
                false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(TOut))),
            },
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
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
    public sealed record ClusterCase(Seq<Point3d> Vertices, Context Tolerance) : VectorCloud;
    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => op.AcceptValue(value: point).ToValidation()).As().ToFin()
               let closed = valid.Count > 1 && valid[0].DistanceTo(other: valid[valid.Count - 1]) <= model.Absolute.Value
               let vertices = closed ? valid.Init : valid
               from _ in guard(vertices.Count >= 3, op.InvalidInput())
               let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
               from __ in guard(native.IsValid && native.IsClosedWithinTolerance(model.Absolute.Value) && native.SegmentCount >= 3, op.InvalidInput())
               from ___ in CloudKernel.SimpleLoop(native: native, context: model, key: op)
               select (VectorCloud)new RingCase(Vertices: vertices, Native: native, Tolerance: model);
    }
    public static Fin<VectorCloud> Polyline(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => op.AcceptValue(value: point).ToValidation()).As().ToFin()
               from _ in guard(valid.Count >= 2, op.InvalidInput())
               select (VectorCloud)new PolylineCase(Vertices: valid, Tolerance: model);
    }
    public static Fin<VectorCloud> Cluster(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => op.AcceptValue(value: point).ToValidation()).As().ToFin()
               from _ in guard(valid.Count >= 1, op.InvalidInput())
               select (VectorCloud)new ClusterCase(Vertices: valid, Tolerance: model);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class CloudKernel {
    // --- [RAILS] ------------------------------------------------------------------------
    internal static Fin<T> WithCaseRails<T>(
        VectorCloud cloud,
        Func<VectorCloud.RingCase, Op, Fin<T>> ringRail,
        Func<Seq<Point3d>, Context, Op, Fin<T>> pointRail,
        Op key) =>
        cloud switch {
            VectorCloud.RingCase ring => ringRail(arg1: ring, arg2: key),
            VectorCloud.PolylineCase polyline => pointRail(arg1: polyline.Vertices, arg2: polyline.Tolerance, arg3: key),
            VectorCloud.ClusterCase cluster => pointRail(arg1: cluster.Vertices, arg2: cluster.Tolerance, arg3: key),
            _ => Fin.Fail<T>(error: key.InvalidInput()),
        };

    // --- [WELFORD] ----------------------------------------------------------------------
    internal static Fin<(Vector3d Mean, SymmetricMatrix3d Cov)> CovarianceOf(Seq<Point3d> points, Op key) =>
        points.Count switch {
            0 => Fin.Fail<(Vector3d, SymmetricMatrix3d)>(key.InvalidResult()),
            _ => points.Fold(
                initialState: (N: 0, Mean: Vector3d.Zero, M: (M00: 0.0, M01: 0.0, M02: 0.0, M11: 0.0, M12: 0.0, M22: 0.0)),
                f: static (state, p) => WelfordStep(state: state, value: (Vector3d)p)) switch {
                    (int n, Vector3d mean, (double M00, double M01, double M02, double M11, double M12, double M22) m) when n > 0 => key.AcceptValue(value: (
                        Mean: mean,
                        Cov: new SymmetricMatrix3d(
                            M00: m.M00 / n, M01: m.M01 / n, M02: m.M02 / n,
                            M11: m.M11 / n, M12: m.M12 / n, M22: m.M22 / n))),
                    _ => Fin.Fail<(Vector3d, SymmetricMatrix3d)>(key.InvalidResult()),
                },
        };
    private static (int N, Vector3d Mean, (double M00, double M01, double M02, double M11, double M12, double M22) M) WelfordStep(
        (int N, Vector3d Mean, (double M00, double M01, double M02, double M11, double M12, double M22) M) state,
        Vector3d value) {
        int n = state.N + 1;
        Vector3d delta = value - state.Mean;
        Vector3d meanNew = state.Mean + (delta / n);
        Vector3d delta2 = value - meanNew;
        return (N: n, Mean: meanNew, M: (
            M00: state.M.M00 + (delta.X * delta2.X),
            M01: state.M.M01 + (delta.X * delta2.Y),
            M02: state.M.M02 + (delta.X * delta2.Z),
            M11: state.M.M11 + (delta.Y * delta2.Y),
            M12: state.M.M12 + (delta.Y * delta2.Z),
            M22: state.M.M22 + (delta.Z * delta2.Z)));
    }

    // --- [EIGEN3X3] ---------------------------------------------------------------------
    // Jacobi cyclic eigendecomposition. State carries the symmetric A and the three columns of V
    // (the accumulated orthogonal rotation). After convergence A is diagonal and V columns are eigenvectors.
    internal static Fin<Seq<(double Eigenvalue, Vector3d Eigenvector)>> DecomposeEigen(SymmetricMatrix3d m, Op key) {
        (SymmetricMatrix3d A, Vector3d V0, Vector3d V1, Vector3d V2) = toSeq(Enumerable.Range(start: 0, count: MaxSweeps)).Fold(
            initialState: (A: m, V0: Vector3d.XAxis, V1: Vector3d.YAxis, V2: Vector3d.ZAxis),
            f: static (s, _) => (Math.Abs(s.A.M01) + Math.Abs(s.A.M02) + Math.Abs(s.A.M12)) < (EigenEpsilon * (Math.Abs(s.A.M00) + Math.Abs(s.A.M11) + Math.Abs(s.A.M22)))
                ? s
                : Givens(state: Givens(state: Givens(state: s, p: 0, q: 1), p: 0, q: 2), p: 1, q: 2));
        return toSeq(Seq(
                (Eigenvalue: A.M00, Eigenvector: V0),
                (Eigenvalue: A.M11, Eigenvector: V1),
                (Eigenvalue: A.M22, Eigenvector: V2)).AsIterable()
            .OrderByDescending(static p => Math.Abs(p.Eigenvalue)))
            .TraverseM(p => key.AcceptValue(value: p)).As();
    }
    private const int MaxSweeps = 12;
    private const double EigenEpsilon = 1e-14;
    // One Givens rotation: reads the (p,q) off-diagonal of A, computes Jacobi (c,s), writes the rotated A
    // cells via `with { … }` and rotates the two affected V columns. r = 3-p-q identifies the untouched axis.
    private static (SymmetricMatrix3d A, Vector3d V0, Vector3d V1, Vector3d V2) Givens(
        (SymmetricMatrix3d A, Vector3d V0, Vector3d V1, Vector3d V2) state, int p, int q) {
        double apq = (p, q) switch { (0, 1) => state.A.M01, (0, 2) => state.A.M02, _ => state.A.M12 };
        int r = 3 - p - q;
        double app = p switch { 0 => state.A.M00, 1 => state.A.M11, _ => state.A.M22 };
        double aqq = q switch { 0 => state.A.M00, 1 => state.A.M11, _ => state.A.M22 };
        double arp = (Math.Min(val1: r, val2: p), Math.Max(val1: r, val2: p)) switch { (0, 1) => state.A.M01, (0, 2) => state.A.M02, _ => state.A.M12 };
        double arq = (Math.Min(val1: r, val2: q), Math.Max(val1: r, val2: q)) switch { (0, 1) => state.A.M01, (0, 2) => state.A.M02, _ => state.A.M12 };
        double theta = (aqq - app) / (2.0 * apq);
        double t = theta >= 0.0
            ? 1.0 / (theta + Math.Sqrt(1.0 + (theta * theta)))
            : 1.0 / (theta - Math.Sqrt(1.0 + (theta * theta)));
        double c = 1.0 / Math.Sqrt(1.0 + (t * t));
        double s = t * c;
        double newApp = (c * c * app) - (2.0 * s * c * apq) + (s * s * aqq);
        double newAqq = (s * s * app) + (2.0 * s * c * apq) + (c * c * aqq);
        double newArp = (c * arp) - (s * arq);
        double newArq = (s * arp) + (c * arq);
        return (Math.Abs(apq) < 1e-16, p, q, r) switch {
            (true, _, _, _) => state,
            (_, 0, 1, 2) => (
                state.A with { M00 = newApp, M11 = newAqq, M01 = 0.0, M02 = newArp, M12 = newArq },
                (c * state.V0) - (s * state.V1),
                (s * state.V0) + (c * state.V1),
                state.V2),
            (_, 0, 2, 1) => (
                state.A with { M00 = newApp, M22 = newAqq, M02 = 0.0, M01 = newArp, M12 = newArq },
                (c * state.V0) - (s * state.V2),
                state.V1,
                (s * state.V0) + (c * state.V2)),
            _ => (
                state.A with { M11 = newApp, M22 = newAqq, M12 = 0.0, M01 = newArp, M02 = newArq },
                state.V0,
                (c * state.V1) - (s * state.V2),
                (s * state.V1) + (c * state.V2)),
        };
    }

    // --- [BISHOP] -----------------------------------------------------------------------
    internal static Fin<Seq<Plane>> BishopFramesOf(VectorCloud cloud, Op key) =>
        cloud switch {
            VectorCloud.RingCase ring => RingNormalOf(ring: ring, key: key)
                .Bind(normal => Direction.Of(value: normal, context: ring.Tolerance, key: key))
                .Bind(initialNormal => BishopChainOf(points: ring.Vertices, initialNormal: initialNormal, closed: true, context: ring.Tolerance, key: key)),
            VectorCloud.PolylineCase polyline when polyline.Vertices.Count >= 2 =>
                (VectorFrame.SeedPerpendicular(axis: polyline.Vertices[1] - polyline.Vertices[0]) switch {
                    Vector3d seed => Direction.Of(value: seed, context: polyline.Tolerance, key: key),
                    _ => Fin.Fail<Direction>(key.InvalidResult()),
                }).Bind(initialNormal => BishopChainOf(points: polyline.Vertices, initialNormal: initialNormal, closed: false, context: polyline.Tolerance, key: key)),
            _ => Fin.Fail<Seq<Plane>>(key.InvalidInput()),
        };
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
    private static Vector3d DoubleReflect(Vector3d rPrev, Vector3d tPrev, Vector3d tCurr) =>
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
                .Fold(initialState: 0.0, f: (sum, pair) => sum + Math.Atan2(y: planeNormal * Vector3d.CrossProduct(a: pair.V0, b: pair.V1), x: pair.V0 * pair.V1)) / RhinoMath.TwoPI)),
        };

    // --- [DISPATCH] ---------------------------------------------------------------------
    internal static Fin<Point3d> CentroidOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => WithMassProperties(ring: ring, project: static (props, op) => op.AcceptValue(value: props.Centroid), key: k),
            pointRail: static (points, _, k) => CovarianceOf(points: points, key: k).Bind(stats => k.AcceptValue(value: (Point3d)stats.Mean)));
    internal static Fin<Plane> BestFitPlaneOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => BestFitOf(points: ring.Vertices, key: k).Map(static fit => fit.Plane),
            pointRail: static (points, _, k) => BestFitOf(points: points, key: k).Map(static fit => fit.Plane));
    internal static Fin<Seq<Vector3d>> PrincipalAxesOf(VectorCloud cloud, Op key) =>
        WithCaseRails(cloud: cloud, key: key,
            ringRail: static (ring, k) => WithMassProperties(ring: ring,
                project: static (props, op) => AxesOf(mass: props, key: op).Bind(axes => axes.Map(static a => a.Axis).TraverseM(a => op.AcceptValue(value: a)).As()),
                key: k),
            pointRail: static (points, _, k) => CovarianceOf(points: points, key: k)
                .Bind(stats => DecomposeEigen(m: stats.Cov, key: k))
                .Bind(eigen => eigen.Map(static p => p.Eigenvector).TraverseM(v => k.AcceptValue(value: v)).As()));
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
                .Bind(stats => DecomposeEigen(m: stats.Cov, key: k)
                    .Bind(eigen => VectorFrame.Of(origin: (Point3d)stats.Mean, normal: eigen[2].Eigenvector, xHint: Some(eigen[0].Eigenvector), context: ctx, key: k))
                    .Bind(frame => frame.Project<Plane>(key: k))));
    internal static Fin<VectorCloudShape> ShapeOf(VectorCloud cloud, Op key) =>
        cloud switch {
            VectorCloud.RingCase ring => RingShapeOf(ring: ring, key: key),
            VectorCloud.PolylineCase polyline => PointSetShapeOf(points: polyline.Vertices, context: polyline.Tolerance, forPolyline: true, key: key),
            VectorCloud.ClusterCase cluster => PointSetShapeOf(points: cluster.Vertices, context: cluster.Tolerance, forPolyline: false, key: key),
            _ => Fin.Fail<VectorCloudShape>(key.InvalidInput()),
        };
    internal static Fin<Vector3d> PrincipalDirectionOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, key: key)
            .Bind(stats => DecomposeEigen(m: stats.Cov, key: key))
            .Bind(eigen => key.AcceptValue(value: eigen[0].Eigenvector));
    internal static Fin<Vector3d> SpreadOf(Seq<Point3d> points, Op key) =>
        CovarianceOf(points: points, key: key)
            .Bind(stats => DecomposeEigen(m: stats.Cov, key: key))
            .Bind(eigen => key.AcceptValue(value: new Vector3d(eigen[0].Eigenvalue, eigen[1].Eigenvalue, eigen[2].Eigenvalue)));

    // --- [RING] -------------------------------------------------------------------------
    internal static Fin<Vector3d> RingNormalOf(VectorCloud.RingCase ring, Op key) =>
        WithRingCurve(ring: ring, project: static (state, curve) =>
            RingOrientationOf(curve: curve, context: state.Context, key: state.Key).Map(static o => o.Normal), key: key);
    internal static Fin<double> RingAreaOf(VectorCloud.RingCase ring, Op key) =>
        WithMassProperties(ring: ring, project: static (props, op) => op.AcceptValue(value: props.Area), key: key);
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
        from eigen in DecomposeEigen(m: stats.Cov, key: key)
        let axes = eigen.Map(static p => (Moment: p.Eigenvalue, Axis: p.Eigenvector))
        from principal in VectorFrame.Of(origin: (Point3d)stats.Mean, normal: eigen[2].Eigenvector, xHint: Some(eigen[0].Eigenvector), context: context, key: key)
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
    internal static Fin<Unit> SimpleLoop(Polyline native, Context context, Op key) =>
        Optional(native.ToPolylineCurve()).ToFin(key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(
            state: (Context: context, Key: key),
            project: static (state, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: state.Context.Absolute.Value)).ToFin(state.Key.InvalidResult())
                .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(state.Key.InvalidInput()))));
    private static Fin<TResult> WithRingCurve<TResult>(VectorCloud.RingCase ring, Func<(Seq<Point3d> Vertices, Polyline Native, Context Context, Op Key), PolylineCurve, Fin<TResult>> project, Op key) =>
        Optional(ring.Native.ToPolylineCurve()).ToFin(key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: (ring.Vertices, ring.Native, Context: ring.Tolerance, Key: key), project: project));
    private static Fin<TResult> WithMassPropertiesInternal<TState, TResult>(PolylineCurve curve, Context context, Func<TState, AreaMassProperties, Fin<TResult>> project, TState state, Op key) =>
        Optional(AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: context.Absolute.Value)).ToFin(key.InvalidResult())
            .Bind(props => new Lease<AreaMassProperties>.Owned(Value: props).Use(state: state, project: project));
    internal static Fin<TResult> WithMassProperties<TResult>(VectorCloud.RingCase ring, Func<AreaMassProperties, Op, Fin<TResult>> project, Op key) =>
        WithRingCurve(ring: ring, project: (state, curve) => WithMassPropertiesInternal(
            curve: curve, context: state.Context,
            project: (s, mass) => project(arg1: mass, arg2: s.Key),
            state: (state.Context, state.Key, Curve: curve),
            key: state.Key), key: key);

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
}
