# [RASM_NUMERICS_ATOMS]

`Rasm.Numerics` owns the typed scalar, transform, vector, and output-projection algebras that every higher kernel concern composes.

`Band` is the branch's ONE range guard: every generated `ValidateFactoryArguments` body on this page and every `ToleranceLane` row at `Domain/context#TOLERANCE_LANES` reads a row here rather than spelling its own bounds, so a numeric window is declared once and admitted everywhere. It is the host-NEUTRAL rail — `double.IsFinite` screening — and stands beside, never over, the `Domain/rails` `ValidityClaim` evidence rail whose `RhinoMath.IsValidDouble` screen exists to reject the host `UnsetValue` sentinel a finiteness probe admits as an ordinary value; stratum direction makes the split structural, since Domain cannot compose a Numerics row. `PerceptualColor` carries the whole colour crossing at both hosts: the numeric floor admits and emits `System.Drawing.Color` and packed ARGB here, and the Eto pair rides an `extension(PerceptualColor)` block on `Interaction/paint#COLOR` so this page links no UI toolkit.

## [01]-[INDEX]

- [02]-[SCALAR_FLOOR]: epsilon policy, the range-guard band vocabulary, generated scalar and angle admission, and the perceptual color algebra.
- [03]-[TRANSFORM_ALGEBRA]: affine construction union and the one `Placement` build, analysis, and rewrite surface.
- [04]-[VECTOR_ALGEBRA]: admitted-direction currency with the span, frame, and cone models over it.
- [05]-[CELL_LATTICE]: ONE bounded rectangular cell lattice — index-to-world affine, per-axis census, budget ceiling.
- [06]-[PROJECTION_RAIL]: corpus-wide raw-to-typed output dispatch.

## [02]-[SCALAR_FLOOR]

- Owner: `EpsilonPolicy` names the three anchor rows — sqrt-epsilon for near-unit and residual gates, zero-tolerance for degeneracy floors, seam-ulp for the convergence floor no double iterate reaches below. `Bound` closes one interval endpoint as a case — open, closed, or unbounded — and `Band` pairs two of them into the one named range vocabulary every scalar admission reads: `Dimension`, `PositiveMagnitude`, `UnitInterval`, `SignedUnit`, `VectorAngle`, and `PerceptualColor` all guard through a row, so a count, positive length, normalized parameter, bipolar reading, radian angle, or opponent component carries the owner and never a raw primitive re-gated per call site. `BoundarySense`, `SignedAxis`, `VectorRelation`, `AnglePivot`, and `VectorAngle` close directional sign, cardinal axis, coplanarity, measurement pivot, and radian-bounded angle. `PerceptualColor` owns the OKLab triple with normalized alpha, its mix, ramp, tonal, contrast, contrast-targeted tonal solve, simulation, difference, compositing, colorimetric read-back, appearance reading, and gamut-safe RGB, ARGB, and host egress composing `Wacton.Unicolour` through `BlendPath`, `RgbProfile`, `DeltaMetric`, `GamutPolicy`, `ToneSweep`, and `RgbTransfer` values, never a host-edge conversion. `RgbProfile` is the branch working-space roster and the corpus' ONE `Configuration` mint — the instance is the colour-space identity, so every package above composes a row, and `Condition` and `Viewed` extend that same mint with the viewing-condition slot rather than opening a second one; `BlendPath` splits interpolation space from the axes only some spaces admit, one row per space with the hue traversal on the polar case's payload and the viewing condition on the appearance case's; `DeltaMetric` splits the difference axis the same way, an opponent row condition-free beside an appearance row carrying its condition; `AppearanceReading` carries the correlates with the condition they were measured under; `GamutPolicy` rows own a reproducibility domain with both its containment predicate and its nearest-in-domain projection, `Unbounded` the whole-space row an HDR or scene-linear egress names; `RgbTransfer` rows own the representation the profile egress reads a bounded colour off, the companded encoding beside scene-linear light, because a return shape cannot discriminate what the ingress triple's shape does.
- Cases: `Bound.Open`/`Closed` carry the endpoint value, `Bound.Unbounded` carries none — an unbounded side is a CASE, never an infinity sentinel, and inclusivity is that case rather than a flag beside a number, so an open-versus-closed mismatch is unrepresentable.
- Law: `Band.Guard` canonicalizes negative zero to positive zero before the bound test, because `-0.0` passes every comparison its band admits and then forks the content key and every reciprocal sign downstream — this is the one canonicalization the `ref` slot exists for, and a row needing another lands it here rather than at a factory. `ValidationError?` returns from a guard as the Thinktecture generated-seam signal, admitted-as-null by the generator's own contract and never a domain absence; that seam carries ONE error, so a multi-column body is a first-refusal chain and accumulation rides `Validation` at the `Op` admission rail instead. Exact bounds — zero, one, the unit endpoints, the count floor, the device half-unit — are normalization identities and state so on site; every INEXACT bound derives from an `EpsilonPolicy` row or `Math.Tau`, and a bare epsilon at a call site is the deleted form.
- Exemption: `Band` and `ValidityClaim` are two rails, not two spellings — `Band` guards a generated factory on host-neutral material through `double.IsFinite`, `ValidityClaim` folds receipt evidence over host-read material through `RhinoMath.IsValidDouble` so the `UnsetValue` sentinel is refused; Numerics composes Domain and never the reverse, so the two cannot merge.
- Entry: `Dimension`, `PositiveMagnitude`, `UnitInterval`, and `SignedUnit` admit through generated `TryCreate`/`Validate`; `SignedAxis.Of` resolves the world or frame axis and `Cardinal(rank)` filters the axes a rank admits; `VectorRelation.Of` classifies and `VectorAngle.Of` measures two vectors through the ambient `Context` and pivot; `PerceptualColor.Of`/`OfRgb`/`OfArgb`/`OfHost`/`OfTemperature`/`Achromatic` admit — display bytes with a unit-gated or byte alpha, a packed ARGB word, a host `System.Drawing.Color`, an encoded unit-interval triple, an unbounded scene-linear double triple, a correlated colour temperature, or a lightness alone on the neutral axis — `Mix` and `Ramp` interpolate along a `BlendPath` and read the interpolated alpha off the result, `Blend` composites onto a backdrop under any `BlendMode`, `Simulate` previews a colour-vision deficiency at a unit-bounded severity, `Difference` measures perceptual distance under a `DeltaMetric` row, `ReferenceLightness` reads the reference-corrected lightness a ramp asserts monotonicity on, `Contrast` reads the WCAG ratio and `ToneFor` inverts it through a `ToneSweep` walk, `Colorimetry` reads relative luminance, correlated colour temperature, dominant wavelength, and excitation purity as one column, `Appearance` reads the CAM correlates a `BlendPath.Appearance` row states, `InGamut` tests the selected reproducibility domain, and `ToRgb`/`ToArgb`/`ToDrawing`/`ToColor4f` bound through that domain before quantizing (the RhinoCommon float quad enters through `OfHost(Color4f, transfer)` and leaves through `ToColor4f`, the `RgbTransfer` row discriminating companded from scene-linear on both legs); `RgbProfile.Viewed` mints the cam-bearing `Configuration` a direct-`Unicolour` composer states its condition through and `DeltaMetric.Measure` measures a `Unicolour` operand pair under the row's own condition; `RgbProfile.Condition` admits an authored viewing condition from a rostered illuminant, a stated observer, an ambient illuminance, a background luminance, and a `Surround`.
- Auto: every generated `ValidateFactoryArguments` body is one `Band.Guard` call, so interior code never re-validates an admitted scalar and a bound moves at one row; `Band.Admits(ReadOnlySpan<double>)` reduces through `TensorPrimitives` so a whole plane admits in one vectorized pass; `AnglePivot.Admit` re-validates only the case payload and `Compute` dispatches the three `Vector3d.VectorAngle` overloads through the generated `Switch`; `VectorRelation.Of` admits both operands as `Direction` before reading parallel and perpendicular relations under the context angle tolerance; `RgbProfile.Viewed` memoizes each cam-bearing `Configuration` on the condition's reference identity and resolves the package-default condition to the row's own instance, so no caller sequences a mint.
- Receipt: `AppearanceReading` alone — CAM correlates are meaningless apart from the condition that produced them, so the reading carries it; every other owner here is its own admission evidence.
- Packages: Thinktecture.Runtime.Extensions for the generated value-object, union, and smart-enum owners; LanguageExt.Core for the `Fin`/`Option`/`Seq` rails and the `Atom<HashMap<_,_>>` cell behind the cam-bearing mint cache; Wacton.Unicolour for the perceptual model behind `PerceptualColor`; System.Numerics.Tensors for the span-arm band reduction; System.Drawing.Common for the host colour carrier at the ARGB egress pair; Rasm.Domain (project) for the `Op` key, `Context` tolerance, `ValidityClaim` evidence, and `Admit` vocabulary; RhinoCommon for the `Vector3d` and `Plane` value structs.
- Growth: a new range window is one `Band` row and a new endpoint modality one `Bound` case, never a bespoke bounds expression inside a factory; a new scalar invariant is one `[ValueObject]` owner reading a band; a new axis member, relation class, pivot modality, working space, or reproducibility domain is one enum row or union case, never a sibling type; a new interpolation space is one `BlendPath` row whose case states which of the traversal and condition axes it admits; a new difference metric is one `DeltaMetric` row on the case matching its condition dependence; a new egress representation is one `RgbTransfer` row, never a sibling `ToRgb`; an HDR egress publishing above-white light names the `GamutPolicy.Unbounded` row rather than skipping the bound; a declared viewing condition is a `Condition` construction at its own site, never a roster row, because a surround measures the viewer's room rather than naming a colour vocabulary member; a new epsilon is one named `EpsilonPolicy` row; a new tonal-search direction is one `ToneSweep` row and never a comparator argument, because a caller-supplied ordering re-opens the monotonicity the walk depends on; a new color capability is one member on `PerceptualColor` reading deeper into the `Unicolour` it holds.
- Boundary: `RhinoMath.SqrtEpsilon`/`ZeroTolerance`/`TwoPI` give way to `EpsilonPolicy` and `Math.Tau` everywhere, and `RhinoMath.IsValidDouble` gives way to `double.IsFinite` on HOST-NEUTRAL shapes — host-read material instead admits through the `Domain/rails` `ValidityClaim.Finite` row — keeping the numeric floor portable while the assembly stays RhinoCommon-aware; a raw `double` meaning dimension, magnitude, unit parameter, or bipolar-normalized reading never crosses a signature, the generated owner does, and a package above that re-declares a `[-1,1]` value object is the split-owner form this row closes; angle measurement reaches `Vector3d.VectorAngle` only through `AnglePivot.Compute`; a componentwise sRGB lerp, a hand-rolled opponent-space matrix, a host color-blend, or a call-site tone search against a contrast target never stands in for perceptual math — every host edge admits into `PerceptualColor`, interpolates through `BlendPath`, solves a readable rung through `ToneFor`, and quantizes through `ToRgb`, whose byte leg is the ONE content-key quantizer the federation addresses against and therefore carries no transfer slot at all and CLIPS by ruling, while the ARGB and `System.Drawing` legs REFUSE an out-of-display colour because a paint instruction that clipped silently is a colour no consumer can attribute; the `Eto.Drawing` pair of that same correspondence is an `extension(PerceptualColor)` block on `Interaction/paint#COLOR`, so this page names no UI toolkit and the numeric floor stays Eto-free; AppUi's colour-space vocabulary is a coordinate in the space, transfer, and domain axes already declared here — its scene-linear float row is `RgbProfile.Srgb` read through `RgbTransfer.Linear` under `GamutPolicy.Unbounded`, never a fourth axis or a parallel roster; a hue traversal never travels beside an interpolation space as a parallel argument, because the polar case is the only shape that carries one, and a viewing condition never travels beside one either, because the appearance case is; a working space enters as an `RgbProfile` row and never as a peer-minted `Configuration`, a chromaticity table, or a whitepoint literal — the cam-bearing crossing is the SAME row's `Viewed` mint, published so every chartered direct-`Unicolour` composer reaches it; an appearance space or CAM difference metric with no stated condition is unspellable and no default surround is ever fabricated for one, which is why `Viewed` and `DeltaMetric.Measure` publish rather than the law carving an exemption, while the WCAG `Contrast` read stays condition-free because WCAG fixes its own.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Numerics.Tensors;
using Rasm.Domain;
using Thinktecture;
using Wacton.Unicolour;

namespace Rasm.Numerics;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class EpsilonPolicy {
    public const double SqrtEpsilon = 1.4901161193847656e-8;
    public const double CbrtEpsilon = 6.0554544523933395e-6;
    public const double ZeroTolerance = 2.3283064365386963e-10;
    public const double SeamUlp = 1e-12;
    public const double SubTolerance = 0.1;
}

public static class Reduce {
    public static double Floored(double value, double period) => value - (period * Math.Floor(d: value / period));
    public static double Centred(double value, double period) => Floored(value: value + (period * 0.5), period: period) - (period * 0.5);
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Bound {
    private Bound() { }
    public sealed record OpenCase(double Value) : Bound;
    public sealed record ClosedCase(double Value) : Bound;
    public sealed record UnboundedCase : Bound;
    public static Bound Open(double value) => new OpenCase(Value: value);
    public static Bound Closed(double value) => new ClosedCase(Value: value);
    public static Bound Unbounded { get; } = new UnboundedCase();
    internal bool AtLeast(double probe) => Switch(
        state: probe,
        openCase: static (value, bound) => value > bound.Value,
        closedCase: static (value, bound) => value >= bound.Value,
        unboundedCase: static (_, _) => true);
    internal bool AtMost(double probe) => Switch(
        state: probe,
        openCase: static (value, bound) => value < bound.Value,
        closedCase: static (value, bound) => value <= bound.Value,
        unboundedCase: static (_, _) => true);
    internal string Lower => Switch(
        openCase: static bound => string.Create(CultureInfo.InvariantCulture, $"({bound.Value:R}"),
        closedCase: static bound => string.Create(CultureInfo.InvariantCulture, $"[{bound.Value:R}"),
        unboundedCase: static _ => "(-inf");
    internal string Upper => Switch(
        openCase: static bound => string.Create(CultureInfo.InvariantCulture, $"{bound.Value:R})"),
        closedCase: static bound => string.Create(CultureInfo.InvariantCulture, $"{bound.Value:R}]"),
        unboundedCase: static _ => "+inf)");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Band {
    public static readonly Band Positive = new(key: "positive", floor: Bound.Open(EpsilonPolicy.ZeroTolerance), ceiling: Bound.Unbounded);
    public static readonly Band Nonnegative = new(key: "nonnegative", floor: Bound.Closed(0.0), ceiling: Bound.Unbounded);
    public static readonly Band Unit = new(key: "unit", floor: Bound.Closed(0.0), ceiling: Bound.Closed(1.0));
    public static readonly Band SignedUnit = new(key: "signed-unit", floor: Bound.Closed(-1.0), ceiling: Bound.Closed(1.0));
    public static readonly Band Angle = new(key: "angle", floor: Bound.Closed(0.0), ceiling: Bound.Closed(Math.Tau));
    public static readonly Band HalfTurn = new(key: "half-turn", floor: Bound.Open(0.0), ceiling: Bound.Closed(Math.PI));
    public static readonly Band Ratio = new(key: "ratio", floor: Bound.Open(EpsilonPolicy.ZeroTolerance), ceiling: Bound.Closed(1.0));
    public static readonly Band Fractional = new(key: "fractional", floor: Bound.Closed(0.0), ceiling: Bound.Open(1.0));
    public static readonly Band Growth = new(key: "growth", floor: Bound.Open(1.0), ceiling: Bound.Unbounded);
    public static readonly Band Residual = new(key: "residual", floor: Bound.Open(EpsilonPolicy.SeamUlp), ceiling: Bound.Closed(1.0));
    public static readonly Band Length = new(key: "length", floor: Bound.Open(EpsilonPolicy.SqrtEpsilon), ceiling: Bound.Unbounded);
    public static readonly Band Parameter = new(key: "parameter", floor: Bound.Unbounded, ceiling: Bound.Unbounded);
    public static readonly Band Device = new(key: "device", floor: Bound.Closed(DeviceQuantum), ceiling: Bound.Unbounded);
    public static readonly Band Count = new(key: "count", floor: Bound.Closed(1.0), ceiling: Bound.Unbounded);
    public static readonly Band Octave = new(key: "octave", floor: Bound.Closed(1.0), ceiling: Bound.Closed(32.0));
    public static readonly Band Percentile = new(key: "percentile", floor: Bound.Closed(0.0), ceiling: Bound.Closed(100.0));


    public Bound Floor { get; }
    public Bound Ceiling { get; }
    public string Interval => string.Create(CultureInfo.InvariantCulture, $"{Floor.Lower}, {Ceiling.Upper}");
    public bool Admits(double value) => double.IsFinite(value) && Floor.AtLeast(probe: value) && Ceiling.AtMost(probe: value);
    public bool Admits(ReadOnlySpan<double> values) =>
        !values.IsEmpty && TensorPrimitives.IsFiniteAll(values)
        && Floor.AtLeast(probe: TensorPrimitives.Min(values)) && Ceiling.AtMost(probe: TensorPrimitives.Max(values));
    public ValidationError Refuse(string label, double value) =>
        new(message: string.Create(CultureInfo.InvariantCulture, $"{label} must lie in {Interval} (got {value:R})."));
    public ValidationError? Guard(string label, ref double value) {
        value = value == 0.0 ? 0.0 : value;
        return Admits(value: value) ? null : Refuse(label: label, value: value);
    }
    public ValidationError? Guard(string label, ref int value) =>
        Admits(value: value) ? null : Refuse(label: label, value: value);

    private const double DeviceQuantum = 0.5;
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Dimension {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Band.Count.Guard(label: nameof(Dimension), value: ref value);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct PositiveMagnitude {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = Band.Positive.Guard(label: nameof(PositiveMagnitude), value: ref value);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UnitInterval {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = Band.Unit.Guard(label: nameof(UnitInterval), value: ref value);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct SignedUnit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = Band.SignedUnit.Guard(label: nameof(SignedUnit), value: ref value);
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
    internal static Seq<SignedAxis> Cardinal(Dimension rank) => toSeq(Items).Filter(axis => Math.Abs(value: axis.Key) <= rank.Value);
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
        validationError = Band.Angle.Guard(label: nameof(VectorAngle), value: ref value);
    internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot, Op key) =>
        from activePivot in pivot.Admit(key: key)
        from angle in key.AcceptValidated<VectorAngle>(candidate: activePivot.Compute(a: a.Value, b: b.Value))
        select angle;
    internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, Option<AnglePivot> pivot, Op key) =>
        from left in Direction.Of(value: a, context: context, key: key)
        from right in Direction.Of(value: b, context: context, key: key)
        from angle in Of(a: left, b: right, pivot: pivot.IfNone(AnglePivot.World), key: key)
        select angle;
    internal Fin<TOut> Project<TOut>(Op key) => AtomProjection.SelfOrValue<VectorAngle, double, TOut>(self: this, value: Value, key: key);
}

[BoundaryAdapter]
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

[BoundaryAdapter]
[SmartEnum<int>]
public sealed partial class RgbProfile {
    public static readonly RgbProfile Srgb = new(key: 0, rgb: RgbConfiguration.StandardRgb, range: DynamicRange.Standard);
    public static readonly RgbProfile DisplayP3 = new(key: 1, rgb: RgbConfiguration.DisplayP3, range: DynamicRange.Standard);
    public static readonly RgbProfile A98 = new(key: 2, rgb: RgbConfiguration.A98, range: DynamicRange.Standard);
    public static readonly RgbProfile Rec2020 = new(key: 3, rgb: RgbConfiguration.Rec2020, range: DynamicRange.Standard);
    public static readonly RgbProfile ProPhoto = new(key: 4, rgb: RgbConfiguration.ProPhoto, range: DynamicRange.Standard, xyz: XyzConfiguration.D50);
    public static readonly RgbProfile Rec2100Pq = new(key: 5, rgb: RgbConfiguration.Rec2100Pq, range: DynamicRange.High);
    public static readonly RgbProfile Rec2100Hlg = new(key: 6, rgb: RgbConfiguration.Rec2100Hlg, range: DynamicRange.High);
    public static readonly RgbProfile Aces20651 = new(key: 7, rgb: RgbConfiguration.Aces20651, range: DynamicRange.High);
    public static readonly RgbProfile Acescg = new(key: 8, rgb: RgbConfiguration.Acescg, range: DynamicRange.High);
    public static readonly RgbProfile Acescct = new(key: 9, rgb: RgbConfiguration.Acescct, range: DynamicRange.High);
    public static readonly RgbProfile Acescc = new(key: 10, rgb: RgbConfiguration.Acescc, range: DynamicRange.High);

    public Configuration Configuration { get; }
    public (Chromaticity Red, Chromaticity Green, Chromaticity Blue, Chromaticity White) Geometry =>
        (Configuration.Rgb.ChromaticityR, Configuration.Rgb.ChromaticityG, Configuration.Rgb.ChromaticityB,
            Configuration.Rgb.WhitePoint.Chromaticity);

    public static Fin<CamConfiguration> Condition(
        Illuminant illuminant,
        Observer observer,
        double ambientLux,
        double backgroundLuminance,
        Surround surround,
        string name,
        Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(illuminant).ToFin(Fail: op.InvalidInput())
               from view in Optional(observer).ToFin(Fail: op.InvalidInput())
               from label in Optional(name).Filter(static text => !string.IsNullOrWhiteSpace(value: text)).ToFin(Fail: op.InvalidInput())
               from ambient in op.AcceptValidated<PositiveMagnitude>(candidate: ambientLux)
               from background in op.AcceptValidated<PositiveMagnitude>(candidate: backgroundLuminance)
               select new CamConfiguration(
                   whitePoint: source.GetWhitePoint(observer: view),
                   adaptingLuminance: ambient.Value / Math.PI / 5.0,
                   backgroundLuminance: background.Value,
                   surround: surround,
                   name: label);
    }

    public Configuration Viewed(CamConfiguration condition) =>
        ReferenceEquals(objA: condition, objB: CamConfiguration.StandardRgb)
            ? Configuration
            : Cell.Claim(cell: viewed, key: condition, mint: () => new Configuration(rgbConfig: rgb, xyzConfig: xyz, camConfig: condition, dynamicRange: range))
                .Current[condition];

    private readonly RgbConfiguration rgb;
    private readonly XyzConfiguration? xyz;
    private readonly DynamicRange range;
    private readonly Atom<HashMap<CamConfiguration, Configuration>> viewed = Atom(HashMap<CamConfiguration, Configuration>());
    private RgbProfile(int key, RgbConfiguration rgb, DynamicRange range, XyzConfiguration? xyz = null) : this(key) {
        (this.rgb, this.range, this.xyz) = (rgb, range, xyz);
        Configuration = new Configuration(rgbConfig: rgb, xyzConfig: xyz, dynamicRange: range);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlendPath {
    private protected BlendPath(ColourSpace space, Configuration working) => (Space, Working) = (space, working);
    internal ColourSpace Space { get; }
    internal Configuration Working { get; }

    public sealed record Rectangular : BlendPath {
        internal Rectangular(ColourSpace space, RgbProfile reference) : base(space: space, working: reference.Configuration) { }
    }

    public sealed record Polar : BlendPath {
        internal Polar(ColourSpace space, RgbProfile reference, HueSpan span) : base(space: space, working: reference.Configuration) => Span = span;
        internal HueSpan Span { get; }
    }

    public sealed record Appearance : BlendPath {
        internal Appearance(ColourSpace space, RgbProfile reference, CamConfiguration condition)
            : base(space: space, working: reference.Viewed(condition: condition)) => Condition = condition;
        public CamConfiguration Condition { get; }
    }

    public static BlendPath Oklab { get; } = new Rectangular(space: ColourSpace.Oklab, reference: RgbProfile.Srgb);
    public static BlendPath Oklrab { get; } = new Rectangular(space: ColourSpace.Oklrab, reference: RgbProfile.Srgb);
    public static BlendPath Jzazbz { get; } = new Rectangular(space: ColourSpace.Jzazbz, reference: RgbProfile.Rec2100Pq);
    public static BlendPath Ictcp { get; } = new Rectangular(space: ColourSpace.Ictcp, reference: RgbProfile.Rec2100Pq);
    public static BlendPath Oklch(HueSpan span = HueSpan.Shorter) => new Polar(space: ColourSpace.Oklch, reference: RgbProfile.Srgb, span: span);
    public static BlendPath Oklrch(HueSpan span = HueSpan.Shorter) => new Polar(space: ColourSpace.Oklrch, reference: RgbProfile.Srgb, span: span);
    public static BlendPath Jzczhz(HueSpan span = HueSpan.Shorter) => new Polar(space: ColourSpace.Jzczhz, reference: RgbProfile.Rec2100Pq, span: span);
    public static BlendPath Hct(HueSpan span = HueSpan.Shorter) => new Polar(space: ColourSpace.Hct, reference: RgbProfile.Srgb, span: span);
    public static BlendPath Cam02(CamConfiguration condition) => new Appearance(space: ColourSpace.Cam02, reference: RgbProfile.Srgb, condition: condition);
    public static BlendPath Cam16(CamConfiguration condition) => new Appearance(space: ColourSpace.Cam16, reference: RgbProfile.Srgb, condition: condition);

    internal Unicolour Mix(Unicolour from, Unicolour to, double amount) => Switch(
        state: (From: Under(from), To: Under(to), Amount: amount),
        rectangular: static (state, route) => state.From.Mix(state.To, route.Space, state.Amount),
        polar: static (state, route) => state.From.Mix(state.To, route.Space, state.Amount, route.Span),
        appearance: static (state, route) => state.From.Mix(state.To, route.Space, state.Amount));

    internal Seq<Unicolour> Palette(Unicolour from, Unicolour to, int count) => Switch(
        state: (From: Under(from), To: Under(to), Count: count),
        rectangular: static (state, route) => toSeq(state.From.Palette(state.To, route.Space, state.Count)),
        polar: static (state, route) => toSeq(state.From.Palette(state.To, route.Space, state.Count, route.Span)),
        appearance: static (state, route) => toSeq(state.From.Palette(state.To, route.Space, state.Count)));

    private Unicolour Under(Unicolour colour) => colour.ConvertToConfiguration(Working);
}

[SmartEnum<int>]
public sealed partial class GamutPolicy {
    public static readonly GamutPolicy Clipped = new(key: 0,
        static colour => colour.IsInRgbGamut, static colour => colour.MapToRgbGamut(GamutMap.RgbClipping));
    public static readonly GamutPolicy Perceptual = new(key: 1,
        static colour => colour.IsInRgbGamut, static colour => colour.MapToRgbGamut(GamutMap.OklchChromaReduction));
    public static readonly GamutPolicy Spectral = new(key: 2,
        static colour => colour.IsInRgbGamut, static colour => colour.MapToRgbGamut(GamutMap.WxyPurityReduction));
    public static readonly GamutPolicy Pointer = new(key: 3,
        static colour => colour.IsInPointerGamut, static colour => colour.MapToPointerGamut());
    public static readonly GamutPolicy MacAdam = new(key: 4,
        static colour => !colour.IsImaginary && colour.IsInMacAdamLimits, static colour => colour.MapToMacAdamLimits());
    public static readonly GamutPolicy Unbounded = new(key: 5,
        static _ => true, static colour => colour);

    [UseDelegateFromConstructor]
    public partial bool Contains(Unicolour colour);
    [UseDelegateFromConstructor]
    public partial Unicolour Bound(Unicolour colour);
}

[SmartEnum<int>]
public sealed partial class RgbTransfer {
    public static readonly RgbTransfer Encoded = new(key: 0,
        read: static colour => (colour.Rgb.R, colour.Rgb.G, colour.Rgb.B));
    public static readonly RgbTransfer Linear = new(key: 1,
        read: static colour => (colour.RgbLinear.R, colour.RgbLinear.G, colour.RgbLinear.B));

    [UseDelegateFromConstructor]
    public partial (double Red, double Green, double Blue) Read(Unicolour colour);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeltaMetric {
    private protected DeltaMetric(DeltaE metric) => Metric = metric;
    internal DeltaE Metric { get; }

    public sealed record Opponent : DeltaMetric {
        internal Opponent(DeltaE metric) : base(metric: metric) { }
    }

    public sealed record Appearance : DeltaMetric {
        internal Appearance(DeltaE metric, RgbProfile reference, CamConfiguration condition) : base(metric: metric) =>
            (Working, Condition) = (reference.Viewed(condition: condition), condition);
        internal Configuration Working { get; }
        public CamConfiguration Condition { get; }
    }

    public static DeltaMetric Ciede2000 { get; } = new Opponent(metric: DeltaE.Ciede2000);
    public static DeltaMetric Cie76 { get; } = new Opponent(metric: DeltaE.Cie76);
    public static DeltaMetric Cie94 { get; } = new Opponent(metric: DeltaE.Cie94);
    public static DeltaMetric Cie94Textiles { get; } = new Opponent(metric: DeltaE.Cie94Textiles);
    public static DeltaMetric CmcAcceptability { get; } = new Opponent(metric: DeltaE.CmcAcceptability);
    public static DeltaMetric CmcPerceptibility { get; } = new Opponent(metric: DeltaE.CmcPerceptibility);
    public static DeltaMetric Itp { get; } = new Opponent(metric: DeltaE.Itp);
    public static DeltaMetric Z { get; } = new Opponent(metric: DeltaE.Z);
    public static DeltaMetric Hyab { get; } = new Opponent(metric: DeltaE.Hyab);
    public static DeltaMetric Ok { get; } = new Opponent(metric: DeltaE.Ok);
    public static DeltaMetric Cam02(CamConfiguration condition) => new Appearance(metric: DeltaE.Cam02, reference: RgbProfile.Srgb, condition: condition);
    public static DeltaMetric Cam16(CamConfiguration condition) => new Appearance(metric: DeltaE.Cam16, reference: RgbProfile.Srgb, condition: condition);

    public double Measure(Unicolour from, Unicolour to) => Switch(
        state: (From: from, To: to),
        opponent: static (state, route) => state.From.Difference(state.To, route.Metric),
        appearance: static (state, route) => state.From.ConvertToConfiguration(route.Working)
            .Difference(state.To.ConvertToConfiguration(route.Working), route.Metric));
}

[SmartEnum]
public sealed partial class ToneSweep {
    public static readonly ToneSweep Away = new(step: static ground => ground >= 0.5 ? -1 : 1);
    public static readonly ToneSweep Lighter = new(step: static _ => 1);
    public static readonly ToneSweep Darker = new(step: static _ => -1);

    [UseDelegateFromConstructor]
    internal partial int Step(double ground);

    internal Seq<UnitInterval> Walk(double ground, Dimension grid) =>
        (Direction: Step(ground: ground), Steps: grid.Value) switch {
            var (direction, steps) => toSeq(Enumerable.Range(start: 0, count: steps + 1))
                .Map(step => UnitInterval.Create(value: direction > 0 ? 1.0 - ((double)step / steps) : (double)step / steps)),
        };
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AppearanceReading(double Lightness, double OpponentA, double OpponentB, CamConfiguration Condition);

[BoundaryAdapter]
[ComplexValueObject]
public sealed partial class PerceptualColor {
    public double Lightness { get; }
    public double OpponentA { get; }
    public double OpponentB { get; }
    public double Alpha { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double lightness, ref double opponentA, ref double opponentB, ref double alpha) =>
        validationError = Band.Parameter.Guard(label: nameof(Lightness), value: ref lightness)
            ?? Band.Parameter.Guard(label: nameof(OpponentA), value: ref opponentA)
            ?? Band.Parameter.Guard(label: nameof(OpponentB), value: ref opponentB)
            ?? Band.Unit.Guard(label: nameof(Alpha), value: ref alpha);
    public static Dimension ToneGrid { get; } = Dimension.Create(value: 100);
    public static Fin<PerceptualColor> Of(double lightness, double opponentA, double opponentB, double alpha = 1.0, Op? key = null) =>
        Validate(lightness, opponentA, opponentB, alpha, out PerceptualColor? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput());
    public static Fin<PerceptualColor> OfRgb(byte red, byte green, byte blue, double alpha = 1.0, Op? key = null) =>
        from coverage in key.OrDefault().AcceptValidated<UnitInterval>(candidate: alpha)
        from admitted in OfOklab(colour: new Unicolour(ColourSpace.Rgb255, red, green, blue, coverage.Value), alpha: coverage.Value, key: key)
        select admitted;
    public static Fin<PerceptualColor> OfRgb(byte red, byte green, byte blue, byte alpha, Op? key = null) =>
        OfRgb(red: red, green: green, blue: blue, alpha: alpha / (double)byte.MaxValue, key: key);
    public static Fin<PerceptualColor> OfRgb(UnitInterval red, UnitInterval green, UnitInterval blue, RgbProfile profile, double alpha = 1.0, Op? key = null) =>
        from coverage in key.OrDefault().AcceptValidated<UnitInterval>(candidate: alpha)
        from admitted in OfOklab(
            colour: new Unicolour(profile.Configuration, ColourSpace.Rgb, red.Value, green.Value, blue.Value, coverage.Value).ConvertToConfiguration(Configuration.Default),
            alpha: coverage.Value, key: key)
        select admitted;
    public static Fin<PerceptualColor> OfRgb(double red, double green, double blue, RgbProfile profile, double alpha = 1.0, Op? key = null) =>
        from coverage in key.OrDefault().AcceptValidated<UnitInterval>(candidate: alpha)
        from admitted in Band.Parameter.Admits(value: red) && Band.Parameter.Admits(value: green) && Band.Parameter.Admits(value: blue)
            ? OfOklab(
                colour: new Unicolour(profile.Configuration, ColourSpace.RgbLinear, red, green, blue, coverage.Value).ConvertToConfiguration(Configuration.Default),
                alpha: coverage.Value, key: key)
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput())
        select admitted;
    public static Fin<PerceptualColor> OfArgb(int packed, Op? key = null) =>
        OfRgb(red: (byte)(packed >> 16), green: (byte)(packed >> 8), blue: (byte)packed, alpha: (byte)(packed >> 24), key: key);
    public static Fin<PerceptualColor> OfHost(System.Drawing.Color host, Op? key = null) =>
        OfArgb(packed: host.ToArgb(), key: key);
    public static Fin<PerceptualColor> OfHost(Rhino.Display.Color4f host, Option<RgbTransfer> transfer = default, Op? key = null) =>
        transfer.IfNone(RgbTransfer.Encoded) == RgbTransfer.Linear
            ? OfRgb(red: host.R, green: host.G, blue: host.B, profile: RgbProfile.Srgb, alpha: host.A, key: key)
            : from red in key.OrDefault().AcceptValidated<UnitInterval>(candidate: host.R)
              from green in key.OrDefault().AcceptValidated<UnitInterval>(candidate: host.G)
              from blue in key.OrDefault().AcceptValidated<UnitInterval>(candidate: host.B)
              from admitted in OfRgb(red: red, green: green, blue: blue, profile: RgbProfile.Srgb, alpha: host.A, key: key)
              select admitted;
    public static Fin<PerceptualColor> OfTemperature(double cct, double duv = 0.0, Locus locus = Locus.Blackbody, double luminance = 1.0, Op? key = null) =>
        Band.Positive.Admits(value: cct) && Math.Abs(duv) <= 0.05 && (duv == 0.0 || locus == Locus.Blackbody)
        && Band.Nonnegative.Admits(value: luminance)
            ? OfOklab(
                colour: duv == 0.0
                    ? new Unicolour(Configuration.Default, cct, locus, luminance)
                    : new Unicolour(Configuration.Default, new Temperature(cct, duv), luminance),
                alpha: 1.0, key: key)
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput());
    public static Fin<PerceptualColor> Achromatic(double lightness, double alpha = 1.0, Op? key = null) =>
        Of(lightness: lightness, opponentA: 0.0, opponentB: 0.0, alpha: alpha, key: key);
    public PerceptualColor Mix(PerceptualColor other, UnitInterval amount, Option<BlendPath> path = default) {
        Unicolour mixed = path.IfNone(BlendPath.Oklch()).Mix(from: AsUnicolour(), to: other.AsUnicolour(), amount: amount.Value);
        return FromOklab(colour: mixed, alpha: mixed.Alpha.A);
    }
    public Seq<PerceptualColor> Ramp(PerceptualColor to, Dimension stops, Option<BlendPath> path = default) =>
        path.IfNone(BlendPath.Oklch()).Palette(from: AsUnicolour(), to: to.AsUnicolour(), count: Math.Max(val1: stops.Value, val2: 2))
            .Map(static stop => FromOklab(colour: stop, alpha: stop.Alpha.A));
    public double ReferenceLightness => AsUnicolour().Oklrab.L;
    public double Contrast(PerceptualColor other) => AsUnicolour().Contrast(other.AsUnicolour());
    public (double RelativeLuminance, Temperature Temperature, double DominantWavelength, double ExcitationPurity) Colorimetry =>
        AsUnicolour() switch {
            { } colour => (colour.RelativeLuminance, colour.Temperature, colour.DominantWavelength, colour.ExcitationPurity),
        };
    public PerceptualColor Blend(PerceptualColor backdrop, BlendMode mode = BlendMode.Normal) {
        Unicolour blended = AsUnicolour().Blend(backdrop.AsUnicolour(), mode);
        return FromOklab(colour: blended, alpha: blended.Alpha.A);
    }
    public PerceptualColor Simulate(Cvd deficiency, UnitInterval severity) =>
        FromOklab(colour: AsUnicolour().Simulate(deficiency, severity.Value), alpha: Alpha);
    public Fin<PerceptualColor> Tone(UnitInterval tone) {
        Hct hct = AsUnicolour().Hct;
        return OfOklab(colour: new Unicolour(ColourSpace.Hct, hct.H, hct.C, tone.Value * 100.0), alpha: Alpha);
    }
    public Fin<PerceptualColor> ToneFor(PerceptualColor against, PositiveMagnitude ratio, ToneSweep sweep, Option<Dimension> grid = default, Op? key = null) {
        PerceptualColor seed = this;
        return sweep.Walk(ground: against.ReferenceLightness, grid: grid.IfNone(ToneGrid))
            .Map(tone => seed.Tone(tone: tone))
            .Choose(static candidate => candidate.ToOption())
            .TakeWhile(candidate => candidate.Contrast(other: against) >= ratio.Value)
            .Last
            .ToFin(key.OrDefault().InvalidResult());
    }
    public AppearanceReading Appearance(BlendPath.Appearance under) =>
        AsUnicolour().ConvertToConfiguration(under.Working).GetRepresentation(under.Space).Triplet switch {
            { } correlates => new AppearanceReading(
                Lightness: correlates.First,
                OpponentA: correlates.Second,
                OpponentB: correlates.Third,
                Condition: under.Condition),
        };
    public double Difference(PerceptualColor other, Option<DeltaMetric> metric = default) =>
        metric.IfNone(DeltaMetric.Ciede2000).Measure(from: AsUnicolour(), to: other.AsUnicolour());
    public bool InGamut(Option<GamutPolicy> policy = default) => policy.IfNone(GamutPolicy.Perceptual).Contains(AsUnicolour());
    public (byte Red, byte Green, byte Blue, byte Alpha) ToRgb(Option<GamutPolicy> gamut = default) =>
        gamut.IfNone(GamutPolicy.Perceptual).Bound(AsUnicolour()).Rgb.Byte255.Clipped switch {
            { } clipped => ((byte)clipped.R, (byte)clipped.G, (byte)clipped.B, byte.CreateSaturating(Math.Round(Alpha * byte.MaxValue))),
        };
    public (double Red, double Green, double Blue, double Alpha) ToRgb(RgbProfile profile, Option<GamutPolicy> gamut = default, Option<RgbTransfer> transfer = default) =>
        transfer.IfNone(RgbTransfer.Encoded).Read(colour: gamut.IfNone(GamutPolicy.Perceptual).Bound(AsUnicolour().ConvertToConfiguration(profile.Configuration))) switch {
            var (red, green, blue) => (red, green, blue, Alpha),
        };
    public Fin<int> ToArgb(Option<GamutPolicy> gamut = default, Op? key = null) =>
        gamut.IfNone(GamutPolicy.Perceptual).Bound(AsUnicolour()) switch {
            { } bounded when GamutPolicy.Clipped.Contains(colour: bounded) => bounded.Rgb.Byte255.Clipped switch {
                { } clipped => Fin.Succ(value: System.Drawing.Color.FromArgb(
                    alpha: byte.CreateSaturating(Math.Round(Alpha * byte.MaxValue)),
                    red: (int)clipped.R,
                    green: (int)clipped.G,
                    blue: (int)clipped.B).ToArgb()),
            },
            _ => Fin.Fail<int>(error: key.OrDefault().InvalidResult(detail: "colour outside the display gamut")),
        };
    public Fin<System.Drawing.Color> ToDrawing(Option<GamutPolicy> gamut = default, Op? key = null) =>
        ToArgb(gamut: gamut, key: key).Map(static packed => System.Drawing.Color.FromArgb(packed));
    public Fin<Rhino.Display.Color4f> ToColor4f(Option<GamutPolicy> gamut = default, Option<RgbTransfer> transfer = default, Op? key = null) =>
        transfer.IfNone(RgbTransfer.Encoded) == RgbTransfer.Encoded && !GamutPolicy.Clipped.Contains(colour: gamut.IfNone(GamutPolicy.Perceptual).Bound(AsUnicolour()))
            ? Fin.Fail<Rhino.Display.Color4f>(error: key.OrDefault().InvalidResult(detail: "colour outside the display gamut"))
            : ToRgb(profile: RgbProfile.Srgb, gamut: gamut, transfer: transfer) switch {
                var (red, green, blue, alpha) => Fin.Succ(value: new Rhino.Display.Color4f((float)red, (float)green, (float)blue, (float)alpha)),
            };
    private Unicolour AsUnicolour() => new(ColourSpace.Oklab, Lightness, OpponentA, OpponentB, Alpha);
    private static Fin<PerceptualColor> OfOklab(Unicolour colour, double alpha, Op? key = null) {
        Oklab lab = colour.Oklab;
        return Of(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: alpha, key: key);
    }
    private static PerceptualColor FromOklab(Unicolour colour, double alpha) {
        Oklab lab = colour.Oklab;
        return Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: alpha);
    }
}
```

## [03]-[TRANSFORM_ALGEBRA]

- Owner: `TransformSpec` is the public construction `[Union]`, each case the irreducible payload of one affine factory semantic, and `Compose` an ordered program of already-built transforms. `OrientationSense` re-closes host orientation results, `Decomposition` is the typed result `[Union]`, `DecomposeAs` and `TransformRewrite` are behavior-bearing smart-enum rows, and `Placement` is the single construction and transform-operation surface.
- Entry: `Placement.Build` constructs every spec case through one generated total `Switch`; the `Transform` extension members admit the receiver once and keep every refusal on `Fin<T>`.
- Auto: `Compose` left-composes its sequence first to last and maps the empty sequence to `Transform.Identity`; `DecomposeAs` carries each host factorization as one delegate row, `TransformRewrite` copies before every mutating host operation, and `OrientationSense` converts only admitted rigid or similarity outcomes.
- Receipt: `Decomposition` preserves every factor and orientation discriminant the selected factorization produces; construction, inverse, rewrite, bounds, list, and transpose return the admitted host value directly.
- Packages: Thinktecture.Runtime.Extensions for the union and smart-enum owners; LanguageExt.Core for the `Fin`/`Option`/`Seq` rails; Rasm.Domain (project) for `Context`, `Op`, and `Admit`; RhinoCommon for `Transform` and its factorization results.
- Growth: a factory semantic is one `TransformSpec` case and one generated-switch arm; a factorization or copy rewrite is one behavior row; a new result shape is one `Decomposition` case. Every consumer continues through `Placement`.
- Boundary: `TransformSpec` is DISTINCT-BY-DESIGN from every same-named upper twin — it names an affine CONSTRUCTION request the host factories realize, where an upper `TransformSpec` names a placement authored against a document; the discriminant is the admission path, stated here once and never per site. `Transform.Unset`, zero matrices, and pseudo-inverses are never control values; failed construction and factorization stay failures, `TryGetInverse` returning `false` rejects its pseudo-inverse output, and only `Identity` or an empty `Compose` supplies an identity value.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
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
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from angle in state.Key.AcceptInput(value: value.Angle)
                from axis in Direction.Of(value: value.Axis, context: model, key: state.Key)
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
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from start in Direction.Of(value: value.From, context: model, key: state.Key)
                from end in Direction.Of(value: value.To, context: model, key: state.Key)
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
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from point in state.Key.AcceptInput(value: value.Point)
                from normal in Direction.Of(value: value.Normal, context: model, key: state.Key)
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
                from model in state.Context.ToFin(Fail: state.Key.MissingContext())
                from plane in Admit.Plane(basis: value.Plane, key: state.Key)
                from direction in Direction.Of(value: value.Direction, context: model, key: state.Key)
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

- Owner: `Direction` is the single admitted unit-vector currency of the kernel; `VectorSpan` the anchored vector, `VectorFrame` the validated orthonormal frame over `Plane`, `VectorCone` the apex/axis/half-angle solid sector. `ChainClosure` closes the ring posture of a point chain. All four carriers are construction-gated — the private constructor is unreachable except through the validating `Of`, so an instance is its own admission evidence.
- Cases: `Direction` owns admission, reflection, refraction, and transport; `VectorSpan` anchored magnitude decomposition; `VectorFrame` orthonormal admission and chained construction; `VectorCone` containment, enclosure, and rim partition.
- Law: `Direction` implements `IValidityEvidence`, so its `IsValid` is the ruled `ValidityClaim.All` fold rather than a loose bool — the unit-length claim and the host-finiteness claim compose there and every reader sees one evidence surface.
- Entry: every constructor and host-backed transform returns `Fin<T>` under one `Op`; `Direction.Reflect` and `ParallelTransport`, the `VectorFrame` transform projection, and the `VectorCone` rotation folds construct only through `Placement.Build`.
- Auto: `Transported` re-admits every rigid-transform result against the type's OWN validity band so reflection, refraction, and parallel transport share one floor instead of gating a unit quantity on a distance-degeneracy epsilon; `VectorSpan.Value` recomposes `Direction * Magnitude` so the stored triple is the canonical decomposition; `SeedPerpendicular` is the deterministic perpendicular seed shared by frame construction and cone partition; `NewellNormal` is the one inexact polygon-normal fold every ring and panel fit composes, the exact carrier staying on the predicates ladder.
- Receipt: none — the models are self-evident admitted values, and failures carry the `Op` typed fault.
- Packages: LanguageExt.Core for the `Fin`/`Seq`/`Option` rails; Thinktecture.Runtime.Extensions for the generated owners; Rasm.Domain (project) for `Op`, `Context`, `ValidityClaim`, and the `Admit` vocabulary; RhinoCommon for the `Vector3d`, `Point3d`, `Plane`, and `Line` value structs.
- Growth: a new direction algorithm is one member on `Direction` or `VectorCone`, never a sibling `DirectionUtils`; a new frame-construction modality is one `Of` overload discriminating on input shape; a new chain posture is one `ChainClosure` row.
- Boundary: `VectorFrame.Chain` composes the one rotation-minimizing-frame owner in `Spatial/neighbors`, which owns the chain math while this page owns only frame admission over the chained planes and the `ChainClosure` posture it hands down; quaternion pose interpolation is `Parametric/projections`' and never re-derives here; `Direction.ParallelTransport` transports through given frames, so a second double-reflection implementation here is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ChainClosure {
    public static readonly ChainClosure Open = new(key: 0);
    public static readonly ChainClosure Closed = new(key: 1);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Direction : IValidityEvidence {
    private Direction(Vector3d value) => Value = value;
    public Vector3d Value { get; }
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(value: Value),
        Math.Abs(value: Value.Length - 1.0) <= EpsilonPolicy.SqrtEpsilon);
    public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext()).Bind(model => Of(value: value, tolerance: model.Absolute.Value, key: key.OrDefault()));
    internal static Fin<Direction> Of(Vector3d value, double tolerance, Op key) =>
        Admit.Directional(value: value, tolerance: tolerance, key: key).Bind(vector =>
            vector.Unitize() ? Fin.Succ(new Direction(value: vector)) : Fin.Fail<Direction>(error: key.InvalidInput()));
    private static Fin<Direction> Transported(Vector3d value, Op key) => Of(value: value, tolerance: EpsilonPolicy.SqrtEpsilon, key: key);
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
            .Bind(transform => Transported(value: transform * self.Value, key: op));
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
            double rootable when rootable > -EpsilonPolicy.ZeroTolerance => Transported(value: (eta * incident.Value) + (((eta * cosI) - Math.Sqrt(d: Math.Max(val1: 0.0, val2: rootable))) * orientedNormal), key: key),
            _ => Fin.Fail<Direction>(error: key.InvalidResult()),
        }
        select direction;
    public Fin<Direction> ParallelTransport(Seq<Plane> frames, Op? key = null) {
        Vector3d value = Value;
        Op op = key.OrDefault();
        return Admit.All(values: frames, claim: static frame => frame.IsValid, floor: 1, key: op).Bind(admittedFrames =>
            toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: admittedFrames.Count - 1))).Fold(
                initialState: Transported(value: value, key: op),
                f: (acc, i) => acc.Bind(prev =>
                    Placement.Build(
                            spec: new TransformSpec.PlaneMap(
                                From: admittedFrames[index: i - 1],
                                To: admittedFrames[index: i]),
                            key: op)
                        .Bind(transform => Transported(value: transform * prev.Value, key: op)))));
    }
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
        from valid in Admit.Plane(basis: frame, key: key.OrDefault())
        select new VectorFrame(value: valid);
    public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, ChainClosure closure, Context context, Op? key = null) =>
        NeighborKernel.BishopChain(points: points, initialNormal: initialNormal, closure: closure, context: context, key: key.OrDefault())
            .Bind(planes => planes.TraverseM(p => Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key.OrDefault())).As());
    internal static Vector3d SeedPerpendicular(Vector3d axis) {
        Vector3d seed = Vector3d.Zero;
        return seed.PerpendicularTo(other: axis) && seed.Unitize() ? seed : Vector3d.XAxis;
    }
    public static Vector3d NewellNormal(ReadOnlySpan<Point3d> ring) {
        Vector3d normal = Vector3d.Zero;
        for (int i = 0; i < ring.Length; i++) {
            (Point3d a, Point3d b) = (ring[i], ring[(i + 1) % ring.Length]);
            normal += new Vector3d(x: (a.Y - b.Y) * (a.Z + b.Z), y: (a.Z - b.Z) * (a.X + b.X), z: (a.X - b.X) * (a.Y + b.Y));
        }
        return normal;
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

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
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

## [05]-[CELL_LATTICE]

- Owner: `CellLattice` is the kernel's ONE bounded rectangular cell lattice — an index-to-world affine, a per-axis cell census, and one budget ceiling admitted together. `LatticeInterpolation` rows carry the sample reconstruction each consumer reads. Construction is gated: the private constructor is unreachable except through `Of`, so an instance is its own admission evidence and every derived member is total.
- Entry: `CellLattice.Of(Transform indexToWorld, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null)` is the general admission, `Of(ReadOnlySpan<double> affine, …)` the host-neutral twelve-value form seam and wire consumers round-trip through with `Affine` its projection dual, and `Of(BoundingBox bounds, PositiveMagnitude cell, long ceiling, Op? key = null)` the axis-aligned isotropic overload discriminating on input shape. `Center`, `Corner`, `Locate`, `Nearest`, `Contains`, and `Linear`/`Coordinate` close addressing; `Coarsen` halves the census for a pyramid level.
- Auto: `Of` computes and stores the inverse affine at admission, so `Locate` is a multiply rather than a per-call factorization and a singular map is unrepresentable past the gate. Host-neutral admission runs its twelve values through `Band.Parameter` before minting, so a wire-borne `NaN` never reaches the inverse. `Rank` derives from `Layers` — a one-layer lattice IS the plane, so no sibling 2D type exists and no consumer branches on dimension. `CellSize` reads the affine's per-axis column norm, so an anisotropic, rotated, or sheared lattice reports its own extents.
- Receipt: none — the lattice is an admitted value and its evidence is its own construction. Sweep census, budget, and outcome ride the consuming surface's receipt.
- Packages: Rasm.Domain (project) for `Op`, `Context`, and the `Admit` vocabulary; LanguageExt.Core for the `Fin`/`Option` rails; Thinktecture.Runtime.Extensions for the generated smart-enum owner; RhinoCommon for `Transform`, `Point3d`, and `BoundingBox`.
- Growth: a new addressing modality is one member; a new sample reconstruction is one `LatticeInterpolation` row carrying its own separable `Axis` body, so a consumer's recursion picks it up with no branch edited; a new census projection is one derived property. Consumer-local `Nx`/`Ny`/`Nz`, `Columns`/`Rows`, cell-center arithmetic, per-row interpolation branch ladders, and budget comparison are the deleted form.
- Boundary: the lattice carries NO payload. Scalar planes are `Numerics/matrix` `Matrix` over one lattice, a typed texel arena is the consumer's own, and the byte arena is `Drawing/pack`'s — this owner addresses cells and never stores them. Index space is column-major-free: `Linear` is the one linearization and a consumer re-deriving `x + (Nx * (y + (Ny * z)))` re-opens the collapsed duplication.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LatticeInterpolation {
    public static readonly LatticeInterpolation Nearest = new(key: 0, support: 0, continuity: 0,
        axis: static (tap, _) => tap(arg: 0));
    public static readonly LatticeInterpolation Linear  = new(key: 1, support: 1, continuity: 0,
        axis: static (tap, t) => double.Lerp(tap(arg: 0), tap(arg: 1), t));
    public static readonly LatticeInterpolation Cubic   = new(key: 2, support: 2, continuity: 1,
        axis: static (tap, t) => CatmullRom(p0: tap(arg: -1), p1: tap(arg: 0), p2: tap(arg: 1), p3: tap(arg: 2), t: t));
    internal int Support { get; }
    internal int Continuity { get; }
    internal double CenterOffset => Support == 0 ? 0.0 : 0.5;
    [UseDelegateFromConstructor] internal partial double Axis(Func<int, double> tap, double t);

    private static double CatmullRom(double p0, double p1, double p2, double p3, double t) =>
        p1 + (0.5 * t * (p2 - p0 + (t * ((2.0 * p0) - (5.0 * p1) + (4.0 * p2) - p3 + (t * ((3.0 * (p1 - p2)) + p3 - p0))))));
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CellLattice {
    private CellLattice(Transform indexToWorld, Transform worldToIndex, Dimension columns, Dimension rows, Dimension layers, long ceiling) =>
        (IndexToWorld, WorldToIndex, Columns, Rows, Layers, Ceiling) =
            (indexToWorld, worldToIndex, columns, rows, layers, ceiling);
    public Transform IndexToWorld { get; }
    public Transform WorldToIndex { get; }
    public Dimension Columns { get; }
    public Dimension Rows { get; }
    public Dimension Layers { get; }
    public long Ceiling { get; }

    public static Fin<CellLattice> Of(Transform indexToWorld, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null) {
        Op op = key.OrDefault();
        long cells = (long)columns.Value * rows.Value * layers.Value;
        return indexToWorld.TryGetInverse(inverseTransform: out Transform inverse) && inverse.IsValid
            ? cells <= ceiling && Band.Count.Admits(value: ceiling)
                ? Fin.Succ(new CellLattice(indexToWorld: indexToWorld, worldToIndex: inverse,
                      columns: columns, rows: rows, layers: layers, ceiling: ceiling))
                : Fin.Fail<CellLattice>(error: new KernelFault.OutOfRange(Label: "lattice-cells", Scalar: cells, Requirement: $"<= {ceiling}", Key: Some(op)))
            : Fin.Fail<CellLattice>(error: op.InvalidInput());
    }

    public static Fin<CellLattice> Of(ReadOnlySpan<double> affine, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null) {
        Op op = key.OrDefault();
        return affine.Length is 12 && Band.Parameter.Admits(values: affine)
            ? Of(indexToWorld: new Transform {
                  M00 = affine[0], M01 = affine[1], M02 = affine[2],  M03 = affine[3],
                  M10 = affine[4], M11 = affine[5], M12 = affine[6],  M13 = affine[7],
                  M20 = affine[8], M21 = affine[9], M22 = affine[10], M23 = affine[11], M33 = 1.0 },
                  columns: columns, rows: rows, layers: layers, ceiling: ceiling, key: op)
            : Fin.Fail<CellLattice>(error: op.InvalidInput());
    }

    public static Fin<CellLattice> Of(BoundingBox bounds, PositiveMagnitude cell, long ceiling, Op? key = null) {
        Op op = key.OrDefault();
        return bounds.IsValid
            ? from columns in op.AcceptValidated<Dimension>(candidate: (int)Math.Ceiling(a: bounds.Diagonal.X / cell.Value))
              from rows in op.AcceptValidated<Dimension>(candidate: (int)Math.Ceiling(a: bounds.Diagonal.Y / cell.Value))
              from layers in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: (int)Math.Ceiling(a: bounds.Diagonal.Z / cell.Value)))
              from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: cell.Value), key: op)
              from shift in Placement.Build(spec: new TransformSpec.Translation(Motion: (Vector3d)bounds.Min), key: op)
              from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, shift)), key: op)
              from lattice in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: ceiling, key: op)
              select lattice
            : Fin.Fail<CellLattice>(error: op.InvalidInput());
    }

    public int Rank => Layers.Value > 1 ? 3 : 2;
    public long CellCount => (long)Columns.Value * Rows.Value * Layers.Value;
    public long NodeCount => (long)(Columns.Value + 1) * (Rows.Value + 1) * (Layers.Value + 1);
    public Vector3d CellSize => new(
        x: new Vector3d(x: IndexToWorld.M00, y: IndexToWorld.M10, z: IndexToWorld.M20).Length,
        y: new Vector3d(x: IndexToWorld.M01, y: IndexToWorld.M11, z: IndexToWorld.M21).Length,
        z: new Vector3d(x: IndexToWorld.M02, y: IndexToWorld.M12, z: IndexToWorld.M22).Length);
    public double CellMeasure => Rank is 2 ? CellSize.X * CellSize.Y : CellSize.X * CellSize.Y * CellSize.Z;
    public ImmutableArray<double> Affine => [
        IndexToWorld.M00, IndexToWorld.M01, IndexToWorld.M02, IndexToWorld.M03,
        IndexToWorld.M10, IndexToWorld.M11, IndexToWorld.M12, IndexToWorld.M13,
        IndexToWorld.M20, IndexToWorld.M21, IndexToWorld.M22, IndexToWorld.M23];
    public ImmutableArray<double> Inverse => [
        WorldToIndex.M00, WorldToIndex.M01, WorldToIndex.M02, WorldToIndex.M03,
        WorldToIndex.M10, WorldToIndex.M11, WorldToIndex.M12, WorldToIndex.M13,
        WorldToIndex.M20, WorldToIndex.M21, WorldToIndex.M22, WorldToIndex.M23];

    public long Linear(int column, int row, int layer = 0) =>
        column + ((long)Columns.Value * (row + ((long)Rows.Value * layer)));
    public Dimension Extent(int ordinal) => ordinal switch { 0 => Columns, 1 => Rows, _ => Layers };
    public int Stride(int ordinal) => ordinal switch { 0 => 1, 1 => Columns.Value, _ => Columns.Value * Rows.Value };
    public double Spacing(int ordinal) => ordinal switch { 0 => CellSize.X, 1 => CellSize.Y, _ => CellSize.Z };
    public (int Column, int Row, int Layer) Coordinate(long linear) {
        long plane = (long)Columns.Value * Rows.Value;
        long layer = linear / plane, rest = linear - (layer * plane), row = rest / Columns.Value;
        return (Column: (int)(rest - (row * Columns.Value)), Row: (int)row, Layer: (int)layer);
    }

    public bool Contains(int column, int row, int layer = 0) =>
        column >= 0 && column < Columns.Value && row >= 0 && row < Rows.Value && layer >= 0 && layer < Layers.Value;
    public Point3d Center(int column, int row, int layer = 0) =>
        IndexToWorld * new Point3d(x: column + 0.5, y: row + 0.5, z: Rank is 3 ? layer + 0.5 : 0.0);
    public Point3d Corner(int column, int row, int layer = 0) =>
        IndexToWorld * new Point3d(x: column, y: row, z: Rank is 3 ? layer : 0.0);
    public Point3d Locate(Point3d sample) => WorldToIndex * sample;
    public (int Column, int Row, int Layer) Nearest(Point3d sample) {
        Point3d local = Locate(sample: sample);
        return (Column: Math.Clamp(value: (int)Math.Floor(d: local.X), min: 0, max: Columns.Value - 1),
                Row: Math.Clamp(value: (int)Math.Floor(d: local.Y), min: 0, max: Rows.Value - 1),
                Layer: Math.Clamp(value: (int)Math.Floor(d: local.Z), min: 0, max: Layers.Value - 1));
    }
    public BoundingBox Bounds {
        get {
            BoundingBox box = new(min: Point3d.Origin,
                max: new Point3d(x: Columns.Value, y: Rows.Value, z: Rank is 3 ? Layers.Value : 0.0));
            _ = box.Transform(xform: IndexToWorld);
            return box;
        }
    }

    public Fin<CellLattice> Coarsen(Op? key = null) {
        Op op = key.OrDefault();
        return from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: 2.0), key: op)
               from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, IndexToWorld)), key: op)
               from columns in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: Columns.Value / 2))
               from rows in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: Rows.Value / 2))
               from layers in op.AcceptValidated<Dimension>(candidate: Rank is 3 ? Math.Max(val1: 1, val2: Layers.Value / 2) : 1)
               from level in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: Ceiling, key: op)
               select level;
    }
}
```

## [06]-[PROJECTION_RAIL]

- Owner: `ProjectionRow` is the typed dispatch row — a `Type`/`Make` pair whose `Of<TValue>` factory erases once at declaration so call sites never spell an `(object)` cast — and `AtomProjection` is the corpus-wide raw-to-typed output dispatch every kernel surface resolves its `.Project<TOut>` output type through. `RawAdmission` is the capability vocabulary a raw-lattice caller declares its conditional arms with.
- Cases: `Rows` scans a typed row-table with identity fallthrough; `Self`, `Value`, `SelfOrValue`, `Values`, and `Custom` cover the fixed acceptance shapes; `Raw` is the one raw-`object` boundary lattice where a loose payload meets the typed world.
- Law: a conditional lattice arm reads a `CapabilitySet<RawAdmission>` the caller declares as row data, never a boolean beside the payload — magnitude admission is a property of the producing row and the set is how that row states it.
- Exemption: `Rows` carries two arities because a `params` span may not follow an optional parameter; the owner-bearing arity is the primary and the other forwards its absent owner.
- Entry: `AtomProjection.Rows` scans the row table, first match winning and `TOut == TSelf` yielding the value itself, anything else failing `key.Unsupported`; `ProjectionRow.Of` declares one row.
- Auto: the row table is data — a surface grows an output modality by adding one `ProjectionRow` beside its peers while the dispatch body never changes; `Raw` admits through the owning model's `Of`, so the rail is an admission funnel, not a cast.
- Receipt: none — the rail transports values, and failures are the `Op` `Unsupported` typed fault carrying both endpoint types.
- Packages: LanguageExt.Core for the `Fin`/`Option`/`Seq` rails; Thinktecture.Runtime.Extensions for the capability vocabulary; Rasm.Domain (project) for the `Op` fault factory and the `ICapability`/`CapabilitySet` idiom; RhinoCommon for the value structs at the `Raw` lattice; the BCL for `Type` and `ReadOnlySpan<T>`.
- Growth: a new projectable output is one `ProjectionRow` at the owning surface or one arm in the `Raw` lattice; a new conditional raw arm is one `RawAdmission` row read by set algebra, never a new parameter; a surface-local `typeof(TOut)` switch is the collapse trigger that routes here.
- Boundary: `AtomProjection` is the one sanctioned type-directed dispatch site in the kernel; inline `typeof(TOut)` reflection branching inside a consumer surface is the deleted form. `AtomProjection` stays `internal`, so consumers reach it only through their surface's `.Project<TOut>` and the public API never exposes an untyped `object` seam. `AtomProjection.Rows`' identity fallthrough IS the whole-result row — an explicit self row earns its seat only by adding admission.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RawAdmission : ICapability<RawAdmission> {
    public static readonly RawAdmission VectorMagnitude = new(key: "vector-magnitude", rank: 0);
    public int Rank { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal readonly record struct ProjectionRow(Type Output, Func<Fin<object>> Make) {
    internal static ProjectionRow Of<TValue>(Func<Fin<TValue>> make) =>
        new(Output: typeof(TValue), Make: () => make().Map(static value => (object)value!));
}

internal static class AtomProjection {
    internal static Fin<TOut> Rows<TSelf, TOut>(TSelf self, Op key, Option<Type> owner, params ReadOnlySpan<ProjectionRow> rows) {
        foreach (ProjectionRow row in rows) {
            if (row.Output == typeof(TOut)) {
                return row.Make().Map(static projected => (TOut)projected!);
            }
        }
        return typeof(TOut) == typeof(TSelf) ? Fin.Succ((TOut)(object)self!) : Fin.Fail<TOut>(error: key.Unsupported(inputType: owner.IfNone(typeof(TSelf)), outputType: typeof(TOut)));
    }
    internal static Fin<TOut> Rows<TSelf, TOut>(TSelf self, Op key, params ReadOnlySpan<ProjectionRow> rows) => Rows<TSelf, TOut>(self: self, key: key, owner: default, rows: rows);
    internal static Fin<TOut> Self<TSelf, TOut>(TSelf value, Op key, Option<Type> owner = default) =>
        typeof(TOut) == typeof(TSelf) ? Fin.Succ((TOut)(object)value!) : Fin.Fail<TOut>(error: key.Unsupported(inputType: owner.IfNone(typeof(TSelf)), outputType: typeof(TOut)));
    internal static Fin<TOut> Value<TValue, TOut>(TValue value, Op key, Option<Type> owner = default) =>
        typeof(TOut) == typeof(TValue)
            ? key.AcceptValue(value: value).Map(static accepted => (TOut)(object)accepted!)
            : Fin.Fail<TOut>(error: key.Unsupported(inputType: owner.IfNone(typeof(TValue)), outputType: typeof(TOut)));
    internal static Fin<TOut> SelfOrValue<TSelf, TValue, TOut>(TSelf self, TValue value, Op key) =>
        typeof(TOut) == typeof(TValue) ? Value<TValue, TOut>(value: value, key: key) : Self<TSelf, TOut>(value: self, key: key);
    internal static Fin<TOut> Values<TValue, TOut>(IEnumerable<TValue> values, Op key, Option<Type> owner = default) =>
        typeof(TOut) == typeof(Seq<TValue>)
            ? key.Accept(values: values).Map(static accepted => (TOut)(object)accepted!)
            : Fin.Fail<TOut>(error: key.Unsupported(inputType: owner.IfNone(typeof(TValue)), outputType: typeof(TOut)));
    internal static Fin<TOut> Custom<TValue, TOut>(TValue value, ValidityClaim claim, Op key, Option<Type> owner = default) =>
        typeof(TOut) == typeof(TValue)
            ? claim ? Fin.Succ((TOut)(object)value!) : Fin.Fail<TOut>(error: key.InvalidResult())
            : Fin.Fail<TOut>(error: key.Unsupported(inputType: owner.IfNone(typeof(TValue)), outputType: typeof(TOut)));
    internal static Fin<TOut> Raw<TOut>(object raw, Option<Context> context, Op key, Type owner, CapabilitySet<RawAdmission> admits) =>
        (raw, typeof(TOut)) switch {
            (Vector3d v, Type t) when t == typeof(Vector3d) => Value<Vector3d, TOut>(value: v, key: key),
            (Vector3d v, Type t) when t == typeof(Direction) => context.ToFin(Fail: key.MissingContext()).Bind(model => Direction.Of(value: v, context: model, key: key).Bind(direction => direction.Project<TOut>(key: key))),
            (Vector3d v, Type t) when t == typeof(double) && admits.Admits(RawAdmission.VectorMagnitude) => key.AcceptValue(value: v).Bind(valid => Value<double, TOut>(value: valid.Length, key: key)),
            (Plane p, Type t) when t == typeof(Plane) => Admit.Plane(basis: p, key: key).Bind(valid => Value<Plane, TOut>(value: valid, key: key)),
            (Plane p, Type t) when t == typeof(VectorFrame) => context.ToFin(Fail: key.MissingContext()).Bind(model => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: model, key: key).Bind(frame => frame.Project<TOut>(key: key))),
            (double d, Type t) when t == typeof(double) => Value<double, TOut>(value: d, key: key),
            (Circle c, Type t) when t == typeof(Circle) => Value<Circle, TOut>(value: c, key: key),
            (Point3d p, Type t) when t == typeof(Point3d) => Value<Point3d, TOut>(value: p, key: key),
            (Matrix matrix, Type t) when t == typeof(Matrix) => Custom<Matrix, TOut>(value: matrix, claim: matrix.IsValid, key: key),
            (Seq<double> ks, Type t) when t == typeof(Seq<double>) => ks.ForAll(Band.Parameter.Admits) ? Fin.Succ((TOut)(object)ks) : Fin.Fail<TOut>(error: key.InvalidResult()),
            (SymmetricMatrix matrix, Type t) when t == typeof(SymmetricMatrix) => Custom<SymmetricMatrix, TOut>(value: matrix, claim: matrix.IsValid, key: key),
            (VectorAngle angle, Type t) when t == typeof(VectorAngle) || t == typeof(double) => angle.Project<TOut>(key: key),
            (Direction direction, Type t) when t == typeof(Direction) || t == typeof(Vector3d) => direction.Project<TOut>(key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(inputType: owner, outputType: typeof(TOut))),
        };
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
