# [BIM_SPATIAL_STRUCTURE]

The Bim spatial-structure VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`: the IFC project→site→building→storey→space hierarchy read as a DERIVED `SpatialStructure` over the seam's neutral `Compose` edges, never a parallel relationship union and never a second stored tree. The seam owns the property graph — the `Object` nodes, the neutral `Relations/relation#EDGE_ALGEBRA` `Relationship` algebra, and the `QuikGraph` topology view it materializes once at the read-snapshot freeze; this page owns the IFC SPATIAL INTERPRETATION the seam is deliberately blind to: the `SpatialClass` `[SmartEnum<string>]` spatial-container vocabulary (canonical nesting `Rank` + `SpatialRole`), the `SpatialStructure` derived spatial-tree view, and the ONE polymorphic `Walk(NodeId, SpatialAxis)` composing `QuikGraph` reachability with the seam incidence. The retired `BimAssembly`/`SpatialContainer`/`AssemblyRel` owners and the `IfcSemanticModel` flat-row source are GONE: the spatial relationships are the seam's `Compose.Aggregate` (spatial decomposition), `Compose.Contain` (single-parent element containment), and `Compose.Reference` (non-owning spatial reference) edges the `Projection/semantic#RELATION_ALGEBRA` projector lowered, and the IFC `IfcRel*` roster lives WHOLLY in that projector [C5] — so the spatial structure re-opens no IFC-schema strata on the seam and mints no second IFC relationship union. The view is the join surface the rest of the folder composes: the `Model/query#ELEMENT_SET` `BySpatialContainer` arm selects on the same single-parent `Compose.Contain` containment, the `Model/zones#ZONE_GRAPH` many-to-many overlay is its orthogonal companion, the `Review/validation#IDS_FACETS` `PartOf` facet and the `Review/diff#MODEL_DIFF` move detection walk the same `Walk`, and the `Semantics/georeference#GEO_REFERENCE` reconciliation anchors the resolved `Root` into one real-world frame.

## [01]-[INDEX]

- [01]-[SPATIAL_STRUCTURE]: the `SpatialClass` `[SmartEnum<string>]` spatial-container vocabulary (`Rank`/`SpatialRole`/`IsRoot`/`IsContainer`/`CanContain`) with the `Resolve` projector classification ingress, the `SpatialAxis` traversal discriminant, the `SpatialStructure` derived spatial-tree view folded from the seam's OWNING spatial `Compose` edges (`Aggregate`/`Contain`) into a `QuikGraph` `BidirectionalGraph`, the ONE polymorphic `Walk(NodeId, SpatialAxis)` traversal, and the `Container`/`Containment`/`Root`/`Level` accessors.

## [02]-[SPATIAL_STRUCTURE]

- Owner: `SpatialStructure` the derived spatial-tree view over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — it holds the seam graph (for node resolution and the non-owning `Reference` axis), a FOCUSED `QuikGraph` `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` folded from the OWNING spatial `Compose` edges (`Aggregate`/`Contain`), the content-resolved spatial `Root`, the root breadth-first path accessor, and the single-parent `Containment` index; `SpatialClass` the `[SmartEnum<string>]` spatial-container vocabulary keyed on the IFC spatial entity-type string (the seam `Classification.Code`), each row carrying its canonical nesting `Rank` and its `SpatialRole` (the `IfcProject` root that aggregates the site versus the `IfcSite`/`IfcBuilding`/`IfcFacility` (+ its concrete `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` infra subtypes)/`IfcBuildingStorey`/`IfcFacilityPart`/`IfcSpace` containers an element is contained in), and `Resolve(entityType, key)` the strict classification ingress the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes to stamp a spatial-structure entity the `Model/elements#IFC_CLASS` roster omits (faulting `BimFault.UnmappedClass` `spatial-class-miss` BARE); `SpatialAxis` the `[SmartEnum<string>]` traversal discriminant the one `Walk` routes on; `SpatialRole` the root/container partition. There is NO stored tree (the structure is a fold over the seam graph, rebuilt at the view boundary like the seam's own `QuikGraph` cache), NO parallel relationship union (the seam's neutral `Compose` owns the edges), and NO per-element spatial record (the `Object` node IS the spatial node).
- Entry: `SpatialStructure.Of(ElementGraph graph, Op key)` folds the graph's OWNING spatial `Compose` edges — every non-`Reference` `Compose` whose `Whole` resolves to a `SpatialClass` — into the `BidirectionalGraph` (tagged by `ComposeKind` so a traversal filters aggregate-vs-contain), builds the single-parent containment index from the `Compose.Contain` edges (the `Container` axis source), reads the non-owning `Compose.Reference` edges as a direct axis off the seam incidence, resolves the spatial root (the `Object` node whose `Classification.Code` resolves to a `SpatialClass.IsRoot` row, falling back to the `QuikGraph` `Roots()` no-incoming-edge spatial source), and returns `Fin<SpatialStructure>` faulting `Model/faults#FAULT_BAND` `BimFault.DanglingReference(key, "spatial-root-miss")` BARE (the band is `Expected`-derived, no `.ToError()` hop) when no root resolves; `Walk(NodeId from, SpatialAxis axis)` is the ONE polymorphic spatial walk over the complete decomposition graph — `Ancestors` the transitive container chain up to the root, `Descendants` the transitive sub-containers and contained elements, `Children` the direct sub-containers, `Contained` the directly contained elements, `Referenced` the non-owning spatial references, and `Container` the single owning container — never a `WalkAncestors`/`WalkDescendants`/`WalkContained` operation family; `Container(NodeId element)` resolves the single containment parent from the index, `Containment` exposes the whole element→container join, `Root` the spatial root, `Level(NodeId)` the node's resolved `SpatialClass`.
- Auto: `Of` folds the OWNING spatial edges ONCE through `Relationship.Compose` pattern matching keyed on the resolved `Whole` class (a `Compose.Aggregate` between containers, a `Compose.Contain` to an element) so the `BidirectionalGraph` carries only the owning spatial subgraph — a non-owning `Compose.Reference` never enters the transitive closure and a curtain-wall→panel aggregation whose `Whole` is a non-spatial element never enters it; `Walk` dispatches the seam-generated total `SpatialAxis.Switch` (compile-time exhaustive, no runtime `_` arm) — the transitive descendants arm runs `QuikGraph` `TreeBreadthFirstSearch(from)` and filters the vertices to the reachable closure, the transitive ancestors arm reads the SAME root breadth-first path accessor built once at `Of` (the path edges' `Source` vertices ARE the container chain, reversed nearest-first), the direct children/contained arms read the `ComposeKind`-tagged out-edges, the referenced arm reads the non-owning `Compose.Reference` edges off the seam incidence (`EdgesAt`), and the container arm reads the prebuilt single-parent index — one walk over the typed adjacency, never a per-direction recursion and never a mutable visited-set fold; the `Walk` guards a non-spatial `from` (`ContainsVertex`) returning the empty `Seq` so the traversal is total; `Container`/`Containment` read the prebuilt single-parent index so this view's container resolution is O(1) per element, never a per-call edge scan (the `Model/query#ELEMENT_SET` `BySpatialContainer` arm selects on the same `Compose.Contain` relation directly off the seam incidence).
- Packages: Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`ComposeKind`/`Classification`), QuikGraph (the `BidirectionalGraph`/`STaggedEdge` containers + the `AlgorithmExtensions` `TreeBreadthFirstSearch`/`Roots` facade), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new spatial-container level is one `SpatialClass` row carrying its rank and role (an IFC4.3 `IfcFacilityPart` subdivision rides the same fold); a new traversal direction is one `SpatialAxis` row and one `Walk` arm reading the same `STaggedEdge` adjacency; a new spatial-decomposition flavor is one `ComposeKind` the seam already carries and one tag filter the `Walk` reads; never a per-relationship `AssemblyRel` arm, never a per-direction `Traverse` method, and never a second stored spatial tree.
- Boundary: `SpatialStructure` is a VIEW over the seam graph — the retired `BimAssembly` stored tree, the `SpatialContainer` per-node record, the `AssemblyRel` `[Union]` mirroring the IFC `IfcRel*` decomposition relationships, and the `IfcSemanticModel` flat-row source are the deleted form [C5], because the seam carries the neutral `Compose` edge algebra and the IFC `IfcRel*` roster lives in `Projection/semantic#RELATION_ALGEBRA` — a typed `AssemblyRel.Aggregates`/`Nests`/`ContainedIn`/`Voids`/`Connects` union on this page re-opens the IFC-schema strata leak the `Classification` collapse closed and is the named seam violation; the decomposition graph is the seam's `Compose.Aggregate`/`Compose.Contain`/`Compose.Reference` edges (the projector lowers `IfcRelAggregates`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` onto them), so this view reads the seam's typed `ComposeKind`, never an IFC relationship name; the traversal composes `QuikGraph` (the shared `libs/csharp/.api` graph-algorithm owner the whole stack folds a transient graph into) — the hand-rolled breadth-first `Closure` fold over a `Map<string, Seq<string>>` adjacency with a mutated `HashSet<string>` visited set is the deleted form, the `Walk` reading the package `TreeBreadthFirstSearch` reachable closure and the root path accessor; the spatial-container vocabulary is the ONE `SpatialClass` owner — the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment-whole-must-be-spatial gate consumes `SpatialClass.IsContainer` rather than re-listing a private spatial `FrozenSet`, and the projector classifies a spatial-structure entity through `SpatialClass.Resolve` (the partition counterpart of `Model/elements#IFC_CLASS` `IfcClass.Resolve` and `Model/zones#ZONE_GRAPH` `BimZoneKind.Resolve`) — a duplicated private set or a parallel spatial classifier is the deleted form; the void/opening relationship is the seam `Relationship.Void` edge a `Semantics/connection#CONNECTION_DETAIL` consumer reads, NOT a spatial-traversal axis, so the old `Voids` traversal direction is gone from this view; the view is HOST-NEUTRAL — it reads seam `NodeId`/`Classification` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation, the Rhino-native block/layer capture coexisting at the universal seam contract in `csharp:Rasm.Rhino/Exchange`, neither thinned to feed the other; `Container` is the single-parent `Compose.Contain` containment the `Model/query#ELEMENT_SET` `BySpatialContainer` arm selects on (the same whole→part relation, read directly off the seam incidence rather than this view's index) and the `Model/zones#ZONE_GRAPH` many-to-many grouping overlay's orthogonal companion (an element sits in exactly one container yet belongs to arbitrarily many zones); the spatial `Root` is the frame the `Semantics/georeference#GEO_REFERENCE` reconciliation anchors so federated models share an origin; a root-resolution failure lifts the typed `BimFault.DanglingReference` BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop is the named defect this rebuild closes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The spatial-container role: the IFC project root (it aggregates the site and is never an element's
// containment target) versus a spatial container (a site/building/storey/space an element is contained in).
public enum SpatialRole : byte { Root = 0, Container = 1 }

// The traversal axis the ONE polymorphic Walk discriminates on — the seam carries no per-direction walk
// method; the axis selects the edge orientation + ComposeKind a single QuikGraph composition reads. A
// [SmartEnum] (not a bare enum) for the compile-time-total Switch and the stable wire token a query/persistence
// consumer routes on. NOT the IFC IfcRel* roster (that lives in the Bim projector); NOT a Void axis (voids
// are the seam Relationship.Void edge a Semantics/connection consumer reads, never a spatial-traversal direction).
[SmartEnum<string>]
public sealed partial class SpatialAxis {
    public static readonly SpatialAxis Ancestors   = new("ancestors");    // transitive container chain up to the root
    public static readonly SpatialAxis Descendants = new("descendants");  // transitive sub-containers + contained elements
    public static readonly SpatialAxis Children    = new("children");      // direct sub-containers (Compose.Aggregate)
    public static readonly SpatialAxis Contained   = new("contained");     // directly contained elements (Compose.Contain)
    public static readonly SpatialAxis Referenced  = new("referenced");    // non-owning spatial references (Compose.Reference)
    public static readonly SpatialAxis Container    = new("container");      // the single owning container (inverse Compose.Contain)
}

// --- [MODELS] -----------------------------------------------------------------------------
// The spatial-container vocabulary: the IFC spatial-structure entity classes keyed on the seam Classification
// code, each carrying its canonical nesting Rank and SpatialRole. This is the IFC-schema vocabulary the spatial
// VIEW interprets over the seam's generic Classification — distinct from the Model/elements#IFC_CLASS element
// roster (IfcSpace + the infra facility subtypes IfcBridge/IfcRoad/IfcRailway/IfcMarineFacility appear in both) and the SOLE owner the Projection/semantic#GRAPH_LEGALITY containment
// gate consumes (IsContainer) rather than a private spatial FrozenSet.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SpatialClass {
    public static readonly SpatialClass Project      = new("IfcProject",        0, SpatialRole.Root);
    public static readonly SpatialClass Site         = new("IfcSite",           1, SpatialRole.Container);
    public static readonly SpatialClass Building     = new("IfcBuilding",       2, SpatialRole.Container);
    public static readonly SpatialClass Facility     = new("IfcFacility",       2, SpatialRole.Container);   // IFC4.3 facility base under the site (IfcBuilding peer); its instantiated subtypes follow
    // The instantiated IFC4.3 IfcFacility subtypes a real infrastructure model carries AS the spatial container:
    // the projector stamps the concrete entity name (Model/elements#IFC_CLASS IfcClass.Bridge/Road/Railway/MarineFacility),
    // NOT the abstract IfcFacility, so the view needs the concrete row to fold an infra facility into the tree at all.
    public static readonly SpatialClass Bridge       = new("IfcBridge",         2, SpatialRole.Container);
    public static readonly SpatialClass Road         = new("IfcRoad",           2, SpatialRole.Container);
    public static readonly SpatialClass Railway      = new("IfcRailway",        2, SpatialRole.Container);
    public static readonly SpatialClass Marine       = new("IfcMarineFacility", 2, SpatialRole.Container);
    public static readonly SpatialClass Storey       = new("IfcBuildingStorey", 3, SpatialRole.Container);
    public static readonly SpatialClass FacilityPart = new("IfcFacilityPart",   3, SpatialRole.Container);   // IFC4.3 IfcBuildingStorey peer
    public static readonly SpatialClass Space        = new("IfcSpace",          4, SpatialRole.Container);

    public int Rank { get; }
    public SpatialRole Role { get; }

    public bool IsRoot => Role == SpatialRole.Root;
    public bool IsContainer => Role == SpatialRole.Container;

    // Containment legality: a parent is no deeper than its child (monotone NON-decreasing rank) and never
    // contains the root, so the IFC4.3 level skips (a Site directly over a Storey) AND same-rank nesting (a
    // site within a site, a building complex, a storey mezzanine, a sub-space — all valid IfcRelAggregates)
    // both pass, while an inverted nesting and a contained IfcProject are rejected — the rank-legality the
    // Projection/semantic#GRAPH_LEGALITY gate composes per parent->child spatial edge.
    public bool CanContain(SpatialClass child) => Rank <= child.Rank && !child.IsRoot;

    // The strict classification ingress the Projection/semantic#SEMANTIC_PROJECTOR projector composes for a spatial-
    // STRUCTURE IfcObjectDefinition — the pure-spatial IfcProject/IfcSite/IfcBuilding/IfcBuildingStorey/IfcFacility/
    // IfcFacilityPart entities the Model/elements#IFC_CLASS element roster deliberately omits (IfcClass owns placeable
    // products, Model/zones#ZONE_GRAPH BimZoneKind grouping, this owner spatial structure) — resolving the entity name
    // to the row supplying the generic Classification("ifc", row.Key) the seam Object node carries, faulting
    // Model/faults#FAULT_BAND BimFault.UnmappedClass spatial-class-miss BARE (band 2600 is Expected-derived, no
    // .ToError() hop) over the SAME generated TryGet, never a second resolver.
    public static Fin<SpatialClass> Resolve(string entityType, Op key) =>
        TryGet(entityType).ToFin(new BimFault.UnmappedClass(key, $"spatial-class-miss:{entityType}"));
}

// The derived spatial-tree view over the seam ElementGraph: a transient fold (rebuilt at the view boundary,
// never a stored domain field) holding the focused spatial QuikGraph subgraph, the resolved root, the root
// breadth-first path accessor (ancestors), and the single-parent containment index (the BySpatialContainer join).
public sealed class SpatialStructure {
    readonly ElementGraph graph;
    readonly BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree;
    readonly TryFunc<NodeId, IEnumerable<STaggedEdge<NodeId, ComposeKind>>> pathsFromRoot;
    readonly Map<NodeId, NodeId> containment;

    public NodeId Root { get; }

    SpatialStructure(ElementGraph graph, BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree, NodeId root, Map<NodeId, NodeId> containment) {
        (this.graph, this.tree, Root, this.containment) = (graph, tree, root, containment);
        if (!tree.ContainsVertex(root)) { tree.AddVertex(root); }   // a degenerate root with no spatial edges still seeds the BFS, never throws
        pathsFromRoot = tree.TreeBreadthFirstSearch(root);          // one BFS at construction; the ancestors path accessor
    }

    // Fold the seam's OWNING spatial Compose edges into the focused QuikGraph subgraph + the single-parent containment
    // index, resolve the root, and rail a missing spatial root. The owning spatial edge is a non-Reference Compose whose
    // Whole resolves to a SpatialClass (a container aggregating a sub-container, or containing an element) — so a non-spatial
    // aggregation (a curtain wall over its panels) and the non-owning Reference (a direct axis off the seam incidence)
    // both stay out of the transitive tree.
    public static Fin<SpatialStructure> Of(ElementGraph graph, Op key) {
        var spatialEdges = graph.Edges.Choose(e =>
            e is Relationship.Compose c && c.SubKind != ComposeKind.Reference && ClassOf(graph, c.Whole).IsSome
                ? Some(new STaggedEdge<NodeId, ComposeKind>(c.Whole, c.Part, c.SubKind))
                : Option<STaggedEdge<NodeId, ComposeKind>>.None);
        var tree = new BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>(allowParallelEdges: true);
        tree.AddVerticesAndEdgeRange(spatialEdges);
        var containment = graph.Edges
            .Choose(e => e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Contain ? Some((Part: c.Part, Whole: c.Whole)) : Option<(NodeId Part, NodeId Whole)>.None)
            .ToSeq()
            .Fold(Map<NodeId, NodeId>(), static (m, p) => m.AddOrUpdate(p.Part, p.Whole));
        return graph.ObjectNodes
            .Find(static o => SpatialClass.TryGet(o.Classification.Code).Exists(static c => c.IsRoot)).Map(static o => o.Id)
            .OrElse(() => tree.Roots().ToSeq().HeadOrNone())
            .ToFin(new BimFault.DanglingReference(key, "spatial-root-miss"))
            .Map(root => new SpatialStructure(graph, tree, root, containment));
    }

    // The ONE polymorphic spatial walk over the owning decomposition graph: the SmartEnum-generated total Switch
    // selects the QuikGraph composition, NEVER a per-direction method. Ancestors reads the root path accessor's
    // edge sources (reversed nearest-first) and Descendants the forward BFS reachable closure over the OWNING
    // (Aggregate/Contain) tree; children/contained read the tagged out-edges, referenced reads the non-owning
    // Compose.Reference edges off the seam incidence, container reads the single-parent index. A non-spatial node
    // yields the empty Seq, so the walk is total.
    public Seq<NodeId> Walk(NodeId from, SpatialAxis axis) =>
        !tree.ContainsVertex(from)
            ? Seq<NodeId>()
            : axis.Switch(
                ancestors:   () => pathsFromRoot(from, out var path) ? path.ToSeq().Map(static e => e.Source).Rev() : Seq<NodeId>(),
                descendants: () => { var reach = tree.TreeBreadthFirstSearch(from); return tree.Vertices.ToSeq().Filter(v => v != from && reach(v, out _)); },
                children:    () => Adjacent(from, ComposeKind.Aggregate),
                contained:   () => Adjacent(from, ComposeKind.Contain),
                referenced:  () => graph.EdgesAt(from).Choose(e => e is Relationship.Compose c && c.SubKind == ComposeKind.Reference && c.Whole == from ? Some(c.Part) : Option<NodeId>.None).ToSeq(),
                container:   () => Container(from).ToSeq());

    // The single containment parent the Model/query#ELEMENT_SET BySpatialContainer arm joins by, and the whole
    // element->container index the join folds — read from the prebuilt single-parent map, never a per-call scan.
    public Option<NodeId> Container(NodeId element) => containment.Find(element);
    public Map<NodeId, NodeId> Containment => containment;

    // The node's resolved spatial level: Some for a spatial container, None for a contained leaf element.
    public Option<SpatialClass> Level(NodeId node) => ClassOf(graph, node);

    // One direct out-edge read filtered by ComposeKind: the children (Aggregate) and contained (Contain) axes
    // collapse onto it, differing only by the tag — never sibling methods. The non-owning referenced axis reads
    // the seam incidence and the single owning container reads the prebuilt index, so both stay off this read.
    Seq<NodeId> Adjacent(NodeId node, ComposeKind kind) =>
        tree.OutEdges(node).ToSeq().Filter(e => e.Tag == kind).Map(static e => e.Target);

    // Resolve a node's seam Classification code to a SpatialClass — the one resolution the edge filter and Level
    // share, so the spatial-container test is one owner, never an inline string compare against "IfcProject".
    static Option<SpatialClass> ClassOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));
}
```

## [03]-[RESEARCH]

- [SPATIAL_VOCABULARY]: the `SpatialClass` closed-vocabulary case list and the per-row `Rank`/`SpatialRole` ground against the GeometryGym spatial-structure surface (`.api/api-geometrygym-ifc` `IfcSpatialElement`/`IfcSpatialStructureElement` base + the `IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace` rows, the IFC4.3 `IfcFacility`/`IfcFacilityPart` rows, and the concrete `IfcFacility` subtypes `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility`) and the `IfcProject` root context (the project aggregates the uppermost site, carries the units/representation contexts, and is the decomposition root rather than a containment target) — so `IfcProject` is the single `SpatialRole.Root` row and the spatial structure-element classes are the `SpatialRole.Container` rows the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment gate validates against; the `Rank` mirrors the canonical IFC spatial nesting (project→site→building→storey→space) with the IFC4.3 facility classes ranked at their building/storey peers (`IfcFacility` and its concrete `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` subtypes rank-2 `IfcBuilding` peers the site aggregates, `IfcFacilityPart` a rank-3 `IfcBuildingStorey` peer), the `CanContain` monotone non-decreasing-rank guard admitting the IFC4.3 flexible hierarchy's level skips, a `IfcSite`→`IfcFacility` aggregation, AND same-rank nesting (a site within a site, a building complex, a sub-space — all valid `IfcRelAggregates`) while rejecting an inverted nesting or a contained root `IfcProject`; the spatial-container vocabulary is distinct from the `Model/elements#IFC_CLASS` element roster (the overlap is `IfcSpace` and the infra facility subtypes `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` — each both a placeable product and a spatial container, the pure-spatial `IfcProject`/`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcFacility`/`IfcFacilityPart` being spatial-only) and is the SOLE spatial-class owner, the `IfcLegality` gate consuming `SpatialClass.IsContainer` rather than a duplicated private `FrozenSet`, and `SpatialClass.Resolve` the spatial-structure classification ingress the projector composes — the partition counterpart of `Model/elements#IFC_CLASS` `IfcClass.Resolve` (placeable products) and `Model/zones#ZONE_GRAPH` `BimZoneKind.Resolve` (grouping), so the pure-spatial entities the element roster omits still stamp the generic `Classification` this view interprets.
- [SEAM_DECOMPOSITION]: the `Compose.Aggregate`/`Compose.Contain`/`Compose.Reference` reading grounds against the seam `Relations/relation#EDGE_ALGEBRA` `ComposeKind` vocabulary (`Aggregate` whole-decomposes-into-parts, `Contain` spatial containment, `Reference` non-owning spatial reference) and `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the seam carries the neutral five-case `Relationship` algebra, the `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection` lowering `IfcRelAggregates`→`Compose.Aggregate`, `IfcRelContainedInSpatialStructure`→`Compose.Contain`, and `IfcRelReferencedInSpatialStructure`→`Compose.Reference`, with the IFC wire-name/directionality riding the projector's `IfcRelKind` roster, NOT a seam `IfcRel*` case; the egress `Projection/semantic#IFC_EGRESS` `Emit` reads the same `ComposeKind` back to re-author the `IfcRel*` entity, so the spatial structure rides the typed `ComposeKind` round-trip and never names an IFC relationship; the single-parent `Compose.Contain` containment is the orthogonal companion to the `Model/zones#ZONE_GRAPH` many-to-many grouping overlay (`IfcRelAssignsToGroup`/`IfcRelReferencedInSpatialStructure` zone membership), the two coexisting on one element collection, never collapsed.
- [TRAVERSAL_COLLAPSE]: the one polymorphic `Walk` over the shared `QuikGraph` substrate grounds against `libs/csharp/.api/api-quikgraph` (the `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` container, the `STaggedEdge` `ComposeKind`-tagged value edge, the `AlgorithmExtensions.TreeBreadthFirstSearch` reachable-closure path accessor, and the `Roots` no-incoming-edge source) and the no-operation-family law — the `BimAssembly.Traverse(globalId, TraversalDirection)` hand-rolled breadth-first `Closure` fold over a `Map<string, Seq<string>>` adjacency with a mutated `HashSet<string>` visited set is the deleted form, the `Walk` reading the package reachability so Bim folds a transient graph into the one graph-algorithm owner the whole stack shares rather than re-implementing a walk; the focused `ComposeKind`-tagged spatial subgraph is the Bim-stratum view (OWNING spatial `Compose` edges only — `Aggregate`/`Contain` — tagged so an aggregate/contain filter is one read, the non-owning `Reference` read off the seam incidence), distinct from the seam's unfiltered `Graph/element#ELEMENT_GRAPH` `Topology()` `SEdge` view (every edge kind, no tag) and the `csharp:Rasm.Persistence/Query/topology` durable-store lane — each stratum folds its OWN transient view over the shared `QuikGraph` substrate, aligned through the seam graph, never one referencing another; the `Descendants`/`Ancestors` transitive closures compose the forward BFS and the root path accessor over the owning tree, the `Children`/`Contained` direct adjacencies the `ComposeKind`-tagged out-edges, the `Referenced` the non-owning `Compose.Reference` edges off the seam incidence and the `Container` the prebuilt single-parent index, all six axes one `Walk` discriminating on the `SpatialAxis` value, never a `TraverseDescendants`/`TraverseAncestors`/`TraverseContained` method family; the `BySpatialContainer` query arm, the `Review/validation#IDS_FACETS` `PartOf` facet, the `Review/diff#MODEL_DIFF` move detection, and the `Semantics/georeference#GEO_REFERENCE` root reconciliation all read this one view rather than a per-consumer spatial walk.
