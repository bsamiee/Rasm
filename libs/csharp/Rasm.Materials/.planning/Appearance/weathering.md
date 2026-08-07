# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` POLICY-ROW axis (patina · oxidation · soiling · uv-fade · biological · efflorescence · wetting · streaking) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters`, an `AgeParameter`, and the `SurfaceExposure` occlusion-and-curvature pair, and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar and the surface's own cavity evidence, never a `WeatheredCopper`/`SoiledFacade` type. An applied occurrence is one `WeatheringDose` — the row and its per-occurrence deposition exponent — whose `Eased(age, exposure)` curve is the ONE aging law BOTH paths read: the dose scales the raw age through its own row's `CavityResponse` and then eases the scaled result, so the flat row and the slab column age by the same number at the same shade point and CANNOT fork on exposure. An effect is ONE row carrying four data columns — the `Terminal(double f, WeatherEnvironment)` delegate (the terminal law: a `CorrosionSequence` of published mineral phases where the mechanism's chemistry is sourced, else a `Colourmap` read whose DIRECTION is row data or a constant named-colour bleach — never a second sampler), the `CavityResponse` exposure law, the optional SOURCED deposition exponent, and the ONE `SurfaceDelta` BOTH aging paths read. Every sampled terminal is fixed in a D65 working space — the sampled ramps and named colours in sRGB/D65, the measured corrosion phases in their own published D65 Lab; the fold rebases it to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `RgbLinear` mix — never a default-D65 sample mixed into an `Acescg` base as if the primaries matched, the same cross-space grounding `finish#FINISH` `FinishMix.Reflectance` performs — then grounds the rebased sample in the Pointer real-surface gamut through the kernel `GamutPolicy.Pointer` row's own `Contains`/`Bound` pair: verdigris, rust, grime, chalk, biofilm, and salt bloom are REAL surface reflectances, so a ramp chroma no physical corrosion product reaches projects to the nearest real colour before any mix. The flat `MaterialParameters` `Apply` interpolation is the row-vector path the `graph#MATERIAL_GRAPH` re-evaluates and the `press#TEXTURE_PRESS` age ladder quantizes; the `ApplySlab` path is the same trajectory over an already-lowered `surface#OPENPBR_SLAB` stack, the carrier the integrator holds after `SlabStack.Lower` — the press drives the vector path because no `SlabStack`→`OpenPbrSurface` inverse exists, and both paths derive their targets from the same `SurfaceDelta`, keyed by the `SurfaceColumn` roster DERIVED from the `OpenPbrSurface` vector's own constructor, so the two paths CANNOT diverge on a shared column and a widened vector needs no edit here — a second hand-mirrored flat column set is the deleted form. Patina greens the `Slab.Base` color and de-metalizes it (the conductor corrodes to a dielectric verdigris, NEVER a metal-to-metal `ConductorMetal` swap), oxidation roughens and rusts the base, soiling fouls transmission and tints the `Slab.Fuzz` grime weight and color, chalking lifts the `Slab.Coat` roughness and tints it — every aged column a convex `RgbSpectrum.Lerp`/scalar lerp of validated endpoints at `f∈[0,1]` (in-band by construction, so the slab path is TOTAL and carries no fault rail), and the flat `Apply` path's `Mix` overshoot is MAPPED back into the working gamut through `GamutPolicy.Perceptual`, never hard-faulted; a per-effect `CavityResponse` (crevice · exposed · uniform · convex · concave) scales the age by the ONE axis its row names, so crevice-accumulating effects (soiling, patina, biological) ride cavity depth, exposure-driven bleaching (uv-fade) rides its complement, and abrasion and crystalline deposition ride the signed curvature the second field carries. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters`/`MaterialParameters.Of`, the kernel `GamutPolicy` `Pointer` and `Perceptual` rows for the terminal grounding and the working-gamut projection, `surface#OPENPBR_SLAB` `SlabStack`/`Slab` for the slab columns, Wacton.Unicolour directly for the scene-linear `Mix` and the `ConvertToConfiguration` rebase, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps and `Css`/`Xkcd` named colours for the grime/chalk/biofilm/bleach tints (the hand-keyed hex literal is the deleted form), and the `MaterialFault` band-2450 rail solely through the composed `MaterialParameters.Of` egress re-admission.

## [01]-[INDEX]

- [02]-[WEATHERING]: `AgeParameter` bounds the age, `CavityResponse` axes the exposure, `SurfaceColumn` derives the aging-target roster and `SurfaceDelta` carries a mechanism's targets over it, `WeatheringDose` owns the one exposure-scaled eased age, `WeatheringEffect` tables the policy rows (terminal law · exposure law · sourced rate · surface delta), `Weathering.Apply` and `ApplySlab` fold the row vector and the slab columns, `Scene`/`SceneBand` ground every terminal and tint, `Drift` reads the aging magnitude, and `Ramp` samples the trajectory.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `SurfaceExposure` the shade point's occlusion-and-curvature evidence pair; `CavityResponse` `[SmartEnum<string>]` the per-effect exposure axis (crevice · exposed · uniform · convex · concave); `SurfaceColumn` the roster derived from the `OpenPbrSurface` vector and `SurfaceDelta` the ONE target set both paths read; `WeatherEnvironment` the terminal atmosphere; `EndpointEvidence` the published-measurement class and `CorrosionPhase`/`CorrosionSequence` the measured mineral phases the sourced mechanisms walk; `WeatheringDose` the per-occurrence (row, rate) pair owning the one `Eased(age, exposure)` curve; `WeatheringEffect` `[SmartEnum<string>]` the effect POLICY ROWS.
- Cases: {`Patina` (the cuprite→brochantite/atacamite compound sequence on the environment's own anion, de-metalize, base roughen, crevice), `Oxidation` (the lepidocrocite→goethite compound sequence, base roughen, uniform), `Soiling` (Mako reversed toward the near-black grime end, transmission fouled, grey fuzz grime, crevice), `UvFade` (constant `Css.WhiteSmoke` bleach terminal — hue-preserving desaturation toward pale, coat chalks, exposure-driven), `Biological` (Crest forward — the living film greens then darkens, green fuzz colonization, crevice/moisture-driven), `Efflorescence` (constant `Css.Linen` salt-bloom terminal — the crystalline deposit on masonry/CMU/mortar/concrete, pale fuzz veil, base roughened, crevice/moisture-driven)} — the closed axis spanning the chemical, particulate, photochemical, biotic, AND mineral facade-aging mechanisms; a new effect is ONE row, never an effect subtype and never a trajectory switch arm.
- Law: every ramp read stays `Colourmap.Map(f)` — `MapWithClipping` substitutes the `Colourmap.Black`/`White` CLIP colours rather than the ramp ends on an out-of-range fraction, and the eased fraction is in-range by construction, so the clipping overload injects a wrong terminal exactly where it fires.
- Law: BOTH folds read ONE eased age — `dose.Eased(age, exposure)` over the row's own `CavityResponse` — so a crevice effect ages identically on the flat row and the slab column, and the `graph#MATERIAL_GRAPH` path supplies its `Texture` cavity and curvature node values as the exposure ARGUMENT rather than as a second, silent multiplier. The `SurfaceExposure` pair is REQUIRED on both entries and can never be defaulted whole: there is no neutral OCCLUSION — `Crevice(a,1)=a` while `Exposed(a,1)=0` — so a defaulted pair would silently run one half of the roster at full age and the other at zero. Its CURVATURE lane alone has a neutral, and `SurfaceExposure.Flat` names it: at zero every curvature-keyed row scales at unity, so a consumer carrying no curvature field gets exactly the aging the cavity-only roster produced and the axis costs nothing to ignore.
- Law: `SurfaceExposure.Occlusion` is the CAVITY scalar — `1.0` the fully occluded crevice, `0.0` the open face — and its baked source is the `Raster/set#TEXTURE_SET` `occlusion` channel (`ChannelOrigin.Derived` over `Raster/filter#PLANE_OP` `HeightDerivative.Occlusion`), sampled through `texture#TEXTURE_UV`. That channel stores VISIBILITY: `Occlude` deposits `open/rays` and the row's own neutral is `1.0` unoccluded, so an occlusion plane crosses into a cavity field through the landed `filter#PLANE_OP` `RemapCurve.Levels.Invert` row and NEVER by binding raw AO — the raw bind is polarity-inverted and ages every crevice effect on the open face, which reads as a plausible render rather than as a fault.
- Law: the coat and fuzz TINT targets drive BOTH paths. `graph#MATERIAL_LIBRARY` `MaterialParameters` carries real `CoatColor`/`FuzzColor` columns and `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` sources the vector's coat and fuzz tints from them, so the flat `Age` fold lerps each toward its delta through the same check-then-map the base colour takes; the prior slab-only carve — a flat row carrying no coat/fuzz colour source — is deleted with the premise that held it, and a chalked coat now reads the same tint on the vector the integrator lowers and on the plane the press bakes.
- Law: CURVATURE IS THE SECOND EXPOSURE AXIS and it is INDEPENDENT of the first. On a cavity scalar alone `Convex` reduces to `age·(1−occ)`, byte-identical to `Exposed`, because convexity and openness are one number until a second evidence field carries the signed `Raster/set#TEXTURE_SET` `curvature` measure; that field now exists, so `Scale` reads the whole `SurfaceExposure` pair and `Convex`/`Concave` are distinguishable rows rather than name-only siblings. Each row reads the ONE axis it names — the curvature pair never consults occlusion and the cavity pair never consults curvature — which is what keeps the four spatial rows four decisions instead of one axis wearing two labels, and a crevice on a proud arris is representable because the two lanes disagree there.
- Law: the trajectory's PLANE form composes at the raster stratum, never here. `Ramp` returns the effect's own scene-linear texel run and a consumer lifts it in one call — `texture#TEXTURE_UV` `TextureSource.Image.Of(width, height: 1, ramp.Map(static b => new ShadeVec4(b.R, b.G, b.B, 1.0)), key)` — so an aging trajectory samples as a one-dimensional palette through the estate's own sampler with zero new surface. `Appearance` core consumes no `Raster` type, so a `Fin<TexturePlane>` ramp mint here would invert the folder strata; and `filter#PLANE_OP` `RemapCurve.Lut` is NOT its applying owner — that case carries a TinyEXR `Lut3D` parsed from `.cube` SOURCE TEXT with no lattice-construction seam and a `TableKey` minted from those bytes, so a generated ramp has no legal `Lut` spelling.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure, Op key)` ages the flat row vector as a PURE per-dose fold — a `Mix` that overshoots the working gamut is checked then projected back through `GamutPolicy.Perceptual` (the check-then-bound law), so the ONE fallible point is the composed `MaterialParameters.Of` egress re-admission and `Fin<T>` aborts solely on a genuinely degenerate column; `public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure)` ages the lowered `surface#OPENPBR_SLAB` slab columns and is TOTAL — every aged column is a convex lerp of a validated endpoint toward a validated terminal at `f∈[0,1]`, in-band by construction, so no fault can arise and a `Fin<SlabStack>` would fabricate a rail the inputs cannot trip. Both fold the dose `Seq` left-to-right by the identical call `dose.Eased(age.Value, exposure)`; arity is one — a multi-effect aging is the fold, never a per-effect method. The two entries differ in CARRIER alone: `Apply` produces the row vector `press#TEXTURE_PRESS` quantizes into its age ladder and `graph#MATERIAL_GRAPH` re-evaluates, `ApplySlab` produces the already-lowered stack the integrator holds after `SlabStack.Lower`, and the press drives the vector path because no `SlabStack`→`OpenPbrSurface` inverse exists on the surface page.
- Packages: `surface#OPENPBR_SLAB` (composed — the `SlabStack`/`Slab` columns the slab aging drives), `graph#MATERIAL_LIBRARY` (composed — `MaterialParameters`/`MaterialParameters.Of` with its `CoatColor`/`FuzzColor` columns and `PortValue.SceneLinear`), Wacton.Unicolour (composed — scene-linear `Mix` toward each rebased terminal, `ConvertToConfiguration` rebasing the sRGB/D65 dataset sample into the `Acescg` scene-linear space, and `Difference(DeltaE.Ciede2000)` the `Drift` calibration metric), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramps the terminal delegates sample, and the `Css`/`Xkcd` named-colour tints `Xkcd.Charcoal`/`Css.WhiteSmoke`/`Xkcd.DarkForestGreen` resolved per the datasets named-colour law), Rasm (project — `UnitInterval`, and the `GamutPolicy` `Perceptual` working-gamut and `Pointer` real-surface rows the mix and the terminal grounding check and bound through), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` ROW carrying its terminal delegate, `CavityResponse`, its optional sourced rate, and its `SurfaceDelta` — zero dispatch edits, never a per-effect material or a second appearance owner; a new aging-curve shape is the dose's `Rate` exponent, the eased `scaled^rate` the one curve every row reads; a new aging target is one `.To(nameof(OpenPbrSurface.X), …)` on the row — the roster derives from the vector, so a column added to `OpenPbrSurface` is targetable with ZERO edits on this page; a new exposure law is one `CavityResponse` row carrying its `Scale(age, exposure)` delegate, admitted only where the axis it reads genuinely discriminates — a new axis is one `SurfaceExposure` lane rather than a second pair; a new terminal is row data — a forward/reversed/capped `Colourmap` read or a `Css`/`Xkcd`/`Nord` named-colour constant through the same `Scene` rebase+grounding law, never a hand-keyed `RgbSpectrum` literal. Aging is an AUTHORED operator over the `surface#OPENPBR_SLAB` vector and the `graph#MATERIAL_LIBRARY` row — MaterialX 1.39 carries no standard aging node, so the effect set is grounded in the measured ramps and the slab projection, not a node-category parity target.
- Boundary: `Weathering.Apply` is the ONE flat aging operator — an aged-material type is the deleted form. The terminal law is the row's `Terminal(double f, WeatherEnvironment)` delegate, and it answers from THREE arms by what the mechanism's evidence admits: a chemically sourced row walks its own `CorrosionSequence` of published mineral phases, a row with no compound chemistry samples a perceptual ramp, and a row whose product no ramp traverses mixes toward a constant named colour. The two SOURCED rows are patina and oxidation — copper runs cuprite to atacamite in marine chloride and to brochantite in temperate sulfate, steel runs lepidocrocite to goethite in both — so the compound is the colour and the ramp is what a mechanism falls back to, never what a chemistry approximates; `Flare` is consequently sampled by NO row, and a ramp kept alive for a mechanism whose minerals are published would be the deleted form. Ramp DIRECTION stays a LUT fact, not a naming convention — `Crest` runs light green → teal → dark navy, `Mako` near-black → slate → pale mint — so soiling samples Mako REVERSED (grime darkening toward near-black), biological samples Crest FORWARD (the film greens then darkens), uv-fade mixes toward the CONSTANT pale `Css.WhiteSmoke` bleach (chromophore loss is hue-preserving desaturation toward pale — no shipped ramp bleaches, so the constant arm of the same delegate column is the honest terminal), and efflorescence mixes toward the CONSTANT `Css.Linen` salt-white (no perceptual ramp traverses toward a crystalline salt deposit — the same constant arm); a forward `Map(f)` asserted over an unread LUT direction was the deleted form. Every sampled terminal crosses the one `Scene` boundary — `ConvertToConfiguration(PortValue.SceneLinear)` rebase preserving the device-independent XYZ, then Pointer real-surface grounding (`GamutPolicy.Pointer`, check-then-bound) — BEFORE the `Mix(rebased, ColourSpace.RgbLinear, f, premultiplyAlpha: false)`, while the MIXED row is never Pointer-forced (a fresh conductor F0 is not a diffuse reflectance, and a near-zero age must not snap the row onto the Pointer boundary); a mixed color that overshoots the working gamut is checked then projected back through `GamutPolicy.Perceptual` — the perceptual chroma-reduction map, never an RGB clamp and never a hard fault, because an epsilon overshoot at a ramp extreme (the D65→D60 adaptation edge of a rebased sRGB sample) is a mappable pipeline fact, not a domain error; emission is NOT aged — weathering shifts a surface's reflectance, not its self-emission, so the fold leaves `Emission`/`EmissionLuminance` untouched (a luminous sign does not green with a copper roof), and a future thermochromic/phosphorescent decay is one delta column, never an emission lerp smuggled onto the reflectance terminal. The flat columns read the one `SurfaceDelta` by `OpenPbrSurface` column NAME — the row's `Sheen` from the fuzz weight, its `ClearcoatRoughness` from the coat roughness — so a chalked finish reads the SAME raised and tinted coat and a soiled row the SAME fouled transmission and grime tint on both paths BY CONSTRUCTION (the prior duplicated flat column set had already drifted from its slab mirror on three rows and is deleted). `ApplySlab` drives every weathered slab column by the eased fraction BEFORE the `ToLayered` collapse: the `Slab.Base` color lerps toward the rebased terminal (the slab path greens the copper exactly as the flat path does), its `Metalness` drops toward its own target (patina/oxidation corrode the conductor to a dielectric corrosion product — verdigris/rust are dielectrics, so the aging DE-METALIZES the base rather than swapping one `ConductorMetal` for another the 8-member smart-enum cannot represent), its `Roughness`/`Transmission` shift, the `Slab.Coat` `Roughness` rises (chalking) and its `Color` tints toward the coat tint, and the `Slab.Fuzz` `Weight`/`Color` rise toward the grime — each `None` column leaving its slab value untouched (a typed absence, never an in-band `-1.0` sentinel a `[0,1]` column cannot otherwise carry); every aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of two validated in-band endpoints at `f∈[0,1]`, so the slab aging is TOTAL and returns a bare `SlabStack`; the `SurfaceExposure` pair scales the uniform age per shade point through each row's `CavityResponse` so spatially-varying weathering (grime in crevices, sun-bleached exposed faces, algae in the damp shaded joint, scour on a proud arris, salt crust in a hollow) rides the existing fold — `Crevice` (soiling, patina, biological) at `age·occ`, `Exposed` (uv-fade) at `age·(1−occ)`, `Uniform` (bulk oxidation) at `age`, `Convex` at `age·(1+min(0,κ))` and `Concave` at `age·(1−max(0,κ))` — never a second aging surface. The two curvature rows return the RAW age at `κ=0`, so the flat default and an absent curvature field are the same aging the cavity-only roster produced, and each row consults the ONE axis it names rather than a blend of both.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;             // FrozenDictionary — the definition-time SurfaceColumn roster
using System.Linq;                           // the definition-time constructor-parameter derivation
using System.Reflection;                     // the OpenPbrSurface primary-constructor read the aging roster derives from
using LanguageExt;                           // Fin, Seq, Option, HashMap (the rail TYPES; the static Prelude below carries Some/None/toSeq)
using Rasm.Domain;                           // Op (the fault-correlation key the one MaterialParameters.Of egress re-admission rails on)
using Rasm.Materials.Appearance.Bsdf;        // RgbSpectrum (MaterialFault rails only inside the composed MaterialParameters.Of)
using Rasm.Materials.Appearance.Graph;       // MaterialParameters, ThinFilm, PortValue (PortValue.SceneLinear is the Acescg working space) — declared in the .Graph child namespace, not auto-imported by the parent
using Rasm.Materials.Appearance.Surface;     // Slab, SlabStack, OpenPbrSurface (the vector whose own roster the aging targets derive from)
using Rasm.Numerics;                         // UnitInterval, GamutPolicy (the kernel Pointer real-surface row this fold checks and bounds through)
using Thinktecture;                          // [SmartEnum]/[ValueObject], [UseDelegateFromConstructor], ComparerAccessors, ValidationError
using Wacton.Unicolour;                      // Unicolour, ColourSpace, DeltaE, ConvertToConfiguration, Configuration/RgbConfiguration/XyzConfiguration (the D65 Lab space the measured corrosion phases enter under)
using Wacton.Unicolour.Datasets;             // Colourmaps, Css, Xkcd
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;          // folder-root, beside finish#FINISH and acquisition#ACQUISITION; MaterialParameters is the .Graph child the prelude imports

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

// The atmosphere a mechanism runs in. It is a TERMINAL axis rather than a rate knob: a marine chloride load drives
// copper to atacamite where a temperate sulfate load drives it to brochantite, so the two converge on different
// COMPOUNDS and no single terminal describes both. Temperate is the default a caller who states nothing gets, and
// a mechanism whose chemistry does not vary on the axis says so by ignoring the parameter.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeatherEnvironment {
    public static readonly WeatherEnvironment Temperate = new("temperate");
    public static readonly WeatherEnvironment Marine = new("marine");
}

// EndpointEvidence states HOW MUCH published measurement stands behind a compound's colour, and it is a real
// consumer fact rather than a label: a single-source endpoint measured on one artefact carries the matrix of that
// artefact, so a reader weighting a trajectory knows which end is corroborated and which is one study.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EndpointEvidence {
    // The phase is independently colour-measured more than once.
    public static readonly EndpointEvidence Corroborated = new("corroborated");
    // One published measurement stands behind the value, and it carries its own specimen's matrix with it.
    public static readonly EndpointEvidence Single = new("single-source");
}

// A corrosion phase is its measured colour and the evidence class behind it, never a bare triple — the pair travels
// together so a trajectory cannot quietly weight a one-study endpoint as if it were corroborated.
public readonly record struct CorrosionPhase(Unicolour Colour, EndpointEvidence Evidence);

// A mechanism's chemistry is a SEQUENCE, never one endpoint: copper runs cuprite to its environment's own sulfate or
// chloride, steel runs lepidocrocite to goethite. `At` walks that sequence by the same fraction the outer mix blends
// base into terminal, so a young surface reads the early phase and a full-aged one the terminal — the trajectory the
// perceptual ramps only approximated, now the actual mineralogy. The walk is a scene-linear reflectance mix because
// two mineral phases coexisting on a surface combine by area, not by perceptual hue path.
public readonly record struct CorrosionSequence(CorrosionPhase Early, CorrosionPhase Terminal) {
    public Unicolour At(double f) =>
        Early.Colour.Mix(Terminal.Colour, ColourSpace.RgbLinear, f, premultiplyAlpha: false);
}

// The shade point's exposure evidence as ONE pair, because the two axes are INDEPENDENT and a response row reads
// whichever it names: a crevice can sit on a convex arris and a gutter can be both concave and open, so one scalar
// cannot answer both. Occlusion is the CAVITY scalar — 1.0 the fully occluded crevice, 0.0 the open face — and the
// Raster/set occlusion channel stores VISIBILITY, so a plane crosses into it through the filter#PLANE_OP
// RemapCurve.Levels.Invert row and a raw AO bind is polarity-inverted. Curvature is SIGNED on [-1,1], the
// Raster/set curvature channel's own declared range: -1 the deepest concavity, +1 the proudest arris, 0 flat.
// FLAT IS THE DEGRADATION EXTREME and every curvature-keyed row scales at UNITY there, so a consumer that never
// heard of the axis hands 0.0 and gets exactly the aging the cavity-only roster always produced.
public readonly record struct SurfaceExposure(UnitInterval Occlusion, double Curvature) {
    public static Fin<SurfaceExposure> Of(UnitInterval occlusion, double curvature, Op key) =>
        double.IsFinite(curvature) && curvature is >= -1.0 and <= 1.0
            ? Fin.Succ(new SurfaceExposure(occlusion, curvature))
            : MaterialFault.Parameter(key, $"<surface-exposure-curvature:{curvature:R}>");

    // Flat is the neutral the absent-field consumer hands down, named once here rather than spelled at each site.
    public static SurfaceExposure Flat(UnitInterval occlusion) => new(occlusion, 0.0);
}

// The per-effect exposure law mapping the shade point's SurfaceExposure to the effect's age multiplier. Each row
// reads the ONE axis it names and ignores the other, which is exactly what makes the four spatial rows distinct:
// crevice-accumulating effects (soiling deposit, patina pooling, biofilm) age with occlusion, exposure-driven
// bleaching ages with its complement, abrasion and deposition age with curvature, and a uniform effect (bulk
// oxidation) reads neither. The curvature pair returns UNITY at flat and falls to zero only as the surface bends
// AGAINST the row — so a curvature-keyed row on a flat or absent field ages at its raw age rather than vanishing,
// and the two rows separate precisely where the second field exists. The delegate rides the row so both folds read
// CavityResponse.Scale(age, exposure) by data, never a switch on an exposure enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityResponse {
    public static readonly CavityResponse Crevice = new("crevice", static (age, e) => age * e.Occlusion.Value);
    public static readonly CavityResponse Exposed = new("exposed", static (age, e) => age * (1.0 - e.Occlusion.Value));
    public static readonly CavityResponse Uniform = new("uniform", static (age, _) => age);
    // Abrasion, chalking wear, and rain-scour strip the PROUD surface: full on any convex or flat face, vanishing
    // into a hollow the abrading agent never reaches.
    public static readonly CavityResponse Convex = new("convex", static (age, e) => age * (1.0 + Math.Min(0.0, e.Curvature)));
    // Crystalline growth and particulate settling need a HOLLOW to hold them: full in any concave or flat region,
    // vanishing on an arris nothing rests on.
    public static readonly CavityResponse Concave = new("concave", static (age, e) => age * (1.0 - Math.Max(0.0, e.Curvature)));

    [UseDelegateFromConstructor]
    public partial double Scale(double age, SurfaceExposure exposure);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ONE aging-target column set BOTH paths read (was two hand-mirrored parallel sets that had already drifted): the
// slab fold drives the surface#OPENPBR_SLAB columns directly, and the flat fold derives its MaterialParameters targets
// through the OpenPbrSurface.Of column correspondence — Roughness↔BaseRoughness, Metalness↔BaseMetalness,
// Transmission↔BaseTransmission, Sheen↔FuzzWeight, ClearcoatRoughness↔CoatRoughness, CoatColor↔CoatColor,
// FuzzColor↔FuzzColor — so the two paths CANNOT diverge on a shared column. None leaves the column at its fresh value
// (a typed absence, never an in-band -1.0 sentinel a [0,1] column cannot carry).
// The column set is DERIVED FROM THE OpenPbrSurface ROSTER, not curated against it: every vector column an aging
// mechanism can move carries a target here, so a column added to that vector is an aging target BY CONSTRUCTION and
// a mechanism that needs it lands as one row rather than as a widening of this record after the fact. The prior
// seven-column set was a curated subset, and the columns it omitted were exactly the ones the effect roster could not
// then express — anodized film THINNING and pearl-mica DULLING both move the thin_film group, wetting moves
// subsurface and specular together, and none of the three had a column to move.
// SurfaceColumn is the aging-target vocabulary DERIVED from the OpenPbrSurface vector's own primary constructor at
// type init, never curated beside it. That derivation IS the acceptance: a column added to the surface becomes an
// aging target the moment it exists, with ZERO edits on this page — where the prior hand-mirrored record needed one
// field per column and had already fallen behind its own source on every axis no effect could then express.
// SYMBOLIC_REFERENCE keeps the coupling honest at the other end: a row names its target through
// `nameof(OpenPbrSurface.X)`, so a renamed or retired column breaks the row at COMPILE rather than silently
// addressing a slot that no longer exists.
public static class SurfaceColumn {
    // The widest constructor IS the positional vector; its parameter names are the roster. Ordinal rides along so a
    // consumer needing a stable order reads one derived source rather than re-deriving a second.
    static readonly FrozenDictionary<string, int> Slots =
        typeof(OpenPbrSurface).GetConstructors()
            .OrderByDescending(static ctor => ctor.GetParameters().Length)
            .First()
            .GetParameters()
            .Select(static (parameter, index) => (Name: parameter.Name!, Index: index))
            .ToFrozenDictionary(static row => row.Name, static row => row.Index, StringComparer.Ordinal);

    public static bool Declares(string column) => Slots.ContainsKey(column);
    public static Seq<string> All => toSeq(Slots.OrderBy(static slot => slot.Value).Select(static slot => slot.Key));
}

// A target is EITHER a scalar or a tint, and the case is the discriminant an applier reads — a single carrier
// holding both would make every read a shape test the union already performs.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnTarget {
    private ColumnTarget() { }
    public sealed record Scalar(double Value) : ColumnTarget;
    public sealed record Tint(RgbSpectrum Value) : ColumnTarget;
}

// SurfaceDelta carries a mechanism's targets keyed by the DERIVED roster. None is the algebra zero — every column
// absent — so a row states only what its own mechanism moves and a widened vector needs no edit at any existing row.
// Every key proves against OpenPbrSurface at SEAT, so a column that no longer exists fails the load with the key
// named rather than seating a target nothing will ever read.
public readonly record struct SurfaceDelta(HashMap<string, ColumnTarget> Targets) {
    public static readonly SurfaceDelta None = new(HashMap<string, ColumnTarget>());

    public SurfaceDelta To(string column, double target) => Seat(column, new ColumnTarget.Scalar(target));
    public SurfaceDelta To(string column, RgbSpectrum tint) => Seat(column, new ColumnTarget.Tint(tint));

    SurfaceDelta Seat(string column, ColumnTarget target) =>
        SurfaceColumn.Declares(column)
            ? this with { Targets = Targets.AddOrUpdate(column, target) }
            : throw new InvalidOperationException($"<weathering-unknown-surface-column:{column}>");

    // The two typed reads every applier takes. A column absent, or present under the other case, reads as absence —
    // so an applier asking for a scalar never receives a tint and the lerp stays total.
    public Option<double> Scalar(string column) =>
        Targets.Find(column).Bind(static target => target is ColumnTarget.Scalar row ? Some(row.Value) : None);
    public Option<RgbSpectrum> Tint(string column) =>
        Targets.Find(column).Bind(static target => target is ColumnTarget.Tint row ? Some(row.Value) : None);
}

// One applied occurrence of an effect: the policy row plus the per-occurrence deposition exponent. Eased is the ONE
// aging law both folds read — the row's own CavityResponse scales the raw age by the shade point's cavity scalar, then
// the sub-linear rate exponent eases the SCALED value, so the flat row and the slab column cannot fork on exposure and
// the press reaches the exposure law at vector grain. Both Scale arms are products of two [0,1] values, so the clamp
// stays a NaN guard rather than a range fix and the terminal sample is in-range by construction with no outer clamp.
// The rate floor is comparison-ordered, not Math.Max — Max propagates NaN, so a non-finite consumer Rate lands at the
// floor and the TOTAL claim on both folds holds over any raw dose.
public readonly record struct WeatheringDose(WeatheringEffect Effect, double Rate) {
    const double RateFloor = 0.1;
    const double UnsourcedRate = 0.5;

    // Of prefers the effect row's OWN sourced exponent over a caller's guess, so the two mechanisms carrying a
    // measured per-interval series age at their measured rate wherever a caller does not deliberately override —
    // and the unsourced rows fall to one NAMED default rather than to whatever number a call site happened to type.
    public static WeatheringDose Of(WeatheringEffect effect) => new(effect, effect.Rate.IfNone(UnsourcedRate));

    public double Eased(double age, SurfaceExposure exposure) =>
        Math.Pow(Math.Clamp(Effect.Cavity.Scale(age, exposure), 0.0, 1.0), Rate >= RateFloor ? Rate : RateFloor);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The closed effect axis as POLICY ROWS spanning the chemical (patina/oxidation), particulate (soiling), photochemical
// (uv-fade), biotic (biological), and mineral (efflorescence) facade-aging mechanisms: each row carries its terminal-sampling law as a delegate
// column — ramp DIRECTION, cap, and a constant bleach are row DATA, never a second sampler — plus its exposure law and
// the one SurfaceDelta. A new effect is one row with zero dispatch edits. Terminal directions are LUT facts: Crest
// runs light green→teal→dark navy and Mako near-black→slate→pale mint; the sourced rows walk minerals, not ramps.
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

    // The corrosion COMPOUNDS the sourced mechanisms run through, each a PUBLISHED colorimetric characterization of
    // the mineral phase. The endpoint is the CHEMISTRY rather than an extrapolated colour series, because a compound
    // has one measurable colour where a series extrapolated past its last observed interval asserts a trajectory
    // nobody measured. Two facts fix the values that land here:
    //   MEASURE THE SURFACE, NOT THE POWDER. A phase measured as a reagent powder and the same phase measured as a
    //   corrosion layer diverge by tens of delta-E, because the matrix differs — so every row takes the SURFACE
    //   measurement where one is published, a weathering terminal being a surface and never a jar of mineral.
    //   ONE ILLUMINANT OR THE COMPARISON IS FICTION. The published corpus is split between illuminant C (the
    //   monomineralic Munsell mineralogy) and D65 (the pigment and weathering-steel colorimetry); every value below
    //   is the D65/2-degree measurement, so the pair enters ONE Lab configuration and the Scene rebase adapts once
    //   rather than mixing two white points inside a single trajectory.
    static readonly Configuration Measured = new(RgbConfiguration.StandardRgb, XyzConfiguration.D65);
    static CorrosionPhase Phase(double l, double a, double b, EndpointEvidence evidence) =>
        new(new Unicolour(Measured, ColourSpace.Lab, l, a, b), evidence);

    // Copper: cuprite forms first in EVERY atmosphere and the anion of the terminal phase is what the environment
    // decides — chloride-rich marine air converges on atacamite, sulfate-rich urban and rural air on brochantite.
    // One terminal per effect was the wrong shape and the environment parameter is what fixes it.
    static readonly CorrosionPhase Cuprite = Phase(53.5, 10.95, 11.15, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Brochantite = Phase(58.9, -10.1, 1.1, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Atacamite = Phase(74.9, -18.5, 8.6, EndpointEvidence.Single);

    // Iron: lepidocrocite is the EARLY orange phase and converts to goethite, which is the stable endpoint in rural,
    // urban, industrial, AND marine atmospheres alike — so oxidation reads one terminal across both environments and
    // that is a real claim rather than a missing axis. Marine air adds akaganeite and enriches magnetite without
    // redirecting the endpoint; akaganeite carries NO endpoint here because its one published measurement disclaims
    // its own diagnosticity, and a colour its own source calls non-diagnostic is worse than a declared absence.
    // Naming lepidocrocite the terminal was the deleted form: it is the phase the sequence LEAVES.
    static readonly CorrosionPhase Lepidocrocite = Phase(33.2, 15.7, 19.7, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Goethite = Phase(59.3, 16.4, 43.8, EndpointEvidence.Corroborated);

    // Tenorite and antlerite appear in every phase-identification study and have NEVER been colour-measured as
    // separate phases, so neither takes a row: an absent compound is stated by having no endpoint, never by a
    // plausible triple standing in for one.

    // Patina: the sourced 1-4 year copper CIELAB series binds the rate, and the compound sequence binds the colour —
    // cuprite in both atmospheres, converging on atacamite in marine chloride and brochantite in temperate sulfate.
    public static readonly WeatheringEffect Patina = new("patina",
        cavity: CavityResponse.Crevice,
        rate: Some(0.62),
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.55)
            .To(nameof(OpenPbrSurface.BaseMetalness), 0.0),
        terminal: static (f, env) => new CorrosionSequence(
            Cuprite, env == WeatherEnvironment.Marine ? Atacamite : Brochantite).At(f));

    // Oxidation: the sourced 0-24 month steel CIELAB series binds the rate. Goethite is the terminal in both
    // admitted environments — a marine atmosphere accelerates the mechanism and adds phases rather than redirecting
    // its endpoint — so the row reads the environment and answers one sequence, a real claim not a missing axis.
    public static readonly WeatheringEffect Oxidation = new("oxidation",
        cavity: CavityResponse.Uniform,
        rate: Some(0.48),
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.80)
            .To(nameof(OpenPbrSurface.BaseMetalness), 0.2)
            .To(nameof(OpenPbrSurface.CoatRoughness), 0.85),
        terminal: static (f, _) => new CorrosionSequence(Lepidocrocite, Goethite).At(f));

    // Soiling: Mako REVERSED — the particulate deposit darkens through slate toward the near-black grime end; fouling
    // kills transmission and the grey grime rides the fuzz slot (charcoal, not the fresh fuzz tint).
    public static readonly WeatheringEffect Soiling = new("soiling",
        cavity: CavityResponse.Crevice,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseTransmission), 0.0)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.4)
            .To(nameof(OpenPbrSurface.FuzzColor), GrimeFuzz),
        terminal: static (f, _) => Colourmaps.Mako.Map(1.0 - f));

    // UvFade: the CONSTANT pale bleach terminal — chromophore loss is hue-preserving desaturation toward pale, not a
    // hue traverse, and no shipped ramp bleaches (Vlag's ends are saturated blue and red), so the constant arm of the
    // same delegate column is the honest terminal law; the coat chalks alongside.
    public static readonly WeatheringEffect UvFade = new("uv-fade",
        cavity: CavityResponse.Exposed,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.40)
            .To(nameof(OpenPbrSurface.CoatRoughness), 0.50)
            .To(nameof(OpenPbrSurface.CoatColor), ChalkCoat),
        terminal: static (_, _) => Css.WhiteSmoke);

    // Biological: Crest FORWARD — the living film (algae · lichen · moss) greens then darkens on shaded/damp faces;
    // the biotic mechanism distinct from the grey particulate Soiling and the OPPOSITE exposure law to UvFade (it
    // COLONIZES the protected crevice the sun bleaches). The film coats any substrate, leaving the conductor intact.
    public static readonly WeatheringEffect Biological = new("biological",
        cavity: CavityResponse.Crevice,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.75)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.5)
            .To(nameof(OpenPbrSurface.FuzzColor), BioFilm),
        terminal: static (f, _) => Colourmaps.Crest.Map(f));

    // Efflorescence: the MINERAL mechanism — dissolved salts wick to the surface of masonry/CMU/mortar/concrete and
    // crystallize as the pale bloom where moisture lingers (the damp crevice feeds it, so it rides Crevice like the
    // deposits it resembles); the constant Linen terminal is the salt-white no ramp traverses, the powdery veil rides
    // the fuzz slot and the crystalline crust roughens the base. The canonical masonry-facade aging the AEC families
    // demand — one row, zero dispatch edits.
    // Efflorescence rides Concave rather than Crevice: the salt crust needs a HOLLOW to crystallize in, and a
    // concavity is where the wicking moisture lingers — the curvature axis is what finally distinguishes that from
    // the merely-occluded face the deposit mechanisms share.
    public static readonly WeatheringEffect Efflorescence = new("efflorescence",
        cavity: CavityResponse.Concave,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.85)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.6)
            .To(nameof(OpenPbrSurface.FuzzColor), SaltBloom),
        terminal: static (_, _) => Css.Linen);

    // Wetting: the HYDRIC mechanism, and the only reversible one in the roster — a water film fills the surface
    // microrelief, which raises the effective index at the interface and destroys the diffuse scattering that made
    // the dry surface look pale. Every column it moves follows from that one physical fact rather than from a
    // measured series: specular roughness and DIFFUSE roughness both drop (the film is smoother than what it
    // covers), specular weight rises toward a full Fresnel interface, subsurface weight rises (light that no longer
    // scatters at the surface enters the body instead), and the base darkens toward its own terminal because a
    // wetted surface returns less of what enters it. The terminal is the row's own colour driven toward the dark
    // end of the grime ramp — a wet surface is its own colour deepened, never a new pigment — and the mechanism
    // rides Crevice because water pools where the geometry holds it.
    public static readonly WeatheringEffect Wetting = new("wetting",
        cavity: CavityResponse.Crevice,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.08)
            .To(nameof(OpenPbrSurface.BaseDiffuseRoughness), 0.0)
            .To(nameof(OpenPbrSurface.Subsurface), 0.35)
            .To(nameof(OpenPbrSurface.SpecularWeight), 1.0)
            .To(nameof(OpenPbrSurface.CoatRoughness), 0.08),
        terminal: static (f, _) => Colourmaps.Mako.Map(0.35 - (0.25 * f)));

    // Streaking: the RUNOFF mechanism — rainwater carries dissolved and particulate soiling down a facade and
    // redeposits it in vertical tracks below every sill, ledge, and joint. It is soiling's directional sibling and
    // NOT a second soiling row: the deposit chemistry is the same grime, the discriminant is that runoff concentrates
    // on the EXPOSED face the rain reaches rather than in the sheltered crevice, so the exposure law is the whole of
    // the difference and the roster carries both because a facade shows both at once. The spatial TRACK is the
    // consumer's own cavity field, exactly as every other row's distribution is — this row owns the colour and
    // roughness move, never a geometry.
    public static readonly WeatheringEffect Streaking = new("streaking",
        cavity: CavityResponse.Exposed,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.70)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.3)
            .To(nameof(OpenPbrSurface.FuzzColor), GrimeFuzz),
        terminal: static (f, _) => Colourmaps.Mako.Map(0.55 - (0.35 * f)));

    // Terminal reads the ENVIRONMENT because a mechanism's product is a function of the atmosphere it runs in, not
    // of the material alone — the marine-versus-temperate copper split is the standing proof that one terminal per
    // effect was the wrong shape. A row whose chemistry no source names ignores the parameter and answers its ramp,
    // which is a real statement rather than a defaulted knob: the axis exists, and that row does not vary on it.
    [UseDelegateFromConstructor]
    public partial Unicolour Terminal(double f, WeatherEnvironment environment);

    public CavityResponse Cavity { get; }

    // The SOURCED deposition exponent where a measured per-interval series binds one, typed absence otherwise. A
    // dose takes this in preference to a caller's guess, so the two mechanisms with a measured trace age at their
    // measured rate and the rest stay explicitly unsourced rather than sharing an invented default.
    public Option<double> Rate { get; }

    public SurfaceDelta Surface { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Weathering {
    // Aging over the flat MaterialParameters row vector — the carrier press#TEXTURE_PRESS quantizes into its age
    // ladder and graph#MATERIAL_GRAPH re-evaluates. The cavity scalar is REQUIRED: no neutral value exists, since a
    // Crevice row reads 1.0 as full age while an Exposed row reads it as none. The fold is PURE — a working-gamut
    // overshoot maps back per effect — so the ONE fallible point is the composed egress re-admission, the same
    // gamut/unit gate a registered row passes.
    public static Fin<MaterialParameters> Apply(
        MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure,
        WeatherEnvironment environment, Op key) =>
        MaterialParameters.Of(
            effects.Fold(baseRow, (row, dose) => Age(row, dose.Effect, dose.Eased(age.Value, exposure), environment)), key);

    // The same trajectory over an already-lowered surface#OPENPBR_SLAB SlabStack — the carrier the integrator holds
    // after SlabStack.Lower, driven BEFORE the ToLayered collapse. The dose supplies the IDENTICAL eased age the flat
    // fold reads, so the two carriers cannot diverge on a shared column. TOTAL: every aged column is a convex blend of
    // validated endpoints at f∈[0,1], so a Fin<SlabStack> here would model a non-finite the inputs cannot produce.
    public static SlabStack ApplySlab(
        SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure, WeatherEnvironment environment) =>
        effects.Fold(stack, (s, dose) => AgeSlab(s, dose.Effect, dose.Eased(age.Value, exposure), environment));

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
    public static Seq<RgbSpectrum> Ramp(WeatheringEffect effect, int count, WeatherEnvironment environment) =>
        Math.Max(2, count) switch {
            var n => toSeq(Enumerable.Range(0, n)).Map(i => SceneBand(effect.Terminal(i / (n - 1.0), environment))),
        };

    // Total per-effect flat aging: the targets read the one SurfaceDelta through the OpenPbrSurface.Of
    // correspondence (the row's Sheen lowers to the fuzz weight, ClearcoatRoughness to the coat roughness, and the
    // coat/fuzz tints land on the row's own colour columns), the mix runs in scene-linear RgbLinear toward the
    // Pointer-grounded terminal, and an overshooting result is checked then MAPPED (OklchChromaReduction —
    // perceptual, never an RGB clamp), so no per-effect fault exists for a projectable pipeline fact. Emission columns
    // are untouched — weathering shifts reflectance, never self-emission.
    static MaterialParameters Age(MaterialParameters row, WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        (effect.Surface, row.BaseColor.Mix(SceneTerminal(effect, f, environment), ColourSpace.RgbLinear, f, premultiplyAlpha: false)) switch {
            // Each target reads by its OpenPbrSurface column NAME, so this fold and the slab fold address one derived
            // roster and neither carries a column the other cannot answer. The correspondence to the row's own
            // spelling — Sheen from the fuzz weight, ClearcoatRoughness from the coat roughness — lives HERE, at the
            // one place the flat carrier differs from the vector, rather than being mirrored into a second record.
            var (d, mixed) => row with {
                BaseColor = Mapped(mixed),
                Roughness = LerpToward(row.Roughness, d.Scalar(nameof(OpenPbrSurface.BaseRoughness)), f),
                Metalness = LerpToward(row.Metalness, d.Scalar(nameof(OpenPbrSurface.BaseMetalness)), f),
                Transmission = LerpToward(row.Transmission, d.Scalar(nameof(OpenPbrSurface.BaseTransmission)), f),
                BaseDiffuseRoughness = LerpToward(row.BaseDiffuseRoughness, d.Scalar(nameof(OpenPbrSurface.BaseDiffuseRoughness)), f),
                Subsurface = LerpToward(row.Subsurface, d.Scalar(nameof(OpenPbrSurface.Subsurface)), f),
                SpecularColor = LerpColorUni(row.SpecularColor, d.Tint(nameof(OpenPbrSurface.SpecularColor)), f),
                Sheen = LerpToward(row.Sheen, d.Scalar(nameof(OpenPbrSurface.FuzzWeight)), f),
                Clearcoat = LerpToward(row.Clearcoat, d.Scalar(nameof(OpenPbrSurface.CoatWeight)), f),
                ClearcoatRoughness = LerpToward(row.ClearcoatRoughness, d.Scalar(nameof(OpenPbrSurface.CoatRoughness)), f),
                CoatColor = LerpColorUni(row.CoatColor, d.Tint(nameof(OpenPbrSurface.CoatColor)), f),
                FuzzColor = LerpColorUni(row.FuzzColor, d.Tint(nameof(OpenPbrSurface.FuzzColor)), f),
                Film = ThinFilm.Create(
                    LerpToward(row.Film.Weight, d.Scalar(nameof(OpenPbrSurface.ThinFilmWeight)), f),
                    LerpToward(row.Film.ThicknessNm, d.Scalar(nameof(OpenPbrSurface.ThinFilmThickness)), f),
                    row.Film.Ior) },
        };

    // The slab-column aging: the base COLOR ages toward the scene-linear terminal (the slab path greens the copper
    // exactly as the flat path does), metalness drops (the conductor corrodes to a dielectric, never a ConductorMetal
    // swap), roughness/transmission shift, and the coat/fuzz tint toward their chalk/grime targets.
    static SlabStack AgeSlab(SlabStack stack, WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        SceneBand(effect.Terminal(f, environment)) switch {
            var baseTerminal => new SlabStack(stack.Slabs.Map(slab => AgeSlabCase(slab, effect.Surface, baseTerminal, f))),
        };

    // The delta, the terminal, and the fraction thread as STATE so every arm stays static: this fold runs once per
    // slab per dose per shade point on the integrator's own path, and a capturing arm allocates a closure at each.
    // Each arm reads its targets by OpenPbrSurface column NAME — the slab is a DIFFERENT roster from the vector, so
    // the correspondence between the two is spelled at this one boundary rather than derived, and stating that is
    // what keeps the derived roster honest: it makes a new surface column an aging TARGET by construction, while a
    // new slab FIELD to drive it remains the surface page's own edit.
    static Slab AgeSlabCase(Slab slab, SurfaceDelta d, RgbSpectrum baseTerminal, double f) =>
        slab.Switch<(SurfaceDelta Delta, RgbSpectrum Terminal, double Fraction), Slab>(
            state:    (d, baseTerminal, f),
            fuzz:     static (s, fz) => fz with {
                Weight = LerpToward(fz.Weight, s.Delta.Scalar(nameof(OpenPbrSurface.FuzzWeight)), s.Fraction),
                Roughness = LerpToward(fz.Roughness, s.Delta.Scalar(nameof(OpenPbrSurface.FuzzRoughness)), s.Fraction),
                Color = LerpColor(fz.Color, s.Delta.Tint(nameof(OpenPbrSurface.FuzzColor)), s.Fraction) },
            coat:     static (s, c) => c with {
                Weight = LerpToward(c.Weight, s.Delta.Scalar(nameof(OpenPbrSurface.CoatWeight)), s.Fraction),
                Roughness = LerpToward(c.Roughness, s.Delta.Scalar(nameof(OpenPbrSurface.CoatRoughness)), s.Fraction),
                Color = LerpColor(c.Color, s.Delta.Tint(nameof(OpenPbrSurface.CoatColor)), s.Fraction),
                Film = AgeFilm(c.Film, s.Delta, s.Fraction) },
            emission: static (_, e) => e,
            @base:    static (s, b) => b with {
                BaseColor = b.BaseColor.Lerp(s.Terminal, s.Fraction),
                Metalness = LerpToward(b.Metalness, s.Delta.Scalar(nameof(OpenPbrSurface.BaseMetalness)), s.Fraction),
                Roughness = LerpToward(b.Roughness, s.Delta.Scalar(nameof(OpenPbrSurface.BaseRoughness)), s.Fraction),
                DiffuseRoughness = LerpToward(b.DiffuseRoughness, s.Delta.Scalar(nameof(OpenPbrSurface.BaseDiffuseRoughness)), s.Fraction),
                SpecularWeight = LerpToward(b.SpecularWeight, s.Delta.Scalar(nameof(OpenPbrSurface.SpecularWeight)), s.Fraction),
                SpecularTint = LerpColor(b.SpecularTint, s.Delta.Tint(nameof(OpenPbrSurface.SpecularColor)), s.Fraction),
                Subsurface = LerpToward(b.Subsurface, s.Delta.Scalar(nameof(OpenPbrSurface.Subsurface)), s.Fraction),
                Transmission = LerpToward(b.Transmission, s.Delta.Scalar(nameof(OpenPbrSurface.BaseTransmission)), s.Fraction) });

    // The interference film ages as a WHOLE carrier rather than as two loose columns, because ThinFilm.Create owns
    // its own admission: a thinning oxide and a dulling mica move weight and thickness together, and reconstructing
    // the carrier is what keeps a negative thickness or an out-of-unit weight unrepresentable mid-trajectory.
    static ThinFilm AgeFilm(ThinFilm film, SurfaceDelta d, double f) =>
        ThinFilm.Create(
            LerpToward(film.Weight, d.Scalar(nameof(OpenPbrSurface.ThinFilmWeight)), f),
            LerpToward(film.ThicknessNm, d.Scalar(nameof(OpenPbrSurface.ThinFilmThickness)), f),
            film.Ior);

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

    static Unicolour SceneTerminal(WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        Scene(effect.Terminal(f, environment));

    // The same law as a validated RgbSpectrum band for the slab columns and the named tints (the slab carries
    // RgbSpectrum, not Unicolour): rebase, ground, read RgbLinear, clamp non-negative for the carrier.
    internal static RgbSpectrum SceneBand(Unicolour raw) =>
        Scene(raw).RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };

    // The ONE working-gamut projection every mixed colour crosses: an epsilon overshoot at a ramp extreme is a
    // mappable pipeline fact, so the check-then-map runs once here rather than inline at each mixing column.
    static Unicolour Mapped(Unicolour mixed) =>
        GamutPolicy.Perceptual.Contains(mixed) ? mixed : GamutPolicy.Perceptual.Bound(mixed);

    // A None target leaves the column at its fresh value; Some(target) eases toward it by f — the typed-absence lerp.
    static double LerpToward(double current, Option<double> target, double f) => current + (target.IfNone(current) - current) * f;

    // A None tint leaves the slab color at its fresh value; Some(tint) eases toward it through RgbSpectrum.Lerp (a convex
    // blend of two validated bands at f∈[0,1] stays in-band, so the lerp is total — no Fin, no AllFinite re-check).
    static RgbSpectrum LerpColor(RgbSpectrum current, Option<RgbSpectrum> target, double f) =>
        current.Lerp(target.IfNone(current), f);

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

- (none)
