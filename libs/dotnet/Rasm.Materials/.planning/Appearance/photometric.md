# [MATERIALS_PHOTOMETRIC]

ONE `Photometric` static admission fold over the closed `PhotometricQuantity` band coercing every luminous/radiometric light unit to the canonical radiometric graph-emission inputs through the in-folder `MaterialUnits` UnitsNet boundary and the author-kernel 683 lm/W luminous↔radiometric efficacy divide. Each band row carries ONE closed `Coercion` discriminant — `Gated` (UnitsNet publishes the quantity family: dimension+name gate, then `ToUnit(UnitSystem.SI)` derives BOTH the SI magnitude and the canonical-unit witness) or `Borrowed` (the per-steradian rows UnitsNet has no quantity for: a prefix rescale over the named SI-base enum) — never a parallel family column beside a canonical-unit column where each is meaningful only when the other is absent. `EmissionSpectrum` carries the blackbody/daylight-locus CCT (a non-zero `Duv` planckian offset admitting the ANSI C78.377 binned source), the CIE standard-illuminant, the datasheet chromaticity point, the measured-SPD, and the constant emission color; every arm resolves under the ONE `PortValue.SceneLinear` Acescg working space through the config-explicit `Unicolour` constructors — a default-configuration construction would resolve CCT/illuminant/SPD to sRGB-linear channels and silently re-tag them as AP1, the working-space corruption this page forecloses. `PhotometricPolicy` maps admitted light rows onto the BSDF/graph emission node and carries the CIE standard observer as a policy column. A light unit is a `PhotometricQuantity` ROW, an emission model an `EmissionSpectrum` CASE, a color the one `Unicolour` carrier — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, a parallel `nit` quantity (`nit` is `LuminanceUnit.Nit`, a unit of the luminance row), or a second color register. The page admits `UnitsNet` IN-FOLDER through the `MaterialUnits` owner — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner — and consumes Wacton.Unicolour directly for blackbody/SPD/illuminant→XYZ→scene-linear emission color, resolving the canonical `EmissionInput` payload (scene-linear radiance, radiometric-SI intensity, the `Temperature` CCT+Duv readout, dominant-wavelength/purity chromaticity, relative luminance, gamut-map evidence, and `EmissionEvidence` provenance — the seam `Rasm.Element/Properties/quantity#UNIT_SCHEME` `MeasureEvidence` conversion receipt with the radiometric twin beside it) — never re-minting a unit owner, a conversion receipt, or a color axis. That payload is the CONSTRUCTOR of a row's two emission columns through `Photometric.WithEmission`, and its terminals are the `graph#MATERIAL_GRAPH` `BsdfOutput` sink's `Emission` port and the `SurfaceShade.EmissionLinear` column the integrator adds as radiance OUTSIDE the BSDF fold; the `bsdf#LOBE_FAMILY` set is closed at seven scattering lobes and carries no emission case, so emitted radiance reaches its consumer on the `surface#OPENPBR_SLAB` collapse's own emission field rather than as an eighth lobe a normalized convex sum would divide by a pdf.

## [01]-[INDEX]

- [02]-[PHOTOMETRIC]: the `PhotometricQuantity` band over the closed `Coercion` gate/rescale discriminant, the in-folder `MaterialUnits` UnitsNet boundary (`UnitSystem.SI` coercion + per-row family gate + `EmissionEvidence` receipt over the seam `MeasureEvidence`), the unified gate-then-radiometric-divide `Admit` on the band row, the `EmissionSpectrum` blackbody/illuminant/chromaticity/spectral/constant family resolved scene-linear config-explicit, the `PhotometricPolicy` light→emission map with the observer policy column, and the `EmissionInput` payload carrying radiance, intensity, the CCT+Duv `Temperature` readout, chromaticity, and unit provenance.

## [02]-[PHOTOMETRIC]

- Owner: `Photometric` static admission fold; `PhotometricQuantity` `[SmartEnum<int>]` band owning the radiometric divide over its `Coercion` column; `Coercion` `[Union]` the closed gate-or-rescale discriminant; `MaterialUnits` the in-folder UnitsNet boundary (family-membership gate + `UnitSystem.SI` coercion + `EmissionEvidence` receipt); `EmissionEvidence` the photometric COMPOSITION over the seam `MeasureEvidence` — the ONE conversion receipt carrying the radiometric twin the seam has no column for, never a re-spelling of its columns; `EmissionSpectrum` `[Union]`; `PhotometricPolicy` light→emission record.
- Cases: quantity {illuminance, luminance, luminous-flux, luminous-intensity, irradiance, radiance, radiant-intensity, radiant-flux}; coercion {`Gated` (a published UnitsNet `QuantityInfo` family), `Borrowed` (a per-steradian row over a borrowed SI-base unit enum)}; emission {`Blackbody` (a CCT source over a `Locus` discriminant — Planckian or CIE Daylight — with the `Duv` planckian-offset column carrying an ANSI C78.377 binned source), `Standard` (a CIE standard illuminant — D-series daylight, A incandescent, F-series fluorescent), `Chromatic` (a datasheet xy chromaticity point plus luminance), `Spectral` (a measured SPD over the folder-root `SpectralCurve` grid), `Constant`}.
- Entry: `Admit` is the magnitude coercion — `public static Fin<EmissionEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0)` returning the `EmissionEvidence` receipt whose `Measure.CanonicalValue` is the faithful SI-base UNIT magnitude (lux/cd·m⁻²/lm/cd/W·m⁻²/W) and whose `RadiometricSi` is the radiometric scalar the emission node consumes (luminous rows gated against their UnitsNet family, coerced to SI base through `ToUnit(UnitSystem.SI)`, then divided to their radiometric twin; radiometric rows carry `RadiometricSi == Measure.CanonicalValue`) — the unit-faithful seam receipt and the derived radiometric value kept on distinct fields so neither contradicts the other; `Resolve` is the graph-node entry — `public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation)` composing `Admit` with the resolved scene-linear emission color into the canonical `EmissionInput` payload; `public static Fin<MaterialParameters> WithEmission(MaterialParameters row, EmissionInput emission, Op key)` is the row-side terminal writing that payload onto the row's `Emission`/`EmissionLuminance` pair with the WHOLE `EmissionInput` receipt on `EmissionProvenance`, so an ADMITTED emission is a construction rather than two hand-set columns and the full measurement — unit witness, chromaticity readouts, CCT+Duv, measured Y, gamut evidence — survives to `interchange#MATERIAL_WIRE`. The `Op key` correlates the `MaterialFault` rail; the `Guid correlation` threads the `MaterialUnits` receipt's own seam `Correlation` onto the payload — distinct identifiers for distinct rails. Conversion runs exactly once at admission and interior numerics are raw doubles per the BOUNDARY_ADMISSION law.
- Packages: UnitsNet (admitted IN-FOLDER through the `MaterialUnits` owner — `Quantity.TryFrom` the dynamic-quantity construction the family gate inspects, the `QuantityInfo`/`BaseDimensions`/`QuantityInfo.Name` family-membership surface, `ToUnit(UnitSystem.SI)` the one SI-base coercion deriving magnitude AND unit witness for every published family, `UnitConverter.TryConvert` the non-throwing prefix rescale for the borrowed per-steradian rows; catalogued in `libs/dotnet/.api/api-unitsnet.md`), Rasm (project — `Op` boundary key, `MaterialFault` band), Rasm.Element (project — `MeasureEvidence` the ONE conversion receipt this owner composes, `UnitResolution` its posture vocabulary, `QuantityType` the family identity minted from the UnitsNet registry name), Wacton.Unicolour (composed for blackbody/daylight-locus CCT→scene-linear, CIE standard-illuminant→scene-linear, and SPD→XYZ→scene-linear — the config-explicit `new Unicolour(Configuration, double cct, Locus, double luminance)`/`(Configuration, Temperature, double luminance)`/`(Configuration, Chromaticity, double luminance)`/`(Configuration, Spd)` constructors under the Acescg working space; the `Locus.Blackbody`/`Locus.Daylight` temperature loci; the `Illuminant.D65`/`A`/`F2`… statics + `Illuminant.GetWhitePoint(Observer)`/`WhitePoint.Chromaticity` projection; the `Observer.Degree2`/`Degree10` standard observers; the `Temperature` CCT+Duv readout record with `IsValid`/`IsHighAccuracy`; `DominantWavelength`/`ExcitationPurity`/`RelativeLuminance`; `IsInRgbGamut` + `MapToRgbGamut(GamutMap.OklchChromaReduction)` for out-of-working-gamut emission), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new light unit is one `PhotometricQuantity` row binding its `Coercion` case (`Gated` where UnitsNet publishes the quantity — `Illuminance.Info`/`Luminance.Info`/`LuminousFlux.Info`/`LuminousIntensity.Info`/`Irradiance.Info`/`Power.Info`; `Borrowed` for a per-steradian radiance/radiant-intensity row, naming the `IrradianceUnit`/`PowerUnit` SI-base enum the prefix rescale targets since steradian carries no SI prefix), its `Photopic` luminous-twin marker driving the 683 lm/W divide, and its `CanonicalIsRadiance` discriminant (a directly-usable emitted radiance W/(sr·m²) versus a flux/irradiance/power needing area/solid-angle normalization — read through `EmissionInput.Source.CanonicalIsRadiance`) — never a per-unit type, a `LumenToWatt`/`NitToRadiance` helper family, or a parallel `nit` quantity. A sub/multiple of an existing unit is the SAME row called with that unit `Enum` (`Luminance` with `LuminanceUnit.Nit`, `Illuminance` with `IlluminanceUnit.Kilolux`), the SI coercion resolving it — never a parallel alias row. A new emission model is one `EmissionSpectrum` case and a new sampled grid one `SpectralCurve` value; a new temperature locus is one `Locus` value on the `Blackbody` discriminant, a binned off-Planckian source is the `Blackbody` row's `Duv` column (never a parallel LED case), a datasheet xy source is the one `Chromatic` case, a new named illuminant one `Illuminant` static carried on the one `Standard` case, and a new observer policy one `PhotometricPolicy.Observer` value — never a parallel daylight, fluorescent, or wide-field emission case. Time-integrated quantities (luminous energy lm·s, exposure lux·s) are deliberately absent: the band admits steady-state emission, and an integrated row lands only with a consumer that integrates.
- Law: `PhotometricQuantity`'s `Photopic` and `CanonicalIsRadiance` STAY two `bool` columns, and the kernel `Domain/validation#CAPABILITY` `CapabilitySet` law is what settles it rather than a preference — that law deletes adjacent bools only where a SUBSET of the boolean product's corners is legal, and all four corners here carry a real row (illuminance photopic-and-not-radiance, luminance photopic-and-radiance, irradiance neither, radiance radiometric-radiance). With no legal-corner law to carry, a capability set would publish a set algebra no gate reads over two facts consumed at different sites: `Photopic` drives the 683 lm/W divide inside the row's own `Admit` and `CanonicalIsRadiance` gates the row write at `WithEmission`, neither read reconstructing the other. The kernel law requires the owner to SAY SO where a pair survives, which is what the declaration states.
- Law: every arm of `EmissionSpectrum` resolves under the ONE scene-linear working space — the `Blackbody`, `Standard`, `Chromatic`, and `Spectral` arms construct through the config-explicit `Unicolour` constructors over `PortValue.SceneLinear` (or its `Observer.Degree10` sibling when the policy selects the CIE 1964 observer), so the Planckian table, the daylight-locus polynomial, the illuminant white points, and the SPD integration all resolve INTO AP1-linear channels; constructing under `Configuration.Default` and re-tagging the sRGB-linear channels as Acescg is the silent primary-shift defect. The luminous↔radiometric coercion runs a real divide: 683 lm/W — exact only at the 555 nm photopic peak — scaled by the source's `EfficacyRatio` (luminance cd/m² → radiance W/(sr·m²); luminous flux lm → radiant flux W; luminous intensity cd → radiant intensity W/sr; illuminance lux → irradiance W/m²), while a radiometric quantity passes its SI-base magnitude through. `EfficacyRatio` is the source's radiant-power fraction inside the photopic band — unity at the monochromatic anchor, below it for a broadband emitter — and `PhotometricPolicy.Of` is its ONE producer: a `Spectral` arm folds `SpectralCurve.LuminousEfficacy()`, the scale-invariant ∫V(λ)S(λ)/∫S(λ) over the curve's own grid, while every spectrum-less arm takes the DECLARED unity default. The package SPD→XYZ path normalizes Y against the reference white, so the absolute fraction is unrecoverable from a relative tristimulus readout and the measured fold reads the curve directly; the 683 lm/W ideal is the anchor the divide uses, never the efficacy of a real emitter, so the unity default is a stated idealization a measured source replaces. The divide composes AFTER the coercion so a dimensionally-admitted luminous magnitude always lowers to its radiometric twin. `WithEmission` gates the row write on `EmissionInput.Source.CanonicalIsRadiance`: `EmissionLuminance` is a radiance column, and a flux, illuminance, irradiance, or power magnitude needs the emitting area and solid angle no appearance row carries, so the band refuses with a typed fault rather than admitting a conversion nothing can perform. Emission that resolves outside the working RGB gamut (a 1800 K blackbody, a near-locus monochromatic SPD) gamut-maps through `MapToRgbGamut(GamutMap.OklchChromaReduction)` with the mapping recorded on `EmissionInput.GamutMapped` — never an RGB clamp, never a negative channel propagated into the lobe math, never a fault for a physically-real chromaticity; only a non-finite resolve rails `MaterialFault.Gamut`.
- Boundary: `Photometric` NEVER re-mints a unit owner OR a conversion receipt — it admits `UnitsNet` IN-FOLDER through `MaterialUnits` (the seam the Compute `ARCHITECTURE [04]` enshrines: AEC admits UnitsNet in-folder) and CONSTRUCTS the seam `Rasm.Element/Properties/quantity#UNIT_SCHEME` `MeasureEvidence` from that coercion's own columns. Composing the seam receipt upward is not the forbidden reach: the refusal names `Rasm.Compute`, the app-platform unit owner, and stands unchanged. A `Gated` row gates through `MaterialUnits.Admit(family, value, unit, key, correlation)` — `Quantity.TryFrom` constructs, `typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)` gates (the name check load-bearing because UnitsNet collapses lumen/candela/cd·m⁻² to the one luminous-intensity dimension), and ONE `ToUnit(UnitSystem.SI)` derives the SI magnitude and the canonical-unit witness the receipt names, crossing no quantity type into an interior signature. A `Borrowed` row (UnitsNet has no `Radiance`/`RadiantIntensity` quantity — steradian is dimensionally absent in SI) takes the pure `MaterialUnits.Coerce` prefix rescale over its named `IrradianceUnit`/`PowerUnit` SI-base enum. The gate and the radiometric divide COMPOSE in the one `PhotometricQuantity.Admit` row method: coerce to `Measure.CanonicalValue`, THEN derive `RadiometricSi` for a photopic row. Every admission here records `UnitResolution.Declared`, because a caller hands this owner the unit beside the magnitude and no header row or policy default ever supplies one — the inferred and assumed postures have no site on this page. The `Blackbody` case resolves `new Unicolour(config, bb.Cct, bb.Locus, bb.Luminance)` on either the Planckian or the CIE Daylight locus (Planck's law and the daylight polynomial owned by Unicolour, never re-derived); the per-locus CCT guard is inline in the arm — any finite positive CCT on the Planckian arm, the `Radiometry.DaylightCctMinKelvin`–`DaylightCctMaxKelvin` (4000–25000 K, the CIE 15 D-series polynomial domain) range on the daylight arm — with a finite non-negative luminance on both authored arms; a non-zero `Duv` is Planckian-arm-only, bounded `|Duv| ≤ Radiometry.DuvValidBound` (the package `Temperature.IsValid` domain), and resolves `new Unicolour(config, new Temperature(cct, duv), luminance)` — the binned LED admits without a parallel case. `Standard` resolves `illuminant.GetWhitePoint(policy.Observer).Chromaticity` through the config-explicit chromaticity constructor (the `Illuminant.Spd` is `internal`, so the case lowers through the public white-point surface); `Chromatic` guards its point inside the xy triangle (finite, `X ≥ 0`, `Y > 0`, `X + Y ≤ 1`) and lowers through the SAME chromaticity constructor — a datasheet source and a standard illuminant differ only by where the point comes from. `Spectral` carries an already-admitted `SpectralCurve`, whose own `Of` proved the interval against the `Spd.IsValid` domain, the extent, and non-negativity (a measured SPD is non-negative by definition), so the arm resolves rather than re-checks — the same carrier `acquisition#ACQUISITION` freezes to a durable spectral EXR, because a sampled spectrum is one shape whether it measures reflectance or emission. The resolved color projects its `Temperature` readout (CCT + Duv; `IsValid` marks |Duv| ≤ 0.05 where a CCT is chromatically meaningful, `IsHighAccuracy` the 1000–20000 K search band — the ANSI C78.377 binning discriminant), `DominantWavelength`/`ExcitationPurity`, and `RelativeLuminance` (the measured Y the `EmissionInput` construction divides OUT, so `Radiance` is unit-Y chromaticity by construction, intensity carries all the energy, and Y survives as receipt evidence) onto `EmissionInput` — every readout composed from the Unicolour surface, never re-derived. The `EmissionEvidence` receipt rides `EmissionInput.Provenance` so a consumer distinguishes a gated-and-rescaled luminous admission from a raw radiometric passthrough. A non-finite or negative admission rails `MaterialFault.Parameter` (band 2450), never a sentinel emission; a non-finite emission RGB rails `MaterialFault.Gamut`. EVERY AUTHORED LIGHT MAGNITUDE IN THE FOLDER CROSSES `Photometric.Admit` — a declared unit with no admission behind it is a claim, not a quantity — and the folder holds exactly two such sites beside this page's own graph entry: the `environment#SKY_MODEL` zenith level (already composed, so a cd/m² sky and a lux sky reach one radiometric scalar with no page-local efficacy divide) and the `environment#ENVIRONMENT_MAP` `Intensity`, whose admitted `EmissionEvidence` lets an HDRI authored in `lux` and one authored as a bare multiplier stay distinguishable at every domain `Scale` read and in analytics; generated `Set.Ibl.intensity` carries only the resolved scalar, so the authored evidence stays a named loss rather than a fabricated wire field. A dimensionless multiplier admits as `PhotometricQuantity.Radiance` with `RadiometricSi == Measure.CanonicalValue`, so the unitless case costs one construction and no branch. Reciprocally, `Raster/set#TEXTURE_SET` `ChannelUnit` is the PER-TEXEL PROJECTION of this same band and not a second unit vocabulary: each row names the `PhotometricQuantity` this page's admissions already gate through and READS its UCUM code off that row's own `Ucum` column — so a channel's declared unit and the folder's admitted unit are one fact read at two grains, never two rosters that drift, and a channel carrying no light quantity states the UCUM unity `1` on its own roster. UCUM lives here rather than on the channel roster because UnitsNet publishes no UCUM surface: its abbreviation cache yields display renderings (`cd/m²`, Unicode superscript) a reader consumes and a wire cannot. It is a QUANTITY-ROW column rather than a unit-keyed side table, because the code names what `Measure.CanonicalValue` is measured in and the row's own `Coercion` already fixes that — a lookup that could MISS made naming a quantity on a wire a second success that failed by exception, and covered none of the per-steradian rows whose SI-base enum is dimensionally silent about the steradian. High-luminance colour DIFFERENCE reads the HDR-correct metrics on the composed Unicolour selector — `Difference(candidate, DeltaE.Itp)` over the `Ictcp`/`Jzazbz` PQ-grounded spaces where CIELAB's luminance model breaks past the diffuse-white anchor — as one more `DeltaE` policy value on the caller's existing metric column, never a second colour owner or a photometric-local difference kernel.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Properties;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Graph;
using UnitsNet;
using UnitsNet.Units;
using Rasm.Numerics;
using Wacton.Unicolour;
using Thinktecture;
using static LanguageExt.Prelude;
using Temperature = Wacton.Unicolour.Temperature;   // UnitsNet also exports Temperature; the bare name pins the CCT readout

namespace Rasm.Materials.Appearance.Photometric;

// --- [TYPES] -------------------------------------------------------------------------------
// The closed gate-or-rescale discriminant: Gated rows own a published UnitsNet family (dimension+name gate, then
// ONE ToUnit(UnitSystem.SI) deriving the SI magnitude AND the canonical-unit witness); Borrowed rows own the
// per-steradian prefix rescale over a named SI-base enum. One column replaces the prior Family/CanonicalUnit pair
// where each was meaningful only when the other was absent.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Coercion {
    private Coercion() { }

    public sealed record Gated(QuantityInfo Family) : Coercion;
    // Family is DECLARED beside the borrowed SI-base enum because the receipt's family column crosses the wire, and
    // reading it off the enum's runtime type spells `IrradianceUnit` where the quantity is a radiance.
    public sealed record Borrowed(Enum Canonical, string Family) : Coercion;

    internal Fin<EmissionEvidence> Admit(double value, Enum unit, Op key, Guid correlation) =>
        Switch(
            state: (Value: value, Unit: unit, Key: key, Correlation: correlation),
            gated:    static (s, g) => MaterialUnits.Admit(g.Family, s.Value, s.Unit, s.Key, s.Correlation),
            borrowed: static (s, b) => MaterialUnits.Coerce(s.Value, s.Unit, b.Canonical, s.Key)
                .Map(si => EmissionEvidence.Raw(s.Value, s.Unit, si, b.Canonical, b.Family, s.Correlation)));
}

// One light-unit band: each row binds its Coercion case, the Photopic marker driving the 683 lm/W divide, and the
// radiance-vs-scalar discriminant. Admit composes coerce THEN divide in one place, so a luminous row is BOTH
// dimensionally admitted AND lowered. `nit` is LuminanceUnit.Nit on Luminance, not a row.
[SmartEnum<int>]
public sealed partial class PhotometricQuantity {
    public static readonly PhotometricQuantity Illuminance       = new(0, new Coercion.Gated(UnitsNet.Illuminance.Info),            photopic: true,  canonicalIsRadiance: false, ucum: "lx");
    public static readonly PhotometricQuantity Luminance         = new(1, new Coercion.Gated(UnitsNet.Luminance.Info),              photopic: true,  canonicalIsRadiance: true,  ucum: "cd/m2");
    public static readonly PhotometricQuantity LuminousFlux      = new(2, new Coercion.Gated(UnitsNet.LuminousFlux.Info),           photopic: true,  canonicalIsRadiance: false, ucum: "lm");
    public static readonly PhotometricQuantity LuminousIntensity = new(3, new Coercion.Gated(UnitsNet.LuminousIntensity.Info),      photopic: true,  canonicalIsRadiance: true,  ucum: "cd");
    public static readonly PhotometricQuantity Irradiance        = new(4, new Coercion.Gated(UnitsNet.Irradiance.Info),             photopic: false, canonicalIsRadiance: false, ucum: "W/m2");
    public static readonly PhotometricQuantity RadiantFlux       = new(5, new Coercion.Gated(UnitsNet.Power.Info),                  photopic: false, canonicalIsRadiance: false, ucum: "W");
    public static readonly PhotometricQuantity Radiance          = new(6, new Coercion.Borrowed(IrradianceUnit.WattPerSquareMeter, "Radiance"), photopic: false, canonicalIsRadiance: true,  ucum: "W/(sr.m2)");
    public static readonly PhotometricQuantity RadiantIntensity  = new(7, new Coercion.Borrowed(PowerUnit.Watt, "RadiantIntensity"),  photopic: false, canonicalIsRadiance: true,  ucum: "W/sr");

    public Coercion Coercion { get; }

    // THE TWO MARKERS STAY BOOLS, and the kernel CapabilitySet law decides it: a boolean product collapses only
    // where a subset of its corners is legal, and ALL FOUR corners here carry a real row. The two facts read at
    // different sites — Photopic drives the 683 lm/W divide inside this row's Admit, CanonicalIsRadiance gates the
    // write at WithEmission — neither asking the other, so the law's own carve applies and the owner states it.
    public bool Photopic { get; }
    public bool CanonicalIsRadiance { get; }

    // UCUM is a ROW COLUMN because it is a property of the QUANTITY, not of a unit enum a lookup has to find: the
    // code names what CanonicalValue is measured in, which the row's own Coercion fixes, so a row cannot exist
    // without its wire spelling. The prior side-table made naming a quantity on a wire a second success that failed
    // by exception. Owned HERE because UnitsNet publishes DISPLAY renderings and no UCUM surface at all.
    public string Ucum { get; }

    // The ONE row coercion: SI-base magnitude through the Coercion case, then the radiometric-twin derivation for a
    // photopic row. INTERNAL, because Photometric.Admit is the single admission door and a public row method here
    // would be a second ungated ingress.
    internal Fin<EmissionEvidence> Admit(double value, Enum unit, double efficacyRatio, Op key, Guid correlation) =>
        Coercion.Admit(value, unit, key, correlation)
            .Map(evidence => evidence with {
                RadiometricSi = Photopic ? evidence.Measure.CanonicalValue / (Radiometry.LuminousEfficacy * efficacyRatio) : evidence.Measure.CanonicalValue });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmissionSpectrum {
    private EmissionSpectrum() { }

    // Duv is the planckian offset an ANSI C78.377 bin quotes beside its CCT — 0.0 IS the locus, and a non-zero
    // offset is Planckian-arm-only, resolving through the Temperature constructor.
    public sealed record Blackbody(double Cct, double Luminance, Locus Locus = Locus.Blackbody, double Duv = 0.0) : EmissionSpectrum;

    public sealed record Standard(Illuminant Illuminant, double Luminance) : EmissionSpectrum;

    // The datasheet source: a luminaire/LED specification quotes an xy point plus output — the fourth canonical
    // emission spec form beside CCT, named illuminant, and measured SPD.
    public sealed record Chromatic(Chromaticity Point, double Luminance) : EmissionSpectrum;

    // The measured SPD reads the folder-root SpectralCurve carrier acquisition#ACQUISITION owns: a sampled spectrum
    // is one shape whether it measures reflectance or emission, so this arm resolves a grid that carrier's own Of
    // already proved and carries no coefficient guard.
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
// package, never a Rasm.Compute reference (the acyclic strata forbids the AEC->app-platform edge). Conversion runs
// exactly once and the receipt carries plain strings/doubles, so no UnitsNet type crosses an interior signature.
// The seam MeasureEvidence IS the conversion half, its UnitResolution.Declared the posture every admission here
// takes. RadiometricSi is the photometric EXTENSION the seam has no column for, kept OUTSIDE that receipt.
public readonly record struct EmissionEvidence(MeasureEvidence Measure, double RadiometricSi) {
    // Gated receipt: ONE ToUnit(UnitSystem.SI) derives the SI magnitude AND the canonical-unit witness, so no
    // per-row canonical-unit column exists for gated rows.
    public static EmissionEvidence From(IQuantity quantity, Guid correlation) {
        IQuantity si = quantity.ToUnit(UnitSystem.SI);
        double canonical = si.As(si.Unit);
        return new(new MeasureEvidence(
            QuantityType.Create(quantity.QuantityInfo.Name), quantity.Unit.ToString(), quantity.As(quantity.Unit),
            si.Unit.ToString(), canonical, UnitResolution.Declared, correlation), canonical);
    }

    // Borrowed receipt: no UnitsNet quantity exists to read a family or SI unit from, so the row's Borrowed enum is
    // the SI-base witness for the prefix rescale and its DECLARED family string names the seam identity — reading
    // that off the enum's runtime type spells IrradianceUnit on a radiance.
    public static EmissionEvidence Raw(double originalValue, Enum originalUnit, double canonicalValue, Enum canonical, string family, Guid correlation) =>
        new(new MeasureEvidence(
            QuantityType.Create(family), originalUnit.ToString(), originalValue,
            canonical.ToString(), canonicalValue, UnitResolution.Declared, correlation), canonicalValue);
}

public static class MaterialUnits {
    // The UCUM correspondence lives on the PhotometricQuantity row, so this owner publishes no unit-code table and
    // no reader that can throw. A channel carrying no light quantity is dimensionless and states the UCUM unity "1"
    // on its own roster rather than asking a lookup for it.
    public static Fin<double> Coerce(double value, Enum from, Enum to, Op key) =>
        UnitConverter.TryConvert(value, from, to, out double converted)
            ? Fin.Succ(converted)
            : new MaterialFault.Parameter(key, $"<unit-convert:{from}->{to}>");

    // The name check is load-bearing: UnitsNet collapses lumen/candela/cd·m⁻² to one luminous-intensity dimension,
    // so dimensions alone cannot distinguish a flux from an intensity from a luminance.
    public static Fin<EmissionEvidence> Admit(QuantityInfo family, double value, Enum unit, Op key, Guid correlation) =>
        Quantity.TryFrom(value, unit, out IQuantity? typed) && typed.QuantityInfo.Name == family.Name && typed.Dimensions.Equals(family.BaseDimensions)
            ? Fin.Succ(EmissionEvidence.From(typed, correlation))
            : new MaterialFault.Parameter(key, $"<unit-admit:{unit}:outside:{family.Name}>");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The canonical emission payload: unit-Y scene-linear chromaticity, exposure-scaled radiometric-SI intensity, the
// Temperature CCT+Duv readout (the ANSI C78.377 binning discriminant), the chromaticity pair, the MEASURED relative
// luminance the construction divided out (Radiance is unit-Y BY CONSTRUCTION, so Intensity carries the energy),
// the gamut-map evidence, and the threaded provenance. Every readout composes off the ONE resolved Unicolour
// BEFORE normalization, so the receipt witnesses the resolve, not the normalized product.
public readonly record struct EmissionInput(
    Unicolour Radiance, double Intensity, PhotometricQuantity Source,
    double DominantWavelengthNm, double ExcitationPurity, Temperature Temperature, double RelativeLuminance,
    bool GamutMapped, EmissionEvidence Provenance) {

    // Normalization re-anchors on the ONE Degree2 scene-linear carrier the graph consumes — the observer did its
    // work during integration and channels are AP1/D65 either way; a zero-Y emission normalizes to itself.
    public static EmissionInput Of(Unicolour sceneLinear, double intensity, PhotometricQuantity source, bool gamutMapped, EmissionEvidence canonical) {
        var (rgb, y) = (sceneLinear.RgbLinear, sceneLinear.RelativeLuminance);
        Unicolour chroma = y > 0.0
            ? new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R / y, rgb.G / y, rgb.B / y)
            : sceneLinear;
        return new(chroma, intensity, source, sceneLinear.DominantWavelength, sceneLinear.ExcitationPurity,
            sceneLinear.Temperature, y, gamutMapped, canonical);
    }
}

// Observer is a policy column, not a knob: Degree2 the point-source default, Degree10 the large-field architectural
// readout, selecting the white-point projection AND the SPD/CCT integration observer.
public readonly record struct PhotometricPolicy(EmissionSpectrum Spectrum, double Exposure, double EfficacyRatio, Observer Observer) {
    public static readonly PhotometricPolicy Neutral = new(new EmissionSpectrum.Constant(1.0, 1.0, 1.0), Exposure: 1.0, EfficacyRatio: 1.0, Observer.Degree2);

    // Of is the MEASURED mint and the ONE producer of EfficacyRatio. A Spectral arm carries its own source SPD, so
    // the photopic-band fraction is a property of that curve: SpectralCurve.LuminousEfficacy() folds the
    // scale-invariant ∫V(λ)S(λ)/∫S(λ) over the curve's own grid, surviving where the package's relative tristimulus
    // readout cannot recover it. Every other arm takes the DECLARED unity default, the 555 nm anchor at which the
    // 683 lm/W divide is exact — a stated idealization a broadband emitter reads high against.
    public static PhotometricPolicy Of(EmissionSpectrum spectrum, double exposure, Observer observer) =>
        new(spectrum, exposure,
            spectrum is EmissionSpectrum.Spectral measured ? measured.Curve.LuminousEfficacy() : 1.0,
            observer);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Photometric {
    // efficacyRatio is the photopic-band radiant-power fraction, (0,1] by physics: zero divides the radiometric twin
    // to infinity, so it gates with the magnitude.
    public static Fin<EmissionEvidence> Admit(PhotometricQuantity quantity, double value, Enum unit, Op key, Guid correlation, double efficacyRatio = 1.0) =>
        double.IsFinite(value) && value >= 0.0 && efficacyRatio is > 0.0 and <= 1.0
            ? quantity.Admit(value, unit, efficacyRatio, key, correlation)
            : new MaterialFault.Parameter(key, $"<photometric-magnitude:{quantity.Key}:{value:R}@{efficacyRatio:R}>");

    public static Fin<EmissionInput> Resolve(PhotometricQuantity quantity, double value, Enum unit, PhotometricPolicy policy, Op key, Guid correlation) =>
        from _ in guard(double.IsFinite(policy.Exposure) && policy.Exposure >= 0.0,
            new MaterialFault.Parameter(key, $"<photometric-exposure:{policy.Exposure:R}>"))
        from canonical in Admit(quantity, value, unit, key, correlation, policy.EfficacyRatio)
        from resolved in SceneLinear(policy.Spectrum, policy.Observer, key)
        select EmissionInput.Of(resolved.Colour, canonical.RadiometricSi * policy.Exposure, quantity, resolved.Mapped, canonical);

    // WithEmission makes the payload the CONSTRUCTOR of the row's two emission columns: an authored row and an
    // admitted one stay distinguishable because only this path leaves an EmissionEvidence.
    // THE QUANTITY BAND GATES THE WRITE. EmissionLuminance is a RADIANCE column, so only a row answering
    // CanonicalIsRadiance may fill it: a flux, illuminance, irradiance, or power magnitude measures a whole emitter
    // or receiving surface, and normalizing one needs the AREA and SOLID ANGLE no appearance row carries. Gating
    // HERE rather than at Resolve is deliberate — Resolve's receipt is faithful for ANY quantity, which is what the
    // sky and environment consumers read.
    public static Fin<MaterialParameters> WithEmission(MaterialParameters row, EmissionInput emission, Op key) =>
        emission.Source.CanonicalIsRadiance
            ? MaterialParameters.Of(
                row with { Emission = emission.Radiance, EmissionLuminance = emission.Intensity, EmissionProvenance = Some(emission) }, key)
            : new MaterialFault.Parameter(key, $"<emission-quantity-not-radiance:{emission.Source.Key}:{emission.Source.Ucum}>");

    // Observer selects WHICH of the two graph-owned Acescg instances integrates; both mint at PortValue, so the
    // working-space cache identity stays countable at one owner.
    static Configuration WorkingSpace(Observer observer) =>
        observer == Observer.Degree10 ? PortValue.SceneLinearDegree10 : PortValue.SceneLinear;

    // Every authored arm guards its inputs BEFORE construction and constructs CONFIG-EXPLICIT under the scene-linear
    // working space, so .RgbLinear is AP1-linear everywhere.
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
                : new MaterialFault.Parameter(s.Key, $"<photometric-{(bb.Locus == Locus.Daylight ? "daylight" : "blackbody")}-cct:{bb.Cct:R}@{bb.Luminance:R}:duv={bb.Duv:R}>"),
            standard: static (s, st) => double.IsFinite(st.Luminance) && st.Luminance >= 0.0
                ? Gate(new Unicolour(WorkingSpace(s.Observer), st.Illuminant.GetWhitePoint(s.Observer).Chromaticity, st.Luminance), s.Key)
                : new MaterialFault.Parameter(s.Key, $"<photometric-illuminant-luminance:{st.Luminance:R}>"),
            chromatic: static (s, c) =>
                double.IsFinite(c.Point.X) && double.IsFinite(c.Point.Y) && c.Point.X >= 0.0 && c.Point.Y > 0.0 && c.Point.X + c.Point.Y <= 1.0
                    && double.IsFinite(c.Luminance) && c.Luminance >= 0.0
                ? Gate(new Unicolour(WorkingSpace(s.Observer), c.Point, c.Luminance), s.Key)
                : new MaterialFault.Parameter(s.Key, $"<photometric-chromaticity:{c.Point.X:R},{c.Point.Y:R}@{c.Luminance:R}>"),
            spectral: static (s, sp) => Gate(new Unicolour(WorkingSpace(s.Observer), sp.Curve.ToSpd()), s.Key),
            constant: static (s, c) => Gate(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, c.R, c.G, c.B), s.Key));

    // Non-finite rails loud; an out-of-working-gamut emission bounds through the kernel GamutPolicy.Perceptual row
    // with the mapping recorded — never an RGB clamp, never a negative channel into the lobe math, never a fault
    // for a physically-real chromaticity.
    static Fin<(Unicolour Colour, bool Mapped)> Gate(Unicolour colour, Op key) {
        var rgb = colour.RgbLinear;
        return !double.IsFinite(rgb.R) || !double.IsFinite(rgb.G) || !double.IsFinite(rgb.B)
            ? new MaterialFault.Gamut(key, "<emission-non-finite-rgb>")
            : GamutPolicy.Perceptual.Contains(colour)
                ? Fin.Succ((colour, Mapped: false))
                : Fin.Succ((GamutPolicy.Perceptual.Bound(colour), Mapped: true));
    }

}
```

## [03]-[RESEARCH]

(none)
