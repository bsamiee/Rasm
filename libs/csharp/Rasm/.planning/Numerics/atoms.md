# [RASM_NUMERICS_ATOMS]

`Rasm.Numerics` owns the typed scalar, transform, vector, and output-projection algebras that every higher kernel concern composes.

## [01]-[INDEX]

- [02]-[SCALAR_FLOOR]: epsilon policy, generated scalar and angle admission, and the perceptual color algebra.
- [03]-[TRANSFORM_ALGEBRA]: affine construction union and the one `Placement` build, analysis, and rewrite surface.
- [04]-[VECTOR_ALGEBRA]: admitted-direction currency with the span, frame, and cone models over it.
- [05]-[PROJECTION_RAIL]: corpus-wide raw-to-typed output dispatch.

## [02]-[SCALAR_FLOOR]

- Owner: `EpsilonPolicy` names the two epsilon rows — sqrt-epsilon for near-unit and residual gates, zero-tolerance for degeneracy floors. `Dimension`, `PositiveMagnitude`, and `UnitInterval` generate scalar admission, so every count, positive-length, or normalized-parameter signature carries the owner, never a raw primitive re-gated per call site. `BoundarySense`, `SignedAxis`, `VectorRelation`, `AnglePivot`, and `VectorAngle` close directional sign, cardinal axis, coplanarity, measurement pivot, and radian-bounded angle. `PerceptualColor` owns the OKLab triple with normalized alpha, its mix, ramp, contrast, and gamut-safe RGB egress composing `Wacton.Unicolour` through `BlendPath`, `RgbProfile`, and `GamutPolicy` values, never a host-edge conversion.
- Entry: `Dimension`, `PositiveMagnitude`, and `UnitInterval` admit through generated `TryCreate`/`Validate`; `SignedAxis.Of` resolves the world or frame axis; `VectorRelation.Of` classifies and `VectorAngle.Of` measures two vectors through the ambient `Context` and pivot; `PerceptualColor.Of`/`OfRgb` admit, `Mix` and `Ramp` interpolate, `Contrast` reads the WCAG ratio, and `ToRgb` quantizes to the sRGB byte quadruple.
- Auto: generated `ValidateFactoryArguments` gates finiteness and the owner's bound, so interior code never re-validates an admitted scalar; `AnglePivot.Admit` re-validates only the case payload and `Compute` dispatches the three `Vector3d.VectorAngle` overloads through the generated `Switch`; `VectorRelation.Of` admits both operands as `Direction` before reading parallel and perpendicular relations under the context angle tolerance.
- Receipt: none — scalar owners are their own admission evidence.
- Packages: Thinktecture.Runtime.Extensions for the generated value-object, union, and smart-enum owners; LanguageExt.Core for the `Fin`/`Option`/`Seq` rails; Wacton.Unicolour for the perceptual model behind `PerceptualColor`; Rasm.Domain (project) for the `Op` key, `Context` tolerance, and `Admit` vocabulary; RhinoCommon for the `Vector3d` and `Plane` value structs.
- Growth: a new scalar invariant is one `[ValueObject]` owner; a new axis member, relation class, pivot modality, interpolation path, RGB profile, or gamut strategy is one enum row or union case, never a sibling type; a new epsilon is one named `EpsilonPolicy` row, and a bare epsilon literal at a call site is the deleted form; a new color capability is one member on `PerceptualColor` reading deeper into the `Unicolour` it holds.
- Boundary: `RhinoMath.IsValidDouble`/`SqrtEpsilon`/`ZeroTolerance`/`TwoPI` give way to `double.IsFinite`, `EpsilonPolicy`, and `Math.Tau`, keeping the numeric floor portable while the assembly stays RhinoCommon-aware; a raw `double` meaning dimension, magnitude, or unit parameter never crosses a signature, the generated owner does; angle measurement reaches `Vector3d.VectorAngle` only through `AnglePivot.Compute`; a componentwise sRGB lerp, a hand-rolled opponent-space matrix, or a host color-blend never stands in for perceptual math — every host edge admits into `PerceptualColor`, interpolates through `BlendPath`, and quantizes through `ToRgb`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Wacton.Unicolour;

namespace Rasm.Numerics;

// --- [CONSTANTS] ------------------------------------------------------------------------------
public static class EpsilonPolicy {
    public const double SqrtEpsilon = 1.4901161193847656e-8;
    public const double ZeroTolerance = 2.3283064365386963e-10;
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
        from angle in key.AcceptValidated<VectorAngle>(candidate: activePivot.Compute(a: a.Value, b: b.Value))
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

[SmartEnum<int>]
public sealed partial class BlendPath {
    public static readonly BlendPath Oklab = new(key: 0, space: ColourSpace.Oklab, span: HueSpan.Shorter);
    public static readonly BlendPath OklchShorter = new(key: 1, space: ColourSpace.Oklch, span: HueSpan.Shorter);
    public static readonly BlendPath OklchLonger = new(key: 2, space: ColourSpace.Oklch, span: HueSpan.Longer);
    public static readonly BlendPath OklchIncreasing = new(key: 3, space: ColourSpace.Oklch, span: HueSpan.Increasing);
    public static readonly BlendPath OklchDecreasing = new(key: 4, space: ColourSpace.Oklch, span: HueSpan.Decreasing);
    internal ColourSpace Space { get; }
    internal HueSpan Span { get; }
}

[SmartEnum<int>]
public sealed partial class RgbProfile {
    public static readonly RgbProfile Srgb = new(key: 0, configuration: Configuration.Default);
    public static readonly RgbProfile DisplayP3 = new(key: 1, configuration: new Configuration(rgbConfig: RgbConfiguration.DisplayP3));
    internal Configuration Configuration { get; }
}

[SmartEnum<int>]
public sealed partial class GamutPolicy {
    public static readonly GamutPolicy Clipped = new(key: 0, map: GamutMap.RgbClipping);
    public static readonly GamutPolicy Perceptual = new(key: 1, map: GamutMap.OklchChromaReduction);
    public static readonly GamutPolicy Spectral = new(key: 2, map: GamutMap.WxyPurityReduction);
    internal GamutMap Map { get; }
}

[ComplexValueObject]
public sealed partial class PerceptualColor {
    public double Lightness { get; }
    public double OpponentA { get; }
    public double OpponentB { get; }
    public double Alpha { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double lightness, ref double opponentA, ref double opponentB, ref double alpha) =>
        validationError = double.IsFinite(lightness) && double.IsFinite(opponentA) && double.IsFinite(opponentB) && double.IsFinite(alpha) && alpha is >= 0.0 and <= 1.0
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"PerceptualColor requires finite OKLab components and alpha in [0,1] (got L={lightness:R} a={opponentA:R} b={opponentB:R} alpha={alpha:R})."));
    public static Fin<PerceptualColor> Of(double lightness, double opponentA, double opponentB, double alpha = 1.0, Op? key = null) =>
        Validate(lightness, opponentA, opponentB, alpha, out PerceptualColor? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput());
    public static Fin<PerceptualColor> OfRgb(byte red, byte green, byte blue, double alpha = 1.0, Op? key = null) =>
        new Unicolour(ColourSpace.Rgb255, red, green, blue, alpha).Oklab switch {
            { } lab => Of(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: alpha, key: key),
        };
    public static Fin<PerceptualColor> OfRgb((byte Red, byte Green, byte Blue, byte Alpha) rgba, Op? key = null) =>
        OfRgb(red: rgba.Red, green: rgba.Green, blue: rgba.Blue, alpha: rgba.Alpha / (double)byte.MaxValue, key: key);
    public PerceptualColor Mix(PerceptualColor other, UnitInterval amount, BlendPath? path = null) =>
        (path ?? BlendPath.OklchShorter) switch {
            { } route => AsUnicolour().Mix(other.AsUnicolour(), route.Space, amount.Value, route.Span).Oklab switch {
                { } lab => Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: double.Lerp(Alpha, other.Alpha, amount.Value)),
            },
        };
    public Seq<PerceptualColor> Ramp(PerceptualColor to, Dimension stops, BlendPath? path = null) =>
        (Route: path ?? BlendPath.OklchShorter, Count: Math.Max(val1: stops.Value, val2: 2)) switch {
            { } plan => toSeq(AsUnicolour().Palette(to.AsUnicolour(), plan.Route.Space, plan.Count, plan.Route.Span))
                .Map((stop, index) => stop.Oklab switch {
                    { } lab => Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: double.Lerp(Alpha, to.Alpha, index / (double)(plan.Count - 1))),
                }),
        };
    public double Contrast(PerceptualColor other) => AsUnicolour().Contrast(other.AsUnicolour());
    public bool InRgbGamut => AsUnicolour().IsInRgbGamut;
    public (byte Red, byte Green, byte Blue, byte Alpha) ToRgb(GamutPolicy? gamut = null) =>
        AsUnicolour().MapToRgbGamut((gamut ?? GamutPolicy.Perceptual).Map).Rgb.Byte255.Clipped switch {
            { } clipped => ((byte)clipped.R, (byte)clipped.G, (byte)clipped.B, byte.CreateSaturating(Math.Round(Alpha * byte.MaxValue))),
        };
    public (double Red, double Green, double Blue, double Alpha) ToRgb(RgbProfile profile, GamutPolicy? gamut = null) =>
        AsUnicolour().ConvertToConfiguration(profile.Configuration).MapToRgbGamut((gamut ?? GamutPolicy.Perceptual).Map).Rgb switch {
            { } mapped => (mapped.R, mapped.G, mapped.B, Alpha),
        };
    private Unicolour AsUnicolour() => new(ColourSpace.Oklab, Lightness, OpponentA, OpponentB, Alpha);
}
```

## [03]-[TRANSFORM_ALGEBRA]

- Owner: `TransformSpec` is the public construction `[Union]`, each case the irreducible payload of one affine factory semantic, and `Compose` an ordered program of already-built transforms. `OrientationSense` re-closes host orientation results, `Decomposition` is the typed result `[Union]`, `DecomposeAs` and `TransformRewrite` are behavior-bearing smart-enum rows, and `Placement` is the single construction and transform-operation surface.
- Entry: `Placement.Build` constructs every spec case through one generated total `Switch`; the `Transform` extension members admit the receiver once and keep every refusal on `Fin<T>`.
- Auto: `Compose` left-composes its sequence first to last and maps the empty sequence to `Transform.Identity`; `DecomposeAs` carries each host factorization as one delegate row, `TransformRewrite` copies before every mutating host operation, and `OrientationSense` converts only admitted rigid or similarity outcomes.
- Receipt: `Decomposition` preserves every factor and orientation discriminant the selected factorization produces; construction, inverse, rewrite, bounds, list, and transpose return the admitted host value directly.
- Packages: Thinktecture.Runtime.Extensions for the union and smart-enum owners; LanguageExt.Core for the `Fin`/`Option`/`Seq` rails; Rasm.Domain (project) for `Context`, `Op`, and `Admit`; RhinoCommon for `Transform` and its factorization results.
- Growth: a factory semantic is one `TransformSpec` case and one generated-switch arm; a factorization or copy rewrite is one behavior row; a new result shape is one `Decomposition` case. Every consumer continues through `Placement`.
- Boundary: `Transform.Unset`, zero matrices, and pseudo-inverses are never control values; failed construction and factorization stay failures, `TryGetInverse` returning `false` rejects its pseudo-inverse output, and only `Identity` or an empty `Compose` supplies an identity value.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record TransformSpec {
    private TransformSpec() { }
    public sealed record Existing(Transform Value) : TransformSpec;
    public sealed record Identity : TransformSpec;
    public sealed record Translation(Vector3d Motion) : TransformSpec;
    public sealed record Diagonal(Vector3d Values) : TransformSpec;
    public sealed record UniformScale(Point3d Anchor, double Factor) : TransformSpec;
    public sealed record PlaneScale(Plane Plane, Vector3d Factors) : TransformSpec;
    public sealed record AxisRotation(double Angle, Vector3d Axis, Point3d Center) : TransformSpec;
    public sealed record SinCosRotation(double Sin, double Cos, Vector3d Axis, Point3d Center) : TransformSpec;
    public sealed record CenterRotation(double Angle, Point3d Center) : TransformSpec;
    public sealed record VectorRotation(Vector3d From, Vector3d To, Point3d Center) : TransformSpec;
    public sealed record BasisRotation(Vector3d X0, Vector3d Y0, Vector3d Z0, Vector3d X1, Vector3d Y1, Vector3d Z1) : TransformSpec;
    public sealed record YawPitchRoll(double Yaw, double Pitch, double Roll) : TransformSpec;
    public sealed record EulerZYZ(double Alpha, double Beta, double Gamma) : TransformSpec;
    public sealed record Mirror(Point3d Point, Vector3d Normal) : TransformSpec;
    public sealed record TextureMapping(Vector3d Offset, Vector3d Repeat, Vector3d Rotation) : TransformSpec;
    public sealed record PlaneMap(Plane From, Plane To) : TransformSpec;
    public sealed record PlaneBasisMap(Plane From, Plane To) : TransformSpec;
    public sealed record VectorBasisMap(Vector3d X0, Vector3d Y0, Vector3d Z0, Vector3d X1, Vector3d Y1, Vector3d Z1) : TransformSpec;
    public sealed record PointBasisMap(Point3d P0, Vector3d X0, Vector3d Y0, Vector3d Z0, Point3d P1, Vector3d X1, Vector3d Y1, Vector3d Z1) : TransformSpec;
    public sealed record PlanarProjection(Plane Plane) : TransformSpec;
    public sealed record DirectionalProjection(Plane Plane, Vector3d Direction) : TransformSpec;
    public sealed record Shear(Plane Plane, Vector3d X, Vector3d Y, Vector3d Z) : TransformSpec;
    public sealed record Compose(Seq<Transform> Values) : TransformSpec;
}

[SmartEnum<int>]
public sealed partial class OrientationSense {
    public static readonly OrientationSense Reversing = new(key: -1);
    public static readonly OrientationSense Preserving = new(key: 1);

    internal static Fin<OrientationSense> Of(TransformSimilarityType value, Op key) =>
        value switch {
            TransformSimilarityType.OrientationReversing => Fin.Succ(Reversing),
            TransformSimilarityType.OrientationPreserving => Fin.Succ(Preserving),
            TransformSimilarityType.NotSimilarity => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
            _ => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
        };

    internal static Fin<OrientationSense> Of(TransformRigidType value, Op key) =>
        value switch {
            TransformRigidType.RigidReversing => Fin.Succ(Reversing),
            TransformRigidType.Rigid => Fin.Succ(Preserving),
            TransformRigidType.NotRigid => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
            _ => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
        };
}

[Union]
public abstract partial record Decomposition {
    private Decomposition() { }
    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation, OrientationSense Orientation) : Decomposition;
    public sealed record Rigid(Vector3d Translation, Transform Rotation, OrientationSense Orientation) : Decomposition;
    public sealed record TranslationLinear(Vector3d Translation, Transform Linear) : Decomposition;
    public sealed record LinearTranslation(Transform Linear, Vector3d Translation) : Decomposition;
    public sealed record AffineFactors(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition;
    public sealed record Symmetric(Transform Basis, Vector3d Diagonal) : Decomposition;
    public sealed record Quaternion(Rhino.Geometry.Quaternion Value) : Decomposition;
    public sealed record YawPitchRoll(double Yaw, double Pitch, double Roll) : Decomposition;
    public sealed record EulerZYZ(double Alpha, double Beta, double Gamma) : Decomposition;
    public sealed record Texture(Vector3d Offset, Vector3d Repeat, Vector3d Rotation) : Decomposition;
}

[SmartEnum]
public sealed partial class DecomposeAs {
    public static readonly DecomposeAs Similarity = new(apply: SimilarityOf);
    public static readonly DecomposeAs Rigid = new(apply: RigidOf);
    public static readonly DecomposeAs TranslationLinear = new(apply: TranslationLinearOf);
    public static readonly DecomposeAs LinearTranslation = new(apply: LinearTranslationOf);
    public static readonly DecomposeAs AffineFactors = new(apply: AffineFactorsOf);
    public static readonly DecomposeAs Symmetric = new(apply: SymmetricOf);
    public static readonly DecomposeAs Quaternion = new(apply: QuaternionOf);
    public static readonly DecomposeAs YawPitchRoll = new(apply: YawPitchRollOf);
    public static readonly DecomposeAs EulerZYZ = new(apply: EulerZYZOf);
    public static readonly DecomposeAs Texture = new(apply: TextureOf);

    [UseDelegateFromConstructor]
    internal partial Fin<Decomposition> Apply(Transform source, Context context, Op key);

    private static Fin<Decomposition> SimilarityOf(Transform source, Context context, Op key) {
        TransformSimilarityType kind = source.DecomposeSimilarity(
            translation: out Vector3d translation,
            dilation: out double dilation,
            rotation: out Transform rotation,
            tolerance: context.Fractional);
        return from orientation in OrientationSense.Of(value: kind, key: key)
               from result in (key.AcceptValue(value: translation), key.AcceptValue(value: dilation), key.AcceptValue(value: rotation))
                   .Apply((move, scale, spin) => (Decomposition)new Decomposition.Similarity(
                       Translation: move,
                       Dilation: scale,
                       Rotation: spin,
                       Orientation: orientation))
                   .As()
               select result;
    }

    private static Fin<Decomposition> RigidOf(Transform source, Context context, Op key) {
        TransformRigidType kind = source.DecomposeRigid(
            translation: out Vector3d translation,
            rotation: out Transform rotation,
            tolerance: context.Fractional);
        return from orientation in OrientationSense.Of(value: kind, key: key)
               from result in (key.AcceptValue(value: translation), key.AcceptValue(value: rotation))
                   .Apply((move, spin) => (Decomposition)new Decomposition.Rigid(
                       Translation: move,
                       Rotation: spin,
                       Orientation: orientation))
                   .As()
               select result;
    }

    private static Fin<Decomposition> TranslationLinearOf(Transform source, Context context, Op key) =>
        source.DecomposeAffine(translation: out Vector3d translation, linear: out Transform linear)
            ? (key.AcceptValue(value: translation), key.AcceptValue(value: linear))
                .Apply(static (move, map) => (Decomposition)new Decomposition.TranslationLinear(Translation: move, Linear: map))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> LinearTranslationOf(Transform source, Context context, Op key) =>
        source.DecomposeAffine(linear: out Transform linear, translation: out Vector3d translation)
            ? (key.AcceptValue(value: linear), key.AcceptValue(value: translation))
                .Apply(static (map, move) => (Decomposition)new Decomposition.LinearTranslation(Linear: map, Translation: move))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> AffineFactorsOf(Transform source, Context context, Op key) =>
        source.DecomposeAffine(
            translation: out Vector3d translation,
            rotation: out Transform rotation,
            orthogonal: out Transform orthogonal,
            diagonal: out Vector3d diagonal)
            ? (key.AcceptValue(value: translation), key.AcceptValue(value: rotation), key.AcceptValue(value: orthogonal), key.AcceptValue(value: diagonal))
                .Apply(static (move, spin, basis, scale) => (Decomposition)new Decomposition.AffineFactors(
                    Translation: move,
                    Rotation: spin,
                    Orthogonal: basis,
                    Diagonal: scale))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> SymmetricOf(Transform source, Context context, Op key) =>
        source.DecomposeSymmetric(matrix: out Transform matrix, diagonal: out Vector3d diagonal)
            ? (key.AcceptValue(value: matrix), key.AcceptValue(value: diagonal))
                .Apply(static (basis, scale) => (Decomposition)new Decomposition.Symmetric(Basis: basis, Diagonal: scale))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> QuaternionOf(Transform source, Context context, Op key) =>
        source.GetQuaternion(quaternion: out Rhino.Geometry.Quaternion quaternion)
        && quaternion.IsValid
        && Math.Abs(value: quaternion.Length - 1.0) <= Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: context.Fractional)
            ? Fin.Succ<Decomposition>(value: new Decomposition.Quaternion(Value: quaternion))
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> YawPitchRollOf(Transform source, Context context, Op key) =>
        source.GetYawPitchRoll(yaw: out double yaw, pitch: out double pitch, roll: out double roll)
            ? (key.AcceptValue(value: yaw), key.AcceptValue(value: pitch), key.AcceptValue(value: roll))
                .Apply(static (z, y, x) => (Decomposition)new Decomposition.YawPitchRoll(Yaw: z, Pitch: y, Roll: x))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> EulerZYZOf(Transform source, Context context, Op key) =>
        source.GetEulerZYZ(alpha: out double alpha, beta: out double beta, gamma: out double gamma)
            ? (key.AcceptValue(value: alpha), key.AcceptValue(value: beta), key.AcceptValue(value: gamma))
                .Apply(static (a, b, c) => (Decomposition)new Decomposition.EulerZYZ(Alpha: a, Beta: b, Gamma: c))
                .As()
            : Fin.Fail<Decomposition>(error: key.InvalidResult());

    private static Fin<Decomposition> TextureOf(Transform source, Context context, Op key) {
        source.DecomposeTextureMapping(
            offset: out Vector3d offset,
            repeat: out Vector3d repeat,
            rotation: out Vector3d rotation);
        return (key.AcceptValue(value: offset), key.AcceptValue(value: repeat), key.AcceptValue(value: rotation))
            .Apply(static (move, scale, spin) => (Decomposition)new Decomposition.Texture(
                Offset: move,
                Repeat: scale,
                Rotation: spin))
            .As();
    }
}

[SmartEnum]
public sealed partial class TransformRewrite {
    public static readonly TransformRewrite Affine = new(apply: AffineOf);
    public static readonly TransformRewrite Linear = new(apply: LinearOf);
    public static readonly TransformRewrite Orthogonal = new(apply: OrthogonalOf);

    [UseDelegateFromConstructor]
    internal partial Fin<Transform> Apply(Transform source, Context context, Op key);

    private static Fin<Transform> AffineOf(Transform source, Context context, Op key) {
        Transform rewritten = source;
        rewritten.Affineize();
        return key.AcceptValue(value: rewritten);
    }

    private static Fin<Transform> LinearOf(Transform source, Context context, Op key) {
        Transform rewritten = source;
        rewritten.Linearize();
        return key.AcceptValue(value: rewritten);
    }

    private static Fin<Transform> OrthogonalOf(Transform source, Context context, Op key) {
        Transform rewritten = source;
        double tolerance = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: context.Fractional);
        return rewritten.Orthogonalize(tolerance: tolerance)
            ? key.AcceptValue(value: rewritten)
            : Fin.Fail<Transform>(error: key.InvalidResult());
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Placement {
    public static Fin<Transform> Build(TransformSpec spec, Option<Context> context = default, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(spec).ToFin(Fail: op.InvalidInput()).Bind(request => request.Switch(
            state: (Context: context, Key: op),
            existing: static (state, value) => state.Key.AcceptInput(value: value.Value),
            identity: static (state, _) => state.Key.AcceptValue(value: Transform.Identity),
            translation: static (state, value) =>
                from motion in state.Key.AcceptInput(value: value.Motion)
                from result in state.Key.AcceptValue(value: Transform.Translation(motion: motion))
                select result,
            diagonal: static (state, value) =>
                from diagonal in state.Key.AcceptInput(value: value.Values)
                from result in state.Key.AcceptValue(value: Transform.Diagonal(diagonal: diagonal))
                select result,
            uniformScale: static (state, value) =>
                from anchor in state.Key.AcceptInput(value: value.Anchor)
                from factor in state.Key.AcceptInput(value: value.Factor)
                from result in state.Key.AcceptValue(value: Transform.Scale(anchor: anchor, scaleFactor: factor))
                select result,
            planeScale: static (state, value) =>
                from plane in Admit.Plane(basis: value.Plane, key: state.Key)
                from factors in state.Key.AcceptInput(value: value.Factors)
                from result in state.Key.AcceptValue(value: Transform.Scale(
                    plane: plane,
                    xScaleFactor: factors.X,
                    yScaleFactor: factors.Y,
                    zScaleFactor: factors.Z))
                select result,
            axisRotation: static (state, value) =>
                from angle in state.Key.AcceptInput(value: value.Angle)
                from axis in Direction.Of(
                    value: value.Axis,
                    tolerance: DirectionTolerance(context: state.Context),
                    key: state.Key)
                from center in state.Key.AcceptInput(value: value.Center)
                from result in state.Key.AcceptValue(value: Transform.Rotation(
                    angleRadians: angle,
                    rotationAxis: axis.Value,
                    rotationCenter: center))
                select result,
            sinCosRotation: static (state, value) =>
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from sin in state.Key.AcceptInput(value: value.Sin)
                from cos in state.Key.AcceptInput(value: value.Cos)
                from _ in guard(
                    Math.Abs(value: ((sin * sin) + (cos * cos)) - 1.0)
                        <= Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: model.Fractional),
                    state.Key.InvalidInput())
                    .ToFin()
                from axis in Direction.Of(value: value.Axis, context: model, key: state.Key)
                from center in state.Key.AcceptInput(value: value.Center)
                from result in state.Key.AcceptValue(value: Transform.Rotation(
                    sinAngle: sin,
                    cosAngle: cos,
                    rotationAxis: axis.Value,
                    rotationCenter: center))
                select result,
            centerRotation: static (state, value) =>
                from angle in state.Key.AcceptInput(value: value.Angle)
                from center in state.Key.AcceptInput(value: value.Center)
                from result in state.Key.AcceptValue(value: Transform.Rotation(
                    angleRadians: angle,
                    rotationCenter: center))
                select result,
            vectorRotation: static (state, value) =>
                from start in Direction.Of(
                    value: value.From,
                    tolerance: DirectionTolerance(context: state.Context),
                    key: state.Key)
                from end in Direction.Of(
                    value: value.To,
                    tolerance: DirectionTolerance(context: state.Context),
                    key: state.Key)
                from center in state.Key.AcceptInput(value: value.Center)
                from result in state.Key.AcceptValue(value: Transform.Rotation(
                    startDirection: start.Value,
                    endDirection: end.Value,
                    rotationCenter: center))
                select result,
            basisRotation: static (state, value) =>
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from source in RotationBasis(
                    x: value.X0,
                    y: value.Y0,
                    z: value.Z0,
                    context: model,
                    key: state.Key)
                from target in RotationBasis(
                    x: value.X1,
                    y: value.Y1,
                    z: value.Z1,
                    context: model,
                    key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.Rotation(
                    x0: source.X,
                    y0: source.Y,
                    z0: source.Z,
                    x1: target.X,
                    y1: target.Y,
                    z1: target.Z))
                select result,
            yawPitchRoll: static (state, value) =>
                from yaw in state.Key.AcceptInput(value: value.Yaw)
                from pitch in state.Key.AcceptInput(value: value.Pitch)
                from roll in state.Key.AcceptInput(value: value.Roll)
                from result in state.Key.AcceptValue(value: Transform.RotationZYX(
                    yaw: yaw,
                    pitch: pitch,
                    roll: roll))
                select result,
            eulerZYZ: static (state, value) =>
                from alpha in state.Key.AcceptInput(value: value.Alpha)
                from beta in state.Key.AcceptInput(value: value.Beta)
                from gamma in state.Key.AcceptInput(value: value.Gamma)
                from result in state.Key.AcceptValue(value: Transform.RotationZYZ(
                    alpha: alpha,
                    beta: beta,
                    gamma: gamma))
                select result,
            mirror: static (state, value) =>
                from point in state.Key.AcceptInput(value: value.Point)
                from normal in Direction.Of(
                    value: value.Normal,
                    tolerance: DirectionTolerance(context: state.Context),
                    key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.Mirror(
                    pointOnMirrorPlane: point,
                    normalToMirrorPlane: normal.Value))
                select result,
            textureMapping: static (state, value) =>
                from offset in state.Key.AcceptInput(value: value.Offset)
                from repeat in state.Key.AcceptInput(value: value.Repeat)
                from rotation in state.Key.AcceptInput(value: value.Rotation)
                from result in state.Key.AcceptValue(value: Transform.TextureMapping(
                    offset: offset,
                    repeat: repeat,
                    rotation: rotation))
                select result,
            planeMap: static (state, value) =>
                from source in Admit.Plane(basis: value.From, key: state.Key)
                from target in Admit.Plane(basis: value.To, key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.PlaneToPlane(
                    plane0: source,
                    plane1: target))
                select result,
            planeBasisMap: static (state, value) =>
                from source in Admit.Plane(basis: value.From, key: state.Key)
                from target in Admit.Plane(basis: value.To, key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.ChangeBasis(
                    plane0: source,
                    plane1: target))
                select result,
            vectorBasisMap: static (state, value) => VectorBasis(
                x0: value.X0,
                y0: value.Y0,
                z0: value.Z0,
                x1: value.X1,
                y1: value.Y1,
                z1: value.Z1,
                key: state.Key),
            pointBasisMap: static (state, value) => PointBasis(
                p0: value.P0,
                x0: value.X0,
                y0: value.Y0,
                z0: value.Z0,
                p1: value.P1,
                x1: value.X1,
                y1: value.Y1,
                z1: value.Z1,
                key: state.Key),
            planarProjection: static (state, value) =>
                from plane in Admit.Plane(basis: value.Plane, key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.PlanarProjection(plane: plane))
                select result,
            directionalProjection: static (state, value) =>
                from plane in Admit.Plane(basis: value.Plane, key: state.Key)
                from direction in Direction.Of(
                    value: value.Direction,
                    tolerance: DirectionTolerance(context: state.Context),
                    key: state.Key)
                from result in state.Key.AcceptValue(value: Transform.ProjectAlong(
                    plane: plane,
                    direction: direction.Value))
                select result,
            shear: static (state, value) =>
                from plane in Admit.Plane(basis: value.Plane, key: state.Key)
                from x in state.Key.AcceptInput(value: value.X)
                from y in state.Key.AcceptInput(value: value.Y)
                from z in state.Key.AcceptInput(value: value.Z)
                from result in state.Key.AcceptValue(value: Transform.Shear(
                    plane: plane,
                    x: x,
                    y: y,
                    z: z))
                select result,
            compose: static (state, value) => Compose(
                values: value.Values,
                key: state.Key)));
    }

    extension(Transform source) {
        public Fin<Transform> Inverse(Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from inverse in active.TryGetInverse(inverseTransform: out Transform result)
                       ? op.AcceptValue(value: result)
                       : Fin.Fail<Transform>(error: op.InvalidResult())
                   select inverse;
        }

        public Fin<Decomposition> Decompose(DecomposeAs mode, Context context, Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from selector in Optional(mode).ToFin(Fail: op.InvalidInput())
                   from model in Optional(context).ToFin(Fail: op.MissingContext())
                   from result in selector.Apply(source: active, context: model, key: op)
                   select result;
        }

        public Fin<Transform> Rewrite(TransformRewrite rewrite, Context context, Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from selector in Optional(rewrite).ToFin(Fail: op.InvalidInput())
                   from model in Optional(context).ToFin(Fail: op.MissingContext())
                   from result in selector.Apply(source: active, context: model, key: op)
                   select result;
        }

        public Fin<BoundingBox> TransformBoundingBox(BoundingBox bounds, Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from admitted in op.AcceptInput(value: bounds)
                   from result in op.AcceptValue(value: active.TransformBoundingBox(bbox: admitted))
                   select result;
        }

        public Fin<Seq<Point3d>> TransformList(IEnumerable<Point3d> points, Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from values in Optional(points).ToFin(Fail: op.InvalidInput())
                   from admitted in values.AsIterable().ToSeq()
                       .TraverseM(value => op.AcceptInput(value: value))
                       .As()
                   from result in op.Catch(body: () => op.Accept(values: active.TransformList(points: admitted)))
                   select result;
        }

        public Fin<Transform> Transpose(Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.AcceptInput(value: source)
                   from result in op.AcceptValue(value: active.Transpose())
                   select result;
        }
    }

    private static double DirectionTolerance(Option<Context> context) =>
        context.Map(static model => model.Absolute.Value).IfNone(EpsilonPolicy.ZeroTolerance);

    private static Fin<(Vector3d X, Vector3d Y, Vector3d Z)> RotationBasis(
        Vector3d x,
        Vector3d y,
        Vector3d z,
        Context context,
        Op key) =>
        from frame in Admit.Plane(
            basis: new Plane(
                origin: Point3d.Origin,
                xDirection: x,
                yDirection: y),
            key: key)
        from supplied in Direction.Of(value: z, context: context, key: key)
        from relation in VectorRelation.Of(
            a: frame.ZAxis,
            b: supplied.Value,
            context: context,
            key: key)
        from _ in guard(relation == VectorRelation.Parallel, key.InvalidInput()).ToFin()
        select (X: frame.XAxis, Y: frame.YAxis, Z: frame.ZAxis);

    private static Fin<Transform> VectorBasis(
        Vector3d x0,
        Vector3d y0,
        Vector3d z0,
        Vector3d x1,
        Vector3d y1,
        Vector3d z1,
        Op key) =>
        (key.AcceptInput(value: x0),
         key.AcceptInput(value: y0),
         key.AcceptInput(value: z0),
         key.AcceptInput(value: x1),
         key.AcceptInput(value: y1),
         key.AcceptInput(value: z1))
            .Apply(static (ax, ay, az, bx, by, bz) => Transform.ChangeBasis(
                X0: ax,
                Y0: ay,
                Z0: az,
                X1: bx,
                Y1: by,
                Z1: bz))
            .As()
            .Bind(result => key.AcceptValue(value: result));

    private static Fin<Transform> PointBasis(
        Point3d p0,
        Vector3d x0,
        Vector3d y0,
        Vector3d z0,
        Point3d p1,
        Vector3d x1,
        Vector3d y1,
        Vector3d z1,
        Op key) =>
        (key.AcceptInput(value: p0),
         key.AcceptInput(value: x0),
         key.AcceptInput(value: y0),
         key.AcceptInput(value: z0),
         key.AcceptInput(value: p1),
         key.AcceptInput(value: x1),
         key.AcceptInput(value: y1),
         key.AcceptInput(value: z1))
            .Apply(static (a0, ax, ay, az, b0, bx, by, bz) => Transform.ChangeBasis(
                P0: a0,
                X0: ax,
                Y0: ay,
                Z0: az,
                P1: b0,
                X1: bx,
                Y1: by,
                Z1: bz))
            .As()
            .Bind(result => key.AcceptValue(value: result));

    private static Fin<Transform> Compose(Seq<Transform> values, Op key) =>
        values
            .TraverseM(value => key.AcceptInput(value: value))
            .As()
            .Map(static admitted => admitted.Fold(
                initialState: Transform.Identity,
                f: static (combined, next) => next * combined))
            .Bind(result => key.AcceptValue(value: result));
}
```

## [04]-[VECTOR_ALGEBRA]

- Owner: `Direction` is the single admitted unit-vector currency of the kernel; `VectorSpan` the anchored vector, `VectorFrame` the validated orthonormal frame over `Plane`, `VectorCone` the apex/axis/half-angle solid sector. All four are construction-gated — the private constructor is unreachable except through the validating `Of`, so an instance is its own admission evidence.
- Cases: `Direction` owns admission, reflection, refraction, and transport; `VectorSpan` anchored magnitude decomposition; `VectorFrame` orthonormal admission and chained construction; `VectorCone` containment, envelope, and rim partition.
- Entry: every constructor and host-backed transform returns `Fin<T>` under one `Op`; `Direction.Reflect` and `ParallelTransport`, the `VectorFrame` transform projection, and the `VectorCone` rotation folds construct only through `Placement.Build`.
- Auto: `Direction.IsValid` is the unit-length gate, semantic rather than a mechanical fold; `VectorSpan.Value` recomposes `Direction * Magnitude` so the stored triple is the canonical decomposition; `SeedPerpendicular` is the deterministic perpendicular seed shared by frame construction and cone partition.
- Receipt: none — the models are self-evident admitted values, and failures carry the `Op` typed fault.
- Packages: LanguageExt.Core for the `Fin`/`Seq`/`Option` rails; Thinktecture.Runtime.Extensions for the generated owners; Rasm.Domain (project) for `Op`, `Context`, and the `Admit` vocabulary; RhinoCommon for the `Vector3d`, `Point3d`, `Plane`, and `Line` value structs.
- Growth: a new direction algorithm is one member on `Direction` or `VectorCone`, never a sibling `DirectionUtils`; a new frame-construction modality is one `Of` overload discriminating on input shape.
- Boundary: `VectorFrame.Chain` composes the one rotation-minimizing-frame owner in `Spatial/neighbors`, which owns the chain math while this page owns only frame admission over the chained planes; quaternion pose interpolation is `Parametric/projections`' `MotionInterpolation` and never re-derives here; `Direction.ParallelTransport` transports through given frames, so a second double-reflection implementation here is the deleted form.

```csharp signature
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
    public Fin<Direction> Reflect(Direction normal, Op? key = null) {
        Op op = key.OrDefault();
        Direction self = this;
        return Placement.Build(
                spec: new TransformSpec.Mirror(
                    Point: Point3d.Origin,
                    Normal: normal.Value),
                key: op)
            .Bind(transform => Of(
                value: transform * self.Value,
                tolerance: EpsilonPolicy.ZeroTolerance,
                key: op));
    }
    public static Fin<Direction> Refract(Direction incident, Direction normal, double etaIncident, double etaTransmitted, Op key) =>
        from activeIncident in key.AcceptValidated<PositiveMagnitude>(candidate: etaIncident)
        from activeTransmitted in key.AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted)
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
                f: (acc, i) => acc.Bind(prev =>
                    Placement.Build(
                            spec: new TransformSpec.PlaneMap(
                                From: admittedFrames[index: i - 1],
                                To: admittedFrames[index: i]),
                            key: op)
                        .Bind(transform => Of(
                            value: transform * prev.Value,
                            tolerance: EpsilonPolicy.ZeroTolerance,
                            key: op)))));
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
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: magnitude)
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
            ProjectionRow.Of<Transform>(() => Placement.Build(
                spec: new TransformSpec.PlaneMap(
                    From: Plane.WorldXY,
                    To: self.Value),
                key: key)));
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
        from angle in key.OrDefault().AcceptValidated<VectorAngle>(candidate: halfAngleRadians)
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
                       .Bind(_ => Placement.Build(
                           spec: new TransformSpec.AxisRotation(
                               Angle: envelope.Half - envelope.A,
                               Axis: rotationAxis,
                               Center: Point3d.Origin),
                           context: Some(model),
                           key: op))
                       .Bind(transform => Direction.Of(
                           value: transform * left.Axis.Value,
                           context: model,
                           key: op))
                       .Bind(axis => Of(
                           apex: left.Apex,
                           axis: axis.Value,
                           halfAngleRadians: Math.Min(val1: Math.PI, val2: envelope.Half),
                           context: model,
                           key: op)),
               }
               select result;
    }
    public Fin<Seq<Direction>> PartitionBy(int sectors, Context context, Op? key = null) {
        Op op = key.OrDefault();
        VectorCone cone = this;
        return from sectorCount in op.AcceptValidated<Dimension>(candidate: sectors)
               from rim in Direction.Of(value: VectorFrame.SeedPerpendicular(axis: cone.Axis.Value), context: context, key: op)
               let stepAngle = Math.Tau / sectorCount.Value
               let lateral = Math.Sin(a: cone.HalfAngle.Value)
               let coaxial = Math.Cos(d: cone.HalfAngle.Value) * cone.Axis.Value
               from rays in toSeq(Enumerable.Range(start: 0, count: sectorCount.Value)).TraverseM(i =>
                   Placement.Build(
                           spec: new TransformSpec.AxisRotation(
                               Angle: stepAngle * i,
                               Axis: cone.Axis.Value,
                               Center: Point3d.Origin),
                           context: Some(context),
                           key: op)
                       .Bind(transform => Direction.Of(
                           value: coaxial + (lateral * (transform * rim.Value)),
                           context: context,
                           key: op))).As()
               select rays;
    }
}
```

## [05]-[PROJECTION_RAIL]

- Owner: `ProjectionRow` is the typed dispatch row — a `Type`/`Make` pair whose `Of<TValue>` factory erases once at declaration so call sites never spell an `(object)` cast — and `AtomProjection` is the corpus-wide raw-to-typed output dispatch every kernel surface resolves its `.Project<TOut>` output type through.
- Cases: `Rows` scans a typed row-table with identity fallthrough; `Self`, `Value`, `SelfOrValue`, `Values`, and `Custom` cover the fixed acceptance shapes; `Raw` is the one raw-`object` boundary lattice where a loose payload meets the typed world.
- Entry: `AtomProjection.Rows` scans the row table, first match winning and `TOut == TSelf` yielding the value itself, anything else failing `key.Unsupported`; `ProjectionRow.Of` declares one row.
- Auto: the row table is data — a surface grows an output modality by adding one `ProjectionRow` beside its peers while the dispatch body never changes; `Raw` admits through the owning model's `Of`, so the rail is an admission funnel, not a cast.
- Receipt: none — the rail transports values, and failures are the `Op` `Unsupported` typed fault carrying both endpoint types.
- Packages: LanguageExt.Core for the `Fin`/`Option`/`Seq` rails; Rasm.Domain (project) for the `Op` fault factory; RhinoCommon for the value structs at the `Raw` lattice; the BCL for `Type` and `ReadOnlySpan<T>`.
- Growth: a new projectable output is one `ProjectionRow` at the owning surface or one arm in the `Raw` lattice, never a new dispatch helper; a surface-local `typeof(TOut)` switch is the collapse trigger that routes here.
- Boundary: `AtomProjection` is the one sanctioned type-directed dispatch site in the kernel; inline `typeof(TOut)` reflection branching inside a consumer surface is the deleted form, replaced by declared `ProjectionRow` rows resolved through `Rows`. `AtomProjection` stays `internal`, so consumers reach it only through their surface's `.Project<TOut>` and the public API never exposes an untyped `object` seam.

```csharp signature
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

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
