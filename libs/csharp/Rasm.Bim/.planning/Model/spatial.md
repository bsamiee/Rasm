# [BIM_SPATIAL_STRUCTURE]

`SpatialStructure` is the Bim spatial-structure VIEW over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — the IFC project→site→building→storey→space hierarchy DERIVED from the seam's neutral `Compose` edges, never a parallel union and never a second stored tree. This page owns the IFC spatial INTERPRETATION the seam is blind to: `SpatialClass` with its nesting `Rank`, one polymorphic `Walk`, one up-chain ancestor law every folder consumer composes, and the IFC4.3 linear-referencing axis making an infra element station-addressable.

Composition arrives settled. `Projection/relations#RELATION_ALGEBRA` lowered the `Compose.Aggregate`/`Contain`/`Reference` edges this view folds and owns the whole `IfcRel*` roster [NEUTRAL_EDGE_RULING]; `Model/elements#IFC_CLASS` `IfcClass` owns the stamped code this vocabulary only interprets. Downstream, `Model/query#ELEMENT_SET` `SpatialReach` reads the surfaces published here, `Model/zones#ZONE_GRAPH` owns the orthogonal overlay carrying `IfcSpatialZone`, and `Projection/semantic#GRAPH_LEGALITY` joins `SpatialClass.IsContainer` with that overlay's `IsSpatial` row.

## [01]-[INDEX]

- [02]-[SPATIAL_STRUCTURE]: `SpatialClass` the spatial-interpretation vocabulary (`Rank` with its derived role and `CanContain` law), `SpatialAxis` the traversal discriminant, `SpatialStructure` the tree view folded from the seam's OWNING `Compose` edges, its ONE polymorphic `Walk`, the index-free `ParentOf`/`Ancestry` up-chain law, the `Container`/`Root`/`Level` accessors, and `Separations` pairing spaces through shared 2nd-level boundaries.
- [03]-[LINEAR_POSITIONING]: `PositioningProjection.Attrs` the deep reader lowering the IFC4.3 linear-referencing axis onto typed seam attribute payloads — per-segment design parameters, referent station marks, each linearly-placed product's station/offset quadruple — and `PositioningRows` the public row vocabulary they mint through, composed by the `Projection/semantic#SEMANTIC_PROJECTOR` `SourceBag` synthesis.

## [02]-[SPATIAL_STRUCTURE]

- Owner: `SpatialStructure` the derived spatial-tree view over the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph` — it holds the seam graph (for node resolution and the non-owning `Reference` axis), a FOCUSED `QuikGraph` `BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>>` folded from the OWNING spatial `Compose` edges (`Aggregate`/`Contain`), the content-resolved spatial `Root`, and the single-parent `Containment` index; `SpatialClass` the `[SmartEnum<string>]` spatial-interpretation vocabulary keyed on the IFC spatial entity-type string (the seam `Classification.Code` the generated `Model/elements#IFC_CLASS` ingress stamps), each row carrying its canonical nesting `Rank` — rank 0 the `IfcProject` root that aggregates the site, every deeper rank a container an element is contained in (`IfcSite`/`IfcBuilding`/`IfcFacility` (+ its concrete `IfcBridge`/`IfcRoad`/`IfcRailway`/`IfcMarineFacility` infra subtypes)/`IfcBuildingStorey`/`IfcFacilityPart` (+ its concrete `IfcBridgePart`/`IfcRoadPart`/`IfcRailwayPart`/`IfcMarinePart` leaves)/`IfcSpace`/`IfcExternalSpatialElement`), the `IsRoot`/`IsContainer` role and the `CanContain` rank law DERIVING from it; `SpatialAxis` the `[SmartEnum<string>]` traversal discriminant the one `Walk` routes on. There is NO stored tree (the structure is a fold over the seam graph, rebuilt at the view boundary like the seam's own `QuikGraph` cache), NO parallel relationship union (the seam's neutral `Compose` owns the edges), and NO per-element spatial record (the `Object` node IS the spatial node).
- Law: up-chain PRECEDENCE runs `Compose.Contain` first, `Compose.Aggregate` second — an element crosses its non-spatial aggregate host to reach the spatial ancestors (a curtain-wall panel sits on the storey through its wall, the level membership a `Contain`-only walk structurally misses) and the `Contain` parent wins when a malformed graph carries both. That law lives HERE once: `Walk(from, SpatialAxis.Ancestors)`, the `Model/query#ELEMENT_SET` `SpatialReach.Ancestry` chain, and the `Projection/egress#IFC_EGRESS` scoped-emit closure all read `Ancestry`, so a second `Contain`/`Aggregate` `Choose` at a consumer is the fork this owner deletes. The walk is index-free (per-hop O(degree) off the seam incidence) precisely so a consumer holding no validated view still composes it, and it is cycle-guarded — a corrupt cycle shortens the chain rather than recursing. The `Containment` index stays `Contain`-ONLY because `Container` answers the single OWNING container, which an aggregate host is not.
- Entry: `SpatialStructure.Of(ElementGraph graph, Op key)` accumulates root-cardinality, parent-uniqueness, rank-legality, root-parent, and reachability failures into `Validation<Error, SpatialStructure>` before admitting the rooted tree. `Walk(NodeId from, SpatialAxis axis)` dispatches every traversal modality through one `SpatialAxis` value. `SpatialStructure.ParentOf(ElementGraph, NodeId)` and `SpatialStructure.Ancestry(ElementGraph, NodeId)` are the STATIC up-chain reads a consumer holding only the seam graph composes — no view instance, no validation rail, total. `CommonAncestors(Seq<(NodeId First, NodeId Second)> pairs)` batches rooted-tree pairs through `OfflineLeastCommonAncestor`; absent vertices remain absent from the result. `Separations()` joins second-level boundary pairs by separator.
- Auto: `Of` keys the owning-edge fold on the resolved `Whole` class (a `Compose.Aggregate` between containers, a `Compose.Contain` to an element) so the `BidirectionalGraph` carries only the owning spatial subgraph — a non-owning `Compose.Reference` never enters the transitive closure and a curtain-wall→panel aggregation whose `Whole` is a non-spatial element never enters it; `Walk` dispatches the generated total `SpatialAxis.Switch` (compile-time exhaustive, no runtime `_` arm) with PER-AXIS totality — the tree-backed arms (`Descendants`/`Children`/`Contained`) guard vertex membership because `QuikGraph` `OutEdges` throws on an absent vertex, while `Ancestors` walks the incidence up-chain, `Referenced` reads the seam incidence, and `Container` the prebuilt index, all three total over the whole graph without tree membership — so every axis yields the empty `Seq` rather than a throw on a non-spatial node; the transitive descendants arm folds the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event into the reached set (the package algorithm-object event fold, O(reachable) — never an all-vertex `TryFunc` path-probe sweep), the direct children/contained arms read the `ComposeKind`-tagged out-edges through one `Adjacent` read differing only by tag; `Container`/`Containment` read the prebuilt single-parent index so this view's container resolution is O(1) per element, never a per-call edge scan.
- Packages: Rasm.Element (the seam `ElementGraph`/`Node`/`NodeId`/`Relationship`/`ComposeKind`/`Classification`), QuikGraph (the `BidirectionalGraph`/`STaggedEdge` containers, the `AlgorithmExtensions` `OfflineLeastCommonAncestor` facade, and the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm (the kernel `Op` operation key the fault carries).
- Growth: a new spatial-container level is one `SpatialClass` row carrying its rank (an IFC4.3 `IfcFacilityPart` subdivision and the IFC4 `IfcExternalSpatialElement` exterior region both ride the same fold); a new traversal direction is one `SpatialAxis` row and one `Walk` arm reading the same `STaggedEdge` adjacency; a new spatial-decomposition flavor is one `ComposeKind` the seam already carries and one tag filter the `Walk` reads; a new up-chain consumer composes the static `Ancestry` and a new separation consumer composes `Separations` (a boundary-level refinement is one attr filter on its `Choose`), never a re-derived walk or boundary pairing; never a per-relationship `AssemblyRel` arm, never a per-direction `Traverse` method, and never a second stored spatial tree.
- Boundary: `SpatialStructure` derives only from seam `Compose` edges and resolves only seam `NodeId`/`Classification` values. `SpatialClass` owns containment roles, `BimZoneKind` owns grouping roles, `Relationship.Void` remains outside the traversal axis, and independent structural failures accumulate as typed `BimFault` values before the view admits. Four shapes are each deleted: a per-element spatial record, a `SpatialContainer`/`AssemblyRel` relationship type beside the neutral `Compose` edges, a flat-row spatial source, and a consumer-local `Contain`/`Aggregate` up-chain — the `Object` node IS the spatial node, the seam edge IS the relationship, and this owner IS the walk.

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
    public static readonly SpatialAxis Ancestors   = new("ancestors");    // transitive up-chain, nearest first (Contain then Aggregate)
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

// The derived spatial-tree view over the seam ElementGraph: a transient fold (rebuilt at the view boundary, never a
// stored domain field) holding the focused spatial QuikGraph subgraph, the resolved root, and the single-parent
// containment index. The ancestor walk is NOT a field: it is the static up-chain law below, so a consumer holding
// only the seam graph reaches the identical chain without admitting a view.
public sealed class SpatialStructure {
    private readonly ElementGraph graph;
    private readonly BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree;
    private readonly Map<NodeId, NodeId> containment;
    private readonly Map<NodeId, Seq<NodeId>> boundaries;

    public NodeId Root { get; }

    private SpatialStructure(
        ElementGraph graph, BidirectionalGraph<NodeId, STaggedEdge<NodeId, ComposeKind>> tree, NodeId root,
        Map<NodeId, NodeId> containment, Map<NodeId, Seq<NodeId>> boundaries) =>
        (this.graph, this.tree, Root, this.containment, this.boundaries) = (graph, tree, root, containment, boundaries);

    // Admission builds the tagged tree, the single-parent containment index, AND the separator incidence index, so
    // every later read is an index lookup and no published surface re-scans the whole edge set per call. The owning
    // spatial edge is an Aggregate/Contain Compose whose Whole resolves to a SpatialClass — a non-spatial
    // aggregation (a curtain wall over its panels), an ordered Nest, and the non-owning Reference (a direct axis
    // off the seam incidence) stay out.
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
        // The index folds AddOrUpdate: the tree admits PARALLEL edges by construction, so a duplicated
        // Contain edge re-states one parent and a throwing Add would abort admission on a redundancy the
        // ambiguity census — which counts DISTINCT sources — has already proved harmless.
        Validation<Error, Map<NodeId, NodeId>> parents = ambiguous.IsEmpty
            ? Success<Error, Map<NodeId, NodeId>>(spatial.Filter(static edge => edge.Tag == ComposeKind.Contain)
                .Fold(Map<NodeId, NodeId>(), static (map, edge) => map.AddOrUpdate(edge.Target, edge.Source)))
            : Fail<Error, Map<NodeId, NodeId>>(new BimFault.ModelRejected(key, $"spatial-parent-ambiguous:{string.Join(',', ambiguous.Map(static id => id.Value))}"));
        // The separator incidence index, folded in THIS pass: separator -> the distinct spaces bounded through it,
        // read off the Generic("IfcRelSpaceBoundary") edges at the SemanticProjector.BoundaryLevel "2nd" discriminant.
        Map<NodeId, Seq<NodeId>> boundaries = toSeq(graph.Edges)
            .Choose(static e => e is Relationship.Generic g
                    && string.Equals(g.WireName, IfcRelKind.SpaceBoundary.Key, StringComparison.Ordinal)
                    && g.Attributes.Find(SemanticProjector.BoundaryLevel).Exists(static v => v is PropertyValue.Text { Value: "2nd" })
                ? Some((Space: g.Relating, Separator: g.Related))
                : Option<(NodeId Space, NodeId Separator)>.None)
            .Fold(Map<NodeId, Seq<NodeId>>(), static (map, bound) => map.AddOrUpdate(
                bound.Separator,
                map.Find(bound.Separator).IfNone(Seq<NodeId>()).Add(bound.Space).Distinct().Strict()));
        Seq<(NodeId Parent, NodeId Child)> inverted = spatial
            .Choose(edge => ClassOf(graph, edge.Target).Bind(child => ClassOf(graph, edge.Source)
                .Filter(parent => !parent.CanContain(child))
                .Map(_ => (Parent: edge.Source, Child: edge.Target))));
        Validation<Error, Unit> hierarchy = inverted.IsEmpty
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new BimFault.ModelRejected(key, $"spatial-rank-inverted:{string.Join(',', inverted.Map(static edge => $"{edge.Parent.Value}>{edge.Child.Value}"))}"));
        // Connectivity hashes the reached closure before sweeping: the membership test runs once per tree vertex, so
        // a Seq scan makes admission quadratic in the vertex count — the shape a million-node corpus pays for and a
        // hundred-node fixture never shows.
        LanguageExt.HashSet<NodeId> reached = toHashSet(roots.Count == 1 ? Reachable(tree, roots[0]) : Seq<NodeId>());
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
            .Apply((admittedRoot, admittedParents, _, _, _) => new SpatialStructure(graph, tree, admittedRoot, admittedParents, boundaries))
            .As();
    }

    // The ONE polymorphic spatial walk: the SmartEnum-generated STATE-THREADED total Switch (static lambdas, the
    // (view, from) tuple as state — no per-call closure set) selects the composition, NEVER a per-direction method.
    // Totality is PER-AXIS: the tree-backed arms guard vertex membership (OutEdges throws on an absent vertex),
    // while Ancestors walks the incidence up-chain, Referenced reads the seam incidence, and Container the prebuilt
    // index — all three total over the whole graph — so every axis yields the empty Seq on a non-spatial node.
    public Seq<NodeId> Walk(NodeId from, SpatialAxis axis) => axis.Switch(
        state: (View: this, From: from),
        ancestors:   static s => Ancestry(s.View.graph, s.From),
        descendants: static s => s.View.tree.ContainsVertex(s.From) ? Reachable(s.View.tree, s.From).Filter(v => v != s.From) : Seq<NodeId>(),
        children:    static s => s.View.Adjacent(s.From, ComposeKind.Aggregate),
        contained:   static s => s.View.Adjacent(s.From, ComposeKind.Contain),
        referenced:  static s => toSeq(s.View.graph.EdgesAt(s.From)).Choose(e => e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Reference && c.Whole == s.From ? Some(c.Part) : Option<NodeId>.None),
        container:   static s => s.View.Container(s.From).ToSeq());

    // Single OWNING containment parent, and the whole element->container index the Model/query#ELEMENT_SET
    // SpatialReach.Direct row joins by — read from the prebuilt Contain-only map, never a per-call scan. An
    // aggregate host is not an owning container, so it never enters here; it enters the up-chain below.
    public Option<NodeId> Container(NodeId element) => containment.Find(element);
    public Map<NodeId, NodeId> Containment => containment;

    // ONE up-chain law, published STATIC and index-free so the query SpatialReach.Ancestry row and the egress
    // scoped-emit closure compose it off a bare seam graph — no admitted view, no validation rail, no second walk.
    // Contain first, Aggregate second: an element crosses its non-spatial aggregate host (a curtain-wall panel
    // through its wall) to reach the spatial ancestors, and the Contain parent wins when a graph carries both.
    public static Option<NodeId> ParentOf(ElementGraph graph, NodeId node) =>
        graph.ContainerOf(node) | toSeq(graph.EdgesAt(node)).Choose(e =>
            e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Aggregate && c.Part == node
                ? Some(c.Whole) : Option<NodeId>.None).Head;

    // Nearest-first ancestor chain. The seen set bounds a corrupt cyclic snapshot into termination — the read is
    // Op-free and never rails, exactly as the seam's own ContainmentPath is; Of owns the railed structural verdict.
    public static Seq<NodeId> Ancestry(ElementGraph graph, NodeId node) =>
        Chain(graph, node, HashSet(node));

    private static Seq<NodeId> Chain(ElementGraph graph, NodeId node, LanguageExt.HashSet<NodeId> seen) =>
        ParentOf(graph, node).Filter(parent => !seen.Contains(parent)).Match(
            Some: parent => parent.Cons(Chain(graph, parent, seen.Add(parent))),
            None: static () => Seq<NodeId>());

    public Map<(NodeId First, NodeId Second), NodeId> CommonAncestors(Seq<(NodeId First, NodeId Second)> pairs) {
        Seq<SEquatableEdge<NodeId>> queries = pairs
            .Filter(pair => tree.ContainsVertex(pair.First) && tree.ContainsVertex(pair.Second))
            .Map(static pair => new SEquatableEdge<NodeId>(pair.First, pair.Second));
        TryFunc<SEquatableEdge<NodeId>, NodeId> ancestors = tree.OfflineLeastCommonAncestor(Root, queries);
        // AddOrUpdate, because the caller's pair list is unfiltered input: a repeated pair resolves the SAME
        // ancestor, so a throwing Add would fault a batch on a duplicate the result is indifferent to.
        return queries.Choose(query => ancestors(query, out NodeId ancestor)
                ? Some((Pair: (query.Source, query.Target), Ancestor: ancestor))
                : Option<((NodeId, NodeId) Pair, NodeId Ancestor)>.None)
            .Fold(Map<(NodeId, NodeId), NodeId>(), static (map, row) => map.AddOrUpdate(row.Pair, row.Ancestor));
    }

    // The node's resolved spatial level: Some for a spatial container, None for a contained leaf element.
    public Option<SpatialClass> Level(NodeId node) => ClassOf(graph, node);

    // The space-SEPARATION adjacency — the topology the containment tree structurally cannot express: two
    // 2nd-level space boundaries on ONE separating element join their spaces through it (the fire-separation,
    // acoustic-rating, and thermal-envelope backbone — "which two spaces meet through which wall"), read off the
    // separator INCIDENCE index Of folded in its one edge pass, so this is a pairing over the index and never a
    // second whole-edge scan the callers pay per invocation. TOTAL; the unordered space pair is ordinal-stable so a
    // re-read yields identical rows, and a virtual boundary (no shared physical element in IFC still mints the
    // edge) pairs through its boundary entity exactly as the schema records it.
    public Seq<(NodeId SpaceA, NodeId SpaceB, NodeId Separator)> Separations() =>
        toSeq(boundaries.AsIterable()).Bind(row =>
            from a in row.Value
            from b in row.Value
            where string.CompareOrdinal(a.Value, b.Value) < 0
            select (SpaceA: a, SpaceB: b, Separator: row.Key));

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

## [03]-[LINEAR_POSITIONING]

- Owner: `PositioningProjection` the linear-referencing deep reader — the positioning peer of the `Model/structural#STRUCTURAL_PROJECTION` reader, lowering the IFC4.3 alignment axis onto typed seam attribute payloads: the per-segment design parameters off each `IfcAlignmentSegment.DesignParameters` (`IfcAlignmentHorizontalSegment`/`IfcAlignmentVerticalSegment`/`IfcAlignmentCantSegment`), the `IfcReferent` station-mark `RestartDistance`, and each linearly-placed product's station/offset quadruple off its `IfcLinearPlacement.Distance` `IfcPointByDistanceExpression`. The rows land as one `Projection/semantic#SEMANTIC_PROJECTOR` `SourceBag` Import bag per entity, so every infra consumer addresses elements as "1+240 to 1+380" through the standing property machinery.
- Entry: `PositioningProjection.Attrs(IfcObjectDefinition definition, UnitScale scale, Op key)` reads one entity's positioning facts into the typed attr map — `Fin<T>` because a malformed station magnitude faults typed through `MeasureValue.OfSi`, never a swallowed NaN row; a non-positioning entity yields the empty map, so the `SourceBag` synthesis mints no empty bag. `PositioningRows` is the PUBLIC row vocabulary both the writer and every reader compose by static.
- Auto: the segment arm switches `DesignParameters` — a horizontal segment lands `StartDirection` (angular), its radii/length/gravity-centre-height (lengths), and its `IfcAlignmentHorizontalSegmentTypeEnum` token as `Text`; a vertical segment lands its distance/length/height/radius lengths, the `StartGradient`/`EndGradient` dimensionless ratios, and its token; a cant segment lands its distance, its length, its four cant lengths, and its token — the segment's curve geometry stays content-keyed in `Representations`, never an inlined `StartPoint` coordinate; the referent arm lands `RestartDistance` (the `PredefinedType` `STATION`/`KILOMETREPOINT` token riding the node's own predefined read); the placement arm probes `IfcProduct.ObjectPlacement` for `IfcLinearPlacement` and lands the resolved station off `Distance.DistanceAlong` (the `IfcCurveMeasureSelect` length leg dimensioned, the parameter leg dimensionless) and its three offset lengths — the positioned element's alignment identity riding the rostered `Generic("IfcRelPositions")` edge the `Projection/relations#RELATION_ALGEBRA` roster already lands, never a duplicate bag row. EVERY magnitude crosses the ONE `UnitScale.Coerce` native→SI transform keyed by its `MeasureRow` — the length rows on `MeasureRow.Length`, the direction on `MeasureRow.Angle`, the ratios on neither — so no slot multiplies a bare `scale` field of its own.
- Receipt: the attr rows are the station evidence the `Model/query#ELEMENT_SET` `ByProperty` range arm selects over (a station-interval query is `Range` over `PositioningRows.Station` with ZERO query edits), the `Rasm.AppUi` station-addressed reports render, and a setting-out or progress-reporting consumer keys on — the IFC4.3 infra support deepened from spatial-tree-only to the stationing axis this page's `Bridge`/`Road`/`Railway` `SpatialClass` rows already claim.
- Packages: GeometryGymIFC_Core (`IfcAlignment`/`IfcAlignmentSegment`/`IfcAlignmentParameterSegment` concretes, `IfcLinearPlacement`, `IfcPointByDistanceExpression`, `IfcReferent`, `IfcRelPositions` — decompile-verified members), Rasm.Element (the seam `PropertyName`/`PropertyCategory` custody pair and `MeasureValue`), LanguageExt.Core, Rasm (the kernel `Op`).
- Growth: a new segment parameter is one `PositioningRows` static and one slot on the owning segment arm; a new positioning entity family is one arm on the `Attrs` switch and zero `SourceBag` edits (the synthesis dispatches on the returned map); a station-interval query, a per-alignment rollup, or a station-sorted schedule composes the existing query algebra over the landed rows — never a positioning-specific selection surface.
- Boundary: the reader emits SI SCALARS onto Import bags — the alignment curve, the segment start points, and the placement basis curve are geometry the inline prohibition keeps off bags and edges, content-keyed in `Representations` and resolved one-hop by `Rasm.Compute`; every row name resolves to a `PositioningRows` static, the bare IFC4.3 EXPRESS attribute names minting through the owner-blessed empty-prefix `PropertyCategory.Seam` and the two Bim-DERIVED names through `PropertyCategory.Bim`, so a call-site `PropertyName.Create` — which forks the bag key space against the query and `Rasm.AppUi` readers that key these rows by name — is the deleted form, and a name a non-referencing peer begins keying on is PROMOTED to a `Rasm.Element` owner static rather than re-declared here; the alignment↔element join is the rostered `Generic("IfcRelPositions")` edge, never a bag-row duplicate of graph topology; the synthesized bag is ingest-landed evidence the egress skips like its `PortAttributeSet`/`StructuralDefinitionSet` peers — the `IfcLinearPlacement` re-author is a NAMED bounded drop (a re-emitted infra model re-anchors placement from its content-keyed geometry, the station rows riding the fidelity receipt), and forcing a phantom placement entity from scalar rows is the deleted form; stationing INTERPRETATION stays this page's — the segment-geometry evaluation (station→cartesian) is the kernel/Compute lane's over the content-keyed curves.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// The positioning deep reader — the StructuralProjection.Attrs idiom over the IFC4.3 linear-referencing surface;
// composed by the Projection/semantic#SEMANTIC_PROJECTOR SourceBag synthesis, the bag symbol declared beside its
// peers on the projector.
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Projection;
using Rasm.Element.Properties;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [CONSTANTS] --------------------------------------------------------------------------
// Positioning row vocabulary, PUBLIC because the Model/query#ELEMENT_SET ByProperty facets and the Rasm.AppUi
// station reports key on these names — a reader composes the static, never a literal. The bare IFC4.3 EXPRESS
// attribute names an alignment round-trip froze mint through PropertyCategory.Seam (the owner-blessed EMPTY prefix
// that keeps a round-tripped name bare); SegmentKind and Station are Bim-DERIVED evidence with no EXPRESS
// counterpart — the predefined token folded to a row, the station resolved off IfcCurveMeasureSelect — so they
// mint under PropertyCategory.Bim and can never collide with a schema attribute in the seam's open key space.
public static class PositioningRows {
    public static readonly PropertyName SegmentKind = PropertyCategory.Bim.Row("SegmentKind");
    // The IfcCurveMeasureSelect carries EITHER a length along the alignment OR a dimensionless curve parameter,
    // and the two are different quantities: one row carrying both signs one dimension for two measures, so a
    // station-band Range over a length bound would silently admit a parameter value at the same magnitude. Two
    // rows, two dimensions — a consumer selects the one its bound is dimensioned for.
    public static readonly PropertyName Station = PropertyCategory.Bim.Row("Station");
    public static readonly PropertyName StationParameter = PropertyCategory.Bim.Row("StationParameter");

    public static readonly PropertyName StartDirection = PropertyCategory.Seam.Row("StartDirection");
    public static readonly PropertyName StartRadiusOfCurvature = PropertyCategory.Seam.Row("StartRadiusOfCurvature");
    public static readonly PropertyName EndRadiusOfCurvature = PropertyCategory.Seam.Row("EndRadiusOfCurvature");
    public static readonly PropertyName SegmentLength = PropertyCategory.Seam.Row("SegmentLength");
    public static readonly PropertyName GravityCenterLineHeight = PropertyCategory.Seam.Row("GravityCenterLineHeight");
    public static readonly PropertyName StartDistAlong = PropertyCategory.Seam.Row("StartDistAlong");
    public static readonly PropertyName HorizontalLength = PropertyCategory.Seam.Row("HorizontalLength");
    public static readonly PropertyName StartHeight = PropertyCategory.Seam.Row("StartHeight");
    public static readonly PropertyName StartGradient = PropertyCategory.Seam.Row("StartGradient");
    public static readonly PropertyName EndGradient = PropertyCategory.Seam.Row("EndGradient");
    public static readonly PropertyName RadiusOfCurvature = PropertyCategory.Seam.Row("RadiusOfCurvature");
    public static readonly PropertyName StartCantLeft = PropertyCategory.Seam.Row("StartCantLeft");
    public static readonly PropertyName EndCantLeft = PropertyCategory.Seam.Row("EndCantLeft");
    public static readonly PropertyName StartCantRight = PropertyCategory.Seam.Row("StartCantRight");
    public static readonly PropertyName EndCantRight = PropertyCategory.Seam.Row("EndCantRight");
    public static readonly PropertyName RestartDistance = PropertyCategory.Seam.Row("RestartDistance");
    public static readonly PropertyName OffsetLateral = PropertyCategory.Seam.Row("OffsetLateral");
    public static readonly PropertyName OffsetVertical = PropertyCategory.Seam.Row("OffsetVertical");
    public static readonly PropertyName OffsetLongitudinal = PropertyCategory.Seam.Row("OffsetLongitudinal");
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class PositioningProjection {
    // One entity's positioning facts -> the typed attr rows the SourceBag Import bag carries. A non-positioning
    // entity yields the empty map (no bag minted); a malformed magnitude faults typed through MeasureValue.OfSi.
    public static Fin<Map<PropertyName, PropertyValue>> Attrs(IfcObjectDefinition definition, UnitScale scale, Op key) =>
        definition switch {
            IfcAlignmentSegment segment => segment.DesignParameters switch {
                IfcAlignmentHorizontalSegment h => Rows(key,
                    (PositioningRows.SegmentKind, Token(h.PredefinedType.ToString())),
                    (PositioningRows.StartDirection, Angle(h.StartDirection, scale)),
                    (PositioningRows.StartRadiusOfCurvature, Length(h.StartRadiusOfCurvature, scale)),
                    (PositioningRows.EndRadiusOfCurvature, Length(h.EndRadiusOfCurvature, scale)),
                    (PositioningRows.SegmentLength, Length(h.SegmentLength, scale)),
                    (PositioningRows.GravityCenterLineHeight, Length(h.GravityCenterLineHeight, scale))),
                IfcAlignmentVerticalSegment v => Rows(key,
                    (PositioningRows.SegmentKind, Token(v.PredefinedType.ToString())),
                    (PositioningRows.StartDistAlong, Length(v.StartDistAlong, scale)),
                    (PositioningRows.HorizontalLength, Length(v.HorizontalLength, scale)),
                    (PositioningRows.StartHeight, Length(v.StartHeight, scale)),
                    (PositioningRows.StartGradient, Ratio(v.StartGradient)),
                    (PositioningRows.EndGradient, Ratio(v.EndGradient)),
                    (PositioningRows.RadiusOfCurvature, Length(v.RadiusOfCurvature, scale))),
                IfcAlignmentCantSegment c => Rows(key,
                    (PositioningRows.SegmentKind, Token(c.PredefinedType.ToString())),
                    (PositioningRows.StartDistAlong, Length(c.StartDistAlong, scale)),
                    (PositioningRows.HorizontalLength, Length(c.HorizontalLength, scale)),
                    (PositioningRows.StartCantLeft, Length(c.StartCantLeft, scale)),
                    (PositioningRows.EndCantLeft, Length(c.EndCantLeft, scale)),
                    (PositioningRows.StartCantRight, Length(c.StartCantRight, scale)),
                    (PositioningRows.EndCantRight, Length(c.EndCantRight, scale))),
                _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
            },
            IfcReferent referent => Rows(key, (PositioningRows.RestartDistance, Length(referent.RestartDistance, scale))),
            IfcProduct { ObjectPlacement: IfcLinearPlacement { Distance: { } distance } } => Rows(key,
                (PositioningRows.Station, distance.DistanceAlong is IfcLengthMeasure along
                    ? Length(along.Measure, scale)
                    : Option<Fin<PropertyValue>>.None),
                (PositioningRows.StationParameter, distance.DistanceAlong is IfcParameterValue parameter
                    ? Ratio(parameter.Measure)
                    : Option<Fin<PropertyValue>>.None),
                (PositioningRows.OffsetLateral, Length(distance.OffsetLateral, scale)),
                (PositioningRows.OffsetVertical, Length(distance.OffsetVertical, scale)),
                (PositioningRows.OffsetLongitudinal, Length(distance.OffsetLongitudinal, scale))),
            _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
        };

    // The row fold: absent slots (a NaN GG default, an unset optional) drop before the rail so an unset radius
    // never fabricates a zero row; a present-but-malformed magnitude faults typed through the slot's own rail.
    // Each slot carries the owner-declared PropertyName itself, so no name is spelled between roster and bag.
    static Fin<Map<PropertyName, PropertyValue>> Rows(Op key, params ReadOnlySpan<(PropertyName Name, Option<Fin<PropertyValue>> Value)> slots) =>
        toSeq(Iterable<(PropertyName Name, Option<Fin<PropertyValue>> Value)>.FromSpan(slots))
            .Choose(static slot => slot.Value.Map(fin => fin.Map(value => (slot.Name, Value: value))))
            .TraverseM(identity).As()
            .Map(static rows => rows.Fold(Map<PropertyName, PropertyValue>(),
                static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)));

    // Native->SI runs through the folder's ONE UnitScale.Coerce pair keyed by the slot's MeasureRow — the axis the
    // row signs picks the factor, so no site multiplies a bare scale field. The declared-unit argument is null:
    // an alignment attribute is a plain EXPRESS measure carrying no per-value IfcUnit override.
    static Option<Fin<PropertyValue>> Length(double native, UnitScale scale) =>
        double.IsFinite(native)
            ? Some(MeasureValue.OfSi(Dimension.LengthDim, scale.Coerce(native, MeasureRow.Length, null)).Map(static m => (PropertyValue)new PropertyValue.Measure(m)))
            : Option<Fin<PropertyValue>>.None;

    static Option<Fin<PropertyValue>> Angle(double native, UnitScale scale) =>
        double.IsFinite(native)
            ? Some(MeasureValue.OfSi(Dimension.Dimensionless, scale.Coerce(native, MeasureRow.Angle, null)).Map(static m => (PropertyValue)new PropertyValue.Measure(m)))
            : Option<Fin<PropertyValue>>.None;

    static Option<Fin<PropertyValue>> Ratio(double native) =>
        double.IsFinite(native)
            ? Some(MeasureValue.OfSi(Dimension.Dimensionless, native).Map(static m => (PropertyValue)new PropertyValue.Measure(m)))
            : Option<Fin<PropertyValue>>.None;

    static Option<Fin<PropertyValue>> Token(string value) =>
        value is { Length: > 0 } ? Some(Fin.Succ<PropertyValue>(new PropertyValue.Text(value))) : Option<Fin<PropertyValue>>.None;
}
```

## [04]-[RESEARCH]

(none)
