# [PERSISTENCE_QUERY_TOPOLOGY]

Rasm.Persistence answers authoritative topology synchronously from a kind-filtered `QuikGraph` view built once per read snapshot off the `Element/graph#GRAPH_PROJECTION` `ElementGraph` — this is the DEFAULT topology owner, never an external graph database. The seam snapshot already freezes the one `FrozenDictionary<NodeId, ImmutableArray<Relationship>>` incidence index (read O(degree) through `graph.EdgesAt`) and a kind-AGNOSTIC `BidirectionalGraph<NodeId, SEdge<NodeId>>` view (`graph.Topology()`) that `Bake` needs anyway; this lane composes the ONE thing the seam view cannot express — a kind-FILTERED `BidirectionalGraph<NodeId, TypedEdge>` whose `TypedEdge` carries the neutral `Relationship` so a containment-only ancestry, a connection-only adjacency, and a void-only host-feature walk each traverse a different subgraph of the same edge set. Every algorithm is a `QuikGraph` composition off the `AlgorithmExtensions` facade — `TreeBreadthFirstSearch` reachability, `OfflineLeastCommonAncestor` nearest-common-container, `ShortestPathsDijkstra` unit-weight paths (the geometry-weighted A* route is the `Query/cypher` `pgrouting` lane's, which holds the `H3Cell` node space the seam topology lane does not), `StronglyConnectedComponents`/`WeaklyConnectedComponents` cycle and island analysis, `SourceFirstTopologicalSort` Kahn ordering, `Roots`/`Sinks` spatial-structure anchors — so the topology lane never hand-rolls a graph walk and never re-freezes a second incidence structure beside the one the seam owns. Apache AGE (`Query/cypher`) is an OPTIONAL self-hosted-only analytical projection demoted beneath this; the in-process view is the one a `Bake` needs anyway, so building the incidence index twice is the deleted form. `EdgeFilter` is the neutral edge-kind selection vocabulary every traversal routes through; `TopologyQuery` is the request `[Union]` discriminant; `TopologyView` is the content-keyed memoized view family; `TopologyResult` is the typed receipt; `TopologyFault` is the closed band a query rooted at an absent node or ordered over a cyclic graph rails. `NodeId`, `Relationship`, `RelationshipKind`, `ComposeKind`, `ElementGraph` arrive from `Rasm.Element`; the inline projection arrives from `Element/graph`; `ContentAddress.OfGraph` arrives from `Projection/address`; `ElementSet`/`SetExpr`/`StalenessWatermark` arrive from `Query/lane`; the clock frame ingredient arrives as a VALUE on the Persistence-owned `ProjectionContext` port-input shape (`Element/graph` defines it — no AppHost type crosses down); `CommunityToolkit.HighPerformance` arrives from the substrate. `Rasm.Bim` consumes this view by REFERENCE — its soft spatial anchors resolve traversal results into `Query/cache` reuse rows (the `Rasm.Bim` -> topology -> cache read chain), and no Bim type crosses down.

## [01]-[INDEX]

- [01]-[GRAPH_TOPOLOGY]: the neutral `EdgeFilter` kind vocabulary, the `TypedEdge` relationship-carrying QuikGraph edge, the kind-filtered view built off the seam-frozen edge set and incidence index, and the content-keyed memoized snapshot view boundary.
- [02]-[TRAVERSAL]: the `TopologyQuery` request family, the `TopologyResult` typed receipt, the `TopologyFault` band, and the `Traversals` static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, connection adjacency, void resolution, nearest-common-container, shortest path, components, islands, topological order, anchors, and cycle detection — every result an `ElementSet`.

## [02]-[GRAPH_TOPOLOGY]

- Owner: `EdgeFilter` the `[SmartEnum<string>]` neutral edge-kind selection vocabulary carrying a `Func<Relationship, bool>` admit predicate (`All`/`Containment`/`Spatial`/`Connection`/`Void`/`Assignment`) — a traversal filters the view through a filter ROW, never a re-derived inline `is Relationship.Compose` predicate the neutral seam algebra does not own; `TypedEdge` the `IEdge<NodeId>` QuikGraph edge carrying the seam `Relationship` and its `Endpoints`-projected source/target so the kind-filtered view keeps the neutral seam edge on the arc — every kind read happens at ADMISSION through the `EdgeFilter` row's predicate over the `Relationship` itself, never through re-derived edge accessors; `TopologyView` the content-keyed memoized view family; `GraphTopology` the static surface owning the kind-filtered `BidirectionalGraph<NodeId, TypedEdge>` build off the seam edge set and the content-keyed memo.
- Cases: `EdgeFilter` rows are `All` (every edge — the full reachability/cycle/island graph), `Containment` (`Compose { SubKind: Contain }` only — the narrow `IfcRelContainedInSpatialStructure` element→placement edge a pure storey-membership query reads), `Spatial` (`Compose { SubKind: Contain | Aggregate }` — the FULL IFC spatial-structure tree the ancestry/descent/LCA/anchors walks climb, the `IfcRelAggregates` site→building→storey decomposition the storey→element containment hangs off), `Connection` (`Connect` only — MEP/path adjacency), `Void` (`Void` only — host→feature opening resolution), `Assignment` (`Assign` only — group/system/type membership); a `TypedEdge` carries its `Relationship` so a traversal filters by neutral edge kind without a parallel `RelationshipKind` enum the seam does not duplicate.
- Entry: `public static BidirectionalGraph<NodeId, TypedEdge> View(ElementGraph graph, EdgeFilter filter)` builds the kind-filtered QuikGraph view off `graph.Edges` (the seam's frozen edge set) admitting only edges the filter accepts; `public static BidirectionalGraph<NodeId, TypedEdge> Reversed(ElementGraph graph, EdgeFilter filter)` builds the same view with each edge's endpoints swapped so a forward search walks UP the relation (containment ancestry); `TopologyView.Of(ElementGraph)` mints the per-snapshot memo carrier stamping the `ContentAddress.OfGraph(graph)` as its identity, `view.Filtered(filter)`/`view.Ascending(filter)` reads the per-`(filter, ascending)` forward/reversed view from the view's own cache so a re-query of one snapshot under the same filter reuses the built view, and a `GraphDelta` (a fresh snapshot minting a fresh `TopologyView` with a fresh address and a fresh empty cache) builds it anew.
- Auto: the view is built ONCE per read snapshot off the seam edge set — the live authoring/delta path uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta`, O(log n) edits) and the seam freezes the incidence index plus the kind-agnostic `SEdge` view at the read-snapshot boundary (`Graph/element#ELEMENT_GRAPH` `Of`/`Topology`), so this lane NEVER re-derives the incidence index — a degree read goes through `graph.EdgesAt(node)` and only the kind-FILTERED `TypedEdge` view (the one structure the seam's `SEdge` view cannot express) is built here, the `TopologyView` minted per read snapshot carrying its `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` graph content hash as identity so a `GraphDelta` (a fresh address) means a fresh view with a fresh per-instance `(filter, ascending)` cache; `View` admits an edge through `filter.Admit(edge.Edge)` and `AddVerticesAndEdge` (which adds both endpoints if absent) plus an `AddVertexRange` over `graph.Nodes.Keys` so an isolated node still participates in ordering; `Reversed` constructs each admitted `TypedEdge` with `(Target, Source)` swapped so a forward `TreeBreadthFirstSearch` ascends the spatial-structure tree without a non-existent graph-reverse member.
- Receipt: a view build rides `store.topology.build` carrying the filter key and the node/edge count; a memo hit rides `store.topology.memo` carrying the content address.
- Packages: QuikGraph (`BidirectionalGraph`/`IEdge<TVertex>`/`AddVerticesAndEdge`/`AddVertexRange`), Rasm.Element (`ElementGraph`/`Relationship`/`Relationship.Compose`/`RelationshipKind`/`ComposeKind`/`NodeId`/`EdgesAt`/`ContentAddress.OfGraph`), CommunityToolkit.HighPerformance, System.Collections.Frozen, System.Collections.Immutable, BCL inbox.
- Growth: a new edge filter is one `EdgeFilter` row carrying its `Relationship`-kind predicate; a new view orientation is the existing forward/reversed pair; zero new surface — an external graph database for authoritative topology, a per-read whole-edge scan, or a SECOND incidence structure beside the one the seam snapshot freezes is the deleted form because the seam owns the incidence index (`EdgesAt`) and the kind-agnostic view (`Topology()`), and this lane owns only the kind-filtered `TypedEdge` view.
- Boundary: the in-process QuikGraph view is the DEFAULT topology owner (`H5`) — Apache AGE is demoted to an OPTIONAL self-hosted-only analytical read projection (`Query/cypher`), so authoritative containment/connection/void topology reads bind THIS view and never assume one PostgreSQL instance hosts both AGE and Marten; the seam owns the incidence index and the kind-agnostic view and FREEZES them once (`H3`/`H4`), so this lane reads degree through `graph.EdgesAt` and builds only the kind-FILTERED `TypedEdge` view — re-deriving the incidence index here is the deleted duplication the seam's frozen index makes unnecessary, and building it twice is the named defect; the live HAMT delta path and the frozen read snapshot are distinct phases (`H4`) and the kind-filtered view memoizes on the `ContentAddress.OfGraph` content address so an unchanged graph reuses the view and a `GraphDelta` invalidates it; the `EdgeFilter.Containment` row reads the seam `Relationship.IsContainment` (`Compose { SubKind: Contain }`, NOT every `Compose` flavor) so the NARROW `Containment` filter never mis-admits an `Aggregate`/`Nest`/`Reference` edge — but the spatial ANCESTRY/descent/LCA/anchors walks route through the wider `Spatial` filter (`Contain` UNION `Aggregate`), the full IFC spatial-structure tree, because a Contain-only ancestry would reach an element's storey and stop short of the building/site/project the `IfcRelAggregates` decomposition models (two walls in different buildings would then share no container, the silently-incomplete spatial tree the deleted form); the topology lane is the synchronous authoritative correctness lane (`Query/lane#READ_ROUTING`) so a containment/void-resolution query reads the inline-projection-derived view, never the daemon-lagged columnar/cypher lane.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Immutable;
using LanguageExt;
using QuikGraph;
using Rasm.Element;
using Thinktecture;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// The neutral edge-kind selection vocabulary — POLICY_VALUES over the seam's neutral RelationshipKind, the admit
// predicate carried as a constructor delegate so a traversal filters the view through a ROW (filter.Admit(edge))
// rather than a re-derived inline `is Relationship.Compose` the neutral algebra does not own. TWO containment-
// flavored rows answer two distinct questions: `Containment` is the seam's own narrow IsContainment predicate
// (Compose.Contain ONLY — the IfcRelContainedInSpatialStructure element->placement relation), while `Spatial` is
// the FULL IFC spatial-structure tree (Compose.Contain UNION Compose.Aggregate — the IfcRelAggregates
// site->building->storey decomposition the storey->element containment hangs off), the tree spatial ancestry,
// nearest-common-container, and the spatial-root anchors must climb. A Contain-only ancestry would reach an
// element's storey and STOP — it could never climb storey->building->site, so two walls in different buildings
// would share NO container; routing the spatial walks through `Spatial` (matching the seam Bake's owning-Compose
// recursion over Aggregate/Nest/Contain) is the correctness fix, `Containment` staying the narrow placement edge
// a pure storey-membership query reads. A new filter is one row; the IFC roster stays in Bim's projector — the
// seam carries five neutral kinds plus a Generic passthrough.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeFilter {
    public static readonly EdgeFilter All         = new("all", static _ => true);
    public static readonly EdgeFilter Containment = new("containment", static r => r.IsContainment);
    public static readonly EdgeFilter Spatial     = new("spatial", static r => r is Relationship.Compose { SubKind: var k } && (k == ComposeKind.Contain || k == ComposeKind.Aggregate));
    public static readonly EdgeFilter Connection  = new("connection", static r => r.Kind == RelationshipKind.Connect);
    public static readonly EdgeFilter Void        = new("void", static r => r.Kind == RelationshipKind.Void);
    public static readonly EdgeFilter Assignment  = new("assignment", static r => r.Kind == RelationshipKind.Assign);
    [UseDelegateFromConstructor] public partial bool Admit(Relationship edge);
}

// The QuikGraph edge carrying the neutral seam Relationship — a consumer-defined IEdge<NodeId>. The seam's own
// SEdge<NodeId> view (graph.Topology()) carries no relationship and so cannot filter; this is the ONE structure
// THIS lane owns. Kind reads happen ONCE at view admission (`filter.Admit(edge)` over the Relationship), so the
// edge carries the seam Relationship as payload and re-exposes NO kind accessors — the audited `IsContainment`/
// `Kind` delegating properties had zero call sites and are pruned as dead carriers. Source/Target are the seam
// Endpoints projection (or its swap for ascent).
public readonly record struct TypedEdge(NodeId Source, NodeId Target, Relationship Edge) : IEdge<NodeId>;

// --- [MODELS] -----------------------------------------------------------------------------
// The content-keyed memoized view family: the snapshot, its content address (the seam ContentAddress.OfGraph — NOT
// a second hasher), and the per-(filter, orientation) view cache. Filtered/Ascending read the kind-filtered forward
// or endpoint-swapped view, building each at most once per snapshot and reusing it across queries; a fresh snapshot
// from a GraphDelta carries a fresh address and a fresh cache, so the live HAMT delta path never pays the build.
public readonly record struct TopologyView(ElementGraph Graph, UInt128 Address, ConcurrentDictionary<(string Filter, bool Ascending), BidirectionalGraph<NodeId, TypedEdge>> Cache) {
    public static TopologyView Of(ElementGraph graph) => new(graph, ContentAddress.OfGraph(graph).Value, new());
    public BidirectionalGraph<NodeId, TypedEdge> Filtered(EdgeFilter filter) => Cache.GetOrAdd((filter.Key, false), _ => GraphTopology.View(Graph, filter));
    public BidirectionalGraph<NodeId, TypedEdge> Ascending(EdgeFilter filter) => Cache.GetOrAdd((filter.Key, true), _ => GraphTopology.Reversed(Graph, filter));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GraphTopology {
    // Build the kind-filtered forward view off the seam's FROZEN edge set (graph.Edges) — NEVER re-deriving the
    // seam-owned incidence index (graph.EdgesAt is read directly for degree). AddVertexRange admits isolates so an
    // unconnected node still orders; AddVerticesAndEdge adds endpoints if absent. The filter admits per the neutral
    // RelationshipKind, so a Containment view holds only the Contain-flavored Compose edges the seam IsContainment names.
    public static BidirectionalGraph<NodeId, TypedEdge> View(ElementGraph graph, EdgeFilter filter) {
        var view = new BidirectionalGraph<NodeId, TypedEdge>(allowParallelEdges: true, vertexCapacity: graph.Nodes.Count);
        view.AddVertexRange(graph.Nodes.Keys);
        foreach (var edge in graph.Edges)
            if (filter.Admit(edge)) { var (relating, related) = edge.Endpoints; view.AddVerticesAndEdge(new TypedEdge(relating, related, edge)); }
        return view;
    }

    // The endpoint-swapped view: each admitted edge becomes (Target, Source) so a FORWARD TreeBreadthFirstSearch
    // ascends the relation (spatial ancestry = walking whole<-part up the Spatial Contain+Aggregate tree). QuikGraph
    // exposes no instance graph-reverse for a custom edge type (ReversedBidirectionalGraph re-types edges as
    // SReversedEdge, incompatible with the TypedEdge algorithms), so the reversal is expressed at construction over the seam edge.
    public static BidirectionalGraph<NodeId, TypedEdge> Reversed(ElementGraph graph, EdgeFilter filter) {
        var view = new BidirectionalGraph<NodeId, TypedEdge>(allowParallelEdges: true, vertexCapacity: graph.Nodes.Count);
        view.AddVertexRange(graph.Nodes.Keys);
        foreach (var edge in graph.Edges)
            if (filter.Admit(edge)) { var (relating, related) = edge.Endpoints; view.AddVerticesAndEdge(new TypedEdge(related, relating, edge)); }
        return view;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | default topology    | in-process QuikGraph view              | AGE demoted to optional self-hosted (`H5`)                |
|  [02]   | incidence index     | seam-frozen `graph.EdgesAt` (read-only) | NEVER re-derived here; a second index is the deleted form (`H3`) |
|  [03]   | filtered view owner | the kind-filtered `TypedEdge` view     | the one structure the seam `SEdge` view cannot express    |
|  [04]   | edge filter         | `EdgeFilter` row + admit predicate     | the seam `IsContainment`; never an inline re-derived `is` |
|  [05]   | memo key            | `ContentAddress.OfGraph` (seam hasher) | a `GraphDelta` invalidates; never a second hasher         |

## [03]-[TRAVERSAL]

- Owner: `TopologyQuery` the `[Union]` traversal-request family; `TopologyResult` the typed result `[Union]`; `TopologyFault` the closed `Expected`-band the absent-root, cyclic-order, and cyclic-LCA rejections rail; `Traversals` the static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, direct placement, group/system membership, connection adjacency, void resolution, nearest-common-container, shortest path, strongly/weakly connected components, topological order, spatial-structure anchors, and cycle detection.
- Cases: on `TopologyQuery` — `Ancestry(Node, Depth)` (spatial-tree upward walk over `Contain`+`Aggregate`), `Descent(Node, Depth)` (spatial-tree downward walk), `Connected(Node)` (connection-edge SYMMETRIC adjacency — both edge directions), `Resolve(Host)` (void-edge host→feature openings — the `Query/lane#READ_ROUTING` void-resolution correctness query), `Placement(Element)` (the NARROW `Containment` filter — the single directly-containing spatial structure an element is placed in, the `IfcRelContainedInSpatialStructure` storey distinct from the full `Spatial` ancestry climb), `Members(Group)` (the `Assignment` filter — every member of a group/system/zone, the `IfcRelAssignsToGroup` reverse read), `Ancestor(Left, Right)` (nearest common spatial container — the `IfcRelAggregates`+`IfcRelContainedInSpatialStructure` tree LCA), `Path(From, To)` (shortest containment/connection path), `Components` (strongly-connected partitions for cycle isolation), `Islands` (weakly-connected components — disconnected model partitions), `Anchors` (spatial-structure roots and dangling sinks), `Order` (topological dependency ordering), `Cycles` (containment-cycle detection); on `TopologyResult` — `Reached(ElementSet)`, `Route(Seq<NodeId>, double)`, `Common(Option<NodeId>)`, `Partitions(Seq<ElementSet>)`, `Ordered(Seq<NodeId>)`, `Cyclic(Seq<ElementSet>)`.
- Entry: `public static Fin<TopologyResult> Run(TopologyView view, TopologyQuery query)` dispatches the query to its QuikGraph algorithm through the generated total `Switch` (no runtime-silent arm), railing `TopologyFault.RootAbsent` when a rooted query names a node the snapshot never declares and `TopologyFault.Cyclic` when `Order` is asked over a cyclic graph OR `Ancestor` over a cyclic spatial tree — Tarjan `OfflineLeastCommonAncestor` holds a rooted-tree contract that is UNSOUND over cyclic/multi-parent input, so the LCA pre-gates `tree.IsDirectedAcyclicGraph()` exactly as `Order` does; every reachability/partition traversal returns an `ElementSet`-compatible key set so a topology result composes with the `Query/lane#ELEMENT_SET_ALGEBRA` selection currency.
- Auto: spatial ancestry runs `TreeBreadthFirstSearch` over the ASCENDING `Spatial` view (the full `Contain`+`Aggregate` spatial-structure tree) bounded by depth and descent over the FORWARD `Spatial` view, the bounded reach folded off the seam-frozen incidence degree; connection adjacency unions `view.OutEdges` AND `view.InEdges` over the connection view because an `IfcRelConnectsElements` join is undirected — an out-only read would drop every peer that authored the edge from the other side; direct placement reads the element's out-edge over the ASCENDING narrow `Containment` view so the single directly-containing storey resolves without climbing the full ancestry, and group membership reads the group's out-edge over the ASCENDING `Assignment` view so the `Assign{Group}` reverse read yields every member; void resolution reads the host's void-filtered out-edges so a `Void`/`Fill` opening resolves synchronously at the authoritative lane; nearest-common-container pre-gates the `Spatial` tree with `IsDirectedAcyclicGraph` (topology models containment cycles as REAL — the `Version/merge#STRUCTURAL_DIFF` `ContainmentCycle` conflict — and Tarjan's offline LCA over a cyclic tree returns arbitrary ancestors rather than throwing, the silent-wrong-answer class the gate converts to a typed fault) then runs `OfflineLeastCommonAncestor` keyed on one `SEquatableEdge<NodeId>` query pair, folded over EVERY `Roots()` entry (the `Spatial` view is a forest — federated sites and detached islands each mint a root — so an arbitrary single-root pick would silently mask a pair under a later root as no-ancestor) so two elements in different buildings resolve to their shared site/project rather than stranding at a storey; shortest path runs `ShortestPathsDijkstra` over unit edge weights — the topology lane has no node coordinate (the seam holds geometry by content-hash reference, not inline), so the geometry-weighted A* route lives in the `Query/cypher` `pgrouting` lane over the `H3Cell` node space, not here; components, the cycle arm, and the topological-cycle fallback all route ONE `StronglyConnectedComponents` composition (any component of size > 1 is a cycle) rather than inlining it three times; islands run `WeaklyConnectedComponents`; anchors read `Roots`/`Sinks`; topological order runs `SourceFirstTopologicalSort` (Kahn) gated by `IsDirectedAcyclicGraph` so a residual cycle rails `TopologyFault.Cyclic` before the order is trusted; every result projects to an `ElementSet` so "every element under room X" is a `Descent` whose key set composes with any other selection.
- Receipt: a traversal rides `store.topology.<query>` carrying the reached count and the depth; a cycle detection rides `store.topology.cycle` carrying the cycle count; an absent-root rejection rides the `TopologyFault.RootAbsent` rail.
- Packages: QuikGraph (`AlgorithmExtensions.TreeBreadthFirstSearch`/`OfflineLeastCommonAncestor`/`ShortestPathsDijkstra`/`StronglyConnectedComponents`/`WeaklyConnectedComponents`/`SourceFirstTopologicalSort`/`IsDirectedAcyclicGraph`/`Roots`/`Sinks`, `BidirectionalGraph.OutEdges`/`InEdges`, `SEquatableEdge`/`TryFunc`), Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new traversal is one `TopologyQuery` case plus one `AlgorithmExtensions` composition reading the matching `EdgeFilter` view; zero new surface — a hand-rolled BFS/DFS, a recursive ltree CTE for in-memory ancestry, a second path solver, or a silent empty-result fallback is the deleted form because QuikGraph owns the graph algorithms, every result is an `ElementSet`, and an absent root rails the typed band.
- Boundary: every traversal is a QuikGraph `AlgorithmExtensions` composition, never a hand-rolled walk — a recursive descent over the edge sequence is the deleted form because the seam-frozen incidence index plus the kind-filtered view answer O(degree); the dispatch is the generated `query.Switch(...)` so the union is exhaustive at compile time with NO runtime-silent `_` arm, and an absent root is a typed `TopologyFault.RootAbsent` on the `Fin` rail rather than a silent `ElementSet.Empty` (the deleted form that fakes a successful empty query) — the one-endpoint queries gate through `Rooted` and the two-endpoint pair queries (`Ancestor`/`Path`) through `Paired` so EITHER absent endpoint rails `RootAbsent`, never a silent empty `Route`/`Common(None)` that masks an unknown vertex behind a no-path/no-ancestor result; the result is always an `ElementSet`-compatible key set so a topology query composes with the selection algebra (a `Descent` intersected with a `Classification` selection is one `SetExpr.Intersect`); cycle detection routes the ONE `StronglyConnectedComponents` composition the `Components`, `Cycles`, and `Order`-fallback arms share rather than inlining the probe three times, so a containment-cycle (the `Version/merge#STRUCTURAL_DIFF` `ContainmentCycle` conflict) is caught synchronously at the authoritative lane; the two algorithms with an acyclicity CONTRACT — `SourceFirstTopologicalSort` and `OfflineLeastCommonAncestor` — are SYMMETRICALLY gated by the cheap `IsDirectedAcyclicGraph` predicate railing `TopologyFault.Cyclic`, because the sort would silently drop the cyclic remainder and the offline LCA would certify an arbitrary ancestor, two silent-wrong-answer forms one gate deletes; void resolution and nearest-common-container are first-class query cases so the `Query/lane#READ_ROUTING` void-resolution correctness query has its in-process owner rather than a prose-only claim; connection adjacency is SYMMETRIC (`OutEdges` ∪ `InEdges`) because the neutral `Connect` edge is an undirected join, an out-only read being the directionally-incomplete deleted form; `Placement` and `Members` make the narrow `Containment` and the `Assignment` filter rows load-bearing — the single directly-containing storey (distinct from the full `Spatial` ancestry) and the group/system reverse membership read are first-class queries, never prose-justified dead vocabulary rows; the shortest path is `ShortestPathsDijkstra` over unit edge weights because the seam node carries no inline coordinate (geometry is content-hash-by-reference resolved through an app-wired `GeometrySource` this lane does not compose), so the geometry-weighted A* routing query is the `pgrouting` analytical lane's (`Query/cypher`) over the `H3Cell` node space — the two meet at the result `ElementSet`, the in-process lane owning topological distance and the analytical lane the metric route, never one duplicating the other's algorithm.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyQuery {
    private TopologyQuery() { }
    public sealed record Ancestry(NodeId Node, int Depth) : TopologyQuery;
    public sealed record Descent(NodeId Node, int Depth) : TopologyQuery;
    public sealed record Connected(NodeId Node) : TopologyQuery;
    public sealed record Resolve(NodeId Host) : TopologyQuery;
    public sealed record Placement(NodeId Element) : TopologyQuery;
    public sealed record Members(NodeId Group) : TopologyQuery;
    public sealed record Ancestor(NodeId Left, NodeId Right) : TopologyQuery;
    public sealed record Path(NodeId From, NodeId To) : TopologyQuery;
    public sealed record Components : TopologyQuery;
    public sealed record Islands : TopologyQuery;
    public sealed record Anchors : TopologyQuery;
    public sealed record Order : TopologyQuery;
    public sealed record Cycles : TopologyQuery;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyResult {
    private TopologyResult() { }
    public sealed record Reached(ElementSet Set) : TopologyResult;
    public sealed record Route(Seq<NodeId> Path, double Cost) : TopologyResult;
    public sealed record Common(Option<NodeId> Ancestor) : TopologyResult;
    public sealed record Partitions(Seq<ElementSet> Components) : TopologyResult;
    public sealed record Ordered(Seq<NodeId> Topological) : TopologyResult;
    public sealed record Cyclic(Seq<ElementSet> Cycles) : TopologyResult;
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The closed Persistence-query topology band (837x): a [Union] over the KERNEL `Rasm.Domain.Expected`
// (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation
// base the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)`
// ctor (no `Category` to override) is the deleted form. The case lifts BARE onto the Fin rail through the Expected
// derivation (no `.ToError()` hop), no `[GenerateUnionOps]` (the kernel union-ops generator is strictly opt-in — the
// band wants no per-case `SelfOp`), the `[Union]`-generated `Switch`/`Map` untouched; band membership derives `Code => FaultBand.Topology + n` through the one
// `Element/graph#FAULT_TABLES` registry pointer (837x — a duplicate decade integer fails at type init). RootAbsent: a rooted query (ancestry/descent/connected/resolve/ancestor/path)
// names a NodeId the snapshot never declares. Cyclic: Order OR Ancestor is asked over a graph the
// IsDirectedAcyclicGraph pre-gate rejects — the sort would silently drop the cyclic remainder and the Tarjan
// offline LCA (a rooted-tree contract) would certify an arbitrary ancestor — the detail naming the cyclic-partition
// count; the offending partitions themselves are recovered through the Cycles query (the same one SCC probe), never thrown.
[Union]
public abstract partial record TopologyFault : Expected, IValidationError<TopologyFault> {
    private TopologyFault() : base() { }
    public sealed record RootAbsent(string Detail) : TopologyFault;
    public sealed record Cyclic(string Detail) : TopologyFault;

    public override int Code => FaultBand.Topology + Switch(
        rootAbsent: static _ => 0,
        cyclic:     static _ => 1);

    public override string Message => Switch(
        rootAbsent: static c => $"<topology-root-absent:{c.Detail}>",
        cyclic:     static c => $"<topology-cyclic-order:{c.Detail}>");

    public override string Category => Switch(
        rootAbsent: static _ => "RootAbsent",
        cyclic:     static _ => "Cyclic");

    public static TopologyFault Create(string message) => new RootAbsent(message);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Traversals {
    public static Fin<TopologyResult> Run(TopologyView view, TopologyQuery query) => query.Switch(
        ancestry:   a => Rooted(view, a.Node, () => new TopologyResult.Reached(Walk(view.Ascending(EdgeFilter.Spatial), a.Node, a.Depth))),
        descent:    d => Rooted(view, d.Node, () => new TopologyResult.Reached(Walk(view.Filtered(EdgeFilter.Spatial), d.Node, d.Depth))),
        connected:  c => Rooted(view, c.Node, () => new TopologyResult.Reached(Incident(view.Filtered(EdgeFilter.Connection), c.Node))),
        resolve:    r => Rooted(view, r.Host, () => new TopologyResult.Reached(Adjacent(view.Filtered(EdgeFilter.Void), r.Host))),
        placement:  p => Rooted(view, p.Element, () => new TopologyResult.Reached(Adjacent(view.Ascending(EdgeFilter.Containment), p.Element))),
        members:    m => Rooted(view, m.Group, () => new TopologyResult.Reached(Adjacent(view.Ascending(EdgeFilter.Assignment), m.Group))),
        ancestor:   a => Paired(view, a.Left, a.Right, () => Lca(view, a.Left, a.Right)),
        path:       p => Paired(view, p.From, p.To, () => Fin.Succ(Shortest(view.Filtered(EdgeFilter.All), p.From, p.To))),
        components: _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Components(view))),
        islands:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Islands(view))),
        anchors:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Reached(Anchors(view))),
        order:      _ => Topological(view),
        cycles:     _ => Fin.Succ<TopologyResult>(new TopologyResult.Cyclic(toSeq(Components(view).Filter(static c => c.Count > 1)))));

    // The one rooted-query guard: a query naming a node the snapshot never declares rails RootAbsent rather than
    // returning a silent empty set, so an absent-root read is an honest typed fault on the Fin rail.
    static Fin<TopologyResult> Rooted(TopologyView view, NodeId root, Func<TopologyResult> run) =>
        view.Graph.Nodes.ContainsKey(root) ? Fin.Succ(run()) : Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(root.Value));

    // The TWO-endpoint guard the pair queries (Ancestor/Path) take: EITHER endpoint the snapshot never declares
    // rails RootAbsent, so a Path(existing, absent) is an honest typed fault rather than a silent empty Route and
    // an Ancestor(existing, absent) a fault rather than a silent Common(None) — the same RootAbsent honesty the
    // one-endpoint Rooted guard owns, extended to both ends so neither pair query masks an absent node behind a
    // no-path / no-ancestor result the algorithm would otherwise produce for an unknown vertex. `run` yields the
    // rail so a gated body (the Lca DAG pre-gate) composes its own Cyclic fault through the one guard.
    static Fin<TopologyResult> Paired(TopologyView view, NodeId left, NodeId right, Func<Fin<TopologyResult>> run) =>
        !view.Graph.Nodes.ContainsKey(left) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(left.Value))
        : !view.Graph.Nodes.ContainsKey(right) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(right.Value))
        : run();

    // Bounded reachability through the QuikGraph TreeBreadthFirstSearch accessor — paths(v, out _) is true exactly
    // for the reachable v, the bound applied by the seam-frozen incidence degree fold off the directed view; the
    // root is excluded so the reached set is the descendants/ancestors alone. The ascending Spatial view (the full
    // Contain+Aggregate spatial-structure tree) gives ancestry, the forward view descent — one Walk over a
    // pre-oriented view, no graph-reverse member.
    static ElementSet Walk(BidirectionalGraph<NodeId, TypedEdge> view, NodeId root, int depth) {
        var paths = view.TreeBreadthFirstSearch(root);
        var bounded = view.Vertices.Where(v => v != root && paths(v, out var edges) && Enumerable.Count(edges) <= depth);
        return ElementSet.Of(toSeq(bounded));
    }

    // The DIRECTIONAL out-neighbour set — a Void host->feature resolution reads the host's openings (the Void edge is
    // directional Host->Feature, so the forward out-edges ARE the openings and an in-edge would be a different host).
    static ElementSet Adjacent(BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node) ? ElementSet.Of(toSeq(view.OutEdges(node).Select(static e => e.Target))) : ElementSet.Empty;

    // The SYMMETRIC incident-neighbour set — a Connect edge is undirected adjacency (an IfcRelConnectsElements joins two
    // peers with no owning direction), so a connection query reads BOTH the out-targets (node as From) AND the in-sources
    // (node as To): an out-only read would silently drop every peer that authored the edge from the other side, the
    // directionally-incomplete MEP/path adjacency the deleted form. BidirectionalGraph carries InEdges, so the symmetric
    // closure is one union over the kind-filtered Connection view, never a second reversed view build.
    static ElementSet Incident(BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? ElementSet.Of(toSeq(view.OutEdges(node).Select(static e => e.Target).Concat(view.InEdges(node).Select(static e => e.Source))))
            : ElementSet.Empty;

    // Nearest common spatial container — QuikGraph Tarjan offline LCA over the FORWARD Spatial tree (the full
    // Contain+Aggregate spatial-structure decomposition, parent whole->part, the orientation Tarjan offline LCA
    // requires), keyed on the one SEquatableEdge<NodeId> query pair. Tarjan offline LCA holds a ROOTED-TREE
    // contract — over a cyclic spatial tree it certifies an arbitrary ancestor instead of throwing, and topology
    // models containment cycles as REAL (the merge ContainmentCycle conflict) — so the tree is pre-gated by the
    // same cheap IsDirectedAcyclicGraph predicate that gates Order, railing TopologyFault.Cyclic symmetrically.
    // The Spatial view is a FOREST, not one guaranteed tree — federated sites and detached islands each mint a
    // Roots() entry — so the query folds over EVERY root, first resolved ancestor wins: an arbitrary single-root
    // pick would silently answer Common(None) for a pair living under a later root, the masked-wrong-answer
    // form; a pair genuinely split across two trees resolves None from every root, the honest no-shared-container.
    static Fin<TopologyResult> Lca(TopologyView view, NodeId left, NodeId right) {
        var tree = view.Filtered(EdgeFilter.Spatial);
        var pair = new SEquatableEdge<NodeId>(left, right);
        return !tree.IsDirectedAcyclicGraph()
            ? Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(toSeq(Components(view).Filter(static c => c.Count > 1)).Count.ToString()))
            : Fin.Succ<TopologyResult>(new TopologyResult.Common(
                toSeq(tree.Roots()).Fold(Option<NodeId>.None, (held, root) =>
                    held.IsSome
                        ? held
                        : tree.OfflineLeastCommonAncestor(root, [pair]) is var lca && lca(pair, out var ancestor) ? Some(ancestor) : None)));
    }

    static TopologyResult Shortest(BidirectionalGraph<NodeId, TypedEdge> view, NodeId from, NodeId to) {
        var paths = view.ShortestPathsDijkstra(static _ => 1.0, from);
        return paths(to, out var edges)
            ? new TopologyResult.Route(from.Cons(toSeq(edges.Select(static e => e.Target))), Enumerable.Count(edges))
            : new TopologyResult.Route(Seq<NodeId>(), double.PositiveInfinity);
    }

    // The ONE strongly-connected partition body — the Components, Cycles, and Order-fallback arms all route here
    // rather than each inlining StronglyConnectedComponents, so the SCC composition is owned once (the DERIVED_LOGIC
    // collapse of three near-identical arm bodies); the all-edge view itself is reused from the per-snapshot view cache
    // (the SCC label fold re-runs per call — the view build, not the partition, is what the cache amortizes).
    static Seq<ElementSet> Components(TopologyView view) {
        var labels = new System.Collections.Generic.Dictionary<NodeId, int>();
        view.Filtered(EdgeFilter.All).StronglyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static kv => kv.Value).Select(static g => ElementSet.Of(toSeq(g.Select(static kv => kv.Key)))));
    }

    static Seq<ElementSet> Islands(TopologyView view) {
        var labels = new System.Collections.Generic.Dictionary<NodeId, int>();
        view.Filtered(EdgeFilter.All).WeaklyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static kv => kv.Value).Select(static g => ElementSet.Of(toSeq(g.Select(static kv => kv.Key)))));
    }

    // The spatial-structure anchors — the no-incoming roots and no-outgoing sinks of the FULL Spatial tree
    // (Contain+Aggregate), so a root is the true project/site (not a storey a Contain-only view would mistake for
    // a root because its building link is an Aggregate edge) and a sink is a leaf element with nothing contained.
    static ElementSet Anchors(TopologyView view) {
        var spatial = view.Filtered(EdgeFilter.Spatial);
        return ElementSet.Of(toSeq(spatial.Roots().Concat(spatial.Sinks())));
    }

    // Kahn source-first order gated by the cheap DAG predicate: a residual cycle rails TopologyFault.Cyclic (the
    // detail naming the cyclic-partition count, the partitions recovered through Cycles) rather than letting
    // SourceFirstTopologicalSort silently drop the cyclic remainder, the IsDirectedAcyclicGraph pre-gate sparing the
    // NonAcyclicGraphException the throwing DFS TopologicalSort would surface.
    static Fin<TopologyResult> Topological(TopologyView view) {
        var graph = view.Filtered(EdgeFilter.All);
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ<TopologyResult>(new TopologyResult.Ordered(toSeq(graph.SourceFirstTopologicalSort())))
            : Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(toSeq(Components(view).Filter(static c => c.Count > 1)).Count.ToString()));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | algorithm owner     | QuikGraph `AlgorithmExtensions` facade | never a hand-rolled walk or recursive CTE                 |
|  [02]   | dispatch            | the generated `query.Switch(...)`      | exhaustive at compile time; no runtime-silent `_` arm     |
|  [03]   | absent root         | `Rooted` (one endpoint) / `Paired` (both ends) → `TopologyFault.RootAbsent` | a typed fault, never a silent `ElementSet.Empty`/`Route`/`Common(None)` |
|  [04]   | result shape        | `ElementSet`-compatible key set        | a topology result composes with the selection algebra     |
|  [05]   | cycle detection     | one `StronglyConnectedComponents` probe | shared by `Cycles`/`Order`; never recomputed per arm      |
|  [06]   | void resolution     | first-class `Resolve` query case       | the `READ_ROUTING` correctness query has an in-process owner |
|  [07]   | nearest container   | `IsDirectedAcyclicGraph`-gated `OfflineLeastCommonAncestor` over the `Spatial` tree | rooted-tree contract enforced — `Cyclic` railed, never an arbitrary ancestor; `Contain`+`Aggregate`, never stranded at a storey |
|  [08]   | shortest path       | `ShortestPathsDijkstra` unit weights   | no node coordinate here; the geometry-weighted A* is the `pgrouting` lane's |
|  [09]   | spatial walks       | the `Spatial` filter (`Contain`∪`Aggregate`) | ancestry/descent/LCA/anchors climb the full tree; `Containment` stays the narrow placement edge |
|  [10]   | connection adjacency| `OutEdges` ∪ `InEdges` over `Connection` | symmetric — an undirected join reads both sides, never out-only |
|  [11]   | placement / members | the narrow `Containment` / `Assignment` filters | `Placement` is the one containing storey, `Members` the group reverse read; both filter rows live |
