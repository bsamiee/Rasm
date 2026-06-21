# [PY_DATA_API_RIOXARRAY]

`rioxarray` extends `xarray` with the rasterio-backed `.rio` accessor for the COVERAGE/CATALOG_COVERAGE_ODCSTAC raster IO rail: `open_rasterio` reads any GDAL-openable raster into a georeferenced `DataArray`/`Dataset`, and the `.rio` accessor (`RasterArray` on `DataArray`, `RasterDataset` on `Dataset`, both extending `XRasterBase`) owns CRS/transform/nodata metadata, `reproject`/`reproject_match` warping, `clip`/`clip_box` masking, `pad_box`/`slice_xy`/`isel_window` windowing, and `to_raster` GeoTIFF writeback. The `merge_arrays`/`merge_datasets` functions mosaic tiles; `Convention` selects CF or Zarr metadata encoding. The package owner composes `open_rasterio` and the `.rio` accessor into the coverage read path; it carries chunking, masking, and CRS as call rows, registers accessors at import, and never re-implements the rasterio/GDAL raster decode xarray already binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rioxarray`
- package: `rioxarray`
- import: `import rioxarray`
- owner: `data`
- rail: geospatial
- version: `0.22.0`
- license: Apache-2.0 (vendors `LICENSE_xarray` and `LICENSE_datacube`)
- asset: pure-Python; `py3-none-any` wheel (`Root-Is-Purelib: true`), no ABI floor of its own — `Requires-Python >=3.12`. The cp315 ABI floor is inherited from its `rasterio` dependency (the locked `rasterio==1.5.0` cp313 wheel does not load on the cp315 core), so `.rio` access is gated on the rasterio sync, not on rioxarray itself. Accessor and module signatures below are reflection-verified against the locked `0.22.0` wheel source (`rioxarray.py`/`raster_array.py`/`raster_dataset.py`/`_io.py`/`exceptions.py`); runtime deps `numpy`/`pyproj`/`xarray`/`packaging`/`rasterio`.
- entry points: xarray backend `rasterio = rioxarray.xarray_plugin:RasterioBackend` (registers `engine="rasterio"` for `xarray.open_dataset`); no console script; library use is import-only, which registers the `.rio` accessor on `DataArray`/`Dataset`
- capability: rasterio/GDAL raster read into georeferenced `DataArray`/`Dataset` with auto pixel-center coordinates; dask chunking, masking, and `mask_and_scale`; CRS/transform/nodata read and CF/Zarr-convention write; reprojection and grid-matching warping; geometry and bounding-box clipping; spatial padding, slicing, and window selection; nodata interpolation; multi-tile array/dataset merge; GeoTIFF/driver writeback

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: accessor, convention, and error roots
- rail: geospatial

`open_rasterio` returns a `DataArray`, `Dataset`, or `list[Dataset]`; the `.rio` accessor materializes `RasterArray` on a `DataArray` and `RasterDataset` on a `Dataset`, both subclassing the shared `XRasterBase` CRS/transform/window base. `Convention` selects the metadata encoding for `write_crs`/`write_transform`. `RioXarrayError` is the base failure; `NoDataInBounds`, `MissingCRS`, `OneDimensionalRaster`, and the dimension errors are the typed leaves.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `XRasterBase`                     | accessor base | shared CRS/transform/bounds/window surface for both accessors |
|  [02]   | `RasterArray`                     | accessor      | `DataArray.rio` GIS extension (reproject/clip/to_raster)      |
|  [03]   | `RasterDataset`                   | accessor      | `Dataset.rio` GIS extension over multi-variable rasters       |
|  [04]   | `Convention`                      | enum          | `CF`/`ZARR` geospatial metadata encoding selector             |
|  [05]   | `RioXarrayError`                  | error (base)  | base `RuntimeError` for rioxarray failures                    |
|  [06]   | `NoDataInBounds`                  | error         | clip bounds contain no data                                   |
|  [07]   | `MissingCRS`                      | error         | CRS absent from the dataset                                   |
|  [08]   | `DimensionError`                  | error (base)  | base for unsupported/malformed spatial dimensions             |
|  [09]   | `MissingSpatialDimensionError`    | error         | spatial dimension cannot be found                             |
|  [10]   | `TooManyDimensions`               | error         | more dimensions than the method supports                      |
|  [11]   | `InvalidDimensionOrder`           | error         | dimensions are not ordered correctly                          |
|  [12]   | `OneDimensionalRaster`            | error         | raster collapsed to one dimension                             |
|  [13]   | `SingleVariableDataset`           | error         | dataset method requires a single variable                     |
|  [14]   | `DimensionMissingCoordinateError` | error         | dimension lacks its supporting coordinate                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module functions
- rail: geospatial

`open_rasterio` is the read entry; `merge_arrays`/`merge_datasets` mosaic tiles; `set_options` toggles global CF-export and convention policy (context manager or global); `show_versions` prints dependency diagnostics.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                                                                                                                                                                                                                            | [CAPABILITY]                                                                                        |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `open_rasterio`        | `open_rasterio(filename, *, parse_coordinates=None, chunks=None, cache=None, lock=None, masked=False, mask_and_scale=False, variable=None, group=None, default_name=None, decode_times=True, decode_timedelta=None, band_as_variable=False, **open_kwargs) -> Union[Dataset, DataArray, list[Dataset]]` | open a GDAL raster into a georeferenced xarray object                                               |
|  [02]   | `merge.merge_arrays`   | `merge_arrays(dataarrays, *, bounds=None, res=None, nodata=None, precision=None, method=None, crs=None, parse_coordinates=True) -> DataArray`                                                                                                                                                           | mosaic a sequence of `DataArray` tiles                                                              |
|  [03]   | `merge.merge_datasets` | `merge_datasets(datasets, *, bounds=None, res=None, nodata=None, precision=None, method=None, crs=None) -> Dataset`                                                                                                                                                                                     | mosaic a sequence of `Dataset` tiles                                                                |
|  [04]   | `set_options`          | `set_options(**kwargs)`                                                                                                                                                                                                                                                                                 | set `export_grid_mapping`/`skip_missing_spatial_dims`/`convention` globally or as a context manager |
|  [05]   | `show_versions`        | `show_versions() -> None`                                                                                                                                                                                                                                                                               | print rioxarray and dependency versions                                                             |

[ENTRYPOINT_SCOPE]: `XRasterBase` shared accessor surface
- rail: geospatial

Both `.rio` accessors inherit CRS/transform metadata read-write, dimension binding, and window/bounds geometry from `XRasterBase`. Write methods carry `inplace` and `convention`; geometry methods read `x_dim`/`y_dim`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                                      | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `XRasterBase.crs`              | property -> `Optional[rasterio.crs.CRS]`                                                          | resolved coordinate reference system       |
|  [02]   | `XRasterBase.set_crs`          | `set_crs(input_crs, inplace=True)`                                                                | set CRS without writing CF metadata        |
|  [03]   | `XRasterBase.write_crs`        | `write_crs(input_crs=None, grid_mapping_name=None, convention=None, inplace=False)`               | write CRS as CF/Zarr grid-mapping metadata |
|  [04]   | `XRasterBase.grid_mapping` / `write_grid_mapping` | property -> `str`; `write_grid_mapping(grid_mapping_name='spatial_ref', inplace=False)` | grid-mapping variable name read/write   |
|  [05]   | `XRasterBase.write_transform`  | `write_transform(transform=None, grid_mapping_name=None, convention=None, inplace=False)`         | persist the affine transform to metadata   |
|  [06]   | `XRasterBase.write_coordinate_system` | `write_coordinate_system(inplace=False)`                                                   | write CF axis/coordinate-system attrs      |
|  [07]   | `XRasterBase.transform`        | `transform(recalc=False) -> Affine`                                                               | affine geotransform                        |
|  [08]   | `XRasterBase.estimate_utm_crs` | `estimate_utm_crs(datum_name='WGS 84') -> rasterio.crs.CRS`                                       | infer the local UTM CRS                    |
|  [09]   | `XRasterBase.set_spatial_dims` | `set_spatial_dims(x_dim, y_dim, inplace=True)`                                                    | bind the x/y spatial dimension names       |
|  [10]   | `XRasterBase.resolution`       | `resolution(recalc=False) -> tuple[float, float]`                                                 | pixel resolution                           |
|  [11]   | `XRasterBase.bounds`           | `bounds(*, recalc=False) -> tuple[float, float, float, float]`                                    | spatial bounding box                       |
|  [12]   | `XRasterBase.transform_bounds` | `transform_bounds(dst_crs, *, densify_pts=21, recalc=False) -> tuple[float, float, float, float]` | reproject the bounding box to another CRS  |
|  [13]   | `XRasterBase.slice_xy`         | `slice_xy(minx, miny, maxx, maxy)`                                                                | spatial slice by coordinate box            |
|  [14]   | `XRasterBase.isel_window`      | `isel_window(window, *, pad=False)`                                                               | select a rasterio `Window` region          |
|  [15]   | `XRasterBase.write_gcps` / `get_gcps` | `write_gcps(gcps, gcp_crs, *, grid_mapping_name=None, inplace=False)`; `get_gcps()`        | persist / read ground control points       |
|  [16]   | `XRasterBase.write_rpcs` / `get_rpcs` | `write_rpcs(rpcs, *, grid_mapping_name=None, inplace=False)`; `get_rpcs()`                 | persist / read `rasterio.rpc.RPC` coefficients |
|  [17]   | `XRasterBase.set_attrs` / `update_attrs` / `set_encoding` / `update_encoding` | `(new_attrs|new_encoding, inplace=False)`                              | replace/merge CF attrs and encoding        |
|  [18]   | `XRasterBase.shape` / `width` / `height` / `count` | properties -> `tuple[int,int]` / `int` / `int` / `int`                          | `(height, width)` shape and band count     |

[ENTRYPOINT_SCOPE]: `RasterArray` / `RasterDataset` raster operations
- rail: geospatial

The `.rio` accessor methods share signatures across `DataArray` (`RasterArray`) and `Dataset` (`RasterDataset`); the return type tracks the object (`xarray.DataArray` vs `xarray.Dataset`). `nodata`/`encoded_nodata`/`set_nodata`/`write_nodata`/`to_rasterio_dataset` and `pad_xy` are `RasterArray`-only; `vars` is `RasterDataset`-only.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                                                                                                              | [CAPABILITY]                                    |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `reproject`                       | `reproject(dst_crs, *, resolution=None, shape=None, transform=None, resampling=Resampling.nearest, nodata=None, **kwargs)` (`resolution: float|tuple`, `resampling: Resampling|str`) | warp to a target CRS/grid             |
|  [02]   | `reproject_match`                 | `reproject_match(match_data_array, *, resampling=Resampling.nearest, **reproject_kwargs)` (`match_data_array: DataArray|Dataset`)                          | reproject onto another object's grid            |
|  [03]   | `clip`                            | `clip(geometries, crs=None, *, all_touched=False, drop=True, invert=False, from_disk=False)`                                                              | mask by GeoJSON-like geometries                 |
|  [04]   | `clip_box`                        | `clip_box(minx, miny, maxx, maxy, *, auto_expand=False, auto_expand_limit=3, crs=None, allow_one_dimensional_raster=False)`                               | crop to a bounding box                          |
|  [05]   | `pad_box`                         | `pad_box(minx, miny, maxx, maxy, *, constant_values=None)`                                                                                                | pad to a bounding box with constant fill        |
|  [06]   | `interpolate_na`                  | `interpolate_na(method='nearest')`  (`'linear'`/`'nearest'`/`'cubic'`)                                                                                    | fill nodata gaps by interpolation               |
|  [07]   | `to_raster`                       | `to_raster(raster_path, *, driver=None, dtype=None, tags=None, windowed=False, recalc_transform=True, lock=None, compute=True, **profile_kwargs) -> None` | write to a GDAL raster (GeoTIFF default)        |
|  [08]   | `RasterArray.nodata`              | property -> `Optional[float]`                                                                                                                             | decoded nodata value (`DataArray`)              |
|  [09]   | `RasterArray.write_nodata`        | `write_nodata(input_nodata, *, encoded=False, inplace=False) -> xarray.DataArray`                                                                         | persist nodata to CF attributes                 |
|  [10]   | `RasterArray.pad_xy`              | `pad_xy(minx, miny, maxx, maxy, *, constant_values=None) -> xarray.DataArray`                                                                             | pad x/y to a coordinate box                     |
|  [11]   | `RasterArray.to_rasterio_dataset` | `to_rasterio_dataset() -> Generator[DatasetReader, None, None]`                                                                                           | open the array as an in-memory rasterio dataset |
|  [12]   | `RasterDataset.vars`              | property -> `list`                                                                                                                                        | non-coordinate variable names                   |

## [04]-[IMPLEMENTATION_LAW]

[GEOSPATIAL_RASTER_IO]:
- import: `import rioxarray` at boundary scope only; the bare import registers the `.rio` accessor on `xarray.DataArray`/`Dataset` and the `engine="rasterio"` xarray backend, so importing for side effects is required before any `.rio` access — module-level import stays banned by the manifest import policy.
- read axis: `open_rasterio` is the single raster read surface; `chunks` selects dask vs eager, `masked`/`mask_and_scale` decode nodata, `band_as_variable`/`variable`/`group` shape the band-to-variable mapping, and `parse_coordinates=False` skips coordinate generation for large grids — never a per-format reader type.
- accessor axis: `.rio` is one polymorphic accessor; `RasterArray` and `RasterDataset` share `XRasterBase` and expose method-identical `reproject`/`clip`/`clip_box`/`pad_box`/`to_raster`, with the object type as the discriminant, never parallel single-object operations.
- metadata axis: CRS, transform, and nodata are read through `crs`/`transform`/`nodata` and written through `write_crs`/`write_transform`/`write_nodata`; `convention` selects the `Convention.CF` or `Convention.ZARR` encoding row, `inplace` toggles mutation, never a side-channel attribute mutation.
- warp axis: `reproject`/`reproject_match` own grid transformation; `resampling` is a `rasterio.enums.Resampling` row and `resolution`/`shape`/`transform` pick the target grid, never a hand-rolled GDAL warp.
- clip axis: `clip` (geometry) and `clip_box` (bounds) are the masking rows; `all_touched`/`drop`/`invert`/`from_disk` are call flags, and empty results surface as `NoDataInBounds`, never a silent empty array.
- merge axis: `merge_arrays`/`merge_datasets` mosaic tile sequences with `bounds`/`res`/`nodata`/`method` rows feeding the catalog coverage path, never a manual concat-and-align.
- evidence: each read/write captures CRS, affine transform, resolution, bounds, shape, dtype, nodata, and driver as a geospatial receipt.
- boundary: rioxarray owns the rasterio/GDAL-to-xarray raster boundary; vector geometry routes to `shapely`/`geopandas`, CRS algebra to `pyproj`, STAC discovery to `pystac`/`pystac-client`, and lower-level band IO to `rasterio` directly when no xarray labeling is needed; live UI stays outside this package.

[STACK_LAW]:
- `rasterio` -> `rioxarray`: `open_rasterio` wraps `rasterio.open`/`rasterio.vrt.WarpedVRT` (the `filename` argument accepts an open `DatasetReader`/`WarpedVRT` directly), labels bands as a georeferenced `DataArray`/`Dataset`, and shares the `rasterio.crs.CRS` object so CRS algebra is never duplicated; `resampling` rows are `rasterio.enums.Resampling`, and `to_rasterio_dataset` reopens the array as an in-memory rasterio dataset.
- `pystac`/`pystac-client` -> `rioxarray`: a signed COG asset href from a `pystac.Item` (`MediaType.COG`) flows into `open_rasterio(..., chunks=...)`; the item projection-extension `transform`/`epsg` match the `.rio.transform()`/`.rio.crs` the accessor resolves.
- `pystac` -> `odc-stac`/`stackstac` -> `rioxarray`: `odc.stac.load`/`stackstac.stack` assemble a multi-item cube and rioxarray's `.rio` accessor then owns per-band CRS/transform/nodata, `reproject_match` onto a reference grid, and `merge_arrays`/`merge_datasets` for the catalog-coverage mosaic — never a manual concat-and-align.
- `rioxarray` -> `zarr`/`icechunk`/`virtualizarr`: `write_crs(convention=Convention.ZARR)` plus `to_raster` (GDAL drivers) or the xarray `to_zarr` path persist the georeferenced cube; `Convention` selects CF vs Zarr grid-mapping encoding so the same object round-trips to both GeoTIFF and Zarr.
- `rioxarray` <-> `shapely`/`geopandas`/`pyproj`: `.rio.clip(geometries, crs=...)` consumes GeoJSON-like/`shapely` geometries (reprojected when `crs` differs), and `estimate_utm_crs`/`transform_bounds` defer the CRS algebra to the shared `pyproj`/PROJ owner.

[RAIL_LAW]:
- Package: `rioxarray`
- Owns: rasterio/GDAL raster read into georeferenced xarray objects, the `.rio` CRS/transform/nodata accessor, reprojection and grid matching, geometry and bounding-box clipping, spatial padding/slicing/windowing, nodata interpolation, multi-tile merge, and GDAL-driver writeback
- Accept: COVERAGE/CATALOG_COVERAGE_ODCSTAC raster read, warp, clip, and write feeding the data and coverage owners through the xarray boundary
- Reject: wrapper-renames of `open_rasterio`/`.rio`; a hand-rolled GDAL warp or window reader; a parallel accessor type per object kind where `XRasterBase` already unifies `DataArray`/`Dataset`; vector or CRS algebra duplication the `shapely`/`pyproj` owners hold; module-level import for accessor registration
