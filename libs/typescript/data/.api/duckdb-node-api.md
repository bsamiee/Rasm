# [TS_DATA_API_DUCKDB_NODE_API]

`@duckdb/node-api` binds the embedded DuckDB engine in-process over a promise-native surface — vectorized OLAP with lossless typing, streaming result readers, and prepared binds — the single-node analytical row of the `data` lane. It reads Parquet, CSV, JSON, and Arrow zero-copy, range-reads object storage, and `ATTACH`es Postgres or SQLite through extension SQL; past its single-writer embedded ceiling the workload moves to the ClickHouse row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/node-api`
- package: `@duckdb/node-api` (MIT)
- module: ESM/CJS; native engine via `@duckdb/node-bindings` (N-API, prebuilt platform binaries)
- runtime: `runtime:node`/bun services and CLI
- rail: `lane/olap` embedded node row — no Effect peer; the boundary kernel wraps it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, session, bind, cell, and user-defined-function types, each named by its producing call

`run` returns `DuckDBMaterializedResult`, `stream` a lazy `DuckDBResult`; the `*AndRead*` family returns `DuckDBResultReader`, `prepare` a `DuckDBPreparedStatement`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `DuckDBInstance`               | class         | one database file or `:memory:`; single-writer ACID WAL |
|  [02]   | `DuckDBConnection`             | class         | leased session running statements                       |
|  [03]   | `DuckDBResult`                 | class         | lazy stream; `yieldRowObjects()` async batch iterator   |
|  [04]   | `DuckDBResultReader`           | class         | materialized `getRows`/`getColumns`/`getRowObjects`     |
|  [05]   | `DuckDBPreparedStatement`      | class         | `bind(values, types?)` then run/stream mirrors          |
|  [06]   | `DuckDBValue`                  | union         | typed bind/result cell crossing the kernel cast-free    |
|  [07]   | `DuckDBScalarFunction`         | class         | per-row UDF over an input chunk into one output vector  |
|  [08]   | `DuckDBTableFunction`          | class         | scan-producing UDF answering a chunk per call           |
|  [09]   | `DuckDBScalarFunctionBindInfo` | class         | scalar bind seam: client context, bind data, refusal    |
|  [10]   | `DuckDBScalarFunctionInfo`     | class         | scalar call seam: bind data, extra info, refusal        |
|  [11]   | `DuckDBTableFunctionBindInfo`  | class         | result columns, parameters, cardinality, bind data      |
|  [12]   | `DuckDBTableFunctionInfo`      | class         | scan call seam: bind, init, and per-thread local init   |
|  [13]   | `DuckDBTableFunctionInitInfo`  | class         | projection roster, thread cap, init data                |
|  [14]   | `DuckDBDataChunk`              | class         | the vector-width write surface a scan fills per call    |
|  [15]   | `DuckDBAppender`               | class         | typed row append into an existing relation              |

- `DuckDBValue` opens on `null | boolean | number | bigint | string` beside the wrapper classes — `DuckDBArrayValue`, `DuckDBBitValue`, `DuckDBBlobValue`, `DuckDBDateValue`, `DuckDBDecimalValue`, `DuckDBGeometryValue`, `DuckDBIntervalValue`, `DuckDBListValue`, `DuckDBMapValue`, `DuckDBStructValue`, the four `DuckDBTimestamp*Value` widths beside `DuckDBTimestampTZValue`, `DuckDBTime*Value`, `DuckDBUnionValue`, `DuckDBUUIDValue`, `DuckDBVariantValue` — so a `VARCHAR` column reads as a JS `string` and a wrapper class carries every type JS holds no primitive for.
- `getRowObjects(): Record<string, DuckDBValue>[]` keys each row by column name; `convertRowObjects<T>(converter)`, `getRowObjectsJS()`, and `getRowObjectsJson()` are the converted twins, and `getRows`/`getColumns`/`getColumnsObject` carry the same four-way conversion family.
- `EXPLAIN ANALYZE` under `PRAGMA enable_profiling='json'` answers exactly ONE row of two `VARCHAR` columns — `explain_key` reading `analyzed_plan` and `explain_value` carrying the profile JSON — so a harvest reads the second cell as a string and parses it.
- That profile tree nests the analyzed plan one level under an `EXPLAIN_ANALYZE` operator the harvest's own statement introduced, and the root's `rows_returned` measures the OUTER statement, so it reads zero on every harvest and returned rows come from the plan root's `operator_cardinality` instead.
- `DuckDBDataChunk` writes column-major through `setColumns`/`setColumnValues` or row-major through `setRows`, and `rowCount` is a settable property the engine refuses past 2048 rows — one chunk is one vector width, never a whole result.
- `DuckDBAppender` carries a typed `append*` member per engine type beside `appendDefault`, `appendNull`, `appendValue`, and `appendDataChunk`, with `endRow` closing a row and `flushSync`/`closeSync` the two commit arms; `columnCount`/`columnType` read the target relation, and every append is POSITIONAL — the appender resolves no column name at all.
- Root fields are `latency` and `query_name` beside the counter set `cpu_time`, `blocked_thread_time`, `cumulative_cardinality`, `cumulative_rows_scanned`, `result_set_size`, `total_bytes_read`, `total_bytes_written`, `total_memory_allocated`, `system_peak_buffer_memory`, and `system_peak_temp_dir_size`; each child carries `operator_type`, `operator_name`, `operator_timing`, `operator_cardinality`, and `operator_rows_scanned`, and every timing is SECONDS.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped acquire, execute, stream, prepared binds, and user-defined-function registration

`connection` owns every execute and read; the read family is `{run,stream}AndRead{All,Until}` returning `DuckDBResultReader`. Release arms are `instance.closeSync()` and `connection.disconnectSync()` (alias `closeSync()`).

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `DuckDBInstance.create(string?, Record?)`                 | factory  | engine acquire; `:memory:` or file path  |
|  [02]   | `instance.connect() -> DuckDBConnection`                  | instance | scoped session lease per unit of work    |
|  [03]   | `connection.run(sql, values?, types?)`                    | instance | DDL/DML to completion, materialized      |
|  [04]   | `connection.runAndReadAll(sql, values?)`                  | instance | bounded set to a reader                  |
|  [05]   | `connection.streamAndReadUntil(sql, count)`               | instance | incremental reader to `targetRowCount`   |
|  [06]   | `connection.stream(sql, values?, types?)`                 | instance | chunk-lazy `DuckDBResult`; no re-buffer  |
|  [07]   | `connection.prepare(sql)`                                 | instance | `DuckDBPreparedStatement`; bind then run |
|  [08]   | `connection.registerScalarFunction(DuckDBScalarFunction)` | instance | install a per-row UDF on the instance    |
|  [09]   | `connection.registerTableFunction(DuckDBTableFunction)`   | instance | install a scan UDF on the instance       |
|  [10]   | `connection.createAppender(table, schema?, catalog?)`     | instance | typed row append into an existing table  |
|  [11]   | `DuckDBScalarFunction.create(spec)`                       | factory  | assemble a scalar UDF from one record    |
|  [12]   | `DuckDBTableFunction.create(spec)`                        | factory  | assemble a table UDF from one record     |
|  [13]   | `readValue(Value) -> DuckDBValue`                         | static   | one engine value to a typed cell         |
|  [14]   | `quotedString(input)`                                     | static   | injection-safe string literal splice     |
|  [15]   | `quotedIdentifier(input)`                                 | static   | injection-safe identifier splice         |

- `DuckDBScalarFunction.create`: `{name, mainFunction, returnType}` are required; `bindFunction`, `parameterTypes`, `varArgsType`, `specialHandling`, `volatile`, and `extraInfo` refine it, and every key has a `set*`/`add*` mutator for assembly in parts.
- `DuckDBTableFunction.create`: `{name, bindFunction, initFunction, mainFunction}` are required; `localInitFunction`, `parameterTypes`, `namedParameterTypes`, `supportsProjectionPushdown`, and `extraInfo` refine it.
- `readValue`: covers the two places the engine hands back a value rather than a vector — table-function parameters and `duckdb_get_table_names` — and costs roughly a microsecond per scalar, so a per-row path reads its vector instead.
- `registerTableFunction` and `registerScalarFunction` are SYNCHRONOUS and return `void`, so both lift through `Effect.try`; `createAppender` alone answers a promise, and it resolves an attached catalog through its third argument where a bare table name resolves against `main` and refuses.
- Column declaration rides `addResultColumn(name, type)` at the bind seam, arity rides `setCardinality(count, isExact)` (a `number`, never a `bigint`), and parameters read through `getParameter(index)` beside `getNamedParameter(name)`, which answers `null` for an unsupplied name and refuses an undeclared one at the binder; the bind function fires TWICE per statement, so it declares and never advances.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every promise lifts through `Effect.tryPromise` into a typed lane fault; instance and connection ride `Effect.acquireRelease` under `Scope`; readers lift to `Stream` at the lane seam.
- Single-file ACID, one concurrent writer, out-of-core spill is the embedded ceiling; past it the workload moves to the ClickHouse row, never a second embedded instance fleet.
- Registration is INSTANCE-scoped: the entry lands in the `system.main` catalog, every sibling connection reads it, it survives the registering connection's close, and nothing drops it — a catalog error refuses the attempt — so a name is minted once per source and a per-lease name leaks a permanent entry.
- Re-registering a live name is a SILENT no-op that keeps the FIRST registration's bind data serving, so a function resolving content from a closure serves whatever the first mint captured; a name-keyed lookup the bind function performs is the only shape a re-pump can refresh.
- `DuckDBTableMainFunction` sets `outputDataChunk.rowCount` before writing that many rows to each column vector, and a row count of zero is the scan's ONLY termination signal — a main function that never writes zero never terminates.
- That terminator is OVERLOADED — it spells exhausted and nothing-yet alike — so a scan is admissible only over content resident before the bind: an async main returns a promise the engine never awaits and the scan ends empty, a busy-spin deadlocks the loop that delivers the rows, and `Atomics.wait` against a worker-fed `SharedArrayBuffer` is the one proven in-scan block at the cost of every fiber sharing the thread.
- `main` is marshalled onto the node MAIN THREAD and re-entered strictly serially — `localInitFunction` fires once even under `SET threads=4` — and `connection.interrupt()` reaches a running scan, so an abandoned unit stops at the engine like any other statement.
- `setBindData` on the bind seam is what `getBindData` answers on every later call, `setInitData` likewise per scan and `localInitFunction` per scanning thread, so per-scan state rides those slots rather than a closure the engine re-enters concurrently.
- Every UDF reports failure through `setError(string)` on whichever info seam it holds: a thrown JS error folds to the SAME `Invalid Input Error: <text>` on the outer promise, so the slot is the declared arm for attribution rather than for reach, and a mid-scan refusal fails the whole statement rather than truncating it.
- `supportsProjectionPushdown` is mandatory for any scan writing the projected roster: `DuckDBTableFunctionInitInfo.columnCount` and `getColumnIndexes()` answer the projected set either way, but only the opt-in NARROWS the output chunk, so a roster-width write without it refuses inside the value converter. That roster carries the PLAN's order rather than the SELECT list's, and `count(*)` still requests one column.
- Predicate pushdown is ABSENT — no filter member reaches the init seam, so a `WHERE` clause scans every row the function emits while `LIMIT` does terminate early; `setCardinality(count, true)` is exact and free for a resident source and is the one planner advantage the route carries.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): result IPC egress folds through `tableFromIPC`/`Olap.wire.decode`, outbound through `tableToIPC`/`Olap.wire.encode`; every engine seam meets on Arrow IPC, never row-materialized re-encoding.
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): the browser peer of this node row, sharing the Arrow wire and the `INSTALL`/`LOAD` extension model.
- `@effect/sql-clickhouse`(`.api/effect-sql-clickhouse.md`): the at-scale OLAP row this engine hands off to past the embedded ceiling, joined on the same Arrow IPC wire.
- `lane/olap`: its kernel wraps `create`/`connect` in `Effect.acquireRelease` and lifts every `run`/`stream`/`prepare` call through `Effect.tryPromise`, the boundary rail this raw promise API never carries.
- `lane/olap`: `registerTableFunction` projects a PRE-PUMPED lane source into SQL as a scan — `addResultColumn(name, type)` declares the columns at the bind seam, the main function fills chunks off a residency the handle holds by name, and `setError` folds a source refusal into the query's own fault instead of a materialize-then-load hop.

[LOCAL_ADMISSION]:
- Analytical accelerator, never a record of truth — journal facts in, verdicts out.
- Admission crosses as SQL statements minted through `quotedString`/`quotedIdentifier`, never an API member: the lane records which extensions a deployment admits — `httpfs` `postgres` `sqlite` `ducklake` `iceberg` `delta` `spatial` `vss` `fts` — and a load failure refuses the capability, never crashes the lane.
- This package ships NO extension: `autocomplete`, `core_functions`, `icu`, `json`, and `parquet` are statically linked and everything else — `httpfs`, `postgres_scanner`, `sqlite_scanner`, `ducklake` — installs over the network on first `INSTALL`, so an offline or egress-sealed host reaches only the linked five. Local Parquet needs none of them: `read_parquet`, `parquet_scan`, and the bare-path replacement scan all resolve against the linked reader.
- Arrow ingress and the replacement-scan hook are both ABSENT on this surface — no `registerArrow`, `insertArrowTable`, `registerFileBuffer`, or `registerReplacementScan` member exists, and `arrow_scan` takes three raw pointers nothing on this side yields — so `createAppender` is the uncatalogued load-into-a-plane counterparty and Arrow crosses as IPC bytes a statement reads.

[RAIL_LAW]:
- Package: `@duckdb/node-api`
- Owns: the embedded single-node analytical engine — instance/connect lifecycle, run/read/stream/prepared execution, extension SQL admission, scalar and table user-defined functions, the typed appender
- Accept: scoped acquire-release wrap, `tryPromise` lifts for the promise members and `Effect.try` for the synchronous registrations, Arrow IPC interchange, `httpfs`/`ATTACH`/DuckLake as statements, a RESIDENT lane source exposed to SQL as one instance-scoped table function whose bind resolves content by name
- Reject: the standalone `duckdb` callback binding, OLAP-in-OLTP transaction coupling, a second hand-rolled analytical client, unscoped instance leaks, a per-lease registration name, a scan over a source pulled lazily inside `main`, a per-scan closure standing in for the bind and init data slots, a scan writing the bound roster without the projection opt-in
