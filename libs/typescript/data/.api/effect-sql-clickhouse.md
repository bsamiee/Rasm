# [TS_DATA_API_EFFECT_SQL_CLICKHOUSE]

`@effect/sql-clickhouse` binds the neutral `@effect/sql` `SqlClient` to `@clickhouse/client` — the one `data` OLAP row past the single-node ceiling (distributed columnar MergeTree, concurrent high-throughput ingestion, incremental materialized views), extending the contract with ClickHouse-native `param` fragments, streamed `insertQuery` ingest, command-mode routing, and per-query settings.

## [01]-[PUBLIC_TYPES]

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

- `insertQuery<T>({ table, values, format? })` answers `Effect<Clickhouse.InsertResult, SqlError>` and every read answers decoded rows, so NO member on this surface publishes query cost — profile evidence is a second read of `system.query_log` keyed by the statement's id, gated on `SYSTEM FLUSH LOGS query_log` because that log is written asynchronously.
- `withQueryId` and `withClickhouseSettings` are both `Function.dual` — `(value) => (effect) => effect` beside `(effect, value) => effect` — and each is `Effect.locally` over its `FiberRef`, so the scope is the WHOLE wrapped effect: several statements inside one `withQueryId` file under one log key and lose per-statement attribution.
- Every execution mode stamps `query_id` — the `query`, `command`, and `insert` arms each pass it — and an unset `currentQueryId` defaults to a fresh `Crypto.randomUUID()`, so a caller that wants to read its own row back supplies the id rather than discovering one.
- Interruption mints `KILL QUERY WHERE query_id = '<id>'` on the `query`/`command` and `insert` arms alike, so a shared id makes one interrupt kill every sibling statement under that scope.
- `ClickhouseClientConfig` extends `Clickhouse.ClickHouseClientConfigOptions` with `spanAttributes`, `transformResultNames`, and `transformQueryNames`.
- Every execution arm passes raw statement text to the server and gates none of it, so ClickHouse's own `url()`, `postgresql()`, `s3()`, and `remote()` table functions ride the neutral `sql` DSL through the existing `sql.literal` seam — a source becomes a relation SERVER-side here, with no client-side registration surface to reach for.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- `layer`/`layerConfig` yield `ClickhouseClient \| SqlClient` in one Layer (`layerConfig` adds `ConfigError`); `make` opens the connection with a `SELECT 1` probe under a 5-second connect timeout and returns `Effect<ClickhouseClient, SqlError, Scope \| Reactivity>`.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                  |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `ClickhouseClient.layer(config)`                                    | driver layer   | fixed-config at-scale row            |
|  [02]   | `ClickhouseClient.layerConfig(Config.Wrap<ClickhouseClientConfig>)` | driver layer   | env/secret resolution, standing row  |
|  [03]   | `ClickhouseClient.make(config)`                                     | scoped make    | scoped construction in acquire graph |
|  [04]   | `ClickhouseClient.makeCompiler(transform?)`                         | compiler       | identifier-transform harness         |

## [03]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): inherits the `sql` DSL and typed IO — its compiler reports the `sqlite` dialect, so `sql.onDialect` sees `sqlite` and ClickHouse divergence rides the concrete Tag members and `param`'s `ClickhouseParam` custom `Segment` the core `Compiler` folds, never a dialect arm; `SqlSchema` decodes `JSONEachRow` rows as OLTP rows and `executeStream` folds chunks through `@effect/platform-node` `NodeStream`.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): the embedded OLAP row owns every workload below the distributed trigger; `apache-arrow`(`.api/apache-arrow.md`) IPC is the interchange back to those rows and the viewer.
- `data` lane: journal facts replicate INTO MergeTree for concurrent high-throughput serving, and nothing folds back as authority.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; OLAP rows yield `SqlClient` and reach the concrete Tag solely for `param`/`insertQuery`/`asCommand`/`withClickhouseSettings`.
- Admit ClickHouse only past the crisp trigger — concurrent high-throughput ingestion, multi-node scale, high-cardinality real-time serving; it never rides the OLTP transaction and is never the record of truth.
