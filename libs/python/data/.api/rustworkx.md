# [PY_DATA_API_RUSTWORKX]

`rustworkx` supplies the Rust-core graph surface for the data graph rail: `PyGraph`/`PyDiGraph`/`PyDAG` containers with stable non-recycled integer indices, plus the full path/DAG/connectivity/traversal/centrality/coloring/matching/cut/isomorphism/bisimulation/layout/IO/generator algorithm suite. Every major algorithm exposes a polymorphic bare-name form that dispatches on graph type alongside `graph_*`/`digraph_*` type-bound forms; the data graph owner routes through the bare name, reads the typed result carrier (`PathMapping`/`CentralityMapping`/`NodeMap`/`Pos2DMapping`) as the receipt, drives event-stepped search through the `visit.*` visitor base classes, and never re-implements a BFS/DFS/Dijkstra loop the Rust core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rustworkx`
- package: `rustworkx`
- version: `0.18.0`
- license: Apache-2.0 (permissive; safe to link into a host-distributed plugin, unlike the GPL `igraph` sibling)
- module: `import rustworkx as rx`
- owner: `data`
- rail: graph
- asset: native Rust extension (`rustworkx.rustworkx` PyO3 core) shipped as an `abi3` wheel built against the CPython 3.10 stable ABI — one wheel covers cp310 through cp315, so the cp315 project core resolves the same artifact without a per-version rebuild
- requires-python: `>=3.10`
- entry points: import-only; no console script
- capability: undirected/directed/DAG containers with stable integer indices and arbitrary-object payloads; Dijkstra/Bellman-Ford/A*/Floyd-Warshall/k-shortest/all-pairs/all-simple-path shortest-path family; topological sort and generations, longest-path, transitive reduction, dominators and dominance frontiers; connected/strongly/weakly-connected components, condensation, core-number, articulation points, bridges, chain/cycle-basis decomposition, Stoer-Wagner min-cut; BFS/DFS/Dijkstra event-stepped search via `visit.*` visitors; betweenness/closeness/eigenvector/Katz/PageRank/HITS/degree and group centrality; greedy/edge/Misra-Gries/bipartite vertex-and-edge coloring; max-weight matching; minimum spanning tree/edges, Steiner tree, metric closure; VF2 (sub)graph isomorphism and maximum bisimulation; spring/circular/shell/spiral/bipartite/Kamada-Kawai layouts; GraphML/DOT/node-link-JSON/Matrix-Market IO and adjacency-matrix export; NetworkX one-way conversion; `generators` synthetic-graph submodule; `TopologicalSorter` incremental graphlib-style sorter

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers
- rail: graph
- node/edge payloads are arbitrary Python objects; integer indices are stable and never recycled after removal, so `NodeIndices`/`EdgeIndices` are valid stable keys into sibling tables

| [INDEX] | [SYMBOL]              | [GRAPH_KIND]     | [ADDED_MEMBERS_OVER_SHARED]                                                                                                                                                              |
| :-----: | :-------------------- | :--------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `rustworkx.PyGraph`   | undirected       | `multigraph` parallel-edge gate, `degree`, `to_directed`                                                                                                                               |
|  [02]   | `rustworkx.PyDiGraph` | directed         | `in_degree`/`out_degree`, `predecessors`/`successors`/`predecessor_indices`/`successor_indices`, `reverse`/`make_symmetric`/`is_symmetric`, `merge_nodes`, `can_contract_without_cycle`, `add_child`/`add_parent`, `remove_node_retain_edges`/`_by_id`/`_by_key`, `insert_node_on_in_edges`/`_out_edges`(`_multiple`), `find_successor_node_by_edge`/`find_predecessor_node_by_edge`, `to_undirected` |
|  [03]   | `rustworkx.PyDAG`     | directed acyclic | alias of `PyDiGraph` with `check_cycle=True` set; the full `PyDiGraph` surface plus mutation-time acyclicity enforcement                                                               |

[PUBLIC_TYPE_SCOPE]: shared container members (`PyGraph` and `PyDiGraph`)
- rail: graph

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                                                                                                                          |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | mutation        | `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `add_edges_from_no_data`, `remove_node`, `remove_nodes_from`, `remove_edge`, `remove_edges_from`, `remove_edge_from_index`, `contract_nodes`, `update_edge`, `update_edge_by_index`, `clear`, `clear_edges` |
|  [02]   | query           | `get_node_data`, `get_edge_data`, `get_edge_data_by_index`, `get_all_edge_data`, `get_edge_endpoints_by_index`, `has_node`, `has_edge`, `has_parallel_edges`, `num_nodes`, `num_edges`, `node_indices`, `edge_indices`, `neighbors`, `degree`, `find_node_by_weight`, `nodes`, `edges`, `attrs` (graph-level attribute payload) |
|  [03]   | edge views      | `edge_list`, `weighted_edge_list`, `edge_index_map`, `incident_edges`, `incident_edge_index_map`, `in_edges`/`out_edges`, `in_edge_indices`/`out_edge_indices`, `edge_indices_from_endpoints`, `adj`              |
|  [04]   | derivation      | `subgraph`, `subgraph_with_nodemap`, `edge_subgraph`, `copy`, `filter_nodes`, `filter_edges`, `compose`, `substitute_node_with_subgraph`                                                                          |
|  [05]   | IO/adjacency    | `to_dot`, `read_edge_list`, `write_edge_list`, `extend_from_edge_list`, `extend_from_weighted_edge_list`, `from_adjacency_matrix`, `from_complex_adjacency_matrix`                                                |

[PUBLIC_TYPE_SCOPE]: search visitors, incremental sorter, and coloring strategy
- rail: graph
- the `visit.*` visitors are subclassed; the search drives `discover_vertex`/`tree_edge`/`finish_vertex` (and Dijkstra `examine_edge`/`edge_relaxed`) callbacks; raising `visit.PruneSearch` skips a subtree and `visit.StopSearch` halts the whole traversal

| [INDEX] | [SYMBOL]                            | [KIND]              | [CONSUMER_NOTE]                                                                                          |
| :-----: | :---------------------------------- | :------------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `rustworkx.visit.BFSVisitor`        | visitor base        | subclass for `bfs_search`; override discover/examine callbacks for event-stepped BFS                    |
|  [02]   | `rustworkx.visit.DFSVisitor`        | visitor base        | subclass for `dfs_search`; tree/back/forward edge classification callbacks                              |
|  [03]   | `rustworkx.visit.DijkstraVisitor`   | visitor base        | subclass for `dijkstra_search`; `edge_relaxed`/`examine_edge` weighted-frontier callbacks               |
|  [04]   | `rustworkx.visit.PruneSearch`       | control exception   | raise inside a visitor callback to prune the current subtree without halting                            |
|  [05]   | `rustworkx.visit.StopSearch`        | control exception   | raise inside a visitor callback to terminate the entire search early                                    |
|  [06]   | `rustworkx.TopologicalSorter`       | incremental sorter  | `graphlib`-shaped streaming topo sort over a `PyDiGraph`: `get_ready`/`done`/`is_active` ready-set loop |
|  [07]   | `rustworkx.ColoringStrategy`        | enum                | `Degree`/`IndependentSet`/`Saturation` (DSATUR) ordering strategies for the greedy-color family         |

[PUBLIC_TYPE_SCOPE]: result containers and exceptions
- rail: graph
- result carriers are mapping/sequence-like views over node/edge indices; they iterate and index without a Python re-materialization step

| [INDEX] | [SYMBOL]                                                                       | [KIND]    | [PRODUCED_BY]                                  |
| :-----: | :----------------------------------------------------------------------------- | :-------- | :--------------------------------------------- |
|  [01]   | `NodeIndices`, `EdgeIndices`, `NodeMap`, `NodesCountMapping`                   | result    | index/count queries and structural maps        |
|  [02]   | `EdgeList`, `WeightedEdgeList`, `EdgeIndexMap`                                 | result    | edge enumeration                               |
|  [03]   | `PathMapping`, `PathLengthMapping`, `MultiplePathMapping`                      | result    | single-source shortest path / all-simple-paths |
|  [04]   | `AllPairsPathMapping`, `AllPairsPathLengthMapping`, `AllPairsMultiplePathMapping` | result | all-pairs shortest path / all-pairs simple paths |
|  [05]   | `CentralityMapping`, `EdgeCentralityMapping`                                   | result    | centrality scores                              |
|  [06]   | `BFSSuccessors`, `BFSPredecessors`, `Chains`                                   | result    | BFS traversal / chain decomposition            |
|  [07]   | `BiconnectedComponents`, `Pos2DMapping`, `ProductNodeMap`                      | result    | biconnectivity, layout, graph-product node map |
|  [08]   | `IndexPartitionBlock`, `RelationalCoarsestPartition`                           | result    | maximum-bisimulation coarsest partition        |
|  [09]   | `GraphMLDomain`, `GraphMLKey`, `GraphMLType`                                   | enum      | GraphML read/write key typing                  |
|  [10]   | `DAGHasCycle`, `DAGWouldCycle`                                                 | exception | `PyDAG`/`check_cycle` violation at mutation     |
|  [11]   | `NoPathFound`, `NegativeCycle`                                                 | exception | path search failure                            |
|  [12]   | `NoEdgeBetweenNodes`, `InvalidNode`, `InvalidMapping`                          | exception | invalid index / token-swapper input            |
|  [13]   | `GraphNotBipartite`, `FailedToConverge`, `NoSuitableNeighbors`, `NullGraph`    | exception | algorithm precondition / empty-graph failure   |
|  [14]   | `JSONSerializationError`, `JSONDeserializationError`                           | exception | node-link-JSON IO failure                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shortest-path, simple-path, and DAG algorithms
- rail: graph
- bare names dispatch on graph type; `graph_*`/`digraph_*` forms bind one type; weight is a `weight_fn` callback over the edge payload, not a stored attribute

| [INDEX] | [FAMILY]      | [ENTRY]                                                                                                                | [RESULT]                  |
| :-----: | :------------ | :--------------------------------------------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | shortest path | `dijkstra_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)`         | `PathMapping`             |
|  [02]   | shortest path | `bellman_ford_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)`     | `PathMapping`             |
|  [03]   | shortest path | `astar_shortest_path(graph, node, goal_fn, edge_cost_fn, estimate_cost_fn)`                                            | `NodeIndices`             |
|  [04]   | shortest path | `all_shortest_paths(graph, source, target, weight_fn=None, ...)`, `single_source_all_shortest_paths(graph, source, ...)` | path list / mapping     |
|  [05]   | shortest path | `k_shortest_path_lengths(graph, start, k, edge_cost, goal=None)`, `has_path(graph, source, target, as_undirected=False)` | `PathLengthMapping` / bool |
|  [06]   | shortest path | `num_shortest_paths_unweighted(graph, source)`, `unweighted_average_shortest_path_length(graph, parallel_threshold=300, disconnected=False)` | counts / float |
|  [07]   | all pairs     | `all_pairs_dijkstra_shortest_paths(graph, edge_cost_fn)`, `all_pairs_bellman_ford_shortest_paths(graph, edge_cost_fn)` | `AllPairsPathMapping`     |
|  [08]   | all pairs     | `floyd_warshall(graph, weight_fn=None, default_weight=1.0, parallel_threshold=300)`, `floyd_warshall_numpy(graph, ...)`, `floyd_warshall_successor_and_distance(graph, ...)` | distance map / NumPy matrix |
|  [09]   | all pairs     | `distance_matrix(graph, parallel_threshold=300, as_undirected=False, null_value=0.0)`                                  | NumPy matrix              |
|  [10]   | simple paths  | `all_simple_paths(graph, from_, to, min_depth=None, cutoff=None)`, `longest_simple_path(graph)`, `all_pairs_all_simple_paths(graph, ...)` | path list / `MultiplePathMapping` |
|  [11]   | DAG           | `topological_sort(graph)`, `topological_generations(graph)`, `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None)` | `NodeIndices` / generations / node list |
|  [12]   | DAG           | `dag_longest_path(graph, /, weight_fn=None)`, `dag_weighted_longest_path(graph, weight_fn)`, `dag_longest_path_length(graph, ...)`, `is_directed_acyclic_graph(graph)` | node path / length / bool |
|  [13]   | DAG           | `transitive_reduction(graph)`, `immediate_dominators(graph, start_node)`, `dominance_frontiers(graph, start_node)`     | graph / dominator map / frontier map |

[ENTRYPOINT_SCOPE]: connectivity, traversal, search, and centrality
- rail: graph

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                                                  | [RESULT]                |
| :-----: | :----------- | :----------------------------------------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | connectivity | `connected_components(graph)`, `strongly_connected_components(graph)`, `weakly_connected_components(graph)`               | list of sets            |
|  [02]   | connectivity | `number_connected_components`, `number_strongly_connected_components`, `number_weakly_connected_components`, `is_connected`, `is_strongly_connected`, `is_weakly_connected`, `is_semi_connected` | int or bool |
|  [03]   | connectivity | `node_connected_component(graph, node)`, `connected_subgraphs(graph, k)`, `condensation(graph, sccs=None)`, `core_number(graph)` | set / subgraphs / graph / `NodesCountMapping` |
|  [04]   | structure    | `articulation_points(graph)`, `bridges(graph)`, `biconnected_components(graph)`, `chain_decomposition(graph, source=None)`, `cycle_basis(graph, root=None)` | sets / `BiconnectedComponents` / `Chains` |
|  [05]   | cut          | `stoer_wagner_min_cut(graph, /, weight_fn=None)`, `simple_cycles(graph)`, `digraph_find_cycle(graph, source=None)`, `find_negative_cycle(graph, edge_cost_fn)`, `negative_edge_cycle(graph, edge_cost_fn)` | cut / cycle list / path / bool |
|  [06]   | traversal    | `bfs_successors(graph, node)`, `bfs_predecessors(graph, node)`, `bfs_layers(graph, sources)`, `dfs_edges(graph, source=None)`, `ancestors(graph, node)`, `descendants(graph, node)`, `isolates(graph)`, `collect_runs(graph, filter_fn)`, `collect_bicolor_runs(graph, filter_fn, color_fn)` | result carriers / sets / runs |
|  [07]   | search       | `bfs_search(graph, source, visitor)`, `dfs_search(graph, source, visitor)`, `dijkstra_search(graph, source, weight_fn, visitor)` | visitor-driven (None)   |
|  [08]   | centrality   | `betweenness_centrality(graph, normalized=True, endpoints=False, parallel_threshold=50)`, `edge_betweenness_centrality(graph, normalized=True, parallel_threshold=50)` | `CentralityMapping` / `EdgeCentralityMapping` |
|  [09]   | centrality   | `closeness_centrality(graph, wf_improved=True)`, `newman_weighted_closeness_centrality(graph, weight_fn, ...)`, `degree_centrality(graph)`, `in_degree_centrality`/`out_degree_centrality` | `CentralityMapping` |
|  [10]   | centrality   | `eigenvector_centrality(graph, weight_fn=None, default_weight=1.0, max_iter=100, tol=1e-06)`, `katz_centrality(graph, alpha=0.1, beta=1.0, ...)` | `CentralityMapping` |
|  [11]   | centrality   | `pagerank(graph, /, alpha=0.85, weight_fn=None, nstart=None, personalization=None, tol=1e-06, max_iter=100, dangling=None)`, `hits(graph, /, weight_fn=None, nstart=None, tol=1e-08, max_iter=100, normalized=True)` | `CentralityMapping` / (hubs, authorities) |
|  [12]   | centrality   | `group_betweenness_centrality(graph, sources, ...)`, `group_closeness_centrality(graph, sources, ...)`, `group_degree_centrality(graph, sources)` | float (group score) |

[ENTRYPOINT_SCOPE]: coloring, matching, spanning, isomorphism, bisimulation, structure, layout, IO, conversion, generators
- rail: graph

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                                                        | [RESULT]                  |
| :-----: | :----------- | :----------------------------------------------------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | coloring     | `graph_greedy_color(graph, /, preset_color_fn=None, strategy=ColoringStrategy.Degree)`, `graph_greedy_edge_color(graph, ...)`, `graph_misra_gries_edge_color(graph)`, `graph_bipartite_edge_color(graph)`, `two_color(graph)` | node/edge color `dict` / `NodeMap` |
|  [02]   | matching     | `max_weight_matching(graph, /, max_cardinality=False, weight_fn=None, default_weight=1, verify_optimum=False)`, `is_matching(graph, matching)`, `is_maximal_matching(graph, matching)` | set of edges / bool |
|  [03]   | spanning     | `minimum_spanning_tree(graph, weight_fn=None, default_weight=1.0)`, `minimum_spanning_edges(graph, ...)`, `steiner_tree(graph, terminal_nodes, weight_fn, /)`, `metric_closure(graph, weight_fn)` | tree / edge list / tree / graph |
|  [04]   | isomorphism  | `is_isomorphic(first, second, node_matcher=None, edge_matcher=None, id_order=True, call_limit=None)`, `is_isomorphic_node_match(first, second, matcher, id_order=True)`, `is_subgraph_isomorphic(first, second, ..., induced=True)`, `vf2_mapping(first, second, ..., subgraph=False, mapping_parameters=None)` | bool / iterator |
|  [05]   | bisimulation | `digraph_maximum_bisimulation(graph)`                                                                                          | `RelationalCoarsestPartition` |
|  [06]   | structure    | `is_planar(graph)`, `is_bipartite(graph)`, `transitivity(graph)`, `complement(graph)`, `local_complement(graph, node)`, `line_graph(graph)`, `union(first, second, merge_nodes=False, merge_edges=False)`, `cartesian_product(first, second)`, `tensor_product(first, second)`, `graph_token_swapper(graph, mapping, /, trials=None, seed=None, parallel_threshold=50)` | bool / float / graph / (graph, `ProductNodeMap`) / swap list |
|  [07]   | routing      | `hyperbolic_greedy_routing(graph, ...)`, `hyperbolic_greedy_success_rate(graph, ...)`                                          | path / float              |
|  [08]   | layout       | `spring_layout(graph, ...)`, `circular_layout(graph, scale=1, center=None)`, `shell_layout(graph, ...)`, `spiral_layout(graph, ...)`, `bipartite_layout(graph, first_nodes, ...)`, `random_layout(graph, center=None, seed=None)`, `kamada_kawai_layout(graph, ...)` | `Pos2DMapping`            |
|  [09]   | IO           | `node_link_json(graph, path=None, graph_attrs=None, node_attrs=None, edge_attrs=None)`, `parse_node_link_json(data, ...)`, `from_node_link_json_file(path, ...)`, `from_dot(dot_str)` | JSON str / graph          |
|  [10]   | IO           | `read_graphml(path)`, `write_graphml(graph, path)`, `read_matrix_market(...)`, `read_matrix_market_file(path)`, `write_matrix_market(graph, ...)`, `adjacency_matrix(graph, weight_fn=None, default_weight=1.0, null_value=0.0)` | graph / file / NumPy matrix |
|  [11]   | conversion   | `networkx_converter(graph, keep_attributes=False)`                                                                            | `PyGraph`/`PyDiGraph`     |
|  [12]   | generators   | `undirected_gnp_random_graph(num_nodes, probability, seed=None)`, `directed_gnp_random_graph`, `undirected_gnm_random_graph`, `directed_gnm_random_graph`, `random_geometric_graph(num_nodes, radius, ...)`, `barabasi_albert_graph(n, m, seed=None, initial_graph=None)`, `directed_barabasi_albert_graph`, `random_regular_graph(d, n, seed=None)`, `undirected_sbm_random_graph`/`directed_sbm_random_graph`, `hyperbolic_random_graph`, `generate_random_path` | graph |
|  [13]   | generators   | `rustworkx.generators.{complete_graph, cycle_graph, grid_graph, path_graph, star_graph, mesh_graph, full_rary_tree, binomial_tree_graph, barbell_graph, lollipop_graph, hexagonal_lattice_graph, heavy_hex_graph, heavy_square_graph, generalized_petersen_graph, dorogovtsev_goltsev_mendes_graph, karate_club_graph}` (plus `directed_*` mirrors and `empty_graph`/`directed_empty_graph`) | graph |

## [04]-[IMPLEMENTATION_LAW]

[DISPATCH_TOPOLOGY]:
- Graph-typed forms (`graph_*`, `digraph_*`) and the polymorphic bare-name forms both exist; the bare name dispatches on graph type — route through the bare name and bind a type-specific form only inside a single-type kernel.
- Node and edge payloads are arbitrary Python objects; integer index stability with no recycling after removal makes `NodeIndices`/`EdgeIndices` durable join keys into sibling tables and the data-rail node table.
- `PyDAG` is `PyDiGraph` with `check_cycle=True`; a DAG-typed owner constructs the digraph with the flag and treats `DAGWouldCycle` as the typed rejection at mutation time, never a post-hoc cycle scan.
- Event-stepped traversal is the `visit.*` visitor protocol: subclass `BFSVisitor`/`DFSVisitor`/`DijkstraVisitor`, override the discover/examine/finish callbacks, and raise `PruneSearch`/`StopSearch` for early termination — never re-implement the frontier loop in Python.
- `TopologicalSorter` is the incremental `graphlib`-shaped ready-set sorter over a `PyDiGraph` (`get_ready`/`done`/`is_active`); the static one-shot order is `topological_sort`/`topological_generations`. Pick the incremental sorter when staged execution releases nodes as dependencies complete.
- Greedy coloring is parameterized by the `ColoringStrategy` enum (`Saturation` is DSATUR); a `preset_color_fn` pins specific nodes — never hand-roll a saturation-degree ordering.
- Layout functions return `Pos2DMapping` (node index to `(x, y)`); they compute coordinates only and feed a downstream visualization carrier, never render.

[SIBLING_STACK]:
- `networkx_converter` is the one-way NetworkX-to-rustworkx bridge at the boundary; the `networkx` sibling stays the read-side interop surface and rustworkx owns the hot algorithm path. Graph construction is never re-implemented locally on either side of the bridge.
- `igraph` (GPL C core) owns Leiden/Louvain/Infomap community detection that rustworkx lacks; rustworkx owns the permissive-license path/centrality/structure suite. Route GRAPH_COMMUNITY to `igraph`, GRAPH_PATH/GRAPH_CENTRALITY/GRAPH_STRUCTURE to rustworkx, and keep the GPL dependency off the host-distributed plugin.
- `floyd_warshall_numpy`/`distance_matrix`/`adjacency_matrix` emit dense NumPy matrices that fold directly into the `pyarrow`/`numpy` tensor carriers; `node_link_json` and GraphML/Matrix-Market IO are the durable serialization seam for the data graph table.
- `betweenness_centrality`/`pagerank`/`core_number`/`CentralityMapping` keyed by stable node index join back to the data-rail node table by the same index, so a centrality run is a left-join enrichment, not a re-keyed copy.

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: undirected/directed/DAG containers with stable integer indices; the full path/DAG/connectivity/traversal/search/centrality/coloring/matching/cut/isomorphism/bisimulation/layout/IO/generator algorithm suite; `visit.*` visitor-driven search; `TopologicalSorter`; the `generators` submodule; NetworkX one-way conversion
- Accept: polymorphic bare-name dispatch over `graph_*`/`digraph_*` forms; `visit.*` visitors with `PruneSearch`/`StopSearch` over hand-rolled traversal loops; `ColoringStrategy`-parameterized greedy coloring; typed result carriers (`PathMapping`/`CentralityMapping`/`NodeMap`/`Pos2DMapping`) as receipts; stable node index as the join key into the data graph table; `networkx_converter` at the read-side boundary; `igraph` for the community-detection split
- Reject: per-graph-type parallel entrypoints when the polymorphic form covers both; manual BFS/DFS/Dijkstra loop reimplementation in place of `visit.*`; a post-hoc cycle scan where `PyDAG`/`check_cycle` rejects at mutation; re-keying centrality output away from the stable node index; re-exporting rustworkx result carriers through thin rename wrappers; routing community detection here instead of `igraph`
