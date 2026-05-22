using Foundation.CSharp.Analyzers.Contracts;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using LinearMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

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
    public sealed record ClusterCase(Seq<Point3d> Vertices, Context Tolerance) : VectorCloud {
        private static readonly ConditionalWeakTable<ClusterCase, PointCloud> CloudCache = [];
        private static readonly ConditionalWeakTable<ClusterCase, RTree> TreeCache = [];
        internal PointCloud Indexed => CloudCache.GetValue(key: this, createValueCallback: static c => {
            PointCloud pc = [];
            pc.AddRange(points: c.Vertices.AsIterable());
            return pc;
        });
        internal RTree Tree => TreeCache.GetValue(key: this,
            createValueCallback: static c => RTree.CreatePointCloudTree(cloud: c.Indexed));
        internal Fin<ClosestHit> ClosestVertex(Point3d sample, Op key) =>
            Indexed.ClosestPoint(testPoint: sample) switch {
                int idx when idx >= 0 && idx < Vertices.Count => key.AcceptValue(value: ClosestHit.At(
                    target: sample,
                    point: Vertices[idx],
                    component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: idx)))),
                _ => Fin.Fail<ClosestHit>(error: key.InvalidResult()),
            };
        internal Fin<Seq<int>> WithinRadius(Point3d sample, double radius, Op key) {
            Sphere ball = new(center: sample, radius: radius);
            return ball.IsValid switch {
                false => Fin.Fail<Seq<int>>(error: key.InvalidInput()),
                true => SearchTree(ball: ball, key: key),
            };
        }
        private Fin<Seq<int>> SearchTree(Sphere ball, Op key) {
            List<int> buffer = [];
            return Tree.Search(sphere: ball, callback: (_, args) => buffer.Add(item: args.Id))
                ? key.AcceptValue(value: toSeq(buffer))
                : Fin.Fail<Seq<int>>(error: key.InvalidResult());
        }
    }
    public static Fin<VectorCloud> Ring(Seq<Point3d> points, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from valid in points.Traverse(point => op.AcceptValue(value: point).ToValidation()).As().ToFin()
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
                LinearMatrix coordinates = DenseMatrixD.Build.Dense(rows: rows.Count, columns: dim, init: (i, j) => rows[i][j]);
                double[] mean = new double[dim];
                for (int j = 0; j < dim; j++) mean[j] = coordinates.Column(j).Average();
                LinearMatrix centered = DenseMatrixD.Build.Dense(rows: rows.Count, columns: dim, init: (i, j) => coordinates[i, j] - mean[j]);
                LinearMatrix covariance = centered.TransposeThisAndMultiply(centered) / rows.Count;
                double[] upper = [.. Enumerable.Range(start: 0, count: dim)
                    .SelectMany(i => Enumerable.Range(start: i, count: dim - i).Select(j => covariance[i, j]))];
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
    // Reused by CurveProjection.RotationMinimizing in Atoms.cs.
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
}
