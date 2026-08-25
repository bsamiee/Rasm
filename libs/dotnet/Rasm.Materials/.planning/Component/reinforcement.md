# [MATERIALS_REINFORCEMENT]

THE REINFORCING-BAR SEED FAMILY and THE HOST-NEUTRAL REINFORCED-CONCRETE-SECTION ASSEMBLER. A rebar and a tendon are each one `ComponentRow` over ONE `ReinforcementRow` roster and ONE `SeedLaw` the `ComponentFamily.Reinforcement` policy row binds (`ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Circle`, cross-nominal the circle diameter — the bar's own disc solves like any other section while its RC participation rides `[03]-[RC_SECTION]`, which reads the assembled layer geometry rather than a single bar's receipt), never a `Rebar` type and never a bespoke `RebarSection` payload: the geometry is `SectionProfile.Circle(DiameterMm)`, the IFC stamp is the row's own binding, and the realization detail is the seed-built `ReinforcementDetail.Of` bag.

Under `SEED_ROW_LAW` the pure standards data are frozen row tables with per-column evidence — `Bars` (imperial, CSA, and EN nominal sizes, the EN H-series VENDOR-keyed to the `VividOrange.ISections` `BarDiameter` catalogue) and `ShapeCodes` (the BS 8666:2020 schedule set) — while every GRADE, passive and active alike, is a `component#MATERIAL_GRADE` `MaterialGrade` row over its `GradeProperties.Rebar` or `GradeProperties.Strand` arm. Beside the strand line rides the POST-TENSIONING HARDWARE estate as CLOSED KIND ROSTERS: `AnchorageKind`/`TendonConduitKind` carry the roster-verified `IfcTendonAnchor` (`TENSIONING_END`/`FIXED_END`/`COUPLER`) and `IfcTendonConduit` (`DUCT`/`GROUTING_DUCT`/`TRUMPET`/`DIABOLO`/`COUPLER`) leaves, `TendonProfileKind` the drape vocabulary, and `Ducts` the one row currency still holding a certificate slot — every kind ROSTER now owning its one reader — the seeded post-tensioned tendon assembly — with row coverage growing by seeded tendon (the two Post rows exercise two rows per roster today), never a vocabulary declared beside a fold that no row touches. `RebarSchedule` owns the bend algebra over one `SizeLadder` correspondence, and its receipt is what a BAR-BENDING SCHEDULE is made of: every placed bar publishes its BS 8666 shape code and, where it is bent, its ACI inside-bend diameter, hook extension, and EN mandrel diameter into the realization bag under the names the shop deliverable reads. The `ForceBasis` and `TendonBasis` policy rows own the passive and active force projections over one shape, and the jacking ceiling resolves at `ComponentAuthority.JackingCeilingMpa` — the two-term minimum evaluated at the body that publishes both terms.

`[03]-[RC_SECTION]` is the family-agnostic assembler: `RcSectionBuilder.Of` lowers EN grades through the `EnGrade` boundary, builds the `VividOrange.Sections` `ConcreteSection` over the PROFILE-FAITHFUL `SectionSolver.ProfileOf(concrete.Profile, key)` `IProfile` of ANY `Component`, folds the `RebarLayout` `[Union]` through the collapsed placement engines, and mints the `RcSection` receipt whose transformed-section columns read the `ConcreteSectionProperties` carrier — never a hand-summed bar loop. `RcSectionBuilder.Capacity` is the reinforcement-side ENTRY into the capacity rail. Growth is one row: a new bar one `BarRow`, a new grade one `MaterialGrade` row, a new role one `RebarUsage` row, a new schedule shape one `ShapeCodes` static, a new placed bar or tendon one `ReinforcementRow` case value.

## [01]-[INDEX]

- [02]-[REINFORCEMENT_FAMILY]: the policy SmartEnums (`RebarStandard` · `RebarUsage` · `RebarSurface` · `RibPattern` · `HookKind` · `RebarHook` · `RelaxationClass`), the `SizeLadder` published-band correspondence, the `Bars` and `ShapeCodes` frozen tables, the PT-hardware kind rosters (`AnchorageKind` · `TendonConduitKind` · `TendonProfileKind` with the `Ducts` certificate-slot rows), the `GradeProperties.Rebar`/`Strand` arm physics and the `MaterialGrade` reinforcement members, the `RebarRibGeometry`/`RebarBend` receipts, the `RebarSchedule` rib/hook algebra with the `ForceBasis`/`TendonBasis` policy rows and the `TendonForce` relaxation projection, the ONE `ReinforcementDetail.Of` realization bag, and the `ReinforcementSeed.Roster`/`Law`/`Capacity` triple.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete assembler — the `RebarLayout` `[Union]` over the four `VividOrange.Sections` placement engines, the `EnGrade` EN-grade admission boundary, `RcSectionBuilder.Of` over the family-agnostic `SectionSolver.ProfileOf` concrete outline, the `ConcreteSectionProperties`-backed transformed-section receipt columns, and the EC2 `MinimumReinforcementSpacing` rule with the aggregate term wired.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: `RebarStandard`/`RebarUsage`/`RebarSurface`/`RibPattern`/`HookKind`/`RebarHook` the policy vocabularies; `AnchorageKind`/`TendonConduitKind`/`TendonProfileKind` the PT-hardware kind rosters with their roster-verified IFC leaves; `SizeLadder` the ONE published size-band correspondence three ladders share; `BarRow`/`ShapeCodeRow`/`ConduitRow` the row currencies with `Bars`/`ShapeCodes`/`Ducts` the frozen tables; `GradeProperties.Rebar`/`Strand` the grade physics; `RebarRibGeometry`/`RebarBend` the receipts; `RebarSchedule` the rib/hook operation owner; `ForceBasis`/`TendonBasis` the force-policy rows; `ReinforcementDetail` the ONE seed-time bag builder; `ReinforcementSeed` the roster and law `component#COMPONENT_OWNER` binds.
- Cases: `ReinforcementRow.{Bar · Tendon}` — grade {A615 Gr40/Gr60/Gr75/Gr80 (carbon, non-weldable) · A706 Gr60W/Gr80W (low-alloy, weldable) · G30.18 400W/500W (CSA metric, weldable) · EN 10080 B500A/B500B/B500C (the ductility classes `EnRebarFactory.CreateBiLinear` k = 1.05/1.08/1.15 branches read) · the five A416 / EN 10138-3 seven-wire strand rows} × size {#3..#11, #14, #18 imperial · 10M..55M CSA · H6..H50 EN keyed `BarDiameter.D6`..`D50`} × usage {the full verified 11-member `IfcReinforcingBarTypeEnum`} × surface {textured · plain} × rib-pattern {uniform-height 90° · crescent 60°} × hook {90°/135°/180° over development/stirrup-tie/seismic ACI tables} × shape-code {the BS 8666:2020 37-code set}; a tendon is pre-tensioned (no assembly) or POST-tensioned (an `AnchorageKind` × `ConduitRow` × `TendonProfileKind` assembly), and the standard-consistency law `MaterialGrade.Admits(BarRow)` is proven in the seed coherence census BEFORE construction.
- Entry: `ComponentSeed.Rows(context, ReinforcementSeed.Roster, ReinforcementSeed.Law)` — this page states the roster and the policy, never the fold, and the OWNER's applicative traverse names every offending row of BOTH modalities in one verdict. `RebarSchedule.StandardHook(bar, usage, kind, hook, key)` rejects a longitudinal usage paired with a tie/seismic hook policy and a transverse usage paired with the development policy before emitting the ACI/EN/BS bend receipt.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `EvidenceGrade`, `PropertyBag`/`DetailSchema`/`Dimension`), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue the EN `BarRow` rows VENDOR-key; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnRebarGrade` the EN-bodied binding, `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` the registered yield + ductility ultimate; `.api/api-vividorange-materials.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[Union]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Option`/`Traverse`/`.Apply`/`guard`), BCL inbox (`FrozenDictionary`, `ImmutableArray`).
- Growth: one row per new fact — a new bar size one `BarRow`, a new grade one `MaterialGrade` row on `component#MATERIAL_GRADE` bound to its `EnRebarGrade` when EN-bodied, a new role one `RebarUsage` row carrying its verified token, a new schedule shape one `ShapeCodes` static, a new hook table one `HookKind` row with its own bend ladder, a new realized bar or tendon one `ReinforcementRow` value, a new relaxation certification one `RelaxationClass` row; a new anchorage or conduit product is one `AnchorageKind`/`ConduitRow` whose dimension columns fill ONLY from an ETA/vendor certificate, and a duct row gaining a certified inner diameter both stamps its `DuctDiameter` bag row and seeds as an `IfcTendonConduit` component once the family admission widens to its annular profile — never a per-bar type, never a `ComponentFamily` edit. A welded mesh grows as an `IfcReinforcingMesh` projection over the same row currencies, never an eleventh family.
- Boundary: the seed admits raw standards data once through the symbolic `Bars` and `MaterialGrade` row references, the `MaterialGrade.Admits(BarRow)` size-system law, and the `SectionProfile.Circle.Of` rail. `RebarStandard` keeps its STANDARD IDENTITY alone — weldability and the size system its mills roll — because the issuing body is the `MaterialGrade` row's own `ComponentAuthority` column and the free-text authority string, the body column, and the region column were one fact spelled four ways. The regional receipt therefore derives at seed time from `grade.Authority.Region`. `ForceBasis.ForceKn` validates the bar/grade system before any registered-grade projection; `TendonBasis.Jacking` resolves the ceiling at `ComponentAuthority.JackingCeilingMpa`, which evaluates the two-term minimum both codes state (ACI 318 Table 20.3.2.5.1 `min(0.80·fpu, 0.94·fpy)`, EC2 §5.10.2.1 `min(0.80·fpk, 0.90·fp0,1k)`) at the body that publishes both coefficients — so a body with no published rule projects ABSENCE and no ultimate-term literal is re-typed here.
- Boundary: PT hardware carries no dimension a certificate has not published, so no hardware `Component` seeds today — the hardware reaches the seam as the post-tensioned tendon's own `ReinforcementDetail` rows (`AnchorageType`/`DuctDiameter`/`TendonProfile`, each stamped only where its column is `Some`), and the `DuctDiameter` row rides BESIDE the strand's own `NominalDiameter` because a tendon assembly carries both facts as two rows, exactly as `Rasm.Element/Properties/property#DETAIL_SCHEMA` declares them. The retired `AnchorageRow` was a key beside a kind and nothing else, and its strand-count column restated the mono-versus-multi split the kind row already makes, so the assembly references `AnchorageKind` directly; `ConduitRow` survives as the certificate-slot CURRENCY: both rows carry `InnerDiameterMm: None` today, so the `DuctDiameter` stamp is TYPED-ABSENT until a certified diameter two-sources — the reader exists and fires the day one cell fills, and a row whose slot never fills falls to the `AnchorageRow` verdict at that census.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;
using Thinktecture;
using VividOrange.Sections.Reinforcement;
using VividOrange.Sections.Exceptions;
using VividOrange.Materials.StandardMaterials.En;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615    = new("astm-a615",  weldable: false, rolls: static () => A615);
    public static readonly RebarStandard A706    = new("astm-a706",  weldable: true,  rolls: static () => A615);
    public static readonly RebarStandard G30     = new("csa-g30.18", weldable: true,  rolls: static () => G30);
    public static readonly RebarStandard En10080 = new("en-10080",   weldable: true,  rolls: static () => En10080);
    public bool Weldable { get; }
    [UseDelegateFromConstructor] public partial RebarStandard Rolls();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarUsage {
    public static readonly RebarUsage Main        = new("main",        ifcPredefinedType: "MAIN",        stirrup: false);
    public static readonly RebarUsage Ligature    = new("ligature",    ifcPredefinedType: "LIGATURE",    stirrup: true);
    public static readonly RebarUsage Shear       = new("shear",       ifcPredefinedType: "SHEAR",       stirrup: true);
    public static readonly RebarUsage Punching    = new("punching",    ifcPredefinedType: "PUNCHING",    stirrup: false);
    public static readonly RebarUsage Edge        = new("edge",        ifcPredefinedType: "EDGE",        stirrup: false);
    public static readonly RebarUsage Ring        = new("ring",        ifcPredefinedType: "RING",        stirrup: true);
    public static readonly RebarUsage Anchoring   = new("anchoring",   ifcPredefinedType: "ANCHORING",   stirrup: false);
    public static readonly RebarUsage Spacer      = new("spacer",      ifcPredefinedType: "SPACEBAR",    stirrup: false);
    public static readonly RebarUsage Stud        = new("stud",        ifcPredefinedType: "STUD",        stirrup: false);
    public static readonly RebarUsage UserDefined = new("userdefined", ifcPredefinedType: "USERDEFINED", stirrup: false);
    public static readonly RebarUsage NotDefined  = new("notdefined",  ifcPredefinedType: "NOTDEFINED",  stirrup: false);
    public string IfcPredefinedType { get; }
    public bool Stirrup { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarSurface {
    public static readonly RebarSurface Textured = new("textured", ifcSurface: "TEXTURED", ribbed: true);
    public static readonly RebarSurface Plain    = new("plain",    ifcSurface: "PLAIN",    ribbed: false);
    public string IfcSurface { get; }
    public bool Ribbed { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RibPattern {
    public static readonly RibPattern UniformHeight = new("uniform-height", inclinationDeg: 90.0);
    public static readonly RibPattern Crescent      = new("crescent",       inclinationDeg: 60.0);
    public double InclinationDeg { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RelaxationClass {
    public static readonly RelaxationClass LowRelaxation = new("low-relaxation", rho1000Percent: 2.5);
    public static readonly RelaxationClass Normal        = new("normal",         rho1000Percent: 8.0);
    public double Rho1000Percent { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnchorageKind {
    public static readonly AnchorageKind MonoLive  = new("mono-live",  ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind MultiLive = new("multi-live", ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind DeadEnd   = new("dead-end",   ifcPredefinedType: "FIXED_END");
    public static readonly AnchorageKind Coupler   = new("coupler",    ifcPredefinedType: "COUPLER");
    public string IfcPredefinedType { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TendonConduitKind {
    public static readonly TendonConduitKind CorrugatedSteel   = new("corrugated-steel",   ifcPredefinedType: "DUCT");
    public static readonly TendonConduitKind CorrugatedPlastic = new("corrugated-plastic", ifcPredefinedType: "DUCT");
    public static readonly TendonConduitKind Trumpet           = new("trumpet",            ifcPredefinedType: "TRUMPET");
    public static readonly TendonConduitKind Diabolo           = new("diabolo",            ifcPredefinedType: "DIABOLO");
    public static readonly TendonConduitKind GroutVent         = new("grout-vent",         ifcPredefinedType: "GROUTING_DUCT");
    public static readonly TendonConduitKind ConduitCoupler    = new("conduit-coupler",    ifcPredefinedType: "COUPLER");
    public string IfcPredefinedType { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TendonProfileKind {
    public static readonly TendonProfileKind Straight  = new("straight");
    public static readonly TendonProfileKind Parabolic = new("parabolic");
    public static readonly TendonProfileKind Harped    = new("harped");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SizeLadderRow(double CeilingMm, double Value);

public static class SizeLadder {
    public static Seq<SizeLadderRow> Of(params (double CeilingMm, double Value)[] rows) =>
        toSeq(rows).Map(static row => new SizeLadderRow(row.CeilingMm, row.Value));

    public static double At(Seq<SizeLadderRow> ladder, double diameterMm) =>
        ladder.Find(row => diameterMm <= row.CeilingMm).Map(static row => row.Value).IfNone(ladder[^1].Value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookKind {
    static readonly Seq<SizeLadderRow> DevelopmentBend = SizeLadder.Of((25.4, 6.0), (36.0, 8.0), (double.PositiveInfinity, 10.0));
    static readonly Seq<SizeLadderRow> TransverseBend  = SizeLadder.Of((16.0, 4.0), (double.PositiveInfinity, 6.0));

    public static readonly HookKind Development = new("development", aciTable: "ACI 318-19 Table 25.3.1", bend: DevelopmentBend);
    public static readonly HookKind StirrupTie  = new("stirrup-tie", aciTable: "ACI 318-19 Table 25.3.2", bend: TransverseBend);
    public static readonly HookKind Seismic     = new("seismic",     aciTable: "ACI 318-19 Table 25.3.4", bend: TransverseBend);
    public string AciTable { get; }
    public Seq<SizeLadderRow> Bend { get; }
    public double MinInsideBendFactor(double barDiameterMm) => SizeLadder.At(Bend, barDiameterMm);
}

public static class ReinforcementSteel {
    public const double DensityKgM3 = 7850.0;
}

public readonly record struct BarRow(string Key, RebarStandard Standard, Option<BarDiameter> CatalogueKey,
    double NominalDiameterMm, double NominalAreaMm2, double NominalWeightKgM);

public readonly record struct ShapeCodeRow(string Key, int Legs, bool Link);

public readonly record struct ConduitRow(string Key, TendonConduitKind Kind, Option<double> InnerDiameterMm);

public readonly record struct RebarRibGeometry(
    double TransverseRibHeightMm,
    double TransverseRibSpacingMm,
    double LongitudinalRibHeightMm,
    double FlankInclinationDeg,
    double RibInclinationDeg,
    double RelativeRibArea,
    double RiblessPerimeterFraction,
    RibPattern Pattern);

public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm, double MandrelDiameterMm, ShapeCodeRow Shape);

public readonly record struct TendonAssembly(AnchorageKind Anchorage, ConduitRow Duct, TendonProfileKind Profile);

public partial record GradeProperties {
    public sealed partial record Rebar {
        public Option<double> CharacteristicYieldMpa =>
            En.Map(static grade => EnRebarFactory.CreateLinearElastic(grade).Strength.Megapascals).IfNone(() => YieldMpa);
        public bool Weldable => Standard.Weldable;
        public bool Rolls(BarRow bar) => Standard.Rolls() == bar.Standard;
    }

    public sealed partial record Strand {
        public double ProofMpa => YieldRatio * UltimateMpa;
        public double NominalWeightKgM => AreaMm2 * 1e-6 * ReinforcementSteel.DensityKgM3;
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Rebar> RebarArm => Columns is GradeProperties.Rebar arm ? Some(arm) : None;
    public Option<GradeProperties.Strand> StrandArm => Columns is GradeProperties.Strand arm ? Some(arm) : None;
    public bool Admits(BarRow bar) => RebarArm.Exists(arm => arm.Rolls(bar));
    public ComponentStandard Receipt => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);
}

// --- [TABLES] --------------------------------------------------------------------------
public static class Bars {
    public static readonly BarRow No3  = new("no3",  RebarStandard.A615,    None,                     9.525, 71.0,   0.560);
    public static readonly BarRow No4  = new("no4",  RebarStandard.A615,    None,                    12.700, 129.0,  0.994);
    public static readonly BarRow No5  = new("no5",  RebarStandard.A615,    None,                    15.875, 199.0,  1.552);
    public static readonly BarRow No6  = new("no6",  RebarStandard.A615,    None,                    19.050, 284.0,  2.235);
    public static readonly BarRow No7  = new("no7",  RebarStandard.A615,    None,                    22.225, 387.0,  3.042);
    public static readonly BarRow No8  = new("no8",  RebarStandard.A615,    None,                    25.400, 510.0,  3.973);
    public static readonly BarRow No9  = new("no9",  RebarStandard.A615,    None,                    28.651, 645.0,  5.060);
    public static readonly BarRow No10 = new("no10", RebarStandard.A615,    None,                    32.258, 819.0,  6.404);
    public static readonly BarRow No11 = new("no11", RebarStandard.A615,    None,                    35.814, 1006.0, 7.907);
    public static readonly BarRow No14 = new("no14", RebarStandard.A615,    None,                    43.002, 1452.0, 11.380);
    public static readonly BarRow No18 = new("no18", RebarStandard.A615,    None,                    57.328, 2581.0, 20.240);
    public static readonly BarRow M10  = new("10m",  RebarStandard.G30,     None,                    11.300, 100.0,  0.785);
    public static readonly BarRow M15  = new("15m",  RebarStandard.G30,     None,                    16.000, 200.0,  1.570);
    public static readonly BarRow M20  = new("20m",  RebarStandard.G30,     None,                    19.500, 300.0,  2.355);
    public static readonly BarRow M25  = new("25m",  RebarStandard.G30,     None,                    25.200, 500.0,  3.925);
    public static readonly BarRow M30  = new("30m",  RebarStandard.G30,     None,                    29.900, 700.0,  5.495);
    public static readonly BarRow M35  = new("35m",  RebarStandard.G30,     None,                    35.700, 1000.0, 7.850);
    public static readonly BarRow M45  = new("45m",  RebarStandard.G30,     None,                    43.700, 1500.0, 11.775);
    public static readonly BarRow M55  = new("55m",  RebarStandard.G30,     None,                    56.400, 2500.0, 19.625);
    public static readonly BarRow H6   = new("h6",   RebarStandard.En10080, Some(BarDiameter.D6),     6.000, 28.3,   0.222);
    public static readonly BarRow H8   = new("h8",   RebarStandard.En10080, Some(BarDiameter.D8),     8.000, 50.3,   0.395);
    public static readonly BarRow H10  = new("h10",  RebarStandard.En10080, Some(BarDiameter.D10),   10.000, 78.5,   0.617);
    public static readonly BarRow H12  = new("h12",  RebarStandard.En10080, Some(BarDiameter.D12),   12.000, 113.0,  0.888);
    public static readonly BarRow H14  = new("h14",  RebarStandard.En10080, Some(BarDiameter.D14),   14.000, 154.0,  1.210);
    public static readonly BarRow H16  = new("h16",  RebarStandard.En10080, Some(BarDiameter.D16),   16.000, 201.0,  1.580);
    public static readonly BarRow H20  = new("h20",  RebarStandard.En10080, Some(BarDiameter.D20),   20.000, 314.0,  2.470);
    public static readonly BarRow H25  = new("h25",  RebarStandard.En10080, Some(BarDiameter.D25),   25.000, 491.0,  3.850);
    public static readonly BarRow H28  = new("h28",  RebarStandard.En10080, Some(BarDiameter.D28),   28.000, 616.0,  4.840);
    public static readonly BarRow H32  = new("h32",  RebarStandard.En10080, Some(BarDiameter.D32),   32.000, 804.0,  6.310);
    public static readonly BarRow H40  = new("h40",  RebarStandard.En10080, Some(BarDiameter.D40),   40.000, 1257.0, 9.860);
    public static readonly BarRow H50  = new("h50",  RebarStandard.En10080, Some(BarDiameter.D50),   50.000, 1964.0, 15.420);
    public static readonly ImmutableArray<BarRow> Rows = [
        No3, No4, No5, No6, No7, No8, No9, No10, No11, No14, No18,
        M10, M15, M20, M25, M30, M35, M45, M55,
        H6, H8, H10, H12, H14, H16, H20, H25, H28, H32, H40, H50];
}

public static class Ducts {
    public static readonly ConduitRow CorrugatedSteel   = new("duct-corrugated-steel",   TendonConduitKind.CorrugatedSteel,   None);
    public static readonly ConduitRow CorrugatedPlastic = new("duct-corrugated-plastic", TendonConduitKind.CorrugatedPlastic, None);
    public static readonly ImmutableArray<ConduitRow> Rows = [CorrugatedSteel, CorrugatedPlastic];
}

public static class ShapeCodes {
    public static readonly ShapeCodeRow Straight         = new("00", 1, false);
    public static readonly ShapeCodeRow LBar             = new("11", 2, false);
    public static readonly ShapeCodeRow SemicircularHook = new("12", 2, false);
    public static readonly ShapeCodeRow AngledHook       = new("13", 3, false);
    public static readonly ShapeCodeRow ClosedLink       = new("51", 4, true);

    public static readonly ImmutableArray<ShapeCodeRow> Rows = [
        Straight, new("01", 1, false), LBar, SemicircularHook, AngledHook,
        new("14", 2, false), new("15", 2, false), new("21", 3, false), new("22", 4, false), new("23", 3, false),
        new("24", 3, false), new("25", 3, false), new("26", 3, false), new("27", 3, false), new("28", 3, false),
        new("29", 3, false), new("31", 4, false), new("32", 4, false), new("33", 3, true),  new("34", 4, false),
        new("35", 4, false), new("36", 4, false), new("41", 5, false), new("44", 5, false), new("46", 5, false),
        new("47", 4, true),  new("48", 4, true),  ClosedLink,          new("52", 4, true),  new("56", 5, false),
        new("63", 5, true),  new("64", 6, false), new("67", 1, false), new("75", 2, true),  new("77", 1, true),
        new("98", 5, false), new("99", 0, false)];
    public static readonly FrozenDictionary<string, ShapeCodeRow> ByKey = Rows.ToFrozenDictionary(static r => r.Key, StringComparer.Ordinal);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarHook {
    public static readonly RebarHook Ninety        = new("90",  bendDegrees: 90.0,  extensionFactor: 12.0, minExtensionMm: 0.0,  shape: ShapeCodes.LBar);
    public static readonly RebarHook OneThirtyFive = new("135", bendDegrees: 135.0, extensionFactor: 6.0,  minExtensionMm: 75.0, shape: ShapeCodes.AngledHook);
    public static readonly RebarHook OneEighty     = new("180", bendDegrees: 180.0, extensionFactor: 4.0,  minExtensionMm: 65.0, shape: ShapeCodes.SemicircularHook);
    public double BendDegrees { get; }
    public double ExtensionFactor { get; }
    public double MinExtensionMm { get; }
    public ShapeCodeRow Shape { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RebarSchedule {
    const double RibHeightRatio = 0.05;
    const double RibSpacingRatio = 0.7;
    const double RiblessGapFraction = 0.125;
    const double FlankInclinationDeg = 45.0;

    static readonly Seq<SizeLadderRow> RelativeRibArea = SizeLadder.Of((6.0, 0.035), (12.0, 0.040), (double.PositiveInfinity, 0.056));
    static readonly Seq<SizeLadderRow> MandrelFactor   = SizeLadder.Of((16.0, 4.0), (double.PositiveInfinity, 7.0));

    public static Option<RebarRibGeometry> Ribs(BarRow bar, RebarSurface surface, RibPattern pattern) =>
        surface.Ribbed
            ? Some(new RebarRibGeometry(
                TransverseRibHeightMm:    RibHeightRatio * bar.NominalDiameterMm,
                TransverseRibSpacingMm:   RibSpacingRatio * bar.NominalDiameterMm,
                LongitudinalRibHeightMm:  RibHeightRatio * bar.NominalDiameterMm,
                FlankInclinationDeg:      FlankInclinationDeg,
                RibInclinationDeg:        pattern.InclinationDeg,
                RelativeRibArea:          SizeLadder.At(RelativeRibArea, bar.NominalDiameterMm),
                RiblessPerimeterFraction: RiblessGapFraction,
                Pattern:                  pattern))
            : None;

    public static Fin<RebarBend> StandardHook(BarRow bar, RebarUsage usage, HookKind kind, RebarHook hook, Op key) =>
        guard(usage.Stirrup == (kind != HookKind.Development),
                new KernelFault.InvalidValue(nameof(kind), "a hook kind matching bar usage", Some(key)))
            .ToFin()
            .Map(_ => new RebarBend(
                hook.BendDegrees,
                kind.MinInsideBendFactor(bar.NominalDiameterMm) * bar.NominalDiameterMm,
                Math.Max(hook.ExtensionFactor * bar.NominalDiameterMm, hook.MinExtensionMm),
                SizeLadder.At(MandrelFactor, bar.NominalDiameterMm) * bar.NominalDiameterMm,
                usage.Stirrup ? ShapeCodes.ClosedLink : hook.Shape));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ForceBasis {
    public static readonly ForceBasis Characteristic = new("characteristic",
        projectKn: static (bar, arm) => arm.CharacteristicYieldMpa.Map(yield => yield * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnUltimate = new("en-ultimate",
        projectKn: static (bar, arm) => arm.En.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    [UseDelegateFromConstructor] private partial Option<double> ProjectKn(BarRow bar, GradeProperties.Rebar arm);

    public Fin<double> ForceKn(BarRow bar, MaterialGrade grade, Op key) =>
        from arm in grade.RebarArm.ToFin(new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Reinforcement))
        from admitted in guard(arm.Rolls(bar), new KernelFault.InvalidValue(nameof(bar), "a bar rolled by its grade system", Some(key)))
        from force in ProjectKn(bar, arm).ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Reinforcement, typeof(ForceBasis)))
        select force;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TendonBasis {
    public static readonly TendonBasis Ultimate = new("ultimate",
        projectKn: static (arm, _) => Some(arm.UltimateMpa * arm.AreaMm2 * 1e-3));
    public static readonly TendonBasis Yield = new("yield",
        projectKn: static (arm, _) => Some(arm.ProofMpa * arm.AreaMm2 * 1e-3));
    public static readonly TendonBasis Jacking = new("jacking",
        projectKn: static (arm, authority) => authority.JackingCeilingMpa(arm.UltimateMpa, arm.ProofMpa).Map(stress => stress * arm.AreaMm2 * 1e-3));
    [UseDelegateFromConstructor] private partial Option<double> ProjectKn(GradeProperties.Strand arm, ComponentAuthority authority);

    public Fin<double> ForceKn(MaterialGrade strand, Op key) =>
        from arm in strand.StrandArm.ToFin(new ComponentFault.GradeBodyMissing(key, strand, ComponentFamily.Reinforcement))
        from force in ProjectKn(arm, strand.Authority).ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Reinforcement, typeof(TendonBasis)))
        select force;
}

public static class TendonForce {
    public static double RelaxationLoss1000hKn(GradeProperties.Strand strand, double initialForceKn) =>
        strand.Relaxation.Rho1000Percent / 100.0 * initialForceKn;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Union]
public abstract partial record ReinforcementRow {
    private ReinforcementRow() { }
    public sealed record Bar(BarRow Size, MaterialGrade Grade, RebarUsage Usage, RebarSurface Surface, Option<(HookKind Kind, RebarHook Hook)> Bend) : ReinforcementRow;
    public sealed record Tendon(MaterialGrade Strand, Option<TendonAssembly> Post) : ReinforcementRow;

    public string Designation => Switch(
        bar: static row => row.Usage == RebarUsage.Main
            ? $"reinforcement.rebar-{row.Size.Key}-{row.Grade.Key}"
            : $"reinforcement.rebar-{row.Size.Key}-{row.Grade.Key}-{row.Usage.Key}",
        tendon: static row => row.Post.Match(
            Some: post => $"reinforcement.pt-{row.Strand.Key}-{post.Profile.Key}",
            None: () => $"reinforcement.{row.Strand.Key}"));

    public MaterialGrade Grade => Switch(bar: static row => row.Grade, tendon: static row => row.Strand);

    public IfcBinding Ifc => Switch(
        bar: static row => IfcBinding.Of("IfcReinforcingBar", row.Usage.IfcPredefinedType),
        tendon: static _ => IfcBinding.Of("IfcTendon", "STRAND"));

    public MaterialId Appearance => Switch(
        bar: static row => row.Grade.Appearance.IfNone(row.Grade.Substance),
        tendon: static _ => MaterialId.Of("metal.steel"));

    public Option<(double DiameterMm, double AreaMm2)> Section => Switch(
        bar: static row => Some((row.Size.NominalDiameterMm, row.Size.NominalAreaMm2)),
        tendon: static row => row.Strand.StrandArm.Map(static arm => (arm.DiameterMm, arm.AreaMm2)));

    public Validation<Error, Unit> Coherence(Op key) => Switch(
        bar: row => (
            Prove(row.Grade.Family == ComponentFamily.Reinforcement, new ComponentFault.GradeFamilyMismatch(key, row.Grade, ComponentFamily.Reinforcement)),
            Prove(row.Grade.RebarArm.IsSome, new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Reinforcement)),
            Prove(row.Grade.Admits(row.Size), new KernelFault.InvalidValue(nameof(row.Size), "a size admitted by its grade system", Some(key))))
            .Apply(static (_, _, _) => unit).As(),
        tendon: row => (
            Prove(row.Strand.Family == ComponentFamily.Reinforcement, new ComponentFault.GradeFamilyMismatch(key, row.Strand, ComponentFamily.Reinforcement)),
            Prove(row.Strand.StrandArm.IsSome, new ComponentFault.GradeBodyMissing(key, row.Strand, ComponentFamily.Reinforcement)))
            .Apply(static (_, _) => unit).As());

    static Validation<Error, Unit> Prove(bool held, Error fault) => guard(held, fault).ToValidation();
}

public static class ReinforcementDetail {
    public static Fin<PropertyBag> Of(ReinforcementRow row, EvidenceGrade source, Op key) =>
        from section in row.Section.ToFin(new KernelFault.InvalidValue(nameof(row.Section), "a reinforcement cross-section", Some(key)))
        from joint in ComponentDetail.Joint("Cast", key)
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, section.DiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, section.AreaMm2 * 1e-6)
        from extension in Extension(row, key)
        select ComponentDetail.RealizationRows([joint, ComponentDetail.Sourced(source), diameter, area, .. extension]);

    static Fin<Seq<(PropertyName, PropertyValue)>> Extension(ReinforcementRow row, Op key) => row.Switch(
        bar: item =>
            from bend in item.Bend.Match(
                Some: policy => RebarSchedule.StandardHook(item.Size, item.Usage, policy.Kind, policy.Hook, key).Map(Some),
                None: static () => Fin.Succ(Option<RebarBend>.None))
            from schedule in bend.Match(Some: b => BendRow(b).Map(Some), None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
            select Seq(
                ComponentDetail.Token(DetailSchema.BarType, item.Usage.IfcPredefinedType),
                ComponentDetail.Token(DetailSchema.BendShapeCode, bend.Map(static b => b.Shape.Key).IfNone(ShapeCodes.Straight.Key)))
                + schedule.ToSeq(),
        tendon: item => item.Post.Match(
            Some: post =>
                from duct in post.Duct.InnerDiameterMm.Match(
                    Some: mm => ComponentDetail.Measured(DetailSchema.DuctDiameter, Dimension.LengthDim, mm * 1e-3).Map(Some),
                    None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
                select Seq(
                    ComponentDetail.Token(DetailSchema.AnchorageType, post.Anchorage.Key),
                    ComponentDetail.Token(DetailSchema.TendonProfile, post.Profile.Key))
                    + duct.ToSeq(),
            None: static () => Fin.Succ(Seq<(PropertyName, PropertyValue)>())));

    static Fin<(PropertyName, PropertyValue)> BendRow(RebarBend bend) =>
        from inside in Si(bend.InsideBendDiameterMm)
        from extension in Si(bend.HookExtensionMm)
        from mandrel in Si(bend.MandrelDiameterMm)
        select (DetailSchema.BendSchedule, (PropertyValue)new PropertyValue.Complex("bend-schedule", Map(
            (DetailSchema.BendAngle, (PropertyValue)new PropertyValue.Text($"{bend.BendDegrees:R}")),
            (DetailSchema.InsideBendDiameter, inside),
            (DetailSchema.HookExtension, extension),
            (DetailSchema.MandrelDiameter, mandrel))));

    static Fin<PropertyValue> Si(double mm) =>
        MeasureValue.OfSi(Dimension.LengthDim, mm * 1e-3).Map(static value => (PropertyValue)new PropertyValue.Measure(value));
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class ReinforcementSeed {
    static readonly Option<(HookKind, RebarHook)> DevelopmentHook = Some((HookKind.Development, RebarHook.Ninety));
    static readonly Option<(HookKind, RebarHook)> SeismicHook = Some((HookKind.Seismic, RebarHook.OneThirtyFive));
    static readonly Option<(HookKind, RebarHook)> TieHook = Some((HookKind.StirrupTie, RebarHook.OneThirtyFive));

    static ReinforcementRow Bar(BarRow size, MaterialGrade grade, RebarUsage usage, RebarSurface surface, Option<(HookKind, RebarHook)> bend = default) =>
        new ReinforcementRow.Bar(size, grade, usage, surface, bend);

    static ReinforcementRow Pre(MaterialGrade strand) => new ReinforcementRow.Tendon(strand, None);

    static ReinforcementRow Post(MaterialGrade strand, AnchorageKind anchorage, ConduitRow duct, TendonProfileKind profile) =>
        new ReinforcementRow.Tendon(strand, Some(new TendonAssembly(anchorage, duct, profile)));

    public static readonly Seq<ReinforcementRow> Roster = Seq(
        Bar(Bars.No3,  MaterialGrade.Gr40,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No3,  MaterialGrade.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.Main,        RebarSurface.Textured, DevelopmentHook),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.Ligature,    RebarSurface.Textured, TieHook),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.Shear,       RebarSurface.Textured, TieHook),
        Bar(Bars.No5,  MaterialGrade.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No5,  MaterialGrade.Gr60,   RebarUsage.Edge,        RebarSurface.Textured, DevelopmentHook),
        Bar(Bars.No6,  MaterialGrade.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No6,  MaterialGrade.Gr60,   RebarUsage.Ring,        RebarSurface.Textured, TieHook),
        Bar(Bars.No7,  MaterialGrade.Gr75,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No8,  MaterialGrade.Gr75,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No8,  MaterialGrade.Gr75,   RebarUsage.Anchoring,   RebarSurface.Textured, DevelopmentHook),
        Bar(Bars.No9,  MaterialGrade.Gr80,   RebarUsage.Main,        RebarSurface.Textured, SeismicHook),
        Bar(Bars.No11, MaterialGrade.Gr80,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No18, MaterialGrade.Gr80,   RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No5,  MaterialGrade.Gr60W,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No5,  MaterialGrade.Gr60W,  RebarUsage.Punching,    RebarSurface.Textured),
        Bar(Bars.No8,  MaterialGrade.Gr80W,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.No3,  MaterialGrade.Gr60,   RebarUsage.Spacer,      RebarSurface.Plain),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.Stud,        RebarSurface.Textured),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.UserDefined, RebarSurface.Textured),
        Bar(Bars.No4,  MaterialGrade.Gr60,   RebarUsage.NotDefined,  RebarSurface.Plain),
        Bar(Bars.M10,  MaterialGrade.Gr400W, RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.M10,  MaterialGrade.Gr400W, RebarUsage.Ligature,    RebarSurface.Textured, TieHook),
        Bar(Bars.M15,  MaterialGrade.Gr400W, RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.M25,  MaterialGrade.Gr500W, RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.M35,  MaterialGrade.Gr500W, RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.H8,   MaterialGrade.B500A,  RebarUsage.Ligature,    RebarSurface.Plain,    TieHook),
        Bar(Bars.H12,  MaterialGrade.B500B,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.H14,  MaterialGrade.B500B,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.H16,  MaterialGrade.B500C,  RebarUsage.Main,        RebarSurface.Textured, SeismicHook),
        Bar(Bars.H25,  MaterialGrade.B500C,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.H32,  MaterialGrade.B500C,  RebarUsage.Main,        RebarSurface.Textured),
        Bar(Bars.H40,  MaterialGrade.B500C,  RebarUsage.Main,        RebarSurface.Textured),
        Pre(MaterialGrade.S13Gr1725),
        Pre(MaterialGrade.S13Gr1860),
        Pre(MaterialGrade.S15Gr1860),
        Pre(MaterialGrade.Y1860S7D125),
        Pre(MaterialGrade.Y1860S7D157),
        Post(MaterialGrade.S15Gr1860,  AnchorageKind.MonoLive,  Ducts.CorrugatedSteel,   TendonProfileKind.Parabolic),
        Post(MaterialGrade.Y1860S7D157, AnchorageKind.MultiLive, Ducts.CorrugatedPlastic, TendonProfileKind.Harped));

    static readonly EvidenceGrade Tabulated = EvidenceGrade.Catalogue;

    public static readonly SeedLaw<ReinforcementRow> Law = SeedLaw<ReinforcementRow>.Of(
        family: ComponentFamily.Reinforcement,
        designation: static row => row.Designation,
        coherence: static (row, key) => row.Coherence(key),
        profile: ProfileOf,
        substance: static row => row.Grade.Substance,
        source: static _ => Tabulated,
        standard: static row => row.Grade.Receipt,
        detail: Some<Func<ReinforcementRow, SectionProfile, Op, Fin<PropertyBag>>>(
            static (row, _, key) => ReinforcementDetail.Of(row, Tabulated, key)),
        appearance: static row => row.Appearance,
        ifc: static row => row.Ifc);

    static Fin<SectionProfile> ProfileOf(ReinforcementRow row, Op key) =>
        row.Section
            .ToFin(new KernelFault.InvalidValue(nameof(row.Section), "a reinforcement cross-section", Some(key)))
            .Bind(section => SectionProfile.Circle.Of(section.DiameterMm, key));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the reinforced-concrete receipt over the `VividOrange.Sections` `IConcreteSection` AND the held `ConcreteSectionProperties` transformed-section carrier; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`/`Placed`) collapsing the four `VividOrange.Sections` layout-engine constructors plus the loose-bar ingress; `EnGrade` the EN-grade admission boundary lowering the `VividOrange.Materials` derivation throws onto the typed `ComponentFault` rail; `RcSectionBuilder` the one assembler minting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`) · `FaceSpacing` (max-spacing bars on a face) · `PerimeterCount` (n bars round the whole section — `PerimeterReinforcementLayer`, no face) · `PerimeterSpacing` · `Placed` (one bar at an explicit Y-Z section-plane station — `LongitudinalReinforcement`, the engine ingress the four rule-driven cases structurally cannot express)} — the face cases over the `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`; NO `Perimeter` member — perimeter distribution is the separate engine, never a face value); a stirrup is the `Link` promoted once from the same bar mint the layouts use.
- Entry: `RcSectionBuilder.Of(concrete, concreteGrade, barGrade, link, layout, coverMm, annex, key)` lowers grades through `EnGrade`, proves the link and every layout bar through `MaterialGrade.Admits`, builds the profile-faithful `ConcreteSection`, and captures its `ConcreteSectionProperties`; absent grade bodies rail `GradeBodyMissing`.
- Packages: VividOrange.Sections (`ConcreteSection`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `MinimumReinforcementSpacing` with the settable `MaximumAggregateSize`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` throws trapped here), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` — the transformed-section columns the receipt reads), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial` + their factories; the `ArgumentException`/`MissingNationalAnnexException` throws trapped here), VividOrange.Standards (`En1992`/`NationalAnnex`), VividOrange.Profiles (`IProfile` via `component#SECTION_SOLVER` `ProfileOf`), UnitsNet (`Length` at the edge), Rasm.Domain (`Op.Catch` preserving exact exceptional errors), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Fin`/`Seq`).
- Growth: a new rebar arrangement is one `RebarLayout` case binding its engine ingress through `RebarPlacement` (the generated `Switch` breaks every dispatch site at compile time); a new tendon force basis is one `TendonBasis` row and a new prestressing authority one `ComponentAuthority.Jacking` pair, never a branch; a new transformed-section read is one projection on the `RcSection` receipt over the held carrier; a new constitutive concrete law is a `capacity#SECTION_CAPACITY` concern over the same `IConcreteSection` — never a per-arrangement builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a re-summed bar area where `ConcreteSectionProperties` carries it.
- Boundary: `RcSectionBuilder.Of` admits the `VividOrange` throwing surface once — documented grade refusals lower to cause-bearing `GradeDerivation`, documented material/profile construction refusals to `SectionConstruction`, and unknown throws remain exact. Missing capacity observations retain their direct semantic leaves.
- Boundary: `Of` admits ANY `Component` as its concrete outline because `SectionSolver.ProfileOf` switches the closed `SectionProfile` axis regardless of family, PROFILE-FAITHFUL — a circular drilled shaft feeds its true `ICircle`, a trapezoidal member its integrated perimeter, a `cmu#CMU_FAMILY` grouted unit its gross rectangle — so the cmu unit admits as the reinforced-masonry concrete input through this ONE boundary and no cmu-specific builder exists. The RC section is NOT a `Component`: a `Component` is one discrete bar in the schedule, the `RcSection` the populated member it reinforces, and the two meet at the `BarRow`/`MaterialGrade` currencies this page owns. The full elastic stress state and the N-M-M hull stay the `capacity#SECTION_CAPACITY` owner's over the SAME `IConcreteSection`.

```csharp signature
using VividOrange.Sections;
using VividOrange.Sections.Reinforcement;
using VividOrange.Geometry;
using VividOrange.Sections.SectionProperties;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Profiles;
using VividOrange.Standards.Eurocode;
using UnitsNet;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RebarLayout {
    private RebarLayout() { }
    public sealed record FaceCount(SectionFace Face, BarRow Bar, int Count) : RebarLayout;
    public sealed record FaceSpacing(SectionFace Face, BarRow Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record PerimeterCount(BarRow Bar, int Count) : RebarLayout;
    public sealed record PerimeterSpacing(BarRow Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record Placed(BarRow Bar, double YMm, double ZMm) : RebarLayout;

    public BarRow Bar => Switch(
        faceCount:        static c => c.Bar,
        faceSpacing:      static s => s.Bar,
        perimeterCount:   static c => c.Bar,
        perimeterSpacing: static s => s.Bar,
        placed:           static p => p.Bar);

    public Option<SectionFace> Face => Switch(
        faceCount:        static c => Some(c.Face),
        faceSpacing:      static s => Some(s.Face),
        perimeterCount:   static _ => Option<SectionFace>.None,
        perimeterSpacing: static _ => Option<SectionFace>.None,
        placed:           static _ => Option<SectionFace>.None);
}

[Union<IReinforcementLayer, ILongitudinalReinforcement>(T1Name = "Layer", T2Name = "Placed")]
public readonly partial struct RebarPlacement;

public sealed record RcSection(
    IConcreteSection Section, ConcreteSectionProperties Properties,
    EnConcreteMaterial Concrete, EnRebarMaterial Rebar, Option<double> LinkYieldMpa, double CoverMm,
    Component ConcreteProfile, FrozenSet<SectionFace> BarredFaces) {

    public double GrossSteelAreaMm2 => Properties.TotalReinforcementArea.SquareMillimeters;
    public double ConcreteAreaMm2 => Properties.ConcreteArea.SquareMillimeters;
    public double ReinforcementRatio => Properties.GeometricReinforcementRatio.DecimalFractions;
    public double ShearLinkAreaMm2 => Properties.CrossSectionalShearReinforcementArea.SquareMillimeters;
    public double ReinforcementInertiaYyMm4 => Properties.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth;
    public double ReinforcementInertiaZzMm4 => Properties.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth;

    public Option<double> EffectiveDepthMm(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.EffectiveDepth(face).Millimeters) : None;
    public Option<double> FaceSteelAreaMm2(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.ReinforcementArea(face).SquareMillimeters) : None;

    public IReadOnlyList<ILongitudinalReinforcement> PlacedBars => Section.Rebars;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RcCapacityIntent {
    private RcCapacityIntent() { }
    public sealed record Hull(DiagramResolution Resolution) : RcCapacityIntent;
    public sealed record Elastic : RcCapacityIntent;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        key.Catch(
            () => Fin.Succ(new EnConcreteMaterial(grade, annex)),
            cause => GradeRefusal(key, cause));

    public static Fin<EnRebarMaterial> Rebar(MaterialGrade grade, NationalAnnex annex, Op key) =>
        grade.RebarArm.Bind(static arm => arm.En).Match(
            Some: g => key.Catch(
                () => Fin.Succ(new EnRebarMaterial(g, annex)),
                cause => GradeRefusal(key, cause)),
            None: () => Fin.Fail<EnRebarMaterial>(new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Reinforcement)));

    internal static Option<ComponentFault.GradeDerivation> GradeRefusal(Op key, Error cause) =>
        cause.Exception.Case is ArgumentException or MissingNationalAnnexException or InvalidSteelSpecificationException
            ? Some(new ComponentFault.GradeDerivation(key, cause))
            : None;
}

public static class RcSectionBuilder {
    public static Fin<RcSection> Of(Component concrete, EnConcreteGrade concreteGrade, MaterialGrade barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from cover in guard(double.IsFinite(coverMm) && coverMm >= 0.0,
            new KernelFault.OutOfRange(nameof(coverMm), coverMm, "finite and non-negative", Some(key)))
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade, annex, key)
        from arm in barGrade.RebarArm.ToFin(new ComponentFault.GradeBodyMissing(key, barGrade, ComponentFamily.Reinforcement))
        from linkAdmitted in guard(barGrade.Admits(link), new KernelFault.InvalidValue(nameof(link), "a link admitted by its grade system", Some(key)))
        from admittedLayout in layout.Traverse(item => ValidateLayout(item, barGrade, key)).As()
        from profile in SectionSolver.ProfileOf(concrete.Profile, key)
        from built in key.Catch(
            () => Fin.Succ(Build(profile, concreteMaterial, rebarMaterial, link, admittedLayout, coverMm)),
            cause => cause.Exception.Case is InvalidMaterialTypeException or InvalidProfileTypeException
                ? Some(new ComponentFault.SectionConstruction(key, cause))
                : None)
        from properties in key.Catch(() => { ConcreteSectionProperties p = new(built); _ = p.TotalReinforcementArea; return Fin.Succ(p); })
        select new RcSection(built, properties, concreteMaterial, rebarMaterial, arm.CharacteristicYieldMpa, coverMm, concrete,
            admittedLayout.Choose(static item => item.Face).ToFrozenSet());

    static Fin<RebarLayout> ValidateLayout(RebarLayout layout, MaterialGrade grade, Op key) =>
        (guard(grade.Admits(layout.Bar), new KernelFault.InvalidValue(nameof(layout.Bar), "a layout bar admitted by its grade system", Some(key))).ToValidation(),
         Shape(layout, key))
            .Apply(static (_, _) => unit).As().ToFin().Map(_ => layout);

    static Validation<Error, Unit> Shape(RebarLayout layout, Op key) => layout.Switch(
        faceCount: item => Prove(item.Count > 0, new KernelFault.OutOfRange(nameof(item.Count), item.Count, "positive", Some(key))),
        faceSpacing: item => Prove(double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0,
            new KernelFault.OutOfRange(nameof(item.MaxSpacingMm), item.MaxSpacingMm, "finite and positive", Some(key))),
        perimeterCount: item => Prove(item.Count > 0, new KernelFault.OutOfRange(nameof(item.Count), item.Count, "positive", Some(key))),
        perimeterSpacing: item => Prove(double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0,
            new KernelFault.OutOfRange(nameof(item.MaxSpacingMm), item.MaxSpacingMm, "finite and positive", Some(key))),
        placed: item => Prove(double.IsFinite(item.YMm) && double.IsFinite(item.ZMm),
            new KernelFault.InvalidValue(nameof(RebarLayout.Placed), "finite section-plane coordinates", Some(key))));

    static Validation<Error, Unit> Prove(bool held, Error fault) => guard(held, fault).ToValidation();

    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarRow link, Seq<RebarLayout> layout, double coverMm) {
        Seq<RebarPlacement> placements = layout.Map(l => PlacementOf(l, rebar));
        ConcreteSection section = new(profile, concrete, new Link(RebarOf(link, rebar)), Length.FromMillimeters(coverMm),
            [.. placements.Filter(static p => p.IsPlaced).Map(static p => p.AsPlaced)]);
        placements.Filter(static p => p.IsLayer).Iter(p => section.AddRebarLayer(p.AsLayer));
        return section;
    }

    static Rebar RebarOf(BarRow bar, EnRebarMaterial rebar) =>
        bar.CatalogueKey.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(bar.NominalDiameterMm)));

    static RebarPlacement PlacementOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => RebarPlacement.CreateLayer(new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count)),
        faceSpacing:      s => RebarPlacement.CreateLayer(new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        perimeterCount:   c => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count)),
        perimeterSpacing: s => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        placed:           p => RebarPlacement.CreatePlaced(new LongitudinalReinforcement(
                                   RebarOf(p.Bar, rebar), new LocalPoint2d(Length.FromMillimeters(p.YMm), Length.FromMillimeters(p.ZMm)))));

    public static Fin<SectionCapacity> Capacity(
        Component concrete, EnConcreteGrade concreteGrade, MaterialGrade barGrade, BarRow link,
        Seq<RebarLayout> layout, double coverMm, RcCapacityIntent intent, CapacityPlacement placement, Op key) =>
        from section in Of(concrete, concreteGrade, barGrade, link, layout, coverMm, placement.Annex, key)
        from capacity in SectionCapacity.Resolve(Request(concrete.Designation, section, intent), key)
        select capacity;

    static CapacityBuild Request(ComponentId subject, RcSection section, RcCapacityIntent intent) => intent.Switch<CapacityBuild>(
        hull: h => new CapacityBuild.Hull(subject, section, h.Resolution),
        elastic: _ => new CapacityBuild.Elastic(subject, section));

    public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key) =>
        from aggregate in guard(double.IsFinite(maxAggregateMm) && maxAggregateMm > 0.0,
            new KernelFault.OutOfRange(nameof(maxAggregateMm), maxAggregateMm, "finite and positive", Some(key)))
        from spacing in key.Catch(() => Fin.Succ(new MinimumReinforcementSpacing(annex) { MaximumAggregateSize = Length.FromMillimeters(maxAggregateMm) }
            .GetMinimumReinforcementSpacing(Length.FromMillimeters(bar.NominalDiameterMm)).Millimeters))
        select spacing;
}
```

## [04]-[RESEARCH]

(none)
