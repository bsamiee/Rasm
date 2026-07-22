# [PY_DATA_API_DAFT]

`daft` is a Rust-backed distributed and streaming dataframe engine for multimodal, out-of-core data: a lazy `DataFrame` builds a logical plan from `Expression` column nodes and defers execution until a sink (`collect`, `show`, `iter_rows`, `write_*`) materializes it, enabling predicate/projection pushdown and partition-parallel evaluation across the native single-node runner or a Ray cluster. A typed `DataType` vocabulary (numeric, temporal, `list`/`fixed_size_list`/`struct`/`map`, plus `tensor`/`image`/`embedding` multimodal types) backs every column; top-level factories (`col`, `lit`, `coalesce`, `interval`, `udf`, `sql`) compose expressions, and `read_*`/`from_*` constructors ingest Parquet/CSV/JSON, Delta/Iceberg/Hudi/Lance lakehouse tables, SQL, and Arrow/pandas/pydict sources without eager whole-set materialization. The data package owner composes `DataFrame`, `Expression`, and the `read_*` rail into the `DAFT_ELASTICITY` path; it never re-implements the columnar execution, partitioning, or lakehouse readers daft already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `daft`
- package: `daft`
- owner: `data`
- import: `daft`
- rail: distributed dataframe
- installed: `0.7.16`
- entry points: console script `daft` (`daft.cli:main`); library use is import-only
- license: Apache-2.0
- capability: lazy logical-plan dataframes over partitioned out-of-core data; native and Ray runners; predicate/projection pushdown; typed numeric/temporal/nested/multimodal (`tensor`/`image`/`embedding`) dtypes; expression algebra with string/datetime/list/struct/map/image/url/embedding/binary/float/json/partitioning namespaces; window functions; scalar and class UDFs; Parquet/CSV/JSON/WARC/MCAP IO; Delta Lake/Iceberg/Hudi/Lance/Unity/Glue lakehouse readers and catalogs; SQL over registered frames; Arrow/pandas/Ray/Dask/PyTorch interop and streaming row/batch/partition iterators. Reflection note: 555 types across 30 namespaces, fidelity introspected

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame, expression, schema, catalog, and config types
- rail: distributed dataframe

`__all__` exports 21 public types and 81 functions. `DataFrame` is the lazy plan root; `Expression` is the composable column node carrying typed sub-namespaces (`str`, `dt`, `list`, `struct`, `map`, `image`, `url`, `embedding`, `binary`, `float`, `partitioning`); `DataType` is the dtype factory; `Session`/`Catalog`/`Identifier` own catalog-scoped table resolution; `Window` builds window-function frames. `DataCatalogType`/`MediaType`/`ImageFormat`/`ImageMode`/`ImageProperty`/`TimeUnit` are bounded vocabularies.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [ROLE]                                               |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `DataFrame`        | lazy frame        | deferred logical-plan table over partitioned data    |
|  [02]   | `Expression`       | expression node   | composable typed column expression with namespaces   |
|  [03]   | `DataType`         | dtype factory     | numeric/temporal/nested/multimodal type constructor  |
|  [04]   | `Schema`           | schema value      | ordered `{name: DataType}` field mapping             |
|  [05]   | `Series`           | typed column      | named typed 1-D column over Arrow buffers            |
|  [06]   | `Window`           | window spec       | partition/order/frame builder for `Expression.over`  |
|  [07]   | `Session`          | session context   | catalog/namespace/runner-scoped table and SQL access |
|  [08]   | `Catalog`          | catalog binding   | Iceberg/Unity/Glue/S3Tables/pydict table provider    |
|  [09]   | `Table`            | catalog table     | catalog-resolved readable/writable table             |
|  [10]   | `Identifier`       | qualified name    | namespace-qualified catalog identifier               |
|  [11]   | `DataCatalogTable` | catalog table ref | external data-catalog table reference                |
|  [12]   | `DataCatalogType`  | enum              | data-catalog backend selector                        |
|  [13]   | `IOConfig`         | IO config         | object-store credentials and IO policy carrier       |
|  [14]   | `ResourceRequest`  | resource spec     | per-task CPU/GPU/memory resource request             |
|  [15]   | `File`             | file handle       | multimodal file-column reference                     |
|  [16]   | `VideoFile`        | file handle       | video file-column reference for frame decode         |
|  [17]   | `TimeUnit`         | dtype unit        | timestamp/duration time-unit vocabulary              |
|  [18]   | `MediaType`        | media vocabulary  | media-type vocabulary for file columns               |
|  [19]   | `ImageMode`        | image vocabulary  | image channel/mode vocabulary                        |
|  [20]   | `ImageFormat`      | image vocabulary  | image encoding-format vocabulary                     |
|  [21]   | `ImageProperty`    | image vocabulary  | image property selector vocabulary                   |

[PUBLIC_TYPE_SCOPE]: exception hierarchy
- rail: distributed dataframe — `daft.exceptions`; rooted at `DaftCoreException`, the transient branch is the retry discriminant a `stamina` rail targets

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]   | [ROLE]                                                 |
| :-----: | :--------------------------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `DaftCoreException`                                        | base error      | root of every daft exception (subclasses `ValueError`) |
|  [02]   | `DaftTransientError`                                       | transient base  | retryable network fault; the retry discriminant        |
|  [03]   | `ConnectTimeoutError` / `ReadTimeoutError` / `SocketError` | transient error | concrete transient subclass (429/5xx/timeout)          |
|  [04]   | `ThrottleError` / `ByteStreamError` / `MiscTransientError` | transient error | concrete transient subclass (429/5xx/timeout)          |
|  [05]   | `DaftTypeError`                                            | type error      | non-transient schema/dtype misuse                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest and source constructors
- rail: distributed dataframe

`read_*` constructors build a lazy `DataFrame` over a scan source with pushdown; `from_*` constructors ingest in-memory or framework objects. Every reader threads `io_config` for object-store access; schema is inferred unless `schema`/`infer_schema=False` pins it. The keyed list carries each constructor's full signature.

| [INDEX] | [SURFACE]        | [CAPABILITY]                               |
| :-----: | :--------------- | :----------------------------------------- |
|  [01]   | `read_parquet`   | lazy Parquet scan with row-group pushdown  |
|  [02]   | `read_csv`       | lazy CSV scan with dialect control         |
|  [03]   | `read_json`      | lazy newline-delimited JSON scan           |
|  [04]   | `read_deltalake` | Delta Lake table read at version/timestamp |
|  [05]   | `read_iceberg`   | Iceberg table read at snapshot             |
|  [06]   | `read_hudi`      | Hudi table read                            |
|  [07]   | `read_lance`     | Lance dataset read                         |
|  [08]   | `read_sql`       | partitioned SQL read with pushdown         |
|  [09]   | `read_warc`      | WARC web-archive scan                      |
|  [10]   | `read_table`     | read a catalog-registered table            |
|  [11]   | `from_glob_path` | file-listing frame from a glob             |
|  [12]   | `from_pydict`    | in-memory frame from column dict           |
|  [13]   | `from_pylist`    | in-memory frame from row dicts             |
|  [14]   | `from_arrow`     | zero-copy frame from Arrow table(s)        |
|  [15]   | `from_pandas`    | frame from pandas DataFrame(s)             |
|  [16]   | `write_table`    | write a frame to a catalog table           |

- [01]-[READ_PARQUET]: `read_parquet(path, row_groups=None, infer_schema=True, schema=None, io_config=None, file_path_column=None, hive_partitioning=False, coerce_int96_timestamp_unit=None) -> DataFrame`
- [02]-[READ_CSV]: `read_csv(path, infer_schema=True, schema=None, has_headers=True, delimiter=None, double_quote=True, quote=None, escape_char=None, comment=None, allow_variable_columns=False, io_config=None, file_path_column=None, hive_partitioning=False) -> DataFrame`
- [03]-[READ_JSON]: `read_json(path, infer_schema=True, schema=None, io_config=None, file_path_column=None, hive_partitioning=False) -> DataFrame`
- [04]-[READ_DELTALAKE]: `read_deltalake(table, version=None, io_config=None) -> DataFrame`
- [05]-[READ_ICEBERG]: `read_iceberg(table, snapshot_id=None, io_config=None) -> DataFrame`
- [06]-[READ_HUDI]: `read_hudi(table_uri, io_config=None) -> DataFrame`
- [07]-[READ_LANCE]: `read_lance(url, io_config=None, version=None, asof=None, block_size=None, index_cache_size=None, default_scan_options=None) -> DataFrame`
- [08]-[READ_SQL]: `read_sql(sql, conn, partition_col=None, num_partitions=None, partition_bound_strategy='min-max', disable_pushdowns_to_sql=False, infer_schema=True, infer_schema_length=10, schema=None) -> DataFrame`
- [09]-[READ_WARC]: `read_warc(path, io_config=None, file_path_column=None) -> DataFrame`
- [10]-[READ_TABLE]: `read_table(identifier, **options) -> DataFrame`
- [11]-[FROM_GLOB_PATH]: `from_glob_path(path, io_config=None) -> DataFrame`
- [12]-[FROM_PYDICT]: `from_pydict(data) -> DataFrame`
- [13]-[FROM_PYLIST]: `from_pylist(data) -> DataFrame`
- [14]-[FROM_ARROW]: `from_arrow(data) -> DataFrame`
- [15]-[FROM_PANDAS]: `from_pandas(data) -> DataFrame`
- [16]-[WRITE_TABLE]: `write_table(identifier, df, mode='append', **options) -> None`

[ENTRYPOINT_SCOPE]: expression and SQL factories
- rail: distributed dataframe

`col`/`lit`/`coalesce`/`interval`/`list_`/`struct`/`element` mint `Expression` nodes; `sql` runs a query over bound frames; `udf` decorates a Python callable into a partition UDF. The keyed list carries each factory's full signature.

| [INDEX] | [SURFACE]  | [CAPABILITY]                             |
| :-----: | :--------- | :--------------------------------------- |
|  [01]   | `col`      | reference a column by name               |
|  [02]   | `lit`      | literal-value expression                 |
|  [03]   | `element`  | element placeholder for list operations  |
|  [04]   | `list_`    | construct a list column from expressions |
|  [05]   | `struct`   | construct a struct column from fields    |
|  [06]   | `coalesce` | first-non-null across expressions        |
|  [07]   | `interval` | temporal interval literal                |
|  [08]   | `sql`      | run SQL over bound/global frames         |
|  [09]   | `sql_expr` | parse a SQL fragment into an expression  |
|  [10]   | `udf`      | decorate a callable into a partition UDF |

- [01]-[COL]: `col(name) -> Expression`
- [02]-[LIT]: `lit(value) -> Expression`
- [03]-[ELEMENT]: `element() -> Expression`
- [04]-[LIST]: `list_(*items) -> Expression`
- [05]-[STRUCT]: `struct(*fields) -> Expression`
- [06]-[COALESCE]: `coalesce(*args) -> Expression`
- [07]-[INTERVAL]: `interval(years=None, months=None, days=None, hours=None, minutes=None, seconds=None, millis=None, nanos=None) -> Expression`
- [08]-[SQL]: `sql(sql, register_globals=True, **bindings) -> DataFrame`
- [09]-[SQL_EXPR]: `sql_expr(sql) -> Expression`
- [10]-[UDF]: `udf(*, return_dtype, num_cpus=None, num_gpus=None, memory_bytes=None, batch_size=None, concurrency=None, use_process=None) -> Callable[[func], UDF]`

[ENTRYPOINT_SCOPE]: `DataFrame` transform and sink
- rail: distributed dataframe

Transform methods return a new lazy `DataFrame`; `collect`/`show`/`count_rows`/`to_*`/`iter_rows`/`write_*` are sinks that trigger execution. Columns accept an `Expression` or a `str` column name interchangeably.

| [INDEX] | [SURFACE]                   | [CAPABILITY]                               |
| :-----: | :-------------------------- | :----------------------------------------- |
|  [01]   | `DataFrame.select`          | project columns and named expressions      |
|  [02]   | `DataFrame.where`           | filter rows by predicate (`filter` alias)  |
|  [03]   | `DataFrame.with_column`     | add/replace one computed column            |
|  [04]   | `DataFrame.with_columns`    | add/replace columns from a dict            |
|  [05]   | `DataFrame.exclude`         | drop named columns                         |
|  [06]   | `DataFrame.sort`            | order rows by columns/expressions          |
|  [07]   | `DataFrame.limit`           | cap row count                              |
|  [08]   | `DataFrame.distinct`        | deduplicate rows (optionally on keys)      |
|  [09]   | `DataFrame.join`            | relational join with strategy hint         |
|  [10]   | `DataFrame.groupby`         | group rows for aggregation                 |
|  [11]   | `DataFrame.agg`             | global aggregation over expressions        |
|  [12]   | `DataFrame.explode`         | unnest list columns to rows                |
|  [13]   | `DataFrame.unpivot`         | wide-to-long reshape                       |
|  [14]   | `DataFrame.sample`          | random row sample                          |
|  [15]   | `DataFrame.repartition`     | hash/round-robin repartition               |
|  [16]   | `DataFrame.into_partitions` | coalesce/split into N partitions           |
|  [17]   | `DataFrame.collect`         | execute and cache the result               |
|  [18]   | `DataFrame.show`            | execute and render a preview               |
|  [19]   | `DataFrame.count_rows`      | execute and count rows                     |
|  [20]   | `DataFrame.iter_rows`       | stream rows out-of-core                    |
|  [21]   | `DataFrame.to_arrow`        | execute and egress to Arrow                |
|  [22]   | `DataFrame.to_pydict`       | execute and egress to column dict          |
|  [23]   | `DataFrame.to_pandas`       | execute and egress to pandas               |
|  [24]   | `DataFrame.write_parquet`   | partitioned Parquet sink                   |
|  [25]   | `DataFrame.write_deltalake` | Delta Lake table sink                      |
|  [26]   | `DataFrame.write_iceberg`   | Iceberg table sink                         |
|  [27]   | `DataFrame.write_csv`       | partitioned CSV sink                       |
|  [28]   | `DataFrame.write_json`      | partitioned newline-JSON sink              |
|  [29]   | `DataFrame.write_lance`     | Lance dataset sink                         |
|  [30]   | `DataFrame.pivot`           | long-to-wide pivot with aggregation        |
|  [31]   | `DataFrame.transform`       | apply a `DataFrame -> DataFrame` function  |
|  [32]   | `DataFrame.summarize`       | per-column statistics frame                |
|  [33]   | `DataFrame.describe`        | schema as a frame                          |
|  [34]   | `DataFrame.agg_list`        | global list/set/concat aggregations        |
|  [35]   | `DataFrame.explain`         | print the logical/physical plan            |
|  [36]   | `DataFrame.schema`          | resolved output schema                     |
|  [37]   | `DataFrame.metrics`         | materialized per-operator statistics frame |
|  [38]   | `DataFrame.num_partitions`  | resolved partition count post-execution    |

- [01]-[SELECT]: `select(*columns, **projections) -> DataFrame`
- [02]-[WHERE]: `where(predicate) -> DataFrame`
- [03]-[WITH_COLUMN]: `with_column(column_name, expr) -> DataFrame`
- [04]-[WITH_COLUMNS]: `with_columns(columns) -> DataFrame`
- [05]-[EXCLUDE]: `exclude(*names) -> DataFrame`
- [06]-[SORT]: `sort(by, desc=False, nulls_first=None) -> DataFrame`
- [07]-[LIMIT]: `limit(num) -> DataFrame`
- [08]-[DISTINCT]: `distinct(*on) -> DataFrame`
- [09]-[JOIN]: `join(other, on=None, left_on=None, right_on=None, how='inner', strategy=None, prefix=None, suffix=None) -> DataFrame`
- [10]-[GROUPBY]: `groupby(*group_by) -> GroupedDataFrame`
- [11]-[AGG]: `agg(*to_agg) -> DataFrame`
- [12]-[EXPLODE]: `explode(*columns) -> DataFrame`
- [13]-[UNPIVOT]: `unpivot(ids, values=[], variable_name='variable', value_name='value') -> DataFrame`
- [14]-[SAMPLE]: `sample(fraction, with_replacement=False, seed=None) -> DataFrame`
- [15]-[REPARTITION]: `repartition(num, *partition_by) -> DataFrame`
- [16]-[INTO_PARTITIONS]: `into_partitions(num) -> DataFrame`
- [17]-[COLLECT]: `collect(num_preview_rows=8) -> DataFrame`
- [18]-[SHOW]: `show(n=8, format=None, verbose=False, max_width=30, align='left', columns=None) -> None`
- [19]-[COUNT_ROWS]: `count_rows() -> int`
- [20]-[ITER_ROWS]: `iter_rows(results_buffer_size='num_cpus', column_format='python') -> Iterator[dict[str, Any]]`
- [21]-[TO_ARROW]: `to_arrow() -> pyarrow.Table`
- [22]-[TO_PYDICT]: `to_pydict() -> dict[str, list[Any]]`
- [23]-[TO_PANDAS]: `to_pandas(coerce_temporal_nanoseconds=False) -> pandas.DataFrame`
- [24]-[WRITE_PARQUET]: `write_parquet(root_dir, compression='snappy', write_mode='append', partition_cols=None, io_config=None) -> DataFrame`
- [25]-[WRITE_DELTALAKE]: `write_deltalake(table, partition_cols=None, mode='append', schema_mode=None, ..., io_config=None) -> DataFrame`
- [26]-[WRITE_ICEBERG]: `write_iceberg(table, mode='append', io_config=None) -> DataFrame`
- [27]-[WRITE_CSV]: `write_csv(root_dir, write_mode='append', partition_cols=None, io_config=None) -> DataFrame`
- [28]-[WRITE_JSON]: `write_json(root_dir, write_mode='append', partition_cols=None, io_config=None) -> DataFrame`
- [29]-[WRITE_LANCE]: `write_lance(uri, mode='create', io_config=None, **kwargs) -> DataFrame`
- [30]-[PIVOT]: `pivot(group_by, pivot_col, value_col, agg_fn, names=None) -> DataFrame`
- [31]-[TRANSFORM]: `transform(func, *args, **kwargs) -> DataFrame`
- [32]-[SUMMARIZE]: `summarize() -> DataFrame`
- [33]-[DESCRIBE]: `describe() -> DataFrame`
- [34]-[AGG_LIST]: `agg_list(*cols)` / `agg_set(*cols)` / `agg_concat(*cols) -> DataFrame`
- [35]-[EXPLAIN]: `explain(show_all=False, format='ascii', simple=False, file=None)`
- [36]-[SCHEMA]: `schema() -> Schema`
- [37]-[METRICS]: `metrics -> RecordBatch | None` — property; the structured per-operator execution-statistics frame (`id`/`name`/`type`/`category`/`duration` plus a `stats` map keyed by `duration`/`rows.in`/`rows.out`/`bytes.*`), raising until the frame is materialized through `collect`, `None` when the runner attaches no metadata
- [38]-[NUM_PARTITIONS]: `num_partitions() -> int | None` — the resolved partition count after materialization, `None` on the single-partition native runner

[ENTRYPOINT_SCOPE]: `DataFrame` streaming and framework egress
- rail: distributed dataframe

Streaming sinks pull execution incrementally so an out-of-core result never fully materializes in host memory; framework sinks hand the partitioned plan to Ray/Dask/PyTorch without an Arrow roundtrip the consumer must redo. `melt` aliases `unpivot`.

| [INDEX] | [SURFACE]                         | [CAPABILITY]                             |
| :-----: | :-------------------------------- | :--------------------------------------- |
|  [01]   | `DataFrame.iter_partitions`       | stream executed partitions out-of-core   |
|  [02]   | `DataFrame.to_arrow_iter`         | stream Arrow record batches              |
|  [03]   | `DataFrame.to_pylist`             | execute and egress to row dicts          |
|  [04]   | `DataFrame.to_ray_dataset`        | hand partitions to a Ray Dataset         |
|  [05]   | `DataFrame.to_dask_dataframe`     | egress to a Dask dataframe               |
|  [06]   | `DataFrame.to_torch_iter_dataset` | stream rows as a PyTorch IterableDataset |
|  [07]   | `DataFrame.to_torch_map_dataset`  | map-style PyTorch Dataset                |

- [01]-[ITER_PARTITIONS]: `iter_partitions(results_buffer_size='num_cpus') -> Iterator[MicroPartition \| ray.ObjectRef]`
- [02]-[TO_ARROW_ITER]: `to_arrow_iter(results_buffer_size='num_cpus') -> Iterator[pyarrow.RecordBatch]`
- [03]-[TO_PYLIST]: `to_pylist() -> list[dict[str, Any]]`
- [04]-[TO_RAY_DATASET]: `to_ray_dataset() -> ray.data.Dataset`
- [05]-[TO_DASK_DATAFRAME]: `to_dask_dataframe(meta=None) -> dask.dataframe.DataFrame`
- [06]-[TO_TORCH_ITER_DATASET]: `to_torch_iter_dataset() -> torch.utils.data.IterableDataset`
- [07]-[TO_TORCH_MAP_DATASET]: `to_torch_map_dataset() -> torch.utils.data.Dataset`

[ENTRYPOINT_SCOPE]: `Expression`, `DataType`, `Window`, and runner control
- rail: distributed dataframe

`Expression` methods chain typed transforms; `DataType` classmethods mint dtypes; `Window` builds frame specs consumed by `Expression.over`; `set_runner_*`/`session` select the execution backend.

| [INDEX] | [SURFACE]                  | [CAPABILITY]                          |
| :-----: | :------------------------- | :------------------------------------ |
|  [01]   | `Expression.alias`         | rename the output column              |
|  [02]   | `Expression.cast`          | cast to a `DataType`                  |
|  [03]   | `Expression.is_null`       | null-test predicate                   |
|  [04]   | `Expression.is_in`         | membership predicate                  |
|  [05]   | `Expression.between`       | range predicate                       |
|  [06]   | `Expression.if_else`       | conditional select                    |
|  [07]   | `Expression.apply`         | element-wise Python UDF               |
|  [08]   | `Expression.over`          | apply window function over a `Window` |
|  [09]   | `Expression.fill_null`     | null-imputation                       |
|  [10]   | `Expression.not_null`      | non-null predicate                    |
|  [11]   | `Expression.hash`          | stable column hash                    |
|  [12]   | `Expression.json.query`    | jq query over a JSON-string column    |
|  [13]   | `DataType.list`            | variable-length list dtype            |
|  [14]   | `DataType.struct`          | struct dtype from `{name: DataType}`  |
|  [15]   | `DataType.tensor`          | N-D tensor dtype                      |
|  [16]   | `DataType.image`           | image dtype                           |
|  [17]   | `DataType.embedding`       | fixed-size embedding-vector dtype     |
|  [18]   | `DataType.timestamp`       | timestamp dtype                       |
|  [19]   | `DataType.from_arrow_type` | dtype from a pyarrow type             |
|  [20]   | `Window.partition_by`      | window partition keys                 |
|  [21]   | `Window.order_by`          | window ordering                       |
|  [22]   | `Window.rows_between`      | row-based window frame                |
|  [23]   | `set_runner_native`        | select the single-node native runner  |
|  [24]   | `set_runner_ray`           | select the Ray distributed runner     |
|  [25]   | `session`                  | the active catalog/runner session     |
|  [26]   | `get_version`              | installed daft version string         |

- [01]-[ALIAS]: `alias(name) -> Expression`
- [02]-[CAST]: `cast(dtype) -> Expression`
- [03]-[IS_NULL]: `is_null() -> Expression`
- [04]-[IS_IN]: `is_in(other) -> Expression`
- [05]-[BETWEEN]: `between(lower, upper) -> Expression`
- [06]-[IF_ELSE]: `if_else(if_true, if_false) -> Expression`
- [07]-[APPLY]: `apply(func, return_dtype) -> Expression`
- [08]-[OVER]: `over(window) -> Expression`
- [09]-[FILL_NULL]: `fill_null(fill_value) -> Expression`
- [10]-[NOT_NULL]: `not_null() -> Expression`
- [11]-[HASH]: `hash(seed=None) -> Expression`
- [12]-[QUERY]: `json.query(jq_query) -> Expression`
- [13]-[LIST]: `list(dtype) -> DataType`
- [14]-[STRUCT]: `struct(fields) -> DataType`
- [15]-[TENSOR]: `tensor(dtype, shape=None) -> DataType`
- [16]-[IMAGE]: `image(mode=None, height=None, width=None) -> DataType`
- [17]-[EMBEDDING]: `embedding(dtype, size) -> DataType`
- [18]-[TIMESTAMP]: `timestamp(timeunit, timezone=None) -> DataType`
- [19]-[FROM_ARROW_TYPE]: `from_arrow_type(arrow_type, python_fallback=True) -> DataType`
- [20]-[PARTITION_BY]: `partition_by(*cols) -> Window`
- [21]-[ORDER_BY]: `order_by(*cols, desc=False, nulls_first=None) -> Window`
- [22]-[ROWS_BETWEEN]: `rows_between(start, end, min_periods=1) -> Window`
- [23]-[SET_RUNNER_NATIVE]: `set_runner_native(num_threads=None) -> Runner`
- [24]-[SET_RUNNER_RAY]: `set_runner_ray(address=None, noop_if_initialized=False, max_task_backlog=None, force_client_mode=False) -> Runner`
- [25]-[SESSION]: `session() -> Session`
- [26]-[GET_VERSION]: `get_version() -> str`

## [04]-[IMPLEMENTATION_LAW]

[DATAFRAME_ELASTICITY]:
- import: `import daft` at boundary scope only; module-level import is banned by the manifest import policy.
- laziness axis: every `read_*`/`from_*` constructor and every transform returns a lazy `DataFrame` carrying a logical plan; execution happens only at a sink (`collect`, `show`, `count_rows`, `iter_rows`, `to_arrow`/`to_pydict`/`to_pandas`, `write_*`); intermediate `DataFrame` values never materialize the full set, so pushdown and partition-parallel evaluation own the cost.
- expression axis: one `Expression` algebra owns column logic; `col`/`lit`/`coalesce`/`interval`/`list_`/`struct` mint nodes and `str`/`dt`/`list`/`struct`/`map`/`image`/`url`/`embedding`/`binary`/`float`/`json`/`partitioning` sub-namespaces carry typed methods (`json.query` runs a jq query over a JSON-string column), never a parallel per-domain column type; `select`/`where`/`with_columns`/`agg`/`sort`/`join` accept an `Expression` or a column-name `str` interchangeably.
- dtype axis: `DataType` classmethods are the single dtype factory spanning numeric, temporal, `list`/`fixed_size_list`/`struct`/`map`, and multimodal `tensor`/`image`/`embedding`; `from_arrow_type` bridges pyarrow; schema is inferred per reader unless `schema`/`infer_schema=False` pins it.
- ingest axis: `read_*` is the lazy scan rail keyed by format and lakehouse backend (Parquet/CSV/JSON/WARC/MCAP, Delta/Iceberg/Hudi/Lance), threading `io_config` for object-store credentials and `hive_partitioning`/`partition_col` for partition pruning; `from_*` ingests in-memory and framework objects (pydict/pylist/Arrow/pandas/Ray/Dask) zero-copy where the source allows; never a hand-rolled format parser.
- runner axis: `set_runner_native`/`set_runner_ray` select the execution backend as a runner row, not a parallel frame type; the same `DataFrame` plan runs single-node or on a Ray cluster; `Session`/`Catalog`/`Identifier` resolve catalog-scoped tables and SQL, and `sql`/`sql_expr` address the same plan the builder addresses.
- udf axis: `udf(return_dtype=...)` decorates a Python callable into a partition UDF with `num_cpus`/`num_gpus`/`memory_bytes`/`concurrency` resource rows; `Expression.apply` is the element-wise row; UDFs declare `return_dtype`, never an untyped Python escape hatch.
- evidence: each plan captures the resolved output `Schema`, partition count, runner backend, scan source/format, pushdown predicates, and executed row count as a dataframe receipt.
- boundary: daft owns distributed/streaming dataframe execution, partitioning, lakehouse readers, and multimodal dtypes; `to_arrow`/`to_arrow_iter` feed the columnar interop owner (pyarrow/arro3-core); `to_ray_dataset`/`to_dask_dataframe`/`to_torch_*` hand the partitioned plan to the ML/distributed-compute consumer without an Arrow roundtrip; pandas egress is an explicit boundary.

[STACKS_WITH]:
- connectorx: daft's `read_sql(partition_col=, num_partitions=)` owns the lazy distributed SQL scan; `connectorx.read_sql` owns the eager in-process parallel pull — they are the distributed and in-process halves of one SQL-ingest concern, not duplicate readers.
- pyiceberg / deltalake / pylance: daft `read_iceberg`/`read_deltalake`/`read_lance` and `write_iceberg`/`write_deltalake`/`write_lance` are the partition-pushdown lakehouse rail; the dedicated table-format owners own catalog/transaction lifecycle, daft owns the scan/write plan over them.
- dataframely / pandera: the resolved `Schema` and a `to_arrow`/`to_pandas` egress feed a dataframe contract gate; route post-read validation to dataframely (Polars) or pandera, not a hand-rolled assertion pass inside a daft UDF.
- polars / narwhals: narrow, fully-materializable frames egress to the eager Polars owner via `to_arrow`; narwhals provides the engine-agnostic dataframe surface when a consumer must stay backend-neutral. daft owns the out-of-core/distributed plan; do not re-express a daft plan as an eager Polars chain.
- ibis-framework: ibis is the portable logical-plan/SQL-builder front end; daft is one execution backend with a native multimodal engine. Address one engine through the `DataFrame` builder or `sql(**bindings)`, never string-interpolated SQL.
- stamina retry classification: `daft.exceptions.DaftTransientError` (and its `ConnectTimeoutError`/`ReadTimeoutError`/`SocketError`/`ThrottleError`/`ByteStreamError`/`MiscTransientError` subclasses) is the retryable-fault discriminant a `stamina` `retry_context`/`guarded` rail targets for a daft scan/sink leg; a non-transient `DaftCoreException`/`DaftTypeError` fails fast, never a blanket `except Exception` retry.

[RAIL_LAW]:
- Package: `daft`
- Owns: lazy distributed/streaming dataframe execution over partitioned out-of-core data, expression algebra with multimodal dtypes, native and Ray runners, pushdown ingest from Parquet/CSV/JSON/Delta/Iceberg/Hudi/Lance/SQL, catalog/session table resolution, scalar/class UDFs, and SQL over registered frames
- Accept: lazy `DataFrame` plans triggered at a sink, `Expression`/`DataType` as the column and dtype algebra, `read_*`/`from_*` constructors with `io_config` and partition pushdown, `set_runner_native`/`set_runner_ray` for backend selection, `udf(return_dtype=...)` for typed partition UDFs, feeding the `DAFT_ELASTICITY` path and Arrow interop owner
- Reject: wrapper-renames of `read_*`/`select`/`collect`; eager whole-set materialization before a sink; a hand-rolled Parquet/CSV/lakehouse reader where the constructor owns the format; parallel per-format frame or per-domain column types; untyped UDFs without `return_dtype`; duplicate `Get`/`List`/`Read` entry points where one polymorphic constructor discriminates by source; string-interpolated SQL where `sql(**bindings)` binds frames
