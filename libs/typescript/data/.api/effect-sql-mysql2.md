# [TS_DATA_API_EFFECT_SQL_MYSQL2]

`@effect/sql-mysql2` binds the neutral `@effect/sql` `SqlClient` to the `mysql2` pool as the read-only interop lane — a typed ingress into enterprise MySQL an app already owns, never a record of truth; this driver owns only the pooled connection, the `mysql`-seeded span, and construction, its `dialect: "mysql"` compiler lighting the `sql.onDialect` `mysql` arm to emit MySQL SQL from one statement definition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-mysql2`
- package: `@effect/sql-mysql2` (MIT)
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core this extends), `@effect/experimental` (`Reactivity`), `@effect/platform`
- backing: `mysql2` pool via `Mysql.PoolOptions`
- runtime: `runtime:node`/bun services — `mysql2` is a node-native wire driver, never the browser plane
- rail: read-only `lane/mysql` interop row — typed MySQL ingress folded into the journal, never authority
- modules: `MysqlClient`, `MysqlMigrator` (banned)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `MysqlClient` service Tag and its config — `MysqlClient extends SqlClient` adds `config` alone, no `listen`/`notify`/`json` surface

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

[ENTRYPOINT_SCOPE]: constructing the driver Layer — either Layer yields both `MysqlClient` and `SqlClient` Tags, only construction reaching the concrete Tag

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MysqlClient.layer(config)`                               | driver layer   | fixed-config interop row                       |
|  [02]   | `MysqlClient.layerConfig(Config.Wrap<MysqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `MysqlClient.make(config)`                                | scoped make    | construction inside a larger acquire graph     |
|  [04]   | `MysqlClient.makeCompiler(transform?)`                    | compiler       | `dialect: "mysql"` harness; lights `onDialect` |

- `layer`/`layerConfig` yield `Layer<MysqlClient | SqlClient, ConfigError | SqlError>`, `make` yields `Effect<MysqlClient, SqlError, Scope | Reactivity>`, and `makeCompiler` seeds every span `db.system.name=mysql`/`server.address`/`server.port` (`3306`)/`db.namespace`.

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): the driver Layer satisfies the neutral `SqlClient` Tag, so every `SqlSchema` decode, `SqlResolver` batch, and `withTransaction` scope runs on this pool; the `dialect: "mysql"` compiler makes `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` emit MySQL SQL from the shared definition, the `mysql` arm realized.
- within-`data`: one read-only `lane/mysql` interop row folds enterprise-MySQL facts INTO the append-only journal, never authority — reactive read-your-writes and LISTEN/NOTIFY are pg-spine capabilities absent on this lane.

[LOCAL_ADMISSION]:
- Provide the Layer at the app root only; a neutral row yields `SqlClient` and reaches the concrete `MysqlClient` Tag solely for construction.
- `url`/`password` ride `Config.redacted`; pool sizing (`maxConnections`/`connectionTTL`) and `poolConfig` are `Config`/`iac` facts, never row literals.
- `MysqlMigrator` is banned branch-wide — an interop source is read, never schema-owned; DDL is `iac`↔`store` declarative ensure.

[RAIL_LAW]:
- Package: `@effect/sql-mysql2`
- Owns: the `mysql2` binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`, the `MysqlClientConfig` family, the `dialect: "mysql"` compiler lighting the `sql.onDialect` `mysql` arm, and the banned `MysqlMigrator`
- Accept: the read-only interop row under one `@effect/sql` contract, `Config`-sourced credentials, MySQL SQL through the `mysql` `onDialect` arm, facts folded into the journal
- Reject: a driver import in a neutral row, MySQL as a record of truth, `MysqlMigrator` or runtime schema mutation, hardcoded credentials or pool sizes, a second relational contract for interop
