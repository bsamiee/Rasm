# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical radiometric graph-emission inputs through the in-folder `MaterialUnits` UnitsNet boundary (the SI-base rescale plus the per-row family-membership gate) and the author-kernel 683 lm/W luminous↔radiometric efficacy divide, with `EmissionSpectrum` carrying the blackbody/daylight-locus CCT, the CIE standard-illuminant, the measured-SPD, and the constant emission color, and `PhotometricPolicy` mapping admitted light rows onto the BSDF/graph emission node. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, a parallel `nit` quantity (`nit` is `LuminanceUnit.Nit`, a unit of the luminance row), or a second color register. The page admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner (`UnitConverter.TryConvert` for the SI-base rescale, the per-quantity `QuantityInfo` family the membership gate) — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner, the seam the Compute units owner and its `ARCHITECTURE [04]` enshrine — and consumes Wacton.Unicolour directly for blackbody/SPD/illuminant→XYZ→scene-linear emission color, resolving the canonical `EmissionInput` payload the `graph#MATERIAL_GRAPH` emission node and the `bsdf#LOBE_FAMILY` emission lobe consume — never re-minting a unit owner or a color axis.

## [01]-[INDEX]

- [01]-[PHOTOMETRIC]: the `PhotometricQuantity` band, the in-folder `MaterialUnits` UnitsNet boundary (SI-base rescale + per-row family gate + `UnitEvidence` receipt), the unified gate-then-radiometric-divide `Admit` on the band row, the `Photometric` admission fold, the `EmissionSpectrum` blackbody/illuminant/spectral/constant family, the `PhotometricPolicy` light→emission map, and the `EmissionInput` payload carrying the scene-linear radiance, canonical-SI intensity, chromaticity readout, and unit provenance.

## [02]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band owning the per-row family gate, SI-base rescale, and radiometric divide; `MaterialUnits` the in-folder UnitsNet boundary (SI-base numeric rescale + `QuantityInfo` family-membership gate + `UnitEvidence` receipt); `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, luminous-flux, luminous-intensity, irradiance, radiance, radiant-intensity, radiant-flux}; emission {`Blackbody` (a CCT source over a `Locus` discriminant — Planckian or CIE Daylight), `Standard` (a CIE standard illuminant — D-series daylight, A incandescent, F-series fluorescent), `Spectral` (a measured SPD), `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<UnitEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0)` returning the `UnitEvidence` receipt whose `CanonicalValue` is the faithful SI-base UNIT magnitude (lux/cd·m⁻²/lm/cd/W·m⁻²/W) and whose `RadiometricSi` is the radiometric scalar the emission node consumes (luminous rows gated against their UnitsNet family, rescaled to SI base, then divided to their radiometric twin; radiometric rows carry `RadiometricSi == CanonicalValue`) — the unit and the radiometric value kept on distinct fields so the receipt never contradicts itself; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload (a scene-linear radiance `Unicolour`, the radiometric-SI intensity, the chromaticity readout, and the threaded `UnitEvidence` provenance). The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the `MaterialUnits.UnitEvidence` receipt onto the payload — distinct identifiers for distinct rails. Every row gates its dimensional family (where UnitsNet publishes one) and rescales through `MaterialUnits.Coerce` to its SI-base `CanonicalUnit` in ONE row method, conversion runs exactly once at admission, and interior numerics are raw doubles per the BOUNDARY_ADMISSION law.
- Packages: UnitsNet (admitted IN-FOLDER through the `MaterialUnits` owner — `UnitConverter.TryConvert` the non-throwing SI-base rescale, `Quantity.TryFrom` the dynamic-quantity construction the family gate inspects, the `QuantityInfo`/`BaseDimensions`/`QuantityInfo.Name` family-membership surface, the `IlluminanceUnit`/`LuminanceUnit`/`LuminousFluxUnit`/`LuminousIntensityUnit`/`IrradianceUnit`/`PowerUnit` SI-base unit enums the `CanonicalUnit` column rescales to; catalogued in `.api/api-unitsnet.md`), Rasm (project — `Op` boundary key, `MaterialFault` band), Wacton.Unicolour (composed for blackbody/daylight-locus CCT→scene-linear, CIE standard-illuminant→scene-linear, and SPD→XYZ→scene-linear; the `Locus.Blackbody`/`Locus.Daylight` temperature loci, the `Illuminant.D65`/`A`/`F2`… statics + `Illuminant.GetWhitePoint(Observer)`/`WhitePoint.Chromaticity` chromaticity projection, the `new Unicolour(double cct, Locus, double luminance)`/`new Unicolour(Chromaticity, double luminance)`/`new Unicolour(Configuration, Spd)` ctors, and the `DominantWavelength`/`ExcitationPurity`/`Wxy` chromaticity projection), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its coercion columns (`Family` the UnitsNet quantity `QuantityInfo` whose membership gates the row — `Illuminance.Info`/`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`/`Power.Info` where UnitsNet publishes the quantity, `None` for the per-steradian radiance/radiant-intensity rows UnitsNet has no quantity for; `CanonicalUnit` the row's SI-base UnitsNet `Enum` the rescale targets — `IlluminanceUnit.Lux`/`LuminanceUnit.CandelaPerSquareMeter`/`LuminousFluxUnit.Lumen`/`LuminousIntensityUnit.Candela`/`IrradianceUnit.WattPerSquareMeter`/`PowerUnit.Watt`, the per-steradian rows borrowing `IrradianceUnit`/`PowerUnit` for the prefix rescale since steradian carries no SI prefix; `Photopic` the luminous-twin marker driving the 683 lm/W divide, `CanonicalIsRadiance` marking a per-steradian-per-area radiance row versus a scalar lux/candela/watt intensity row — the discriminant the emission node reads through `EmissionInput.Source.CanonicalIsRadiance` to decide whether the `Intensity` scalar is a directly-usable emitted radiance W/(sr·m²) or a flux/irradiance/power needing area/solid-angle normalization before it becomes radiance) — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a parallel `nit` quantity. A sub/multiple of an existing unit is the SAME row called with that unit `Enum` (`Luminance` with `LuminanceUnit.Nit`, `Illuminance` with `IlluminanceUnit.Kilolux`), the rescale resolving it to the SI base — never a parallel alias row. A new emission model is one `EmissionSpectrum` case; a new temperature locus is one `Locus` value on the `Blackbody` discriminant (Planckian and CIE Daylight both ride the one `Blackbody` case), and a new named illuminant is one `Illuminant` static carried on the one `Standard` case (D-series, A, F-series all ride it), never a parallel daylight or fluorescent emission case.
- Boundary: `Photometric` NEVER re-mints a unit owner — it admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner (the strata-acyclic AEC-domain owns its own unit boundary, the seam `Rasm.Compute` `ARCHITECTURE [04]` enshrines). Every luminous/radiometric row that UnitsNet publishes a quantity for gates its quantity through `MaterialUnits.Admit(row.Family, value, unit, key, correlation)` returning `Fin<UnitEvidence>` — the `QuantityInfo` family membership the gate (`typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)`, the name check load-bearing because UnitsNet collapses lumen/candela/cd·m⁻² to one luminous-intensity dimension), the row's `BaseUnitInfo.Value` the SI base — populating `evidence.CanonicalValue` with the SI-base unit magnitude, crossing no quantity type into an interior signature. The per-steradian `Radiance`/`RadiantIntensity` rows carry `Family = None` (UnitsNet has no `Radiance`/`RadiantIntensity` quantity) and take the pure `MaterialUnits.Coerce` SI-base rescale over the borrowed `IrradianceUnit`/`PowerUnit` prefixes. The gate and the radiometric divide COMPOSE in the one `PhotometricQuantity.Admit` row method rather than alternating: the row admits or rescales to its SI-base `CanonicalValue`, THEN — for a photopic row — derives `RadiometricSi` by dividing to its radiometric twin (a radiometric row sets `RadiometricSi == CanonicalValue`), so the receipt carries both the faithful unit magnitude and the radiometric value the emission node reads. The luminous↔radiometric coercion runs a real divide: the luminous efficacy of monochromatic 555 nm radiation is 683 lm/W (the photometric definition), so a photopic quantity in luminous units coerces to its radiometric SI twin by dividing the SI-base magnitude by `Radiometry.LuminousEfficacy` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²) scaled by the source's `EfficacyRatio` (the photopic-band radiant-power fraction; 1.0 at the monochromatic anchor), while a radiometric quantity passes its SI-base magnitude through. The `EmissionSpectrum.Blackbody` case is a CCT source over a `Locus` discriminant — `new Unicolour(cct, bb.Locus, luminance)` → `.RgbLinear` resolves the CCT on either the Planckian (`Locus.Blackbody`) or the CIE Daylight (`Locus.Daylight`, the D-series) locus (Planck's law and the daylight-locus polynomial both owned by Unicolour's CCT ctor — never re-derived here), so a daylight-correlated source (D50–D75 interior daylighting) resolves on the physically-correct daylight locus rather than the Planckian one, never a parallel daylight emission case; the CCT guard is per-locus through `Radiometry.LocusCctValid` — any finite positive CCT on the Planckian arm, the roughly `DaylightCctMinKelvin`–`DaylightCctMaxKelvin` (4000–25000 K) D-series range on the daylight arm, an out-of-range daylight CCT railing `MaterialFault.Parameter` the way the blackbody CCT guard does. `EmissionSpectrum.Standard` carries a CIE standard illuminant — `source.Illuminant.GetWhitePoint(Observer.Degree2).Chromaticity` → `new Unicolour(chromaticity, luminance)` → `.RgbLinear` resolves the named illuminant's scene-emission chromaticity (D65 north-sky daylight, A incandescent, the F-series fluorescent and D-series daylight statics Unicolour ships), so a fixture specified by CIE illuminant resolves to its standard chromaticity through the public white-point projection rather than a re-keyed triple — the `Illuminant.Spd` is `internal` so the case lowers through the public `GetWhitePoint`/`Chromaticity` surface, the chromaticity the renderer's light color. `Spectral` composes `new Unicolour(new Configuration(RgbConfiguration.Acescg), Spd)` → `.RgbLinear` under the scene-linear working space; `Constant` carries a pre-resolved scene-linear `Unicolour`. The resolved emission color projects its `DominantWavelength` (from `Wxy.W`) and `ExcitationPurity` (from `Wxy.X`) onto the `EmissionInput` receipt so a daylight- or illuminant-correlated source reports its dominant wavelength and excitation purity beside the scene-linear radiance, the chromaticity readout composed from the Unicolour `Wxy` surface, never re-derived. The `UnitEvidence` receipt the magnitude admission produces rides onto `EmissionInput.Provenance` so a consumer distinguishes a gated-and-rescaled luminous admission from a raw radiometric passthrough — the receipt is load-bearing on the payload, never built and dropped. The emission color is constructed through directly-consumed Wacton.Unicolour — no second `ColourSpace` wrapper. A non-finite or negative admission rails `MaterialFault.Parameter` (band 2450), never a sentinel emission; a non-finite emission RGB rails `MaterialFault.Gamut`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;                       // Op (the boundary-admission key)
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault (band 2450) declared on bsdf#SHADING_FRAME, composed here
using Rasm.Materials.Appearance.Graph;   // PortValue (the scene-linear Acescg Configuration owner — PortValue.SceneLinear)
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Photometric;

// MaterialFault (band 2450) is declared on bsdf#SHADING_FRAME and composed here; PortValue.SceneLinear is the
// Acescg Configuration declared on graph#MATERIAL_GRAPH and composed here — no second fault band, no second
// scene-linear working space.

// --- [TYPES] -------------------------------------------------------------------------------
// One light-unit band: each row binds its UnitsNet family gate (Some where UnitsNet publishes the quantity, None
// for the per-steradian radiance/radiant-intensity rows UnitsNet has no quantity for), its SI-base CanonicalUnit,
// and the Photopic marker driving the 683 lm/W radiometric divide. The Admit row method composes gate-or-rescale
// THEN the radiometric divide in one place, so a luminous row is BOTH dimensionally admitted AND lowered to its
// radiometric twin — never one or the other. `nit` is NOT a row: it is LuminanceUnit.Nit on the Luminance row.
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    public static readonly PhotometricQuantity Illuminance       = new(0, Some<QuantityInfo>(UnitsNet.Illuminance.Info),       photopic: true,  canonicalIsRadiance: false, canonicalUnit: IlluminanceUnit.Lux);
    public static readonly PhotometricQuantity Luminance         = new(1, Some<QuantityInfo>(UnitsNet.Luminance.Info),         photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminanceUnit.CandelaPerSquareMeter);
    public static readonly PhotometricQuantity LuminousFlux      = new(2, Some<QuantityInfo>(UnitsNet.LuminousFlux.Info),      photopic: true,  canonicalIsRadiance: false, canonicalUnit: LuminousFluxUnit.Lumen);
    public static readonly PhotometricQuantity LuminousIntensity = new(3, Some<QuantityInfo>(UnitsNet.LuminousIntensity.Info), photopic: true,  canonicalIsRadiance: true,  canonicalUnit: LuminousIntensityUnit.Candela);
    public static readonly PhotometricQuantity Irradiance        = new(4, Some<QuantityInfo>(UnitsNet.Irradiance.Info),        photopic: false, canonicalIsRadiance: false, canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity RadiantFlux       = new(5, Some<QuantityInfo>(UnitsNet.Power.Info),             photopic: false, canonicalIsRadiance: false, canonicalUnit: PowerUnit.Watt);
    public static readonly PhotometricQuantity Radiance          = new(6, Option<QuantityInfo>.None,                           photopic: false, canonicalIsRadiance: true,  canonicalUnit: IrradianceUnit.WattPerSquareMeter);
    public static readonly PhotometricQuantity RadiantIntensity  = new(7, Option<QuantityInfo>.None,                           photopic: false, canonicalIsRadiance: true,  canonicalUnit: PowerUnit.Watt);

    // Some -> the row gates dimensions through this QuantityInfo (the name check distinguishes the lumen/candela/
    // cd·m⁻² rows UnitsNet collapses to one luminous-intensity dimension); None -> a pure MaterialUnits.Coerce
    // SI-base rescale over the borrowed per-area/power unit prefixes. Never a Compute QuantityFamily.
    public Option<QuantityInfo> Family { get; }
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }
    public Enum CanonicalUnit { get; }

    // The ONE row admission: gate the dimensional family (or rescale where UnitsNet has no quantity) to the SI-base
    // unit magnitude, then derive the radiometric-twin magnitude for a photopic row. The gate and the radiometric
    // divide COMPOSE — never alternate. The receipt stays unit-faithful (CanonicalUnit names CanonicalValue's unit);
    // RadiometricSi is the separate radiometric magnitude the emission node consumes (== CanonicalValue for a
    // radiometric row, CanonicalValue / (683 · ratio) for a photopic row).
    public Fin<UnitEvidence> Admit(double value, Enum unit, double efficacyRatio, Op key, Guid correlation) =>
        Family
            .Match(
                Some: family => MaterialUnits.Admit(family, value, unit, key, correlation),
                None: () => MaterialUnits.Coerce(value, unit, CanonicalUnit, key)
                    .Map(si => UnitEvidence.Raw(value, unit, si, CanonicalUnit, correlation)))
            .Map(evidence => evidence with {
                RadiometricSi = Photopic ? evidence.CanonicalValue / (Radiometry.LuminousEfficacy * efficacyRatio) : evidence.CanonicalValue });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    public sealed record Blackbody(double Cct, double Luminance, Locus Locus = Locus.Blackbody) : EmissionSpectrum;

    public sealed record Standard(Illuminant Illuminant, double Luminance) : EmissionSpectrum;

    public sealed record Spectral(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) : EmissionSpectrum;

    public sealed record Constant(double R, double G, double B) : EmissionSpectrum;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static class Radiometry {
    public const double LuminousEfficacy = 683.0;   // lm/W at the 555 nm monochromatic photopic peak — the luminous->radiometric divide anchor
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
// SI-base rescale (UnitConverter.TryConvert); Admit gates a quantity against a UnitsNet family's dimensions+name
// and emits the UnitEvidence receipt every gated row threads. Conversion runs exactly once at admission and the
// receipt carries plain strings/doubles, so no UnitsNet type crosses an interior signature or a wire.
// The unit-coercion receipt: CanonicalUnit/CanonicalValue are the faithful SI-base UNIT magnitude (lux/cd·m⁻²/lm/
// cd/W·m⁻²/W); RadiometricSi is the radiometric magnitude derived by the row (the photopic divide applied, or the
// same value for a radiometric row), kept distinct so the receipt never contradicts itself (unit names value).
public readonly record struct UnitEvidence(string Family, string OriginalUnit, double OriginalValue, string CanonicalUnit, double CanonicalValue, double RadiometricSi, Guid CorrelationId) {
    public static UnitEvidence From(IQuantity quantity, Enum canonical, Guid correlation) {
        double si = quantity.As(canonical);
        return new(quantity.QuantityInfo.Name, quantity.Unit.ToString(), quantity.As(quantity.Unit), canonical.ToString(), si, si, correlation);
    }

    // The per-steradian rows have no UnitsNet quantity to read a family name from; the receipt records the borrowed
    // unit-prefix rescale (e.g. kW/(sr·m²) -> W/(sr·m²)) with the row's CanonicalUnit as the SI-base witness.
    public static UnitEvidence Raw(double originalValue, Enum originalUnit, double canonicalValue, Enum canonical, Guid correlation) =>
        new(canonical.GetType().Name, originalUnit.ToString(), originalValue, canonical.ToString(), canonicalValue, canonicalValue, correlation);
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
public readonly record struct EmissionInput(Unicolour Radiance, double Intensity, PhotometricQuantity Source, double DominantWavelengthNm, double ExcitationPurity, UnitEvidence Provenance) {
    public static EmissionInput Of(Unicolour sceneLinear, UnitEvidence canonical, PhotometricQuantity source) =>
        new(sceneLinear, canonical.RadiometricSi, source, sceneLinear.DominantWavelength, sceneLinear.ExcitationPurity, canonical);
}

public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    public static Fin<UnitEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0) =>
        double.IsFinite(value) && value >= 0.0
            ? quantity.Admit(value, unit, efficacyRatio, key, correlation)
            : MaterialFault.Parameter(key, $"<photometric-non-finite:{quantity.Key}:{value:R}>");

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
            standard: static (k, s) => Gate(new Unicolour(s.Illuminant.GetWhitePoint(Observer.Degree2).Chromaticity, s.Luminance), k),
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

- [LUMINOUS_EFFICACY_BAND]: the 683 lm/W efficacy is exact only at 555 nm; a broadband source's luminous↔radiometric ratio is the photopic-weighted integral of its SPD against the CIE V(λ) luminosity function. The author-kernel `PhotometricQuantity.Admit` divides the SI-base magnitude by the 683 constant as the monochromatic-peak anchor scaled by `PhotometricPolicy.EfficacyRatio` — the per-source radiant-power fraction in the photopic band, 1.0 at the anchor and <1.0 for a broadband emitter. The ratio is a real policy column threaded into the row method today; a spectral-source consumer that integrates V(λ) against the `EmissionSpectrum.Spectral` SPD supplies the exact ratio per source without a new surface, the divide composing AFTER the family gate so a dimensionally-admitted luminous quantity is always lowered to its radiometric twin.
- [DAYLIGHT_LOCUS_AND_STANDARD_ILLUMINANT]: REALIZED — the `EmissionSpectrum.Blackbody` case carries a `Locus` discriminant so a CCT source resolves on either the Planckian (`Locus.Blackbody`) or the CIE Daylight (`Locus.Daylight`, D-series) locus through the one `new Unicolour(double cct, Locus, double luminance)` ctor, and the `EmissionSpectrum.Standard` case resolves a CIE standard illuminant (the `Illuminant.D65`/`D50`/`A`/`F2`/`F7`/`F11`… statics Unicolour ships) through the public `Illuminant.GetWhitePoint(Observer.Degree2).Chromaticity` → `new Unicolour(Chromaticity, double luminance)` projection — the named illuminant the most common architectural-lighting source specification (D65 north-sky daylight, A incandescent, the F-series fluorescent), resolved through the public white-point surface because `Illuminant.Spd` is `internal`. The per-locus CCT guard `Radiometry.LocusCctValid` admits any finite positive CCT on the Planckian arm and the roughly 4000–25000 K D-series range on the daylight arm, an out-of-range daylight CCT railing `MaterialFault.Parameter`. The `EmissionInput` receipt projects `DominantWavelength` (from the resolved colour's `Wxy.W`) and `ExcitationPurity` (from `Wxy.X`) so a daylight- or illuminant-correlated source reports its chromaticity readout. The Unicolour daylight-locus polynomial and the standard-illuminant white-point tables own the chromaticity construction; this page composes them, never re-deriving a daylight curve or an illuminant table.
- [FAMILY_GATE_REALIZED]: REALIZED across every gateable row — UnitsNet publishes `Illuminance`/`Luminance`/`LuminousFlux`/`LuminousIntensity`/`Irradiance` quantity families and `Power` (the radiant-flux twin), so each of those rows sets its `Family` column to its own `QuantityInfo` (`UnitsNet.Illuminance.Info`/`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`/`Power.Info`) and gates the measured magnitude through `MaterialUnits.Admit` — the `typed.QuantityInfo.Name == family.Name` check load-bearing because UnitsNet collapses lumen/candela/cd·m⁻² to the one luminous-intensity dimension `(0,0,0,0,0,0,1)`, so a name check distinguishes a flux from an intensity from a luminance the dimension check alone cannot. The per-steradian `Radiance`/`RadiantIntensity` rows carry `Family = None` because UnitsNet has no `Radiance`/`RadiantIntensity` quantity (steradian is dimensionally absent in SI, so radiance shares irradiance's dimension and radiant-intensity shares power's); those rows take the pure `MaterialUnits.Coerce` SI-base rescale over the borrowed `IrradianceUnit`/`PowerUnit` prefixes. The gate and the 683 lm/W radiometric divide compose in the ONE `PhotometricQuantity.Admit` row method — a luminous row is dimensionally admitted, rescaled to its SI base, THEN divided to its radiometric twin, the prior split where the family gate and the divide were mutually-exclusive `Match` arms (admitting illuminance as lux without the divide) collapsed. The strata-correct admission is entirely Materials-owned; the seam the Compute units owner enshrines is "AEC admits UnitsNet in-folder," realized here, never a reach DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner.
