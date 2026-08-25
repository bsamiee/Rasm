# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` POLICY-ROW axis (patina · oxidation · soiling · uv-fade · biological · efflorescence · wetting · streaking) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters`, an `AgeParameter`, and the `SurfaceExposure` occlusion-and-curvature pair, and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar and the surface's own cavity evidence, never a `WeatheredCopper`/`SoiledFacade` type. An applied occurrence is one `WeatheringDose` — the row and its per-occurrence deposition exponent — whose `Eased(age, exposure)` curve is the ONE aging law BOTH paths read: the dose scales the raw age through its own row's `CavityResponse` and then eases the scaled result, so the flat row and the slab column age by the same number at the same shade point and CANNOT fork on exposure. An effect is ONE row carrying four data columns — the `Terminal(double f, WeatherEnvironment)` delegate (the terminal law: a `CorrosionSequence` of published mineral phases where the mechanism's chemistry is sourced, else a `Colourmap` read whose DIRECTION is row data or a constant named-colour bleach — never a second sampler), the `CavityResponse` exposure law, the optional SOURCED deposition exponent, and the ONE `SurfaceDelta` BOTH aging paths read. Every sampled terminal is fixed in a D65 working space — the sampled ramps and named colours in sRGB/D65, the measured corrosion phases in their own published D65 Lab; the fold rebases it to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `RgbLinear` mix — never a default-D65 sample mixed into an `Acescg` base as if the primaries matched, the same cross-space grounding `finish#FINISH` `FinishMix.Reflectance` performs — then grounds the rebased sample in the Pointer real-surface gamut through the kernel `GamutPolicy.Pointer` row's own `Contains`/`Bound` pair: verdigris, rust, grime, chalk, biofilm, and salt bloom are REAL surface reflectances, so a ramp chroma no physical corrosion product reaches projects to the nearest real colour before any mix. The flat `MaterialParameters` `Apply` interpolation is the row-vector path the `graph#MATERIAL_GRAPH` re-evaluates and the `press#TEXTURE_PRESS` age ladder quantizes; the `ApplySlab` path is the same trajectory over an already-lowered `surface#OPENPBR_SLAB` stack, the carrier the integrator holds after `SlabStack.Lower` — the press drives the vector path because no `SlabStack`→`OpenPbrSurface` inverse exists, and both paths derive their targets from the same `SurfaceDelta`, keyed by the `SurfaceColumn` roster DERIVED from the `OpenPbrSurface` vector's own member set — the positional constructor for ORDER, its remaining public instance properties after it — so the roster covers exactly what `nameof(OpenPbrSurface.X)` binds, an unrostered target key is unrepresentable rather than guarded at each seat, the two paths CANNOT diverge on a shared column, and a widened vector needs no edit here; a second hand-mirrored flat column set is the deleted form. Patina greens the `Slab.Base` color and de-metalizes it (the conductor corrodes to a dielectric verdigris, NEVER a metal-to-metal `ConductorMetal` swap), oxidation roughens and rusts the base, soiling fouls transmission and tints the `Slab.Fuzz` grime weight and color, chalking lifts the `Slab.Coat` roughness and tints it — every aged column a convex `RgbSpectrum.Lerp`/scalar lerp of validated endpoints at `f∈[0,1]` (in-band by construction, so the slab path is TOTAL and carries no fault rail), and the flat `Apply` path's `Mix` overshoot is MAPPED back into the working gamut through `GamutPolicy.Perceptual`, never hard-faulted; a per-effect `CavityResponse` (crevice · exposed · uniform · convex · concave) scales the age by the ONE axis its row names, so crevice-accumulating effects (soiling, patina, biological) ride cavity depth, exposure-driven bleaching (uv-fade) rides its complement, and abrasion and crystalline deposition ride the signed curvature the second field carries. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters`/`MaterialParameters.Of`, the kernel `GamutPolicy` `Pointer` and `Perceptual` rows for the terminal grounding and the working-gamut projection, `surface#OPENPBR_SLAB` `SlabStack`/`Slab` for the slab columns, Wacton.Unicolour directly for the scene-linear `Mix` and the `ConvertToConfiguration` rebase, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps and `Css`/`Xkcd` named colours for the grime/chalk/biofilm/bleach tints (the hand-keyed hex literal is the deleted form), and the `MaterialFault` band-2450 rail solely through the composed `MaterialParameters.Of` egress re-admission.

## [01]-[INDEX]

- [02]-[WEATHERING]: `AgeParameter` bounds the age, `CavityResponse` axes the exposure, `SurfaceColumn` derives the aging-target roster and `SurfaceDelta` carries a mechanism's targets over it, `WeatheringDose` owns the one exposure-scaled eased age, `WeatheringEffect` tables the policy rows (terminal law · exposure law · sourced rate · surface delta), `Weathering.Apply` and `ApplySlab` fold the row vector and the slab columns, `Scene`/`SceneBand` ground every terminal and tint, `Drift` reads the aging magnitude, and `Ramp` samples the trajectory.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `SurfaceExposure` the shade point's occlusion-and-curvature evidence pair; `CavityResponse` `[SmartEnum<string>]` the per-effect exposure axis (crevice · exposed · uniform · convex · concave); `SurfaceColumn` the roster derived from the `OpenPbrSurface` vector and `SurfaceDelta` the ONE target set both paths read; `WeatherEnvironment` the terminal atmosphere; `EndpointEvidence` the published-measurement class and `CorrosionPhase`/`CorrosionSequence` the measured mineral phases the sourced mechanisms walk; `WeatheringDose` the per-occurrence (row, rate) pair owning the one `Eased(age, exposure)` curve; `WeatheringEffect` `[SmartEnum<string>]` the effect POLICY ROWS.
- Cases: {`Patina` (the cuprite→brochantite/atacamite compound sequence on the environment's own anion, de-metalize, base roughen, crevice), `Oxidation` (the lepidocrocite→goethite compound sequence, base roughen, uniform), `Soiling` (Mako reversed toward the near-black grime end, transmission fouled, grey fuzz grime, crevice), `UvFade` (constant `Css.WhiteSmoke` bleach terminal — hue-preserving desaturation toward pale, coat chalks, exposure-driven), `Biological` (Crest forward — the living film greens then darkens, green fuzz colonization, crevice/moisture-driven), `Efflorescence` (constant `Css.Linen` salt-bloom terminal — the crystalline deposit on masonry/CMU/mortar/concrete, pale fuzz veil, base roughened, crevice/moisture-driven)} — the closed axis spanning the chemical, particulate, photochemical, biotic, AND mineral facade-aging mechanisms; a new effect is ONE row, never an effect subtype and never a trajectory switch arm.
- Law: `SurfaceDelta.Seat` is TOTAL and declares NO refusal. `nameof(OpenPbrSurface.X)` is the compile proof a column exists and `SurfaceColumn` derives its roster from that same member set, so the two authorities cannot disagree and the prior `throw` guarded a divergence that is now unrepresentable — an invalid target is refused at construction rather than at each seat. `SurfaceColumn.Declares` survives as the BOUNDARY ARM at the kernel `CapabilitySet.Admits(string)` grain: an untrusted column token (a decoded authoring document, a scripted delta) resolves against the vocabulary BEFORE it reaches `To`, so text no member names refuses at the edge and no interior path carries a string compare.
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

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using System.Reflection;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Surface;
using Rasm.Numerics;
using Thinktecture;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeatherEnvironment {
    public static readonly WeatherEnvironment Temperate = new("temperate");
    public static readonly WeatherEnvironment Marine = new("marine");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EndpointEvidence {
    public static readonly EndpointEvidence Corroborated = new("corroborated");
    public static readonly EndpointEvidence Single = new("single-source");
}

public readonly record struct CorrosionPhase(Unicolour Colour, EndpointEvidence Evidence);

public readonly record struct CorrosionSequence(CorrosionPhase Early, CorrosionPhase Terminal) {
    public Unicolour At(double f) =>
        Early.Colour.Mix(Terminal.Colour, ColourSpace.RgbLinear, f, premultiplyAlpha: false);
}

public readonly record struct SurfaceExposure(UnitInterval Occlusion, double Curvature) {
    public static Fin<SurfaceExposure> Of(UnitInterval occlusion, double curvature, Op key) =>
        double.IsFinite(curvature) && curvature is >= -1.0 and <= 1.0
            ? Fin.Succ(new SurfaceExposure(occlusion, curvature))
            : new MaterialFault.Parameter(key, $"<surface-exposure-curvature:{curvature:R}>");

    public static SurfaceExposure Flat(UnitInterval occlusion) => new(occlusion, 0.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityResponse {
    public static readonly CavityResponse Crevice = new("crevice", static (age, e) => age * e.Occlusion.Value);
    public static readonly CavityResponse Exposed = new("exposed", static (age, e) => age * (1.0 - e.Occlusion.Value));
    public static readonly CavityResponse Uniform = new("uniform", static (age, _) => age);
    public static readonly CavityResponse Convex = new("convex", static (age, e) => age * (1.0 + Math.Min(0.0, e.Curvature)));
    public static readonly CavityResponse Concave = new("concave", static (age, e) => age * (1.0 - Math.Max(0.0, e.Curvature)));

    [UseDelegateFromConstructor]
    public partial double Scale(double age, SurfaceExposure exposure);
}

// --- [MODELS] --------------------------------------------------------------------------
public static class SurfaceColumn {
    static readonly FrozenDictionary<string, int> Slots =
        typeof(OpenPbrSurface).GetConstructors()
            .OrderByDescending(static ctor => ctor.GetParameters().Length)
            .First()
            .GetParameters()
            .Select(static parameter => parameter.Name!)
            .Concat(typeof(OpenPbrSurface).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(static property => property.Name))
            .Distinct(StringComparer.Ordinal)
            .Select(static (name, index) => (Name: name, Index: index))
            .ToFrozenDictionary(static row => row.Name, static row => row.Index, StringComparer.Ordinal);

    public static bool Declares(string column) => Slots.ContainsKey(column);
    public static Seq<string> All => toSeq(Slots.OrderBy(static slot => slot.Value).Select(static slot => slot.Key));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnTarget {
    private ColumnTarget() { }
    public sealed record Scalar(double Value) : ColumnTarget;
    public sealed record Tint(RgbSpectrum Value) : ColumnTarget;
}

public readonly record struct SurfaceDelta(HashMap<string, ColumnTarget> Targets) {
    public static readonly SurfaceDelta None = new(HashMap<string, ColumnTarget>());

    public SurfaceDelta To(string column, double target) => Seat(column, new ColumnTarget.Scalar(target));
    public SurfaceDelta To(string column, RgbSpectrum tint) => Seat(column, new ColumnTarget.Tint(tint));

    SurfaceDelta Seat(string column, ColumnTarget target) =>
        this with { Targets = Targets.AddOrUpdate(column, target) };

    public Option<double> Scalar(string column) =>
        Targets.Find(column).Bind(static target => target is ColumnTarget.Scalar row ? Some(row.Value) : None);
    public Option<RgbSpectrum> Tint(string column) =>
        Targets.Find(column).Bind(static target => target is ColumnTarget.Tint row ? Some(row.Value) : None);
}

public readonly record struct WeatheringDose(WeatheringEffect Effect, double Rate) {
    const double RateFloor = 0.1;
    const double UnsourcedRate = 0.5;

    public static WeatheringDose Of(WeatheringEffect effect) => new(effect, effect.Rate.IfNone(UnsourcedRate));

    public double Eased(double age, SurfaceExposure exposure) =>
        Math.Pow(Math.Clamp(Effect.Cavity.Scale(age, exposure), 0.0, 1.0), Rate >= RateFloor ? Rate : RateFloor);
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeatheringEffect {
    static readonly RgbSpectrum GrimeFuzz = Weathering.SceneBand(Xkcd.Charcoal);
    static readonly RgbSpectrum ChalkCoat = Weathering.SceneBand(Css.WhiteSmoke);
    static readonly RgbSpectrum BioFilm   = Weathering.SceneBand(Xkcd.DarkForestGreen);
    static readonly RgbSpectrum SaltBloom = Weathering.SceneBand(Css.Linen);

    static readonly Configuration Measured = new(RgbConfiguration.StandardRgb, XyzConfiguration.D65);
    static CorrosionPhase Phase(double l, double a, double b, EndpointEvidence evidence) =>
        new(new Unicolour(Measured, ColourSpace.Lab, l, a, b), evidence);

    static readonly CorrosionPhase Cuprite = Phase(53.5, 10.95, 11.15, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Brochantite = Phase(58.9, -10.1, 1.1, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Atacamite = Phase(74.9, -18.5, 8.6, EndpointEvidence.Single);

    static readonly CorrosionPhase Lepidocrocite = Phase(33.2, 15.7, 19.7, EndpointEvidence.Corroborated);
    static readonly CorrosionPhase Goethite = Phase(59.3, 16.4, 43.8, EndpointEvidence.Corroborated);

    public static readonly WeatheringEffect Patina = new("patina",
        cavity: CavityResponse.Crevice,
        rate: Some(0.62),
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.55)
            .To(nameof(OpenPbrSurface.BaseMetalness), 0.0),
        terminal: static (f, env) => new CorrosionSequence(
            Cuprite, env == WeatherEnvironment.Marine ? Atacamite : Brochantite).At(f));

    public static readonly WeatheringEffect Oxidation = new("oxidation",
        cavity: CavityResponse.Uniform,
        rate: Some(0.48),
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.80)
            .To(nameof(OpenPbrSurface.BaseMetalness), 0.2)
            .To(nameof(OpenPbrSurface.CoatRoughness), 0.85),
        terminal: static (f, _) => new CorrosionSequence(Lepidocrocite, Goethite).At(f));

    public static readonly WeatheringEffect Soiling = new("soiling",
        cavity: CavityResponse.Crevice,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseTransmission), 0.0)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.4)
            .To(nameof(OpenPbrSurface.FuzzColor), GrimeFuzz),
        terminal: static (f, _) => Colourmaps.Mako.Map(1.0 - f));

    public static readonly WeatheringEffect UvFade = new("uv-fade",
        cavity: CavityResponse.Exposed,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.40)
            .To(nameof(OpenPbrSurface.CoatRoughness), 0.50)
            .To(nameof(OpenPbrSurface.CoatColor), ChalkCoat),
        terminal: static (_, _) => Css.WhiteSmoke);

    public static readonly WeatheringEffect Biological = new("biological",
        cavity: CavityResponse.Crevice,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.75)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.5)
            .To(nameof(OpenPbrSurface.FuzzColor), BioFilm),
        terminal: static (f, _) => Colourmaps.Crest.Map(f));

    public static readonly WeatheringEffect Efflorescence = new("efflorescence",
        cavity: CavityResponse.Concave,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.85)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.6)
            .To(nameof(OpenPbrSurface.FuzzColor), SaltBloom),
        terminal: static (_, _) => Css.Linen);

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

    public static readonly WeatheringEffect Streaking = new("streaking",
        cavity: CavityResponse.Exposed,
        rate: None,
        surface: SurfaceDelta.None
            .To(nameof(OpenPbrSurface.BaseRoughness), 0.70)
            .To(nameof(OpenPbrSurface.FuzzWeight), 0.3)
            .To(nameof(OpenPbrSurface.FuzzColor), GrimeFuzz),
        terminal: static (f, _) => Colourmaps.Mako.Map(0.55 - (0.35 * f)));

    [UseDelegateFromConstructor]
    public partial Unicolour Terminal(double f, WeatherEnvironment environment);

    public CavityResponse Cavity { get; }

    public Option<double> Rate { get; }

    public SurfaceDelta Surface { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Weathering {
    public static Fin<MaterialParameters> Apply(
        MaterialParameters baseRow, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure,
        WeatherEnvironment environment, Op key) =>
        MaterialParameters.Of(
            effects.Fold(baseRow, (row, dose) => Age(row, dose.Effect, dose.Eased(age.Value, exposure), environment)), key);

    public static SlabStack ApplySlab(
        SlabStack stack, Seq<WeatheringDose> effects, AgeParameter age, SurfaceExposure exposure, WeatherEnvironment environment) =>
        effects.Fold(stack, (s, dose) => AgeSlab(s, dose.Effect, dose.Eased(age.Value, exposure), environment));

    public static double Drift(MaterialParameters fresh, MaterialParameters aged) =>
        fresh.BaseColor.Difference(aged.BaseColor, DeltaE.Ciede2000);

    public static Seq<RgbSpectrum> Ramp(WeatheringEffect effect, int count, WeatherEnvironment environment) =>
        Math.Max(2, count) switch {
            var n => toSeq(Enumerable.Range(0, n)).Map(i => SceneBand(effect.Terminal(i / (n - 1.0), environment))),
        };

    static MaterialParameters Age(MaterialParameters row, WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        (effect.Surface, row.BaseColor.Mix(SceneTerminal(effect, f, environment), ColourSpace.RgbLinear, f, premultiplyAlpha: false)) switch {
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

    static SlabStack AgeSlab(SlabStack stack, WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        SceneBand(effect.Terminal(f, environment)) switch {
            var baseTerminal => new SlabStack(stack.Slabs.Map(slab => AgeSlabCase(slab, effect.Surface, baseTerminal, f))),
        };

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

    static ThinFilm AgeFilm(ThinFilm film, SurfaceDelta d, double f) =>
        ThinFilm.Create(
            LerpToward(film.Weight, d.Scalar(nameof(OpenPbrSurface.ThinFilmWeight)), f),
            LerpToward(film.ThicknessNm, d.Scalar(nameof(OpenPbrSurface.ThinFilmThickness)), f),
            film.Ior);

    static Unicolour Scene(Unicolour raw) =>
        raw.ConvertToConfiguration(PortValue.SceneLinear) switch {
            var scene => GamutPolicy.Pointer.Contains(scene) ? scene : GamutPolicy.Pointer.Bound(scene),
        };

    static Unicolour SceneTerminal(WeatheringEffect effect, double f, WeatherEnvironment environment) =>
        Scene(effect.Terminal(f, environment));

    internal static RgbSpectrum SceneBand(Unicolour raw) =>
        Scene(raw).RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };

    static Unicolour Mapped(Unicolour mixed) =>
        GamutPolicy.Perceptual.Contains(mixed) ? mixed : GamutPolicy.Perceptual.Bound(mixed);

    static double LerpToward(double current, Option<double> target, double f) => current + (target.IfNone(current) - current) * f;

    static RgbSpectrum LerpColor(RgbSpectrum current, Option<RgbSpectrum> target, double f) =>
        current.Lerp(target.IfNone(current), f);

    static Unicolour LerpColorUni(Unicolour current, Option<RgbSpectrum> target, double f) =>
        target.Match(
            Some: tint => Mapped(current.Mix(
                new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, tint.R, tint.G, tint.B),
                ColourSpace.RgbLinear, f, premultiplyAlpha: false)),
            None: () => current);
}
```

## [03]-[RESEARCH]

(none)
