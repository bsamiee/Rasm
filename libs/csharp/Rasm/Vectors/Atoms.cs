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
    internal static Fin<VectorAngle> Of(Direction a, Direction b, Option<Plane> frame, Op key) =>
        frame.Map(plane => Vector3d.VectorAngle(a: a.Value, b: b.Value, plane: plane))
            .IfNone(() => Vector3d.VectorAngle(a: a.Value, b: b.Value))
            .TryCreateValidated<VectorAngle>()
            .ToFin()
            .BindFail(_ => Fin.Fail<VectorAngle>(error: key.InvalidResult()));
    internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, Op key) =>
        from left in Direction.Of(value: a, context: context, key: key)
        from right in Direction.Of(value: b, context: context, key: key)
        from angle in Of(a: left, b: right, frame: Option<Plane>.None, key: key)
        select angle;
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
    public static Fin<VectorRelation> Of(Vector3d a, Vector3d b, double tolerance, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(VectorRelation));
        return from left in Direction.Of(value: a, tolerance: RhinoMath.ZeroTolerance, key: op)
               from right in Direction.Of(value: b, tolerance: RhinoMath.ZeroTolerance, key: op)
               select (left.Value.IsParallelTo(other: right.Value, angleTolerance: tolerance), left.Value.IsPerpendicularTo(other: right.Value, angleTolerance: tolerance)) switch {
                   (1, _) => Parallel,
                   (-1, _) => AntiParallel,
                   (_, true) => Perpendicular,
                   _ => Oblique,
               };
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct Direction {
    private Direction(Vector3d value) => Value = value;
    public Vector3d Value { get; }
    public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null) =>
        Optional(context).ToFin((key ?? Op.Of(name: nameof(Direction))).MissingContext())
            .Bind(model => Of(value: value, tolerance: model.Absolute.Value, key: key));
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Direction));
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
        Op op = key ?? Op.Of(name: nameof(VectorSpan));
        return from point in op.AcceptValue(value: anchor)
               from direction in Rasm.Vectors.Direction.Of(value: vector, context: context, key: op)
               from magnitude in vector.Length.TryCreateValidated<PositiveMagnitude>().ToFin()
               select new VectorSpan(anchor: point, direction: direction, magnitude: magnitude);
    }
    internal static Fin<VectorSpan> Of(Point3d anchor, Direction direction, double magnitude, Op key) =>
        (key.AcceptValue(value: anchor),
         magnitude.TryCreateValidated<PositiveMagnitude>().ToFin())
            .Apply((point, length) => new VectorSpan(anchor: point, direction: direction, magnitude: length.Value))
            .As()
            .Bind(span => guard(span.Axis.IsValid, key.InvalidResult()).Bind(_ => Fin.Succ(span)));
    internal Fin<double> Component(Direction axis, Op key) =>
        key.AcceptValue(value: Value * axis.Value);
    internal Fin<(double X, double Y)> Components(Plane frame, Op key) {
        VectorSpan span = this;
        return from validFrame in key.AcceptValue(value: frame)
               from xAxis in Direction.Of(value: validFrame.XAxis, tolerance: RhinoMath.ZeroTolerance, key: key)
               from yAxis in Direction.Of(value: validFrame.YAxis, tolerance: RhinoMath.ZeroTolerance, key: key)
               from x in span.Component(axis: xAxis, key: key)
               from y in span.Component(axis: yAxis, key: key)
               select (X: x, Y: y);
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
internal readonly record struct VectorRing {
    private VectorRing(Seq<Point3d> points) => Points = points;
    internal Seq<Point3d> Points { get; }
    internal static Fin<VectorRing> Of(Seq<Point3d> points, Op key) =>
        from valid in points.TraverseM(point => key.AcceptValue(value: point)).As()
        from _ in guard(valid.Count >= 3, key.InvalidInput())
        select new VectorRing(points: valid);
    internal Fin<Vector3d> Normal(Op key) =>
        Direction.Of(value: AreaVector, tolerance: RhinoMath.ZeroTolerance, key: key).Map(static direction => direction.Value);
    internal Fin<double> Area(Op key) {
        Seq<Vector3d> fan = FanVectors;
        return key.AcceptValue(value: 0.5 * fan.Fold(initialState: 0.0, f: static (sum, value) => sum + value.Length));
    }
    internal Fin<double> Perimeter(Op key) =>
        key.AcceptValue(value: EdgeLengths.Fold(initialState: 0.0, f: static (sum, length) => sum + length));
    internal Fin<double> EdgeAspect(Op key) =>
        EdgeLengths.Fold(
            initialState: (Count: 0, Min: double.PositiveInfinity, Max: 0.0),
            f: static (range, length) => length > RhinoMath.ZeroTolerance
                ? (Count: range.Count + 1, Min: Math.Min(val1: range.Min, val2: length), Max: Math.Max(val1: range.Max, val2: length))
                : range) switch {
                    (Count: > 0, Min: double min, Max: double max) when RhinoMath.IsValidDouble(x: min) && min > RhinoMath.ZeroTolerance && RhinoMath.IsValidDouble(x: max) =>
                        key.AcceptValue(value: max / min),
                    _ => Fin.Fail<double>(error: key.InvalidResult()),
                };
    internal Fin<double> Skewness(Op key) {
        Seq<Point3d> points = Points;
        return points.Count switch {
            int count when count >= 3 => points.Map((point, index) => Vector3d.VectorAngle(a: points[(index + count - 1) % count] - point, b: points[(index + 1) % count] - point))
                .Fold(initialState: Fin.Succ((Max: 0.0, Ideal: (count - 2) * Math.PI / count)), f: static (state, angle) => state.Bind(s => angle.TryCreateValidated<VectorAngle>().ToFin()
                    .Map(a => (Max: Math.Max(val1: s.Max, val2: Math.Max(val1: (a.Value - s.Ideal) / (Math.PI - s.Ideal), val2: (s.Ideal - a.Value) / s.Ideal)), s.Ideal))))
                .Map(static state => state.Max),
            _ => Fin.Fail<double>(error: key.InvalidResult()),
        };
    }
    private Seq<double> EdgeLengths {
        get {
            Seq<Point3d> points = Points;
            return points.Map((point, index) => point.DistanceTo(other: points[(index + 1) % points.Count]));
        }
    }
    private Vector3d AreaVector {
        get {
            Seq<Vector3d> fan = FanVectors;
            return fan.Fold(initialState: Vector3d.Zero, f: static (sum, value) => sum + value);
        }
    }
    private Seq<Vector3d> FanVectors {
        get {
            Seq<Point3d> points = Points;
            return points.Map((point, index) => Vector3d.CrossProduct(a: point - points[0], b: points[(index + 1) % points.Count] - points[0]));
        }
    }
}
