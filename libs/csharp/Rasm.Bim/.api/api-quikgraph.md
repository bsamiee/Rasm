# [RASM_BIM_API_QUIKGRAPH]

`QuikGraph` is the pure-managed generic graph-algorithm library: parameterized graph containers
(`AdjacencyGraph`/`BidirectionalGraph`/`UndirectedGraph` over a `TVertex`/`TEdge:IEdge<TVertex>`
pair), value and reference edge shapes (`SEdge`/`SEquatableEdge`/`Edge`/`TaggedEdge`), and the
`AlgorithmExtensions` static facade exposing topological sort, BFS/DFS traversal with path
recovery, offline least-common-ancestor, strongly/weakly-connected components and condensation,
transitive closure/reduction, shortest paths (Dijkstra/A*/Bellman-Ford/DAG), minimum spanning
tree, and maximum flow. It is the shared directed-graph algorithm owner of `Rasm.Bim`,
collapsing the three hand-rolled graph walks the domain otherwise re-implements: the
`Planning/schedule#CRITICAL_PATH` CPM topological order, the `Model/systems#SYSTEM_TRACE` MEP
reachability closure, and the `Review/versioning#VERSION_GRAPH` commit-DAG common-ancestor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `QuikGraph`
- package: `QuikGraph`
- version: `2.5.0`
- license: MS-PL (Microsoft Public License — OSI permissive)
- assembly: `QuikGraph`
- namespace: `QuikGraph` (the graph containers, edge shapes, `IEdge<TVertex>`/`IBidirectionalGraph`/`IVertexAndEdgeListGraph`/`IUndirectedGraph` contracts, the `VertexAction`/`EdgeAction`/`TryFunc` delegates)
- namespace: `QuikGraph.Algorithms` (`AlgorithmExtensions` — the static consumer facade)
- namespace: `QuikGraph.Algorithms.Search` / `.ShortestPath` / `.TopologicalSort` / `.ConnectedComponents` / `.MaximumFlow` / `.MinimumSpanningTree` / `.Observers` (the algorithm objects the facade wraps; bind the facade)
- asset: netstandard2.0 single managed AnyCPU assembly (multi-targets net35/net40/net45/netstandard1.3 — the net10.0 consumer binds `lib/netstandard2.0`); ships the `QuikGraph.xml` doc set
- asset: IL-only, no P/Invoke; the `.NETStandard2.0` dependency group is EMPTY (zero transitive packages — the embedded `JetBrains.Annotations` attributes are internal)
- rail: graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: edge shapes
- package: `QuikGraph`
- namespace: `QuikGraph`
- rail: graph

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `IEdge<out TVertex>`        | graph    | the edge contract; `TVertex Source`, `TVertex Target`. Every algorithm is generic over `TEdge : IEdge<TVertex>` |
|  [02]   | `SEdge<TVertex>`            | graph    | the default struct directed edge; `Source`/`Target`, ctor `SEdge(source, target)` — the value-type edge with no allocation, the form for a dense `GlobalId`-keyed network |
|  [03]   | `SEquatableEdge<TVertex>`   | graph    | the struct directed edge with value equality (`IEquatable<SEquatableEdge<TVertex>>`); the edge `OfflineLeastCommonAncestor` keys its query pairs on |
|  [04]   | `Edge<TVertex>`             | graph    | the reference directed edge; ctor `Edge(source, target)` — the class form when an edge identity must be referenced |
|  [05]   | `TaggedEdge<TVertex, TTag>` / `STaggedEdge<TVertex, TTag>` | graph | edge carrying a `TTag` payload (`ITagged<TTag>`) — the `SequenceRel` lag / `PortConnection` joint carried ON the edge rather than a side map |

[PUBLIC_TYPE_SCOPE]: graph containers
- package: `QuikGraph`
- namespace: `QuikGraph`
- rail: graph

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `AdjacencyGraph<TVertex, TEdge>` | graph | the mutable directed out-edge graph; ctors `()`/`(bool allowParallelEdges)`/`(allowParallelEdges, vertexCapacity, edgeCapacity)`, `AddVertex`/`int AddVertexRange(IEnumerable<TVertex>)`/`AddVerticesAndEdge(TEdge)`/`AddEdge`, `bool ContainsEdge(source, target)`, `bool TryGetEdge(source, target, out TEdge)`/`TryGetEdges(...)`, `int OutDegree(v)`/`OutEdges(v)`, `IEnumerable<TVertex> Vertices`/`IEnumerable<TEdge> Edges`. The forward-only DAG the topological sort and reachability read |
|  [02]   | `BidirectionalGraph<TVertex, TEdge>` | graph | the directed graph with in AND out edges; adds `InDegree(v)`/`InEdge(v,i)`/`Degree(v)` — the form when the algorithm needs predecessor access (the commit-DAG parent walk, the `Roots`/`Sinks` query, condensation) |
|  [03]   | `UndirectedGraph<TVertex, TEdge>`    | graph | the undirected adjacency graph; `AdjacentDegree(v)`/`AdjacentEdge(v,i)`, `TryGetEdge(source, target, out edge)` — the symmetric form the MEP port-adjacency closure and the MST/connected-components read |

[PUBLIC_TYPE_SCOPE]: delegates and traversal recorders
- package: `QuikGraph`
- namespace: `QuikGraph`, `QuikGraph.Algorithms.Observers`
- rail: graph

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `TryFunc<in T, TResult>`    | graph    | `delegate bool TryFunc<in T, TResult>(T arg, out TResult result)` — the try-accessor the path-query algorithms return (`paths(target, out edges)` yields the edge path to a vertex or `false`) |
|  [02]   | `VertexAction<in TVertex>` / `EdgeAction<TVertex, in TEdge>` | graph | the visit callbacks the `*SearchAlgorithm` events fire (`DiscoverVertex`/`TreeEdge`/`ExamineEdge`/`FinishVertex`) — the hook for an in-traversal fold |
|  [03]   | `VertexPredecessorRecorderObserver<TVertex, TEdge>` | graph | the predecessor-map recorder; `IDictionary<TVertex, TEdge> VerticesPredecessors`, `IDisposable Attach(ITreeBuilderAlgorithm<...>)`, `bool TryGetPath(TVertex, out IEnumerable<TEdge>)` — attached to a BFS/DFS it records the spanning tree and recovers the edge path to any vertex |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: topological order (`AlgorithmExtensions`)
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `TopologicalSort`               | `(this IVertexListGraph<TVertex, TEdge> graph)` → `IEnumerable<TVertex>`                  | DFS-based topological order — throws `NonAcyclicGraphException` on a cycle |
|  [02]   | `SourceFirstTopologicalSort`    | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph)` → `IEnumerable<TVertex>`           | Kahn's algorithm (source-first by in-degree) — the order the CPM forward pass folds in (a source task before any successor); the cycle the schedule rejects surfaces here |
|  [03]   | `SourceFirstBidirectionalTopologicalSort` | `(this IBidirectionalGraph<TVertex, TEdge> graph[, TopologicalSortDirection direction])` → `IEnumerable<TVertex>` | the Kahn order over a bidirectional graph, `direction` selecting source-first vs sink-first (the backward-pass reverse order) |
|  [04]   | `IsDirectedAcyclicGraph`        | `(this IVertexListGraph<TVertex, TEdge> graph)` → `bool` / `(this IEnumerable<TEdge> edges)` → `bool` | the DAG predicate — the cheap acyclicity gate before a topological fold |

[ENTRYPOINT_SCOPE]: traversal and reachability with path recovery
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `TreeBreadthFirstSearch`        | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | runs a BFS from `root`, returns the path accessor: `paths(v, out edges)` yields the edge path root→`v` or `false` if `v` is unreachable — the downstream-reachability closure in one call |
|  [02]   | `TreeDepthFirstSearch`          | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | the DFS mirror — the depth-first spanning-tree path accessor |
|  [03]   | `BreadthFirstSearchAlgorithm<TVertex, TEdge>` | `new BreadthFirstSearchAlgorithm<…>(graph)` + the `DiscoverVertex`/`TreeEdge`/`FinishVertex` events, `Compute(root)` | the algorithm object for an in-traversal fold (accumulate the reached set as `DiscoverVertex` fires) when the `TryFunc` accessor is not enough; attach a `VertexPredecessorRecorderObserver` for the predecessor map |
|  [04]   | `Roots` / `Sinks`               | `(this IBidirectionalGraph<TVertex, TEdge> graph)` / `(this IVertexListGraph<…> graph)` → `IEnumerable<TVertex>` | the no-in-edge sources / no-out-edge sinks — the schedule start/finish anchors and the trace seed candidates |

[ENTRYPOINT_SCOPE]: least common ancestor, components, transitive structure
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `OfflineLeastCommonAncestor`    | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root, IEnumerable<SEquatableEdge<TVertex>> pairs)` → `TryFunc<SEquatableEdge<TVertex>, TVertex>` | Tarjan offline LCA — answers `lca((a,b), out ancestor)` for every queried `(a,b)` pair in one near-linear pass; the commit-DAG merge-base over a batch of branch-head pairs |
|  [02]   | `StronglyConnectedComponents` / `WeaklyConnectedComponents` | `(this IVertexListGraph<TVertex, TEdge> graph, IDictionary<TVertex, int> components)` → `int` | Tarjan SCC / weak components — labels each vertex with its component index, returns the count (the cycle clusters in a dependency or system graph) |
|  [03]   | `CondensateStronglyConnected`   | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph)` → `IMutableBidirectionalGraph<TGraph, CondensedEdge<…>>` | collapses each SCC into a super-vertex — the acyclic condensation a cyclic network is scheduled over |
|  [04]   | `ComputeTransitiveClosure` / `ComputeTransitiveReduction` | `(this IEdgeListGraph<TVertex, TEdge> graph[, Func<TVertex, TVertex, TEdge> edgeFactory])` → `BidirectionalGraph<TVertex, TEdge>` | the reachability closure / minimal equivalent edge set — the "is X downstream of Y" precompute and the redundant-dependency prune |

[ENTRYPOINT_SCOPE]: weighted paths, spanning tree, flow
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `ShortestPathsDijkstra`         | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | non-negative single-source shortest path — the path accessor from `root` |
|  [02]   | `ShortestPathsAStar`            | `(this IVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeWeights, Func<TVertex, double> costHeuristic, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | A* with an admissible heuristic |
|  [03]   | `ShortestPathsBellmanFord` / `ShortestPathsDag` | `(this IVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeWeights, TVertex root[, out bool hasNegativeCycle])` → `TryFunc<…>` | negative-weight-tolerant / DAG-optimized single-source shortest path |
|  [04]   | `MinimumSpanningTreeKruskal` / `MinimumSpanningTreePrim` | `(this IUndirectedGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights)` → `IEnumerable<TEdge>` | the MST edge set |
|  [05]   | `MaximumFlow`                   | `(this IMutableVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeCapacities, TVertex source, TVertex sink, out TryFunc<TVertex, TEdge> flowPredecessors, EdgeFactory<…> edgeFactory, ReversedEdgeAugmentorAlgorithm<…> augmentor)` → `double` | Edmonds-Karp max-flow / min-cut — the network capacity solve |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- a single managed `QuikGraph.dll` (netstandard2.0 binds forward under net10.0); no P/Invoke, zero transitive packages. Every algorithm is generic over `TVertex` and `TEdge : IEdge<TVertex>` — there is no built-in vertex identity, so a `GlobalId` `string` or a `UInt128` content-key is the vertex directly.
- `AlgorithmExtensions` is THE consumer entrypoint: the algorithm-object classes (`DepthFirstSearchAlgorithm`, `TarjanOfflineLeastCommonAncestorAlgorithm`, `SourceFirstTopologicalSortAlgorithm`, …) exist for the event-driven in-traversal fold, but the standard composition wraps them through the extension facade and reads the returned `IEnumerable`/`TryFunc`. A design page calling the algorithm class directly is justified only when it needs the per-vertex `DiscoverVertex`/`TreeEdge` event fold.
- edge-shape law: prefer the struct edges (`SEdge<TVertex>` general, `SEquatableEdge<TVertex>` when the algorithm keys on edge equality — `OfflineLeastCommonAncestor` requires it) for a dense `GlobalId`-keyed network — they allocate nothing per edge. The reference `Edge<TVertex>`/`TaggedEdge<TVertex,TTag>` are for a referenced edge identity or an edge-carried payload.

[GRAPH_CONSTRUCTION]:
- build a graph by `new AdjacencyGraph<string, SEdge<string>>()` then `g.AddVerticesAndEdge(new SEdge<string>(from, to))` per edge (the `AddVerticesAndEdge` form adds both endpoints if absent — no separate `AddVertex` pass). `BidirectionalGraph` when predecessor access is needed (parent walk, `Roots`), `UndirectedGraph` for symmetric adjacency (the MEP port graph, MST).
- the graph is the ALGORITHM input only — the domain owns the typed record (`DistributionSystem`, `BimRepository`, `ScheduleNetwork`), folds it into a transient QuikGraph once, runs the algorithm, and projects the result back onto the typed receipt. The graph never becomes a domain field.

[STACK_INTEGRATION]:
- `Planning/schedule#CRITICAL_PATH` (CPM): the `SequenceRel` predecessor→successor DAG folds into an `AdjacencyGraph<string, SEdge<string>>` (vertices = task `GlobalId`); `graph.SourceFirstTopologicalSort()` IS the Kahn order the forward/backward CPM pass folds in — replacing the hand-rolled `Topological.Order`. A residual cycle throws, lowered to `BimFault.ModelRejected`; `graph.IsDirectedAcyclicGraph()` is the cheap pre-gate. The `EarlyStart`/`LateFinish` float algebra stays the domain's `WorkCalendar` fold over that order — QuikGraph owns the ORDER, the domain owns the calendar arithmetic. The `SequenceRel` edges themselves originate either from native Bim authoring OR from a `Rasm.Persistence` `MPXJ.Net`-parsed `ProjectFile` (`api-mpxj`): MPXJ reads a P6 XER / MS-Project MPP / Asta schedule into a `Task.Predecessors`/`Successors` → `Relation` (`RelationType` + `Lag`) network and is parse-only — it supplies the activity-on-node edges but NEVER the forward/backward pass, so the topological order over those edges is THIS owner's and the float/calendar arithmetic is the `WorkCalendar` fold's. The `RelationType` (`FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`) and `Lag` map onto the `SequenceRel` payload (a `TaggedEdge<string, SequenceRel>` when the lag rides the edge), and the codec's `Duration.Units` is read through the canonical `NodaTime`/`UnitsNet` time vocabulary before the order folds — never trusted as raw days.
- `Model/systems#SYSTEM_TRACE` (MEP reachability): the `PortConnection` edges fold into an `AdjacencyGraph`/`UndirectedGraph` over port `GlobalId`s; `var paths = graph.TreeBreadthFirstSearch(seedPort)` returns the `TryFunc<string, IEnumerable<SEdge<string>>>` whose reachable domain IS the downstream closure — replacing the hand-rolled `SystemNetwork.Walk` visited-set fold. The `SystemTrace` receipt projects the reached vertex set (every port `paths(p, out _)` answers true for) back onto the owner-element `GlobalId` set. The undirected form gives the both-directions trace; an `AdjacencyGraph` with `FlowDirection`-oriented edges gives the directed downstream-only trace.
- `Review/versioning#VERSION_GRAPH` (commit-DAG merge-base): the commit `ParentKeys` lineage folds into a `BidirectionalGraph<UInt128, SEdge<UInt128>>` (a child→parent edge per parent); `var lca = graph.OfflineLeastCommonAncestor(rootCommit, new[] { new SEquatableEdge<UInt128>(ours, theirs) })` answers `lca(pair, out mergeBase)` — replacing the hand-rolled `CommonAncestor` BFS-intersection. The Tarjan offline form answers a BATCH of `(ours, theirs)` pairs in one near-linear pass, so an octopus merge or a multi-branch reconciliation resolves every merge-base together. `History` (the ancestor walk) is `graph.TreeBreadthFirstSearch(head)` over the parent edges.
- the typed receipts (`CriticalPath`, `SystemTrace`, `MergeOutcome`) are unchanged — QuikGraph collapses the IMPERATIVE GRAPH WALK inside each `Auto` fold, never the domain algebra around it. The three pages share ONE graph-algorithm owner instead of three bespoke walks, the `[GRAPH_ALGORITHM]` collapse the manifest admission states.

[CONTENT_KEY_MEMOIZATION]:
- each graph fold keys on the domain content-key the page already derives (the `DistributionSystem.Identity` `(GeometryKey, TopologyKey)`, the `CommitKey`, the schedule `(GeometryKey, ScheduleKey)`) through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom — so a trace/sort/LCA re-runs only on a changed graph, never per query. QuikGraph holds no identity scheme of its own; the vertex IS the domain key.

[LOCAL_ADMISSION]:
- a graph algorithm enters through the `AlgorithmExtensions` facade over a transient `AdjacencyGraph`/`BidirectionalGraph`/`UndirectedGraph` folded from the typed domain record; the result projects back onto the typed receipt.
- a hand-rolled topological sort, BFS/DFS reachability fold, or BFS-intersection LCA over a `Map<>`/`Seq<>` adjacency is the rejected form — those are exactly the walks this owner collapses.
- the in-traversal event fold (`DiscoverVertex`/`TreeEdge`) is used only when the `IEnumerable`/`TryFunc` facade result is insufficient; otherwise bind the facade.

[RAIL_LAW]:
- Package: `QuikGraph`
- Owns: the generic graph containers and edge shapes, and the graph-algorithm facade — topological sort (DFS + Kahn), BFS/DFS traversal with path recovery, Tarjan offline LCA, connected components + condensation, transitive closure/reduction, weighted shortest paths, MST, max-flow
- Accept: ordering a DAG, computing reachability/closure, resolving a least-common-ancestor over a DAG, component/cycle analysis, weighted-path and flow solves over a domain-folded graph
- Reject: the domain typed records and their content-key identity (the Bim pages own `DistributionSystem`/`BimRepository`/`ScheduleNetwork` and the `XxHash128` key), the calendar/float arithmetic around the CPM order (the `WorkCalendar` fold owns it), the geometric spatial index (the `NetTopologySuite` `STRtree` and the `SwiftCollections.Lean` BVH own broad-phase), persistence of any graph (it is a transient algorithm input)
