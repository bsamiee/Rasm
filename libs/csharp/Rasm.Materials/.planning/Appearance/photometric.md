# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical radiometric graph-emission inputs through the in-folder `MaterialUnits` UnitsNet boundary and the author-kernel 683 lm/W luminous↔radiometric efficacy divide. Each band row carries ONE closed `Coercion` discriminant — `Gated` (UnitsNet publishes the quantity family: dimension+name gate, then `ToUnit(UnitSystem.SI)` derives BOTH the SI magnitude and the canonical-unit witness) or `Borrowed` (the per-steradian rows UnitsNet has no quantity for: a prefix rescale over the named SI-base enum) — never a parallel family column beside a canonical-unit column where each is meaningful only when the other is absent. `EmissionSpectrum` carries the blackbody/daylight-locus CCT (a non-zero `Duv` planckian offset admitting the ANSI C78.377 binned source), the CIE standard-illuminant, the datasheet chromaticity point, the measured-SPD, and the constant emission color; every arm resolves under the ONE `PortValue.SceneLinear` Acescg working space through the config-explicit `Unicolour` constructors — a default-configuration construction would resolve CCT/illuminant/SPD to sRGB-linear channels and silently re-tag them as AP1, the working-space corruption this page forecloses. `PhotometricPolicy` maps admitted light rows onto the BSDF/graph emission node and carries the CIE standard observer as a policy column. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, a parallel `nit` quantity (`nit` is `LuminanceUnit.Nit`, a unit of the luminance row), or a second color register. The page admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner — and consumes Wacton.Unicolour directly for blackbody/SPD/illuminant→XYZ→scene-linear emission color, resolving the canonical `EmissionInput` payload (scene-linear radiance, radiometric-SI intensity, the `Temperature` CCT+Duv readout, dominant-wavelength/purity chromaticity, relative luminance, gamut-map evidence, and `UnitEvidence` provenance) the `graph#MATERIAL_GRAPH` emission node and the `bsdf#LOBE_FAMILY` emission lobe consume — never re-minting a unit owner or a color axis.

## [01]-[INDEX]

- [01]-[PHOTOMETRIC]: the `PhotometricQuantity` band over the closed `Coercion` gate/rescale discriminant, the in-folder `MaterialUnits` UnitsNet boundary (`UnitSystem.SI` coercion + per-row family gate + `UnitEvidence` receipt), the unified gate-then-radiometric-divide `Admit` on the band row, the `EmissionSpectrum` blackbody/illuminant/chromaticity/spectral/constant family resolved scene-linear config-explicit, the `PhotometricPolicy` light→emission map with the observer policy column, and the `EmissionInput` payload carrying radiance, intensity, the CCT+Duv `Temperature` readout, chromaticity, and unit provenance.

## [02]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band owning the radiometric divide over its `Coercion` column; `Coercion` `[Union]` the closed gate-or-rescale discriminant; `MaterialUnits` the in-folder UnitsNet boundary (family-membership gate + `UnitSystem.SI` coercion + `UnitEvidence` receipt); `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, luminous-flux, luminous-intensity, irradiance, radiance, radiant-intensity, radiant-flux}; coercion {`Gated` (a published UnitsNet `QuantityInfo` family), `Borrowed` (a per-steradian row over a borrowed SI-base unit enum)}; emission {`Blackbody` (a CCT source over a `Locus` discriminant — Planckian or CIE Daylight — with the `Duv` planckian-offset column carrying an ANSI C78.377 binned source), `Standard` (a CIE standard illuminant — D-series daylight, A incandescent, F-series fluorescent), `Chromatic` (a datasheet xy chromaticity point plus luminance), `Spectral` (a measured SPD), `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<UnitEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0)` returning the `UnitEvidence` receipt whose `CanonicalValue` is the faithful SI-base UNIT magnitude (lux/cd·m⁻²/lm/cd/W·m⁻²/W) and whose `RadiometricSi` is the radiometric scalar the emission node consumes (luminous rows gated against their UnitsNet family, coerced to SI base through `ToUnit(UnitSystem.SI)`, then divided to their radiometric twin; radiometric rows carry `RadiometricSi == CanonicalValue`) — the unit and the radiometric value kept on distinct fields so the receipt never contradicts itself; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload. The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the `MaterialUnits.UnitEvidence` receipt onto the payload — distinct identifiers for distinct rails. Conversion runs exactly once at admission and interior numerics are raw doubles per the BOUNDARY_ADMISSION law.
- Packages: UnitsNet (admitted IN-FOLDER through the `MaterialUnits` owner — `Quantity.TryFrom` the dynamic-quantity construction the family gate inspects, the `QuantityInfo`/`BaseDimensions`/`QuantityInfo.Name` family-membership surface, `ToUnit(UnitSystem.SI)` the one SI-base coercion deriving magnitude AND unit witness for every published family, `UnitConverter.TryConvert` the non-throwing prefix rescale for the borrowed per-steradian rows; catalogued in `.api/api-unitsnet.md`), Rasm (project — `Op` boundary key, `MaterialFault` band), Wacton.Unicolour (composed for blackbody/daylight-locus CCT→scene-linear, CIE standard-illuminant→scene-linear, and SPD→XYZ→scene-linear — the config-explicit `new Unicolour(Configuration, double cct, Locus, double luminance)`/`(Configuration, Temperature, double luminance)`/`(Configuration, Chromaticity, double luminance)`/`(Configuration, Spd)` constructors under the Acescg working space; the `Locus.Blackbody`/`Locus.Daylight` temperature loci; the `Illuminant.D65`/`A`/`F2`… statics + `Illuminant.GetWhitePoint(Observer)`/`WhitePoint.Chromaticity` projection; the `Observer.Degree2`/`Degree10` standard observers; the `Temperature` CCT+Duv readout record with `IsValid`/`IsHighAccuracy`; `DominantWavelength`/`ExcitationPurity`/`RelativeLuminance`; `IsInRgbGamut` + `MapToRgbGamut(GamutMap.OklchChromaReduction)` for out-of-working-gamut emission), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its `Coercion` case (`Gated` where UnitsNet publishes the quantity — `Illuminance.Info`/`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`/`Power.Info`; `Borrowed` for a per-steradian radiance/radiant-intensity row, naming the `IrradianceUnit`/`PowerUnit` SI-base enum the prefix rescale targets since steradian carries no SI prefix), its `Photopic` luminous-twin marker driving the 683 lm/W divide, and its `CanonicalIsRadiance` discriminant (a directly-usable emitted radiance W/(sr·m²) versus a flux/irradiance/power needing area/solid-angle normalization — read through `EmissionInput.Source.CanonicalIsRadiance`) — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a parallel `nit` quantity. A sub/multiple of an existing unit is the SAME row called with that unit `Enum` (`Luminance` with `LuminanceUnit.Nit`, `Illuminance` with `IlluminanceUnit.Kilolux`), the SI coercion resolving it — never a parallel alias row. A new emission model is one `EmissionSpectrum` case; a new temperature locus is one `Locus` value on the `Blackbody` discriminant, a binned off-Planckian source is the `Blackbody` row's `Duv` column (never a parallel LED case), a datasheet xy source is the one `Chromatic` case, a new named illuminant one `Illuminant` static carried on the one `Standard` case, and a new observer policy one `PhotometricPolicy.Observer` value — never a parallel daylight, fluorescent, or wide-field emission case. Time-integrated quantities (luminous energy lm·s, exposure lux·s) are deliberately absent: the band admits steady-state emission, and an integrated row lands only with a consumer that integrates.
- Law: every arm of `EmissionSpectrum` resolves under the ONE scene-linear working space — the `Blackbody`, `Standard`, `Chromatic`, and `Spectral` arms construct through the config-explicit `Unicolour` constructors over `PortValue.SceneLinear` (or its `Observer.Degree10` sibling when the policy selects the CIE 1964 observer), so the Planckian table, the daylight-locus polynomial, the illuminant white points, and the SPD integration all resolve INTO AP1-linear channels; constructing under `Configuration.Default` and re-tagging the sRGB-linear channels as Acescg is the silent primary-shift defect. The luminous↔radiometric coercion runs a real divide: 683 lm/W at the 555 nm photopic peak scaled by the source's `EfficacyRatio` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²), while a radiometric quantity passes its SI-base magnitude through. Emission that resolves outside the working RGB gamut (a 1800 K blackbody, a near-locus monochromatic SPD) gamut-maps through `MapToRgbGamut(GamutMap.OklchChromaReduction)` with the mapping recorded on `EmissionInput.GamutMapped` — never an RGB clamp, never a negative channel propagated into the lobe math, never a fault for a physically-real chromaticity; only a non-finite resolve rails `MaterialFault.Gamut`.
- Boundary: `Photometric` NEVER re-mints a unit owner — it admits `UnitsNet` IN-FOLDER through `MaterialUnits` (the seam the Compute `ARCHITECTURE [04]` enshrines: AEC admits UnitsNet in-folder). A `Gated` row gates through `MaterialUnits.Admit(family, value, unit, key, correlation)` — `Quantity.TryFrom` constructs, `typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)` gates (the name check load-bearing because UnitsNet collapses lumen/candela/cd·m⁻² to the one luminous-intensity dimension), and ONE `ToUnit(UnitSystem.SI)` derives the SI magnitude and the canonical-unit witness the receipt names, crossing no quantity type into an interior signature. A `Borrowed` row (UnitsNet has no `Radiance`/`RadiantIntensity` quantity — steradian is dimensionally absent in SI) takes the pure `MaterialUnits.Coerce` prefix rescale over its named `IrradianceUnit`/`PowerUnit` SI-base enum. The gate and the radiometric divide COMPOSE in the one `PhotometricQuantity.Admit` row method: coerce to `CanonicalValue`, THEN derive `RadiometricSi` for a photopic row. The `Blackbody` case resolves `new Unicolour(config, bb.Cct, bb.Locus, bb.Luminance)` on either the Planckian or the CIE Daylight locus (Planck's law and the daylight polynomial owned by Unicolour, never re-derived); the per-locus CCT guard is inline in the arm — any finite positive CCT on the Planckian arm, the `Radiometry.DaylightCctMinKelvin`–`DaylightCctMaxKelvin` (4000–25000 K, the CIE 15 D-series polynomial domain) range on the daylight arm — with a finite non-negative luminance on both authored arms; a non-zero `Duv` is Planckian-arm-only, bounded `|Duv| ≤ Radiometry.DuvValidBound` (the package `Temperature.IsValid` domain), and resolves `new Unicolour(config, new Temperature(cct, duv), luminance)` — the binned LED admits without a parallel case. `Standard` resolves `illuminant.GetWhitePoint(policy.Observer).Chromaticity` through the config-explicit chromaticity constructor (the `Illuminant.Spd` is `internal`, so the case lowers through the public white-point surface); `Chromatic` guards its point inside the xy triangle (finite, `X ≥ 0`, `Y > 0`, `X + Y ≤ 1`) and lowers through the SAME chromaticity constructor — a datasheet source and a standard illuminant differ only by where the point comes from. `Spectral` rejects a negative coefficient (a measured SPD is non-negative by definition — `ContainsAnyInRange` over the span) and an interval outside `Spd.IsValid`. The resolved color projects its `Temperature` readout (CCT + Duv; `IsValid` marks |Duv| ≤ 0.05 where a CCT is chromatically meaningful, `IsHighAccuracy` the 1000–20000 K search band — the ANSI C78.377 binning discriminant), `DominantWavelength`/`ExcitationPurity`, and `RelativeLuminance` (the Y the emission node normalizes color against so intensity carries all the energy) onto `EmissionInput` — every readout composed from the Unicolour surface, never re-derived. The `UnitEvidence` receipt rides `EmissionInput.Provenance` so a consumer distinguishes a gated-and-rescaled luminous admission from a raw radiometric passthrough. A non-finite or negative admission rails `MaterialFault.Parameter` (band 2450), never a sentinel emission; a non-finite emission RGB rails `MaterialFault.Gamut`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                       // Op (the boundary-admission key)
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault (band 2450) declared on bsdf#SHADING_FRAME, composed here
using Rasm.Materials.Appearance.Graph;   // PortValue (the scene-linear Acescg Configuration owner — PortValue.SceneLinear)
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;
using Thinktecture;
using static LanguageExt.Prelude;
using Temperature = Wacton.Unicolour.Temperature;   // UnitsNet also exports Temperature — the bare name pins the CCT+Duv colour readout

namespace Rasm.Materials.Appearance.Photometric;

// MaterialFault (band 2450) is declared on bsdf#SHADING_FRAME and composed here; PortValue.SceneLinear is the
// Acescg Configuration declared on graph#MATERIAL_GRAPH and composed here — no second fault band, no second
// scene-linear working space.

// --- [TYPES] -------------------------------------------------------------------------------
// The closed gate-or-rescale discriminant: Gated rows own a published UnitsNet family (dimension+name gate, then
// ONE ToUnit(UnitSystem.SI) deriving the SI magnitude AND the canonical-unit witness); Borrowed rows own the
// per-steradian prefix rescale over a named SI-base enum (UnitsNet has no Radiance/RadiantIntensity quantity).
// One column replaces the prior Family/CanonicalUnit pair where each was meaningful only when the other was absent.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Coercion {
    private Coercion() { }

    public sealed record Gated(QuantityInfo Family) : Coercion;
    public sealed record Borrowed(Enum Canonical) : Coercion;

    internal Fin<UnitEvidence> Admit(double value, Enum unit, Op key, Guid correlation) =>
        Switch(
            state: (Value: value, Unit: unit, Key: key, Correlation: correlation),
            gated:    static (s, g) => MaterialUnits.Admit(g.Family, s.Value, s.Unit, s.Key, s.Correlation),
            borrowed: static (s, b) => MaterialUnits.Coerce(s.Value, s.Unit, b.Canonical, s.Key)
                .Map(si => UnitEvidence.Raw(s.Value, s.Unit, si, b.Canonical, s.Correlation)));
}

// One light-unit band: each row binds its Coercion case, the Photopic marker driving the 683 lm/W radiometric
// divide, and the radiance-vs-scalar discriminant. The Admit row method composes coerce THEN divide in one place,
// so a luminous row is BOTH dimensionally admitted AND lowered to its radiometric twin — never one or the other.
// `nit` is NOT a row: it is LuminanceUnit.Nit on the Luminance row.
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    public static readonly PhotometricQuantity Illuminance       = new(0, new Coercion.Gated(UnitsNet.Illuminance.Info),            photopic: true,  canonicalIsRadiance: false);
    public static readonly PhotometricQuantity Luminance         = new(1, new Coercion.Gated(UnitsNet.Luminance.Info),              photopic: true,  canonicalIsRadiance: true);
    public static readonly PhotometricQuantity LuminousFlux      = new(2, new Coercion.Gated(UnitsNet.LuminousFlux.Info),           photopic: true,  canonicalIsRadiance: false);
    public static readonly PhotometricQuantity LuminousIntensity = new(3, new Coercion.Gated(UnitsNet.LuminousIntensity.Info),      photopic: true,  canonicalIsRadiance: true);
    public static readonly PhotometricQuantity Irradiance        = new(4, new Coercion.Gated(UnitsNet.Irradiance.Info),             photopic: false, canonicalIsRadiance: false);
    public static readonly PhotometricQuantity RadiantFlux       = new(5, new Coercion.Gated(UnitsNet.Power.Info),                  photopic: false, canonicalIsRadiance: false);
    public static readonly PhotometricQuantity Radiance          = new(6, new Coercion.Borrowed(IrradianceUnit.WattPerSquareMeter), photopic: false, canonicalIsRadiance: true);
    public static readonly PhotometricQuantity RadiantIntensity  = new(7, new Coercion.Borrowed(PowerUnit.Watt),                    photopic: false, canonicalIsRadiance: true);

    public Coercion Coercion { get; }
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }

    // The ONE row coercion: SI-base magnitude through the Coercion case, then the radiometric-twin derivation for a
    // photopic row (CanonicalValue / (683 · ratio); == CanonicalValue for a radiometric row). INTERNAL — the
    // magnitude/ratio gate lives on Photometric.Admit, the single admission door; a public row method would be a
    // second ungated ingress. The receipt stays unit-faithful — CanonicalUnit names CanonicalValue's unit.
    internal Fin<UnitEvidence> Admit(double value, Enum unit, double efficacyRatio, Op key, Guid correlation) =>
        Coercion.Admit(value, unit, key, correlation)
            .Map(evidence => evidence with {
                RadiometricSi = Photopic ? evidence.CanonicalValue / (Radiometry.LuminousEfficacy * efficacyRatio) : evidence.CanonicalValue });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    // Duv is the planckian offset an ANSI C78.377 chromaticity bin quotes beside its CCT — 0.0 IS the locus; a
    // non-zero offset is admissible on the Planckian arm only and resolves through the Temperature constructor.
    public sealed record Blackbody(double Cct, double Luminance, Locus Locus = Locus.Blackbody, double Duv = 0.0) : EmissionSpectrum;

    public sealed record Standard(Illuminant Illuminant, double Luminance) : EmissionSpectrum;

    // The datasheet source: a luminaire/LED specification quotes an xy chromaticity point plus output — the fourth
    // canonical emission spec form beside CCT, named illuminant, and measured SPD.
    public sealed record Chromatic(Chromaticity Point, double Luminance) : EmissionSpectrum;

    public sealed record Spectral(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) : EmissionSpectrum;

    public sealed record Constant(double R, double G, double B) : EmissionSpectrum;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static class Radiometry {
    public const double LuminousEfficacy = 683.0;        // lm/W at the 555 nm monochromatic photopic peak — the luminous->radiometric divide anchor
    public const double DaylightCctMinKelvin = 4000.0;   // CIE 15 D-series locus polynomial domain — outside it the daylight curve extrapolates silently
    public const double DaylightCctMaxKelvin = 25000.0;
    public const double DuvValidBound = 0.05;            // |Duv| beyond which a CCT is chromatically meaningless — the package Temperature.IsValid domain
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The in-folder UnitsNet admission boundary: Materials owns its OWN unit coercion through the directly-pinned
// UnitsNet package, never a Rasm.Compute project reference (the acyclic strata forbids the AEC->app-platform edge).
// Conversion runs exactly once at admission and the receipt carries plain strings/doubles, so no UnitsNet type
// crosses an interior signature or a wire. CanonicalUnit/CanonicalValue are the faithful SI-base UNIT magnitude
// (lux/cd·m⁻²/lm/cd/W·m⁻²/W); RadiometricSi is the radiometric magnitude the row derives, kept distinct so the
// receipt never contradicts itself (unit names value).
public readonly record struct UnitEvidence(string Family, string OriginalUnit, double OriginalValue, string CanonicalUnit, double CanonicalValue, double RadiometricSi, Guid CorrelationId) {
    // Gated receipt: ONE ToUnit(UnitSystem.SI) derives the SI magnitude AND the canonical-unit witness — the
    // published family's SI base IS the receipt's unit, so no per-row canonical-unit column exists for gated rows.
    public static UnitEvidence From(IQuantity quantity, Guid correlation) {
        IQuantity si = quantity.ToUnit(UnitSystem.SI);
        double canonical = si.As(si.Unit);
        return new(quantity.QuantityInfo.Name, quantity.Unit.ToString(), quantity.As(quantity.Unit), si.Unit.ToString(), canonical, canonical, correlation);
    }

    // Borrowed receipt: no UnitsNet quantity exists to read a family or SI unit from; the row's Borrowed enum is
    // the SI-base witness for the prefix rescale (e.g. kW/(sr·m²) -> W/(sr·m²)).
    public static UnitEvidence Raw(double originalValue, Enum originalUnit, double canonicalValue, Enum canonical, Guid correlation) =>
        new(canonical.GetType().Name, originalUnit.ToString(), originalValue, canonical.ToString(), canonicalValue, canonicalValue, correlation);
}

public static class MaterialUnits {
    public static Fin<double> Coerce(double value, Enum from, Enum to, Op key) =>
        UnitConverter.TryConvert(value, from, to, out double converted)
            ? Fin.Succ(converted)
            : MaterialFault.Parameter(key, $"<unit-convert:{from}->{to}>");

    // The name check is load-bearing: UnitsNet collapses lumen/candela/cd·m⁻² to the one luminous-intensity
    // dimension (0,0,0,0,0,0,1), so dimensions alone cannot distinguish a flux from an intensity from a luminance.
    public static Fin<UnitEvidence> Admit(QuantityInfo family, double value, Enum unit, Op key, Guid correlation) =>
        Quantity.TryFrom(value, unit, out IQuantity? typed) && typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)
            ? Fin.Succ(UnitEvidence.From(typed, correlation))
            : MaterialFault.Parameter(key, $"<unit-admit:{unit}:outside:{family.Name}>");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The canonical emission payload: scene-linear radiance, radiometric-SI intensity, the Unicolour Temperature
// CCT+Duv readout (IsValid = |Duv| <= 0.05 marks a chromatically-meaningful CCT, IsHighAccuracy the 1000-20000 K
// search band — the ANSI C78.377 binning discriminant), the dominant-wavelength/purity chromaticity, the relative
// luminance (Y) the consumer normalizes color against so intensity carries all the energy, the gamut-map evidence,
// and the threaded UnitEvidence provenance — every readout composed off the ONE resolved Unicolour.
public readonly record struct EmissionInput(
    Unicolour Radiance, double Intensity, PhotometricQuantity Source,
    double DominantWavelengthNm, double ExcitationPurity, Temperature Temperature, double RelativeLuminance,
    bool GamutMapped, UnitEvidence Provenance) {

    public static EmissionInput Of(Unicolour sceneLinear, UnitEvidence canonical, PhotometricQuantity source, bool gamutMapped) =>
        new(sceneLinear, canonical.RadiometricSi, source, sceneLinear.DominantWavelength, sceneLinear.ExcitationPurity,
            sceneLinear.Temperature, sceneLinear.RelativeLuminance, gamutMapped, canonical);
}

// Observer is a policy column, not a knob: Degree2 (CIE 1931) is the point-source default, Degree10 (CIE 1964) the
// large-field architectural readout — it selects the white-point projection AND the SPD/CCT integration observer.
public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio, Observer Observer) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0, Observer.Degree2);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    // The Degree10 sibling of PortValue.SceneLinear: same Acescg primaries and D65 white, the CIE 1964 observer
    // governing the Planckian table, the daylight polynomial, and the SPD integration.
    static readonly Configuration SceneLinearDegree10 = new(RgbConfiguration.Acescg, new XyzConfiguration(Illuminant.D65, Observer.Degree10, "<acescg-degree10>"));

    // efficacyRatio is the photopic-band radiant-power fraction — (0,1] by physics, 1.0 at the monochromatic anchor;
    // zero would divide the radiometric twin to infinity, so it gates with the magnitude.
    public static Fin<UnitEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0) =>
        double.IsFinite(value) && value >= 0.0 && efficacyRatio is > 0.0 and <= 1.0
            ? quantity.Admit(value, unit, efficacyRatio, key, correlation)
            : MaterialFault.Parameter(key, $"<photometric-magnitude:{quantity.Key}:{value:R}@{efficacyRatio:R}>");

    public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation) =>
        from _ in guard(double.IsFinite(policy.Exposure) && policy.Exposure >= 0.0,
            MaterialFault.Parameter(key, $"<photometric-exposure:{policy.Exposure:R}>"))
        from canonical in Admit(quantity, value, unit, key, correlation, policy.EfficacyRatio)
        from resolved in SceneLinear(policy.Spectrum, policy.Observer, key)
        select EmissionInput.Of(Expose(resolved.Colour, policy.Exposure), canonical, quantity, resolved.Mapped);

    static Configuration WorkingSpace(Observer observer) =>
        observer == Observer.Degree10 ? SceneLinearDegree10 : PortValue.SceneLinear;

    // Every authored arm guards its inputs BEFORE construction (finite positive CCT — daylight additionally inside
    // the CIE D-series domain, a non-zero Duv Planckian-only and |Duv|-bounded — an xy point inside the chromaticity
    // triangle, finite non-negative luminance, non-negative SPD coefficients), and every arm constructs
    // CONFIG-EXPLICIT under the scene-linear working space so .RgbLinear is AP1-linear everywhere.
    static Fin<(Unicolour Colour, bool Mapped)> SceneLinear(EmissionSpectrum spectrum, Observer observer, Op key) =>
        spectrum.Switch(
            state: (Observer: observer, Key: key),
            blackbody: static (s, bb) =>
                double.IsFinite(bb.Cct) && bb.Cct > 0.0 && double.IsFinite(bb.Luminance) && bb.Luminance >= 0.0
                    && (bb.Locus != Locus.Daylight || bb.Cct is >= Radiometry.DaylightCctMinKelvin and <= Radiometry.DaylightCctMaxKelvin)
                    && double.IsFinite(bb.Duv) && Math.Abs(bb.Duv) <= Radiometry.DuvValidBound && (bb.Duv == 0.0 || bb.Locus == Locus.Blackbody)
                ? Gate(bb.Duv == 0.0
                    ? new Unicolour(WorkingSpace(s.Observer), bb.Cct, bb.Locus, bb.Luminance)
                    : new Unicolour(WorkingSpace(s.Observer), new Temperature(bb.Cct, bb.Duv), bb.Luminance), s.Key)
                : MaterialFault.Parameter(s.Key, $"<photometric-{(bb.Locus == Locus.Daylight ? "daylight" : "blackbody")}-cct:{bb.Cct:R}@{bb.Luminance:R}:duv={bb.Duv:R}>"),
            standard: static (s, st) => double.IsFinite(st.Luminance) && st.Luminance >= 0.0
                ? Gate(new Unicolour(WorkingSpace(s.Observer), st.Illuminant.GetWhitePoint(s.Observer).Chromaticity, st.Luminance), s.Key)
                : MaterialFault.Parameter(s.Key, $"<photometric-illuminant-luminance:{st.Luminance:R}>"),
            chromatic: static (s, c) =>
                double.IsFinite(c.Point.X) && double.IsFinite(c.Point.Y) && c.Point.X >= 0.0 && c.Point.Y > 0.0 && c.Point.X + c.Point.Y <= 1.0
                    && double.IsFinite(c.Luminance) && c.Luminance >= 0.0
                ? Gate(new Unicolour(WorkingSpace(s.Observer), c.Point, c.Luminance), s.Key)
                : MaterialFault.Parameter(s.Key, $"<photometric-chromaticity:{c.Point.X:R},{c.Point.Y:R}@{c.Luminance:R}>"),
            spectral: static (s, sp) => sp.Coefficients.Span.ContainsAnyInRange(double.NegativeInfinity, -double.Epsilon)
                ? MaterialFault.Parameter(s.Key, "<photometric-spd-negative-coefficient>")
                : new Spd(sp.StartNm, sp.IntervalNm, sp.Coefficients.ToArray()) is { IsValid: true } spd
                    ? Gate(new Unicolour(WorkingSpace(s.Observer), spd), s.Key)
                    : MaterialFault.Parameter(s.Key, $"<photometric-spd-interval:{sp.IntervalNm}>"),
            constant: static (s, c) => Gate(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, c.R, c.G, c.B), s.Key));

    // Non-finite rails loud; an out-of-working-gamut emission (a 1800 K blackbody, a near-locus monochromatic SPD)
    // gamut-maps by OklchChromaReduction with the mapping recorded on the receipt — never an RGB clamp, never a
    // negative channel propagated into the lobe math, never a fault for a physically-real chromaticity.
    static Fin<(Unicolour Colour, bool Mapped)> Gate(Unicolour colour, Op key) {
        var rgb = colour.RgbLinear;
        return !double.IsFinite(rgb.R) || !double.IsFinite(rgb.G) || !double.IsFinite(rgb.B)
            ? MaterialFault.Gamut(key, "<emission-non-finite-rgb>")
            : colour.IsInRgbGamut
                ? Fin.Succ((colour, Mapped: false))
                : Fin.Succ((colour.MapToRgbGamut(GamutMap.OklchChromaReduction), Mapped: true));
    }

    // Exposure scales the AP1-linear channels and re-anchors on the ONE Degree2 scene-linear carrier the graph
    // consumes — the observer already did its work during integration; the channel values are AP1/D65 either way.
    static Unicolour Expose(Unicolour colour, double exposure) {
        var rgb = colour.RgbLinear;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R * exposure, rgb.G * exposure, rgb.B * exposure);
    }
}
```

## [03]-[RESEARCH]

- [LUMINOUS_EFFICACY_BAND]: the 683 lm/W efficacy is exact only at 555 nm; a broadband source's luminous↔radiometric ratio is the photopic-weighted integral of its SPD against the CIE V(λ) luminosity function. The author-kernel `PhotometricQuantity.Admit` divides the SI-base magnitude by the 683 constant scaled by `PhotometricPolicy.EfficacyRatio` — the per-source radiant-power fraction in the photopic band, 1.0 at the monochromatic anchor and <1.0 for a broadband emitter. The ratio stays a policy column: Unicolour's `Xyz.FromSpd` normalizes Y against the reference white, so the ABSOLUTE ∫V(λ)S(λ)/∫S(λ) ratio is not recoverable from the relative tristimulus readout — a spectral-source consumer that integrates V(λ) against the `EmissionSpectrum.Spectral` SPD supplies the exact ratio per source without a new surface, the divide composing AFTER the coercion so a dimensionally-admitted luminous quantity is always lowered to its radiometric twin.
- [SCENE_LINEAR_WORKING_SPACE]: REALIZED — every `EmissionSpectrum` arm constructs CONFIG-EXPLICIT under the Acescg scene-linear working space (`new Unicolour(WorkingSpace(observer), …)` over the verified `(Configuration, double cct, Locus, double luminance)`/`(Configuration, Temperature, double luminance)`/`(Configuration, Chromaticity, double luminance)`/`(Configuration, Spd)` constructors), replacing the default-configuration constructions that resolved CCT/illuminant chromaticities to sRGB-linear channels and re-tagged them as AP1 at `Expose` — a silent primary shift on the chromaticity-resolved arms. The `PhotometricPolicy.Observer` column selects `Observer.Degree2` (CIE 1931, the default) or `Observer.Degree10` (CIE 1964, large-field architectural readouts), governing the white-point projection, the Planckian table, and the SPD integration through one `WorkingSpace` policy pair; `Expose` re-anchors the exposed channels on the one `PortValue.SceneLinear` carrier the graph consumes.
- [TEMPERATURE_READOUT_AND_LOCUS]: REALIZED — the `Blackbody` case resolves on either the Planckian (`Locus.Blackbody`) or the CIE Daylight (`Locus.Daylight`, D-series) locus through the one CCT constructor, the per-locus guard inline in the arm (any finite positive CCT Planckian; 4000–25000 K daylight — the CIE 15 polynomial domain); a non-zero `Duv` column admits the off-Planckian binned source through `new Unicolour(config, new Temperature(cct, duv), luminance)` — Duv-to-chromaticity is package math, never a hand-rolled perpendicular offset; `Standard` resolves a CIE illuminant through the public `GetWhitePoint(Observer).Chromaticity` projection because `Illuminant.Spd` is `internal`, and `Chromatic` admits the datasheet xy point through the same chromaticity constructor. The resolved colour's `Temperature` readout (the Unicolour `Temperature(Cct, Duv)` record — `IsValid` = |Duv| ≤ 0.05, `IsHighAccuracy` = the 1000–20000 K search band) rides `EmissionInput.Temperature` beside `DominantWavelength`/`ExcitationPurity`/`RelativeLuminance`, so a daylight- or illuminant-correlated source reports the CCT+Duv chromaticity bin (ANSI C78.377) a lighting consumer reads — composed from the Unicolour surface, never a hand-rolled Robertson search. Out-of-working-gamut emission gamut-maps via `MapToRgbGamut(GamutMap.OklchChromaReduction)` with `EmissionInput.GamutMapped` as the receipt evidence.
- [COERCION_COLLAPSE]: REALIZED — the prior parallel `Family`/`CanonicalUnit` row columns (each meaningful only when the other was absent) collapsed onto the ONE closed `Coercion` `[Union]`: `Gated` rows gate dimension+name through `MaterialUnits.Admit` and derive BOTH the SI magnitude and the canonical-unit witness from ONE `ToUnit(UnitSystem.SI)` (the family's SI base IS the receipt unit — `IlluminanceUnit.Lux`, `LuminanceUnit.CandelaPerSquareMeter`, `LuminousFluxUnit.Lumen`, `LuminousIntensityUnit.Candela`, `IrradianceUnit.WattPerSquareMeter`, `PowerUnit.Watt`); `Borrowed` rows (UnitsNet publishes no `Radiance`/`RadiantIntensity` quantity — steradian is dimensionally absent in SI, so radiance shares irradiance's dimension and radiant-intensity shares power's) name their SI-base enum for the `MaterialUnits.Coerce` prefix rescale. The gate and the 683 lm/W divide compose in the ONE `PhotometricQuantity.Admit` row method. The strata-correct admission is entirely Materials-owned — never a reach DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner.
