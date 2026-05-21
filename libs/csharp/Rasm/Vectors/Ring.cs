using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorRingMetric {
    public static readonly VectorRingMetric Normal = new(key: 0, output: typeof(Vector3d), measure: static (ring, key) => ring.Normal(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric EdgeAspect = new(key: 1, output: typeof(double), measure: static (ring, key) => ring.EdgeAspect(key: key).Map(static value => (object)value)), Area = new(key: 2, output: typeof(double), measure: static (ring, key) => ring.Area(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Perimeter = new(key: 3, output: typeof(double), measure: static (ring, key) => ring.Perimeter(key: key).Map(static value => (object)value)), Skewness = new(key: 4, output: typeof(double), measure: static (ring, key) => ring.Skewness(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Centroid = new(key: 5, output: typeof(Point3d), measure: static (ring, key) => ring.Centroid(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric BestFitPlane = new(key: 6, output: typeof(Plane), measure: static (ring, key) => ring.BestFitPlane(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric PrincipalAxes = new(key: 7, output: typeof(Seq<Vector3d>), measure: static (ring, key) => ring.PrincipalAxes(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric PrincipalFrame = new(key: 8, output: typeof(Plane), measure: static (ring, key) => ring.PrincipalFrame(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Shape = new(key: 9, output: typeof(VectorRingShape), measure: static (ring, key) => ring.Shape(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Compactness = new(key: 10, output: typeof(double), measure: static (ring, key) => ring.Compactness(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric MomentAnisotropy = new(key: 11, output: typeof(double), measure: static (ring, key) => ring.MomentAnisotropy(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric RadiiOfGyration = new(key: 12, output: typeof(Vector3d), measure: static (ring, key) => ring.RadiiOfGyration(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric AreaError = new(key: 13, output: typeof(double), measure: static (ring, key) => ring.AreaError(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric CentroidError = new(key: 14, output: typeof(Vector3d), measure: static (ring, key) => ring.CentroidError(key: key).Map(static value => (object)value));
    public Type Output { get; }
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorRing ring, Op key);
    internal Fin<TOut> Project<TOut>(VectorRing ring, Op key) =>
        Output.Equals(typeof(TOut)) switch {
            true => Measure(ring: ring, key: key).Bind(value => value switch {
                Seq<Vector3d> axes => axes.TraverseM(axis => key.AcceptValue(value: axis)).As().Map(static valid => (TOut)(object)valid),
                VectorRingShape shape when shape.IsValid => Fin.Succ((TOut)(object)shape),
                VectorRingShape => Fin.Fail<TOut>(key.InvalidResult()),
                _ => key.AcceptValue(value: value).Map(static valid => (TOut)valid),
            }),
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorRing), outputType: typeof(TOut))),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorRingShape(
    Vector3d Normal,
    double SignedArea,
    double Area,
    double Perimeter,
    double EdgeAspect,
    double Skewness,
    double PlanarityDeviation,
    double Compactness,
    double MomentAnisotropy,
    Vector3d RadiiOfGyration,
    double AreaError,
    Vector3d CentroidError,
    Point3d Centroid,
    Plane BestFitPlane,
    Plane PrincipalFrame,
    Seq<(double Moment, Vector3d Axis)> PrincipalAxes,
    bool Convex,
    CurveOrientation Orientation) {
    internal bool IsValid => Normal is { IsValid: true } && !Normal.IsTiny()
        && RhinoMath.IsValidDouble(SignedArea)
        && RhinoMath.IsValidDouble(Area) && Area >= 0.0
        && RhinoMath.IsValidDouble(Perimeter) && Perimeter > 0.0
        && RhinoMath.IsValidDouble(EdgeAspect) && EdgeAspect >= 1.0
        && RhinoMath.IsValidDouble(Skewness) && Skewness >= 0.0
        && RhinoMath.IsValidDouble(PlanarityDeviation) && PlanarityDeviation >= 0.0
        && RhinoMath.IsValidDouble(Compactness) && Compactness is >= 0.0 and <= 1.0
        && RhinoMath.IsValidDouble(MomentAnisotropy) && MomentAnisotropy >= 1.0
        && RadiiOfGyration.IsValid
        && RhinoMath.IsValidDouble(AreaError) && AreaError >= 0.0
        && CentroidError.IsValid
        && Centroid.IsValid
        && BestFitPlane.IsValid
        && PrincipalFrame.IsValid
        && PrincipalAxes.ForAll(static axis => OpAcceptance.ValidityOf(source: axis).IfNone(false))
        && Orientation is CurveOrientation.Clockwise or CurveOrientation.CounterClockwise;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
internal readonly record struct VectorRing {
    private VectorRing(Seq<Point3d> points, Polyline native, Context context) {
        Points = points;
        Native = native;
        Context = context;
    }
    internal Seq<Point3d> Points { get; }
    private Polyline Native { get; }
    private Context Context { get; }
    internal static Fin<VectorRing> Of(Seq<Point3d> points, Context context, Op key) =>
        from model in Optional(context).ToFin(key.MissingContext())
        from valid in points.Traverse(point => key.AcceptValue(value: point).ToValidation()).As().ToFin()
        let closed = valid.Count > 1 && valid[0].DistanceTo(other: valid[valid.Count - 1]) <= model.Absolute.Value
        let vertices = closed ? valid.Init : valid
        from _ in guard(vertices.Count >= 3, key.InvalidInput())
        let native = new Polyline([.. vertices.AsIterable(), vertices[0]])
        from __ in guard(native.IsValid && native.IsClosedWithinTolerance(model.Absolute.Value) && native.SegmentCount >= 3, key.InvalidInput())
        from ___ in SimpleLoop(native: native, context: model, key: key)
        select new VectorRing(points: vertices, native: native, context: model);
    internal Fin<Vector3d> Normal(Op key) =>
        WithCurve(
            state: (Context, Key: key),
            project: static (state, curve) =>
                OrientationOf(curve: curve, context: state.Context, key: state.Key).Map(static ring => ring.Normal),
            key: key);
    internal Fin<double> Area(Op key) =>
        WithMassProperties(project: static (props, op) => op.AcceptValue(value: props.Area), key: key);
    internal Fin<double> Perimeter(Op key) =>
        key.AcceptValue(value: Native.Length);
    internal Fin<double> Compactness(Op key) {
        double perimeter = Native.Length;
        return Area(key: key).Bind(area => CompactnessOf(area: area, perimeter: perimeter, key: key));
    }
    internal Fin<double> MomentAnisotropy(Op key) =>
        WithCurve(
            state: (Context, Key: key),
            project: static (state, curve) =>
                from ring in OrientationOf(curve: curve, context: state.Context, key: state.Key)
                from value in WithMassProperties(
                    curve: curve,
                    context: state.Context,
                    project: static (s, mass) => AxesOf(mass: mass, key: s.Key).Bind(axes => MomentAnisotropyOf(axes: axes, normal: s.Normal, context: s.Context, key: s.Key)),
                    state: (state.Context, state.Key, ring.Normal),
                    key: state.Key)
                select value,
            key: key);
    internal Fin<Vector3d> RadiiOfGyration(Op key) =>
        WithMassProperties(project: static (props, op) => op.AcceptValue(value: props.CentroidCoordinatesRadiiOfGyration), key: key);
    internal Fin<double> AreaError(Op key) =>
        WithMassProperties(project: static (props, op) => op.AcceptValue(value: props.AreaError), key: key);
    internal Fin<Vector3d> CentroidError(Op key) =>
        WithMassProperties(project: static (props, op) => op.AcceptValue(value: props.CentroidError), key: key);
    internal Fin<double> EdgeAspect(Op key) =>
        EdgeAspectOf(native: Native, context: Context, key: key);
    internal Fin<double> Skewness(Op key) =>
        WithCurve(
            state: (Points, Context, Key: key),
            project: static (state, curve) =>
                from ring in OrientationOf(curve: curve, context: state.Context, key: state.Key)
                from skewness in SkewnessOf(points: state.Points, normal: ring.Normal, key: state.Key)
                select skewness,
            key: key);
    internal Fin<Point3d> Centroid(Op key) =>
        WithMassProperties(project: static (props, k) => k.AcceptValue(value: props.Centroid), key: key);
    internal Fin<Plane> BestFitPlane(Op key) =>
        BestFitPlaneOf(points: Points, key: key);
    private static Fin<(Plane Plane, double Deviation)> BestFitOf(Seq<Point3d> points, Op key) =>
        (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
            (PlaneFitResult.Success, { IsValid: true } valid) =>
                from acceptedPlane in key.AcceptValue(value: valid)
                from acceptedDeviation in key.AcceptValue(value: deviation)
                select (Plane: acceptedPlane, Deviation: acceptedDeviation),
            _ => Fin.Fail<(Plane Plane, double Deviation)>(error: key.InvalidResult()),
        };
    private static Fin<Plane> BestFitPlaneOf(Seq<Point3d> points, Op key) =>
        BestFitOf(points: points, key: key).Map(static fit => fit.Plane);
    internal Fin<Seq<Vector3d>> PrincipalAxes(Op key) =>
        WithMassProperties(project: static (props, k) => AxesOf(mass: props, key: k).Bind(axes => axes.Map(static axis => axis.Axis).TraverseM(axis => k.AcceptValue(value: axis)).As()), key: key);
    internal Fin<Plane> PrincipalFrame(Op key) =>
        WithCurve(
            state: (Context, Key: key),
            project: static (state, curve) =>
                from ring in OrientationOf(curve: curve, context: state.Context, key: state.Key)
                from frame in WithMassProperties(
                    curve: curve,
                    context: state.Context,
                    project: static (s, mass) => AxesOf(mass: mass, key: s.Key).Bind(axes => PrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: s.Normal, context: s.Context, key: s.Key)),
                    state: (state.Context, state.Key, ring.Normal),
                    key: state.Key)
                select frame,
            key: key);
    internal Fin<VectorRingShape> Shape(Op key) =>
        WithCurve(
            state: (Points, Native, Context, Key: key),
            project: static (state, curve) => WithMassProperties(
                curve: curve,
                context: state.Context,
                project: static (s, mass) =>
                    from ring in OrientationOf(curve: s.Curve, context: s.Context, key: s.Key)
                    from fit in BestFitOf(points: s.Points, key: s.Key)
                    from edgeAspect in EdgeAspectOf(native: s.Native, context: s.Context, key: s.Key)
                    from skewness in SkewnessOf(points: s.Points, normal: ring.Normal, key: s.Key)
                    from axes in AxesOf(mass: mass, key: s.Key)
                    from compactness in CompactnessOf(area: mass.Area, perimeter: s.Native.Length, key: s.Key)
                    from anisotropy in MomentAnisotropyOf(axes: axes, normal: ring.Normal, context: s.Context, key: s.Key)
                    from radii in s.Key.AcceptValue(value: mass.CentroidCoordinatesRadiiOfGyration)
                    from areaError in s.Key.AcceptValue(value: mass.AreaError)
                    from centroidError in s.Key.AcceptValue(value: mass.CentroidError)
                    from principal in PrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: ring.Normal, context: s.Context, key: s.Key)
                    let shape = new VectorRingShape(
                        Normal: ring.Normal,
                        SignedArea: ring.Orientation == CurveOrientation.CounterClockwise ? mass.Area : -mass.Area,
                        Area: mass.Area,
                        Perimeter: s.Native.Length,
                        EdgeAspect: edgeAspect,
                        Skewness: skewness,
                        PlanarityDeviation: fit.Deviation,
                        Compactness: compactness,
                        MomentAnisotropy: anisotropy,
                        RadiiOfGyration: radii,
                        AreaError: areaError,
                        CentroidError: centroidError,
                        Centroid: mass.Centroid,
                        BestFitPlane: fit.Plane,
                        PrincipalFrame: principal,
                        PrincipalAxes: axes,
                        Convex: s.Native.IsConvexLoop(strictlyConvex: false),
                        Orientation: ring.Orientation)
                    from valid in shape.IsValid ? Fin.Succ(shape) : Fin.Fail<VectorRingShape>(s.Key.InvalidResult())
                    select valid,
                state: (state.Points, state.Native, state.Context, state.Key, Curve: curve),
                key: state.Key),
            key: key);
    private static Fin<double> CompactnessOf(double area, double perimeter, Op key) =>
        from validArea in key.AcceptValue(value: area)
        from validPerimeter in key.AcceptValue(value: perimeter)
        from compactness in validPerimeter > RhinoMath.ZeroTolerance
            ? key.AcceptValue(value: 4.0 * Math.PI * validArea / (validPerimeter * validPerimeter))
            : Fin.Fail<double>(error: key.InvalidResult())
        select compactness;
    private static Fin<double> EdgeAspectOf(Polyline native, Context context, Op key) {
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
    private static Fin<Plane> PrincipalFrameOf(Point3d centroid, Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
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
    private static Fin<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)> OrientationOf(PolylineCurve curve, Context context, Op key) =>
        (curve.TryGetPlane(plane: out Plane plane, tolerance: context.Absolute.Value), plane) switch {
            (true, { IsValid: true } frame) => curve.ClosedCurveOrientation(plane: frame) switch {
                CurveOrientation.Clockwise => Direction.Of(value: -frame.Normal, context: context, key: key).Map(normal => (Plane: frame, Normal: normal.Value, Orientation: CurveOrientation.Clockwise)),
                CurveOrientation.CounterClockwise => Direction.Of(value: frame.Normal, context: context, key: key).Map(normal => (Plane: frame, Normal: normal.Value, Orientation: CurveOrientation.CounterClockwise)),
                _ => Fin.Fail<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)>(key.InvalidResult()),
            },
            _ => Fin.Fail<(Plane Plane, Vector3d Normal, CurveOrientation Orientation)>(key.InvalidResult()),
        };
    private static Fin<Unit> SimpleLoop(Polyline native, Context context, Op key) =>
        Optional(native.ToPolylineCurve()).ToFin(key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(
            state: (Context: context, Key: key),
            project: static (state, active) => Optional(Intersection.CurveSelf(curve: active, tolerance: state.Context.Absolute.Value)).ToFin(state.Key.InvalidResult())
                .Bind(events => events.Count == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(state.Key.InvalidInput()))));
    private Fin<TResult> WithCurve<TState, TResult>(TState state, Func<TState, PolylineCurve, Fin<TResult>> project, Op key) =>
        Optional(Native.ToPolylineCurve()).ToFin(key.InvalidResult()).Bind(curve => new Lease<PolylineCurve>.Owned(Value: curve).Use(state: state, project: project));
    private static Fin<TResult> WithMassProperties<TState, TResult>(PolylineCurve curve, Context context, Func<TState, AreaMassProperties, Fin<TResult>> project, TState state, Op key) =>
        Optional(AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: context.Absolute.Value)).ToFin(key.InvalidResult())
            .Bind(props => new Lease<AreaMassProperties>.Owned(Value: props).Use(state: state, project: project));
    private Fin<TResult> WithMassProperties<TResult>(Func<AreaMassProperties, Op, Fin<TResult>> project, Op key) {
        return WithCurve(
            state: (Context, Key: key, Project: project),
            project: static (state, curve) => WithMassProperties(curve: curve, context: state.Context, project: static (s, mass) => s.Project(arg1: mass, arg2: s.Key), state, key: state.Key),
            key: key);
    }
}
