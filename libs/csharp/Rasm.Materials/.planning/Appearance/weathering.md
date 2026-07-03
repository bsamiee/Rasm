# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` POLICY-ROW axis (patina · oxidation · soiling · uv-fade · biological · efflorescence) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters` and an `AgeParameter` and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar, never a `WeatheredCopper`/`SoiledFacade` type. An applied occurrence is one `WeatheringDose` (the row plus its per-occurrence `age^rate` deposition exponent); an effect is ONE row carrying three data columns — the `Terminal(double f)` delegate (the terminal-sampling law: a `Wacton.Unicolour.Datasets` `Colourmap` read whose DIRECTION and cap are row data, or a constant named-colour bleach — never a second sampler), the `CavityResponse` exposure law, and the ONE `SlabColumnDelta` BOTH aging paths read. Every sampled terminal is fixed in its dataset's own sRGB/D65 working space; the fold rebases it to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `RgbLinear` mix — never a default-D65 sample mixed into an `Acescg` base as if the primaries matched, the same cross-space grounding `finish#FINISH` `FinishMix.Reflectance` performs — then grounds the rebased sample in the Pointer real-surface gamut (`IsInPointerGamut` → `graph#MATERIAL_LIBRARY` `MaterialLibrary.MapToPointer`): verdigris, rust, grime, chalk, biofilm, and salt bloom are REAL surface reflectances, so a ramp chroma no physical corrosion product reaches projects to the nearest real colour before any mix. The flat `MaterialParameters` `Apply` interpolation is the uniform row-level path the `graph#MATERIAL_GRAPH` re-evaluates; the `ApplySlab` slab path is the spatially-varying realized path the renderer shades — and the flat path DERIVES its scalar targets from the same `SlabColumnDelta` through the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` column correspondence (`Roughness`↔`BaseRoughness`, `Metalness`↔`BaseMetalness`, `Transmission`↔`BaseTransmission`, `Sheen`↔`FuzzWeight`, `ClearcoatRoughness`↔`CoatRoughness`), so the two paths CANNOT diverge on a shared column — a second hand-mirrored flat column set is the deleted form. Patina greens the `Slab.Base` color and de-metalizes it (the conductor corrodes to a dielectric verdigris, NEVER a metal-to-metal `ConductorMetal` swap), oxidation roughens and rusts the base, soiling fouls transmission and tints the `Slab.Fuzz` grime weight and color, chalking lifts the `Slab.Coat` roughness — every aged column a convex `RgbSpectrum.Lerp`/scalar lerp of validated endpoints at `f∈[0,1]` (in-band by construction, so the slab path is TOTAL and carries no fault rail), and the flat `Apply` path's `Mix` overshoot is MAPPED back into the working gamut through `MapToRgbGamut(GamutMap.OklchChromaReduction)`, never hard-faulted; a per-effect `CavityResponse` (crevice · exposed · uniform) scales the age by the consumer's ambient-occlusion scalar so crevice-accumulating effects (soiling, patina, biological) ride occlusion while exposure-driven bleaching (uv-fade) rides its complement. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters`/`MaterialParameters.Of` and `MaterialLibrary.MapToPointer`, `surface#OPENPBR_SLAB` `SlabStack`/`Slab` for the slab columns, Wacton.Unicolour directly for the scene-linear `Mix`, the `ConvertToConfiguration` rebase, and the `MapToRgbGamut` working-gamut projection, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps and `Css`/`Xkcd` named colours for the grime/chalk/biofilm/bleach tints (the hand-keyed hex literal is the deleted form), and the `MaterialFault` band-2450 rail solely through the composed `MaterialParameters.Of` egress re-admission.

## [01]-[INDEX]

- [01]-[WEATHERING]: the `AgeParameter` value-object, the `CavityResponse` exposure axis, the `SlabColumnDelta` shared aging-target columns, the `WeatheringDose` occurrence row, the `WeatheringEffect` policy-row table (terminal law · exposure law · slab delta per row), the `Weathering.Apply` flat-row + `ApplySlab` slab-column aging folds, the `Scene`/`SceneBand` rebase+Pointer-grounding law every terminal and tint crosses, the `Drift` CIEDE2000 aging-magnitude read, and the `Ramp` pre-baked scene-linear LUT egress.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `CavityResponse` `[SmartEnum<string>]` the per-effect occlusion-vs-exposure axis (crevice · exposed · uniform); `SlabColumnDelta` the ONE aging-target column set both paths read; `WeatheringDose` the per-occurrence (row, rate) pair owning the eased `age^rate` curve; `WeatheringEffect` `[SmartEnum<string>]` the effect POLICY ROWS.
- Rows: {`Patina` (Crest reversed-capped toward the mid-ramp sea-green verdigris, de-metalize, base roughen, crevice), `Oxidation` (Flare forward — light copper rust bloom deepening to dark scale, base roughen, uniform), `Soiling` (Mako reversed toward the near-black grime end, transmission fouled, grey fuzz grime, crevice), `UvFade` (constant `Css.WhiteSmoke` bleach terminal — hue-preserving desaturation toward pale, coat chalks, exposure-driven), `Biological` (Crest forward — the living film greens then darkens, green fuzz colonization, crevice/moisture-driven), `Efflorescence` (constant `Css.Linen` salt-bloom terminal — the crystalline deposit on masonry/CMU/mortar/concrete, pale fuzz veil, base roughened, crevice/moisture-driven)} — the closed axis spanning the chemical, particulate, photochemical, biotic, AND mineral facade-aging mechanisms; a new effect is ONE row, never an effect subtype and never a trajectory switch arm.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, Op key)` ages the flat row uniformly (the row-level path `graph#MATERIAL_GRAPH` re-evaluates) as a PURE per-dose fold — a `Mix` that overshoots the working gamut is checked (`IsInRgbGamut`) then projected back through `MapToRgbGamut(GamutMap.OklchChromaReduction)` (the check-then-map law), so the ONE fallible point is the composed `MaterialParameters.Of` egress re-admission and `Fin<T>` aborts solely on a genuinely degenerate column; `public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion)` ages the lowered `surface#OPENPBR_SLAB` slab columns spatially and is TOTAL — every aged column is a convex lerp of a validated endpoint toward a validated terminal at `f∈[0,1]`, in-band by construction, so no fault can arise and a `Fin<SlabStack>` would fabricate a rail the inputs cannot trip. Both fold the dose `Seq` left-to-right by the eased fraction (`dose.Eased`); arity is one — a multi-effect aging is the fold, never a per-effect method. The flat `Apply` is uniform because the `graph#MATERIAL_GRAPH` re-evaluation applies it per shade point with the graph's own `Texture` AO already in the node fold; the slab `ApplySlab` carries the `cavityOcclusion` `[0,1]` scalar the consumer samples from its `texture#TEXTURE_UV` cavity/ambient-occlusion node and threads in, each row's `CavityResponse` mapping that one scalar to its own age multiplier (a crevice effect ages at `age·occlusion`, an exposed effect at `age·(1−occlusion)`).
- Packages: `surface#OPENPBR_SLAB` (composed — the `SlabStack`/`Slab` columns the slab aging drives), `graph#MATERIAL_LIBRARY` (composed — `MaterialParameters`/`MaterialParameters.Of`, `PortValue.SceneLinear`, and the `MaterialLibrary.MapToPointer` real-surface projection wrapper), Wacton.Unicolour (composed — scene-linear `Mix` toward each rebased terminal, `ConvertToConfiguration` rebasing the sRGB/D65 dataset sample into the `Acescg` scene-linear space, the `IsInRgbGamut` check + `MapToRgbGamut(GamutMap.OklchChromaReduction)` working-gamut projection, `IsInPointerGamut` gating the terminal grounding, and `Difference(DeltaE.Ciede2000)` the `Drift` calibration metric), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramps the terminal delegates sample, and the `Css`/`Xkcd` named-colour tints `Xkcd.Charcoal`/`Css.WhiteSmoke`/`Xkcd.DarkForestGreen` resolved per the datasets named-colour law), Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` ROW carrying its terminal delegate, `CavityResponse`, and `SlabColumnDelta` — zero dispatch edits, never a per-effect material or a second appearance owner; a new aging-curve shape is the dose's `Rate` exponent, the eased `age^rate` the one curve every row reads; a new slab-column target is one `Option`-typed column on `SlabColumnDelta` (the flat path inherits it through the `Of` correspondence); a new exposure law is one `CavityResponse` row carrying its `Scale(age, occlusion)` delegate; a new terminal is row data — a forward/reversed/capped `Colourmap` read or a `Css`/`Xkcd`/`Nord` named-colour constant through the same `Scene` rebase+grounding law, never a hand-keyed `RgbSpectrum` literal. Aging is an AUTHORED operator over the `surface#OPENPBR_SLAB` vector and the `graph#MATERIAL_LIBRARY` row — MaterialX 1.39 carries no standard aging node, so the effect set is grounded in the measured ramps and the slab projection, not a node-category parity target.
- Boundary: `Weathering.Apply` is the ONE flat aging operator — an aged-material type is the deleted form. The terminal law is the row's `Terminal(double f)` delegate: the ramp DIRECTION is a LUT fact, not a naming convention — `Crest` runs light green → teal → dark navy, `Flare` light copper-orange → maroon → dark purple-brown, `Mako` near-black → slate → pale mint — so patina samples Crest REVERSED and capped (`Map(1 − 0.75f)`: young tarnish reads the dark teal end, full age the mid-ramp sea-green verdigris; the pale yellow-green ramp head is no corrosion product), oxidation samples Flare FORWARD (rust bloom deepening to dark scale), soiling samples Mako REVERSED (grime darkening toward near-black), biological samples Crest FORWARD (the film greens then darkens), uv-fade mixes toward the CONSTANT pale `Css.WhiteSmoke` bleach (chromophore loss is hue-preserving desaturation toward pale — no shipped ramp bleaches, so the constant arm of the same delegate column is the honest terminal), and efflorescence mixes toward the CONSTANT `Css.Linen` salt-white (no perceptual ramp traverses toward a crystalline salt deposit — the same constant arm); a forward `Map(f)` asserted over an unread LUT direction was the deleted form. Every sampled terminal crosses the one `Scene` boundary — `ConvertToConfiguration(PortValue.SceneLinear)` rebase preserving the device-independent XYZ, then Pointer real-surface grounding (`IsInPointerGamut` → `MaterialLibrary.MapToPointer`, check-then-map) — BEFORE the `Mix(rebased, ColourSpace.RgbLinear, f, premultiplyAlpha: false)`, while the MIXED row is never Pointer-forced (a fresh conductor F0 is not a diffuse reflectance, and a near-zero age must not snap the row onto the Pointer boundary); a mixed color that overshoots the working gamut is checked (`IsInRgbGamut`) then projected back through `MapToRgbGamut(GamutMap.OklchChromaReduction)` — the perceptual chroma-reduction map, never an RGB clamp and never a hard fault, because an epsilon overshoot at a ramp extreme (the D65→D60 adaptation edge of a rebased sRGB sample) is a mappable pipeline fact, not a domain error; emission is NOT aged — weathering shifts a surface's reflectance, not its self-emission, so the fold leaves `Emission`/`EmissionLuminance` untouched (a luminous sign does not green with a copper roof), and a future thermochromic/phosphorescent decay is one delta column, never an emission lerp smuggled onto the reflectance terminal. The flat scalar columns DERIVE from the one `SlabColumnDelta` through the `OpenPbrSurface.Of` correspondence — `Roughness`←`BaseRoughnessTo`, `Metalness`←`BaseMetalnessTo`, `Transmission`←`BaseTransmissionTo`, `Sheen`←`FuzzWeightTo` (the row's sheen lowers to the fuzz weight), `ClearcoatRoughness`←`CoatRoughnessTo` — so a chalked finish reads the SAME raised coat roughness and a soiled row the SAME fouled transmission on both paths BY CONSTRUCTION (the prior duplicated flat column set had already drifted from its slab mirror on three rows and is deleted); `CoatColorTo`/`FuzzColorTo` stay slab-only, the named lossy edge of a flat row that carries no coat/fuzz colour source. `ApplySlab` drives every weathered slab column by the eased fraction BEFORE the `ToLayered` collapse: the `Slab.Base` color lerps toward the rebased terminal (the slab path greens the copper exactly as the flat path does), its `Metalness` drops toward `BaseMetalnessTo` (patina/oxidation corrode the conductor to a dielectric corrosion product — verdigris/rust are dielectrics, so the aging DE-METALIZES the base rather than swapping one `ConductorMetal` for another the 8-member smart-enum cannot represent), its `Roughness`/`Transmission` shift, the `Slab.Coat` `Roughness` rises (chalking) and its `Color` tints toward `CoatColorTo`, and the `Slab.Fuzz` `Weight`/`Color` rise toward the grime — each `None` column leaving its slab value untouched (a typed absence, never an in-band `-1.0` sentinel a `[0,1]` column cannot otherwise carry); every aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of two validated in-band endpoints at `f∈[0,1]`, so the slab aging is TOTAL and returns a bare `SlabStack`; the `cavityOcclusion` AO sample scales the uniform age per shade point through each row's `CavityResponse` so spatially-varying weathering (grime in crevices, sun-bleached exposed faces, algae in the damp shaded joint) rides the existing slab fold — `Crevice` (soiling, patina, biological) at `age·occlusion`, `Exposed` (uv-fade) at `age·(1−occlusion)`, `Uniform` (bulk oxidation) at `age` — never a second aging surface.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                           // Fin, Seq, Option (the rail TYPES; the static Prelude below carries Some/None/toSeq)
using Rasm.Domain;                           // Op (the fault-correlation key the one MaterialParameters.Of egress re-admission rails on)
using Rasm.Materials.Appearance.Bsdf;        // RgbSpectrum (MaterialFault rails only inside the composed MaterialParameters.Of)
using Rasm.Materials.Appearance.Graph;       // MaterialParameters, PortValue (PortValue.SceneLinear is the Acescg working space), MaterialLibrary (the MapToPointer real-surface wrapper) — declared in the .Graph child namespace, not auto-imported by the parent
using Rasm.Materials.Appearance.Surface;     // Slab, SlabStack
using Rasm.Vectors;                          // UnitInterval
using Thinktecture;                          // [SmartEnum]/[ValueObject], [UseDelegateFromConstructor], ComparerAccessors, ValidationError
using Wacton.Unicolour;                      // Unicolour, ColourSpace, GamutMap, DeltaE, ConvertToConfiguration
using Wacton.Unicolour.Datasets;             // Colourmaps, Css, Xkcd
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;          // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

// The per-effect exposure law mapping the consumer's [0,1] ambient-occlusion scalar to the effect's age multiplier:
// crevice-accumulating effects (soiling deposit, patina pooling, biofilm) age with occlusion, exposure-driven
// bleaching (uv-fade) ages with its complement, and a uniform effect (bulk oxidation) ignores it. The delegate rides
// the row so ApplySlab reads CavityResponse.Scale(age, occlusion) by data, never a switch on an exposure enum.
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
// Transmission↔BaseTransmission, Sheen↔FuzzWeight, ClearcoatRoughness↔CoatRoughness — so the two paths CANNOT diverge
// on a shared column. None leaves the column at its fresh value (a typed absence, never an in-band -1.0 sentinel a
// [0,1] column cannot carry); CoatColorTo/FuzzColorTo are slab-only, the flat row carrying no coat/fuzz colour source.
public readonly record struct SlabColumnDelta(
    Option<double> BaseRoughnessTo,
    Option<double> BaseMetalnessTo,
    Option<double> BaseTransmissionTo,
    Option<double> CoatRoughnessTo,
    Option<RgbSpectrum> CoatColorTo,
    Option<double> FuzzWeightTo,
    Option<RgbSpectrum> FuzzColorTo);

// One applied occurrence of an effect: the policy row plus the per-occurrence deposition exponent. Eased is the one
// sub-linear age^rate curve every row and both folds read — a clamped [0,1] base under the floored positive rate stays
// in [0,1], so the terminal sample is in-range by construction and no outer clamp exists. The rate floor is
// comparison-ordered, not Math.Max — Max propagates NaN, so a non-finite consumer Rate lands at the floor and the
// TOTAL claim on both folds holds over any raw dose.
public readonly record struct WeatheringDose(WeatheringEffect Effect, double Rate) {
    const double RateFloor = 0.1;
    public double Eased(double age) => Math.Pow(Math.Clamp(age, 0.0, 1.0), Rate >= RateFloor ? Rate : RateFloor);
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
    // Uniform aging over the flat MaterialParameters row (the row-level path graph#MATERIAL_GRAPH re-evaluates per
    // shade point, the graph's own Texture AO node already supplying spatial variation in the node fold). The fold is
    // PURE — a working-gamut overshoot maps back per effect — so the ONE fallible point is the composed egress
    // re-admission, the same gamut/unit gate a registered row passes.
    public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, Op key) =>
        MaterialParameters.Of(effects.Fold(baseRow, (row, dose) => Age(row, dose.Effect, dose.Eased(age.Value))), key);

    // Spatially-varying aging over the lowered surface#OPENPBR_SLAB SlabStack columns: a weathered row ages through the
    // SAME slab algebra a fresh row does, each row's SlabColumnDelta driving every weathered slab column BEFORE the
    // ToLayered collapse, each row's CavityResponse mapping the per-shade-point AO scalar to its own age multiplier.
    // TOTAL: every aged column is a convex blend of validated endpoints at f∈[0,1], so a Fin<SlabStack> here would
    // model a non-finite the inputs cannot produce. Both paths read one terminal law — the slab terminal crosses the
    // same Scene rebase+Pointer-grounding boundary the flat Mix terminal does.
    public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, UnitInterval cavityOcclusion) =>
        effects.Fold(stack, (s, dose) => AgeSlab(s, dose.Effect, dose.Eased(dose.Effect.Cavity.Scale(age.Value, cavityOcclusion.Value))));

    // The perceptual aging magnitude: the CIEDE2000 difference between the fresh and aged base colors — the metric
    // graph#MATERIAL_LIBRARY NearestChecker now takes as the caller's DeltaE policy value (Ciede2000 the pigment-row
    // default), so a trajectory calibrates against a measured aging series in the one appearance drift currency.
    public static double Drift(MaterialParameters fresh, MaterialParameters aged) =>
        fresh.BaseColor.Difference(aged.BaseColor, DeltaE.Ciede2000);

    // The pre-baked scene-linear aging LUT: an even N-sample of the ROW'S terminal law (direction, cap, and constant
    // respected — a raw Colourmap.Palette would ignore a reversed or constant row), every sample crossing the SAME
    // Scene rebase+grounding law the live folds read.
    public static Seq<RgbSpectrum> Ramp(WeatheringEffect effect, int count) =>
        Math.Max(2, count) switch {
            var n => toSeq(Enumerable.Range(0, n)).Map(i => SceneBand(effect.Terminal(i / (n - 1.0)))),
        };

    // Total per-effect flat aging: the scalar targets DERIVE from the one SlabColumnDelta through the OpenPbrSurface.Of
    // correspondence (the row's Sheen lowers to the fuzz weight, ClearcoatRoughness to the coat roughness), the mix
    // runs in scene-linear RgbLinear toward the Pointer-grounded terminal, and an overshooting result is checked then
    // MAPPED (OklchChromaReduction — perceptual, never an RGB clamp), so no per-effect fault exists for a projectable
    // pipeline fact. Emission columns are untouched — weathering shifts reflectance, never self-emission.
    static MaterialParameters Age(MaterialParameters row, WeatheringEffect effect, double f) =>
        (effect.Slab, row.BaseColor.Mix(SceneTerminal(effect, f), ColourSpace.RgbLinear, f, premultiplyAlpha: false)) switch {
            var (d, mixed) => row with {
                BaseColor = mixed.IsInRgbGamut ? mixed : mixed.MapToRgbGamut(GamutMap.OklchChromaReduction),
                Roughness = LerpToward(row.Roughness, d.BaseRoughnessTo, f),
                Metalness = LerpToward(row.Metalness, d.BaseMetalnessTo, f),
                Transmission = LerpToward(row.Transmission, d.BaseTransmissionTo, f),
                Sheen = LerpToward(row.Sheen, d.FuzzWeightTo, f),
                ClearcoatRoughness = LerpToward(row.ClearcoatRoughness, d.CoatRoughnessTo, f) },
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

    // The ONE authored-colour boundary every terminal, tint, and LUT sample crosses: rebase the raw dataset colour
    // (fixed in ITS OWN sRGB/D65 working space) onto the scene PortValue.SceneLinear (Acescg) pipeline — preserving the
    // device-independent XYZ, never a Rec.709-linear sample treated as AP1-linear (the finish#FINISH grounding law) —
    // then ground it in the Pointer real-surface gamut through the graph#MATERIAL_LIBRARY wrapper (check-then-map): a
    // weathering product is a REAL surface reflectance, so a ramp chroma no physical corrosion product reaches projects
    // to the nearest real colour before any mix.
    static Unicolour Scene(Unicolour raw) =>
        raw.ConvertToConfiguration(PortValue.SceneLinear) switch {
            var scene => scene.IsInPointerGamut ? scene : MaterialLibrary.MapToPointer(scene),
        };

    static Unicolour SceneTerminal(WeatheringEffect effect, double f) => Scene(effect.Terminal(f));

    // The same law as a validated RgbSpectrum band for the slab columns and the named tints (the slab carries
    // RgbSpectrum, not Unicolour): rebase, ground, read RgbLinear, clamp non-negative for the carrier.
    internal static RgbSpectrum SceneBand(Unicolour raw) =>
        Scene(raw).RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };

    // A None target leaves the column at its fresh value; Some(target) eases toward it by f — the typed-absence lerp.
    static double LerpToward(double current, Option<double> target, double f) => target.Match(Some: to => current + (to - current) * f, None: () => current);

    // A None tint leaves the slab color at its fresh value; Some(tint) eases toward it through RgbSpectrum.Lerp (a convex
    // blend of two validated bands at f∈[0,1] stays in-band, so the lerp is total — no Fin, no AllFinite re-check).
    static RgbSpectrum LerpColor(RgbSpectrum current, Option<RgbSpectrum> target, double f) =>
        target.Match(Some: tint => current.Lerp(tint, f), None: () => current);
}
```

## [03]-[RESEARCH]

- [WEATHERING_TRAJECTORY]: the per-effect terminals sample measured `Wacton.Unicolour.Datasets` `Colourmap` ramps through each row's `Terminal(f)` delegate, and the sampling law respects the ramp's ACTUAL LUT direction rather than asserting a hue over an unread table — `Crest` runs light green→teal→dark navy (patina samples it REVERSED and capped at the mid-ramp sea-green; biological samples it FORWARD toward the dark film), `Flare` runs light copper→maroon→dark purple-brown (oxidation FORWARD, the rust-deepening traverse), `Mako` runs near-black→slate→pale mint (soiling REVERSED toward the grime-black end), and no shipped ramp bleaches or salt-blooms, so uv-fade's terminal is the constant pale `Css.WhiteSmoke` arm and efflorescence's the constant `Css.Linen` arm of the same delegate column. Every sample is fixed in the dataset's own sRGB/D65 working space and crosses the one `Scene` boundary — `ConvertToConfiguration(PortValue.SceneLinear)` rebase, then Pointer real-surface grounding (`IsInPointerGamut` → `MaterialLibrary.MapToPointer`) — before the `RgbLinear` mix, with the sub-linear `age^rate` deposition curve owned by `WeatheringDose.Eased`. The ramp read stays `Map(f)` — `MapWithClipping`'s out-of-range policy substitutes the `Colourmap.Black`/`White` CLIP colours, not the ramp ends, and the eased fraction is in-range by construction, so the clipping overload would inject a wrong terminal exactly where it fired. The calibration currency is `Weathering.Drift` — the CIEDE2000 `Difference` between fresh and aged base colors, the same metric `graph#MATERIAL_LIBRARY` `NearestChecker` runs over `Macbeth.All` — so a row's terminal drift verifies against a measured aging series (Cu→CuCO₃ chromaticity, Fe₂O₃ deepening) in one perceptual unit; the probe is the per-row curve calibration, not a re-architecture of the fold.
- [OPENPBR_SLAB_AGING]: REALIZED — `Weathering.ApplySlab` ages the realized `surface#OPENPBR_SLAB` `SlabStack` columns directly: each `WeatheringEffect` ROW carries the ONE `SlabColumnDelta` the fold lerps by the eased fraction BEFORE the `ToLayered` collapse — the `Slab.Base` color toward the rebased terminal, its `Metalness` toward `BaseMetalnessTo` (patina/oxidation corrode the conductor to a dielectric verdigris/rust — both DIELECTRICS, so the aging de-metalizes the base rather than swapping one `ConductorMetal` for another the 8-member smart-enum cannot represent), its `Roughness`/`Transmission`, the `Slab.Coat` `Roughness` (chalking) and `Color` (a pale chalk bloom), and the `Slab.Fuzz` `Weight`/`Color` (crevice grime). The FLAT path reads the SAME delta through the `OpenPbrSurface.Of` column correspondence — the prior duplicated flat column set (`RoughnessTo`/`MetalnessTo`/`SheenTo`/`ClearcoatRoughnessTo`) had drifted from its slab mirror on three of five effects (soiling sheen 0.3 vs fuzz 0.4, biological 0.2 vs 0.5, soiling's fouled transmission absent flat) and is DELETED; divergence between the two aging paths is now unrepresentable rather than prose-promised. Each aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of validated in-band endpoints at `f∈[0,1]`, so the slab aging is TOTAL and returns a bare `SlabStack` — a `Fin<SlabStack>` was the illusory rail the rebuild removed. The slab terminal and the `GrimeFuzz`/`ChalkCoat`/`BioFilm` tints (Datasets named colours `Xkcd.Charcoal`/`Css.WhiteSmoke`/`Xkcd.DarkForestGreen`) cross the identical `SceneBand` rebase+Pointer-grounding boundary, so the flat and slab paths read one terminal law. The slab columns are the shared aging target `finish#FINISH` also drives (a finish chalks through the same `Slab.Coat` roughness lift).
- [PROCEDURAL_SOILING_MASK]: REALIZED — spatially-varying weathering drives the aging fraction from a `texture#TEXTURE_UV` cavity/ambient-occlusion sample the consumer evaluates upstream rather than a uniform age: `ApplySlab` takes a `UnitInterval cavityOcclusion` (the per-shade-point AO the consumer samples from its `Texture` graph node and threads in — this page reads the scalar, it never composes `TextureUv` sampling itself, the strata keeping the texture fold on the `graph#MATERIAL_GRAPH` node owner) and each row's `CavityResponse` maps that one scalar to its own age multiplier: `Crevice` (soiling, patina, biological) ages at `age·occlusion` so a fully-shadowed crevice (`occlusion=1`) accumulates grime at the full rate — the damp shaded crevice is exactly where algae colonizes — `Exposed` (uv-fade) ages at `age·(1−occlusion)` so the sun-struck face bleaches while the protected crevice does not, and `Uniform` (bulk oxidation) ages at `age` regardless. The per-effect exposure law closes the AO-to-age response in place — a single `age·occlusion` for all effects mis-models exposure-driven bleaching, which is strongest on the OPPOSITE (exposed) faces — riding the existing slab fold without a second aging surface. The remaining tuning is the per-effect AO-to-age gamma against measured soiling/bleaching series, not a re-architecture.
