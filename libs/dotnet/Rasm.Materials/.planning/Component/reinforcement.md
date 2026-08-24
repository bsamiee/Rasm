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
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

// Every family page declares in the ONE Rasm.Materials.Component namespace; component#COMPONENT_OWNER binds
// ReinforcementSeed.Roster/Law/Capacity by bare name on the ComponentFamily.Reinforcement policy row.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The spec body's STANDARD IDENTITY — weldability and the SIZE SYSTEM whose BarRow rows its mills roll. The issuing
// authority is the MaterialGrade row's own ComponentAuthority column, so this row carries none: the free text, the
// body column, and the region column were one fact spelled three ways beside a fourth on the grade. Rolls is a
// DELEGATE COLUMN rather than an equality branch — A706 bars roll at the A615 imperial sizes, and stating that as a
// row value keeps the size correspondence with the vocabulary that owns it while the deferred evaluation dissolves
// the static self-reference an eagerly-read column would have needed.
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

// The bar's STRUCTURAL ROLE — the FULL verified 11-member IfcReinforcingBarTypeEnum (GeometryGym 25.7.30) so the seam
// PredefinedType is a row read, never a widened-later subset. Stirrup routes the RcSection link-vs-longitudinal
// placement AND the StandardHook closed-link ShapeCode override.
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
    public static readonly RebarUsage Stud        = new("stud",        ifcPredefinedType: "STUD",        stirrup: false);   // cast-in headed-stud reinforcement — distinct from the welded shear connector joint#JOINT_FAMILY owns
    public static readonly RebarUsage UserDefined = new("userdefined", ifcPredefinedType: "USERDEFINED", stirrup: false);
    public static readonly RebarUsage NotDefined  = new("notdefined",  ifcPredefinedType: "NOTDEFINED",  stirrup: false);
    public string IfcPredefinedType { get; }
    public bool Stirrup { get; }
}

// The bond surface — the verified IfcReinforcingBarSurfaceEnum {PLAIN, TEXTURED}. Ribbed gates RebarSchedule.Ribs.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarSurface {
    public static readonly RebarSurface Textured = new("textured", ifcSurface: "TEXTURED", ribbed: true);    // deformed/ribbed — the hot-rolled default
    public static readonly RebarSurface Plain    = new("plain",    ifcSurface: "PLAIN",    ribbed: false);   // plain round — ties, spacers, smooth dowels
    public string IfcSurface { get; }
    public bool Ribbed { get; }
}

// The rib-deformation FORM — ISO 6935-2 §4.15 β between a transverse rib and the bar axis: parallel uniform-height
// 90°, modern hot-rolled crescent two-series 60°.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RibPattern {
    public static readonly RibPattern UniformHeight = new("uniform-height", inclinationDeg: 90.0);
    public static readonly RibPattern Crescent      = new("crescent",       inclinationDeg: 60.0);
    public double InclinationDeg { get; }
}

// The 1000 h relaxation certification class (ASTM A416 §9 low-relaxation / EN 10138 Class 1-2): Rho1000Percent the
// certified stress loss at 1000 h under 0.7·fpu initial stress — the loss-schedule anchor a prestress-loss fold scales.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RelaxationClass {
    public static readonly RelaxationClass LowRelaxation = new("low-relaxation", rho1000Percent: 2.5);   // A416 low-relaxation / EN 10138 Class 2
    public static readonly RelaxationClass Normal        = new("normal",         rho1000Percent: 8.0);   // EN 10138 Class 1 — the stress-relieved wire class
    public double Rho1000Percent { get; }
}

// The PT anchorage kind axis over the verified IfcTendonAnchor leaf set (GeometryGym 25.7.30) {COUPLER, FIXED_END,
// TENSIONING_END}: mono- and multi-strand live ends share the TENSIONING_END token and split on THIS axis, the dead
// end is FIXED_END, and the tendon coupler the anchor entity's own COUPLER. Every geometric column is ETA/vendor
// territory — the kind axis is what this page can close without inventing a plate size, and a certified product is a
// further row that lands its columns together with the reader that stamps them.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnchorageKind {
    public static readonly AnchorageKind MonoLive  = new("mono-live",  ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind MultiLive = new("multi-live", ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind DeadEnd   = new("dead-end",   ifcPredefinedType: "FIXED_END");
    public static readonly AnchorageKind Coupler   = new("coupler",    ifcPredefinedType: "COUPLER");
    public string IfcPredefinedType { get; }
}

// The tendon conduit kind axis over the verified IfcTendonConduit leaf set {COUPLER, DIABOLO, DUCT, GROUTING_DUCT,
// TRUMPET}: the two duct BODIES (corrugated galvanized steel, corrugated HDPE plastic) both ride the DUCT token and
// split on this axis's own body discriminant; the accessory kinds carry their exact tokens.
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

// The drape-profile vocabulary the DetailSchema.TendonProfile row stamps — the tendon geometry CLASS a layout
// declares (the drape ordinates themselves are member-run geometry outside Materials).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TendonProfileKind {
    public static readonly TendonProfileKind Straight  = new("straight");
    public static readonly TendonProfileKind Parabolic = new("parabolic");
    public static readonly TendonProfileKind Harped    = new("harped");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The published SIZE-BANDED ladder: a diameter ceiling and the coefficient the standard prints for every bar at or
// below it. Three inline ternary ladders — the ACI 318 §25.3 inside-bend multiple, the ISO/fib relative rib area, and
// the EN 1992 §8.3 mandrel multiple — were three spellings of ONE lookup with their thresholds and values interleaved
// in expression positions no reader can diff against a printed table. As rows the ladder IS the table, and a revised
// band is one row. The top row's ceiling is unbounded, so the fold is total.
public readonly record struct SizeLadderRow(double CeilingMm, double Value);

public static class SizeLadder {
    public static Seq<SizeLadderRow> Of(params (double CeilingMm, double Value)[] rows) =>
        toSeq(rows).Map(static row => new SizeLadderRow(row.CeilingMm, row.Value));

    public static double At(Seq<SizeLadderRow> ladder, double diameterMm) =>
        ladder.Find(row => diameterMm <= row.CeilingMm).Map(static row => row.Value).IfNone(ladder[^1].Value);
}

// The ACI 318-19 §25.3 bend-table discriminant with the minimum inside-bend multiple (×d_b) as its own published
// ladder: development (Table 25.3.1) 6·d_b to 25.4 mm, 8 to 36 mm, 10 above; stirrup-tie (25.3.2) and seismic
// (25.3.4) 4·d_b to 16 mm, 6 above — so a #5 stirrup bends at 4·d_b, never the 6·d_b a development bar uses.
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

// The ONE nominal mass density every reinforcement weight derivation reads — declared once at page grain because it
// is one published material fact, not a per-row column two carriers would each get to spell differently.
public static class ReinforcementSteel {
    public const double DensityKgM3 = 7850.0;   // Catalogue: EN 10080 nominal mass density
}

// SEED_ROW_LAW row currency: CatalogueKey is VENDOR evidence (Some only for the EN H-series, the BarDiameter the
// Rebar(IMaterial, BarDiameter) ctor consumes; imperial/CSA None so the exact non-EN nominal feeds a raw Length);
// diameter, area, and weight all Catalogue, because every size in every rostered body PRINTS all three. Standard is
// the size-defining spec body, and it is the ONLY reason this row survives the MaterialGrade landing: a bar SIZE is
// not a grade, and the grade's own standard admits against this column.
public readonly record struct BarRow(string Key, RebarStandard Standard, Option<BarDiameter> CatalogueKey,
    double NominalDiameterMm, double NominalAreaMm2, double NominalWeightKgM);

// The BS 8666:2020 schedule shape row: Legs the straight-leg count (the A..F dimension letters the host shape-table
// lofts the polyline from), Link true for the closed-perimeter stirrup/circular/helix shapes. The cutting-length
// formula is the host shape-table's; this row carries the token the IfcReinforcingBar BendingShapeCode wire reads.
public readonly record struct ShapeCodeRow(string Key, int Legs, bool Link);

// The PT conduit row: the kind is CLOSED and the inner diameter is a CERTIFICATE SLOT — duct diameter series are
// ETA/vendor data no pack two-sources, so `Some` names a certificate or does not land, and the seeded assembly bag
// stamps DetailSchema.DuctDiameter only where it does. The retired wall-gauge column had no reader at either state.
public readonly record struct ConduitRow(string Key, TendonConduitKind Kind, Option<double> InnerDiameterMm);

// The ISO 6935-2 rib-deformation receipt: RelativeRibArea fR the GOVERNING bond invariant, the transverse/longitudinal
// rib form, the §4.14 flank inclination α, the §4.15 rib-to-axis inclination β, and the ASTM A615 §7.4 ribless-perimeter
// gap fraction Σf_i.
public readonly record struct RebarRibGeometry(
    double TransverseRibHeightMm,
    double TransverseRibSpacingMm,
    double LongitudinalRibHeightMm,
    double FlankInclinationDeg,
    double RibInclinationDeg,
    double RelativeRibArea,
    double RiblessPerimeterFraction,
    RibPattern Pattern);

// The host-neutral bend receipt: the ACI inside bend diameter, the floored straight extension, the EN 1992 §8.3
// mandrel (former) diameter, and the BS 8666 ShapeCodeRow — the two diameters distinct because the EN former rule
// (4·d / 7·d) and the ACI hook rule (4..10·d by HookKind × band) differ.
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm, double MandrelDiameterMm, ShapeCodeRow Shape);

// The POST-TENSIONED assembly a tendon row declares: the live/dead anchorage kind, the conduit product, and the drape
// class. A pre-tensioned strand carries none — the absence IS the modality, so no anchorage token, duct row, or
// drape class is ever fabricated for a strand cast straight into the bed.
public readonly record struct TendonAssembly(AnchorageKind Anchorage, ConduitRow Duct, TendonProfileKind Profile);

// The REBAR and STRAND arm physics, co-located with the family that owns them: component#MATERIAL_GRADE declares the
// columns and this page states what they mean. The CHARACTERISTIC YIELD has exactly ONE owner per row — an EN-bodied
// grade reads the registered f_yk off EnRebarFactory and the others read the column their own spec prints — so
// exactly one arm answers on every rostered row and neither source shadows the other.
public partial record GradeProperties {
    public sealed partial record Rebar {
        public Option<double> CharacteristicYieldMpa =>
            En.Map(static grade => EnRebarFactory.CreateLinearElastic(grade).Strength.Megapascals).IfNone(() => YieldMpa);
        public bool Weldable => Standard.Weldable;
        // The standard-consistency law: a grade admits only the bar rows its own body rolls.
        public bool Rolls(BarRow bar) => Standard.Rolls() == bar.Standard;
    }

    public sealed partial record Strand {
        // The printed yield-to-ultimate proof ratio applied to the printed ultimate — A416 low-relaxation
        // fpy = 0.90·fpu, EN Fp0,1/Fm = 0.88 off the printed force pair.
        public double ProofMpa => YieldRatio * UltimateMpa;
        // Defined: A·ρ, no printed twin kept.
        public double NominalWeightKgM => AreaMm2 * 1e-6 * ReinforcementSteel.DensityKgM3;
    }
}

// The reinforcement members of the one grade identity. Admits pairs the SIZE SYSTEM law with the arm read, so a
// strand, a steel section, or a timber class reaching a bar path answers false rather than matching an arm it does
// not carry; the two arm accessors are total projections over the closed payload that four consumers each would
// otherwise spell as a Switch.
public sealed partial class MaterialGrade {
    public Option<GradeProperties.Rebar> RebarArm => Columns is GradeProperties.Rebar arm ? Some(arm) : None;
    public Option<GradeProperties.Strand> StrandArm => Columns is GradeProperties.Strand arm ? Some(arm) : None;
    public bool Admits(BarRow bar) => RebarArm.Exists(arm => arm.Rolls(bar));
    // The regional receipt every reinforcement row seeds under — the authority's own region, so a CSA grade seeds
    // `ca` and an EN grade `eu` with no per-standard region column to keep in step. A bar has no mortar joint, so
    // the coursing column is 0.
    public ComponentStandard Receipt => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The 31-row nominal-bar table. Imperial: ASTM A615 soft-metric printed values. CSA: G30.18 printed values. EN
// H-series: ISO 6935-2 Table 2 printed values, each row VENDOR-keyed to its BarDiameter catalogue member (D6..D50,
// the full roster). NAMED statics so the roster references rows SYMBOLICALLY — a typo'd bar is a compile miss.
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

// The two conduit BODIES, both typed-absent on diameter today. A certified series is a further row.
public static class Ducts {
    public static readonly ConduitRow CorrugatedSteel   = new("duct-corrugated-steel",   TendonConduitKind.CorrugatedSteel,   None);
    public static readonly ConduitRow CorrugatedPlastic = new("duct-corrugated-plastic", TendonConduitKind.CorrugatedPlastic, None);
    public static readonly ImmutableArray<ConduitRow> Rows = [CorrugatedSteel, CorrugatedPlastic];
}

// The BS 8666:2020 37-code schedule set. The five load-bearing codes are NAMED statics the hook rows, the stirrup
// override, and the bend-schedule publication reference SYMBOLICALLY — the retired form declared the whole roster
// first and then re-entered it five times by string key to recover rows that sit fourteen lines above, so a mistyped
// key was a runtime lookup rather than a compile miss. Straight is the code an unbent bar schedules under, so a
// bar-bending sheet lists it rather than dropping it.
public static class ShapeCodes {
    public static readonly ShapeCodeRow Straight         = new("00", 1, false);
    public static readonly ShapeCodeRow LBar             = new("11", 2, false);
    public static readonly ShapeCodeRow SemicircularHook = new("12", 2, false);
    public static readonly ShapeCodeRow AngledHook       = new("13", 3, false);
    public static readonly ShapeCodeRow ClosedLink       = new("51", 4, true);

    // 00/01 straight, 11..15 single-bend/hook, 21..36 multi-bend, 41..56 complex, 47/48/51/52/63 closed links, 64
    // six-leg, 67 radiused arc, 75 circular link, 77 helix, 98 chair, 99 the non-standard fully-dimensioned sketch.
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

// The ACI 318-19 standard end-hook angles: the straight-extension factor (×d_b), the absolute tail floor (180°
// development >= 65 mm, 135° stirrup/seismic >= 75 mm, 90° none), and the BS 8666 shape a longitudinal bar with that
// hook schedules as.
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

// --- [OPERATIONS] --------------------------------------------------------------------------
// The reinforcement algebra over the row currencies — the deleted RebarSection's projections re-homed as ONE
// operation owner so no bespoke payload record survives.
public static class RebarSchedule {
    // The rib-deformation receipt — Some for a Textured bar, None for a plain round. Each coefficient names the
    // clause it comes from, because they come from DIFFERENT clauses in DIFFERENT bodies: the height, spacing, and
    // ribless-gap ratios are the ASTM A615 §7.4 and Table 1 bounds, the flank inclination the ISO 6935-2 §4.14
    // minimum, the relative rib area the ISO/fib bond invariant a development-length law reads, and the rib-to-axis
    // inclination the RibPattern row's own §4.15 angle. The bounds each body sets on the OTHER body's bars are not
    // transcribed, so this receipt states the geometry a bar is rolled TO rather than a per-body limit set.
    const double RibHeightRatio = 0.05;          // ASTM A615 Table 1 minimum average rib height / d
    const double RibSpacingRatio = 0.7;          // ASTM A615 §7.4 maximum average rib spacing / d
    const double RiblessGapFraction = 0.125;     // ASTM A615 §7.4 maximum ribless perimeter fraction
    const double FlankInclinationDeg = 45.0;     // ISO 6935-2 §4.14 minimum flank inclination

    static readonly Seq<SizeLadderRow> RelativeRibArea = SizeLadder.Of((6.0, 0.035), (12.0, 0.040), (double.PositiveInfinity, 0.056));
    static readonly Seq<SizeLadderRow> MandrelFactor   = SizeLadder.Of((16.0, 4.0), (double.PositiveInfinity, 7.0));   // EN 1992 §8.3 former diameter

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

// The schedule-force basis as POLICY ROWS over ONE bar×grade projection (kN) — a new basis (a 0.2%-proof stress, a
// CSA-registered read) is one row. Characteristic is the spec-printed band × nominal area for a non-EN grade and the
// EnRebarFactory registered f_yk for an EN one; EnUltimate is the ductility-class ultimate (CreateBiLinear k·f_yk,
// k = 1.05/1.08/1.15 for A/B/C), Some only for the EN-bodied rows — the development/lap/overstrength capacity-design
// seam reads these, never a hand-keyed f_u beside the registered grade.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ForceBasis {
    public static readonly ForceBasis Characteristic = new("characteristic",
        projectKn: static (bar, arm) => arm.CharacteristicYieldMpa.Map(yield => yield * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnUltimate = new("en-ultimate",
        projectKn: static (bar, arm) => arm.En.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    [UseDelegateFromConstructor] private partial Option<double> ProjectKn(BarRow bar, GradeProperties.Rebar arm);

    // TWO states, not four: either the pairing is admissible and the basis projects, or the caller learns which of
    // the two it got wrong. A Fin<Option<double>> made "system mismatch", "basis unavailable for this grade", and
    // "here is your force" three outcomes a consumer had to unwrap in two steps, and the middle one read as an absent
    // quantity rather than the unanswerable request it is.
    public Fin<double> ForceKn(BarRow bar, MaterialGrade grade, Op key) =>
        from arm in grade.RebarArm.ToFin(new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Reinforcement))
        from admitted in guard(arm.Rolls(bar), new KernelFault.InvalidValue(nameof(bar), "a bar rolled by its grade system", Some(key)))
        from force in ProjectKn(bar, arm).ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Reinforcement, typeof(ForceBasis)))
        select force;
}

// The ACTIVE force basis as POLICY ROWS over ONE strand projection (kN) — the exact ForceBasis shape the passive bars
// ride. The jacking row resolves the ceiling at ComponentAuthority.JackingCeilingMpa, which evaluates the two-term
// minimum at the body that publishes BOTH coefficients: storing the proof coefficient alone left the ultimate term to
// be re-typed here under whatever literal this page happened to carry, so the two codes' ceilings could diverge from
// their own bodies without either column moving. A body publishing no jacking rule yields None rather than a
// fabricated ceiling.
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

// The certification-read relaxation loss stays a two-argument projection on the strand arm: it reads the 1000 h
// certification datum against an already-chosen initial force, never a force BASIS. The time-dependent loss SCHEDULE
// (creep/shrinkage/temperature interaction) is the forward Compute prestress fold's.
public static class TendonForce {
    public static double RelaxationLoss1000hKn(GradeProperties.Strand strand, double initialForceKn) =>
        strand.Relaxation.Rho1000Percent / 100.0 * initialForceKn;
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The ONE seed row over both modalities. Two rosters and two folds could not survive the landed seed spine — a
// ComponentFamily row binds exactly one roster and one law — and unifying them is the stronger form anyway: the
// owner's applicative traverse names every offending bar AND tendon in one verdict, where the retired page needed a
// hand `.Apply` over two independent traversals to get the same census.
[Union]
public abstract partial record ReinforcementRow {
    private ReinforcementRow() { }
    // The realized bar: SYMBOLIC BarRow/MaterialGrade/usage/surface references — a typo'd bar or grade is a compile
    // miss, never a runtime key fault — plus the optional hook policy a bent bar declares.
    public sealed record Bar(BarRow Size, MaterialGrade Grade, RebarUsage Usage, RebarSurface Surface, Option<(HookKind Kind, RebarHook Hook)> Bend) : ReinforcementRow;
    // The realized tendon: a strand grade row and, for a POST-tensioned line, its hardware assembly. A strand is a
    // Circle-profiled reinforcing part in the SAME family, never a new family row.
    public sealed record Tendon(MaterialGrade Strand, Option<TendonAssembly> Post) : ReinforcementRow;

    // The designation is DERIVED from the row's own currencies rather than hand-spelled beside them: authored strings
    // restating the bar, grade, and usage already in the row were as many chances for a name to disagree with the
    // thing it names, and the derivation makes that disagreement unrepresentable.
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

    // The two dimensional columns the profile and the bag both read, so neither re-enters the union.
    public Option<(double DiameterMm, double AreaMm2)> Section => Switch(
        bar: static row => Some((row.Size.NominalDiameterMm, row.Size.NominalAreaMm2)),
        tendon: static row => row.Strand.StrandArm.Map(static arm => (arm.DiameterMm, arm.AreaMm2)));

    // The row census, ACCUMULATING: the grade family, the arm, and the size-system admission are INDEPENDENT columns,
    // so a bar row naming a strand grade at a foreign size names both defects in ONE verdict.
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

// The ONE seed-time realization bag both modalities build (DetailLane.Realization). The four rows every reinforcing
// part carries — the cast joint token, the evidence grade, the nominal diameter, the cross-section area — are stated
// ONCE, and the per-modality extension is the only thing that dispatches. Three builders repeating the same four
// `from` lines were three chances for one of them to drift.
public static class ReinforcementDetail {
    public static Fin<PropertyBag> Of(ReinforcementRow row, EvidenceGrade source, Op key) =>
        from section in row.Section.ToFin(new KernelFault.InvalidValue(nameof(row.Section), "a reinforcement cross-section", Some(key)))
        from joint in ComponentDetail.Joint("Cast", key)
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, section.DiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, section.AreaMm2 * 1e-6)
        from extension in Extension(row, key)
        select ComponentDetail.RealizationRows([joint, ComponentDetail.Sourced(source), diameter, area, .. extension]);

    // A BAR publishes the BEND SCHEDULE: the BS 8666 shape code every placed bar carries, and — where the placement
    // declares a hook — the ACI inside-bend diameter, the floored straight extension, and the EN mandrel diameter as
    // one complex row. That block is what a bar-bending schedule is bought with. A STRAIGHT bar publishes shape code
    // 00 and no bend block, which is why the deliverable's bend columns are an optional extension rather than a gate.
    // A POST-TENSIONED tendon publishes its anchorage kind, its drape class, and — only where the conduit row holds a
    // certified inner diameter — the duct diameter beside the strand's own nominal.
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

// --- [POLICIES] ----------------------------------------------------------------------------
public static class ReinforcementSeed {
    // A longitudinal bar takes the development bend table; a tie, stirrup, or ring takes the stirrup-tie table and
    // schedules as a closed link — the correspondence StandardHook proves before it emits a receipt.
    static readonly Option<(HookKind, RebarHook)> DevelopmentHook = Some((HookKind.Development, RebarHook.Ninety));
    static readonly Option<(HookKind, RebarHook)> SeismicHook = Some((HookKind.Seismic, RebarHook.OneThirtyFive));
    static readonly Option<(HookKind, RebarHook)> TieHook = Some((HookKind.StirrupTie, RebarHook.OneThirtyFive));

    static ReinforcementRow Bar(BarRow size, MaterialGrade grade, RebarUsage usage, RebarSurface surface, Option<(HookKind, RebarHook)> bend = default) =>
        new ReinforcementRow.Bar(size, grade, usage, surface, bend);

    static ReinforcementRow Pre(MaterialGrade strand) => new ReinforcementRow.Tendon(strand, None);

    static ReinforcementRow Post(MaterialGrade strand, AnchorageKind anchorage, ConduitRow duct, TendonProfileKind profile) =>
        new ReinforcementRow.Tendon(strand, Some(new TendonAssembly(anchorage, duct, profile)));

    // The realized selection spans every USAGE the vocabulary declares, so the eleven-token IfcReinforcingBarTypeEnum
    // roster the page advertises is a roster the catalogue actually reaches; the tendon line covers both modalities,
    // so the anchorage, conduit, and drape vocabularies have readers rather than standing beside a fold that never
    // touched them. The Bars × MaterialGrade × RebarUsage space is the generator's domain; this is the realized
    // SELECTION over it.
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
        // The POST-TENSIONED lines: the mono-strand live end drawn through a corrugated steel duct on a parabolic
        // drape, and the multi-strand live end through a plastic duct on a harped one. Every dimension stays with the
        // certificate — these rows declare the ASSEMBLY, which is a design fact, not a product measurement.
        Post(MaterialGrade.S15Gr1860,  AnchorageKind.MonoLive,  Ducts.CorrugatedSteel,   TendonProfileKind.Parabolic),
        Post(MaterialGrade.Y1860S7D157, AnchorageKind.MultiLive, Ducts.CorrugatedPlastic, TendonProfileKind.Harped));

    // Both modalities transcribe a printed table whole — the ASTM A615/A706 and EN 10080 size and area columns, the
    // ASTM A416 / EN 10138-3 seven-wire line — so one evidence grade covers the realized selection; the per-COLUMN
    // vendor/defined splits the Bars table carries are facts of that table, not of the placed row.
    static readonly EvidenceGrade Tabulated = EvidenceGrade.Catalogue;

    // The seed POLICY value: this page states the roster and the law, component#COMPONENT_SEED owns the traverse and
    // the accumulating census. The regional receipt derives from the grade's own authority row.
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

    // The ComponentFamily.Reinforcement CAPACITY producer is an EXPLICIT TYPED REFUSAL, not silence: a bar and a
    // strand carry no section capacity of their own — a bar's structural participation is the member it reinforces,
    // and RcSectionBuilder.Capacity is that route, built and resolved in one entry. Binding the refusal is what makes
    // that route the ONLY one and keeps the family axis compiler-forced.
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
// Same Rasm.Materials.Component namespace as the section-02 fence; composes its prelude plus the VividOrange RC
// surface below.
using VividOrange.Sections;
using VividOrange.Sections.Reinforcement;
using VividOrange.Geometry;
using VividOrange.Sections.SectionProperties;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Profiles;
using VividOrange.Standards.Eurocode;
using UnitsNet;

// --- [MODELS] ------------------------------------------------------------------------------
// One RebarLayout [Union] collapses the four VividOrange.Sections layout-engine constructors — face/perimeter ×
// count/spacing — each case carrying the BarRow currency, PLUS the explicitly PLACED bar the engine's own
// LongitudinalReinforcement(IRebar, ILocalPoint2d) surface admits and the four engine cases structurally cannot
// express: a corner-bundled column, a haunched beam's staggered chord, any asymmetric arrangement whose stations are
// the design input rather than a rule's output. The coordinates are the engine's Y-Z SECTION plane (LocalPoint2d
// carries {Length Y; Length Z;}, never an X-Y pair).
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

    // The face this case places bars on, or absence for the perimeter and placed engines — read by the builder to
    // compute the barred-face set from the LAYOUT rather than by probing the engine.
    public Option<SectionFace> Face => Switch(
        faceCount:        static c => Some(c.Face),
        faceSpacing:      static s => Some(s.Face),
        perimeterCount:   static _ => Option<SectionFace>.None,
        perimeterSpacing: static _ => Option<SectionFace>.None,
        placed:           static _ => Option<SectionFace>.None);
}

// The placement PROJECTION of one layout case: a rule-driven case is an IReinforcementLayer the section routes by
// face or perimeter, a Placed case a LOOSE ILongitudinalReinforcement the ConcreteSection ctor takes directly —
// decompile-verified: `ConcreteSection(IProfile, IMaterial, ILink, Length, IList<ILongitudinalReinforcement>)` adds
// its list to the loose set and `Rebars` collects loose bars beside every materialized layer. The ad-hoc union is the
// ONE projection over both engine ingresses, so Build partitions on the discriminant rather than dispatching the
// layout family twice; both members are interfaces, so the CreateLayer/CreatePlaced factories are the ingress.
[Union<IReinforcementLayer, ILongitudinalReinforcement>(T1Name = "Layer", T2Name = "Placed")]
public readonly partial struct RebarPlacement;

// The reinforced-concrete receipt: the assembled IConcreteSection, the ONE ConcreteSectionProperties carrier (lazy,
// memoizing — constructed and eager-forced at the boundary), the resolved EN grade DATA, the cover, and the source
// Component. The transformed-section columns are carrier reads coerced to SI-mm at the receipt surface — the QTO seam
// and the capacity#SECTION_CAPACITY solvers read these, never a re-derived bar-area sum.
public sealed record RcSection(
    IConcreteSection Section, ConcreteSectionProperties Properties,
    EnConcreteMaterial Concrete, EnRebarMaterial Rebar, Option<double> LinkYieldMpa, double CoverMm,
    Component ConcreteProfile, FrozenSet<SectionFace> BarredFaces) {

    public double GrossSteelAreaMm2 => Properties.TotalReinforcementArea.SquareMillimeters;              // As
    public double ConcreteAreaMm2 => Properties.ConcreteArea.SquareMillimeters;                          // Ac (gross minus steel)
    public double ReinforcementRatio => Properties.GeometricReinforcementRatio.DecimalFractions;         // ρ = As/Ac
    public double ShearLinkAreaMm2 => Properties.CrossSectionalShearReinforcementArea.SquareMillimeters; // Asw — both link legs
    public double ReinforcementInertiaYyMm4 => Properties.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth;
    public double ReinforcementInertiaZzMm4 => Properties.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth;

    // The face-keyed reads are OPTIONED, and the discriminant is STRUCTURAL: the admitted layout names exactly which
    // faces carry bars, so a face outside that set answers absence without asking the engine — whose own
    // CalculateEffectiveDepth divides a face-layer centroid by an area that is zero there and answers a throw or a
    // NaN. Trapping that throw turned a known-empty query into an exception round trip and made every OTHER failure
    // inside the same call indistinguishable from a bar-less face. The set is computed once at the boundary from the
    // layout the builder already proved, so both reads are total over it.
    public Option<double> EffectiveDepthMm(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.EffectiveDepth(face).Millimeters) : None;
    public Option<double> FaceSteelAreaMm2(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.ReinforcementArea(face).SquareMillimeters) : None;

    // The PLACED bars the layout engines materialized — the section collects them by walking every layer through its
    // own GetPath/GetRebars pair, so the placement OUTPUT this page's layout algebra produces is readable rather than
    // computed and discarded inside the engine. A rebar detailer and the QTO seam both read this set.
    public IReadOnlyList<ILongitudinalReinforcement> PlacedBars => Section.Rebars;
}

// The only two capacity modalities an assembled reinforced-concrete section can request. The general CapacityBuild
// union remains the canonical solver request, but detail, anchorage, and bearing builds are unrepresentable here.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RcCapacityIntent {
    private RcCapacityIntent() { }
    public sealed record Hull(DiagramResolution Resolution) : RcCapacityIntent;
    public sealed record Elastic : RcCapacityIntent;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary classifies only the documented provider refusals and keeps their exact cause; an
// absent EN arm remains the typed return-contract refusal and every unknown throw remains exceptional.
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        key.Catch(
            () => Fin.Succ(new EnConcreteMaterial(grade, annex)),
            cause => GradeRefusal(key, cause));

    // The EN binding rides the grade's own Rebar arm, so a steel, strand, or timber row reaching this boundary
    // refuses on the arm rather than on a column it never carried.
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
    // The ONE reinforced-section boundary: admit the raw cover scalar (UnitsNet accepts a negative or NaN Length
    // silently, so the guard is load-bearing), lower the grades, prove the link and every layout bar against
    // MaterialGrade.Admits, mint the family-agnostic PROFILE-FAITHFUL IProfile, build the ConcreteSection + layers,
    // then construct the ConcreteSectionProperties carrier and EAGER-FORCE its first read so any degenerate-section
    // throw surfaces HERE, never on an interior receipt read.
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
        // LinkYieldMpa is the link grade's CHARACTERISTIC f_yk off the admitted arm — Option because a grade without
        // a published yield declares absence, and the capacity screen's V_Rd,s publication then stays absent rather
        // than riding a fabricated yield.
        select new RcSection(built, properties, concreteMaterial, rebarMaterial, arm.CharacteristicYieldMpa, coverMm, concrete,
            admittedLayout.Choose(static item => item.Face).ToFrozenSet());

    // Every layout bar proves the SAME MaterialGrade.Admits standard-consistency law the seed census runs — the one
    // owner, so an EN grade can never mint an A615/G30 layout bar through the builder — then its own shape admits.
    // The ADMITTED layout rides back out, so the builder consumes what the gate proved rather than the caller's
    // unproven sequence; the two admissions are INDEPENDENT, so a foreign-system bar at a non-positive count names
    // both defects at once.
    static Fin<RebarLayout> ValidateLayout(RebarLayout layout, MaterialGrade grade, Op key) =>
        (guard(grade.Admits(layout.Bar), new KernelFault.InvalidValue(nameof(layout.Bar), "a layout bar admitted by its grade system", Some(key))).ToValidation(),
         Shape(layout, key))
            .Apply(static (_, _) => unit).As().ToFin().Map(_ => layout);

    // A placed bar carries COORDINATES, so both section-plane offsets prove finite — UnitsNet accepts a NaN Length
    // silently, and a NaN station would egress as a bar the engine places nowhere.
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

    // The layout partitions ONCE on its own placement projection: rule-driven cases route through AddRebarLayer,
    // placed bars ride the ctor's loose-rebar list. Both sets reach ConcreteSection.Rebars, so the transformed-section
    // carrier sees every bar regardless of how it was expressed.
    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarRow link, Seq<RebarLayout> layout, double coverMm) {
        Seq<RebarPlacement> placements = layout.Map(l => PlacementOf(l, rebar));
        ConcreteSection section = new(profile, concrete, new Link(RebarOf(link, rebar)), Length.FromMillimeters(coverMm),
            [.. placements.Filter(static p => p.IsPlaced).Map(static p => p.AsPlaced)]);
        placements.Filter(static p => p.IsLayer).Iter(p => section.AddRebarLayer(p.AsLayer));
        return section;
    }

    // ONE bar mint serves layouts AND the promoted link: an EN BarRow feeds the catalogued BarDiameter ctor, an
    // imperial/CSA row its exact raw Length.
    static Rebar RebarOf(BarRow bar, EnRebarMaterial rebar) =>
        bar.CatalogueKey.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(bar.NominalDiameterMm)));

    // Each RebarLayout case -> its engine ingress; the generated [Union] Switch is the totality proof — a sixth case
    // breaks this arm at compile time, never a runtime-silent `_`.
    static RebarPlacement PlacementOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => RebarPlacement.CreateLayer(new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count)),
        faceSpacing:      s => RebarPlacement.CreateLayer(new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        perimeterCount:   c => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count)),
        perimeterSpacing: s => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        placed:           p => RebarPlacement.CreatePlaced(new LongitudinalReinforcement(
                                   RebarOf(p.Bar, rebar), new LocalPoint2d(Length.FromMillimeters(p.YMm), Length.FromMillimeters(p.ZMm)))));

    // The REINFORCEMENT-SIDE ENTRY into the capacity rail. RcCapacityIntent makes non-RC build variants impossible;
    // this boundary owns the subject and section when it lowers the intent to the canonical solver request.
    public static Fin<SectionCapacity> Capacity(
        Component concrete, EnConcreteGrade concreteGrade, MaterialGrade barGrade, BarRow link,
        Seq<RebarLayout> layout, double coverMm, RcCapacityIntent intent, CapacityPlacement placement, Op key) =>
        from section in Of(concrete, concreteGrade, barGrade, link, layout, coverMm, placement.Annex, key)
        from capacity in SectionCapacity.Resolve(Request(concrete.Designation, section, intent), key)
        select capacity;

    static CapacityBuild Request(ComponentId subject, RcSection section, RcCapacityIntent intent) => intent.Switch<CapacityBuild>(
        hull: h => new CapacityBuild.Hull(subject, section, h.Resolution),
        elastic: _ => new CapacityBuild.Elastic(subject, section));

    // The EC2 clear bar-spacing rule with the aggregate term LIVE: MaximumAggregateSize is a settable rule property,
    // so the (d_g + k2) branch participates. The aggregate scalar admits first — UnitsNet accepts a NaN Length
    // silently, so an unguarded NaN egresses as a Succ(NaN) spacing. Never an inline EC2 constant.
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
