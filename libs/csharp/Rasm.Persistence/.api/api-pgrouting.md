# [RASM_PERSISTENCE_API_PGROUTING]

`pgrouting` owns in-database network routing over PostGIS: every `pgr_*` function takes its graph as a `text` inner query and returns paths, costs, spanning trees, tours, flows, or components as SQL rows. It ships no managed assembly — a `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` row installs it and the `Query/cypher#GRAPH_QUERY` cases drive it through raw `Npgsql` — so a shortest-path, catchment, or connectivity query folds inside the PostGIS residence over H3-cell node ids while `Query/topology` keeps the authoritative in-process graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pgrouting`
- package: server-side PostgreSQL extension (C++ over Boost Graph, not a NuGet package); repo `pgrouting/pgrouting`, installed name `pgrouting`
- namespace: SQL `public` — the `pgr_*` set-returning functions and the inner-query row contracts they consume
- depends: `requires = 'plpgsql,postgis'`, pulled in one DDL step by the row's `CREATE EXTENSION IF NOT EXISTS "pgrouting" CASCADE`
- license: GPL-2.0-or-later — the in-DB deployment is the license boundary, no managed linkage
- registration: `relocatable = true` function extension whose C functions load lazily per call, registering no background worker, planner hook, or index access method, so `Store/provisioning#SERVER_EXTENSIONS` gates the row as `ExtensionAdmission.BaseType("postgis")`
- consumed by: `Query/cypher#GRAPH_QUERY` — the `GraphQuery` cases (`Path`/`Via`/`Located`/`Kth`/`Spread`/`Tour`/`TourPlanar`/`Flow`/`Cleave`) selected by the `RouteMode`/`FlowKind`/`CleaveKind` policy rows, over the `Element/identity#ELEMENT_IDENTITY` `NodeCell` cell ids the `network_edge` `source`/`target` carries
- rail: routing-provisioning, graph-lane

## [02]-[INNER_QUERIES]

[PUBLIC_TYPE_SCOPE]: each call defines its graph by a `text` inner query bound as one `Npgsql` parameter. Identifier columns are `ANY-INTEGER` (`SMALLINT`/`INTEGER`/`BIGINT`), weight and coordinate columns `ANY-NUMERICAL` (adding `REAL`/`FLOAT`). `reverse_cost` and `reverse_capacity` default `-1`, and a negative weight means that directed edge is absent from the graph.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                 |
| :-----: | :------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Edges SQL`          | graph           | `id` `source` `target` `cost` `reverse_cost`                 |
|  [02]   | `xy Edges SQL`       | graph           | base columns `+ x1` `y1` `x2` `y2` endpoint coordinates      |
|  [03]   | `capacity Edges SQL` | graph           | `id` `source` `target` `capacity` `reverse_capacity`         |
|  [04]   | `Combinations SQL`   | pair set        | `source` `target` — an explicit start/end pairing            |
|  [05]   | `Matrix SQL`         | cost matrix     | `start_vid` `end_vid` `agg_cost` — a `*CostMatrix` result    |
|  [06]   | `Coordinates SQL`    | point set       | `id` `x` `y` — plane input for `pgr_TSPeuclidean`            |
|  [07]   | `Points SQL`         | point set       | `pid` `edge_id` `fraction` `side` — temporary graph vertices |
|  [08]   | `Restrictions SQL`   | restriction set | `path` `cost` — forbidden edge sequences for the TRSP family |
|  [09]   | `Vertex SQL`         | vertex set      | `id` `in_edges` `out_edges` — a prebuilt vertices table      |

## [03]-[SHORTEST_PATH]

[ENTRYPOINT_SCOPE]: every path member ships five overloads — one-to-one, one-to-many, many-to-one, many-to-many, and a `Combinations SQL` — over the base call `f(Edges SQL, start vid(s), end vid(s))` closing on named options, `directed` defaulting `true`; `[SURFACE]` carries only what a member takes beyond that base. A* members read an `xy Edges SQL` under named `heuristic`/`factor`/`epsilon`, withPoints members insert a `Points SQL` argument beside a positional unnamed driving side, and TRSP members insert a `Restrictions SQL` argument.

| [INDEX] | [SURFACE]                                           | [SHAPE]    | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------ |
|  [01]   | `pgr_dijkstra`                                      | PATH       | shortest path                         |
|  [02]   | `pgr_dijkstraCost`                                  | COST       | aggregate cost alone                  |
|  [03]   | `pgr_dijkstraCostMatrix(vids)`                      | COST       | all-pairs matrix over one vertex set  |
|  [04]   | `pgr_dijkstraVia(via vids, strict, U_turn_on_edge)` | VIA        | route visiting an ordered vertex list |
|  [05]   | `pgr_dijkstraNear(cap, global)`                     | PATH       | route to the nearest of a target set  |
|  [06]   | `pgr_dijkstraNearCost(cap, global)`                 | COST       | cost to the nearest of a target set   |
|  [07]   | `pgr_KSP(K, heap_paths)`                            | MULTI-PATH | Yen K shortest paths                  |
|  [08]   | `pgr_aStar`                                         | PATH       | heuristic search over endpoint xy     |
|  [09]   | `pgr_aStarCost`                                     | COST       | A* aggregate cost                     |
|  [10]   | `pgr_aStarCostMatrix(vids)`                         | COST       | A* matrix over one vertex set         |
|  [11]   | `pgr_bdDijkstra`                                    | PATH       | bidirectional Dijkstra                |
|  [12]   | `pgr_bdDijkstraCost`                                | COST       | bidirectional aggregate cost          |
|  [13]   | `pgr_bdDijkstraCostMatrix(vids)`                    | COST       | bidirectional matrix                  |
|  [14]   | `pgr_bdAstar`                                       | PATH       | bidirectional heuristic search        |
|  [15]   | `pgr_bdAstarCost`                                   | COST       | bidirectional A* aggregate cost       |
|  [16]   | `pgr_bdAstarCostMatrix(vids)`                       | COST       | bidirectional A* matrix               |
|  [17]   | `pgr_floydWarshall`                                 | COST       | all-pairs costs, dense graphs         |
|  [18]   | `pgr_johnson`                                       | COST       | all-pairs costs, sparse graphs        |
|  [19]   | `pgr_trsp`                                          | PATH       | turn-restricted shortest path         |
|  [20]   | `pgr_trspVia(via vids)`                             | VIA        | restricted ordered-waypoint route     |
|  [21]   | `pgr_trsp_withPoints(Points SQL)`                   | PATH       | restricted route anchored on points   |
|  [22]   | `pgr_trspVia_withPoints(Points SQL, via vids)`      | VIA        | restricted waypoint route on points   |
|  [23]   | `pgr_withPoints(details)`                           | PATH       | route from or to points along an edge |
|  [24]   | `pgr_withPointsCost`                                | COST       | point-anchored aggregate cost         |
|  [25]   | `pgr_withPointsCostMatrix(vids)`                    | COST       | point-anchored matrix                 |
|  [26]   | `pgr_withPointsDD(distance, details, equicost)`     | SPANTREE   | point-anchored catchment              |
|  [27]   | `pgr_withPointsKSP(K, heap_paths, details)`         | MULTI-PATH | point-anchored K shortest paths       |
|  [28]   | `pgr_withPointsVia(via vids, details)`              | VIA        | point-anchored waypoint route         |

- `pgr_withPoints`: a `Points SQL` vertex enters the result as a negative `node` id, so one path sequence carries point ids beside vertex ids.
- `pgr_floydWarshall`/`pgr_johnson`: the all-pairs pair reads the whole graph — `(Edges SQL, directed)` is the entire call.

[OUTPUT_SHAPES]:
- PATH: `(seq, path_seq, start_vid, end_vid, node, edge, cost, agg_cost)`, `edge = -1` on a path's last node.
- MULTI-PATH: PATH `+ path_id`, ascending by aggregate cost.
- VIA: MULTI-PATH `+ route_agg_cost`, `edge = -2` on the route's last node.
- COST: `(start_vid, end_vid, agg_cost)`, symmetric when a `*CostMatrix` runs `directed => false`.
- SPANTREE: `(seq, depth, start_vid, pred, node, edge, cost, agg_cost)`.
- MST: `(edge, cost)`.
- TOUR: `(seq, node, cost, agg_cost)`.
- FLOW: `(seq, edge, start_vid, end_vid, flow, residual_capacity)`.

## [04]-[SPREAD_TOUR_FLOW]

[ENTRYPOINT_SCOPE]: catchment members bound a Dijkstra or spanning-tree walk by distance or depth, taking a single root vid or a vids array under `equicost`, which lands each node in its nearest root's tree alone. Tour members approximate a metric tour over an undirected graph and ignore negative matrix costs. Flow members read a `capacity Edges SQL` on a graph directed by construction, so `(source vid(s), sink vid(s))` or a `Combinations SQL` is their whole variance.

| [INDEX] | [SURFACE]                                              | [SHAPE]    | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------- | :--------- | :------------------------------------ |
|  [01]   | `pgr_drivingDistance(root vid(s), distance, equicost)` | SPANTREE   | Dijkstra catchment to `agg_cost`      |
|  [02]   | `pgr_kruskal`                                          | MST        | minimum spanning forest, undirected   |
|  [03]   | `pgr_kruskalBFS(root vid(s), max_depth)`               | SPANTREE   | Kruskal forest in breadth-first order |
|  [04]   | `pgr_kruskalDD(root vid(s), distance)`                 | SPANTREE   | Kruskal catchment                     |
|  [05]   | `pgr_kruskalDFS(root vid(s), max_depth)`               | SPANTREE   | Kruskal forest in depth-first order   |
|  [06]   | `pgr_prim`                                             | MST        | minimum spanning forest, undirected   |
|  [07]   | `pgr_primBFS(root vid(s), max_depth)`                  | SPANTREE   | Prim forest in breadth-first order    |
|  [08]   | `pgr_primDD(root vid(s), distance)`                    | SPANTREE   | Prim catchment                        |
|  [09]   | `pgr_primDFS(root vid(s), max_depth)`                  | SPANTREE   | Prim forest in depth-first order      |
|  [10]   | `pgr_TSP(Matrix SQL, start_id, end_id)`                | TOUR       | metric tour over a cost matrix        |
|  [11]   | `pgr_TSPeuclidean(Coordinates SQL, start_id, end_id)`  | TOUR       | metric tour over plane coordinates    |
|  [12]   | `pgr_maxFlow(source vid(s), sink vid(s))`              | `bigint`   | push-relabel maximum-flow value       |
|  [13]   | `pgr_boykovKolmogorov(source vid(s), sink vid(s))`     | FLOW       | per-edge flow, Boykov-Kolmogorov      |
|  [14]   | `pgr_pushRelabel(source vid(s), sink vid(s))`          | FLOW       | per-edge flow, push-relabel           |
|  [15]   | `pgr_edmondsKarp(source vid(s), sink vid(s))`          | FLOW       | per-edge flow, Edmonds-Karp           |
|  [16]   | `pgr_edgeDisjointPaths(source vid(s), sink vid(s))`    | MULTI-PATH | edge-disjoint paths over max flow     |
|  [17]   | `pgr_maxCardinalityMatch`                              | `(edge)`   | maximum cardinality matching          |

- `pgr_TSP`: a `*CostMatrix` built under `directed => false` yields the symmetric, fully-connected, triangle-obeying matrix the metric bound assumes.

## [05]-[GRAPH_ANALYSIS]

[ENTRYPOINT_SCOPE]: connectivity, contraction, and degree members read a bare `Edges SQL`; the geometry utilities read an `id`/`geom` linework query instead. Each utility exposes `dryrun` to emit its generated SQL as a `NOTICE`, so an application refines the query in place rather than reimplementing it.

| [INDEX] | [SURFACE]                                              | [SHAPE]          | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------- | :--------------- | :--------------------------------- |
|  [01]   | `pgr_connectedComponents`                              | COMPONENT        | undirected connected components    |
|  [02]   | `pgr_strongComponents`                                 | COMPONENT        | Tarjan strong components, directed |
|  [03]   | `pgr_biconnectedComponents`                            | COMPONENT        | biconnected, keyed by `edge`       |
|  [04]   | `pgr_articulationPoints`                               | `(node)`         | cut vertices, ascending            |
|  [05]   | `pgr_bridges`                                          | `(edge)`         | cut edges, ascending               |
|  [06]   | `pgr_contraction(methods, cycles, forbidden)`          | CONTRACTION      | dead-end and linear contraction    |
|  [07]   | `pgr_degree(Vertex SQL, dryrun)`                       | `(node, degree)` | incident-edge count per vertex     |
|  [08]   | `pgr_extractVertices(dryrun)`                          | VERTEX           | vertices table from the edge set   |
|  [09]   | `pgr_findCloseEdges(point(s), tolerance, cap, dryrun)` | CLOSE_EDGE       | snap points onto edges             |
|  [10]   | `pgr_separateCrossing(tolerance, dryrun)`              | SEGMENT          | split linework at crossings        |
|  [11]   | `pgr_separateTouching(tolerance, dryrun)`              | SEGMENT          | split linework at contacts         |

[OUTPUT_SHAPES]:
- COMPONENT: `(seq, component, node)`, `component` carrying the component's minimum node id.
- CONTRACTION: `(type, id, contracted_vertices, source, target, cost)`, `type = 'v'` a modified vertex and `'e'` a new edge under a negative pseudo id.
- VERTEX: `(id, in_edges, out_edges, x, y, geom)`, derived from `geom`, `startpoint`+`endpoint`, or `source`+`target`.
- CLOSE_EDGE: `(edge_id, fraction, side, distance, geom, edge)` — the `Points SQL` shape `pgr_withPoints` consumes.
- SEGMENT: `(seq, id, sub_id, geom)`.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Edge sign is the graph definition: a negative `cost` or `reverse_cost` removes that directed edge, so a one-way edge is a positive `cost` beside a negative `reverse_cost`, and the `directed` option reinterprets one Edges SQL as directed or undirected — one edge projection serves both readings.
- Inline `OUT` parameters resolve every `SETOF record` result, so `SELECT * FROM pgr_dijkstra(…)` binds by column name with no column-definition list; `pgr_maxFlow` returns a scalar `bigint` and the cut members a single `bigint` column.
- Every inner query, vid, and vid array crosses as an `Npgsql` parameter, so bound values alone select the subgraph a call traverses.

[STACKING]:
- `api-postgis`(`.api/api-postgis.md`): PostGIS owns the linework the Edges SQL reads — `pgr_extractVertices`, `pgr_findCloseEdges`, and the `pgr_separate*` pair consume the `LINESTRING` `geom` column directly, and a returned `edge` id joins back to `ST_*` geometry to render the route.
- `api-h3-pg`(`.api/api-h3-pg.md`): `h3_latlng_to_cell` mints the `h3index` values the Edges SQL binds as `source`/`target`, so an `h3_cell = ANY(@cells)` membership test narrows the routing subgraph before the fold runs.
- `api-h3`(`.api/api-h3.md`): `H3Index.FromPoint(Point, int)` computes the same 64-bit cell the server computes, so `GridPathCells`/`GridDistance` in-process and `pgr_dijkstra` in-database agree on node identity.
- `api-quikgraph`(`../../.api/api-quikgraph.md`): `Query/topology` answers the authoritative walk from `AlgorithmExtensions.ShortestPathsDijkstra` under unit weights, and metric, capacity, and turn-restricted routing folds here — the two lanes split on weight semantics over one edge projection.
- within-lib: `Query/cypher#GRAPH_QUERY` selects one member per case through the `RouteMode`/`FlowKind`/`CleaveKind` rows and decodes each `[SHAPE]` into its own space — PATH and VIA into the cell mesh, `pgr_bridges` and `pgr_biconnectedComponents` into the edge space, FLOW into the per-edge assignment — so the whole roster composes under one `[Union]` verb.

[LOCAL_ADMISSION]:
- Routing enters through the PostgreSQL store profile alone: one `ServerExtension` row keyed `pgrouting` under `ExtensionAdmission.BaseType("postgis")`, over an Edges SQL built on the `network_edge` projection whose `source`/`target` are `NodeCell` cell ids.

[RAIL_LAW]:
- Package: `pgrouting` (server-side, in the deploy-image PG18)
- Owns: in-PG network routing over PostGIS — the shortest-path, cost, cost-matrix, catchment, spanning-tree, K-shortest-path, turn-restricted, point-anchored, tour, flow, component, contraction, and vertex-derivation functions over the inner-query contracts
- Accept: the `ServerExtension` CASCADE install over the `postgis` base type, the negative-means-absent edge contract, the five-overload path families and their `*Cost`/`*CostMatrix` siblings read through `FromSql`/`SqlQuery`, `pgr_extractVertices` for vertex derivation, `pgr_findCloseEdges` feeding a `Points SQL`, `pgr_TSP` over a `*CostMatrix`-built Matrix SQL
- Reject: linking the extension into managed code, an EF-translated `pgr_*` member, a positive `reverse_cost` standing for a missing reverse edge, a runtime-concatenated Edges SQL string, a managed graph engine re-solving the metric routes this router owns
