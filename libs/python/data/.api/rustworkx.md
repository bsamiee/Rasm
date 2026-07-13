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
- entry points: import-only; no console script
- capability: undirected/directed/DAG containers with stable integer indices and arbitrary-object payloads; Dijkstra/Bellman-Ford/A*/Floyd-Warshall/k-shortest/all-pairs/all-simple-path shortest-path family; topological sort and generations, longest-path, transitive reduction, dominators and dominance frontiers; connected/strongly/weakly-connected components, condensation, core-number, articulation points, bridges, chain/cycle-basis decomposition, Stoer-Wagner min-cut; BFS/DFS/Dijkstra event-stepped search via `visit.*` visitors; betweenness/closeness/eigenvector/Katz/PageRank/HITS/degree and group centrality; greedy/edge/Misra-Gries/bipartite vertex-and-edge coloring; max-weight matching; minimum spanning tree/edges, Steiner tree, metric closure; VF2 (sub)graph isomorphism and maximum bisimulation; spring/circular/shell/spiral/bipartite/Kamada-Kawai layouts; GraphML/DOT/node-link-JSON/Matrix-Market IO and adjacency-matrix export; NetworkX one-way conversion; `generators` synthetic-graph submodule; `TopologicalSorter` incremental graphlib-style sorter

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers
- rail: graph
- node/edge payloads are arbitrary Python objects; integer indices are stable and never recycled after removal, so `NodeIndices`/`EdgeIndices` are valid stable keys into sibling tables

| [INDEX] | [SYMBOL]              | [GRAPH_KIND]     | [CAPABILITY]                                                            |
| :-----: | :-------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `rustworkx.PyGraph`   | undirected       | `multigraph` parallel-edge gate, `degree`, `to_directed`                |
|  [02]   | `rustworkx.PyDiGraph` | directed         | directed degree/adjacency plus node/edge mutation family (roster below) |
|  [03]   | `rustworkx.PyDAG`     | directed acyclic | alias of `PyDiGraph` with `check_cycle=True`; mutation-time acyclicity  |

`PyDiGraph` adds over the shared surface: `in_degree`/`out_degree`, `predecessors`/`successors`/`predecessor_indices`/`successor_indices`, `reverse`/`make_symmetric`/`is_symmetric`, `merge_nodes`, `can_contract_without_cycle`, `add_child`/`add_parent`, `remove_node_retain_edges`/`_by_id`/`_by_key`, `insert_node_on_in_edges`/`_out_edges`(`_multiple`), `find_successor_node_by_edge`/`find_predecessor_node_by_edge`, `to_undirected`.

[PUBLIC_TYPE_SCOPE]: shared container members (`PyGraph` and `PyDiGraph`)
- rail: graph

Members shared across both containers, by family:
- [01]-[MUTATION]: `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `add_edges_from_no_data`, `remove_node`, `remove_nodes_from`, `remove_edge`, `remove_edges_from`, `remove_edge_from_index`, `contract_nodes`, `update_edge`, `update_edge_by_index`, `clear`, `clear_edges`
- [02]-[QUERY]: `get_node_data`, `get_edge_data`, `get_edge_data_by_index`, `get_all_edge_data`, `get_edge_endpoints_by_index`, `has_node`, `has_edge`, `has_parallel_edges`, `num_nodes`, `num_edges`, `node_indices`, `edge_indices`, `neighbors`, `degree`, `find_node_by_weight`, `nodes`, `edges`, `attrs` (graph-level attribute payload)
- [03]-[EDGE_VIEWS]: `edge_list`, `weighted_edge_list`, `edge_index_map`, `incident_edges`, `incident_edge_index_map`, `in_edges`/`out_edges`, `in_edge_indices`/`out_edge_indices`, `edge_indices_from_endpoints`, `adj`
- [04]-[DERIVATION]: `subgraph`, `subgraph_with_nodemap`, `edge_subgraph`, `copy`, `filter_nodes`, `filter_edges`, `compose`, `substitute_node_with_subgraph`
- [05]-[IO_ADJACENCY]: `to_dot`, `read_edge_list`, `write_edge_list`, `extend_from_edge_list`, `extend_from_weighted_edge_list`, `from_adjacency_matrix`, `from_complex_adjacency_matrix`

[PUBLIC_TYPE_SCOPE]: search visitors, incremental sorter, and coloring strategy
- rail: graph
- the `visit.*` visitors are subclassed; the search drives `discover_vertex`/`tree_edge`/`finish_vertex` (and Dijkstra `examine_edge`/`edge_relaxed`) callbacks; raising `visit.PruneSearch` skips a subtree and `visit.StopSearch` halts the whole traversal

| [INDEX] | [SYMBOL]                | [KIND]             | [CONSUMER_NOTE]                                                                  |
| :-----: | :---------------------- | :----------------- | :------------------------------------------------------------------------------- |
|  [01]   | `visit.BFSVisitor`      | visitor base       | subclass for `bfs_search`; discover/examine callbacks for event-stepped BFS      |
|  [02]   | `visit.DFSVisitor`      | visitor base       | subclass for `dfs_search`; tree/back/forward edge classification callbacks       |
|  [03]   | `visit.DijkstraVisitor` | visitor base       | subclass for `dijkstra_search`; `edge_relaxed`/`examine_edge` frontier callbacks |
|  [04]   | `visit.PruneSearch`     | control exception  | raise in a callback to prune the current subtree without halting                 |
|  [05]   | `visit.StopSearch`      | control exception  | raise inside a visitor callback to terminate the entire search early             |
|  [06]   | `TopologicalSorter`     | incremental sorter | `graphlib`-style topo sort; `get_ready`/`done`/`is_active` ready-set loop        |
|  [07]   | `ColoringStrategy`      | enum               | `Degree`/`IndependentSet`/`Saturation` (DSATUR) greedy-color ordering            |

[PUBLIC_TYPE_SCOPE]: result containers and exceptions
- rail: graph
- result carriers are mapping/sequence-like views over node/edge indices; they iterate and index without a Python re-materialization step

| [INDEX] | [SYMBOL]                                                                    | [KIND]    | [PRODUCED_BY]                                  |
| :-----: | :-------------------------------------------------------------------------- | :-------- | :--------------------------------------------- |
|  [01]   | `NodeIndices`, `EdgeIndices`, `NodeMap`, `NodesCountMapping`                | result    | index/count queries and structural maps        |
|  [02]   | `EdgeList`, `WeightedEdgeList`, `EdgeIndexMap`                              | result    | edge enumeration                               |
|  [03]   | `PathMapping`, `PathLengthMapping`, `MultiplePathMapping`                   | result    | single-source shortest path / all-simple-paths |
|  [04]   | `AllPairsPathMapping`                                                       | result    | all-pairs shortest path                        |
|  [05]   | `AllPairsPathLengthMapping`                                                 | result    | all-pairs path length                          |
|  [06]   | `AllPairsMultiplePathMapping`                                               | result    | all-pairs simple paths                         |
|  [07]   | `CentralityMapping`, `EdgeCentralityMapping`                                | result    | centrality scores                              |
|  [08]   | `BFSSuccessors`, `BFSPredecessors`, `Chains`                                | result    | BFS traversal / chain decomposition            |
|  [09]   | `BiconnectedComponents`, `Pos2DMapping`, `ProductNodeMap`                   | result    | biconnectivity, layout, graph-product node map |
|  [10]   | `IndexPartitionBlock`, `RelationalCoarsestPartition`                        | result    | maximum-bisimulation coarsest partition        |
|  [11]   | `GraphMLDomain`, `GraphMLKey`, `GraphMLType`                                | enum      | GraphML read/write key typing                  |
|  [12]   | `DAGHasCycle`, `DAGWouldCycle`                                              | exception | `PyDAG`/`check_cycle` violation at mutation    |
|  [13]   | `NoPathFound`, `NegativeCycle`                                              | exception | path search failure                            |
|  [14]   | `NoEdgeBetweenNodes`, `InvalidNode`, `InvalidMapping`                       | exception | invalid index / token-swapper input            |
|  [15]   | `GraphNotBipartite`, `FailedToConverge`, `NoSuitableNeighbors`, `NullGraph` | exception | algorithm precondition / empty-graph failure   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shortest-path, simple-path, and DAG algorithms
- rail: graph
- bare names dispatch on graph type; `graph_*`/`digraph_*` forms bind one type; weight is a `weight_fn` callback over the edge payload, not a stored attribute
- result carriers are the `[02]` typed rows

| [INDEX] | [FAMILY]      | [ENTRY]                                                                                                            |
| :-----: | :------------ | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | shortest path | `dijkstra_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)`     |
|  [02]   | shortest path | `bellman_ford_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)` |
|  [03]   | shortest path | `astar_shortest_path(graph, node, goal_fn, edge_cost_fn, estimate_cost_fn)`                                        |
|  [04]   | shortest path | `all_shortest_paths(graph, source, target, weight_fn=None, ...)`                                                   |
|  [05]   | shortest path | `single_source_all_shortest_paths(graph, source, ...)`                                                             |
|  [06]   | shortest path | `k_shortest_path_lengths(graph, start, k, edge_cost, goal=None)`                                                   |
|  [07]   | shortest path | `has_path(graph, source, target, as_undirected=False)`                                                             |
|  [08]   | shortest path | `num_shortest_paths_unweighted(graph, source)`                                                                     |
|  [09]   | shortest path | `unweighted_average_shortest_path_length(graph, parallel_threshold=300, disconnected=False)`                       |
|  [10]   | all pairs     | `all_pairs_dijkstra_shortest_paths(graph, edge_cost_fn)`                                                           |
|  [11]   | all pairs     | `all_pairs_bellman_ford_shortest_paths(graph, edge_cost_fn)`                                                       |
|  [12]   | all pairs     | `floyd_warshall(graph, weight_fn=None, default_weight=1.0, parallel_threshold=300)`                                |
|  [13]   | all pairs     | `floyd_warshall_numpy(graph, ...)`                                                                                 |
|  [14]   | all pairs     | `floyd_warshall_successor_and_distance(graph, ...)`                                                                |
|  [15]   | all pairs     | `distance_matrix(graph, parallel_threshold=300, as_undirected=False, null_value=0.0)`                              |
|  [16]   | simple paths  | `all_simple_paths(graph, from_, to, min_depth=None, cutoff=None)`                                                  |
|  [17]   | simple paths  | `longest_simple_path(graph)`                                                                                       |
|  [18]   | simple paths  | `all_pairs_all_simple_paths(graph, ...)`                                                                           |
|  [19]   | DAG           | `topological_sort(graph)`                                                                                          |
|  [20]   | DAG           | `topological_generations(graph)`                                                                                   |
|  [21]   | DAG           | `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None)`                                    |
|  [22]   | DAG           | `dag_longest_path(graph, /, weight_fn=None)`                                                                       |
|  [23]   | DAG           | `dag_weighted_longest_path(graph, weight_fn)`                                                                      |
|  [24]   | DAG           | `dag_longest_path_length(graph, ...)`                                                                              |
|  [25]   | DAG           | `is_directed_acyclic_graph(graph)`                                                                                 |
|  [26]   | DAG           | `transitive_reduction(graph)`                                                                                      |
|  [27]   | DAG           | `immediate_dominators(graph, start_node)`                                                                          |
|  [28]   | DAG           | `dominance_frontiers(graph, start_node)`                                                                           |

[ENTRYPOINT_SCOPE]: connectivity, traversal, search, and centrality
- rail: graph
- call: `pagerank(graph, /, alpha=0.85, weight_fn=None, nstart=None, personalization=None, tol=1e-06, max_iter=100, dangling=None)`

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                      |
| :-----: | :----------- | :------------------------------------------------------------------------------------------- |
|  [01]   | connectivity | `connected_components(graph)`                                                                |
|  [02]   | connectivity | `strongly_connected_components(graph)`                                                       |
|  [03]   | connectivity | `weakly_connected_components(graph)`                                                         |
|  [04]   | connectivity | `number_connected_components`                                                                |
|  [05]   | connectivity | `number_strongly_connected_components`                                                       |
|  [06]   | connectivity | `number_weakly_connected_components`                                                         |
|  [07]   | connectivity | `is_connected`                                                                               |
|  [08]   | connectivity | `is_strongly_connected`                                                                      |
|  [09]   | connectivity | `is_weakly_connected`                                                                        |
|  [10]   | connectivity | `is_semi_connected`                                                                          |
|  [11]   | connectivity | `node_connected_component(graph, node)`                                                      |
|  [12]   | connectivity | `connected_subgraphs(graph, k)`                                                              |
|  [13]   | connectivity | `condensation(graph, sccs=None)`                                                             |
|  [14]   | connectivity | `core_number(graph)`                                                                         |
|  [15]   | structure    | `articulation_points(graph)`                                                                 |
|  [16]   | structure    | `bridges(graph)`                                                                             |
|  [17]   | structure    | `biconnected_components(graph)`                                                              |
|  [18]   | structure    | `chain_decomposition(graph, source=None)`                                                    |
|  [19]   | structure    | `cycle_basis(graph, root=None)`                                                              |
|  [20]   | cut          | `stoer_wagner_min_cut(graph, /, weight_fn=None)`                                             |
|  [21]   | cut          | `simple_cycles(graph)`                                                                       |
|  [22]   | cut          | `digraph_find_cycle(graph, source=None)`                                                     |
|  [23]   | cut          | `find_negative_cycle(graph, edge_cost_fn)`                                                   |
|  [24]   | cut          | `negative_edge_cycle(graph, edge_cost_fn)`                                                   |
|  [25]   | traversal    | `bfs_successors(graph, node)`                                                                |
|  [26]   | traversal    | `bfs_predecessors(graph, node)`                                                              |
|  [27]   | traversal    | `bfs_layers(graph, sources)`                                                                 |
|  [28]   | traversal    | `dfs_edges(graph, source=None)`                                                              |
|  [29]   | traversal    | `ancestors(graph, node)`                                                                     |
|  [30]   | traversal    | `descendants(graph, node)`                                                                   |
|  [31]   | traversal    | `isolates(graph)`                                                                            |
|  [32]   | traversal    | `collect_runs(graph, filter_fn)`                                                             |
|  [33]   | traversal    | `collect_bicolor_runs(graph, filter_fn, color_fn)`                                           |
|  [34]   | search       | `bfs_search(graph, source, visitor)`                                                         |
|  [35]   | search       | `dfs_search(graph, source, visitor)`                                                         |
|  [36]   | search       | `dijkstra_search(graph, source, weight_fn, visitor)`                                         |
|  [37]   | centrality   | `betweenness_centrality(graph, normalized=True, endpoints=False, parallel_threshold=50)`     |
|  [38]   | centrality   | `edge_betweenness_centrality(graph, normalized=True, parallel_threshold=50)`                 |
|  [39]   | centrality   | `closeness_centrality(graph, wf_improved=True)`                                              |
|  [40]   | centrality   | `newman_weighted_closeness_centrality(graph, weight_fn, ...)`                                |
|  [41]   | centrality   | `degree_centrality(graph)`                                                                   |
|  [42]   | centrality   | `in_degree_centrality`/`out_degree_centrality`                                               |
|  [43]   | centrality   | `eigenvector_centrality(graph, weight_fn=None, default_weight=1.0, max_iter=100, tol=1e-06)` |
|  [44]   | centrality   | `katz_centrality(graph, alpha=0.1, beta=1.0, ...)`                                           |
|  [45]   | centrality   | `pagerank(graph, /, ...)` (full signature in scope)                                          |
|  [46]   | centrality   | `hits(graph, /, weight_fn=None, nstart=None, tol=1e-08, max_iter=100, normalized=True)`      |
|  [47]   | centrality   | `group_betweenness_centrality(graph, sources, ...)`                                          |
|  [48]   | centrality   | `group_closeness_centrality(graph, sources, ...)`                                            |
|  [49]   | centrality   | `group_degree_centrality(graph, sources)`                                                    |

[ENTRYPOINT_SCOPE]: coloring, matching, spanning, isomorphism, bisimulation, structure, layout, IO, conversion, generators
- rail: graph
- call: `max_weight_matching(graph, /, max_cardinality=False, weight_fn=None, default_weight=1, verify_optimum=False)`

| [INDEX] | [FAMILY]     | [ENTRY]                                                                                              |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | coloring     | `graph_greedy_color(graph, /, preset_color_fn=None, strategy=ColoringStrategy.Degree)`               |
|  [02]   | coloring     | `graph_greedy_edge_color(graph, ...)`                                                                |
|  [03]   | coloring     | `graph_misra_gries_edge_color(graph)`                                                                |
|  [04]   | coloring     | `graph_bipartite_edge_color(graph)`                                                                  |
|  [05]   | coloring     | `two_color(graph)`                                                                                   |
|  [06]   | matching     | `max_weight_matching(graph, /, ...)` (full signature in scope)                                       |
|  [07]   | matching     | `is_matching(graph, matching)`                                                                       |
|  [08]   | matching     | `is_maximal_matching(graph, matching)`                                                               |
|  [09]   | spanning     | `minimum_spanning_tree(graph, weight_fn=None, default_weight=1.0)`                                   |
|  [10]   | spanning     | `minimum_spanning_edges(graph, ...)`                                                                 |
|  [11]   | spanning     | `steiner_tree(graph, terminal_nodes, weight_fn, /)`                                                  |
|  [12]   | spanning     | `metric_closure(graph, weight_fn)`                                                                   |
|  [13]   | isomorphism  | `is_isomorphic(first, second, node_matcher=None, edge_matcher=None, id_order=True, call_limit=None)` |
|  [14]   | isomorphism  | `is_isomorphic_node_match(first, second, matcher, id_order=True)`                                    |
|  [15]   | isomorphism  | `is_subgraph_isomorphic(first, second, ..., induced=True)`                                           |
|  [16]   | isomorphism  | `vf2_mapping(first, second, ..., subgraph=False, mapping_parameters=None)`                           |
|  [17]   | bisimulation | `digraph_maximum_bisimulation(graph)`                                                                |
|  [18]   | structure    | `is_planar(graph)`                                                                                   |
|  [19]   | structure    | `is_bipartite(graph)`                                                                                |
|  [20]   | structure    | `transitivity(graph)`                                                                                |
|  [21]   | structure    | `complement(graph)`                                                                                  |
|  [22]   | structure    | `local_complement(graph, node)`                                                                      |
|  [23]   | structure    | `line_graph(graph)`                                                                                  |
|  [24]   | structure    | `union(first, second, merge_nodes=False, merge_edges=False)`                                         |
|  [25]   | structure    | `cartesian_product(first, second)`                                                                   |
|  [26]   | structure    | `tensor_product(first, second)`                                                                      |
|  [27]   | structure    | `graph_token_swapper(graph, mapping, /, trials=None, seed=None, parallel_threshold=50)`              |
|  [28]   | routing      | `hyperbolic_greedy_routing(graph, ...)`                                                              |
|  [29]   | routing      | `hyperbolic_greedy_success_rate(graph, ...)`                                                         |
|  [30]   | layout       | `spring_layout(graph, ...)`                                                                          |
|  [31]   | layout       | `circular_layout(graph, scale=1, center=None)`                                                       |
|  [32]   | layout       | `shell_layout(graph, ...)`                                                                           |
|  [33]   | layout       | `spiral_layout(graph, ...)`                                                                          |
|  [34]   | layout       | `bipartite_layout(graph, first_nodes, ...)`                                                          |
|  [35]   | layout       | `random_layout(graph, center=None, seed=None)`                                                       |
|  [36]   | layout       | `kamada_kawai_layout(graph, ...)`                                                                    |
|  [37]   | IO           | `node_link_json(graph, path=None, graph_attrs=None, node_attrs=None, edge_attrs=None)`               |
|  [38]   | IO           | `parse_node_link_json(data, ...)`                                                                    |
|  [39]   | IO           | `from_node_link_json_file(path, ...)`                                                                |
|  [40]   | IO           | `from_dot(dot_str)`                                                                                  |
|  [41]   | IO           | `read_graphml(path)`                                                                                 |
|  [42]   | IO           | `write_graphml(graph, path)`                                                                         |
|  [43]   | IO           | `read_matrix_market(...)`                                                                            |
|  [44]   | IO           | `read_matrix_market_file(path)`                                                                      |
|  [45]   | IO           | `write_matrix_market(graph, ...)`                                                                    |
|  [46]   | IO           | `adjacency_matrix(graph, weight_fn=None, default_weight=1.0, null_value=0.0)`                        |
|  [47]   | conversion   | `networkx_converter(graph, keep_attributes=False)`                                                   |
|  [48]   | generators   | `undirected_gnp_random_graph(num_nodes, probability, seed=None)`                                     |
|  [49]   | generators   | `directed_gnp_random_graph`                                                                          |
|  [50]   | generators   | `undirected_gnm_random_graph`                                                                        |
|  [51]   | generators   | `directed_gnm_random_graph`                                                                          |
|  [52]   | generators   | `random_geometric_graph(num_nodes, radius, ...)`                                                     |
|  [53]   | generators   | `barabasi_albert_graph(n, m, seed=None, initial_graph=None)`                                         |
|  [54]   | generators   | `directed_barabasi_albert_graph`                                                                     |
|  [55]   | generators   | `random_regular_graph(d, n, seed=None)`                                                              |
|  [56]   | generators   | `undirected_sbm_random_graph`/`directed_sbm_random_graph`                                            |
|  [57]   | generators   | `hyperbolic_random_graph`                                                                            |
|  [58]   | generators   | `generate_random_path`                                                                               |

`rustworkx.generators` synthetic-graph builders: `complete_graph`, `cycle_graph`, `grid_graph`, `path_graph`, `star_graph`, `mesh_graph`, `full_rary_tree`, `binomial_tree_graph`, `barbell_graph`, `lollipop_graph`, `hexagonal_lattice_graph`, `heavy_hex_graph`, `heavy_square_graph`, `generalized_petersen_graph`, `dorogovtsev_goltsev_mendes_graph`, `karate_club_graph` (plus `directed_*` mirrors and `empty_graph`/`directed_empty_graph`).

## [04]-[IMPLEMENTATION_LAW]

[DISPATCH_TOPOLOGY]:
- Graph-typed forms (`graph_*`, `digraph_*`) and the polymorphic bare-name forms both exist; the bare name dispatches on graph type — route through the bare name and bind a type-specific form only inside a single-type kernel.
- Node and edge payloads are arbitrary Python objects; integer index stability with no recycling after removal makes `NodeIndices`/`EdgeIndices` durable join keys — the data graph owner lowers a node-index-keyed result into a `node`-keyed Arrow frame the tabular plane left-joins a node-attribute table against by the same index, never re-keys.
- `PyDAG` is `PyDiGraph` with `check_cycle=True`; a DAG-typed owner constructs the digraph with the flag and treats `DAGWouldCycle` as the typed rejection at mutation time, never a post-hoc cycle scan.
- Event-stepped traversal is the `visit.*` visitor protocol: subclass `BFSVisitor`/`DFSVisitor`/`DijkstraVisitor`, override the discover/examine/finish callbacks, and raise `PruneSearch`/`StopSearch` for early termination — never re-implement the frontier loop in Python.
- `TopologicalSorter` is the incremental `graphlib`-shaped ready-set sorter over a `PyDiGraph` (`get_ready`/`done`/`is_active`); the static one-shot order is `topological_sort`/`topological_generations`. Pick the incremental sorter when staged execution releases nodes as dependencies complete.
- Greedy coloring is parameterized by the `ColoringStrategy` enum (`Saturation` is DSATUR); a `preset_color_fn` pins specific nodes — never hand-roll a saturation-degree ordering.
- Layout functions return `Pos2DMapping` (node index to `(x, y)`); they compute coordinates only and feed a downstream visualization carrier, never render.

[SIBLING_STACK]:
- `networkx_converter` is the one-way NetworkX-to-rustworkx bridge at the boundary; the `networkx` sibling stays the read-side interop surface and rustworkx owns the hot algorithm path. Graph construction is never re-implemented locally on either side of the bridge.
- `igraph` (GPL C core) owns Leiden/Louvain/Infomap community detection that rustworkx lacks; rustworkx owns the permissive-license path/centrality/structure suite. Route GRAPH_COMMUNITY to `igraph`, GRAPH_PATH/GRAPH_CENTRALITY/GRAPH_STRUCTURE to rustworkx, and keep the GPL dependency off the host-distributed plugin.
- `floyd_warshall_numpy`/`distance_matrix`/`adjacency_matrix` emit dense NumPy matrices that fold directly into the `pyarrow`/`numpy` tensor carriers; `node_link_json` and GraphML/Matrix-Market IO are the durable serialization seam the data graph owner content-keys.
- `betweenness_centrality`/`pagerank`/`core_number`/`CentralityMapping` keyed by stable node index lower into a `node`-keyed Arrow frame (the data graph owner's result-to-frame projection) that the tabular plane left-outer-joins a node-attribute table against by the same index, so a centrality run is a left-join enrichment, not a re-keyed copy.

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: undirected/directed/DAG containers with stable integer indices; the full path/DAG/connectivity/traversal/search/centrality/coloring/matching/cut/isomorphism/bisimulation/layout/IO/generator algorithm suite; `visit.*` visitor-driven search; `TopologicalSorter`; the `generators` submodule; NetworkX one-way conversion
- Accept: polymorphic bare-name dispatch over `graph_*`/`digraph_*` forms; `visit.*` visitors with `PruneSearch`/`StopSearch` over hand-rolled traversal loops; `ColoringStrategy`-parameterized greedy coloring; typed result carriers (`PathMapping`/`CentralityMapping`/`NodeMap`/`Pos2DMapping`) as receipts; stable node index lowered into the `node`-keyed Arrow frame the tabular plane left-outer-joins a node-attribute table against; `networkx_converter` at the read-side boundary; `igraph` for the community-detection split
- Reject: per-graph-type parallel entrypoints when the polymorphic form covers both; manual BFS/DFS/Dijkstra loop reimplementation in place of `visit.*`; a post-hoc cycle scan where `PyDAG`/`check_cycle` rejects at mutation; re-keying centrality output away from the stable node index; re-exporting rustworkx result carriers through thin rename wrappers; routing community detection here instead of `igraph`
