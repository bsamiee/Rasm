# [RASM_API_QUIKGRAPH]

`QuikGraph` owns the managed graph lane: generic vertex-and-edge containers, the edge shape family every algorithm binds, the projection layer between container forms, and the traversal, ordering, component, path, spanning-tree, flow, and matching algorithms over them. Its boundary is the domain-folded graph — the caller supplies vertices, weights, capacities, roots, partitions, and factories, and every result leaves as an ordering, a component map, a `TryFunc` accessor, or an edge sequence.

## [01]-[PUBLIC_TYPES]

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
|  [17]   | `DelegateIncidenceGraph<TVertex, TEdge>`              | lazy class    | one `TryFunc` out-edge accessor, no vertex set |

[PUBLIC_TYPE_SCOPE]: `QuikGraph.Predicates` — the filtered-view ladder over a base container and the `Test`-shaped predicates that feed it

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `FilteredGraph<TVertex, TEdge, TGraph>`                  | view class    | the ladder root, `BaseGraph` plus both predicates |
|  [02]   | `FilteredImplicitGraph<TVertex, TEdge, TGraph>`          | view class    | filtered out-edge access                          |
|  [03]   | `FilteredIncidenceGraph<TVertex, TEdge, TGraph>`         | view class    | filtered incidence and edge lookup                |
|  [04]   | `FilteredImplicitVertexSet<TVertex, TEdge, TGraph>`      | view class    | filtered vertex membership                        |
|  [05]   | `FilteredVertexListGraph<TVertex, TEdge, TGraph>`        | view class    | filtered vertex enumeration                       |
|  [06]   | `FilteredEdgeListGraph<TVertex, TEdge, TGraph>`          | view class    | filtered edge enumeration                         |
|  [07]   | `FilteredVertexAndEdgeListGraph<TVertex, TEdge, TGraph>` | view class    | filtered vertex and edge sets together            |
|  [08]   | `FilteredUndirectedGraph<TVertex, TEdge, TGraph>`        | view class    | filtered symmetric adjacency                      |
|  [09]   | `ResidualEdgePredicate<TVertex, TEdge>`                  | class         | `Test` on positive residual capacity              |
|  [10]   | `ReversedResidualEdgePredicate<TVertex, TEdge>`          | class         | `Test` on the reverse arc's residual              |
|  [11]   | `InDictionaryVertexPredicate<TVertex, TValue>`           | class         | `Test` on membership in a vertex map              |
|  [12]   | `IsolatedVertexPredicate<TVertex, TEdge>`                | class         | `Test` on zero in- and out-degree                 |
|  [13]   | `SinkVertexPredicate<TVertex, TEdge>`                    | class         | `Test` on zero out-degree                         |

- Every filtered view takes `(TGraph baseGraph, VertexPredicate<TVertex>, EdgePredicate<TVertex, TEdge>)` and holds the base by reference, so the scope is a live read and a later mutation of the base shows through. `FilteredBidirectionalGraph<TVertex, TEdge, TGraph>` is the predecessor-bearing member of the same ladder. The `Test`-shaped predicates in rows [09]-[13] are METHODS, not `VertexPredicate`/`EdgePredicate` values — a view binds them as a method group.

[PUBLIC_TYPE_SCOPE]: delegates, observers, and the algorithm service surface

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `TryFunc<T, TResult>`                                   | delegate      | `bool` return with the payload on `out`        |
|  [02]   | `VertexAction<TVertex>`                                 | delegate      | vertex event fold                              |
|  [03]   | `EdgeAction<TVertex, TEdge>`                            | delegate      | edge event fold                                |
|  [04]   | `VertexPredicate<TVertex>`                              | delegate      | vertex filter for views and removal            |
|  [05]   | `EdgePredicate<TVertex, TEdge>`                         | delegate      | edge filter for views and removal              |
|  [06]   | `VertexFactory<TVertex>`                                | delegate      | synthetic vertex mint for augmentation         |
|  [07]   | `EdgeFactory<TVertex, TEdge>`                           | delegate      | synthetic edge mint for augmentation           |
|  [08]   | `EdgeEqualityComparer<TVertex>`                         | delegate      | undirected or sorted vertex-pair equality      |
|  [09]   | `VertexPredecessorRecorderObserver<TVertex, TEdge>`     | observer      | `VerticesPredecessors` with `TryGetPath`       |
|  [10]   | `VertexPredecessorPathRecorderObserver<TVertex, TEdge>` | observer      | `AllPaths` and `EndPathVertices`               |
|  [11]   | `VertexDistanceRecorderObserver<TVertex, TEdge>`        | observer      | `Distances` under a chosen relaxer             |
|  [12]   | `EdgeRecorderObserver<TVertex, TEdge>`                  | observer      | `Edges` in visit order                         |
|  [13]   | `EdgePredecessorRecorderObserver<TVertex, TEdge>`       | observer      | `AllPaths`, `MergedPath`, `EndPathEdges`       |
|  [14]   | `VertexTimeStamperObserver<TVertex>`                    | observer      | `DiscoverTimes` and `FinishTimes`              |
|  [15]   | `IDistanceRelaxer`                                      | interface     | `InitialDistance` plus `Combine` accumulation  |
|  [16]   | `DistanceRelaxers`                                      | static class  | one relaxer static per accumulation rule       |
|  [17]   | `IEdgeChain<TVertex, TEdge>`                            | interface     | successor selection for one walk step          |
|  [18]   | `IMarkovEdgeChain<TVertex, TEdge>`                      | interface     | `IEdgeChain` plus the walk's `Rand` seat       |
|  [19]   | `MarkovEdgeChainBase<TVertex, TEdge>`                   | abstract      | `Rand` seat defaulting to a `CryptoRandom`     |
|  [20]   | `NormalizedMarkovEdgeChain<TVertex, TEdge>`             | class         | uniform draw over the out-edges                |
|  [21]   | `WeightedMarkovEdgeChain<TVertex, TEdge>`               | class         | weight-proportional draw over a weight map     |
|  [22]   | `VanishingWeightedMarkovEdgeChain<TVertex, TEdge>`      | class         | weighted draw decaying the taken edge          |
|  [23]   | `RoundRobinEdgeChain<TVertex, TEdge>`                   | class         | per-vertex cyclic index, no `Rand`             |
|  [24]   | `ICancelManager`                                        | interface     | `Cancel`, `IsCancelling`, `CancelRequested`    |
|  [25]   | `IDistancesCollection<TVertex>`                         | interface     | the live distance read every path solver bears |

- Rows [17]-[23] are the walk policy `RandomWalkAlgorithm` and `CyclePoppingRandomTreeAlgorithm` take: the Markov chains draw through the `Rand` a `CryptoRandom` seeds by default, so a reproducible walk assigns its own seeded `Random` before `Compute()`, while `RoundRobinEdgeChain` carries no randomness at all and is the deterministic frontier for a coverage walk. `VanishingWeightedMarkovEdgeChain` multiplies the taken edge's weight by `Factor` (default `0.2`) and renormalizes its siblings, so revisits fall away across one traversal and the supplied `IDictionary<TEdge, double>` is MUTATED in place.
- Every observer subscribes through its own `Attach` method, and the method names the capability it needs, never a concrete algorithm: the predecessor and distance recorders take `ITreeBuilderAlgorithm<TVertex, TEdge>`, the path recorder `IVertexPredecessorRecorderAlgorithm<TVertex, TEdge>`, the edge-predecessor recorder `IEdgePredecessorRecorderAlgorithm<TVertex, TEdge>`, and the time stamper `IVertexTimeStamperAlgorithm<TVertex>`, so one recorder serves every traversal implementing its interface.

[PUBLIC_TYPE_SCOPE]: algorithm objects — the stateful traversal, path, flow, matching, and coloring surface

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `AlgorithmBase<TGraph>`                                     | abstract class | `Compute`, `Abort`, `State`, `Services`         |
|  [02]   | `BreadthFirstSearchAlgorithm<TVertex, TEdge>`               | class          | traversal events over a chosen `IQueue`         |
|  [03]   | `UndirectedBreadthFirstSearchAlgorithm<TVertex, TEdge>`     | class          | level traversal over symmetric adjacency        |
|  [04]   | `DepthFirstSearchAlgorithm<TVertex, TEdge>`                 | class          | `MaxDepth` and `ProcessAllComponents` DFS       |
|  [05]   | `UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>`       | class          | DFS over an undirected container                |
|  [06]   | `BidirectionalDepthFirstSearchAlgorithm<TVertex, TEdge>`    | class          | DFS over out- and in-edges together             |
|  [07]   | `EdgeDepthFirstSearchAlgorithm<TVertex, TEdge>`             | class          | edge-centric DFS for predecessor recording      |
|  [08]   | `ImplicitDepthFirstSearchAlgorithm<TVertex, TEdge>`         | class          | DFS over a delegate-adjacent graph              |
|  [09]   | `ImplicitEdgeDepthFirstSearchAlgorithm<TVertex, TEdge>`     | class          | edge-centric DFS with no vertex set             |
|  [10]   | `BestFirstFrontierSearchAlgorithm<TVertex, TEdge>`          | class          | relaxer-ordered frontier to a seated target     |
|  [11]   | `DijkstraShortestPathAlgorithm<TVertex, TEdge>`             | class          | distance state plus relaxation events           |
|  [12]   | `UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>`   | class          | distance state over symmetric adjacency         |
|  [13]   | `AStarShortestPathAlgorithm<TVertex, TEdge>`                | class          | heuristic-guided distance state                 |
|  [14]   | `BellmanFordShortestPathAlgorithm<TVertex, TEdge>`          | class          | signed weights with cycle detection             |
|  [15]   | `DagShortestPathAlgorithm<TVertex, TEdge>`                  | class          | one topological pass under a relaxer            |
|  [16]   | `FloydWarshallAllShortestPathAlgorithm<TVertex, TEdge>`     | class          | all-pairs `TryGetDistance` and `TryGetPath`     |
|  [17]   | `YenShortestPathsAlgorithm<TVertex>`                        | class          | k loopless paths as `SortedPath` values         |
|  [18]   | `HoffmanPavleyRankedShortestPathAlgorithm<TVertex, TEdge>`  | class          | ranked deviation paths to a target              |
|  [19]   | `StronglyConnectedComponentsAlgorithm<TVertex, TEdge>`      | class          | `Components`, `Roots`, `Graphs`, `Steps`        |
|  [20]   | `IncrementalConnectedComponentsAlgorithm<TVertex, TEdge>`   | class          | live component count across mutation            |
|  [21]   | `EdmondsKarpMaximumFlowAlgorithm<TVertex, TEdge>`           | class          | augmenting max-flow over residual capacity      |
|  [22]   | `ReversedEdgeAugmentorAlgorithm<TVertex, TEdge>`            | class          | reversed-edge add and removal lifecycle         |
|  [23]   | `MaximumBipartiteMatchingAlgorithm<TVertex, TEdge>`         | class          | `MatchedEdges` over two vertex partitions       |
|  [24]   | `EulerianTrailAlgorithm<TVertex, TEdge>`                    | class          | `Circuit` and `Trails` with temporary edges     |
|  [25]   | `RandomWalkAlgorithm<TVertex, TEdge>`                       | class          | `IEdgeChain`-driven walk under `EndPredicate`   |
|  [26]   | `CyclePoppingRandomTreeAlgorithm<TVertex, TEdge>`           | class          | `Successors` tree by loop erasure               |
|  [27]   | `TarjanOfflineLeastCommonAncestorAlgorithm<TVertex, TEdge>` | class          | `Ancestors` over a rooted vertex-pair set       |
|  [28]   | `CondensationGraphAlgorithm<TVertex, TEdge, TGraph>`        | class          | `CondensedGraph` over component subgraphs       |
|  [29]   | `VertexColoringAlgorithm<TVertex, TEdge>`                   | class          | greedy `Colors` map with a colored event        |
|  [30]   | `KernighanLinAlgorithm<TVertex, TEdge>`                     | class          | balanced two-way `Partition` by cut cost        |
|  [31]   | `MinimumVertexCoverApproximationAlgorithm<TVertex, TEdge>`  | class          | randomized `CoverSet`                           |
|  [32]   | `PageRankAlgorithm<TVertex, TEdge>`                         | class          | `Ranks` under `Damping` and `Tolerance`         |
|  [33]   | `TSP<TVertex, TEdge, TGraph>`                               | class          | `BestCost` and `ResultPath` by branch-and-bound |
|  [34]   | `HungarianAlgorithm`                                        | class          | `AgentsTasks` assignment over a cost matrix     |
|  [35]   | `MaximumCliqueAlgorithmBase<TVertex, TEdge>`                | abstract class | the whole `Cliques` namespace, unimplemented    |
|  [36]   | `IsEulerianGraphAlgorithm<TVertex, TEdge>`                  | class          | `IsEulerian` predicate                          |
|  [37]   | `IsHamiltonianGraphAlgorithm<TVertex, TEdge>`               | class          | `IsHamiltonian` predicate                       |

- Row [35] is the ONLY type in `QuikGraph.Algorithms.Cliques` and it ships no concrete subclass, so a maximum-clique need has no member to bind here and lands as a domain fold or a different package outright.

[PUBLIC_TYPE_SCOPE]: frontier structures and disjoint sets

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `IQueue<T>`                          | interface     | the frontier slot `BreadthFirstSearch` takes  |
|  [02]   | `IPriorityQueue<T>`                  | interface     | `IQueue<T>` plus `Update` on a changed key    |
|  [03]   | `BinaryQueue<TVertex, TDistance>`    | class         | binary-heap priority frontier                 |
|  [04]   | `FibonacciQueue<TVertex, TDistance>` | class         | Fibonacci-heap frontier, cheap decrease-key   |
|  [05]   | `SoftHeap<TKey, TValue>`             | class         | bounded-corruption heap under `ErrorRate`     |
|  [06]   | `IDisjointSet<T>`                    | interface     | `MakeSet`, `Union`, `FindSet`, `AreInSameSet` |
|  [07]   | `ForestDisjointSet<T>`               | class         | union-find with `SetCount` and `ElementCount` |

[FAULTS]: `NonAcyclicGraphException` `NegativeCycleGraphException` `NegativeWeightException` `NegativeCapacityException` `NoPathFoundException` `NonStronglyConnectedGraphException` `ParallelEdgeNotAllowedException` `VertexNotFoundException` `QuikGraphException`

## [02]-[ENTRYPOINTS]

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
|  [11]   | `MaximumFlow(IMutableVertexAndEdgeListGraph, Func<TEdge, double>, TVertex, TVertex)`               | static  | Edmonds-Karp max flow    |

- `AlgorithmExtensions.RankedShortestPathHoffmanPavley`: returns `IEnumerable<IEnumerable<TEdge>>` and defaults `maxCount` to `3`.
- `AlgorithmExtensions.OfflineLeastCommonAncestor`: returns `TryFunc<SEquatableEdge<TVertex>, TVertex>` keyed on the pairs supplied up front.
- `AlgorithmExtensions.MaximumFlow`: closes on `out TryFunc<TVertex, TEdge>`, `EdgeFactory<TVertex, TEdge>`, and a constructed `ReversedEdgeAugmentorAlgorithm<TVertex, TEdge>`; it returns the max flow as `double` over a capacity fold, requires that augmentor to have run `AddReversedEdges()`, and leaves the auxiliary edges until `RemoveReversedEdges()`. It constructs the algorithm object internally and surfaces only the flow value and the predecessor accessor, so a caller needing the CUT binds the object below.

[ENTRYPOINT_SCOPE]: rooted traversal — the `RootedAlgorithmBase` run surface every root-seeded algorithm inherits, and the `BreadthFirstSearchAlgorithm` event fan a visitor subscribes instead of re-walking the frontier

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `RootedAlgorithmBase.Compute(TVertex root)`         | virtual  | set the root then run                    |
|  [02]   | `RootedAlgorithmBase.SetRootVertex(TVertex)`        | instance | seat the root, fire the change event     |
|  [03]   | `RootedAlgorithmBase.TryGetRootVertex(out TVertex)` | instance | read the seated root, `false` when unset |
|  [04]   | `RootedAlgorithmBase.RootVertexChanged`             | event    | `EventHandler` on a re-seated root       |
|  [05]   | `BreadthFirstSearchAlgorithm.InitializeVertex`      | event    | once per vertex before the walk          |
|  [06]   | `BreadthFirstSearchAlgorithm.StartVertex`           | event    | the root the walk opens on               |
|  [07]   | `BreadthFirstSearchAlgorithm.ExamineVertex`         | event    | vertex dequeued from the frontier        |
|  [08]   | `BreadthFirstSearchAlgorithm.DiscoverVertex`        | event    | vertex first reached, entering the queue |
|  [09]   | `BreadthFirstSearchAlgorithm.FinishVertex`          | event    | every out-edge of the vertex examined    |
|  [10]   | `BreadthFirstSearchAlgorithm.ExamineEdge`           | event    | edge visited from an examined vertex     |
|  [11]   | `BreadthFirstSearchAlgorithm.TreeEdge`              | event    | edge that discovered a white target      |
|  [12]   | `BreadthFirstSearchAlgorithm.NonTreeEdge`           | event    | edge onto an already-discovered target   |
|  [13]   | `BreadthFirstSearchAlgorithm.GrayTarget`            | event    | non-tree edge onto a queued target       |
|  [14]   | `BreadthFirstSearchAlgorithm.BlackTarget`           | event    | non-tree edge onto a finished target     |

- `Compute(root)` is `SetRootVertex(root)` then the parameterless `Compute()`, so seating a root and running are one call; the parameterless form on an unrooted instance has no root to walk from.
- Subscribers share ONE walk through the event fan: a distance recorder, a predecessor recorder, and a level partition all attach to the same run rather than each re-running it. `DiscoverVertex` fires once per vertex on first reach, `ExamineVertex` once per dequeue, and `FinishVertex` after its out-edges — a depth or level fold keys on `TreeEdge` and `DiscoverVertex`, never on `ExamineEdge`, which fires for every incident arc.
- `GrayTarget` and `BlackTarget` partition `NonTreeEdge` by the target's colour, so a cross-edge-versus-back-edge classification reads a case rather than probing `VerticesColors` mid-walk.

[ENTRYPOINT_SCOPE]: maximum-flow objects — the augmentation lifecycle and the residual state a minimum cut reads; the solver constructor closes on `EdgeFactory<TVertex, TEdge>` and a constructed `ReversedEdgeAugmentorAlgorithm<TVertex, TEdge>` after its capacity fold

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `ReversedEdgeAugmentorAlgorithm(IMutableVertexAndEdgeListGraph, EdgeFactory)` | ctor     | augmentor over one graph  |
|  [02]   | `ReversedEdgeAugmentorAlgorithm.AddReversedEdges()`                           | instance | mint the missing reverses |
|  [03]   | `ReversedEdgeAugmentorAlgorithm.RemoveReversedEdges()`                        | instance | retire the auxiliaries    |
|  [04]   | `ReversedEdgeAugmentorAlgorithm.ReversedEdges -> IDictionary<TEdge, TEdge>`   | instance | forward-to-reverse map    |
|  [05]   | `ReversedEdgeAugmentorAlgorithm.Augmented -> bool`                            | instance | augmentation state        |
|  [06]   | `EdmondsKarpMaximumFlowAlgorithm(graph, Func<TEdge, double>, …)`              | ctor     | augmenting solver         |
|  [07]   | `MaximumFlowAlgorithm.Compute(TVertex, TVertex)`                              | instance | run source to sink        |
|  [08]   | `MaximumFlowAlgorithm.MaxFlow -> double`                                      | instance | the flow value            |
|  [09]   | `MaximumFlowAlgorithm.ResidualCapacities -> Dictionary<TEdge, double>`        | instance | residual state per edge   |
|  [10]   | `MaximumFlowAlgorithm.Predecessors -> Dictionary<TVertex, TEdge>`             | instance | augmenting-path parents   |
|  [11]   | `MaximumFlowAlgorithm.VerticesColors -> IDictionary<TVertex, GraphColor>`     | instance | last traversal colouring  |

- `ReversedEdgeAugmentorAlgorithm` is `IDisposable` and its dispose runs `RemoveReversedEdges()`, so a `using` scope bounds the auxiliary edges to the solve.
- `EdmondsKarpMaximumFlowAlgorithm` throws `ArgumentException` when the augmentor targets a different graph instance, so both take the SAME container reference.
- `ResidualCapacities` derives the MINIMUM CUT from published state: a breadth-first walk from the source over out-edges holding positive residual capacity yields the source side, while `VerticesColors` reports only the last traversal's own colouring.
- `ReversedEdgeAugmentorAlgorithm` mints one reverse per edge, so a graph whose domain already carries both directions constructs with `allowParallelEdges: true`; `false` silently drops half the residual capacity and the solve cuts the wrong edges.
- `Edge<TVertex>` declares `Source`, `Target`, and `ToString` alone, so its identity is its INSTANCE — a capacity or weight map keyed by it under `ReferenceEqualityComparer` distinguishes an augmentor-minted reverse from the arc it duplicates. `SEdge<TVertex>` and `EquatableEdge<TVertex>` carry value identity, so the same map collapses those two onto one entry and hands the solver twice the residual capacity the arc carries.

[ENTRYPOINT_SCOPE]: path, matching, tour, and trail objects — the constructor knobs and published products the `AlgorithmExtensions` entry does not surface

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `DagShortestPathAlgorithm(IVertexListGraph, Func<TEdge, double>)`                | ctor     | defaults to `ShortestDistance`            |
|  [02]   | `DagShortestPathAlgorithm(…, IDistanceRelaxer)`                                  | ctor     | relaxer-chosen accumulation               |
|  [03]   | `DagShortestPathAlgorithm(IAlgorithmComponent?, …)`                              | ctor     | hosted form sharing caller services       |
|  [04]   | `ShortestPathAlgorithmBase.GetDistance(TVertex) -> double`                       | instance | distance read, faults when unrecorded     |
|  [05]   | `ShortestPathAlgorithmBase.TryGetDistance(TVertex, out double)`                  | instance | guarded distance read                     |
|  [06]   | `ShortestPathAlgorithmBase.GetDistances()`                                       | instance | every vertex-distance pair                |
|  [07]   | `ShortestPathAlgorithmBase.Weights -> Func<TEdge, double>`                       | property | the weight fold as constructed            |
|  [08]   | `ShortestPathAlgorithmBase.DistanceRelaxer -> IDistanceRelaxer`                  | property | the accumulation rule in force            |
|  [09]   | `ShortestPathAlgorithmBase.GetVertexColor(TVertex) -> GraphColor`                | instance | per-vertex traversal state after the run  |
|  [10]   | `BestFirstFrontierSearchAlgorithm(IBidirectionalIncidenceGraph, …)`              | ctor     | relaxer as the sole frontier knob         |
|  [11]   | `RootedSearchAlgorithmBase.Compute(TVertex root, TVertex target)`                | instance | run root to a seated target               |
|  [12]   | `RootedSearchAlgorithmBase.SetTargetVertex(TVertex)` / `.ClearTargetVertex()`    | instance | seat and clear the target                 |
|  [13]   | `RootedSearchAlgorithmBase.TargetReached` / `.TargetVertexChanged`               | event    | target-hit and re-seat signals            |
|  [14]   | `MaximumBipartiteMatchingAlgorithm(IMutableVertexAndEdgeListGraph, …)`           | ctor     | both partitions plus the two factories    |
|  [15]   | `MaximumBipartiteMatchingAlgorithm.MatchedEdges -> TEdge[]`                      | instance | the matching after `Compute()`            |
|  [16]   | `MaximumBipartiteMatchingAlgorithm.SourceToVertices` / `.VerticesToSink`         | instance | the partitions as constructed             |
|  [17]   | `TSP(TGraph, Func<TEdge, double>)`                                               | ctor     | branch-and-bound tour over one graph      |
|  [18]   | `TSP.ResultPath -> BidirectionalGraph<TVertex, TEdge>`                           | instance | the tour AS A GRAPH, not a sequence       |
|  [19]   | `TSP.BestCost -> double`                                                         | instance | `PositiveInfinity` until a tour closes    |
|  [20]   | `EulerianTrailAlgorithm(IMutableVertexAndEdgeListGraph)`                         | ctor     | trail solver over a MUTABLE container     |
|  [21]   | `EulerianTrailAlgorithm.Circuit -> TEdge[]`                                      | instance | the single closed circuit                 |
|  [22]   | `EulerianTrailAlgorithm.Trails()` / `.Trails(TVertex)`                           | instance | `ICollection<TEdge>` per trail            |
|  [23]   | `EulerianTrailAlgorithm.AddTemporaryEdges(EdgeFactory) -> TEdge[]`               | instance | odd-degree repair, returns what it minted |
|  [24]   | `EulerianTrailAlgorithm.RemoveTemporaryEdges()`                                  | instance | retire the repair edges                   |
|  [25]   | `EulerianTrailAlgorithm.ComputeEulerianPathCount(IVertexAndEdgeListGraph)`       | static   | trail count from the odd-degree census    |
|  [26]   | `HoffmanPavleyRankedShortestPathAlgorithm(IBidirectionalGraph, …)`               | ctor     | optional relaxer, optional host           |
|  [27]   | `HoffmanPavleyRankedShortestPathAlgorithm.Compute(TVertex, TVertex)`             | instance | run one root-target pair                  |
|  [28]   | `RankedShortestPathAlgorithmBase.ShortestPathCount`                              | property | k, default `3`, must exceed `1`           |
|  [29]   | `RankedShortestPathAlgorithmBase.ComputedShortestPaths`                          | instance | ranked `IEnumerable<TEdge>` sequence      |
|  [30]   | `RankedShortestPathAlgorithmBase.ComputedShortestPathCount`                      | instance | how many k the run actually found         |
|  [31]   | `YenShortestPathsAlgorithm(AdjacencyGraph, TVertex, TVertex, int, Func?, Func?)` | ctor     | source, target, k, weights, path filter   |
|  [32]   | `YenShortestPathsAlgorithm.Execute() -> IEnumerable<SortedPath>`                 | instance | the k loopless paths                      |
|  [33]   | `YenShortestPathsAlgorithm.SortedPath` — `Count`, `IEquatable<SortedPath>`       | struct   | value-equal path over tagged edges        |
|  [34]   | `IncrementalConnectedComponentsAlgorithm(IMutableVertexAndEdgeSet)`              | ctor     | subscribes to the container's events      |
|  [35]   | `IncrementalConnectedComponentsAlgorithm.ComponentCount -> int`                  | instance | live count, valid only after `Compute()`  |
|  [36]   | `IncrementalConnectedComponentsAlgorithm.GetComponents()`                        | instance | `KeyValuePair<int, IDictionary<…, int>>`  |
|  [37]   | `IncrementalConnectedComponentsAlgorithm.Dispose()`                              | instance | unsubscribe from the mutation events      |

- `DagShortestPathAlgorithm` takes `IVertexListGraph` and runs ONE `TopologicalSort` pass, so it throws `NonAcyclicGraphException` on a cyclic container rather than relaxing. Under `DistanceRelaxers.CriticalDistance` — `InitialDistance` of `double.MinValue` and an inverted `Compare` — the same pass yields the LONGEST path, so a critical-path fold reads `GetDistances()` directly instead of negating weights into a shortest-path run.
- `IDistancesCollection<TVertex>` is the whole distance contract every `ShortestPathAlgorithmBase` and `UndirectedShortestPathAlgorithmBase` implements: `TryGetDistance` guards, `GetDistance` throws `VertexNotFoundException` on an unrecorded vertex, and `GetDistances()` enumerates the pairs. A raw distance dictionary is not part of the live surface.
- `MaximumBipartiteMatchingAlgorithm` MUTATES the supplied container during `Compute()`: it mints a super-source, a super-sink, and a reverse per edge through the two factories, then rolls both augmentations back in a `finally`. The graph must therefore be the caller's own working copy, `VertexFactory` must mint vertices no partition already holds, and `MatchedEdges` is a fresh array per read.
- `EulerianTrailAlgorithm` also mutates: `AddTemporaryEdges` pairs the `OddVertices` before `Compute()` and `RemoveTemporaryEdges` retires them, so a trail over a graph with more than two odd vertices brackets the solve between the two calls.
- `YenShortestPathsAlgorithm` is the one path algorithm that fixes its own containers: `AdjacencyGraph<TVertex, EquatableTaggedEdge<TVertex, double>>` with the weight in the edge `Tag`, source, target, and k at CONSTRUCTION, and `Execute()` rather than `Compute()`. Its `filter` argument post-processes each candidate round.

[ENTRYPOINT_SCOPE]: combinatorial objects — assignment, balanced partition, coloring, condensation, union-find; each `AlgorithmBase` object runs via `Compute()` and publishes its product as a property

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `HungarianAlgorithm(int[,] costs)`                                           | ctor     | assignment over an agent-x-task matrix    |
|  [02]   | `HungarianAlgorithm.Compute() -> int[]`                                      | instance | task index per agent row                  |
|  [03]   | `HungarianAlgorithm.AgentsTasks -> int[]`                                    | instance | the computed assignment, re-readable      |
|  [04]   | `HungarianAlgorithm.GetIterations() -> IEnumerable<HungarianIteration>`      | instance | per-step matrix/mask/row-col evidence     |
|  [05]   | `KernighanLinAlgorithm(IUndirectedGraph<TVertex, TEdge>, int nbIterations)`  | ctor     | balanced bisection over tagged edges      |
|  [06]   | `KernighanLinAlgorithm.Partition -> Partition<TVertex>`                      | instance | product after `Compute()`                 |
|  [07]   | `Partition<TVertex>` — `VertexSetA`/`VertexSetB` sorted sets, `CutCost`      | struct   | the two halves and the crossing weight    |
|  [08]   | `VertexColoringAlgorithm(IUndirectedGraph<TVertex, TEdge>)`                  | ctor     | greedy coloring, `IEdge` edges            |
|  [09]   | `VertexColoringAlgorithm.Colors -> IDictionary<TVertex, int?>`               | instance | color index per vertex after `Compute`    |
|  [10]   | `CondensationGraphAlgorithm(IVertexAndEdgeListGraph<TVertex, TEdge>)`        | ctor     | `TGraph : IMutableVertexAndEdgeSet`       |
|  [11]   | `CondensationGraphAlgorithm.CondensedGraph`                                  | instance | condensed bidirectional component graph   |
|  [12]   | `CondensationGraphAlgorithm.StronglyConnected -> bool`                       | instance | SCC (`true`, default) or weak contraction |
|  [13]   | `ForestDisjointSet()` / `ForestDisjointSet(int capacity)`                    | ctor     | union-find forest                         |
|  [14]   | `ForestDisjointSet.MakeSet(T)` / `.FindSet(T) -> T` / `.Union(T, T) -> bool` | instance | singleton mint, representative, merge     |
|  [15]   | `ForestDisjointSet.AreInSameSet(T, T) -> bool` / `.Contains(T) -> bool`      | instance | membership predicates                     |
|  [16]   | `ForestDisjointSet.SetCount` / `.ElementCount -> int`                        | instance | live partition census                     |

- `KernighanLinAlgorithm<TVertex, TEdge>` lives at `QuikGraph.Algorithms.GraphPartition` and constrains `TEdge : IUndirectedEdge<TVertex>, ITagged<double>` — the cut weight IS the edge `Tag`, so a weightless bisection tags every edge `1.0`; `Partition<TVertex>` sorts both halves, so `TVertex` is `IComparable` in practice.
- `VertexColoringAlgorithm<TVertex, TEdge>` lives at `QuikGraph.Algorithms.VertexColoring`; `Colors` values are dense from `0`, so `Colors.Values.Max() + 1` is the batch count and one color class is one non-conflicting concurrent batch.
- `CondensationGraphAlgorithm<TVertex, TEdge, TGraph>` lives at `QuikGraph.Algorithms.Condensation`; `HungarianAlgorithm` at `QuikGraph.Algorithms.Assignment` binds no graph container — its whole input is the rectangular `int[,]` cost matrix; `ForestDisjointSet<T>` at `QuikGraph.Collections` — `Union` returns `true` only when it merged two distinct sets.

[ENTRYPOINT_SCOPE]: `ClusteredAdjacencyGraph` cluster hierarchy — the nesting surface on top of the `AdjacencyGraph` write API every container shares

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `ClusteredAdjacencyGraph(AdjacencyGraph<TVertex, TEdge>)`          | ctor     | root cluster over a wrapped graph        |
|  [02]   | `ClusteredAdjacencyGraph(ClusteredAdjacencyGraph<TVertex, TEdge>)` | ctor     | child cluster under a parent             |
|  [03]   | `ClusteredAdjacencyGraph.AddCluster() -> ClusteredAdjacencyGraph`  | instance | mint and register a child                |
|  [04]   | `ClusteredAdjacencyGraph.RemoveCluster(IClusteredGraph)`           | instance | drop a child by reference                |
|  [05]   | `ClusteredAdjacencyGraph.Clusters -> IEnumerable`                  | property | the children, UNTYPED `IEnumerable`      |
|  [06]   | `ClusteredAdjacencyGraph.ClustersCount -> int`                     | property | direct-child census                      |
|  [07]   | `ClusteredAdjacencyGraph.Parent`                                   | property | owning cluster, `null` at the root       |
|  [08]   | `ClusteredAdjacencyGraph.Collapsed -> bool`                        | property | mutable render/fold flag, no read effect |

- A vertex or edge added to a child propagates UP to every ancestor, so the root's wrapped graph always carries the union and a cluster is a scoped write handle rather than a second container. Removal propagates BOTH ways — down through every descendant holding it and up through every ancestor — so `RemoveVertex` on a leaf cluster evicts it from the whole hierarchy, and a scoped retraction removes from the wrapped graph directly. `Clusters` is the non-generic `IEnumerable` off `IClusteredGraph`, so a typed walk casts each element itself.

[ENTRYPOINT_SCOPE]: `GraphExtensions` materialization and copy — `GraphExtensions` mints a new container over the source's vertices and edges, and `AlgorithmExtensions.Clone` fills a supplied one through cloner delegates

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :---------------------------------- |
|  [01]   | `ToAdjacencyGraph(IEnumerable<TEdge>, bool)`                                          | static  | materialize from edges              |
|  [02]   | `ToAdjacencyGraph(IEnumerable<TVertex>, Func<TVertex, IEnumerable<TEdge>>, bool)`     | static  | materialize from a fold             |
|  [03]   | `ToAdjacencyGraph(TVertex[][])`                                                       | static  | materialize from pair rows          |
|  [04]   | `ToBidirectionalGraph(IEnumerable<TEdge>, bool)`                                      | static  | predecessor graph from edges        |
|  [05]   | `ToBidirectionalGraph(IEnumerable<TVertex>, Func<TVertex, IEnumerable<TEdge>>, bool)` | static  | predecessor graph from a fold       |
|  [06]   | `ToBidirectionalGraph(IVertexAndEdgeListGraph)`                                       | static  | add an in-edge index                |
|  [07]   | `ToBidirectionalGraph(IUndirectedGraph)`                                              | static  | direct a symmetric graph            |
|  [08]   | `ToUndirectedGraph(IEnumerable<TEdge>, bool)`                                         | static  | symmetric from edges                |
|  [09]   | `ToArrayAdjacencyGraph(IVertexAndEdgeListGraph)`                                      | static  | freeze an outgoing snapshot         |
|  [10]   | `ToArrayBidirectionalGraph(IBidirectionalGraph)`                                      | static  | freeze predecessor state            |
|  [11]   | `ToArrayUndirectedGraph(IUndirectedGraph)`                                            | static  | freeze a symmetric snapshot         |
|  [12]   | `ToCompressedRowGraph(IVertexAndEdgeListGraph)`                                       | static  | pack incidence as CSR               |
|  [13]   | `Clone(IVertexAndEdgeListGraph, Func, Func, IMutableVertexAndEdgeSet)`                | static  | deep copy into a supplied container |

- `AlgorithmExtensions.Clone`: takes `Func<TVertex, TVertex>` and `Func<TEdge, TVertex, TVertex, TEdge>` cloners plus the destination `IMutableVertexAndEdgeSet<TVertex, TEdge>`, returning `void` — it is the working-copy mint for the algorithms that MUTATE their container, so a matching or trail solve never runs over the caller's own graph.

[ENTRYPOINT_SCOPE]: `GraphExtensions` delegate-backed views — each container defers to its accessor delegates and materializes nothing

| [INDEX] | [SURFACE]                                                                                      | [SHAPE] | [CAPABILITY]              |
| :-----: | :--------------------------------------------------------------------------------------------- | :------ | :------------------------ |
|  [01]   | `ToDelegateIncidenceGraph(TryFunc<TVertex, IEnumerable<TEdge>>)`                               | static  | out-edge accessor only    |
|  [02]   | `ToDelegateVertexAndEdgeListGraph(IEnumerable<TVertex>, TryFunc<TVertex, IEnumerable<TEdge>>)` | static  | vertex roster + out-edges |
|  [03]   | `ToDelegateVertexAndEdgeListGraph(IDictionary<TVertex, TEdges>)`                               | static  | adjacency map as roster   |
|  [04]   | `ToDelegateBidirectionalIncidenceGraph(TryFunc, TryFunc)`                                      | static  | paired out- and in-edges  |
|  [05]   | `ToDelegateUndirectedGraph(IEnumerable<TVertex>, TryFunc<TVertex, IEnumerable<TEdge>>)`        | static  | symmetric edge accessor   |

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
|  [11]   | `UndirectedVertexEquality(IEdge<TVertex>, TVertex, TVertex) -> bool`             | static  | endpoint-pair equality, either order  |
|  [12]   | `SortedVertexEquality(IEdge<TVertex>, TVertex, TVertex) -> bool`                 | static  | endpoint-pair equality, sorted order  |
|  [13]   | `ReverseEdges(IEnumerable<TEdge>) -> IEnumerable<SReversedEdge<TVertex, TEdge>>` | static  | reverse a sequence without copy       |

- Rows [11] and [12] ARE the `EdgeEqualityComparer<TVertex>` shape an `UndirectedGraph<TVertex, TEdge>` constructor takes as a method group; only the unsorted form has a `Get*` factory, so a sorted-pair container binds `EdgeExtensions.SortedVertexEquality` directly.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AddVerticesAndEdge` admits both endpoints where `AddEdge` requires them present, and `AddVertexRange` preserves the isolated vertices a later fold reads.
- `AddEdgeRange(IEnumerable<TEdge>)` batches edge admission under `AddEdge` semantics — endpoints must already be present (verified on the installed 2.5.0).
- Direction is a container choice: `IVertexListGraph` serves outgoing traversal, `IBidirectionalGraph` predecessor access, `IUndirectedGraph` symmetric adjacency, and `GraphExtensions` converts between them.
- `TryFunc` accessors signal an unreachable target as a `false` return, never a fault.
- Every algorithm object folds through `AlgorithmBase<TGraph>`: `Compute()` runs it, `State` and the `Started`/`Finished`/`Aborted` events report it, and `Services` carries the `ICancelManager` an `Abort()` trips.
- Observers scope to the `IDisposable` their `Attach(...)` returns, so a recorder composes onto one traversal instead of subclassing it.
- `IDistanceRelaxer` decides accumulation, and `DistanceRelaxers` carries one static relaxer per rule: `ShortestDistance` sums, `CriticalDistance` takes the longest path, `EdgeShortestDistance` relaxes per edge, `Prim` keeps the single edge weight.
- Materialized graphs key on domain content; a memoized algorithm result also keys on every weight, capacity, root, partition, and factory input.

[STACKING]:
- `CSparse`(`.api/api-csparse.md`): pattern-graph decomposition stays on `SymbolicColumnStorage` through its own `DulmageMendelsohn` and `StronglyConnectedComponents`, so a sparse matrix never round-trips into a vertex-and-edge container and this package takes only graphs the domain already folds.
- `Google.OrTools.Graph`(`Rasm.Compute/.api/api-ortools.md`): metric flow at the circulation egress runner is OrTools' `MaxFlow` and `MinCostFlow`; this package feeds them the space-adjacency subgraph and keeps the structural side — `MaximumFlow` augmentation and `MaximumBipartiteMatchingAlgorithm` for structural rank.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Op.Catch` preserves each thrown `NonAcyclicGraphException`, `NegativeCycleGraphException`, `NoPathFoundException`, or `VertexNotFoundException` until an owning boundary classifies it; a `TryFunc` `bool`-plus-`out` refusal maps directly without exception capture.
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): `STRtree<T>.Query(Envelope)` mints the candidate pairs a domain fold turns into edges; NTS owns planar predicate topology and this package the incidence algebra over the resulting graph.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `[SmartEnum<TKey>]` vertex keys and `[ValueObject<T>]` weights cross in as `TVertex` and `Func<TEdge, double>`, and every ordering, component map, and path leaves as a generated domain result.
- Within-library: one domain fold mints `AdjacencyGraph` or `BidirectionalGraph` per owner, `ToArrayBidirectionalGraph` freezes the content-keyed snapshot the memo binds, `FilteredBidirectionalGraph` scopes a subproblem without a second materialization, `DelegateVertexAndEdgeListGraph` serves a lazily-adjacent domain index outright, and one attached observer set projects the traversal into the domain result. `Rasm.Materials/Appearance/graph#MATERIAL_GRAPH` folds the appearance DAG onto `IsDirectedAcyclicGraph` and `SourceFirstTopologicalSort`, and `Rasm.Materials/Raster/tile#TILE_SYNTH` binds the flow objects directly — the min-cut needs `ResidualCapacities`, which the `AlgorithmExtensions.MaximumFlow` entry does not surface. `Rasm.Compute/Runtime/scheduling#JOB_GRAPH` composes the ordering and condensation objects over the job DAG, and `Rasm.Compute/Runtime/payload#RESIDENCY` binds `ForestDisjointSet` for the meshlet shell partition and `KernighanLinAlgorithm` for cluster bisection; `Rasm.Fabrication/Ingress/solid#SOLID_TOPOLOGY` binds `ForestDisjointSet<int>` for the mesh shell census, `SetCount` reading the shell count directly. `Rasm/Processing/flow#TOPOLOGY` composes `ToAdjacencyGraph` + `StronglyConnectedComponents` + `CondensateStronglyConnected` over the cell-transition digraph; `Rasm/Solving/solver#CONSTRAINT_SOLVER` binds `ForestDisjointSet<int>` for the island partition with `SetCount` as its census; `Rasm/Meshing/arrangement#ARRANGEMENT` composes `UndirectedGraph` + `ConnectedComponents` for the managed shell decomposition.

[LOCAL_ADMISSION]:
- `AlgorithmExtensions` is the entry surface over a domain-folded graph; an algorithm object binds only where traversal events, mutable component state, or augmentation lifecycle are part of the result contract.
- `GraphExtensions` owns every container conversion, so a domain index reaches an algorithm through one `To*` projection.
- `OfflineLeastCommonAncestor` binds rooted trees; a multi-parent DAG resolves its merge base through BFS closure intersection in the domain fold.
- `MaximumBipartiteMatchingAlgorithm` rolls its super-terminal and reversed-edge augmentation back inside `Compute()`, and `MatchedEdges` reads after it returns; the type lives at `QuikGraph.Algorithms` (decompile-verified), never `QuikGraph.Algorithms.MaximumFlow` where the namespace roster suggests it.
- `IQueue<TVertex>` selects the frontier ONLY on `BreadthFirstSearchAlgorithm`: `BinaryQueue`, `FibonacciQueue` where decrease-key dominates, `SoftHeap` where bounded corruption buys the bound. Every other search seats its own frontier — `AStarShortestPathAlgorithm` and `DijkstraShortestPathAlgorithm` construct `FibonacciQueue<TVertex, double>` in `Initialize()`, and `BestFirstFrontierSearchAlgorithm` takes only `(IBidirectionalIncidenceGraph, Func<TEdge, double>, IDistanceRelaxer)` with an optional host, running an INTERNAL `BinaryHeap` — so `IDistanceRelaxer` is the one composition knob on all three.
