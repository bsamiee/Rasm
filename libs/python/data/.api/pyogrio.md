# [PY_DATA_API_PYOGRIO]

`pyogrio` supplies vectorized OGR-backed vector file I/O over GDAL, reading and writing geospatial layers as GeoPandas `GeoDataFrame` or Arrow tables. It provides `read_dataframe`, `write_dataframe`, `read_arrow`, `write_arrow`, `open_arrow`, `read_info`, `list_layers`, and `list_drivers` as primary entrypoints, with attribute, geometry, spatial-filter, and SQL pushdown applied inside the OGR scan.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyogrio`
- package: `pyogrio`
- import: `import pyogrio`
- owner: `data`
- rail: geospatial
- capability: vectorized OGR vector read/write, GeoDataFrame and Arrow egress, attribute/geometry/bbox/mask/SQL pushdown, driver and layer metadata, GDAL configuration, and VSI virtual-filesystem control

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `pyogrio.errors.DataSourceError` — data source open or access failure; also re-exported as `pyogrio.raw.DataSourceError`.
- `pyogrio.errors.DataLayerError` — layer-level access failure.
- `pyogrio.errors.CRSError` — coordinate reference system parse or assignment failure.
- `pyogrio.errors.FeatureError` — feature read or write failure.
- `pyogrio.errors.FieldError` — attribute field type or access failure.
- `pyogrio.errors.GeometryError` — geometry encode, decode, or type failure.
- module flags: `pyogrio.__gdal_version__` (version tuple), `pyogrio.__gdal_geos_version__`, `pyogrio.raw.HAS_PYARROW`, `pyogrio.raw.HAS_ARROW_WRITE_API` gate Arrow-path availability.

[ENTRYPOINTS]:
- GeoDataFrame read: `read_dataframe(path_or_buffer, /, layer=None, encoding=None, columns=None, read_geometry=True, force_2d=False, skip_features=0, max_features=None, where=None, bbox=None, mask=None, fids=None, sql=None, sql_dialect=None, fid_as_index=False, use_arrow=None, on_invalid='raise', arrow_to_pandas_kwargs=None, datetime_as_string=False, mixed_offsets_as_utc=True, **kwargs) -> GeoDataFrame`.
- GeoDataFrame write: `write_dataframe(df, path, layer=None, driver=None, encoding=None, geometry_type=None, promote_to_multi=None, nan_as_null=True, append=False, use_arrow=None, dataset_metadata=None, layer_metadata=None, metadata=None, dataset_options=None, layer_options=None, **kwargs)`.
- Arrow read: `read_arrow(path_or_buffer, /, layer=None, columns=None, read_geometry=True, skip_features=0, max_features=None, where=None, bbox=None, mask=None, fids=None, sql=None, sql_dialect=None, return_fids=False, datetime_as_string=False, **kwargs) -> (meta, pyarrow.Table)`.
- Arrow stream: `open_arrow(path_or_buffer, /, ..., batch_size=65536, use_pyarrow=False, ...) -> contextmanager yielding (meta, reader)`; `write_arrow(arrow_obj, path, layer=None, driver=None, geometry_name=None, geometry_type=None, crs=None, append=False, ...)`.
- layer introspection: `read_info(path_or_buffer, /, layer=None, encoding=None, force_feature_count=False, force_total_bounds=False, **kwargs) -> dict`, `list_layers(path_or_buffer, /) -> ndarray`, `read_bounds(path_or_buffer, /, layer=None, skip_features=0, max_features=None, where=None, bbox=None, mask=None) -> (fids, bounds)`.
- driver discovery: `list_drivers(read=False, write=False) -> dict`, `detect_write_driver(path) -> str`.
- GDAL configuration: `set_gdal_config_options(options: dict)`, `get_gdal_config_option(name) -> value`, `get_gdal_data_path() -> str`.
- VSI virtual filesystem: `vsi_listtree(path, pattern=None) -> list[str]`, `vsi_rmtree(path)`, `vsi_unlink(path)`.
- raw OGR layer: `pyogrio.raw.read(...)`, `pyogrio.raw.write(...)`, `pyogrio.raw.read_arrow(...)`, `pyogrio.raw.open_arrow(...)`, `pyogrio.raw.get_gdal_version() -> tuple`, `pyogrio.raw.ogr_driver_supports_write(driver) -> bool`, `pyogrio.raw.ogr_driver_supports_vsi(driver) -> bool`.

[IMPLEMENTATION_LAW]:
- `read_dataframe` and `read_arrow` push `columns`, `where`, `bbox`, `mask`, `fids`, and `sql` down into the OGR scan; pushdown avoids materializing filtered features, so client-side post-filtering is the anti-pattern.
- `bbox` and `mask` are mutually exclusive spatial filters; `where` is an attribute filter and `sql` with `sql_dialect` is a full query, applied by the driver rather than in Python.
- The Arrow path (`use_arrow=True`, `read_arrow`, `open_arrow`) requires PyArrow and a GDAL Arrow-capable build; `read_dataframe(use_arrow=None)` auto-selects Arrow when available, degrading to the row-by-row path otherwise.
- `write_dataframe` infers the driver from the file extension when `driver=None` via `detect_write_driver`; `promote_to_multi` and `geometry_type` reconcile mixed single/multi geometry against driver constraints listed in `raw.DRIVERS_NO_MIXED_SINGLE_MULTI` and `raw.DRIVERS_NO_MIXED_DIMENSIONS`.
- `read_info` is the metadata-only entry; `force_feature_count` and `force_total_bounds` trade a full scan for exact counts and bounds when the driver does not cache them.
- `set_gdal_config_options` mutates process-global GDAL state; scope credential and network options at the call boundary rather than per-read.
- VSI paths (`/vsizip/`, `/vsicurl/`, `/vsimem/`) are accepted wherever a path is; `vsi_listtree`/`vsi_rmtree`/`vsi_unlink` manage the virtual filesystem in place.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyogrio`
- Owns: vectorized OGR vector file read/write, GeoDataFrame and Arrow egress, OGR-side attribute/geometry/spatial/SQL pushdown, driver and layer metadata, GDAL configuration, VSI virtual filesystem
- Accept: `read_dataframe`/`read_arrow` with pushdown filters (`columns`, `where`, `bbox`, `mask`, `fids`, `sql`), `read_info` for metadata-only inspection, `write_dataframe`/`write_arrow` with extension-inferred drivers, VSI paths for archive and remote access
- Reject: client-side filtering after a full read, Fiona-style per-feature iteration loops, hand-rolled OGR ctypes binding, and per-driver parallel read/write entrypoints when the polymorphic entry covers them
