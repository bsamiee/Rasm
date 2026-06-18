# [PY_DATA_API_NETWORKX]

`networkx` supplies `Graph`, `DiGraph`, `MultiGraph`, and `MultiDiGraph` payload classes with shared mutation, inspection, and derivation members, plus tabular/array/dict conversion bridges, file-format codecs, and shortest-path, DAG, component, spanning-tree, centrality, and flow algorithm families. The `create_using` argument discriminates graph kind across one polymorphic call, and `backend`/`**backend_kwargs` select the dispatch backend.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `networkx`
- package: `networkx`
- import: `import networkx as nx`
- owner: `data`
- rail: graph
- capability: graph payload classes, conversion bridges, file-format codecs, and algorithm families over directed, undirected, and multi-edge graphs

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph payload classes
- rail: graph
- shared members carry across all four classes; directed and multi-edge classes add the extra members noted

| [INDEX] | [SYMBOL]          | [GRAPH_KIND]          | [ADDED_MEMBERS]                                       |
| :-----: | :---------------- | :-------------------- | :---------------------------------------------------- |
|   [1]   | `nx.Graph`        | undirected simple     | base mutation, inspection, derivation members         |
|   [2]   | `nx.DiGraph`      | directed simple       | `successors`, `predecessors`, `in_edges`, `out_edges` |
|   [3]   | `nx.MultiGraph`   | undirected multi-edge | `new_edge_key`, `edge_key_dict_factory`               |
|   [4]   | `nx.MultiDiGraph` | directed multi-edge   | directed members plus `new_edge_key`                  |

[PUBLIC_TYPE_SCOPE]: shared graph members
- rail: graph

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                         |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | mutation        | `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `remove_node`, `remove_edge`, `clear` |
|   [2]   | inspection      | `has_node`, `has_edge`, `number_of_nodes`, `number_of_edges`, `nodes`, `edges`, `adj`, `degree`   |
|   [3]   | derivation      | `subgraph`, `edge_subgraph`, `copy`, `to_directed`, `to_undirected`                               |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- rail: graph
- every member derives from `NetworkXException`; domain rails translate at the boundary

| [INDEX] | [SYMBOL]                          | [PARENT]                 | [FAILURE]                        |
| :-----: | :-------------------------------- | :----------------------- | :------------------------------- |
|   [1]   | `NetworkXException`               | `Exception`              | base of the taxonomy             |
|   [2]   | `NetworkXError`                   | `NetworkXException`      | invalid graph argument           |
|   [3]   | `NetworkXAlgorithmError`          | `NetworkXException`      | algorithm precondition failure   |
|   [4]   | `NetworkXUnfeasible`              | `NetworkXAlgorithmError` | no feasible solution             |
|   [5]   | `NetworkXNoPath`                  | `NetworkXUnfeasible`     | no path between endpoints        |
|   [6]   | `NetworkXNoCycle`                 | `NetworkXUnfeasible`     | no cycle present                 |
|   [7]   | `NetworkXUnbounded`               | `NetworkXAlgorithmError` | unbounded optimisation           |
|   [8]   | `NetworkXNotImplemented`          | `NetworkXException`      | unsupported graph kind           |
|   [9]   | `NetworkXPointlessConcept`        | `NetworkXException`      | operation undefined for input    |
|  [10]   | `AmbiguousSolution`               | `NetworkXException`      | non-unique solution              |
|  [11]   | `ExceededMaxIterations`           | `NetworkXException`      | iteration cap reached            |
|  [12]   | `PowerIterationFailedConvergence` | `ExceededMaxIterations`  | eigen iteration did not converge |
|  [13]   | `HasACycle`                       | `NetworkXException`      | cycle where acyclicity required  |
|  [14]   | `NodeNotFound`                    | `NetworkXException`      | node absent from graph           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: conversion bridges
- rail: graph
- `create_using` selects target graph kind; `backend`/`**backend_kwargs` select dispatch backend

| [INDEX] | [SURFACE]                                                           | [BRIDGE]  | [PEER_DIRECTION]                |
| :-----: | :------------------------------------------------------------------ | :-------- | :------------------------------ |
|   [1]   | `from_pandas_edgelist(df, source='source', target='target', ...)`   | tabular   | `to_pandas_edgelist(G, ...)`    |
|   [2]   | `from_pandas_adjacency(df, create_using=None, ...)`                 | tabular   | `to_pandas_adjacency(G, ...)`   |
|   [3]   | `from_numpy_array(A, parallel_edges=False, create_using=None, ...)` | array     | `to_numpy_array(G, ...)`        |
|   [4]   | `from_scipy_sparse_array(A, parallel_edges=False, ...)`             | array     | `to_scipy_sparse_array(G, ...)` |
|   [5]   | `from_dict_of_dicts(d, create_using=None, multigraph_input=False)`  | dict      | `to_dict_of_dicts(G, ...)`      |
|   [6]   | `from_dict_of_lists(d, create_using=None, ...)`                     | dict      | `to_dict_of_lists(G, ...)`      |
|   [7]   | `from_edgelist(edgelist, create_using=None, ...)`                   | edge list | `to_edgelist(G, ...)`           |

[ENTRYPOINT_SCOPE]: file-format codecs
- rail: graph
- each codec family exposes `read_*`, `write_*`, `parse_*`, and `generate_*` where applicable

| [INDEX] | [FORMAT]            | [READ]                   | [WRITE]                                                     |
| :-----: | :------------------ | :----------------------- | :---------------------------------------------------------- |
|   [1]   | adjacency list      | `read_adjlist`           | `write_adjlist`                                             |
|   [2]   | multiline adjacency | `read_multiline_adjlist` | `write_multiline_adjlist`                                   |
|   [3]   | edge list           | `read_edgelist`          | `write_edgelist`                                            |
|   [4]   | weighted edge list  | `read_weighted_edgelist` | `write_weighted_edgelist`                                   |
|   [5]   | GEXF                | `read_gexf`              | `write_gexf`                                                |
|   [6]   | GML                 | `read_gml`               | `write_gml`                                                 |
|   [7]   | GraphML             | `read_graphml`           | `write_graphml` (`write_graphml_lxml`, `write_graphml_xml`) |
|   [8]   | Pajek               | `read_pajek`             | `write_pajek`                                               |
|   [9]   | LEDA                | `read_leda`              | n/a                                                         |
|  [10]   | graph6              | `read_graph6`            | `write_graph6`                                              |
|  [11]   | sparse6             | `read_sparse6`           | `write_sparse6`                                             |
|  [12]   | network text        | `generate_network_text`  | `write_network_text`                                        |
|  [13]   | node-link JSON      | `node_link_graph`        | `node_link_data`                                            |

[ENTRYPOINT_SCOPE]: algorithm families
- rail: graph
- every algorithm accepts `*, backend=None, **backend_kwargs`

| [INDEX] | [FAMILY]      | [ENTRY]                                                                      | [RESULT]           |
| :-----: | :------------ | :--------------------------------------------------------------------------- | :----------------- |
|   [1]   | shortest path | `shortest_path(G, source=None, target=None, weight=None, method='dijkstra')` | path or path dict  |
|   [2]   | shortest path | `dijkstra_path(G, source, target, weight='weight')`                          | node list          |
|   [3]   | shortest path | `astar_path(G, source, target, heuristic=None, weight='weight')`             | node list          |
|   [4]   | DAG           | `topological_sort(G)`                                                        | node generator     |
|   [5]   | DAG           | `is_directed_acyclic_graph(G)`                                               | bool               |
|   [6]   | components    | `connected_components(G)`                                                    | set generator      |
|   [7]   | components    | `strongly_connected_components(G)`                                           | set generator      |
|   [8]   | spanning tree | `minimum_spanning_tree(G, weight='weight', algorithm='kruskal')`             | graph              |
|   [9]   | centrality    | `pagerank(G, alpha=0.85, max_iter=100, tol=1e-06, weight='weight')`          | node-score dict    |
|  [10]   | centrality    | `betweenness_centrality(G, k=None, normalized=True, weight=None)`            | node-score dict    |
|  [11]   | flow          | `maximum_flow(flowG, _s, _t, capacity='capacity', flow_func=None)`           | (value, flow dict) |

## [4]-[IMPLEMENTATION_LAW]

[GRAPH_TOPOLOGY]:
- The graph payload owner threads `create_using=` to select directedness and multiplicity; one entry discriminates on graph kind instead of branching per subtype.
- Node and edge attributes live in per-element dicts; `nodes`, `edges`, `adj`, and `degree` return live views over the graph.
- Directed classes add `successors`/`predecessors` and `in_edges`/`out_edges`; multi-edge classes key parallel edges through `new_edge_key`.
- `backend`/`**backend_kwargs` is the dispatch-backend axis; the receipt records the backend and never forks parallel call sites per backend.

[ALGORITHM_RAIL]:
- Algorithm failures raise typed `NetworkX*` exceptions on the `NetworkXException` base; the receipt captures the algorithm name plus the typed failure, and the domain rail translates the raised exception at the boundary into the Result rail.
- `shortest_path` discriminates `method='dijkstra'`/`'bellman-ford'` and source/target presence to return a single path, per-target dict, or all-pairs dict from one entry.
- Tabular egress flows through `to_pandas_edgelist`/`to_pandas_adjacency` at the boundary; columnar reframing into Arrow or Polars belongs to the columnar rail.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `networkx`
- Owns: graph payload classes, conversion bridges, file-format codecs, and algorithm families
- Accept: graph kind discrimination via `create_using`, tabular/array/dict bridges, file codecs, algorithm receipts on the typed exception base
- Reject: wrapper-renames, per-graph-kind parallel entrypoints, weaker local reimplementation of supplied algorithms, and product graph-database or route-discovery state
