# [RASM_API_QUIKGRAPH]

`QuikGraph` owns the C# stack's pure-managed generic graph containers, edge shapes, and algorithm facade. Consumers fold typed domain records into transient graphs and project each algorithm result back onto a typed receipt. The vertex is the domain key, and QuikGraph owns no identity scheme.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `QuikGraph`

- Package: `QuikGraph`
- License: `MS-PL`
- Assembly: `QuikGraph`
- Namespace: `QuikGraph`
- Namespace: `QuikGraph.Algorithms`
- Namespace: `QuikGraph.Algorithms.ConnectedComponents`
- Namespace: `QuikGraph.Algorithms.MaximumFlow`
- Namespace: `QuikGraph.Algorithms.Observers`
- Namespace: `QuikGraph.Algorithms.Search`
- Namespace: `QuikGraph.Algorithms.ShortestPath`
- Asset: managed IL assembly with XML documentation
- Rail: graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: edge shapes

Every graph algorithm accepts edges implementing `IEdge<TVertex>`.

| [INDEX] | [SYMBOL]                     | [SHAPE]          | [CAPABILITY]                   |
| :-----: | :--------------------------- | :--------------- | :----------------------------- |
|  [01]   | `IEdge<out TVertex>`         | interface        | generic source-target contract |
|  [02]   | `SEdge<TVertex>`             | struct           | value directed edge            |
|  [03]   | `SEquatableEdge<TVertex>`    | equatable struct | value-keyed directed edge      |
|  [04]   | `Edge<TVertex>`              | class            | reference directed edge        |
|  [05]   | `TaggedEdge<TVertex, TTag>`  | tagged class     | payload-carrying edge          |
|  [06]   | `STaggedEdge<TVertex, TTag>` | tagged struct    | value payload edge             |

[EDGE_CONTRACT]:

- Source: `TVertex Source`
- Target: `TVertex Target`
- Tag: `TTag Tag` on tagged edges

[PUBLIC_TYPE_SCOPE]: graph containers

Container construction, mutation, and lookup remain on each graph instance.

| [INDEX] | [SYMBOL]                             | [DIRECTION]   | [CAPABILITY]            |
| :-----: | :----------------------------------- | :------------ | :---------------------- |
|  [01]   | `AdjacencyGraph<TVertex, TEdge>`     | outgoing      | mutable adjacency       |
|  [02]   | `BidirectionalGraph<TVertex, TEdge>` | bidirectional | predecessor access      |
|  [03]   | `UndirectedGraph<TVertex, TEdge>`    | symmetric     | undirected neighborhood |

[ADJACENCY_GRAPH]:

- Construction: `allowParallelEdges`, vertex capacity, edge capacity
- Mutation: `AddVertex`, `AddVertexRange`, `AddEdge`, `AddVerticesAndEdge`
- Query: `ContainsEdge`, `TryGetEdge`, `TryGetEdges`, `OutDegree`, `OutEdges`

[BIDIRECTIONAL_GRAPH]:

- Construction: `allowParallelEdges`, vertex capacity, edge capacity
- Query: `InDegree`, `InEdge`, `InEdges`, `Degree`

[UNDIRECTED_GRAPH]:

- Query: `AdjacentDegree`, `AdjacentEdge`, `AdjacentEdges`, `TryGetEdge`

[PUBLIC_TYPE_SCOPE]: delegates and traversal recorders

Traversal algorithms expose event delegates and attachable path observers.

| [INDEX] | [SYMBOL]                                            | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `TryFunc<in T, TResult>`                            | delegate | conditional result access |
|  [02]   | `VertexAction<in TVertex>`                          | delegate | vertex event fold         |
|  [03]   | `EdgeAction<TVertex, in TEdge>`                     | delegate | edge event fold           |
|  [04]   | `VertexPredecessorRecorderObserver<TVertex, TEdge>` | observer | predecessor path recovery |

[TRY_FUNC]:

- Return: `bool`
- Match: `out TResult`

[VERTEX_PREDECESSOR_RECORDER]:

- State: `VerticesPredecessors`
- Attach: `Attach(ITreeBuilderAlgorithm<TVertex, TEdge>)`
- Match: `TryGetPath(TVertex, out IEnumerable<TEdge>)`

[TRAVERSAL_EVENTS]:

- Vertex: `DiscoverVertex`, `FinishVertex`
- Edge: `ExamineEdge`, `TreeEdge`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: topological order (`AlgorithmExtensions`)

Topological extensions return vertex orderings and reject cyclic input with `NonAcyclicGraphException`.

| [INDEX] | [SURFACE]                                 | [INPUT]                    | [CAPABILITY]         |
| :-----: | :---------------------------------------- | :------------------------- | :------------------- |
|  [01]   | `TopologicalSort`                         | directed vertex-list graph | DFS order            |
|  [02]   | `TopologicalSort`                         | undirected graph           | DFS order            |
|  [03]   | `SourceFirstTopologicalSort`              | directed edge-list graph   | Kahn source order    |
|  [04]   | `SourceFirstTopologicalSort`              | undirected graph           | Kahn source order    |
|  [05]   | `SourceFirstBidirectionalTopologicalSort` | bidirectional graph        | forward Kahn order   |
|  [06]   | `SourceFirstBidirectionalTopologicalSort` | graph plus direction       | directional order    |
|  [07]   | `IsDirectedAcyclicGraph(graph)`           | vertex-list graph          | acyclicity predicate |
|  [08]   | `IsDirectedAcyclicGraph(edges)`           | edge sequence              | acyclicity predicate |

[BIDIRECTIONAL_TOPOLOGICAL_ORDER]:

- Result: `IEnumerable<TVertex>`
- Direction: `TopologicalSortDirection`
- Default: `Forward`

[ENTRYPOINT_SCOPE]: traversal and reachability with path recovery

Tree-search extensions return `TryFunc<TVertex, IEnumerable<TEdge>>` path accessors; algorithm objects expose traversal events.

| [INDEX] | [SURFACE]                                     | [INPUT]             | [CAPABILITY]            |
| :-----: | :-------------------------------------------- | :------------------ | :---------------------- |
|  [01]   | `TreeBreadthFirstSearch`                      | graph plus root     | BFS path recovery       |
|  [02]   | `TreeDepthFirstSearch`                        | graph plus root     | DFS path recovery       |
|  [03]   | `BreadthFirstSearchAlgorithm<TVertex, TEdge>` | vertex-list graph   | event-driven BFS        |
|  [04]   | `DepthFirstSearchAlgorithm<TVertex, TEdge>`   | vertex-list graph   | event-driven DFS        |
|  [05]   | `Roots`                                       | vertex-list graph   | DFS forest roots        |
|  [06]   | `Roots`                                       | bidirectional graph | zero-indegree vertices  |
|  [07]   | `Sinks`                                       | vertex-list graph   | zero-outdegree vertices |

[ENTRYPOINT_SCOPE]: least common ancestor, components, transitive structure

Component extensions label supplied component maps; closure extensions return bidirectional graphs.

| [INDEX] | [SURFACE]                                              | [RESULT]            | [CAPABILITY]             |
| :-----: | :----------------------------------------------------- | :------------------ | :----------------------- |
|  [01]   | `OfflineLeastCommonAncestor`                           | `TryFunc` ancestor  | rooted-tree LCA          |
|  [02]   | `ConnectedComponents`                                  | component count     | undirected labels        |
|  [03]   | `StronglyConnectedComponents`                          | component count     | Tarjan SCC labels        |
|  [04]   | `WeaklyConnectedComponents`                            | component count     | weak component labels    |
|  [05]   | `StronglyConnectedComponentsAlgorithm<TVertex, TEdge>` | component map       | inspectable Tarjan state |
|  [06]   | `CondensateStronglyConnected`                          | condensation graph  | SCC contraction          |
|  [07]   | `ComputeTransitiveClosure`                             | bidirectional graph | reachability closure     |
|  [08]   | `ComputeTransitiveReduction`                           | bidirectional graph | redundant-edge removal   |

[OFFLINE_LEAST_COMMON_ANCESTOR]:

- Input: graph, root, `IEnumerable<SEquatableEdge<TVertex>>`
- Result: `TryFunc<SEquatableEdge<TVertex>, TVertex>`

[COMPONENT_LABELING]:

- Mutation: supplied `IDictionary<TVertex, int>`
- Result: component count

[TRANSITIVE_CLOSURE]:

- Input: edge-list graph plus edge factory
- Result: `BidirectionalGraph<TVertex, TEdge>`

[STRONG_CONDENSATION]:

- Result: `IMutableBidirectionalGraph<TGraph, CondensedEdge<TVertex, TEdge, TGraph>>`

[ENTRYPOINT_SCOPE]: weighted paths, spanning tree, flow

Shortest-path extensions return `TryFunc<TVertex, IEnumerable<TEdge>>`; spanning-tree extensions return edge sequences.

| [INDEX] | [SURFACE]                                           | [PRECONDITION]           | [CAPABILITY]               |
| :-----: | :-------------------------------------------------- | :----------------------- | :------------------------- |
|  [01]   | `ShortestPathsDijkstra`                             | non-negative weights     | Dijkstra path recovery     |
|  [02]   | `DijkstraShortestPathAlgorithm<TVertex, TEdge>`     | non-negative weights     | event-driven Dijkstra      |
|  [03]   | `ShortestPathsAStar`                                | admissible heuristic     | A\* path recovery          |
|  [04]   | `AStarShortestPathAlgorithm<TVertex, TEdge>`        | admissible heuristic     | event-driven A\*           |
|  [05]   | `ShortestPathsBellmanFord`                          | signed weights           | path plus cycle verdict    |
|  [06]   | `ShortestPathsDag`                                  | directed acyclic graph   | DAG-optimized paths        |
|  [07]   | `MinimumSpanningTreeKruskal`                        | undirected graph         | Kruskal spanning forest    |
|  [08]   | `MinimumSpanningTreePrim`                           | nonempty connected graph | Prim spanning tree         |
|  [09]   | `MaximumFlow`                                       | augmented mutable graph  | Edmonds-Karp max-flow      |
|  [10]   | `MaximumBipartiteMatchingAlgorithm<TVertex, TEdge>` | bipartite partitions     | bipartite maximum matching |

[BELLMAN_FORD]:

- Result: `TryFunc<TVertex, IEnumerable<TEdge>>`
- Verdict: `out bool hasNegativeCycle`

[DIJKSTRA]:

- Input: directed or undirected graph, edge weights, root
- Result: `TryFunc<TVertex, IEnumerable<TEdge>>`

[MAXIMUM_FLOW]:

- Input: positive capacities, source, sink, edge factory, reversed-edge augmentor
- Precondition: `ReversedEdgeAugmentorAlgorithm.AddReversedEdges()` completed
- Mutation: auxiliary reversed edges remain until caller cleanup
- Result: `double` plus `out TryFunc<TVertex, TEdge>`

[MAXIMUM_BIPARTITE_MATCHING]:

- Input: mutable graph, vertex partitions, vertex factory, edge factory
- Mutation: temporary super-terminal and reversed-edge augmentation
- Cleanup: augmentation rolls back before `Compute()` returns
- Result: `MatchedEdges` after `Compute()`

## [04]-[IMPLEMENTATION_LAW]

[QUIKGRAPH_TOPOLOGY]:

- `TVertex` carries domain identity, and `TEdge` implements `IEdge<TVertex>` with domain payload where required.
- `AddVerticesAndEdge` admits both endpoints, while `AddVertexRange` preserves isolated vertices.
- `BidirectionalGraph<TVertex, TEdge>` serves predecessor access, and `UndirectedGraph<TVertex, TEdge>` serves symmetric adjacency.
- A materialized graph keys on domain content; a memoized result also keys on every algorithm input.

[STACKING]:

- Geometry partitions disconnected kNN graphs and runs `MinimumSpanningTreePrim` once per component before its BFS sign fold.
- Constraint analysis composes `ConnectedComponents` for islands and `MaximumBipartiteMatchingAlgorithm<TVertex, TEdge>` for structural rank.
- Snapshot topology composes content-keyed `BidirectionalGraph<TVertex, TEdge>` views beside the domain incidence index.
- Scheduling composes directional Kahn order while retaining calendar arithmetic in the domain fold.
- Systems tracing composes `TreeBreadthFirstSearch`.
- Version ancestry composes BFS closure intersection over its commit DAG.
- Typed receipts retain domain algebra after every graph projection.

[LOCAL_ADMISSION]:

- `AlgorithmExtensions` owns the standard entry rail over a domain-folded graph and projects its result onto a typed receipt.
- An algorithm object binds when traversal events, mutable component state, or augmentation lifecycle form part of the result contract.
- `OfflineLeastCommonAncestor` binds rooted trees; a multi-parent DAG retains BFS closure intersection as its domain merge-base fold.

[RAIL_LAW]:

- Package: `QuikGraph`
- Owns: generic graph containers, edge shapes, and graph algorithms
- Accept: domain-folded graphs with explicit weight, capacity, root, partition, and factory inputs
- Reject: domain identity, arithmetic, persistence, analytical projection, and receipt ownership
