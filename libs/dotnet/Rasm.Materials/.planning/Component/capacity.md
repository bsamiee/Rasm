# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION PIPELINE. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Component` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the realized `ComponentFamily` structural paths: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section, AND the EC2 §6.2 section-level shear screen), `SteelMember` (the `steel#STEEL_FAMILY` `DesignCapacity` design resistance lifted whole under the basis it names — the AISC 360 φ-form or the EN 1993-1-1 γM-divided resistances — with `CompactnessClass`/slenderness; the AISI deck capacity and the EN 1993-1-2 fire state land the same case), `TimberMember` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` lifted whole), and `MasonryUnreinforced` (the axial-flexural unity check AND the flexural-tension screen over the cmu `MaterialGrade` row's `GradeProperties.Cmu` `f'm` + grouted `ComputedSection` + the `masonry#MASONRY_FAMILY` mortar-keyed row feed). Every case binds a `DesignBasis`: the JURISDICTION axis carrying the authority body, the `SafetyFormat`, the partial-factor and resistance-factor columns ONE `Resist` fold reads, the `NationalAnnex`-threaded typed `IStandard` citation, the interaction kernel `Check` dispatches, and — where the jurisdiction publishes one — the masonry resistance algebra. A second design code for an already-cased family is a BASIS ROW, never a sibling case forking the closed `GoverningAction`/`Utilisation` verdict vocabulary the `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` consumer keys on, and a case name spelling one code is the deleted form. A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules lifts its already-computed capacity into ONE case here, and the RC, fatigue, anchorage, and base-plate surfaces are `Resolve` builds over their DECLARED inputs — the design-code COMPUTATION stays the family owner's where a family owns it, the unified VERDICT this owner's. The pipeline is TOTAL over the load path: `MasonryReinforced` carries the TMS 402 §9.3 steel-couple arm, `GlassPane` the EN 16612 pane resistance the glazing family lifts, `Connection` the weld/adhesive/stud/connector/anchor capacities, `AluminumMember` the EN 1999-1-1 elastic-floor resistances over the banded (fo, fu) pair, `Fatigue` the ONE detail-category S-N law spanning the EN 1993-1-9 fourteen-rung ladder and the AISC 360 Appendix 3 A–E′ constants, and `BasePlate` the AISC DG1 bearing/plate-thickness pair — one `Check` from cross-section to weld to hanger to anchor — while `SectionSelection` is the pipeline's INVERSE query, ONE least-MASS passing scan over three declared candidate sources: the frozen catalogue the full-database steel seed supplies, a caller-parameterized composition sweep, and the (thread × grade) bolt table the fastener standards tables publish. This owner is the ULTIMATE complement to `component#COMPONENT_OWNER` `SectionSolver`: that solver gives the elastic `ComputedSection` every family solves from its `SectionProfile` arm, THIS owner gives the reinforced-section transformed properties, the EC2 section-level shear screen, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so `HullCache` pays that sweep ONCE per `(ComponentId, DiagramResolution)` pair and hands the frozen body to the artifact row a composition root writes. The page composes `reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck`, the `steel`/`timber`/`cmu` sibling capacities, the in-folder `UnitsNet` quantity coercion at the edge, and the `component#COMPONENT_OWNER` `ComponentFault` band-2300 channel for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` structural-Assessment route by `MaterialId`/section key, host-neutral here.

## [01]-[INDEX]

- [02]-[DESIGN_BASIS]: `DesignBasis` rows the jurisdiction algebra — `SafetyFormat` carrying `Resist`/`Reduce`, `ResistanceAction` the factor axis both formats key on with the `SectionFactors` pair it assembles, `MasonryAlgebra` the two masonry folds over `MasonrySection`/`MasonryCouple`, `LateralHazard`, `AxialRegime`, `InteractionOperands`, `DiagramResolution`, and the type-init parity census over the shared MEMBER key set.
- [03]-[FATIGUE_LAW]: `FatigueLaw` folds the two-ladder S-N algebra over `FatigueAssessment`, `EnFatigueCategory`, and `AiscFatigueCategory`.
- [04]-[DEMAND_VERDICT]: `CapacityBuild` requests every DECLARED build (`Hull` · `Elastic` · `Detail` · `Anchorage` · `Bearing`) over `AnchorBed`/`PlateBed`/`AnchorPlacement`; `CapacityLift` requests every sibling-capacity lift and mints the fire pair off the `FireState` input contract; `Demand` and its `DemandColumn`/`DemandBand` roster admit the action vector; `CapacityPlacement` threads the placement currency; `GoverningAction`, `Utilisation`, `MemberCheckRequirement`, `MasonryReduction`, and `LateralRule`/`LateralPair` close the verdict vocabulary.
- [05]-[SECTION_CAPACITY]: `SectionCapacity` closes the capacity family, `Check` folds every arm over the `GuardedRatio`/`FibreRatio`/`Worst` candidate algebra, and the boundary block holds `Resolve`, the TOTAL `Lift`, `HullCache` with its `Freeze`/`Thaw` round trip, and the Möller–Trumbore hull kernel.
- [06]-[SECTION_SELECTION]: `SectionSelection` inverts `Check` — `SectionCandidate` over three producers (`Stocked` · `Fabricated` · `Threaded`), `BoltJoint` declaring what a thread/grade sweep holds fixed, and ONE `Least` fold.
- [07]-[RESEARCH]: terminal.

## [02]-[DESIGN_BASIS]

- Owner: `DesignBasis` is the jurisdiction row every `SectionCapacity` case binds — authority body, `SafetyFormat`, the partial-factor and resistance-factor columns, the annex-threaded `IStandard` citation, the `Interact(InteractionOperands)` combined-action kernel, and the optional `MasonryAlgebra` — so a second code over an already-cased family is one ROW and a case name spelling one code is the deleted form. `SafetyFormat` owns how a nominal strength becomes a design resistance AND how a tabulated lateral nominal reduces; `ResistanceAction` owns which factor a fold takes and assembles the `SectionFactors` (φ, γ) pair the format consumes — the ALTITUDE WORD keeping it apart from the member-altitude `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` `ResistanceFactors` φ column set exactly as `SectionCapacity` is kept apart from `MemberCapacity` — so no arm re-spells "×φ or ÷γM"; `MasonryAlgebra` owns the whole per-jurisdiction masonry resistance difference; `LateralHazard` owns the SDPWS reduction pair; `InteractionOperands` carries the already-normalized ratios a kernel folds and nothing else.
- Entry: `basis.Resist(action, nominal)` is the ONE nominal-to-design fold; `basis.Interact(operands)` the ONE combined-action kernel; `basis.Standard(annex)` the citation under the project's annex; `format.Reduce(hazard, nominal)` the lateral reduction, `Option`-shaped because SDPWS serves the two US formats alone.
- Growth: a new jurisdiction is one `DesignBasis` row — body, format, factors, citation, `Interact` kernel, and (masonry only) its algebra row; a new resistance class is one `ResistanceAction` row with one cell on each format's column; a new masonry jurisdiction is one `MasonryAlgebra` row, never a branch inside a fold.
- Boundary: `DesignBasis` MEMBER keys ARE the `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` `DesignCode` roster spelled identically — one vocabulary carried by two typed rows because the branch strata forbid a reference in either direction — so the parity census below DERIVES the claim at type-init rather than asserting it in prose (Materials `RULINGS.md [02]`). The section-and-load-path-only keys (`en16612` glazing, `en1993-1-8`/`aws-d1-1`/`astm-d1002`/`icc-es` connection, `en1992-4` anchorage, `en1993-1-9`/`aisc-app3` fatigue) are the DECLARED carve with no member-check counterpart and never cross.
- Boundary: γM2 has ONE authority — the `DesignBasis` partial-factor column read through `ResistanceAction.Fracture` — so a family page divides joint and fracture resistances through this row and a local copy is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using VividOrange.ForceMomentInteraction;
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;
using VividOrange.Sections;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Serialization;
using VividOrange.Standards;
using VividOrange.Standards.Eurocode;
using UnitsNet;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DiagramResolution {
    public static readonly DiagramResolution Draft    = new("draft",    steps: 16, concreteMaxAreaMm2: 500.0, rebarDivisions: 12);
    public static readonly DiagramResolution Standard = new("standard", steps: 30, concreteMaxAreaMm2: 250.0, rebarDivisions: 16);
    public static readonly DiagramResolution Fine     = new("fine",     steps: 48, concreteMaxAreaMm2: 120.0, rebarDivisions: 24);
    public int Steps { get; }
    public double ConcreteMaxAreaMm2 { get; }
    public int RebarDivisions { get; }

    public DiagramSettings ToSettings() =>
        new(Area.FromSquareMillimeters(ConcreteMaxAreaMm2), Angle.FromDegrees(25.0),
            Area.FromSquareMillimeters(ConcreteMaxAreaMm2 * 0.8), Angle.FromDegrees(25.0), RebarDivisions, Steps);
}

public readonly record struct SectionFactors(double Phi, double Gamma);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafetyFormat {
    public static readonly SafetyFormat Asd        = new("asd",
        resist: static (nominal, _) => nominal,
        reduce: static (hazard, nominal) => Some(nominal / hazard.AsdDivisor));
    public static readonly SafetyFormat Lrfd       = new("lrfd",
        resist: static (nominal, factors) => nominal * factors.Phi,
        reduce: static (hazard, nominal) => Some(nominal * hazard.LrfdFactor));
    public static readonly SafetyFormat LimitState = new("limit-state",
        resist: static (nominal, factors) => nominal / factors.Gamma,
        reduce: static (_, _) => Option<double>.None);

    [UseDelegateFromConstructor] public partial double Resist(double nominal, SectionFactors factors);
    [UseDelegateFromConstructor] public partial Option<double> Reduce(LateralHazard hazard, double nominalKnPerM);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResistanceAction {
    public static readonly ResistanceAction CrossSection  = new("cross-section",  factors: static b => new SectionFactors(b.PhiFlexure, b.GammaM0));
    public static readonly ResistanceAction Stability     = new("stability",      factors: static b => new SectionFactors(b.PhiFlexure, b.GammaM1));
    public static readonly ResistanceAction Fracture      = new("fracture",       factors: static b => new SectionFactors(b.PhiFlexure, b.GammaM2));
    public static readonly ResistanceAction Shear         = new("shear",          factors: static b => new SectionFactors(b.PhiShear,   b.GammaM0));
    public static readonly ResistanceAction Reinforcement = new("reinforcement",  factors: static b => new SectionFactors(b.PhiFlexure, b.GammaS));

    [UseDelegateFromConstructor] public partial SectionFactors Factors(DesignBasis basis);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralHazard {
    public static readonly LateralHazard Wind    = new("wind",    asdDivisor: 2.0, lrfdFactor: 0.80);
    public static readonly LateralHazard Seismic = new("seismic", asdDivisor: 2.8, lrfdFactor: 0.50);
    public double AsdDivisor { get; }
    public double LrfdFactor { get; }

    public Fin<double> Design(double nominalKnPerM, SafetyFormat format) =>
        format.Reduce(this, nominalKnPerM)
            .ToFin(new ComponentFault.LateralFormatUnsupported(format));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialRegime {
    public static readonly AxialRegime Buckling = new("buckling", term: static axial => axial);
    public static readonly AxialRegime Stocky   = new("stocky",   term: static axial => axial * axial);
    [UseDelegateFromConstructor] public partial double Term(double axial);
    public static AxialRegime Of(double relativeSlenderness) => relativeSlenderness > 0.3 ? Buckling : Stocky;
}

public readonly record struct InteractionOperands(double Axial, double Major, double Minor, double MinorWeight, AxialRegime Regime) {
    public static InteractionOperands Of(double axial, double major, double minor) =>
        new(axial, major, minor, MinorWeight: 1.0, AxialRegime.Buckling);
}

public readonly record struct MasonrySection(
    double FmMpa, double NetAreaMm2, double SectionModulusXMm3, double SectionModulusYMm3,
    double SlendernessReduction, double FlexuralTensionMpa, double ShearBondMpa);

public readonly record struct MasonryCouple(
    double FmMpa, Option<double> FyMpa, double SteelAreaMm2, double NetAreaMm2,
    double EffectiveDepthMm, double BedLengthMm, double SlendernessReduction);

public readonly record struct MasonryResistances(double Pn, double Mnx, double Mny, double Tension, double Vn);

public readonly record struct ReinforcedStresses(double Fm, Option<double> Fy, double Phi, double PhiV);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MasonryAlgebra {
    public static readonly MasonryAlgebra Strength = new("strength", unreinforced: TmsUnreinforced, reinforced: TmsReinforced);
    public static readonly MasonryAlgebra Factored = new("factored", unreinforced: EnUnreinforced,  reinforced: EnReinforced);

    [UseDelegateFromConstructor] public partial MasonryResistances Unreinforced(DesignBasis basis, MasonrySection wall, double compressionKn);
    [UseDelegateFromConstructor] public partial ReinforcedStresses Reinforced(DesignBasis basis, MasonryCouple couple);

    static MasonryResistances TmsUnreinforced(DesignBasis basis, MasonrySection wall, double compressionKn) {
        double fibre = basis.StressBlock * wall.FmMpa;
        double material = Math.Min(0.315 * Math.Sqrt(wall.FmMpa) * wall.NetAreaMm2 * 1e-3,
            wall.ShearBondMpa * wall.NetAreaMm2 * 1e-3 + 0.45 * compressionKn);
        return new MasonryResistances(
            SlendernessCoefficient * basis.Resist(ResistanceAction.CrossSection, fibre * wall.NetAreaMm2 * wall.SlendernessReduction * 1e-3),
            basis.Resist(ResistanceAction.CrossSection, fibre * wall.SectionModulusXMm3 * 1e-6),
            basis.Resist(ResistanceAction.CrossSection, fibre * wall.SectionModulusYMm3 * 1e-6),
            basis.Resist(ResistanceAction.CrossSection, wall.FlexuralTensionMpa),
            basis.Resist(ResistanceAction.Shear, basis.ShearCeilingMpa.Match(
                Some: ceiling => Math.Min(material, ceiling * wall.NetAreaMm2 * 1e-3),
                None: () => material)));
    }

    static MasonryResistances EnUnreinforced(DesignBasis basis, MasonrySection wall, double compressionKn) {
        double fxd = basis.Resist(ResistanceAction.CrossSection, wall.FlexuralTensionMpa);
        double sigmaD = compressionKn * 1e3 / Math.Max(wall.NetAreaMm2, double.Epsilon);
        return new MasonryResistances(
            basis.Resist(ResistanceAction.CrossSection, wall.FmMpa * wall.NetAreaMm2 * wall.SlendernessReduction * 1e-3),
            fxd * wall.SectionModulusXMm3 * 1e-6,
            fxd * wall.SectionModulusYMm3 * 1e-6,
            fxd,
            basis.Resist(ResistanceAction.CrossSection, (wall.ShearBondMpa + 0.4 * sigmaD) * wall.NetAreaMm2 * 1e-3));
    }

    static ReinforcedStresses TmsReinforced(DesignBasis basis, MasonryCouple couple) =>
        new(basis.StressBlock * couple.FmMpa, couple.FyMpa, 0.90, 0.80);

    static ReinforcedStresses EnReinforced(DesignBasis basis, MasonryCouple couple) =>
        new(basis.Resist(ResistanceAction.CrossSection, couple.FmMpa),
            couple.FyMpa.Map(fyk => basis.Resist(ResistanceAction.Reinforcement, fyk)), 1.0, 1.0);

    const double SlendernessCoefficient = 0.80;
}

// --- [POLICIES] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignBasis {
    public static readonly DesignBasis Aisc360      = new("aisc360",    ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis AisiS100     = new("aisi-s100",  ComponentAuthority.Aisi, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1992       = new("en1992",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.50, 1.50, Ec2,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1993       = new("en1993",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3,          Linear, gammaM2: 1.25);
    public static readonly DesignBasis En1993Stainless = new("en1993-1-4", ComponentAuthority.En, SafetyFormat.LimitState, 1.10, 1.10, Ec3Stainless, Linear, gammaM2: 1.25);
    public static readonly DesignBasis En1993Fatigue = new("en1993-1-9", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Fatigue,   Linear);
    public static readonly DesignBasis AiscFatigue  = new("aisc-app3",  ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1994       = new("en1994",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec4,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1995       = new("en1995",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.25, 1.25, Ec5,          Timber);
    public static readonly DesignBasis En1996       = new("en1996",     ComponentAuthority.En,   SafetyFormat.LimitState, 2.00, 2.00, Ec6,          Linear, gammaS: 1.15, masonry: MasonryAlgebra.Factored);
    public static readonly DesignBasis En1999       = new("en1999",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.10, 1.10, Ec9,          Linear, gammaM2: 1.25);
    public static readonly DesignBasis En1992Anchors = new("en1992-4", ComponentAuthority.En,    SafetyFormat.LimitState, 1.50, 1.50, NoCitation,   Linear, gammaS: 1.15);
    public static readonly DesignBasis Tms402       = new("tms402",     ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear,
        phiFlexure: 0.60, phiShear: 0.80, stressBlock: 0.80, shearCeilingMpa: Some(2.07), masonry: MasonryAlgebra.Strength);
    public static readonly DesignBasis En16612      = new("en16612",    ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis En1993Joints = new("en1993-1-8", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Joints,    Linear, gammaM2: 1.25);
    public static readonly DesignBasis AwsD11       = new("aws-d1-1",   ComponentAuthority.Aws,  SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis AstmD1002    = new("astm-d1002", ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis IccEs        = new("icc-es",     ComponentAuthority.IccEs, SafetyFormat.Asd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis Sdpws        = new("sdpws",      ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis Nds          = new("nds",        ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Timber);
    public static readonly DesignBasis Aci318       = new("aci318",     ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear,
        phiFlexure: 0.90, phiShear: 0.75, stressBlock: 0.85);

    public ComponentAuthority Body { get; }
    public SafetyFormat Format { get; }
    public double GammaM0 { get; }
    public double GammaM1 { get; }
    public double GammaM2 { get; }
    public double GammaS { get; }
    public double PhiFlexure { get; }
    public double PhiShear { get; }
    public double StressBlock { get; }
    public Option<double> ShearCeilingMpa { get; }
    public Option<MasonryAlgebra> Masonry { get; }

    public double Resist(ResistanceAction action, double nominal) => Format.Resist(nominal, action.Factors(this));

    [UseDelegateFromConstructor]
    public partial Option<IStandard> Standard(NationalAnnex annex);

    [UseDelegateFromConstructor]
    public partial double Interact(InteractionOperands operands);

    static Option<IStandard> Ec2(NationalAnnex a)          => Some<IStandard>(new En1992(En1992Part.Part1_1, a));
    static Option<IStandard> Ec3(NationalAnnex a)          => Some<IStandard>(new En1993(En1993Part.Part1_1, a));
    static Option<IStandard> Ec3Stainless(NationalAnnex a) => Some<IStandard>(new En1993(En1993Part.Part1_4, a));
    static Option<IStandard> Ec3Joints(NationalAnnex a)    => Some<IStandard>(new En1993(En1993Part.Part1_8, a));
    static Option<IStandard> Ec3Fatigue(NationalAnnex a)   => Some<IStandard>(new En1993(En1993Part.Part1_9, a));
    static Option<IStandard> Ec4(NationalAnnex a)          => Some<IStandard>(new En1994(En1994Part.Part1_1, a));
    static Option<IStandard> Ec5(NationalAnnex a)          => Some<IStandard>(new En1995(En1995Part.Part1_1, a));
    static Option<IStandard> Ec6(NationalAnnex a)          => Some<IStandard>(new En1996(En1996Part.Part1_1, a));
    static Option<IStandard> Ec9(NationalAnnex a)          => Some<IStandard>(new En1999(En1999Part.Part1_1, a));
    static Option<IStandard> NoCitation(NationalAnnex _)   => Option<IStandard>.None;

    static double Aisc(InteractionOperands o) =>
        o.Axial >= 0.2 ? o.Axial + 8.0 / 9.0 * (o.Major + o.Minor) : o.Axial / 2.0 + o.Major + o.Minor;

    static double Linear(InteractionOperands o) => o.Axial + o.Major + o.Minor;

    static double Timber(InteractionOperands o) =>
        o.Regime.Term(o.Axial) + Math.Max(o.Major + o.MinorWeight * o.Minor, o.MinorWeight * o.Major + o.Minor);

    // --- [PARITY_CENSUS]
    static readonly FrozenSet<string> SectionCarve = FrozenSet.ToFrozenSet(
        ["en16612", "en1993-1-8", "aws-d1-1", "astm-d1002", "icc-es", "en1992-4", "en1993-1-9", "aisc-app3"],
        StringComparer.Ordinal);
    static readonly FrozenSet<string> MemberKeys = FrozenSet.ToFrozenSet(
        ["aisc360", "aisi-s100", "en1992", "en1993", "en1993-1-4", "en1994", "en1995", "en1996", "en1999",
         "tms402", "sdpws", "nds", "aci318"],
        StringComparer.Ordinal);
    static readonly Unit RosterParity = ProveRoster();

    static Unit ProveRoster() {
        Seq<string> declared = toSeq(Items).Map(static basis => basis.Key);
        Seq<string> member = declared.Filter(static key => !SectionCarve.Contains());
        return member.Count == MemberKeys.Count && member.ForAll(MemberKeys.Contains)
            && declared.Count == member.Count + SectionCarve.Count
            ? unit
            : throw new InvalidOperationException(
                $"Design-basis members are absent from the canonical roster: {string.Join(',', member.Filter(key => !MemberKeys.Contains(key)))}.");
    }
}
```

## [03]-[FATIGUE_LAW]

- Owner: `FatigueLaw` owns the two-ladder S-N algebra as ONE closed family — the `EnFatigueCategory` fourteen-rung EN 1993-1-9 direct-stress set under the `FatigueAssessment` γMf grid, and the `AiscFatigueCategory` Appendix 3 A–E′ constants — and projects both the governing `DesignBasis` and the design stress range at a demanded cycle count.
- Cases: `En(Category, Assessment)` · `Aisc(Category)`. The two codes publish the SAME anatomy — a permissible direct-stress range as a function of cycle count with a threshold floor — under NON-CONVERTIBLE ladders (EN fixes its knee at 5×10⁶ for every rung; the AISC thresholds knee anywhere from ~1.8 to 22×10⁶), so the ladder is the case discriminant and never a conversion.
- Entry: `law.DesignMpa(cycles)` — the EN arm walks the m = 3 / m = 5 two-slope law to the ΔσL floor and divides by its assessment's γMf (γFf = 1.0); the AISC arm evaluates (Cf·329/n)^(1/3) MPa floored at FTH, the allowable form carrying no further factor. `law.Basis` names the jurisdiction the verdict reports under.
- Growth: a new rung is one `[SmartEnum]` row; the shear ΔτC rungs (100/80, single slope m = 5) are typed-absent this pass and land as a sibling column pair; a third national ladder is one `FatigueLaw` case with its own category roster.
- Boundary: the per-detail assignment — WHICH constructional detail takes which rung, EN Tables 8.1–8.10 or the AISC descriptive rows — is the CALLER's declaration riding `CapacityPlacement.Detail`, exactly as a weld states its load angle. `SectionCapacity.Fatigue` carries the law whole as its payload, so the ladder never forks the capacity case set.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FatigueAssessment {
    public static readonly FatigueAssessment DamageTolerantLow  = new("damage-tolerant-low",  gammaMf: 1.00);
    public static readonly FatigueAssessment DamageTolerantHigh = new("damage-tolerant-high", gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeLow        = new("safe-life-low",        gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeHigh       = new("safe-life-high",       gammaMf: 1.35);
    public double GammaMf { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnFatigueCategory {
    public static readonly EnFatigueCategory C160 = new("160", refMpa: 160.0);
    public static readonly EnFatigueCategory C140 = new("140", refMpa: 140.0);
    public static readonly EnFatigueCategory C125 = new("125", refMpa: 125.0);
    public static readonly EnFatigueCategory C112 = new("112", refMpa: 112.0);
    public static readonly EnFatigueCategory C100 = new("100", refMpa: 100.0);
    public static readonly EnFatigueCategory C90  = new("90",  refMpa: 90.0);
    public static readonly EnFatigueCategory C80  = new("80",  refMpa: 80.0);
    public static readonly EnFatigueCategory C71  = new("71",  refMpa: 71.0);
    public static readonly EnFatigueCategory C63  = new("63",  refMpa: 63.0);
    public static readonly EnFatigueCategory C56  = new("56",  refMpa: 56.0);
    public static readonly EnFatigueCategory C50  = new("50",  refMpa: 50.0);
    public static readonly EnFatigueCategory C45  = new("45",  refMpa: 45.0);
    public static readonly EnFatigueCategory C40  = new("40",  refMpa: 40.0);
    public static readonly EnFatigueCategory C36  = new("36",  refMpa: 36.0);
    public double RefMpa { get; }
    public double CaflMpa => Math.Pow(2.0 / 5.0, 1.0 / 3.0) * RefMpa;
    public double CutoffMpa => Math.Pow(5.0 / 100.0, 1.0 / 5.0) * CaflMpa;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AiscFatigueCategory {
    public static readonly AiscFatigueCategory A      = new("a",       cf: 250e8, fthMpa: 165.0);
    public static readonly AiscFatigueCategory B      = new("b",       cf: 120e8, fthMpa: 110.0);
    public static readonly AiscFatigueCategory BPrime = new("b-prime", cf: 61e8,  fthMpa: 83.0);
    public static readonly AiscFatigueCategory C      = new("c",       cf: 44e8,  fthMpa: 69.0);
    public static readonly AiscFatigueCategory CPrime = new("c-prime", cf: 44e8,  fthMpa: 83.0);
    public static readonly AiscFatigueCategory D      = new("d",       cf: 22e8,  fthMpa: 48.0);
    public static readonly AiscFatigueCategory E      = new("e",       cf: 11e8,  fthMpa: 31.0);
    public static readonly AiscFatigueCategory EPrime = new("e-prime", cf: 3.9e8, fthMpa: 18.0);
    public double Cf { get; }
    public double FthMpa { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FatigueLaw {
    private FatigueLaw() { }
    public sealed record En(EnFatigueCategory Category, FatigueAssessment Assessment) : FatigueLaw;
    public sealed record Aisc(AiscFatigueCategory Category) : FatigueLaw;

    public DesignBasis Basis => Switch(
        en: static _ => DesignBasis.En1993Fatigue,
        aisc: static _ => DesignBasis.AiscFatigue);

    public double DesignMpa(double cycles) => Switch(
        en: e => EnRange(e.Category, cycles) / e.Assessment.GammaMf,
        aisc: a => Math.Max(Math.Pow(a.Category.Cf * 329.0 / cycles, 1.0 / 3.0), a.Category.FthMpa));

    static double EnRange(EnFatigueCategory category, double cycles) =>
        cycles <= 5e6 ? category.RefMpa * Math.Pow(2e6 / cycles, 1.0 / 3.0)
        : cycles <= 1e8 ? category.CaflMpa * Math.Pow(5e6 / cycles, 1.0 / 5.0)
        : category.CutoffMpa;
}
```

## [04]-[DEMAND_VERDICT]

- Owner: `CapacityBuild` is the DECLARED-INPUT build request and `CapacityLift` the SIBLING-CAPACITY lift request — two discriminants because the two carry different KINDS of input, never two spellings of one modality: a build arm holds the declaration a solver consumes (an RC section, an S-N detail, an anchor bed, a plate bed) and a lift arm holds a design value a family owner already computed. `Demand` admits the signed action vector over the `DemandColumn` roster; `CapacityPlacement` is the ONE currency threading `component#COMPONENT_OWNER` `ComponentFamily.Capacity`, so a new placement input is one column here rather than a per-family parameter tail; `Utilisation` distinguishes a bounded verdict, a section pass owing a named member check, and an UNBOUNDED verdict; `MemberCheckRequirement` closes the section-undecidable deferral vocabulary; `MasonryReduction` OWNS the stability bracket as a derivation over (height, radius of gyration); `LateralRule` owns the connector report's own two-direction interaction convention.
- Cases: `CapacityBuild.{Hull · Elastic · Detail · Anchorage · Bearing}`, each arm carrying EXACTLY the inputs its solver consumes — the prior loose parameter pair forced a half-dead knob onto every elastic call, and the same law now keeps an anchor bed off a fatigue request. `CapacityLift.{Steel · Timber · DeckSheet · Masonry · ReinforcedMasonry · Glass · SteelFire · TimberFire · Weld · Adhesive · Stud · Connector · LateralPanel · Bolt · SlipCritical · TimberDowel · Aluminum}`, each carrying its full lift context so the modality is recoverable from the request value alone.
- Entry: `Demand.Admit(…)` is the ACCUMULATING boundary naming every offending column in one verdict and `Demand.Of` the same proof collapsed onto `Fin`; `CapacityBuild.Declared(subject, placement)` turns the placement's declarations into the build requests they name and `CapacityLift.Fire(subject, state)` the family's fire state into its lift, so every declared modality is reachable from a `Component` and its placement; `CapacityLift.Kind`/`CapacityBuild.Kind` own the case-name projection every signal dimension and analytics column keys on, so a reflected runtime type name at a consumer has no reason to exist.
- Growth: a new demand axis is one `DemandColumn` row with its `Demand` column — the token, the admitted band, and the guard land together; a new declared modality is one `CapacityBuild` arm with one `CapacityPlacement` column; a new family lift is one `CapacityLift` case, never another overload; a new fire-rated family is one `FireState` case and one `Lift` arm.
- Boundary: `Demand` MODALITY columns bind their OWN case — unit shear to `LateralPanel`, the range/count pair to `Fatigue` — so a member arm neither resists nor reads them and the check that consumes them is its own invocation. The identity rides the lift and build BASE where a new case cannot forget it: the analytics per-check dataset and the `MaterialsFact` stream both key on (kind, governing), which collides for two members of one kind under one op.
- Boundary: `FireState` is the fire modality's typed input contract and `CapacityLift.Fire` its ONE mint, so both fire cases are constructed rather than assembled at a call site. BOTH producers are LANDED at their owners: `steel#STEEL_FAMILY` `SteelSeed.Capacity` reads `CapacityPlacement.FireExposure` through the `SteelFire` §4.2.5.1 step and `SteelDesign.Fire` into `FireState.Steel`, and `timber#TIMBER_CAPACITY` `TimberSeed.Capacity` routes its `TimberDesign.Fire` reduced-section capacity into `FireState.Timber` — every fire lift in the folder constructs through this mint and a `new CapacityLift.SteelFire`/`TimberFire` beside it is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct AnchorBed(
    double FckMpa,
    PositiveMagnitude HefMm,
    Option<double> EdgeMm,
    bool Cracked);

public readonly record struct PlateBed(
    PositiveMagnitude WidthMm,
    PositiveMagnitude LengthMm,
    PositiveMagnitude ThicknessMm,
    double FyMpa,
    PositiveMagnitude ColumnDepthMm,
    PositiveMagnitude ColumnFlangeMm,
    double FcMpa,
    double ConfinementRatio);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityBuild {
    private CapacityBuild(ComponentId subject) => Subject = subject;
    public ComponentId Subject { get; }
    public sealed record Hull(ComponentId Subject, RcSection Section, DiagramResolution Resolution) : CapacityBuild(Subject);
    public sealed record Elastic(ComponentId Subject, RcSection Section) : CapacityBuild(Subject);
    public sealed record Detail(ComponentId Subject, FatigueLaw Law) : CapacityBuild(Subject);
    public sealed record Anchorage(ComponentId Subject, FastenerAssembly Assembly, ShearPlane Plane, AnchorBed Bed) : CapacityBuild(Subject);
    public sealed record Bearing(ComponentId Subject, PlateBed Plate) : CapacityBuild(Subject);

    public string Kind => Switch(
        hull: static _ => nameof(Hull),
        elastic: static _ => nameof(Elastic),
        detail: static _ => nameof(Detail),
        anchorage: static _ => nameof(Anchorage),
        bearing: static _ => nameof(Bearing));

    public static Seq<CapacityBuild> Declared(ComponentId subject, CapacityPlacement placement) =>
        placement.Detail.Map(law => (CapacityBuild)new Detail(subject, law)).ToSeq()
            + placement.Anchorage.Map(anchor => (CapacityBuild)new Anchorage(subject, anchor.Assembly, anchor.Plane, anchor.Bed)).ToSeq()
            + placement.Bearing.Map(plate => (CapacityBuild)new Bearing(subject, plate)).ToSeq();
}

public readonly record struct AnchorPlacement(FastenerAssembly Assembly, ShearPlane Plane, AnchorBed Bed);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FireState {
    private FireState() { }
    public sealed record Steel(DesignCapacity Ambient, SteelFireFacts Retention) : FireState;
    public sealed record Timber(TimberCapacity Residual) : FireState;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityLift {
    private CapacityLift(ComponentId subject) => Subject = subject;
    public ComponentId Subject { get; }
    public sealed record Steel(ComponentId Subject, DesignCapacity Capacity) : CapacityLift(Subject);
    public sealed record Timber(ComponentId Subject, TimberCapacity Capacity) : CapacityLift(Subject);
    public sealed record DeckSheet(ComponentId Subject, GaugeRow Gauge, DeckProfileRow Rib, DesignCapacity Capacity) : CapacityLift(Subject);
    public sealed record Masonry(ComponentId Subject, GradeProperties.Cmu Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, RuptureModulus Rupture, FlexuralStrengthEn Flexural, MortarSystem System, MortarType Mortar) : CapacityLift(Subject);
    public sealed record ReinforcedMasonry(ComponentId Subject, GradeProperties.Cmu Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, CmuRow Unit, MaterialGrade Bar) : CapacityLift(Subject);
    public sealed record Glass(ComponentId Subject, GlassCapacity Capacity) : CapacityLift(Subject);
    public sealed record SteelFire(ComponentId Subject, DesignCapacity Ambient, double Ky, double Ke, double SteelTemperatureC) : CapacityLift(Subject);
    public sealed record TimberFire(ComponentId Subject, TimberCapacity Residual) : CapacityLift(Subject);
    public sealed record Weld(ComponentId Subject, JointRow.Weld Row, double LoadAngleDeg) : CapacityLift(Subject);
    public sealed record Adhesive(ComponentId Subject, JointRow.Adhesive Row) : CapacityLift(Subject);
    public sealed record Stud(ComponentId Subject, JointRow.Stud Row, StudGroup Group, int Count) : CapacityLift(Subject);
    public sealed record Connector(ComponentId Subject, ConnectorCapacity Capacity) : CapacityLift(Subject);
    public sealed record LateralPanel(ComponentId Subject, double DesignKnPerM, LateralHazard Hazard) : CapacityLift(Subject);
    public sealed record Bolt(ComponentId Subject, FastenerAssembly Assembly, BearingDesign Bearing, ShearPlane Plane) : CapacityLift(Subject);
    public sealed record SlipCritical(ComponentId Subject, FastenerAssembly Assembly, FastenerInstallation Install) : CapacityLift(Subject);
    public sealed record TimberDowel(ComponentId Subject, double PerPlaneShearKn, int Planes) : CapacityLift(Subject);
    public sealed record Aluminum(ComponentId Subject, MaterialGrade Grade, ExtrusionForm Form, double FoMpa, double FuMpa, ComputedSection Section, DesignBasis Basis) : CapacityLift(Subject);

    public static CapacityLift Fire(ComponentId subject, FireState state) => state.Switch(
        steel: s => (CapacityLift)new SteelFire(subject, s.Ambient, s.Retention.Ky, s.Retention.KE, s.Retention.CriticalTemperatureC),
        timber: t => new TimberFire(subject, t.Residual));

    public string Kind => Switch(
        steel: static _ => nameof(Steel),
        timber: static _ => nameof(Timber),
        deckSheet: static _ => nameof(DeckSheet),
        masonry: static _ => nameof(Masonry),
        reinforcedMasonry: static _ => nameof(ReinforcedMasonry),
        glass: static _ => nameof(Glass),
        steelFire: static _ => nameof(SteelFire),
        timberFire: static _ => nameof(TimberFire),
        weld: static _ => nameof(Weld),
        adhesive: static _ => nameof(Adhesive),
        stud: static _ => nameof(Stud),
        connector: static _ => nameof(Connector),
        lateralPanel: static _ => nameof(LateralPanel),
        bolt: static _ => nameof(Bolt),
        slipCritical: static _ => nameof(SlipCritical),
        timberDowel: static _ => nameof(TimberDowel),
        aluminum: static _ => nameof(Aluminum));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GoverningAction {
    public static readonly GoverningAction Axial         = new("axial");
    public static readonly GoverningAction Flexure       = new("flexure");
    public static readonly GoverningAction BiaxialMoment = new("biaxial-moment");
    public static readonly GoverningAction Combined      = new("combined");
    public static readonly GoverningAction Shear         = new("shear");
    public static readonly GoverningAction Torsion       = new("torsion");
    public static readonly GoverningAction Bearing       = new("bearing");
    public static readonly GoverningAction InPlaneShear  = new("in-plane-shear");
    public static readonly GoverningAction Fatigue       = new("fatigue");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DemandBand {
    public static readonly DemandBand Signed      = new("signed",       admits: static value => double.IsFinite(value));
    public static readonly DemandBand NonNegative = new("non-negative", admits: static value => double.IsFinite(value) && value >= 0.0);
    [UseDelegateFromConstructor] public partial bool Admits(double value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DemandColumn {
    public static readonly DemandColumn Axial       = new("n",    DemandBand.Signed);
    public static readonly DemandColumn MomentY     = new("my",   DemandBand.Signed);
    public static readonly DemandColumn MomentZ     = new("mz",   DemandBand.Signed);
    public static readonly DemandColumn ShearY      = new("vy",   DemandBand.Signed);
    public static readonly DemandColumn ShearZ      = new("vz",   DemandBand.Signed);
    public static readonly DemandColumn Torsion     = new("mt",   DemandBand.Signed);
    public static readonly DemandColumn BearingRb   = new("rb",   DemandBand.Signed);
    public static readonly DemandColumn UnitShear   = new("q",    DemandBand.Signed);
    public static readonly DemandColumn StressRange = new("dsig", DemandBand.NonNegative);
    public static readonly DemandColumn CycleCount  = new("ncyc", DemandBand.NonNegative);
    public DemandBand Band { get; }

    public static Seq<string> Refusals(params ReadOnlySpan<double> values) =>
        toSeq(Items).Zip(Iterable<double>.FromSpan(values).ToSeq())
            .Filter(static pair => !pair.Item1.Band.Admits(pair.Item2))
            .Map(static pair => $"{pair.Item1.Key}={pair.Item2:R}");
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct Demand {
    public double AxialKn { get; }
    public double MomentYKnm { get; }
    public double MomentZKnm { get; }
    public double ShearYKn { get; }
    public double ShearZKn { get; }
    public double TorsionKnm { get; }
    public double BearingKn { get; }
    public double UnitShearKnPerM { get; }
    public double StressRangeMpa { get; }
    public double CycleCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double axialKn, ref double momentYKnm, ref double momentZKnm,
        ref double shearYKn, ref double shearZKn, ref double torsionKnm, ref double bearingKn,
        ref double unitShearKnPerM, ref double stressRangeMpa, ref double cycleCount) {
        Seq<string> offending = DemandColumn.Refusals(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn,
            torsionKnm, bearingKn, unitShearKnPerM, stressRangeMpa, cycleCount);
        validationError = offending.IsEmpty ? null : new ValidationError($"Demand columns must be finite: {string.Join(':', offending)}.");
    }

    public static Validation<Error, Demand> Admit(double axialKn, double momentYKnm, double momentZKnm,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0,
        double unitShearKnPerM = 0.0, double stressRangeMpa = 0.0, double cycleCount = 0.0) =>
        DemandColumn.Refusals(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn,
                unitShearKnPerM, stressRangeMpa, cycleCount)
            .Traverse(token => Validation<Error, Unit>.Fail(
                new KernelFault.InvalidValue(token, "a finite demand scalar")))
            .Map(_ => Create(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn,
                unitShearKnPerM, stressRangeMpa, cycleCount)).As();

    public static Fin<Demand> Of(double axialKn, double momentYKnm, double momentZKnm,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0,
        double unitShearKnPerM = 0.0, double stressRangeMpa = 0.0, double cycleCount = 0.0) =>
        Admit(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn,
            unitShearKnPerM, stressRangeMpa, cycleCount).ToFin();

    public double MomentResultantKnm => Math.Sqrt(MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double ShearResultantKn => Math.Sqrt(ShearYKn * ShearYKn + ShearZKn * ShearZKn);
}

public readonly record struct CapacityPlacement(
    double EffectiveLengthMm,
    double UnbracedLengthMm,
    DesignBasis Basis,
    NationalAnnex Annex,
    ServiceClass Service,
    LoadDuration Duration,
    DurationRow ConnectorDuration,
    PositiveMagnitude HeightMm,
    double LoadAngleDeg,
    int StudCount,
    double GlassLoadDurationS,
    LateralHazard Hazard,
    LateralAssembly Assembly,
    double FramingWidthMm,
    int DiaphragmCase,
    Option<FastenerPlacement> Fastener,
    StudGroup StudGroup,
    GlassBasis GlassBasis,
    double GlassEdgeFactor,
    Option<PositiveMagnitude> FireExposure,
    Option<FatigueLaw> Detail,
    Option<AnchorPlacement> Anchorage,
    Option<PlateBed> Bearing,
    RuptureModulus Rupture,
    FlexuralStrengthEn Flexural,
    MortarSystem System,
    MortarType Mortar,
    MaterialGrade BarGrade);

[ValueObject<double>]
public readonly partial struct MasonryReduction {
    const double SlendernessBreak = 99.0;
    const double EnInitialEccentricity = 2.0 / (450.0 * 3.4641016151377544);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and <= 1.0
            ? null
            : new ValidationError($"Masonry reduction must be finite and inside (0, 1]; received {value:R}.");

    public static MasonryReduction Of(DesignBasis basis, PositiveMagnitude heightMm, double radiusOfGyrationMm) =>
        heightMm.Value / radiusOfGyrationMm is var ratio && basis == DesignBasis.En1996
            ? Create(Math.Clamp(1.0 - EnInitialEccentricity * ratio, double.Epsilon, 1.0))
            : ratio <= SlendernessBreak
                ? Create(1.0 - Math.Pow(ratio / 140.0, 2.0))
                : Create(Math.Pow(70.0 / ratio, 2.0));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralRule {
    public static readonly LateralRule Interacting = new("interacting", fold: static (primary, secondary) => primary + secondary);
    public static readonly LateralRule Independent = new("independent", fold: Math.Max);
    [UseDelegateFromConstructor] public partial double Fold(double primary, double secondary);
}

public readonly record struct LateralPair(double SecondKn, LateralRule Rule);

[Union]
public abstract partial record Utilisation {
    private Utilisation(GoverningAction governing) => Governing = governing;
    public GoverningAction Governing { get; }
    public bool Adequate => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static _ => false,
        unbounded: static _ => false);
    public bool SectionPasses => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static verdict => verdict.Value <= 1.0,
        unbounded: static _ => false);
    public Option<double> Ratio => Switch(
        bounded: static verdict => Some(verdict.Value),
        requiresMemberCheck: static verdict => Some(verdict.Value),
        unbounded: static _ => Option<double>.None);

    public sealed record Bounded(double Value, GoverningAction Action) : Utilisation(Action);
    public sealed record RequiresMemberCheck(double Value, GoverningAction Action, MemberCheckRequirement Requirement) : Utilisation(Action);
    public sealed record Unbounded(GoverningAction Action) : Utilisation(Action);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberCheckRequirement {
    public static readonly MemberCheckRequirement RcShearReinforcement          = new("rc-shear-reinforcement");
    public static readonly MemberCheckRequirement SteelWarpingTorsion           = new("steel-warping-torsion");
    public static readonly MemberCheckRequirement CltInPlaneBending             = new("clt-in-plane-bending");
    public static readonly MemberCheckRequirement ReinforcedMasonryShearSpacing = new("reinforced-masonry-shear-spacing");
    public static readonly MemberCheckRequirement TimberBearingLength           = new("timber-bearing-length");
    public static readonly MemberCheckRequirement AnchorForwardModes            = new("anchor-forward-modes");
    public static readonly MemberCheckRequirement AluminumMemberBuckling        = new("aluminum-member-buckling");
}
```

## [05]-[SECTION_CAPACITY]

- Owner: one `SectionCapacity` `[Union]` closes the structural-capacity family across the realized member paths AND the connection load path — the ultimate N-M-M hull, the elastic transformed RC section, the rolled/composite/cold-formed (and, basis-told, stainless) steel capacity, the EC5 timber capacity, the EN 1999 aluminium member, the TMS 402 URM and §9.3 reinforced masonry checks, the EN 16612 glass pane, the weld/adhesive/stud/connector/anchor `Connection` triple, the detail-category `Fatigue` law, and the DG1 `BasePlate` pair — so a member AND its connection are checked through one `Check` fold, never a per-type surface.
- Cases: the non-RC member cases LIFT their family-owner capacities WHOLE (the design-code computation stays the sibling page's, the unified verdict this owner's); the RC, fatigue, anchorage, and base-plate cases are `Resolve` builds over declared inputs, the aluminium case computing at lift because its family owns DATA, not algebra. `RcInteraction` carries its own content key beside the mesh, so the cached hull's identity is the (subject, resolution) pair and NOT a float-wise comparison of a foreign hull.
- Entry: `SectionCapacity.Resolve(CapacityBuild)` dispatches every DECLARED build; the TOTAL `SectionCapacity.Lift(lift)` dispatches every already-computed sibling capacity under the caller's own operation key; `Check(Demand)` returns the closed `Utilisation` verdict; `HullCache.Of` is the ONE content-keyed round trip through `Freeze`/`Thaw`. The masonry lifts carry the member HEIGHT as a kernel-admitted `PositiveMagnitude` beside their section, so `Lift` mints the stability reduction from the section's own governing radius — no caller-supplied stability scalar and no re-derived code bracket exists.
- Growth: a new structural family's capacity is one `[Union]` case binding either a `Resolve` build or a lift factory and one `Check` arm, admitted only when no existing case's column set carries it; a new design code over an already-cased family is one `DesignBasis` row and the owning family page's per-basis resistance arm, NEVER a sibling case; a persisted-capacity need is the one `HullCache` pair over the `ITaxonomySerializable` marker, never a second serializer.
- Boundary: `Resolve` and `Check` are the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CapacityCheck(Key, Lift, Verdict, Elapsed)` tap SUBJECTS and `Check` the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.InteractionSweep` measured kernel; the tap is a composition-root decorator on the folder hook set at `MaterialsPoint.CapacityCheck`, so this owner emits nothing, carries no `Duration`, and references no signal type.
- Boundary: `Resolve` admits the `VividOrange.InteractionDiagram` engine once and reads the `ConcreteSectionProperties` captured by `RcSectionBuilder.Of`. Documented engine exceptions become cause-bearing `CapacitySolve`; missing effective depth or tension steel remain distinct semantic leaves, and unknown throws remain exact.
- Boundary: the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces, the no-pierce case (an eccentric hull not enclosing the origin) yielding the typed `Utilisation.Unbounded` verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity. Force and moment axes are never Euclidean-normalized together.
- Boundary: the frozen hull's store row is REGISTERED at the custodian — `Rasm.Persistence` `Version/retention#RETENTION_CLASSES` `ArtifactKind.CapacityHull` (`RetentionClass.Cache` because the hull rebuilds from the eager fibre-integration sweep, so eviction costs compute and never evidence; `CacheTier.ArtifactBlob` so the L1 lane never locally caches the mesh). The composition root crosses `ArtifactIndexRow.Admit(ArtifactKind.CapacityHull, bytes, classification, at, sourceKey)` at the custodian's `Query/cache#ARTIFACT_BLOB_INDEX`, the `(ComponentId, DiagramResolution.Key)` pair riding as the content-key preimage/sourceKey — `HullCache.Of` reads and writes through the store projections that root supplies, so this owner writes no row and the custodian edits nothing further. `Thaw` is fed EXCLUSIVELY what a trusted `Freeze` minted: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so the store carries an opaque content-keyed blob it never decodes, no peer document reaches `Thaw`, and the `$type` shape never crosses to a peer.
- Boundary: the verdict crosses to `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` as portable scalar data keyed by section, never a `VividOrange` assembly type, and `DesignBasis.Key` is that crossing's JURISDICTION column. Checks stand REFUSED at this altitude as standing law, never faked as arms: SLS DEFLECTION needs the span, the load distribution, and the modulus — none a `SectionCapacity` carries; RC PUNCHING SHEAR is a slab-column JUNCTION check over a control perimeter no cross-section carries; and the SEISMIC system coefficients are DEMAND-side scalars the load derivation consumes before a `Demand` ever reaches `Check`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    private SectionCapacity(DesignBasis basis) => Basis = basis;
    public DesignBasis Basis { get; }
    public ComponentAuthority Body => Basis.Body;

    [Equatable(Explicit = true)]
    public sealed partial record RcInteraction(
        [property: DefaultEquality] ComponentId Subject,
        [property: DefaultEquality] DiagramResolution Resolution,
        [property: IgnoreEquality] IForceMomentMesh Hull) : SectionCapacity(DesignBasis.En1992);

    public sealed record RcElastic(
        double TotalReinforcementAreaMm2,
        double TensionSteelAreaMm2,
        double ShearLinkAreaMm2,
        Option<double> FywdMpa,
        double ConcreteAreaMm2,
        double ReinforcementRatio,
        double GrossInertiaYyMm4,
        double GrossInertiaZzMm4,
        double ReinforcementInertiaYyMm4,
        double ReinforcementInertiaZzMm4,
        double EffectiveDepthMm,
        double DepthMm,
        double WidthMm,
        double FckMpa,
        double FctmMpa) : SectionCapacity(DesignBasis.En1992) {

        public double VrdMaxKn =>
            Math.Max(WidthMm, 1.0) * 0.9 * Math.Max(EffectiveDepthMm, 1.0) * 0.6
                * (1.0 - FckMpa / 250.0) * (FckMpa / 1.5) / (2.5 + 0.4) * 1e-3;

        public Fin<Seq<(PropertyName Row, PropertyValue Value)>> ShearLinkRows() =>
            ShearLinkAreaMm2 > 0.0
                ? FywdMpa.Match(
                    Some: fywd =>
                        from area in MeasureValue.Of(ShearLinkAreaMm2 * 1e-6, UnitsNet.Units.AreaUnit.SquareMeter)
                        from fywdPa in MeasureValue.Of(fywd * 1e6, UnitsNet.Units.PressureUnit.Pascal)
                        from ceiling in MeasureValue.Of(VrdMaxKn * 1e3, UnitsNet.Units.ForceUnit.Newton)
                        select Seq(
                            (StructuralRows.ShearLinkArea, (PropertyValue)new PropertyValue.Measure(area)),
                            (StructuralRows.ShearLinkYield, (PropertyValue)new PropertyValue.Measure(fywdPa)),
                            (StructuralRows.ShearLinkCeiling, (PropertyValue)new PropertyValue.Measure(ceiling))),
                    None: () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))
                : Fin.Succ(Seq<(PropertyName, PropertyValue)>());
    }

    public sealed record SteelMember(
        DesignBasis Basis,
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        double TorsionalKnm,
        CompactnessClass Classification,
        double Slenderness,
        double Chi,
        double ChiLt,
        double StiffnessRetention) : SectionCapacity(Basis);

    public sealed record TimberMember(
        double BendingKnm,
        double BendingMinorKnm,
        double CompressionKn,
        double ShearKn,
        double BearingPerpKnPerMm,
        double TorsionalKnm,
        double RelativeSlenderness,
        double Km,
        double Kmod) : SectionCapacity(DesignBasis.En1995);

    public sealed record AluminumMember(
        DesignBasis Basis,
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        Option<BucklingClass> Curve,
        double FoMpa,
        double FuMpa) : SectionCapacity(Basis) {

        public Seq<(PropertyName Row, PropertyValue Value)> BucklingRows() =>
            Curve.Map(static curve => Seq(
                    (StructuralRows.BucklingAlpha, (PropertyValue)new PropertyValue.Number(curve.Alpha)),
                    (StructuralRows.BucklingPlateau, (PropertyValue)new PropertyValue.Number(curve.Plateau))))
                .IfNone(Seq<(PropertyName, PropertyValue)>());
    }

    public sealed record MasonryUnreinforced(DesignBasis Basis, MasonrySection Wall) : SectionCapacity(Basis);

    public sealed record MasonryReinforced(DesignBasis Basis, MasonryCouple Couple) : SectionCapacity(Basis);

    public sealed record GlassPane(
        double BendingKnmPerM,
        double ResistanceMpa,
        double EffectiveThicknessMm,
        double LoadShareFraction) : SectionCapacity(DesignBasis.En16612);

    public sealed record Connection(
        DesignBasis Basis,
        Option<double> ShearKn,
        Option<double> TensionKn,
        Option<double> BearingKn,
        Option<LateralPair> Lateral = default,
        Option<MemberCheckRequirement> Defer = default) : SectionCapacity(Basis);

    public sealed record LateralPanel(
        DesignBasis Basis,
        double DesignKnPerM,
        LateralHazard Hazard) : SectionCapacity(Basis);

    public sealed record Fatigue(FatigueLaw Law) : SectionCapacity(Law.Basis);

    public sealed record BasePlate(
        double BearingKn,
        double PlateBendingKn) : SectionCapacity(DesignBasis.Aisc360);

    // --- [OPERATIONS]
    public Utilisation Check(Demand demand) => Switch(
        rcInteraction: h => Cast(h.Hull, demand),
        rcElastic: e => RcElasticUtilisation(e, demand),
        steelMember: s => SteelUtilisation(s, demand),
        timberMember: t => TimberUtilisation(t, demand),
        aluminumMember: a => AluminumUtilisation(a, demand),
        masonryUnreinforced: m => MasonryUtilisation(m, demand),
        masonryReinforced: m => MasonryReinforcedUtilisation(m, demand),
        glassPane: g => GlassUtilisation(g, demand),
        connection: c => ConnectionUtilisation(c, demand),
        lateralPanel: p => LateralUtilisation(p, demand),
        fatigue: f => FatigueUtilisation(f, demand),
        basePlate: p => BasePlateUtilisation(p, demand));

    static Utilisation RcElasticUtilisation(RcElastic e, Demand demand) {
        (Option<double> cracking, GoverningAction axis) = Cracking(e, demand);
        Option<double> shear = GuardedRatio(demand.ShearResultantKn, ShearResistanceKn(e));
        Option<MemberCheckRequirement> linked = e.ShearLinkAreaMm2 > 0.0 ? Some(MemberCheckRequirement.RcShearReinforcement) : None;
        return Worst(
            (cracking, axis, linked),
            (shear, GoverningAction.Shear, linked),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static (Option<double> Ratio, GoverningAction Governing) Cracking(RcElastic e, Demand demand) {
        double axialStress = demand.AxialKn * 1e3 / Math.Max(e.ConcreteAreaMm2, double.Epsilon);
        double bendingYStress = Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.GrossInertiaYyMm4, double.Epsilon);
        double bendingZStress = Math.Abs(demand.MomentZKnm) * 1e6 * (e.WidthMm * 0.5) / Math.Max(e.GrossInertiaZzMm4, double.Epsilon);
        GoverningAction governing = Math.Max(bendingYStress, bendingZStress) >= Math.Abs(axialStress)
            ? GoverningAction.Flexure : GoverningAction.Axial;
        return (FibreRatio(axialStress + bendingYStress + bendingZStress, e.FctmMpa), governing);
    }

    static double ShearResistanceKn(RcElastic e) {
        double d = Math.Max(e.EffectiveDepthMm, 1.0), bw = Math.Max(e.WidthMm, 1.0);
        double k = Math.Min(1.0 + Math.Sqrt(200.0 / d), 2.0);
        double rho = Math.Min(e.TensionSteelAreaMm2 / (bw * d), 0.02);
        double vrdc = Math.Max(0.12 * k * Math.Cbrt(100.0 * rho * e.FckMpa), 0.035 * Math.Pow(k, 1.5) * Math.Sqrt(e.FckMpa)) * bw * d * 1e-3;
        return e.ShearLinkAreaMm2 > 0.0 ? e.VrdMaxKn : vrdc;
    }

    static Utilisation SteelUtilisation(SteelMember s, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, s.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, s.FlexuralKnm)
            from minor in GuardedRatio(demand.MomentZKnm, s.FlexuralMinorKnm)
            select s.Basis.Interact(InteractionOperands.Of(axial, major, minor));
        return Worst(
            (combined, GoverningAction.Combined, None),
            (GuardedRatio(demand.ShearResultantKn, s.ShearKn), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, s.TorsionalKnm), GoverningAction.Torsion,
                s.TorsionalKnm > 0.0 ? None : Some(MemberCheckRequirement.SteelWarpingTorsion)),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Utilisation TimberUtilisation(TimberMember t, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, t.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, t.BendingKnm)
            from minor in GuardedRatio(demand.MomentZKnm, t.BendingMinorKnm)
            select t.Basis.Interact(new InteractionOperands(axial, major, minor, t.Km, AxialRegime.Of(t.RelativeSlenderness)));
        Option<MemberCheckRequirement> bearing =
            Math.Abs(demand.BearingKn) > double.Epsilon ? Some(MemberCheckRequirement.TimberBearingLength) : None;
        return Worst(
            (combined, GoverningAction.Combined,
                t.BendingMinorKnm > 0.0 || Math.Abs(demand.MomentZKnm) <= double.Epsilon
                    ? bearing : Some(MemberCheckRequirement.CltInPlaneBending)),
            (GuardedRatio(demand.ShearResultantKn, t.ShearKn), GoverningAction.Shear, bearing),
            (GuardedRatio(demand.TorsionKnm, t.TorsionalKnm), GoverningAction.Torsion, bearing));
    }

    static Utilisation AluminumUtilisation(AluminumMember a, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, a.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, a.FlexuralKnm)
            from minor in GuardedRatio(demand.MomentZKnm, a.FlexuralMinorKnm)
            select a.Basis.Interact(InteractionOperands.Of(axial, major, minor));
        return Worst(
            (combined, GoverningAction.Combined, Some(MemberCheckRequirement.AluminumMemberBuckling)),
            (GuardedRatio(demand.ShearResultantKn, a.ShearKn), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Utilisation FatigueUtilisation(Fatigue f, Demand demand) {
        Option<double> range = demand.StressRangeMpa <= double.Epsilon
            ? Some(0.0)
            : demand.CycleCount >= 1.0
                ? GuardedRatio(demand.StressRangeMpa, f.Law.DesignMpa(demand.CycleCount))
                : None;
        return Worst(
            (range, GoverningAction.Fatigue, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Utilisation BasePlateUtilisation(BasePlate p, Demand demand) {
        double download = Math.Max(0.0, -demand.AxialKn);
        return Worst(
            (GuardedRatio(download, p.BearingKn), GoverningAction.Bearing, None),
            (GuardedRatio(download, p.PlateBendingKn), GoverningAction.Flexure, None),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None));
    }

    static Utilisation MasonryUtilisation(MasonryUnreinforced m, Demand demand) {
        Option<MasonryResistances> resisted = m.Basis.Masonry.Map(algebra =>
            algebra.Unreinforced(m.Basis, m.Wall, Math.Max(0.0, -demand.AxialKn)));
        Option<double> combined =
            from r in resisted
            from axial in GuardedRatio(demand.AxialKn, demand.AxialKn > 0.0 ? 0.0 : r.Pn)
            from major in GuardedRatio(demand.MomentYKnm, r.Mnx)
            from minor in GuardedRatio(demand.MomentZKnm, r.Mny)
            select m.Basis.Interact(InteractionOperands.Of(axial, major, minor));
        double sigmaT = Math.Abs(demand.MomentYKnm) * 1e6 / Math.Max(m.Wall.SectionModulusXMm3, double.Epsilon)
            + Math.Abs(demand.MomentZKnm) * 1e6 / Math.Max(m.Wall.SectionModulusYMm3, double.Epsilon)
            + demand.AxialKn * 1e3 / Math.Max(m.Wall.NetAreaMm2, double.Epsilon);
        return Worst(
            (combined, GoverningAction.Combined, None),
            (resisted.Bind(r => FibreRatio(sigmaT, r.Tension)), GoverningAction.Flexure, None),
            (resisted.Bind(r => GuardedRatio(demand.ShearResultantKn, r.Vn)), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Utilisation MasonryReinforcedUtilisation(MasonryReinforced m, Demand demand) {
        Option<(double Pn, double Mn, double Phi, double PhiV)> couple =
            from algebra in m.Basis.Masonry
            let stresses = algebra.Reinforced(m.Basis, m.Couple)
            from fy in stresses.Fy
            let fm = stresses.Fm
            let block = m.Couple.SteelAreaMm2 * fy / Math.Max(fm * m.Couple.BedLengthMm, double.Epsilon)
            select (Pn: 0.80 * (fm * Math.Max(m.Couple.NetAreaMm2 - m.Couple.SteelAreaMm2, 0.0) + fy * m.Couple.SteelAreaMm2)
                        * m.Couple.SlendernessReduction * 1e-3,
                    Mn: m.Couple.SteelAreaMm2 * fy * Math.Max(m.Couple.EffectiveDepthMm - block / 2.0, 0.0) * 1e-6,
                    stresses.Phi, stresses.PhiV);
        Option<double> combined =
            from c in couple
            from fy in m.Couple.FyMpa
            from axial in GuardedRatio(demand.AxialKn,
                c.Phi * (demand.AxialKn > 0.0 ? m.Couple.SteelAreaMm2 * fy * 1e-3 : c.Pn))
            from major in GuardedRatio(demand.MomentYKnm, c.Phi * c.Mn)
            select m.Basis.Interact(InteractionOperands.Of(axial, major, minor: 0.0));
        double vnm = 0.083 * 2.25 * m.Couple.NetAreaMm2 * Math.Sqrt(m.Couple.FmMpa) * 1e-3;
        return Worst(
            (combined, GoverningAction.Combined, None),
            (GuardedRatio(demand.MomentZKnm, 0.0), GoverningAction.Flexure, None),
            (couple.Bind(c => GuardedRatio(demand.ShearResultantKn, c.PhiV * vnm)), GoverningAction.Shear,
                Some(MemberCheckRequirement.ReinforcedMasonryShearSpacing)),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Utilisation GlassUtilisation(GlassPane g, Demand demand) =>
        Worst(
            (GuardedRatio((Math.Abs(demand.MomentYKnm) + Math.Abs(demand.MomentZKnm)) * g.LoadShareFraction, g.BendingKnmPerM),
                GoverningAction.Flexure, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    static Utilisation ConnectionUtilisation(Connection c, Demand demand) =>
        Worst(
            (LateralRatio(c, demand), GoverningAction.Shear, c.Defer),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), c.TensionKn), GoverningAction.Axial, c.Defer),
            (GuardedRatio(demand.BearingKn, c.BearingKn), GoverningAction.Bearing, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None));

    static Utilisation LateralUtilisation(LateralPanel p, Demand demand) =>
        Worst(
            (GuardedRatio(demand.UnitShearKnPerM, p.DesignKnPerM), GoverningAction.InPlaneShear, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    static Option<double> LateralRatio(Connection c, Demand demand) =>
        c.Lateral.Match(
            Some: pair =>
                from primary in GuardedRatio(demand.ShearYKn, c.ShearKn)
                from secondary in GuardedRatio(demand.ShearZKn, pair.SecondKn)
                select pair.Rule.Fold(primary, secondary),
            None: () => GuardedRatio(demand.ShearResultantKn, c.ShearKn));

    static Option<double> GuardedRatio(double demand, Option<double> capacity) =>
        double.IsFinite(demand) && Math.Abs(demand) <= double.Epsilon
            ? Some(0.0)
            : capacity.Bind(held => GuardedRatio(demand, held));

    static Option<double> GuardedRatio(double demand, double capacity) =>
        (Math.Abs(demand) <= double.Epsilon, double.IsFinite(demand) && capacity > 0.0 && double.IsFinite(capacity)) switch {
            (true, _)      => Some(0.0),
            (false, true)  => Some(Math.Abs(demand) / capacity),
            (false, false) => None,
        };

    static Option<double> FibreRatio(double stress, double limit) =>
        (double.IsFinite(stress) && limit > 0.0, stress <= 0.0) switch {
            (true, _)      => Some(stress / limit),
            (false, true)  => Some(0.0),
            (false, false) => None,
        };

    static Utilisation Worst(params ReadOnlySpan<(Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)> candidates) {
        (Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer) won =
            Iterable<(Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)>.FromSpan(candidates[1..])
                .Fold(candidates[0], static (best, next) => (best.Ratio, next.Ratio) switch {
                    ({ IsSome: true, Case: double held }, { IsSome: true, Case: double rival }) => rival > held ? next : best,
                    ({ IsSome: true }, _) => next,
                    _ => best,
                });
        return (won.Ratio, won.Defer) switch {
            ({ IsSome: true, Case: double ratio }, { IsSome: true, Case: MemberCheckRequirement owed }) =>
                (Utilisation)new Utilisation.RequiresMemberCheck(ratio, won.Action, owed),
            ({ IsSome: true, Case: double ratio }, _) => new Utilisation.Bounded(ratio, won.Action),
            _ => new Utilisation.Unbounded(won.Action),
        };
    }

    // --- [BOUNDARIES]
    public static Fin<SectionCapacity> Resolve(CapacityBuild build) =>
        build.Switch(
            hull: h => Try.lift(() => Fin.Succ(new ForceMomentEngine(h.Section.Section, h.Resolution.ToSettings()).Mesh)).Run().Bind(static inner => inner)
                .Map(mesh => (SectionCapacity)new RcInteraction(h.Subject, h.Resolution, mesh)),
            elastic: e =>
                (e.Section.EffectiveDepthMm(SectionFace.Bottom).ToValidation((Error)new ComponentFault.EffectiveDepthUnavailable(e.Subject)),
                 e.Section.FaceSteelAreaMm2(SectionFace.Bottom).ToValidation((Error)new ComponentFault.TensionChordUnavailable(e.Subject)))
                    .Apply(static (depth, steel) => (Depth: depth, Steel: steel)).As().ToFin()
                    .Bind(chord => Try.lift(() => {
                        double fck = EnConcreteFactory.CreateLinearElastic(e.Section.Concrete.Grade).Strength.Megapascals;
                        return Fin.Succ<SectionCapacity>(new RcElastic(
                            e.Section.GrossSteelAreaMm2,
                            chord.Steel,
                            e.Section.ShearLinkAreaMm2,
                            e.Section.LinkYieldMpa.Map(static fyk => DesignBasis.En1992.Resist(ResistanceAction.Reinforcement, fyk)),
                            e.Section.ConcreteAreaMm2,
                            e.Section.ReinforcementRatio,
                            e.Section.Properties.MomentOfInertiaYy.MillimetersToTheFourth,
                            e.Section.Properties.MomentOfInertiaZz.MillimetersToTheFourth,
                            e.Section.ReinforcementInertiaYyMm4,
                            e.Section.ReinforcementInertiaZzMm4,
                            chord.Depth,
                            e.Section.ConcreteProfile.GrossRectangleMm.DepthMm.Value,
                            e.Section.ConcreteProfile.GrossRectangleMm.WidthMm.Value,
                            fck,
                            Fctm(fck)));
                    }).Run().Bind(static inner => inner)),
            detail: d => Fin.Succ((SectionCapacity)new Fatigue(d.Law)),
            anchorage: a => Anchoring(a),
            bearing: b => Fin.Succ(Baseplating(b.Plate)));

    public static Fin<SectionCapacity> Lift(CapacityLift lift) => lift.Switch(
        steel: static r => Held(new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0)),
        timber: static r => Held(new TimberMember(
            r.Capacity.BendingNmm * 1e-6, r.Capacity.BendingMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.BearingPerpNPerMm * 1e-3, r.Capacity.TorsionalNmm * 1e-6,
            r.Capacity.RelativeSlenderness, r.Capacity.Km, r.Capacity.Kmod)),
        deckSheet: static r => Held(new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0)),
        masonry: static r => Held(new MasonryUnreinforced(r.Basis, new MasonrySection(
            r.Strength.FmMpa, r.Section.AreaMm2.Value, r.Section.SxMm3.Value, r.Section.SyMm3.Value,
            MasonryReduction.Of(r.Basis, r.HeightMm, r.Section.GoverningRadiusMm).Value,
            r.Basis == DesignBasis.En1996
                ? r.Flexural.FxkMpa(r.Mortar, r.Rupture)
                : r.Rupture.FrMpa(r.System, r.Mortar),
            r.Basis == DesignBasis.En1996 ? r.Flexural.Fvk0Mpa(r.Mortar) : TmsRunningBondShearMpa))),
        reinforcedMasonry: static r => Held(new MasonryReinforced(r.Basis, new MasonryCouple(
            r.Strength.FmMpa,
            ReinforcingYieldMpa(r.Bar),
            r.Unit.ReinforcedCells * Math.PI / 4.0 * r.Unit.RebarBarMm * r.Unit.RebarBarMm,
            r.Section.AreaMm2.Value, r.Unit.WMm / 2.0, r.Unit.LMm,
            MasonryReduction.Of(r.Basis, r.HeightMm, r.Section.GoverningRadiusMm).Value))),
        glass: static r => Held(new GlassPane(r.Capacity.StripBendingKnmPerM, r.Capacity.ResistanceMpa, r.Capacity.EffectiveThicknessMm, r.Capacity.LoadShareFraction)),
        steelFire: static r => Held(new SteelMember(
            r.Ambient.Basis,
            r.Ambient.FlexuralNmm * r.Ky * 1e-6, r.Ambient.FlexuralMinorNmm * r.Ky * 1e-6, r.Ambient.CompressionN * r.Ky * 1e-3,
            r.Ambient.ShearN * r.Ky * 1e-3, r.Ambient.TorsionalNmm * r.Ky * 1e-6, r.Ambient.Classification, r.Ambient.Slenderness,
            r.Ambient.Chi, r.Ambient.ChiLt, StiffnessRetention: r.Ke)),
        timberFire: static r => Held(new TimberMember(
            r.Residual.BendingNmm * 1e-6, r.Residual.BendingMinorNmm * 1e-6, r.Residual.CompressionN * 1e-3,
            r.Residual.ShearN * 1e-3, r.Residual.BearingPerpNPerMm * 1e-3, r.Residual.TorsionalNmm * 1e-6,
            r.Residual.RelativeSlenderness, r.Residual.Km, r.Residual.Kmod)),
        weld: static r => Held(new Connection(DesignBasis.AwsD11, r.Row.DirectionalShearKn(Angle.FromDegrees(r.LoadAngleDeg)), None, None)),
        adhesive: static r => Held(new Connection(DesignBasis.AstmD1002, Some(r.Row.DesignShearKn), r.Row.DesignTensionKn, None)),
        stud: static r => Held(new Connection(DesignBasis.Aisc360, Some(Math.Max(r.Count, 0) * r.Row.DesignShearKn(r.Group)), None, None)),
        connector: static r => Held(new Connection(DesignBasis.IccEs,
            r.Capacity.LateralF1Kn, r.Capacity.UpliftKn, r.Capacity.DownloadKn,
            r.Capacity.LateralF2Kn.Map(second => new LateralPair(second, r.Capacity.Rule)))),
        lateralPanel: static r => Held(new LateralPanel(DesignBasis.Sdpws, r.DesignKnPerM, r.Hazard)),
        bolt: r =>
            from shear in r.Assembly.ShearResistanceKn(r.Plane, DesignBasis.En1993Joints)
            from tension in r.Assembly.TensionResistanceKn(DesignBasis.En1993Joints)
            from bearing in r.Assembly.BearingResistanceKn(r.Bearing, DesignBasis.En1993Joints)
            select (SectionCapacity)new Connection(DesignBasis.En1993Joints, Some(shear), Some(tension), Some(bearing)),
        slipCritical: static r => Held(new Connection(DesignBasis.En1993Joints, r.Assembly.SlipResistanceKn(r.Install), None, None)),
        timberDowel: static r => Held(new Connection(DesignBasis.En1995, Some(Math.Max(r.Planes, 0) * r.PerPlaneShearKn), None, None)),
        aluminum: static r => Held(new AluminumMember(
            r.Basis,
            r.Basis.Resist(ResistanceAction.Stability, r.Section.SxMm3.Value * r.FoMpa * 1e-6),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.SyMm3.Value * r.FoMpa * 1e-6),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.AreaMm2.Value * r.FoMpa * 1e-3),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.AvyMm2.Value * r.FoMpa / Math.Sqrt(3.0) * 1e-3),
            BucklingCurve(r.Grade),
            r.FoMpa, r.FuMpa)));

    static Fin<SectionCapacity> Held(SectionCapacity capacity) => Fin.Succ(capacity);

    static Option<double> ReinforcingYieldMpa(MaterialGrade grade) =>
        grade.Columns is GradeProperties.Rebar rebar ? rebar.YieldMpa : None;

    static Option<BucklingClass> BucklingCurve(MaterialGrade grade) =>
        grade.Columns is GradeProperties.Aluminum alloy ? Some(alloy.Class) : None;

    static Fin<SectionCapacity> Anchoring(CapacityBuild.Anchorage a) {
        double k1 = a.Bed.Cracked ? 8.9 : 12.7;
        double edge = a.Bed.EdgeMm.Map(ca => Math.Min(0.7 + 0.3 * ca / (1.5 * a.Bed.HefMm.Value), 1.0)).IfNone(1.0);
        double coneKn = DesignBasis.En1992Anchors.Resist(ResistanceAction.CrossSection,
            k1 * Math.Sqrt(a.Bed.FckMpa) * Math.Pow(a.Bed.HefMm.Value, 1.5) * edge * 1e-3);
        return from shear in a.Assembly.ShearResistanceKn(a.Plane, DesignBasis.En1992Anchors)
               from tension in a.Assembly.TensionResistanceKn(DesignBasis.En1992Anchors)
               select (SectionCapacity)new Connection(
                   DesignBasis.En1992Anchors, Some(shear), Some(Math.Min(tension, coneKn)), None,
                   Defer: Some(MemberCheckRequirement.AnchorForwardModes));
    }

    static SectionCapacity Baseplating(PlateBed plate) {
        double b = plate.WidthMm.Value, n = plate.LengthMm.Value, t = plate.ThicknessMm.Value;
        double bearingKn = 0.65 * 0.85 * plate.FcMpa * b * n * Math.Clamp(plate.ConfinementRatio, 1.0, 2.0) * 1e-3;
        double mArm = (n - 0.95 * plate.ColumnDepthMm.Value) / 2.0;
        double nArm = (b - 0.8 * plate.ColumnFlangeMm.Value) / 2.0;
        double nPrime = Math.Sqrt(plate.ColumnDepthMm.Value * plate.ColumnFlangeMm.Value) / 4.0;
        double l = Math.Max(Math.Max(mArm, nArm), nPrime);
        double bendingKn = 0.9 * plate.FyMpa * b * n * t * t / (2.0 * Math.Max(l, double.Epsilon) * Math.Max(l, double.Epsilon)) * 1e-3;
        return new BasePlate(bearingKn, bendingKn);
    }

    const double TmsRunningBondShearMpa = 0.386;

    static double Fctm(double fckMpa) =>
        fckMpa <= 50.0 ? 0.30 * Math.Pow(fckMpa, 2.0 / 3.0) : 2.12 * Math.Log(1.0 + (fckMpa + 8.0) / 10.0);

    internal static Fin<string> Freeze(RcInteraction capacity) =>
        Try.lift(() => Fin.Succ(capacity.Hull.ToJson())).Run().Bind(static inner => inner);

    internal static Fin<SectionCapacity> Thaw(ComponentId subject, DiagramResolution resolution, string json) =>
        Try.lift(() => Fin.Succ(json.FromJson<IForceMomentMesh>())).Run().Bind(static inner => inner)
            .Bind(mesh => mesh is null
                ? Fin.Fail<SectionCapacity>(new ComponentFault.CapacityDocumentEmpty(subject))
                : Fin.Succ((SectionCapacity)new RcInteraction(subject, resolution, mesh)));

    static Utilisation Cast(IForceMomentMesh hull, Demand demand) {
        GoverningAction governing = demand.MomentResultantKnm > double.Epsilon
            ? GoverningAction.BiaxialMoment
            : GoverningAction.Axial;
        Option<double> ray = Math.Abs(demand.AxialKn) <= double.Epsilon && demand.MomentResultantKnm <= double.Epsilon
            ? Some(0.0)
            : toSeq(hull.Faces)
                .Map(face => Pierce(face, demand.AxialKn, demand.MomentYKnm, demand.MomentZKnm))
                .Somes()
                .Filter(static multiplier => multiplier > 0.0)
                .Fold(Option<double>.None, static (best, multiplier) => Some(best.Map(won => Math.Min(won, multiplier)).IfNone(multiplier)))
                .Map(static multiplier => 1.0 / multiplier);
        return Worst(
            (ray, governing, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    static Option<double> Pierce(IForceMomentTriFace face, double dN, double dMy, double dMz) {
        (double ax, double ay, double az) = Coord(face.A);
        (double e1x, double e1y, double e1z) = Sub(Coord(face.B), (ax, ay, az));
        (double e2x, double e2y, double e2z) = Sub(Coord(face.C), (ax, ay, az));
        (double px, double py, double pz) = Cross((dN, dMy, dMz), (e2x, e2y, e2z));
        double determinant = e1x * px + e1y * py + e1z * pz;
        double edgeNormSquared = e1x * e1x + e1y * e1y + e1z * e1z;
        double crossNormSquared = px * px + py * py + pz * pz;
        double determinantTolerance = 1e-12 * Math.Sqrt(edgeNormSquared * crossNormSquared);
        if (Math.Abs(determinant) <= determinantTolerance) return None;
        double inverse = 1.0 / determinant;
        double u = -(ax * px + ay * py + az * pz) * inverse;
        if (u is < 0.0 or > 1.0) return None;
        (double qx, double qy, double qz) = Cross((-ax, -ay, -az), (e1x, e1y, e1z));
        double v = (dN * qx + dMy * qy + dMz * qz) * inverse;
        if (v < 0.0 || u + v > 1.0) return None;
        return (e2x * qx + e2y * qy + e2z * qz) * inverse;
    }

    static (double, double, double) Coord(IForceMomentVertex vertex) =>
        (vertex.X.Kilonewtons, vertex.Y.KilonewtonMeters, vertex.Z.KilonewtonMeters);

    static (double, double, double) Sub((double x, double y, double z) left, (double x, double y, double z) right) =>
        (left.x - right.x, left.y - right.y, left.z - right.z);

    static (double, double, double) Cross((double x, double y, double z) left, (double x, double y, double z) right) =>
        (left.y * right.z - left.z * right.y, left.z * right.x - left.x * right.z, left.x * right.y - left.y * right.x);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public readonly record struct HullCache(SectionCapacity Capacity, Option<string> Pending) {
    public static string Key(ComponentId subject, DiagramResolution resolution) => $"{subject.Value}:{resolution.Key}";

    public static Fin<HullCache> Of(CapacityBuild.Hull build, Func<string, Option<string>> read) =>
        read(Key(build.Subject, build.Resolution)).Match(
            Some: body => SectionCapacity.Thaw(build.Subject, build.Resolution, body)
                .Map(capacity => new HullCache(capacity, None)),
            None: () => SectionCapacity.Resolve(build)
                .Bind(capacity => SectionCapacity.Freeze((SectionCapacity.RcInteraction)capacity)
                    .Map(body => new HullCache(capacity, Some(body)))));
}
```

## [06]-[SECTION_SELECTION]

- Owner: `SectionSelection` is the INVERSE of `Check` — design as selection over a produced candidate sequence under ONE law. `SectionCandidate<TSubject>` is that sequence's element: the subject the query returns, the linear mass it ranks by, and the DEFERRED capacity the demand is checked against; `Least` is the one acceptance fold; the three producers are the only thing that differs between a catalogue query, a fabricated-member sweep, and a bolt sizing scan.
- Cases: `Stocked` scans the frozen catalogue (a section someone stocked); `Fabricated` sweeps a caller-parameterized composition space (a section nobody stocked yet), solving each candidate HERE because a fabricated section has no catalogue entry to have solved it; `Threaded` sweeps the (thread × grade) table the fastener standards tables publish, which is what makes every catalogued thread and grade REACHABLE — a row no `Stocked` selection names is still selected the moment its size and system fit the demand, so the tables are the admission domain rather than decoration.
- Entry: `SectionSelection.Least(candidates, demand, key)` over any producer's output. Mass is `Area × ρ(substance)`, never area alone: area ranks linear mass only INSIDE one substance, so a mixed steel/timber/masonry catalogue ordered by area returns a 90 mm sawn section ahead of every W-shape. Density arrives through the caller's `densityOf` projection (the composing root binds it to `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Lookup(id, key)`'s Mechanical density column), so `admit` stays a genuine POLICY filter — a stocked subset, a depth cap, one `SteelClass` — rather than a correctness precondition the type system never enforced.
- Growth: a new search space is one producer returning the ranked candidate sequence; the fold, the acceptance rule, and the exhaustion fault are already written.
- Boundary: the capacity is a THUNK because only the lightest passing candidate is worth pricing — an eager capacity column pays a hull solve per catalogue row. The fold is therefore LAZY and halts on the first pass OR the first fault: a candidate whose density or capacity FAULTS aborts the scan loud (a filter admitting a family the projections cannot price is a caller defect, never a silently skipped row), and an exhausted search faults typed.
- Boundary: acceptance is the SECTION-altitude verdict, so a linked RC section that passes and merely owes stirrup detailing returns WITH its deferral for the caller to route forward — the strict `Adequate` bit stays the terminal report's, never the sizing gate's.
- Boundary: NAMED LOSS on absorbing the fastener-grain scan — the retired `LeastShear` returned the winning EN 1993-1-8 resistance and ranked by thread major diameter; this fold returns the `Utilisation` verdict and ranks by real linear mass. WITNESS: the resistance is `demand / verdict.Ratio` off the returned verdict, and the mass rank orders a mixed-grade sweep correctly where diameter ordered it only within one substance.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SectionCandidate<TSubject>(TSubject Subject, double MassPerMm, Func<Fin<SectionCapacity>> Capacity);

public readonly record struct BoltJoint(
    ComponentId Subject,
    ThreadSeries Series,
    BoltCategory Category,
    FayingSurface Faying,
    HeadForm Head,
    int GripPlies,
    int ShearPlanes,
    Option<HexHardware> Washer,
    BearingDesign Bearing,
    ShearPlane Plane);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionSelection {
    public static Fin<Seq<SectionCandidate<Component>>> Stocked(
        FrozenDictionary<ComponentId, Component> rows,
        FrozenDictionary<ComponentId, ComputedSection> sections,
        Func<Component, bool> admit,
        CapacityPlacement placement,
        Func<MaterialId, Fin<double>> densityOf) =>
        toSeq(sections)
            .Filter(pair => rows.ContainsKey(pair.Key) && admit(rows[pair.Key]))
            .Traverse(pair => densityOf(rows[pair.Key].SubstanceId).Map(density =>
                Candidate(rows[pair.Key], pair.Value, density, placement)))
            .As();

    public static Fin<Seq<SectionCandidate<Component>>> Fabricated(
        Func<int, Seq<(Component Row, SectionProfile Profile)>> sweep,
        int sweeps,
        CapacityPlacement placement,
        Func<MaterialId, Fin<double>> densityOf) =>
        toSeq(Enumerable.Range(0, Math.Max(sweeps, 0))).Bind(sweep)
            .Traverse(candidate => SectionSolver.Solve(candidate.Profile)
                .Bind(section => densityOf(candidate.Row.SubstanceId)
                    .Map(density => Candidate(candidate.Row, section, density, placement))))
            .As();

    public static Fin<Seq<SectionCandidate<(ThreadRow Thread, MaterialGrade Grade)>>> Threaded(
        BoltJoint joint,
        Func<MaterialId, Fin<double>> densityOf) =>
        toSeq(Threads.Rows)
            .Filter(thread => thread.Series == joint.Series)
            .Bind(thread => toSeq(MaterialGrade.Items)
                .Filter(grade => grade.Family == ComponentFamily.Fastener && grade.Admits(thread))
                .Map(grade => (Thread: thread, Grade: grade)))
            .Traverse(pair => densityOf(pair.Grade.Substance).Map(density =>
                new SectionCandidate<(ThreadRow, MaterialGrade)>(
                    pair,
                    pair.Thread.StressAreaMm2 * density,
                    () => FastenerAssembly.Of(pair.Thread, pair.Grade, joint.Category, joint.Faying, joint.Head,
                            joint.GripPlies, joint.ShearPlanes, joint.Washer)
                        .Bind(assembly => SectionCapacity.Lift(
                            new CapacityLift.Bolt(joint.Subject, assembly, joint.Bearing, joint.Plane))))))
            .As();

    public static Fin<(TSubject Subject, Utilisation Verdict)> Least<TSubject>(
        Fin<Seq<SectionCandidate<TSubject>>> candidates,
        Demand demand) =>
        candidates.Bind(ranked =>
            toSeq(ranked.OrderBy(static candidate => candidate.MassPerMm))
                .Map(candidate => candidate.Capacity().Map(capacity => (candidate.Subject, Verdict: capacity.Check(demand))))
                .Choose(static priced => priced.Match(
                    Succ: static won => won.Verdict.SectionPasses ? Some(Fin.Succ(won)) : None,
                    Fail: static fault => Some(Fin.Fail<(TSubject Subject, Utilisation Verdict)>(fault))))
                .Head
                .IfNone(() => Fin.Fail<(TSubject Subject, Utilisation Verdict)>(
                    new ComponentFault.SelectionExhausted(typeof(TSubject)))));

    static SectionCandidate<Component> Candidate(Component row, ComputedSection section, double density,
        CapacityPlacement placement) =>
        new(row, section.AreaMm2.Value * density, () => row.Family.Capacity(row, Some(section), placement));

}
```

## [07]-[RESEARCH]

(none)
