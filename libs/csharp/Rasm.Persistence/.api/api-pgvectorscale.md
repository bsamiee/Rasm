# [RASM_PERSISTENCE_API_PGVECTORSCALE]

`pgvectorscale` (Timescale) is a PostgreSQL extension that adds the StreamingDiskANN
(`diskann`) index access method for approximate nearest-neighbour search over `vector`
columns, with scalar binary quantisation and optional memory-optimised storage.

## [1]-[EXTENSION_SURFACE]

[EXTENSION_SURFACE]: `vectorscale`
- extension: `vectorscale`
- access method: `diskann`
- version: `0.9.0`
- depends: `pgvector` (`vector` type and distance operators)
- asset: PostgreSQL extension (not a .NET assembly; not decompilable)
- rail: search-provisioning

[DDL_ADMISSION]:

| [INDEX] | [DDL]                                                                                | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------------------- | :----------------------------------------- |
|   [1]   | `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE;`                                | installs extension and pgvector dependency |
|   [2]   | `CREATE EXTENSION IF NOT EXISTS vector; CREATE EXTENSION IF NOT EXISTS vectorscale;` | explicit two-step admission                |

## [2]-[INDEX_DDL]

[INDEX_ACCESS_METHOD]: `diskann`
- access method: `diskann`
- column type: `vector`
- ops classes: `vector_cosine_ops`, `vector_l2_ops`, `vector_ip_ops`

[CREATE_INDEX_SHAPE]:

```sql
CREATE INDEX <name> ON <table>
USING diskann (<column> <ops_class>)
WITH (
    storage_layout         = 'memory_optimized',
    num_neighbors          = 50,
    search_list_size       = 100,
    max_alpha              = 1.2,
    num_dimensions         = 0,
    num_bits_per_dimension = 2
);
```

[BUILD_OPTIONS]:
- used by: `server-tier#SEARCH_PROVISIONING`
- evidence: GitHub `timescale/pgvectorscale` README and `options.rs`

| [INDEX] | [OPTION]                 | [VALUE_SHAPE]                     | [CAPABILITY]              |
| :-----: | :----------------------- | :-------------------------------- | :------------------------ |
|   [1]   | `storage_layout`         | `'memory_optimized'` or `'plain'` | storage representation    |
|   [2]   | `num_neighbors`          | `integer`, default `50`           | graph degree              |
|   [3]   | `search_list_size`       | `integer`, default `100`          | build search breadth      |
|   [4]   | `max_alpha`              | `real`, default `1.2`             | pruning alpha             |
|   [5]   | `num_dimensions`         | `integer`, default `0`            | indexed dimension prefix  |
|   [6]   | `num_bits_per_dimension` | `integer`, default `2`            | scalar quantisation width |

Sentinel values are `num_neighbors = -1` for dimension-derived graph degree,
`num_dimensions = 0` for all dimensions, and `num_bits_per_dimension = 0` for
automatic bit selection. The bit-width default is `2` below 900 dimensions and
`1` at 900 dimensions or above.

## [3]-[DISTANCE_OPERATORS]

[OPERATOR_SURFACE]: distance operators registered via pgvector; used in `ORDER BY` and `WHERE` against `diskann` indexes
- used by: `data-lanes#SEARCH_LANES`
- evidence: GitHub `timescale/pgvectorscale` README

| [INDEX] | [OPERATOR] | [OPS_CLASS]         | [CAPABILITY]           |
| :-----: | :--------- | :------------------ | :--------------------- |
|   [1]   | `<=>`      | `vector_cosine_ops` | cosine distance        |
|   [2]   | `<->`      | `vector_l2_ops`     | L2 distance            |
|   [3]   | `<#>`      | `vector_ip_ops`     | negative inner product |

`<#>` is incompatible with `storage_layout = 'plain'`.

## [4]-[GUC_PARAMETERS]

[GUC_SURFACE]: session/transaction GUCs controlling query-time ANN behaviour
- used by: `data-lanes#SEARCH_LANES`
- evidence: GitHub `timescale/pgvectorscale` README and PostgreSQL core docs

| [INDEX] | [PARAMETER]                      | [SET_SHAPE]                                 | [CAPABILITY]             |
| :-----: | :------------------------------- | :------------------------------------------ | :----------------------- |
|   [1]   | `diskann.query_search_list_size` | `SET diskann.query_search_list_size = 100;` | query search breadth     |
|   [2]   | `diskann.query_rescore`          | `SET diskann.query_rescore = 50;`           | quantised-result rescore |
|   [3]   | `enable_seqscan`                 | `SET enable_seqscan = off;`                 | forced index usage       |

`diskann.query_rescore` defaults to `50`; `0` disables rescoring, and the GUC
only affects indexes with `storage_layout = 'memory_optimized'`. `enable_seqscan`
is a core PostgreSQL GUC used by benchmark and proof queries.

## [5]-[IMPLEMENTATION_LAW]

[SEARCH_PROVISIONING]:
- extension: `vectorscale`
- index access method: `diskann`
- ops class maps to operator: `vector_cosine_ops` → `<=>`, `vector_l2_ops` → `<->`, `vector_ip_ops` → `<#>`
- `storage_layout='plain'` disables SBQ; `query_rescore` GUC is a no-op on plain indexes
- `num_dimensions > 0` enables Matryoshka truncated indexing; index uses prefix embeddings, rescores with full vectors
- `num_neighbors` and `num_bits_per_dimension` sentinel values (`-1`, `0`) are resolved at build time from vector dimensionality

[RAIL_LAW]:
- Extension: `vectorscale`
- Owns: diskann ANN index over pgvector columns
- Accept: profile-declared search indexes and query-scoped GUC overrides
- Reject: extension DDL scattered outside store-profile migrations
