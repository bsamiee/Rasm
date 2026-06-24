# [RASM_PERSISTENCE_API_PGVECTORSCALE]

`pgvectorscale` (the project/repo name; the installed extension is `vectorscale`) supplies the
`diskann` index access method over a `pgvector` `vector(N)` column — a disk-backed StreamingDiskANN
graph with statistical binary quantization (SBQ) that scales approximate-nearest-neighbour search
beyond RAM-resident HNSW. It carries no managed assembly: every surface is server-side SQL the
`Schema/ddl#EXTENSION_DDL` `SchemaDdl.DiskAnn` case projects through its `ProvisionSql` `Fin<string>`
and the `SchemaDdl.Sql` fold lands into `MigrationBuilder.Sql`, and the `Query/lanes#SEARCH_LANES`
planner routes a `pgvector` distance query through transparently. The companion is preload-free — it
registers through its index AM, so it is correctly absent from the `ClusterConfig`
`shared_preload_libraries` row — and runs in-process inside the PG18 server tier, never linked into
managed code. It installs `CASCADE` (`CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE`) to pull
its `vector` (pgvector) dependency in one step.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pgvectorscale` / extension `vectorscale`
- package: server-side PostgreSQL extension (Rust/pgrx, not a NuGet package); repo `timescale/pgvectorscale`, installed name `vectorscale`
- access method: `diskann`; depends on `vector` (pgvector) — installed via `CASCADE`
- namespace: SQL (`CREATE INDEX ... USING diskann`, `diskann.*` session GUCs)
- license: PostgreSQL License (Timescale) — the in-DB deployment is the license boundary, no managed linkage
- registration: index-AM, preload-free — absent from `ClusterConfig.Rows` `shared_preload_libraries` by design; the `SchemaDdl.Extension("vectorscale", AccessMethod: "diskann", Cascade: true)` row carries its install story
- consumed by: `SchemaDdl.DiskAnn` (`Schema/ddl`), `VectorQuery`/`HybridRetrieve` (`Query/lanes#SEARCH_LANES`), the `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops` ops classes shared with `api-pgvector-ef.md`
- rail: search-provisioning, search-lanes

## [02]-[INDEX_DDL]

The `diskann` index builds over a `vector(N)` column under one ops class. The column stays the
`EmbeddingArity.Dense` `vector` store type — `halfvec`/`sparsevec` over diskann are not catalogued
(held under `Query/lanes#SEARCH_PROVISIONING_PROBE`). One BM25-style restriction does not apply; a
table may carry multiple diskann indexes over distinct vector columns. The ops-class row IS the
`Schema/ddl#EXTENSION_DDL` `DiskAnnOps` `[SmartEnum<string>]`: `DiskAnnOps.Cosine`/`L2`/`InnerProduct`
each carry the `Key` (ops-class) and the `Operator`, and the EF query path reuses the catalogued
`api-pgvector-ef.md` distance `DbFunctions` so the same operator drives both index build and query.

| [INDEX] | [OPS_CLASS]         | [OPERATOR] | [DISTANCE]          | [PGVECTOR_FN]     | [DiskAnnOps]            |
| :-----: | :------------------ | :--------- | :------------------ | :---------------- | :--------------------- |
|  [01]   | `vector_cosine_ops` | `<=>`      | cosine              | `CosineDistance`  | `DiskAnnOps.Cosine`    |
|  [02]   | `vector_l2_ops`     | `<->`      | L2 / euclidean      | `L2Distance`      | `DiskAnnOps.L2`        |
|  [03]   | `vector_ip_ops`     | `<#>`      | negative inner-prod | `MaxInnerProduct` | `DiskAnnOps.InnerProduct` |

`CREATE INDEX <name> ON <table> USING diskann (<column> <ops_class>) WITH (<build-options>)` is the
canonical build. The `SchemaDdl.DiskAnn(Index, Table, Column, DiskAnnOps Ops, DiskAnnOptions Options)`
case emits `CREATE INDEX CONCURRENTLY` (no `ACCESS EXCLUSIVE` table lock) carrying the full options
map. The `<#>` inner-product operator is rejected against a `storage_layout = plain` build — SBQ
requires `memory_optimized` — and `ProvisionSql` surfaces that as `Fin<string>.Fail(<diskann-ip-on-plain-layout:…>)`
to the deploy-gate caller BEFORE any `Sql()` lands, never a runtime PG error.

## [03]-[BUILD_OPTIONS]

The `WITH (...)` storage parameters the `Schema/ddl#EXTENSION_DDL` `DiskAnnOptions` record projects
(`DiskAnnOptions.Default` pins `MemoryOptimized` / `NumNeighbors:50` / `SearchListSize:100` /
`MaxAlpha:1.2` / `NumDimensions:0` / `NumBitsPerDimension:0`). `DiskAnnLayout` is a `[SmartEnum]`
(`MemoryOptimized`/`Plain`). Sentinel values resolve at build time from the column dimensionality.

| [INDEX] | [WITH_KEY]               | [DiskAnnOptions FIELD] | [TYPE]  | [DEFAULT]          | [SEMANTICS]                                                      |
| :-----: | :----------------------- | :--------------------- | :------ | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `storage_layout`         | `StorageLayout` (`DiskAnnLayout`) | text | `memory_optimized` | `memory_optimized` enables SBQ; `plain` stores full vectors |
|  [02]   | `num_neighbors`          | `NumNeighbors`         | integer | `50`               | graph degree; `-1` derives degree from dimensionality            |
|  [03]   | `search_list_size`       | `SearchListSize`       | integer | `100`              | build-time candidate-list breadth                                |
|  [04]   | `max_alpha`              | `MaxAlpha`             | double  | `1.2`              | graph density / pruning aggression                               |
|  [05]   | `num_dimensions`         | `NumDimensions`        | integer | `0`                | dimensions to index; `0` indexes all                             |
|  [06]   | `num_bits_per_dimension` | `NumBitsPerDimension`  | integer | `0`                | SBQ bits/dim; `0` auto-selects (2 below 900 dims, 1 at or above) |

## [04]-[QUERY_GUC]

The session GUC the planner reads at query time to widen the search-list breadth for a diskann scan;
set per-session or per-transaction, never a build option.

| [INDEX] | [GUC]                            | [DEFAULT] | [SEMANTICS]                                        |
| :-----: | :------------------------------- | :-------- | :------------------------------------------------- |
|  [01]   | `diskann.query_search_list_size` | `100`     | query-time candidate-list breadth; higher = recall |
|  [02]   | `diskann.query_rescore`          | `50`      | full-precision rescore count after SBQ pre-filter  |

These GUCs are a SEARCH-LANE concern, never a build option — they are `SET LOCAL` per session/
transaction by the `Query/lanes#SEARCH_LANES` binder, distinct from the `WITH (...)` build map the
`SchemaDdl.DiskAnn` row owns. A build-time `query_*` knob is the rejected spelling.

## [05]-[IMPLEMENTATION_LAW]

[DISKANN_TOPOLOGY]:
- Index-AM, preload-free: `vectorscale` registers `diskann` as an access method, so it never enters `ClusterConfig.Rows` `shared_preload_libraries`. The `SchemaDdl.Extension("vectorscale", AccessMethod: "diskann", Cascade: true)` row emits `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE`, pulling its `vector` (pgvector) dependency in one DDL step.
- No managed assembly, no EF translator: the index DDL lands through `MigrationBuilder.Sql` via the `SchemaDdl.DiskAnn` → `ProvisionSql` → `SchemaDdl.Sql` fold; the column stays the `EmbeddingArity.Dense` `vector(N)` store type the pgvector EF plugin maps, so a diskann index adds NO parallel vector column.
- Provisioning gate: the `<#>`-on-`storage_layout=plain` rejection is a typed `Fin.Fail` from `ProvisionSql`, caught at the deploy gate before `Sql()` lands — SBQ requires `memory_optimized`.
- Build vs query split: `DiskAnnOptions` (`storage_layout`/`num_neighbors`/`search_list_size`/`max_alpha`/`num_dimensions`/`num_bits_per_dimension`) is the build-time `WITH` map owned by `Schema/ddl`; `diskann.query_search_list_size`/`query_rescore` are the query-time GUCs owned by the search lane. Disjoint owners, never crossed.

[SEARCH_LANE_STACK]:
- Transparent route: a `vector(N)` distance query ordered by the catalogued pgvector distance function (`CosineDistance`/`L2Distance`/`MaxInnerProduct`, `api-pgvector-ef.md`) is planner-routed through the diskann index with no query rewrite — the `VectorMetric.Order` `Switch` projects the `ORDER BY` distance `Expression` and the planner picks diskann over the exact scan.
- Route observability: the `search.vector.route` fact discriminates exact-scan vs HNSW vs IVFFlat vs diskann; the always-present exact-scan brute-force scan stays the correctness baseline so a route degradation is observable. diskann complements, never replaces, RAM-resident HNSW — it scales disk-backed ANN beyond memory.
- Hybrid fusion: `HybridRetrieve.Fuse` (`Query/lanes#SEARCH_LANES`) composes the diskann vector branch and the `pg_search` BM25 branch (`api-pg-search.md`) in one reciprocal-rank-fusion CTE — `SUM(1.0 / (rrfConstant + rank))` with `rrfConstant=60` — projecting identities (not re-materializing both payloads) and needing no learned reranker. The dense embedding the vector branch probes is generated upstream at `Compute/models#INFERENCE_MODES`.

[RAIL_LAW]:
- Package: `pgvectorscale` / extension `vectorscale` (server-side, in the deploy-image PG18)
- Owns: the `diskann` disk-backed StreamingDiskANN + SBQ ANN index over a pgvector `vector(N)` column
- Accept: `CREATE EXTENSION ... vectorscale CASCADE` install, `SchemaDdl.DiskAnn` `CREATE INDEX CONCURRENTLY ... USING diskann` with the `DiskAnnOptions` `WITH` map against `DiskAnnOps` ops classes, the catalogued pgvector distance functions for query, the `diskann.query_*` GUCs `SET LOCAL` by the search-lane binder, the `HybridRetrieve.Fuse` RRF stack with the pg_search branch
- Reject: linking the extension into managed code, a parallel vector column beside the existing pgvector column, a `<#>`-on-`plain` build, a build-time `query_*` knob or a query-time build option, treating `pgvectorscale` as the installed extension name (it is `vectorscale`), placing it on the `shared_preload_libraries` row (it is index-AM, preload-free)
