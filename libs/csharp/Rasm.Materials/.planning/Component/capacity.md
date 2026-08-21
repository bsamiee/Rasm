# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Component` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the realized `ComponentFamily` structural rails: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section, AND the EC2 §6.2 section-level shear screen), `SteelMember` (the `steel#STEEL_FAMILY` `DesignCapacity` design-resistance receipt lifted whole under the basis it names — the AISC 360 φ-form or the EN 1993-1-1 γM-divided resistances — with `CompactnessClass`/slenderness; the AISI deck receipt and the EN 1993-1-2 fire state land the same case), `TimberMember` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` receipt lifted whole), and `MasonryUnreinforced` (the axial-flexural unity check AND the flexural-tension screen over the cmu `MaterialGrade` row's `GradeProperties.Cmu` `f'm` + grouted `ComputedSection` + the `masonry#MASONRY_FAMILY` mortar-keyed row feed). Every case binds a `DesignBasis`: the JURISDICTION axis carrying the authority body, the `SafetyFormat`, the partial-factor and resistance-factor columns ONE `Resist` fold reads, the `NationalAnnex`-threaded typed `IStandard` citation, the interaction kernel `Check` dispatches, and — where the jurisdiction publishes one — the masonry resistance algebra. A second design code for an already-cased family is a BASIS ROW, never a sibling case forking the closed `GoverningAction`/`Utilisation` verdict vocabulary the `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` consumer keys on, and a case name spelling one code is the deleted form. A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules lifts its already-computed receipt into ONE case here, and the RC, fatigue, anchorage, and base-plate surfaces are `Resolve` builds over their DECLARED inputs — the design-code COMPUTATION stays the family owner's where a family owns it, the unified VERDICT this owner's. The rail is TOTAL over the load path: `MasonryReinforced` carries the TMS 402 §9.3 steel-couple arm, `GlassPane` the EN 16612 pane resistance the glazing family lifts, `Connection` the weld/adhesive/stud/connector/anchor receipts, `AluminumMember` the EN 1999-1-1 elastic-floor resistances over the banded (fo, fu) pair, `Fatigue` the ONE detail-category S-N law spanning the EN 1993-1-9 fourteen-rung ladder and the AISC 360 Appendix 3 A–E′ constants, and `BasePlate` the AISC DG1 bearing/plate-thickness pair — one `Check` from cross-section to weld to hanger to anchor — while `SectionSelection` is the rail's INVERSE query, ONE least-MASS passing scan over three declared candidate sources: the frozen catalogue the full-database steel seed supplies, a caller-parameterized composition sweep, and the (thread × grade) bolt lattice the fastener standards tables publish. This owner is the ULTIMATE complement to `component#COMPONENT_OWNER` `SectionSolver`: that solver gives the elastic `ComputedSection` every family solves from its `SectionProfile` arm, THIS owner gives the reinforced-section transformed properties, the EC2 section-level shear screen, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so `HullCache` pays that sweep ONCE per `(ComponentId, DiagramResolution)` pair and hands the frozen body to the artifact row a composition root writes. The page composes `reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck`, the `steel`/`timber`/`cmu` sibling receipts, the in-folder `UnitsNet` quantity coercion at the edge, and the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` structural-Assessment route by `MaterialId`/section key, host-neutral here.

## [01]-[INDEX]

- [02]-[DESIGN_BASIS]: `DesignBasis` rows the jurisdiction algebra — `SafetyFormat` carrying `Resist`/`Reduce`, `ResistanceAction` the factor axis both formats key on with the `SectionFactors` pair it assembles, `MasonryAlgebra` the two masonry folds over `MasonrySection`/`MasonryCouple`, `LateralHazard`, `AxialRegime`, `InteractionOperands`, `DiagramResolution`, and the type-init parity census over the shared MEMBER key set.
- [03]-[FATIGUE_LAW]: `FatigueLaw` folds the two-ladder S-N algebra over `FatigueAssessment`, `EnFatigueCategory`, and `AiscFatigueCategory`.
- [04]-[DEMAND_VERDICT]: `CapacityBuild` requests every DECLARED build (`Hull` · `Elastic` · `Detail` · `Anchorage` · `Bearing`) over `AnchorBed`/`PlateBed`/`AnchorPlacement`; `CapacityReceipt` requests every sibling-receipt lift and mints the fire pair off the `FireState` input contract; `Demand` and its `DemandColumn`/`DemandBand` roster admit the action vector; `CapacityPlacement` threads the placement currency; `GoverningAction`, `Utilisation`, `MemberCheckRequirement`, `MasonryReduction`, and `LateralRule`/`LateralPair` close the verdict vocabulary.
- [05]-[SECTION_CAPACITY]: `SectionCapacity` closes the capacity family, `Check` folds every arm over the `GuardedRatio`/`FibreRatio`/`Worst` candidate algebra, and the boundary block holds `Resolve`, the TOTAL `Lift`, `HullCache` with its `Freeze`/`Thaw` round trip, and the Möller–Trumbore hull kernel.
- [06]-[SECTION_SELECTION]: `SectionSelection` inverts `Check` — `SectionCandidate` over three producers (`Stocked` · `Fabricated` · `Threaded`), `BoltJoint` declaring what a thread/grade sweep holds fixed, and ONE `Least` fold.
- [07]-[RESEARCH]: terminal.

## [02]-[DESIGN_BASIS]

- Owner: `DesignBasis` is the jurisdiction row every `SectionCapacity` case binds — authority body, `SafetyFormat`, the partial-factor and resistance-factor columns, the annex-threaded `IStandard` citation, the `Interact(InteractionOperands)` combined-action kernel, and the optional `MasonryAlgebra` — so a second code over an already-cased family is one ROW and a case name spelling one code is the deleted form. `SafetyFormat` owns how a nominal strength becomes a design resistance AND how a tabulated lateral nominal reduces; `ResistanceAction` owns which factor a fold takes and assembles the `SectionFactors` (φ, γ) pair the format consumes — the ALTITUDE WORD keeping it apart from the member-altitude `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` `ResistanceFactors` φ column set exactly as `SectionCapacity` is kept apart from `MemberCapacity` — so no arm re-spells "×φ or ÷γM"; `MasonryAlgebra` owns the whole per-jurisdiction masonry resistance difference; `LateralHazard` owns the SDPWS reduction pair; `InteractionOperands` carries the already-normalized ratios a kernel folds and nothing else.
- Entry: `basis.Resist(action, nominal)` is the ONE nominal-to-design fold; `basis.Interact(operands)` the ONE combined-action kernel; `basis.Standard(annex)` the citation under the project's annex; `format.Reduce(hazard, nominal)` the lateral reduction, `Option`-shaped because SDPWS serves the two US formats alone.
- Growth: a new jurisdiction is one `DesignBasis` row — body, format, factors, citation, `Interact` kernel, and (masonry only) its algebra row; a new resistance class is one `ResistanceAction` row with one cell on each format's column; a new masonry jurisdiction is one `MasonryAlgebra` row, never a branch inside a fold.
- Boundary: `DesignBasis` MEMBER keys ARE the `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` `DesignCode` roster spelled identically — one vocabulary carried by two typed rows because the branch strata forbid a reference in either direction — so the parity census below DERIVES the claim at type-init rather than asserting it in prose (Materials `RULINGS.md [02]`). The section-and-load-path-only keys (`en16612` glazing, `en1993-1-8`/`aws-d1-1`/`astm-d1002`/`icc-es` connection, `en1992-4` anchorage, `en1993-1-9`/`aisc-app3` fatigue) are the DECLARED carve with no member-check counterpart and never cross.
- Boundary: γM2 has ONE authority — the `DesignBasis` partial-factor column read through `ResistanceAction.Fracture` — so a family page divides joint and fracture resistances through this row and a local copy is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;  // frees the bare name for the SectionCapacity owner
using VividOrange.Sections;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Serialization;
using VividOrange.Standards;
using VividOrange.Standards.Eurocode;
using UnitsNet;
using Dimension = Rasm.Element.Properties.Dimension;  // the SI-dimension axis, disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;

// The ONE flat Rasm.Materials.Component namespace binds every family owner this page lifts receipts from by bare
// name (the codemap maps Component/Capacity.cs flat and dotnet_style_namespace_match_folder forces the folder path).
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The Steps knob drives a Steps² strain-plane sweep, so the band trades hull fidelity for solve cost rather than
// scattering a DiagramSettings ctor at the call site (.api/api-vividorange-interactiondiagram.md [03]-[ENTRYPOINTS]).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DiagramResolution {
    public static readonly DiagramResolution Draft    = new("draft",    steps: 16, concreteMaxAreaMm2: 500.0, rebarDivisions: 12);
    public static readonly DiagramResolution Standard = new("standard", steps: 30, concreteMaxAreaMm2: 250.0, rebarDivisions: 16);
    public static readonly DiagramResolution Fine     = new("fine",     steps: 48, concreteMaxAreaMm2: 120.0, rebarDivisions: 24);
    public int Steps { get; }
    public double ConcreteMaxAreaMm2 { get; }
    public int RebarDivisions { get; }

    // The rebar mesh takes 0.8x the concrete max face area at the same 25 degree minimum-angle quality constraint,
    // matching the engine's shipped 250/200 mm² default ratio.
    public DiagramSettings ToSettings() =>
        new(Area.FromSquareMillimeters(ConcreteMaxAreaMm2), Angle.FromDegrees(25.0),
            Area.FromSquareMillimeters(ConcreteMaxAreaMm2 * 0.8), Angle.FromDegrees(25.0), RebarDivisions, Steps);
}

// The two factors a resistance fold can take, assembled by the action and consumed by the format — never read as a
// pair anywhere else, so no arm can apply one jurisdiction's factor under another's format. `SectionFactors` takes
// its ALTITUDE WORD for the reason `SectionCapacity` takes one against the member-altitude `MemberCapacity`: this
// pair is the SECTION-side (φ, γ) one `ResistanceAction` assembles, never the member-side
// `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` `ResistanceFactors`, that page's per-`DesignCode` four-action φ
// column set. Both spellings stay live on their own sides of the seam, and one spelling across it would let a
// member-check φ set reach a section fold that reads (φ, γ).
public readonly record struct SectionFactors(double Phi, double Gamma);

// The SAFETY-FORMAT axis, keys shared with Rasm.Compute/Analysis/capacity#DESIGN_CHECK: φ-format multiplies,
// partial-factor divides, and an ALLOWABLE body publishes values already reduced — so its Resist arm is identity and
// no Ω column is minted for a slot no admitted body prints. Reduce answers None on limit-state because SDPWS §4.1.4
// defines its lateral reduction for the two US formats alone.
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

// The RESISTANCE CLASS a fold is pricing — a φ-format jurisdiction's per-action factors and a partial-factor
// jurisdiction's γ set as ONE roster read twice. EN 1993 separates γM0 cross-section, γM1 stability, and γM2
// fracture-and-joints; EN 1992 pairs γc with the reinforcement γs. A row mixing them prices a joint through the
// cross-section factor: the wrong factor under a right-looking number.
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

// SDPWS 2021 publishes a SINGLE nominal per configuration and expresses the wind-versus-seismic distinction as the
// factor applied to it, so a second seeded nominal column would fork the table it transcribes. The reduction reads
// BOTH axes, which is why the hazard rides the placement and the SELECTION rides SafetyFormat.Reduce.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralHazard {
    public static readonly LateralHazard Wind    = new("wind",    asdDivisor: 2.0, lrfdFactor: 0.80);
    public static readonly LateralHazard Seismic = new("seismic", asdDivisor: 2.8, lrfdFactor: 0.50);
    public double AsdDivisor { get; }
    public double LrfdFactor { get; }

    // An unserved (format, hazard) pair reports not-applicable instead of borrowing whichever factor sat in the
    // other branch.
    public Fin<double> Design(double nominalKnPerM, SafetyFormat format, Op key) =>
        format.Reduce(this, nominalKnPerM)
            .ToFin(new ComponentFault.LateralFormatUnsupported(key, format, this));
}

// EN 1995-1-1 selects the AXIAL term by regime: §6.3.2 eq 6.23/6.24 takes it linear over the already k_c-reduced
// N_Rd where buckling governs, §6.2.4 eq 6.19/6.20 squares it for the stocky member. The λ_rel = 0.3 break is the
// code's own, so the row owns the threshold AND the term and no kernel re-spells either.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialRegime {
    public static readonly AxialRegime Buckling = new("buckling", term: static axial => axial);
    public static readonly AxialRegime Stocky   = new("stocky",   term: static axial => axial * axial);
    [UseDelegateFromConstructor] public partial double Term(double axial);
    public static AxialRegime Of(double relativeSlenderness) => relativeSlenderness > 0.3 ? Buckling : Stocky;
}

// The three ALREADY-NORMALIZED ratios a kernel folds plus the two shape facts EC5 alone reads (§6.1.6(2) k_m and
// the axial-term regime): the arms hand the basis a pure dimensionless triple, so a row owns the INTERACTION ALGEBRA
// and never re-reads a capacity column. Of is the three-ratio form every other jurisdiction states, so the neutral
// pair is spelled once rather than as two literals at four call sites.
public readonly record struct InteractionOperands(double Axial, double Major, double Minor, double MinorWeight, AxialRegime Regime) {
    public static InteractionOperands Of(double axial, double major, double minor) =>
        new(axial, major, minor, MinorWeight: 1.0, AxialRegime.Buckling);
}

// The unreinforced-wall section facts a masonry resistance fold reads, hoisted off the capacity case so the two
// jurisdictions' algebra lives beside the basis row that selects it and neither reaches into the capacity family.
// FlexuralTensionMpa is the CHARACTERISTIC tension-fibre limit the lift resolved off the basis's own table (TMS 402
// Table 9.1.9.2 fr, or EN 1996-1-1 Table 3.4 f_xk); ShearBondMpa the zero-compression bond both codes publish.
public readonly record struct MasonrySection(
    double FmMpa, double NetAreaMm2, double SectionModulusXMm3, double SectionModulusYMm3,
    double SlendernessReduction, double FlexuralTensionMpa, double ShearBondMpa);

// The reinforced-wall couple facts: f'm, the reinforced-cell steel area, the grouted net area, the out-of-plane
// lever d, the per-unit bed length b, the slenderness reduction, and the bar yield — OPTIONAL because the yield
// rides the ONE MaterialGrade row and a grade arm publishing none states absence rather than a fabricated stress.
public readonly record struct MasonryCouple(
    double FmMpa, Option<double> FyMpa, double SteelAreaMm2, double NetAreaMm2,
    double EffectiveDepthMm, double BedLengthMm, double SlendernessReduction);

public readonly record struct MasonryResistances(double Pn, double Mnx, double Mny, double Tension, double Vn);

// Fm/Fy are the stress scalars the couple algebra multiplies; Phi/PhiV the resistance factors it applies AFTER,
// unity on a partial-factor basis whose strengths arrived already divided so no factor lands twice.
public readonly record struct ReinforcedStresses(double Fm, Option<double> Fy, double Phi, double PhiV);

// The WHOLE per-jurisdiction masonry difference as two rows, so the folds carry no basis branch. TMS 402 §9.2/§9.3:
// the 0.80 accidental-eccentricity coefficient over the 0.80 stress-block cap (a full-f'm fibre over-prices flexure
// 25%), φ·fr = 0 making net tension govern outright, the §9.2.6.1 ceiling clamping the RESOLVED shear at the outlet,
// and the reinforced §9.3 φ pair this row's own clause. EN 1996-1-1 §6.1/§6.2/§6.3/§6.6: flexure prices on f_xd
// ALONE so bending and tension coincide, and §6.6 strengths arrive already divided so no factor lands twice.
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

    // TMS 402 §9.2: the 0.80 multiplier on the slenderness-reduced axial resistance is the code's own accidental-
    // eccentricity coefficient, NOT the stress-block cap that shares its value — two clauses reading one number, so
    // folding them into one column would make an edit to either silently move the other.
    const double SlendernessCoefficient = 0.80;
}

// --- [POLICIES] ----------------------------------------------------------------------------
// The JURISDICTION axis every SectionCapacity case binds in place of a hardcoded code. γM values are the codes' own
// recommended sets (EN 1993 §6.1, EN 1992 §2.4.2.4, EN 1995 Table 2.3, EN 1994 §2.4.1.2, EN 1996 Table 2.3 at the
// class-3 category-I value); a φ-format row carries unity γM and a partial-factor row unity φ, so one arm reads the same columns on either basis through Resist.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignBasis {
    public static readonly DesignBasis Aisc360      = new("aisc360",    ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis AisiS100     = new("aisi-s100",  ComponentAuthority.Aisi, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1992       = new("en1992",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.50, 1.50, Ec2,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1993       = new("en1993",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3,          Linear, gammaM2: 1.25);
    // EN 1993-1-4 STAINLESS declares its OWN set — γM0 = γM1 = 1.10, unlike the carbon-steel unity pair — so a
    // stainless receipt lands the SAME SteelMember case under this row: the reduced ε and the 200 GPa design modulus
    // stay the steel owner's jurisdiction columns, the γ set and the interaction algebra this row's.
    public static readonly DesignBasis En1993Stainless = new("en1993-1-4", ComponentAuthority.En, SafetyFormat.LimitState, 1.10, 1.10, Ec3Stainless, Linear, gammaM2: 1.25);
    // EN 1993-1-9 FATIGUE: γMf is NOT a γM column — it keys on the (assessment method, consequence) pair a project
    // declares, so it rides the FatigueAssessment row inside the law and this row's γ pair stays unity; γFf = 1.0.
    public static readonly DesignBasis En1993Fatigue = new("en1993-1-9", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Fatigue,   Linear);
    // AISC 360 Appendix 3: the allowable-stress-range form carries the whole margin in the category constants.
    public static readonly DesignBasis AiscFatigue  = new("aisc-app3",  ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1994       = new("en1994",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec4,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1995       = new("en1995",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.25, 1.25, Ec5,          Timber);
    public static readonly DesignBasis En1996       = new("en1996",     ComponentAuthority.En,   SafetyFormat.LimitState, 2.00, 2.00, Ec6,          Linear, gammaS: 1.15, masonry: MasonryAlgebra.Factored);
    // EN 1999-1-1 declares NO γM0 — γM1 = 1.10 covers cross-section resistance and member instability alike, so BOTH
    // slots carry 1.10 and the invariant is mirrored at aluminum#ALUMINUM_FAMILY, stating at both owners and moving
    // as one; γM2 = 1.25 rides the fracture rail.
    public static readonly DesignBasis En1999       = new("en1999",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.10, 1.10, Ec9,          Linear, gammaM2: 1.25);
    // EN 1992-4 ANCHORAGE divides the concrete failure modes by the γc family the En1992 row already cites;
    // VividOrange.Standards ships NO En1992 Part 4, so the citation answers None honestly. Cast-in only today — a
    // post-installed product's ETA-set installation factor has no proven cell.
    public static readonly DesignBasis En1992Anchors = new("en1992-4", ComponentAuthority.En,    SafetyFormat.LimitState, 1.50, 1.50, NoCitation,   Linear, gammaS: 1.15);
    // TMS 402 strength design is a φ-format jurisdiction by this axis's own definition (strength design multiplies
    // by φ — the Aci318 twin), so the format row reads lrfd and the γM pair stays unity; limit-state is the
    // partial-factor family alone. Compute's DesignCode roster spells this cell limit-state; THIS cell is argued and the peer half re-proves against it.
    public static readonly DesignBasis Tms402       = new("tms402",     ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear,
        phiFlexure: 0.60, phiShear: 0.80, stressBlock: 0.80, shearCeilingMpa: Some(2.07), masonry: MasonryAlgebra.Strength);
    public static readonly DesignBasis En16612      = new("en16612",    ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, NoCitation,   Linear);   // a European Norm outside the Eurocode set — no VividOrange body
    // The joint row divides JOINT resistance by γM2 and leaves cross-section and stability at the recommended unity.
    public static readonly DesignBasis En1993Joints = new("en1993-1-8", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Joints,    Linear, gammaM2: 1.25);
    public static readonly DesignBasis AwsD11       = new("aws-d1-1",   ComponentAuthority.Aws,  SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis AstmD1002    = new("astm-d1002", ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis IccEs        = new("icc-es",     ComponentAuthority.IccEs, SafetyFormat.Asd,       1.00, 1.00, NoCitation,   Linear);   // the evaluation report itself is the issuing body
    // SDPWS publishes nominal unit shears reduced by the §4.1.4 factor pair, so the row states the tables' NATIVE
    // reading and the reduction reads the PROJECT's format off the placement's declared basis.
    public static readonly DesignBasis Sdpws        = new("sdpws",      ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Linear);
    // STANDS as the [WIRE] counterpart of the consumer's `nds` key — deleting it strands that key against the
    // two-roster law — while no Materials-side producer mints under it, the timber family being EN-bodied.
    public static readonly DesignBasis Nds          = new("nds",        ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Timber);
    // ACI 318 §21.2 φ = 0.90 flexure, φv = 0.75 shear, §22.2.2.4.1 stress block 0.85. STANDS as the `aci318` [WIRE]
    // counterpart while its producer waits on a US-bodied RC arm — every non-EN concrete factory arm throws.
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
    // Present only where a code CAPS the resolved shear — absence is the honest state for every jurisdiction that
    // publishes no cap, and a sentinel ceiling would clamp arms nothing clamps.
    public Option<double> ShearCeilingMpa { get; }
    // Present on the two masonry jurisdictions alone; a masonry receipt under any other basis is unpriceable and the
    // arm rails rather than borrowing an algebra the body never published.
    public Option<MasonryAlgebra> Masonry { get; }

    // The ONE nominal-to-design fold: the format decides the operation and the action decides the factor, so no arm
    // spells "×φ or ÷γM" and a jurisdiction added to either axis lands one row.
    public double Resist(ResistanceAction action, double nominal) => Format.Resist(nominal, action.Factors(this));

    // The citation is a FUNCTION of the placement annex, never a frozen field: an EN row cites its own part under the
    // project's annex, and a body shipping no VividOrange type answers None rather than a fabricated identity.
    [UseDelegateFromConstructor]
    public partial Option<IStandard> Standard(NationalAnnex annex);

    // The per-basis COMBINED-ACTION kernel — the one place a jurisdiction's interaction algebra lives, so a steel arm
    // folding AISC §H1.1 and the same arm folding EN 1993-1-1 §6.3.3 are ONE code path over two rows.
    [UseDelegateFromConstructor]
    public partial double Interact(InteractionOperands operands);

    // Part 1-1 the general member rules, 1-4 the stainless supplement, 1-8 the joint rules, 1-9 fatigue — all four
    // decompile-verified En1993Part members (.api/api-vividorange-standards.md).
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

    // AISC 360 §H1.1 / AISI S100 §C5: the two-branch form a max-of-independents under-predicts (p = m = 0.9 passes a
    // max fold yet fails H1.1 at 1.7); biaxial bending is the PER-AXIS two-term sum, never a resultant against the major resistance alone.
    static double Aisc(InteractionOperands o) =>
        o.Axial >= 0.2 ? o.Axial + 8.0 / 9.0 * (o.Major + o.Minor) : o.Axial / 2.0 + o.Major + o.Minor;

    // EN 1993-1-1 §6.3.3 eq 6.61/6.62, EN 1994-1-1 §6.7.3.6, EN 1992 §6.1, EN 1996 §6.1, TMS 402 §9.2/§9.3, EN 16612:
    // the LINEAR unity sum, the kyy/kzz interaction factors an Annex-A/B evaluation refines riding at the 1.0
    // conservative bound this estate states rather than a per-annex table it has not transcribed.
    static double Linear(InteractionOperands o) => o.Axial + o.Major + o.Minor;

    // EN 1995-1-1 §6.3.2 eq 6.23/6.24 and §6.2.4 eq 6.19/6.20, MAX-swapped on the k_m weight so the redistribution credit lands on one bending axis at a time.
    static double Timber(InteractionOperands o) =>
        o.Regime.Term(o.Axial) + Math.Max(o.Major + o.MinorWeight * o.Minor, o.MinorWeight * o.Major + o.Minor);

    // --- [PARITY_CENSUS]
    // The cross-roster parity claim DERIVED, never asserted: the strata forbid a reference either way, so each end
    // proves its own half against the ONE declared shared set with the section-and-load-path carve as the stated
    // complement. A key minted on one side alone fails this type initializer — the loud structural refusal a prose
    // parity claim never gives (Materials RULINGS [02]).
    static readonly FrozenSet<string> SectionCarve = FrozenSet.ToFrozenSet(
        ["en16612", "en1993-1-8", "aws-d1-1", "astm-d1002", "icc-es", "en1992-4", "en1993-1-9", "aisc-app3"],
        StringComparer.Ordinal);
    static readonly FrozenSet<string> MemberKeys = FrozenSet.ToFrozenSet(
        ["aisc360", "aisi-s100", "en1992", "en1993", "en1993-1-4", "en1994", "en1995", "en1996", "en1999",
         "tms402", "sdpws", "nds", "aci318"],
        StringComparer.Ordinal);
    // Declared LAST so every row above has initialized when the census reads Items.
    static readonly Unit RosterParity = ProveRoster();

    static Unit ProveRoster() {
        Seq<string> declared = toSeq(Items).Map(static basis => basis.Key);
        Seq<string> member = declared.Filter(static key => !SectionCarve.Contains(key));
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

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The EN 1993-1-9 §3(7) γMf grid as ONE four-row axis over (assessment method × consequence of failure) — the pair a
// PROJECT declares together, so no arm re-pairs the two halves.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FatigueAssessment {
    public static readonly FatigueAssessment DamageTolerantLow  = new("damage-tolerant-low",  gammaMf: 1.00);
    public static readonly FatigueAssessment DamageTolerantHigh = new("damage-tolerant-high", gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeLow        = new("safe-life-low",        gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeHigh       = new("safe-life-high",       gammaMf: 1.35);
    public double GammaMf { get; }
}

// The standard's CLOSED fourteen-rung direct-stress ladder, ΔσC the printed category number at 2×10⁶ cycles and the
// ONE stored column. ΔσD/ΔσL are the standard's own §7.1 generators — DERIVED, because the independently tabulated
// integer columns reproduce cell-for-cell off these two lines and a stored copy could only agree or drift.
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
    public double CaflMpa => Math.Pow(2.0 / 5.0, 1.0 / 3.0) * RefMpa;       // ΔσD, the constant-amplitude limit at 5×10⁶
    public double CutoffMpa => Math.Pow(5.0 / 100.0, 1.0 / 5.0) * CaflMpa;  // ΔσL, the cut-off at 10⁸
}

// AISC 360 Appendix 3 Table A-3.1 — the TWO-SOURCED categories A–E′ alone under the uniform 0.333 exponent: Cf the
// ksi-form constant (the SI evaluation carries the standard's own ×329 factor) and FTH the printed MPa threshold.
// Categories F (its own 0.167-exponent equation) and G are single-sourced and typed-absent; F′ does not exist.
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

    // §7.1: m = 3 to the 5×10⁶ knee, m = 5 to the 10⁸ cut-off, ΔσL the constant-amplitude floor beyond — a range
    // below ΔσL contributes no damage, so the floor is the honest terminal resistance, never a zero.
    static double EnRange(EnFatigueCategory category, double cycles) =>
        cycles <= 5e6 ? category.RefMpa * Math.Pow(2e6 / cycles, 1.0 / 3.0)
        : cycles <= 1e8 ? category.CaflMpa * Math.Pow(5e6 / cycles, 1.0 / 5.0)
        : category.CutoffMpa;
}
```

## [04]-[DEMAND_VERDICT]

- Owner: `CapacityBuild` is the DECLARED-INPUT build request and `CapacityReceipt` the SIBLING-RECEIPT lift request — two discriminants because the two carry different KINDS of input, never two spellings of one modality: a build arm holds the declaration a solver consumes (an RC section, an S-N detail, an anchor bed, a plate bed) and a receipt arm holds a design value a family owner already computed. `Demand` admits the signed action vector over the `DemandColumn` roster; `CapacityPlacement` is the ONE currency threading `component#COMPONENT_OWNER` `ComponentFamily.Capacity`, so a new placement input is one column here rather than a per-family parameter tail; `Utilisation` distinguishes a bounded verdict, a section pass owing a named member check, and an UNBOUNDED verdict; `MemberCheckRequirement` closes the section-undecidable deferral vocabulary; `MasonryReduction` OWNS the stability bracket as a derivation over (height, radius of gyration); `LateralRule` owns the connector report's own two-direction interaction convention.
- Cases: `CapacityBuild.{Hull · Elastic · Detail · Anchorage · Bearing}`, each arm carrying EXACTLY the inputs its solver consumes — the prior loose parameter pair forced a half-dead knob onto every elastic call, and the same law now keeps an anchor bed off a fatigue request. `CapacityReceipt.{Steel · Timber · DeckSheet · Masonry · ReinforcedMasonry · Glass · SteelFire · TimberFire · Weld · Adhesive · Stud · Connector · LateralPanel · Bolt · SlipCritical · TimberDowel · Aluminum}`, each carrying its full lift context so the modality is recoverable from the request value alone.
- Entry: `Demand.Admit(…)` is the ACCUMULATING boundary naming every offending column in one verdict and `Demand.Of` the same proof collapsed onto `Fin`; `CapacityBuild.Declared(subject, placement)` turns the placement's declarations into the build requests they name and `CapacityReceipt.Fire(subject, state)` the family's fire state into its receipt, so every declared modality is reachable from a `Component` and its placement; `CapacityReceipt.Kind`/`CapacityBuild.Kind` own the case-name projection every signal dimension and analytics column keys on, so a reflected runtime type name at a consumer has no reason to exist.
- Growth: a new demand axis is one `DemandColumn` row with its `Demand` column — the token, the admitted band, and the guard land together; a new declared modality is one `CapacityBuild` arm with one `CapacityPlacement` column; a new family receipt is one `CapacityReceipt` case, never another overload; a new fire-rated family is one `FireState` case and one `Lift` arm.
- Boundary: `Demand` MODALITY columns bind their OWN case — unit shear to `LateralPanel`, the range/count pair to `Fatigue` — so a member arm neither resists nor reads them and the check that consumes them is its own invocation. The identity rides the receipt and build BASE where a new case cannot forget it: the analytics per-check dataset and the `MaterialsFact` stream both key on (op, kind, governing), which collides for two members of one kind under one op.
- Boundary: `FireState` is the fire modality's typed input contract and `CapacityReceipt.Fire` its ONE mint, so both fire cases are constructed rather than assembled at a call site. BOTH producers are LANDED at their owners: `steel#STEEL_FAMILY` `SteelSeed.Capacity` reads `CapacityPlacement.FireExposure` through the `SteelFire` §4.2.5.1 step and `SteelDesign.Fire` into `FireState.Steel`, and `timber#TIMBER_CAPACITY` `TimberSeed.Capacity` routes its `TimberDesign.Fire` reduced-section receipt into `FireState.Timber` — every fire receipt in the folder constructs through this mint and a `new CapacityReceipt.SteelFire`/`TimberFire` beside it is the deleted form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The CAST-IN anchor's concrete-side facts — the placement declaration the fastener row cannot carry. NO basis
// column: en1992-4 is the one realized anchorage jurisdiction (the two-source-proven ACI arm lands with its anchor φ
// roster, and the bed regains its Abrg column then), so the build pins the row by construction.
public readonly record struct AnchorBed(
    double FckMpa,
    PositiveMagnitude HefMm,
    Option<double> EdgeMm,
    bool Cracked);

// The DG1 base-plate declaration: plate B × N × t and its yield, the wide-flange column footprint the m/n cantilever
// pair reads (the HSS/pipe variants are single-sourced and typed-absent), the bearing concrete, and the caller's
// √(A2/A1) confinement ratio the arm clamps at the J8 ceiling of 2.
public readonly record struct PlateBed(
    PositiveMagnitude WidthMm,
    PositiveMagnitude LengthMm,
    PositiveMagnitude ThicknessMm,
    double FyMpa,
    PositiveMagnitude ColumnDepthMm,
    PositiveMagnitude ColumnFlangeMm,
    double FcMpa,
    double ConfinementRatio);

// The DECLARED-input build request the ONE Resolve dispatches. Each arm carries EXACTLY the knobs its solver
// consumes, so no call site passes a knob its modality never reads.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityBuild {
    private CapacityBuild(ComponentId subject) => Subject = subject;
    public ComponentId Subject { get; }
    public sealed record Hull(ComponentId Subject, RcSection Section, DiagramResolution Resolution) : CapacityBuild(Subject);
    public sealed record Elastic(ComponentId Subject, RcSection Section) : CapacityBuild(Subject);
    // The detail-category declaration: the LAW carries the ladder rung and (EN) its γMf assessment row, exactly as a
    // weld states its load angle and a stud its group.
    public sealed record Detail(ComponentId Subject, FatigueLaw Law) : CapacityBuild(Subject);
    public sealed record Anchorage(ComponentId Subject, FastenerAssembly Assembly, ShearPlane Plane, AnchorBed Bed) : CapacityBuild(Subject);
    public sealed record Bearing(ComponentId Subject, PlateBed Plate) : CapacityBuild(Subject);

    public string Kind => Switch(
        hull: static _ => nameof(Hull),
        elastic: static _ => nameof(Elastic),
        detail: static _ => nameof(Detail),
        anchorage: static _ => nameof(Anchorage),
        bearing: static _ => nameof(Bearing));

    // The DECLARED-MODALITY producer: each declaration names a capacity the section itself cannot state, so this
    // reader turns it into its build and every declared case is reachable from a Component and its placement. The
    // modalities are ADDITIONAL surfaces — a member and its fatigue detail are two Check invocations — so the fold
    // answers a Seq and an undeclared placement answers empty rather than a fabricated request.
    public static Seq<CapacityBuild> Declared(ComponentId subject, CapacityPlacement placement) =>
        placement.Detail.Map(law => (CapacityBuild)new Detail(subject, law)).ToSeq()
            + placement.Anchorage.Map(anchor => (CapacityBuild)new Anchorage(subject, anchor.Assembly, anchor.Plane, anchor.Bed)).ToSeq()
            + placement.Bearing.Map(plate => (CapacityBuild)new Bearing(subject, plate)).ToSeq();
}

// Three columns that only price together, so the placement carries ONE Option and a half-declared anchor is unrepresentable.
public readonly record struct AnchorPlacement(FastenerAssembly Assembly, ShearPlane Plane, AnchorBed Bed);

// The ACCIDENTAL fire design situation's INPUT CONTRACT — the typed handoff a family owner fills once its own fire
// route resolves, so the two fire receipts have a declared producer instead of a construction each caller invents.
// Neither arm derives fire physics: the steel arm carries the ambient receipt beside the EN 1993-1-2 Table 3.1
// retention pair its owner computed, the timber arm the EN 1995-1-2 residual receipt already priced at
// kmod = γM = 1.0. The DISCRIMINANT is the family, so a third fire-rated family is one case here and one Lift arm.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FireState {
    private FireState() { }
    public sealed record Steel(DesignCapacity Ambient, SteelFireFacts Retention) : FireState;
    public sealed record Timber(TimberCapacity Residual) : FireState;
}

// The sibling-receipt request the ONE Lift dispatches (FORM_CHOOSER row 1: a receipt family collapses onto a request
// union + total Switch, never an overload roster). SUBJECT rides the BASE where a new case cannot forget it — it is
// the only column separating two members of one family in one report.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityReceipt {
    private CapacityReceipt(ComponentId subject) => Subject = subject;
    public ComponentId Subject { get; }
    public sealed record Steel(ComponentId Subject, DesignCapacity Capacity) : CapacityReceipt(Subject);
    public sealed record Timber(ComponentId Subject, TimberCapacity Capacity) : CapacityReceipt(Subject);
    // Gauge and Rib ride the receipt because the report and the analytics dimension name WHICH deck, never a bare
    // steel row — and the seam datum GaugeRow.AxialSectionCapacityKnPerMm finally has a check behind it.
    public sealed record DeckSheet(ComponentId Subject, GaugeRow Gauge, DeckProfileRow Rib, DesignCapacity Capacity) : CapacityReceipt(Subject);
    // BOTH tension tables ride the receipt because they key on different axes — TMS 402 on span direction × grout
    // form, EN 1996 on unit group — so neither collapses into the other and one direction source serves both.
    public sealed record Masonry(ComponentId Subject, GradeProperties.Cmu Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, RuptureModulus Rupture, FlexuralStrengthEn Flexural, MortarSystem System, MortarType Mortar) : CapacityReceipt(Subject);
    // The reinforced case reads the cmu lattice facts the unreinforced case never consumed, and the bar identity
    // rides the ONE MaterialGrade row — the yield resolves off its Rebar arm, absent where a grade publishes none.
    public sealed record ReinforcedMasonry(ComponentId Subject, GradeProperties.Cmu Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, CmuRow Unit, MaterialGrade Bar) : CapacityReceipt(Subject);
    public sealed record Glass(ComponentId Subject, GlassCapacity Capacity) : CapacityReceipt(Subject);
    // The ACCIDENTAL situation as two lift cases over the SAME law: the EN 1993-1-2 Table 3.1 retention pair beside
    // the ambient receipt, and the charred ResidualStack already priced at kmod = γM = 1.0. Neither arm derives fire
    // physics — the family owner computes, this owner lifts, and Check folds it through the ambient interaction.
    public sealed record SteelFire(ComponentId Subject, DesignCapacity Ambient, double Ky, double Ke, double SteelTemperatureC) : CapacityReceipt(Subject);
    public sealed record TimberFire(ComponentId Subject, TimberCapacity Residual) : CapacityReceipt(Subject);
    public sealed record Weld(ComponentId Subject, JointRow.Weld Row, double LoadAngleDeg) : CapacityReceipt(Subject);
    public sealed record Adhesive(ComponentId Subject, JointRow.Adhesive Row) : CapacityReceipt(Subject);
    // The stud group is a PLACEMENT fact and AISC Eq I8-1 reads it directly, so it rides the receipt rather than the
    // row: one stud class welded into three deck conditions is three capacities.
    public sealed record Stud(ComponentId Subject, JointRow.Stud Row, StudGroup Group, int Count) : CapacityReceipt(Subject);
    public sealed record Connector(ComponentId Subject, ConnectorCapacity Capacity) : CapacityReceipt(Subject);
    public sealed record LateralPanel(ComponentId Subject, double DesignKnPerM, LateralHazard Hazard) : CapacityReceipt(Subject);
    // The BEARING-type bolted connection. BoltCategory is NOT a case column — the assembly already holds it, and a
    // second spelling is the redundant parallel lift parameter this owner bans.
    public sealed record Bolt(ComponentId Subject, FastenerAssembly Assembly, BearingDesign Bearing, ShearPlane Plane) : CapacityReceipt(Subject);
    // The SLIP-CRITICAL (EN 1993-1-8 category B/C/E) state of the SAME assembly: the shear column is the §3.9 slip
    // resistance rather than the shank shear, and a non-preloaded assembly answers None.
    public sealed record SlipCritical(ComponentId Subject, FastenerAssembly Assembly, FastenerInstallation Install) : CapacityReceipt(Subject);
    // EC5 §8 dowel-type: Fastening.TimberDowelShearKn is the family owner's railed six-mode Johansen minimum, so its
    // ALREADY-COMPUTED per-shear-plane design value arrives as a column and Lift stays total.
    public sealed record TimberDowel(ComponentId Subject, double PerPlaneShearKn, int Planes) : CapacityReceipt(Subject);
    // The banded (fo, fu) pair the family registry proved at seed time under en1999, the one jurisdiction with
    // landed aluminium bands, which AluminumSeed.Capacity refuses to leave.
    public sealed record Aluminum(ComponentId Subject, MaterialGrade Grade, ExtrusionForm Form, double FoMpa, double FuMpa, ComputedSection Section, DesignBasis Basis) : CapacityReceipt(Subject);

    // The FIRE producer: the ONE site a fire state becomes a request, so the retention pair reaches the SteelFire
    // case and the residual receipt the TimberFire case by construction rather than by a caller assembling columns.
    // The critical temperature rides the receipt for the design report — the verdict prices strength through ky and
    // stability through kE, and a report that cannot name the temperature cannot be checked against its fire rating.
    public static CapacityReceipt Fire(ComponentId subject, FireState state) => state.Switch(
        steel: s => (CapacityReceipt)new SteelFire(subject, s.Ambient, s.Retention.Ky, s.Retention.KE, s.Retention.CriticalTemperatureC),
        timber: t => new TimberFire(subject, t.Residual));

    // Case identity IS the kind dimension every downstream reader keys on — signal roster tag and analytics column
    // alike — so this total projection holds the one spelling and a further case breaks it at compile time.
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

// One canonical term per action: flexure owns every bending-governed verdict (a `bending` synonym is the deleted
// form) and COMBINED every unity ratio that is definitionally an axial-plus-flexure INTERACTION, so a report never
// reads `Axial` on a 1.7 ratio neither component attains. InPlaneShear serves a shear wall and a diaphragm alike and
// Fatigue both ladders — each pair differs in the table that publishes it, never in the action.
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

// The band a demand column admits. A SIGNED action never licenses NaN/∞; the two fatigue columns are magnitudes and
// a negative range or cycle count is not a direction, it is a malformed vector.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DemandBand {
    public static readonly DemandBand Signed      = new("signed",       admits: static value => double.IsFinite(value));
    public static readonly DemandBand NonNegative = new("non-negative", admits: static value => double.IsFinite(value) && value >= 0.0);
    [UseDelegateFromConstructor] public partial bool Admits(double value);
}

// The refusal token and admitted band per column, in the DECLARATION order the factory hands its arguments in. A
// new action column lands ONE row rather than a conjunct plus an interpolation slot, and the refusal names EVERY
// offender where the prior && chain reported the whole vector on any single defect.
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

    // Zipped against the roster by ordinal: declaration order IS the parameter order, stated here so the pairing is
    // a read of this roster rather than an assumption a later row could break silently.
    public static Seq<string> Refusals(params ReadOnlySpan<double> values) =>
        toSeq(Items).Zip(Iterable<double>.FromSpan(values).ToSeq())
            .Filter(static pair => !pair.Item1.Band.Admits(pair.Item2))
            .Map(static pair => $"{pair.Item1.Key}={pair.Item2:R}");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The applied action vector in SI engineering units (kN, kNm), SIGNED and ADMITTED ONCE so no per-case arm re-checks
// a column. q is a per-LENGTH action and the fatigue pair per-cycle, so each is its own column: one column serving a
// diaphragm's shear per metre and a column's shear would compare unlike quantities. The moment magnitude and the
// shear resultant are DERIVED projections, never re-passed columns.
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

    // The generated CONSTRUCTION floor and the accumulating rail below read ONE roster, so an unadmitted Create is
    // unrepresentable and the two entry points can never disagree about a band.
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double axialKn, ref double momentYKnm, ref double momentZKnm,
        ref double shearYKn, ref double shearZKn, ref double torsionKnm, ref double bearingKn,
        ref double unitShearKnPerM, ref double stressRangeMpa, ref double cycleCount) {
        Seq<string> offending = DemandColumn.Refusals(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn,
            torsionKnm, bearingKn, unitShearKnPerM, stressRangeMpa, cycleCount);
        validationError = offending.IsEmpty ? null : new ValidationError($"Demand columns must be finite: {string.Join(':', offending)}.");
    }

    // Every column proves INDEPENDENTLY and the verdict carries one fault per offender, so a three-bad-column vector reports three tokens in one refusal.
    public static Validation<Error, Demand> Admit(double axialKn, double momentYKnm, double momentZKnm, Op key,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0,
        double unitShearKnPerM = 0.0, double stressRangeMpa = 0.0, double cycleCount = 0.0) =>
        DemandColumn.Refusals(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn,
                unitShearKnPerM, stressRangeMpa, cycleCount)
            .Map(token => (Error)new KernelFault.InvalidValue(token, "a finite demand scalar", Some(key)))
            .Match(
                Empty: () => Validation<Error, Demand>.Success(Create(axialKn, momentYKnm, momentZKnm, shearYKn,
                    shearZKn, torsionKnm, bearingKn, unitShearKnPerM, stressRangeMpa, cycleCount)),
                More: faults => Validation<Error, Demand>.Fail(faults.Reduce(static (all, next) => all + next)));

    public static Fin<Demand> Of(double axialKn, double momentYKnm, double momentZKnm, Op key,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0,
        double unitShearKnPerM = 0.0, double stressRangeMpa = 0.0, double cycleCount = 0.0) =>
        Admit(axialKn, momentYKnm, momentZKnm, key, shearYKn, shearZKn, torsionKnm, bearingKn,
            unitShearKnPerM, stressRangeMpa, cycleCount).ToFin();

    public double MomentResultantKnm => Math.Sqrt(MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double ShearResultantKn => Math.Sqrt(ShearYKn * ShearYKn + ShearZKn * ShearZKn);
}

// The PLACEMENT facts a capacity needs and a catalogue row cannot carry. ONE currency threads
// component#COMPONENT_OWNER ComponentFamily.Capacity, so a new placement input is one column here rather than a
// per-family parameter tail, and basis and annex are selected TOGETHER so no arm reads a second annex. The four
// Option columns are DECLARATIONS whose absence is the honest state, never a zero a producer must interpret.
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
    // The ACCIDENTAL fire design situation's exposure, absent for the ambient state — the prior 0-minutes sentinel
    // spelled an absence no producer could tell from a declared zero. The placement carries no exposed-face count:
    // the timber fire producer takes the fully-exposed conservative bound until an occurrence fact lands the
    // ExposedFaces column here WITH its charring-fold reader in the same change.
    Option<PositiveMagnitude> FireExposure,
    // The three DECLARED modalities: an S-N detail category, a cast-in anchorage, and a base-plate bed. Each names a
    // capacity surface beside the family's own, so CapacityBuild.Declared reads them into their build requests.
    Option<FatigueLaw> Detail,
    Option<AnchorPlacement> Anchorage,
    Option<PlateBed> Bearing,
    RuptureModulus Rupture,
    FlexuralStrengthEn Flexural,
    MortarSystem System,
    MortarType Mortar,
    MaterialGrade BarGrade);

// The member-stability reduction as a DERIVED value object, PER BASIS: the formula IS the owner, so a transposed
// branch is unrepresentable. The admitted height and the always-positive governing radius make BOTH derivations
// TOTAL over h/r ∈ (0, ∞) with range (0, 1], so the throwing Create is the sanctioned re-admission of a value the
// algebra already proves; every producer is the Lift arm holding the section AND its basis.
[ValueObject<double>]
public readonly partial struct MasonryReduction {
    const double SlendernessBreak = 99.0;   // TMS 402: h/r ≤ 99 takes the parabolic bracket, above it the Euler-form ratio
    // EN 1996-1-1 §5.5.1.1 e_init = h_ef/450 folded into §6.1.2.2(1) Φ = 1 − 2·e/t: for the solid rectangle whose
    // r = t/√12 the EN reduction reads the SAME two inputs the TMS bracket does and needs no second placement column.
    // A slenderness driving Φ to zero is the §5.5.1.4 h/t = 27 ceiling, expressed as a floor rather than a throw.
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

// A connector report publishes its two lateral directions either as an INTERACTING pair — a resultant must fit
// inside the combined envelope, so the ratios SUM — or as INDEPENDENT checks each verified on its own axis, where
// the WORST governs. The report states which, so the rule is a row: one fold guessing a single envelope for both
// conventions either over-rates an interacting connector or refuses an independent one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralRule {
    public static readonly LateralRule Interacting = new("interacting", fold: static (primary, secondary) => primary + secondary);
    public static readonly LateralRule Independent = new("independent", fold: Math.Max);
    [UseDelegateFromConstructor] public partial double Fold(double primary, double secondary);
}

// The SECOND lateral direction and the rule it is published under as ONE presence — a direction without its
// interaction rule is a pair no fold can price, and a rule without a direction governs nothing.
public readonly record struct LateralPair(double SecondKn, LateralRule Rule);

[Union]
public abstract partial record Utilisation {
    private Utilisation(GoverningAction governing) => Governing = governing;
    public GoverningAction Governing { get; }
    // The strict ACCEPTANCE bit: only a bounded ratio at or under unity is a finished verdict.
    public bool Adequate => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static _ => false,
        unbounded: static _ => false);
    // The SECTION-altitude pass: a deferring verdict decided everything the section can and owes only the named
    // member check, so a sizing query returns it WITH its deferral rather than rejecting it.
    public bool SectionPasses => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static verdict => verdict.Value <= 1.0,
        unbounded: static _ => false);
    // Every reader takes the verdict's own optional ratio, so no consumer re-enumerates which cases hold a Value.
    public Option<double> Ratio => Switch(
        bounded: static verdict => Some(verdict.Value),
        requiresMemberCheck: static verdict => Some(verdict.Value),
        unbounded: static _ => Option<double>.None);

    public sealed record Bounded(double Value, GoverningAction Action) : Utilisation(Action);
    public sealed record RequiresMemberCheck(double Value, GoverningAction Action, MemberCheckRequirement Requirement) : Utilisation(Action);
    // The capacity surface does not BOUND this demand — a ray piercing no hull face, or a demand against a
    // declared-zero column. The fold REACHES it by a candidate publishing no ratio, never by a sentinel magnitude.
    public sealed record Unbounded(GoverningAction Action) : Utilisation(Action);
}

// The section-UNDECIDABLE deferrals: a check whose remaining input is member-level DETAILING the cross-section does
// not carry, so the verdict passes with the named obligation attached instead of failing on a zero column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberCheckRequirement {
    public static readonly MemberCheckRequirement RcShearReinforcement          = new("rc-shear-reinforcement");            // EC2 §6.2.3(3) V_Rd,s needs the stirrup spacing
    public static readonly MemberCheckRequirement SteelWarpingTorsion           = new("steel-warping-torsion");             // AISC §H3.3 open-shape warping torsion is not a single resistance
    public static readonly MemberCheckRequirement CltInPlaneBending             = new("clt-in-plane-bending");              // the form declares no edgewise bending strength
    public static readonly MemberCheckRequirement ReinforcedMasonryShearSpacing = new("reinforced-masonry-shear-spacing");  // TMS 402 §9.3.4.1.2 V_ns needs the bar spacing
    public static readonly MemberCheckRequirement TimberBearingLength           = new("timber-bearing-length");             // EN 1995-1-1 §6.1.5 R_90,Rd needs the support bearing length
    public static readonly MemberCheckRequirement AnchorForwardModes            = new("anchor-forward-modes");              // EN 1992-4 group areas/spacing, the shear edge mode, the ETA-owned pullout, a non-EN rod band
    public static readonly MemberCheckRequirement AluminumMemberBuckling        = new("aluminum-member-buckling");          // EN 1999-1-1 §6.3.1/§6.3.2 χ needs the effective length
}
```

## [05]-[SECTION_CAPACITY]

- Owner: one `SectionCapacity` `[Union]` closes the structural-capacity family across the realized member rails AND the connection load path — the ultimate N-M-M hull, the elastic transformed RC section, the rolled/composite/cold-formed (and, basis-told, stainless) steel receipt, the EC5 timber receipt, the EN 1999 aluminium member, the TMS 402 URM and §9.3 reinforced masonry checks, the EN 16612 glass pane, the weld/adhesive/stud/connector/anchor `Connection` triple, the detail-category `Fatigue` law, and the DG1 `BasePlate` pair — so a member AND its connection are checked through one `Check` fold, never a per-type surface.
- Cases: the non-RC member cases LIFT their family-owner receipts WHOLE (the design-code computation stays the sibling page's, the unified verdict this owner's); the RC, fatigue, anchorage, and base-plate cases are `Resolve` builds over declared inputs, the aluminium case computing at lift because its family owns DATA, not algebra. `RcInteraction` carries its own content key beside the mesh, so the cached hull's identity is the (subject, resolution) pair and NOT a float-wise comparison of a foreign hull.
- Entry: `SectionCapacity.Resolve(CapacityBuild, Op)` dispatches every DECLARED build; the TOTAL `SectionCapacity.Lift(receipt, key)` dispatches every already-computed sibling receipt under the caller's own operation key; `Check(Demand)` returns the closed `Utilisation` verdict; `HullCache.Of` is the ONE content-keyed round trip through `Freeze`/`Thaw`. The masonry receipts carry the member HEIGHT as a kernel-admitted `PositiveMagnitude` beside their section, so `Lift` mints the stability reduction from the section's own governing radius — no caller-supplied stability scalar and no re-derived code bracket exists.
- Growth: a new structural family's capacity is one `[Union]` case binding either a `Resolve` build or a lift factory and one `Check` arm, admitted only when no existing case's column set carries it; a new design code over an already-cased family is one `DesignBasis` row and the owning family page's per-basis resistance arm, NEVER a sibling case; a persisted-capacity need is the one `HullCache` pair over the `ITaxonomySerializable` marker, never a second serializer.
- Boundary: `Resolve` and `Check` are the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CapacityCheck(Key, Receipt, Verdict, Elapsed)` tap SUBJECTS and `Check` the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.InteractionSweep` measured kernel; the tap is a composition-root decorator on the folder rail at `MaterialsPoint.CapacityCheck`, so this owner emits nothing, carries no `Duration`, and references no signal type.
- Boundary: `Resolve` admits the `VividOrange.InteractionDiagram` engine once and reads the `ConcreteSectionProperties` captured by `RcSectionBuilder.Of`. Documented engine exceptions become cause-bearing `CapacitySolve`; missing effective depth or tension steel remain distinct semantic leaves, and unknown throws remain exact.
- Boundary: the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces, the no-pierce case (an eccentric hull not enclosing the origin) yielding the typed `Utilisation.Unbounded` verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity. Force and moment axes are never Euclidean-normalized together.
- Boundary: the frozen hull's store row is REGISTERED at the custodian — `Rasm.Persistence` `Version/retention#RETENTION_CLASSES` `ArtifactKind.CapacityHull` (`RetentionClass.Cache` because the hull rebuilds from the eager fibre-integration sweep, so eviction costs compute and never evidence; `CacheTier.ArtifactBlob` so the L1 lane never locally caches the mesh). The composition root crosses `ArtifactIndexRow.Admit(ArtifactKind.CapacityHull, key, bytes, classification, at, sourceKey)` at the custodian's `Query/cache#ARTIFACT_BLOB_INDEX`, the `(ComponentId, DiagramResolution.Key)` pair riding as the content-key preimage/sourceKey — `HullCache.Of` reads and writes through the store projections that root supplies, so this owner writes no row and the custodian edits nothing further. `Thaw` is fed EXCLUSIVELY what a trusted `Freeze` minted: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so the store carries an opaque content-keyed blob it never decodes, no peer document reaches `Thaw`, and the `$type` shape never crosses to a peer.
- Boundary: the verdict crosses to `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` as portable scalar data keyed by section, never a `VividOrange` assembly type, and `DesignBasis.Key` is that crossing's JURISDICTION column. Checks stand REFUSED at this altitude as standing law, never faked as arms: SLS DEFLECTION needs the span, the load distribution, and the modulus — none a `SectionCapacity` carries; RC PUNCHING SHEAR is a slab-column JUNCTION check over a control perimeter no cross-section carries; and the SEISMIC system coefficients are DEMAND-side scalars the load derivation consumes before a `Demand` ever reaches `Check`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    // The ONE base-constructor column: a verdict names its jurisdiction, so report, analytics, and forward member
    // check read one row rather than a (body, code) pair a case had to spell twice.
    private SectionCapacity(DesignBasis basis) => Basis = basis;
    public DesignBasis Basis { get; }
    public ComponentAuthority Body => Basis.Body;

    // EC2 §6.1 ultimate resistance over the rigid-plastic fibre integral, the hull held once from the eager solve.
    // IDENTITY IS THE CONTENT KEY: the mesh is a FOREIGN interface whose only structural equality would be a
    // fabricated float-wise vertex compare, so the cache pair IS what the generated comparer reads.
    [Equatable(Explicit = true)]
    public sealed partial record RcInteraction(
        [property: DefaultEquality] ComponentId Subject,
        [property: DefaultEquality] DiagramResolution Resolution,
        [property: IgnoreEquality] IForceMomentMesh Hull) : SectionCapacity(DesignBasis.En1992);

    // Read off the ONE ConcreteSectionProperties carrier the RcSection receipt holds. EffectiveDepth is the ULS
    // lever to the tension STEEL, distinct from the SLS extreme-fibre distance; Asw > 0 is the §6.2.2-vs-§6.2.3(3)
    // discriminant. TWO inertia pairs, two limit states: GrossInertia the EC2 7.1 SLS fibre DIVISOR,
    // ReinforcementInertia the Σ(As·d²) cracked-Icr readout a fibre stress is NEVER divided by.
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

        // The EC2 §6.2.3 web-crushing ceiling V_Rd,max at the policy cotθ = 2.5 — DERIVED off the record's own
        // columns, the guarded floors matching the shear screen's own admission.
        public double VrdMaxKn =>
            Math.Max(WidthMm, 1.0) * 0.9 * Math.Max(EffectiveDepthMm, 1.0) * 0.6
                * (1.0 - FckMpa / 250.0) * (FckMpa / 1.5) / (2.5 + 0.4) * 1e-3;

        // The seam publication the Compute V_Rd,s truss arm reads. SI conversion happens at THIS one site and the
        // triple publishes ONLY whole, so the reader's all-three-present gate is the producer's own absence.
        public Fin<Seq<(PropertyName Row, PropertyValue Value)>> ShearLinkRows(Op key) =>
            ShearLinkAreaMm2 > 0.0
                ? FywdMpa.Match(
                    Some: fywd =>
                        from area in MeasureValue.Of(ShearLinkAreaMm2 * 1e-6, UnitsNet.Units.AreaUnit.SquareMeter, key)
                        from fywdPa in MeasureValue.Of(fywd * 1e6, UnitsNet.Units.PressureUnit.Pascal, key)
                        from ceiling in MeasureValue.Of(VrdMaxKn * 1e3, UnitsNet.Units.ForceUnit.Newton, key)
                        select Seq(
                            (StructuralRows.ShearLinkArea, (PropertyValue)new PropertyValue.Measure(area)),
                            (StructuralRows.ShearLinkYield, (PropertyValue)new PropertyValue.Measure(fywdPa)),
                            (StructuralRows.ShearLinkCeiling, (PropertyValue)new PropertyValue.Measure(ceiling))),
                    None: () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))
                : Fin.Succ(Seq<(PropertyName, PropertyValue)>());
    }

    // Lifted WHOLE and BASIS-TAGGED: φMn/φMny/φPn/φVn under aisc360, Mb,Rd/Mz,Rd/Nb,Rd/Vpl,Rd under en1993, one
    // shape the Check fold reads through the basis's kernel. TorsionalKnm is 0 for an OPEN shape whose §H3.3 warping
    // torsion is not a single resistance; Chi/ChiLt publish 1.0 on a φ-format receipt, AISC folding buckling INTO
    // Fcr and Mn; StiffnessRetention is the kE,θ the fire lift carries, never an input to the strength interaction.
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
        double StiffnessRetention) : SectionCapacity(Basis);   // aisc360 · aisi-s100 · en1993 · en1994, the fire state riding either EN row

    // Lifted WHOLE. BendingMinorKnm is 0.0 only on a panel form declaring no edgewise strength, so an in-plane Mz
    // demand governs loud; Km is the §6.1.6(2) weight the biaxial fold swaps and TorsionalKnm the §6.1.8 T_Rd.
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

    // The ONE family whose design algebra lives HERE: no aluminium producer exists among admitted packages and the
    // family owns only DATA. The lift computes the class-3 elastic floor, so a class-1/2 plastic credit stays
    // unclaimed rather than unproven; Curve is the §6.3.1.2 class owning its OWN Table 6.6 pair, and no torsional
    // modulus crosses the die receipt so a torsion demand folds against 0.
    public sealed record AluminumMember(
        DesignBasis Basis,
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        Option<BucklingClass> Curve,
        double FoMpa,
        double FuMpa) : SectionCapacity(Basis) {

        // The seam publication the Compute en1999 axial-compression cell reduces by (the ShearLinkRows precedent):
        // the pair publishes WHOLE or not at all, matching the reader's both-present gate.
        public Seq<(PropertyName Row, PropertyValue Value)> BucklingRows() =>
            Curve.Map(static curve => Seq(
                    (StructuralRows.BucklingAlpha, (PropertyValue)new PropertyValue.Number(curve.Alpha)),
                    (StructuralRows.BucklingPlateau, (PropertyValue)new PropertyValue.Number(curve.Plateau))))
                .IfNone(Seq<(PropertyName, PropertyValue)>());
    }

    // The basis and the wall facts its algebra reads: f'm (the EN characteristic f_k under that basis, one column
    // the basis names the symbol of), the grouted net section with BOTH moduli so a biaxially-bent pier folds each
    // moment against ITS modulus, the slenderness reduction, and two PRE-FACTOR limits the arm reduces.
    public sealed record MasonryUnreinforced(DesignBasis Basis, MasonrySection Wall) : SectionCapacity(Basis);   // TMS 402 §9.1/§9.2 · EN 1996-1-1 §6.1/§6.2/§6.3

    // The REINFORCED case over the cmu lattice facts: the steel-couple flexural arm plus the reinforced axial the
    // unreinforced case's no-steel-term admission law reserved for exactly this case.
    public sealed record MasonryReinforced(DesignBasis Basis, MasonryCouple Couple) : SectionCapacity(Basis);   // TMS 402 §9.3 · EN 1996-1-1 §6.6

    // Lifted WHOLE. LoadShareFraction is the insulating-unit share the governing pane draws — a Demand states the
    // pressure on the UNIT and the cavity partitions it by stiffness — carried rather than pre-multiplied so a
    // design report reads both numbers.
    public sealed record GlassPane(
        double BendingKnmPerM,
        double ResistanceMpa,
        double EffectiveThicknessMm,
        double LoadShareFraction) : SectionCapacity(DesignBasis.En16612);

    // ONE case for the weld, adhesive, stud-group, connector, bolt, and anchor receipts. Each resistance is OPTIONAL:
    // absence means the producer publishes no band, while Some(0) is a published zero — a distinction no lift flattens.
    // The basis is PER LIFT ARM: six publishing bodies over one capacity SHAPE, so no arm loses its citation.
    public sealed record Connection(
        DesignBasis Basis,
        Option<double> ShearKn,
        Option<double> TensionKn,
        Option<double> BearingKn,
        Option<LateralPair> Lateral = default,
        Option<MemberCheckRequirement> Defer = default) : SectionCapacity(Basis);   // aws-d1-1 · astm-d1002 · aisc360 · icc-es · en1993-1-8 · en1995 · en1992-4

    // The column is the FINISHED design unit shear — the §4.1.4 reduction ran once at the family producer where the
    // rail and the placement's hazard both exist — so this arm applies nothing further.
    public sealed record LateralPanel(
        DesignBasis Basis,
        double DesignKnPerM,
        LateralHazard Hazard) : SectionCapacity(Basis);   // sdpws

    // The law IS the capacity, so the case carries it whole and its basis derives from the ladder. Every static
    // column is unresisted and governs loud: a static action against a cycle surface is a modelling error.
    public sealed record Fatigue(FatigueLaw Law) : SectionCapacity(Law.Basis);   // en1993-1-9 · aisc-app3

    // TWO precomputed axial capacities, both demand-linear in the download, so the verdict rides the standard fold.
    // A moment-transferring or uplift base rides its anchor receipts and those columns govern loud here.
    public sealed record BasePlate(
        double BearingKn,
        double PlateBendingKn) : SectionCapacity(DesignBasis.Aisc360);   // AISC DG1 / §J8, wide-flange cantilever method

    // --- [OPERATIONS]
    // ONE polymorphic Check over the closed family — never per-type and never per-code. Each arm divides demand by
    // ITS OWN columns and hands the normalized triple to the case's basis kernel, so the jurisdiction's algebra lives
    // on the row while the resistance reading stays with the family that owns it. Every arm is TOTAL over the
    // member-action columns: an unresisted action folds against 0 and governs loud (the consumed-action discipline).
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

    // One RC elastic arm, two limit states through the same Worst fold — never a second RC surface for shear. The
    // §6.2.3(3) stirrup obligation rides only the two candidates that spacing finishes; torsion and bearing are
    // UNRESISTED here, not deferred, no detailing completing a check the section publishes no resistance for.
    // EXPRESSION_SPINE exemption: the intermediate candidate bindings feed one closed Worst fold.
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

    // EC2 SLS cracking: the FULL σ = N/A ± My·cy/Iyy ± Mz·cz/Izz against fctm, never the major-axis-only slice. The
    // axial term is SIGNED so compression DELAYS cracking; the levers are the GROSS half-depths, not the effective
    // depth to the STEEL; the divisor is the GROSS inertia, because the Σ(As·d²) column inflates the fibre stress
    // ~20× and falsely cracks every service state. FibreRatio, not GuardedRatio: a compressive state must lose.
    static (Option<double> Ratio, GoverningAction Governing) Cracking(RcElastic e, Demand demand) {
        double axialStress = demand.AxialKn * 1e3 / Math.Max(e.ConcreteAreaMm2, double.Epsilon);
        double bendingYStress = Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.GrossInertiaYyMm4, double.Epsilon);
        double bendingZStress = Math.Abs(demand.MomentZKnm) * 1e6 * (e.WidthMm * 0.5) / Math.Max(e.GrossInertiaZzMm4, double.Epsilon);
        GoverningAction governing = Math.Max(bendingYStress, bendingZStress) >= Math.Abs(axialStress)
            ? GoverningAction.Flexure : GoverningAction.Axial;   // biaxial-moment names only the hull ray
        return (FibreRatio(axialStress + bendingYStress + bendingZStress, e.FctmMpa), governing);
    }

    // §6.2.2 V_Rd,c floored at v_min for a LINKLESS section; a LINKED one is decidable only at the §6.2.3(3)
    // web-crushing ceiling, V_Rd,s needing the stirrup SPACING the RcSection lacks — so a linked pass DEFERS and a
    // linked fail refutes outright, no spacing curing crushing.
    static double ShearResistanceKn(RcElastic e) {
        double d = Math.Max(e.EffectiveDepthMm, 1.0), bw = Math.Max(e.WidthMm, 1.0);
        double k = Math.Min(1.0 + Math.Sqrt(200.0 / d), 2.0);
        double rho = Math.Min(e.TensionSteelAreaMm2 / (bw * d), 0.02);
        double vrdc = Math.Max(0.12 * k * Math.Cbrt(100.0 * rho * e.FckMpa), 0.035 * Math.Pow(k, 1.5) * Math.Sqrt(e.FckMpa)) * bw * d * 1e-3;
        return e.ShearLinkAreaMm2 > 0.0 ? e.VrdMaxKn : vrdc;
    }

    // Combined axial-flexure through the receipt's OWN basis kernel, selected by the row and never by a branch here.
    // Both bases read the PER-AXIS ratios — a moment resultant folded against the major resistance alone is the
    // DELETED unconservative spelling, crediting a weak-axis moment the full major/minor ratio (3-10× on an I-shape).
    // The three operands are each a candidate, so an operand the section cannot state propagates absence.
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

    // The EN 1995 kernel folds the km-swapped MAX pair, the AxialRegime row selecting §6.3.2's linear term or
    // §6.2.4's quadratic n². §6.1.5 bearing is section-UNDECIDABLE — R_90,Rd arrives PER MM and the length is support
    // DETAILING — so a bearing demand attaches its obligation to the WHOLE verdict rather than dividing against a fabricated w×d area.
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

    // §6.2 cross-section unity through the row's Linear kernel, the combined candidate carrying the §6.3 buckling
    // deferral by NAME: χ over the effective length is the forward check's, so a slender die never passes silent.
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

    // A zero range is trivially satisfied; a real range with no lawful count is the ABSENT candidate — no S-N law
    // prices it, so the verdict is structural Unbounded rather than a fabricated finite resistance.
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

    // The DOWNLOAD against the two precomputed axial capacities, worst-folded so the report names WHICH governs.
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

    // ONE ratio structure over the basis's own algebra row: a net-TENSION axial governs outright on either code
    // (TMS §9.2.5 neglects URM axial tensile strength; EN 1996 §6.1 admits none). A basis publishing NO masonry
    // algebra prices nothing, so every candidate is absent and the verdict is structurally Unbounded rather than
    // computed through a body the jurisdiction never wrote.
    static Utilisation MasonryUtilisation(MasonryUnreinforced m, Demand demand) {
        Option<MasonryResistances> resisted = m.Basis.Masonry.Map(algebra =>
            algebra.Unreinforced(m.Basis, m.Wall, Math.Max(0.0, -demand.AxialKn)));
        // The axial branch is on the CAPACITY column, never on two ratio constructions: a net-tension demand meets
        // the axial tensile resistance neither code grants URM, a compression demand the slenderness-reduced Pn.
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

    // ONE steel-couple algebra both codes publish, the row supplying only the STRESS SCALARS: §9.3.4.1.1 / §6.6.2
    // axial Pn over the stress block, §9.3.5 / §6.6.1 flexure Mn = As·fy·(d − a/2) with net tension on the steel
    // alone, and the §9.3.4.1.2 shear screen at the M/(V·dv) = 1 bound — the ONE form both bases run, since EN
    // §6.7.2 and TMS alike complete only with the bar spacing. Bar STATIONS are lattice facts, never section columns.
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
            // Vnm alone is section-decidable: Vns needs the bar SPACING, so a shear-governed reinforced verdict
            // DEFERS rather than reporting a resistance the section cannot complete.
            (couple.Bind(c => GuardedRatio(demand.ShearResultantKn, c.PhiV * vnm)), GoverningAction.Shear,
                Some(MemberCheckRequirement.ReinforcedMasonryShearSpacing)),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // BOTH plate bending directions fold against the SAME isotropic per-metre resistance, their SUM the
    // conservative bound: dividing the whole-unit moment by one pane's resistance over-rated every asymmetric build.
    static Utilisation GlassUtilisation(GlassPane g, Demand demand) =>
        Worst(
            (GuardedRatio((Math.Abs(demand.MomentYKnm) + Math.Abs(demand.MomentZKnm)) * g.LoadShareFraction, g.BendingKnmPerM),
                GoverningAction.Flexure, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    // Three resisted axes: the shear resultant, a POSITIVE axial (uplift) against the tension column, and the seat
    // reaction against bearing. A compressive axial rides the member, and moments and torsion are unresisted at
    // connection altitude. ONE guarded construction covers an absent tension band and a published zero alike.
    static Utilisation ConnectionUtilisation(Connection c, Demand demand) =>
        Worst(
            (LateralRatio(c, demand), GoverningAction.Shear, c.Defer),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), c.TensionKn), GoverningAction.Axial, c.Defer),
            (GuardedRatio(demand.BearingKn, c.BearingKn), GoverningAction.Bearing, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None));

    // A sheathed panel carries shear in its plane and nothing else — a moment or a bearing reaction stated against
    // one is a modelling error the verdict must surface, never a column this case quietly ignores.
    static Utilisation LateralUtilisation(LateralPanel p, Demand demand) =>
        Worst(
            (GuardedRatio(demand.UnitShearKnPerM, p.DesignKnPerM), GoverningAction.InPlaneShear, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    // A SINGLE published direction reads the demand resultant; a PAIR folds the per-axis ratios through the
    // report's own rule, sequenced so either axis lacking its resistance leaves the candidate absent.
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

    // The ONE candidate-ratio constructor and the ONLY site deciding whether a candidate EXISTS: a zero demand is
    // trivially satisfied (an unpriced column never governs an unloaded member), a real demand over a positive finite
    // capacity divides, and a real demand against a capacity the case does not publish is ABSENT — the structural
    // failure the fold reads as Unbounded. Both sides guard, because both can arrive unstatable.
    static Option<double> GuardedRatio(double demand, double capacity) =>
        (Math.Abs(demand) <= double.Epsilon, double.IsFinite(demand) && capacity > 0.0 && double.IsFinite(capacity)) switch {
            (true, _)      => Some(0.0),
            (false, true)  => Some(Math.Abs(demand) / capacity),
            (false, false) => None,
        };

    // The SIGNED counterpart both fibre screens ride: compression RELIEVES the fibre and must lose the fold, where
    // a magnitude reports a governance the state lacks; a limit the basis does not publish is the absent candidate.
    static Option<double> FibreRatio(double stress, double limit) =>
        (double.IsFinite(stress) && limit > 0.0, stress <= 0.0) switch {
            (true, _)      => Some(stress / limit),
            (false, true)  => Some(0.0),
            (false, false) => None,
        };

    // The unified governing-axis fold, so a check reports WHICH action governs; the span-params buffer stack-
    // allocates per Check and the strict-greater fold keeps the earliest-maximal tie-break. ABSENCE DOMINATES BY
    // STRUCTURE: a candidate with no ratio outranks every present one and the verdict is Unbounded by REACHING it,
    // never by comparing a sentinel. A PRESENT candidate carries its DEFERRAL; an ABSENT one carries it no further.
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
    // The ONE declared-build boundary: dispatch the request onto its solver, admit the eager solve ONCE, coerce the
    // UnitsNet outputs to SI scalars at the edge, classifies documented refusals with their cause, and preserves every
    // unknown VividOrange throw as its exact exceptional Error.
    public static Fin<SectionCapacity> Resolve(CapacityBuild build, Op key) =>
        build.Switch(
            hull: h => key.Catch(
                    () => Fin.Succ(new ForceMomentEngine(h.Section.Section, h.Resolution.ToSettings()).Mesh),
                    cause => cause.Exception.Case is ArgumentException
                        ? Some(new ComponentFault.CapacitySolve(key, cause))
                        : None)
                .Map(mesh => (SectionCapacity)new RcInteraction(h.Subject, h.Resolution, mesh)),
            // The two face queries are Option-typed AT the RcSection seam and prove INDEPENDENTLY, so a section
            // missing both names both; the lazy gross-integral reads trap in one Op.Catch so no throw escapes.
            elastic: e =>
                (e.Section.EffectiveDepthMm(SectionFace.Bottom).ToValidation((Error)new ComponentFault.EffectiveDepthUnavailable(key, e.Subject)),
                 e.Section.FaceSteelAreaMm2(SectionFace.Bottom).ToValidation((Error)new ComponentFault.TensionChordUnavailable(key, e.Subject)))
                    .Apply(static (depth, steel) => (Depth: depth, Steel: steel)).As().ToFin()
                    .Bind(chord => key.Catch(() => {
                        double fck = EnConcreteFactory.CreateLinearElastic(e.Section.Concrete.Grade).Strength.Megapascals;
                        return Fin.Succ<SectionCapacity>(new RcElastic(
                            e.Section.GrossSteelAreaMm2,
                            chord.Steel,                                                     // tension steel As — the EC2 ρl input
                            e.Section.ShearLinkAreaMm2,                                      // two-leg link area Asw (engine: 2·A_link)
                            e.Section.LinkYieldMpa.Map(static fyk => DesignBasis.En1992.Resist(ResistanceAction.Reinforcement, fyk)),
                            e.Section.ConcreteAreaMm2,
                            e.Section.ReinforcementRatio,
                            e.Section.Properties.MomentOfInertiaYy.MillimetersToTheFourth,   // GROSS uncracked — the SLS fibre divisor
                            e.Section.Properties.MomentOfInertiaZz.MillimetersToTheFourth,
                            e.Section.ReinforcementInertiaYyMm4,                             // Σ(As·d²) — the cracked-Icr readout
                            e.Section.ReinforcementInertiaZzMm4,
                            chord.Depth,
                            e.Section.ConcreteProfile.GrossRectangleMm.DepthMm.Value,        // gross h — the major-axis lever cy = h/2
                            e.Section.ConcreteProfile.GrossRectangleMm.WidthMm.Value,        // gross b — the minor-axis lever cz = b/2
                            fck,
                            Fctm(fck)));
                    }, cause => EnGrade.GradeRefusal(key, cause))),
            detail: d => Fin.Succ((SectionCapacity)new Fatigue(d.Law)),
            anchorage: a => Anchoring(a, key),
            bearing: b => Fin.Succ(Baseplating(b.Plate)));

    // ONE canonical name over the request union, the case the modality discriminant — never a per-family factory
    // roster and never an overload set. Each case carries its family receipt WHOLE into the rail as kN·m/kN, every
    // column read DIRECTLY off it, so no redundant parallel lift parameter exists.
    public static Fin<SectionCapacity> Lift(CapacityReceipt receipt, Op key) => receipt.Switch(
        steel: static r => Held(new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0)),
        timber: static r => Held(new TimberMember(
            r.Capacity.BendingNmm * 1e-6, r.Capacity.BendingMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.BearingPerpNPerMm * 1e-3, r.Capacity.TorsionalNmm * 1e-6,
            r.Capacity.RelativeSlenderness, r.Capacity.Km, r.Capacity.Kmod)),
        // The deck's AISI receipt lands the SAME case — one cold-formed verdict shape for a stud and a sheet; only
        // the receipt KIND distinguishes them for the report and the analytics dimension.
        deckSheet: static r => Held(new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0)),
        // The reduction MINTS here off the basis, the carried height, and the section's governing radius, so no
        // caller re-derives either bracket; the tension limit and shear bond read the basis's OWN table — TMS Table
        // 9.1.9.2 by mortar or EN Table 3.4/3.5 by unit group — under the ONE span direction the rupture row states.
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
            r.Unit.ReinforcedCells * Math.PI / 4.0 * r.Unit.RebarBarMm * r.Unit.RebarBarMm,   // As off the lattice facts
            r.Section.AreaMm2.Value, r.Unit.WMm / 2.0, r.Unit.LMm,                            // d = W/2 mid-wall bars, b the bed length
            MasonryReduction.Of(r.Basis, r.HeightMm, r.Section.GoverningRadiusMm).Value))),
        glass: static r => Held(new GlassPane(r.Capacity.StripBendingKnmPerM, r.Capacity.ResistanceMpa, r.Capacity.EffectiveThicknessMm, r.Capacity.LoadShareFraction)),
        // ky,θ scales every STRENGTH column and kE,θ rides StiffnessRetention; classification/slenderness/χ carry
        // unchanged and the AMBIENT basis stands, EN 1993-1-2 modifying the resistance and not the jurisdiction.
        steelFire: static r => Held(new SteelMember(
            r.Ambient.Basis,
            r.Ambient.FlexuralNmm * r.Ky * 1e-6, r.Ambient.FlexuralMinorNmm * r.Ky * 1e-6, r.Ambient.CompressionN * r.Ky * 1e-3,
            r.Ambient.ShearN * r.Ky * 1e-3, r.Ambient.TorsionalNmm * r.Ky * 1e-6, r.Ambient.Classification, r.Ambient.Slenderness,
            r.Ambient.Chi, r.Ambient.ChiLt, StiffnessRetention: r.Ke)),
        // The EN 1995-1-2 residual section is already priced at kmod = γM = 1.0 by the timber owner, so the fire arm
        // lifts it verbatim — the charring is geometry, never a factor applied here.
        timberFire: static r => Held(new TimberMember(
            r.Residual.BendingNmm * 1e-6, r.Residual.BendingMinorNmm * 1e-6, r.Residual.CompressionN * 1e-3,
            r.Residual.ShearN * 1e-3, r.Residual.BearingPerpNPerMm * 1e-3, r.Residual.TorsionalNmm * 1e-6,
            r.Residual.RelativeSlenderness, r.Residual.Km, r.Residual.Kmod)),
        // AWS D1.1 publishes one shear band and no tension allowable, so the tension column is ABSENT rather than 0;
        // an electrode publishing no shear area collapses onto the unprovided-0 column and governs loud.
        weld: static r => Held(new Connection(DesignBasis.AwsD11, r.Row.DirectionalShearKn(Angle.FromDegrees(r.LoadAngleDeg)), None, None)),
        // The ASTM C1401 structural-bite tension is the adhesive row's OWN Option: a silicone SSG row publishes it,
        // an epoxy/MMA/PU row does not, and that ABSENCE is distinct from zero resistance.
        adhesive: static r => Held(new Connection(DesignBasis.AstmD1002, Some(r.Row.DesignShearKn), r.Row.DesignTensionKn, None)),
        stud: static r => Held(new Connection(DesignBasis.Aisc360, Some(Math.Max(r.Count, 0) * r.Row.DesignShearKn(r.Group)), None, None)),
        // Duration scaling ran at ConnectorRow.GovernedCapacity where each cell meets its OWN basis, so this lift
        // re-scales nothing; an absent direction stays ABSENT, a report publishing no uplift differing from zero.
        connector: static r => Held(new Connection(DesignBasis.IccEs,
            r.Capacity.LateralF1Kn, r.Capacity.UpliftKn, r.Capacity.DownloadKn,
            r.Capacity.LateralF2Kn.Map(second => new LateralPair(second, r.Capacity.Rule)))),
        // The panel family already applied the §4.1.4 reduction on its own rail, so the lift is a straight seat.
        lateralPanel: static r => Held(new LateralPanel(DesignBasis.Sdpws, r.DesignKnPerM, r.Hazard)),
        // §3.6 bearing-type: the assembly's OWN plane-counted shear, head-factored tension, and ply bearing, read
        // rather than re-derived. All three are EN-RAILED, so an untabulated grade refuses the lift with its exact
        // typed error rather than becoming a zero or an absent column.
        // All three projections take the JOINTS basis explicitly, because Fastening.JointFactor refuses a
        // resistance-factor basis outright — passing one silently would publish a resistance no body computed.
        bolt: r =>
            from shear in r.Assembly.ShearResistanceKn(r.Plane, DesignBasis.En1993Joints, key)
            from tension in r.Assembly.TensionResistanceKn(DesignBasis.En1993Joints, key)
            from bearing in r.Assembly.BearingResistanceKn(r.Bearing, DesignBasis.En1993Joints, key)
            select (SectionCapacity)new Connection(DesignBasis.En1993Joints, Some(shear), Some(tension), Some(bearing)),
        // §3.9 slip resistance: a non-preloaded assembly answers None, retained as an absent shear column so a
        // slip-critical demand on a snug-tight joint governs loud rather than reading bearing by accident.
        slipCritical: static r => Held(new Connection(DesignBasis.En1993Joints, r.Assembly.SlipResistanceKn(r.Install), None, None)),
        // EC5 §8: the family owner's railed six-mode Johansen minimum per shear plane, summed over the planes.
        timberDowel: static r => Held(new Connection(DesignBasis.En1995, Some(Math.Max(r.Planes, 0) * r.PerPlaneShearKn), None, None)),
        // EN 1999: the banded (fo, fu) pair arrives PROVEN (the aluminum seed refused any die outside its printed
        // window), and the resistances compute on the class-3 elastic floor under the row's γM1-covers-everything set through the ONE Resist fold.
        aluminum: static r => Held(new AluminumMember(
            r.Basis,
            r.Basis.Resist(ResistanceAction.Stability, r.Section.SxMm3.Value * r.FoMpa * 1e-6),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.SyMm3.Value * r.FoMpa * 1e-6),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.AreaMm2.Value * r.FoMpa * 1e-3),
            r.Basis.Resist(ResistanceAction.Stability, r.Section.AvyMm2.Value * r.FoMpa / Math.Sqrt(3.0) * 1e-3),
            BucklingCurve(r.Grade),
            r.FoMpa, r.FuMpa)));

    static Fin<SectionCapacity> Held(SectionCapacity capacity) => Fin.Succ(capacity);

    // The yield off the ONE MaterialGrade row: a non-Rebar arm, or a Rebar arm publishing no characteristic yield,
    // states ABSENCE and the couple candidate governs loud like every other unpublished column on this rail.
    static Option<double> ReinforcingYieldMpa(MaterialGrade grade) =>
        grade.Columns is GradeProperties.Rebar rebar ? rebar.YieldMpa : None;

    // The §6.3.1.2 buckling class off the grade's Aluminum arm — the class row owns its own Table 6.6 pair, so no
    // constant is copied onto the receipt and an edit to the table moves both readers at once.
    static Option<BucklingClass> BucklingCurve(MaterialGrade grade) =>
        grade.Columns is GradeProperties.Aluminum alloy ? Some(alloy.Class) : None;

    // The one site the EN 1992-4 single-anchor coefficients live. An EN-tabulated grade mins its steel mode beside
    // the cone; an untabulated band refuses typed rather than becoming a zero. Group areas, the shear edge mode,
    // the ETA pullout, and EN pryout at its unproven k8 ride the one deferral; the ACI set waits whole on its φ roster.
    static Fin<SectionCapacity> Anchoring(CapacityBuild.Anchorage a, Op key) {
        double k1 = a.Bed.Cracked ? 8.9 : 12.7;                                     // EN 1992-4 cast-in headed kcr,N/kucr,N
        double edge = a.Bed.EdgeMm.Map(ca => Math.Min(0.7 + 0.3 * ca / (1.5 * a.Bed.HefMm.Value), 1.0)).IfNone(1.0);
        double coneKn = DesignBasis.En1992Anchors.Resist(ResistanceAction.CrossSection,
            k1 * Math.Sqrt(a.Bed.FckMpa) * Math.Pow(a.Bed.HefMm.Value, 1.5) * edge * 1e-3);
        return from shear in a.Assembly.ShearResistanceKn(a.Plane, DesignBasis.En1992Anchors, key)
               from tension in a.Assembly.TensionResistanceKn(DesignBasis.En1992Anchors, key)
               select (SectionCapacity)new Connection(
                   DesignBasis.En1992Anchors, Some(shear), Some(Math.Min(tension, coneKn)), None,
                   Defer: Some(MemberCheckRequirement.AnchorForwardModes));
    }

    // §J8 bearing at φ = 0.65 with √(A2/A1) clamped at 2, and the cantilever plate bending — t_min inverted at the
    // plate's own thickness, l = max(m, n, n′) under the two-sourced λ = 1 bound, wide-flange only (the HSS/pipe m–n
    // variants are typed-absent). EXPRESSION_SPINE exemption: the geometry scalars bind once, one BasePlate exits.
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

    // The TMS 402 §9.2.6.1 running-bond shear constant (56 psi) lifted onto the ShearBondMpa column so the
    // unreinforced shear arm reads one DATA source on either basis — the EN side fills it from Table 3.5.
    const double TmsRunningBondShearMpa = 0.386;

    // EC2 mean flexural tensile strength: fctm = 0.30·fck^(2/3) for ≤C50, 2.12·ln(1+(fck+8)/10) above. The fck
    // source is EnConcreteFactory.CreateLinearElastic(grade).Strength — verified the parsed characteristic cylinder
    // strength, not the design fcd (.api/api-vividorange-materials.md).
    static double Fctm(double fckMpa) =>
        fckMpa <= 50.0 ? 0.30 * Math.Pow(fckMpa, 2.0 / 3.0) : 2.12 * Math.Log(1.0 + (fckMpa + 8.0) / 10.0);

    // Through the ITaxonomySerializable marker IForceMomentMesh itself extends ($type-tagged wire, UnitsNet
    // SI-scalar quantities); Thaw rehydrates via that tag WITHOUT re-running the Steps² sweep.
    internal static Fin<string> Freeze(RcInteraction capacity, Op key) =>
        key.Catch(() => Fin.Succ(capacity.Hull.ToJson()));

    internal static Fin<SectionCapacity> Thaw(ComponentId subject, DiagramResolution resolution, string json, Op key) =>
        key.Catch(
                () => Fin.Succ(json.FromJson<IForceMomentMesh>()),
                cause => cause.Exception.Case is JsonException
                    ? Some(new ComponentFault.CapacityDecode(key, cause))
                    : None)
            .Bind(mesh => mesh is null
                ? Fin.Fail<SectionCapacity>(new ComponentFault.CapacityDocumentEmpty(key, subject))
                : Fin.Succ((SectionCapacity)new RcInteraction(subject, resolution, mesh)));

    // The hull carries N-M-M resistance ONLY, so the ray verdict worst-folds with the shear/torsion/bearing demands
    // against 0 and an unresisted action governs LOUD. A hull enclosing the demand direction answers the smallest
    // positive pierce and the utilisation is its reciprocal; a hull the ray never pierces bounds nothing along it,
    // and THAT is the absent candidate — reached by the pierce set being empty, never by a magnitude.
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

    // EXPRESSION_SPINE measured-kernel exemption: Möller-Trumbore ray-triangle scalar kernel — span-free numeric
    // intermediates with early degenerate exits, the bounded kernel role the doctrine names for statement bodies.
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

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The content-keyed hull round trip as ONE fold, so the Freeze/Thaw pair has a call site rather than a claim. The
// key pair IS the whole preimage — the section identity and the resolution are the only inputs the solve reads — so
// a resolution change is a distinct row, never a stale hit; the artifact store is the ONLY Thaw ingress, the $type
// wire being a deserialization-gadget surface no peer document may reach.
public readonly record struct HullCache(SectionCapacity Capacity, Option<string> Pending) {
    public static string Key(ComponentId subject, DiagramResolution resolution) => $"{subject.Value}:{resolution.Key}";

    public static Fin<HullCache> Of(CapacityBuild.Hull build, Func<string, Option<string>> read, Op key) =>
        read(Key(build.Subject, build.Resolution)).Match(
            Some: body => SectionCapacity.Thaw(build.Subject, build.Resolution, body, key)
                .Map(capacity => new HullCache(capacity, None)),
            None: () => SectionCapacity.Resolve(build, key)
                .Bind(capacity => SectionCapacity.Freeze((SectionCapacity.RcInteraction)capacity, key)
                    .Map(body => new HullCache(capacity, Some(body)))));
}
```

## [06]-[SECTION_SELECTION]

- Owner: `SectionSelection` is the INVERSE of `Check` — design as selection over a produced candidate sequence under ONE law. `SectionCandidate<TSubject>` is that sequence's element: the subject the query returns, the linear mass it ranks by, and the DEFERRED capacity the demand is checked against; `Least` is the one acceptance fold; the three producers are the only thing that differs between a catalogue query, a fabricated-member sweep, and a bolt sizing scan.
- Cases: `Stocked` scans the frozen catalogue (a section someone stocked); `Fabricated` sweeps a caller-parameterized composition space (a section nobody stocked yet), solving each candidate HERE because a fabricated section has no catalogue entry to have solved it; `Threaded` sweeps the (thread × grade) lattice the fastener standards tables publish, which is what makes every catalogued thread and grade REACHABLE — a row no `Stocked` selection names is still selected the moment its size and system fit the demand, so the tables are the admission domain rather than decoration.
- Entry: `SectionSelection.Least(candidates, demand, key)` over any producer's output. Mass is `Area × ρ(substance)`, never area alone: area ranks linear mass only INSIDE one substance, so a mixed steel/timber/masonry catalogue ordered by area returns a 90 mm sawn section ahead of every W-shape. Density arrives through the caller's `densityOf` projection (the composing root binds it to `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Lookup(id, key)`'s Mechanical density column), so `admit` stays a genuine POLICY filter — a stocked subset, a depth cap, one `SteelClass` — rather than a correctness precondition the type system never enforced.
- Growth: a new search space is one producer returning the ranked candidate sequence; the fold, the acceptance rule, and the exhaustion fault are already written.
- Boundary: the capacity is a THUNK because only the lightest passing candidate is worth pricing — an eager capacity column pays a hull solve per catalogue row. The fold is therefore LAZY and halts on the first pass OR the first fault: a candidate whose density or capacity FAULTS aborts the scan loud (a filter admitting a family the projections cannot price is a caller defect, never a silently skipped row), and an exhausted search faults typed.
- Boundary: acceptance is the SECTION-altitude verdict, so a linked RC section that passes and merely owes stirrup detailing returns WITH its deferral for the caller to route forward — the strict `Adequate` bit stays the terminal report's, never the sizing gate's.
- Boundary: NAMED LOSS on absorbing the fastener-grain scan — the retired `LeastShear` returned the winning EN 1993-1-8 resistance and ranked by thread major diameter; this fold returns the `Utilisation` verdict and ranks by real linear mass. WITNESS: the resistance is `demand / verdict.Ratio` off the returned verdict, and the mass rank orders a mixed-grade sweep correctly where diameter ordered it only within one substance.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The SOURCE decides all three columns, so the fold below is one body over every search space; Capacity is DEFERRED
// because the scan prices candidates only until the lightest passer.
public readonly record struct SectionCandidate<TSubject>(TSubject Subject, double MassPerMm, Func<Fin<SectionCapacity>> Capacity);

// The joint state a bolt sizing scan holds FIXED while it sweeps the thread and grade rosters. Every column is a
// joint DECLARATION no roster row carries, so the scan varies exactly the two axes the tables publish.
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

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SectionSelection {
    // Every stocked row the policy filter admits, priced through the ONE ComponentFamily.Capacity currency — so no
    // caller hands in a capacity lambda the row already determines, nor prices a family through a foreign arm.
    public static Fin<Seq<SectionCandidate<Component>>> Stocked(
        FrozenDictionary<ComponentId, Component> rows,
        FrozenDictionary<ComponentId, ComputedSection> sections,
        Func<Component, bool> admit,
        CapacityPlacement placement,
        Func<MaterialId, Fin<double>> densityOf,
        Op key) =>
        toSeq(sections)
            .Filter(pair => rows.ContainsKey(pair.Key) && admit(rows[pair.Key]))
            .Traverse(pair => densityOf(rows[pair.Key].SubstanceId).Map(density =>
                Candidate(rows[pair.Key], pair.Value, density, placement, key)))
            .As();

    // The GENERATIVE counterpart: component#SECTION_SOLVER Compose already prices an arbitrary positioned member set
    // exactly, so a caller-supplied sweep — a plate-girder web-depth × flange-width lattice, a battened-column
    // spacing sweep — folds through the SAME solve, Check, and acceptance as a catalogue row. The generator is
    // INDEXED, so the sweep is a pure function of its ordinal: replayable, and cappable without a mutable cursor.
    public static Fin<Seq<SectionCandidate<Component>>> Fabricated(
        Func<int, Seq<(Component Row, SectionProfile Profile)>> sweep,
        int sweeps,
        CapacityPlacement placement,
        Func<MaterialId, Fin<double>> densityOf,
        Op key) =>
        toSeq(Enumerable.Range(0, Math.Max(sweeps, 0))).Bind(sweep)
            .Traverse(candidate => SectionSolver.Solve(candidate.Profile, key)
                .Bind(section => densityOf(candidate.Row.SubstanceId)
                    .Map(density => Candidate(candidate.Row, section, density, placement, key))))
            .As();

    // The (thread × grade) lattice under one declared joint, ranked by the shank's tensile stress area against its
    // grade's density — the roster read off the ONE MaterialGrade owner, so a new grade is scannable on landing.
    public static Fin<Seq<SectionCandidate<(ThreadRow Thread, MaterialGrade Grade)>>> Threaded(
        BoltJoint joint,
        Func<MaterialId, Fin<double>> densityOf,
        Op key) =>
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
                            joint.GripPlies, joint.ShearPlanes, joint.Washer, key)
                        .Bind(assembly => SectionCapacity.Lift(
                            new CapacityReceipt.Bolt(joint.Subject, assembly, joint.Bearing, joint.Plane), key)))))
            .As();

    // The first MASS-ORDERED candidate passing at section altitude wins and carries its verdict verbatim. The scan
    // is lazy, so it stops AT the win rather than walking the tail re-testing a decided state.
    public static Fin<(TSubject Subject, Utilisation Verdict)> Least<TSubject>(
        Fin<Seq<SectionCandidate<TSubject>>> candidates,
        Demand demand,
        Op key) =>
        candidates.Bind(ranked =>
            toSeq(ranked.OrderBy(static candidate => candidate.MassPerMm))
                .AsIterable()
                .Map(candidate => candidate.Capacity().Map(capacity => (candidate.Subject, Verdict: capacity.Check(demand))))
                .Choose(static priced => priced.Match(
                    Succ: static won => won.Verdict.SectionPasses ? Some(Fin.Succ(won)) : None,
                    Fail: static fault => Some(Fin.Fail<(TSubject Subject, Utilisation Verdict)>(fault))))
                .Head()
                .IfNone(() => Fin.Fail<(TSubject Subject, Utilisation Verdict)>(
                    new ComponentFault.SelectionExhausted(key, typeof(TSubject)))));

    static SectionCandidate<Component> Candidate(Component row, ComputedSection section, double density,
        CapacityPlacement placement, Op key) =>
        new(row, section.AreaMm2.Value * density, () => row.Family.Capacity(row, Some(section), placement, key));

}
```

## [07]-[RESEARCH]

(none)
