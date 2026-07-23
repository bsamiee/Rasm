# [PY_DATA_API_IGRAPH]

`igraph` owns the libigraph C-core graph surface for the data graph rail: one `Graph` container carrying the `community_*` detection family, modularity scoring, component and core decomposition, and C-core centrality, with `VertexClustering`/`VertexDendrogram` result carriers and the `compare_communities` partition-distance surface. Data graph detection routes through `Graph.community_leiden`/`community_multilevel`/`community_infomap`, reads `VertexClustering.membership`/`modularity` as the receipt, and folds the C-core modularity optimization rather than re-implementing it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `igraph`
- package: `igraph` (GPL)
- module: `import igraph`
- rail: graph — the data-branch community-detection and C-core graph engine
- asset: native C extension (`igraph._igraph`, libigraph C core) over a `GraphBase` base
- entry points: console script `igraph` (interactive shell); library use is import-only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph container and clustering carriers

`Graph` owns the `community_*` family over 0-based contiguous vertex and edge indices; flat algorithms return a `VertexClustering`, hierarchical algorithms a `VertexDendrogram` cut to a `VertexClustering` via `as_clustering`. `Clustering`/`Cover` are the abstract roots; `VertexClustering`/`VertexCover` bind a `Graph` and add `subgraph`/`giant`/`crossing`.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [CAPABILITY]                    |
| :-----: | :------------------------ | :---------------- | :------------------------------ |
|  [01]   | `igraph.Graph`            | container         | C-core graph, detection surface |
|  [02]   | `igraph.GraphBase`        | container base    | libigraph C-extension base      |
|  [03]   | `igraph.VertexClustering` | clustering result | flat partition carrier          |
|  [04]   | `igraph.VertexDendrogram` | clustering result | hierarchical merge carrier      |
|  [05]   | `igraph.Clustering`       | clustering base   | abstract flat membership root   |
|  [06]   | `igraph.Dendrogram`       | clustering base   | abstract merge-tree root        |
|  [07]   | `igraph.VertexCover`      | overlap result    | overlapping partition carrier   |
|  [08]   | `igraph.Cover`            | overlap base      | abstract overlap root           |
|  [09]   | `igraph.VertexSeq`        | sequence          | vertex view, attribute access   |
|  [10]   | `igraph.EdgeSeq`          | sequence          | edge view, attribute access     |
|  [11]   | `igraph.Vertex`           | element           | single vertex proxy             |
|  [12]   | `igraph.Edge`             | element           | single edge proxy               |
|  [13]   | `igraph.ARPACKOptions`    | solver options    | ARPACK eigensolver tuning       |
|  [14]   | `igraph.InternalError`    | error             | libigraph C-core failure        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graph` community detection

Detection methods bind on `Graph` as instance methods; `weights` accepts an edge-attribute name or a per-edge sequence. `community_multilevel` is Louvain; `community_leiden` runs refined Leiden with `objective_function` selecting `CPM` or `modularity`.

| [INDEX] | [SURFACE]                                                                                             | [CAPABILITY]             |
| :-----: | :---------------------------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `community_leiden(objective_function, weights, resolution=1.0, n_iterations=2) -> VertexClustering`   | refined Leiden           |
|  [02]   | `community_multilevel(weights=None, return_levels=False, resolution=1) -> VertexClustering`           | Louvain multilevel       |
|  [03]   | `community_infomap(edge_weights=None, vertex_weights=None, trials=10) -> VertexClustering`            | Infomap flow             |
|  [04]   | `community_fastgreedy(weights=None) -> VertexDendrogram`                                              | fast-greedy modularity   |
|  [05]   | `community_walktrap(weights=None, steps=4) -> VertexDendrogram`                                       | random-walk merges       |
|  [06]   | `community_edge_betweenness(clusters=None, directed=True, weights=None) -> VertexDendrogram`          | Girvan-Newman splits     |
|  [07]   | `community_label_propagation(weights=None, initial=None, fixed=None) -> VertexClustering`             | label propagation        |
|  [08]   | `community_leading_eigenvector(clusters=None, weights=None, arpack_options=None) -> VertexClustering` | leading eigenvector      |
|  [09]   | `community_optimal_modularity(*args, **kwds) -> VertexClustering`                                     | exact modularity optimum |
|  [10]   | `community_spinglass(*args, **kwds) -> VertexClustering`                                              | spinglass                |
|  [11]   | `community_voronoi(lengths=None, weights=None, mode='out', radius=None) -> VertexClustering`          | Voronoi partition        |
|  [12]   | `community_fluid_communities(no_of_communities) -> VertexClustering`                                  | fluid communities        |

[ENTRYPOINT_SCOPE]: `Graph` general accessors

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `modularity(membership, weights=None, resolution=1, directed=True) -> float` | instance | membership modularity score   |
|  [02]   | `vcount() -> int`                                                            | instance | vertex count                  |
|  [03]   | `ecount() -> int`                                                            | instance | edge count                    |
|  [04]   | `is_directed() -> bool`                                                      | instance | directedness predicate        |
|  [05]   | `has_multiple() -> bool`                                                     | instance | parallel-edge predicate       |
|  [06]   | `add_vertices(n, attributes=None)`                                           | instance | append vertices in place      |
|  [07]   | `add_edges(es, attributes=None)`                                             | instance | append edges in place         |
|  [08]   | `write_graphml(f)`                                                           | instance | write GraphML to file         |
|  [09]   | `to_networkx(create_using=None)`                                             | instance | export to networkx            |
|  [10]   | `vs`                                                                         | property | vertex sequence (`VertexSeq`) |
|  [11]   | `es`                                                                         | property | edge sequence (`EdgeSeq`)     |

- `g.vs['name']` recovers the `vertex_name_attr` a `TupleList` build stored — the seam back to source vertex ids.

[ENTRYPOINT_SCOPE]: clustering result inspection

`compare_communities`/`split_join_distance` accept either membership lists or `VertexClustering` objects.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `VertexClustering.membership`                                               | property | per-vertex cluster-id list         |
|  [02]   | `VertexClustering.modularity`                                               | property | partition modularity score         |
|  [03]   | `VertexClustering.sizes(*args)`                                             | instance | cluster size list                  |
|  [04]   | `VertexClustering.subgraph(idx)`                                            | instance | one cluster induced subgraph       |
|  [05]   | `VertexClustering.subgraphs()`                                              | instance | induced subgraph per cluster       |
|  [06]   | `VertexClustering.giant()`                                                  | instance | largest-cluster subgraph           |
|  [07]   | `VertexClustering.crossing()`                                               | instance | per-edge cross-cluster boolean     |
|  [08]   | `VertexClustering.cluster_graph(combine_vertices=None, combine_edges=None)` | instance | contracted cluster-level graph     |
|  [09]   | `VertexClustering.compare_to(other)`                                        | instance | distance to another clustering     |
|  [10]   | `VertexClustering.recalculate_modularity()`                                 | instance | recompute after membership edit    |
|  [11]   | `VertexClustering.as_cover()`                                               | instance | view as a `VertexCover`            |
|  [12]   | `VertexDendrogram.as_clustering(n=None)`                                    | instance | cut merges to a `VertexClustering` |
|  [13]   | `VertexDendrogram.optimal_count`                                            | property | modularity-optimal cluster count   |
|  [14]   | `compare_communities(comm1, comm2, method='vi', remove_none=False)`         | function | VI/NMI/RI/split-join distance      |
|  [15]   | `split_join_distance(comm1, comm2, remove_none=False)`                      | function | directed split-join pair           |

[ENTRYPOINT_SCOPE]: `Graph` construction

Construction classmethods each build a `Graph`.

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `TupleList(edges, directed=False, vertex_name_attr='name', edge_attrs=None, weights=False)` | build from `(u, v)` tuples               |
|  [02]   | `DataFrame(edges, directed=True, vertices=None, use_vids=True)`                             | build from a pandas edge frame           |
|  [03]   | `Adjacency(matrix, mode='directed', loops='once')`                                          | build from an adjacency matrix           |
|  [04]   | `Weighted_Adjacency(matrix, mode='directed', attr='weight', loops='once')`                  | build from a weighted adjacency matrix   |
|  [05]   | `Biadjacency(matrix)`                                                                       | build a bipartite graph                  |
|  [06]   | `Realize_Degree_Sequence(out, in_=None)`                                                    | build from a degree sequence             |
|  [07]   | `Read(f, format=None)`                                                                      | read GraphML/GML/edgelist/Pajek/NCOL/LGL |
|  [08]   | `Famous(name)`                                                                              | load a named reference graph             |
|  [09]   | `Erdos_Renyi(n, p, m, directed=False, loops=False)`                                         | G(n,p)/G(n,m) random graph               |
|  [10]   | `Barabasi(n, m, power=1, zero_appeal=1)`                                                    | scale-free preferential attachment       |

- `Read` dispatches to format readers `Read_GraphML` `Read_Pickle` `Read_Edgelist` `Read_Ncol` `Read_Pajek`.

[ENTRYPOINT_SCOPE]: component decomposition, centrality, and egress

All rows are instance methods on `Graph`.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `connected_components(mode='strong')`                                              | `VertexClustering` by reachability |
|  [02]   | `decompose(mode='strong', maxcompno=None, minelements=1)`                          | list of component `Graph`s         |
|  [03]   | `k_core(*args)`                                                                    | k-core subgraph(s)                 |
|  [04]   | `coreness(mode='all')`                                                             | per-vertex coreness vector         |
|  [05]   | `pagerank(vertices=None, directed=True, damping=0.85)`                             | per-vertex PageRank vector         |
|  [06]   | `betweenness(vertices=None, directed=True, cutoff=None, weights=None)`             | per-vertex betweenness vector      |
|  [07]   | `closeness(vertices=None, mode='all', cutoff=None, weights=None, normalized=True)` | per-vertex closeness vector        |
|  [08]   | `distances(source=None, target=None, weights=None, mode='out', algorithm='auto')`  | shortest-path distance matrix      |
|  [09]   | `induced_subgraph(vertices, implementation='auto')`                                | community-subset induced subgraph  |
|  [10]   | `get_edge_dataframe()`                                                             | pandas edge frame (egress)         |
|  [11]   | `get_vertex_dataframe()`                                                           | pandas vertex frame (egress)       |
|  [12]   | `simplify(multiple=True, loops=True, combine_edges=None)`                          | drop parallel edges / self-loops   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Graph` owns construction, mutation, and the `community_*` detection family; algorithm selection is a method row, never a per-algorithm detector class.
- Flat algorithms return a `VertexClustering`; hierarchical algorithms (`community_fastgreedy`/`community_walktrap`/`community_edge_betweenness`) return a `VertexDendrogram` cut via `as_clustering`; partition state reads off `membership`/`modularity`/`sizes`.
- `Graph.modularity` scores an external membership against the C-core; `community_optimal_modularity` is the exact-optimum row; resolution rides the `resolution` argument.
- `compare_communities` is the single partition-distance surface keyed by `method`; `split_join_distance` is the directed split-join row.
- `connected_components`/`decompose` partition by reachability — choose them for connectivity, `community_*` for community; `k_core`/`coreness` own core decomposition; `pagerank`/`betweenness`/`closeness`/`distances` are the C-core ranking surface.
- Each detection captures algorithm, cluster count, per-cluster sizes, membership vector, modularity, and resolution as a graph receipt.

[STACKING]:
- `pandas`(`.api/pandas.md`): `Graph.DataFrame` builds the container from an edge frame at ingress, `get_edge_dataframe`/`get_vertex_dataframe` return the frames at egress, and a `VertexClustering.membership` vector joins back as a vertex column.
- data graph owner: build `Graph` from the pandas edge frame, run `community_leiden`, read `VertexClustering.membership`/`modularity`, and fold the membership onto the downstream frame in one pass with no Python re-walk of the C-core result.

[LOCAL_ADMISSION]:
- `igraph` carries a GPL libigraph C core; confine it to the data graph rail, and route a plugin-distributed graph to the Apache `rustworkx`(`.api/rustworkx.md`) sibling.
- Import at boundary scope; the admitted surface is C-core detection, modularity, centrality, and component split, with live drawing (`plot`/Cairo/Matplotlib) outside the rail.

[RAIL_LAW]:
- Package: `igraph`
- Owns: libigraph C-core graph containers, the `community_*` detection family, modularity scoring, `connected_components`/`decompose` component split, `k_core`/`coreness` decomposition, `pagerank`/`betweenness`/`closeness`/`distances` centrality, `VertexClustering`/`VertexDendrogram` carriers, and `compare_communities` partition comparison
- Accept: `Graph.community_leiden`/`community_multilevel`/`community_infomap` for detection, `VertexClustering.membership`/`modularity` as the receipt, `compare_communities` keyed by `method`, `Graph.DataFrame`/`TupleList` at construction, and `get_*_dataframe`/`to_networkx` at egress
- Reject: a hand-rolled modularity optimization or Leiden/Louvain loop the C core owns, a per-algorithm detector type where the method row covers it, a re-implemented partition-distance metric where `compare_communities` carries `method`, a Python re-walk of centrality or components the C core computes, and the GPL C core linked into a host-distributed plugin
