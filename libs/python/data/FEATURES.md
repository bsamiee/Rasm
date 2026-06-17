# [PY_DATA_FEATURES]

The realized capability list for portable data interchange. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[COLUMNAR_AND_QUERY]

The dataset identity owner, the cross-engine scan and egress, the Delta lakehouse, the relational query engine, and the data-quality gate.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]            |
| :-----: | :------------------------------------------------------------------------ | :----------------------- |
|   [1]   | One polymorphic dataset ref discriminating by source shape               | columnar-query#DATASET   |
|   [2]   | Engine scan plans: Polars lazy, DuckDB relational, PyArrow dataset        | columnar-query#SCAN      |
|   [3]   | Typed Arrow/Parquet/IPC egress folding one query receipt                  | columnar-query#SCAN      |
|   [4]   | Delta lakehouse: write/read/time-travel/optimize/vacuum/changefeed        | columnar-query#LAKEHOUSE |
|   [5]   | Relational query engine over duckdb-sql/relational and narwhals           | columnar-query#QUERY     |
|   [6]   | Data-quality gate folding IDS-style rules into one pandera schema          | columnar-query#QUALITY   |

## [2]-[INTEROP_AND_GEOSPATIAL]

The dataframe-agnostic interop boundary, the structural frame admission, and the two-axis geospatial surface.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]      |
| :-----: | :------------------------------------------------------------------------ | :------------------ |
|   [7]   | Backend-agnostic frame translation over narwhals with Arrow C-data rows   | schema-geo#INTEROP  |
|   [8]   | Structural field shapes with the null mask read from the live frame       | schema-geo#ADMISSION |
|   [9]   | Frame admission proving required field shapes against the live schema      | schema-geo#ADMISSION |
|  [10]   | Vector geospatial claim: CRS/units/axis-order/geometry-family             | schema-geo#GEO      |
|  [11]   | Raster geospatial claim: coverage/band/resampling/nodata                  | schema-geo#GEO      |

## [3]-[GRAPH_TENSOR_MESH]

The graph payload with backend-dispatched algorithms, the chunked tensor store, and mesh-file exchange.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]    |
| :-----: | :------------------------------------------------------------------------ | :---------------- |
|  [12]   | Graph payload over a rustworkx fast-path with a networkx compat row       | graph-mesh#GRAPH  |
|  [13]   | Typed graph algorithm receipts with tabular/JSON/GraphML/bundle egress    | graph-mesh#GRAPH  |
|  [14]   | Chunked N-D tensor store over zarr with cubed/awkward/icechunk rows       | graph-mesh#TENSOR |
|  [15]   | Mesh-file identity, cell-block topology, units, preview export            | graph-mesh#MESH   |
