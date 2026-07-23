# [PY_BRANCH_API_NETWORKX]

`networkx` is the branch graph substrate: four payload classes spanning the directed/undirected × simple/multi-edge matrix, tabular/array/dict/sparse conversion bridges, file-format and JSON codecs, and the graph-algorithm families, all folded through one `create_using` kind discriminator and one `@_dispatchable` backend axis so a single call site serves every graph kind and installed engine. `nx.community` detection and the `nx.config` dispatch-policy layer ride the same surface; product graph-database and route-discovery state stay outside it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `networkx`
- package: `networkx` (BSD-3-Clause)
- import: `import networkx as nx`
- owner: `data` (codec/egress), `geometry` (`graph/features` analysis)
- rail: graph
- capability: payload classes, conversion bridges, file-format and JSON codecs, algorithm families over directed/undirected/multi-edge graphs, the `nx.community` detection namespace, and the `nx.config` layer routing any `@_dispatchable` algorithm to an installed backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph payload classes
- shared members carry across all four classes; directed and multi-edge classes add the members noted

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]         | [CAPABILITY]                                          |
| :-----: | :---------------- | :-------------------- | :---------------------------------------------------- |
|  [01]   | `nx.Graph`        | undirected simple     | base mutation, inspection, derivation members         |
|  [02]   | `nx.DiGraph`      | directed simple       | `successors`, `predecessors`, `in_edges`, `out_edges` |
|  [03]   | `nx.MultiGraph`   | undirected multi-edge | `new_edge_key`, `edge_key_dict_factory`               |
|  [04]   | `nx.MultiDiGraph` | directed multi-edge   | directed members plus `new_edge_key`                  |

[PUBLIC_TYPE_SCOPE]: shared graph members

[mutation]: `add_node` `add_nodes_from` `add_edge` `add_edges_from` `remove_node` `remove_edge` `clear`
[inspection]: `has_node` `has_edge` `number_of_nodes` `number_of_edges` `nodes` `edges` `adj` `degree`
[derivation]: `subgraph` `edge_subgraph` `copy` `to_directed` `to_undirected`

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- every member derives from `NetworkXException`; the domain rail translates at the boundary

| [INDEX] | [SYMBOL]                          | [PARENT]                 | [FAILURE]                        |
| :-----: | :-------------------------------- | :----------------------- | :------------------------------- |
|  [01]   | `NetworkXException`               | `Exception`              | base of the taxonomy             |
|  [02]   | `NetworkXError`                   | `NetworkXException`      | invalid graph argument           |
|  [03]   | `NetworkXAlgorithmError`          | `NetworkXException`      | algorithm precondition failure   |
|  [04]   | `NetworkXUnfeasible`              | `NetworkXAlgorithmError` | no feasible solution             |
|  [05]   | `NetworkXNoPath`                  | `NetworkXUnfeasible`     | no path between endpoints        |
|  [06]   | `NetworkXNoCycle`                 | `NetworkXUnfeasible`     | no cycle present                 |
|  [07]   | `NetworkXUnbounded`               | `NetworkXAlgorithmError` | unbounded optimisation           |
|  [08]   | `NetworkXNotImplemented`          | `NetworkXException`      | unsupported graph kind           |
|  [09]   | `NetworkXPointlessConcept`        | `NetworkXException`      | operation undefined for input    |
|  [10]   | `AmbiguousSolution`               | `NetworkXException`      | non-unique solution              |
|  [11]   | `ExceededMaxIterations`           | `NetworkXException`      | iteration cap reached            |
|  [12]   | `PowerIterationFailedConvergence` | `ExceededMaxIterations`  | eigen iteration did not converge |
|  [13]   | `HasACycle`                       | `NetworkXException`      | cycle where acyclicity required  |
|  [14]   | `NodeNotFound`                    | `NetworkXException`      | node absent from graph           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: conversion bridges
- each `from_*` has a `to_*` peer; `create_using` selects the target graph kind

| [INDEX] | [SURFACE]                                                           | [BRIDGE]  | [PEER_DIRECTION]                |
| :-----: | :------------------------------------------------------------------ | :-------- | :------------------------------ |
|  [01]   | `from_pandas_edgelist(df, source='source', target='target', ...)`   | tabular   | `to_pandas_edgelist(G, ...)`    |
|  [02]   | `from_pandas_adjacency(df, create_using=None, ...)`                 | tabular   | `to_pandas_adjacency(G, ...)`   |
|  [03]   | `from_numpy_array(A, parallel_edges=False, create_using=None, ...)` | array     | `to_numpy_array(G, ...)`        |
|  [04]   | `from_scipy_sparse_array(A, parallel_edges=False, ...)`             | array     | `to_scipy_sparse_array(G, ...)` |
|  [05]   | `from_dict_of_dicts(d, create_using=None, multigraph_input=False)`  | dict      | `to_dict_of_dicts(G, ...)`      |
|  [06]   | `from_dict_of_lists(d, create_using=None, ...)`                     | dict      | `to_dict_of_lists(G, ...)`      |
|  [07]   | `from_edgelist(edgelist, create_using=None, ...)`                   | edge list | `to_edgelist(G, ...)`           |

[ENTRYPOINT_SCOPE]: file-format codecs
- each codec family exposes `read_*` / `write_*` / `parse_*` / `generate_*` where applicable

| [INDEX] | [FORMAT]            | [READ]                   | [WRITE]                                                     |
| :-----: | :------------------ | :----------------------- | :---------------------------------------------------------- |
|  [01]   | adjacency list      | `read_adjlist`           | `write_adjlist`                                             |
|  [02]   | multiline adjacency | `read_multiline_adjlist` | `write_multiline_adjlist`                                   |
|  [03]   | edge list           | `read_edgelist`          | `write_edgelist`                                            |
|  [04]   | weighted edge list  | `read_weighted_edgelist` | `write_weighted_edgelist`                                   |
|  [05]   | GEXF                | `read_gexf`              | `write_gexf`                                                |
|  [06]   | GML                 | `read_gml`               | `write_gml`                                                 |
|  [07]   | GraphML             | `read_graphml`           | `write_graphml` (`write_graphml_lxml`, `write_graphml_xml`) |
|  [08]   | Pajek               | `read_pajek`             | `write_pajek`                                               |
|  [09]   | LEDA                | `read_leda`              | n/a                                                         |
|  [10]   | graph6              | `read_graph6`            | `write_graph6`                                              |
|  [11]   | sparse6             | `read_sparse6`           | `write_sparse6`                                             |
|  [12]   | network text        | `generate_network_text`  | `write_network_text`                                        |
|  [13]   | node-link JSON      | `node_link_graph`        | `node_link_data`                                            |
|  [14]   | adjacency JSON      | `adjacency_graph`        | `adjacency_data`                                            |
|  [15]   | cytoscape JSON      | `cytoscape_graph`        | `cytoscape_data`                                            |
|  [16]   | tree JSON           | `tree_graph`             | `tree_data`                                                 |

[ENTRYPOINT_SCOPE]: algorithm families
- community entries live under `nx.community` (prefix omitted below)

| [INDEX] | [FAMILY]      | [ENTRY]                                                                                             | [RESULT]           |
| :-----: | :------------ | :-------------------------------------------------------------------------------------------------- | :----------------- |
|  [01]   | shortest path | `shortest_path(G, source=None, target=None, weight=None, method='dijkstra')`                        | path or path dict  |
|  [02]   | shortest path | `dijkstra_path(G, source, target, weight='weight')`                                                 | node list          |
|  [03]   | shortest path | `astar_path(G, source, target, heuristic=None, weight='weight')`                                    | node list          |
|  [04]   | DAG           | `topological_sort(G)`                                                                               | node generator     |
|  [05]   | DAG           | `is_directed_acyclic_graph(G)`                                                                      | bool               |
|  [06]   | components    | `connected_components(G)`                                                                           | set generator      |
|  [07]   | components    | `strongly_connected_components(G)`                                                                  | set generator      |
|  [08]   | spanning tree | `minimum_spanning_tree(G, weight='weight', algorithm='kruskal')`                                    | graph              |
|  [09]   | shortest path | `floyd_warshall(G, weight='weight')`                                                                | dist-dict-of-dicts |
|  [10]   | components    | `weakly_connected_components(G)`                                                                    | set generator      |
|  [11]   | components    | `condensation(G, scc=None)`                                                                         | DAG of SCCs        |
|  [12]   | DAG           | `descendants(G, source)` / `ancestors(G, source)`                                                   | node set           |
|  [13]   | DAG           | `transitive_closure(G, reflexive=False)`                                                            | graph              |
|  [14]   | cycles        | `simple_cycles(G, length_bound=None)` / `find_cycle(G, source=None)`                                | cycle generator    |
|  [15]   | traversal     | `dfs_tree(G, source=None, depth_limit=None)` / `bfs_tree(G, source, ...)`                           | tree graph         |
|  [16]   | flow          | `maximum_flow(flowG, _s, _t, capacity='capacity', flow_func=None)`                                  | (value, flow dict) |
|  [17]   | flow          | `min_cost_flow(G, ...)` / `network_simplex(G, ...)`                                                 | flow dict / cost   |
|  [18]   | community     | `louvain_communities(G, weight='weight', resolution=1, threshold=1e-07, max_level=None, seed=None)` | list[set]          |
|  [19]   | community     | `greedy_modularity_communities(G, weight=None, resolution=1, cutoff=1, best_n=None)`                | list[frozenset]    |
|  [20]   | community     | `girvan_newman(G, most_valuable_edge=None)`                                                         | community iterator |
|  [21]   | community     | `modularity(G, communities, ...)`                                                                   | float              |
|  [22]   | isomorphism   | `is_isomorphic(G1, G2, node_match=None, edge_match=None)`                                           | bool               |
|  [23]   | relabel       | `relabel_nodes(G, mapping, copy=True)` / `convert_node_labels_to_integers(G, ...)`                  | graph              |
|  [24]   | set algebra   | `compose(G, H)` / `union(G, H)` / `disjoint_union(G, H)`                                            | graph              |
|  [25]   | attributes    | `get_node_attributes(G, name)` / `set_node_attributes(G, values, name=None)`                        | dict / in-place    |

[ENTRYPOINT_SCOPE]: centrality algorithms
- each returns a node-score dict

| [INDEX] | [ENTRY]                                                                                                               |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `pagerank(G, alpha=0.85, personalization=None, max_iter=100, tol=1e-06, nstart=None, weight='weight', dangling=None)` |
|  [02]   | `betweenness_centrality(G, k=None, normalized=True, weight=None, endpoints=False, seed=None)`                         |
|  [03]   | `eigenvector_centrality` / `closeness_centrality` / `degree_centrality`                                               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Payload owners thread `create_using=` to select directedness and multiplicity; one entry discriminates on graph kind instead of branching per subtype.
- Node and edge attributes live in per-element dicts; `nodes`, `edges`, `adj`, and `degree` return live views over the graph, never copies.
- Directed classes add `successors`/`predecessors` and `in_edges`/`out_edges`; multi-edge classes key parallel edges through `new_edge_key`.
- Every `@nx._dispatchable` algorithm carries `*, backend=None, **backend_kwargs`; `backend=` selects an installed entry-point engine without changing the call site, and the receipt records the backend rather than forking parallel call sites per backend.
- `shortest_path` discriminates `method=` and source/target presence to return one path, a per-target dict, or the all-pairs dict from a single entry; `floyd_warshall` is the dense all-pairs form.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): `node_link_data(G)` emits `{nodes, edges, ...}` as the canonical persisted graph document through the JSON/msgpack codec, content-keyed like every other payload; `node_link_graph(data, directed=, multigraph=)` rebuilds it, discriminating kind from the two flags rather than a per-kind reader. `adjacency`/`cytoscape`/`tree` JSON are sibling round-trip shapes.
- `numpy`(`.api/numpy.md`): `to_numpy_array` / `from_numpy_array` cross the dense numeric rail; `to_scipy_sparse_array` / `adjacency_matrix` hand the graph to the `scipy.sparse` rail for spectral and algebraic work.
- columnar rail: `to_pandas_edgelist` / `to_pandas_adjacency` are the tabular egress; reframing into Arrow or Polars belongs to the columnar owner, and the graph is never re-parsed from an ad-hoc edge schema.

[LOCAL_ADMISSION]:
- `nx.config` (a `NetworkXConfig`) owns process-global dispatch policy — `backend_priority`, `backends`, `cache_converted_graphs`, `fallback_to_nx`, `warnings_to_ignore`; the graph rail sets it once at boundary scope and threads `backend=` only to override the global priority for one call.
- Algorithm failures raise typed `NetworkX*` exceptions on the `NetworkXException` base; the domain rail translates the raised exception at the boundary into the Result rail, and the receipt captures the algorithm name with the typed failure.
- Community algorithms namespace under `nx.community`: `louvain_communities`/`greedy_modularity_communities`/`girvan_newman` partition, `modularity` scores a partition, returning `list[set]`/`list[frozenset]`/a nested-tuple iterator.

[RAIL_LAW]:
- Package: `networkx`
- Owns: graph payload classes, conversion bridges, file-format and JSON codecs, algorithm families, the `nx.community` detection namespace, and the `nx.config` backend-dispatch layer
- Accept: graph-kind discrimination via `create_using` and `node_link_graph(directed=, multigraph=)`; tabular/array/dict/sparse bridges; file and JSON codecs; algorithm receipts on the typed exception base; backend selection via `backend=` / `nx.config.backend_priority`
- Reject: wrapper-renames, per-graph-kind parallel entrypoints, per-backend forked call sites, weaker local reimplementation of supplied algorithms, and product graph-database or route-discovery state
