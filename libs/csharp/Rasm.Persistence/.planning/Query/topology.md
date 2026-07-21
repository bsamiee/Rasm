# [PERSISTENCE_QUERY_TOPOLOGY]

Rasm.Persistence answers authoritative topology synchronously from one kind-filtered `QuikGraph` view per read snapshot. `ElementGraph` already freezes the incidence index and kind-agnostic graph; this lane adds only the filtered `TypedEdge` views needed for containment, connection, assignment, and void walks. `TopologyView` memoizes those views by content, filter, and orientation, while `Advance` validates a `GraphDelta` through `ElementGraph.Apply` and patches only materialized views. Every traversal composes `QuikGraph.AlgorithmExtensions`; no second incidence index or hand-rolled graph walk exists. `EdgeFilter` owns selection, `TopologyQuery` owns requests, `TopologyResult` owns receipts, and `TopologyFault` owns rejection. `Rasm.Bim` consumes results by reference through the topology-to-cache read chain, and no Bim type crosses down.

## [01]-[INDEX]

- [01]-[GRAPH_TOPOLOGY]: the neutral `EdgeFilter` kind vocabulary, the `TypedEdge` relationship-carrying QuikGraph edge, the kind-filtered view built off the seam-frozen edge set and incidence index, and the content-keyed memoized snapshot view boundary.
- [02]-[TRAVERSAL]: the `TopologyQuery` request family, the `TopologyResult` typed receipt, the `TopologyFault` band, and the `Traversals` static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, connection adjacency, void resolution, nearest-common-container, shortest path, components, islands, topological order, anchors, and cycle detection — every result an `ElementSet`.

## [02]-[GRAPH_TOPOLOGY]

- Owner: `EdgeFilter` is the neutral edge-kind selection vocabulary; `EdgeOrientation` projects the seam endpoints forward or ascending; `TypedEdge` carries the admitted `Relationship`; `TopologyView` is the snapshot-local reference owner for graph identity and its mutable view memo; `GraphTopology.Build` is the polymorphic kind-and-orientation builder.
- Cases: `EdgeFilter` rows are `All` (every edge — the full reachability/cycle/island graph), `Containment` (`Compose { SubKind: Contain }` only — the narrow `IfcRelContainedInSpatialStructure` element→placement edge a pure storey-membership query reads), `Spatial` (`Compose { SubKind: Contain | Aggregate }` — the FULL IFC spatial-structure tree the ancestry/descent/LCA/anchors walks climb, the `IfcRelAggregates` site→building→storey decomposition the storey→element containment hangs off), `Connection` (`Connect` only — MEP/path adjacency), `Void` (`Void` only — host→feature opening resolution), `Assignment` (`Assign` only — group/system/type membership); a `TypedEdge` carries its `Relationship` so a traversal filters by neutral edge kind without a parallel `RelationshipKind` enum the seam does not duplicate.
- Entry: `GraphTopology.Build(ElementGraph, EdgeFilter, EdgeOrientation)` builds the admitted view once; `TopologyView.View(EdgeFilter, EdgeOrientation)` memoizes the result under both row keys, and `Advance(GraphDelta, Op)` validates the next frozen snapshot through `ElementGraph.Apply` before cloning and patching every materialized filtered view.
- Auto: the view is built ONCE per read snapshot off the seam edge set — the live authoring/delta path uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta`, O(log n) edits) and the seam freezes the incidence index plus the kind-agnostic `SEdge` view at the read-snapshot boundary (`Graph/element#ELEMENT_GRAPH` `Of`/`Topology`), so this lane NEVER re-derives the incidence index — a degree read goes through `graph.EdgesAt(node)` and only the kind-FILTERED `TypedEdge` view is built here; `Advance` clones only already-demanded filter/orientation views, removes delta edges and vertices, then adds vertices and admitted projected edges, preserving isolated nodes and moving the memo to the next `ContentAddress.OfGraph` identity in `O(delta)` per active view; an untouched filter remains an unbuilt cache entry rather than paying an eager scan.
- Receipt: a view build rides `store.topology.build` carrying the filter key and the node/edge count; a memo hit rides `store.topology.memo` carrying the content address.
- Packages: QuikGraph (`BidirectionalGraph`/`IEdge<TVertex>`/`AddVerticesAndEdge`/`AddVertexRange`), Rasm.Element (`ElementGraph`/`Relationship`/`Relationship.Compose`/`RelationshipKind`/`ComposeKind`/`NodeId`/`EdgesAt`/`ContentAddress.OfGraph`), CommunityToolkit.HighPerformance, System.Collections.Frozen, System.Collections.Immutable, BCL inbox.
- Growth: a new edge filter is one `EdgeFilter` row carrying its `Relationship`-kind predicate; a new view orientation is the existing forward/reversed pair; `Advance` applies the same row predicates to delta edges, so growth needs no parallel incremental dispatcher; zero new surface — an external graph database for authoritative topology, a per-read whole-edge scan, or a SECOND incidence structure beside the one the seam snapshot freezes is the deleted form because the seam owns the incidence index (`EdgesAt`) and the kind-agnostic view (`Topology()`), and this lane owns only the kind-filtered `TypedEdge` view.
- Boundary: the in-process QuikGraph view is the default authoritative topology owner; Apache AGE remains an optional analytical projection. `EdgeFilter.Containment` admits only `Compose { SubKind: Contain }`, while spatial ancestry, descent, LCA, and anchors use `Spatial` to include `Aggregate`. `Advance` validates through `ElementGraph.Apply` and patches this lane's filtered memo without becoming a second graph-mutation or incidence owner.

```csharp signature
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using LanguageExt;
using QuikGraph;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// `EdgeFilter` rows carry neutral relationship predicates; Bim retains the IFC roster.
// `Containment` admits direct placement, while `Spatial` includes aggregation for full-tree walks.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeFilter {
    public static readonly EdgeFilter All         = new("all", static _ => true);
    public static readonly EdgeFilter Containment = new("containment", static r => r.IsContainment);
    public static readonly EdgeFilter Spatial     = new("spatial", static r => r is Relationship.Compose { SubKind: ComposeKind.Contain or ComposeKind.Aggregate });
    public static readonly EdgeFilter Connection  = new("connection", static r => r.Kind == RelationshipKind.Connect);
    public static readonly EdgeFilter Void        = new("void", static r => r.Kind == RelationshipKind.Void);
    public static readonly EdgeFilter Assignment  = new("assignment", static r => r.Kind == RelationshipKind.Assign);
    [UseDelegateFromConstructor] public partial bool Admit(Relationship edge);
}

// `TypedEdge` retains the neutral `Relationship` needed for filter admission.
// Endpoints project forward or ascending without duplicating kind accessors.
public readonly record struct TypedEdge(NodeId Source, NodeId Target, Relationship Edge) : IEdge<NodeId>;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeOrientation {
    public static readonly EdgeOrientation Forward = new("forward", static edge => {
        (NodeId Relating, NodeId Related) endpoints = edge.Endpoints;
        return new TypedEdge(endpoints.Relating, endpoints.Related, edge);
    });
    public static readonly EdgeOrientation Ascending = new("ascending", static edge => {
        (NodeId Relating, NodeId Related) endpoints = edge.Endpoints;
        return new TypedEdge(endpoints.Related, endpoints.Relating, edge);
    });
    [UseDelegateFromConstructor] public partial TypedEdge Project(Relationship edge);
}

// --- [MODELS] -----------------------------------------------------------------------------
// `TopologyView` carries the snapshot, seam content identity, and filter-orientation memo.
// `Advance` patches only demanded views after the next graph is admitted.
public sealed class TopologyView {
    private readonly ConcurrentDictionary<(EdgeFilter Filter, EdgeOrientation Orientation), BidirectionalGraph<NodeId, TypedEdge>> cache = new();
    public ElementGraph Graph { get; }
    public UInt128 Address { get; }

    private TopologyView(ElementGraph graph) => (Graph, Address) = (graph, ContentAddress.OfGraph(graph).Value);

    public static TopologyView Of(ElementGraph graph) => new(graph);

    public BidirectionalGraph<NodeId, TypedEdge> View(EdgeFilter filter, EdgeOrientation orientation) =>
        cache.GetOrAdd((filter, orientation), static (_, state) => GraphTopology.Build(state.Graph, state.Filter, state.Orientation),
            (Graph, Filter: filter, Orientation: orientation));

    public Fin<TopologyView> Advance(GraphDelta delta, Op key) =>
        Graph.Apply(delta, key).Map(next => Advanced(next, delta));

    TopologyView Advanced(ElementGraph next, GraphDelta delta) {
        TopologyView advanced = new(next);
        cache.Iter(entry => {
            (EdgeFilter Filter, EdgeOrientation Orientation) policy = entry.Key;
            BidirectionalGraph<NodeId, TypedEdge> view = entry.Value.Clone();
            delta.RemovedEdges.Filter(policy.Filter.Admit).Iter(edge => view.RemoveEdge(policy.Orientation.Project(edge)));
            delta.RemovedNodes.Iter(node => view.RemoveVertex(node));
            delta.AddedNodes.Iter(node => view.AddVertex(node.Id));
            delta.AddedEdges.Filter(policy.Filter.Admit).Iter(edge => view.AddVerticesAndEdge(policy.Orientation.Project(edge)));
            advanced.cache.TryAdd(policy, view);
        });
        return advanced;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GraphTopology {
    // Build filters the frozen seam edge set without re-deriving its incidence index.
    // `AddVertexRange` preserves isolates, and filter predicates admit projected relationships once.
    public static BidirectionalGraph<NodeId, TypedEdge> Build(ElementGraph graph, EdgeFilter filter, EdgeOrientation orientation) {
        BidirectionalGraph<NodeId, TypedEdge> view = new(allowParallelEdges: true, vertexCapacity: graph.Nodes.Count);
        view.AddVertexRange(graph.Nodes.Keys);
        foreach (Relationship edge in graph.Edges) {
            if (filter.Admit(edge)) { view.AddVerticesAndEdge(orientation.Project(edge)); }
        }
        return view;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                 | [BINDING]                                                        |
| :-----: | :------------------ | :-------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | default topology    | in-process QuikGraph view               | AGE demoted to optional self-hosted (`H5`)                       |
|  [02]   | incidence index     | seam-frozen `graph.EdgesAt` (read-only) | NEVER re-derived here; a second index is the deleted form (`H3`) |
|  [03]   | filtered view owner | the kind-filtered `TypedEdge` view      | the one structure the seam `SEdge` view cannot express           |
|  [04]   | edge filter         | `EdgeFilter` row + admit predicate      | the seam `IsContainment`; never an inline re-derived `is`        |
|  [05]   | memo key            | `ContentAddress.OfGraph` (seam hasher)  | a `GraphDelta` advances identity; never a second hasher          |
|  [06]   | delta advance       | `TopologyView.Advance(GraphDelta, Op)`  | clones active views and patches admitted rows in `O(delta)`      |

## [03]-[TRAVERSAL]

- Owner: `TopologyQuery` the `[Union]` traversal-request family; `TopologyResult` the typed result `[Union]`; `TopologyFault` the closed `Expected`-band the absent-root, cyclic-order, and cyclic-LCA rejections rail; `Traversals` the static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, direct placement, group/system membership, connection adjacency, void resolution, nearest-common-container, shortest path, strongly/weakly connected components, topological order, spatial-structure anchors, and cycle detection.
- Cases: `TopologyQuery` covers bounded reach, adjacency, containment, shortest path, partitions, ordering, cycles, and reduction. `TopologyResult` distinguishes `Common` from `Unrelated` and `Route` from `Disconnected`; no empty path or absent ancestor acts as a sentinel. `Pruned` carries redundant edge pairs in edge space.
- Entry: `Run` dispatches through the generated total `Switch`; rooted queries rail `RootAbsent`; LCA rails `Cyclic` for cycles and `NotForest` for multiple parents before invoking `OfflineLeastCommonAncestor`; cycle recovery includes singleton self-loops.
- Auto: spatial ancestry runs `TreeBreadthFirstSearch` over the ascending `Spatial` view and descent over the forward view. Connection adjacency unions `OutEdges` and `InEdges` because `IfcRelConnectsElements` is an undirected join. Placement uses ascending `Containment`; membership uses ascending `Assignment`; void resolution uses forward `Void`. LCA folds `OfflineLeastCommonAncestor` over every forest root after an acyclicity gate. Shortest path uses unit-weight `ShortestPathsDijkstra`; metric routing remains in the `pgrouting` lane. Components and cycles share `StronglyConnectedComponents`; islands use `WeaklyConnectedComponents`; order uses gated `SourceFirstTopologicalSort`; redundancy diffs the spatial view against `ComputeTransitiveReduction`. Every reachability result projects to `ElementSet`.
- Receipt: a traversal rides `store.topology.traverse` carrying the query case, the reached count, and the depth; a cycle detection rides `store.topology.cycle` carrying the cycle count; an absent-root rejection rides the `TopologyFault.RootAbsent` rail.
- Packages: QuikGraph (`AlgorithmExtensions.TreeBreadthFirstSearch`/`OfflineLeastCommonAncestor`/`ShortestPathsDijkstra`/`StronglyConnectedComponents`/`WeaklyConnectedComponents`/`SourceFirstTopologicalSort`/`IsDirectedAcyclicGraph`/`ComputeTransitiveReduction`/`Roots`/`Sinks`, `BidirectionalGraph.OutEdges`/`InEdges`/`ContainsEdge`, `SEquatableEdge`/`TryFunc`), Rasm.Element, Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `WalkDepth`/`SelectionFault` — the admitted bounded-depth axis), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new traversal is one `TopologyQuery` case plus one `AlgorithmExtensions` composition reading the matching `EdgeFilter` view; zero new surface — a hand-rolled BFS/DFS, a recursive ltree CTE for in-memory ancestry, a second path solver, or a silent empty-result fallback is the deleted form because QuikGraph owns the graph algorithms, every result is an `ElementSet`, and an absent root rails the typed band.
- Boundary: every traversal composes `AlgorithmExtensions`; generated `query.Switch(...)` dispatch remains exhaustive. `Rooted` and `Paired` convert unknown endpoints to `TopologyFault.RootAbsent`, while `IsDirectedAcyclicGraph` gates order, LCA, and reduction. `Placement`, `Members`, `Void`, and symmetric connection adjacency make every filter row operational. Topological distance remains in-process, metric distance remains in `pgrouting`, and both return `ElementSet`-compatible keys.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyQuery {
    private TopologyQuery() { }
    public sealed record Ancestry(NodeId Node, WalkDepth Depth) : TopologyQuery;
    public sealed record Descent(NodeId Node, WalkDepth Depth) : TopologyQuery;
    public sealed record Neighbors(NodeId Node, Neighborhood Kind) : TopologyQuery;
    public sealed record Ancestor(NodeId Left, NodeId Right) : TopologyQuery;
    public sealed record Path(NodeId From, NodeId To) : TopologyQuery;
    public sealed record Components : TopologyQuery;
    public sealed record Islands : TopologyQuery;
    public sealed record Anchors : TopologyQuery;
    public sealed record Order : TopologyQuery;
    public sealed record Cycles : TopologyQuery;
    public sealed record Redundant : TopologyQuery;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Neighborhood {
    public static readonly Neighborhood Connected = new("connected", EdgeFilter.Connection, EdgeOrientation.Forward, symmetric: true);
    public static readonly Neighborhood Openings = new("openings", EdgeFilter.Void, EdgeOrientation.Forward, symmetric: false);
    public static readonly Neighborhood Placement = new("placement", EdgeFilter.Containment, EdgeOrientation.Ascending, symmetric: false);
    public static readonly Neighborhood Members = new("members", EdgeFilter.Assignment, EdgeOrientation.Ascending, symmetric: false);
    public EdgeFilter Filter { get; }
    public EdgeOrientation Orientation { get; }
    public bool Symmetric { get; }
    private Neighborhood(string key, EdgeFilter filter, EdgeOrientation orientation, bool symmetric) : this(key) =>
        (Filter, Orientation, Symmetric) = (filter, orientation, symmetric);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyResult {
    private TopologyResult() { }
    public sealed record Reached(ElementSet Set) : TopologyResult;
    public sealed record Route(Seq<NodeId> Path, double Cost) : TopologyResult;
    public sealed record Common(NodeId Ancestor) : TopologyResult;
    public sealed record Unrelated(NodeId Left, NodeId Right) : TopologyResult;
    public sealed record Disconnected(NodeId From, NodeId To) : TopologyResult;
    public sealed record Partitions(Seq<ElementSet> Components) : TopologyResult;
    public sealed record Ordered(Seq<NodeId> Topological) : TopologyResult;
    public sealed record Cyclic(Seq<ElementSet> Cycles) : TopologyResult;
    public sealed record Pruned(Seq<(NodeId From, NodeId To)> Redundant) : TopologyResult;
}

// --- [ERRORS] -----------------------------------------------------------------------------
// `TopologyFault` closes `FaultBand.Topology` over `Rasm.Domain.Expected`.
// Missing roots, cycles, and non-forest structure rail before graph algorithms can return plausible wrong answers.
[Union]
public abstract partial record TopologyFault : Expected, IValidationError<TopologyFault> {
    private TopologyFault() : base() { }
    public sealed record RootAbsent(string Detail) : TopologyFault;
    public sealed record Cyclic(string Detail) : TopologyFault;
    public sealed record NotForest(string Node) : TopologyFault;

    public override int Code => FaultBand.Topology + Switch(
        rootAbsent: static _ => 0,
        cyclic:     static _ => 1,
        notForest:  static _ => 2);

    public override string Message => Switch(
        rootAbsent: static c => $"<topology-root-absent:{c.Detail}>",
        cyclic:     static c => $"<topology-cyclic-order:{c.Detail}>",
        notForest:  static c => $"<topology-multiple-parents:{c.Node}>");

    public override string Category => Switch(
        rootAbsent: static _ => "RootAbsent",
        cyclic:     static _ => "Cyclic",
        notForest:  static _ => "NotForest");

    public static TopologyFault Create(string message) => new RootAbsent(message);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Traversals {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.topology.build"), StoreSlot.Create("store.topology.memo"), StoreSlot.Create("store.topology.traverse"),
        StoreSlot.Create("store.topology.cycle"));

    public static Fin<TopologyResult> Run(TopologyView view, TopologyQuery query) => query.Switch(
        ancestry:   a => Rooted(view, a.Node, () => new TopologyResult.Reached(Walk(view.View(EdgeFilter.Spatial, EdgeOrientation.Ascending), a.Node, a.Depth.Value))),
        descent:    d => Rooted(view, d.Node, () => new TopologyResult.Reached(Walk(view.View(EdgeFilter.Spatial, EdgeOrientation.Forward), d.Node, d.Depth.Value))),
        neighbors:  n => Rooted(view, n.Node, () => new TopologyResult.Reached(Neighbors(view, n.Node, n.Kind))),
        ancestor:   a => Paired(view, a.Left, a.Right, () => Lca(view, a.Left, a.Right)),
        path:       p => Paired(view, p.From, p.To, () => Fin.Succ(Shortest(view.View(EdgeFilter.All, EdgeOrientation.Forward), p.From, p.To))),
        components: _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Components(view))),
        islands:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Islands(view))),
        anchors:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Reached(Anchors(view))),
        order:      _ => Topological(view),
        cycles:     _ => Fin.Succ<TopologyResult>(new TopologyResult.Cyclic(Cycles(view.View(EdgeFilter.All, EdgeOrientation.Forward)))),
        redundant:  _ => Reduction(view));

    // Rooted-query guard rails `RootAbsent` when a query names a node absent from the snapshot, rather than
    // returning a silent empty set, so an absent-root read is an honest typed fault on the Fin rail.
    static Fin<TopologyResult> Rooted(TopologyView view, NodeId root, Func<TopologyResult> run) =>
        view.Graph.Nodes.ContainsKey(root) ? Fin.Succ(run()) : Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(root.Value));

    // `Paired` rails either absent endpoint before path or ancestor algorithms execute.
    // Its body retains its own `Fin` so LCA cycle and forest faults compose.
    static Fin<TopologyResult> Paired(TopologyView view, NodeId left, NodeId right, Func<Fin<TopologyResult>> run) =>
        !view.Graph.Nodes.ContainsKey(left) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(left.Value))
        : !view.Graph.Nodes.ContainsKey(right) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(right.Value))
        : run();

    // `TreeBreadthFirstSearch` supplies bounded reachability over a pre-oriented view.
    // Ascending spatial views produce ancestry; forward views produce descent; roots remain excluded.
    static ElementSet Walk(BidirectionalGraph<NodeId, TypedEdge> view, NodeId root, int depth) {
        TryFunc<NodeId, IEnumerable<TypedEdge>> paths = view.TreeBreadthFirstSearch(root);
        IEnumerable<NodeId> bounded = view.Vertices.Where(v => v != root && paths(v, out IEnumerable<TypedEdge>? edges) && Enumerable.Count(edges) <= depth);
        return ElementSet.Of(toSeq(bounded));
    }

    // DIRECTIONAL out-neighbour set resolves a Void host's openings (the Void edge is
    // directional Host->Feature, so the forward out-edges ARE the openings and an in-edge would be a different host).
    static ElementSet Adjacent(BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node) ? ElementSet.Of(toSeq(view.OutEdges(node).Select(static e => e.Target))) : ElementSet.Empty;

    // Symmetric connection adjacency unions outgoing targets and incoming sources.
    // `BidirectionalGraph` supplies both directions without building a reversed view.
    static ElementSet Incident(BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? ElementSet.Of(toSeq(view.OutEdges(node).Select(static e => e.Target).Concat(view.InEdges(node).Select(static e => e.Source))))
            : ElementSet.Empty;

    static ElementSet Neighbors(TopologyView view, NodeId node, Neighborhood kind) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(kind.Filter, kind.Orientation);
        return kind.Symmetric ? Incident(graph, node) : Adjacent(graph, node);
    }

    // Offline LCA runs only after cycle and multiple-parent gates prove a spatial forest.
    // Folding every root distinguishes unrelated trees from pairs under later federated roots.
    static Fin<TopologyResult> Lca(TopologyView view, NodeId left, NodeId right) {
        BidirectionalGraph<NodeId, TypedEdge> tree = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        SEquatableEdge<NodeId> pair = new(left, right);
        if (!tree.IsDirectedAcyclicGraph()) {
            return Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(tree).Count.ToString(CultureInfo.InvariantCulture)));
        }
        Option<NodeId> ambiguous = toSeq(tree.Vertices).Find(vertex => tree.InDegree(vertex) > 1);
        return ambiguous.Match(
            Some: node => Fin.Fail<TopologyResult>(new TopologyFault.NotForest(node.Value)),
            None: () => toSeq(tree.Roots()).Fold(Option<NodeId>.None, (held, root) =>
                    held.IsSome
                        ? held
                        : ResolveAncestor(tree, root, pair))
                .Match<Fin<TopologyResult>>(
                    Some: ancestor => Fin.Succ<TopologyResult>(new TopologyResult.Common(ancestor)),
                    None: () => Fin.Succ<TopologyResult>(new TopologyResult.Unrelated(left, right))));
    }

    static Option<NodeId> ResolveAncestor(BidirectionalGraph<NodeId, TypedEdge> tree, NodeId root, SEquatableEdge<NodeId> pair) {
        TryFunc<SEquatableEdge<NodeId>, NodeId> resolve = tree.OfflineLeastCommonAncestor(root, [pair]);
        return resolve(pair, out NodeId ancestor) ? Some(ancestor) : None;
    }

    static TopologyResult Shortest(BidirectionalGraph<NodeId, TypedEdge> view, NodeId from, NodeId to) {
        TryFunc<NodeId, IEnumerable<TypedEdge>> paths = view.ShortestPathsDijkstra(static _ => 1.0, from);
        return paths(to, out IEnumerable<TypedEdge>? edges)
            ? new TopologyResult.Route(from.Cons(toSeq(edges.Select(static e => e.Target))), Enumerable.Count(edges))
            : new TopologyResult.Disconnected(from, to);
    }

    // One strongly-connected partition body serves components, cycles, and order failure.
    static Seq<ElementSet> Components(TopologyView view) {
        Dictionary<NodeId, int> labels = [];
        view.View(EdgeFilter.All, EdgeOrientation.Forward).StronglyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static kv => kv.Value).Select(static g => ElementSet.Of(toSeq(g.Select(static kv => kv.Key)))));
    }

    static Seq<ElementSet> Cycles(BidirectionalGraph<NodeId, TypedEdge> graph) {
        Dictionary<NodeId, int> labels = [];
        graph.StronglyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static pair => pair.Value)
            .Where(component => component.Count() > 1 || component.Any(pair => graph.ContainsEdge(pair.Key, pair.Key)))
            .Select(static component => ElementSet.Of(toSeq(component.Select(static pair => pair.Key)))));
    }

    static Seq<ElementSet> Islands(TopologyView view) {
        Dictionary<NodeId, int> labels = [];
        view.View(EdgeFilter.All, EdgeOrientation.Forward).WeaklyConnectedComponents(labels);
        return toSeq(labels.GroupBy(static kv => kv.Value).Select(static g => ElementSet.Of(toSeq(g.Select(static kv => kv.Key)))));
    }

    // Spatial anchors are roots and sinks of the full containment-plus-aggregation tree.
    static ElementSet Anchors(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> spatial = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        return ElementSet.Of(toSeq(spatial.Roots().Concat(spatial.Sinks())));
    }

    // A DAG gate prevents source-first sorting from silently dropping cyclic remainders.
    static Fin<TopologyResult> Topological(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(EdgeFilter.All, EdgeOrientation.Forward);
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ<TopologyResult>(new TopologyResult.Ordered(toSeq(graph.SourceFirstTopologicalSort())))
            : Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(graph).Count.ToString(CultureInfo.InvariantCulture)));
    }

    // Transitive reduction diffs a DAG-gated spatial view against its minimal reachability-equivalent graph.
    // Removed direct edges return as typed redundant pairs.
    static Fin<TopologyResult> Reduction(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> tree = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        if (!tree.IsDirectedAcyclicGraph()) {
            return Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(tree).Count.ToString(CultureInfo.InvariantCulture)));
        }
        BidirectionalGraph<NodeId, TypedEdge> reduced = tree.ComputeTransitiveReduction();
        return Fin.Succ<TopologyResult>(new TopologyResult.Pruned(
            toSeq(tree.Edges.Where(edge => !reduced.ContainsEdge(edge)).Select(static edge => (edge.Source, edge.Target)))));
    }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                        | [BINDING]                                    |
| :-----: | :------------------- | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | algorithm owner      | QuikGraph `AlgorithmExtensions`                | no hand-rolled walk or recursive CTE         |
|  [02]   | dispatch             | generated `query.Switch(...)`                  | exhaustive; no silent `_` arm                |
|  [03]   | absent root          | `Rooted`/`Paired` → `TopologyFault.RootAbsent` | typed fault, never an empty sentinel         |
|  [04]   | result shape         | `ElementSet`-compatible keys                   | composes with selection algebra              |
|  [05]   | cycle detection      | one `StronglyConnectedComponents` probe        | shared by `Cycles` and `Order`               |
|  [06]   | void resolution      | `Neighbors` under `Neighborhood.Openings`      | one polymorphic case, never a Resolve twin   |
|  [07]   | nearest container    | forest-gated `OfflineLeastCommonAncestor`      | cycles and multiple parents rail             |
|  [08]   | shortest path        | unit-weight `ShortestPathsDijkstra`            | metric routing belongs to `pgrouting`        |
|  [09]   | spatial walks        | `Spatial` (`Contain` ∪ `Aggregate`)            | full-tree climb                              |
|  [10]   | connection adjacency | `OutEdges` ∪ `InEdges` over `Connection`       | symmetric read                               |
|  [11]   | placement / members  | `Containment` / `Assignment`                   | direct storey / reverse membership           |
|  [12]   | bounded depth        | lane-owned `WalkDepth`                         | no raw `int` enters `Walk`                   |
|  [13]   | redundancy           | DAG-gated `ComputeTransitiveReduction` diff    | redundant edges return typed `Pruned` pairs  |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
