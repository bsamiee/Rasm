# [PY_BRANCH_API_XARRAY]

`xarray` owns the CF-conventioned labelled n-dimensional field cube over `numpy` buffers: named dimensions, coordinate indexes, CF-aware selection, grouped and windowed and weighted reductions, interpolation and coordinate calculus, hierarchical `DataTree`, and the netCDF/Zarr/Icechunk IO plane feeding the `field-dataset` rail. Every `FieldDataset` body binds it function-local, so the cube never widens the module import graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray` (`Apache-2.0`)
- module: `xarray`
- namespaces: `xarray`, `xarray.groupers`, `xarray.indexes`, `xarray.coders`, `xarray.ufuncs`, `xarray.backends`, `xarray.testing`, `xarray.tutorial`
- rail: field-dataset

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: labelled-array owners

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------ | :------------ | :------------------------------------------------- |
|  [01]   | `DataArray`   | class         | array with named dims and coords                   |
|  [02]   | `Dataset`     | class         | named DataArrays sharing dimensions                |
|  [03]   | `DataTree`    | class         | nested Datasets with parent/child links            |
|  [04]   | `Variable`    | class         | array with attached dimension names                |
|  [05]   | `Coordinates` | class         | label index set for dimensions                     |
|  [06]   | `Index`       | protocol      | custom coordinate index backend (`xarray.indexes`) |
|  [07]   | `NamedArray`  | class         | dimension-named array without coordinate indexes   |
|  [08]   | `CFTimeIndex` | index         | non-standard-calendar datetime index (cftime)      |
|  [09]   | `Context`     | class         | `apply_ufunc` execution context handle             |

[exceptions]: `AlignmentError` `CoordinateValidationError` `MergeError` `InvalidTreeError` `NotFoundInTreeError` `TreeIsomorphismError` `SerializationWarning`
[sentinels]: `ALL_DIMS` selects every dimension in a reduction.

[PUBLIC_TYPE_SCOPE]: members shared across `DataArray` and `Dataset`

[selection]: `sel` `isel` `loc` `drop_sel` `drop_isel` `drop_vars` `head` `tail` `thin` `where` `query`
[structure]: `dims` `coords` `sizes` `attrs` `encoding` `assign` `assign_coords` `assign_attrs` `rename` `swap_dims` `set_index` `reset_index` `set_xindex`
[reshape]: `stack` `unstack` `to_stacked_array` `transpose` `expand_dims` `squeeze` `broadcast_like` `pad` `shift` `roll` `sortby`
[grouping]: `groupby` `groupby_bins` `resample` `rolling` `rolling_exp` `coarsen` `weighted` `cumulative`
[reduction]: `mean` `sum` `std` `var` `min` `max` `median` `quantile` `count` `prod` `cumsum` `cumprod` `all` `any` `reduce`
[arg-reduction]: `idxmax` `idxmin` `argmax` `argmin` `argsort` `rank` `dot` `cumulative_integrate`
[alignment]: `reindex` `reindex_like` `interp` `interp_like` `interpolate_na` `ffill` `bfill` `fillna` `dropna` `combine_first`
[fit-calculus]: `polyfit` `polyval` `curvefit` `integrate` `differentiate` `diff`
[chunking]: `chunk` `chunks` `chunksizes` `compute` `persist` `load` `unify_chunks` `map_blocks` `as_numpy`
[egress]: `to_netcdf` `to_zarr` `to_pandas` `to_dataframe` `to_dask_dataframe` `to_numpy` `to_dict` `to_dataset` `to_dataarray`

[PUBLIC_TYPE_SCOPE]: computed accessors and registration

`obj.dt` and `obj.str` materialize only for the matching dtype, and `register_dataarray_accessor`/`register_dataset_accessor`/`register_datatree_accessor` bind a custom namespace as a class decorator. Grouped, rolling, and windowed members return reduction objects — `GroupBy`, `DataArrayRolling`, `DatasetCoarsen`, `Weighted`, `DataArrayResample` — each carrying its own `.mean`/`.sum`/`.reduce`/`.construct`/`.map` surface.

[.dt timedelta]: `days` `seconds` `microseconds` `nanoseconds` `total_seconds()` `floor()` `ceil()` `round()`

```text
.dt (datetime64) — time-component reads and rounders
year month day hour minute second microsecond nanosecond dayofweek/weekday dayofyear
weekofyear/week quarter season days_in_month/daysinmonth days_in_year decimal_year
is_month_start/is_month_end is_quarter_start/is_quarter_end is_year_start/is_year_end
is_leap_year calendar time date isocalendar() strftime() floor() ceil() round()

.str (string) — vectorized string ops; the is* predicate tail is a closed set
len get slice slice_replace cat join contains match startswith endswith
find/rfind/index/rindex replace count extract extractall findall split/rsplit
partition/rpartition pad/center/ljust/rjust/zfill wrap strip/lstrip/rstrip
lower/upper/title/capitalize/swapcase/casefold normalize translate repeat get_dummies decode/encode
isalnum isalpha isdigit isspace isupper islower istitle isnumeric isdecimal

.plot (matplotlib)
line step hist pcolormesh contour contourf imshow surface scatter quiver streamplot facetgrid
```

[PUBLIC_TYPE_SCOPE]: grouper objects (`xarray.groupers`)

`groupby` and `resample` take an explicit `Grouper` instance keyed by dimension — `ds.groupby(time=TimeResampler(freq="1D"))` — never a `freq=`/`bins=` string shorthand.

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------------------------------- | :------------ | :------------------------ |
|  [01]   | `groupers.TimeResampler(freq, closed, label, origin, offset)`                     | grouper       | calendar/offset resampler |
|  [02]   | `groupers.UniqueGrouper(labels)`                                                  | grouper       | unique-value grouper      |
|  [03]   | `groupers.BinGrouper(bins, right, labels, precision, include_lowest, duplicates)` | grouper       | histogram-bin grouper     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IO and construction
- open carry: `engine`, `chunks`, `decode_cf`, `mask_and_scale`, `decode_times`, `drop_variables`, `backend_kwargs`

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------- | :------ | :------------------------- |
|  [01]   | `open_dataset(obj, *, engine, chunks) -> Dataset`                    | static  | CF-decoding lazy read      |
|  [02]   | `open_dataarray(obj, *, engine, chunks) -> DataArray`                | static  | single-variable read       |
|  [03]   | `open_mfdataset(paths, *, combine, preprocess, parallel) -> Dataset` | static  | multi-file concat read     |
|  [04]   | `open_zarr(store, *, group, consolidated) -> Dataset`                | static  | Zarr store read            |
|  [05]   | `open_datatree(obj) -> DataTree` / `open_groups(obj) -> dict`        | static  | hierarchical read          |
|  [06]   | `load_dataset(obj)` / `load_dataarray(obj)` / `load_datatree(obj)`   | static  | eager read                 |
|  [07]   | `save_mfdataset(datasets, paths, *, mode)`                           | static  | multi-file write           |
|  [08]   | `decode_cf(obj, *, decode_times, mask_and_scale, decode_coords)`     | static  | CF mask/scale/time decode  |
|  [09]   | `set_options(**kw)` / `get_options()` / `show_versions()`            | static  | global policy, diagnostics |

[ENTRYPOINT_SCOPE]: combination, computation, and tree
- combine carry: `compat`, `join`, `data_vars`, `coords`

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `concat(objs, dim, *, data_vars, coords)`                                      | static  | concatenate along one dim      |
|  [02]   | `merge(objects, *, fill_value, combine_attrs) -> Dataset`                      | static  | merge coords and variables     |
|  [03]   | `combine_by_coords(objs)`                                                      | static  | coordinate-ordered combine     |
|  [04]   | `combine_nested(datasets, concat_dim)`                                         | static  | explicit-layout combine        |
|  [05]   | `align(*objects, *, join, fill_value)` / `broadcast(*args, exclude)`           | static  | index align, broadcast         |
|  [06]   | `apply_ufunc(func, *args, input_core_dims, output_core_dims, vectorize, dask)` | static  | arbitrary kernel over the cube |
|  [07]   | `dot(*arrays, dim)` / `cross(a, b, *, dim)`                                    | static  | contraction, cross product     |
|  [08]   | `polyval(coord, coeffs, *, degree_dim) -> DataArray`                           | static  | polynomial evaluation          |
|  [09]   | `corr(da_a, da_b, *, dim, weights) -> DataArray`                               | static  | correlation over dims          |
|  [10]   | `cov(da_a, da_b, *, dim, ddof, weights) -> DataArray`                          | static  | covariance over dims           |
|  [11]   | `where(cond, x, y, *, keep_attrs)`                                             | static  | conditional selection          |
|  [12]   | `map_blocks(func, obj, *, args, template)` / `unify_chunks(*objects)`          | static  | chunk-graph map, chunk unify   |
|  [13]   | `full_like(other, fill_value)` / `ones_like(other)` / `zeros_like(other)`      | factory | like-shaped array              |
|  [14]   | `map_over_datasets(func, *args)` / `group_subtrees(*trees)`                    | static  | tree map, subtree zip          |
|  [15]   | `date_range(start, end, periods, freq, *, calendar, use_cftime)`               | factory | calendar-aware index           |
|  [16]   | `date_range_like(source, calendar)` / `infer_freq(index)`                      | factory | derived index, inferred freq   |
|  [17]   | `as_variable(obj, *, name) -> Variable`                                        | factory | wrap an array as `Variable`    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A CF field cube is a `Dataset` of `DataArray`s over `numpy` buffers keyed by one runtime `ContentIdentity`; CF dimension names, coordinate labels, and CF metadata (`units`/`standard_name`/`grid_mapping`) are its whole addressing vocabulary.
- Selection, interpolation, grouping, windowing, and weighting are `FieldSelection` cases discriminating on selection shape, each returning a reduction object whose `.mean`/`.sum`/`.reduce`/`.map` lowers to the kernel.
- `chunk` opts the cube into the lazy chunked graph, `compute`/`load`/`persist` materialize it, and `apply_ufunc(dask='parallelized')`/`map_blocks` lift arbitrary kernels onto it.
- `decode_cf` applies the CF mask/scale/time decode at the boundary, and egress materializes to the content-keyed `pyarrow`/Zarr surface the `tensor` and `columnar` owners speak.
- A read backend registers through the `xarray.backends` entry-point group, so `engine=` admits any installed plugin with no xarray-side edit.
- `.dt` and `.str` are the time-component and string-vector arms; a domain accessor registers through `register_dataarray_accessor`/`register_dataset_accessor`, which is the mechanism `rioxarray` binds `.rio` through.

[STACKING]:
- `numpy`(`.api/numpy.md`): `apply_ufunc(dask='parallelized')` lifts an arbitrary `ndarray` kernel over the labelled cube under declared `input_core_dims`/`output_core_dims`, and `.data` extracts the raw `ndarray` for `numpy`-owned `linalg`/`fft` work.
- `obstore`(`.api/obstore.md`): `open_zarr` and `Dataset.to_zarr(store=...)` bind an object-store root through the `obstore.fsspec` `AbstractFileSystem` adapter wherever a filesystem handle is the store contract.
- `flox`(`libs/python/data/.api/flox.md`): `flox` lowers `groupby`/`groupby_bins`/`resample` onto the vectorized numpy-groupies map-reduce kernel; `flox.xarray.xarray_reduce` is the direct entry for cohorts and blockwise strategies and custom `Aggregation` escapes.
- `netcdf4`(`libs/python/data/.api/netcdf4.md`): `open_dataset(engine='netcdf4')` and `engine='h5netcdf'` route CF decode through the C-extension owner; reach `netcdf4` directly for low-level group/dimension/variable authoring, MPI-collective write, or `memory=`/`diskless=` round-trips, and `cftime` owns the `CFTimeIndex` calendar.
- `zarr`(`libs/python/data/.api/zarr.md`): `open_zarr` and `to_zarr(store=...)` target any `StoreLike`, `to_zarr(region=, append_dim=)` does partial and append writes, and an `IcechunkStore` from `repo.writable_session(branch).store` gives the transactional versioned cube.
- `rioxarray`(`libs/python/data/.api/rioxarray.md`): importing `rioxarray` registers the `.rio` accessor — CRS, transform, nodata, `reproject`, `clip`, `to_raster` — and the `engine='rasterio'` backend, and the raster coverage rail and `odc-stac` lazy cubes ride that accessor.
- `virtualizarr`(`libs/python/data/.api/virtualizarr.md`): `open_virtual_dataset` returns a `ManifestArray`-backed `Dataset` referencing existing files without copy, and `ds.virtualize.to_icechunk`/`to_kerchunk` persists the reference manifest.
- `cubed`(`libs/python/data/.api/cubed.md`): `chunk(chunked_array_type="cubed")` caps per-task memory for out-of-core cubes, where `dask`(`libs/python/compute/.api/dask.md`) backs the default chunked graph.
- within-lib: `numerics/array` extracts `.data` with coords into `NamedAxis` rows through its `ArraySource.Labelled` arm, `FieldSelection` folds `sel`/`isel`/`groupby`/`resample`/`rolling` into one discriminated dispatch, and `FieldDataset` egress shares the content-keyed `pyarrow`/Zarr surface.

[LOCAL_ADMISSION]:
- `open_dataset(engine=)`/`open_zarr` reads and `to_netcdf`/`to_zarr` writes bind function-local under `# noqa: PLC0415`; egress carries compression, chunking, and fill through per-variable `encoding`.
- `decode_cf` owns CF metadata, and `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample`/`rolling`/`coarsen`/`weighted` enter as `FieldSelection` cases.
- `chunk` opts into the dask path with `chunked_array_type="cubed"` selecting the bounded-memory executor; `set_options`/`get_options` set global decode and display policy; a domain accessor registers through `register_*_accessor`.

[RAIL_LAW]:
- Package: `xarray`
- Owns: the CF labelled-field cube for `field-dataset` — named-axis arrays over `numpy`, coordinate indexes, CF-aware selection and grouped/resampled/rolling/weighted reduction, interpolation and fill, polynomial and curve fitting, coordinate calculus, the `.dt`/`.str`/`.plot` and custom accessors, netCDF/Zarr/Icechunk IO, hierarchical `DataTree`, and the dask/cubed lazy path
- Accept: function-local `open_*`/`to_*` binding, `decode_cf` metadata, `FieldSelection`-cased selection and reduction, the `flox` grouped lowering, explicit `Grouper` instances, the `.dt`/`.str` accessors, `chunk` onto dask or cubed, and an `IcechunkStore` for the transactional cube
- Reject: a module-level `xarray` import, positional axis handling where CF names exist, wrapper-renames of label selection, a second labelled-array store inside `tensor`, hand-rolled grouped reductions `flox` lowers, hand-decoded CF time strings `cftime` owns, and CF coordinates treated as wire vocabulary
