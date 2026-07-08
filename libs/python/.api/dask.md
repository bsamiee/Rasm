# [PY_DATA_API_DASK]

`dask` supplies lazy, chunked, parallel collections backed by a deferred task graph. `dask.array.Array` partitions large NumPy-shaped payloads into blocks, `dask.dataframe.DataFrame` partitions pandas-shaped tables under a query-planning expression optimizer, `dask.delayed` wraps arbitrary calls into graph nodes, and `dask.bag.Bag` carries unstructured records. Top-level `compute`, `persist`, `optimize`, and `visualize` drive any collection, and `dask.distributed.Client` submits the graph to a local or remote cluster with future-level fan-out. Two consumers share this branch surface: `data` drives the lazy chunked-cube and partitioned-frame orchestration (the STAC/Zarr lazy cube, the partitioned study table); `compute` consumes `dask.array.Array` PASSIVELY as one Array-API namespace backend — `numerics/array` type-gates the `da.Array` union arm, resolves it through `array_namespace`, and routes its lazy fork through `is_lazy_array`, never importing dask at runtime and never driving a task graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask`
- import: `import dask.array as da` / `import dask.dataframe as dd` / `import dask.bag as db`; submodules `dask.delayed`, `dask.distributed`
- version: `2026.6.0`
- license: BSD-3-Clause
- owner: `data` (lazy chunked orchestration), `compute` (`numerics/array` Array-API backend, passive)
- rail: lazy-collections
- installed: manifest-declared `dask` with no version marker — full CPython 3.15 band (pure Python); `dask.dataframe` needs `pandas`/`pyarrow`, `dask.distributed` ships in the `distributed` package. `dask.dataframe` query planning (the former `dask-expr`) is merged into the core package as of the release line, so the legacy non-expression DataFrame is removed and `dd` is the expression-optimized frame by default.
- capability: lazy chunked NumPy-compatible arrays, query-planned pandas-compatible dataframes, unstructured bags, the deferred task graph, and local/distributed scheduling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lazy collection types
- rail: lazy-collections

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [ROLE]                                    |
| ------- | -------------------------- | ------------------ | ----------------------------------------- |
| [01]    | `dask.array.Array`         | chunked array      | blocked NumPy-compatible n-D array        |
| [02]    | `dask.dataframe.DataFrame` | partitioned frame  | query-planned pandas-compatible table     |
| [03]    | `dask.dataframe.Series`    | partitioned series | query-planned pandas-compatible column    |
| [04]    | `dask.bag.Bag`             | record bag         | partitioned unstructured Python objects   |
| [05]    | `dask.delayed.Delayed`     | deferred node      | a single lazy call in the task graph      |
| [06]    | `dask.distributed.Client`  | scheduler client   | submits and tracks graphs on a cluster    |
| [07]    | `dask.distributed.Future`  | remote result      | handle to a value computed on the cluster |

[PUBLIC_TYPE_SCOPE]: `dask.array.Array` members
- rail: lazy-collections

| [INDEX] | [MEMBER]                                         | [KIND]   | [ROLE]                            |
| ------- | ------------------------------------------------ | -------- | --------------------------------- |
| [01]    | `compute(**kwargs)`                              | method   | materialize this array to NumPy   |
| [02]    | `persist(**kwargs)`                              | method   | compute and keep blocks in memory |
| [03]    | `rechunk(chunks='auto', threshold, ...)`         | method   | change block layout               |
| [04]    | `map_blocks(func, *args, dtype, chunks, ...)`    | method   | apply `func` per block            |
| [05]    | `map_overlap(func, depth, boundary, trim, ...)`  | method   | apply `func` with halo overlap    |
| [06]    | `blocks[selection]`                              | property | block-level indexing view         |
| [07]    | `to_delayed(optimize_graph=True)`                | method   | per-block `Delayed` objects       |
| [08]    | `to_zarr(*args)` / `to_hdf5(filename, datapath)` | method   | persist array to Zarr or HDF5     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: collection-agnostic execution (`dask`)
- rail: lazy-collections

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RAIL]                                                       |
| ------- | ---------------------------------------------------------------------------------------- | -------------- | ------------------------------------------------------------ |
| [01]    | `compute(*args, traverse=True, optimize_graph=True, scheduler=None, get=None, **kwargs)` | execute        | materialize one or more collections                          |
| [02]    | `persist(*args, traverse=True, optimize_graph=True, scheduler=None, **kwargs)`           | execute        | compute and cache collections                                |
| [03]    | `optimize(*args, traverse=True, **kwargs)`                                               | graph          | return optimized collections                                 |
| [04]    | `visualize(*args, filename='mydask', optimize_graph=False, engine=None, **kwargs)`       | inspect        | render the task graph                                        |
| [05]    | `delayed(obj, name=None, pure=None, nout=None, traverse=True)`                           | construct      | wrap a call into a graph node                                |
| [06]    | `is_dask_collection(x)`                                                                  | predicate      | test for a dask collection                                   |
| [07]    | `annotate(**annotations)` / `get_annotations()`                                          | graph          | attach scheduler annotations                                 |
| [08]    | `dask.base.tokenize(*args, **kwargs)`                                                    | graph          | deterministic content hash (`tokenize` lives in `dask.base`) |
| [09]    | `dask.order.order(dsk, dependencies=None)`                                               | graph          | topological execution-priority ordering of a task graph      |
| [10]    | `config.set(scheduler=..., **kwargs)`                                                    | config         | scoped scheduler/config override context                     |

[ENTRYPOINT_SCOPE]: array construction and transform (`dask.array`)
- rail: lazy-collections

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                          |
| ------- | ------------------------------------------------------------------------------------------ | -------------- | ------------------------------- |
| [01]    | `from_array(x, chunks='auto', name, lock, asarray, meta, inline_array)`                    | ingest         | array-like to chunked Array     |
| [02]    | `from_zarr(url, component, storage_options, chunks, ...)`                                  | ingest         | Zarr store to chunked Array     |
| [03]    | `from_delayed(value, shape, dtype, meta, name)`                                            | ingest         | one `Delayed` block to Array    |
| [04]    | `asarray(a, allow_unknown_chunksizes, dtype, order, like)`                                 | ingest         | coerce to chunked Array         |
| [05]    | `zeros` / `ones` / `full(shape, fill_value, ...)`                                          | create         | filled chunked arrays           |
| [06]    | `arange(start, stop, step, *, chunks, dtype)`                                              | create         | range array                     |
| [07]    | `linspace(start, stop, num, endpoint, retstep, chunks, dtype)`                             | create         | linearly spaced array           |
| [08]    | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, ...)`                         | blockwise      | apply func per block            |
| [09]    | `map_overlap(func, *args, depth, boundary, trim, align_arrays, allow_rechunk)`             | blockwise      | apply func with halo overlap    |
| [10]    | `blockwise(func, out_ind, *args, dtype, adjust_chunks, new_axes, ...)`                     | blockwise      | generalized blocked contraction |
| [11]    | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | gufunc         | generalized ufunc               |
| [12]    | `rechunk(x, chunks='auto', threshold, block_size_limit, balance, method)`                  | reshape        | change block layout             |
| [13]    | `concatenate(seq, axis, allow_unknown_chunksizes)` / `stack(seq, axis, ...)`               | combine        | join along existing or new axis |
| [14]    | `reshape(x, shape, merge_chunks, limit)` / `where(condition, x, y)`                        | reshape        | reshape or conditional select   |
| [15]    | `store(sources, targets, lock, regions, compute, return_stored, ...)`                      | persist        | write blocks to array-likes     |
| [16]    | `to_zarr(arr, url, component, storage_options, region, compute, mode, ...)`                | persist        | write Array to Zarr             |

[ENTRYPOINT_SCOPE]: dataframe, bag, and distributed (`dask.dataframe`, `dask.bag`, `dask.distributed`)
- rail: lazy-collections

| [INDEX] | [SURFACE]                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                                                          |
| ------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ------------------------------------------------------------------------------- |
| [01]    | `dd.read_csv(urlpath, blocksize='default', storage_options, assume_missing, ...)`                                                               | ingest         | CSV to partitioned frame                                                        |
| [02]    | `dd.read_parquet(path, columns, filters, index, calculate_divisions, split_row_groups, filesystem, ...)`                                        | ingest         | Parquet to partitioned frame (predicate/projection pushdown via query planning) |
| [03]    | `dd.read_sql_query(sql, con, index_col, npartitions, bytes_per_chunk, meta, ...)`                                                               | ingest         | SQL query to partitioned frame                                                  |
| [04]    | `dd.from_pandas(data, npartitions, sort, chunksize)`                                                                                            | ingest         | pandas object to partitioned frame                                              |
| [05]    | `dd.from_map(func, *iterables, args, meta, divisions, enforce_metadata, ...)`                                                                   | ingest         | function map to partitioned frame                                               |
| [06]    | `dd.concat(dfs, axis, join, interleave_partitions, ...)` / `dd.merge(left, right, how, on, ...)`                                                | combine        | concat or join frames                                                           |
| [07]    | `DataFrame.map_partitions(func, *args, meta, enforce_metadata, ...)`                                                                            | blockwise      | apply func per partition                                                        |
| [08]    | `DataFrame.repartition(divisions, npartitions, partition_size, freq, force)`                                                                    | reshape        | change partition layout                                                         |
| [09]    | `DataFrame.set_index(other, drop, sorted, npartitions, divisions, sort, ...)`                                                                   | reshape        | set and align index                                                             |
| [10]    | `DataFrame.groupby(by, group_keys, sort, observed, dropna, ...)`                                                                                | aggregate      | grouped aggregation                                                             |
| [11]    | `DataFrame.to_parquet(path, **kwargs)`                                                                                                          | persist        | write frame to Parquet                                                          |
| [12]    | `db.from_sequence(seq, partition_size, npartitions)` / `db.read_text(urlpath, blocksize, compression, ...)`                                     | ingest         | sequence or text to Bag                                                         |
| [13]    | `Client(address=None, ...)` / `LocalCluster(n_workers, threads_per_worker, processes, ...)`                                                     | scheduler      | connect to or launch a cluster                                                  |
| [14]    | `Client.submit(func, *args, key, workers, retries, priority, pure, resources, ...)`                                                             | submit         | submit one task, returns `Future`                                               |
| [15]    | `Client.map(func, *iterables, key, workers, retries, batch_size, ...)`                                                                          | submit         | submit many tasks, returns futures                                              |
| [16]    | `Client.gather(futures, errors, direct, asynchronous)` / `wait(fs, timeout, return_when)` / `as_completed(futures, with_results, raise_errors)` | collect        | retrieve, block on, or stream results as they finish                            |

## [04]-[IMPLEMENTATION_LAW]

[COLLECTION_TOPOLOGY]:
- namespace: `dask.array` (`da`), `dask.dataframe` (`dd`), `dask.bag` (`db`), `dask.delayed`, `dask.distributed`
- every collection is lazy; operations build a high-level task graph and no work runs until `compute`, `persist`, `store`, or a `Client` submission triggers a scheduler
- `dask.dataframe` runs a query-planning expression optimizer (the merged former `dask-expr`): projection and predicate pushdown, automatic repartition, and join reordering are applied to the expression before lowering to the task graph, so `read_parquet(columns=, filters=)` pruning is planner-driven, not manual
- the active scheduler is selected by `scheduler=`, by `dask.config.set(scheduler=...)`, or by a `distributed.Client` in scope; defaults are threaded for arrays/dataframes and a local cluster when `Client()` is constructed
- `delayed` wraps arbitrary Python calls into `Delayed` graph nodes; fan-out composes as a tree of `Delayed` or `Client.submit`/`Client.map` futures, drained by `gather`/`wait`/`as_completed`
- `from_delayed` converts `Delayed` blocks into a typed `Array` or `DataFrame`, and `to_delayed` reverses it; this is the bridge between hand-built graphs and typed collections
- `optimize` fuses and prunes the graph before execution; `visualize` renders it; `dask.base.tokenize` gives a deterministic content hash for caching and `dask.order.order` exposes the execution-priority topology

[DATA_ORCHESTRATION]:
- A large payload partitions into a `dask.array.Array` or `dask.dataframe.DataFrame`; the chunk/partition facts join the record and the graph stays lazy until `compute`. `odc-stac` lazy cubes and `dd.read_parquet` partitioned tables are the canonical data-tier ingest.
- Fan-out composes through `delayed` nodes or `Client.submit`/`Client.map` futures, collected by `gather`/`wait`/`as_completed`; a single `compute` sits at the graph boundary, `persist` only when downstream graphs reuse blocks.
- numpy/scipy seam: `da.map_blocks`/`da.map_overlap`/`da.apply_gufunc` carry a per-block `numpy`/`scipy` kernel so a block-local transform runs on each chunk without materializing the whole array.
- codec seam: `dd.read_parquet`/`to_parquet` and `da.from_zarr`/`to_zarr` sit on the Arrow/`pyarrow` and Zarr codecs; the query planner pushes `columns=`/`filters=` predicates into the reader. When bounded-memory guarantees are required the same chunked payload routes through `cubed` (Zarr-backed), sharing the Zarr store as the interchange point.

[COMPUTE_PASSIVE_BACKEND]:
- compute NEVER imports dask at runtime and NEVER drives a task graph. `numerics/array` gates `dask.array.Array` as a `TYPE_CHECKING`-only arm of the `Array` union (beside `jax.Array`/`sparse.SparseArray`), so a chunked operand's `shape`/`dtype`/`device` members satisfy the `ArrayNamespace` protocol without a runtime import.
- `array-api-compat.array_namespace(*arrays)` resolves the chunked backend and `array-api-compat.is_lazy_array(x)` is true for a Dask (and a JAX-deferred) operand, routing the `[LAZY_EAGER_FORK]` deferred path to `array_api_extra.lazy_apply` while the eager path runs the standard `xp` op. dask is the passive lazy substrate the fork recognizes, never a compute-authored orchestration surface.
- compute owns NO `dask.dataframe`, `dask.bag`, `dask.delayed`, or `dask.distributed` consumer — those are the data-tier orchestration owners exclusively.

[RAIL_LAW]:
- Package: `dask`
- Owns: lazy chunked arrays / query-planned dataframes / bags, the deferred task graph, and local/distributed scheduling
- Accept (data): a chunked payload or fan-out graph with a captured partition count and scheduler/`Client` class, computed once at the graph boundary, planner-driven pushdown on `dd` ingest, `tokenize`-keyed dedup
- Accept (compute): `dask.array.Array` as a passive `array_namespace` backend recognized by `is_lazy_array`, resolved through the Array-API rail, never imported at runtime
- Reject: eager full-materialization where chunking applies; hand-rolled parallel loops dask owns; manual column/predicate pruning the query planner performs; a compute-side dask import, task graph, or `Client` — compute consumes the array type only
