# [RASM_COMPUTE_API_SQLITE]

`Microsoft.Data.Sqlite` is a cross-tier package: its DURABLE half (the embedded store rail — the `Handle` raw bridge, blob streams, UDF/collation registration, backup, checkpoint policy) is Persistence-owned and the canonical full-surface catalog is `libs/csharp/Rasm.Persistence/.api/api-sqlite.md`. Compute owns ONE slice: the read-only ADO.NET reader over the EnergyPlus `eplusout.sql` results database — the `Analysis/energy#SUBPROCESS_RESULTS` tabular fold reading the `TabularDataWithStrings` report rows the SWIG `SqlFile` exposes no accessor and no generic SQL exec for (the setpoint-not-met occupied-hours validity rows ASHRAE 90.1 caps at ~300 h/yr). The rail is one bracketed `Mode=ReadOnly` connection per fold over the scratch artifact, ONE parameterized SELECT family discriminating on `(report, table, row, column)`, and scalar extraction onto `Option<double>` — an absent or malformed row is an absent fact, never a fabricated zero. This catalog mines that consumed lifecycle to operator depth; the store rail it sits beside stays at the Persistence catalog.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite` (Compute energy-results reader slice)
- package: `Microsoft.Data.Sqlite` — a meta-package: `Microsoft.Data.Sqlite.Core` carries the assembly (`lib/netstandard2.0/_._` is the meta placeholder; decompile evidence resolves through the Core package), `SQLitePCLRaw.bundle_e_sqlite3` carries the `e_sqlite3` native provider
- license: `MIT`
- canonical catalog: `libs/csharp/Rasm.Persistence/.api/api-sqlite.md` (the embedded-store rail + full package surface)
- assembly: `Microsoft.Data.Sqlite`; namespace `Microsoft.Data.Sqlite`; the `net10.0` consumer binds `lib/net8.0`
- rail: energy-results reader (read-only `eplusout.sql` tabular)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the consumed read-rail types (ADO.NET `Db*` subclasses; every handle-owning type is `IDisposable`)
- rail: energy-results

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                             |
| :-----: | :------------------------------ | :----------------- | :----------------------------------------------------------------------- |
|  [01]   | `SqliteConnection`              | `DbConnection`     | opens the results file; ctor binds the string, `Open()` does the IO      |
|  [02]   | `SqliteConnectionStringBuilder` | connection builder | keywords: `DataSource`/`Mode`/`Cache`/`Pooling`/`DefaultTimeout`/`Vfs`   |
|  [03]   | `SqliteOpenMode`                | open enum          | `ReadWriteCreate`, `ReadWrite`, `ReadOnly`, `Memory`                     |
|  [04]   | `SqliteCommand`                 | `DbCommand`        | parameterized SELECT carrier; `Prepare`/`ExecuteScalar`/`ExecuteReader`  |
|  [05]   | `SqliteParameterCollection`     | parameter store    | `AddWithValue` + typed `Add(name, SqliteType)`; `@`/`$`/`:` placeholders |
|  [06]   | `SqliteParameter`               | `DbParameter`      | one bind; `SqliteType` pins the storage class over inference             |
|  [07]   | `SqliteType`                    | type enum          | `Integer`/`Real`/`Text`/`Blob`; a string binds `Text` deterministically  |
|  [08]   | `SqliteDataReader`              | `DbDataReader`     | row folds; `Read`/`GetOrdinal`/typed getters/`GetValue`/`IsDBNull`       |
|  [09]   | `SqliteException`               | `DbException`      | provider failure with `SqliteErrorCode` + `SqliteExtendedErrorCode`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read-only connection lifecycle — one bracketed connection per fold
- rail: energy-results
- composition law: `Analysis/energy#SUBPROCESS_RESULTS`'s `TabularFacts(sqlPath)` opens `new SqliteConnection($"Data Source={sqlPath};Mode=ReadOnly;")` inside `using`, folds every tabular row over the one open handle, and disposes with the bracket — the solver's file takes no write lock and is never created or mutated

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]       | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `new SqliteConnection(string? connectionString)`             | ctor               | binds the string; zero IO until `Open()`      |
|  [02]   | `Open()` / `Close()` / `Dispose()`                           | lifecycle          | `sqlite3_open_v2` under the resolved flags    |
|  [03]   | `Mode=ReadOnly`                                              | keyword            | `SQLITE_OPEN_READONLY` — the loud-fail floor  |
|  [04]   | `Data Source=file:{path}?immutable=1`                        | keyword (URI)      | `SQLITE_OPEN_URI` + `immutable=1` sealed read |
|  [05]   | `Pooling=False`                                              | keyword            | disables default-ON pooling                   |
|  [06]   | `SqliteConnection.ClearPool(connection)` / `ClearAllPools()` | static call        | evicts pooled physical handles                |
|  [07]   | `Default Timeout` / `DefaultTimeout`                         | keyword / property | busy-timeout seconds (default 30)             |

- [02]-[OPEN_FAIL]: a missing file under `Mode=ReadOnly` throws `SqliteException` (error 14, `unable to open database file`).
- [03]-[READONLY]: the default `ReadWriteCreate` creates a missing path instead of failing — a silent empty database where the fold expects a failure.
- [04]-[IMMUTABLE]: a `file:`-prefixed source arms `SQLITE_OPEN_URI`; `immutable=1` skips locking and change detection over a sealed post-run artifact.
- [05]-[POOLING]: pooling defaults ON keyed by exact string; a scratch-artifact read disables it so the physical handle dies with the bracket.
- [06]-[CLEARPOOL]: a pooled handle otherwise outlives the bracket until the prune timer's first pass at 4 min, so eviction runs before the data source is deleted.
- [07]-[TIMEOUT]: irrelevant under `immutable=1`, load-bearing when the artifact is read while EnergyPlus still holds it.

[ENTRYPOINT_SCOPE]: the parameterized tabular query — `SqliteCommand` + binds
- rail: energy-results
- composition law: `Tabular(connection, report, table, row)` is the ONE query family — `SELECT Value FROM TabularDataWithStrings WHERE ReportName = $report AND TableName = $table AND RowName = $row LIMIT 1`, with `ColumnName` the fourth predicate when a table carries more than one column — never a per-metric method ladder

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]   | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `connection.CreateCommand()`                                  | factory call   | returns `SqliteCommand` bound to the connection |
|  [02]   | `command.CommandText`                                         | property       | the SQL; `@`/`$`/`:`-prefixed placeholders      |
|  [03]   | `command.Parameters.AddWithValue(string?, object?)`           | bind call      | infers the storage class from the CLR value     |
|  [04]   | `command.Parameters.Add(string?, SqliteType)`                 | bind call      | pins the storage class explicitly               |
|  [05]   | `command.Prepare()`                                           | statement call | precompiles into the statement cache            |
|  [06]   | `command.ExecuteScalar()`                                     | execute call   | first cell, `null` on empty → `Option` rail     |
|  [07]   | `command.ExecuteReader()` / `ExecuteReader(CommandBehavior)`  | execute call   | multi-row folds                                 |
|  [08]   | `SqliteException.SqliteErrorCode` / `SqliteExtendedErrorCode` | failure read   | numeric provider codes for the fault row        |

- [03]-[INFER]: a `string` binds `SqliteType.Text`, exactly what the string-keyed report/table/row predicates need.
- [04]-[PIN]: the route when the value type must not decide.
- [05]-[PREPARE]: pays off when one command re-executes across rebound parameters.
- [06]-[SCALAR]: `Optional(...)` absorbs both absence shapes (empty result, SQL `null`) onto the `Option` rail.
- [07]-[READER]: the route when a whole table (per-zone rows) folds instead of one scalar.
- [08]-[ERRORCODE]: the codes a `(Extraction, Foreign)` `ComputeFault.AnalysisFailed` row carries when the artifact is corrupt.

[ENTRYPOINT_SCOPE]: reader folds and schema probes — `SqliteDataReader`
- rail: energy-results

| [INDEX] | [SURFACE]                                                                | [CALL_SHAPE]   | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `reader.Read()` / `NextResult()` / `HasRows`                             | row fold       | row + statement advance                     |
|  [02]   | `reader.GetOrdinal(string)` / `GetName(int)`                             | column resolve | name→ordinal once, ordinal reads after      |
|  [03]   | `reader.GetValue(int)` / `GetString(int)`                                | value read     | reads the TEXT `Value` for invariant parse  |
|  [04]   | `reader.GetDouble(int)` / `GetInt64(int)`                                | value read     | coercing getters — the fabricated-zero trap |
|  [05]   | `reader.GetFieldValue<T>(int)` / `IsDBNull(int)`                         | value read     | generic typed read + null guard             |
|  [06]   | `reader.GetSchemaTable()` / `GetDataTypeName(int)` / `GetFieldType(int)` | result schema  | declared-type result metadata               |
|  [07]   | `SELECT name FROM sqlite_master WHERE type='table'`                      | SQL probe      | table listing                               |
|  [08]   | `SELECT * FROM pragma_table_info($table)`                                | SQL probe      | column listing                              |

- [01]-[ADVANCE]: `Read()` advances rows over one result; `NextResult()` advances across batched statements.
- [03]-[TEXTVALUE]: `TabularDataWithStrings.Value` is TEXT, so the fold reads the string and parses invariant `double.TryParse`.
- [04]-[COERCE]: `sqlite3_column_double`/`_int64` coerce non-numeric TEXT to `0.0` silently — the fabricated-zero shape the invariant-parse route rejects.
- [07]-[INTROSPECT]: `GetSchema` surfaces only `MetaDataCollections` and `ReservedWords`, so table/column probes run as SQL.

## [04]-[IMPLEMENTATION_LAW]

[READONLY_TOPOLOGY]:
- one connection per fold, bracketed `using` with the scratch directory's lifetime; the connection string is built from the resolved `sqlPath` parameter, never a literal path
- `Mode=ReadOnly` is the rail's floor: the open fails loudly on a missing artifact, the solver's file is never created or write-locked, and a write statement fails at the engine (`SQLITE_READONLY`, error 8) by open-flag construction — file mutation is unspellable on this connection
- the query family is total over `(report, table, row, column)` string keys; a new metric is a new key tuple in the caller's data row, never a new method
- absence is the rail's third value: an empty `ExecuteScalar`, a non-numeric `Value`, and a missing table all land `None` through `Optional` + invariant `double.TryParse`; only a corrupt artifact (a thrown `SqliteException`) escalates to the typed `(Extraction, Foreign)` fault row

[STACKING]:
- `NREL.OpenStudio.macOS-arm64` (`.api/api-openstudio.md`): the SWIG `SqlFile` owns every STRUCTURED summary read — `totalSiteEnergy`/`totalSourceEnergy`/`endUses`/`hoursSimulated` — over the same `eplusout.sql`; this rail reads only what `SqlFile` cannot spell, the `TabularDataWithStrings` report table, so the two readers partition the file by accessor coverage
- `PollinationSDK` (`.api/api-pollination-sdk.md`): the `EnergyRoute.Cloud` arm pulls the run's `eplusout.sql` asset and folds it through this identical extraction — one tabular reader serves both routes
- `Rasm.Persistence` (`libs/csharp/Rasm.Persistence/.api/api-sqlite.md`): the durable-store rail — `Handle` raw interop, `SqliteBlob` streams, UDF/collation/extension registration, backup and checkpoint — is Persistence-owned; Compute composes none of it and never grows a second SQLite rail beside this reader
- `Analysis/energy#SUBPROCESS_RESULTS`: the consumer — `TabularFacts`/`Tabular` fold the setpoint-not-met rows into `AssessmentFact.Measure` duration facts beside the `SqlFile` annual reads

[LOCAL_ADMISSION]:
- admitted in `Rasm.Compute.csproj` for exactly this rail; the version pin lives in the central `Directory.Packages.props`
- SQL text exists only inside the one query family; every predicate is a bound parameter — string interpolation into `CommandText` is unspellable because the family's signature carries the keys
- typed getters that coerce (`GetDouble`/`GetInt64` over TEXT) stay off the fold; the invariant-parse route owns numeric extraction

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite` (Compute energy-results reader slice; full surface at the Persistence catalog)
- Owns: the read-only tabular extraction over `eplusout.sql` — the `(report, table, row, column)`-keyed `TabularDataWithStrings` query family and its `Option<double>` scalar fold
- Accept: a post-run EnergyPlus results artifact in the bracketed scratch (local subprocess or cloud-pulled), read under `Mode=ReadOnly` with pooling disabled for the one-shot path
- Reject: any write, transaction, or `PRAGMA` mutation; a second SQLite rail in Compute (EF routes through Persistence `api-ef-sqlite`, raw interop through the Persistence `Handle` bridge); re-deriving a read the SWIG `SqlFile` already spells; coercing typed getters where the invariant parse owns the numeric fold
