# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical radiometric graph-emission inputs through the in-folder `MaterialUnits` UnitsNet boundary and the author-kernel 683 lm/W luminous↔radiometric efficacy divide. Each band row carries ONE closed `Coercion` discriminant — `Gated` (UnitsNet publishes the quantity family: dimension+name gate, then `ToUnit(UnitSystem.SI)` derives BOTH the SI magnitude and the canonical-unit witness) or `Borrowed` (the per-steradian rows UnitsNet has no quantity for: a prefix rescale over the named SI-base enum) — never a parallel family column beside a canonical-unit column where each is meaningful only when the other is absent. `EmissionSpectrum` carries the blackbody/daylight-locus CCT (a non-zero `Duv` planckian offset admitting the ANSI C78.377 binned source), the CIE standard-illuminant, the datasheet chromaticity point, the measured-SPD, and the constant emission color; every arm resolves under the ONE `PortValue.SceneLinear` Acescg working space through the config-explicit `Unicolour` constructors — a default-configuration construction would resolve CCT/illuminant/SPD to sRGB-linear channels and silently re-tag them as AP1, the working-space corruption this page forecloses. `PhotometricPolicy` maps admitted light rows onto the BSDF/graph emission node and carries the CIE standard observer as a policy column. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, a parallel `nit` quantity (`nit` is `LuminanceUnit.Nit`, a unit of the luminance row), or a second color register. The page admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner — and consumes Wacton.Unicolour directly for blackbody/SPD/illuminant→XYZ→scene-linear emission color, resolving the canonical `EmissionInput` payload (scene-linear radiance, radiometric-SI intensity, the `Temperature` CCT+Duv readout, dominant-wavelength/purity chromaticity, relative luminance, gamut-map evidence, and `UnitEvidence` provenance) — never re-minting a unit owner or a color axis. That payload is the CONSTRUCTOR of a row's two emission columns through `Photometric.WithEmission`, and its terminals are the `graph#MATERIAL_GRAPH` `BsdfOutput` sink's `Emission` port and the `SurfaceShade.EmissionLinear` column the integrator adds as radiance OUTSIDE the BSDF fold; the `bsdf#LOBE_FAMILY` set is closed at seven scattering lobes and carries no emission case, so emitted radiance reaches its consumer on the `surface#OPENPBR_SLAB` collapse's own emission field rather than as an eighth lobe a normalized convex sum would divide by a pdf.

## [01]-[INDEX]

- [02]-[PHOTOMETRIC]: the `PhotometricQuantity` band over the closed `Coercion` gate/rescale discriminant, the in-folder `MaterialUnits` UnitsNet boundary (`UnitSystem.SI` coercion + per-row family gate + `UnitEvidence` receipt), the unified gate-then-radiometric-divide `Admit` on the band row, the `EmissionSpectrum` blackbody/illuminant/chromaticity/spectral/constant family resolved scene-linear config-explicit, the `PhotometricPolicy` light→emission map with the observer policy column, and the `EmissionInput` payload carrying radiance, intensity, the CCT+Duv `Temperature` readout, chromaticity, and unit provenance.

## [02]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band owning the radiometric divide over its `Coercion` column; `Coercion` `[Union]` the closed gate-or-rescale discriminant; `MaterialUnits` the in-folder UnitsNet boundary (family-membership gate + `UnitSystem.SI` coercion + `UnitEvidence` receipt); `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, luminous-flux, luminous-intensity, irradiance, radiance, radiant-intensity, radiant-flux}; coercion {`Gated` (a published UnitsNet `QuantityInfo` family), `Borrowed` (a per-steradian row over a borrowed SI-base unit enum)}; emission {`Blackbody` (a CCT source over a `Locus` discriminant — Planckian or CIE Daylight — with the `Duv` planckian-offset column carrying an ANSI C78.377 binned source), `Standard` (a CIE standard illuminant — D-series daylight, A incandescent, F-series fluorescent), `Chromatic` (a datasheet xy chromaticity point plus luminance), `Spectral` (a measured SPD over the folder-root `SpectralCurve` grid), `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<UnitEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0)` returning the `UnitEvidence` receipt whose `CanonicalValue` is the faithful SI-base UNIT magnitude (lux/cd·m⁻²/lm/cd/W·m⁻²/W) and whose `RadiometricSi` is the radiometric scalar the emission node consumes (luminous rows gated against their UnitsNet family, coerced to SI base through `ToUnit(UnitSystem.SI)`, then divided to their radiometric twin; radiometric rows carry `RadiometricSi == CanonicalValue`) — the unit and the radiometric value kept on distinct fields so the receipt never contradicts itself; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload; `public static Fin<MaterialParameters> WithEmission(MaterialParameters row, EmissionInput emission, Op key)` is the row-side terminal writing that payload onto the row's `Emission`/`EmissionLuminance` pair with the WHOLE `EmissionInput` receipt on `EmissionProvenance`, so an ADMITTED emission is a construction rather than two hand-set columns and the full measurement — unit witness, chromaticity readouts, CCT+Duv, measured Y, gamut evidence — survives to `interchange#MATERIAL_WIRE`. The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the `MaterialUnits.UnitEvidence` receipt onto the payload — distinct identifiers for distinct rails. Conversion runs exactly once at admission and interior numerics are raw doubles per the BOUNDARY_ADMISSION law.
- Packages: UnitsNet (admitted IN-FOLDER through the `MaterialUnits` owner — `Quantity.TryFrom` the dynamic-quantity construction the family gate inspects, the `QuantityInfo`/`BaseDimensions`/`QuantityInfo.Name` family-membership surface, `ToUnit(UnitSystem.SI)` the one SI-base coercion deriving magnitude AND unit witness for every published family, `UnitConverter.TryConvert` the non-throwing prefix rescale for the borrowed per-steradian rows; catalogued in `libs/csharp/.api/api-unitsnet.md`), Rasm (project — `Op` boundary key, `MaterialFault` band), Wacton.Unicolour (composed for blackbody/daylight-locus CCT→scene-linear, CIE standard-illuminant→scene-linear, and SPD→XYZ→scene-linear — the config-explicit `new Unicolour(Configuration, double cct, Locus, double luminance)`/`(Configuration, Temperature, double luminance)`/`(Configuration, Chromaticity, double luminance)`/`(Configuration, Spd)` constructors under the Acescg working space; the `Locus.Blackbody`/`Locus.Daylight` temperature loci; the `Illuminant.D65`/`A`/`F2`… statics + `Illuminant.GetWhitePoint(Observer)`/`WhitePoint.Chromaticity` projection; the `Observer.Degree2`/`Degree10` standard observers; the `Temperature` CCT+Duv readout record with `IsValid`/`IsHighAccuracy`; `DominantWavelength`/`ExcitationPurity`/`RelativeLuminance`; `IsInRgbGamut` + `MapToRgbGamut(GamutMap.OklchChromaReduction)` for out-of-working-gamut emission), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its `Coercion` case (`Gated` where UnitsNet publishes the quantity — `Illuminance.Info`/`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`/`Power.Info`; `Borrowed` for a per-steradian radiance/radiant-intensity row, naming the `IrradianceUnit`/`PowerUnit` SI-base enum the prefix rescale targets since steradian carries no SI prefix), its `Photopic` luminous-twin marker driving the 683 lm/W divide, and its `CanonicalIsRadiance` discriminant (a directly-usable emitted radiance W/(sr·m²) versus a flux/irradiance/power needing area/solid-angle normalization — read through `EmissionInput.Source.CanonicalIsRadiance`) — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a parallel `nit` quantity. A sub/multiple of an existing unit is the SAME row called with that unit `Enum` (`Luminance` with `LuminanceUnit.Nit`, `Illuminance` with `IlluminanceUnit.Kilolux`), the SI coercion resolving it — never a parallel alias row. A new emission model is one `EmissionSpectrum` case and a new sampled grid one `SpectralCurve` value; a new temperature locus is one `Locus` value on the `Blackbody` discriminant, a binned off-Planckian source is the `Blackbody` row's `Duv` column (never a parallel LED case), a datasheet xy source is the one `Chromatic` case, a new named illuminant one `Illuminant` static carried on the one `Standard` case, and a new observer policy one `PhotometricPolicy.Observer` value — never a parallel daylight, fluorescent, or wide-field emission case. Time-integrated quantities (luminous energy lm·s, exposure lux·s) are deliberately absent: the band admits steady-state emission, and an integrated row lands only with a consumer that integrates.
- Law: every arm of `EmissionSpectrum` resolves under the ONE scene-linear working space — the `Blackbody`, `Standard`, `Chromatic`, and `Spectral` arms construct through the config-explicit `Unicolour` constructors over `PortValue.SceneLinear` (or its `Observer.Degree10` sibling when the policy selects the CIE 1964 observer), so the Planckian table, the daylight-locus polynomial, the illuminant white points, and the SPD integration all resolve INTO AP1-linear channels; constructing under `Configuration.Default` and re-tagging the sRGB-linear channels as Acescg is the silent primary-shift defect. The luminous↔radiometric coercion runs a real divide: 683 lm/W — exact only at the 555 nm photopic peak — scaled by the source's `EfficacyRatio` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²), while a radiometric quantity passes its SI-base magnitude through. `EfficacyRatio` is the source's radiant-power fraction inside the photopic band — unity at the monochromatic anchor, below it for a broadband emitter — and it is SUPPLIED rather than derived: the package SPD→XYZ path normalizes Y against the reference white, so the absolute ∫V(λ)S(λ)/∫S(λ) fraction is unrecoverable from a relative tristimulus readout, and a spectral source integrates V(λ) against its own `Spectral` SPD and hands the ratio in, the divide composing AFTER the coercion so a dimensionally-admitted luminous magnitude always lowers to its radiometric twin. Emission that resolves outside the working RGB gamut (a 1800 K blackbody, a near-locus monochromatic SPD) gamut-maps through `MapToRgbGamut(GamutMap.OklchChromaReduction)` with the mapping recorded on `EmissionInput.GamutMapped` — never an RGB clamp, never a negative channel propagated into the lobe math, never a fault for a physically-real chromaticity; only a non-finite resolve rails `MaterialFault.Gamut`.
- Boundary: `Photometric` NEVER re-mints a unit owner — it admits `UnitsNet` IN-FOLDER through `MaterialUnits` (the seam the Compute `ARCHITECTURE [04]` enshrines: AEC admits UnitsNet in-folder). A `Gated` row gates through `MaterialUnits.Admit(family, value, unit, key, correlation)` — `Quantity.TryFrom` constructs, `typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)` gates (the name check load-bearing because UnitsNet collapses lumen/candela/cd·m⁻² to the one luminous-intensity dimension), and ONE `ToUnit(UnitSystem.SI)` derives the SI magnitude and the canonical-unit witness the receipt names, crossing no quantity type into an interior signature. A `Borrowed` row (UnitsNet has no `Radiance`/`RadiantIntensity` quantity — steradian is dimensionally absent in SI) takes the pure `MaterialUnits.Coerce` prefix rescale over its named `IrradianceUnit`/`PowerUnit` SI-base enum. The gate and the radiometric divide COMPOSE in the one `PhotometricQuantity.Admit` row method: coerce to `CanonicalValue`, THEN derive `RadiometricSi` for a photopic row. The `Blackbody` case resolves `new Unicolour(config, bb.Cct, bb.Locus, bb.Luminance)` on either the Planckian or the CIE Daylight locus (Planck's law and the daylight polynomial owned by Unicolour, never re-derived); the per-locus CCT guard is inline in the arm — any finite positive CCT on the Planckian arm, the `Radiometry.DaylightCctMinKelvin`–`DaylightCctMaxKelvin` (4000–25000 K, the CIE 15 D-series polynomial domain) range on the daylight arm — with a finite non-negative luminance on both authored arms; a non-zero `Duv` is Planckian-arm-only, bounded `|Duv| ≤ Radiometry.DuvValidBound` (the package `Temperature.IsValid` domain), and resolves `new Unicolour(config, new Temperature(cct, duv), luminance)` — the binned LED admits without a parallel case. `Standard` resolves `illuminant.GetWhitePoint(policy.Observer).Chromaticity` through the config-explicit chromaticity constructor (the `Illuminant.Spd` is `internal`, so the case lowers through the public white-point surface); `Chromatic` guards its point inside the xy triangle (finite, `X ≥ 0`, `Y > 0`, `X + Y ≤ 1`) and lowers through the SAME chromaticity constructor — a datasheet source and a standard illuminant differ only by where the point comes from. `Spectral` carries an already-admitted `SpectralCurve`, whose own `Of` proved the interval against the `Spd.IsValid` domain, the extent, and non-negativity (a measured SPD is non-negative by definition), so the arm resolves rather than re-checks — the same carrier `acquisition#ACQUISITION` freezes to a durable spectral EXR, because a sampled spectrum is one shape whether it measures reflectance or emission. The resolved color projects its `Temperature` readout (CCT + Duv; `IsValid` marks |Duv| ≤ 0.05 where a CCT is chromatically meaningful, `IsHighAccuracy` the 1000–20000 K search band — the ANSI C78.377 binning discriminant), `DominantWavelength`/`ExcitationPurity`, and `RelativeLuminance` (the measured Y the `EmissionInput` construction divides OUT, so `Radiance` is unit-Y chromaticity by construction, intensity carries all the energy, and Y survives as receipt evidence) onto `EmissionInput` — every readout composed from the Unicolour surface, never re-derived. The `UnitEvidence` receipt rides `EmissionInput.Provenance` so a consumer distinguishes a gated-and-rescaled luminous admission from a raw radiometric passthrough. A non-finite or negative admission rails `MaterialFault.Parameter` (band 2450), never a sentinel emission; a non-finite emission RGB rails `MaterialFault.Gamut`. EVERY AUTHORED LIGHT MAGNITUDE IN THE FOLDER CROSSES `Photometric.Admit` — a declared unit with no admission behind it is a claim, not a quantity — and the folder holds exactly two such sites beside this page's own graph entry: the `environment#SKY_MODEL` zenith level (already composed, so a cd/m² sky and a lux sky reach one radiometric scalar with no page-local efficacy divide) and the `environment#ENVIRONMENT_MAP` `Intensity`, whose admitted `UnitEvidence` lets an HDRI authored in `lux` and one authored as a bare multiplier stay distinguishable at every `Scale` read and on the `EnvironmentLightWire` mirror; a dimensionless multiplier admits as `PhotometricQuantity.Radiance` with `RadiometricSi == CanonicalValue`, so the unitless case costs one construction and no branch. Reciprocally, `Raster/set#TEXTURE_SET` `ChannelUnit` is the PER-TEXEL PROJECTION of this same band and not a second unit vocabulary: each row binds the UnitsNet member this page's quantities already gate through and DERIVES its UCUM string from that binding via `MaterialUnits.Ucum` — so a channel's declared unit and the folder's admitted unit are one fact read at two grains, never two rosters that drift. UCUM lives here rather than on the channel roster because UnitsNet publishes no UCUM surface: its abbreviation cache yields display renderings (`cd/m²`, Unicode superscript) a reader consumes and a wire cannot, so the correspondence is a declared table on the unit owner and a transcribed string beside a channel row is the deleted form. High-luminance colour DIFFERENCE reads the HDR-correct metrics on the composed Unicolour selector — `Difference(candidate, DeltaE.Itp)` over the `Ictcp`/`Jzazbz` PQ-grounded spaces where CIELAB's luminance model breaks past the diffuse-white anchor — as one more `DeltaE` policy value on the caller's existing metric column, never a second colour owner or a photometric-local difference kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;         // the UCUM correspondence table
using LanguageExt;
using Rasm.Domain;                       // Op the boundary-admission key; CorrelationId the S0 causal half
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault (band 2450) declared on bsdf#SHADING_FRAME, composed here
using Rasm.Materials.Appearance;         // SpectralCurve — the folder-root sampled-spectrum carrier the measured-SPD arm reads
using Rasm.Materials.Appearance.Graph;   // MaterialParameters (the row WithEmission constructs) + PortValue (the two per-observer Acescg Configuration instances)
using UnitsNet;
using UnitsNet.Units;
using Rasm.Numerics;                     // GamutPolicy — the kernel working-gamut row the emission gate checks and bounds through
using Wacton.Unicolour;
using Thinktecture;
using static LanguageExt.Prelude;
using Temperature = Wacton.Unicolour.Temperature;   // UnitsNet also exports Temperature — the bare name pins the CCT+Duv colour readout

namespace Rasm.Materials.Appearance.Photometric;

// MaterialFault (band 2450) is declared on bsdf#SHADING_FRAME and composed here; PortValue.SceneLinear and its
// PortValue.SceneLinearDegree10 observer sibling are the two Acescg Configurations declared on
// graph#MATERIAL_GRAPH and composed here — no second fault band, no third working-space instance.

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

    // The measured SPD reads the folder-root SpectralCurve carrier acquisition#ACQUISITION owns: a sampled spectrum
    // is one shape whether it measures reflectance or emission, and its admission (interval domain, extent,
    // non-negativity) belongs to the carrier's own Of rather than re-spelled per arm — so this arm resolves a value
    // whose grid is already proven and the SceneLinear switch carries no coefficient guard at all.
    public sealed record Spectral(SpectralCurve Curve) : EmissionSpectrum;

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
public readonly record struct UnitEvidence(string Family, string OriginalUnit, double OriginalValue, string CanonicalUnit, double CanonicalValue, double RadiometricSi, CorrelationId Correlation) {
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
    // Ucum publishes this folder's machine-readable unit spelling, owned HERE because UnitsNet has no UCUM surface:
    // UnitAbbreviationsCache yields DISPLAY abbreviations (`cd/m²`, Unicode superscript), which is a rendering for a
    // reader and not a case-sensitive machine code a wire may carry. So the correspondence is a declared table on the
    // unit owner, and a channel row DERIVES its ucum string from its own SI unit instead of transcribing a second
    // string beside it — one fact, read at two grains.
    private static readonly FrozenDictionary<Enum, string> Tokens = new Dictionary<Enum, string> {
        [LengthUnit.Millimeter] = "mm",
        [LengthUnit.Nanometer] = "nm",
        [LuminanceUnit.CandelaPerSquareMeter] = "cd/m2",
    }.ToFrozenDictionary();

    // Exemption: a DECLARATION-TIME total assertion, not domain control flow — an unmapped unit is a row authored
    // against a token this owner never declared, so it fails the type initializer that reads it (the same posture
    // MaterialLibrary's admission sweep and GraphContext.Tolerant take) rather than shipping a wire string nothing
    // maps. Absence of a unit is the dimensionless "1", which is a real UCUM code and never a missing mapping.
    public static string Ucum(Enum? unit) =>
        unit is null ? "1"
        : Tokens.TryGetValue(unit, out string? token) ? token
        : throw new InvalidOperationException($"<ucum-unmapped:{unit}>");

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
// The canonical emission payload: unit-Y scene-linear chromaticity, exposure-scaled radiometric-SI intensity, the
// Unicolour Temperature CCT+Duv readout (IsValid = |Duv| <= 0.05 marks a chromatically-meaningful CCT,
// IsHighAccuracy the 1000-20000 K search band — the ANSI C78.377 binning discriminant), the dominant-wavelength/
// purity chromaticity, the MEASURED relative luminance (Y) the construction divided out — Radiance is unit-Y BY
// CONSTRUCTION so Intensity carries all the energy and Y survives as receipt evidence — the gamut-map evidence,
// and the threaded UnitEvidence provenance; every readout composed off the ONE resolved Unicolour BEFORE the
// normalization, so the receipt witnesses the resolve, not the normalized product.
public readonly record struct EmissionInput(
    Unicolour Radiance, double Intensity, PhotometricQuantity Source,
    double DominantWavelengthNm, double ExcitationPurity, Temperature Temperature, double RelativeLuminance,
    bool GamutMapped, UnitEvidence Provenance) {

    // Normalization re-anchors on the ONE Degree2 scene-linear carrier the graph consumes (the observer already did
    // its work during integration; channels are AP1/D65 either way); a zero-Y (black) emission normalizes to itself.
    public static EmissionInput Of(Unicolour sceneLinear, double intensity, PhotometricQuantity source, bool gamutMapped, UnitEvidence canonical) {
        var (rgb, y) = (sceneLinear.RgbLinear, sceneLinear.RelativeLuminance);
        Unicolour chroma = y > 0.0
            ? new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R / y, rgb.G / y, rgb.B / y)
            : sceneLinear;
        return new(chroma, intensity, source, sceneLinear.DominantWavelength, sceneLinear.ExcitationPurity,
            sceneLinear.Temperature, y, gamutMapped, canonical);
    }
}

// Observer is a policy column, not a knob: Degree2 (CIE 1931) is the point-source default, Degree10 (CIE 1964) the
// large-field architectural readout — it selects the white-point projection AND the SPD/CCT integration observer.
public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio, Observer Observer) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0, Observer.Degree2);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
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
        select EmissionInput.Of(resolved.Colour, canonical.RadiometricSi * policy.Exposure, quantity, resolved.Mapped, canonical);

    // WithEmission makes the payload the CONSTRUCTOR of the row's two emission columns: a row authoring
    // EmissionLuminance directly is the AUTHORED path and this the ADMITTED one, distinguishable afterwards because
    // only this path leaves a UnitEvidence on the row. Re-admission runs the row gate so a resolved emission passes
    // the same finite/gamut ladder a registered row passes rather than trusting its own resolve.
    public static Fin<MaterialParameters> WithEmission(MaterialParameters row, EmissionInput emission, Op key) =>
        MaterialParameters.Of(
            row with { Emission = emission.Radiance, EmissionLuminance = emission.Intensity, EmissionProvenance = Some(emission) }, key);

    // Observer selects WHICH of the two graph-owned Acescg instances integrates — both mint at
    // graph#MATERIAL_GRAPH PortValue so the working-space cache identity stays countable at one owner.
    static Configuration WorkingSpace(Observer observer) =>
        observer == Observer.Degree10 ? PortValue.SceneLinearDegree10 : PortValue.SceneLinear;

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
            spectral: static (s, sp) => Gate(new Unicolour(WorkingSpace(s.Observer), sp.Curve.ToSpd()), s.Key),
            constant: static (s, c) => Gate(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, c.R, c.G, c.B), s.Key));

    // Non-finite rails loud; an out-of-working-gamut emission (a 1800 K blackbody, a near-locus monochromatic SPD)
    // bounds through the kernel GamutPolicy.Perceptual row with the mapping recorded on the receipt — never an RGB clamp, never a
    // negative channel propagated into the lobe math, never a fault for a physically-real chromaticity.
    static Fin<(Unicolour Colour, bool Mapped)> Gate(Unicolour colour, Op key) {
        var rgb = colour.RgbLinear;
        return !double.IsFinite(rgb.R) || !double.IsFinite(rgb.G) || !double.IsFinite(rgb.B)
            ? MaterialFault.Gamut(key, "<emission-non-finite-rgb>")
            : GamutPolicy.Perceptual.Contains(colour)
                ? Fin.Succ((colour, Mapped: false))
                : Fin.Succ((GamutPolicy.Perceptual.Bound(colour), Mapped: true));
    }

}
```

## [03]-[RESEARCH]

(none)
