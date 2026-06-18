# [PY_DATA_API_RUSTWORKX]

`rustworkx` supplies `PyGraph`, `PyDiGraph`, and `PyDAG` graph classes backed by a Rust core, with shortest-path, all-pairs, DAG, connectivity, traversal, centrality, matching, cut, isomorphism, layout, IO, and generator algorithms. Every major algorithm exposes a polymorphic dispatch form (bare name) alongside graph-typed forms (`graph_*`, `digraph_*`); the bare form dispatches on graph type and is the preferred surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rustworkx`
- package: `rustworkx`
- import: `import rustworkx`
- owner: `data`
- rail: graph
- capability: undirected and directed graph containers, full algorithm suite, GraphML/DOT/JSON/Matrix-Market IO, NetworkX conversion, and a `generators` submodule

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers
- rail: graph
- payloads are arbitrary Python objects; integer indices are stable and never recycled

| [INDEX] | [SYMBOL]              | [GRAPH_KIND]     | [ADDED_MEMBERS]                                                                    |
| :-----: | :-------------------- | :--------------- | :--------------------------------------------------------------------------------- |
|   [1]   | `rustworkx.PyGraph`   | undirected       | `multigraph` gate on parallel edges                                                |
|   [2]   | `rustworkx.PyDiGraph` | directed         | `in_degree`, `out_degree`, `predecessors`, `successors`, `reverse`, `is_symmetric` |
|   [3]   | `rustworkx.PyDAG`     | directed acyclic | `check_cycle` guard plus the full `PyDiGraph` surface                              |

[PUBLIC_TYPE_SCOPE]: shared graph members
- rail: graph

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                                                 |
| :-----: | :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | mutation        | `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `remove_node`, `remove_edge`, `contract_nodes`, `merge_nodes`, `clear`        |
|   [2]   | query           | `get_node_data`, `get_edge_data`, `has_node`, `has_edge`, `num_nodes`, `num_edges`, `node_indices`, `edge_indices`, `neighbors`, `degree` |
|   [3]   | derivation      | `subgraph`, `subgraph_with_nodemap`, `edge_subgraph`, `copy`, `filter_nodes`, `filter_edges`, `compose`, `substitute_node_with_subgraph`  |
|   [4]   | IO/export       | `to_dot`, `read_edge_list`, `write_edge_list`, `extend_from_edge_list`, `extend_from_weighted_edge_list`                                  |
|   [5]   | index views     | `edge_index_map`, `node_indices`, `edge_indices`, `edge_list`, `weighted_edge_list`                                                       |

[PUBLIC_TYPE_SCOPE]: result containers and exceptions
- rail: graph

| [INDEX] | [SYMBOL]                                           | [KIND]    | [PRODUCED_BY]                    |
| :-----: | :------------------------------------------------- | :-------- | :------------------------------- |
|   [1]   | `NodeIndices`, `EdgeIndices`                       | result    | node/edge index queries          |
|   [2]   | `EdgeList`, `WeightedEdgeList`, `EdgeIndexMap`     | result    | edge enumeration                 |
|   [3]   | `PathMapping`, `PathLengthMapping`                 | result    | single-source shortest path      |
|   [4]   | `AllPairsPathMapping`, `AllPairsPathLengthMapping` | result    | all-pairs shortest path          |
|   [5]   | `CentralityMapping`, `EdgeCentralityMapping`       | result    | centrality scores                |
|   [6]   | `BFSSuccessors`, `BFSPredecessors`                 | result    | BFS traversal                    |
|   [7]   | `NodeMap`, `BiconnectedComponents`, `Pos2DMapping` | result    | matching, biconnectivity, layout |
|   [8]   | `DAGHasCycle`, `DAGWouldCycle`                     | exception | `PyDAG` cycle violation          |
|   [9]   | `NoPathFound`, `NegativeCycle`                     | exception | path search failure              |
|  [10]   | `NoEdgeBetweenNodes`, `InvalidNode`                | exception | invalid index access             |
|  [11]   | `GraphNotBipartite`, `FailedToConverge`            | exception | algorithm precondition           |
|  [12]   | `NullGraph`, `NoSuitableNeighbors`                 | exception | empty-graph or layout failure    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: path and DAG algorithms
- rail: graph
- bare names dispatch on graph type; `graph_*`/`digraph_*` forms bind a single graph type

| [INDEX] | [FAMILY]      | [ENTRY]                                                                                   | [RESULT]               |
| :-----: | :------------ | :---------------------------------------------------------------------------------------- | :--------------------- |
|   [1]   | shortest path | `dijkstra_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0)` | `PathMapping`          |
|   [2]   | shortest path | `bellman_ford_shortest_paths(graph, source, target=None, weight_fn=None)`                 | `PathMapping`          |
|   [3]   | shortest path | `astar_shortest_path(graph, node, goal_fn, edge_cost_fn, estimate_cost_fn)`               | node path              |
|   [4]   | shortest path | `k_shortest_path_lengths(graph, start, k, edge_cost, goal=None)`                          | `PathLengthMapping`    |
|   [5]   | all pairs     | `all_pairs_dijkstra_shortest_paths(graph, edge_cost_fn)`                                  | `AllPairsPathMapping`  |
|   [6]   | all pairs     | `floyd_warshall(graph, weight_fn=None, default_weight=1.0, parallel_threshold=300)`       | distance map           |
|   [7]   | all pairs     | `floyd_warshall_numpy(graph, weight_fn=None, ...)`                                        | NumPy matrix           |
|   [8]   | DAG           | `topological_sort(graph)`                                                                 | `NodeIndices`          |
|   [9]   | DAG           | `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None)`           | node list              |
|  [10]   | DAG           | `dag_longest_path(graph, /, weight_fn=None)`                                              | node path              |
|  [11]   | DAG           | `is_directed_acyclic_graph(graph)`                                                        | bool                   |
|  [12]   | DAG           | `transitive_reduction(graph)`, `immediate_dominators(graph, start_node)`                  | graph or dominator map |

[ENTRYPOINT_SCOPE]: connectivity, traversal, and centrality
- rail: graph

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                  | [RESULT]                |
| :-----: | :----------- | :--------------------------------------------------------------------------------------- | :---------------------- |
|   [1]   | connectivity | `connected_components(graph)`, `strongly_connected_components(graph)`                    | list of sets            |
|   [2]   | connectivity | `number_connected_components(graph)`, `is_connected(graph)`                              | int or bool             |
|   [3]   | connectivity | `node_connected_component(graph, node)`, `connected_subgraphs(graph, k)`                 | set or subgraphs        |
|   [4]   | traversal    | `bfs_successors(graph, node)`, `bfs_predecessors(graph, node)`                           | `BFSSuccessors`         |
|   [5]   | traversal    | `dfs_edges(graph, source=None)`, `bfs_layers(graph, sources=None)`                       | `EdgeList` or layers    |
|   [6]   | traversal    | `ancestors(graph, node)`, `descendants(graph, node)`                                     | set                     |
|   [7]   | search       | `bfs_search(graph, source, visitor)`, `dfs_search(graph, source, visitor)`               | visitor-driven          |
|   [8]   | centrality   | `betweenness_centrality(graph, normalized=True, endpoints=False, parallel_threshold=50)` | `CentralityMapping`     |
|   [9]   | centrality   | `closeness_centrality(graph, ...)`, `eigenvector_centrality(graph, ...)`                 | `CentralityMapping`     |
|  [10]   | centrality   | `pagerank(graph, /, alpha=0.85, weight_fn=None, max_iter=100, tol=1e-06)`                | `CentralityMapping`     |
|  [11]   | centrality   | `katz_centrality(graph, alpha=0.1, beta=1.0, ...)`, `degree_centrality(graph)`           | `CentralityMapping`     |
|  [12]   | centrality   | `edge_betweenness_centrality(graph, normalized=True, parallel_threshold=50)`             | `EdgeCentralityMapping` |

[ENTRYPOINT_SCOPE]: matching, structure, isomorphism, layout, IO, and generators
- rail: graph

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                                                           | [RESULT]           |
| :-----: | :----------- | :-------------------------------------------------------------------------------------------------------------------------------- | :----------------- |
|   [1]   | matching     | `max_weight_matching(graph, /, max_cardinality=False, weight_fn=None)`                                                            | set of edges       |
|   [2]   | cycle        | `simple_cycles(graph)`, `find_negative_cycle(graph, edge_cost_fn)`                                                                | cycle list or path |
|   [3]   | cut/spanning | `stoer_wagner_min_cut(graph, /, weight_fn=None)`, `minimum_spanning_tree(graph, weight_fn=None)`                                  | cut or tree        |
|   [4]   | cut/spanning | `steiner_tree(graph, terminal_nodes, weight_fn)`, `metric_closure(graph, weight_fn)`                                              | tree or graph      |
|   [5]   | structure    | `is_planar(graph)`, `is_bipartite(graph)`, `two_color(graph)`                                                                     | bool or `NodeMap`  |
|   [6]   | structure    | `biconnected_components(graph)`, `articulation_points(graph)`, `bridges(graph)`                                                   | components or sets |
|   [7]   | isomorphism  | `is_isomorphic(first, second, node_matcher=None, edge_matcher=None, id_order=True)`                                               | bool               |
|   [8]   | isomorphism  | `vf2_mapping(first, second, ...)`, `is_subgraph_isomorphic(first, second, ...)`                                                   | iterator or bool   |
|   [9]   | layout       | `spring_layout(graph, ...)`, `circular_layout(graph)`, `kamada_kawai_layout(graph)`                                               | `Pos2DMapping`     |
|  [10]   | IO           | `node_link_json(graph, path=None, ...)`, `parse_node_link_json(data, ...)`, `from_dot(dot_str)`                                   | JSON or graph      |
|  [11]   | IO           | `read_graphml(path)`, `write_graphml(graph, path)`, `read_matrix_market(contents)`, `write_matrix_market(graph)`                  | graph or file      |
|  [12]   | conversion   | `networkx_converter(graph, keep_attributes=False)`, `adjacency_matrix(graph, weight_fn=None)`                                     | graph or matrix    |
|  [13]   | generators   | `undirected_gnp_random_graph(num_nodes, probability)`, `barabasi_albert_graph(n, m)`, `random_geometric_graph(num_nodes, radius)` | graph              |
|  [14]   | generators   | `rustworkx.generators.complete_graph`, `.cycle_graph`, `.grid_graph`, `.path_graph`, `.star_graph`, `.full_rary_tree`             | graph              |

## [4]-[IMPLEMENTATION_LAW]

[DISPATCH_TOPOLOGY]:
- Graph-typed forms (`graph_*`, `digraph_*`) and the polymorphic bare-name forms both exist; prefer the bare name and let rustworkx dispatch on graph type.
- Node and edge payloads are arbitrary Python objects; integer index stability with no recycling after removal makes `NodeIndices`/`EdgeIndices` valid stable keys.
- `PyDAG.check_cycle=True` enforces the DAG invariant at mutation time; code that needs the DAG guarantee sets the flag at construction.
- `networkx_converter` is the one-way NetworkX-to-rustworkx bridge at the boundary; graph construction is never re-implemented locally.
- Layout functions return `Pos2DMapping` (node index to `(x, y)`); they do not render and feed a visualisation library.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: graph containers, full algorithm suite, GraphML/DOT/JSON/Matrix-Market IO, NetworkX conversion, generator submodule
- Accept: polymorphic dispatch forms over graph-typed dispatch, `networkx_converter` at the NetworkX boundary, `generators` submodule for synthetic graphs
- Reject: per-graph-type parallel entrypoints when the polymorphic form covers both, manual BFS/DFS loop reimplementation, and re-exporting rustworkx types through thin wrappers
