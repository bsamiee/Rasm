# [PY_DATA_API_DATAFUSION]

`datafusion` owns the embedded Arrow-native query and federation engine for the data rail: a `SessionContext` registering heterogeneous sources (files, batches, foreign frames, object stores, catalogs), a lazy `DataFrame` algebra compiling to `LogicalPlan`/`ExecutionPlan`, a sync/async `RecordBatchStream` interface, and a `substrait` namespace serializing SQL or a `LogicalPlan` into a portable `Plan`. `datafusion` owns `Plan` interchange for the SUBSTRAIT_PORTABILITY rail, re-implementing neither the Arrow kernels, the SQL planner, nor the Substrait codec.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `datafusion`
- package: `datafusion`
- import: `datafusion`
- owner: `data`
- rail: engine
- entry points: library use is import-only; no console script
- capability: SQL and DataFrame execution over Arrow `RecordBatch` partitions, multi-format reader/writer registration, catalog providers and object-store federation, the four UDF kinds, the `functions`/`functions.spark` namespaces, lazy plan inspection with typed `MetricsSet` receipts, sync/async streaming, zero-copy pandas/polars/pyarrow export, and Substrait `Plan` serialize/deserialize

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine, frame, plan, and interchange roots

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                             |
| :-----: | :---------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `SessionContext`        | engine root       | query execution, table/UDF registration, file reading    |
|  [02]   | `DataFrame`             | lazy frame        | relational algebra compiling to plans and Arrow batches  |
|  [03]   | `RecordBatchStream`     | stream            | sync/async iterable Arrow `RecordBatch` stream           |
|  [04]   | `RecordBatch`           | batch             | wrapper over `pa.RecordBatch` with `to_pyarrow`          |
|  [05]   | `SessionConfig`         | builder           | fluent session-policy builder                            |
|  [06]   | `RuntimeEnvBuilder`     | builder           | fluent disk/memory-pool runtime builder                  |
|  [07]   | `SQLOptions`            | builder           | DDL/DML/statement permission gate                        |
|  [08]   | `Expr`                  | expression        | column/predicate/aggregate expression node               |
|  [09]   | `LogicalPlan`           | plan              | optimized/unoptimized logical plan                       |
|  [10]   | `ExecutionPlan`         | plan              | physical execution plan with per-operator metrics        |
|  [11]   | `MetricsSet`            | metrics           | one operator's runtime metric rollups                    |
|  [12]   | `Metric`                | metrics           | single named metric with value/partition/labels          |
|  [13]   | `DFSchema`              | schema            | qualified plan schema for expression parsing             |
|  [14]   | `Table`                 | table provider    | registered table/view provider                           |
|  [15]   | `Catalog`               | catalog           | named catalog of schemas and tables                      |
|  [16]   | `ScalarUDF`             | udf               | scalar user-defined function                             |
|  [17]   | `AggregateUDF`          | udf               | aggregate user-defined function                          |
|  [18]   | `WindowUDF`             | udf               | window user-defined function                             |
|  [19]   | `TableFunction`         | udtf              | table-returning user-defined function                    |
|  [20]   | `WindowFrame`           | window            | window-frame bound specification                         |
|  [21]   | `Accumulator`           | udf base          | Python aggregate state machine for `udaf`                |
|  [22]   | `WindowEvaluator`       | udf base          | Python window evaluation strategy for `udwf`             |
|  [23]   | `functions` (`f.*`)     | expression ns     | built-in scalar/aggregate/window `Expr` builders         |
|  [24]   | `functions.spark`(`fs.*`)| expression ns    | Spark-semantics scalar/aggregate `Expr` builders         |
|  [25]   | `substrait.Plan`        | interchange       | portable Substrait plan (`encode`/`to_json`/`from_json`) |
|  [26]   | `substrait.Serde`       | interchange codec | SQL <-> `Plan` serialize/deserialize over bytes and path |
|  [27]   | `substrait.Producer`    | interchange codec | `LogicalPlan` -> `Plan`                                  |
|  [28]   | `substrait.Consumer`    | interchange codec | `Plan` -> `LogicalPlan`                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `SessionContext` register, read, and execute

Every surface is a `SessionContext` method, the prefix dropped from each `[CALL_SHAPE]`. A `<provider>` is `Table | TableProviderExportable | DataFrame | pa.dataset.Dataset`, a `<catalog-provider>` is `CatalogProviderExportable | CatalogProvider | Catalog`, an `<arrow-capsule>` is `ArrowStreamExportable | ArrowArrayExportable`; `from_arrow` and its peers `from_pylist`/`from_pydict`/`from_pandas`/`from_polars` adopt Arrow-capsule or foreign-frame data.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                        | [CAPABILITY]                           |
| :-----: | :----------------------------------- | :-------------------------------------------------- | :------------------------------------- |
|  [01]   | `SessionContext`                     | `(config=None, runtime=None)`                       | construct an execution context         |
|  [02]   | `global_ctx`                         | `() -> SessionContext` (classmethod)                | process-wide shared context            |
|  [03]   | `sql`                                | `(query, options=None, param_values=None, **named)` | plan SQL to a lazy `DataFrame` |
|  [04]   | `sql_with_options`                   | `(query, options, param_values=None, **named)`      | lazy frame under DDL/DML gating  |
|  [05]   | `parse_sql_expr`                     | `(sql, schema: DFSchema) -> Expr`                   | parse a SQL fragment into an `Expr`    |
|  [06]   | `create_dataframe_from_logical_plan` | `(plan) -> DataFrame`                               | build from a logical plan              |
|  [07]   | `table`                              | `(name) -> DataFrame`                               | resolve a registered name              |
|  [08]   | `catalog`                            | `(name='datafusion') -> Catalog`                    | access a named catalog                 |
|  [09]   | `execute`                            | `(plan, partitions) -> RecordBatchStream`           | run a physical plan to a stream        |
|  [10]   | `copied_config`                      | `() -> SessionConfig`                               | copy the active session config         |
|  [11]   | `parse_capacity_limit`               | `(config_name, limit) -> int` (staticmethod)        | parse a size string to a byte count    |
|  [12]   | `with_python_udf_inlining`           | `(*, enabled) -> SessionContext`                    | per-session UDF inline/strict toggle   |
|  [13]   | `add_physical_optimizer_rule`        | `(rule) -> None`                                    | append an FFI physical-optimizer rule  |
|  [14]   | `with_logical_extension_codec`       | `(codec)`; peer `with_physical_extension_codec` | plug an FFI plan-serde codec|
|  [15]   | `enable_spark_functions`             | `() -> None`                                        | register Spark-semantics UDFs for SQL  |

Every `read_*` returns a `DataFrame` and shares the source tail `(schema=None, table_partition_cols=None, file_extension, file_compression_type=None, schema_infer_max_records=...)` — also carried by `register_listing_table` — so each row lists only its format-specific keywords.

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                | [CAPABILITY]                    |
| :-----: | :------------- | :-------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `read_parquet` | `(path, parquet_pruning=True, skip_metadata=True, file_sort_order=None)`    | read Parquet into a `DataFrame` |
|  [02]   | `read_csv`     | `(path: str \| Path \| list, has_header=True, delimiter=',', options=None)` | read CSV into a `DataFrame`     |
|  [03]   | `read_json`    | `(path)`                                                                    | read NDJSON into a `DataFrame`  |
|  [04]   | `read_avro`    | `(path, file_partition_cols=None)`                                          | read Avro into a `DataFrame`    |
|  [05]   | `read_table`   | `(table: <provider>)`                                                       | adopt a registered provider     |

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                           | [CAPABILITY]                         |
| :-----: | :-------------------------- | :----------------------------------------------------- | :----------------------------------- |
|  [01]   | `register_listing_table`    | `(name, path, ...) -> None`                            | partitioned directory listing        |
|  [02]   | `register_object_store`     | `(schema, store, host=None) -> None`                   | federate a remote object store       |
|  [03]   | `register_table`            | `(name, table: <provider>) -> None`                    | register a named table/view provider |
|  [04]   | `register_catalog_provider` | `(name, provider: <catalog-provider>) -> None`         | federate a foreign catalog           |
|  [05]   | `register_udf`              | `(udf) -> None` (peers: `register_udaf`/`udwf`/`udtf`) | register the four UDF kinds          |
|  [06]   | `from_arrow`                | `(data: <arrow-capsule>, name=None) -> DataFrame`      | adopt Arrow capsule data             |

[ENTRYPOINT_SCOPE]: `DataFrame` algebra, materialize, and write

`DataFrame` is lazy; every surface is a `DataFrame` method with the prefix dropped, and a transform returns a new `DataFrame` unless the row states otherwise. Nothing executes until `collect`, `show`, `execute_stream`, a `to_*` export, or a `write_*` sink. `join(right, on=None, how='inner', *, left_on=None, right_on=None, join_keys=None, coalesce_duplicate_keys=True)` is the equijoin surface overloaded on key shape; `join_on(right, *on_exprs, how='inner')` joins on arbitrary predicates.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                                                  | [CAPABILITY]                       |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `select`         | `(*exprs: Expr \| str)`                                                       | project columns/expressions        |
|  [02]   | `filter`         | `(*predicates: Expr \| str)`                                                  | conjunctive predicate filter       |
|  [03]   | `aggregate`      | `(group_by, aggs)` — each an `Expr`/`str` or sequence; `group_by=None` global | grouped/global aggregation |
|  [04]   | `join`           | `(right, on=None, how='inner', ...)`                                          | equijoin (overloaded on key shape) |
|  [05]   | `join_on`        | `(right, *on_exprs, how='inner')`                                             | join on arbitrary predicates       |
|  [06]   | `with_column`    | `(name, expr: Expr \| str)`                                                   | append a derived column            |
|  [07]   | `with_columns`   | `(*exprs, **named_exprs)` (peers: `with_column_renamed`, `drop`, `distinct`)  | append/rename/drop a column cohort |
|  [08]   | `sort`           | `(*exprs: SortKey)`                                                           | order rows                         |
|  [09]   | `limit`          | `(count, offset=0)`                                                           | bound and offset rows              |
|  [10]   | `union`          | `(other, distinct)`; peers `intersect`, `except_all`, `union_by_name` | set algebra over frames            |
|  [11]   | `window`         | `(*exprs: Expr)`                                                              | apply window expressions           |
|  [12]   | `unnest_columns` | `(*columns, preserve_nulls=True, recursions=None)`                            | explode list/struct columns        |

- `DataFrame.aggregate`: `group_by=None` or `[]` aggregates over the whole frame; a `GroupingSet` (`rollup`/`cube`/`grouping_sets`) in `group_by` yields multi-level subtotals in one pass.

| [INDEX] | [SURFACE]     | [CALL_SHAPE]                                        | [CAPABILITY]                             |
| :-----: | :------------ | :-------------------------------------------------- | :--------------------------------------- |
|  [01]   | `repartition` | `(num)` (peers: `repartition_by_hash(*exprs, num)`) | partition fan-out for parallel streaming |
|  [02]   | `cache`       | `()`                                                | materialize once, reuse downstream       |
|  [03]   | `fill_null`   | `(value: Any, subset: list[str] \| None = None)`    | replace nulls before egress              |

`collect` peers `head(n)`/`tail(n)`/`count()`/`collect_column(name) -> pa.Array | pa.ChunkedArray`; `to_polars` peers `to_pandas`/`to_pylist`/`to_pydict`; `logical_plan` peers `optimized_logical_plan`/`execution_plan`; `write_parquet` also takes `compression_level` and a `ParquetWriterOptions` override; `write_csv` peers `write_json`/`write_table`.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                               | [CAPABILITY]                          |
| :-----: | :--------------------------- | :--------------------------------------------------------- | :------------------------------------ |
|  [01]   | `collect`                    | `() -> list[pa.RecordBatch]`                               | materialize all partitions to Arrow   |
|  [02]   | `execute_stream`             | `() -> RecordBatchStream`                                  | stream results lazily                 |
|  [03]   | `execute_stream_partitioned` | `() -> list[RecordBatchStream]`                            | one stream per output partition       |
|  [04]   | `to_arrow_table`             | `() -> pa.Table`                                           | materialize to a pyarrow `Table`      |
|  [05]   | `to_polars`                  | `() -> pl.DataFrame`                                       | export to a foreign frame             |
|  [06]   | `logical_plan`               | `() -> LogicalPlan`                                        | inspect plan stages                   |
|  [07]   | `explain`                    | `(verbose=False, analyze=False, format=None) -> None`      | print the plan tree                   |
|  [08]   | `write_parquet`              | `(path, compression=Compression.ZSTD, write_options=None)` | sink to Parquet (overloaded)          |
|  [09]   | `write_csv`                  | `(path, with_header=False, write_options=None)`            | sink to CSV/JSON/registered table     |
|  [10]   | `into_view`                  | `(temporary=False) -> Table`                               | promote a frame to a registrable view |

[ENTRYPOINT_SCOPE]: `ExecutionPlan` runtime metrics

`ExecutionPlan.metrics` returns this operator's `MetricsSet` (`None` before execution); `collect_metrics` walks the tree into `(description, MetricsSet)` pairs ordered outer-to-leaf. `MetricsSet` exposes typed rollups `output_rows`/`elapsed_compute`/`spill_count`/`spilled_bytes`/`spilled_rows` (each `-> int | None`), `sum_by_name(name) -> int | None` for any other, and `metrics() -> list[Metric]`; each `Metric` carries `name`, `value -> int | datetime | None`, `value_as_datetime`, `partition`, and `labels() -> dict`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                        | [CAPABILITY]                        |
| :-----: | :------------------------------ | :--------------------------------- | :---------------------------------- |
|  [01]   | `ExecutionPlan.metrics`         | `() -> MetricsSet \| None`          | this operator's metric set          |
|  [02]   | `ExecutionPlan.collect_metrics` | `() -> list[tuple[str, MetricsSet]]` | per-operator metrics, outer-to-leaf |

[ENTRYPOINT_SCOPE]: streaming, expression, and builder surfaces

`RecordBatchStream` is a synchronous and asynchronous iterator; `next` pulls one `RecordBatch`. `col`/`column` mint a column `Expr`; `lit`/`literal` mint a literal `Expr`, and peers `string_literal`/`str_lit` and `lit_with_metadata`/`literal_with_metadata` carry the typed/metadata variants.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                    | [CAPABILITY]                       |
| :-----: | :----------------------- | :-------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `RecordBatchStream.next` | `() -> RecordBatch` (sync `iter`/`next`, async `aiter`/`anext`) | sync/async batch iteration         |
|  [02]   | `RecordBatch.to_pyarrow` | `() -> pa.RecordBatch`                                          | unwrap to a pyarrow batch          |
|  [03]   | `col` / `column`         | `col(value: str) -> Expr` (attr `col.name` also mints one)      | mint a column-reference expression |
|  [04]   | `lit` / `literal`        | `lit(value: Any) -> Expr`                                       | mint a literal expression          |

`SessionConfig`, `RuntimeEnvBuilder`, and `SQLOptions` return `self` for fluent chaining; each tunes one policy axis.

| [INDEX] | [BUILDER]           | [FLUENT_METHODS]                                                                                              |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `SessionConfig`     | `with_target_partitions(int)`, `with_batch_size`, `with_information_schema`, `with_repartition_joins`, `set`  |
|  [02]   | `RuntimeEnvBuilder` | `with_fair_spill_pool(size)`, `with_greedy_memory_pool`, `with_disk_manager_specified`, `with_temp_file_path` |
|  [03]   | `SQLOptions`        | `with_allow_ddl(allow=True)`, `with_allow_dml`, `with_allow_statements`                                       |

[ENTRYPOINT_SCOPE]: built-in functions and user-defined functions

`functions` is the built-in expression namespace (`import datafusion.functions as f`): scalar/aggregate/window builders return `Expr` and compose with `over(WindowFrame(...))`; aggregate builders carry `distinct=False, filter=None`. `functions.spark` mints Spark-semantics builders (1-indexed `substring`, NULL-propagating `concat`, HALF_UP `round`); `SessionContext.enable_spark_functions` registers the same set for SQL.

`udf`/`udaf`/`udwf` mint the UDF kinds over `Accumulator`/`WindowEvaluator` and a `Volatility` policy, handed to `SessionContext.register_udf`/`register_udaf`/`register_udwf`/`register_udtf`; each takes `(callable/type, input_types, return_type: pa.DataType, volatility: str, name=...)`, `udaf` adding `state_type: list[pa.DataType]`. A `TableFunction(name, func, ctx=None, *, with_session=False)` with `with_session=True` receives the calling `SessionContext` as a `session` kwarg (pure-Python callables only).

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                  | [CAPABILITY]                                   |
| :-----: | :------------------ | :------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `functions` (`f.*`) | `f.sum(expr).over(WindowFrame('rows', start, end))`           | compose built-in SQL functions as `Expr` trees |
|  [02]   | `udf`               | `(func: Callable, ...) -> ScalarUDF`                          | mint a scalar UDF (Arrow-vectorized `func`)    |
|  [03]   | `udaf`              | `(accum: type[Accumulator], ..., state_type) -> AggregateUDF` | mint an aggregate UDF over an `Accumulator`    |
|  [04]   | `udwf`              | `(evaluator: WindowEvaluator, ...) -> WindowUDF`              | mint a window UDF over a `WindowEvaluator`     |

`Accumulator` and `WindowEvaluator` are the two UDF bases a `udaf`/`udwf` subclasses and implements:

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                                                                |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Accumulator`     | `update(*arrays)`, `merge(states)`, `state() -> list[pa.Scalar]`, `evaluate() -> pa.Scalar` |
|  [02]   | `WindowEvaluator` | `evaluate_all(values, num_rows)`, `evaluate(...)`, `evaluate_all_with_rank(...)`            |

[ENTRYPOINT_SCOPE]: `substrait` portability codec

`substrait` round-trips a SQL string or `LogicalPlan` through a portable `Plan`: every `Serde`/`Producer`/`Consumer` method is a staticmethod with the `substrait.` prefix dropped — `Serde` covers SQL <-> bytes/path, `Producer`/`Consumer` bridge the in-process `LogicalPlan`, and `Plan` self-encodes to protobuf bytes or JSON.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                    | [CAPABILITY]                     |
| :-----: | :----------------------------- | :---------------------------------------------- | :------------------------------- |
|  [01]   | `Serde.serialize_to_plan`      | `(sql, ctx) -> Plan`                            | SQL -> portable `Plan`           |
|  [02]   | `Serde.serialize_bytes`        | `(sql, ctx) -> bytes`                           | SQL -> protobuf bytes            |
|  [03]   | `Serde.serialize`              | `(sql, ctx, path: str \| pathlib.Path) -> None` | SQL -> plan written to path      |
|  [04]   | `Serde.deserialize`            | `(path: str \| pathlib.Path) -> Plan`           | path -> `Plan`                   |
|  [05]   | `Serde.deserialize_bytes`      | `(proto_bytes: bytes) -> Plan`                  | protobuf bytes -> `Plan`         |
|  [06]   | `Producer.to_substrait_plan`   | `(logical_plan: LogicalPlan, ctx) -> Plan`      | `LogicalPlan` -> portable `Plan` |
|  [07]   | `Consumer.from_substrait_plan` | `(ctx, plan: Plan) -> LogicalPlan`              | portable `Plan` -> `LogicalPlan` |
|  [08]   | `Plan.encode`                  | `() -> bytes`                                   | `Plan` -> protobuf bytes         |
|  [09]   | `Plan.to_json`                 | `() -> str`                                     | `Plan` -> JSON                   |
|  [10]   | `Plan.from_json`               | `(json: str) -> Plan`                           | JSON -> `Plan`                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `SessionContext` owns query execution and source registration; `SessionConfig`/`RuntimeEnvBuilder`/`SQLOptions` thread policy into construction or `sql_with_options`.
- `register_*`/`read_*` is one ingestion family discriminated by source kind — file format, object store, in-memory batch, foreign catalog, table provider, UDF; CSV/Parquet/JSON/Avro/Arrow are rows.
- `DataFrame` is lazy relational algebra: `select`/`filter`/`aggregate`/`join`/`window`/`union` return new frames and execute nothing until `collect`, `show`, an `execute_stream*`, a `to_*` export, or a `write_*` sink; plan inspection routes through `logical_plan`/`optimized_logical_plan`/`execution_plan`.
- `execute_stream` and `SessionContext.execute` yield a sync/async iterable `RecordBatchStream` under backpressured `RecordBatch` pull; `execute_stream_partitioned` fans one stream per output partition.
- `col`/`column` and `lit` mint `Expr` nodes; predicates and projections are `Expr` trees or `parse_sql_expr` fragments over untrusted SQL text.
- `udf`/`udaf`/`udwf` are the three UDF factories over `Accumulator`/`WindowEvaluator` and a volatility policy, routing to `register_udf`/`register_udaf`/`register_udwf`/`register_udtf`; the `functions` namespace composes built-in `Expr` trees ahead of any Python UDF.
- `substrait` owns the portable `Plan`: `Serde` serializes SQL, `Producer`/`Consumer` bridge a `LogicalPlan`, and `Plan.encode`/`to_json`/`from_json` carry the protobuf/JSON wire for the SUBSTRAIT_PORTABILITY rail.
- `to_arrow_table`/`to_pandas`/`to_polars`/`to_pylist`/`to_pydict` are zero-copy or near-zero-copy Arrow exports over the shared C-data capsule.
- `SessionContext` extends the engine over FFI: `add_physical_optimizer_rule` appends a compiled physical-optimizer rule, `with_logical_extension_codec`/`with_physical_extension_codec` plug plan-serialization codecs, `enable_spark_functions` overrides built-ins with Spark semantics, and `with_python_udf_inlining(enabled=False)` disables UDF inlining and hardens `Expr.from_bytes` deserialization.
- Each execution captures the engine receipt: session id, plan stage (logical/optimized/physical), partition count, and Substrait `Plan` byte length, with per-operator runtime metrics read post-execution through `ExecutionPlan.metrics`/`collect_metrics` as a typed `MetricsSet` (output rows, elapsed compute, spill counts) walked outer-to-leaf.
- `pyarrow` owns the `RecordBatch`/`Schema` wire types at the seam; downstream owners consume `pa.RecordBatch`/`pa.Table` or a portable `Plan`, never the `_internal` Rust handles.

[STACKING]:
- `duckdb`(`.api/duckdb.md`) / `substrait`(`.api/substrait.md`): `substrait.Producer.to_substrait_plan` emits the wire `Plan` DuckDB `con.from_substrait(plan.encode())` re-binds, and a DuckDB `con.get_substrait(sql)` BLOB re-binds through `substrait.Consumer.from_substrait_plan`; one portable `Plan` and its JSON twin cross both engines.
- `polars`(`.api/polars.md`) / `pyarrow`(`.api/pyarrow.md`): `from_arrow`/`from_polars`/`from_pandas` ingest and `to_arrow_table`/`to_polars`/`to_pandas` egress over the shared Arrow C-data capsule, so federation is provider registration rather than frame materialization.
- `deltalake`(`.api/deltalake.md`): a `DeltaTable.to_pyarrow_dataset()` registers via `register_table`/`read_table` as a pushdown-capable `pa.dataset.Dataset`, joining Parquet listings and object-store CSV under one `sql` with predicate/column pruning pushed into the Delta scan.
- within-lib: a federated `execute_stream` over a remote object store composes under a `stamina` `retry_context` and an OpenTelemetry span keyed by the engine receipt (session id, plan byte length, `ExecutionPlan.collect_metrics` output rows), threading one instrumented, retried streaming pull.

[LOCAL_ADMISSION]:
- import `datafusion` at boundary scope only; the branch admits it as the sole Arrow-native SQL/DataFrame engine over registered sources.

[RAIL_LAW]:
- Package: `datafusion`
- Owns: Arrow-native SQL and DataFrame execution, multi-format and object-store/catalog federation, the four UDF kinds via `udf`/`udaf`/`udwf` over `Accumulator`/`WindowEvaluator`, the `functions` and `functions.spark` namespaces, lazy plan inspection, typed per-operator execution metrics via `ExecutionPlan.metrics`/`MetricsSet`, FFI physical-optimizer-rule and plan-codec extension, sync/async `RecordBatchStream` streaming, and Substrait `Plan` serialize/deserialize
- Accept: federated query and streaming over registered sources (a `deltalake` `to_pyarrow_dataset` provider, a DuckDB Arrow reader), Spark-semantics execution via `functions.spark`/`enable_spark_functions`, and Substrait plan interchange round-tripping the same wire `Plan` with `duckdb`/`substrait` for the SUBSTRAIT_PORTABILITY rail
- Reject: wrapper-renames of `SessionContext`/`DataFrame`/`substrait`; a hand-rolled SQL planner, execution kernel, or Substrait protobuf codec; a metrics scraper over `explain`/`display` text where `ExecutionPlan.metrics` returns a typed `MetricsSet`; a Spark-compatibility shim where `functions.spark` owns the semantics; a reader type per file format where `register_*`/`read_*` rows suffice; a parallel context type per policy where the fluent builders own the axis; a UDF builder per kind where `udf`/`udaf`/`udwf` own the axis; re-serializing a plan per engine where one portable `Plan` crosses both; raw `_internal` handles crossing the package boundary
