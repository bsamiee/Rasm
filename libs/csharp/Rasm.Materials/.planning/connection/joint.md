# [MATERIALS_JOINT]

THE FOURTH REALIZED CONNECTIONFAMILY and THE CONTINUOUS-CONNECTION VOCABULARY. The joint vocabulary — the `JointKind` weld/adhesive/stud discriminant, the `WeldType`/`AdhesiveClass`/`StudClass` axes, the `JointSection` continuous-connection receipt (throat/bond-line/stud-diameter), and the AWS D1.1 / structural-adhesive `JointRow` table — is the fourth realized `connection#CONNECTION_OWNER` `ConnectionItem` vocabulary, carried in the `ConnectionFamily.Joint` case, the metallurgical/adhesive complement to the discrete reinforcement/fastener/hanger items the axis already owns. A fillet weld is a `ConnectionItem` row, never a `Weld` type: the weld geometry, the electrode strength, and the throat are joint-`ConnectionItem` columns, and the `JointSection` projection feeds the SAME `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the SAME `ConnectionCatalogue.Build` fold the three discrete families drive — a weld schedule, a bonded curtain-wall joint, and a composite shear-stud row place through the construction layout fold over one `ConnectionItem`, never a per-joint schedule owner.

A continuous weld/bond/stud is STRUCTURALLY DISTINCT from a discrete placed item: it carries no thread or bar-diameter section, so it cannot fold into an existing family arm the way `anchor` folds into `fastener` (`connection#CONNECTION_OWNER`, `connection.md` line 12). This is the SINGLE deliberate widening of the closed-at-three axis to closed-at-four — justified because `STEEL_COMPOSITE_AND_COLDFORMED` depends on the `StudClass` vocabulary landing here and because the metallurgical/adhesive continuous-connection discipline has no representation among the discrete items. The joint vocabulary grows by data — a new electrode is one `ElectrodeClass` row, a new adhesive one `AdhesiveClass` row, a new stud one `StudClass` row, a new designation one `JointRow` catalogue entry — never a per-joint type. The bond-line/throat/stud geometry is a scalar receipt the host materializes, NEVER a host curve here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps). The `JointSection.DesignShearKn` projection emits the SPEC-NOMINAL filler/adhesive/stud-steel design shear (the `RebarSection.YieldForceKn` / `FastenerSection.ProofLoadKn` mirror — a total `double` over the section's own band), while the measured base-metal capacity is the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt the design seam reads by `MaterialId` and the composite stud's concrete-governed branch is the `Rasm.Compute` join — neither re-derived in this section. The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every length/area column, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel`/`metal.iron`) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here.

## [01]-[INDEX]

- [01]-[JOINT_FAMILY]: the `JointKind` weld/adhesive/stud discriminant, the `WeldType` (with its `LineWeld` flag)/`ElectrodeClass`/`AdhesiveClass`/`StudClass` (with its AISC `Rg`/`Rp` factors) axes, the `JointSection` continuous-connection receipt with its `NominalMm` and SPEC-NOMINAL `DesignShearKn` projections, the `ConnectionSection.Joint` arm's `NominalMm` throat/bond-line/stud-diameter dispatch, and the `ConnectionCatalogue.BuildJointRows` AWS D1.1 / structural-adhesive row table.

## [02]-[JOINT_FAMILY]

- Owner: the joint vocabulary (`JointKind` the weld/adhesive/stud discriminant, `WeldType` the AWS D1.1 fillet/groove/butt/plug geometry with its `LineWeld` per-length-resistance flag, `ElectrodeClass` the AWS A5.1 filler-metal strength axis, `AdhesiveClass` the structural-adhesive lap-shear/peel axis, `StudClass` the AWS D1.1 stud-welded shear-connector axis with its AISC 360-22 `Rg`/`Rp` group-position factors, `JointSection` the continuous-connection receipt); `ConnectionCatalogue.BuildJointRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` concatenates; the `JointSection.DesignShearKn` SPEC-NOMINAL projection emitting the weld/bond/stud-steel design shear the structural-connection-design seam reads (the `RebarSection.YieldForceKn` / `FastenerSection.ProofLoadKn` mirror — a total `double` over the section's own filler/adhesive/stud-steel band, the base-metal/concrete receipt the design seam's, never re-derived here).
- Cases: kind {`Weld` (continuous fusion over `WeldType` × `ElectrodeClass`), `Adhesive` (structural bond over `AdhesiveClass`), `Stud` (welded shear connector over `StudClass`)} — the closed continuous-connection family carried in one `JointSection` `[Union]` arm; weld {fillet / groove / plug / butt geometries} over the AWS A5.1 E60XX..E110XX electrode strength band; adhesive {epoxy / methacrylate / polyurethane / silicone-structural} over the lap-shear/peel allowable band; stud {the 1/2in..7/8in AWS D1.1 Type-B headed shear connectors} over diameter/height/spacing — a joint is a `ConnectionItem` row over one `JointKind`, never a joint subtype.
- Entry: `public double DesignShearKn` on `JointSection` — the SPEC-NOMINAL continuous-connection design shear, a TOTAL projection over the section's own filler/adhesive/stud-steel band exactly as `RebarSection.YieldForceKn` reads `RebarGrade.MinimumYieldMpa·NominalAreaMm2` and `FastenerSection.ProofLoadKn` reads `FastenerGrade.ProofStressMpa·TensileStressAreaMm2`: a line weld is the AWS D1.1 `0.6·FEXX·throat·length` (the `0.6` shear-fraction-of-tensile, the throat the `WeldType.EffectiveThroat` projection), a `LineWeld == false` plug weld the AWS D1.1 area-shear `0.6·FEXX·(π/4·d²)` over the plug-hole diameter the `LegMm` column carries (a realized branch, not a 0-returning stub), an adhesive the ASTM D1002 `LapShearMpa·overlap·width`, a stud the AISC 360-22 Eq I8-1 STEEL-side `StudClass.SteelShearKn = Rg·Rp·Asc·Fu`. There is NO `resolve`/`Func<MaterialId, …>` parameter and NO base-metal/concrete read: the `MaterialId`-keyed `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt is the design seam's, and the composite stud's `min(steel, 0.5·Asc·√(f'c·Ec))` concrete-governed branch is the `Rasm.Compute` `Profiles/capacity#SECTION_CAPACITY` join that reads the slab `f'c`/`Ec` off the seam `Material` node, never a section column here. `ConnectionCatalogue.BuildJointRows(context)` folds the AWS D1.1 / structural-adhesive `JointRow` table through `JointOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetWeldByType`/`GetStudBySize` family.
- Packages: Rasm (project — `PositiveMagnitude` for the throat/leg/bond-line/stud-diameter/area columns, never an int-backed `Dimension` that truncates a fractional millimetre throat), Thinktecture.Runtime.Extensions (`[Union]` for `JointSection` the payload-carrying continuous-connection receipt, `[SmartEnum<string>]` for the `JointKind` family discriminant and the weld/electrode/adhesive/stud axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the joint vocabulary grows by data — a new weld geometry is one `WeldType` row carrying its throat-from-leg factor, a new electrode one `ElectrodeClass` row carrying its tensile strength, a new adhesive one `AdhesiveClass` row carrying its lap-shear allowable, a new stud one `StudClass` row carrying its diameter/height/ultimate, a new designation one `JointRow` catalogue entry — never a per-joint type, never a `Weld`/`AdhesiveBond`/`ShearStud` sibling type. The `ConnectionFamily` axis is now closed-at-FOUR (reinforcement/fastener/hanger/joint), the continuous weld/bond/stud the deliberate fourth case; `anchor` stays a `FastenerKind` arm inside the fastener vocabulary, never a fifth family. A composite floor beam's shear interface references `StudClass` here (`steel#STEEL_FAMILY` `Composite` arm), the one shear-stud vocabulary, never a parallel stud owner.
- Boundary: the joint vocabulary is the fourth realized `ConnectionFamily` — a per-joint `Weld`/`ShearStud` class is the deleted form; `JointSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every dimensional column (`LegMm`/`DepthMm`/`LengthMm` for the weld, `BondLineMm`/`OverlapMm`/`WidthMm` for the adhesive, `HeightMm`/`SpacingMm` for the stud) so the section never re-mints a length primitive and a fractional weld throat (`a = 0.707·leg` for an equal-leg fillet) admits without the truncation an int-backed `Dimension` count would force, those columns the IFC realizing-element wire (the stud's `IfcMechanicalFastener`, the weld/adhesive's `IfcFastener`) and the structural-design seam read; the continuous weld/bond/stud has no thread or bar-diameter section, so the `ConnectionSection.Joint` arm projects `NominalMm` to the throat (weld), the bond-line thickness (adhesive), or the stud diameter (stud) — the one `NominalMm` dispatch the `connection#CONNECTION_OWNER` `ConnectionSection` already exposes, widened by one arm; the weld throat geometry is the scalar `WeldType.EffectiveThroat(legMm, depthMm)` projection (the AWS D1.1 `0.707·leg` fillet, the `depth` PJP-groove depth-of-preparation, the thinner-part CJP-butt throat) the host materializes into the weld bead — NEVER a host `Curve`, the host boundary lofts the weld profile from the scalar throat exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for a weld/stud, `metal.iron` for a cast joint) the row's `ConnectionItem.AppearanceId` column carries, never a joint-specific shade; the structural capacity SPLITS by source exactly as the sibling families split it — the section's `DesignShearKn` is the SPEC-NOMINAL filler/adhesive/stud-steel band (`ElectrodeClass.TensileMpa` the spec-nominal AWS A5.1 filler-metal strength, `AdhesiveClass.LapShearMpa` the ASTM D1002 allowable, `StudClass.UltimateMpa` the AWS D1.1 stud-steel ultimate), while the measured base-metal capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` read by the `CapacityKey` `MaterialId` at the design seam and is NEVER re-derived in this section (the `RebarGrade.MinimumYieldMpa` / `FastenerGrade.ProofStressMpa` discipline the siblings hold), so the section threads no `resolve` func and the composite stud's concrete-governed `0.5·Asc·√(f'c·Ec)` branch is the `Rasm.Compute` `Profiles/capacity#SECTION_CAPACITY` join, not a column here; `ConnectionCatalogue.BuildJointRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the AWS D1.1 weld / structural-adhesive / shear-stud rows keyed `connection.<designation>` (`connection.weld-fillet-8mm-e70`, `connection.stud-19mm-h100`, `connection.adhesive-epoxy-2mm`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a weld schedule or stud pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold (a stud row keyed by spacing is a station-stepped pattern, a continuous weld a single run-length placement), never a parallel joint-layout owner; the item reaches `Rasm.Bim` ONLY as the `connection#CONNECTION_PROJECTOR` `ConnectionProjector`-authored seam `Object` node plus its `Rasm_ConnectionRealization` `PropertySet` detail bag (NEVER a second `ConnectionItemWire`/`ConnectionWire` carrier — the deleted form `connection#CONNECTION_PROJECTOR` retires), the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reader recovering the scalar columns off that bag and the `Projection/semantic#IFC_EGRESS` `Emit` serializing them to the IFC 4.3 element the `connection#CONNECTION_OWNER` `ConnectionSection.IfcEntity` joint arm selects — an `IfcMechanicalFastener` (`PredefinedType` `STUDSHEARCONNECTOR`) for the welded shear stud, an `IfcFastener` (`IfcFastenerTypeEnum` `WELD` for the weld bead, `GLUE` for the adhesive bead) for the non-mechanical continuous weld/bond — plus an `IfcRelConnectsWithRealizingElements` (the seam `Relations/relation#EDGE_ALGEBRA` `Connect(ConnectKind.Realizing)` edge the app/Bim authors once both member ids are known) for the weld/stud realizing the structural connection — portable scalar data here, never an interior `IfcOpenShell` evaluation; the joint `FastenerType` detail-bag token is the verified `IfcMechanicalFastenerTypeEnum` member name (`STUDSHEARCONNECTOR`/`SHEARCONNECTOR` for the welded stud, `BOLT`/`ANCHORBOLT` for a discrete fastener), never the bare reinforcing-bar `STUD` enum the cast-in bar carries.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;     // FrozenDictionary (the registered-row table)
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude — the throat/leg/bond-line/stud-diameter columns) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ConnectionItem AppearanceId/CapacityKey reference)
using Rasm.Materials.Connection;     // ConnectionFamily/ConnectionId/ConnectionItem/ConnectionSection/ConnectionFault (the parent CONNECTION_OWNER)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Connection family page is its OWN Rasm.Materials.Connection.<Family> sub-namespace so the four sibling
// `ConnectionCatalogue` static classes are distinct types (a shared Rasm.Materials.Connection would be a CS0101 collision
// with connection.md's own `ConnectionCatalogue`); connection#CONNECTION_OWNER stays the parent Rasm.Materials.Connection
// and folds Joint.ConnectionCatalogue.BuildJointRows by the sub-namespace-qualified name. This page DEFINES StudClass the
// Profiles steel#STEEL_FAMILY composite leg imports as Rasm.Materials.Connection.Joint; parent types via the using above.
namespace Rasm.Materials.Connection.Joint;

// --- [TYPES] -------------------------------------------------------------------------------
// The continuous-connection family discriminant the JointRow carries and the JointSection [Union] mirrors; a value
// discriminant (JointRow.Kind, the generated total Switch over weld/adhesive/stud), NEVER a [Union] (the cases carry
// no payload here — the payload lives on the JointSection arm). IfcPredefinedType is the ONE per-kind predefined token
// (the FastenerKind.IfcPredefinedType / RebarUsage.IfcPredefinedType mirror) the connection#CONNECTION_OWNER
// ConnectionSection.PredefinedToken joint arm READS (never re-hardcoded there) and the ConnectionProjector lands on the
// seam Object node's PredefinedType + the Rasm_ConnectionRealization detail bag's FastenerType row: a welded stud is the
// IfcMechanicalFastenerTypeEnum STUDSHEARCONNECTOR, a weld bead the IfcFastenerTypeEnum WELD, an adhesive bead the
// IfcFastenerTypeEnum GLUE (the C6 token the Bim Emit gate resolves + validates against the frozen valid-set — STUD is
// the IfcReinforcingBarTypeEnum cast-in bar, never a fastener value, the [IFC_JOINT_WIRE] distinction).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointKind {
    public static readonly JointKind Weld     = new("weld",     ifcPredefinedType: "WELD");
    public static readonly JointKind Adhesive = new("adhesive", ifcPredefinedType: "GLUE");
    public static readonly JointKind Stud     = new("stud",     ifcPredefinedType: "STUDSHEARCONNECTOR");
    public string IfcPredefinedType { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeldType {
    // EffectiveThroat: AWS D1.1 Table 2.x — fillet throat is 0.707·leg (equal-leg), PJP groove throat is the depth of
    // preparation, CJP butt throat is the thinner connected part (depthMm carries the thickness). A plug/slot weld carries
    // NO line throat: it resists on the faying-plane HOLE AREA (π/4·LegMm², the LegMm column carrying the plug-hole
    // diameter), so its EffectiveThroat is unused and DesignShearKn takes the area-shear branch. LineWeld flags WHICH
    // design-shear branch a kind takes — fillet/groove/butt the throat×length line-shear, a plug the hole-area shear.
    public static readonly WeldType Fillet = new("fillet", lineWeld: true,  static (leg, depth) => 0.707 * leg);
    public static readonly WeldType Groove = new("groove", lineWeld: true,  static (_, depth) => depth);
    public static readonly WeldType Butt   = new("butt",   lineWeld: true,  static (_, depth) => depth);
    public static readonly WeldType Plug   = new("plug",   lineWeld: false, static (_, depth) => depth);
    public bool LineWeld { get; }

    [UseDelegateFromConstructor]
    public partial double EffectiveThroat(double legMm, double depthMm);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElectrodeClass {
    // AWS A5.1 carbon-steel covered electrodes; TensileMpa is the FEXX minimum filler-metal tensile strength.
    public static readonly ElectrodeClass E60  = new("e60",  tensileMpa: 415.0, appearanceId: "metal.iron");
    public static readonly ElectrodeClass E70  = new("e70",  tensileMpa: 485.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E80  = new("e80",  tensileMpa: 550.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E90  = new("e90",  tensileMpa: 620.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E100 = new("e100", tensileMpa: 690.0, appearanceId: "metal.steel");
    public static readonly ElectrodeClass E110 = new("e110", tensileMpa: 760.0, appearanceId: "metal.steel");
    public double TensileMpa { get; }
    public string AppearanceId { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdhesiveClass {
    // Structural-adhesive lap-shear (ASTM D1002) and T-peel (ASTM D1876) allowables.
    public static readonly AdhesiveClass Epoxy        = new("epoxy",        lapShearMpa: 30.0, peelNmm: 5.0,  serviceCelsius: 80.0);
    public static readonly AdhesiveClass Methacrylate = new("methacrylate", lapShearMpa: 25.0, peelNmm: 12.0, serviceCelsius: 100.0);
    public static readonly AdhesiveClass Polyurethane = new("polyurethane", lapShearMpa: 15.0, peelNmm: 20.0, serviceCelsius: 90.0);
    public static readonly AdhesiveClass SiliconeStructural = new("silicone-structural", lapShearMpa: 1.0, peelNmm: 8.0, serviceCelsius: 150.0);
    public double LapShearMpa { get; }
    public double PeelNmm { get; }
    public double ServiceCelsius { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StudClass {
    // AWS D1.1 Type-B headed shear connectors (composite construction); diameter / standard height / ultimate strength.
    public static readonly StudClass S13 = new("stud-1/2",  diameterMm: 12.7,  standardHeightMm: 75.0,  ultimateMpa: 450.0);
    public static readonly StudClass S16 = new("stud-5/8",  diameterMm: 15.9,  standardHeightMm: 100.0, ultimateMpa: 450.0);
    public static readonly StudClass S19 = new("stud-3/4",  diameterMm: 19.1,  standardHeightMm: 100.0, ultimateMpa: 450.0);
    public static readonly StudClass S22 = new("stud-7/8",  diameterMm: 22.2,  standardHeightMm: 125.0, ultimateMpa: 450.0);
    public double DiameterMm { get; }
    public double StandardHeightMm { get; }
    public double UltimateMpa { get; }
    public double AreaMm2 => Math.PI * 0.25 * DiameterMm * DiameterMm;
    // AISC 360-22 Eq I8-1 steel-side strength is Rg·Rp·Asc·Fu — the group/position factors cap the concrete-side
    // 0.5·Asc·√(f'c·Ec) the Rasm.Compute composite-section seam computes. The Rg·Rp product is the conservative
    // ONE-stud-per-rib / strong-position default (Rg=1.0, Rp=0.75 → 0.75); a weak-position or multi-stud-per-rib
    // layout is a Rasm.Compute placement input, NOT a stud-vocabulary edit, so the cap a schedule row reports is the
    // strong-position upper bound the design seam tightens by the realized deck/rib geometry.
    public double GroupFactorRg => 1.0;
    public double PositionFactorRp => 0.75;
    public double SteelShearKn => GroupFactorRg * PositionFactorRp * AreaMm2 * UltimateMpa * 1e-3;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One continuous-connection receipt across all three joint kinds, the connection#CONNECTION_OWNER
// ConnectionSection.Joint arm carries; NominalMm dispatches throat (weld), bond-line (adhesive), diameter (stud).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointSection {
    private JointSection() { }

    public sealed record Weld(WeldType Type, ElectrodeClass Electrode, PositiveMagnitude LegMm, PositiveMagnitude DepthMm, PositiveMagnitude LengthMm) : JointSection;
    public sealed record Adhesive(AdhesiveClass Class, PositiveMagnitude BondLineMm, PositiveMagnitude OverlapMm, PositiveMagnitude WidthMm) : JointSection;
    public sealed record Stud(StudClass Class, PositiveMagnitude HeightMm, PositiveMagnitude SpacingMm) : JointSection;

    public JointKind Kind => Switch<JointKind>(
        weld:     static _ => JointKind.Weld,
        adhesive: static _ => JointKind.Adhesive,
        stud:     static _ => JointKind.Stud);

    public PositiveMagnitude NominalMm => Switch(
        weld:     static w => PositiveMagnitude.Create(w.Type.EffectiveThroat(w.LegMm.Value, w.DepthMm.Value)),
        adhesive: static a => a.BondLineMm,
        stud:     static s => PositiveMagnitude.Create(s.Class.DiameterMm));

    // The FINISH the appearance projection reads (the electrode appearance for a weld, the adhesive polymer for a bond,
    // steel for a stud), INDEPENDENT from CapacityKey by the connection#CONNECTION_OWNER two-slot law.
    public MaterialId AppearanceId => Switch(
        weld:     static w => MaterialId.Of(w.Electrode.AppearanceId),
        adhesive: static _ => MaterialId.Of("polymer.adhesive"),
        stud:     static _ => MaterialId.Of("metal.steel"));

    // The CAPACITY material whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads — the joint's
    // base-metal STEEL (metal.steel; the spec-nominal filler/adhesive/stud-steel band rides DesignShearKn, the measured
    // base-metal capacity the Mechanical receipt), sourced INDEPENDENTLY from the AppearanceId finish so a bonded joint
    // whose appearance is the adhesive polymer still reads its steel base-metal capacity.
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    // SPEC-NOMINAL continuous-connection design shear — the RebarSection.YieldForceKn / FastenerSection.ProofLoadKn
    // mirror: a TOTAL double projection over the section's OWN filler/adhesive/stud-steel spec band, NEVER a base-metal
    // re-derivation (the MaterialId-keyed properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt is the design seam's,
    // read once at the federation, never threaded into the section). A LINE weld (fillet/groove/butt) is the AWS D1.1
    // 0.6·FEXX·throat·length (the 0.6 shear-fraction-of-tensile factor over the effective throat × the run length); a PLUG
    // weld carries NO per-length throat — it resists on its faying-plane HOLE AREA, the AWS D1.1 0.6·FEXX·(π/4·d²) where
    // d is the plug-hole diameter the LegMm column carries (a realized area-shear branch, NOT a 0-returning stub: LineWeld
    // is the branch discriminant, not a capability gate). An adhesive is the ASTM D1002 lap-shear·overlap·width allowable.
    // A stud is the AISC 360-22 Eq I8-1 STEEL-side cap Rg·Rp·Asc·Fu (the StudClass.SteelShearKn) — the concrete-side
    // 0.5·Asc·√(f'c·Ec) governs only when the slab concrete is weak, and that term needs the composite slab's f'c/Ec off
    // the seam graph, so it is the Rasm.Compute composite-section seam's min(steel, concrete) join
    // (Profiles/capacity#SECTION_CAPACITY), NOT a section column here.
    public double DesignShearKn => Switch(
        weld:     static w => w.Type.LineWeld
                      ? 0.6 * w.Electrode.TensileMpa * w.Type.EffectiveThroat(w.LegMm.Value, w.DepthMm.Value) * w.LengthMm.Value * 1e-3
                      : 0.6 * w.Electrode.TensileMpa * (Math.PI * 0.25 * w.LegMm.Value * w.LegMm.Value) * 1e-3,
        adhesive: static a => a.Class.LapShearMpa * a.OverlapMm.Value * a.WidthMm.Value * 1e-3,
        stud:     static s => s.Class.SteelShearKn);
}

public readonly record struct JointRow(JointKind Kind, string Designation, string Type, double AMm, double BMm, double CMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    // AWS D1.1 weld / structural-adhesive / shear-stud seed rows; A=primary dim, B=secondary, C=run-length/overlap.
    static readonly Seq<JointRow> JointRows = Seq(
        new JointRow(JointKind.Weld,     "connection.weld-fillet-6mm-e70",  "fillet:e70",  6.0,  6.0,  100.0),
        new JointRow(JointKind.Weld,     "connection.weld-fillet-8mm-e70",  "fillet:e70",  8.0,  8.0,  150.0),
        new JointRow(JointKind.Weld,     "connection.weld-fillet-10mm-e80", "fillet:e80",  10.0, 10.0, 200.0),
        new JointRow(JointKind.Weld,     "connection.weld-groove-12mm-e80", "groove:e80",  12.0, 12.0, 250.0),
        new JointRow(JointKind.Weld,     "connection.weld-butt-cjp-e90",    "butt:e90",    16.0, 16.0, 300.0),
        new JointRow(JointKind.Weld,     "connection.weld-plug-20mm-e70",   "plug:e70",    20.0, 8.0,  20.0),
        new JointRow(JointKind.Stud,     "connection.stud-13mm-h75",        "stud-1/2",    13.0, 75.0,  150.0),
        new JointRow(JointKind.Stud,     "connection.stud-19mm-h100",       "stud-3/4",    19.0, 100.0, 200.0),
        new JointRow(JointKind.Stud,     "connection.stud-22mm-h125",       "stud-7/8",    22.0, 125.0, 250.0),
        new JointRow(JointKind.Adhesive, "connection.adhesive-epoxy-2mm",   "epoxy",        2.0,  25.0,  50.0),
        new JointRow(JointKind.Adhesive, "connection.adhesive-mma-1mm",     "methacrylate", 1.0,  20.0,  40.0));

    static Fin<(ConnectionId Id, ConnectionItem Item)> JointOf(JointRow r, Context context, Op key) =>
        from section in r.Kind.Switch(
            weld:     _ => WeldOf(r, key),
            adhesive: _ => AdhesiveOf(r, key),
            stud:     _ => StudOf(r, key))
        from item in ConnectionItem.Of(ConnectionFamily.Joint, r.Designation, new ConnectionSection.Joint(section), section.CapacityKey, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    static Fin<JointSection> WeldOf(JointRow r, Op key) =>
        from type in r.Type.Split(':') is [var t, _] && WeldType.TryGet(t, out WeldType? wt) ? Fin.Succ(wt!) : Fin.Fail<WeldType>(ConnectionFault.Designation(key, $"<unknown-weld-type:{r.Type}>"))
        from electrode in r.Type.Split(':') is [_, var e] && ElectrodeClass.TryGet(e, out ElectrodeClass? ec) ? Fin.Succ(ec!) : Fin.Fail<ElectrodeClass>(ConnectionFault.Grade(key, $"<unknown-electrode:{r.Type}>"))
        from leg in key.AcceptValidated<PositiveMagnitude>(candidate: r.AMm)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Weld(type, electrode, leg, depth, length);

    static Fin<JointSection> AdhesiveOf(JointRow r, Op key) =>
        from cls in AdhesiveClass.TryGet(r.Type, out AdhesiveClass? a) ? Fin.Succ(a!) : Fin.Fail<AdhesiveClass>(ConnectionFault.Grade(key, $"<unknown-adhesive:{r.Type}>"))
        from bond in key.AcceptValidated<PositiveMagnitude>(candidate: r.AMm)
        from overlap in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Adhesive(cls, bond, overlap, width);

    static Fin<JointSection> StudOf(JointRow r, Op key) =>
        from cls in StudClass.TryGet(r.Type, out StudClass? s) ? Fin.Succ(s!) : Fin.Fail<StudClass>(ConnectionFault.Designation(key, $"<unknown-stud-class:{r.Type}>"))
        from height in key.AcceptValidated<PositiveMagnitude>(candidate: r.BMm)
        from spacing in key.AcceptValidated<PositiveMagnitude>(candidate: r.CMm)
        select (JointSection)new JointSection.Stud(cls, height, spacing);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildJointRows(Context context) =>
        JointRows
            .Choose(row => JointOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [AWS_D11_WELD_TABLE]: REALIZED — the AWS D1.1 structural-welding geometry seeds through `ConnectionCatalogue.BuildJointRows(context)` over the `JointRow` kind/designation/type table, the leg/depth/length columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object. The `WeldType.EffectiveThroat` factor is the AWS D1.1 throat rule (`0.707·leg` for an equal-leg fillet, the depth of preparation for a PJP groove, the thinner connected part for a CJP butt); the plug/slot weld carries the `LineWeld == false` flag because it resists on its faying-plane HOLE AREA, not a linear throat × length, so `DesignShearKn` takes the realized area-shear branch `0.6·FEXX·(π/4·d²)` over the plug-hole diameter the `LegMm` column carries (the seeded `connection.weld-plug-20mm-e70` row exercises it) rather than a line-shear or a 0-returning stub. The `ElectrodeClass.TensileMpa` is the AWS A5.1 minimum filler-metal tensile (E60XX=415, E70XX=485, E80XX=550, E90XX=620, E100XX=690, E110XX=760 MPa). The line-weld design shear is the AWS D1.1 `0.6·FEXX·throat·length`, the `0.6` the shear-fraction-of-tensile factor, a spec-nominal `double` projection over the section's own `ElectrodeClass` band. The matching base-metal capacity is the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrength` `MeasureValue` read by `CapacityKey` at the design seam, never duplicated as a weld column and never read inside `DesignShearKn`.
- [STRUCTURAL_ADHESIVE_ALLOWABLE]: REALIZED — the `AdhesiveClass` lap-shear (ASTM D1002 single-lap-joint) and T-peel (ASTM D1876) allowables seed the structural-adhesive rows; the bonded-joint design capacity is `LapShearMpa·overlap·width`, the overlap length the shear-transfer area a lap joint develops. Epoxy (30 MPa lap-shear, 80 °C service), methacrylate/MMA (25 MPa, high peel, 100 °C), polyurethane (15 MPa, very high peel/flexibility), and structural silicone (1 MPa, the SSG curtain-wall sealant the highest service temperature) are the seed; a measured manufacturer adhesive lands one `AdhesiveClass` row with its datasheet lap-shear/peel/service columns. The bonded curtain-wall joint the structural-silicone case admits keys and serializes the way a bolt does.
- [AWS_D11_SHEAR_STUD]: REALIZED — the AWS D1.1 Type-B headed shear connectors (the composite-construction welded studs) seed the `StudClass` axis at the standard 1/2in..7/8in diameters, the standard heights, the 450 MPa minimum ultimate tensile, and the AISC 360-22 `Rg`/`Rp` group-position factors. The section's `DesignShearKn` projection emits the STEEL-side strength `StudClass.SteelShearKn = Rg·Rp·Asc·Fu` (the spec-nominal upper bound, the conservative `Rg=1.0`/`Rp=0.75` strong-position default a `Rasm.Compute` deck/rib placement input tightens) — it does NOT read concrete off any property set, holding the same spec-nominal discipline as `RebarSection.YieldForceKn` and `FastenerSection.ProofLoadKn`. The full composite shear governs at `min(steel, concrete)`: the concrete-side AISC 360-22 Eq I8-1 `0.5·Asc·√(f'c·Ec)` term needs the COMPOSITE SLAB's `f'c`/`Ec`, which live on the slab `Material` node, so that join is the `Rasm.Compute` `Profiles/capacity#SECTION_CAPACITY` concern reading the seam graph — NOT a `StudClass` column and NOT a base-metal read threaded into this section. This is the `StudClass` vocabulary the `steel#STEEL_FAMILY` `Composite` arm references for the steel-concrete shear interface (the `STEEL_COMPOSITE_AND_COLDFORMED` task), the one shear-stud owner both pages read — a composite floor beam's stud row keys `connection.stud-19mm-h100`, this page reports the stud-steel cap, and `Rasm.Compute` computes the governing `Qn` per stud against the slab concrete, never a re-minted stud type.
- [IFC_JOINT_WIRE]: the joint family round-trips to IFC 4.3 split by the `connection#CONNECTION_OWNER` `ConnectionSection.IfcEntity` joint arm — the welded shear stud as an `IfcMechanicalFastener` (`PredefinedType` `STUDSHEARCONNECTOR`; `STUD` is the `IfcReinforcingBarTypeEnum` cast-in bar, NOT an `IfcMechanicalFastenerTypeEnum` value, so the welded stud is `STUDSHEARCONNECTOR` on the fastener wire, not `STUD`), the continuous weld bead and the structural-adhesive bead as an `IfcFastener` (the non-mechanical sibling; `IfcFastenerTypeEnum` `WELD` for the weld, `GLUE` for the adhesive — the assay-verified `{NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}` set). The stud carries its diameter and run-length on the associated `IfcMaterialProfileSetUsage` circle-profile cross-section (the verified GeometryGym round-trip channel — `IfcMechanicalFastener.mNominalDiameter`/`mNominalLength` are `internal` fields with no public getter on the occurrence OR its type, so the wire never writes them directly; `IfcFastener` exposes only its `PredefinedType` publicly, so the weld/adhesive bead carries its throat/bond-line in the `Rasm_ConnectionRealization` detail bag, never on a public scalar); each realizes the structural connection through an `IfcRelConnectsWithRealizingElements` (the seam `Relations/relation#EDGE_ALGEBRA` `Connect(ConnectKind.Realizing)` edge the app/Bim authors once both member ids are known) relating the realizing weld/stud/adhesive element to the connected structural members. The wire mapping is the `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` the electrode/adhesive/stud asserts), host-neutral here — this page emits the scalar columns the `connection#CONNECTION_PROJECTOR` `ConnectionProjector` lowers into the `Rasm_ConnectionRealization` detail bag, never an IFC entity and never a second carrier. Ripple counterpart: `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` (the `ConnectionProjection.Detail` reader of the seam-projected connection nodes + the `Projection/semantic#IFC_EGRESS` `Emit` round-trip).
