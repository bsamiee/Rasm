# [PY_DATA_API_CUBED]

`cubed` mints lazy Array-API n-dimensional arrays whose every operation appends to a deferred task graph that runs only when `compute`/`store`/`to_zarr` fires a pluggable executor. Each `Array` backs onto Zarr storage under a `Spec` declaring the work directory, a hard per-task memory budget (`allowed_mem`), and the executor, so out-of-core and distributed workloads run at provable peak memory. It owns the chunked-compute rail the data owner folds `Array` + `Spec` + one graph-boundary `compute` into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cubed`
- package: `cubed`
- module: `cubed`; `cubed.array_api.linalg` (linalg), `cubed.runtime.executors` (executor backends), `cubed.random` (chunked RNG)
- owner: `data`
- rail: chunked-compute
- entry points: library use is import-only; no console script
- capability: lazy chunked Array-API arrays, Zarr-backed bounded-memory execution, pluggable local/distributed executors, out-of-core TSQR/SVD/QR linalg, blockwise `map_blocks`/`map_overlap`/`apply_gufunc` kernels, NaN-aware reductions, chunked random generation, and callback-driven memory telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core, runtime, and telemetry types (`cubed`)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                                       |
| :-----: | :------------------- | :--------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `cubed.Array`        | lazy array       | deferred chunked array carrying the full Array API surface                         |
|  [02]   | `cubed.Spec`         | execution spec   | work_dir, intermediate_store, allowed_mem, reserved_mem, executor, zarr_compressor |
|  [03]   | `cubed.config`       | config singleton | donfig `Config` runtime configuration (`config.set`/`config.get`)                  |
|  [04]   | `cubed.plan`         | graph finalizer  | `plan(*arrays) -> FinalizedPlan` for task-graph inspection                         |
|  [05]   | `cubed.Callback`     | event observer   | base for compute/operation/task callbacks                                          |
|  [06]   | `cubed.TaskEndEvent` | event payload    | per-task completion event carrying timing and peak-memory fields                   |

[PUBLIC_TYPE_SCOPE]: `cubed.Array` members

`spec` reads the resolved `Spec` (`work_dir`/`allowed_mem`/`reserved_mem`/`executor_name`) off a materialized array rather than re-passing budget and executor.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `compute(*, executor, callbacks, optimize_graph, resume, ...)` | instance | materialize this array |
|  [02]   | `rechunk(chunks, *, min_mem, allow_irregular)`                 | instance | change chunk layout    |
|  [03]   | `visualize(filename, format, optimize_graph, ...)`             | instance | render the task graph  |

[ARRAY_PROPERTIES]: `chunks` `chunksize` `npartitions` `blocks[selection]` `spec`

[CALLBACK_TELEMETRY]:
- `Callback` subclasses observe `on_compute_start`/`on_compute_end`, `on_operation_start`/`on_operation_end`, and `on_task_end(TaskEndEvent)`.
- `TaskEndEvent` carries `name`, `num_tasks`, `result`, `task_create_tstamp`, `function_start_tstamp`, `function_end_tstamp`, `task_result_tstamp`, `peak_measured_mem_start`, `peak_measured_mem_end` — the per-operation timing and measured peak-memory receipt fields.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation (`cubed`)

Array-API creation factories; every factory accepts `chunks=` and `spec=` to bind the chunk layout and execution spec at construction.

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `asarray(obj, /, *, dtype, device, copy, chunks, spec) -> Array`               | any array-like to cubed Array |
|  [02]   | `from_array(x, chunks='auto', asarray, spec) -> Array`                         | existing array-like to cubed  |
|  [03]   | `from_zarr(store, path, spec) -> Array`                                        | Zarr store to cubed Array     |
|  [04]   | `zeros` / `ones` / `empty` / `full(shape, fill_value, *, dtype, chunks, spec)` | filled lazy arrays            |
|  [05]   | `zeros_like` / `ones_like` / `empty_like` / `full_like(x, /, ...)`             | shape-matched filled arrays   |
|  [06]   | `arange(start, /, stop, step, *, dtype, chunks, spec) -> Array`                | range array                   |
|  [07]   | `eye(n_rows, n_cols, /, *, k, dtype, chunks, spec) -> Array`                   | identity / diagonal array     |
|  [08]   | `linspace(start, stop, /, num, *, dtype, endpoint, chunks, spec) -> Array`     | linearly spaced array         |
|  [09]   | `meshgrid(*arrays, indexing='xy') -> list[Array]`                              | coordinate grids              |
|  [10]   | `tril(x, /, *, k)` / `triu(x, /, *, k)`                                        | lower/upper triangle          |
|  [11]   | `cubed.random.random(size, *, chunks, spec) -> Array`                          | chunked uniform RNG           |

[ENTRYPOINT_SCOPE]: execution, store, blockwise, and reduce (`cubed`)

| [INDEX] | [SURFACE]                                                                                  | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `compute(*arrays, executor, callbacks, optimize_graph, optimize_function, resume, ...)`    | materialize arrays         |
|  [02]   | `store(sources, targets, executor, ...)`                                                   | write arrays to Zarr       |
|  [03]   | `to_zarr(x, store, path, executor, ...)`                                                   | write array to Zarr        |
|  [04]   | `visualize(*arrays, filename='cubed', format, optimize_graph, ...)`                        | render task graph          |
|  [05]   | `measure_reserved_mem(executor, work_dir, ...)`                                            | calibrate overhead         |
|  [06]   | `raise_if_computes()`                                                                      | assert no eager compute    |
|  [07]   | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, spec) -> Array`               | apply func per chunk       |
|  [08]   | `map_overlap(func, *args, depth, boundary, trim, ...) -> Array`                            | apply func over halos      |
|  [09]   | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | generalized ufunc          |
|  [10]   | `rechunk(x, chunks, *, min_mem)`                                                           | change chunk specification |
|  [11]   | `concat(arrays, /, *, axis)` / `stack(arrays, /, *, axis)`                                 | concatenate or stack       |
|  [12]   | `reshape(x, /, shape)` / `broadcast_to(x, /, shape, *, chunks)`                            | reshape or broadcast       |
|  [13]   | `where(condition, x1, x2, /)` / `pad(x, pad_width, mode, ...)`                             | conditional select or pad  |
|  [14]   | `nanmean(x, *, axis, ...)` / `nansum(x, *, axis, ...)`                                     | NaN-aware reductions       |

[ENTRYPOINT_SCOPE]: linear algebra (`cubed.array_api.linalg`), all over `cubed.Array`

| [INDEX] | [SURFACE]                          | [CAPABILITY]                              |
| :-----: | :--------------------------------- | :---------------------------------------- |
|  [01]   | `matmul(x1, x2)`                   | matrix multiplication                     |
|  [02]   | `svd(x)`                           | singular value decomposition (TSQR-based) |
|  [03]   | `qr(x)`                            | QR decomposition (TSQR)                   |
|  [04]   | `svdvals(x)`                       | singular values only                      |
|  [05]   | `tensordot(x1, x2, axes)`          | generalized tensor contraction            |
|  [06]   | `outer(x1, x2)` / `vecdot(x1, x2)` | outer product or vector dot               |
|  [07]   | `matrix_transpose(x)`              | swap last two axes                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every operation appends to a task graph; nothing runs until `compute`/`store`/`to_zarr` fires the executor, and `raise_if_computes` guards a region as lazy.
- `Spec` sets the execution context: `allowed_mem` is the hard per-task peak-memory budget, `reserved_mem` the Python-overhead headroom subtracted from it, `intermediate_store`/`storage_options` route scratch Zarr writes, and `zarr_compressor` sets the codec.
- `executor_name` selects the backend — `single-threaded` (synchronous default), `threads`/`processes` (local), `dask`/`lithops`/`modal`/`coiled`/`ray`/`spark` (distributed), `raise-if-computes` (guard) — and `executor_options` passes backend kwargs.
- `Array` implements the Python Array API, so NumPy/CuPy/xarray interop crosses through `asarray` and `__array_namespace__`; intermediate and output arrays write Zarr under `Spec.work_dir`.
- `rechunk(min_mem=)` realigns chunk boundaries without recomputing values; `map_overlap(depth=, boundary=)` is the haloed-block primitive for stencil kernels `map_blocks` alone cannot express.
- Memory budget and `TaskEndEvent` peak-memory and timing fields form the chunked-compute receipt; `measure_reserved_mem` calibrates `reserved_mem` per executor before a budgeted run.

[STACKING]:
- `zarr`(`.api/zarr.md`): `from_zarr`/`to_zarr` and `Spec.intermediate_store` read and write Zarr stores.
- `icechunk`(`.api/icechunk.md`): back the spec on an icechunk session for transactional versioned chunked output.
- `tensorstore`(`.api/tensorstore.md`): back the spec on a tensorstore KV-store for the same Zarr layout under a high-throughput async store.
- `virtualizarr`(`.api/virtualizarr.md`): compute over a virtual Zarr scanned across existing files without rewrite.
- `xarray`(`.api/xarray.md`): wrap an `Array` as an `xarray.DataArray` backend (`chunked_array_type='cubed'`) for labelled-dimension bounded-memory compute; `xarray-spatial`/`rioxarray` raster kernels then run out-of-core.
- `flox`(`.api/flox.md`): grouped/segmented reductions route through flox's chunk-aware group-by so a chunked group reduction stays inside the budget.
- within-lib: the data owner folds `Array` + `Spec` + one graph-boundary `compute` into the chunked-compute path; `dask` rides as one distributed executor via `executor_name='dask'`, never a competing array type.

[LOCAL_ADMISSION]:
- Array operations compose as a lazy graph under one `Spec` naming `work_dir`, `allowed_mem`, and executor; `compute` fires once at the graph boundary.
- Cubed execution is offline study evidence; the materialized result crosses as a Zarr store or an Arrow/xarray frame, and production substrate selection stays in the C# compute owner.

[RAIL_LAW]:
- Package: `cubed`
- Owns: lazy chunked Array-API arrays, Zarr-backed bounded-memory execution, pluggable local/distributed executors, out-of-core TSQR linalg, blockwise/haloed/gufunc kernels, NaN-aware reductions, chunked RNG, and callback-driven memory telemetry
- Accept: array operations composed as a lazy graph under a `Spec`, computed once at the graph boundary, reading and writing Zarr through the store family and backing an xarray labelled array via the Array API
- Reject: eager full-materialization where the lazy graph applies, an in-memory NumPy pass over an out-of-core payload, a hand-rolled chunk-iteration loop, and re-expressing a cubed graph as a raw `dask.array` instead of selecting dask as a cubed executor
