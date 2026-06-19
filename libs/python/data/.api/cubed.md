# [PY_DATA_API_CUBED]

`cubed` supplies a lazy, chunked, Array API-compliant n-dimensional array backed by Zarr storage and a bounded-memory task graph for the data chunked-compute rail. `cubed.Array` builds a deferred graph from Array API operations, `cubed.Spec` declares the work directory, memory budget, and executor, and `compute`/`store`/`to_zarr` materialize against a Zarr store. The graph runs through a pluggable executor (`local`, `dask`, `lithops`, `modal`, `coiled`, `beam`, `ray`, `spark`) for out-of-core and distributed workloads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cubed`
- package: `cubed`
- module: `cubed`; submodules `cubed.array_api.linalg`, `cubed.runtime.executors`
- asset: pure Python
- owner: `data`
- rail: chunked-compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core and execution types
- rail: chunked-compute

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [ROLE]                                          |
| :-----: | :------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `cubed.Array`        | lazy array     | deferred chunked array with Array API surface   |
|  [02]   | `cubed.Spec`         | execution spec | work_dir, allowed_mem, reserved_mem, executor   |
|  [03]   | `cubed.Config`       | runtime config | donfig-backed configuration manager             |
|  [04]   | `cubed.Callback`     | event observer | base for compute, operation, and task callbacks |
|  [05]   | `cubed.TaskEndEvent` | event payload  | per-task completion event with memory metrics   |

[PUBLIC_TYPE_SCOPE]: `cubed.Array` members
- rail: chunked-compute

| [INDEX] | [MEMBER]                                                       | [KIND]   | [ROLE]                    |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `compute(*, executor, callbacks, optimize_graph, resume, ...)` | method   | materialize this array    |
|  [02]   | `rechunk(chunks, *, min_mem, allow_irregular)`                 | method   | change chunk layout       |
|  [03]   | `visualize(filename, format, optimize_graph, engine)`          | method   | render the task graph     |
|  [04]   | `chunks`                                                       | property | per-axis chunk tuple      |
|  [05]   | `chunksize`                                                    | property | single-chunk shape        |
|  [06]   | `npartitions`                                                  | property | total chunk count         |
|  [07]   | `blocks[selection]`                                            | property | block-level indexing view |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array construction (`cubed`)
- rail: chunked-compute

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `asarray(obj, /, *, dtype, device, copy, chunks, spec) -> Array`           | ingest         | any array-like to cubed Array |
|  [02]   | `from_array(x, chunks='auto', asarray, spec) -> Array`                     | ingest         | existing array-like to cubed  |
|  [03]   | `from_zarr(store, path, spec) -> Array`                                    | ingest         | Zarr store to cubed Array     |
|  [04]   | `zeros(shape, *, dtype, device, chunks, spec) -> Array`                    | create         | zero-filled lazy array        |
|  [05]   | `ones(shape, *, dtype, device, chunks, spec) -> Array`                     | create         | one-filled lazy array         |
|  [06]   | `empty(shape, *, dtype, device, chunks, spec) -> Array`                    | create         | uninitialized lazy array      |
|  [07]   | `full(shape, fill_value, *, dtype, device, chunks, spec) -> Array`         | create         | constant-filled lazy array    |
|  [08]   | `arange(start, /, stop, step, *, dtype, device, chunks, spec) -> Array`    | create         | range array                   |
|  [09]   | `eye(n_rows, n_cols, /, *, k, dtype, device, chunks, spec) -> Array`       | create         | identity / diagonal array     |
|  [10]   | `linspace(start, stop, /, num, *, dtype, endpoint, chunks, spec) -> Array` | create         | linearly spaced array         |

[ENTRYPOINT_SCOPE]: execution, store, and transform (`cubed`)
- rail: chunked-compute

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `compute(*arrays, executor, callbacks, optimize_graph, optimize_function, resume, ...)`    | execute        | materialize one or more arrays |
|  [02]   | `store(sources, targets, regions, compute, *, executor, ...)`                              | persist        | write arrays to Zarr targets   |
|  [03]   | `to_zarr(x, store, path, region, compute, *, executor, ...)`                               | persist        | write array to Zarr            |
|  [04]   | `visualize(*arrays, filename='cubed', format, optimize_graph, engine)`                     | inspect        | render task graph diagram      |
|  [05]   | `measure_reserved_mem(executor, work_dir, ...)`                                            | inspect        | measure executor overhead      |
|  [06]   | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, spec) -> Array`               | blockwise      | apply func per chunk           |
|  [07]   | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | gufunc         | generalized ufunc              |
|  [08]   | `rechunk(x, chunks, *, min_mem, allow_irregular)`                                          | reshape        | change chunk specification     |
|  [09]   | `concat(arrays, /, *, axis, chunks)` / `stack(arrays, /, *, axis)`                         | combine        | concatenate or stack arrays    |
|  [10]   | `reshape(x, /, shape, *, copy)` / `broadcast_to(x, /, shape, *, chunks)`                   | reshape        | reshape or broadcast           |
|  [11]   | `where(condition, x1, x2, /)` / `pad(x, pad_width, mode, constant_values, chunks)`         | transform      | conditional select or pad      |
|  [12]   | `nanmean(x, /, *, axis, dtype, keepdims, split_every)`                                     | reduce         | NaN-aware mean reduction       |

[ENTRYPOINT_SCOPE]: linear algebra (`cubed.array_api.linalg`)
- rail: chunked-compute
- family: linalg, all over `cubed.Array`

| [INDEX] | [SURFACE]                          | [OPERATION]                    |
| :-----: | :--------------------------------- | :----------------------------- |
|  [01]   | `matmul(x1, x2)`                   | matrix multiplication          |
|  [02]   | `svd(x)`                           | singular value decomposition   |
|  [03]   | `qr(x)`                            | QR decomposition (TSQR)        |
|  [04]   | `svdvals(x)`                       | singular values only           |
|  [05]   | `tensordot(x1, x2, axes)`          | generalized tensor contraction |
|  [06]   | `outer(x1, x2)` / `vecdot(x1, x2)` | outer product or vector dot    |
|  [07]   | `matrix_transpose(x)`              | swap last two axes             |

## [04]-[IMPLEMENTATION_LAW]

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
