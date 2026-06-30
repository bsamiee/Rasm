# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade · biological) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters` and an `AgeParameter` and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar, never a `WeatheredCopper`/`SoiledFacade` type. The aging is a parameterized operator over the existing `MaterialParameters` columns (base color, roughness, metalness, sheen) interpolated through Wacton.Unicolour scene-linear mixing toward each effect's terminal state, composed not re-minted; the terminal is a measured `Wacton.Unicolour.Datasets` `Colourmap` ramp the fold samples at the eased age fraction (`Terminal.Map(f)`) so every effect traverses a perceptually-uniform gradient rather than a single frozen endpoint. The `Colourmap` ramp is fixed in its OWN sRGB/D65 working space (`Colourmap.Config`); the fold rebases each sample to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `RgbLinear` mix — never a default-D65 sample mixed into an `Acescg` base as if the primaries matched, the same cross-space grounding `finish#FINISH` `FinishMix.Reflectance` performs. Each effect's `SlabColumnDelta` drives the realized `surface#OPENPBR_SLAB` `SlabStack` slab columns directly through `Weathering.ApplySlab` — patina greens the `Slab.Base` color and de-metalizes it (the conductor corrodes to a dielectric verdigris, NEVER a metal-to-metal `ConductorMetal` swap), oxidation roughens and reddens the base, soiling tints the `Slab.Fuzz` grime weight and color, chalking lifts the `Slab.Coat` roughness — so a weathered row ages through the SAME slab algebra a fresh row does, every aged column a convex `RgbSpectrum.Lerp` of two validated bands at `f∈[0,1]` (in-band by construction, so the slab path is TOTAL and carries no fault rail — the genuine gamut gate is the flat `Apply` path's `Mix` toward the colourmap terminal); a per-effect `CavityResponse` (crevice · exposed · uniform) scales the age by the consumer's ambient-occlusion scalar so crevice-accumulating effects (soiling, patina) ride occlusion while exposure-driven bleaching (uv-fade) rides its complement. The flat `MaterialParameters` `Apply` interpolation is the uniform row-level path the `graph#MATERIAL_GRAPH` re-evaluates; the `ApplySlab` slab path is the spatially-varying realized path the renderer shades. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters` for the parameter vector, `surface#OPENPBR_SLAB` `SlabStack`/`Slab`/`ConductorMetal` for the slab columns, Wacton.Unicolour directly for the scene-linear color interpolation and the `ConvertToConfiguration` rebase, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps, and the `MaterialFault` band-2450 rail for a non-finite or out-of-gamut aged column.

## [01]-[INDEX]

- [01]-[WEATHERING]: the `AgeParameter` value-object, the `CavityResponse` exposure axis, the `WeatheringEffect` `[Union]` effect family, the `WeatheringTrajectory` per-effect terminal/rate/`SlabColumnDelta` row, and the `Weathering.Apply` flat-row + `ApplySlab` slab-column aging folds, the cavity scalar scaling the age per shade point by each effect's exposure response.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `CavityResponse` `[SmartEnum<string>]` the per-effect occlusion-vs-exposure axis (crevice · exposed · uniform); `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade · biological); `WeatheringTrajectory` the per-effect terminal-state/rate row carrying a `SlabColumnDelta` for the `surface#OPENPBR_SLAB` columns; `SlabColumnDelta` the per-effect base/coat/fuzz/transmission slab projection.
- Cases: effect {`Patina` (verdigris-green terminal, de-metalize, base roughen, crevice-accumulating), `Oxidation` (rust/tarnish terminal, base roughen, uniform), `Soiling` (grey grime terminal, fuzz weight + color, crevice-accumulating), `UvFade` (desaturated terminal, base-color lerp + roughen, exposure-driven), `Biological` (green-black algal/lichen biofilm terminal, base roughen, green fuzz colonization, crevice/moisture-driven — the biotic mechanism distinct from the grey particulate `Soiling` and opposite the sun-driven `UvFade`)} — the closed weathering family spanning the inorganic, photochemical, AND biotic facade-aging mechanisms; an effect is a `WeatheringEffect` case carrying its trajectory, never an effect subtype.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key)` ages the flat row uniformly (the row-level path `graph#MATERIAL_GRAPH` re-evaluates) and is the FALLIBLE path — its `Mix` toward the colourmap terminal can leave RGB gamut (`IsInRgbGamut`) and `MaterialParameters.Of` re-admits the aged row against the gamut/unit/IOR gate, so `Fin<T>` aborts on an out-of-gamut aged color (`MaterialFault.Gamut`); `public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringEffect> effects, AgeParameter age, UnitInterval cavityOcclusion)` ages the lowered `surface#OPENPBR_SLAB` slab columns spatially and is TOTAL — every aged column is a convex `RgbSpectrum.Lerp`/scalar lerp of a validated band/scalar toward a validated terminal at `f∈[0,1]`, in-band by construction, so no fault can arise and a `Fin<SlabStack>` would fabricate a rail the inputs cannot trip. Both fold each effect's `WeatheringTrajectory` over the carrier by the eased age fraction (`age^rate`); arity is one — a multi-effect aging folds the effect `Seq` left-to-right, never a per-effect method. The flat `Apply` is uniform because the `graph#MATERIAL_GRAPH` re-evaluation applies it per shade point with the graph's own `Texture` AO already in the node fold; the slab `ApplySlab` carries the `cavityOcclusion` `[0,1]` scalar the consumer samples from its `texture#TEXTURE_UV` cavity/ambient-occlusion node and threads in, each effect's `CavityResponse` mapping that one scalar to its own age multiplier (a crevice effect ages at `age·occlusion`, an exposed effect at `age·(1−occlusion)`).
- Packages: `surface#OPENPBR_SLAB` (composed — the `SlabStack`/`Slab`/`ConductorMetal` columns the slab aging drives), Wacton.Unicolour (composed — scene-linear `Mix` toward each rebased terminal, `ConvertToConfiguration` rebasing the sRGB/D65 colourmap sample into the `Acescg` scene-linear space, and `IsInRgbGamut` for the aged-color gate), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramp the trajectory terminal samples by the eased age fraction), Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` case carrying its `WeatheringTrajectory` `Colourmap` ramp, rate exponent, `CavityResponse`, and `SlabColumnDelta` — never a per-effect material or a second appearance owner; a new aging curve shape is one `rate` column on the trajectory, the eased age fraction `age^rate` the one curve every effect reads; a new slab-column target is one `Option`-typed field on `SlabColumnDelta`; a new exposure law is one `CavityResponse` row carrying its `Scale(age, occlusion)` delegate. Aging is an AUTHORED operator over the `surface#OPENPBR_SLAB` vector and the `graph#MATERIAL_LIBRARY` row — MaterialX 1.39 carries no standard aging node, so the effect set is grounded in the measured `Colourmap` ramps and the slab projection, not a node-category parity target.
- Boundary: `Weathering.Apply` is the ONE flat aging operator — an aged-material type is the deleted form; the trajectory terminal is NOT a single frozen endpoint but a measured perceptually-uniform `Wacton.Unicolour.Datasets` `Colourmap` ramp the aging fold samples at the eased fraction through `Terminal.Map(f)` (an oxidation reads the warm `Colourmaps.Rocket` ramp, a soiling the desaturating `Colourmaps.Mako`, a patina the `Colourmaps.Flare` copper-green ramp, a UV-fade the `Colourmaps.Vlag` diverging bleach, a biological colonization the `Colourmaps.Crest` teal-green ramp). The sampled `Unicolour` is fixed in the colourmap's OWN sRGB/D65 working space (`Colourmap.Config`), so the fold rebases it to the scene-linear `Acescg` pipeline through `Terminal.Map(f).ConvertToConfiguration(PortValue.SceneLinear)` BEFORE the `Mix(rebased, ColourSpace.RgbLinear, f, premultiplyAlpha: false)` — the device-independent XYZ is preserved by the rebase so the mix runs between two colors genuinely in the AP1 scene-linear primaries, never a Rec.709-linear sample treated as AP1-linear (the cross-space grounding defect `finish#FINISH` names and avoids); the base color interpolates toward that rebased terminal so the aging never re-mints a color space or hand-rolls a channel lerp, and the aged color gates `IsInRgbGamut` before admission; emission is NOT aged — weathering shifts a surface's reflectance, not its self-emission, so `Age` leaves `Emission`/`EmissionLuminance` untouched (a luminous sign or a backlit panel does not green with a copper roof), and a future thermochromic/phosphorescent decay is one trajectory column, never an emission lerp smuggled onto the reflectance terminal. The scalar columns (`Roughness`/`Metalness`/`Sheen`/`ClearcoatRoughness`) lerp by the same eased fraction toward the trajectory terminal so a copper row at `age=1` reads the patina-ramp base color, near-zero metalness, and raised roughness the patina trajectory carries, and a chalked finish reads the raised `ClearcoatRoughness` the flat trajectory mirrors from the slab `CoatRoughnessTo` (so the flat-row re-evaluation and the slab path never diverge on coat chalking); the `AgeParameter` is the one normalized `[0,1]` age the graph driver threads — a library row carries its `Seq<WeatheringEffect>` trajectory and the consumer evaluates the aged row through the unchanged `graph#MATERIAL_GRAPH` `Evaluate`, never a per-age graph variant; the aging is a pure projection over `MaterialParameters` columns, the `MaterialParameters.Of` row validation re-admitting the aged row so an aged value passes the same gamut/unit gate a registered row passes, and a non-finite or out-of-gamut aged column rails `MaterialFault`, never a propagated NaN. `ApplySlab` is the realized slab-column path — a weathered row ages through the SAME `surface#OPENPBR_SLAB` slab algebra a fresh row does, the trajectory's `SlabColumnDelta` driving every weathered slab column by the eased fraction BEFORE the `ToLayered` collapse: the `Slab.Base` color lerps toward the rebased terminal (so the slab path greens the copper exactly as the flat path does, never an un-aged base color the renderer shades), its `Metalness` drops toward `MetalnessTo` (patina/oxidation corrode the conductor to a dielectric corrosion product — verdigris/rust are dielectrics, so the aging DE-METALIZES the base rather than swapping one `ConductorMetal` for another the smart-enum cannot represent), its `Roughness` rises by `BaseRoughnessTo`, and its `Transmission` drops by `TransmissionTo` (a fouling surface loses clarity); the `Slab.Coat` `Roughness` rises by `CoatRoughnessTo` (chalking) and its `Color` tints toward `CoatColorTo`; the `Slab.Fuzz` `Weight` rises by `FuzzWeightTo` (crevice grime) and its `Color` tints toward `FuzzColorTo` (the grime is grey-brown, not the fresh fuzz tint) — each `None` column leaving its slab value untouched (a typed absence, never an in-band `-1.0` sentinel a `[0,1]` column cannot otherwise carry), and every aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of two validated in-band endpoints at `f∈[0,1]` (a blend of non-negative finite channels stays non-negative finite), so the slab aging is TOTAL and returns a bare `SlabStack` — a `Fin<SlabStack>` would be an always-`Succ` decoration modeling a non-finite the validated carriers cannot produce; the `cavityOcclusion` `[0,1]` AO sample from a `texture#TEXTURE_UV` cavity/ambient-occlusion node the consumer samples upstream scales the uniform age per shade point through each effect's `CavityResponse` so spatially-varying weathering (grime in crevices, water-streak runoff, sun-bleached exposed faces) rides the existing slab fold — a `CavityResponse.Crevice` effect (soiling, patina, biological) ageing at `age·occlusion` (a fully-shadowed crevice `occlusion=1` at the full rate, an exposed face less — the damp shaded crevice is exactly where algae colonizes), a `CavityResponse.Exposed` effect (uv-fade) at `age·(1−occlusion)` (the sun-struck face bleaches, the crevice protected), and a `CavityResponse.Uniform` effect (oxidation) at `age` regardless — never a second aging surface.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                           // Fin, Seq, Option (the rail TYPES; the static Prelude below carries Some/None only)
using Rasm;                                  // UnitInterval
using Rasm.Domain;                           // Op (the fault-correlation key the flat Apply path rails MaterialFault on)
using Rasm.Materials.Appearance.Bsdf;        // MaterialFault, RgbSpectrum
using Rasm.Materials.Appearance.Graph;       // MaterialParameters, PortValue (PortValue.SceneLinear is the Acescg working space) — declared in the .Graph child namespace, not auto-imported by the parent
using Rasm.Materials.Appearance.Surface;     // Slab, SlabStack, ConductorMetal
using Wacton.Unicolour;                       // Unicolour, Configuration, ColourSpace, ConvertToConfiguration
using Wacton.Unicolour.Datasets;             // Colourmap, Colourmaps
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;          // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

// The per-effect exposure law mapping the consumer's [0,1] ambient-occlusion scalar to the effect's age multiplier:
// crevice-accumulating effects (soiling deposit, patina pooling) age with occlusion, exposure-driven bleaching
// (uv-fade) ages with its complement, and a uniform effect (bulk oxidation) ignores it. The delegate is carried on
// the row so ApplySlab reads CavityResponse.Scale(age, occlusion) by data, never a switch on an exposure enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityResponse {
    public static readonly CavityResponse Crevice = new("crevice", static (age, occ) => age * occ);
    public static readonly CavityResponse Exposed = new("exposed", static (age, occ) => age * (1.0 - occ));
    public static readonly CavityResponse Uniform = new("uniform", static (age, _) => age);

    [UseDelegateFromConstructor]
    public partial double Scale(double age, double occlusion);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeatheringEffect {
    private WeatheringEffect() { }

    public sealed record Patina(double Rate) : WeatheringEffect;
    public sealed record Oxidation(double Rate) : WeatheringEffect;
    public sealed record Soiling(double Rate) : WeatheringEffect;
    public sealed record UvFade(double Rate) : WeatheringEffect;
    // Biological colonization (algae · lichen · moss) is the biotic facade-weathering mechanism — the class the inorganic
    // Patina/Oxidation (chemical/corrosion), Soiling (particulate), and UvFade (photochemical) cases leave uncovered: a
    // moisture-driven living biofilm that greens then blackens a surface on shaded/damp/north faces, distinct from the grey
    // particulate Soiling deposit (a green organic film, not a mineral-grime tint) and the OPPOSITE exposure law to UvFade
    // (it COLONIZES the protected crevice the sun bleaches). The closed family thus spans the chemical, particulate,
    // photochemical, AND biotic facade-aging mechanisms, not an inorganic slice.
    public sealed record Biological(double Rate) : WeatheringEffect;

    // The flat ClearcoatRoughnessTo mirrors the SlabColumnDelta CoatRoughnessTo per effect, so a chalked finish reads the
    // SAME raised coat roughness whether the consumer re-evaluates the flat MaterialParameters row through the graph or
    // shades the lowered slab path — the two aging paths never diverge on the coat the flat row also lowers.
    public WeatheringTrajectory Trajectory => Switch(
        patina:     static p => new WeatheringTrajectory(Colourmaps.Flare,  CavityResponse.Crevice, RoughnessTo: Some(0.55), MetalnessTo: Some(0.0), SheenTo: Some(0.0),  ClearcoatRoughnessTo: None,       Math.Max(0.1, p.Rate),
                                   new SlabColumnDelta(BaseRoughnessTo: Some(0.55), BaseMetalnessTo: Some(0.0),  BaseTransmissionTo: None,      CoatRoughnessTo: None,       CoatColorTo: None,                  FuzzWeightTo: None,      FuzzColorTo: None)),
        oxidation:  static o => new WeatheringTrajectory(Colourmaps.Rocket, CavityResponse.Uniform, RoughnessTo: Some(0.80), MetalnessTo: Some(0.2), SheenTo: Some(0.0),  ClearcoatRoughnessTo: Some(0.85), Math.Max(0.1, o.Rate),
                                   new SlabColumnDelta(BaseRoughnessTo: Some(0.80), BaseMetalnessTo: Some(0.2),  BaseTransmissionTo: None,      CoatRoughnessTo: Some(0.85), CoatColorTo: None,                  FuzzWeightTo: None,      FuzzColorTo: None)),
        soiling:    static s => new WeatheringTrajectory(Colourmaps.Mako,   CavityResponse.Crevice, RoughnessTo: Some(0.70), MetalnessTo: None,      SheenTo: Some(0.3),  ClearcoatRoughnessTo: None,       Math.Max(0.1, s.Rate),
                                   new SlabColumnDelta(BaseRoughnessTo: None,       BaseMetalnessTo: None,       BaseTransmissionTo: Some(0.0), CoatRoughnessTo: None,       CoatColorTo: None,                  FuzzWeightTo: Some(0.4), FuzzColorTo: Some(GrimeFuzz))),
        uvFade:     static f => new WeatheringTrajectory(Colourmaps.Vlag,   CavityResponse.Exposed, RoughnessTo: Some(0.40), MetalnessTo: None,      SheenTo: Some(0.0),  ClearcoatRoughnessTo: Some(0.50), Math.Max(0.1, f.Rate),
                                   new SlabColumnDelta(BaseRoughnessTo: Some(0.40), BaseMetalnessTo: None,       BaseTransmissionTo: None,      CoatRoughnessTo: Some(0.50), CoatColorTo: Some(ChalkCoat),       FuzzWeightTo: None,      FuzzColorTo: None)),
        // Biofilm: a damp-crevice green-black living layer — the Crest teal-green ramp toward a dark biofilm, base roughened
        // (a matte organic mat), NOT de-metalized (the film coats any substrate, leaving the conductor intact), and a green
        // BioFilm fuzz grime distinct from the grey GrimeFuzz so algae reads green and soot reads grey on the SAME fuzz slot.
        biological: static b => new WeatheringTrajectory(Colourmaps.Crest,  CavityResponse.Crevice, RoughnessTo: Some(0.75), MetalnessTo: None,      SheenTo: Some(0.2),  ClearcoatRoughnessTo: None,       Math.Max(0.1, b.Rate),
                                   new SlabColumnDelta(BaseRoughnessTo: Some(0.75), BaseMetalnessTo: None,       BaseTransmissionTo: None,      CoatRoughnessTo: None,       CoatColorTo: None,                  FuzzWeightTo: Some(0.5), FuzzColorTo: Some(BioFilm))));

    // The grime/chalk/biofilm tints the slab colors age toward: grey-brown soiling deposit on the fuzz crevice grime, a
    // pale chalk bloom on the UV-faded coat, a dark green algal biofilm on the colonized fuzz — RgbSpectrum literals in the
    // SAME Acescg scene-linear space the slab base colors carry, so the slab-color lerp runs in one primaries set (no rebase
    // needed, these are authored AP1); the base-color terminal still samples the rebased Colourmap, only these fuzz/coat
    // tints are authored directly.
    static readonly RgbSpectrum GrimeFuzz = RgbSpectrum.Create(0.06, 0.055, 0.05);
    static readonly RgbSpectrum ChalkCoat = RgbSpectrum.Create(0.78, 0.78, 0.76);
    static readonly RgbSpectrum BioFilm = RgbSpectrum.Create(0.035, 0.060, 0.040);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-effect projection onto the surface#OPENPBR_SLAB Slab columns: patina/oxidation roughen + de-metalize the
// base (verdigris/rust are dielectrics — the conductor corrodes AWAY, never a metal-to-metal ConductorMetal swap the
// 8-member smart-enum cannot represent), soiling fouls transmission and adds a grey-brown fuzz grime, chalking lifts
// coat roughness and blooms a pale coat tint. None leaves the column at its fresh value (a typed absence, never an
// in-band -1.0 sentinel a [0,1] column cannot carry). Color targets are RgbSpectrum (the validated scene-linear
// carrier), scalar targets Option<double>; the slab base COLOR ages toward the rebased Colourmap terminal directly,
// so every weathered slab column the renderer shades moves, never a base color the flat path greens but the slab drops.
public readonly record struct SlabColumnDelta(
    Option<double> BaseRoughnessTo,
    Option<double> BaseMetalnessTo,
    Option<double> BaseTransmissionTo,
    Option<double> CoatRoughnessTo,
    Option<RgbSpectrum> CoatColorTo,
    Option<double> FuzzWeightTo,
    Option<RgbSpectrum> FuzzColorTo);

// Terminal is a measured perceptual ramp, never a frozen endpoint: Age samples Terminal.Map(f) at the eased fraction
// and rebases it from the colourmap's sRGB/D65 space into the scene-linear Acescg pipeline. Cavity is the per-effect
// exposure law scaling the age by the consumer's occlusion scalar. Slab carries the slab-column projection the
// surface#OPENPBR_SLAB columns age through once a row lowers. Scalar targets are Option<double> — None is the typed
// "this effect leaves the column at its fresh value", never -1.0.
public readonly record struct WeatheringTrajectory(
    Colourmap Terminal,
    CavityResponse Cavity,
    Option<double> RoughnessTo,
    Option<double> MetalnessTo,
    Option<double> SheenTo,
    Option<double> ClearcoatRoughnessTo,   // mirrors SlabColumnDelta.CoatRoughnessTo so the flat-row chalking matches the slab path
    double Rate,
    SlabColumnDelta Slab);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Weathering {
    // Uniform aging over the flat MaterialParameters row (the row-level path graph#MATERIAL_GRAPH re-evaluates per
    // shade point, the graph's own Texture AO node already supplying spatial variation in the node fold).
    public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key) =>
        effects.Fold(Fin.Succ(baseRow), (acc, effect) => acc.Bind(row => Age(row, effect.Trajectory, age.Value, key)))
            .Bind(aged => MaterialParameters.Of(aged, key));

    // Spatially-varying aging over the lowered surface#OPENPBR_SLAB SlabStack columns: a weathered row ages through the
    // SAME slab algebra a fresh row does, the trajectory's SlabColumnDelta driving every weathered slab column BEFORE
    // the ToLayered collapse, each effect's CavityResponse mapping the per-shade-point AO scalar to its own age
    // multiplier (crevice effects ride occlusion, exposure-driven bleaching its complement). The fold is TOTAL: every
    // aged column is a convex blend of a validated RgbSpectrum endpoint and a validated RgbSpectrum terminal at f∈[0,1],
    // so the aged channel is finite and non-negative by construction and never trips the RgbSpectrum admission — the
    // slab path carries NO fault rail (a fabricated Fin<SlabStack> here would model a non-finite the inputs cannot
    // produce). The genuine gamut rail is the flat Apply path, where a Mix toward a colourmap terminal CAN leave RGB
    // gamut and MaterialParameters.Of re-admits; the slab terminal is read straight from the rebased colourmap below.
    public static SlabStack ApplySlab(SlabStack stack, Seq<WeatheringEffect> effects, AgeParameter age, UnitInterval cavityOcclusion) =>
        effects.Fold(stack, (s, effect) => AgeSlab(s, effect.Trajectory, effect.Trajectory.Cavity.Scale(age.Value, cavityOcclusion.Value)));

    static Fin<MaterialParameters> Age(MaterialParameters row, WeatheringTrajectory t, double age, Op key) {
        double f = Eased(age, t.Rate);
        Unicolour terminal = SceneTerminal(t.Terminal, f);
        Unicolour basecolor = row.BaseColor.Mix(terminal, ColourSpace.RgbLinear, f, premultiplyAlpha: false);
        return basecolor.IsInRgbGamut
            ? Fin.Succ(row with {
                BaseColor = basecolor,
                Roughness = LerpToward(row.Roughness, t.RoughnessTo, f),
                Metalness = LerpToward(row.Metalness, t.MetalnessTo, f),
                Sheen = LerpToward(row.Sheen, t.SheenTo, f),
                ClearcoatRoughness = LerpToward(row.ClearcoatRoughness, t.ClearcoatRoughnessTo, f) })
            : MaterialFault.Gamut(key, $"<aged-color-out-of-gamut:{basecolor.Hex}>");
    }

    // The slab-column aging: the trajectory delta drives every realized slab column by the eased fraction. The base
    // COLOR ages toward the scene-linear terminal (so the slab path greens the copper exactly as the flat path does),
    // metalness drops (the conductor corrodes to a dielectric, never a ConductorMetal swap), roughness/transmission
    // shift, and the coat/fuzz tint toward their grime/chalk targets. A None delta leaves the column at its fresh value.
    static SlabStack AgeSlab(SlabStack stack, WeatheringTrajectory t, double age) {
        double f = Eased(age, t.Rate);
        RgbSpectrum baseTerminal = SceneTerminalRgb(t.Terminal, f);
        return new SlabStack(stack.Slabs.Map(slab => AgeSlabCase(slab, t.Slab, baseTerminal, f)));
    }

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

    // age^rate clamped to [0,1] — the one sub-linear deposition curve every effect and both folds read.
    static double Eased(double age, double rate) => Math.Clamp(Math.Pow(Math.Clamp(age, 0.0, 1.0), rate), 0.0, 1.0);

    // The Colourmap sample rebased into the scene-linear Acescg pipeline: Map(f) returns a Unicolour fixed in the
    // colourmap's own sRGB/D65 working space (Colourmap.Config), so ConvertToConfiguration rebases it onto the scene
    // PortValue.SceneLinear (Acescg) space — preserving the device-independent XYZ — BEFORE any RgbLinear mix, never a
    // Rec.709-linear sample mixed into an AP1-linear base as if the primaries matched (the finish#FINISH grounding law).
    static Unicolour SceneTerminal(Colourmap ramp, double f) => ramp.Map(f).ConvertToConfiguration(PortValue.SceneLinear);

    // The scene-linear terminal as an RgbSpectrum band for the slab base-color lerp (the slab carries RgbSpectrum, not
    // Unicolour): rebase, read the scene-linear RgbLinear channels, clamp non-negative for the validated carrier.
    static RgbSpectrum SceneTerminalRgb(Colourmap ramp, double f) {
        var lin = SceneTerminal(ramp, f).RgbLinear;
        return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B));
    }

    // A None target leaves the column at its fresh value; Some(target) eases toward it by f — the typed-absence lerp.
    static double LerpToward(double current, Option<double> target, double f) => target.Match(Some: to => current + (to - current) * f, None: () => current);

    // A None tint leaves the slab color at its fresh value; Some(tint) eases toward it through RgbSpectrum.Lerp (a convex
    // blend of two validated bands at f∈[0,1] stays in-band, so the lerp is total — no Fin, no AllFinite re-check).
    static RgbSpectrum LerpColor(RgbSpectrum current, Option<RgbSpectrum> target, double f) =>
        target.Match(Some: tint => current.Lerp(tint, f), None: () => current);
}
```

## [03]-[RESEARCH]

- [WEATHERING_TRAJECTORY]: the per-effect terminals are measured `Wacton.Unicolour.Datasets` `Colourmap` ramps the fold samples at the eased fraction rather than hand-keyed endpoints — patina the `Colourmaps.Flare` copper-green ramp (Cu→CuCO₃ chromaticity traverse), oxidation the warm `Colourmaps.Rocket` ramp (Fe₂O₃ rust deepening), soiling the desaturating `Colourmaps.Mako` ramp (grey-grime deposition), UV-fade the `Colourmaps.Vlag` diverging bleach (chromophore desaturation), biological the `Colourmaps.Crest` teal-green ramp (algal/lichen colonization toward a dark biofilm) — each a published perceptually-uniform gradient read through `Map(f)`. Every sample is fixed in the colourmap's own sRGB/D65 working space and rebased to the scene-linear `Acescg` pipeline through `ConvertToConfiguration(PortValue.SceneLinear)` before the `RgbLinear` mix, so the perceptual gradient lands in the same AP1 primaries the base color carries (the cross-space discipline `finish#FINISH` `FinishMix.Reflectance` enforces), with a sub-linear `age^rate` deposition curve. The five realized trajectories span the inorganic, photochemical, and biotic facade-aging mechanisms; a new effect lands its `Colourmap` ramp, rate, `CavityResponse`, and `SlabColumnDelta` as one `WeatheringEffect` case. The probe is the per-effect curve calibration against measured aging series, not a re-architecture of the fold.
- [OPENPBR_SLAB_AGING]: REALIZED — `Weathering.ApplySlab` ages the realized `surface#OPENPBR_SLAB` `SlabStack` columns directly: each `WeatheringEffect.Trajectory` carries a `SlabColumnDelta` projecting onto every weathered slab column the fold lerps by the eased fraction BEFORE the `ToLayered` collapse — the `Slab.Base` color toward the rebased `Colourmap` terminal (so the slab path greens the copper exactly as the flat path does, never an un-aged base color the renderer shades), the `Slab.Base` `Metalness` toward `BaseMetalnessTo` (patina/oxidation corrode the conductor to a dielectric verdigris/rust — both DIELECTRICS, so the aging de-metalizes the base rather than swapping one `ConductorMetal` for another the 8-member smart-enum cannot represent and that would be a no-op on a copper/iron row), the `Slab.Base` `Roughness`/`Transmission`, the `Slab.Coat` `Roughness` (chalking) and `Color` (a pale chalk bloom), and the `Slab.Fuzz` `Weight` (crevice grime) and `Color` (a grey-brown soiling deposit). Each aged `RgbSpectrum` column is a convex `RgbSpectrum.Lerp` of two validated in-band endpoints at `f∈[0,1]`, so it stays in-band by construction and the slab aging is TOTAL — `ApplySlab` returns a bare `SlabStack`, and a `Fin<SlabStack>` (with an `AllFinite` re-check downstream of the throwing `RgbSpectrum.Lerp`→`Create`) was the illusory rail the rebuild removed: it modeled a non-finite the validated carriers cannot produce and would in any case throw inside `Create` before any re-check ran. A `None` column delta leaves that column at its fresh value (a typed absence, never an in-band `-1.0` a `[0,1]` column cannot carry); the flat-row `Apply` survives as the uniform row-level path `graph#MATERIAL_GRAPH` re-evaluates and remains the ONE fallible path (the `Mix`→gamut gate + `MaterialParameters.Of` re-admission). The slab columns are the shared aging target `finish#FINISH` also drives (a finish chalks through the same `Slab.Coat` roughness lift).
- [PROCEDURAL_SOILING_MASK]: REALIZED — spatially-varying weathering drives the aging fraction from a `texture#TEXTURE_UV` cavity/ambient-occlusion sample the consumer evaluates upstream rather than a uniform age: `ApplySlab` takes a `UnitInterval cavityOcclusion` (the per-shade-point AO the consumer samples from its `Texture` graph node and threads in — this page reads the scalar, it never composes `TextureUv` sampling itself, the strata keeping the texture fold on the `graph#MATERIAL_GRAPH` node owner) and each effect's `CavityResponse` maps that one scalar to its own age multiplier: `Crevice` (soiling, patina) ages at `age·occlusion` so a fully-shadowed crevice (`occlusion=1`) accumulates grime at the full rate and an exposed face less, `Exposed` (uv-fade) ages at `age·(1−occlusion)` so the sun-struck face bleaches while the protected crevice does not, and `Uniform` (bulk oxidation) ages at `age` regardless. The per-effect exposure law closes the AO-to-age response in place — a single `age·occlusion` for all effects mis-models exposure-driven bleaching, which is strongest on the OPPOSITE (exposed) faces — riding the existing slab fold without a second aging surface. The remaining tuning is the per-effect AO-to-age gamma against measured soiling/bleaching series, not a re-architecture.
