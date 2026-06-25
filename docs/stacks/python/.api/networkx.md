# [PY_DATA_API_NETWORKX]

`networkx` supplies `Graph`, `DiGraph`, `MultiGraph`, and `MultiDiGraph` payload classes with shared mutation, inspection, and derivation members, plus tabular/array/dict conversion bridges, file-format codecs, and shortest-path, DAG, component, spanning-tree, centrality, and flow algorithm families. The `create_using` argument discriminates graph kind across one polymorphic call, and `backend`/`**backend_kwargs` select the dispatch backend.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `networkx`
- package: `networkx`
- version: `3.6.1` (manifest unpinned — newest stable; requires-python `>=3.11`)
- license: BSD-3-Clause
- import: `import networkx as nx`
- abi: pure Python; no native extension, no ABI floor
- owner: `data`
- rail: graph
- capability: graph payload classes, conversion bridges, file-format codecs, algorithm families over directed/undirected/multi-edge graphs, a community-detection namespace (`nx.community`), and a pluggable backend-dispatch layer (`nx.config`) that routes any `@_dispatchable` algorithm to an alternate engine

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph payload classes
- rail: graph
- shared members carry across all four classes; directed and multi-edge classes add the extra members noted

| [INDEX] | [SYMBOL]          | [GRAPH_KIND]          | [ADDED_MEMBERS]                                       |
| :-----: | :---------------- | :-------------------- | :---------------------------------------------------- |
|  [01]   | `nx.Graph`        | undirected simple     | base mutation, inspection, derivation members         |
|  [02]   | `nx.DiGraph`      | directed simple       | `successors`, `predecessors`, `in_edges`, `out_edges` |
|  [03]   | `nx.MultiGraph`   | undirected multi-edge | `new_edge_key`, `edge_key_dict_factory`               |
|  [04]   | `nx.MultiDiGraph` | directed multi-edge   | directed members plus `new_edge_key`                  |

[PUBLIC_TYPE_SCOPE]: shared graph members
- rail: graph

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                         |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | mutation        | `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `remove_node`, `remove_edge`, `clear` |
|  [02]   | inspection      | `has_node`, `has_edge`, `number_of_nodes`, `number_of_edges`, `nodes`, `edges`, `adj`, `degree`   |
|  [03]   | derivation      | `subgraph`, `edge_subgraph`, `copy`, `to_directed`, `to_undirected`                               |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- rail: graph
- every member derives from `NetworkXException`; domain rails translate at the boundary

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
- rail: graph
- `create_using` selects target graph kind; `backend`/`**backend_kwargs` select dispatch backend

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
- rail: graph
- each codec family exposes `read_*`, `write_*`, `parse_*`, and `generate_*` where applicable

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
- rail: graph
- every algorithm accepts `*, backend=None, **backend_kwargs`

| [INDEX] | [FAMILY]      | [ENTRY]                                                                      | [RESULT]           |
| :-----: | :------------ | :--------------------------------------------------------------------------- | :----------------- |
|  [01]   | shortest path | `shortest_path(G, source=None, target=None, weight=None, method='dijkstra')` | path or path dict  |
|  [02]   | shortest path | `dijkstra_path(G, source, target, weight='weight')`                          | node list          |
|  [03]   | shortest path | `astar_path(G, source, target, heuristic=None, weight='weight')`             | node list          |
|  [04]   | DAG           | `topological_sort(G)`                                                        | node generator     |
|  [05]   | DAG           | `is_directed_acyclic_graph(G)`                                               | bool               |
|  [06]   | components    | `connected_components(G)`                                                    | set generator      |
|  [07]   | components    | `strongly_connected_components(G)`                                           | set generator      |
|  [08]   | spanning tree | `minimum_spanning_tree(G, weight='weight', algorithm='kruskal')`             | graph              |
|  [09]   | shortest path | `floyd_warshall(G, weight='weight')`                                         | dist-dict-of-dicts |
|  [10]   | components    | `weakly_connected_components(G)`                                             | set generator      |
|  [11]   | components    | `condensation(G, scc=None)`                                                  | DAG of SCCs        |
|  [12]   | DAG           | `descendants(G, source)` / `ancestors(G, source)`                           | node set           |
|  [13]   | DAG           | `transitive_closure(G, reflexive=False)`                                     | graph              |
|  [14]   | cycles        | `simple_cycles(G, length_bound=None)` / `find_cycle(G, source=None)`         | cycle generator    |
|  [15]   | traversal     | `dfs_tree(G, source=None, depth_limit=None)` / `bfs_tree(G, source, ...)`   | tree graph         |
|  [16]   | centrality    | `pagerank(G, alpha=0.85, personalization=None, max_iter=100, tol=1e-06, nstart=None, weight='weight', dangling=None)` | node-score dict |
|  [17]   | centrality    | `betweenness_centrality(G, k=None, normalized=True, weight=None, endpoints=False, seed=None)` | node-score dict |
|  [18]   | centrality    | `eigenvector_centrality` / `closeness_centrality` / `degree_centrality`     | node-score dict    |
|  [19]   | flow          | `maximum_flow(flowG, _s, _t, capacity='capacity', flow_func=None)`           | (value, flow dict) |
|  [20]   | flow          | `min_cost_flow(G, ...)` / `network_simplex(G, ...)`                         | flow dict / cost   |
|  [21]   | community     | `nx.community.louvain_communities(G, weight='weight', resolution=1, threshold=1e-07, max_level=None, seed=None)` | list[set] |
|  [22]   | community     | `nx.community.greedy_modularity_communities(G, weight=None, resolution=1, cutoff=1, best_n=None)` | list[frozenset] |
|  [23]   | community     | `nx.community.girvan_newman(G, most_valuable_edge=None)` / `nx.community.modularity(G, communities, ...)` | community iterator / float |
|  [24]   | isomorphism   | `is_isomorphic(G1, G2, node_match=None, edge_match=None)`                    | bool               |
|  [25]   | relabel       | `relabel_nodes(G, mapping, copy=True)` / `convert_node_labels_to_integers(G, ...)` | graph        |
|  [26]   | set algebra   | `compose(G, H)` / `union(G, H)` / `disjoint_union(G, H)`                     | graph              |
|  [27]   | attributes    | `get_node_attributes(G, name)` / `set_node_attributes(G, values, name=None)` | dict / in-place   |

## [04]-[IMPLEMENTATION_LAW]

[GRAPH_TOPOLOGY]:
- The graph payload owner threads `create_using=` to select directedness and multiplicity; one entry discriminates on graph kind instead of branching per subtype.
- Node and edge attributes live in per-element dicts; `nodes`, `edges`, `adj`, and `degree` return live views over the graph.
- Directed classes add `successors`/`predecessors` and `in_edges`/`out_edges`; multi-edge classes key parallel edges through `new_edge_key`.
- `backend`/`**backend_kwargs` is the dispatch-backend axis; the receipt records the backend and never forks parallel call sites per backend.

[BACKEND_DISPATCH]:
- Every algorithm decorated `@nx._dispatchable` accepts `*, backend=None, **backend_kwargs`; `backend='cugraph'|'graphblas'|'parallel'|...` selects an installed entry-point backend without changing the call site.
- `nx.config` (a `NetworkXConfig`) owns process-global dispatch policy: `nx.config.backend_priority` (ordered fallback list), `nx.config.backends` (per-backend config namespaces), `nx.config.cache_converted_graphs`, `nx.config.fallback_to_nx`, `nx.config.warnings_to_ignore`.
- The graph rail sets dispatch policy once on `nx.config` at boundary scope and threads `backend=` only when one call must override the global priority; it never branches per-backend call sites.

[ALGORITHM_RAIL]:
- Algorithm failures raise typed `NetworkX*` exceptions on the `NetworkXException` base; the receipt captures the algorithm name plus the typed failure, and the domain rail translates the raised exception at the boundary into the Result rail.
- `shortest_path` discriminates `method='dijkstra'`/`'bellman-ford'` and source/target presence to return a single path, per-target dict, or all-pairs dict from one entry; `floyd_warshall` is the dense all-pairs alternative.
- The community family is namespaced under `nx.community` (alias of `networkx.algorithms.community`): `louvain_communities`/`greedy_modularity_communities`/`girvan_newman` partition, `modularity` scores a partition; partitions return `list[set]`/`list[frozenset]`/an iterator of nested community tuples.

[INTEGRATION]:
- node-link JSON is the canonical wire form: `node_link_data(G)` emits `{nodes, edges, ...}` (the `edges='edges'` key is the settled 3.x default — `links` is the removed legacy spelling, never pin it) and `node_link_graph(data, directed=, multigraph=)` rebuilds, discriminating graph kind from the two positional flags rather than a per-kind reader; `adjacency`/`cytoscape`/`tree` JSON are sibling shapes for the same round-trip.
- Tabular egress flows through `to_pandas_edgelist`/`to_pandas_adjacency`; columnar reframing into Arrow or Polars belongs to the columnar rail. `to_scipy_sparse_array`/`adjacency_matrix` hand the graph to the sparse-LA rail (`scipy.sparse`) for spectral/algebraic work, and `to_numpy_array` to the dense numeric rail.
- A `node_link_data` payload serializes through the `msgspec`/JSON codec rail as the canonical persisted graph document, keyed by the same content-identity discipline as other data payloads; the graph is never re-parsed from an ad-hoc edge schema.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `networkx`
- Owns: graph payload classes, conversion bridges, file-format/JSON codecs, algorithm families, the `nx.community` detection namespace, and the `nx.config` backend-dispatch layer
- Accept: graph kind discrimination via `create_using` and `node_link_graph(directed=, multigraph=)`; tabular/array/dict/sparse bridges; file and JSON codecs; algorithm receipts on the typed exception base; backend selection via `backend=`/`nx.config.backend_priority`
- Reject: wrapper-renames, per-graph-kind parallel entrypoints, per-backend forked call sites, weaker local reimplementation of supplied algorithms, pinning the removed `edges='links'` node-link key, and product graph-database or route-discovery state
