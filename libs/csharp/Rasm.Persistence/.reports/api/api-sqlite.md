# [RASM_PERSISTENCE_API_SQLITE]

`Microsoft.Data.Sqlite` supplies raw SQLite connections, commands, transactions, readers, parameters, and backup surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite`
- package: `Microsoft.Data.Sqlite`
- assembly: `Microsoft.Data.Sqlite`
- namespace: `Microsoft.Data.Sqlite`
- asset: runtime library
- rail: native-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SQLite family
- rail: native-store

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]               |
| :-----: | :------------------------------ | :----------------- | :------------------------- |
|   [1]   | `SqliteConnection`              | transport handle   | owns transport state       |
|   [2]   | `SqliteCommand`                 | command surface    | executes user intent       |
|   [3]   | `SqliteTransaction`             | transaction handle | bounds atomic work         |
|   [4]   | `SqliteDataReader`              | row reader         | executes store operation   |
|   [5]   | `SqliteParameter`               | SQL parameter      | executes store operation   |
|   [6]   | `SqliteConnectionStringBuilder` | builder surface    | constructs configured root |
|   [7]   | `SqliteException`               | provider exception | executes store operation   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SQLite operations
- rail: native-store

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]              |
| :-----: | :----------------- | :------------------ | :------------------------ |
|   [1]   | `Open`             | operation call      | executes operation        |
|   [2]   | `OpenAsync`        | async operation     | executes async work       |
|   [3]   | `CreateCommand`    | factory call        | creates configured handle |
|   [4]   | `BeginTransaction` | transaction factory | starts transaction        |
|   [5]   | `ExecuteReader`    | operation call      | executes operation        |
|   [6]   | `ExecuteNonQuery`  | operation call      | executes operation        |
|   [7]   | `BackupDatabase`   | backup method       | copies database           |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite`
- Owns: raw SQLite control
- Accept: native gates run at store open
- Reject: raw SQL throughout domain logic

