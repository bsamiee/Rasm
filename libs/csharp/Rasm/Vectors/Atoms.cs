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

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Dimension {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 1
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"Dimension must be >= 1 (got {value})."));
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
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorRelation), outputType: typeof(TOut))),
        };
}

[SmartEnum<int>]
public sealed partial class CurveProjection {
    public static readonly CurveProjection Tangent = new(key: 0,
        sample: static (curve, t) => Fin.Succ((object)curve.TangentAt(t: t)));
    public static readonly CurveProjection Curvature = new(key: 1,
        sample: static (curve, t) => Fin.Succ((object)curve.CurvatureAt(t: t)));
    public static readonly CurveProjection FrenetFrame = new(key: 2,
        sample: static (curve, t) => curve.FrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection BishopFrame = new(key: 3,
        sample: static (curve, t) => curve.PerpendicularFrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection ArcLength = new(key: 4,
        sample: static (curve, t) => Fin.Succ((object)curve.GetLength(new Interval(curve.Domain.T0, t))));
    public static readonly CurveProjection RotationMinimizing = new(key: 5,
        sample: static (curve, t) => RmfFrameAt(curve: curve, parameter: t));
    private static Fin<object> RmfFrameAt(Curve curve, double parameter) =>
        !curve.Domain.IncludesParameter(t: parameter)
            ? Fin.Fail<object>(Op.Of().InvalidInput())
            : curve.PerpendicularFrameAt(t: parameter, plane: out Plane frame) && frame.IsValid
                ? Fin.Succ((object)frame)
                : Fin.Fail<object>(Op.Of().InvalidResult());
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter);
    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from _ in guard(curve.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: curve, parameter: parameter).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in (raw, typeof(TOut)) switch {
            (Vector3d v, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: (TOut)(object)v),
            (Vector3d v, Type t) when t == typeof(Direction) => Direction.Of(value: v, context: context, key: key).Bind(d => d.Project<TOut>(key: key)),
            (Vector3d v, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)v.Length),
            (Plane p, Type t) when t == typeof(Plane) => key.AcceptValue(value: (TOut)(object)p),
            (Plane p, Type t) when t == typeof(VectorFrame) => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key).Bind(f => f.Project<TOut>(key: key)),
            (double d, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)d),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(CurveProjection), outputType: typeof(TOut))),
        }
        select output;
}

[SmartEnum<int>]
public sealed partial class SurfaceProjection {
    public static readonly SurfaceProjection PrincipalCurvatures = new(key: 0,
        sample: static (s, u, v) => s.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet
            ? Fin.Succ((object)Seq(sc.MaximumPrincipalCurvature, sc.MinimumPrincipalCurvature))
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly SurfaceProjection Gaussian = new(key: 1,
        sample: static (s, u, v) => s.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet
            ? Fin.Succ((object)sc.Gaussian)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly SurfaceProjection Mean = new(key: 2,
        sample: static (s, u, v) => s.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet
            ? Fin.Succ((object)sc.Mean)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly SurfaceProjection OsculatingCircle = new(key: 3,
        sample: static (s, u, v) => s.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet && sc.OsculatingCircle(0).IsValid
            ? Fin.Succ((object)sc.OsculatingCircle(0))
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly SurfaceProjection Normal = new(key: 4,
        sample: static (s, u, v) => s.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet
            ? Fin.Succ((object)sc.Normal)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Surface surface, double u, double v);
    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from _ in guard(surface.Domain(direction: 0).IncludesParameter(t: u) && surface.Domain(direction: 1).IncludesParameter(t: v), key.InvalidInput())
        from raw in Sample(surface: surface, u: u, v: v).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in (raw, typeof(TOut)) switch {
            (double d, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)d),
            (Circle c, Type t) when t == typeof(Circle) => key.AcceptValue(value: (TOut)(object)c),
            (Vector3d n, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: (TOut)(object)n),
            (Seq<double> ks, Type t) when t == typeof(Seq<double>) => key.AcceptValue(value: (TOut)(object)ks),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SurfaceProjection), outputType: typeof(TOut))),
        }
        select output;
}

[SmartEnum<int>]
public sealed partial class MotionInterpolation {
    public static readonly MotionInterpolation Linear = new(key: 0,
        interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: false));
    public static readonly MotionInterpolation Slerp = new(key: 1,
        interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: true));
    public static readonly MotionInterpolation Screw = new(key: 3,
        interpolate: static (a, b, t) => ScrewInterpolate(a: a, b: b, t: t));
    [UseDelegateFromConstructor] internal partial Plane Interpolate(Plane a, Plane b, double t);

    private static Plane InterpolatePlanes(Plane a, Plane b, double t, bool spherical) {
        if (a.EpsilonEquals(other: b, epsilon: RhinoMath.ZeroTolerance)) return a;
        Rhino.Geometry.Quaternion qa = Rhino.Geometry.Quaternion.Rotation(plane0: Plane.WorldXY, plane1: a);
        Rhino.Geometry.Quaternion qb = Rhino.Geometry.Quaternion.Rotation(plane0: Plane.WorldXY, plane1: b);
        Rhino.Geometry.Quaternion qt = spherical
            ? Rhino.Geometry.Quaternion.Slerp(a: qa, b: qb, t: t)
            : Rhino.Geometry.Quaternion.Lerp(a: qa, b: qb, t: t);
        Plane oriented = qt.GetRotation(plane: out Plane raw) ? raw : a;
        Point3d origin = a.Origin;
        origin.Interpolate(pA: a.Origin, pB: b.Origin, t: t);
        return new Plane(origin: origin, xDirection: oriented.XAxis, yDirection: oriented.YAxis);
    }
    private static Plane ScrewInterpolate(Plane a, Plane b, double t) {
        if (a.EpsilonEquals(other: b, epsilon: RhinoMath.ZeroTolerance)) return a;
        DualQuaternion dqA = DualQuaternion.Of(transform: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: a));
        DualQuaternion dqB = DualQuaternion.Of(transform: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: b));
        DualQuaternion dqT = DualQuaternion.ScLerp(a: dqA, b: dqB, t: t);
        Plane result = Plane.WorldXY;
        _ = result.Transform(xform: dqT.ToTransform());
        return result;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SurfaceSpace {
    private SurfaceSpace(Surface native, Context tolerance) { Native = native; Tolerance = tolerance; }
    public Surface Native { get; }
    public Context Tolerance { get; }
    public static Fin<SurfaceSpace> Of(Surface native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               select new SurfaceSpace(native: active, tolerance: ctx);
    }
    public Fin<TOut> Sample<TOut>(SurfaceProjection projection, double u, double v, Op? key = null) {
        Op op = key.OrDefault();
        Surface native = Native; Context tolerance = Tolerance;
        return Optional(projection).ToFin(op.InvalidInput()).Bind(p => p.Project<TOut>(surface: native, u: u, v: v, context: tolerance, key: op));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Quaternion(double W, double X, double Y, double Z) {
    public bool IsValid => RhinoMath.IsValidDouble(x: W) && RhinoMath.IsValidDouble(x: X) && RhinoMath.IsValidDouble(x: Y) && RhinoMath.IsValidDouble(x: Z);
    public double Norm => Math.Sqrt(d: (W * W) + (X * X) + (Y * Y) + (Z * Z));
    public Quaternion Conjugate => new(W: W, X: -X, Y: -Y, Z: -Z);
    public static readonly Quaternion Identity = new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0);
    public static Fin<Quaternion> Of(double angleRadians, Direction axis, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: angleRadians)
            ? Fin.Succ(FromAxisAngle(angle: angleRadians, axis: axis.Value))
            : Fin.Fail<Quaternion>(op.InvalidInput());
    }
    public static Fin<Quaternion> Of(Plane from, Plane to, Op? key = null) {
        Op op = key.OrDefault();
        return Transform.PlaneToPlane(plane0: from, plane1: to).GetQuaternion(quaternion: out Rhino.Geometry.Quaternion rq)
            ? op.AcceptValue(value: new Quaternion(W: rq.A, X: rq.B, Y: rq.C, Z: rq.D))
            : Fin.Fail<Quaternion>(op.InvalidResult());
    }
    private static Quaternion FromAxisAngle(double angle, Vector3d axis) {
        double half = angle * 0.5; double s = Math.Sin(a: half);
        return new Quaternion(W: Math.Cos(d: half), X: axis.X * s, Y: axis.Y * s, Z: axis.Z * s);
    }
    public Quaternion Normalize() {
        double n = Norm;
        return n > RhinoMath.ZeroTolerance ? new Quaternion(W: W / n, X: X / n, Y: Y / n, Z: Z / n) : Identity;
    }
    public static Quaternion operator *(Quaternion a, Quaternion b) => new(
        W: (a.W * b.W) - (a.X * b.X) - (a.Y * b.Y) - (a.Z * b.Z),
        X: (a.W * b.X) + (a.X * b.W) + (a.Y * b.Z) - (a.Z * b.Y),
        Y: (a.W * b.Y) - (a.X * b.Z) + (a.Y * b.W) + (a.Z * b.X),
        Z: (a.W * b.Z) + (a.X * b.Y) - (a.Y * b.X) + (a.Z * b.W));
    public static Quaternion operator +(Quaternion a, Quaternion b) => new(W: a.W + b.W, X: a.X + b.X, Y: a.Y + b.Y, Z: a.Z + b.Z);
    // Shortest-arc SLERP: flip on cos<0; lerp+normalize fallback when cos>1-√ε.
    public static Quaternion Slerp(Quaternion a, Quaternion b, double t) {
        double cos = (a.W * b.W) + (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        Quaternion target = cos < 0.0 ? new Quaternion(W: -b.W, X: -b.X, Y: -b.Y, Z: -b.Z) : b;
        cos = Math.Abs(value: cos);
        if (cos > 1.0 - RhinoMath.SqrtEpsilon) {
            Quaternion linear = new(W: a.W + (t * (target.W - a.W)), X: a.X + (t * (target.X - a.X)), Y: a.Y + (t * (target.Y - a.Y)), Z: a.Z + (t * (target.Z - a.Z)));
            return linear.Normalize();
        }
        double theta = Math.Acos(d: cos); double sinTheta = Math.Sin(a: theta);
        double wa = Math.Sin(a: (1.0 - t) * theta) / sinTheta; double wb = Math.Sin(a: t * theta) / sinTheta;
        return new Quaternion(W: (wa * a.W) + (wb * target.W), X: (wa * a.X) + (wb * target.X), Y: (wa * a.Y) + (wb * target.Y), Z: (wa * a.Z) + (wb * target.Z));
    }
    public Transform ToTransform() {
        Rhino.Geometry.Quaternion rq = new(a: W, b: X, c: Y, d: Z);
        return rq.GetRotation(xform: out Transform rotation) ? rotation : Transform.Identity;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DualQuaternion(Quaternion Real, Quaternion Dual) {
    public bool IsValid => Real.IsValid && Dual.IsValid;
    public static DualQuaternion Of(Transform transform) {
        _ = transform.GetQuaternion(quaternion: out Rhino.Geometry.Quaternion rq);
        Quaternion real = new Quaternion(W: rq.A, X: rq.B, Y: rq.C, Z: rq.D).Normalize();
        _ = transform.DecomposeAffine(out Vector3d t, out Transform _);
        Quaternion translationQ = new(W: 0.0, X: t.X, Y: t.Y, Z: t.Z);
        Quaternion dual = translationQ * real;
        return new DualQuaternion(Real: real, Dual: new Quaternion(W: 0.5 * dual.W, X: 0.5 * dual.X, Y: 0.5 * dual.Y, Z: 0.5 * dual.Z));
    }
    public static DualQuaternion operator *(DualQuaternion a, DualQuaternion b) =>
        new(Real: a.Real * b.Real, Dual: (a.Real * b.Dual) + (a.Dual * b.Real));
    // Kavan 2006 ScLerp: SLERP Real, lerp Dual, then orthogonalise Dual against Real.
    public static DualQuaternion ScLerp(DualQuaternion a, DualQuaternion b, double t) {
        Quaternion real = Quaternion.Slerp(a: a.Real, b: b.Real, t: t);
        Quaternion dual = new(W: a.Dual.W + (t * (b.Dual.W - a.Dual.W)), X: a.Dual.X + (t * (b.Dual.X - a.Dual.X)), Y: a.Dual.Y + (t * (b.Dual.Y - a.Dual.Y)), Z: a.Dual.Z + (t * (b.Dual.Z - a.Dual.Z)));
        double dot = (real.W * dual.W) + (real.X * dual.X) + (real.Y * dual.Y) + (real.Z * dual.Z);
        Quaternion ortho = new(W: dual.W - (dot * real.W), X: dual.X - (dot * real.X), Y: dual.Y - (dot * real.Y), Z: dual.Z - (dot * real.Z));
        return new DualQuaternion(Real: real, Dual: ortho);
    }
    public Transform ToTransform() {
        Transform rotation = Real.ToTransform();
        double tw = Dual.W; double tx = Dual.X; double ty = Dual.Y; double tz = Dual.Z;
        Quaternion translationQ = new Quaternion(W: tw, X: tx, Y: ty, Z: tz) * Real.Conjugate;
        Vector3d translation = new(x: 2.0 * translationQ.X, y: 2.0 * translationQ.Y, z: 2.0 * translationQ.Z);
        return rotation * Transform.Translation(motion: translation);
    }
}

[SmartEnum<int>]
public sealed partial class ConeProjection {
    public static readonly ConeProjection HalfAngle = new(key: 0, sample: static cone => cone.HalfAngle);
    public static readonly ConeProjection SolidAngle = new(key: 1, sample: static cone => cone.SolidAngle);
    public static readonly ConeProjection Axis = new(key: 2, sample: static cone => cone.Axis);
    public static readonly ConeProjection Apex = new(key: 3, sample: static cone => cone.Apex);
    [UseDelegateFromConstructor] private partial object Sample(VectorCone cone);
    internal Fin<TOut> Project<TOut>(VectorCone cone, Op key) =>
        (Sample(cone: cone), typeof(TOut)) switch {
            (VectorAngle a, Type t) when t == typeof(VectorAngle) => Fin.Succ((TOut)(object)a),
            (VectorAngle a, Type t) when t == typeof(double) => key.AcceptValue(value: a.Value).Map(static x => (TOut)(object)x),
            (double d, Type t) when t == typeof(double) => key.AcceptValue(value: d).Map(static x => (TOut)(object)x),
            (Direction d, Type t) when t == typeof(Direction) => Fin.Succ((TOut)(object)d),
            (Direction d, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: d.Value).Map(static x => (TOut)(object)x),
            (Point3d p, Type t) when t == typeof(Point3d) => key.AcceptValue(value: p).Map(static x => (TOut)(object)x),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(ConeProjection), outputType: typeof(TOut))),
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
        return (candidate.IsValid, candidate.IsTiny(tolerance), candidate.Unitize()) switch {
            (true, false, true) => Fin.Succ(new Direction(value: candidate)),
            _ => Fin.Fail<Direction>(error: op.InvalidInput()),
        };
    }
    public static Direction operator -(Direction direction) => new(value: -direction.Value);
    public static Vector3d operator *(Direction direction, double magnitude) => direction.Value * magnitude;
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
               from x in xHint.Case switch {
                   Vector3d raw => Direction.Of(value: raw - (z.Value * (raw * z.Value)), context: context, key: op),
                   _ => Direction.Of(value: SeedPerpendicular(axis: z.Value), context: context, key: op),
               }
               from y in Direction.Of(value: Vector3d.CrossProduct(a: z.Value, b: x.Value), context: context, key: op)
               let frame = new Plane(origin: point, xDirection: x.Value, yDirection: y.Value)
               from valid in (frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis) && Vector3d.AreRighthanded(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis)) switch {
                   true => op.AcceptValue(value: frame),
                   false => Fin.Fail<Plane>(error: op.InvalidResult()),
               }
               select new VectorFrame(value: valid);
    }
    public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return CloudKernel.BishopChainOf(points: points, initialNormal: initialNormal, closed: closed, context: context, key: op)
            .Bind(planes => planes.TraverseM(p => Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: op)).As());
    }
    internal static Vector3d SeedPerpendicular(Vector3d axis) {
        Vector3d seed = axis;
        _ = seed.PerpendicularTo(other: axis);
        return seed;
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorFrame) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Plane) => key.AcceptValue(value: Value).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Transform) => key.AcceptValue(value: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: Value)).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorFrame), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCone {
    private VectorCone(Point3d apex, Direction axis, VectorAngle halfAngle) {
        Apex = apex;
        Axis = axis;
        HalfAngle = halfAngle;
    }
    public Point3d Apex { get; }
    public Direction Axis { get; }
    public VectorAngle HalfAngle { get; }
    public double SolidAngle => RhinoMath.TwoPI * (1.0 - Math.Cos(d: HalfAngle.Value));
    public static Fin<VectorCone> Of(Point3d apex, Vector3d axis, double halfAngleRadians, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from anchor in op.AcceptValue(value: apex)
               from direction in Direction.Of(value: axis, context: context, key: op)
               from angle in op.AcceptValidated<VectorAngle>(candidate: halfAngleRadians)
               from _ in guard(angle.Value <= Math.PI, op.InvalidInput())
               select new VectorCone(apex: anchor, axis: direction, halfAngle: angle);
    }
    public Fin<bool> Contains(Vector3d query, Context context, Op? key = null) {
        Op op = key.OrDefault();
        Direction axis = Axis;
        double halfAngle = HalfAngle.Value;
        return from probe in Direction.Of(value: query, context: context, key: op)
               from angle in VectorAngle.Of(a: axis, b: probe, pivot: AnglePivot.World, key: op)
               select angle.Value <= halfAngle;
    }
    public static Fin<VectorCone> Enclose(VectorCone left, VectorCone right, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from _ in guard(left.Apex.DistanceTo(other: right.Apex) <= model.Absolute.Value, op.InvalidInput())
               from between in VectorAngle.Of(a: left.Axis, b: right.Axis, pivot: AnglePivot.World, key: op)
               let widest = Math.Max(val1: left.HalfAngle.Value, val2: right.HalfAngle.Value)
               let combined = Math.Min(val1: Math.PI, val2: Math.Max(val1: widest, val2: (between.Value * 0.5) + widest))
               from bisector in (left.Axis.Value + right.Axis.Value) switch {
                   Vector3d sum when !sum.IsTiny() => Direction.Of(value: sum, context: model, key: op),
                   _ => Direction.Of(value: VectorFrame.SeedPerpendicular(axis: left.Axis.Value), context: model, key: op),
               }
               from result in Of(apex: left.Apex, axis: bisector.Value, halfAngleRadians: combined, context: model, key: op)
               select result;
    }
    public Fin<Seq<Direction>> PartitionBy(int sectors, Context context, Op? key = null) {
        Op op = key.OrDefault();
        Vector3d axisVector = Axis.Value;
        double halfAngle = HalfAngle.Value;
        return from _ in guard(sectors >= 1, op.InvalidInput())
               from rim in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: axisVector), context: context, key: op)
               let stepAngle = RhinoMath.TwoPI / sectors
               let lateral = Math.Sin(a: halfAngle)
               let coaxial = Math.Cos(d: halfAngle) * axisVector
               let rimCross = Vector3d.CrossProduct(a: axisVector, b: rim.Value)
               from rays in toSeq(Enumerable.Range(start: 0, count: sectors)).TraverseM(i =>
                   Direction.Of(
                       value: coaxial + (lateral * ((Math.Cos(d: stepAngle * i) * rim.Value) + (Math.Sin(a: stepAngle * i) * rimCross))),
                       context: context,
                       key: op)).As()
               select rays;
    }
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(VectorCone) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(VectorAngle) => HalfAngle.Project<TOut>(key: key),
            Type t when t == typeof(double) => key.AcceptValue(value: SolidAngle).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Direction) => Axis.Project<TOut>(key: key),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: Axis.Value).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Point3d) => key.AcceptValue(value: Apex).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCone), outputType: typeof(TOut))),
        };
}
