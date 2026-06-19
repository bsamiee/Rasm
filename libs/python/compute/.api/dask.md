# [PY_COMPUTE_API_DASK]

`dask` supplies lazy, chunked, parallel collections backed by a deferred task graph for the compute study-orchestration rail. `dask.array.Array` partitions large NumPy-shaped payloads into blocks, `dask.dataframe.DataFrame` partitions pandas-shaped tables, `dask.delayed` wraps arbitrary calls into graph nodes, and `dask.bag.Bag` carries unstructured records. Top-level `compute`, `persist`, `optimize`, and `visualize` drive any collection, and `dask.distributed.Client` submits the graph to a local or remote cluster.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask`
- module: `dask`; submodules `dask.array` (alias `da`), `dask.dataframe` (alias `dd`), `dask.bag` (alias `db`), `dask.delayed`, `dask.distributed`
- asset: pure Python; `dask.dataframe` needs `pandas`/`pyarrow`, `dask.distributed` ships in the `distributed` package
- owner: `compute`
- rail: studies

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lazy collection types
- rail: studies

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [ROLE]                                    |
| :-----: | :------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `dask.array.Array`         | chunked array      | blocked NumPy-compatible n-D array        |
|  [02]   | `dask.dataframe.DataFrame` | partitioned frame  | blocked pandas-compatible table           |
|  [03]   | `dask.dataframe.Series`    | partitioned series | blocked pandas-compatible column          |
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
|  [08]   | `tokenize(*args, **kwargs)`                                                              | graph          | deterministic content hash          |

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
|  [02]   | `dd.read_parquet(path, columns, filters, index, calculate_divisions, split_row_groups, filesystem, ...)`    | ingest         | Parquet to partitioned frame       |
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
|  [13]   | `Client(address=None, ...)` / `LocalCluster(...)`                                                           | scheduler      | connect to or launch a cluster     |
|  [14]   | `Client.submit(func, *args, key, workers, retries, priority, pure, ...)`                                    | submit         | submit one task, returns `Future`  |
|  [15]   | `Client.map(func, *iterables, key, workers, retries, batch_size, ...)`                                      | submit         | submit many tasks, returns futures |
|  [16]   | `Client.gather(futures, errors, direct, asynchronous)` / `wait(fs, timeout, return_when)`                   | collect        | retrieve or block on results       |

## [04]-[IMPLEMENTATION_LAW]

[STUDY_TOPOLOGY]:
- namespace: `dask.array` (`da`), `dask.dataframe` (`dd`), `dask.bag` (`db`), `dask.delayed`, `dask.distributed`
- every collection is lazy; operations build a high-level task graph and no work runs until `compute`, `persist`, `store`, or a `Client` submission triggers a scheduler
- the active scheduler is selected by `scheduler=` or by a `distributed.Client` in scope; defaults are threaded for arrays/dataframes and a local cluster when `Client()` is constructed
- `delayed` wraps arbitrary Python calls into `Delayed` graph nodes; experiment-run fan-out composes as a tree of `Delayed` or `Client.submit`/`Client.map` futures
- `from_delayed` converts `Delayed` blocks into a typed `Array` or `DataFrame`, and `to_delayed` reverses it; this is the bridge between hand-built graphs and typed collections
- `optimize` fuses and prunes the graph before execution; `visualize` renders it for inspection; `tokenize` gives a deterministic content hash for caching

[LOCAL_ADMISSION]:
- A large study payload partitions into a `dask.array.Array` or `dask.dataframe.DataFrame`; the chunk or partition facts join the array-admission record and the graph stays lazy until `compute`.
- Experiment-run fan-out composes through `delayed` nodes or `Client.submit`/`Client.map`; the study receipt captures the partition count and the scheduler or `Client` class.
- A single `compute` call sits at the graph boundary; intermediate persistence uses `persist` only when downstream graphs reuse the blocks.
- Dask orchestration is offline study evidence; production scheduling and substrate selection stay in the C# compute owner.

[RAIL_LAW]:
- Package: `dask`
- Owns: lazy chunked arrays/dataframes/bags, the deferred task graph, and study/experiment-run orchestration for the studies rail
- Accept: a chunked study payload or experiment-run graph with a captured partition count and scheduler/`Client` class, computed once at the graph boundary
- Reject: eager full-materialization where chunking applies, hand-rolled parallel loops dask owns, and product scheduling claims
