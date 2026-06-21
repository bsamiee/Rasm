# [PY_DATA_API_IGRAPH]

`igraph` supplies the C-core graph surface for the data graph rail: a single `Graph` container that exposes the full `community_*` detection family (Leiden, Louvain/multilevel, Infomap, fast-greedy, walktrap, edge-betweenness, label-propagation, leading-eigenvector, optimal-modularity, spinglass, voronoi, fluid) over the libigraph C library, plus the `VertexClustering`/`VertexDendrogram` result carriers and `compare_communities`/`split_join_distance` partition-distance functions. The package owner routes GRAPH_COMMUNITY through `Graph.community_leiden`, `Graph.community_multilevel`, and `Graph.community_infomap`, reads `VertexClustering.membership`/`modularity` as the receipt, and never re-implements the C-core modularity optimization the library already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `igraph`
- package: `igraph`
- import: `import igraph`
- owner: `data`
- rail: graph
- installed: `1.0.0` reflected via `python -c "import igraph"` on cp315
- entry points: console script `igraph` (interactive shell); library use is import-only
- capability: undirected/directed/weighted graph containers backed by the libigraph C core; Leiden/Louvain/Infomap/fast-greedy/walktrap/edge-betweenness/label-propagation/leading-eigenvector/optimal-modularity/spinglass/voronoi/fluid community detection; `VertexClustering` flat partitions and `VertexDendrogram` hierarchical merges; modularity scoring; k-core decomposition; VI/NMI/RI/split-join partition comparison; edge-list/DataFrame/adjacency/famous-graph construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph container and clustering carriers
- rail: graph
- vertex/edge indices are 0-based contiguous integers; `community_*` methods are bound on `Graph` and return a `VertexClustering` or `VertexDendrogram`

`Graph` owns the `community_*` detection family; modularity-flat algorithms return a `VertexClustering`, while hierarchical algorithms return a `VertexDendrogram` whose `as_clustering` cut yields a `VertexClustering`. `Clustering`/`Cover` are the abstract membership/overlap roots; `VertexClustering`/`VertexCover` bind a `Graph` and add `subgraph`/`giant`/`crossing`.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                                            |
| :-----: | :------------------------ | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `igraph.Graph`            | container         | C-core graph with the full `community_*` and `modularity` surface |
|  [02]   | `igraph.GraphBase`        | container base    | libigraph C extension base under `Graph`                          |
|  [03]   | `igraph.VertexClustering` | clustering result | flat vertex partition with `membership`/`modularity`/`subgraphs`  |
|  [04]   | `igraph.VertexDendrogram` | clustering result | hierarchical merges with `as_clustering`/`optimal_count`          |
|  [05]   | `igraph.Clustering`       | clustering base   | abstract flat membership root                                     |
|  [06]   | `igraph.Dendrogram`       | clustering base   | abstract merge-tree root                                          |
|  [07]   | `igraph.VertexCover`      | overlap result    | overlapping vertex partition bound to a `Graph`                   |
|  [08]   | `igraph.Cover`            | overlap base      | abstract overlapping membership root                              |
|  [09]   | `igraph.VertexSeq`        | sequence          | vertex view with attribute access                                 |
|  [10]   | `igraph.EdgeSeq`          | sequence          | edge view with attribute access                                   |
|  [11]   | `igraph.Vertex`           | element           | single vertex proxy                                               |
|  [12]   | `igraph.Edge`             | element           | single edge proxy                                                 |
|  [13]   | `igraph.ARPACKOptions`    | solver options    | ARPACK eigensolver tuning for leading-eigenvector                 |
|  [14]   | `igraph.InternalError`    | error             | libigraph C-core failure                                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graph` community detection
- rail: graph
- methods are bound on `Graph` (the leading `graph` parameter is `self`); `weights` accepts an edge-attribute name or a per-edge sequence

`community_multilevel` is the Louvain implementation; `community_leiden` is the refined Leiden algorithm with `objective_function` selecting `CPM` or `modularity`. Modularity-flat algorithms return a `VertexClustering`; `community_fastgreedy`, `community_walktrap`, and `community_edge_betweenness` return a `VertexDendrogram` cut via `as_clustering`.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                                                                                                                                    | [RESULT]           |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------------- |
|  [01]   | `Graph.community_leiden`              | `community_leiden(objective_function='CPM', weights=None, resolution=1.0, beta=0.01, initial_membership=None, n_iterations=2, node_weights=None, node_in_weights=None, **kwds)` | `VertexClustering` |
|  [02]   | `Graph.community_multilevel`          | `community_multilevel(weights=None, return_levels=False, resolution=1)`                                                                                                         | `VertexClustering` |
|  [03]   | `Graph.community_infomap`             | `community_infomap(edge_weights=None, vertex_weights=None, trials=10)`                                                                                                          | `VertexClustering` |
|  [04]   | `Graph.community_fastgreedy`          | `community_fastgreedy(weights=None)`                                                                                                                                            | `VertexDendrogram` |
|  [05]   | `Graph.community_walktrap`            | `community_walktrap(weights=None, steps=4)`                                                                                                                                     | `VertexDendrogram` |
|  [06]   | `Graph.community_edge_betweenness`    | `community_edge_betweenness(clusters=None, directed=True, weights=None)`                                                                                                        | `VertexDendrogram` |
|  [07]   | `Graph.community_label_propagation`   | `community_label_propagation(weights=None, initial=None, fixed=None)`                                                                                                           | `VertexClustering` |
|  [08]   | `Graph.community_leading_eigenvector` | `community_leading_eigenvector(clusters=None, weights=None, arpack_options=None)`                                                                                               | `VertexClustering` |
|  [09]   | `Graph.community_optimal_modularity`  | `community_optimal_modularity(*args, **kwds)`                                                                                                                                   | `VertexClustering` |
|  [10]   | `Graph.community_spinglass`           | `community_spinglass(*args, **kwds)`                                                                                                                                            | `VertexClustering` |
|  [11]   | `Graph.community_voronoi`             | `community_voronoi(lengths=None, weights=None, mode='out', radius=None)`                                                                                                        | `VertexClustering` |
|  [12]   | `Graph.community_fluid_communities`   | `community_fluid_communities(no_of_communities)`                                                                                                                                | `VertexClustering` |
|  [13]   | `Graph.modularity`                    | `modularity(membership, weights=None, resolution=1, directed=True)`                                                                                                             | `float`            |
|  [14]   | `Graph.k_core`                        | `k_core(*args)`                                                                                                                                                                 | `Graph` or list    |

[ENTRYPOINT_SCOPE]: clustering result inspection
- rail: graph

`VertexClustering` carries the flat partition receipt (`membership`, `modularity`, sizes, induced subgraphs); `VertexDendrogram` carries the hierarchical merges and cuts to a `VertexClustering` at `as_clustering`. `compare_communities`/`split_join_distance` compute partition distance over membership lists or `VertexClustering` objects.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                        | [CAPABILITY]                                            |
| :-----: | :------------------------------- | :------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `VertexClustering.membership`    | property                                                            | per-vertex cluster-id list                              |
|  [02]   | `VertexClustering.modularity`    | property                                                            | partition modularity score                              |
|  [03]   | `VertexClustering.sizes`         | `sizes(*args)`                                                      | cluster size list                                       |
|  [04]   | `VertexClustering.subgraph`      | `subgraph(idx)`                                                     | induced subgraph for one cluster                        |
|  [05]   | `VertexClustering.subgraphs`     | `subgraphs()`                                                       | induced subgraph per cluster                            |
|  [06]   | `VertexClustering.giant`         | `giant()`                                                           | largest-cluster induced subgraph                        |
|  [07]   | `VertexClustering.crossing`      | `crossing()`                                                        | per-edge cross-cluster boolean                          |
|  [08]   | `VertexClustering.cluster_graph` | `cluster_graph(combine_vertices=None, combine_edges=None)`          | contracted cluster-level graph                          |
|  [09]   | `VertexClustering.compare_to`    | `compare_to(other, *args, **kwds)`                                  | partition distance against another clustering           |
|  [10]   | `VertexDendrogram.as_clustering` | `as_clustering(n=None)`                                             | cut merges to a flat `VertexClustering`                 |
|  [11]   | `VertexDendrogram.optimal_count` | property                                                            | modularity-optimal cluster count                        |
|  [12]   | `compare_communities`            | `compare_communities(comm1, comm2, method='vi', remove_none=False)` | VI/NMI/split-join/RI/ARI/Danon/meila partition distance |
|  [13]   | `split_join_distance`            | `split_join_distance(comm1, comm2, remove_none=False)`              | directed split-join distance pair                       |

[ENTRYPOINT_SCOPE]: `Graph` construction
- rail: graph
- construction classmethods build a `Graph` from edge data, frames, adjacency, or named generators

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                        | [CAPABILITY]                              |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `Graph.TupleList`          | `TupleList(edges, directed=False, vertex_name_attr='name', edge_attrs=None, weights=False)`                         | build from `(source, target, ...)` tuples |
|  [02]   | `Graph.DataFrame`          | `DataFrame(edges, directed=True, vertices=None, use_vids=True)`                                                     | build from a pandas edge frame            |
|  [03]   | `Graph.Adjacency`          | `Adjacency(matrix, mode='directed', loops='once')`                                                                  | build from an adjacency matrix            |
|  [04]   | `Graph.Weighted_Adjacency` | `Weighted_Adjacency(matrix, mode='directed', attr='weight', loops='once')`                                          | build from a weighted adjacency matrix    |
|  [05]   | `Graph.Read`               | `Read(f, format=None, *args, **kwds)`                                                                               | read GraphML/GML/edgelist/Pajek/NCOL/LGL  |
|  [06]   | `Graph.Famous`             | `Famous(name)`                                                                                                      | load a named reference graph              |
|  [07]   | `Graph.Erdos_Renyi`        | `Erdos_Renyi(n, p, m, directed=False, loops=False, edge_labeled=False)`                                             | G(n,p)/G(n,m) random graph                |
|  [08]   | `Graph.Barabasi`           | `Barabasi(n, m, outpref=False, directed=False, power=1, zero_appeal=1, implementation='psumtree', start_from=None)` | scale-free preferential-attachment graph  |

## [04]-[IMPLEMENTATION_LAW]

[GRAPH_COMMUNITY]:
- import: `import igraph` at boundary scope only; module-level import is banned by the manifest import policy.
- container axis: one `Graph` owns construction, mutation, and the `community_*` detection family; `TupleList`/`DataFrame`/`Adjacency`/`Read` are construction rows, never a per-source builder type; the leading `graph` parameter in the bound method is `self`.
- detection axis: `community_leiden` (refined Leiden, `objective_function` selects `CPM`/`modularity`), `community_multilevel` (Louvain), and `community_infomap` are the GRAPH_COMMUNITY rows; algorithm selection is a method row, never a parallel detector class; `weights` accepts an edge-attribute name or per-edge sequence.
- result axis: modularity-flat algorithms return a `VertexClustering`; hierarchical algorithms (`community_fastgreedy`/`community_walktrap`/`community_edge_betweenness`) return a `VertexDendrogram` cut to a `VertexClustering` via `as_clustering`; partition state is read off `membership`/`modularity`/`sizes`, never hand-recomputed.
- scoring axis: `Graph.modularity` scores an external membership against the C-core; `community_optimal_modularity` is the exact-optimum row; resolution control is the `resolution` argument, never a forked algorithm.
- comparison axis: `compare_communities` is the single partition-distance surface keyed by `method` (`vi`/`nmi`/`split-join`/`rand`/`adjusted_rand`/`danon`/`meila`); `split_join_distance` is the directed split-join row; partition agreement is a `method` row, never a re-implemented metric.
- evidence: each detection captures algorithm name, cluster count, per-cluster sizes, membership vector, modularity score, and resolution as a graph receipt.
- boundary: igraph owns the libigraph C-core detection and modularity; the `Graph` container is built from edge tuples or a pandas frame at the boundary and the membership vector feeds downstream owners; live drawing stays outside this package.

[RAIL_LAW]:
- Package: `igraph`
- Owns: C-core graph containers, the full `community_*` detection family (Leiden/Louvain/Infomap and the dendrogram algorithms), modularity scoring, k-core decomposition, `VertexClustering`/`VertexDendrogram` result carriers, and VI/NMI/RI/split-join partition comparison
- Accept: `Graph.community_leiden`/`community_multilevel`/`community_infomap` for GRAPH_COMMUNITY, `VertexClustering.membership`/`modularity` as the receipt, `compare_communities` keyed by `method` for partition distance, `Graph.TupleList`/`DataFrame` at the construction boundary
- Reject: wrapper-renames of `community_*`/`modularity`; a hand-rolled modularity-optimization or Leiden/Louvain loop the C core already owns; a parallel detector type per algorithm when the method row covers it; a re-implemented partition-distance metric where `compare_communities` carries the `method`; identity minting the runtime owns
