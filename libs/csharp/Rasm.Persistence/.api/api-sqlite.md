# [RASM_PERSISTENCE_API_SQLITE]

`Microsoft.Data.Sqlite` supplies SQLite connections, commands, transactions,
parameters, readers, blobs, extension loading, function registration, and backup
operations for embedded store profiles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite`
- assembly: `Microsoft.Data.Sqlite`
- namespace: `Microsoft.Data.Sqlite`
- asset: provider admission and runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: connection and command surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :------------------------------ | :----------------- | :------------------------ |
|   [1]   | `SqliteConnection`              | connection         | opens embedded store      |
|   [2]   | `SqliteConnectionStringBuilder` | connection builder | builds connection strings |
|   [3]   | `SqliteCommand`                 | command            | executes statements       |
|   [4]   | `SqliteTransaction`             | transaction        | bounds atomic work        |
|   [5]   | `SqliteDataReader`              | data reader        | reads result rows         |
|   [6]   | `SqliteParameter`               | parameter          | binds statement values    |
|   [7]   | `SqliteParameterCollection`     | parameter store    | owns parameters           |
|   [8]   | `SqliteException`               | provider exception | reports provider failure  |

[STORE_TYPES]: embedded store extensions
- rail: store-provider

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :---------------- | :--------------- | :------------------------ |
|   [1]   | `SqliteBlob`      | blob handle      | streams blob content      |
|   [2]   | `SqliteFactory`   | provider factory | creates provider objects  |
|   [3]   | `SqliteOpenMode`  | open classifier  | classifies open behavior  |
|   [4]   | `SqliteCacheMode` | cache classifier | classifies cache behavior |
|   [5]   | `SqliteType`      | type classifier  | classifies value binding  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and execution
- rail: store-provider

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]          |
| :-----: | :----------------- | :------------------ | :-------------------- |
|   [1]   | `Open`             | connection call     | opens store           |
|   [2]   | `OpenAsync`        | async call          | opens store           |
|   [3]   | `CreateCommand`    | factory call        | creates command       |
|   [4]   | `BeginTransaction` | transaction factory | starts transaction    |
|   [5]   | `ExecuteReader`    | command call        | reads rows            |
|   [6]   | `ExecuteNonQuery`  | command call        | writes changes        |
|   [7]   | `ExecuteScalar`    | command call        | reads scalar value    |
|   [8]   | `BackupDatabase`   | connection call     | copies store database |

[ENTRYPOINT_SCOPE]: embedded features
- rail: store-provider

| [INDEX] | [SURFACE]             | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :-------------------- | :--------------- | :------------------------ |
|   [1]   | `CreateFunction`      | connection call  | registers scalar function |
|   [2]   | `CreateAggregate`     | connection call  | registers aggregate       |
|   [3]   | `EnableExtensions`    | connection call  | enables extension loading |
|   [4]   | `LoadExtension`       | connection call  | loads native extension    |
|   [5]   | `new SqliteBlob(...)` | constructor call | opens blob stream         |
|   [6]   | `Read` / `Write`      | blob stream call | moves blob bytes          |
|   [7]   | `CreateCollation`     | connection call  | registers collation       |
|   [8]   | `GetStream`           | data reader call | streams blob column       |
|   [9]   | `Handle`              | connection read  | exposes raw handle        |

## [4]-[IMPLEMENTATION_LAW]

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
