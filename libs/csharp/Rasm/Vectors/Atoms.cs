using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct VectorAngle {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= RhinoMath.TwoPI
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"VectorAngle must be in [0,2*pi] radians (got {value:R})."));
    internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot, Op key) =>
        key.AcceptValidated<VectorAngle>(candidate: pivot.Compute(a: a.Value, b: b.Value));
    internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, AnglePivot? pivot = null, Op? key = null) {
        Op op = key.OrDefault();
        return from left in Direction.Of(value: a, context: context, key: op)
               from right in Direction.Of(value: b, context: context, key: op)
               from angle in Of(a: left, b: right, pivot: pivot ?? AnglePivot.World, key: op)
               select angle;
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorAngle) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(double) => key.AcceptValue(value: Value).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorAngle), outputType: typeof(TOut))),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct PositiveMagnitude {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = RhinoMath.IsValidDouble(x: value) && value > RhinoMath.ZeroTolerance
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"PositiveMagnitude requires a positive finite value (got {value:R})."));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class BoundarySense {
    public static readonly BoundarySense Toward = new(key: 1, sign: 1.0), Away = new(key: -1, sign: -1.0);
    public double Sign { get; }
}

[SmartEnum<int>]
public sealed partial class SignedAxis {
    public static readonly SignedAxis PositiveX = new(key: 1, world: Vector3d.XAxis, axis: static frame => frame.XAxis), NegativeX = new(key: -1, world: -Vector3d.XAxis, axis: static frame => -frame.XAxis);
    public static readonly SignedAxis PositiveY = new(key: 2, world: Vector3d.YAxis, axis: static frame => frame.YAxis), NegativeY = new(key: -2, world: -Vector3d.YAxis, axis: static frame => -frame.YAxis);
    public static readonly SignedAxis PositiveZ = new(key: 3, world: Vector3d.ZAxis, axis: static frame => frame.ZAxis), NegativeZ = new(key: -3, world: -Vector3d.ZAxis, axis: static frame => -frame.ZAxis);
    public Vector3d World { get; }
    internal Vector3d Of(Option<Plane> frame) =>
        frame.Map(Axis).IfNone(World);
    internal static Seq<SignedAxis> Cardinal(bool planar) =>
        planar switch {
            true => Seq(NegativeX, PositiveX, NegativeY, PositiveY),
            false => Seq(NegativeX, PositiveX, NegativeY, PositiveY, NegativeZ, PositiveZ),
        };
    [UseDelegateFromConstructor] private partial Vector3d Axis(Plane frame);
}

[SmartEnum<int>]
public sealed partial class VectorRelation {
    public static readonly VectorRelation Oblique = new(key: 0), Parallel = new(key: 1), AntiParallel = new(key: -1), Perpendicular = new(key: 2);
    internal IntersectionTangency Tangency => (Equals(Parallel), Equals(AntiParallel)) switch {
        (true, _) or (_, true) => IntersectionTangency.Tangent,
        _ => IntersectionTangency.Transversal,
    };
    public static Fin<VectorRelation> Of(Vector3d a, Vector3d b, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from left in Direction.Of(value: a, context: model, key: op)
               from right in Direction.Of(value: b, context: model, key: op)
               select (left.Value.IsParallelTo(other: right.Value, angleTolerance: model.Angle.Value), left.Value.IsPerpendicularTo(other: right.Value, angleTolerance: model.Angle.Value)) switch {
                   (1, _) => Parallel,
                   (-1, _) => AntiParallel,
                   (_, true) => Perpendicular,
                   _ => Oblique,
               };
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorRelation) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(IntersectionTangency) => key.AcceptValue(value: Tangency).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorRelation), outputType: typeof(TOut))),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record AnglePivot {
    private AnglePivot() { }
    public sealed record WorldCase : AnglePivot;
    public sealed record FrameCase(Plane Value) : AnglePivot;
    public sealed record NormalCase(Direction Value) : AnglePivot;
    public static AnglePivot World => new WorldCase();
    public static AnglePivot Frame(Plane frame) => new FrameCase(Value: frame);
    public static AnglePivot Normal(Direction normal) => new NormalCase(Value: normal);
    internal double Compute(Vector3d a, Vector3d b) => Switch(
        state: (A: a, B: b),
        worldCase: static (state, _) => Vector3d.VectorAngle(a: state.A, b: state.B),
        frameCase: static (state, frame) => Vector3d.VectorAngle(a: state.A, b: state.B, plane: frame.Value),
        normalCase: static (state, normal) => Vector3d.VectorAngle(v1: state.A, v2: state.B, vNormal: normal.Value.Value));
}

public readonly record struct Direction {
    private Direction(Vector3d value) => Value = value;
    public Vector3d Value { get; }
    public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext())
            .Bind(model => Of(value: value, tolerance: model.Absolute.Value, key: key));
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op? key = null) {
        Op op = key.OrDefault();
        Vector3d candidate = value;
        return (candidate.IsValid, candidate.IsTiny(tolerance), candidate.Unitize(), candidate.IsValid) switch {
            (true, false, true, true) => Fin.Succ(new Direction(value: candidate)),
            _ => Fin.Fail<Direction>(error: op.InvalidInput()),
        };
    }
    public static Direction operator -(Direction direction) => new(value: -direction.Value);
    public static Vector3d operator *(Direction direction, double magnitude) => direction.Value * magnitude;
    public static Direction Negate(Direction direction) => -direction;
    public static Vector3d Multiply(Direction direction, double magnitude) => direction * magnitude;
    public Direction Reflect(Direction normal) =>
        new(value: Value - (2.0 * (Value * normal.Value) * normal.Value));
    public static Fin<Direction> Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key) =>
        from activeIncident in key.AcceptValidated<PositiveMagnitude>(candidate: etaIncident)
        from activeTransmitted in key.AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted)
        let exiting = incident.Value * normal.Value > 0.0
        let orientedNormal = exiting ? -normal.Value : normal.Value
        let eta = exiting ? activeTransmitted.Value / activeIncident.Value : activeIncident.Value / activeTransmitted.Value
        let cosI = Math.Clamp(value: -(incident.Value * orientedNormal), min: -1.0, max: 1.0)
        let k = 1.0 - (eta * eta * (1.0 - (cosI * cosI)))
        from direction in k switch {
            >= 0.0 => Of(value: (eta * incident.Value) + (((eta * cosI) - Math.Sqrt(k)) * orientedNormal), tolerance: RhinoMath.ZeroTolerance, key: key),
            double small when small > -RhinoMath.ZeroTolerance => Of(value: (eta * incident.Value) + (eta * cosI * orientedNormal), tolerance: RhinoMath.ZeroTolerance, key: key),
            _ => Fin.Fail<Direction>(error: key.InvalidResult()),
        }
        select direction;
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Direction) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: Value).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Direction), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorSpan {
    private VectorSpan(Point3d anchor, Direction direction, double magnitude) {
        Anchor = anchor;
        Direction = direction;
        Magnitude = magnitude;
    }
    public Point3d Anchor { get; }
    public Direction Direction { get; }
    public double Magnitude { get; }
    public Vector3d Value => Direction * Magnitude;
    public Line Axis => new(from: Anchor, to: Anchor + Value);
    public static Fin<VectorSpan> Of(Point3d anchor, Vector3d vector, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from point in op.AcceptValue(value: anchor)
               from direction in Rasm.Vectors.Direction.Of(value: vector, context: context, key: op)
               from magnitude in op.AcceptValidated<PositiveMagnitude>(candidate: vector.Length)
               select new VectorSpan(anchor: point, direction: direction, magnitude: magnitude.Value);
    }
    internal static Fin<VectorSpan> Of(Point3d anchor, Direction direction, double magnitude, Op key) =>
        (key.AcceptValue(value: anchor), key.AcceptValidated<PositiveMagnitude>(candidate: magnitude))
            .Apply((point, length) => new VectorSpan(anchor: point, direction: direction, magnitude: length.Value))
            .As()
            .Bind(span => guard(span.Axis.IsValid, key.InvalidResult()).Bind(_ => Fin.Succ(span)));
    internal Fin<(double X, double Y)> Components(Plane frame, Op key) {
        Vector3d value = Value;
        return key.AcceptValue(value: frame).Bind(validFrame =>
            Vector3d.Decompose(v: value, a: validFrame.XAxis, b: validFrame.YAxis, x: out double x, y: out double y) switch {
                true => (key.AcceptValue(value: x), key.AcceptValue(value: y))
                    .Apply(static (validX, validY) => (X: validX, Y: validY))
                    .As(),
                false => Fin.Fail<(double X, double Y)>(error: key.InvalidResult()),
            });
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorSpan) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Direction) => Direction.Project<TOut>(key: key),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: Value).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Line) => key.AcceptValue(value: Axis).Map(static value => (TOut)(object)value),
            Type t when t == typeof(double) => key.AcceptValue(value: Magnitude).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorSpan), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorRingShape(
    Vector3d Normal,
    double SignedArea,
    double Area,
    double Perimeter,
    double EdgeAspect,
    double Skewness,
    double PlanarityDeviation,
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
            project: static (state, curve) => {
                using AreaMassProperties? props = AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: state.Context.Absolute.Value);
                return from mass in Optional(props).ToFin(state.Key.InvalidResult())
                       from ring in OrientationOf(curve: curve, context: state.Context, key: state.Key)
                       from fit in BestFitOf(points: state.Points, key: state.Key)
                       from edgeAspect in EdgeAspectOf(native: state.Native, context: state.Context, key: state.Key)
                       from skewness in SkewnessOf(points: state.Points, normal: ring.Normal, key: state.Key)
                       from axes in AxesOf(mass: mass, key: state.Key)
                       from principal in PrincipalFrameOf(centroid: mass.Centroid, axes: axes, normal: ring.Normal, context: state.Context, key: state.Key)
                       let shape = new VectorRingShape(
                           Normal: ring.Normal,
                           SignedArea: ring.Orientation == CurveOrientation.CounterClockwise ? mass.Area : -mass.Area,
                           Area: mass.Area,
                           Perimeter: state.Native.Length,
                           EdgeAspect: edgeAspect,
                           Skewness: skewness,
                           PlanarityDeviation: fit.Deviation,
                           Centroid: mass.Centroid,
                           BestFitPlane: fit.Plane,
                           PrincipalFrame: principal,
                           PrincipalAxes: axes,
                           Convex: state.Native.IsConvexLoop(strictlyConvex: false),
                           Orientation: ring.Orientation)
                       from valid in shape.IsValid ? Fin.Succ(shape) : Fin.Fail<VectorRingShape>(state.Key.InvalidResult())
                       select valid;
            },
            key: key);
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
    private static Fin<Plane> PrincipalFrameOf(Point3d centroid, Seq<(double Moment, Vector3d Axis)> axes, Vector3d normal, Context context, Op key) =>
        axes.TraverseM(axis => Direction.Of(value: axis.Axis, context: context, key: key).Map(direction => (axis.Moment, Axis: direction.Value, Score: Math.Abs(direction.Value * normal)))).As()
            .Bind(valid => toSeq(valid.AsIterable().OrderBy(static axis => axis.Score)).Take(2) switch {
                Seq<(double Moment, Vector3d Axis, double Score)> chosen when chosen.Count == 2 => new Plane(
                    origin: centroid,
                    xDirection: chosen[0].Axis,
                    yDirection: Vector3d.CrossProduct(a: chosen[0].Axis, b: chosen[1].Axis) * normal >= 0.0 ? chosen[1].Axis : -chosen[1].Axis) switch {
                    { IsValid: true } frame => key.AcceptValue(value: frame),
                        _ => Fin.Fail<Plane>(error: key.InvalidResult()),
                    },
                _ => Fin.Fail<Plane>(error: key.InvalidResult()),
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
            .Bind(props => new Lease<AreaMassProperties>.Owned(Value: props).Use<TState, Fin<TResult>>(state: state, project: project));
    private Fin<TResult> WithMassProperties<TResult>(Func<AreaMassProperties, Op, Fin<TResult>> project, Op key) {
        return WithCurve(
            state: (Context, Key: key, Project: project),
            project: static (state, curve) => WithMassProperties(curve: curve, context: state.Context, project: static (s, mass) => s.Project(arg1: mass, arg2: s.Key), state, key: state.Key),
            key: key);
    }
}
