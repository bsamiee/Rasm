# [MATERIALS_GLAZING]

THE GLAZING SEED PAGE — the `glazing` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Product`) grounded in the insulating-glass build physics. An IGU is a `Component` whose geometry is the `SectionProfile.Layered` arm (the pane/cavity/pane ply stack, `WidthMm = OverallMm` preserving the square gross projection), whose build inputs ride the `GlazingDetail` `DetailSchema.Product` `PropertyBag`, and whose engineering performance is COMPUTED from those bag-carried inputs at every read (never a stored scalar that drifts): the `GlazingThermal` EN 673 center-of-glass `Ug` over ONE shared series-resistance chain (conductive + convective-Nusselt + emissivity-radiative gas exchange; pillar-conduction + free-molecular + radiative vacuum exchange), the EN 410 / ISO 9050 multi-layer net solar factor `g` (`τe` plus the inward-flowing secondary flux `qi` the SAME chain partitions) and visible transmittance `τv`, and the mass-law `Acoustic` spectrum its `Rw` derives from. Glazing is the ONE family that contributes NO `ComputedSection`: an IGU crosses to IFC 4.3 as an `IfcMaterialLayerSet`, never an `IfcProfileDef`, so every `GlazingSeed` row is `Sectioned: false`, `graph.SectionOf` dereferences a glazing `ComponentId` to `(Component, None)`, and `SectionSolver.Solve` faults `ComponentFault.Section` `<unsectioned-profile:layered>` if a mis-flagged row reaches it. An asymmetric `6-16-4`, a coated `6-16-6-lowe`, a laminated `66.4-16-4`, and a vacuum `4-vac-4` are each one `GlazingRow` over typed `Pane`/`Cavity` sub-rows — never a uniform single-pane placeholder a designation suffix leaves unmodeled, never a per-unit `DoubleGlazedUnit` type. The vocabulary grows by data: a new IGU is one `GlazingRow`, a new glass a `GlassType` row, a new coating a `Coating` row, a new gas a `CavityGas` row, an interlayer an `Interlayer` row, a quad-pane build one `GlazingBuild` row — zero central edits. The page composes `Component/component#COMPONENT_OWNER` (`Component.Of`, `ComponentRow`, `SectionProfile.Layered`, `Ply`, `IfcBinding.Supertype`, `QuantityRow`, `ComponentDetail`, `ComponentFault`), the seam `Properties/property#DETAIL_SCHEMA` IGU product rows (`PaneBuild`/`CavityBuild`/`SpacerType`/`EdgeSeal`/`MuntinGrid`/`FireResistanceEi`), the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` lowering, `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic`/`AcousticBand`, `Rasm.Vectors` `PositiveMagnitude`, and the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` each pane shades to. The `Projection/component#COMPONENT_PROJECTOR` lowers the bag and composes `GlazingDetail.Properties` for the IGU material's seam set; its `CompositionAuthor` `LayerSet` bridge reads the `Layered` plies directly (each `Ply.Role` the human-readable `IfcMaterialLayer.Name`).

## [01]-[INDEX]

- [02]-[GLAZING_FAMILY]: the `GlassType` glass-substance axis (emissivity / conductivity / density / specific heat / substance GWP / thermal-form process adder / solar+visible optics / safety), the `Coating` low-E/solar-control axis (`Option<double>` corrected emissivity + transmittance multipliers + coated-face reflectance + sputter GWP), the `CavityGas` EN 673 Annex-B fill axis, the `Interlayer` lamination axis, the `Sealant`/`Desiccant` EN 1279 edge-seal vocabularies, the `MuntinStyle` grid axis, the `SpacerType` warm/cold-edge axis (Ψg + sight-line + conductivity + edge GWP), the `GlazingBuild` pane-count classification, the `CavityFill` `[Union]` (gas mixture vs ISO 19916 vacuum), the `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` typed rows, the `GlazingThermal` EN 673 + EN 410 + mass-law kernel over ONE resistance chain, the `GlazingPerformance` receipt, the `GlazingGwp` substance/process-split carbon vector, the `GlazingDetail` bag/properties/plies owner, and the `GlazingSeed` authored row table with its `Rows : Context -> Fin<Seq<ComponentRow>>` fold.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing vocabulary (`GlassType`, `Coating`, `CavityGas`, `Interlayer`, `Sealant`, `Desiccant`, `MuntinStyle`, `SpacerType`, `GlazingBuild` — FORM-law policy SmartEnums with data columns the kernel dispatches on; `CavityFill` the gas-vs-vacuum `[Union]`; `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` the typed stack rows); `GlazingThermal` the EN 673 + EN 410 + mass-law kernel sharing ONE series-resistance chain; `GlazingGwp` the substance/process-split carbon vector; `GlazingPerformance` the computed receipt; `GlazingDetail` the ONE glazing-build admission (`Of` the `DetailSchema.Product` bag + gates, `Properties` the seam `MaterialPropertySet` lowering, `Plies` the `SectionProfile.Layered` geometry); `GlazingSeed` the authored EN 1279 row table and the `Rows` fold `ComponentFamily.Glazing` binds.
- Cases: glass {float, low-iron, tempered, heat-strengthened, fire-rated} — each `GlassType` row carrying its uncoated normal emissivity (soda-lime `0.837`, the EN 673 Annex-A baseline), conductivity (soda-lime `1.00`, borosilicate `1.14`), EN 572-1 density (`2500`/`2230`) and specific heat (`720`/`830`), EN 15804 substance GWP-per-kg (soda-lime `1.43`, low-iron `1.50`, borosilicate `2.00` — the ONLY substance variants), thermal-form process GWP-per-m², broadband EN 410 solar+visible transmittance+reflectance, and safety class; a laminated pane is any glass plus an `Interlayer`, never a glass case. Coating {none, hard-coat-lowe, soft-coat-double, soft-coat-triple, solar-control} over `Option<double>` corrected emissivity (`None` for the uncoated face — no `double.NaN` sentinel). Gas {air, argon, krypton, xenon} over the four EN 673 Annex-B properties. Interlayer {none, pvb, sgp, eva}; spacer {warm-edge-stainless, warm-edge-foam, cold-edge-aluminum}; build {double, triple} DERIVED from `Panes.Count`. A build is `Seq<Pane>`/`Seq<Cavity>`/`SpacerType`/`EdgeSeal`/`Option<MuntinGrid>`/EI-minutes — never a section subtype, never a bespoke payload record.
- Entry: `GlazingSeed.Rows(Context) : Fin<Seq<ComponentRow>>` — the ONE generator fold: per row, `GlazingDetail.Of` admits the build (the shared `Admit` gate law — arity `panes = cavities + 1`, pane count named by `GlazingBuild`, fire-rated-pane⟺positive-EI totality with negative EI railed, per-pane interlayer EXACTNESS, per-cavity fill-fraction and ISO 19916 vacuum-ceiling gates — plus the degenerate-grid gate) and returns the Product bag; `GlazingThermal.Evaluate` gates the physics (the seam `Acoustic.Of` band admission); `GlazingDetail.Plies` builds the `Layered` geometry; `SectionProfile.Layered.Of(plies, overallMm, widthMm: overallMm, key)` rails the profile; `Component.Of(..., detail: Some(bag), key)` seals the row. `Traverse` is the rail — a malformed row ABORTS the build (`ComponentFault`, key-correlated), never a swallowed `Choose` drop. `GlazingDetail.Properties(panes, cavities, ei, key) : Fin<Seq<MaterialPropertySet>>` is the seam lowering the projector composes so the IGU material carries `Thermal`/`Acoustic`/`Environmental`/`Fire` — it composes the SAME `Admit` law, so a malformed stack faults identically at either ingress door.
- Packages: Rasm.Vectors (`PositiveMagnitude` — every pane/gap/pillar/bar column), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `MaterialPropertySet`, `MeasureValue`, `Dimension`, `MeasurementBasis`, `LifecycleStage`, `Acoustic`, `AcousticBand`, `FireRating`, `FireResistance`, `DetailSchema`, `PropertyValue`, `PropertyName`, `PropertyBag`), Rasm.Materials.Component (`Component`/`ComponentRow`/`SectionProfile`/`Ply`/`IfcBinding`/`QuantityRow`/`ComponentDetail`/`ComponentFault`/`Coring`/`ComponentStandard`/`ComponentAuthority`), Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet (`RatioUnit.DecimalFraction` — the dimensionless `g`/`τv` seam admission). VividOrange is NOT composed (glazing fills no profile solve). Wacton.Unicolour is NOT composed: a coating's OPTICAL signal crosses as the coated pane's content-keyed `Node.Appearance`; glazing tags the `MaterialId`, never the colour kernel.
- Growth: a new IGU is one `GlazingRow`; a new glass substance one `GlassType` row; a new coating tier one `Coating` row; a new gas one `CavityGas` row; a new interlayer one `Interlayer` row; a new edge-seal chemistry one `Sealant`/`Desiccant` row; a quad build one `GlazingBuild` row the derived `Build` read maps; an electrochromic variant a `GlassType` row plus a `Coating` row. The full per-wavelength `τ(λ)`/`ρ(λ)` angular EN 410 §5 spectral integral is a `GlassType`/`Coating` per-wavelength-curve column growth the broadband recursion here is the center-of-glass simplification of, never a parallel optical owner.
- Boundary: the `Layered` profile is the geometric GROSS only — the physics reads the typed `Pane`/`Cavity` rows, never re-parses plies. `GlazingThermal.Evaluate` computes ONE ordered series-resistance chain (`he = 23`, `hi = 8 W·m⁻²·K⁻¹` surface films, per-pane glass-plus-interlayer conductive resistance, per-cavity conductance) that BOTH `Ug = 1/ΣR` AND the EN 410 `qi = Σ αe,i·R_out,i/R_tot` read; the cavity conductance dispatches on the `CavityFill` arm — a gas cavity sums the EN 673 convective (Nusselt over the §B.2 volume-mixed Rayleigh) and radiative (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹`) terms; a vacuum cavity sums the Collins pillar conduction (`2·λ_glass·a/p²`), the free-molecular residual-gas term (∝ ISO 19916 pressure), and the same radiative term with no convection. `g`/`τv` are dimensionless `Ratio` `MeasureValue`s (`RatioUnit.DecimalFraction`, the seam `IsDimensionless` path); the `Ug` mints through `QuantityRow.HeatTransferCoefficient.OfSi` (the one typed-mint owner — content keys byte-identical to the prior hand mint); bag rows ride the DIMENSION-only `MeasureValue.OfSi(Dimension, si)` so an authored and an imported bag content-key identically. The `SpacerType.PsiWmK` edge bridge is the `Rasm.Compute` `AssemblyAggregator` whole-window `Uw` input read off the seam receipt — glazing OWNS `Ug` + the `Ψg` datum, Compute OWNS the area-and-perimeter combination. The `Layered` plies carry the alternating glass / cavity / glass rows (a laminated pane its glass-interlayer-glass sub-plies within the pane thickness, a vacuum cavity an `IsVentilated`-false sealed gap at the Bim edge), each `Ply.Role` the human-readable layer name the `CompositionAuthor` `LayerSet` bridge and Bim `IfcMaterialLayer.Name` round-trip; the low-E `Coating` rides the coated pane's `Node.Appearance`, never a fabricated `glass.lowe` bulk row; the `MuntinGrid` is FACE geometry the generator places across the pane, never a through-thickness ply. The whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` is Compute's; face dimensions are OCCURRENCE geometry (a window order), never a type column — the prior constant `1200×1500` catalogue face carried zero per-row information and is deleted.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using NodaTime;                      // LocalDate — the PropertyEvidence expiry axis the EPD evidence row carries
using Rasm.Vectors;                  // PositiveMagnitude (the kernel >0 finite magnitude atom — NOT Rasm.Domain)
using Rasm.Domain;                   // Context, Op, AcceptValidated
using Rasm.Element;                  // MaterialId, MaterialPropertySet, MeasureValue, MeasurementBasis, LifecycleStage,
                                     // Acoustic, AcousticBand, FireRating, FireResistance, DetailSchema, PropertyValue, PropertyName
using Thinktecture;
using UnitsNet;                      // RatioUnit (the dimensionless g/τv fraction unit, admitted through the seam MeasureValue)
using Dimension = Rasm.Element.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Vectors discrete-count Dimension
using static LanguageExt.Prelude;
using static Rasm.Materials.Component.ComponentDetail;   // Token / Measured / ProductRows (the relocated bag constructors)

// The seed pages share the parent namespace: the per-family owners are the collision-free <Family>Seed /
// <Family>Detail statics, so the prior per-family sub-namespace (a CS0101 workaround for sibling
// ComponentCatalogue statics) is retired with the statics that forced it.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The glass-substance axis: uncoated NORMAL EMISSIVITY (soda-lime εn 0.837, the EN 673 Annex-A baseline the cavity
// radiative term reads absent a coating), CONDUCTIVITY (soda-lime 1.00, borosilicate 1.14 W·m⁻¹·K⁻¹), EN 572-1 DENSITY
// (2500 / 2230 kg·m⁻³) and SPECIFIC HEAT (720 / 830 J·kg⁻¹·K⁻¹), EN 15804 RAW-SUBSTANCE GWP-per-kg (cradle-to-gate pane
// substance ONLY — low-iron and borosilicate the only substance variants; secondary process carbon is the per-m² adders,
// never double-counted into this base), thermal-FORM process GWP-per-m² (tempering / heat-strengthening / ceramic-firing),
// broadband EN 410 / ISO 9050 SOLAR and VISIBLE transmittance and reflectance, and the safety class. All columns
// PUBLISHED (EN 572-1 / EN 673 Annex A / EN 15804 generic); a laminated pane is any glass plus an Interlayer, not a case.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassType {
    public static readonly GlassType Float            = new("float",             normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType LowIron          = new("low-iron",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.50, formProcessGwpPerM2: 0.0, solarTransmittance: 0.90, solarReflectance: 0.080, visibleTransmittance: 0.91, visibleReflectance: 0.08, safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType Tempered         = new("tempered",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: true,  appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType HeatStrengthened = new("heat-strengthened", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.9, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType FireRated        = new("fire-rated",        normalEmissivity: 0.837, conductivityWmK: 1.14, densityKgM3: 2230.0, specificHeatJKgK: 830.0, substanceGwpPerKg: 2.00, formProcessGwpPerM2: 5.0, solarTransmittance: 0.70, solarReflectance: 0.070, visibleTransmittance: 0.85, visibleReflectance: 0.08, safety: true,  appearance: MaterialId.Of("glass.flint"));
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

    // The library appearance ROW COLUMN each pane shades to (clear crown; the heavier flint for fire-rated
    // borosilicate — a row value, never an identity switch on `this`). A low-E/solar-control COATING is a thin-film
    // surface effect, NOT a bulk shade — it rides the coated pane's Node.Appearance, so this column takes no Coating knob.
    public MaterialId Appearance { get; }
}

// The low-E / solar-control coating axis: Option<double> CORRECTED NORMAL EMISSIVITY the EN 673 cavity radiative term
// reads (None = the uncoated face, the glass NormalEmissivity stands — no NaN sentinel; pyrolytic 0.16, double-silver
// 0.04, triple-silver 0.02 vs uncoated 0.837), the EN 410 solar/visible TRANSMITTANCE multipliers over the base glass,
// the Option<double> coated-face SOLAR/VISIBLE REFLECTANCE (the elevated reflectance the recursion reads on the coated
// face), and the sputter/pyrolytic process GWP-per-m². A new coating tier is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coating {
    public static readonly Coating None           = new("none",            correctedEmissivity: Option<double>.None, solarTransmittanceMultiplier: 1.00, coatedSolarReflectance: Option<double>.None, visibleTransmittanceMultiplier: 1.00, coatedVisibleReflectance: Option<double>.None, processGwpPerM2: 0.0);
    public static readonly Coating HardCoatLowE   = new("hard-coat-lowe",   correctedEmissivity: Some(0.16), solarTransmittanceMultiplier: 0.80, coatedSolarReflectance: Some(0.20), visibleTransmittanceMultiplier: 0.95, coatedVisibleReflectance: Some(0.11), processGwpPerM2: 0.5);
    public static readonly Coating SoftCoatDouble = new("soft-coat-double", correctedEmissivity: Some(0.04), solarTransmittanceMultiplier: 0.55, coatedSolarReflectance: Some(0.30), visibleTransmittanceMultiplier: 0.90, coatedVisibleReflectance: Some(0.11), processGwpPerM2: 2.0);
    public static readonly Coating SoftCoatTriple = new("soft-coat-triple", correctedEmissivity: Some(0.02), solarTransmittanceMultiplier: 0.40, coatedSolarReflectance: Some(0.34), visibleTransmittanceMultiplier: 0.82, coatedVisibleReflectance: Some(0.12), processGwpPerM2: 3.0);
    public static readonly Coating SolarControl   = new("solar-control",    correctedEmissivity: Some(0.04), solarTransmittanceMultiplier: 0.30, coatedSolarReflectance: Some(0.40), visibleTransmittanceMultiplier: 0.55, coatedVisibleReflectance: Some(0.15), processGwpPerM2: 3.0);
    public Option<double> CorrectedEmissivity { get; }
    public double SolarTransmittanceMultiplier { get; }
    public Option<double> CoatedSolarReflectance { get; }
    public double VisibleTransmittanceMultiplier { get; }
    public Option<double> CoatedVisibleReflectance { get; }
    public double ProcessGwpPerM2 { get; }
}

// The cavity fill gas: the four EN 673 Annex-B properties at the 283 K mean cavity temperature — CONDUCTIVITY λ,
// DENSITY ρ, dynamic VISCOSITY μ, SPECIFIC HEAT c — so the convective Nusselt/Rayleigh term reads a typed gas receipt.
// A mixture is the volume-weighted blend of the fill gas and the CavityFill.GasFill balance gas, computed in the kernel.
// All columns PUBLISHED (EN 673 Annex B).
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

// The lamination interlayer axis (EN 14449 / EN 12543): NOMINAL single-ply thickness (PVB 0.38, SGP 0.89, EVA 0.38 mm —
// multi-ply is a thicker captured total), ACOUSTIC coincidence-dip DAMPING bonus, SHEAR MODULUS (PVB ~2, SGP ~110,
// EVA ~8 MPa — the structural stiffness the laminate transfers), CONDUCTIVITY (~0.2 W·m⁻¹·K⁻¹ — the pane conductive
// resistance adds it), DENSITY, substance GWP-per-kg, lamination process GWP-per-m². None is the monolithic pane.
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

// The EN 1279-2 edge-seal sealant: the primary moisture barrier (PIB butyl) and the structural/durability secondary seal
// (silicone is structural-glazing-rated; polysulfide and hot-melt-butyl are not), each with its seal GWP-per-perimeter-metre.
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

// The EN 1279-2 spacer desiccant: molecular sieve or silica, carrying its water-adsorption capacity (kg water per kg
// desiccant) the durability/dew-point reserve reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Desiccant {
    public static readonly Desiccant MolecularSieve3A = new("molecular-sieve-3a", adsorptionCapacity: 0.22);
    public static readonly Desiccant Silica           = new("silica",             adsorptionCapacity: 0.30);
    public double AdsorptionCapacity { get; }
}

// The muntin/grid style: true-divided structural grid, simulated-divided applied grille, or between-glass grille. Bar
// dimensions are MANUFACTURER values (no EN/ASTM table grounds them), captured on the MuntinGrid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MuntinStyle {
    public static readonly MuntinStyle TrueDivided      = new("true-divided");
    public static readonly MuntinStyle SimulatedDivided = new("simulated-divided");
    public static readonly MuntinStyle BetweenGlass     = new("between-glass");
}

// The edge-seal spacer axis: the EN ISO 10077-1 linear thermal-bridge Ψg (warm-edge stainless 0.04, warm-edge foam 0.03,
// cold-edge aluminium 0.11 W·m⁻¹·K⁻¹), the SIGHT-LINE width, the spacer-frame CONDUCTIVITY, and the spacer+seal
// fabrication GWP-per-perimeter-metre. Spacer DEPTH is the cavity gap (read from the cavity, not stored). The whole-window
// Uw combining Ψg with Ug and the frame fraction is Rasm.Compute AssemblyAggregator's — glazing OWNS Ug + the Ψg datum.
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

// The IGU build classification DERIVED from the pane count — the layer-count semantics as a vocabulary, never a stored
// field a malformed "double" row with three panes contradicts. A quad build is one more row this derived read then maps.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlazingBuild {
    public static readonly GlazingBuild Double = new("double", panes: 2);
    public static readonly GlazingBuild Triple = new("triple", panes: 3);
    public int Panes { get; }

    public static Option<GlazingBuild> OfPaneCount(int panes) =>
        Items.FirstOrDefault(b => b.Panes == panes) is { } build ? Some(build) : None;
}

// The cavity fill discriminant: a GAS fill (the EN 673 mixture — the fill gas at FillFraction, the remainder the Balance
// gas, typically air) or a VACUUM fill (the ISO 19916 VIG — residual pressure + the support-pillar geometry the Collins
// pillar-conduction model reads). The kernel dispatches the cavity conductance on the arm: gas convects (Nusselt) and
// radiates; vacuum conducts through pillars and the free-molecular residual gas and radiates with no convection.
[Union]
public abstract partial record CavityFill {
    public sealed record GasFill(CavityGas Gas, double FillFraction, CavityGas Balance) : CavityFill;
    public sealed record VacuumFill(double ResidualPressurePa, PositiveMagnitude PillarRadiusMm, PositiveMagnitude PillarPitchMm) : CavityFill;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The volume-mixed gas property carrier the kernel computes from a CavityFill.GasFill — the EN 673 §B.2 volume-fraction
// blend of fill and balance gas, read by the Nusselt convective term.
public readonly record struct GasProperties(double ConductivityWmK, double DensityKgM3, double ViscosityPaS, double SpecificHeatJKgK);

// One pane in the IGU stack: glass substance, TOTAL thickness, surface Coating, the coated-face flag (false = the
// outboard face, true = the inboard face — EN 673 reads the coated surface facing the cavity; a BOUNDED flag, so a
// mis-transcribed surface index is unrepresentable rather than silently uncoated), and an Interlayer with its total
// thickness (0 / Interlayer.None monolithic; > 0 the glass-interlayer-glass laminate). Glass-only = total − interlayer.
public readonly record struct Pane(GlassType Glass, PositiveMagnitude ThicknessMm, Coating Coating, bool CoatedInboard, Interlayer Interlayer, double InterlayerThicknessMm) {
    public bool IsLaminated => Interlayer != Interlayer.None && InterlayerThicknessMm > 0.0;
    public double GlassThicknessMm => ThicknessMm.Value - InterlayerThicknessMm;

    // The EN 673 emissivity of ONE of the pane's two faces: the Coating's corrected εn on the COATED face (a coating
    // suppresses radiation across the cavity it faces, not the opposite face), else the glass NormalEmissivity.
    public double EmissivityOf(bool inboard) =>
        inboard == CoatedInboard ? Coating.CorrectedEmissivity.IfNone(Glass.NormalEmissivity) : Glass.NormalEmissivity;

    // The pane's directional solar optics (τ, ρ_front, ρ_back) the EN 410 recursion combines: transmittance = glass base
    // × coating multiplier; the COATED face carries the coating's elevated reflectance, the opposite the glass value,
    // so a low-E/solar-control pane is asymmetric front-to-back.
    public (double T, double Rf, double Rb) Solar() {
        double t = Glass.SolarTransmittance * Coating.SolarTransmittanceMultiplier;
        double rCoated = Coating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance);
        return (t, CoatedInboard ? Glass.SolarReflectance : rCoated, CoatedInboard ? rCoated : Glass.SolarReflectance);
    }

    public (double T, double Rf, double Rb) Visible() {
        double t = Glass.VisibleTransmittance * Coating.VisibleTransmittanceMultiplier;
        double rCoated = Coating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance);
        return (t, CoatedInboard ? Glass.VisibleReflectance : rCoated, CoatedInboard ? rCoated : Glass.VisibleReflectance);
    }
}

// One cavity in the IGU stack: the fill discriminant and the gap width the EN 673 / ISO 19916 conductance read.
public readonly record struct Cavity(CavityFill Fill, PositiveMagnitude WidthMm);

// The EN 1279-2 edge-seal construction: primary moisture sealant (PIB), structural/durability secondary sealant, spacer
// desiccant, and keyed-vs-bent corners — the durability + edge-thermal + GWP datums.
public readonly record struct EdgeSeal(Sealant Primary, Sealant Secondary, Desiccant Desiccant, bool CorneredKeys);

// The face muntin/grid: style, horizontal/vertical bar counts, manufacturer bar width/depth. FACE geometry the generator
// places across the pane, never a through-thickness ply.
public readonly record struct MuntinGrid(MuntinStyle Style, int HorizontalBars, int VerticalBars, PositiveMagnitude BarWidthMm, PositiveMagnitude BarDepthMm);

// The computed IGU receipt the seam lowering reads — the DEFINING glazing performance COMPUTED from the build: the EN 673
// center-of-glass U-value, the EN 410 net solar factor g (SHGC) and visible transmittance τv (dimensionless Ratio
// measures), and the mass-law Acoustic spectrum the Rw derives from.
public readonly record struct GlazingPerformance(
    MeasureValue UgCenterOfGlass,
    MeasureValue SolarFactorG,
    MeasureValue LightTransmittanceTv,
    Acoustic Acoustic) {
    public int Rw => Acoustic.Rw;

    // The NFRC light-to-solar-gain selection ratio LSG = τv/g — a derived read over the two stored measures (the
    // GoverningRadiusMm pattern), listed beside Ug/g/τv on every IGU datasheet; an opaque build reads 0.
    public double LightToSolarGain => SolarFactorG.Si > 0.0 ? LightTransmittanceTv.Si / SolarFactorG.Si : 0.0;
}

// One authored EN 1279 IGU build: the designation, the spacer, the EN 1279-2 edge-seal construction (a structural-
// glazing build names its silicone secondary here — the Sealant.Structural datum consumers read), the TYPED
// pane/cavity sub-rows (SmartEnum refs and PositiveMagnitude literals directly — no string re-parse, an unknown key
// is unrepresentable), the EN 13501-2 EI minutes (0 absent a fire-rated pane), and the optional face grid. Each build
// is a distinct engineering unit — an AUTHORED row, never a generator target.
public readonly record struct GlazingRow(string Designation, SpacerType Spacer, EdgeSeal EdgeSeal, Seq<Pane> Panes, Seq<Cavity> Cavities, int FireResistanceEiMinutes, Option<MuntinGrid> Muntin);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 673 center-of-glass U-value + EN 410 / ISO 9050 net-g/τv projection + mass-law Acoustic spectrum — the glazing
// family's domain-physics owner. Evaluate computes ONE ordered series-resistance chain (surface films, per-pane
// conductive resistance, per-cavity conductance) that BOTH the Ug (1/ΣR) AND the EN 410 secondary heat flux qi read, so
// the optical and thermal kernels share the resistance network rather than re-deriving it. Evaluate is INTERIOR over an
// Admit-gated stack — both ingress doors run GlazingDetail.Admit first, so the position indexing never sees a malformed
// arity and no gate re-runs here. KERNEL EXEMPTION: the indexed resistance-array and band-array loops are the measured
// numeric kernel this page names — the chain is position-indexed (pane i faces cavity i and i−1), so the fold state IS
// the index.
public static class GlazingThermal {
    const double SurfaceExternalWmK = 23.0;        // EN 673 external surface coefficient he (W·m⁻²·K⁻¹)
    const double SurfaceInternalWmK = 8.0;         // EN 673 internal surface coefficient hi
    const double StefanBoltzmann = 5.67e-8;        // σ (W·m⁻²·K⁻⁴)
    const double MeanTemperatureK = 283.0;         // EN 673 mean cavity temperature (10 °C)
    const double TemperatureDeltaK = 15.0;         // EN 673 reference ΔT across the cavity
    const double GravityMs2 = 9.81;
    const double MassLawOffsetDb = 47.0;           // field-incidence mass-law offset R = 20·log₁₀(m'·f) − 47
    const double FreeMolecularConductanceAirPerPa = 1.2;   // free-molecular (Knudsen-regime) air conduction W·m⁻²·K⁻¹·Pa⁻¹, near-unity accommodation — the VIG residual-gas term

    public static Fin<GlazingPerformance> Evaluate(Seq<Pane> panes, Seq<Cavity> cavities, Op key) {
        double[] rPane = panes.Map(PaneConductiveResistance).ToArray();
        double[] rCav = new double[cavities.Count];
        for (int i = 0; i < cavities.Count; i++) rCav[i] = 1.0 / CavityConductance(panes, cavities, i);
        double rse = 1.0 / SurfaceExternalWmK, rsi = 1.0 / SurfaceInternalWmK;
        double rTot = rse + rPane.Sum() + rCav.Sum() + rsi;
        double ug = 1.0 / rTot;
        double g = SolarFactor(panes, rPane, rCav, rTot, rse);
        double tv = Span(panes, 0, panes.Count, static p => p.Visible()).T;
        // g/τv admit as dimensionless Ratio measures (RatioUnit.DecimalFraction, the seam IsDimensionless path — no SI
        // reprojection, content keys frozen); the Ug typed mint routes through QuantityRow.HeatTransferCoefficient.
        return from acoustic in MassLawSpectrum(panes, key)
               from solarG in MeasureValue.Of(g, UnitsNet.Units.RatioUnit.DecimalFraction, key)
               from lightTv in MeasureValue.Of(tv, UnitsNet.Units.RatioUnit.DecimalFraction, key)
               select new GlazingPerformance(QuantityRow.HeatTransferCoefficient.OfSi(ug), solarG, lightTv, acoustic);
    }

    // Each pane's conductive resistance t/λ — the glass conductive path plus the interlayer's small sub-resistance
    // (a thin ~0.2 W·m⁻¹·K⁻¹ polymer; zero for a monolithic pane where InterlayerThicknessMm is 0).
    static double PaneConductiveResistance(Pane p) =>
        (p.GlassThicknessMm / 1000.0) / p.Glass.ConductivityWmK + (p.InterlayerThicknessMm / 1000.0) / p.Interlayer.ConductivityWmK;

    // Each cavity's total conductance h_total dispatched on the CavityFill arm. The cavity sees the INBOARD face of
    // pane i and the OUTBOARD face of pane i+1: a low-E coating lowers h_rad only when it sits on one of these two
    // cavity-facing faces. A gas cavity convects (Nusselt over the volume-mixed gas) and radiates; a
    // vacuum cavity conducts through the Collins pillar array (2·λ_glass·a/p² over the two bounding panes' mean
    // conductivity) and the free-molecular residual gas (∝ pressure) and radiates with no convection.
    static double CavityConductance(Seq<Pane> panes, Seq<Cavity> cavities, int i) {
        Cavity cavity = cavities[i];
        double hRad = RadiativeCoefficient(panes[i].EmissivityOf(inboard: true), panes[i + 1].EmissivityOf(inboard: false));
        double s = cavity.WidthMm.Value / 1000.0;
        return cavity.Fill.Switch(
            gasFill: gas => {
                GasProperties p = EffectiveGas(gas);
                return Nusselt(p, s) * p.ConductivityWmK / s + hRad;
            },
            vacuumFill: vac => {
                double kGlass = 0.5 * (panes[i].Glass.ConductivityWmK + panes[i + 1].Glass.ConductivityWmK);
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

    // EN 673 radiative coefficient h_r = 4·σ·T_m³ / (1/ε₁ + 1/ε₂ − 1): the two facing corrected emissivities drive the
    // cavity radiation — uncoated 0.837/0.837 yields the dominant exchange, a single low-E 0.04 surface collapses it by
    // an order of magnitude (the entire reason a coated cavity outperforms an uncoated one of the same gas/gap).
    static double RadiativeCoefficient(double e1, double e2) =>
        4.0 * StefanBoltzmann * MeanTemperatureK * MeanTemperatureK * MeanTemperatureK / (1.0 / e1 + 1.0 / e2 - 1.0);

    // EN 673 Nusselt number Nu = max(1, 0.035·Ra^0.38) for a vertical cavity over the Rayleigh number
    // Ra = ρ²·s³·g·c·ΔT / (T_m·μ·λ). Below the critical Rayleigh the cavity does not convect (Nu = 1, pure conduction
    // λ/s); above it the gas circulates and raises the conductance — the heavier krypton/xenon suppresses both terms.
    static double Nusselt(GasProperties gas, double s) {
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return Math.Max(1.0, 0.035 * Math.Pow(ra, 0.38));
    }

    // The EN 410 / ISO 9050 net solar factor g = τe + qi: the multi-layer transmittance τe (panes combined through the
    // two-flux recursion) plus the secondary internal heat flux qi — each pane's absorptance αe,i times its inward-flowing
    // fraction R_out,i/R_tot, the inward fraction being the resistance from the outer environment to the pane centre over
    // the total resistance the SHARED chain already computed (absorbed heat flows inward in proportion to the resistance
    // to the OTHER side). A clear double reads a real g below the single-pane value; a solar-control coat collapses it.
    static double SolarFactor(Seq<Pane> panes, double[] rPane, double[] rCav, double rTot, double rse) {
        double te = Span(panes, 0, panes.Count, static p => p.Solar()).T;
        double qi = 0.0;
        for (int j = 0; j < panes.Count; j++) {
            double rOut = rse + 0.5 * rPane[j];
            for (int k = 0; k < j; k++) rOut += rPane[k] + rCav[k];
            qi += SolarAbsorptance(panes, j) * rOut / rTot;
        }
        return Math.Clamp(te + qi, 0.0, 1.0);
    }

    // The EN 410 per-pane absorptance with full inter-reflection: the forward flux density Φj incident on pane j (the
    // outer sub-stack transmittance over the multiple-reflection denominator between the outer back reflectance and the
    // [pane j ⊕ inner] front reflectance) drives the front-incidence absorptance, plus the part transmitted through j and
    // reflected back by the inner sub-stack drives the back-incidence absorptance.
    static double SolarAbsorptance(Seq<Pane> panes, int j) {
        (double T, double Rf, double Rb) o = Span(panes, 0, j, static p => p.Solar());
        (double T, double Rf, double Rb) inn = Span(panes, j + 1, panes.Count, static p => p.Solar());
        (double T, double Rf, double Rb) pane = panes[j].Solar();
        double aFwd = 1.0 - pane.T - pane.Rf;
        double aBwd = 1.0 - pane.T - pane.Rb;
        double rJin = pane.Rf + pane.T * pane.T * inn.Rf / (1.0 - pane.Rb * inn.Rf);
        double phi = o.T / (1.0 - o.Rb * rJin);
        return phi * (aFwd + pane.T * inn.Rf * aBwd / (1.0 - pane.Rb * inn.Rf));
    }

    // The two-system EN 410 combination of system a (outboard) onto system b (inboard): transmittance, front reflectance,
    // and back reflectance of the combined system over the inter-reflection denominator 1 − ρa_back·ρb_front.
    static (double T, double Rf, double Rb) Combine((double T, double Rf, double Rb) a, (double T, double Rf, double Rb) b) {
        double d = 1.0 - a.Rb * b.Rf;
        return (a.T * b.T / d, a.Rf + a.T * a.T * b.Rf / d, b.Rb + b.T * b.T * a.Rb / d);
    }

    // The combined directional optics of the contiguous pane span [lo, hi) folded left-to-right; the empty span is the
    // clear identity (full transmission, zero reflectance). One Combine fold over a per-pane optics selector serves solar
    // AND visible.
    static (double T, double Rf, double Rb) Span(Seq<Pane> panes, int lo, int hi, Func<Pane, (double T, double Rf, double Rb)> optics) {
        (double T, double Rf, double Rb) acc = (1.0, 0.0, 0.0);
        for (int i = lo; i < hi; i++) acc = Combine(acc, optics(panes[i]));
        return acc;
    }

    // The field-incidence mass-law Acoustic spectrum: R(f) = 20·log₁₀(m'·f) − 47 dB over the total areal mass
    // m' = Σ(ρ_glass·t_glass + ρ_interlayer·t_interlayer) at the eighteen AcousticBand one-third-octave centres, with the
    // best interlayer's coincidence-dip damping bonus and an asymmetric-pane bonus (unequal panes shift the coincidence
    // dips apart). Absorption is glass's near-zero 0.03 flat; Rw is the seam RatingContour.Rw.Fit read. Admits through the
    // seam's public Acoustic.Of band gate (the curated vectors valid by construction).
    static Fin<Acoustic> MassLawSpectrum(Seq<Pane> panes, Op key) {
        double areal = panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 + p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0);
        double bonus = panes.Fold(0.0, static (acc, p) => Math.Max(acc, p.Interlayer.AcousticDampingDb)) + (Asymmetric(panes) ? 2.0 : 0.0);
        double[] sri = new double[AcousticBand.Count];
        double[] absorption = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(areal, 1e-9) * band.CenterHz) - MassLawOffsetDb + bonus);
            absorption[band.Key] = 0.03;
        }
        return Acoustic.Of(absorption, sri, key);
    }

    // Asymmetric iff some pane thickness differs from the first — the unequal-pane coincidence-dip shift, a scan against
    // the first thickness (no Distinct materialization).
    static bool Asymmetric(Seq<Pane> panes) =>
        panes.Count >= 2 && panes.Exists(p => p.ThicknessMm.Value != panes[0].ThicknessMm.Value);
}

// The per-m² embodied-carbon stage vector the Environmental lowering embeds via
// MaterialPropertySet.Environmental.CarbonMatrix. A1-A3 splits RAW SUBSTANCE (each pane's and interlayer's mass times its
// per-kg base) from SECONDARY PROCESSING (the per-m² thermal-form, coating-sputter, lamination, and IGU-assembly adders)
// so the per-kg base is never double-counted; A4-D scale A1-A3 as the transport/install/end-of-life tail over the
// EN 15978 LifecycleStage banding (a negative D the recovery benefit beyond the system boundary).
public static class GlazingGwp {
    const double IguAssemblyGwpPerM2 = 2.5;   // EN 15804 IGU fabrication: spacer forming + gas fill + edge-seal + desiccant per m²

    public static ReadOnlyMemory<double> StagesPerM2(Seq<Pane> panes) {
        double substance = panes.Sum(static p =>
            p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 * p.Glass.SubstanceGwpPerKg
            + (p.IsLaminated ? p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0 * p.Interlayer.SubstanceGwpPerKg : 0.0));
        double processing = panes.Sum(static p =>
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

// The ONE glazing-build admission and its three seed-time projections: one Admit law gates the stack at BOTH ingress
// doors — Of (the seed) builds the DetailSchema.Product bag (build inputs — the seam-declared PaneBuild/CavityBuild/
// SpacerType/EdgeSeal/MuntinGrid/FireResistanceEi rows); Plies builds the SectionProfile.Layered geometry (each
// Ply.Role the human-readable layer name the CompositionAuthor LayerSet bridge and Bim IfcMaterialLayer.Name
// round-trip); Properties (the projector) lowers the computed receipt into the seam MaterialPropertySet set attached
// to the IGU material. Receipts are COMPUTED from the bag-carried inputs — never a stored scalar that drifts.
public static class GlazingDetail {
    const double VacuumIntegrityThresholdPa = 0.1;   // ISO 19916 functional-vacuum ceiling — above it the VIG is compromised

    // The EN 15804 generic-IGU EPD identity rides the seam evidence axis — the per-case epd/validUntilYear columns
    // are the deleted seam form.
    static readonly PropertyEvidence GenericEpd = new("epd", "en 15804 generic insulating glass unit", Option<LocalDate>.None);

    // The ONE stack gate law BOTH ingress doors compose — the seed Of and the projector Properties admit identically,
    // so a malformed pane/cavity set never reaches the kernel from either door: panes = cavities + 1 (an IGU alternates
    // pane/cavity/pane), the pane count one the GlazingBuild vocabulary names, the fire-rated-pane⟺positive-EI
    // relation total with a negative EI railed outright (the Fire property the lowering emits is always backed),
    // per-pane interlayer EXACTNESS (None pairs
    // with thickness exactly 0 — a negative thickness cannot hide behind None — a present interlayer with thickness in
    // (0, pane total)), and per-cavity fill sanity (gas fraction in (0,1]; vacuum pressure in (0, 0.1 Pa]). A violation
    // rails ComponentFault.Family rather than seeding a unit whose DERIVED Build mislabels.
    static Fin<Unit> Admit(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, Op key) =>
        panes.IsEmpty || cavities.Count != panes.Count - 1
            ? ComponentFault.Family(key, $"<glazing-stack-arity:panes={panes.Count}:cavities={cavities.Count}>")
            : GlazingBuild.OfPaneCount(panes.Count).IsNone
                ? ComponentFault.Family(key, $"<glazing-build-unmodeled-pane-count:{panes.Count}>")
                : fireEiMinutes < 0 || panes.Exists(static p => p.Glass == GlassType.FireRated) != (fireEiMinutes > 0)
                    ? ComponentFault.Family(key, $"<glazing-fire-rating-mismatch:ei={fireEiMinutes}>")
                    : panes.Find(static p => p.Interlayer == Interlayer.None
                            ? p.InterlayerThicknessMm != 0.0
                            : p.InterlayerThicknessMm <= 0.0 || p.InterlayerThicknessMm >= p.ThicknessMm.Value).Match(
                        Some: p => Fin.Fail<Unit>(ComponentFault.Family(key, $"<glazing-interlayer-inconsistent:{p.Interlayer.Key}:{p.InterlayerThicknessMm:R}>")),
                        None: () => cavities.Find(static c => c.Fill.Switch(
                                gasFill: static g => g.FillFraction is <= 0.0 or > 1.0,
                                vacuumFill: static v => v.ResidualPressurePa is <= 0.0 or > VacuumIntegrityThresholdPa)).Match(
                            Some: c => Fin.Fail<Unit>(ComponentFault.Family(key, $"<glazing-cavity-fill-out-of-range:{c.WidthMm.Value:R}mm>")),
                            None: () => Fin.Succ(unit)));

    // The seed door: the shared Admit law, the degenerate-grid gate (a PRESENT grid owns at least one non-negative
    // bar), then the Product bag.
    public static Fin<PropertyBag> Of(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes, Op key) =>
        from admitted in Admit(panes, cavities, fireEiMinutes, key)
        from grid in guard(muntin.ForAll(static m => m.HorizontalBars >= 0 && m.VerticalBars >= 0 && m.HorizontalBars + m.VerticalBars > 0),
            ComponentFault.Family(key, "<glazing-muntin-degenerate>"))
        select Bag(panes, cavities, spacer, edgeSeal, muntin, fireEiMinutes);

    // The projector door: the seam MaterialPropertySet set the IGU material carries — Thermal the EN 673 Ug + the
    // series-harmonic glass conductivity + the mass-weighted specific heat (EN 572-1 soda-lime 720, borosilicate
    // 830 J·kg⁻¹·K⁻¹) + the vapour-tight μ (EN ISO 13788 μ → ∞ for a sealed IGU); Acoustic the banded spectrum (Rw a
    // derived read); Environmental the substance/process-split per-m² GWP under the GenericEpd evidence; Fire the
    // parameterized EN 13501-2 EI rating where EI minutes are positive.
    public static Fin<Seq<MaterialPropertySet>> Properties(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, Op key) =>
        from admitted in Admit(panes, cavities, fireEiMinutes, key)
        from perf in GlazingThermal.Evaluate(panes, cavities, key)
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: GlassConductivity(panes),
            specificHeat: GlassSpecificHeat(panes),
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6,
            key)
        from environmental in MaterialPropertySet.OfEnvironmental(
            MeasurementBasis.PerM2, MaterialPropertySet.Environmental.CarbonMatrix(GlazingGwp.StagesPerM2(panes)),
            recycledContent: 0.25, endOfLifeRecovery: 0.90, key, evidence: GenericEpd)
        let acoustic = MaterialPropertySet.OfAcoustic(perf.Acoustic)
        let core = Seq(thermal, acoustic, environmental)
        select fireEiMinutes > 0
            ? core.Add(MaterialPropertySet.OfFire(FireRating.A1, FireResistance.Ei(fireEiMinutes)))
            : core;

    // The Layered geometry: alternating pane / cavity plies — a monolithic pane one glass ply; a laminated pane the
    // glass-interlayer-glass sub-plies within the pane thickness (the interlayer shades as clear glass: an optically
    // near-glass transparent polymer whose laminate identity rides the Role, not a fabricated polymer appearance row);
    // a cavity the gas.cavity ply (a vacuum cavity an IsVentilated-false sealed gap at the Bim edge). Ply.Role carries
    // the human-readable layer name; the sub-ply half-thickness and interlayer lifts rail on the dimensional band.
    public static Fin<Seq<Ply>> Plies(Seq<Pane> panes, Seq<Cavity> cavities, Op key) =>
        toSeq(Enumerable.Range(0, panes.Count + cavities.Count))
            .Traverse(slot => (slot & 1) == 0 ? PanePlies(panes[slot / 2], slot / 2, key) : CavityPly(cavities[slot / 2], slot / 2)).As()
            .Map(static plies => plies.Bind(static p => p));

    static Fin<Seq<Ply>> PanePlies(Pane pane, int index, Op key) =>
        pane.IsLaminated
            ? from half in key.AcceptValidated<PositiveMagnitude>(candidate: pane.GlassThicknessMm / 2.0)
              from inter in key.AcceptValidated<PositiveMagnitude>(candidate: pane.InterlayerThicknessMm)
              select Seq(
                  new Ply(pane.Glass.Appearance, half, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}-outer"),
                  new Ply(MaterialId.Of("glass.crown"), inter, $"pane-{index}-{pane.Interlayer.Key}-interlayer"),
                  new Ply(pane.Glass.Appearance, half, $"pane-{index}-{pane.Glass.Key}-inner"))
            : Fin.Succ(Seq(new Ply(pane.Glass.Appearance, pane.ThicknessMm, $"pane-{index}-{pane.Glass.Key}-{pane.Coating.Key}")));

    static Fin<Seq<Ply>> CavityPly(Cavity c, int index) =>
        Fin.Succ(Seq(new Ply(MaterialId.Of("gas.cavity"), c.WidthMm, c.Fill.Switch(
            gasFill: gas => $"cavity-{gas.Gas.Key}-{gas.FillFraction:P0}-{index}",
            vacuumFill: vac => $"cavity-vacuum-{vac.ResidualPressurePa:R}pa-{index}"))));

    // The DetailSchema.Product bag: the seam-declared IGU rows — PaneBuild/CavityBuild recursive List-of-Complex
    // sub-rows, the SpacerType token, the EdgeSeal complex, the optional MuntinGrid complex (omitted rows content-key a
    // gridless unit distinctly), and the EI minutes (SI seconds over the time dimension). Dimensional rows ride the
    // DIMENSION-only MeasureValue.OfSi so an authored and an imported bag content-key identically; discrete
    // indices/counts ride Text tokens (PropertyValue carries no integer case).
    static PropertyBag Bag(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes) {
        Seq<(PropertyName, PropertyValue)> rows = Seq(
            (DetailSchema.PaneBuild, (PropertyValue)new PropertyValue.List(panes.Map(PaneComplex).Map(static v => (PropertyValue)v))),
            (DetailSchema.CavityBuild, new PropertyValue.List(cavities.Map(CavityComplex).Map(static v => (PropertyValue)v))),
            Token(DetailSchema.SpacerType, spacer.Key),
            (DetailSchema.EdgeSeal, new PropertyValue.Complex("edge-seal", Map(
                (PropertyName.Create("Primary"), (PropertyValue)new PropertyValue.Text(edgeSeal.Primary.Key)),
                (PropertyName.Create("Secondary"), new PropertyValue.Text(edgeSeal.Secondary.Key)),
                (PropertyName.Create("Desiccant"), new PropertyValue.Text(edgeSeal.Desiccant.Key)),
                (PropertyName.Create("CorneredKeys"), new PropertyValue.Boolean(edgeSeal.CorneredKeys))))))
            + muntin.Match(
                Some: m => Seq((DetailSchema.MuntinGrid, (PropertyValue)new PropertyValue.Complex("muntin", Map(
                    (PropertyName.Create("Style"), (PropertyValue)new PropertyValue.Text(m.Style.Key)),
                    (PropertyName.Create("HorizontalBars"), new PropertyValue.Text($"{m.HorizontalBars}")),
                    (PropertyName.Create("VerticalBars"), new PropertyValue.Text($"{m.VerticalBars}")),
                    (PropertyName.Create("BarWidth"), Si(Dimension.LengthDim, m.BarWidthMm.Value * 1e-3)),
                    (PropertyName.Create("BarDepth"), Si(Dimension.LengthDim, m.BarDepthMm.Value * 1e-3)))))),
                None: () => Seq<(PropertyName, PropertyValue)>())
            + (fireEiMinutes > 0
                ? Seq(Measured(DetailSchema.FireResistanceEi, Dimension.Create(0, 0, 1, 0, 0, 0, 0), fireEiMinutes * 60.0))
                : Seq<(PropertyName, PropertyValue)>());
        return ProductRows([.. rows]);
    }

    // The bare dimension-only Measure value the recursive Complex sub-rows carry (the tuple-returning ComponentDetail
    // Measured serves top-level bag rows only).
    static PropertyValue Si(Dimension dimension, double si) => new PropertyValue.Measure(MeasureValue.OfSi(dimension, si));

    // CoatedSurface stays the wire "0"/"1" token (outboard/inboard) — the typed flag re-spells the model, never the bag.
    static PropertyValue.Complex PaneComplex(Pane p, int index) => new($"pane-{index}", Map(
        (PropertyName.Create("Glass"), (PropertyValue)new PropertyValue.Text(p.Glass.Key)),
        (PropertyName.Create("Thickness"), Si(Dimension.LengthDim, p.ThicknessMm.Value * 1e-3)),
        (PropertyName.Create("Coating"), new PropertyValue.Text(p.Coating.Key)),
        (PropertyName.Create("CoatedSurface"), new PropertyValue.Text(p.CoatedInboard ? "1" : "0")),
        (PropertyName.Create("Interlayer"), new PropertyValue.Text(p.Interlayer.Key)),
        (PropertyName.Create("InterlayerThickness"), Si(Dimension.LengthDim, p.InterlayerThicknessMm * 1e-3))));

    static PropertyValue.Complex CavityComplex(Cavity c, int index) => new($"cavity-{index}", c.Fill.Switch(
        gasFill: gas => Map(
            (PropertyName.Create("Gas"), (PropertyValue)new PropertyValue.Text(gas.Gas.Key)),
            (PropertyName.Create("FillFraction"), new PropertyValue.Text($"{gas.FillFraction:R}")),
            (PropertyName.Create("Balance"), new PropertyValue.Text(gas.Balance.Key)),
            (PropertyName.Create("Width"), Si(Dimension.LengthDim, c.WidthMm.Value * 1e-3))),
        vacuumFill: vac => Map(
            (PropertyName.Create("ResidualPressure"), (PropertyValue)Si(Dimension.PressureDim, vac.ResidualPressurePa)),
            (PropertyName.Create("PillarRadius"), Si(Dimension.LengthDim, vac.PillarRadiusMm.Value * 1e-3)),
            (PropertyName.Create("PillarPitch"), Si(Dimension.LengthDim, vac.PillarPitchMm.Value * 1e-3)),
            (PropertyName.Create("Width"), Si(Dimension.LengthDim, c.WidthMm.Value * 1e-3)))));

    // The homogenized glass-only Thermal columns, each under its own physical mixing law: conductivity the SERIES
    // harmonic mean Σt/Σ(t/λ) (the through-thickness slab law — an arithmetic mean overstates a mixed borosilicate/
    // soda-lime stack), specific heat the MASS-weighted mean Σ(ρ·t·c)/Σ(ρ·t) (heat capacity mixes by mass, never by
    // thickness). The per-cavity gas/vacuum resistance is the Ug the kernel already computed, so these are the
    // glass-only means a non-IGU thermal read uses. Admit guarantees a non-empty stack with positive glass per pane,
    // so both divisors are positive — no fallback knob exists.
    static double GlassConductivity(Seq<Pane> panes) =>
        panes.Sum(static p => p.GlassThicknessMm) / panes.Sum(static p => p.GlassThicknessMm / p.Glass.ConductivityWmK);

    static double GlassSpecificHeat(Seq<Pane> panes) =>
        panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm * p.Glass.SpecificHeatJKgK)
            / panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The glazing family seed — the AUTHORED EN 1279 IGU builds (SEED_ROW_LAW: AUTHORED, no vendor producer exists; values
// PUBLISHED from EN 673 Annex B / EN 410 / ISO 9050 / EN 1279). Each build is a distinct engineering unit — a typed row,
// never a generator target. ComponentFamily.Glazing binds Rows; every row is Sectioned: false (the IGU is an
// IfcMaterialLayerSet, not a profile — SectionSolver never sees a Layered arm from this family).
public static class GlazingSeed {
    // ComponentAuthority.En (EN 1279 IGU authority, region "eu"); an IGU lays no mortar joint. Every current build
    // ships the standard PIB primary + polysulfide secondary + molecular-sieve + keyed-corner EN 1279-2 construction;
    // a structural-glazing row swaps its own EdgeSeal column to a silicone secondary, never a parallel row shape.
    static readonly ComponentStandard IguStandard = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly EdgeSeal StandardEdgeSeal = new(Sealant.Pib, Sealant.Polysulfide, Desiccant.MolecularSieve3A, CorneredKeys: true);
    static readonly Option<MuntinGrid> NoGrid = Option<MuntinGrid>.None;
    static readonly Cavity Argon16 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0));
    static readonly Cavity Argon12 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(12.0));

    static Pane Mono(GlassType glass, double thicknessMm, Coating coating, bool coatedInboard = false) =>
        new(glass, PositiveMagnitude.Create(thicknessMm), coating, coatedInboard, Interlayer.None, 0.0);

    // The EN 1279 builds as full typed pane/cavity sub-rows — the asymmetric, inboard-coated, laminated, vacuum,
    // fire-rated, and gridded units each their own rows, never a designation suffix the model leaves unmodeled. Surface
    // conventions annotated per row where load-bearing.
    static readonly Seq<GlazingRow> Builds = Seq(
        new GlazingRow("glazing.double-4-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.None)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-6-12-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 6.0, Coating.None), Mono(GlassType.Float, 6.0, Coating.None)), Seq(Argon12), 0, NoGrid),
        new GlazingRow("glazing.double-4-20-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.None)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(20.0))), 0, NoGrid),
        // The inboard pane carries a soft-coat double-silver low-E on its outboard face (surface 3, cavity-facing) — so
        // Pane.EmissivityOf(inboard: false) reads the εn 0.04 the cavity radiative term sees, the row honestly modeled.
        new GlazingRow("glazing.double-6-16-6-lowe", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 6.0, Coating.None), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-4-12-4-alu", SpacerType.ColdEdgeAluminum, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.None)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Air, 1.00, CavityGas.Air), PositiveMagnitude.Create(12.0))), 0, NoGrid),
        // Laminated 66.4 outboard pane (two 3 mm glass + 0.76 mm two-ply PVB): Plies splits glass-PVB-glass; the
        // MassLawSpectrum reads the coincidence-damping bonus; 6.76-vs-4.0 asymmetry adds the dip-shift bonus.
        new GlazingRow("glazing.double-lam664-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(new Pane(GlassType.Float, PositiveMagnitude.Create(6.76), Coating.None, CoatedInboard: false, Interlayer.Pvb, 0.76), Mono(GlassType.Float, 4.0, Coating.None)),
            Seq(Argon16), 0, NoGrid),
        // Triple low-E on surfaces 2 and 5 (outer pane inboard face + inner pane outboard face) — each cavity sees one
        // low-E surface; krypton for the narrow gaps.
        new GlazingRow("glazing.triple-4-16kr-4-16kr-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble, coatedInboard: true), Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Krypton, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0)),
                new Cavity(new CavityFill.GasFill(CavityGas.Krypton, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0))), 0, NoGrid),
        new GlazingRow("glazing.triple-4-12ar-4-12ar-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble, coatedInboard: true), Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble)),
            Seq(Argon12, Argon12), 0, NoGrid),
        // ISO 19916 vacuum unit: soft-coat triple-silver on surface 2 suppressing the now-dominant radiative exchange; a
        // 0.3 mm gap at 0.08 Pa with 0.25 mm-radius pillars on a 20 mm pitch (the Collins conduction the kernel reads).
        new GlazingRow("glazing.vig-4lowe-vac-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.SoftCoatTriple, coatedInboard: true), Mono(GlassType.Float, 4.0, Coating.None)),
            Seq(new Cavity(new CavityFill.VacuumFill(0.08, PositiveMagnitude.Create(0.25), PositiveMagnitude.Create(20.0)), PositiveMagnitude.Create(0.3))), 0, NoGrid),
        // Fire-rated EI 30: a 6 mm borosilicate outboard pane (the fire side) + a 6 mm float with low-E on surface 3; the
        // positive EI the gate requires of a fire-rated pane drives the OfFire(A1, Ei(30)) lowering.
        new GlazingRow("glazing.fire-ei30-6fr-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.FireRated, 6.0, Coating.None), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble)), Seq(Argon16), 30, NoGrid),
        // True-divided grid: one horizontal + two vertical 25×20 mm muntin bars (manufacturer dims) — face geometry the
        // generator places across the pane.
        new GlazingRow("glazing.double-4-16-4-grid", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None), Mono(GlassType.Float, 4.0, Coating.None)), Seq(Argon16), 0,
            Some(new MuntinGrid(MuntinStyle.TrueDivided, 1, 2, PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(20.0)))));

    // One row -> one ComponentRow: GlazingDetail.Of admits the build ONCE (the bag), Evaluate gates the physics (a build
    // whose spectrum cannot admit never seeds), Plies + Layered.Of rail the geometry (WidthMm = OverallMm preserving the
    // square gross projection), Component.Of seals family/lane/laminate invariants. Substance and appearance both resolve
    // to the outboard pane's glass row (the IGU's engineering receipt rides the material's own property set, so the
    // capacity slot coincides with the appearance row rather than a separate Mechanical key).
    static Fin<ComponentRow> Row(GlazingRow r, Context context) =>
        from bag in GlazingDetail.Of(r.Panes, r.Cavities, r.Spacer, r.EdgeSeal, r.Muntin, r.FireResistanceEiMinutes, context.Key)
        from perf in GlazingThermal.Evaluate(r.Panes, r.Cavities, context.Key)
        from plies in GlazingDetail.Plies(r.Panes, r.Cavities, context.Key)
        let overallMm = r.Panes.Sum(static p => p.ThicknessMm.Value) + r.Cavities.Sum(static c => c.WidthMm.Value)
        from profile in SectionProfile.Layered.Of(plies, overallMm: overallMm, widthMm: overallMm, context.Key)
        from item in Component.Of(
            ComponentFamily.Glazing, r.Designation, profile, IfcBinding.Supertype(ComponentFamily.Glazing.Class),
            Coring.None, IguStandard, substanceId: r.Panes[0].Glass.Appearance, appearanceId: r.Panes[0].Glass.Appearance,
            detail: Some(bag), context.Key)
        select new ComponentRow(item, Sectioned: false);

    // The family fold ComponentFamily.Glazing binds: Traverse is the rail — a malformed build ABORTS the catalogue,
    // never a swallowed Choose drop.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Builds.Traverse(row => Row(row, context)).As();
}
```

## [03]-[RESEARCH]

- [SEED_PARADIGM]: REALIZED — the bespoke `GlazingSection` payload record and its `ComponentSection.Glazing` arm are DELETED; geometry lands as `SectionProfile.Layered(plies, overallMm, widthMm: overallMm)` (the square gross convention preserved by seed authoring), the IGU build inputs land in the `GlazingDetail` `DetailSchema.Product` bag (the seam-declared `PaneBuild`/`CavityBuild`/`SpacerType`/`EdgeSeal`/`MuntinGrid`/`FireResistanceEi` rows, dimension-only `MeasureValue.OfSi` mints), and the seed rows are TYPED (`Pane`/`Cavity`/SmartEnum refs directly — the prior string-keyed `PaneRow`/`CavityRow` sub-rows and their five `TryGet` parse lifts are deleted, an unknown key now unrepresentable); the EN 1279-2 `EdgeSeal` is a per-row column, never a fold-level constant — a structural-glazing build names its silicone secondary in data. `GlazingSeed.Rows : Context -> Fin<Seq<ComponentRow>>` folds through ONE `Traverse` (the prior fault-swallowing outer `Choose` retired); every row is `Sectioned: false`. `GlazingRows` stays a typed AUTHORED table — each EN 1279 build is a distinct engineering unit, never a generator target.
- [EN_673_CENTER_OF_GLASS]: REALIZED — `GlazingThermal.Evaluate` builds ONE ordered series-resistance chain (`he = 23`, `hi = 8 W·m⁻²·K⁻¹` films, per-pane glass-plus-interlayer conductive resistance, per-cavity conductance) the `Ug = 1/ΣR` reads. The cavity conductance dispatches on the `CavityFill` arm: gas sums the convective (Nusselt `Nu = max(1, 0.035·Ra^0.38)` over the §B.2 volume-mixed Rayleigh) and radiative (`h_r = 4·σ·T_m³·(1/ε₁ + 1/ε₂ − 1)⁻¹`) terms; vacuum sums the Collins pillar conduction (`2·λ_glass·a/p²`), the free-molecular residual-gas term (∝ ISO 19916 pressure), and the same radiative term with no convection. The radiative term is the load-bearing physics: a low-E `0.04`/`0.02` corrected emissivity collapses the exchange the uncoated `0.837` dominates, `Coating.None`'s `Option<double>` falling back to the glass `NormalEmissivity` so `Pane.EmissivityOf` is the ONE emissivity source. The `Ug` mints through `QuantityRow.HeatTransferCoefficient.OfSi` — the one typed-mint owner, content keys byte-identical to the prior hand mint.
- [EN_410_SOLAR_OPTICAL]: REALIZED — `g = τe + qi` and `τv` are the multi-layer EN 410 / ISO 9050 projection over per-pane directional `(τ, ρ_front, ρ_back)` optics (a coated face's reflectance asymmetric front-to-back): panes combine left-to-right through the two-flux `Combine` recursion, per-pane absorptance follows from outer/inner sub-stack inter-reflection, and `qi = Σ αe,i·R_out,i/R_tot` partitions each pane's absorbed energy by the SAME resistance chain the `Ug` reads. `g`/`τv` are dimensionless `Ratio` `MeasureValue`s (`RatioUnit.DecimalFraction`, the seam `IsDimensionless` path — content keys frozen, so no `QuantityRow` re-route); `GlazingPerformance.LightToSolarGain` is the derived NFRC `τv/g` selection ratio over the two stored measures. The per-wavelength `τ(λ)`/`ρ(λ)` angular spectral ray-trace is a `GlassType`/`Coating` spectral-curve column growth, never a parallel optical owner.
- [GLAZING_ACOUSTIC_RW]: REALIZED — `GlazingThermal.MassLawSpectrum`: the field-incidence mass law `R(f) = 20·log₁₀(m'·f) − 47 dB` over the total areal mass at the eighteen `AcousticBand` centres, with the best interlayer's typed coincidence-dip damping bonus (`Interlayer.AcousticDampingDb`: PVB 3, EVA 2.5, SGP 2 dB) and the asymmetric-pane bonus; the `Rw` is the seam `RatingContour.Rw.Fit` read so the IGU rating and an assembly rating share ONE contour fit. The spectral coincidence-frequency correction is a `Rasm.Compute` refinement, never a parallel acoustic owner.
- [MATERIAL_PROPERTY_LOWERING]: REALIZED — `GlazingDetail.Properties` lowers the computed receipt into the seam set so the IGU material "has it all": `Thermal` the EN 673 Ug + the glass-only homogenized columns each under its own physical mixing law (conductivity the series-harmonic `Σt/Σ(t/λ)` slab mean, specific heat the mass-weighted `Σ(ρ·t·c)/Σ(ρ·t)` mean — EN 572-1 `720`/`830 J·kg⁻¹·K⁻¹`) + the vapour-tight μ; `Acoustic` the banded spectrum; `Environmental` the substance/process-split per-m² GWP under the `PropertyEvidence` EPD row (`GenericEpd` — the seam's evidence axis; the deleted per-case `epd`/`validUntilYear` columns never re-enter); `Fire` the parameterized EN 13501-2 `FireResistance.Ei(minutes)` where the row carries positive EI. `Properties` composes the SAME `Admit` gate the seed door runs, so the projector ingress never feeds the kernel an unadmitted stack. The `Projection/component#COMPONENT_PROJECTOR` composes it for the IGU material and lowers the Product bag beside it; receipts are re-derived deterministically from the bag-carried inputs, never stored scalars that drift.
- [GWP_SUBSTANCE_PROCESS_SPLIT]: REALIZED — `GlazingGwp.StagesPerM2` splits A1-A3 into RAW SUBSTANCE (pane and interlayer mass × per-kg base — soda-lime `1.43`, low-iron `1.50`, borosilicate `2.00` kgCO2e/kg) and SECONDARY PROCESSING (per-m² thermal-form, coating-sputter, lamination, IGU-assembly adders) so the per-kg base is never double-counted; A4-D scale A1-A3 over the EN 15978 `LifecycleStage` banding, a negative D the recovery benefit, `recycledContent 0.25` / `endOfLifeRecovery 0.90` the EN 15804 generic-IGU rows.
- [VIG_VACUUM_ISO_19916]: REALIZED — a vacuum unit is a `CavityFill.VacuumFill` arm carrying the ISO 19916 residual pressure (gated below the `0.1 Pa` functional-vacuum ceiling in `GlazingDetail.Of`) and the support-pillar radius/pitch; the conductance is the Collins pillar term plus the free-molecular residual-gas term plus the radiative term the low-E coating suppresses, no convective term — the `4lowe-vac-4` unit computes the order-of-magnitude lower `Ug` a 0.3 mm vacuum gap delivers that no gas fill of the same gap can.
- [IFCMATERIALLAYERSET_GLAZING_ALIGNMENT]: REALIZED — glazing is the one family that is an `IfcMaterialLayerSet` rather than an `IfcProfileDef`: every seed row is `Sectioned: false`, `graph.SectionOf` dereferences a glazing `ComponentId` to `(Component, None)`, and `SectionSolver.Solve` faults loudly on a mis-flagged `Layered` arm. The `Layered` plies ARE the layer geometry (pane / cavity / pane; a laminated pane the glass-interlayer-glass sub-plies; a vacuum cavity an `IsVentilated`-false sealed gap at the Bim edge), each `Ply.Role` the human-readable `IfcMaterialLayer.Name` — the `Projection/component#COMPOSITION_AUTHOR` `LayerSet` bridge reads them directly, so the prior `ToLayerSet` layer-row builder is subsumed by the profile geometry itself. The low-E `Coating` rides the coated pane's `Node.Appearance`, never a fabricated `glass.lowe` bulk row; the `MuntinGrid` is FACE geometry, never a ply. Ripple counterpart: `Projection/component.md` (the bag lowering + the `CompositionAuthor` ply read + the `GlazingDetail.Properties` composition).
- [WHOLE_WINDOW_U_IS_COMPUTE]: `Ug` is the EN 673 CENTER-OF-GLASS value glazing OWNS and lowers onto the seam `Thermal.UValue`. The EN ISO 10077-1 whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` incorporating `SpacerType.PsiWmK` and the frame fraction is a `Rasm.Compute` ASSEMBLY concern (`AssemblyAggregator.AggregateWindow` reads `Ug` off the seam `Thermal.UValue`, `Ψg` off the glazing receipt, frame `Uf` + areas off the window's parts); face dimensions are OCCURRENCE geometry, never a type column. Ripple counterpart: `Rasm.Compute/Analysis/aggregator` + `Rasm.Compute/Analysis/physics` (the thermal runner's window branch).
