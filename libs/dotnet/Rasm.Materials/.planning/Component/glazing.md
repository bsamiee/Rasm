# [MATERIALS_GLAZING]

THE GLAZING SEED PAGE — the `glazing` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Product`) grounded in insulating-glass build physics. An IGU is a `Component` whose `SectionProfile.Layered` geometry contains only `PlyRole.Pane`/`Interlayer`/`Cavity`, whose build inputs ride the `DetailSchema.Product` bag, and whose engineering performance derives from the typed `GlazingRow`: `GlazingThermal` owns EN 673 `Ug`, EN 410 / ISO 9050 `g` and `τv`, and the mass-law acoustic spectrum; `GlazingStructural` owns the EN 16612 pane resistance the capacity pipeline lifts; `GlazingLifetime` owns the EN 1279-3 gas-decay and EN ISO 13788 `fRsi` service-life row; `GlazingGwp` owns the lifecycle vector; `GlazingDetail.Properties` lowers the performance row to `MaterialPropertySet`. An IGU crosses as `IfcMaterialLayerSet`, never `IfcProfileDef`, so its `Layered` profile answers unsectioned membership from its own `ProfileTopology`. `GlazingSeed.Resolve` joins a resolved `ComponentId` back to its pane, cavity, edge, grid, and fire axes so the projector can execute the promised lowering without parsing the bag or the designation.

The ONE stack-admission law is `GlazingDetail.Stack`, an ACCUMULATING census: arity, modeled pane count, EI sign, per-pane interlayer exactness, and per-cavity fill sanity are five INDEPENDENT proofs, so a malformed build names every column it broke in one verdict. The seed lifts it as its `SeedLaw` coherence (where it now spans the whole roster rather than aborting on the first bad build) and the capacity and service doors lift the same census on the `Fin` result — one law, three ingresses, no re-admission anywhere.

## [01]-[INDEX]

- [02]-[GLAZING_FAMILY]: the glazing policy vocabularies, `CavityFill`, the typed build rows, the shared `GlazingThermal` resistance/optical/acoustic kernel, `GlazingPerformance`, the `GlazingStructural` EN 16612 pane-resistance kernel with its `GlassCapacity`, the `GlazingLifetime` service-life row, `GlazingGwp`, `GlazingDetail`, and the `GlazingSeed` roster with its seed law and `SeedJoin` resolver.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing policy vocabulary; `CavityFill` the gas-vs-vacuum `[Union]`; `Pane`/`Cavity`/`EdgeSeal`/`MuntinGrid` the typed stack rows; `GlazingThermal` the shared resistance, optical, and acoustic kernel; `GlazingGwp` the lifecycle vector; `GlazingPerformance` the computed performance row; `GlazingDetail` the shared stack census, bag, property, and ply operations; `GlazingSeed` the EN 1279 roster, its seed law, and the typed resolver.
- Cases: the glazing vocabulary spans the glass, per-face coating, gas, interlayer, spacer, and edge-seal axes. `GlazingBuild` derives `Double`, `Triple`, or `Quadruple` from `Panes.Count`; stack arity and finite pane/cavity values admit before either physics boundary runs.
- Entry: `ComponentSeed.Rows(context, GlazingSeed.Roster, GlazingSeed.Law)` — the law's coherence is the stack census plus the grid gate, its profile the ply projection onto `Layered`, and its detail the performance gate before the Product bag, so a build whose spectrum cannot admit never seeds and one malformed row aborts the catalogue. `GlazingSeed.Resolve(Component, Op)` restores the typed build axes through the shared `SeedJoin` result. `GlazingDetail.Properties(panes, cavities, ei, key, serviceYears)` lowers `Thermal`/`Acoustic`/`Environmental`/`Fire` as one result AT the declared service age (`None` reads year zero).
- Packages: Rasm.Numerics (`PositiveMagnitude` — every pane/gap/pillar/bar column), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `MaterialPropertySet`, `MeasureValue`, `Dimension`, `MeasurementBasis`, `LifecycleStage`, `Acoustic`, `AcousticBand`, `FireRating`, `FireResistance`, `DetailSchema`, `PropertyValue`, `PropertyName`, `PropertyBag`), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED`/`#QUANTITY_ROW` owners, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime (`LocalDate` — the EPD evidence expiry axis), UnitsNet (`RatioUnit.DecimalFraction` — the dimensionless `g`/`τv` contract admission), VividOrange.Uncertainties + VividOrange.Uncertainties.Quantities (`WithRelativeUncertainty`, `IUncertainty<HeatTransferCoefficient>.LowerBound`/`UpperBound` — the typed `Ug` model band lowered onto `MeasureBand`). VividOrange.Materials is NOT composed (glazing fills no profile solve). Wacton.Unicolour is NOT composed: a coating's OPTICAL signal crosses as the coated pane's content-keyed `Node.Appearance`; glazing tags the `MaterialId`, never the colour kernel.
- Growth: a new IGU is one `GlazingRow`; a new glass substance one `GlassType` row; a new coating tier one `Coating` row; a new gas one `CavityGas` row; a new interlayer one `Interlayer` row; a new edge-seal chemistry one `Sealant`/`Desiccant` row; a quad build one `GlazingBuild` row the derived `Build` read maps; an electrochromic variant a `GlassType` row plus a `Coating` row. The full per-wavelength `τ(λ)`/`ρ(λ)` angular EN 410 §5 spectral integral is a `GlassType`/`Coating` per-wavelength-curve column growth the broadband recursion here is the center-of-glass simplification of, never a parallel optical owner.
- Boundary: `SectionProfile.Layered` is the geometric gross only; `ComponentFamily.Glazing.Admits` rejects every non-glazing `PlyRole`, and physics reads the typed `Pane`/`Cavity` rows restored through `GlazingSeed.Resolve`, never re-parsed plies or bag text. `GlazingThermal.Evaluate` is INTERIOR over a census-gated stack and computes one ordered resistance chain shared by `Ug` and the EN 410 inward-flowing secondary flux. `QuantityRow.HeatTransferCoefficient.OfNative` owns the `Ug` mint, while dimension-only bag rows use `MeasureValue.OfSi(Dimension, si)`. `SpacerType.PsiWmK` feeds the Compute-owned whole-window aggregation. The IFC layer name derives from `(Material, Role, ordinal)`, coating stays face data, and `MuntinGrid` stays face geometry.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassType {
    public static readonly GlassType Float            = new("float",             normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 1.00, strengtheningFactor: None,       safety: false, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType LowIron          = new("low-iron",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.50, formProcessGwpPerM2: 0.0, solarTransmittance: 0.90, solarReflectance: 0.080, visibleTransmittance: 0.91, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 1.00, strengtheningFactor: None,       safety: false, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType Patterned        = new("patterned",         normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.0, solarTransmittance: 0.78, solarReflectance: 0.075, visibleTransmittance: 0.85, visibleReflectance: 0.08, characteristicBendingMpa: 45.0, surfaceProfileFactor: 0.75, strengtheningFactor: None,       safety: false, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType HeatStrengthened = new("heat-strengthened", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 0.9, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 70.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: false, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType Tempered         = new("tempered",          normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: true, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType TemperedVertical = new("tempered-vertical", normalEmissivity: 0.837, conductivityWmK: 1.00, densityKgM3: 2500.0, specificHeatJKgK: 720.0, substanceGwpPerKg: 1.43, formProcessGwpPerM2: 1.2, solarTransmittance: 0.82, solarReflectance: 0.075, visibleTransmittance: 0.90, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(0.6), safety: true, appearance: MaterialId.Create("glass.crown"));
    public static readonly GlassType Borosilicate     = new("borosilicate",      normalEmissivity: 0.837, conductivityWmK: 1.14, densityKgM3: 2230.0, specificHeatJKgK: 830.0, substanceGwpPerKg: 2.00, formProcessGwpPerM2: 5.0, solarTransmittance: 0.70, solarReflectance: 0.070, visibleTransmittance: 0.85, visibleReflectance: 0.08, characteristicBendingMpa: 120.0, surfaceProfileFactor: 1.00, strengtheningFactor: Some(1.0), safety: true, appearance: MaterialId.Create("glass.flint"));
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
    public double CharacteristicBendingMpa { get; }
    public double SurfaceProfileFactor { get; }
    public Option<double> StrengtheningFactor { get; }
    public bool Safety { get; }

    public MaterialId Appearance { get; }
    public MaterialId Substance => MaterialId.Create($"glass.{Key}");
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
    public double Omega { get; }
    public EvidenceGrade OmegaSource { get; }
}

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Desiccant {
    public static readonly Desiccant MolecularSieve3A = new("molecular-sieve-3a", adsorptionCapacity: 0.22);
    public static readonly Desiccant Silica           = new("silica",             adsorptionCapacity: 0.30);

    public double AdsorptionCapacity { get; }

    public Option<double> StandardCapacity { get; }

    public double CapacityFraction => StandardCapacity.IfNone(AdsorptionCapacity);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MuntinStyle {
    public static readonly MuntinStyle TrueDivided      = new("true-divided");
    public static readonly MuntinStyle SimulatedDivided = new("simulated-divided");
    public static readonly MuntinStyle BetweenGlass     = new("between-glass");
}

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

[Union]
public abstract partial record CavityFill {
    public sealed record GasFill(CavityGas Gas, double FillFraction, CavityGas Balance) : CavityFill;
    public sealed record VacuumFill(double ResidualPressurePa, PositiveMagnitude PillarRadiusMm, PositiveMagnitude PillarPitchMm) : CavityFill;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GasProperties(double ConductivityWmK, double DensityKgM3, double ViscosityPaS, double SpecificHeatJKgK);

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

public readonly record struct MoisturePenetration(double InitialFraction, double FinalFraction, double CapacityFraction) {
    public const double AverageCeiling = 0.20;
    public const double IndividualCeiling = 0.25;

    public double Index => (FinalFraction - InitialFraction) / (CapacityFraction - InitialFraction);
    public bool Conforms => Index <= IndividualCeiling;

    public static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, Desiccant desiccant) =>
        Of(initialFraction, finalFraction, desiccant.CapacityFraction);

    static Fin<MoisturePenetration> Of(double initialFraction, double finalFraction, double capacityFraction) =>
        from finite in guard(double.IsFinite(initialFraction) && double.IsFinite(finalFraction) && double.IsFinite(capacityFraction),
            new KernelFault.InvalidValue(nameof(MoisturePenetration), "finite fractions"))
        from ordered in guard(initialFraction >= 0.0 && initialFraction < capacityFraction && finalFraction >= initialFraction,
            new KernelFault.InvalidValue(nameof(MoisturePenetration), "ordered non-negative fractions below capacity"))
        select new MoisturePenetration(initialFraction, finalFraction, capacityFraction);
}

public readonly record struct EdgeSeal(Sealant Primary, Sealant Secondary, Desiccant Desiccant, bool CorneredKeys, Option<MoisturePenetration> Moisture);

public readonly record struct MuntinGrid(MuntinStyle Style, int HorizontalBars, int VerticalBars, PositiveMagnitude BarWidthMm, PositiveMagnitude BarDepthMm);

public readonly record struct GlazingPerformance(
    MeasureValue UgCenterOfGlass,
    MeasureValue SolarFactorG,
    MeasureValue LightTransmittanceTv,
    Acoustic Acoustic,
    EvidenceGrade AcousticSource) {
    public int Rw => Acoustic.Rw;

    public double LightToSolarGain => SolarFactorG.Si > 0.0 ? LightTransmittanceTv.Si / SolarFactorG.Si : 0.0;
}

public readonly record struct GlazingRow(string Designation, SpacerType Spacer, EdgeSeal EdgeSeal, Seq<Pane> Panes, Seq<Cavity> Cavities, int FireResistanceEiMinutes, Option<MuntinGrid> Muntin) {
    public EvidenceGrade Source { get; init; } = EvidenceGrade.User;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GlazingThermal {
    const double SurfaceExternalWmK = 23.0;
    const double SurfaceInternalWmK = 8.0;
    const double StefanBoltzmann = 5.67e-8;
    const double MeanTemperatureK = 283.0;
    const double TemperatureDeltaK = 15.0;
    const double GravityMs2 = 9.81;
    const double MassLawOffsetDb = 47.0;
    const double FreeMolecularConductanceAirPerPa = 1.2;
    const double ThermalModelRelativeUncertainty = 0.05;

    internal static Fin<GlazingPerformance> Evaluate(Seq<Pane> panes, Seq<Cavity> cavities, CavityTilt tilt) {
        double[] rPane = panes.Map(PaneConductiveResistance).ToArray();
        double[] rCav = new double[cavities.Count];
        for (int i = 0; i < cavities.Count; i++) rCav[i] = 1.0 / CavityConductance(panes, cavities, i, tilt);
        double rse = 1.0 / SurfaceExternalWmK, rsi = 1.0 / SurfaceInternalWmK;
        double rTot = rse + rPane.Sum() + rCav.Sum() + rsi;
        double ug = 1.0 / rTot;
        double g = SolarFactor(panes, rPane, rCav, rTot, rse);
        double tv = Span(panes, 0, panes.Count, static p => p.Visible()).T;
        HeatTransferCoefficient ugQuantity = HeatTransferCoefficient.FromWattsPerSquareMeterKelvin(ug);
        IUncertainty<HeatTransferCoefficient> ugUncertainty = ugQuantity.WithRelativeUncertainty(ThermalModelRelativeUncertainty);
        return from ugMeasure in QuantityRow.HeatTransferCoefficient.OfNative(ug)
               from ugBand in MeasureBand.Admit(UncertaintyKind.Relative,
                   ugUncertainty.LowerBound.WattsPerSquareMeterKelvin, ugUncertainty.UpperBound.WattsPerSquareMeterKelvin,
                   Option<double>.None, Option<double>.None)
               from ugBanded in ugMeasure.WithUncertainty(ugBand)
               from acoustic in MassLawSpectrum(panes, cavities)
               from solarG in MeasureValue.Of(g, UnitsNet.Units.RatioUnit.DecimalFraction)
               from lightTv in MeasureValue.Of(tv, UnitsNet.Units.RatioUnit.DecimalFraction)
               select new GlazingPerformance(ugBanded, solarG, lightTv, acoustic, EvidenceGrade.Defined);
    }

    static double PaneConductiveResistance(Pane p) =>
        (p.GlassThicknessMm / 1000.0) / p.Glass.ConductivityWmK + (p.InterlayerThicknessMm / 1000.0) / p.Interlayer.ConductivityWmK;

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

    static GasProperties EffectiveGas(CavityFill.GasFill gas) => new(
        Mix(gas.Gas.ConductivityWmK, gas.Balance.ConductivityWmK, gas.FillFraction),
        Mix(gas.Gas.DensityKgM3, gas.Balance.DensityKgM3, gas.FillFraction),
        Mix(gas.Gas.ViscosityPaS, gas.Balance.ViscosityPaS, gas.FillFraction),
        Mix(gas.Gas.SpecificHeatJKgK, gas.Balance.SpecificHeatJKgK, gas.FillFraction));

    static double Mix(double fill, double balance, double x) => x * fill + (1.0 - x) * balance;

    static double RadiativeCoefficient(double e1, double e2) =>
        4.0 * StefanBoltzmann * MeanTemperatureK * MeanTemperatureK * MeanTemperatureK / (1.0 / e1 + 1.0 / e2 - 1.0);

    static double Nusselt(GasProperties gas, double s, CavityTilt tilt) {
        double ra = gas.DensityKgM3 * gas.DensityKgM3 * s * s * s * GravityMs2 * gas.SpecificHeatJKgK * TemperatureDeltaK
                    / (MeanTemperatureK * gas.ViscosityPaS * gas.ConductivityWmK);
        return tilt.Convects ? Math.Max(1.0, tilt.Coefficient * Math.Pow(ra, tilt.Exponent)) : 1.0;
    }

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

    static Fin<Acoustic> MassLawSpectrum(Seq<Pane> panes, Seq<Cavity> cavities) {
        double areal = panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 + p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0);
        double bonus = panes.Fold(0.0, static (acc, p) => Math.Max(acc, p.Interlayer.AcousticDampingDb)) + (Asymmetric(panes) ? 2.0 : 0.0);
        Seq<double> resonances = cavities.Map((cavity, index) => {
            double left = PaneArealMass(panes[index]);
            double right = PaneArealMass(panes[index + 1]);
            return 60.0 * Math.Sqrt((left + right) / (left * right * cavity.WidthMm.Value / 1000.0));
        });
        double[] sri = new double[AcousticBand.Items.Count];
        double[] absorption = new double[AcousticBand.Items.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            double resonanceDip = resonances.Fold(0.0, (worst, resonance) => Math.Max(worst, Math.Max(0.0, 8.0 - 6.0 * Math.Abs(Math.Log2(band.CenterHz / resonance)))));
            sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(areal, 1e-9) * band.CenterHz) - MassLawOffsetDb + bonus - resonanceDip);
            absorption[band.Key] = 0.03;
        }
        return Acoustic.Of(absorption, sri);
    }

    static double PaneArealMass(Pane pane) =>
        pane.Glass.DensityKgM3 * pane.GlassThicknessMm / 1000.0 + pane.Interlayer.DensityKgM3 * pane.InterlayerThicknessMm / 1000.0;

    static bool Asymmetric(Seq<Pane> panes) =>
        panes.Count >= 2 && panes.Exists(p => p.ThicknessMm.Value != panes[0].ThicknessMm.Value);
}

public static class GlazingGwp {
    const double IguAssemblyGwpPerM2 = 2.5;

    public static ReadOnlyMemory<Option<double>> StagesPerM2(Seq<Pane> panes, EdgeSeal seal, SpacerType spacer, double perimeterToAreaRatio) {
        double substance = panes.Sum(static p =>
            p.Glass.DensityKgM3 * p.GlassThicknessMm / 1000.0 * p.Glass.SubstanceGwpPerKg
            + (p.IsLaminated ? p.Interlayer.DensityKgM3 * p.InterlayerThicknessMm / 1000.0 * p.Interlayer.SubstanceGwpPerKg : 0.0));
        double processing = panes.Sum(static p =>
            p.Glass.FormProcessGwpPerM2 + p.OutboardCoating.ProcessGwpPerM2 + p.InboardCoating.ProcessGwpPerM2 + (p.IsLaminated ? p.Interlayer.ProcessGwpPerM2 : 0.0))
            + IguAssemblyGwpPerM2;
        double edge = (seal.Primary.ProcessGwpPerM + seal.Secondary.ProcessGwpPerM + spacer.EdgeSealGwpPerM) * perimeterToAreaRatio;
        Option<double>[] stages = new Option<double>[LifecycleStage.Items.Count];
        System.Array.Fill(stages, Option<double>.None);
        stages[LifecycleStage.A1A3.Key] = Some(substance + processing + edge);
        return stages;
    }
}

public readonly record struct GlassCapacity(
    GlassBasis Basis, double ResistanceMpa, double EffectiveThicknessMm, double StripBendingKnmPerM,
    double Kmod, double LoadShareFraction);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlassBasis {
    public static readonly GlassBasis En16612 = new("en16612", gammaAnnealed: 1.8, gammaPrestressed: 1.2);
    public double GammaAnnealed { get; }
    public double GammaPrestressed { get; }
}

public static class GlazingStructural {
    const double KmodFloor = 0.25;
    const double KmodCoefficient = 0.663;
    const double StressCorrosionExponent = 16.0;

    static double AnnealedFgkMpa => GlassType.Float.CharacteristicBendingMpa;

    public static Fin<GlassCapacity> Capacity(
        Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double loadDurationS,
        GlassBasis basis, double edgeFactor) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes)
        from timed in guard(double.IsFinite(loadDurationS) && loadDurationS > 0.0,
            new KernelFault.OutOfRange(nameof(loadDurationS), loadDurationS, "finite and positive"))
        from edged in guard(double.IsFinite(edgeFactor) && edgeFactor is > 0.0 and <= 1.0,
            new KernelFault.OutOfRange(nameof(edgeFactor), edgeFactor, "inside (0, 1]", Some()))
        let kmod = Math.Clamp(KmodCoefficient * Math.Pow(loadDurationS / 3600.0, -1.0 / StressCorrosionExponent), KmodFloor, 1.0)
        let shares = LoadShare(panes)
        select panes.Map((pane, index) => PaneCapacity(pane, basis, kmod, edgeFactor, shares[index]))
            .MinBy(static c => c.StripBendingKnmPerM / c.LoadShareFraction);

    static Seq<double> LoadShare(Seq<Pane> panes) {
        Seq<double> stiffness = panes.Map(static pane => Math.Pow(EffectiveThicknessMm(pane), 3.0));
        double total = stiffness.Sum();
        return total > 0.0 ? stiffness.Map(k => k / total) : panes.Map(_ => 1.0 / Math.Max(panes.Count, 1));
    }

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

public readonly record struct GlazingService(GlazingPerformance Aged, double FRsi, double FillFractionRemaining);

public static class GlazingLifetime {
    const double RsiCondensationM2KPerW = 0.25;
    const double GasRetentionPerYear = 0.99;

    public static Fin<GlazingService> AtYears(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes, double years, CavityTilt tilt) =>
        from admitted in GlazingDetail.Admit(panes, cavities, fireEiMinutes)
        from aged in guard(double.IsFinite(years) && years >= 0.0,
            new KernelFault.OutOfRange(nameof(years), years, "finite and non-negative"))
        let retention = Math.Pow(GasRetentionPerYear, years)
        let decayed = cavities.Map(c => c.Fill is CavityFill.GasFill gas
            ? c with { Fill = new CavityFill.GasFill(gas.Gas, gas.FillFraction * retention, gas.Balance) }
            : c)
        from perf in GlazingThermal.Evaluate(panes, decayed, tilt)
        select new GlazingService(
            perf,
            1.0 - perf.UgCenterOfGlass.Si * RsiCondensationM2KPerW,
            retention);
}

public static class GlazingDetail {
    const double VacuumIntegrityThresholdPa = 0.1;

    static readonly PropertyEvidence GenericEpd = new("epd", "en 15804 generic insulating glass unit", Option<LocalDate>.None);

    internal static Validation<Error, Unit> Stack(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                !panes.IsEmpty && cavities.Count == panes.Count - 1,
                new KernelFault.InvalidValue(nameof(GlazingDetail), "one fewer cavity than panes")),
            AdmissionSlots.Gate(
                !panes.IsEmpty && GlazingBuild.OfPaneCount(panes.Count).IsSome,
                new KernelFault.InvalidValue(nameof(GlazingBuild), "a published pane-count build")),
            AdmissionSlots.Gate(
                fireEiMinutes >= 0,
                new KernelFault.OutOfRange(nameof(fireEiMinutes), fireEiMinutes, "non-negative")),
            AdmissionSlots.Gate(
                panes.ForAll(Coherent),
                new KernelFault.InvalidValue(nameof(panes), "coherent pane and interlayer declarations")),
            AdmissionSlots.Gate(
                cavities.ForAll(Sane),
                new KernelFault.InvalidValue(nameof(cavities), "admitted cavity width and fill fractions"))));

    internal static Fin<Unit> Admit(Seq<Pane> panes, Seq<Cavity> cavities, int fireEiMinutes) =>
        Stack(panes, cavities, fireEiMinutes).ToFin();

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

    public static Fin<Seq<MaterialPropertySet>> Properties(
        Seq<Pane> panes, Seq<Cavity> cavities, EdgeSeal seal, SpacerType spacer, double perimeterToAreaRatio,
        int fireEiMinutes, CavityTilt tilt, Option<double> serviceYears = default) =>
        from service in GlazingLifetime.AtYears(panes, cavities, fireEiMinutes, serviceYears.IfNone(0.0), tilt)
        let perf = service.Aged
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: GlassConductivity(panes),
            specificHeat: GlassSpecificHeat(panes),
            uValue: perf.UgCenterOfGlass.Si,
            vapourResistanceFactor: 1.0e6)
        from environmental in MaterialPropertySet.OfEnvironmental(
            MeasurementBasis.PerM2,
            MaterialPropertySet.Environmental.CarbonMatrix(GlazingGwp.StagesPerM2(panes, seal, spacer, perimeterToAreaRatio)),
            recycledContent: None, endOfLifeRecovery: None, evidence: GenericEpd)
        from fire in fireEiMinutes > 0
            ? FireResistance.Of(FireCoverage.Ei, fireEiMinutes).Map(resistance => Seq(MaterialPropertySet.OfFire(None, resistance)))
            : Fin.Succ(Seq<MaterialPropertySet>())
        let acoustic = MaterialPropertySet.OfAcoustic(perf.Acoustic)
        select Seq(thermal, acoustic, environmental) + fire;

    internal static Fin<Seq<Ply>> Plies(Seq<Pane> panes, Seq<Cavity> cavities) =>
        toSeq(Enumerable.Range(0, panes.Count + cavities.Count))
            .Traverse(slot => (slot & 1) == 0
                ? PanePlies(panes[slot / 2])
                : Fin.Succ(Seq(new Ply(MaterialId.Create("gas.cavity"), cavities[slot / 2].WidthMm, PlyRole.Cavity)))).As()
            .Map(static plies => plies.Bind(static p => p));

    static Fin<Seq<Ply>> PanePlies(Pane pane) =>
        pane.IsLaminated
            ? from half in FactoryBridge.Accept<PositiveMagnitude>(candidate: pane.GlassThicknessMm / 2.0)
              from inter in FactoryBridge.Accept<PositiveMagnitude>(candidate: pane.InterlayerThicknessMm)
              select Seq(
                  new Ply(pane.Glass.Appearance, half, PlyRole.Pane),
                  new Ply(MaterialId.Create("glass.crown"), inter, PlyRole.Interlayer),
                  new Ply(pane.Glass.Appearance, half, PlyRole.Pane))
            : Fin.Succ(Seq(new Ply(pane.Glass.Appearance, pane.ThicknessMm, PlyRole.Pane)));

    internal static Fin<PropertyBag> Bag(Seq<Pane> panes, Seq<Cavity> cavities, SpacerType spacer, EdgeSeal edgeSeal, Option<MuntinGrid> muntin, int fireEiMinutes, EvidenceGrade source) =>
        from paneRows in toSeq(Enumerable.Range(0, panes.Count)).Traverse(i => PaneComplex(panes[i], i)).As()
        from cavityRows in toSeq(Enumerable.Range(0, cavities.Count)).Traverse(i => CavityComplex(cavities[i], i)).As()
        from muntinRows in muntin.TraverseM(MuntinRows).As().Map(static rows => rows.IfNone(Seq<(PropertyName, PropertyValue)>()))
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

    static double GlassConductivity(Seq<Pane> panes) =>
        panes.Sum(static p => p.GlassThicknessMm) / panes.Sum(static p => p.GlassThicknessMm / p.Glass.ConductivityWmK);

    static double GlassSpecificHeat(Seq<Pane> panes) =>
        panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm * p.Glass.SpecificHeatJKgK)
            / panes.Sum(static p => p.Glass.DensityKgM3 * p.GlassThicknessMm);
}

// --- [TABLES] --------------------------------------------------------------------------
public static class GlazingSeed {
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
        new GlazingRow("glazing.double-6-16-6-lowe", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 6.0), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble, Coating.None)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.double-4-12-4-alu", SpacerType.ColdEdgeAluminum, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.GasFill(CavityGas.Air, 1.00, CavityGas.Air), PositiveMagnitude.Create(12.0))), 0, NoGrid),
        new GlazingRow("glazing.double-lam664-16-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(new Pane(GlassType.Float, PositiveMagnitude.Create(6.76), Coating.None, Coating.None, Interlayer.Pvb, 0.76), Clear(GlassType.Float, 4.0)),
            Seq(Argon16), 0, NoGrid),
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
        new GlazingRow("glazing.double-6sol2lowe-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 6.0, Coating.SolarControl, Coating.SoftCoatTriple), Clear(GlassType.Float, 6.0)), Seq(Argon16), 0, NoGrid),
        new GlazingRow("glazing.vig-4lowe-vac-4", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Mono(GlassType.Float, 4.0, Coating.None, Coating.SoftCoatTriple), Clear(GlassType.Float, 4.0)),
            Seq(new Cavity(new CavityFill.VacuumFill(0.08, PositiveMagnitude.Create(0.25), PositiveMagnitude.Create(20.0)), PositiveMagnitude.Create(0.3))), 0, NoGrid),
        new GlazingRow("glazing.fire-ei30-6fr-16-6", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Borosilicate, 6.0), Mono(GlassType.Float, 6.0, Coating.SoftCoatDouble, Coating.None)), Seq(Argon16), 30, NoGrid),
        new GlazingRow("glazing.double-4-16-4-grid", SpacerType.WarmEdgeStainless, StandardEdgeSeal,
            Seq(Clear(GlassType.Float, 4.0), Clear(GlassType.Float, 4.0)), Seq(Argon16), 0,
            Some(new MuntinGrid(MuntinStyle.TrueDivided, 1, 2, PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(20.0)))));

    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, GlazingRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static Fin<GlazingRow> Resolve(Component component) =>
        SeedJoin.Resolve(Table, component.Designation);

    public static readonly SeedLaw<GlazingRow> Law = SeedLaw<GlazingRow>.Of(
        family: ComponentFamily.Glazing,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: static r => r.Panes[0].Glass.Substance,
        source: static r => r.Source,
        standard: static _ => IguStandard,
        detail: Some<Func<GlazingRow, SectionProfile, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.Panes[0].Glass.Appearance);

    static Validation<Error, Unit> Coherence(GlazingRow r) =>
        AdmissionSlots.Accumulate(Seq(
            GlazingDetail.Stack(r.Panes, r.Cavities, r.FireResistanceEiMinutes),
            AdmissionSlots.Gate(
                r.Muntin.ForAll(static m => m.HorizontalBars >= 0 && m.VerticalBars >= 0 && m.HorizontalBars + m.VerticalBars > 0),
                new KernelFault.InvalidValue(nameof(r.Muntin), "non-negative bars with at least one muntin"))));

    static Fin<SectionProfile> Profile(GlazingRow r) =>
        from plies in GlazingDetail.Plies(r.Panes, r.Cavities)
        let overallMm = r.Panes.Sum(static p => p.ThicknessMm.Value) + r.Cavities.Sum(static c => c.WidthMm.Value)
        from profile in SectionProfile.Layered.Of(plies, overallMm: overallMm, widthMm: overallMm)
        select profile;

    static Fin<PropertyBag> Detail(GlazingRow r, SectionProfile profile) =>
        from performance in GlazingThermal.Evaluate(r.Panes, r.Cavities, CavityTilt.Vertical)
        from bag in GlazingDetail.Bag(r.Panes, r.Cavities, r.Spacer, r.EdgeSeal, r.Muntin, r.FireResistanceEiMinutes, r.Source)
        select bag;

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        from row in Resolve(component)
        from capacity in GlazingStructural.Capacity(
            row.Panes, row.Cavities, row.FireResistanceEiMinutes, placement.GlassLoadDurationS,
            placement.GlassBasis, placement.GlassEdgeFactor)
        from lifted in SectionCapacity.Lift(new CapacityLift.Glass(component.Designation, capacity))
        select lifted;
}
```

## [03]-[RESEARCH]

(none)
