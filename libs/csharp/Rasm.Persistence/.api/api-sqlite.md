# [RASM_PERSISTENCE_API_SQLITE]

`Microsoft.Data.Sqlite` is the ADO.NET transport over the embedded `e_sqlite3` provider: `DbConnection`/`DbCommand`/`DbDataReader`/`DbTransaction` subclasses with blob streams, scalar and aggregate UDF registration, collations, extension loading, online backup, and pooling. `SqliteConnection.Handle` (`SQLitePCL.sqlite3?`) is its seam to the embedded-store rail — the bridge reaching the raw `sqlite3_snapshot_*`, `sqlite3_wal_checkpoint_v2`, `sqlite3_db_config`, and paged `sqlite3_backup_*` calls the managed API never surfaces.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite` (MIT)
- assembly: `Microsoft.Data.Sqlite` — types ship in `Microsoft.Data.Sqlite.Core`; the meta-package binds the native provider bundle
- namespace: `Microsoft.Data.Sqlite`
- depends: `SQLitePCLRaw.bundle_e_sqlite3` (`api-sqlitepcl`) native provider; `api-ef-sqlite` rides the EF provider
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
|  [07]   | `SqliteParameterCollection`     | `DbParameterCollection`     | owns parameters                                                         |
|  [08]   | `SqliteException`               | `DbException`               | reports provider failure with `SqliteErrorCode`                         |

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
|  [03]   | `Handle` (`SQLitePCL.sqlite3?`)                                     | property | raw-call bridge to `api-sqlitepcl`         |
|  [04]   | `CreateCommand()`                                                   | factory  | returns a `SqliteCommand`                  |
|  [05]   | `BeginTransaction(bool)` / `BeginTransaction(IsolationLevel, bool)` | factory  | deferred-capable, isolation-scoped         |
|  [06]   | `BeginTransactionAsync(...)`                                        | factory  | async transaction start                    |
|  [07]   | `ExecuteReader()` / `ExecuteReaderAsync(...)`                       | instance | reads rows                                 |
|  [08]   | `ExecuteNonQuery()` / `ExecuteNonQueryAsync(...)`                   | instance | writes changes                             |
|  [09]   | `ExecuteScalar()` / `ExecuteScalarAsync(...)`                       | instance | reads a scalar                             |
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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SqliteConnection.Handle` (`SQLitePCL.sqlite3?`) and the ADO subclasses share one native `e_sqlite3` connection, so raw `sqlite3_snapshot_*`/`sqlite3_wal_checkpoint_v2`/`sqlite3_db_config`/`sqlite3_backup_*` policy layers onto the managed surface through the same handle.
- `SqliteCommand` executes under active transaction carries it on `SqliteCommand.Transaction`, else the provider throws `InvalidOperationException` at execute.
- `*Async` members are `System.Data.Common` base schedulers over a synchronous engine, a mirror never a native async path; `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`/`SqliteTransaction` implement `IAsyncDisposable`.

[STACKING]:
- `api-sqlitepcl`(`.api/api-sqlitepcl.md`): `Handle` carries every raw `sqlite3_*` call; paged `sqlite3_backup_*` over `Handle` subsumes the whole-file `BackupDatabase`.
- `api-ef-sqlite`(`.api/api-ef-sqlite.md`): the EF provider maps the model over this transport; `api-thinktecture-ef`(`.api/api-thinktecture-ef.md`) adds value-object conversions — this package owns transport, EF owns mapping.
- `Store/provisioning#ENGINE_OPERATIONS` capsules the ADO + raw-interop ceremony: pins typed `SqliteType` on `SqliteParameter` for `Checkpoint`/`Backup` binds, streams blobs through the constructed `SqliteBlob` write and `GetStream` read, and steps paged `sqlite3_backup_*` over `Handle`.
- `Store/provisioning#EMBEDDED_FLOOR`: registers `uuid7`/`xxh128` scalar UDFs and an `instant_iso` collation as connection-scoped `Capabilities`, and applies the defensive `sqlite3_db_config` set through `Handle`, keeping `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` absent so the loader arms only per deployment.

[LOCAL_ADMISSION]:
- SQL text, parameters, transactions, and blob streams pass through query-shape ownership beneath the unified store-profile rail.
- Extension loading is an explicit profile capability, never ambient.
- Backup and checkpoint operations emit typed `SqliteFact` snapshot/receipt projections.

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite`
- Owns: the embedded SQLite ADO.NET transport and the `Handle` bridge to the raw provider
- Accept: the `SqliteBlob` span stream and typed reader/parameter surfaces
- Reject: raw SQLite public service families; whole-payload blob materialization; `AddWithValue` type inference on maintenance binds
