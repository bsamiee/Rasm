# [PY_DATA_API_RASTERIO]

`rasterio` owns the NumPy-array raster IO rail over GDAL: `open` is the single polymorphic dataset entry (mode, `driver`, and creation kwargs discriminate read versus write), and `DatasetReader`/`DatasetWriter`, `MemoryFile`, `Env`, and the `Session` cloud family carry band read/write, windowing, reprojection, masking, and merge under affine and CRS georeferencing. rasterio is the band IO beneath `rioxarray`, the COG decoder for `pystac` hrefs, and the CRS owner shared with `pyproj`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasterio`
- package: `rasterio` (BSD-3-Clause)
- module: `import rasterio`
- owner: `data`
- rail: geospatial
- entry points: console script `rio` (the `rio` CLI plugin family); library use is `import`-only
- capability: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed/tiled/boundless access, in-memory (`MemoryFile`) and VSI (`/vsicurl/`, `/vsizip/`, `/vsis3/`) datasets, GDAL/PROJ/GEOS reprojection and warping, raster/vector conversion, masking, multi-dataset merge, virtual warped datasets, scoped GDAL config, and cloud credential sessions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset handles, georeferencing carriers, and the enum vocabulary

`open` returns a `DatasetReader` or `DatasetWriter`; `MemoryFile`/`ZipMemoryFile` back both without disk. `CRS` with the `Affine` transform carries georeferencing, and `Window`/`BoundingBox` carry pixel and world extents.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                               |
| :-----: | :---------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `DatasetReader`         | class           | read-mode band and metadata handle         |
|  [02]   | `DatasetWriter`         | class           | write-mode dataset handle                  |
|  [03]   | `BufferedDatasetWriter` | class           | buffered write-mode dataset handle         |
|  [04]   | `MemoryFile`            | class           | in-memory dataset backing                  |
|  [05]   | `ZipMemoryFile`         | class           | in-memory zip-archive dataset backing      |
|  [06]   | `Env`                   | context-manager | scoped GDAL drivers, config, credentials   |
|  [07]   | `Session`               | class-family    | cloud credential providers                 |
|  [08]   | `Window`                | class           | pixel-space subregion geometry             |
|  [09]   | `BoundingBox`           | namedtuple      | `(left, bottom, right, top)` extent        |
|  [10]   | `CRS`                   | class           | GDAL/PROJ coordinate reference system      |
|  [11]   | `Affine`                | value           | georeferencing transform                   |
|  [12]   | `AffineTransformer`     | class-family    | pixel/world affine, RPC, GCP transformers  |
|  [13]   | `GroundControlPoint`    | value           | GCP georeferencing carrier                 |
|  [14]   | `RPC`                   | value           | rational-polynomial georeferencing carrier |
|  [15]   | `WarpedVRT`             | class           | virtual warped dataset over a source       |
|  [16]   | `Profile`               | value-family    | creation-option profile dicts              |

- `Session` family: `AWSSession` `GSSession` `AzureSession` `OSSSession` `SwiftSession` `DummySession` — passed to `Env`/`open` for signed `/vsi*/` access.
- `AffineTransformer`/`RPCTransformer`/`GCPTransformer` extend `TransformerBase`; `TransformMethodsMixin` gives `DatasetReader` its `index`/`xy`.
- `Profile`/`DefaultGTiffProfile`/`default_gtiff_profile` carry driver, dtype, tiling, and compression creation defaults; `Affine` re-exports the `affine` package.

[ENUMS]:
- `Resampling`: `nearest` `bilinear` `cubic` `cubic_spline` `lanczos` `average` `mode` `gauss` `max` `min` `med` `q1` `q3` `sum` `rms`
- `OverviewResampling`: `nearest` `bilinear` `cubic` `cubic_spline` `lanczos` `average` `mode` `gauss` `rms` (the `build_overviews` subset)
- `Compression`: `jpeg` `lzw` `packbits` `deflate` `ccittrle` `ccittfax3` `ccittfax4` `lzma` `none` `zstd` `lerc` `lerc_deflate` `lerc_zstd` `webp` `jpeg2000`
- `ColorInterp`: `undefined` `gray` `palette` `red` `green` `blue` `alpha` `hue` `saturation` `lightness` `cyan` `magenta` `yellow` `black` `Y` `Cb` `Cr` `pan` `coastal` `rededge` `nir` `swir` `mwir` `lwir` `tir` `other_ir` `sar_ka` `sar_k` `sar_ku` `sar_x` `sar_c` `sar_s` `sar_l` `sar_p`
- `MaskFlags`: `all_valid` `per_dataset` `alpha` `nodata`
- `MergeAlg`: `replace` `add`
- `Interleaving`: `pixel` `line` `band` `tile`
- `PhotometricInterp`: `black` `white` `rgb` `cmyk` `ycbcr` `cielab` `icclab` `itulab`
- `WktVersion`: `WKT2_2015` `WKT2` `WKT2_2019` `WKT1_GDAL` `WKT1` `WKT1_ESRI`
- `TransformMethod`: `affine` `gcps` `rpcs`; `TransformDirection`: `forward` `reverse`

[EXCEPTIONS]: rooted at `RasterioError` under `rasterio.errors`
- `RasterioIOError` `PathError` `OpenerRegistrationError` — open, path/VSI, and custom `opener=` faults
- `DriverCapabilityError` `DriverRegistrationError` `OverviewCreationError` — driver create, write, and overview faults
- `CRSError` `TransformError` `RPCError` `WindowError` `WindowEvaluationError` — CRS, transform/RPC, and window-geometry faults
- `MergeError` `StackError` `WarpOperationError` `WarpedVRTError` `WarpOptionsError` `StatisticsError` `BandOverviewError` `DatasetIOShapeError` `InvalidArrayError` — merge, warp, and IO-shape faults
- `GDALVersionError` `GDALOptionNotImplementedError` `GDALBehaviorChangeException` — GDAL build mismatches
- `RasterioDeprecationWarning` `NotGeoreferencedWarning` `NodataShadowWarning` `ShapeSkipWarning` `TransformWarning` — warnings

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dataset lifecycle, band IO, and georeferencing
- call: `open(fp, mode='r', driver=None, width=None, height=None, count=None, crs=None, transform=None, dtype=None, nodata=None, sharing=False, thread_safe=False, opener=None, **kwargs) -> DatasetReader | DatasetWriter`
- call: `band(ds, bidx) -> Band`; `pad(array, transform, pad_width, mode, **kwargs) -> (padded, transform)`
- call: `read(indexes=None, out=None, window=None, masked=False, out_shape=None, boundless=False, resampling=Resampling.nearest, fill_value=None, out_dtype=None)`
- call: `read_masks(indexes=None, out=None, out_shape=None, window=None, boundless=False, resampling=Resampling.nearest)`, `sample(xy, indexes=None, masked=False)`, `write(arr, indexes=None, window=None)`, `write_band`, `build_overviews`, `update_tags`, `write_colormap`
- call: `tags(bidx=0, ns=None)`, `overviews(bidx)`, `statistics(bidx, approx=False, clear_cache=False)`, `block_windows(bidx=0)`, `colormap(bidx)`
- call: `index(x, y, z=None, op=None, precision=None, transform_method=TransformMethod.affine, **rpc_options)`, `xy(row, col, z=None, offset='center', transform_method=TransformMethod.affine, **rpc_options)`, `window(left, bottom, right, top)`, `window_bounds(window)`, `window_transform(window)`
- properties: `meta` `profile` `name` `bounds` `crs` `transform` `res` `shape` `height` `width` `count` `dtypes` `nodata` `nodatavals` `colorinterp` `descriptions` `units` `scales` `offsets` `block_shapes` `gcps` `rpcs`

[ENTRYPOINT_SCOPE]: reproject, warp, and the raster/vector bridge
- call: `warp.reproject(source, destination=None, src_transform=None, gcps=None, rpcs=None, src_crs=None, src_nodata=None, dst_transform=None, dst_crs=None, dst_nodata=None, dst_resolution=None, src_alpha=0, dst_alpha=0, masked=False, resampling=Resampling.nearest, num_threads=1, init_dest_nodata=True, warp_mem_limit=0, src_geoloc_array=None, **kwargs)`
- call: `warp.calculate_default_transform(src_crs, dst_crs, width, height, left=None, bottom=None, right=None, top=None, gcps=None, rpcs=None, resolution=None, dst_width=None, dst_height=None, src_geoloc_array=None, **kwargs)`, `warp.transform(src_crs, dst_crs, xs, ys, zs=None)`, `warp.transform_bounds(src_crs, dst_crs, left, bottom, right, top, densify_pts=21)`, `warp.transform_geom(src_crs, dst_crs, geom, antimeridian_cutting=None, antimeridian_offset=None, precision=-1)`, `warp.aligned_target(transform, width, height, resolution)`
- call: `features.shapes(source, mask=None, connectivity=4, transform=IDENTITY)`, `features.rasterize(shapes, out_shape=None, fill=0, nodata=None, masked=False, out=None, transform=IDENTITY, all_touched=False, merge_alg=MergeAlg.replace, default_value=1, dtype=None, skip_invalid=True, dst_path=None, dst_kwds=None)`, `features.geometry_mask(geometries, out_shape, transform, all_touched=False, invert=False)`, `features.geometry_window(dataset, shapes, pad_x=0, pad_y=0, north_up=None, rotated=None, pixel_precision=None, boundless=False)`, `features.sieve(source, size, out=None, mask=None, connectivity=4)`, `features.bounds(geometry, north_up=True, transform=None)`, `features.dataset_features(src, bidx=None, sampling=1, band=True, as_mask=False, with_nodata=False, geographic=True, precision=-1)`, `features.is_valid_geom(geom)`
- call: `mask.mask(dataset, shapes, all_touched=False, invert=False, nodata=None, filled=True, crop=False, pad=False, pad_width=0.5, indexes=None) -> (out_image, out_transform)`, `mask.raster_geometry_mask(dataset, shapes, all_touched=False, invert=False, crop=False, pad=False, pad_width=0.5)`
- call: `merge.merge(sources, bounds=None, res=None, nodata=None, dtype=None, precision=None, indexes=None, output_count=None, resampling=Resampling.nearest, method='first', target_aligned_pixels=False, mem_limit=64, use_highest_res=False, masked=False, dst_path=None, dst_kwds=None) -> (mosaic, out_transform)` — `method` accepts `'first'`/`'last'`/`'min'`/`'max'`/`'sum'`/`'count'` or a callable

[ENTRYPOINT_SCOPE]: windows, fill, drivers, and transform construction
- call: `windows.from_bounds(left, bottom, right, top, transform=None, height=None, width=None, precision=None)`, `windows.bounds(window, transform, height=0, width=0)`, `windows.transform(window, transform)`, `windows.shape(window, height=-1, width=-1)`, `windows.get_data_window(arr, nodata=None)`, `windows.union(*windows)`, `windows.intersection(*windows)`, `windows.window_index(window, height=0, width=0)`, `windows.round_window_to_full_blocks(window, block_shapes, height=0, width=0)`
- call: `fill.fillnodata(image, mask=None, max_search_distance=100.0, smoothing_iterations=0)` — inverse-distance nodata interpolation
- call: `drivers.driver_from_extension(path)`, `drivers.raster_driver_extensions()`, `drivers.is_blacklisted(name)`
- call: `CRS.from_epsg`/`from_wkt`/`from_user_input`/`to_epsg`/`to_wkt(version=WktVersion.*)`/`to_authority`; `Affine.scale(sx, sy=None)`/`translation(dx, dy)`/`identity()` composed as `source.transform * Affine.scale(factor)` for a decimation/zoom transform
- `Window` methods: `from_slices` `round_lengths` `round_shape` `round_offsets` `round` `crop` `intersection` `flatten` `todict` `toranges` `toslices`; `coords.disjoint_bounds(bounds1, bounds2)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `open` is the single polymorphic dataset entry; mode (`'r'`/`'w'`/`'r+'`), `driver`, and creation kwargs discriminate read versus write, and it enters as a context manager to guarantee GDAL close.
- Band indexes are 1-based; `read()` returns a `(count, rows, cols)` array for all bands, a 2D array for a single integer index, and a `numpy.ma.MaskedArray` under `masked=True` honoring `MaskFlags`.
- Georeferencing is the affine `transform` with `crs`; `xy`/`index`/`window` map pixel to world, and `transform_method` selects the affine, GCP, or RPC transformer.
- Windowed, `boundless`, and `block_windows` reads stream tile-aligned without materializing the full raster; `merge(method=callable)` reduces a custom mosaic over tiles.
- `warp.reproject`/`calculate_default_transform`/`WarpedVRT` own reprojection with a `Resampling` kernel and GDAL `num_threads`/`warp_mem_limit`; `features.shapes`/`features.rasterize` are the bidirectional raster/vector bridge honoring `transform` and `MergeAlg`.
- `Env` scopes process-global GDAL state and injects a `Session` for cloud access; `MemoryFile`/`ZipMemoryFile` and the `/vsicurl/`, `/vsizip/`, `/vsis3/` prefixes (or an `opener=` callable) cover in-memory, archive, and remote datasets.

[STACKING]:
- `pystac`(`.api/pystac.md`): a `MediaType.COG` asset href, signed through `planetary_computer.sign`, opens directly in `rasterio.open`/`WarpedVRT`; the projection-extension transform and CRS match the dataset `transform`/`crs`.
- `rioxarray`(`.api/rioxarray.md`): `open_rasterio` wraps `rasterio.open`/`WarpedVRT` into a georeferenced xarray `DataArray`/`Dataset`; raw rasterio serves only where xarray labeling and dask chunking are unneeded.
- `shapely`(`.api/shapely.md`)/`geopandas`(`.api/geopandas.md`): `features.shapes`/`features.rasterize`/`mask.mask` exchange GeoJSON-like mappings with shapely geometries and GeoDataFrames, and `warp.transform_geom` reprojects geometry through the shared PROJ.
- `stac-geoparquet`(`.api/stac-geoparquet.md`)/`obstore`(`python/.api/obstore.md`): a `MemoryFile` write with an `obstore` PUT (or a `/vsis3/` path under `AWSSession`) is the COG write-and-upload rail; `default_gtiff_profile` with `Compression.zstd`/`webp` and tiling kwargs sets the creation profile.
- `pyproj`(`.api/pyproj.md`): the `CRS` object is shared unchanged, never re-derived; one `Env(session=...)` scopes the whole read/warp/merge pipeline, the cloud provider a `Session` row rather than a per-call credential.
- within-lib: the geospatial owner threads `open` (context-managed) into windowed/`block_windows` streaming, then `warp.reproject`/`WarpedVRT`, the `features`/`mask` vector bridge, and `merge` mosaicking under one `Env(session=...)`, every stage a `Resampling`/`MergeAlg` row.

[LOCAL_ADMISSION]:
- Admit `rasterio.open` as a context manager for band IO, windowed and `boundless` streaming, `warp`/`WarpedVRT` reprojection, `features`/`mask` vector bridging, `merge` mosaics, and `Env(session=...)` scoping; the shared `CRS` crosses to rioxarray and pyproj unchanged.

[RAIL_LAW]:
- Package: `rasterio`
- Owns: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed/tiled/boundless access, reprojection and warping, raster/vector conversion, masking, multi-dataset merge, virtual warped datasets, in-memory and VSI datasets, scoped GDAL config, and cloud sessions
- Accept: `rasterio.open` context-managed, windowed/`boundless`/`block_windows` streaming, `warp.reproject`/`WarpedVRT` with a `Resampling` enum, `features.shapes`/`features.rasterize` for the vector bridge, `mask.mask` for geometry masking, `merge` with a custom-callable `method`, `Env(session=...)` for scoped GDAL/cloud config, the shared `CRS` across rioxarray/pyproj
- Reject: manual pixel-to-world math where `xy`/`index`/`transform_method` apply, full-raster reads where a window suffices, hand-rolled resampling or mosaic kernels where `Resampling`/`merge` dispatch, per-driver parallel open functions when `open` dispatches on mode and driver, duplicated CRS algebra the shared PROJ owns, raw GDAL-config mutation where `Env`/`Session` scope it, raw band reads where `rioxarray` owns xarray labeling
