# [RASM_PERSISTENCE_API_PGVECTORSCALE]

`pgvectorscale` (Timescale `vectorscale`) supplies the `diskann` index access method over a
`pgvector` `vector(N)` column — a disk-backed StreamingDiskANN graph with statistical binary
quantization (SBQ) that scales approximate-nearest-neighbour search beyond RAM-resident HNSW. It
carries no managed assembly: every surface is server-side SQL the `Store/server#SEARCH_PROVISIONING`
`IndexSpec.DiskAnn` fold emits through `MigrationBuilder.Sql` and the `Query/lanes#SEARCH_LANES`
planner routes a `pgvector` distance query through transparently. The companion is preload-free —
it registers through its index AM, so it is correctly absent from the `ClusterConfig` preload row —
and runs in-process inside the PG18 server tier, never linked into managed code.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pgvectorscale`
- package: `pgvectorscale` (server-side PostgreSQL extension, not a NuGet package)
- access method: `diskann`
- namespace: SQL (`CREATE INDEX ... USING diskann`)
- asset: server extension, preload-free (index AM registration)
- rail: search-provisioning, search-lanes

## [2]-[INDEX_DDL]

The `diskann` index builds over a `vector(N)` column under one ops class. The column stays the
`EmbeddingArity.Dense` `vector` store type — `halfvec`/`sparsevec` over diskann are not catalogued
(held under `[SEARCH_PROVISIONING_PROBE]`). One BM25-style restriction does not apply; a table may
carry multiple diskann indexes over distinct vector columns.

| [INDEX] | [OPS_CLASS]         | [OPERATOR] | [DISTANCE]          | [PGVECTOR_FN]     |
| :-----: | :------------------ | :--------- | :------------------ | :---------------- |
|   [1]   | `vector_cosine_ops` | `<=>`      | cosine              | `CosineDistance`  |
|   [2]   | `vector_l2_ops`     | `<->`      | L2 / euclidean      | `L2Distance`      |
|   [3]   | `vector_ip_ops`     | `<#>`      | negative inner-prod | `MaxInnerProduct` |

`CREATE INDEX <name> ON <table> USING diskann (<column> <ops_class>) WITH (<build-options>)` is the
canonical build. `CREATE INDEX CONCURRENTLY` builds without an `ACCESS EXCLUSIVE` table lock. The
`<#>` inner-product operator is rejected against a `storage_layout = plain` build — SBQ requires
`memory_optimized`.

## [3]-[BUILD_OPTIONS]

The `WITH (...)` storage parameters the `DiskAnnOptions` record projects. Sentinel values resolve at
build time from the column dimensionality.

| [INDEX] | [OPTION]                 | [TYPE]  | [DEFAULT]          | [SEMANTICS]                                                      |
| :-----: | :----------------------- | :------ | :----------------- | :--------------------------------------------------------------- |
|   [1]   | `storage_layout`         | text    | `memory_optimized` | `memory_optimized` enables SBQ; `plain` stores full vectors      |
|   [2]   | `num_neighbors`          | integer | `50`               | graph degree; `-1` derives degree from dimensionality            |
|   [3]   | `search_list_size`       | integer | `100`              | build-time candidate-list breadth                                |
|   [4]   | `max_alpha`              | double  | `1.2`              | graph density / pruning aggression                               |
|   [5]   | `num_dimensions`         | integer | `0`                | dimensions to index; `0` indexes all                             |
|   [6]   | `num_bits_per_dimension` | integer | `0`                | SBQ bits/dim; `0` auto-selects (2 below 900 dims, 1 at or above) |

## [4]-[QUERY_GUC]

The session GUC the planner reads at query time to widen the search-list breadth for a diskann scan;
set per-session or per-transaction, never a build option.

| [INDEX] | [GUC]                            | [DEFAULT] | [SEMANTICS]                                        |
| :-----: | :------------------------------- | :-------- | :------------------------------------------------- |
|   [1]   | `diskann.query_search_list_size` | `100`     | query-time candidate-list breadth; higher = recall |
|   [2]   | `diskann.query_rescore`          | `50`      | full-precision rescore count after SBQ pre-filter  |
