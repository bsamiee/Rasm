# [BIM_SPATIAL_STRUCTURE]

The Bim spatial-structure VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`: the IFC project→site→building→storey→space hierarchy read as a DERIVED `SpatialStructure` over the seam's neutral `Compose` edges, never a parallel relationship union and never a second stored tree. The seam owns the property graph — the `Object` nodes, the neutral `Relations/relation#EDGE_ALGEBRA` `Relationship` algebra, and the `QuikGraph` topology view it materializes once at the read-snapshot freeze; this page owns the IFC SPATIAL INTERPRETATION the seam is deliberately blind to: the `SpatialClass` `[SmartEnum<string>]` spatial-container vocabulary (canonical nesting `Rank`, the role and containment law deriving from it), the `SpatialStructure` derived spatial-tree view, and the ONE polymorphic `Walk(NodeId, SpatialAxis)` composing `QuikGraph` reachability with the seam incidence. The retired `BimAssembly`/`SpatialContainer`/`AssemblyRel` owners and the `IfcSemanticModel` flat-row source are GONE: the spatial relationships are the seam's `Compose.Aggregate` (spatial decomposition), `Compose.Contain` (single-parent element containment), and `Compose.Reference` (non-owning spatial reference) edges the `Projection/relations#RELATION_ALGEBRA` projector lowered, and the IFC `IfcRel*` roster lives WHOLLY in that projector [C5] — so the spatial structure re-opens no IFC-schema strata on the seam and mints no second IFC relationship union. The view is the join surface the folder composes: the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same single-parent `Compose.Contain` relation this view indexes and its `SpatialReach.Ancestry` reach walks the same `Contain`/`Aggregate` up-chain off the seam incidence per candidate, the `Model/zones#ZONE_GRAPH` many-to-many overlay is its orthogonal companion (owning `IfcSpatialZone` reference membership this vocabulary deliberately omits), the `Review/validation#IDS_FACETS` `PartOf` `Contained` relation lowers onto that same containment join, and the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` containment gate consumes `SpatialClass.IsContainer` joined with the `Model/zones#ZONE_GRAPH` `BimZoneKind.SpatialZone` `IsSpatial` row (the partition's two owners cover the schema-legal containment-whole set); classification stays the ONE generated `Model/elements#IFC_CLASS` `IfcClass` ingress — the spatial backbone rides `IfcDomain.General` in the reflected roster — and this vocabulary only INTERPRETS the stamped code, never re-classifies.

## [01]-[INDEX]

- [01]-[SPATIAL_STRUCTURE]: the `SpatialClass` `[SmartEnum<string>]` spatial-interpretation vocabulary (`Rank` with the derived `IsRoot`/`IsContainer` role and the `CanContain` rank law, over the generated `IfcClass`-stamped codes), the `SpatialAxis` traversal discriminant, the `SpatialStructure` derived spatial-tree view folded from the seam's OWNING spatial `Compose` edges (`Aggregate`/`Contain`) into a `QuikGraph` `BidirectionalGraph`, the ONE polymorphic `Walk(NodeId, SpatialAxis)` traversal, the `Container`/`Containment`/`Root`/`Level` accessors, and the `Separations` space-separation adjacency pairing spaces through their shared 2nd-level boundary elements off the seam `Generic("IfcRelSpaceBoundary")` edges.

## [02]-[SPATIAL_STRUCTURE]

- Owner: `SpatialStructure` the derived spatial-tree view over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — it holds the seam graph (for node resolution and the non-owning `Reference` axis), a FOCUSED `QuikGraph` `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` folded from the OWNING spatial `Compose` edges (`Aggregate`/`Contain`), the content-resolved spatial `Root`, the root breadth-first path accessor, and the single-parent `Containment` index; `SpatialClass` the `[SmartEnum<string>]` spatial-interpretation vocabulary keyed on the IFC spatial entity-type string (the seam `Classification.Code` the generated `Model/elements#IFC_CLASS` ingress stamps), each row carrying its canonical nesting `Rank` — rank 0 the `IfcProject` root that aggregates the site, every deeper rank a container an element is contained in (`IfcSite`/`IfcBuilding`/`IfcFacility` (+ its concrete `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` infra subtypes)/`IfcBuildingStorey`/`IfcFacilityPart` (+ its concrete `IfcBridgePart`/`IfcRoadPart`/`IfcRailwayPart`/`IfcMarinePart` leaves)/`IfcSpace`/`IfcExternalSpatialElement`), the `IsRoot`/`IsContainer` role and the `CanContain` rank law DERIVING from it; `SpatialAxis` the `[SmartEnum<string>]` traversal discriminant the one `Walk` routes on. There is NO stored tree (the structure is a fold over the seam graph, rebuilt at the view boundary like the seam's own `QuikGraph` cache), NO parallel relationship union (the seam's neutral `Compose` owns the edges), and NO per-element spatial record (the `Object` node IS the spatial node).
- Entry: `SpatialStructure.Of(ElementGraph graph, Op key)` accumulates root-cardinality, parent-uniqueness, rank-legality, root-parent, and reachability failures into `Validation<Error, SpatialStructure>` before admitting the rooted tree. `Walk(NodeId from, SpatialAxis axis)` dispatches every traversal modality through one `SpatialAxis` value. `CommonAncestors(Seq<(NodeId First, NodeId Second)> pairs)` batches rooted-tree pairs through `OfflineLeastCommonAncestor`; absent vertices remain absent from the result. `Separations()` joins second-level boundary pairs by separator.
- Auto: `Of` keys the owning-edge fold on the resolved `Whole` class (a `Compose.Aggregate` between containers, a `Compose.Contain` to an element) so the `BidirectionalGraph` carries only the owning spatial subgraph — a non-owning `Compose.Reference` never enters the transitive closure and a curtain-wall→panel aggregation whose `Whole` is a non-spatial element never enters it; `Walk` dispatches the generated total `SpatialAxis.Switch` (compile-time exhaustive, no runtime `_` arm) with PER-AXIS totality — the tree-backed arms (`Ancestors`/`Descendants`/`Children`/`Contained`) guard vertex membership because `QuikGraph` `OutEdges` throws on an absent vertex, while `Referenced` reads the seam incidence and `Container` the prebuilt index, both total over the whole graph without tree membership — so every axis yields the empty `Seq` rather than a throw on a non-spatial node; the transitive descendants arm folds the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event into the reached set (the package algorithm-object event fold, O(reachable) — never an all-vertex `TryFunc` path-probe sweep), the transitive ancestors arm reads the SAME root breadth-first path accessor built once at `Of` (the path edges' `Source` vertices ARE the container chain, reversed nearest-first), the direct children/contained arms read the `ComposeKind`-tagged out-edges through one `Adjacent` read differing only by tag; `Container`/`Containment` read the prebuilt single-parent index so this view's container resolution is O(1) per element, never a per-call edge scan (the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same `Compose.Contain` relation directly off the seam incidence).
- Packages: Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`ComposeKind`/`Classification`), QuikGraph (the `BidirectionalGraph`/`STaggedEdge` containers, the `AlgorithmExtensions` `TreeBreadthFirstSearch`/`Roots` facade, and the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new spatial-container level is one `SpatialClass` row carrying its rank (an IFC4.3 `IfcFacilityPart` subdivision and the IFC4 `IfcExternalSpatialElement` exterior region both ride the same fold); a new traversal direction is one `SpatialAxis` row and one `Walk` arm reading the same `STaggedEdge` adjacency; a new spatial-decomposition flavor is one `ComposeKind` the seam already carries and one tag filter the `Walk` reads; a new separation consumer composes `Separations` (a boundary-level refinement is one attr filter on its `Choose`), never a re-derived boundary pairing; never a per-relationship `AssemblyRel` arm, never a per-direction `Traverse` method, and never a second stored spatial tree.
- Boundary: `SpatialStructure` derives only from seam `Compose` edges and resolves only seam `NodeId`/`Classification` values. `SpatialClass` owns containment roles, `BimZoneKind` owns grouping roles, `Relationship.Void` remains outside the traversal axis, and independent structural failures accumulate as typed `BimFault` values before the view admits.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Bim.Projection;   // Separations composes the Projection/relations#RELATION_ALGEBRA IfcRelKind.SpaceBoundary
                             // wire-name row and the Projection/semantic#SEMANTIC_PROJECTOR BoundaryLevel attr key.
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

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
    private readonly ElementGraph graph;
    private readonly BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree;
    private readonly TryFunc<NodeId, IEnumerable<STaggedEdge<NodeId, ComposeKind>>> pathsFromRoot;
    private readonly Map<NodeId, NodeId> containment;

    public NodeId Root { get; }

    private SpatialStructure(ElementGraph graph, BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree, NodeId root, Map<NodeId, NodeId> containment) {
        (this.graph, this.tree, Root, this.containment) = (graph, tree, root, containment);
        pathsFromRoot = tree.TreeBreadthFirstSearch(root);   // one BFS at construction; the ancestors path accessor — Of
                                                             // seeds every root vertex, so a degenerate root never throws
    }

    // ONE Choose pass over the seam edges feeds BOTH the tagged tree and the single-parent containment index,
    // so the tree and the index agree on the spatial-whole gate by construction. The owning spatial edge is an
    // Aggregate/Contain Compose whose Whole resolves to a SpatialClass — a non-spatial aggregation (a curtain wall
    // over its panels), an ordered Nest, and the non-owning Reference (a direct axis off the seam incidence) stay out.
    public static Validation<Error, SpatialStructure> Of(ElementGraph graph, Op key) {
        Seq<STaggedEdge<NodeId, ComposeKind>> spatial = toSeq(graph.Edges).Choose(e =>
            e is Relationship.Compose c && (c.SubKind == ComposeKind.Aggregate || c.SubKind == ComposeKind.Contain) && ClassOf(graph, c.Whole).IsSome
                ? Some(new STaggedEdge<NodeId, ComposeKind>(c.Whole, c.Part, c.SubKind))
                : Option<STaggedEdge<NodeId, ComposeKind>>.None);
        BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree = new(allowParallelEdges: true);
        tree.AddVerticesAndEdgeRange(spatial);
        Seq<NodeId> roots = graph.ObjectNodes
            .Filter(static node => SpatialClass.TryGet(node.Classification.Code).Exists(static spatialClass => spatialClass.IsRoot))
            .Map(static node => node.Id)
            .ToSeq();
        tree.AddVertexRange(roots);
        Seq<NodeId> ambiguous = toSeq(spatial.GroupBy(static edge => edge.Target))
            .Filter(static group => group.Select(static edge => edge.Source).Distinct().Count() > 1)
            .Map(static group => group.Key);
        Validation<Error, NodeId> root = roots.Count switch {
            0 => Fail<Error, NodeId>(new BimFault.DanglingReference(key, "spatial-root-miss")),
            1 => Success<Error, NodeId>(roots[0]),
            _ => Fail<Error, NodeId>(new BimFault.ModelRejected(key, $"spatial-root-cardinality:{roots.Count}")),
        };
        Validation<Error, Map<NodeId, NodeId>> parents = ambiguous.IsEmpty
            ? Success<Error, Map<NodeId, NodeId>>(spatial.Filter(static edge => edge.Tag == ComposeKind.Contain)
                .Fold(Map<NodeId, NodeId>(), static (map, edge) => map.Add(edge.Target, edge.Source)))
            : Fail<Error, Map<NodeId, NodeId>>(new BimFault.ModelRejected(key, $"spatial-parent-ambiguous:{string.Join(',', ambiguous.Map(static id => id.Value))}"));
        Seq<(NodeId Parent, NodeId Child)> inverted = spatial
            .Choose(edge => ClassOf(graph, edge.Target).Bind(child => ClassOf(graph, edge.Source)
                .Filter(parent => !parent.CanContain(child))
                .Map(_ => (Parent: edge.Source, Child: edge.Target))));
        Validation<Error, Unit> hierarchy = inverted.IsEmpty
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new BimFault.ModelRejected(key, $"spatial-rank-inverted:{string.Join(',', inverted.Map(static edge => $"{edge.Parent.Value}>{edge.Child.Value}"))}"));
        Seq<NodeId> reached = roots.Count == 1 ? Reachable(tree, roots[0]) : Seq<NodeId>();
        Validation<Error, Unit> rootParent = roots.Count == 1 && tree.InEdges(roots[0]).Any()
            ? Fail<Error, Unit>(new BimFault.ModelRejected(key, $"spatial-root-has-parent:{roots[0].Value}"))
            : Success<Error, Unit>(unit);
        Seq<NodeId> disconnected = roots.Count == 1
            ? toSeq(tree.Vertices).Filter(vertex => !reached.Contains(vertex))
            : Seq<NodeId>();
        Validation<Error, Unit> connectivity = disconnected.IsEmpty
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new BimFault.ModelRejected(key, $"spatial-disconnected:{string.Join(',', disconnected.Map(static id => id.Value))}"));
        return (root, parents, hierarchy, rootParent, connectivity)
            .Apply((admittedRoot, admittedParents, _, _, _) => new SpatialStructure(graph, tree, admittedRoot, admittedParents))
            .As();
    }

    // The ONE polymorphic spatial walk: the SmartEnum-generated STATE-THREADED total Switch (static lambdas, the
    // (view, from) tuple as state — no per-call closure set) selects the QuikGraph composition, NEVER a
    // per-direction method. Totality is PER-AXIS: the tree-backed arms guard vertex membership (OutEdges throws
    // on an absent vertex), while Referenced reads the seam incidence and Container the prebuilt index — both
    // total over the whole graph — so every axis yields the empty Seq on a non-spatial node, never a throw.
    public Seq<NodeId> Walk(NodeId from, SpatialAxis axis) => axis.Switch(
        state: (View: this, From: from),
        ancestors:   static s => s.View.tree.ContainsVertex(s.From) && s.View.pathsFromRoot(s.From, out var path) ? toSeq(path).Map(static e => e.Source).Rev() : Seq<NodeId>(),
        descendants: static s => s.View.tree.ContainsVertex(s.From) ? Reachable(s.View.tree, s.From).Filter(v => v != s.From) : Seq<NodeId>(),
        children:    static s => s.View.Adjacent(s.From, ComposeKind.Aggregate),
        contained:   static s => s.View.Adjacent(s.From, ComposeKind.Contain),
        referenced:  static s => toSeq(s.View.graph.EdgesAt(s.From)).Choose(e => e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Reference && c.Whole == s.From ? Some(c.Part) : Option<NodeId>.None),
        container:   static s => s.View.Container(s.From).ToSeq());

    // The single containment parent the Model/query#ELEMENT_SET BySpatialContainer arm joins by, and the whole
    // element->container index the join folds — read from the prebuilt single-parent map, never a per-call scan.
    public Option<NodeId> Container(NodeId element) => containment.Find(element);
    public Map<NodeId, NodeId> Containment => containment;

    public Map<(NodeId First, NodeId Second), NodeId> CommonAncestors(Seq<(NodeId First, NodeId Second)> pairs) {
        Seq<SEquatableEdge<NodeId>> queries = pairs
            .Filter(pair => tree.ContainsVertex(pair.First) && tree.ContainsVertex(pair.Second))
            .Map(static pair => new SEquatableEdge<NodeId>(pair.First, pair.Second));
        TryFunc<SEquatableEdge<NodeId>, NodeId> ancestors = tree.OfflineLeastCommonAncestor(Root, queries);
        return queries.Choose(query => ancestors(query, out NodeId ancestor)
                ? Some((Pair: (query.Source, query.Target), Ancestor: ancestor))
                : Option<((NodeId, NodeId) Pair, NodeId Ancestor)>.None)
            .Fold(Map<(NodeId, NodeId), NodeId>(), static (map, row) => map.Add(row.Pair, row.Ancestor));
    }

    // The node's resolved spatial level: Some for a spatial container, None for a contained leaf element.
    public Option<SpatialClass> Level(NodeId node) => ClassOf(graph, node);

    // The space-SEPARATION adjacency — the topology the containment tree structurally cannot express: two
    // 2nd-level space boundaries on ONE separating element join their spaces through it (the fire-separation,
    // acoustic-rating, and thermal-envelope backbone — "which two spaces meet through which wall"), read straight
    // off the seam Generic("IfcRelSpaceBoundary") edges the Projection/relations#RELATION_ALGEBRA SpatialBoundaries
    // fold lands (Relating = space, Related = element, the three-valued SemanticProjector.BoundaryLevel attr
    // discriminating "2nd" — a 1st-level or base boundary never pairs). TOTAL; the unordered space pair is
    // ordinal-stable so a re-read yields identical rows, and a virtual boundary (no shared physical element in
    // IFC still mints the edge) pairs through its boundary entity exactly as the schema records it.
    public Seq<(NodeId SpaceA, NodeId SpaceB, NodeId Separator)> Separations() =>
        toSeq(toSeq(graph.Edges)
            .Choose(static e => e is Relationship.Generic g
                    && string.Equals(g.WireName, IfcRelKind.SpaceBoundary.Key, StringComparison.Ordinal)
                    && g.Attributes.Find(SemanticProjector.BoundaryLevel).Exists(static v => v is PropertyValue.Text { Value: "2nd" })
                ? Some((Space: g.Relating, Separator: g.Related))
                : Option<(NodeId Space, NodeId Separator)>.None)
            .GroupBy(static bound => bound.Separator))
            .Bind(group =>
                from a in toSeq(group).Map(static bound => bound.Space).Distinct()
                from b in toSeq(group).Map(static bound => bound.Space).Distinct()
                where string.CompareOrdinal(a.Value, b.Value) < 0
                select (SpaceA: a, SpaceB: b, Separator: group.Key));

    // One direct out-edge read filtered by ComposeKind: the children (Aggregate) and contained (Contain) axes
    // collapse onto it, differing only by the tag — never sibling methods. The vertex guard keeps the read total.
    private Seq<NodeId> Adjacent(NodeId node, ComposeKind kind) =>
        tree.ContainsVertex(node) ? toSeq(tree.OutEdges(node)).Filter(e => e.Tag == kind).Map(static e => e.Target) : Seq<NodeId>();

    // The transitive reachable closure through the package algorithm object's DiscoverVertex event fold —
    // O(reachable), never an all-vertex TryFunc path-probe sweep. The event accumulation is the QuikGraph
    // in-traversal fold form; the statements are that platform event seam, confined here.
    private static Seq<NodeId> Reachable(BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> graph, NodeId from) {
        BreadthFirstSearchAlgorithm<NodeId, STaggedEdge<NodeId, ComposeKind>> search = new(graph);
        Seq<NodeId> reached = Seq<NodeId>();
        search.DiscoverVertex += v => reached = reached.Add(v);
        search.Compute(from);
        return reached;
    }

    // Resolve a node's seam Classification code to a SpatialClass — the one resolution the edge filter and Level
    // share, so the spatial-container test is one owner, never an inline string compare against "IfcProject".
    private static Option<SpatialClass> ClassOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));
}
```

## [03]-[RESEARCH]

- [SPATIAL_VOCABULARY]: `SpatialClass` maps every spatial-structure class to one canonical rank, derives root/container roles, and admits monotone level skips plus same-rank aggregation. `IfcSpatialZone` remains in `BimZoneKind`, while classification remains in generated `IfcClass`; the three vocabularies share codes without duplicating ownership.
- [SEAM_DECOMPOSITION]: the `Compose.Aggregate`/`Compose.Contain`/`Compose.Reference` reading grounds against the seam `Relations/relation#EDGE_ALGEBRA` `ComposeKind` vocabulary (`Aggregate` whole-decomposes-into-parts, `Contain` spatial containment, `Reference` non-owning spatial reference) and `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the seam carries the neutral five-case `Relationship` algebra, the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` lowering `IfcRelAggregates`→`Compose.Aggregate`, `IfcRelContainedInSpatialStructure`→`Compose.Contain`, and `IfcRelReferencedInSpatialStructure`→`Compose.Reference`, with the IFC wire-name/directionality riding the projector's `IfcRelKind` roster, NOT a seam `IfcRel*` case; the egress `Projection/egress#IFC_EGRESS` `Emit` reads the same `ComposeKind` back to re-author the `IfcRel*` entity, so the spatial structure rides the typed `ComposeKind` round-trip and never names an IFC relationship; the seam `Edges`/`EdgesAt` yield frozen `ImmutableArray<Relationship>` rows, folded through `toSeq` exactly as the seam's own operations spell it, and the ONE `Choose` pass feeding both the tagged tree and the `Contain`-filtered containment index keeps the two structures agreeing on the spatial-whole gate by construction (a second unfiltered edge scan whose index admits a non-spatial whole the tree excludes is the closed asymmetry); the single-parent `Compose.Contain` containment is the orthogonal companion to the `Model/zones#ZONE_GRAPH` many-to-many grouping overlay (`IfcRelAssignsToGroup`/`IfcRelReferencedInSpatialStructure` zone membership), the two coexisting on one element collection, never collapsed, and the `Model/query#ELEMENT_SET` `BySpatialContainer` arm joins the same relation — `Direct` off the seam incidence, `SpatialReach.Ancestry` walking the same `Contain`/`Aggregate` up-chain this tree encodes, per candidate, so the query never constructs the view and the view never re-implements the predicate.
- [TRAVERSAL_COLLAPSE]: the one polymorphic `Walk` over the shared `QuikGraph` substrate grounds against `libs/csharp/.api/api-quikgraph` (the `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` container, the `STaggedEdge` `ComposeKind`-tagged value edge, the `AlgorithmExtensions.TreeBreadthFirstSearch` path accessor, the `Roots` no-incoming-edge source, and the `BreadthFirstSearchAlgorithm` + `DiscoverVertex` algorithm-object event fold the catalog names for accumulating a reached set) and the no-operation-family law — the `BimAssembly.Traverse(globalId, TraversalDirection)` hand-rolled breadth-first `Closure` fold over a `Map<string, Seq<string>>` adjacency with a mutated `HashSet<string>` visited set is the deleted form, the descendants closure riding the package event fold (O(reachable), replacing the all-vertex `TryFunc` path-probe sweep that re-recovered a path per vertex) and the ancestors chain the root path accessor built once at `Of`; the focused `ComposeKind`-tagged spatial subgraph is the Bim-stratum view (OWNING spatial `Compose` edges only — `Aggregate`/`Contain` — tagged so an aggregate/contain filter is one read, the non-owning `Reference` read off the seam incidence), distinct from the seam's unfiltered `Graph/element#ELEMENT_GRAPH` `Topology()` `SEdge` view (every edge kind, no tag) and the `csharp:Rasm.Persistence/Query/topology` durable-store lane — each stratum folds its OWN transient view over the shared `QuikGraph` substrate, aligned through the seam graph, never one referencing another; totality is per-axis (the tree-backed arms guard `ContainsVertex` because `OutEdges` throws on an absent vertex; `Referenced` and `Container` stay total over the whole graph off the seam incidence and the prebuilt index), all six axes one `Walk` discriminating on the `SpatialAxis` value, never a `TraverseDescendants`/`TraverseAncestors`/`TraverseContained` method family; the `Review/validation#IDS_FACETS` `PartOf` `Contained` relation and the `Model/query#ELEMENT_SET` containment arm read the same `Compose.Contain` join this view indexes, so container resolution has one law across query, validation, and view.
- [SEPARATION_ADJACENCY]: the `Separations` space↔space join grounds against the `Projection/relations#RELATION_ALGEBRA` `SpatialBoundaries` fold — every `IfcRelSpaceBoundary` lands as `Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, element, attrs)` with the three-valued `SemanticProjector.BoundaryLevel` `PropertyValue.Text` (`"2nd"`/`"1st"`/`""`, the exact-subtype discriminant the egress refined-construct re-authors) — and the IFC 2nd-level space-boundary semantics: paired `IfcRelSpaceBoundary2ndLevel` instances on one separating element are the schema's own space-adjacency encoding, so grouping the `"2nd"`-leveled edges by their `Related` element and pairing the distinct `Relating` spaces IS the separation topology (the separator the shared element — the fire reviewer's "compartment wall between zone A and zone B", the acoustician's party wall), with the unordered pair ordinal-ordered for a stable re-read; the raw boundary edges were previously consumed only by the up-stratum Compute OSM build, so the join closes the space-boundary graph's in-package consumer gap the `Model/query#ELEMENT_SET` `ByGeneric("IfcRelSpaceBoundary", …)` arm complements element-centrically (the arm asks "does this space bound that element", the view asks "which spaces meet through it").
