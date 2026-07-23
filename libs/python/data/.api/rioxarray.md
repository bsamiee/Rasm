# [PY_DATA_API_RIOXARRAY]

`rioxarray` extends `xarray` with the rasterio-backed `.rio` accessor for georeferenced raster IO: `open_rasterio` reads any GDAL raster into a `DataArray`/`Dataset`, and `RasterArray`/`RasterDataset` over the shared `XRasterBase` own CRS/transform/nodata metadata, warping, clipping, windowing, and GDAL-driver writeback as one object-discriminated accessor. Raster decode, CRS algebra, and vector geometry defer to `rasterio`/GDAL, `pyproj`, and `shapely`/`geopandas`; rioxarray composes the coverage and STAC-catalog raster rail, never re-implementing the GDAL stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rioxarray`
- package: `rioxarray` (Apache-2.0)
- module: `import rioxarray`
- owner: `data`
- rail: geospatial
- asset: pure Python over rasterio/GDAL; the bare import registers the `.rio` accessor on `xarray.DataArray`/`Dataset` and the `engine="rasterio"` backend for `xarray.open_dataset`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: accessor, convention, and error roots

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                  |
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

| [INDEX] | [SURFACE]              | [CAPABILITY]                                                                            |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `open_rasterio`        | open a GDAL raster into a georeferenced xarray object                                   |
|  [02]   | `merge.merge_arrays`   | mosaic a sequence of `DataArray` tiles                                                  |
|  [03]   | `merge.merge_datasets` | mosaic a sequence of `Dataset` tiles                                                    |
|  [04]   | `set_options`          | set `export_grid_mapping`/`skip_missing_spatial_dims`/`convention` global or contextual |
|  [05]   | `show_versions`        | print rioxarray and dependency versions                                                 |

- call: `open_rasterio(filename, *, parse_coordinates=None, chunks=None, cache=None, lock=None, masked=False, mask_and_scale=False, variable=None, group=None, default_name=None, decode_times=True, decode_timedelta=None, band_as_variable=False, **open_kwargs) -> DataArray | Dataset | list[Dataset]`
- call: `merge_arrays(dataarrays, *, bounds=None, res=None, nodata=None, precision=None, method=None, crs=None, parse_coordinates=True) -> DataArray`; `merge_datasets(datasets, *, bounds=None, res=None, nodata=None, precision=None, method=None, crs=None) -> Dataset`
- call: `set_options(**kwargs)`; `show_versions() -> None`

[ENTRYPOINT_SCOPE]: `XRasterBase` shared accessor surface

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `crs`                                                                                        | CRS as `Optional[rasterio.crs.CRS]`   |
|  [02]   | `set_crs(input_crs, inplace=True)`                                                           | attach CRS without reprojecting       |
|  [03]   | `write_crs(input_crs=None, grid_mapping_name=None, convention=None, inplace=False)`          | persist CRS to the grid-mapping coord |
|  [04]   | `grid_mapping` / `write_grid_mapping(grid_mapping_name='spatial_ref', inplace=False)`        | grid-mapping variable name read/write |
|  [05]   | `write_transform(transform=None, grid_mapping_name=None, convention=None, inplace=False)`    | persist the affine transform          |
|  [06]   | `write_coordinate_system(inplace=False)`                                                     | write CF coordinate-system attrs      |
|  [07]   | `transform(recalc=False)`                                                                    | affine `Affine` from coords or cache  |
|  [08]   | `estimate_utm_crs(datum_name='WGS 84')`                                                      | derive a metric UTM CRS from extent   |
|  [09]   | `set_spatial_dims(x_dim, y_dim, inplace=True)`                                               | bind the x/y dimension names          |
|  [10]   | `resolution(recalc=False)`                                                                   | x/y pixel resolution                  |
|  [11]   | `bounds(*, recalc=False)`                                                                    | native-CRS bounding box               |
|  [12]   | `transform_bounds(dst_crs, *, densify_pts=21, recalc=False)`                                 | bbox reprojected to another CRS       |
|  [13]   | `slice_xy(minx, miny, maxx, maxy)`                                                           | slice to a coordinate box             |
|  [14]   | `isel_window(window, *, pad=False)`                                                          | select a `rasterio` window            |
|  [15]   | `write_gcps(gcps, gcp_crs, *, grid_mapping_name=None, inplace=False)` / `get_gcps()`         | ground-control-point read/write       |
|  [16]   | `write_rpcs(rpcs, *, grid_mapping_name=None, inplace=False)` / `get_rpcs()`                  | rational-polynomial-coeff read/write  |
|  [17]   | `set_attrs(new_attrs, inplace=False)` / `update_attrs(new_attrs, inplace=False)`             | replace or merge attrs                |
|  [18]   | `set_encoding(new_encoding, inplace=False)` / `update_encoding(new_encoding, inplace=False)` | replace or merge encoding             |
|  [19]   | `shape` / `width` / `height` / `count`                                                       | grid dimensions                       |

[ENTRYPOINT_SCOPE]: `RasterArray` / `RasterDataset` raster operations

`RasterArray` and `RasterDataset` share these method signatures, the return tracking the object; `nodata`/`write_nodata`/`pad_xy`/`to_rasterio_dataset` are `RasterArray`-only, `vars` is `RasterDataset`-only.

| [INDEX] | [SURFACE]                         | [CAPABILITY]                                    |
| :-----: | :-------------------------------- | :---------------------------------------------- |
|  [01]   | `reproject`                       | warp to a target CRS/grid                       |
|  [02]   | `reproject_match`                 | reproject onto another object's grid            |
|  [03]   | `clip`                            | mask by GeoJSON-like geometries                 |
|  [04]   | `clip_box`                        | crop to a bounding box                          |
|  [05]   | `pad_box`                         | pad to a bounding box with constant fill        |
|  [06]   | `interpolate_na`                  | fill nodata gaps by interpolation               |
|  [07]   | `to_raster`                       | write to a GDAL raster (GeoTIFF default)        |
|  [08]   | `RasterArray.nodata`              | decoded nodata value (`DataArray`)              |
|  [09]   | `RasterArray.write_nodata`        | persist nodata to CF attributes                 |
|  [10]   | `RasterArray.pad_xy`              | pad x/y to a coordinate box                     |
|  [11]   | `RasterArray.to_rasterio_dataset` | open the array as an in-memory rasterio dataset |
|  [12]   | `RasterDataset.vars`              | non-coordinate variable names                   |

- call: `reproject(dst_crs, *, resolution=None, shape=None, transform=None, resampling=Resampling.nearest, nodata=None, **kwargs)`; `reproject_match(match_data_array, *, resampling=Resampling.nearest, **reproject_kwargs)`
- call: `clip(geometries, crs=None, *, all_touched=False, drop=True, invert=False, from_disk=False)`; `clip_box(minx, miny, maxx, maxy, *, auto_expand=False, auto_expand_limit=3, crs=None, allow_one_dimensional_raster=False)`
- call: `pad_box(minx, miny, maxx, maxy, *, constant_values=None)`; `interpolate_na(method='nearest')`
- call: `to_raster(raster_path, *, driver=None, dtype=None, tags=None, windowed=False, recalc_transform=True, lock=None, compute=True, **profile_kwargs) -> None`
- call: `RasterArray.write_nodata(input_nodata, *, encoded=False, inplace=False)`; `RasterArray.pad_xy(minx, miny, maxx, maxy, *, constant_values=None)`; `RasterArray.to_rasterio_dataset() -> Generator[DatasetReader]`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- read axis: `open_rasterio` is the single raster read surface; `chunks` selects dask vs eager, `masked`/`mask_and_scale` decode nodata, `band_as_variable`/`variable`/`group` shape the band-to-variable mapping, and `parse_coordinates=False` skips coordinate generation for large grids — never a per-format reader type.
- accessor axis: `.rio` is one polymorphic accessor; `RasterArray` and `RasterDataset` share `XRasterBase` and expose method-identical `reproject`/`clip`/`clip_box`/`pad_box`/`to_raster` discriminated by object type, never parallel per-object operations.
- metadata axis: CRS, transform, and nodata read through `crs`/`transform`/`nodata` and write through `write_crs`/`write_transform`/`write_nodata`; `convention` selects the `Convention.CF` or `Convention.ZARR` encoding and `inplace` toggles mutation, never a side-channel attribute write.
- warp axis: `reproject`/`reproject_match` own grid transformation; `resampling` is a `rasterio.enums.Resampling` row and `resolution`/`shape`/`transform` pick the target grid, never a hand-rolled GDAL warp.
- clip axis: `clip` (geometry) and `clip_box` (bounds) are the masking rows; `all_touched`/`drop`/`invert`/`from_disk` are call flags, and an empty result surfaces `NoDataInBounds`, never a silent empty array.
- merge axis: `merge_arrays`/`merge_datasets` mosaic tile sequences with `bounds`/`res`/`nodata`/`method` rows, never a manual concat-and-align.
- evidence: each read/write captures CRS, affine transform, resolution, bounds, shape, dtype, nodata, and driver as a geospatial receipt.
- boundary: rioxarray owns the rasterio/GDAL-to-xarray raster seam; vector geometry routes to `shapely`/`geopandas`, CRS algebra to `pyproj`, STAC discovery to `pystac`/`pystac-client`, and band-level IO to `rasterio` when no xarray labeling is needed.

[STACKING]:
- `rasterio`(`.api/rasterio.md`): `open_rasterio` wraps `rasterio.open`/`rasterio.vrt.WarpedVRT` (`filename` accepts an open `DatasetReader`/`WarpedVRT` directly) and shares the `rasterio.crs.CRS` object so CRS algebra is never duplicated; `resampling` rows are `rasterio.enums.Resampling`, and `to_rasterio_dataset` reopens the array as an in-memory rasterio dataset.
- `pystac`(`.api/pystac.md`)/`pystac-client`(`.api/pystac-client.md`): a signed COG asset href from a `pystac.Item` (`MediaType.COG`) flows into `open_rasterio(chunks=...)`; the item projection-extension `transform`/`epsg` match the `.rio.transform()`/`.rio.crs` the accessor resolves.
- `odc-stac`(`.api/odc-stac.md`): `odc.stac.load` assembles a multi-item cube and `.rio` then owns per-band CRS/transform/nodata, `reproject_match` onto a reference grid, and `merge_arrays`/`merge_datasets` for the catalog-coverage mosaic.
- `zarr`(`.api/zarr.md`)/`icechunk`(`.api/icechunk.md`)/`virtualizarr`(`.api/virtualizarr.md`): `write_crs(convention=Convention.ZARR)` and `to_raster` (or the xarray `to_zarr` path) persist the georeferenced cube; `Convention` selects CF vs Zarr grid-mapping so one object round-trips to both GeoTIFF and Zarr.
- `shapely`(`.api/shapely.md`)/`geopandas`(`.api/geopandas.md`)/`pyproj`(`.api/pyproj.md`): `.rio.clip(geometries, crs=...)` consumes GeoJSON-like/`shapely` geometries (reprojected when `crs` differs), and `estimate_utm_crs`/`transform_bounds` defer the CRS algebra to the shared `pyproj` owner.
- within-lib: the data geospatial owner drives `open_rasterio` and the `.rio` accessor as the coverage read path with chunking/masking/CRS as call rows, handing `reproject_match` and merge output to the STAC catalog and object-store egress rails.

[LOCAL_ADMISSION]:
- Admit `rioxarray` as the rasterio/GDAL-to-xarray raster owner on the data geospatial rail, composing `rasterio`/`pyproj`/`shapely` rather than re-decoding rasters or duplicating CRS algebra.

[RAIL_LAW]:
- Package: `rioxarray`
- Owns: rasterio/GDAL raster read into georeferenced xarray objects, the `.rio` CRS/transform/nodata accessor, reprojection and grid matching, geometry and bounding-box clipping, spatial padding/slicing/windowing, nodata interpolation, multi-tile merge, and GDAL-driver writeback
- Accept: coverage and STAC-catalog raster read, warp, clip, and write feeding the data and coverage owners through the xarray boundary
- Reject: wrapper-renames of `open_rasterio`/`.rio`; a hand-rolled GDAL warp or window reader; a parallel accessor type per object kind where `XRasterBase` already unifies `DataArray`/`Dataset`; vector or CRS algebra duplication the `shapely`/`pyproj` owners hold; module-level import for accessor registration
