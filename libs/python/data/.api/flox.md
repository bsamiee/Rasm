# [PY_DATA_API_FLOX]

`flox` supplies fast, vectorized, and parallel grouped reductions for the `field-dataset` selection rail. The package owner rewrites `xarray` `groupby`/`groupby_bins`/`resample` reductions onto the numpy-groupies kernel for pure-NumPy arrays and onto map-reduce/blockwise/cohorts tree strategies for dask arrays, exposes `flox.xarray.xarray_reduce` as the one xarray-aware grouped-reduction entrypoint, and admits a custom `Aggregation` escape; it never re-implements the labelled-array container `xarray` owns. `xarray` defers to `flox` automatically when installed, so the package is the vectorized lowering beneath the `FieldSelection` grouped/binned/resampled arms.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `flox`
- package: `flox`
- import: `import flox; from flox.xarray import xarray_reduce; from flox import Aggregation`
- owner: `data`
- rail: field-dataset
- capability: vectorized and parallel grouped/binned/resampled reductions over NumPy and dask arrays — numpy-groupies kernel reductions, map-reduce/blockwise/cohorts dask strategies, the xarray-aware `xarray_reduce` entrypoint, and a custom `Aggregation` escape

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reduction value types
- rail: field-dataset

| [INDEX] | [SYMBOL]                                                                                      | [PACKAGE_ROLE]   | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `flox.Aggregation(name, *, numpy, chunk, combine, finalize, fill_value, dtypes, final_dtype)` | custom reduction | user-defined reduction passed as the `func` argument      |
|  [02]   | `flox.aggregations.Aggregation`                                                               | reduction class  | the `Aggregation` class re-exported at `flox.Aggregation` |

[PUBLIC_TYPE_SCOPE]: grouper value types
- rail: field-dataset

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]   | [CAPABILITY]                                                                         |
| :-----: | :------------------------- | :--------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `flox.grouping.factorize_` | group factorizer | factorize one or more `by` arrays into integer group codes and expected group labels |

The resampling grouper used on the `field-dataset` resample arm is `xarray.groupers.TimeResampler(freq, closed, label, origin, offset)`, an `xarray` grouper object, not a `flox` export; `xarray_reduce` accepts it positionally as a `by` argument. `flox` itself exports no `TimeResampler`, `Resampler`, or `Grouper` symbol.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: xarray-aware grouped reduction
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                               | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------ |
|  [01]   | `flox.xarray.xarray_reduce(obj, *by, func, expected_groups=None, isbin=False, sort=True, dim=None, fill_value=None, dtype=None, method=None, engine=None, keep_attrs=True, skipna=None, min_count=None, reindex=None, **finalize_kwargs) -> xr.Dataset` | reduce         | xarray grouped/binned/resampled |

[ENTRYPOINT_SCOPE]: core array reduction
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                                                                                                                            | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `flox.groupby_reduce(array, *by, func, expected_groups=None, sort=True, isbin=False, axis=None, fill_value=None, dtype=None, method=None, engine=None, reindex=None, finalize_kwargs=None) -> tuple` | reduce         | raw-array grouped reduce |

## [04]-[IMPLEMENTATION_LAW]

[REDUCTION_TOPOLOGY]:
- `func` vocabulary (the numpy-groupies-backed aggregation superset): `"all"`, `"any"`, `"count"`, `"sum"`, `"nansum"`, `"mean"`, `"nanmean"`, `"max"`, `"nanmax"`, `"min"`, `"nanmin"`, `"argmax"`, `"nanargmax"`, `"argmin"`, `"nanargmin"`, `"quantile"`, `"nanquantile"`, `"median"`, `"nanmedian"`, `"mode"`, `"nanmode"`, `"first"`, `"nanfirst"`, `"last"`, `"nanlast"`, or an `Aggregation`
- the nan-aware reductions pair a base name with its `nan`-prefixed form; `argmax`/`argmin` pair with `nanargmax`/`nanargmin`, never a bare `nanargmax` without the prefix
- `std`, `var`, and `prod` are not `flox` `func` values; the standard-deviation, variance, and product reductions resolve through the bare-`xarray` reduction family on the grouped object, never through `xarray_reduce(func=)`
- `method` strategy values: `"map-reduce"` (tree reduction, the default), `"blockwise"` (no cross-block combine, for already-sorted groups), `"cohorts"` (group-into-block cohort detection for spatially clustered groups)
- `engine` kernel values: `"flox"` (the internal optimized kernel, the default), `"numpy"` (numpy-groupies `aggregate`), `"numba"` (numba-compiled), `"numbagg"` (numbagg kernels)
- `isbin=True` reads each `expected_groups` entry as bin edges; `isbin=False` reads each entry as a simple group label
- `expected_groups` declares the group labels (or bin edges under `isbin`) so the reduction skips an eager group-discovery pass; declaring it is required for the lazy dask path to avoid computing the groups
- `flox` works on pure-NumPy arrays through numpy-groupies and on dask arrays through the `method` tree strategies; dask is not required
- `min_count` sets the minimum count of valid values for a non-fill group result; `fill_value` fills empty or below-`min_count` groups
- `sort=True` returns groups in sorted order; `reindex` controls whether each block reindexes to the full `expected_groups` set at combine time
- `xarray_reduce(obj, *by, ...)` accepts each `by` as a coordinate name string, a `DataArray`, or an `xarray.groupers` grouper object (`TimeResampler`, `BinGrouper`, `UniqueGrouper`); multiple `by` arguments group over the cross-product

[AGGREGATION_TOPOLOGY]:
- `Aggregation(name, *, numpy, chunk, combine, finalize, fill_value, dtypes, final_dtype)` builds a custom reduction: `numpy` is the pure-NumPy operation, `chunk` the blockwise dask reduction, `combine` the intermediate-combine step, `finalize` the final-result projection, `fill_value` the reindex fill, and `final_dtype` the output dtype
- the built-in `mean` aggregation is `chunk=("sum", "nanlen")`, `combine=("sum", "sum")`, `fill_value=(0, 0)`, `final_dtype=np.floating` — the two-pass sum-and-count pattern a custom mean-like reduction mirrors

[RAIL_LAW]:
- Package: `flox`
- Owns: vectorized and parallel grouped/binned/resampled reductions over NumPy and dask arrays, the numpy-groupies kernel dispatch, the map-reduce/blockwise/cohorts dask strategies, and the custom `Aggregation` escape
- Accept: `xarray_reduce(obj, *by, func=, ...)` as the xarray-aware grouped-reduction entrypoint with `func` drawn from the aggregation superset, `expected_groups` declared for the lazy dask path, `method`/`engine` as parallelization and kernel policy, and `xarray.groupers` grouper objects passed as `by`
- Reject: passing `std`/`var`/`prod` as `xarray_reduce(func=)`; importing `TimeResampler` from `flox`; hand-rolled group-loop reductions where `xarray_reduce` vectorizes; a bare-`groupby` reduction where `flox` is installed and `xarray` defers to it
