# [PY_DATA_API_PYOGRIO]

`pyogrio` owns vectorized OGR-backed vector file I/O over GDAL, reading and writing geospatial layers as a GeoPandas `GeoDataFrame` or Arrow table with attribute, geometry, spatial, and SQL filters pushed into the OGR scan. Geometry, CRS, and columnar concerns delegate to their owners — the DataFrame path returns `shapely` geometry and a `pyproj.CRS`, the Arrow path yields a zero-copy ArrowStream PyCapsule — so pyogrio holds the geospatial IO edge without materializing filtered features.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyogrio`
- package: `pyogrio` (MIT)
- module: `import pyogrio`
- owner: `data`
- rail: geospatial
- native: GDAL/OGR Cython binding bundling `libgdal`; `__gdal_version__`/`__gdal_geos_version__` report the linked native versions, and `raw.HAS_PYARROW`/`raw.HAS_ARROW_WRITE_API` gate the Arrow read/write path
- asset: pure-Python surface over the OGR binding; the DataFrame path binds `geopandas`+`shapely`, the Arrow path binds `pyarrow`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the OGR failure rail, each error re-exported at `pyogrio.raw`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :---------------- | :------------ | :--------------------------------------- |
|  [01]   | `DataSourceError` | class         | data source open or access failure       |
|  [02]   | `DataLayerError`  | class         | layer-level access failure               |
|  [03]   | `CRSError`        | class         | CRS parse or assignment failure          |
|  [04]   | `FeatureError`    | class         | feature read or write failure            |
|  [05]   | `FieldError`      | class         | attribute field type or access failure   |
|  [06]   | `GeometryError`   | class         | geometry encode, decode, or type failure |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OGR read and write, layer metadata, driver discovery, GDAL configuration, VSI virtual filesystem
- read family carry: `layer`, `encoding`, `columns`, `read_geometry`, `force_2d`, `skip_features`, `max_features`, `where`, `bbox`, `mask`, `fids`, `sql`, `sql_dialect`
- write family carry: `layer`, `driver`, `encoding`, `geometry_type`, `append`, `dataset_metadata`, `layer_metadata`, `metadata`, `dataset_options`, `layer_options`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `read_dataframe(path, /, *, use_arrow) -> GeoDataFrame`        | static  | GeoDataFrame read with OGR pushdown        |
|  [02]   | `write_dataframe(df, path, *, use_arrow, append)`              | static  | GeoDataFrame write, driver from extension  |
|  [03]   | `read_arrow(path, /, *, return_fids) -> (meta, Table)`         | static  | Arrow table, WKB geometry, CRS/schema meta |
|  [04]   | `open_arrow(path, /, *, batch_size) -> ctx(meta, reader)`      | static  | streamed ArrowArrayStream PyCapsule        |
|  [05]   | `write_arrow(obj, path, *, geometry_name, crs)`                | static  | write any Arrow C-stream, WKB geometry     |
|  [06]   | `read_info(path, /, *, force_total_bounds) -> dict`            | static  | metadata-only layer inspection             |
|  [07]   | `read_bounds(path, /, *, where, bbox, mask) -> (fids, bounds)` | static  | per-feature bounds, no geometry decode     |
|  [08]   | `list_layers(path, /) -> ndarray`                              | static  | `[name, geometry_type]` rows per layer     |
|  [09]   | `list_drivers(read, write, append) -> dict`                    | static  | driver capability map                      |
|  [10]   | `list_drivers_details() -> dict`                               | static  | per-driver capability detail               |
|  [11]   | `detect_write_driver(path) -> str`                             | static  | driver from file extension                 |

- `open_arrow`: `use_pyarrow=False` yields a raw PyCapsule ArrowArrayStream, `True` a `pyarrow.RecordBatchReader`.
- `read_dataframe`: `use_arrow=None` auto-selects the Arrow path where the build carries it, degrading to the row-by-row reader otherwise.

[config]: `set_gdal_config_options(dict)` `get_gdal_config_option(name)` `get_gdal_data_path()`
[vsi]: `vsi_listtree(path, pattern)` `vsi_rmtree(path)` `vsi_unlink(path)` `vsi_curl_clear_cache(prefix)`

[raw]: `read` `write` `read_arrow` `open_arrow` `write_arrow` `ogr_read` `ogr_write` `ogr_open_arrow` `ogr_write_arrow` `get_gdal_version` `get_gdal_version_string` `ogr_driver_supports_write` `ogr_driver_supports_vsi` `vsi_path` `get_vsi_path_or_buffer` `GDALEnv` `DRIVERS_NO_MIXED_SINGLE_MULTI` `DRIVERS_NO_MIXED_DIMENSIONS`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `read_dataframe`/`read_arrow`/`open_arrow` push `columns`, `where`, `bbox`, `mask`, `fids`, and `sql` into the OGR scan; the driver evaluates them, so a filtered feature never materializes and client-side post-filtering is the anti-pattern.
- `bbox` and `mask` are mutually exclusive spatial filters, `where` is the attribute filter, and `sql` with `sql_dialect` is a full driver query; the three compose at the OGR edge.
- `write_dataframe` infers the driver from the extension through `detect_write_driver` when `driver=None`; `promote_to_multi` and `geometry_type` reconcile mixed single/multi geometry against `raw.DRIVERS_NO_MIXED_SINGLE_MULTI` and `raw.DRIVERS_NO_MIXED_DIMENSIONS`.
- `set_gdal_config_options` mutates process-global GDAL state; scope credential and network options at the call boundary, never per read.
- VSI paths (`/vsizip/`, `/vsicurl/`, `/vsimem/`) are accepted wherever a path is; `read_info` `capabilities` reports the driver's random-read and fast-feature-count support before a scan commits.

[STACKING]:
- `geopandas`(`.api/geopandas.md`): `geopandas.read_file`/`to_file` dispatch to `read_dataframe`/`write_dataframe` under the pyogrio engine, returning `shapely` geometry and a `pyproj.CRS`; `list_layers` enumerates layers before a scoped read.
- `pyarrow`(`.api/pyarrow.md`): `open_arrow(..., use_pyarrow=False)` yields a PyCapsule ArrowArrayStream that `pyarrow.RecordBatchReader.from_stream` consumes without materializing the layer; `write_arrow` accepts any `pyarrow.Table`/`RecordBatchReader`.
- `polars-st`(`.api/polars-st.md`), `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`), `duckdb`(`.api/duckdb.md`), `datafusion`(`.api/datafusion.md`): the Arrow C-stream registers zero-copy into the columnar engines and geoarrow decodes WKB to native geometry, so vector features reach the engines with no GeoDataFrame intermediary or per-feature loop.
- `stac-geoparquet`(`.api/stac-geoparquet.md`): a pushed-down `read_arrow(bbox=, where=, columns=)` feeds already-filtered rows, and `write_arrow(table, path, driver='Parquet')` is the inverse egress.
- `pyproj`(`.api/pyproj.md`): the reader emits CRS as WKT that `pyproj.CRS.from_user_input` ingests directly, never re-parsed.
- within-lib: VSI paths with `set_gdal_config_options({'CPL_VSIL_CURL_*': ...})` read a zipped GeoPackage straight from object storage, complementary to the substrate `obstore` owner for the non-OGR blobs.

[LOCAL_ADMISSION]:
- Admit `pyogrio` as the OGR vector-IO owner on the data geospatial rail, feeding both the GeoDataFrame and Arrow egress paths rather than a per-driver or per-feature reader.

[RAIL_LAW]:
- Package: `pyogrio`
- Owns: vectorized OGR vector file read/write, GeoDataFrame and Arrow egress, OGR-side attribute/geometry/spatial/SQL pushdown, layer and driver metadata, GDAL configuration, VSI virtual filesystem
- Accept: `read_dataframe`/`read_arrow`/`open_arrow` with pushdown filters, `read_info` for metadata-only inspection, `write_dataframe`/`write_arrow` with extension-inferred drivers, VSI paths for archive and remote access
- Reject: client-side filtering after a full read, per-feature iteration loops, hand-rolled OGR ctypes binding, and per-driver read/write entrypoints where the polymorphic entry covers them
