# [PY_DATA_API_RASTERIO]

`rasterio` supplies a Pythonic NumPy-array interface over GDAL for raster geospatial data, reading and writing bands as `ndarray` with affine georeferencing. `open` is the single polymorphic dataset entry (mode/driver/creation-kwargs discriminate read versus write); `DatasetReader`/`DatasetWriter`, `MemoryFile`, `Env`, and the `Session` cloud family are the owners. Windowed and boundless reads, masking, reprojection/warping, raster<->vector conversion, multi-dataset merge, and `WarpedVRT` virtual warping ride the `warp`/`features`/`mask`/`merge`/`windows`/`transform` submodules. rasterio is the low-level band IO beneath `rioxarray` (xarray labeling), the asset decoder for `pystac` COG hrefs, and the CRS owner shared with `pyproj`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasterio`
- package: `rasterio`
- import: `import rasterio`
- owner: `data`
- rail: geospatial
- version: `1.5.0`
- license: BSD-3-Clause
- entry points: console scripts `rio` (the `rio` CLI plugin family) — library use is `import`-only
- capability: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed/tiled/boundless access, in-memory (`MemoryFile`) and VSI (`/vsicurl/`, `/vsizip/`, `/vsis3/`) datasets, GDAL/PROJ/GEOS reprojection and warping, raster/vector conversion, masking, multi-dataset merge, virtual warped datasets, and scoped GDAL config + cloud credential sessions

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `rasterio.DatasetReader` — read-mode dataset (`io.py` re-export of `_io.DatasetReader`); band read, windowing, masking, sampling, statistics, overviews, tags, block windows, colormap.
- `rasterio.io.DatasetWriter` / `BufferedDatasetWriter` — write-mode datasets returned by `open(mode='w'|'r+')`; `write`, `write_band`, `build_overviews`, `update_tags`, `write_colormap`.
- `rasterio.io.MemoryFile` / `ZipMemoryFile` — in-memory dataset backing for read and write without disk I/O; `.open(...)` yields a reader/writer over the buffer.
- `rasterio.Env` — GDAL environment context manager; scopes drivers, credentials, and config options; `Env(session=...)` injects a cloud `Session`.
- `rasterio.session.Session` family — `AWSSession`, `GSSession`, `AzureSession`, `OSSSession`, `SwiftSession`, `DummySession`; cloud credential providers passed to `Env`/`open` for signed `/vsi*/` access.
- `rasterio.windows.Window` — pixel-space subregion; methods `from_slices`, `round_lengths`, `round_shape`, `round_offsets`, `round`, `crop`, `intersection`, `flatten`, `todict`, `toranges`, `toslices`.
- `rasterio.coords.BoundingBox` — `namedtuple(left, bottom, right, top)`; module `disjoint_bounds(bounds1, bounds2)`.
- `rasterio.crs.CRS` — coordinate reference system (Cython, GDAL/PROJ-backed); `from_epsg`/`from_wkt`/`from_user_input`/`to_epsg`/`to_wkt(version=WktVersion.*)`/`to_authority`. `rasterio.Affine` (re-export of the `affine` package) is the georeferencing transform; `Affine.scale(sx, sy=None)`/`Affine.translation(dx, dy)`/`Affine.identity()` are its constructor classmethods, composed as `source.transform * Affine.scale(factor)` to derive a decimation/zoom/window transform.
- `rasterio.transform.AffineTransformer` / `RPCTransformer` / `GCPTransformer` (over `TransformerBase`) — pixel<->world transformers; `TransformMethodsMixin` gives `DatasetReader` its `index`/`xy`.
- `rasterio.transform.GroundControlPoint` (re-exported from `control.py`) and `rasterio.rpc.RPC` — GCP-list and rational-polynomial-coefficient georeferencing carriers.
- `rasterio.vrt.WarpedVRT` — virtual warped dataset wrapping a source under a target CRS/transform/resampling; reads warp lazily and is itself a dataset (feeds `open_rasterio`).
- `rasterio.profiles.Profile` / `DefaultGTiffProfile` / `default_gtiff_profile` — creation-option profile dicts (driver/dtype/tiling/compression defaults).
- `rasterio.enums.*` — `Resampling`, `OverviewResampling`, `ColorInterp`, `Compression`, `MaskFlags`, `MergeAlg`, `PhotometricInterp`, `Interleaving`, `WktVersion`, `TransformDirection`, `TransformMethod`.

[ENUMS]:
- `Resampling` — `nearest`, `bilinear`, `cubic`, `cubic_spline`, `lanczos`, `average`, `mode`, `gauss`, `max`, `min`, `med`, `q1`, `q3`, `sum`, `rms` (read/warp resampling kernel).
- `OverviewResampling` — `nearest`/`bilinear`/`cubic`/`cubic_spline`/`lanczos`/`average`/`mode`/`gauss`/`rms` (the subset valid for `build_overviews`).
- `Compression` — `jpeg`, `lzw`, `packbits`, `deflate`, `ccittrle`, `ccittfax3`, `ccittfax4`, `lzma`, `none`, `zstd`, `lerc`, `lerc_deflate`, `lerc_zstd`, `webp`, `jpeg2000`.
- `ColorInterp` — `undefined`/`gray`/`palette`/`red`/`green`/`blue`/`alpha`/`hue`/`saturation`/`lightness`/`cyan`/`magenta`/`yellow`/`black`/`Y`/`Cb`/`Cr`/`pan`/`coastal`/`rededge`/`nir`/`swir`/`tir` (+ SAR band interpretations).
- `MaskFlags` — `all_valid`/`per_dataset`/`alpha`/`nodata`; `MergeAlg` — `replace`/`add`; `Interleaving` — `pixel`/`line`/`band`/`tile`.
- `PhotometricInterp` — `black`/`white`/`rgb`/`cmyk`/`ycbcr`/`cielab`/`icclab`/`itulab`.
- `WktVersion` — `WKT2_2015`/`WKT2`/`WKT2_2019`/`WKT1_GDAL`/`WKT1`/`WKT1_ESRI`; `TransformMethod` — `affine`/`gcps`/`rpcs`; `TransformDirection` — `forward`/`reverse`.

[ENTRYPOINTS]:
- open: `rasterio.open(fp, mode='r', driver=None, width=None, height=None, count=None, crs=None, transform=None, dtype=None, nodata=None, sharing=False, thread_safe=False, opener=None, **kwargs) -> DatasetReader | DatasetWriter`; `opener=` accepts a custom file-opener callable (fsspec/obstore VSI bridge).
- top-level: `rasterio.band(ds, bidx)` -> a `Band` reference for `warp.reproject`/`features.shapes`; `rasterio.pad(array, transform, pad_width, mode, **kwargs) -> (padded, transform)`.
- band read/write: `DatasetReader.read(indexes=None, out=None, window=None, masked=False, out_shape=None, boundless=False, resampling=Resampling.nearest, fill_value=None, out_dtype=None)`, `DatasetReader.read_masks(indexes=None, out=None, out_shape=None, window=None, boundless=False, resampling=Resampling.nearest)`, `DatasetReader.sample(xy, indexes=None, masked=False)`, `DatasetWriter.write(arr, indexes=None, window=None)`.
- dataset metadata: `DatasetReader.meta`, `.profile`, `.name`, `.bounds`, `.crs`, `.transform`, `.res`, `.shape`, `.height`, `.width`, `.count`, `.dtypes`, `.nodata`, `.nodatavals`, `.colorinterp`, `.descriptions`, `.units`, `.scales`, `.offsets`, `.tags(bidx=0, ns=None)`, `.overviews(bidx)`, `.statistics(bidx, approx=False, clear_cache=False)`, `.block_windows(bidx=0)`, `.block_shapes`, `.colormap(bidx)`, `.gcps`, `.rpcs`.
- coordinate mapping: `DatasetReader.index(x, y, z=None, op=None, precision=None, transform_method=TransformMethod.affine, **rpc_options)`, `DatasetReader.xy(row, col, z=None, offset='center', transform_method=TransformMethod.affine, **rpc_options)`, `.window(left, bottom, right, top)`, `.window_bounds(window)`, `.window_transform(window)`.
- reproject/warp (`rasterio.warp`): `reproject(source, destination=None, src_transform=None, gcps=None, rpcs=None, src_crs=None, src_nodata=None, dst_transform=None, dst_crs=None, dst_nodata=None, dst_resolution=None, src_alpha=0, dst_alpha=0, masked=False, resampling=Resampling.nearest, num_threads=1, init_dest_nodata=True, warp_mem_limit=0, src_geoloc_array=None, **kwargs)`, `calculate_default_transform(src_crs, dst_crs, width, height, left=None, bottom=None, right=None, top=None, gcps=None, rpcs=None, resolution=None, dst_width=None, dst_height=None, src_geoloc_array=None, **kwargs)`, `transform(src_crs, dst_crs, xs, ys, zs=None)`, `transform_bounds(src_crs, dst_crs, left, bottom, right, top, densify_pts=21)`, `transform_geom(src_crs, dst_crs, geom, antimeridian_cutting=None, antimeridian_offset=None, precision=-1)`, `aligned_target(transform, width, height, resolution)`.
- raster <-> vector (`rasterio.features`): `shapes(source, mask=None, connectivity=4, transform=IDENTITY)`, `rasterize(shapes, out_shape=None, fill=0, nodata=None, masked=False, out=None, transform=IDENTITY, all_touched=False, merge_alg=MergeAlg.replace, default_value=1, dtype=None, skip_invalid=True, dst_path=None, dst_kwds=None)`, `geometry_mask(geometries, out_shape, transform, all_touched=False, invert=False)`, `geometry_window(dataset, shapes, pad_x=0, pad_y=0, north_up=None, rotated=None, pixel_precision=None, boundless=False)`, `sieve(source, size, out=None, mask=None, connectivity=4)`, `bounds(geometry, north_up=True, transform=None)`, `dataset_features(src, bidx=None, sampling=1, band=True, as_mask=False, with_nodata=False, geographic=True, precision=-1)`, `is_valid_geom(geom)`.
- mask (`rasterio.mask`): `mask(dataset, shapes, all_touched=False, invert=False, nodata=None, filled=True, crop=False, pad=False, pad_width=0.5, indexes=None) -> (out_image, out_transform)`, `raster_geometry_mask(dataset, shapes, all_touched=False, invert=False, crop=False, pad=False, pad_width=0.5)`.
- merge (`rasterio.merge`): `merge(sources, bounds=None, res=None, nodata=None, dtype=None, precision=None, indexes=None, output_count=None, resampling=Resampling.nearest, method='first', target_aligned_pixels=False, mem_limit=64, use_highest_res=False, masked=False, dst_path=None, dst_kwds=None) -> (mosaic, out_transform)`; `method` accepts `'first'`/`'last'`/`'min'`/`'max'`/`'sum'`/`'count'` or a custom callable.
- windows (`rasterio.windows`): `from_bounds(left, bottom, right, top, transform=None, height=None, width=None, precision=None)`, `bounds(window, transform, height=0, width=0)`, `transform(window, transform)`, `shape(window, height=-1, width=-1)`, `get_data_window(arr, nodata=None)`, `union(*windows)`, `intersection(*windows)`, `window_index(window, height=0, width=0)`, `round_window_to_full_blocks(window, block_shapes, height=0, width=0)`.
- fill (`rasterio.fill`): `fillnodata(image, mask=None, max_search_distance=100.0, smoothing_iterations=0)` — inverse-distance nodata interpolation.
- drivers (`rasterio.drivers`): `driver_from_extension(path)`, `raster_driver_extensions()`, `is_blacklisted(name)`.

[EXCEPTIONS] (all under `rasterio.errors`, rooted at `RasterioError`):
- `RasterioIOError` — dataset open or I/O failure; `PathError` — bad path/VSI spelling; `OpenerRegistrationError` — custom `opener=` registration failure.
- `DriverCapabilityError` / `DriverRegistrationError` / `OverviewCreationError` — driver create/write/overview capability faults.
- `CRSError` — invalid CRS; `TransformError` / `RPCError` — transform/RPC faults; `WindowError` / `WindowEvaluationError` — invalid window geometry.
- `MergeError` / `StackError` / `WarpOperationError` / `WarpedVRTError` / `WarpOptionsError` — merge/stack/warp faults; `StatisticsError`, `BandOverviewError`, `DatasetIOShapeError`, `InvalidArrayError`.
- `GDALVersionError` / `GDALOptionNotImplementedError` / `GDALBehaviorChangeException` — GDAL build/version mismatches.
- warnings: `RasterioDeprecationWarning`, `NotGeoreferencedWarning`, `NodataShadowWarning`, `ShapeSkipWarning`, `TransformWarning`.

[IMPLEMENTATION_LAW]:
- `rasterio.open` is the single polymorphic dataset entry; mode (`'r'`, `'w'`, `'r+'`), `driver`, and creation kwargs discriminate read versus write, so use it as a context manager to guarantee GDAL close — never a per-driver parallel open function.
- Band indexes are 1-based; `read()` with no `indexes` returns all bands as a `(count, rows, cols)` array, a single integer index returns a 2D array, and `read(masked=True)` returns a `numpy.ma.MaskedArray` honoring `MaskFlags`.
- Georeferencing is the affine `transform` plus `crs`; pixel<->world mapping flows through `xy`/`index`/`window`, and the `transform_method` argument selects affine vs GCP vs RPC transformers — never manual row/col arithmetic.
- Windowed, `boundless`, and `block_windows` reads avoid materializing the full raster; pair windows with `block_windows`/`block_shapes` for tile-aligned streaming, and use `merge(..., method=callable)` for custom mosaic reduction over tiles.
- `warp.reproject`/`calculate_default_transform`/`WarpedVRT` own raster reprojection (CPU `num_threads`, `warp_mem_limit`, `init_dest_nodata`); resampling is a `Resampling` enum row applied inside GDAL — never a hand-rolled resampling kernel.
- `features.shapes`/`features.rasterize` are the bidirectional raster/vector bridge; both honor `transform` and `MergeAlg`, emit or consume GeoJSON-like geometry mappings, and `geometry_window` localizes a rasterize/read to a geometry footprint.
- `Env` scopes process-global GDAL state (drivers, config, credentials); enter it around dataset work and inject a `Session` (`AWSSession`/`GSSession`/`AzureSession`/...) for cloud access rather than mutating GDAL config or environment variables directly.
- `MemoryFile`/`ZipMemoryFile` and the `/vsicurl/`, `/vsizip/`, `/vsis3/` VSI prefixes (or an `opener=` callable bridging `obstore`/fsspec) cover in-memory, archive, and remote datasets without local disk.

[STACK_LAW]:
- `pystac` -> `rasterio`: a `pystac.Item` asset with `media_type == MediaType.COG` carries its href (often signed via `planetary_computer.sign`) straight into `rasterio.open` / `WarpedVRT`; the projection-extension transform/CRS on the item match the dataset `transform`/`crs`.
- `rasterio` -> `rioxarray`: `rioxarray.open_rasterio` wraps `rasterio.open`/`WarpedVRT` and labels the band array as a georeferenced xarray `DataArray`/`Dataset`; reach for raw rasterio only when xarray labeling/dask chunking is not needed, otherwise compose through rioxarray.
- `rasterio` <-> `shapely`/`geopandas`: `features.shapes`/`features.rasterize`/`mask.mask` exchange GeoJSON-like mappings with `shapely` geometries and `geopandas` GeoDataFrames at the vector boundary; `transform_geom` reprojects geometries through the shared PROJ.
- `rasterio` -> `stac-geoparquet`/cloud-egress: `MemoryFile` write + `obstore` PUT (or a `/vsis3/` path under an `AWSSession`) is the COG write-and-upload rail; `default_gtiff_profile` plus `Compression.zstd`/`webp` and tiling kwargs set the COG creation profile.
- `Session` family is the credential discriminant: one `Env(session=...)` scopes the whole read/warp/merge pipeline; the cloud provider is a session row, not a per-call credential argument.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rasterio`
- Owns: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed/tiled/boundless access, reprojection and warping, raster/vector conversion, masking, multi-dataset merge, virtual warped datasets, in-memory and VSI datasets, scoped GDAL config and cloud sessions
- Accept: `rasterio.open` as a context manager, windowed/`boundless`/`block_windows` streaming reads, `warp.reproject`/`WarpedVRT` with `Resampling` enum, `features.shapes`/`features.rasterize` for the vector bridge, `mask.mask` for geometry masking, `merge` (with custom-callable `method`) for mosaics, `Env(session=...)` for scoped GDAL/cloud config, the shared `CRS` object across rioxarray/pyproj
- Reject: manual pixel-to-world coordinate math where `xy`/`index`/`transform_method` apply, full-raster reads where a window suffices, hand-rolled resampling/mosaic kernels where `Resampling`/`merge` dispatch, per-driver parallel open functions when `open` dispatches on mode and driver, duplicated CRS algebra the shared PROJ already owns, raw GDAL-config mutation where `Env`/`Session` scope it, raw rasterio band reads where `rioxarray` should own xarray labeling
