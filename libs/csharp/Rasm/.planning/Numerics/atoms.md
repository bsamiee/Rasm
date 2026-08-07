# [RASM_NUMERICS_ATOMS]

`Rasm.Numerics` owns the typed scalar, transform, vector, and output-projection algebras that every higher kernel concern composes.

## [01]-[INDEX]

- [02]-[SCALAR_FLOOR]: epsilon policy, generated scalar and angle admission, and the perceptual color algebra.
- [03]-[TRANSFORM_ALGEBRA]: affine construction union and the one `Placement` build, analysis, and rewrite surface.
- [04]-[VECTOR_ALGEBRA]: admitted-direction currency with the span, frame, and cone models over it.
- [05]-[CELL_LATTICE]: the ONE bounded rectangular cell lattice — index-to-world affine, per-axis census, budget ceiling.
- [06]-[PROJECTION_RAIL]: corpus-wide raw-to-typed output dispatch.

## [02]-[SCALAR_FLOOR]

- Owner: `EpsilonPolicy` names the two epsilon rows — sqrt-epsilon for near-unit and residual gates, zero-tolerance for degeneracy floors. `Dimension`, `PositiveMagnitude`, `UnitInterval`, and `SignedUnit` generate scalar admission, so every count, positive-length, normalized-parameter, or bipolar-normalized signature carries the owner, never a raw primitive re-gated per call site — the unsigned and signed normal bounds are two rows of one family, so a `[-1,1]` reading admits here rather than re-declaring the gate one stratum up. `BoundarySense`, `SignedAxis`, `VectorRelation`, `AnglePivot`, and `VectorAngle` close directional sign, cardinal axis, coplanarity, measurement pivot, and radian-bounded angle. `PerceptualColor` owns the OKLab triple with normalized alpha, its mix, ramp, tonal, contrast, contrast-targeted tonal solve, simulation, difference, compositing, colorimetric read-back, appearance reading, and gamut-safe RGB egress composing `Wacton.Unicolour` through `BlendPath`, `RgbProfile`, `DeltaMetric`, `GamutPolicy`, `ToneSweep`, and `RgbTransfer` values, never a host-edge conversion. `ToneSweep` closes the direction a contrast-targeted solve walks the tonal axis, the ground-relative row beside the two absolute ones, so a readable pigment is derived at this owner and never searched at a call site. `RgbProfile` is the branch working-space roster and the corpus' ONE `Configuration` mint — the instance is the colour-space identity, so every package above composes a row, and `Condition` plus `Viewed` extend that same mint with the viewing-condition slot rather than opening a second one; `BlendPath` splits interpolation space from the axes only some spaces admit, one row per space with the hue traversal on the polar case's payload and the viewing condition on the appearance case's, so an HDR-referred or reference-corrected space is one row while a traversal on a rectangular space and an unconditioned appearance space are both unrepresentable; `DeltaMetric` splits the difference axis the same way, an opponent row condition-free beside an appearance row carrying its condition; `AppearanceReading` carries the correlates with the condition they were measured under; `GamutPolicy` rows own a reproducibility domain with both its containment predicate and its nearest-in-domain projection; `RgbTransfer` rows own the representation the profile egress reads a bounded colour off, the companded encoding beside scene-linear light, because a return shape cannot discriminate what the ingress triple's shape does.
- Entry: `Dimension`, `PositiveMagnitude`, `UnitInterval`, and `SignedUnit` admit through generated `TryCreate`/`Validate`; `SignedAxis.Of` resolves the world or frame axis; `VectorRelation.Of` classifies and `VectorAngle.Of` measures two vectors through the ambient `Context` and pivot; `PerceptualColor.Of`/`OfRgb` admit — display bytes under the default configuration, an encoded unit-interval triple, or an unbounded scene-linear double triple, the latter two under an `RgbProfile` row — `Mix` and `Ramp` interpolate along a `BlendPath` and read the interpolated alpha off the result, `Blend` composites onto a backdrop under any `BlendMode`, `Simulate` previews a colour-vision deficiency at a unit-bounded severity, `Difference` measures perceptual distance under a `DeltaMetric` row, `ReferenceLightness` reads the reference-corrected lightness a ramp asserts monotonicity on, `Contrast` reads the WCAG ratio and `ToneFor` inverts it — holding hue and chroma while tone walks a `ToneSweep` row to the least extreme rung clearing a stated ratio against a stated backdrop, refusing when no tone clears it, `Colorimetry` reads relative luminance, correlated colour temperature, dominant wavelength, and excitation purity as one column, `OfTemperature` admits the inverse — a correlated colour temperature on either locus with an optional blackbody-referenced `Duv` offset and a stated luminance, `Appearance` reads the CAM correlates a `BlendPath.Appearance` row states, `InGamut` tests the selected reproducibility domain, and `ToRgb` bounds through the same row then either quantizes to the display-referred sRGB byte quadruple or reads the profile's double quadruple through an `RgbTransfer` row after the same domain bound — the companded encoding by default, scene-linear light on the `Linear` row, the counterpart of the two `OfRgb` profile ingresses; `RgbProfile.Viewed` mints the cam-bearing `Configuration` a direct-`Unicolour` composer states its condition through and `DeltaMetric.Measure` measures a `Unicolour` operand pair under the row's own condition; `RgbProfile.Condition` admits an authored viewing condition from a rostered illuminant, a stated observer, an ambient illuminance, a background luminance, and a `Surround`.
- Auto: generated `ValidateFactoryArguments` gates finiteness and the owner's bound, so interior code never re-validates an admitted scalar; `AnglePivot.Admit` re-validates only the case payload and `Compute` dispatches the three `Vector3d.VectorAngle` overloads through the generated `Switch`; `VectorRelation.Of` admits both operands as `Direction` before reading parallel and perpendicular relations under the context angle tolerance; `RgbProfile.Viewed` memoizes each cam-bearing `Configuration` on the condition's reference identity and resolves the package-default condition to the row's own instance, so no caller sequences a mint.
- Receipt: `AppearanceReading` alone — CAM correlates are meaningless apart from the condition that produced them, so the reading carries it; every other owner here is its own admission evidence.
- Packages: Thinktecture.Runtime.Extensions for the generated value-object, union, and smart-enum owners; LanguageExt.Core for the `Fin`/`Option`/`Seq` rails and the `Atom<HashMap<_,_>>` cell behind the cam-bearing mint cache; Wacton.Unicolour for the perceptual model behind `PerceptualColor`; Rasm.Domain (project) for the `Op` key, `Context` tolerance, and `Admit` vocabulary; RhinoCommon for the `Vector3d` and `Plane` value structs.
- Growth: a new scalar invariant is one `[ValueObject]` owner; a new axis member, relation class, pivot modality, working space, or reproducibility domain is one enum row or union case, never a sibling type; a new interpolation space is one `BlendPath` row whose case states which of the traversal and condition axes it admits, never a row per space-and-axis pair; a new difference metric is one `DeltaMetric` row on the case matching its condition dependence; a new egress representation is one `RgbTransfer` row, never a sibling `ToRgb`; egress is domain-bounded on every leg by construction, so an HDR egress publishing above-white light arrives as one `GamutPolicy` row whose domain is the whole space, never a bound-skipping flag; a declared viewing condition is a `Condition` construction at its own site, never a roster row, because a surround measures the viewer's room rather than naming a colour vocabulary member; a new epsilon is one named `EpsilonPolicy` row, and a bare epsilon literal at a call site is the deleted form; a new tonal-search direction is one `ToneSweep` row and never a comparator argument, because a caller-supplied ordering re-opens the monotonicity the walk depends on; a new color capability is one member on `PerceptualColor` reading deeper into the `Unicolour` it holds.
- Boundary: `RhinoMath.SqrtEpsilon`/`ZeroTolerance`/`TwoPI` give way to `EpsilonPolicy` and `Math.Tau` everywhere, and `RhinoMath.IsValidDouble` gives way to `double.IsFinite` on HOST-NEUTRAL shapes — host-read material instead admits through the `Domain/rails` `ValidityClaim.Finite` row, whose scalar predicate deliberately stays `RhinoMath.IsValidDouble` because it screens the host `RhinoMath.UnsetValue` sentinel a bare finiteness probe admits as an ordinary value — keeping the numeric floor portable while the assembly stays RhinoCommon-aware; a raw `double` meaning dimension, magnitude, unit parameter, or bipolar-normalized reading never crosses a signature, the generated owner does — a package above that re-declares a `[-1,1]` value object is the split-owner form this row closes; angle measurement reaches `Vector3d.VectorAngle` only through `AnglePivot.Compute`; a componentwise sRGB lerp, a hand-rolled opponent-space matrix, a host color-blend, or a call-site tone search against a contrast target never stands in for perceptual math — every host edge admits into `PerceptualColor`, interpolates through `BlendPath`, solves a readable rung through `ToneFor`, and quantizes through `ToRgb`, whose byte leg is the ONE content-key quantizer the federation addresses against and therefore carries no transfer slot at all; a hue traversal never travels beside an interpolation space as a parallel argument, because the polar case is the only shape that carries one, and a viewing condition never travels beside one either, because the appearance case is; a working space enters as an `RgbProfile` row and never as a peer-minted `Configuration`, a chromaticity table, or a whitepoint literal, because the instance carries the space's identity and a second instance re-adapts every crossing while forking the conversion cache — the cam-bearing crossing is the SAME row's `Viewed` mint, published so every chartered direct-`Unicolour` composer reaches it, and a `Configuration` carrying a `camConfig` minted anywhere else is the same deleted form; an appearance space or CAM difference metric with no stated condition is unspellable and no default surround is ever fabricated for one — a branch-wide law binding only where the compliant construction is obtainable, which is why `Viewed` and `DeltaMetric.Measure` publish rather than the law carving an exemption — while the WCAG `Contrast` read stays condition-free because WCAG fixes its own.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Globalization;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Thinktecture;
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

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct SignedUnit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= -1.0 and <= 1.0 ? null : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"SignedUnit must be in [-1,1] (got {value:R})."));
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

// RgbProfile is the branch's working-space roster and the ONE site that mints a Unicolour Configuration. That
// instance IS the colour-space identity: ConvertToConfiguration short-circuits on `config == Configuration`
// reference equality and Configuration overrides no value equality, so a second instance of one space forces a
// chromatic-adaptation round trip on every crossing and forks the lazy-conversion cache — every consuming package
// composes a row here rather than constructing its own. DynamicRange is an EXPLICIT column because the package
// default is High: an SDR row left to inherit it silently encodes the PQ/HLG transfers at the 203-nit HDR white.
// The luma-legacy broadcast presets (Rec601/Pal/Ntsc/Secam) are YbrConfiguration's axis and stay unrostered until a
// component-video decoder reaches them; a row lands here only for a space this estate states colour against.
// The cam slot is that SAME mint parameterized — `camConfig:` on the row's own Configuration ctor, reached through
// Viewed — never a second mint and never a column on the roster: a viewing condition governs the appearance spaces
// alone, so a nullable cam column would sit dead on all eleven rows while the one space that reads it went unstated.
[SmartEnum<int>]
public sealed partial class RgbProfile {
    public static readonly RgbProfile Srgb = new(key: 0, rgb: RgbConfiguration.StandardRgb, range: DynamicRange.Standard);
    public static readonly RgbProfile DisplayP3 = new(key: 1, rgb: RgbConfiguration.DisplayP3, range: DynamicRange.Standard);
    public static readonly RgbProfile A98 = new(key: 2, rgb: RgbConfiguration.A98, range: DynamicRange.Standard);
    public static readonly RgbProfile Rec2020 = new(key: 3, rgb: RgbConfiguration.Rec2020, range: DynamicRange.Standard);
    // ProPhoto is a D50-native space, so the working-white slot travels with the primaries — a D50 gamut read under
    // the D65 default adapts twice and lands a warm cast no consumer can attribute.
    public static readonly RgbProfile ProPhoto = new(key: 4, rgb: RgbConfiguration.ProPhoto, range: DynamicRange.Standard, xyz: XyzConfiguration.D50);
    public static readonly RgbProfile Rec2100Pq = new(key: 5, rgb: RgbConfiguration.Rec2100Pq, range: DynamicRange.High);
    public static readonly RgbProfile Rec2100Hlg = new(key: 6, rgb: RgbConfiguration.Rec2100Hlg, range: DynamicRange.High);
    public static readonly RgbProfile Aces20651 = new(key: 7, rgb: RgbConfiguration.Aces20651, range: DynamicRange.High);
    public static readonly RgbProfile Acescg = new(key: 8, rgb: RgbConfiguration.Acescg, range: DynamicRange.High);
    public static readonly RgbProfile Acescct = new(key: 9, rgb: RgbConfiguration.Acescct, range: DynamicRange.High);
    public static readonly RgbProfile Acescc = new(key: 10, rgb: RgbConfiguration.Acescc, range: DynamicRange.High);

    public Configuration Configuration { get; }
    // The published chromaticity geometry as ONE column, because every consumer reads the four together — a
    // container-declaration axis matching a file's own attribute, a luminance-weight derivation, a colour-matrix
    // fold — so a primaries roster above this owner carries container labels alone and a transcribed coordinate
    // table anywhere in the estate is the deleted form.
    public (Chromaticity Red, Chromaticity Green, Chromaticity Blue, Chromaticity White) Geometry =>
        (Configuration.Rgb.ChromaticityR, Configuration.Rgb.ChromaticityG, Configuration.Rgb.ChromaticityB,
            Configuration.Rgb.WhitePoint.Chromaticity);

    // The authored viewing condition. The two package presets — StandardRgb (sRGB white, 64 lux, 20 cd/m^2
    // background, Average surround) and Hct — are the ONLY lookups, and every other condition an estate states is
    // CONSTRUCTED here, once, at its own declaration site: there is no roster to grow, because a surround is a
    // measurement of the viewer's room rather than a member of a closed colour vocabulary. The white derives from a
    // rostered illuminant under a stated observer, so a transcribed tristimulus triple beside a preset that
    // publishes its own is the deleted form. The ambient field enters as ILLUMINANCE because that is what a meter
    // reads, and the package's own lux conversion is internal, so the row authors it — illuminance over pi against
    // the 20% grey reference reflectance. Surround closes the ambient axis at three (Dark, Dim, Average).
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

    // ONE cam-bearing mint per row, memoized: a Configuration INSTANCE is the working-space identity, so a
    // (space, condition) pair minted twice re-adapts every crossing and forks the lazy-conversion cache exactly as
    // a duplicate space instance does. The map keys on the condition's own reference identity — CamConfiguration
    // overrides no value equality, as Configuration does not — and the package already binds StandardRgb as the
    // Configuration default, so the default condition resolves to the row's own instance with no second mint.
    // PUBLIC because the branch legislates that an unconditioned appearance read is unspellable: a chartered
    // direct-Unicolour composer obeys that law only by obtaining the cam-bearing Configuration from this one mint,
    // and a law whose only compliant construction is unreachable legislates a peer mint into existence instead.
    public Configuration Viewed(CamConfiguration condition) =>
        ReferenceEquals(objA: condition, objB: CamConfiguration.StandardRgb)
            ? Configuration
            : viewed.Swap(cache => cache.ContainsKey(condition)
                    ? cache
                    : cache.Add(condition, new Configuration(rgbConfig: rgb, xyzConfig: xyz, camConfig: condition, dynamicRange: range)))
                .Find(condition)
                .IfNone(Configuration);

    private readonly RgbConfiguration rgb;
    private readonly XyzConfiguration? xyz;
    private readonly DynamicRange range;
    private readonly Atom<HashMap<CamConfiguration, Configuration>> viewed = Atom(HashMap<CamConfiguration, Configuration>());
    private RgbProfile(int key, RgbConfiguration rgb, DynamicRange range, XyzConfiguration? xyz = null) : this(key) {
        (this.rgb, this.range, this.xyz) = (rgb, range, xyz);
        Configuration = new Configuration(rgbConfig: rgb, xyzConfig: xyz, dynamicRange: range);
    }
}

// BlendPath carries three orthogonal axes as one shape: the interpolation SPACE names the row, and the axes only
// some spaces admit ride those cases' own payloads — the hue traversal on the polar case, the viewing condition on
// the appearance case. A rectangular space therefore has neither to spell — the package's mix reads a hue span only
// where the representation publishes a hue component, so a traversal on an opponent space is a value with no
// effect, and the case split makes both pairings unrepresentable instead of two dead columns every row carries.
// Each row states the Configuration it interpolates under, taken from the RgbProfile row that mints it: Jzazbz and
// ICtCp encode through the SMPTE PQ inverse EOTF at DynamicRange.WhiteLuminance, so an absolute-luminance blend
// states the reference white its channel scale is stated against rather than inheriting whichever operand happened
// to be the receiver, and the relative rows state the SDR reference they are invariant under. The display cylinders
// (Okhsv/Okhsl/Okhwb) are Oklch's own cylinder under a different chroma normalization, the CIE pairs (Lab/Lchab,
// Luv/Lchuv) are the predecessors the Ok family supersedes, and Munsell states colour against a measured lattice no
// consumer here declares — a row lands for a space this estate interpolates in, and nothing else.
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

    // Cam02 and Cam16 report appearance correlates that are FUNCTIONS of the viewer's adaptation, so the row is
    // constructible only from a stated condition and reads it back for the receipt — an appearance blend under a
    // fabricated surround publishes a measurement nobody declared. Hct is a polar row with no condition payload
    // because the package's own HCT transform pins its conditions internally, so that space stays separable.
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

    // One interpolation seam for both arities — the package's Palette is its own fold over Mix, so a point
    // blend and a ramp differ by the count the caller names, never by a second dispatch.
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

// GamutPolicy rows carry a REPRODUCIBILITY DOMAIN and both of its operations — the containment predicate and the
// nearest-in-domain projection — so the three RGB strategies the package parameterizes and the two physical volumes
// it publishes argument-free read as ONE vocabulary a caller names once. The MacAdam row folds the imaginary test
// into its own containment because a colour outside the spectral locus fails the optimal-limit test for a different
// reason and mapping it is meaningless. A call site spelling MapToRgbGamut, IsInPointerGamut, or MapToMacAdamLimits
// beside a row is the deleted form, and no consumer re-mints a gamut predicate pair of its own.
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

    [UseDelegateFromConstructor]
    public partial bool Contains(Unicolour colour);
    [UseDelegateFromConstructor]
    public partial Unicolour Bound(Unicolour colour);
}

// RgbTransfer names WHICH representation the profile egress reads a bounded colour off. The OfRgb ingress pair
// discriminates encoded from scene-linear by the SHAPE of its triple, and C# cannot discriminate a return the same
// way — both legs are a double quadruple — so the axis rides a row rather than a flag: Encoded is the companded
// display-referred triple the package's `Rgb` publishes and Linear the scene-linear light `RgbLinear` publishes,
// the two differing by exactly the working transfer and by nothing about gamut, working space, or alpha. Only Rgb
// carries a clipping or byte projection, which is why the byte egress has no transfer slot to name.
[SmartEnum<int>]
public sealed partial class RgbTransfer {
    public static readonly RgbTransfer Encoded = new(key: 0,
        read: static colour => (colour.Rgb.R, colour.Rgb.G, colour.Rgb.B));
    public static readonly RgbTransfer Linear = new(key: 1,
        read: static colour => (colour.RgbLinear.R, colour.RgbLinear.G, colour.RgbLinear.B));

    [UseDelegateFromConstructor]
    public partial (double Red, double Green, double Blue) Read(Unicolour colour);
}

// DeltaMetric splits the difference axis exactly as BlendPath splits interpolation: an OPPONENT row names a metric
// whose value is condition-free, and an APPEARANCE row carries the viewing condition in its own payload. The CAM
// metrics measure appearance correlates that are FUNCTIONS of the observer's adaptation, so a Cam02 or Cam16
// distance with no stated condition would publish a number measured under a surround nobody declared — the pairing
// is unspellable here rather than defaulted, and an unstated condition therefore keeps an opponent metric. WCAG
// contrast stays off this owner entirely because WCAG fixes its own condition.
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

    // Both operands rebase onto the row's own Configuration before the appearance arm measures, because the
    // package re-projects a mismatched operand onto the RECEIVER's configuration — leaving the receiver to decide
    // the condition would make the distance depend on argument order. PUBLIC on the same reasoning Viewed is: a
    // direct-Unicolour composer measuring a CAM difference obeys the stated-condition law only through this row,
    // and `Working` stays interior because it is this mint's own memo, obtainable from `RgbProfile.Viewed`.
    public double Measure(Unicolour from, Unicolour to) => Switch(
        state: (From: from, To: to),
        opponent: static (state, route) => state.From.Difference(state.To, route.Metric),
        appearance: static (state, route) => state.From.ConvertToConfiguration(route.Working)
            .Difference(state.To.ConvertToConfiguration(route.Working), route.Metric));
}

// The appearance correlates a colour reports under ONE stated condition, carried with that condition: the numbers
// mean nothing apart from the white, adapting field, background, and surround they were measured against, so the
// condition rides the receipt and a consumer reads that geometry off it instead of re-pairing numbers with a
// condition asserted beside them.
public readonly record struct AppearanceReading(double Lightness, double OpponentA, double OpponentB, CamConfiguration Condition);

// ToneSweep names the DIRECTION a contrast-targeted tonal solve walks, and it is a row set rather than a boolean
// because the third case is real: `Away` is the row a readable ink wants, moving opposite whatever ground it is
// drawn on, while the two absolute rows serve an ink whose family must stay light or dark regardless of ground.
// A caller-supplied comparator would re-open the monotonicity the walk depends on, so the axis stays closed.
[SmartEnum]
public sealed partial class ToneSweep {
    public static readonly ToneSweep Away = new(step: static ground => ground >= 0.5 ? -1 : 1);
    public static readonly ToneSweep Lighter = new(step: static _ => 1);
    public static readonly ToneSweep Darker = new(step: static _ => -1);
    public Func<double, int> Step { get; }
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
    // Profile-parameterized DISPLAY-REFERRED ingress: an encoded triple is three admitted unit values by
    // definition — a normalized ratio measure is [0,1] — so the shape of the triple, never a flag beside it,
    // discriminates encoded from the unbounded scene-linear triple below, and the row's own transfer decodes it
    // at full double precision instead of through an 8-bit quantization no source value asked for.
    public static Fin<PerceptualColor> OfRgb(UnitInterval red, UnitInterval green, UnitInterval blue, RgbProfile profile, double alpha = 1.0, Op? key = null) =>
        new Unicolour(profile.Configuration, ColourSpace.Rgb, red.Value, green.Value, blue.Value, alpha).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { } lab => Of(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: alpha, key: key),
        };
    // Profile-parameterized LINEAR ingress — the counterpart of the profile-parameterized ToRgb egress: a working-space
    // triple (Acescg the scene-linear instance) admits without a byte quantization, rebasing onto the default
    // configuration exactly as ToRgb rebases off it, so ingress and egress stay one symmetric pair.
    public static Fin<PerceptualColor> OfRgb(double red, double green, double blue, RgbProfile profile, double alpha = 1.0, Op? key = null) =>
        double.IsFinite(red) && double.IsFinite(green) && double.IsFinite(blue)
            ? new Unicolour(profile.Configuration, ColourSpace.RgbLinear, red, green, blue, alpha).ConvertToConfiguration(Configuration.Default).Oklab switch {
                { } lab => Of(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: alpha, key: key),
            }
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput());
    // The CCT ingress — the inverse of the Colorimetry temperature read. Decompile-verified ctor facts: the third
    // slot of Unicolour(double cct, Locus locus, double luminance) binds LUMINANCE, never alpha (alpha rides the
    // tuple and quad ctors alone), and Temperature(double Cct, double Duv = 0.0) is blackbody-referenced, so a
    // nonzero Planckian offset under the daylight locus is contradictory and refuses. |Duv| <= 0.05 is the package's
    // own validity bound; the 1000-20000 K high-accuracy band is published evidence, never an admission gate.
    public static Fin<PerceptualColor> OfTemperature(double cct, double duv = 0.0, Locus locus = Locus.Blackbody, double luminance = 1.0, Op? key = null) =>
        double.IsFinite(cct) && cct > 0.0 && Math.Abs(duv) <= 0.05 && (duv == 0.0 || locus == Locus.Blackbody)
        && double.IsFinite(luminance) && luminance >= 0.0
            ? (duv == 0.0
                ? new Unicolour(Configuration.Default, cct, locus, luminance)
                : new Unicolour(Configuration.Default, new Temperature(cct, duv), luminance)).Oklab switch {
                    { } lab => Of(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: 1.0, key: key),
                }
            : Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput());
    // Alpha comes OFF the interpolated value, never from a second pass: the package's mix premultiplies by default,
    // so the returned Alpha.A is the coverage-correct result and a hand-lerped straight alpha beside premultiplied
    // colour channels bends every partially-transparent tween. The path owns the space, the reference white, and
    // whether a traversal exists at all, so neither entry re-reads an interpolation column.
    public PerceptualColor Mix(PerceptualColor other, UnitInterval amount, BlendPath? path = null) =>
        (path ?? BlendPath.Oklch()).Mix(from: AsUnicolour(), to: other.AsUnicolour(), amount: amount.Value) switch {
            { } mixed => Create(lightness: mixed.Oklab.L, opponentA: mixed.Oklab.A, opponentB: mixed.Oklab.B, alpha: mixed.Alpha.A),
        };
    public Seq<PerceptualColor> Ramp(PerceptualColor to, Dimension stops, BlendPath? path = null) =>
        (path ?? BlendPath.Oklch()).Palette(from: AsUnicolour(), to: to.AsUnicolour(), count: Math.Max(val1: stops.Value, val2: 2))
            .Map(static stop => Create(lightness: stop.Oklab.L, opponentA: stop.Oklab.A, opponentB: stop.Oklab.B, alpha: stop.Alpha.A));
    // Reference-corrected lightness answers exactly the question a ramp asks, where the stored basis channel
    // mis-ranks near-black: a monotonicity assertion reads this projection, published by the Ok family for that
    // purpose, while the canonical basis keeps carrying the colour.
    public double ReferenceLightness => AsUnicolour().Oklrab.L;
    public double Contrast(PerceptualColor other) => AsUnicolour().Contrast(other.AsUnicolour());
    // The derived colorimetric facts as ONE column, on the same reasoning the RgbProfile geometry column carries:
    // every consumer that wants one wants the set — a WCAG luminance gate beside a CCT-labelled swatch beside a
    // spectral-purity admission — and each projection memoizes on the package's own first touch, so the column
    // costs no more than any single read. A consumer re-deriving luminance off Xyz.Y, or reaching Wxy.W/Wxy.X
    // past this owner for dominant wavelength and excitation purity, is the deleted form.
    public (double RelativeLuminance, Temperature Temperature, double DominantWavelength, double ExcitationPurity) Colorimetry =>
        AsUnicolour() switch {
            { } colour => (colour.RelativeLuminance, colour.Temperature, colour.DominantWavelength, colour.ExcitationPurity),
        };
    // W3C backdrop compositing over the package's own sixteen-mode vocabulary — the separable and non-separable
    // modes are one argument, so the alpha-composited contrast path flattens through Normal while a tinting or
    // shading composite names its mode, and a mode column pinned to one value on a gamut row is the deleted form.
    public PerceptualColor Blend(PerceptualColor backdrop, BlendMode mode = BlendMode.Normal) =>
        AsUnicolour().Blend(backdrop.AsUnicolour(), mode) switch {
            { } blended => blended.Oklab switch {
                { } lab => Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: blended.Alpha.A),
            },
        };
    // Colour-vision-deficiency preview at a unit-bounded severity — the accessibility gate simulates, then contrasts.
    public PerceptualColor Simulate(Cvd deficiency, UnitInterval severity) =>
        AsUnicolour().Simulate(deficiency, severity.Value).Oklab switch {
            { } lab => Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: Alpha),
        };
    // HCT tonal re-render — hue and chroma hold while tone (CIE L*, unit-scaled) moves, so a tonal ladder is a Seq
    // of Tone reads over declared stops and no palette roster mints beside the owner; the derived colour re-admits
    // through the Oklab canonical basis exactly as every other ingress does.
    public PerceptualColor Tone(UnitInterval tone) =>
        AsUnicolour().Hct switch {
            { } hct => new Unicolour(ColourSpace.Hct, hct.H, hct.C, tone.Value * 100.0).Oklab switch {
                { } lab => Create(lightness: lab.L, opponentA: lab.A, opponentB: lab.B, alpha: Alpha),
            },
        };
    // The CONTRAST-TARGETED tonal solve, the inverse of the Contrast read: hue and chroma hold from the seed while
    // tone walks the direction the sweep row names, and the answer is the LEAST extreme tone still clearing the
    // ratio, so a readable ink is derived rather than authored and never over-contrasts past what its floor asked
    // for. The walk starts at the direction's limit and stops at the first candidate that fails, which is total
    // because the ratio is monotone in tone along one direction; a seed whose whole tonal range fails against this
    // backdrop REFUSES, because handing back the nearest miss is exactly how an ink ships below the floor the
    // accessibility gate will measure it against. Every consumer that needs a readable pigment reaches this member —
    // a bisection over Tone beside Contrast at a call site is the deleted form, on the same reasoning a local
    // opponent-space matrix is.
    public Fin<PerceptualColor> ToneFor(PerceptualColor against, PositiveMagnitude ratio, ToneSweep sweep, Dimension? grid = null, Op? key = null) =>
        (Grid: (grid ?? Dimension.Create(64)).Value, Direction: sweep.Step(against.ReferenceLightness)) switch {
            var (steps, direction) => Enumerable.Range(0, steps + 1)
                .Select(step => direction > 0 ? 1.0 - (double)step / steps : (double)step / steps)
                .Select(tone => Tone(UnitInterval.Create(tone)))
                .TakeWhile(candidate => candidate.Contrast(against) >= ratio.Value)
                .LastOrDefault() switch {
                    { } admitted => Fin.Succ(value: admitted),
                    _ => Fin.Fail<PerceptualColor>(error: key.OrDefault().InvalidInput()),
                },
        };
    // The appearance read under a STATED condition — the same Appearance row a ramp interpolates along is the row a
    // reading states, so space, working white, and condition are one value at both sites and no consumer re-pairs
    // them. The reading is total on an admitted row, because the row is unconstructible without its condition.
    public AppearanceReading Appearance(BlendPath.Appearance under) =>
        AsUnicolour().ConvertToConfiguration(under.Working).GetRepresentation(under.Space).Triplet switch {
            { } correlates => new AppearanceReading(
                Lightness: correlates.First,
                OpponentA: correlates.Second,
                OpponentB: correlates.Third,
                Condition: under.Condition),
        };
    // Perceptual distance under the selected metric; Ciede2000 is the default adjudicator, and an appearance metric
    // reaches this member only carrying the condition it measured under.
    public double Difference(PerceptualColor other, DeltaMetric? metric = null) =>
        (metric ?? DeltaMetric.Ciede2000).Measure(from: AsUnicolour(), to: other.AsUnicolour());
    // Containment reads the SELECTED domain, so a display-gamut check, a Pointer real-surface check, and a MacAdam
    // optimal-limit check are one member over one row set rather than a display predicate here and two package
    // accessors re-exposed per consumer.
    public bool InGamut(GamutPolicy? policy = null) => (policy ?? GamutPolicy.Perceptual).Contains(AsUnicolour());
    // The byte leg is display-referred by construction — an 8-bit scene-linear channel states nothing — and it is the
    // ONE content-key quantizer the federation addresses against, so it carries no transfer slot and never moves.
    public (byte Red, byte Green, byte Blue, byte Alpha) ToRgb(GamutPolicy? gamut = null) =>
        (gamut ?? GamutPolicy.Perceptual).Bound(AsUnicolour()).Rgb.Byte255.Clipped switch {
            { } clipped => ((byte)clipped.R, (byte)clipped.G, (byte)clipped.B, byte.CreateSaturating(Math.Round(Alpha * byte.MaxValue))),
        };
    // The profile leg bounds into the same reproducibility domain, then reads whichever transfer the row names —
    // ENCODED by default, so a settled call site keeps the companded triple it was written against and a
    // scene-linear consumer states `RgbTransfer.Linear` to read the light instead of decoding the encoded triple.
    public (double Red, double Green, double Blue, double Alpha) ToRgb(RgbProfile profile, GamutPolicy? gamut = null, RgbTransfer? transfer = null) =>
        (transfer ?? RgbTransfer.Encoded).Read(colour: (gamut ?? GamutPolicy.Perceptual).Bound(AsUnicolour().ConvertToConfiguration(profile.Configuration))) switch {
            var (red, green, blue) => (red, green, blue, Alpha),
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
- Auto: `Direction.IsValid` is the unit-length gate, semantic rather than a mechanical fold, and `Transported` re-admits every rigid-transform result against that SAME band so reflection, refraction, and parallel transport share one floor instead of gating a unit quantity on a distance-degeneracy epsilon that lets `Unitize` mint a direction out of roundoff; `VectorSpan.Value` recomposes `Direction * Magnitude` so the stored triple is the canonical decomposition; `SeedPerpendicular` is the deterministic perpendicular seed shared by frame construction and cone partition; `NewellNormal` is the one inexact polygon-normal fold every ring and panel fit composes, the exact carrier staying on the predicates ladder.
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
    // Re-admission of an ALREADY-ADMITTED direction carried through a rigid transform. A model distance floor
    // does not gate a unit quantity and the degeneracy floor is looser still, so the band is the type's OWN
    // validity band: under sqrt-epsilon the transform has consumed every significant bit of direction and
    // `Unitize` would mint a confident unit vector out of roundoff. Reflect, Refract, and ParallelTransport all
    // read this one row — a per-member epsilon beside them is three floors for one concept.
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
        return Admit.PlaneSequence(planes: frames, allowEmpty: false, key: op).Bind(admittedFrames =>
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
    // The ONE Newell polygon-normal fold — robust on the nonplanar ring a corner cross is not. Inexact by
    // construction: an exact carrier stays on the predicates ladder; every inexact ring, panel, and chain-seed
    // fit composes this floor member instead of a page-local copy.
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

## [05]-[CELL_LATTICE]

- Owner: `CellLattice` is the kernel's ONE bounded rectangular cell lattice — an index-to-world affine, a per-axis cell census, and one budget ceiling admitted together. `LatticeInterpolation` rows carry the sample reconstruction each consumer reads. Construction is gated: the private constructor is unreachable except through `Of`, so an instance is its own admission evidence and every derived member is total.
- Entry: `CellLattice.Of(Transform indexToWorld, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null)` is the general admission, `Of(ReadOnlySpan<double> affine, …)` the host-neutral twelve-value form seam and wire consumers round-trip through with `Affine` its projection dual, and `Of(BoundingBox bounds, PositiveMagnitude cell, long ceiling, Op? key = null)` the axis-aligned isotropic overload discriminating on input shape. `Center`, `Corner`, `Locate`, `Nearest`, `Contains`, and `Linear`/`Coordinate` close addressing; `Coarsen` halves the census for a pyramid level.
- Auto: `Of` computes and stores the inverse affine at admission, so `Locate` is a multiply rather than a per-call factorization and a singular map is unrepresentable past the gate. `Rank` derives from `Layers` — a one-layer lattice IS the plane, so no sibling 2D type exists and no consumer branches on dimension. `CellSize` reads the affine's per-axis column norm, so an anisotropic, rotated, or sheared lattice reports its own extents.
- Receipt: none — the lattice is an admitted value and its evidence is its own construction. A sweep's census, budget, and outcome ride the consuming surface's receipt.
- Packages: Rasm.Domain (project) for `Op`, `Context`, and the `Admit` vocabulary; LanguageExt.Core for the `Fin`/`Option` rails; Thinktecture.Runtime.Extensions for the generated smart-enum owner; RhinoCommon for `Transform`, `Point3d`, and `BoundingBox`.
- Growth: a new addressing modality is one member; a new sample reconstruction is one `LatticeInterpolation` row; a new census projection is one derived property. A consumer's local `Nx`/`Ny`/`Nz`, `Columns`/`Rows`, cell-center arithmetic, or budget comparison is the deleted form.
- Boundary: the lattice carries NO payload. A scalar plane is `Numerics/matrix` `Matrix` over one lattice, a typed texel arena is the consumer's own, and the byte arena is `Drawing/pack`'s — this owner addresses cells and never stores them. Index space is column-major-free: `Linear` is the one linearization and a consumer re-deriving `x + (Nx * (y + (Ny * z)))` re-opens the collapsed duplication.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The sample reconstruction a lattice-backed value reads. Nearest is exact on an occupancy plane; Linear is the
// trilinear/bilinear reconstruction one Rank column selects the arity of; Cubic reads the four-tap Catmull-Rom.
[SmartEnum<int>]
public sealed partial class LatticeInterpolation {
    public static readonly LatticeInterpolation Nearest = new(key: 0, support: 0, continuity: 0);
    public static readonly LatticeInterpolation Linear  = new(key: 1, support: 1, continuity: 0);
    public static readonly LatticeInterpolation Cubic   = new(key: 2, support: 2, continuity: 1);
    // Support is the half-width in cells a reconstruction reads; a border policy sizes its pad from this row.
    internal int Support { get; }
    internal int Continuity { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// THE one bounded rectangular cell lattice. Layers == 1 IS the plane — no sibling 2D type, no dimension branch at a
// call site. IndexToWorld maps a fractional cell coordinate to world; WorldToIndex is its stored inverse, computed
// once at admission so Locate never re-factors and a singular map never reaches a consumer.
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

    // General admission: any invertible affine. The ceiling gate is the ONE budget in the kernel — IsoSurfacePolicy
    // MaxCells, VolumeGridPolicy MaxNodes, and a downstream MaximumCells all lower onto it.
    public static Fin<CellLattice> Of(Transform indexToWorld, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null) {
        Op op = key.OrDefault();
        long cells = (long)columns.Value * rows.Value * layers.Value;
        return indexToWorld.TryGetInverse(inverseTransform: out Transform inverse) && inverse.IsValid
            ? cells <= ceiling && ceiling > 0L
                ? Fin.Succ(new CellLattice(indexToWorld: indexToWorld, worldToIndex: inverse,
                      columns: columns, rows: rows, layers: layers, ceiling: ceiling))
                : Fin.Fail<CellLattice>(error: new Fault.OutOfRange(Label: "lattice-cells", Scalar: cells, Requirement: $"<= {ceiling}", Key: Some(op)))
            : Fin.Fail<CellLattice>(error: op.InvalidInput());
    }

    // Host-neutral admission — twelve row-major doubles of the 3x4 index-to-world affine. The seam and wire
    // consumers (Element coverage, the lattice wire codec) round-trip a lattice through THIS pair with no host
    // type crossing their fences; the Transform mint is interior to the kernel.
    public static Fin<CellLattice> Of(ReadOnlySpan<double> affine, Dimension columns, Dimension rows, Dimension layers, long ceiling, Op? key = null) {
        Op op = key.OrDefault();
        return affine.Length is 12
            ? Of(indexToWorld: new Transform {
                  M00 = affine[0], M01 = affine[1], M02 = affine[2],  M03 = affine[3],
                  M10 = affine[4], M11 = affine[5], M12 = affine[6],  M13 = affine[7],
                  M20 = affine[8], M21 = affine[9], M22 = affine[10], M23 = affine[11], M33 = 1.0 },
                  columns: columns, rows: rows, layers: layers, ceiling: ceiling, key: op)
            : Fin.Fail<CellLattice>(error: op.InvalidInput());
    }

    // Axis-aligned isotropic overload — the shape every kernel sweep and every downstream 2D mint spells today.
    // Cell counts derive by ceiling so a partial trailing cell is retained rather than silently clipped.
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

    // Rank is DERIVED — a one-layer lattice is the plane, so a consumer never carries a dimension flag beside it.
    public int Rank => Layers.Value > 1 ? 3 : 2;
    public long CellCount => (long)Columns.Value * Rows.Value * Layers.Value;
    public long NodeCount => (long)(Columns.Value + 1) * (Rows.Value + 1) * (Layers.Value + 1);
    // Per-axis extent from the affine's own column norms, so an anisotropic, rotated, or sheared lattice reports true.
    public Vector3d CellSize => new(
        x: new Vector3d(x: IndexToWorld.M00, y: IndexToWorld.M10, z: IndexToWorld.M20).Length,
        y: new Vector3d(x: IndexToWorld.M01, y: IndexToWorld.M11, z: IndexToWorld.M21).Length,
        z: new Vector3d(x: IndexToWorld.M02, y: IndexToWorld.M12, z: IndexToWorld.M22).Length);
    // Planar cell measure for a rank-2 lattice, cell volume for rank 3 — the one measure a density fold reads.
    public double CellMeasure => Rank is 2 ? CellSize.X * CellSize.Y : CellSize.X * CellSize.Y * CellSize.Z;
    // Host-neutral projection — the twelve row-major 3x4 affine values the neutral Of inverts; a seam or wire
    // consumer reads THIS, never the host Transform.
    public ImmutableArray<double> Affine => [
        IndexToWorld.M00, IndexToWorld.M01, IndexToWorld.M02, IndexToWorld.M03,
        IndexToWorld.M10, IndexToWorld.M11, IndexToWorld.M12, IndexToWorld.M13,
        IndexToWorld.M20, IndexToWorld.M21, IndexToWorld.M22, IndexToWorld.M23];
    // The stored inverse on the same neutral axis — a seam consumer's fractional locate never re-inverts.
    public ImmutableArray<double> Inverse => [
        WorldToIndex.M00, WorldToIndex.M01, WorldToIndex.M02, WorldToIndex.M03,
        WorldToIndex.M10, WorldToIndex.M11, WorldToIndex.M12, WorldToIndex.M13,
        WorldToIndex.M20, WorldToIndex.M21, WorldToIndex.M22, WorldToIndex.M23];

    // ONE linearization, column-fastest. A consumer re-deriving the stride expression re-opens the duplication.
    public long Linear(int column, int row, int layer = 0) =>
        column + ((long)Columns.Value * (row + ((long)Rows.Value * layer)));
    public (int Column, int Row, int Layer) Coordinate(long linear) {
        long plane = (long)Columns.Value * Rows.Value;
        long layer = linear / plane, rest = linear - (layer * plane), row = rest / Columns.Value;
        return (Column: (int)(rest - (row * Columns.Value)), Row: (int)row, Layer: (int)layer);
    }

    public bool Contains(int column, int row, int layer = 0) =>
        column >= 0 && column < Columns.Value && row >= 0 && row < Rows.Value && layer >= 0 && layer < Layers.Value;
    // Cell CENTRE at the half-offset; Corner takes the integral lattice node. Both total on an admitted lattice.
    public Point3d Center(int column, int row, int layer = 0) =>
        IndexToWorld * new Point3d(x: column + 0.5, y: row + 0.5, z: Rank is 3 ? layer + 0.5 : 0.0);
    public Point3d Corner(int column, int row, int layer = 0) =>
        IndexToWorld * new Point3d(x: column, y: row, z: Rank is 3 ? layer : 0.0);
    // Fractional cell coordinate of a world sample — the stored inverse, so no per-call factorization.
    public Point3d Locate(Point3d sample) => WorldToIndex * sample;
    // Containing cell, clamped to the census: a reconstruction pads through its LatticeInterpolation support row.
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

    // ONE pyramid step: halved census, doubled cell, same ceiling — the level chain every overview and mip fold reads.
    // Compose runs first-to-last, so the doubling scale applies in index space before the stored affine lifts to world.
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

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
