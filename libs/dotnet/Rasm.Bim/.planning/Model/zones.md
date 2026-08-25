# [BIM_ZONE_GRAPH]

`BimZone` is the Bim grouping VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — the IFC many-to-many overlay DERIVED from the seam's neutral `Assign`/`Compose` edges, never a parallel union nor a second stored record. This page owns the IFC GROUPING INTERPRETATION the seam is blind to: `BimZoneKind` the vocabulary and DECLARING owner of every grouping class row, `BimGroupFamily` its partition, `ZoneProjection` the resolving fold. It companions the `Model/spatial#SPATIAL_STRUCTURE` tree orthogonally — one container per element against many groups, two algebras never collapsed.

Composition arrives settled. `Projection/semantic#SEMANTIC_PROJECTOR` classifies grouping entities as `Object` nodes carrying `Classification(IfcClass.System, code)`; `Projection/relations#RELATION_ALGEBRA` lowered the two membership edges and owns the `IfcRel*` roster [NEUTRAL_EDGE_RULING]; `Model/elements#IFC_CLASS` owns every classification axis `Taxonomy` derives. `Model/structural#STRUCTURAL_PROJECTION` and `Model/systems#CONNECTIVITY` select grouping nodes through the `BimGroupFamily` partition declared here, and `Model/query#ELEMENT_SET` `ByZone` is the element-centric dual.

## [01]-[INDEX]

- [02]-[ZONE_GRAPH]: `BimZoneKind` the grouping vocabulary and `BimGroupFamily` its partition (the `IsSpatial` modality deriving from it, `Taxonomy`/`Domain` deriving from the roster row, `Audit` the accumulating completeness rail beside the strict `Resolve` and the permissive `TryGet(...).IfNone(Group)`), `BimZone` the group-centric overlay record, and the `ZoneProjection.Of`/`All`/`Closure`/`Aggregate`/`Values`/`Uncovered`/`Adjacencies` surface.

## [02]-[ZONE_GRAPH]

- Owner: `BimZoneKind` is the DECLARING owner of every IFC grouping class row, keyed by seam `Classification.Code`; each row carries its `BimGroupFamily` partition and derives domain, schema span, and predefined tokens from `IfcClass`. `BimGroupFamily` is the semantic axis a sibling view selects grouping nodes by, and the `Compose.Reference`-versus-`Assign.Group` membership modality derives from it. `BimZone` is the group-centric view over members and parent groups, and `ZoneProjection` owns its resolution, aggregation, coverage, and shared-membership operations.
- Cases: `BimZoneKind` rows close over the live `IfcGroup`/`IfcSystem` descendants beside the spatial `IfcSpatialZone`. `Spatial` is the sole single-row family, so a fire/thermal `IfcSpatialZone` reads the `Compose.Reference` membership overlay where every other family reads `Assign.Group`. Everything schema-shaped is the roster's: the `Condition`/`ElectricalCircuit` `RemovedIn = Ifc4` retired windows ride the emitter `Retirements` overlay rows (superseded by `IfcDistributionCircuit` — admitted on an IFC2x3 round-trip, refused on any IFC4+ emit, never degraded to `Group`), the non-empty token sets (`IfcBuiltSystemTypeEnum`/`IfcDistributionSystemEnum`/`IfcLoadGroupTypeEnum`/`IfcAnalysisModelTypeEnum`/`IfcInventoryTypeEnum`/`IfcSpatialZoneTypeEnum`, the circuit and load-case inheriting their parent enum through the emitter's nearest-declared walk) commit as generated `PredefinedRow` spans, and the `IFC4X4_DRAFT` members stay excluded by the `ReleaseMap` law until the released row lands. The membership modality is the two seam edge kinds (`Assign.Group` logical, `Compose.Reference` spatial), never a per-relationship Bim case.
- Entry: `BimZoneKind.Resolve(string entityType, Op key)` is the strict VIEW-side lookup interpreting a stamped grouping `Classification` code — INGRESS classification is the projector's ONE permissive `IfcClass.TryGet(...).IfNone(BuildingElementProxy)` classifier over the generated roster (grouping entities included), so this vocabulary never runs at ingest; it resolves the code for the zone view, the legality `IsSpatial` join, and the membership dispatch, faulting `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Unmapped` `zone-class-miss` BARE on a grouping class the vocabulary omits; a permissive view read is `BimZoneKind.TryGet(entityType).IfNone(Group)` so a genuinely-unrostered future `IfcGroup` subtype lands the base `Group` row, the two paths sharing the ONE `Option`-lift `TryGet`. `BimZoneKind.Audit(Op key)` is the roster-completeness rail every composition runs once — it accumulates one `zone-class-miss` per row the generated taxonomy no longer carries, so a pin bump that renamed a grouping entity names EVERY drifted row at once. There is NO grouping egress gate: the predefined/window admission at `Projection/egress#IFC_EGRESS` `Emit` is the ONE `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token gate [PREDEFINED_TOKEN_RULING] over the roster row every node — grouping or placeable — resolves, so a 4x3-only `IfcSpatialZone` `COMPARTMENT` on an IFC4 emit and an `IfcBuiltSystem` on an IFC2x3 emit fault through the same typed arms every element does. `ZoneProjection.Of(ElementGraph graph, Node.Object group)` is the read fold resolving one grouping `Object` node into a typed `BimZone` — total, `Option<T>` (`None` when the node is not an `IfcClass.System`-classified grouping class, so a non-grouping `Object` is skipped rather than mis-projected) — and `ZoneProjection.All(ElementGraph graph)` folds every grouping `Object` in the graph into the `Seq<BimZone>` the analysis and systems consumers read; the reads carry NO `Fin` rail because the projector already classified and admitted at ingest. The ANALYSIS surface rides the same owner: `Closure(graph, zone)` is the transitive leaf closure, `Aggregate(graph, zone, source, key)` the per-zone quantity rollup over that closure (the only `Fin`-railed read, the seam `MeasureValue.Sum` cross-type guard), `Values(graph, zone, source)` the zone's own semantic read, `Uncovered(graph, kind, candidate)` the coverage-gap audit, `Adjacencies` the boundary-derived zone pairing, and `BimZone.SharedWith` the member-overlap adjacency.
- Law: schema truth has ONE source. `Taxonomy` resolves each row against the generated `IfcClass` roster and answers `Option`, `Domain` derives from it, and `Audit` is where a miss becomes a typed accumulated verdict — a type-init throw over that census took every consumer of the type down on a fact only the audit needed, so the completeness gate is a rail a composition reads and never a static constructor. `Domain`/`Span`/`ValidPredefined` read that one roster row, so a pin bump moves the grouping windows and token sets with zero edits here.
- Auto: `Resolve` reads the SmartEnum table by key through `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair rather than the `BimZoneKind` type itself, keeping the seam IFC-schema-free; `ZoneProjection.Of` gates the seam `Classification.System` against `IfcClass.System` under the roster's own `OrdinalIgnoreCase` key space, resolves the kind, and reads the members through `MembersOf`, which dispatches the `kind.IsSpatial` membership modality — a spatial grouping reads the `Compose.Reference` `Part`s whose `Whole` is the zone, every logical grouping the `Assign.Group` `Subject`s whose `Definition` is the group (the projector's INVERTED `Assign` lowers `IfcRelAssignsToGroup` to `Subject` = member, `Definition` = group, the same inversion the seam `Bake` and the `DefinesByType` fold read) — over the built-once `EdgesAt` incidence index in O(degree), the read pinning the group as the `Definition` (logical) / `Whole` (spatial) endpoint so a NESTED group never folds its parent into its own member set; `ParentsOf` reads the inverse endpoint off the same index so the circuit→system nesting is one O(degree) read, a spatial zone's referenced-within-structure parents staying the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis; membership TESTS ride the record's own derived hash index, so `Contains` is O(1) and a two-zone overlap is linear rather than the quadratic a `Seq` scan per probe made it; `ZoneProjection.All` folds `graph.ObjectNodes` through `Choose` discarding the non-grouping `Object`s.
- Output: the typed `Seq<BimZone>` is the grouping evidence the `Model/structural#STRUCTURAL_PROJECTION` thermal-zone/load-group selection and the `Model/systems#CONNECTIVITY` MEP distribution-system grouping read by reference — never re-deriving the grouping graph per consumer — and each `BimZone` carries its full member `NodeId` set on one record: a fire compartment spanning three storeys, a thermal zone aggregating spaces across a building, an HVAC distribution system threading every air terminal, and a structural load group binding a set of members each one fold over the seam edges, the nested group's parent set riding the same record so a distribution circuit reads its owning system without a graph rescan; the `BimZone.Contains(member)` membership test is the group-centric dual of the `Model/query#ELEMENT_SET` element-centric `ByZone(group)` arm (both reading the SAME seam edges), and the resolved `BimZoneKind` is the typed grouping-class evidence on the record, never a stringly-typed relationship-name column; the zone AGGREGATE — a compartment's summed `GrossFloorArea`, a thermal zone's conditioned `NetVolume`, an electrical group's connected load — is the rollup evidence the `Energy/derive` BIM-to-BEM lowering and the `Review/coordination#COORDINATION` rule engine consume, the `Uncovered` gap set the completeness verdict a code-compliance review reads first, and `Audit` the roster-conformance verdict a composition proves at boot; the grouping node is also the `Energy/results#RESULTS_ADMISSION` Zone-scope bag target, so a run's per-zone energy results land on the overlay through the ordinary `Assign.PropertyDefinition` edge with zero zones-side edits.
- Packages: GeometryGymIFC_Core (the grouping entity-class + predefined-enum vocabulary the rows ground against, consumed as settled data, never imported here), Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`AssignKind`/`ComposeKind`/`Classification`/`PredefinedType`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Option`/`Map`/`HashSet`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new grouping class is one `(key, family)` row — every shared axis arrives from the roster through `Taxonomy`, so an IFC4.4 grouping lands one row here and one emitter regeneration, and a genuinely-unrostered future or long-tail `IfcGroup` subtype rides the permissive `TryGet(...).IfNone(Group)` until rostered; a new semantic partition a sibling view selects on is one `BimGroupFamily` row, and every consumer of an existing partition picks up a new class row with ZERO edits; a new sub-kind token or schema window is the EMITTER's regeneration diff, zero edits here; a new per-zone binding is one column on `BimZone` read from the existing seam node and edges; a new zone analysis is one operation composing the query surface (`Aggregate`/`Uncovered` the standing exemplars over `ElementQuery.SumOf`/`Query`); a new membership modality is the seam's concern — the algebra is closed at the two seam edge kinds the derived `IsSpatial` discriminates, so a new row is PURELY ADDITIVE data the `MembersOf` dispatch carries unchanged; never a per-zone-kind record, never a parallel `IfcGroup`/`IfcZone`/`IfcSystem` type family, never a `GetFireZones`/`GetByZoneKind` operation family, and never the retired `ZoneAssignment` union.
- Boundary: `BimZone` is ONE derived record discriminated by the `BimZoneKind` row data — a `FireZone`/`ThermalZone`/`MepSystem`/`LoadGroup` class family, or one sibling type per grouping row, is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; this page is the DECLARING owner of the grouping class rows and their `BimGroupFamily` partition, so a sibling view selecting grouping nodes composes `BimZoneKind`/`BimGroupFamily` and a private entity-name `FrozenSet` at a consumer is the deleted form that silently omits every row landing here afterwards; the two IFC membership relationships are the seam's neutral `Assign.Group`/`Compose.Reference` edges, so the retired `ZoneAssignment` `[Union]` — a Bim-side relationship case re-opening the IFC-schema strata leak the `Classification` collapse closed — is the deleted form [NEUTRAL_EDGE_RULING], and a typed `IfcRel*` case on this page is the named seam violation; the retired `BimModel.Zones` `Map<string,Seq<string>>` index, the `BimZone.IndexOf` fold, and the `IfcSemanticModel.ZoneRow` flat-row source are GONE — there is no `BimModel`, the membership reading the seam graph's built-once incidence index and the consumer `Element` deriving from the seam `Bake`; the classification SYSTEM token is `IfcClass.System` read under the roster's `OrdinalIgnoreCase` key space, so a bare `"ifc"` literal beside a `==` is the deleted form that reads two systems where the roster declares one; the grouping vocabulary keys on the LIVE GeometryGym type name (`IfcBuiltSystem`, NOT the pre-IFC4.3 `IfcBuildingSystem` STEP class name the class emits for older schemas) and `Audit` is where a row keyed on a non-rostered entity name surfaces, accumulated rather than thrown; the shared classification axes DERIVE from the one generated roster row and a `BimZoneKind` restating `Domain`/`Span`/`ValidPredefined` beside it is the deleted parallel-roster form, `BimZoneKind` keeping only the grouping semantics the element taxonomy cannot carry — a stored `IsSpatial` column beside the family is the deleted form that can disagree with the axis it restates; the grouping predefined/window egress is the ONE `IfcClass.AdmitPredefined` per-token gate and a kind-keyed second gate re-spelling the same admission idiom is the deleted parallel-gate form; the predefined token is the seam `PredefinedType` value-object and a Bim-owned one is the deleted form; the overlay is HOST-NEUTRAL — it joins by stable seam `NodeId` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation, the orthogonal companion to the single-parent `Model/spatial#SPATIAL_STRUCTURE` containment tree (an element's one `Compose.Contain` container versus its arbitrarily many groups, the two coexisting and never collapsed, the spatial-reference `IfcSpatialZone` this page's and the spatial-containment hierarchy the tree's over disjoint whole-class sets); re-deriving the grouping graph in any consumer is the named cross-page drift; the `Parents` column reads only the logical `Assign.Group` `Subject` endpoint and a spatial-reference parent is the `Model/spatial#SPATIAL_STRUCTURE` `Referenced` axis, never re-read here; a grouping rejection lifts `BimFault.Refused` with `BimReason.Unmapped` BARE onto the rail and a `.ToError()` lowering hop OR a hand-built `Error.New(2600, …)` bypassing the typed case is the named defect.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BimGroupFamily {
    public static readonly BimGroupFamily Logical      = new("logical");
    public static readonly BimGroupFamily Distribution = new("distribution");
    public static readonly BimGroupFamily Structural   = new("structural");
    public static readonly BimGroupFamily Asset        = new("asset");
    public static readonly BimGroupFamily Spatial      = new("spatial");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BimZoneKind {
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

    public bool IsSpatial => Family == BimGroupFamily.Spatial;

    private static readonly Lazy<FrozenDictionary<string, IfcClass>> Rostered = new(
        static () => Items.Choose(static row => IfcClass.TryGet(row.Key).Map(taxonomy => (row.Key, Taxonomy: taxonomy)))
            .ToFrozenDictionary(static hit => hit.Key, static hit => hit.Taxonomy, StringComparer.OrdinalIgnoreCase),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public Option<IfcClass> Taxonomy => Rostered.Value.TryGetValue(Key, out IfcClass? row) ? Some(row) : None;

    public Option<IfcDomain> Domain => Taxonomy.Map(static row => row.Domain);

    public static Validation<Error, Unit> Audit(Op key) =>
        toSeq(Items).Traverse(row => row.Taxonomy.Match(
                Some: static _ => Success<Error, Unit>(unit),
                None: () => Fail<Error, Unit>(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "zone-class-miss", row.Key })))))
            .As().Map(static _ => unit);

    public static Option<BimZoneKind> TryGet(string entityType) =>
        TryGet(entityType, out BimZoneKind? row) && row is { } hit ? Some(hit) : None;

    public static Fin<BimZoneKind> Resolve(string entityType, Op key) =>
        TryGet(entityType).ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "zone-class-miss", entityType })));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BimZone(
    NodeId Id,
    BimZoneKind Kind,
    string Name,
    PredefinedType Predefined,
    Option<string> ExternalId,
    Seq<NodeId> Members,
    Seq<NodeId> Parents) {
    private LanguageExt.HashSet<NodeId> Index { get; } = toHashSet(Members);

    public BimGroupFamily Family => Kind.Family;
    public bool IsSpatial => Kind.IsSpatial;
    public Option<IfcDomain> Domain => Kind.Domain;
    public int Count => Members.Count;
    public bool Contains(NodeId member) => Index.Contains(member);

    public Seq<NodeId> SharedWith(BimZone other) => Members.Filter(other.Contains);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ZoneProjection {
    public static Option<BimZone> Of(ElementGraph graph, Node.Object group) =>
        StringComparer.OrdinalIgnoreCase.Equals(group.Classification.System, IfcClass.System)
            ? BimZoneKind.TryGet(group.Classification.Code)
                .Map(kind => new BimZone(
                    group.Id, kind, group.Name, group.PredefinedType, group.ExternalId,
                    MembersOf(graph, group.Id, kind),
                    ParentsOf(graph, group.Id)))
            : None;

    public static Seq<BimZone> All(ElementGraph graph) =>
        graph.ObjectNodes.Choose(o => Of(graph, o));

    private static Seq<NodeId> MembersOf(ElementGraph graph, NodeId group, BimZoneKind kind) =>
        kind.IsSpatial
            ? toSeq(graph.EdgesAt(group)).Choose(e => e is Relationship.Compose c && c.SubKind == ComposeKind.Reference && c.Whole == group ? Some(c.Part) : Option<NodeId>.None)
            : toSeq(graph.EdgesAt(group)).Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Definition == group ? Some(a.Subject) : Option<NodeId>.None);

    private static Seq<NodeId> ParentsOf(ElementGraph graph, NodeId zone) =>
        toSeq(graph.EdgesAt(zone)).Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Subject == zone ? Some(a.Definition) : Option<NodeId>.None);

    public static Seq<NodeId> Closure(ElementGraph graph, BimZone zone) =>
        Expand(graph, zone.Members, HashSet(zone.Id)).Leaves;

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

    public static Fin<Option<MeasureValue>> Aggregate(ElementGraph graph, BimZone zone, ValueSource source, Op key) =>
        ElementQuery.SumOf(graph, Closure(graph, zone), source, key);

    public static Seq<PropertyValue> Values(ElementGraph graph, BimZone zone, ValueSource source) =>
        graph.Find<Node.Object>(zone.Id).ToSeq().Bind(o => ElementQuery.ValuesOf(graph, o, source));

    public static Seq<NodeId> Uncovered(ElementGraph graph, BimZoneKind kind, BimTerm candidate) {
        LanguageExt.HashSet<NodeId> covered = toHashSet(All(graph).Filter(zone => zone.Kind == kind).Bind(zone => Closure(graph, zone)));
        return ElementQuery.Query(graph, candidate).Ids.Filter(id => !covered.Contains(id)).ToSeq();
    }

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
