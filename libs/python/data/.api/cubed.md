# [PY_DATA_API_CUBED]

`cubed` supplies a lazy, chunked, Array API-compliant n-dimensional array backed by Zarr storage and a bounded-memory task graph for the data chunked-compute rail. `cubed.Array` builds a deferred graph from Array API operations, `cubed.Spec` declares the work directory, memory budget, and executor, and `compute`/`store`/`to_zarr` materialize against a Zarr store. The graph runs through a pluggable executor (`local`, `dask`, `lithops`, `modal`, `coiled`, `beam`, `ray`, `spark`) for out-of-core and distributed workloads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cubed`
- package: `cubed`
- module: `cubed`; submodules `cubed.array_api.linalg`, `cubed.runtime.executors`
- asset: pure Python
- owner: `data`
- rail: chunked-compute

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core and execution types
- rail: chunked-compute

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [ROLE]                                          |
| :-----: | :------------------- | :------------- | :---------------------------------------------- |
|   [1]   | `cubed.Array`        | lazy array     | deferred chunked array with Array API surface   |
|   [2]   | `cubed.Spec`         | execution spec | work_dir, allowed_mem, reserved_mem, executor   |
|   [3]   | `cubed.Config`       | runtime config | donfig-backed configuration manager             |
|   [4]   | `cubed.Callback`     | event observer | base for compute, operation, and task callbacks |
|   [5]   | `cubed.TaskEndEvent` | event payload  | per-task completion event with memory metrics   |

[PUBLIC_TYPE_SCOPE]: `cubed.Array` members
- rail: chunked-compute

| [INDEX] | [MEMBER]                                                       | [KIND]   | [ROLE]                    |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------ |
|   [1]   | `compute(*, executor, callbacks, optimize_graph, resume, ...)` | method   | materialize this array    |
|   [2]   | `rechunk(chunks, *, min_mem, allow_irregular)`                 | method   | change chunk layout       |
|   [3]   | `visualize(filename, format, optimize_graph, engine)`          | method   | render the task graph     |
|   [4]   | `chunks`                                                       | property | per-axis chunk tuple      |
|   [5]   | `chunksize`                                                    | property | single-chunk shape        |
|   [6]   | `npartitions`                                                  | property | total chunk count         |
|   [7]   | `blocks[selection]`                                            | property | block-level indexing view |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array construction (`cubed`)
- rail: chunked-compute

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `asarray(obj, /, *, dtype, device, copy, chunks, spec) -> Array`           | ingest         | any array-like to cubed Array |
|   [2]   | `from_array(x, chunks='auto', asarray, spec) -> Array`                     | ingest         | existing array-like to cubed  |
|   [3]   | `from_zarr(store, path, spec) -> Array`                                    | ingest         | Zarr store to cubed Array     |
|   [4]   | `zeros(shape, *, dtype, device, chunks, spec) -> Array`                    | create         | zero-filled lazy array        |
|   [5]   | `ones(shape, *, dtype, device, chunks, spec) -> Array`                     | create         | one-filled lazy array         |
|   [6]   | `empty(shape, *, dtype, device, chunks, spec) -> Array`                    | create         | uninitialized lazy array      |
|   [7]   | `full(shape, fill_value, *, dtype, device, chunks, spec) -> Array`         | create         | constant-filled lazy array    |
|   [8]   | `arange(start, /, stop, step, *, dtype, device, chunks, spec) -> Array`    | create         | range array                   |
|   [9]   | `eye(n_rows, n_cols, /, *, k, dtype, device, chunks, spec) -> Array`       | create         | identity / diagonal array     |
|  [10]   | `linspace(start, stop, /, num, *, dtype, endpoint, chunks, spec) -> Array` | create         | linearly spaced array         |

[ENTRYPOINT_SCOPE]: execution, store, and transform (`cubed`)
- rail: chunked-compute

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `compute(*arrays, executor, callbacks, optimize_graph, optimize_function, resume, ...)`    | execute        | materialize one or more arrays |
|   [2]   | `store(sources, targets, regions, compute, *, executor, ...)`                              | persist        | write arrays to Zarr targets   |
|   [3]   | `to_zarr(x, store, path, region, compute, *, executor, ...)`                               | persist        | write array to Zarr            |
|   [4]   | `visualize(*arrays, filename='cubed', format, optimize_graph, engine)`                     | inspect        | render task graph diagram      |
|   [5]   | `measure_reserved_mem(executor, work_dir, ...)`                                            | inspect        | measure executor overhead      |
|   [6]   | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, spec) -> Array`               | blockwise      | apply func per chunk           |
|   [7]   | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | gufunc         | generalized ufunc              |
|   [8]   | `rechunk(x, chunks, *, min_mem, allow_irregular)`                                          | reshape        | change chunk specification     |
|   [9]   | `concat(arrays, /, *, axis, chunks)` / `stack(arrays, /, *, axis)`                         | combine        | concatenate or stack arrays    |
|  [10]   | `reshape(x, /, shape, *, copy)` / `broadcast_to(x, /, shape, *, chunks)`                   | reshape        | reshape or broadcast           |
|  [11]   | `where(condition, x1, x2, /)` / `pad(x, pad_width, mode, constant_values, chunks)`         | transform      | conditional select or pad      |
|  [12]   | `nanmean(x, /, *, axis, dtype, keepdims, split_every)`                                     | reduce         | NaN-aware mean reduction       |

[ENTRYPOINT_SCOPE]: linear algebra (`cubed.array_api.linalg`)
- rail: chunked-compute
- family: linalg, all over `cubed.Array`

| [INDEX] | [SURFACE]                          | [OPERATION]                    |
| :-----: | :--------------------------------- | :----------------------------- |
|   [1]   | `matmul(x1, x2)`                   | matrix multiplication          |
|   [2]   | `svd(x)`                           | singular value decomposition   |
|   [3]   | `qr(x)`                            | QR decomposition (TSQR)        |
|   [4]   | `svdvals(x)`                       | singular values only           |
|   [5]   | `tensordot(x1, x2, axes)`          | generalized tensor contraction |
|   [6]   | `outer(x1, x2)` / `vecdot(x1, x2)` | outer product or vector dot    |
|   [7]   | `matrix_transpose(x)`              | swap last two axes             |

## [4]-[IMPLEMENTATION_LAW]

[CHUNKED_TOPOLOGY]:
- namespace: `cubed` (top level), `cubed.array_api.linalg` (linalg), `cubed.runtime.executors` (executor backends)
- all operations build a task graph; no computation runs until `compute()`, `store()`, or `to_zarr()` triggers the executor
- `Spec` declares `work_dir`, `allowed_mem` (peak memory per task), `reserved_mem` (Python-overhead headroom), and the executor via `executor`, `executor_name`, or `executor_options`; `zarr_compressor` sets the Zarr codec
- executors: `local` is the default synchronous backend; distributed backends are `dask`, `lithops`, `modal`, `coiled`, `beam`, `ray`, and `spark`, selected by `executor_name`
- intermediate and output arrays write to Zarr stores under `Spec.work_dir`; `cubed.Array` implements the Python Array API standard for NumPy and CuPy interop via `asarray`
- `rechunk` realigns chunk boundaries without recomputing values; `min_mem` lets cubed choose chunks within a memory bound
- `Callback` subclasses observe `on_compute_start`/`on_compute_end`, `on_operation_start`/`on_operation_end`, and `on_task_end`, which receives a `TaskEndEvent` carrying peak measured memory

[LOCAL_ADMISSION]:
- Array operations compose as a lazy graph under a `Spec` that names `work_dir`, `allowed_mem`, and executor; `compute` is called once at the graph boundary.
- The memory budget (`allowed_mem`, `reserved_mem`) is part of the chunked-compute receipt; `measure_reserved_mem` calibrates it per executor.
- Out-of-core and distributed runs select an executor through `Spec`; never hand-roll a chunk iteration loop.
- Cubed execution is offline study evidence; production substrate selection stays in the C# compute owner.

[RAIL_LAW]:
- Package: `cubed`
- Owns: lazy chunked n-dimensional arrays, Zarr-backed execution, pluggable executors, bounded-memory scheduling, and out-of-core linalg
- Accept: array operations composed as a lazy graph with a `Spec` naming work_dir, allowed_mem, and executor, computed once at the graph boundary
- Reject: eager full-materialization where the lazy graph applies, hand-rolled chunked execution loops cubed owns, and in-memory NumPy for out-of-core payloads
