# [MATERIALS_GLAZING]

THE GLAZING SEED PAGE — the `glazing` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Product`) grounded in insulating-glass build physics. An IGU is a `Component` whose `SectionProfile.Layered` geometry contains only `PlyRole.Pane`/`Interlayer`/`Cavity`, whose build inputs ride the `DetailSchema.Product` bag, and whose engineering performance derives from the typed `GlazingRow`: `GlazingThermal` owns EN 673 `Ug`, EN 410 / ISO 9050 `g` and `τv`, and the mass-law acoustic spectrum; `GlazingStructural` owns the EN 16612 pane resistance the capacity rail lifts; `GlazingLifetime` owns the EN 1279-3 gas-decay and EN ISO 13788 `fRsi` service receipt; `GlazingGwp` owns the lifecycle vector; `GlazingDetail.Properties` lowers the receipt to `MaterialPropertySet`. An IGU crosses as `IfcMaterialLayerSet`, never `IfcProfileDef`, so its `Layered` profile answers unsectioned membership from its own `ProfileTopology`. `GlazingSeed.Resolve` joins a resolved `ComponentId` back to its pane, cavity, edge, grid, and fire axes so the projector can execute the promised lowering without parsing the bag or the designation.

## [01]-[INDEX]

- [02]-[GLAZING_FAMILY]: the glazing policy vocabularies, `CavityFill`, the typed build rows, the shared `GlazingThermal` resistance/optical/acoustic kernel, `GlazingPerformance`, the `GlazingStructural` EN 16612 pane-resistance kernel with its `GlassCapacity` receipt, the `GlazingLifetime` service receipt, `GlazingGwp`, `GlazingDetail`, and the `GlazingSeed` authored table with its `Rows` fold and `ComponentId`-keyed `Resolve` join.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing policy vocabulary; `CavityFill` the gas-vs-vacuum `[Union]`; `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` the typed stack rows; `GlazingThermal` the shared resistance, optical, and acoustic kernel; `GlazingGwp` the lifecycle vector; `GlazingPerformance` the computed receipt; `GlazingDetail` the shared admission, bag, property, and ply operations; `GlazingSeed` the authored EN 1279 table, generator, and typed resolver.
- Cases: the glazing vocabulary spans the glass, per-face coating, gas, interlayer, spacer, and edge-seal axes. `GlazingBuild` derives `Double`, `Triple`, or `Quadruple` from `Panes.Count`; stack arity and finite pane/cavity values admit before either physics boundary runs.
- Entry: `GlazingSeed.Rows(Context) : Fin<Seq<ComponentRow>>` traverses the typed table through the shared build admission, performance gate, ply projection, layered-profile admission, and `Component.Of`; one malformed row aborts the catalogue. `GlazingSeed.Resolve(Component, Op) : Fin<GlazingRow>` restores the typed build axes by `ComponentId`. `GlazingDetail.Properties(panes, cavities, ei, key, serviceYears)` composes the same admission and lowers `Thermal`/`Acoustic`/`Environmental`/`Fire` as one rail AT the declared service age (`None` reads year zero).
- Packages: Rasm.Numerics (`PositiveMagnitude` — every pane/gap/pillar/bar column), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `MaterialPropertySet`, `MeasureValue`, `Dimension`, `MeasurementBasis`, `LifecycleStage`, `Acoustic`, `AcousticBand`, `FireRating`, `FireResistance`, `DetailSchema`, `PropertyValue`, `PropertyName`, `PropertyBag`), Rasm.Materials.Component (`Component`/`ComponentRow`/`SectionProfile`/`Ply`/`IfcBinding`/`QuantityRow`/`ComponentDetail`/`ComponentFault`/`Coring`/`ComponentStandard`/`ComponentAuthority`), Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet (`RatioUnit.DecimalFraction` — the dimensionless `g`/`τv` seam admission), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (`WithRelativeUncertainty`, `IUncertainty<HeatTransferCoefficient>.LowerBound`/`UpperBound` — the typed `Ug` model band lowered onto `MeasureBand`). VividOrange.Materials is NOT composed (glazing fills no profile solve). Wacton.Unicolour is NOT composed: a coating's OPTICAL signal crosses as the coated pane's content-keyed `Node.Appearance`; glazing tags the `MaterialId`, never the colour kernel.
- Growth: a new IGU is one `GlazingRow`; a new glass substance one `GlassType` row; a new coating tier one `Coating` row; a new gas one `CavityGas` row; a new interlayer one `Interlayer` row; a new edge-seal chemistry one `Sealant`/`Desiccant` row; a quad build one `GlazingBuild` row the derived `Build` read maps; an electrochromic variant a `GlassType` row plus a `Coating` row. The full per-wavelength `τ(λ)`/`ρ(λ)` angular EN 410 §5 spectral integral is a `GlassType`/`Coating` per-wavelength-curve column growth the broadband recursion here is the center-of-glass simplification of, never a parallel optical owner.
- Boundary: `SectionProfile.Layered` is the geometric gross only; `ComponentFamily.Glazing.Admits` rejects every non-glazing `PlyRole`, and physics reads the typed `Pane`/`Cavity` rows restored through `GlazingSeed.Resolve`, never re-parsed plies or bag text. `GlazingThermal.Evaluate` is INTERIOR (internal, every ingress admitted by the ONE `GlazingDetail.Admit` stack gate) and computes one ordered resistance chain shared by `Ug` and the EN 410 inward-flowing secondary flux. `QuantityRow.HeatTransferCoefficient.OfNative` owns the `Ug` mint, while dimension-only bag rows use `MeasureValue.OfSi(Dimension, si)`. `SpacerType.PsiWmK` feeds the Compute-owned whole-window aggregation. The IFC layer name derives from `(Material, Role, ordinal)`, coating stays face data, and `MuntinGrid` stays face geometry.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using NodaTime;                      // LocalDate — the PropertyEvidence expiry axis the EPD evidence row carries
using Rasm.Numerics;                  // PositiveMagnitude (the kernel >0 finite magnitude atom — NOT Rasm.Domain)
using Rasm.Domain;                   // Context, Op, AcceptValidated
using Rasm.Element.Composition;                  // MaterialId, MaterialPropertySet, MeasureValue, MeasurementBasis, LifecycleStage,
using Rasm.Element.Properties;
                                     // Acoustic, AcousticBand, FireRating, FireResistance, DetailSchema, PropertyValue, PropertyName
using Thinktecture;
using UnitsNet;                      // RatioUnit (the dimensionless g/τv fraction unit, admitted through the seam MeasureValue)
using VividOrange.Uncertainties;
using VividOrange.Uncertainties.Quantities.Utility;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete-count Dimension
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
// broadband EN 410 / ISO 9050 SOLAR and VISIBLE transmittance and reflectance, the characteristic bending strength the
// EN 16612 pane resistance reads (annealed 45, heat-strengthened 70, fully-tempered/toughened-borosilicate 120 MPa —
// EN 572-1 / EN 1863 / EN 12150 / EN 13024), and the safety class. All columns
// PUBLISHED (EN 572-1 / EN 673 Annex A / EN 15804 generic); a laminated pane is any glass plus an Interlayer, not a case.
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
    // patterned. It was previously frozen at unity in the resistance kernel, which is the float value — so a patterned
    // pane, whose 25% profile penalty is exactly what the factor exists to carry, priced as if it were float.
    public double SurfaceProfileFactor { get; }
    // EN 16612 Table 7 k_v, the STRENGTHENING factor, present only on a PRESTRESSED glass: 1.0 where the process uses
    // no tongs or holding devices (horizontal toughening) and 0.6 where it does (vertical). Absence is what makes an
    // ANNEALED glass annealed — the prestress term of the design formula does not apply to it at all, rather than
    // applying with a factor of one, and the two are different equations.
    public Option<double> StrengtheningFactor { get; }
    public bool Safety { get; }

    // The library appearance ROW COLUMN each pane shades to (clear crown; the heavier flint for borosilicate — a row
    // value, never an identity switch on `this`). A low-E/solar-control COATING is a thin-film surface effect, NOT a
    // bulk shade — it rides the coated pane's Node.Appearance, so this column takes no Coating knob. The SUBSTANCE is
    // the separate slot the property catalogue keys on, derived from the row's own key so a new glass names one row
    // and gets both identities.
    public MaterialId Appearance { get; }
    public MaterialId Substance => MaterialId.Of($"glass.{Key}");
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
    public static readonly Interlayer None = new("none", nominalPlyMm: 0.0,  acousticDampingDb: 0.0, shearModulusMPa: 0.0,   conductivityWmK: 1.00, densityKgM3: 0.0,    substanceGwpPerKg: 0.0, processGwpPerM2: 0.0, omega: 0.0, omegaSource: Provenance.Defined);
    public static readonly Interlayer Pvb  = new("pvb",  nominalPlyMm: 0.38, acousticDampingDb: 3.0, shearModulusMPa: 2.0,   conductivityWmK: 0.20, densityKgM3: 1070.0, substanceGwpPerKg: 3.40, processGwpPerM2: 1.5, omega: 0.0, omegaSource: Provenance.Authored);
    public static readonly Interlayer Sgp  = new("sgp",  nominalPlyMm: 0.89, acousticDampingDb: 2.0, shearModulusMPa: 110.0, conductivityWmK: 0.20, densityKgM3: 950.0,  substanceGwpPerKg: 4.20, processGwpPerM2: 2.0, omega: 0.0, omegaSource: Provenance.Authored);
    public static readonly Interlayer Eva  = new("eva",  nominalPlyMm: 0.38, acousticDampingDb: 2.5, shearModulusMPa: 8.0,   conductivityWmK: 0.23, densityKgM3: 950.0,  substanceGwpPerKg: 2.90, processGwpPerM2: 1.4, omega: 0.0, omegaSource: Provenance.Authored);
    public double NominalPlyMm { get; }
    public double AcousticDampingDb { get; }
    public double ShearModulusMPa { get; }
    public double ConductivityWmK { get; }
    public double DensityKgM3 { get; }
    public double SubstanceGwpPerKg { get; }
    public double ProcessGwpPerM2 { get; }
    // The EN 16613 shear-transfer coefficient ω the effective-thickness formula weights the sandwich term by: 0 is the
    // NO-SHEAR lower bound (the two sub-plies bend independently), 1 the fully-coupled monolithic upper bound. It is an
    // AUTHORED column at 0 on every real interlayer, and the provenance column says so out loud — the published ω
    // family tables are load-duration × temperature grids this estate does not hold, and crediting shear coupling a
    // family has not declared over-states a laminate's resistance by up to the ratio of the monolithic to the cube-sum
    // thickness. A declared family fills omega and its provenance flips to Published, with no formula edit anywhere:
    // EffectiveThicknessMm already reads the column.
    public double Omega { get; }
    public Provenance OmegaSource { get; }
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

    // The published typical water-adsorption capacity at standard conditions, as a mass fraction — the 3A sieve's
    // 20-24 g per 100 g band read at its midpoint, silica's own figure beside it.
    public double AdsorptionCapacity { get; }

    // The EN 1279-2 STANDARD adsorption capacity Tc — the reserve the moisture-penetration index divides by, measured
    // under the clause's own limit environment rather than at the ambient conditions the typical figure is quoted at.
    // It is a DIFFERENT number from AdsorptionCapacity and it is optional because the standard's own value is
    // desiccant-PRODUCT specific: two sieves both sold as 3A differ in bead size, binder fraction, and activation, and
    // the index is sensitive to exactly that difference.
    // [SPIKE]: a product-declared Tc lands as one column value per row and needs no other edit. Until one does, the
    // floor below is the declared default — the typical capacity, which is the honest conservative stand-in and is
    // stated as such rather than presented as the standard's constant.
    public Option<double> StandardCapacity { get; }

    public double CapacityFraction => StandardCapacity.IfNone(AdsorptionCapacity);
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

// The EN 673 cavity INCLINATION rows — the convection correlation constants per heat-flow condition. A vertical
// cavity carries horizontal heat flow; a tilted or horizontal cavity carries it upward and convects more strongly at
// the same Rayleigh number; downward heat flow suppresses convection entirely, which the standard states as a
// condition rather than as a small coefficient. A new inclination is one row.
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

// The IGU build classification DERIVED from the pane count — the layer-count semantics as a vocabulary, never a stored
// field a malformed "double" row with three panes contradicts. A quad build is one more row this derived read then maps.
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

// One pane in the IGU stack: glass substance, TOTAL thickness, ONE Coating state PER PHYSICAL FACE (outboard the
// exterior-facing face, inboard the interior-facing face; Coating.None the uncoated state — a dual-coated pane
// carries two independent rows, and a mis-transcribed surface index is unrepresentable rather than silently
// uncoated), and an Interlayer with its total thickness (0 / Interlayer.None monolithic; > 0 the
// glass-interlayer-glass laminate). Glass-only = total − interlayer.
public readonly record struct Pane(GlassType Glass, PositiveMagnitude ThicknessMm, Coating OutboardCoating, Coating InboardCoating, Interlayer Interlayer, double InterlayerThicknessMm) {
    public bool IsLaminated => Interlayer != Interlayer.None && InterlayerThicknessMm > 0.0;
    public double GlassThicknessMm => ThicknessMm.Value - InterlayerThicknessMm;

    // The EN 673 emissivity of ONE of the pane's two faces: the face's OWN coating row corrects εn (a coating
    // suppresses radiation across the cavity IT faces); an uncoated face reads the glass NormalEmissivity. The
    // thermal and EN 410 reads dispatch on the same per-face declaration.
    public double EmissivityOf(bool inboard) =>
        (inboard ? InboardCoating : OutboardCoating).CorrectedEmissivity.IfNone(Glass.NormalEmissivity);

    // The pane's directional solar optics (τ, ρ_front, ρ_back) the EN 410 recursion combines: transmittance = glass
    // base × BOTH face multipliers (each thin film attenuates the through-path once); each face's reflectance is its
    // own coating's elevated value, else the glass value — a coated pane is asymmetric front-to-back and a
    // dual-coated pane carries both elevations.
    public (double T, double Rf, double Rb) Solar() => (
        Glass.SolarTransmittance * OutboardCoating.SolarTransmittanceMultiplier * InboardCoating.SolarTransmittanceMultiplier,
        OutboardCoating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance),
        InboardCoating.CoatedSolarReflectance.IfNone(Glass.SolarReflectance));

    public (double T, double Rf, double Rb) Visible() => (
        Glass.VisibleTransmittance * OutboardCoating.VisibleTransmittanceMultiplier * InboardCoating.VisibleTransmittanceMultiplier,
        OutboardCoating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance),
        InboardCoating.CoatedVisibleReflectance.IfNone(Glass.VisibleReflectance));
}

// One cavity in the IGU stack: the fill discriminant and the gap width the EN 673 / ISO 19916 conductance read.
public readonly record struct Cavity(CavityFill Fill, PositiveMagnitude WidthMm);

// The EN 1279-2 moisture-penetration index — the one durability quantity deciding whether an edge seal keeps its cavity
// dry, and a MEASURED result rather than a catalogue column. The index reads as the share of the desiccant's drying
// reserve that standardized ageing consumed: Ti is the as-filled moisture content, Tf the content after the climate
// test, and Tc the standard adsorption capacity under the clause's own limit environment — so the capacity, not the
// final content, is the denominator's endpoint and an index above one would mean ageing outran the reserve entirely.
// Every input is a per-SYSTEM observation — one spacer, sealant, and desiccant measured together — which is why a
// seeded build carries ABSENCE: the standard states outright that comparing indices across unit systems is
// meaningless, so there is no per-system index table to transcribe, and the per-DESICCANT capacity constants it does
// tabulate reach one publisher only and therefore stay off the Desiccant rows.
public readonly record struct MoisturePenetration(double InitialFraction, double FinalFraction, double CapacityFraction) {
    public const double AverageCeiling = 0.20;      // the aged specimen set's mean index
    public const double IndividualCeiling = 0.25;   // the worst single unit in that set

    public double Index => (FinalFraction - InitialFraction) / (CapacityFraction - InitialFraction);
    public bool Conforms => Index <= IndividualCeiling;

    // Admission is the ordering the index depends on: the reserve must be a real interval (Ti strictly below Tc) or
    // the quotient is a division by a vanishing or negated denominator reported as a durability number.
    // The capacity endpoint reads off the DESICCANT rather than arriving as a caller scalar: the reserve is a property
    // of the material in the spacer, and letting a caller state it invites an index divided by a number nothing in the
    // build declares.
    public static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, Desiccant desiccant, Op key) =>
        Of(initialFraction, finalFraction, desiccant.CapacityFraction, key);

    static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, double capacityFraction, Op key) =>
        from finite in guard(double.IsFinite(initialFraction) && double.IsFinite(finalFraction) && double.IsFinite(capacityFraction),
            ComponentFault.Family(key, "<moisture-index-nonfinite>"))
        from ordered in guard(initialFraction >= 0.0 && initialFraction < capacityFraction && finalFraction >= initialFraction,
            ComponentFault.Family(key, $"<moisture-index-reserve-degenerate:{initialFraction:R}:{finalFraction:R}:{capacityFraction:R}>"))
        select new MoisturePenetration(initialFraction, finalFraction, capacityFraction);
}

// The EN 1279-2 edge-seal construction: primary moisture sealant (PIB), structural/durability secondary sealant, spacer
// desiccant, keyed-vs-bent corners, and the moisture-penetration index its system carries once a climate test has
// measured one — the durability + edge-thermal + GWP datums.
public readonly record struct EdgeSeal(Sealant Primary, Sealant Secondary, Desiccant Desiccant, bool CorneredKeys, Option<MoisturePenetration> Moisture);

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
    Acoustic Acoustic,
    Provenance AcousticSource) {
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
        // g/τv admit as dimensionless Ratio measures (RatioUnit.DecimalFraction, the seam IsDimensionless path — no SI
        // reprojection, content keys frozen); the Ug typed mint routes through QuantityRow.HeatTransferCoefficient, and
        // the model band rides the seam's PUBLIC MeasureBand.Admit + WithUncertainty(band, key) rail (Interval is
        // seam-internal), so a band excluding the nominal faults typed instead of minting silently.
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
               // Ug, g, and tv are computed from published data; the acoustic spectrum is an estimation model with no
               // standardized calculation route, so the receipt names that difference rather than presenting four
               // numbers of one apparent standing.
               select new GlazingPerformance(ugBanded, solarG, lightTv, acoustic, Provenance.Defined);
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

    // EN 673 Nusselt number Nu = max(1, A·Ra^n) over the Rayleigh number Ra = ρ²·s³·g·c·ΔT / (T_m·μ·λ). Below the
    // critical Rayleigh the cavity does not convect (Nu = 1, pure conduction λ/s); above it the gas circulates and
    // raises the conductance — the heavier krypton and xenon suppress both terms.
    //
    // A and n are INCLINATION-dependent and the standard publishes a set per heat-flow condition, which the previous
    // kernel collapsed to the vertical pair alone. Buoyancy in a tilted cavity is not the buoyancy in a vertical one:
    // an inclined or horizontal cavity with UPWARD heat flow convects harder than a vertical cavity at the same
    // Rayleigh number, so a sloped rooflight priced on the vertical constants under-reads its own conductance, and a
    // cavity with DOWNWARD heat flow does not convect at all — the warm gas is already on top and Nu is 1 by the
    // physics, not by falling under a threshold. The tilt therefore selects a ROW, and an intermediate angle
    // interpolates between the two nearest, which is the standard's own instruction.
    static double Nusselt(GasProperties gas, double s, CavityTilt tilt) {
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return tilt.Convects ? Math.Max(1.0, tilt.Coefficient * Math.Pow(ra, tilt.Exponent)) : 1.0;
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
    static (double T, double Rf, double Rb) Span(Seq<Pane> panes, int lo, int hi, Func<Pane, (double T, double Rf, double Rb)> optics) =>
        panes.Skip(lo).Take(hi - lo).Fold((T: 1.0, Rf: 0.0, Rb: 0.0), (acc, pane) => Combine(acc, optics(pane)));

    // The field-incidence mass-law acoustic spectrum. This is an ESTIMATION MODEL and it is the one number this page
    // produces that is not derived from published data: there is NO standardized calculation method for the sound
    // reduction index of a glazing product. The product standard admits exactly three routes to an R_w — a
    // laboratory measurement, its own table of conservatively measured typical values, and its extension rules for
    // carrying a measurement across to a related build — and the industry guidance behind it states in terms that a
    // mathematical determination from surface weight is neither correct nor permitted for declaration. The published
    // typical values prove why: a symmetric 8-8 unit rates BELOW both 8-4 and 8-6, because equal leaves put both
    // coincidence dips at the same frequency, and no mass-law estimator reproduces that inversion.
    //
    // The model therefore rides its own named policy row and the receipt records that the acoustic column is
    // MODEL-DERIVED, so a consumer reading the seam sees an estimate labelled as one rather than a spectrum it would
    // otherwise take for measured data alongside the EN 673 and EN 410 values computed beside it. A build carrying a
    // real test report replaces the model with the measurement, and the provenance column is what makes the swap
    // legible instead of silent.
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
    const double IguAssemblyGwpPerM2 = 2.5;   // EN 15804 IGU fabrication: spacer forming + gas fill + desiccant per m²

    // A1-A3 ONLY, and the edge contribution is measured rather than absorbed into the flat assembly figure: the
    // sealant and spacer carbon is published PER PERIMETRE METRE, so it scales with the unit's own edge-to-area ratio
    // — a small pane carries several times the edge burden of a large one per square metre — and the three per-metre
    // columns the vocabulary already published had no reader while a single flat 2.5 stood in for all of them.
    //
    // The A4, A5, C, and D stages carry NO value. They were previously fixed fractions of A1-A3 — transport at five
    // percent, installation at three, end-of-life at eight, and a negative fifteen percent recovery credit — applied
    // identically to every build in the roster, which is not a lifecycle assessment but a shape wearing one, and the
    // recovery credit in particular is a NEGATIVE number a whole-life total would subtract. Transport depends on
    // where the unit is made and installed, and end-of-life on the recovery route a project can actually reach;
    // neither is a property of the build. They stay ABSENT until the declared-EPD ingestion supplies per-family
    // records, and a whole-life reader sees a gap rather than a fabricated total it cannot audit.
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

// The EN 16612 pane-resistance receipt for the GOVERNING pane: the design bending strength f_g,d, the effective laminate
// thickness the bending stress reads, the per-metre-strip design moment resistance the capacity#SECTION_CAPACITY GlassPane
// case folds demand against, and the applied kmod. Per-pane load SHARING across the IGU (stiffness-proportional pressure
// partition) is a placement/Compute concern — the governing single-pane resistance is this receipt's conservative statement.
// The EN 16612 pane-resistance receipt for the GOVERNING pane. BendingKnmPerM is a per-metre-STRIP moment resistance,
// which is a resistance and not a capacity: it is the moment a unit strip of the pane can carry, and turning it into
// a pane's load capacity needs the plate's dimensions, aspect ratio, and support condition — none of which a glazing
// BUILD carries, because they belong to the opening the build is installed in. The column therefore states what it
// is, the placement supplies the rest, and the basis it was derived under rides beside it so a consumer reads which
// code's partial factors and which load duration produced the number.
public readonly record struct GlassCapacity(
    GlassBasis Basis, double ResistanceMpa, double EffectiveThicknessMm, double StripBendingKnmPerM,
    double Kmod, double LoadShareFraction);

// The GLAZING DESIGN BASIS — the jurisdiction row the pane resistance binds, in place of two frozen partial factors.
// EN 16612 publishes γ_M,A and γ_M,v as PROPOSED values and states in its own introduction that they, together with
// k_mod and k_e, are subject to national determination; national codes exercise that, and the second-generation
// technical specification lowers γ_M,A again where wind dominates. Freezing 1.8 and 1.2 into the kernel asserted one
// jurisdiction's numbers as physics, so the numbers are a ROW and a second jurisdiction is one more row rather than a
// kernel edit. The keys mirror the capacity#SECTION_CAPACITY DesignBasis roster for the glazing altitude.
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
// the prestress term does not exist for a glass that was never prestressed. k_sp and k_v are the panes' own published
// columns rather than frozen ones. k_mod is the duration relation 0.663·t_h^(−1/16), bounded at 1.0 above and at the
// code's own 0.25 floor for normal building loads. The laminated effective thickness is the ω-weighted interpolation
// between the NON-SHEAR bound ∛(Σ h_k³) and the fully-coupled Σ h_k, ω the Interlayer row's own provenance-gated
// column. The IGU LOAD SHARE is derived here: a sealed cavity couples the panes, so each pane draws its stiffness
// fraction of the applied pressure and the GOVERNING pane is the worst resistance-per-share, not the weakest pane.
public static class GlazingStructural {
    const double KmodFloor = 0.25;         // EN 16612: k_mod is not taken below 0.25 for normal building loads
    const double KmodCoefficient = 0.663;
    const double StressCorrosionExponent = 16.0;

    // The characteristic bending strength of ANNEALED glass, which every prestressed row's second term measures its
    // own strength against. It is the Float row's own published column read once — the same number spelled a second
    // time as a kernel constant beside the table that owns it is a value with two editors.
    static double AnnealedFgkMpa => GlassType.Float.CharacteristicBendingMpa;

    public static Fin<GlassCapacity> Capacity(
        Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double loadDurationS,
        GlassBasis basis, double edgeFactor, Op key) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes, key)
        from timed in guard(double.IsFinite(loadDurationS) && loadDurationS > 0.0,
            ComponentFault.Capacity(key, $"<glass-load-duration-rejected:{loadDurationS:R}>"))
        from edged in guard(double.IsFinite(edgeFactor) && edgeFactor is > 0.0 and <= 1.0,
            ComponentFault.Capacity(key, $"<glass-edge-factor-out-of-range:{edgeFactor:R}>"))
        let kmod = Math.Clamp(KmodCoefficient * Math.Pow(loadDurationS / 3600.0, -1.0 / StressCorrosionExponent), KmodFloor, 1.0)
        let shares = LoadShare(panes)
        select panes.Map((pane, index) => PaneCapacity(pane, basis, kmod, edgeFactor, shares[index]))
            .MinBy(static c => c.StripBendingKnmPerM / c.LoadShareFraction);

    // EN 16612 §5 INSULATING-UNIT load sharing: the sealed cavity couples the panes, so an external pressure on one
    // pane is carried by the whole unit in proportion to each pane's own BENDING STIFFNESS — δ = h_ef³ for a plate of
    // the same span. The governing pane is therefore the one whose demand-to-resistance ratio is worst once its SHARE
    // of the pressure is applied, not simply the weakest pane: a 4 mm outboard lite beside a 10 mm inboard lite draws
    // only its stiffness fraction of the load, and treating it as carrying the whole pressure under-rates the unit.
    // The fraction is derived from the pane geometry the build already carries — no new column, no placement input.
    static Seq<double> LoadShare(Seq<Pane> panes) {
        Seq<double> stiffness = panes.Map(static pane => Math.Pow(EffectiveThicknessMm(pane), 3.0));
        double total = stiffness.Sum();
        return total > 0.0 ? stiffness.Map(k => k / total) : panes.Map(_ => 1.0 / Math.Max(panes.Count, 1));
    }

    // The EN 16612 effective thickness for bending, ω-weighted: a monolithic pane is its own glass thickness, and a
    // laminate interpolates between the NO-SHEAR cube-sum lower bound ∛(Σ h_k³) at ω = 0 and the fully-coupled
    // monolithic upper bound Σ h_k at ω = 1. The interlayer row owns ω and its provenance, so the code default stays
    // the conservative bound until a declared EN 16613 family earns otherwise — and the formula never changes.
    static double EffectiveThicknessMm(Pane pane) {
        if (!pane.IsLaminated) { return pane.ThicknessMm.Value; }
        double ply = pane.GlassThicknessMm / 2.0;
        double unshear = Math.Cbrt(2.0 * Math.Pow(ply, 3.0));
        return unshear + pane.Interlayer.Omega * (pane.GlassThicknessMm - unshear);
    }

    // One pane at its own load share: W = 1000·h_ef²/6 per metre strip; M_Rd = f_g,d·W. The design strength dispatches
    // on the pane's OWN strengthening column — an annealed glass takes the edge factor and no prestress term, a
    // prestressed glass takes the prestress term and no edge factor.
    static GlassCapacity PaneCapacity(Pane pane, GlassBasis basis, double kmod, double edgeFactor, double share) {
        double hef = EffectiveThicknessMm(pane);
        double annealed = kmod * pane.Glass.SurfaceProfileFactor * AnnealedFgkMpa / basis.GammaAnnealed;
        double fgd = pane.Glass.StrengtheningFactor.Match(
            Some: kv => annealed + kv * (pane.Glass.CharacteristicBendingMpa - AnnealedFgkMpa) / basis.GammaPrestressed,
            None: () => edgeFactor * annealed);
        return new GlassCapacity(basis, fgd, hef, fgd * (1000.0 * hef * hef / 6.0) * 1e-6, kmod, share);
    }
}

// The IGU service-life receipt — the TIME dimension of the same build rows: the EN 1279-3 gas-retention decay re-enters the
// ONE resistance chain (fill fraction × 0.99^years, the ≤ 1 %/yr certification cap as the declared worst case; a vacuum
// cavity carries no declared decay law and re-evaluates unchanged), and the EN ISO 13788 temperature factor derives off the
// decayed Ug as fRsi = 1 − Ug·Rsi (Rsi = 0.25 m²·K·W⁻¹, the condensation-risk surface resistance) — the cold-climate
// condensation verdict is the placement comparison of fRsi against the climate's required factor. Never stored: derived
// from the bag-carried inputs at any year.
// Aged carries the WHOLE re-evaluated performance receipt, not a lone Ug: the decayed chain is one Evaluate, and the
// acoustic spectrum, g, and τv it also computes are the same receipt at the same age — a second Ug column beside the
// receipt that produced it would be the stored-scalar drift this page forbids. At years = 0 the retention is exactly
// 1.0, so the year-zero receipt IS the ambient evaluation and the two folds collapse to ONE.
public readonly record struct GlazingService(GlazingPerformance Aged, double FRsi, double FillFractionRemaining);

public static class GlazingLifetime {
    const double RsiCondensationM2KPerW = 0.25;
    const double GasRetentionPerYear = 0.99;   // EN 1279-3: Li ≤ 1.0 %/yr

    public static Fin<GlazingService> AtYears(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double years, CavityTilt tilt, Op key) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes, key)
        from aged in guard(double.IsFinite(years) && years >= 0.0, ComponentFault.Family(key, $"<glazing-service-years-rejected:{years:R}>"))
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

// The ONE glazing-build admission and its three seed-time projections: one Admit law gates the stack at every ingress
// door — Of (the seed) builds the DetailSchema.Product bag (build inputs — the seam-declared PaneBuild/CavityBuild/
// SpacerType/EdgeSeal/MuntinGrid/FireResistanceEi rows); Plies builds the SectionProfile.Layered geometry (each
// Ply.Role the bounded PlyRole row the CompositionAuthor LayerSet bridge derives the IfcMaterialLayer.Name
// from); Properties (the projector) lowers the computed receipt into the seam MaterialPropertySet set attached
// to the IGU material. Receipts are COMPUTED from the bag-carried inputs — never a stored scalar that drifts.
public static class GlazingDetail {
    const double VacuumIntegrityThresholdPa = 0.1;   // ISO 19916 functional-vacuum ceiling — above it the VIG is compromised

    // The EN 15804 generic-IGU EPD identity rides the seam evidence axis — the per-case epd/validUntilYear columns
    // are the deleted seam form.
    static readonly PropertyEvidence GenericEpd = new("epd", "en 15804 generic insulating glass unit", Option<LocalDate>.None);

    // The ONE stack gate law: panes = cavities + 1 (an IGU alternates pane/cavity/pane), the pane count one the
    // GlazingBuild vocabulary names, a non-negative EI, per-pane interlayer EXACTNESS (None pairs
    // with thickness exactly 0 — a negative thickness cannot hide behind None — a present interlayer with thickness in
    // (0, pane total)), and per-cavity fill sanity (gas fraction in (0,1]; vacuum pressure in (0, 0.1 Pa]). A violation
    // rails ComponentFault.Family rather than seeding a unit whose DERIVED Build mislabels.
    //
    // Fire resistance is NOT gated against a pane's substance, because it is not a substance property. An EI rating is
    // awarded to a TESTED SYSTEM — a specific glass in a specific frame with a specific glazing method — and the
    // previous roster expressed it as a "fire-rated" glass row whose optical, thermal, and density columns were plain
    // borosilicate, then required that row's presence to justify a positive EI. That made a tested-system property
    // into a material identity, forced every fire-rated build to name a substance it was not made of, and left a real
    // borosilicate pane with no honest row of its own. The substance roster now names borosilicate as what it is, and
    // the EI minutes ride the BUILD as the tested-assembly evidence they are, absent by default.
    internal static Fin<Unit> Admit(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, Op key) =>
        panes.IsEmpty || cavities.Count != panes.Count - 1
            ? ComponentFault.Family(key, $"<glazing-stack-arity:panes={panes.Count}:cavities={cavities.Count}>")
            : GlazingBuild.OfPaneCount(panes.Count).IsNone
                ? ComponentFault.Family(key, $"<glazing-build-unmodeled-pane-count:{panes.Count}>")
                : fireEiMinutes < 0
                    ? ComponentFault.Family(key, $"<glazing-fire-rating-negative:ei={fireEiMinutes}>")
                    : panes.Find(static p => !double.IsFinite(p.InterlayerThicknessMm) || (p.Interlayer == Interlayer.None
                            ? p.InterlayerThicknessMm != 0.0
                            : p.InterlayerThicknessMm <= 0.0 || p.InterlayerThicknessMm >= p.ThicknessMm.Value)).Match(
                        Some: p => Fin.Fail<Unit>(ComponentFault.Family(key, $"<glazing-interlayer-inconsistent:{p.Interlayer.Key}:{p.InterlayerThicknessMm:R}>")),
                        None: () => cavities.Find(static c => c.Fill.Switch(
                                gasFill: static g => !double.IsFinite(g.FillFraction) || g.FillFraction is <= 0.0 or > 1.0,
                                vacuumFill: static v => !double.IsFinite(v.ResidualPressurePa) || v.ResidualPressurePa is <= 0.0 or > VacuumIntegrityThresholdPa)).Match(
                            Some: c => Fin.Fail<Unit>(ComponentFault.Family(key, $"<glazing-cavity-fill-out-of-range:{c.WidthMm.Value:R}mm>")),
                            None: () => Fin.Succ(unit)));

    // The seed door: the shared Admit law, the degenerate-grid gate (a PRESENT grid owns at least one non-negative
    // bar), then the Product bag.
    public static Fin<PropertyBag> Of(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes, Provenance source, Op key) =>
        from admitted in Admit(panes, cavities, fireEiMinutes, key)
        from grid in guard(muntin.ForAll(static m => m.HorizontalBars >= 0 && m.VerticalBars >= 0 && m.HorizontalBars + m.VerticalBars > 0),
            ComponentFault.Family(key, "<glazing-muntin-degenerate>"))
        from bag in Bag(panes, cavities, spacer, edgeSeal, muntin, fireEiMinutes, source)
        select bag;

    // The projector door: the seam MaterialPropertySet set the IGU material carries — Thermal the EN 673 Ug + the
    // series-harmonic glass conductivity + the mass-weighted specific heat (EN 572-1 soda-lime 720, borosilicate
    // 830 J·kg⁻¹·K⁻¹) + the vapour-tight μ (EN ISO 13788 μ → ∞ for a sealed IGU); Acoustic the banded spectrum (Rw a
    // derived read); Environmental the substance/process-split per-m² GWP under the GenericEpd evidence; Fire the
    // parameterized EN 13501-2 EI rating where EI minutes are positive.
    // serviceYears is the DECLARED SERVICE AGE the whole receipt evaluates at: the EN 1279-3 gas-retention decay is a
    // property of the unit's own certification cap, so the aged state is a re-derivation through the ONE resistance
    // chain, never a stored decay curve. `None` reads year zero, whose retention is exactly 1.0, so the ambient
    // lowering is the same fold with the same bytes and the two evaluations collapse to ONE — an aged IGU lowers its
    // decayed Ug onto the seam Thermal case, giving the computed service receipt its first consumer and a
    // cold-climate condensation verdict a real input. The receipt's fRsi is DERIVED from that Ug (fRsi = 1 − Ug·Rsi),
    // so it needs no seam column of its own; the seam Durability case is the fib Model Code concrete service-life
    // carrier (carbonation K, chloride D_RCM, ageing exponent α) and carries no glazing quantity, so a gas-retention
    // fraction lowered into it would be three columns asserting quantities they do not hold.
    public static Fin<Seq<MaterialPropertySet>> Properties(
        Seq<Pane> panes, Seq<Cavity> cavities, EdgeSeal seal, SpacerType spacer, double perimeterToAreaRatio,
        int fireEiMinutes, CavityTilt tilt, Op key, Option<double> serviceYears = default) =>
        // AtYears IS the ingress gate — its first clause is this page's one Admit over the same stack, so a second
        // call here re-admits what the rail already proved.
        from service in GlazingLifetime.AtYears(panes, cavities, fireEiMinutes, serviceYears.IfNone(0.0), tilt, key)
        let perf = service.Aged
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: GlassConductivity(panes),
            specificHeat: GlassSpecificHeat(panes),
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6,
            key)
        // Recycled content and end-of-life recovery are ABSENT rather than asserted: the previous call declared a
        // quarter recycled and ninety percent recovered for every build in the roster, which is a claim about a
        // supply chain and a waste stream that no build column carries and no declaration backed. They land with the
        // EPD ingestion, per family, or they do not land.
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
    // near-glass transparent polymer whose laminate identity rides the Role, not a fabricated polymer appearance row);
    // a cavity the gas.cavity ply (a vacuum cavity an IsVentilated-false sealed gap at the Bim edge). Ply.Role is the
    // BOUNDED PlyRole row (Pane/Interlayer/Cavity); the human-readable IfcMaterialLayer.Name derives at the boundary
    // from (Material, Role, ordinal), and the build identity (glass, coating, gas, fill) rides the Product bag —
    // never a parsed layer-name string. The sub-ply half-thickness and interlayer lifts rail on the dimensional band.
    // INTERNAL: the slot walk indexes both runs off one alternating ordinal, which is total only over an admitted
    // stack — the ONE Admit gate every public ingress door composes proves the alternation before this is reached.
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
    // sub-rows, the SpacerType token, the EdgeSeal complex, the optional MuntinGrid complex (omitted rows content-key a
    // gridless unit distinctly), and the EI minutes (SI seconds over the time dimension). Dimensional rows ride the
    // DIMENSION-only MeasureValue.OfSi so an authored and an imported bag content-key identically; discrete
    // indices/counts ride Text tokens (PropertyValue carries no integer case).
    static Fin<PropertyBag> Bag(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes, Provenance source) =>
        from paneRows in toSeq(Enumerable.Range(0, panes.Count)).Traverse(i => PaneComplex(panes[i], i)).As()
        from cavityRows in toSeq(Enumerable.Range(0, cavities.Count)).Traverse(i => CavityComplex(cavities[i], i)).As()
        from muntinRows in muntin.Match(Some: MuntinRows, None: static () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))
        from fireRows in fireEiMinutes > 0
            ? Measured(DetailSchema.FireResistanceEi, Dimension.Create(0, 0, 1, 0, 0, 0, 0), fireEiMinutes * 60.0).Map(static row => Seq(row))
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

    // The bare dimension-only Measure value the recursive Complex sub-rows carry (the tuple-returning ComponentDetail
    // Measured serves top-level bag rows only).
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

    // The per-face coating rows carry the wire truth directly — one token per physical face, "none" the uncoated state.
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
// The glazing family seed — the EN 1279 IGU builds. Each build is a distinct engineering unit — a typed row, never a
// generator target. ComponentFamily.Glazing binds Rows; a Layered profile answers unsectioned membership from its own
// topology, so the IGU reaches SectionSolver from no seed assertion at all.
public static class GlazingSeed {
    // The BUILD is this estate's own composition — which panes, which cavity, which coating stack — even though every
    // column it composes is a published EN 673 / EN 410 / ISO 9050 datum, and no vendor producer publishes the build.
    // That is what keeps ComponentCatalogue.AdmitImported from handing a vendor IGU type this geometry as though a
    // standards body had published the unit.
    static readonly Provenance Built = Provenance.Authored;

    // ComponentAuthority.En (EN 1279 IGU authority, region "eu"); an IGU lays no mortar joint. Every current build
    // ships the standard PIB primary + polysulfide secondary + molecular-sieve + keyed-corner EN 1279-2 construction;
    // a structural-glazing row swaps its own EdgeSeal column to a silicone secondary, never a parallel row shape.
    static readonly ComponentStandard IguStandard = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    // No seeded build has been through an EN 1279-2 climate test, so every row carries the index as absence — the one
    // honest statement about a quantity only a tested system possesses.
    static readonly EdgeSeal StandardEdgeSeal = new(Sealant.Pib, Sealant.Polysulfide, Desiccant.MolecularSieve3A, CorneredKeys: true, Moisture: None);
    static readonly Option<MuntinGrid> NoGrid = Option<MuntinGrid>.None;
    static readonly Cavity Argon16 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(16.0));
    static readonly Cavity Argon12 = new(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(12.0));

    static Pane Mono(GlassType glass, double thicknessMm, Coating outboard, Coating inboard) =>
        new(glass, PositiveMagnitude.Create(thicknessMm), outboard, inboard, Interlayer.None, 0.0);
    static Pane Clear(GlassType glass, double thicknessMm) => Mono(glass, thicknessMm, Coating.None, Coating.None);

    // The EN 1279 builds as full typed pane/cavity sub-rows — the asymmetric, inboard-coated, laminated, vacuum,
    // fire-rated, and gridded units each their own rows, never a designation suffix the model leaves unmodeled. Every
    // coating names its physical FACE (outboard/inboard argument position); surface numbers annotated where load-bearing.
    static readonly Seq<GlazingRow> Builds = Seq(
        new GlazingRow("glazing.double-4-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-6-12-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 6.0), Clear(GlassType.Float, 6.0)), Seq(Argon12), 0, NoGrid),
        new GlazingRow("glazing.double-4-20-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Argon, 0.90, CavityGas.Air), PositiveMagnitude.Create(20.0))), 0, NoGrid),
        // The inboard pane carries a soft-coat double-silver low-E on its OUTBOARD face (surface 3, cavity-facing) —
        // its OutboardCoating row, so Pane.EmissivityOf(inboard: false) reads the εn 0.04 the cavity radiative term sees.
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
        // Triple low-E on surfaces 2 and 5 (outer pane INBOARD face + inner pane OUTBOARD face) — each cavity sees one
        // low-E surface; krypton for the narrow gaps.
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
        // on surface 2 (cavity-facing) — two independent per-face rows on ONE pane, the build a single-coating
        // shape cannot spell.
        new GlazingRow("glazing.double-6sol2lowe-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 6.0, Coating.SolarControl, Coating.SoftCoatTriple), Clear(GlassType.Float, 6.0)), Seq(Argon16), 0, NoGrid),
        // ISO 19916 vacuum unit: soft-coat triple-silver on surface 2 suppressing the now-dominant radiative exchange; a
        // 0.3 mm gap at 0.08 Pa with 0.25 mm-radius pillars on a 20 mm pitch (the Collins conduction the kernel reads).
        new GlazingRow("glazing.vig-4lowe-vac-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatTriple), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.VacuumFill(0.08, PositiveMagnitude.Create(0.25), PositiveMagnitude.Create(20.0)), PositiveMagnitude.Create(0.3))), 0, NoGrid),
        // Fire-rated EI 30: a 6 mm borosilicate outboard pane (the fire side) + a 6 mm float with low-E on surface 3; the
        // positive EI the gate requires of a fire-rated pane drives the OfFire(A1, Ei(30)) lowering.
        new GlazingRow("glazing.fire-ei30-6fr-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Borosilicate, 6.0), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble, Coating.None)), Seq(Argon16), 30, NoGrid),
        // True-divided grid: one horizontal + two vertical 25×20 mm muntin bars (manufacturer dims) — face geometry the
        // generator places across the pane.
        new GlazingRow("glazing.double-4-16-4-grid", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)), Seq(Argon16), 0,
            Some(new MuntinGrid(MuntinStyle.TrueDivided, 1, 2, PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(20.0)))));

    static readonly FrozenDictionary<ComponentId, GlazingRow> Table =
        Builds.ToFrozenDictionary(static row => ComponentId.Create(row.Designation), static row => row);

    public static Fin<GlazingRow> Resolve(Component component, Op key) =>
        Table.TryGetValue(component.Designation, out GlazingRow row)
            ? Fin.Succ(row)
            : ComponentFault.Family(key, $"<glazing-row-unregistered:{component.Designation.Value}>");

    // One row -> one ComponentRow: GlazingDetail.Of admits the build ONCE (the bag), Evaluate gates the physics (a build
    // whose spectrum cannot admit never seeds), Plies + Layered.Of rail the geometry (WidthMm = OverallMm preserving the
    // square gross projection), Component.Of seals family/lane/laminate invariants. Substance and appearance both resolve
    // to the outboard pane's glass row (the IGU's engineering receipt rides the material's own property set, so the
    // capacity slot coincides with the appearance row rather than a separate Mechanical key).
    static Fin<ComponentRow> Row(GlazingRow r, Context context) =>
        from bag in GlazingDetail.Of(r.Panes, r.Cavities, r.Spacer, r.EdgeSeal, r.Muntin, r.FireResistanceEiMinutes, Built, context.Key)
        from perf in GlazingThermal.Evaluate(r.Panes, r.Cavities, CavityTilt.Vertical, context.Key)
        from plies in GlazingDetail.Plies(r.Panes, r.Cavities, context.Key)
        let overallMm = r.Panes.Sum(static p => p.ThicknessMm.Value) + r.Cavities.Sum(static c => c.WidthMm.Value)
        from profile in SectionProfile.Layered.Of(plies, overallMm: overallMm, widthMm: overallMm, context.Key)
        from item in Component.Of(
            ComponentFamily.Glazing, r.Designation, profile, ComponentFamily.Glazing.Ifc,
            // The two slots stay INDEPENDENT per the two-slot law: the SUBSTANCE is the outboard pane's own glass
            // material — the row a property read resolves mechanical and thermal facts through — while the
            // APPEARANCE is the library shade it renders as. Binding both to the appearance row made the IGU's
            // material identity its render identity, so a low-iron and a soda-lime unit shading to the same crown
            // row became one substance and any property read keyed on it answered for the wrong glass.
            Coring.None, IguStandard, substanceId: r.Panes[0].Glass.Substance, appearanceId: r.Panes[0].Glass.Appearance,
            detail: Some(bag), context.Key)
        select new ComponentRow(item, Built);

    // The family fold ComponentFamily.Glazing binds: Traverse is the rail — a malformed build ABORTS the catalogue,
    // never a swallowed Choose drop.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Builds.Traverse(row => Row(row, context)).As();

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
        select SectionCapacity.Lift(new CapacityReceipt.Glass(component.Designation, capacity));
}
```

## [03]-[RESEARCH]

(none)
