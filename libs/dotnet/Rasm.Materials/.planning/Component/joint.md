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
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
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

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldFace {
    public static readonly WeldFace Flat    = new("flat");
    public static readonly WeldFace Convex  = new("convex");
    public static readonly WeldFace Concave = new("concave");
}

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Penetration {
    public static readonly Penetration Cjp = new("cjp");
    public static readonly Penetration Pjp = new("pjp");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RootTreatment {
    public static readonly RootTreatment AsWelded  = new("as-welded");
    public static readonly RootTreatment Backgouge = new("backgouge");
    public static readonly RootTreatment SealPass  = new("seal-pass");
}

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
    public double GroupFactor { get; }
    public double PositionFactor { get; }
}

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

public readonly record struct AdhesiveClass(string Key, double LapShearMpa, double PeelNmm, double ServiceCelsius, Option<double> StructuralBiteMpa, string SubstanceId) {
    public static readonly AdhesiveClass Epoxy              = new("epoxy",               30.0, 5.0,  80.0,  None,       "adhesive.epoxy");
    public static readonly AdhesiveClass Methacrylate       = new("methacrylate",        25.0, 12.0, 100.0, None,       "adhesive.methacrylate");
    public static readonly AdhesiveClass Polyurethane       = new("polyurethane",        15.0, 20.0, 90.0,  None,       "adhesive.polyurethane");
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", 1.0,  8.0,  150.0, Some(0.14), "sealant.silicone-structural");
    public static readonly ImmutableArray<AdhesiveClass> Rows = [Epoxy, Methacrylate, Polyurethane, SiliconeStructural];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
}

public readonly record struct StudClass(string Key, double DiameterMm, double HeadDiameterMm, double HeadThicknessMm, double WeldCollarDiameterMm, double WeldCollarHeightMm, double BurnoffMm, double UltimateMpa) {
    public static readonly StudClass S13 = new("stud-1/2", 12.7, 25.0, 8.0,  17.0, 3.0, 3.0,  450.0);
    public static readonly StudClass S16 = new("stud-5/8", 15.9, 32.0, 8.0,  21.0, 4.5, 4.0,  450.0);
    public static readonly StudClass S19 = new("stud-3/4", 19.1, 32.0, 10.0, 23.0, 6.0, 4.5,  450.0);
    public static readonly StudClass S22 = new("stud-7/8", 22.2, 35.0, 10.0, 29.0, 6.0, 5.0,  450.0);
    public static readonly StudClass S25 = new("stud-1",   25.4, 40.0, 12.0, 31.0, 7.0, 5.5,  450.0);
    public const double TipAngleDeg = 140.0;
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
    public double SteelShearKn(StudGroup group) => group.GroupFactor * group.PositionFactor * AreaMm2 * UltimateMpa * 1e-3;
}

public static class StudClasses {
    public static readonly ImmutableArray<StudClass> Rows = [StudClass.S13, StudClass.S16, StudClass.S19, StudClass.S22, StudClass.S25];
}

public readonly record struct StudGrade(string Key, double YieldMpa, double UltimateMpa, string SubstanceId, string AppearanceId) {
    const double EnUltimateCapMpa = 500.0;
    const double EnGammaV = 1.25;
    public static readonly StudGrade Sd1  = new("sd1",   350.0, 450.0, "steel.sd1",   "metal.steel");
    public static readonly StudGrade Sd2  = new("sd2",   235.0, 400.0, "steel.sd2",   "metal.steel");
    public static readonly StudGrade Sd3  = new("sd3",   350.0, 500.0, "steel.sd3",   "metal.chrome");
    public static readonly StudGrade AwsA = new("aws-a", 340.0, 420.0, "steel.aws-a", "metal.steel");
    public static readonly StudGrade AwsB = new("aws-b", 350.0, 450.0, "steel.aws-b", "metal.steel");
    public static readonly ImmutableArray<StudGrade> Rows = [Sd1, Sd2, Sd3, AwsA, AwsB];
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);

    public double EnShearResistanceKn(StudClass stud) =>
        0.8 * Math.Min(UltimateMpa, EnUltimateCapMpa) * stud.AreaMm2 / EnGammaV * 1e-3;

    public static double EnHeightFactor(StudClass stud, double realizedHeightMm) =>
        realizedHeightMm / stud.DiameterMm > 4.0 ? 1.0 : 0.2 * (realizedHeightMm / stud.DiameterMm + 1.0);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WeldProfile(WeldFace Face, double ReinforcementMm, double ToeRadiusMm, RootTreatment Root);

public readonly record struct GroovePrep(GrooveGeometry Geometry, Penetration Penetration, BackingType Backing, RootTreatment Root, double RootOpeningMm, double RootFaceMm) {
    public const double StandardRootOpeningMm = 2.0;
    public const double StandardRootFaceMm = 1.5;
    public double IncludedAngleDeg => Geometry.IncludedAngleDeg;
    public double BevelAngleDeg => Geometry.BevelAngleDeg;
    public double GrooveRadiusMm => Geometry.RootRadiusMm;
    public Option<double> EffectiveThroatMm(double depthMm, double partThicknessMm, WeldProcess process) =>
        Penetration == Penetration.Cjp
            ? Some(partThicknessMm)
            : Some(depthMm - (Geometry.RequiresPjpDeduction ? process.PjpDeductionMm : 0.0)).Filter(static throat => throat > 0.0);
}

public readonly record struct HoleWeld(PositiveMagnitude DiameterMm, PositiveMagnitude DepthMm) {
    const double ThickMaterialThresholdMm = 16.0;

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

    public double PartThicknessMm => Switch(
        fillet: static g => g.PartMm.Value, groove: static g => g.PartMm.Value,
        plug: static g => g.PartMm.Value, slot: static g => g.PartMm.Value,
        flareBevel: static g => g.PartMm.Value, flareV: static g => g.PartMm.Value);
}

[Union]
public abstract partial record JointRow {
    private JointRow(string designation, JointKind kind) { Designation = designation; Kind = kind; }
    public string Designation { get; }
    public JointKind Kind { get; }

    public sealed record Weld(string Designation, WeldGeometry Geometry, ElectrodeClass Electrode)
        : JointRow(Designation, JointKind.Weld) {
        public WeldType Type => Geometry.Switch(
            fillet: static _ => WeldType.Fillet,
            groove: static _ => WeldType.Groove,
            plug: static _ => WeldType.Plug,
            slot: static _ => WeldType.Slot,
            flareBevel: static _ => WeldType.FlareBevel,
            flareV: static _ => WeldType.FlareV);

        public WeldProfile Profile => new(Type.Face, Type.ReinforcementMm, Type.ToeRadiusMm, Geometry.Switch(
            groove: static geometry => geometry.Prep.Root,
            fillet: static _ => RootTreatment.AsWelded, plug: static _ => RootTreatment.AsWelded,
            slot: static _ => RootTreatment.AsWelded, flareBevel: static _ => RootTreatment.AsWelded,
            flareV: static _ => RootTreatment.AsWelded));

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

        public static double MinimumFilletLegMm(double thinnerPartMm) => thinnerPartMm switch {
            <= 6.0  => 3.0,
            <= 13.0 => 5.0,
            <= 19.0 => 6.0,
            _       => 8.0
        };

        public static double MaximumFilletLegMm(double edgeThicknessMm) =>
            edgeThicknessMm < 6.0 ? edgeThicknessMm : edgeThicknessMm - 1.6;
    }

    public sealed record Stud(string Designation, StudClass Class, StudGrade Grade, PositiveMagnitude LengthBeforeWeldMm, PositiveMagnitude SpacingMm)
        : JointRow(Designation, JointKind.Stud) {
        public double RealizedLengthMm => LengthBeforeWeldMm.Value - Class.BurnoffMm;
        public double DesignShearKn(StudGroup group) => Class.SteelShearKn(group);
        public double EnDesignShearKn => Grade.EnShearResistanceKn(Class);
        public MaterialId Substance => Grade.Substance;
    }

    public sealed record Adhesive(string Designation, AdhesiveClass Class, PositiveMagnitude BondMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm)
        : JointRow(Designation, JointKind.Adhesive) {
        public double DesignShearKn => Class.LapShearMpa * OverlapMm.Value * WidthMm.Value * 1e-3;
        public Option<double> DesignTensionKn => Class.StructuralBiteMpa.Map(strength => strength * OverlapMm.Value * WidthMm.Value * 1e-3);
        public MaterialId Substance => Class.Substance;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

// --- [TABLES] --------------------------------------------------------------------------
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

    static Fin<SectionProfile> WeldProfileOf(JointRow.Weld row, Op key) => row.Geometry.Switch(
        fillet: geometry => SectionProfile.FilletTriangle.Of(geometry.LegMm.Value, geometry.LegMm.Value, key),
        groove: geometry => GrooveProfile(geometry, key),
        plug: geometry => SectionProfile.Circle.Of(geometry.Hole.DiameterMm.Value, key),
        slot: geometry => SectionProfile.Rectangle.Of(geometry.Hole.DiameterMm.Value, geometry.LengthMm.Value, key),
        flareBevel: geometry => SectionProfile.FilletTriangle.Of(geometry.RadiusMm.Value, geometry.RadiusMm.Value, key),
        flareV: geometry => SectionProfile.FilletTriangle.Of(geometry.RadiusMm.Value, geometry.RadiusMm.Value, key));

    static Fin<SectionProfile> GrooveProfile(WeldGeometry.Groove geometry, Op key) {
        GroovePrep p = geometry.Prep;
        double flare = p.IncludedAngleDeg > 0.0 ? 2.0 * Math.Tan(p.IncludedAngleDeg * Math.PI / 360.0) : Math.Tan(p.BevelAngleDeg * Math.PI / 180.0);
        double top = p.RootOpeningMm + geometry.DepthMm.Value * flare;
        return SectionProfile.Trapezium.Of(
            bottomWidthMm: p.RootOpeningMm, topWidthMm: top, depthMm: geometry.DepthMm.Value,
            topOffsetMm: p.BevelAngleDeg > 0.0 ? (top - p.RootOpeningMm) / 2.0 : 0.0, key);
    }

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

    static Validation<Error, Unit> Coherence(JointRow row, Op key) => row.Switch(
        weld: r => AdmitWeld(r, key).ToValidation().Map(static _ => unit),
        stud: r => key.AcceptValidated<PositiveMagnitude>(candidate: r.RealizedLengthMm).ToValidation().Map(static _ => unit),
        adhesive: static _ => Validation<Error, Unit>.Success(unit));

    static Fin<SectionProfile> ProfileOf(JointRow row, Op key) => row.Switch(
        weld: r => WeldProfileOf(r, key),
        stud: r => SectionProfile.Circle.Of(diameterMm: r.Class.DiameterMm, key),
        adhesive: r => SectionProfile.Nominal.Of(nominalMm: r.BondMm.Value, key));

    static readonly FrozenDictionary<ComponentId, JointRow> Rowset =
        Roster.ToFrozenDictionary(static row => ComponentId.Create(row.Designation), static row => row);

    public static Fin<JointRow> Resolve(Component component, Op key) =>
        Rowset.TryGetValue(component.Designation, out JointRow row)
            ? Fin.Succ(row)
            : new ComponentFault.ComponentMissing(key, ProfileRef.Of(component.Designation.Value));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        Resolve(component, key).Bind(row => SectionCapacity.Lift(row.Switch<CapacityReceipt>(
            weld: r => new CapacityReceipt.Weld(component.Designation, r, placement.LoadAngleDeg),
            adhesive: r => new CapacityReceipt.Adhesive(component.Designation, r),
            stud: r => new CapacityReceipt.Stud(component.Designation, r, placement.StudGroup, placement.StudCount)), key));
}
```

## [03]-[RESEARCH]

(none)
