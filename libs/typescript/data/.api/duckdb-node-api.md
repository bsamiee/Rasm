# [TS_DATA_API_DUCKDB_NODE_API]

`@duckdb/node-api` (the "Neo" bindings) is the official promise-native DuckDB binding — vectorized in-process analytics with lossless type support, streaming result readers, and prepared-statement binds. It is a raw promise API — the OLAP lane wraps instance/connection lifecycle in `Effect.acquireRelease` under `Scope` and lifts every call through `Effect.tryPromise`. It reads Parquet/CSV/JSON/Arrow zero-copy, range-reads object storage through `httpfs`, `ATTACH`es Postgres/SQLite, and speaks DuckLake/Iceberg/Delta through extensions — the single-node analytical row of the lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/node-api`
- package: `@duckdb/node-api` (MIT)
- backing: `@duckdb/node-bindings` (N-API native, prebuilt platform binaries)
- runtime: `runtime:node`/bun services and CLI; the browser row is `@duckdb/duckdb-wasm` (`.api/duckdb-duckdb-wasm.md`)
- rail: `lane/olap` embedded node row — no Effect peer; boundary-kernel wrap is the lane's

## [02]-[PUBLIC_TYPES]

Result types name their producing call: `DuckDBResult` from `run`/`stream`, `DuckDBResultReader` from `runAndReadAll`/`streamAndRead*`, prepared statement from `connection.prepare`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `DuckDBInstance`     | engine handle  | one per database file (or `:memory:`); single-writer ACID WAL |
|  [02]   | `DuckDBConnection`   | session handle | per-fiber-tree leased session; runs statements                |
|  [03]   | `DuckDBResult`       | result surface | streaming result; `yieldRowObjects()` async batch iterator    |
|  [04]   | `DuckDBResultReader` | result surface | `getRows()` / `getColumns()` / `getRowObjects()` projections  |
|  [05]   | prepared statement   | bind surface   | `bind(values, types?)` then `run`/`stream` mirrors            |
|  [06]   | `DuckDBValue`        | cell union     | typed bind and result cell; crosses the lane kernel cast-free |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped acquire, execute, stream, and extension SQL
- rail: lane/olap
- Every execute/read is a `connection` member; `instance.closeSync()` and `connection.disconnectSync()` (alias `closeSync()`) are the release arms. Extension SQL admits `httpfs`/`postgres`/`sqlite`/`ducklake`/`iceberg`/`delta`/`spatial`/`vss`/`fts`.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER]                                             |
| :-----: | :------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `DuckDBInstance.create(path, config?)`                        | engine acquire | `":memory:"` or file path, `{ threads }`; acquire arm  |
|  [02]   | `instance.connect()` → `DuckDBConnection`                     | session lease  | scoped connection per analytical unit of work          |
|  [03]   | `run(sql, values?, types?)`                                   | execute        | DDL/DML to completion                                  |
|  [04]   | `runAndReadAll(sql, values?, types?)`                         | materialize    | bounded result sets → reader                           |
|  [05]   | `streamAndReadUntil` / `streamAndReadAll` / `runAndReadUntil` | stream read    | incremental readers to `targetRowCount`; `Stream` lift |
|  [06]   | `stream(sql, values?, types?)` → `DuckDBResult`               | native stream  | chunk-lazy result; `yieldRowObjects()` no re-buffer    |
|  [07]   | `prepare(sql)` → `prepared.bind(values, types?)`              | prepared       | repeated parameterized analytics; `.run()`/`.stream()` |
|  [08]   | `quotedString(input)` / `quotedIdentifier(input)`             | SQL mint       | injection-safe splice for ATTACH/INSTALL mints         |
|  [09]   | `ATTACH` / `INSTALL` / `LOAD` as SQL                          | extension SQL  | capability admission is a statement, never an API      |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Boundary-kernel wrap: every promise lifts through `Effect.tryPromise` with a typed lane fault; instance and connection ride `Effect.acquireRelease` under `Scope`; readers lift to `Stream` at the lane seam.
- Arrow is the wire (`.api/apache-arrow.md`): result IPC egress folds through `tableFromIPC`/`Olap.wire.decode` and outbound through `tableToIPC`/`Olap.wire.encode`; interchange with the wasm row, the viewer, and ClickHouse rides Arrow IPC — never row-materialized re-encoding.
- Storage law: single-file ACID, single concurrent writer, out-of-core spill — the embedded ceiling; past it the workload moves to the ClickHouse row, never to a second embedded instance fleet.

[LOCAL_ADMISSION]:
- Engines here are analytical accelerators, never a record of truth; journal facts flow in, verdicts flow out.
- Extension load (`INSTALL`/`LOAD`) is grant-shaped — the lane records which extensions a deployment admits; a load failure refuses the capability, never crashes the lane.

[RAIL_LAW]:
- Package: `@duckdb/node-api`
- Owns: the embedded single-node analytical engine — instance/connect lifecycle, run/read/stream/prepared execution, extension SQL admission
- Accept: scoped acquire-release wrap, `tryPromise` lifts, Arrow interchange, `httpfs`/`ATTACH`/DuckLake as statements
- Reject: the retired `duckdb` npm binding, OLAP-in-OLTP-transaction coupling, a second hand-rolled analytical client, unscoped instance leaks
