# [PY_DATA_API_XARRAY]

`xarray` supplies `DataArray`, `Dataset`, `Variable`, `Coordinates`, and `DataTree` labelled named-axis containers with label-based selection, grouping, reduction, interpolation, dask-backed chunking, and netCDF/Zarr IO. Study plans address axes by dimension name through `sel`/`isel` rather than position, and `chunk` opts an array into the lazy dask path.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray`
- import: `import xarray as xr`
- owner: `data`
- rail: arrays
- capability: labelled n-dimensional arrays — named dimensions, coordinate indexes, dataset grouping, label-based selection, hierarchical `DataTree`, and a dask-backed lazy/chunked path

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: labelled-array owners
- rail: arrays

| [INDEX] | [SYMBOL]             | [ROLE]              | [CAPABILITY]                            |
| :-----: | :------------------- | :------------------ | :-------------------------------------- |
|   [1]   | `xarray.DataArray`   | labelled array      | array plus named dims and coords        |
|   [2]   | `xarray.Dataset`     | labelled collection | named DataArrays sharing dimensions     |
|   [3]   | `xarray.Variable`    | dim-aware buffer    | array with attached dimension names     |
|   [4]   | `xarray.Coordinates` | coordinate index    | label index set for dimensions          |
|   [5]   | `xarray.DataTree`    | hierarchical tree   | nested Datasets with parent/child links |
|   [6]   | `xarray.Index`       | index protocol      | custom coordinate index backend         |

[PUBLIC_TYPE_SCOPE]: shared array members
- rail: arrays
- members carry across `DataArray` and `Dataset` except where noted

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                  |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------- |
|   [1]   | selection       | `sel`, `isel`, `loc`, `drop_sel`, `drop_isel`, `head`, `tail`, `thin`, `where`                             |
|   [2]   | structure       | `dims`, `coords`, `sizes`, `attrs`, `assign_coords`, `assign_attrs`, `rename`, `swap_dims`, `set_index`    |
|   [3]   | reshape         | `stack`, `unstack`, `transpose`, `expand_dims`, `squeeze`, `broadcast_like`, `pad`, `shift`, `roll`        |
|   [4]   | grouping        | `groupby`, `groupby_bins`, `resample`, `rolling`, `coarsen`, `weighted`                                    |
|   [5]   | reduction       | `mean`, `sum`, `std`, `var`, `min`, `max`, `median`, `quantile`, `count`, `prod`, `cumsum`                 |
|   [6]   | alignment       | `reindex`, `reindex_like`, `interp`, `interp_like`, `interpolate_na`, `ffill`, `bfill`, `fillna`, `dropna` |
|   [7]   | chunking        | `chunk`, `chunks`, `chunksizes`, `compute`, `persist`, `load`, `unify_chunks`, `map_blocks`                |
|   [8]   | egress          | `to_netcdf`, `to_zarr`, `to_pandas`, `to_dataframe`, `to_numpy`, `to_dict`, `to_dataset` (DataArray)       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IO and construction
- rail: arrays

| [INDEX] | [SURFACE]                                                          | [FAMILY]     | [RETURNS]   |
| :-----: | :----------------------------------------------------------------- | :----------- | :---------- |
|   [1]   | `open_dataset(filename_or_obj, *, engine=None, chunks=None)`       | IO           | `Dataset`   |
|   [2]   | `open_dataarray(filename_or_obj, *, engine=None, chunks=None)`     | IO           | `DataArray` |
|   [3]   | `open_mfdataset(paths, chunks=None, ...)`                          | IO           | `Dataset`   |
|   [4]   | `open_zarr(store, group=None, chunks=..., decode_cf=True)`         | IO           | `Dataset`   |
|   [5]   | `open_datatree(filename_or_obj, *, engine=None)`                   | IO           | `DataTree`  |
|   [6]   | `load_dataset(filename_or_obj)`, `load_dataarray(filename_or_obj)` | IO           | eager value |
|   [7]   | `save_mfdataset(datasets, paths, mode='w')`                        | IO           | `None`      |
|   [8]   | `decode_cf(obj, decode_times=True, mask_and_scale=True)`           | construction | `Dataset`   |

[ENTRYPOINT_SCOPE]: combination and computation
- rail: arrays

| [INDEX] | [SURFACE]                                                                                               | [FAMILY]     | [RETURNS]              |
| :-----: | :------------------------------------------------------------------------------------------------------ | :----------- | :--------------------- |
|   [1]   | `concat(objs, dim, data_vars=all, coords=...)`                                                          | combine      | `Dataset`/`DataArray`  |
|   [2]   | `merge(objects, compat=..., join=...)`                                                                  | combine      | `Dataset`              |
|   [3]   | `combine_by_coords(data_objects=[], ...)`                                                               | combine      | `Dataset`/`DataArray`  |
|   [4]   | `combine_nested(datasets, concat_dim)`                                                                  | combine      | `Dataset`/`DataTree`   |
|   [5]   | `align(*objects, join='inner', copy=True)`                                                              | align        | aligned tuple          |
|   [6]   | `broadcast(*args, exclude=None)`                                                                        | align        | broadcast tuple        |
|   [7]   | `apply_ufunc(func, *args, input_core_dims=None, output_core_dims=None)`                                 | compute      | wrapped result         |
|   [8]   | `dot(*arrays, dim=None)`, `cross(a, b, *, dim)`                                                         | compute      | `DataArray`/`Variable` |
|   [9]   | `corr(da_a, da_b, dim=None, weights=None)`, `cov(da_a, da_b, dim=None, ddof=1)`                         | compute      | `DataArray`            |
|  [10]   | `polyval(coord, coeffs, degree_dim='degree')`                                                           | compute      | `Dataset`/`DataArray`  |
|  [11]   | `map_blocks(func, obj, args=(), kwargs=None)`                                                           | dask         | `Dataset`/`DataArray`  |
|  [12]   | `full_like(other, fill_value, ...)`, `ones_like(other, ...)`, `zeros_like(other, ...)`                  | construction | like-shaped array      |
|  [13]   | `where(cond, x, y, keep_attrs=None)`                                                                    | compute      | `Dataset`/`DataArray`  |
|  [14]   | `date_range(start, end, periods, freq)`, `cftime_range(start, end, periods, freq)`, `infer_freq(index)` | time         | index or freq          |

## [4]-[IMPLEMENTATION_LAW]

[NAMING_TOPOLOGY]:
- An admitted `ndarray` is wrapped in a `DataArray`/`Dataset` carrying study dimension names and coordinate labels; the axis record is consumed by study plans.
- Label-based `sel`/`isel` address study axes by name; free-dimension evidence is captured from the dimension set rather than positional indices.
- `chunk` opts an array into the dask-backed lazy path; `compute`/`load`/`persist` materialise it, and `unify_chunks`/`map_blocks` operate over the chunked graph.
- `apply_ufunc` is the boundary for arbitrary NumPy-style functions over labelled arrays; `input_core_dims`/`output_core_dims` declare the broadcasting contract.
- IO flows through `open_dataset`/`open_zarr`/`open_datatree` at the boundary; named axes are study evidence, not wire vocabulary.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `xarray`
- Owns: labelled named-axis arrays, coordinate indexes, hierarchical `DataTree`, and free-dimension evidence for the array rail
- Accept: an admitted array wrapped with study dimension names and coordinate labels, label selection via `sel`/`isel`, the dask path via `chunk`
- Reject: positional-only axis handling where names exist, wrapper-renames of label selection, and treating axes as wire vocabulary
