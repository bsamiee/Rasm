# [MATERIALS_JOINT]

THE CONTINUOUS-CONNECTION COMPONENT FAMILY and THE WELD/ADHESIVE/STUD GENERATIVE VOCABULARY. The joint vocabulary — the `JointKind` weld/adhesive/stud discriminant, the collapsed `WeldType` geometry axis plus its `GrooveGeometry`/`WeldProcess`/`Penetration`/`WeldFace`/`RootTreatment`/`BackingType` welding sub-axes, the `ElectrodeClass`/`AdhesiveClass`/`StudClass`/`StudGrade` strength axes, and the `JointSection` `[Union]` continuous-connection receipt carrying the `WeldProfile`/`GroovePrep`/`PlugSlot` generative geometry — is the FOURTH realized `component#COMPONENT_OWNER` family, the `ComponentClass.Minor` (`IfcElementComponent`) metallurgical/adhesive complement to the discrete reinforcement/fastener/connector parts. A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed item: it carries no thread or bar-diameter cross-section, so it cannot fold into an existing family arm the way `anchor` folds into `fastener` — the SINGLE deliberate widening past the discrete triple, justified because the steel `steel#STEEL_FAMILY` `Composite` arm depends on the `StudClass` shear-stud vocabulary landing here and the continuous-connection discipline has no representation among the discrete parts. A fillet weld is a `Component` row in the `joint` family, never a `Weld` type: the weld geometry, the groove preparation, the bead profile, the electrode strength, and the throat are `JointSection.Weld` columns, and the `JointSection` projection feeds the SAME `component#COMPONENT_OWNER` `Component.Of` admission and the SAME `ComponentCatalogue.Build` fold the eight sibling families drive. The joint vocabulary grows by data — a new electrode is one `ElectrodeClass` row, a new groove one `GrooveGeometry` row, a new stud diameter one `StudClass` row, a new stud grade one `StudGrade` row, a new designation one `WeldRow`/`StudRow`/`AdhesiveRow` catalogue entry — never a per-joint type. The geometry is the hand-rolled generative capture (AWS D1.1:2020 + AISC 360 J2/I8 + ISO 13918:2017 + EN 1994): GeometryGym mirrors only the IFC class, never a weld solid, and VividOrange owns no weld; the bead/groove/stud solid is materialized by the host from the scalar receipt, NEVER a host `Curve` here. The `JointSection.DesignShearKn` projection emits the SPEC-NOMINAL filler/adhesive/stud-steel design shear (the `RebarSection.YieldForceKn` / `FastenerSection.ProofLoadKn` mirror) while the measured base-metal capacity is the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt the design seam reads by `MaterialId`, never re-derived here; the AISC J2.4 minimum fillet leg reads the connected member's `VividOrange.Profiles` `II` `FlangeThickness`/`WebThickness`, and the composite stud's concrete-governed branch is the `Rasm.Compute/structural#DESIGN_CHECK` join over the slab `f'c`/`Ec`, neither a column here. `StudClass.SteelShearKn` is the ONE shear-stud vocabulary the `steel#STEEL_FAMILY` `Composite` arm reads for its `ΣQn` cap. The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentId`/`ComponentSection`/`ComponentFault` shape, the `Rasm.Vectors` `PositiveMagnitude` for every dimensional column, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` appearance the row carries, and the `Projection/component#COMPONENT_PROJECTOR` detail-bag lowering plus the `Rasm.Bim` egress that selects the `IfcFastener` (`WELD`/`GLUE`) / `IfcMechanicalFastener` (`STUDSHEARCONNECTOR`) realizing element.

## [01]-[INDEX]

- [02]-[JOINT_FAMILY]: the `JointKind` weld/adhesive/stud discriminant with its `IfcPredefinedType`; the collapsed `WeldType` (fillet/groove/plug/slot/flare-bevel/flare-v) geometry axis with the `LineWeld` design-shear discriminant and the AWS flare-throat factor; the `WeldProcess`/`GrooveGeometry`/`Penetration`/`WeldFace`/`RootTreatment`/`BackingType` welding sub-axes; the `ElectrodeClass` (AWS A5.1 / A5.5) filler-metal axis; the `AdhesiveClass` (lap-shear + ASTM C1401 SSG) axis; the `StudClass` (ISO 13918 Type SD per-diameter geometry + AISC `SteelShearKn`) and `StudGrade` (SD1/SD2/SD3 + AWS Type A/B `fy`/`fu`) stud axes; the `WeldProfile`/`GroovePrep`/`PlugSlot` generative-geometry records; the `JointSection` `[Union]` continuous-connection receipt with its KEPT `NominalMm`, `EffectiveThroatMm`, the AWS D1.1 + AISC J2-5 directional `DesignShearKn`/`DirectionalShearKn`, and the `RealizedLengthMm` projection; and the `ComponentCatalogue.BuildJointRows` typed-seed weld/stud/adhesive table plus the AISC J2.4 `MinimumFilletLegMm(II)` connected-part read.

## [02]-[JOINT_FAMILY]

- Owner: the joint vocabulary (`JointKind` the weld/adhesive/stud discriminant carrying the `IfcPredefinedType`; `WeldType` the 6 AWS D1.1 weld geometries collapsed off the groove subtypes; `WeldProcess` the AISC J2.1 PJP-deduction axis; `GrooveGeometry` the 9 AWS A2.4 groove geometries; `Penetration` the CJP/PJP throat discriminant; `WeldFace`/`RootTreatment` the bead-profile axes; `BackingType` the groove backing; `ElectrodeClass` the AWS A5.1/A5.5 filler axis; `AdhesiveClass` the structural-adhesive/SSG axis; `StudClass`/`StudGrade` the ISO 13918 stud geometry + grade axes; `WeldProfile`/`GroovePrep`/`PlugSlot` the generative geometry records; `JointSection` the `[Union]` continuous-connection receipt); `ComponentCatalogue.BuildJointRows` the registered-row seed the `component#COMPONENT_OWNER` `ComponentCatalogue.Build` concatenates; the `JointSection.DesignShearKn` SPEC-NOMINAL projection emitting the weld/bond/stud-steel design shear the structural-connection-design seam reads (a TOTAL `double` over the section's own filler/adhesive/stud-steel band, the base-metal/concrete receipt the design seam's, never re-derived here).
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `GrooveGeometry` × `ElectrodeClass` × `WeldProcess`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass` × `StudGrade`)} — the closed continuous-connection family carried in one `JointSection` `[Union]` arm; weld {fillet · groove (square/V/bevel/U/J × single/double on `GrooveGeometry`) · plug · slot · flare-bevel · flare-v} over the E60..E110 electrode band; adhesive {epoxy · methacrylate · polyurethane · silicone-structural} over the lap-shear/peel/SSG-bite band; stud {the 13..25 mm ISO 13918 Type SD headed connectors} over diameter/grade/height/spacing — a joint is a `Component` row in `ComponentFamily.Joint`, never a joint subtype, and the groove subtype geometry is a `GrooveGeometry` row, never a per-subtype `WeldType` (a 14-member `WeldType` duplicating the 9 `GrooveGeometry` cases is the deleted form, the `[01]-[WORKSPACE_LAW]` no-split).
- Entry: `public double DesignShearKn` on `JointSection` — the SPEC-NOMINAL continuous-connection design shear, a TOTAL projection over the section's own filler/adhesive/stud-steel band exactly as `RebarSection.YieldForceKn` reads `RebarGrade.MinimumYieldMpa·NominalAreaMm2`: a LINE weld (fillet/groove/flare, `WeldType.LineWeld`) is the AWS D1.1 `0.6·FEXX·throat·length` (the throat the `EffectiveThroatMm` projection — `0.707·leg` fillet, the `GroovePrep.EffectiveThroatMm` CJP-full / PJP-depth-less-deduction groove, the `FlareThroatFactor·radius` flare), a PLUG/SLOT weld the AWS D1.1 area-shear `0.6·FEXX·PlugSlot.ShearAreaMm2` over the faying-plane hole footprint (a realized branch, not a 0-returning stub), an adhesive the ASTM D1002 `LapShearMpa·overlap·width`, a stud the AISC 360-22 Eq I8-1 STEEL-side `StudClass.SteelShearKn = Rg·Rp·Asc·Fu`; `public double DirectionalShearKn(double loadAngleDeg)` applies the AISC 360 Eq J2-5 directional strength increase `1 + 0.50·sin^1.5θ` to a fillet/flare line weld (`θ` the loading angle from the weld axis, `1.0` longitudinal / `1.5` transverse), the groove/plug/stud/adhesive arms returning the base shear; there is NO `resolve`/`Func<MaterialId, …>` parameter and NO base-metal/concrete read — the `MaterialId`-keyed `Mechanical` receipt is the design seam's and the composite stud's `0.5·Asc·√(f'c·Ec)` concrete branch the `Rasm.Compute/structural#DESIGN_CHECK` join. `ComponentCatalogue.BuildJointRows(context)` folds the typed `WeldRow`/`StudRow`/`AdhesiveRow` seed tables through `WeldOf`/`StudOf`/`AdhesiveOf` into the registered `Component` rows `ComponentCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetWeldByType`/`GetStudBySize` family.
- Packages: Rasm.Vectors (project — `PositiveMagnitude` for the throat/leg/size/length/bond-line/overlap/width/spacing columns, never an int-backed `Dimension` that truncates a fractional millimetre throat; the kernel value-object atoms live in `Rasm.Vectors`, NOT `Rasm.Domain`), Rasm.Domain (`Op` the boundary-admission key, the `AcceptValidated` column-admission extension, `Context` the catalogue fold), Rasm.Element (`MaterialId` the seam-carried appearance/capacity identity), Rasm.Materials.Component (the parent `component#COMPONENT_OWNER` — `ComponentFamily`/`ComponentId`/`Component`/`ComponentSection`/`ComponentFault`), Thinktecture.Runtime.Extensions (`[Union]` for `JointSection` the payload-carrying receipt, `[SmartEnum<string>]` for the kind/weld-geometry/process/groove/penetration/face/root/backing/electrode/adhesive/stud/grade axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), VividOrange.Profiles (`II` the I-section `FlangeThickness`/`WebThickness` the AISC J2.4 `MinimumFilletLegMm` connected-part read consumes; `.api/api-vividorange-profiles-catalogue.md`), UnitsNet (`Length.Millimeters` the `II` thickness coercion at the read edge; `.api/api-unitsnet.md`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Choose`/`Match` for the admission rail and catalogue fold), BCL inbox (`FrozenDictionary`). No new external package — the generative geometry is hand-rolled in-fence; GeometryGym mirrors only the IFC class, never a weld solid.
- Growth: the joint vocabulary grows by data — a new weld geometry is one `WeldType` row, a new groove one `GrooveGeometry` row carrying its angle/radius, a new electrode one `ElectrodeClass` row carrying its FEXX tensile + specification, a new adhesive one `AdhesiveClass` row, a new stud diameter one `StudClass` row carrying its ISO 13918 geometry, a new stud grade one `StudGrade` row carrying its `fy`/`fu`, a new designation one `WeldRow`/`StudRow`/`AdhesiveRow` catalogue entry — never a per-joint type, never a `Weld`/`AdhesiveBond`/`ShearStud` sibling type. The `ComponentFamily` axis is closed at NINE (masonry/cmu/steel/timber/glazing/reinforcement/fastener/connector/joint), the continuous weld/bond/stud the deliberate continuous-connection case; a structural-joint utilisation case is future `capacity#SECTION_CAPACITY` `[Union]` growth, not this campaign.
- Boundary: the joint vocabulary is the realized continuous-connection `ComponentFamily` — a per-joint `Weld`/`ShearStud` class, a 14-member `WeldType` duplicating the groove subtypes, and a VividOrange/host weld owner are the deleted forms; `JointSection` composes the `Rasm.Vectors` `PositiveMagnitude` for every dimensional column (`SizeMm`/`LengthMm`/`PartThicknessMm` for the weld, `BondLineMm`/`OverlapMm`/`WidthMm` for the adhesive, `LengthBeforeWeldMm`/`SpacingMm` for the stud) so a fractional weld throat (`a = 0.707·leg`) admits without the truncation an int-backed `Dimension` count forces; the continuous weld/bond/stud has no thread or bar-diameter cross-section, so the `component#COMPONENT_OWNER` `ComponentSection.Joint(Continuous)` arm projects `CrossNominalMm` to `j.Continuous.NominalMm` — the throat-or-bond-line-or-shank the family-agnostic `ComponentSection` already dispatches, widened by one arm — while `JointSection.NominalMm` is KEPT (the disambiguation gives the Component-level projection `CrossNominalMm` and keeps the joint arm's own `NominalMm`: the weld nominal size, the adhesive glueline, the stud shank); the weld geometry is the scalar `WeldProfile`/`GroovePrep`/`PlugSlot` receipt the host materializes into the bead/groove/plug solid — NEVER a host `Curve`, the host lofts the weld from the scalar throat exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel`/`metal.iron` for a weld over the electrode, `polymer.adhesive` for a bond, `metal.steel` for a stud) the row's `AppearanceId` carries, independent from the `metal.steel` `CapacityKey` by the two-slot law; the structural capacity SPLITS by source — the section's `DesignShearKn` is the SPEC-NOMINAL filler/adhesive/stud-steel band (`ElectrodeClass.TensileMpa` the AWS A5.1/A5.5 filler strength, `AdhesiveClass.LapShearMpa` the ASTM D1002 allowable, `StudClass.UltimateMpa` the AWS D1.1 Type B / AISC §I8 `Fu`), while the measured base-metal capacity crosses to `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` read by `CapacityKey` and the composite stud's concrete-governed `0.5·Asc·√(f'c·Ec)` branch the `Rasm.Compute/structural#DESIGN_CHECK` join, so the section threads no `resolve` func; the AISC J2.4 minimum fillet leg reads the connected member's `VividOrange.Profiles` `II.FlangeThickness`/`WebThickness` (the thinner part governs) through `MinimumFilletLegMm(II)`, the one VividOrange composition (the published I-section geometry, never a hand-keyed thickness); `ComponentCatalogue.BuildJointRows` seeds the `component#COMPONENT_OWNER` registry with the typed AWS D1.1 weld / structural-adhesive / ISO 13918 shear-stud rows keyed `joint.<designation>` (the family prefix, the literal `connection.` prefix retired), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`, the generative geometry (`WeldProfile`/`GroovePrep`/`PlugSlot`, the stud collar) filled from the `[SmartEnum]` defaults so the seed carries only the design columns; the placement of a weld schedule or stud pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold (a stud row keyed by spacing is a station-stepped pattern, a continuous weld a single run-length placement), never a parallel joint-layout owner; the item reaches `Rasm.Bim` ONLY as the `Projection/component#COMPONENT_PROJECTOR`-authored seam node plus its neutral detail-bag (NEVER a second `JointWire` carrier), the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader recovering the scalar columns and the `Projection/semantic#IFC_EGRESS` `Emit` serializing them to the IFC 4.3 element the `ComponentSection.IfcEntity` joint arm selects — an `IfcMechanicalFastener` (`PredefinedType` `STUDSHEARCONNECTOR`) for the welded shear stud, an `IfcFastener` (`IfcFastenerTypeEnum` `WELD` for the weld bead, `GLUE` for the adhesive) for the continuous weld/bond — plus an `IfcRelConnectsWithRealizingElements` (the seam `Relations/relation#EDGE_ALGEBRA` `Connect(ConnectKind.Realizing)` edge the app/Bim authors once both member ids are known); the joint `FastenerType` detail token is the verified `IfcMechanicalFastenerTypeEnum` member name (`STUDSHEARCONNECTOR` for the welded stud), never the bare reinforcing-bar `STUD` the cast-in bar carries.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;     // FrozenDictionary (the registered-row table)
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude — throat/leg/size/length/bond-line/overlap/width/spacing) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated column-admission extension, Context (the catalogue fold)
using Rasm.Element;                  // MaterialId (the seam-carried material identity the JointSection AppearanceId/CapacityKey reference)
using Rasm.Materials.Component;      // ComponentFamily/ComponentId/Component/ComponentSection/Coring/ComponentStandard/ComponentAuthority (the parent COMPONENT_OWNER; ComponentFault rails transitively via Component.Of)
using Thinktecture;
using UnitsNet;                      // Length (.Millimeters on the II.FlangeThickness/WebThickness reads)
using VividOrange.Profiles;          // II (the I-section FlangeThickness/WebThickness the AISC J2.4 MinimumFilletLegMm connected-part read consumes)
using static LanguageExt.Prelude;

// Each Component family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the nine sibling
// `ComponentCatalogue` static classes are distinct types (a shared Rasm.Materials.Component collides at CS0101 with
// component.md's own `ComponentCatalogue`); component#COMPONENT_OWNER stays the parent Rasm.Materials.Component and
// folds Joint.ComponentCatalogue.BuildJointRows by the sub-namespace-qualified name. This page DEFINES StudClass the
// steel#STEEL_FAMILY composite leg imports as Rasm.Materials.Component.Joint; parent owner types via the using above.
namespace Rasm.Materials.Component.Joint;

// --- [TYPES] -------------------------------------------------------------------------------
// The continuous-connection family discriminant the JointSection [Union] mirrors and the JointRow carries; a value
// discriminant (the generated total Switch over weld/adhesive/stud), NEVER a [Union] (the cases carry no payload here —
// the payload lives on the JointSection arm). IfcPredefinedType is the ONE per-kind predefined token the
// component#COMPONENT_OWNER ComponentSection.PredefinedToken joint arm READS (never re-hardcoded there) and the
// Projection/component#COMPONENT_PROJECTOR lands on the seam Object node's PredefinedType + the detail bag's FastenerType
// row: a welded stud is the IfcMechanicalFastenerTypeEnum STUDSHEARCONNECTOR, a weld bead the IfcFastenerTypeEnum WELD,
// an adhesive bead the IfcFastenerTypeEnum GLUE (the Bim Emit gate resolves + validates against the frozen valid-set —
// STUD is the IfcReinforcingBarTypeEnum cast-in bar, never a fastener value, the [IFC_JOINT_WIRE] distinction).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointKind {
    public static readonly JointKind Weld     = new("weld",     ifcPredefinedType: "WELD");
    public static readonly JointKind Adhesive = new("adhesive", ifcPredefinedType: "GLUE");
    public static readonly JointKind Stud     = new("stud",     ifcPredefinedType: "STUDSHEARCONNECTOR");
    public string IfcPredefinedType { get; }
}

// The 6 AWS D1.1 weld GEOMETRIES. The groove SUBTYPE geometry (square/V/bevel/U/J × single/double) lives on
// GrooveGeometry, NOT a per-subtype WeldType — a 14-member WeldType duplicating the 9 GrooveGeometry cases is the deleted
// form (the no-split collapse). LineWeld flags the DesignShearKn branch: a fillet/groove/flare resists on the throat ×
// length line-shear, a plug/slot on the faying-plane HOLE AREA (PlugSlot.ShearAreaMm2). FlareThroatFactor is the AWS
// D1.1 Table 5.2 flare-groove radius factor (5/16·R flare-bevel, 1/2·R flare-V — the non-SAW common value), 0 elsewhere.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldType {
    public static readonly WeldType Fillet     = new("fillet",      lineWeld: true,  flareThroatFactor: 0.0);
    public static readonly WeldType Groove     = new("groove",      lineWeld: true,  flareThroatFactor: 0.0);
    public static readonly WeldType Plug       = new("plug",        lineWeld: false, flareThroatFactor: 0.0);
    public static readonly WeldType Slot       = new("slot",        lineWeld: false, flareThroatFactor: 0.0);
    public static readonly WeldType FlareBevel = new("flare-bevel", lineWeld: true,  flareThroatFactor: 0.3125);
    public static readonly WeldType FlareV     = new("flare-v",     lineWeld: true,  flareThroatFactor: 0.5);
    public bool LineWeld { get; }
    public double FlareThroatFactor { get; }
}

// AISC 360 Table J2.1 PJP effective-throat deduction: SMAW/GMAW/FCAW deduct 3 mm (1/8 in) at a sharp (<60°) bevel groove
// where reliable root fusion is process-limited; SAW's deeper penetration takes the FULL groove depth (no deduction).
// The deduction is the one PJP throat-loss column GroovePrep.EffectiveThroatMm reads — never a SAW value (the corrected
// process-set: the prior field + table wrongly included SAW; AISC J2.1 deducts only for SMAW/GMAW/FCAW).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldProcess {
    public static readonly WeldProcess Smaw = new("smaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Gmaw = new("gmaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Fcaw = new("fcaw", pjpDeductionMm: 3.0);
    public static readonly WeldProcess Saw  = new("saw",  pjpDeductionMm: 0.0);
    public double PjpDeductionMm { get; }
}

// The 9 AWS A2.4 groove geometries the GroovePrep.Geometry carries — moved OFF WeldType (the no-split collapse).
// IncludedAngleDeg is the V/U total angle, BevelAngleDeg the single-wall bevel/J angle, RootRadiusMm the U/J root radius
// (U = 6 mm, J = 10 mm — the standard prep radii, NOT a 'U larger' rule). DoubleSided flags a both-face prep. Square
// carries no angle or radius. A sharp bevel groove (45° bevel, no radius) takes the WeldProcess PJP deduction; a 60° V,
// a radiused U/J, or any CJP develops the full depth (RequiresPjpDeduction false).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
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
public sealed partial class Penetration {
    public static readonly Penetration Cjp = new("cjp", complete: true);
    public static readonly Penetration Pjp = new("pjp", complete: false);
    public bool Complete { get; }
}

// The bead face contour (AWS D1.1 weld-profile acceptance): flat, convex (fillet reinforcement), concave (the
// reduced-throat profile). The face describes the deposited reinforcement; the effective throat is measured to the
// THEORETICAL face (0.707·leg for a fillet), never the convex reinforcement, so the face is descriptive not structural.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldFace {
    public static readonly WeldFace Flat    = new("flat");
    public static readonly WeldFace Convex  = new("convex");
    public static readonly WeldFace Concave = new("concave");
}

// The root condition the WeldProfile carries — Backgouge is the root-treatment axis SPLIT OUT of the groove BackingType
// (a back-gouged + back-welded root is a root TREATMENT, the backing bar a groove MATERIAL): AsWelded the open root left
// as deposited, Backgouge gouged to sound metal and back-welded, SealPass a seal weld over the root.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RootTreatment {
    public static readonly RootTreatment AsWelded  = new("as-welded");
    public static readonly RootTreatment Backgouge = new("backgouge");
    public static readonly RootTreatment SealPass  = new("seal-pass");
}

// The groove backing material the GroovePrep carries — None for an open/back-gouged root, Steel for a fused steel
// backing bar, Ceramic/Copper/Flux for the removable/consumable backings. Distinct from the WeldProfile RootTreatment
// (the Backgouge axis split out): backing is a MATERIAL, root-treatment a CONDITION.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BackingType {
    public static readonly BackingType None    = new("none");
    public static readonly BackingType Steel   = new("steel");
    public static readonly BackingType Ceramic = new("ceramic");
    public static readonly BackingType Copper  = new("copper");
    public static readonly BackingType Flux    = new("flux");
}

// AWS A5.1 carbon-steel (E60/E70) and AWS A5.5 low-alloy (E80/E90/E100/E110) covered-electrode classifications;
// TensileMpa is the FEXX minimum filler-metal tensile. E80+ are A5.5 (the low-alloy specification), NOT A5.1 — the
// corrected spec axis the matching-filler selection reads (E60/E70 alone are A5.1). AppearanceId is the weld finish
// (metal.iron for the lowest carbon-steel filler, metal.steel above) the JointSection.AppearanceId weld arm reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElectrodeClass {
    public static readonly ElectrodeClass E60  = new("e60",  tensileMpa: 415.0, specification: "AWS A5.1", appearanceId: "metal.iron");
    public static readonly ElectrodeClass E70  = new("e70",  tensileMpa: 485.0, specification: "AWS A5.1", appearanceId: "metal.steel");
    public static readonly ElectrodeClass E80  = new("e80",  tensileMpa: 550.0, specification: "AWS A5.5", appearanceId: "metal.steel");
    public static readonly ElectrodeClass E90  = new("e90",  tensileMpa: 620.0, specification: "AWS A5.5", appearanceId: "metal.steel");
    public static readonly ElectrodeClass E100 = new("e100", tensileMpa: 690.0, specification: "AWS A5.5", appearanceId: "metal.steel");
    public static readonly ElectrodeClass E110 = new("e110", tensileMpa: 760.0, specification: "AWS A5.5", appearanceId: "metal.steel");
    public double TensileMpa { get; }
    public string Specification { get; }
    public string AppearanceId { get; }
}

// Structural-adhesive lap-shear (ASTM D1002), T-peel (ASTM D1876), and the ASTM C1401 structural-sealant-glazing (SSG)
// design tensile allowable the silicone curtain-wall bite develops. LapShearMpa drives the bonded-lap DesignShearKn;
// StructuralBiteMpa the SSG tensile bite (silicone the SSG sealant at the highest service temperature, the 0.14 MPa /
// 20 psi C1401 design allowable distinct from its cured lap-shear).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdhesiveClass {
    public static readonly AdhesiveClass Epoxy              = new("epoxy",               lapShearMpa: 30.0, peelNmm: 5.0,  serviceCelsius: 80.0,  structuralBiteMpa: 6.0);
    public static readonly AdhesiveClass Methacrylate       = new("methacrylate",        lapShearMpa: 25.0, peelNmm: 12.0, serviceCelsius: 100.0, structuralBiteMpa: 5.0);
    public static readonly AdhesiveClass Polyurethane       = new("polyurethane",        lapShearMpa: 15.0, peelNmm: 20.0, serviceCelsius: 90.0,  structuralBiteMpa: 2.0);
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", lapShearMpa: 1.0,  peelNmm: 8.0,  serviceCelsius: 150.0, structuralBiteMpa: 0.14);
    public double LapShearMpa { get; }
    public double PeelNmm { get; }
    public double ServiceCelsius { get; }
    public double StructuralBiteMpa { get; }
}

// ISO 13918:2017 Type SD headed shear connectors (AWS D1.1 Type B equivalent) — the per-diameter standard geometry keyed
// by the nominal shank: DiameterMm (ISO d, the AWS imperial nominal), HeadDiameterMm (ISO dc / d5), HeadThicknessMm (h3),
// WeldCollarDiameterMm (ISO d1 / d3 — the as-welded base fillet), WeldCollarHeightMm (ISO h / h4), BurnoffMm (the L1−L2
// length the arc consumes, 3..5.5 mm by diameter). UltimateMpa is the AWS D1.1 Type B / AISC §I8 Fu = 450 MPa nominal the
// SteelShearKn AISC cap reads (the StudGrade axis carries the specified fy/fu for the EN 1994 PRd path); TipAngleDeg is the
// ISO 13918 140° ± 7° point. The weld develops over the shank cross-section AreaMm2 and forms the WeldCollarDiameterMm
// footprint, so no separate weld-area column — the collar IS the as-welded base diameter.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StudClass {
    public static readonly StudClass S13 = new("stud-1/2", diameterMm: 12.7, headDiameterMm: 25.0, headThicknessMm: 8.0,  weldCollarDiameterMm: 17.0, weldCollarHeightMm: 3.0, burnoffMm: 3.0,  ultimateMpa: 450.0);
    public static readonly StudClass S16 = new("stud-5/8", diameterMm: 15.9, headDiameterMm: 32.0, headThicknessMm: 8.0,  weldCollarDiameterMm: 21.0, weldCollarHeightMm: 4.5, burnoffMm: 4.0,  ultimateMpa: 450.0);
    public static readonly StudClass S19 = new("stud-3/4", diameterMm: 19.1, headDiameterMm: 32.0, headThicknessMm: 10.0, weldCollarDiameterMm: 23.0, weldCollarHeightMm: 6.0, burnoffMm: 4.5,  ultimateMpa: 450.0);
    public static readonly StudClass S22 = new("stud-7/8", diameterMm: 22.2, headDiameterMm: 35.0, headThicknessMm: 10.0, weldCollarDiameterMm: 29.0, weldCollarHeightMm: 6.0, burnoffMm: 5.0,  ultimateMpa: 450.0);
    public static readonly StudClass S25 = new("stud-1",   diameterMm: 25.4, headDiameterMm: 40.0, headThicknessMm: 12.0, weldCollarDiameterMm: 31.0, weldCollarHeightMm: 7.0, burnoffMm: 5.5,  ultimateMpa: 450.0);
    public const double TipAngleDeg = 140.0;
    public double DiameterMm { get; }
    public double HeadDiameterMm { get; }
    public double HeadThicknessMm { get; }
    public double WeldCollarDiameterMm { get; }
    public double WeldCollarHeightMm { get; }
    public double BurnoffMm { get; }
    public double UltimateMpa { get; }
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
    // AISC 360-22 Eq I8-1 steel-side strength Rg·Rp·Asc·Fu — the conservative one-stud-per-rib / strong-position default
    // (Rg = 1.0, Rp = 0.75); a weak-position or multi-stud-per-rib layout is a Rasm.Compute placement input, NOT a
    // stud-vocabulary edit, so the cap a schedule reports is the strong-position upper bound the design seam tightens by
    // the realized deck/rib geometry. This is the ONE shear-stud cap the steel#STEEL_FAMILY Composite arm reads for ΣQn.
    public double GroupFactorRg => 1.0;
    public double PositionFactorRp => 0.75;
    public double SteelShearKn => GroupFactorRg * PositionFactorRp * AreaMm2 * UltimateMpa * 1e-3;
}

// ISO 13918:2017 SD material grades + AWS D1.1 stud types — the specified fy/fu the EN 1994 §6.6.3.1 PRd design and the
// generative material data read (the spec band rides the StudClass-keyed SteelShearKn AISC cap, the grade the separate
// material axis): SD1 (S235J2G3+C450, the standard carbon shear stud, Re 350 / Rm 450), SD2 (the higher-elongation carbon
// variant, Re 235 / Rm 400), SD3 (X5CrNi18-10 austenitic stainless, Rm 500), AWS Type A (general purpose), Type B (shear
// connector, the SD1 equivalent). EN 1994 caps the design fu at 500 MPa; AISC §I8 caps Fu at 450 MPa.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StudGrade {
    public static readonly StudGrade Sd1  = new("sd1",   yieldMpa: 350.0, ultimateMpa: 450.0);
    public static readonly StudGrade Sd2  = new("sd2",   yieldMpa: 235.0, ultimateMpa: 400.0);
    public static readonly StudGrade Sd3  = new("sd3",   yieldMpa: 350.0, ultimateMpa: 500.0);
    public static readonly StudGrade AwsA = new("aws-a", yieldMpa: 340.0, ultimateMpa: 420.0);
    public static readonly StudGrade AwsB = new("aws-b", yieldMpa: 350.0, ultimateMpa: 450.0);
    public double YieldMpa { get; }
    public double UltimateMpa { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The deposited bead profile (AWS D1.1 weld-profile geometry) — the face contour, the convex reinforcement above the
// theoretical face, the toe radius, and the root treatment. ReinforcementMm / ToeRadiusMm are non-negative doubles (a
// flat-faced weld has 0 reinforcement, a sharp toe 0 radius), descriptive metrics the host materializes, NEVER the
// structural throat (the throat is the EffectiveThroatMm projection over the theoretical face).
public readonly record struct WeldProfile(WeldFace Face, double ReinforcementMm, double ToeRadiusMm, RootTreatment Root);

// The groove preparation geometry (AWS A2.4 / AWS D1.1): the GrooveGeometry (carrying the angle/radius), the Penetration
// (CJP/PJP), the BackingType, and the as-prepared root opening + root face. The angle/radius projections READ the
// GrooveGeometry defaults so the standard prep is not re-keyed per joint. EffectiveThroatMm is the AISC J2.1 throat: a
// CJP groove develops the full connected-part thickness, a PJP groove the depth-of-prep less the WeldProcess deduction
// at a sharp (RequiresPjpDeduction) bevel groove, clamped non-negative.
public readonly record struct GroovePrep(GrooveGeometry Geometry, Penetration Penetration, BackingType Backing, double RootOpeningMm, double RootFaceMm) {
    public double IncludedAngleDeg => Geometry.IncludedAngleDeg;
    public double BevelAngleDeg => Geometry.BevelAngleDeg;
    public double GrooveRadiusMm => Geometry.RootRadiusMm;
    public double EffectiveThroatMm(double depthMm, double partThicknessMm, WeldProcess process) =>
        Penetration.Complete
            ? partThicknessMm
            : Math.Max(0.0, depthMm - (Geometry.RequiresPjpDeduction ? process.PjpDeductionMm : 0.0));
}

// The plug/slot hole geometry (AWS D1.1 §2.x). A plug weld is a circular hole (SlotLengthMm = 0); a slot weld an
// elongated hole (SlotLengthMm > 0). ShearAreaMm2 is the faying-plane footprint — π/4·d² (plug) or d·slotLength (slot) —
// the AWS D1.1 area-shear branch DesignShearKn takes when WeldType.LineWeld is false. Filled flags a fully-filled hole.
public readonly record struct PlugSlot(PositiveMagnitude HoleDiameterMm, PositiveMagnitude DepthMm, double SlotLengthMm, bool Filled) {
    public double ShearAreaMm2 => SlotLengthMm > 0.0
        ? HoleDiameterMm.Value * SlotLengthMm
        : Math.PI * 0.25 * HoleDiameterMm.Value * HoleDiameterMm.Value;
}

// One continuous-connection receipt across all three joint kinds, the component#COMPONENT_OWNER ComponentSection.Joint
// arm carries (the Continuous field). NominalMm is KEPT (the joint arm reads j.Continuous.NominalMm, the Component-level
// CrossNominalMm projection dispatches onto it): the weld nominal size (leg/depth/radius), the adhesive glueline, the
// stud shank. EffectiveThroatMm is the derived structural throat (weld) / glueline (adhesive) / collar height (stud);
// RealizedLengthMm the weld run / adhesive lap / stud as-welded L2 = L1 − burn-off.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointSection {
    private JointSection() { }

    // The weld arm — the WeldType geometry + WeldProcess + ElectrodeClass + the WeldProfile bead + the Option<GroovePrep>
    // (Some only for Groove) + the Option<PlugSlot> (Some only for Plug/Slot) + SizeMm (the type's primary dim: fillet
    // leg / groove depth-of-prep / flare bend radius / plug-slot weld depth) + LengthMm (the run length) + PartThicknessMm
    // (the governing thinner connected part the AISC J2.4 min-leg + CJP throat read).
    public sealed record Weld(WeldType Type, WeldProcess Process, ElectrodeClass Electrode, WeldProfile Profile, Option<GroovePrep> Prep, Option<PlugSlot> Hole, PositiveMagnitude SizeMm, PositiveMagnitude LengthMm, PositiveMagnitude PartThicknessMm) : JointSection;
    public sealed record Adhesive(AdhesiveClass Class, PositiveMagnitude BondLineMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm) : JointSection;
    public sealed record Stud(StudClass Class, StudGrade Grade, PositiveMagnitude LengthBeforeWeldMm, PositiveMagnitude SpacingMm) : JointSection;

    public JointKind Kind => Switch(
        weld:     static _ => JointKind.Weld,
        adhesive: static _ => JointKind.Adhesive,
        stud:     static _ => JointKind.Stud);

    // The KEPT joint-arm nominal the component#COMPONENT_OWNER ComponentSection.CrossNominalMm projects onto via
    // j.Continuous.NominalMm: the weld nominal SIZE (leg/depth/radius, the catalogued dimension, always > 0), the adhesive
    // glueline, the stud shank — distinct from the derived EffectiveThroatMm (which a PJP groove can drive small).
    public PositiveMagnitude NominalMm => Switch(
        weld:     static w => w.SizeMm,
        adhesive: static a => a.BondLineMm,
        stud:     static s => PositiveMagnitude.Create(s.Class.DiameterMm));

    // The FINISH the appearance projection reads (the electrode appearance for a weld, the adhesive polymer for a bond,
    // steel for a stud), INDEPENDENT from CapacityKey by the component#COMPONENT_OWNER two-slot law — the
    // Appearance/graph#MATERIAL_LIBRARY MaterialId inbound carrier this family preserves.
    public MaterialId AppearanceId => Switch(
        weld:     static w => MaterialId.Of(w.Electrode.AppearanceId),
        adhesive: static _ => MaterialId.Of("polymer.adhesive"),
        stud:     static _ => MaterialId.Of("metal.steel"));

    // The CAPACITY material whose Properties/properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads —
    // the joint's base-metal STEEL, sourced INDEPENDENTLY from the AppearanceId finish so a bonded joint whose appearance
    // is the adhesive polymer still reads its steel base-metal capacity.
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    // The derived structural throat — the weld effective throat (AISC J2 / the WeldThroat dispatch), the adhesive
    // glueline, or the stud weld-collar height. Plug/slot welds carry a 0 line throat (they resist on the hole area).
    public double EffectiveThroatMm => Switch(
        weld:     static w => WeldThroat(w),
        adhesive: static a => a.BondLineMm.Value,
        stud:     static s => s.Class.WeldCollarHeightMm);

    // The realized linear extent: the weld run length, the bonded lap length, or the stud AS-WELDED length L2 = L1 −
    // burn-off (so the stud exposes both L1 = LengthBeforeWeldMm and L2 = RealizedLengthMm, the ISO 13918 arc shortening).
    public double RealizedLengthMm => Switch(
        weld:     static w => w.LengthMm.Value,
        adhesive: static a => a.OverlapMm.Value,
        stud:     static s => s.LengthBeforeWeldMm.Value - s.Class.BurnoffMm);

    // SPEC-NOMINAL continuous-connection design shear — the RebarSection.YieldForceKn / FastenerSection.ProofLoadKn
    // mirror: a TOTAL double over the section's OWN filler/adhesive/stud-steel spec band, NEVER a base-metal re-derivation
    // (the MaterialId-keyed Mechanical receipt is the design seam's). A LINE weld is the AWS D1.1 0.6·FEXX·throat·length; a
    // PLUG/SLOT weld the area-shear 0.6·FEXX·ShearAreaMm2 over the faying-plane hole (LineWeld the branch discriminant, not
    // a capability gate); an adhesive the ASTM D1002 lap-shear·overlap·width; a stud the AISC 360-22 Eq I8-1 STEEL-side
    // StudClass.SteelShearKn — the concrete-side 0.5·Asc·√(f'c·Ec) governs only when the slab is weak, that join the
    // Rasm.Compute/structural#DESIGN_CHECK min(steel, concrete) over the seam slab f'c/Ec, NOT a section column here.
    public double DesignShearKn => Switch(
        weld:     static w => WeldShear(w),
        adhesive: static a => a.Class.LapShearMpa * a.OverlapMm.Value * a.WidthMm.Value * 1e-3,
        stud:     static s => s.Class.SteelShearKn);

    // AISC 360 Eq J2-5 directional strength increase kds = 1 + 0.50·sin^1.5θ for a FILLET / FLARE line weld — θ the angle
    // of loading from the weld longitudinal axis (0° longitudinal kds = 1.0, 90° transverse kds = 1.5). A groove develops
    // the base metal and a plug/slot resists on the hole area, so kds is 1 for the groove/plug/slot/adhesive/stud arms.
    public double DirectionalShearKn(double loadAngleDeg) => Switch(
        weld:     w => w.Type == WeldType.Fillet || w.Type == WeldType.FlareBevel || w.Type == WeldType.FlareV
                      ? WeldShear(w) * (1.0 + 0.50 * Math.Pow(Math.Sin(loadAngleDeg * Math.PI / 180.0), 1.5))
                      : WeldShear(w),
        adhesive: a => a.Class.LapShearMpa * a.OverlapMm.Value * a.WidthMm.Value * 1e-3,
        stud:     s => s.Class.SteelShearKn);

    // The weld effective throat dispatch: 0.707·leg fillet, the GroovePrep CJP/PJP throat (the connected-part thickness
    // for CJP, the depth-of-prep less the WeldProcess deduction for a sharp PJP groove), 0 for a plug/slot line throat,
    // FlareThroatFactor·radius for a flare groove. A groove with no carried GroovePrep falls back to the part thickness.
    static double WeldThroat(Weld w) => w.Type.Switch(
        fillet:     _ => 0.707 * w.SizeMm.Value,
        groove:     _ => w.Prep.Match(p => p.EffectiveThroatMm(w.SizeMm.Value, w.PartThicknessMm.Value, w.Process), () => w.PartThicknessMm.Value),
        plug:       _ => 0.0,
        slot:       _ => 0.0,
        flareBevel: _ => w.Type.FlareThroatFactor * w.SizeMm.Value,
        flareV:     _ => w.Type.FlareThroatFactor * w.SizeMm.Value);

    // The base (longitudinal, kds = 1) weld shear: the throat × length line-shear for a line weld, the hole-area shear for
    // a plug/slot (the PlugSlot.ShearAreaMm2, falling back to the SizeMm circular footprint when no hole geometry rides).
    static double WeldShear(Weld w) => w.Type.LineWeld
        ? 0.6 * w.Electrode.TensileMpa * WeldThroat(w) * w.LengthMm.Value * 1e-3
        : 0.6 * w.Electrode.TensileMpa * w.Hole.Match(h => h.ShearAreaMm2, () => Math.PI * 0.25 * w.SizeMm.Value * w.SizeMm.Value) * 1e-3;
}

// The typed catalogue seed rows — one record per family so each carries only ITS columns (no nullable smear over a flat
// JointRow, no fragile Type-string parse): the WeldType/GrooveGeometry/ElectrodeClass/WeldProcess refs are the SmartEnum
// values directly. The generative geometry (WeldProfile, GroovePrep, PlugSlot, the stud collar) fills from the SmartEnum
// defaults in WeldOf/StudOf, so the seed carries only the design columns + the family key.
public readonly record struct WeldRow(string Designation, WeldType Type, GrooveGeometry Groove, Penetration Pen, ElectrodeClass Electrode, WeldProcess Process, double SizeMm, double PartMm, double LengthMm);
public readonly record struct StudRow(string Designation, StudClass Class, StudGrade Grade, double LengthBeforeWeldMm, double SpacingMm);
public readonly record struct AdhesiveRow(string Designation, AdhesiveClass Class, double BondMm, double OverlapMm, double WidthMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The non-regional joint standard the registered rows cite (AWS D1.1 / AISC 360 / ISO 13918 are weld/stud specifications,
    // not a masonry-style regional joint thickness) — StandardJointThicknessMm 0.0 (a continuous weld/bond/stud lays no mortar
    // joint), ComponentAuthority.Astm the AWS/AISC-aligned US authority, mirroring steel.md's Aisc/En statics.
    static readonly ComponentStandard Standard = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    // AWS D1.1 weld seed (SizeMm = fillet leg / groove depth-of-prep / flare radius / plug-hole diameter; PartMm = the
    // governing thinner connected part; LengthMm = run length). Groove rows carry the GrooveGeometry + Penetration the
    // GroovePrep reads; fillet/plug/flare rows carry Square + Pjp as the inert default the prep ignores.
    static readonly Seq<WeldRow> WeldRows = Seq(
        new WeldRow("joint.weld-fillet-6mm-e70",     WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70,  WeldProcess.Smaw, 6.0,  10.0, 100.0),
        new WeldRow("joint.weld-fillet-8mm-e70",     WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70,  WeldProcess.Gmaw, 8.0,  12.0, 150.0),
        new WeldRow("joint.weld-fillet-10mm-e80",    WeldType.Fillet,     GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E80,  WeldProcess.Fcaw, 10.0, 16.0, 200.0),
        new WeldRow("joint.weld-groove-v-cjp-e80",   WeldType.Groove,     GrooveGeometry.SingleV,     Penetration.Cjp, ElectrodeClass.E80,  WeldProcess.Saw,  12.0, 12.0, 250.0),
        new WeldRow("joint.weld-groove-bevel-pjp-e90", WeldType.Groove,   GrooveGeometry.SingleBevel, Penetration.Pjp, ElectrodeClass.E90,  WeldProcess.Smaw, 16.0, 20.0, 300.0),
        new WeldRow("joint.weld-plug-20mm-e70",      WeldType.Plug,       GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70,  WeldProcess.Smaw, 20.0, 8.0,  20.0),
        new WeldRow("joint.weld-flarebevel-r10-e70", WeldType.FlareBevel, GrooveGeometry.Square,      Penetration.Pjp, ElectrodeClass.E70,  WeldProcess.Gmaw, 10.0, 6.0,  120.0));

    // ISO 13918 Type SD shear-stud seed (LengthBeforeWeldMm = L1, the catalogue length before the burn-off shortening;
    // SpacingMm = the station-stepped pitch the Construction/layout#ASSEMBLY_FOLD pattern reads). The per-diameter collar/head geometry
    // rides StudClass; the grade rides StudGrade.
    static readonly Seq<StudRow> StudRows = Seq(
        new StudRow("joint.stud-13mm-h75",  StudClass.S13, StudGrade.Sd1,  75.0,  150.0),
        new StudRow("joint.stud-19mm-h100", StudClass.S19, StudGrade.Sd1,  100.0, 200.0),
        new StudRow("joint.stud-22mm-h125", StudClass.S22, StudGrade.AwsB, 125.0, 250.0),
        new StudRow("joint.stud-25mm-h150", StudClass.S25, StudGrade.Sd3,  150.0, 300.0));

    // Structural-adhesive seed (BondMm = glueline thickness; OverlapMm = the bonded lap / SSG structural bite; WidthMm =
    // the joint width). The SSG silicone row is the curtain-wall structural-glazing bead.
    static readonly Seq<AdhesiveRow> AdhesiveRows = Seq(
        new AdhesiveRow("joint.adhesive-epoxy-2mm", AdhesiveClass.Epoxy,              2.0,  25.0, 50.0),
        new AdhesiveRow("joint.adhesive-mma-1mm",   AdhesiveClass.Methacrylate,       1.0,  20.0, 40.0),
        new AdhesiveRow("joint.adhesive-ssg-12mm",  AdhesiveClass.SiliconeStructural, 12.0, 12.0, 1000.0));

    // One weld row -> the JointSection.Weld with the generative geometry filled from the SmartEnum defaults: a flat-faced
    // (convex for a fillet) WeldProfile, a GroovePrep ONLY for a Groove (the standard 2 mm root opening / 1.5 mm root face,
    // a steel backing for a CJP groove and an open root for a PJP), a PlugSlot ONLY for a Plug/Slot (the SizeMm hole
    // diameter, the PartMm weld depth, a circular plug). The dimensional columns admit once through AcceptValidated.
    static Fin<(ComponentId Id, Component Item)> WeldOf(WeldRow r, Op key) =>
        from size in key.AcceptValidated<PositiveMagnitude>(candidate: r.SizeMm)
        from part in key.AcceptValidated<PositiveMagnitude>(candidate: r.PartMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
        let profile = new WeldProfile(r.Type == WeldType.Fillet ? WeldFace.Convex : WeldFace.Flat, r.Type == WeldType.Fillet ? 1.0 : 0.0, 0.0, RootTreatment.AsWelded)
        let prep = r.Type == WeldType.Groove
            ? Some(new GroovePrep(r.Groove, r.Pen, r.Pen.Complete ? BackingType.Steel : BackingType.None, RootOpeningMm: 2.0, RootFaceMm: 1.5))
            : Option<GroovePrep>.None
        let hole = r.Type == WeldType.Plug || r.Type == WeldType.Slot
            ? Some(new PlugSlot(size, part, r.Type == WeldType.Slot ? r.LengthMm : 0.0, Filled: true))
            : Option<PlugSlot>.None
        let section = (JointSection)new JointSection.Weld(r.Type, r.Process, r.Electrode, profile, prep, hole, size, length, part)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, new ComponentSection.Joint(section), Coring.None, Standard, section.CapacityKey, section.AppearanceId, key)
        select (ComponentId.Of(r.Designation), item);

    static Fin<(ComponentId Id, Component Item)> StudOf(StudRow r, Op key) =>
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthBeforeWeldMm)
        from spacing in key.AcceptValidated<PositiveMagnitude>(candidate: r.SpacingMm)
        let section = (JointSection)new JointSection.Stud(r.Class, r.Grade, length, spacing)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, new ComponentSection.Joint(section), Coring.None, Standard, section.CapacityKey, section.AppearanceId, key)
        select (ComponentId.Of(r.Designation), item);

    static Fin<(ComponentId Id, Component Item)> AdhesiveOf(AdhesiveRow r, Op key) =>
        from bond in key.AcceptValidated<PositiveMagnitude>(candidate: r.BondMm)
        from overlap in key.AcceptValidated<PositiveMagnitude>(candidate: r.OverlapMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
        let section = (JointSection)new JointSection.Adhesive(r.Class, bond, overlap, width)
        from item in Component.Of(ComponentFamily.Joint, r.Designation, new ComponentSection.Joint(section), Coring.None, Standard, section.CapacityKey, section.AppearanceId, key)
        select (ComponentId.Of(r.Designation), item);

    // The context-folded registered-row table component#COMPONENT_OWNER ComponentCatalogue.Build concatenates — one fold
    // over the three typed seeds, a malformed row dropping through Choose rather than seeding a degenerate Component.
    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO
    // explicit comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>,
    // a type mismatch on a ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention).
    public static FrozenDictionary<ComponentId, Component> BuildJointRows(Context context) =>
        WeldRows.Choose(r => WeldOf(r, default).ToOption())
            .Concat(StudRows.Choose(r => StudOf(r, default).ToOption()))
            .Concat(AdhesiveRows.Choose(r => AdhesiveOf(r, default).ToOption()))
            .ToFrozenDictionary(static r => r.Id, static r => r.Item);

    // AISC 360 Table J2.4 minimum fillet weld leg from the governing thinner connected part (the thresholds 6/13/20 mm =
    // 1/4 / 1/2 / 3/4 in -> 3/5/6/8 mm). The II overload reads the VividOrange.Profiles I-section FlangeThickness /
    // WebThickness (the thinner governs) for a flange-or-web fillet — the one VividOrange composition, the published
    // section geometry rather than a hand-keyed thickness.
    public static double MinimumFilletLegMm(double thinnerPartMm) => thinnerPartMm switch {
        <= 6.0  => 3.0,
        <= 13.0 => 5.0,
        <= 20.0 => 6.0,
        _       => 8.0
    };

    public static double MinimumFilletLegMm(II connectedPart) =>
        MinimumFilletLegMm(Math.Min(connectedPart.FlangeThickness.Millimeters, connectedPart.WebThickness.Millimeters));

    // AISC 360 J2.2b maximum fillet weld leg along an edge: the full edge thickness for material < 6 mm (1/4 in), else
    // the edge thickness less 1.6 mm (1/16 in) — the upper bound the design check pairs with MinimumFilletLegMm.
    public static double MaximumFilletLegMm(double edgeThicknessMm) =>
        edgeThicknessMm < 6.0 ? edgeThicknessMm : edgeThicknessMm - 1.6;
}
```

## [03]-[RESEARCH]

- [WELD_GEOMETRY_AND_DIRECTIONAL]: REALIZED — the AWS D1.1 weld geometry collapses to the 6-member `WeldType` (fillet/groove/plug/slot/flare-bevel/flare-v), the groove SUBTYPE geometry (square/V/bevel/U/J × single/double) moved OFF `WeldType` onto the 9-member `GrooveGeometry` (a 14-member `WeldType` duplicating the groove subtypes is the deleted form, the `[01]-[WORKSPACE_LAW]` no-split). The fillet `EffectiveThroatMm` is the AWS D1.1 / AISC J2.2a `0.707·leg` (equal-leg, the theoretical throat over the `WeldProfile.Face`, never the convex reinforcement); the flare-bevel `0.3125·R` and flare-V `0.5·R` are the AWS D1.1 Table 5.2 radius factors (the non-SAW common value). `DesignShearKn` is the AWS D1.1 line `0.6·FEXX·throat·length`, and `DirectionalShearKn(loadAngleDeg)` applies the AISC 360 Eq J2-5 directional strength increase `1 + 0.50·sin^1.5θ` to a fillet/flare weld (`1.0` longitudinal, `1.5` transverse), the groove/plug/stud/adhesive arms returning the base. `MinimumFilletLegMm` is the AISC 360 Table J2.4 minimum leg keyed by the thinner connected part (6/13/20 mm -> 3/5/6/8 mm), the `II` overload reading the `VividOrange.Profiles` `FlangeThickness`/`WebThickness` (`.api/api-vividorange-profiles-catalogue.md` `II`), `MaximumFilletLegMm` the AISC J2.2b edge bound. The matching base-metal capacity is the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrength` read by `CapacityKey`, never a weld column. Ripple counterpart: `steel#STEEL_FAMILY` (the connected `II` section the J2.4 min-leg reads).
- [GROOVE_PREP_GEOMETRY]: REALIZED — the `GroovePrep` captures the AWS A2.4 / AWS D1.1 groove preparation: the `GrooveGeometry` (carrying `IncludedAngleDeg` for the V/U total angle, `BevelAngleDeg` for the single-wall bevel/J angle, `RootRadiusMm` U = 6 mm / J = 10 mm — the standard prep radii, never a 'U larger' rule), the `Penetration` (CJP/PJP), the `BackingType`, and the as-prepared `RootOpeningMm`/`RootFaceMm`. `EffectiveThroatMm` is the AISC J2.1 throat: a CJP groove develops the full connected-part thickness (the weld matches the base metal), a PJP groove the depth-of-prep less the `WeldProcess` deduction. The PJP deduction (3 mm / 1/8 in) applies to SMAW/GMAW/FCAW at a sharp (`RequiresPjpDeduction`: 45° bevel, no root radius) groove ONLY — SAW's deeper penetration takes the full depth (no deduction), the corrected process-set the prior field + table wrongly extended to SAW (AISC J2.1 deducts for SMAW/GMAW/FCAW per the root-fusion reliability at angles below 60°). The `WeldProfile.Root` `RootTreatment` (AsWelded/Backgouge/SealPass) is the Backgouge axis SPLIT OUT of the groove `BackingType` (a back-gouged + back-welded root is a root TREATMENT, the backing bar a groove MATERIAL).
- [PLUG_SLOT_WELD]: REALIZED — the `PlugSlot` captures the AWS D1.1 plug/slot hole geometry (`HoleDiameterMm`, `DepthMm`, `SlotLengthMm` the elongation for a slot vs 0 for a plug, `Filled`). The plug/slot weld carries the `WeldType.LineWeld == false` flag because it resists on its faying-plane HOLE AREA, not a linear throat × length, so `DesignShearKn` takes the realized area-shear branch `0.6·FEXX·ShearAreaMm2` (`π/4·d²` plug / `d·slotLength` slot) — the seeded `joint.weld-plug-20mm-e70` row exercises it, a realized branch never a 0-returning stub.
- [STRUCTURAL_ADHESIVE_AND_SSG]: REALIZED — the `AdhesiveClass` lap-shear (ASTM D1002 single-lap), T-peel (ASTM D1876), and ASTM C1401 structural-sealant-glazing (SSG) tensile-bite allowables seed the structural-adhesive rows; the bonded-lap design capacity is `LapShearMpa·overlap·width` over the lap shear-transfer area. Epoxy (30 MPa lap-shear, 80 °C), methacrylate (25 MPa, high peel, 100 °C), polyurethane (15 MPa, very high peel), and structural silicone (1 MPa lap-shear, the `StructuralBiteMpa` 0.14 MPa / 20 psi ASTM C1401 SSG design tensile the curtain-wall bite develops, 150 °C the highest service temperature) are the seed; the `joint.adhesive-ssg-12mm` row is the SSG curtain-wall structural bead a bonded curtain-wall keys and serializes the way a weld does.
- [ISO_13918_SHEAR_STUD]: REALIZED — the ISO 13918:2017 Type SD headed shear connectors (AWS D1.1 Type B equivalent) seed the `StudClass` axis at the 13/16/19/22/25 mm nominal diameters with the per-diameter standard geometry: `HeadDiameterMm` (ISO dc, 25/32/32/35/40), `HeadThicknessMm` (h3, 8/8/10/10/12), `WeldCollarDiameterMm` (ISO d1, the as-welded base fillet, 17/21/23/29/31), `WeldCollarHeightMm` (ISO h, 3/4.5/6/6/7), `BurnoffMm` (the L1−L2 arc shortening, 3/4/4.5/5/5.5), and the constant 140° ± 7° `TipAngleDeg`. The `JointSection.Stud` arm exposes both L1 (`LengthBeforeWeldMm`) and L2 (`RealizedLengthMm` = L1 − burn-off, the as-welded length). The `StudGrade` axis carries the specified `fy`/`fu`: SD1 (S235J2G3+C450, the standard carbon shear stud, 350/450), SD2 (the higher-elongation carbon variant, 235/400), SD3 (X5CrNi18-10 austenitic stainless, 350/500), AWS Type A (general purpose, 340/420), Type B (shear connector, 350/450). `DesignShearKn` emits the STEEL-side `StudClass.SteelShearKn = Rg·Rp·Asc·Fu` (the AISC 360-22 Eq I8-1 spec-nominal upper bound over the AWS D1.1 Type B / §I8 `UltimateMpa` = 450 MPa cap, the conservative `Rg = 1.0`/`Rp = 0.75` strong-position default a `Rasm.Compute` deck/rib placement tightens) — it reads no concrete: the concrete-side `0.5·Asc·√(f'c·Ec)` governs only when the slab is weak, that join the `Rasm.Compute/structural#DESIGN_CHECK` `min(steel, concrete)` over the seam slab `f'c`/`Ec`. This is the ONE shear-stud vocabulary the `steel#STEEL_FAMILY` `Composite` arm reads for its `ΣQn = StudClass.SteelShearKn × StudsPerMetre` cap — a composite floor beam's stud row keys `joint.stud-19mm-h100`, this page reports the stud-steel cap, never a re-minted stud type. Ripple counterpart: `steel#STEEL_FAMILY` (the `CompositeDetail.Stud` `StudClass.SteelShearKn` read).
- [IFC_JOINT_WIRE]: the joint family round-trips to IFC 4.3 split by the `component#COMPONENT_OWNER` `ComponentSection.IfcEntity` joint arm — the welded shear stud as an `IfcMechanicalFastener` (`PredefinedType` `STUDSHEARCONNECTOR`; `STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, NOT an `IfcMechanicalFastenerTypeEnum` value, so the welded stud is `STUDSHEARCONNECTOR` on the fastener wire), the continuous weld bead and the structural-adhesive bead as an `IfcFastener` (the non-mechanical sibling; `IfcFastenerTypeEnum` `WELD` for the weld, `GLUE` for the adhesive — the assay-verified `{NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}` set), each carried as `JointKind.IfcPredefinedType` the `ComponentSection.PredefinedToken` joint arm reads. The page emits the scalar columns the `Projection/component#COMPONENT_PROJECTOR` lowers into the neutral detail bag (the weld `EffectiveThroat`/`RealizedLength`, the adhesive `BondLine`/`Overlap`, the stud `NominalDiameter`/`RealizedLength`), the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader recovering them at IFC ingress and the `Projection/semantic#IFC_EGRESS` `Emit` serializing the IFC element + an `IfcRelConnectsWithRealizingElements` (the seam `Relations/relation#EDGE_ALGEBRA` `Connect(ConnectKind.Realizing)` edge the app/Bim authors once both member ids are known) — host-neutral here, never an IFC entity and never a second carrier. Ripple counterpart: `Projection/component#COMPONENT_PROJECTOR` (the detail-bag lowering) + `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` (the reader/round-trip).
