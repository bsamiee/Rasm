# [MATERIALS_WEATHERING]

THE AGING OPERATOR. One `Weathering` static fold over the closed `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade) drives a `graph#MATERIAL_LIBRARY` `MaterialParameters` row forward along an `AgeParameter` so a library row carries its weathering trajectory rather than a single frozen state. An aged material is NEVER a second appearance surface: `Weathering.Apply` takes the base `MaterialParameters` and an `AgeParameter` and returns the aged `MaterialParameters` the SAME `graph#MATERIAL_GRAPH` node fold and `bsdf#LOBE_FAMILY` lobe set shade — a copper roof greens, a facade soils, a coating chalks as a function of the one age scalar, never a `WeatheredCopper`/`SoiledFacade` type. The aging is a parameterized operator over the existing `MaterialParameters` columns (base color, roughness, metalness, sheen) interpolated through Wacton.Unicolour scene-linear mixing toward each effect's terminal state, composed not re-minted; the terminal is a measured `Wacton.Unicolour.Datasets` `Colourmap` ramp the fold samples at the eased age fraction (`Terminal.Map(f)`) so every effect traverses a perceptually-uniform gradient rather than a single frozen endpoint, and each effect's `SlabColumnDelta` drives the realized `surface#OPENPBR_SLAB` `SlabStack` slab columns directly through `Weathering.ApplySlab` (chalking the `Slab.Coat` roughness, soiling the `Slab.Fuzz` weight, patina the `Slab.Base` `ConductorMetal`) so a weathered row ages through the SAME slab algebra a fresh row does, a `texture#TEXTURE_UV` cavity-mask ambient-occlusion sample scaling the age per shade point; the flat `MaterialParameters` `Apply` interpolation is the row-level path the `graph#MATERIAL_GRAPH` re-evaluates. The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters` for the parameter vector, Wacton.Unicolour directly for the scene-linear color interpolation, `Wacton.Unicolour.Datasets` `Colourmaps` for the perceptual ramps, and the `MaterialFault` band-2450 rail for a non-finite or out-of-range age.

## [01]-[INDEX]

- [01]-[WEATHERING]: the `AgeParameter` value-object, the `WeatheringEffect` `[Union]` effect family, the `WeatheringTrajectory` per-effect terminal/rate/`SlabColumnDelta` row, and the `Weathering.Apply` flat-row + `ApplySlab` slab-column aging folds, the cavity-mask AO source scaling the age per shade point.

## [02]-[WEATHERING]

- Owner: `Weathering` static aging fold; `AgeParameter` `[ValueObject<double>]` the `[0,1]` normalized age; `WeatheringEffect` `[Union]` (patina · oxidation · soiling · uv-fade); `WeatheringTrajectory` the per-effect terminal-state/rate row carrying a `SlabColumnDelta` for the `surface#OPENPBR_SLAB` columns; `SlabColumnDelta` the per-effect coat/fuzz/base-conductor slab delta.
- Cases: effect {`Patina` (verdigris green terminal, metalness drop), `Oxidation` (rust/tarnish terminal, roughness rise), `Soiling` (grey grime terminal, sheen drop), `UvFade` (desaturated terminal, base-color lerp)} — the closed weathering family; an effect is a `WeatheringEffect` case carrying its trajectory, never an effect subtype.
- Entry: `public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key)` ages the flat row (the row-level path `graph#MATERIAL_GRAPH` re-evaluates), and `public static Fin<SlabStack> ApplySlab(SlabStack stack, Seq<WeatheringEffect> effects, AgeParameter age, UnitInterval cavityOcclusion, Op key)` ages the lowered `surface#OPENPBR_SLAB` slab columns — both fold each effect's `WeatheringTrajectory` over the carrier by the eased age fraction (`age^rate`), `Fin<T>` aborting on an out-of-gamut aged color (`MaterialFault.Gamut`) or a non-finite age (`MaterialFault.Parameter`); arity is one — a multi-effect aging folds the effect `Seq` left-to-right, never a per-effect method, and the `cavityOcclusion` `[0,1]` AO sample scales the age per shade point.
- Packages: Wacton.Unicolour (composed — scene-linear `Mix` toward each sampled terminal and `IsInRgbGamut` for the aged-color gate), Wacton.Unicolour.Datasets (composed — the perceptual `Colourmap` ramp the trajectory terminal samples by the eased age fraction), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new weathering effect is one `WeatheringEffect` case carrying its `WeatheringTrajectory` `Colourmap` ramp, rate exponent, and `SlabColumnDelta` — never a per-effect material or a second appearance owner; a new aging curve shape is one `rate` column on the trajectory, the eased age fraction `age^rate` the one curve every effect reads, a new slab-column target one `SlabColumnDelta` column. The effect set aligns to the MaterialX node-graph aging operators at `interchange#MATERIALX_DOCUMENT`; each effect's `SlabColumnDelta` targets the realized `surface#OPENPBR_SLAB` slab columns (coat roughness for chalking, fuzz weight for soiling, base `ConductorMetal` swap for patina) through `ApplySlab` once a row lowers, the flat `MaterialParameters` interpolation the row-level path the graph re-evaluates.
- Boundary: `Weathering.Apply` is the ONE aging operator — an aged-material type is the deleted form; the trajectory terminal is NOT a single frozen endpoint but a measured perceptually-uniform `Wacton.Unicolour.Datasets` `Colourmap` ramp the aging fold samples at the eased fraction through `Terminal.Map(f)` (an oxidation reads the warm `Colourmaps.Rocket` ramp, a soiling the desaturating `Colourmaps.Mako`, a patina the `Colourmaps.Flare` copper-green ramp, a UV-fade the `Colourmaps.Vlag` diverging bleach), each sampled `Unicolour` mixed into the base through `Mix(sampled, ColourSpace.RgbLinear, f, ...)` so the interpolation runs in the scene-linear pipeline and the terminal stays a reflectance not a hand-keyed display swatch; the base color and emission interpolate toward that sampled terminal through the directly-composed Wacton.Unicolour `Mix(sampled, ColourSpace.RgbLinear, fraction, premultiplyAlpha: false)` so the aging never re-mints a color space or hand-rolls a channel lerp, and the aged color gates `IsInRgbGamut` before admission; the scalar columns (`Roughness`/`Metalness`/`Sheen`) lerp by the same eased fraction toward the trajectory terminal so a copper row at `age=1` reads the patina-ramp base color, near-zero metalness, and raised roughness the patina trajectory carries; the `AgeParameter` is the one normalized `[0,1]` age the graph driver threads — a library row carries its `Seq<WeatheringEffect>` trajectory and the consumer evaluates the aged row through the unchanged `graph#MATERIAL_GRAPH` `Evaluate`, never a per-age graph variant; the aging is a pure projection over `MaterialParameters` columns, the `MaterialParameters.Of` row validation re-admitting the aged row so an aged value passes the same gamut/unit gate a registered row passes, and a non-finite or out-of-gamut aged column rails `MaterialFault`, never a propagated NaN. `ApplySlab` is the realized slab-column path — a weathered row ages through the SAME `surface#OPENPBR_SLAB` slab algebra a fresh row does, the trajectory's `SlabColumnDelta` driving the `Slab.Coat` roughness (chalking), the `Slab.Fuzz` weight (soiling crevice grime), and the `Slab.Base` `ConductorMetal` swap (patina Cu→verdigris, oxidation Fe→rust) by the eased fraction BEFORE the `ToLayered` collapse rather than the flat base-color lerp, so the renderer shades the aged material through one `LayeredBsdf`; the `cavityOcclusion` `[0,1]` AO sample from a `texture#TEXTURE_UV` cavity/ambient-occlusion node scales the uniform age per shade point through `CavityScaled` so spatially-varying weathering (grime in crevices, water-streak runoff) rides the existing graph fold, a fully-shadowed crevice (`occlusion=1`) ageing at the full rate and an exposed face less, never a second aging surface.

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
        patina:    static p => new WeatheringTrajectory(Colourmaps.Flare,  RoughnessTo: 0.65, MetalnessTo: 0.0, SheenTo: 0.0, Math.Max(0.1, p.Rate), new SlabColumnDelta(CoatRoughnessTo: -1.0, FuzzWeightTo: -1.0, ConductorSwap: Some(ConductorMetal.Copper))),
        oxidation: static o => new WeatheringTrajectory(Colourmaps.Rocket, RoughnessTo: 0.80, MetalnessTo: 0.2, SheenTo: 0.0, Math.Max(0.1, o.Rate), new SlabColumnDelta(CoatRoughnessTo: 0.85, FuzzWeightTo: -1.0, ConductorSwap: Some(ConductorMetal.Iron))),
        soiling:   static s => new WeatheringTrajectory(Colourmaps.Mako,   RoughnessTo: 0.70, MetalnessTo: -1.0, SheenTo: 0.3, Math.Max(0.1, s.Rate), new SlabColumnDelta(CoatRoughnessTo: -1.0, FuzzWeightTo: 0.4, ConductorSwap: None)),
        uvFade:    static f => new WeatheringTrajectory(Colourmaps.Vlag,   RoughnessTo: -1.0, MetalnessTo: -1.0, SheenTo: 0.0, Math.Max(0.1, f.Rate), new SlabColumnDelta(CoatRoughnessTo: 0.50, FuzzWeightTo: -1.0, ConductorSwap: None)));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-effect delta onto the surface#OPENPBR_SLAB SlabStack columns: chalking raises coat_roughness, soiling
// raises fuzz_weight, patina swaps the base ConductorMetal — a -1.0 sentinel leaves the column untouched.
public readonly record struct SlabColumnDelta(double CoatRoughnessTo, double FuzzWeightTo, Option<ConductorMetal> ConductorSwap);

// Terminal is a measured perceptual ramp, never a frozen endpoint: Age samples Terminal.Map(f) at the eased fraction.
// Slab carries the slab-column trajectory the surface#OPENPBR_SLAB SlabStack columns age through once a row lowers.
public readonly record struct WeatheringTrajectory(Colourmap Terminal, double RoughnessTo, double MetalnessTo, double SheenTo, double Rate, SlabColumnDelta Slab);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Weathering {
    // Aging over the flat MaterialParameters row (the row-level path graph#MATERIAL_GRAPH re-evaluates).
    public static Fin<MaterialParameters> Apply(MaterialParameters baseRow, Seq<WeatheringEffect> effects, AgeParameter age, Op key) =>
        effects.Fold(Fin.Succ(baseRow), (acc, effect) => acc.Bind(row => Age(row, effect.Trajectory, age.Value, key)))
            .Bind(aged => MaterialParameters.Of(aged, key));

    // Aging over the lowered surface#OPENPBR_SLAB SlabStack columns: a weathered row ages through the SAME slab
    // algebra a fresh row does, the trajectory's SlabColumnDelta driving the coat/fuzz/base-conductor slabs before
    // the ToLayered collapse rather than a flat base-color lerp; the per-shade-point cavity mask scales the age.
    public static Fin<SlabStack> ApplySlab(SlabStack stack, Seq<WeatheringEffect> effects, AgeParameter age, UnitInterval cavityOcclusion, Op key) =>
        effects.Fold(Fin.Succ(stack), (acc, effect) => acc.Map(s => AgeSlab(s, effect.Trajectory, CavityScaled(age.Value, cavityOcclusion.Value))));

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

    // The slab-column aging: the trajectory delta drives the realized coat/fuzz/base slabs by the eased fraction,
    // a -1.0 delta leaving the column at its fresh value and a ConductorSwap re-grounding the base metal.
    static SlabStack AgeSlab(SlabStack stack, WeatheringTrajectory t, double age) {
        double f = Math.Clamp(Math.Pow(age, t.Rate), 0.0, 1.0);
        return new SlabStack(stack.Slabs.Map(slab => slab.Switch<Slab>(
            fuzz:     fz => t.Slab.FuzzWeightTo < 0.0 ? fz : fz with { Weight = LerpToward(fz.Weight, t.Slab.FuzzWeightTo, f) },
            coat:     c => t.Slab.CoatRoughnessTo < 0.0 ? c : c with { Roughness = LerpToward(c.Roughness, t.Slab.CoatRoughnessTo, f) },
            emission: e => e,
            @base:    b => t.Slab.ConductorSwap.Match(Some: m => b with { Conductor = m, Metalness = LerpToward(b.Metalness, t.MetalnessTo, f) }, None: () => b with { Roughness = LerpToward(b.Roughness, t.RoughnessTo, f) }))));
    }

    // The texture#TEXTURE_UV cavity/ambient-occlusion sample scales the uniform age per shade point so grime
    // accumulates in crevices; the [0,1] occlusion the graph fold supplies (1.0 = fully shadowed crevice, max aging).
    static double CavityScaled(double age, double cavityOcclusion) => Math.Clamp(age * cavityOcclusion, 0.0, 1.0);

    static double LerpToward(double current, double target, double f) => target < 0.0 ? current : current + (target - current) * f;
}
```

## [03]-[RESEARCH]

- [WEATHERING_TRAJECTORY]: the per-effect terminals are measured `Wacton.Unicolour.Datasets` `Colourmap` ramps the fold samples at the eased fraction rather than four hand-keyed endpoints — patina the `Colourmaps.Flare` copper-green ramp (Cu→CuCO₃ chromaticity traverse), oxidation the warm `Colourmaps.Rocket` ramp (Fe₂O₃ rust deepening), soiling the desaturating `Colourmaps.Mako` ramp (grey-grime deposition), UV-fade the `Colourmaps.Vlag` diverging bleach (chromophore desaturation) — each a published perceptually-uniform gradient read through `Map(f)` into `ColourSpace.RgbLinear`, with a sub-linear `age^rate` deposition curve. The four realized trajectories are the seed; a new effect lands its `Colourmap` ramp and rate as one `WeatheringEffect` case. The probe is the per-effect curve calibration against measured aging series, not a re-architecture of the fold.
- [OPENPBR_SLAB_AGING]: REALIZED — `Weathering.ApplySlab` ages the realized `surface#OPENPBR_SLAB` `SlabStack` columns directly: each `WeatheringEffect.Trajectory` carries a `SlabColumnDelta` (CoatRoughnessTo, FuzzWeightTo, ConductorSwap) the fold lerps onto the `Slab.Coat` roughness (chalking raises coat_roughness), the `Slab.Fuzz` weight (soiling raises fuzz_weight for crevice grime), and the `Slab.Base` `ConductorMetal` (patina swaps Cu→verdigris, oxidation Fe→rust) by the eased fraction BEFORE the `ToLayered` collapse, so a weathered material is the realized slab-column delta the renderer shades through one `LayeredBsdf` rather than a flat `MaterialParameters` base-color lerp. A `-1.0` delta sentinel leaves a column at its fresh value (an effect that does not touch that slab); the flat-row `Apply` survives as the row-level path `graph#MATERIAL_GRAPH` re-evaluates. The trajectory aligns to the MaterialX aging-operator node-graph at `interchange#MATERIALX_DOCUMENT`.
- [PROCEDURAL_SOILING_MASK]: REALIZED — spatially-varying weathering drives the aging fraction from a `texture#TEXTURE_UV` cavity/ambient-occlusion sample rather than a uniform age: `ApplySlab` takes a `UnitInterval cavityOcclusion` (the per-shade-point AO the `Texture` graph node supplies) and `CavityScaled` multiplies the age by it, so a fully-shadowed crevice (`occlusion=1`) accumulates grime at the full rate and an exposed face ages less, the same fold reading a per-point fraction. The procedural-soiling mask drives crevice-accumulation grime without a second aging surface, riding the existing graph fold; calibrating the AO-to-age response per effect (linear vs gamma) against measured soiling series is the remaining tuning, not a re-architecture.
```
