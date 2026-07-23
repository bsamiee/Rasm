# [PY_DATA_API_RUSTWORKX]

`rustworkx` owns the Rust-core graph surface for the data graph rail: `PyGraph`/`PyDiGraph`/`PyDAG` containers over stable, never-recycled integer indices, with a polymorphic bare-name algorithm suite dispatching on graph type beside the `graph_*`/`digraph_*` type-bound forms. Data graph callers route through the bare name, read the typed result carrier (`PathMapping`/`CentralityMapping`/`NodeMap`/`Pos2DMapping`) as the receipt, drive event-stepped search through the `visit.*` visitor base classes, and fold the Rust-core traversal over re-walking a BFS/DFS/Dijkstra loop in Python.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rustworkx`
- package: `rustworkx` (Apache-2.0)
- module: `import rustworkx as rx`
- rail: graph — permissive-license Rust-core graph algorithm engine for the data plane
- asset: native compiled Rust extension over a PyO3 core; `generators` and `visit` submodules
- entry points: import-only; no console script

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers
- integer indices are stable and never recycled after removal, so `NodeIndices`/`EdgeIndices` are durable keys into sibling tables; node/edge payloads are arbitrary Python objects

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [CAPABILITY]                                                  |
| :-----: | :-------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `rustworkx.PyGraph`   | undirected       | `multigraph` parallel-edge gate, `degree`, `to_directed`      |
|  [02]   | `rustworkx.PyDiGraph` | directed         | directed degree/adjacency plus the node/edge mutation family  |
|  [03]   | `rustworkx.PyDAG`     | directed acyclic | `PyDiGraph` with `check_cycle=True`; mutation-time acyclicity |

[DIGRAPH_ADDS]: `in_degree` `out_degree` `predecessors` `successors` `predecessor_indices` `successor_indices` `reverse` `make_symmetric` `is_symmetric` `merge_nodes` `can_contract_without_cycle` `add_child` `add_parent` `remove_node_retain_edges` `remove_node_retain_edges_by_id` `remove_node_retain_edges_by_key` `insert_node_on_in_edges` `insert_node_on_out_edges` `insert_node_on_in_edges_multiple` `insert_node_on_out_edges_multiple` `find_successor_node_by_edge` `find_predecessor_node_by_edge` `to_undirected`

[PUBLIC_TYPE_SCOPE]: shared container members (`PyGraph` and `PyDiGraph`)

[MUTATION]: `add_node` `add_nodes_from` `add_edge` `add_edges_from` `add_edges_from_no_data` `remove_node` `remove_nodes_from` `remove_edge` `remove_edges_from` `remove_edge_from_index` `contract_nodes` `update_edge` `update_edge_by_index` `clear` `clear_edges`
[QUERY]: `get_node_data` `get_edge_data` `get_edge_data_by_index` `get_all_edge_data` `get_edge_endpoints_by_index` `has_node` `has_edge` `has_parallel_edges` `num_nodes` `num_edges` `node_indices` `edge_indices` `neighbors` `degree` `find_node_by_weight` `nodes` `edges` `attrs`
[EDGE_VIEWS]: `edge_list` `weighted_edge_list` `edge_index_map` `incident_edges` `incident_edge_index_map` `in_edges` `out_edges` `in_edge_indices` `out_edge_indices` `edge_indices_from_endpoints` `adj`
[DERIVATION]: `subgraph` `subgraph_with_nodemap` `edge_subgraph` `copy` `filter_nodes` `filter_edges` `compose` `substitute_node_with_subgraph`
[IO_ADJACENCY]: `to_dot` `read_edge_list` `write_edge_list` `extend_from_edge_list` `extend_from_weighted_edge_list` `from_adjacency_matrix` `from_complex_adjacency_matrix`

[PUBLIC_TYPE_SCOPE]: search visitors, incremental sorter, and coloring strategy
- `visit.*` visitors subclass to override `discover_vertex`/`tree_edge`/`finish_vertex` (and Dijkstra `examine_edge`/`edge_relaxed`) callbacks; `PruneSearch` skips a subtree and `StopSearch` halts the whole traversal

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [CAPABILITY]                                                       |
| :-----: | :---------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `visit.BFSVisitor`      | visitor base       | `bfs_search` discover/examine callbacks for event-stepped BFS      |
|  [02]   | `visit.DFSVisitor`      | visitor base       | `dfs_search` tree/back/forward edge classification callbacks       |
|  [03]   | `visit.DijkstraVisitor` | visitor base       | `dijkstra_search` `edge_relaxed`/`examine_edge` frontier callbacks |
|  [04]   | `visit.PruneSearch`     | control exception  | prune the current subtree without halting                          |
|  [05]   | `visit.StopSearch`      | control exception  | terminate the entire search early                                  |
|  [06]   | `TopologicalSorter`     | incremental sorter | `graphlib`-style topo sort; `get_ready`/`done`/`is_active` loop    |
|  [07]   | `ColoringStrategy`      | enum               | `Degree`/`IndependentSet`/`Saturation` (DSATUR) color ordering     |

[PUBLIC_TYPE_SCOPE]: result carriers, enums, and exceptions
- result carriers are mapping/sequence-like views over node/edge indices; they iterate and index without a Python re-materialization step, grouped below by the producer that emits them

[INDEX_MAPS]: `NodeIndices` `EdgeIndices` `NodeMap` `NodesCountMapping`
[EDGE_VIEWS_RESULT]: `EdgeList` `WeightedEdgeList` `EdgeIndexMap`
[PATH_SINGLE]: `PathMapping` `PathLengthMapping` `MultiplePathMapping`
[PATH_ALLPAIRS]: `AllPairsPathMapping` `AllPairsPathLengthMapping` `AllPairsMultiplePathMapping`
[CENTRALITY_RESULT]: `CentralityMapping` `EdgeCentralityMapping`
[TRAVERSAL_RESULT]: `BFSSuccessors` `BFSPredecessors` `Chains`
[STRUCTURE_RESULT]: `BiconnectedComponents` `Pos2DMapping` `ProductNodeMap`
[BISIMULATION_RESULT]: `IndexPartitionBlock` `RelationalCoarsestPartition`
[GRAPHML_ENUMS]: `GraphMLDomain` `GraphMLKey` `GraphMLType`
[EXC_CYCLE]: `DAGHasCycle` `DAGWouldCycle`
[EXC_PATH]: `NoPathFound` `NegativeCycle`
[EXC_INPUT]: `NoEdgeBetweenNodes` `InvalidNode` `InvalidMapping`
[EXC_PRECONDITION]: `GraphNotBipartite` `FailedToConverge` `NoSuitableNeighbors` `NullGraph`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shortest-path, simple-path, and DAG algorithms
- module-level functions; the bare name dispatches on graph type while `graph_*`/`digraph_*` forms bind one type; `weight_fn` is a callback over the edge payload, not a stored attribute; result carriers are the `[02]` typed rows

| [INDEX] | [SURFACE]                                                                                                          | [CAPABILITY]  |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------ |
|  [01]   | `dijkstra_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)`     | shortest path |
|  [02]   | `bellman_ford_shortest_paths(graph, source, target=None, weight_fn=None, default_weight=1.0, as_undirected=False)` | shortest path |
|  [03]   | `astar_shortest_path(graph, node, goal_fn, edge_cost_fn, estimate_cost_fn)`                                        | shortest path |
|  [04]   | `all_shortest_paths(graph, source, target, weight_fn=None, ...)`                                                   | shortest path |
|  [05]   | `single_source_all_shortest_paths(graph, source, ...)`                                                             | shortest path |
|  [06]   | `k_shortest_path_lengths(graph, start, k, edge_cost, goal=None)`                                                   | shortest path |
|  [07]   | `has_path(graph, source, target, as_undirected=False)`                                                             | shortest path |
|  [08]   | `num_shortest_paths_unweighted(graph, source)`                                                                     | shortest path |
|  [09]   | `unweighted_average_shortest_path_length(graph, parallel_threshold=300, disconnected=False)`                       | shortest path |
|  [10]   | `all_pairs_dijkstra_shortest_paths(graph, edge_cost_fn)`                                                           | all pairs     |
|  [11]   | `all_pairs_bellman_ford_shortest_paths(graph, edge_cost_fn)`                                                       | all pairs     |
|  [12]   | `floyd_warshall(graph, weight_fn=None, default_weight=1.0, parallel_threshold=300)`                                | all pairs     |
|  [13]   | `floyd_warshall_numpy(graph, ...)`                                                                                 | all pairs     |
|  [14]   | `floyd_warshall_successor_and_distance(graph, ...)`                                                                | all pairs     |
|  [15]   | `distance_matrix(graph, parallel_threshold=300, as_undirected=False, null_value=0.0)`                              | all pairs     |
|  [16]   | `all_simple_paths(graph, from_, to, min_depth=None, cutoff=None)`                                                  | simple paths  |
|  [17]   | `longest_simple_path(graph)`                                                                                       | simple paths  |
|  [18]   | `all_pairs_all_simple_paths(graph, ...)`                                                                           | simple paths  |
|  [19]   | `topological_sort(graph)`                                                                                          | DAG           |
|  [20]   | `topological_generations(graph)`                                                                                   | DAG           |
|  [21]   | `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None)`                                    | DAG           |
|  [22]   | `dag_longest_path(graph, /, weight_fn=None)`                                                                       | DAG           |
|  [23]   | `dag_weighted_longest_path(graph, weight_fn)`                                                                      | DAG           |
|  [24]   | `dag_longest_path_length(graph, ...)`                                                                              | DAG           |
|  [25]   | `is_directed_acyclic_graph(graph)`                                                                                 | DAG           |
|  [26]   | `transitive_reduction(graph)`                                                                                      | DAG           |
|  [27]   | `immediate_dominators(graph, start_node)`                                                                          | DAG           |
|  [28]   | `dominance_frontiers(graph, start_node)`                                                                           | DAG           |

[ENTRYPOINT_SCOPE]: connectivity, traversal, search, and centrality
- `pagerank(graph, /, alpha=0.85, weight_fn=None, nstart=None, personalization=None, tol=1e-06, max_iter=100, dangling=None)` carries its full signature; the row abbreviates it

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY] |
| :-----: | :------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | `connected_components(graph)`                                                                | connectivity |
|  [02]   | `strongly_connected_components(graph)`                                                       | connectivity |
|  [03]   | `weakly_connected_components(graph)`                                                         | connectivity |
|  [04]   | `number_connected_components`                                                                | connectivity |
|  [05]   | `number_strongly_connected_components`                                                       | connectivity |
|  [06]   | `number_weakly_connected_components`                                                         | connectivity |
|  [07]   | `is_connected`                                                                               | connectivity |
|  [08]   | `is_strongly_connected`                                                                      | connectivity |
|  [09]   | `is_weakly_connected`                                                                        | connectivity |
|  [10]   | `is_semi_connected`                                                                          | connectivity |
|  [11]   | `node_connected_component(graph, node)`                                                      | connectivity |
|  [12]   | `connected_subgraphs(graph, k)`                                                              | connectivity |
|  [13]   | `condensation(graph, sccs=None)`                                                             | connectivity |
|  [14]   | `core_number(graph)`                                                                         | connectivity |
|  [15]   | `articulation_points(graph)`                                                                 | structure    |
|  [16]   | `bridges(graph)`                                                                             | structure    |
|  [17]   | `biconnected_components(graph)`                                                              | structure    |
|  [18]   | `chain_decomposition(graph, source=None)`                                                    | structure    |
|  [19]   | `cycle_basis(graph, root=None)`                                                              | structure    |
|  [20]   | `stoer_wagner_min_cut(graph, /, weight_fn=None)`                                             | cut          |
|  [21]   | `simple_cycles(graph)`                                                                       | cut          |
|  [22]   | `digraph_find_cycle(graph, source=None)`                                                     | cut          |
|  [23]   | `find_negative_cycle(graph, edge_cost_fn)`                                                   | cut          |
|  [24]   | `negative_edge_cycle(graph, edge_cost_fn)`                                                   | cut          |
|  [25]   | `bfs_successors(graph, node)`                                                                | traversal    |
|  [26]   | `bfs_predecessors(graph, node)`                                                              | traversal    |
|  [27]   | `bfs_layers(graph, sources)`                                                                 | traversal    |
|  [28]   | `dfs_edges(graph, source=None)`                                                              | traversal    |
|  [29]   | `ancestors(graph, node)`                                                                     | traversal    |
|  [30]   | `descendants(graph, node)`                                                                   | traversal    |
|  [31]   | `isolates(graph)`                                                                            | traversal    |
|  [32]   | `collect_runs(graph, filter_fn)`                                                             | traversal    |
|  [33]   | `collect_bicolor_runs(graph, filter_fn, color_fn)`                                           | traversal    |
|  [34]   | `bfs_search(graph, source, visitor)`                                                         | search       |
|  [35]   | `dfs_search(graph, source, visitor)`                                                         | search       |
|  [36]   | `dijkstra_search(graph, source, weight_fn, visitor)`                                         | search       |
|  [37]   | `betweenness_centrality(graph, normalized=True, endpoints=False, parallel_threshold=50)`     | centrality   |
|  [38]   | `edge_betweenness_centrality(graph, normalized=True, parallel_threshold=50)`                 | centrality   |
|  [39]   | `closeness_centrality(graph, wf_improved=True)`                                              | centrality   |
|  [40]   | `newman_weighted_closeness_centrality(graph, weight_fn, ...)`                                | centrality   |
|  [41]   | `degree_centrality(graph)`                                                                   | centrality   |
|  [42]   | `in_degree_centrality`/`out_degree_centrality`                                               | centrality   |
|  [43]   | `eigenvector_centrality(graph, weight_fn=None, default_weight=1.0, max_iter=100, tol=1e-06)` | centrality   |
|  [44]   | `katz_centrality(graph, alpha=0.1, beta=1.0, ...)`                                           | centrality   |
|  [45]   | `pagerank(graph, /, ...)`                                                                    | centrality   |
|  [46]   | `hits(graph, /, weight_fn=None, nstart=None, tol=1e-08, max_iter=100, normalized=True)`      | centrality   |
|  [47]   | `group_betweenness_centrality(graph, sources, ...)`                                          | centrality   |
|  [48]   | `group_closeness_centrality(graph, sources, ...)`                                            | centrality   |
|  [49]   | `group_degree_centrality(graph, sources)`                                                    | centrality   |

[ENTRYPOINT_SCOPE]: coloring, matching, spanning, isomorphism, bisimulation, structure, routing, layout, IO, conversion, generators
- `max_weight_matching(graph, /, max_cardinality=False, weight_fn=None, default_weight=1, verify_optimum=False)` carries its full signature; the row abbreviates it

| [INDEX] | [SURFACE]                                                                                            | [CAPABILITY] |
| :-----: | :--------------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | `graph_greedy_color(graph, /, preset_color_fn=None, strategy=ColoringStrategy.Degree)`               | coloring     |
|  [02]   | `graph_greedy_edge_color(graph, ...)`                                                                | coloring     |
|  [03]   | `graph_misra_gries_edge_color(graph)`                                                                | coloring     |
|  [04]   | `graph_bipartite_edge_color(graph)`                                                                  | coloring     |
|  [05]   | `two_color(graph)`                                                                                   | coloring     |
|  [06]   | `max_weight_matching(graph, /, ...)`                                                                 | matching     |
|  [07]   | `is_matching(graph, matching)`                                                                       | matching     |
|  [08]   | `is_maximal_matching(graph, matching)`                                                               | matching     |
|  [09]   | `minimum_spanning_tree(graph, weight_fn=None, default_weight=1.0)`                                   | spanning     |
|  [10]   | `minimum_spanning_edges(graph, ...)`                                                                 | spanning     |
|  [11]   | `steiner_tree(graph, terminal_nodes, weight_fn, /)`                                                  | spanning     |
|  [12]   | `metric_closure(graph, weight_fn)`                                                                   | spanning     |
|  [13]   | `is_isomorphic(first, second, node_matcher=None, edge_matcher=None, id_order=True, call_limit=None)` | isomorphism  |
|  [14]   | `is_isomorphic_node_match(first, second, matcher, id_order=True)`                                    | isomorphism  |
|  [15]   | `is_subgraph_isomorphic(first, second, ..., induced=True)`                                           | isomorphism  |
|  [16]   | `vf2_mapping(first, second, ..., subgraph=False, mapping_parameters=None)`                           | isomorphism  |
|  [17]   | `digraph_maximum_bisimulation(graph)`                                                                | bisimulation |
|  [18]   | `is_planar(graph)`                                                                                   | structure    |
|  [19]   | `is_bipartite(graph)`                                                                                | structure    |
|  [20]   | `transitivity(graph)`                                                                                | structure    |
|  [21]   | `complement(graph)`                                                                                  | structure    |
|  [22]   | `local_complement(graph, node)`                                                                      | structure    |
|  [23]   | `graph_line_graph(graph)`                                                                            | structure    |
|  [24]   | `union(first, second, merge_nodes=False, merge_edges=False)`                                         | structure    |
|  [25]   | `cartesian_product(first, second)`                                                                   | structure    |
|  [26]   | `tensor_product(first, second)`                                                                      | structure    |
|  [27]   | `graph_token_swapper(graph, mapping, /, trials=None, seed=None, parallel_threshold=50)`              | structure    |
|  [28]   | `hyperbolic_greedy_routing(graph, ...)`                                                              | routing      |
|  [29]   | `hyperbolic_greedy_success_rate(graph, ...)`                                                         | routing      |
|  [30]   | `spring_layout(graph, ...)`                                                                          | layout       |
|  [31]   | `circular_layout(graph, scale=1, center=None)`                                                       | layout       |
|  [32]   | `shell_layout(graph, ...)`                                                                           | layout       |
|  [33]   | `spiral_layout(graph, ...)`                                                                          | layout       |
|  [34]   | `bipartite_layout(graph, first_nodes, ...)`                                                          | layout       |
|  [35]   | `random_layout(graph, center=None, seed=None)`                                                       | layout       |
|  [36]   | `kamada_kawai_layout(graph, ...)`                                                                    | layout       |
|  [37]   | `node_link_json(graph, path=None, graph_attrs=None, node_attrs=None, edge_attrs=None)`               | IO           |
|  [38]   | `parse_node_link_json(data, ...)`                                                                    | IO           |
|  [39]   | `from_node_link_json_file(path, ...)`                                                                | IO           |
|  [40]   | `from_dot(dot_str)`                                                                                  | IO           |
|  [41]   | `read_graphml(path)`                                                                                 | IO           |
|  [42]   | `write_graphml(graph, path)`                                                                         | IO           |
|  [43]   | `read_matrix_market(...)`                                                                            | IO           |
|  [44]   | `read_matrix_market_file(path)`                                                                      | IO           |
|  [45]   | `write_matrix_market(graph, ...)`                                                                    | IO           |
|  [46]   | `adjacency_matrix(graph, weight_fn=None, default_weight=1.0, null_value=0.0)`                        | IO           |
|  [47]   | `networkx_converter(graph, keep_attributes=False)`                                                   | conversion   |
|  [48]   | `undirected_gnp_random_graph(num_nodes, probability, seed=None)`                                     | generators   |
|  [49]   | `directed_gnp_random_graph`                                                                          | generators   |
|  [50]   | `undirected_gnm_random_graph`                                                                        | generators   |
|  [51]   | `directed_gnm_random_graph`                                                                          | generators   |
|  [52]   | `random_geometric_graph(num_nodes, radius, ...)`                                                     | generators   |
|  [53]   | `barabasi_albert_graph(n, m, seed=None, initial_graph=None)`                                         | generators   |
|  [54]   | `directed_barabasi_albert_graph`                                                                     | generators   |
|  [55]   | `random_regular_graph(d, n, seed=None)`                                                              | generators   |
|  [56]   | `undirected_sbm_random_graph`/`directed_sbm_random_graph`                                            | generators   |
|  [57]   | `hyperbolic_random_graph`                                                                            | generators   |
|  [58]   | `generate_random_path`                                                                               | generators   |

[GENERATORS_SUBMODULE]: `rustworkx.generators` synthetic-graph builders, each with a `directed_*` mirror where directedness applies — `complete_graph` `cycle_graph` `grid_graph` `path_graph` `star_graph` `mesh_graph` `full_rary_tree` `binomial_tree_graph` `barbell_graph` `lollipop_graph` `hexagonal_lattice_graph` `heavy_hex_graph` `heavy_square_graph` `generalized_petersen_graph` `dorogovtsev_goltsev_mendes_graph` `karate_club_graph` `empty_graph` `directed_empty_graph`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Bare-name forms dispatch on graph type; a single-type kernel binds a `graph_*`/`digraph_*` form, every other caller routes through the bare name.
- Stable never-recycled integer indices are durable join keys; a node-index-keyed result lowers into a `node`-keyed Arrow frame the tabular plane left-joins by index, never re-keyed.
- `PyDAG` constructs `PyDiGraph` with `check_cycle=True` and treats `DAGWouldCycle` as the mutation-time rejection, never a post-hoc cycle scan.
- Event-stepped traversal subclasses `BFSVisitor`/`DFSVisitor`/`DijkstraVisitor`, overrides the discover/examine/finish callbacks, and raises `PruneSearch`/`StopSearch` for early termination.
- `TopologicalSorter` is the incremental `graphlib`-shaped ready-set sorter (`get_ready`/`done`/`is_active`) for staged execution; `topological_sort`/`topological_generations` give the one-shot order.
- `ColoringStrategy` (`Degree`/`IndependentSet`/`Saturation`, the last DSATUR) parameterizes greedy coloring, and a `preset_color_fn` pins specific nodes.
- Layout functions return `Pos2DMapping` node-index-to-`(x, y)` coordinates that feed a downstream visualization carrier, never render.

[STACKING]:
- `networkx`(`.api/networkx.md`): `networkx_converter` is the one-way NetworkX-to-rustworkx bridge; the `networkx` side stays the read-side interop surface and rustworkx owns the hot algorithm path.
- `igraph`(`.api/igraph.md`): route `GRAPH_COMMUNITY` to `igraph` Leiden/Louvain/Infomap detection rustworkx lacks, keeping `GRAPH_PATH`/`GRAPH_CENTRALITY`/`GRAPH_STRUCTURE` on rustworkx.
- `pyarrow`(`.api/pyarrow.md`): `floyd_warshall_numpy`/`distance_matrix`/`adjacency_matrix` emit dense NumPy matrices folding into the tensor carriers, and a `CentralityMapping` keyed by stable node index lowers into the `node`-keyed Arrow frame the tabular plane left-outer-joins a node-attribute table against, so a centrality run is a left-join enrichment; `node_link_json`/GraphML/Matrix-Market IO are the content-keyed serialization seam.
- data graph owner: build from the interop frame, run the bare-name algorithm, read the typed carrier, and fold the receipt onto the downstream frame in one pass with no Python re-walk of the Rust-core result.

[LOCAL_ADMISSION]:
- Apache-2.0 links safely into a host-distributed plugin; import at boundary scope, and route community detection to the GPL `igraph`(`.api/igraph.md`) sibling.

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: undirected/directed/DAG containers over stable integer indices and the Rust-core graph algorithm suite, with `visit.*` visitor-driven search, `TopologicalSorter`, the `generators` submodule, and one-way NetworkX conversion
- Accept: polymorphic bare-name dispatch, `visit.*` visitors with `PruneSearch`/`StopSearch`, `ColoringStrategy`-parameterized coloring, the typed result carriers as receipts, stable node index lowered into the `node`-keyed Arrow frame, `networkx_converter` at the read-side boundary, and `igraph` for the community-detection split
- Reject: per-graph-type entrypoints where the polymorphic form covers both, a hand-rolled BFS/DFS/Dijkstra loop, a post-hoc cycle scan where `check_cycle` rejects at mutation, re-keying centrality output off the stable node index, a thin rename wrapper over a result carrier, and community detection routed here instead of `igraph`
