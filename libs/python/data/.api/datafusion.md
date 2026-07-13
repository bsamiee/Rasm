# [PY_DATA_API_DATAFUSION]

`datafusion` supplies the embedded Arrow-native query and federation engine for the data engine rail: a `SessionContext` that registers heterogeneous tables (CSV/Parquet/JSON/Avro/Arrow, in-memory batches, pandas/polars/pyarrow, listing directories, object stores, and foreign catalogs), a lazy `DataFrame` algebra that compiles to a `LogicalPlan`/`ExecutionPlan`, a `RecordBatchStream` push-pull streaming interface, and a `substrait` namespace that serializes a SQL string or `LogicalPlan` into a portable `Plan`. The package owner composes `SessionContext`, `DataFrame`, `RecordBatchStream`, and `substrait.Producer`/`substrait.Consumer` into the federation and streaming paths; it owns Substrait plan interchange for the SUBSTRAIT_PORTABILITY rail and never re-implements the Arrow execution kernels, the SQL planner, or the Substrait codec datafusion already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `datafusion`
- package: `datafusion`
- import: `datafusion`
- owner: `data`
- rail: engine
- version: `53.0.0`
- entry points: library use is import-only; no console script
- capability: SQL and DataFrame query execution over Arrow `RecordBatch` partitions, multi-format reader/writer registration (CSV/Parquet/JSON/Avro/Arrow), in-memory and foreign-catalog table providers, object-store federation, scalar/aggregate/window/table user-defined functions via `udf`/`udaf`/`udwf` factories over `Accumulator`/`WindowEvaluator` bases, the `functions` built-in expression namespace, lazy `LogicalPlan`/`ExecutionPlan` inspection, sync and async `RecordBatchStream` streaming, zero-copy export to pandas/polars/pyarrow, and Substrait `Plan` serialize/deserialize for cross-engine portability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine, frame, plan, and interchange roots
- rail: engine

`SessionContext` is the single execution and registration root; `DataFrame` is the lazy relational algebra that collects to Arrow `RecordBatch` lists or a `RecordBatchStream`. `SessionConfig`, `RuntimeEnvBuilder`, and `SQLOptions` are fluent builders gating session, runtime, and SQL policy. `LogicalPlan`/`ExecutionPlan` are the inspectable plan stages; the `substrait` namespace carries the portable `Plan` plus its codec surfaces.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                                                   |
| :-----: | :------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `SessionContext`     | engine root       | query execution, table/UDF registration, file reading    |
|  [02]   | `DataFrame`          | lazy frame        | relational algebra compiling to plans and Arrow batches  |
|  [03]   | `RecordBatchStream`  | stream            | sync/async iterable Arrow `RecordBatch` stream           |
|  [04]   | `RecordBatch`        | batch             | wrapper over `pa.RecordBatch` with `to_pyarrow`          |
|  [05]   | `SessionConfig`      | builder           | fluent session-policy builder                            |
|  [06]   | `RuntimeEnvBuilder`  | builder           | fluent disk/memory-pool runtime builder                  |
|  [07]   | `SQLOptions`         | builder           | DDL/DML/statement permission gate                        |
|  [08]   | `Expr`               | expression        | column/predicate/aggregate expression node               |
|  [09]   | `LogicalPlan`        | plan              | optimized/unoptimized logical plan                       |
|  [10]   | `ExecutionPlan`      | plan              | physical execution plan                                  |
|  [11]   | `DFSchema`           | schema            | qualified plan schema for expression parsing             |
|  [12]   | `Table`              | table provider    | registered table/view provider                           |
|  [13]   | `Catalog`            | catalog           | named catalog of schemas and tables                      |
|  [14]   | `ScalarUDF`          | udf               | scalar user-defined function                             |
|  [15]   | `AggregateUDF`       | udf               | aggregate user-defined function                          |
|  [16]   | `WindowUDF`          | udf               | window user-defined function                             |
|  [17]   | `TableFunction`      | udtf              | table-returning user-defined function                    |
|  [18]   | `WindowFrame`        | window            | window-frame bound specification                         |
|  [19]   | `Accumulator`        | udf base          | Python aggregate state machine for `udaf`                |
|  [20]   | `WindowEvaluator`    | udf base          | Python window evaluation strategy for `udwf`             |
|  [21]   | `functions` (`f.*`)  | expression ns     | built-in scalar/aggregate/window `Expr` builders         |
|  [22]   | `substrait.Plan`     | interchange       | portable Substrait plan (`encode`/`to_json`/`from_json`) |
|  [23]   | `substrait.Serde`    | interchange codec | SQL <-> `Plan` serialize/deserialize over bytes and path |
|  [24]   | `substrait.Producer` | interchange codec | `LogicalPlan` -> `Plan`                                  |
|  [25]   | `substrait.Consumer` | interchange codec | `Plan` -> `LogicalPlan`                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `SessionContext` register, read, and execute
- rail: engine

Every surface below is a `SessionContext` method (prefix dropped), and each `[CALL_SHAPE]` omits the leading method name. `SessionContext()` accepts an optional `SessionConfig` and `RuntimeEnvBuilder`; `global_ctx` returns the process-wide context. A provider argument is `Table | TableProviderExportable | DataFrame | pa.dataset.Dataset`; a catalog-provider argument is `CatalogProviderExportable | CatalogProvider | Catalog`; an Arrow-capsule argument is `ArrowStreamExportable | ArrowArrayExportable`. `from_arrow` adopts Arrow capsule data and its peers `from_pylist`/`from_pydict`/`from_pandas`/`from_polars` adopt the matching in-memory/foreign frame.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                        | [CAPABILITY]                           |
| :-----: | :----------------------------------- | :-------------------------------------------------- | :------------------------------------- |
|  [01]   | `SessionContext`                     | `(config=None, runtime=None)`                       | construct an execution context         |
|  [02]   | `global_ctx`                         | `() -> SessionContext` (classmethod)                | process-wide shared context            |
|  [03]   | `sql`                                | `(query, options=None, param_values=None, **named)` | plan a SQL query to a lazy `DataFrame` |
|  [04]   | `sql_with_options`                   | `(query, options, param_values=None, **named)`      | lazy `DataFrame` under DDL/DML gating  |
|  [05]   | `parse_sql_expr`                     | `(sql, schema: DFSchema) -> Expr`                   | parse a SQL fragment into an `Expr`    |
|  [06]   | `create_dataframe_from_logical_plan` | `(plan) -> DataFrame`                               | build from a logical plan              |
|  [07]   | `table`                              | `(name) -> DataFrame`                               | resolve a registered name              |
|  [08]   | `catalog`                            | `(name='datafusion') -> Catalog`                    | access a named catalog                 |
|  [09]   | `execute`                            | `(plan, partitions) -> RecordBatchStream`           | run a physical plan to a stream        |

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
- rail: engine

`DataFrame` is lazy; every surface below is a `DataFrame` method (prefix dropped), each `[CALL_SHAPE]` omits the leading method name, and a transform returns a new `DataFrame` unless the row states another return. Nothing executes until `collect`, `show`, `execute_stream`, a `to_*` export, or a `write_*` sink. The equijoin surface is `join(right, on=None, how='inner', *, left_on=None, right_on=None, join_keys=None, coalesce_duplicate_keys=True)` overloaded on key shape and `join_on(right, *on_exprs, how='inner')` on arbitrary predicates.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                                                  | [CAPABILITY]                       |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `select`         | `(*exprs: Expr \| str)`                                                       | project columns/expressions        |
|  [02]   | `filter`         | `(*predicates: Expr \| str)`                                                  | conjunctive predicate filter       |
|  [03]   | `aggregate`      | `(group_by: Sequence[Expr \| str] \| Expr, aggs: Sequence[Expr] \| Expr)`     | grouped aggregation                |
|  [04]   | `join`           | `(right, on=None, how='inner', ...)`                                          | equijoin (overloaded on key shape) |
|  [05]   | `join_on`        | `(right, *on_exprs, how='inner')`                                             | join on arbitrary predicates       |
|  [06]   | `with_column`    | `(name, expr: Expr \| str)`                                                   | append a derived column            |
|  [07]   | `with_columns`   | `(*exprs, **named_exprs)` (peers: `with_column_renamed`, `drop`, `distinct`)  | append/rename/drop a column cohort |
|  [08]   | `sort`           | `(*exprs: SortKey)`                                                           | order rows                         |
|  [09]   | `limit`          | `(count, offset=0)`                                                           | bound and offset rows              |
|  [10]   | `union`          | `(other, distinct=False)` (peers: `intersect`, `except_all`, `union_by_name`) | set algebra over frames            |
|  [11]   | `window`         | `(*exprs: Expr)`                                                              | apply window expressions           |
|  [12]   | `unnest_columns` | `(*columns, preserve_nulls=True, recursions=None)`                            | explode list/struct columns        |

| [INDEX] | [SURFACE]     | [CALL_SHAPE]                                        | [CAPABILITY]                             |
| :-----: | :------------ | :-------------------------------------------------- | :--------------------------------------- |
|  [01]   | `repartition` | `(num)` (peers: `repartition_by_hash(*exprs, num)`) | partition fan-out for parallel streaming |
|  [02]   | `cache`       | `()`                                                | materialize once, reuse downstream       |
|  [03]   | `fill_null`   | `(value: Any, subset: list[str] \| None = None)`    | replace nulls before egress              |

`collect` peers `head(n)`/`tail(n)`/`count()`; `to_polars` peers `to_pandas`/`to_pylist`/`to_pydict`; `logical_plan` peers `optimized_logical_plan`/`execution_plan`. `write_parquet` also takes `compression_level` and a `ParquetWriterOptions` override; `write_csv` peers `write_json`/`write_table`.

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

[ENTRYPOINT_SCOPE]: streaming, expression, and builder surfaces
- rail: engine

`RecordBatchStream` is both a synchronous and asynchronous iterator; `next` pulls one `RecordBatch`. `col`/`column` mint a column `Expr`; `lit`/`literal` mint a literal `Expr` and their peers `string_literal`/`str_lit` and `lit_with_metadata`/`literal_with_metadata` carry the typed/metadata variants.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                    | [CAPABILITY]                       |
| :-----: | :----------------------- | :-------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `RecordBatchStream.next` | `() -> RecordBatch` (sync `iter`/`next`, async `aiter`/`anext`) | sync/async batch iteration         |
|  [02]   | `RecordBatch.to_pyarrow` | `() -> pa.RecordBatch`                                          | unwrap to a pyarrow batch          |
|  [03]   | `col` / `column`         | `col(value: str) -> Expr` (attr `col.name` also mints one)      | mint a column-reference expression |
|  [04]   | `lit` / `literal`        | `lit(value: Any) -> Expr`                                       | mint a literal expression          |

`SessionConfig`, `RuntimeEnvBuilder`, and `SQLOptions` return `self` for fluent chaining; each tunes its own policy axis.

| [INDEX] | [BUILDER]           | [FLUENT_METHODS]                                                                                              |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `SessionConfig`     | `with_target_partitions(int)`, `with_batch_size`, `with_information_schema`, `with_repartition_joins`, `set`  |
|  [02]   | `RuntimeEnvBuilder` | `with_fair_spill_pool(size)`, `with_greedy_memory_pool`, `with_disk_manager_specified`, `with_temp_file_path` |
|  [03]   | `SQLOptions`        | `with_allow_ddl(allow=True)`, `with_allow_dml`, `with_allow_statements`                                       |

[ENTRYPOINT_SCOPE]: built-in functions and user-defined functions
- rail: engine

`functions` is the built-in expression namespace (`import datafusion.functions as f`): `f.col`-composing scalar/aggregate/window builders (`f.sum`, `f.avg`, `f.lower`, `f.array_agg`, `f.lead`, `f.row_number`, `f.coalesce`, `f.case`) that return `Expr` and compose with `over(WindowFrame(...))`. UDFs are minted by the module factories `udf`/`udaf`/`udwf` over the `Accumulator`/`WindowEvaluator` base classes and a `Volatility` policy, then handed to `SessionContext.register_udf`/`register_udaf`/`register_udwf`/`register_udtf`; one factory family owns all four UDF modalities, never a builder per UDF kind.

Each `udf`/`udaf`/`udwf` factory takes `(callable/type, input_types, return_type: pa.DataType, volatility: str, name=...)` and returns the matching UDF; `udaf` adds a `state_type: list[pa.DataType]`.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                  | [CAPABILITY]                                   |
| :-----: | :------------------ | :------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `functions` (`f.*`) | `f.sum(expr).over(WindowFrame('rows', start, end))`           | compose built-in SQL functions as `Expr` trees |
|  [02]   | `udf`               | `(func: Callable, ...) -> ScalarUDF`                          | mint a scalar UDF (Arrow-vectorized `func`)    |
|  [03]   | `udaf`              | `(accum: type[Accumulator], ..., state_type) -> AggregateUDF` | mint an aggregate UDF over an `Accumulator`    |
|  [04]   | `udwf`              | `(evaluator: WindowEvaluator, ...) -> WindowUDF`              | mint a window UDF over a `WindowEvaluator`     |

The two UDF base classes a `udaf`/`udwf` subclasses and implements:

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                                                                |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Accumulator`     | `update(*arrays)`, `merge(states)`, `state() -> list[pa.Scalar]`, `evaluate() -> pa.Scalar` |
|  [02]   | `WindowEvaluator` | `evaluate_all(values, num_rows)`, `evaluate(...)`, `evaluate_all_with_rank(...)`            |

[ENTRYPOINT_SCOPE]: `substrait` portability codec
- rail: interchange

The `substrait` namespace round-trips a SQL string or `LogicalPlan` through a portable `Plan`. Every `Serde`/`Producer`/`Consumer` method is a staticmethod (prefix `substrait.` dropped, method-name repeat omitted); `Serde` covers SQL <-> bytes/path, `Producer`/`Consumer` bridge the in-process `LogicalPlan`, and `Plan` self-encodes to protobuf bytes or JSON.

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

[ENGINE_FEDERATION]:
- import: `import datafusion` at boundary scope only; module-level import is banned by the manifest import policy.
- context axis: one `SessionContext` owns query execution and source registration; `SessionConfig`/`RuntimeEnvBuilder`/`SQLOptions` are fluent policy builders threaded into construction or `sql_with_options`, never parallel context types per policy.
- registration axis: `register_*` is the single ingestion family discriminated by source kind (file format, object store, in-memory batch, foreign catalog, table provider, UDF); CSV/Parquet/JSON/Avro/Arrow are rows on `register_*`/`read_*`, never a reader type per format.
- frame axis: `DataFrame` is the lazy relational algebra; `select`/`filter`/`aggregate`/`join`/`window`/`union` return new frames and execute nothing until `collect`, `show`, an `execute_stream*`, a `to_*` export, or a `write_*` sink; plan inspection routes through `logical_plan`/`optimized_logical_plan`/`execution_plan`.
- streaming axis: `execute_stream` and `SessionContext.execute` yield a `RecordBatchStream` that is both sync and async iterable; backpressured `RecordBatch` pull is a stream row, never a manual partition loop; `execute_stream_partitioned` fans out one stream per partition.
- interchange axis: the `substrait` namespace owns the portable `Plan`; SQL serializes through `Serde`, an in-process `LogicalPlan` bridges through `Producer`/`Consumer`, and `Plan.encode`/`to_json`/`from_json` carry the protobuf/JSON wire form for the SUBSTRAIT_PORTABILITY rail; never hand-roll Substrait protobuf encoding.
- expression axis: `col`/`column` mint column references and `lit` and peers mint literals as `Expr` nodes; predicates and projections are `Expr` trees or SQL fragments parsed via `parse_sql_expr`, never string interpolation into SQL text.
- export axis: `to_arrow_table`/`to_pandas`/`to_polars`/`to_pylist`/`to_pydict` are zero-copy or near-zero-copy Arrow exports; foreign-frame interchange is a `to_*`/`from_*` row, never a manual batch concatenation.
- udf axis: `udf`/`udaf`/`udwf` are the three UDF factories over `Accumulator`/`WindowEvaluator` and a volatility policy; one factory family owns all UDF modalities and routes to `register_udf`/`register_udaf`/`register_udwf`/`register_udtf`, never a builder per kind; the `functions` namespace composes built-in expressions as `Expr` trees before any Python UDF is admitted.
- evidence: each execution captures session id, plan stage (logical/optimized/physical), partition count, batch and row counts, and Substrait `Plan` byte length as an engine receipt.
- boundary: datafusion owns Arrow-native SQL/DataFrame execution, multi-source federation, streaming, and Substrait interchange; pyarrow owns the `RecordBatch`/`Schema` wire types at the seam; object-store and catalog federation register foreign providers rather than copying data; downstream owners consume `pa.RecordBatch`/`pa.Table` or a portable `Plan`, never the `_internal` Rust handles.

[INTEGRATION_STACKING]:
- substrait spine: `datafusion.substrait` and `duckdb-substrait` are the two ends of one cross-engine plan rail — a `LogicalPlan` bridged through `substrait.Producer.to_substrait_plan` emits the identical wire `Plan` that `con.from_substrait(plan.encode())` re-binds on DuckDB, and DuckDB's `con.get_substrait(sql)` BLOB re-binds through `substrait.Consumer.from_substrait_plan`; both engines share the protobuf `Plan` and JSON twin, so the data owner round-trips one artifact rather than re-serializing per engine.
- arrow zero-copy seam: `from_arrow`/`from_polars`/`from_pandas` ingest and `to_arrow_table`/`to_polars`/`to_pandas` egress thread the same Arrow C-data capsule polars and pyarrow expose; a DuckDB `fetch_arrow_table()`/`to_arrow_reader()` or a `deltalake` `DeltaTable.to_pyarrow_dataset()` registers directly via `register_table`/`from_arrow` with no intermediate copy, so federation is provider registration, never frame materialization.
- delta federation: a `deltalake.DeltaTable` exposes `to_pyarrow_dataset()` (a `pa.dataset.Dataset`) that `SessionContext.register_table`/`read_table` adopts as a pushdown-capable provider, so a Delta lakehouse table joins a Parquet listing and an object-store CSV under one `SessionContext.sql` without leaving Arrow; predicate/column pruning pushes into the Delta dataset scan.
- retry/observability stack: a federated `execute_stream` over a remote object store composes under a `stamina` `retry_context` for transient store faults and an OpenTelemetry span keyed by the engine receipt (session id, plan byte length, row count), so the streaming pull is one instrumented, retried rail rather than a bare loop.

[RAIL_LAW]:
- Package: `datafusion`
- Owns: Arrow-native SQL and DataFrame execution, multi-format and object-store/catalog federation, scalar/aggregate/window/table UDFs via `udf`/`udaf`/`udwf` over `Accumulator`/`WindowEvaluator`, the `functions` built-in expression namespace, lazy plan inspection, sync/async `RecordBatchStream` streaming, and Substrait `Plan` serialize/deserialize
- Accept: federated query execution and streaming over registered sources (including a `deltalake` `to_pyarrow_dataset` provider and a DuckDB Arrow reader), and Substrait plan interchange round-tripping the same wire `Plan` with `duckdb-substrait` for the SUBSTRAIT_PORTABILITY rail
- Reject: wrapper-renames of `SessionContext`/`DataFrame`/`substrait`; a hand-rolled SQL planner, execution kernel, or Substrait protobuf codec; a reader type per file format where `register_*`/`read_*` rows suffice; a parallel context type per policy where the fluent builders own the axis; a UDF builder per kind where `udf`/`udaf`/`udwf` own the axis; re-serializing a plan per engine where one portable `Plan` crosses both engines; raw `_internal` handles crossing the package boundary
