using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record VectorFault : Rasm.Domain.Expected {
    private VectorFault() { }
    public sealed record Invalid(Op Key, string Requirement) : VectorFault {
        public override string Message => $"Vector operation '{Key}' requires {Requirement}.";
        public override string Category => "Vector";
    }
    public sealed record Unsupported(Op Key, Type Source, Type Output) : VectorFault {
        public override string Message => $"Vector operation '{Key}' does not support '{Source.Name}' as '{Output.Name}'.";
        public override string Category => "Vector";
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct VectorAngle {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= RhinoMath.TwoPI
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"VectorAngle must be in [0,2*pi] radians (got {value:R})."));
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
               from angle in Vector3d.VectorAngle(a: left.Value, b: right.Value).TryCreateValidated<VectorAngle>().ToFin()
               select angle.Value switch {
                   double value when value <= tolerance => Parallel,
                   double value when Math.Abs(value: Math.PI - value) <= tolerance => AntiParallel,
                   double value when Math.Abs(value: (Math.PI * 0.5) - value) <= tolerance => Perpendicular,
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
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op? key = null) =>
        Unitized(value: value, tolerance: tolerance, op: key ?? Op.Of(name: nameof(Direction)));
    public static Direction operator -(Direction direction) => new(value: -direction.Value);
    public static Vector3d operator *(Direction direction, double magnitude) => direction.Value * magnitude;
    public static Direction Negate(Direction direction) => -direction;
    public static Vector3d Multiply(Direction direction, double magnitude) => direction * magnitude;
    [BoundaryAdapter]
    private static Fin<Direction> Unitized(Vector3d value, double tolerance, Op op) {
        Vector3d candidate = value;
        return (candidate.IsValid, candidate.IsTiny(tolerance), candidate.Unitize(), candidate.IsValid) switch {
            (true, false, true, true) => Fin.Succ(new Direction(value: candidate)),
            _ => Fin.Fail<Direction>(error: new VectorFault.Invalid(Key: op, Requirement: "a valid non-tiny unitizable Vector3d")),
        };
    }
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
               from magnitude in RhinoMath.IsValidDouble(x: vector.Length) && vector.Length > context.Absolute.Value
                   ? Fin.Succ(vector.Length)
                   : Fin.Fail<double>(error: new VectorFault.Invalid(Key: op, Requirement: "positive finite vector magnitude"))
               select new VectorSpan(anchor: point, direction: direction, magnitude: magnitude);
    }
    internal static Fin<VectorSpan> Of(Point3d anchor, Direction direction, double magnitude, Op key) =>
        (key.AcceptValue(value: anchor),
         RhinoMath.IsValidDouble(x: magnitude) && magnitude > RhinoMath.ZeroTolerance
            ? Fin.Succ(magnitude)
            : Fin.Fail<double>(error: new VectorFault.Invalid(Key: key, Requirement: "positive finite span magnitude")))
            .Apply((point, length) => new VectorSpan(anchor: point, direction: direction, magnitude: length))
            .As()
            .Bind(span => span.Axis.IsValid ? Fin.Succ(span) : Fin.Fail<VectorSpan>(key.InvalidResult()));
}
