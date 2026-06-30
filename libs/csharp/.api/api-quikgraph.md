# [RASM_API_QUIKGRAPH]

`QuikGraph` is the shared pure-managed generic graph-algorithm substrate of the C# stack: parameterized
graph containers (`AdjacencyGraph`/`BidirectionalGraph`/`UndirectedGraph` over a `TVertex`/`TEdge:IEdge<TVertex>`
pair), value and reference edge shapes (`SEdge`/`SEquatableEdge`/`Edge`/`TaggedEdge`), and the
`AlgorithmExtensions` static facade exposing topological sort, BFS/DFS traversal with path
recovery, offline least-common-ancestor, strongly/weakly-connected components and condensation,
transitive closure/reduction, shortest paths (Dijkstra/A*/Bellman-Ford/DAG), minimum spanning
tree, and maximum flow. It is cross-cutting substrate shared across the strata, one graph-algorithm
owner the whole stack folds a transient graph into rather than re-implementing a walk:
- `Rasm.Element` builds the canonical `ElementGraph` topology view — the lazy
  `BidirectionalGraph<NodeId, SEdge<NodeId>>` plus the `FrozenDictionary<NodeId, ImmutableArray<Relationship>>`
  incidence index materialized ONCE at the read-snapshot freeze, the structure `Bake` reads O(degree) and
  `Topology()` exposes for reachability/topological-order/LCA.
- `Rasm.Persistence` composes the DEFAULT synchronous `Query/topology` lane over it — the authoritative
  in-process topology owner (containment ancestry, connection traversal, void-resolution, cycle detection),
  with Apache AGE / DuckDB demoted to async analytical lanes.
- `Rasm.Bim` collapses its three hand-rolled domain walks onto it — the `Planning/schedule#CRITICAL_PATH`
  CPM topological order, the `Model/systems#SYSTEM_TRACE` MEP reachability closure, and the
  `Review/versioning#VERSION_GRAPH` rooted ancestry projection for batched LCA. Multi-parent commit-DAG
  merge-base logic needs an explicit domain projection or a separate DAG merge-base algorithm.

The vertex IS the domain key (`NodeId`/`GlobalId` string or `UInt128` content key); QuikGraph holds no
identity scheme of its own, and every result projects back onto the consumer's typed receipt.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `QuikGraph`
- package: `QuikGraph`
- version: `2.5.0`
- license: MS-PL (Microsoft Public License — OSI permissive)
- assembly: `QuikGraph`
- namespace: `QuikGraph` (the graph containers, edge shapes, `IEdge<TVertex>`/`IBidirectionalGraph`/`IVertexAndEdgeListGraph`/`IUndirectedGraph` contracts, the `VertexAction`/`EdgeAction`/`TryFunc` delegates)
- namespace: `QuikGraph.Algorithms` (`AlgorithmExtensions` — the static consumer facade)
- namespace: `QuikGraph.Algorithms.Search` / `.ShortestPath` / `.TopologicalSort` / `.ConnectedComponents` / `.MaximumFlow` / `.MinimumSpanningTree` / `.Observers` (the algorithm objects the facade wraps; bind the facade unless an in-traversal event fold is required)
- asset: netstandard2.0 single managed AnyCPU assembly (multi-targets net35/net40/net45/netstandard1.3 — the net10.0 consumer binds `lib/netstandard2.0`); ships the `QuikGraph.xml` doc set
- asset: IL-only, no P/Invoke; the `.NETStandard2.0` dependency group is EMPTY (zero transitive packages — the embedded `JetBrains.Annotations` attributes are internal); ALC-safe inside an in-Rhino plugin assembly
- rail: graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: edge shapes
- package: `QuikGraph`
- namespace: `QuikGraph`
- rail: graph

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `IEdge<out TVertex>`        | graph    | the edge contract; `TVertex Source`, `TVertex Target`. Every algorithm is generic over `TEdge : IEdge<TVertex>`, so a consumer-defined edge (the Persistence `TypedEdge` carrying a `Relationship` kind) is a first-class edge |
|  [02]   | `SEdge<TVertex>`            | graph    | the default struct directed edge; `Source`/`Target`, ctor `SEdge(source, target)` — the value-type edge with no allocation, the form for a dense `NodeId`/`GlobalId`-keyed network (the seam topology view's `SEdge<NodeId>`, the Bim schedule DAG's `SEdge<string>`) |
|  [03]   | `SEquatableEdge<TVertex>`   | graph    | the struct directed edge with value equality (`IEquatable<SEquatableEdge<TVertex>>`); the edge `OfflineLeastCommonAncestor` keys its query pairs on |
|  [04]   | `Edge<TVertex>`             | graph    | the reference directed edge; ctor `Edge(source, target)` — the class form when an edge identity must be referenced |
|  [05]   | `TaggedEdge<TVertex, TTag>` / `STaggedEdge<TVertex, TTag>` | graph | edge carrying a `TTag` payload (`ITagged<TTag>`) — a kind tag carried ON the edge (the Bim `SequenceRel` lag / `PortConnection` joint) rather than a side map; a consumer needing a richer edge (the Persistence `TypedEdge`) implements `IEdge<TVertex>` directly |

[PUBLIC_TYPE_SCOPE]: graph containers
- package: `QuikGraph`
- namespace: `QuikGraph`
- rail: graph

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `AdjacencyGraph<TVertex, TEdge>` | graph | the mutable directed out-edge graph; ctors `()`/`(bool allowParallelEdges)`/`(allowParallelEdges, vertexCapacity, edgeCapacity)`, `AddVertex`/`int AddVertexRange(IEnumerable<TVertex>)`/`AddVerticesAndEdge(TEdge)`/`AddEdge`, `bool ContainsEdge(source, target)`, `bool TryGetEdge(source, target, out TEdge)`/`TryGetEdges(...)`, `int OutDegree(v)`/`OutEdges(v)`, `IEnumerable<TVertex> Vertices`/`IEnumerable<TEdge> Edges`. The forward-only DAG the topological sort and reachability read (the Bim schedule/trace walks) |
|  [02]   | `BidirectionalGraph<TVertex, TEdge>` | graph | the directed graph with in AND out edges; adds `InDegree(v)`/`InEdge(v,i)`/`Degree(v)` — the form when the algorithm needs predecessor access: the seam `ElementGraph` topology view (`BidirectionalGraph<NodeId, SEdge<NodeId>>`) and the Persistence `Query/topology` view (`BidirectionalGraph<NodeId, TypedEdge>`) both build it, plus the commit-DAG parent walk, the `Roots`/`Sinks` query, condensation |
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
|  [02]   | `SourceFirstTopologicalSort`    | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph)` → `IEnumerable<TVertex>`           | Kahn's algorithm (source-first by in-degree) — the order the CPM forward pass folds in (a source task before any successor) and the seam/Persistence dependency ordering; the cycle a schedule rejects surfaces here |
|  [03]   | `SourceFirstBidirectionalTopologicalSort` | `(this IBidirectionalGraph<TVertex, TEdge> graph[, TopologicalSortDirection direction])` → `IEnumerable<TVertex>` | the Kahn order over a bidirectional graph, `direction` selecting source-first vs sink-first (the backward-pass reverse order) |
|  [04]   | `IsDirectedAcyclicGraph`        | `(this IVertexListGraph<TVertex, TEdge> graph)` → `bool` / `(this IEnumerable<TEdge> edges)` → `bool` | the DAG predicate — the cheap acyclicity gate before a topological fold |

[ENTRYPOINT_SCOPE]: traversal and reachability with path recovery
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `TreeBreadthFirstSearch`        | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | runs a BFS from `root`, returns the path accessor: `paths(v, out edges)` yields the edge path root→`v` or `false` if `v` is unreachable — the downstream-reachability closure (MEP trace, containment descent) in one call |
|  [02]   | `TreeDepthFirstSearch`          | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | the DFS mirror — the depth-first spanning-tree path accessor |
|  [03]   | `BreadthFirstSearchAlgorithm<TVertex, TEdge>` / `DepthFirstSearchAlgorithm<TVertex, TEdge>` | `new BreadthFirstSearchAlgorithm<…>(graph)` + the `DiscoverVertex`/`TreeEdge`/`FinishVertex` events, `Compute(root)` | the algorithm object for an in-traversal fold (accumulate the reached set as `DiscoverVertex` fires) when the `TryFunc` accessor is not enough — the Persistence `Query/topology` containment/connection traversal binds these over the edge-kind-filtered view; attach a `VertexPredecessorRecorderObserver` for the predecessor map |
|  [04]   | `Roots` / `Sinks`               | `(this IBidirectionalGraph<TVertex, TEdge> graph)` / `(this IVertexListGraph<…> graph)` → `IEnumerable<TVertex>` | the no-in-edge sources / no-out-edge sinks — the schedule start/finish anchors and the trace seed candidates |

[ENTRYPOINT_SCOPE]: least common ancestor, components, transitive structure
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `OfflineLeastCommonAncestor`    | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root, IEnumerable<SEquatableEdge<TVertex>> pairs)` → `TryFunc<SEquatableEdge<TVertex>, TVertex>` | Tarjan offline LCA over a rooted tree — answers `lca((a,b), out ancestor)` for every queried pair in one near-linear pass; a commit-DAG merge-base is valid only after the domain projects lineage to a rooted tree |
|  [02]   | `StronglyConnectedComponents` / `WeaklyConnectedComponents` / `StronglyConnectedComponentsAlgorithm<TVertex, TEdge>` | `(this IVertexListGraph<TVertex, TEdge> graph, IDictionary<TVertex, int> components)` → `int` | Tarjan SCC / weak components — labels each vertex with its component index, returns the count; the algorithm object reports any component of size > 1 as the cycle the Persistence cycle-detection / containment-cycle probe reads |
|  [03]   | `CondensateStronglyConnected`   | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph)` → `IMutableBidirectionalGraph<TGraph, CondensedEdge<…>>` | collapses each SCC into a super-vertex — the acyclic condensation a cyclic network is scheduled over |
|  [04]   | `ComputeTransitiveClosure` / `ComputeTransitiveReduction` | `(this IEdgeListGraph<TVertex, TEdge> graph[, Func<TVertex, TVertex, TEdge> edgeFactory])` → `BidirectionalGraph<TVertex, TEdge>` | the reachability closure / minimal equivalent edge set — the "is X downstream of Y" precompute and the redundant-dependency prune |

[ENTRYPOINT_SCOPE]: weighted paths, spanning tree, flow
- package: `QuikGraph`
- namespace: `QuikGraph.Algorithms`
- rail: graph

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `ShortestPathsDijkstra` / `DijkstraShortestPathAlgorithm<TVertex, TEdge>` | `(this IVertexAndEdgeListGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | non-negative single-source shortest path — the path accessor from `root` (the Persistence topology shortest-path query) |
|  [02]   | `ShortestPathsAStar` / `AStarShortestPathAlgorithm<TVertex, TEdge>` | `(this IVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeWeights, Func<TVertex, double> costHeuristic, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | A* with an admissible heuristic — the in-process counterpart of the `pgrouting` analytical route, the heuristic reading the node spatial envelope |
|  [03]   | `ShortestPathsBellmanFord` / `ShortestPathsDag` | `(this IVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeWeights, TVertex root[, out bool hasNegativeCycle])` → `TryFunc<…>` | negative-weight-tolerant / DAG-optimized single-source shortest path |
|  [04]   | `MinimumSpanningTreeKruskal` / `MinimumSpanningTreePrim` | `(this IUndirectedGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights)` → `IEnumerable<TEdge>` | the MST edge set |
|  [05]   | `MaximumFlow`                   | `(this IMutableVertexAndEdgeListGraph<…> graph, Func<TEdge, double> edgeCapacities, TVertex source, TVertex sink, out TryFunc<TVertex, TEdge> flowPredecessors, EdgeFactory<…> edgeFactory, ReversedEdgeAugmentorAlgorithm<…> augmentor)` → `double` | Edmonds-Karp max-flow / min-cut — the network capacity solve |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- a single managed `QuikGraph.dll` (netstandard2.0 binds forward under net10.0); no P/Invoke, zero transitive packages. Every algorithm is generic over `TVertex` and `TEdge : IEdge<TVertex>` — there is no built-in vertex identity, so a `NodeId`/`GlobalId` `string` or a `UInt128` content-key is the vertex directly, and a consumer-defined edge (the Persistence `TypedEdge` carrying a `Relationship`) is a first-class `IEdge<TVertex>`.
- `AlgorithmExtensions` is THE consumer entrypoint: the algorithm-object classes (`BreadthFirstSearchAlgorithm`, `DepthFirstSearchAlgorithm`, `DijkstraShortestPathAlgorithm`, `AStarShortestPathAlgorithm`, `StronglyConnectedComponentsAlgorithm`, `TarjanOfflineLeastCommonAncestorAlgorithm`, `SourceFirstTopologicalSortAlgorithm`, …) exist for the event-driven in-traversal fold, but the standard composition wraps them through the extension facade and reads the returned `IEnumerable`/`TryFunc`. A page binding the algorithm class directly is justified when it needs the per-vertex `DiscoverVertex`/`TreeEdge` event fold or an edge-kind-filtered subgraph (the Persistence `Query/topology` containment/connection traversal).
- edge-shape law: prefer the struct edges (`SEdge<TVertex>` general, `SEquatableEdge<TVertex>` when the algorithm keys on edge equality — `OfflineLeastCommonAncestor` requires it) for a dense `NodeId`/`GlobalId`-keyed network — they allocate nothing per edge. The reference `Edge<TVertex>`/`TaggedEdge<TVertex,TTag>` or a consumer `IEdge<TVertex>` (`TypedEdge`) are for a referenced edge identity or an edge-carried payload (a `Relationship` kind a traversal filters on).

[GRAPH_CONSTRUCTION]:
- build a graph by `new AdjacencyGraph<string, SEdge<string>>()` / `new BidirectionalGraph<NodeId, SEdge<NodeId>>(allowParallelEdges: true)` then `g.AddVerticesAndEdge(new SEdge<…>(from, to))` per edge (the `AddVerticesAndEdge` form adds both endpoints if absent — no separate `AddVertex` pass; `AddVertexRange` adds isolates so an unconnected node still orders). `BidirectionalGraph` when predecessor access is needed (the seam/Persistence topology view, the parent walk, `Roots`), `UndirectedGraph` for symmetric adjacency (the MEP port graph, MST).
- the graph is the ALGORITHM input — each consumer owns the typed record (the seam `ElementGraph`, Bim's `DistributionSystem`/`BimRepository`/`ScheduleNetwork`, the Persistence read snapshot), folds it into a QuikGraph container, runs the algorithm, and projects the result back onto the typed receipt. The built view is NOT a stored domain field: the seam `ElementGraph` and the Persistence `Query/topology` lane cache the `BidirectionalGraph` per read-snapshot (`[IgnoreEquality]`, rebuilt at the `Freeze`/snapshot boundary, excluded from content equality and persistence), and Bim folds a transient graph per query — never a mutable domain field.

[STACK_INTEGRATION]:
- `Rasm.Element` `Graph/element#ELEMENT_GRAPH` (the canonical topology view): the frozen `ElementGraph` read snapshot carries a lazy `BidirectionalGraph<NodeId, SEdge<NodeId>>` built once at `Of`/`Freeze` (`g.AddVerticesAndEdge(new SEdge<NodeId>(edge.Endpoints.Relating, edge.Endpoints.Related))` over the `Relationship` edges, `allowParallelEdges: true`) alongside the `FrozenDictionary<NodeId, ImmutableArray<Relationship>>` incidence index — `Bake` reads the incidence index O(degree) to fold an `Object`'s reachable subgraph, and `Topology()` exposes the `BidirectionalGraph` for a reachability/topological-order/LCA consumer composing `AlgorithmExtensions`. The seam splits the graph by PHASE (`H4`): the live authoring/delta path is the `Graph/delta#GRAPH_DELTA` `ImmutableDictionary` HAMT (O(log n) structural sharing), and the `QuikGraph` view + incidence index materialize ONLY at the read-snapshot freeze, both `[IgnoreEquality]` so two snapshots compare by their nodes and edges, never the lazily-built analytical caches. This is the foundational consumer — the seam owns the canonical graph; Persistence and Bim compose views over the same containers.
- `Rasm.Persistence` `Query/topology#GRAPH_TOPOLOGY` (the DEFAULT synchronous topology lane, `H5`): `GraphTopology.View(ElementGraph)` builds a `BidirectionalGraph<NodeId, TypedEdge>` (the `TypedEdge` a custom `IEdge<NodeId>` carrying the `Relationship` kind so a traversal filters to containment-only or connection-only edges) from the seam relationships plus the frozen incidence index, memoized keyed on the `Projection/address#CONTENT_ADDRESS` graph content hash so a re-read of an unchanged graph reuses the view and a `GraphDelta` invalidates it. `Traversals.Run` dispatches a `TopologyQuery` to its QuikGraph algorithm — containment ancestry/descent via `BreadthFirstSearchAlgorithm` over the containment-filtered subgraph, connection adjacency, shortest path via `DijkstraShortestPathAlgorithm`/`AStarShortestPathAlgorithm` (the A* heuristic reading the node spatial envelope), cycle/containment-cycle detection via `StronglyConnectedComponentsAlgorithm`, dependency ordering via `TopologicalSortAlgorithm` — every result projecting to an `ElementSet`-compatible key set. This is the AUTHORITATIVE correctness lane: Apache AGE (`Query/cypher`) and DuckDB are demoted to OPTIONAL async analytical projections carrying a `StalenessWatermark`, so an interactive-correctness read (clash, void-resolution, live QTO, containment) binds THIS in-process view and never a daemon-lagged projection.
- `Rasm.Bim` (the three domain walks, one owner instead of three bespoke walks):
  - `Planning/schedule#CRITICAL_PATH` (CPM): the `SequenceRel` predecessor→successor DAG folds into an `AdjacencyGraph<string, SEdge<string>>` (vertices = task `GlobalId`, `AddVertexRange` admitting every task so an unsequenced isolate still orders, the value `SEdge<string>` allocating nothing); `graph.SourceFirstTopologicalSort()` IS the Kahn order the forward/backward CPM pass folds in, `graph.IsDirectedAcyclicGraph()` the cheap pre-gate lowering a residual cycle onto `BimFault.ModelRejected` before the sort would throw `NonAcyclicGraphException`. The `EarlyStart`/`LateFinish` float algebra stays the domain's `WorkCalendar` fold over that order — QuikGraph owns the ORDER, the domain owns the calendar arithmetic. The `SequenceRel` edges originate either from native Bim authoring OR from a `Rasm.Persistence` `MPXJ.Net`-parsed `ProjectFile` (`api-mpxj`): MPXJ reads a P6 XER / MS-Project MPP / Asta schedule into a `Task.Predecessors`/`Successors` → `Relation` (`RelationType` + `Lag`) network and is parse-only — it supplies the activity-on-node edges but NEVER the forward/backward pass, so the topological order over those edges is THIS owner's. The `RelationType` (`FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`) and `Lag` map onto the `SequenceRel` payload (a `TaggedEdge<string, SequenceRel>` when the lag rides the edge), and the codec's `Duration.Units` is read through the canonical `NodaTime`/`UnitsNet` time vocabulary before the order folds — never trusted as raw days.
  - `Model/systems#SYSTEM_TRACE` (MEP reachability): the `PortConnection` edges fold into an `AdjacencyGraph`/`UndirectedGraph` over port `GlobalId`s; `var paths = graph.TreeBreadthFirstSearch(seedPort)` returns the `TryFunc<string, IEnumerable<SEdge<string>>>` whose reachable domain IS the downstream closure — replacing the hand-rolled `SystemNetwork.Walk` visited-set fold. The `SystemTrace` receipt projects the reached vertex set back onto the owner-element `GlobalId` set. The undirected form gives the both-directions trace; an `AdjacencyGraph` with `FlowDirection`-oriented edges gives the directed downstream-only trace.
  - `Review/versioning#VERSION_GRAPH` (rooted ancestry LCA): a version lineage that is projected to one rooted parent tree can fold into a `BidirectionalGraph<UInt128, SEdge<UInt128>>`; `var lca = graph.OfflineLeastCommonAncestor(rootCommit, new[] { new SEquatableEdge<UInt128>(ours, theirs) })` answers `lca(pair, out ancestor)` for that rooted projection. A multi-parent commit DAG is not automatically a valid Tarjan-LCA input; a merge-base over the full DAG needs either the domain's rooted projection policy or a separate DAG merge-base algorithm. `History` (the ancestor walk) can still use `graph.TreeBreadthFirstSearch(head)` over the selected parent edges.
- the typed receipts (the seam `Element`/`Topology()`, the Persistence `TopologyResult`/`ElementSet`, the Bim `CriticalPath`/`SystemTrace`/`MergeOutcome`) are the durable surface — QuikGraph collapses the IMPERATIVE GRAPH WALK inside each fold, never the domain algebra around it. The whole stack shares ONE graph-algorithm owner instead of N bespoke walks, the `[GRAPH_ALGORITHM]` substrate the registry admits.

[CONTENT_KEY_MEMOIZATION]:
- each consumer keys its built view / fold on the content address it already derives — the seam `ElementGraph`'s `Projection/address#CONTENT_ADDRESS` `UInt128`, the Persistence `Query/topology` graph content hash, the Bim `DistributionSystem.Identity` `(GeometryKey, TopologyKey)` / `CommitKey` / schedule `(GeometryKey, ScheduleKey)` through the `XxHash128.HashToUInt128` idiom — so a view build, trace, sort, or LCA re-runs only on a changed graph, never per query. QuikGraph holds no identity scheme of its own; the vertex IS the domain key.

[LOCAL_ADMISSION]:
- a graph algorithm enters through the `AlgorithmExtensions` facade (or an algorithm object for an edge-kind-filtered / event-fold traversal) over a transient `AdjacencyGraph`/`BidirectionalGraph`/`UndirectedGraph` folded from the typed domain record; the result projects back onto the typed receipt.
- a hand-rolled topological sort, BFS/DFS reachability fold, recursive ltree-style in-memory ancestry, or BFS-intersection LCA over a `Map<>`/`Seq<>` adjacency is the rejected form — those are exactly the walks this owner collapses; a second incidence structure beside the one the seam snapshot freezes is also rejected.
- the in-traversal event fold (`DiscoverVertex`/`TreeEdge`) is used only when the `IEnumerable`/`TryFunc` facade result is insufficient; otherwise bind the facade.

[RAIL_LAW]:
- Package: `QuikGraph`
- Owns: the generic graph containers and edge shapes, and the graph-algorithm facade — topological sort (DFS + Kahn), BFS/DFS traversal with path recovery, Tarjan offline LCA over rooted trees, connected components + condensation, transitive closure/reduction, weighted shortest paths, MST, max-flow
- Accept: ordering a DAG, computing reachability/closure, resolving a least-common-ancestor over a rooted tree projection, component/cycle analysis, weighted-path and flow solves over a domain-folded graph
- Reject: the domain typed records and their content-key identity (the seam owns `ElementGraph`/`NodeId`/`ContentAddress`; Bim owns `DistributionSystem`/`BimRepository`/`ScheduleNetwork` and the `XxHash128` key; Persistence owns the read snapshot and the `Query/lane` routing), the calendar/float arithmetic around the CPM order (the Bim `WorkCalendar` fold), the geometric spatial index (the `NetTopologySuite` `STRtree` and the `SwiftCollections.Lean` BVH own broad-phase), the durable graph store (Marten / the object store own persistence — the built view is a transient `[IgnoreEquality]` per-snapshot cache, never the system of record), and the async analytical graph projection (Apache AGE / DuckDB own the demoted analytical lanes — QuikGraph is the synchronous authoritative topology owner)
