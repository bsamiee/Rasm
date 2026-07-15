# [BIM_ZONE_GRAPH]

The Bim grouping VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`: the IFC many-to-many grouping overlay read as a derived `BimZone` over the seam's neutral `Relations/relation#EDGE_ALGEBRA` `Assign`/`Compose` edges, never a parallel relationship union and never a second stored grouping record. The seam owns the property graph — the `Object` nodes, the neutral `Relationship` edge algebra, and the built-once incidence index `EdgesAt` reads in O(degree); this page owns the IFC GROUPING INTERPRETATION the seam is deliberately blind to: the `BimZoneKind` `[SmartEnum<string>]` grouping-interpretation vocabulary (the `IfcGroup`/`IfcSystem`/`IfcZone`/`IfcBuiltSystem`/`IfcDistributionSystem`/`IfcStructuralLoadGroup`/`IfcSpatialZone` family, each row carrying ONLY the `IsSpatial` membership-modality flag the element taxonomy cannot — every shared classification axis, the `IfcDomain`, the `SchemaSpan` availability window, and the per-token `PredefinedRow` valid set, DERIVING from the row's resolved generated `Model/elements#IFC_CLASS` roster row through the one `Taxonomy` read, so schema truth has one source), the `BimZone` derived grouping record with its rollup/coverage analysis surface, and the `ZoneProjection` fold that resolves a grouping `Object` node plus its membership edges off the canonical graph.

The grouping entities are seam `Object` nodes the `Projection/semantic#SEMANTIC_PROJECTOR` projector classifies with the generic `Classification("ifc", code)`, and the two IFC membership relationships are the seam's `Assign.Group` (logical assignment, `IfcRelAssignsToGroup`) and `Compose.Reference` (non-owning spatial reference, `IfcRelReferencedInSpatialStructure`) edges the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` lowered, the IFC `IfcRel*` roster living WHOLLY in that projector [C5] — so the grouping overlay re-opens no IFC-schema strata on the seam and mints no second relationship union. The retired `BimZone`-over-`IfcSemanticModel.ZoneRow` projection, the `ZoneAssignment` `[Union]`, and the `BimModel.Zones` index are GONE: the membership is read off the seam incidence index, and `BimZoneKind` is the IFC-schema grouping authority the projector composes — the grouping dual of the `Model/elements#IFC_CLASS` element taxonomy, distinct from it (only the empty intersection: no entity is both a placeable product and a logical group), keyed on the same seam `Classification.Code`.

The overlay is the orthogonal companion to the single-parent `Model/spatial#SPATIAL_STRUCTURE` `SpatialStructure` containment tree: an element sits in exactly one `Compose.Contain` container yet belongs to arbitrarily many groups, so the grouping graph is the many-to-many algebra the single-parent tree structurally cannot express — the spatial REFERENCE (`IfcSpatialZone` over `Compose.Reference`) is this page's, the spatial CONTAINMENT (`IfcBuildingStorey` over `Compose.Contain`) the tree's, the two reading the same `ComposeKind` algebra over disjoint `Whole`-class sets and never collapsed. The typed `Seq<BimZone>` is the grouping evidence the `Model/structural#STRUCTURAL_PROJECTION` thermal-zone/load-group selection and the `Model/systems#CONNECTIVITY` MEP distribution-system grouping read by reference, and the group-centric `BimZone` is the dual of the element-centric `Model/query#ELEMENT_SET` `ByZone` arm — the arm asks "is this element in that group", the view asks "what does that group hold". A grouping-class or predefined miss at the projector's ingress/egress lifts the typed `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` BARE onto the `Fin<T>` rail (the band is `Expected`-derived, no `.ToError()` hop); the read view is TOTAL.

## [01]-[INDEX]

- [01]-[ZONE_GRAPH]: the `BimZoneKind` `[SmartEnum<string>]` grouping-interpretation vocabulary (the `IsSpatial` modality flag plus the `Taxonomy` derivation onto the generated roster row — `Domain`/`Span`/`ValidPredefined` read THERE, never restated; the `Resolve` strict view-side lookup + `TryGet(...).IfNone(Group)` permissive read), the `BimZone` derived group-centric overlay record with its member-overlap projection, and the `ZoneProjection.Of`/`All`/`Aggregate`/`Values`/`Uncovered`/`Adjacencies` surface — membership, semantic and quantity reads, coverage audit, and boundary-derived zone adjacency off the seam `ElementGraph`.

## [02]-[ZONE_GRAPH]

- Owner: `BimZoneKind` is the grouping-interpretation vocabulary keyed by seam `Classification.Code`; each row carries only the `IsSpatial` membership modality and derives domain, schema span, and predefined tokens from `IfcClass`. `BimZone` is the group-centric view over members and parent groups, and `ZoneProjection` owns its resolution, aggregation, coverage, and shared-membership operations.
- Cases: `BimZoneKind` rows `Group`/`System`/`Zone`/`BuiltSystem`/`DistributionSystem`/`DistributionCircuit`/`ElectricalCircuit`/`StructuralLoadGroup`/`StructuralLoadCase`/`StructuralResultGroup`/`StructuralAnalysisModel`/`Inventory`/`Asset`/`Condition`/`SpatialZone` (15 — the complete IFC grouping closure, every live `IfcGroup`/`IfcSystem` descendant plus the spatial `IfcSpatialZone`, proven by the exhaustive `IfcGroup`-assignable reflection sweep), `SpatialZone` the sole `IsSpatial = true` row so a fire/thermal `IfcSpatialZone` reads the `Compose.Reference` membership overlay rather than the `Assign.Group` overlay. Everything else is the roster's: the `Condition`/`ElectricalCircuit` `RemovedIn = Ifc4` retired windows ride the emitter `Retirements` overlay rows (`[Obsolete] DEPRECATED IFC4`, the legacy circuit superseded by `IfcDistributionCircuit` — admitted on an IFC2x3 round-trip, refused on any IFC4+ emit, never degraded to `Group`), the non-empty token sets (`IfcBuiltSystemTypeEnum`/`IfcDistributionSystemEnum`/`IfcLoadGroupTypeEnum`/`IfcAnalysisModelTypeEnum`/`IfcInventoryTypeEnum`/`IfcSpatialZoneTypeEnum`, the circuit and load-case inheriting their parent enum through the emitter's nearest-declared walk) commit as generated `PredefinedRow` spans, and the `IFC4X4_DRAFT` members stay excluded by the `ReleaseMap` law until the released row lands. The membership modality is the two seam edge kinds (`Assign.Group` logical, `Compose.Reference` spatial), never a per-relationship Bim case.
- Entry: `BimZoneKind.Resolve(string entityType, Op key)` is the strict VIEW-side lookup interpreting a stamped grouping `Classification` code — INGRESS classification is the projector's ONE permissive `IfcClass.TryGet(...).IfNone(BuildingElementProxy)` classifier over the generated roster (grouping entities included), so this vocabulary never runs at ingest; it resolves the code for the zone view, the legality `IsSpatial` join, and the membership dispatch, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` `zone-class-miss` BARE on a grouping class the vocabulary omits; a permissive view read is `BimZoneKind.TryGet(entityType).IfNone(Group)` so a genuinely-unrostered future `IfcGroup` subtype lands the base `Group` row, the two paths sharing the ONE `Option`-lift `TryGet`. There is NO grouping egress gate: the predefined/window admission at `Projection/egress#IFC_EGRESS` `Emit` is the ONE `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token gate [C6][H8] over the roster row every node — grouping or placeable — resolves, so a 4x3-only `IfcSpatialZone` `COMPARTMENT` on an IFC4 emit and an `IfcBuiltSystem` on an IFC2x3 emit fault through the same typed arms every element does. `ZoneProjection.Of(ElementGraph graph, Node.Object group)` is the read fold resolving one grouping `Object` node into a typed `BimZone` — total, `Option<T>` (`None` when the node is not an `"ifc"`-classified grouping class — the `Classification.System` gate keeps a foreign classification system from key-colliding into the grouping vocabulary — so a non-grouping `Object` is skipped not mis-projected) — and `ZoneProjection.All(ElementGraph graph)` folds every grouping `Object` in the graph into the `Seq<BimZone>` the analysis/systems consumers read; the reads carry NO `Fin` rail because the projector already classified and admitted at ingest, so the view resolves the kind through the total `TryGet` and reads the already-admitted `PredefinedType`. The ANALYSIS surface rides the same owner: `ZoneProjection.Aggregate(graph, zone, source, key)` is the per-zone quantity rollup (the member set's effective values reduced through the `Model/query#ELEMENT_SET` `SumOf` composition — the only `Fin`-railed read, the seam `MeasureValue.Sum` cross-type guard), `ZoneProjection.Values(graph, zone, source)` the zone's own semantic read (a FIRESAFETY rating, a THERMAL setpoint, a load group's `Coefficient`), `ZoneProjection.Uncovered(graph, kind, candidate)` the coverage-gap audit (every candidate object no zone of the kind reaches), and `BimZone.SharedWith` the member-overlap adjacency.
- Auto: `Resolve` reads the SmartEnum table by key through `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcSpatialZone"`) rather than the `BimZoneKind` type itself, keeping the seam IFC-schema-free; the row's `Taxonomy` resolves its generated roster row through `IfcClass.TryGet(...).Match(...)` — the registry gate, so a grouping key the reflected roster does not carry (a rename like the pre-4.3 `IfcBuildingSystem`) dies loudly at first touch, and `Domain`/`Span`/`ValidPredefined` read that one row so a pin bump moves the grouping windows and token sets with zero edits here; `ZoneProjection.Of` gates the seam `Classification.System == "ifc"` first, resolves the kind through `BimZoneKind.TryGet(group.Classification.Code)`, and reads the members through `MembersOf`, which dispatches the `kind.IsSpatial` membership modality — a spatial grouping reads the `Compose.Reference` `Part`s whose `Whole` is the zone, every logical grouping the `Assign.Group` `Subject`s whose `Definition` is the group (the projector's INVERTED `Assign` lowers `IfcRelAssignsToGroup` to `Subject` = member, `Definition` = group, the same inversion the seam `Bake` and the `DefinesByType` fold read) — over the built-once `EdgesAt` incidence index in O(degree), the read pinning the group as the `Definition` (logical) / `Whole` (spatial) endpoint so a NESTED group (the group as a member of a parent group) never folds its parent into its own member set; `ParentsOf` reads the inverse endpoint (the zone as `Subject`) off the same index so the circuit→system nesting is one O(degree) read, a spatial zone's referenced-within-structure parents staying the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis; `ZoneProjection.All` folds `graph.ObjectNodes` through `Choose` discarding the non-grouping `Object`s, so a model carrying a wall and a fire zone indexes only the zone.
- Receipt: the typed `Seq<BimZone>` is the grouping evidence the `Model/structural#STRUCTURAL_PROJECTION` thermal-zone/load-group selection and the `Model/systems#CONNECTIVITY` MEP distribution-system grouping read by reference — never re-deriving the grouping graph per consumer — and each `BimZone` carries its full member `NodeId` set on one record: a fire compartment spanning three storeys, a thermal zone aggregating spaces across a building, an HVAC distribution system threading every air terminal, and a structural load group binding a set of members each one fold over the seam edges, the nested group's parent set riding the same record (`Parents` — a distribution circuit reads its owning system without a graph rescan); the `BimZone.Contains(member)` membership test is the group-centric dual of the `Model/query#ELEMENT_SET` element-centric `ByZone(group)` arm (both reading the SAME `Assign.Group`/`Compose.Reference` seam edges), and the resolved `BimZoneKind` is the typed grouping-class evidence on the record, never a stringly-typed relationship-name column; the zone AGGREGATE — a compartment's summed `GrossFloorArea`, a thermal zone's conditioned `NetVolume`, an electrical group's connected load — is the `Aggregate` rollup evidence the `Energy/derive` BIM-to-BEM lowering and the `Review/coordination#COORDINATION` rule engine consume, and the `Uncovered` gap set is the completeness receipt a code-compliance review reads first.
- Packages: GeometryGymIFC_Core (the grouping entity-class + predefined-enum vocabulary the rows ground against, consumed as settled data, never imported here), Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`AssignKind`/`ComposeKind`/`Classification`/`PredefinedType`/`SchemaSpan`/`ReleaseVersion`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new grouping family is one `(key, isSpatial)` row — every shared axis (domain, window, tokens) arrives from the roster through `Taxonomy`, so an IFC4.4 grouping lands one row here plus the emitter regeneration, and a genuinely-unrostered future/long-tail `IfcGroup` subtype rides the permissive `TryGet(...).IfNone(Group)` ingress until rostered; a new sub-kind token or schema window is the EMITTER's regeneration diff, zero edits here; a new per-zone binding is one column on `BimZone` read from the existing seam node/edges; a new zone analysis is one operation composing the query surface (`Aggregate`/`Uncovered` the standing exemplars over `ElementSet.SumOf`/`Query`); a new membership modality is the seam's concern — the algebra is closed at the two seam edge kinds (`Assign.Group`, `Compose.Reference`) the `IsSpatial` flag discriminates, so a new row is PURELY ADDITIVE data the `MembersOf` dispatch carries unchanged; never a per-zone-kind record, never a parallel `IfcGroup`/`IfcZone`/`IfcSystem` type family, never a `GetFireZones`/`GetByZoneKind` operation family, and never the retired `ZoneAssignment` union.
- Boundary: `BimZone` is ONE derived record discriminated by the `BimZoneKind` row data — a `FireZone`/`ThermalZone`/`MepSystem`/`LoadGroup` class family or fourteen sibling grouping types is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the two IFC membership relationships are the seam's neutral `Assign.Group`/`Compose.Reference` edges (the `Projection/relations#RELATION_ALGEBRA` projector lowers `IfcRelAssignsToGroup`→`Assign.Group` and `IfcRelReferencedInSpatialStructure`→`Compose.Reference`), so the retired `ZoneAssignment` `[Union]` (`AssignsToGroup`/`ReferencedInSpatialStructure`) — a Bim-side relationship case re-opening the IFC-schema strata leak the `Classification` collapse closed — is the deleted form [C5], and a typed `IfcRel*` case on this page is the named seam violation; the retired `BimModel.Zones` `Map<string,Seq<string>>` index, the `BimZone.IndexOf` fold, and the `IfcSemanticModel.ZoneRow` flat-row source are GONE — there is no `BimModel`, the membership reading the seam graph's built-once incidence index and the consumer `Element` deriving from the seam `Bake`; the grouping vocabulary keys on the LIVE GeometryGym type name (`IfcBuiltSystem`, NOT the pre-IFC4.3 `IfcBuildingSystem` STEP class name the class emits for older schemas) and the `Taxonomy` `TryGet(...).Match(...)` gate makes a row keyed on a non-rostered entity name die at first touch — the named-phantom guard is structural, not review discipline; the shared classification axes DERIVE from the one generated roster row and a `BimZoneKind` restating `Domain`/`Span`/`ValidPredefined` beside it is the deleted parallel-roster form (two sources that diverge under a pin bump split schema truth across owners), `BimZoneKind` keeping only the grouping semantics the element taxonomy cannot carry (`IsSpatial`, the `Group` permissive fallback, the `MembersOf` modality dispatch, the zone analysis surface); the grouping predefined/window egress is the ONE `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token gate and a kind-keyed second gate re-spelling the same admission idiom is the deleted parallel-gate form; the predefined token is the seam `Rasm.Element/Graph/element#NODE_MODEL` `PredefinedType` value-object and a Bim-owned `PredefinedType` type is the deleted form (the seam owns the value-object, the roster owns the valid-sets and the one gate); the overlay is HOST-NEUTRAL — it joins by stable seam `NodeId` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation, the orthogonal companion to the single-parent `Model/spatial#SPATIAL_STRUCTURE` `SpatialStructure` containment tree (an element's one `Compose.Contain` container versus its arbitrarily many groups, the two coexisting and never collapsed, the spatial-reference `IfcSpatialZone` this page's and the spatial-containment hierarchy the tree's over disjoint `Whole`-class sets); the `BimZone` view is the typed grouping projection the `analysis`/`systems` consumers read and the group-centric dual of the `Model/query#ELEMENT_SET` element-centric `ByZone` arm, re-deriving the grouping graph in any consumer the named cross-page drift; the `Parents` column reads only the logical `Assign.Group` `Subject` endpoint and a spatial-reference parent (a zone referenced within a storey) is the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis, never re-read here; a grouping rejection lifts `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop OR a hand-built `Error.New(2600, …)` bypassing the typed case is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The IFC grouping-INTERPRETATION vocabulary — the grouping overlay over the ONE generated Model/elements#IFC_CLASS
// taxonomy, keyed on the same seam Classification.Code. The rows own ONLY the semantics the element taxonomy cannot
// carry: the IsSpatial membership-modality flag and the Group permissive fallback; the classification axes the
// grouping entities SHARE with every rostered entity — IfcDomain, SchemaSpan, per-token PredefinedRow spans — DERIVE
// from the row's resolved Taxonomy (the generated roster covers every IfcObjectDefinition-rooted entity, the
// grouping family included), so schema truth has ONE source and a pin bump moves the grouping windows and token
// sets with zero edits here. The seam Object node carries only Classification("ifc", Key) + the typed
// PredefinedType token — never this type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BimZoneKind {
    // Logical groupings (members via IfcRelAssignsToGroup -> seam Assign.Group); IsSpatial: false. The retired
    // circuits (IfcElectricalCircuit/IfcCondition, [Obsolete] DEPRECATED IFC4) carry their RemovedIn=Ifc4 windows
    // on the ROSTER's Retirements overlay rows — admitted on an IFC2x3 round-trip, refused on any IFC4+ emit,
    // never degraded to Group and never a second window declared here.
    public static readonly BimZoneKind Group                   = new("IfcGroup",                   isSpatial: false);
    public static readonly BimZoneKind System                  = new("IfcSystem",                  isSpatial: false);
    public static readonly BimZoneKind Zone                    = new("IfcZone",                    isSpatial: false);
    public static readonly BimZoneKind BuiltSystem             = new("IfcBuiltSystem",             isSpatial: false);
    public static readonly BimZoneKind DistributionSystem      = new("IfcDistributionSystem",      isSpatial: false);
    public static readonly BimZoneKind DistributionCircuit     = new("IfcDistributionCircuit",     isSpatial: false);
    public static readonly BimZoneKind ElectricalCircuit       = new("IfcElectricalCircuit",       isSpatial: false);
    public static readonly BimZoneKind StructuralLoadGroup     = new("IfcStructuralLoadGroup",     isSpatial: false);
    public static readonly BimZoneKind StructuralLoadCase      = new("IfcStructuralLoadCase",      isSpatial: false);
    public static readonly BimZoneKind StructuralResultGroup   = new("IfcStructuralResultGroup",   isSpatial: false);
    public static readonly BimZoneKind StructuralAnalysisModel = new("IfcStructuralAnalysisModel", isSpatial: false);
    public static readonly BimZoneKind Inventory               = new("IfcInventory",               isSpatial: false);
    public static readonly BimZoneKind Asset                   = new("IfcAsset",                   isSpatial: false);
    public static readonly BimZoneKind Condition               = new("IfcCondition",               isSpatial: false);
    // Spatial-reference grouping (members via IfcRelReferencedInSpatialStructure -> seam Compose.Reference).
    public static readonly BimZoneKind SpatialZone             = new("IfcSpatialZone",             isSpatial: true);

    public bool IsSpatial { get; }

    // The generated-roster row this grouping key interprets — the registry gate makes a grouping key absent from
    // the reflected roster die loudly at first touch, never become a silent phantom row. The shared
    // classification axes (Domain, Span, ValidPredefined) and the per-token egress gate all read THIS row, so the
    // grouping overlay can never drift from the emitter's committed schema truth.
    public IfcClass Taxonomy => IfcClass.TryGet(Key).Match(
        Some: static row => row,
        None: () => throw new InvalidOperationException($"<zone-taxonomy-miss:{Key}>"));

    public IfcDomain Domain => Taxonomy.Domain;

    // The ONE Option-lift over the generated bool TryGet(string?, out BimZoneKind?) — the settled idiom
    // elements.md/spatial.md declare; every Option-form read (the view resolve, the legality IsSpatial join,
    // the permissive TryGet(...).IfNone(Group)) rides this overload, never a phantom generated member.
    public static Option<BimZoneKind> TryGet(string entityType) =>
        TryGet(entityType, out BimZoneKind? row) && row is { } hit ? Some(hit) : None;

    // The strict VIEW-side lookup: entity-type string -> the grouping-interpretation row. INGRESS classification is
    // the projector's ONE permissive IfcClass classifier over the generated roster (grouping entities included) —
    // this vocabulary interprets the stamped Classification code for the zone view and the legality IsSpatial join,
    // never a second ingress classifier and never a second egress gate: the grouping predefined/window egress rides
    // the ONE IfcClass.AdmitPredefined over the roster row every node resolves. zone-class-miss BARE on a grouping
    // class the vocabulary omits; a permissive view read is TryGet(entityType).IfNone(Group).
    public static Fin<BimZoneKind> Resolve(string entityType, Op key) =>
        TryGet(entityType).ToFin(new BimFault.UnmappedClass(key, $"zone-class-miss:{entityType}"));
}

// --- [MODELS] -----------------------------------------------------------------------------
// The derived group-centric grouping overlay: ONE record per grouping Object node carrying the resolved kind, the
// admitted predefined sub-kind, the IFC GlobalId, the resolved member NodeId set, and the parent logical-group set
// (the zone's own Assign.Group memberships — the IfcRelAssignsToGroup group-in-group nesting: a distribution circuit
// inside its system, a zone grouping zones). A DERIVED projection over the seam ElementGraph, never a stored record.
public sealed record BimZone(
    NodeId Id,
    BimZoneKind Kind,
    string Name,
    PredefinedType Predefined,
    Option<string> ExternalId,
    Seq<NodeId> Members,
    Seq<NodeId> Parents) {
    public bool IsSpatial => Kind.IsSpatial;
    public IfcDomain Domain => Kind.Domain;
    public int Count => Members.Count;
    public bool Contains(NodeId member) => Members.Exists(m => m == member);

    // Zone-to-zone adjacency by member overlap — two fire compartments sharing a shaft, a thermal zone overlapping
    // an occupancy zone; the boundary-element adjacency composes the spatial view's Separations rolled up over the
    // member spaces, never re-derived here.
    public Seq<NodeId> SharedWith(BimZone other) => Members.Filter(other.Contains);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ZoneProjection {
    // The read fold: a grouping Object node + its incident membership edges -> one typed BimZone. TOTAL — the
    // projector already classified and admitted at ingest; None when the node is not an "ifc"-classified grouping
    // class (a wall, a foreign classification system), so a non-grouping Object is skipped rather than mis-projected.
    public static Option<BimZone> Of(ElementGraph graph, Node.Object group) =>
        group.Classification.System == "ifc"
            ? BimZoneKind.TryGet(group.Classification.Code)
                .Map(kind => new BimZone(
                    group.Id, kind, group.Name, group.PredefinedType, group.ExternalId,
                    MembersOf(graph, group.Id, kind),
                    ParentsOf(graph, group.Id)))
            : None;

    // Every grouping Object in the graph folded into the typed zone family the Model/structural#STRUCTURAL_PROJECTION
    // thermal-zone/load-group selection and the Model/systems#CONNECTIVITY MEP-system grouping read by reference —
    // total, the non-grouping Objects discarded through Choose, never a per-consumer re-projection of the graph.
    public static Seq<BimZone> All(ElementGraph graph) =>
        graph.ObjectNodes.Choose(o => Of(graph, o));

    // The members a grouping node binds, dispatched on the kind's membership modality: a spatial grouping
    // (IfcSpatialZone) reads the Compose.Reference parts (IfcRelReferencedInSpatialStructure), every logical grouping
    // the Assign.Group Subjects whose Definition is the group (IfcRelAssignsToGroup, the projector's INVERTED Assign:
    // member -> Subject, group -> Definition; the spatial Compose is non-inverted, Whole = zone) off BimZoneKind.IsSpatial
    // over the built-once EdgesAt incidence index in O(degree) — never a typed IfcRel* case, never an O(edges) rescan, and
    // (the read pins the group as the Definition (logical) / Whole (spatial) endpoint) never folding a parent group into its members.
    private static Seq<NodeId> MembersOf(ElementGraph graph, NodeId group, BimZoneKind kind) =>
        kind.IsSpatial
            ? toSeq(graph.EdgesAt(group)).Choose(e => e is Relationship.Compose c && c.SubKind == ComposeKind.Reference && c.Whole == group ? Some(c.Part) : Option<NodeId>.None)
            : toSeq(graph.EdgesAt(group)).Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Definition == group ? Some(a.Subject) : Option<NodeId>.None);

    // The zone's OWN parent logical groups — the Assign.Group edges where the zone is the Subject (member): the
    // IfcRelAssignsToGroup group-in-group nesting (circuit→system, zone→zone). A spatial zone's referenced-within-
    // structure parents are the Model/spatial#SPATIAL_STRUCTURE Referenced axis, never this column.
    private static Seq<NodeId> ParentsOf(ElementGraph graph, NodeId zone) =>
        toSeq(graph.EdgesAt(zone)).Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Subject == zone ? Some(a.Definition) : Option<NodeId>.None);

    // The per-zone quantity ROLLUP: the member set's effective values for one source reduced through the ONE
    // Model/query#ELEMENT_SET SumOf composition (the seam same-type MeasureValue.Sum reducer under the dimension
    // law) — GrossFloorArea over a fire compartment, NetVolume over a thermal zone, connected load over an
    // electrical group — railed (a cross-type mix rails the seam ElementFault.ValueRejected), None when no member
    // carries the source. The zone AGGREGATE every downstream discipline consumes, never the membership list.
    public static Fin<Option<MeasureValue>> Aggregate(ElementGraph graph, BimZone zone, ValueSource source, Op key) =>
        ElementSet.SumOf(graph, zone.Members, source, key);

    // The zone's OWN semantic read: the grouping node's effective attribute/property values through the ONE
    // ElementSet.ValuesOf exposure — a FIRESAFETY compartment's required rating, a THERMAL zone's setpoint, a
    // StructuralLoadGroup's Coefficient off its structural definition bag — never a re-derived bag merge.
    public static Seq<PropertyValue> Values(ElementGraph graph, BimZone zone, ValueSource source) =>
        graph.Find<Node.Object>(zone.Id).ToSeq().Bind(o => ElementSet.ValuesOf(graph, o, source));

    // The coverage-gap AUDIT — the completeness question a code reviewer asks first: every candidate object (an
    // ElementPredicate — every IfcSpace, every storey) reached by NO zone of the kind, so an IfcSpace uncovered by
    // any THERMAL IfcSpatialZone or a storey area outside every FIRESAFETY compartment surfaces as a typed gap
    // set. TOTAL; the candidate selection rides the one query algebra, never a second selection surface.
    public static Seq<NodeId> Uncovered(ElementGraph graph, BimZoneKind kind, ElementPredicate candidate) {
        LanguageExt.HashSet<NodeId> covered = toHashSet(All(graph).Filter(zone => zone.Kind == kind).Bind(static zone => zone.Members));
        return ElementSet.Query(graph, candidate).Ids.Filter(id => !covered.Contains(id)).ToSeq();
    }

    // Boundary-derived zone adjacency: each space separation is lifted through the zone membership index, then
    // emitted once per unordered zone pair. Member overlap and geometric adjacency remain distinct projections.
    public static Seq<(NodeId ZoneA, NodeId ZoneB, NodeId Separator)> Adjacencies(
        SpatialStructure spatial,
        Seq<BimZone> zones) {
        Map<NodeId, Seq<NodeId>> byMember = zones
            .Bind(zone => zone.Members.Map(member => (Member: member, Zone: zone.Id)))
            .Fold(Map<NodeId, Seq<NodeId>>(), static (map, row) => map.AddOrUpdate(
                row.Member,
                map.Find(row.Member).IfNone(Seq<NodeId>()).Add(row.Zone)));
        return spatial.Separations()
            .Bind(separation =>
                from first in byMember.Find(separation.SpaceA).IfNone(Seq<NodeId>())
                from second in byMember.Find(separation.SpaceB).IfNone(Seq<NodeId>())
                where string.CompareOrdinal(first.Value, second.Value) < 0
                select (ZoneA: first, ZoneB: second, Separator: separation.Separator))
            .Distinct()
            .ToSeq();
    }
}
```

## [03]-[RESEARCH]

- [GROUPING_VOCABULARY]: `BimZoneKind` spans the exported grouping hierarchy plus `IfcSpatialZone`, and `Taxonomy` derives every class window and predefined set from generated `IfcClass`. The runtime key is the resolvable entity type, so `IfcBuiltSystem` owns the built-system row and the phantom `IfcBuildingSystem` key is absent.
- [MEMBERSHIP_EDGE]: the two membership modalities ground against the seam `Rasm.Element/Relations/relation#EDGE_ALGEBRA` algebra and `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` lowers `IfcRelAssignsToGroup` (relating `RelatingGroup` → related `RelatedObjects`) through the INVERTED `Assign` onto `Assign.Group` edges (`Subject` = member, `Definition` = group — the seam `Assign` is occurrence→definition, the inverse of the IFC relating→related, the same inversion the `DefinesByType` fold and the seam `Bake` read) and `IfcRelReferencedInSpatialStructure` (relating `RelatingStructure` → related `RelatedElements`) through the non-inverted `Compose` onto `Compose.Reference` edges (`Whole` = spatial zone, `Part` = member), the IFC wire-name/directionality riding the projector's `IfcRelKind` roster, NOT a seam `IfcRel*` case; the `Compose.Reference` mapping is the seam's `ComposeKind.Reference` "non-owning spatial reference" slot the `Model/spatial#SPATIAL_STRUCTURE` `SpatialStructure` `Referenced` axis also reads (over spatial-STRUCTURE `Whole`s — `IfcSite`/`IfcBuilding`/`IfcBuildingStorey`), this page reading it over the spatial-ZONE `Whole` (`IfcSpatialZone`, absent from the `SpatialClass` roster), the two disjoint `Whole`-class sets never overlapping; the directional filter (the group pinned as the `Definition` for the logical `Assign.Group` and the `Whole` for the spatial `Compose.Reference`, the member read as the `Subject`/`Part`) is the verified single-edge invariant so a group nested as a member of a parent group folds its own members and never its parent; the group-centric `BimZone.Contains` and the element-centric `Model/query#ELEMENT_SET` `ByZone` arm read the SAME edges, the arm matching both membership edge kinds incident to the element with the element pinned as the `Subject`/`Part` endpoint, the view collecting the members incident to a group through the directional read; the `Parents` column is the same edge set read from the `Subject` endpoint — one incidence index, three directional reads.
- [PROJECTOR_COMPOSITION]: the projector's ingress classification is ONE permissive `IfcClass` classifier over the generated roster (grouping entities included) stamping the generic `Classification("ifc", code)` and the typed `PredefinedType` on the seam `Object` node, and the egress `Projection/egress#IFC_EGRESS` `Emit` runs the ONE `IfcClass.AdmitPredefined` per-token valid-set + span gate over every node's resolved roster row before authoring the IFC entity [C6][H8] — the grouping family passes through the same typed arms every element does, so no `zone-*`-prefixed second fault vocabulary exists; `BimZoneKind.Resolve` is the VIEW-side interpretation lookup producing the `zone-class-miss` arm the `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` band enumerates, lifted BARE because the band is `Expected`-derived (the typed case IS the `Error`); the read `ZoneProjection.Of`/`All` carry no `Fin` rail because the rejection already lowered at the projector gate, the grouping `Object` node already classified and admitted, so the view is total over the already-validated graph — the same total-read / Fin-gate split the `Model/query#ELEMENT_SET` `Query`/`Bake` and `Model/spatial#SPATIAL_STRUCTURE` `Walk`/`Of` owners hold, `Aggregate` alone riding `Fin` for the seam `MeasureValue.Sum` cross-type guard.
