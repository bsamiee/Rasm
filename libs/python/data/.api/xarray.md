# [PY_DATA_API_XARRAY]

`xarray` supplies `DataArray`, `Dataset`, `Variable`, `Coordinates`, and `DataTree` labelled named-axis containers with label-based selection, grouping, reduction, interpolation, dask-backed chunking, and netCDF/Zarr IO. It is the canonical owner surface for the `field-dataset` CF labelled-field dataset: the `FieldDataset` owner addresses CF coordinates by dimension name through `sel`/`isel`, reads and writes CF field cubes through `open_dataset(engine=)`/`open_zarr`/`to_netcdf`/`to_zarr` over the netcdf4/h5netcdf/zarr engines, and `chunk` opts a cube into the lazy dask path. xarray is on `banned-module-level-imports`, so every `FieldDataset` body binds it function-local under `# noqa: PLC0415`; it is not a `<3.15` gated dist (source-build/pure-Python, cp315-available), so there is no subprocess seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray`
- import: `import xarray as xr`
- owner: `data`
- rail: field-dataset
- capability: CF-conventioned labelled n-dimensional field cubes — named dimensions, coordinate indexes, CF-aware label selection, grouped and resampled reductions, interpolation, hierarchical `DataTree`, netCDF/Zarr IO over the netcdf4/h5netcdf/zarr engines, and a dask-backed lazy/chunked path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: labelled-array owners
- rail: field-dataset

| [INDEX] | [SYMBOL]             | [ROLE]              | [CAPABILITY]                            |
| :-----: | :------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `xarray.DataArray`   | labelled array      | array plus named dims and coords        |
|  [02]   | `xarray.Dataset`     | labelled collection | named DataArrays sharing dimensions     |
|  [03]   | `xarray.Variable`    | dim-aware buffer    | array with attached dimension names     |
|  [04]   | `xarray.Coordinates` | coordinate index    | label index set for dimensions          |
|  [05]   | `xarray.DataTree`    | hierarchical tree   | nested Datasets with parent/child links |
|  [06]   | `xarray.Index`       | index protocol      | custom coordinate index backend         |

[PUBLIC_TYPE_SCOPE]: shared array members
- rail: field-dataset
- members carry across `DataArray` and `Dataset` except where noted

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                  |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | selection       | `sel`, `isel`, `loc`, `drop_sel`, `drop_isel`, `head`, `tail`, `thin`, `where`                             |
|  [02]   | structure       | `dims`, `coords`, `sizes`, `attrs`, `assign_coords`, `assign_attrs`, `rename`, `swap_dims`, `set_index`    |
|  [03]   | reshape         | `stack`, `unstack`, `transpose`, `expand_dims`, `squeeze`, `broadcast_like`, `pad`, `shift`, `roll`        |
|  [04]   | grouping        | `groupby`, `groupby_bins`, `resample`, `rolling`, `coarsen`, `weighted`                                    |
|  [05]   | reduction       | `mean`, `sum`, `std`, `var`, `min`, `max`, `median`, `quantile`, `count`, `prod`, `cumsum`                 |
|  [06]   | alignment       | `reindex`, `reindex_like`, `interp`, `interp_like`, `interpolate_na`, `ffill`, `bfill`, `fillna`, `dropna` |
|  [07]   | chunking        | `chunk`, `chunks`, `chunksizes`, `compute`, `persist`, `load`, `unify_chunks`, `map_blocks`                |
|  [08]   | egress          | `to_netcdf`, `to_zarr`, `to_pandas`, `to_dataframe`, `to_numpy`, `to_dict`, `to_dataset` (DataArray)       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IO and construction
- rail: field-dataset

| [INDEX] | [SURFACE]                                                          | [FAMILY]     | [RETURNS]   |
| :-----: | :----------------------------------------------------------------- | :----------- | :---------- |
|  [01]   | `open_dataset(filename_or_obj, *, engine=None, chunks=None)`       | IO           | `Dataset`   |
|  [02]   | `open_dataarray(filename_or_obj, *, engine=None, chunks=None)`     | IO           | `DataArray` |
|  [03]   | `open_mfdataset(paths, chunks=None, ...)`                          | IO           | `Dataset`   |
|  [04]   | `open_zarr(store, group=None, chunks=..., decode_cf=True)`         | IO           | `Dataset`   |
|  [05]   | `open_datatree(filename_or_obj, *, engine=None)`                   | IO           | `DataTree`  |
|  [06]   | `load_dataset(filename_or_obj)`, `load_dataarray(filename_or_obj)` | IO           | eager value |
|  [07]   | `save_mfdataset(datasets, paths, mode='w')`                        | IO           | `None`      |
|  [08]   | `decode_cf(obj, decode_times=True, mask_and_scale=True)`           | construction | `Dataset`   |

[ENTRYPOINT_SCOPE]: combination and computation
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                               | [FAMILY]     | [RETURNS]              |
| :-----: | :------------------------------------------------------------------------------------------------------ | :----------- | :--------------------- |
|  [01]   | `concat(objs, dim, data_vars=all, coords=...)`                                                          | combine      | `Dataset`/`DataArray`  |
|  [02]   | `merge(objects, compat=..., join=...)`                                                                  | combine      | `Dataset`              |
|  [03]   | `combine_by_coords(data_objects=[], ...)`                                                               | combine      | `Dataset`/`DataArray`  |
|  [04]   | `combine_nested(datasets, concat_dim)`                                                                  | combine      | `Dataset`/`DataTree`   |
|  [05]   | `align(*objects, join='inner', copy=True)`                                                              | align        | aligned tuple          |
|  [06]   | `broadcast(*args, exclude=None)`                                                                        | align        | broadcast tuple        |
|  [07]   | `apply_ufunc(func, *args, input_core_dims=None, output_core_dims=None)`                                 | compute      | wrapped result         |
|  [08]   | `dot(*arrays, dim=None)`, `cross(a, b, *, dim)`                                                         | compute      | `DataArray`/`Variable` |
|  [09]   | `corr(da_a, da_b, dim=None, weights=None)`, `cov(da_a, da_b, dim=None, ddof=1)`                         | compute      | `DataArray`            |
|  [10]   | `polyval(coord, coeffs, degree_dim='degree')`                                                           | compute      | `Dataset`/`DataArray`  |
|  [11]   | `map_blocks(func, obj, args=(), kwargs=None)`                                                           | dask         | `Dataset`/`DataArray`  |
|  [12]   | `full_like(other, fill_value, ...)`, `ones_like(other, ...)`, `zeros_like(other, ...)`                  | construction | like-shaped array      |
|  [13]   | `where(cond, x, y, keep_attrs=None)`                                                                    | compute      | `Dataset`/`DataArray`  |
|  [14]   | `date_range(start, end, periods, freq)`, `cftime_range(start, end, periods, freq)`, `infer_freq(index)` | time         | index or freq          |

## [04]-[IMPLEMENTATION_LAW]

[NAMING_TOPOLOGY]:
- A CF field cube is a `Dataset` of `DataArray`s carrying CF dimension names, coordinate labels, and CF metadata (`units`/`standard_name`/`grid_mapping`); `open_dataset(engine=)`/`open_mfdataset`/`open_zarr` materialise it, `decode_cf` applies the CF mask/scale/time decode, and the `FieldDataset` owner keys the cube by one runtime `ContentIdentity`.
- Label-based `sel`/`isel` address CF coordinates by name (`FieldSelection.Sel`/`Isel`); `interp` is the CF-aware interpolation arm and `groupby`/`groupby_bins`/`resample` the grouped/binned/resampled reductions, each a `FieldSelection` case rather than a sibling method.
- `chunk` opts a cube into the dask-backed lazy path; `compute`/`load`/`persist` materialise it, and `unify_chunks`/`map_blocks` operate over the chunked graph.
- `apply_ufunc` is the boundary for arbitrary NumPy-style functions over labelled cubes; `input_core_dims`/`output_core_dims` declare the broadcasting contract.
- IO flows through `open_dataset`/`open_zarr`/`open_datatree` at the boundary and `to_netcdf`/`to_zarr` at egress; the `FieldDataset` egress materialises to the same content-keyed `pyarrow`/Zarr surface the `tensor`/`columnar` owners speak.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `xarray`
- Owns: the CF labelled-field cube for the `field-dataset` owner — labelled named-axis arrays, coordinate indexes, CF-aware selection/grouping/resampling/interpolation, netCDF/Zarr IO over netcdf4/h5netcdf/zarr, hierarchical `DataTree`, and the dask lazy path
- Accept: `open_dataset(engine=)`/`open_zarr` reads and `to_netcdf`/`to_zarr` writes bound function-local under `# noqa: PLC0415`, `decode_cf` for CF metadata, `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample` as `FieldSelection` cases, the dask path via `chunk`
- Reject: a module-level xarray import (banned), positional-only axis handling where CF names exist, wrapper-renames of label selection, a second labelled-array store inside `tensor`, and treating CF coordinates as wire vocabulary
