# [RASM_PERSISTENCE_API_SQLITEPCL]

`SQLitePCLRaw.bundle_e_sqlite3` admits the bundled SQLite native provider used
by the embedded SQLite store profile.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3`
- package: `SQLitePCLRaw.bundle_e_sqlite3`
- assembly: package admission asset
- namespace: `SQLitePCL`
- asset: native provider bundle
- rail: store-provider

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: bundle assets
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :-------------------------------- | :------------------ | :----------------------- |
|  [01]   | `SQLitePCLRaw.bundle_e_sqlite3`   | bundle package      | admits native provider   |
|  [02]   | `SQLitePCLRaw.core`               | core dependency     | exposes raw SQLite API   |
|  [03]   | `SQLitePCLRaw.config.e_sqlite3`   | config dependency   | carries batteries asset  |
|  [04]   | `SQLitePCLRaw.provider.e_sqlite3` | provider dependency | supplies native provider |
|  [05]   | `SourceGear.sqlite3`              | native dependency   | supplies SQLite library  |
|  [06]   | `Batteries_V2`                    | initializer         | initializes provider     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider initialization
- rail: store-provider

| [INDEX] | [SURFACE]        | [CALL_SHAPE]     | [CAPABILITY]          |
| :-----: | :--------------- | :--------------- | :-------------------- |
|  [01]   | `Batteries.Init` | initializer call | initializes SQLitePCL |
|  [02]   | `Batteries_V2`   | initializer type | binds provider bundle |
|  [03]   | native library   | runtime asset    | executes SQLite calls |
|  [04]   | provider factory | runtime asset    | connects raw provider |

[ENTRYPOINT_SCOPE]: raw interop surface (`SQLitePCLRaw.core`, namespace `SQLitePCL`)
- rail: store-provider

| [INDEX] | [SURFACE]                                                                     | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :-------------------- |
|  [01]   | `raw.sqlite3_backup_init` / `_step` / `_remaining` / `_pagecount` / `_finish` | static raw call | pages live backup     |
|  [02]   | `raw.sqlite3_snapshot_get` / `_open` / `_cmp` / `_recover` / `_free`          | static raw call | pins consistent view  |
|  [03]   | `raw.sqlite3_db_config`                                                       | static raw call | configures connection |
|  [04]   | `raw.sqlite3_wal_checkpoint_v2`                                               | static raw call | checkpoints WAL log   |
|  [05]   | `raw.sqlite3_extended_result_codes`                                           | static raw call | arms extended codes   |
|  [06]   | `raw.SQLITE_OK` / `raw.SQLITE_DONE`                                           | status constant | reports raw status    |
|  [07]   | `sqlite3` / `sqlite3_backup` / `sqlite3_snapshot`                             | handle type     | owns native handle    |

[RAW_SIGNATURES]: verified `SQLitePCL.raw` member signatures (e_sqlite3 3.0.3)
- rail: store-provider

| [INDEX] | [SURFACE]                       | [CAPABILITY]               |
| :-----: | :------------------------------ | :------------------------- |
|  [01]   | `sqlite3_db_config`             | per-connection config flag |
|  [02]   | `sqlite3_wal_checkpoint_v2`     | WAL checkpoint with frames |
|  [03]   | `sqlite3_extended_result_codes` | arms extended result codes |
|  [04]   | `sqlite3_snapshot_get`          | pins read-view snapshot    |
|  [05]   | `sqlite3_snapshot_open`         | opens pinned snapshot      |
|  [06]   | `sqlite3_snapshot_cmp`          | orders snapshots           |
|  [07]   | snapshot recover/free           | recovers / frees snapshot  |
|  [08]   | backup API                      | pages live backup          |

```csharp generated
int sqlite3_db_config(sqlite3 db, int op, int val, out int result)
int sqlite3_wal_checkpoint_v2(sqlite3 db, string dbName, int eMode, out int logSize, out int framesCheckPointed)
int sqlite3_extended_result_codes(sqlite3 db, int onoff)
int sqlite3_snapshot_get(sqlite3 db, string schema, out sqlite3_snapshot snap)
int sqlite3_snapshot_open(sqlite3 db, string schema, sqlite3_snapshot snap)
int sqlite3_snapshot_cmp(sqlite3_snapshot p1, sqlite3_snapshot p2)
int sqlite3_snapshot_recover(sqlite3 db, string name)
void sqlite3_snapshot_free(sqlite3_snapshot snap)
sqlite3_backup sqlite3_backup_init(sqlite3 destDb, string destName, sqlite3 sourceDb, string sourceName)
int sqlite3_backup_step(sqlite3_backup backup, int nPage)
int sqlite3_backup_remaining(sqlite3_backup backup)
int sqlite3_backup_pagecount(sqlite3_backup backup)
int sqlite3_backup_finish(sqlite3_backup backup)
```

[RAW_CONSTANTS]: verified `SQLitePCL.raw` integer constants (e_sqlite3 3.0.3)
- rail: store-provider

| [INDEX] | [CONSTANT]                              | [VALUE] | [CAPABILITY]                |
| :-----: | :-------------------------------------- | ------: | :-------------------------- |
|  [01]   | `SQLITE_DBCONFIG_DEFENSIVE`             |    1010 | defensive-mode hardening    |
|  [02]   | `SQLITE_DBCONFIG_DQS_DML`               |    1013 | double-quoted DML rejection |
|  [03]   | `SQLITE_DBCONFIG_DQS_DDL`               |    1014 | double-quoted DDL rejection |
|  [04]   | `SQLITE_DBCONFIG_ENABLE_TRIGGER`        |    1003 | trigger enablement          |
|  [05]   | `SQLITE_DBCONFIG_ENABLE_VIEW`           |    1015 | view enablement             |
|  [06]   | `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` |    1005 | db_config extension arming  |
|  [07]   | `SQLITE_OK`                             |       0 | status code                 |
|  [08]   | `SQLITE_DONE`                           |     101 | status code                 |
|  [09]   | `SQLITE_BUSY`                           |       5 | extended status code        |
|  [10]   | `SQLITE_CORRUPT`                        |      11 | extended status code        |
|  [11]   | `SQLITE_NOTADB`                         |      26 | extended status code        |
|  [12]   | `SQLITE_CHECKPOINT_PASSIVE`             |       0 | checkpoint mode             |
|  [13]   | `SQLITE_CHECKPOINT_FULL`                |       1 | checkpoint mode             |
|  [14]   | `SQLITE_CHECKPOINT_RESTART`             |       2 | checkpoint mode             |
|  [15]   | `SQLITE_CHECKPOINT_TRUNCATE`            |       3 | checkpoint mode             |

## [04]-[IMPLEMENTATION_LAW]

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
