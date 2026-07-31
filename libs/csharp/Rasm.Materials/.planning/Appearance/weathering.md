# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` POLICY-ROW axis (patina · oxidation · soiling · uv-fade · biological · efflorescence) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters`, an `AgeParameter`, and the `UnitInterval` cavity scalar, and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar and the surface's own cavity evidence, never a `WeatheredCopper`/`SoiledFacade` type. An applied occurrence is one `WeatheringDose` — the row and its per-occurrence deposition exponent — whose `Eased(age, occlusion)` curve is the ONE aging law BOTH paths read: the dose scales the raw age through its own row's `CavityResponse` and then eases the scaled result, so the flat row and the slab column age by the same number at the same shade point and CANNOT fork on exposure. An effect is ONE row carrying three data columns — the `Terminal(double f)` delegate (the terminal-sampling law: a `Wacton.Unicolour.Datasets` `Colourmap` read whose DIRECTION and cap are row data, or a constant named-colour bleach — never a second sampler), the `CavityResponse` exposure law, and the ONE `SlabColumnDelta` BOTH aging paths read. Every sampled terminal is fixed in its dataset's own sRGB/D65 working space; the fold rebases it to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `RgbLinear` mix — never a default-D65 sample mixed into an `Acescg` base as if the primaries matched, the same cross-space grounding `finish#FINISH` `FinishMix.Reflectance` performs — then grounds the rebased sample in the Pointer real-surface gamut through the kernel `GamutPolicy.Pointer` row's own `Contains`/`Bound` pair: verdigris, rust, grime, chalk, biofilm, and salt bloom are REAL surface reflectances, so a ramp chroma no physical corrosion product reaches projects to the nearest real colour before any mix. The flat `MaterialParameters` `Apply` interpolation is the row-vector path the `graph#MATERIAL_GRAPH` re-evaluates and the `press#TEXTURE_PRESS` age ladder quantizes; the `ApplySlab` path is the same trajectory over an already-lowered `surface#OPENPBR_SLAB` stack, the carrier the integrator holds after `SlabStack.Lower` — the press drives the vector path because no `SlabStack`→`OpenPbrSurface` inverse exists, and both paths derive their targets from the same `SlabColumnDelta` through the `OpenPbrSurface.Of` column correspondence (`Roughness`↔`BaseRoughness`, `Metalness`↔`BaseMetalness`, `Transmission`↔`BaseTransmission`, `Sheen`↔`FuzzWeight`, `ClearcoatRoughness`↔`CoatRoughness`, `CoatColor`↔`CoatColor`, `FuzzColor`↔`FuzzColor`), so the two paths CANNOT diverge on a shared column — a second hand-mirrored flat column set is the deleted form. Patina greens the `Slab.Base` color and de-metalizes it (the conductor corrodes to a dielectric verdigris, NEVER a metal-to-metal `ConductorMetal` swap), oxidation roughens and rusts the base, soiling fouls transmission and tints the `Slab.Fuzz` grime weight and color, chalking lifts the `Slab.Coat` roughness and tints it — every aged column a convex `RgbSpectrum.Lerp`/scalar lerp of validated endpoints at `f∈[0,1]` (in-band by construction, so the slab path is TOTAL and carries no fault rail), and the flat `Apply` path's `Mix` overshoot is MAPPED back into the working gamut through `GamutPolicy.Perceptual`, never hard-faulted; a per-effect `CavityResponse` (crevice · exposed · uniform) scales the age by the consumer's cavity scalar so crevice-accumulating effects (soiling, patina, biological) ride cavity depth while exposure-driven bleaching (uv-fade) rides its complement. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters`/`MaterialParameters.Of`, the kernel `GamutPolicy` `Pointer` and `Perceptual` rows for the terminal grounding and the working-gamut projection, `surface#OPENPBR_SLAB` `SlabStack`/`Slab` for the slab columns, Wacton.Unicolour directly for the scene-linear `Mix` and the `ConvertToConfiguration` rebase, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps and `Css`/`Xkcd` named colours for the grime/chalk/biofilm/bleach tints (the hand-keyed hex literal is the deleted form), and the `MaterialFault` band-2450 rail solely through the composed `MaterialParameters.Of` egress re-admission.

## [01]-[INDEX]

- [02]-[WEATHERING]: `AgeParameter` bounds the age, `CavityResponse` axes the exposure, `SlabColumnDelta` shares the aging-target columns, `WeatheringDose` owns the one cavity-scaled eased age, `WeatheringEffect` tables the policy rows (terminal law · exposure law · slab delta), `Weathering.Apply` and `ApplySlab` fold the row vector and the slab columns, `Scene`/`SceneBand` ground every terminal and tint, `Drift` reads the aging magnitude, and `Ramp` samples the trajectory.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `CavityResponse` `[SmartEnum<string>]` the per-effect cavity-vs-exposure axis (crevice · exposed · uniform); `SlabColumnDelta` the ONE aging-target column set both paths read; `WeatheringDose` the per-occurrence (row, rate) pair owning the one `Eased(age, occlusion)` curve; `WeatheringEffect` `[SmartEnum<string>]` the effect POLICY ROWS.
- Cases: {`Patina` (Crest reversed-capped toward the mid-ramp sea-green verdigris, de-metalize, base roughen, crevice), `Oxidation` (Flare forward — light copper rust bloom deepening to dark scale, base roughen, uniform), `Soiling` (Mako reversed toward the near-black grime end, transmission fouled, grey fuzz grime, crevice), `UvFade` (constant `Css.WhiteSmoke` bleach terminal — hue-preserving desaturation toward pale, coat chalks, exposure-driven), `Biological` (Crest forward — the living film greens then darkens, green fuzz colonization, crevice/moisture-driven), `Efflorescence` (constant `Css.Linen` salt-bloom terminal — the crystalline deposit on masonry/CMU/mortar/concrete, pale fuzz veil, base roughened, crevice/moisture-driven)} — the closed axis spanning the chemical, particulate, photochemical, biotic, AND mineral facade-aging mechanisms; a new effect is ONE row, never an effect subtype and never a trajectory switch arm.
- Law: every ramp read stays `Colourmap.Map(f)` — `MapWithClipping` substitutes the `Colourmap.Black`/`White` CLIP colours rather than the ramp ends on an out-of-range fraction, and the eased fraction is in-range by construction, so the clipping overload injects a wrong terminal exactly where it fires.
- Law: BOTH folds read ONE eased age — `dose.Eased(age, occlusion)` over the row's own `CavityResponse` — so a crevice effect ages identically on the flat row and the slab column, and the `graph#MATERIAL_GRAPH` path supplies its `Texture` cavity node value as the occlusion ARGUMENT rather than as a second, silent multiplier. There is no neutral occlusion value — `Crevice(a,1)=a` while `Exposed(a,1)=0` — so the argument is REQUIRED on both entries and can never be defaulted; a defaulted occlusion would silently run one half of the roster at full age and the other at zero.
- Law: `cavityOcclusion` is the CAVITY scalar — `1.0` the fully occluded crevice, `0.0` the open face — and its baked source is the `Raster/set#TEXTURE_SET` `occlusion` channel (`ChannelOrigin.Derived` over `Raster/filter#PLANE_OP` `HeightDerivative.Occlusion`), sampled through `texture#TEXTURE_UV`. That channel stores VISIBILITY: `Occlude` deposits `open/rays` and the row's own neutral is `1.0` unoccluded, so an occlusion plane crosses into a cavity field through the landed `filter#PLANE_OP` `RemapCurve.Levels.Invert` row and NEVER by binding raw AO — the raw bind is polarity-inverted and ages every crevice effect on the open face, which reads as a plausible render rather than as a fault.
- Law: `CoatColorTo` and `FuzzColorTo` drive BOTH paths. `graph#MATERIAL_LIBRARY` `MaterialParameters` carries real `CoatColor`/`FuzzColor` columns and `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` sources the vector's coat and fuzz tints from them, so the flat `Age` fold lerps each toward its delta through the same check-then-map the base colour takes; the prior slab-only carve — a flat row carrying no coat/fuzz colour source — is deleted with the premise that held it, and a chalked coat now reads the same tint on the vector the integrator lowers and on the plane the press bakes.
- Law: a curvature-keyed exposure row is NOT admissible on the current axis. `Convex` over the cavity scalar alone is `age·(1−occ)` — byte-identical to `Exposed` — because convexity and openness are one number until a SECOND evidence field carries the signed `Raster/set#TEXTURE_SET` `curvature` measure; the `IDEAS.md [CURVATURE_DRIVEN_WEAR]` card owns that widening (`Scale(age, SurfaceExposure)`) and mints `Convex`/`Concave` with it, so minting the row early is a name-only sibling the collapse law deletes.
- Law: the trajectory's PLANE form composes at the raster stratum, never here. `Ramp` returns the effect's own scene-linear texel run and a consumer lifts it in one call — `texture#TEXTURE_UV` `TextureSource.Image.Of(width, height: 1, ramp.Map(static b => new ShadeVec4(b.R, b.G, b.B, 1.0)), key)` — so an aging trajectory samples as a one-dimensional palette through the estate's own sampler with zero new surface. `Appearance` core consumes no `Raster` type, so a `Fin<TexturePlane>` ramp mint here would invert the folder strata; and `filter#PLANE_OP` `RemapCurve.Lut` is NOT its applying owner — that case carries a TinyEXR `Lut3D` parsed from `.cube` SOURCE TEXT with no lattice-construction seam and a `TableKey` minted from those bytes, so a generated ramp has no legal `Lut` spelling.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion, Op key)` ages the flat row vector as a PURE per-dose fold — a `Mix` that overshoots the working gamut is checked then projected back through `GamutPolicy.Perceptual` (the check-then-bound law), so the ONE fallible point is the composed `MaterialParameters.Of` egress re-admission and `Fin<T>` aborts solely on a genuinely degenerate column; `public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion)` ages the lowered `surface#OPENPBR_SLAB` slab columns and is TOTAL — every aged column is a convex lerp of a validated endpoint toward a validated terminal at `f∈[0,1]`, in-band by construction, so no fault can arise and a `Fin<SlabStack>` would fabricate a rail the inputs cannot trip. Both fold the dose `Seq` left-to-right by the identical call `dose.Eased(age.Value, cavityOcclusion.Value)`; arity is one — a multi-effect aging is the fold, never a per-effect method. The two entries differ in CARRIER alone: `Apply` produces the row vector `press#TEXTURE_PRESS` quantizes into its age ladder and `graph#MATERIAL_GRAPH` re-evaluates, `ApplySlab` produces the already-lowered stack the integrator holds after `SlabStack.Lower`, and the press drives the vector path because no `SlabStack`→`OpenPbrSurface` inverse exists on the surface page.
- Packages: `surface#OPENPBR_SLAB` (composed — the `SlabStack`/`Slab` columns the slab aging drives), `graph#MATERIAL_LIBRARY` (composed — `MaterialParameters`/`MaterialParameters.Of` with its `CoatColor`/`FuzzColor` columns and `PortValue.SceneLinear`), Wacton.Unicolour (composed — scene-linear `Mix` toward each rebased terminal, `ConvertToConfiguration` rebasing the sRGB/D65 dataset sample into the `Acescg` scene-linear space, and `Difference(DeltaE.Ciede2000)` the `Drift` calibration metric), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramps the terminal delegates sample, and the `Css`/`Xkcd` named-colour tints `Xkcd.Charcoal`/`Css.WhiteSmoke`/`Xkcd.DarkForestGreen` resolved per the datasets named-colour law), Rasm (project — `UnitInterval`, and the `GamutPolicy` `Perceptual` working-gamut and `Pointer` real-surface rows the mix and the terminal grounding check and bound through), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` ROW carrying its terminal delegate, `CavityResponse`, and `SlabColumnDelta` — zero dispatch edits, never a per-effect material or a second appearance owner; a new aging-curve shape is the dose's `Rate` exponent, the eased `scaled^rate` the one curve every row reads; a new slab-column target is one `Option`-typed column on `SlabColumnDelta` (the flat path inherits it through the `Of` correspondence); a new exposure law is one `CavityResponse` row carrying its `Scale(age, occlusion)` delegate, admitted only where the scalar it reads genuinely discriminates; a new terminal is row data — a forward/reversed/capped `Colourmap` read or a `Css`/`Xkcd`/`Nord` named-colour constant through the same `Scene` rebase+grounding law, never a hand-keyed `RgbSpectrum` literal. Aging is an AUTHORED operator over the `surface#OPENPBR_SLAB` vector and the `graph#MATERIAL_LIBRARY` row — MaterialX 1.39 carries no standard aging node, so the effect set is grounded in the measured ramps and the slab projection, not a node-category parity target.
- Boundary: `Weathering.Apply` is the ONE flat aging operator — an aged-material type is the deleted form. The terminal law is the row's `Terminal(double f)` delegate: the ramp DIRECTION is a LUT fact, not a naming convention — `Crest` runs light green → teal → dark navy, `Flare` light copper-orange → maroon → dark purple-brown, `Mako` near-black → slate → pale mint — so patina samples Crest REVERSED and capped (`Map(1 − 0.75f)`: young tarnish reads the dark teal end, full age the mid-ramp sea-green verdigris; the pale yellow-green ramp head is no corrosion product), oxidation samples Flare FORWARD (rust bloom deepening to dark scale), soiling samples Mako REVERSED (grime darkening toward near-black), biological samples Crest FORWARD (the film greens then darkens), uv-fade mixes toward the CONSTANT pale `Css.WhiteSmoke` bleach (chromophore loss is hue-preserving desaturation toward pale — no shipped ramp bleaches, so the constant arm of the same delegate column is the honest terminal), and efflorescence mixes toward the CONSTANT `Css.Linen` salt-white (no perceptual ramp traverses toward a crystalline salt deposit — the same constant arm); a forward `Map(f)` asserted over an unread LUT direction was the deleted form. Every sampled terminal crosses the one `Scene` boundary — `ConvertToConfiguration(PortValue.SceneLinear)` rebase preserving the device-independent XYZ, then Pointer real-surface grounding (`GamutPolicy.Pointer`, check-then-bound) — BEFORE the `Mix(rebased, ColourSpace.RgbLinear, f, premultiplyAlpha: false)`, while the MIXED row is never Pointer-forced (a fresh conductor F0 is not a diffuse reflectance, and a near-zero age must not snap the row onto the Pointer boundary); a mixed color that overshoots the working gamut is checked then projected back through `GamutPolicy.Perceptual` — the perceptual chroma-reduction map, never an RGB clamp and never a hard fault, because an epsilon overshoot at a ramp extreme (the D65→D60 adaptation edge of a rebased sRGB sample) is a mappable pipeline fact, not a domain error; emission is NOT aged — weathering shifts a surface's reflectance, not its self-emission, so the fold leaves `Emission`/`EmissionLuminance` untouched (a luminous sign does not green with a copper roof), and a future thermochromic/phosphorescent decay is one delta column, never an emission lerp smuggled onto the reflectance terminal. The flat columns DERIVE from the one `SlabColumnDelta` through the `OpenPbrSurface.Of` correspondence — `Roughness`←`BaseRoughnessTo`, `Metalness`←`BaseMetalnessTo`, `Transmission`←`BaseTransmissionTo`, `Sheen`←`FuzzWeightTo` (the row's sheen lowers to the fuzz weight), `ClearcoatRoughness`←`CoatRoughnessTo`, `CoatColor`←`CoatColorTo`, `FuzzColor`←`FuzzColorTo` — so a chalked finish reads the SAME raised and tinted coat and a soiled row the SAME fouled transmission and grime tint on both paths BY CONSTRUCTION (the prior duplicated flat column set had already drifted from its slab mirror on three rows and is deleted). `ApplySlab` drives every weathered slab column by the eased fraction BEFORE the `ToLayered` collapse: the `Slab.Base` color lerps toward the rebased terminal (the slab path greens the copper exactly as the flat path does), its `Metalness` drops toward `BaseMetalnessTo` (patina/oxidation corrode the conductor to a dielectric corrosion product — verdigris/rust are dielectrics, so the aging DE-METALIZES the base rather than swapping one `ConductorMetal` for another the 8-member smart-enum cannot represent), its `Roughness`/`Transmission` shift, the `Slab.Coat` `Roughness` rises (chalking) and its `Color` tints toward `CoatColorTo`, and the `Slab.Fuzz` `Weight`/`Color` rise toward the grime — each `None` column leaving its slab value untouched (a typed absence, never an in-band `-1.0` sentinel a `[0,1]` column cannot otherwise carry); every aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of two validated in-band endpoints at `f∈[0,1]`, so the slab aging is TOTAL and returns a bare `SlabStack`; the cavity sample scales the uniform age per shade point through each row's `CavityResponse` so spatially-varying weathering (grime in crevices, sun-bleached exposed faces, algae in the damp shaded joint) rides the existing fold — `Crevice` (soiling, patina, biological) at `age·occlusion`, `Exposed` (uv-fade) at `age·(1−occlusion)`, `Uniform` (bulk oxidation) at `age` — never a second aging surface.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                           // Fin, Seq, Option (the rail TYPES; the static Prelude below carries Some/None/toSeq)
using Rasm.Domain;                           // Op (the fault-correlation key the one MaterialParameters.Of egress re-admission rails on)
using Rasm.Materials.Appearance.Bsdf;        // RgbSpectrum (MaterialFault rails only inside the composed MaterialParameters.Of)
using Rasm.Materials.Appearance.Graph;       // MaterialParameters, PortValue (PortValue.SceneLinear is the Acescg working space) — declared in the .Graph child namespace, not auto-imported by the parent
using Rasm.Materials.Appearance.Surface;     // Slab, SlabStack
using Rasm.Numerics;                         // UnitInterval, GamutPolicy (the kernel Pointer real-surface row this fold checks and bounds through)
using Thinktecture;                          // [SmartEnum]/[ValueObject], [UseDelegateFromConstructor], ComparerAccessors, ValidationError
using Wacton.Unicolour;                      // Unicolour, ColourSpace, DeltaE, ConvertToConfiguration
using Wacton.Unicolour.Datasets;             // Colourmaps, Css, Xkcd
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;          // folder-root, beside finish#FINISH and acquisition#ACQUISITION; MaterialParameters is the .Graph child the prelude imports

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

// The per-effect exposure law mapping the consumer's [0,1] CAVITY scalar to the effect's age multiplier: 1.0 is the
// fully occluded crevice and 0.0 the open face, so crevice-accumulating effects (soiling deposit, patina pooling,
// biofilm) age with the scalar, exposure-driven bleaching (uv-fade) ages with its complement, and a uniform effect
// (bulk oxidation) ignores it. The Raster/set occlusion channel stores VISIBILITY, so a plane crosses into this
// scalar through the filter#PLANE_OP RemapCurve.Levels.Invert row — a raw AO bind is polarity-inverted. The delegate
// rides the row so both folds read CavityResponse.Scale(age, occlusion) by data, never a switch on an exposure enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityResponse {
    public static readonly CavityResponse Crevice = new("crevice", static (age, occ) => age * occ);
    public static readonly CavityResponse Exposed = new("exposed", static (age, occ) => age * (1.0 - occ));
    public static readonly CavityResponse Uniform = new("uniform", static (age, _) => age);

    [UseDelegateFromConstructor]
    public partial double Scale(double age, double occlusion);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ONE aging-target column set BOTH paths read (was two hand-mirrored parallel sets that had already drifted): the
// slab fold drives the surface#OPENPBR_SLAB columns directly, and the flat fold derives its MaterialParameters targets
// through the OpenPbrSurface.Of column correspondence — Roughness↔BaseRoughness, Metalness↔BaseMetalness,
// Transmission↔BaseTransmission, Sheen↔FuzzWeight, ClearcoatRoughness↔CoatRoughness, CoatColor↔CoatColor,
// FuzzColor↔FuzzColor — so the two paths CANNOT diverge on a shared column. None leaves the column at its fresh value
// (a typed absence, never an in-band -1.0 sentinel a [0,1] column cannot carry).
public readonly record struct SlabColumnDelta(
    Option<double> BaseRoughnessTo,
    Option<double> BaseMetalnessTo,
    Option<double> BaseTransmissionTo,
    Option<double> CoatRoughnessTo,
    Option<RgbSpectrum> CoatColorTo,
    Option<double> FuzzWeightTo,
    Option<RgbSpectrum> FuzzColorTo);

// One applied occurrence of an effect: the policy row plus the per-occurrence deposition exponent. Eased is the ONE
// aging law both folds read — the row's own CavityResponse scales the raw age by the shade point's cavity scalar, then
// the sub-linear rate exponent eases the SCALED value, so the flat row and the slab column cannot fork on exposure and
// the press reaches the exposure law at vector grain. Both Scale arms are products of two [0,1] values, so the clamp
// stays a NaN guard rather than a range fix and the terminal sample is in-range by construction with no outer clamp.
// The rate floor is comparison-ordered, not Math.Max — Max propagates NaN, so a non-finite consumer Rate lands at the
// floor and the TOTAL claim on both folds holds over any raw dose.
public readonly record struct WeatheringDose(WeatheringEffect Effect, double Rate) {
    const double RateFloor = 0.1;
    public double Eased(double age, double occlusion) =>
        Math.Pow(Math.Clamp(Effect.Cavity.Scale(age, occlusion), 0.0, 1.0), Rate >= RateFloor ? Rate : RateFloor);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The closed effect axis as POLICY ROWS spanning the chemical (patina/oxidation), particulate (soiling), photochemical
// (uv-fade), biotic (biological), and mineral (efflorescence) facade-aging mechanisms: each row carries its terminal-sampling law as a delegate
// column — ramp DIRECTION, cap, and a constant bleach are row DATA, never a second sampler — plus its exposure law and
// the one SlabColumnDelta. A new effect is one row with zero dispatch edits. Terminal directions are LUT facts: Crest
// runs light green→teal→dark navy, Flare light copper→maroon→dark purple-brown, Mako near-black→slate→pale mint.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeatheringEffect {
    // Declaration order is load-bearing: the rows read these tints at type-init. Datasets NAMED colours (a soot-grey
    // charcoal grime, a pale chalk bloom, a dark algal green), never hand-keyed hex — each resolves through the ONE
    // SceneBand law (sRGB/D65 rebase -> Pointer real-surface grounding -> validated band).
    static readonly RgbSpectrum GrimeFuzz = Weathering.SceneBand(Xkcd.Charcoal);
    static readonly RgbSpectrum ChalkCoat = Weathering.SceneBand(Css.WhiteSmoke);
    static readonly RgbSpectrum BioFilm   = Weathering.SceneBand(Xkcd.DarkForestGreen);
    static readonly RgbSpectrum SaltBloom = Weathering.SceneBand(Css.Linen);

    // Patina: Crest REVERSED and capped at the mid-ramp sea-green (Map(0.25) at full age) — young tarnish samples the
    // dark teal end, full age the verdigris; the pale yellow-green ramp head is no corrosion product. De-metalizes.
    public static readonly WeatheringEffect Patina = new("patina",
        static f => Colourmaps.Crest.Map(1.0 - 0.75 * f), CavityResponse.Crevice,
        new SlabColumnDelta(BaseRoughnessTo: Some(0.55), BaseMetalnessTo: Some(0.0), BaseTransmissionTo: None, CoatRoughnessTo: None, CoatColorTo: None, FuzzWeightTo: None, FuzzColorTo: None));

    // Oxidation: Flare FORWARD — a light copper-orange rust bloom deepens through maroon to the dark purple-brown scale.
    public static readonly WeatheringEffect Oxidation = new("oxidation",
        static f => Colourmaps.Flare.Map(f), CavityResponse.Uniform,
        new SlabColumnDelta(BaseRoughnessTo: Some(0.80), BaseMetalnessTo: Some(0.2), BaseTransmissionTo: None, CoatRoughnessTo: Some(0.85), CoatColorTo: None, FuzzWeightTo: None, FuzzColorTo: None));

    // Soiling: Mako REVERSED — the particulate deposit darkens through slate toward the near-black grime end; fouling
    // kills transmission and the grey grime rides the fuzz slot (charcoal, not the fresh fuzz tint).
    public static readonly WeatheringEffect Soiling = new("soiling",
        static f => Colourmaps.Mako.Map(1.0 - f), CavityResponse.Crevice,
        new SlabColumnDelta(BaseRoughnessTo: None, BaseMetalnessTo: None, BaseTransmissionTo: Some(0.0), CoatRoughnessTo: None, CoatColorTo: None, FuzzWeightTo: Some(0.4), FuzzColorTo: Some(GrimeFuzz)));

    // UvFade: the CONSTANT pale bleach terminal — chromophore loss is hue-preserving desaturation toward pale, not a
    // hue traverse, and no shipped ramp bleaches (Vlag's ends are saturated blue and red), so the constant arm of the
    // same delegate column is the honest terminal law; the coat chalks alongside.
    public static readonly WeatheringEffect UvFade = new("uv-fade",
        static _ => Css.WhiteSmoke, CavityResponse.Exposed,
        new SlabColumnDelta(BaseRoughnessTo: Some(0.40), BaseMetalnessTo: None, BaseTransmissionTo: None, CoatRoughnessTo: Some(0.50), CoatColorTo: Some(ChalkCoat), FuzzWeightTo: None, FuzzColorTo: None));

    // Biological: Crest FORWARD — the living film (algae · lichen · moss) greens then darkens on shaded/damp faces;
    // the biotic mechanism distinct from the grey particulate Soiling and the OPPOSITE exposure law to UvFade (it
    // COLONIZES the protected crevice the sun bleaches). The film coats any substrate, leaving the conductor intact.
    public static readonly WeatheringEffect Biological = new("biological",
        static f => Colourmaps.Crest.Map(f), CavityResponse.Crevice,
        new SlabColumnDelta(BaseRoughnessTo: Some(0.75), BaseMetalnessTo: None, BaseTransmissionTo: None, CoatRoughnessTo: None, CoatColorTo: None, FuzzWeightTo: Some(0.5), FuzzColorTo: Some(BioFilm)));

    // Efflorescence: the MINERAL mechanism — dissolved salts wick to the surface of masonry/CMU/mortar/concrete and
    // crystallize as the pale bloom where moisture lingers (the damp crevice feeds it, so it rides Crevice like the
    // deposits it resembles); the constant Linen terminal is the salt-white no ramp traverses, the powdery veil rides
    // the fuzz slot and the crystalline crust roughens the base. The canonical masonry-facade aging the AEC families
    // demand — one row, zero dispatch edits.
    public static readonly WeatheringEffect Efflorescence = new("efflorescence",
        static _ => Css.Linen, CavityResponse.Crevice,
        new SlabColumnDelta(BaseRoughnessTo: Some(0.85), BaseMetalnessTo: None, BaseTransmissionTo: None, CoatRoughnessTo: None, CoatColorTo: None, FuzzWeightTo: Some(0.6), FuzzColorTo: Some(SaltBloom)));

    [UseDelegateFromConstructor]
    public partial Unicolour Terminal(double f);
    public CavityResponse Cavity { get; }
    public SlabColumnDelta Slab { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Weathering {
    // Aging over the flat MaterialParameters row vector — the carrier press#TEXTURE_PRESS quantizes into its age
    // ladder and graph#MATERIAL_GRAPH re-evaluates. The cavity scalar is REQUIRED: no neutral value exists, since a
    // Crevice row reads 1.0 as full age while an Exposed row reads it as none. The fold is PURE — a working-gamut
    // overshoot maps back per effect — so the ONE fallible point is the composed egress re-admission, the same
    // gamut/unit gate a registered row passes.
    public static Fin<MaterialParameters> Apply(
        MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion, Op key) =>
        MaterialParameters.Of(
            effects.Fold(baseRow, (row, dose) => Age(row, dose.Effect, dose.Eased(age.Value, cavityOcclusion.Value))), key);

    // The same trajectory over an already-lowered surface#OPENPBR_SLAB SlabStack — the carrier the integrator holds
    // after SlabStack.Lower, driven BEFORE the ToLayered collapse. The dose supplies the IDENTICAL eased age the flat
    // fold reads, so the two carriers cannot diverge on a shared column. TOTAL: every aged column is a convex blend of
    // validated endpoints at f∈[0,1], so a Fin<SlabStack> here would model a non-finite the inputs cannot produce.
    public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion) =>
        effects.Fold(stack, (s, dose) => AgeSlab(s, dose.Effect, dose.Eased(age.Value, cavityOcclusion.Value)));

    // The perceptual aging magnitude: the CIEDE2000 difference between the fresh and aged base colors — the metric
    // graph#MATERIAL_LIBRARY NearestChecker takes as the caller's DeltaE policy value (Ciede2000 the pigment-row
    // default), so a trajectory calibrates against a measured aging series in the one appearance drift currency.
    public static double Drift(MaterialParameters fresh, MaterialParameters aged) =>
        fresh.BaseColor.Difference(aged.BaseColor, DeltaE.Ciede2000);

    // The pre-baked scene-linear aging trajectory: an even N-sample of the ROW'S terminal law (direction, cap, and
    // constant respected — a raw Colourmap.Palette would ignore a reversed or constant row), every sample crossing the
    // SAME Scene rebase+grounding law the live folds read. A raster consumer lifts the run into the estate's own
    // sampler in one call — TextureSource.Image.Of over `ramp.Map(b => new ShadeVec4(b.R, b.G, b.B, 1.0))` at height 1
    // — so an aging trajectory samples as a one-dimensional palette with no plane owner declared at this stratum.
    public static Seq<RgbSpectrum> Ramp(WeatheringEffect effect, int count) =>
        Math.Max(2, count) switch {
            var n => toSeq(Enumerable.Range(0, n)).Map(i => SceneBand(effect.Terminal(i / (n - 1.0)))),
        };

    // Total per-effect flat aging: the targets DERIVE from the one SlabColumnDelta through the OpenPbrSurface.Of
    // correspondence (the row's Sheen lowers to the fuzz weight, ClearcoatRoughness to the coat roughness, and the
    // coat/fuzz tints land on the row's own colour columns), the mix runs in scene-linear RgbLinear toward the
    // Pointer-grounded terminal, and an overshooting result is checked then MAPPED (OklchChromaReduction —
    // perceptual, never an RGB clamp), so no per-effect fault exists for a projectable pipeline fact. Emission columns
    // are untouched — weathering shifts reflectance, never self-emission.
    static MaterialParameters Age(MaterialParameters row, WeatheringEffect effect, double f) =>
        (effect.Slab, row.BaseColor.Mix(SceneTerminal(effect, f), ColourSpace.RgbLinear, f, premultiplyAlpha: false)) switch {
            var (d, mixed) => row with {
                BaseColor = Mapped(mixed),
                Roughness = LerpToward(row.Roughness, d.BaseRoughnessTo, f),
                Metalness = LerpToward(row.Metalness, d.BaseMetalnessTo, f),
                Transmission = LerpToward(row.Transmission, d.BaseTransmissionTo, f),
                Sheen = LerpToward(row.Sheen, d.FuzzWeightTo, f),
                ClearcoatRoughness = LerpToward(row.ClearcoatRoughness, d.CoatRoughnessTo, f),
                CoatColor = LerpColorUni(row.CoatColor, d.CoatColorTo, f),
                FuzzColor = LerpColorUni(row.FuzzColor, d.FuzzColorTo, f) },
        };

    // The slab-column aging: the base COLOR ages toward the scene-linear terminal (the slab path greens the copper
    // exactly as the flat path does), metalness drops (the conductor corrodes to a dielectric, never a ConductorMetal
    // swap), roughness/transmission shift, and the coat/fuzz tint toward their chalk/grime targets.
    static SlabStack AgeSlab(SlabStack stack, WeatheringEffect effect, double f) =>
        SceneBand(effect.Terminal(f)) switch {
            var baseTerminal => new SlabStack(stack.Slabs.Map(slab => AgeSlabCase(slab, effect.Slab, baseTerminal, f))),
        };

    static Slab AgeSlabCase(Slab slab, SlabColumnDelta d, RgbSpectrum baseTerminal, double f) => slab.Switch<Slab>(
        fuzz: fz => fz with {
            Weight = LerpToward(fz.Weight, d.FuzzWeightTo, f),
            Color = LerpColor(fz.Color, d.FuzzColorTo, f) },
        coat: c => c with {
            Roughness = LerpToward(c.Roughness, d.CoatRoughnessTo, f),
            Color = LerpColor(c.Color, d.CoatColorTo, f) },
        emission: static e => e,
        @base: b => b with {
            BaseColor = b.BaseColor.Lerp(baseTerminal, f),
            Metalness = LerpToward(b.Metalness, d.BaseMetalnessTo, f),
            Roughness = LerpToward(b.Roughness, d.BaseRoughnessTo, f),
            Transmission = LerpToward(b.Transmission, d.BaseTransmissionTo, f) });

    // The ONE authored-colour boundary every terminal, tint, and trajectory sample crosses: rebase the raw dataset
    // colour (fixed in ITS OWN sRGB/D65 working space) onto the scene PortValue.SceneLinear (Acescg) pipeline —
    // preserving the device-independent XYZ, never a Rec.709-linear sample treated as AP1-linear (the finish#FINISH
    // grounding law) — then ground it in the Pointer real-surface gamut through the graph#MATERIAL_LIBRARY wrapper
    // (check-then-map): a weathering product is a REAL surface reflectance, so a ramp chroma no physical corrosion
    // product reaches projects to the nearest real colour before any mix.
    static Unicolour Scene(Unicolour raw) =>
        raw.ConvertToConfiguration(PortValue.SceneLinear) switch {
            var scene => GamutPolicy.Pointer.Contains(scene) ? scene : GamutPolicy.Pointer.Bound(scene),
        };

    static Unicolour SceneTerminal(WeatheringEffect effect, double f) => Scene(effect.Terminal(f));

    // The same law as a validated RgbSpectrum band for the slab columns and the named tints (the slab carries
    // RgbSpectrum, not Unicolour): rebase, ground, read RgbLinear, clamp non-negative for the carrier.
    internal static RgbSpectrum SceneBand(Unicolour raw) =>
        Scene(raw).RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };

    // The ONE working-gamut projection every mixed colour crosses: an epsilon overshoot at a ramp extreme is a
    // mappable pipeline fact, so the check-then-map runs once here rather than inline at each mixing column.
    static Unicolour Mapped(Unicolour mixed) =>
        GamutPolicy.Perceptual.Contains(mixed) ? mixed : GamutPolicy.Perceptual.Bound(mixed);

    // A None target leaves the column at its fresh value; Some(target) eases toward it by f — the typed-absence lerp.
    static double LerpToward(double current, Option<double> target, double f) => target.Match(Some: to => current + (to - current) * f, None: () => current);

    // A None tint leaves the slab color at its fresh value; Some(tint) eases toward it through RgbSpectrum.Lerp (a convex
    // blend of two validated bands at f∈[0,1] stays in-band, so the lerp is total — no Fin, no AllFinite re-check).
    static RgbSpectrum LerpColor(RgbSpectrum current, Option<RgbSpectrum> target, double f) =>
        target.Match(Some: tint => current.Lerp(tint, f), None: () => current);

    // The row-vector twin: the delta's band is already Scene-rebased and Pointer-grounded by SceneBand, so lifting it
    // back to Unicolour at the SAME scene-linear coordinates re-grounds nothing, and the mix takes the one working-
    // gamut projection the base colour takes. A None tint leaves the column untouched.
    static Unicolour LerpColorUni(Unicolour current, Option<RgbSpectrum> target, double f) =>
        target.Match(
            Some: tint => Mapped(current.Mix(
                new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, tint.R, tint.G, tint.B),
                ColourSpace.RgbLinear, f, premultiplyAlpha: false)),
            None: () => current);
}
```

## [03]-[RESEARCH]

- [AGING_CALIBRATION]-[BLOCKED]: which `Terminal` cap and which `WeatheringDose.Rate` exponent each `WeatheringEffect` binds so its `Weathering.Drift` CIEDE2000 trace matches a measured aging series (Cu→CuCO₃ chromaticity, Fe₂O₃ deepening); an atmospheric-exposure study reporting CIELAB per interval under a stated illuminant admits it, and `Wacton.Unicolour.Datasets` ships observers, illuminants, `ArtistPaint`, and `ColorChecker` alone, so the trace lands content-keyed under the `environment#SKY` Hosek-Wilkie precedent.
