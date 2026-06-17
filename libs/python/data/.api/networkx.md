# [PY_DATA_API_NETWORKX]

`networkx` API capture for `data`, reflected from the installed distribution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `networkx`
- package: `networkx`
- import: `import networkx as nx`
- owner: `data`
- rail: graph
- capability: graph payload classes, conversion bridges, file-format codecs, and algorithm families over directed, undirected, and multi-edge graphs

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `nx.Graph` — undirected simple graph; node and edge attribute dicts; views `.nodes`, `.edges`, `.adj`, `.degree`.
- `nx.DiGraph` — directed simple graph; adds `.successors`, `.predecessors`, `.in_degree`, `.out_degree`, `.in_edges`, `.out_edges`.
- `nx.MultiGraph` — undirected parallel-edge graph keyed by edge key.
- `nx.MultiDiGraph` — directed parallel-edge graph keyed by edge key.

[GRAPH_MEMBERS]: (shared across the four classes)
- mutation: `add_node`, `add_nodes_from`, `add_edge`, `add_edges_from`, `remove_node`, `remove_edge`, `clear`.
- inspection: `has_node`, `has_edge`, `number_of_nodes`, `number_of_edges`, `nodes`, `edges`, `adj`, `degree`.
- derivation: `subgraph`, `copy`, `to_directed`, `to_undirected`.

[ENTRYPOINTS]:
- tabular bridge: `from_pandas_edgelist(df, source='source', target='target', edge_attr=None, create_using=None, edge_key=None, *, backend=None, **backend_kwargs)`, `to_pandas_edgelist(G, source='source', target='target', nodelist=None, dtype=None, edge_key=None, *, backend=None, **backend_kwargs)`, `from_pandas_adjacency`, `to_pandas_adjacency`.
- array bridge: `from_numpy_array`, `to_numpy_array`, `from_scipy_sparse_array`, `to_scipy_sparse_array`.
- dict/list bridge: `from_dict_of_dicts`, `to_dict_of_dicts`, `from_dict_of_lists`, `to_dict_of_lists`, `from_edgelist`, `to_edgelist`, `to_networkx_graph`.
- file codecs (read/write/parse/generate): `adjlist`, `multiline_adjlist`, `edgelist`, `weighted_edgelist`, `gexf`, `gml`, `graphml` (`write_graphml_lxml`, `write_graphml_xml`), `pajek`, `leda`, `graph6`, `sparse6`, `network_text`, `latex` (`to_latex`, `to_latex_raw`).
- prufer/nested codecs: `from_prufer_sequence`, `to_prufer_sequence`, `from_nested_tuple`, `to_nested_tuple`.
- algorithm families: shortest-path (`shortest_path`, `shortest_path_length`, `all_pairs_shortest_path`, `dijkstra_path`, `astar_path`); DAG (`is_directed_acyclic_graph`, `topological_sort`, `descendants`, `ancestors`); components (`connected_components`, `strongly_connected_components`); trees (`minimum_spanning_tree`); centrality (`pagerank`, `betweenness_centrality`); flow (`maximum_flow`).

[EXCEPTIONS]:
- `networkx.exception.__all__` taxonomy: `NetworkXException` (base) -> `NetworkXError`; `NetworkXAlgorithmError`; `NetworkXNoPath` (under `NetworkXUnfeasible`); `NetworkXNoCycle` (under `NetworkXUnfeasible`); `NetworkXUnfeasible` (under `NetworkXAlgorithmError`); `NetworkXUnbounded` (under `NetworkXAlgorithmError`); `NetworkXNotImplemented`; `NetworkXPointlessConcept`; `AmbiguousSolution`; `ExceededMaxIterations` -> `PowerIterationFailedConvergence`; `HasACycle` (under `NetworkXException`); `NodeNotFound`.
- algorithm-local `NetworkX*` raised through lazy `nx.*` resolution and `NetworkXException`-derived: `NetworkXTreewidthBoundExceeded` (`networkx.algorithms.chordal`).

[IMPLEMENTATION_LAW]:
- The graph payload owner threads `create_using=` to select directedness and multiplicity instead of branching on graph subtype; one entry discriminates on graph kind.
- Algorithm receipts capture the algorithm name plus the typed `NetworkX*` failure on the `NetworkXException` base; domain rails translate the raised exception at the boundary into the Result rail and never propagate it.
- Tabular egress flows through `to_pandas_edgelist`/`to_pandas_adjacency` at the boundary; columnar reframing into Arrow/Polars is owned by the columnar rail, not re-minted here.
- `backend=`/`**backend_kwargs` is the dispatch-backend axis; the owner records the backend in the receipt and never forks parallel call sites per backend.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `networkx`
- Owns: graph payload classes, conversion bridges, file-format codecs, and algorithm families
- Accept: graph kind discrimination via `create_using`, tabular/array/dict bridges, file codecs, algorithm receipts on the typed exception base
- Reject: wrapper-renames, per-graph-kind parallel entrypoints, weaker local reimplementation of supplied algorithms, and product graph-database or route-discovery state
