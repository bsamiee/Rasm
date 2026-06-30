# [MATERIALS_GLAZING]

THE GLAZING PROFILEFAMILY GROUNDED IN THE INSULATING-GLASS BUILD PHYSICS. The glazing cross-section vocabulary — the per-`Pane` glass type / thickness / low-E or solar-control `Coating`, the per-`Cavity` EN 673 gas fill and gap width, the warm/cold-edge `SpacerType`, and the IGU build the pane count derives — is the fifth realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Glazing` case. A double-glazed unit is a `Profile` row, never a `DoubleGlazedUnit` type: the pane stack, the cavity stack, the spacer, and the regional standard are glazing-`Profile` columns, the asymmetric `6-16-4` and the coated `6-16-6-lowe` and the laminated `66.2-16-4` builds all one `GlazingRow` with a per-pane sub-table — never the uniform single-pane-thickness placeholder a designation-string `lowe`/`lam` suffix could not honor. A glazing unit owns its DEFINING engineering performance the way `steel#STEEL_FAMILY` owns `ComputedSection`/`DesignCapacity` and `timber#TIMBER_FAMILY` owns the `Fmk`/`E0Mean` grade receipt: the `GlazingThermal` EN 673 center-of-glass U-value (the conductive + convective-Nusselt + emissivity-driven radiative cavity exchange the low-E coating exists to suppress), the EN 410 solar factor `g` (SHGC) and visible transmittance `τv`, and the mass-law `Acoustic` spectrum its `Rw` derives from — all COMPUTED from the pane/coating/cavity vocabulary, never a stored scalar that drifts from the build. The `GlazingSection.ToProperties` projection lowers that receipt into the `Composition/material#MATERIAL_PROPERTY` seam `MaterialPropertySet` set (`Thermal`/`Acoustic`/`Environmental`/`Fire`) so a `Rasm.Compute` energy route, a `Rasm.Bim` `Pset_*ThermalTransmittance` emitter, and a façade designer read the IGU's U/g/τv/Rw off the seam material; the `GlazingSection.ToLayerSet` projection feeds the same `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` the IGU stack resolves to — a glazing unit is the IFC `IfcMaterialLayerSet` the layer-set assignment owner already models (pane / cavity / pane, the laminated pane its own glass-interlayer-glass sub-stack), never a per-family layout. The vocabulary grows by data — a new IGU is one `GlazingRow` with its pane/cavity sub-rows, a new glass a `GlassType` case, a new coating a `Coating` case, a new gas a `CavityGas` case — never a per-unit type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape, `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet`/`MaterialLayer` for the pane-cavity buildup, the `Composition/material#MATERIAL_PROPERTY` seam `MaterialPropertySet` for the engineering receipt, `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic`/`RatingContour` for the airborne spectrum and `Rw`, the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue`/`QuantityType`/`Dimension` for every measured column, and the `Rasm` kernel `PositiveMagnitude` for every length column; cmu/timber land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[GLAZING_FAMILY]: the `GlassType` glass vocabulary (normal emissivity / conductivity / base solar+light transmittance / safety+laminated class), the `Coating` low-E / solar-control axis (corrected emissivity + solar/light multipliers), the `CavityGas` EN 673 fill axis (conductivity / density / viscosity / specific heat), the `SpacerType` warm/cold-edge axis, the `GlazingBuild` pane-count-derived double/triple classification, the `Pane`/`Cavity` rows and the `GlazingSection` pane-stack record, the `GlazingThermal` EN 673 center-of-glass kernel + EN 410 solar/light projection + mass-law acoustic spectrum, the `GlazingPerformance` (`UgCenterOfGlass`/`SolarFactorG`/`LightTransmittanceTv`/`Acoustic`) receipt, the `GlazingSection.ToProperties` seam `MaterialPropertySet` lowering, the `GlazingSection.ToUnit` profile projection, the `GlazingSection.ToLayerSet` IGU layer-set bridge, and the `ProfileCatalogue.BuildGlazingRows` row table.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing unit vocabulary (`GlassType` the glass-product axis carrying its thermal-radiative identity, `Coating` the low-E/solar-control axis, `CavityGas` the EN 673 fill, `SpacerType` the warm/cold-edge spacer, `GlazingBuild` the pane-count-derived classification, `Pane`/`Cavity` the stack rows, `GlazingSection` the pane-stack record); `GlazingThermal` the EN 673 center-of-glass U-value kernel and the EN 410 solar/light projection; `GlazingPerformance` the computed IGU receipt; `ProfileCatalogue.BuildGlazingRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `GlazingSection.ToProperties`/`ToUnit`/`ToLayerSet` projections.
- Cases: glass {float, low-iron, tempered, heat-strengthened, laminated, fire-rated} — each `GlassType` carrying its uncoated normal emissivity, its conductivity, its safety/laminated class, and its `IfcWindowTypePartitioningEnum` hint; coating {none, hard-coat-lowE (pyrolytic), soft-coat-lowE-double-silver, soft-coat-lowE-triple-silver, solar-control} — each `Coating` carrying the corrected normal emissivity `εn` the radiative cavity term reads plus the EN 410 solar / light transmittance multipliers; gas {air, argon, krypton, xenon} — each `CavityGas` carrying the four EN 673 Annex-B gas properties the cavity conductance reads; spacer {warm-edge, cold-edge-aluminum} — each `SpacerType` carrying its linear thermal-bridge `Ψ`; build {double (panes-cavity-pane), triple (pane-cavity-pane-cavity-pane)} — DERIVED from `Panes.Count`, never stored; a section is a `GlazingSection` over a `Seq<Pane>`/`Seq<Cavity>`/`SpacerType`, never a section subtype and never a uniform single-pane-thickness placeholder.
- Entry: `public Fin<GlazingSection> GlazingSection.Of(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, Op key)` admits a build once — panes = cavities + 1, at least one pane, the pane count one the `GlazingBuild` vocabulary names (so the DERIVED `Build` read is total, never a silent `Double` mislabel of an unmodeled count), every gap width positive — `Fin<T>` railing the profile-family `ProfileFault.Family` on a malformed stack or an out-of-vocabulary pane count; `public Fin<GlazingPerformance> Performance(Op key)` computes the EN 673 `UgCenterOfGlass`, the EN 410 `SolarFactorG`/`LightTransmittanceTv`, and the mass-law `Acoustic` spectrum the `Rw` derives from (`Fin` because the spectrum admits through the seam's public `Acoustic.Of` band gate); `public Fin<Seq<MaterialPropertySet>> ToProperties(Op key)` lowers that receipt into the seam `MaterialPropertySet` set (`Thermal` carrying the Ug, `Acoustic` the banded spectrum, `Environmental` the glass GWP, `Fire` the fire-rated class) the `Projection/material#MATERIAL_PROJECTOR` reads; `public Fin<ProfileUnit> ToUnit(Context context, Op key)` projects the section to the canonical `ProfileUnit` (`WidthMm` = the overall unit thickness, the frame-rebate module the host overrides on the other axes); `public Fin<MaterialComposition> ToLayerSet(Op key)` the IGU pane-cavity-pane `LayerSet` bridge (the laminated pane its own glass-interlayer-glass sub-stack); `public double OverallThicknessMm()` the unit-build depth the frame seam reads.
- Packages: Rasm (project — `PositiveMagnitude` for the pane/gap columns), Rasm.Element (project — `MaterialId`/`MaterialComposition`/`MaterialPropertySet`/`MeasureValue`/`QuantityType`/`Dimension`/`Acoustic`/`RatingContour`/`FireRating`/`FireResistance`/`MeasurementBasis` the section lowers into), VividOrange via the shared owners is NOT composed here (glazing carries no `IfcProfileDef` section — it is an `IfcMaterialLayerSet`), Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet (via the seam `MeasureValue`), BCL inbox (`FrozenDictionary`/`Math`).
- Growth: the glazing vocabulary grows by data — a new IGU is one `GlazingRow` with its `PaneRow`/`CavityRow` sub-rows keyed by its build designation, a new glass one `GlassType` case carrying its emissivity/conductivity/class, a new coating one `Coating` case carrying its corrected `εn` + solar/light multipliers, a new gas one `CavityGas` case carrying its EN 673 Annex-B properties, a new spacer one `SpacerType` row, a quad-pane build one more `GlazingBuild` case the derived `Build` read maps — never a per-unit type, never a per-family `Profile` variant; an electrochromic/vacuum (VIG) variant is a `GlassType` case plus a `Coating` row, never a parallel section owner. A cmu/timber family lands its own vocabulary on its own page the way glazing carries `GlassType`/`GlazingSection`.
- Boundary: the glazing vocabulary is a realized `ProfileFamily` whose DOMAIN RECEIPT is the IGU thermal/optical/acoustic performance, not the rectangle a per-unit class could carry — a per-unit class AND a uniform single-pane-thickness `GlazingSection` (which cannot represent the asymmetric `6-16-4`, the coated `6-16-6-lowe`, or the laminated `66.2-16-4` the catalogue names) are the deleted forms; `GlazingSection` carries a `Seq<Pane>` (each `GlassType` + `PositiveMagnitude ThicknessMm` + `Coating` + the coated-surface index + an optional laminated interlayer) and a `Seq<Cavity>` (each `CavityGas` + `PositiveMagnitude WidthMm`) so the per-pane glass type, the per-surface low-E coating, the per-cavity gas, and the asymmetric thicknesses are first-class, every column the kernel `PositiveMagnitude` (double-backed `> 0` finite) so a fractional-millimetre 4 mm/6 mm pane and a 12/16/20 mm gap admit without truncation; `GlazingThermal.UgCenterOfGlass` is the EN 673 series-resistance chain — the external/internal surface films (`he = 23`, `hi = 8 W·m⁻²·K⁻¹`), each pane's conductive resistance (`t / λ_glass`), and each cavity's TOTAL conductance the conductive (`λ_gas / s`) PLUS the convective (the EN 673 Nusselt number over the Rayleigh number the gas density/viscosity/specific-heat and the gap width drive) PLUS the RADIATIVE term (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹` over the two facing pane corrected emissivities) — so the low-E `Coating.NormalEmissivity` of `0.04`/`0.02` collapses the radiative exchange the uncoated `0.837` dominates and the coated rows compute a real Ug the uncoated rows cannot, the radiative term the physics the prior bare `λ/gap` conductance silently dropped; `GlazingThermal` reads the EN 410 `Coating` solar/light multipliers over the base glass transmittance for the `SolarFactorG` (SHGC) and the `LightTransmittanceTv`, each a dimensionless `Ratio` `MeasureValue` (`RatioUnit.DecimalFraction`, the seam `IsDimensionless` path); the `Acoustic` spectrum is the field-incidence mass-law `R(f) = 20·log₁₀(m'·f) − 47 dB` over the total areal mass `m' = Σ(ρ_glass · t_pane)` evaluated at the seventeen `Composition/acoustic#ACOUSTIC_FOLDS` `AcousticBand` centres with the laminated-interlayer and asymmetric-pane corrections, the `Rw` the seam `RatingContour.Rw.Fit` over that vector so the IGU rating and the assembly rating share ONE contour fit; `GlazingSection.ToProperties` lowers the receipt into the seam `MaterialPropertySet` set so the IGU material "has it all" (`Thermal` the Ug, `Acoustic` the spectrum, `Environmental` the per-m² glass GWP, `Fire` the fire-rated class) — never an element Pset (the `Projection/material#MATERIAL_PROJECTOR` reads the material's own property set); `GlazingSection.ToLayerSet` is the ONE bridge to the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` — a double-glazed unit resolves to the alternating glass-cavity-glass set, a triple the five-layer set, a laminated pane its own glass-PVB-glass sub-stack, each `MaterialLayer` carrying its `Dimension` thickness and the `Appearance/graph#MATERIAL_LIBRARY` clear-glass `MaterialId` (`glass.crown` the panes and the PVB interlayer, `glass.flint` a fire-rated pane, `gas.cavity` the transparent sealed-cavity appearance row the library carries for the air/argon/krypton/xenon fills) — the per-pane glass type, the low-E `Coating`, and the gas species held in the layer label and the typed `GlassType`/`Coating`/`CavityGas` thermal-optical identity, NOT a bulk appearance row (a low-E coating is a thin-film surface effect whose optical signal crosses the seam as the coated pane's content-keyed `Node.Appearance` the `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` `Author` emits as an `IfcSurfaceStyle` — never Bim reading the Materials-internal `Coating` type, never a fabricated `glass.lowe` bulk material the library does not carry) — so the IGU shares the IFC `IfcMaterialLayerSet` the layer-set assignment owner already models, the `SpacerType.PsiWmK` edge-seal thermal-bridge datum the `Rasm.Compute` `AssemblyAggregator` reads off the glazing receipt directly (the app-platform downward strata edge) rather than a Bim IFC egress row or a profile column; `ProfileCatalogue.BuildGlazingRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the rows keyed `glazing.<designation>`, the realized cross-section grounded in the EN 1279 IGU build values and the EN 673 / EN 410 published gas and coating properties.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Context, Op
using Rasm.Element;                  // MaterialId, MaterialComposition, MaterialPropertySet, MeasureValue, QuantityType,
                                     // Dimension, MeasurementBasis, LifecycleStage, Acoustic, RatingContour, AcousticBand, FireRating, FireResistance
using Rasm.Materials.Construction;   // CompositionAuthor (the seam-MaterialComposition author the ToLayerSet bridge calls)
using Rasm.Materials.Profiles;       // Profile/ProfileUnit/ProfileStandard/ProfileId/ProfileFault/ProfileFamily (the parent PROFILE_OWNER)
using Rasm.Materials.Profiles.Masonry;   // Coring (the masonry#PROFILE_FAMILY void-class this glazing family passes Coring.None)
using Thinktecture;
using static LanguageExt.Prelude;

// Each family page is its OWN Rasm.Materials.Profiles.<Family> sub-namespace so the six sibling `ProfileCatalogue` static
// classes are distinct types (one shared namespace would be a CS0101 collision); profile#PROFILE_OWNER stays the parent
// Rasm.Materials.Profiles and folds Glazing.ProfileCatalogue.BuildGlazingRows by the sub-namespace-qualified name. The
// parent owner types and the masonry Coring are composed via the usings above.
namespace Rasm.Materials.Profiles.Glazing;

// --- [TYPES] -------------------------------------------------------------------------------
// The glass-product axis: each row carries the uncoated NORMAL EMISSIVITY (soda-lime float radiates at εn 0.837, the
// EN 673 Annex-A baseline the cavity radiative term reads when no coating overrides it), the glass CONDUCTIVITY
// (λ 1.0 W·m⁻¹·K⁻¹ soda-lime), the SAFETY/LAMINATED class the IGU build reads, and the IFC window-partitioning hint.
// A low-iron pane differs in solar/light transmittance not thermal identity; a laminated pane carries an interlayer
// the build sub-stacks; a new glass product (electrochromic, vacuum) is one row, never a parallel pane owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassType {
    public static readonly GlassType Float            = new("float",             normalEmissivity: 0.837, conductivityWmK: 1.0,  baseSolarTransmittance: 0.82, baseLightTransmittance: 0.90, laminated: false, safety: false);
    public static readonly GlassType LowIron          = new("low-iron",          normalEmissivity: 0.837, conductivityWmK: 1.0,  baseSolarTransmittance: 0.90, baseLightTransmittance: 0.91, laminated: false, safety: false);
    public static readonly GlassType Tempered         = new("tempered",          normalEmissivity: 0.837, conductivityWmK: 1.0,  baseSolarTransmittance: 0.82, baseLightTransmittance: 0.90, laminated: false, safety: true);
    public static readonly GlassType HeatStrengthened = new("heat-strengthened", normalEmissivity: 0.837, conductivityWmK: 1.0,  baseSolarTransmittance: 0.82, baseLightTransmittance: 0.90, laminated: false, safety: false);
    public static readonly GlassType Laminated        = new("laminated",         normalEmissivity: 0.837, conductivityWmK: 1.0,  baseSolarTransmittance: 0.78, baseLightTransmittance: 0.88, laminated: true,  safety: true);
    public static readonly GlassType FireRated        = new("fire-rated",        normalEmissivity: 0.837, conductivityWmK: 0.96, baseSolarTransmittance: 0.70, baseLightTransmittance: 0.85, laminated: true,  safety: true);
    public double NormalEmissivity { get; }
    public double ConductivityWmK { get; }
    public double BaseSolarTransmittance { get; }
    public double BaseLightTransmittance { get; }
    public bool Laminated { get; }
    public bool Safety { get; }

    // The library appearance row the LayerSet bridge tags each pane with — the Appearance/graph#MATERIAL_LIBRARY clear
    // glass row (`glass.crown`, IOR 1.52, transmission 1.0; the heavier-flint optic for fire-rated). A low-E or
    // solar-control COATING is NOT a bulk appearance material — it is a thin-film surface effect: the COATED pane's
    // optical signal crosses the seam as the content-keyed Node.Appearance the Appearance/interchange#MATERIAL_WIRE owner
    // mints (the coating's solar/light multipliers fold into that material row's appearance), and Rasm.Bim's
    // Semantics/appearance#APPEARANCE_PROJECTION Author emits the IfcSurfaceStyle from THAT seam Node.Appearance — NEVER
    // by Bim reading the Materials-internal Coating type (which does not cross the seam) and never a fabricated
    // `glass.lowe` bulk row the appearance library does not carry (the coating drives the thermal/optical identity, not
    // the bulk shade, so this read takes no Coating knob).
    public MaterialId Appearance => this == FireRated ? MaterialId.Of("glass.flint") : MaterialId.Of("glass.crown");
}

// The low-E / solar-control coating axis: each row carries the CORRECTED NORMAL EMISSIVITY εn the EN 673 cavity
// radiative term reads (a hard-coat pyrolytic ε 0.16, a soft-coat double-silver ε 0.04, a triple-silver ε 0.02 vs the
// uncoated 0.837 — the order-of-magnitude drop the low-E coating exists to deliver) plus the EN 410 solar-factor and
// light-transmittance multipliers a coated surface applies over the base glass transmittance. None is the uncoated
// surface (the glass NormalEmissivity stands); a new coating tier is one row, never a parallel coated-pane owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coating {
    public static readonly Coating None            = new("none",              correctedEmissivity: double.NaN, solarMultiplier: 1.00, lightMultiplier: 1.00);
    public static readonly Coating HardCoatLowE     = new("hard-coat-lowe",     correctedEmissivity: 0.16,        solarMultiplier: 0.80, lightMultiplier: 0.95);
    public static readonly Coating SoftCoatDouble    = new("soft-coat-double",    correctedEmissivity: 0.04,        solarMultiplier: 0.55, lightMultiplier: 0.90);
    public static readonly Coating SoftCoatTriple    = new("soft-coat-triple",    correctedEmissivity: 0.02,        solarMultiplier: 0.40, lightMultiplier: 0.82);
    public static readonly Coating SolarControl      = new("solar-control",       correctedEmissivity: 0.04,        solarMultiplier: 0.30, lightMultiplier: 0.55);
    public double CorrectedEmissivity { get; }   // NaN for None — EmissivityOf falls back to the glass NormalEmissivity
    public double SolarMultiplier { get; }
    public double LightMultiplier { get; }
}

// The cavity fill: a closed gas vocabulary carrying the four EN 673 Annex-B properties the cavity conductance reads —
// the thermal CONDUCTIVITY λ (W·m⁻¹·K⁻¹), the DENSITY ρ (kg·m⁻³ at 283 K), the dynamic VISCOSITY μ (Pa·s), and the
// SPECIFIC HEAT c (J·kg⁻¹·K⁻¹) — so the convective Nusselt/Rayleigh term reads a typed gas receipt, never a bare λ a
// pure-conduction model would force. The heavier gas (krypton/xenon) lowers BOTH conduction and convection; a new
// gas (SF6, a gas mixture) is one row, never a parallel fill model.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityGas {
    public static readonly CavityGas Air     = new("air",     conductivityWmK: 0.0241, densityKgM3: 1.232, viscosityPaS: 1.73e-5, specificHeatJKgK: 1008.0);
    public static readonly CavityGas Argon   = new("argon",   conductivityWmK: 0.0162, densityKgM3: 1.699, viscosityPaS: 2.10e-5, specificHeatJKgK: 519.0);
    public static readonly CavityGas Krypton = new("krypton", conductivityWmK: 0.0090, densityKgM3: 3.560, viscosityPaS: 2.34e-5, specificHeatJKgK: 245.0);
    public static readonly CavityGas Xenon   = new("xenon",   conductivityWmK: 0.0054, densityKgM3: 5.689, viscosityPaS: 2.31e-5, specificHeatJKgK: 161.0);
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double ViscosityPaS { get; }
    public double SpecificHeatJKgK { get; }
}

// The edge-seal spacer axis: each row carries the linear thermal-bridge Ψ (W·m⁻¹·K⁻¹) the EN ISO 10077-1 WHOLE-WINDOW
// U-value adds at the perimeter — a warm-edge spacer Ψ 0.04, a cold-edge aluminium Ψ 0.11 (the conductive edge the
// warm-edge stainless/foam spacer suppresses). This row carries the Ψg DATUM; the whole-window Uw that COMBINES it with
// the EN 673 center-of-glass Ug (UgCenterOfGlass below) and the frame fraction is a Rasm.Compute ASSEMBLY concern (the
// Analysis/aggregator AssemblyAggregator.AggregateWindow fold the thermal runner composes), NOT a glazing-section field —
// glazing OWNS the Ug + the Ψg datum, Compute OWNS their area-and-perimeter combination into Uw. Rasm.Compute (APP-PLATFORM,
// above this AEC-DOMAIN stratum, so the downward dependency on Materials is the allowed strata edge) reads PsiWmK off the
// glazing receipt DIRECTLY, so the spacer Ψg needs no IFC egress carrier and Rasm.Bim authors no thermal-bridge Pset from
// data the seam does not carry (MaterialPropertySet.Thermal holds the Ug center-of-glass column only, no perimeter-bridge
// datum) — the Bim IFC egress emits the IGU Ug as a material Pset off the seam MaterialPropertySet.Thermal that DOES cross,
// the perimeter Ψg staying the Compute assembly-fold input the AssemblyAggregator reads, never a Bim row and never a profile column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpacerType {
    public static readonly SpacerType WarmEdge         = new("warm-edge",          psiWmK: 0.04);
    public static readonly SpacerType ColdEdgeAluminum = new("cold-edge-aluminum", psiWmK: 0.11);
    public double PsiWmK { get; }
}

// The IGU build classification DERIVED from the pane count (two panes → double, three → triple) — the IFC layer-count
// semantics carried as a vocabulary, never a stored field a malformed "double" row with three panes could contradict.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlazingBuild {
    public static readonly GlazingBuild Double = new("double", panes: 2);
    public static readonly GlazingBuild Triple = new("triple", panes: 3);
    public int Panes { get; }

    // The pane-count → build classification (the steel CompactnessClass-style derived verdict): a section's Build is
    // this read over Panes.Count, never a stored token; a quad-pane build is one more row.
    public static Option<GlazingBuild> OfPaneCount(int panes) =>
        Items.FirstOrDefault(b => b.Panes == panes) is { } build ? Some(build) : None;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One pane in the IGU stack: its glass product, its thickness, its surface Coating, and the coated-surface index
// (0 = outboard face, 1 = inboard face — which of the pane's two surfaces carries the low-E coating, EN 673 reading
// the coated surface that faces the cavity). An InterlayerMm > 0 marks a laminated pane (the PVB sub-layer the
// ToLayerSet bridge splits into glass-PVB-glass); zero for a monolithic pane.
public readonly record struct Pane(
    GlassType Glass,
    PositiveMagnitude ThicknessMm,
    Coating Coating,
    int CoatedSurface,
    double InterlayerMm = 0.0) {

    // The EN 673 emissivity of ONE of the pane's two surfaces (0 = outboard, 1 = inboard): the Coating's corrected εn
    // ONLY on the COATED surface (a coating suppresses radiation across the cavity it faces, not the opposite face),
    // else the glass's uncoated NormalEmissivity — so a low-E coating on the wrong surface does not falsely lower the
    // adjacent cavity's radiative exchange, the CoatedSurface index load-bearing rather than a stored-but-unread field.
    public double EmissivityOf(int surface) =>
        surface == CoatedSurface && Coating != Coating.None ? Coating.CorrectedEmissivity : Glass.NormalEmissivity;
}

// One cavity (gap) in the IGU stack: the sealed gas fill and the gap width the EN 673 conduction/convection read.
public readonly record struct Cavity(CavityGas Gas, PositiveMagnitude WidthMm);

// The computed IGU receipt the seam MaterialPropertySet lowering reads — the DEFINING glazing performance, COMPUTED
// from the pane/coating/cavity vocabulary, never a stored scalar: the EN 673 center-of-glass U-value, the EN 410
// solar factor g (SHGC) and visible transmittance τv (dimensionless Ratio measures), and the field-incidence mass-law
// Acoustic spectrum the Rw derives from. The Rw is a derived read off the spectrum through the shared RatingContour fit.
public readonly record struct GlazingPerformance(
    MeasureValue UgCenterOfGlass,
    MeasureValue SolarFactorG,
    MeasureValue LightTransmittanceTv,
    Acoustic Acoustic) {
    public int Rw => Acoustic.Rw;
}

// The pane-stack section: the panes, the cavities (panes = cavities + 1), and the edge-seal spacer; the build is
// DERIVED from the pane count. The asymmetric 6-16-4, the coated 6-16-6-lowe (the inboard pane low-E), and the
// laminated 66.2-16-4 (the outboard pane laminated) are all one GlazingSection — never a uniform single-pane-thickness
// placeholder, never a per-build subtype.
public readonly record struct GlazingSection(Seq<Pane> Panes, Seq<Cavity> Cavities, SpacerType Spacer) {
    // TOTAL by construction: Of admits ONLY a pane count the GlazingBuild vocabulary names, so OfPaneCount is always
    // Some and the IfNone is the defensive-unreachable — never a silent mislabel of an out-of-vocabulary stack as Double.
    public GlazingBuild Build => GlazingBuild.OfPaneCount(Panes.Count).IfNone(GlazingBuild.Double);
    public double OverallThicknessMm() => Panes.Sum(static p => p.ThicknessMm.Value) + Cavities.Sum(static c => c.WidthMm.Value);

    // The admission gate: panes = cavities + 1 (an IGU alternates pane/cavity/pane), AND the pane count is one the
    // GlazingBuild vocabulary names (double=2 / triple=3 today, a quad landing as ONE more GlazingBuild row that this
    // gate then admits automatically) — every gap positive (the kernel PositiveMagnitude gated each thickness/width at
    // the row). A malformed stack arity OR an out-of-vocabulary pane count (a monolithic single pane, an unmodeled quad)
    // rails the profile-family band ProfileFault.Family rather than seeding a unit whose DERIVED Build would silently
    // mislabel — so a section that survives Of has a TOTAL Build read. (The ConstructionFault band stays the
    // Construction/assembly LayerSet author's, surfaced only via ToLayerSet.)
    public static Fin<GlazingSection> Of(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, Op key) =>
        panes.IsEmpty || cavities.Count != panes.Count - 1
            ? Fin.Fail<GlazingSection>(ProfileFault.Family(key, $"<glazing-stack-arity:panes={panes.Count}:cavities={cavities.Count}>"))
            : GlazingBuild.OfPaneCount(panes.Count).IsNone
                ? Fin.Fail<GlazingSection>(ProfileFault.Family(key, $"<glazing-build-unmodeled-pane-count:{panes.Count}>"))
                : Fin.Succ(new GlazingSection(panes, cavities, spacer));

    // The EN 673 center-of-glass U-value + the EN 410 solar/light projection + the mass-law Acoustic spectrum, COMPUTED
    // through the GlazingThermal kernel — the IGU's defining performance the seam material carries (never a stored
    // scalar). Fin because the Acoustic spectrum admits through the seam's PUBLIC Acoustic.Of band gate — its only
    // admission, with no re-hydration bypass — so the band arity/unit rail surfaces here (the properties.md sibling
    // lowers acoustic the same way).
    public Fin<GlazingPerformance> Performance(Op key) => GlazingThermal.Evaluate(this, key);

    // The seam MaterialPropertySet set the IGU material carries (Composition/material#MATERIAL_PROPERTY) — the Projection
    // /material#MATERIAL_PROJECTOR lowers it onto the Material node so a Rasm.Compute energy route and a Rasm.Bim
    // Pset_*ThermalTransmittance emitter read the IGU's U/g/τv/Rw off the seam. The Thermal case carries the Ug as a
    // HeatTransferCoefficient MeasureValue, the Acoustic the banded spectrum (Rw a derived read), the Environmental the
    // per-m² glass GWP over the LayerSet glass mass, the Fire case a fire-rated class where any pane is fire-rated.
    public Fin<Seq<MaterialPropertySet>> ToProperties(Op key) =>
        from perf in Performance(key)
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: WeightedGlassConductivity(),
            specificHeat: 840.0,                                  // soda-lime glass c (J·kg⁻¹·K⁻¹)
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6,                        // glass is a vapour barrier (EN ISO 13788 μ → vapour-tight)
            key)
        from environmental in MaterialPropertySet.OfEnvironmental(
            MeasurementBasis.PerM2, MaterialPropertySet.Environmental.CarbonMatrix(GlazingGwp.StagesPerM2(this)), recycledContent: 0.25, endOfLifeRecovery: 0.90, epd: "EN 15804 generic float glass", validUntilYear: 0, key)
        let acoustic = MaterialPropertySet.OfAcoustic(perf.Acoustic)
        let core = Seq(thermal, acoustic, environmental)
        select Panes.Exists(static p => p.Glass == GlassType.FireRated)
            ? core.Add(MaterialPropertySet.OfFire(FireRating.A1, FireResistance.Ei(30)))
            : core;

    public Fin<ProfileUnit> ToUnit(Context context, Op key) {
        double overall = OverallThicknessMm();
        // WidthMm = the overall unit thickness (the IGU depth the frame rebate reads); the height/length/course carry
        // the standard 1200 mm rebate module the host overrides per occurrence — never the thickness echoed four times.
        return ProfileUnit.Of(overall, FrameRebateModuleMm, FrameRebateModuleMm, FrameRebateModuleMm, context, key);
    }

    // The IGU pane-cavity-pane stack lowers to a seam MaterialComposition.LayerSet via the Construction/assembly
    // CompositionAuthor (the seam owns the composition + the MeasureValue thickness coercion); the alternating panes
    // and cavities tag GlassType/Coating-derived glass.* and gas.cavity material ids the appearance and Rasm.Bim sides
    // read off the seam node, a laminated pane splitting into its glass-PVB-glass sub-layers within the pane thickness.
    public Fin<MaterialComposition> ToLayerSet(Op key) =>
        CompositionAuthor.LayerSet(LayerRows(), key);

    const double FrameRebateModuleMm = 1200.0;

    // The alternating glass / cavity / glass layer rows: each pane its GlassType/Coating appearance id (a laminated
    // pane its glass-PVB-glass sub-stack of three layers), each cavity the gas.cavity id, the layer NAME carrying the
    // glass type + coating + gas as the human-readable IfcMaterialLayer.Name the Bim egress round-trips (the coating's
    // OPTICAL effect rides the coated pane's Node.Appearance, not a Bim parse of this label string — the label is the
    // descriptive layer name, not a Materials-private grammar Bim decodes).
    Seq<(MaterialId Material, double ThicknessMm, string Name)> LayerRows() =>
        toSeq(Enumerable.Range(0, Panes.Count + Cavities.Count))
            .Bind(slot => (slot & 1) == 0
                ? PaneLayers(Panes[slot / 2], slot / 2)
                : Seq1((MaterialId.Of("gas.cavity"), Cavities[slot / 2].WidthMm.Value, $"cavity-{Cavities[slot / 2].Gas.Key}-{slot / 2}")));

    // A monolithic pane is one glass layer; a laminated pane is glass / PVB-interlayer / glass — the InterlayerMm split
    // out of the pane thickness so the IGU's IfcMaterialLayerSet carries the laminate sub-stack the prior model dropped.
    // The interlayer shades as the clear glass row (an optically near-glass transparent PVB — its laminate identity
    // rides the layer NAME the Bim IfcMaterialLayer.Name round-trips, not a fabricated polymer appearance row).
    static Seq<(MaterialId Material, double ThicknessMm, string Name)> PaneLayers(Pane pane, int index) =>
        pane.InterlayerMm > 0.0
            ? Seq(
                (pane.Glass.Appearance, (pane.ThicknessMm.Value - pane.InterlayerMm) / 2.0, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}-outer"),
                (MaterialId.Of("glass.crown"), pane.InterlayerMm,                          $"pane-{index}-pvb-interlayer"),
                (pane.Glass.Appearance, (pane.ThicknessMm.Value - pane.InterlayerMm) / 2.0, $"pane-{index}-{pane.Glass.Key}-inner"))
            : Seq1((pane.Glass.Appearance, pane.ThicknessMm.Value, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}"));

    // The mass-weighted glass conductivity the Thermal case carries as the homogenized IGU conductivity (the seam
    // Thermal column is a single λ; the per-cavity gas resistance is the Ug the kernel already computed, so this λ is
    // the glass-only mass-weighted mean a non-IGU thermal read uses, never the dropped cavity physics).
    double WeightedGlassConductivity() {
        double mass = Panes.Sum(static p => p.ThicknessMm.Value), acc = Panes.Sum(p => p.Glass.ConductivityWmK * p.ThicknessMm.Value);
        return mass > 0.0 ? acc / mass : 1.0;
    }
}

public readonly record struct PaneRow(string Glass, double ThicknessMm, string Coating, int CoatedSurface, double InterlayerMm);
public readonly record struct CavityRow(string Gas, double WidthMm);
public readonly record struct GlazingRow(string Designation, string Spacer, Seq<PaneRow> Panes, Seq<CavityRow> Cavities);

public sealed record GlazingShape(ProfileId Id, GlazingSection Section, ProfileStandard Standard);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 673 center-of-glass U-value kernel + the EN 410 solar/light projection + the mass-law Acoustic spectrum — the
// glazing family's domain-physics owner the way profile#PROFILE_OWNER ParametricSection is the section-property owner.
// The chain is a series-resistance sum the panes and cavities fold; the cavity conductance is the EN 673 conductive +
// convective (Nusselt over Rayleigh) + radiative (emissivity-driven) exchange, never the bare λ/gap the prior model used.
public static class GlazingThermal {
    const double SurfaceExternalWmK = 23.0;        // EN 673 external surface coefficient he (W·m⁻²·K⁻¹)
    const double SurfaceInternalWmK = 8.0;         // EN 673 internal surface coefficient hi
    const double StefanBoltzmann = 5.67e-8;        // σ (W·m⁻²·K⁻⁴)
    const double MeanTemperatureK = 283.0;         // EN 673 mean cavity temperature (10 °C)
    const double TemperatureDeltaK = 15.0;         // EN 673 reference ΔT across the cavity
    const double GravityMs2 = 9.81;
    const double MassLawOffsetDb = 47.0;           // field-incidence mass-law offset R = 20·log₁₀(m'·f) − 47

    public static Fin<GlazingPerformance> Evaluate(GlazingSection section, Op key) {
        double ug = 1.0 / (1.0 / SurfaceExternalWmK + 1.0 / SurfaceInternalWmK + PaneResistance(section) + CavityResistance(section));
        (double g, double tv) = SolarOptical(section);
        return MassLawSpectrum(section, key).Map(acoustic => new GlazingPerformance(UValue(ug), Fraction(g, key), Fraction(tv, key), acoustic));
    }

    // Σ each pane's conductive resistance t/λ — the glass conductive path, small beside the cavity resistance but real
    // (a laminated pane's PVB adds a further sub-resistance the homogenized λ absorbs).
    static double PaneResistance(GlazingSection section) =>
        section.Panes.Sum(static p => (p.ThicknessMm.Value / 1000.0) / p.Glass.ConductivityWmK);

    // Σ each cavity's resistance 1/h_total where h_total = h_conv + h_rad — the EN 673 cavity heat exchange: the
    // convective coefficient is the Nusselt number over the conductive base (Nu ≥ 1; for a thin sealed cavity Nu
    // collapses to 1 and the path is pure conduction λ/s), the radiative coefficient the emissivity-driven exchange the
    // low-E coating suppresses. The bounding panes' emissivities (the cavity's two facing surfaces) drive h_rad.
    static double CavityResistance(GlazingSection section) {
        double total = 0.0;
        for (int i = 0; i < section.Cavities.Count; i++) {
            Cavity cavity = section.Cavities[i];
            double s = cavity.WidthMm.Value / 1000.0;
            double hConv = Nusselt(cavity, s) * cavity.Gas.ConductivityWmK / s;
            // The cavity sees the INBOARD surface of pane i (index 1) and the OUTBOARD surface of pane i+1 (index 0):
            // a low-E coating lowers h_rad only when it sits on one of these two cavity-facing surfaces.
            double hRad = RadiativeCoefficient(section.Panes[i].EmissivityOf(1), section.Panes[i + 1].EmissivityOf(0));
            total += 1.0 / (hConv + hRad);
        }
        return total;
    }

    // EN 673 radiative coefficient h_r = 4·σ·T_m³ / (1/ε₁ + 1/ε₂ − 1): the two facing pane corrected emissivities drive
    // the cavity radiation — uncoated 0.837/0.837 yields the dominant exchange, a single low-E 0.04 surface collapses
    // it by an order of magnitude (the entire reason a coated cavity outperforms an uncoated one of the same gas/gap).
    static double RadiativeCoefficient(double e1, double e2) =>
        4.0 * StefanBoltzmann * MeanTemperatureK * MeanTemperatureK * MeanTemperatureK / (1.0 / e1 + 1.0 / e2 - 1.0);

    // EN 673 Nusselt number Nu = max(1, A·(Ra·cos φ)ⁿ) over the Rayleigh number for a vertical cavity (φ = 90°, A 0.035,
    // n 0.38); Ra = ρ²·s³·g·c·ΔT / (T_m·μ·λ). Below the critical Rayleigh the cavity does not convect (Nu = 1, pure
    // conduction); above it the gas circulates and raises the conductance — the heavier krypton/xenon suppresses both.
    static double Nusselt(Cavity cavity, double s) {
        CavityGas gas = cavity.Gas;
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return Math.Max(1.0, 0.035 * Math.Pow(ra, 0.38));
    }

    // EN 410 solar factor g (SHGC) and visible transmittance τv: the SERIES TRANSMITTANCE of the panes — the product
    // Π(baseTransmittance · coatingMultiplier) over the stack, the first-order multi-layer τ panes in series transmit
    // (each pane passes a fraction, so a second pane genuinely lowers the unit's g/τv, a clear double ~0.75 not the
    // single-pane 0.82 a per-pane mean would falsely return). The product is the conservative EN 410 center-of-glass
    // simplification the data the panes carry admits — slightly UNDER the true g because it omits the inter-reflection
    // uplift (light reflected between panes adds transmittance back) and the inwardly-flowing absorbed-heat secondary
    // gain, both requiring per-pane spectral REFLECTANCE/ABSORPTANCE the Coating/GlassType do not yet carry; the full
    // EN 410 net g (the multi-layer τ/ρ/α recursion with the secondary heat term) is a Coating reflectance-column +
    // GlassType absorptance-column growth, never a parallel optical owner. A single-pane unit IS its base glass · coating.
    static (double G, double Tv) SolarOptical(GlazingSection section) {
        double g = section.Panes.Fold(1.0, static (acc, p) => acc * p.Glass.BaseSolarTransmittance * p.Coating.SolarMultiplier);
        double tv = section.Panes.Fold(1.0, static (acc, p) => acc * p.Glass.BaseLightTransmittance * p.Coating.LightMultiplier);
        return (Math.Clamp(g, 0.0, 1.0), Math.Clamp(tv, 0.0, 1.0));
    }

    // The field-incidence mass-law Acoustic spectrum: R(f) = 20·log₁₀(m'·f) − 47 over the total areal mass
    // m' = Σ(ρ_glass · t_pane) evaluated at the seventeen Composition/acoustic#ACOUSTIC_FOLDS one-third-octave centres,
    // with a laminated-interlayer bonus (PVB damps the coincidence dip, +3 dB) and an asymmetric-pane bonus (unequal
    // panes shift the coincidence dips apart, +2 dB) — the honest closed form the seam carries no decoupling cavity to
    // refine. The absorption spectrum is glass's near-zero (0.03 flat); the Rw is the seam RatingContour.Rw.Fit read.
    // Admits through the seam's PUBLIC Acoustic.Of band gate (the curated vectors are valid by construction — absorption
    // 0.03 ∈ [0,1], every sound-reduction band finite — so Of succeeds).
    static Fin<Acoustic> MassLawSpectrum(GlazingSection section, Op key) {
        const double GlassDensityKgM3 = 2500.0;
        double areal = section.Panes.Sum(static p => GlassDensityKgM3 * p.ThicknessMm.Value / 1000.0);
        double bonus = (section.Panes.Exists(static p => p.InterlayerMm > 0.0) ? 3.0 : 0.0)
                     + (Asymmetric(section) ? 2.0 : 0.0);
        double[] sri = new double[AcousticBand.Count];
        double[] absorption = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            sri[band.Index] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(areal, 1e-9) * band.CenterHz) - MassLawOffsetDb + bonus);
            absorption[band.Index] = 0.03;
        }
        return Acoustic.Of(absorption, sri, key);
    }

    // Asymmetric iff some pane thickness differs from the first — the unequal-pane coincidence-dip shift, computed
    // without a Distinct materialization (a scan over the panes against the first thickness; Seq indexer, not the
    // Option-returning Head).
    static bool Asymmetric(GlazingSection section) =>
        section.Panes.Count >= 2 && section.Panes.Exists(p => p.ThicknessMm.Value != section.Panes[0].ThicknessMm.Value);

    static MeasureValue UValue(double ug) =>
        MeasureValue.OfSi(QuantityType.Create("HeatTransferCoefficient"), Dimension.ThermalTransmittance, ug);

    // The dimensionless g / τv as a Ratio MeasureValue — RatioUnit.DecimalFraction admits through the seam
    // IsDimensionless path (no SI reprojection), carrying the Ratio QuantityType and the zero-vector Dimension. The
    // clamped [0,1] value is always finite, so Of succeeds and the IfNone total projection never fires.
    static MeasureValue Fraction(double value, Op key) =>
        MeasureValue.Of(value, UnitsNet.Units.RatioUnit.DecimalFraction, key).IfFail(MeasureValue.Zero);
}

// The per-m² embodied-carbon CARBON-only per-module GwpTotal stage vector ToProperties embeds into the seam Environmental
// case's FULL (ImpactCategory × LifecycleStage) matrix through MaterialPropertySet.Environmental.CarbonMatrix — the EN
// 15804 generic float-glass GWP scaled by the IGU glass mass per m² (the cradle-to-gate A1-A3 dominant, A4-D the
// transport/install/end-of-life tail), over the SAME EN 15978 LifecycleStage banding the seam matrix's GwpTotal row lays
// out (kgCO2e per the PerM2 basis); the un-declared EN 15804+A2 indicator rows zero (the seam's partial-EPD invariant).
public static class GlazingGwp {
    const double GlassDensityKgM3 = 2500.0;
    const double GlassGwpPerKg = 1.20;   // EN 15804 generic float glass cradle-to-gate kgCO2e/kg

    public static ReadOnlyMemory<double> StagesPerM2(GlazingSection section) {
        double massPerM2 = section.Panes.Sum(static p => GlassDensityKgM3 * p.ThicknessMm.Value / 1000.0);
        double a1a3 = massPerM2 * GlassGwpPerKg;
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
public static class ProfileCatalogue {
    // The bounded standards body is ProfileAuthority.En (the EN 1279 IGU authority, region "eu"); an IGU has no mortar
    // joint, so StandardJointThicknessMm is 0.0 — never a free authority string.
    static readonly ProfileStandard IguStandard = new("eu", StandardJointThicknessMm: 0.0, Authority: ProfileAuthority.En);

    // The EN 1279 IGU builds as full pane/cavity sub-tables — the asymmetric 6-16-4, the inboard-coated 6-16-6-lowe,
    // and the outboard-laminated 66.2-16-4 are each their own pane rows, never a designation-string suffix the model
    // could not honor. A pane row is (glass, thickness, coating, coated-surface, interlayer); a cavity row (gas, width).
    static readonly Seq<GlazingRow> GlazingRows = Seq(
        new GlazingRow("glazing.double-4-16-4", "warm-edge",
            Seq(new PaneRow("float", 4.0, "none", 0, 0.0), new PaneRow("float", 4.0, "none", 0, 0.0)),
            Seq(new CavityRow("argon", 16.0))),
        new GlazingRow("glazing.double-6-12-6", "warm-edge",
            Seq(new PaneRow("float", 6.0, "none", 0, 0.0), new PaneRow("float", 6.0, "none", 0, 0.0)),
            Seq(new CavityRow("argon", 12.0))),
        new GlazingRow("glazing.double-4-20-4", "warm-edge",
            Seq(new PaneRow("float", 4.0, "none", 0, 0.0), new PaneRow("float", 4.0, "none", 0, 0.0)),
            Seq(new CavityRow("argon", 20.0))),
        // The inboard pane carries a soft-coat double-silver low-E on its CoatedSurface 0 (the per-pane outboard face =
        // the cavity-facing surface of the inboard pane, EN 673 whole-IGU surface 3) — so Pane.EmissivityOf(0) reads the
        // εn 0.04 the cavity radiative term sees, the row honestly modeled, not a name suffix.
        new GlazingRow("glazing.double-6-16-6-lowe", "warm-edge",
            Seq(new PaneRow("float", 6.0, "none", 0, 0.0), new PaneRow("float", 6.0, "soft-coat-double", 0, 0.0)),
            Seq(new CavityRow("argon", 16.0))),
        new GlazingRow("glazing.double-4-12-4-alu", "cold-edge-aluminum",
            Seq(new PaneRow("float", 4.0, "none", 0, 0.0), new PaneRow("float", 4.0, "none", 0, 0.0)),
            Seq(new CavityRow("air", 12.0))),
        // The outboard pane is laminated 66.2 (two 3 mm glass + 0.4 mm PVB) — the InterlayerMm 0.4 the ToLayerSet
        // splits into glass-PVB-glass and the MassLawSpectrum reads for the +3 dB coincidence-damping acoustic bonus.
        new GlazingRow("glazing.double-66.2-16-4-lam", "warm-edge",
            Seq(new PaneRow("laminated", 6.4, "none", 0, 0.4), new PaneRow("float", 4.0, "none", 0, 0.0)),
            Seq(new CavityRow("argon", 16.0))),
        new GlazingRow("glazing.triple-4-16-4-16-4", "warm-edge",
            Seq(new PaneRow("float", 4.0, "none", 0, 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, 0.0)),
            Seq(new CavityRow("krypton", 16.0), new CavityRow("krypton", 16.0))),
        new GlazingRow("glazing.triple-4-12-4-12-4", "warm-edge",
            Seq(new PaneRow("float", 4.0, "none", 0, 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, 0.0), new PaneRow("float", 4.0, "soft-coat-double", 0, 0.0)),
            Seq(new CavityRow("argon", 12.0), new CavityRow("argon", 12.0))));

    static Fin<Pane> PaneOf(PaneRow r, Op key) =>
        from t in key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
        from glass in GlassType.TryGet(r.Glass, out GlassType? g) ? Fin.Succ(g!) : Fin.Fail<GlassType>(ProfileFault.Family(key, $"<unknown-glass:{r.Glass}>"))
        from coating in Coating.TryGet(r.Coating, out Coating? c) ? Fin.Succ(c!) : Fin.Fail<Coating>(ProfileFault.Family(key, $"<unknown-coating:{r.Coating}>"))
        select new Pane(glass, t, coating, r.CoatedSurface, r.InterlayerMm);

    static Fin<Cavity> CavityOf(CavityRow r, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
        from gas in CavityGas.TryGet(r.Gas, out CavityGas? g) ? Fin.Succ(g!) : Fin.Fail<CavityGas>(ProfileFault.Family(key, $"<unknown-cavity-gas:{r.Gas}>"))
        select new Cavity(gas, w);

    // The per-row Traverse is the assembly#MATERIAL_COMPOSITION CompositionAuthor.LayerSet idiom — Seq<A>.Traverse over
    // a Fin-returning arm yields Fin<Seq<A>> directly (the Fin applicative), so a single malformed pane/cavity short-
    // circuits the whole row to its ProfileFault and BuildGlazingRows drops it through Choose rather than seeding a partial.
    static Fin<GlazingShape> GlazingOf(GlazingRow r, Context context, Op key) =>
        from panes in r.Panes.Traverse(p => PaneOf(p, key))
        from cavities in r.Cavities.Traverse(c => CavityOf(c, key))
        from spacer in SpacerType.TryGet(r.Spacer, out SpacerType? s) ? Fin.Succ(s!) : Fin.Fail<SpacerType>(ProfileFault.Family(key, $"<unknown-spacer:{r.Spacer}>"))
        from section in GlazingSection.Of(panes, cavities, spacer, key)
        select new GlazingShape(ProfileId.Of(r.Designation), section, IguStandard);

    public static FrozenDictionary<ProfileId, Profile> BuildGlazingRows(Context context) =>
        GlazingRows
            .Choose(row => GlazingOf(row, context, default).ToOption())
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Glazing, unit, Coring.None, shape.Standard, shape.Section.Panes[0].Glass.Appearance))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [GLAZING_BUILD_TRANSCRIPTION]: REALIZED — the standard EN 1279 IGU builds carry the per-pane glass type / thickness / coating and the per-cavity gas / width as full sub-tables, so the double `4-16-4` (24 mm), the asymmetric `6-16-4`, the inboard-coated `6-16-6-lowe`, the outboard-laminated `66.2-16-4`, and the triple `4-16-4-16-4` configurations are each one `GlazingRow` with its `PaneRow`/`CavityRow` sub-rows — never the uniform single-pane-thickness placeholder a `lowe`/`lam`/`66.2` designation suffix could not honor (the prior model's `PaneThicknessMm` could not represent the asymmetric `6-16-4` nor the laminated `66.2` two-glass-plus-PVB pane, the named fake the rebuild cures). The build is DERIVED from the pane count (`GlazingBuild.OfPaneCount`), the panes = cavities + 1 topology gated at `GlazingSection.Of`; the remaining pane thicknesses, coating tiers, and electrochromic/vacuum (VIG) variants are one further `GlazingRow`/`GlassType`/`Coating` data addition, each one row, never a new type. The overall unit thickness is the pane-thickness sum plus the gap-width sum, the `IfcMaterialLayerSet` pane-cavity-pane stack (the laminated pane its glass-PVB-glass sub-stack) the wire shape.
- [EN_673_CENTER_OF_GLASS]: REALIZED — the `GlazingThermal.UgCenterOfGlass` is the EN 673 series-resistance chain the prior bare `λ/gap` conductance silently approximated: the external/internal surface films (`he = 23`, `hi = 8 W·m⁻²·K⁻¹`), each pane's conductive resistance (`t / λ_glass`), and each cavity's TOTAL conductance the conductive (`λ_gas / s`) PLUS the convective (the Nusselt number `Nu = max(1, 0.035·Ra^0.38)` over the Rayleigh number `Ra = ρ²·s³·g·c·ΔT / (T_m·μ·λ)` the gas density/viscosity/specific-heat and the gap width drive — the four EN 673 Annex-B gas properties now first-class on `CavityGas`) PLUS the RADIATIVE term (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹` over the two facing pane corrected emissivities). The radiative term is the load-bearing physics: a low-E `Coating.NormalEmissivity` of `0.04`/`0.02` collapses the cavity radiation the uncoated `0.837` dominates, so the `6-16-6-lowe` and the triple low-E rows compute a real Ug the prior emissivity-blind model could not distinguish from an uncoated unit of the same gas/gap — the entire reason a coated cavity outperforms an uncoated one. The `Coating.None` falls back to the glass `NormalEmissivity` so an uncoated surface needs no second store, and `Pane.EmissivityOf` is the ONE emissivity source (the coating where present, else the glass), never a parallel stored field.
- [EN_410_SOLAR_OPTICAL]: REALIZED — the IGU's solar factor `g` (SHGC) and visible transmittance `τv` are the OTHER defining glazing performance attributes (the façade-selection criterion the prior model omitted entirely), computed by `GlazingThermal.SolarOptical` as the SERIES TRANSMITTANCE — the product `Π(BaseSolarTransmittance·SolarMultiplier)` / `Π(BaseLightTransmittance·LightMultiplier)` of the panes in series — over the base glass transmittance scaled by each pane's `Coating.SolarMultiplier`/`LightMultiplier`, each a dimensionless `Ratio` `MeasureValue` (`RatioUnit.DecimalFraction`, the seam `MeasureValue.Of` `IsDimensionless` path that keeps the as-constructed unit rather than reprojecting to a throwing SI base). The PRODUCT is the directionally-correct first-order EN 410 center-of-glass simplification (a second pane genuinely lowers the unit's `g`, so a clear double reads ~0.75 not the single-pane 0.82 a per-pane geometric mean would falsely return — the deleted over-prediction that erased the pane-count effect); it is slightly conservative (UNDER the true `g`) because it omits the inter-reflection uplift (light reflected between panes adds transmittance back) and the inwardly-flowing absorbed-heat secondary gain, both requiring per-pane spectral REFLECTANCE/ABSORPTANCE the `Coating`/`GlassType` do not yet carry. The full EN 410 net `g` (the multi-layer τ/ρ/α recursion with the secondary heat term and the angular-dependent spectral ray-trace) is a `Coating` reflectance-column + `GlassType` absorptance/spectral-curve column growth, never a parallel optical owner.
- [GLAZING_ACOUSTIC_RW]: REALIZED — a laminated and an asymmetric IGU are acoustic products, so the family computes the IGU's airborne `Acoustic` spectrum (the `Composition/acoustic#ACOUSTIC_FOLDS` carrier) through `GlazingThermal.MassLawSpectrum`: the field-incidence mass law `R(f) = 20·log₁₀(m'·f) − 47 dB` over the total areal mass `m' = Σ(ρ_glass · t_pane)` evaluated at the seventeen `AcousticBand` one-third-octave centres, with a laminated-interlayer bonus (the PVB damps the coincidence dip, +3 dB) and an asymmetric-pane bonus (unequal panes shift the coincidence dips apart, +2 dB), the `Rw` the seam `RatingContour.Rw.Fit` read so the IGU rating and a `Rasm.Compute` assembly rating share ONE contour fit. The mass-law combined-mass estimate is the honest closed form the seam carries no decoupling cavity to refine (a `MaterialLayer` is material + thickness only, so the independent-leaf model that over-predicts by tens of dB is the deleted form); the spectral coincidence-frequency correction and the per-pane damping-loss-factor are an `Acoustic`-spectrum refinement `Rasm.Compute` carries when the laminated PVB's frequency-dependent loss factor is admitted, never a parallel acoustic owner.
- [MATERIAL_PROPERTY_LOWERING]: REALIZED — `GlazingSection.ToProperties` lowers the computed `GlazingPerformance` into the seam `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` set so the IGU material "has it all": the `Thermal` case carries the EN 673 Ug as a `HeatTransferCoefficient` `MeasureValue` (plus the mass-weighted glass conductivity and the vapour-tight `μ → ∞`), the `Acoustic` case the banded mass-law spectrum (the `Rw` a derived read), the `Environmental` case the per-m² EN 15804 generic float-glass GWP over the `LayerSet` glass mass (the EN 15978 `LifecycleStage` vector the `GlazingGwp.StagesPerM2` fold seeds, the same banding the seam carries), and the `Fire` case a fire-rated EI rating where any pane is `GlassType.FireRated`. The `Projection/material#MATERIAL_PROJECTOR` reads the IGU material's own `MaterialPropertySet` set (never an element Pset), so a `Rasm.Compute` energy route reads `props.Thermal.Map(t => t.UValue)`, a `Rasm.Bim` `Pset_*ThermalTransmittance` emitter the same Ug, and a façade designer the `g`/`τv`/`Rw` off one seam node — the glazing family's contribution to the unified rich element, exactly as `steel#STEEL_FAMILY` contributes `ComputedSection`/`DesignCapacity` to the structural seam.
- [IFCPROFILEDEF_GLAZING_ALIGNMENT]: glazing is the one family that is an `IfcMaterialLayerSet` rather than an `IfcProfileDef` profile — the `GlazingSection.ToLayerSet` bridge resolves the IGU to the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` (pane / cavity / pane), so a glazing unit round-trips to IFC 4.3 as the `IfcMaterialLayerSet` the layer-set assignment owner serializes (`IfcMaterialLayer.LayerThickness` per pane/cavity/sub-layer, `IfcMaterialLayer.IsVentilated` false for the sealed cavity), the laminated pane a glass-PVB-glass two-`MaterialLayer`-plus-interlayer sub-stack within the pane thickness (the prior model's dropped laminate now realized), the `SpacerType.PsiWmK` edge-seal a `Pset_` thermal-bridge property on the element rather than a profile column, and the low-E `Coating` an `IfcSurfaceStyle`/`Pset_` per-coated-surface property the typed `Coating` row, the layer label, and the `Pane.CoatedSurface` index carry — the bulk pane shading as `glass.crown` while the coating rides the surface, never a fabricated `glass.lowe` bulk row (the per-coating emissivity-to-surface-style mapping landed at the `Rasm.Bim` boundary). The `ToUnit` rectangle projection serves only the catalogue keying and the frame-rebate module the host overrides; the wire shape is the layer set, not a rectangle profile.
- [WHOLE_WINDOW_U_IS_COMPUTE]: `GlazingSection.Performance.UgCenterOfGlass` is the EN 673 CENTER-OF-GLASS U-value — the IGU pane-stack performance glazing OWNS and `ToProperties` lowers onto the seam `MaterialPropertySet.Thermal.UValue`. The EN ISO 10077-1 WHOLE-WINDOW `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` that INCORPORATES the `SpacerType.PsiWmK` perimeter linear thermal bridge AND the frame fraction (the frame `Uf` + the glazed/frame area split) is a `Rasm.Compute` ASSEMBLY concern, NOT a glazing-section field: a window is a side-by-side glazing-in-frame composition spanning the glazing material, the frame member, and the window element's quantities, so its `Uw` cannot be a property of the glazing alone. The `Rasm.Compute` `Analysis/aggregator` `AssemblyAggregator.AggregateWindow` fold (composed by the `Analysis/physics` thermal runner) reads the IGU `Ug` off the seam `Thermal.UValue`, the spacer `Ψg` off the window's `Pset` thermal-bridge property (where this family lowers it), and the frame `Uf` + areas off the window's parts and `Qto_*BaseQuantities`, computing the whole-window `Uw`. The clean ownership split: glazing OWNS the `Ug` + the `Ψg` datum + the layer-set/Pset egress; Compute OWNS the area-and-perimeter combination into `Uw` — glazing never computes a window-assembly U, and Compute never re-authors the glazing pane physics. Ripple counterpart: `Rasm.Compute/Analysis/aggregator` (the `AggregateWindow` whole-window owner) and `Rasm.Compute/Analysis/physics` (the thermal runner's window branch).
