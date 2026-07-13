# [TS_DATA_API_EFFECT_SQL_CLICKHOUSE]

`@effect/sql-clickhouse` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to `@clickhouse/client` — the ONLY row of the OLAP lane that crosses the single-node ceiling: distributed columnar MergeTree, high-throughput concurrent ingestion, incremental materialized views. It extends the neutral contract with the ClickHouse-native members the analytical lane needs — typed `param` fragments, streamed `insertQuery` ingestion, command-mode routing, per-query settings — and rides the `clickhouse` arm of `sql.onDialect`. The OLAP lane never shares the OLTP transaction; Arrow is the wire between engines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-clickhouse`
- package: `@effect/sql-clickhouse`
- license: `MIT`
- effect-peer: `effect`, `@effect/sql` (`.api/effect-sql.md`)
- backing: `@clickhouse/client` (HTTP interface, streaming inserts, `ClickHouseClientConfigOptions`)
- runtime: `runtime:node`/bun services; the browser analytical row is `@duckdb/duckdb-wasm` (`.api/duckdb-duckdb-wasm.md`)
- modules: `ClickhouseClient`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `ClickhouseClient` service and its OLAP-native additions
- rail: data/lane
- `ClickhouseClient extends SqlClient` — the layer yields both Tags; OLAP rows compose the neutral `SqlClient` and reach the concrete Tag only for `param`/`insertQuery`/`asCommand`/`withClickhouseSettings`. `ClickhouseClientConfig` extends `ClickHouseClientConfigOptions` with the shared `spanAttributes`/`transformResultNames`/`transformQueryNames` transforms.

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                              |
| :-----: | :------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `ClickhouseClient` (Tag) / `interface ClickhouseClient`       | service Tag     | `lane/olap` at-scale row                         |
|  [02]   | `ClickhouseClient.param(dataType, value): Statement.Fragment` | typed param     | ClickHouse-typed param splice (`{p: DateTime}`)  |
|  [03]   | `ClickhouseClient.asCommand(effect)`                          | mode transform  | route through command mode (DDL, mutations)      |
|  [04]   | `ClickhouseClient.insertQuery({ table, values, format? })`    | bulk ingest     | streamed insert — fact/meter fan-in path         |
|  [05]   | `ClickhouseClient.withQueryId` / `.withClickhouseSettings`    | per-query knobs | query-id correlation; settings scoped to a fiber |
|  [06]   | `ClickhouseClientConfig`                                      | config          | url/auth/compression; `Config`-sourced           |
|  [07]   | `currentClientMethod` (`"query" \| "command" \| "insert"`)    | FiberRef        | ambient execution-mode coordinate                |
|  [08]   | `currentQueryId`                                              | FiberRef        | query-id correlation coordinate                  |
|  [09]   | `currentClickhouseSettings`                                   | FiberRef        | fiber-scoped settings coordinate                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- rail: data/lane
- `layer`/`layerConfig` provide `ClickhouseClient \| SqlClient` in one Layer, error `ConfigError \| SqlError`; `make` returns `Effect<ClickhouseClient, SqlError, Scope>`.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                  |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `ClickhouseClient.layer(config)`                                    | driver layer   | fixed-config at-scale row            |
|  [02]   | `ClickhouseClient.layerConfig(Config.Wrap<ClickhouseClientConfig>)` | driver layer   | env/secret resolution, standing row  |
|  [03]   | `ClickhouseClient.make(config)`                                     | scoped make    | scoped construction in acquire graph |
|  [04]   | `ClickhouseClient.makeCompiler(transform?)`                         | compiler       | identifier-transform harness         |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): the `sql` DSL and typed IO are inherited; `clickhouse` is an `onDialect` arm-KEY. `SqlSchema` decodes analytical result rows exactly as OLTP rows.
- Stack across `data`: admitted only past the crisp trigger — concurrent high-throughput ingestion, multi-node scale, high-cardinality real-time serving; below it the OLAP lane's embedded rows (`.api/duckdb-node-api.md`) own the workload. Arrow output is the interchange back to the embedded rows and the viewer.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; analytical rows yield `SqlClient` and reach the concrete Tag solely for `param`/`insertQuery`/`asCommand`/`withClickhouseSettings`.
- The OLAP lane is correctness-adjacent, never the record of truth — journal facts replicate INTO MergeTree; nothing folds back as authority.

[RAIL_LAW]:
- Package: `@effect/sql-clickhouse`
- Owns: the ClickHouse binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`, `param` typed fragments, `insertQuery` streamed ingest, `asCommand`, `withQueryId`/`withClickhouseSettings`, the execution-mode FiberRefs
- Accept: the at-scale OLAP row past the distributed trigger, streamed fact ingestion, Arrow interchange to the embedded rows
- Reject: ClickHouse below the single-node ceiling, OLAP riding the OLTP transaction, ClickHouse as a record of truth, a driver import in a neutral row
