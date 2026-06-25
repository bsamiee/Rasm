# [PY_COMPUTE_API_DASK]

`dask` supplies lazy, chunked, parallel collections backed by a deferred task graph for the compute study-orchestration rail. `dask.array.Array` partitions large NumPy-shaped payloads into blocks, `dask.dataframe.DataFrame` partitions pandas-shaped tables under a query-planning expression optimizer, `dask.delayed` wraps arbitrary calls into graph nodes, and `dask.bag.Bag` carries unstructured records. Top-level `compute`, `persist`, `optimize`, and `visualize` drive any collection, and `dask.distributed.Client` submits the graph to a local or remote cluster with future-level fan-out.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask`
- module: `dask`; submodules `dask.array` (alias `da`), `dask.dataframe` (alias `dd`), `dask.bag` (alias `db`), `dask.delayed`, `dask.distributed`
- asset: pure Python; `dask.dataframe` needs `pandas`/`pyarrow`, `dask.distributed` ships in the `distributed` package
- license: BSD-3-Clause
- installed: manifest-declared `dask` with no version marker — full CPython 3.15 band (pure Python); `dask.dataframe` query planning (the former `dask-expr`) is merged into the core package as of the 2025.1.0 line, so the legacy non-expression DataFrame is removed and `dd` is the expression-optimized frame by default.
- owner: `compute`
- rail: studies

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lazy collection types
- rail: studies

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [ROLE]                                    |
| :-----: | :------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `dask.array.Array`         | chunked array      | blocked NumPy-compatible n-D array        |
|  [02]   | `dask.dataframe.DataFrame` | partitioned frame  | query-planned pandas-compatible table     |
|  [03]   | `dask.dataframe.Series`    | partitioned series | query-planned pandas-compatible column    |
|  [04]   | `dask.bag.Bag`             | record bag         | partitioned unstructured Python objects   |
|  [05]   | `dask.delayed.Delayed`     | deferred node      | a single lazy call in the task graph      |
|  [06]   | `dask.distributed.Client`  | scheduler client   | submits and tracks graphs on a cluster    |
|  [07]   | `dask.distributed.Future`  | remote result      | handle to a value computed on the cluster |

[PUBLIC_TYPE_SCOPE]: `dask.array.Array` members
- rail: studies

| [INDEX] | [MEMBER]                                         | [KIND]   | [ROLE]                            |
| :-----: | :----------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `compute(**kwargs)`                              | method   | materialize this array to NumPy   |
|  [02]   | `persist(**kwargs)`                              | method   | compute and keep blocks in memory |
|  [03]   | `rechunk(chunks='auto', threshold, ...)`         | method   | change block layout               |
|  [04]   | `map_blocks(func, *args, dtype, chunks, ...)`    | method   | apply `func` per block            |
|  [05]   | `map_overlap(func, depth, boundary, trim, ...)`  | method   | apply `func` with halo overlap    |
|  [06]   | `blocks[selection]`                              | property | block-level indexing view         |
|  [07]   | `to_delayed(optimize_graph=True)`                | method   | per-block `Delayed` objects       |
|  [08]   | `to_zarr(*args)` / `to_hdf5(filename, datapath)` | method   | persist array to Zarr or HDF5     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: collection-agnostic execution (`dask`)
- rail: studies

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `compute(*args, traverse=True, optimize_graph=True, scheduler=None, get=None, **kwargs)` | execute        | materialize one or more collections |
|  [02]   | `persist(*args, traverse=True, optimize_graph=True, scheduler=None, **kwargs)`           | execute        | compute and cache collections       |
|  [03]   | `optimize(*args, traverse=True, **kwargs)`                                               | graph          | return optimized collections        |
|  [04]   | `visualize(*args, filename='mydask', optimize_graph=False, engine=None, **kwargs)`       | inspect        | render the task graph               |
|  [05]   | `delayed(obj, name=None, pure=None, nout=None, traverse=True)`                           | construct      | wrap a call into a graph node       |
|  [06]   | `is_dask_collection(x)`                                                                  | predicate      | test for a dask collection          |
|  [07]   | `annotate(**annotations)` / `get_annotations()`                                          | graph          | attach scheduler annotations        |
|  [08]   | `dask.base.tokenize(*args, **kwargs)`                                                    | graph          | deterministic content hash (`tokenize` lives in `dask.base`) |
|  [09]   | `dask.order.order(dsk, dependencies=None)`                                               | graph          | topological execution-priority ordering of a task graph |
|  [10]   | `config.set(scheduler=..., **kwargs)`                                                    | config         | scoped scheduler/config override context |

[ENTRYPOINT_SCOPE]: array construction and transform (`dask.array`)
- rail: studies

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `from_array(x, chunks='auto', name, lock, asarray, meta, inline_array)`                    | ingest         | array-like to chunked Array     |
|  [02]   | `from_zarr(url, component, storage_options, chunks, ...)`                                  | ingest         | Zarr store to chunked Array     |
|  [03]   | `from_delayed(value, shape, dtype, meta, name)`                                            | ingest         | one `Delayed` block to Array    |
|  [04]   | `asarray(a, allow_unknown_chunksizes, dtype, order, like)`                                 | ingest         | coerce to chunked Array         |
|  [05]   | `zeros` / `ones` / `full(shape, fill_value, ...)`                                          | create         | filled chunked arrays           |
|  [06]   | `arange(start, stop, step, *, chunks, dtype)`                                              | create         | range array                     |
|  [07]   | `linspace(start, stop, num, endpoint, retstep, chunks, dtype)`                             | create         | linearly spaced array           |
|  [08]   | `map_blocks(func, *args, dtype, chunks, drop_axis, new_axis, ...)`                         | blockwise      | apply func per block            |
|  [09]   | `map_overlap(func, *args, depth, boundary, trim, align_arrays, allow_rechunk)`             | blockwise      | apply func with halo overlap    |
|  [10]   | `blockwise(func, out_ind, *args, dtype, adjust_chunks, new_axes, ...)`                     | blockwise      | generalized blocked contraction |
|  [11]   | `apply_gufunc(func, signature, *args, axes, output_dtypes, vectorize, allow_rechunk, ...)` | gufunc         | generalized ufunc               |
|  [12]   | `rechunk(x, chunks='auto', threshold, block_size_limit, balance, method)`                  | reshape        | change block layout             |
|  [13]   | `concatenate(seq, axis, allow_unknown_chunksizes)` / `stack(seq, axis, ...)`               | combine        | join along existing or new axis |
|  [14]   | `reshape(x, shape, merge_chunks, limit)` / `where(condition, x, y)`                        | reshape        | reshape or conditional select   |
|  [15]   | `store(sources, targets, lock, regions, compute, return_stored, ...)`                      | persist        | write blocks to array-likes     |
|  [16]   | `to_zarr(arr, url, component, storage_options, region, compute, mode, ...)`                | persist        | write Array to Zarr             |

[ENTRYPOINT_SCOPE]: dataframe, bag, and distributed (`dask.dataframe`, `dask.bag`, `dask.distributed`)
- rail: studies

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `dd.read_csv(urlpath, blocksize='default', storage_options, assume_missing, ...)`                           | ingest         | CSV to partitioned frame           |
|  [02]   | `dd.read_parquet(path, columns, filters, index, calculate_divisions, split_row_groups, filesystem, ...)`    | ingest         | Parquet to partitioned frame (predicate/projection pushdown via query planning) |
|  [03]   | `dd.read_sql_query(sql, con, index_col, npartitions, bytes_per_chunk, meta, ...)`                           | ingest         | SQL query to partitioned frame     |
|  [04]   | `dd.from_pandas(data, npartitions, sort, chunksize)`                                                        | ingest         | pandas object to partitioned frame |
|  [05]   | `dd.from_map(func, *iterables, args, meta, divisions, enforce_metadata, ...)`                               | ingest         | function map to partitioned frame  |
|  [06]   | `dd.concat(dfs, axis, join, interleave_partitions, ...)` / `dd.merge(left, right, how, on, ...)`            | combine        | concat or join frames              |
|  [07]   | `DataFrame.map_partitions(func, *args, meta, enforce_metadata, ...)`                                        | blockwise      | apply func per partition           |
|  [08]   | `DataFrame.repartition(divisions, npartitions, partition_size, freq, force)`                                | reshape        | change partition layout            |
|  [09]   | `DataFrame.set_index(other, drop, sorted, npartitions, divisions, sort, ...)`                               | reshape        | set and align index                |
|  [10]   | `DataFrame.groupby(by, group_keys, sort, observed, dropna, ...)`                                            | aggregate      | grouped aggregation                |
|  [11]   | `DataFrame.to_parquet(path, **kwargs)`                                                                      | persist        | write frame to Parquet             |
|  [12]   | `db.from_sequence(seq, partition_size, npartitions)` / `db.read_text(urlpath, blocksize, compression, ...)` | ingest         | sequence or text to Bag            |
|  [13]   | `Client(address=None, ...)` / `LocalCluster(n_workers, threads_per_worker, processes, ...)`                 | scheduler      | connect to or launch a cluster     |
|  [14]   | `Client.submit(func, *args, key, workers, retries, priority, pure, resources, ...)`                         | submit         | submit one task, returns `Future`  |
|  [15]   | `Client.map(func, *iterables, key, workers, retries, batch_size, ...)`                                      | submit         | submit many tasks, returns futures |
|  [16]   | `Client.gather(futures, errors, direct, asynchronous)` / `wait(fs, timeout, return_when)` / `as_completed(futures, with_results, raise_errors)` | collect | retrieve, block on, or stream results as they finish |

## [04]-[IMPLEMENTATION_LAW]

[STUDY_TOPOLOGY]:
- namespace: `dask.array` (`da`), `dask.dataframe` (`dd`), `dask.bag` (`db`), `dask.delayed`, `dask.distributed`
- every collection is lazy; operations build a high-level task graph and no work runs until `compute`, `persist`, `store`, or a `Client` submission triggers a scheduler
- `dask.dataframe` runs a query-planning expression optimizer (the merged former `dask-expr`): projection and predicate pushdown, automatic repartition, and join reordering are applied to the expression before lowering to the task graph, so `read_parquet(columns=, filters=)` pruning is planner-driven, not manual
- the active scheduler is selected by `scheduler=`, by `dask.config.set(scheduler=...)`, or by a `distributed.Client` in scope; defaults are threaded for arrays/dataframes and a local cluster when `Client()` is constructed
- `delayed` wraps arbitrary Python calls into `Delayed` graph nodes; experiment-run fan-out composes as a tree of `Delayed` or `Client.submit`/`Client.map` futures, drained by `gather`/`wait`/`as_completed`
- `from_delayed` converts `Delayed` blocks into a typed `Array` or `DataFrame`, and `to_delayed` reverses it; this is the bridge between hand-built graphs and typed collections
- `optimize` fuses and prunes the graph before execution; `visualize` renders it for inspection; `dask.base.tokenize` gives a deterministic content hash for caching and `dask.order.order` exposes the execution-priority topology

[LOCAL_ADMISSION]:
- A large study payload partitions into a `dask.array.Array` or `dask.dataframe.DataFrame`; the chunk or partition facts join the array-admission record and the graph stays lazy until `compute`.
- Experiment-run fan-out composes through `delayed` nodes or `Client.submit`/`Client.map` futures, collected by `gather`/`wait`/`as_completed`; the study receipt captures the partition count and the scheduler or `Client` class.
- A single `compute` call sits at the graph boundary; intermediate persistence uses `persist` only when downstream graphs reuse the blocks.
- Dask orchestration is offline study evidence; production scheduling and substrate selection stay in the C# compute owner.

[INTEGRATION_STACK]:
- numpy/scipy seam: `dask.array.Array` is the chunked NumPy substrate; `da.map_blocks`/`da.map_overlap`/`da.apply_gufunc` carry a per-block `numpy`/`scipy` (`scipy.md`) kernel so a block-local FEM/transform runs on each chunk without materializing the whole array — the array stays lazy and the block kernel is the only eager unit.
- dataframe codec seam: `dd.read_parquet`/`to_parquet` sit on the Arrow/`pyarrow` codec and the query planner pushes `columns=`/`filters=` predicates into the Parquet reader; a partitioned study table joins the data tier without an eager pandas load.
- delayed-sweep seam: a parametrized study (e.g. a `cvxpy` DPP sweep, `cvxpy.md`, or a `diffrax` integration grid, `diffrax.md`) fans out as a tree of `delayed` nodes or `Client.submit` futures keyed by `tokenize(params)`, so identical design points dedupe by content hash and a single `compute`/`gather` drains the grid into one study receipt.
- cubed/zarr seam: when bounded-memory guarantees are required the same chunked payload routes through `cubed` (Zarr-backed) instead of the in-cluster array; `dask` and `cubed` share the Zarr store, so the persistence boundary (`to_zarr`/`from_zarr`) is the interchange point, not a re-encode.

[RAIL_LAW]:
- Package: `dask`
- Owns: lazy chunked arrays/query-planned dataframes/bags, the deferred task graph, and study/experiment-run orchestration for the studies rail
- Accept: a chunked study payload or experiment-run graph with a captured partition count and scheduler/`Client` class, computed once at the graph boundary, with planner-driven pushdown on `dd` ingest and `tokenize`-keyed fan-out dedup
- Reject: eager full-materialization where chunking applies, hand-rolled parallel loops dask owns, manual column/predicate pruning the query planner performs, and product scheduling claims
