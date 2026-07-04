# [MATERIALS_REINFORCEMENT]

THE REINFORCING-BAR SEED FAMILY and THE HOST-NEUTRAL REINFORCED-CONCRETE-SECTION ASSEMBLER. A #5 rebar is one `ComponentRow` minted by the ONE generator `ReinforcementSeed.Rows -> Component.Of` over the `ComponentFamily.Reinforcement` policy row (`ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Circle`, cross-nominal the circle diameter, `Sectioned: false` — a bar contributes no `ComputedSection`, its RC participation riding `[03]-[RC_SECTION]`), never a `Rebar` type and never a bespoke `RebarSection` payload: the geometry is `SectionProfile.Circle(DiameterMm)`, the IFC stamp is `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` computed at seed time from the eleven-token `RebarUsage` vocabulary, and the realization detail is the seed-built `RebarDetail.Of` bag whose rows are byte-identical to the retired projector switch. Under `SEED_ROW_LAW` the pure standards-data vocabularies convert 1:1 to frozen row tables with per-column provenance — `Bars` (31 `BarRow`: imperial #3..#18 and CSA 10M..55M PUBLISHED verbatim with `CatalogueKey: None`, EN H6..H50 VENDOR-keyed to the `VividOrange.ISections` `BarDiameter` D6..D50 catalogue, the EN 10080 `πd²/4` area and `A·ρ` weight DEFINED fallbacks guarding any unprinted column), `Grades` (11 `RebarGradeRow`: ASTM A615 Gr40..Gr80, A706 Gr60W/Gr80W, CSA G30.18 400W/500W, EN 10080 B500A/B/C bound to their `EnRebarGrade`), and `ShapeCodes` (the 37-code BS 8666:2020 schedule set) — while the policy vocabularies with delegate or IFC-token behavior STAY `[SmartEnum<string>]`: `RebarStandard` (spec body carrying weldability, the size system it rolls, and the `ComponentStandard` projection), `RebarUsage` (`IfcReinforcingBarTypeEnum`), `RebarSurface` (`IfcReinforcingBarSurfaceEnum`), `RibPattern` (ISO 6935-2 §4.15 β), `HookKind` (the ACI 318-19 §25.3 bend-table delegate), and `RebarHook` (the standard-hook angle rows anchored to their `ShapeCodeRow`). `RebarSchedule` keeps the deleted payload's algebra as operations over the row currencies — the ISO 6935-2 `RebarRibGeometry` projection and the ACI/EN/BS `StandardHook` bend receipt — while the `ForceBasis` policy rows own the ONE bar×grade schedule-force projection (spec-nominal, `EnRebarFactory` registered characteristic yield, ductility-class ultimate — a new basis is one row). `[03]-[RC_SECTION]` is the family-agnostic assembler: `RcSectionBuilder.Of` lowers EN grades through the `EnGrade` boundary, builds the `VividOrange.Sections` `ConcreteSection` over the `SectionSolver.ProfileOf` gross-rectangle `IProfile` of ANY `Component` (`concrete.Profile.GrossRectangleMm` — an RC beam, a grouted `cmu` unit), folds the `RebarLayout` `[Union]` through the four collapsed placement engines, and mints the `RcSection` receipt whose transformed-section columns read the `ConcreteSectionProperties` carrier — `TotalReinforcementArea`, `GeometricReinforcementRatio`, `EffectiveDepth(SectionFace)`, `CrossSectionalShearReinforcementArea`, `ReinforcementSecondMomentOfAreaYy/Zz` — never a hand-summed bar loop; the `IConcreteSection` egress feeds the `capacity#SECTION_CAPACITY` `ConcreteSectionProperties` elastic and `InteractionDiagram` N-M-M solvers. Growth is one row: a new bar one `BarRow`, a new grade one `RebarGradeRow`, a new role one `RebarUsage` row, a new schedule shape one `ShapeCodeRow`, a new placed bar one `Placements` row through the same generator — zero type edits, zero central edits.

## [01]-[INDEX]

- [02]-[REINFORCEMENT_FAMILY]: the retained policy SmartEnums (`RebarStandard` · `RebarUsage` · `RebarSurface` · `RibPattern` · `HookKind` · `RebarHook`), the tier-3 frozen row tables (`Bars` 31 · `Grades` 11 · `ShapeCodes` 37) with per-column `VENDOR`/`DEFINED`/`PUBLISHED` provenance, the `RebarRibGeometry`/`RebarBend` receipts, the `RebarSchedule` rib/hook algebra plus the `ForceBasis` force-policy rows, the seed-time `RebarDetail.Of` realization bag, and the fail-loud `ReinforcementSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Reinforcement` policy row binds.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete assembler — the `RebarLayout` `[Union]` over the four `VividOrange.Sections` placement engines, the `EnGrade` EN-grade admission boundary, `RcSectionBuilder.Of` over the family-agnostic `SectionSolver.ProfileOf` concrete outline, the `ConcreteSectionProperties`-backed transformed-section receipt columns, and the EC2 `MinimumReinforcementSpacing` rule with the aggregate term wired.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: `RebarStandard`/`RebarUsage`/`RebarSurface`/`RibPattern`/`HookKind`/`RebarHook` the retained `[SmartEnum<string>]` policy vocabularies; `BarRow`/`RebarGradeRow`/`ShapeCodeRow` the tier-3 row currencies with `Bars`/`Grades`/`ShapeCodes` the frozen tables; `RebarRibGeometry`/`RebarBend` the receipts; `RebarSchedule` the rib/hook operation owner; `ForceBasis` the schedule-force policy rows; `RebarDetail` the seed-time realization-bag constructor; `ReinforcementSeed` the `Rows` fold the `component#COMPONENT_OWNER` `ComponentFamily.Reinforcement` policy row binds.
- Cases: grade {A615 Gr40/Gr60/Gr75/Gr80 (carbon, non-weldable) · A706 Gr60W/Gr80W (low-alloy, weldable) · G30.18 400W/500W (CSA metric, weldable) · EN 10080 B500A/B500B/B500C (the ductility classes the `EnRebarFactory.CreateBiLinear` k = 1.05/1.08/1.15 branches read)} × size {#3..#11, #14, #18 imperial · 10M..55M CSA · H6..H50 EN keyed `BarDiameter.D6`..`D50`} × usage {main · ligature · shear · punching · edge · ring · anchoring · spacer · stud · userdefined · notdefined — the full verified 11-member `IfcReinforcingBarTypeEnum`} × surface {textured · plain} × rib-pattern {uniform-height 90° · crescent 60°} × hook {90°/135°/180° over development/stirrup-tie/seismic ACI tables} × shape-code {the BS 8666:2020 37-code set} — a bar is one `Placements` row over one `BarRow` and one `RebarGradeRow`, the standard-consistency law `RebarGradeRow.Admits` (a grade admits only the bar rows its spec body rolls) enforced BEFORE construction.
- Entry: `public static Fin<Seq<ComponentRow>> ReinforcementSeed.Rows(Context context)` — the ONE generator: `Placements.Traverse(RebarOf)` resolves each seed row's bar/grade keys against the frozen tables, guards `RebarGradeRow.Admits`, lifts the diameter once into `PositiveMagnitude`, and constructs through `SectionProfile.Circle.Of` -> `Component.Of` with the seed-computed `IfcBinding` and the seed-built `RebarDetail.Of` bag — a faulted row ABORTS the catalogue build (`Traverse`, never a `.Choose` swallow); `public static RebarBend RebarSchedule.StandardHook(BarRow bar, RebarUsage usage, HookKind kind, RebarHook hook)` — the ACI 318-19 §25.3 bend + EN 1992 §8.3 mandrel + BS 8666:2020 shape-code projection, TOTAL over the frozen rows, one polymorphic entry, never a per-table hook family.
- Packages: Rasm.Numerics (project — `PositiveMagnitude` the `>0` finite magnitude every admitted diameter column lifts into), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `PropertyBag`/`DetailSchema`/`Dimension` the detail bag composes), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue the EN `BarRow` rows VENDOR-key; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnRebarGrade` the EN-bodied binding, `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` the registered yield + ductility ultimate; `.api/api-vividorange-materials.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, `[UseDelegateFromConstructor]` the `HookKind` bend delegate), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`/`guard`), BCL inbox (`FrozenDictionary`, `ImmutableArray`, collection expressions).
- Growth: one row per new fact — a new bar size one `BarRow` (printed diameter/area/weight PUBLISHED, or the DEFINED `πd²/4`/`A·ρ` fallbacks), a new grade one `RebarGradeRow` bound to its `EnRebarGrade` when EN-bodied, a new role one `RebarUsage` row carrying its verified token, a new schedule shape one `ShapeCodeRow`, a new hook table one `HookKind` row with its bend delegate, a new realized bar one `Placements` row — never a per-bar type, never a `ComponentFamily` edit, never a central edit. A welded mesh grows as an `IfcReinforcingMesh` projection over the same row currencies, never an eleventh family.
- Boundary: the seed admits raw standards data EXACTLY ONCE — the diameter lifts through `key.AcceptValidated<PositiveMagnitude>` into `SectionProfile.Circle.Of`, the bar/grade key lookups and the `RebarGradeRow.Admits` size-system guard rail typed `ComponentFault` cases (band `FaultBand.Component`), and a malformed row aborts the whole build rather than seeding a degenerate `Component`; per-column provenance is law — the EN H-series `CatalogueKey` is VENDOR (the `BarDiameter` catalogue member `RcSectionBuilder` feeds the `Rebar(IMaterial, BarDiameter)` ctor), printed diameter/area/weight are PUBLISHED and never re-derived (a rounded imperial unit weight stays printed), and the `NominalAreaMm2`/`NominalWeightKgM` fallbacks are DEFINED (EN 10080's own `πd²/4` and `A·ρ` at 7850 kg/m³) guarding only an unprinted column; imperial/CSA rows carry `CatalogueKey: None` so the exact non-EN nominal feeds a raw `Length`, never a catalogue approximation (`BarDiameter` is EN-10080 D6..D50 ONLY); the IFC role is the seed-computed `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` over the verified 11-member `IfcReinforcingBarTypeEnum` and the bond surface the `RebarSurface.IfcSurface` token — never a flat `"MAIN"` per row; the realization bag is `RebarDetail.Of(bar, usage)` composing the `component#COMPONENT_DETAIL` `ComponentDetail` constructors with the SAME four rows the retired projector switch built (`Joint("Cast")` + `BarType` token + dimension-only `NominalDiameter`/`CrossSectionArea` mints), so the `PropertyBag` content and the projected `Node.PropertySet` bytes are identical; the two `MaterialId` slots stay INDEPENDENT — `RebarGradeRow.SubstanceId` the per-grade `steel.<designation>` `Mechanical` row (the grade's `f_yk` + the ACI 318 §20.2.2.2 `E_s` 200 GPa, NEVER the generic `metal.steel` 235/210 baseline that under-reads a 500 MPa bar), `AppearanceId` the render finish (`metal.iron` plain carbon, `metal.steel` weldable), neither derived from the other; the bend geometry is the scalar `RebarBend` receipt `RebarSchedule.StandardHook` emits — the `HookKind` delegate splits ACI Table 25.3.1 development (6/8/10·d by band) from Table 25.3.2/25.3.4 stirrup-tie/seismic (4/6·d), the mandrel is the EN 1992 §8.3 former (4·d ≤16 mm, 7·d above — `Link.MinimumMandrelDiameter` parity), and the `ShapeCodeRow` is the BS 8666 designation the `IfcReinforcingBar` `BendingShapeCode` wire reads, a `RebarUsage.Stirrup` bar overriding to the closed link `51` — the host materializes the centreline curve from the scalars, NEVER a host `Curve` here; the rib deformation is the `RebarSchedule.Ribs` ISO 6935-2 receipt (fR by diameter band 0.035/0.040/0.056, the ASTM A615 §7.4 spacing/gap maxima, the §4.14 45° flank, the `RibPattern` β) — `Some` for a `Textured` bar, `None` for a plain round.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                                // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                                 // Op (the boundary-admission key), AcceptValidated, Context
using Rasm.Element;                                // MaterialId; PropertyBag/DetailSchema/Dimension (the seam detail-bag currencies RebarDetail composes)
using Thinktecture;
using Rasm.Materials.Component;                    // Component/ComponentRow/ComponentFamily/ComponentFault/ComponentStandard/ComponentAuthority/Coring/IfcBinding/SectionProfile/SectionSolver/ComponentDetail/DetailLane (the parent COMPONENT_OWNER)
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
public static class Bars {
    public static readonly ImmutableArray<BarRow> Rows = [
        new("no3",  RebarStandard.A615,    None,                     9.525, 71.0,   0.560),
        new("no4",  RebarStandard.A615,    None,                    12.700, 129.0,  0.994),
        new("no5",  RebarStandard.A615,    None,                    15.875, 199.0,  1.552),
        new("no6",  RebarStandard.A615,    None,                    19.050, 284.0,  2.235),
        new("no7",  RebarStandard.A615,    None,                    22.225, 387.0,  3.042),
        new("no8",  RebarStandard.A615,    None,                    25.400, 510.0,  3.973),
        new("no9",  RebarStandard.A615,    None,                    28.651, 645.0,  5.060),
        new("no10", RebarStandard.A615,    None,                    32.258, 819.0,  6.404),
        new("no11", RebarStandard.A615,    None,                    35.814, 1006.0, 7.907),
        new("no14", RebarStandard.A615,    None,                    43.002, 1452.0, 11.380),
        new("no18", RebarStandard.A615,    None,                    57.328, 2581.0, 20.240),
        new("10m",  RebarStandard.G30,     None,                    11.300, 100.0,  0.785),
        new("15m",  RebarStandard.G30,     None,                    16.000, 200.0,  1.570),
        new("20m",  RebarStandard.G30,     None,                    19.500, 300.0,  2.355),
        new("25m",  RebarStandard.G30,     None,                    25.200, 500.0,  3.925),
        new("30m",  RebarStandard.G30,     None,                    29.900, 700.0,  5.495),
        new("35m",  RebarStandard.G30,     None,                    35.700, 1000.0, 7.850),
        new("45m",  RebarStandard.G30,     None,                    43.700, 1500.0, 11.775),
        new("55m",  RebarStandard.G30,     None,                    56.400, 2500.0, 19.625),
        new("h6",   RebarStandard.En10080, Some(BarDiameter.D6),     6.000, 28.3,   0.222),
        new("h8",   RebarStandard.En10080, Some(BarDiameter.D8),     8.000, 50.3,   0.395),
        new("h10",  RebarStandard.En10080, Some(BarDiameter.D10),   10.000, 78.5,   0.617),
        new("h12",  RebarStandard.En10080, Some(BarDiameter.D12),   12.000, 113.0,  0.888),
        new("h14",  RebarStandard.En10080, Some(BarDiameter.D14),   14.000, 154.0,  1.210),
        new("h16",  RebarStandard.En10080, Some(BarDiameter.D16),   16.000, 201.0,  1.580),
        new("h20",  RebarStandard.En10080, Some(BarDiameter.D20),   20.000, 314.0,  2.470),
        new("h25",  RebarStandard.En10080, Some(BarDiameter.D25),   25.000, 491.0,  3.850),
        new("h28",  RebarStandard.En10080, Some(BarDiameter.D28),   28.000, 616.0,  4.840),
        new("h32",  RebarStandard.En10080, Some(BarDiameter.D32),   32.000, 804.0,  6.310),
        new("h40",  RebarStandard.En10080, Some(BarDiameter.D40),   40.000, 1257.0, 9.860),
        new("h50",  RebarStandard.En10080, Some(BarDiameter.D50),   50.000, 1964.0, 15.420)];
    public static readonly FrozenDictionary<string, BarRow> ByKey = Rows.ToFrozenDictionary(static r => r.Key, StringComparer.Ordinal);
}

// The 11-row grade table. Yields are the spec-nominal PUBLISHED bands; the EN B500 bands bind their EnRebarGrade so the
// RC path reads the registered f_yk/k rather than a hand key.
public static class Grades {
    public static readonly ImmutableArray<RebarGradeRow> Rows = [
        new("gr40",  280.0, RebarStandard.A615,    "steel.gr40",  "metal.iron",  None),
        new("gr60",  420.0, RebarStandard.A615,    "steel.gr60",  "metal.iron",  None),
        new("gr75",  520.0, RebarStandard.A615,    "steel.gr75",  "metal.iron",  None),
        new("gr80",  550.0, RebarStandard.A615,    "steel.gr80",  "metal.iron",  None),
        new("gr60w", 420.0, RebarStandard.A706,    "steel.gr60w", "metal.steel", None),
        new("gr80w", 550.0, RebarStandard.A706,    "steel.gr80w", "metal.steel", None),
        new("400w",  400.0, RebarStandard.G30,     "steel.400w",  "metal.steel", None),
        new("500w",  500.0, RebarStandard.G30,     "steel.500w",  "metal.steel", None),
        new("b500a", 500.0, RebarStandard.En10080, "steel.b500a", "metal.steel", Some(EnRebarGrade.B500A)),
        new("b500b", 500.0, RebarStandard.En10080, "steel.b500b", "metal.steel", Some(EnRebarGrade.B500B)),
        new("b500c", 500.0, RebarStandard.En10080, "steel.b500c", "metal.steel", Some(EnRebarGrade.B500C))];
    public static readonly FrozenDictionary<string, RebarGradeRow> ByKey = Rows.ToFrozenDictionary(static r => r.Key, StringComparer.Ordinal);
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
            var d => Some(new RebarRibGeometry(
                TransverseRibHeightMm:    0.05 * d,
                TransverseRibSpacingMm:   0.7 * d,
                LongitudinalRibHeightMm:  0.05 * d,
                FlankInclinationDeg:      45.0,
                RibInclinationDeg:        pattern.InclinationDeg,
                RelativeRibArea:          d <= 6.0 ? 0.035 : d <= 12.0 ? 0.040 : 0.056,
                RiblessPerimeterFraction: 0.125,
                Pattern:                  pattern)),
        };

    // ACI 318 §25.3 standard hook + EN 1992 §8.3 mandrel + BS 8666 shape code: the bend diameter splits by the HookKind
    // delegate (a #5 stirrup at 4·d_b, a #5 development bar at 6·d_b); the extension floors at hook.MinExtensionMm; the
    // mandrel is the EN former rule (4·d ≤ 16 mm, 7·d above — Link.MinimumMandrelDiameter parity); a stirrup usage
    // overrides the end-hook shape to the closed link 51. TOTAL — every input is a frozen row, so no rail exists to fail
    // (the prior Fin guarded an ExtensionFactor that is positive by row construction — an unrepresentable state).
    public static RebarBend StandardHook(BarRow bar, RebarUsage usage, HookKind kind, RebarHook hook) =>
        new(hook.BendDegrees,
            kind.MinInsideBendFactor(bar.NominalDiameterMm) * bar.NominalDiameterMm,
            Math.Max(hook.ExtensionFactor * bar.NominalDiameterMm, hook.MinExtensionMm),
            (bar.NominalDiameterMm <= 16.0 ? 4.0 : 7.0) * bar.NominalDiameterMm,
            usage.Stirrup ? ShapeCodes.ClosedLink : hook.Shape);
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
    public static readonly ForceBasis Nominal    = new("nominal",     forceKn: static (bar, grade) => Some(grade.MinimumYieldMpa * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnYield    = new("en-yield",    forceKn: static (bar, grade) => grade.EnGrade.Map(g => EnRebarFactory.CreateLinearElastic(g).Strength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    public static readonly ForceBasis EnUltimate = new("en-ultimate", forceKn: static (bar, grade) => grade.EnGrade.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * bar.NominalAreaMm2 * 1e-3));
    [UseDelegateFromConstructor] public partial Option<double> ForceKn(BarRow bar, RebarGradeRow grade);
}

// The seed-time realization bag (DetailLane.Realization) composing the component#COMPONENT_DETAIL ComponentDetail
// constructors — the SAME four rows the retired projector switch built (Joint("Cast") + the BarType usage token + the
// dimension-only mm->SI NominalDiameter/CrossSectionArea mints), so the PropertyBag content and the projected
// Node.PropertySet bytes are identical. Weight/rib/bend realization stays typed page algebra (RebarSchedule), never a
// bag row — a new bag row is a wire-visible content-key change.
public static class RebarDetail {
    public static PropertyBag Of(BarRow bar, RebarUsage usage) =>
        ComponentDetail.RealizationRows(
            ComponentDetail.Joint("Cast"),
            ComponentDetail.Token(DetailSchema.BarType, usage.IfcPredefinedType),
            ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, bar.NominalDiameterMm * 1e-3),
            ComponentDetail.Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, bar.NominalAreaMm2 * 1e-6));
}

// The realized-placement seed: designation verbatim (the wire-stable ComponentId), bar/grade row keys, and the typed
// usage/surface rows. The Bars × Grades × RebarUsage space is the generator's domain; this table is the realized
// SELECTION — a new placed bar is one row, the standard-consistency exclusion the Admits guard, never a swallowed fault.
public readonly record struct PlacementRow(string Designation, string Bar, string Grade, RebarUsage Usage, RebarSurface Surface);

public static class ReinforcementSeed {
    static readonly ImmutableArray<PlacementRow> Placements = [
        new("reinforcement.rebar-no3-gr60",      "no3",  "gr60",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no4-gr60",      "no4",  "gr60",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no4-gr60-tie",  "no4",  "gr60",  RebarUsage.Ligature, RebarSurface.Textured),
        new("reinforcement.rebar-no5-gr60",      "no5",  "gr60",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no6-gr60",      "no6",  "gr60",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no7-gr75",      "no7",  "gr75",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no8-gr75",      "no8",  "gr75",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no9-gr80",      "no9",  "gr80",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no11-gr80",     "no11", "gr80",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no18-gr80",     "no18", "gr80",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no5-gr60w",     "no5",  "gr60w", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-no8-gr80w",     "no8",  "gr80w", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-10m-400w",      "10m",  "400w",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-10m-400w-tie",  "10m",  "400w",  RebarUsage.Ligature, RebarSurface.Textured),
        new("reinforcement.rebar-15m-400w",      "15m",  "400w",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-25m-500w",      "25m",  "500w",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-35m-500w",      "35m",  "500w",  RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h8-b500a-link", "h8",   "b500a", RebarUsage.Ligature, RebarSurface.Plain),
        new("reinforcement.rebar-h12-b500b",     "h12",  "b500b", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h14-b500b",     "h14",  "b500b", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h16-b500c",     "h16",  "b500c", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h25-b500c",     "h25",  "b500c", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h32-b500c",     "h32",  "b500c", RebarUsage.Main,     RebarSurface.Textured),
        new("reinforcement.rebar-h40-b500c",     "h40",  "b500c", RebarUsage.Main,     RebarSurface.Textured)];

    // The ONE generator arm: table lookups and the Admits size-system guard rail typed ComponentFault cases; the diameter
    // lifts ONCE through the railed SectionProfile.Circle.Of; the IfcBinding and the RebarDetail bag are seed-computed —
    // every reinforcement row flows Component.Of -> ComponentRow(Sectioned: false), no second construction path.
    static Fin<ComponentRow> RebarOf(PlacementRow r) {
        Op key = Op.Of(name: r.Designation);
        return
            from bar in Bars.ByKey.TryGetValue(r.Bar, out BarRow b) ? Fin.Succ(b) : Fin.Fail<BarRow>(ComponentFault.Designation(key, $"<unknown-bar:{r.Bar}>"))
            from grade in Grades.ByKey.TryGetValue(r.Grade, out RebarGradeRow g) ? Fin.Succ(g) : Fin.Fail<RebarGradeRow>(ComponentFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
            from admitted in guard(grade.Admits(bar), ComponentFault.Grade(key, $"<grade-size-system-mismatch:{r.Grade}:{r.Bar}>"))
            from profile in SectionProfile.Circle.Of(bar.NominalDiameterMm, key)
            from item in Component.Of(
                ComponentFamily.Reinforcement, r.Designation, profile,
                IfcBinding.Of("IfcReinforcingBar", r.Usage.IfcPredefinedType),
                Coring.None, grade.Standard.Component, grade.Substance, grade.Appearance,
                detail: Some(RebarDetail.Of(bar, r.Usage)), key)
            select new ComponentRow(item, Sectioned: false);
    }

    // Fail-loud: one Traverse, a faulted row ABORTS the catalogue build — the .Choose/.ToOption swallow is the deleted
    // form. The Context parameter is the ComponentFamily.Rows delegate contract; the rebar seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Placements.ToSeq().Traverse(RebarOf).As();
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the reinforced-concrete receipt over the `VividOrange.Sections` `IConcreteSection` PLUS the held `ConcreteSectionProperties` transformed-section carrier; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the EN-grade admission boundary lowering the `VividOrange.Materials` derivation throws onto the typed `ComponentFault` rail; `RcSectionBuilder` the one assembler minting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`) · `FaceSpacing` (max-spacing bars on a face) · `PerimeterCount` (n bars round the whole section — `PerimeterReinforcementLayer`, no face) · `PerimeterSpacing`} — the face cases over the `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`; NO `Perimeter` member — perimeter distribution is the separate engine, never a face value); a bar arrangement is a `RebarLayout` case, never a scattered layer constructor; a stirrup is the `Link` promoted once from the same `RebarOf` bar the layouts use.
- Entry: `public static Fin<RcSection> RcSectionBuilder.Of(Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the grades through `EnGrade.Concrete`/`Rebar` (a non-EN `barGrade.EnGrade == None` railing `ComponentFault.Grade`), builds the `ConcreteSection` from the FAMILY-AGNOSTIC `SectionSolver.ProfileOf(concrete.Profile.GrossRectangleMm...)` `IProfile` + the concrete `IMaterial` + the promoted `Link` + the `coverMm` `Length`, folds each `RebarLayout` case to its placement engine through `AddRebarLayer`, and constructs the `ConcreteSectionProperties` carrier ONCE (eager-forced at the boundary) onto the receipt; `public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarRow bar, double maxAggregateMm, Op key)` reads the EC2 clear-spacing rule with `MaximumAggregateSize` SET so the `+(d_g + k2)` aggregate term is live — one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `MinimumReinforcementSpacing` with the settable `MaximumAggregateSize`/`AdditionalAggregateFactor`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`CrossSectionalShearReinforcementArea`/`ReinforcementSecondMomentOfAreaYy`/`Zz`/`EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)`; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial`, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex`; `.api/api-vividorange-standards.md`), VividOrange.Profiles (`IProfile` via `component#SECTION_SOLVER` `ProfileOf`), UnitsNet (`Length` cover/diameter/aggregate at the edge), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Fin`/`Seq`/`Try`).
- Growth: a new rebar arrangement is one `RebarLayout` case binding its placement engine (the generated `Switch` breaks every dispatch site at compile time); a new transformed-section read is one projection on the `RcSection` receipt over the held carrier; a new constitutive concrete law is a `capacity#SECTION_CAPACITY` concern over the same `IConcreteSection` — never a per-arrangement builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a re-summed bar area where `ConcreteSectionProperties` carries it.
- Boundary: `RcSectionBuilder.Of` is the BOUNDARY_ADMISSION point where the `VividOrange` throwing surface is admitted EXACTLY ONCE — grade-derivation throws lower onto `ComponentFault.Grade`, section/layout/property construction throws onto `ComponentFault.Section` (`ComponentFault.Capacity` RESERVED for the `capacity#SECTION_CAPACITY` SOLVE) — and the receipt egress carries only validated DATA: the transformed-section columns are `ConcreteSectionProperties` reads coerced to SI-mm scalars at the receipt surface (`GrossSteelAreaMm2` from `TotalReinforcementArea` — the hand `Σ π/4·d²·count` bar loop is the deleted form; `ReinforcementRatio` from `GeometricReinforcementRatio`; `ShearLinkAreaMm2` from `CrossSectionalShearReinforcementArea`; `ReinforcementInertiaYyMm4`/`ZzMm4`; the face queries `EffectiveDepthMm(SectionFace)`/`FaceSteelAreaMm2(SectionFace)` OPTIONED because the engine's face read throws/NaNs on a bar-less face — absence, never a sentinel) so no `UnitsNet` quantity crosses an interior signature, while the full elastic transformed-section stress state and the N-M-M hull stay the `capacity#SECTION_CAPACITY` owner's over the SAME `IConcreteSection`; the link bar promotes through `new Link(RebarOf(link, material))` off the same `CatalogueKey` Match every layout bar uses — the prior `LinkDiameter` `IfNone(BarDiameter.D8)` silent 8 mm default for a non-EN link is the deleted swallow (a #4 tie now feeds its true 12.7 mm `Length`); `RcSectionBuilder.Of` admits ANY `component#COMPONENT_OWNER` `Component` as its concrete outline because `SectionSolver.ProfileOf` reads the `Component.Profile.GrossRectangleMm` bounding pair regardless of family — a `cmu#CMU_FAMILY` grouted unit (its fully-grouted net solid IS its gross rectangle) admits as the reinforced-masonry concrete input through this ONE boundary, its grout `EnConcreteGrade` the section concrete, no cmu-specific builder; the RC section is NOT a `Component` — a `Component` is one discrete bar in the schedule, the `RcSection` the populated member it reinforces, meeting at the `BarRow`/`RebarGradeRow` currencies this page owns; `RcSection.ConcreteProfile` carries the source `Component` (the QTO key + the gross lever the `capacity#SECTION_CAPACITY` `RcElastic` fibre arm reads).

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
    // so the guard is load-bearing), lower the grades, mint the family-agnostic gross-rectangle IProfile through
    // SectionSolver.ProfileOf, build the ConcreteSection + layers, then construct the ConcreteSectionProperties carrier
    // and EAGER-FORCE its first read so any degenerate-section throw surfaces HERE, never on an interior receipt read.
    public static Fin<RcSection> Of(Component concrete, EnConcreteGrade concreteGrade, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from cover in guard(double.IsFinite(coverMm) && coverMm >= 0.0, ComponentFault.Dimension(key, $"<cover-negative-or-nonfinite:{coverMm:R}>"))
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade.EnGrade, annex, key)
        from profile in SectionSolver.ProfileOf(concrete.Profile.GrossRectangleMm.WidthMm.Value, concrete.Profile.GrossRectangleMm.DepthMm.Value, key)
        from built in Try.lift(() => Build(profile, concreteMaterial, rebarMaterial, link, layout, coverMm)).Run()
            .MapFail(e => ComponentFault.Section(key, $"<rc-section-build:{concrete.Family.Key}:{e.Message}>"))
        from properties in Try.lift(() => { ConcreteSectionProperties p = new(built); _ = p.TotalReinforcementArea; return p; }).Run()   // boundary kernel: the eager force is the platform-forced seam
            .MapFail(e => ComponentFault.Section(key, $"<rc-transformed-properties:{concrete.Designation.Value}:{e.Message}>"))
        select new RcSection(built, properties, concreteMaterial, rebarMaterial, coverMm, concrete);

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
    // would egress as a Succ(NaN) spacing. Never an inline EC2 constant.
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
- [FAIL_LOUD_SEED]: REALIZED — `ReinforcementSeed.Rows : Context -> Fin<Seq<ComponentRow>>` is ONE `Traverse` over the 24-row `Placements` selection: bar/grade key misses rail `ComponentFault.Designation`/`Grade`, the `RebarGradeRow.Admits` size-system law (a grade admits only bar rows its `Standard.Rolls` body defines — A706 rolls the A615 imperial sizes) guards BEFORE construction, the diameter lifts once through the railed `SectionProfile.Circle.Of`, and every row constructs through the ONE generator `Component.Of` into `ComponentRow(item, Sectioned: false)` — the prior `BuildRebarRows` `.Choose(...).ToOption())` fault swallow that silently dropped a malformed row from the frozen dictionary is the deleted form; a faulted row now ABORTS `ComponentCatalogue.Of`. The seed designations are the verbatim wire-stable keys (`reinforcement.rebar-no5-gr60`, `reinforcement.rebar-10m-400w-tie`, `reinforcement.rebar-h8-b500a-link`), and the usage/surface columns are typed row references, deleting two string lookups per row.
- [DETAIL_BAG_BYTES]: REALIZED — the reinforcement realization detail is built AT SEED TIME by `RebarDetail.Of(bar, usage)` composing the `component#COMPONENT_DETAIL` `ComponentDetail` constructors with exactly the four rows the retired `Projection/component.md` `Detail(Component)` switch built — `Joint("Cast")`, the `BarType` token from `usage.IfcPredefinedType`, and the dimension-only `MeasureValue.OfSi` `NominalDiameter` (mm→m) and `CrossSectionArea` (mm²→m²) mints — so the `PropertyBag` content keys and the projected `Node.PropertySet` bytes are identical to the pre-campaign wire. Weight, rib geometry, and bend receipts stay TYPED page algebra (`RebarSchedule`) rather than bag rows because any added row is a wire-visible content-key change; the `ComponentFamily.Reinforcement` row is `DetailLane.Realization`, so `Component.Of`'s lane/detail totality law proves every seeded bar carries its bag.
- [SCHEDULE_ALGEBRA]: REALIZED — the deleted `RebarSection` payload's projections re-homed as `RebarSchedule` operations over the row currencies: `Ribs(bar, surface, pattern)` the ISO 6935-2 receipt (fR 0.035/0.040/0.056 by the ≤6/≤12/>12 mm band, the ASTM A615 §7.4 `0.7·d` spacing and `0.125` gap maxima, the Table 1 `0.05·d` rib height, the §4.14 45° flank, the `RibPattern` §4.15 β — `Some` for `Textured`, `None` for plain); `StandardHook(bar, usage, kind, hook)` the TOTAL `RebarBend` emitter (every input a frozen row — the prior `Fin` guarded an unrepresentable `ExtensionFactor`) — the `HookKind` constructor-delegate splits ACI Table 25.3.1 development (6/8/10·d_b at the 25.4/36 mm band boundaries) from 25.3.2/25.3.4 stirrup-tie/seismic (4/6·d_b at 16 mm), the extension floors at 65/75 mm, the EN 1992 §8.3 mandrel is `(d ≤ 16 ? 4 : 7)·d` (`Link.MinimumMandrelDiameter` parity), and a `Stirrup` usage overrides the end-hook `ShapeCodeRow` to the closed link `51`; the schedule force is the `ForceBasis` policy-row projection `basis.ForceKn(bar, grade)` — `Nominal` the spec-printed band × nominal area (always `Some`), `EnYield`/`EnUltimate` the `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` registered characteristic yield and ductility-class `k·f_yk` ultimate (k = 1.05/1.08/1.15 for A/B/C), `Some` only for the EN-bodied bands — the three sibling `*ForceKn` methods are the deleted form. The bend/rib receipts are host-neutral scalars the host curve-materializes; this owner never constructs a host `Curve` or an IFC entity.
- [RC_TRANSFORMED_RECEIPT]: REALIZED — `RcSection` holds the `ConcreteSectionProperties` carrier constructed and eager-forced ONCE at `RcSectionBuilder.Of`, and its columns are carrier reads: `GrossSteelAreaMm2` from `TotalReinforcementArea` (the `.Utility` `Rebars.CalculateArea` kernel behind the getter — the hand `Σ π/4·d²·CountPerBundle` loop is the deleted re-derivation), `ConcreteAreaMm2`, `ReinforcementRatio` from `GeometricReinforcementRatio`, `ShearLinkAreaMm2` from `CrossSectionalShearReinforcementArea` (both link legs), `ReinforcementInertiaYyMm4`/`ZzMm4` from `ReinforcementSecondMomentOfAreaYy`/`Zz`, and the face queries `EffectiveDepthMm(SectionFace)`/`FaceSteelAreaMm2(SectionFace)` — the scalar columns coerced to SI-mm at the receipt surface, the face queries `Option<double>` because the engine's `CalculateEffectiveDepth` throws/NaNs for a face with no bars (a perimeter-only layout): a bar-less face is absence, never a NaN or throw escaping an interior read, and no `UnitsNet` quantity crosses an interior signature. The full elastic combined-stress state (`RcElastic`) and the `InteractionDiagram` N-M-M hull remain `capacity#SECTION_CAPACITY` owners over the SAME `IConcreteSection`; this receipt carries the reinforcement-side geometry facts the QTO seam and the capacity lift read without re-solving.
- [RC_BOUNDARY_REBIND]: REALIZED — `RcSectionBuilder.Of` rebinds the retired `ParametricSection.ProfileOf` to `component#SECTION_SOLVER` `SectionSolver.ProfileOf` and the gross outline to `concrete.Profile.GrossRectangleMm` (the `SectionProfile` base-constructor state every arm declares), staying FAMILY-AGNOSTIC — a `cmu#CMU_FAMILY` grouted unit admits as the reinforced-masonry concrete input through this one boundary (its grout `EnConcreteGrade` the section concrete), no cmu-specific builder. The link mints through the SAME `RebarOf` catalogue-or-raw-`Length` Match as every layout bar and promotes via `new Link(IRebar)` — the prior `LinkDiameter` `Catalogue.IfNone(BarDiameter.D8)` silently rebuilt a non-EN link at 8 mm (a #4 tie under-sized by 37%), the deleted swallow. `MinimumBarSpacingMm` now SETS the rule's `MaximumAggregateSize` so the EC2 `(d_g + k2)` aggregate branch is live — the prior signature carried `maxAggregateMm` as a dead parameter. The two raw boundary scalars are ADMITTED before any `UnitsNet` lift (`coverMm` non-negative finite, `maxAggregateMm` positive finite — `Length.FromMillimeters` accepts a NaN silently, so an unguarded value would egress as a `Succ(NaN)`). Fault split holds: grade derivation → `ComponentFault.Grade`, section/layout/property construction → `ComponentFault.Section`, `ComponentFault.Capacity` reserved for the capacity SOLVE; band integers ride the `FaultBand.Component` registry row.
- [IFC_REINFORCING_WIRE]: REALIZED — each seeded bar stamps `IfcBinding.Of("IfcReinforcingBar", usage.IfcPredefinedType)` at seed time over the FULL verified 11-member `IfcReinforcingBarTypeEnum` (`MAIN`/`LIGATURE`/`SHEAR`/`PUNCHING`/`EDGE`/`RING`/`ANCHORING`/`SPACEBAR`/`STUD`/`USERDEFINED`/`NOTDEFINED`), never a flat per-row string; the bond surface rides `RebarSurface.IfcSurface` (`IfcReinforcingBarSurfaceEnum` {`PLAIN`, `TEXTURED`}), the schedule designation `RebarBend.Shape.Key` (the `IfcReinforcingBar` `BendingShapeCode`), and the grade token `RebarGradeRow.Key` (`SteelGrade`) on the `Rasm.Bim` egress mapping; a mesh round-trips as `IfcReinforcingMesh` over the same row columns. Validation is Bim's: the strings here stay neutral, composition-time `IfcLegality` and egress-time `AdmitPredefined` gate them against the generated roster.
