# [TS_DATA_API_EFFECT_SQL_MSSQL]

`@effect/sql-mssql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `tedious` SQL Server wire as the read-oriented interop lane, adding SQL Server's own shape — the typed `param` fragment, the stored-procedure `call`, the `Procedure`/`Parameter` builders, the `MssqlTypes` `DataType` catalog — atop the inherited query, transaction, and typed-IO spine. Its `dialect: "mssql"` compiler lights the `sql.onDialect` `mssql` arm, emitting T-SQL from one statement definition; `MssqlMigrator` ships branch-banned.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `MssqlClient` service and its config
- `MssqlClient extends SqlClient`: providing the layer yields both Tags, so interop rows compose the neutral `SqlClient` and only construction with the SQL-Server-specific `config`/`param`/`call` reaches the concrete Tag. `MssqlClientConfig` carries the connection shape the rows below enumerate; every field but `server` is optional, and `parameterTypes` swaps the compiler's whole `PrimitiveKind`→`DataType` fallback map.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                 |
| :-----: | :----------------------------------------------------- | :------------------ | :-------------------------------------------------- |
|  [01]   | `MssqlClient` (Tag) / `interface MssqlClient`          | service Tag         | `read/query` interop row; only ctor reaches Tag     |
|  [02]   | `MssqlClient.config: MssqlClientConfig`                | resolved config     | span/transform/parameter-type introspection         |
|  [03]   | `MssqlClient.param(type, value, options?)`             | typed fragment      | `DataType`-bound `Fragment`; T-SQL parameter splice |
|  [04]   | `MssqlClient.call(procedure)`                          | stored-proc invoke  | run a `ProcedureWithValues` → typed output + rows   |
|  [05]   | `MssqlClientConfig.server` (required)                  | connection          | host or named endpoint; the one non-optional field  |
|  [06]   | `MssqlClientConfig.database`/`.username`               | connection          | discrete target DB + login; `Config`-sourced        |
|  [07]   | `MssqlClientConfig.domain`/`.instanceName`/`.authType` | auth shape          | Windows-domain and named-instance authentication    |
|  [08]   | `MssqlClientConfig.encrypt`/`.trustServer`             | TLS posture         | `encrypt` true, `trustServer` false by default      |
|  [09]   | `MssqlClientConfig.password` (`Redacted.Redacted`)     | credential          | pool auth; never a literal                          |
|  [10]   | `MssqlClientConfig.minConnections`/`.maxConnections`   | pool sizing         | per-app pool budget; `connectionTTL` a `Duration`   |
|  [11]   | `MssqlClientConfig.parameterTypes`                     | type override       | `Record<string, DataType>` replacing the default    |
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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- `layer`/`layerConfig` yield `MssqlClient | SqlClient` in one Layer under error `ConfigError | SqlError`; `make` returns `Effect<MssqlClient, SqlError, Scope | Reactivity>`. `make` — not the compiler — seeds every span: `db.system.name=microsoft.sql_server`, `server.address` off the required `server`, and `db.namespace` and `server.port` off the optional fields with `master` and `1433` filled where absent.
- `makeCompiler` fixes `dialect: "mssql"` on bracket identifier escaping and positional parameter names, folding `RETURNING` into `OUTPUT INSERTED.*` for both insert and multi-row update. `defaultParameterTypes` is the built-in `Record<Statement.PrimitiveKind, DataType>` the compiler resolves an unannotated value's kind through; `MssqlClientConfig.parameterTypes` REPLACES that map whole rather than merging into it, so a partial override drops every kind it omits.
- `MssqlClient.param` is `Statement.custom("MssqlParam")` and the compiler's `onCustom` arm passes its triple straight to the driver's own `addParameter`, so a `param` value bypasses `PrimitiveKind` inference entirely — that is the one path a declared `DataType` reaches the wire on.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MssqlClient` serves read-oriented ingress alone: an app reads enterprise SQL Server through this Tag and folds facts INTO the append-only journal, and no fact folds back as authority.

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): inherits every query/transaction/typed-IO surface — `SqlSchema` decodes interop rows into `Schema` models, `SqlResolver` batches the read side, `withTransaction` scopes a multi-statement read; the `dialect: "mssql"` compiler realizes the `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` `mssql` arm, emitting T-SQL from the shared definition rather than a parallel journal.
- `data` folder: one `read/query` interop row whose typed `param`/`call` serve read-side procedure ingress; reactive read-your-writes and LISTEN/NOTIFY stay pg-spine capabilities absent on this lane.

[LOCAL_ADMISSION]:
- Provide the layer at the app root only; interop rows yield the neutral `SqlClient` and reach the concrete `MssqlClient` Tag solely for construction and the `param`/`call` surface.
- `password` rides `Config.redacted`; pool sizing, `encrypt`/`trustServer` TLS posture, and named-instance auth are `Config`/`iac` facts, never row literals.
- Both TLS knobs stay unset: the driver's own defaults encrypt the wire and validate the certificate, so `encrypt: false` transmits the credential in cleartext and `trustServer: true` accepts any presented certificate.
- No pool-adoption entrypoint ships: `layer`/`layerConfig`/`make` each build their own pool, so one composition owns one pool per interop lane and the pg spine's `layerFromPool` fan-out has no counterpart here.
- Stored procedures compose `Procedure.make`→`param`/`outputParam`/`withRows`→`compile` and run via `MssqlClient.call`; inline typed values splice through `MssqlClient.param` naming a `MssqlTypes` `DataType`, never a raw string-built parameter.
- `Procedure.withRows<A>()` re-types the result set and decodes nothing, so a `Procedure.Result`'s `rows` and `output` scalars pass a `Schema` before domain code reads them, exactly as `SqlSchema` proves a `Connection.Row`.
- `MssqlTypes` resolves through the package root alone — the distribution publishes no `MssqlTypes` subpath, so a deep import of the `DataType` catalog does not exist.
- `MssqlMigrator` is banned branch-wide — DDL is `iac`↔`data` declarative ensure, runtime never mutates.
