# [BIM_ZONE_GRAPH]

`BimZone` is the Bim grouping VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — the IFC many-to-many overlay DERIVED from the seam's neutral `Assign`/`Compose` edges, never a parallel union nor a second stored record. This page owns the IFC GROUPING INTERPRETATION the seam is blind to: `BimZoneKind` the vocabulary and DECLARING owner of every grouping class row, `BimGroupFamily` its partition, `ZoneProjection` the resolving fold. It companions the `Model/spatial#SPATIAL_STRUCTURE` tree orthogonally — one container per element against many groups, two algebras never collapsed.

Composition arrives settled. `Projection/semantic#SEMANTIC_PROJECTOR` classifies grouping entities as `Object` nodes carrying `Classification("ifc", code)`; `Projection/relations#RELATION_ALGEBRA` lowered the two membership edges and owns the `IfcRel*` roster [NEUTRAL_EDGE_RULING]; `Model/elements#IFC_CLASS` owns every classification axis `Taxonomy` derives. `Model/structural#STRUCTURAL_PROJECTION` and `Model/systems#CONNECTIVITY` select grouping nodes through the `BimGroupFamily` partition declared here, and `Model/query#ELEMENT_SET` `ByZone` is the element-centric dual.

## [01]-[INDEX]

- [02]-[ZONE_GRAPH]: `BimZoneKind` the grouping vocabulary and `BimGroupFamily` its partition (the `IsSpatial` modality deriving from it, `Domain`/`Span`/`ValidPredefined` deriving from the roster row through `Taxonomy`, the strict `Resolve` beside the permissive `TryGet(...).IfNone(Group)`), `BimZone` the group-centric overlay record, and the `ZoneProjection.Of`/`All`/`Closure`/`Aggregate`/`Values`/`Uncovered`/`Adjacencies` surface.

## [02]-[ZONE_GRAPH]

- Owner: `BimZoneKind` is the DECLARING owner of every IFC grouping class row, keyed by seam `Classification.Code`; each row carries its `BimGroupFamily` partition and derives domain, schema span, and predefined tokens from `IfcClass`. `BimGroupFamily` is the semantic axis a sibling view selects grouping nodes by, and the `Compose.Reference`-versus-`Assign.Group` membership modality derives from it. `BimZone` is the group-centric view over members and parent groups, and `ZoneProjection` owns its resolution, aggregation, coverage, and shared-membership operations.
- Cases: `BimZoneKind` rows close over the live `IfcGroup`/`IfcSystem` descendants beside the spatial `IfcSpatialZone`, the type-init census being the completeness gate. `Spatial` is the sole single-row family, so a fire/thermal `IfcSpatialZone` reads the `Compose.Reference` membership overlay where every other family reads `Assign.Group`. Everything schema-shaped is the roster's: the `Condition`/`ElectricalCircuit` `RemovedIn = Ifc4` retired windows ride the emitter `Retirements` overlay rows (superseded by `IfcDistributionCircuit` — admitted on an IFC2x3 round-trip, refused on any IFC4+ emit, never degraded to `Group`), the non-empty token sets (`IfcBuiltSystemTypeEnum`/`IfcDistributionSystemEnum`/`IfcLoadGroupTypeEnum`/`IfcAnalysisModelTypeEnum`/`IfcInventoryTypeEnum`/`IfcSpatialZoneTypeEnum`, the circuit and load-case inheriting their parent enum through the emitter's nearest-declared walk) commit as generated `PredefinedRow` spans, and the `IFC4X4_DRAFT` members stay excluded by the `ReleaseMap` law until the released row lands. The membership modality is the two seam edge kinds (`Assign.Group` logical, `Compose.Reference` spatial), never a per-relationship Bim case.
- Entry: `BimZoneKind.Resolve(string entityType, Op key)` is the strict VIEW-side lookup interpreting a stamped grouping `Classification` code — INGRESS classification is the projector's ONE permissive `IfcClass.TryGet(...).IfNone(BuildingElementProxy)` classifier over the generated roster (grouping entities included), so this vocabulary never runs at ingest; it resolves the code for the zone view, the legality `IsSpatial` join, and the membership dispatch, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` `zone-class-miss` BARE on a grouping class the vocabulary omits; a permissive view read is `BimZoneKind.TryGet(entityType).IfNone(Group)` so a genuinely-unrostered future `IfcGroup` subtype lands the base `Group` row, the two paths sharing the ONE `Option`-lift `TryGet`. There is NO grouping egress gate: the predefined/window admission at `Projection/egress#IFC_EGRESS` `Emit` is the ONE `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token gate [PREDEFINED_TOKEN_RULING] over the roster row every node — grouping or placeable — resolves, so a 4x3-only `IfcSpatialZone` `COMPARTMENT` on an IFC4 emit and an `IfcBuiltSystem` on an IFC2x3 emit fault through the same typed arms every element does. `ZoneProjection.Of(ElementGraph graph, Node.Object group)` is the read fold resolving one grouping `Object` node into a typed `BimZone` — total, `Option<T>` (`None` when the node is not an `"ifc"`-classified grouping class — the `Classification.System` gate keeps a foreign classification system from key-colliding into the grouping vocabulary — so a non-grouping `Object` is skipped not mis-projected) — and `ZoneProjection.All(ElementGraph graph)` folds every grouping `Object` in the graph into the `Seq<BimZone>` the analysis/systems consumers read; the reads carry NO `Fin` rail because the projector already classified and admitted at ingest, so the view resolves the kind through the total `TryGet` and reads the already-admitted `PredefinedType`. The ANALYSIS surface rides the same owner: `ZoneProjection.Closure(graph, zone)` is the transitive leaf closure — a member that is itself a group expands, cycle-guarded, so nesting reaches the held elements — and `ZoneProjection.Aggregate(graph, zone, source, key)` is the per-zone quantity rollup (the LEAF-closure set's effective values reduced through the `Model/query#ELEMENT_SET` `SumOf` composition — the only `Fin`-railed read, the seam `MeasureValue.Sum` cross-type guard), `ZoneProjection.Values(graph, zone, source)` the zone's own semantic read (a FIRESAFETY rating, a THERMAL setpoint, a load group's `Coefficient`), `ZoneProjection.Uncovered(graph, kind, candidate)` the coverage-gap audit (every candidate object no zone of the kind reaches), and `BimZone.SharedWith` the member-overlap adjacency.
- Auto: `Resolve` reads the SmartEnum table by key through `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcSpatialZone"`) rather than the `BimZoneKind` type itself, keeping the seam IFC-schema-free; the row's `Taxonomy` resolves its generated roster row through `IfcClass.TryGet(...).Match(...)` — the registry gate, so a grouping key the reflected roster does not carry (a rename like the pre-4.3 `IfcBuildingSystem`) dies loudly at first touch, and `Domain`/`Span`/`ValidPredefined` read that one row so a pin bump moves the grouping windows and token sets with zero edits here; `ZoneProjection.Of` gates the seam `Classification.System == "ifc"` first, resolves the kind through `BimZoneKind.TryGet(group.Classification.Code)`, and reads the members through `MembersOf`, which dispatches the `kind.IsSpatial` membership modality — a spatial grouping reads the `Compose.Reference` `Part`s whose `Whole` is the zone, every logical grouping the `Assign.Group` `Subject`s whose `Definition` is the group (the projector's INVERTED `Assign` lowers `IfcRelAssignsToGroup` to `Subject` = member, `Definition` = group, the same inversion the seam `Bake` and the `DefinesByType` fold read) — over the built-once `EdgesAt` incidence index in O(degree), the read pinning the group as the `Definition` (logical) / `Whole` (spatial) endpoint so a NESTED group (the group as a member of a parent group) never folds its parent into its own member set; `ParentsOf` reads the inverse endpoint (the zone as `Subject`) off the same index so the circuit→system nesting is one O(degree) read, a spatial zone's referenced-within-structure parents staying the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis; `ZoneProjection.All` folds `graph.ObjectNodes` through `Choose` discarding the non-grouping `Object`s, so a model carrying a wall and a fire zone indexes only the zone.
- Receipt: the typed `Seq<BimZone>` is the grouping evidence the `Model/structural#STRUCTURAL_PROJECTION` thermal-zone/load-group selection and the `Model/systems#CONNECTIVITY` MEP distribution-system grouping read by reference — never re-deriving the grouping graph per consumer — and each `BimZone` carries its full member `NodeId` set on one record: a fire compartment spanning three storeys, a thermal zone aggregating spaces across a building, an HVAC distribution system threading every air terminal, and a structural load group binding a set of members each one fold over the seam edges, the nested group's parent set riding the same record (`Parents` — a distribution circuit reads its owning system without a graph rescan); the `BimZone.Contains(member)` membership test is the group-centric dual of the `Model/query#ELEMENT_SET` element-centric `ByZone(group)` arm (both reading the SAME `Assign.Group`/`Compose.Reference` seam edges), and the resolved `BimZoneKind` is the typed grouping-class evidence on the record, never a stringly-typed relationship-name column; the zone AGGREGATE — a compartment's summed `GrossFloorArea`, a thermal zone's conditioned `NetVolume`, an electrical group's connected load — is the `Aggregate` rollup evidence the `Energy/derive` BIM-to-BEM lowering and the `Review/coordination#COORDINATION` rule engine consume, and the `Uncovered` gap set is the completeness receipt a code-compliance review reads first; the grouping node is also the `Energy/results#RESULTS_ADMISSION` Zone-scope bag target, so a run's per-zone energy results land on the overlay through the ordinary `Assign.PropertyDefinition` edge with zero zones-side edits.
- Packages: GeometryGymIFC_Core (the grouping entity-class + predefined-enum vocabulary the rows ground against, consumed as settled data, never imported here), Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`AssignKind`/`ComposeKind`/`Classification`/`PredefinedType`/`SchemaSpan`/`ReleaseVersion`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new grouping class is one `(key, family)` row — every shared axis (domain, window, tokens) arrives from the roster through `Taxonomy`, so an IFC4.4 grouping lands one row here and one emitter regeneration, and a genuinely-unrostered future/long-tail `IfcGroup` subtype rides the permissive `TryGet(...).IfNone(Group)` ingress until rostered; a new semantic partition a sibling view selects on is one `BimGroupFamily` row, and every consumer of an existing partition picks up a new class row with ZERO edits; a new sub-kind token or schema window is the EMITTER's regeneration diff, zero edits here; a new per-zone binding is one column on `BimZone` read from the existing seam node/edges; a new zone analysis is one operation composing the query surface (`Aggregate`/`Uncovered` the standing exemplars over `ElementSet.SumOf`/`Query`); a new membership modality is the seam's concern — the algebra is closed at the two seam edge kinds (`Assign.Group`, `Compose.Reference`) the derived `IsSpatial` discriminates, so a new row is PURELY ADDITIVE data the `MembersOf` dispatch carries unchanged; never a per-zone-kind record, never a parallel `IfcGroup`/`IfcZone`/`IfcSystem` type family, never a `GetFireZones`/`GetByZoneKind` operation family, and never the retired `ZoneAssignment` union.
- Boundary: `BimZone` is ONE derived record discriminated by the `BimZoneKind` row data — a `FireZone`/`ThermalZone`/`MepSystem`/`LoadGroup` class family, or one sibling type per grouping row, is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; this page is the DECLARING owner of the grouping class rows and their `BimGroupFamily` partition, so a sibling view selecting grouping nodes composes `BimZoneKind`/`BimGroupFamily` and a private entity-name `FrozenSet` at a consumer is the deleted form — it silently omits every row landing here afterwards; the two IFC membership relationships are the seam's neutral `Assign.Group`/`Compose.Reference` edges (the `Projection/relations#RELATION_ALGEBRA` projector lowers `IfcRelAssignsToGroup`→`Assign.Group` and `IfcRelReferencedInSpatialStructure`→`Compose.Reference`), so the retired `ZoneAssignment` `[Union]` (`AssignsToGroup`/`ReferencedInSpatialStructure`) — a Bim-side relationship case re-opening the IFC-schema strata leak the `Classification` collapse closed — is the deleted form [NEUTRAL_EDGE_RULING], and a typed `IfcRel*` case on this page is the named seam violation; the retired `BimModel.Zones` `Map<string,Seq<string>>` index, the `BimZone.IndexOf` fold, and the `IfcSemanticModel.ZoneRow` flat-row source are GONE — there is no `BimModel`, the membership reading the seam graph's built-once incidence index and the consumer `Element` deriving from the seam `Bake`; the grouping vocabulary keys on the LIVE GeometryGym type name (`IfcBuiltSystem`, NOT the pre-IFC4.3 `IfcBuildingSystem` STEP class name the class emits for older schemas) and the `Taxonomy` `TryGet(...).Match(...)` gate makes a row keyed on a non-rostered entity name die at first touch — the named-phantom guard is structural, not review discipline; the shared classification axes DERIVE from the one generated roster row and a `BimZoneKind` restating `Domain`/`Span`/`ValidPredefined` beside it is the deleted parallel-roster form (two sources that diverge under a pin bump split schema truth across owners), `BimZoneKind` keeping only the grouping semantics the element taxonomy cannot carry (the `BimGroupFamily` partition with `IsSpatial` derived from it, the `Group` permissive fallback, the `MembersOf` modality dispatch, the zone analysis surface) — a stored `IsSpatial` column beside the family is the deleted form that can disagree with the axis it restates; the grouping predefined/window egress is the ONE `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token gate and a kind-keyed second gate re-spelling the same admission idiom is the deleted parallel-gate form; the predefined token is the seam `Rasm.Element/Graph/element#NODE_MODEL` `PredefinedType` value-object and a Bim-owned `PredefinedType` type is the deleted form (the seam owns the value-object, the roster owns the valid-sets and the one gate); the overlay is HOST-NEUTRAL — it joins by stable seam `NodeId` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation, the orthogonal companion to the single-parent `Model/spatial#SPATIAL_STRUCTURE` `SpatialStructure` containment tree (an element's one `Compose.Contain` container versus its arbitrarily many groups, the two coexisting and never collapsed, the spatial-reference `IfcSpatialZone` this page's and the spatial-containment hierarchy the tree's over disjoint `Whole`-class sets); the `BimZone` view is the typed grouping projection the `analysis`/`systems` consumers read and the group-centric dual of the `Model/query#ELEMENT_SET` element-centric `ByZone` arm, re-deriving the grouping graph in any consumer the named cross-page drift; the `Parents` column reads only the logical `Assign.Group` `Subject` endpoint and a spatial-reference parent (a zone referenced within a storey) is the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis, never re-read here; a grouping rejection lifts `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop OR a hand-built `Error.New(2600, …)` bypassing the typed case is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Frozen;
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
// Grouping-family PARTITION: the semantic axis every consumer of this vocabulary selects its grouping nodes by
// — the Model/systems#CONNECTIVITY distribution network folds Distribution, the Model/structural
// #STRUCTURAL_PROJECTION analysis selection folds Structural. The partition is DECLARED here because the class rows
// are, so a sibling composes `BimZoneKind.TryGet(code).Exists(k => k.Family == …)` instead of re-spelling a private
// entity-name FrozenSet that silently omits every row landing here afterwards. It is also the membership modality:
// Spatial is exactly the family whose members ride Compose.Reference, so IsSpatial DERIVES rather than storing a
// second column that can disagree with the family it restates.
[SmartEnum<string>]
public sealed partial class BimGroupFamily {
    public static readonly BimGroupFamily Logical      = new("logical");       // the unpartitioned grouping base and its zone peer
    public static readonly BimGroupFamily Distribution = new("distribution");  // IfcSystem and its built/distribution/circuit descendants
    public static readonly BimGroupFamily Structural   = new("structural");    // the analysis load, result, and model groups
    public static readonly BimGroupFamily Asset        = new("asset");         // inventory, asset, and condition holdings
    public static readonly BimGroupFamily Spatial      = new("spatial");       // IfcSpatialZone — the Compose.Reference membership modality
}

// The IFC grouping-INTERPRETATION vocabulary — the grouping overlay over the ONE generated Model/elements#IFC_CLASS
// taxonomy, keyed on the same seam Classification.Code, and the DECLARING owner of the grouping class rows a
// sibling view composes. The rows own ONLY the semantics the element taxonomy cannot carry: the BimGroupFamily
// partition (the membership modality deriving from it) and the Group permissive fallback; the classification axes
// the grouping entities SHARE with every rostered entity — IfcDomain, SchemaSpan, per-token PredefinedRow spans —
// DERIVE from the row's resolved Taxonomy (the generated roster covers every IfcObjectDefinition-rooted entity, the
// grouping family included), so schema truth has ONE source and a pin bump moves the grouping windows and token
// sets with zero edits here. The seam Object node carries only Classification("ifc", Key) + the typed
// PredefinedType token — never this type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BimZoneKind {
    // Retired circuits (IfcElectricalCircuit/IfcCondition) carry their RemovedIn=Ifc4 windows on the ROSTER's
    // Retirements overlay rows — admitted on an IFC2x3 round-trip, refused on any IFC4+ emit, never degraded to
    // Group and never a second window declared here. The superseded circuit stays in the Distribution family:
    // a 2x3 model's electrical circuit IS a distribution grouping to every consumer.
    public static readonly BimZoneKind Group                   = new("IfcGroup",                   BimGroupFamily.Logical);
    public static readonly BimZoneKind Zone                    = new("IfcZone",                    BimGroupFamily.Logical);
    public static readonly BimZoneKind System                  = new("IfcSystem",                  BimGroupFamily.Distribution);
    public static readonly BimZoneKind BuiltSystem             = new("IfcBuiltSystem",             BimGroupFamily.Distribution);
    public static readonly BimZoneKind DistributionSystem      = new("IfcDistributionSystem",      BimGroupFamily.Distribution);
    public static readonly BimZoneKind DistributionCircuit     = new("IfcDistributionCircuit",     BimGroupFamily.Distribution);
    public static readonly BimZoneKind ElectricalCircuit       = new("IfcElectricalCircuit",       BimGroupFamily.Distribution);
    public static readonly BimZoneKind StructuralLoadGroup     = new("IfcStructuralLoadGroup",     BimGroupFamily.Structural);
    public static readonly BimZoneKind StructuralLoadCase      = new("IfcStructuralLoadCase",      BimGroupFamily.Structural);
    public static readonly BimZoneKind StructuralResultGroup   = new("IfcStructuralResultGroup",   BimGroupFamily.Structural);
    public static readonly BimZoneKind StructuralAnalysisModel = new("IfcStructuralAnalysisModel", BimGroupFamily.Structural);
    public static readonly BimZoneKind Inventory               = new("IfcInventory",               BimGroupFamily.Asset);
    public static readonly BimZoneKind Asset                   = new("IfcAsset",                   BimGroupFamily.Asset);
    public static readonly BimZoneKind Condition               = new("IfcCondition",               BimGroupFamily.Asset);
    public static readonly BimZoneKind SpatialZone             = new("IfcSpatialZone",             BimGroupFamily.Spatial);

    public BimGroupFamily Family { get; }

    // Members ride IfcRelReferencedInSpatialStructure -> seam Compose.Reference for exactly the spatial family,
    // IfcRelAssignsToGroup -> seam Assign.Group for every other. Derived, never a stored flag beside the family.
    public bool IsSpatial => Family == BimGroupFamily.Spatial;

    // Registry gate as a TYPE-INIT census, never a throwing getter on a total view: the census folds every row
    // against the generated roster once, so a grouping key the reflected roster does not carry (a rename like the
    // pre-4.3 IfcBuildingSystem) dies at first type touch, and Taxonomy is then a total frozen read. The shared
    // classification axes (Domain, Span, ValidPredefined) and the per-token egress gate all read THIS row, so the
    // grouping overlay can never drift from the emitter's committed schema truth.
    private static readonly FrozenDictionary<string, IfcClass> Census;

    static BimZoneKind() =>
        Census = Items.ToFrozenDictionary(
            static row => row.Key,
            static row => IfcClass.TryGet(row.Key).Match(
                Some: static roster => roster,
                None: () => throw new InvalidOperationException($"<zone-taxonomy-miss:{row.Key}>")),
            StringComparer.OrdinalIgnoreCase);

    public IfcClass Taxonomy => Census[Key];

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
    public BimGroupFamily Family => Kind.Family;
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

    // The transitive LEAF closure: a member that is itself a grouping node expands, a leaf collects — a fire
    // compartment grouping floor zones reaches the spaces those zones hold. Cycle-guarded by the threaded seen set
    // (a group cycle is malformed input the guard closes, never a hang). Direct membership stays BimZone.Members;
    // the rollup and the coverage audit read leaves, so nesting never zeroes an aggregate or blinds a gap audit.
    public static Seq<NodeId> Closure(ElementGraph graph, BimZone zone) =>
        Expand(graph, zone.Members, HashSet(zone.Id)).Leaves;

    // Each frontier member either expands (a nested grouping node) or collects (a leaf), the seen set threaded
    // through the fold. The nested descent recurses in the ARM BODY over the state the arm already advanced: a
    // `when` guard that runs the recursion to decide whether its own arm applies does the work in the guard and
    // discards it, so the pattern-match cost is the traversal itself rather than a test.
    private static (Seq<NodeId> Leaves, LanguageExt.HashSet<NodeId> Seen) Expand(
        ElementGraph graph, Seq<NodeId> frontier, LanguageExt.HashSet<NodeId> seen) =>
        frontier.Fold((Leaves: Seq<NodeId>(), Seen: seen), (state, member) =>
            state.Seen.Contains(member)
                ? state
                : graph.Find<Node.Object>(member).Bind(node => Of(graph, node)).Match(
                    Some: nested => Expand(graph, nested.Members, state.Seen.Add(member)) switch {
                        var (leaves, descended) => (state.Leaves + leaves, descended),
                    },
                    None: () => (state.Leaves.Add(member), state.Seen.Add(member))));

    // The per-zone quantity ROLLUP: the LEAF-closure member set's effective values for one source reduced through
    // the ONE Model/query#ELEMENT_SET SumOf composition (the seam same-type MeasureValue.Sum reducer under the
    // dimension law) — GrossFloorArea over a fire compartment, NetVolume over a thermal zone, connected load over
    // an electrical group — railed (a cross-type mix rails the seam ElementFault.ValueRejected), None when no leaf
    // carries the source. A nested group contributes its held spaces, never a zero row for the sub-zone node.
    public static Fin<Option<MeasureValue>> Aggregate(ElementGraph graph, BimZone zone, ValueSource source, Op key) =>
        ElementSet.SumOf(graph, Closure(graph, zone), source, key);

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
        LanguageExt.HashSet<NodeId> covered = toHashSet(All(graph).Filter(zone => zone.Kind == kind).Bind(zone => Closure(graph, zone)));
        return ElementSet.Query(graph, candidate).Ids.Filter(id => !covered.Contains(id)).ToSeq();
    }

    // Boundary-derived zone adjacency: each space separation is lifted through the zone membership index, then
    // emitted once per unordered zone pair. Member overlap and geometric adjacency remain distinct projections.
    // The index keys on the LEAF Closure, not direct Members: a fire compartment grouping floor zones holds its
    // spaces one level down, so a direct-membership index reports every nested compartment non-adjacent to
    // everything — the same nesting blindness Aggregate and Uncovered already read Closure to avoid.
    public static Seq<(NodeId ZoneA, NodeId ZoneB, NodeId Separator)> Adjacencies(
        ElementGraph graph,
        SpatialStructure spatial,
        Seq<BimZone> zones) {
        Map<NodeId, Seq<NodeId>> byMember = zones
            .Bind(zone => Closure(graph, zone).Map(member => (Member: member, Zone: zone.Id)))
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

(none)
