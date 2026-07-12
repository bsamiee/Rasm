# [RASM_PERSISTENCE_API_SQLITE]

`Microsoft.Data.Sqlite` is the ADO.NET surface over the `e_sqlite3` provider (`api-sqlitepcl`):
`DbConnection`/`DbCommand`/`DbDataReader`/`DbTransaction` subclasses plus blob streams, scalar and
aggregate function registration, collations, extension loading, online backup, and connection
pooling. Its load-bearing seam for the embedded store rail is `SqliteConnection.Handle`
(`SQLitePCL.sqlite3?`) — the bridge that lets the engine reach the raw `sqlite3_snapshot_*`,
`sqlite3_wal_checkpoint_v2`, `sqlite3_db_config`, and paged `sqlite3_backup_*` calls the managed
API does not surface. The full ADO async mirror (`OpenAsync`/`ExecuteReaderAsync`/…) is inherited
from the `System.Data.Common` base types and returns `Sqlite*` results.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite`
- license: `MIT`
- assembly: `Microsoft.Data.Sqlite` (the runtime type lives in `Microsoft.Data.Sqlite.Core`; the meta-package is a native-provider convenience referencing the bundle)
- bound TFM: `lib/net8.0` (the `net10.0` consumer binds `net8.0` over `netstandard2.0`)
- namespace: `Microsoft.Data.Sqlite`
- native provider: `SQLitePCLRaw.bundle_e_sqlite3` (`api-sqlitepcl`); the EF provider rides through `api-ef-sqlite`
- asset: provider admission and runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: connection and command surfaces (ADO.NET `Db*` subclasses)
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                                           |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `SqliteConnection`              | `DbConnection`     | opens embedded store; `Handle`, pooling, backup, functions                             |
|  [02]   | `SqliteConnectionStringBuilder` | connection builder | `Mode`/`Cache`/`Pooling`/`ForeignKeys`/`RecursiveTriggers`/`Password`/`DefaultTimeout` |
|  [03]   | `SqliteCommand`                 | `DbCommand`        | executes statements; `Prepare`/`Cancel`; async via base                                |
|  [04]   | `SqliteTransaction`             | `DbTransaction`    | bounds atomic work; deferred-mode supported                                            |
|  [05]   | `SqliteDataReader`              | `DbDataReader`     | reads rows; `GetStream`/`GetTextReader`/`GetFieldValue<T>`                             |
|  [06]   | `SqliteParameter`               | `DbParameter`      | binds typed statement values (`SqliteType`)                                            |
|  [07]   | `SqliteParameterCollection`     | parameter store    | owns parameters                                                                        |
|  [08]   | `SqliteException`               | `DbException`      | reports provider failure with `SqliteErrorCode`                                        |

[STORE_TYPES]: embedded store extensions and classifiers
- rail: store-provider

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]      | [CAPABILITY]                                                          |
| :-----: | :---------------- | :------------------ | :-------------------------------------------------------------------- |
|  [01]   | `SqliteBlob`      | `Stream` handle     | seekable blob stream; `byte[]` + `Span<byte>`/`ReadOnlySpan<byte>` IO |
|  [02]   | `SqliteFactory`   | `DbProviderFactory` | creates provider objects                                              |
|  [03]   | `SqliteOpenMode`  | open enum           | `ReadWriteCreate`, `ReadWrite`, `ReadOnly`, `Memory`                  |
|  [04]   | `SqliteCacheMode` | cache enum          | classifies shared/private cache                                       |
|  [05]   | `SqliteType`      | type enum           | pins value binding (`Integer`/`Real`/`Text`/`Blob`)                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection, execution, and the raw-handle bridge
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                                            | [CALL_SHAPE]        | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `Open()` / `OpenAsync(CancellationToken)`                                                                            | connection call     | opens store; async mirror inherited from `DbConnection`          |
|  [02]   | `Close()` / `CloseAsync()`                                                                                           | connection call     | closes; returns the connection to its pool                       |
|  [03]   | `Handle` (`sqlite3? Handle`)                                                                                         | connection read     | the `SQLitePCL.sqlite3` bridge to the `api-sqlitepcl` raw calls  |
|  [04]   | `CreateCommand()`                                                                                                    | factory call        | returns `SqliteCommand` (via `DbConnection`)                     |
|  [05]   | `BeginTransaction(bool deferred)` / `BeginTransaction(IsolationLevel, bool deferred)` / `BeginTransactionAsync(...)` | transaction factory | starts a (deferred) transaction; async mirror inherited          |
|  [06]   | `ExecuteReader()` / `ExecuteReaderAsync(...)`                                                                        | command call        | reads rows; async mirror inherited from `DbCommand`              |
|  [07]   | `ExecuteNonQuery()` / `ExecuteNonQueryAsync(...)`                                                                    | command call        | writes changes; async mirror inherited                           |
|  [08]   | `ExecuteScalar()` / `ExecuteScalarAsync(...)`                                                                        | command call        | reads a scalar; async mirror inherited                           |
|  [09]   | `Prepare()` / `PrepareAsync(...)` / `Cancel()`                                                                       | command call        | precompiles / cancels a statement                                |
|  [10]   | `BackupDatabase(SqliteConnection)` / `BackupDatabase(SqliteConnection, string destName, string srcName)`             | connection call     | whole-file copy; the engine prefers the paged raw backup session |
|  [11]   | `ClearPool(SqliteConnection)` / `ClearAllPools()`                                                                    | static call         | flushes pooled physical connections                              |
|  [12]   | `DefaultTimeout` / `ServerVersion` / `DataSource` / `Database`                                                       | connection read     | busy-timeout policy and store identity facts                     |

[ENTRYPOINT_SCOPE]: embedded features — functions, collations, blobs, extensions
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                                    | [CALL_SHAPE]     | [CAPABILITY]                                                                    |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------ |
|  [01]   | `CreateFunction<…,TResult>(name, Func<…>, isDeterministic)`                                                  | connection call  | registers a scalar UDF; arity-0..15 + `object?[]` params overloads              |
|  [02]   | `CreateAggregate<…TAccumulate[,TResult]>(name, [seed,] func, [resultSelector,] isDeterministic)`             | connection call  | registers an aggregate; seeded/stateless + resultSelector families              |
|  [03]   | `CreateCollation(name, Comparison<string>)` / `CreateCollation<T>(name, T state, Func<T,string,string,int>)` | connection call  | registers a (stateful) collation                                                |
|  [04]   | `EnableExtensions(bool enable = true)`                                                                       | connection call  | arms C-API extension loading                                                    |
|  [05]   | `LoadExtension(string file, string? proc = null)`                                                            | connection call  | loads a native extension by path                                                |
|  [06]   | `new SqliteBlob(connection, table, column, rowid, readOnly)`                                                 | constructor call | opens a seekable blob stream                                                    |
|  [07]   | `Read(byte[]…)` / `Read(Span<byte>)` / `Write(byte[]…)` / `Write(ReadOnlySpan<byte>)` / `Seek` / `Position`  | blob stream call | zero-copy span blob IO; full `Stream` (`CanRead`/`CanWrite`/`CanSeek`/`Length`) |
|  [08]   | `GetStream(int)` / `GetTextReader(int)` / `GetFieldValue<T>(int)` / `GetBytes` / `GetChars`                  | data reader call | streams or width-reads a blob/text column without materializing it              |

## [04]-[IMPLEMENTATION_LAW]

[EMBEDDED_STORE]:
- namespace: `Microsoft.Data.Sqlite`
- connection root: `SqliteConnection` (with `Handle`, pooling, `BackupDatabase`, function/collation registration)
- command root: `SqliteCommand` (`Prepare`/`Cancel` + the inherited async mirror)
- transaction root: `SqliteTransaction` (deferred-mode via `BeginTransaction(bool deferred)`)
- parameter root: `SqliteParameter` (typed binding through `SqliteType`)
- reader root: `SqliteDataReader` (`GetStream`/`GetTextReader`/`GetFieldValue<T>` typed/streamed access)
- extension root: function, aggregate, collation, blob, backup, and native extension calls

[HANDLE_BRIDGE]:
- `SqliteConnection.Handle` is `SQLitePCL.sqlite3?` — the same handle type the `api-sqlitepcl` raw calls accept; this is the one seam joining the managed ADO surface to the low-level `sqlite3_snapshot_*`/`sqlite3_wal_checkpoint_v2`/`sqlite3_db_config`/`sqlite3_backup_*` API
- the bound `e_sqlite3` provider is shared: opening a `SqliteConnection` and issuing a raw call through `Handle` target the same native connection, so the engine layers raw snapshot/checkpoint policy onto an ordinary ADO connection

[ASYNC_MIRROR]:
- `OpenAsync`/`CloseAsync`/`ExecuteReaderAsync`/`ExecuteNonQueryAsync`/`ExecuteScalarAsync`/`BeginTransactionAsync`/`PrepareAsync` are the `System.Data.Common` base async members; SQLite executes synchronously underneath, so these are scheduling mirrors over the same provider, not a separate native async path
- `SqliteConnection`/`SqliteCommand`/`SqliteDataReader`/`SqliteTransaction` implement `IAsyncDisposable`

[INTEGRATION_STACK]:
- `Store/provisioning#ENGINE_OPERATIONS` is the boundary capsule over this ADO + raw-interop ceremony: the `Checkpoint`/`Backup` binds pin typed `SqliteType` on a typed `SqliteParameter` (no `AddWithValue` inference); blob payloads stream through the constructed `SqliteBlob` write stream and the `GetStream` read path (whole-payload `byte[]` materialization is the deleted pattern); paged backup steps `sqlite3_backup_*` over `Handle` rather than the whole-file `BackupDatabase`
- `Store/provisioning#EMBEDDED_FLOOR` registers `uuid7` and `xxh128` scalar UDFs and an `instant_iso` collation via `CreateFunction`/`CreateCollation` as the open ritual's connection-scoped `Capabilities`, so the SQLite leg of the identity policy and chronological `ExtendedIso` ordering live as connection registrations
- `Store/provisioning#EMBEDDED_FLOOR` applies the defensive `sqlite3_db_config` set through the `Handle` raw bridge as connection policy; the SQL-level `load_extension()` function and the C-API loader both stay OFF (`SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` is absent from the `DbConfig` set), so the single-RID `e_sqlite3` dylib from `api-sqlitepcl` admits no runtime extension and arming the loader is a deliberate per-deployment opt-in
- the embedded profile pairs with `api-ef-sqlite` (the EF provider) and `api-thinktecture-ef` (value-object conversions) — this package owns the transport, EF owns the model mapping

[LOCAL_ADMISSION]:
- raw SQLite APIs stay beneath the unified store-profile rail; SQL text, parameters, transactions, and blob streams require query-shape ownership
- extension loading is an explicit profile capability and never ambient behavior
- backup and checkpoint operations emit typed snapshot/receipt projections (`SqliteFact`), never opaque side effects

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite`
- Owns: the embedded SQLite ADO.NET transport API and the `Handle` bridge to the raw provider
- Accept: SQLite provider implementation; the `SqliteBlob` span stream and typed reader/parameter surfaces
- Reject: raw SQLite public service families; whole-payload blob materialization; `AddWithValue` type inference on maintenance binds
