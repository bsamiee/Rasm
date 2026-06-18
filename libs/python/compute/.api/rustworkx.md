# [PY_COMPUTE_API_RUSTWORKX]

`rustworkx` supplies Rust-backed graph containers (`PyGraph`, `PyDiGraph`, `PyDAG`) and a comprehensive algorithm library covering shortest paths, centrality, traversal, layout, isomorphism, connectivity, generation, and serialization for the compute graph-topology rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rustworkx`
- package: `rustworkx`
- module: `rustworkx`
- asset: runtime library (Rust extension)
- rail: graph-topology

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph container types
- rail: graph-topology

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]          | [RAIL]                                   |
| :-----: | :------------------ | :--------------------- | :--------------------------------------- |
|   [1]   | `PyGraph`           | undirected graph       | multigraph, node/edge payloads           |
|   [2]   | `PyDiGraph`         | directed graph         | multigraph, predecessor/successor access |
|   [3]   | `PyDAG`             | directed acyclic graph | cycle-checked directed container         |
|   [4]   | `TopologicalSorter` | topo-sort state        | incremental topological order            |

[PUBLIC_TYPE_SCOPE]: result container types
- rail: graph-topology

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]       | [RAIL]                       |
| :-----: | :---------------------------- | :------------------ | :--------------------------- |
|   [1]   | `AllPairsMultiplePathMapping` | path result         | all-pairs multi-path         |
|   [2]   | `AllPairsPathLengthMapping`   | length result       | all-pairs length map         |
|   [3]   | `AllPairsPathMapping`         | path result         | all-pairs single-path map    |
|   [4]   | `BFSPredecessors`             | traversal result    | BFS predecessor map          |
|   [5]   | `BFSSuccessors`               | traversal result    | BFS successor map            |
|   [6]   | `BiconnectedComponents`       | connectivity result | biconnected component set    |
|   [7]   | `CentralityMapping`           | centrality result   | node centrality map          |
|   [8]   | `EdgeCentralityMapping`       | centrality result   | edge centrality map          |
|   [9]   | `EdgeIndexMap`                | index result        | edge index mapping           |
|  [10]   | `EdgeIndices`                 | index result        | edge index list              |
|  [11]   | `EdgeList`                    | edge result         | plain edge list              |
|  [12]   | `MultiplePathMapping`         | path result         | single-source multi-path map |
|  [13]   | `NodeIndices`                 | index result        | node index list              |
|  [14]   | `NodeMap`                     | mapping result      | node data map                |
|  [15]   | `PathLengthMapping`           | length result       | single-source length map     |
|  [16]   | `PathMapping`                 | path result         | single-source path map       |
|  [17]   | `Pos2DMapping`                | layout result       | 2D position map              |
|  [18]   | `WeightedEdgeList`            | edge result         | weighted edge list           |

[PUBLIC_TYPE_SCOPE]: error types
- rail: graph-topology

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :------------------------- | :-------------- | :------------------------------- |
|   [1]   | `DAGHasCycle`              | graph error     | cycle detected in DAG            |
|   [2]   | `DAGWouldCycle`            | graph error     | operation would introduce cycle  |
|   [3]   | `FailedToConverge`         | algorithm error | layout/algorithm non-convergence |
|   [4]   | `GraphNotBipartite`        | graph error     | bipartite check failure          |
|   [5]   | `InvalidMapping`           | input error     | invalid mapping argument         |
|   [6]   | `InvalidNode`              | input error     | invalid node index               |
|   [7]   | `JSONDeserializationError` | IO error        | node-link JSON parse failure     |
|   [8]   | `JSONSerializationError`   | IO error        | node-link JSON write failure     |
|   [9]   | `NegativeCycle`            | algorithm error | negative-weight cycle detected   |
|  [10]   | `NoEdgeBetweenNodes`       | graph error     | requested edge absent            |
|  [11]   | `NoPathFound`              | algorithm error | no path between nodes            |
|  [12]   | `NoSuitableNeighbors`      | algorithm error | neighbor selection failure       |
|  [13]   | `NullGraph`                | graph error     | operation on empty graph         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `PyGraph` instance operations
- rail: graph-topology

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------- |
|   [1]   | `add_node(obj)`                                     | mutation       | add node with payload       |
|   [2]   | `add_edge(u, v, obj)`                               | mutation       | add edge with payload       |
|   [3]   | `remove_node(idx)` / `remove_nodes_from(idxs)`      | mutation       | node removal                |
|   [4]   | `remove_edge(u, v)` / `remove_edge_from_index(idx)` | mutation       | edge removal                |
|   [5]   | `get_node_data(idx)` / `get_edge_data(u, v)`        | query          | payload retrieval           |
|   [6]   | `nodes()` / `edges()` / `edge_list()`               | query          | enumeration                 |
|   [7]   | `num_nodes()` / `num_edges()`                       | query          | cardinality                 |
|   [8]   | `adj(idx)` / `neighbors(idx)`                       | query          | adjacency access            |
|   [9]   | `subgraph(nodes)` / `edge_subgraph(edges)`          | derived graph  | induced subgraph            |
|  [10]   | `compose(other, ...)` / `copy()`                    | graph ops      | union / clone               |
|  [11]   | `to_directed()` / `to_dot(...)`                     | conversion     | directed copy / DOT export  |
|  [12]   | `has_edge(u, v)` / `has_node(idx)`                  | predicate      | membership check            |
|  [13]   | `from_adjacency_matrix(mat)`                        | factory        | construct from dense matrix |
|  [14]   | `read_edge_list(path)` / `write_edge_list(path)`    | IO             | edge-list file IO           |

[ENTRYPOINT_SCOPE]: `PyDiGraph` / `PyDAG` additional operations
- rail: graph-topology

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `add_child(parent, obj, edge)` / `add_parent(child, obj, edge)` | mutation       | DAG-safe node add                |
|   [2]   | `predecessors(idx)` / `successors(idx)`                         | query          | directed neighbor access         |
|   [3]   | `in_degree(idx)` / `out_degree(idx)`                            | query          | degree by direction              |
|   [4]   | `check_cycle()` / `can_contract_without_cycle(u, v)`            | predicate      | cycle safety                     |
|   [5]   | `reverse()` / `to_undirected()`                                 | conversion     | edge-direction flip / undirected |
|   [6]   | `merge_nodes(u, v)` / `remove_node_retain_edges(idx)`           | mutation       | node merge / edge inherit        |
|   [7]   | `is_symmetric()` / `make_symmetric()`                           | property       | symmetry query and enforcement   |

[ENTRYPOINT_SCOPE]: shortest-path algorithms
- rail: graph-topology

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]   | [RAIL]                          |
| :-----: | :---------------------------------------------------------------- | :--------------- | :------------------------------ |
|   [1]   | `dijkstra_shortest_paths(graph, source, ...)`                     | weighted SSSP    | Dijkstra single-source          |
|   [2]   | `dijkstra_shortest_path_lengths(graph, node, ...)`                | weighted SSSP    | Dijkstra lengths                |
|   [3]   | `bellman_ford_shortest_paths(graph, source, ...)`                 | weighted SSSP    | Bellman-Ford (negative weights) |
|   [4]   | `astar_shortest_path(graph, node, goal_fn, ...)`                  | heuristic search | A* single-pair                  |
|   [5]   | `k_shortest_path_lengths(graph, start, k, ...)`                   | k-shortest       | Yen's k shortest                |
|   [6]   | `all_pairs_dijkstra_shortest_paths(graph, ...)`                   | all-pairs        | Dijkstra all-pairs paths        |
|   [7]   | `all_pairs_bellman_ford_shortest_paths(graph, ...)`               | all-pairs        | Bellman-Ford all-pairs          |
|   [8]   | `floyd_warshall(graph, ...)` / `floyd_warshall_numpy(graph, ...)` | all-pairs        | Floyd-Warshall matrix           |
|   [9]   | `all_simple_paths(graph, source, target, ...)`                    | path enumeration | all simple paths                |
|  [10]   | `steiner_tree(graph, terminal_nodes, ...)`                        | Steiner tree     | minimum Steiner tree            |

[ENTRYPOINT_SCOPE]: centrality algorithms
- rail: graph-topology

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :------------------------ |
|   [1]   | `betweenness_centrality(graph, ...)`                                                        | centrality     | node betweenness          |
|   [2]   | `edge_betweenness_centrality(graph, ...)`                                                   | centrality     | edge betweenness          |
|   [3]   | `closeness_centrality(graph, ...)`                                                          | centrality     | closeness                 |
|   [4]   | `eigenvector_centrality(graph, ...)`                                                        | centrality     | eigenvector               |
|   [5]   | `katz_centrality(graph, ...)`                                                               | centrality     | Katz                      |
|   [6]   | `degree_centrality(graph)` / `in_degree_centrality(graph)` / `out_degree_centrality(graph)` | centrality     | degree-based              |
|   [7]   | `pagerank(graph, ...)`                                                                      | centrality     | PageRank                  |
|   [8]   | `hits(graph, ...)`                                                                          | centrality     | HITS authority/hub        |
|   [9]   | `newman_weighted_closeness_centrality(graph, ...)`                                          | centrality     | weighted Newman closeness |

[ENTRYPOINT_SCOPE]: connectivity and structure
- rail: graph-topology

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]     | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------- | :----------------- | :------------------------------ |
|   [1]   | `connected_components(graph)`                                              | connectivity       | undirected components           |
|   [2]   | `strongly_connected_components(graph)`                                     | connectivity       | Tarjan SCC                      |
|   [3]   | `weakly_connected_components(graph)`                                       | connectivity       | weakly connected sets           |
|   [4]   | `is_connected(graph)` / `is_strongly_connected(graph)`                     | predicate          | connectivity check              |
|   [5]   | `articulation_points(graph)` / `bridges(graph)`                            | structure          | cut vertices and bridges        |
|   [6]   | `biconnected_components(graph)`                                            | structure          | biconnected component set       |
|   [7]   | `minimum_spanning_tree(graph, ...)` / `minimum_spanning_edges(graph, ...)` | spanning           | MST                             |
|   [8]   | `is_bipartite(graph)` / `two_color(graph)`                                 | predicate/coloring | bipartite test and 2-coloring   |
|   [9]   | `is_planar(graph)`                                                         | predicate          | planarity test                  |
|  [10]   | `stoer_wagner_min_cut(graph, ...)`                                         | min-cut            | minimum cut value and partition |
|  [11]   | `transitive_reduction(graph)`                                              | DAG reduction      | DAG transitive reduction        |
|  [12]   | `dag_longest_path(graph)` / `dag_longest_path_length(graph)`               | DAG                | longest path in DAG             |
|  [13]   | `topological_sort(graph)` / `topological_generations(graph)`               | DAG                | topological order               |

[ENTRYPOINT_SCOPE]: traversal, layout, and generation
- rail: graph-topology

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------- |
|   [1]   | `bfs_search(graph, source, visitor)` / `dfs_search(graph, source, visitor)`            | traversal      | callback-driven BFS/DFS     |
|   [2]   | `bfs_successors(graph, node)` / `bfs_predecessors(graph, node)`                        | traversal      | BFS neighbor lists          |
|   [3]   | `spring_layout(graph, ...)` / `kamada_kawai_layout(graph, ...)`                        | layout         | force-directed / KK layout  |
|   [4]   | `circular_layout(graph)` / `shell_layout(graph, ...)` / `bipartite_layout(graph, ...)` | layout         | geometric layout            |
|   [5]   | `random_layout(graph, ...)` / `spiral_layout(graph, ...)`                              | layout         | random and spiral placement |
|   [6]   | `barabasi_albert_graph(n, m, ...)` / `directed_barabasi_albert_graph(n, m, ...)`       | generation     | scale-free graph            |
|   [7]   | `undirected_gnp_random_graph(n, p, ...)` / `directed_gnp_random_graph(n, p, ...)`      | generation     | Erdős-Rényi                 |
|   [8]   | `undirected_gnm_random_graph(n, m, ...)` / `directed_gnm_random_graph(n, m, ...)`      | generation     | fixed-edge random           |
|   [9]   | `random_regular_graph(d, n, ...)` / `hyperbolic_random_graph(n, ...)`                  | generation     | regular and hyperbolic      |
|  [10]   | `is_isomorphic(first, second, ...)` / `vf2_mapping(first, second, ...)`                | isomorphism    | VF2 isomorphism             |
|  [11]   | `read_graphml(path)` / `write_graphml(graph, path)`                                    | IO             | GraphML read/write          |
|  [12]   | `node_link_json(graph)` / `parse_node_link_json(json)`                                 | IO             | node-link JSON round-trip   |

## [4]-[IMPLEMENTATION_LAW]

[GRAPH_TOPOLOGY]:
- namespace: `rustworkx` (top-level); no required submodule imports
- `PyGraph`: undirected, supports parallel edges when `multigraph=True`; node and edge payloads are arbitrary Python objects
- `PyDiGraph`: directed, same payload model; `PyDAG` aliases `PyDiGraph` with cycle-checking enabled by default
- node and edge indices are stable non-negative integers; removal leaves gaps (no compaction without explicit rebuild)
- algorithm functions are top-level callables; `digraph_*` and `graph_*` prefixed variants are the concrete type-specific forms; unprefixed names dispatch based on graph type
- `ColoringStrategy` enum governs greedy graph coloring strategy selection
- `GraphMLKey`, `GraphMLType`, `GraphMLDomain` support GraphML schema metadata during IO

[LOCAL_ADMISSION]:
- Graph containers persist as `PyGraph` or `PyDiGraph` values; algorithm results are unwrapped from mapping/index containers before storage.
- Layout results emit `Pos2DMapping` (node index → `(x, y)` float tuple); consume as dict before serialization.
- `networkx_converter(graph)` converts to a NetworkX graph when NetworkX is installed; treat as interop bridge only.
- Traversal functions accept a visitor object with optional `discover_vertex`, `examine_edge`, `tree_edge`, `finish_vertex` methods.

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: graph container lifecycle, structural graph algorithms, layout, and graph IO
- Accept: `PyGraph` / `PyDiGraph` / `PyDAG` as the canonical graph containers
- Reject: hand-rolled adjacency list representations when `rustworkx` containers own the concern
