# [RASM_PERSISTENCE_API_SQLITE]

`Microsoft.Data.Sqlite` supplies SQLite connections, commands, transactions,
parameters, readers, blobs, extension loading, function registration, and backup
operations for embedded store profiles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite`
- assembly: `Microsoft.Data.Sqlite`
- namespace: `Microsoft.Data.Sqlite`
- asset: provider admission and runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: connection and command surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :------------------------------ | :----------------- | :------------------------ |
|  [01]   | `SqliteConnection`              | connection         | opens embedded store      |
|  [02]   | `SqliteConnectionStringBuilder` | connection builder | builds connection strings |
|  [03]   | `SqliteCommand`                 | command            | executes statements       |
|  [04]   | `SqliteTransaction`             | transaction        | bounds atomic work        |
|  [05]   | `SqliteDataReader`              | data reader        | reads result rows         |
|  [06]   | `SqliteParameter`               | parameter          | binds statement values    |
|  [07]   | `SqliteParameterCollection`     | parameter store    | owns parameters           |
|  [08]   | `SqliteException`               | provider exception | reports provider failure  |

[STORE_TYPES]: embedded store extensions
- rail: store-provider

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :---------------- | :--------------- | :------------------------ |
|  [01]   | `SqliteBlob`      | blob handle      | streams blob content      |
|  [02]   | `SqliteFactory`   | provider factory | creates provider objects  |
|  [03]   | `SqliteOpenMode`  | open classifier  | classifies open behavior  |
|  [04]   | `SqliteCacheMode` | cache classifier | classifies cache behavior |
|  [05]   | `SqliteType`      | type classifier  | classifies value binding  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and execution
- rail: store-provider

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]          |
| :-----: | :----------------- | :------------------ | :-------------------- |
|  [01]   | `Open`             | connection call     | opens store           |
|  [02]   | `OpenAsync`        | async call          | opens store           |
|  [03]   | `CreateCommand`    | factory call        | creates command       |
|  [04]   | `BeginTransaction` | transaction factory | starts transaction    |
|  [05]   | `ExecuteReader`    | command call        | reads rows            |
|  [06]   | `ExecuteNonQuery`  | command call        | writes changes        |
|  [07]   | `ExecuteScalar`    | command call        | reads scalar value    |
|  [08]   | `BackupDatabase`   | connection call     | copies store database |

[ENTRYPOINT_SCOPE]: embedded features
- rail: store-provider

| [INDEX] | [SURFACE]             | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :-------------------- | :--------------- | :------------------------ |
|  [01]   | `CreateFunction`      | connection call  | registers scalar function |
|  [02]   | `CreateAggregate`     | connection call  | registers aggregate       |
|  [03]   | `EnableExtensions`    | connection call  | enables extension loading |
|  [04]   | `LoadExtension`       | connection call  | loads native extension    |
|  [05]   | `new SqliteBlob(...)` | constructor call | opens blob stream         |
|  [06]   | `Read` / `Write`      | blob stream call | moves blob bytes          |
|  [07]   | `CreateCollation`     | connection call  | registers collation       |
|  [08]   | `GetStream`           | data reader call | streams blob column       |
|  [09]   | `Handle`              | connection read  | exposes raw handle        |

## [04]-[IMPLEMENTATION_LAW]

[EMBEDDED_STORE]:
- namespace: `Microsoft.Data.Sqlite`
- connection root: `SqliteConnection`
- command root: `SqliteCommand`
- transaction root: `SqliteTransaction`
- parameter root: `SqliteParameter`
- extension root: function, aggregate, blob, backup, and native extension calls

[LOCAL_ADMISSION]:
- Raw SQLite APIs stay beneath the unified store-profile rail.
- SQL text, parameters, transactions, and blob streams require query-shape ownership.
- Extension loading is an explicit profile capability and never ambient behavior.
- Backup operations emit snapshot and receipt projections.

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite`
- Owns: embedded SQLite transport API
- Accept: SQLite provider implementation
- Reject: raw SQLite public service families
