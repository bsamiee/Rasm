# [PY_COMPUTE_API_DASK]

`dask` owns lazy chunked parallel collections over a deferred task graph — blocked arrays, partitioned dataframes, unstructured bags, and lifted calls — none materializing until `compute`, `persist`, `store`, or a scheduler submission drives it. `compute` consumes `dask.array.Array` as one passive Array-API backend and imports dask at no runtime point; `data` owns the graph-orchestration surface, feeding the lazy-collections rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask` (BSD-3-Clause, Dask core developers)
- module: `dask.array` (`da`), `dask.dataframe` (`dd`), `dask.bag` (`db`)
- namespaces: `dask`, `dask.delayed`, `dask.base`, `dask.order`, `dask.config`, `dask.distributed`
- owner: `data` (lazy chunked orchestration), `compute` (`numerics/array` Array-API backend, passive)
- depends: `dask.dataframe` binds `pandas`/`pyarrow`; `dask.distributed` ships in the `distributed` distribution
- rail: lazy-collections

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lazy collection and scheduler types

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                              |
| :-----: | :------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `dask.array.Array`         | chunked array      | blocked NumPy-compatible n-D array        |
|  [02]   | `dask.dataframe.DataFrame` | partitioned frame  | query-planned pandas-compatible table     |
|  [03]   | `dask.dataframe.Series`    | partitioned series | query-planned pandas-compatible column    |
|  [04]   | `dask.bag.Bag`             | record bag         | partitioned unstructured Python objects   |
|  [05]   | `dask.delayed.Delayed`     | deferred node      | a single lazy call in the task graph      |
|  [06]   | `dask.distributed.Client`  | scheduler client   | submits and tracks graphs on a cluster    |
|  [07]   | `dask.distributed.Future`  | remote result      | handle to a value computed on the cluster |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: collection-agnostic execution (`dask`)
- `compute`/`persist`/`optimize`/`visualize` carry: `traverse=True`, `optimize_graph=True`, `**kwargs`.

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                             |
| :-----: | :----------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `compute(*args, get=None, ...)`            | static  | materialize one or more collections      |
|  [02]   | `persist(*args, ...)`                      | static  | compute and cache collections            |
|  [03]   | `optimize(*args, traverse=True, **kwargs)` | static  | return optimized collections             |
|  [04]   | `visualize(*args, filename='mydask', ...)` | static  | render the task graph                    |
|  [05]   | `delayed(obj, name=None, nout=None, ...)`  | static  | wrap a call into a graph node            |
|  [06]   | `is_dask_collection(x)`                    | static  | test for a dask collection               |
|  [07]   | `annotate(**annotations)`                  | static  | attach scheduler annotations             |
|  [08]   | `get_annotations()`                        | static  | read the active scheduler annotations    |
|  [09]   | `dask.base.tokenize(*args, **kwargs)`      | static  | deterministic content hash for caching   |
|  [10]   | `dask.order.order(dsk, dependencies=None)` | static  | execution-priority topological ordering  |
|  [11]   | `config.set(scheduler=..., **kwargs)`      | static  | scoped scheduler/config override context |

[ENTRYPOINT_SCOPE]: array construction and transform (`dask.array`)

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `from_array(x, chunks='auto', asarray, meta, ...)`         | static  | array-like to chunked Array     |
|  [02]   | `from_zarr(url, component, storage_options, chunks, ...)`  | static  | Zarr store to chunked Array     |
|  [03]   | `from_delayed(value, shape, dtype, meta, name)`            | static  | one `Delayed` block to Array    |
|  [04]   | `asarray(a, allow_unknown_chunksizes, dtype, order, like)` | static  | coerce to chunked Array         |
|  [05]   | `zeros(shape, *, chunks, dtype)`                           | static  | zero-filled chunked array       |
|  [06]   | `ones(shape, *, chunks, dtype)`                            | static  | one-filled chunked array        |
|  [07]   | `full(shape, fill_value, *, chunks, dtype)`                | static  | value-filled chunked array      |
|  [08]   | `arange(start, stop, step, *, chunks, dtype)`              | static  | range array                     |
|  [09]   | `linspace(start, stop, num, endpoint, ...)`                | static  | linearly spaced array           |
|  [10]   | `map_blocks(func, *args, dtype, drop_axis, new_axis, ...)` | static  | apply func per block            |
|  [11]   | `map_overlap(func, *args, depth, boundary, trim, ...)`     | static  | apply func with halo overlap    |
|  [12]   | `blockwise(func, out_ind, *args, dtype, new_axes, ...)`    | static  | generalized blocked contraction |
|  [13]   | `apply_gufunc(func, signature, *args, output_dtypes, ...)` | static  | generalized ufunc               |
|  [14]   | `rechunk(x, chunks='auto', threshold, balance, ...)`       | static  | change block layout             |
|  [15]   | `concatenate(seq, axis, ...)`                              | static  | join along an existing axis     |
|  [16]   | `stack(seq, axis, ...)`                                    | static  | join along a new axis           |
|  [17]   | `reshape(x, shape, ...)`                                   | static  | reshape an array                |
|  [18]   | `where(condition, x, y)`                                   | static  | element-wise conditional select |
|  [19]   | `store(sources, targets, regions, compute, ...)`           | static  | write blocks to array-likes     |
|  [20]   | `to_zarr(arr, url, component, region, compute, ...)`       | static  | write Array to Zarr             |

[ENTRYPOINT_SCOPE]: `dask.array.Array` methods

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Array.compute(**kwargs)`                       | instance | materialize this array to NumPy   |
|  [02]   | `Array.persist(**kwargs)`                       | instance | compute and keep blocks in memory |
|  [03]   | `Array.rechunk(chunks='auto', threshold, ...)`  | instance | change block layout               |
|  [04]   | `Array.map_blocks(func, *args, dtype, chunks)`  | instance | apply `func` per block            |
|  [05]   | `Array.map_overlap(func, depth, boundary, ...)` | instance | apply `func` with halo overlap    |
|  [06]   | `Array.blocks[selection]`                       | property | block-level indexing view         |
|  [07]   | `Array.to_delayed(optimize_graph=True)`         | instance | per-block `Delayed` objects       |
|  [08]   | `Array.to_zarr(*args)`                          | instance | persist array to Zarr             |
|  [09]   | `Array.to_hdf5(filename, datapath)`             | instance | persist array to HDF5             |

[ENTRYPOINT_SCOPE]: dataframe, bag, and distributed (`dask.dataframe`, `dask.bag`, `dask.distributed`)

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `dd.read_csv(urlpath, blocksize='default', ...)`           | static   | CSV to partitioned frame                                |
|  [02]   | `dd.read_parquet(path, columns, filters, ...)`             | static   | Parquet to frame; planner predicate/projection pushdown |
|  [03]   | `dd.read_sql_query(sql, con, index_col, npartitions, ...)` | static   | SQL query to partitioned frame                          |
|  [04]   | `dd.from_pandas(data, npartitions, sort, chunksize)`       | static   | pandas object to partitioned frame                      |
|  [05]   | `dd.from_map(func, *iterables, meta, divisions, ...)`      | static   | function map to partitioned frame                       |
|  [06]   | `dd.concat(dfs, axis, join, ...)`                          | static   | concat frames along an axis                             |
|  [07]   | `dd.merge(left, right, how, on, ...)`                      | static   | join frames                                             |
|  [08]   | `DataFrame.map_partitions(func, *args, meta, ...)`         | instance | apply func per partition                                |
|  [09]   | `DataFrame.repartition(divisions, npartitions, ...)`       | instance | change partition layout                                 |
|  [10]   | `DataFrame.set_index(other, drop, sorted, ...)`            | instance | set and align index                                     |
|  [11]   | `DataFrame.groupby(by, group_keys, sort, ...)`             | instance | grouped aggregation                                     |
|  [12]   | `DataFrame.to_parquet(path, **kwargs)`                     | instance | write frame to Parquet                                  |
|  [13]   | `db.from_sequence(seq, partition_size, npartitions)`       | static   | sequence to Bag                                         |
|  [14]   | `db.read_text(urlpath, blocksize, compression, ...)`       | static   | text to Bag                                             |
|  [15]   | `Client(address=None, ...)`                                | ctor     | connect to a cluster                                    |
|  [16]   | `LocalCluster(n_workers, threads_per_worker, ...)`         | ctor     | launch a local cluster                                  |
|  [17]   | `Client.submit(func, *args, key, retries, ...)`            | instance | submit one task, returns `Future`                       |
|  [18]   | `Client.map(func, *iterables, key, retries, ...)`          | instance | submit many tasks, returns futures                      |
|  [19]   | `Client.gather(futures, errors, direct, ...)`              | instance | retrieve results                                        |
|  [20]   | `wait(fs, timeout, return_when)`                           | static   | block until futures finish                              |
|  [21]   | `as_completed(futures, with_results, ...)`                 | static   | stream results as they finish                           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every collection is lazy: operations build a high-level task graph and no work runs until `compute`, `persist`, `store`, or a `Client` submission drives a scheduler.
- `dask.dataframe` lowers through a query-planning expression optimizer: projection pushdown, predicate pushdown, automatic repartition, and join reordering apply to the expression before the task graph, so `read_parquet(columns=, filters=)` pruning is planner-driven.
- Active scheduler rides `scheduler=`, `dask.config.set(scheduler=...)`, or a `distributed.Client` in scope; arrays and dataframes thread locally, and `Client()` binds a local cluster.
- `delayed` wraps arbitrary calls into `Delayed` graph nodes; `from_delayed`/`to_delayed` bridge hand-built graphs and typed `Array`/`DataFrame` collections.
- `optimize` fuses and prunes the graph, `visualize` renders it, `dask.base.tokenize` keys a deterministic content hash, and `dask.order.order` exposes the execution-priority topology.

[STACKING]:
- `array-api-compat`(`.api/array-api-compat.md`): `array_namespace(*arrays)` resolves a `dask.array.Array` backend and `is_lazy_array(x)` is true for it, routing the deferred fork.
- `array-api-extra`(`.api/array-api-extra.md`): the deferred fork drives `lazy_apply` while the eager path runs the standard `xp` op.
- `pyarrow`(`../../data/.api/pyarrow.md`) / `zarr`(`../../data/.api/zarr.md`): `dd.read_parquet`/`to_parquet` and `da.from_zarr`/`to_zarr` ride the Arrow and Zarr codecs, the planner pushing `columns=`/`filters=` into the reader.
- `cubed`(`../../data/.api/cubed.md`): a bounded-memory chunked payload routes through `cubed` over the shared Zarr store.
- `odc-stac`(`../../data/.api/odc-stac.md`): lazy cubes are the canonical data-tier ingest.
- `numerics/array`: gates `dask.array.Array` as a `TYPE_CHECKING`-only arm of the `Array` union beside `jax.Array`/`sparse.SparseArray`, so a chunked operand's `shape`/`dtype`/`device` satisfy `ArrayNamespace` with no runtime import.
- within-lib: `da.map_blocks`/`da.map_overlap`/`da.apply_gufunc` carry a per-block `numpy`/`scipy` kernel per chunk; fan-out composes `delayed` nodes or `Client.submit`/`Client.map` futures drained by `gather`/`wait`/`as_completed`, one `compute` at the graph boundary and `persist` only when downstream graphs reuse blocks.

[LOCAL_ADMISSION]:
- A large payload enters as a `dask.array.Array` or `dask.dataframe.DataFrame` capturing chunk/partition count and the scheduler or `Client` class; the graph stays lazy until one `compute` at the boundary, `tokenize`-keyed for dedup.
- `compute` admits `dask.array.Array` only as a passive `array_namespace` backend recognized by `is_lazy_array`; it owns no `dask.dataframe`, `dask.bag`, `dask.delayed`, or `dask.distributed` consumer and imports dask at no runtime point — those orchestration surfaces are the data tier's alone.

[RAIL_LAW]:
- Package: `dask`
- Owns: lazy chunked arrays, query-planned dataframes, bags, the deferred task graph, and local/distributed scheduling
- Accept: a chunked payload or fan-out graph carrying a captured partition count and scheduler/`Client` class computed once at the boundary, planner-driven pushdown on `dd` ingest, `tokenize`-keyed dedup, and `dask.array.Array` as a passive `array_namespace` backend on the compute rail
- Reject: eager full-materialization where chunking applies, hand-rolled parallel loops, manual column/predicate pruning the query planner performs, and a compute-side dask import, task graph, or `Client`
