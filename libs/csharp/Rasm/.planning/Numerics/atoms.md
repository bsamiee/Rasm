# [RASM_NUMERICS_ATOMS]

The typed vector-algebra primitive floor of `Rasm.Vectors` and the promoted output-projection owner every `.Project<TOut>` in the kernel routes through. The page owns the scalar admission floor (`EpsilonPolicy` owned epsilon constants over `double.IsFinite` — the host-neutral-shaped gate; `Dimension`/`PositiveMagnitude`/`UnitInterval` generated value objects), the direction/angle vocabulary (`BoundarySense`, `SignedAxis`, `VectorRelation`, `AnglePivot`, `VectorAngle`), the four vector-algebra models (`Direction` with Snell refraction and plane-sequence parallel transport, `VectorSpan`, `VectorFrame`, `VectorCone` with solid angle, envelope merge, and sector partition), and `AtomProjection` + `ProjectionRow` — the corpus-wide raw→typed projection dispatch, promoted from a buried internal helper to the named owner whose typed row table is the ONE sanctioned type-directed dispatch site in the kernel.

Rhino geometry value structs (`Vector3d`, `Point3d`, `Plane`, `Line`, `Transform`, `Circle`) remain the coordinate carriers per the kernel boundary law; the RhinoMath numeric primitives do not — finiteness is `double.IsFinite`, epsilons are `EpsilonPolicy` rows, and the full turn is `Math.Tau`. Every operation threads the `Rasm.Domain` `Op` key explicitly and fails through its typed fault factory; long pipelines lift the same key into `Eff<Env>` at the consumer, never a second threading paradigm.

## [01]-[INDEX]

- [02]-[SCALAR_FLOOR]: `EpsilonPolicy` owned epsilon constants + `Dimension`/`PositiveMagnitude`/`UnitInterval` `[ValueObject]` admission floor + `BoundarySense`/`SignedAxis`/`VectorRelation` smart enums + `AnglePivot` `[Union]` + `VectorAngle` — the scalar and angle vocabulary every higher owner composes.
- [03]-[VECTOR_ALGEBRA]: `Direction` (unit vector; reflect, Snell refract, plane-sequence parallel transport) · `VectorSpan` (anchor + direction + magnitude) · `VectorFrame` (orthonormal frame; Bishop chain via the `Spatial/neighbors` RMF owner) · `VectorCone` (solid angle, containment, envelope merge, sector partition).
- [04]-[PROJECTION_RAIL]: `ProjectionRow` typed row + `AtomProjection` — the promoted corpus-wide raw→typed output dispatch (`Rows`/`Self`/`Value`/`SelfOrValue`/`Values`/`Custom`/`Raw`); scattered `typeof(TOut)` switch branching is the deleted form.

## [02]-[SCALAR_FLOOR]

- Owner: `EpsilonPolicy` the `static` constants owner naming the kernel's two epsilon rows — `SqrtEpsilon` (`2⁻²⁶`, the square root of binary64 machine epsilon — near-unit and residual gates) and `ZeroTolerance` (`2⁻³²` — degeneracy floors); `double.IsFinite` is the finiteness predicate and `Math.Tau` the full turn, so no `RhinoMath` member survives on the numeric floor. `Dimension` `[ValueObject<int>]` (`>= 1` count), `PositiveMagnitude` `[ValueObject<double>]` (finite, `> ZeroTolerance`), `UnitInterval` `[ValueObject<double>]` (finite, `[0,1]`) are the generated scalar admission owners — `ValidateFactoryArguments` is the one admission seam, `TryCreate`/`Validate` the rail bridges, and every downstream signature that means "a count", "a positive length", or "a normalized parameter" carries the owner, never a raw primitive re-gated per call site. `BoundarySense` `[SmartEnum<int>]` (2, `Sign` column), `SignedAxis` `[SmartEnum<int>]` (6 cardinal signed axes; `World` column + `[UseDelegateFromConstructor]` frame-axis column; `Of(Option<Plane>)` resolves world-or-frame; `Cardinal(planar)` filters the planar four), `VectorRelation` `[SmartEnum<int>]` (4; `Of` classifies two vectors against the `Context` angle tolerance), `AnglePivot` `[Union]` (World/Frame/Normal measurement pivot with `Admit` + `Compute` dispatch over the three `Vector3d.VectorAngle` overloads), `VectorAngle` `[ValueObject<double>]` (`[0, Math.Tau]` radians; `Of(a, b, pivot)` measures through the pivot).
- Cases: `BoundarySense` `Toward(+1)` · `Away(-1)`; `SignedAxis` `NegativeX(-1)` · `PositiveX(1)` · `NegativeY(-2)` · `PositiveY(2)` · `NegativeZ(-3)` · `PositiveZ(3)`; `VectorRelation` `Oblique(0)` · `Parallel(1)` · `AntiParallel(-1)` · `Perpendicular(2)`; `AnglePivot` `WorldCase` · `FrameCase(Plane)` · `NormalCase(Direction)` (3).
- Entry: `Dimension.Validate`/`TryCreate`/`Create` (generated); `PositiveMagnitude`/`UnitInterval` likewise; `SignedAxis.Of(Option<Plane> frame)` returns the world or frame-resolved axis vector; `SignedAxis.Cardinal(bool planar)` the declared axis sweep; `VectorRelation.Of(Vector3d a, Vector3d b, Context context, Op? key = null)` classifies; `AnglePivot.World`/`Frame(plane)`/`Normal(direction)` construct; `VectorAngle.Of(Direction a, Direction b, AnglePivot pivot, Op key)` and the raw-vector overload `Of(Vector3d, Vector3d, Context, AnglePivot?, Op?)` measure; `VectorAngle.Project<TOut>(Op key)` routes self-or-value through the projection rail.
- Auto: the generated `ValidateFactoryArguments` hooks gate finiteness by `double.IsFinite` and bounds by the owner's invariant, so interior code never re-validates an admitted scalar; `AnglePivot.Admit` re-validates only the case payload (a frame's orthonormality via `Admit.Plane`, a normal via the `Direction` unit gate) and `Compute` dispatches the three `VectorAngle` overloads through the generated `Switch`; `VectorRelation.Of` admits both operands as `Direction` first, then reads `IsParallelTo`/`IsPerpendicularTo` under the context angle tolerance as one tuple pattern.
- Receipt: none — scalar owners are admission evidence themselves; the `Sign` column on `BoundarySense` and the `World`/axis columns on `SignedAxis` are behavior rows, not receipts.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject<T>]`, `[SmartEnum<int>]`, `[Union]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Fin`, `Option`, `Seq`, `guard`), Rasm.Domain (project — `Op` key + fault factory, `Context` tolerance bundle, the `Admit` vocabulary + `AcceptValidated<TVO, TRaw>` bridge), RhinoCommon (`Vector3d`, `Plane` value structs only).
- Growth: a new scalar invariant is one `[ValueObject]` owner beside these three; a new axis family member, relation class, or pivot modality is one enum row or union case — never a sibling type; a new epsilon is one named `EpsilonPolicy` row, and a bare epsilon literal at a call site is the named defect.
- Boundary: `RhinoMath.IsValidDouble`/`SqrtEpsilon`/`ZeroTolerance`/`TwoPI` are the deleted forms on this floor — `double.IsFinite`, `EpsilonPolicy.SqrtEpsilon`, `EpsilonPolicy.ZeroTolerance`, and `Math.Tau` replace them so the numeric floor is portable by inspection while the assembly stays RhinoCommon-aware; a raw `double` parameter that means dimension, magnitude, or unit parameter is the deleted form — the generated owner crosses the signature; angle measurement never reaches `Vector3d.VectorAngle` directly from a consumer — `AnglePivot.Compute` owns the three-overload dispatch.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Vectors;

// --- [CONSTANTS] ------------------------------------------------------------------------------
public static class EpsilonPolicy {
    public const double SqrtEpsilon = 1.4901161193847656e-8;   // 2^-26, sqrt of binary64 unit roundoff
    public const double ZeroTolerance = 2.3283064365386963e-10; // 2^-32, degeneracy floor
}

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Dimension {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 1 ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"Dimension must be >= 1 (got {value})."));
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct PositiveMagnitude {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > EpsilonPolicy.ZeroTolerance ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"PositiveMagnitude requires a positive finite value (got {value:R})."));
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UnitInterval {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"UnitInterval must be in [0,1] (got {value:R})."));
}

[SmartEnum<int>]
public sealed partial class BoundarySense {
    public static readonly BoundarySense Toward = new(key: 1, sign: 1.0);
    public static readonly BoundarySense Away = new(key: -1, sign: -1.0);
    public double Sign { get; }
}

[SmartEnum<int>]
public sealed partial class SignedAxis {
    public static readonly SignedAxis NegativeX = new(key: -1, world: -Vector3d.XAxis, axis: static frame => -frame.XAxis);
    public static readonly SignedAxis PositiveX = new(key: 1, world: Vector3d.XAxis, axis: static frame => frame.XAxis);
    public static readonly SignedAxis NegativeY = new(key: -2, world: -Vector3d.YAxis, axis: static frame => -frame.YAxis);
    public static readonly SignedAxis PositiveY = new(key: 2, world: Vector3d.YAxis, axis: static frame => frame.YAxis);
    public static readonly SignedAxis NegativeZ = new(key: -3, world: -Vector3d.ZAxis, axis: static frame => -frame.ZAxis);
    public static readonly SignedAxis PositiveZ = new(key: 3, world: Vector3d.ZAxis, axis: static frame => frame.ZAxis);
    public Vector3d World { get; }
    internal Vector3d Of(Option<Plane> frame) => frame.Map(Axis).IfNone(World);
    internal static Seq<SignedAxis> Cardinal(bool planar) => toSeq(Items).Filter(axis => !planar || Math.Abs(value: axis.Key) < PositiveZ.Key);
    [UseDelegateFromConstructor] private partial Vector3d Axis(Plane frame);
}

[Union]
public abstract partial record AnglePivot {
    private AnglePivot() { }
    public sealed record WorldCase : AnglePivot;
    public sealed record FrameCase(Plane Value) : AnglePivot;
    public sealed record NormalCase(Direction Value) : AnglePivot;
    public static AnglePivot World { get; } = new WorldCase();
    public static AnglePivot Frame(Plane frame) => new FrameCase(Value: frame);
    public static AnglePivot Normal(Direction normal) => new NormalCase(Value: normal);
    internal Fin<AnglePivot> Admit(Op key) => Switch(
        state: key,
        worldCase: static (_, pivot) => Fin.Succ<AnglePivot>(pivot),
        frameCase: static (op, pivot) => Rasm.Domain.Admit.Plane(basis: pivot.Value, key: op).Map(_ => (AnglePivot)pivot),
        normalCase: static (op, pivot) => guard(pivot.Value.IsValid, op.InvalidInput()).ToFin().Map(_ => (AnglePivot)pivot));
    internal double Compute(Vector3d a, Vector3d b) => Switch(
        state: (A: a, B: b),
        worldCase: static (state, _) => Vector3d.VectorAngle(a: state.A, b: state.B),
        frameCase: static (state, frame) => Vector3d.VectorAngle(a: state.A, b: state.B, plane: frame.Value),
        normalCase: static (state, normal) => Vector3d.VectorAngle(v1: state.A, v2: state.B, vNormal: normal.Value.Value));
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct VectorAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value >= 0.0 && value <= Math.Tau ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"VectorAngle must be in [0, tau] radians (got {value:R})."));
    internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot, Op key) =>
        from activePivot in pivot.Admit(key: key)
        from angle in key.AcceptValidated<VectorAngle, double>(candidate: activePivot.Compute(a: a.Value, b: b.Value))
        select angle;
    internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, AnglePivot? pivot = null, Op? key = null) =>
        from left in Direction.Of(value: a, context: context, key: key.OrDefault())
        from right in Direction.Of(value: b, context: context, key: key.OrDefault())
        from angle in Of(a: left, b: right, pivot: pivot ?? AnglePivot.World, key: key.OrDefault())
        select angle;
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.SelfOrValue<VectorAngle, double, TOut>(self: this, value: Value, key: key);
}

[SmartEnum<int>]
public sealed partial class VectorRelation {
    public static readonly VectorRelation Oblique = new(key: 0);
    public static readonly VectorRelation Parallel = new(key: 1);
    public static readonly VectorRelation AntiParallel = new(key: -1);
    public static readonly VectorRelation Perpendicular = new(key: 2);
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
```

## [03]-[VECTOR_ALGEBRA]

- Owner: `Direction` the `readonly record struct` unit vector — the single admitted-direction currency of the kernel; `VectorSpan` the anchored vector (`Point3d` anchor + `Direction` + `PositiveMagnitude`, so a span is valid by construction); `VectorFrame` the validated orthonormal frame over `Plane`; `VectorCone` the apex/axis/half-angle solid sector. All four are construction-gated: the private constructor is unreachable except through the validating `Of`, so an instance IS its admission evidence.
- Cases: `Direction` operations `Of` (context or explicit tolerance) · unary `-` · `*` (magnitude scale) · `Reflect(normal)` (mirror transform) · `Refract(incident, normal, etaIncident, etaTransmitted, key)` (Snell with total-internal-reflection failure) · `ParallelTransport(Seq<Plane> frames)` (plane-to-plane transport fold over an admitted frame sequence); `VectorSpan` `Of` · `Value` · `Axis` · `Components(frame)` (in-plane decomposition) · `Project<TOut>`; `VectorFrame` `Of(origin, normal, xHint)` (Gram-Schmidt against the hint, `SeedPerpendicular` fallback) · `Chain(points, initialNormal, closed, context)` (rotation-minimizing Bishop chain — delegated) · `Project<TOut>`; `VectorCone` `Of` · `SolidAngle` (`Math.Tau·(1−cos θ)`) · `Contains(query)` · `Enclose(left, right)` (smallest enclosing cone: containment shortcuts, coaxial max, half-sum rotation merge) · `PartitionBy(sectors)` (rim-ray fan).
- Entry: every constructor is a `Fin`-returning `Of` threading the `Op` key — `Direction.Of` gates through `Admit.Directional`, `VectorCone.Of` through `Admit.Cone` (finite apex, directional axis, half-angle in `(0, π]`); `Direction.Refract` admits both refractive indices as `PositiveMagnitude`, orients the normal against the incident side, and fails typed on the total-internal-reflection branch (`k < −ZeroTolerance`); `VectorCone.Enclose` requires coincident apexes within the context absolute tolerance and solves the envelope as one four-way decision expression; `VectorFrame.Chain` returns `Fin<Seq<VectorFrame>>` re-admitting each chained plane.
- Auto: `Direction.IsValid` is the unit-length gate (`|‖v‖ − 1| <= EpsilonPolicy.SqrtEpsilon`) — semantic, not a mechanical fold; `VectorSpan.Value` recomposes `Direction * Magnitude` so the stored triple is the canonical decomposition; `SeedPerpendicular` is the deterministic perpendicular seed (`Vector3d.PerpendicularTo` with `XAxis` fallback) shared by frame construction and cone partition.
- Receipt: none — the models are self-evident admitted values; failures carry the `Op` typed fault.
- Packages: LanguageExt.Core (`Fin`, `Seq`, `Option`, `TraverseM`, `Apply`, `guard`), Thinktecture.Runtime.Extensions, Rasm.Domain (project — `Op`, `Context`, the `Admit` vocabulary: `Directional`/`Cone`/`Plane`/`PlaneSequence`), RhinoCommon (`Vector3d`, `Point3d`, `Plane`, `Line`, `Transform` value structs).
- Growth: a new direction algorithm (rotation toward, cone-constrained clamp, slerp-adjacent blend) is one member on `Direction` or `VectorCone` — never a sibling `DirectionUtils`; a new frame-construction modality is one `Of` overload discriminating on input shape.
- Boundary: `VectorFrame.Chain` composes the ONE rotation-minimizing-frame owner in `Spatial/neighbors` (`NeighborKernel.BishopChain`, Wang double-reflection — the point-form overload) — the chain math lives there, this page owns only the frame admission over the chained planes; quaternion pose interpolation is `Parametric/projections`' `MotionInterpolation` (the ONE slerp site) and never re-derives here; `Direction.ParallelTransport` transports through GIVEN frames — building frames from points is the neighbors owner's concern, and a second double-reflection implementation here is the deleted form.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct Direction {
    private Direction(Vector3d value) => Value = value;
    public Vector3d Value { get; }
    public bool IsValid => Value.IsValid && Math.Abs(value: Value.Length - 1.0) <= EpsilonPolicy.SqrtEpsilon;
    public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext()).Bind(model => Of(value: value, tolerance: model.Absolute.Value, key: key));
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op? key = null) =>
        Admit.Directional(value: value, tolerance: tolerance, key: key.OrDefault()).Bind(vector =>
            vector.Unitize() ? Fin.Succ(new Direction(value: vector)) : Fin.Fail<Direction>(error: key.OrDefault().InvalidInput()));
    public static Direction operator -(Direction direction) => new(value: -direction.Value);
    public static Vector3d operator *(Direction direction, double magnitude) => direction.Value * magnitude;
    public Direction Reflect(Direction normal) =>
        new(value: Transform.Mirror(pointOnMirrorPlane: Point3d.Origin, normalToMirrorPlane: normal.Value) * Value);
    public static Fin<Direction> Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key) =>
        from activeIncident in key.AcceptValidated<PositiveMagnitude, double>(candidate: etaIncident)
        from activeTransmitted in key.AcceptValidated<PositiveMagnitude, double>(candidate: etaTransmitted)
        let exiting = incident.Value * normal.Value > 0.0
        let orientedNormal = exiting switch { true => -normal.Value, false => normal.Value }
        let eta = activeIncident.Value / activeTransmitted.Value
        let cosI = Math.Clamp(value: -(incident.Value * orientedNormal), min: -1.0, max: 1.0)
        let k = 1.0 - (eta * eta * (1.0 - (cosI * cosI)))
        from direction in k switch {
            double rootable when rootable > -EpsilonPolicy.ZeroTolerance => Of(value: (eta * incident.Value) + (((eta * cosI) - Math.Sqrt(d: Math.Max(val1: 0.0, val2: rootable))) * orientedNormal), tolerance: EpsilonPolicy.ZeroTolerance, key: key),
            _ => Fin.Fail<Direction>(error: key.InvalidResult()),
        }
        select direction;
    public Fin<Direction> ParallelTransport(Seq<Plane> frames, Op? key = null) {
        Vector3d value = Value;
        Op op = key.OrDefault();
        return Admit.PlaneSequence(planes: frames, allowEmpty: false, key: op).Bind(admittedFrames =>
            toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: admittedFrames.Count - 1))).Fold(
                initialState: Of(value: value, tolerance: EpsilonPolicy.ZeroTolerance, key: op),
                f: (acc, i) => acc.Bind(prev => Of(value: Transform.PlaneToPlane(plane0: admittedFrames[index: i - 1], plane1: admittedFrames[index: i]) * prev.Value, tolerance: EpsilonPolicy.ZeroTolerance, key: op))));
    }
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.SelfOrValue<Direction, Vector3d, TOut>(self: this, value: Value, key: key);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct VectorSpan {
    private VectorSpan(Point3d anchor, Direction direction, PositiveMagnitude magnitude) { Anchor = anchor; Direction = direction; Magnitude = magnitude; }
    public Point3d Anchor { get; }
    public Direction Direction { get; }
    public PositiveMagnitude Magnitude { get; }
    public Vector3d Value => Direction * Magnitude.Value;
    public Line Axis => new(from: Anchor, to: Anchor + Value);
    public static Fin<VectorSpan> Of(Point3d anchor, Vector3d vector, Context context, Op? key = null) =>
        from direction in Direction.Of(value: vector, context: context, key: key.OrDefault())
        from span in Of(anchor: anchor, direction: direction, magnitude: vector.Length, key: key.OrDefault())
        select span;
    internal static Fin<VectorSpan> Of(Point3d anchor, Direction direction, double magnitude, Op key) =>
        from point in key.AcceptValue(value: anchor)
        from length in key.AcceptValidated<PositiveMagnitude, double>(candidate: magnitude)
        let span = new VectorSpan(anchor: point, direction: direction, magnitude: length)
        from _ in guard(span.Axis.IsValid, key.InvalidResult())
        select span;
    internal Fin<(double X, double Y)> Components(Plane frame, Op key) {
        Vector3d value = Value;
        return Admit.Plane(basis: frame, key: key).Bind(validFrame =>
            (key.AcceptValue(value: value * validFrame.XAxis), key.AcceptValue(value: value * validFrame.YAxis))
            .Apply(static (x, y) => (X: x, Y: y))
            .As());
    }
    internal Fin<TOut> Project<TOut>(Op key) {
        VectorSpan self = this;
        return AtomProjection.Rows<VectorSpan, TOut>(self: self, key: key,
            ProjectionRow.Of<Direction>(() => Fin.Succ(self.Direction)),
            ProjectionRow.Of<Vector3d>(() => key.AcceptValue(value: self.Value)),
            ProjectionRow.Of<Line>(() => key.AcceptValue(value: self.Axis)),
            ProjectionRow.Of<double>(() => Fin.Succ(self.Magnitude.Value)));
    }
}

[StructLayout(LayoutKind.Auto)]
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
        from valid in Admit.Plane(basis: frame, key: key.OrDefault())
        select new VectorFrame(value: valid);
    // The chain math is Spatial/neighbors' one rotation-minimizing-frame owner; this member admits its planes.
    public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op? key = null) =>
        NeighborKernel.BishopChain(points: points, initialNormal: initialNormal, closed: closed, context: context, key: key.OrDefault())
            .Bind(planes => planes.TraverseM(p => Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key.OrDefault())).As());
    internal static Vector3d SeedPerpendicular(Vector3d axis) {
        Vector3d seed = Vector3d.Zero;
        return seed.PerpendicularTo(other: axis) && seed.Unitize() ? seed : Vector3d.XAxis;
    }
    internal Fin<TOut> Project<TOut>(Op key) {
        VectorFrame self = this;
        return AtomProjection.Rows<VectorFrame, TOut>(self: self, key: key,
            ProjectionRow.Of<Plane>(() => Admit.Plane(basis: self.Value, key: key)),
            ProjectionRow.Of<Transform>(() => key.AcceptValue(value: Transform.PlaneToPlane(plane0: Plane.WorldXY, plane1: self.Value))));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct VectorCone {
    private VectorCone(Point3d apex, Direction axis, VectorAngle halfAngle) { Apex = apex; Axis = axis; HalfAngle = halfAngle; }
    public Point3d Apex { get; }
    public Direction Axis { get; }
    public VectorAngle HalfAngle { get; }
    public double SolidAngle => Math.Tau * (1.0 - Math.Cos(d: HalfAngle.Value));
    public static Fin<VectorCone> Of(Point3d apex, Vector3d axis, double halfAngleRadians, Context context, Op? key = null) =>
        from _ in Admit.Cone(apex: apex, axis: axis, halfAngle: halfAngleRadians, key: key.OrDefault())
        from direction in Direction.Of(value: axis, context: context, key: key.OrDefault())
        from angle in key.OrDefault().AcceptValidated<VectorAngle, double>(candidate: halfAngleRadians)
        select new VectorCone(apex: apex, axis: direction, halfAngle: angle);
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
        return from sectorCount in op.AcceptValidated<Dimension, int>(candidate: sectors)
               from rim in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: cone.Axis.Value), context: context, key: op)
               let stepAngle = Math.Tau / sectorCount.Value
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
```

## [04]-[PROJECTION_RAIL]

- Owner: `ProjectionRow` the typed dispatch row (`Type Output` + `Func<Fin<object>> Make`, with the `Of<TValue>` factory that erases once at declaration so call sites never spell an `(object)` cast) and `AtomProjection` the corpus-wide raw→typed output dispatch — promoted from a buried internal helper to the NAMED owner: every `.Project<TOut>` on every kernel surface (`VectorIntent`, `SupportProjection`, `DiscreteCalculus`, `SinkhornPlan`, cloud/mesh/extraction receipts) resolves its output type through this one rail.
- Cases: `Rows` (typed row-table scan + identity fallthrough — THE row mechanism that replaces scattered `typeof(TOut)` switch branching) · `Self` (identity-only) · `Value` (single accepted value) · `SelfOrValue` (identity or unwrapped key value) · `Values` (sequence acceptance to `Seq<TValue>`) · `Custom` (pre-admitted value with explicit gate) · `Raw` (the raw-`object` boundary lattice over `Vector3d`/`Direction`/`Plane`/`VectorFrame`/`double`/`Circle`/`Point3d`/`Matrix`/`SymmetricMatrix`/`Seq<double>`/`VectorAngle` — the one place a loose payload meets the typed world) (7).
- Entry: `AtomProjection.Rows<TSelf, TOut>(self, key, owner?, params ReadOnlySpan<ProjectionRow> rows)` — first matching row wins, `TOut == TSelf` yields the value itself, anything else fails `key.Unsupported(geometryType, outputType)`; `ProjectionRow.Of<TValue>(Func<Fin<TValue>> make)` declares a row.
- Auto: the row table is data — a surface grows a new output modality by adding ONE `ProjectionRow` beside its peers, and the dispatch body never changes; `Raw` admits through the owning model's `Of` (a raw `Vector3d` requested as `Direction` runs full direction admission with the ambient `Context`), so the rail is an admission funnel, not a cast.
- Receipt: none — the rail transports values; failures are the `Op` `Unsupported` typed fault carrying both endpoint types.
- Packages: LanguageExt.Core (`Fin`, `Option`, `Seq`), Rasm.Domain (project — `Op` fault factory), RhinoCommon (value structs at the `Raw` lattice), BCL inbox (`Type`, `ReadOnlySpan<T>`).
- Growth: a new projectable output is one `ProjectionRow` at the owning surface or one arm in the `Raw` lattice — never a new dispatch helper; a surface-local `typeof(TOut)` switch is the collapse trigger that routes here.
- Boundary: this is the ONE sanctioned type-directed dispatch site in the kernel — inline `typeof(TOut)` reflection branching inside a consumer surface (the mature extraction rail's pattern) is the named deleted form, replaced by declared `ProjectionRow` rows resolved through `Rows`; the rail stays `internal` — consumers reach it only through their surface's `.Project<TOut>`, so the public API never exposes an untyped `object` seam.

```csharp
// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct ProjectionRow(Type Output, Func<Fin<object>> Make) {
    internal static ProjectionRow Of<TValue>(Func<Fin<TValue>> make) =>
        new(Output: typeof(TValue), Make: () => make().Map(static value => (object)value!));
}

internal static class AtomProjection {
    internal static Fin<TOut> Rows<TSelf, TOut>(TSelf self, Op key, Type? owner, params ReadOnlySpan<ProjectionRow> rows) {
        foreach (ProjectionRow row in rows) {
            if (row.Output == typeof(TOut)) {
                return row.Make().Map(static projected => (TOut)projected!);
            }
        }
        return typeof(TOut) == typeof(TSelf) ? Fin.Succ((TOut)(object)self!) : Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner ?? typeof(TSelf), outputType: typeof(TOut)));
    }
    internal static Fin<TOut> Rows<TSelf, TOut>(TSelf self, Op key, params ReadOnlySpan<ProjectionRow> rows) => Rows<TSelf, TOut>(self: self, key: key, owner: null, rows: rows);
    internal static Fin<TOut> Self<TSelf, TOut>(TSelf value, Op key, Type? owner = null) =>
        typeof(TOut) == typeof(TSelf) ? Fin.Succ((TOut)(object)value!) : Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner ?? typeof(TSelf), outputType: typeof(TOut)));
    internal static Fin<TOut> Value<TValue, TOut>(TValue value, Op key, Type? owner = null) =>
        typeof(TOut) == typeof(TValue)
            ? key.AcceptValue(value: value).Map(static accepted => (TOut)(object)accepted!)
            : Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner ?? typeof(TValue), outputType: typeof(TOut)));
    internal static Fin<TOut> SelfOrValue<TSelf, TValue, TOut>(TSelf self, TValue value, Op key) =>
        typeof(TOut) == typeof(TValue) ? Value<TValue, TOut>(value: value, key: key) : Self<TSelf, TOut>(value: self, key: key);
    internal static Fin<TOut> Values<TValue, TOut>(IEnumerable<TValue> values, Op key, Type? owner = null) =>
        typeof(TOut) == typeof(Seq<TValue>)
            ? key.Accept(values: values).Map(static accepted => (TOut)(object)accepted!)
            : Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner ?? typeof(TValue), outputType: typeof(TOut)));
    internal static Fin<TOut> Custom<TValue, TOut>(TValue value, bool admitted, Op key, Type? owner = null) =>
        typeof(TOut) == typeof(TValue)
            ? admitted ? Fin.Succ((TOut)(object)value!) : Fin.Fail<TOut>(error: key.InvalidResult())
            : Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner ?? typeof(TValue), outputType: typeof(TOut)));
    internal static Fin<TOut> Raw<TOut>(object raw, Option<Context> context, Op key, Type owner, bool admitsVectorMagnitude = false) =>
        (raw, typeof(TOut)) switch {
            (Vector3d v, Type t) when t == typeof(Vector3d) => Value<Vector3d, TOut>(value: v, key: key),
            (Vector3d v, Type t) when t == typeof(Direction) => context.ToFin(Fail: key.MissingContext()).Bind(model => Direction.Of(value: v, context: model, key: key).Bind(direction => direction.Project<TOut>(key: key))),
            (Vector3d v, Type t) when t == typeof(double) && admitsVectorMagnitude => key.AcceptValue(value: v).Bind(valid => Value<double, TOut>(value: valid.Length, key: key)),
            (Plane p, Type t) when t == typeof(Plane) => Admit.Plane(basis: p, key: key).Bind(valid => Value<Plane, TOut>(value: valid, key: key)),
            (Plane p, Type t) when t == typeof(VectorFrame) => context.ToFin(Fail: key.MissingContext()).Bind(model => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: model, key: key).Bind(frame => frame.Project<TOut>(key: key))),
            (double d, Type t) when t == typeof(double) => Value<double, TOut>(value: d, key: key),
            (Circle c, Type t) when t == typeof(Circle) => Value<Circle, TOut>(value: c, key: key),
            (Point3d p, Type t) when t == typeof(Point3d) => Value<Point3d, TOut>(value: p, key: key),
            (Matrix matrix, Type t) when t == typeof(Matrix) => Custom<Matrix, TOut>(value: matrix, admitted: matrix.IsValid, key: key),
            (Seq<double> ks, Type t) when t == typeof(Seq<double>) => ks.ForAll(double.IsFinite) ? Fin.Succ((TOut)(object)ks) : Fin.Fail<TOut>(error: key.InvalidResult()),
            (SymmetricMatrix matrix, Type t) when t == typeof(SymmetricMatrix) => Custom<SymmetricMatrix, TOut>(value: matrix, admitted: matrix.IsValid, key: key),
            (VectorAngle angle, Type t) when t == typeof(VectorAngle) || t == typeof(double) => angle.Project<TOut>(key: key),
            (Direction direction, Type t) when t == typeof(Direction) || t == typeof(Vector3d) => direction.Project<TOut>(key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: owner, outputType: typeof(TOut))),
        };
}
```

## [05]-[DENSITY_BAR]

One owner per concern; a new modality is a row, case, or member on the owner below — never a sibling type.

| [INDEX] | [AXIS/CONCERN]        | [OWNER]                                          | [KIND]                                         | [CASES] |
| :-----: | :-------------------- | :----------------------------------------------- | :--------------------------------------------- | :-----: |
|  [01]   | Epsilon policy        | `EpsilonPolicy`                                  | `static` constants (`double.IsFinite` gate)    |    2    |
|  [02]   | Scalar admission      | `Dimension` · `PositiveMagnitude` · `UnitInterval` | `[ValueObject<T>]` generated owners            |    3    |
|  [03]   | Axis/angle vocabulary | `BoundarySense` · `SignedAxis` · `VectorRelation` · `AnglePivot` · `VectorAngle` | `[SmartEnum<int>]` ×3 + `[Union]` + `[ValueObject<double>]` | 2·6·4·3 |
|  [04]   | Vector algebra        | `Direction` · `VectorSpan` · `VectorFrame` · `VectorCone` | construction-gated `readonly record struct`    |    4    |
|  [05]   | Output projection     | `AtomProjection` + `ProjectionRow`               | typed row-table dispatch (`Rows`/…/`Raw`)      |    7    |
