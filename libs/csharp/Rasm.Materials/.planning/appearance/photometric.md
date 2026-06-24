# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical graph-emission inputs through the in-folder `MaterialUnits` UnitsNet boundary (the SI-base rescale plus the illuminance family-membership gate) and author-kernel radiometry (the 683 lm/W luminous↔radiometric efficacy divide), with `EmissionSpectrum` carrying the blackbody/daylight-locus CCT and SPD emission color and `PhotometricPolicy` mapping admitted light rows onto the BSDF/graph emission node. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a second color register. The page admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner (`UnitConverter.TryConvert` for the SI-base rescale, the `UnitsNet.Illuminance` family the illuminance row's membership gate) — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner, the seam the Compute units owner and its `ARCHITECTURE [04]` enshrine — and consumes Wacton.Unicolour directly for blackbody/SPD→XYZ→scene-linear emission color, resolving the canonical `EmissionInput` payload the `graph#MATERIAL_GRAPH` emission node and the `bsdf#LOBE_FAMILY` emission lobe consume — never re-minting a unit owner or a color axis.

## [01]-[INDEX]

- [01]-[PHOTOMETRIC]: the `PhotometricQuantity` band, the in-folder `MaterialUnits` UnitsNet boundary (SI-base rescale + illuminance family gate + `UnitEvidence` receipt), the `Photometric` admission fold, the `EmissionSpectrum` blackbody/SPD/constant family, the `PhotometricPolicy` light→emission map, and the `EmissionInput` payload.

## [02]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band; `MaterialUnits` the in-folder UnitsNet boundary (SI-base numeric rescale + illuminance family-membership gate + `UnitEvidence` receipt); `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, radiance, irradiance, luminous-flux, radiant-flux, luminous-intensity, radiant-intensity, nit (= cd/m²)}; emission {`Blackbody` (a CCT source over a `Locus` discriminant — Planckian or CIE Daylight), `Spectral`, `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation)` returning the canonical SI scalar; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload (a scene-linear radiance `Unicolour` plus a scalar intensity in canonical SI). The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the in-folder `MaterialUnits.UnitEvidence` receipt — distinct identifiers for distinct rails. The illuminance row gates its quantity through the in-folder `UnitsNet.Illuminance` family and rescales through `MaterialUnits.Coerce`, the radiometric/luminous rows coerce through the same `MaterialUnits.Coerce` SI-base rescale plus the author-kernel 683 lm/W radiometry, and conversion runs exactly once at admission; interior numerics are raw doubles per the BOUNDARY_ADMISSION law.
- Packages: UnitsNet (admitted IN-FOLDER through the `MaterialUnits` owner — `UnitConverter.TryConvert` the non-throwing SI-base rescale, the `UnitsNet.Illuminance` family + `Illuminance.BaseUnit` the illuminance row's membership gate, the `IlluminanceUnit`/`LuminanceUnit`/`LuminousFluxUnit`/`LuminousIntensityUnit`/`IrradianceUnit`/`PowerUnit` SI-base unit enums the `CanonicalUnit` column rescales to; catalogued in `.api/api-unitsnet.md`), Rasm (project), Wacton.Unicolour (composed for blackbody/daylight-locus CCT→scene-linear and SPD→XYZ→scene-linear; the `Locus.Blackbody`/`Locus.Daylight` temperature loci, the `new Unicolour(double cct, Locus, double luminance)` ctor, and the `DominantWavelength`/`ExcitationPurity`/`Wxy` chromaticity projection), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its coercion columns (`Family` the in-folder `UnitsNet` quantity `QuantityInfo` whose membership gates a measured-twin row, `CanonicalUnit` the row's SI-base UnitsNet `Enum` the author-kernel rescales to — `IlluminanceUnit.Lux`/`LuminanceUnit.CandelaPerSquareMeter`/`LuminousFluxUnit.Lumen`/`LuminousIntensityUnit.Candela`/`IrradianceUnit.WattPerSquareMeter`/`PowerUnit.Watt`, `Photopic` the luminous-twin marker driving the 683 lm/W divide, `CanonicalIsRadiance` selecting a per-steradian-per-area radiance feeding the emission triple versus a scalar lux/candela/watt intensity, `AliasOf` resolving to another row's coercion with no conversion of its own as `nit` aliases luminance) — never a per-unit type or a `LumenToWatt`/`NitToRadiance` helper family. A luminous/radiometric row that wants the same UnitsNet family-membership gate the illuminance row carries sets its `Family` column to its own `UnitsNet` `QuantityInfo` (`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`) — one column per row, the gate the in-folder `MaterialUnits` already owns, zero new surface and never a reach DOWN to the Compute units owner. A new emission model is one `EmissionSpectrum` case; a new temperature locus is one `Locus` value on the `Blackbody` discriminant column (the Planckian and CIE Daylight loci both ride the one `Blackbody` case), never a parallel daylight emission case.
- Boundary: `Photometric` NEVER re-mints a unit owner — it admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner (the strata-acyclic AEC-domain owns its own unit boundary, the seam `Rasm.Compute` `ARCHITECTURE [04]` enshrines). The illuminance row gates its quantity through the in-folder `MaterialUnits.Admit(UnitsNet.Illuminance.Info, value, unit, correlation)` returning `Fin<UnitEvidence>` — the `UnitsNet.Illuminance` family membership and `Dimensions.Equals(Illuminance.Info.BaseDimensions)` the gate, `Illuminance.BaseUnit` (`IlluminanceUnit.Lux`) the SI base — reads `evidence.CanonicalValue` (the SI base magnitude), and crosses no quantity type into an interior signature. The luminous↔radiometric coercion is author-kernel and runs a real divide: the luminous efficacy of monochromatic 555 nm radiation is 683 lm/W (the photometric definition), so a photopic quantity in luminous units coerces to its radiometric SI twin by dividing by `Radiometry.LuminousEfficacy` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²) scaled by the source's `EfficacyRatio` (the photopic-band radiant-power fraction; 1.0 at the monochromatic anchor), while a radiometric quantity passes its SI-base magnitude through. The unit `Enum` rescales any sub/multiple to the SI base through `MaterialUnits.Coerce(value, unit, CanonicalUnit)` — the input unit as source and the row's SI-base `CanonicalUnit` column as target, the real source/SI-base pair `UnitConverter.TryConvert` resolves non-throwing, never the identity-unit tautology. `nit` aliases the luminance arm. `EmissionSpectrum.Blackbody` is a CCT source over a `Locus` discriminant column — `new Unicolour(cct, bb.Locus, luminance)` → `.RgbLinear` for the scene-linear color resolves the CCT on either the Planckian (`Locus.Blackbody`) or the CIE Daylight (`Locus.Daylight`, the D-series) locus (Planck's law and the daylight-locus polynomial both owned by Unicolour's CCT ctor — never re-derived here), so a daylight-correlated source (D50–D75 interior daylighting) resolves on the physically-correct daylight locus rather than the Planckian one, never a parallel daylight emission case; the CCT guard is per-locus through `Radiometry.LocusCctValid` — any finite positive CCT on the Planckian arm, the roughly `DaylightCctMinKelvin`–`DaylightCctMaxKelvin` (4000–25000 K) D-series range on the daylight arm, an out-of-range daylight CCT railing `MaterialFault.Parameter` the way the blackbody CCT guard does. `Spectral` composes `new Unicolour(Spd)` → `.Xyz` → scene-linear under `RgbConfiguration.Acescg`; `Constant` carries a pre-resolved scene-linear `Unicolour`. The resolved emission color projects its `DominantWavelength` (from `Wxy.W`) and `ExcitationPurity` (from `Wxy.X`) onto the `EmissionInput` receipt so a daylight-correlated source reports its dominant wavelength and excitation purity beside the scene-linear radiance, the chromaticity readout composed from the Unicolour `Wxy` surface, never re-derived. The emission color is constructed through directly-consumed Wacton.Unicolour — no second `ColourSpace` wrapper. A non-finite or negative admission rails to `MaterialFault` (band 2450, parameter case), never a sentinel emission.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    public static readonly PhotometricQuantity Illuminance       = new(0, Some(UnitsNet.Illuminance.Info), photopic: true,  canonicalIsRadiance: false, canonicalUnit: IlluminanceUnit.Lux);
    public static readonly PhotometricQuantity Luminance         = new(1, Option<QuantityInfo>.None,       photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminanceUnit.CandelaPerSquareMeter);
    public static readonly PhotometricQuantity Nit               = new(2, Option<QuantityInfo>.None,       photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminanceUnit.CandelaPerSquareMeter, aliasOf: Luminance);
    public static readonly PhotometricQuantity LuminousFlux      = new(3, Option<QuantityInfo>.None,       photopic: true,  canonicalIsRadiance: false, canonicalUnit: LuminousFluxUnit.Lumen);
    public static readonly PhotometricQuantity LuminousIntensity = new(4, Option<QuantityInfo>.None,       photopic: true,  canonicalIsRadiance: false, canonicalUnit: LuminousIntensityUnit.Candela);
    public static readonly PhotometricQuantity Radiance          = new(5, Option<QuantityInfo>.None,       photopic: false, canonicalIsRadiance: true,  canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity Irradiance        = new(6, Option<QuantityInfo>.None,       photopic: false, canonicalIsRadiance: false, canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity RadiantFlux       = new(7, Option<QuantityInfo>.None,       photopic: false, canonicalIsRadiance: false, canonicalUnit: PowerUnit.Watt);
    public static readonly PhotometricQuantity RadiantIntensity  = new(8, Option<QuantityInfo>.None,       photopic: false, canonicalIsRadiance: true,  canonicalUnit: PowerUnit.Watt);

    // The in-folder UnitsNet family-membership gate (Some -> the admitted row gates dimensions through this
    // QuantityInfo, illuminance today; None -> a pure MaterialUnits.Coerce SI-base rescale). Never a Compute
    // QuantityFamily: the strata-acyclic AEC-domain owns its own unit boundary.
    public Option<QuantityInfo> Family { get; }
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }
    public Enum CanonicalUnit { get; }
    public Option<PhotometricQuantity> AliasOf { get; }

    public Fin<double> Coerce(double value, Enum unit, double efficacyRatio, Op key) =>
        AliasOf.Match(
            Some: alias => alias.Coerce(value, unit, efficacyRatio, key),
            None: () => MaterialUnits.Coerce(value, unit, CanonicalUnit, key).Map(si =>
                Photopic ? si / (Radiometry.LuminousEfficacy * efficacyRatio) : si));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    public sealed record Blackbody(double Cct, double Luminance, Locus Locus = Locus.Blackbody) : EmissionSpectrum;

    public sealed record Spectral(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) : EmissionSpectrum;

    public sealed record Constant(double R, double G, double B) : EmissionSpectrum;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static class Radiometry {
    public const double LuminousEfficacy = 683.0;
    public const int    Wavelength555Index = 555;
    public const double DaylightCctMinKelvin = 4000.0;
    public const double DaylightCctMaxKelvin = 25000.0;

    public static bool LocusCctValid(Locus locus, double cct) =>
        locus == Locus.Daylight
            ? cct is >= DaylightCctMinKelvin and <= DaylightCctMaxKelvin
            : double.IsFinite(cct) && cct > 0.0;
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The in-folder UnitsNet admission boundary: Materials owns its OWN unit coercion through the directly-pinned
// UnitsNet package, never a Rasm.Compute project reference (the acyclic strata forbids the AEC->app-platform
// edge; the Rasm.Compute Symbolic/units QuantityFamily owner is Compute-internal). Coerce is the non-throwing
// SI-base rescale (UnitConverter.TryConvert); Admit gates a quantity against a UnitsNet family's dimensions and
// emits the UnitEvidence receipt the illuminance row threads. Conversion runs exactly once at admission and the
// receipt carries plain strings/doubles, so no UnitsNet type crosses an interior signature or a wire.
public readonly record struct UnitEvidence(string Family, string OriginalUnit, double OriginalValue, string CanonicalUnit, double CanonicalValue, Guid CorrelationId) {
    public static UnitEvidence From(IQuantity quantity, Enum canonical, Guid correlation) =>
        new(quantity.QuantityInfo.Name, quantity.Unit.ToString(), quantity.As(quantity.Unit), canonical.ToString(), quantity.As(canonical), correlation);
}

public static class MaterialUnits {
    public static Fin<double> Coerce(double value, Enum from, Enum to, Op key) =>
        UnitConverter.TryConvert(value, from, to, out double converted)
            ? Fin<double>.Succ(converted)
            : MaterialFault.Parameter(key, $"<unit-convert:{from}->{to}>");

    public static Fin<UnitEvidence> Admit(QuantityInfo family, double value, Enum unit, Op key, Guid correlation) =>
        Quantity.TryFrom(value, unit, out IQuantity? typed) && typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)
            ? Fin<UnitEvidence>.Succ(UnitEvidence.From(typed, family.BaseUnitInfo.Value, correlation))
            : MaterialFault.Parameter(key, $"<unit-admit:{unit}:outside:{family.Name}>");
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct EmissionInput(Unicolour Radiance, double Intensity, PhotometricQuantity Source, double DominantWavelengthNm, double ExcitationPurity) {
    public static EmissionInput Of(Unicolour sceneLinear, double canonicalIntensity, PhotometricQuantity source) =>
        new(sceneLinear, canonicalIntensity, source, sceneLinear.DominantWavelength, sceneLinear.ExcitationPurity);
}

public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    public static Fin<double> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0) {
        if (!double.IsFinite(value) || value < 0.0) { return MaterialFault.Parameter(key, $"<photometric-non-finite:{quantity.Key}:{value:R}>"); }
        return quantity.Family.Match(
            Some: family => MaterialUnits.Admit(family, value, unit, key, correlation).Map(static evidence => evidence.CanonicalValue),
            None: () => quantity.Coerce(value, unit, efficacyRatio, key));
    }

    public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation) =>
        from canonical in Admit(quantity, value, unit, key, correlation, policy.EfficacyRatio)
        from color in SceneLinear(policy.Spectrum, key)
        select EmissionInput.Of(Expose(color, policy.Exposure), canonical, quantity);

    private static Unicolour Expose(Unicolour colour, double exposure) {
        var rgb = colour.RgbLinear;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R * exposure, rgb.G * exposure, rgb.B * exposure);
    }

    private static Fin<Unicolour> SceneLinear(EmissionSpectrum spectrum, Op key) =>
        spectrum.Switch(
            state: key,
            blackbody: static (k, bb) => Radiometry.LocusCctValid(bb.Locus, bb.Cct)
                ? Gate(new Unicolour(bb.Cct, bb.Locus, bb.Luminance), k)
                : MaterialFault.Parameter(k, $"<photometric-{(bb.Locus == Locus.Daylight ? "daylight" : "blackbody")}-cct:{bb.Cct:R}>"),
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

## [03]-[RESEARCH]

- [LUMINOUS_EFFICACY_BAND]: the 683 lm/W efficacy is exact only at 555 nm; a broadband source's luminous↔radiometric ratio is the photopic-weighted integral of its SPD against the CIE V(λ) luminosity function. The author-kernel `PhotometricQuantity.Coerce` divides by the 683 constant as the monochromatic-peak anchor scaled by `PhotometricPolicy.EfficacyRatio` — the per-source radiant-power fraction in the photopic band, 1.0 at the anchor and <1.0 for a broadband emitter. The ratio is a real policy column threaded into the coercion today; a spectral-source consumer that integrates V(λ) supplies the exact ratio per source without a new surface.
- [DAYLIGHT_LOCUS_EMISSION]: REALIZED — the `EmissionSpectrum.Blackbody` case carries a `Locus` discriminant column so a CCT source resolves on either the Planckian (`Locus.Blackbody`) or the CIE Daylight (`Locus.Daylight`, D-series) locus through the one `new Unicolour(double cct, Locus, double luminance)` ctor, the daylight locus the physically-correct curve for D50–D75 interior daylighting. The per-locus CCT guard `Radiometry.LocusCctValid` admits any finite positive CCT on the Planckian arm and the roughly 4000–25000 K D-series range on the daylight arm, an out-of-range daylight CCT railing `MaterialFault.Parameter`. The `EmissionInput` receipt projects `DominantWavelength` (from the resolved colour's `Wxy.W`) and `ExcitationPurity` (from `Wxy.X`) so a daylight-correlated source reports its chromaticity readout. The Unicolour daylight-locus polynomial owns the chromaticity construction; this page composes it, never re-deriving a daylight curve.
- [FAMILY_GATE_GROWTH]: the illuminance row gates its quantity through the in-folder `UnitsNet.Illuminance.Info` family membership (the `MaterialUnits.Admit` dimensions check) and emits a `UnitEvidence` receipt; every luminous/radiometric row rides the pure `MaterialUnits.Coerce` SI-base rescale plus the author-kernel efficacy divide. UnitsNet already publishes the `Luminance`/`LuminousFlux`/`LuminousIntensity`/`Irradiance` quantity families (catalogued in `.api/api-unitsnet.md`), so a row that wants the same family-membership gate the illuminance row carries sets its `Family` column to its own `QuantityInfo` (`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`) — one column per row, no upstream wait and no reach DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner (the acyclic strata forbids the AEC->app-platform edge; the in-folder UnitsNet pin owns the boundary). The strata-correct admission is entirely Materials-owned today; the seam the Compute units owner enshrines is "AEC admits UnitsNet in-folder," realized here.
