# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters` and an `AgeParameter` and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar, never a `WeatheredCopper`/`SoiledFacade` type. The aging is a parameterized operator over the existing `MaterialParameters` columns (base color, roughness, metalness, sheen) interpolated through Wacton.Unicolour scene-linear mixing toward each effect's terminal state, composed not re-minted; the terminal is a measured `Wacton.Unicolour.Datasets` `Colourmap` ramp the fold samples at the eased age fraction (`Terminal.Map(f)`) so every effect traverses a perceptually-uniform gradient rather than a single frozen endpoint, and the realized `bsdf#OPENPBR_SLAB` `SlabStack` slab columns are the available layering target the aging trajectory terminal drives once a row lowers, the flat `MaterialParameters` interpolation the form today until a consumer wires the slab-column delta. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters` for the parameter vector, Wacton.Unicolour directly for the scene-linear color interpolation, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps, and the `MaterialFault` band-2450 rail for a non-finite or out-of-range age.

## [01]-[INDEX]

- [01]-[WEATHERING]: the `AgeParameter` value-object, the `WeatheringEffect` `[Union]` effect family, the `WeatheringTrajectory` per-effect terminal/rate row, and the one `Weathering.Apply` aging fold over `MaterialParameters`.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade); `WeatheringTrajectory` the per-effect terminal-state/rate row.
- Cases: effect {`Patina` (verdigris green terminal, metalness drop), `Oxidation` (rust/tarnish terminal, roughness rise), `Soiling` (grey grime terminal, sheen drop), `UvFade` (desaturated terminal, base-color lerp)} — the closed weathering family; an effect is a `WeatheringEffect` case carrying its trajectory, never an effect subtype.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key)` — the aging fold composing each effect's `WeatheringTrajectory` over the base row by the eased age fraction, `Fin<T>` aborting on an out-of-gamut aged color (`MaterialFault.Gamut`) or a non-finite age (`MaterialFault.Parameter`); arity is one — a multi-effect aging folds the effect `Seq` left-to-right, never a per-effect method.
- Packages: Wacton.Unicolour (composed — scene-linear `Mix` toward each sampled terminal and `IsInRgbGamut` for the aged-color gate), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramp the trajectory terminal samples by the eased age fraction), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` case carrying its `WeatheringTrajectory` `Colourmap` ramp and rate exponent — never a per-effect material or a second appearance owner; a new aging curve shape is one `rate` column on the trajectory, the eased age fraction `age^rate` the one curve every effect reads. The effect set aligns to the MaterialX node-graph aging operators at `interchange#MATERIALX_DOCUMENT`; with the realized `bsdf#OPENPBR_SLAB` slab stack, each effect's terminal state targets the OpenPBR slab columns (coat for chalking, fuzz for soiling) once a row lowers rather than the flat `MaterialParameters` interpolation.
- Boundary: `Weathering.Apply` is the ONE aging operator — an aged-material type is the deleted form; the trajectory terminal is NOT a single frozen endpoint but a measured perceptually-uniform `Wacton.Unicolour.Datasets` `Colourmap` ramp the aging fold samples at the eased fraction through `Terminal.Map(f)` (an oxidation reads the warm `Colourmaps.Rocket` ramp, a soiling the desaturating `Colourmaps.Mako`, a patina the `Colourmaps.Flare` copper-green ramp, a UV-fade the `Colourmaps.Vlag` diverging bleach), each sampled `Unicolour` mixed into the base through `Mix(sampled, ColourSpace.RgbLinear, f, ...)` so the interpolation runs in the scene-linear pipeline and the terminal stays a reflectance not a hand-keyed display swatch; the base color and emission interpolate toward that sampled terminal through the directly-composed Wacton.Unicolour `Mix(sampled, ColourSpace.RgbLinear, fraction, premultiplyAlpha: false)` so the aging never re-mints a color space or hand-rolls a channel lerp, and the aged color gates `IsInRgbGamut` before admission; the scalar columns (`Roughness`/`Metalness`/`Sheen`) lerp by the same eased fraction toward the trajectory terminal so a copper row at `age=1` reads the patina-ramp base color, near-zero metalness, and raised roughness the patina trajectory carries; the `AgeParameter` is the one normalized `[0,1]` age the graph driver threads — a library row carries its `Seq<WeatheringEffect>` trajectory and the consumer evaluates the aged row through the unchanged `graph#MATERIAL_GRAPH` `Evaluate`, never a per-age graph variant; the aging is a pure projection over `MaterialParameters` columns, the `MaterialParameters.Of` row validation re-admitting the aged row so an aged value passes the same gamut/unit gate a registered row passes, and a non-finite or out-of-gamut aged column rails `MaterialFault`, never a propagated NaN.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct AgeParameter {
    static partial void NormalizeAndValidate(ref double value, ref ValidationError? validationError) =>
        validationError = double.IsFinite(value) && value is >= 0.0 and <= 1.0 ? null : new ValidationError("<age requires [0,1]>");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeatheringEffect {
    private WeatheringEffect() { }

    public sealed record Patina(double Rate) : WeatheringEffect;
    public sealed record Oxidation(double Rate) : WeatheringEffect;
    public sealed record Soiling(double Rate) : WeatheringEffect;
    public sealed record UvFade(double Rate) : WeatheringEffect;

    public WeatheringTrajectory Trajectory => Switch(
        patina:    static p => new WeatheringTrajectory(Colourmaps.Flare, RoughnessTo: 0.65, MetalnessTo: 0.0, SheenTo: 0.0, Math.Max(0.1, p.Rate)),
        oxidation: static o => new WeatheringTrajectory(Colourmaps.Rocket, RoughnessTo: 0.80, MetalnessTo: 0.2, SheenTo: 0.0, Math.Max(0.1, o.Rate)),
        soiling:   static s => new WeatheringTrajectory(Colourmaps.Mako, RoughnessTo: 0.70, MetalnessTo: -1.0, SheenTo: 0.0, Math.Max(0.1, s.Rate)),
        uvFade:    static f => new WeatheringTrajectory(Colourmaps.Vlag, RoughnessTo: -1.0, MetalnessTo: -1.0, SheenTo: 0.0, Math.Max(0.1, f.Rate)));
}

// --- [MODELS] ------------------------------------------------------------------------------
// Terminal is a measured perceptual ramp, never a frozen endpoint: Age samples Terminal.Map(f) at the eased fraction.
public readonly record struct WeatheringTrajectory(Colourmap Terminal, double RoughnessTo, double MetalnessTo, double SheenTo, double Rate);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Weathering {
    public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key) =>
        effects.Fold(Fin.Succ(baseRow), (acc, effect) => acc.Bind(row => Age(row, effect.Trajectory, age.Value, key)))
            .Bind(aged => MaterialParameters.Of(aged, key));

    static Fin<MaterialParameters> Age(MaterialParameters row, WeatheringTrajectory t, double age, Op key) {
        double f = Math.Clamp(Math.Pow(age, t.Rate), 0.0, 1.0);
        Unicolour terminal = t.Terminal.Map(f);
        Unicolour basecolor = row.BaseColor.Mix(terminal, ColourSpace.RgbLinear, f, premultiplyAlpha: false);
        return basecolor.IsInRgbGamut
            ? Fin.Succ(row with {
                BaseColor = basecolor,
                Roughness = LerpToward(row.Roughness, t.RoughnessTo, f),
                Metalness = LerpToward(row.Metalness, t.MetalnessTo, f),
                Sheen = LerpToward(row.Sheen, t.SheenTo, f) })
            : MaterialFault.Gamut(key, $"<aged-color-out-of-gamut:{basecolor.Hex}>");
    }

    static double LerpToward(double current, double target, double f) => target < 0.0 ? current : current + (target - current) * f;
}
```

## [03]-[RESEARCH]

- [WEATHERING_TRAJECTORY]: the per-effect terminals are measured `Wacton.Unicolour.Datasets` `Colourmap` ramps the fold samples at the eased fraction rather than four hand-keyed endpoints — patina the `Colourmaps.Flare` copper-green ramp (Cu→CuCO₃ chromaticity traverse), oxidation the warm `Colourmaps.Rocket` ramp (Fe₂O₃ rust deepening), soiling the desaturating `Colourmaps.Mako` ramp (grey-grime deposition), UV-fade the `Colourmaps.Vlag` diverging bleach (chromophore desaturation) — each a published perceptually-uniform gradient read through `Map(f)` into `ColourSpace.RgbLinear`, with a sub-linear `age^rate` deposition curve. The four realized trajectories are the seed; a new effect lands its `Colourmap` ramp and rate as one `WeatheringEffect` case. The probe is the per-effect curve calibration against measured aging series, not a re-architecture of the fold.
- [OPENPBR_SLAB_AGING]: the realized `bsdf#OPENPBR_SLAB` `SlabStack` lets the aging target the OpenPBR slab columns directly once a row lowers — chalking raises the coat roughness slab, soiling raises the fuzz weight, patina swaps the base `ConductorMetal` — so the trajectory terminal becomes a slab-column delta rather than a flat `MaterialParameters` interpolation. The flat `MaterialParameters` interpolation is the current form because no consumer yet drives the slab columns through weathering; the probe is wiring the trajectory terminal onto the `SlabStack.Lower` columns.
- [PROCEDURAL_SOILING_MASK]: spatially-varying weathering (grime accumulation in crevices, water-streak runoff) drives the aging fraction from a `texture#TEXTURE_UV` cavity/ambient-occlusion mask rather than a uniform age — the `AgeParameter` becomes a per-shade-point sample the `Texture` graph node supplies, the same `Weathering.Apply` fold reading a per-point fraction. The probe is the cavity-mask texture node wiring, riding the existing graph fold, never a second aging surface.
```
