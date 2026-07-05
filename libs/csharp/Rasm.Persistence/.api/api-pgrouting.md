# [RASM_PERSISTENCE_API_PGROUTING]

`pgrouting` supplies network/graph routing over PostGIS — the `pgr_*` SELECT functions (Dijkstra, A*,
bidirectional, driving-distance, K-shortest-path, TSP, max-flow, component/topology analysis) over the
one Edges-SQL inner-query contract (`id`/`source`/`target`/`cost`/`reverse_cost`). It carries no managed
assembly: every surface is server-side SQL the `Store/provisioning#SERVER_EXTENSIONS`
`ServerExtension("pgrouting", Cascade: true)` row installs and the `Query/lane#GEO_LANES` routing
consumer drives through raw `Npgsql`/`FromSql`/`SqlQuery` against the `SETOF record` result, so an
in-database shortest-path/driving-distance/clash-graph query runs beside the PostGIS spatial lane
without a managed graph engine. The extension is preload-free — it is a pure FUNCTION extension (Boost
Graph C functions loaded lazily per call, no background worker, no planner hook, no index access
method), so it is correctly absent from the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries`
row — and installs `CASCADE` (`CREATE EXTENSION IF NOT EXISTS pgrouting CASCADE`) to pull its `postgis`
(and `plpgsql`) dependency in one step.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pgrouting`
- package: server-side PostgreSQL extension (C++/Boost Graph, not a NuGet package); repo `pgrouting/pgrouting`, installed name `pgrouting`, `default_version = '4.0.1'`
- namespace: SQL `public` (`pgr_*` set-returning functions; the Edges/Combinations/Matrix/Coordinates/Points inner-query contracts)
- depends: `requires = 'plpgsql,postgis'` (runtime PostGIS >= 3.0) — pulled by `CREATE EXTENSION pgrouting CASCADE`
- license: GPL-2.0-or-later — the in-DB deployment is the license boundary, no managed linkage
- registration: function extension, preload-free — no index access method, no `shared_preload_libraries` row; the `ServerExtension("pgrouting", Cascade: true, PreloadGated: false)` row emits `CREATE EXTENSION IF NOT EXISTS pgrouting CASCADE` through `Store/provisioning#SERVER_EXTENSIONS` `Migrate`
- consumed by: `Query/lane#GEO_LANES` routing over the federated entity graph (`Query/federation#ENTITY_GRAPH`), driven through raw `Npgsql` against the `SETOF record` result with the mandatory column-definition list
- rail: routing-provisioning, geo-lanes

Every `pgr_*` routing function is a SELECT function returning `SETOF record` via inline `OUT` params; the
compulsory leading args are positional (a `text` Edges/Combinations/Matrix/Coordinates SQL string, then
`bigint` vid or `anyarray` vids). Exceptions: `pgr_maxFlow` returns scalar `bigint`;
`pgr_articulationPoints`/`pgr_bridges` return `SETOF bigint`.

## [02]-[EDGES_SQL]

The Edges-SQL inner query is the graph definition every routing function takes as its first `text`
argument. `id`/`source`/`target`/`cost` are required; `reverse_cost` is optional with default `-1`. A
negative `cost` or `reverse_cost` means the corresponding directed edge does NOT exist in the graph
(absent, not zero-weight), so a one-way edge is a positive `cost` with a negative `reverse_cost`.

| [INDEX] | [COLUMN]       | [TYPE]        | [REQUIRED] | [SEMANTICS]                                                  |
| :-----: | :------------- | :------------ | :--------- | :---------------------------------------------------------- |
|  [01]   | `id`           | ANY-INTEGER   | yes        | edge identifier                                            |
|  [02]   | `source`       | ANY-INTEGER   | yes        | first end-point vertex id                                   |
|  [03]   | `target`       | ANY-INTEGER   | yes        | second end-point vertex id                                  |
|  [04]   | `cost`         | ANY-NUMERICAL | yes        | weight of edge (`source` → `target`); negative ⇒ edge absent |
|  [05]   | `reverse_cost` | ANY-NUMERICAL | no (`-1`)  | weight of edge (`target` → `source`); negative ⇒ reverse edge absent |

`ANY-INTEGER` = `SMALLINT`/`INTEGER`/`BIGINT`; `ANY-NUMERICAL` = `SMALLINT`/`INTEGER`/`BIGINT`/`REAL`/
`FLOAT`. Variant inner queries: A*/bidirectional-A* add `x1`/`y1`/`x2`/`y2` (ANY-NUMERICAL source/target
coordinates); the flow family replaces `cost` with `capacity`/`reverse_capacity` (ANY-INTEGER, reverse
default `-1`); a Combinations SQL is `(source ANY-INTEGER, target ANY-INTEGER)`; a Matrix SQL (TSP input)
is `(start_vid ANY-INTEGER, end_vid ANY-INTEGER, agg_cost ANY-NUMERICAL)`; a Coordinates SQL
(`pgr_TSPeuclidean`) is `(id ANY-INTEGER, x ANY-NUMERICAL, y ANY-NUMERICAL)`. The Edges SQL string binds
through one `Npgsql` parameter from the routing consumer, never a runtime-concatenated literal.

## [03]-[ROUTING_FUNCTIONS]

Each path function ships five overloads — `One to One` (`vid`, `vid`), `One to Many` (`vid`, `vids`),
`Many to One` (`vids`, `vid`), `Many to Many` (`vids`, `vids`), and `Combinations` (a Combinations SQL) —
all closing on `directed BOOLEAN DEFAULT true`. The `*Cost` sibling returns only the aggregate; the
`*CostMatrix` sibling takes one vertex set and feeds `pgr_TSP`.

| [INDEX] | [FUNCTION]               | [SIGNATURE]                                                                                          | [OUTPUT]                          |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `pgr_dijkstra`           | `pgr_dijkstra(Edges SQL, start vid(s), end vid(s)[, directed])` / `(Edges SQL, Combinations SQL[, directed])` | PATH                              |
|  [02]   | `pgr_dijkstraCost`       | `pgr_dijkstraCost(Edges SQL, start vid(s), end vid(s)[, directed])`                                  | COST                              |
|  [03]   | `pgr_dijkstraCostMatrix` | `pgr_dijkstraCostMatrix(Edges SQL, vids[, directed])`                                                | COST (symmetric when `directed => false`) |
|  [04]   | `pgr_dijkstraVia`        | `pgr_dijkstraVia(Edges SQL, via vids[, directed, strict, U_turn_on_edge])`                           | VIA                               |
|  [05]   | `pgr_aStar`              | `pgr_aStar(Edges SQL, start vid(s), end vid(s)[, directed, heuristic => 5, factor => 1.0, epsilon => 1.0])` | PATH (needs xy Edges SQL)         |
|  [06]   | `pgr_aStarCost` / `…CostMatrix` | same options + xy Edges SQL                                                                   | COST                              |
|  [07]   | `pgr_bdDijkstra` / `…Cost` / `…CostMatrix` | `pgr_bdDijkstra(Edges SQL, start vid(s), end vid(s)[, directed])`                  | PATH / COST                       |
|  [08]   | `pgr_bdAstar` / `…Cost` / `…CostMatrix` | xy Edges SQL + `[directed, heuristic => 5, factor => 1.0, epsilon => 1.0]`            | PATH / COST                       |
|  [09]   | `pgr_KSP`                | `pgr_KSP(Edges SQL, start vid(s), end vid(s), K integer[, directed, heap_paths => false])`           | MULTI-PATH (`path_id=1` cheapest) |
|  [10]   | `pgr_withPoints` / `…Cost` / `…CostMatrix` / `…DD` / `…KSP` | `pgr_withPoints(Edges SQL, Points SQL, start vid(s), end vid(s)[, driving_side, directed, details])` | PATH (negative node ⇒ a Point)    |

Output-row shapes (inline `OUT` params, no named composite type): PATH =
`(seq integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)`
with `edge = -1` on the last node of each path; MULTI-PATH = PATH `+ path_id integer`; VIA = MULTI-PATH
`+ route_agg_cost float` (`edge = -2` on the last node of the route); COST =
`(start_vid bigint, end_vid bigint, agg_cost float)`. `pgr_aStar`/`pgr_bdAstar` require the xy Edges SQL
(`x1`/`y1`/`x2`/`y2`); `heuristic` is `0`..`5` (5 = `abs(dx)+abs(dy)`), `factor > 0`, `epsilon >= 1`.

## [04]-[DISTANCE_TSP_FLOW]

Driving-distance is a spanning-tree fold; TSP is metric-only and undirected (the signature carries
only `start_id`/`end_id`, no simulated-annealing knobs); the flow family routes over
`capacity`/`reverse_capacity` and has no `directed` arg (the graph is directed by construction).

| [INDEX] | [FUNCTION]              | [SIGNATURE]                                                                                  | [OUTPUT]                          |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `pgr_drivingDistance`   | `pgr_drivingDistance(Edges SQL, root vid, distance float[, directed])` / `(Edges SQL, root vids, distance[, directed, equicost => true])` | SPANTREE — `(seq bigint, depth bigint, start_vid bigint, pred bigint, node bigint, edge bigint, cost float, agg_cost float)` |
|  [02]   | `pgr_TSP`               | `pgr_TSP(Matrix SQL, start_id bigint => 0, end_id bigint => 0)`                              | TOUR — `(seq integer, node bigint, cost float, agg_cost float)` |
|  [03]   | `pgr_TSPeuclidean`      | `pgr_TSPeuclidean(Coordinates SQL, start_id bigint => 0, end_id bigint => 0)`                | TOUR                              |
|  [04]   | `pgr_maxFlow`           | `pgr_maxFlow(Edges SQL, source vid(s), sink vid(s))` → `bigint`                              | scalar max-flow value             |
|  [05]   | `pgr_boykovKolmogorov` / `pgr_pushRelabel` / `pgr_edmondsKarp` | `pgr_boykovKolmogorov(Edges SQL, source vid(s), sink vid(s))`           | FLOW — `(seq integer, edge bigint, start_vid bigint, end_vid bigint, flow bigint, residual_capacity bigint)` |

`equicost` is the array-root default `true`: each node lands in only its closest root's spanning tree
(arbitrary tie-break); `equicost => false` resembles independent single-root calls, so a node within
`distance` of several roots appears once per root tree. `pgr_drivingDistance` extracts nodes with
`agg_cost <= distance`. A TSP Matrix SQL is built by a
`*CostMatrix` (preferably `directed => false`); negative matrix costs are ignored.

## [05]-[COMPONENTS_TOPOLOGY]

Component/connectivity analysis takes a single basic Edges SQL. The graph/topology builder is
`pgr_extractVertices`. The `pgr_createTopology`/`pgr_createVerticesTable`/`pgr_analyzeGraph`/
`pgr_analyzeOneWay`/`pgr_nodeNetwork` family is a removed phantom spelling; `pgr_extractVertices` is
the replacement.

| [INDEX] | [FUNCTION]                  | [SIGNATURE]                                       | [OUTPUT]                                     |
| :-----: | :-------------------------- | :----------------------------------------------- | :------------------------------------------ |
|  [01]   | `pgr_connectedComponents`   | `pgr_connectedComponents(Edges SQL)`             | `(seq bigint, component bigint, node bigint)` — undirected, `component` = min node id |
|  [02]   | `pgr_strongComponents`      | `pgr_strongComponents(Edges SQL)`                | `(seq bigint, component bigint, node bigint)` — Tarjan SCC, directed |
|  [03]   | `pgr_biconnectedComponents` | `pgr_biconnectedComponents(Edges SQL)`           | `(seq bigint, component bigint, edge bigint)` — undirected |
|  [04]   | `pgr_articulationPoints`    | `pgr_articulationPoints(Edges SQL)` → `SETOF bigint` | cut vertices (`node`), ascending          |
|  [05]   | `pgr_bridges`               | `pgr_bridges(Edges SQL)` → `SETOF bigint`        | cut edges (`edge`), ascending               |
|  [06]   | `pgr_extractVertices`       | `pgr_extractVertices(Edges SQL, dryrun boolean => false)` | `(id bigint, in_edges bigint[], out_edges bigint[], x float, y float, geom geometry)` — derives the vertices table from `geom`/`startpoint`+`endpoint`/`source`+`target` |

## [06]-[IMPLEMENTATION_LAW]

[PGROUTING_TOPOLOGY]:
- Function extension, preload-free, CASCADE install: `pgrouting` registers no background worker, no planner hook, and no index access method (`relocatable = true`, C functions loaded lazily per call), so it is correctly absent from the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row — install is `ServerExtension("pgrouting", Cascade: true, PreloadGated: false)` whose `CreateSql` emits `CREATE EXTENSION IF NOT EXISTS pgrouting CASCADE` through `Store/provisioning#SERVER_EXTENSIONS` `Migrate`, pulling its `postgis` (and `plpgsql`) dependency (`requires = 'plpgsql,postgis'`) in one DDL step exactly like the `vectorscale` CASCADE row.
- No managed assembly, no EF translator: every `pgr_*` call rides raw `Npgsql`/`FromSql`/`SqlQuery`, and because each function is declared `RETURNS SETOF record`, PostgreSQL requires the caller to supply a column-definition list (`AS (seq integer, path_seq integer, …)`) matching the function's `OUT` params — an anonymous-record call without the list is the faulted spelling. The Edges/Combinations/Matrix/Points SQL strings and the vid/vids arrive as `Npgsql` parameters, never a runtime-concatenated routing query.
- Edge contract is the one graph definition: a one-way edge is a positive `cost` with a negative `reverse_cost` (a negative weight means the directed edge is ABSENT, never a zero/negative-weight traversable edge), and `directed BOOLEAN DEFAULT true` selects directed vs undirected interpretation of the same Edges SQL — a parallel reversed-edge table or a per-direction Edges SQL is the rejected form.

[GEO_LANE_STACK]:
- In-residence routing: `pgrouting` lives inside the one `PostgresServer` residence beside the PostGIS spatial lane, so the `Query/lane#GEO_LANES` routing capability is a within-PG `pgr_dijkstra`/`pgr_drivingDistance`/`pgr_aStar` query over the federated entity graph (`Query/federation#ENTITY_GRAPH`), never a managed graph engine or a cross-store fan-out.
- H3 node-space seam: the managed `pocketken.H3` `GridPathCells`/`GridDistance` (`api-h3.md`) and the in-database `pgr_dijkstra` share the H3-cell id node space — an H3-cell-keyed graph routes in-process or in-database against the same `id`/`source`/`target` cell ids, so the in-process path and the in-database route agree on node identity.

[RAIL_LAW]:
- Package: `pgrouting` (server-side, in the deploy-image PG18)
- Owns: in-PG network/graph routing over PostGIS — the `pgr_*` shortest-path/cost/driving-distance/KSP/TSP/flow/component functions over the Edges-SQL contract
- Accept: `CREATE EXTENSION pgrouting CASCADE` via `ServerExtension("pgrouting", Cascade: true)`, the `id`/`source`/`target`/`cost`/`reverse_cost` Edges-SQL contract with negative-means-absent edges, the five-overload path functions and their `*Cost`/`*CostMatrix` siblings driven through `FromSql`/`SqlQuery` with the mandatory column-definition list, `pgr_extractVertices` for vertices-table derivation, `pgr_TSP` over a `*CostMatrix`-built Matrix SQL
- Reject: linking the extension into managed code, an EF-translated `pgr_*` member, an anonymous-record routing call without the column-definition list, placing `pgrouting` on the `shared_preload_libraries` row (it is preload-free), the removed `pgr_createTopology`/`pgr_createVerticesTable`/`pgr_analyzeGraph`/`pgr_nodeNetwork` family (use `pgr_extractVertices`), a positive `reverse_cost` on a non-existent reverse edge (use a negative), a runtime-concatenated Edges SQL string, a parallel managed graph engine beside the in-PG router
