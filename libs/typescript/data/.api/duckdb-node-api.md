# [TS_DATA_API_DUCKDB_NODE_API]

`@duckdb/node-api` binds the embedded DuckDB engine in-process over a promise-native surface — vectorized OLAP with lossless typing, streaming result readers, and prepared binds — the single-node analytical row of the `data` lane. It reads Parquet, CSV, JSON, and Arrow zero-copy, range-reads object storage, and `ATTACH`es Postgres or SQLite through extension SQL; past its single-writer embedded ceiling the workload moves to the ClickHouse row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/node-api`
- package: `@duckdb/node-api` (MIT)
- module: ESM/CJS; native engine via `@duckdb/node-bindings` (N-API, prebuilt platform binaries)
- runtime: `runtime:node`/bun services and CLI
- rail: `lane/olap` embedded node row — no Effect peer; the boundary kernel wraps it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, session, bind, and cell types, each named by its producing call

`run` returns `DuckDBMaterializedResult`, `stream` a lazy `DuckDBResult`; the `*AndRead*` family returns `DuckDBResultReader`, `prepare` a `DuckDBPreparedStatement`.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `DuckDBInstance`          | engine handle  | one database file or `:memory:`; single-writer ACID WAL |
|  [02]   | `DuckDBConnection`        | session handle | leased session running statements                       |
|  [03]   | `DuckDBResult`            | result surface | lazy stream; `yieldRowObjects()` async batch iterator   |
|  [04]   | `DuckDBResultReader`      | result surface | materialized `getRows`/`getColumns`/`getRowObjects`     |
|  [05]   | `DuckDBPreparedStatement` | bind surface   | `bind(values, types?)` then run/stream mirrors          |
|  [06]   | `DuckDBValue`             | cell union     | typed bind/result cell crossing the kernel cast-free    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped acquire, execute, stream, and prepared binds

`connection` owns every execute and read; the read family is `{run,stream}AndRead{All,Until}` returning `DuckDBResultReader`. Release arms are `instance.closeSync()` and `connection.disconnectSync()` (alias `closeSync()`).

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `DuckDBInstance.create(string?, Record?)`   | static   | engine acquire; `:memory:` or file path  |
|  [02]   | `instance.connect() -> DuckDBConnection`    | instance | scoped session lease per unit of work    |
|  [03]   | `connection.run(sql, values?, types?)`      | instance | DDL/DML to completion, materialized      |
|  [04]   | `connection.runAndReadAll(sql, values?)`    | instance | bounded set to a reader                  |
|  [05]   | `connection.streamAndReadUntil(sql, count)` | instance | incremental reader to `targetRowCount`   |
|  [06]   | `connection.stream(sql, values?, types?)`   | instance | chunk-lazy `DuckDBResult`; no re-buffer  |
|  [07]   | `connection.prepare(sql)`                   | instance | `DuckDBPreparedStatement`; bind then run |
|  [08]   | `quotedString(input)`                       | static   | injection-safe string literal splice     |
|  [09]   | `quotedIdentifier(input)`                   | static   | injection-safe identifier splice         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every promise lifts through `Effect.tryPromise` into a typed lane fault; instance and connection ride `Effect.acquireRelease` under `Scope`; readers lift to `Stream` at the lane seam.
- Single-file ACID, one concurrent writer, out-of-core spill is the embedded ceiling; past it the workload moves to the ClickHouse row, never a second embedded instance fleet.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): result IPC egress folds through `tableFromIPC`/`Olap.wire.decode`, outbound through `tableToIPC`/`Olap.wire.encode`; every engine seam meets on Arrow IPC, never row-materialized re-encoding.
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): the browser peer of this node row, sharing the Arrow wire and the `INSTALL`/`LOAD` extension model.
- `@effect/sql-clickhouse`(`.api/effect-sql-clickhouse.md`): the at-scale OLAP row this engine hands off to past the embedded ceiling, joined on the same Arrow IPC wire.
- `lane/olap`: its kernel wraps `create`/`connect` in `Effect.acquireRelease` and lifts every `run`/`stream`/`prepare` call through `Effect.tryPromise`, the boundary rail this raw promise API never carries.

[LOCAL_ADMISSION]:
- Analytical accelerator, never a record of truth — journal facts in, verdicts out.
- Admission crosses as SQL statements minted through `quotedString`/`quotedIdentifier`, never an API member: the lane records which extensions a deployment admits — `httpfs` `postgres` `sqlite` `ducklake` `iceberg` `delta` `spatial` `vss` `fts` — and a load failure refuses the capability, never crashes the lane.

[RAIL_LAW]:
- Package: `@duckdb/node-api`
- Owns: the embedded single-node analytical engine — instance/connect lifecycle, run/read/stream/prepared execution, extension SQL admission
- Accept: scoped acquire-release wrap, `tryPromise` lifts, Arrow IPC interchange, `httpfs`/`ATTACH`/DuckLake as statements
- Reject: the standalone `duckdb` callback binding, OLAP-in-OLTP transaction coupling, a second hand-rolled analytical client, unscoped instance leaks
