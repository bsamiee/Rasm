# [TS_DATA_API_EFFECT_SQL_MYSQL2]

`@effect/sql-mysql2` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `mysql2` pool as the read-oriented INTEROP lane — a typed ingress into enterprise-held MySQL data an app already owns, never a record of truth. Its compiler reports `dialect: "mysql"`, so `sql.onDialect`'s `mysql` arm goes live: the dialect algebra the pg spine and sqlite lanes already pre-paid now emits real MySQL SQL from one statement definition, no parallel query family. Query, transaction, and typed-IO surface stay the spine's; this driver owns only the pooled connection, the `mysql`-seeded span, and construction. `MysqlMigrator` ships and is banned branch-wide — DDL is idempotent declarative ensure split `iac` applies / `store` verifies, and an interop source is never schema-owned here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-mysql2`
- package: `@effect/sql-mysql2` (MIT)
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core this extends; `.api/effect-sql.md`), `@effect/experimental` (`Reactivity` — `make`/`reactive`/`reactiveMailbox` require it; `.api/effect-experimental.md`), `@effect/platform`
- backing: `mysql2` (connection pool via `Mysql.PoolOptions`; direct dependency, not peer)
- runtime: `runtime:node`/bun services — `mysql2` is a node-native wire driver; the interop lane never reaches the browser plane
- modules: `MysqlClient`, `MysqlMigrator` (banned)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `MysqlClient` service and its config
- rail: data/lane
- `MysqlClient extends SqlClient` — providing the layer yields both Tags, so interop rows compose the neutral `SqlClient` and only construction reaches the concrete Tag; the extension is `config` alone, no `listen`/`notify`/`json` additions. `MysqlClientConfig` splits url-first (`url` overrides the discrete fields) from host/port/database/username/password, with `poolConfig` handing the raw `mysql2` pool knobs and the shared `spanAttributes`/`transformResultNames`/`transformQueryNames` transforms every dialect driver carries.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                               |
| :-----: | :------------------------------------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `MysqlClient` (Tag) / `interface MysqlClient`            | service Tag         | `lane/mysql` interop row; only ctor reaches Tag   |
|  [02]   | `MysqlClient.config: MysqlClientConfig`                  | resolved config     | span/transform introspection                      |
|  [03]   | `MysqlClientConfig.url` (`Redacted.Redacted`)            | connection          | URI override of discrete fields; `Config`-sourced |
|  [04]   | `MysqlClientConfig.host`/`.port`/`.database`/`.username` | connection          | discrete DSN; `port` defaults `3306`              |
|  [05]   | `MysqlClientConfig.password` (`Redacted.Redacted`)       | credential          | pool auth; never a literal                        |
|  [06]   | `MysqlClientConfig.maxConnections`/`.connectionTTL`      | pool sizing         | per-app pool budget; TTL a `Duration` fact        |
|  [07]   | `MysqlClientConfig.poolConfig` (`Mysql.PoolOptions`)     | raw pool knobs      | TLS/charset/timezone the shared fields omit       |
|  [08]   | `MysqlClientConfig.spanAttributes` + name transforms     | telemetry/transform | shared with every dialect driver                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- rail: data/lane
- `layer`/`layerConfig` yield `MysqlClient | SqlClient` in one Layer, error `ConfigError | SqlError`; `make` returns `Effect<MysqlClient, SqlError, Scope | Reactivity>`. Providing either Layer yields both Tags; only construction reaches the concrete `MysqlClient`. `makeCompiler` returns a `Statement.Compiler` fixed to `dialect: "mysql"`, seeding `db.system.name=mysql`, `server.address`, `server.port` (`3306`), and `db.namespace` on every span.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MysqlClient.layer(config)`                               | driver layer   | fixed-config interop row                       |
|  [02]   | `MysqlClient.layerConfig(Config.Wrap<MysqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `MysqlClient.make(config)`                                | scoped make    | construction inside a larger acquire graph     |
|  [04]   | `MysqlClient.makeCompiler(transform?)`                    | compiler       | `dialect: "mysql"` harness; lights `onDialect` |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): all query/transaction/typed-IO surface is inherited — `SqlSchema` decodes interop rows into `Schema` models, `SqlResolver` batches the read side, `withTransaction` scopes a multi-statement read. Its compiler carries `dialect: "mysql"`, so `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` emits MySQL SQL from the same definition the pg spine and sqlite lanes share — the `mysql` arm-key is realized, not a parallel journal.
- Stack across `data`: the profile is one `lane/mysql` interop row — ingress only, never authority. Journal law holds: an app reads enterprise MySQL through this Tag and folds facts INTO the append-only journal; nothing folds back as a record of truth. Reactive read-your-writes and LISTEN/NOTIFY are pg-spine capabilities, absent on this ingress lane.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; interop rows yield the neutral `SqlClient` and reach the concrete `MysqlClient` Tag solely for construction.
- `url`/`password` ride `Config.redacted`; pool sizing (`maxConnections`/`connectionTTL`) and raw `poolConfig` are `Config`/`iac` facts, never literals in a row.
- `MysqlMigrator` (re-exporting `@effect/sql/Migrator`) is banned branch-wide — an interop source is read, not schema-owned; DDL is `iac`↔`store` declarative ensure, runtime never mutates.

[RAIL_LAW]:
- Package: `@effect/sql-mysql2`
- Owns: the `mysql2` binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`, the `MysqlClientConfig` family, the `dialect: "mysql"` compiler that lights the `sql.onDialect` `mysql` arm, and the banned `MysqlMigrator`
- Accept: the read-oriented interop lane row under the one `@effect/sql` contract, `Config`-sourced credentials, MySQL SQL emitted through the `mysql` `onDialect` arm, facts folded into the journal
- Reject: a driver import in a neutral row, MySQL treated as a record of truth, `MysqlMigrator` or any runtime schema mutation, hardcoded credentials or pool sizes, a second relational contract for interop
