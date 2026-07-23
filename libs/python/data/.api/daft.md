# [PY_DATA_API_DAFT]

`daft` is a Rust-backed distributed and streaming dataframe engine for multimodal, out-of-core data: a lazy `DataFrame` builds a logical plan from `Expression` nodes and defers execution to a sink, so pushdown and partition-parallel evaluation own the cost on the native or Ray runner. A typed `DataType` vocabulary — numeric, temporal, nested, and multimodal `tensor`/`image`/`embedding` — backs every column, and `read_*`/`from_*` constructors ingest the file, lakehouse, SQL, and Arrow sources without eager materialization.

`data` composes `DataFrame`, `Expression`, and the `read_*` rail into the `DAFT_ELASTICITY` path, never re-implementing the execution, partitioning, or lakehouse readers daft owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `daft`
- package: `daft` (Apache-2.0)
- import: `import daft`
- owner: `data`
- rail: distributed dataframe
- capability: lazy logical-plan dataframes over partitioned out-of-core data on native and Ray runners; pushdown; numeric, temporal, nested, and multimodal `tensor`/`image`/`embedding` dtypes; a flat typed `Expression` algebra; Parquet/CSV/JSON/WARC IO with Delta/Iceberg/Hudi/Lance lakehouse readers and writers; catalog and session resolution; SQL over registered frames; scalar and class UDFs; Arrow/pandas/Ray/Dask/PyTorch interop and streaming row/batch/partition iterators

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame, expression, schema, catalog, and config types

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                         |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `DataFrame`        | lazy frame        | deferred logical-plan table over partitioned data    |
|  [02]   | `Expression`       | expression node   | composable typed column node over a flat method set  |
|  [03]   | `DataType`         | dtype factory     | numeric/temporal/nested/multimodal type constructor  |
|  [04]   | `Schema`           | schema value      | ordered `{name: DataType}` field mapping             |
|  [05]   | `Series`           | typed column      | named typed 1-D column over Arrow buffers            |
|  [06]   | `Window`           | window spec       | partition/order/frame builder for `Expression.over`  |
|  [07]   | `Session`          | session context   | catalog/namespace/runner-scoped table and SQL access |
|  [08]   | `Catalog`          | catalog binding   | Iceberg/Unity/Glue/S3Tables/pydict table provider    |
|  [09]   | `Table`            | catalog table     | catalog-resolved readable and writable table         |
|  [10]   | `Identifier`       | qualified name    | namespace-qualified catalog identifier               |
|  [11]   | `DataCatalogTable` | catalog table ref | external data-catalog table reference                |
|  [12]   | `DataCatalogType`  | enum              | data-catalog backend selector                        |
|  [13]   | `IOConfig`         | IO config         | object-store credentials and IO policy carrier       |
|  [14]   | `ResourceRequest`  | resource spec     | per-task CPU/GPU/memory resource request             |
|  [15]   | `File`             | file handle       | multimodal file-column reference                     |
|  [16]   | `VideoFile`        | file handle       | video file-column reference for frame decode         |
|  [17]   | `TimeUnit`         | dtype unit        | timestamp/duration time-unit vocabulary              |
|  [18]   | `MediaType`        | media vocabulary  | media-type vocabulary for file columns               |
|  [19]   | `ImageMode`        | image vocabulary  | image channel and mode vocabulary                    |
|  [20]   | `ImageFormat`      | image vocabulary  | image encoding-format vocabulary                     |
|  [21]   | `ImageProperty`    | image vocabulary  | image property selector vocabulary                   |

[PUBLIC_TYPE_SCOPE]: exception hierarchy

`daft.exceptions` roots every fault at `DaftCoreException` (subclasses `ValueError`); the `DaftTransientError` branch is the retry discriminant a `stamina` rail targets.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                            |
| :-----: | :------------------- | :------------- | :-------------------------------------- |
|  [01]   | `DaftCoreException`  | base error     | root of every daft exception            |
|  [02]   | `DaftTransientError` | transient base | retryable 429/5xx/timeout network fault |
|  [03]   | `DaftTypeError`      | type error     | non-transient schema or dtype misuse    |

[TRANSIENT_SUBCLASS]: `ConnectTimeoutError` `ReadTimeoutError` `SocketError` `ThrottleError` `ByteStreamError` `MiscTransientError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest and source constructors

`read_*` and `from_*` return a lazy `DataFrame` and `write_table` returns `None`; `read_*` scan a source with pushdown, `from_*` ingest in-memory or framework objects.
- `read_*` carry: `io_config`, `schema`, `infer_schema`, `file_path_column`, `hive_partitioning`

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `read_parquet(path, *, row_groups, coerce_int96_timestamp_unit)` | factory | lazy Parquet scan with row-group pushdown |
|  [02]   | `read_csv(path, *, has_headers, delimiter, quote, comment)`      | factory | lazy CSV scan with dialect control        |
|  [03]   | `read_json(path)`                                                | factory | lazy newline-delimited JSON scan          |
|  [04]   | `read_deltalake(table, *, version)`                              | factory | Delta Lake read at version or timestamp   |
|  [05]   | `read_iceberg(table, *, snapshot_id)`                            | factory | Iceberg read at snapshot                  |
|  [06]   | `read_hudi(table_uri)`                                           | factory | Hudi table read                           |
|  [07]   | `read_lance(url, *, version, asof, block_size)`                  | factory | Lance dataset read                        |
|  [08]   | `read_sql(sql, conn, *, partition_col, num_partitions)`          | factory | partitioned SQL read with pushdown        |
|  [09]   | `read_warc(path)`                                                | factory | WARC web-archive scan                     |
|  [10]   | `read_mcap(path, *, topics, start_time, end_time, batch_size)`   | factory | MCAP robotics-log scan by topic and time  |
|  [11]   | `read_table(identifier, **options)`                              | factory | read a catalog-registered table           |
|  [12]   | `from_glob_path(path, *, io_config)`                             | factory | file-listing frame from a glob            |
|  [13]   | `from_pydict(data)`                                              | factory | in-memory frame from a column dict        |
|  [14]   | `from_pylist(data)`                                              | factory | in-memory frame from row dicts            |
|  [15]   | `from_arrow(data)`                                               | factory | zero-copy frame from Arrow tables         |
|  [16]   | `from_pandas(data)`                                              | factory | frame from pandas DataFrames              |
|  [17]   | `write_table(identifier, df, *, mode, **options)`                | static  | write a frame to a catalog table          |

[ENTRYPOINT_SCOPE]: expression and SQL factories

`col`/`lit`/`element`/`interval`/`sql_expr` and the `daft.functions` builders return an `Expression`; `when` returns a `WhenExpr`, `sql` returns a `DataFrame`, and `udf` returns the partition-UDF decorator.

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `col(name)`                                                                | factory | reference a column by name                     |
|  [02]   | `lit(value)`                                                               | factory | literal-value expression                       |
|  [03]   | `element()`                                                                | factory | element placeholder for list operations        |
|  [04]   | `interval(*, years, months, days, hours, minutes, seconds, millis, nanos)` | factory | temporal interval literal                      |
|  [05]   | `functions.struct(*fields)`                                                | factory | construct a struct column from fields          |
|  [06]   | `functions.coalesce(*exprs)`                                               | factory | first-non-null across expressions              |
|  [07]   | `functions.when(condition, then)`                                          | factory | conditional chained with `.otherwise(default)` |
|  [08]   | `sql(sql, *, register_globals, **bindings)`                                | factory | run SQL over bound or global frames            |
|  [09]   | `sql_expr(sql)`                                                            | factory | parse a SQL fragment into an expression        |
|  [10]   | `udf(*, return_dtype, num_cpus, num_gpus, concurrency)`                    | factory | decorate a callable into a partition UDF       |

[ENTRYPOINT_SCOPE]: `DataFrame` transform and sink

Transform methods return a new lazy `DataFrame`; `collect`/`show`/`count_rows`/`to_*`/`iter_rows`/`write_*` are sinks that execute the plan. A column argument accepts an `Expression` or a `str` name interchangeably.

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `select(*columns, **projections)`                                                  | instance | project columns and named expressions   |
|  [02]   | `where(predicate)`                                                                 | instance | filter rows (`filter` alias)            |
|  [03]   | `with_column(name, expr)`                                                          | instance | add or replace one computed column      |
|  [04]   | `with_columns(columns)`                                                            | instance | add or replace columns from a dict      |
|  [05]   | `exclude(*names)`                                                                  | instance | drop named columns                      |
|  [06]   | `sort(by, *, desc, nulls_first)`                                                   | instance | order rows by columns or expressions    |
|  [07]   | `limit(num)`                                                                       | instance | cap row count                           |
|  [08]   | `distinct(*on)`                                                                    | instance | deduplicate rows, optionally on keys    |
|  [09]   | `join(other, *, on, left_on, right_on, how, strategy)`                             | instance | relational join with strategy hint      |
|  [10]   | `groupby(*group_by) -> GroupedDataFrame`                                           | instance | group rows for aggregation              |
|  [11]   | `agg(*to_agg)`                                                                     | instance | global aggregation over expressions     |
|  [12]   | `explode(*columns)`                                                                | instance | unnest list columns to rows             |
|  [13]   | `unpivot(ids, values, *, variable_name, value_name)`                               | instance | wide-to-long reshape (`melt` alias)     |
|  [14]   | `sample(fraction, *, with_replacement, seed)`                                      | instance | random row sample                       |
|  [15]   | `repartition(num, *partition_by)`                                                  | instance | hash or round-robin repartition         |
|  [16]   | `into_partitions(num)`                                                             | instance | coalesce or split into N partitions     |
|  [17]   | `pivot(group_by, pivot_col, value_col, agg_fn, *, names)`                          | instance | long-to-wide pivot with aggregation     |
|  [18]   | `transform(func, *args, **kwargs)`                                                 | instance | apply a `DataFrame -> DataFrame` fn     |
|  [19]   | `agg_list(*cols)`                                                                  | instance | global list aggregation                 |
|  [20]   | `collect(*, num_preview_rows)`                                                     | instance | execute and cache the result            |
|  [21]   | `show(n, *, format, max_width, columns)`                                           | instance | execute and render a preview            |
|  [22]   | `count_rows() -> int`                                                              | instance | execute and count rows                  |
|  [23]   | `iter_rows(*, results_buffer_size, column_format)`                                 | instance | stream rows out-of-core                 |
|  [24]   | `to_arrow() -> pyarrow.Table`                                                      | instance | execute and egress to Arrow             |
|  [25]   | `to_pydict() -> dict`                                                              | instance | execute and egress to a column dict     |
|  [26]   | `to_pandas(*, coerce_temporal_nanoseconds)`                                        | instance | execute and egress to pandas            |
|  [27]   | `write_parquet(root_dir, *, compression, write_mode, partition_cols, single_file)` | instance | partitioned Parquet sink                |
|  [28]   | `write_deltalake(table, *, mode, schema_mode, partition_cols)`                     | instance | Delta Lake table sink                   |
|  [29]   | `write_iceberg(table, *, mode)`                                                    | instance | Iceberg table sink                      |
|  [30]   | `write_csv(root_dir, *, write_mode, partition_cols)`                               | instance | partitioned CSV sink                    |
|  [31]   | `write_json(root_dir, *, write_mode, partition_cols)`                              | instance | partitioned newline-JSON sink           |
|  [32]   | `write_lance(uri, *, mode)`                                                        | instance | Lance dataset sink                      |
|  [33]   | `summarize() -> DataFrame`                                                         | instance | per-column statistics frame             |
|  [34]   | `describe() -> DataFrame`                                                          | instance | schema as a frame                       |
|  [35]   | `explain(*, show_all, format, simple)`                                             | instance | print the logical or physical plan      |
|  [36]   | `schema() -> Schema`                                                               | instance | resolved output schema                  |
|  [37]   | `metrics`                                                                          | property | per-op stats frame; `None` unattached   |
|  [38]   | `num_partitions()`                                                                 | instance | resolved partition count post-execution |

- `metrics`: `RecordBatch` of per-operator execution stats, raising until `collect` materializes the plan.
- `num_partitions()`: `None` on the single-partition native runner; `agg_set`/`agg_concat` mirror `agg_list` for set-dedupe and concat aggregation.

[ENTRYPOINT_SCOPE]: `DataFrame` streaming and framework egress

Streaming sinks pull execution incrementally so an out-of-core result never fully materializes; framework sinks hand the partitioned plan to Ray/Dask/PyTorch without an Arrow roundtrip.

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `iter_partitions(*, results_buffer_size)` | instance | stream partitions (`ray.ObjectRef` on Ray) |
|  [02]   | `to_arrow_iter(*, results_buffer_size)`   | instance | stream Arrow record batches                |
|  [03]   | `to_pylist() -> list`                     | instance | execute and egress to row dicts            |
|  [04]   | `to_ray_dataset() -> ray.data.Dataset`    | instance | hand partitions to a Ray Dataset           |
|  [05]   | `to_dask_dataframe(*, meta)`              | instance | egress to a Dask dataframe                 |
|  [06]   | `to_torch_iter_dataset()`                 | instance | stream rows as a PyTorch IterableDataset   |
|  [07]   | `to_torch_map_dataset()`                  | instance | map-style PyTorch Dataset                  |

[ENTRYPOINT_SCOPE]: `Expression`, `DataType`, `Window`, and runner control

`Expression` methods chain typed transforms over a flat method surface; `DataType` classmethods mint dtypes; `Window` builds frame specs consumed by `Expression.over`; `set_runner_*` and `session` select the execution backend.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Expression.alias(name)`                                   | instance | rename the output column                |
|  [02]   | `Expression.cast(dtype)`                                   | instance | cast to a `DataType`                    |
|  [03]   | `Expression.is_null()`                                     | instance | null-test predicate                     |
|  [04]   | `Expression.is_in(other)`                                  | instance | membership predicate                    |
|  [05]   | `Expression.between(lower, upper)`                         | instance | range predicate                         |
|  [06]   | `Expression.apply(func, return_dtype)`                     | instance | element-wise Python UDF                 |
|  [07]   | `Expression.over(window)`                                  | instance | apply a window function over a `Window` |
|  [08]   | `Expression.fill_null(fill_value)`                         | instance | null imputation                         |
|  [09]   | `Expression.not_null()`                                    | instance | non-null predicate                      |
|  [10]   | `Expression.hash(*, seed)`                                 | instance | stable column hash                      |
|  [11]   | `Expression.jq(filter)`                                    | instance | jq query over a JSON-string column      |
|  [12]   | `DataType.list(dtype)`                                     | factory  | variable-length list dtype              |
|  [13]   | `DataType.struct(fields)`                                  | factory  | struct dtype from `{name: DataType}`    |
|  [14]   | `DataType.tensor(dtype, *, shape)`                         | factory  | N-D tensor dtype                        |
|  [15]   | `DataType.image(*, mode, height, width)`                   | factory  | image dtype                             |
|  [16]   | `DataType.embedding(dtype, size)`                          | factory  | fixed-size embedding-vector dtype       |
|  [17]   | `DataType.timestamp(timeunit, *, timezone)`                | factory  | timestamp dtype                         |
|  [18]   | `DataType.from_arrow_type(arrow_type, *, python_fallback)` | factory  | dtype from a pyarrow type               |
|  [19]   | `Window.partition_by(*cols)`                               | factory  | window partition keys                   |
|  [20]   | `Window.order_by(*cols, *, desc, nulls_first)`             | factory  | window ordering                         |
|  [21]   | `Window.rows_between(start, end, *, min_periods)`          | factory  | row-based window frame                  |
|  [22]   | `set_runner_native(*, num_threads) -> Runner`              | static   | select the single-node native runner    |
|  [23]   | `set_runner_ray(*, address) -> Runner`                     | static   | select the Ray distributed runner       |
|  [24]   | `session() -> Session`                                     | static   | the active catalog and runner session   |
|  [25]   | `get_version() -> str`                                     | static   | installed daft version string           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `read_*`/`from_*` constructor and every transform returns a lazy `DataFrame` carrying a logical plan; execution fires only at a sink (`collect`, `show`, `count_rows`, `iter_rows`, `to_*`, `write_*`), so intermediate frames never materialize the full set and pushdown and partition-parallel evaluation own the cost.
- One `Expression` algebra owns column logic as a flat typed method surface (`list_*`, `image_*`, `regexp_*`, `jq`, predicates, and `over`); `col`/`lit`/`element`/`interval` and `functions.struct`/`coalesce`/`when` mint nodes, and `select`/`where`/`with_columns`/`agg`/`sort`/`join` accept an `Expression` or a column-name `str` interchangeably.
- `DataType` classmethods are the single dtype factory spanning numeric, temporal, `list`/`struct`/`map`, and multimodal `tensor`/`image`/`embedding`; `from_arrow_type` bridges pyarrow, and schema infers per reader unless `schema`/`infer_schema=False` pins it.
- `read_*` is the lazy scan rail keyed by format and lakehouse backend, threading `io_config` for credentials and `hive_partitioning`/`partition_col` for pruning; `from_*` ingests in-memory and framework objects zero-copy where the source admits.
- `set_runner_native`/`set_runner_ray` select the backend as a runner row, not a parallel frame type — the same plan runs single-node or on Ray; `Session`/`Catalog`/`Identifier` resolve catalog-scoped tables, and `sql`/`sql_expr` address the plan the builder addresses.
- `udf(return_dtype=...)` decorates a callable into a partition UDF carrying `num_cpus`/`num_gpus`/`memory_bytes`/`concurrency` resource rows, and `Expression.apply` is the element-wise row; every UDF declares `return_dtype`.
- Each plan captures the resolved `Schema`, partition count, runner backend, scan source and format, pushdown predicates, and executed row count as a dataframe receipt.

[STACKING]:
- `connectorx`(`.api/connectorx.md`): daft `read_sql(partition_col=, num_partitions=)` owns the lazy distributed SQL scan and `connectorx.read_sql` owns the eager in-process parallel pull — the distributed and in-process halves of one SQL-ingest concern.
- `pyiceberg`(`.api/pyiceberg.md`) / `deltalake`(`.api/deltalake.md`) / `pylance`(`.api/pylance.md`): daft `read_*`/`write_*` are the partition-pushdown lakehouse scan-and-write plan; the table-format owners hold catalog and transaction lifecycle beneath it.
- `dataframely`(`.api/dataframely.md`) / `pandera`(`.api/pandera.md`): the resolved `Schema` and a `to_arrow`/`to_pandas` egress feed a dataframe contract gate; post-read validation routes here, never a hand-rolled assertion inside a daft UDF.
- `polars`(`.api/polars.md`) / `narwhals`(`.api/narwhals.md`): a narrow, fully-materializable frame egresses to eager Polars via `to_arrow` and narwhals carries the engine-agnostic surface for a backend-neutral consumer; daft owns the out-of-core plan, which never re-expresses as an eager Polars chain.
- `ibis-framework`(`.api/ibis-framework.md`): ibis is the portable logical-plan front end and daft one execution backend; address the engine through the `DataFrame` builder or `sql(**bindings)`, never string-interpolated SQL.
- `stamina` (`libs/python/runtime/.api/stamina.md`): `DaftTransientError` and its subclasses are the retryable-fault discriminant a `stamina` `retry_context` targets for a scan or sink leg, and `DaftCoreException`/`DaftTypeError` fail fast.
- within `data`: the folder composes `read_*`, `Expression`, and a sink into the `DAFT_ELASTICITY` path; `to_arrow`/`to_arrow_iter` feed the Arrow interop carrier and `to_ray_dataset`/`to_dask_dataframe`/`to_torch_*` hand the partitioned plan to the ML and distributed consumer without an Arrow roundtrip.

[LOCAL_ADMISSION]:
- daft admits into `data` as the sole distributed and streaming dataframe owner; `import daft` at boundary scope only, module-level import banned by the manifest import policy.

[RAIL_LAW]:
- Package: `daft`
- Owns: lazy distributed and streaming dataframe execution over partitioned out-of-core data, a flat typed `Expression` algebra with multimodal dtypes, native and Ray runners, pushdown ingest from Parquet/CSV/JSON/Delta/Iceberg/Hudi/Lance/SQL, catalog and session table resolution, scalar and class UDFs, and SQL over registered frames.
- Accept: lazy `DataFrame` plans triggered at a sink, `Expression`/`DataType` as the column and dtype algebra, `read_*`/`from_*` with `io_config` and partition pushdown, `set_runner_native`/`set_runner_ray` for backend selection, `udf(return_dtype=...)` for typed partition UDFs, feeding the `DAFT_ELASTICITY` path and the Arrow interop owner.
- Reject: wrapper-renames of `read_*`/`select`/`collect`; eager whole-set materialization before a sink; a hand-rolled Parquet/CSV/lakehouse reader where the constructor owns the format; parallel per-format frame or per-domain column types; untyped UDFs without `return_dtype`; duplicate `Get`/`List`/`Read` entry points where one polymorphic constructor discriminates by source; string-interpolated SQL where `sql(**bindings)` binds frames.
