# [TS_DATA_API_EFFECT_SQL_MYSQL2]

`@effect/sql-mysql2` binds the neutral `@effect/sql` `SqlClient` to the `mysql2` pool as the read-only interop lane — a typed ingress into enterprise MySQL an app already owns, never a record of truth; this driver owns only the pooled connection, the `mysql`-seeded span, and construction, its `dialect: "mysql"` compiler lighting the `sql.onDialect` `mysql` arm to emit MySQL SQL from one statement definition.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `MysqlClient` service Tag and its config — `MysqlClient extends SqlClient` adds `config` alone, no `listen`/`notify`/`json` surface

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                               |
| :-----: | :------------------------------------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `MysqlClient` (Tag) / `interface MysqlClient`            | service Tag         | `read/query` interop row; only ctor reaches Tag   |
|  [02]   | `MysqlClient.config: MysqlClientConfig`                  | resolved config     | span/transform introspection                      |
|  [03]   | `MysqlClientConfig.url` (`Redacted.Redacted`)            | connection          | URI override of discrete fields; `Config`-sourced |
|  [04]   | `MysqlClientConfig.host`/`.port`/`.database`/`.username` | connection          | discrete DSN; every field optional, driver-filled |
|  [05]   | `MysqlClientConfig.password` (`Redacted.Redacted`)       | credential          | pool auth; never a literal                        |
|  [06]   | `MysqlClientConfig.maxConnections`/`.connectionTTL`      | pool sizing         | per-app pool budget; TTL a `Duration` fact        |
|  [07]   | `MysqlClientConfig.poolConfig` (`Mysql.PoolOptions`)     | raw pool knobs      | TLS/charset/timezone the shared fields omit       |
|  [08]   | `MysqlClientConfig.spanAttributes` + name transforms     | telemetry/transform | shared with every dialect driver                  |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer — either Layer yields both `MysqlClient` and `SqlClient` Tags, only construction reaching the concrete Tag

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MysqlClient.layer(config)`                               | driver layer   | fixed-config interop row                       |
|  [02]   | `MysqlClient.layerConfig(Config.Wrap<MysqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `MysqlClient.make(config)`                                | scoped make    | construction inside a larger acquire graph     |
|  [04]   | `MysqlClient.makeCompiler(transform?)`                    | compiler       | `dialect: "mysql"` harness; lights `onDialect` |

- `layer`/`layerConfig` yield `Layer<MysqlClient | SqlClient, ConfigError | SqlError>` and `make` yields `Effect<MysqlClient, SqlError, Scope | Reactivity>`. `make` — not the compiler — seeds every span: `db.system.name=mysql` beside `server.address` and `server.port` read off the config's optional fields, the driver filling `localhost` and `3306` where each is absent, and `db.namespace` appearing only when `database` is set.
- `makeCompiler` fixes `dialect: "mysql"` on a `?` placeholder, and its `onCustom` and `onRecordUpdate` arms emit EMPTY — a `Statement.custom` segment and a `sql.updateValues` multi-row update each compile to nothing on this lane rather than failing, so neither reaches a MySQL statement.

## [03]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): the driver Layer satisfies the neutral `SqlClient` Tag, so every `SqlSchema` decode, `SqlResolver` batch, and `withTransaction` scope runs on this pool; the `dialect: "mysql"` compiler makes `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` emit MySQL SQL from the shared definition, the `mysql` arm realized.
- within `data`: one read-only `read/query` interop row folds enterprise-MySQL facts INTO the append-only journal, never authority — reactive read-your-writes and LISTEN/NOTIFY are pg-spine capabilities absent on this lane.

[LOCAL_ADMISSION]:
- Provide the Layer at the app root only; a neutral row yields `SqlClient` and reaches the concrete `MysqlClient` Tag solely for construction.
- `url`/`password` ride `Config.redacted`; pool sizing (`maxConnections`/`connectionTTL`) and `poolConfig` are `Config`/`iac` facts, never row literals.
- Every connection field is optional on the config, so a lane that means to pin a host, port, or database states it — the driver's own fallback is a silent default, never a declaration.
- No pool-adoption entrypoint ships: `layer`/`layerConfig`/`make` each build their own pool, so one composition owns one pool per interop lane and the pg spine's `layerFromPool` fan-out has no counterpart here.
- `sql.updateValues` and `Statement.custom` compile empty under this dialect, so neither belongs in a statement this client runs.
- `MysqlMigrator` is banned branch-wide — an interop source is read, never schema-owned; DDL is `iac`↔`data` declarative ensure.
