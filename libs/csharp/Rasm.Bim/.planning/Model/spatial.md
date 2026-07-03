# [BIM_SPATIAL_STRUCTURE]

The Bim spatial-structure VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`: the IFC project→site→building→storey→space hierarchy read as a DERIVED `SpatialStructure` over the seam's neutral `Compose` edges, never a parallel relationship union and never a second stored tree. The seam owns the property graph — the `Object` nodes, the neutral `Relations/relation#EDGE_ALGEBRA` `Relationship` algebra, and the `QuikGraph` topology view it materializes once at the read-snapshot freeze; this page owns the IFC SPATIAL INTERPRETATION the seam is deliberately blind to: the `SpatialClass` `[SmartEnum<string>]` spatial-container vocabulary (canonical nesting `Rank`, the role and containment law deriving from it), the `SpatialStructure` derived spatial-tree view, and the ONE polymorphic `Walk(NodeId, SpatialAxis)` composing `QuikGraph` reachability with the seam incidence. The retired `BimAssembly`/`SpatialContainer`/`AssemblyRel` owners and the `IfcSemanticModel` flat-row source are GONE: the spatial relationships are the seam's `Compose.Aggregate` (spatial decomposition), `Compose.Contain` (single-parent element containment), and `Compose.Reference` (non-owning spatial reference) edges the `Projection/relations#RELATION_ALGEBRA` projector lowered, and the IFC `IfcRel*` roster lives WHOLLY in that projector [C5] — so the spatial structure re-opens no IFC-schema strata on the seam and mints no second IFC relationship union. The view is the join surface the folder composes: the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same single-parent `Compose.Contain` relation this view indexes and its `SpatialReach.Ancestry` reach walks the same `Contain`/`Aggregate` up-chain off the seam incidence per candidate, the `Model/zones#ZONE_GRAPH` many-to-many overlay is its orthogonal companion (owning `IfcSpatialZone` reference membership this vocabulary deliberately omits), the `Review/validation#IDS_FACETS` `PartOf` `Contained` relation lowers onto that same containment join, and the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment gate consumes `SpatialClass.IsContainer` joined with the `Model/zones#ZONE_GRAPH` `BimZoneKind.SpatialZone` `IsSpatial` row (the partition's two owners cover the schema-legal containment-whole set); classification stays the ONE generated `Model/elements#IFC_CLASS` `IfcClass` ingress — the spatial backbone rides `IfcDomain.General` in the reflected roster — and this vocabulary only INTERPRETS the stamped code, never re-classifies.

## [01]-[INDEX]

- [01]-[SPATIAL_STRUCTURE]: the `SpatialClass` `[SmartEnum<string>]` spatial-interpretation vocabulary (`Rank` with the derived `IsRoot`/`IsContainer` role and the `CanContain` rank law, over the generated `IfcClass`-stamped codes), the `SpatialAxis` traversal discriminant, the `SpatialStructure` derived spatial-tree view folded from the seam's OWNING spatial `Compose` edges (`Aggregate`/`Contain`) into a `QuikGraph` `BidirectionalGraph`, the ONE polymorphic `Walk(NodeId, SpatialAxis)` traversal, and the `Container`/`Containment`/`Root`/`Level` accessors.

## [02]-[SPATIAL_STRUCTURE]

- Owner: `SpatialStructure` the derived spatial-tree view over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — it holds the seam graph (for node resolution and the non-owning `Reference` axis), a FOCUSED `QuikGraph` `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` folded from the OWNING spatial `Compose` edges (`Aggregate`/`Contain`), the content-resolved spatial `Root`, the root breadth-first path accessor, and the single-parent `Containment` index; `SpatialClass` the `[SmartEnum<string>]` spatial-interpretation vocabulary keyed on the IFC spatial entity-type string (the seam `Classification.Code` the generated `Model/elements#IFC_CLASS` ingress stamps), each row carrying its canonical nesting `Rank` — rank 0 the `IfcProject` root that aggregates the site, every deeper rank a container an element is contained in (`IfcSite`/`IfcBuilding`/`IfcFacility` (+ its concrete `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` infra subtypes)/`IfcBuildingStorey`/`IfcFacilityPart` (+ its concrete `IfcBridgePart`/`IfcRoadPart`/`IfcRailwayPart`/`IfcMarinePart` leaves)/`IfcSpace`/`IfcExternalSpatialElement`), the `IsRoot`/`IsContainer` role and the `CanContain` rank law DERIVING from it; `SpatialAxis` the `[SmartEnum<string>]` traversal discriminant the one `Walk` routes on. There is NO stored tree (the structure is a fold over the seam graph, rebuilt at the view boundary like the seam's own `QuikGraph` cache), NO parallel relationship union (the seam's neutral `Compose` owns the edges), and NO per-element spatial record (the `Object` node IS the spatial node).
- Entry: `SpatialStructure.Of(ElementGraph graph, Op key)` folds the graph's OWNING spatial `Compose` edges — every `Aggregate`/`Contain` `Compose` whose `Whole` resolves to a `SpatialClass` — in ONE `Choose` pass feeding BOTH the `BidirectionalGraph` (tagged by `ComposeKind` so a traversal filters aggregate-vs-contain) and the single-parent containment index (the `Contain`-tagged rows of the same pass, so the tree and the index agree on the spatial-whole gate by construction), reads the non-owning `Compose.Reference` edges as a direct axis off the seam incidence, resolves the spatial root (the `Object` node whose `Classification.Code` resolves to a `SpatialClass.IsRoot` row, falling back to the `QuikGraph` `Roots()` no-incoming-edge spatial source), and returns `Fin<SpatialStructure>` faulting `Model/faults#FAULT_BAND` `BimFault.DanglingReference(key, "spatial-root-miss")` BARE (the band is `Expected`-derived, no `.ToError()` hop) when no root resolves; `Walk(NodeId from, SpatialAxis axis)` is the ONE polymorphic spatial walk — `Ancestors` the transitive container chain up to the root, `Descendants` the transitive sub-containers and contained elements, `Children` the direct sub-containers, `Contained` the directly contained elements, `Referenced` the non-owning spatial references, and `Container` the single owning container — never a `WalkAncestors`/`WalkDescendants`/`WalkContained` operation family; `Container(NodeId element)` resolves the single containment parent from the index, `Containment` exposes the whole element→container join, `Root` the spatial root, `Level(NodeId)` the node's resolved `SpatialClass`.
- Auto: `Of` keys the owning-edge fold on the resolved `Whole` class (a `Compose.Aggregate` between containers, a `Compose.Contain` to an element) so the `BidirectionalGraph` carries only the owning spatial subgraph — a non-owning `Compose.Reference` never enters the transitive closure and a curtain-wall→panel aggregation whose `Whole` is a non-spatial element never enters it; `Walk` dispatches the generated total `SpatialAxis.Switch` (compile-time exhaustive, no runtime `_` arm) with PER-AXIS totality — the tree-backed arms (`Ancestors`/`Descendants`/`Children`/`Contained`) guard vertex membership because `QuikGraph` `OutEdges` throws on an absent vertex, while `Referenced` reads the seam incidence and `Container` the prebuilt index, both total over the whole graph without tree membership — so every axis yields the empty `Seq` rather than a throw on a non-spatial node; the transitive descendants arm folds the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event into the reached set (the package algorithm-object event fold, O(reachable) — never an all-vertex `TryFunc` path-probe sweep), the transitive ancestors arm reads the SAME root breadth-first path accessor built once at `Of` (the path edges' `Source` vertices ARE the container chain, reversed nearest-first), the direct children/contained arms read the `ComposeKind`-tagged out-edges through one `Adjacent` read differing only by tag; `Container`/`Containment` read the prebuilt single-parent index so this view's container resolution is O(1) per element, never a per-call edge scan (the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same `Compose.Contain` relation directly off the seam incidence).
- Packages: Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`ComposeKind`/`Classification`), QuikGraph (the `BidirectionalGraph`/`STaggedEdge` containers, the `AlgorithmExtensions` `TreeBreadthFirstSearch`/`Roots` facade, and the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new spatial-container level is one `SpatialClass` row carrying its rank (an IFC4.3 `IfcFacilityPart` subdivision and the IFC4 `IfcExternalSpatialElement` exterior region both ride the same fold); a new traversal direction is one `SpatialAxis` row and one `Walk` arm reading the same `STaggedEdge` adjacency; a new spatial-decomposition flavor is one `ComposeKind` the seam already carries and one tag filter the `Walk` reads; never a per-relationship `AssemblyRel` arm, never a per-direction `Traverse` method, and never a second stored spatial tree.
- Boundary: `SpatialStructure` is a VIEW over the seam graph — the retired `BimAssembly` stored tree, the `SpatialContainer` per-node record, the `AssemblyRel` `[Union]` mirroring the IFC `IfcRel*` decomposition relationships, and the `IfcSemanticModel` flat-row source are the deleted form [C5], because the seam carries the neutral `Compose` edge algebra and the IFC `IfcRel*` roster lives in `Projection/relations#RELATION_ALGEBRA` — a typed `AssemblyRel.Aggregates`/`Nests`/`ContainedIn`/`Voids`/`Connects` union on this page re-opens the IFC-schema strata leak the `Classification` collapse closed and is the named seam violation; the decomposition graph is the seam's `Compose.Aggregate`/`Compose.Contain`/`Compose.Reference` edges (the projector lowers `IfcRelAggregates`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` onto them), so this view reads the seam's typed `ComposeKind`, never an IFC relationship name; the traversal composes `QuikGraph` (the shared `libs/csharp/.api` graph-algorithm owner the whole stack folds a transient graph into) — the hand-rolled breadth-first `Closure` fold over a `Map<string, Seq<string>>` adjacency with a mutated `HashSet<string>` visited set is the deleted form, the reached-set closure riding the package `BreadthFirstSearchAlgorithm` event fold and the ancestors chain the package path accessor; the spatial-container vocabulary is the ONE `SpatialClass` owner — the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment-whole-must-be-spatial gate consumes `SpatialClass.IsContainer` joined with the `Model/zones#ZONE_GRAPH` `BimZoneKind.SpatialZone` `IsSpatial` row (`IfcSpatialZone` is a schema-legal `IfcRelContainedInSpatialStructure` relating structure this vocabulary deliberately omits, so the gate composes the partition's two owners) rather than re-listing a private spatial `FrozenSet` (a duplicated private set drifts from the vocabulary — omitting an infra facility or facility-part row wrongly faults an `IfcBridge` or `IfcRoadPart` containment); classification is the ONE generated `Model/elements#IFC_CLASS` `IfcClass` ingress over the full reflected `IfcObjectDefinition` roster (the spatial backbone on `IfcDomain.General`), so a `SpatialClass.Resolve` parallel classifier beside it is the deleted second-resolver form — this vocabulary exposes only the Option-lift `TryGet` the view, the gate, and `Level` read; `IfcSpatialZone` is DELIBERATELY absent from this vocabulary — the zones overlay owns it (`BimZoneKind.SpatialZone`, `IsSpatial: true`, membership over `Compose.Reference`), so the two vocabularies partition the spatial entity set over disjoint `Whole`-class sets and a `SpatialClass.SpatialZone` row would double-own the entity and pull reference membership into the owning tree; the void/opening relationship is the seam `Relationship.Void` edge the `Model/query#ELEMENT_SET` `ByVoided` arm and a `Semantics/connection#CONNECTION_DETAIL` consumer read, NOT a spatial-traversal axis, so the old `Voids` traversal direction is gone from this view; the view is HOST-NEUTRAL — it reads seam `NodeId`/`Classification` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation, the Rhino-native block/layer capture coexisting at the universal seam contract in `csharp:Rasm.Rhino/Exchange`, neither thinned to feed the other; `Container` is the single-parent `Compose.Contain` containment the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins (Direct off the seam incidence, `Ancestry` walking the same `Contain`/`Aggregate` up-chain this tree encodes) and the `Model/zones#ZONE_GRAPH` many-to-many grouping overlay's orthogonal companion (an element sits in exactly one container yet belongs to arbitrarily many zones); a root-resolution failure lifts the typed `BimFault.DanglingReference` BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop is the named defect this rebuild closes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The traversal axis the ONE polymorphic Walk discriminates on — the axis selects the edge orientation +
// ComposeKind a single QuikGraph composition reads; a [SmartEnum] for the compile-time-total Switch and the
// stable wire token a query/persistence consumer routes on. NOT the IFC IfcRel* roster (the Bim projector's);
// NOT a Void axis (voids are the seam Relationship.Void edge the query ByVoided arm reads).
[SmartEnum<string>]
public sealed partial class SpatialAxis {
    public static readonly SpatialAxis Ancestors   = new("ancestors");    // transitive container chain up to the root
    public static readonly SpatialAxis Descendants = new("descendants");  // transitive sub-containers + contained elements
    public static readonly SpatialAxis Children    = new("children");     // direct sub-containers (Compose.Aggregate)
    public static readonly SpatialAxis Contained   = new("contained");    // directly contained elements (Compose.Contain)
    public static readonly SpatialAxis Referenced  = new("referenced");   // non-owning spatial references (Compose.Reference)
    public static readonly SpatialAxis Container   = new("container");    // the single owning container (inverse Compose.Contain)
}

// --- [MODELS] -----------------------------------------------------------------------------
// The spatial-INTERPRETATION vocabulary: the IFC spatial-structure entity classes keyed on the seam
// Classification code, each carrying the canonical nesting Rank, the root flag, and the CanContain rank law —
// the spatial-view columns the generated Model/elements#IFC_CLASS region cannot carry (the emitter commits
// Domain/Span/Instantiable/tokens; containment rank is view law, never vocabulary data). The generated IfcClass
// roster is the ONE classification ingress (the spatial backbone rides IfcDomain.General), so this owner never
// re-classifies — it interprets the stamped code. Disjoint from Model/zones#ZONE_GRAPH grouping (IfcSpatialZone
// is the zones overlay's IsSpatial row, never a row here); the SOLE owner the Projection/semantic#GRAPH_LEGALITY
// containment gate consumes (IsContainer) rather than a private spatial FrozenSet.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SpatialClass {
    public static readonly SpatialClass Project      = new("IfcProject",        0);   // rank 0 IS the root: aggregates the site, never a containment target
    public static readonly SpatialClass Site         = new("IfcSite",           1);
    public static readonly SpatialClass Building     = new("IfcBuilding",       2);
    public static readonly SpatialClass Facility     = new("IfcFacility",       2);   // IFC4.3 facility base under the site (IfcBuilding peer); its instantiated subtypes follow
    // The instantiated IFC4.3 IfcFacility subtypes a real infrastructure model carries AS the spatial container:
    // the projector stamps the concrete entity name (IfcClass.Bridge/Road/Railway/MarineFacility), NOT the
    // abstract IfcFacility, so the view needs the concrete row to fold an infra facility into the tree at all.
    public static readonly SpatialClass Bridge       = new("IfcBridge",         2);
    public static readonly SpatialClass Road         = new("IfcRoad",           2);
    public static readonly SpatialClass Railway      = new("IfcRailway",        2);
    public static readonly SpatialClass Marine       = new("IfcMarineFacility", 2);
    public static readonly SpatialClass Storey       = new("IfcBuildingStorey", 3);
    public static readonly SpatialClass FacilityPart = new("IfcFacilityPart",   3);   // IFC4.3 IfcBuildingStorey peer (schema-abstract in ADD2; the row still classifies RC-era files)
    // The concrete IfcFacilityPart subtypes an IFC4.3 infra model stamps AS the storey-peer container — the projector
    // stamps the LEAF entity name, so a missing part row silently drops every infra-part Compose.Contain edge from the tree.
    public static readonly SpatialClass BridgePart   = new("IfcBridgePart",     3);
    public static readonly SpatialClass RoadPart     = new("IfcRoadPart",       3);
    public static readonly SpatialClass RailwayPart  = new("IfcRailwayPart",    3);
    public static readonly SpatialClass MarinePart   = new("IfcMarinePart",     3);
    public static readonly SpatialClass Space        = new("IfcSpace",          4);
    public static readonly SpatialClass External     = new("IfcExternalSpatialElement", 4);   // IFC4 outside-facility region — the exterior elements' legal containment/reference target the roster otherwise drops

    public int Rank { get; }

    // The role DERIVES from the rank — rank 0 is the decomposition root, every deeper rank a container an
    // element is contained in; a stored role column would restate the rank and drift from it.
    public bool IsRoot => Rank == 0;
    public bool IsContainer => Rank > 0;

    // Containment legality: a parent is no deeper than its child (monotone NON-decreasing rank) and never
    // contains the root, so the IFC4.3 level skips (a Site directly over a Storey) AND same-rank nesting (a
    // site within a site, a building complex, a storey mezzanine, a sub-space — all valid IfcRelAggregates)
    // both pass, while an inverted nesting and a contained IfcProject are rejected — the rank-legality the
    // Projection/semantic#GRAPH_LEGALITY gate composes per parent->child spatial edge.
    public bool CanContain(SpatialClass child) => Rank <= child.Rank && !child.IsRoot;

    // The ONE Option-lift over the generated bool TryGet(string?, out SpatialClass?) — the raw seam stays
    // beneath it, never a second resolver; the view, the legality gate, and Level all read this member.
    public static Option<SpatialClass> TryGet(string entityType) =>
        TryGet(entityType, out SpatialClass? row) && row is { } hit ? Some(hit) : None;
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

    // ONE Choose pass over the seam edges feeds BOTH the tagged tree and the single-parent containment index,
    // so the tree and the index agree on the spatial-whole gate by construction. The owning spatial edge is an
    // Aggregate/Contain Compose whose Whole resolves to a SpatialClass — a non-spatial aggregation (a curtain wall
    // over its panels), an ordered Nest, and the non-owning Reference (a direct axis off the seam incidence) stay out.
    public static Fin<SpatialStructure> Of(ElementGraph graph, Op key) {
        Seq<STaggedEdge<NodeId, ComposeKind>> spatial = toSeq(graph.Edges).Choose(e =>
            e is Relationship.Compose c && (c.SubKind == ComposeKind.Aggregate || c.SubKind == ComposeKind.Contain) && ClassOf(graph, c.Whole).IsSome
                ? Some(new STaggedEdge<NodeId, ComposeKind>(c.Whole, c.Part, c.SubKind))
                : Option<STaggedEdge<NodeId, ComposeKind>>.None);
        var tree = new BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>(allowParallelEdges: true);
        tree.AddVerticesAndEdgeRange(spatial);
        Map<NodeId, NodeId> containment = spatial.Filter(static e => e.Tag == ComposeKind.Contain)
            .Fold(Map<NodeId, NodeId>(), static (m, e) => m.AddOrUpdate(e.Target, e.Source));
        return (toSeq(graph.ObjectNodes
                .Filter(static o => SpatialClass.TryGet(o.Classification.Code).Exists(static c => c.IsRoot))
                .Map(static o => o.Id)
                .OrderBy(static id => id.Value, StringComparer.Ordinal)).Head   // ordinal-min pick: a federated multi-root file resolves the SAME root every run
                | toSeq(tree.Roots()).Head)                                     // the verified Option `|` alternative (eager, left-biased) — `.OrElse` is a phantom member
            .ToFin(new BimFault.DanglingReference(key, "spatial-root-miss"))
            .Map(root => new SpatialStructure(graph, tree, root, containment));
    }

    // The ONE polymorphic spatial walk: the SmartEnum-generated STATE-THREADED total Switch (static lambdas, the
    // (view, from) tuple as state — no per-call closure set) selects the QuikGraph composition, NEVER a
    // per-direction method. Totality is PER-AXIS: the tree-backed arms guard vertex membership (OutEdges throws
    // on an absent vertex), while Referenced reads the seam incidence and Container the prebuilt index — both
    // total over the whole graph — so every axis yields the empty Seq on a non-spatial node, never a throw.
    public Seq<NodeId> Walk(NodeId from, SpatialAxis axis) => axis.Switch(
        state: (View: this, From: from),
        ancestors:   static s => s.View.tree.ContainsVertex(s.From) && s.View.pathsFromRoot(s.From, out var path) ? toSeq(path).Map(static e => e.Source).Rev() : Seq<NodeId>(),
        descendants: static s => s.View.tree.ContainsVertex(s.From) ? s.View.Reachable(s.From) : Seq<NodeId>(),
        children:    static s => s.View.Adjacent(s.From, ComposeKind.Aggregate),
        contained:   static s => s.View.Adjacent(s.From, ComposeKind.Contain),
        referenced:  static s => toSeq(s.View.graph.EdgesAt(s.From)).Choose(e => e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Reference && c.Whole == s.From ? Some(c.Part) : Option<NodeId>.None),
        container:   static s => s.View.Container(s.From).ToSeq());

    // The single containment parent the Model/query#ELEMENT_SET BySpatialContainer arm joins by, and the whole
    // element->container index the join folds — read from the prebuilt single-parent map, never a per-call scan.
    public Option<NodeId> Container(NodeId element) => containment.Find(element);
    public Map<NodeId, NodeId> Containment => containment;

    // The node's resolved spatial level: Some for a spatial container, None for a contained leaf element.
    public Option<SpatialClass> Level(NodeId node) => ClassOf(graph, node);

    // One direct out-edge read filtered by ComposeKind: the children (Aggregate) and contained (Contain) axes
    // collapse onto it, differing only by the tag — never sibling methods. The vertex guard keeps the read total.
    Seq<NodeId> Adjacent(NodeId node, ComposeKind kind) =>
        tree.ContainsVertex(node) ? toSeq(tree.OutEdges(node)).Filter(e => e.Tag == kind).Map(static e => e.Target) : Seq<NodeId>();

    // The transitive reachable closure through the package algorithm object's DiscoverVertex event fold —
    // O(reachable), never an all-vertex TryFunc path-probe sweep. The event accumulation is the QuikGraph
    // in-traversal fold form; the statements are that platform event seam, confined here.
    Seq<NodeId> Reachable(NodeId from) {
        var search = new BreadthFirstSearchAlgorithm<NodeId, STaggedEdge<NodeId, ComposeKind>>(tree);
        Seq<NodeId> reached = Seq<NodeId>();
        search.DiscoverVertex += v => reached = reached.Add(v);
        search.Compute(from);
        return reached.Filter(v => v != from);
    }

    // Resolve a node's seam Classification code to a SpatialClass — the one resolution the edge filter and Level
    // share, so the spatial-container test is one owner, never an inline string compare against "IfcProject".
    static Option<SpatialClass> ClassOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));
}
```

## [03]-[RESEARCH]

- [SPATIAL_VOCABULARY]: the `SpatialClass` closed-vocabulary case list and the per-row `Rank` ground against the GeometryGym spatial-structure surface (`.api/api-geometrygym-ifc` `IfcSpatialElement`/`IfcSpatialStructureElement` base + the `IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace` rows, the IFC4.3 `IfcFacility`/`IfcFacilityPart` rows, the concrete `IfcFacility` subtypes `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility`, the concrete `IfcFacilityPart` subtypes `IfcBridgePart`/`IfcRoadPart`/`IfcRailwayPart`/`IfcMarinePart` (assembly-verified leaves the projector stamps AS the storey-peer container — the `Projection/semantic#GRAPH_LEGALITY` `Spatial` set already enumerates them, so a part-less vocabulary silently dropped every infra-part containment this roster now folds), and the IFC4 `IfcExternalSpatialElement` outside-facility region — a legal containment target for exterior elements whose omission would silently drop their `Compose.Contain` edges from the tree) and the `IfcProject` root context (the project aggregates the uppermost site, carries the units/representation contexts, and is the decomposition root rather than a containment target) — so `IfcProject` is the single rank-0 root row (`IsRoot`/`IsContainer` DERIVE from the rank — a stored role column would restate it) and the deeper-ranked structure-element classes are the containers the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment gate validates against; the `Rank` mirrors the canonical IFC spatial nesting (project→site→building→storey→space) with the IFC4.3 facility classes ranked at their building/storey peers and `IfcExternalSpatialElement` a rank-4 space peer, the `CanContain` monotone non-decreasing-rank guard admitting the IFC4.3 flexible hierarchy's level skips, a `IfcSite`→`IfcFacility` aggregation, AND same-rank nesting (a site within a site, a building complex, a sub-space — all valid `IfcRelAggregates`) while rejecting an inverted nesting or a contained root `IfcProject`; the INTERPRETATION vocabularies partition over disjoint code sets — this owner the spatial-structure containment law, `Model/zones#ZONE_GRAPH` `BimZoneKind` the grouping law INCLUDING `IfcSpatialZone` (its `IsSpatial: true` row over `Compose.Reference` membership), so a `SpatialClass.SpatialZone` row is the double-ownership defect this partition forbids — and `SpatialClass` is the SOLE spatial-container owner: the `IfcLegality` gate consumes `SpatialClass.IsContainer` joined with the zones overlay's `BimZoneKind.SpatialZone` `IsSpatial` row rather than a duplicated private `FrozenSet` (a private set drifts from the vocabularies — an omitted facility or facility-part row wrongly faults an `IfcBridge` or `IfcRoadPart` containment); CLASSIFICATION is not partitioned — the generated `Model/elements#IFC_CLASS` `IfcClass` ingress stamps every `IfcObjectDefinition` (the spatial backbone on `IfcDomain.General`), so the retired `SpatialClass.Resolve` parallel classifier is the deleted second-resolver form and this vocabulary reads the stamped `Classification.Code` through its one Option-lift `TryGet`.
- [SEAM_DECOMPOSITION]: the `Compose.Aggregate`/`Compose.Contain`/`Compose.Reference` reading grounds against the seam `Relations/relation#EDGE_ALGEBRA` `ComposeKind` vocabulary (`Aggregate` whole-decomposes-into-parts, `Contain` spatial containment, `Reference` non-owning spatial reference) and `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the seam carries the neutral five-case `Relationship` algebra, the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` lowering `IfcRelAggregates`→`Compose.Aggregate`, `IfcRelContainedInSpatialStructure`→`Compose.Contain`, and `IfcRelReferencedInSpatialStructure`→`Compose.Reference`, with the IFC wire-name/directionality riding the projector's `IfcRelKind` roster, NOT a seam `IfcRel*` case; the egress `Projection/egress#IFC_EGRESS` `Emit` reads the same `ComposeKind` back to re-author the `IfcRel*` entity, so the spatial structure rides the typed `ComposeKind` round-trip and never names an IFC relationship; the seam `Edges`/`EdgesAt` yield frozen `ImmutableArray<Relationship>` rows, folded through `toSeq` exactly as the seam's own operations spell it, and the ONE `Choose` pass feeding both the tagged tree and the `Contain`-filtered containment index keeps the two structures agreeing on the spatial-whole gate by construction (a second unfiltered edge scan whose index admits a non-spatial whole the tree excludes is the closed asymmetry); the single-parent `Compose.Contain` containment is the orthogonal companion to the `Model/zones#ZONE_GRAPH` many-to-many grouping overlay (`IfcRelAssignsToGroup`/`IfcRelReferencedInSpatialStructure` zone membership), the two coexisting on one element collection, never collapsed, and the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same relation — `Direct` off the seam incidence, `SpatialReach.Ancestry` walking the same `Contain`/`Aggregate` up-chain this tree encodes, per candidate, so the query never constructs the view and the view never re-implements the predicate.
- [TRAVERSAL_COLLAPSE]: the one polymorphic `Walk` over the shared `QuikGraph` substrate grounds against `libs/csharp/.api/api-quikgraph` (the `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` container, the `STaggedEdge` `ComposeKind`-tagged value edge, the `AlgorithmExtensions.TreeBreadthFirstSearch` path accessor, the `Roots` no-incoming-edge source, and the `BreadthFirstSearchAlgorithm` + `DiscoverVertex` algorithm-object event fold the catalog names for accumulating a reached set) and the no-operation-family law — the `BimAssembly.Traverse(globalId, TraversalDirection)` hand-rolled breadth-first `Closure` fold over a `Map<string, Seq<string>>` adjacency with a mutated `HashSet<string>` visited set is the deleted form, the descendants closure riding the package event fold (O(reachable), replacing the all-vertex `TryFunc` path-probe sweep that re-recovered a path per vertex) and the ancestors chain the root path accessor built once at `Of`; the focused `ComposeKind`-tagged spatial subgraph is the Bim-stratum view (OWNING spatial `Compose` edges only — `Aggregate`/`Contain` — tagged so an aggregate/contain filter is one read, the non-owning `Reference` read off the seam incidence), distinct from the seam's unfiltered `Graph/element#ELEMENT_GRAPH` `Topology()` `SEdge` view (every edge kind, no tag) and the `csharp:Rasm.Persistence/Query/topology` durable-store lane — each stratum folds its OWN transient view over the shared `QuikGraph` substrate, aligned through the seam graph, never one referencing another; totality is per-axis (the tree-backed arms guard `ContainsVertex` because `OutEdges` throws on an absent vertex; `Referenced` and `Container` stay total over the whole graph off the seam incidence and the prebuilt index), all six axes one `Walk` discriminating on the `SpatialAxis` value, never a `TraverseDescendants`/`TraverseAncestors`/`TraverseContained` method family; the `Review/validation#IDS_FACETS` `PartOf` `Contained` relation and the `Model/query#ELEMENT_SET` containment arm read the same `Compose.Contain` join this view indexes, so container resolution has one law across query, validation, and view.
