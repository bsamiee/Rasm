# [PERSISTENCE_QUERY_TOPOLOGY]

Rasm.Persistence answers authoritative topology synchronously from an in-process QuikGraph view built once per read snapshot off the `Element/graph#GRAPH_PROJECTION` `ElementGraph` — this is the DEFAULT topology owner, never an external graph database. The `GraphTopology` builds one `BidirectionalGraph<NodeId, TypedEdge>` from the graph's `Relationship` edges plus the `FrozenDictionary<NodeId, ImmutableArray<Relationship>>` incidence index the snapshot freezes once, so containment ancestry, connection traversal, void-resolution, and cycle detection run in-process against the read-your-writes inline projection rather than a daemon-lagged view. Every algorithm is a QuikGraph composition — `BreadthFirstSearchAlgorithm`/`DepthFirstSearchAlgorithm` traversal, `DijkstraShortestPathAlgorithm`/`AStarShortestPathAlgorithm` paths, `StronglyConnectedComponentsAlgorithm` components, `TopologicalSortAlgorithm` ordering, `IsDescendantOfQuery` containment — so the topology lane never hand-rolls a graph walk. Apache AGE (`Query/cypher`) is an OPTIONAL self-hosted-only analytical projection demoted beneath this; the in-process view is the one a `Bake` needs anyway, so building it twice is the deleted form. `NodeId`, `Relationship`, `ElementGraph` arrive from `Rasm.Element`; the inline projection arrives from `Element/graph`; `ElementSet`/`SetExpr` arrive from `Query/lane`; `ClockPolicy`, `CommunityToolkit.HighPerformance` arrive from the substrate.

## [01]-[INDEX]

- [01]-[GRAPH_TOPOLOGY]: the QuikGraph view over the seam graph, the frozen incidence index, the typed edge, and the build-once snapshot boundary.
- [02]-[TRAVERSAL]: the QuikGraph algorithm compositions — containment ancestry, connection traversal, shortest path, components, topological sort, and cycle detection.

## [02]-[GRAPH_TOPOLOGY]

- Owner: `TypedEdge` the QuikGraph edge carrying the seam `Relationship` and its `Endpoints`-projected source/target (a containment filter is `Edge is Relationship.Compose`, never a parallel `RelationshipKind` enum the seam does not own); `IncidenceIndex` the `FrozenDictionary<NodeId, ImmutableArray<Relationship>>` built once at snapshot freeze; `GraphTopology` the static surface owning the QuikGraph view build, the incidence index build, and the memoized snapshot view keyed on the graph content address.
- Cases: a `TypedEdge` carries its `Relationship` so a traversal can filter by edge kind (containment-only ancestry, connection-only adjacency); the incidence index maps each node to its incident relationships so `Bake` and the topology view both read O(degree) rather than scanning the whole edge sequence.
- Entry: `public static BidirectionalGraph<NodeId, TypedEdge> View(ElementGraph graph)` builds the QuikGraph view from the seam relationships; `public static IncidenceIndex Incidence(ElementGraph graph)` builds the frozen incidence index once; `public static Func<ElementGraph, BidirectionalGraph<NodeId, TypedEdge>> Memoized()` returns a view builder keyed on the graph content address so the view rebuilds only when the graph changes.
- Auto: the view is built ONCE per read snapshot — the live authoring/delta path uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (O(log n) edits), and the topology view freezes to the `BidirectionalGraph` plus the `FrozenDictionary` incidence index only at the read-snapshot boundary, so the delta path never pays the view-build cost and a read snapshot is O(1) lookup; the view is memoized keyed on the `Element/codec#CONTENT_ADDRESS` graph content hash so a re-read of an unchanged graph reuses the built view and a `GraphDelta` invalidates it; the incidence index is the one structure `Bake` (the seam-level fold) and the topology traversals both read so a node's incident edges resolve O(degree).
- Receipt: a view build rides `store.topology.build` carrying the node/edge count; a memo hit rides `store.topology.memo`.
- Packages: QuikGraph (`BidirectionalGraph`/`AdjacencyGraph`), Rasm.Element (`ElementGraph`/`Relationship`/`NodeId`), CommunityToolkit.HighPerformance, System.Collections.Frozen, System.Collections.Immutable, BCL inbox.
- Growth: a new edge filter is one `Relationship`-kind predicate over the typed edge; a new index is one projection off the incidence index; zero new surface — an external graph database for authoritative topology, a per-read whole-edge scan, or a second incidence structure is the deleted form because the QuikGraph view is the default topology owner and the incidence index is the one `Bake` already needs.
- Boundary: the in-process QuikGraph view is the DEFAULT topology owner (`H5`) — Apache AGE is demoted to an OPTIONAL self-hosted-only analytical read projection (`Query/cypher`), so authoritative containment/connection topology reads bind THIS view and never assume one PostgreSQL instance hosts both AGE and Marten; the view is built once at the read-snapshot boundary and memoized on the graph content address, so the live HAMT delta path and the frozen read snapshot are distinct phases (`H4`) and building the view twice is the deleted form; the incidence index is the `FrozenDictionary<NodeId, ImmutableArray<Relationship>>` the snapshot freezes once so `Bake` runs O(degree) and the QuikGraph view builds from it; the topology lane is the synchronous authoritative correctness lane (`Query/lane#READ_ROUTING`) so a containment query reads the inline-projection-derived view, never the daemon-lagged columnar/cypher lane.

```csharp signature
public readonly record struct TypedEdge(NodeId Source, NodeId Target, Relationship Edge) : IEdge<NodeId> {
    public bool IsContainment => Edge is Relationship.Compose;
}

public readonly record struct IncidenceIndex(FrozenDictionary<NodeId, ImmutableArray<Relationship>> ByNode) {
    public ImmutableArray<Relationship> Incident(NodeId node) => ByNode.TryGetValue(node, out var edges) ? edges : ImmutableArray<Relationship>.Empty;
}

public static class GraphTopology {
    public static IncidenceIndex Incidence(ElementGraph graph) {
        var builder = new System.Collections.Generic.Dictionary<NodeId, ImmutableArray<Relationship>.Builder>();
        foreach (var edge in graph.Edges)
            foreach (var endpoint in new[] { edge.Endpoints.Relating, edge.Endpoints.Related })
                (builder.TryGetValue(endpoint, out var slot) ? slot : builder[endpoint] = ImmutableArray.CreateBuilder<Relationship>()).Add(edge);
        return new IncidenceIndex(builder.ToFrozenDictionary(static kv => kv.Key, static kv => kv.Value.ToImmutable()));
    }

    public static BidirectionalGraph<NodeId, TypedEdge> View(ElementGraph graph) {
        var view = new BidirectionalGraph<NodeId, TypedEdge>(allowParallelEdges: true, vertexCapacity: graph.Nodes.Count);
        view.AddVertexRange(graph.Nodes.Keys);
        view.AddEdgeRange(graph.Edges.Map(edge => new TypedEdge(edge.Endpoints.Relating, edge.Endpoints.Related, edge)));
        return view;
    }

    public static Func<ElementGraph, BidirectionalGraph<NodeId, TypedEdge>> Memoized() {
        var cache = new System.Collections.Concurrent.ConcurrentDictionary<UInt128, BidirectionalGraph<NodeId, TypedEdge>>();
        return graph => cache.GetOrAdd(NodeHash.OfGraph(graph).Value, _ => View(graph));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | default topology    | in-process QuikGraph view              | AGE demoted to optional self-hosted (`H5`)                |
|  [02]   | build phase         | freeze once at read-snapshot boundary  | HAMT delta path stays O(log n); view memoized (`H4`)      |
|  [03]   | incidence index     | `FrozenDictionary<NodeId, edges>`      | `Bake` and traversal read O(degree)                       |
|  [04]   | memo key            | graph content address                  | a `GraphDelta` invalidates the built view                 |

## [03]-[TRAVERSAL]

- Owner: `TopologyQuery` the `[Union]` traversal-request family; `TopologyResult` the typed result; `Traversals` the static surface composing the QuikGraph algorithms — containment ancestry/descent, connection adjacency, shortest path, strongly-connected components, topological sort, and cycle detection.
- Cases: `Ancestry(NodeId, depth)` (containment-edge upward walk), `Descent(NodeId, depth)` (containment-edge downward walk), `Connected(NodeId)` (connection-edge adjacency), `Path(NodeId from, NodeId to)` (shortest containment/connection path), `Components` (strongly-connected components for cycle isolation), `Order` (topological sort for dependency ordering), `Cycles` (containment-cycle detection) on `TopologyQuery`.
- Entry: `public static TopologyResult Run(BidirectionalGraph<NodeId, TypedEdge> view, TopologyQuery query)` dispatches the query to its QuikGraph algorithm; every traversal returns an `ElementSet`-compatible key set so a topology result composes with the `Query/lane#ELEMENT_SET_ALGEBRA` selection currency.
- Auto: containment ancestry/descent filters the view to containment edges then runs a `BreadthFirstSearchAlgorithm` bounded by depth; connection adjacency filters to connection edges; shortest path runs `DijkstraShortestPathAlgorithm` (unit edge weights) or `AStarShortestPathAlgorithm` with a spatial heuristic; cycle detection runs `StronglyConnectedComponentsAlgorithm` and reports any component of size > 1 as a cycle; topological sort runs `TopologicalSortAlgorithm` for dependency ordering and faults on a cycle through the same SCC probe; every result projects to an `ElementSet` so a "every element under room X" query is a `Descent` whose key set composes with any other selection.
- Receipt: a traversal rides `store.topology.<query>` carrying the reached count and the depth; a cycle detection rides `store.topology.cycle` carrying the cycle count.
- Packages: QuikGraph (`BreadthFirstSearchAlgorithm`/`DepthFirstSearchAlgorithm`/`DijkstraShortestPathAlgorithm`/`AStarShortestPathAlgorithm`/`StronglyConnectedComponentsAlgorithm`/`TopologicalSortAlgorithm`), Rasm.Element, LanguageExt.Core, BCL inbox.
- Growth: a new traversal is one `TopologyQuery` case plus one QuikGraph algorithm composition; zero new surface — a hand-rolled BFS/DFS, a recursive ltree CTE for in-memory ancestry, or a second path solver is the deleted form because QuikGraph owns the graph algorithms and every result is an `ElementSet`.
- Boundary: every traversal is a QuikGraph algorithm composition, never a hand-rolled walk — a recursive descent over the edge sequence is the deleted form because the incidence index plus the QuikGraph view answer O(degree); the result is always an `ElementSet`-compatible key set so a topology query composes with the selection algebra (a `Descent` intersected with a `Classification` selection is one `SetExpr.Intersect`); cycle detection runs over the in-process view so a containment-cycle (the `Version/merge#STRUCTURAL_DIFF` `ContainmentCycle` conflict) is caught synchronously at the authoritative lane; the shortest-path A* heuristic reads the node spatial envelope so a routing query (the `pgrouting` analytical equivalent in `Query/cypher`) has its in-process counterpart, the two meeting at the result `ElementSet`, never duplicating the algorithm.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyQuery {
    private TopologyQuery() { }
    public sealed record Ancestry(NodeId Node, int Depth) : TopologyQuery;
    public sealed record Descent(NodeId Node, int Depth) : TopologyQuery;
    public sealed record Connected(NodeId Node) : TopologyQuery;
    public sealed record Path(NodeId From, NodeId To) : TopologyQuery;
    public sealed record Components : TopologyQuery;
    public sealed record Order : TopologyQuery;
    public sealed record Cycles : TopologyQuery;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyResult {
    private TopologyResult() { }
    public sealed record Reached(ElementSet Set) : TopologyResult;
    public sealed record Route(Seq<NodeId> Path, double Cost) : TopologyResult;
    public sealed record Partitions(Seq<ElementSet> Components) : TopologyResult;
    public sealed record Ordered(Seq<NodeId> Topological) : TopologyResult;
    public sealed record Cyclic(Seq<ElementSet> Cycles) : TopologyResult;
}

public static class Traversals {
    public static TopologyResult Run(BidirectionalGraph<NodeId, TypedEdge> view, TopologyQuery query) => query switch {
        TopologyQuery.Ancestry a => new TopologyResult.Reached(Walk(Containment(view), a.Node, a.Depth, reverse: true)),
        TopologyQuery.Descent d => new TopologyResult.Reached(Walk(Containment(view), d.Node, d.Depth, reverse: false)),
        TopologyQuery.Connected c => new TopologyResult.Reached(ElementSet.Of(toSeq(view.OutEdges(c.Node).Where(static e => !e.IsContainment).Select(static e => e.Target)))),
        TopologyQuery.Path p => Shortest(view, p.From, p.To),
        TopologyQuery.Components => new TopologyResult.Partitions(StronglyConnected(view)),
        TopologyQuery.Order => Topological(view),
        TopologyQuery.Cycles => new TopologyResult.Cyclic(StronglyConnected(view).Filter(static c => c.Count > 1)),
        _ => new TopologyResult.Reached(ElementSet.Empty),
    };

    static BidirectionalGraph<NodeId, TypedEdge> Containment(BidirectionalGraph<NodeId, TypedEdge> view) {
        var sub = new BidirectionalGraph<NodeId, TypedEdge>(allowParallelEdges: false);
        sub.AddVertexRange(view.Vertices);
        sub.AddEdgeRange(view.Edges.Where(static e => e.IsContainment));
        return sub;
    }

    static ElementSet Walk(BidirectionalGraph<NodeId, TypedEdge> view, NodeId root, int depth, bool reverse) {
        var reached = new System.Collections.Generic.List<NodeId>();
        var bfs = new BreadthFirstSearchAlgorithm<NodeId, TypedEdge>(reverse ? view.ToReversed() : view);
        bfs.ExamineVertex += v => reached.Add(v);
        bfs.Compute(root);
        return ElementSet.Of(toSeq(reached).Filter(v => v != root));
    }

    static TopologyResult Shortest(BidirectionalGraph<NodeId, TypedEdge> view, NodeId from, NodeId to) {
        var dijkstra = new DijkstraShortestPathAlgorithm<NodeId, TypedEdge>(view, static _ => 1.0);
        var recorder = new VertexPredecessorRecorderObserver<NodeId, TypedEdge>();
        using (recorder.Attach(dijkstra)) dijkstra.Compute(from);
        return recorder.TryGetPath(to, out var path)
            ? new TopologyResult.Route(toSeq(path.Select(static e => e.Target)).Insert(0, from), path.Count())
            : new TopologyResult.Route(Seq<NodeId>(), double.PositiveInfinity);
    }

    static Seq<ElementSet> StronglyConnected(BidirectionalGraph<NodeId, TypedEdge> view) {
        var scc = new StronglyConnectedComponentsAlgorithm<NodeId, TypedEdge>(view);
        scc.Compute();
        return toSeq(scc.Components.GroupBy(static kv => kv.Value).Select(g => ElementSet.Of(toSeq(g.Select(static kv => kv.Key)))));
    }

    static TopologyResult Topological(BidirectionalGraph<NodeId, TypedEdge> view) =>
        Try.lift(() => { var sort = new TopologicalSortAlgorithm<NodeId, TypedEdge>(view); sort.Compute(); return toSeq(sort.SortedVertices); }).Run()
            .Match(Succ: ordered => (TopologyResult)new TopologyResult.Ordered(ordered), Fail: _ => new TopologyResult.Cyclic(StronglyConnected(view).Filter(static c => c.Count > 1)));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | algorithm owner     | QuikGraph compositions                 | never a hand-rolled walk or recursive CTE                 |
|  [02]   | result shape        | `ElementSet`-compatible key set        | a topology result composes with the selection algebra     |
|  [03]   | cycle detection     | `StronglyConnectedComponentsAlgorithm` | synchronous at the authoritative lane                     |
|  [04]   | shortest path       | Dijkstra / A* with spatial heuristic   | the in-process counterpart of the `pgrouting` lane        |
