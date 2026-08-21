# [MATERIALS_JOINT]

THE JOINT SEED PAGE — the `joint` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Realization`), the continuous-connection weld/adhesive/stud vocabulary. A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed part: it carries no thread or bar cross-section, so it cannot fold into `fastener` the way `anchor` does — the ONE deliberate widening past the discrete triple, load-bearing because `steel#STEEL_FAMILY`'s `Composite` arm reads the stud shear resistance from here for its `ΣQn` cap. An 8 mm fillet weld is a `Component` row in the `joint` family, never a `Weld` type: geometry lands as the `SectionProfile` arm the family admits (`FilletTriangle` the fillet and flare welds — the `0.707·leg` throat staying the family's DEFINED derivation; `Trapezium` the PJP/CJP groove derived from the prep geometry; `Circle` the plug hole and the stud shank; `Rectangle` the slot's obround footprint; `Nominal` the continuous adhesive bond-line), the realization scalars land in the `JointDetail` `DetailSchema.Realization` `PropertyBag`, and the strength axes are frozen row tables with per-column provenance. `JointSeed` binds the DUAL IFC entity at seed per the IFC-BINDING law — `JointKind.Binding` the row-owned `IfcBinding.Of(kind.IfcEntity, kind.IfcPredefinedType)` projection, the entity a vocabulary COLUMN beside its token — an `IfcMechanicalFastener` `STUDSHEARCONNECTOR` for the welded stud, an `IfcFastener` `WELD`/`GLUE` for the weld/adhesive bead. The vocabulary grows by data: a new electrode/adhesive/stud diameter/stud grade is one row in its frozen table, a new groove one `GrooveGeometry` row, a new designation one `JointRow` table entry — never a per-joint type. The generative geometry is the hand-rolled AWS D1.1:2020 + AISC 360 J2/I8 + ISO 13918:2017 + EN 1994 capture (GeometryGym mirrors only the IFC class; VividOrange owns no weld); the host materializes the bead/groove/stud solid from the scalar receipt, NEVER a host `Curve` here. `DesignShearKn` emits the SPEC-NOMINAL filler/adhesive/stud-steel band; the measured base-metal capacity is the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt read by `MaterialId`, and the composite stud's concrete branch is the `Rasm.Compute/Analysis/capacity#DESIGN_CHECK` join — neither a column here. The EN 1993-1-8 / EN 1994 design codes a verdict cites ride the `capacity#SECTION_CAPACITY` `SectionCapacity.Code` column, never a static beside these rows. The page composes `Component/component#COMPONENT_OWNER` (`Component.Of`, `ComponentRow`, `SectionProfile`, `IfcBinding`, `ComponentDetail`, `ComponentFault`), the `Rasm.Element/Properties/property#DETAIL_SCHEMA` realization rows, `Rasm.Numerics` `PositiveMagnitude`, and the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` two-slot law.

## [01]-[INDEX]

- [02]-[JOINT_FAMILY]: the `JointKind` IFC binding vocabulary; the welding policy axes and frozen strength tables; the `StudGroup` AISC I8-1 placement table; the `WeldGeometry` union over fillet, groove, plug, slot, flare-bevel, and flare-V payloads; the `GroovePrep`, `HoleWeld`, and `WeldProfile` geometry values; the `JointRow` weld/stud/adhesive family; `JointDetail`; and the `JointSeed.Roster`/`Law` set.

## [02]-[JOINT_FAMILY]

- Owner: `JointSeed` owns the roster, the seed law, and the capacity producer; `JointKind` owns the complete IFC binding; the welding SmartEnums own reusable policy; the frozen strength tables own published data; `StudGroup` owns the AISC I8-1 group-and-position table; `WeldGeometry` owns the payload-timed geometry variants without inert defaults; `JointRow` owns the weld/stud/adhesive seed family; `JointDetail` owns the realization bag; and `JointSeed` owns the closed table and fold.
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `GrooveGeometry` × `ElectrodeClass` × `WeldProcess`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass` × `StudGrade`)}; weld {fillet · groove (square/V/bevel/U/J × single/double on `GrooveGeometry`) · plug · slot · flare-bevel · flare-v} over the E60..E110 electrode band; adhesive {epoxy · methacrylate · polyurethane · silicone-structural} over the lap-shear/peel/SSG-bite band; stud {the 13..25 mm ISO 13918 Type SD headed connectors} over diameter × grade × height × spacing. A joint is a `Component` row in `ComponentFamily.Joint`, never a joint subtype; the groove subtype is a `GrooveGeometry` row, never a per-subtype `WeldType`.
- Entry: `ComponentSeed.Rows(context, JointSeed.Roster, JointSeed.Law)` — this page states the roster and the policy, never the fold, and every law selector is ONE total `Switch` over the closed `JointRow` family. Weld geometry is already admitted through `PositiveMagnitude`; the coherence census proves the AISC J2.4 minimum leg, the AWS §4.4.5.4 depth of filling, the slot aspect, and the PJP prep surviving its process deduction, and a stud row proves its as-welded length positive. `JointRow.Weld.DirectionalShearKn(Angle)` applies the directional factor without a raw-angle convention.
- Packages: Rasm.Numerics (`PositiveMagnitude` — throat/leg/size/length/bond-line/overlap/width/spacing, never an int-backed count that truncates a fractional throat), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `Dimension`, `PropertyBag`, `PropertyName`, `PropertyValue`), Rasm.Materials.Component (`Component`/`ComponentRow`/`SectionProfile`/`IfcBinding`/`ComponentDetail`/`ComponentFault`/`Coring`/`ComponentStandard`/`ComponentAuthority`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + generated total `Switch` for the policy axes), UnitsNet (`Angle` at the directional-strength read), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`Option`), BCL inbox (`ImmutableArray`, `FrozenDictionary`). AWS/AISC/ISO/ASTM have no VividOrange body, so every design-code citation on these rows stays `PUBLISHED` provenance and the typed EN identity rides `capacity#SECTION_CAPACITY` `SectionCapacity.Code`.
- Growth: a new weld geometry is one `WeldType` row; a new groove one `GrooveGeometry` row carrying its angle/radius; a new electrode one `ElectrodeClass` row naming its own classification number; a new adhesive one `AdhesiveClass` row; a new stud diameter one `StudClass` row; a new stud grade one `StudGrade` row (`fy`/`fu`); a new deck-and-position combination one `StudGroup` row; a new designation one `JointRow` table entry; a new continuous-connection modality ONE `JointRow` case whose missing `Switch` arms break `JointDetail.Of` and `JointSeed.Row` at compile time. The structural-joint utilisation verdict is the `capacity#SECTION_CAPACITY` `Connection` case lifting these receipts, never this page's.
- Boundary: strength axes remain frozen rows because identity and lookup behavior are absent; policy axes remain SmartEnums because they carry dispatch data. `WeldGeometry` distinguishes payload arity and timing: only groove geometry carries preparation and process, only hole welds carry diameter/depth, and only line welds carry a run. A THROAT is a line-weld concept — a plug and a slot resist on the faying-plane hole area with no throat at all — so `EffectiveThroatMm` is `Option<double>` and the two hole arms answer absence rather than the zero a reader divides by.
- Boundary: the plug and slot effective area is the NOMINAL AREA OF THE HOLE OR SLOT IN THE PLANE OF THE FAYING SURFACE, stated identically by AWS D1.1 §4.4.5.3 and AISC 360 §J2.3a, so the weld DEPTH enters no strength term at all. Depth is instead an ADMISSION datum: the code requires a plug or slot in material 16 mm or thinner to be filled to the full thickness and a thicker one to half its thickness or 16 mm, whichever is greater, capped at the thinner joined part — an underfilled plug is a NONCONFORMING weld rather than a weaker one, so the depth column gates the row at seed and never reduces a resistance afterwards.
- Boundary: `StudClass.SteelShearKn` takes its `StudGroup` and has no default. AISC Eq I8-1 caps the stud at `Rg·Rp·Asa·Fu`, where `Rg` falls to 0.85 at two studs per rib and 0.70 at three, and `Rp` falls from 0.75 to 0.60 the moment the stud sits in the weak position — a stud group frozen at `1.0`/`0.75` reports a strong-position, directly-welded connector's capacity for a three-per-rib weak-position one and over-states it by more than half. The deck relation, the studs-per-rib count, and the rib position are PLACEMENT facts, so they arrive as the placement's own `StudGroup` and the vocabulary carries the published pair for each.

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
using Thinktecture;
using UnitsNet;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;
using static Rasm.Materials.Component.ComponentDetail;

// Every family page declares in the ONE flat Rasm.Materials.Component namespace (component#COMPONENT_OWNER;
// dotnet_style_namespace_match_folder). This page DEFINES StudClass and StudGroup; the steel#STEEL_FAMILY Composite
// arm reads StudClass.S19.SteelShearKn(group) by bare name.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The continuous-connection kind discriminant owning the COMPLETE dual-entity binding (POLICY_VALUES — the entity
// axis is a row column beside its token, never an external equality branch): the welded stud is IfcMechanicalFastener
// STUDSHEARCONNECTOR, the weld bead IfcFastener WELD, the adhesive bead IfcFastener GLUE — IfcFastenerTypeEnum admits
// {NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}, so a new bead kind draws its token from that set. Binding derives the
// seed-time IfcBinding whole off the row, so a new kind that omits its entity is a missing constructor argument.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointKind {
    public static readonly JointKind Weld     = new("weld",     ifcEntity: "IfcFastener",           ifcPredefinedType: "WELD");
    public static readonly JointKind Adhesive = new("adhesive", ifcEntity: "IfcFastener",           ifcPredefinedType: "GLUE");
    public static readonly JointKind Stud     = new("stud",     ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "STUDSHEARCONNECTOR");
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public IfcBinding Binding => IfcBinding.Of(IfcEntity, IfcPredefinedType);
}

// The bead face contour (AWS D1.1 weld-profile acceptance): flat, convex (fillet reinforcement), concave. The face
// describes the deposited reinforcement; the effective throat is measured to the THEORETICAL face, never the convex
// reinforcement, so the face is descriptive not structural — and its consumer is the weld-procedure block a weld map
// carries, where the as-deposited profile is exactly what an inspector reads against.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldFace {
    public static readonly WeldFace Flat    = new("flat");
    public static readonly WeldFace Convex  = new("convex");
    public static readonly WeldFace Concave = new("concave");
}

// The 6 AWS D1.1 weld GEOMETRIES. The groove SUBTYPE geometry (square/V/bevel/U/J × single/double) lives on
// GrooveGeometry, NOT a per-subtype WeldType. Directional flags the AISC Eq J2-5 k_ds eligibility and ONLY the fillet
// row carries it: J2.4(a) grants the directional-strength increase to fillet welds alone, so a FLARE weld — a groove
// weld by classification, priced on the Table 5.2 radius throat — reads false. There is no line-versus-hole column
// beside it: which resistance a weld takes is the WeldGeometry ARM's own shape, and the throat Option already states
// it, so a boolean here would be that fact spelled a third time. FlareThroatFactor is the Table 5.2 flare-groove
// radius factor; Face/ReinforcementMm/ToeRadiusMm the as-deposited bead-profile columns the weld map publishes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldType {
    public static readonly WeldType Fillet     = new("fillet",      directional: true,  flareThroatFactor: 0.0,    face: WeldFace.Convex, reinforcementMm: 1.0, toeRadiusMm: 1.5);
    public static readonly WeldType Groove     = new("groove",      directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0, toeRadiusMm: 0.0);
    public static readonly WeldType Plug       = new("plug",        directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0, toeRadiusMm: 0.0);
    public static readonly WeldType Slot       = new("slot",        directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0, toeRadiusMm: 0.0);
    public static readonly WeldType FlareBevel = new("flare-bevel", directional: false, flareThroatFactor: 0.3125, face: WeldFace.Flat,   reinforcementMm: 0.0, toeRadiusMm: 0.0);
    public static readonly WeldType FlareV     = new("flare-v",     directional: false, flareThroatFactor: 0.5,    face: WeldFace.Flat,   reinforcementMm: 0.0, toeRadiusMm: 0.0);
    public bool Directional { get; }
    public double FlareThroatFactor { get; }
    public WeldFace Face { get; }
    public double ReinforcementMm { get; }
    public double ToeRadiusMm { get; }
}

// AISC 360 Table J2.1 PJP effective-throat deduction: SMAW/GMAW/FCAW deduct 3 mm (1/8 in) at a sharp (<60°) bevel
// groove where reliable root fusion is process-limited; SAW's deeper penetration takes the FULL groove depth.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldProcess {
    public static readonly WeldProcess Smaw = new("smaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Gmaw = new("gmaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Fcaw = new("fcaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Saw  = new("saw",  pjpDeductionMm: 0.0);
    public double PjpDeductionMm { get; }
}

// The 9 AWS A2.4 groove geometries. IncludedAngleDeg the V/U total angle, BevelAngleDeg the single-wall bevel/J angle,
// RootRadiusMm the U/J root radius. A both-face prep is the ROW — the double-* keys name it and the weld-procedure
// block publishes that key — so no sidedness column stands beside them. A sharp bevel groove (45°, no radius) takes
// the WeldProcess PJP deduction; a 60° V, a radiused U/J, or any CJP develops the full depth.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GrooveGeometry {
    public static readonly GrooveGeometry Square      = new("square",       includedAngleDeg: 0.0,  bevelAngleDeg: 0.0,  rootRadiusMm: 0.0);
    public static readonly GrooveGeometry SingleV     = new("single-v",     includedAngleDeg: 60.0, bevelAngleDeg: 0.0,  rootRadiusMm: 0.0);
    public static readonly GrooveGeometry DoubleV     = new("double-v",     includedAngleDeg: 60.0, bevelAngleDeg: 0.0,  rootRadiusMm: 0.0);
    public static readonly GrooveGeometry SingleBevel = new("single-bevel", includedAngleDeg: 0.0,  bevelAngleDeg: 45.0, rootRadiusMm: 0.0);
    public static readonly GrooveGeometry DoubleBevel = new("double-bevel", includedAngleDeg: 0.0,  bevelAngleDeg: 45.0, rootRadiusMm: 0.0);
    public static readonly GrooveGeometry SingleU     = new("single-u",     includedAngleDeg: 20.0, bevelAngleDeg: 0.0,  rootRadiusMm: 6.0);
    public static readonly GrooveGeometry DoubleU     = new("double-u",     includedAngleDeg: 20.0, bevelAngleDeg: 0.0,  rootRadiusMm: 6.0);
    public static readonly GrooveGeometry SingleJ     = new("single-j",     includedAngleDeg: 0.0,  bevelAngleDeg: 20.0, rootRadiusMm: 10.0);
    public static readonly GrooveGeometry DoubleJ     = new("double-j",     includedAngleDeg: 0.0,  bevelAngleDeg: 20.0, rootRadiusMm: 10.0);
    public double IncludedAngleDeg { get; }
    public double BevelAngleDeg { get; }
    public double RootRadiusMm { get; }
    public bool RequiresPjpDeduction => BevelAngleDeg is > 0.0 and <= 45.0 && RootRadiusMm <= 0.0;
}

// CJP develops the full connected-part thickness (the weld matches the base metal); PJP develops only the depth of
// preparation, reduced by the WeldProcess deduction at a sharp groove. Completeness IS the row — `== Penetration.Cjp`
// is the probe — so a column true for exactly one of two rows states nothing its key does not.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Penetration {
    public static readonly Penetration Cjp = new("cjp");
    public static readonly Penetration Pjp = new("pjp");
}

// The root CONDITION — split out of the groove BackingType (a back-gouged, back-welded root is a root TREATMENT, the
// backing bar a groove MATERIAL): AsWelded the open root, Backgouge gouged to sound metal and back-welded, SealPass a
// seal weld over the root. A groove row NAMES its root treatment, so all three are seeded and all three reach the
// weld-procedure block a weld map carries — a welder reads the root condition off that block before striking an arc.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RootTreatment {
    public static readonly RootTreatment AsWelded  = new("as-welded");
    public static readonly RootTreatment Backgouge = new("backgouge");
    public static readonly RootTreatment SealPass  = new("seal-pass");
}

// The groove backing MATERIAL — None for an open or back-gouged root, Steel for a fused backing bar, Ceramic/Copper/
// Flux the removable and consumable backings. Distinct from RootTreatment: backing is a material, root treatment a
// condition, and a weld map carries both because a shop procures the one and performs the other.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BackingType {
    public static readonly BackingType None    = new("none");
    public static readonly BackingType Steel   = new("steel");
    public static readonly BackingType Ceramic = new("ceramic");
    public static readonly BackingType Copper  = new("copper");
    public static readonly BackingType Flux    = new("flux");
}

// AISC 360 §I8.2a — the stud group and position table, the FLATTENED PRODUCT of the three axes Eq I8-1 keys on: the
// deck relation, the number of studs in a rib, and the stud's own position within it. Rg is 1.0 for a stud welded
// directly to the shape, for one stud in a perpendicular rib, and for a parallel rib whose width-to-depth ratio
// reaches 1.5; it falls to 0.85 at two studs per rib or a narrow parallel rib, and to 0.70 at three or more. Rp is
// 0.75 where the stud is welded direct, where the deck runs parallel, or where the rib measurement e_mid-ht reaches
// 50 mm — the STRONG position — and 0.60 below it, the WEAK position the specification takes as its own default. The
// Direct row is the declared default a designer selects deliberately; it is NOT a fallback any arm reaches for.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StudGroup {
    public static readonly StudGroup Direct           = new("direct",             groupFactor: 1.00, positionFactor: 0.75);
    public static readonly StudGroup ParallelWide     = new("parallel-wide",      groupFactor: 1.00, positionFactor: 0.75);
    public static readonly StudGroup ParallelNarrow   = new("parallel-narrow",    groupFactor: 0.85, positionFactor: 0.75);
    public static readonly StudGroup RibOneStrong     = new("rib-1-strong",       groupFactor: 1.00, positionFactor: 0.75);
    public static readonly StudGroup RibOneWeak       = new("rib-1-weak",         groupFactor: 1.00, positionFactor: 0.60);
    public static readonly StudGroup RibTwoStrong     = new("rib-2-strong",       groupFactor: 0.85, positionFactor: 0.75);
    public static readonly StudGroup RibTwoWeak       = new("rib-2-weak",         groupFactor: 0.85, positionFactor: 0.60);
    public static readonly StudGroup RibThreeStrong   = new("rib-3-strong",       groupFactor: 0.70, positionFactor: 0.75);
    public static readonly StudGroup RibThreeWeak     = new("rib-3-weak",         groupFactor: 0.70, positionFactor: 0.60);
    public double GroupFactor { get; }      // Rg
    public double PositionFactor { get; }   // Rp
}

// AWS A5.1 carbon-steel (E60/E70) and AWS A5.5 low-alloy (E80..E110) covered-electrode classifications — a FROZEN row
// table. The classification NUMBER IS the minimum filler tensile strength in ksi, which is what the designation
// means, so the row states that number and the strength derives; a hand-converted megapascal column beside it would
// be a second spelling of the same fact and could drift from it. The specification body derives the same way: A5.1
// owns the carbon-steel classifications through 70 and A5.5 the low-alloy ones above. The appearance is the weld
// finish the two-slot law reads.
public readonly record struct ElectrodeClass(string Key, double ClassificationKsi, string SubstanceId, string AppearanceId) {
    const double KsiToMpa = 6.894757;
    public static readonly ElectrodeClass E60  = new("e60",   60.0, "steel.e60",  "metal.iron");
    public static readonly ElectrodeClass E70  = new("e70",   70.0, "steel.e70",  "metal.steel");
    public static readonly ElectrodeClass E80  = new("e80",   80.0, "steel.e80",  "metal.steel");
    public static readonly ElectrodeClass E90  = new("e90",   90.0, "steel.e90",  "metal.steel");
    public static readonly ElectrodeClass E100 = new("e100", 100.0, "steel.e100", "metal.steel");
    public static readonly ElectrodeClass E110 = new("e110", 110.0, "steel.e110", "metal.steel");
    public static readonly ImmutableArray<ElectrodeClass> Rows = [E60, E70, E80, E90, E100, E110];
    public double TensileMpa => ClassificationKsi * KsiToMpa;
    public string Specification => ClassificationKsi <= 70.0 ? "AWS A5.1" : "AWS A5.5";
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

// Structural-adhesive allowables — a FROZEN row table. All columns PUBLISHED: LapShearMpa (ASTM D1002 single-lap),
// PeelNmm (ASTM D1876 T-peel), ServiceCelsius, StructuralBiteMpa (the ASTM C1401 SSG design tensile the silicone
// curtain-wall bite develops, distinct from its cured lap-shear).
public readonly record struct AdhesiveClass(string Key, double LapShearMpa, double PeelNmm, double ServiceCelsius, Option<double> StructuralBiteMpa, string SubstanceId) {
    public static readonly AdhesiveClass Epoxy              = new("epoxy",               30.0, 5.0,  80.0,  None,       "adhesive.epoxy");
    public static readonly AdhesiveClass Methacrylate       = new("methacrylate",        25.0, 12.0, 100.0, None,       "adhesive.methacrylate");
    public static readonly AdhesiveClass Polyurethane       = new("polyurethane",        15.0, 20.0, 90.0,  None,       "adhesive.polyurethane");
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", 1.0,  8.0,  150.0, Some(0.14), "sealant.silicone-structural");
    public static readonly ImmutableArray<AdhesiveClass> Rows = [Epoxy, Methacrylate, Polyurethane, SiliconeStructural];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
}

// ISO 13918:2017 Type SD headed shear connectors (AWS D1.1 Type B equivalent) — a FROZEN row table keyed by the
// nominal shank. PUBLISHED columns: DiameterMm (ISO d), HeadDiameterMm, HeadThicknessMm, WeldCollarDiameterMm (the
// as-welded base fillet — the collar IS the weld footprint, no separate weld-area column), WeldCollarHeightMm,
// BurnoffMm (the ISO 13918 Table 10 l1 − l2 arc consumption, published per diameter), UltimateMpa (the AWS D1.1
// Type B / AISC §I8 Fu cap). DEFINED columns: AreaMm2 = πd²/4; SteelShearKn the Eq I8-1 cap over the group's own
// published Rg·Rp pair. This is the ONE shear-stud cap steel#STEEL_FAMILY's Composite arm reads for ΣQn.
public readonly record struct StudClass(string Key, double DiameterMm, double HeadDiameterMm, double HeadThicknessMm, double WeldCollarDiameterMm, double WeldCollarHeightMm, double BurnoffMm, double UltimateMpa) {
    public static readonly StudClass S13 = new("stud-1/2", 12.7, 25.0, 8.0,  17.0, 3.0, 3.0,  450.0);
    public static readonly StudClass S16 = new("stud-5/8", 15.9, 32.0, 8.0,  21.0, 4.5, 4.0,  450.0);
    public static readonly StudClass S19 = new("stud-3/4", 19.1, 32.0, 10.0, 23.0, 6.0, 4.5,  450.0);
    public static readonly StudClass S22 = new("stud-7/8", 22.2, 35.0, 10.0, 29.0, 6.0, 5.0,  450.0);
    public static readonly StudClass S25 = new("stud-1",   25.4, 40.0, 12.0, 31.0, 7.0, 5.5,  450.0);
    public const double TipAngleDeg = 140.0;   // ISO 13918 140° ± 7° point
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
    public double SteelShearKn(StudGroup group) => group.GroupFactor * group.PositionFactor * AreaMm2 * UltimateMpa * 1e-3;
}

public static class StudClasses {
    public static readonly ImmutableArray<StudClass> Rows = [StudClass.S13, StudClass.S16, StudClass.S19, StudClass.S22, StudClass.S25];
}

// ISO 13918:2017 SD material grades + AWS D1.1 stud types — a FROZEN row table. PUBLISHED columns: the specified
// fy/fu the EN 1994 §6.6.3.1 PRd path and the mill certificate read (SD1 the standard carbon shear-connector stud
// 350/450, numerically identical to AWS Type B by design — one global grade under two designations; SD2 the
// higher-elongation carbon 235/400; SD3 the austenitic stainless 350/500; AWS Type A general 340/420). EN 1994 caps
// the DESIGN fu at 500 MPa, which the resistance applies rather than the table pre-clamping it. Appearance is
// GRADE-borne per the two-slot law: the SD3 stainless renders the library chromium conductor row, the carbon grades
// plain steel.
public readonly record struct StudGrade(string Key, double YieldMpa, double UltimateMpa, string SubstanceId, string AppearanceId) {
    const double EnUltimateCapMpa = 500.0;   // EN 1994-1-1 §6.6.3.1: fu is taken not greater than 500 N/mm²
    const double EnGammaV = 1.25;            // the recommended partial factor for shear connection
    public static readonly StudGrade Sd1  = new("sd1",   350.0, 450.0, "steel.sd1",   "metal.steel");
    public static readonly StudGrade Sd2  = new("sd2",   235.0, 400.0, "steel.sd2",   "metal.steel");
    public static readonly StudGrade Sd3  = new("sd3",   350.0, 500.0, "steel.sd3",   "metal.chrome");
    public static readonly StudGrade AwsA = new("aws-a", 340.0, 420.0, "steel.aws-a", "metal.steel");
    public static readonly StudGrade AwsB = new("aws-b", 350.0, 450.0, "steel.aws-b", "metal.steel");
    public static readonly ImmutableArray<StudGrade> Rows = [Sd1, Sd2, Sd3, AwsA, AwsB];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);

    // EN 1994-1-1 §6.6.3.1 Eq 6.18 — the STEEL-governed shear resistance of one headed stud, 0.8·fu·A/γV under the
    // code's own 500 MPa ceiling. This is where the grade's published fu becomes structural rather than descriptive,
    // and it is the EN twin of the AISC cap StudClass carries: the two codes price the same stud through their own
    // material column, which is the dual-basis pair the design seam selects between. The concrete-governed Eq 6.19
    // branch needs the slab fck and Ecm and is the Rasm.Compute min(steel, concrete) join, never a column here.
    public double EnShearResistanceKn(StudClass stud) =>
        0.8 * Math.Min(UltimateMpa, EnUltimateCapMpa) * stud.AreaMm2 / EnGammaV * 1e-3;

    // EN 1994-1-1 Eq 6.21 — the height factor the concrete branch weights its resistance by, published over the
    // stud's own overall as-welded height. The forward join reads it; the page owns the relation.
    public static double EnHeightFactor(StudClass stud, double realizedHeightMm) =>
        realizedHeightMm / stud.DiameterMm > 4.0 ? 1.0 : 0.2 * (realizedHeightMm / stud.DiameterMm + 1.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The deposited bead profile (AWS D1.1 weld-profile geometry) — face contour, convex reinforcement above the
// theoretical face, toe radius, root treatment. Descriptive metrics the weld map publishes and the host materializes,
// NEVER the structural throat.
public readonly record struct WeldProfile(WeldFace Face, double ReinforcementMm, double ToeRadiusMm, RootTreatment Root);

// The groove preparation geometry (AWS A2.4 / AWS D1.1): the GrooveGeometry, the Penetration, the BackingType, the
// RootTreatment, and the as-prepared root opening + root face. EffectiveThroatMm is the AISC J2.1 throat: CJP
// develops the full connected-part thickness; PJP the depth-of-prep less the WeldProcess deduction at a sharp bevel
// groove. A prep whose deduction reaches its own depth develops NOTHING — that is a prep specification error, so it
// answers absence and the seed rails it, where a clamp to zero would have seeded a weld with no throat at all.
public readonly record struct GroovePrep(GrooveGeometry Geometry, Penetration Penetration, BackingType Backing, RootTreatment Root, double RootOpeningMm, double RootFaceMm) {
    public const double StandardRootOpeningMm = 2.0;   // AWS D1.1 prequalified open-root defaults — declared ONCE
    public const double StandardRootFaceMm = 1.5;
    public double IncludedAngleDeg => Geometry.IncludedAngleDeg;
    public double BevelAngleDeg => Geometry.BevelAngleDeg;
    public double GrooveRadiusMm => Geometry.RootRadiusMm;
    public Option<double> EffectiveThroatMm(double depthMm, double partThicknessMm, WeldProcess process) =>
        Penetration == Penetration.Cjp
            ? Some(partThicknessMm)
            : Some(depthMm - (Geometry.RequiresPjpDeduction ? process.PjpDeductionMm : 0.0)).Filter(static throat => throat > 0.0);
}

// The common plug/slot hole geometry. DiameterMm is the hole that sets the effective area; DepthMm is the depth of
// FILLING the code polices — it never reduces a resistance, it decides whether the weld conforms at all.
public readonly record struct HoleWeld(PositiveMagnitude DiameterMm, PositiveMagnitude DepthMm) {
    const double ThickMaterialThresholdMm = 16.0;   // AWS D1.1 §4.4.5.4 / AISC J2.3b(h): the 5/8 in breakpoint

    // The required depth of filling: the full thickness at or below 16 mm, else the greater of half the thickness and
    // 16 mm, in no case more than the thinner joined part.
    public static double RequiredFillMm(double partThicknessMm) =>
        partThicknessMm <= ThickMaterialThresholdMm
            ? partThicknessMm
            : Math.Min(partThicknessMm, Math.Max(0.5 * partThicknessMm, ThickMaterialThresholdMm));
}

[Union]
public abstract partial record WeldGeometry {
    private WeldGeometry() { }
    public sealed record Fillet(PositiveMagnitude LegMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record Groove(GroovePrep Prep, WeldProcess Process, PositiveMagnitude DepthMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record Plug(HoleWeld Hole, PositiveMagnitude PartMm) : WeldGeometry;
    public sealed record Slot(HoleWeld Hole, PositiveMagnitude LengthMm, PositiveMagnitude PartMm) : WeldGeometry;
    public sealed record FlareBevel(PositiveMagnitude RadiusMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record FlareV(PositiveMagnitude RadiusMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;

    // The connected-part thickness every weld geometry carries — the one column the weld map's part column reads and
    // the fillet minimum-leg and hole-fill gates both prove against.
    public double PartThicknessMm => Switch(
        fillet: static g => g.PartMm.Value, groove: static g => g.PartMm.Value,
        plug: static g => g.PartMm.Value, slot: static g => g.PartMm.Value,
        flareBevel: static g => g.PartMm.Value, flareV: static g => g.PartMm.Value);
}

// ONE closed seed-row family (SHAPE_BUDGET: the weld/stud/adhesive rows share the admission path, the detail-bag
// consumer, and the Component.Of construction, so they are cases of one [Union], never three sibling structs); the
// throat and design-shear algebra rides each case as DEFINED derived columns, JointDetail/JointSeed dispatch the
// generated total Switch, and a fourth continuous-connection modality is ONE case that breaks every dispatch site at
// compile time. Kind is the base-level policy read the dual IFC binding derives from. Every row in this family
// transcribes AWS D1.1 / ISO 13918 / ASTM geometry and strength verbatim, so provenance is a property of the TABLE
// and states itself once at the table rather than as a per-row column no row ever varies.
[Union]
public abstract partial record JointRow {
    private JointRow(string designation, JointKind kind) { Designation = designation; Kind = kind; }
    public string Designation { get; }
    public JointKind Kind { get; }

    // Each weld case carries only the dimensions and policy its geometry admits.
    public sealed record Weld(string Designation, WeldGeometry Geometry, ElectrodeClass Electrode)
        : JointRow(Designation, JointKind.Weld) {
        public WeldType Type => Geometry.Switch(
            fillet: static _ => WeldType.Fillet,
            groove: static _ => WeldType.Groove,
            plug: static _ => WeldType.Plug,
            slot: static _ => WeldType.Slot,
            flareBevel: static _ => WeldType.FlareBevel,
            flareV: static _ => WeldType.FlareV);

        // The bead profile reads its root treatment from the groove prep that declares one and the open-root default
        // from every other geometry — the prep owns the condition, so a back-gouged groove reports as back-gouged
        // rather than as the as-welded root the type column alone would have asserted.
        public WeldProfile Profile => new(Type.Face, Type.ReinforcementMm, Type.ToeRadiusMm, Geometry.Switch(
            groove: static geometry => geometry.Prep.Root,
            fillet: static _ => RootTreatment.AsWelded, plug: static _ => RootTreatment.AsWelded,
            slot: static _ => RootTreatment.AsWelded, flareBevel: static _ => RootTreatment.AsWelded,
            flareV: static _ => RootTreatment.AsWelded));

        // A THROAT is a line-weld dimension. A plug and a slot have none — they resist on the faying-plane hole area —
        // so the two hole arms answer absence and a consumer reads the shape of the weld out of the Option rather
        // than out of a zero it must know to interpret.
        public Option<double> EffectiveThroatMm => Geometry.Switch(
            fillet: static geometry => Some(0.707 * geometry.LegMm.Value),
            groove: static geometry => geometry.Prep.EffectiveThroatMm(geometry.DepthMm.Value, geometry.PartMm.Value, geometry.Process),
            plug: static _ => Option<double>.None,
            slot: static _ => Option<double>.None,
            flareBevel: static geometry => Some(WeldType.FlareBevel.FlareThroatFactor * geometry.RadiusMm.Value),
            flareV: static geometry => Some(WeldType.FlareV.FlareThroatFactor * geometry.RadiusMm.Value));

        public double LengthMm => Geometry.Switch(
            fillet: static geometry => geometry.RunMm.Value,
            groove: static geometry => geometry.RunMm.Value,
            plug: static geometry => geometry.Hole.DiameterMm.Value,
            slot: static geometry => geometry.LengthMm.Value,
            flareBevel: static geometry => geometry.RunMm.Value,
            flareV: static geometry => geometry.RunMm.Value);

        // The shear-transfer AREA: a LINE arm is throat × run off the ONE EffectiveThroatMm law, a HOLE arm the
        // nominal area of the hole or slot in the plane of the faying surface — a circle for a plug, the obround of a
        // slot's two semicircular ends and its parallel sides. A line weld whose prep develops no throat has no area
        // either, so absence propagates rather than becoming a zero resistance a reader would treat as measured.
        public Option<double> ShearAreaMm2 => Geometry.Switch(
            fillet: _ => EffectiveThroatMm.Map(throat => throat * LengthMm),
            groove: _ => EffectiveThroatMm.Map(throat => throat * LengthMm),
            plug: static geometry => Some(Math.PI * 0.25 * geometry.Hole.DiameterMm.Value * geometry.Hole.DiameterMm.Value),
            slot: static geometry => Some(geometry.Hole.DiameterMm.Value * (geometry.LengthMm.Value - geometry.Hole.DiameterMm.Value)
                + Math.PI * 0.25 * geometry.Hole.DiameterMm.Value * geometry.Hole.DiameterMm.Value),
            flareBevel: _ => EffectiveThroatMm.Map(throat => throat * LengthMm),
            flareV: _ => EffectiveThroatMm.Map(throat => throat * LengthMm));

        public Option<double> DesignShearKn => ShearAreaMm2.Map(area => 0.6 * Electrode.TensileMpa * area * 1e-3);

        public Option<double> DirectionalShearKn(Angle loadAngle) =>
            DesignShearKn.Map(shear => Type.Directional
                ? shear * (1.0 + 0.50 * Math.Pow(Math.Abs(Math.Sin(loadAngle.Radians)), 1.5))
                : shear);

        public MaterialId Substance => Electrode.Substance;
        public MaterialId Appearance => Electrode.Appearance;

        // AISC 360 Table J2.4 minimum fillet leg from the governing thinner connected part — the PUBLISHED metric
        // bounds 6/13/19 mm (1/4 / 1/2 / 3/4 in) -> 3/5/6/8 mm legs, transcribed verbatim (a rounded 20 mm bound
        // under-sizes a 19-20 mm part's leg non-conservatively). It takes the THICKNESS the caller governs with,
        // because a connected part is a thickness to this rule and nothing else — an I-section overload would have
        // made a rolled shape the only connected part expressible and left every plate, angle, and channel outside a
        // rule that governs them identically.
        public static double MinimumFilletLegMm(double thinnerPartMm) => thinnerPartMm switch {
            <= 6.0  => 3.0,
            <= 13.0 => 5.0,
            <= 19.0 => 6.0,
            _       => 8.0
        };

        // AISC 360 J2.2b maximum fillet leg along an edge: the full edge thickness below 6 mm (1/4 in), else the edge
        // thickness less 1.6 mm (1/16 in).
        public static double MaximumFilletLegMm(double edgeThicknessMm) =>
            edgeThicknessMm < 6.0 ? edgeThicknessMm : edgeThicknessMm - 1.6;
    }

    // The stud case: L1 the catalogue length before burn-off, SpacingMm the station-stepped pitch a layout pattern
    // reads. RealizedLengthMm is the ISO 13918 as-welded l2 = l1 − burn-off (DEFINED) and is THE length the shop and
    // the design both mean: the arc consumes the difference, so a layout dimensioned to l1 dimensions a stud that
    // does not exist once welded. DesignShearKn is the AISC steel-side cap at the group the placement declares.
    public sealed record Stud(string Designation, StudClass Class, StudGrade Grade, PositiveMagnitude LengthBeforeWeldMm, PositiveMagnitude SpacingMm)
        : JointRow(Designation, JointKind.Stud) {
        public double RealizedLengthMm => LengthBeforeWeldMm.Value - Class.BurnoffMm;
        public double DesignShearKn(StudGroup group) => Class.SteelShearKn(group);
        public double EnDesignShearKn => Grade.EnShearResistanceKn(Class);
        public MaterialId Substance => Grade.Substance;
    }

    // The adhesive case: BondMm the glueline, OverlapMm the bonded lap / SSG structural bite, WidthMm the joint width.
    public sealed record Adhesive(string Designation, AdhesiveClass Class, PositiveMagnitude BondMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm)
        : JointRow(Designation, JointKind.Adhesive) {
        public double DesignShearKn => Class.LapShearMpa * OverlapMm.Value * WidthMm.Value * 1e-3;
        public Option<double> DesignTensionKn => Class.StructuralBiteMpa.Map(strength => strength * OverlapMm.Value * WidthMm.Value * 1e-3);
        public MaterialId Substance => Class.Substance;
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time realization-bag owner — ONE Of over the ONE JointRow family, the generated total Switch the modality
// dispatch. The weld arm publishes the throat, the run, the CONNECTED-PART thickness, and the full weld-procedure
// block: geometry, penetration, backing, root treatment, root opening and face, and the as-deposited profile. That
// block is what a weld map IS — a shop cannot strike an arc from a throat dimension alone — so the prep vocabulary
// reaches its consumer here rather than describing a joint nothing downstream can read. The stud arm publishes the
// AS-WELDED length and the stud pitch as the layout's own type-level spacing, plus the grade block a mill
// certificate is checked against.
public static class JointDetail {
    public static Fin<PropertyBag> Of(JointRow row, EvidenceGrade source) => row.Switch(
        weld: r =>
            from throat in r.EffectiveThroatMm.Match(
                Some: value => Measured(DetailSchema.EffectiveThroat, Dimension.LengthDim, value * 1e-3).Map(Some),
                None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
            from length in Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.LengthMm * 1e-3)
            from part in Measured(DetailSchema.PartThickness, Dimension.LengthDim, r.Geometry.PartThicknessMm * 1e-3)
            from prep in PrepRow(r)
            select RealizationRows([
                Joint("Welded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), Sourced(source),
                length, part, prep, .. throat.ToSeq(),
            ]),
        stud: r =>
            from diameter in Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, r.Class.DiameterMm * 1e-3)
            from length in Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.RealizedLengthMm * 1e-3)
            from spacing in Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, r.SpacingMm.Value * 1e-3)
            select RealizationRows([
                Joint("Welded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), Sourced(source),
                diameter, length, spacing, GradeRow(r.Grade),
            ]),
        adhesive: r =>
            from bond in Measured(DetailSchema.BondLine, Dimension.LengthDim, r.BondMm.Value * 1e-3)
            from overlap in Measured(DetailSchema.Overlap, Dimension.LengthDim, r.OverlapMm.Value * 1e-3)
            select RealizationRows([
                Joint("Bonded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), Sourced(source), bond, overlap,
            ]));

    // The weld procedure as ONE complex row: the prep a groove declares, the as-deposited profile every geometry
    // carries, and the process. A non-groove weld carries the prep-free half alone.
    static Fin<(PropertyName, PropertyValue)> PrepRow(JointRow.Weld row) =>
        from opening in row.Geometry.Switch(
            groove: static g => Si(g.Prep.RootOpeningMm).Map(Some),
            fillet: static _ => Fin.Succ(Option<PropertyValue>.None), plug: static _ => Fin.Succ(Option<PropertyValue>.None),
            slot: static _ => Fin.Succ(Option<PropertyValue>.None), flareBevel: static _ => Fin.Succ(Option<PropertyValue>.None),
            flareV: static _ => Fin.Succ(Option<PropertyValue>.None))
        from face in Si(row.Geometry.Switch(
            groove: static g => g.Prep.RootFaceMm,
            fillet: static _ => 0.0, plug: static _ => 0.0, slot: static _ => 0.0,
            flareBevel: static _ => 0.0, flareV: static _ => 0.0))
        let profile = row.Profile
        select (DetailSchema.WeldPrep, (PropertyValue)new PropertyValue.Complex("weld-prep", Map(
            (DetailSchema.WeldType, (PropertyValue)new PropertyValue.Text(row.Type.Key)),
            (DetailSchema.Electrode, new PropertyValue.Text(row.Electrode.Key)),
            (DetailSchema.Specification, new PropertyValue.Text(row.Electrode.Specification)),
            (DetailSchema.Face, new PropertyValue.Text(profile.Face.Key)),
            (DetailSchema.RootTreatment, new PropertyValue.Text(profile.Root.Key)),
            (DetailSchema.Reinforcement, new PropertyValue.Text($"{profile.ReinforcementMm:R}")),
            (DetailSchema.ToeRadius, new PropertyValue.Text($"{profile.ToeRadiusMm:R}")),
            (DetailSchema.RootFace, face))
            + row.Geometry.Switch(
                groove: g => Map(
                    (DetailSchema.Groove, (PropertyValue)new PropertyValue.Text(g.Prep.Geometry.Key)),
                    (DetailSchema.Penetration, new PropertyValue.Text(g.Prep.Penetration.Key)),
                    (DetailSchema.Backing, new PropertyValue.Text(g.Prep.Backing.Key)),
                    (DetailSchema.Process, new PropertyValue.Text(g.Process.Key))),
                fillet: static _ => Map<PropertyName, PropertyValue>(), plug: static _ => Map<PropertyName, PropertyValue>(),
                slot: static _ => Map<PropertyName, PropertyValue>(), flareBevel: static _ => Map<PropertyName, PropertyValue>(),
                flareV: static _ => Map<PropertyName, PropertyValue>())
            + opening.Map(static value => Map((DetailSchema.RootOpening, value))).IfNone(Map<PropertyName, PropertyValue>())));

    static (PropertyName, PropertyValue) GradeRow(StudGrade grade) =>
        (DetailSchema.StudGrade, new PropertyValue.Complex("stud-grade", Map(
            (DetailSchema.Grade, (PropertyValue)new PropertyValue.Text(grade.Key)),
            (DetailSchema.YieldStrength, new PropertyValue.Text($"{grade.YieldMpa:R}")),
            (DetailSchema.UltimateStrength, new PropertyValue.Text($"{grade.UltimateMpa:R}")))));

    static Fin<PropertyValue> Si(double mm) =>
        MeasureValue.OfSi(Dimension.LengthDim, mm * 1e-3).Map(static value => (PropertyValue)new PropertyValue.Measure(value));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The joint family seed — ONE closed table over the JointRow family under SEED_ROW_LAW: no vendor producer exists, and
// every AWS D1.1 / ISO 13918 / ASTM value in it is transcribed verbatim, which is a property of the whole table and so
// rides one provenance the three arms share. ComponentFamily.Joint binds Rows; each row's profile answers its own
// section membership. The dual IFC entity binds at seed as the kind row's OWN Binding projection.
public static class JointSeed {
    public static readonly Seq<JointRow> Roster = Seq<JointRow>(
        new JointRow.Weld("joint.weld-fillet-6mm-e70", new WeldGeometry.Fillet(PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(100.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-fillet-8mm-e70", new WeldGeometry.Fillet(PositiveMagnitude.Create(8.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(150.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-fillet-10mm-e80", new WeldGeometry.Fillet(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(16.0), PositiveMagnitude.Create(200.0)), ElectrodeClass.E80),
        new JointRow.Weld("joint.weld-groove-v-cjp-e80", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.SingleV, Penetration.Cjp, BackingType.Steel, RootTreatment.AsWelded, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Saw, PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(250.0)), ElectrodeClass.E80),
        new JointRow.Weld("joint.weld-groove-v-cjp-backgouged-e90", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.DoubleV, Penetration.Cjp, BackingType.None, RootTreatment.Backgouge, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Saw, PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(300.0)), ElectrodeClass.E90),
        new JointRow.Weld("joint.weld-groove-u-sealed-e100", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.SingleU, Penetration.Cjp, BackingType.Ceramic, RootTreatment.SealPass, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Gmaw, PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(400.0)), ElectrodeClass.E100),
        new JointRow.Weld("joint.weld-groove-bevel-pjp-e90", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.SingleBevel, Penetration.Pjp, BackingType.None, RootTreatment.AsWelded, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Smaw, PositiveMagnitude.Create(16.0), PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(300.0)), ElectrodeClass.E90),
        new JointRow.Weld("joint.weld-plug-20mm-e70", new WeldGeometry.Plug(new HoleWeld(PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(12.0)), PositiveMagnitude.Create(12.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-slot-20x60-e70", new WeldGeometry.Slot(new HoleWeld(PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(10.0)), PositiveMagnitude.Create(60.0), PositiveMagnitude.Create(10.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-flarebevel-r10-e70", new WeldGeometry.FlareBevel(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(120.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-flarev-r10-e70", new WeldGeometry.FlareV(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(120.0)), ElectrodeClass.E70),
        new JointRow.Stud("joint.stud-13mm-h75",  StudClass.S13, StudGrade.Sd1,  PositiveMagnitude.Create(75.0),  PositiveMagnitude.Create(150.0)),
        new JointRow.Stud("joint.stud-16mm-h100", StudClass.S16, StudGrade.Sd2,  PositiveMagnitude.Create(100.0), PositiveMagnitude.Create(150.0)),
        new JointRow.Stud("joint.stud-19mm-h100", StudClass.S19, StudGrade.Sd1,  PositiveMagnitude.Create(100.0), PositiveMagnitude.Create(200.0)),
        new JointRow.Stud("joint.stud-22mm-h125", StudClass.S22, StudGrade.AwsB, PositiveMagnitude.Create(125.0), PositiveMagnitude.Create(250.0)),
        new JointRow.Stud("joint.stud-25mm-h150", StudClass.S25, StudGrade.Sd3,  PositiveMagnitude.Create(150.0), PositiveMagnitude.Create(300.0)),
        new JointRow.Adhesive("joint.adhesive-epoxy-2mm", AdhesiveClass.Epoxy,              PositiveMagnitude.Create(2.0),  PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(50.0)),
        new JointRow.Adhesive("joint.adhesive-mma-1mm",   AdhesiveClass.Methacrylate,       PositiveMagnitude.Create(1.0),  PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(40.0)),
        new JointRow.Adhesive("joint.adhesive-pu-2mm",    AdhesiveClass.Polyurethane,       PositiveMagnitude.Create(2.0),  PositiveMagnitude.Create(30.0), PositiveMagnitude.Create(50.0)),
        new JointRow.Adhesive("joint.adhesive-ssg-12mm",  AdhesiveClass.SiliconeStructural, PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(1000.0)));

    // The weld geometry -> SectionProfile arm per ComponentFamily.Joint.admits: FilletTriangle the fillet AND flare
    // welds (the equal-leg gross triangle), Trapezium the groove, Circle the plug hole, and Rectangle the SLOT — a
    // slot's faying footprint is an elongated obround whose bounding rectangle is its width by its length, and
    // profiling it as a circle of its width alone described a footprint a third the size of the one the weld occupies.
    static Fin<SectionProfile> WeldProfileOf(JointRow.Weld row, Op key) => row.Geometry.Switch(
        fillet: geometry => SectionProfile.FilletTriangle.Of(geometry.LegMm.Value, geometry.LegMm.Value, key),
        groove: geometry => GrooveProfile(geometry, key),
        plug: geometry => SectionProfile.Circle.Of(geometry.Hole.DiameterMm.Value, key),
        slot: geometry => SectionProfile.Rectangle.Of(geometry.Hole.DiameterMm.Value, geometry.LengthMm.Value, key),
        flareBevel: geometry => SectionProfile.FilletTriangle.Of(geometry.RadiusMm.Value, geometry.RadiusMm.Value, key),
        flareV: geometry => SectionProfile.FilletTriangle.Of(geometry.RadiusMm.Value, geometry.RadiusMm.Value, key));

    // The groove cross-section as the DEFINED Trapezium derivation over the prep geometry: bottom = the root opening,
    // the walls flaring over the depth-of-prep at the included angle (2·tan(α/2), V/U) or the single-wall bevel angle
    // (tan(β), bevel/J — TopOffset shifting the asymmetric single-wall prep); a square groove degenerates to the
    // equal-width slit. The throat stays GroovePrep.EffectiveThroatMm — the profile is the gross prep envelope only.
    static Fin<SectionProfile> GrooveProfile(WeldGeometry.Groove geometry, Op key) {
        GroovePrep p = geometry.Prep;
        double flare = p.IncludedAngleDeg > 0.0 ? 2.0 * Math.Tan(p.IncludedAngleDeg * Math.PI / 360.0) : Math.Tan(p.BevelAngleDeg * Math.PI / 180.0);
        double top = p.RootOpeningMm + geometry.DepthMm.Value * flare;
        return SectionProfile.Trapezium.Of(
            bottomWidthMm: p.RootOpeningMm, topWidthMm: top, depthMm: geometry.DepthMm.Value,
            topOffsetMm: p.BevelAngleDeg > 0.0 ? (top - p.RootOpeningMm) / 2.0 : 0.0, key);
    }

    // The per-geometry seed gates, one arm each: a fillet proves the AISC J2.4 minimum leg against its own thinner
    // part; a groove proves its prep survives its process deduction, which the throat Option already decides; a plug
    // or slot proves the AWS §4.4.5.4 depth of filling and, for a slot, that its length reaches its own width, since
    // an obround shorter than it is wide is not a slot. The J2.2b edge maximum stays a design-check read — it is
    // T-versus-lap configuration-dependent and no seed row declares its configuration.
    static Fin<Unit> AdmitWeld(JointRow.Weld row, Op key) => row.Geometry.Switch(
        fillet: geometry => guard(geometry.LegMm.Value >= JointRow.Weld.MinimumFilletLegMm(geometry.PartMm.Value),
            new KernelFault.InvalidValue(nameof(SectionProfile.FilletTriangle), "a fillet leg meeting the J2.4 minimum", Some(key))).ToFin(),
        groove: geometry => guard(row.EffectiveThroatMm.IsSome,
            new KernelFault.InvalidValue(nameof(row.EffectiveThroatMm), "a PJP effective throat after process deduction", Some(key))).ToFin(),
        plug: geometry => guard(geometry.Hole.DepthMm.Value >= HoleWeld.RequiredFillMm(geometry.PartMm.Value),
            new KernelFault.InvalidValue(nameof(SectionProfile.Circle), "plug fill meeting the AWS minimum", Some(key))).ToFin(),
        slot: geometry => from filled in guard(geometry.Hole.DepthMm.Value >= HoleWeld.RequiredFillMm(geometry.PartMm.Value),
                new KernelFault.InvalidValue(nameof(SectionProfile.Outline), "slot fill meeting the AWS minimum", Some(key)))
            from shaped in guard(geometry.LengthMm.Value >= geometry.Hole.DiameterMm.Value,
                new KernelFault.InvalidValue(nameof(geometry.LengthMm), "at least the slot width", Some(key)))
            select unit,
        flareBevel: static _ => Fin.Succ(unit), flareV: static _ => Fin.Succ(unit));

    // The seed POLICY value, every selector ONE total Switch over the JointRow family so a fourth modality breaks
    // each of them at compile time. The AUTHORITY rides the row's own standards body — AWS for the weld and stud
    // tables (D1.1 welds, D1.1 Type B studs over the ISO 13918 geometry), ASTM for the adhesive table
    // (D1002/D1876/C1401) — never one blended authority, and the region derives from that body rather than a
    // per-lane ComponentStandard static; a continuous connection carries no masonry-style joint thickness. Both stud
    // MaterialId slots ride the GRADE row, so the SD3 stainless renders chromium and never blanket steel. Every value
    // in this table transcribes its standard verbatim, which is a property of the TABLE, so one evidence grade serves
    // all three arms rather than a per-row column no row varies.
    public static readonly SeedLaw<JointRow> Law = SeedLaw<JointRow>.Of(
        family: ComponentFamily.Joint,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: ProfileOf,
        substance: static row => row.Switch(
            weld: static r => r.Substance, stud: static r => r.Substance, adhesive: static r => r.Substance),
        source: static _ => EvidenceGrade.Catalogue,
        standard: static row => new ComponentStandard(Body(row).Region, StandardJointThicknessMm: 0.0, Body(row)),
        detail: Some<Func<JointRow, SectionProfile, Op, Fin<PropertyBag>>>(static (row, _, _) => JointDetail.Of(row, EvidenceGrade.Catalogue)),
        appearance: static row => row.Switch(
            weld: static r => r.Appearance,
            stud: static r => r.Grade.Appearance,
            adhesive: static _ => MaterialId.Of("polymer.adhesive")),
        ifc: static row => row.Kind.Binding);

    static ComponentAuthority Body(JointRow row) => row.Switch(
        weld: static _ => ComponentAuthority.Aws,
        stud: static _ => ComponentAuthority.Aws,
        adhesive: static _ => ComponentAuthority.Astm);

    // The row census, ACCUMULATING across the modality's own gates: a weld proves its geometry admission (the AISC
    // J2.4 minimum leg, the PJP prep surviving its process deduction, the AWS §4.4.5.4 depth of filling and the
    // slot's own aspect) and a stud proves the ISO 13918 l2 = l1 − burn-off positive, so a roster with three bad rows
    // names all three in ONE verdict instead of aborting at the first. An adhesive row's dimensions are already
    // PositiveMagnitude and carry no further law.
    static Validation<Error, Unit> Coherence(JointRow row, Op key) => row.Switch(
        weld: r => AdmitWeld(r, key).ToValidation().Map(static _ => unit),
        stud: r => key.AcceptValidated<PositiveMagnitude>(candidate: r.RealizedLengthMm).ToValidation().Map(static _ => unit),
        adhesive: static _ => Validation<Error, Unit>.Success(unit));

    // The profile route, ONE Switch: the weld geometry's own arm, the stud shank's circle, the adhesive glueline's
    // nominal.
    static Fin<SectionProfile> ProfileOf(JointRow row, Op key) => row.Switch(
        weld: r => WeldProfileOf(r, key),
        stud: r => SectionProfile.Circle.Of(diameterMm: r.Class.DiameterMm, key),
        adhesive: r => SectionProfile.Nominal.Of(nominalMm: r.BondMm.Value, key));

    // The designation-keyed join. JointRow is a class-root [Union] rather than a struct, so the railed SeedJoin —
    // whose admission is constrained to value rows — cannot carry it; the admission it performs is instead the seed
    // law's own, and this table is a pure projection of designations already proven there.
    static readonly FrozenDictionary<ComponentId, JointRow> Rowset =
        Roster.ToFrozenDictionary(static row => ComponentId.Create(row.Designation), static row => row);

    public static Fin<JointRow> Resolve(Component component, Op key) =>
        Rowset.TryGetValue(component.Designation, out JointRow row)
            ? Fin.Succ(row)
            : new ComponentFault.ComponentMissing(key, ProfileRef.Of(component.Designation.Value));

    // The ComponentFamily.Joint CAPACITY producer: the resolved row selects its receipt case, and the placement
    // carries the facts a continuous connection cannot hold on its own row — the weld's load angle, and the stud
    // group's count together with the deck relation and rib position its Rg·Rp pair is keyed on. A weld/bond/stud is
    // unsectioned, so the section argument is structurally absent.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        Resolve(component, key).Bind(row => SectionCapacity.Lift(row.Switch<CapacityReceipt>(
            weld: r => new CapacityReceipt.Weld(component.Designation, r, placement.LoadAngleDeg),
            adhesive: r => new CapacityReceipt.Adhesive(component.Designation, r),
            stud: r => new CapacityReceipt.Stud(component.Designation, r, placement.StudGroup, placement.StudCount)), key));
}
```

## [03]-[RESEARCH]

(none)
