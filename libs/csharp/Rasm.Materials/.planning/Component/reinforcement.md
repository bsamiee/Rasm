# [MATERIALS_REINFORCEMENT]

THE REINFORCING-BAR SEED FAMILY and THE HOST-NEUTRAL REINFORCED-CONCRETE-SECTION ASSEMBLER. A #5 rebar is one `ComponentRow` minted by the ONE generator `ReinforcementSeed.Rows -> Component.Of` over the `ComponentFamily.Reinforcement` policy row (`ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Circle`, cross-nominal the circle diameter, `Sectioned: false` — a bar contributes no `ComputedSection`, its RC participation riding `[03]-[RC_SECTION]`), never a `Rebar` type and never a bespoke `RebarSection` payload: the geometry is `SectionProfile.Circle(DiameterMm)`, the IFC stamp is `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` computed at seed time from the eleven-token `RebarUsage` vocabulary, and the realization detail is the seed-built `RebarDetail.Of` bag whose rows are byte-identical to the retired projector switch. Under `SEED_ROW_LAW` the pure standards-data vocabularies convert 1:1 to frozen row tables with per-column provenance — `Bars` (31 `BarRow`: imperial #3..#18 and CSA 10M..55M PUBLISHED verbatim with `CatalogueKey: None`, EN H6..H50 VENDOR-keyed to the `VividOrange.ISections` `BarDiameter` D6..D50 catalogue, the EN 10080 `πd²/4` area and `A·ρ` weight DEFINED fallbacks guarding any unprinted column), `Grades` (11 `RebarGradeRow`: ASTM A615 Gr40..Gr80, A706 Gr60W/Gr80W, CSA G30.18 400W/500W, EN 10080 B500A/B/C bound to their `EnRebarGrade`), `ShapeCodes` (the 37-code BS 8666:2020 schedule set), and `Strands` (the ACTIVE modality — ASTM A416 / EN 10138-3 seven-wire prestressing rows with their `RelaxationClass` certification data and the `TendonForce` ultimate/jacking/relaxation projections, seeded as `IfcTendon` `STRAND` component rows through the same generator) — while the policy vocabularies with delegate or IFC-token behavior STAY `[SmartEnum<string>]`: `RebarStandard` (spec body carrying weldability, the size system it rolls, and the `ComponentStandard` projection), `RebarUsage` (`IfcReinforcingBarTypeEnum`), `RebarSurface` (`IfcReinforcingBarSurfaceEnum`), `RibPattern` (ISO 6935-2 §4.15 β), `HookKind` (the ACI 318-19 §25.3 bend-table delegate), and `RebarHook` (the standard-hook angle rows anchored to their `ShapeCodeRow`). `RebarSchedule` keeps the deleted payload's algebra as operations over the row currencies — the ISO 6935-2 `RebarRibGeometry` projection and the ACI/EN/BS `StandardHook` bend receipt — while the `ForceBasis` policy rows own the ONE bar×grade schedule-force projection (spec-nominal, `EnRebarFactory` registered characteristic yield, ductility-class ultimate — a new basis is one row). `[03]-[RC_SECTION]` is the family-agnostic assembler: `RcSectionBuilder.Of` lowers EN grades through the `EnGrade` boundary, builds the `VividOrange.Sections` `ConcreteSection` over the PROFILE-FAITHFUL `SectionSolver.ProfileOf(concrete.Profile, key)` `IProfile` of ANY `Component` (an RC beam its true outline — a circular column its `ICircle` — a grouted `cmu` unit its gross rectangle), folds the `RebarLayout` `[Union]` through the four collapsed placement engines, and mints the `RcSection` receipt whose transformed-section columns read the `ConcreteSectionProperties` carrier — `TotalReinforcementArea`, `GeometricReinforcementRatio`, `EffectiveDepth(SectionFace)`, `CrossSectionalShearReinforcementArea`, `ReinforcementSecondMomentOfAreaYy/Zz` — never a hand-summed bar loop; the `IConcreteSection` egress feeds the `capacity#SECTION_CAPACITY` `ConcreteSectionProperties` elastic and `InteractionDiagram` N-M-M solvers. Growth is one row: a new bar one `BarRow`, a new grade one `RebarGradeRow`, a new role one `RebarUsage` row, a new schedule shape one `ShapeCodeRow`, a new placed bar one `Placements` row through the same generator — zero type edits, zero central edits.

## [01]-[INDEX]

- [02]-[REINFORCEMENT_FAMILY]: the retained policy SmartEnums (`RebarStandard` · `RebarUsage` · `RebarSurface` · `RibPattern` · `HookKind` · `RebarHook` · `RelaxationClass`), the tier-3 frozen row tables (`Bars` 31 · `Grades` 11 · `ShapeCodes` 37 · the `Strands` seven-wire prestressing line) with per-column `VENDOR`/`DEFINED`/`PUBLISHED` provenance, the `RebarRibGeometry`/`RebarBend` receipts, the `RebarSchedule` rib/hook algebra plus the `ForceBasis` force-policy rows and the `TendonForce` ultimate/jacking/relaxation projections, the seed-time `RebarDetail.Of`/`TendonDetail.Of` realization bags, and the fail-loud `ReinforcementSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Reinforcement` policy row binds.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete assembler — the `RebarLayout` `[Union]` over the four `VividOrange.Sections` placement engines, the `EnGrade` EN-grade admission boundary, `RcSectionBuilder.Of` over the family-agnostic `SectionSolver.ProfileOf` concrete outline, the `ConcreteSectionProperties`-backed transformed-section receipt columns, and the EC2 `MinimumReinforcementSpacing` rule with the aggregate term wired.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: `RebarStandard`/`RebarUsage`/`RebarSurface`/`RibPattern`/`HookKind`/`RebarHook` the retained `[SmartEnum<string>]` policy vocabularies; `BarRow`/`RebarGradeRow`/`ShapeCodeRow` the tier-3 row currencies with `Bars`/`Grades`/`ShapeCodes` the frozen tables; `RebarRibGeometry`/`RebarBend` the receipts; `RebarSchedule` the rib/hook operation owner; `ForceBasis` the schedule-force policy rows; `RebarDetail` the seed-time realization-bag constructor; `ReinforcementSeed` the `Rows` fold the `component#COMPONENT_OWNER` `ComponentFamily.Reinforcement` policy row binds.
- Cases: grade {A615 Gr40/Gr60/Gr75/Gr80 (carbon, non-weldable) · A706 Gr60W/Gr80W (low-alloy, weldable) · G30.18 400W/500W (CSA metric, weldable) · EN 10080 B500A/B500B/B500C (the ductility classes the `EnRebarFactory.CreateBiLinear` k = 1.05/1.08/1.15 branches read)} × size {#3..#11, #14, #18 imperial · 10M..55M CSA · H6..H50 EN keyed `BarDiameter.D6`..`D50`} × usage {main · ligature · shear · punching · edge · ring · anchoring · spacer · stud · userdefined · notdefined — the full verified 11-member `IfcReinforcingBarTypeEnum`} × surface {textured · plain} × rib-pattern {uniform-height 90° · crescent 60°} × hook {90°/135°/180° over development/stirrup-tie/seismic ACI tables} × shape-code {the BS 8666:2020 37-code set} — a bar is one `Placements` row over one `BarRow` and one `RebarGradeRow`, the standard-consistency law `RebarGradeRow.Admits` (a grade admits only the bar rows its spec body rolls) enforced BEFORE construction.
- Entry: `public static Fin<Seq<ComponentRow>> ReinforcementSeed.Rows(Context context)` traverses `Placements` through the common `Component.Of` rail. `RebarSchedule.StandardHook(BarRow, RebarUsage, HookKind, RebarHook, Op)` rejects a longitudinal usage paired with a tie/seismic hook policy and a transverse usage paired with the development policy before emitting the ACI/EN/BS bend receipt.
- Packages: Rasm.Numerics (project — `PositiveMagnitude` the `>0` finite magnitude every admitted diameter column lifts into), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `PropertyBag`/`DetailSchema`/`Dimension` the detail bag composes), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue the EN `BarRow` rows VENDOR-key; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnRebarGrade` the EN-bodied binding, `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` the registered yield + ductility ultimate; `.api/api-vividorange-materials.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, `[UseDelegateFromConstructor]` the `HookKind` bend delegate), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`/`guard`), BCL inbox (`FrozenDictionary`, `ImmutableArray`, collection expressions).
- Growth: one row per new fact — a new bar size one `BarRow` (printed diameter/area/weight PUBLISHED, or the DEFINED `πd²/4`/`A·ρ` fallbacks), a new grade one `RebarGradeRow` bound to its `EnRebarGrade` when EN-bodied, a new role one `RebarUsage` row carrying its verified token, a new schedule shape one `ShapeCodeRow`, a new hook table one `HookKind` row with its bend delegate, a new realized bar one `Placements` row; a new strand diameter or grade one `StrandRow`, a new relaxation certification one `RelaxationClass` row, a new realized tendon one `Tendons` row — never a per-bar type, never a `ComponentFamily` edit, never a central edit. A welded mesh grows as an `IfcReinforcingMesh` projection over the same row currencies, never an eleventh family; anchorage and duct hardware is vendor-certified data outside the published-table provenance.
- Boundary: the seed admits raw standards data once through the symbolic `Bars`/`Grades` row references, `RebarGradeRow.Admits`, and the `SectionProfile.Circle.Of` rail. `StandardHook` validates the `usage.Stirrup`/`HookKind` correspondence, and `ForceBasis.ForceKn` validates the bar/grade system before any registered-grade projection. The IFC role remains `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)`, and the independent substance and appearance identifiers remain grade-carried.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                                // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                                 // Op (the boundary-admission key), AcceptValidated, Context
using Rasm.Element.Composition;                                // MaterialId; PropertyBag/DetailSchema/Dimension (the seam detail-bag currencies RebarDetail composes)
using Rasm.Element.Properties;
using Thinktecture;
using VividOrange.Sections.Reinforcement;          // BarDiameter (the EN-10080 D6..D50 catalogue the EN BarRow rows VENDOR-key)
using VividOrange.Materials.StandardMaterials.En;  // EnRebarGrade, EnRebarFactory (the registered yield + ductility-class ultimate)
using static LanguageExt.Prelude;                  // guard, Seq, Some/None

// Every family page declares in the ONE Rasm.Materials.Component namespace; component#COMPONENT_OWNER binds
// ReinforcementSeed.Rows by bare name on the ComponentFamily.Reinforcement policy row (the <Family>Seed naming keeps rows collision-free).
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The spec body — the FORM-law survivor carrying behavior columns: weldability, the ComponentStandard projection (the
// retired StandardOf 3-arm switch, now a row read), and Rolls the SIZE SYSTEM the body's mills roll (A706 bars roll at the
// A615 imperial sizes — the RebarGradeRow.Admits standard-consistency predicate reads it, evaluated at call time so no static
// self-reference exists).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615    = new("astm-a615",  weldable: false, authority: "ASTM A615/A615M",              body: ComponentAuthority.Astm, region: "us");
    public static readonly RebarStandard A706    = new("astm-a706",  weldable: true,  authority: "ASTM A706/A706M",              body: ComponentAuthority.Astm, region: "us");
    public static readonly RebarStandard G30     = new("csa-g30.18", weldable: true,  authority: "CSA G30.18",                   body: ComponentAuthority.Csa,  region: "ca");
    public static readonly RebarStandard En10080 = new("en-10080",   weldable: true,  authority: "EN 1992-1-1 / EN 10080 / ISO 6935-2", body: ComponentAuthority.En, region: "eu");
    public bool Weldable { get; }
    public string Authority { get; }
    public ComponentAuthority Body { get; }
    public string Region { get; }
    public RebarStandard Rolls => this == A706 ? A615 : this;                                       // the size-defining spec whose BarRow rows this body rolls
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
    public static readonly RelaxationClass Normal        = new("normal",         rho1000Percent: 8.0);   // EN 10138 Class 1 — the stress-relieved legacy class
    public double Rho1000Percent { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// SEED_ROW_LAW tier-3 currencies: pure standards data as frozen readonly-record-struct rows with per-column provenance.
// BarRow — CatalogueKey VENDOR (Some only for the EN H-series, the BarDiameter the Rebar(IMaterial, BarDiameter) ctor
// consumes; imperial/CSA None so the exact non-EN nominal feeds a raw Length); the printed diameter/area/weight PUBLISHED
// and never re-derived; NominalAreaMm2/NominalWeightKgM the DEFINED EN 10080 πd²/4 and A·ρ fallbacks guarding only an
// unprinted column. Standard is the size-defining spec body (imperial -> A615, CSA -> G30, EN -> En10080).
public readonly record struct BarRow(string Key, RebarStandard Standard, Option<BarDiameter> CatalogueKey,
    double NominalDiameterMm, double PublishedAreaMm2, double PublishedWeightKgM) {
    const double SteelDensityKgM3 = 7850.0;                        // DEFINED: EN 10080 nominal mass density
    public double NominalAreaMm2 => PublishedAreaMm2 > 0.0 ? PublishedAreaMm2
        : Math.PI * NominalDiameterMm * NominalDiameterMm / 4.0;   // DEFINED: EN 10080 πd²/4
    public double NominalWeightKgM => PublishedWeightKgM > 0.0 ? PublishedWeightKgM
        : NominalAreaMm2 * 1e-6 * SteelDensityKgM3;                // DEFINED fallback: A·ρ — the safety net, never the primary for printed values
}

// The grade row: spec-nominal yield (PUBLISHED), the spec body, the two INDEPENDENT MaterialId slots (SubstanceId the
// per-grade steel.<designation> Mechanical row carrying f_yk + the ACI 318 §20.2.2.2 E_s 200 GPa — NEVER the generic
// metal.steel 235 MPa / 210 GPa section baseline; AppearanceId the render finish), and the Option<EnRebarGrade> binding
// (Some only for the EN-bodied B500A/B/C the RC σ(ε) law and the ForceBasis EN rows read). Admits is the
// standard-consistency law: a grade admits only the bar rows its body rolls — the explicit exclusion predicate the seed
// guards BEFORE construction.
public readonly record struct RebarGradeRow(string Key, double MinimumYieldMpa, RebarStandard Standard,
    string SubstanceId, string AppearanceId, Option<EnRebarGrade> EnGrade) {
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
    public bool Weldable => Standard.Weldable;
    public bool Admits(BarRow bar) => Standard.Rolls == bar.Standard;
}

// The seven-wire prestressing strand row — the ACTIVE reinforcement modality beside the passive bars: the printed
// nominal diameter, cross-section area, and ultimate strength fpu PUBLISHED verbatim (ASTM A416/A416M Table 1 for the
// imperial grades, EN 10138-3 for Y1860S7), ProofRatio the printed yield-to-ultimate ratio (A416 low-relaxation
// fpy = 0.90·fpu; EN Fp0,1/Fm = 0.88 off the printed force pair), the RelaxationClass certification datum, and the
// per-strand Mechanical substance slot. Weight is the DEFINED A·ρ derivation (no printed twin kept).
public readonly record struct StrandRow(string Key, ComponentAuthority Authority, string Region,
    double NominalDiameterMm, double AreaMm2, double UltimateMpa, double ProofRatio, RelaxationClass Relaxation, string SubstanceId) {
    const double SteelDensityKgM3 = 7850.0;
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public double NominalWeightKgM => AreaMm2 * 1e-6 * SteelDensityKgM3;
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

// The 11-row grade table. Yields are the spec-nominal PUBLISHED bands; the EN B500 bands bind their EnRebarGrade so the
// RC path reads the registered f_yk/k rather than a hand key. Named statics for the same symbolic-selection law.
public static class Grades {
    public static readonly RebarGradeRow Gr40   = new("gr40",  280.0, RebarStandard.A615,    "steel.gr40",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr60   = new("gr60",  420.0, RebarStandard.A615,    "steel.gr60",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr75   = new("gr75",  520.0, RebarStandard.A615,    "steel.gr75",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr80   = new("gr80",  550.0, RebarStandard.A615,    "steel.gr80",  "metal.iron",  None);
    public static readonly RebarGradeRow Gr60W  = new("gr60w", 420.0, RebarStandard.A706,    "steel.gr60w", "metal.steel", None);
    public static readonly RebarGradeRow Gr80W  = new("gr80w", 550.0, RebarStandard.A706,    "steel.gr80w", "metal.steel", None);
    public static readonly RebarGradeRow Gr400W = new("400w",  400.0, RebarStandard.G30,     "steel.400w",  "metal.steel", None);
    public static readonly RebarGradeRow Gr500W = new("500w",  500.0, RebarStandard.G30,     "steel.500w",  "metal.steel", None);
    public static readonly RebarGradeRow B500A  = new("b500a", 500.0, RebarStandard.En10080, "steel.b500a", "metal.steel", Some(EnRebarGrade.B500A));
    public static readonly RebarGradeRow B500B  = new("b500b", 500.0, RebarStandard.En10080, "steel.b500b", "metal.steel", Some(EnRebarGrade.B500B));
    public static readonly RebarGradeRow B500C  = new("b500c", 500.0, RebarStandard.En10080, "steel.b500c", "metal.steel", Some(EnRebarGrade.B500C));
    public static readonly ImmutableArray<RebarGradeRow> Rows = [Gr40, Gr60, Gr75, Gr80, Gr60W, Gr80W, Gr400W, Gr500W, B500A, B500B, B500C];
}

// The strand table, PUBLISHED verbatim (ASTM A416 Grade 250/270 printed area rows; EN 10138-3 Y1860S7 printed
// diameter/area rows) — named statics so a tendon placement references its row symbolically. The realized selection;
// a new diameter or grade is one row.
public static class Strands {
    public static readonly StrandRow S13Gr1725   = new("strand-1/2-gr250",  ComponentAuthority.Astm, "us", 12.70, 92.9,  1725.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1725");
    public static readonly StrandRow S13Gr1860   = new("strand-1/2-gr270",  ComponentAuthority.Astm, "us", 12.70, 98.7,  1860.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1860");
    public static readonly StrandRow S15Gr1860   = new("strand-0.6-gr270",  ComponentAuthority.Astm, "us", 15.24, 140.0, 1860.0, 0.90, RelaxationClass.LowRelaxation, "steel.strand-1860");
    public static readonly StrandRow Y1860S7D125 = new("y1860s7-12.5",      ComponentAuthority.En,   "eu", 12.50, 93.0,  1860.0, 0.88, RelaxationClass.LowRelaxation, "steel.y1860s7");
    public static readonly StrandRow Y1860S7D157 = new("y1860s7-15.7",      ComponentAuthority.En,   "eu", 15.70, 150.0, 1860.0, 0.88, RelaxationClass.LowRelaxation, "steel.y1860s7");
    public static readonly ImmutableArray<StrandRow> Rows = [S13Gr1725, S13Gr1860, S15Gr1860, Y1860S7D125, Y1860S7D157];
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
    // The load-bearing anchors the hook rows and the stirrup override read (declared after ByKey — textual init order).
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
    // ISO 6935-2 rib geometry — Some for a Textured bar, None for a plain round. fR by diameter band (the bond invariant
    // the development-length / fib Model Code bond law reads); spacing 0.7·d and gap 0.125 the ASTM A615 §7.4 maxima;
    // rib height 0.05·d the A615 Table 1 minimum-average ratio; flank 45° the ISO §4.14 minimum; β the RibPattern's §4.15 angle.
    public static Option<RebarRibGeometry> Ribs(BarRow bar, RebarSurface surface, RibPattern pattern) =>
        !surface.Ribbed ? None : bar.NominalDiameterMm switch {
            double d => Some(new RebarRibGeometry(
                TransverseRibHeightMm:    0.05 * d,
                TransverseRibSpacingMm:   0.7 * d,
                LongitudinalRibHeightMm:  0.05 * d,
                FlankInclinationDeg:      45.0,
                RibInclinationDeg:        pattern.InclinationDeg,
                RelativeRibArea:          d <= 6.0 ? 0.035 : d <= 12.0 ? 0.040 : 0.056,
                RiblessPerimeterFraction: 0.125,
                Pattern:                  pattern)),
        };

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
    public static readonly ForceBasis Nominal    = new("nominal",     projectKn: static (bar, grade) => Some(grade.MinimumYieldMpa * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnYield    = new("en-yield",    projectKn: static (bar, grade) => grade.EnGrade.Map(g => EnRebarFactory.CreateLinearElastic(g).Strength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnUltimate = new("en-ultimate", projectKn: static (bar, grade) => grade.EnGrade.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    [UseDelegateFromConstructor] private partial Option<double> ProjectKn(BarRow bar, RebarGradeRow grade);

    public Fin<Option<double>> ForceKn(BarRow bar, RebarGradeRow grade, Op key) =>
        guard(grade.Admits(bar), ComponentFault.Grade(key, $"<rebar-force-system-mismatch:{bar.Key}:{grade.Key}>"))
            .ToFin()
            .Map(_ => ProjectKn(bar, grade));
}

// The tendon force projections over the strand row — the active complement of ForceBasis, every rule DEFINED off the
// row's published columns: the ultimate and yield forces, the code-body jacking ceiling (ACI 318 Table 20.3.2.5.1
// min(0.80·fpu, 0.94·fpy) for the A416 rows; EC2 §5.10.2.1 σp,max = min(0.80·fpk, 0.90·fp0,1k) for the EN rows — the
// row's Authority selects the rule), and the 1000 h relaxation loss off the RelaxationClass certification datum. The
// time-dependent loss SCHEDULE (creep/shrinkage/temperature interaction) is the forward Compute prestress fold's.
public static class TendonForce {
    public static double UltimateKn(StrandRow strand) => strand.UltimateMpa * strand.AreaMm2 * 1e-3;

    public static double YieldKn(StrandRow strand) => strand.ProofRatio * strand.UltimateMpa * strand.AreaMm2 * 1e-3;

    public static double JackingLimitKn(StrandRow strand) =>
        (strand.Authority == ComponentAuthority.En
            ? Math.Min(0.80, 0.90 * strand.ProofRatio)
            : Math.Min(0.80, 0.94 * strand.ProofRatio)) * strand.UltimateMpa * strand.AreaMm2 * 1e-3;

    public static double RelaxationLoss1000hKn(StrandRow strand, double initialForceKn) =>
        strand.Relaxation.Rho1000Percent / 100.0 * initialForceKn;
}

// The seed-time realization bag (DetailLane.Realization) composing the component#COMPONENT_DETAIL ComponentDetail
// constructors — the SAME four rows the retired projector switch built (Joint("Cast") + the BarType usage token + the
// dimension-only mm->SI NominalDiameter/CrossSectionArea mints), so the PropertyBag content and the projected
// Node.PropertySet bytes are identical. Weight/rib/bend realization stays typed page algebra (RebarSchedule), never a
// bag row — a new bag row is a wire-visible content-key change.
public static class RebarDetail {
    public static Fin<PropertyBag> Of(BarRow bar, RebarUsage usage) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, bar.NominalDiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, bar.NominalAreaMm2 * 1e-6)
        select ComponentDetail.RealizationRows(
            ComponentDetail.Joint("Cast"),
            ComponentDetail.Token(DetailSchema.BarType, usage.IfcPredefinedType),
            diameter,
            area);
}

// The tendon realization bag: the same dimension-only NominalDiameter/CrossSectionArea mints the bar bag carries plus
// the Joint("Cast") row — a strand realizes cast-in like a bar; the anchorage/duct hardware is vendor-certified data
// outside this table's provenance.
public static class TendonDetail {
    public static Fin<PropertyBag> Of(StrandRow strand) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, strand.NominalDiameterMm * 1e-3)
        from area in ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, strand.AreaMm2 * 1e-6)
        select ComponentDetail.RealizationRows(ComponentDetail.Joint("Cast"), diameter, area);
}

// The realized-placement seed: designation verbatim (the wire-stable ComponentId) plus SYMBOLIC BarRow/RebarGradeRow/
// usage/surface row references — a typo'd bar or grade is a compile miss, never a runtime key fault. The
// Bars × Grades × RebarUsage space is the generator's domain; this table is the realized SELECTION — a new placed bar
// is one row, the standard-consistency exclusion the Admits guard, never a swallowed fault.
public readonly record struct PlacementRow(string Designation, BarRow Bar, RebarGradeRow Grade, RebarUsage Usage, RebarSurface Surface);

public readonly record struct TendonRow(string Designation, StrandRow Strand);

public static class ReinforcementSeed {
    static readonly ImmutableArray<PlacementRow> Placements = [
        new("reinforcement.rebar-no3-gr60",      Bars.No3,  Grades.Gr60,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no4-gr60",      Bars.No4,  Grades.Gr60,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no4-gr60-tie",  Bars.No4,  Grades.Gr60,   RebarUsage.Ligature, RebarSurface.Textured),
        new("reinforcement.rebar-no5-gr60",      Bars.No5,  Grades.Gr60,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no6-gr60",      Bars.No6,  Grades.Gr60,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no7-gr75",      Bars.No7,  Grades.Gr75,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no8-gr75",      Bars.No8,  Grades.Gr75,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no9-gr80",      Bars.No9,  Grades.Gr80,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no11-gr80",     Bars.No11, Grades.Gr80,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no18-gr80",     Bars.No18, Grades.Gr80,   RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no5-gr60w",     Bars.No5,  Grades.Gr60W,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no8-gr80w",     Bars.No8,  Grades.Gr80W,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-10m-400w",      Bars.M10,  Grades.Gr400W, RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-10m-400w-tie",  Bars.M10,  Grades.Gr400W, RebarUsage.Ligature, RebarSurface.Textured),
        new("reinforcement.rebar-15m-400w",      Bars.M15,  Grades.Gr400W, RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-25m-500w",      Bars.M25,  Grades.Gr500W, RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-35m-500w",      Bars.M35,  Grades.Gr500W, RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h8-b500a-link", Bars.H8,   Grades.B500A,  RebarUsage.Ligature, RebarSurface.Plain),
        new("reinforcement.rebar-h12-b500b",     Bars.H12,  Grades.B500B,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h14-b500b",     Bars.H14,  Grades.B500B,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h16-b500c",     Bars.H16,  Grades.B500C,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h25-b500c",     Bars.H25,  Grades.B500C,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h32-b500c",     Bars.H32,  Grades.B500C,  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h40-b500c",     Bars.H40,  Grades.B500C,  RebarUsage.Main,     RebarSurface.Textured)];

    // The ONE generator arm: the Admits size-system guard rails the typed ComponentFault case (row references are
    // symbolic, so no key can miss); the diameter lifts ONCE through the railed SectionProfile.Circle.Of; the
    // IfcBinding and the RebarDetail bag are seed-computed — every reinforcement row flows Component.Of ->
    // ComponentRow(Sectioned: false), no second construction path.
    static Fin<ComponentRow> RebarOf(PlacementRow r) {
        Op key = Op.Of(name: r.Designation);
        return
            from admitted in guard(r.Grade.Admits(r.Bar), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{r.Grade.Key}:{r.Bar.Key}>"))
            from profile in SectionProfile.Circle.Of(r.Bar.NominalDiameterMm, key)
            from detail in RebarDetail.Of(r.Bar, r.Usage)
            from item in Component.Of(
                ComponentFamily.Reinforcement, r.Designation, profile,
                IfcBinding.Of("IfcReinforcingBar", r.Usage.IfcPredefinedType),
                Coring.None, r.Grade.Standard.Component, r.Grade.Substance, r.Grade.Appearance,
                detail: Some(detail), key)
            select new ComponentRow(item, Sectioned: false);
    }

    // The realized tendon selection — strand components in the SAME family (a strand is a Circle-profiled reinforcing
    // part; family stays closed at ten), the IfcTendon STRAND wire the row's own binding.
    static readonly ImmutableArray<TendonRow> Tendons = [
        new("reinforcement.strand-13-gr1860",    Strands.S13Gr1860),
        new("reinforcement.strand-15-gr1860",    Strands.S15Gr1860),
        new("reinforcement.strand-y1860s7-15.7", Strands.Y1860S7D157)];

    static Fin<ComponentRow> TendonOf(TendonRow r) {
        Op key = Op.Of(name: r.Designation);
        return
            from profile in SectionProfile.Circle.Of(r.Strand.NominalDiameterMm, key)
            from detail in TendonDetail.Of(r.Strand)
            from item in Component.Of(
                ComponentFamily.Reinforcement, r.Designation, profile,
                IfcBinding.Of("IfcTendon", "STRAND"),
                Coring.None, new ComponentStandard(r.Strand.Region, StandardJointThicknessMm: 0.0, Authority: r.Strand.Authority),
                r.Strand.Substance, MaterialId.Of("metal.steel"),
                detail: Some(detail), key)
            select new ComponentRow(item, Sectioned: false);
    }

    // Fail-loud: one Traverse per selection, a faulted row ABORTS the catalogue build — the .Choose/.ToOption swallow is
    // the deleted form. The Context parameter is the ComponentFamily.Rows delegate contract; the seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Placements.ToSeq().Traverse(RebarOf).As()
            .Bind(bars => Tendons.ToSeq().Traverse(TendonOf).As().Map(tendons => bars + tendons));
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the reinforced-concrete receipt over the `VividOrange.Sections` `IConcreteSection` PLUS the held `ConcreteSectionProperties` transformed-section carrier; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the EN-grade admission boundary lowering the `VividOrange.Materials` derivation throws onto the typed `ComponentFault` rail; `RcSectionBuilder` the one assembler minting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`) · `FaceSpacing` (max-spacing bars on a face) · `PerimeterCount` (n bars round the whole section — `PerimeterReinforcementLayer`, no face) · `PerimeterSpacing`} — the face cases over the `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`; NO `Perimeter` member — perimeter distribution is the separate engine, never a face value); a bar arrangement is a `RebarLayout` case, never a scattered layer constructor; a stirrup is the `Link` promoted once from the same `RebarOf` bar the layouts use.
- Entry: `public static Fin<RcSection> RcSectionBuilder.Of(Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the grades through `EnGrade.Concrete`/`Rebar` (a non-EN `barGrade.EnGrade == None` railing `ComponentFault.Grade`), proves the link AND every layout bar against the ONE `RebarGradeRow.Admits` standard-consistency law (a standard mismatch railing the same typed grade fault the seed fold rails), builds the `ConcreteSection` from the FAMILY-AGNOSTIC profile-faithful `SectionSolver.ProfileOf(concrete.Profile, key)` `IProfile` + the concrete `IMaterial` + the promoted `Link` + the `coverMm` `Length`, folds each `RebarLayout` case to its placement engine through `AddRebarLayer`, and constructs the `ConcreteSectionProperties` carrier ONCE (eager-forced at the boundary) onto the receipt; `public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key)` reads the EC2 clear-spacing rule with `MaximumAggregateSize` SET so the `+(d_g + k2)` aggregate term is live — one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `MinimumReinforcementSpacing` with the settable `MaximumAggregateSize`/`AdditionalAggregateFactor`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`CrossSectionalShearReinforcementArea`/`ReinforcementSecondMomentOfAreaYy`/`Zz`/`EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)`; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial`, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex`; `.api/api-vividorange-standards.md`), VividOrange.Profiles (`IProfile` via `component#SECTION_SOLVER` `ProfileOf`), UnitsNet (`Length` cover/diameter/aggregate at the edge), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Fin`/`Seq`/`Try`).
- Growth: a new rebar arrangement is one `RebarLayout` case binding its placement engine (the generated `Switch` breaks every dispatch site at compile time); a new transformed-section read is one projection on the `RcSection` receipt over the held carrier; a new constitutive concrete law is a `capacity#SECTION_CAPACITY` concern over the same `IConcreteSection` — never a per-arrangement builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a re-summed bar area where `ConcreteSectionProperties` carries it.
- Boundary: `RcSectionBuilder.Of` is the BOUNDARY_ADMISSION point where the `VividOrange` throwing surface is admitted EXACTLY ONCE — grade-derivation throws lower onto `ComponentFault.Grade`, section/layout/property construction throws onto `ComponentFault.Section` (`ComponentFault.Capacity` RESERVED for the `capacity#SECTION_CAPACITY` SOLVE) — and the receipt egress carries only validated DATA: the transformed-section columns are `ConcreteSectionProperties` reads coerced to SI-mm scalars at the receipt surface (`GrossSteelAreaMm2` from `TotalReinforcementArea` — the hand `Σ π/4·d²·count` bar loop is the deleted form; `ReinforcementRatio` from `GeometricReinforcementRatio`; `ShearLinkAreaMm2` from `CrossSectionalShearReinforcementArea`; `ReinforcementInertiaYyMm4`/`ZzMm4`; the face queries `EffectiveDepthMm(SectionFace)`/`FaceSteelAreaMm2(SectionFace)` OPTIONED because the engine's face read throws/NaNs on a bar-less face — absence, never a sentinel) so no `UnitsNet` quantity crosses an interior signature, while the full elastic transformed-section stress state and the N-M-M hull stay the `capacity#SECTION_CAPACITY` owner's over the SAME `IConcreteSection`; the link bar promotes through `new Link(RebarOf(link, material))` off the same `CatalogueKey` Match every layout bar uses — the prior `LinkDiameter` `IfNone(BarDiameter.D8)` silent 8 mm default for a non-EN link is the deleted swallow (a #4 tie now feeds its true 12.7 mm `Length`); `RcSectionBuilder.Of` admits ANY `component#COMPONENT_OWNER` `Component` as its concrete outline because `SectionSolver.ProfileOf` switches the closed `SectionProfile` axis regardless of family, PROFILE-FAITHFUL (a circular drilled shaft feeds its true `ICircle`, a trapezoidal member its integrated perimeter, a `cmu#CMU_FAMILY` grouted unit its gross rectangle — the fully-grouted net solid IS the gross) — the cmu unit admits as the reinforced-masonry concrete input through this ONE boundary, its grout `EnConcreteGrade` the section concrete, no cmu-specific builder; the RC section is NOT a `Component` — a `Component` is one discrete bar in the schedule, the `RcSection` the populated member it reinforces, meeting at the `BarRow`/`RebarGradeRow` currencies this page owns; `RcSection.ConcreteProfile` carries the source `Component` (the QTO key + the gross lever the `capacity#SECTION_CAPACITY` `RcElastic` fibre arm reads).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
// Same Rasm.Materials.Component namespace as the section-02 fence; composes its prelude plus the
// VividOrange RC surface below.
using VividOrange.Sections;                          // ConcreteSection, IConcreteSection, SectionFace
using VividOrange.Sections.Reinforcement;            // Rebar, Link, FaceReinforcementLayer, PerimeterReinforcementLayer, MinimumReinforcementSpacing, IReinforcementLayer
using VividOrange.Sections.SectionProperties;        // ConcreteSectionProperties (the lazily-memoizing transformed-section carrier held on the receipt)
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteMaterial, EnRebarMaterial, EnConcreteGrade
using VividOrange.Profiles;                          // IProfile (the concrete-outline perimeter SectionSolver.ProfileOf mints)
using VividOrange.Standards.Eurocode;                // NationalAnnex
using UnitsNet;                                      // Length (cover / diameter / aggregate at the edge)

// --- [MODELS] ------------------------------------------------------------------------------
// One RebarLayout [Union] collapses the four VividOrange.Sections layout-engine constructors — face/perimeter ×
// count/spacing — each case carrying the BarRow currency, never four scattered `new ...Layer(...)` sites.
[Union]
public abstract partial record RebarLayout {
    private RebarLayout() { }
    public sealed record FaceCount(SectionFace Face, BarRow Bar, int Count) : RebarLayout;
    public sealed record FaceSpacing(SectionFace Face, BarRow Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record PerimeterCount(BarRow Bar, int Count) : RebarLayout;
    public sealed record PerimeterSpacing(BarRow Bar, double MaxSpacingMm) : RebarLayout;
}

// The reinforced-concrete receipt: the assembled IConcreteSection, the ONE ConcreteSectionProperties carrier (lazy,
// memoizing — constructed and eager-forced at the boundary), the resolved EN grade DATA, the cover, and the source
// Component. The transformed-section columns are carrier reads coerced to SI-mm at the receipt surface — the QTO seam
// and the capacity#SECTION_CAPACITY solvers read these, never a re-derived bar-area sum. The full elastic stress state
// (RcElastic) and the N-M-M hull stay capacity#SECTION_CAPACITY's over the same Section.
public sealed record RcSection(
    IConcreteSection Section, ConcreteSectionProperties Properties,
    EnConcreteMaterial Concrete, EnRebarMaterial Rebar, double CoverMm, Component ConcreteProfile) {

    public double GrossSteelAreaMm2 => Properties.TotalReinforcementArea.SquareMillimeters;          // As — the .Utility Rebars kernel behind the carrier
    public double ConcreteAreaMm2 => Properties.ConcreteArea.SquareMillimeters;                       // Ac (gross minus steel)
    public double ReinforcementRatio => Properties.GeometricReinforcementRatio.DecimalFractions;      // ρ = As/Ac
    public double ShearLinkAreaMm2 => Properties.CrossSectionalShearReinforcementArea.SquareMillimeters;   // Asw — both link legs
    public double ReinforcementInertiaYyMm4 => Properties.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth;
    public double ReinforcementInertiaZzMm4 => Properties.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth;

    // The face-keyed reads are OPTIONED: the engine's CalculateEffectiveDepth divides a face-layer centroid by its
    // area and throws/NaNs for a face with no bars (a perimeter-only layout), so a bar-less face is ABSENCE — never
    // a NaN/throw escaping an interior receipt read. The boundary eager-force cannot pre-prove every face.
    public Option<double> EffectiveDepthMm(SectionFace face) =>
        Try.lift(() => Properties.EffectiveDepth(face).Millimeters).Run().ToOption().Filter(double.IsFinite);   // d to the face's tension steel
    public Option<double> FaceSteelAreaMm2(SectionFace face) =>
        Try.lift(() => Properties.ReinforcementArea(face).SquareMillimeters).Run().ToOption().Filter(static a => a > 0.0);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary: the VividOrange.Materials ctors throw on an unknown grade / untabulated annex (the
// .api [BOUNDARY_EXCEPTION_LAW]); EnGrade traps the throw ONCE onto the typed ComponentFault.Grade rail — no VividOrange
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
        select new RcSection(built, properties, concreteMaterial, rebarMaterial, coverMm, concrete);

    // Every layout bar proves the SAME RebarGradeRow.Admits standard-consistency law the seed fold runs — the one
    // owner, so an EN grade can never mint an A615/G30 layout bar through the builder — then its shape admits.
    static Fin<Unit> ValidateLayout(RebarLayout layout, RebarGradeRow grade, Op key) =>
        from admitted in guard(grade.Admits(BarOf(layout)), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{grade.Key}:{BarOf(layout).Key}>"))
        from shape in layout.Switch(
            faceCount: item => item.Count > 0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-face-count-nonpositive:{item.Count}>")),
            faceSpacing: item => double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-face-spacing-invalid:{item.MaxSpacingMm:R}>")),
            perimeterCount: item => item.Count > 0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-perimeter-count-nonpositive:{item.Count}>")),
            perimeterSpacing: item => double.IsFinite(item.MaxSpacingMm) && item.MaxSpacingMm > 0.0
                ? Fin.Succ(unit) : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<rebar-perimeter-spacing-invalid:{item.MaxSpacingMm:R}>")))
        select shape;

    static BarRow BarOf(RebarLayout layout) => layout.Switch(
        faceCount:        static c => c.Bar,
        faceSpacing:      static s => s.Bar,
        perimeterCount:   static c => c.Bar,
        perimeterSpacing: static s => s.Bar);

    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarRow link, Seq<RebarLayout> layout, double coverMm) {
        ConcreteSection section = new(profile, concrete, new Link(RebarOf(link, rebar)), Length.FromMillimeters(coverMm));
        layout.Iter(l => section.AddRebarLayer(LayerOf(l, rebar)));
        return section;
    }

    // ONE bar mint serves layouts AND the promoted link: an EN BarRow feeds the catalogued BarDiameter ctor, an
    // imperial/CSA row its exact raw Length — the prior link-only IfNone(BarDiameter.D8) 8 mm default is the deleted
    // swallow (a #4 tie now carries its true 12.7 mm).
    static Rebar RebarOf(BarRow bar, EnRebarMaterial rebar) =>
        bar.CatalogueKey.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(bar.NominalDiameterMm)));

    // Each RebarLayout case -> its placement engine; the generated [Union] Switch is the totality proof — a fifth case
    // breaks this arm at compile time, never a runtime-silent `_`.
    static IReinforcementLayer LayerOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count),
        faceSpacing:      s => new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)),
        perimeterCount:   c => (IReinforcementLayer)new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count),
        perimeterSpacing: s => new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)));

    // The EC2 clear bar-spacing rule with the aggregate term LIVE: MaximumAggregateSize is a settable rule property, so
    // the (d_g + k2) branch participates — the prior signature accepted maxAggregateMm and never used it (the deleted
    // dead knob). The aggregate scalar is admitted first: UnitsNet accepts a NaN Length silently, so an unguarded NaN
    // egresses as a Succ(NaN) spacing. Never an inline EC2 constant.
    public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key) =>
        from aggregate in guard(double.IsFinite(maxAggregateMm) && maxAggregateMm > 0.0, ComponentFault.Dimension(key, $"<aggregate-nonpositive-or-nonfinite:{maxAggregateMm:R}>"))
        from spacing in Try.lift(() => new MinimumReinforcementSpacing(annex) { MaximumAggregateSize = Length.FromMillimeters(maxAggregateMm) }
                .GetMinimumReinforcementSpacing(Length.FromMillimeters(bar.NominalDiameterMm)).Millimeters).Run()
            .MapFail(e => ComponentFault.Section(key, $"<min-bar-spacing:{annex}:{bar.Key}:{e.Message}>"))
        select spacing;
}
```

## [04]-[RESEARCH]

- [SEED_ROW_CONVERSION]: REALIZED — the `BarSize` (31), `ShapeCode` (37), and `RebarGrade` (11) SmartEnums converted 1:1 to the frozen `BarRow`/`ShapeCodeRow`/`RebarGradeRow` tables under `SEED_ROW_LAW` tier 3 with identical columns and per-column provenance: EN H-series `CatalogueKey` VENDOR (`BarDiameter.D6`..`D50` — the full catalogue roster bound, D8/D14/D28/D32/D40/D50 included), every printed diameter/area/weight PUBLISHED verbatim (ISO 6935-2 Table 2 for EN, ASTM A615 soft-metric for imperial, CSA G30.18 for metric — rounded printed values never re-derived), and the `πd²/4` area / `A·ρ` (7850 kg/m³) weight fallbacks DEFINED, guarding only an unprinted column. `RebarUsage`/`RebarSurface` (IFC-token), `RibPattern` (β policy), `HookKind` (the `[UseDelegateFromConstructor]` bend delegate), `RebarHook` (shape-anchored angle rows), and `RebarStandard` (weldability + `Rolls` size system + the `ComponentStandard` projection that deleted the `StandardOf` switch) STAY `[SmartEnum]` under the FORM law. `BarDiameter` breadth is EN-10080 D6..D50 ONLY — the imperial/CSA rows carry `None` and feed a raw `Length` at every consumer, never a catalogue approximation.
- [FAIL_LOUD_SEED]: REALIZED — `ReinforcementSeed.Rows : Context -> Fin<Seq<ComponentRow>>` is ONE `Traverse` over the 24-row `Placements` selection: every bar/grade/usage/surface column is a SYMBOLIC row reference to the `Bars`/`Grades` named statics (the fastener `Threads`/`Grades` form — a typo'd row is a compile miss, so the prior `ByKey` frozen indexes and their `<unknown-bar>`/`<unknown-grade>` runtime fault cases are deleted with the string columns that fed them), the `RebarGradeRow.Admits` size-system law (a grade admits only bar rows its `Standard.Rolls` body defines — A706 rolls the A615 imperial sizes) guards BEFORE construction, the diameter lifts once through the railed `SectionProfile.Circle.Of`, and every row constructs through the ONE generator `Component.Of` into `ComponentRow(item, Sectioned: false)` — the prior `BuildRebarRows` `.Choose(...).ToOption())` fault swallow that silently dropped a malformed row from the frozen dictionary is the deleted form; a faulted row now ABORTS `ComponentCatalogue.Of`. The seed designations are the verbatim wire-stable keys (`reinforcement.rebar-no5-gr60`, `reinforcement.rebar-10m-400w-tie`, `reinforcement.rebar-h8-b500a-link`).
- [DETAIL_BAG_BYTES]: REALIZED — the reinforcement realization detail is built AT SEED TIME by `RebarDetail.Of(bar, usage)` composing the `component#COMPONENT_DETAIL` `ComponentDetail` constructors with exactly the four rows the retired `Projection/component.md` `Detail(Component)` switch built — `Joint("Cast")`, the `BarType` token from `usage.IfcPredefinedType`, and the dimension-only `MeasureValue.OfSi` `NominalDiameter` (mm→m) and `CrossSectionArea` (mm²→m²) mints — so the `PropertyBag` content keys and the projected `Node.PropertySet` bytes are identical to the pre-campaign wire. Weight, rib geometry, and bend receipts stay TYPED page algebra (`RebarSchedule`) rather than bag rows because any added row is a wire-visible content-key change; the `ComponentFamily.Reinforcement` row is `DetailLane.Realization`, so `Component.Of`'s lane/detail totality law proves every seeded bar carries its bag.
- [STRAND_VOCABULARY]: REALIZED — the reinforcement axis carries the ACTIVE modality: `StrandRow`/`Strands` are the seven-wire prestressing line under `SEED_ROW_LAW` (ASTM A416 Grade 250/270 printed diameter/area/fpu rows, EN 10138-3 Y1860S7 printed rows — the realized selection, every value PUBLISHED verbatim; weight the DEFINED `A·ρ`), `RelaxationClass` the 1000 h certification datum (A416 low-relaxation / EN Class 2 at 2.5 %, EN Class 1 at 8 %), `TendonForce` the DEFINED force projections (ultimate, yield off the printed proof ratio, the code-body jacking ceiling — ACI 318 Table `20.3.2.5.1` vs EC2 §`5.10.2.1` selected by the row's `Authority` — and the ρ1000 relaxation loss; the time-dependent loss schedule is the forward Compute prestress fold's), and the tendon seeds mint `IfcTendon` `STRAND` component rows through the same generator with the `TendonDetail` bag. Anchorage and duct hardware is vendor-certified (VSL/Freyssinet ETA data) outside the published-table provenance — the open card names that gate.
- [SCHEDULE_ALGEBRA]: `RebarSchedule.Ribs` emits the ISO 6935-2 deformation receipt. `StandardHook(..., Op)` admits the `usage.Stirrup`/`HookKind` correspondence before emitting the ACI/EN/BS bend receipt, and `ForceBasis.ForceKn(..., Op)` admits the bar/grade system before selecting nominal or registered EN force.
- [RC_TRANSFORMED_RECEIPT]: REALIZED — `RcSection` holds the `ConcreteSectionProperties` carrier constructed and eager-forced ONCE at `RcSectionBuilder.Of`, and its columns are carrier reads: `GrossSteelAreaMm2` from `TotalReinforcementArea` (the `.Utility` `Rebars.CalculateArea` kernel behind the getter — the hand `Σ π/4·d²·CountPerBundle` loop is the deleted re-derivation), `ConcreteAreaMm2`, `ReinforcementRatio` from `GeometricReinforcementRatio`, `ShearLinkAreaMm2` from `CrossSectionalShearReinforcementArea` (both link legs), `ReinforcementInertiaYyMm4`/`ZzMm4` from `ReinforcementSecondMomentOfAreaYy`/`Zz`, and the face queries `EffectiveDepthMm(SectionFace)`/`FaceSteelAreaMm2(SectionFace)` — the scalar columns coerced to SI-mm at the receipt surface, the face queries `Option<double>` because the engine's `CalculateEffectiveDepth` throws/NaNs for a face with no bars (a perimeter-only layout): a bar-less face is absence, never a NaN or throw escaping an interior read, and no `UnitsNet` quantity crosses an interior signature. The full elastic combined-stress state (`RcElastic`) and the `InteractionDiagram` N-M-M hull remain `capacity#SECTION_CAPACITY` owners over the SAME `IConcreteSection`; this receipt carries the reinforcement-side geometry facts the QTO seam and the capacity lift read without re-solving.
- [RC_BOUNDARY_REBIND]: REALIZED — `RcSectionBuilder.Of` rebinds the retired `ParametricSection.ProfileOf` to `component#SECTION_SOLVER` `SectionSolver.ProfileOf(concrete.Profile, key)`, the PROFILE-FAITHFUL polymorphic entry (one generated total Switch over the closed `SectionProfile` axis — the width×depth bounding-rectangle spelling is the deleted narrowing that lowered every non-rectangular RC outline to its bounding box), staying FAMILY-AGNOSTIC — a `cmu#CMU_FAMILY` grouted unit admits as the reinforced-masonry concrete input through this one boundary (its grout `EnConcreteGrade` the section concrete), no cmu-specific builder. The link mints through the SAME `RebarOf` catalogue-or-raw-`Length` Match as every layout bar and promotes via `new Link(IRebar)` — the prior `LinkDiameter` `Catalogue.IfNone(BarDiameter.D8)` silently rebuilt a non-EN link at 8 mm (a #4 tie under-sized by 37%), the deleted swallow. `MinimumBarSpacingMm` now SETS the rule's `MaximumAggregateSize` so the EC2 `(d_g + k2)` aggregate branch is live — the prior signature carried `maxAggregateMm` as a dead parameter. The two raw boundary scalars are ADMITTED before any `UnitsNet` lift (`coverMm` non-negative finite, `maxAggregateMm` positive finite — `Length.FromMillimeters` accepts a NaN silently, so an unguarded value egresses as a `Succ(NaN)`). Fault split holds: grade derivation → `ComponentFault.Grade`, section/layout/property construction → `ComponentFault.Section`, `ComponentFault.Capacity` reserved for the capacity SOLVE; band integers ride the `FaultBand.Component` registry row.
- [IFC_REINFORCING_WIRE]: REALIZED — each seeded bar stamps `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` at seed time over the FULL verified 11-member `IfcReinforcingBarTypeEnum` (`MAIN`/`LIGATURE`/`SHEAR`/`PUNCHING`/`EDGE`/`RING`/`ANCHORING`/`SPACEBAR`/`STUD`/`USERDEFINED`/`NOTDEFINED`), never a flat per-row string; the bond surface rides `RebarSurface.IfcSurface` (`IfcReinforcingBarSurfaceEnum` {`PLAIN`, `TEXTURED`}), the schedule designation `RebarBend.Shape.Key` (the `IfcReinforcingBar` `BendingShapeCode`), and the grade token `RebarGradeRow.Key` (`SteelGrade`) on the `Rasm.Bim` egress mapping; a mesh round-trips as `IfcReinforcingMesh` over the same row columns. Validation is Bim's: the strings here stay neutral, composition-time `IfcLegality` and egress-time `AdmitPredefined` gate them against the generated roster.
