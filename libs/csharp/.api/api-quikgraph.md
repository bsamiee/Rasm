# [RASM_API_QUIKGRAPH]

`QuikGraph` owns the managed graph lane: generic vertex-and-edge containers, the edge shape family every algorithm binds, the projection rail between container forms, and the traversal, ordering, component, path, spanning-tree, flow, and matching algorithms over them. Its boundary is the domain-folded graph — the caller supplies vertices, weights, capacities, roots, partitions, and factories, and every result leaves as an ordering, a component map, a `TryFunc` accessor, or an edge sequence.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `QuikGraph`
- package: `QuikGraph` (MS-PL, Alexandre Rabérin)
- assembly: `QuikGraph`
- namespace: `QuikGraph`, `.Algorithms`, `.Algorithms.Assignment`, `.Algorithms.Cliques`, `.Algorithms.Condensation`, `.Algorithms.ConnectedComponents`, `.Algorithms.Exploration`, `.Algorithms.GraphPartition`, `.Algorithms.MaximumFlow`, `.Algorithms.MinimumSpanningTree`, `.Algorithms.Observers`, `.Algorithms.RandomWalks`, `.Algorithms.RankedShortestPath`, `.Algorithms.Ranking`, `.Algorithms.Search`, `.Algorithms.Services`, `.Algorithms.ShortestPath`, `.Algorithms.TopologicalSort`, `.Algorithms.TSP`, `.Algorithms.VertexColoring`, `.Algorithms.VertexCover`, `.Collections`, `.Predicates`
- abi: `TEdge : IEdge<TVertex>` constrains every container and algorithm; `TVertex` takes no constraint, so identity rides the caller's comparer
- rail: graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: edge shapes — `Source` and `Target` are the whole contract, a tagged shape adds `Tag`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `IEdge<TVertex>`                        | interface     | constrains every container and algorithm      |
|  [02]   | `IUndirectedEdge<TVertex>`              | interface     | source-before-target ordering marker          |
|  [03]   | `ITermEdge<TVertex>`                    | interface     | `SourceTerminal` and `TargetTerminal` indices |
|  [04]   | `ITagged<TTag>`                         | interface     | mutable `Tag` raising `TagChanged`            |
|  [05]   | `SEdge<TVertex>`                        | struct        | value directed edge                           |
|  [06]   | `SEquatableEdge<TVertex>`               | struct        | value-equal edge, the ancestry pair carrier   |
|  [07]   | `SReversedEdge<TVertex, TEdge>`         | struct        | reversed view over a wrapped edge             |
|  [08]   | `STaggedEdge<TVertex, TTag>`            | struct        | value payload edge                            |
|  [09]   | `Edge<TVertex>`                         | class         | reference directed edge                       |
|  [10]   | `EquatableEdge<TVertex>`                | class         | value-equal reference edge                    |
|  [11]   | `TaggedEdge<TVertex, TTag>`             | class         | payload-carrying reference edge               |
|  [12]   | `EquatableTaggedEdge<TVertex, TTag>`    | class         | value-equal payload edge, the Yen input shape |
|  [13]   | `TermEdge<TVertex>`                     | class         | port-indexed reference edge                   |
|  [14]   | `MergedEdge<TVertex, TEdge>`            | class         | condensed edge carrying its merged `Edges`    |
|  [15]   | `CondensedEdge<TVertex, TEdge, TGraph>` | class         | inter-component edge over condensed subgraphs |

[PUBLIC_TYPE_SCOPE]: graph containers — mutable owners, frozen snapshots, and zero-copy views

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `AdjacencyGraph<TVertex, TEdge>`                      | mutable class | outgoing incidence, the default write target   |
|  [02]   | `BidirectionalGraph<TVertex, TEdge>`                  | mutable class | predecessor access, `MergeVertex`, `Degree`    |
|  [03]   | `UndirectedGraph<TVertex, TEdge>`                     | mutable class | symmetric adjacency under an edge comparer     |
|  [04]   | `EdgeListGraph<TVertex, TEdge>`                       | mutable class | edge set with no vertex-keyed incidence        |
|  [05]   | `BidirectionalMatrixGraph<TEdge>`                     | mutable class | dense int-vertex adjacency matrix              |
|  [06]   | `ClusteredAdjacencyGraph<TVertex, TEdge>`             | mutable class | nested cluster hierarchy over one parent graph |
|  [07]   | `ArrayAdjacencyGraph<TVertex, TEdge>`                 | frozen class  | immutable outgoing snapshot                    |
|  [08]   | `ArrayBidirectionalGraph<TVertex, TEdge>`             | frozen class  | immutable predecessor snapshot                 |
|  [09]   | `ArrayUndirectedGraph<TVertex, TEdge>`                | frozen class  | immutable symmetric snapshot                   |
|  [10]   | `CompressedSparseRowGraph<TVertex>`                   | frozen class  | CSR incidence over `SEquatableEdge<TVertex>`   |
|  [11]   | `BidirectionalAdapterGraph<TVertex, TEdge>`           | view class    | in-edge index over an outgoing-only graph      |
|  [12]   | `ReversedBidirectionalGraph<TVertex, TEdge>`          | view class    | direction flip yielding `SReversedEdge`        |
|  [13]   | `UndirectedBidirectionalGraph<TVertex, TEdge>`        | view class    | symmetric read over a directed graph           |
|  [14]   | `FilteredBidirectionalGraph<TVertex, TEdge, TGraph>`  | view class    | predicate-scoped subgraph, no copy             |
|  [15]   | `DelegateVertexAndEdgeListGraph<TVertex, TEdge>`      | lazy class    | vertex sequence plus a `TryFunc` adjacency     |
|  [16]   | `DelegateBidirectionalIncidenceGraph<TVertex, TEdge>` | lazy class    | paired out- and in-edge accessors              |

[PUBLIC_TYPE_SCOPE]: delegates, observers, and the algorithm service surface

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------ | :------------ | :-------------------------------------------- |
|  [01]   | `TryFunc<T, TResult>`                                   | delegate      | `bool` return with the payload on `out`       |
|  [02]   | `VertexAction<TVertex>`                                 | delegate      | vertex event fold                             |
|  [03]   | `EdgeAction<TVertex, TEdge>`                            | delegate      | edge event fold                               |
|  [04]   | `VertexPredicate<TVertex>`                              | delegate      | vertex filter for views and removal           |
|  [05]   | `EdgePredicate<TVertex, TEdge>`                         | delegate      | edge filter for views and removal             |
|  [06]   | `VertexFactory<TVertex>`                                | delegate      | synthetic vertex mint for augmentation        |
|  [07]   | `EdgeFactory<TVertex, TEdge>`                           | delegate      | synthetic edge mint for augmentation          |
|  [08]   | `EdgeEqualityComparer<TVertex>`                         | delegate      | undirected or sorted vertex-pair equality     |
|  [09]   | `VertexPredecessorRecorderObserver<TVertex, TEdge>`     | observer      | `VerticesPredecessors` plus `TryGetPath`      |
|  [10]   | `VertexPredecessorPathRecorderObserver<TVertex, TEdge>` | observer      | `AllPaths` and `EndPathVertices`              |
|  [11]   | `VertexDistanceRecorderObserver<TVertex, TEdge>`        | observer      | `Distances` under a chosen relaxer            |
|  [12]   | `EdgeRecorderObserver<TVertex, TEdge>`                  | observer      | `Edges` in visit order                        |
|  [13]   | `EdgePredecessorRecorderObserver<TVertex, TEdge>`       | observer      | `AllPaths`, `MergedPath`, `EndPathEdges`      |
|  [14]   | `VertexTimeStamperObserver<TVertex>`                    | observer      | `DiscoverTimes` and `FinishTimes`             |
|  [15]   | `IDistanceRelaxer`                                      | interface     | `InitialDistance` plus `Combine` accumulation |
|  [16]   | `ICancelManager`                                        | interface     | `Cancel`, `IsCancelling`, `CancelRequested`   |

[PUBLIC_TYPE_SCOPE]: algorithm objects — the stateful traversal, path, flow, matching, and coloring surface

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `AlgorithmBase<TGraph>`                                     | abstract class | `Compute`, `Abort`, `State`, `Services`         |
|  [02]   | `BreadthFirstSearchAlgorithm<TVertex, TEdge>`               | class          | traversal events over a chosen `IQueue`         |
|  [03]   | `DepthFirstSearchAlgorithm<TVertex, TEdge>`                 | class          | `MaxDepth` and `ProcessAllComponents` DFS       |
|  [04]   | `EdgeDepthFirstSearchAlgorithm<TVertex, TEdge>`             | class          | edge-centric DFS for predecessor recording      |
|  [05]   | `DijkstraShortestPathAlgorithm<TVertex, TEdge>`             | class          | distance state plus relaxation events           |
|  [06]   | `AStarShortestPathAlgorithm<TVertex, TEdge>`                | class          | heuristic-guided distance state                 |
|  [07]   | `BellmanFordShortestPathAlgorithm<TVertex, TEdge>`          | class          | signed weights with cycle detection             |
|  [08]   | `FloydWarshallAllShortestPathAlgorithm<TVertex, TEdge>`     | class          | all-pairs `TryGetDistance` and `TryGetPath`     |
|  [09]   | `YenShortestPathsAlgorithm<TVertex>`                        | class          | k loopless paths as `SortedPath` values         |
|  [10]   | `HoffmanPavleyRankedShortestPathAlgorithm<TVertex, TEdge>`  | class          | ranked deviation paths to a target              |
|  [11]   | `StronglyConnectedComponentsAlgorithm<TVertex, TEdge>`      | class          | `Components`, `Roots`, `Graphs`, `Steps`        |
|  [12]   | `IncrementalConnectedComponentsAlgorithm<TVertex, TEdge>`   | class          | live component count across mutation            |
|  [13]   | `EdmondsKarpMaximumFlowAlgorithm<TVertex, TEdge>`           | class          | augmenting max-flow over residual capacity      |
|  [14]   | `ReversedEdgeAugmentorAlgorithm<TVertex, TEdge>`            | class          | reversed-edge add and removal lifecycle         |
|  [15]   | `MaximumBipartiteMatchingAlgorithm<TVertex, TEdge>`         | class          | `MatchedEdges` over two vertex partitions       |
|  [16]   | `EulerianTrailAlgorithm<TVertex, TEdge>`                    | class          | `Circuit` and `Trails` with temporary edges     |
|  [17]   | `RandomWalkAlgorithm<TVertex, TEdge>`                       | class          | `IEdgeChain`-driven walk under `EndPredicate`   |
|  [18]   | `TarjanOfflineLeastCommonAncestorAlgorithm<TVertex, TEdge>` | class          | `Ancestors` over a rooted vertex-pair set       |
|  [19]   | `CondensationGraphAlgorithm<TVertex, TEdge, TGraph>`        | class          | `CondensedGraph` over component subgraphs       |
|  [20]   | `VertexColoringAlgorithm<TVertex, TEdge>`                   | class          | greedy `Colors` map with a colored event        |
|  [21]   | `KernighanLinAlgorithm<TVertex, TEdge>`                     | class          | balanced two-way `Partition` by cut cost        |
|  [22]   | `MinimumVertexCoverApproximationAlgorithm<TVertex, TEdge>`  | class          | randomized `CoverSet`                           |
|  [23]   | `PageRankAlgorithm<TVertex, TEdge>`                         | class          | `Ranks` under `Damping` and `Tolerance`         |
|  [24]   | `TSP<TVertex, TEdge, TGraph>`                               | class          | `BestCost` and `ResultPath` by branch-and-bound |
|  [25]   | `HungarianAlgorithm`                                        | class          | `AgentsTasks` assignment over a cost matrix     |
|  [26]   | `IsEulerianGraphAlgorithm<TVertex, TEdge>`                  | class          | `IsEulerian` predicate                          |
|  [27]   | `IsHamiltonianGraphAlgorithm<TVertex, TEdge>`               | class          | `IsHamiltonian` predicate                       |

[PUBLIC_TYPE_SCOPE]: frontier structures and disjoint sets

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `IQueue<T>`                          | interface     | frontier contract every search constructor takes |
|  [02]   | `IPriorityQueue<T>`                  | interface     | `IQueue<T>` plus `Update` on a changed key       |
|  [03]   | `BinaryQueue<TVertex, TDistance>`    | class         | binary-heap priority frontier                    |
|  [04]   | `FibonacciQueue<TVertex, TDistance>` | class         | Fibonacci-heap frontier, cheap decrease-key      |
|  [05]   | `SoftHeap<TKey, TValue>`             | class         | bounded-corruption heap under `ErrorRate`        |
|  [06]   | `IDisjointSet<T>`                    | interface     | `MakeSet`, `Union`, `FindSet`, `AreInSameSet`    |
|  [07]   | `ForestDisjointSet<T>`               | class         | union-find with `SetCount` and `ElementCount`    |

[FAULTS]: `NonAcyclicGraphException` `NegativeCycleGraphException` `NegativeWeightException` `NegativeCapacityException` `NoPathFoundException` `NonStronglyConnectedGraphException` `ParallelEdgeNotAllowedException` `VertexNotFoundException` `QuikGraphException`

## [03]-[ENTRYPOINTS]

Every graph interface named in a signature is `<TVertex, TEdge>`-parameterized and abbreviates to its bare name.

[ENTRYPOINT_SCOPE]: `AlgorithmExtensions` ordering, reachability, and acyclicity — orderings and vertex sets return `IEnumerable<TVertex>`, tree searches return `TryFunc<TVertex, IEnumerable<TEdge>>`, predicates return `bool`

| [INDEX] | [SURFACE]                                                                                | [SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `TopologicalSort(IVertexListGraph)`                                                      | static  | DFS order over a directed graph     |
|  [02]   | `TopologicalSort(IUndirectedGraph)`                                                      | static  | DFS order over an undirected graph  |
|  [03]   | `SourceFirstTopologicalSort(IVertexAndEdgeListGraph)`                                    | static  | Kahn source-degree order            |
|  [04]   | `SourceFirstTopologicalSort(IUndirectedGraph)`                                           | static  | Kahn order over adjacency degree    |
|  [05]   | `SourceFirstBidirectionalTopologicalSort(IBidirectionalGraph)`                           | static  | forward Kahn order                  |
|  [06]   | `SourceFirstBidirectionalTopologicalSort(IBidirectionalGraph, TopologicalSortDirection)` | static  | `Forward` or `Backward` order       |
|  [07]   | `IsDirectedAcyclicGraph(IVertexListGraph)`                                               | static  | acyclicity over a container         |
|  [08]   | `IsDirectedAcyclicGraph(IEnumerable<TEdge>)`                                             | static  | acyclicity over bare edges          |
|  [09]   | `IsUndirectedAcyclicGraph(IUndirectedGraph)`                                             | static  | forest predicate over a container   |
|  [10]   | `IsUndirectedAcyclicGraph(IEnumerable<TEdge>)`                                           | static  | forest predicate over bare edges    |
|  [11]   | `TreeBreadthFirstSearch(IVertexListGraph, TVertex)`                                      | static  | BFS path accessor from a root       |
|  [12]   | `TreeDepthFirstSearch(IVertexListGraph, TVertex)`                                        | static  | DFS path accessor from a root       |
|  [13]   | `TreeCyclePoppingRandom(IVertexListGraph, TVertex, IMarkovEdgeChain)`                    | static  | uniform random spanning tree        |
|  [14]   | `Roots(IVertexListGraph)`                                                                | static  | DFS forest roots                    |
|  [15]   | `Roots(IBidirectionalGraph)`                                                             | static  | zero-indegree vertices              |
|  [16]   | `Sinks(IVertexListGraph)`                                                                | static  | zero-outdegree vertices             |
|  [17]   | `IsolatedVertices(IBidirectionalGraph)`                                                  | static  | zero-degree vertices                |
|  [18]   | `OddVertices(IVertexAndEdgeListGraph)`                                                   | static  | odd-degree vertices, the Euler gate |

- `TopologicalSort`, `SourceFirstTopologicalSort`, and their bidirectional forms throw `NonAcyclicGraphException` on cyclic input.

[ENTRYPOINT_SCOPE]: `AlgorithmExtensions` components, condensation, and transitive structure — labeling extensions fill a supplied `IDictionary<TVertex, int>` and return the component count, transitive folds return `BidirectionalGraph<TVertex, TEdge>`

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `ConnectedComponents(IUndirectedGraph, IDictionary<TVertex, int>)`             | static  | undirected labels                       |
|  [02]   | `StronglyConnectedComponents(IVertexListGraph, IDictionary<TVertex, int>)`     | static  | Tarjan labels                           |
|  [03]   | `WeaklyConnectedComponents(IVertexListGraph, IDictionary<TVertex, int>)`       | static  | weak-component labels                   |
|  [04]   | `IncrementalConnectedComponents(IMutableVertexAndEdgeSet, out Func<...>)`      | static  | live count across mutation              |
|  [05]   | `ComputeDisjointSet(IUndirectedGraph) -> IDisjointSet<TVertex>`                | static  | union-find over the component partition |
|  [06]   | `CondensateStronglyConnected<TVertex, TEdge, TGraph>(IVertexAndEdgeListGraph)` | static  | SCC contraction                         |
|  [07]   | `CondensateWeaklyConnected<TVertex, TEdge, TGraph>(IVertexAndEdgeListGraph)`   | static  | weak-component contraction              |
|  [08]   | `CondensateEdges(IBidirectionalGraph, VertexPredicate)`                        | static  | degree-two path contraction             |
|  [09]   | `ComputeTransitiveClosure(IEdgeListGraph, Func<TVertex, TVertex, TEdge>)`      | static  | reachability closure                    |
|  [10]   | `ComputeTransitiveReduction(IEdgeListGraph)`                                   | static  | redundant-edge removal                  |

- `AlgorithmExtensions.IncrementalConnectedComponents`: its `out Func<KeyValuePair<int, IDictionary<TVertex, int>>>` accessor reads the live labeling, and the returned `IDisposable` bounds the subscription to the graph's mutation events.
- `AlgorithmExtensions.CondensateStronglyConnected`, `.CondensateWeaklyConnected`: both return `IMutableBidirectionalGraph<TGraph, CondensedEdge<TVertex, TEdge, TGraph>>`, so a component is itself a graph and the inter-component edge carries its merged `Edges`.
- `AlgorithmExtensions.CondensateEdges`: returns `IMutableBidirectionalGraph<TVertex, MergedEdge<TVertex, TEdge>>`, keeping only vertices the predicate admits.

[ENTRYPOINT_SCOPE]: `AlgorithmExtensions` weighted paths, spanning trees, flow, and ancestry — path extensions return `TryFunc<TVertex, IEnumerable<TEdge>>` and spanning-tree extensions `IEnumerable<TEdge>`

| [INDEX] | [SURFACE]                                                                                          | [SHAPE] | [CAPABILITY]             |
| :-----: | :------------------------------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `ShortestPathsDijkstra(IVertexAndEdgeListGraph, Func<TEdge, double>, TVertex)`                     | static  | non-negative weights     |
|  [02]   | `ShortestPathsDijkstra(IUndirectedGraph, Func<TEdge, double>, TVertex)`                            | static  | non-negative weights     |
|  [03]   | `ShortestPathsAStar(IVertexAndEdgeListGraph, Func<TEdge, double>, Func<TVertex, double>, TVertex)` | static  | admissible heuristic     |
|  [04]   | `ShortestPathsBellmanFord(IVertexAndEdgeListGraph, Func<TEdge, double>, TVertex, out bool)`        | static  | signed weights           |
|  [05]   | `ShortestPathsDag(IVertexAndEdgeListGraph, Func<TEdge, double>, TVertex)`                          | static  | DAG single pass          |
|  [06]   | `RankedShortestPathHoffmanPavley(IBidirectionalGraph, Func<TEdge, double>, TVertex, TVertex, int)` | static  | k ranked paths           |
|  [07]   | `MinimumSpanningTreePrim(IUndirectedGraph, Func<TEdge, double>)`                                   | static  | Prim tree, one component |
|  [08]   | `MinimumSpanningTreeKruskal(IUndirectedGraph, Func<TEdge, double>)`                                | static  | Kruskal forest           |
|  [09]   | `OfflineLeastCommonAncestor(IVertexListGraph, TVertex, IEnumerable<SEquatableEdge<TVertex>>)`      | static  | rooted-tree LCA          |
|  [10]   | `ComputePredecessorCost(IDictionary<TVertex, TEdge>, IDictionary<TEdge, double>, TVertex)`         | static  | recovered-path cost      |

- `AlgorithmExtensions.RankedShortestPathHoffmanPavley`: returns `IEnumerable<IEnumerable<TEdge>>` and defaults `maxCount` to `3`.
- `AlgorithmExtensions.OfflineLeastCommonAncestor`: returns `TryFunc<SEquatableEdge<TVertex>, TVertex>` keyed on the pairs supplied up front.
- `AlgorithmExtensions.MaximumFlow`: returns the max flow as `double` over a capacity fold, requires the passed `ReversedEdgeAugmentorAlgorithm` to have run `AddReversedEdges()`, and leaves the auxiliary edges until `RemoveReversedEdges()`.

[ENTRYPOINT_SCOPE]: `GraphExtensions` container projection — every conversion mints a new container over the source's vertices and edges

| [INDEX] | [SURFACE]                                                                                      | [SHAPE] | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------------------------------------- | :------ | :-------------------------- |
|  [01]   | `ToAdjacencyGraph(IEnumerable<TEdge>, bool)`                                                   | static  | materialize from edges      |
|  [02]   | `ToAdjacencyGraph(IEnumerable<TVertex>, Func<TVertex, IEnumerable<TEdge>>, bool)`              | static  | materialize from a fold     |
|  [03]   | `ToAdjacencyGraph(TVertex[][])`                                                                | static  | materialize from pair rows  |
|  [04]   | `ToBidirectionalGraph(IVertexAndEdgeListGraph)`                                                | static  | add an in-edge index        |
|  [05]   | `ToBidirectionalGraph(IUndirectedGraph)`                                                       | static  | direct a symmetric graph    |
|  [06]   | `ToUndirectedGraph(IEnumerable<TEdge>, bool)`                                                  | static  | symmetric from edges        |
|  [07]   | `ToArrayAdjacencyGraph(IVertexAndEdgeListGraph)`                                               | static  | freeze an outgoing snapshot |
|  [08]   | `ToArrayBidirectionalGraph(IBidirectionalGraph)`                                               | static  | freeze predecessor state    |
|  [09]   | `ToArrayUndirectedGraph(IUndirectedGraph)`                                                     | static  | freeze a symmetric snapshot |
|  [10]   | `ToCompressedRowGraph(IVertexAndEdgeListGraph)`                                                | static  | pack incidence as CSR       |
|  [11]   | `ToDelegateVertexAndEdgeListGraph(IEnumerable<TVertex>, TryFunc<TVertex, IEnumerable<TEdge>>)` | static  | lazy graph, domain index    |
|  [12]   | `ToDelegateVertexAndEdgeListGraph(IDictionary<TVertex, TEdges>)`                               | static  | lazy graph, adjacency map   |
|  [13]   | `ToDelegateBidirectionalIncidenceGraph(TryFunc, TryFunc)`                                      | static  | lazy graph, paired edges    |
|  [14]   | `ToDelegateUndirectedGraph(IEnumerable<TVertex>, TryFunc<TVertex, IEnumerable<TEdge>>)`        | static  | lazy symmetric graph        |

- `GraphExtensions.ToDelegateBidirectionalIncidenceGraph`: both parameters are `TryFunc<TVertex, IEnumerable<TEdge>>`, out-edges first and in-edges second.

[ENTRYPOINT_SCOPE]: `EdgeExtensions` edge and path predicates over a recovered result

| [INDEX] | [SURFACE]                                                                        | [SHAPE] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `IsSelfEdge(IEdge<TVertex>) -> bool`                                             | static  | loop detection                        |
|  [02]   | `IsAdjacent(IEdge<TVertex>, TVertex) -> bool`                                    | static  | endpoint membership                   |
|  [03]   | `GetOtherVertex(IEdge<TVertex>, TVertex) -> TVertex`                             | static  | opposite endpoint                     |
|  [04]   | `ToVertexPair(IEdge<TVertex>) -> SEquatableEdge<TVertex>`                        | static  | value-equal pair for ancestry input   |
|  [05]   | `IsPath(IEnumerable<TEdge>) -> bool`                                             | static  | contiguity of a recovered sequence    |
|  [06]   | `HasCycles(IEnumerable<TEdge>) -> bool`                                          | static  | repeated vertex in a walk             |
|  [07]   | `IsPathWithoutCycles(IEnumerable<TEdge>) -> bool`                                | static  | simple-path predicate                 |
|  [08]   | `TryGetPath(IDictionary<TVertex, TEdge>, TVertex, out IEnumerable<TEdge>)`       | static  | walk a raw predecessor map            |
|  [09]   | `IsPredecessor(IDictionary<TVertex, TEdge>, TVertex, TVertex) -> bool`           | static  | reachability inside a predecessor map |
|  [10]   | `GetUndirectedVertexEquality() -> EdgeEqualityComparer<TVertex>`                 | static  | comparer for an undirected container  |
|  [11]   | `ReverseEdges(IEnumerable<TEdge>) -> IEnumerable<SReversedEdge<TVertex, TEdge>>` | static  | reverse a sequence without copy       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AddVerticesAndEdge` admits both endpoints where `AddEdge` requires them present, and `AddVertexRange` preserves the isolated vertices a later fold reads.
- Direction is a container choice: `IVertexListGraph` serves outgoing traversal, `IBidirectionalGraph` predecessor access, `IUndirectedGraph` symmetric adjacency, and `GraphExtensions` converts between them.
- A `TryFunc` accessor signals an unreachable target as a `false` return, never a fault.
- Every algorithm object folds through `AlgorithmBase<TGraph>`: `Compute()` runs it, `State` and the `Started`/`Finished`/`Aborted` events report it, and `Services` carries the `ICancelManager` an `Abort()` trips.
- An observer scopes to the `IDisposable` its `Attach(...)` returns, so a recorder composes onto one traversal instead of subclassing it.
- `IDistanceRelaxer` decides accumulation, and `DistanceRelaxers` carries one static relaxer per rule: `ShortestDistance` sums, `CriticalDistance` takes the longest path, `EdgeShortestDistance` relaxes per edge, `Prim` keeps the single edge weight.
- A materialized graph keys on domain content; a memoized algorithm result also keys on every weight, capacity, root, partition, and factory input.

[STACKING]:
- `CSparse`(`.api/api-csparse.md`): pattern-graph decomposition stays on `SymbolicColumnStorage` through its own `DulmageMendelsohn` and `StronglyConnectedComponents`, so a sparse matrix never round-trips into a vertex-and-edge container and this rail takes only graphs the domain already folds.
- `Google.OrTools.Graph`(`Rasm.Compute/.api/api-ortools.md`): metric flow at the circulation egress runner is OrTools' `MaxFlow` and `MinCostFlow`; this package feeds them the space-adjacency subgraph and keeps the structural side — `MaximumFlow` augmentation and `MaximumBipartiteMatchingAlgorithm` for structural rank.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Try.lift(...).Run()` traps `NonAcyclicGraphException`, `NegativeCycleGraphException`, `NoPathFoundException`, and `VertexNotFoundException` onto `Fin<A>`, and a `TryFunc` `bool`-plus-`out` result converts on the same seam.
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): `STRtree<T>.Query(Envelope)` mints the candidate pairs a domain fold turns into edges; NTS owns planar predicate topology and this package the incidence algebra over the resulting graph.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `[SmartEnum<TKey>]` vertex keys and `[ValueObject<T>]` weights cross in as `TVertex` and `Func<TEdge, double>`, and every ordering, component map, and path leaves onto a generated receipt.
- Within-library: one domain fold mints `AdjacencyGraph` or `BidirectionalGraph` per owner, `ToArrayBidirectionalGraph` freezes the content-keyed snapshot the memo binds, `FilteredBidirectionalGraph` scopes a subproblem without a second materialization, `DelegateVertexAndEdgeListGraph` serves a lazily-adjacent domain index outright, and one attached observer set projects the traversal onto the typed receipt.

[LOCAL_ADMISSION]:
- `AlgorithmExtensions` is the entry rail over a domain-folded graph; an algorithm object binds only where traversal events, mutable component state, or augmentation lifecycle are part of the result contract.
- `GraphExtensions` owns every container conversion, so a domain index reaches an algorithm through one `To*` projection.
- `OfflineLeastCommonAncestor` binds rooted trees; a multi-parent DAG resolves its merge base through BFS closure intersection in the domain fold.
- `MaximumBipartiteMatchingAlgorithm` rolls its super-terminal and reversed-edge augmentation back inside `Compute()`, and `MatchedEdges` reads after it returns.
- `IQueue<TVertex>` selects the frontier on the search constructor that takes one: `BinaryQueue`, `FibonacciQueue` where decrease-key dominates, `SoftHeap` where bounded corruption buys the bound.

[RAIL_LAW]:
- Package: `QuikGraph`
- Owns: generic graph containers, the edge shape family, container projection, and every algorithm over them
- Accept: domain-folded graphs carrying caller-supplied weights, capacities, roots, partitions, comparers, and factories, with `IQueue<TVertex>`, `IDistanceRelaxer`, and observer attachment as the composition knobs
- Reject: domain identity, arithmetic, persistence, analytical projection, and receipt ownership; a hand-rolled adjacency map, visited-set traversal, or path reconstruction beside the container and `TryFunc` accessor this package owns
