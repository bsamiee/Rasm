# [BIM_CONNECTION]

The host-neutral structural-connection sub-domain: one `ConnectionDetail` record carrying the physical joint the `Model/structural#ANALYSIS_MODEL` `MemberConnection` edge names only abstractly — a closed `ConnectionRealization` `[Union]` (`Bolted`/`Welded`/`Bearing`/`Cast`) over the `IfcRelConnectsWithRealizingElements` realizing-element set and the realizing `IfcMechanicalFastener`/`IfcFastener`/`IfcReinforcingElement` family, a `BoltPattern` `[ComplexValueObject]` (grade/diameter/gauge grid/edge distance), a `WeldSchedule` `[ComplexValueObject]` (process/throat/leg/intermittent pitch), a `Clearance` envelope, and the `ConnectionProjection.Project` fold from the GeometryGym realizing-element surface — joined to the analytical graph by the `MemberConnection` `GlobalId` pair and to the physical `BimElement` by `GlobalId`, with a `(GeometryKey, DetailKey)` content-key identity the fabrication consumer reads. The connection is the host-neutral counterpart to the abstract `MemberConnection` edge: the analytical model names WHICH members meet, the connection detail names HOW they join — bolt patterns, weld schedules, bearing surfaces, and clearance envelopes — so the fabrication and structural consumers both read one typed detail. The connection is HOST-NEUTRAL — the realizing-element geometry binds the kernel `Rasm` geometry by reference like every other Bim owner, never re-tessellated — and is the `Model <- csharp:Rasm.Materials/Connection` `ConnectionItem` IFC-wire seam owner on the Bim side: `Rasm.Materials` defers the IFC 4.3 serialization of its `ConnectionItem` axis to this owner as portable scalar/handle data, the Bim side mapping the `ConnectionItem` `Family`/`Section` columns onto the `IfcReinforcingBar`/`IfcMechanicalFastener` structural elements and the new joint weld/stud onto `IfcMechanicalFastener` + `IfcRelConnectsWithRealizingElements`. A connection rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[CONNECTION_DETAIL]: `ConnectionDetail` record, the `ConnectionRealization` `[Union]` (`Bolted`/`Welded`/`Bearing`/`Cast`), the `BoltPattern`/`WeldSchedule`/`BearingSurface` `[ComplexValueObject]` rows, the `ConnectionKind` `[SmartEnum<string>]`, and the `ConnectionProjection.Project`/`ProjectAll` fold from `IfcRelConnectsWithRealizingElements`.
- [02]-[CONNECTION_WIRE]: the `Rasm.Materials` `ConnectionItem` axis IFC 4.3 serialization — the `ConnectionItemWire` carrier and `ConnectionWire.Author` realizing-element `IfcReinforcingBar`/`IfcMechanicalFastener` + `IfcRelConnectsWithRealizingElements` egress, the connection-item egress this owner keeps until a future Connection `IElementProjection` subsumes it; the material/composition/property/classification egress is RETIRED here (the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers are gone) — it reads the projected seam `Material` subgraph through `Semantics/composition`/`classification` composed by `Projection/semantic#IFC_EGRESS` `Emit` (appearance reconciles at the content-key through `Semantics/appearance`).

## [02]-[CONNECTION_DETAIL]

- Owner: `ConnectionDetail` the single host-neutral connection record carrying the `ConnectionKind` discriminant, the connected `(MemberGlobalId, MemberGlobalId)` pair the `Model/structural#ANALYSIS_MODEL` `MemberConnection` edge names, the realizing-element `GlobalId` set, the typed `ConnectionRealization`, the `Clearance` envelope, and the `(GeometryKey, DetailKey)` content-key identity the fabrication consumer reads; `ConnectionRealization` the closed `[Union]` discriminating the four physical joint modalities — `Bolted` (the `BoltPattern` over the `IfcMechanicalFastener` `BOLT`/`ANCHORBOLT` realizing set), `Welded` (the `WeldSchedule` over the `IfcMechanicalFastener` `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` weld/stud realizing set), `Bearing` (the `BearingSurface` over a bearing realizing element), `Cast` (the `IfcReinforcingBar`/`IfcReinforcingMesh` cast-in reinforcing realizing set) — each binding its realizing-element `GlobalId` set; `BoltPattern` the `[ComplexValueObject]` (grade/nominal diameter/gauge grid rows/edge distance), `WeldSchedule` the `[ComplexValueObject]` (process/throat/leg/intermittent pitch), `BearingSurface` the `[ComplexValueObject]` (area/material/restraint); `ConnectionKind` the `[SmartEnum<string>]` over the joint discipline; `ConnectionProjection` the static fold over the GeometryGym `IfcRelConnectsWithRealizingElements` surface.
- Cases: `ConnectionRealization` arms `Bolted` (the `BoltPattern` plus the `Seq<string>` fastener realizing GlobalIds) · `Welded` (the `WeldSchedule` plus the stud/weld realizing GlobalIds) · `Bearing` (the `BearingSurface` plus the bearing realizing GlobalIds) · `Cast` (the `Seq<string>` reinforcing-bar/mesh realizing GlobalIds plus the cover/lap rows) (4) — a guessed fifth modality folds onto the nearest arm through the `ConnectionRealization.Of` resolver reading the realizing `IfcElement` runtime type; the `BoltPattern` carries `Grade` (the bolt grade string, e.g. `8.8`/`A325`), `NominalDiameter` (a kernel-SI scalar), the `Seq<(double Gauge, double Pitch)>` grid rows, and `EdgeDistance`; the `WeldSchedule` carries `Process` (e.g. `FW`/`PJP`/`CJP`), `Throat`, `Leg`, and `IntermittentPitch` (zero for a continuous weld); the `BearingSurface` carries `Area`, `Material`, and the `Restraint` degrees-of-freedom flags.
- Entry: `ConnectionProjection.Project(IfcRelConnectsWithRealizingElements rel, BimModel federated)` folds one GeometryGym realizing-element relationship into one `ConnectionDetail` — reading the `RelatingElement`/`RelatedElement` `IfcElement` pair onto the connected member GlobalIds, the `RealizingElements` `SET<IfcElement>` onto the realizing-element GlobalId set, discriminating the realizing-element runtime family onto the `ConnectionRealization` arm (an `IfcMechanicalFastener` `BOLT` realizing set folds onto `Bolted` reading the `NominalDiameter`/`NominalLength` onto the `BoltPattern`, a `STUDSHEARCONNECTOR` onto `Welded`, an `IfcReinforcingBar`/`IfcReinforcingMesh` onto `Cast` reading the `NominalDiameter`/`CrossSectionArea`/`BarLength`), and deriving the `(GeometryKey, DetailKey)` identity — `Fin<T>` aborts on a realizing element the federated model never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`; `ConnectionProjection.ProjectAll(DatabaseIfc db, BimModel federated)` lifts every `IfcRelConnectsWithRealizingElements` the database carries onto the `Seq<ConnectionDetail>` the fabrication consumer and the `[02]-[CONNECTION_WIRE]` author read.
- Auto: `Project` reads the `IfcRelConnectsWithRealizingElements` runtime graph — the `RealizingOf` projection materializes the `RealizingElements` set once, the `RealizationOf` fold discriminates the realizing-element runtime type (the dominant family selecting the arm: a fastener-majority realizing set is `Bolted`/`Welded` by the `IfcMechanicalFastenerTypeEnum` `PredefinedType` partition — `BOLT`/`ANCHORBOLT`/`DOWEL`/`SCREW`/`RIVET`/`NAIL` onto `Bolted`, `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` onto `Welded` — a reinforcing-majority set is `Cast`, a bearing realizing element is `Bearing`), each arm reading its realizing-element scalar members (the fastener `BoltPattern`/`WeldSchedule` diameter through the fastener's associated `IfcMaterialProfileSetUsage` circle-profile radius — the public channel for the GeometryGym-internal `IfcMechanicalFastener` nominal scalars — and the public `IfcReinforcingBar.NominalDiameter`/`CrossSectionArea`/`BarLength` onto the `Cast` cover/lap rows) as host-neutral scalar data, never re-tessellating the fastener geometry; the `ConnectionDetail.Identity` derives the `(GeometryKey, DetailKey)` `UInt128` pair — `GeometryKey` over the realizing-element GlobalIds through `XxHash128.HashToUInt128` and `DetailKey` over the typed realization scalars so the fabrication consumer re-reads only a changed detail; the `BindFederated` fold confirms each realizing-element and connected-member GlobalId resolves against the `Model/elements#ELEMENT_MODEL` `BimModel` index so a dangling realizing element lowers `BimFault.DanglingReference`.
- Receipt: the `Seq<ConnectionDetail>` is the connection evidence the `csharp:Rasm.Fabrication` connection-detailing consumer reads by the `(GeometryKey, DetailKey)` reference (the bolt pattern, the weld schedule, the bearing surface, and the cast reinforcing each binding the realizing-element set), the `Model/structural#ANALYSIS_MODEL` `MemberConnection` edge resolves its physical joint by the connected-member GlobalId pair, and the `[02]-[CONNECTION_WIRE]` IFC author serializes the realizing elements to IFC 4.3; a steel bolted moment connection, a precast bearing, and a cast-in reinforcing lap each carry their full physical detail on one host-neutral record.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new joint modality is one `ConnectionRealization` union arm reading the next realizing-element family; a new bolt/weld/bearing parameter is one column on the existing `[ComplexValueObject]`; a new realizing-element type is one runtime-type case the `RealizationOf` fold reads; never a per-joint-type connection record, never a second connection store, and never a re-tessellation of the realizing fastener.
- Boundary: `ConnectionDetail` is ONE record discriminated by the `ConnectionRealization` union — a `BoltedConnection`/`WeldedConnection`/`BearingConnection`/`CastConnection` class family or four sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the connection detail stays host-neutral scalar/handle data and never re-tessellates the fastener — a RhinoCommon `Brep`/`Mesh` realizing-element field is the named seam violation, the realizing-element geometry binds the kernel `Rasm` geometry by reference; the GeometryGym `IfcRelConnectsWithRealizingElements.RealizingElements` (`SET<IfcElement>`), `IfcMechanicalFastener.PredefinedType` (`IfcMechanicalFastenerTypeEnum`, the only public scalar — the nominal diameter rides the associated `IfcMaterialProfileSetUsage` circle-profile radius), the public `IfcReinforcingBar.NominalDiameter`/`CrossSectionArea`/`BarLength`/`PredefinedType`, and `IfcReinforcingMesh.MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter` member spellings are consumed as settled vocabulary through the realizing-element discrimination and a hand-rolled connection reader is the deleted form; the connection joins the structural graph by the `Model/structural#ANALYSIS_MODEL` `MemberConnection` `GlobalId` pair and the physical `BimElement` by `GlobalId`, never a second member-selection surface; the `(GeometryKey, DetailKey)` content-key identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and the fabrication consumer reads the detail by that reference; a connection rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class ConnectionKind {
    public static readonly ConnectionKind Bolted  = new("BOLTED");
    public static readonly ConnectionKind Welded  = new("WELDED");
    public static readonly ConnectionKind Bearing = new("BEARING");
    public static readonly ConnectionKind Cast    = new("CAST");
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class BoltPattern {
    public string Grade { get; }
    public double NominalDiameter { get; }
    public Seq<(double Gauge, double Pitch)> Grid { get; }
    public double EdgeDistance { get; }
}

[ComplexValueObject]
public sealed partial class WeldSchedule {
    public string Process { get; }
    public double Throat { get; }
    public double Leg { get; }
    public double IntermittentPitch { get; }
}

[ComplexValueObject]
public sealed partial class BearingSurface {
    public double Area { get; }
    public string Material { get; }
    public byte Restraint { get; }
}

[Union]
public partial record ConnectionRealization {
    partial record Bolted(BoltPattern Pattern, Seq<string> FastenerGlobalIds);
    partial record Welded(WeldSchedule Schedule, Seq<string> WeldGlobalIds);
    partial record Bearing(BearingSurface Surface, Seq<string> BearingGlobalIds);
    partial record Cast(Seq<string> ReinforcingGlobalIds, double Cover, double LapLength);

    public Seq<string> RealizingGlobalIds => Switch(
        bolted:  static r => r.FastenerGlobalIds,
        welded:  static r => r.WeldGlobalIds,
        bearing: static r => r.BearingGlobalIds,
        cast:    static r => r.ReinforcingGlobalIds);

    public ConnectionKind Kind => Switch(
        bolted:  static _ => ConnectionKind.Bolted,
        welded:  static _ => ConnectionKind.Welded,
        bearing: static _ => ConnectionKind.Bearing,
        cast:    static _ => ConnectionKind.Cast);
}

public readonly record struct Clearance(double Minimum, double Maintenance);

public sealed record ConnectionDetail(
    string GlobalId,
    string FirstMemberGlobalId,
    string SecondMemberGlobalId,
    ConnectionRealization Realization,
    Clearance Clearance) {
    public ConnectionKind Kind => Realization.Kind;

    public (UInt128 GeometryKey, UInt128 DetailKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join(",", Realization.RealizingGlobalIds.Order()))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"{Kind.Key}:{FirstMemberGlobalId}>{SecondMemberGlobalId}")));

    public Fin<ConnectionDetail> BindFederated(BimModel federated) {
        var index = toHashSet(federated.Elements.Map(static e => e.GlobalId));
        return Realization.RealizingGlobalIds.Append(Seq(FirstMemberGlobalId, SecondMemberGlobalId))
            .Filter(static id => id.Length > 0)
            .Find(id => !index.Contains(id))
            .Match(
                Some: id => FinFail<ConnectionDetail>(new BimFault.DanglingReference($"connection-realizing-absent:{GlobalId}:{id}").ToError()),
                None: () => FinSucc(this));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ConnectionProjection {
    public static Fin<ConnectionDetail> Project(IfcRelConnectsWithRealizingElements rel, BimModel federated) =>
        new ConnectionDetail(
            rel.GlobalId,
            rel.RelatingElement?.GlobalId ?? "",
            rel.RelatedElement?.GlobalId ?? "",
            RealizationOf(rel.RealizingElements.AsIterable().ToSeq()),
            new Clearance(0d, 0d))
        .BindFederated(federated);

    public static Fin<Seq<ConnectionDetail>> ProjectAll(DatabaseIfc db, BimModel federated) =>
        db.Project.Extract<IfcRelConnectsWithRealizingElements>()
            .AsIterable().ToSeq()
            .TraverseM(rel => Project(rel, federated)).As();

    static ConnectionRealization RealizationOf(Seq<IfcElement> realizing) {
        var fasteners = realizing.Choose(static e => e is IfcMechanicalFastener f ? Some(f) : None);
        var reinforcing = realizing.Filter(static e => e is IfcReinforcingBar or IfcReinforcingMesh);
        return reinforcing.Count >= fasteners.Count && reinforcing.IsEmpty == false
            ? new ConnectionRealization.Cast(reinforcing.Map(static e => e.GlobalId), 0d, LapOf(reinforcing))
            : fasteners.Exists(IsWeld)
                ? new ConnectionRealization.Welded(WeldOf(fasteners), fasteners.Map(static f => f.GlobalId))
                : new ConnectionRealization.Bolted(BoltOf(fasteners), fasteners.Map(static f => f.GlobalId));
    }

    static bool IsWeld(IfcMechanicalFastener fastener) =>
        fastener.PredefinedType is IfcMechanicalFastenerTypeEnum.STUDSHEARCONNECTOR or IfcMechanicalFastenerTypeEnum.SHEARCONNECTOR;

    // IfcMechanicalFastener.NominalDiameter/NominalLength are GeometryGym-internal (no public getter on the
    // occurrence OR its type), so the fastener bolt diameter is read from the cross-section profile the
    // realizing element binds — DiameterOf reads the IfcMaterialProfileSetUsage radius when present, else 0d.
    static BoltPattern BoltOf(Seq<IfcMechanicalFastener> fasteners) =>
        fasteners.HeadOrNone().Match(
            Some: f => BoltPattern.Create(f.PredefinedType.ToString(), DiameterOf(f), Seq<(double, double)>(), 0d),
            None: () => BoltPattern.Create("", 0d, Seq<(double, double)>(), 0d));

    static WeldSchedule WeldOf(Seq<IfcMechanicalFastener> fasteners) =>
        WeldSchedule.Create("FW", fasteners.HeadOrNone().Map(DiameterOf).IfNone(0d), 0d, 0d);

    static double LapOf(Seq<IfcElement> reinforcing) =>
        reinforcing.Choose(static e => e is IfcReinforcingBar bar ? Some(NaNZero(bar.BarLength)) : None).HeadOrNone().IfNone(0d);

    // The fastener carries no public nominal scalar, so the diameter rides its cross-section profile radius
    // reached through the inherited HasAssociations IfcRelAssociatesMaterial.RelatingMaterial
    // (IfcMaterialProfileSetUsage → IfcCircleProfileDef.Radius); an unprofiled fastener reads 0d.
    static double DiameterOf(IfcMechanicalFastener fastener) =>
        fastener.HasAssociations
            .AsIterable()
            .OfType<IfcRelAssociatesMaterial>()
            .Select(static rel => rel.RelatingMaterial)
            .OfType<IfcMaterialProfileSetUsage>()
            .SelectMany(static usage => usage.ForProfileSet.MaterialProfiles.AsIterable())
            .Select(static profile => profile.Profile)
            .OfType<IfcCircleProfileDef>()
            .HeadOrNone()
            .Map(static circle => NaNZero(circle.Radius) * 2d)
            .IfNone(0d);

    static double NaNZero(double value) => double.IsNaN(value) ? 0d : value;
}
```

## [03]-[CONNECTION_WIRE]

- Owner: `ConnectionWire` the Bim-side IFC 4.3 serializer of the `csharp:Rasm.Materials/Connection` `ConnectionItem` axis ONLY — mapping the `ConnectionItem` `Family`/`Section` columns onto the `IfcReinforcingBar`/`IfcMechanicalFastener` structural elements (and the new joint weld/stud onto `IfcMechanicalFastener` `STUDSHEARCONNECTOR` + `IfcRelConnectsWithRealizingElements`). The `ConnectionItemWire` is the ONE carrier this owner reads — the material/composition/property/classification/appearance egress is RETIRED from here: the `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers are GONE (the `csharp:Rasm.Materials/Projection#MATERIAL_PROJECTOR` `MaterialProjector` lowers a material's composition/property/appearance/assessment into the seam `ElementGraph`), so the material subgraph egress reads the projected seam graph through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` and `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author`, composed by `Projection/semantic#IFC_EGRESS` `Emit` — never a Materials wire crossing this boundary (the appearance round-trips separately through the `Semantics/appearance#MATERIAL_APPEARANCE` `BimAppearance.Author` content-key owner, the seam `Appearance`-node egress its own concern).
- Entry: `ConnectionWire.Author(DatabaseIfc db, ConnectionItemWire item)` mints the IFC entities from the Materials portable connection-item data — a `ConnectionItem` keyed on its `ConnectionId`/`Family` discriminant authors the `IfcReinforcingBar` (writing the public `NominalDiameter`/`BarLength` setters) or the `IfcMechanicalFastener` (writing the public `IfcMechanicalFastenerTypeEnum` `PredefinedType` for the weld/stud — `STUDSHEARCONNECTOR` for the welded stud, never the reinforcing-bar `STUD` enum — and authoring through the native `(IfcProduct, IfcMaterialProfileSetUsage, IfcAxis2Placement3D, double length)` ctor whose `profile.Associate(this)` mints the `IfcRelAssociatesMaterial` circle-profile association `DiameterOf` reads back AND whose `length` sweep round-trips the run-length, the fastener's `NominalDiameter`/`NominalLength` being GeometryGym-internal on both the occurrence and its type), wrapping the joint in an `IfcRelConnectsWithRealizingElements` over the connected `IfcElement` pair (the native `(IfcConnectionGeometry, relating, related, realizing)` ctor auto-registering the `IfcElement.IsConnectionRealization` back-pointer) — `Fin<T>` aborts on a captured GeometryGym authoring fault (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`. `Author` is the ONLY entry — the `ConnectionItemWire` remains the connection egress until a future Connection `IElementProjection` subsumes it; the material/composition/property/classification egress is NOT an entry here, it reads the projected seam `Material` subgraph through `Projection/semantic#IFC_EGRESS` `Emit` composing the `Semantics/composition`/`classification` egress owners (appearance reconciles at the content-key through `Semantics/appearance` `BimAppearance.Author`).
- Auto: `Author` reads the Materials portable `ConnectionItemWire` (the `ConnectionId`/`Family`/`Section`/scalar fields the Materials owner exposes as host-neutral data, never a Materials assembly type) and constructs the IFC entity through the `FactoryIfc` canonical placements — a bolted/welded fastener authors `IfcMechanicalFastener` with the `PredefinedType` (`BOLT`/`STUDSHEARCONNECTOR`) through the native profile-hosting ctor that carries its diameter on the associated `IfcMaterialProfileSetUsage` circle-profile cross-section and its run-length on the swept axis representation (the public round-trip channel for the GeometryGym-internal fastener nominal scalars), a reinforcing item authors `IfcReinforcingBar`/`IfcReinforcingMesh` with the public diameter/area/length setters, and the realizing relationship authors `IfcRelConnectsWithRealizingElements` over the connected element pair with the fastener as the realizing element; the material/composition/property/classification/appearance egress is NOT folded here — the `Projection/semantic#IFC_EGRESS` `Emit` composes `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` (the `IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` + the `Pset_Material*` rows + the `Associate`-edge `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` [C7]) and `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` (the `IfcRelAssociatesClassification`/`IfcClassificationReference`) reading the projected seam graph, so this owner folds only the connection-item entity.
- Receipt: the authored IFC entities are the connection-item serialization the `Model <- csharp:Rasm.Materials/Connection` `[WIRE]` seam reads — a connection round-trips to IFC by its family discriminant, the reinforcing bar / mechanical fastener carrying its nominal diameter on the associated circle-profile and its run-length on the swept axis; the MATERIAL binding of a connection item (its steel grade, embodied carbon, classification, appearance) rides the seam `Material` subgraph the `MaterialProjector` lowers and the `Emit` material egress authors, joined by the connection's element NodeId through the `Associate` edge, NOT this carrier — the Bim side owns the connection-item IFC 4.3 wire here, the material wire reads the seam graph.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new realizing-element family is one IFC entity the `Author` switch mints from the Materials portable connection-item row; never a per-family wire entrypoint, never a Materials assembly type crossing this boundary; a new material Pset, sustainability metric, or classification standard is NOT a growth point here — those grow on the seam `MaterialPropertySet`/`Classification` family the `Semantics/composition`/`classification` egress authors; a future Connection `IElementProjection` (one registration row, the `csharp:Rasm.Materials/Projection#MATERIAL_PROJECTOR` sibling) would subsume even this `ConnectionItemWire` carrier by lowering a connection item into the seam `Object` graph — until then the connection-item egress is this wire.
- Boundary: the connection wire is the connection-item consumer end of the `Model <- csharp:Rasm.Materials/Connection` `[WIRE]` seam — host-neutral, reading the Materials `ConnectionId`/`Family`/`Section`-keyed portable scalar/string data (never a `Rasm.Materials` assembly type crossing the boundary) and emitting the `IfcReinforcingBar`/`IfcMechanicalFastener`/`IfcRelConnectsWithRealizingElements` entities through the `FactoryIfc` canonical placements, a hand-rolled STEP entity writer being the deleted form; the fastener nominal diameter rides its associated `IfcMaterialProfileSetUsage` circle-profile cross-section (the public round-trip channel for the GeometryGym-internal nominal scalars); the `IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` material-assembly egress, the `IfcMaterialProperties` `Pset_Material*` egress, and the `IfcClassificationReference` egress are NOT here — those read the projected seam `Material` node + `Associate` usage edge + `Object` node `Classification` through `Semantics/composition#MATERIAL_COMPOSITION`/`Semantics/classification#CLASSIFICATION_AXIS` (composed by `Projection/semantic#IFC_EGRESS` `Emit`), so a `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing this boundary is the deleted form (those Materials wires are retired, the material/property/classification egress reading the seam graph, the seam aligning by the element NodeId and `MaterialId`); a wire rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through the `Try.lift(...).Run().MapFail(...)` funnel.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The connection-item seam carrier is DECLARED ONCE by the producer at csharp:Rasm.Materials/Connection#CONNECTION_WIRE
// (the host-neutral owner that owns the portable wire face — ARCHITECTURE C#-sole-producer law); this consumer reads
// the IDENTICAL field shapes one-hop and NEVER re-declares a divergent carrier. Reproduced here as the settled seam
// contract this fence reads: host-neutral scalar/string rows, NEVER a Rasm.Materials ConnectionItem crossing the
// boundary. The retired MaterialAssignmentWire/MaterialPropertyWire carriers are GONE — the material/composition/
// property/classification egress reads the projected seam Material subgraph (Semantics/composition +
// Semantics/classification, composed by Projection/semantic#IFC_EGRESS Emit; appearance reconciles at the
// content-key via Semantics/appearance), never a wire.
public sealed record ConnectionItemWire(string ConnectionId, string Family, string Section, double NominalDiameter, double Length, string FastenerType);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConnectionWire {
    public static Fin<IfcElement> Author(DatabaseIfc db, ConnectionItemWire item) =>
        Try.lift(() => AuthorElement(db, item)).Run().MapFail(static error => new BimFault.ModelRejected($"connection-wire:{error.Message}").ToError());

    // IfcReinforcingBar exposes public NominalDiameter/BarLength setters, so the bar writes its scalars
    // directly through the (IfcObjectDefinition host, IfcObjectPlacement, representation) ctor (NO single-arg
    // (DatabaseIfc) ctor exists), hosted on db.Project at the factory root placement. IfcMechanicalFastener
    // carries NO public nominal scalar (mNominalDiameter/mNominalLength are internal on both the occurrence
    // AND its type), so the fastener authors through the native (IfcProduct host, IfcMaterialProfileSetUsage
    // profile, IfcAxis2Placement3D placement, double length) ctor: that ctor calls profile.Associate(this)
    // — minting the same IfcRelAssociatesMaterial → IfcMaterialProfileSetUsage → IfcCircleProfileDef.Radius
    // chain DiameterOf reads back — AND sweeps the NominalLength as the IfcShapeRepresentation axis, so the
    // diameter (profile radius × 2) and the run-length round-trip on one native construction, never a
    // hand-built association that drops the length.
    static IfcElement AuthorElement(DatabaseIfc db, ConnectionItemWire item) =>
        item.Family switch {
            "ReinforcingBar" => new IfcReinforcingBar(db.Project, db.Factory.RootPlacement, null) {
                NominalDiameter = item.NominalDiameter, BarLength = item.Length, GlobalId = ParserIfc.HashGlobalID(item.ConnectionId),
            },
            _ => new IfcMechanicalFastener(db.Project, ProfileUsageOf(db, item), db.Factory.XYPlanePlacement, item.Length) {
                PredefinedType = Enum.TryParse<IfcMechanicalFastenerTypeEnum>(item.FastenerType, true, out var kind) ? kind : IfcMechanicalFastenerTypeEnum.BOLT,
                GlobalId = ParserIfc.HashGlobalID(item.ConnectionId),
            },
        };

    // The fastener nominal diameter rides the circle-profile cross-section the native ctor's profile.Associate
    // wires, so the projection's DiameterOf (radius × 2) recovers it from the public material-profile chain.
    static IfcMaterialProfileSetUsage ProfileUsageOf(DatabaseIfc db, ConnectionItemWire item) =>
        new(new IfcMaterialProfileSet(item.ConnectionId,
            new IfcMaterialProfile(item.Section, new IfcMaterial(db, item.Family), new IfcCircleProfileDef(db, item.Section, item.NominalDiameter / 2d))));
}
```

## [04]-[RESEARCH]

- [REALIZING_ELEMENT_SURFACE]: the `IfcRelConnectsWithRealizingElements`/`IfcMechanicalFastener`/`IfcReinforcingBar`/`IfcReinforcingMesh` realizing-element surface the `ConnectionProjection.Project` fold reads is verified against the live GeometryGym decompile — `IfcRelConnectsWithRealizingElements : IfcRelConnectsElements` carries `RealizingElements` (`SET<IfcElement>`) plus an internal `ConnectionType` string and inherits `RelatingElement`/`RelatedElement` (`IfcElement`)/`ConnectionGeometry`, the realizing-element back-pointer being `IfcElement.IsConnectionRealization`; `IfcMechanicalFastener : IfcElementComponent` exposes only `PredefinedType` (`IfcMechanicalFastenerTypeEnum`: `BOLT`/`ANCHORBOLT`/`DOWEL`/`NAIL`/`RIVET`/`SCREW`/`SHEARCONNECTOR`/`STAPLE`/`STUDSHEARCONNECTOR`/`COUPLER`) as a public property — its `mNominalDiameter`/`mNominalLength` are `internal` fields with NO public getter/setter on the occurrence OR on `IfcMechanicalFastenerType`, so the fastener diameter rides its associated `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section (reached through the inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial`) on both the projection read (`DiameterOf`) and the wire author (the native `IfcMechanicalFastener(IfcProduct, IfcMaterialProfileSetUsage, IfcAxis2Placement3D, double length)` ctor whose `profile.Associate(this)` mints that exact association and whose `length` sweep round-trips the nominal length the internal `mNominalLength` cannot expose), the public round-trip channel for the internal nominal scalars; `IfcReinforcingBar : IfcReinforcingElement` carries `NominalDiameter`/`CrossSectionArea`/`BarLength` (double) and `PredefinedType` (`IfcReinforcingBarTypeEnum`: `MAIN`/`SHEAR`/`LIGATURE`/`STUD`/`PUNCHING`/`EDGE`/`RING`/`ANCHORING`/`SPACEBAR`) plus `BarSurface`; `IfcReinforcingMesh : IfcReinforcingElement` carries `MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing` — so the `RealizationOf` fold reads the real realizing-element scalar members and the `IfcMechanicalFastenerTypeEnum` partition discriminates the `Bolted`/`Welded` arm (the `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` weld/stud onto `Welded`), the connection detail staying host-neutral scalar/handle data and never re-tessellating the fastener; the `.api/api-geometrygym-ifc` catalogue gains the `IfcRelConnectsWithRealizingElements`/`IfcMechanicalFastener`/`IfcReinforcingBar`/`IfcReinforcingMesh` rows the projection fold reads.
- [MEMBER_CONNECTION_JOIN]: the `Model/structural#ANALYSIS_MODEL` `MemberConnection` `(MemberGlobalId, ConnectionGlobalId)` edge the connection detail resolves its physical joint by grounds against the structural-analysis graph — the analytical model names WHICH members meet (the abstract `MemberConnection` edge) and the `ConnectionDetail` names HOW they join (the `ConnectionRealization` over the realizing elements), the two joined by the connected-member `GlobalId` pair and the physical `BimElement` by `GlobalId` so a connection-capacity binding to the analytical member reads one typed detail; the `(GeometryKey, DetailKey)` content-key the fabrication consumer reads grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom, the connection owner producing the typed detail and its content-key identity, never a second member-selection surface.
- [MATERIALS_IFC_WIRE]: the `[03]-[CONNECTION_WIRE]` IFC 4.3 serialization of the `csharp:Rasm.Materials/Connection` `ConnectionItem` axis is the Bim-side counterpart of the `Rasm.Materials` `JOINT_CONNECTION_FAMILY` idea — the Materials owner exposes the `ConnectionItem` `Family`/`Section` columns as the host-neutral `ConnectionItemWire` carrier, and the Bim `ConnectionWire.Author` maps them onto the GeometryGym `IfcReinforcingBar`/`IfcMechanicalFastener` structural elements + `IfcRelConnectsWithRealizingElements`, the fastener nominal diameter riding its `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section. The material/composition/property/classification egress is RETIRED from this owner (`ELEMENT-REBUILD-PLAN.md` §6 `Rasm.Materials` ripple — the `MaterialProjector` lowers a material's composition/property/appearance/assessment into the seam `ElementGraph`, the retired `MaterialAssignmentWire`/`MaterialPropertyWire` carriers GONE): `Rasm.Bim` reads the projected seam `Material` node + the `Associate` edge `MaterialUsage` and authors the IFC material-definition family + `IfcMaterialProperties` `Pset_Material*` + `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` [C7] through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage`, the `Object` node `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` through `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` — both composed by `Projection/semantic#IFC_EGRESS` `Emit`, the GeometryGym material/usage/classification ctor surface decompile-confirmed against the live 25.7.30 surface (`.api/api-geometrygym-ifc` material + classification families); the appearance reconciles separately at the content-key through `Semantics/appearance#MATERIAL_APPEARANCE` `BimAppearance.Author` (the seam `Appearance`-node egress its own round-trip concern, not this material-wire retirement). The `ConnectionItemWire` connection-item egress remains here until a future Connection `IElementProjection` (one registration row, the `csharp:Rasm.Materials/Projection#MATERIAL_PROJECTOR` sibling) subsumes it by lowering a connection item into the seam `Object` graph.
