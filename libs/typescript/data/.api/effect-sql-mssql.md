# [TS_DATA_API_EFFECT_SQL_MSSQL]

`@effect/sql-mssql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `tedious` SQL Server wire as the read-oriented interop lane, adding SQL Server's own shape — the typed `param` fragment, the stored-procedure `call`, the `Procedure`/`Parameter` builders, the `MssqlTypes` `DataType` catalog — atop the inherited query, transaction, and typed-IO spine. Its `dialect: "mssql"` compiler lights the `sql.onDialect` `mssql` arm, emitting T-SQL from one statement definition; `MssqlMigrator` ships branch-banned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-mssql`
- package: `@effect/sql-mssql` (MIT)
- rail: `lane/mssql` — the read-oriented SQL Server interop ingress row
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core this extends; `.api/effect-sql.md`), `@effect/experimental` (`Reactivity`, required by `make`; `.api/effect-experimental.md`), `@effect/platform`
- backing: `tedious` (SQL Server TDS wire + connection pool)
- runtime: `runtime:node`/bun; `tedious` is a node-native TDS driver, never the browser plane
- modules: `MssqlClient`, `Parameter`, `Procedure`, `MssqlMigrator` (banned), `MssqlTypes` (re-export of `tedious` `TYPES`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `MssqlClient` service and its config
- `MssqlClient extends SqlClient`: providing the layer yields both Tags, so interop rows compose the neutral `SqlClient` and only construction with the SQL-Server-specific `config`/`param`/`call` reaches the concrete Tag. `MssqlClientConfig` carries the connection shape the rows below enumerate, seeding the compiler's `PrimitiveKind`→`DataType` binding through `parameterTypes`.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                 |
| :-----: | :----------------------------------------------------- | :------------------ | :-------------------------------------------------- |
|  [01]   | `MssqlClient` (Tag) / `interface MssqlClient`          | service Tag         | `lane/mssql` interop row; only ctor reaches Tag     |
|  [02]   | `MssqlClient.config: MssqlClientConfig`                | resolved config     | span/transform/parameter-type introspection         |
|  [03]   | `MssqlClient.param(type, value, options?)`             | typed fragment      | `DataType`-bound `Fragment`; T-SQL parameter splice |
|  [04]   | `MssqlClient.call(procedure)`                          | stored-proc invoke  | run a `ProcedureWithValues` → typed output + rows   |
|  [05]   | `MssqlClientConfig.server` (required)                  | connection          | host or named endpoint; the one non-optional field  |
|  [06]   | `MssqlClientConfig.database`/`.username`               | connection          | discrete target DB + login; `Config`-sourced        |
|  [07]   | `MssqlClientConfig.domain`/`.instanceName`/`.authType` | auth shape          | Windows-domain and named-instance authentication    |
|  [08]   | `MssqlClientConfig.encrypt`/`.trustServer`             | TLS posture         | wire encryption + cert-trust policy                 |
|  [09]   | `MssqlClientConfig.password` (`Redacted.Redacted`)     | credential          | pool auth; never a literal                          |
|  [10]   | `MssqlClientConfig.minConnections`/`.maxConnections`   | pool sizing         | per-app pool budget; `connectionTTL` a `Duration`   |
|  [11]   | `MssqlClientConfig.parameterTypes`                     | type override       | `PrimitiveKind`→`DataType` seed the compiler binds  |
|  [12]   | `MssqlClientConfig.spanAttributes` + name transforms   | telemetry/transform | shared with every dialect driver                    |

[PUBLIC_TYPE_SCOPE]: the typed parameter and stored-procedure families
- `Parameter<A>` names one `DataType`-typed value, its phantom `A` the decoded type; `Procedure<I, O, A>` is the `Pipeable` builder accreting input record `I`, output record `O`, and row type `A`, `compile` binds concrete input values into `ProcedureWithValues<I, O, A>`, and `Procedure.Result<O, A>` returns the decoded `output` record with `rows`. `MssqlTypes` re-exports `tedious` `TYPES` — the `DataType` catalog (`Int`/`NVarChar`/`DateTime2`/`TVP`/…) every `param`/`Parameter`/`Procedure` binding names.

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
- `layer`/`layerConfig` yield `MssqlClient | SqlClient` in one Layer under error `ConfigError | SqlError`; `make` returns `Effect<MssqlClient, SqlError, Scope | Reactivity>`. `makeCompiler` fixes `dialect: "mssql"`, seeding `db.system.name` and `server.address` per span; `defaultParameterTypes` is the built-in `PrimitiveKind`→`DataType` map, overridable via `MssqlClientConfig.parameterTypes`.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MssqlClient.layer(config)`                               | driver layer   | fixed-config interop row                       |
|  [02]   | `MssqlClient.layerConfig(Config.Wrap<MssqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `MssqlClient.make(config)`                                | scoped make    | construction inside a larger acquire graph     |
|  [04]   | `MssqlClient.makeCompiler(transform?)`                    | compiler       | `dialect: "mssql"` harness; lights `onDialect` |
|  [05]   | `MssqlClient.defaultParameterTypes`                       | type map       | `PrimitiveKind`→`DataType` default binding     |

[ENTRYPOINT_SCOPE]: composing and invoking a stored procedure
- `Procedure.make`→`param`/`outputParam`/`withRows` accrete typed input/output parameters and the result-set row type, then `compile(self)(input)` binds concrete input values into a `ProcedureWithValues` the client's `call` runs. `Parameter.make` builds a standalone typed parameter; `MssqlClient.param` splices a `DataType`-typed value directly into a `Fragment` for inline T-SQL.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Procedure.make(name)`                        | proc builder    | empty `Procedure<{}, {}>` seed                     |
|  [02]   | `Procedure.param<A>()(name, type, options?)`  | input param     | accrete a typed input into `I`                     |
|  [03]   | `Procedure.outputParam<A>()(name, type, ...)` | output param    | accrete a typed output into `O`                    |
|  [04]   | `Procedure.withRows<A>()`                     | row type        | declare the decoded result-set element type        |
|  [05]   | `Procedure.compile(self)(input)`              | bind values     | `Procedure` + input record → `ProcedureWithValues` |
|  [06]   | `Parameter.make(name, type, options?)`        | typed parameter | standalone `DataType`-named value                  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MssqlClient` serves read-oriented ingress alone: an app reads enterprise SQL Server through this Tag and folds facts INTO the append-only journal, and no fact folds back as authority.

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): inherits every query/transaction/typed-IO surface — `SqlSchema` decodes interop rows into `Schema` models, `SqlResolver` batches the read side, `withTransaction` scopes a multi-statement read; the `dialect: "mssql"` compiler realizes the `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` `mssql` arm, emitting T-SQL from the shared definition rather than a parallel journal.
- `data` folder: one `lane/mssql` interop row whose typed `param`/`call` serve read-side procedure ingress; reactive read-your-writes and LISTEN/NOTIFY stay pg-spine capabilities absent on this lane.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; interop rows yield the neutral `SqlClient` and reach the concrete `MssqlClient` Tag solely for construction and the `param`/`call` surface.
- `password` rides `Config.redacted`; pool sizing, `encrypt`/`trustServer` TLS posture, and named-instance auth are `Config`/`iac` facts, never row literals.
- Stored procedures compose `Procedure.make`→`param`/`outputParam`/`withRows`→`compile` and run via `MssqlClient.call`; inline typed values splice through `MssqlClient.param` naming a `MssqlTypes` `DataType`, never a raw string-built parameter.
- `MssqlMigrator` is banned branch-wide — DDL is `iac`↔`store` declarative ensure, runtime never mutates.

[RAIL_LAW]:
- Package: `@effect/sql-mssql`
- Owns: the `tedious` binding of `SqlClient` — `layer`/`layerConfig`/`make`/`makeCompiler`/`defaultParameterTypes`, the `MssqlClientConfig` family, the typed `param` fragment and stored-procedure `call`, the `Parameter`/`Procedure` builders, the `MssqlTypes` `DataType` catalog, the `dialect: "mssql"` compiler lighting the `sql.onDialect` `mssql` arm, and the banned `MssqlMigrator`
- Accept: the read-oriented interop row under the one `@effect/sql` contract, `Config`-sourced credentials, T-SQL through the `mssql` `onDialect` arm, typed procedure ingress via `call`, facts folded into the journal
- Reject: a driver import in a neutral row, SQL Server as a record of truth, `MssqlMigrator` or any runtime schema mutation, hardcoded credentials or pool sizes, raw string-built parameters bypassing `param`/`Parameter`, a second relational contract for interop
