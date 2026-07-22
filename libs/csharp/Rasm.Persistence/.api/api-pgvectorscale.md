# [RASM_PERSISTENCE_API_PGVECTORSCALE]

`pgvectorscale` (installed extension `vectorscale`) mints the `diskann` index access method over a pgvector `vector(N)` column — a disk-backed StreamingDiskANN graph with statistical binary quantization (SBQ) that scales approximate-nearest-neighbour search past RAM-resident HNSW. Every surface is server-side SQL, no managed assembly: the diskann index DDL feeds the search-provisioning rail through the EF migration boundary, and a pgvector distance query planner-routes through the index transparently.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pgvectorscale` / extension `vectorscale`
- package: `vectorscale` (PostgreSQL License, Timescale) — server-side Rust/pgrx extension, repo `timescale/pgvectorscale`
- asset: server SQL, no managed assembly; `diskann` access method over pgvector `vector`, `CASCADE`-installed; SQL surface is `CREATE INDEX ... USING diskann` and `diskann.*` session GUCs
- rail: search-provisioning, search-lanes

## [02]-[INDEX_DDL]

`diskann` builds over one `vector(N)` column under one ops class; the column stays the `EmbeddingArity.Dense` `vector` store type the pgvector plugin maps (`api-pgvector-ef.md`), and a table carries multiple diskann indexes over distinct columns. `DiskAnnOps` (`[SmartEnum<string>]`, `Store/provisioning#SERVER_EXTENSIONS`) is the ops-class row — `Key` and `Operator` per case — reusing the catalogued pgvector distance `DbFunctions` so one operator drives both build and query.

| [INDEX] | [OPS_CLASS]         | [OPERATOR] | [DISTANCE]          | [PGVECTOR_FN]     | [DISKANNOPS]              |
| :-----: | :------------------ | :--------- | :------------------ | :---------------- | :------------------------ |
|  [01]   | `vector_cosine_ops` | `<=>`      | cosine              | `CosineDistance`  | `DiskAnnOps.Cosine`       |
|  [02]   | `vector_l2_ops`     | `<->`      | L2 / euclidean      | `L2Distance`      | `DiskAnnOps.L2`           |
|  [03]   | `vector_ip_ops`     | `<#>`      | negative inner-prod | `MaxInnerProduct` | `DiskAnnOps.InnerProduct` |

## [03]-[BUILD_OPTIONS]

`DiskAnnOptions` projects the `WITH (...)` storage parameters; `DiskAnnLayout` (`[SmartEnum]`) carries `MemoryOptimized`/`Plain`.

| [INDEX] | [WITH_KEY]               | [FIELD]               | [TYPE]  | [DEFAULT]          | [SEMANTICS]                                            |
| :-----: | :----------------------- | :-------------------- | :------ | :----------------- | :----------------------------------------------------- |
|  [01]   | `storage_layout`         | `StorageLayout`       | text    | `memory_optimized` | `memory_optimized` enables SBQ; `plain` = full vectors |
|  [02]   | `num_neighbors`          | `NumNeighbors`        | integer | `50`               | max neighbors per node (graph degree)                  |
|  [03]   | `search_list_size`       | `SearchListSize`      | integer | `100`              | build-time candidate-list breadth                      |
|  [04]   | `max_alpha`              | `MaxAlpha`            | double  | `1.2`              | graph density / pruning aggression                     |
|  [05]   | `num_dimensions`         | `NumDimensions`       | integer | `0`                | dimensions to index; `0` indexes all                   |
|  [06]   | `num_bits_per_dimension` | `NumBitsPerDimension` | integer | `0`                | SBQ bits/dim; `0`=auto (2 if <900 dims, else 1)        |

## [04]-[QUERY_GUC]

Session GUCs the planner reads to widen a diskann scan, `SET LOCAL` per session/transaction by the `Query/retrieval#SEARCH_PROVISIONING_PROBE` binder.

| [INDEX] | [GUC]                            | [DEFAULT] | [SEMANTICS]                                        |
| :-----: | :------------------------------- | :-------- | :------------------------------------------------- |
|  [01]   | `diskann.query_search_list_size` | `100`     | query-time candidate-list breadth; higher = recall |
|  [02]   | `diskann.query_rescore`          | `50`      | full-precision rescore count after SBQ pre-filter  |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `vectorscale` registers `diskann` as an access method, so the `ServerExtension("vectorscale", AccessMethod: "diskann", Cascade: true)` row emits `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` and stays off the `ClusterConfig` `shared_preload_libraries` row.
- Index DDL lands through `MigrationBuilder.Sql` via `ServerExtension` `CreateSql` (`Element/identity#SCHEMA_VERDICT`), emitting `CREATE INDEX CONCURRENTLY ... USING diskann (<col> <ops_class>) WITH (<options>)`; the column stays the pgvector-mapped `vector(N)` type, so a diskann index adds no parallel vector column.
- `DiskAnnOptions` is the build-time `WITH` map owned by `Store/provisioning#SERVER_EXTENSIONS`; the `diskann.query_*` GUCs are the query-time search-lane knobs — disjoint owners.

[STACKING]:
- `api-pgvector-ef.md`: `DiskAnnOps.Cosine`/`L2`/`InnerProduct` reuse the catalogued `CosineDistance`/`L2Distance`/`MaxInnerProduct` distance functions, one operator driving both build and the `VectorMetric.Order` `Switch`-projected `ORDER BY`; a `vector(N)` distance query planner-routes through diskann with no rewrite, the always-present exact scan staying the `store.vector.route` correctness baseline diskann complements past RAM-resident HNSW.
- `api-pg-search.md`: `FusionRank.Fuse` composes the diskann vector branch with the `pg_search` BM25 branch in one reciprocal-rank-fusion CTE projecting identities, no learned reranker (`Query/retrieval#FUSION_AND_REUSE`); the probed embedding is generated upstream at `Compute/models#INFERENCE_MODES`.

[LOCAL_ADMISSION]:
- A `<#>` inner-product build against `storage_layout = plain` is a typed `ProvisionSql` `Fin.Fail` caught at the deploy gate before `Sql()` lands — SBQ requires `memory_optimized`.
- `vectorscale` installs through its `ServerExtension` `CASCADE` row, never a `shared_preload_libraries` entry and never linked into managed code.

[RAIL_LAW]:
- Package: `vectorscale` (server-side, deploy-image PG18)
- Owns: the `diskann` disk-backed StreamingDiskANN + SBQ ANN index over a pgvector `vector(N)` column
- Accept: the `CASCADE` install, `CREATE INDEX CONCURRENTLY ... USING diskann` with the `DiskAnnOptions` `WITH` map against `DiskAnnOps` ops classes, catalogued pgvector distance functions for query, the `diskann.query_*` GUCs `SET LOCAL` by the search-lane binder, the `FusionRank.Fuse` RRF stack with the pg_search branch
- Reject: linking into managed code, a parallel vector column beside the pgvector column, a `<#>`-on-`plain` build, a build-time `query_*` knob or a query-time build option, a `shared_preload_libraries` placement
