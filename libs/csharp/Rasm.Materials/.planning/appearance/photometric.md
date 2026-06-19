# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical graph-emission inputs through the Compute `UnitAlgebra` (illuminance) and author-kernel radiometry (luminance/radiance/luminous-flux), with `EmissionSpectrum` carrying the blackbody/SPD emission color and `PhotometricPolicy` mapping admitted light rows onto the BSDF/graph emission node. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a second color register. The page composes the `Rasm.Compute/units/quantities#QUANTITY_TABLE` `QuantityFamily.Illuminance` admission seam (illuminance only — luminance/radiance/luminous-flux rescale their UnitsNet unit enum to the row's SI-base `CanonicalUnit` and apply the author-kernel efficacy divide until those `QuantityFamily` rows land) and consumes Wacton.Unicolour directly for blackbody/SPD→XYZ→scene-linear emission color, and resolves the canonical `EmissionInput` payload the `graph#MATERIAL_GRAPH` emission node and the `bsdf#LOBE_FAMILY` emission lobe consume — never re-minting a unit owner or a color axis.

## [1]-[INDEX]

One cluster: `[2]-[PHOTOMETRIC]` owns the `PhotometricQuantity` band, the `Photometric` admission fold composing Compute `QuantityFamily.Illuminance`, the `EmissionSpectrum` blackbody/SPD/constant family, the `PhotometricPolicy` light→emission map, and the `EmissionInput` payload.

## [2]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band; `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, radiance, irradiance, luminous-flux, radiant-flux, luminous-intensity, radiant-intensity, nit (= cd/m²)}; emission {`Blackbody`, `Spectral`, `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, UnitPolicy policy, Op key, Guid correlation)` returning the canonical SI scalar; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, UnitPolicy unitPolicy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload (a scene-linear radiance `Unicolour` plus a scalar intensity in canonical SI). The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the Compute `UnitEvidence` seam — distinct identifiers for distinct rails. The composed row coerces through that row's own Compute `QuantityFamily` (illuminance today, the matching family as rows land), the author-kernel rows coerce through the 683 lm/W radiometry, and conversion runs exactly once at admission; interior numerics are raw doubles per the Compute seam law.
- Packages: Rasm.Compute (`QuantityFamily`, `UnitAlgebra`, `UnitPolicy`, `UnitEvidence` — composed at the seam), UnitsNet (the `IlluminanceUnit`/`LuminanceUnit`/`LuminousFluxUnit`/`LuminousIntensityUnit`/`IrradianceUnit`/`PowerUnit` SI-base unit enums the author-kernel `CanonicalUnit` column rescales to), Rasm (project), Wacton.Unicolour (composed for blackbody/SPD→XYZ→scene-linear), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its coercion columns (`Family` the composed `QuantityFamily` row when `Composed`, `CanonicalUnit` the row's SI-base UnitsNet `Enum` the author-kernel rescales to — `IlluminanceUnit.Lux`/`LuminanceUnit.CandelaPerSquareMeter`/`LuminousFluxUnit.Lumen`/`LuminousIntensityUnit.Candela`/`IrradianceUnit.WattPerSquareMeter`/`PowerUnit.Watt`, `Photopic` the luminous-twin marker driving the 683 lm/W divide, `CanonicalIsRadiance` selecting a per-steradian-per-area radiance feeding the emission triple versus a scalar lux/candela/watt intensity, `AliasOf` resolving to another row's coercion with no conversion of its own as `nit` aliases luminance) — never a per-unit type or a `LumenToWatt`/`NitToRadiance` helper family. When the Compute `QuantityFamily` owner lands `Luminance`/`LuminousFlux`/`LuminousIntensity` rows, the matching `PhotometricQuantity` rows set their `Family` column to their OWN new `QuantityFamily` row and flip `Composed: true` — two column edits per row, keyed so `Admit` reaches each flipped row's own family (never illuminance), zero new surface. A new emission model is one `EmissionSpectrum` case.
- Boundary: `Photometric` NEVER re-mints a unit owner. The composed row composes that row's `QuantityFamily.Admit(value, unit, policy, correlation)` (keyed off the `PhotometricQuantity.Family` column, never a hard-wired literal, so a row flipped to `Composed` reaches its own family and not illuminance) returning `Fin<UnitEvidence>`, reads `evidence.CanonicalValue` (the SI base unit), and crosses no quantity type into an interior signature. The luminous↔radiometric coercion is author-kernel and runs a real divide: the luminous efficacy of monochromatic 555 nm radiation is 683 lm/W (the photometric definition), so a photopic quantity in luminous units coerces to its radiometric SI twin by dividing by `Radiometry.LuminousEfficacy` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²) scaled by the source's `EfficacyRatio` (the photopic-band radiant-power fraction; 1.0 at the monochromatic anchor), while a radiometric quantity passes its SI-base magnitude through. The unit `Enum` rescales any sub/multiple to the SI base through `UnitAlgebra.Numeric(value, unit, CanonicalUnit)` — the input unit as source and the row's SI-base `CanonicalUnit` column as target, the real source/SI-base pair UnitsNet's `UnitConverter` resolves, never the identity-unit tautology. `nit` aliases the luminance arm. `EmissionSpectrum.Blackbody` composes Unicolour `new Unicolour(cct, Locus.Blackbody, luminance)` → `.RgbLinear` for the scene-linear color (Planck's law owned by Unicolour's CCT ctor — never re-derived here); `Spectral` composes `new Unicolour(Spd)` → `.Xyz` → scene-linear under `RgbConfiguration.Acescg`; `Constant` carries a pre-resolved scene-linear `Unicolour`. The emission color is constructed through directly-consumed Wacton.Unicolour — no second `ColourSpace` wrapper. A non-finite or negative admission rails to `MaterialFault` (band 2450, parameter case), never a sentinel emission.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    public static readonly PhotometricQuantity Illuminance       = new(0, Some<QuantityFamily>(QuantityFamily.Illuminance), composed: true,  photopic: true,  canonicalIsRadiance: false, canonicalUnit: IlluminanceUnit.Lux);
    public static readonly PhotometricQuantity Luminance         = new(1, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminanceUnit.CandelaPerSquareMeter);
    public static readonly PhotometricQuantity Nit               = new(2, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminanceUnit.CandelaPerSquareMeter, aliasOf: Luminance);
    public static readonly PhotometricQuantity LuminousFlux      = new(3, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: false, canonicalUnit: LuminousFluxUnit.Lumen);
    public static readonly PhotometricQuantity LuminousIntensity = new(4, Option<QuantityFamily>.None,                      composed: false, photopic: true,  canonicalIsRadiance: false, canonicalUnit: LuminousIntensityUnit.Candela);
    public static readonly PhotometricQuantity Radiance          = new(5, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: true,  canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity Irradiance        = new(6, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: false, canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity RadiantFlux       = new(7, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: false, canonicalUnit: PowerUnit.Watt);
    public static readonly PhotometricQuantity RadiantIntensity  = new(8, Option<QuantityFamily>.None,                      composed: false, photopic: false, canonicalIsRadiance: true,  canonicalUnit: PowerUnit.Watt);

    public Option<QuantityFamily> Family { get; }
    public bool Composed { get; }
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }
    public Enum CanonicalUnit { get; }
    public Option<PhotometricQuantity> AliasOf { get; }

    public Fin<double> Coerce(double value, Enum unit, double efficacyRatio) =>
        AliasOf.Match(
            Some: alias => alias.Coerce(value, unit, efficacyRatio),
            None: () => UnitAlgebra.Numeric(value, unit, CanonicalUnit).Map(si =>
                Photopic ? si / (Radiometry.LuminousEfficacy * efficacyRatio) : si));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    public sealed record Blackbody(double Cct, double Luminance) : EmissionSpectrum;

    public sealed record Spectral(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) : EmissionSpectrum;

    public sealed record Constant(double R, double G, double B) : EmissionSpectrum;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static class Radiometry {
    public const double LuminousEfficacy = 683.0;
    public const int    Wavelength555Index = 555;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct EmissionInput(Unicolour Radiance, double Intensity, PhotometricQuantity Source) {
    public static EmissionInput Of(Unicolour sceneLinear, double canonicalIntensity, PhotometricQuantity source) =>
        new(sceneLinear, canonicalIntensity, source);
}

public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, UnitPolicy policy, Op key, Guid correlation, double efficacyRatio = 1.0) {
        if (!double.IsFinite(value) || value < 0.0) { return MaterialFault.Parameter(key, $"<photometric-non-finite:{quantity.Key}:{value:R}>"); }
        return quantity.Family.Match(
            Some: family => family.Admit(value, unit, policy, correlation).Map(static evidence => evidence.CanonicalValue),
            None: () => quantity.Coerce(value, unit, efficacyRatio));
    }

    public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, UnitPolicy unitPolicy, Op key, Guid correlation) =>
        from canonical in Admit(quantity, value, unit, unitPolicy, key, correlation, policy.EfficacyRatio)
        from color in SceneLinear(policy.Spectrum, key)
        select EmissionInput.Of(Expose(color, policy.Exposure), canonical, quantity);

    private static Unicolour Expose(Unicolour colour, double exposure) {
        var rgb = colour.RgbLinear;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R * exposure, rgb.G * exposure, rgb.B * exposure);
    }

    private static Fin<Unicolour> SceneLinear(EmissionSpectrum spectrum, Op key) =>
        spectrum.Switch(
            state: key,
            blackbody: static (k, bb) => double.IsFinite(bb.Cct) && bb.Cct > 0.0
                ? Gate(new Unicolour(bb.Cct, Locus.Blackbody, bb.Luminance), k)
                : MaterialFault.Parameter(k, $"<photometric-blackbody-cct:{bb.Cct:R}>"),
            spectral: static (k, s) => new Spd(s.StartNm, s.IntervalNm, s.Coefficients.ToArray()) is { IsValid: true } spd
                ? Gate(new Unicolour(new Configuration(RgbConfiguration.Acescg), spd), k)
                : MaterialFault.Parameter(k, $"<photometric-spd-interval:{s.IntervalNm}>"),
            constant: static (k, c) => Gate(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, c.R, c.G, c.B), k));

    private static Fin<Unicolour> Gate(Unicolour colour, Op key) {
        var rgb = colour.RgbLinear;
        return double.IsFinite(rgb.R) && double.IsFinite(rgb.G) && double.IsFinite(rgb.B)
            ? Fin.Succ(colour)
            : MaterialFault.Gamut(key, "<emission-non-finite-rgb>");
    }
}
```

## [3]-[RESEARCH]

- [LUMINOUS_EFFICACY_BAND]: the 683 lm/W efficacy is exact only at 555 nm; a broadband source's luminous↔radiometric ratio is the photopic-weighted integral of its SPD against the CIE V(λ) luminosity function. The author-kernel `PhotometricQuantity.Coerce` divides by the 683 constant as the monochromatic-peak anchor scaled by `PhotometricPolicy.EfficacyRatio` — the per-source radiant-power fraction in the photopic band, 1.0 at the anchor and <1.0 for a broadband emitter. The ratio is a real policy column threaded into the coercion today; a spectral-source consumer that integrates V(λ) supplies the exact ratio per source without a new surface.
- [QUANTITYFAMILY_GROWTH]: `Luminance`/`LuminousFlux`/`LuminousIntensity` are absent from the Compute `QuantityFamily` owner today (only `Illuminance` exists). Admitting them as composed rows requires landing one `QuantityFamily` row each on the Compute owner first (per its Growth rule). Until then the matching `PhotometricQuantity` rows carry `Family = Option.None` and `Composed: false`, riding the author-kernel `Coerce`. The flip is row-local: set the row's `Family` column to its own new `QuantityFamily` row (keyed per-row, never the illuminance literal) and `Composed: true`; the per-row `Family` keying means `Admit` reaches each flipped row's own family, never mis-routing a luminance to illuminance. The probe is the row-by-row flip, not a re-architecture. The seam is `Rasm.Compute/units/quantities#QUANTITY_TABLE`.
