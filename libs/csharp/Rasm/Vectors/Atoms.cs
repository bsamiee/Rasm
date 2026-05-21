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
               from direction in Direction.Of(value: vector, context: context, key: op)
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
public readonly record struct VectorFrame {
    private VectorFrame(Plane value) => Value = value;
    public Plane Value { get; }
    public static Fin<VectorFrame> Of(Point3d origin, Vector3d normal, Option<Vector3d> xHint, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from point in op.AcceptValue(value: origin)
               from z in Direction.Of(value: normal, context: context, key: op)
               from x in XDirectionOf(normal: z, hint: xHint, context: context, key: op)
               from y in Direction.Of(value: Vector3d.CrossProduct(a: z.Value, b: x.Value), context: context, key: op)
               let frame = new Plane(origin: point, xDirection: x.Value, yDirection: y.Value)
               from valid in (frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis) && Vector3d.AreRighthanded(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis)) switch {
                   true => op.AcceptValue(value: frame),
                   false => Fin.Fail<Plane>(error: op.InvalidResult()),
               }
               select new VectorFrame(value: valid);
    }
    private static Fin<Direction> XDirectionOf(Direction normal, Option<Vector3d> hint, Context context, Op key) {
        Vector3d candidate = normal.Value;
        return hint.Case switch {
            Vector3d raw => Direction.Of(value: raw - (normal.Value * (raw * normal.Value)), context: context, key: key),
            _ => candidate.PerpendicularTo(other: normal.Value) switch {
                true => Direction.Of(value: candidate, context: context, key: key),
                false => Fin.Fail<Direction>(error: key.InvalidResult()),
            },
        };
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorFrame) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Plane) => key.AcceptValue(value: Value).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Transform) => key.AcceptValue(value: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: Value)).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorFrame), outputType: typeof(TOut))),
        };
}
