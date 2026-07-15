# [MATERIALS_JOINT]

THE JOINT SEED PAGE — the `joint` `ComponentFamily` row (`ComponentClass.Minor`, `DetailLane.Realization`), the continuous-connection weld/adhesive/stud vocabulary. A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed part: it carries no thread or bar cross-section, so it cannot fold into `fastener` the way `anchor` does — the ONE deliberate widening past the discrete triple, load-bearing because `steel#STEEL_FAMILY`'s `Composite` arm reads `StudClass.SteelShearKn` from here for its `ΣQn` cap. An 8 mm fillet weld is a `Component` row in the `joint` family, never a `Weld` type: geometry lands as the `SectionProfile` arm the family admits (`FilletTriangle` the fillet and flare welds — the `0.707·leg` throat staying the family's DEFINED derivation; `Trapezium` the PJP/CJP groove derived from the prep geometry; `Circle` the plug/slot hole and the stud shank; `Nominal` the continuous adhesive bond-line), the realization scalars land in the `JointDetail` `DetailSchema.Realization` `PropertyBag`, and the strength axes are frozen row tables with per-column provenance. `JointSeed` binds the DUAL IFC entity at seed per the IFC-BINDING law — `JointKind.Binding` the row-owned `IfcBinding.Of(kind.IfcEntity, kind.IfcPredefinedType)` projection, the entity a vocabulary COLUMN beside its token (POLICY_VALUES — never an external equality branch) — an `IfcMechanicalFastener` `STUDSHEARCONNECTOR` for the welded stud, an `IfcFastener` `WELD`/`GLUE` for the weld/adhesive bead. The vocabulary grows by data: a new electrode/adhesive/stud diameter/stud grade is one row in its frozen table, a new groove one `GrooveGeometry` row, a new designation one `JointRow.Weld`/`JointRow.Stud`/`JointRow.Adhesive` table entry — never a per-joint type. The generative geometry is the hand-rolled AWS D1.1:2020 + AISC 360 J2/I8 + ISO 13918:2017 + EN 1994 capture (GeometryGym mirrors only the IFC class; VividOrange owns no weld); the host materializes the bead/groove/stud solid from the scalar receipt, NEVER a host `Curve` here. `DesignShearKn` emits the SPEC-NOMINAL filler/adhesive/stud-steel band; the measured base-metal capacity is the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt read by `MaterialId`, and the composite stud's concrete branch is the `Rasm.Compute/structural#DESIGN_CHECK` join — neither a column here. The AISC J2.4 minimum fillet leg reads the connected member's `VividOrange.Profiles` `II` `FlangeThickness`/`WebThickness`; the EN 1993-1-8 / EN 1994 design codes are NAMED as typed `VividOrange.Standards` citations. The page composes `Component/component#COMPONENT_OWNER` (`Component.Of`, `ComponentRow`, `SectionProfile`, `IfcBinding`, `ComponentDetail`, `ComponentFault`), the seam `Properties/property#DETAIL_SCHEMA` realization rows, `Rasm.Numerics` `PositiveMagnitude`, and the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` two-slot law.

## [01]-[INDEX]

- [02]-[JOINT_FAMILY]: the `JointKind` IFC binding vocabulary; the welding policy axes and frozen strength tables; the `WeldGeometry` union over fillet, groove, plug, slot, flare-bevel, and flare-V payloads; the `GroovePrep`, `HoleWeld`, and `WeldProfile` geometry values; the `JointRow` weld/stud/adhesive family; `JointDetail`; and the `JointSeed.Rows` fold.

## [02]-[JOINT_FAMILY]

- Owner: `JointKind` owns the complete IFC binding; the welding SmartEnums own reusable policy; the frozen strength tables own published data; `WeldGeometry` owns the payload-timed geometry variants without inert defaults; `JointRow` owns the weld/stud/adhesive seed family; `JointDetail` owns the realization bag; and `JointSeed` owns the closed table and fold.
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `GrooveGeometry` × `ElectrodeClass` × `WeldProcess`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass` × `StudGrade`)}; weld {fillet · groove (square/V/bevel/U/J × single/double on `GrooveGeometry`) · plug · slot · flare-bevel · flare-v} over the E60..E110 electrode band; adhesive {epoxy · methacrylate · polyurethane · silicone-structural} over the lap-shear/peel/SSG-bite band; stud {the 13..25 mm ISO 13918 Type SD headed connectors} over diameter × grade × height × spacing. A joint is a `Component` row in `ComponentFamily.Joint`, never a joint subtype; the groove subtype is a `GrooveGeometry` row, never a per-subtype `WeldType` (a 14-member `WeldType` duplicating the 9 `GrooveGeometry` cases is the deleted form).
- Entry: `JointSeed.Rows(Context)` traverses the closed `JointRow` table through one total row dispatch. Weld geometry is already admitted through `PositiveMagnitude`; fillet rows additionally prove the AISC J2.4 minimum, while stud and adhesive dimensions rail through the same component fault family. `JointRow.Weld.DirectionalShearKn(Angle)` applies the directional factor without a raw-angle convention.
- Packages: Rasm.Numerics (`PositiveMagnitude` — throat/leg/size/length/bond-line/overlap/width/spacing, never an int-backed count that truncates a fractional throat), Rasm.Domain (`Context`/`Op`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `Dimension`, `PropertyBag`), Rasm.Materials.Component (`Component`/`ComponentRow`/`SectionProfile`/`IfcBinding`/`ComponentDetail`/`ComponentFault`/`Coring`/`ComponentStandard`/`ComponentAuthority`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + generated total `Switch` for the policy axes), VividOrange.Profiles (`II` — the AISC J2.4 `FlangeThickness`/`WebThickness` connected-part read, the ONE VividOrange geometry composition), VividOrange.Standards (`En1993`/`En1993Part.Part1_8`, `En1994`, `NationalAnnex` — the typed Eurocode citations replacing inline code strings; AWS/AISC/ISO/ASTM have no VividOrange body, so those stay `PUBLISHED` provenance on the rows), UnitsNet (`Length.Millimeters` at the `II` read edge), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`Option`), BCL inbox (`ImmutableArray`).
- Growth: a new weld geometry is one `WeldType` row; a new groove one `GrooveGeometry` row carrying its angle/radius; a new electrode one `ElectrodeClass` row (FEXX + specification); a new adhesive one `AdhesiveClass` row; a new stud diameter one `StudClass` row (ISO 13918 geometry, the `SteelShearKn` column deriving); a new stud grade one `StudGrade` row (`fy`/`fu`); a new designation one `JointRow` table entry; a new continuous-connection modality ONE `JointRow` case whose missing `Switch` arms break `JointDetail.Of` and `JointSeed.Row` at compile time — zero central edits, never a `Weld`/`AdhesiveBond`/`ShearStud` sibling type. A structural-joint utilisation case is future `capacity#SECTION_CAPACITY` growth, not this page.
- Boundary: strength axes remain frozen rows because identity and lookup behavior are absent; policy axes remain SmartEnums because they carry dispatch data. `WeldGeometry` distinguishes payload arity and timing: only groove geometry carries preparation and process, only hole welds carry diameter/depth/fill, and only line welds carry a run. `JointRow.Weld` derives throat, length, profile, and strength through total dispatch. `DirectionalShearKn(Angle)` consumes a typed angle. Plug and slot require distinct section geometry; the current shared `SectionProfile.Circle` approximation is a first-order seam opening on `component#COMPONENT_OWNER`, recorded for its concurrent owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;  // ImmutableArray (the frozen standards row tables)
using LanguageExt;
using Rasm.Numerics;                  // PositiveMagnitude (>0 finite — throat/leg/size/length/bond-line/overlap/width/spacing)
using Rasm.Domain;                   // Context, Op, AcceptValidated
using Rasm.Element.Composition;      // MaterialId
using Rasm.Element.Properties;       // DetailSchema, PropertyBag
using Thinktecture;
using UnitsNet;                      // Length (.Millimeters on the II.FlangeThickness/WebThickness reads)
using VividOrange.Profiles;          // II (the I-section thickness pair the AISC J2.4 MinimumFilletLegMm read consumes)
using VividOrange.Standards.Eurocode; // En1993/En1993Part, En1994, NationalAnnex (the typed design-code citations)
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;
using static Rasm.Materials.Component.ComponentDetail;   // Joint / Token / Measured / RealizationRows (the relocated bag constructors)

// Every family page declares in the ONE flat Rasm.Materials.Component namespace (component#COMPONENT_OWNER;
// dotnet_style_namespace_match_folder). This page DEFINES StudClass; the steel#STEEL_FAMILY Composite arm reads
// StudClass.S19.SteelShearKn by bare name.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The continuous-connection kind discriminant owning the COMPLETE dual-entity binding (POLICY_VALUES — the entity
// axis is a row column beside its token, never an external equality branch): the welded stud is IfcMechanicalFastener
// STUDSHEARCONNECTOR, the weld bead IfcFastener WELD, the adhesive bead IfcFastener GLUE (STUD is the
// IfcReinforcingBarTypeEnum cast-in bar, never a fastener value). Binding derives the seed-time IfcBinding whole off
// the row, so a new kind that omits its entity is a missing constructor argument, not a silently non-stud entity.
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
// reinforcement, so the face is descriptive not structural. Declared before WeldType, whose rows carry it as the
// per-type bead-profile column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldFace {
    public static readonly WeldFace Flat    = new("flat");
    public static readonly WeldFace Convex  = new("convex");
    public static readonly WeldFace Concave = new("concave");
}

// The 6 AWS D1.1 weld GEOMETRIES. The groove SUBTYPE geometry (square/V/bevel/U/J × single/double) lives on
// GrooveGeometry, NOT a per-subtype WeldType (the no-split collapse). LineWeld flags the DesignShearKn branch: a
// fillet/groove/flare resists on throat × length line-shear, a plug/slot on the faying-plane HOLE AREA (Hole derives
// from !LineWeld — the non-line welds ARE the hole welds). Directional flags the AISC Eq J2-5 kds eligibility: the
// fillet/flare throat-shear welds take the directional increase, a groove develops base metal and a plug/slot resists
// on hole area. FlareThroatFactor is the AWS D1.1 Table 5.2 flare-groove radius factor (5/16·R flare-bevel, 1/2·R
// flare-V — the non-SAW common value). Face/ReinforcementMm are the type's as-deposited bead-profile columns the
// JointRow.Weld.Profile projection reads (POLICY_VALUES — an external `Type == Fillet` equality branch is the deleted
// form).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldType {
    public static readonly WeldType Fillet     = new("fillet",      lineWeld: true,  directional: true,  flareThroatFactor: 0.0,    face: WeldFace.Convex, reinforcementMm: 1.0);
    public static readonly WeldType Groove     = new("groove",      lineWeld: true,  directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0);
    public static readonly WeldType Plug       = new("plug",        lineWeld: false, directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0);
    public static readonly WeldType Slot       = new("slot",        lineWeld: false, directional: false, flareThroatFactor: 0.0,    face: WeldFace.Flat,   reinforcementMm: 0.0);
    public static readonly WeldType FlareBevel = new("flare-bevel", lineWeld: true,  directional: true,  flareThroatFactor: 0.3125, face: WeldFace.Flat,   reinforcementMm: 0.0);
    public static readonly WeldType FlareV     = new("flare-v",     lineWeld: true,  directional: true,  flareThroatFactor: 0.5,    face: WeldFace.Flat,   reinforcementMm: 0.0);
    public bool LineWeld { get; }
    public bool Directional { get; }
    public double FlareThroatFactor { get; }
    public WeldFace Face { get; }
    public double ReinforcementMm { get; }
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
public readonly record struct AdhesiveClass(string Key, double LapShearMpa, double PeelNmm, double ServiceCelsius, Option<double> StructuralBiteMpa, string SubstanceId) {
    public static readonly AdhesiveClass Epoxy              = new("epoxy",               30.0, 5.0,  80.0,  None,       "adhesive.epoxy");
    public static readonly AdhesiveClass Methacrylate       = new("methacrylate",        25.0, 12.0, 100.0, None,       "adhesive.methacrylate");
    public static readonly AdhesiveClass Polyurethane       = new("polyurethane",        15.0, 20.0, 90.0,  None,       "adhesive.polyurethane");
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", 1.0,  8.0,  150.0, Some(0.14), "sealant.silicone-structural");
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

// The common plug/slot hole geometry; the `WeldGeometry` case owns whether longitudinal length exists.
public readonly record struct HoleWeld(PositiveMagnitude DiameterMm, PositiveMagnitude DepthMm);

[Union]
public abstract partial record WeldGeometry {
    private WeldGeometry() { }
    public sealed record Fillet(PositiveMagnitude LegMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record Groove(GroovePrep Prep, WeldProcess Process, PositiveMagnitude DepthMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record Plug(HoleWeld Hole) : WeldGeometry;
    public sealed record Slot(HoleWeld Hole, PositiveMagnitude LengthMm) : WeldGeometry;
    public sealed record FlareBevel(PositiveMagnitude RadiusMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
    public sealed record FlareV(PositiveMagnitude RadiusMm, PositiveMagnitude PartMm, PositiveMagnitude RunMm) : WeldGeometry;
}

// ONE closed seed-row family (SHAPE_BUDGET: the weld/stud/adhesive rows share the admission path, the detail-bag
// consumer, and the Component.Of construction, so they are cases of one [Union], never three sibling structs); the
// throat/design-shear algebra rides each case as DEFINED derived columns, JointDetail/JointSeed dispatch the generated
// total Switch, and a fourth continuous-connection modality is ONE case that breaks every dispatch site at compile
// time. Kind is the base-level policy read the dual IFC binding derives from.
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

        public WeldProfile Profile => new(Type.Face, Type.ReinforcementMm, 0.0, RootTreatment.AsWelded);
        public double EffectiveThroatMm => Geometry.Switch(
            fillet: static geometry => 0.707 * geometry.LegMm.Value,
            groove: static geometry => geometry.Prep.EffectiveThroatMm(geometry.DepthMm.Value, geometry.PartMm.Value, geometry.Process),
            plug: static _ => 0.0,
            slot: static _ => 0.0,
            flareBevel: static geometry => WeldType.FlareBevel.FlareThroatFactor * geometry.RadiusMm.Value,
            flareV: static geometry => WeldType.FlareV.FlareThroatFactor * geometry.RadiusMm.Value);
        public double LengthMm => Geometry.Switch(
            fillet: static geometry => geometry.RunMm.Value,
            groove: static geometry => geometry.RunMm.Value,
            plug: static geometry => geometry.Hole.DiameterMm.Value,
            slot: static geometry => geometry.LengthMm.Value,
            flareBevel: static geometry => geometry.RunMm.Value,
            flareV: static geometry => geometry.RunMm.Value);
        // 0.6·FEXX over the shear-transfer area: a LINE arm derives throat × run from the ONE EffectiveThroatMm/LengthMm
        // law (DERIVED_LOGIC — the throat is stated once, never re-spelled per arm), a HOLE arm its faying-plane area.
        public double DesignShearKn => 0.6 * Electrode.TensileMpa * 1e-3 * Geometry.Switch(
            fillet: _ => EffectiveThroatMm * LengthMm,
            groove: _ => EffectiveThroatMm * LengthMm,
            plug: static geometry => Math.PI * 0.25 * geometry.Hole.DiameterMm.Value * geometry.Hole.DiameterMm.Value,
            slot: static geometry => geometry.Hole.DiameterMm.Value * geometry.LengthMm.Value,
            flareBevel: _ => EffectiveThroatMm * LengthMm,
            flareV: _ => EffectiveThroatMm * LengthMm);
        public double DirectionalShearKn(Angle loadAngle) =>
            Type.Directional
                ? DesignShearKn * (1.0 + 0.50 * Math.Pow(Math.Abs(Math.Sin(loadAngle.Radians)), 1.5))
                : DesignShearKn;

        public MaterialId Substance => Electrode.Substance;
        public MaterialId Appearance => Electrode.Appearance;

        // AISC 360 Table J2.4 minimum fillet leg from the governing thinner connected part — the PUBLISHED metric
        // bounds 6/13/19 mm (1/4 / 1/2 / 3/4 in) -> 3/5/6/8 mm legs, transcribed verbatim (a rounded 20 mm bound
        // under-sizes a 19-20 mm part's leg non-conservatively). The II overload reads the VividOrange.Profiles
        // I-section FlangeThickness/WebThickness (the thinner governs) — the one VividOrange geometry composition.
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

    // The stud case: L1 the catalogue length before burn-off, SpacingMm the station-stepped pitch a layout pattern
    // reads. RealizedLengthMm is the ISO 13918 as-welded L2 = L1 − burn-off (DEFINED); DesignShearKn the AISC steel-
    // side StudClass cap — the concrete-governed 0.5·Asc·√(f'c·Ec) branch is the Rasm.Compute min(steel, concrete)
    // join over the seam slab f'c/Ec, never a column here.
    public sealed record Stud(string Designation, StudClass Class, StudGrade Grade, PositiveMagnitude LengthBeforeWeldMm, PositiveMagnitude SpacingMm)
        : JointRow(Designation, JointKind.Stud) {
        public double RealizedLengthMm => LengthBeforeWeldMm.Value - Class.BurnoffMm;
        public double DesignShearKn => Class.SteelShearKn;
        public MaterialId Substance => Grade.Substance;
    }

    // The adhesive case: BondMm the glueline, OverlapMm the bonded lap / SSG structural bite, WidthMm the joint width.
    // DesignShearKn is the ASTM D1002 LapShearMpa·overlap·width over the lap shear-transfer area (DEFINED);
    // DesignTensionKn the ASTM C1401 SSG bite path — StructuralBiteMpa over the bite area, the wind-suction tension
    // the curtain-wall bite check reads (shear and tension are DISTINCT allowable bands, never one blended capacity).
    public sealed record Adhesive(string Designation, AdhesiveClass Class, PositiveMagnitude BondMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm)
        : JointRow(Designation, JointKind.Adhesive) {
        public double DesignShearKn => Class.LapShearMpa * OverlapMm.Value * WidthMm.Value * 1e-3;
        public Option<double> DesignTensionKn => Class.StructuralBiteMpa.Map(strength => strength * OverlapMm.Value * WidthMm.Value * 1e-3);
        public MaterialId Substance => Class.Substance;
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time realization-bag owner — ONE Of over the ONE JointRow family, the generated total Switch the modality
// dispatch (a new joint modality is a JointRow case + one arm here, never a new public overload). Rows are
// byte-identical to the retired projector Detail switch: the JointType modality over the schema's closed allowed-set,
// the FastenerType token off the case's own kind row, and the dimension-only SI scalars the Bim connection reader
// round-trips the derived effective throat and realized length.
public static class JointDetail {
    public static Fin<PropertyBag> Of(JointRow row) => row.Switch(
        weld: r =>
            from throat in Measured(DetailSchema.EffectiveThroat, Dimension.LengthDim, r.EffectiveThroatMm * 1e-3)
            from length in Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.LengthMm * 1e-3)
            select RealizationRows(Joint("Welded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), throat, length),
        stud: r =>
            from diameter in Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, r.Class.DiameterMm * 1e-3)
            from length in Measured(DetailSchema.NominalLength, Dimension.LengthDim, r.LengthBeforeWeldMm.Value * 1e-3)
            select RealizationRows(Joint("Welded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), diameter, length),
        adhesive: r =>
            from bond in Measured(DetailSchema.BondLine, Dimension.LengthDim, r.BondMm.Value * 1e-3)
            from overlap in Measured(DetailSchema.Overlap, Dimension.LengthDim, r.OverlapMm.Value * 1e-3)
            select RealizationRows(Joint("Bonded"), Token(DetailSchema.FastenerType, r.Kind.IfcPredefinedType), bond, overlap));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The joint family seed — ONE AUTHORED closed table over the JointRow family (SEED_ROW_LAW: no vendor producer;
// AWS D1.1 / ISO 13918 / ASTM values PUBLISHED). ComponentFamily.Joint binds Rows; every row is Sectioned: false
// (a weld/bond/stud fills no ComputedSection). The dual IFC entity binds at seed as the kind row's OWN Binding
// projection, never a hand string and never an external equality branch.
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

    // The closed table covers every geometry arm plus the stud and adhesive modalities.
    static readonly Seq<JointRow> Table = Seq<JointRow>(
        new JointRow.Weld("joint.weld-fillet-6mm-e70", new WeldGeometry.Fillet(PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(100.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-fillet-8mm-e70", new WeldGeometry.Fillet(PositiveMagnitude.Create(8.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(150.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-fillet-10mm-e80", new WeldGeometry.Fillet(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(16.0), PositiveMagnitude.Create(200.0)), ElectrodeClass.E80),
        new JointRow.Weld("joint.weld-groove-v-cjp-e80", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.SingleV, Penetration.Cjp, BackingType.Steel, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Saw, PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(250.0)), ElectrodeClass.E80),
        new JointRow.Weld("joint.weld-groove-bevel-pjp-e90", new WeldGeometry.Groove(
            new GroovePrep(GrooveGeometry.SingleBevel, Penetration.Pjp, BackingType.None, GroovePrep.StandardRootOpeningMm, GroovePrep.StandardRootFaceMm),
            WeldProcess.Smaw, PositiveMagnitude.Create(16.0), PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(300.0)), ElectrodeClass.E90),
        new JointRow.Weld("joint.weld-plug-20mm-e70", new WeldGeometry.Plug(new HoleWeld(PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(8.0))), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-slot-20x60-e70", new WeldGeometry.Slot(new HoleWeld(PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(8.0)), PositiveMagnitude.Create(60.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-flarebevel-r10-e70", new WeldGeometry.FlareBevel(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(120.0)), ElectrodeClass.E70),
        new JointRow.Weld("joint.weld-flarev-r10-e70", new WeldGeometry.FlareV(PositiveMagnitude.Create(10.0), PositiveMagnitude.Create(6.0), PositiveMagnitude.Create(120.0)), ElectrodeClass.E70),
        new JointRow.Stud("joint.stud-13mm-h75",  StudClass.S13, StudGrade.Sd1,  PositiveMagnitude.Create(75.0),  PositiveMagnitude.Create(150.0)),
        new JointRow.Stud("joint.stud-19mm-h100", StudClass.S19, StudGrade.Sd1,  PositiveMagnitude.Create(100.0), PositiveMagnitude.Create(200.0)),
        new JointRow.Stud("joint.stud-22mm-h125", StudClass.S22, StudGrade.AwsB, PositiveMagnitude.Create(125.0), PositiveMagnitude.Create(250.0)),
        new JointRow.Stud("joint.stud-25mm-h150", StudClass.S25, StudGrade.Sd3,  PositiveMagnitude.Create(150.0), PositiveMagnitude.Create(300.0)),
        new JointRow.Adhesive("joint.adhesive-epoxy-2mm", AdhesiveClass.Epoxy,              PositiveMagnitude.Create(2.0),  PositiveMagnitude.Create(25.0), PositiveMagnitude.Create(50.0)),
        new JointRow.Adhesive("joint.adhesive-mma-1mm",   AdhesiveClass.Methacrylate,       PositiveMagnitude.Create(1.0),  PositiveMagnitude.Create(20.0), PositiveMagnitude.Create(40.0)),
        new JointRow.Adhesive("joint.adhesive-ssg-12mm",  AdhesiveClass.SiliconeStructural, PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(12.0), PositiveMagnitude.Create(1000.0)));

    // The weld geometry -> SectionProfile arm per ComponentFamily.Joint.admits: FilletTriangle the fillet AND flare
    // welds (the equal-leg gross triangle; the 0.707·leg and FlareThroatFactor·radius throats stay the row's DEFINED
    // derivations, never profile state), Trapezium the groove, Circle the plug/slot hole footprint.
    static Fin<SectionProfile> WeldProfileOf(JointRow.Weld row, Op key) => row.Geometry.Switch(
        fillet: geometry => SectionProfile.FilletTriangle.Of(geometry.LegMm.Value, geometry.LegMm.Value, key),
        groove: geometry => GrooveProfile(geometry, key),
        plug: geometry => SectionProfile.Circle.Of(geometry.Hole.DiameterMm.Value, key),
        slot: geometry => SectionProfile.Circle.Of(geometry.Hole.DiameterMm.Value, key),
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

    // ONE row dispatch — the generated total Switch over the JointRow family (a fourth modality breaks this arm at
    // compile time). Per case: the dimensional gates (the lifted values ARE the gate — a non-positive column aborts
    // before any construction; the stud gate also proves the ISO 13918 L2 = L1 − burn-off positive), the AISC J2.4
    // minimum-leg gate over the weld case's OWN algebra (a seeded fillet undersized for its thinner part is a
    // transcription fault; the J2.2b edge maximum stays a design-check read — it is T-vs-lap configuration-dependent),
    // the profile rail, the seed-built bag, and the dual IfcBinding read whole off the case's kind row. Both stud
    // MaterialId slots ride the GRADE row (the SD3 stainless renders chromium, never blanket steel).
    static Fin<ComponentRow> Row(JointRow row, Op key) => row.Switch(
        weld: r =>
            from legged in r.Geometry.Switch(
                fillet: geometry => guard(geometry.LegMm.Value >= JointRow.Weld.MinimumFilletLegMm(geometry.PartMm.Value), ComponentFault.Dimension(key, $"<fillet-leg-below-j2.4-minimum:{r.Designation}>")).ToFin(),
                groove: static _ => Fin.Succ(unit), plug: static _ => Fin.Succ(unit), slot: static _ => Fin.Succ(unit),
                flareBevel: static _ => Fin.Succ(unit), flareV: static _ => Fin.Succ(unit))
            from profile in WeldProfileOf(r, key)
            from detail in JointDetail.Of(r)
            from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, r.Kind.Binding,
                Coring.None, WeldStandard, substanceId: r.Substance, appearanceId: r.Appearance, detail: Some(detail), key)
            select new ComponentRow(item, Sectioned: false),
        stud: r =>
            from realized in key.AcceptValidated<PositiveMagnitude>(candidate: r.RealizedLengthMm)
            from profile in SectionProfile.Circle.Of(diameterMm: r.Class.DiameterMm, key)
            from detail in JointDetail.Of(r)
            from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, r.Kind.Binding,
                Coring.None, WeldStandard, substanceId: r.Substance, appearanceId: r.Grade.Appearance, detail: Some(detail), key)
            select new ComponentRow(item, Sectioned: false),
        adhesive: r =>
            from profile in SectionProfile.Nominal.Of(nominalMm: r.BondMm.Value, key)
            from detail in JointDetail.Of(r)
            from item in Component.Of(ComponentFamily.Joint, r.Designation, profile, r.Kind.Binding,
                Coring.None, AdhesiveStandard, substanceId: r.Substance, appearanceId: MaterialId.Of("polymer.adhesive"), detail: Some(detail), key)
            select new ComponentRow(item, Sectioned: false));

    // The family fold ComponentFamily.Joint binds: ONE Traverse over the ONE closed table — Fin's applicative UNIONS
    // the faults, so every malformed row across all three modalities reports in one build abort (the prior
    // three-table tuple Apply, the Choose + .Concat swallowing fold, and the unfused .Map(f).Traverse(identity)
    // re-spelling are the deleted forms).
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Table.Traverse(r => Row(r, context.Key)).As();
}
```

## [03]-[RESEARCH]

- [SEED_PARADIGM]: REALIZED — the bespoke `JointSection` `[Union]` and its `ComponentSection.Joint` arm are DELETED; geometry lands as the family-admitted `SectionProfile` arms (`FilletTriangle` fillet/flare, `Trapezium` groove — the DEFINED prep-envelope derivation, `Circle` plug/slot hole + stud shank, `Nominal` continuous bond-line), the realization scalars land in the `JointDetail` `DetailSchema.Realization` bag (rows byte-identical to the retired projector `Detail` switch), and the throat/design-shear algebra re-homes as DEFINED derived columns on the ONE `[Union]` `JointRow` seed-row family (`JointRow.Weld.EffectiveThroatMm`/`DesignShearKn`/`DirectionalShearKn`, `JointRow.Stud.RealizedLengthMm`/`DesignShearKn`, `JointRow.Adhesive.DesignShearKn` — the three sibling structs collapsed per SHAPE_BUDGET: shared admission path, shared detail-bag consumer, shared construction). `ElectrodeClass`(6)/`StudGrade`(5)/`AdhesiveClass`(4) convert 1:1 from `[SmartEnum]` to frozen `readonly record struct` row tables (values verbatim, `PUBLISHED` provenance — no runtime key lookup, no delegate column, no IFC-token derivation); `StudClass`(5) converts carrying the `DEFINED` `SteelShearKn = Rg·Rp·Asc·Fu` column with the symbol path (`StudClass.S19.SteelShearKn`) and every value byte-identical for the `steel#STEEL_FAMILY` `Composite` `ΣQn` read. `JointSeed.Rows : Context -> Fin<Seq<ComponentRow>>` is ONE `Traverse` over the ONE closed table through the ONE `Switch`-dispatched `Row` — `Fin`'s applicative unions the faults so every malformed row reports in one build abort (the fault-swallowing `Choose` + `.Concat`, the unfused `.Map(f).Traverse(identity)` re-spelling, and the three-table tuple `Apply` all retired); every row `Sectioned: false`, and `JointDetail.Of(JointRow)` is the one modality-polymorphic bag entry (the prior three public overloads collapsed onto the generated total `Switch`).
- [WELD_GEOMETRY_AND_DIRECTIONAL]: `WeldGeometry` separates six payload shapes. Fillet, groove, and flare cases derive line strength from their own throat and run; plug and slot derive area strength from `HoleWeld`. `DirectionalShearKn(Angle)` applies AISC J2-5 only to directional rows and reads `Angle.Radians`. `MinimumFilletLegMm(II)` composes the verified profile thickness members.
- [GROOVE_PREP_GEOMETRY]: REALIZED — `GroovePrep` captures the AWS A2.4 / D1.1 preparation (`GrooveGeometry` angle/radius — U = 6 mm / J = 10 mm standard prep radii, `Penetration`, `BackingType`, root opening/face). `EffectiveThroatMm` is the AISC J2.1 throat: CJP the full connected-part thickness, PJP the depth less the `WeldProcess` deduction at a sharp (`RequiresPjpDeduction`) groove — the deduction applies to SMAW/GMAW/FCAW ONLY (SAW takes full depth). `RootTreatment` stays split from `BackingType` (treatment vs material). The `Trapezium` profile is the DEFINED gross prep envelope (root opening flaring at the included/bevel angle over the depth); the throat NEVER reads the profile.
- [PLUG_SLOT_WELD]: `HoleWeld` carries the shared admitted diameter and depth; `WeldGeometry.Plug` derives `π/4·d²`, while `WeldGeometry.Slot` carries its additional length and derives `d·l`. The always-true fill flag is absent.
- [STRUCTURAL_ADHESIVE_AND_SSG]: `AdhesiveClass.StructuralBiteMpa` is present only for the structural-silicone row. `JointRow.Adhesive.DesignTensionKn` therefore returns `Option<double>`; epoxy, methacrylate, and polyurethane cannot acquire a fabricated ASTM C1401 tensile allowance.
- [ISO_13918_SHEAR_STUD]: REALIZED — the `StudClass` frozen rows carry the ISO 13918:2017 Type SD per-diameter geometry (13/16/19/22/25 mm; head `dc` 25/32/32/35/40, `h3` 8/8/10/10/12, collar `d1` 17/21/23/29/31, collar height 3/4.5/6/6/7, burn-off 3/4/4.5/5/5.5, the constant `140°` tip) and the AWS D1.1 Type B / AISC §I8 `Fu = 450 MPa` cap; `JointRow.Stud` exposes both L1 and the DEFINED L2 = L1 − burn-off (the seed gate proves L2 positive). `StudGrade` carries the specified `fy`/`fu` for the EN 1994 §6.6.3.1 PRd path — the typed `En1994` citation NAMES the composite code — plus the GRADE-borne appearance slot (`metal.chrome` the SD3 austenitic stainless, `metal.steel` the carbon grades; the blanket steel appearance the stud row previously hardcoded is the deleted form). `DesignShearKn` emits the STEEL-side `SteelShearKn = Rg·Rp·Asc·Fu` (strong-position `Rg = 1.0`/`Rp = 0.75`); the concrete-side `0.5·Asc·√(f'c·Ec)` join is `Rasm.Compute/structural#DESIGN_CHECK`'s `min(steel, concrete)`. Ripple counterpart: `steel#STEEL_FAMILY` (`CompositeDetail.Stud` reads `StudClass.SteelShearKn`; the `Rasm.Materials.Component.Joint` sub-namespace import re-points to the flattened parent namespace).
- [IFC_JOINT_WIRE]: REALIZED — the dual entity binds AT SEED per the IFC-BINDING law: `JointKind.Binding` derives `IfcBinding.Of(kind.IfcEntity, kind.IfcPredefinedType)` whole off the kind row — the entity a vocabulary COLUMN (the prior `kind == JointKind.Stud` equality branch in a seed helper re-derived encoded policy outside its owner and is the deleted form; a new kind that omits its entity is now a missing constructor argument, never a silently non-stud entity) — `STUDSHEARCONNECTOR` the welded stud (`STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, never a fastener value), `WELD`/`GLUE` the weld/adhesive bead on the non-mechanical `IfcFastener` (the assay-verified `{NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}` set). The `JointDetail` bag rows (`EffectiveThroat`/`NominalLength` weld, `BondLine`/`Overlap` adhesive, `NominalDiameter`/`NominalLength` stud, each dimension-only SI) are byte-identical to the retired switch, so the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader and the `Projection/egress#IFC_EGRESS` `Emit` + `IfcRelConnectsWithRealizingElements` edge round-trip unchanged. Ripple counterpart: `Projection/component.md` (the `Detail` switch deletion — `ProjectType` reads `c.Detail`).
