# [PY_DATA_API_CUBED]

`cubed` is the bounded-memory chunked-compute rail: a lazy, Array-API-compliant n-dimensional array whose every operation appends to a deferred task graph that does not run until `compute`/`store`/`to_zarr` triggers a pluggable executor. Each `cubed.Array` is backed by Zarr storage under a `Spec` that declares the work directory, a hard per-task memory budget (`allowed_mem`), and the executor; the scheduler guarantees no task exceeds the budget, so out-of-core and distributed workloads run with provable peak memory. The data package owner composes `cubed.Array` + `Spec` + a single graph-boundary `compute` into the chunked-compute path, stacking on Zarr/Icechunk/TensorStore stores and the xarray labelled-array layer; it never hand-rolls a chunk iteration loop, eager NumPy materialization for out-of-core payloads, or a parallel chunked engine cubed already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cubed`
- package: `cubed`
- module: `cubed`; submodules `cubed.array_api.linalg` (linalg), `cubed.runtime.executors` (executor backends), `cubed.random` (chunked RNG)
- owner: `data`
- rail: chunked-compute
- asset: pure Python; license Apache-2.0; no native extension, no CPython floor (rides the cp315 core)
- entry points: library use is import-only; no console script
- capability: lazy chunked Array-API n-dimensional arrays, Zarr-backed bounded-memory execution, pluggable local/distributed executors, out-of-core TSQR/SVD linalg, full Array-API creation/elementwise/reduction/manipulation surface, blockwise `map_blocks`/`map_overlap`/`apply_gufunc`, NaN-aware reduction family, chunked random generation, callback-driven memory telemetry, and graph visualization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core, runtime, and config singletons (`cubed`)
- rail: chunked-compute

`config` and `plan` are module-level singletons, not classes: `config` is the donfig-backed runtime configuration manager (`config.set(...)`, `config.get(...)`), and `plan` exposes graph-plan inspection. `Spec` is the per-array execution declaration; `Callback`/`TaskEndEvent` drive telemetry.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [ROLE]                                                          |
| :-----: | :------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `cubed.Array`        | lazy array       | deferred chunked array with full Array API surface             |
|  [02]   | `cubed.Spec`         | execution spec   | work_dir, intermediate_store, allowed_mem, reserved_mem, executor, zarr_compressor |
|  [03]   | `cubed.config`       | runtime config   | donfig-backed configuration singleton (`config.set`/`config.get`) |
|  [04]   | `cubed.plan`         | plan inspection  | graph-plan inspection singleton                                |
|  [05]   | `cubed.Callback`     | event observer   | base for compute/operation/task callbacks                      |
|  [06]   | `cubed.TaskEndEvent` | event payload    | per-task completion event with timing + peak-memory fields     |

[PUBLIC_TYPE_SCOPE]: `cubed.Array` members
- rail: chunked-compute

| [INDEX] | [MEMBER]                                                       | [KIND]   | [ROLE]                    |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `compute(*, executor, callbacks, optimize_graph, resume, ...)` | method   | materialize this array    |
|  [02]   | `rechunk(chunks, *, min_mem)`                                  | method   | change chunk layout       |
|  [03]   | `visualize(filename, format, optimize_graph, ...)`             | method   | render the task graph     |
|  [04]   | `chunks`                                                       | property | per-axis chunk tuple      |
|  [05]   | `chunksize`                                                    | property | single-chunk shape        |
|  [06]   | `npartitions`                                                  | property | total chunk count         |
|  [07]   | `blocks[selection]`                                            | property | block-level indexing view |

[CALLBACK_TELEMETRY]:
- `Callback` subclasses observe `on_compute_start(ComputeStartEvent)` / `on_compute_end(ComputeEndEvent)`, `on_operation_start(OperationStartEvent)` / `on_operation_end(OperationEndEvent)`, and `on_task_end(TaskEndEvent)`.
- `TaskEndEvent` carries `name`, `num_tasks`, `result`, `task_create_tstamp`, `function_start_tstamp`, `function_end_tstamp`, `task_result_tstamp`, `peak_measured_mem_start`, `peak_measured_mem_end` — these are the chunked-compute receipt fields (per-operation timing and measured peak memory per task).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation (`cubed`)
- rail: chunked-compute

Array-API creation factories; every factory accepts `chunks=` and `spec=` to bind the chunk layout and execution spec at construction.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `asarray(obj, /, *, dtype, device, copy, chunks, spec) -> Array`           | ingest         | any array-like to cubed Array |
|  [02]   | `from_array(x, chunks='auto', asarray, spec) -> Array`                     | ingest         | existing array-like to cubed  |
|  [03]   | `from_zarr(store, path, spec) -> Array`                                    | ingest         | Zarr store to cubed Array     |
|  [04]   | `zeros` / `ones` / `empty` / `full(shape, fill_value, *, dtype, chunks, spec)` | create     | filled lazy arrays            |
|  [05]   | `zeros_like` / `ones_like` / `empty_like` / `full_like(x, /, ...)`         | create         | shape-matched filled arrays   |
|  [06]   | `arange(start, /, stop, step, *, dtype, chunks, spec) -> Array`            | create         | range array                   |
|  [07]   | `eye(n_rows, n_cols, /, *, k, dtype, chunks, spec) -> Array`               | create         | identity / diagonal array     |
|  [08]   | `linspace(start, stop, /, num, *, dtype, endpoint, chunks, spec) -> Array` | create         | linearly spaced array         |
|  [09]   | `meshgrid(*arrays, indexing='xy') -> list[Array]`                          | create         | coordinate grids              |
|  [10]   | `tril(x, /, *, k)` / `triu(x, /, *, k)`                                    | create         | lower/upper triangle          |
|  [11]   | `cubed.random.random(size, *, chunks, spec) -> Array`                      | create         | chunked uniform RNG           |

[ENTRYPOINT_SCOPE]: execution, store, blockwise, and reduce (`cubed`)
- rail: chunked-compute

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `compute(*arrays, executor, callbacks, optimize_graph, optimize_function, resume, ...)`    | execute        | materialize one or more arrays |
|  [02]   | `store(sources, targets, executor, ...)`                                                   | persist        | write arrays to Zarr targets   |
|  [03]   | `to_zarr(x, store, path, executor, ...)`                                                   | persist        | write array to Zarr            |
|  [04]   | `visualize(*arrays, filename='cubed', format, optimize_graph, ...)`                        | inspect        | render task graph diagram      |
|  [05]   | `measure_reserved_mem(executor, work_dir, ...)`                                            | inspect        | calibrate executor overhead    |
|  [06]   | `raise_if_computes()`                                                                       | inspect        | assert no eager compute fires  |
|  [07]   | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, spec) -> Array`               | blockwise      | apply func per chunk           |
|  [08]   | `map_overlap(func, *args, depth, boundary, trim, ...) -> Array`                            | blockwise      | apply func over haloed blocks  |
|  [09]   | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | gufunc         | generalized ufunc              |
|  [10]   | `rechunk(x, chunks, *, min_mem)`                                                            | reshape        | change chunk specification     |
|  [11]   | `concat(arrays, /, *, axis)` / `stack(arrays, /, *, axis)`                                  | combine        | concatenate or stack arrays    |
|  [12]   | `reshape(x, /, shape)` / `broadcast_to(x, /, shape, *, chunks)`                            | reshape        | reshape or broadcast           |
|  [13]   | `where(condition, x1, x2, /)` / `pad(x, pad_width, mode, ...)`                             | transform      | conditional select or pad      |
|  [14]   | `nanmean` / `nansum` / `nanmax` / `nanmin` / `nanprod` / `nanstd` / `nanvar` / `nanmedian` | reduce         | NaN-aware reductions           |
|  [15]   | `nanargmax` / `nanargmin` / `nancumsum` / `nancumprod`                                      | reduce         | NaN-aware arg/cumulative ops   |

[ENTRYPOINT_SCOPE]: linear algebra (`cubed.array_api.linalg`)
- rail: chunked-compute
- family: linalg, all over `cubed.Array`

| [INDEX] | [SURFACE]                          | [OPERATION]                    |
| :-----: | :--------------------------------- | :----------------------------- |
|  [01]   | `matmul(x1, x2)`                   | matrix multiplication          |
|  [02]   | `svd(x)`                           | singular value decomposition (TSQR-based) |
|  [03]   | `qr(x)`                            | QR decomposition (TSQR)        |
|  [04]   | `svdvals(x)`                       | singular values only           |
|  [05]   | `tensordot(x1, x2, axes)`          | generalized tensor contraction |
|  [06]   | `outer(x1, x2)` / `vecdot(x1, x2)` | outer product or vector dot    |
|  [07]   | `matrix_transpose(x)`              | swap last two axes             |

## [04]-[IMPLEMENTATION_LAW]

[CHUNKED_TOPOLOGY]:
- namespace: `cubed` (top level + Array-API elementwise/dtype-introspection surface), `cubed.array_api.linalg` (linalg), `cubed.runtime.executors` (executors), `cubed.random` (chunked RNG).
- every operation appends to a task graph; no computation runs until `compute()`, `store()`, or `to_zarr()` triggers the executor. `raise_if_computes` is the test guard that asserts a region stays lazy.
- `Spec(work_dir, *, intermediate_store=None, allowed_mem=None, reserved_mem=0, executor=None, executor_name=None, executor_options=None, storage_options=None, zarr_compressor='auto')` declares the execution context: `allowed_mem` is the hard peak-memory budget per task, `reserved_mem` is the Python-overhead headroom subtracted from it, `intermediate_store`/`storage_options` route scratch Zarr writes, and `zarr_compressor` sets the codec.
- executors are selected by `executor_name`: `single-threaded` (default synchronous, debug-friendly), `threads` and `processes` (local concurrency), `dask`, `lithops`, `modal`, `coiled`, `ray`, `spark` (distributed), and `raise-if-computes` (the no-op guard executor). `executor_options` passes backend kwargs.
- intermediate and output arrays write to Zarr stores under `Spec.work_dir`; `cubed.Array` implements the Python Array API standard, so NumPy/CuPy/xarray interop flows through `asarray` and `__array_namespace__`.
- `rechunk(min_mem=...)` realigns chunk boundaries without recomputing values, letting cubed pick chunks inside a memory bound; `map_overlap(depth=, boundary=)` is the haloed-block primitive for stencil/neighborhood kernels that `map_blocks` cannot express alone.
- the memory budget plus the `TaskEndEvent` peak-memory and timing fields form the chunked-compute receipt; `measure_reserved_mem` calibrates `reserved_mem` per executor before a budgeted run.

## [05]-[INTEGRATION]

[STACKS_WITH]:
- zarr / icechunk / tensorstore: `from_zarr`/`to_zarr` and `Spec.intermediate_store` read and write Zarr stores; back the spec on an `icechunk` session for transactional versioned chunked output, or on a `tensorstore` KV-store for the same Zarr layout under a high-throughput async store. `virtualizarr` lets cubed scan a virtual Zarr over existing files without rewrite.
- xarray / xarray-spatial / rioxarray: cubed satisfies the Array API, so an `xarray.DataArray` can wrap a `cubed.Array` as its backend (`chunked_array_type='cubed'`), giving labelled-dimension bounded-memory compute; xarray-spatial/rioxarray raster kernels then run out-of-core through the cubed scheduler.
- dask: `executor_name='dask'` runs the cubed graph on a Dask cluster — cubed owns the bounded-memory chunk plan, dask is one distributed executor backend, not a competing array type; do not re-express a cubed graph as a raw `dask.array`.
- flox: grouped/segmented reductions over a `cubed.Array` route through flox's chunk-aware group-by so a chunked group reduction stays inside the memory budget rather than collapsing to an eager pass.
- numpy / cupy: the Array-API conformance makes `asarray` the single bridge from in-memory NumPy and the dtype-introspection surface (`finfo`/`iinfo`/`isdtype`/`result_type`/`astype`/`can_cast`) the portable type algebra; small fully-in-memory payloads stay NumPy, out-of-core payloads become a cubed graph.

[LOCAL_ADMISSION]:
- Array operations compose as a lazy graph under one `Spec` naming `work_dir`, `allowed_mem`, and executor; `compute` fires once at the graph boundary, never per intermediate.
- The memory budget (`allowed_mem`/`reserved_mem`) plus `TaskEndEvent` peak-memory facts are the chunked-compute receipt; `measure_reserved_mem` calibrates it per executor.
- Out-of-core and distributed runs select an executor through `Spec`; never hand-roll a chunk iteration loop, an eager NumPy materialization of an out-of-core payload, or a parallel chunked engine.
- Cubed execution is offline study evidence; production substrate selection stays in the C# compute owner, and the materialized result crosses as a Zarr store or an Arrow/xarray frame, never a live cubed graph handle.

## [06]-[RAIL_LAW]

- Package: `cubed`
- Owns: lazy chunked n-dimensional Array-API arrays, Zarr-backed bounded-memory execution, pluggable local/distributed executors, out-of-core TSQR linalg, blockwise/haloed/gufunc kernels, NaN-aware reductions, chunked RNG, and callback-driven memory telemetry
- Accept: array operations composed as a lazy graph under a `Spec` naming work_dir, allowed_mem, and executor, computed once at the graph boundary, reading/writing Zarr (zarr/icechunk/tensorstore/virtualizarr) and backing an xarray labelled array via the Array API
- Reject: eager full-materialization where the lazy graph applies, hand-rolled chunked execution loops cubed owns, in-memory NumPy for out-of-core payloads, and re-expressing a cubed graph as a raw dask.array instead of selecting dask as a cubed executor
