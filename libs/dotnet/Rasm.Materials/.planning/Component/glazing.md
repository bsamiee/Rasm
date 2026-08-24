# [MATERIALS_GLAZING]

THE GLAZING SEED PAGE — the `glazing` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Product`) grounded in insulating-glass build physics. An IGU is a `Component` whose `SectionProfile.Layered` geometry contains only `PlyRole.Pane`/`Interlayer`/`Cavity`, whose build inputs ride the `DetailSchema.Product` bag, and whose engineering performance derives from the typed `GlazingRow`: `GlazingThermal` owns EN 673 `Ug`, EN 410 / ISO 9050 `g` and `τv`, and the mass-law acoustic spectrum; `GlazingStructural` owns the EN 16612 pane resistance the capacity rail lifts; `GlazingLifetime` owns the EN 1279-3 gas-decay and EN ISO 13788 `fRsi` service receipt; `GlazingGwp` owns the lifecycle vector; `GlazingDetail.Properties` lowers the receipt to `MaterialPropertySet`. An IGU crosses as `IfcMaterialLayerSet`, never `IfcProfileDef`, so its `Layered` profile answers unsectioned membership from its own `ProfileTopology`. `GlazingSeed.Resolve` joins a resolved `ComponentId` back to its pane, cavity, edge, grid, and fire axes so the projector can execute the promised lowering without parsing the bag or the designation.

The ONE stack-admission law is `GlazingDetail.Stack`, an ACCUMULATING census: arity, modeled pane count, EI sign, per-pane interlayer exactness, and per-cavity fill sanity are five INDEPENDENT proofs, so a malformed build names every column it broke in one verdict. The seed lifts it as its `SeedLaw` coherence (where it now spans the whole roster rather than aborting on the first bad build) and the capacity and service doors lift the same census on the `Fin` rail — one law, three ingresses, no re-admission anywhere.

## [01]-[INDEX]

- [02]-[GLAZING_FAMILY]: the glazing policy vocabularies, `CavityFill`, the typed build rows, the shared `GlazingThermal` resistance/optical/acoustic kernel, `GlazingPerformance`, the `GlazingStructural` EN 16612 pane-resistance kernel with its `GlassCapacity` receipt, the `GlazingLifetime` service receipt, `GlazingGwp`, `GlazingDetail`, and the `GlazingSeed` roster with its seed law and `SeedJoin` resolver.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing policy vocabulary; `CavityFill` the gas-vs-vacuum `[Union]`; `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` the typed stack rows; `GlazingThermal` the shared resistance, optical, and acoustic kernel; `GlazingGwp` the lifecycle vector; `GlazingPerformance` the computed receipt; `GlazingDetail` the shared stack census, bag, property, and ply operations; `GlazingSeed` the EN 1279 roster, its seed law, and the typed resolver.
- Cases: the glazing vocabulary spans the glass, per-face coating, gas, interlayer, spacer, and edge-seal axes. `GlazingBuild` derives `Double`, `Triple`, or `Quadruple` from `Panes.Count`; stack arity and finite pane/cavity values admit before either physics boundary runs.
- Entry: `ComponentSeed.Rows(context, GlazingSeed.Roster, GlazingSeed.Law)` — the law's coherence is the stack census plus the grid gate, its profile the ply projection onto `Layered`, and its detail the performance gate before the Product bag, so a build whose spectrum cannot admit never seeds and one malformed row aborts the catalogue. `GlazingSeed.Resolve(Component, Op)` restores the typed build axes through the shared `SeedJoin` rail. `GlazingDetail.Properties(panes, cavities, ei, key, serviceYears)` lowers `Thermal`/`Acoustic`/`Environmental`/`Fire` as one rail AT the declared service age (`None` reads year zero).
- Packages: Rasm.Numerics (`PositiveMagnitude` — every pane/gap/pillar/bar column), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `MaterialPropertySet`, `MeasureValue`, `Dimension`, `MeasurementBasis`, `LifecycleStage`, `Acoustic`, `AcousticBand`, `FireRating`, `FireResistance`, `DetailSchema`, `PropertyValue`, `PropertyName`, `PropertyBag`), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED`/`#QUANTITY_ROW` owners, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime (`LocalDate` — the EPD evidence expiry axis), UnitsNet (`RatioUnit.DecimalFraction` — the dimensionless `g`/`τv` seam admission), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (`WithRelativeUncertainty`, `IUncertainty<HeatTransferCoefficient>.LowerBound`/`UpperBound` — the typed `Ug` model band lowered onto `MeasureBand`). VividOrange.Materials is NOT composed (glazing fills no profile solve). Wacton.Unicolour is NOT composed: a coating's OPTICAL signal crosses as the coated pane's content-keyed `Node.Appearance`; glazing tags the `MaterialId`, never the colour kernel.
- Growth: a new IGU is one `GlazingRow`; a new glass substance one `GlassType` row; a new coating tier one `Coating` row; a new gas one `CavityGas` row; a new interlayer one `Interlayer` row; a new edge-seal chemistry one `Sealant`/`Desiccant` row; a quad build one `GlazingBuild` row the derived `Build` read maps; an electrochromic variant a `GlassType` row plus a `Coating` row. The full per-wavelength `τ(λ)`/`ρ(λ)` angular EN 410 §5 spectral integral is a `GlassType`/`Coating` per-wavelength-curve column growth the broadband recursion here is the center-of-glass simplification of, never a parallel optical owner.
- Boundary: `SectionProfile.Layered` is the geometric gross only; `ComponentFamily.Glazing.Admits` rejects every non-glazing `PlyRole`, and physics reads the typed `Pane`/`Cavity` rows restored through `GlazingSeed.Resolve`, never re-parsed plies or bag text. `GlazingThermal.Evaluate` is INTERIOR over a census-gated stack and computes one ordered resistance chain shared by `Ug` and the EN 410 inward-flowing secondary flux. `QuantityRow.HeatTransferCoefficient.OfNative` owns the `Ug` mint, while dimension-only bag rows use `MeasureValue.OfSi(Dimension, si)`. `SpacerType.PsiWmK` feeds the Compute-owned whole-window aggregation. The IFC layer name derives from `(Material, Role, ordinal)`, coating stays face data, and `MuntinGrid` stays face geometry.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using UnitsNet;
using VividOrange.Uncertainties;
using VividOrange.Uncertainties.Quantities.Utility;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;
using static Rasm.Materials.Component.ComponentDetail;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The glass-substance axis: uncoated NORMAL EMISSIVITY (soda-lime εn 0.837, the EN 673 Annex-A baseline the cavity
// radiative term reads absent a coating), CONDUCTIVITY, EN 572-1 DENSITY and SPECIFIC HEAT, EN 15804 RAW-SUBSTANCE
// GWP-per-kg (cradle-to-gate pane substance ONLY — secondary process carbon is the per-m² adders, never
// double-counted into this base), thermal-FORM process GWP-per-m², broadband EN 410 / ISO 9050 SOLAR and VISIBLE
// transmittance and reflectance, the characteristic bending strength the EN 16612 pane resistance reads, and the
// safety class. All columns PUBLISHED (EN 572-1 / EN 673 Annex A / EN 15804 generic); a laminated pane is any glass
// plus an Interlayer, not a case.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassType {
    public static readonly GlassType Float            = new("float",             normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 1.00, strengtheningFactor: None,       safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType LowIron          = new("low-iron",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.50, formProcessGwpPerM2: 0.0, solarTransmittance: 0.90, solarReflectance: 0.080, visibleTransmittance: 0.91, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 1.00, strengtheningFactor: None,       safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType Patterned        = new("patterned",         normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.78, solarReflectance: 0.075, visibleTransmittance: 0.85, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 0.75, strengtheningFactor: None,       safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType HeatStrengthened = new("heat-strengthened", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.9, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 70.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: false, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType Tempered         = new("tempered",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: true, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType TemperedVertical = new("tempered-vertical", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(0.6), safety: true, appearance: MaterialId.Of("glass.crown"));
    public static readonly GlassType Borosilicate     = new("borosilicate",      normalEmissivity: 0.837, conductivityWmK: 1.14, densityKgM3: 2230.0, specificHeatJKgK: 830.0, substanceGwpPerKg: 2.00, formProcessGwpPerM2: 5.0, solarTransmittance: 0.70, solarReflectance: 0.070, visibleTransmittance: 0.85, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: true, appearance: MaterialId.Of("glass.flint"));
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
    public double CharacteristicBendingMpa { get; }   // fb,k — annealed 45 (EN 572-1), heat-strengthened 70 (EN 1863), fully-tempered/toughened-borosilicate 120 (EN 12150/EN 13024)
    // EN 16612 Table 4 k_sp, the SURFACE PROFILE factor: 1.00 for float and drawn sheet as produced, 0.75 for
    // patterned — frozen at unity it priced a patterned pane, whose 25% profile penalty is exactly what the factor
    // exists to carry, as though it were float.
    public double SurfaceProfileFactor { get; }
    // EN 16612 Table 7 k_v, the STRENGTHENING factor, present only on a PRESTRESSED glass: 1.0 where the process uses
    // no tongs or holding devices (horizontal toughening) and 0.6 where it does (vertical). Absence is what makes an
    // ANNEALED glass annealed — the prestress term does not apply to it at all rather than applying at unity, and
    // the two are different equations.
    public Option<double> StrengtheningFactor { get; }
    public bool Safety { get; }   // EN 12600 safety classification, the submittal datum a specification reads

    // The library appearance ROW COLUMN each pane shades to (clear crown; the heavier flint for borosilicate). A
    // low-E/solar-control COATING is a thin-film surface effect, NOT a bulk shade — it rides the coated pane's
    // Node.Appearance, so this column takes no Coating knob. The SUBSTANCE is the separate slot the property
    // catalogue keys on, derived from the row's own key so a new glass names one row and gets both identities.
    public MaterialId Appearance { get; }
    public MaterialId Substance => MaterialId.Of($"glass.{Key}");
}

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
// DENSITY ρ, dynamic VISCOSITY μ, SPECIFIC HEAT c — so the convective Nusselt/Rayleigh term reads a typed gas
// receipt. A mixture is the volume-weighted blend of the fill gas and the CavityFill.GasFill balance gas.
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

// The lamination interlayer axis (EN 14449 / EN 12543): NOMINAL single-ply thickness, ACOUSTIC coincidence-dip
// DAMPING bonus, SHEAR MODULUS (the structural stiffness the laminate transfers), CONDUCTIVITY, DENSITY, substance
// GWP-per-kg, lamination process GWP-per-m². None is the monolithic pane.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Interlayer {
    public static readonly Interlayer None = new("none", nominalPlyMm: 0.0,  acousticDampingDb: 0.0, shearModulusMPa: 0.0,   conductivityWmK: 1.00, densityKgM3: 0.0,    substanceGwpPerKg: 0.0, processGwpPerM2: 0.0, omega: 0.0, omegaSource: EvidenceGrade.Defined);
    public static readonly Interlayer Pvb  = new("pvb",  nominalPlyMm: 0.38, acousticDampingDb: 3.0, shearModulusMPa: 2.0,   conductivityWmK: 0.20, densityKgM3: 1070.0, substanceGwpPerKg: 3.40, processGwpPerM2: 1.5, omega: 0.0, omegaSource: EvidenceGrade.User);
    public static readonly Interlayer Sgp  = new("sgp",  nominalPlyMm: 0.89, acousticDampingDb: 2.0, shearModulusMPa: 110.0, conductivityWmK: 0.20, densityKgM3: 950.0,  substanceGwpPerKg: 4.20, processGwpPerM2: 2.0, omega: 0.0, omegaSource: EvidenceGrade.User);
    public static readonly Interlayer Eva  = new("eva",  nominalPlyMm: 0.38, acousticDampingDb: 2.5, shearModulusMPa: 8.0,   conductivityWmK: 0.23, densityKgM3: 950.0,  substanceGwpPerKg: 2.90, processGwpPerM2: 1.4, omega: 0.0, omegaSource: EvidenceGrade.User);
    public double NominalPlyMm { get; }
    public double AcousticDampingDb { get; }
    public double ShearModulusMPa { get; }
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double SubstanceGwpPerKg { get; }
    public double ProcessGwpPerM2 { get; }
    // The EN 16613 shear-transfer coefficient ω the effective-thickness formula weights the sandwich term by: 0 is
    // the NO-SHEAR lower bound, 1 the fully-coupled monolithic upper bound. It is the estate's OWN value at 0 on
    // every real interlayer and the evidence column says so — the published ω family tables are load-duration ×
    // temperature grids this estate does not hold, and crediting shear coupling a family has not declared over-states
    // a laminate's resistance by up to the ratio of the monolithic to the cube-sum thickness. A declared family fills
    // omega and its grade rises to Catalogue with no formula edit: EffectiveThicknessMm already reads the column.
    public double Omega { get; }
    public EvidenceGrade OmegaSource { get; }
}

// The EN 1279-2 edge-seal sealant: the primary moisture barrier (PIB butyl) and the structural/durability secondary
// seal (silicone is structural-glazing-rated; polysulfide and hot-melt-butyl are not), each with its seal
// GWP-per-perimeter-metre.
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

// The EN 1279-2 spacer desiccant, carrying its water-adsorption capacity the durability/dew-point reserve reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Desiccant {
    public static readonly Desiccant MolecularSieve3A = new("molecular-sieve-3a", adsorptionCapacity: 0.22);
    public static readonly Desiccant Silica           = new("silica",             adsorptionCapacity: 0.30);

    // The published typical capacity at standard conditions as a mass fraction — the 3A sieve's 20-24 g per 100 g
    // band read at its midpoint, silica's own figure beside it.
    public double AdsorptionCapacity { get; }

    // The EN 1279-2 STANDARD adsorption capacity Tc — the reserve the moisture-penetration index divides by, measured
    // under the clause's own limit environment rather than at the ambient conditions the typical figure is quoted at.
    // It is a DIFFERENT number and it is optional because the standard's own value is desiccant-PRODUCT specific: two
    // sieves both sold as 3A differ in bead size, binder fraction, and activation, and the index is sensitive to
    // exactly that. Until a product declares one, the typical capacity is the stated conservative stand-in.
    public Option<double> StandardCapacity { get; }

    public double CapacityFraction => StandardCapacity.IfNone(AdsorptionCapacity);
}

// Bar dimensions are MANUFACTURER values (no EN/ASTM table grounds them), captured on the MuntinGrid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MuntinStyle {
    public static readonly MuntinStyle TrueDivided      = new("true-divided");
    public static readonly MuntinStyle SimulatedDivided = new("simulated-divided");
    public static readonly MuntinStyle BetweenGlass     = new("between-glass");
}

// The edge-seal spacer axis: the EN ISO 10077-1 linear thermal-bridge Ψg, the SIGHT-LINE width, the spacer-frame
// CONDUCTIVITY, and the spacer+seal fabrication GWP-per-perimeter-metre. Spacer DEPTH is the cavity gap (read from
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityTilt {
    public static readonly CavityTilt Vertical      = new("vertical",       coefficient: 0.035, exponent: 0.38, convects: true);
    public static readonly CavityTilt Inclined45    = new("inclined-45",    coefficient: 0.100, exponent: 0.31, convects: true);
    public static readonly CavityTilt HorizontalUp  = new("horizontal-up",  coefficient: 0.160, exponent: 0.28, convects: true);
    public static readonly CavityTilt DownwardFlow  = new("downward-flow",  coefficient: 0.000, exponent: 0.00, convects: false);
    public double Coefficient { get; }
    public double Exponent { get; }
    public bool Convects { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlazingBuild {
    public static readonly GlazingBuild Double = new("double", panes: 2);
    public static readonly GlazingBuild Triple = new("triple", panes: 3);
    public static readonly GlazingBuild Quadruple = new("quadruple", panes: 4);
    public int Panes { get; }

    public static Option<GlazingBuild> OfPaneCount(int panes) =>
        Items.FirstOrDefault(b => b.Panes == panes) is { } build ? Some(build) : None;
}

// The cavity fill discriminant: a GAS fill (the EN 673 mixture) or a VACUUM fill (the ISO 19916 VIG — residual
// pressure + the support-pillar geometry the Collins pillar-conduction model reads). The kernel dispatches the
// cavity conductance on the arm: gas convects (Nusselt) and radiates; vacuum conducts through pillars and the
// free-molecular residual gas and radiates with no convection.
[Union]
public abstract partial record CavityFill {
    public sealed record GasFill(CavityGas Gas, double FillFraction, CavityGas Balance) : CavityFill;
    public sealed record VacuumFill(double ResidualPressurePa, PositiveMagnitude PillarRadiusMm, PositiveMagnitude PillarPitchMm) : CavityFill;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct GasProperties(double ConductivityWmK, double DensityKgM3, double ViscosityPaS, double SpecificHeatJKgK);

// One pane in the IGU stack: glass substance, TOTAL thickness, ONE Coating state PER PHYSICAL FACE (outboard the
// exterior-facing face, inboard the interior-facing face; Coating.None the uncoated state — a dual-coated pane
// carries two independent rows, and a mis-transcribed surface index is unrepresentable rather than silently
// uncoated), and an Interlayer with its total thickness. Glass-only = total − interlayer.
public readonly record struct Pane(GlassType Glass, PositiveMagnitude ThicknessMm, Coating OutboardCoating, Coating InboardCoating, Interlayer Interlayer, double InterlayerThicknessMm) {
    public bool IsLaminated => Interlayer != Interlayer.None && InterlayerThicknessMm > 0.0;
    public double GlassThicknessMm => ThicknessMm.Value - InterlayerThicknessMm;

    public double EmissivityOf(bool inboard) =>
        (inboard ? InboardCoating : OutboardCoating).CorrectedEmissivity.IfNone(Glass.NormalEmissivity);

    public (double T, double Rf, double Rb) Solar() => (
        Glass.SolarTransmittance * OutboardCoating.SolarTransmittanceMultiplier * InboardCoating.SolarTransmittanceMultiplier,
        OutboardCoating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance),
        InboardCoating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance));

    public (double T, double Rf, double Rb) Visible() => (
        Glass.VisibleTransmittance * OutboardCoating.VisibleTransmittanceMultiplier * InboardCoating.VisibleTransmittanceMultiplier,
        OutboardCoating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance),
        InboardCoating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance));
}

public readonly record struct Cavity(CavityFill Fill, PositiveMagnitude WidthMm);

// The EN 1279-2 moisture-penetration index — the one durability quantity deciding whether an edge seal keeps its
// cavity dry, and a MEASURED result rather than a catalogue column. The index reads as the share of the desiccant's
// drying reserve that standardized ageing consumed: Ti the as-filled moisture content, Tf the content after the
// climate test, Tc the standard adsorption capacity — so the capacity, not the final content, is the denominator's
// endpoint and an index above one would mean ageing outran the reserve entirely. Every input is a per-SYSTEM
// observation, which is why a seeded build carries ABSENCE: the standard states outright that comparing indices
// across unit systems is meaningless, so there is no per-system table to transcribe.
public readonly record struct MoisturePenetration(double InitialFraction, double FinalFraction, double CapacityFraction) {
    public const double AverageCeiling = 0.20;      // the aged specimen set's mean index
    public const double IndividualCeiling = 0.25;   // the worst single unit in that set

    public double Index => (FinalFraction - InitialFraction) / (CapacityFraction - InitialFraction);
    public bool Conforms => Index <= IndividualCeiling;

    // Admission is the ordering the index depends on: the reserve must be a real interval (Ti strictly below Tc) or
    // the quotient is a division by a vanishing or negated denominator reported as a durability number. The capacity
    // endpoint reads off the DESICCANT rather than arriving as a caller scalar — the reserve is a property of the
    // material in the spacer, and a caller-stated one invites an index divided by a number nothing declares.
    public static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, Desiccant desiccant, Op key) =>
        Of(initialFraction, finalFraction, desiccant.CapacityFraction, key);

    static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, double capacityFraction, Op key) =>
        from finite in guard(double.IsFinite(initialFraction) && double.IsFinite(finalFraction) && double.IsFinite(capacityFraction),
            new KernelFault.InvalidValue(nameof(MoisturePenetration), "finite fractions", Some(key)))
        from ordered in guard(initialFraction >= 0.0 && initialFraction < capacityFraction && finalFraction >= initialFraction,
            new KernelFault.InvalidValue(nameof(MoisturePenetration), "ordered non-negative fractions below capacity", Some(key)))
        select new MoisturePenetration(initialFraction, finalFraction, capacityFraction);
}

// The EN 1279-2 edge-seal construction: primary moisture sealant (PIB), structural/durability secondary sealant,
// spacer desiccant, keyed-vs-bent corners, and the moisture-penetration index its system carries once a climate test
// has measured one.
public readonly record struct EdgeSeal(Sealant Primary, Sealant Secondary, Desiccant Desiccant, bool CorneredKeys, Option<MoisturePenetration> Moisture);

public readonly record struct MuntinGrid(MuntinStyle Style, int HorizontalBars, int VerticalBars, PositiveMagnitude BarWidthMm, PositiveMagnitude BarDepthMm);

public readonly record struct GlazingPerformance(
    MeasureValue UgCenterOfGlass,
    MeasureValue SolarFactorG,
    MeasureValue LightTransmittanceTv,
    Acoustic Acoustic,
    EvidenceGrade AcousticSource) {
    public int Rw => Acoustic.Rw;

    // The NFRC light-to-solar-gain selection ratio LSG = τv/g — a derived read over the two stored measures (the
    // GoverningRadiusMm pattern), listed beside Ug/g/τv on every IGU datasheet; an opaque build reads 0.
    public double LightToSolarGain => SolarFactorG.Si > 0.0 ? LightTransmittanceTv.Si / SolarFactorG.Si : 0.0;
}

// One EN 1279 IGU build: the designation, the spacer, the edge-seal construction (a structural-glazing build names
// its silicone secondary here), the TYPED pane/cavity sub-rows (SmartEnum refs and PositiveMagnitude literals
// directly — no string re-parse, an unknown key unrepresentable), the EN 13501-2 EI minutes (0 absent a fire-rated
// pane), the optional face grid, and the row's own evidence grade. Each build is a distinct engineering unit — a
// roster row, never a generator target — and the BUILD is this estate's own composition even though every column it
// composes is a published datum, which is what keeps AdmitImported from handing a vendor IGU type this geometry as
// though a standards body had published the unit.
public readonly record struct GlazingRow(string Designation, SpacerType Spacer, EdgeSeal EdgeSeal, Seq<Pane> Panes, Seq<Cavity> Cavities, int FireResistanceEiMinutes, Option<MuntinGrid> Muntin) {
    public EvidenceGrade Source { get; init; } = EvidenceGrade.User;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 673 center-of-glass U-value + EN 410 / ISO 9050 net-g/τv projection + mass-law Acoustic spectrum — the
// glazing family's domain-physics owner. Evaluate computes ONE ordered series-resistance chain (surface films,
// per-pane conductive resistance, per-cavity conductance) that BOTH the Ug (1/ΣR) AND the EN 410 secondary heat flux
// qi read, so the optical and thermal kernels share the resistance network rather than re-deriving it. Evaluate is
// INTERIOR over a census-gated stack — every ingress door runs GlazingDetail.Stack first, so the position indexing
// never sees a malformed arity. KERNEL EXEMPTION: the indexed resistance-array and band-array loops are the measured
// numeric kernel this page names — the chain is position-indexed (pane i faces cavity i and i−1), so the fold state
// IS the index.
public static class GlazingThermal {
    const double SurfaceExternalWmK = 23.0;        // EN 673 external surface coefficient he (W·m⁻²·K⁻¹)
    const double SurfaceInternalWmK = 8.0;         // EN 673 internal surface coefficient hi
    const double StefanBoltzmann = 5.67e-8;        // σ (W·m⁻²·K⁻⁴)
    const double MeanTemperatureK = 283.0;         // EN 673 mean cavity temperature (10 °C)
    const double TemperatureDeltaK = 15.0;         // EN 673 reference ΔT across the cavity
    const double GravityMs2 = 9.81;
    const double MassLawOffsetDb = 47.0;           // field-incidence mass-law offset R = 20·log₁₀(m'·f) − 47
    const double FreeMolecularConductanceAirPerPa = 1.2;   // free-molecular (Knudsen-regime) air conduction W·m⁻²·K⁻¹·Pa⁻¹ — the VIG residual-gas term
    const double ThermalModelRelativeUncertainty = 0.05;

    // Tilt is an INSTALLATION condition, not a build column: the same insulating unit glazed vertically in a facade
    // and at a slope in a rooflight has two different cavity conductances, and the difference belongs to the opening.
    // The seed evaluates at EN 673's own standardized vertical reference; a placed unit supplies its real tilt.
    internal static Fin<GlazingPerformance> Evaluate(Seq<Pane> panes, Seq<Cavity> cavities, CavityTilt tilt, Op key) {
        double[] rPane = panes.Map(PaneConductiveResistance).ToArray();
        double[] rCav = new double[cavities.Count];
        for (int i = 0; i < cavities.Count; i++) rCav[i] = 1.0 / CavityConductance(panes, cavities, i, tilt);
        double rse = 1.0 / SurfaceExternalWmK, rsi = 1.0 / SurfaceInternalWmK;
        double rTot = rse + rPane.Sum() + rCav.Sum() + rsi;
        double ug = 1.0 / rTot;
        double g = SolarFactor(panes, rPane, rCav, rTot, rse);
        double tv = Span(panes, 0, panes.Count, static p => p.Visible()).T;
        // g/τv admit as dimensionless Ratio measures (the seam IsDimensionless path — no SI reprojection, content
        // keys frozen); the Ug typed mint routes through QuantityRow.HeatTransferCoefficient, and the model band
        // rides the seam's PUBLIC MeasureBand.Admit + WithUncertainty rail, so a band excluding the nominal faults
        // typed instead of minting silently.
        HeatTransferCoefficient ugQuantity = HeatTransferCoefficient.FromWattsPerSquareMeterKelvin(ug);
        IUncertainty<HeatTransferCoefficient> ugUncertainty = ugQuantity.WithRelativeUncertainty(ThermalModelRelativeUncertainty);
        return from ugMeasure in QuantityRow.HeatTransferCoefficient.OfNative(ug)
               from ugBand in MeasureBand.Admit(UncertaintyKind.Relative,
                   ugUncertainty.LowerBound.WattsPerSquareMeterKelvin, ugUncertainty.UpperBound.WattsPerSquareMeterKelvin,
                   Option<double>.None, Option<double>.None, key)
               from ugBanded in ugMeasure.WithUncertainty(ugBand, key)
               from acoustic in MassLawSpectrum(panes, cavities, key)
               from solarG in MeasureValue.Of(g, UnitsNet.Units.RatioUnit.DecimalFraction, key)
               from lightTv in MeasureValue.Of(tv, UnitsNet.Units.RatioUnit.DecimalFraction, key)
               select new GlazingPerformance(ugBanded, solarG, lightTv, acoustic, EvidenceGrade.Defined);
    }

    static double PaneConductiveResistance(Pane p) =>
        (p.GlassThicknessMm / 1000.0) / p.Glass.ConductivityWmK + (p.InterlayerThicknessMm / 1000.0) / p.Interlayer.ConductivityWmK;

    // Each cavity's total conductance h_total dispatched on the CavityFill arm. The cavity sees the INBOARD face of
    // pane i and the OUTBOARD face of pane i+1: a low-E coating lowers h_rad only when it sits on one of these two
    // cavity-facing faces. A vacuum cavity conducts through the Collins pillar array (2·λ_glass·a/p² over the two
    // bounding panes' mean conductivity) and the free-molecular residual gas, and radiates with no convection.
    static double CavityConductance(Seq<Pane> panes, Seq<Cavity> cavities, int i, CavityTilt tilt) {
        Cavity cavity = cavities[i];
        double hRad = RadiativeCoefficient(panes[i].EmissivityOf(inboard: true), panes[i + 1].EmissivityOf(inboard: false));
        double s = cavity.WidthMm.Value / 1000.0;
        return cavity.Fill.Switch(
            state: (Panes: panes, Index: i, HRad: hRad, GapM: s, Tilt: tilt),
            gasFill: static (x, gas) => {
                GasProperties p = EffectiveGas(gas);
                return Nusselt(p, x.GapM, x.Tilt) * p.ConductivityWmK / x.GapM + x.HRad;
            },
            vacuumFill: static (x, vac) => {
                double kGlass = 0.5 * (x.Panes[x.Index].Glass.ConductivityWmK + x.Panes[x.Index + 1].Glass.ConductivityWmK);
                double hPillar = 2.0 * kGlass * (vac.PillarRadiusMm.Value / 1000.0) / Math.Pow(vac.PillarPitchMm.Value / 1000.0, 2.0);
                return hPillar + FreeMolecularConductanceAirPerPa * vac.ResidualPressurePa + x.HRad;
            });
    }

    // The EN 673 §B.2 volume-fraction gas mixture, each of the four properties linearly volume-weighted.
    static GasProperties EffectiveGas(CavityFill.GasFill gas) => new(
        Mix(gas.Gas.ConductivityWmK, gas.Balance.ConductivityWmK, gas.FillFraction),
        Mix(gas.Gas.DensityKgM3, gas.Balance.DensityKgM3, gas.FillFraction),
        Mix(gas.Gas.ViscosityPaS, gas.Balance.ViscosityPaS, gas.FillFraction),
        Mix(gas.Gas.SpecificHeatJKgK, gas.Balance.SpecificHeatJKgK, gas.FillFraction));

    static double Mix(double fill, double balance, double x) => x * fill + (1.0 - x) * balance;

    // EN 673 radiative coefficient h_r = 4·σ·T_m³ / (1/ε₁ + 1/ε₂ − 1): a single low-E 0.04 surface collapses the
    // uncoated 0.837/0.837 exchange by an order of magnitude, which is the entire reason a coated cavity outperforms
    // an uncoated one of the same gas and gap.
    static double RadiativeCoefficient(double e1, double e2) =>
        4.0 * StefanBoltzmann * MeanTemperatureK * MeanTemperatureK * MeanTemperatureK / (1.0 / e1 + 1.0 / e2 - 1.0);

    // EN 673 Nusselt number Nu = max(1, A·Ra^n) over the Rayleigh number Ra = ρ²·s³·g·c·ΔT / (T_m·μ·λ). A and n are
    // INCLINATION-dependent: an inclined or horizontal cavity with UPWARD heat flow convects harder than a vertical
    // one at the same Rayleigh number, so a sloped rooflight priced on the vertical constants under-reads its own
    // conductance, and a cavity with DOWNWARD heat flow does not convect at all — Nu is 1 by the physics, not by
    // falling under a threshold. The tilt selects a ROW; an intermediate angle interpolates between the two nearest,
    // which is the standard's own instruction.
    static double Nusselt(GasProperties gas, double s, CavityTilt tilt) {
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return tilt.Convects ? Math.Max(1.0, tilt.Coefficient * Math.Pow(ra, tilt.Exponent)) : 1.0;
    }

    // The EN 410 / ISO 9050 net solar factor g = τe + qi: the multi-layer transmittance τe plus the secondary
    // internal heat flux qi — each pane's absorptance αe,i times its inward-flowing fraction R_out,i/R_tot, the
    // inward fraction being the resistance from the outer environment to the pane centre over the total resistance
    // the SHARED chain already computed (absorbed heat flows inward in proportion to the resistance to the other side).
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
    // outer sub-stack transmittance over the multiple-reflection denominator between the outer back reflectance and
    // the [pane j ⊕ inner] front reflectance) drives the front-incidence absorptance, plus the part transmitted
    // through j and reflected back by the inner sub-stack drives the back-incidence absorptance.
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

    static (double T, double Rf, double Rb) Combine((double T, double Rf, double Rb) a, (double T, double Rf, double Rb) b) {
        double d = 1.0 - a.Rb * b.Rf;
        return (a.T * b.T / d, a.Rf + a.T * a.T * b.Rf / d, b.Rb + b.T * b.T * a.Rb / d);
    }

    static (double T, double Rf, double Rb) Span(Seq<Pane> panes, int lo, int hi, Func<Pane, (double T, double Rf, double Rb)> optics) =>
        panes.Skip(lo).Take(hi - lo).Fold((T: 1.0, Rf: 0.0, Rb: 0.0), (acc, pane) => Combine(acc, optics(pane)));

    // The field-incidence mass-law acoustic spectrum. This is an ESTIMATION MODEL and it is the one number this page
    // produces that is not derived from published data: there is NO standardized calculation method for the sound
    // reduction index of a glazing product. The product standard admits exactly three routes to an R_w — a laboratory
    // measurement, its own table of conservatively measured typical values, and its extension rules — and the
    // industry guidance behind it states that a mathematical determination from surface weight is neither correct nor
    // permitted for declaration. The published typical values prove why: a symmetric 8-8 unit rates BELOW both 8-4
    // and 8-6, because equal leaves put both coincidence dips at the same frequency, and no mass-law estimator
    // reproduces that inversion. The receipt therefore records the acoustic column as MODEL-DERIVED, so a consumer
    // reading the seam sees an estimate labelled as one, and a build carrying a real test report replaces the model
    // with the measurement legibly instead of silently.
    static Fin<Acoustic> MassLawSpectrum(Seq<Pane> panes, Seq<Cavity> cavities, Op key) {
        double areal = panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 + p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0);
        double bonus = panes.Fold(0.0, static (acc, p) => Math.Max(acc, p.Interlayer.AcousticDampingDb)) + (Asymmetric(panes) ? 2.0 : 0.0);
        Seq<double> resonances = cavities.Map((cavity, index) => {
            double left = PaneArealMass(panes[index]);
            double right = PaneArealMass(panes[index + 1]);
            return 60.0 * Math.Sqrt((left + right) / (left * right * cavity.WidthMm.Value / 1000.0));
        });
        double[] sri = new double[AcousticBand.Count];
        double[] absorption = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            double resonanceDip = resonances.Fold(0.0, (worst, resonance) => Math.Max(worst, Math.Max(0.0, 8.0 - 6.0 * Math.Abs(Math.Log2(band.CenterHz / resonance)))));
            sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(areal, 1e-9) * band.CenterHz) - MassLawOffsetDb + bonus - resonanceDip);
            absorption[band.Key] = 0.03;
        }
        return Acoustic.Of(absorption, sri, key);
    }

    static double PaneArealMass(Pane pane) =>
        pane.Glass.DensityKgM3 * pane.GlassThicknessMm / 1000.0 + pane.Interlayer.DensityKgM3 * pane.InterlayerThicknessMm / 1000.0;

    static bool Asymmetric(Seq<Pane> panes) =>
        panes.Count >= 2 && panes.Exists(p => p.ThicknessMm.Value != panes[0].ThicknessMm.Value);
}

public static class GlazingGwp {
    const double IguAssemblyGwpPerM2 = 2.5;   // EN 15804 IGU fabrication: spacer forming + gas fill + desiccant per m²

    // A1-A3 ONLY, and the edge contribution is MEASURED rather than absorbed into the flat assembly figure: the
    // sealant and spacer carbon is published PER PERIMETRE METRE, so it scales with the unit's own edge-to-area ratio
    // — a small pane carries several times the edge burden of a large one per square metre.
    //
    // The A4, A5, C, and D stages carry NO value. They were fixed fractions of A1-A3 applied identically to every
    // build in the roster, which is not a lifecycle assessment but a shape wearing one, and the recovery credit in
    // particular is a NEGATIVE number a whole-life total would subtract. Transport depends on where the unit is made
    // and installed, and end-of-life on the recovery route a project can reach; neither is a property of the build,
    // so both stay ABSENT until declared-EPD ingestion supplies per-family records and a whole-life reader sees a
    // gap rather than a fabricated total it cannot audit.
    public static ReadOnlyMemory<Option<double>> StagesPerM2(Seq<Pane> panes, EdgeSeal seal, SpacerType spacer, double perimeterToAreaRatio) {
        double substance = panes.Sum(static p =>
            p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 * p.Glass.SubstanceGwpPerKg
            + (p.IsLaminated ? p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0 * p.Interlayer.SubstanceGwpPerKg : 0.0));
        double processing = panes.Sum(static p =>
            p.Glass.FormProcessGwpPerM2 + p.OutboardCoating.ProcessGwpPerM2 + p.InboardCoating.ProcessGwpPerM2 + (p.IsLaminated ? p.Interlayer.ProcessGwpPerM2 : 0.0))
            + IguAssemblyGwpPerM2;
        double edge = (seal.Primary.ProcessGwpPerM + seal.Secondary.ProcessGwpPerM + spacer.EdgeSealGwpPerM) * perimeterToAreaRatio;
        Option<double>[] stages = new Option<double>[LifecycleStage.Count];
        Array.Fill(stages, Option<double>.None);
        stages[LifecycleStage.A1A3.Index] = Some(substance + processing + edge);
        return stages;
    }
}

// The EN 16612 pane-resistance receipt for the GOVERNING pane. StripBendingKnmPerM is a per-metre-STRIP moment
// resistance, which is a resistance and not a capacity: it is the moment a unit strip of the pane can carry, and
// turning it into a pane's load capacity needs the plate's dimensions, aspect ratio, and support condition — none of
// which a glazing BUILD carries, because they belong to the opening the build is installed in. The column therefore
// states what it is, the placement supplies the rest, and the basis rides beside it so a consumer reads which code's
// partial factors and which load duration produced the number.
public readonly record struct GlassCapacity(
    GlassBasis Basis, double ResistanceMpa, double EffectiveThicknessMm, double StripBendingKnmPerM,
    double Kmod, double LoadShareFraction);

// The GLAZING DESIGN BASIS — the jurisdiction row the pane resistance binds, in place of two frozen partial factors.
// EN 16612 publishes γ_M,A and γ_M,v as PROPOSED values and states in its own introduction that they, together with
// k_mod and k_e, are subject to national determination; national codes exercise that, and the second-generation
// technical specification lowers γ_M,A again where wind dominates. Freezing 1.8 and 1.2 into the kernel asserted one
// jurisdiction's numbers as physics, so a second jurisdiction is one more row rather than a kernel edit. The keys
// mirror the capacity#SECTION_CAPACITY DesignBasis roster for the glazing altitude.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassBasis {
    public static readonly GlassBasis En16612 = new("en16612", gammaAnnealed: 1.8, gammaPrestressed: 1.2);
    public double GammaAnnealed { get; }      // γ_M,A
    public double GammaPrestressed { get; }   // γ_M,v
}

// EN 16612 structural-glass resistance over the typed build rows — the fifth structural rail. An ANNEALED pane takes
// f_g,d = k_e·k_mod·k_sp·f_g,k/γ_M,A; a PRESTRESSED pane takes k_mod·k_sp·f_g,k/γ_M,A + k_v·(f_b,k − f_g,k)/γ_M,v.
// The two are different equations, not one equation with unity factors: the edge factor k_e applies to annealed glass
// alone, because a toughened edge carries surface-like compression and the code applies no edge reduction to it, and
// the prestress term does not exist for a glass that was never prestressed. The IGU LOAD SHARE is derived here: a
// sealed cavity couples the panes, so each pane draws its stiffness fraction of the applied pressure and the
// GOVERNING pane is the worst resistance-per-share, not the weakest pane.
public static class GlazingStructural {
    const double KmodFloor = 0.25;         // EN 16612: k_mod is not taken below 0.25 for normal building loads
    const double KmodCoefficient = 0.663;
    const double StressCorrosionExponent = 16.0;

    static double AnnealedFgkMpa => GlassType.Float.CharacteristicBendingMpa;

    public static Fin<GlassCapacity> Capacity(
        Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double loadDurationS,
        GlassBasis basis, double edgeFactor, Op key) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes, key)
        from timed in guard(double.IsFinite(loadDurationS) && loadDurationS > 0.0,
            new KernelFault.OutOfRange(nameof(loadDurationS), loadDurationS, "finite and positive", Some(key)))
        from edged in guard(double.IsFinite(edgeFactor) && edgeFactor is > 0.0 and <= 1.0,
            new KernelFault.OutOfRange(nameof(edgeFactor), edgeFactor, "inside (0, 1]", Some(key)))
        let kmod = Math.Clamp(KmodCoefficient * Math.Pow(loadDurationS / 3600.0, -1.0 / StressCorrosionExponent), KmodFloor, 1.0)
        let shares = LoadShare(panes)
        select panes.Map((pane, index) => PaneCapacity(pane, basis, kmod, edgeFactor, shares[index]))
            .MinBy(static c => c.StripBendingKnmPerM / c.LoadShareFraction);

    // EN 16612 §5 INSULATING-UNIT load sharing: the sealed cavity couples the panes, so an external pressure on one
    // pane is carried by the whole unit in proportion to each pane's own BENDING STIFFNESS — δ = h_ef³ for a plate of
    // the same span. A 4 mm outboard lite beside a 10 mm inboard lite draws only its stiffness fraction of the load,
    // and treating it as carrying the whole pressure under-rates the unit. The fraction derives from the pane
    // geometry the build already carries — no new column, no placement input.
    static Seq<double> LoadShare(Seq<Pane> panes) {
        Seq<double> stiffness = panes.Map(static pane => Math.Pow(EffectiveThicknessMm(pane), 3.0));
        double total = stiffness.Sum();
        return total > 0.0 ? stiffness.Map(k => k / total) : panes.Map(_ => 1.0 / Math.Max(panes.Count, 1));
    }

    // The EN 16612 effective thickness for bending, ω-weighted: a monolithic pane is its own glass thickness, and a
    // laminate interpolates between the NO-SHEAR cube-sum lower bound ∛(Σ h_k³) at ω = 0 and the fully-coupled
    // monolithic upper bound Σ h_k at ω = 1. The interlayer row owns ω and its evidence, so the conservative bound
    // stands until a declared EN 16613 family earns otherwise — and the formula never changes.
    static double EffectiveThicknessMm(Pane pane) {
        if (!pane.IsLaminated) { return pane.ThicknessMm.Value; }
        double ply = pane.GlassThicknessMm / 2.0;
        double unshear = Math.Cbrt(2.0 * Math.Pow(ply, 3.0));
        return unshear + pane.Interlayer.Omega * (pane.GlassThicknessMm - unshear);
    }

    static GlassCapacity PaneCapacity(Pane pane, GlassBasis basis, double kmod, double edgeFactor, double share) {
        double hef = EffectiveThicknessMm(pane);
        double annealed = kmod * pane.Glass.SurfaceProfileFactor * AnnealedFgkMpa / basis.GammaAnnealed;
        double fgd = pane.Glass.StrengtheningFactor.Match(
            Some: kv => annealed + kv * (pane.Glass.CharacteristicBendingMpa - AnnealedFgkMpa) / basis.GammaPrestressed,
            None: () => edgeFactor * annealed);
        return new GlassCapacity(basis, fgd, hef, fgd * (1000.0 * hef * hef / 6.0) * 1e-6, kmod, share);
    }
}

// The IGU service-life receipt — the TIME dimension of the same build rows: the EN 1279-3 gas-retention decay
// re-enters the ONE resistance chain (fill fraction × 0.99^years, the ≤ 1 %/yr certification cap as the declared
// worst case; a vacuum cavity carries no declared decay law and re-evaluates unchanged), and the EN ISO 13788
// temperature factor derives off the decayed Ug as fRsi = 1 − Ug·Rsi. Aged carries the WHOLE re-evaluated receipt,
// not a lone Ug: at years = 0 the retention is exactly 1.0, so the year-zero receipt IS the ambient evaluation and
// the two folds collapse to ONE.
public readonly record struct GlazingService(GlazingPerformance Aged, double FRsi, double FillFractionRemaining);

public static class GlazingLifetime {
    const double RsiCondensationM2KPerW = 0.25;
    const double GasRetentionPerYear = 0.99;   // EN 1279-3: Li ≤ 1.0 %/yr

    public static Fin<GlazingService> AtYears(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double years, CavityTilt tilt, Op key) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes, key)
        from aged in guard(double.IsFinite(years) && years >= 0.0,
            new KernelFault.OutOfRange(nameof(years), years, "finite and non-negative", Some(key)))
        let retention = Math.Pow(GasRetentionPerYear, years)
        let decayed = cavities.Map(c => c.Fill is CavityFill.GasFill gas
            ? c with { Fill = new CavityFill.GasFill(gas.Gas, gas.FillFraction * retention, gas.Balance) }
            : c)
        from perf in GlazingThermal.Evaluate(panes, decayed, tilt, key)
        select new GlazingService(
            perf,
            1.0 - perf.UgCenterOfGlass.Si * RsiCondensationM2KPerW,
            retention);
}

public static class GlazingDetail {
    const double VacuumIntegrityThresholdPa = 0.1;   // ISO 19916 functional-vacuum ceiling — above it the VIG is compromised

    static readonly PropertyEvidence GenericEpd = new("epd", "en 15804 generic insulating glass unit", Option<LocalDate>.None);

    // Fire resistance is NOT gated against a pane's substance, because it is not a substance property. An EI rating
    // is awarded to a TESTED SYSTEM — a specific glass in a specific frame with a specific glazing method — and the
    // prior roster expressed it as a "fire-rated" glass row whose optical, thermal, and density columns were plain
    // borosilicate, which made a tested-assembly property into a material identity and left a real borosilicate pane
    // with no honest row of its own.
    internal static Validation<Error, Unit> Stack(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, Op key) =>
        (guard(!panes.IsEmpty && cavities.Count == panes.Count - 1,
             new KernelFault.InvalidValue(nameof(GlazingDetail), "one fewer cavity than panes", Some(key))).ToValidation(),
         guard(!panes.IsEmpty && GlazingBuild.OfPaneCount(panes.Count).IsSome,
             new KernelFault.InvalidValue(nameof(GlazingBuild), "a published pane-count build", Some(key))).ToValidation(),
         guard(fireEiMinutes >= 0,
             new KernelFault.OutOfRange(nameof(fireEiMinutes), fireEiMinutes, "non-negative", Some(key))).ToValidation(),
         guard(panes.ForAll(Coherent),
             new KernelFault.InvalidValue(nameof(panes), "coherent pane and interlayer declarations", Some(key))).ToValidation(),
         guard(cavities.ForAll(Sane),
             new KernelFault.InvalidValue(nameof(cavities), "admitted cavity width and fill fractions", Some(key))).ToValidation())
            .Apply(static (_, _, _, _, _) => unit).As();

    internal static Fin<Unit> Admit(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, Op key) =>
        Stack(panes, cavities, fireEiMinutes, key).ToFin();

    static bool Coherent(Pane p) =>
        double.IsFinite(p.InterlayerThicknessMm)
        && (p.Interlayer == Interlayer.None
            ? p.InterlayerThicknessMm == 0.0
            : p.InterlayerThicknessMm > 0.0 && p.InterlayerThicknessMm < p.ThicknessMm.Value);

    static bool Sane(Cavity c) => c.Fill.Switch(
        gasFill: static g => double.IsFinite(g.FillFraction) && g.FillFraction is > 0.0 and <= 1.0,
        vacuumFill: static v => double.IsFinite(v.ResidualPressurePa) && v.ResidualPressurePa is > 0.0 and <= VacuumIntegrityThresholdPa);

    static string Named<T>(Seq<T> rows, Func<T, bool> sound, Func<T, string> label) =>
        string.Join(",", rows.Filter(row => !sound(row)).Map(label));

    // serviceYears is the DECLARED SERVICE AGE the whole receipt evaluates at: the EN 1279-3 gas-retention decay is a
    // property of the unit's own certification cap, so the aged state is a re-derivation through the ONE resistance
    // chain, never a stored decay curve. `None` reads year zero, whose retention is exactly 1.0, so the ambient
    // lowering is the same fold with the same bytes. The receipt's fRsi is DERIVED from that Ug, so it needs no seam
    // column of its own; the seam Durability case is the fib Model Code concrete service-life carrier and holds no
    // glazing quantity, so a gas-retention fraction lowered into it would be three columns asserting facts they do
    // not hold.
    public static Fin<Seq<MaterialPropertySet>> Properties(
        Seq<Pane> panes, Seq<Cavity> cavities, EdgeSeal seal, SpacerType spacer, double perimeterToAreaRatio,
        int fireEiMinutes, CavityTilt tilt, Op key, Option<double> serviceYears = default) =>
        from service in GlazingLifetime.AtYears(panes, cavities, fireEiMinutes, serviceYears.IfNone(0.0), tilt, key)
        let perf = service.Aged
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: GlassConductivity(panes),
            specificHeat: GlassSpecificHeat(panes),
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6,
            key)
        // Recycled content and end-of-life recovery are ABSENT rather than asserted: the prior call declared a
        // quarter recycled and ninety percent recovered for every build in the roster, which is a claim about a
        // supply chain and a waste stream that no build column carries and no declaration backed.
        from environmental in MaterialPropertySet.OfEnvironmental(
            MeasurementBasis.PerM2,
            MaterialPropertySet.Environmental.CarbonMatrix(GlazingGwp.StagesPerM2(panes, seal, spacer, perimeterToAreaRatio)),
            recycledContent: None, endOfLifeRecovery: None, key, evidence: GenericEpd)
        // The EI minutes are the tested ASSEMBLY's evidence and the reaction-to-fire class is a separate declaration
        // this page holds for no build, so the fire set carries the resistance alone where one is declared.
        from fire in fireEiMinutes > 0
            ? FireResistance.Ei(fireEiMinutes, key).Map(resistance => Seq(MaterialPropertySet.OfFire(None, resistance)))
            : Fin.Succ(Seq<MaterialPropertySet>())
        let acoustic = MaterialPropertySet.OfAcoustic(perf.Acoustic)
        select Seq(thermal, acoustic, environmental) + fire;

    // The Layered geometry: alternating pane / cavity plies — a monolithic pane one glass ply; a laminated pane the
    // glass-interlayer-glass sub-plies within the pane thickness (the interlayer shades as clear glass: an optically
    // near-glass transparent polymer whose laminate identity rides the Role, not a fabricated polymer appearance
    // row); a cavity the gas.cavity ply. Ply.Role is the BOUNDED PlyRole row; the human-readable
    // IfcMaterialLayer.Name derives at the boundary from (Material, Role, ordinal), and the build identity rides the
    // Product bag — never a parsed layer-name string. INTERNAL: the slot walk indexes both runs off one alternating
    // ordinal, total only over a census-gated stack.
    internal static Fin<Seq<Ply>> Plies(Seq<Pane> panes, Seq<Cavity> cavities, Op key) =>
        toSeq(Enumerable.Range(0, panes.Count + cavities.Count))
            .Traverse(slot => (slot & 1) == 0 ? PanePlies(panes[slot / 2], key) : CavityPly(cavities[slot / 2])).As()
            .Map(static plies => plies.Bind(static p => p));

    static Fin<Seq<Ply>> PanePlies(Pane pane, Op key) =>
        pane.IsLaminated
            ? from half in key.AcceptValidated<PositiveMagnitude>(candidate: pane.GlassThicknessMm / 2.0)
              from inter in key.AcceptValidated<PositiveMagnitude>(candidate: pane.InterlayerThicknessMm)
              select Seq(
                  new Ply(pane.Glass.Appearance, half, PlyRole.Pane),
                  new Ply(MaterialId.Of("glass.crown"), inter, PlyRole.Interlayer),
                  new Ply(pane.Glass.Appearance, half, PlyRole.Pane))
            : Fin.Succ(Seq(new Ply(pane.Glass.Appearance, pane.ThicknessMm, PlyRole.Pane)));

    static Fin<Seq<Ply>> CavityPly(Cavity c) =>
        Fin.Succ(Seq(new Ply(MaterialId.Of("gas.cavity"), c.WidthMm, PlyRole.Cavity)));

    // The DetailSchema.Product bag: the seam-declared IGU rows — PaneBuild/CavityBuild recursive List-of-Complex
    // sub-rows, the SpacerType token, the EdgeSeal complex, the optional MuntinGrid complex (omitted rows content-key
    // a gridless unit distinctly), and the EI minutes over the seam duration dimension. Dimensional rows ride the
    // DIMENSION-only MeasureValue.OfSi so an authored and an imported bag content-key identically; discrete
    // indices/counts ride Text tokens (PropertyValue carries no integer case). INTERNAL: the seed law's detail
    // selector is its one caller, and the census it used to re-run is that law's own coherence.
    internal static Fin<PropertyBag> Bag(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes, EvidenceGrade source) =>
        from paneRows in toSeq(Enumerable.Range(0, panes.Count)).Traverse(i => PaneComplex(panes[i], i)).As()
        from cavityRows in toSeq(Enumerable.Range(0, cavities.Count)).Traverse(i => CavityComplex(cavities[i], i)).As()
        from muntinRows in muntin.Match(Some: MuntinRows, None: static () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))
        from fireRows in fireEiMinutes > 0
            ? Measured(DetailSchema.FireResistanceEi, Dimension.DurationDim, fireEiMinutes * 60.0).Map(static row => Seq(row))
            : Fin.Succ(Seq<(PropertyName, PropertyValue)>())
        let rows = Seq(
            (DetailSchema.PaneBuild, (PropertyValue)new PropertyValue.List(paneRows.Map(static value => (PropertyValue)value))),
            (DetailSchema.CavityBuild, (PropertyValue)new PropertyValue.List(cavityRows.Map(static value => (PropertyValue)value))),
            Token(DetailSchema.SpacerType, spacer.Key),
            Sourced(source),
            (DetailSchema.EdgeSeal, (PropertyValue)new PropertyValue.Complex("edge-seal", Map(
                (DetailSchema.Primary, (PropertyValue)new PropertyValue.Text(edgeSeal.Primary.Key)),
                (DetailSchema.Secondary, new PropertyValue.Text(edgeSeal.Secondary.Key)),
                (DetailSchema.Desiccant, new PropertyValue.Text(edgeSeal.Desiccant.Key)),
                (DetailSchema.CorneredKeys, new PropertyValue.Boolean(edgeSeal.CorneredKeys))))))
            + muntinRows
            + fireRows
        select ProductRows([.. rows]);

    static Fin<PropertyValue> Si(Dimension dimension, double si) =>
        MeasureValue.OfSi(dimension, si).Map(static value => (PropertyValue)new PropertyValue.Measure(value));

    static Fin<Seq<(PropertyName, PropertyValue)>> MuntinRows(MuntinGrid muntin) =>
        from width in Si(Dimension.LengthDim, muntin.BarWidthMm.Value * 1e-3)
        from depth in Si(Dimension.LengthDim, muntin.BarDepthMm.Value * 1e-3)
        select Seq((DetailSchema.MuntinGrid, (PropertyValue)new PropertyValue.Complex("muntin", Map(
            (DetailSchema.Style, (PropertyValue)new PropertyValue.Text(muntin.Style.Key)),
            (DetailSchema.HorizontalBars, new PropertyValue.Text($"{muntin.HorizontalBars}")),
            (DetailSchema.VerticalBars, new PropertyValue.Text($"{muntin.VerticalBars}")),
            (DetailSchema.BarWidth, width),
            (DetailSchema.BarDepth, depth)))));

    static Fin<PropertyValue.Complex> PaneComplex(Pane pane, int index) =>
        from thickness in Si(Dimension.LengthDim, pane.ThicknessMm.Value * 1e-3)
        from interlayerThickness in Si(Dimension.LengthDim, pane.InterlayerThicknessMm * 1e-3)
        select new PropertyValue.Complex($"pane-{index}", Map(
            (DetailSchema.Glass, (PropertyValue)new PropertyValue.Text(pane.Glass.Key)),
            (DetailSchema.Thickness, thickness),
            (DetailSchema.CoatingOutboard, new PropertyValue.Text(pane.OutboardCoating.Key)),
            (DetailSchema.CoatingInboard, new PropertyValue.Text(pane.InboardCoating.Key)),
            (DetailSchema.Interlayer, new PropertyValue.Text(pane.Interlayer.Key)),
            (DetailSchema.InterlayerThickness, interlayerThickness)));

    static Fin<PropertyValue.Complex> CavityComplex(Cavity cavity, int index) => cavity.Fill.Switch(
        state: (WidthMm: cavity.WidthMm.Value, Index: index),
        gasFill: static (state, gas) =>
            from width in Si(Dimension.LengthDim, state.WidthMm * 1e-3)
            select new PropertyValue.Complex($"cavity-{state.Index}", Map(
                (DetailSchema.Gas, (PropertyValue)new PropertyValue.Text(gas.Gas.Key)),
                (DetailSchema.FillFraction, new PropertyValue.Text($"{gas.FillFraction:R}")),
                (DetailSchema.Balance, new PropertyValue.Text(gas.Balance.Key)),
                (DetailSchema.Width, width))),
        vacuumFill: static (state, vacuum) =>
            from pressure in Si(Dimension.PressureDim, vacuum.ResidualPressurePa)
            from radius in Si(Dimension.LengthDim, vacuum.PillarRadiusMm.Value * 1e-3)
            from pitch in Si(Dimension.LengthDim, vacuum.PillarPitchMm.Value * 1e-3)
            from width in Si(Dimension.LengthDim, state.WidthMm * 1e-3)
            select new PropertyValue.Complex($"cavity-{state.Index}", Map(
                (DetailSchema.ResidualPressure, pressure),
                (DetailSchema.PillarRadius, radius),
                (DetailSchema.PillarPitch, pitch),
                (DetailSchema.Width, width))));

    // The homogenized glass-only Thermal columns, each under its own physical mixing law: conductivity the SERIES
    // harmonic mean Σt/Σ(t/λ) (an arithmetic mean overstates a mixed borosilicate/soda-lime stack), specific heat the
    // MASS-weighted mean Σ(ρ·t·c)/Σ(ρ·t) (heat capacity mixes by mass, never by thickness). The census guarantees a
    // non-empty stack with positive glass per pane, so both divisors are positive — no fallback knob exists.
    static double GlassConductivity(Seq<Pane> panes) =>
        panes.Sum(static p => p.GlassThicknessMm) / panes.Sum(static p => p.GlassThicknessMm / p.Glass.ConductivityWmK);

    static double GlassSpecificHeat(Seq<Pane> panes) =>
        panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm * p.Glass.SpecificHeatJKgK)
            / panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm);
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class GlazingSeed {
    // ComponentAuthority.En (EN 1279 IGU authority) supplies its own region column; an IGU lays no mortar joint.
    // Every current build ships the standard PIB primary + polysulfide secondary + molecular-sieve + keyed-corner
    // EN 1279-2 construction; a structural-glazing row swaps its own EdgeSeal column to a silicone secondary, never a
    // parallel row shape. No seeded build has been through an EN 1279-2 climate test, so every row carries the
    // moisture index as absence — the one honest statement about a quantity only a tested system possesses.
    static readonly ComponentStandard IguStandard =
        new(ComponentAuthority.En.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.En);
    static readonly EdgeSeal StandardEdgeSeal = new(Sealant.Pib, Sealant.Polysulfide, Desiccant.MolecularSieve3A, CorneredKeys: true, Moisture: None);
    static readonly Option<MuntinGrid> NoGrid = Option<MuntinGrid>.None;
    static readonly Cavity Argon16 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0));
    static readonly Cavity Argon12 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(12.0));

    static Pane Mono(GlassType glass, double thicknessMm, Coating outboard, Coating inboard) =>
        new(glass, PositiveMagnitude.Create(thicknessMm), outboard, inboard, Interlayer.None, 0.0);
    static Pane Clear(GlassType glass, double thicknessMm) => Mono(glass, thicknessMm, Coating.None, Coating.None);

    public static readonly Seq<GlazingRow> Roster = Seq(
        new GlazingRow("glazing.double-4-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-6-12-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 6.0), Clear(GlassType.Float, 6.0)), Seq(Argon12), 0, NoGrid),
        new GlazingRow("glazing.double-4-20-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(20.0))), 0, NoGrid),
        // The inboard pane carries a soft-coat double-silver low-E on its OUTBOARD face (surface 3, cavity-facing) —
        // its OutboardCoating row, so Pane.EmissivityOf(inboard: false) reads the εn 0.04 the cavity term sees.
        new GlazingRow("glazing.double-6-16-6-lowe", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 6.0), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble, Coating.None)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-4-12-4-alu", SpacerType.ColdEdgeAluminum, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Air, 1.00, CavityGas.Air), PositiveMagnitude.Create(12.0))), 0, NoGrid),
        // Laminated 66.4 outboard pane (two 3 mm glass + 0.76 mm two-ply PVB): Plies splits glass-PVB-glass; the
        // MassLawSpectrum reads the coincidence-damping bonus; 6.76-vs-4.0 asymmetry adds the dip-shift bonus.
        new GlazingRow("glazing.double-lam664-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(new Pane(GlassType.Float, PositiveMagnitude.Create(6.76), Coating.None, Coating.None, Interlayer.Pvb, 0.76), Clear(GlassType.Float, 4.0)),
            Seq(Argon16), 0, NoGrid),
        // Triple low-E on surfaces 2 and 5 (outer pane INBOARD face + inner pane OUTBOARD face) — each cavity sees
        // one low-E surface; krypton for the narrow gaps.
        new GlazingRow("glazing.triple-4-16kr-4-16kr-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatDouble), Clear(GlassType.Float, 4.0), Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble, Coating.None)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Krypton, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0)),
                new Cavity(new CavityFill.GasFill(CavityGas.Krypton, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0))), 0, NoGrid),
        new GlazingRow("glazing.triple-4-12ar-4-12ar-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatDouble), Clear(GlassType.Float, 4.0), Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble, Coating.None)),
            Seq(Argon12, Argon12), 0, NoGrid),
        new GlazingRow("glazing.quadruple-4-12ar-4-12ar-4-12ar-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatDouble), Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0), Mono(GlassType.Float, 4.0, Coating.SoftCoatDouble, Coating.None)),
            Seq(Argon12, Argon12, Argon12), 0, NoGrid),
        // Dual-coated outboard pane: solar-control on surface 1 (the exterior weather face) AND triple-silver low-E
        // on surface 2 (cavity-facing) — two independent per-face rows on ONE pane, the build a single-coating shape
        // cannot spell.
        new GlazingRow("glazing.double-6sol2lowe-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 6.0, Coating.SolarControl, Coating.SoftCoatTriple), Clear(GlassType.Float, 6.0)), Seq(Argon16), 0, NoGrid),
        // ISO 19916 vacuum unit: soft-coat triple-silver on surface 2 suppressing the now-dominant radiative
        // exchange; a 0.3 mm gap at 0.08 Pa with 0.25 mm-radius pillars on a 20 mm pitch (the Collins conduction).
        new GlazingRow("glazing.vig-4lowe-vac-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatTriple), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.VacuumFill(0.08, PositiveMagnitude.Create(0.25), PositiveMagnitude.Create(20.0)), PositiveMagnitude.Create(0.3))), 0, NoGrid),
        // Fire-rated EI 30: a 6 mm borosilicate outboard pane (the fire side) + a 6 mm float with low-E on surface 3.
        new GlazingRow("glazing.fire-ei30-6fr-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Borosilicate, 6.0), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble, Coating.None)), Seq(Argon16), 30, NoGrid),
        // True-divided grid: one horizontal + two vertical 25×20 mm muntin bars (manufacturer dims) — face geometry
        // the generator places across the pane.
        new GlazingRow("glazing.double-4-16-4-grid", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)), Seq(Argon16), 0,
            Some(new MuntinGrid(MuntinStyle.TrueDivided, 1, 2, PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(20.0)))));

    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, GlazingRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static Fin<GlazingRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    // The seed POLICY value. Substance and appearance stay INDEPENDENT per the two-slot law: the SUBSTANCE is the
    // outboard pane's own glass material — the row a property read resolves mechanical and thermal facts through —
    // while the APPEARANCE is the library shade it renders as. Binding both to the appearance row made the IGU's
    // material identity its render identity, so a low-iron and a soda-lime unit shading to the same crown row became
    // one substance and any property read keyed on it answered for the wrong glass.
    public static readonly SeedLaw<GlazingRow> Law = SeedLaw<GlazingRow>.Of(
        family: ComponentFamily.Glazing,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: static r => r.Panes[0].Glass.Substance,
        source: static r => r.Source,
        standard: static _ => IguStandard,
        detail: Some<Func<GlazingRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.Panes[0].Glass.Appearance);

    static Validation<Error, Unit> Coherence(GlazingRow r, Op key) =>
        (GlazingDetail.Stack(r.Panes, r.Cavities, r.FireResistanceEiMinutes, key),
         guard(r.Muntin.ForAll(static m => m.HorizontalBars >= 0 && m.VerticalBars >= 0 && m.HorizontalBars + m.VerticalBars > 0),
             new KernelFault.InvalidValue(nameof(r.Muntin), "non-negative bars with at least one muntin", Some(key))).ToValidation())
            .Apply(static (_, _) => unit).As();

    static Fin<SectionProfile> Profile(GlazingRow r, Op key) =>
        from plies in GlazingDetail.Plies(r.Panes, r.Cavities, key)
        let overallMm = r.Panes.Sum(static p => p.ThicknessMm.Value) + r.Cavities.Sum(static c => c.WidthMm.Value)
        from profile in SectionProfile.Layered.Of(plies, overallMm: overallMm, widthMm: overallMm, key)
        select profile;

    static Fin<PropertyBag> Detail(GlazingRow r, SectionProfile profile, Op key) =>
        from performance in GlazingThermal.Evaluate(r.Panes, r.Cavities, CavityTilt.Vertical, key)
        from bag in GlazingDetail.Bag(r.Panes, r.Cavities, r.Spacer, r.EdgeSeal, r.Muntin, r.FireResistanceEiMinutes, r.Source)
        select bag;

    // The ComponentFamily.Glazing CAPACITY producer: the typed Resolve restores the pane and cavity stacks the
    // Component's Layered plies flatten, and the placement carries the three inputs a build cannot hold — the load
    // duration a pane's k_mod reads, the JURISDICTION whose partial factors the resistance divides by, and the edge
    // condition of the opening the pane is installed in, which is a property of the installation and never of the
    // unit. An IGU is unsectioned by construction, so the section argument is structurally absent.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Resolve(component, key)
        from capacity in GlazingStructural.Capacity(
            row.Panes, row.Cavities, row.FireResistanceEiMinutes, placement.GlassLoadDurationS,
            placement.GlassBasis, placement.GlassEdgeFactor, key)
        from lifted in SectionCapacity.Lift(new CapacityReceipt.Glass(component.Designation, capacity), key)
        select lifted;
}
```

## [03]-[RESEARCH]

(none)
