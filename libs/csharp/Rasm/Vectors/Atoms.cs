using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
internal static class AtomProjection {
    internal static Fin<TOut> Self<TSelf, TOut>(TSelf value, Op key) => typeof(TOut) == typeof(TSelf) ? Fin.Succ((TOut)(object)value!) : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(TSelf), outputType: typeof(TOut)));
    internal static Fin<TOut> Value<TValue, TOut>(TValue value, Op key) => key.AcceptValue(value: value).Map(static accepted => (TOut)(object)accepted!);
    internal static Fin<TOut> SelfOrValue<TSelf, TValue, TOut>(TSelf self, TValue value, Op key) => typeof(TOut) == typeof(TValue) ? Value<TValue, TOut>(value: value, key: key) : Self<TSelf, TOut>(value: self, key: key);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct VectorAngle {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= RhinoMath.TwoPI ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"VectorAngle must be in [0,2*pi] radians (got {value:R})."));
    internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot, Op key) =>
        key.AcceptValidated<VectorAngle>(candidate: pivot.Compute(a: a.Value, b: b.Value));
    internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, AnglePivot? pivot = null, Op? key = null) =>
        from left in Direction.Of(value: a, context: context, key: key.OrDefault())
        from right in Direction.Of(value: b, context: context, key: key.OrDefault())
        from angle in Of(a: left, b: right, pivot: pivot ?? AnglePivot.World, key: key.OrDefault())
        select angle;
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.SelfOrValue<VectorAngle, double, TOut>(self: this, value: Value, key: key);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct PositiveMagnitude { [BoundaryAdapter] static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = RhinoMath.IsValidDouble(x: value) && value > RhinoMath.ZeroTolerance ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"PositiveMagnitude requires a positive finite value (got {value:R}).")); }

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UnitInterval { [BoundaryAdapter] static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"UnitInterval must be in [0,1] (got {value:R}).")); }

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Dimension { [BoundaryAdapter] static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = value >= 1 ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"Dimension must be >= 1 (got {value}).")); }

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class BoundarySense { public static readonly BoundarySense Toward = new(key: 1, sign: 1.0), Away = new(key: -1, sign: -1.0); public double Sign { get; } }

[SmartEnum<int>]
public sealed partial class SignedAxis {
    public static readonly SignedAxis NegativeX = new(key: -1, world: -Vector3d.XAxis, axis: static frame => -frame.XAxis), PositiveX = new(key: 1, world: Vector3d.XAxis, axis: static frame => frame.XAxis);
    public static readonly SignedAxis NegativeY = new(key: -2, world: -Vector3d.YAxis, axis: static frame => -frame.YAxis), PositiveY = new(key: 2, world: Vector3d.YAxis, axis: static frame => frame.YAxis);
    public static readonly SignedAxis NegativeZ = new(key: -3, world: -Vector3d.ZAxis, axis: static frame => -frame.ZAxis), PositiveZ = new(key: 3, world: Vector3d.ZAxis, axis: static frame => frame.ZAxis);
    public Vector3d World { get; }
    internal Vector3d Of(Option<Plane> frame) => frame.Map(Axis).IfNone(World);
    internal static Seq<SignedAxis> Cardinal(bool planar) => toSeq(Items).Filter(axis => !planar || Math.Abs(value: axis.Key) < PositiveZ.Key);
    [UseDelegateFromConstructor] private partial Vector3d Axis(Plane frame);
}

[SmartEnum<int>]
public sealed partial class VectorRelation {
    public static readonly VectorRelation Oblique = new(key: 0), Parallel = new(key: 1), AntiParallel = new(key: -1), Perpendicular = new(key: 2);
    public static Fin<VectorRelation> Of(Vector3d a, Vector3d b, Context context, Op? key = null) =>
        from model in Optional(context).ToFin(key.OrDefault().MissingContext())
        from left in Direction.Of(value: a, context: model, key: key.OrDefault())
        from right in Direction.Of(value: b, context: model, key: key.OrDefault())
        select (left.Value.IsParallelTo(other: right.Value, angleTolerance: model.Angle.Value), left.Value.IsPerpendicularTo(other: right.Value, angleTolerance: model.Angle.Value)) switch {
            (1, _) => Parallel,
            (-1, _) => AntiParallel,
            (_, true) => Perpendicular,
            _ => Oblique,
        };
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.Self<VectorRelation, TOut>(value: this, key: key);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record AnglePivot {
    private AnglePivot() { }
    public sealed record WorldCase : AnglePivot; public sealed record FrameCase(Plane Value) : AnglePivot; public sealed record NormalCase(Direction Value) : AnglePivot;
    public static AnglePivot World { get; } = new WorldCase(); public static AnglePivot Frame(Plane frame) => new FrameCase(Value: frame); public static AnglePivot Normal(Direction normal) => new NormalCase(Value: normal);
    internal double Compute(Vector3d a, Vector3d b) => Switch(state: (A: a, B: b), worldCase: static (state, _) => Vector3d.VectorAngle(a: state.A, b: state.B), frameCase: static (state, frame) => Vector3d.VectorAngle(a: state.A, b: state.B, plane: frame.Value), normalCase: static (state, normal) => Vector3d.VectorAngle(v1: state.A, v2: state.B, vNormal: normal.Value.Value));
}

public readonly record struct Direction {
    private Direction(Vector3d value) => Value = value;
    public Vector3d Value { get; }
    public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext()).Bind(model => Of(value: value, tolerance: model.Absolute.Value, key: key));
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op? key = null) =>
        (value.IsValid, value.IsTiny(tolerance), value.Unitize()) switch {
            (true, false, true) => Fin.Succ(new Direction(value: value)),
            _ => Fin.Fail<Direction>(error: key.OrDefault().InvalidInput()),
        };
    public static Direction operator -(Direction direction) => new(value: -direction.Value);
    public static Vector3d operator *(Direction direction, double magnitude) => direction.Value * magnitude;
    public Direction Reflect(Direction normal) =>
        new(value: Transform.Mirror(pointOnMirrorPlane: Point3d.Origin, normalToMirrorPlane: normal.Value) * Value);
    public Fin<Direction> ParallelTransport(Seq<Plane> frames, Op? key = null) =>
        toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: frames.Count - 1))).Fold(
            initialState: Of(value: Value, tolerance: RhinoMath.ZeroTolerance, key: key.OrDefault()),
            f: (acc, i) => acc.Bind(prev => Of(value: Transform.PlaneToPlane(plane0: frames[index: i - 1], plane1: frames[index: i]) * prev.Value, tolerance: RhinoMath.ZeroTolerance, key: key.OrDefault())));
    public static Fin<Direction> Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key) =>
        from activeIncident in key.AcceptValidated<PositiveMagnitude>(candidate: etaIncident)
        from activeTransmitted in key.AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted)
        let exiting = incident.Value * normal.Value > 0.0
        let orientedNormal = exiting switch { true => -normal.Value, false => normal.Value }
        let eta = activeIncident.Value / activeTransmitted.Value
        let cosI = Math.Clamp(value: -(incident.Value * orientedNormal), min: -1.0, max: 1.0)
        let k = 1.0 - (eta * eta * (1.0 - (cosI * cosI)))
        from direction in k switch {
            double rootable when rootable > -RhinoMath.ZeroTolerance => Of(value: (eta * incident.Value) + (((eta * cosI) - Math.Sqrt(d: Math.Max(val1: 0.0, val2: rootable))) * orientedNormal), tolerance: RhinoMath.ZeroTolerance, key: key),
            _ => Fin.Fail<Direction>(error: key.InvalidResult()),
        }
        select direction;
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.SelfOrValue<Direction, Vector3d, TOut>(self: this, value: Value, key: key);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorSpan {
    private VectorSpan(Point3d anchor, Direction direction, PositiveMagnitude magnitude) { Anchor = anchor; Direction = direction; Magnitude = magnitude; }
    public Point3d Anchor { get; }
    public Direction Direction { get; }
    public PositiveMagnitude Magnitude { get; }
    public Vector3d Value => Direction * Magnitude.Value;
    public Line Axis => new(from: Anchor, to: Anchor + Value);
    public static Fin<VectorSpan> Of(Point3d anchor, Vector3d vector, Context context, Op? key = null) =>
        from direction in Direction.Of(value: vector, context: context, key: key.OrDefault())
        from span in Of(anchor: anchor, direction: direction, magnitude: vector.Length, key: key.OrDefault()) select span;
    internal static Fin<VectorSpan> Of(Point3d anchor, Direction direction, double magnitude, Op key) =>
        from point in key.AcceptValue(value: anchor)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: magnitude)
        let span = new VectorSpan(anchor: point, direction: direction, magnitude: length)
        from _ in guard(span.Axis.IsValid, key.InvalidResult())
        select span;
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
    internal Fin<TOut> Project<TOut>(Op key) => typeof(TOut) switch {
        Type t when t == typeof(Direction) => Direction.Project<TOut>(key: key),
        Type t when t == typeof(Vector3d) => AtomProjection.Value<Vector3d, TOut>(value: Value, key: key),
        Type t when t == typeof(Line) => AtomProjection.Value<Line, TOut>(value: Axis, key: key),
        Type t when t == typeof(double) => AtomProjection.Value<double, TOut>(value: Magnitude.Value, key: key),
        _ => AtomProjection.Self<VectorSpan, TOut>(value: this, key: key),
    };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorFrame {
    private VectorFrame(Plane value) => Value = value;
    public Plane Value { get; }
    public static Fin<VectorFrame> Of(Point3d origin, Vector3d normal, Option<Vector3d> xHint, Context context, Op? key = null) =>
        from point in key.OrDefault().AcceptValue(value: origin)
        from z in Direction.Of(value: normal, context: context, key: key.OrDefault())
        let tangent = xHint.Map(raw => raw - (z.Value * (raw * z.Value))).Filter(v => !v.IsTiny(context.Absolute.Value)).IfNone(SeedPerpendicular(axis: z.Value))
        from x in Direction.Of(value: tangent, context: context, key: key.OrDefault())
        from y in Direction.Of(value: Vector3d.CrossProduct(a: z.Value, b: x.Value), context: context, key: key.OrDefault())
        let frame = new Plane(origin: point, xDirection: x.Value, yDirection: y.Value)
        from valid in guard(frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis) && Vector3d.AreRighthanded(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis), key.OrDefault().InvalidResult()).Bind(_ => key.OrDefault().AcceptValue(value: frame))
        select new VectorFrame(value: valid);
    public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op? key = null) =>
        CloudKernel.BishopChainOf(points: points, initialNormal: initialNormal, closed: closed, context: context, key: key.OrDefault()).Bind(planes => planes.TraverseM(p => Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key.OrDefault())).As());
    internal static Vector3d SeedPerpendicular(Vector3d axis) {
        Vector3d guide = Math.Abs(value: axis * Vector3d.ZAxis) switch { < 0.9 => Vector3d.ZAxis, _ => Vector3d.XAxis };
        Vector3d seed = Vector3d.CrossProduct(a: axis, b: guide);
        return seed.Unitize() switch { true => seed, false => Vector3d.XAxis };
    }
    internal Fin<TOut> Project<TOut>(Op key) => typeof(TOut) switch {
        Type t when t == typeof(Plane) => AtomProjection.Value<Plane, TOut>(value: Value, key: key),
        Type t when t == typeof(Transform) => AtomProjection.Value<Transform, TOut>(value: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: Value), key: key),
        _ => AtomProjection.Self<VectorFrame, TOut>(value: this, key: key),
    };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCone {
    private VectorCone(Point3d apex, Direction axis, VectorAngle halfAngle) { Apex = apex; Axis = axis; HalfAngle = halfAngle; }
    public Point3d Apex { get; }
    public Direction Axis { get; }
    public VectorAngle HalfAngle { get; }
    public double SolidAngle => RhinoMath.TwoPI * (1.0 - Math.Cos(d: HalfAngle.Value));
    public static Fin<VectorCone> Of(Point3d apex, Vector3d axis, double halfAngleRadians, Context context, Op? key = null) =>
        from anchor in key.OrDefault().AcceptValue(value: apex)
        from direction in Direction.Of(value: axis, context: context, key: key.OrDefault())
        from angle in key.OrDefault().AcceptValidated<VectorAngle>(candidate: halfAngleRadians)
        from _ in guard(angle.Value <= Math.PI, key.OrDefault().InvalidInput())
        select new VectorCone(apex: anchor, axis: direction, halfAngle: angle);
    public Fin<bool> Contains(Vector3d query, Context context, Op? key = null) {
        VectorCone cone = this;
        return from probe in Direction.Of(value: query, context: context, key: key.OrDefault())
               from angle in VectorAngle.Of(a: cone.Axis, b: probe, pivot: AnglePivot.World, key: key.OrDefault())
               select angle.Value <= cone.HalfAngle.Value;
    }
    public static Fin<VectorCone> Enclose(VectorCone left, VectorCone right, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from _ in guard(left.Apex.DistanceTo(other: right.Apex) <= model.Absolute.Value, op.InvalidInput())
               from between in VectorAngle.Of(a: left.Axis, b: right.Axis, pivot: AnglePivot.World, key: op)
               let envelope = (Theta: between.Value, A: left.HalfAngle.Value, B: right.HalfAngle.Value, Tolerance: model.Angle.Value, Half: (between.Value + left.HalfAngle.Value + right.HalfAngle.Value) * 0.5)
               let cross = Vector3d.CrossProduct(a: left.Axis.Value, b: right.Axis.Value)
               let rotationAxis = cross.IsTiny(model.Absolute.Value) switch { true => VectorFrame.SeedPerpendicular(axis: left.Axis.Value), false => cross }
               from result in (envelope.Theta + envelope.B <= envelope.A + envelope.Tolerance, envelope.Theta + envelope.A <= envelope.B + envelope.Tolerance, envelope.Theta <= envelope.Tolerance) switch {
                   (true, _, _) => Fin.Succ(left),
                   (_, true, _) => Fin.Succ(right),
                   (_, _, true) => Of(apex: left.Apex, axis: (envelope.A >= envelope.B ? left : right).Axis.Value, halfAngleRadians: Math.Max(val1: envelope.A, val2: envelope.B), context: model, key: op),
                   _ => guard(envelope.Half <= Math.PI + envelope.Tolerance, op.InvalidInput())
                       .Bind(_ => Direction.Of(value: Transform.Rotation(angleRadians: envelope.Half - envelope.A, rotationAxis: rotationAxis, rotationCenter: Point3d.Origin) * left.Axis.Value, context: model, key: op))
                       .Bind(axis => Of(apex: left.Apex, axis: axis.Value, halfAngleRadians: Math.Min(val1: Math.PI, val2: envelope.Half), context: model, key: op)),
               }
               select result;
    }
    public Fin<Seq<Direction>> PartitionBy(int sectors, Context context, Op? key = null) {
        Op op = key.OrDefault();
        VectorCone cone = this;
        return from sectorCount in op.AcceptValidated<Dimension>(candidate: sectors)
               from rim in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: cone.Axis.Value), context: context, key: op)
               let stepAngle = RhinoMath.TwoPI / sectorCount.Value
               let lateral = Math.Sin(a: cone.HalfAngle.Value)
               let coaxial = Math.Cos(d: cone.HalfAngle.Value) * cone.Axis.Value
               from rays in toSeq(Enumerable.Range(start: 0, count: sectorCount.Value)).TraverseM(i =>
                   Direction.Of(
                       value: coaxial + (lateral * (Transform.Rotation(angleRadians: stepAngle * i, rotationAxis: cone.Axis.Value, rotationCenter: Point3d.Origin) * rim.Value)),
                       context: context,
                       key: op)).As()
               select rays;
    }
}
