# [PY_DATA_API_FLOX]

`flox` owns vectorized, parallelized grouped reductions and grouped cumulative scans over NumPy and dask arrays, lowering `xarray` groupby, binned, and resample reductions onto the numpy-groupies kernel and the map-reduce/blockwise/cohorts dask strategies. It never re-implements the labelled-array container `xarray` owns; `xarray` dispatches to it automatically when installed, so `flox` is the vectorized lowering beneath the `field-dataset` selection rail's grouped, binned, resampled, and scanned arms.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `flox`
- package: `flox`
- owner: `data`
- module: `flox`
- namespaces: `flox.xarray`, `flox.core`, `flox.aggregations`
- license: `Apache-2.0`
- rail: field-dataset

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: custom-reduction and reindex value types

`Aggregation` and `Scan` are the two custom-escape constructors, a tree reduction against a grouped cumulative scan, each passed as the `func` argument; `ReindexStrategy` and `ReindexArrayType` parameterize the combine-step reindex, dense `NUMPY` against `SPARSE_COO` for high-cardinality groups.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `Aggregation`      | class         | custom tree reduction passed as `func`; `reduction_type` `"reduce"`/`"argreduce"`    |
|  [02]   | `Scan`             | class         | custom grouped cumulative scan passed as `func`                                      |
|  [03]   | `ReindexStrategy`  | value-object  | `ReindexStrategy(blockwise, array_type=AUTO)`, accepted by `reindex` beside a `bool` |
|  [04]   | `ReindexArrayType` | enum          | `AUTO`, `NUMPY`, `SPARSE_COO` — dense against sparse-COO for high-cardinality groups |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grouped reduction and cumulative scan

Each reduction row elides the shared reduction kwargs — `- reduction carry: func, expected_groups=None, sort=True, fill_value=None, dtype=None, method=None, engine=None, reindex=None, min_count=None` — and carries only its surface-unique parameters; `groupby_reduce` returns `(reduced, *group_labels)` and `groupby_scan` returns one value per input element under a `str | Scan` func.

| [INDEX] | [SURFACE]                                                                                  | [SHAPE] | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `xarray_reduce(obj, *by, isbin=False, dim=None, keep_attrs=True, skipna=None, **finalize)` | static  | xarray-aware grouped reduction   |
|  [02]   | `groupby_reduce(array, *by, isbin=False, axis=None, finalize_kwargs=None)`                 | static  | raw-array tree-reduction kernel  |
|  [03]   | `groupby_scan(array, *by, axis=-1)`                                                        | static  | raw-array cumulative-scan kernel |

[ENTRYPOINT_SCOPE]: dask tuning, options, and group primitives

`flox.core.factorize_` returns codes, `pd.Index` labels, and group counts; the rechunk helpers keep the `blockwise`/`cohorts` strategies single-pass.

| [INDEX] | [SURFACE]                                                                               | [SHAPE] | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `rechunk_for_blockwise(array, axis, labels, *, force=True) -> tuple[method, DaskArray]` | static  | chunk precondition for `blockwise` |
|  [02]   | `rechunk_for_cohorts(array, axis, labels, force_new_chunk_at, ...) -> DaskArray`        | static  | chunk precondition for `cohorts`   |
|  [03]   | `set_options(**kwargs)`                                                                 | ctor    | global-options context manager     |
|  [04]   | `is_supported_aggregation(array, func, **kwargs) -> bool`                               | static  | engine-admissibility predicate     |
|  [05]   | `core.factorize_(by, axes, *, expected_groups=None, sort=True) -> tuple`                | static  | group factorizer                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `flox` reduces pure-NumPy arrays through numpy-groupies and dask arrays through the `method` tree strategies; dask is an optional `[all]` extra, not the NumPy path.
- reduction `func` (registered `flox.aggregations` names): `[REDUCE]: `"all"` `"any"` `"count"` `"sum"` `"nansum"` `"prod"` `"nanprod"` `"mean"` `"nanmean"` `"std"` `"nanstd"` `"var"` `"nanvar"` `"max"` `"nanmax"` `"min"` `"nanmin"` `"argmax"` `"nanargmax"` `"argmin"` `"nanargmin"` `"quantile"` `"nanquantile"` `"median"` `"nanmedian"` `"mode"` `"nanmode"` `"first"` `"nanfirst"` `"last"` `"nanlast"``, an `Aggregation`, or a base name paired with its `nan`-prefixed form.
- `std` and `var` take `ddof` through `finalize_kwargs`.
- scan `func` (registered `flox.aggregations` SCANS): `[SCAN]: `"cumsum"` `"nancumsum"` `"ffill"` `"bfill"`` or a `Scan`.
- `method`: `"map-reduce"` tree reduction (default); `"blockwise"` skips the cross-block combine for already-sorted groups, paired with `rechunk_for_blockwise`; `"cohorts"` detects group-into-block cohorts for spatially clustered groups, paired with `rechunk_for_cohorts`.
- `engine`: `"flox"` internal optimized kernel (default); `"numpy"` numpy-groupies `aggregate`; `"numba"` requires the `numba` extra; `"numbagg"` requires the `numbagg` extra — `is_supported_aggregation(array, func)` gates admissibility for the resolved engine.
- `isbin=True` reads each `expected_groups` entry as bin edges, `isbin=False` as a group label, and a per-`by` `Sequence[bool]` mixes the two across multiple `by`.
- `expected_groups` declares the group labels or bin edges so the lazy dask path skips the eager group-discovery pass rather than computing the groups.
- `min_count` sets the minimum valid-value count for a non-fill result and `fill_value` fills empty or below-`min_count` groups; `sort=True` returns groups sorted.
- `reindex` is a `ReindexStrategy(blockwise, array_type)` or a `bool` controlling per-block reindex to the full `expected_groups` at combine time and the dense-`NUMPY`-against-`SPARSE_COO` intermediate, sparse bounding memory for high-cardinality groups.
- a custom `Aggregation` supplies `numpy` (pure-NumPy op), `chunk` (blockwise dask reduction), `combine` (intermediate merge), `finalize` (result projection), `fill_value`, `final_dtype`, and `reduction_type`; built-in `mean` is `chunk=("sum", "nanlen")`, `combine=("sum", "sum")`, `final_dtype=np.floating`, the two-pass sum-and-count a mean-like reduction mirrors.
- a custom `Scan` supplies `binary_op` (associative operator), `scan`, `reduction` (the matching reduce: `cumsum`→`sum`, `ffill`/`bfill`→`nanlast`), `identity`, and `mode`.

[STACKING]:
- `xarray`(`libs/python/.api/xarray.md`): an `xarray` `Dataset`/`DataArray` calls `obj.groupby(...).reduce(...)`/`.resample(...)` and `xarray` dispatches to `groupby_reduce`/`xarray_reduce` when `flox` is installed; `xarray.groupers` objects (`TimeResampler`, `BinGrouper`, `UniqueGrouper`) pass positionally as `*by`.
- `numpy`(`libs/python/.api/numpy.md`): the raw-array kernels reduce a `numpy.ndarray` through numpy-groupies under `engine="flox"`/`"numpy"` with no dask dependency.
- `data:gridded/field`: `FieldSelection` names the `func`/`method`/`engine`/`expected_groups` policy, routes its grouped, binned, and resampled arms to `xarray_reduce` and its cumulative arm to `groupby_scan`, and on the dask arm sets `method`/`ReindexStrategy` and preconditions chunks through `rechunk_for_blockwise`/`rechunk_for_cohorts`.

[LOCAL_ADMISSION]:
- A grouped, binned, or resampled reduction over a `field-dataset` frame enters through `xarray_reduce` or `xarray`'s auto-dispatch, naming its `func` from the registered vocabulary and declaring `expected_groups` for the lazy dask path.
- A running total or forward/back fill over a group enters through `groupby_scan` with a scan `func`, the arm the reduce family cannot express.
- Resample, bin, and distinct groupers enter as `xarray.groupers` objects passed as `*by`; `flox` exports none of them, and `factorize_` is reached at `flox.core`.

[RAIL_LAW]:
- Package: `flox`
- Owns: vectorized, parallel grouped, binned, and resampled reductions and grouped cumulative scans over NumPy and dask arrays, the engine-kernel and dask-strategy dispatch, and the custom `Aggregation`/`Scan` escapes
- Accept: `xarray_reduce(obj, *by, func=, ...)` as the xarray-aware grouped-reduction entrypoint and `groupby_reduce`/`groupby_scan` as the raw-array kernels, `func` from the registered reduction/scan vocabulary, `expected_groups` declared for the lazy dask path, `method`/`engine`/`reindex` as parallelization and kernel policy, and `xarray.groupers` objects as `by`
- Reject: a hand-rolled group loop or cumulative scan where `groupby_reduce`/`groupby_scan` vectorizes, and a bare `xarray` `groupby` reduction where `flox` is installed and `xarray` defers to it
