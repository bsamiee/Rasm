# [RASM_PERSISTENCE_API_SQLITEPCL]

`SQLitePCLRaw.bundle_e_sqlite3` admits the bundled SQLite native provider used
by the embedded SQLite store profile.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3`
- package: `SQLitePCLRaw.bundle_e_sqlite3`
- assembly: package admission asset
- namespace: `SQLitePCL`
- asset: native provider bundle
- rail: store-provider

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: bundle assets
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :-------------------------------- | :------------------ | :----------------------- |
|   [1]   | `SQLitePCLRaw.bundle_e_sqlite3`   | bundle package      | admits native provider   |
|   [2]   | `SQLitePCLRaw.core`               | core dependency     | exposes raw SQLite API   |
|   [3]   | `SQLitePCLRaw.config.e_sqlite3`   | config dependency   | carries batteries asset  |
|   [4]   | `SQLitePCLRaw.provider.e_sqlite3` | provider dependency | supplies native provider |
|   [5]   | `SourceGear.sqlite3`              | native dependency   | supplies SQLite library  |
|   [6]   | `Batteries_V2`                    | initializer         | initializes provider     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider initialization
- rail: store-provider

| [INDEX] | [SURFACE]        | [CALL_SHAPE]     | [CAPABILITY]          |
| :-----: | :--------------- | :--------------- | :-------------------- |
|   [1]   | `Batteries.Init` | initializer call | initializes SQLitePCL |
|   [2]   | `Batteries_V2`   | initializer type | binds provider bundle |
|   [3]   | native library   | runtime asset    | executes SQLite calls |
|   [4]   | provider factory | runtime asset    | connects raw provider |

[ENTRYPOINT_SCOPE]: raw interop surface (`SQLitePCLRaw.core`, namespace `SQLitePCL`)
- rail: store-provider

| [INDEX] | [SURFACE]                                                                     | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :-------------------- |
|   [1]   | `raw.sqlite3_backup_init` / `_step` / `_remaining` / `_pagecount` / `_finish` | static raw call | pages live backup     |
|   [2]   | `raw.sqlite3_snapshot_get` / `_open` / `_cmp` / `_recover` / `_free`          | static raw call | pins consistent view  |
|   [3]   | `raw.sqlite3_db_config`                                                       | static raw call | configures connection |
|   [4]   | `raw.sqlite3_wal_checkpoint_v2`                                               | static raw call | checkpoints WAL log   |
|   [5]   | `raw.sqlite3_extended_result_codes`                                           | static raw call | arms extended codes   |
|   [6]   | `raw.SQLITE_OK` / `raw.SQLITE_DONE`                                           | status constant | reports raw status    |
|   [7]   | `sqlite3` / `sqlite3_backup` / `sqlite3_snapshot`                             | handle type     | owns native handle    |

[RAW_SIGNATURES]: verified `SQLitePCL.raw` member signatures (e_sqlite3 3.0.3)
- rail: store-provider

| [INDEX] | [SIGNATURE]                                                                                        | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------------------- |
|   [1]   | `int sqlite3_db_config(sqlite3 db, int op, int val, out int result)`                               | per-connection config flag |
|   [2]   | `int sqlite3_wal_checkpoint_v2(sqlite3 db, string dbName, int eMode, out int logSize, out int framesCheckPointed)` | WAL checkpoint with frames  |
|   [3]   | `int sqlite3_extended_result_codes(sqlite3 db, int onoff)`                                         | arms extended result codes |
|   [4]   | `int sqlite3_snapshot_get(sqlite3 db, string schema, out sqlite3_snapshot snap)`                   | pins read-view snapshot    |
|   [5]   | `int sqlite3_snapshot_open(sqlite3 db, string schema, sqlite3_snapshot snap)`                      | opens pinned snapshot      |
|   [6]   | `int sqlite3_snapshot_cmp(sqlite3_snapshot p1, sqlite3_snapshot p2)`                               | orders snapshots           |
|   [7]   | `int sqlite3_snapshot_recover(sqlite3 db, string name)` / `void sqlite3_snapshot_free(sqlite3_snapshot snap)` | recovers / frees snapshot |
|   [8]   | `sqlite3_backup sqlite3_backup_init(sqlite3 destDb, string destName, sqlite3 sourceDb, string sourceName)`; `int _step(sqlite3_backup, int nPage)`; `int _remaining` / `_pagecount(sqlite3_backup)`; `int _finish(sqlite3_backup)` | pages live backup |

[RAW_CONSTANTS]: verified `SQLitePCL.raw` integer constants (e_sqlite3 3.0.3)
- rail: store-provider

| [INDEX] | [CONSTANT]                              | [VALUE] | [CAPABILITY]                |
| :-----: | :-------------------------------------- | :-----: | :-------------------------- |
|   [1]   | `SQLITE_DBCONFIG_DEFENSIVE`             |  1010   | defensive-mode hardening    |
|   [2]   | `SQLITE_DBCONFIG_DQS_DML`               |  1013   | double-quoted DML rejection |
|   [3]   | `SQLITE_DBCONFIG_DQS_DDL`               |  1014   | double-quoted DDL rejection |
|   [4]   | `SQLITE_DBCONFIG_ENABLE_TRIGGER`        |  1003   | trigger enablement          |
|   [5]   | `SQLITE_DBCONFIG_ENABLE_VIEW`           |  1015   | view enablement             |
|   [6]   | `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` |  1005   | db_config extension arming  |
|   [7]   | `SQLITE_OK` 0 / `SQLITE_DONE` 101 / `SQLITE_BUSY` 5 / `SQLITE_CORRUPT` 11 / `SQLITE_NOTADB` 26 | status / extended codes |
|   [8]   | `SQLITE_CHECKPOINT_PASSIVE` 0 / `_FULL` 1 / `_RESTART` 2 / `_TRUNCATE` 3 | checkpoint modes |

## [4]-[IMPLEMENTATION_LAW]

[NATIVE_ADMISSION]:
- package role: SQLite native provider admission
- runtime root: bundled e_sqlite3 provider
- initializer root: SQLitePCL batteries
- store root: embedded SQLite profile only

[LOCAL_ADMISSION]:
- Native SQLite provider setup belongs to the SQLite store profile.
- Initialization is explicit and cannot hide in unrelated startup code.
- Provider bundle facts stay in the store-profile rail and do not define public Persistence vocabulary.

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3`
- Owns: SQLite native provider admission
- Accept: embedded SQLite runtime
- Reject: SQLCipher bundle
