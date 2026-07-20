# [TS_DATA_API_EFFECT_SQL_MSSQL]

`@effect/sql-mssql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `tedious` SQL Server wire as the read-oriented INTEROP lane — a typed ingress into enterprise-held SQL Server data an app already owns, never a record of truth. Its compiler reports `dialect: "mssql"`, so `sql.onDialect`'s `mssql` arm goes live, emitting real T-SQL from one statement definition with no parallel query family. Query, transaction, and typed-IO surface stay the spine's; this driver adds SQL Server's own shape atop the pooled connection and `mssql`-seeded span — the typed `param` fragment, the strongly-typed stored-procedure `call`, and the re-exported `MssqlTypes` `DataType` catalog. `MssqlMigrator` ships banned branch-wide — DDL is `iac`↔`store` declarative ensure, never runtime schema mutation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-mssql`
- package: `@effect/sql-mssql`
- license: `MIT`
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core this extends; `.api/effect-sql.md`), `@effect/experimental` (`Reactivity` — `make` requires it; `.api/effect-experimental.md`), `@effect/platform`
- backing: `tedious` (SQL Server TDS wire + connection pool; direct dependency, not peer)
- runtime: `runtime:node`/bun services — `tedious` is a node-native TDS driver; the interop lane never reaches the browser plane
- modules: `MssqlClient`, `Parameter`, `Procedure`, `MssqlMigrator` (banned), and the `MssqlTypes` re-export of `tedious`'s `TYPES` `DataType` catalog

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `MssqlClient` service and its config
- rail: data/lane
- `MssqlClient extends SqlClient` — providing the layer yields both Tags, so interop rows compose the neutral `SqlClient` and only construction reaches the concrete Tag; the extension adds `config`, the typed `param` fragment builder, and the stored-procedure `call`. `MssqlClientConfig` carries the SQL Server connection shape — `server` required, `domain`/`instanceName`/`authType` for Windows/named-instance auth, `encrypt`/`trustServer` for TLS posture, discrete `database`/`username`/`password`, pool sizing (`minConnections`/`maxConnections`/`connectionTTL`/`connectTimeout`), a `parameterTypes` override map seeding the compiler's `PrimitiveKind`→`DataType` binding, and the shared `spanAttributes`/`transformResultNames`/`transformQueryNames` transforms every dialect driver carries.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                 |
| :-----: | :----------------------------------------------------- | :------------------ | :-------------------------------------------------- |
|  [01]   | `MssqlClient` (Tag) / `interface MssqlClient`          | service Tag         | `lane/mssql` interop row; only ctor reaches Tag     |
|  [02]   | `MssqlClient.config: MssqlClientConfig`                | resolved config     | span/transform/parameter-type introspection         |
|  [03]   | `MssqlClient.param(type, value, options?)`             | typed fragment      | `DataType`-bound `Fragment`; T-SQL parameter splice |
|  [04]   | `MssqlClient.call(procedure)`                          | stored-proc invoke  | run a `ProcedureWithValues` → typed output + rows   |
|  [05]   | `MssqlClientConfig.server` (required)                  | connection          | host or named endpoint; the one non-optional field  |
|  [06]   | `MssqlClientConfig.domain`/`.instanceName`/`.authType` | auth shape          | Windows-domain and named-instance authentication    |
|  [07]   | `MssqlClientConfig.encrypt`/`.trustServer`             | TLS posture         | wire encryption + cert-trust policy                 |
|  [08]   | `MssqlClientConfig.password` (`Redacted.Redacted`)     | credential          | pool auth; never a literal                          |
|  [09]   | `MssqlClientConfig.minConnections`/`.maxConnections`   | pool sizing         | per-app pool budget; `connectionTTL` a `Duration`   |
|  [10]   | `MssqlClientConfig.parameterTypes`                     | type override       | `PrimitiveKind`→`DataType` seed the compiler binds  |
|  [11]   | `MssqlClientConfig.spanAttributes` + name transforms   | telemetry/transform | shared with every dialect driver                    |

[PUBLIC_TYPE_SCOPE]: the typed parameter and stored-procedure families
- rail: shapes
- `Parameter<A>` names a single `DataType`-typed value (`_tag: "Parameter"`, `name`, `type`, `options`) — the phantom `A` carries the decoded value type. `Procedure<I, O, A>` is the `Pipeable` builder accreting an input-parameter record `I`, an output-parameter record `O`, and a row type `A`; `ProcedureWithValues<I, O, A>` binds concrete input values, and `Procedure.Result<O, A>` is the invocation return — `output` the decoded output-parameter record, `rows` the decoded result set. `MssqlTypes` re-exports `tedious`'s `TYPES` — the `DataType` catalog (`Int`/`NVarChar`/`DateTime2`/`TVP`/…) every `param`/`Parameter`/`Procedure` binding names.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------------------------------ | :----------------- | :------------------------------------------------------- |
|  [01]   | `Parameter<A>` / `Parameter.make`           | typed parameter    | a named `DataType` value; phantom `A` is the row type    |
|  [02]   | `Procedure<I, O, A>`                        | proc builder       | `Pipeable` accreting input/output params + row type      |
|  [03]   | `ProcedureWithValues<I, O, A>`              | bound proc         | input values bound; the shape `call` accepts             |
|  [04]   | `Procedure.Result<O, A>`                    | invocation result  | `output` record + decoded `rows` array                   |
|  [05]   | `Procedure.ParametersRecord<I>`             | value record       | the concrete input-value record `compile` demands        |
|  [06]   | `MssqlTypes` (re-export of `tedious.TYPES`) | `DataType` catalog | the type vocabulary `param`/`Parameter`/`Procedure` name |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- rail: data/lane
- `layer`/`layerConfig` yield `MssqlClient | SqlClient` in one Layer, error `ConfigError | SqlError`; `make` returns `Effect<MssqlClient, SqlError, Scope | Reactivity>`. Providing either Layer yields both Tags; only construction reaches the concrete `MssqlClient`. `makeCompiler` returns a `Statement.Compiler` fixed to `dialect: "mssql"`, seeding `db.system.name` and `server.address` on every span; `defaultParameterTypes` is the built-in `PrimitiveKind`→`DataType` map the compiler placeholders bind, overridable via `MssqlClientConfig.parameterTypes`.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MssqlClient.layer(config)`                               | driver layer   | fixed-config interop row                       |
|  [02]   | `MssqlClient.layerConfig(Config.Wrap<MssqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `MssqlClient.make(config)`                                | scoped make    | construction inside a larger acquire graph     |
|  [04]   | `MssqlClient.makeCompiler(transform?)`                    | compiler       | `dialect: "mssql"` harness; lights `onDialect` |
|  [05]   | `MssqlClient.defaultParameterTypes`                       | type map       | `PrimitiveKind`→`DataType` default binding     |

[ENTRYPOINT_SCOPE]: composing and invoking a stored procedure
- rail: shapes
- `Procedure.make(name)` opens an empty builder; `Procedure.param<A>()(name, type, options?)` and `Procedure.outputParam<A>()(name, type, options?)` accrete typed input/output parameters, `Procedure.withRows<A>()` declares the result-set row type, and `Procedure.compile(self)(input)` binds concrete input values into a `ProcedureWithValues` the client's `call` runs. `Parameter.make(name, type, options?)` builds a standalone typed parameter; `MssqlClient.param(type, value, options?)` splices a `DataType`-typed value directly into a `Fragment` for inline T-SQL.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Procedure.make(name)`                        | proc builder    | empty `Procedure<{}, {}>` seed                     |
|  [02]   | `Procedure.param<A>()(name, type, options?)`  | input param     | accrete a typed input into `I`                     |
|  [03]   | `Procedure.outputParam<A>()(name, type, ...)` | output param    | accrete a typed output into `O`                    |
|  [04]   | `Procedure.withRows<A>()`                     | row type        | declare the decoded result-set element type        |
|  [05]   | `Procedure.compile(self)(input)`              | bind values     | `Procedure` + input record → `ProcedureWithValues` |
|  [06]   | `Parameter.make(name, type, options?)`        | typed parameter | standalone `DataType`-named value                  |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): all query/transaction/typed-IO surface is inherited — `SqlSchema` decodes interop rows into `Schema` models, `SqlResolver` batches the read side, `withTransaction` scopes a multi-statement read. Its compiler carries `dialect: "mssql"`, so `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` emits T-SQL from the same definition the pg spine and sqlite lanes share — the `mssql` arm-key is realized, not a parallel journal.
- Stack across `data`: the profile is one `lane/mssql` interop row — ingress only, never authority. Journal law holds: an app reads enterprise SQL Server through this Tag and folds facts INTO the append-only journal; nothing folds back as a record of truth. Its typed `param`/`call` surface serves read-side procedure ingress; reactive read-your-writes and LISTEN/NOTIFY are pg-spine capabilities, absent on this ingress lane.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; interop rows yield the neutral `SqlClient` and reach the concrete `MssqlClient` Tag solely for construction and the SQL-Server-specific `param`/`call` surface.
- `password` rides `Config.redacted`; pool sizing (`minConnections`/`maxConnections`/`connectionTTL`), `encrypt`/`trustServer` TLS posture, and named-instance auth are `Config`/`iac` facts, never literals in a row.
- Stored procedures compose through `Procedure.make`→`param`/`outputParam`/`withRows`→`compile` and run via `MssqlClient.call`; inline typed values splice through `MssqlClient.param`, naming a `MssqlTypes` `DataType` — never a raw string-built parameter.
- `MssqlMigrator` (re-exporting `@effect/sql/Migrator`) is banned branch-wide — an interop source is read, not schema-owned; DDL is `iac`↔`store` declarative ensure, runtime never mutates.

[RAIL_LAW]:
- Package: `@effect/sql-mssql`
- Owns: the `tedious` binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`/`defaultParameterTypes`, the `MssqlClientConfig` family, the typed `param` fragment and stored-procedure `call`, the `Parameter`/`Procedure` builders, the `MssqlTypes` `DataType` catalog, the `dialect: "mssql"` compiler that lights the `sql.onDialect` `mssql` arm, and the banned `MssqlMigrator`
- Accept: the read-oriented interop lane row under the one `@effect/sql` contract, `Config`-sourced credentials, T-SQL emitted through the `mssql` `onDialect` arm, typed procedure ingress via `call`, facts folded into the journal
- Reject: a driver import in a neutral row, SQL Server treated as a record of truth, `MssqlMigrator` or any runtime schema mutation, hardcoded credentials or pool sizes, raw string-built parameters bypassing `param`/`Parameter`, a second relational contract for interop
