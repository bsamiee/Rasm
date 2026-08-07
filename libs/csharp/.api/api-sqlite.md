# [RASM_API_SQLITE]

`Microsoft.Data.Sqlite` is the ADO.NET transport over the embedded `e_sqlite3` provider: `DbConnection`/`DbCommand`/`DbDataReader`/`DbTransaction` subclasses with blob streams, scalar and aggregate UDF registration, collations, extension loading, online backup, and pooling. Two folders bind disjoint rails: `Rasm.Persistence` owns the durable embedded-store rail — `SqliteConnection.Handle` (`SQLitePCL.sqlite3?`) is its seam to the raw `sqlite3_snapshot_*`, `sqlite3_wal_checkpoint_v2`, `sqlite3_db_config`, and paged `sqlite3_backup_*` calls the managed API never surfaces — and `Rasm.Compute` binds one read-only ADO fold over the EnergyPlus `eplusout.sql` results database: a bracketed `Mode=ReadOnly` connection carrying a parameterized SELECT family keyed on `(report, table, row, column)` onto `Option<double>`, so an absent or malformed row stays an absent fact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite` (MIT)
- assembly: `Microsoft.Data.Sqlite` — types ship in `Microsoft.Data.Sqlite.Core`; the meta-package binds the native provider bundle; the `net10.0` consumer binds the `lib/net8.0` asset
- namespace: `Microsoft.Data.Sqlite`
- depends: `SQLitePCLRaw.bundle_e_sqlite3` (`Rasm.Persistence/.api/api-sqlitepcl.md`) native provider; `Rasm.Persistence/.api/api-ef-sqlite.md` rides the EF provider
- asset: provider admission and runtime transport
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: connection and command surfaces (ADO.NET `Db*` subclasses)

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]               | [CAPABILITY]                                                            |
| :-----: | :------------------------------ | :-------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `SqliteConnection`              | `DbConnection`              | opens the store; owns `Handle`, pooling, backup, function registration  |
|  [02]   | `SqliteConnectionStringBuilder` | `DbConnectionStringBuilder` | `Mode`/`Cache`/`Pooling`/`ForeignKeys`/`Password`/`DefaultTimeout` keys |
|  [03]   | `SqliteCommand`                 | `DbCommand`                 | executes statements; `Prepare`/`Cancel`; async via base                 |
|  [04]   | `SqliteTransaction`             | `DbTransaction`             | bounds atomic work; deferred mode                                       |
|  [05]   | `SqliteDataReader`              | `DbDataReader`              | reads rows; `GetStream`/`GetTextReader`/`GetFieldValue<T>`              |
|  [06]   | `SqliteParameter`               | `DbParameter`               | binds typed statement values (`SqliteType`)                             |
|  [07]   | `SqliteParameterCollection`     | `DbParameterCollection`     | owns parameters; `AddWithValue` + typed `Add(name, SqliteType)`         |
|  [08]   | `SqliteException`               | `DbException`               | provider failure with `SqliteErrorCode` + `SqliteExtendedErrorCode`     |

[STORE_TYPES]: embedded store extensions and value classifiers

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                                      |
| :-----: | :---------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `SqliteBlob`      | `Stream`            | seekable blob stream; `byte[]` + `Span<byte>` IO  |
|  [02]   | `SqliteFactory`   | `DbProviderFactory` | creates provider objects                          |
|  [03]   | `SqliteOpenMode`  | enum                | `ReadWriteCreate`/`ReadWrite`/`ReadOnly`/`Memory` |
|  [04]   | `SqliteCacheMode` | enum                | shared vs private cache                           |
|  [05]   | `SqliteType`      | enum                | pins binding (`Integer`/`Real`/`Text`/`Blob`)     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection, execution, and the raw-handle bridge

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `Open()` / `OpenAsync(CancellationToken)`                           | instance | opens the embedded store                   |
|  [02]   | `Close()` / `CloseAsync()`                                          | instance | closes; returns the connection to its pool |
|  [03]   | `Handle` (`SQLitePCL.sqlite3?`)                                     | property | raw-call bridge to the native provider     |
|  [04]   | `CreateCommand()`                                                   | factory  | returns a `SqliteCommand`                  |
|  [05]   | `BeginTransaction(bool)` / `BeginTransaction(IsolationLevel, bool)` | factory  | deferred-capable, isolation-scoped         |
|  [06]   | `BeginTransactionAsync(...)`                                        | factory  | async transaction start                    |
|  [07]   | `ExecuteReader()` / `ExecuteReaderAsync(...)`                       | instance | reads rows                                 |
|  [08]   | `ExecuteNonQuery()` / `ExecuteNonQueryAsync(...)`                   | instance | writes changes                             |
|  [09]   | `ExecuteScalar()` / `ExecuteScalarAsync(...)`                       | instance | reads a scalar; `null` on empty            |
|  [10]   | `Prepare()` / `PrepareAsync(...)` / `Cancel()`                      | instance | precompiles / cancels a statement          |
|  [11]   | `BackupDatabase(SqliteConnection[, string, string])`                | instance | whole-file copy, optional named schemas    |
|  [12]   | `ClearPool(SqliteConnection)` / `ClearAllPools()`                   | static   | flushes pooled physical connections        |
|  [13]   | `DefaultTimeout` / `ServerVersion` / `DataSource` / `Database`      | property | busy-timeout policy and store identity     |

[ENTRYPOINT_SCOPE]: embedded features — functions, aggregates, collations, blobs, extensions

`CreateFunction` spans the arity-family, `TState`-stateful, and `object?[]`-params overloads; `CreateAggregate` spans the seeded/stateless and `resultSelector` families.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `CreateCollation(string, Comparison<string>)`                     | instance | registers a collation                      |
|  [02]   | `CreateCollation<T>(string, T, Func<T,string,string,int>)`        | instance | stateful collation                         |
|  [03]   | `CreateFunction<...TResult>(string, Func, bool)`                  | instance | registers a scalar UDF                     |
|  [04]   | `CreateAggregate<...TResult>(string, seed?, func, sel?, bool)`    | instance | registers an aggregate UDF                 |
|  [05]   | `EnableExtensions(bool)`                                          | instance | arms C-API extension loading               |
|  [06]   | `LoadExtension(string, string?)`                                  | instance | loads a native extension by path           |
|  [07]   | `new SqliteBlob(SqliteConnection, string, string, long, bool)`    | ctor     | opens a seekable blob stream               |
|  [08]   | `Read(byte[], int, int)` / `Read(Span<byte>)`                     | instance | zero-copy span/array read                  |
|  [09]   | `Write(byte[], int, int)` / `Write(ReadOnlySpan<byte>)`           | instance | zero-copy span/array write                 |
|  [10]   | `Seek(long, SeekOrigin)` / `Position`                             | instance | stream positioning                         |
|  [11]   | `GetStream(int)` / `GetTextReader(int)` / `GetFieldValue<T>(int)` | instance | streams a blob/text column, unmaterialized |
|  [12]   | `GetBytes(...)` / `GetChars(...)`                                 | instance | width-reads bytes/chars from a column      |

[ENTRYPOINT_SCOPE]: read-only results connection — the Compute `eplusout.sql` fold

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new SqliteConnection(string?)`                              | ctor     | binds the string; zero IO until `Open()`      |
|  [02]   | `Mode=ReadOnly`                                              | property | `SQLITE_OPEN_READONLY` — the loud-fail floor  |
|  [03]   | `Data Source=file:{path}?immutable=1`                        | property | `SQLITE_OPEN_URI` + `immutable=1` sealed read |
|  [04]   | `Pooling=False`                                              | property | disables default-on pooling for the one-shot  |
|  [05]   | `reader.GetOrdinal(string)` / `GetName(int)`                 | instance | name→ordinal once, ordinal reads after        |
|  [06]   | `reader.GetValue(int)` / `GetString(int)`                    | instance | reads the TEXT `Value` for invariant parse    |
|  [07]   | `reader.GetSchemaTable()` / `GetDataTypeName(int)`           | instance | declared-type result metadata                 |
|  [08]   | `SELECT name FROM sqlite_master WHERE type='table'`          | fold     | table listing                                 |
|  [09]   | `SELECT * FROM pragma_table_info($table)`                    | fold     | column listing                                |

- [02]-[READONLY]: `Mode=ReadOnly` throws `SqliteException` (error 14) on a missing file, where the default `ReadWriteCreate` silently creates an empty database — the fold needs the loud failure.
- [03]-[IMMUTABLE]: each `file:`-prefixed source arms `SQLITE_OPEN_URI`; `immutable=1` skips locking and change detection over a sealed post-run artifact; busy-timeout is moot under it, load-bearing when EnergyPlus still holds the artifact.
- [04]-[POOLING]: pooling defaults on keyed by exact string; `Pooling=False` or `ClearPool`/`ClearAllPools` drops the physical handle with the bracket, before the scratch source is deleted.
- [06]-[COERCE]: `sqlite3_column_double`/`_int64` coerce non-numeric TEXT to `0.0` silently — the fabricated-zero shape the invariant `double.TryParse` route rejects; `TabularDataWithStrings.Value` is TEXT, so the fold reads the string.
- [07]-[INTROSPECT]: `GetSchema` surfaces only `MetaDataCollections` and `ReservedWords`, so table and column probes run as SQL.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SqliteConnection.Handle` (`SQLitePCL.sqlite3?`) and the ADO subclasses share one native `e_sqlite3` connection, so raw `sqlite3_snapshot_*`/`sqlite3_wal_checkpoint_v2`/`sqlite3_db_config`/`sqlite3_backup_*` policy layers onto the managed surface through the same handle.
- A `SqliteCommand` executing under an active transaction carries it on `SqliteCommand.Transaction`, else the provider throws `InvalidOperationException` at execute.
- `*Async` members are `System.Data.Common` base schedulers over a synchronous engine, a mirror never a native async path; `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`/`SqliteTransaction` implement `IAsyncDisposable`.
- The Compute results fold binds one connection per fold, bracketed `using` over the scratch directory's lifetime, connection string built from the resolved `sqlPath` parameter, never a literal path; `Mode=ReadOnly` is that rail's floor — the open fails loudly on a missing artifact, the solver's file is never created or write-locked, and a write statement faults at the engine (`SQLITE_READONLY`, error 8) by open-flag construction.
- One `(report, table, row, column)`-keyed query family covers every Compute metric; a new metric is a new key tuple in the caller's data row, never a new method. Absence is that rail's third value: an empty `ExecuteScalar`, a non-numeric `Value`, and a missing table all land `None` through `Optional` + invariant `double.TryParse`; only a corrupt artifact (a thrown `SqliteException`, its `SqliteErrorCode`/`SqliteExtendedErrorCode` on the fault row) escalates to the typed `(Extraction, Foreign)` `ComputeFault.AnalysisFailed` row.

[STACKING]:
- `SQLitePCLRaw`(`Rasm.Persistence/.api/api-sqlitepcl.md`): `Handle` carries every raw `sqlite3_*` call; paged `sqlite3_backup_*` over `Handle` subsumes the whole-file `BackupDatabase`.
- `EF SQLite`(`Rasm.Persistence/.api/api-ef-sqlite.md`): the EF provider maps the model over this transport; `Rasm.Persistence/.api/api-thinktecture-ef.md` adds value-object conversions — this package owns transport, EF owns mapping.
- `NREL.OpenStudio.macOS-arm64`(`api-openstudio.md`): the SWIG `SqlFile` owns the structured summary reads (`totalSiteEnergy`/`totalSourceEnergy`/`endUses`) over the same `eplusout.sql`; the Compute rail reads only the `TabularDataWithStrings` table `SqlFile` cannot spell, partitioning the file by accessor coverage.
- `PollinationSDK`(`api-pollination-sdk.md`): `EnergyRoute.Cloud` pulls the run's `eplusout.sql` asset and folds it through the identical extraction — one tabular reader serves both routes.
- Persistence consumer anchor: `Store/provisioning#ENGINE_OPERATIONS` capsules the ADO + raw-interop ceremony — pins typed `SqliteType` on `SqliteParameter` for `Checkpoint`/`Backup` binds, streams blobs through the constructed `SqliteBlob` write and `GetStream` read, steps paged `sqlite3_backup_*` over `Handle`; `Store/provisioning#EMBEDDED_FLOOR` registers `uuid7`/`xxh128` scalar UDFs and an `instant_iso` collation as connection-scoped `Capabilities`, and applies the defensive `sqlite3_db_config` set through `Handle`, keeping `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` absent so the loader arms only per deployment.
- Compute consumer anchor: `Analysis/energy#SUBPROCESS_RESULTS` — `TabularFacts`/`Tabular` fold the `(report, table, row)`-keyed rows into `AssessmentFact.Measure` duration facts beside the `SqlFile` annual reads.

[LOCAL_ADMISSION]:
- Persistence: SQL text, parameters, transactions, and blob streams pass through query-shape ownership beneath the unified store-profile rail; extension loading is an explicit profile capability, never ambient; backup and checkpoint operations emit typed `SqliteFact` snapshot/receipt projections.
- Compute: SQL text exists only inside the one query family — every predicate is a bound parameter, so `CommandText` interpolation has no site; coercing typed getters (`GetDouble`/`GetInt64` over TEXT) stay off the fold, the invariant-parse route owns numeric extraction.

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite`
- Owns: the embedded SQLite ADO.NET transport, the `Handle` bridge to the raw provider, and the read-only `(report, table, row, column)`-keyed tabular extraction over `eplusout.sql`
- Accept: the `SqliteBlob` span stream and typed reader/parameter surfaces at Persistence; a post-run EnergyPlus results artifact in the bracketed scratch, read under `Mode=ReadOnly` with pooling disabled, at Compute
- Reject: raw SQLite public service families; whole-payload blob materialization; `AddWithValue` type inference on maintenance binds; any Compute write, transaction, or `PRAGMA` mutation; a second SQLite rail in Compute (EF through `api-ef-sqlite`, raw interop through the `Handle` bridge); re-deriving a read the SWIG `SqlFile` already spells; coercing typed getters where the invariant parse owns the numeric fold
