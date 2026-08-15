# [MATERIALS_REINFORCEMENT]

THE REINFORCING-BAR SEED FAMILY and THE HOST-NEUTRAL REINFORCED-CONCRETE-SECTION ASSEMBLER. A rebar is one `ComponentRow` minted by the ONE generator `ReinforcementSeed.Rows -> Component.Of` over the `ComponentFamily.Reinforcement` policy row (`ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Circle`, cross-nominal the circle diameter — the bar's own disc solves like any other section while its RC participation rides `[03]-[RC_SECTION]`, which reads the assembled layer geometry rather than a single bar's receipt), never a `Rebar` type and never a bespoke `RebarSection` payload: the geometry is `SectionProfile.Circle(DiameterMm)`, the IFC stamp is `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` computed at seed time from the `RebarUsage` vocabulary, and the realization detail is the seed-built `RebarDetail.Of` bag.

Under `SEED_ROW_LAW` the pure standards-data vocabularies are frozen row tables with per-column provenance — `Bars` (imperial, CSA, and EN nominal sizes, the EN H-series VENDOR-keyed to the `VividOrange.ISections` `BarDiameter` catalogue), `Grades` (ASTM A615 and A706, CSA G30.18, and the EN 10080 ductility classes bound to their `EnRebarGrade`), `ShapeCodes` (the BS 8666:2020 schedule set), and `Strands` (the ACTIVE modality — ASTM A416 and EN 10138-3 seven-wire prestressing rows with their `RelaxationClass` certification data, seeded as `IfcTendon` `STRAND` component rows through the same generator) — while the policy vocabularies carrying delegate or IFC-token behavior stay `[SmartEnum<string>]`. Beside the strand line rides the POST-TENSIONING HARDWARE estate as CLOSED KIND ROSTERS over typed-absent dimensions: `AnchorageKind`/`TendonConduitKind` carry the roster-verified `IfcTendonAnchor` (`TENSIONING_END`/`FIXED_END`/`COUPLER`) and `IfcTendonConduit` (`DUCT`/`GROUTING_DUCT`/`TRUMPET`/`DIABOLO`/`COUPLER`) leaves, `TendonProfileKind` the drape vocabulary the `DetailSchema.TendonProfile` row stamps, and the `Anchorages`/`Ducts` rows hold every dimension column `None` — anchor bearing plates, wedge geometry, and duct diameter series are ETA/vendor-certified data no evidence pack two-sources, so the axes exist typed and the numbers refuse rather than invent. `RebarSchedule` owns the bend algebra over those currencies, and its receipt is what a BAR-BENDING SCHEDULE is made of: every placed bar publishes its BS 8666 shape code and, where it is bent, its ACI inside-bend diameter, hook extension, and EN mandrel diameter into the realization bag under the names the shop deliverable reads, so a detailed bar reaches a fabricator's cut-and-bend sheet instead of resolving to typed geometry no document consumes. The `ForceBasis` and `TendonBasis` policy rows own the passive and active force projections over one shape, and every fault across both modalities accumulates in one applicative pass rather than one modality masking the other's.

`[03]-[RC_SECTION]` is the family-agnostic assembler: `RcSectionBuilder.Of` lowers EN grades through the `EnGrade` boundary, builds the `VividOrange.Sections` `ConcreteSection` over the PROFILE-FAITHFUL `SectionSolver.ProfileOf(concrete.Profile, key)` `IProfile` of ANY `Component`, folds the `RebarLayout` `[Union]` through the collapsed placement engines, and mints the `RcSection` receipt whose transformed-section columns read the `ConcreteSectionProperties` carrier — never a hand-summed bar loop. `RcSectionBuilder.Capacity` is the reinforcement-side ENTRY into the capacity rail: the built section reaches `SectionCapacity.Resolve` from here, so the RC hull and the elastic transformed-section surfaces `capacity#SECTION_CAPACITY` declares have the producer this family owes them. Growth is one row: a new bar one `BarRow`, a new grade one `RebarGradeRow`, a new role one `RebarUsage` row, a new schedule shape one `ShapeCodeRow`, a new placed bar one `Placements` row through the same generator.

## [01]-[INDEX]

- [02]-[REINFORCEMENT_FAMILY]: the retained policy SmartEnums (`RebarStandard` · `RebarUsage` · `RebarSurface` · `RibPattern` · `HookKind` · `RebarHook` · `RelaxationClass`), the tier-3 frozen row tables (`Bars` 31 · `Grades` 11 · `ShapeCodes` 37 · the `Strands` seven-wire prestressing line) with per-column `VENDOR`/`DEFINED`/`PUBLISHED` provenance, the PT-hardware kind rosters (`AnchorageKind` · `TendonConduitKind` · `TendonProfileKind` with the `Anchorages`/`Ducts` typed-absent dimension rows), the `RebarRibGeometry`/`RebarBend` receipts, the `RebarSchedule` rib/hook algebra with the `ForceBasis` and `TendonBasis` force-policy rows and the `TendonForce` relaxation projection, the seed-time `RebarDetail.Of`/`TendonDetail.Of`/`TendonDetail.Assembly` realization bags, and the fail-loud `ReinforcementSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Reinforcement` policy row binds.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete assembler — the `RebarLayout` `[Union]` over the four `VividOrange.Sections` placement engines, the `EnGrade` EN-grade admission boundary, `RcSectionBuilder.Of` over the family-agnostic `SectionSolver.ProfileOf` concrete outline, the `ConcreteSectionProperties`-backed transformed-section receipt columns, and the EC2 `MinimumReinforcementSpacing` rule with the aggregate term wired.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: `RebarStandard`/`RebarUsage`/`RebarSurface`/`RibPattern`/`HookKind`/`RebarHook` the retained `[SmartEnum<string>]` policy vocabularies; `AnchorageKind`/`TendonConduitKind`/`TendonProfileKind` the PT-hardware kind rosters carrying their roster-verified `IfcTendonAnchor`/`IfcTendonConduit` leaves; `BarRow`/`RebarGradeRow`/`ShapeCodeRow`/`AnchorageRow`/`DuctRow` the tier-3 row currencies with `Bars`/`Grades`/`ShapeCodes`/`Anchorages`/`Ducts` the frozen tables; `RebarRibGeometry`/`RebarBend` the receipts; `RebarSchedule` the rib/hook operation owner; `ForceBasis` the schedule-force policy rows; `RebarDetail` the seed-time realization-bag constructor; `ReinforcementSeed` the `Rows` fold the `component#COMPONENT_OWNER` `ComponentFamily.Reinforcement` policy row binds.
- Cases: grade {A615 Gr40/Gr60/Gr75/Gr80 (carbon, non-weldable) · A706 Gr60W/Gr80W (low-alloy, weldable) · G30.18 400W/500W (CSA metric, weldable) · EN 10080 B500A/B500B/B500C (the ductility classes the `EnRebarFactory.CreateBiLinear` k = 1.05/1.08/1.15 branches read)} × size {#3..#11, #14, #18 imperial · 10M..55M CSA · H6..H50 EN keyed `BarDiameter.D6`..`D50`} × usage {main · ligature · shear · punching · edge · ring · anchoring · spacer · stud · userdefined · notdefined — the full verified 11-member `IfcReinforcingBarTypeEnum`} × surface {textured · plain} × rib-pattern {uniform-height 90° · crescent 60°} × hook {90°/135°/180° over development/stirrup-tie/seismic ACI tables} × shape-code {the BS 8666:2020 37-code set} — a bar is one `Placements` row over one `BarRow` and one `RebarGradeRow`, the standard-consistency law `RebarGradeRow.Admits` (a grade admits only the bar rows its spec body rolls) enforced BEFORE construction.
- Entry: `public static Fin<Seq<ComponentRow>> ReinforcementSeed.Rows(Context context)` traverses `Placements` through the common `Component.Of` rail. `RebarSchedule.StandardHook(BarRow, RebarUsage, HookKind, RebarHook, Op)` rejects a longitudinal usage paired with a tie/seismic hook policy and a transverse usage paired with the development policy before emitting the ACI/EN/BS bend receipt.
- Packages: Rasm.Numerics (project — `PositiveMagnitude` the `>0` finite magnitude every admitted diameter column lifts into), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `PropertyBag`/`DetailSchema`/`Dimension` the detail bag composes), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue the EN `BarRow` rows VENDOR-key; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnRebarGrade` the EN-bodied binding, `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` the registered yield + ductility ultimate; `.api/api-vividorange-materials.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, `[UseDelegateFromConstructor]` the `HookKind` bend delegate), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`/`guard`), BCL inbox (`FrozenDictionary`, `ImmutableArray`, collection expressions).
- Growth: one row per new fact — a new bar size one `BarRow` (printed diameter/area/weight PUBLISHED, or the DEFINED `πd²/4`/`A·ρ` fallbacks), a new grade one `RebarGradeRow` bound to its `EnRebarGrade` when EN-bodied, a new role one `RebarUsage` row carrying its verified token, a new schedule shape one `ShapeCodeRow`, a new hook table one `HookKind` row with its bend delegate, a new realized bar one `Placements` row; a new strand diameter or grade one `StrandRow`, a new relaxation certification one `RelaxationClass` row, a new realized tendon one `Tendons` row; a new anchorage or conduit product is one `AnchorageRow`/`DuctRow` whose dimension columns fill ONLY from an ETA/vendor certificate (the roster is closed vocabulary today, every dimension typed-absent), and a duct row gaining a certified diameter seeds as an `IfcTendonConduit` component through the same generator once the family admission widens to its annular profile — never a per-bar type, never a `ComponentFamily` edit, never a central edit. A welded mesh grows as an `IfcReinforcingMesh` projection over the same row currencies, never an eleventh family.
- Boundary: the seed admits raw standards data once through the symbolic `Bars`/`Grades` row references, `RebarGradeRow.Admits`, and the `SectionProfile.Circle.Of` rail. `StandardHook` validates the `usage.Stirrup`/`HookKind` correspondence, `ForceBasis.ForceKn` validates the bar/grade system before any registered-grade projection, and `TendonBasis.ForceKn` reads the strand row alone — its jacking ceiling resolves the authority's own `JackingProofFactor` column, so a body with no published rule projects absence. The IFC role remains `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)`, and the independent substance and appearance identifiers remain grade-carried. PT hardware is ROSTER-ONLY at the component tier: an `AnchorageRow`/`DuctRow` carries no dimension a certificate has not published, so no hardware `Component` seeds today — the hardware reaches the seam as the `TendonDetail.Assembly` bag rows (`AnchorageType`/`DuctDiameter`/`TendonProfile`, each stamped only where its column is `Some`), and the `DuctDiameter` row rides BESIDE the strand's own `NominalDiameter` because a tendon assembly carries both facts as two rows, exactly as `Rasm.Element/Properties/property#DETAIL_SCHEMA` declares them.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                                // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                                 // Op (the boundary-admission key), AcceptValidated, Context
using Rasm.Element.Composition;                                // MaterialId; PropertyBag/DetailSchema/Dimension (the seam detail-bag currencies RebarDetail composes)
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using Thinktecture;
using VividOrange.Sections.Reinforcement;          // BarDiameter (the EN-10080 D6..D50 catalogue the EN BarRow rows VENDOR-key)
using VividOrange.Materials.StandardMaterials.En;  // EnRebarGrade, EnRebarFactory (the registered yield + ductility-class ultimate)
using static LanguageExt.Prelude;                  // guard, Seq, Some/None

// Every family page declares in the ONE Rasm.Materials.Component namespace; component#COMPONENT_OWNER binds
// ReinforcementSeed.Rows by bare name on the ComponentFamily.Reinforcement policy row (the <Family>Seed naming keeps rows collision-free).
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The spec body — the FORM-law survivor carrying behavior columns: weldability, the ComponentStandard projection, the
// RIB RULE its mills roll deformations to, and Rolls the SIZE SYSTEM whose BarRow rows this body rolls. Rolls is a
// DELEGATE COLUMN rather than an equality branch: A706 bars roll at the A615 imperial sizes, and stating that as a
// row value keeps the size correspondence with the vocabulary that owns it while the deferred evaluation dissolves
// the static self-reference an eagerly-read column would have needed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615    = new("astm-a615",  weldable: false, authority: "ASTM A615/A615M", body: ComponentAuthority.Astm, region: "us", rolls: static () => A615);
    public static readonly RebarStandard A706    = new("astm-a706",  weldable: true,  authority: "ASTM A706/A706M", body: ComponentAuthority.Astm, region: "us", rolls: static () => A615);
    public static readonly RebarStandard G30     = new("csa-g30.18", weldable: true,  authority: "CSA G30.18",      body: ComponentAuthority.Csa,  region: "ca", rolls: static () => G30);
    public static readonly RebarStandard En10080 = new("en-10080",   weldable: true,  authority: "EN 1992-1-1 / EN 10080 / ISO 6935-2", body: ComponentAuthority.En, region: "eu", rolls: static () => En10080);
    public bool Weldable { get; }
    public string Authority { get; }
    public ComponentAuthority Body { get; }
    public string Region { get; }
    [UseDelegateFromConstructor] public partial RebarStandard Rolls();
    public ComponentStandard Component => new(Region, StandardJointThicknessMm: 0.0, Authority: Body);   // a bar has no mortar joint — the coursing column is 0
}

// The bar's STRUCTURAL ROLE — the FULL verified 11-member IfcReinforcingBarTypeEnum (GeometryGym 25.7.30) so the seam
// PredefinedType is a row read, never a widened-later subset: MAIN/LIGATURE/SHEAR/PUNCHING/EDGE/RING/ANCHORING/SPACEBAR/
// STUD plus USERDEFINED (an owner-labelled role) and NOTDEFINED (an undeclared import). Stirrup routes the RcSection
// link-vs-longitudinal placement AND the StandardHook closed-link ShapeCode override.
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
    public bool Stirrup { get; }   // true -> the RcSection link / transverse confinement; false -> a longitudinal layer bar
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

// The rib-deformation FORM — ISO 6935-2 §4.15 β between a transverse rib and the bar axis: parallel uniform-height 90°,
// modern hot-rolled crescent two-series 60°. The RebarSchedule.Ribs argument; deformed bar defaults to Crescent.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RibPattern {
    public static readonly RibPattern UniformHeight = new("uniform-height", inclinationDeg: 90.0);
    public static readonly RibPattern Crescent      = new("crescent",       inclinationDeg: 60.0);
    public double InclinationDeg { get; }
}

// The ACI 318-19 §25.3 bend-table discriminant with the minimum inside-bend multiple (×d_b) as a CONSTRUCTOR DELEGATE
// row: development (Table 25.3.1) 6·d_b for d ≤ 25.4 mm, 8 to 36 mm, 10 above; stirrup-tie (25.3.2) and seismic (25.3.4)
// 4·d_b to 16 mm, 6 above — so a #5 stirrup bends at 4·d_b, never the 6·d_b a development bar uses. The band thresholds
// are the ACI size-group diameter boundaries.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookKind {
    public static readonly HookKind Development = new("development", aciTable: "ACI 318-19 Table 25.3.1", minInsideBendFactor: static d => d <= 25.4 ? 6.0 : d <= 36.0 ? 8.0 : 10.0);
    public static readonly HookKind StirrupTie  = new("stirrup-tie", aciTable: "ACI 318-19 Table 25.3.2", minInsideBendFactor: static d => d <= 16.0 ? 4.0 : 6.0);
    public static readonly HookKind Seismic     = new("seismic",     aciTable: "ACI 318-19 Table 25.3.4", minInsideBendFactor: static d => d <= 16.0 ? 4.0 : 6.0);
    [UseDelegateFromConstructor] public partial double MinInsideBendFactor(double barDiameterMm);
    public string AciTable { get; }
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

// The PT anchorage kind axis — the closed hardware vocabulary over the verified IfcTendonAnchor leaf set (GeometryGym 25.7.30)
// {COUPLER, FIXED_END, TENSIONING_END}: mono- and multi-strand live ends share the TENSIONING_END token and split
// on the strand-count band the AnchorageRow carries (its one dimension-free discriminant), the dead end is
// FIXED_END, and the tendon coupler the anchor entity's own COUPLER. Every geometric column is ETA/vendor
// territory — the kind axis is what this page can close without inventing a plate size.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnchorageKind {
    public static readonly AnchorageKind MonoLive  = new("mono-live",  ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind MultiLive = new("multi-live", ifcPredefinedType: "TENSIONING_END");
    public static readonly AnchorageKind DeadEnd   = new("dead-end",   ifcPredefinedType: "FIXED_END");
    public static readonly AnchorageKind Coupler   = new("coupler",    ifcPredefinedType: "COUPLER");
    public string IfcPredefinedType { get; }
}

// The tendon conduit kind axis over the verified IfcTendonConduit leaf set (GeometryGym 25.7.30) {COUPLER, DIABOLO, DUCT,
// GROUTING_DUCT, TRUMPET}: the two duct BODIES (corrugated galvanized steel, corrugated HDPE plastic) both ride
// the DUCT token and split on this axis's own body discriminant, the accessory kinds carry their exact tokens.
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
// The ONE nominal mass density every reinforcement weight derivation reads — declared once at page grain because it
// is one published material fact, not a per-row column two carriers would each get to spell differently.
public static class ReinforcementSteel {
    public const double DensityKgM3 = 7850.0;   // PUBLISHED: EN 10080 nominal mass density
}

// SEED_ROW_LAW tier-3 currencies: pure standards data as frozen readonly-record-struct rows with per-column provenance.
// BarRow — CatalogueKey VENDOR (Some only for the EN H-series, the BarDiameter the Rebar(IMaterial, BarDiameter) ctor
// consumes; imperial/CSA None so the exact non-EN nominal feeds a raw Length); diameter, area, and weight all PUBLISHED,
// because every size in every rostered body PRINTS all three. The area and weight columns were previously guarded by a
// positivity test selecting a πd²/4 or A·ρ derivation no row could reach — a fallback with no reachable input is a
// second definition of a printed value, and the printed value is the one the standard means. Standard is the
// size-defining spec body (imperial -> A615, CSA -> G30, EN -> En10080).
public readonly record struct BarRow(string Key, RebarStandard Standard, Option<BarDiameter> CatalogueKey,
    double NominalDiameterMm, double NominalAreaMm2, double NominalWeightKgM);

// The grade row: the spec body, the two INDEPENDENT MaterialId slots (SubstanceId the per-grade steel.<designation>
// Mechanical row carrying f_yk + the ACI 318 §20.2.2.2 E_s 200 GPa — NEVER the generic metal.steel 235 MPa / 210 GPa
// section baseline; AppearanceId the render finish), and the Option<EnRebarGrade> binding. The CHARACTERISTIC YIELD
// has exactly ONE owner per row: an EN-bodied grade reads the registered f_yk off EnRebarFactory, and the others read
// the column their own spec prints — the previous shape carried a hand-asserted 500 MPa literal beside the registered
// grade that owns that number, so two sources answered one question and only one of them could be corrected. Admits
// is the standard-consistency law: a grade admits only the bar rows its body rolls.
public readonly record struct RebarGradeRow(string Key, Option<double> PublishedYieldMpa, RebarStandard Standard,
    string SubstanceId, string AppearanceId, Option<EnRebarGrade> EnGrade) {
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
    public bool Weldable => Standard.Weldable;
    public bool Admits(BarRow bar) => Standard.Rolls() == bar.Standard;

    // The one yield read: registered where a registered grade exists, printed where the spec prints it. Exactly one
    // arm answers on every rostered row, so the projection is total without either source shadowing the other.
    public Option<double> CharacteristicYieldMpa =>
        EnGrade.Map(static grade => EnRebarFactory.CreateLinearElastic(grade).Strength.Megapascals)
            .IfNone(() => PublishedYieldMpa);
}

// The seven-wire prestressing strand row — the ACTIVE reinforcement modality beside the passive bars: the printed
// nominal diameter, cross-section area, and ultimate strength fpu PUBLISHED verbatim (ASTM A416/A416M Table 1 for the
// imperial grades, EN 10138-3 for Y1860S7), ProofRatio the printed yield-to-ultimate ratio (A416 low-relaxation
// fpy = 0.90·fpu; EN Fp0,1/Fm = 0.88 off the printed force pair), the RelaxationClass certification datum, and the
// per-strand Mechanical substance slot. Weight is the DEFINED A·ρ derivation (no printed twin kept).
public readonly record struct StrandRow(string Key, ComponentAuthority Authority, string Region,
    double NominalDiameterMm, double AreaMm2, double UltimateMpa, double ProofRatio, RelaxationClass Relaxation, string SubstanceId) {
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public double NominalWeightKgM => AreaMm2 * 1e-6 * ReinforcementSteel.DensityKgM3;   // DEFINED: A·ρ, no printed twin kept
}

// The BS 8666:2020 schedule shape row: Legs the straight-leg count (the A..F dimension letters the host shape-table lofts
// the polyline from), Link true for the closed-perimeter stirrup/circular/helix shapes. The cutting-length formula is the
// host shape-table's; this row carries the token the IfcReinforcingBar BendingShapeCode wire reads.
public readonly record struct ShapeCodeRow(string Key, int Legs, bool Link);

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

// The host-neutral bend receipt: the ACI inside bend diameter, the floored straight extension, the EN 1992 §8.3 mandrel
// (former) diameter (Link.MinimumMandrelDiameter parity), and the BS 8666 ShapeCodeRow — the two diameters distinct
// because the EN former rule (4·d/7·d) and the ACI hook rule (4..10·d by HookKind × band) differ.
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm, double MandrelDiameterMm, ShapeCodeRow Shape);

// --- [TABLES] ------------------------------------------------------------------------------
// The 31-row nominal-bar table. Imperial: ASTM A615 soft-metric printed values. CSA: G30.18 printed values. EN H-series:
// ISO 6935-2 Table 2 printed values, each row VENDOR-keyed to its BarDiameter catalogue member (D6..D50, the full roster).
// NAMED statics (the fastener Threads/Grades form) so Placements references rows SYMBOLICALLY — a typo'd bar is a
// compile miss, never a runtime key.
public static class Bars {
    public static readonly BarRow No3 = new("no3",  RebarStandard.A615,    None,                     9.525, 71.0,   0.560);
    public static readonly BarRow No4 = new("no4",  RebarStandard.A615,    None,                    12.700, 129.0,  0.994);
    public static readonly BarRow No5 = new("no5",  RebarStandard.A615,    None,                    15.875, 199.0,  1.552);
    public static readonly BarRow No6 = new("no6",  RebarStandard.A615,    None,                    19.050, 284.0,  2.235);
    public static readonly BarRow No7 = new("no7",  RebarStandard.A615,    None,                    22.225, 387.0,  3.042);
    public static readonly BarRow No8 = new("no8",  RebarStandard.A615,    None,                    25.400, 510.0,  3.973);
    public static readonly BarRow No9 = new("no9",  RebarStandard.A615,    None,                    28.651, 645.0,  5.060);
    public static readonly BarRow No10 = new("no10", RebarStandard.A615,   None,                    32.258, 819.0,  6.404);
    public static readonly BarRow No11 = new("no11", RebarStandard.A615,   None,                    35.814, 1006.0, 7.907);
    public static readonly BarRow No14 = new("no14", RebarStandard.A615,   None,                    43.002, 1452.0, 11.380);
    public static readonly BarRow No18 = new("no18", RebarStandard.A615,   None,                    57.328, 2581.0, 20.240);
    public static readonly BarRow M10 = new("10m",  RebarStandard.G30,     None,                    11.300, 100.0,  0.785);
    public static readonly BarRow M15 = new("15m",  RebarStandard.G30,     None,                    16.000, 200.0,  1.570);
    public static readonly BarRow M20 = new("20m",  RebarStandard.G30,     None,                    19.500, 300.0,  2.355);
    public static readonly BarRow M25 = new("25m",  RebarStandard.G30,     None,                    25.200, 500.0,  3.925);
    public static readonly BarRow M30 = new("30m",  RebarStandard.G30,     None,                    29.900, 700.0,  5.495);
    public static readonly BarRow M35 = new("35m",  RebarStandard.G30,     None,                    35.700, 1000.0, 7.850);
    public static readonly BarRow M45 = new("45m",  RebarStandard.G30,     None,                    43.700, 1500.0, 11.775);
    public static readonly BarRow M55 = new("55m",  RebarStandard.G30,     None,                    56.400, 2500.0, 19.625);
    public static readonly BarRow H6  = new("h6",   RebarStandard.En10080, Some(BarDiameter.D6),     6.000, 28.3,   0.222);
    public static readonly BarRow H8  = new("h8",   RebarStandard.En10080, Some(BarDiameter.D8),     8.000, 50.3,   0.395);
    public static readonly BarRow H10 = new("h10",  RebarStandard.En10080, Some(BarDiameter.D10),   10.000, 78.5,   0.617);
    public static readonly BarRow H12 = new("h12",  RebarStandard.En10080, Some(BarDiameter.D12),   12.000, 113.0,  0.888);
    public static readonly BarRow H14 = new("h14",  RebarStandard.En10080, Some(BarDiameter.D14),   14.000, 154.0,  1.210);
    public static readonly BarRow H16 = new("h16",  RebarStandard.En10080, Some(BarDiameter.D16),   16.000, 201.0,  1.580);
    public static readonly BarRow H20 = new("h20",  RebarStandard.En10080, Some(BarDiameter.D20),   20.000, 314.0,  2.470);
    public static readonly BarRow H25 = new("h25",  RebarStandard.En10080, Some(BarDiameter.D25),   25.000, 491.0,  3.850);
    public static readonly BarRow H28 = new("h28",  RebarStandard.En10080, Some(BarDiameter.D28),   28.000, 616.0,  4.840);
    public static readonly BarRow H32 = new("h32",  RebarStandard.En10080, Some(BarDiameter.D32),   32.000, 804.0,  6.310);
    public static readonly BarRow H40 = new("h40",  RebarStandard.En10080, Some(BarDiameter.D40),   40.000, 1257.0, 9.860);
    public static readonly BarRow H50 = new("h50",  RebarStandard.En10080, Some(BarDiameter.D50),   50.000, 1964.0, 15.420);
    public static readonly ImmutableArray<BarRow> Rows = [
        No3, No4, No5, No6, No7, No8, No9, No10, No11, No14, No18,
        M10, M15, M20, M25, M30, M35, M45, M55,
        H6, H8, H10, H12, H14, H16, H20, H25, H28, H32, H40, H50];
}

// The 11-row grade table. The ASTM and CSA yields are the spec-nominal PUBLISHED bands their own standards print; the
// EN B500 rows print NONE and bind their EnRebarGrade instead, because EnRebarFactory owns the registered f_yk and k
// for exactly those grades and a column beside it would be a second answer to a question already owned.
public static class Grades {
    public static readonly RebarGradeRow Gr40   = new("gr40",  Some(280.0), RebarStandard.A615,    "steel.gr40",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr60   = new("gr60",  Some(420.0), RebarStandard.A615,    "steel.gr60",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr75   = new("gr75",  Some(520.0), RebarStandard.A615,    "steel.gr75",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr80   = new("gr80",  Some(550.0), RebarStandard.A615,    "steel.gr80",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr60W  = new("gr60w", Some(420.0), RebarStandard.A706,    "steel.gr60w", "metal.steel", None);
    public static readonly RebarGradeRow Gr80W  = new("gr80w", Some(550.0), RebarStandard.A706,    "steel.gr80w", "metal.steel", None);
    public static readonly RebarGradeRow Gr400W = new("400w",  Some(400.0), RebarStandard.G30,     "steel.400w",  "metal.steel", None);
    public static readonly RebarGradeRow Gr500W = new("500w",  Some(500.0), RebarStandard.G30,     "steel.500w",  "metal.steel", None);
    public static readonly RebarGradeRow B500A  = new("b500a", None,        RebarStandard.En10080, "steel.b500a", "metal.steel", Some(EnRebarGrade.B500A));
    public static readonly RebarGradeRow B500B  = new("b500b", None,        RebarStandard.En10080, "steel.b500b", "metal.steel", Some(EnRebarGrade.B500B));
    public static readonly RebarGradeRow B500C  = new("b500c", None,        RebarStandard.En10080, "steel.b500c", "metal.steel", Some(EnRebarGrade.B500C));
    public static readonly ImmutableArray<RebarGradeRow> Rows = [Gr40, Gr60, Gr75, Gr80, Gr60W, Gr80W, Gr400W, Gr500W, B500A, B500B, B500C];
}

// The strand table, PUBLISHED verbatim (ASTM A416 Grade 250/270 printed area rows; EN 10138-3 Y1860S7 printed
// diameter/area rows) — named statics so a tendon placement references its row symbolically. The realized selection;
// a new diameter or grade is one row.
public static class Strands {
    public static readonly StrandRow S13Gr1725   = new("strand-13-gr250",  ComponentAuthority.Astm, "us", 12.70, 92.9,  1725.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1725");
    public static readonly StrandRow S13Gr1860   = new("strand-13-gr270",  ComponentAuthority.Astm, "us", 12.70, 98.7,  1860.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1860");
    public static readonly StrandRow S15Gr1860   = new("strand-15-gr270",  ComponentAuthority.Astm, "us", 15.24, 140.0, 1860.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1860");
    public static readonly StrandRow Y1860S7D125 = new("strand-y1860s7-125", ComponentAuthority.En, "eu", 12.50, 93.0,  1860.0, 0.88, RelaxationClass.LowRelaxation, "steel.y1860s7");
    public static readonly StrandRow Y1860S7D157 = new("strand-y1860s7-157", ComponentAuthority.En, "eu", 15.70, 150.0, 1860.0, 0.88, RelaxationClass.LowRelaxation, "steel.y1860s7");
    public static readonly ImmutableArray<StrandRow> Rows = [S13Gr1725, S13Gr1860, S15Gr1860, Y1860S7D125, Y1860S7D157];
}

// PT-hardware row currencies: the kind is CLOSED, every dimension TYPED-ABSENT — anchor bearing-plate size,
// wedge geometry, strand capacity, duct inner diameter, and wall gauge are ETA/vendor-certificate data no pack
// two-sources, so a `Some` in any column names its certificate or does not land. The rows exist so the tendon
// assembly bag, the IFC leaf, and the growth path are typed TODAY while the numbers stay honest.
public readonly record struct AnchorageRow(string Key, AnchorageKind Kind, Option<int> Strands, Option<double> BearingPlateMm);
public readonly record struct DuctRow(string Key, TendonConduitKind Kind, Option<double> InnerDiameterMm, Option<double> WallMm);

// One realized row per kind — the closed vocabulary made joinable; a certified product is a further row whose
// dimension columns are Some, and a duct row gaining a certified inner diameter is what the Growth clause seeds
// as an IfcTendonConduit component.
public static class Anchorages {
    public static readonly AnchorageRow MonoLive  = new("anchor-mono-live",  AnchorageKind.MonoLive,  None, None);
    public static readonly AnchorageRow MultiLive = new("anchor-multi-live", AnchorageKind.MultiLive, None, None);
    public static readonly AnchorageRow DeadEnd   = new("anchor-dead-end",   AnchorageKind.DeadEnd,   None, None);
    public static readonly AnchorageRow Coupler   = new("anchor-coupler",    AnchorageKind.Coupler,   None, None);
    public static readonly ImmutableArray<AnchorageRow> Rows = [MonoLive, MultiLive, DeadEnd, Coupler];
}

public static class Ducts {
    public static readonly DuctRow CorrugatedSteel   = new("duct-corrugated-steel",   TendonConduitKind.CorrugatedSteel,   None, None);
    public static readonly DuctRow CorrugatedPlastic = new("duct-corrugated-plastic", TendonConduitKind.CorrugatedPlastic, None, None);
    public static readonly ImmutableArray<DuctRow> Rows = [CorrugatedSteel, CorrugatedPlastic];
}

// The BS 8666:2020 37-code schedule set. 00/01 straight, 11..15 single-bend/hook, 21..36 multi-bend, 41..56 complex,
// 47/48/51/52/63 closed links, 64 six-leg, 67 radiused arc, 75 circular link, 77 helix, 98 chair, 99 the non-standard
// fully-dimensioned sketch (Legs 0).
public static class ShapeCodes {
    public static readonly ImmutableArray<ShapeCodeRow> Rows = [
        new("00", 1, false), new("01", 1, false), new("11", 2, false), new("12", 2, false), new("13", 3, false),
        new("14", 2, false), new("15", 2, false), new("21", 3, false), new("22", 4, false), new("23", 3, false),
        new("24", 3, false), new("25", 3, false), new("26", 3, false), new("27", 3, false), new("28", 3, false),
        new("29", 3, false), new("31", 4, false), new("32", 4, false), new("33", 3, true),  new("34", 4, false),
        new("35", 4, false), new("36", 4, false), new("41", 5, false), new("44", 5, false), new("46", 5, false),
        new("47", 4, true),  new("48", 4, true),  new("51", 4, true),  new("52", 4, true),  new("56", 5, false),
        new("63", 5, true),  new("64", 6, false), new("67", 1, false), new("75", 2, true),  new("77", 1, true),
        new("98", 5, false), new("99", 0, false)];
    public static readonly FrozenDictionary<string, ShapeCodeRow> ByKey = Rows.ToFrozenDictionary(static r => r.Key, StringComparer.Ordinal);
    // The load-bearing anchors the hook rows, the stirrup override, and the bend-schedule publication read (declared
    // after ByKey — textual init order). Straight is the code an unbent bar schedules under, so a bar-bending sheet
    // lists it rather than dropping it.
    public static readonly ShapeCodeRow Straight         = ByKey["00"];
    public static readonly ShapeCodeRow LBar             = ByKey["11"];
    public static readonly ShapeCodeRow SemicircularHook = ByKey["12"];
    public static readonly ShapeCodeRow AngledHook       = ByKey["13"];
    public static readonly ShapeCodeRow ClosedLink       = ByKey["51"];
}

// The ACI 318-19 standard end-hook angles: the straight-extension factor (×d_b), the absolute tail floor (180° development
// >= 65 mm, 135° stirrup/seismic >= 75 mm, 90° none), and the BS 8666 shape a longitudinal bar with that hook schedules as.
// Cross-type static initialization is acyclic: touching RebarHook runs ShapeCodes' initializer first.
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
// The reinforcement algebra over the row currencies — the deleted RebarSection's projections re-homed as ONE operation
// owner so no bespoke payload record survives.
public static class RebarSchedule {
    // The rib-deformation receipt — Some for a Textured bar, None for a plain round. Each coefficient names the clause
    // it comes from, because they come from DIFFERENT clauses in DIFFERENT bodies and a table-wide banner claiming one
    // standard for all of them would describe a deformation no single specification defines: the height, spacing, and
    // ribless-gap ratios are the ASTM A615 §7.4 and Table 1 bounds, the flank inclination the ISO 6935-2 §4.14
    // minimum, the relative rib area the ISO/fib bond invariant a development-length law reads, and the rib-to-axis
    // inclination the RibPattern row's own §4.15 angle. The bounds each body sets on the OTHER body's bars are not
    // transcribed, so this receipt states the geometry a bar is rolled TO rather than a per-body limit set.
    const double RibHeightRatio = 0.05;          // ASTM A615 Table 1 minimum average rib height / d
    const double RibSpacingRatio = 0.7;          // ASTM A615 §7.4 maximum average rib spacing / d
    const double RiblessGapFraction = 0.125;     // ASTM A615 §7.4 maximum ribless perimeter fraction
    const double FlankInclinationDeg = 45.0;     // ISO 6935-2 §4.14 minimum flank inclination

    public static Option<RebarRibGeometry> Ribs(BarRow bar, RebarSurface surface, RibPattern pattern) =>
        surface.Ribbed
            ? Some(new RebarRibGeometry(
                TransverseRibHeightMm:    RibHeightRatio * bar.NominalDiameterMm,
                TransverseRibSpacingMm:   RibSpacingRatio * bar.NominalDiameterMm,
                LongitudinalRibHeightMm:  RibHeightRatio * bar.NominalDiameterMm,
                FlankInclinationDeg:      FlankInclinationDeg,
                RibInclinationDeg:        pattern.InclinationDeg,
                RelativeRibArea:          bar.NominalDiameterMm <= 6.0 ? 0.035 : bar.NominalDiameterMm <= 12.0 ? 0.040 : 0.056,
                RiblessPerimeterFraction: RiblessGapFraction,
                Pattern:                  pattern))
            : None;

    public static Fin<RebarBend> StandardHook(BarRow bar, RebarUsage usage, HookKind kind, RebarHook hook, Op key) =>
        guard(usage.Stirrup == (kind != HookKind.Development),
                ComponentFault.Dimension(key, $"<rebar-hook-usage-mismatch:{usage.Key}:{kind.Key}>"))
            .ToFin()
            .Map(_ => new RebarBend(
                hook.BendDegrees,
                kind.MinInsideBendFactor(bar.NominalDiameterMm) * bar.NominalDiameterMm,
                Math.Max(hook.ExtensionFactor * bar.NominalDiameterMm, hook.MinExtensionMm),
                (bar.NominalDiameterMm <= 16.0 ? 4.0 : 7.0) * bar.NominalDiameterMm,
                usage.Stirrup ? ShapeCodes.ClosedLink : hook.Shape));
}

// The schedule-force basis as POLICY ROWS over ONE bar×grade projection (kN) — the three sibling *ForceKn methods are
// the deleted form; a new basis (a 0.2%-proof stress, a CSA-registered read) is one row. Nominal is the spec-printed
// grade band × nominal area, always Some; the EN rows read the EnRebarFactory registered CHARACTERISTIC yield
// (CreateLinearElastic f_yk) and the ductility-class ultimate (CreateBiLinear k·f_yk, k = 1.05/1.08/1.15 for A/B/C),
// Some only for the EN-bodied B500A/B/C — the development/lap/overstrength capacity-design seam reads these, never a
// hand-keyed f_u beside the registered grade. Declared after the row models it projects (dependency cluster).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ForceBasis {
    public static readonly ForceBasis Characteristic = new("characteristic", projectKn: static (bar, grade) => grade.CharacteristicYieldMpa.Map(yield => yield * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnUltimate     = new("en-ultimate",    projectKn: static (bar, grade) => grade.EnGrade.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    [UseDelegateFromConstructor] private partial Option<double> ProjectKn(BarRow bar, RebarGradeRow grade);

    // TWO states, not four: either the pairing is admissible and the basis projects, or the caller learns which of
    // the two it got wrong. The previous Fin<Option<double>> made "system mismatch", "basis unavailable for this
    // grade", and "here is your force" three outcomes a consumer had to unwrap in two steps, and the middle one read
    // as an absent quantity rather than the unanswerable request it is.
    public Fin<double> ForceKn(BarRow bar, RebarGradeRow grade, Op key) =>
        from admitted in guard(grade.Admits(bar), ComponentFault.Grade(key, $"<rebar-force-system-mismatch:{bar.Key}:{grade.Key}>"))
        from force in ProjectKn(bar, grade).ToFin(ComponentFault.Grade(key, $"<basis-unpublished-for-grade:{Key}:{grade.Key}>"))
        select force;
}

// The ACTIVE force basis as POLICY ROWS over ONE strand projection (kN) — the exact ForceBasis shape the passive bars
// already ride, so the two modalities read as one row family rather than a policy vocabulary beside three static
// methods. The three static *Kn methods and their external `strand.Authority == ComponentAuthority.En` equality
// branch are the DELETED form (the `kind == JointKind.Stud` shape joint#JOINT_FAMILY already rules out — encoded
// policy re-derived OUTSIDE its owner): the code-body jacking coefficient is now the ComponentAuthority row's own
// JackingProofFactor column, so a new prestressing authority is a COLUMN on that row, never an arm here. A body
// publishing no jacking rule yields None rather than a fabricated ceiling — the ForceBasis Option posture exactly.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TendonBasis {
    public static readonly TendonBasis Ultimate = new("ultimate", projectKn: static strand => Some(strand.UltimateMpa * strand.AreaMm2 * 1e-3));
    public static readonly TendonBasis Yield    = new("yield",    projectKn: static strand => Some(strand.ProofRatio * strand.UltimateMpa * strand.AreaMm2 * 1e-3));
    public static readonly TendonBasis Jacking  = new("jacking",  projectKn: static strand => strand.Authority.JackingProofFactor
        .Map(factor => Math.Min(JackingStressCeiling, factor * strand.ProofRatio) * strand.UltimateMpa * strand.AreaMm2 * 1e-3));

    // The 0.80·fpu ceiling both codes share (ACI 318 Table 20.3.2.5.1, EC2 §5.10.2.1) — the shared bound is the
    // owner's const; only the proof-stress coefficient differs by body.
    const double JackingStressCeiling = 0.80;

    [UseDelegateFromConstructor] public partial Option<double> ForceKn(StrandRow strand);
}

// The certification-read relaxation loss stays a two-argument projection on the RelaxationClass row: it reads the
// 1000 h certification datum against an already-chosen initial force, never a force BASIS. The time-dependent loss
// SCHEDULE (creep/shrinkage/temperature interaction) is the forward Compute prestress fold's.
public static class TendonForce {
    public static double RelaxationLoss1000hKn(StrandRow strand, double initialForceKn) =>
        strand.Relaxation.Rho1000Percent / 100.0 * initialForceKn;
}

// The seed-time realization bag (DetailLane.Realization). Beyond the bar's own identity it publishes the BEND
// SCHEDULE: the BS 8666 shape code every placed bar carries, and — where the placement declares a hook — the ACI
// inside-bend diameter, the floored straight extension, and the EN mandrel diameter, as one complex row. That block
// is what a bar-bending schedule is bought with, so the RebarSchedule receipt reaches the shop document instead of
// resolving to typed geometry nothing consumes. A STRAIGHT bar publishes shape code 00 and no bend block at all,
// which is why the deliverable's bend columns are an optional extension rather than a gate.
public static class RebarDetail {
    public static Fin<PropertyBag> Of(BarRow bar, RebarUsage usage, Option<RebarBend> bend, Provenance source, Op key) =>
        from joint in ComponentDetail.Joint("Cast", key)
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, bar.NominalDiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, bar.NominalAreaMm2 * 1e-6)
        from bendRows in bend.Match(Some: b => BendRow(b).Map(Some), None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.RealizationRows([
            joint,
            ComponentDetail.Token(DetailSchema.BarType, usage.IfcPredefinedType),
            ComponentDetail.Token(DetailSchema.BendShapeCode, bend.Map(static b => b.Shape.Key).IfNone(ShapeCodes.Straight.Key)),
            ComponentDetail.Sourced(source),
            diameter,
            area,
            .. bendRows.ToSeq(),
        ]);

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

// The tendon realization bag: the same dimension-only NominalDiameter/CrossSectionArea mints the bar bag carries plus
// the Joint("Cast") row — a strand realizes cast-in like a bar. Assembly is the POST-TENSIONED superset: the same
// strand rows plus the three Element-declared PT stamps — AnchorageType off the anchorage row's kind,
// TendonProfile off the drape vocabulary, and DuctDiameter ONLY where the duct row's inner diameter is Some
// (typed-absent today), riding BESIDE the strand's own NominalDiameter because the assembly carries both facts
// as two rows exactly as the Element schema declares. A pre-tensioned strand keeps the plain Of — no duct, no
// anchorage token, no fabricated hardware row.
public static class TendonDetail {
    public static Fin<PropertyBag> Of(StrandRow strand, Provenance source, Op key) =>
        from joint in ComponentDetail.Joint("Cast", key)
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, strand.NominalDiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, strand.AreaMm2 * 1e-6)
        select ComponentDetail.RealizationRows(joint, ComponentDetail.Sourced(source), diameter, area);

    public static Fin<PropertyBag> Assembly(StrandRow strand, AnchorageRow anchorage, DuctRow duct, TendonProfileKind profile, Provenance source, Op key) =>
        from joint in ComponentDetail.Joint("Cast", key)
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, strand.NominalDiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, strand.AreaMm2 * 1e-6)
        from ductRow in duct.InnerDiameterMm.Match(
            Some: mm => ComponentDetail.Measured(DetailSchema.DuctDiameter, Dimension.LengthDim, mm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.RealizationRows([
            joint,
            ComponentDetail.Token(DetailSchema.AnchorageType, anchorage.Kind.Key),
            ComponentDetail.Token(DetailSchema.TendonProfile, profile.Key),
            ComponentDetail.Sourced(source),
            diameter,
            area,
            .. ductRow.ToSeq(),
        ]);
}

// The realized-placement seed: SYMBOLIC BarRow/RebarGradeRow/usage/surface row references — a typo'd bar or grade is
// a compile miss, never a runtime key fault — plus the optional hook policy a bent bar declares. The designation is
// DERIVED from the row's own currencies rather than hand-spelled beside them: twenty-seven authored strings restating
// the bar, grade, and usage already in the row were twenty-seven chances for a name to disagree with the thing it
// names, and the derivation makes that disagreement unrepresentable. The Bars × Grades × RebarUsage space is the
// generator's domain; this table is the realized SELECTION.
public readonly record struct PlacementRow(BarRow Bar, RebarGradeRow Grade, RebarUsage Usage, RebarSurface Surface, Option<(HookKind Kind, RebarHook Hook)> Bend = default) {
    public string Designation =>
        Usage == RebarUsage.Main
            ? $"reinforcement.rebar-{Bar.Key}-{Grade.Key}"
            : $"reinforcement.rebar-{Bar.Key}-{Grade.Key}-{Usage.Key}";
}

public readonly record struct TendonRow(StrandRow Strand) {
    public string Designation => $"reinforcement.{Strand.Key}";
}

public static class ReinforcementSeed {
    // Both modalities transcribe a bar/strand table whole — the ASTM A615/A706 and EN 10080 size and area columns, the
    // ASTM A416 / EN 10138-3 seven-wire line — so one provenance covers the realized selection; the per-COLUMN
    // vendor/defined splits the Bars and Grades tables carry are facts of those tables, not of the placed row.
    static readonly Provenance Tabulated = Provenance.Published;

    // The realized selection spans every USAGE the vocabulary declares, so the eleven-token IfcReinforcingBarTypeEnum
    // roster the page advertises is a roster the catalogue actually reaches rather than nine tokens no row selects.
    // A longitudinal bar takes the development bend table; a tie, stirrup, or ring takes the stirrup-tie table and
    // schedules as a closed link — the correspondence StandardHook proves before it emits a receipt.
    static readonly Option<(HookKind, RebarHook)> DevelopmentHook = Some((HookKind.Development, RebarHook.Ninety));
    static readonly Option<(HookKind, RebarHook)> SeismicHook = Some((HookKind.Seismic, RebarHook.OneThirtyFive));
    static readonly Option<(HookKind, RebarHook)> TieHook = Some((HookKind.StirrupTie, RebarHook.OneThirtyFive));

    static readonly ImmutableArray<PlacementRow> Placements = [
        new(Bars.No3,  Grades.Gr40,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No3,  Grades.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.Main,        RebarSurface.Textured, DevelopmentHook),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.Ligature,    RebarSurface.Textured, TieHook),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.Shear,       RebarSurface.Textured, TieHook),
        new(Bars.No5,  Grades.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No5,  Grades.Gr60,   RebarUsage.Edge,        RebarSurface.Textured, DevelopmentHook),
        new(Bars.No6,  Grades.Gr60,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No6,  Grades.Gr60,   RebarUsage.Ring,        RebarSurface.Textured, TieHook),
        new(Bars.No7,  Grades.Gr75,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No8,  Grades.Gr75,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No8,  Grades.Gr75,   RebarUsage.Anchoring,   RebarSurface.Textured, DevelopmentHook),
        new(Bars.No9,  Grades.Gr80,   RebarUsage.Main,        RebarSurface.Textured, SeismicHook),
        new(Bars.No11, Grades.Gr80,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No18, Grades.Gr80,   RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No5,  Grades.Gr60W,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No5,  Grades.Gr60W,  RebarUsage.Punching,    RebarSurface.Textured),
        new(Bars.No8,  Grades.Gr80W,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.No3,  Grades.Gr60,   RebarUsage.Spacer,      RebarSurface.Plain),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.Stud,        RebarSurface.Textured),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.UserDefined, RebarSurface.Textured),
        new(Bars.No4,  Grades.Gr60,   RebarUsage.NotDefined,  RebarSurface.Plain),
        new(Bars.M10,  Grades.Gr400W, RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.M10,  Grades.Gr400W, RebarUsage.Ligature,    RebarSurface.Textured, TieHook),
        new(Bars.M15,  Grades.Gr400W, RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.M25,  Grades.Gr500W, RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.M35,  Grades.Gr500W, RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.H8,   Grades.B500A,  RebarUsage.Ligature,    RebarSurface.Plain,    TieHook),
        new(Bars.H12,  Grades.B500B,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.H14,  Grades.B500B,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.H16,  Grades.B500C,  RebarUsage.Main,        RebarSurface.Textured, SeismicHook),
        new(Bars.H25,  Grades.B500C,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.H32,  Grades.B500C,  RebarUsage.Main,        RebarSurface.Textured),
        new(Bars.H40,  Grades.B500C,  RebarUsage.Main,        RebarSurface.Textured)];

    // The ONE generator arm: the Admits size-system guard rails the typed ComponentFault case (row references are
    // symbolic, so no key can miss); the diameter lifts ONCE through the railed SectionProfile.Circle.Of; the
    // IfcBinding and the RebarDetail bag are seed-computed — every reinforcement row flows Component.Of ->
    // ComponentRow under the roster's own provenance, no second construction path.
    static Fin<ComponentRow> RebarOf(PlacementRow r) {
        Op key = Op.Of(name: r.Designation);
        return
            from admitted in guard(r.Grade.Admits(r.Bar), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{r.Grade.Key}:{r.Bar.Key}>"))
            from bend in r.Bend.Match(
                Some: policy => RebarSchedule.StandardHook(r.Bar, r.Usage, policy.Kind, policy.Hook, key).Map(Some),
                None: static () => Fin.Succ(Option<RebarBend>.None))
            from profile in SectionProfile.Circle.Of(r.Bar.NominalDiameterMm, key)
            from detail in RebarDetail.Of(r.Bar, r.Usage, bend, Tabulated, key)
            from item in Component.Of(
                ComponentFamily.Reinforcement, r.Designation, profile,
                IfcBinding.Of("IfcReinforcingBar", r.Usage.IfcPredefinedType),
                Coring.None, r.Grade.Standard.Component, r.Grade.Substance, r.Grade.Appearance,
                detail: Some(detail), key)
            select new ComponentRow(item, Tabulated);
    }

    // The realized tendon selection — strand components in the SAME family (a strand is a Circle-profiled reinforcing
    // part, never a new family row), the IfcTendon STRAND wire the row's own binding.
    static readonly ImmutableArray<TendonRow> Tendons = [
        new(Strands.S13Gr1725),
        new(Strands.S13Gr1860),
        new(Strands.S15Gr1860),
        new(Strands.Y1860S7D125),
        new(Strands.Y1860S7D157)];

    static Fin<ComponentRow> TendonOf(TendonRow r) {
        Op key = Op.Of(name: r.Designation);
        return
            from profile in SectionProfile.Circle.Of(r.Strand.NominalDiameterMm, key)
            from detail in TendonDetail.Of(r.Strand, Tabulated, key)
            from item in Component.Of(
                ComponentFamily.Reinforcement, r.Designation, profile,
                IfcBinding.Of("IfcTendon", "STRAND"),
                Coring.None, new ComponentStandard(r.Strand.Region, StandardJointThicknessMm: 0.0, Authority: r.Strand.Authority),
                r.Strand.Substance, MaterialId.Of("metal.steel"),
                detail: Some(detail), key)
            select new ComponentRow(item, Tabulated);
    }

    // Fail-loud, and BOTH modalities in ONE applicative pass: the passive and active selections are INDEPENDENT
    // traversals, so their faults accumulate together and a malformed bar and a malformed tendon both name themselves
    // in one build abort. Binding the second traversal to the first made the tendon table unreachable the moment any
    // bar faulted, which reports half a catalogue's defects per run and hides the rest behind a fix. The Context
    // parameter is the ComponentFamily.Rows delegate contract; the seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        (toSeq(Placements).Traverse(RebarOf).As(), toSeq(Tendons).Traverse(TendonOf).As())
            .Apply(static (bars, tendons) => bars + tendons).As();

    // The ComponentFamily.Reinforcement CAPACITY producer is an EXPLICIT TYPED REFUSAL, not silence: a bar and a
    // strand carry no section capacity of their own — a bar's structural participation is the member it reinforces,
    // and `RcSectionBuilder.Capacity` is that route, built and resolved in one entry. Binding the refusal is what
    // makes that route the ONLY one and keeps the family axis compiler-forced.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<reinforcement-capacity-rides-rc-section:{component.Designation.Value}>");
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the reinforced-concrete receipt over the `VividOrange.Sections` `IConcreteSection` AND the held `ConcreteSectionProperties` transformed-section carrier; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the EN-grade admission boundary lowering the `VividOrange.Materials` derivation throws onto the typed `ComponentFault` rail; `RcSectionBuilder` the one assembler minting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`) · `FaceSpacing` (max-spacing bars on a face) · `PerimeterCount` (n bars round the whole section — `PerimeterReinforcementLayer`, no face) · `PerimeterSpacing` · `Placed` (one bar at an explicit Y-Z section-plane station — `LongitudinalReinforcement`, the engine ingress the four rule-driven cases structurally cannot express)} — the face cases over the `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`; NO `Perimeter` member — perimeter distribution is the separate engine, never a face value); a bar arrangement is a `RebarLayout` case, never a scattered layer constructor; a stirrup is the `Link` promoted once from the same `RebarOf` bar the layouts use.
- Entry: `public static Fin<RcSection> RcSectionBuilder.Of(Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the grades through `EnGrade.Concrete`/`Rebar` (a non-EN `barGrade.EnGrade == None` railing `ComponentFault.Grade`), proves the link AND every layout bar against the ONE `RebarGradeRow.Admits` standard-consistency law (a standard mismatch railing the same typed grade fault the seed fold rails), builds the `ConcreteSection` from the FAMILY-AGNOSTIC profile-faithful `SectionSolver.ProfileOf(concrete.Profile, key)` `IProfile` + the concrete `IMaterial` + the promoted `Link` + the `coverMm` `Length`, folds each `RebarLayout` case to its placement engine through `AddRebarLayer`, and constructs the `ConcreteSectionProperties` carrier ONCE (eager-forced at the boundary) onto the receipt; `public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key)` reads the EC2 clear-spacing rule with `MaximumAggregateSize` SET so the `+(d_g + k2)` aggregate term is live — one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `MinimumReinforcementSpacing` with the settable `MaximumAggregateSize`/`AdditionalAggregateFactor`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`CrossSectionalShearReinforcementArea`/`ReinforcementSecondMomentOfAreaYy`/`Zz`/`EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)`; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial`, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex`; `.api/api-vividorange-standards.md`), VividOrange.Profiles (`IProfile` via `component#SECTION_SOLVER` `ProfileOf`), UnitsNet (`Length` cover/diameter/aggregate at the edge), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Fin`/`Seq`/`Try`).
- Growth: a new rebar arrangement is one `RebarLayout` case binding its engine ingress through `RebarPlacement` (the generated `Switch` breaks every dispatch site at compile time); a new tendon force basis is one `TendonBasis` row and a new prestressing authority one `ComponentAuthority.JackingProofFactor` column, never a branch; a new transformed-section read is one projection on the `RcSection` receipt over the held carrier; a new constitutive concrete law is a `capacity#SECTION_CAPACITY` concern over the same `IConcreteSection` — never a per-arrangement builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a re-summed bar area where `ConcreteSectionProperties` carries it.
- Boundary: `RcSectionBuilder.Of` is the BOUNDARY_ADMISSION point where the `VividOrange` throwing surface is admitted EXACTLY ONCE — grade-derivation throws lower onto `ComponentFault.Grade`, section/layout/property construction throws onto `ComponentFault.Section` (`ComponentFault.Capacity` RESERVED for the `capacity#SECTION_CAPACITY` SOLVE) — and the receipt egress carries only validated DATA: the transformed-section columns are `ConcreteSectionProperties` reads coerced to SI-mm scalars at the receipt surface (`GrossSteelAreaMm2` from `TotalReinforcementArea` — the hand `Σ π/4·d²·count` bar loop is the deleted form; `ReinforcementRatio` from `GeometricReinforcementRatio`; `ShearLinkAreaMm2` from `CrossSectionalShearReinforcementArea`; `ReinforcementInertiaYyMm4`/`ZzMm4`; the face queries `EffectiveDepthMm(SectionFace)`/`FaceSteelAreaMm2(SectionFace)` OPTIONED because the engine's face read throws/NaNs on a bar-less face — absence, never a sentinel) so no `UnitsNet` quantity crosses an interior signature, while the full elastic transformed-section stress state and the N-M-M hull stay the `capacity#SECTION_CAPACITY` owner's over the SAME `IConcreteSection`; the link bar promotes through `new Link(RebarOf(link, material))` off the same `CatalogueKey` Match every layout bar uses — the prior `LinkDiameter` `IfNone(BarDiameter.D8)` silent 8 mm default for a non-EN link is the deleted swallow (a #4 tie now feeds its true 12.7 mm `Length`); `RcSectionBuilder.Of` admits ANY `component#COMPONENT_OWNER` `Component` as its concrete outline because `SectionSolver.ProfileOf` switches the closed `SectionProfile` axis regardless of family, PROFILE-FAITHFUL (a circular drilled shaft feeds its true `ICircle`, a trapezoidal member its integrated perimeter, a `cmu#CMU_FAMILY` grouted unit its gross rectangle — the fully-grouted net solid IS the gross) — the cmu unit admits as the reinforced-masonry concrete input through this ONE boundary, its grout `EnConcreteGrade` the section concrete, no cmu-specific builder; the RC section is NOT a `Component` — a `Component` is one discrete bar in the schedule, the `RcSection` the populated member it reinforces, meeting at the `BarRow`/`RebarGradeRow` currencies this page owns; `RcSection.ConcreteProfile` carries the source `Component` (the QTO key + the gross lever the `capacity#SECTION_CAPACITY` `RcElastic` fibre arm reads).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
// Same Rasm.Materials.Component namespace as the section-02 fence; composes its prelude plus the
// VividOrange RC surface below.
using VividOrange.Sections;                          // ConcreteSection, IConcreteSection, SectionFace
using VividOrange.Sections.Reinforcement;            // Rebar, Link, LongitudinalReinforcement, FaceReinforcementLayer, PerimeterReinforcementLayer, MinimumReinforcementSpacing, IReinforcementLayer, ILongitudinalReinforcement
using VividOrange.Geometry;                          // LocalPoint2d — the engine's Y-Z SECTION-plane point a Placed bar stations at (never an X-Y pair)
using VividOrange.Sections.SectionProperties;        // ConcreteSectionProperties (the lazily-memoizing transformed-section carrier held on the receipt)
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteMaterial, EnRebarMaterial, EnConcreteGrade
using VividOrange.Profiles;                          // IProfile (the concrete-outline perimeter SectionSolver.ProfileOf mints)
using VividOrange.Standards.Eurocode;                // NationalAnnex
using UnitsNet;                                      // Length (cover / diameter / aggregate at the edge)

// --- [MODELS] ------------------------------------------------------------------------------
// One RebarLayout [Union] collapses the four VividOrange.Sections layout-engine constructors — face/perimeter ×
// count/spacing — each case carrying the BarRow currency, never four scattered `new ...Layer(...)` sites, PLUS the
// explicitly PLACED bar the engine's own LongitudinalReinforcement(IRebar, ILocalPoint2d) surface admits and the four
// engine cases structurally cannot express: a corner-bundled column, a haunched beam's staggered chord, any
// asymmetric arrangement whose stations are the design input rather than a rule's output. The coordinates are the
// engine's Y-Z SECTION plane (LocalPoint2d carries {Length Y; Length Z;}, never an X-Y pair).
[Union]
public abstract partial record RebarLayout {
    private RebarLayout() { }
    public sealed record FaceCount(SectionFace Face, BarRow Bar, int Count) : RebarLayout;
    public sealed record FaceSpacing(SectionFace Face, BarRow Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record PerimeterCount(BarRow Bar, int Count) : RebarLayout;
    public sealed record PerimeterSpacing(BarRow Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record Placed(BarRow Bar, double YMm, double ZMm) : RebarLayout;
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
// and the capacity#SECTION_CAPACITY solvers read these, never a re-derived bar-area sum. The full elastic stress state
// (RcElastic) and the N-M-M hull stay capacity#SECTION_CAPACITY's over the same Section.
public sealed record RcSection(
    IConcreteSection Section, ConcreteSectionProperties Properties,
    EnConcreteMaterial Concrete, EnRebarMaterial Rebar, Option<double> LinkYieldMpa, double CoverMm,
    Component ConcreteProfile, FrozenSet<SectionFace> BarredFaces) {

    public double GrossSteelAreaMm2 => Properties.TotalReinforcementArea.SquareMillimeters;          // As — the .Utility Rebars kernel behind the carrier
    public double ConcreteAreaMm2 => Properties.ConcreteArea.SquareMillimeters;                       // Ac (gross minus steel)
    public double ReinforcementRatio => Properties.GeometricReinforcementRatio.DecimalFractions;      // ρ = As/Ac
    public double ShearLinkAreaMm2 => Properties.CrossSectionalShearReinforcementArea.SquareMillimeters;   // Asw — both link legs
    public double ReinforcementInertiaYyMm4 => Properties.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth;
    public double ReinforcementInertiaZzMm4 => Properties.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth;

    // The face-keyed reads are OPTIONED, and the discriminant is STRUCTURAL: the admitted layout names exactly which
    // faces carry bars, so a face outside that set answers absence without asking the engine — whose own
    // CalculateEffectiveDepth divides a face-layer centroid by an area that is zero there and answers a throw or a
    // NaN. Trapping that throw turned a known-empty query into an exception round trip and, worse, made every OTHER
    // failure inside the same call indistinguishable from a bar-less face. The set is computed once at the boundary
    // from the layout the builder already proved, so both reads are total over it.
    public Option<double> EffectiveDepthMm(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.EffectiveDepth(face).Millimeters) : None;   // d to the face's tension steel
    public Option<double> FaceSteelAreaMm2(SectionFace face) =>
        BarredFaces.Contains(face) ? Some(Properties.ReinforcementArea(face).SquareMillimeters) : None;

    // The PLACED bars the layout engines materialized — the section collects them by walking every layer through its
    // own GetPath/GetRebars pair, so the placement OUTPUT this page's layout algebra produces is readable rather than
    // computed and discarded inside the engine. A rebar detailer and the QTO seam both read this set.
    public IReadOnlyList<ILongitudinalReinforcement> PlacedBars => Section.Rebars;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary: the VividOrange.Materials ctors throw on an unknown grade / untabulated annex (the
// .api/api-vividorange-materials.md [LOCAL_ADMISSION]); EnGrade traps the throw ONCE onto the typed ComponentFault.Grade rail — no VividOrange
// throw and no non-EN grade reaches the builder interior.
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        Try.lift(() => new EnConcreteMaterial(grade, annex)).Run()
            .MapFail(e => ComponentFault.Grade(key, $"<en-concrete-grade:{grade}:{annex}:{e.Message}>"));

    public static Fin<EnRebarMaterial> Rebar(Option<EnRebarGrade> grade, NationalAnnex annex, Op key) =>
        grade.Match(
            Some: g => Try.lift(() => new EnRebarMaterial(g, annex)).Run()
                .MapFail(e => ComponentFault.Grade(key, $"<en-rebar-grade:{g}:{annex}:{e.Message}>")),
            None: () => Fin.Fail<EnRebarMaterial>(ComponentFault.Grade(key, "<rebar-grade-not-en-bodied-for-rc-section>")));
}

public static class RcSectionBuilder {
    // The ONE reinforced-section boundary: admit the raw cover scalar (UnitsNet accepts a negative/NaN Length silently,
    // so the guard is load-bearing), lower the grades, prove the link and every layout bar against RebarGradeRow.Admits,
    // mint the family-agnostic PROFILE-FAITHFUL IProfile through SectionSolver.ProfileOf(concrete.Profile, key), build
    // the ConcreteSection + layers, then construct the ConcreteSectionProperties carrier and EAGER-FORCE its first read
    // so any degenerate-section throw surfaces HERE, never on an interior receipt read.
    public static Fin<RcSection> Of(Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from cover in guard(double.IsFinite(coverMm) && coverMm >= 0.0, ComponentFault.Dimension(key, $"<cover-negative-or-nonfinite:{coverMm:R}>"))
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade.EnGrade, annex, key)
        from linkAdmitted in guard(barGrade.Admits(link), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{barGrade.Key}:{link.Key}>"))
        from admittedLayout in layout.Traverse(item => ValidateLayout(item, barGrade, key)).As()
        from profile in SectionSolver.ProfileOf(concrete.Profile, key)
        from built in Try.lift(() => Build(profile, concreteMaterial, rebarMaterial, link, admittedLayout, coverMm)).Run()
            .MapFail(e => ComponentFault.Section(key, $"<rc-section-build:{concrete.Family.Key}:{e.Message}>"))
        from properties in Try.lift(() => { ConcreteSectionProperties p = new(built); _ = p.TotalReinforcementArea; return p; }).Run()   // boundary kernel: the eager force is the platform-forced seam
            .MapFail(e => ComponentFault.Section(key, $"<rc-transformed-properties:{concrete.Designation.Value}:{e.Message}>"))
        // LinkYieldMpa is the link grade's CHARACTERISTIC f_yk off the admitted grade row — Option because a
        // grade without a published yield declares absence, and the capacity screen's V_Rd,s publication then
        // stays absent rather than riding a fabricated yield.
        select new RcSection(built, properties, concreteMaterial, rebarMaterial, barGrade.PublishedYieldMpa, coverMm, concrete, BarredFacesOf(admittedLayout));

    // The faces the ADMITTED layout actually places bars on — the face cases name their own face and the perimeter
    // and placed cases name none, so the set is a read of the layout rather than a probe of the engine.
    static FrozenSet<SectionFace> BarredFacesOf(Seq<RebarLayout> layout) =>
        layout.Choose(static item => item.Switch(
            faceCount:        static c => Some(c.Face),
            faceSpacing:      static s => Some(s.Face),
            perimeterCount:   static _ => Option<SectionFace>.None,
            perimeterSpacing: static _ => Option<SectionFace>.None,
            placed:           static _ => Option<SectionFace>.None)).ToFrozenSet();

    // Every layout bar proves the SAME RebarGradeRow.Admits standard-consistency law the seed fold runs — the one
    // owner, so an EN grade can never mint an A615/G30 layout bar through the builder — then its shape admits.
    // The ADMITTED layout rides back out, so the builder consumes what the gate proved rather than the caller's own
    // unproven sequence — a Unit-returning gate leaves the two one rename apart and the proof decorative.
    static Fin<RebarLayout> ValidateLayout(RebarLayout layout, RebarGradeRow grade, Op key) =>
        from admitted in guard(grade.Admits(BarOf(layout)), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{grade.Key}:{BarOf(layout).Key}>"))
        from shape in layout.Switch(
            faceCount: item => item.Count > 0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-face-count-nonpositive:{item.Count}>")),
            faceSpacing: item => double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-face-spacing-invalid:{item.MaxSpacingMm:R}>")),
            perimeterCount: item => item.Count > 0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-perimeter-count-nonpositive:{item.Count}>")),
            perimeterSpacing: item => double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-perimeter-spacing-invalid:{item.MaxSpacingMm:R}>")),
            // A placed bar carries COORDINATES, so both section-plane offsets prove finite — UnitsNet accepts a NaN
            // Length silently, and a NaN station would egress as a bar the engine places nowhere.
            placed: item => double.IsFinite(item.YMm) && double.IsFinite(item.ZMm)
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-station-nonfinite:{item.YMm:R}:{item.ZMm:R}>")))
        select layout;

    static BarRow BarOf(RebarLayout layout) => layout.Switch(
        faceCount:        static c => c.Bar,
        faceSpacing:      static s => s.Bar,
        perimeterCount:   static c => c.Bar,
        perimeterSpacing: static s => s.Bar,
        placed:           static p => p.Bar);

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
    // imperial/CSA row its exact raw Length — the prior link-only IfNone(BarDiameter.D8) 8 mm default is the deleted
    // swallow (a #4 tie now carries its true 12.7 mm).
    static Rebar RebarOf(BarRow bar, EnRebarMaterial rebar) =>
        bar.CatalogueKey.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(bar.NominalDiameterMm)));

    // Each RebarLayout case -> its engine ingress; the generated [Union] Switch is the totality proof — a sixth case
    // breaks this arm at compile time, never a runtime-silent `_`. A rule-driven case mints its placement ENGINE, the
    // placed case its positioned bar at the engine's own Y-Z section-plane point.
    static RebarPlacement PlacementOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => RebarPlacement.CreateLayer(new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count)),
        faceSpacing:      s => RebarPlacement.CreateLayer(new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        perimeterCount:   c => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count)),
        perimeterSpacing: s => RebarPlacement.CreateLayer(new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm))),
        placed:           p => RebarPlacement.CreatePlaced(new LongitudinalReinforcement(
                                   RebarOf(p.Bar, rebar), new LocalPoint2d(Length.FromMillimeters(p.YMm), Length.FromMillimeters(p.ZMm)))));

    // The EC2 clear bar-spacing rule with the aggregate term LIVE: MaximumAggregateSize is a settable rule property, so
    // the (d_g + k2) branch participates — the prior signature accepted maxAggregateMm and never used it (the deleted
    // dead knob). The aggregate scalar is admitted first: UnitsNet accepts a NaN Length silently, so an unguarded NaN
    // egresses as a Succ(NaN) spacing. Never an inline EC2 constant.
    // The REINFORCEMENT-SIDE ENTRY into the capacity rail. capacity#SECTION_CAPACITY declares the RcInteraction hull
    // and the RcElastic transformed-section surfaces over an RcSection input and owns their solve; this is where that
    // input is BUILT and handed over, so the two RC cases have the producer this family owes them and no consumer
    // assembles a concrete section for itself. The bar family's own Capacity stays a typed refusal because a single
    // bar has no section capacity at all — its structural participation is the member it reinforces, which is
    // precisely this entry.
    public static Fin<SectionCapacity> Capacity(
        Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link,
        Seq<RebarLayout> layout, double coverMm, CapacityBuild build, CapacityPlacement placement, Op key) =>
        from section in Of(concrete, concreteGrade, barGrade, link, layout, coverMm, placement.Annex, key)
        from capacity in SectionCapacity.Resolve(section, build, key)
        select capacity;

    public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key) =>
        from aggregate in guard(double.IsFinite(maxAggregateMm) && maxAggregateMm > 0.0, ComponentFault.Dimension(key, $"<aggregate-nonpositive-or-nonfinite:{maxAggregateMm:R}>"))
        from spacing in Try.lift(() => new MinimumReinforcementSpacing(annex) { MaximumAggregateSize = Length.FromMillimeters(maxAggregateMm) }
                .GetMinimumReinforcementSpacing(Length.FromMillimeters(bar.NominalDiameterMm)).Millimeters).Run()
            .MapFail(e => ComponentFault.Section(key, $"<min-bar-spacing:{annex}:{bar.Key}:{e.Message}>"))
        select spacing;
}
```

## [04]-[RESEARCH]

(none)
