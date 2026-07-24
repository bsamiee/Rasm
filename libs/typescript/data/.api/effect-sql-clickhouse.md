# [TS_DATA_API_EFFECT_SQL_CLICKHOUSE]

`@effect/sql-clickhouse` binds the neutral `@effect/sql` `SqlClient` to `@clickhouse/client` — the one `data` OLAP row past the single-node ceiling (distributed columnar MergeTree, concurrent high-throughput ingestion, incremental materialized views), extending the contract with ClickHouse-native `param` fragments, streamed `insertQuery` ingest, command-mode routing, and per-query settings.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-clickhouse`
- package: `@effect/sql-clickhouse` (MIT)
- effect-peer: `effect`, `@effect/sql` (`.api/effect-sql.md`), `@effect/experimental` (`Reactivity`), `@effect/platform-node` (`NodeStream`)
- backing: `@clickhouse/client` — HTTP interface, streamed inserts, `ClickHouseClientConfigOptions`
- module: `ClickhouseClient`; `runtime:node`/bun (browser analytical row is `@duckdb/duckdb-wasm`, `.api/duckdb-duckdb-wasm.md`)
- rail: the at-scale OLAP row of the `data` lane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `ClickhouseClient` service and its OLAP-native additions over `SqlClient`

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                              |
| :-----: | :------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `ClickhouseClient` (Tag) / `interface ClickhouseClient`       | service Tag     | `lane/olap` at-scale row                         |
|  [02]   | `ClickhouseClient.param(dataType, value): Statement.Fragment` | typed param     | ClickHouse-typed param splice (`{p1: DateTime}`) |
|  [03]   | `ClickhouseClient.asCommand(effect)`                          | mode transform  | route through command mode (DDL, mutations)      |
|  [04]   | `ClickhouseClient.insertQuery({ table, values, format? })`    | bulk ingest     | streamed insert — fact/meter fan-in path         |
|  [05]   | `ClickhouseClient.withQueryId` / `.withClickhouseSettings`    | per-query knobs | query-id correlation; settings scoped to a fiber |
|  [06]   | `ClickhouseClientConfig`                                      | config          | url/auth/compression; `Config`-sourced           |
|  [07]   | `currentClientMethod` (`"query" \| "command" \| "insert"`)    | FiberRef        | ambient execution-mode coordinate                |
|  [08]   | `currentQueryId`                                              | FiberRef        | query-id correlation coordinate                  |
|  [09]   | `currentClickhouseSettings`                                   | FiberRef        | fiber-scoped settings coordinate                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- `layer`/`layerConfig` yield `ClickhouseClient \| SqlClient` in one Layer (`layerConfig` adds `ConfigError`); `make` opens the connection with a `SELECT 1` probe under a 5-second connect timeout and returns `Effect<ClickhouseClient, SqlError, Scope \| Reactivity>`.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                  |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `ClickhouseClient.layer(config)`                                    | driver layer   | fixed-config at-scale row            |
|  [02]   | `ClickhouseClient.layerConfig(Config.Wrap<ClickhouseClientConfig>)` | driver layer   | env/secret resolution, standing row  |
|  [03]   | `ClickhouseClient.make(config)`                                     | scoped make    | scoped construction in acquire graph |
|  [04]   | `ClickhouseClient.makeCompiler(transform?)`                         | compiler       | identifier-transform harness         |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): inherits the `sql` DSL and typed IO — its compiler reports the `sqlite` dialect, so `sql.onDialect` sees `sqlite` and ClickHouse divergence rides the concrete Tag members and `param`'s `ClickhouseParam` custom `Segment` the core `Compiler` folds, never a dialect arm; `SqlSchema` decodes `JSONEachRow` rows as OLTP rows and `executeStream` folds chunks through `@effect/platform-node` `NodeStream`.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): the embedded OLAP row owns every workload below the distributed trigger; `apache-arrow`(`.api/apache-arrow.md`) IPC is the interchange back to those rows and the viewer.
- `data` lane: journal facts replicate INTO MergeTree for concurrent high-throughput serving, and nothing folds back as authority.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; OLAP rows yield `SqlClient` and reach the concrete Tag solely for `param`/`insertQuery`/`asCommand`/`withClickhouseSettings`.
- Admit ClickHouse only past the crisp trigger — concurrent high-throughput ingestion, multi-node scale, high-cardinality real-time serving; it never rides the OLTP transaction and is never the record of truth.

[RAIL_LAW]:
- Package: `@effect/sql-clickhouse`
- Owns: the ClickHouse binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`, `param` typed fragments, `insertQuery` streamed ingest, `asCommand`, `withQueryId`/`withClickhouseSettings`, the execution-mode FiberRefs
- Accept: the at-scale OLAP row past the distributed trigger, streamed fact ingestion, Arrow interchange to the embedded rows
- Reject: ClickHouse below the single-node ceiling, OLAP on the OLTP transaction, ClickHouse as a record of truth, a driver import in a neutral row, `ClickhouseMigrator` or any runtime schema mutation
