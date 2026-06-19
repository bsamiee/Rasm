# [PY_DATA_API_RASTERIO]

`rasterio` supplies a Pythonic NumPy-array interface over GDAL for raster geospatial data, reading and writing bands as `ndarray` with affine georeferencing. It provides `open`, `DatasetReader`, `MemoryFile`, and `Env` as primary owners, with windowed reads, masking, reprojection, vectorization, and merge operations on the `features`, `warp`, `mask`, `windows`, and `merge` submodules.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasterio`
- package: `rasterio`
- import: `import rasterio`
- owner: `data`
- rail: geospatial
- capability: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed and tiled access, in-memory and VSI datasets, reprojection and warping, raster/vector conversion, masking, and multi-dataset merge

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `rasterio.DatasetReader` — open raster dataset; band read, windowing, masking, sampling, and metadata.
- `rasterio.io.DatasetWriter` / `BufferedDatasetWriter` — write-mode datasets returned by `open(mode='w'|'r+')`.
- `rasterio.io.MemoryFile`, `rasterio.io.ZipMemoryFile` — in-memory dataset backing for read and write without disk I/O.
- `rasterio.Env` — GDAL environment context manager; scopes drivers, credentials, and config options.
- `rasterio.windows.Window` — pixel-space read/write subregion with `from_bounds`, `intersection`, `crop`, `round_lengths`.
- `rasterio.coords.BoundingBox` — `(left, bottom, right, top)` named tuple; `rasterio.transform.AffineTransformer` maps pixel and world coordinates.
- `rasterio.crs.CRS` — coordinate reference system; `rasterio.Affine` (from the `affine` package) is the georeferencing transform.
- `rasterio.enums.*` — `Resampling`, `ColorInterp`, `Compression`, `MaskFlags`, `MergeAlg`, `PhotometricInterp`, `Interleaving`, `OverviewResampling`, `WktVersion`.
- `rasterio.profiles.Profile`, `rasterio.profiles.default_gtiff_profile` — creation-option profile dicts.
- `rasterio.transform.GroundControlPoint`, `rasterio.transform.RPC` — GCP and rational-polynomial georeferencing.
- `rasterio.vrt.WarpedVRT` — virtual warped dataset wrapping a source under a target CRS/transform/resampling.

[ENTRYPOINTS]:
- open: `rasterio.open(fp, mode='r', driver=None, width=None, height=None, count=None, crs=None, transform=None, dtype=None, nodata=None, sharing=False, thread_safe=False, opener=None, **kwargs) -> DatasetReader | DatasetWriter`.
- band read/write: `DatasetReader.read(indexes=None, out=None, window=None, masked=False, out_shape=None, boundless=False, fill_value=None, resampling=Resampling.nearest)`, `DatasetReader.read_masks(...)`, `DatasetWriter.write(arr, indexes=None, window=None)`, `DatasetReader.sample(xy, indexes=None, masked=False)`.
- dataset metadata: `DatasetReader.meta`, `.profile`, `.bounds`, `.crs`, `.transform`, `.res`, `.shape`, `.count`, `.dtypes`, `.nodata`, `.nodatavals`, `.colorinterp`, `.descriptions`, `.tags(ns=None)`, `.overviews(bidx)`, `.statistics(bidx, ...)`, `.block_windows(bidx)`.
- coordinate mapping: `DatasetReader.index(x, y)`, `DatasetReader.xy(row, col, offset='center')`, `DatasetReader.window(left, bottom, right, top)`, `DatasetReader.window_bounds(window)`, `DatasetReader.window_transform(window)`.
- reproject/warp: `rasterio.warp.reproject(source, destination, src_transform, src_crs, dst_transform, dst_crs, resampling=Resampling.nearest, ...)`, `rasterio.warp.calculate_default_transform(src_crs, dst_crs, width, height, *bounds, resolution=None)`, `rasterio.warp.transform_bounds(...)`, `rasterio.warp.transform_geom(...)`, `rasterio.warp.aligned_target(...)`.
- raster <-> vector: `rasterio.features.shapes(source, mask=None, connectivity=4, transform=IDENTITY)`, `rasterio.features.rasterize(shapes, out_shape=None, fill=0, transform=IDENTITY, all_touched=False, merge_alg=MergeAlg.replace, dtype=None)`, `rasterio.features.geometry_mask(geometries, out_shape, transform, ...)`, `rasterio.features.sieve(source, size, ...)`, `rasterio.features.dataset_features(...)`.
- mask: `rasterio.mask.mask(dataset, shapes, all_touched=False, invert=False, nodata=None, filled=True, crop=False, pad=False, indexes=None) -> (out_image, out_transform)`, `rasterio.mask.raster_geometry_mask(...)`.
- merge: `rasterio.merge.merge(datasets, bounds=None, res=None, nodata=None, dtype=None, method='first', resampling=Resampling.nearest, ...) -> (mosaic, out_transform)`.
- windows: `rasterio.windows.from_bounds(left, bottom, right, top, transform)`, `rasterio.windows.bounds(window, transform)`, `rasterio.windows.transform(window, transform)`, `rasterio.windows.get_data_window(arr, nodata=None)`.

[EXCEPTIONS]:
- `rasterio.errors.RasterioIOError` — dataset open or I/O failure.
- `rasterio.errors.DriverCapabilityError` — driver lacks the requested create or write capability.
- `rasterio.errors.CRSError` — invalid CRS.
- `rasterio.errors.WindowError` — invalid window geometry.
- `rasterio.errors.RasterioDeprecationWarning` — deprecated API warning.

[IMPLEMENTATION_LAW]:
- `rasterio.open` is the single polymorphic dataset entry; mode (`'r'`, `'w'`, `'r+'`), driver, and creation kwargs discriminate read versus write, so use it as a context manager to guarantee GDAL close.
- Band indexes are 1-based; `read()` with no `indexes` returns all bands as a `(count, rows, cols)` array, and a single integer index returns a 2D array.
- Georeferencing is the affine `transform` plus `crs`; pixel-to-world mapping flows through `xy`/`index`/`window`, never through manual row/col arithmetic.
- Windowed and `boundless` reads avoid materializing the full raster; pair windows with `block_windows` for tile-aligned streaming over large datasets.
- `warp.reproject` and `warp.calculate_default_transform` own raster reprojection; resampling is selected from the `Resampling` enum and applied inside GDAL.
- `features.shapes` and `features.rasterize` are the bidirectional raster/vector bridge; both honor the `transform` and emit or consume GeoJSON-like geometry mappings.
- `Env` scopes process-global GDAL state (drivers, AWS credentials, config); enter it around dataset work rather than mutating GDAL config directly.
- `MemoryFile` enables read/write of in-memory or VSI-backed datasets; combine with VSI paths (`/vsicurl/`, `/vsizip/`) for remote and archive access.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rasterio`
- Owns: raster read/write as NumPy arrays, affine and CRS georeferencing, windowed and tiled access, reprojection and warping, raster/vector conversion, masking, multi-dataset merge, in-memory and VSI datasets
- Accept: `rasterio.open` as a context manager, windowed and `boundless` reads for large rasters, `warp.reproject` with `Resampling` enum, `features.shapes`/`features.rasterize` for the vector bridge, `Env` for scoped GDAL config
- Reject: manual pixel-to-world coordinate math, full-raster reads where a window suffices, hand-rolled resampling kernels, and per-driver parallel open functions when `open` dispatches on mode and driver
