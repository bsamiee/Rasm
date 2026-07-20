# [PY_DATA_API_GEOARROW_RUST_IO]

`geoarrow-rust-io` supplies the GDAL-free geospatial file rail for the data ingress path: native Rust readers parse FlatGeobuf, GeoParquet, GeoJSON, line-delimited GeoJSON, CSV, and live PostGIS straight into GeoArrow-native Arrow `Table` memory, and matching writers lower a GeoArrow `Table` back to each format. `ObjectStore` fronts S3/GCS/Azure so a reader streams a cloud object without a local copy, `ParquetFile`/`ParquetDataset` expose row-group and bbox-pushdown scan control over one or many GeoParquet objects, and `ParquetWriter` streams record batches to a GeoParquet sink. Every reader yields the same `GeometryArray`/`ChunkedGeometryArray` memory the `geoarrow-rust-core` carriers own and the `geoarrow-rust-compute` kernels consume, so a file crosses the ingress path once as an Arrow capsule and never round-trips through a Shapely scalar or a GDAL `OGR` layer; it never re-implements the format parsers the underlying GeoRust crate already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-io`
- package: `geoarrow-rust-io`
- import: `geoarrow.rust.io`
- owner: `data`
- rail: geospatial-ingress
- version: `0.3.0`
- entry points: library use is import-only; no console script; namespace-packaged under `geoarrow.rust`; native module `_io` backs the surface and `enums` carries the layout vocabulary
- capability: GDAL-free format IO between GeoArrow-native Arrow `Table` memory and FlatGeobuf/GeoParquet/GeoJSON/GeoJSON-lines/CSV/PostGIS — sync and async readers (`read_flatgeobuf`/`read_flatgeobuf_async`, `read_geojson`, `read_geojson_lines`, `read_parquet`/`read_parquet_async`, `read_csv`, `read_postgis`/`read_postgis_async`) and writers (`write_flatgeobuf`, `write_geojson`, `write_geojson_lines`, `write_parquet`, `write_csv`), cloud object access through `ObjectStore` (S3/GCS/Azure), row-group and bbox-pushdown GeoParquet scan through `ParquetFile`/`ParquetDataset`, streamed GeoParquet output through `ParquetWriter`, and `GeoParquetEncoding` (`WKB`/`Native`) column-encoding selection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cloud object store
- rail: geospatial-ingress

`ObjectStore` fronts a remote bucket for uniform S3/GCS/Azure access and threads into any reader as the `fs` argument; construction takes a bucket root and an `options`/`client_options` credential mapping supplied explicitly rather than discovered from the environment.

| [INDEX] | [SYMBOL]      | [CALL_SHAPE]                                              | [CAPABILITY]                                      |
| :-----: | :------------ | :------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `ObjectStore` | `ObjectStore(root, options=None, client_options=None)`   | remote S3/GCS/Azure bucket handle for reader `fs` |

[PUBLIC_TYPE_SCOPE]: GeoParquet scan carriers
- rail: geospatial-ingress

`ParquetFile` wraps one GeoParquet object and `ParquetDataset` a multi-file dataset over a shared filesystem; both read to a GeoArrow-native `Table` and expose Arrow schema, row and row-group counts, field CRS, and bbox pushdown, and `ParquetFile` reports per-row-group spatial bounds for predicate planning. `read`/`read_async` share the scan keywords `batch_size`, `limit`, `offset`, `bbox`, `bbox_paths`, each defaulting to `None`.

| [INDEX] | [SYMBOL]                        | [CALL_SHAPE]                                | [CAPABILITY]                                   |
| :-----: | :------------------------------ | :------------------------------------------ | :--------------------------------------------- |
|  [01]   | `ParquetFile`                   | `ParquetFile(path, fs)`                     | single-object GeoParquet scan handle           |
|  [02]   | `ParquetFile.read`              | `read(*, <scan keywords>)`                  | scan to a GeoArrow `Table` with bbox pushdown  |
|  [03]   | `ParquetFile.read_async`        | `read_async(*, <scan keywords>)`            | awaitable bbox-pushdown scan                   |
|  [04]   | `ParquetFile.crs`               | `crs(column_name=None)`                     | field CRS as `pyproj.CRS \| None`              |
|  [05]   | `ParquetFile.file_bbox`         | `file_bbox(column_name=None)`               | whole-file spatial bounds                      |
|  [06]   | `ParquetFile.row_group_bounds`  | `row_group_bounds(row_group_idx, bbox_paths=None)` | bounds of one row group                 |
|  [07]   | `ParquetFile.row_groups_bounds` | `row_groups_bounds(bbox_paths=None)`        | bounds of every row group                      |
|  [08]   | `ParquetDataset`                | `ParquetDataset(paths, fs)`                 | multi-object GeoParquet dataset handle         |
|  [09]   | `ParquetDataset.read`           | `read(*, <scan keywords>)`                  | scan the dataset to a GeoArrow `Table`         |

[PUBLIC_TYPE_SCOPE]: streamed writer and encoding vocabulary
- rail: geospatial-ingress

`ParquetWriter` streams `arro3` record batches or a whole `Table` to a GeoParquet sink under an explicit `close`; `GeoParquetEncoding` selects the geometry column encoding — `WKB` for the interoperable well-known-binary column, `Native` for the GeoArrow-native nested encoding.

| [INDEX] | [SYMBOL]                    | [CALL_SHAPE]                          | [CAPABILITY]                                 |
| :-----: | :-------------------------- | :------------------------------------ | :------------------------------------------- |
|  [01]   | `ParquetWriter`             | `ParquetWriter(file, schema)`         | open a streamed GeoParquet sink              |
|  [02]   | `ParquetWriter.write_batch` | `write_batch(batch)`                  | append one Arrow record batch                |
|  [03]   | `ParquetWriter.write_table` | `write_table(table)`                  | append a whole GeoArrow `Table`              |
|  [04]   | `ParquetWriter.close`       | `close()`                             | finalize and flush the file footer           |
|  [05]   | `ParquetWriter.is_closed`   | `is_closed() -> bool`                 | writer-state guard                           |
|  [06]   | `GeoParquetEncoding`        | `WKB` \| `Native`                     | geometry column encoding for `write_parquet` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file-to-GeoArrow readers
- rail: geospatial-ingress

Each reader parses a format directly into a GeoArrow-native Arrow `Table` — no GDAL/`OGR` layer, no Shapely materialization; `fs` accepts an `ObjectStore` for cloud sources, `batch_size` bounds record-batch width, `bbox` pushes a spatial filter into FlatGeobuf and GeoParquet, and the `_async` variants return awaitables for the object-store path. `read_postgis` runs a spatial SQL query against a PostGIS connection URL straight to Arrow.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                          | [CAPABILITY]                            |
| :-----: | :---------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | `read_flatgeobuf`       | `read_flatgeobuf(file, *, fs=None, batch_size=65536, bbox=None)` | parse FlatGeobuf with bbox pushdown |
|  [02]   | `read_flatgeobuf_async` | `read_flatgeobuf_async(path, *, <same keywords>)`    | awaitable object-store FlatGeobuf read  |
|  [03]   | `read_parquet`          | `read_parquet(path, *, fs=None, batch_size=None)`    | parse GeoParquet to a GeoArrow `Table`  |
|  [04]   | `read_parquet_async`    | `read_parquet_async(path, *, fs=None, batch_size=None)` | awaitable object-store GeoParquet read |
|  [05]   | `read_geojson`          | `read_geojson(file, *, batch_size=65536)`            | parse a GeoJSON feature collection      |
|  [06]   | `read_geojson_lines`    | `read_geojson_lines(file, *, batch_size=65536)`      | parse line-delimited GeoJSON            |
|  [07]   | `read_csv`              | `read_csv(file, geometry_column_name, *, batch_size=65536)` | parse CSV with a named geometry column |
|  [08]   | `read_postgis`          | `read_postgis(connection_url, sql)`                  | run a PostGIS spatial query to Arrow    |
|  [09]   | `read_postgis_async`    | `read_postgis_async(connection_url, sql)`            | awaitable PostGIS spatial query         |

[ENTRYPOINT_SCOPE]: GeoArrow-to-file writers
- rail: geospatial-ingress

Each writer lowers a GeoArrow-native `Table` back to its format in one call; `write_flatgeobuf` writes a spatial packed index by default, and `write_parquet` selects the geometry column encoding through `GeoParquetEncoding`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                       | [CAPABILITY]                              |
| :-----: | :-------------------- | :------------------------------------------------- | :---------------------------------------- |
|  [01]   | `write_flatgeobuf`    | `write_flatgeobuf(table, file, *, write_index=True)` | write FlatGeobuf with a packed R-tree   |
|  [02]   | `write_parquet`       | `write_parquet(table, file, *, encoding='WKB')`    | write GeoParquet under a chosen encoding  |
|  [03]   | `write_geojson`       | `write_geojson(table, file)`                       | write a GeoJSON feature collection        |
|  [04]   | `write_geojson_lines` | `write_geojson_lines(table, file)`                 | write line-delimited GeoJSON              |
|  [05]   | `write_csv`           | `write_csv(table, file)`                           | write CSV with WKT geometry               |

## [04]-[IMPLEMENTATION_LAW]

[GEOSPATIAL_INGRESS_IO]:
- import: `from geoarrow.rust import io` (or `import geoarrow.rust.io`) at boundary scope only; module-level import is banned by the manifest import policy.
- reader axis: `read_flatgeobuf`/`read_parquet`/`read_geojson`/`read_geojson_lines`/`read_csv`/`read_postgis` own every format-to-GeoArrow entry — one reader per format, never a GDAL/`OGR` fallthrough for a format this package parses natively; each returns a GeoArrow-native `Table` the `geoarrow-rust-core` carriers and the `geoarrow-rust-compute` kernels consume zero-copy.
- writer axis: `write_flatgeobuf`/`write_parquet`/`write_geojson`/`write_geojson_lines`/`write_csv` own every GeoArrow-to-format egress; a writer takes a GeoArrow `Table` and never a Shapely/GeoPandas container — cross to `geoarrow.rust.core.from_geopandas` first when the source is external.
- cloud axis: `ObjectStore` is the sole remote-source handle, threaded as `fs` into the readers and the `Parquet*` handles; credentials pass explicitly through `options`/`client_options`, and the `_async` reader variants own the awaitable object-store path — never a synchronous blocking read against a cloud object.
- pushdown axis: `ParquetFile`/`ParquetDataset` own GeoParquet scan planning — `bbox`/`bbox_paths` spatial pushdown, `limit`/`offset` slicing, `row_group_bounds`/`row_groups_bounds` predicate planning, and `crs`/`file_bbox`/`schema_arrow` metadata — so a spatial subset reads only the intersecting row groups, never a full-file scan then filter.
- streaming axis: `ParquetWriter` owns incremental GeoParquet output under an explicit `close`; `write_batch`/`write_table` append and `is_closed` guards state, so a large frame streams to disk without a single in-memory materialization.
- encoding axis: `GeoParquetEncoding` selects `WKB` for the interoperable well-known-binary geometry column or `Native` for the GeoArrow-native nested encoding at write time, never a post-write re-encode.
- evidence: each call captures operation name, format, source kind (local/`ObjectStore`), selected `batch_size`/`bbox`/encoding, and row and row-group counts as an ingress receipt.

[RAIL_LAW]:
- Package: `geoarrow-rust-io`
- Owns: the GDAL-free geospatial file rail — native FlatGeobuf/GeoParquet/GeoJSON/GeoJSON-lines/CSV/PostGIS readers and writers between file bytes and GeoArrow-native Arrow `Table` memory, cloud object access through `ObjectStore`, GeoParquet row-group and bbox-pushdown scan through `ParquetFile`/`ParquetDataset`, streamed GeoParquet output through `ParquetWriter`, and `GeoParquetEncoding` column-encoding selection
- Accept: format IO feeding the geospatial-ingress path and the `geoarrow.rust.core`/`geoarrow.rust.compute` memory model; cloud-object reads through an explicit `ObjectStore`; spatial pushdown and row-group planning over GeoParquet
- Reject: a GDAL/`pyogrio` layer for a format this package reads natively; a Shapely/GeoPandas container passed to a writer where a `Table` is required; a full-file scan where a `bbox` pushdown exists; a synchronous cloud read where the `_async` variant belongs; geometry construction (`geoarrow.rust.core`) or geometry compute (`geoarrow.rust.compute`) this package does not own
