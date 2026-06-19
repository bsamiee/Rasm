# [ANALYSIS_STRUCTURAL_MODEL]

The host-neutral structural-analysis projection: one `AnalysisModel` record carrying the idealized analysis graph — the closed `AnalysisMember` `[Union]` (`Curve`/`Surface`/`PointConnection`) over `IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/`IfcStructuralPointConnection`, the `LoadGroup` and `Support` records over `IfcStructuralLoadGroup`/`IfcStructuralConnection` plus the `MemberConnection` edge over `IfcRelConnectsStructuralMember` — and the `AnalysisProjection.Project` fold that folds the GeometryGym `IfcStructuralAnalysisModel` container into the typed graph the `csharp:AEC_SIMULATION_BRIDGE` Compute solver reads. The analysis model is a VIEW of the federated `Model/elements#ELEMENT_MODEL` `BimModel`, never a re-modeled mesh: each idealized `AnalysisMember` binds its physical `BimElement` by `GlobalId` through the optional `PhysicalGlobalId` reference, the boundary conditions ride the typed `Support` over `IfcBoundaryCondition`, and the loads ride the `LoadGroup` over `IfcStructuralLoadGroup`/`IfcStructuralLoadCase` — so a beam's analytical line, a slab's analytical surface, and a column-base node carry their idealized member while the physical element keeps its solid geometry. The model is HOST-NEUTRAL — the analytical curve/surface binds the kernel `Rasm` geometry by reference through the same `Model/elements#ELEMENT_MODEL` `GeometryHandle` and never carries a RhinoCommon type — and the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.Structural)` arm selects the analysis-domain element set the projection idealizes, never a second selection surface. The `(GeometryKey, PropertyKey)` `UInt128` content-key pair the `AnalysisModel.Identity` derives is the reference the `csharp:AEC_SIMULATION_BRIDGE` solver reads the graph by at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, joining the boundary conditions to the AISC profiles table the solver evaluates against rather than re-projecting the graph; an analysis rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[ANALYSIS_MODEL]: `AnalysisModel` record, the `AnalysisMember` `[Union]` (`Curve`/`Surface`/`PointConnection`), the `LoadGroup`/`Support`/`MemberConnection` records, the `StructuralLoadKind`/`SupportRestraint` value objects, and the `AnalysisProjection.Project` fold from the `IfcStructuralAnalysisModel` surface to the `(GeometryKey, PropertyKey)` solver identity.

## [02]-[ANALYSIS_MODEL]

- Owner: `AnalysisModel` the single host-neutral structural-analysis-graph record carrying the idealized member set, the load groups, the support set, the member-to-connection edges, and the `(GeometryKey, PropertyKey)` content-key identity the Compute solver reads it by; `AnalysisMember` the closed `[Union]` discriminating the three IFC idealized-member modalities — `Curve` (the `IfcStructuralCurveMember` 1D beam/column line carrying its `StructuralCurveMemberKind` joint discriminant), `Surface` (the `IfcStructuralSurfaceMember` 2D slab/wall surface carrying its thickness), `PointConnection` (the `IfcStructuralPointConnection` node) — each binding its physical `BimElement` by an optional `PhysicalGlobalId` reference; `LoadGroup` the `IfcStructuralLoadGroup`/`IfcStructuralLoadCase` grouped-load record carrying its `StructuralLoadKind` discriminant; `Support` the `IfcStructuralConnection`/`IfcBoundaryCondition` boundary-condition record carrying its `SupportRestraint` six-degree-of-freedom value object; `MemberConnection` the `IfcRelConnectsStructuralMember` edge joining an idealized member to a connection; `AnalysisProjection` the static fold over the GeometryGym `IfcStructuralAnalysisModel` surface.
- Cases: `AnalysisMember` arms `Curve` (`IfcStructuralCurveMember` — `GlobalId`, `Name`, `StructuralCurveMemberKind`, the analytical-line `GeometryHandle`, optional `PhysicalGlobalId`) · `Surface` (`IfcStructuralSurfaceMember` — `GlobalId`, `Name`, `Thickness`, the analytical-surface `GeometryHandle`, optional `PhysicalGlobalId`) · `PointConnection` (`IfcStructuralPointConnection` — `GlobalId`, `Name`, the node `GeometryHandle`, optional `PhysicalGlobalId`) (3); the `LoadGroup` record carries its `GlobalId`, `Name`, `StructuralLoadKind` (`LoadGroup`/`LoadCase`/`LoadCombination` over `IfcLoadGroupTypeEnum`), and the `Seq<string>` of structural-item GlobalIds it loads; the `Support` record carries its `GlobalId`, `Name`, the `SupportRestraint` six-DOF restraint value object (translational X/Y/Z and rotational X/Y/Z fixity), and the optional `PhysicalGlobalId` — a point-connection node carrying an `AppliedCondition` is both a `PointConnection` member in the graph and a `Support` row in the boundary set, while an unrestrained connection is a node only; the `MemberConnection` edge carries its `MemberGlobalId` and `ConnectionGlobalId`.
- Entry: `AnalysisProjection.Project(IfcStructuralAnalysisModel model, BimModel federated)` folds one GeometryGym analysis container into one `AnalysisModel` — materializing the container's `IsGroupedBy` `IfcRelAssignsToGroup.RelatedObjects` `IfcStructuralItem` set once (the container is an `IfcSystem` grouping its idealized members and connections), discriminating each member (`IfcStructuralCurveMember` folds to `Curve`, `IfcStructuralSurfaceMember` folds to `Surface`, `IfcStructuralPointConnection` folds to `PointConnection`), folding the `LoadedBy` `IfcStructuralLoadGroup` set onto `LoadGroup` rows, folding the grouped `IfcStructuralConnection` set onto `Support` rows reading the `AppliedCondition` `IfcBoundaryCondition`, folding the members' `ConnectedBy` `IfcRelConnectsStructuralMember` relationships onto `MemberConnection` edges, and binding each idealized member to its physical `BimElement` by resolving the member GlobalId against the `federated` element index — and `AnalysisProjection.ProjectAll(Seq<IfcStructuralAnalysisModel> models, BimModel federated)` lifts every analysis container in a federated model onto the `Seq<AnalysisModel>` the solver reads; `Fin<T>` aborts on an analysis container grouping an unmapped structural-item entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) or a member naming a physical GlobalId the federated model never declares (`BimFault.DanglingReference`) lowered with `.ToError()`.
- Auto: `Project` reads the `IfcStructuralAnalysisModel` runtime graph and folds it into the typed model — the `ItemsOf` projection materializes the container's `IsGroupedBy` grouped `IfcStructuralItem` set once and the member fold discriminates it by runtime type (`IfcStructuralCurveMember` reads `PredefinedType` onto the `StructuralCurveMemberKind` joint discriminant and projects to a `Curve` member, `IfcStructuralSurfaceMember` reads `Thickness` and projects to a `Surface` member, `IfcStructuralPointConnection` projects to a `PointConnection` node, all over the same materialized item set so the supports and member-connection edges read the one traversal rather than three), the `LoadedBy` `IfcStructuralLoadGroup` set folds onto `LoadGroup` rows reading `PredefinedType` onto the `StructuralLoadKind` and the group's own `IsGroupedBy` `IfcRelAssignsToGroup.RelatedObjects` loaded-item set onto the loaded-GlobalId sequence, each grouped `IfcStructuralConnection` folds onto a `Support` reading its `AppliedCondition` `IfcBoundaryNodeCondition`/`IfcBoundaryEdgeCondition` six-DOF fixity onto the `SupportRestraint`, each member's `ConnectedBy` `IfcRelConnectsStructuralMember` folds onto a `MemberConnection` edge carrying its `RelatingStructuralMember.GlobalId`/`RelatedStructuralConnection.GlobalId`; the `AnalysisModel.BindPhysical` fold resolves each idealized member's GlobalId against the `Model/elements#ELEMENT_MODEL` `BimModel` element index so a member that idealizes a physical `BimElement` carries the `PhysicalGlobalId` reference and the solver reads the physical solid by reference, and the `AnalysisModel.Identity` fold derives the `(GeometryKey, PropertyKey)` `UInt128` pair the `csharp:AEC_SIMULATION_BRIDGE` solver reads the graph by — `GeometryKey` over the analytical-member geometry-handle keys through `XxHash128.HashToUInt128` and `PropertyKey` over the load/support boundary-condition rows so the solver re-reads only a changed graph.
- Receipt: the `Seq<AnalysisModel>` is the analysis evidence the `csharp:AEC_SIMULATION_BRIDGE` Compute solver reads by the `(GeometryKey, PropertyKey)` reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.Structural)` arm selects the physical element set the projection idealizes, and the `Support`/`LoadGroup` rows are the boundary conditions the solver evaluates the AISC profiles table against; the idealized line member, the analytical surface member, and the node connection each carry their physical-element binding on one record.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new idealized-member modality is one `AnalysisMember` union arm reading the next `IfcStructuralItem` entity; a new load kind is one `StructuralLoadKind` row reading the next `IfcLoadGroupTypeEnum` member; a new boundary-condition degree of freedom is one column on the existing `SupportRestraint` value object; a new analysis container rides the existing `ProjectAll` fold on one row; never a per-member-type analysis record, never a second analysis store, and never a re-modeled analytical mesh.
- Boundary: `AnalysisModel` is ONE record discriminated by the `AnalysisMember` union — a `CurveMember`/`SurfaceMember`/`NodeMember` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the idealized member is a VIEW of the physical `BimElement` bound by `PhysicalGlobalId` reference and a re-modeled analytical mesh inlined on the analysis record is the named seam violation — the solver reads the physical solid geometry by the `Model/elements#ELEMENT_MODEL` `GeometryHandle` reference, never a second tessellation; the GeometryGym `IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/`IfcStructuralPointConnection`/`IfcStructuralLoadGroup`/`IfcStructuralConnection`/`IfcBoundaryCondition`/`IfcRelConnectsStructuralMember` surface (`.api/api-geometrygym-ifc` structural-analysis-domain rows 1-16) is consumed as settled vocabulary through the `IfcStructuralItem` discrimination and a hand-rolled structural-member reader is the deleted form; the analysis-domain element selection is the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.Structural)` predicate and a parallel analysis-element selection arm is the no-second-selection-surface reject; the `(GeometryKey, PropertyKey)` content-key identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and the `csharp:AEC_SIMULATION_BRIDGE` solver reads the graph by that reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` — minting a second identity scheme for the solver join is the named cross-folder drift defect; an analysis rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

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

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class StructuralLoadKind {
    public static readonly StructuralLoadKind LoadGroup       = new("LOAD_GROUP");
    public static readonly StructuralLoadKind LoadCase        = new("LOAD_CASE");
    public static readonly StructuralLoadKind LoadCombination = new("LOAD_COMBINATION");
    public static readonly StructuralLoadKind UserDefined     = new("USERDEFINED");
    public static readonly StructuralLoadKind NotDefined      = new("NOTDEFINED");

    public static StructuralLoadKind Of(IfcLoadGroupTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class StructuralCurveMemberKind {
    public static readonly StructuralCurveMemberKind RigidJoined  = new("RIGID_JOINED_MEMBER");
    public static readonly StructuralCurveMemberKind PinJoined    = new("PIN_JOINED_MEMBER");
    public static readonly StructuralCurveMemberKind Cable        = new("CABLE");
    public static readonly StructuralCurveMemberKind Tension      = new("TENSION_MEMBER");
    public static readonly StructuralCurveMemberKind Compression  = new("COMPRESSION_MEMBER");
    public static readonly StructuralCurveMemberKind NotDefined   = new("NOTDEFINED");

    public static StructuralCurveMemberKind Of(IfcStructuralCurveMemberTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

public readonly record struct SupportRestraint(
    bool TranslationX, bool TranslationY, bool TranslationZ,
    bool RotationX, bool RotationY, bool RotationZ) {
    public static readonly SupportRestraint Free  = new(false, false, false, false, false, false);
    public static readonly SupportRestraint Fixed = new(true, true, true, true, true, true);
    public static readonly SupportRestraint Pinned = new(true, true, true, false, false, false);

    public bool IsFree => this == Free;
}

[Union]
public partial record AnalysisMember {
    partial record Curve(string GlobalId, string Name, StructuralCurveMemberKind Kind, GeometryHandle Geometry, Option<string> PhysicalGlobalId);
    partial record Surface(string GlobalId, string Name, double Thickness, GeometryHandle Geometry, Option<string> PhysicalGlobalId);
    partial record PointConnection(string GlobalId, string Name, GeometryHandle Geometry, Option<string> PhysicalGlobalId);

    public string GlobalId => Switch(
        curve:           static m => m.GlobalId,
        surface:         static m => m.GlobalId,
        pointConnection: static m => m.GlobalId);

    public Option<string> PhysicalGlobalId => Switch(
        curve:           static m => m.PhysicalGlobalId,
        surface:         static m => m.PhysicalGlobalId,
        pointConnection: static m => m.PhysicalGlobalId);

    public string GeometryKey => Switch(
        curve:           static m => m.Geometry.Key,
        surface:         static m => m.Geometry.Key,
        pointConnection: static m => m.Geometry.Key);
}

public sealed record LoadGroup(string GlobalId, string Name, StructuralLoadKind Kind, Seq<string> LoadedGlobalIds);

public sealed record Support(string GlobalId, string Name, SupportRestraint Restraint, Option<string> PhysicalGlobalId);

public sealed record MemberConnection(string MemberGlobalId, string ConnectionGlobalId);

public sealed record AnalysisModel(
    string GlobalId,
    string Name,
    Seq<AnalysisMember> Members,
    Seq<LoadGroup> Loads,
    Seq<Support> Supports,
    Seq<MemberConnection> Connections) {
    public (UInt128 GeometryKey, UInt128 PropertyKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("", Members.Map(static m => m.GeometryKey)))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("",
            Loads.Map(static l => $"{l.GlobalId}={l.Kind.Key}:{string.Join(",", l.LoadedGlobalIds)}")
                .Concat(Supports.Map(static s => $"{s.GlobalId}={s.Restraint}"))))));

    public AnalysisModel BindPhysical(BimModel federated) {
        var index = toHashSet(federated.Elements.Map(static e => e.GlobalId));
        return this with {
            Members = Members.Map(member => member switch {
                AnalysisMember.Curve c           => c with { PhysicalGlobalId = index.Contains(c.GlobalId) ? Some(c.GlobalId) : None },
                AnalysisMember.Surface s          => s with { PhysicalGlobalId = index.Contains(s.GlobalId) ? Some(s.GlobalId) : None },
                AnalysisMember.PointConnection p  => p with { PhysicalGlobalId = index.Contains(p.GlobalId) ? Some(p.GlobalId) : None },
                _                                 => member,
            }),
        };
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class AnalysisProjection {
    public static Fin<AnalysisModel> Project(IfcStructuralAnalysisModel model, BimModel federated) {
        var items = ItemsOf(model);
        return items
            .Filter(static item => item is IfcStructuralMember or IfcStructuralPointConnection)
            .TraverseM(MemberOf)
            .As()
            .Map(members => new AnalysisModel(
                model.GlobalId,
                model.Name ?? "",
                members,
                model.LoadedBy.AsIterable().Map(LoadGroupOf).ToSeq(),
                SupportsOf(items),
                ConnectionsOf(items))
                .BindPhysical(federated));
    }

    public static Fin<Seq<AnalysisModel>> ProjectAll(Seq<IfcStructuralAnalysisModel> models, BimModel federated) =>
        models.TraverseM(model => Project(model, federated)).As();

    static Seq<IfcStructuralItem> ItemsOf(IfcStructuralAnalysisModel model) =>
        model.IsGroupedBy
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcStructuralItem>()
            .ToSeq();

    static Fin<AnalysisMember> MemberOf(IfcStructuralItem item) =>
        item switch {
            IfcStructuralCurveMember curve =>
                FinSucc<AnalysisMember>(new AnalysisMember.Curve(
                    curve.GlobalId, curve.Name ?? "",
                    StructuralCurveMemberKind.Of(curve.PredefinedType),
                    GeometryHandle.Pending(curve.GlobalId), None)),
            IfcStructuralSurfaceMember surface =>
                FinSucc<AnalysisMember>(new AnalysisMember.Surface(
                    surface.GlobalId, surface.Name ?? "",
                    surface.Thickness,
                    GeometryHandle.Pending(surface.GlobalId), None)),
            IfcStructuralPointConnection node =>
                FinSucc<AnalysisMember>(new AnalysisMember.PointConnection(
                    node.GlobalId, node.Name ?? "",
                    GeometryHandle.Pending(node.GlobalId), None)),
            _ =>
                FinFail<AnalysisMember>(new BimFault.ModelRejected($"structural-item-unmapped:{item.GetType().Name}").ToError()),
        };

    static LoadGroup LoadGroupOf(IfcStructuralLoadGroup group) =>
        new(group.GlobalId, group.Name ?? "",
            StructuralLoadKind.Of(group.PredefinedType),
            group.IsGroupedBy
                .AsIterable()
                .SelectMany(static rel => rel.RelatedObjects.AsIterable())
                .Select(static o => o.GlobalId)
                .Where(static id => id.Length > 0)
                .ToSeq());

    static Seq<Support> SupportsOf(Seq<IfcStructuralItem> items) =>
        items
            .AsIterable()
            .OfType<IfcStructuralConnection>()
            .Where(static connection => connection.AppliedCondition is not null)
            .Map(static connection => new Support(
                connection.GlobalId, connection.Name ?? "",
                RestraintOf(connection.AppliedCondition), None))
            .ToSeq();

    static Seq<MemberConnection> ConnectionsOf(Seq<IfcStructuralItem> items) =>
        items
            .AsIterable()
            .OfType<IfcStructuralMember>()
            .SelectMany(static member => member.ConnectedBy.AsIterable())
            .OfType<IfcRelConnectsStructuralMember>()
            .Map(static rel => new MemberConnection(
                rel.RelatingStructuralMember?.GlobalId ?? "",
                rel.RelatedStructuralConnection?.GlobalId ?? ""))
            .Where(static edge => edge.MemberGlobalId.Length > 0 && edge.ConnectionGlobalId.Length > 0)
            .ToSeq();

    static SupportRestraint RestraintOf(IfcBoundaryCondition? condition) =>
        condition switch {
            IfcBoundaryNodeCondition node => new SupportRestraint(
                IsFixed(node.TranslationalStiffnessX), IsFixed(node.TranslationalStiffnessY), IsFixed(node.TranslationalStiffnessZ),
                IsFixed(node.RotationalStiffnessX), IsFixed(node.RotationalStiffnessY), IsFixed(node.RotationalStiffnessZ)),
            IfcBoundaryEdgeCondition edge => new SupportRestraint(
                IsFixed(edge.LinearStiffnessByLengthX), IsFixed(edge.LinearStiffnessByLengthY), IsFixed(edge.LinearStiffnessByLengthZ),
                IsFixed(edge.RotationalStiffnessByLengthX), IsFixed(edge.RotationalStiffnessByLengthY), IsFixed(edge.RotationalStiffnessByLengthZ)),
            _ => SupportRestraint.Free,
        };

    static bool IsFixed(object? stiffness) =>
        stiffness switch {
            IfcTranslationalStiffnessSelect { Rigid: true }                          => true,
            IfcTranslationalStiffnessSelect { Stiffness.Measure: > 0d }              => true,
            IfcRotationalStiffnessSelect { Rigid: true }                             => true,
            IfcRotationalStiffnessSelect { Stiffness.Measure: > 0d }                 => true,
            IfcModulusOfTranslationalSubgradeReactionSelect { Rigid: true }          => true,
            IfcModulusOfTranslationalSubgradeReactionSelect { Stiffness.Measure: > 0d } => true,
            IfcModulusOfRotationalSubgradeReactionSelect { Rigid: true }             => true,
            IfcModulusOfRotationalSubgradeReactionSelect { Stiffness.Measure: > 0d } => true,
            _                                                                        => false,
        };
}
```

## [03]-[RESEARCH]

- [STRUCTURAL_ITEM_DISPATCH]: the `IfcStructuralAnalysisModel` container traversal — the `IsGroupedBy` `IfcRelAssignsToGroup.RelatedObjects` grouped `IfcStructuralItem` set the `ItemsOf` fold materializes the `IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/`IfcStructuralPointConnection` idealized members and `IfcStructuralConnection` boundaries through, the `LoadedBy` `IfcStructuralLoadGroup` set, and each member's `ConnectedBy` `IfcRelConnectsStructuralMember` edge set — grounds against the GeometryGym structural-analysis-domain member surface (`.api/api-geometrygym-ifc` structural-analysis-domain rows 1-16) so the `MemberOf`/`SupportsOf`/`ConnectionsOf` projections discriminate the real analysis graph rather than a guessed shape; the `IfcStructuralAnalysisModel.IsGroupedBy`/`LoadedBy`/`HasResults`/`OrientationOf2DPlane`, `IfcStructuralLoadGroup.IsGroupedBy`/`PredefinedType`, `IfcStructuralMember.ConnectedBy`, `IfcStructuralCurveMember.PredefinedType`, `IfcStructuralSurfaceMember.Thickness`, `IfcStructuralConnection.AppliedCondition`, and `IfcRelConnectsStructuralMember.RelatingStructuralMember`/`RelatedStructuralConnection` member spellings confirm against the catalogued surface before the projection fold is final — the `IfcStructuralAnalysisModel`/`IfcStructuralLoadGroup` `IsGroupedBy` grouping path is the `IfcSystem`/`IfcGroup` member set, distinct from the `IfcStructuralItem.AssignedToStructuralItem` item-level relationship.
- [BOUNDARY_CONDITION_RESTRAINT]: the `IfcBoundaryNodeCondition`/`IfcBoundaryEdgeCondition` six-degree-of-freedom stiffness members the `RestraintOf` fold reads onto the `SupportRestraint` value object are verified against the live GeometryGym decompile — the node condition carries `TranslationalStiffnessX`/`Y`/`Z` (`IfcTranslationalStiffnessSelect`) and `RotationalStiffnessX`/`Y`/`Z` (`IfcRotationalStiffnessSelect`); the edge condition carries `LinearStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfTranslationalSubgradeReactionSelect`) and `RotationalStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfRotationalSubgradeReactionSelect`). Three of the four selects (`IfcTranslationalStiffnessSelect`, `IfcModulusOfTranslationalSubgradeReactionSelect`, `IfcModulusOfRotationalSubgradeReactionSelect`) derive the generic `StiffnessSelect<T>` base, but `IfcRotationalStiffnessSelect` derives `object` directly with no shared base or interface — so a single `IsFixed<T>(StiffnessSelect<T>?)` generic cannot bind the rotational-node arm. All four nonetheless expose the identical public shape (`Boolean Rigid` rigid-fixity flag and a `Stiffness` measure exposing `.Measure`, the measure type `: IfcDerivedMeasureValue`), so the `IsFixed(object?)` predicate discriminates each concrete select with a `{ Rigid: true }` / `{ Stiffness.Measure: > 0d }` property pattern in one helper — a rigid support reads `Rigid == true`, a finite spring reads `Stiffness.Measure > 0`; no `IfcBoolean.TRUE` (the `Rigid` flag is a plain `bool`) and no `.Magnitude` (the measure exposes `.Measure`).
- [SOLVER_CONTENT_KEY]: the `AnalysisModel.Identity` `(GeometryKey, PropertyKey)` `UInt128` pair the `csharp:AEC_SIMULATION_BRIDGE` Compute solver reads the graph by grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` owner, so the solver re-reads the analysis graph only on a changed `GeometryKey` (the analytical-member geometry handles) or `PropertyKey` (the load/support boundary-condition rows) rather than re-projecting the container; the AISC profiles table the solver evaluates the boundary conditions against is the cross-folder `csharp:AEC_SIMULATION_BRIDGE` solver's concern read by the `(GeometryKey, PropertyKey)` reference, and re-minting an analysis solver or a second identity scheme in the Bim folder is the named cross-folder drift defect — the analysis owner produces the idealized graph and its content-key identity, never the solve.
