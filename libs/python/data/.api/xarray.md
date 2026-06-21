# [PY_DATA_API_XARRAY]

`xarray` supplies `DataArray`, `Dataset`, `Variable`, `Coordinates`, and `DataTree` labelled named-axis containers with label-based selection, grouping, reduction, interpolation, polynomial/curve fitting, dask/cubed-backed chunking, the `.dt`/`.str`/`.plot` computed accessors, a custom-accessor registration hook, and netCDF/Zarr IO. It is the canonical owner surface for the `field-dataset` CF labelled-field dataset: the `FieldDataset` owner addresses CF coordinates by dimension name through `sel`/`isel`, reads and writes CF field cubes through `open_dataset(engine=)`/`open_zarr`/`to_netcdf`/`to_zarr` over the netcdf4/h5netcdf/zarr engines, opts a cube into the lazy chunked path through `chunk`, and defers grouped reductions to the installed `flox` lowering. xarray is on `banned-module-level-imports`, so every `FieldDataset` body binds it function-local under `# noqa: PLC0415`; it is pure-Python (source-build/no native extension, cp315-available) with no CPython floor and no subprocess seam — its IO/grouped/raster companions (`netcdf4`/`h5netcdf`, `flox`, `rioxarray`, `cubed`/`dask`) carry their own ABI and marker gates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray`
- import: `import xarray as xr`
- version: `2025.x` (manifest unpinned; latest stable)
- license: Apache-2.0
- owner: `data`
- rail: field-dataset
- asset: pure Python; no native extension, no CPython floor (rides cp315 core); the netCDF/HDF5 engines (`netcdf4`/`h5netcdf`), grouped-reduction lowering (`flox`), raster accessor (`rioxarray`), and chunked backends (`cubed`/`dask`) are optional companions carrying their own ABI/marker gates
- entry points: backend plugins register through the `xarray.backends` entry-point group (`netcdf4`, `h5netcdf`, `zarr`, `scipy`, `rasterio` via rioxarray); library use is import-only
- capability: CF-conventioned labelled n-dimensional field cubes — named dimensions, coordinate indexes, CF-aware label selection, grouped/binned/resampled/rolling/coarsen/weighted reductions, interpolation and nodata fill, polynomial and non-linear curve fitting, numerical integration/differentiation, the `.dt`/`.str`/`.plot` computed accessors plus custom-accessor registration, hierarchical `DataTree` with `map_over_datasets`, netCDF/Zarr/Icechunk IO over the netcdf4/h5netcdf/zarr engines with per-variable `encoding`, and a dask/cubed-backed lazy/chunked path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: labelled-array owners
- rail: field-dataset

`xarray.__all__` carries the six subpackages (`coders`, `groupers`, `indexes`, `testing`, `tutorial`, `ufuncs`), the container classes below, the top-level functions in [03], and the typed exceptions (`AlignmentError`, `CoordinateValidationError`, `MergeError`, `InvalidTreeError`, `NotFoundInTreeError`, `TreeIsomorphismError`, `SerializationWarning`). `ALL_DIMS` is the reduce-over-all-dimensions sentinel.

| [INDEX] | [SYMBOL]             | [ROLE]              | [CAPABILITY]                                       |
| :-----: | :------------------- | :------------------ | :------------------------------------------------- |
|  [01]   | `xarray.DataArray`   | labelled array      | array plus named dims and coords                   |
|  [02]   | `xarray.Dataset`     | labelled collection | named DataArrays sharing dimensions                |
|  [03]   | `xarray.DataTree`    | hierarchical tree   | nested Datasets with parent/child links            |
|  [04]   | `xarray.Variable`    | dim-aware buffer    | array with attached dimension names                |
|  [05]   | `xarray.Coordinates` | coordinate index    | label index set for dimensions                     |
|  [06]   | `xarray.Index`       | index protocol      | custom coordinate index backend (`xarray.indexes`) |
|  [07]   | `xarray.NamedArray`  | named buffer        | dimension-named array without coordinate indexes   |
|  [08]   | `xarray.CFTimeIndex` | CF time index       | non-standard-calendar datetime index (cftime)      |
|  [09]   | `xarray.Context`     | apply context       | `apply_ufunc` execution context handle             |

[PUBLIC_TYPE_SCOPE]: shared array members
- rail: field-dataset
- members carry across `DataArray` and `Dataset` except where noted

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                  |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | selection       | `sel`, `isel`, `loc`, `drop_sel`, `drop_isel`, `drop_vars`, `head`, `tail`, `thin`, `where`, `query` |
|  [02]   | structure       | `dims`, `coords`, `sizes`, `attrs`, `encoding`, `assign`, `assign_coords`, `assign_attrs`, `rename`, `swap_dims`, `set_index`, `reset_index`, `set_xindex` |
|  [03]   | reshape         | `stack`, `unstack`, `to_stacked_array`, `transpose`, `expand_dims`, `squeeze`, `broadcast_like`, `pad`, `shift`, `roll`, `sortby` |
|  [04]   | grouping        | `groupby`, `groupby_bins`, `resample`, `rolling`, `rolling_exp`, `coarsen`, `weighted`, `cumulative`        |
|  [05]   | reduction       | `mean`, `sum`, `std`, `var`, `min`, `max`, `median`, `quantile`, `count`, `prod`, `cumsum`, `cumprod`, `all`, `any`, `reduce` |
|  [06]   | arg-reduction   | `idxmax`, `idxmin`, `argmax`, `argmin`, `argsort`, `rank`, `dot`, `cumulative_integrate`                    |
|  [07]   | alignment       | `reindex`, `reindex_like`, `interp`, `interp_like`, `interpolate_na`, `ffill`, `bfill`, `fillna`, `dropna`, `combine_first` |
|  [08]   | fit/calculus    | `polyfit`, `polyval`, `curvefit`, `integrate`, `differentiate`, `diff`                                      |
|  [09]   | chunking        | `chunk`, `chunks`, `chunksizes`, `compute`, `persist`, `load`, `unify_chunks`, `map_blocks`, `as_numpy`     |
|  [10]   | egress          | `to_netcdf`, `to_zarr`, `to_pandas`, `to_dataframe`, `to_dask_dataframe`, `to_numpy`, `to_dict`, `to_dataset` (DataArray), `to_dataarray` (Dataset) |

[PUBLIC_TYPE_SCOPE]: computed accessors and registration
- rail: field-dataset

`obj.dt`/`obj.str` materialize only for the matching dtype; `register_dataarray_accessor`/`register_dataset_accessor`/`register_datatree_accessor` are the class decorators that bind a custom namespace (e.g. `ds.rio` from rioxarray). The grouped/rolling/etc. members in `[04]` return reduction *objects* (`DataArrayRolling`, `DatasetCoarsen`, `Weighted`, `DataArrayResample`, `GroupBy`) carrying their own `.mean`/`.sum`/`.reduce`/`.construct`/`.map` surface.

| [INDEX] | [ACCESSOR]                  | [MEMBERS]                                                                                                                                |
| :-----: | :-------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `.dt` (datetime)            | `year`, `month`, `day`, `hour`, `minute`, `second`, `microsecond`, `nanosecond`, `dayofweek`/`weekday`, `dayofyear`, `weekofyear`/`week`, `quarter`, `season`, `days_in_month`/`daysinmonth`, `days_in_year`, `decimal_year`, `is_month_start`/`is_month_end`, `is_quarter_start`/`is_quarter_end`, `is_year_start`/`is_year_end`, `is_leap_year`, `calendar`, `time`, `date`, `isocalendar()`, `strftime()`, `floor()`, `ceil()`, `round()` |
|  [02]   | `.dt` (timedelta)           | `days`, `seconds`, `microseconds`, `nanoseconds`, `total_seconds()`, `floor()`, `ceil()`, `round()`                                     |
|  [03]   | `.str` (string)             | `len`, `get`, `slice`, `slice_replace`, `cat`, `join`, `contains`, `match`, `startswith`, `endswith`, `find`/`rfind`/`index`/`rindex`, `replace`, `count`, `extract`, `extractall`, `findall`, `split`/`rsplit`, `partition`/`rpartition`, `pad`/`center`/`ljust`/`rjust`/`zfill`, `wrap`, `strip`/`lstrip`/`rstrip`, `lower`/`upper`/`title`/`capitalize`/`swapcase`/`casefold`, `normalize`, `translate`, `repeat`, `get_dummies`, `decode`/`encode`, the `is*` predicates (`isalnum`/`isalpha`/`isdigit`/`isspace`/`isupper`/`islower`/`istitle`/`isnumeric`/`isdecimal`) |
|  [04]   | `.plot` (matplotlib)        | `line`, `step`, `hist`, `pcolormesh`, `contour`, `contourf`, `imshow`, `surface`, `scatter`, `quiver`, `streamplot`, `facetgrid`        |
|  [05]   | `register_*_accessor`       | `register_dataarray_accessor(name)`, `register_dataset_accessor(name)`, `register_datatree_accessor(name)` class decorators            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IO and construction
- rail: field-dataset

`open_dataset`/`open_dataarray` carry `engine`, `chunks`, `decode_cf`, `mask_and_scale`, `decode_times`, `drop_variables`, and `backend_kwargs`; `to_netcdf`/`to_zarr` carry per-variable `encoding` (compression, chunking, `_FillValue`, dtype) that the netcdf4/zarr engine applies.

| [INDEX] | [SURFACE]                                                                                                              | [FAMILY]     | [RETURNS]   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :----------- | :---------- |
|  [01]   | `open_dataset(filename_or_obj, *, engine=None, chunks=None, decode_cf=True, mask_and_scale=None, decode_times=True, drop_variables=None, backend_kwargs=None, **kwargs)` | IO | `Dataset` |
|  [02]   | `open_dataarray(filename_or_obj, *, engine=None, chunks=None, ...)`                                                    | IO           | `DataArray` |
|  [03]   | `open_mfdataset(paths, *, chunks=None, concat_dim=None, combine='by_coords', compat='no_conflicts', preprocess=None, parallel=False, join='outer', engine=None)` | IO | `Dataset` |
|  [04]   | `open_zarr(store, *, group=None, chunks='auto', decode_cf=True, consolidated=None, **kwargs)`                          | IO           | `Dataset`   |
|  [05]   | `open_datatree(filename_or_obj, *, engine=None, **kwargs)` / `open_groups(filename_or_obj, *, engine=None, **kwargs)`  | IO           | `DataTree` / `dict[str, Dataset]` |
|  [06]   | `load_dataset(filename_or_obj)` / `load_dataarray(filename_or_obj)` / `load_datatree(filename_or_obj)`                 | IO           | eager value |
|  [07]   | `save_mfdataset(datasets, paths, mode='w', **kwargs)`                                                                  | IO           | `None`      |
|  [08]   | `decode_cf(obj, *, decode_times=True, mask_and_scale=True, decode_coords=True)`                                        | construction | `Dataset`   |
|  [09]   | `show_versions()` / `set_options(**kwargs)` / `get_options()`                                                          | config       | versions / options |

[ENTRYPOINT_SCOPE]: combination, computation, and tree
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                               | [FAMILY]     | [RETURNS]              |
| :-----: | :------------------------------------------------------------------------------------------------------ | :----------- | :--------------------- |
|  [01]   | `concat(objs, dim, *, data_vars='all', coords='different', compat='equals', join='outer')`              | combine      | `Dataset`/`DataArray`  |
|  [02]   | `merge(objects, *, compat='no_conflicts', join='outer', fill_value=<NA>, combine_attrs='drop')`         | combine      | `Dataset`              |
|  [03]   | `combine_by_coords(data_objects, *, compat=..., data_vars=..., coords=..., join=...)`                   | combine      | `Dataset`/`DataArray`  |
|  [04]   | `combine_nested(datasets, concat_dim, *, compat=..., data_vars=..., coords=...)`                        | combine      | `Dataset`/`DataTree`   |
|  [05]   | `align(*objects, join='inner', copy=True, fill_value=<NA>, exclude=...)` / `broadcast(*args, exclude=None)` | align     | aligned / broadcast tuple |
|  [06]   | `apply_ufunc(func, *args, input_core_dims=None, output_core_dims=((),), exclude_dims=..., vectorize=False, dask='forbidden', output_dtypes=None, dask_gufunc_kwargs=None)` | compute | wrapped result |
|  [07]   | `dot(*arrays, dim=None)` / `cross(a, b, *, dim)` / `polyval(coord, coeffs, *, degree_dim='degree')`     | compute      | `DataArray`/`Variable` |
|  [08]   | `corr(da_a, da_b, *, dim=None, weights=None)` / `cov(da_a, da_b, *, dim=None, ddof=1, weights=None)`     | compute      | `DataArray`            |
|  [09]   | `where(cond, x, y, *, keep_attrs=None)`                                                                  | compute      | `Dataset`/`DataArray`  |
|  [10]   | `map_blocks(func, obj, args=(), kwargs=None, template=None)` / `unify_chunks(*objects)`                 | dask/cubed   | `Dataset`/`DataArray`  |
|  [11]   | `full_like(other, fill_value, ...)` / `ones_like(other, ...)` / `zeros_like(other, ...)`                | construction | like-shaped array      |
|  [12]   | `map_over_datasets(func, *args, kwargs=None)` / `group_subtrees(*trees)`                                 | tree         | `DataTree` / iterator  |
|  [13]   | `date_range(start, end, periods, freq, calendar='standard', use_cftime=None)` / `date_range_like(source, calendar)` / `cftime_range(...)` / `infer_freq(index)` | time | index or freq |
|  [14]   | `as_variable(obj, name=None)`                                                                            | construction | `Variable`             |

## [04]-[IMPLEMENTATION_LAW]

[NAMING_TOPOLOGY]:
- A CF field cube is a `Dataset` of `DataArray`s carrying CF dimension names, coordinate labels, and CF metadata (`units`/`standard_name`/`grid_mapping`); `open_dataset(engine=)`/`open_mfdataset`/`open_zarr` materialise it, `decode_cf` applies the CF mask/scale/time decode, and the `FieldDataset` owner keys the cube by one runtime `ContentIdentity`.
- Label-based `sel`/`isel` address CF coordinates by name (`FieldSelection.Sel`/`Isel`); `interp` is the CF-aware interpolation arm and `groupby`/`groupby_bins`/`resample`/`rolling`/`coarsen`/`weighted` the grouped/binned/resampled/windowed/weighted reductions, each a `FieldSelection` case rather than a sibling method, returning a reduction object whose `.mean`/`.sum`/`.reduce`/`.map` lowers to the kernel.
- the `.dt`/`.str` computed accessors are the time-component and string-vector arms; custom accessors register through `register_dataarray_accessor`/`register_dataset_accessor` (the mechanism `rioxarray` uses for `.rio`), never a bare monkeypatch.
- `chunk` opts a cube into the lazy chunked path; `compute`/`load`/`persist` materialise it, and `unify_chunks`/`map_blocks` operate over the chunked graph. The chunked backend is `dask` by default or `cubed` via `chunked_array_type="cubed"` for the bounded-memory path.
- `apply_ufunc` is the boundary for arbitrary NumPy-style functions over labelled cubes; `input_core_dims`/`output_core_dims` declare the broadcasting contract and `dask='parallelized'` lifts it onto the chunked graph. `polyfit`/`curvefit` own polynomial and non-linear fitting; `integrate`/`differentiate` own coordinate calculus.
- IO flows through `open_dataset`/`open_zarr`/`open_datatree` at the boundary and `to_netcdf`/`to_zarr` at egress with per-variable `encoding`; the `FieldDataset` egress materialises to the same content-keyed `pyarrow`/Zarr surface the `tensor`/`columnar` owners speak.

[INTEGRATION]:
- flox seam: when `flox` is installed `xarray` automatically lowers `groupby`/`groupby_bins`/`resample` reductions onto the vectorized numpy-groupies / map-reduce kernel; the `FieldSelection` grouped arm gets parallel grouped reductions for free, and `flox.xarray.xarray_reduce` is the direct entry for the cohorts/blockwise strategies and custom `Aggregation` escapes.
- netcdf4/h5netcdf seam: `open_dataset(path, engine='netcdf4')` (or `engine='h5netcdf'`) routes CF decode through the `netcdf4` C-extension owner; reach `netcdf4` directly only for low-level group/dimension/variable authoring, MPI-collective write, or `memory=`/`diskless=` round-trips. `cftime` owns the non-standard-calendar `CFTimeIndex`.
- zarr/icechunk seam: `open_zarr`/`Dataset.to_zarr(store=...)` target any zarr `StoreLike`; passing an `IcechunkStore` (from `repo.writable_session(branch).store`) gives the transactional/versioned cube, and `to_zarr(region=, append_dim=)` does the partial/append writes. `tensorstore` reads the same on-disk v3 store asynchronously.
- rioxarray seam: `import rioxarray` registers the `.rio` accessor (CRS/transform/nodata, `reproject`/`clip`/`to_raster`) and the `engine='rasterio'` backend on `DataArray`/`Dataset`; the raster coverage rail and `odc-stac` lazy cubes ride this accessor.
- virtualizarr seam: `open_virtual_dataset` returns a `ManifestArray`-backed `xarray.Dataset` that references existing files without copy; `ds.virtualize.to_icechunk`/`to_kerchunk` persists the reference manifest.
- cubed/dask seam: `chunk` + `chunked_array_type="cubed"` (or default dask) backs the lazy path; `map_blocks`/`apply_ufunc(dask='parallelized')` push arbitrary functions onto the chunked graph, and the bounded-memory `cubed` executor caps per-task memory for out-of-core cubes.

[LOCAL_ADMISSION]:
- `open_dataset(engine=)`/`open_zarr` reads and `to_netcdf`/`to_zarr` writes bind function-local under `# noqa: PLC0415`; egress carries per-variable `encoding` for compression/chunking/fill, never a side-channel metadata write.
- `decode_cf` for CF metadata; `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample`/`rolling`/`coarsen`/`weighted` as `FieldSelection` cases discriminating on selection shape, never sibling methods.
- the dask/cubed path via `chunk`; `compute`/`load`/`persist` to materialise; `chunked_array_type="cubed"` for the bounded-memory executor.
- `set_options`/`get_options` for global decode/display policy; custom domain accessors via `register_*_accessor`, never a monkeypatch.

[RAIL_LAW]:
- Package: `xarray`
- Owns: the CF labelled-field cube for the `field-dataset` owner — labelled named-axis arrays, coordinate indexes, CF-aware selection/grouping/resampling/rolling/weighted reductions, interpolation and fill, polynomial/curve fitting and coordinate calculus, the `.dt`/`.str`/`.plot` accessors and custom-accessor registration, netCDF/Zarr/Icechunk IO over netcdf4/h5netcdf/zarr, hierarchical `DataTree`, and the dask/cubed lazy path
- Accept: `open_dataset(engine=)`/`open_zarr` reads and `to_netcdf`/`to_zarr` writes bound function-local under `# noqa: PLC0415`, `decode_cf` for CF metadata, `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample`/`rolling`/`coarsen`/`weighted` as `FieldSelection` cases, the `flox` grouped-reduction lowering, the `.dt`/`.str` accessors, the dask/cubed path via `chunk`, and an `IcechunkStore` for the transactional cube
- Reject: a module-level xarray import (banned), positional-only axis handling where CF names exist, wrapper-renames of label selection, a second labelled-array store inside `tensor`, hand-rolled grouped reductions where `flox` lowers, hand-decoded CF time strings where `cftime` owns the calendar, and treating CF coordinates as wire vocabulary
