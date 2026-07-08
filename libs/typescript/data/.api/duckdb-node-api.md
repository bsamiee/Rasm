# [TS_DATA_API_DUCKDB_NODE_API]

`@duckdb/node-api` (the "Neo" bindings) is the official promise-native DuckDB binding — vectorized in-process analytics with lossless type support, streaming result readers, and prepared-statement binds. It is a raw promise API, not an Effect service: the OLAP lane wraps instance/connection lifecycle in `Effect.acquireRelease` under `Scope` and lifts every call through `Effect.tryPromise`. The engine reads Parquet/CSV/JSON/Arrow zero-copy, range-reads object storage through `httpfs`, `ATTACH`es Postgres/SQLite, and speaks DuckLake/Iceberg/Delta through extensions — the single-node analytical row of the lane, with ClickHouse past the distributed trigger and `pg_duckdb` for analytics-in-OLTP.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/node-api`
- package: `@duckdb/node-api`
- license: `MIT`
- backing: `@duckdb/node-bindings` (N-API native, prebuilt platform binaries)
- runtime: `runtime:node`/bun services and CLI; the browser row is `@duckdb/duckdb-wasm` (`.api/duckdb-duckdb-wasm.md`)
- rail: `lane/olap` embedded node row — no Effect peer; boundary-kernel wrap is the lane's

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `DuckDBInstance` | engine handle | one per database file (or `:memory:`); single-writer ACID WAL |
| [02] | `DuckDBConnection` | session handle | per-fiber-tree leased session; runs statements |
| [03] | result reader (from `runAndReadAll`/`streamAndRead*`) | result surface | `getRows()` / `getColumns()` — row-major or column-major projection |
| [04] | prepared statement (from `connection.prepare`) | bind surface | `bind(values, types?)` then `run`/`stream` mirrors |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------------------------------------------ |:------------- |:---------------------------------------------- |
| [01] | `DuckDBInstance.create(path, config?)` (`":memory:"` or a file path; config e.g. `{ threads }`) | engine acquire | the lane's `acquireRelease` acquire arm; `instance.closeSync()` is the release arm |
| [02] | `instance.connect()` → `DuckDBConnection` | session lease | scoped connection per analytical unit of work; `connection.disconnectSync()` (alias `closeSync()`) is the release arm |
| [03] | `connection.run(sql, values?, types?)` | execute | DDL/DML to completion |
| [04] | `connection.runAndReadAll(sql, values?, types?)` → reader → `getRows()`/`getColumns()` | materialize | bounded result sets |
| [05] | `connection.streamAndReadUntil(sql, targetRowCount)` / `streamAndReadAll` / `runAndReadUntil` | stream read | incremental readers for large results — the lane's `Stream` lift |
| [06] | `connection.prepare(sql)` → `prepared.bind(values, types?)` → `prepared.run()`/`.stream()` | prepared | repeated parameterized analytics |
| [07] | `ATTACH`/`INSTALL`/`LOAD` as SQL (`httpfs`, `postgres`, `sqlite`, `ducklake`, `iceberg`, `delta`, `spatial`, `vss`, `fts`) | extension SQL | capability admission is a statement, never an API |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Boundary-kernel wrap: every promise lifts through `Effect.tryPromise` with a typed lane fault; instance and connection ride `Effect.acquireRelease` under `Scope`; readers lift to `Stream` at the lane seam.
- Arrow is the wire (`../../ui/.api/apache-arrow.md`): result interchange with the wasm row, the viewer, and ClickHouse rides Arrow IPC — never row-materialized re-encoding.
- Storage law: single-file ACID, single concurrent writer, out-of-core spill — the embedded ceiling; past it the workload moves to the ClickHouse row, never to a second embedded instance fleet.

[LOCAL_ADMISSION]:
- The engine is an analytical accelerator, never a record of truth; journal facts flow in, verdicts flow out.
- Extension load (`INSTALL`/`LOAD`) is grant-shaped — the lane records which extensions a deployment admits; a load failure refuses the capability, never crashes the lane.

[RAIL_LAW]:
- Package: `@duckdb/node-api`
- Owns: the embedded single-node analytical engine — instance/connect lifecycle, run/read/stream/prepared execution, extension SQL admission
- Accept: scoped acquire-release wrap, `tryPromise` lifts, Arrow interchange, `httpfs`/`ATTACH`/DuckLake as statements
- Reject: the retired `duckdb` npm binding, OLAP-in-OLTP-transaction coupling, a second hand-rolled analytical client, unscoped instance leaks
