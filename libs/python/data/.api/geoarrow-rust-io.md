# [PY_DATA_API_GEOARROW_RUST_IO]

`geoarrow-rust-io` owns the GDAL-free geospatial file rail: native Rust readers and writers move FlatGeobuf, GeoParquet, GeoJSON, CSV, and live PostGIS between file bytes and GeoArrow-native Arrow `Table` memory, `ObjectStore` fronts S3/GCS/Azure for a local-copy-free read, and the `Parquet*` handles push bbox and row-group predicates into a scan. Every reader yields the `GeometryArray` memory `geoarrow-rust-core` owns and `geoarrow-rust-compute` consumes, so a file crosses the rail once as an Arrow capsule, never round-tripping through a Shapely scalar or a GDAL `OGR` layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-io`
- package: `geoarrow-rust-io`
- import: `geoarrow.rust.io`
- owner: `data`
- rail: geospatial-ingress
- entry points: import-only, namespace-packaged under `geoarrow.rust`; native `_io` module backs the surface and `enums` carries the encoding vocabulary
- capability: GDAL-free format IO on the geospatial-ingress rail between file bytes and GeoArrow-native `Table` memory — every reader and writer, cloud `ObjectStore` access, GeoParquet bbox and row-group pushdown, streamed output, and `GeoParquetEncoding` selection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cloud object store

`ObjectStore` fronts a remote S3/GCS/Azure bucket and threads into any reader or `Parquet*` handle as `fs`; construction takes a bucket root and an explicit `options` credential mapping rather than an environment-discovered one.

| [INDEX] | [SYMBOL]      | [CALL_SHAPE]                      | [CAPABILITY]                                      |
| :-----: | :------------ | :-------------------------------- | :------------------------------------------------ |
|  [01]   | `ObjectStore` | `ObjectStore(root, options=None)` | remote S3/GCS/Azure bucket handle for reader `fs` |

[PUBLIC_TYPE_SCOPE]: GeoParquet scan carriers

`ParquetFile` wraps one GeoParquet object and `ParquetDataset` a multi-file dataset over a shared filesystem; both read to a GeoArrow-native `Table` and expose Arrow schema, row and row-group counts, field CRS, and bbox pushdown, and `ParquetFile` reports per-row-group bounds for predicate planning. `read`/`read_async` share the scan keywords `batch_size`, `limit`, `offset`, `bbox`, `bbox_paths`.

| [INDEX] | [SYMBOL]                        | [CALL_SHAPE]                                       | [CAPABILITY]                                  |
| :-----: | :------------------------------ | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `ParquetFile`                   | `ParquetFile(path, fs)`                            | single-object GeoParquet scan handle          |
|  [02]   | `ParquetFile.read`              | `read(*, <scan keywords>)`                         | scan to a GeoArrow `Table` with bbox pushdown |
|  [03]   | `ParquetFile.read_async`        | `read_async(*, <scan keywords>)`                   | awaitable bbox-pushdown scan                  |
|  [04]   | `ParquetFile.crs`               | `crs(column_name=None)`                            | field CRS as `pyproj.CRS \| None`             |
|  [05]   | `ParquetFile.file_bbox`         | `file_bbox()`                                      | primary-column spatial bounds                 |
|  [06]   | `ParquetFile.row_group_bounds`  | `row_group_bounds(row_group_idx, bbox_paths=None)` | bounds of one row group                       |
|  [07]   | `ParquetFile.row_groups_bounds` | `row_groups_bounds(bbox_paths=None)`               | bounds of every row group                     |
|  [08]   | `ParquetDataset`                | `ParquetDataset(paths, fs)`                        | multi-object GeoParquet dataset handle        |
|  [09]   | `ParquetDataset.read`           | `read(*, <scan keywords>)`                         | scan the dataset to a GeoArrow `Table`        |

[PUBLIC_TYPE_SCOPE]: streamed writer and encoding vocabulary

`ParquetWriter` streams `arro3` record batches or a whole `Table` to a GeoParquet sink and auto-closes as a context manager; `GeoParquetEncoding` selects `WKB` for the interoperable well-known-binary column or `Native` for the GeoArrow-native nested encoding.

| [INDEX] | [SYMBOL]                    | [CALL_SHAPE]                  | [CAPABILITY]                                 |
| :-----: | :-------------------------- | :---------------------------- | :------------------------------------------- |
|  [01]   | `ParquetWriter`             | `ParquetWriter(file, schema)` | open a streamed GeoParquet sink              |
|  [02]   | `ParquetWriter.write_batch` | `write_batch(batch)`          | append one Arrow record batch                |
|  [03]   | `ParquetWriter.write_table` | `write_table(table)`          | append a whole GeoArrow `Table`              |
|  [04]   | `ParquetWriter.close`       | `close()`                     | finalize and flush the file footer           |
|  [05]   | `ParquetWriter.is_closed`   | `is_closed() -> bool`         | writer-state guard                           |
|  [06]   | `GeoParquetEncoding`        | `WKB` \| `Native`             | geometry column encoding for `write_parquet` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file-to-GeoArrow readers

Each reader parses its format straight into a GeoArrow-native `Table` — no GDAL/`OGR` layer, no Shapely materialization; `fs` accepts an `ObjectStore` for cloud sources, `bbox` pushes a spatial filter into FlatGeobuf and GeoParquet, and the `_async` variants return awaitables. `read_postgis` runs a spatial SQL query against a PostGIS connection URL straight to Arrow.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                     | [CAPABILITY]                           |
| :-----: | :---------------------- | :--------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `read_flatgeobuf`       | `read_flatgeobuf(file, *, fs=None, batch_size=65536, bbox=None)` | parse FlatGeobuf with bbox pushdown    |
|  [02]   | `read_flatgeobuf_async` | `read_flatgeobuf_async(path, *, <same keywords>)`                | awaitable object-store FlatGeobuf read |
|  [03]   | `read_parquet`          | `read_parquet(path, *, fs=None, batch_size=65536)`               | parse GeoParquet to a GeoArrow `Table` |
|  [04]   | `read_parquet_async`    | `read_parquet_async(path, *, fs=None, batch_size=65536)`         | awaitable object-store GeoParquet read |
|  [05]   | `read_geojson`          | `read_geojson(file, *, batch_size=65536)`                        | parse a GeoJSON feature collection     |
|  [06]   | `read_geojson_lines`    | `read_geojson_lines(file, *, batch_size=65536)`                  | parse line-delimited GeoJSON           |
|  [07]   | `read_csv`              | `read_csv(file, geometry_column_name, *, batch_size=65536)`      | parse CSV with a named geometry column |
|  [08]   | `read_postgis`          | `read_postgis(connection_url, sql)`                              | run a PostGIS spatial query to Arrow   |
|  [09]   | `read_postgis_async`    | `read_postgis_async(connection_url, sql)`                        | awaitable PostGIS spatial query        |

[ENTRYPOINT_SCOPE]: GeoArrow-to-file writers

Each writer lowers a GeoArrow-native `Table` back to its format in one call; `write_flatgeobuf` writes a packed R-tree index by default and `write_parquet` selects the geometry column encoding through `GeoParquetEncoding`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                     | [CAPABILITY]                             |
| :-----: | :-------------------- | :--------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `write_flatgeobuf`    | `write_flatgeobuf(table, file, *, write_index=True)`             | write FlatGeobuf with a packed R-tree    |
|  [02]   | `write_parquet`       | `write_parquet(table, file, *, encoding=GeoParquetEncoding.WKB)` | write GeoParquet under a chosen encoding |
|  [03]   | `write_geojson`       | `write_geojson(table, file)`                                     | write a GeoJSON feature collection       |
|  [04]   | `write_geojson_lines` | `write_geojson_lines(table, file)`                               | write line-delimited GeoJSON             |
|  [05]   | `write_csv`           | `write_csv(table, file)`                                         | write CSV with WKT geometry              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds file bytes and GeoArrow-native `Table` memory in one crossing; a format this package parses natively never falls through to a GDAL/`OGR` layer or a Shapely scalar.
- `ObjectStore` is the sole remote-source handle, threaded as `fs`; the `_async` reader variants own the awaitable cloud path against a synchronous blocking read.
- `ParquetFile`/`ParquetDataset` push `bbox`/`bbox_paths`, `limit`, and `offset` into the scan and plan on `row_group_bounds`/`row_groups_bounds`, so a spatial subset reads only the intersecting row groups.
- `GeoParquetEncoding` fixes the geometry column encoding at write time against a post-write re-encode; `ParquetWriter` streams frames larger than memory under an explicit `close`.
- evidence: each call captures operation, format, source kind (local or `ObjectStore`), selected `batch_size`/`bbox`/encoding, and row and row-group counts as an ingress receipt.

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): every reader returns an `arro3.core.Table`; `ParquetWriter(file, schema)` binds an `arro3.core.Schema` and `write_batch`/`write_table` append `arro3.core` RecordBatch/Table/RecordBatchReader.
- `geoarrow-rust-core`(`.api/geoarrow-rust-core.md`): writers take the GeoArrow-native `Table` these carriers own, `row_groups_bounds` returns a `GeometryArray`, and an external GeoPandas source crosses through `geoarrow.rust.core.from_geopandas` before a writer call.
- `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`): reader output feeds the compute kernels zero-copy and writer input is their post-kernel geometry.
- `pyproj`(`.api/pyproj.md`): `ParquetFile.crs`/`ParquetDataset.crs` return a `pyproj.CRS`.
- within-lib: the rail folds reader to GeoArrow `Table` to compute kernel to writer as one capsule crossing, `ObjectStore` supplying every cloud source and the `Parquet*` handles pushing the spatial predicate so a subset never full-scans.

[LOCAL_ADMISSION]:
- import at boundary scope only (`from geoarrow.rust import io`); module-level import is banned by the manifest import policy.
- this package is the sole GDAL-free format-IO owner on the geospatial-ingress rail; a format it parses natively admits no `pyogrio`/GDAL fallthrough.

[RAIL_LAW]:
- Package: `geoarrow-rust-io`
- Owns: the GDAL-free geospatial file rail — native FlatGeobuf/GeoParquet/GeoJSON/GeoJSON-lines/CSV/PostGIS IO between file bytes and GeoArrow-native `Table` memory, cloud access through `ObjectStore`, GeoParquet bbox and row-group pushdown, streamed output through `ParquetWriter`, and `GeoParquetEncoding` selection
- Accept: format IO feeding the geospatial-ingress path and the `geoarrow.rust.core`/`geoarrow.rust.compute` memory model; cloud reads through an explicit `ObjectStore`; spatial pushdown over GeoParquet
- Reject: a GDAL/`pyogrio` layer for a natively-read format; a Shapely/GeoPandas container passed where a `Table` is required; a full-file scan where a `bbox` pushdown exists; a synchronous cloud read where an `_async` variant belongs; geometry construction or compute this package does not own
