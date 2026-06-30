# [MATERIALS_GLAZING]

THE GLAZING `ComponentFamily` GROUNDED IN THE INSULATING-GLASS BUILD PHYSICS — the IGU layer-set vocabulary the one `Component/component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Glazing` case, `ComponentClass.Minor` (an IGU is an `IfcElementComponent` standardized part, not a space-bounding `IfcBuiltElement`), `ComputedSection` `None` (an insulating-glass unit crosses to IFC 4.3 as an `IfcMaterialLayerSet`, never an `IfcProfileDef` profile, so glazing contributes NO `ComponentCatalogue.Sections` entry and the seam `ProfileResolution` dereferences a glazing `ComponentId` to `(Component, None)`). The cross-section is the `ComponentSection.Glazing(GlazingSection)` arm — the per-`Pane` glass substance / thickness / surface `Coating` / `Interlayer`, the per-`Cavity` `CavityFill` (the EN 673 gas mixture or the ISO 19916 vacuum), the warm/cold-edge `SpacerType`, the EN 1279 `EdgeSeal`, the captured pane face dimensions, and the optional `MuntinGrid` the build pane count derives over. An asymmetric `6-16-4`, a coated `6-16-6-lowe`, a laminated `66.4-16-4`, and a vacuum `4-vac-4` are all one `GlazingSection` with per-pane sub-rows — never a uniform single-pane-thickness placeholder a designation suffix leaves unmodeled, never a per-unit `DoubleGlazedUnit` type. A glazing unit owns its DEFINING engineering performance COMPUTED from the build (never a stored scalar that drifts): the `GlazingThermal` EN 673 center-of-glass `Ug` over ONE shared series-resistance chain (the conductive + convective-Nusselt + emissivity-radiative gas exchange, the pillar-conduction + free-molecular + radiative vacuum exchange), the EN 410 / ISO 9050 multi-layer net solar factor `g` (`τe` plus the inward-flowing absorbed-heat secondary flux `qi` the SAME resistance chain partitions) and visible transmittance `τv`, and the mass-law `Acoustic` spectrum its `Rw` derives from — lowered through `GlazingSection.ToProperties` into the `Composition/material#MATERIAL_PROPERTY` seam `MaterialPropertySet` set (`Thermal`/`Acoustic`/`Environmental`/`Fire`) so a `Rasm.Compute` energy route, a `Rasm.Bim` `Pset_*ThermalTransmittance` emitter, and a façade designer read the IGU's `U`/`g`/`τv`/`Rw` off the seam material, and through `GlazingSection.ToLayerSet` into the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` the layer-set assignment owner serializes. The vocabulary grows by data — a new IGU is one `GlazingRow`, a new glass a `GlassType` case, a new coating a `Coating` case, a new gas a `CavityGas` case, an interlayer a `Interlayer` case, a quad-pane build one `GlazingBuild` row — never a per-unit type. The page composes `Component/component#COMPONENT_OWNER` for the `Component`/`ComponentSection`/`ComponentId`/`ComponentStandard`/`ComponentFault` shape, `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor` for the pane-cavity buildup, the `Composition/material#MATERIAL_PROPERTY` seam `MaterialPropertySet` for the engineering receipt, `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic`/`AcousticBand`/`RatingContour` for the airborne spectrum and `Rw`, the `Properties/quantity#MEASURE_VALUE` `MeasureValue`/`QuantityType`/`Dimension` for every measured column, the `Rasm.Vectors` `PositiveMagnitude` for every length column, and the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` row each pane shades to; the masonry/cmu/steel/timber families land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [02]-[GLAZING_FAMILY]: the `GlassType` glass-substance axis (normal emissivity / conductivity / density / specific heat / raw-substance GWP-per-kg / thermal-form process adder / solar+visible transmittance+reflectance / safety), the `Coating` low-E/solar-control axis (`Option<double>` corrected emissivity + solar/visible transmittance multipliers + coated-surface reflectance + sputter process adder), the `CavityGas` EN 673 Annex-B fill axis (conductivity/density/viscosity/specific-heat), the `Interlayer` lamination axis (PVB/SGP/EVA ply nominal + damping + shear + conductivity + density + GWP), the `Sealant`/`Desiccant` EN 1279 edge-seal vocabularies, the `MuntinStyle` grid axis, the `SpacerType` warm/cold-edge axis (Ψ + sight-line + conductivity + edge-seal GWP), the `GlazingBuild` pane-count classification, the `CavityFill` `[Union]` (gas mixture vs ISO 19916 vacuum), the `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` rows and the `GlazingSection` pane-stack record (the `ComponentSection.Glazing` payload + `ToProperties`/`ToLayerSet`), the `GlazingThermal` EN 673 + EN 410 + mass-law kernel over ONE resistance chain, the `GlazingPerformance` receipt, the `GlazingGwp` substance/process-split carbon vector, and the `ComponentCatalogue.BuildGlazingRows` row table.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing unit vocabulary (`GlassType` the glass-substance axis carrying its thermal-radiative-optical identity, `Coating` the low-E/solar-control surface axis, `CavityGas` the EN 673 Annex-B fill, `Interlayer` the PVB/SGP/EVA lamination axis, `Sealant`/`Desiccant`/`MuntinStyle` the EN 1279 edge-seal and grid vocabularies, `SpacerType` the warm/cold-edge spacer, `GlazingBuild` the pane-count classification, `CavityFill` the gas-vs-vacuum `[Union]`, `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` the stack rows, `GlazingSection` the pane-stack record that IS the `ComponentSection.Glazing` payload); `GlazingThermal` the EN 673 center-of-glass + EN 410 net-`g` + mass-law kernel sharing ONE series-resistance chain; `GlazingGwp` the substance/process-split carbon vector; `GlazingPerformance` the computed receipt; `ComponentCatalogue.BuildGlazingRows` the registered-row seed `component#COMPONENT_OWNER` folds.
- Cases: glass {float, low-iron, tempered, heat-strengthened, fire-rated} — each `GlassType` carrying its uncoated normal emissivity, its conductivity (soda-lime 1.00, borosilicate 1.14), its EN 572-1 density (soda-lime 2500, borosilicate 2230), its specific heat (soda-lime 720, borosilicate 830), its EN 15804 raw-substance GWP-per-kg (soda-lime 1.43, low-iron 1.50, borosilicate 2.00 — the ONLY substance variants), its thermal-form process GWP-per-m², its solar+visible transmittance+reflectance, and its safety class; a laminated pane is NOT a glass case — it is any glass plus an `Interlayer`, so the prior `laminated` glass case is the deleted redundancy. Coating {none, hard-coat-lowE (pyrolytic), soft-coat-lowE-double-silver, soft-coat-lowE-triple-silver, solar-control} — each carrying the `Option<double>` corrected emissivity (`None` for the uncoated surface, replacing the `double.NaN` sentinel), the EN 410 solar/visible transmittance multipliers, the `Option<double>` coated-surface reflectance, and the sputter/pyrolytic process GWP. Gas {air, argon, krypton, xenon} — the four EN 673 Annex-B properties. Interlayer {none, pvb, sgp, eva}; spacer {warm-edge-stainless, warm-edge-foam, cold-edge-aluminum}; build {double, triple} DERIVED from `Panes.Count`. A section is a `GlazingSection` over `Seq<Pane>`/`Seq<Cavity>`/`SpacerType`/`EdgeSeal`/face-dimensions/`Option<MuntinGrid>`/EI-minutes, never a section subtype.
- Entry: `public Fin<GlazingPerformance> Performance(Op key)` computes the EN 673 `Ug`, the EN 410 net-`g`/`τv`, and the mass-law `Acoustic` spectrum (`Fin` because the spectrum admits through the seam's public `Acoustic.Of` band gate); `public Fin<Seq<MaterialPropertySet>> ToProperties(Op key)` lowers that receipt into the seam set (`Thermal` the Ug + mass-weighted conductivity + vapour-tight μ, `Acoustic` the banded spectrum, `Environmental` the substance/process-split GWP, `Fire` the parameterized EI rating where `FireResistanceEiMinutes > 0`); `public Fin<MaterialComposition> ToLayerSet(Op key)` the IGU pane-cavity-pane `LayerSet` bridge (a laminated pane its glass-interlayer-glass sub-stack); the stored `PositiveMagnitude OverallThicknessMm` unit-build depth the frame seam and the `component#COMPONENT_OWNER` `ComponentSection.CrossNominalMm`/`GrossRectangleMm` glazing arms read (admitted ONCE at `Of` as a typed field, never a throwing re-mint inside the `Component.Of` `Fin` guard); `GlazingSection.Of(panes, cavities, spacer, edgeSeal, faceWidthMm, faceHeightMm, muntin, fireEiMinutes, key)` admits a build once — panes = cavities + 1, the pane count one the `GlazingBuild` vocabulary names, face dimensions positive, the overall build-depth positive, the fire-rated-pane⟺positive-EI relation total — railing `ComponentFault.Family` on a malformed stack. There is NO `ToUnit` rectangle: the face dimensions live on the section directly, replacing the prior hardcoded `1200 mm` frame-rebate placeholder.
- Packages: Rasm.Vectors (`PositiveMagnitude` for every pane/gap/face/pillar/bar column — the kernel atom; NOT `Rasm.Domain`), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`/`MaterialComposition`/`MaterialPropertySet`/`MeasureValue`/`QuantityType`/`Dimension`/`MeasurementBasis`/`LifecycleStage`/`Acoustic`/`AcousticBand`/`FireRating`/`FireResistance`), Rasm.Materials.Construction (`CompositionAuthor` the seam-`MaterialComposition` author the `ToLayerSet` bridge calls), Rasm.Materials.Component (`Component`/`ComponentSection`/`ComponentId`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentCatalogue` the parent `COMPONENT_OWNER`), Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet (`RatioUnit.DecimalFraction` via the seam `MeasureValue` for the dimensionless `g`/`τv`). VividOrange is NOT composed (glazing carries no `IfcProfileDef` section). Wacton.Unicolour is NOT composed here: a coating's OPTICAL signal crosses the seam as the coated pane's content-keyed `Appearance/graph#MATERIAL_LIBRARY` `Node.Appearance` the `Appearance/interchange#MATERIAL_WIRE` owner mints with Unicolour — glazing tags the `MaterialId`, never the colour kernel.
- Growth: a new IGU is one `GlazingRow` with its `PaneRow`/`CavityRow` sub-rows; a new glass substance one `GlassType` case; a new coating tier one `Coating` case; a new gas one `CavityGas` case; a new interlayer one `Interlayer` case; a new edge-seal sealant/desiccant one `Sealant`/`Desiccant` case; a quad-pane build one more `GlazingBuild` case the derived `Build` read maps; an electrochromic variant a `GlassType` case plus a `Coating` row — never a per-unit type, never a per-family section variant. The full per-pane spectral `τ(λ)`/`ρ(λ)` ray-trace (the angular EN 410 §5 spectral integral) is a `GlassType`/`Coating` per-wavelength-curve column growth the broadband multi-layer recursion here is the center-of-glass simplification of, never a parallel optical owner.
- Boundary: the glazing vocabulary is a realized `ComponentFamily` whose DOMAIN RECEIPT is the IGU thermal/optical/acoustic performance, not a rectangle a per-unit class carries — a per-unit class AND a uniform single-pane-thickness section (which cannot represent the asymmetric `6-16-4`, the coated `6-16-6-lowe`, the laminated `66.4`, or the vacuum `4-vac-4`) are the deleted forms; `GlazingSection` carries a `Seq<Pane>` (each `GlassType` + `PositiveMagnitude ThicknessMm` + `Coating` + the coated-surface index + an `Interlayer` + its total thickness so multi-ply lamination is first-class) and a `Seq<Cavity>` (each `CavityFill` `[Union]` arm + `PositiveMagnitude WidthMm` so a 90%-argon-10%-air mixture and a 0.08 Pa vacuum are both honest), every length column the kernel `PositiveMagnitude` (double-backed `> 0` finite) so a 4 mm pane, a 6.76 mm laminate, a 0.3 mm vacuum gap, and a 16 mm cavity admit without truncation. `GlazingThermal.Evaluate` computes ONE ordered series-resistance chain (the external/internal surface films `he = 23`, `hi = 8 W·m⁻²·K⁻¹`, each pane's conductive resistance over the glass-plus-interlayer thickness, each cavity's total conductance) that BOTH the EN 673 `Ug = 1/ΣR` AND the EN 410 secondary heat flux `qi = Σ αe,i·R_out,i/R_tot` read — the cavity conductance dispatching on the `CavityFill` arm: a gas cavity sums the EN 673 convective (the Nusselt number over the Rayleigh number the volume-mixed gas density/viscosity/specific-heat and the gap width drive) and the radiative (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹` over the two facing pane corrected emissivities, so a low-E `0.04` surface collapses the exchange an uncoated `0.837` dominates); a vacuum cavity sums the Collins pillar conduction (`2·λ_glass·a/p²` over the pillar radius and pitch), the free-molecular residual-gas conduction (proportional to the ISO 19916 residual pressure), and the same radiative term. The EN 410 solar factor `g` is the multi-layer net transmittance — the panes combined left-to-right through the two-flux `(τ, ρ_front, ρ_back)` recursion (a coated surface's reflectance asymmetric front-to-back) plus the per-pane absorptance `αe,i` whose inward-flowing fraction `R_out,i/R_tot` the shared resistance chain partitions — so a clear double reads a real `g` below the single-pane value, a solar-control coat collapses it further, and the inter-reflection uplift the prior product-of-transmittances dropped is restored; `g`/`τv` are dimensionless `Ratio` `MeasureValue`s (`RatioUnit.DecimalFraction`, the seam `IsDimensionless` path). The `Acoustic` spectrum is the field-incidence mass law `R(f) = 20·log₁₀(m'·f) − 47 dB` over the total areal mass (glass plus interlayer) at the seventeen `AcousticBand` centres with the interlayer coincidence-damping and asymmetric-pane bonuses, the `Rw` the seam `RatingContour.Rw.Fit` so the IGU rating and an assembly rating share ONE contour fit. `ToProperties` lowers the receipt so the IGU material "has it all" (never an element Pset — the `Projection/component#COMPONENT_PROJECTOR` reads the material's own set); the `SpacerType.PsiWmK` edge-seal linear thermal bridge is the `Rasm.Compute` `AssemblyAggregator` whole-window `Uw` input read off the receipt directly (the app-platform downward strata edge), never a glazing-section field and never a Bim row, glazing OWNING the `Ug` + the `Ψg` datum, Compute OWNING their area-and-perimeter combination. `ToLayerSet` is the ONE bridge to the seam `LayerSet` (pane / cavity / pane, the laminated pane a glass-interlayer-glass sub-stack, a vacuum cavity an `IsVentilated`-false sealed gap), each `MaterialLayer` carrying its `Dimension` thickness and the clear-glass `MaterialId` (`glass.crown` the panes and the transparent interlayer, `glass.flint` a fire-rated pane, `gas.cavity` the sealed-cavity row) — the low-E `Coating` riding the coated pane's `Node.Appearance` the `Appearance/graph#MATERIAL_LIBRARY` carries, never a fabricated `glass.lowe` bulk row; the `MuntinGrid` is FACE geometry (bars across the pane), not a through-thickness layer, so it stays section metadata the geometry generator reads, never a `MaterialLayer`. `ComponentCatalogue.BuildGlazingRows` seeds the registry keyed `glazing.<designation>`, grounded in the EN 1279 IGU builds and the EN 673 / EN 410 / ISO 9050 published gas, coating, and optical properties.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (the kernel >0 finite magnitude atom — NOT Rasm.Domain)
using Rasm.Domain;                   // Context, Op, AcceptValidated (the admission context + boundary-admission key + validated-accept extension)
using Rasm.Element;                  // MaterialId, MaterialComposition, MaterialPropertySet, MeasureValue, QuantityType, MeasurementBasis,
                                     // LifecycleStage, Acoustic, AcousticBand, FireRating, FireResistance
using Rasm.Materials.Construction;   // CompositionAuthor (the seam-MaterialComposition author the ToLayerSet bridge calls)
using Rasm.Materials.Component;      // Component, ComponentSection, ComponentId, ComponentStandard, ComponentAuthority, ComponentFault, Coring, ComponentCatalogue (the parent COMPONENT_OWNER)
using Thinktecture;
using UnitsNet;                      // RatioUnit (the dimensionless g/τv fraction unit, admitted through the seam MeasureValue)
using Dimension = Rasm.Element.Dimension;   // the SI-dimension axis (ThermalTransmittance) — disambiguated from the Rasm.Vectors discrete-count Dimension the magnitude atoms share a namespace with
using static LanguageExt.Prelude;

// Each family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the sibling ComponentCatalogue static classes
// are distinct types (one shared namespace is a CS0101 collision); component#COMPONENT_OWNER stays the parent
// Rasm.Materials.Component and folds Glazing.ComponentCatalogue.BuildGlazingRows by the sub-namespace-qualified name.
namespace Rasm.Materials.Component.Glazing;

// --- [TYPES] -------------------------------------------------------------------------------
// The glass-substance axis: each row carries the uncoated NORMAL EMISSIVITY (soda-lime εn 0.837, the EN 673 Annex-A baseline
// the cavity radiative term reads absent a coating), the CONDUCTIVITY (soda-lime 1.00, borosilicate 1.14 W·m⁻¹·K⁻¹), the
// EN 572-1 DENSITY (soda-lime 2500, borosilicate 2230 kg·m⁻³), the EN 572-1 SPECIFIC HEAT (soda-lime 720, borosilicate 830
// J·kg⁻¹·K⁻¹), the EN 15804 RAW-SUBSTANCE GWP-per-kg (the cradle-to-gate pane substance ONLY — low-iron and borosilicate the
// only substance variants; all secondary process carbon is the per-m² adders, never double-counted into this base), the
// thermal-FORM process GWP-per-m² (tempering/heat-strengthening/ceramic-firing), the broadband EN 410 / ISO 9050 SOLAR and
// VISIBLE transmittance and reflectance (the multi-layer net-g recursion reads), and the safety class. A laminated pane is
// any glass plus an Interlayer, not a glass case.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassType {
    public static readonly GlassType Float            = new("float",             normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: false);
    public static readonly GlassType LowIron          = new("low-iron",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.50, formProcessGwpPerM2: 0.0, solarTransmittance: 0.90, solarReflectance: 0.080, visibleTransmittance: 0.91, visibleReflectance: 0.08, safety: false);
    public static readonly GlassType Tempered         = new("tempered",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: true);
    public static readonly GlassType HeatStrengthened = new("heat-strengthened", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.9, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: false);
    public static readonly GlassType FireRated        = new("fire-rated",        normalEmissivity: 0.837, conductivityWmK: 1.14, densityKgM3: 2230.0, specificHeatJKgK: 830.0, substanceGwpPerKg: 2.00, formProcessGwpPerM2: 5.0, solarTransmittance: 0.70, solarReflectance: 0.070, visibleTransmittance: 0.85, visibleReflectance: 0.08, safety: true);
    public double NormalEmissivity { get; }
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double SpecificHeatJKgK { get; }
    public double SubstanceGwpPerKg { get; }
    public double FormProcessGwpPerM2 { get; }
    public double SolarTransmittance { get; }
    public double SolarReflectance { get; }
    public double VisibleTransmittance { get; }
    public double VisibleReflectance { get; }
    public bool Safety { get; }

    // The library appearance row the LayerSet bridge tags each pane with (the Appearance/graph#MATERIAL_LIBRARY clear-glass
    // row, the heavier flint for fire-rated borosilicate). A low-E/solar-control COATING is a thin-film surface effect, NOT
    // a bulk shade: the coated pane's optical signal crosses the seam as the content-keyed Node.Appearance the
    // Appearance/interchange#MATERIAL_WIRE owner mints, so this read takes no Coating knob.
    public MaterialId Appearance => this == FireRated ? MaterialId.Of("glass.flint") : MaterialId.Of("glass.crown");
}

// The low-E / solar-control coating axis: each row carries the Option<double> CORRECTED NORMAL EMISSIVITY the EN 673 cavity
// radiative term reads (None for the uncoated surface — the glass NormalEmissivity stands, REPLACING the prior double.NaN
// sentinel; a hard-coat pyrolytic 0.16, a soft-coat double-silver 0.04, a triple-silver 0.02 vs the uncoated 0.837), the
// EN 410 solar/visible TRANSMITTANCE multipliers a coated surface applies over the base glass transmittance, the
// Option<double> coated-surface SOLAR/VISIBLE REFLECTANCE (the elevated reflectance the multi-layer recursion reads on the
// coated face; None falls back to the glass reflectance), and the sputter/pyrolytic process GWP-per-m². A new coating tier
// is one row, never a parallel coated-pane owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coating {
    public static readonly Coating None          = new("none",             correctedEmissivity: Option<double>.None, solarTransmittanceMultiplier: 1.00, coatedSolarReflectance: Option<double>.None, visibleTransmittanceMultiplier: 1.00, coatedVisibleReflectance: Option<double>.None, processGwpPerM2: 0.0);
    public static readonly Coating HardCoatLowE   = new("hard-coat-lowe",    correctedEmissivity: Some(0.16), solarTransmittanceMultiplier: 0.80, coatedSolarReflectance: Some(0.20), visibleTransmittanceMultiplier: 0.95, coatedVisibleReflectance: Some(0.11), processGwpPerM2: 0.5);
    public static readonly Coating SoftCoatDouble = new("soft-coat-double",  correctedEmissivity: Some(0.04), solarTransmittanceMultiplier: 0.55, coatedSolarReflectance: Some(0.30), visibleTransmittanceMultiplier: 0.90, coatedVisibleReflectance: Some(0.11), processGwpPerM2: 2.0);
    public static readonly Coating SoftCoatTriple = new("soft-coat-triple",  correctedEmissivity: Some(0.02), solarTransmittanceMultiplier: 0.40, coatedSolarReflectance: Some(0.34), visibleTransmittanceMultiplier: 0.82, coatedVisibleReflectance: Some(0.12), processGwpPerM2: 3.0);
    public static readonly Coating SolarControl   = new("solar-control",     correctedEmissivity: Some(0.04), solarTransmittanceMultiplier: 0.30, coatedSolarReflectance: Some(0.40), visibleTransmittanceMultiplier: 0.55, coatedVisibleReflectance: Some(0.15), processGwpPerM2: 3.0);
    public Option<double> CorrectedEmissivity { get; }
    public double SolarTransmittanceMultiplier { get; }
    public Option<double> CoatedSolarReflectance { get; }
    public double VisibleTransmittanceMultiplier { get; }
    public Option<double> CoatedVisibleReflectance { get; }
    public double ProcessGwpPerM2 { get; }
}

// The cavity fill gas: the four EN 673 Annex-B properties at the 283 K mean cavity temperature — CONDUCTIVITY λ (air 0.0250,
// argon 0.0173, krypton 0.0094, xenon 0.0054 W·m⁻¹·K⁻¹), DENSITY ρ (kg·m⁻³), dynamic VISCOSITY μ (air 1.761e-5, argon
// 2.164e-5 Pa·s), SPECIFIC HEAT c (J·kg⁻¹·K⁻¹) — so the convective Nusselt/Rayleigh term reads a typed gas receipt. A
// mixture is the volume-weighted blend of the fill gas and the CavityFill.GasFill balance gas, computed in the kernel.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityGas {
    public static readonly CavityGas Air     = new("air",     conductivityWmK: 0.0250, densityKgM3: 1.232, viscosityPaS: 1.761e-5, specificHeatJKgK: 1008.0);
    public static readonly CavityGas Argon   = new("argon",   conductivityWmK: 0.0173, densityKgM3: 1.699, viscosityPaS: 2.164e-5, specificHeatJKgK: 519.0);
    public static readonly CavityGas Krypton = new("krypton", conductivityWmK: 0.0094, densityKgM3: 3.560, viscosityPaS: 2.345e-5, specificHeatJKgK: 245.0);
    public static readonly CavityGas Xenon   = new("xenon",   conductivityWmK: 0.0054, densityKgM3: 5.689, viscosityPaS: 2.299e-5, specificHeatJKgK: 161.0);
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double ViscosityPaS { get; }
    public double SpecificHeatJKgK { get; }
}

// The lamination interlayer axis (EN 14449 / EN 12543): each row carries the NOMINAL single-ply thickness (PVB 0.38, SGP
// 0.89, EVA 0.38 mm — multi-ply is a thicker captured total), the ACOUSTIC coincidence-dip DAMPING bonus, the SHEAR MODULUS
// (PVB ~2, SGP ~110, EVA ~8 MPa — the structural stiffness the laminate transfers), the CONDUCTIVITY (~0.2 W·m⁻¹·K⁻¹, the
// pane conductive resistance adds), the DENSITY, the raw-substance GWP-per-kg, and the lamination process GWP-per-m². None
// is the monolithic pane (zero thickness). A new interlayer chemistry is one row, never a parallel laminate owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Interlayer {
    public static readonly Interlayer None = new("none", nominalPlyMm: 0.0,  acousticDampingDb: 0.0, shearModulusMPa: 0.0,   conductivityWmK: 1.00, densityKgM3: 0.0,    substanceGwpPerKg: 0.0, processGwpPerM2: 0.0);
    public static readonly Interlayer Pvb  = new("pvb",  nominalPlyMm: 0.38, acousticDampingDb: 3.0, shearModulusMPa: 2.0,   conductivityWmK: 0.20, densityKgM3: 1070.0, substanceGwpPerKg: 3.40, processGwpPerM2: 1.5);
    public static readonly Interlayer Sgp  = new("sgp",  nominalPlyMm: 0.89, acousticDampingDb: 2.0, shearModulusMPa: 110.0, conductivityWmK: 0.20, densityKgM3: 950.0,  substanceGwpPerKg: 4.20, processGwpPerM2: 2.0);
    public static readonly Interlayer Eva  = new("eva",  nominalPlyMm: 0.38, acousticDampingDb: 2.5, shearModulusMPa: 8.0,   conductivityWmK: 0.23, densityKgM3: 950.0,  substanceGwpPerKg: 2.90, processGwpPerM2: 1.4);
    public double NominalPlyMm { get; }
    public double AcousticDampingDb { get; }
    public double ShearModulusMPa { get; }
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double SubstanceGwpPerKg { get; }
    public double ProcessGwpPerM2 { get; }
}

// The EN 1279-2 edge-seal sealant: the primary moisture barrier (PIB butyl), the structural/durability secondary seal
// (silicone is structural-glazing-rated, polysulfide and hot-melt-butyl are not), each carrying its structural class and
// the seal process GWP-per-perimeter-metre.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sealant {
    public static readonly Sealant Pib          = new("pib",            structural: false, processGwpPerM: 0.10);
    public static readonly Sealant Polysulfide  = new("polysulfide",    structural: false, processGwpPerM: 0.20);
    public static readonly Sealant Silicone     = new("silicone",       structural: true,  processGwpPerM: 0.25);
    public static readonly Sealant HotMeltButyl = new("hot-melt-butyl", structural: false, processGwpPerM: 0.12);
    public bool Structural { get; }
    public double ProcessGwpPerM { get; }
}

// The EN 1279-2 spacer desiccant: the molecular sieve or silica fill drying the sealed cavity, carrying its water-adsorption
// capacity (kg water per kg desiccant) the durability/dew-point reserve reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Desiccant {
    public static readonly Desiccant MolecularSieve3A = new("molecular-sieve-3a", adsorptionCapacity: 0.22);
    public static readonly Desiccant Silica           = new("silica",             adsorptionCapacity: 0.30);
    public double AdsorptionCapacity { get; }
}

// The muntin/grid style: a true-divided structural grid, a simulated-divided applied grille, or a between-glass grille in
// the cavity. The bar dimensions are MANUFACTURER values (no EN/ASTM table grounds them), captured on the MuntinGrid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MuntinStyle {
    public static readonly MuntinStyle TrueDivided      = new("true-divided");
    public static readonly MuntinStyle SimulatedDivided = new("simulated-divided");
    public static readonly MuntinStyle BetweenGlass     = new("between-glass");
}

// The edge-seal spacer axis: each row carries the EN ISO 10077-1 linear thermal-bridge Ψg (warm-edge stainless 0.04, warm-
// edge foam 0.03, cold-edge aluminium 0.11 W·m⁻¹·K⁻¹), the SIGHT-LINE width (the visible spacer face), the spacer-frame
// CONDUCTIVITY (the conductive edge the warm-edge spacer suppresses), and the spacer+seal fabrication GWP-per-perimeter-
// metre. The spacer DEPTH is the cavity gap (read from the cavity, not stored). The whole-window Uw that combines Ψg with
// the EN 673 center-of-glass Ug and the frame fraction is a Rasm.Compute AssemblyAggregator concern reading PsiWmK off the
// glazing receipt directly — glazing OWNS the Ug + the Ψg datum, never the perimeter combination.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpacerType {
    public static readonly SpacerType WarmEdgeStainless = new("warm-edge-stainless", psiWmK: 0.04, sightLineWidthMm: 6.5, conductivityWmK: 16.0,  edgeSealGwpPerM: 0.30);
    public static readonly SpacerType WarmEdgeFoam      = new("warm-edge-foam",      psiWmK: 0.03, sightLineWidthMm: 6.5, conductivityWmK: 0.30,  edgeSealGwpPerM: 0.28);
    public static readonly SpacerType ColdEdgeAluminum  = new("cold-edge-aluminum",  psiWmK: 0.11, sightLineWidthMm: 6.0, conductivityWmK: 160.0, edgeSealGwpPerM: 0.25);
    public double PsiWmK { get; }
    public double SightLineWidthMm { get; }
    public double ConductivityWmK { get; }
    public double EdgeSealGwpPerM { get; }
}

// The IGU build classification DERIVED from the pane count — the IFC layer-count semantics carried as a vocabulary, never a
// stored field a malformed "double" row with three panes contradicts.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlazingBuild {
    public static readonly GlazingBuild Double = new("double", panes: 2);
    public static readonly GlazingBuild Triple = new("triple", panes: 3);
    public int Panes { get; }

    public static Option<GlazingBuild> OfPaneCount(int panes) =>
        Items.FirstOrDefault(b => b.Panes == panes) is { } build ? Some(build) : None;
}

// The cavity fill discriminant: a GAS fill (the EN 673 mixture — the fill gas at its concentration FillFraction with the
// remaining volume the Balance gas, typically air) or a VACUUM fill (the ISO 19916 VIG — the residual pressure plus the
// support-pillar geometry the Collins pillar-conduction model reads). The thermal kernel dispatches the cavity conductance
// on the arm: a gas cavity convects (Nusselt) and radiates, a vacuum cavity conducts through its pillars and the free-
// molecular residual gas and radiates with no convection.
[Union]
public abstract partial record CavityFill {
    public sealed record GasFill(CavityGas Gas, double FillFraction, CavityGas Balance) : CavityFill;
    public sealed record VacuumFill(double ResidualPressurePa, PositiveMagnitude PillarRadiusMm, PositiveMagnitude PillarPitchMm) : CavityFill;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The volume-mixed gas property carrier the kernel computes from a CavityFill.GasFill — the EN 673 §B.2 volume-fraction
// blend of the fill gas and the balance gas, read by the Nusselt convective term.
public readonly record struct GasProperties(double ConductivityWmK, double DensityKgM3, double ViscosityPaS, double SpecificHeatJKgK);

// One pane in the IGU stack: its glass substance, its TOTAL thickness, its surface Coating, the coated-surface index
// (0 = outboard face, 1 = inboard face — EN 673 reading the coated surface that faces the cavity), and an Interlayer with
// its total thickness (0 / Interlayer.None for a monolithic pane; > 0 for a laminate the glass-interlayer-glass sub-stack
// splits). The glass-only thickness is the total minus the interlayer.
public readonly record struct Pane(GlassType Glass, PositiveMagnitude ThicknessMm, Coating Coating, int CoatedSurface, Interlayer Interlayer, double InterlayerThicknessMm) {
    public bool IsLaminated => Interlayer != Interlayer.None && InterlayerThicknessMm > 0.0;
    public double GlassThicknessMm => ThicknessMm.Value - InterlayerThicknessMm;

    // The EN 673 emissivity of ONE of the pane's two surfaces: the Coating's corrected εn on the COATED surface (a coating
    // suppresses radiation across the cavity it faces, not the opposite face), else the glass NormalEmissivity — so the
    // CoatedSurface index is load-bearing and a coating on the wrong surface does not falsely lower the adjacent cavity.
    public double EmissivityOf(int surface) =>
        surface == CoatedSurface ? Coating.CorrectedEmissivity.IfNone(Glass.NormalEmissivity) : Glass.NormalEmissivity;

    // The pane's directional solar optics (transmittance, front reflectance, back reflectance) the EN 410 multi-layer
    // recursion combines: the transmittance the glass base scaled by the coating multiplier, the COATED surface carrying the
    // coating's elevated reflectance and the opposite surface the glass reflectance, so a low-E/solar-control pane is
    // asymmetric front-to-back.
    public (double T, double Rf, double Rb) Solar() {
        double t = Glass.SolarTransmittance * Coating.SolarTransmittanceMultiplier;
        double rCoated = Coating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance);
        return (t, CoatedSurface == 0 ? rCoated : Glass.SolarReflectance, CoatedSurface == 1 ? rCoated : Glass.SolarReflectance);
    }

    public (double T, double Rf, double Rb) Visible() {
        double t = Glass.VisibleTransmittance * Coating.VisibleTransmittanceMultiplier;
        double rCoated = Coating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance);
        return (t, CoatedSurface == 0 ? rCoated : Glass.VisibleReflectance, CoatedSurface == 1 ? rCoated : Glass.VisibleReflectance);
    }
}

// One cavity (gap) in the IGU stack: the fill discriminant and the gap width the EN 673 / ISO 19916 conductance read.
public readonly record struct Cavity(CavityFill Fill, PositiveMagnitude WidthMm);

// The EN 1279-2 edge-seal construction: the primary moisture sealant (PIB), the structural/durability secondary sealant,
// the spacer desiccant, and whether the spacer corners are keyed (vs bent) — the durability + edge-thermal + GWP datums.
public readonly record struct EdgeSeal(Sealant Primary, Sealant Secondary, Desiccant Desiccant, bool CorneredKeys);

// The face muntin/grid: the style, the horizontal/vertical bar counts, and the bar width/depth (manufacturer dimensions).
// FACE geometry the generator places across the pane, never a through-thickness MaterialLayer.
public readonly record struct MuntinGrid(MuntinStyle Style, int HorizontalBars, int VerticalBars, PositiveMagnitude BarWidthMm, PositiveMagnitude BarDepthMm);

// The computed IGU receipt the seam MaterialPropertySet lowering reads — the DEFINING glazing performance COMPUTED from the
// build (never a stored scalar): the EN 673 center-of-glass U-value, the EN 410 net solar factor g (SHGC) and visible
// transmittance τv (dimensionless Ratio measures), and the mass-law Acoustic spectrum the Rw derives from.
public readonly record struct GlazingPerformance(
    MeasureValue UgCenterOfGlass,
    MeasureValue SolarFactorG,
    MeasureValue LightTransmittanceTv,
    Acoustic Acoustic) {
    public int Rw => Acoustic.Rw;
}

// The pane-stack section — the ComponentSection.Glazing payload: the panes, the cavities (panes = cavities + 1), the edge-
// seal spacer, the EN 1279-2 edge-seal construction, the captured face dimensions (REPLACING the prior hardcoded 1200 mm
// frame-rebate placeholder; the per-m² Ug/g/τv are size-independent, the face feeds the geometry + the perimeter the
// spacer Ψg multiplies in Compute's whole-window Uw), the optional face muntin grid, and the EN 13501-2 fire-resistance EI
// minutes (0 absent a fire-rated pane). The build is DERIVED from the pane count.
public readonly record struct GlazingSection(
    Seq<Pane> Panes,
    Seq<Cavity> Cavities,
    SpacerType Spacer,
    EdgeSeal EdgeSeal,
    PositiveMagnitude FaceWidthMm,
    PositiveMagnitude FaceHeightMm,
    PositiveMagnitude OverallThicknessMm,
    Option<MuntinGrid> Muntin,
    int FireResistanceEiMinutes) {

    // TOTAL by construction: Of admits ONLY a pane count the GlazingBuild vocabulary names, so OfPaneCount is always Some
    // and the IfNone is the defensive-unreachable — never a silent mislabel of an out-of-vocabulary stack as Double.
    public GlazingBuild Build => GlazingBuild.OfPaneCount(Panes.Count).IfNone(GlazingBuild.Double);

    // The admission gate: panes = cavities + 1 (an IGU alternates pane/cavity/pane), the pane count one the GlazingBuild
    // vocabulary names (a quad landing as one more GlazingBuild row this gate then admits), the fire-rated-pane⟺positive-EI
    // relation total (a fire-rated pane requires a declared EI class and a declared EI requires a fire-rated pane, so the
    // Fire property the lowering emits is always backed), and the face dimensions positive. The OverallThicknessMm unit-build
    // depth is admitted ONCE here as a stored PositiveMagnitude (the pane + cavity thickness sum, each an already-positive
    // PositiveMagnitude so the sum is positive) — the cmu ActualWidthMm stored-field pattern — so the
    // component#COMPONENT_OWNER ComponentSection.GrossRectangleMm/CrossNominalMm glazing arms READ the typed field rather than
    // re-minting through a throwing PositiveMagnitude.Create inside Component.Of's Fin guard (a GlazingSection built by any
    // path other than Of cannot empty Panes — the arity gate forbids it — so the field is total and the projection is
    // symmetric with the eight already-typed sibling arms). A malformed arity, an unmodeled pane count, or a fire/EI mismatch
    // rails the ComponentFault.Family band rather than seeding a unit whose DERIVED Build silently mislabels.
    public static Fin<GlazingSection> Of(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, double faceWidthMm, double faceHeightMm, Option<MuntinGrid> muntin, int fireEiMinutes, Op key) =>
        panes.IsEmpty || cavities.Count != panes.Count - 1
            ? ComponentFault.Family(key, $"<glazing-stack-arity:panes={panes.Count}:cavities={cavities.Count}>")
            : GlazingBuild.OfPaneCount(panes.Count).IsNone
                ? ComponentFault.Family(key, $"<glazing-build-unmodeled-pane-count:{panes.Count}>")
                : panes.Exists(static p => p.Glass == GlassType.FireRated) != (fireEiMinutes > 0)
                    ? ComponentFault.Family(key, $"<glazing-fire-rating-mismatch:ei={fireEiMinutes}>")
                    : from fw in key.AcceptValidated<PositiveMagnitude>(candidate: faceWidthMm)
                      from fh in key.AcceptValidated<PositiveMagnitude>(candidate: faceHeightMm)
                      from overall in key.AcceptValidated<PositiveMagnitude>(candidate: panes.Sum(static p => p.ThicknessMm.Value) + cavities.Sum(static c => c.WidthMm.Value))
                      select new GlazingSection(panes, cavities, spacer, edgeSeal, fw, fh, overall, muntin, fireEiMinutes);

    // The EN 673 center-of-glass U-value + the EN 410 net-g/τv projection + the mass-law Acoustic spectrum, COMPUTED through
    // the GlazingThermal kernel. Fin because the Acoustic spectrum admits through the seam's public Acoustic.Of band gate.
    public Fin<GlazingPerformance> Performance(Op key) => GlazingThermal.Evaluate(this, key);

    // The seam MaterialPropertySet set the IGU material carries (Composition/material#MATERIAL_PROPERTY) — the Thermal case
    // the Ug as a HeatTransferCoefficient MeasureValue plus the mass-weighted glass conductivity and specific heat and the
    // vapour-tight μ, the Acoustic the banded spectrum (Rw a derived read), the Environmental the substance/process-split
    // per-m² GWP, the Fire case the parameterized EN 13501-2 EI rating where the section carries positive EI minutes.
    public Fin<Seq<MaterialPropertySet>> ToProperties(Op key) =>
        from perf in Performance(key)
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: WeightedGlassConductivity(),
            specificHeat: WeightedGlassSpecificHeat(),
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6,                        // glass + sealed IGU: vapour-tight (EN ISO 13788 μ → ∞)
            key)
        from environmental in MaterialPropertySet.OfEnvironmental(
            MeasurementBasis.PerM2, MaterialPropertySet.Environmental.CarbonMatrix(GlazingGwp.StagesPerM2(this)),
            recycledContent: 0.25, endOfLifeRecovery: 0.90, epd: "EN 15804 generic insulating glass unit", validUntilYear: 0, key)
        let acoustic = MaterialPropertySet.OfAcoustic(perf.Acoustic)
        let core = Seq(thermal, acoustic, environmental)
        select FireResistanceEiMinutes > 0
            ? core.Add(MaterialPropertySet.OfFire(FireRating.A1, FireResistance.Ei(FireResistanceEiMinutes)))
            : core;

    // The IGU pane-cavity-pane stack lowers to a seam MaterialComposition.LayerSet via the Construction/assembly
    // CompositionAuthor (the seam owns the composition + the MeasureValue thickness coercion); a laminated pane splits into
    // its glass-interlayer-glass sub-layers within the pane thickness.
    public Fin<MaterialComposition> ToLayerSet(Op key) => CompositionAuthor.LayerSet(LayerRows(), key);

    // The mass-weighted glass conductivity / specific heat the Thermal case carries as the homogenized IGU columns (the seam
    // Thermal column is a single λ/c; the per-cavity gas/vacuum resistance is the Ug the kernel already computed, so these
    // are the glass-only mass-weighted means a non-IGU thermal read uses, never the dropped cavity physics).
    double WeightedGlassConductivity() {
        double mass = Panes.Sum(static p => p.GlassThicknessMm), acc = Panes.Sum(static p => p.Glass.ConductivityWmK * p.GlassThicknessMm);
        return mass > 0.0 ? acc / mass : 1.0;
    }
    double WeightedGlassSpecificHeat() {
        double mass = Panes.Sum(static p => p.GlassThicknessMm), acc = Panes.Sum(static p => p.Glass.SpecificHeatJKgK * p.GlassThicknessMm);
        return mass > 0.0 ? acc / mass : 720.0;
    }

    // The alternating glass / cavity / glass layer rows: each pane its GlassType/Coating appearance id (a laminated pane its
    // glass-interlayer-glass sub-stack), each cavity the gas.cavity id, the layer NAME carrying the glass / coating / fill
    // as the human-readable IfcMaterialLayer.Name the Bim egress round-trips (the coating's optical effect rides the coated
    // pane's Node.Appearance, not a Bim parse of this label).
    Seq<(MaterialId Material, double ThicknessMm, string Name)> LayerRows() =>
        toSeq(Enumerable.Range(0, Panes.Count + Cavities.Count))
            .Bind(slot => (slot & 1) == 0
                ? PaneLayers(Panes[slot / 2], slot / 2)
                : Seq((MaterialId.Of("gas.cavity"), Cavities[slot / 2].WidthMm.Value, CavityLayerName(Cavities[slot / 2], slot / 2))));

    // A monolithic pane is one glass layer; a laminated pane is glass / interlayer / glass — the interlayer split out of the
    // pane thickness so the IGU's IfcMaterialLayerSet carries the laminate sub-stack. The interlayer shades as the clear
    // glass row (an optically near-glass transparent polymer — its laminate identity rides the layer NAME, not a fabricated
    // polymer appearance row).
    static Seq<(MaterialId Material, double ThicknessMm, string Name)> PaneLayers(Pane pane, int index) =>
        pane.IsLaminated
            ? Seq(
                (pane.Glass.Appearance, pane.GlassThicknessMm / 2.0, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}-outer"),
                (MaterialId.Of("glass.crown"), pane.InterlayerThicknessMm, $"pane-{index}-{pane.Interlayer.Key}-interlayer"),
                (pane.Glass.Appearance, pane.GlassThicknessMm / 2.0, $"pane-{index}-{pane.Glass.Key}-inner"))
            : Seq((pane.Glass.Appearance, pane.ThicknessMm.Value, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}"));

    static string CavityLayerName(Cavity c, int index) => c.Fill.Switch(
        gasFill: gas => $"cavity-{gas.Gas.Key}-{gas.FillFraction:P0}-{index}",
        vacuumFill: vac => $"cavity-vacuum-{vac.ResidualPressurePa:R}pa-{index}");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 673 center-of-glass U-value + the EN 410 / ISO 9050 net-g/τv projection + the mass-law Acoustic spectrum — the
// glazing family's domain-physics owner. Evaluate computes ONE ordered series-resistance chain (surface films, per-pane
// conductive resistance, per-cavity conductance) that BOTH the Ug (1/ΣR) AND the EN 410 secondary heat flux qi read, so the
// optical and thermal kernels share the resistance network rather than re-deriving it.
public static class GlazingThermal {
    const double SurfaceExternalWmK = 23.0;        // EN 673 external surface coefficient he (W·m⁻²·K⁻¹)
    const double SurfaceInternalWmK = 8.0;         // EN 673 internal surface coefficient hi
    const double StefanBoltzmann = 5.67e-8;        // σ (W·m⁻²·K⁻⁴)
    const double MeanTemperatureK = 283.0;         // EN 673 mean cavity temperature (10 °C)
    const double TemperatureDeltaK = 15.0;         // EN 673 reference ΔT across the cavity
    const double GravityMs2 = 9.81;
    const double MassLawOffsetDb = 47.0;           // field-incidence mass-law offset R = 20·log₁₀(m'·f) − 47
    const double FreeMolecularConductanceAirPerPa = 1.2;   // free-molecular (Knudsen-regime) air conduction W·m⁻²·K⁻¹·Pa⁻¹, near-unity accommodation — the VIG residual-gas term

    public static Fin<GlazingPerformance> Evaluate(GlazingSection section, Op key) {
        double[] rPane = section.Panes.Map(PaneConductiveResistance).ToArray();
        double[] rCav = new double[section.Cavities.Count];
        for (int i = 0; i < section.Cavities.Count; i++) rCav[i] = 1.0 / CavityConductance(section, i);
        double rse = 1.0 / SurfaceExternalWmK, rsi = 1.0 / SurfaceInternalWmK;
        double rTot = rse + rPane.Sum() + rCav.Sum() + rsi;
        double ug = 1.0 / rTot;
        double g = SolarFactor(section, rPane, rCav, rTot, rse);
        double tv = Span(section, 0, section.Panes.Count, static p => p.Visible()).T;
        return MassLawSpectrum(section, key).Map(acoustic => new GlazingPerformance(UValue(ug), Fraction(g, key), Fraction(tv, key), acoustic));
    }

    // Each pane's conductive resistance t/λ — the glass conductive path plus the interlayer's small sub-resistance (a thin
    // ~0.2 W·m⁻¹·K⁻¹ polymer, zero for a monolithic pane where InterlayerThicknessMm is 0).
    static double PaneConductiveResistance(Pane p) =>
        (p.GlassThicknessMm / 1000.0) / p.Glass.ConductivityWmK + (p.InterlayerThicknessMm / 1000.0) / p.Interlayer.ConductivityWmK;

    // Each cavity's total conductance h_total dispatched on the CavityFill arm. The cavity sees the INBOARD surface of pane i
    // (index 1) and the OUTBOARD surface of pane i+1 (index 0): a low-E coating lowers h_rad only when it sits on one of
    // these two cavity-facing surfaces. A gas cavity convects (Nusselt over the volume-mixed gas) and radiates; a vacuum
    // cavity conducts through the Collins pillar array (2·λ_glass·a/p² over the two bounding panes' mean conductivity) and
    // the free-molecular residual gas (∝ pressure) and radiates with no convection.
    static double CavityConductance(GlazingSection section, int i) {
        Cavity cavity = section.Cavities[i];
        double hRad = RadiativeCoefficient(section.Panes[i].EmissivityOf(1), section.Panes[i + 1].EmissivityOf(0));
        double s = cavity.WidthMm.Value / 1000.0;
        return cavity.Fill.Switch(
            gasFill: gas => {
                GasProperties p = EffectiveGas(gas);
                return Nusselt(p, s) * p.ConductivityWmK / s + hRad;
            },
            vacuumFill: vac => {
                double kGlass = 0.5 * (section.Panes[i].Glass.ConductivityWmK + section.Panes[i + 1].Glass.ConductivityWmK);
                double hPillar = 2.0 * kGlass * (vac.PillarRadiusMm.Value / 1000.0) / Math.Pow(vac.PillarPitchMm.Value / 1000.0, 2.0);
                return hPillar + FreeMolecularConductanceAirPerPa * vac.ResidualPressurePa + hRad;
            });
    }

    // The EN 673 §B.2 volume-fraction gas mixture: the fill gas at FillFraction blended with the balance gas filling the
    // remainder, each of the four properties linearly volume-weighted.
    static GasProperties EffectiveGas(CavityFill.GasFill gas) => new(
        Mix(gas.Gas.ConductivityWmK, gas.Balance.ConductivityWmK, gas.FillFraction),
        Mix(gas.Gas.DensityKgM3, gas.Balance.DensityKgM3, gas.FillFraction),
        Mix(gas.Gas.ViscosityPaS, gas.Balance.ViscosityPaS, gas.FillFraction),
        Mix(gas.Gas.SpecificHeatJKgK, gas.Balance.SpecificHeatJKgK, gas.FillFraction));

    static double Mix(double fill, double balance, double x) => x * fill + (1.0 - x) * balance;

    // EN 673 radiative coefficient h_r = 4·σ·T_m³ / (1/ε₁ + 1/ε₂ − 1): the two facing pane corrected emissivities drive the
    // cavity radiation — uncoated 0.837/0.837 yields the dominant exchange, a single low-E 0.04 surface collapses it by an
    // order of magnitude (the entire reason a coated cavity outperforms an uncoated one of the same gas/gap).
    static double RadiativeCoefficient(double e1, double e2) =>
        4.0 * StefanBoltzmann * MeanTemperatureK * MeanTemperatureK * MeanTemperatureK / (1.0 / e1 + 1.0 / e2 - 1.0);

    // EN 673 Nusselt number Nu = max(1, 0.035·Ra^0.38) for a vertical cavity over the Rayleigh number Ra =
    // ρ²·s³·g·c·ΔT / (T_m·μ·λ). Below the critical Rayleigh the cavity does not convect (Nu = 1, pure conduction λ/s); above
    // it the gas circulates and raises the conductance — the heavier krypton/xenon suppresses both terms.
    static double Nusselt(GasProperties gas, double s) {
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return Math.Max(1.0, 0.035 * Math.Pow(ra, 0.38));
    }

    // The EN 410 / ISO 9050 net solar factor g = τe + qi: the multi-layer transmittance τe (the panes combined through the
    // two-flux recursion) plus the secondary internal heat flux qi — each pane's absorptance αe,i times its inward-flowing
    // fraction R_out,i/R_tot, the inward fraction being the resistance from the outer environment to the pane centre over
    // the total resistance the SHARED chain already computed (the absorbed heat flows inward in proportion to the resistance
    // to the OTHER side). A clear double reads a real g below the single-pane value; a solar-control coat collapses it.
    static double SolarFactor(GlazingSection section, double[] rPane, double[] rCav, double rTot, double rse) {
        double te = Span(section, 0, section.Panes.Count, static p => p.Solar()).T;
        double qi = 0.0;
        for (int j = 0; j < section.Panes.Count; j++) {
            double rOut = rse + 0.5 * rPane[j];
            for (int k = 0; k < j; k++) rOut += rPane[k] + rCav[k];
            qi += SolarAbsorptance(section, j) * rOut / rTot;
        }
        return Math.Clamp(te + qi, 0.0, 1.0);
    }

    // The EN 410 per-pane absorptance with full inter-reflection: the forward flux density Φj incident on pane j (the outer
    // sub-stack transmittance over the multiple-reflection denominator between the outer back reflectance and the [pane j ⊕
    // inner] front reflectance) drives the front-incidence absorptance, plus the part transmitted through j and reflected
    // back by the inner sub-stack drives the back-incidence absorptance.
    static double SolarAbsorptance(GlazingSection section, int j) {
        (double T, double Rf, double Rb) o = Span(section, 0, j, static p => p.Solar());
        (double T, double Rf, double Rb) inn = Span(section, j + 1, section.Panes.Count, static p => p.Solar());
        (double T, double Rf, double Rb) pane = section.Panes[j].Solar();
        double aFwd = 1.0 - pane.T - pane.Rf;
        double aBwd = 1.0 - pane.T - pane.Rb;
        double rJin = pane.Rf + pane.T * pane.T * inn.Rf / (1.0 - pane.Rb * inn.Rf);
        double phi = o.T / (1.0 - o.Rb * rJin);
        return phi * (aFwd + pane.T * inn.Rf * aBwd / (1.0 - pane.Rb * inn.Rf));
    }

    // The two-system EN 410 combination of system a (outboard) onto system b (inboard): the transmittance, front
    // reflectance, and back reflectance of the combined system over the inter-reflection denominator 1 − ρa_back·ρb_front.
    static (double T, double Rf, double Rb) Combine((double T, double Rf, double Rb) a, (double T, double Rf, double Rb) b) {
        double d = 1.0 - a.Rb * b.Rf;
        return (a.T * b.T / d, a.Rf + a.T * a.T * b.Rf / d, b.Rb + b.T * b.T * a.Rb / d);
    }

    // The combined directional optics of the contiguous pane span [lo, hi) folded left-to-right; the empty span is the clear
    // identity (full transmission, zero reflectance). One Combine fold over a per-pane optics selector serves solar AND
    // visible.
    static (double T, double Rf, double Rb) Span(GlazingSection s, int lo, int hi, Func<Pane, (double T, double Rf, double Rb)> optics) {
        (double T, double Rf, double Rb) acc = (1.0, 0.0, 0.0);
        for (int i = lo; i < hi; i++) acc = Combine(acc, optics(s.Panes[i]));
        return acc;
    }

    // The field-incidence mass-law Acoustic spectrum: R(f) = 20·log₁₀(m'·f) − 47 dB over the total areal mass
    // m' = Σ(ρ_glass·t_glass + ρ_interlayer·t_interlayer) at the seventeen AcousticBand one-third-octave centres, with the
    // best interlayer's coincidence-dip damping bonus and an asymmetric-pane bonus (unequal panes shift the coincidence dips
    // apart). The absorption is glass's near-zero 0.03 flat; the Rw is the seam RatingContour.Rw.Fit read. Admits through the
    // seam's public Acoustic.Of band gate (the curated vectors valid by construction).
    static Fin<Acoustic> MassLawSpectrum(GlazingSection section, Op key) {
        double areal = section.Panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 + p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0);
        double bonus = section.Panes.Fold(0.0, static (acc, p) => Math.Max(acc, p.Interlayer.AcousticDampingDb)) + (Asymmetric(section) ? 2.0 : 0.0);
        double[] sri = new double[AcousticBand.Count];
        double[] absorption = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            sri[band.Index] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(areal, 1e-9) * band.CenterHz) - MassLawOffsetDb + bonus);
            absorption[band.Index] = 0.03;
        }
        return Acoustic.Of(absorption, sri, key);
    }

    // Asymmetric iff some pane thickness differs from the first — the unequal-pane coincidence-dip shift, computed without a
    // Distinct materialization (a scan over the panes against the first thickness).
    static bool Asymmetric(GlazingSection section) =>
        section.Panes.Count >= 2 && section.Panes.Exists(p => p.ThicknessMm.Value != section.Panes[0].ThicknessMm.Value);

    static MeasureValue UValue(double ug) =>
        MeasureValue.OfSi(QuantityType.Create("HeatTransferCoefficient"), Dimension.ThermalTransmittance, ug);

    // The dimensionless g / τv as a Ratio MeasureValue — RatioUnit.DecimalFraction admits through the seam IsDimensionless
    // path (no SI reprojection); the clamped [0,1] value is always finite, so Of succeeds and the IfFail never fires.
    static MeasureValue Fraction(double value, Op key) =>
        MeasureValue.Of(value, UnitsNet.Units.RatioUnit.DecimalFraction, key).IfFail(MeasureValue.Zero);
}

// The per-m² embodied-carbon stage vector ToProperties embeds into the seam Environmental case's (ImpactCategory ×
// LifecycleStage) matrix through MaterialPropertySet.Environmental.CarbonMatrix. The A1-A3 cradle-to-gate splits RAW
// SUBSTANCE (each pane's and interlayer's mass times its per-kg base) from SECONDARY PROCESSING (the per-m² thermal-form,
// coating-sputter, lamination, and IGU-assembly adders) so the per-kg base is never double-counted; A4-D scale the A1-A3 as
// the transport/install/end-of-life tail over the EN 15978 LifecycleStage banding (a negative D the recovery benefit beyond
// the system boundary).
public static class GlazingGwp {
    const double IguAssemblyGwpPerM2 = 2.5;   // EN 15804 IGU fabrication: spacer forming + gas fill + edge-seal + desiccant per m²

    public static ReadOnlyMemory<double> StagesPerM2(GlazingSection section) {
        double substance = section.Panes.Sum(static p =>
            p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 * p.Glass.SubstanceGwpPerKg
            + (p.IsLaminated ? p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0 * p.Interlayer.SubstanceGwpPerKg : 0.0));
        double processing = section.Panes.Sum(static p =>
            p.Glass.FormProcessGwpPerM2 + p.Coating.ProcessGwpPerM2 + (p.IsLaminated ? p.Interlayer.ProcessGwpPerM2 : 0.0))
            + IguAssemblyGwpPerM2;
        double a1a3 = substance + processing;
        double[] stages = new double[LifecycleStage.Count];
        stages[LifecycleStage.A1A3.Index] = a1a3;
        stages[LifecycleStage.A4.Index] = a1a3 * 0.05;
        stages[LifecycleStage.A5.Index] = a1a3 * 0.03;
        stages[LifecycleStage.C.Index] = a1a3 * 0.08;
        stages[LifecycleStage.D.Index] = -a1a3 * 0.15;   // recovery benefit beyond the system boundary
        return stages;
    }
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The bounded standards body is ComponentAuthority.En (the EN 1279 IGU authority, region "eu"); an IGU has no mortar
    // joint, so StandardJointThicknessMm is 0.0. The nominal vision face is the catalogue TYPE reference the occurrence
    // overrides per order (the Ug/g/τv are per-m², size-independent). The standard edge-seal is the PIB primary + polysulfide
    // secondary + molecular-sieve desiccant + keyed corners EN 1279-2 construction.
    static readonly ComponentStandard IguStandard = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly EdgeSeal StandardEdgeSeal = new(Sealant.Pib, Sealant.Polysulfide, Desiccant.MolecularSieve3A, CorneredKeys: true);
    const double NominalVisionWidthMm = 1200.0;
    const double NominalVisionHeightMm = 1500.0;
    const double VacuumIntegrityThresholdPa = 0.1;   // ISO 19916 functional-vacuum ceiling — a residual pressure above it is a compromised VIG

    // The EN 1279 IGU builds as full pane/cavity sub-tables — the asymmetric, inboard-coated, laminated, vacuum, fire-rated,
    // and gridded units are each their own pane rows, never a designation suffix the model leaves unmodeled. A pane row is
    // (glass, thickness, coating, coated-surface, interlayer, interlayer-thickness); a cavity row (gas, width, fill-fraction,
    // balance-gas, vacuum-pressure, pillar-radius, pillar-pitch) where gas "vacuum" selects the VacuumFill arm.
    static readonly Seq<GlazingRow> GlazingRows = Seq(
        new GlazingRow("glazing.double-4-16-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        new GlazingRow("glazing.double-6-12-6", "warm-edge-stainless",
            Seq(new PaneRow("float", 6.0, "none", 0, "none", 0.0), new PaneRow("float", 6.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 12.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        new GlazingRow("glazing.double-4-20-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 20.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        // The inboard pane carries a soft-coat double-silver low-E on its CoatedSurface 0 (the cavity-facing surface 3) — so
        // Pane.EmissivityOf(0) reads the εn 0.04 the cavity radiative term sees, the row honestly modeled.
        new GlazingRow("glazing.double-6-16-6-lowe", "warm-edge-stainless",
            Seq(new PaneRow("float", 6.0, "none", 0, "none", 0.0), new PaneRow("float", 6.0, "soft-coat-double", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        new GlazingRow("glazing.double-4-12-4-alu", "cold-edge-aluminum",
            Seq(new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("air", 12.0, 1.00, "air", 0.0, 0.0, 0.0)), 0, None),
        // The outboard pane is laminated 66.4 (two 3 mm glass + a 0.76 mm two-ply PVB interlayer) — the InterlayerThicknessMm
        // 0.76 the ToLayerSet splits into glass-PVB-glass and the MassLawSpectrum reads for the coincidence-damping bonus,
        // the 6.76-vs-4.0 asymmetry adding the dip-shift bonus.
        new GlazingRow("glazing.double-lam664-16-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 6.76, "none", 0, "pvb", 0.76), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        // Triple low-E on surfaces 2 and 5 (the outer pane inboard face + the inner pane outboard face) so each cavity sees
        // one low-E surface; krypton fill for the narrow gaps.
        new GlazingRow("glazing.triple-4-16kr-4-16kr-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "soft-coat-double", 1, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, "none", 0.0)),
            Seq(new CavityRow("krypton", 16.0, 0.90, "air", 0.0, 0.0, 0.0), new CavityRow("krypton", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        new GlazingRow("glazing.triple-4-12ar-4-12ar-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "soft-coat-double", 1, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 12.0, 0.90, "air", 0.0, 0.0, 0.0), new CavityRow("argon", 12.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, None),
        // ISO 19916 vacuum unit: two 4 mm panes, the outer pane carrying a soft-coat triple-silver low-E on surface 2 to
        // suppress the now-dominant radiative exchange, a 0.3 mm vacuum gap at 0.08 Pa with 0.25 mm-radius pillars on a
        // 20 mm pitch (the Collins pillar conduction the kernel reads).
        new GlazingRow("glazing.vig-4lowe-vac-4", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "soft-coat-triple", 1, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("vacuum", 0.3, 0.0, "", 0.08, 0.25, 20.0)), 0, None),
        // Fire-rated EI 30 unit: a 6 mm fire-rated borosilicate outboard pane (the fire side) + a 6 mm float inboard pane
        // with a soft-coat double-silver low-E on surface 3; the positive EI minutes the section gate requires of a fire-
        // rated pane drives the OfFire(A1, Ei(30)) lowering.
        new GlazingRow("glazing.fire-ei30-6fr-16-6", "warm-edge-stainless",
            Seq(new PaneRow("fire-rated", 6.0, "none", 0, "none", 0.0), new PaneRow("float", 6.0, "soft-coat-double", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 30, None),
        // A true-divided grid unit: one horizontal and two vertical 25 mm-wide × 20 mm-deep muntin bars (manufacturer dims),
        // the face geometry the generator places across the pane.
        new GlazingRow("glazing.double-4-16-4-grid", "warm-edge-stainless",
            Seq(new PaneRow("float", 4.0, "none", 0, "none", 0.0), new PaneRow("float", 4.0, "none", 0, "none", 0.0)),
            Seq(new CavityRow("argon", 16.0, 0.90, "air", 0.0, 0.0, 0.0)), 0, Some(new MuntinRow("true-divided", 1, 2, 25.0, 20.0))));

    static Fin<Pane> PaneOf(PaneRow r, Op key) =>
        from t in key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
        from glass in GlassType.TryGet(r.Glass, out GlassType? g) ? Fin.Succ(g!) : Fin.Fail<GlassType>(ComponentFault.Family(key, $"<unknown-glass:{r.Glass}>"))
        from coating in Coating.TryGet(r.Coating, out Coating? c) ? Fin.Succ(c!) : Fin.Fail<Coating>(ComponentFault.Family(key, $"<unknown-coating:{r.Coating}>"))
        from interlayer in Interlayer.TryGet(r.Interlayer, out Interlayer? il) ? Fin.Succ(il!) : Fin.Fail<Interlayer>(ComponentFault.Family(key, $"<unknown-interlayer:{r.Interlayer}>"))
        // A laminate carries a positive interlayer thickness below the total pane thickness; a monolithic pane carries none —
        // the interlayer-type/thickness relation total so a "pvb" interlayer with zero thickness or a thickness with no
        // interlayer rails the family band rather than seeding an inconsistent pane.
        from _ in (interlayer == Interlayer.None) == (r.InterlayerThicknessMm <= 0.0) && r.InterlayerThicknessMm < r.ThicknessMm
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComponentFault.Family(key, $"<glazing-interlayer-inconsistent:{r.Interlayer}:{r.InterlayerThicknessMm:R}>"))
        select new Pane(glass, t, coating, r.CoatedSurface, interlayer, r.InterlayerThicknessMm);

    static Fin<Cavity> CavityOf(CavityRow r, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
        from fill in r.Gas == "vacuum" ? VacuumOf(r, key) : GasOf(r, key)
        select new Cavity(fill, w);

    static Fin<CavityFill> GasOf(CavityRow r, Op key) =>
        r.FillFraction is <= 0.0 or > 1.0
            ? ComponentFault.Family(key, $"<glazing-fill-fraction-out-of-range:{r.FillFraction:R}>")
            : from gas in CavityGas.TryGet(r.Gas, out CavityGas? g) ? Fin.Succ(g!) : Fin.Fail<CavityGas>(ComponentFault.Family(key, $"<unknown-cavity-gas:{r.Gas}>"))
              from balance in CavityGas.TryGet(r.BalanceGas, out CavityGas? b) ? Fin.Succ(b!) : Fin.Fail<CavityGas>(ComponentFault.Family(key, $"<unknown-balance-gas:{r.BalanceGas}>"))
              select (CavityFill)new CavityFill.GasFill(gas, r.FillFraction, balance);

    static Fin<CavityFill> VacuumOf(CavityRow r, Op key) =>
        r.VacuumPressurePa is <= 0.0 or > VacuumIntegrityThresholdPa
            ? ComponentFault.Family(key, $"<glazing-vacuum-pressure-out-of-range:{r.VacuumPressurePa:R}>")
            : from pr in key.AcceptValidated<PositiveMagnitude>(candidate: r.PillarRadiusMm)
              from pp in key.AcceptValidated<PositiveMagnitude>(candidate: r.PillarPitchMm)
              select (CavityFill)new CavityFill.VacuumFill(r.VacuumPressurePa, pr, pp);

    static Fin<MuntinGrid> MuntinOf(MuntinRow r, Op key) =>
        from style in MuntinStyle.TryGet(r.Style, out MuntinStyle? st) ? Fin.Succ(st!) : Fin.Fail<MuntinStyle>(ComponentFault.Family(key, $"<unknown-muntin-style:{r.Style}>"))
        from bw in key.AcceptValidated<PositiveMagnitude>(candidate: r.BarWidthMm)
        from bd in key.AcceptValidated<PositiveMagnitude>(candidate: r.BarDepthMm)
        select new MuntinGrid(style, r.HorizontalBars, r.VerticalBars, bw, bd);

    // The per-row Traverse is the assembly#MATERIAL_COMPOSITION CompositionAuthor idiom — Seq<A>.Traverse over a Fin-
    // returning arm yields Fin<Seq<A>> directly (the Fin applicative), so a single malformed pane/cavity short-circuits the
    // whole row to its ComponentFault and BuildGlazingRows drops it through Choose rather than seeding a partial; the muntin
    // Option lifts through Match so an absent grid is a clean None and a malformed one rails.
    static Fin<GlazingShape> GlazingOf(GlazingRow r, Context context, Op key) =>
        from panes in r.Panes.Traverse(p => PaneOf(p, key))
        from cavities in r.Cavities.Traverse(c => CavityOf(c, key))
        from spacer in SpacerType.TryGet(r.Spacer, out SpacerType? s) ? Fin.Succ(s!) : Fin.Fail<SpacerType>(ComponentFault.Family(key, $"<unknown-spacer:{r.Spacer}>"))
        from muntin in r.Muntin.Match(Some: m => MuntinOf(m, key).Map(g => Some(g)), None: () => Fin.Succ(Option<MuntinGrid>.None))
        from section in GlazingSection.Of(panes, cavities, spacer, StandardEdgeSeal, NominalVisionWidthMm, NominalVisionHeightMm, muntin, r.FireResistanceEiMinutes, key)
        select new GlazingShape(ComponentId.Of(r.Designation), section, IguStandard);

    // The registered ComponentId -> Component rows the parent ComponentCatalogue.Build folds through the ONE canonical
    // Component(Family, Designation, Section, Coring, Standard, CapacityKey, AppearanceId) shape. A glazing Component carries
    // the ComponentSection.Glazing arm (the cross-section FIELD) and no ComputedSection (the IGU is an IfcMaterialLayerSet),
    // so glazing contributes NO ComponentCatalogue.Sections entry and the M7 ProfileResolution dereferences a glazing
    // ComponentId to (Component, None). Coring is None (an IGU is no void-class unit); the CapacityKey and AppearanceId both
    // resolve to the outboard pane's glass row (glazing carries its engineering receipt on the material's own
    // ToProperties set, so the capacity slot coincides with the appearance row rather than a separate Mechanical key).
    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO explicit
    // comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a type mismatch
    // on a ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildGlazingRows(Context context) =>
        GlazingRows
            .Choose(row => GlazingOf(row, context, default).ToOption())
            .ToFrozenDictionary(
                static shape => shape.Id,
                static shape => new Component(
                    ComponentFamily.Glazing, shape.Id, new ComponentSection.Glazing(shape.Section), Coring.None,
                    shape.Standard, CapacityKey: shape.Section.Panes[0].Glass.Appearance, AppearanceId: shape.Section.Panes[0].Glass.Appearance));
}

public readonly record struct PaneRow(string Glass, double ThicknessMm, string Coating, int CoatedSurface, string Interlayer, double InterlayerThicknessMm);
public readonly record struct CavityRow(string Gas, double WidthMm, double FillFraction, string BalanceGas, double VacuumPressurePa, double PillarRadiusMm, double PillarPitchMm);
public readonly record struct MuntinRow(string Style, int HorizontalBars, int VerticalBars, double BarWidthMm, double BarDepthMm);
public readonly record struct GlazingRow(string Designation, string Spacer, Seq<PaneRow> Panes, Seq<CavityRow> Cavities, int FireResistanceEiMinutes, Option<MuntinRow> Muntin);

public sealed record GlazingShape(ComponentId Id, GlazingSection Section, ComponentStandard Standard);
```

## [03]-[RESEARCH]

- [GLAZING_BUILD_TRANSCRIPTION]: REALIZED — the EN 1279 IGU builds carry the per-pane glass substance / thickness / coating / interlayer and the per-cavity fill / width / mixture as full sub-tables, so the double `4-16-4`, the asymmetric `6-16-4`, the inboard-coated `6-16-6-lowe`, the laminated `66.4-16-4` (two glass plus a two-ply PVB interlayer), the triple `4-16-4-16-4`, the vacuum `4-vac-4`, the fire-rated `EI30`, and the gridded units are each one `GlazingRow` with its `PaneRow`/`CavityRow` sub-rows — never the uniform single-pane-thickness placeholder a `lowe`/`lam`/`66.4` suffix leaves unmodeled. The build is DERIVED from the pane count (`GlazingBuild.OfPaneCount`), the panes = cavities + 1 topology gated at `GlazingSection.Of`; the face dimensions REPLACE the prior hardcoded `1200 mm` frame-rebate placeholder, captured on the section (the per-m² Ug/g/τv size-independent, the face feeding the geometry and the perimeter the spacer Ψg multiplies). The remaining pane thicknesses, coating tiers, interlayer chemistries, and electrochromic variants are one further `GlazingRow`/`GlassType`/`Coating`/`Interlayer` data addition, never a new type.
- [EN_673_CENTER_OF_GLASS]: REALIZED — `GlazingThermal.Evaluate` builds ONE ordered series-resistance chain (the external/internal surface films `he = 23`, `hi = 8 W·m⁻²·K⁻¹`, each pane's conductive resistance over the glass-plus-interlayer thickness, each cavity's total conductance) the `Ug = 1/ΣR` reads. The cavity conductance dispatches on the `CavityFill` arm: a gas cavity sums the convective (Nusselt `Nu = max(1, 0.035·Ra^0.38)` over the Rayleigh number the VOLUME-MIXED gas density/viscosity/specific-heat — the EN 673 §B.2 fill/balance mixture — and the gap width drive) and the radiative (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹`); a vacuum cavity sums the Collins pillar conduction (`2·λ_glass·a/p²`), the free-molecular residual-gas conduction (∝ the ISO 19916 residual pressure), and the same radiative term with no convection. The gas properties are the EN 673 Annex-B values at 283 K (air λ 0.0250 / μ 1.761e-5, argon λ 0.0173 / μ 2.164e-5, krypton, xenon). The radiative term is the load-bearing physics: a low-E `0.04`/`0.02` corrected emissivity collapses the cavity radiation the uncoated `0.837` dominates, the `Coating.None` `Option<double>` falling back to the glass `NormalEmissivity` (replacing the `double.NaN` sentinel) so `Pane.EmissivityOf` is the ONE emissivity source.
- [EN_410_SOLAR_OPTICAL]: REALIZED — the net solar factor `g = τe + qi` and the visible transmittance `τv` are the multi-layer EN 410 / ISO 9050 center-of-glass projection over the per-pane directional `(τ, ρ_front, ρ_back)` optics (a coated surface's reflectance asymmetric front-to-back): the panes combine left-to-right through the two-flux `Combine` recursion for the total `τe`/`τv`, the per-pane absorptance `αe,i` follows from the outer/inner sub-stack inter-reflection, and the secondary internal heat flux `qi = Σ αe,i·R_out,i/R_tot` partitions each pane's absorbed energy by the SAME resistance chain the `Ug` reads (the inward fraction being the resistance to the exterior over the total). This CLOSES the prior deferred growth: the inter-reflection uplift and the absorbed-heat secondary gain the prior product-of-transmittances dropped are restored, so a clear double reads a real `g` below the single-pane value and a solar-control coat collapses it further. `g`/`τv` are dimensionless `Ratio` `MeasureValue`s. The full per-wavelength `τ(λ)`/`ρ(λ)` angular spectral ray-trace is a `GlassType`/`Coating` spectral-curve column growth the broadband recursion is the center-of-glass simplification of, never a parallel optical owner.
- [GLAZING_ACOUSTIC_RW]: REALIZED — the IGU's airborne `Acoustic` spectrum is `GlazingThermal.MassLawSpectrum`: the field-incidence mass law `R(f) = 20·log₁₀(m'·f) − 47 dB` over the total areal mass (glass plus interlayer density × thickness) at the seventeen `AcousticBand` centres, with the best interlayer's typed coincidence-dip damping bonus (PVB 3, EVA 2.5, SGP 2 dB — the `Interlayer.AcousticDampingDb` replacing the prior bare laminated flag) and the asymmetric-pane bonus, the `Rw` the seam `RatingContour.Rw.Fit` so the IGU rating and a `Rasm.Compute` assembly rating share ONE contour fit. The spectral coincidence-frequency correction and the per-interlayer damping-loss-factor are an `Acoustic`-spectrum refinement `Rasm.Compute` carries, never a parallel acoustic owner.
- [MATERIAL_PROPERTY_LOWERING]: REALIZED — `GlazingSection.ToProperties` lowers the receipt into the seam set so the IGU material "has it all": the `Thermal` case the EN 673 Ug plus the mass-weighted glass conductivity AND specific heat (the EN 572-1 soda-lime `720 J·kg⁻¹·K⁻¹`, borosilicate `830`, mass-weighted — REPLACING the non-EN `840` hardcode) and the vapour-tight μ, the `Acoustic` the banded spectrum, the `Environmental` the substance/process-split per-m² GWP, and the `Fire` case the PARAMETERIZED EN 13501-2 `FireResistance.Ei(FireResistanceEiMinutes)` (the section's captured EI class, replacing the hardcoded `Ei(30)`) where the section carries positive EI minutes. The `Projection/component#COMPONENT_PROJECTOR` reads the material's own set (never an element Pset).
- [GWP_SUBSTANCE_PROCESS_SPLIT]: REALIZED — `GlazingGwp.StagesPerM2` splits the A1-A3 cradle-to-gate into RAW SUBSTANCE (each pane's and interlayer's mass times its per-kg base — soda-lime `1.43`, low-iron `1.50`, borosilicate `2.00` kgCO2e/kg, the ONLY substance variants) and SECONDARY PROCESSING (the per-m² thermal-form, coating-sputter, lamination, and IGU-assembly adders) so the per-kg base is never double-counted with the secondary process carbon the prior flat `1.20/kg` conflated; A4-D scale the A1-A3 over the EN 15978 `LifecycleStage` banding, a negative D the recovery benefit, `recycledContent 0.25` and `endOfLifeRecovery 0.90` the EN 15804 generic-IGU rows.
- [VIG_VACUUM_ISO_19916]: REALIZED — a vacuum insulating unit is a `CavityFill.VacuumFill` arm carrying the ISO 19916 residual pressure (gated below the `0.1 Pa` functional-vacuum ceiling at `VacuumOf`) and the support-pillar radius/pitch; the cavity conductance is the Collins pillar conduction (`2·λ_glass·a/p²` over the two bounding panes' mean conductivity) plus the free-molecular residual-gas conduction plus the radiative term the low-E coating suppresses, with no convective Nusselt term — so the `4lowe-vac-4` unit computes the order-of-magnitude lower `Ug` a 0.3 mm vacuum gap delivers that no gas fill of the same gap can.
- [EDGE_SEAL_AND_GRID]: REALIZED — the EN 1279-2 `EdgeSeal` (the PIB primary moisture sealant, the structural/durability secondary seal, the molecular-sieve desiccant, the keyed-corner construction) and the enriched `SpacerType` (the Ψg thermal bridge plus the sight-line width, the spacer-frame conductivity, and the edge-seal fabrication GWP — replacing the prior Ψ-only scalar) carry the edge-seal geometry the prior model dropped; the optional `MuntinGrid` (the true-divided / simulated-divided / between-glass style, the bar counts, and the manufacturer bar width/depth — NO EN/ASTM table grounds the bar dimensions) is FACE geometry the generator places across the pane, never a through-thickness `MaterialLayer`.
- [IFCMATERIALLAYERSET_GLAZING_ALIGNMENT]: REALIZED — glazing is the one family that is an `IfcMaterialLayerSet` rather than an `IfcProfileDef` profile, so `ComputedSection` is `None` and `GlazingSection.ToLayerSet` resolves the IGU to the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` (pane / cavity / pane), the laminated pane a glass-interlayer-glass sub-stack within the pane thickness, the vacuum cavity an `IsVentilated`-false sealed gap. The `SpacerType.PsiWmK` edge-seal is the `Rasm.Compute` `AssemblyAggregator` whole-window `Uw` input read off the receipt directly, the low-E `Coating` an `IfcSurfaceStyle` riding the coated pane's `Node.Appearance`, never a fabricated `glass.lowe` bulk row.
- [WHOLE_WINDOW_U_IS_COMPUTE]: `GlazingPerformance.UgCenterOfGlass` is the EN 673 CENTER-OF-GLASS U-value glazing OWNS and `ToProperties` lowers onto the seam `Thermal.UValue`. The EN ISO 10077-1 WHOLE-WINDOW `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` that incorporates the `SpacerType.PsiWmK` perimeter linear thermal bridge AND the frame fraction is a `Rasm.Compute` ASSEMBLY concern: the `Analysis/aggregator` `AssemblyAggregator.AggregateWindow` fold reads the IGU `Ug` off the seam `Thermal.UValue`, the spacer `Ψg` off the glazing receipt, and the frame `Uf` + areas off the window's parts and `Qto_*BaseQuantities`, computing `Uw`. Glazing OWNS the `Ug` + the `Ψg` datum + the layer-set/Pset egress; Compute OWNS the area-and-perimeter combination. Ripple counterpart: `Rasm.Compute/Analysis/aggregator` (the `AggregateWindow` whole-window owner) and `Rasm.Compute/Analysis/physics` (the thermal runner's window branch).
