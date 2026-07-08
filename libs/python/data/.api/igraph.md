# [PY_DATA_API_IGRAPH]

`igraph` supplies the C-core graph surface for the data graph rail: a single `Graph` container that exposes the full `community_*` detection family (Leiden, Louvain/multilevel, Infomap, fast-greedy, walktrap, edge-betweenness, label-propagation, leading-eigenvector, optimal-modularity, spinglass, voronoi, fluid) over the libigraph C library, plus the `VertexClustering`/`VertexDendrogram` result carriers and `compare_communities`/`split_join_distance` partition-distance functions. The package owner routes GRAPH_COMMUNITY through `Graph.community_leiden`, `Graph.community_multilevel`, and `Graph.community_infomap`, reads `VertexClustering.membership`/`modularity` as the receipt, and never re-implements the C-core modularity optimization the library already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `igraph`
- package: `igraph` (`python-igraph` on PyPI)
- version: `1.0.0`
- license: GPL-release (libigraph C core is GPL; admitting `igraph` makes the build GPL — keep the dependency confined to the `data` graph rail, never linked into a host-distributed plugin)
- module: `import igraph`
- owner: `data`
- rail: graph
- asset: native C extension (`igraph._igraph`, libigraph C core) over a `GraphBase` base; `igraph.version()` is a callable (the `__version__` string is `1.0.0`)
- entry points: console script `igraph` (interactive shell); library use is import-only
- capability: undirected/directed/weighted graph containers backed by the libigraph C core; Leiden/Louvain/Infomap/fast-greedy/walktrap/edge-betweenness/label-propagation/leading-eigenvector/optimal-modularity/spinglass/voronoi/fluid community detection; `VertexClustering` flat partitions and `VertexDendrogram` hierarchical merges; modularity scoring and `recalculate_modularity`; k-core/`coreness` decomposition; `connected_components`/`decompose` component split; PageRank/betweenness/closeness centrality and `distances` shortest-path matrix; VI/NMI/RI/split-join partition comparison; edge-list/DataFrame/adjacency/famous/random-model construction; `to_networkx`/`get_edge_dataframe`/`get_vertex_dataframe` round-trip to the pandas/networkx siblings

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph container and clustering carriers
- rail: graph
- vertex/edge indices are 0-based contiguous integers; `community_*` methods are bound on `Graph` and return a `VertexClustering` or `VertexDendrogram`

`Graph` owns the `community_*` detection family; modularity-flat algorithms return a `VertexClustering`, while hierarchical algorithms return a `VertexDendrogram` whose `as_clustering` cut yields a `VertexClustering`. `Clustering`/`Cover` are the abstract membership/overlap roots; `VertexClustering`/`VertexCover` bind a `Graph` and add `subgraph`/`giant`/`crossing`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `igraph.Graph` | container | C-core graph with the full `community_*` and `modularity` surface |
| [02] | `igraph.GraphBase` | container base | libigraph C extension base under `Graph` |
| [03] | `igraph.VertexClustering` | clustering result | flat vertex partition with `membership`/`modularity`/`subgraphs` |
| [04] | `igraph.VertexDendrogram` | clustering result | hierarchical merges with `as_clustering`/`optimal_count` |
| [05] | `igraph.Clustering` | clustering base | abstract flat membership root |
| [06] | `igraph.Dendrogram` | clustering base | abstract merge-tree root |
| [07] | `igraph.VertexCover` | overlap result | overlapping vertex partition bound to a `Graph` |
| [08] | `igraph.Cover` | overlap base | abstract overlapping membership root |
| [09] | `igraph.VertexSeq` | sequence | vertex view with attribute access |
| [10] | `igraph.EdgeSeq` | sequence | edge view with attribute access |
| [11] | `igraph.Vertex` | element | single vertex proxy |
| [12] | `igraph.Edge` | element | single edge proxy |
| [13] | `igraph.ARPACKOptions` | solver options | ARPACK eigensolver tuning for leading-eigenvector |
| [14] | `igraph.InternalError` | error | libigraph C-core failure |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graph` community detection
- rail: graph
- methods are bound on `Graph` (the leading `graph` parameter is `self`); `weights` accepts an edge-attribute name or a per-edge sequence

`community_multilevel` is the Louvain implementation; `community_leiden` is the refined Leiden algorithm with `objective_function` selecting `CPM` or `modularity`. Modularity-flat algorithms return a `VertexClustering`; `community_fastgreedy`, `community_walktrap`, and `community_edge_betweenness` return a `VertexDendrogram` cut via `as_clustering`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [RESULT] |
| --- | --- | --- | --- |
| [01] | `Graph.community_leiden` | `community_leiden(objective_function='CPM', weights=None, resolution=1.0, beta=0.01, initial_membership=None, n_iterations=2, node_weights=None, node_in_weights=None, **kwds)` | `VertexClustering` |
| [02] | `Graph.community_multilevel` | `community_multilevel(weights=None, return_levels=False, resolution=1)` | `VertexClustering` |
| [03] | `Graph.community_infomap` | `community_infomap(edge_weights=None, vertex_weights=None, trials=10)` | `VertexClustering` |
| [04] | `Graph.community_fastgreedy` | `community_fastgreedy(weights=None)` | `VertexDendrogram` |
| [05] | `Graph.community_walktrap` | `community_walktrap(weights=None, steps=4)` | `VertexDendrogram` |
| [06] | `Graph.community_edge_betweenness` | `community_edge_betweenness(clusters=None, directed=True, weights=None)` | `VertexDendrogram` |
| [07] | `Graph.community_label_propagation` | `community_label_propagation(weights=None, initial=None, fixed=None)` | `VertexClustering` |
| [08] | `Graph.community_leading_eigenvector` | `community_leading_eigenvector(clusters=None, weights=None, arpack_options=None)` | `VertexClustering` |
| [09] | `Graph.community_optimal_modularity` | `community_optimal_modularity(*args, **kwds)` | `VertexClustering` |
| [10] | `Graph.community_spinglass` | `community_spinglass(*args, **kwds)` | `VertexClustering` |
| [11] | `Graph.community_voronoi` | `community_voronoi(lengths=None, weights=None, mode='out', radius=None)` | `VertexClustering` |
| [12] | `Graph.community_fluid_communities` | `community_fluid_communities(no_of_communities)` | `VertexClustering` |
| [13] | `Graph.modularity` | `modularity(membership, weights=None, resolution=1, directed=True)` | `float` |
| [14] | `Graph.k_core` | `k_core(*args)` | `Graph` or list |
| [15] | `Graph.vcount` / `Graph.ecount` | `vcount() -> int`; `ecount() -> int` | vertex / edge count |
| [16] | `Graph.is_directed` / `Graph.has_multiple` | `is_directed() -> bool`; `has_multiple() -> bool` | directedness / parallel-edge predicates |
| [17] | `Graph.TupleList` | `TupleList(edges, directed=False, vertex_name_attr='name', edge_attrs=None, weights=False) -> Graph` (classmethod) | build a `Graph` from an `(u, v)` edge-tuple iterable |
| [18] | `Graph.to_networkx` / `Graph.get_edge_dataframe` | `to_networkx(create_using=None) -> networkx.Graph`; `get_edge_dataframe() -> pandas.DataFrame` | round-trip to the networkx / pandas siblings |
| [19] | `Graph.write_graphml` | `write_graphml(f) -> None` | write the graph as GraphML to a path/file |
| [20] | `Graph.add_vertices` / `Graph.add_edges` | `add_vertices(n, attributes=None) -> None`; `add_edges(es, attributes=None) -> None` | append `n` vertices (per-key `attributes` columns, e.g. `attributes={'name': [...]}`) / append edges to the live graph in place — re-admits edgeless indices as `name`-carrying singletons after a `TupleList` build |
| [21] | `Graph.vs` / `Graph.es` | `vs -> VertexSeq`; `es -> EdgeSeq` (property) | vertex / edge sequence; attribute access reads one column (`g.vs['name']` recovers the `vertex_name_attr` names a `TupleList` stored, the cross-index seam back to the source vertex id) |

[ENTRYPOINT_SCOPE]: clustering result inspection
- rail: graph

`VertexClustering` carries the flat partition receipt (`membership`, `modularity`, sizes, induced subgraphs); `VertexDendrogram` carries the hierarchical merges and cuts to a `VertexClustering` at `as_clustering`. `compare_communities`/`split_join_distance` compute partition distance over membership lists or `VertexClustering` objects.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `VertexClustering.membership` | property | per-vertex cluster-id list |
| [02] | `VertexClustering.modularity` | property | partition modularity score |
| [03] | `VertexClustering.sizes` | `sizes(*args)` | cluster size list |
| [04] | `VertexClustering.subgraph` | `subgraph(idx)` | induced subgraph for one cluster |
| [05] | `VertexClustering.subgraphs` | `subgraphs()` | induced subgraph per cluster |
| [06] | `VertexClustering.giant` | `giant()` | largest-cluster induced subgraph |
| [07] | `VertexClustering.crossing` | `crossing()` | per-edge cross-cluster boolean |
| [08] | `VertexClustering.cluster_graph` | `cluster_graph(combine_vertices=None, combine_edges=None)` | contracted cluster-level graph |
| [09] | `VertexClustering.compare_to` | `compare_to(other, *args, **kwds)` | partition distance against another clustering |
| [10] | `VertexClustering.recalculate_modularity` | `recalculate_modularity()` | recompute modularity after membership edit |
| [11] | `VertexClustering.as_cover` | `as_cover()` | view flat partition as a `VertexCover` |
| [12] | `VertexDendrogram.as_clustering` | `as_clustering(n=None)` | cut merges to a flat `VertexClustering` |
| [13] | `VertexDendrogram.optimal_count` | property | modularity-optimal cluster count |
| [14] | `compare_communities` | `compare_communities(comm1, comm2, method='vi', remove_none=False)` | VI/NMI/split-join/RI/ARI/Danon/meila partition distance |
| [15] | `split_join_distance` | `split_join_distance(comm1, comm2, remove_none=False)` | directed split-join distance pair |

[ENTRYPOINT_SCOPE]: `Graph` construction
- rail: graph
- construction classmethods build a `Graph` from edge data, frames, adjacency, or named generators

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Graph.TupleList` | `TupleList(edges, directed=False, vertex_name_attr='name', edge_attrs=None, weights=False)` | build from `(source, target, ...)` tuples |
| [02] | `Graph.DataFrame` | `DataFrame(edges, directed=True, vertices=None, use_vids=True)` | build from a pandas edge frame |
| [03] | `Graph.Adjacency` | `Adjacency(matrix, mode='directed', loops='once')` | build from an adjacency matrix |
| [04] | `Graph.Weighted_Adjacency` | `Weighted_Adjacency(matrix, mode='directed', attr='weight', loops='once')` | build from a weighted adjacency matrix |
| [05] | `Graph.Read` | `Read(f, format=None, *args, **kwds)` | read GraphML/GML/edgelist/Pajek/NCOL/LGL |
| [06] | `Graph.Famous` | `Famous(name)` | load a named reference graph |
| [07] | `Graph.Erdos_Renyi` | `Erdos_Renyi(n, p, m, directed=False, loops=False, edge_labeled=False)` | G(n,p)/G(n,m) random graph |
| [08] | `Graph.Barabasi` | `Barabasi(n, m, outpref=False, directed=False, power=1, zero_appeal=1, implementation='psumtree', start_from=None)` | scale-free preferential-attachment graph |
| [09] | `Graph.Biadjacency` | `Biadjacency(matrix, ...)` / `Graph.Realize_Degree_Sequence(out, in_=None, ...)` | bipartite / degree-sequence construction |
| [10] | `Graph.Read_GraphML` | `Read_GraphML(f, index=0)` / `Read_Pickle(fname)` / `Read_Edgelist` / `Read_Ncol` / `Read_Pajek` | format-keyed readers `Read` dispatches |

[ENTRYPOINT_SCOPE]: component decomposition, centrality, and pandas/networkx seam
- rail: graph
- component split is the structural sibling of community detection; centrality vectors feed downstream ranking; the pandas/networkx round-trip is the construction/egress boundary

`connected_components`/`decompose` partition by reachability (no modularity), returning a `VertexClustering`/list of `Graph`; `k_core`/`coreness` give the core decomposition. `pagerank`/`betweenness`/`closeness` and the `distances` matrix are the C-core centrality surface — never re-walked in Python. `DataFrame`/`get_edge_dataframe`/`get_vertex_dataframe` bridge the pandas sibling, `to_networkx` the networkx sibling.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [RESULT_CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Graph.connected_components` | `connected_components(mode='strong')` | `VertexClustering` by reachability |
| [02] | `Graph.decompose` | `decompose(mode='strong', maxcompno=None, minelements=1)` | list of induced component `Graph`s |
| [03] | `Graph.k_core` / `coreness` | `k_core(*args)` / `coreness(mode='all')` | k-core subgraph(s) / per-vertex coreness vector |
| [04] | `Graph.pagerank` | `pagerank(vertices=None, directed=True, damping=0.85, weights=None, arpack_options=None, implementation='prpack')` | per-vertex PageRank vector |
| [05] | `Graph.betweenness` | `betweenness(vertices=None, directed=True, cutoff=None, weights=None, sources=None, targets=None)` | per-vertex betweenness vector |
| [06] | `Graph.closeness` | `closeness(vertices=None, mode='all', cutoff=None, weights=None, normalized=True)` | per-vertex closeness vector |
| [07] | `Graph.distances` | `distances(source=None, target=None, weights=None, mode='out', algorithm='auto')` | shortest-path distance matrix |
| [08] | `Graph.get_edge_dataframe` | `get_edge_dataframe()` / `get_vertex_dataframe()` | pandas edge/vertex frames (egress seam) |
| [09] | `Graph.induced_subgraph` | `induced_subgraph(vertices, implementation='auto')` / `simplify(multiple=True, loops=True, combine_edges=None)` | community-subset subgraph / dedup |

## [04]-[IMPLEMENTATION_LAW]

[GRAPH_COMMUNITY]:
- import: `import igraph` at boundary scope only; module-level import is banned by the manifest import policy.
- container axis: one `Graph` owns construction, mutation, and the `community_*` detection family; `TupleList`/`DataFrame`/`Adjacency`/`Read` are construction rows, never a per-source builder type; the leading `graph` parameter in the bound method is `self`.
- detection axis: `community_leiden` (refined Leiden, `objective_function` selects `CPM`/`modularity`), `community_multilevel` (Louvain), and `community_infomap` are the GRAPH_COMMUNITY rows; algorithm selection is a method row, never a parallel detector class; `weights` accepts an edge-attribute name or per-edge sequence.
- result axis: modularity-flat algorithms return a `VertexClustering`; hierarchical algorithms (`community_fastgreedy`/`community_walktrap`/`community_edge_betweenness`) return a `VertexDendrogram` cut to a `VertexClustering` via `as_clustering`; partition state is read off `membership`/`modularity`/`sizes`, never hand-recomputed.
- scoring axis: `Graph.modularity` scores an external membership against the C-core; `community_optimal_modularity` is the exact-optimum row; resolution control is the `resolution` argument, never a forked algorithm.
- comparison axis: `compare_communities` is the single partition-distance surface keyed by `method` (`vi`/`nmi`/`split-join`/`rand`/`adjusted_rand`/`danon`/`meila`); `split_join_distance` is the directed split-join row; partition agreement is a `method` row, never a re-implemented metric.
- structural axis: `connected_components`/`decompose` partition by reachability (no modularity) and are the structural sibling of the `community_*` family — choose component split when the question is connectivity, not community; `k_core`/`coreness` are the core-decomposition rows.
- centrality axis: `pagerank`/`betweenness`/`closeness`/`distances` are the C-core ranking surface; a centrality vector is read off the bound method, never re-walked in Python over the membership.
- evidence: each detection captures algorithm name, cluster count, per-cluster sizes, membership vector, modularity score, and resolution as a graph receipt.
- boundary: igraph owns the libigraph C-core detection, modularity, centrality, and component split; the `Graph` container is built from edge tuples or a pandas frame at the boundary and the membership/centrality vector feeds downstream owners; live drawing (`plot`/Cairo/Matplotlib drawers) stays outside this rail. The GPL C core is confined to the `data` graph rail and never linked into a host-distributed plugin.

[RAIL_LAW]:
- Package: `igraph`
- Owns: C-core graph containers, the full `community_*` detection family (Leiden/Louvain/Infomap and the dendrogram algorithms), modularity scoring, `connected_components`/`decompose` component split, k-core/`coreness` decomposition, PageRank/betweenness/closeness centrality and the `distances` matrix, `VertexClustering`/`VertexDendrogram` result carriers, and VI/NMI/RI/split-join partition comparison
- Accept: `Graph.community_leiden`/`community_multilevel`/`community_infomap` for GRAPH_COMMUNITY, `VertexClustering.membership`/`modularity` as the receipt, `compare_communities` keyed by `method` for partition distance, `Graph.DataFrame`/`TupleList` at the construction boundary, and `get_*_dataframe`/`to_networkx` at the pandas/networkx egress seam
- Reject: wrapper-renames of `community_*`/`modularity`; a hand-rolled modularity-optimization or Leiden/Louvain loop the C core already owns; a parallel detector type per algorithm when the method row covers it; a re-implemented partition-distance metric where `compare_communities` carries the `method`; a Python re-walk of centrality/components the C core computes; identity minting the runtime owns; the GPL C core linked into a host-distributed plugin
