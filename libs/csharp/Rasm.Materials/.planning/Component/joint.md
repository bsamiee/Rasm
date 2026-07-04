# [MATERIALS_JOINT]

THE JOINT SEED PAGE — the `joint` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Realization`), the continuous-connection weld/adhesive/stud vocabulary. A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed part: it carries no thread or bar cross-section, so it cannot fold into `fastener` the way `anchor` does — the ONE deliberate widening past the discrete triple, load-bearing because `steel#STEEL_FAMILY`'s `Composite` arm reads `StudClass.SteelShearKn` from here for its `ΣQn` cap. An 8 mm fillet weld is a `Component` row in the `joint` family, never a `Weld` type: geometry lands as the `SectionProfile` arm the family admits (`FilletTriangle` the fillet and flare welds — the `0.707·leg` throat staying the family's DEFINED derivation; `Trapezium` the PJP/CJP groove derived from the prep geometry; `Circle` the plug/slot hole and the stud shank; `Nominal` the continuous adhesive bond-line), the realization scalars land in the `JointDetail` `DetailSchema.Realization` `PropertyBag`, and the strength axes are frozen row tables with per-column provenance. `JointSeed` binds the DUAL IFC entity at seed per the IFC-BINDING law — `IfcBinding.Of(kind == JointKind.Stud ? "IfcMechanicalFastener" : "IfcFastener", kind.IfcPredefinedType)` — an `IfcMechanicalFastener` `STUDSHEARCONNECTOR` for the welded stud, an `IfcFastener` `WELD`/`GLUE` for the weld/adhesive bead. The vocabulary grows by data: a new electrode/adhesive/stud diameter/stud grade is one row in its frozen table, a new groove one `GrooveGeometry` row, a new designation one `WeldRow`/`StudRow`/`AdhesiveRow` seed entry — never a per-joint type. The generative geometry is the hand-rolled AWS D1.1:2020 + AISC 360 J2/I8 + ISO 13918:2017 + EN 1994 capture (GeometryGym mirrors only the IFC class; VividOrange owns no weld); the host materializes the bead/groove/stud solid from the scalar receipt, NEVER a host `Curve` here. `DesignShearKn` emits the SPEC-NOMINAL filler/adhesive/stud-steel band; the measured base-metal capacity is the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt read by `MaterialId`, and the composite stud's concrete branch is the `Rasm.Compute/structural#DESIGN_CHECK` join — neither a column here. The AISC J2.4 minimum fillet leg reads the connected member's `VividOrange.Profiles` `II` `FlangeThickness`/`WebThickness`; the EN 1993-1-8 / EN 1994 design codes are NAMED as typed `VividOrange.Standards` citations. The page composes `Component/component#COMPONENT_OWNER` (`Component.Of`, `ComponentRow`, `SectionProfile`, `IfcBinding`, `ComponentDetail`, `ComponentFault`), the seam `Properties/property#DETAIL_SCHEMA` realization rows, `Rasm.Numerics` `PositiveMagnitude`, and the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` two-slot law.

## [01]-[INDEX]

- [02]-[JOINT_FAMILY]: the `JointKind` weld/adhesive/stud discriminant with its `IfcPredefinedType`; the `WeldType` 6-geometry axis with the `LineWeld` design-shear discriminant, the `Directional` Eq J2-5 kds-eligibility column, and the AWS flare-throat factor; the `WeldProcess`/`GrooveGeometry`/`Penetration`/`WeldFace`/`RootTreatment`/`BackingType` welding sub-axes (FORM-law policy SmartEnums); the `ElectrodeClass` (AWS A5.1/A5.5), `AdhesiveClass` (ASTM D1002/D1876/C1401), `StudClass` (ISO 13918 Type SD + the AISC `SteelShearKn` DEFINED column), and `StudGrade` (SD1/SD2/SD3 + AWS A/B) frozen row tables; the `WeldProfile`/`GroovePrep`/`PlugSlot` generative-geometry records; the `WeldRow`/`StudRow`/`AdhesiveRow` seed rows carrying the throat/design-shear/directional algebra as DEFINED derived columns; the `JointDetail` realization-bag owner; the typed `En1993`/`En1994` design-code citations; and the `JointSeed` tables with the ONE `Rows : Context -> Fin<Seq<ComponentRow>>` fold (per-table `Traverse` joined applicatively).

## [02]-[JOINT_FAMILY]

- Owner: the joint vocabulary (`JointKind` the kind discriminant carrying the `IfcPredefinedType`; `WeldType` the 6 AWS D1.1 weld geometries; `WeldProcess` the AISC J2.1 PJP-deduction axis; `GrooveGeometry` the 9 AWS A2.4 groove geometries; `Penetration` CJP/PJP; `WeldFace`/`RootTreatment` the bead-profile axes; `BackingType` the groove backing — policy SmartEnums with behavior columns); the `ElectrodeClass`/`AdhesiveClass`/`StudClass`/`StudGrade` FROZEN `readonly record struct` row tables (pure standards data, per-column `PUBLISHED`/`DEFINED` provenance); the `WeldProfile`/`GroovePrep`/`PlugSlot` generative records; the `WeldRow`/`StudRow`/`AdhesiveRow` seed rows owning the throat/design-shear algebra as derived columns; `JointDetail` the `DetailSchema.Realization` bag owner; `JointSeed` the three seed tables, the `SectionProfile` derivations, the AISC J2.4/J2.2b fillet-leg rules, and the `Rows` fold `ComponentFamily.Joint` binds.
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `GrooveGeometry` × `ElectrodeClass` × `WeldProcess`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass` × `StudGrade`)}; weld {fillet · groove (square/V/bevel/U/J × single/double on `GrooveGeometry`) · plug · slot · flare-bevel · flare-v} over the E60..E110 electrode band; adhesive {epoxy · methacrylate · polyurethane · silicone-structural} over the lap-shear/peel/SSG-bite band; stud {the 13..25 mm ISO 13918 Type SD headed connectors} over diameter × grade × height × spacing. A joint is a `Component` row in `ComponentFamily.Joint`, never a joint subtype; the groove subtype is a `GrooveGeometry` row, never a per-subtype `WeldType` (a 14-member `WeldType` duplicating the 9 `GrooveGeometry` cases is the deleted form).
- Entry: `JointSeed.Rows(Context) : Fin<Seq<ComponentRow>>` — the three typed seed tables `Traverse` applicatively and join through the tuple `Apply` (the prior fault-swallowing `Choose` + three-way `.Concat` retired; `Fin`'s applicative UNIONS the faults so every malformed row across all three tables reports in ONE build abort): per row, the dimensional columns gate through `key.AcceptValidated<PositiveMagnitude>` (a non-positive part thickness, run length, overlap, or post-burn-off stud length faults), a fillet leg proves the AISC J2.4 minimum against its own thinner-part column, the `SectionProfile` arm rails through its `Of` factory, `JointDetail.Of(row)` builds the byte-identical realization bag, and `Component.Of(..., detail: Some(bag), key)` seals the row — every row `Sectioned: false` (a weld/bond/stud fills no `ComputedSection`). The design algebra rides the rows: `WeldRow.DesignShearKn` the AWS D1.1 line `0.6·FEXX·throat·length` or plug/slot area-shear `0.6·FEXX·ShearAreaMm2`; `WeldRow.DirectionalShearKn(loadAngleDeg)` the AISC 360 Eq J2-5 `1 + 0.50·sin^1.5θ` fillet/flare increase; `StudRow.DesignShearKn` the AISC 360-22 Eq I8-1 steel-side `StudClass.SteelShearKn = Rg·Rp·Asc·Fu`; `AdhesiveRow.DesignShearKn`/`DesignTensionKn` the ASTM D1002 `LapShearMpa·overlap·width` lap-shear and the ASTM C1401 `StructuralBiteMpa` SSG bite-tension pair — each a TOTAL projection over the row's own spec band, no `resolve` func, no base-metal/concrete read.
- Packages: Rasm.Numerics (`PositiveMagnitude` — throat/leg/size/length/bond-line/overlap/width/spacing, never an int-backed count that truncates a fractional throat), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `Dimension`, `PropertyBag`), Rasm.Materials.Component (`Component`/`ComponentRow`/`SectionProfile`/`IfcBinding`/`ComponentDetail`/`ComponentFault`/`Coring`/`ComponentStandard`/`ComponentAuthority`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + generated total `Switch` for the policy axes), VividOrange.Profiles (`II` — the AISC J2.4 `FlangeThickness`/`WebThickness` connected-part read, the ONE VividOrange geometry composition), VividOrange.Standards (`En1993`/`En1993Part.Part1_8`, `En1994`, `NationalAnnex` — the typed Eurocode citations replacing inline code strings; AWS/AISC/ISO/ASTM have no VividOrange body, so those stay `PUBLISHED` provenance on the rows), UnitsNet (`Length.Millimeters` at the `II` read edge), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`Option`), BCL inbox (`ImmutableArray`).
- Growth: a new weld geometry is one `WeldType` row; a new groove one `GrooveGeometry` row carrying its angle/radius; a new electrode one `ElectrodeClass` row (FEXX + specification); a new adhesive one `AdhesiveClass` row; a new stud diameter one `StudClass` row (ISO 13918 geometry, the `SteelShearKn` column deriving); a new stud grade one `StudGrade` row (`fy`/`fu`); a new designation one `WeldRow`/`StudRow`/`AdhesiveRow` seed entry — zero central edits, never a `Weld`/`AdhesiveBond`/`ShearStud` sibling type. A structural-joint utilisation case is future `capacity#SECTION_CAPACITY` growth, not this page.
- Boundary: the strength axes are FROZEN ROW TABLES, not SmartEnums — `ElectrodeClass`/`StudGrade`/`AdhesiveClass`/`StudClass` carry no runtime key lookup, no delegate column, and no IFC-token derivation, so FORM law converts them 1:1 to `readonly record struct` rows (values verbatim, `PUBLISHED` provenance; `StudClass.AreaMm2`/`SteelShearKn` the `DEFINED` `πd²/4` and AISC Eq I8-1 derivations) — the kept SmartEnums (`JointKind`, `WeldType`, `WeldProcess`, `GrooveGeometry`, `Penetration`, `WeldFace`, `RootTreatment`, `BackingType`) each carry real policy behavior (`WeldType.LineWeld`/`Directional`/`FlareThroatFactor`, `WeldProcess.PjpDeductionMm`, `GrooveGeometry.RequiresPjpDeduction`, `Penetration.Complete`). `StudClass.SteelShearKn` is the ONE shear-stud cap `steel#STEEL_FAMILY`'s `Composite` arm reads for `ΣQn` — the row-table conversion preserves the symbol path (`StudClass.S19.SteelShearKn`) and the value byte-identically (`Rg = 1.0`, `Rp = 0.75`, `Fu = 450 MPa`; a weak-position or multi-stud-per-rib layout is a `Rasm.Compute` placement input, never a vocabulary edit). The weld geometry is the scalar `WeldProfile`/`GroovePrep`/`PlugSlot` receipt — derived projections on `WeldRow` from the SmartEnum defaults — the host lofts the solid from the scalar throat, so this owner stays host-neutral; the `Trapezium` groove section is the DEFINED derivation over the prep geometry (bottom = root opening, walls flaring at the included/bevel angle over the depth). Capacity SPLITS by source: `DesignShearKn` is the SPEC-NOMINAL filler/adhesive/stud-steel band (`ElectrodeClass.TensileMpa` FEXX, `AdhesiveClass.LapShearMpa`, `StudClass.UltimateMpa`); the measured base-metal capacity crosses to the `Mechanical` receipt by `SubstanceId`; the composite stud's concrete-governed `0.5·Asc·√(f'c·Ec)` branch is the `Rasm.Compute/structural#DESIGN_CHECK` `min(steel, concrete)` join. The appearance crosses as the two-slot `MaterialId` law (`metal.iron`/`metal.steel` by electrode band, `polymer.adhesive`, GRADE-borne stud — `metal.chrome` the SD3 stainless, `metal.steel` the carbon grades). The item reaches `Rasm.Bim` ONLY as the projector-authored seam node plus the neutral bag (`EffectiveThroat`/`NominalLength` weld, `BondLine`/`Overlap` adhesive, `NominalDiameter`/`NominalLength` stud — byte-identical rows), the `Semantics/connection#CONNECTION_DETAIL` reader round-tripping them and the egress serializing the dual entity plus an `IfcRelConnectsWithRealizingElements` edge; the stud token is `STUDSHEARCONNECTOR` (`STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, never a fastener value). A weld schedule or stud pattern is a station-stepped `Construction`/spec layout read over the seeded spacing, never a parallel joint-layout owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;  // ImmutableArray (the frozen standards row tables)
using LanguageExt;
using Rasm.Numerics;                  // PositiveMagnitude (>0 finite — throat/leg/size/length/bond-line/overlap/width/spacing)
using Rasm.Domain;                   // Context, Op, AcceptValidated
using Rasm.Element;                  // MaterialId, DetailSchema, PropertyBag
using Thinktecture;
using UnitsNet;                      // Length (.Millimeters on the II.FlangeThickness/WebThickness reads)
using VividOrange.Profiles;          // II (the I-section thickness pair the AISC J2.4 MinimumFilletLegMm read consumes)
using VividOrange.Standards.Eurocode; // En1993/En1993Part, En1994, NationalAnnex (the typed design-code citations)
using Dimension = Rasm.Element.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;
using static Rasm.Materials.Component.ComponentDetail;   // Joint / Token / Measured / RealizationRows (the relocated bag constructors)

// Timber/glazing/fastener/connector/joint seeds share the parent namespace; masonry/cmu/steel/panel/reinforcement
// live in child leaves the owner prelude aliases (component#COMPONENT_OWNER). This page DEFINES StudClass; the
// steel#STEEL_FAMILY Composite arm reads StudClass.S19.SteelShearKn from this parent namespace.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The continuous-connection kind discriminant the seed tables and the IFC binding read; a value discriminant, NEVER a
// payload union (the payload lives on the typed seed rows). IfcPredefinedType is the ONE per-kind token the seed-time
// IfcBinding carries and the bag's FastenerType row round-trips: the welded stud is the IfcMechanicalFastenerTypeEnum
// STUDSHEARCONNECTOR, the weld bead the IfcFastenerTypeEnum WELD, the adhesive bead GLUE (STUD is the
// IfcReinforcingBarTypeEnum cast-in bar, never a fastener value).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointKind {
    public static readonly JointKind Weld     = new("weld",     ifcPredefinedType: "WELD");
    public static readonly JointKind Adhesive = new("adhesive", ifcPredefinedType: "GLUE");
    public static readonly JointKind Stud     = new("stud",     ifcPredefinedType: "STUDSHEARCONNECTOR");
    public string IfcPredefinedType { get; }
}

// The 6 AWS D1.1 weld GEOMETRIES. The groove SUBTYPE geometry (square/V/bevel/U/J × single/double) lives on
// GrooveGeometry, NOT a per-subtype WeldType (the no-split collapse). LineWeld flags the DesignShearKn branch: a
// fillet/groove/flare resists on throat × length line-shear, a plug/slot on the faying-plane HOLE AREA (Hole derives
// from !LineWeld — the non-line welds ARE the hole welds). Directional flags the AISC Eq J2-5 kds eligibility: the
// fillet/flare throat-shear welds take the directional increase, a groove develops base metal and a plug/slot resists
// on hole area. FlareThroatFactor is the AWS D1.1 Table 5.2 flare-groove radius factor (5/16·R flare-bevel, 1/2·R
// flare-V — the non-SAW common value).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldType {
    public static readonly WeldType Fillet     = new("fillet",      lineWeld: true,  directional: true,  flareThroatFactor: 0.0);
    public static readonly WeldType Groove     = new("groove",      lineWeld: true,  directional: false, flareThroatFactor: 0.0);
    public static readonly WeldType Plug       = new("plug",        lineWeld: false, directional: false, flareThroatFactor: 0.0);
    public static readonly WeldType Slot       = new("slot",        lineWeld: false, directional: false, flareThroatFactor: 0.0);
    public static readonly WeldType FlareBevel = new("flare-bevel", lineWeld: true,  directional: true,  flareThroatFactor: 0.3125);
    public static readonly WeldType FlareV     = new("flare-v",     lineWeld: true,  directional: true,  flareThroatFactor: 0.5);
    public bool LineWeld { get; }
    public bool Directional { get; }
    public double FlareThroatFactor { get; }
}

// AISC 360 Table J2.1 PJP effective-throat deduction: SMAW/GMAW/FCAW deduct 3 mm (1/8 in) at a sharp (<60°) bevel groove
// where reliable root fusion is process-limited; SAW's deeper penetration takes the FULL groove depth (no deduction —
// AISC J2.1 deducts only for SMAW/GMAW/FCAW). The one PJP throat-loss column GroovePrep.EffectiveThroatMm reads.
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
// RootRadiusMm the U/J root radius (U = 6 mm, J = 10 mm standard prep radii). DoubleSided flags a both-face prep. A sharp
// bevel groove (45°, no radius) takes the WeldProcess PJP deduction; a 60° V, a radiused U/J, or any CJP develops the
// full depth (RequiresPjpDeduction false).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GrooveGeometry {
    public static readonly GrooveGeometry Square      = new("square",       includedAngleDeg: 0.0,  bevelAngleDeg: 0.0,  rootRadiusMm: 0.0,  doubleSided: false);
    public static readonly GrooveGeometry SingleV     = new("single-v",     includedAngleDeg: 60.0, bevelAngleDeg: 0.0,  rootRadiusMm: 0.0,  doubleSided: false);
    public static readonly GrooveGeometry DoubleV     = new("double-v",     includedAngleDeg: 60.0, bevelAngleDeg: 0.0,  rootRadiusMm: 0.0,  doubleSided: true);
    public static readonly GrooveGeometry SingleBevel = new("single-bevel", includedAngleDeg: 0.0,  bevelAngleDeg: 45.0, rootRadiusMm: 0.0,  doubleSided: false);
    public static readonly GrooveGeometry DoubleBevel = new("double-bevel", includedAngleDeg: 0.0,  bevelAngleDeg: 45.0, rootRadiusMm: 0.0,  doubleSided: true);
    public static readonly GrooveGeometry SingleU     = new("single-u",     includedAngleDeg: 20.0, bevelAngleDeg: 0.0,  rootRadiusMm: 6.0,  doubleSided: false);
    public static readonly GrooveGeometry DoubleU     = new("double-u",     includedAngleDeg: 20.0, bevelAngleDeg: 0.0,  rootRadiusMm: 6.0,  doubleSided: true);
    public static readonly GrooveGeometry SingleJ     = new("single-j",     includedAngleDeg: 0.0,  bevelAngleDeg: 20.0, rootRadiusMm: 10.0, doubleSided: false);
    public static readonly GrooveGeometry DoubleJ     = new("double-j",     includedAngleDeg: 0.0,  bevelAngleDeg: 20.0, rootRadiusMm: 10.0, doubleSided: true);
    public double IncludedAngleDeg { get; }
    public double BevelAngleDeg { get; }
    public double RootRadiusMm { get; }
    public bool DoubleSided { get; }
    public bool RequiresPjpDeduction => BevelAngleDeg is > 0.0 and <= 45.0 && RootRadiusMm <= 0.0;
}

// CJP develops the full connected-part thickness (the weld matches the base metal); PJP develops only the depth of
// preparation, reduced by the WeldProcess deduction at a sharp groove (AISC J2.1). The throat rule reads Complete.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Penetration {
    public static readonly Penetration Cjp = new("cjp", complete: true);
    public static readonly Penetration Pjp = new("pjp", complete: false);
    public bool Complete { get; }
}

// The bead face contour (AWS D1.1 weld-profile acceptance): flat, convex (fillet reinforcement), concave. The face
// describes the deposited reinforcement; the effective throat is measured to the THEORETICAL face, never the convex
// reinforcement, so the face is descriptive not structural.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldFace {
    public static readonly WeldFace Flat    = new("flat");
    public static readonly WeldFace Convex  = new("convex");
    public static readonly WeldFace Concave = new("concave");
}

// The root condition — the root-treatment axis SPLIT OUT of the groove BackingType (a back-gouged + back-welded root is
// a root TREATMENT, the backing bar a groove MATERIAL): AsWelded the open root, Backgouge gouged to sound metal and
// back-welded, SealPass a seal weld over the root.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RootTreatment {
    public static readonly RootTreatment AsWelded  = new("as-welded");
    public static readonly RootTreatment Backgouge = new("backgouge");
    public static readonly RootTreatment SealPass  = new("seal-pass");
}

// The groove backing material — None for an open/back-gouged root, Steel for a fused backing bar, Ceramic/Copper/Flux
// the removable/consumable backings. Distinct from RootTreatment: backing is a MATERIAL, root-treatment a CONDITION.
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

// AWS A5.1 carbon-steel (E60/E70) and AWS A5.5 low-alloy (E80..E110) covered-electrode classifications — a FROZEN row
// table (FORM law: pure standards data, no key lookup, no delegate). All columns PUBLISHED: TensileMpa the FEXX minimum
// filler tensile; Specification the AWS body (E80+ are A5.5, NOT A5.1); the appearance is the weld finish (metal.iron
// the lowest carbon-steel filler, metal.steel above) the two-slot law reads.
public readonly record struct ElectrodeClass(string Key, double TensileMpa, string Specification, string SubstanceId, string AppearanceId) {
    public static readonly ElectrodeClass E60  = new("e60",  415.0, "AWS A5.1", "steel.e60",  "metal.iron");
    public static readonly ElectrodeClass E70  = new("e70",  485.0, "AWS A5.1", "steel.e70",  "metal.steel");
    public static readonly ElectrodeClass E80  = new("e80",  550.0, "AWS A5.5", "steel.e80",  "metal.steel");
    public static readonly ElectrodeClass E90  = new("e90",  620.0, "AWS A5.5", "steel.e90",  "metal.steel");
    public static readonly ElectrodeClass E100 = new("e100", 690.0, "AWS A5.5", "steel.e100", "metal.steel");
    public static readonly ElectrodeClass E110 = new("e110", 760.0, "AWS A5.5", "steel.e110", "metal.steel");
    public static readonly ImmutableArray<ElectrodeClass> Rows = [E60, E70, E80, E90, E100, E110];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

// Structural-adhesive allowables — a FROZEN row table. All columns PUBLISHED: LapShearMpa (ASTM D1002 single-lap),
// PeelNmm (ASTM D1876 T-peel), ServiceCelsius, StructuralBiteMpa (the ASTM C1401 SSG design tensile the silicone
// curtain-wall bite develops — 0.14 MPa / 20 psi, distinct from its cured lap-shear).
public readonly record struct AdhesiveClass(string Key, double LapShearMpa, double PeelNmm, double ServiceCelsius, double StructuralBiteMpa, string SubstanceId) {
    public static readonly AdhesiveClass Epoxy              = new("epoxy",               30.0, 5.0,  80.0,  6.0,  "adhesive.epoxy");
    public static readonly AdhesiveClass Methacrylate       = new("methacrylate",        25.0, 12.0, 100.0, 5.0,  "adhesive.methacrylate");
    public static readonly AdhesiveClass Polyurethane       = new("polyurethane",        15.0, 20.0, 90.0,  2.0,  "adhesive.polyurethane");
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", 1.0,  8.0,  150.0, 0.14, "sealant.silicone-structural");
    public static readonly ImmutableArray<AdhesiveClass> Rows = [Epoxy, Methacrylate, Polyurethane, SiliconeStructural];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
}

// ISO 13918:2017 Type SD headed shear connectors (AWS D1.1 Type B equivalent) — a FROZEN row table keyed by the nominal
// shank. PUBLISHED columns: DiameterMm (ISO d), HeadDiameterMm (dc/d5), HeadThicknessMm (h3), WeldCollarDiameterMm
// (d1/d3, the as-welded base fillet — the collar IS the weld footprint, no separate weld-area column), WeldCollarHeightMm
// (h/h4), BurnoffMm (the L1−L2 arc consumption), UltimateMpa (the AWS D1.1 Type B / AISC §I8 Fu = 450 MPa nominal cap).
// DEFINED columns: AreaMm2 = πd²/4; SteelShearKn = Rg·Rp·Asc·Fu (AISC 360-22 Eq I8-1, the conservative strong-position
// Rg = 1.0 / Rp = 0.75 default — a weak-position or multi-stud-per-rib layout is a Rasm.Compute placement input, never a
// vocabulary edit). This is the ONE shear-stud cap steel#STEEL_FAMILY's Composite arm reads for ΣQn — the symbol path
// (StudClass.S19.SteelShearKn) and every value are FROZEN byte-identical across the row-table conversion.
public readonly record struct StudClass(string Key, double DiameterMm, double HeadDiameterMm, double HeadThicknessMm, double WeldCollarDiameterMm, double WeldCollarHeightMm, double BurnoffMm, double UltimateMpa) {
    public static readonly StudClass S13 = new("stud-1/2", 12.7, 25.0, 8.0,  17.0, 3.0, 3.0,  450.0);
    public static readonly StudClass S16 = new("stud-5/8", 15.9, 32.0, 8.0,  21.0, 4.5, 4.0,  450.0);
    public static readonly StudClass S19 = new("stud-3/4", 19.1, 32.0, 10.0, 23.0, 6.0, 4.5,  450.0);
    public static readonly StudClass S22 = new("stud-7/8", 22.2, 35.0, 10.0, 29.0, 6.0, 5.0,  450.0);
    public static readonly StudClass S25 = new("stud-1",   25.4, 40.0, 12.0, 31.0, 7.0, 5.5,  450.0);
    public static readonly ImmutableArray<StudClass> Rows = [S13, S16, S19, S22, S25];
    public const double TipAngleDeg = 140.0;      // ISO 13918 140° ± 7° point
    public const double GroupFactorRg = 1.0;      // AISC I8-1 strong-position defaults — POLICY constants, not per-row data
    public const double PositionFactorRp = 0.75;
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
    public double SteelShearKn => GroupFactorRg * PositionFactorRp * AreaMm2 * UltimateMpa * 1e-3;
}

// ISO 13918:2017 SD material grades + AWS D1.1 stud types — a FROZEN row table. PUBLISHED columns: the specified fy/fu
// the EN 1994 §6.6.3.1 PRd path reads (SD1 = S235J2G3+C450 the standard carbon stud 350/450; SD2 the higher-elongation
// carbon 235/400; SD3 = X5CrNi18-10 austenitic stainless 350/500; AWS Type A general 340/420; Type B shear connector
// 350/450). EN 1994 caps the design fu at 500 MPa; AISC §I8 caps Fu at 450 (the StudClass column). Appearance is
// GRADE-borne per the two-slot law: the SD3 stainless renders the library chromium conductor row (the passive-layer
// optical response), the carbon grades plain steel.
public readonly record struct StudGrade(string Key, double YieldMpa, double UltimateMpa, string SubstanceId, string AppearanceId) {
    public static readonly StudGrade Sd1  = new("sd1",   350.0, 450.0, "steel.sd1",   "metal.steel");
    public static readonly StudGrade Sd2  = new("sd2",   235.0, 400.0, "steel.sd2",   "metal.steel");
    public static readonly StudGrade Sd3  = new("sd3",   350.0, 500.0, "steel.sd3",   "metal.chrome");
    public static readonly StudGrade AwsA = new("aws-a", 340.0, 420.0, "steel.aws-a", "metal.steel");
    public static readonly StudGrade AwsB = new("aws-b", 350.0, 450.0, "steel.aws-b", "metal.steel");
    public static readonly ImmutableArray<StudGrade> Rows = [Sd1, Sd2, Sd3, AwsA, AwsB];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The deposited bead profile (AWS D1.1 weld-profile geometry) — face contour, convex reinforcement above the theoretical
// face, toe radius, root treatment. ReinforcementMm / ToeRadiusMm are non-negative doubles (a flat face has 0
// reinforcement, a sharp toe 0 radius) — descriptive metrics the host materializes, NEVER the structural throat.
public readonly record struct WeldProfile(WeldFace Face, double ReinforcementMm, double ToeRadiusMm, RootTreatment Root);

// The groove preparation geometry (AWS A2.4 / AWS D1.1): the GrooveGeometry (angle/radius), the Penetration, the
// BackingType, and the as-prepared root opening + root face. EffectiveThroatMm is the AISC J2.1 throat: CJP develops the
// full connected-part thickness; PJP the depth-of-prep less the WeldProcess deduction at a sharp (RequiresPjpDeduction)
// bevel groove, clamped non-negative.
public readonly record struct GroovePrep(GrooveGeometry Geometry, Penetration Penetration, BackingType Backing, double RootOpeningMm, double RootFaceMm) {
    public const double StandardRootOpeningMm = 2.0;   // AWS D1.1 prequalified open-root defaults — declared ONCE; the Prep projection and the profile derivation share them
    public const double StandardRootFaceMm = 1.5;
    public double IncludedAngleDeg => Geometry.IncludedAngleDeg;
    public double BevelAngleDeg => Geometry.BevelAngleDeg;
    public double GrooveRadiusMm => Geometry.RootRadiusMm;
    public double EffectiveThroatMm(double depthMm, double partThicknessMm, WeldProcess process) =>
        Penetration.Complete
            ? partThicknessMm
            : Math.Max(0.0, depthMm - (Geometry.RequiresPjpDeduction ? process.PjpDeductionMm : 0.0));
}

// The plug/slot hole geometry (AWS D1.1). A plug weld is a circular hole (SlotLengthMm = 0); a slot an elongated hole.
// ShearAreaMm2 is the faying-plane footprint — π/4·d² (plug) or d·slotLength (slot) — the area-shear branch DesignShearKn
// takes when WeldType.LineWeld is false. Filled flags a fully-filled hole.
public readonly record struct PlugSlot(PositiveMagnitude HoleDiameterMm, PositiveMagnitude DepthMm, double SlotLengthMm, bool Filled) {
    public double ShearAreaMm2 => SlotLengthMm > 0.0
        ? HoleDiameterMm.Value * SlotLengthMm
        : Math.PI * 0.25 * HoleDiameterMm.Value * HoleDiameterMm.Value;
}

// The weld seed row + the weld family algebra as DEFINED derived columns: SizeMm the type's primary dim (fillet leg /
// groove depth-of-prep / flare bend radius / plug-slot hole diameter), PartMm the governing thinner connected part (the
// J2.4 min-leg + CJP throat read), LengthMm the run.
// The generative Profile/Prep/Hole projections fill from the SmartEnum defaults (a convex fillet face, a 2 mm root
// opening / 1.5 mm root face, steel backing for CJP, a circular filled plug) — evaluated after the seed's dimensional
// gate, so the magnitude lifts inside are post-validated.
public readonly record struct WeldRow(string Designation, WeldType Type, GrooveGeometry Groove, Penetration Pen, ElectrodeClass Electrode, WeldProcess Process, double SizeMm, double PartMm, double LengthMm) {
    public WeldProfile Profile => new(Type == WeldType.Fillet ? WeldFace.Convex : WeldFace.Flat, Type == WeldType.Fillet ? 1.0 : 0.0, 0.0, RootTreatment.AsWelded);
    public Option<GroovePrep> Prep => Type == WeldType.Groove
        ? Some(new GroovePrep(Groove, Pen, Pen.Complete ? BackingType.Steel : BackingType.None, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm))
        : Option<GroovePrep>.None;
    public Option<PlugSlot> Hole => Type.LineWeld
        ? Option<PlugSlot>.None
        : Some(new PlugSlot(PositiveMagnitude.Create(SizeMm), PositiveMagnitude.Create(PartMm), Type == WeldType.Slot ? LengthMm : 0.0, Filled: true));

    // The AISC J2 effective throat: 0.707·leg fillet, the GroovePrep CJP/PJP throat (part thickness for CJP, depth less
    // the process deduction for a sharp PJP groove), 0 for a plug/slot line throat (they resist on the hole area),
    // FlareThroatFactor·radius for a flare groove. Prep is Some iff Type == Groove, so the None fold is unreachable —
    // it degenerates to 0, conservative, never a forged full-thickness throat.
    public double EffectiveThroatMm => Type.Switch(
        fillet:     _ => 0.707 * SizeMm,
        groove:     _ => Prep.Match(p => p.EffectiveThroatMm(SizeMm, PartMm, Process), () => 0.0),
        plug:       _ => 0.0,
        slot:       _ => 0.0,
        flareBevel: _ => Type.FlareThroatFactor * SizeMm,
        flareV:     _ => Type.FlareThroatFactor * SizeMm);

    // The faying-plane hole area exists ONLY for a plug/slot (Hole is Some iff !LineWeld); a line weld reads 0, never a
    // forged π/4·Size² footprint a stray caller would silently trust.
    public double ShearAreaMm2 => Hole.Match(static h => h.ShearAreaMm2, static () => 0.0);

    // SPEC-NOMINAL design shear over the row's own filler band — AWS D1.1 line 0.6·FEXX·throat·length for a line weld,
    // area-shear 0.6·FEXX·ShearAreaMm2 over the faying-plane hole for a plug/slot (LineWeld the branch discriminant, not
    // a capability gate). The matching base-metal capacity is the Mechanical receipt by SubstanceId, never a column here.
    public double DesignShearKn => Type.LineWeld
        ? 0.6 * Electrode.TensileMpa * EffectiveThroatMm * LengthMm * 1e-3
        : 0.6 * Electrode.TensileMpa * ShearAreaMm2 * 1e-3;

    // AISC 360 Eq J2-5 directional increase kds = 1 + 0.50·sin^1.5θ — θ from the weld longitudinal axis (0° kds = 1.0,
    // 90° transverse kds = 1.5). Eligibility is the WeldType.Directional policy column, never an identity chain; |sin|
    // makes the projection total over any axis convention (sin^1.5 of a negative/reflex θ is NaN, the mirror is physical).
    public double DirectionalShearKn(double loadAngleDeg) =>
        Type.Directional
            ? DesignShearKn * (1.0 + 0.50 * Math.Pow(Math.Abs(Math.Sin(loadAngleDeg * Math.PI / 180.0)), 1.5))
            : DesignShearKn;

    public MaterialId Substance => Electrode.Substance;
    public MaterialId Appearance => Electrode.Appearance;

    // AISC 360 Table J2.4 minimum fillet leg from the governing thinner connected part — the PUBLISHED metric bounds
    // 6/13/19 mm (1/4 / 1/2 / 3/4 in) -> 3/5/6/8 mm legs, transcribed verbatim (a rounded 20 mm bound under-sizes a
    // 19-20 mm part's leg non-conservatively). The II overload reads the VividOrange.Profiles I-section
    // FlangeThickness/WebThickness (the thinner governs) — the one VividOrange geometry composition.
    public static double MinimumFilletLegMm(double thinnerPartMm) => thinnerPartMm switch {
        <= 6.0  => 3.0,
        <= 13.0 => 5.0,
        <= 19.0 => 6.0,
        _       => 8.0
    };

    public static double MinimumFilletLegMm(II connectedPart) =>
        MinimumFilletLegMm(Math.Min(connectedPart.FlangeThickness.Millimeters, connectedPart.WebThickness.Millimeters));

    // AISC 360 J2.2b maximum fillet leg along an edge: the full edge thickness below 6 mm (1/4 in), else the edge
    // thickness less 1.6 mm (1/16 in) — the upper bound the design check pairs with MinimumFilletLegMm.
    public static double MaximumFilletLegMm(double edgeThicknessMm) =>
        edgeThicknessMm < 6.0 ? edgeThicknessMm : edgeThicknessMm - 1.6;
}

// The stud seed row + its algebra: L1 the catalogue length before burn-off, SpacingMm the station-stepped pitch a layout
// pattern reads. RealizedLengthMm is the ISO 13918 as-welded L2 = L1 − burn-off (DEFINED); DesignShearKn the AISC steel-
// side StudClass cap — the concrete-governed 0.5·Asc·√(f'c·Ec) branch is the Rasm.Compute min(steel, concrete) join over
// the seam slab f'c/Ec, never a column here.
public readonly record struct StudRow(string Designation, StudClass Class, StudGrade Grade, double LengthBeforeWeldMm, double SpacingMm) {
    public double RealizedLengthMm => LengthBeforeWeldMm - Class.BurnoffMm;
    public double DesignShearKn => Class.SteelShearKn;
    public MaterialId Substance => Grade.Substance;
}

// The adhesive seed row + its algebra: BondMm the glueline, OverlapMm the bonded lap / SSG structural bite, WidthMm the
// joint width. DesignShearKn is the ASTM D1002 LapShearMpa·overlap·width over the lap shear-transfer area (DEFINED);
// DesignTensionKn the ASTM C1401 SSG bite path — StructuralBiteMpa over the bite area, the wind-suction tension the
// curtain-wall bite check reads (shear and tension are DISTINCT allowable bands, never one blended capacity).
public readonly record struct AdhesiveRow(string Designation, AdhesiveClass Class, double BondMm, double OverlapMm, double WidthMm) {
    public double DesignShearKn => Class.LapShearMpa * OverlapMm * WidthMm * 1e-3;
    public double DesignTensionKn => Class.StructuralBiteMpa * OverlapMm * WidthMm * 1e-3;
    public MaterialId Substance => Class.Substance;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time realization-bag owner — ONE polymorphic Of discriminating on the row kind (input-shape dispatch, never
// name suffixes). Rows are byte-identical to the retired projector Detail switch: the JointType modality over the
// schema's closed allowed-set, the FastenerType token, and the dimension-only SI scalars the Bim connection reader
// round-trips (the DERIVED EffectiveThroat, never the nominal SizeMm).
public static class JointDetail {
    public static PropertyBag Of(WeldRow r) => RealizationRows(
        Joint("Welded"),
        Token(DetailSchema.FastenerType, JointKind.Weld.IfcPredefinedType),
        Measured(DetailSchema.EffectiveThroat, Dimension.LengthDim, r.EffectiveThroatMm * 1e-3),
        Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.LengthMm * 1e-3));

    public static PropertyBag Of(AdhesiveRow r) => RealizationRows(
        Joint("Bonded"),
        Token(DetailSchema.FastenerType, JointKind.Adhesive.IfcPredefinedType),
        Measured(DetailSchema.BondLine, Dimension.LengthDim, r.BondMm * 1e-3),
        Measured(DetailSchema.Overlap, Dimension.LengthDim, r.OverlapMm * 1e-3));

    public static PropertyBag Of(StudRow r) => RealizationRows(
        Joint("Welded"),
        Token(DetailSchema.FastenerType, JointKind.Stud.IfcPredefinedType),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, r.Class.DiameterMm * 1e-3),
        Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.LengthBeforeWeldMm * 1e-3));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The joint family seed — AUTHORED tables (SEED_ROW_LAW: no vendor producer; AWS D1.1 / ISO 13918 / ASTM values
// PUBLISHED). ComponentFamily.Joint binds Rows; every row is Sectioned: false (a weld/bond/stud fills no
// ComputedSection). The dual IFC entity binds at seed from the kind's own token vocabulary, never a hand string per row.
public static class JointSeed {
    // Weld/stud specifications carry no masonry-style regional joint thickness — StandardJointThicknessMm 0.0. The
    // authority rides the row's OWN standards body: AWS (D1.1 welds, D1.1 Type B studs over the ISO 13918 geometry) for
    // the weld/stud tables, ASTM (D1002/D1876/C1401) for the adhesive table — never one blended authority. The Eurocode
    // design codes the design seam NAMES are typed VividOrange.Standards citations: EN 1993-1-8 (joints), EN 1994 (the
    // composite stud PRd path) — replacing inline code strings; AWS/AISC/ISO/ASTM have no VividOrange body and stay
    // PUBLISHED strings on the rows.
    static readonly ComponentStandard WeldStandard = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Aws);
    static readonly ComponentStandard AdhesiveStandard = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);
    public static readonly En1993 EnJoints = new(En1993Part.Part1_8, NationalAnnex.RecommendedValues);
    public static readonly En1994 EnComposite = new();

    // The IFC-BINDING seed law: the dual entity computed from the kind's own token vocabulary at seed time.
    static IfcBinding Binding(JointKind kind) =>
        IfcBinding.Of(kind == JointKind.Stud ? "IfcMechanicalFastener" : "IfcFastener", kind.IfcPredefinedType);

    // AWS D1.1 weld seed (SizeMm = fillet leg / groove depth-of-prep / flare radius / plug-slot hole diameter; PartMm =
    // the governing thinner connected part; LengthMm = run length, doubling as the slot elongation on a Slot row).
    // Groove rows carry the GrooveGeometry + Penetration the Prep projection reads; fillet/plug/slot/flare rows carry
    // Square + Pjp as the inert default the projection ignores. The plug/slot pair exercises BOTH area-shear branches.
    static readonly Seq<WeldRow> WeldRows = Seq(
        new WeldRow("joint.weld-fillet-6mm-e70",       WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70, WeldProcess.Smaw, 6.0,  10.0, 100.0),
        new WeldRow("joint.weld-fillet-8mm-e70",       WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70, WeldProcess.Gmaw, 8.0,  12.0, 150.0),
        new WeldRow("joint.weld-fillet-10mm-e80",      WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E80, WeldProcess.Fcaw, 10.0, 16.0, 200.0),
        new WeldRow("joint.weld-groove-v-cjp-e80",     WeldType.Groove,     GrooveGeometry.SingleV,     Penetration.Cjp, ElectrodeClass.E80, WeldProcess.Saw,  12.0, 12.0, 250.0),
        new WeldRow("joint.weld-groove-bevel-pjp-e90", WeldType.Groove,     GrooveGeometry.SingleBevel, Penetration.Pjp, ElectrodeClass.E90, WeldProcess.Smaw, 16.0, 20.0, 300.0),
        new WeldRow("joint.weld-plug-20mm-e70",        WeldType.Plug,       GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70, WeldProcess.Smaw, 20.0, 8.0,  20.0),
        new WeldRow("joint.weld-slot-20x60-e70",       WeldType.Slot,       GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70, WeldProcess.Smaw, 20.0, 8.0,  60.0),
        new WeldRow("joint.weld-flarebevel-r10-e70",   WeldType.FlareBevel, GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70, WeldProcess.Gmaw, 10.0, 6.0,  120.0));

    // ISO 13918 Type SD shear-stud seed (LengthBeforeWeldMm = L1; SpacingMm = the station-stepped pitch). The per-diameter
    // collar/head geometry rides StudClass; the grade rides StudGrade.
    static readonly Seq<StudRow> StudRows = Seq(
        new StudRow("joint.stud-13mm-h75",  StudClass.S13, StudGrade.Sd1,  75.0,  150.0),
        new StudRow("joint.stud-19mm-h100", StudClass.S19, StudGrade.Sd1,  100.0, 200.0),
        new StudRow("joint.stud-22mm-h125", StudClass.S22, StudGrade.AwsB, 125.0, 250.0),
        new StudRow("joint.stud-25mm-h150", StudClass.S25, StudGrade.Sd3,  150.0, 300.0));

    // Structural-adhesive seed (BondMm = glueline; OverlapMm = bonded lap / SSG bite; WidthMm = joint width). The SSG
    // silicone row is the curtain-wall structural-glazing bead.
    static readonly Seq<AdhesiveRow> AdhesiveRows = Seq(
        new AdhesiveRow("joint.adhesive-epoxy-2mm", AdhesiveClass.Epoxy,              2.0,  25.0, 50.0),
        new AdhesiveRow("joint.adhesive-mma-1mm",   AdhesiveClass.Methacrylate,       1.0,  20.0, 40.0),
        new AdhesiveRow("joint.adhesive-ssg-12mm",  AdhesiveClass.SiliconeStructural, 12.0, 12.0, 1000.0));

    // The weld geometry -> SectionProfile arm per ComponentFamily.Joint.admits: FilletTriangle the fillet AND flare
    // welds (the equal-leg gross triangle; the 0.707·leg and FlareThroatFactor·radius throats stay the row's DEFINED
    // derivations, never profile state), Trapezium the groove, Circle the plug/slot hole footprint.
    static Fin<SectionProfile> WeldProfileOf(WeldRow r, Op key) => r.Type.Switch(
        fillet:     _ => SectionProfile.FilletTriangle.Of(legMm: r.SizeMm, leg2Mm: r.SizeMm, key),
        groove:     _ => GrooveProfile(r, key),
        plug:       _ => SectionProfile.Circle.Of(diameterMm: r.SizeMm, key),
        slot:       _ => SectionProfile.Circle.Of(diameterMm: r.SizeMm, key),
        flareBevel: _ => SectionProfile.FilletTriangle.Of(legMm: r.SizeMm, leg2Mm: r.SizeMm, key),
        flareV:     _ => SectionProfile.FilletTriangle.Of(legMm: r.SizeMm, leg2Mm: r.SizeMm, key));

    // The groove cross-section as the DEFINED Trapezium derivation over the prep geometry: bottom = the root opening,
    // the walls flaring over the depth-of-prep at the included angle (2·tan(α/2), V/U) or the single-wall bevel angle
    // (tan(β), bevel/J — TopOffset shifting the asymmetric single-wall prep); a square groove degenerates to the
    // equal-width slit. The throat stays GroovePrep.EffectiveThroatMm — the profile is the gross prep envelope only.
    static Fin<SectionProfile> GrooveProfile(WeldRow r, Op key) {
        GroovePrep p = r.Prep.IfNone(new GroovePrep(r.Groove, r.Pen, BackingType.None, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm));
        double flare = p.IncludedAngleDeg > 0.0 ? 2.0 * Math.Tan(p.IncludedAngleDeg * Math.PI / 360.0) : Math.Tan(p.BevelAngleDeg * Math.PI / 180.0);
        double top = p.RootOpeningMm + r.SizeMm * flare;
        return SectionProfile.Trapezium.Of(
            bottomWidthMm: p.RootOpeningMm, topWidthMm: top, depthMm: r.SizeMm,
            topOffsetMm: p.BevelAngleDeg > 0.0 ? (top - p.RootOpeningMm) / 2.0 : 0.0, key);
    }

    // One weld row -> one ComponentRow: the part/run dimensional gates (the lifted values ARE the gate — a non-positive
    // column aborts before any construction), the AISC J2.4 minimum-leg gate over the row's OWN algebra (a seeded fillet
    // undersized for its thinner part is a transcription fault; the J2.2b edge maximum stays a design-check read — it is
    // T-vs-lap configuration-dependent), the profile rail, the seed-built bag, the seed-computed dual binding.
    static Fin<ComponentRow> Row(WeldRow r, Op key) =>
        from part in key.AcceptValidated<PositiveMagnitude>(candidate: r.PartMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
        from legged in guard(r.Type != WeldType.Fillet || r.SizeMm >= WeldRow.MinimumFilletLegMm(r.PartMm),
            ComponentFault.Dimension(key, $"<fillet-leg-below-j2.4-minimum:{r.Designation}>"))
        from profile in WeldProfileOf(r, key)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, Binding(JointKind.Weld),
            Coring.None, WeldStandard, substanceId: r.Substance, appearanceId: r.Appearance, detail: Some(JointDetail.Of(r)), key)
        select new ComponentRow(item, Sectioned: false);

    // The stud gate also proves the ISO 13918 L2 = L1 − burn-off positive — a stud shorter than its own burn-off aborts.
    // Both MaterialId slots ride the GRADE row (the SD3 stainless renders chromium, never blanket steel).
    static Fin<ComponentRow> Row(StudRow r, Op key) =>
        from spacing in key.AcceptValidated<PositiveMagnitude>(candidate: r.SpacingMm)
        from realized in key.AcceptValidated<PositiveMagnitude>(candidate: r.RealizedLengthMm)
        from profile in SectionProfile.Circle.Of(diameterMm: r.Class.DiameterMm, key)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, Binding(JointKind.Stud),
            Coring.None, WeldStandard, substanceId: r.Substance, appearanceId: r.Grade.Appearance, detail: Some(JointDetail.Of(r)), key)
        select new ComponentRow(item, Sectioned: false);

    static Fin<ComponentRow> Row(AdhesiveRow r, Op key) =>
        from overlap in key.AcceptValidated<PositiveMagnitude>(candidate: r.OverlapMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
        from profile in SectionProfile.Nominal.Of(nominalMm: r.BondMm, key)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, Binding(JointKind.Adhesive),
            Coring.None, AdhesiveStandard, substanceId: r.Substance, appearanceId: MaterialId.Of("polymer.adhesive"), detail: Some(JointDetail.Of(r)), key)
        select new ComponentRow(item, Sectioned: false);

    // The family fold ComponentFamily.Joint binds: three independent tables Traverse applicatively and join through the
    // tuple Apply — Fin's applicative UNIONS the faults, so every malformed row across all three tables reports in one
    // build abort (the prior Choose + .Concat swallowing fold, and the unfused .Map(f).Traverse(identity) re-spelling,
    // are the deleted forms).
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        (WeldRows.Traverse(r => Row(r, context.Key)).As(),
         StudRows.Traverse(r => Row(r, context.Key)).As(),
         AdhesiveRows.Traverse(r => Row(r, context.Key)).As())
            .Apply(static (welds, studs, bonds) => welds + studs + bonds).As();
}
```

## [03]-[RESEARCH]

- [SEED_PARADIGM]: REALIZED — the bespoke `JointSection` `[Union]` and its `ComponentSection.Joint` arm are DELETED; geometry lands as the family-admitted `SectionProfile` arms (`FilletTriangle` fillet/flare, `Trapezium` groove — the DEFINED prep-envelope derivation, `Circle` plug/slot hole + stud shank, `Nominal` continuous bond-line), the realization scalars land in the `JointDetail` `DetailSchema.Realization` bag (rows byte-identical to the retired projector `Detail` switch), and the throat/design-shear algebra re-homes as DEFINED derived columns on the typed seed rows (`WeldRow.EffectiveThroatMm`/`DesignShearKn`/`DirectionalShearKn`, `StudRow.RealizedLengthMm`/`DesignShearKn`, `AdhesiveRow.DesignShearKn`). `ElectrodeClass`(6)/`StudGrade`(5)/`AdhesiveClass`(4) convert 1:1 from `[SmartEnum]` to frozen `readonly record struct` row tables (values verbatim, `PUBLISHED` provenance — no runtime key lookup, no delegate column, no IFC-token derivation); `StudClass`(5) converts carrying the `DEFINED` `SteelShearKn = Rg·Rp·Asc·Fu` column with the symbol path (`StudClass.S19.SteelShearKn`) and every value byte-identical for the `steel#STEEL_FAMILY` `Composite` `ΣQn` read. `JointSeed.Rows : Context -> Fin<Seq<ComponentRow>>` folds the three tables through per-table `Traverse` joined by the tuple `Apply` — `Fin`'s applicative unions the faults so every malformed row reports in one build abort (the fault-swallowing `Choose` + `.Concat` and the unfused `.Map(f).Traverse(identity)` re-spelling both retired); every row `Sectioned: false`.
- [WELD_GEOMETRY_AND_DIRECTIONAL]: REALIZED — the AWS D1.1 geometry collapses to the 6-member `WeldType` with the groove SUBTYPE on the 9-member `GrooveGeometry` (a 14-member `WeldType` is the deleted form). The fillet `EffectiveThroatMm` is `0.707·leg` (theoretical face, never the convex reinforcement); flare-bevel `0.3125·R` / flare-V `0.5·R` per AWS D1.1 Table 5.2. `DesignShearKn` is the AWS D1.1 line `0.6·FEXX·throat·length`; `DirectionalShearKn(loadAngleDeg)` applies AISC 360 Eq J2-5 `1 + 0.50·sin^1.5θ` through the `WeldType.Directional` policy column (fillet/flare true; groove/plug/slot false), never an identity chain — `|sin|` keeps the projection total over any axis convention. `WeldRow.MinimumFilletLegMm` is AISC Table J2.4 (the PUBLISHED metric bounds 6/13/19 mm -> 3/5/6/8 mm) with the `II` overload reading `VividOrange.Profiles` `FlangeThickness`/`WebThickness` (`.Millimeters` at the read edge — the one VividOrange geometry composition), and the SEED FOLD gates every fillet row against it (an undersized seeded leg is a transcription fault); `MaximumFilletLegMm` the J2.2b edge bound — a design-check read, never a seed gate (T-vs-lap configuration-dependent). The typed `En1993(En1993Part.Part1_8)` citation NAMES the Eurocode joint design code (`VividOrange.Standards`); AWS/AISC citations stay `PUBLISHED` strings (the EN-only `[BODY_SCOPE_GATE]`). Ripple counterpart: `steel#STEEL_FAMILY` (the connected `II` section the J2.4 read consumes).
- [GROOVE_PREP_GEOMETRY]: REALIZED — `GroovePrep` captures the AWS A2.4 / D1.1 preparation (`GrooveGeometry` angle/radius — U = 6 mm / J = 10 mm standard prep radii, `Penetration`, `BackingType`, root opening/face). `EffectiveThroatMm` is the AISC J2.1 throat: CJP the full connected-part thickness, PJP the depth less the `WeldProcess` deduction at a sharp (`RequiresPjpDeduction`) groove — the deduction applies to SMAW/GMAW/FCAW ONLY (SAW takes full depth). `RootTreatment` stays split from `BackingType` (treatment vs material). The `Trapezium` profile is the DEFINED gross prep envelope (root opening flaring at the included/bevel angle over the depth); the throat NEVER reads the profile.
- [PLUG_SLOT_WELD]: REALIZED — `PlugSlot` captures the hole geometry (`HoleDiameterMm`, `DepthMm`, `SlotLengthMm` elongation, `Filled`); `WeldType.LineWeld == false` routes `DesignShearKn` to the realized area-shear branch `0.6·FEXX·ShearAreaMm2` — the seeded `joint.weld-plug-20mm-e70` and `joint.weld-slot-20x60-e70` rows exercise BOTH branches (`π/4·d²` plug, `d·slotLength` slot; `LengthMm` doubles as the slot elongation on a `Slot` row).
- [STRUCTURAL_ADHESIVE_AND_SSG]: REALIZED — the `AdhesiveClass` frozen rows carry ASTM D1002 lap-shear, D1876 T-peel, service temperature, and the ASTM C1401 SSG tensile bite (silicone `0.14 MPa` / 20 psi distinct from its cured lap-shear); the bonded-lap capacity is `AdhesiveRow.DesignShearKn = LapShearMpa·overlap·width` and the SSG wind-suction path is `DesignTensionKn = StructuralBiteMpa·overlap·width` (two DISTINCT allowable bands the curtain-wall bite check reads, never one blended capacity); the `joint.adhesive-ssg-12mm` row is the curtain-wall structural bead.
- [ISO_13918_SHEAR_STUD]: REALIZED — the `StudClass` frozen rows carry the ISO 13918:2017 Type SD per-diameter geometry (13/16/19/22/25 mm; head `dc` 25/32/32/35/40, `h3` 8/8/10/10/12, collar `d1` 17/21/23/29/31, collar height 3/4.5/6/6/7, burn-off 3/4/4.5/5/5.5, the constant `140°` tip) and the AWS D1.1 Type B / AISC §I8 `Fu = 450 MPa` cap; `StudRow` exposes both L1 and the DEFINED L2 = L1 − burn-off (the seed gate proves L2 positive). `StudGrade` carries the specified `fy`/`fu` for the EN 1994 §6.6.3.1 PRd path — the typed `En1994` citation NAMES the composite code — plus the GRADE-borne appearance slot (`metal.chrome` the SD3 austenitic stainless, `metal.steel` the carbon grades; the blanket steel appearance the stud row previously hardcoded is the deleted form). `DesignShearKn` emits the STEEL-side `SteelShearKn = Rg·Rp·Asc·Fu` (strong-position `Rg = 1.0`/`Rp = 0.75`); the concrete-side `0.5·Asc·√(f'c·Ec)` join is `Rasm.Compute/structural#DESIGN_CHECK`'s `min(steel, concrete)`. Ripple counterpart: `steel#STEEL_FAMILY` (`CompositeDetail.Stud` reads `StudClass.SteelShearKn`; the `Rasm.Materials.Component.Joint` sub-namespace import re-points to the flattened parent namespace).
- [IFC_JOINT_WIRE]: REALIZED — the dual entity binds AT SEED per the IFC-BINDING law: `IfcBinding.Of(kind == JointKind.Stud ? "IfcMechanicalFastener" : "IfcFastener", kind.IfcPredefinedType)` — `STUDSHEARCONNECTOR` the welded stud (`STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, never a fastener value), `WELD`/`GLUE` the weld/adhesive bead on the non-mechanical `IfcFastener` (the assay-verified `{NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}` set). The `JointDetail` bag rows (`EffectiveThroat`/`NominalLength` weld, `BondLine`/`Overlap` adhesive, `NominalDiameter`/`NominalLength` stud, each dimension-only SI) are byte-identical to the retired switch, so the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader and the `Projection/semantic#IFC_EGRESS` `Emit` + `IfcRelConnectsWithRealizingElements` edge round-trip unchanged. Ripple counterpart: `Projection/component.md` (the `Detail` switch deletion — `ProjectType` reads `c.Detail`).
