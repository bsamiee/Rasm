# [PY_DATA_API_FLOX]

`flox` supplies fast, vectorized, and parallel grouped reductions AND grouped scans for the `field-dataset` selection rail. The package owner rewrites `xarray` `groupby`/`groupby_bins`/`resample` reductions onto the numpy-groupies kernel for pure-NumPy arrays and onto map-reduce/blockwise/cohorts tree strategies for dask arrays, exposes `flox.xarray.xarray_reduce` as the one xarray-aware grouped-reduction entrypoint, surfaces `groupby_reduce`/`groupby_scan` as the raw-array kernels, and admits custom `Aggregation`/`Scan` escapes; it never re-implements the labelled-array container `xarray` owns. `xarray` defers to `flox` automatically when installed, so the package is the vectorized lowering beneath the `FieldSelection` grouped/binned/resampled/scanned arms.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `flox`
- package: `flox`
- import: `import flox; from flox.xarray import xarray_reduce; from flox import Aggregation, Scan, ReindexStrategy, set_options`
- owner: `data`
- rail: field-dataset
- installed: `0.11.2`
- capability: vectorized and parallel grouped/binned/resampled reductions AND grouped scans over NumPy and dask arrays — numpy-groupies kernel reductions, the `flox`/`numpy`/`numba`/`numbagg` engine kernels, map-reduce/blockwise/cohorts dask strategies, the `ReindexStrategy` combine-reindex policy, dask rechunk helpers (`rechunk_for_blockwise`/`rechunk_for_cohorts`), the global `set_options` context, the xarray-aware `xarray_reduce` entrypoint, and custom `Aggregation`/`Scan` escapes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reduction, scan, and reindex value types
- rail: field-dataset

`flox.__all__` is `('Aggregation', 'Scan', 'groupby_reduce', 'groupby_scan', 'rechunk_for_blockwise', 'rechunk_for_cohorts', 'set_options', 'ReindexStrategy', 'ReindexArrayType', 'is_supported_aggregation')`. `Aggregation`/`Scan` are the two custom-reduction escapes (reduce vs. cumulative), full constructors in `AGGREGATION_TOPOLOGY`; `ReindexStrategy`/`ReindexArrayType` parameterize the combine-step reindex (dense NumPy vs. sparse COO), replacing the old bare-`bool` `reindex` argument. Symbols are `flox.*`; `factorize_` is `flox.core.factorize_`.

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]   | [CAPABILITY]                                                                             |
| :-----: | :------------------------- | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Aggregation`              | custom reduction | tree reduction passed as the `func` argument (ctor in `AGGREGATION_TOPOLOGY`)            |
|  [02]   | `Scan`                     | custom scan      | user-defined grouped cumulative scan passed as the `func` argument to `groupby_scan`     |
|  [03]   | `ReindexStrategy`          | reindex policy   | `ReindexStrategy(blockwise, array_type=AUTO)`, accepted by `reindex` beside a `bool`     |
|  [04]   | `ReindexArrayType`         | reindex carrier  | `Enum`: `AUTO`, `NUMPY`, `SPARSE_COO` — dense vs. sparse-COO for high-cardinality groups |
|  [05]   | `is_supported_aggregation` | engine predicate | `(array, func, **kwargs) -> bool`: `func` admissible under resolved engine/method        |
|  [06]   | `set_options`              | global options   | `set_options(**kwargs)` context manager/setter (e.g. `expected_groups_validation`)       |
|  [07]   | `core.factorize_`          | group factorizer | `(by, axes, *, expected_groups=None, ...) -> tuple`: codes, `pd.Index` labels, counts    |

[PUBLIC_TYPE_SCOPE]: grouper value types
- rail: field-dataset

The resampling/binning groupers used on the `field-dataset` resample/bin arms are `xarray.groupers` objects (`TimeResampler(freq, closed, label, origin, offset)`, `BinGrouper`, `UniqueGrouper`), not `flox` exports; `xarray_reduce` accepts them positionally as `by` arguments. `flox` itself exports no `TimeResampler`, `Resampler`, or `Grouper` symbol, and `factorize_` lives in `flox.core`, not a `flox.grouping` module (no such module exists).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: xarray-aware grouped reduction
- rail: field-dataset

`flox.xarray.xarray_reduce` is the one xarray-aware entrypoint; `obj` is a `Dataset` or `DataArray`, each `*by` is a coordinate-name `Hashable`, a `DataArray`, or an `xarray.groupers` grouper. Every reduction row elides the shared kwargs — `func: str \| Aggregation`, `expected_groups=None`, `sort=True`, `fill_value=None`, `dtype=None`, `method=None`, `engine=None`, `reindex: ReindexStrategy \| bool \| None=None`, `min_count=None` (all in `REDUCTION_TOPOLOGY`) — and carries only the surface-unique parameters; `isbin` may be a `bool` or a per-`by` `Sequence[bool]`.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                                                       |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `xarray_reduce` | `(obj, *by, isbin=False, dim=None, keep_attrs=True, skipna=None, **finalize_kwargs) -> xr.Dataset \| xr.DataArray` |

[ENTRYPOINT_SCOPE]: core array reduction and scan
- rail: field-dataset

`flox.groupby_reduce` is the raw-array tree-reduction kernel (returns `(reduced, *group_labels)`) sharing the reduction kwargs elided above; `flox.groupby_scan` is the raw-array grouped cumulative-scan kernel (returns a single array of input shape) whose `func` is `str \| Scan` and which takes no `min_count`/`reindex`/`fill_value`/`sort` since it does not aggregate to one row per group.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                                                                     | [ENTRY_FAMILY] |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `groupby_reduce` | `(array, *by, isbin=False, axis=None, finalize_kwargs=None) -> tuple[Array, *tuple[Array, ...]]` | reduce         |
|  [02]   | `groupby_scan`   | `(array, *by, axis: int \| tuple[int]=-1) -> np.ndarray \| DaskArray`                            | scan           |

[ENTRYPOINT_SCOPE]: dask rechunk helpers and global options
- rail: field-dataset

`flox.rechunk_for_blockwise`/`flox.rechunk_for_cohorts` precondition a dask array's chunking so the `blockwise`/`cohorts` `method` strategies stay single-pass; `flox.set_options` is the global options context. These are dask-tuning and policy surfaces composed around the reduction, not separate reduction entrypoints. `rechunk_for_cohorts` also takes optional `chunksize: int \| None=None`, `ignore_old_chunks=False`, and `debug=False` tuning flags.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                   |
| :-----: | :---------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `rechunk_for_blockwise` | `(array: DaskArray, axis, labels: np.ndarray, *, force=True) -> tuple[method, DaskArray]`      |
|  [02]   | `rechunk_for_cohorts`   | `(array: DaskArray, axis, labels: np.ndarray, force_new_chunk_at: Sequence, ...) -> DaskArray` |
|  [03]   | `set_options`           | `(**kwargs)` global-options context manager/setter                                             |

## [04]-[IMPLEMENTATION_LAW]

[REDUCTION_TOPOLOGY]:
- `func` vocabulary (the registered `flox.aggregations` reduction set, by `.name`): `"all"`, `"any"`, `"count"`, `"sum"`, `"nansum"`, `"prod"`, `"nanprod"`, `"mean"`, `"nanmean"`, `"std"`, `"nanstd"`, `"var"`, `"nanvar"`, `"max"`, `"nanmax"`, `"min"`, `"nanmin"`, `"argmax"`, `"nanargmax"`, `"argmin"`, `"nanargmin"`, `"quantile"`, `"nanquantile"`, `"median"`, `"nanmedian"`, `"mode"`, `"nanmode"`, `"first"`, `"nanfirst"`, `"last"`, `"nanlast"`, or an `Aggregation`
- the nan-aware reductions pair a base name with its `nan`-prefixed form; `argmax`/`argmin` pair with `nanargmax`/`nanargmin`, never a bare `nanargmax` without the prefix
- `std`, `var`, and `prod` (and their `nan`-prefixed forms) ARE first-class `flox` `func` values registered in `flox.aggregations`; pass them as `xarray_reduce(func="std", ...)`/`groupby_reduce(..., func="var")` directly — `std`/`var` accept `ddof` through `finalize_kwargs`
- `scan` `func` vocabulary (the registered `flox.scan` SCANS set): `"cumsum"`, `"nancumsum"`, `"ffill"` (forward-fill), `"bfill"` (back-fill), or a `Scan` — `groupby_scan` is the cumulative counterpart to `groupby_reduce`, returning one value per input element rather than one per group
- `method` strategy values: `"map-reduce"` (tree reduction, the default), `"blockwise"` (no cross-block combine, for already-sorted groups — pair with `rechunk_for_blockwise`), `"cohorts"` (group-into-block cohort detection for spatially clustered groups — pair with `rechunk_for_cohorts`)
- `engine` kernel values: `"flox"` (the internal optimized kernel, the default), `"numpy"` (numpy-groupies `aggregate`), `"numba"` (numba-compiled, requires the `numba` extra), `"numbagg"` (numbagg kernels, requires the `numbagg` extra); `is_supported_aggregation(array, func)` gates whether a `func` is admissible for the resolved engine
- `isbin=True` reads each `expected_groups` entry as bin edges; `isbin=False` reads each entry as a simple group label; for multiple `by`, `isbin` may be a per-`by` `Sequence[bool]`
- `expected_groups` declares the group labels (or bin edges under `isbin`) so the reduction skips an eager group-discovery pass; declaring it is required for the lazy dask path to avoid computing the groups
- `flox` works on pure-NumPy arrays through numpy-groupies and on dask arrays through the `method` tree strategies; dask is an optional `[all]` extra, not required for the NumPy path
- `min_count` sets the minimum count of valid values for a non-fill group result; `fill_value` fills empty or below-`min_count` groups
- `sort=True` returns groups in sorted order; `reindex` is a `ReindexStrategy(blockwise, array_type)` (or a plain `bool`) controlling whether each block reindexes to the full `expected_groups` set at combine time and whether the intermediate is dense `NUMPY` or `SPARSE_COO` — sparse intermediates bound memory for high-cardinality groups
- `xarray_reduce(obj, *by, ...)` accepts each `by` as a coordinate name string, a `DataArray`, or an `xarray.groupers` grouper object (`TimeResampler`, `BinGrouper`, `UniqueGrouper`); multiple `by` arguments group over the cross-product

[AGGREGATION_TOPOLOGY]:
- `Aggregation(name, *, numpy, chunk, combine, preprocess, finalize, fill_value, final_fill_value, dtypes, final_dtype, reduction_type, new_dims_func, preserves_dtype)` builds a custom reduction: `numpy` is the pure-NumPy operation, `chunk` the blockwise dask reduction, `combine` the intermediate-combine step, `preprocess` an optional pre-pass, `finalize` the final-result projection, `fill_value` the reindex fill, `final_dtype` the output dtype, `reduction_type` is `"reduce"` or `"argreduce"` (argmax/argmin-shaped), and `new_dims_func` declares output dims for quantile-style ops
- the built-in `mean` aggregation is `chunk=("sum", "nanlen")`, `combine=("sum", "sum")`, `fill_value=(0, 0)`, `final_dtype=np.floating` — the two-pass sum-and-count pattern a custom mean-like reduction mirrors
- `Scan(name, binary_op, scan, reduction, identity, dtype, preserves_dtype, mode, preprocess, finalize)` builds a custom grouped scan: `binary_op` is the associative scan operator, `reduction` names the matching reduce (e.g. `cumsum` -> `sum`, `ffill`/`bfill` -> `nanlast`), `identity` seeds the scan, and `mode` selects the binary-op application strategy

[INTEGRATION]:
- `flox` is the vectorized lowering BENEATH the data owner's `FieldSelection` arms, not a parallel API: an `xarray` `Dataset`/`DataArray` built from a numpy/zarr/netCDF source calls `obj.groupby(...).reduce(...)`/`.resample(...)` and `xarray` dispatches to `groupby_reduce`/`xarray_reduce` automatically when `flox` is installed — the owner names the `func`/`method`/`engine`/`expected_groups` policy, never a hand-rolled group loop
- dask path: when the labelled array is dask-backed, `method`/`ReindexStrategy` are the parallelization policy and `rechunk_for_blockwise`/`rechunk_for_cohorts` precondition the chunking; the NumPy path uses numpy-groupies under `engine="flox"`/`"numpy"` with no dask dependency
- the grouped/binned arms pass `xarray.groupers` objects (`TimeResampler` for resample, `BinGrouper` for binning, `UniqueGrouper` for distinct labels) as `*by`; the cumulative arm (running totals, forward/back fill over a group) routes to `groupby_scan` with a scan `func`, which the reduce family cannot express

[RAIL_LAW]:
- Package: `flox`
- Owns: vectorized and parallel grouped/binned/resampled reductions AND grouped scans over NumPy and dask arrays, the numpy-groupies kernel dispatch, the `flox`/`numpy`/`numba`/`numbagg` engine kernels, the map-reduce/blockwise/cohorts dask strategies, the `ReindexStrategy` combine-reindex policy, the dask rechunk helpers, and the custom `Aggregation`/`Scan` escapes
- Accept: `xarray_reduce(obj, *by, func=, ...)` as the xarray-aware grouped-reduction entrypoint and `groupby_reduce`/`groupby_scan` as the raw-array kernels, with `func` drawn from the registered reduction/scan vocabulary, `expected_groups` declared for the lazy dask path, `method`/`engine`/`reindex` as parallelization and kernel policy, and `xarray.groupers` grouper objects passed as `by`
- Reject: claiming `std`/`var`/`prod` are not flox `func` values (they are registered aggregations); importing `TimeResampler` from `flox` or referencing a `flox.grouping` module (`factorize_` is in `flox.core`); a hand-rolled group-loop or cumulative-scan where `groupby_reduce`/`groupby_scan` vectorizes; a bare-`groupby` reduction where `flox` is installed and `xarray` defers to it
