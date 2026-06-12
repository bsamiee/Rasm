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
|   [4]   | `raw.SQLITE_OK` / `raw.SQLITE_DONE`                                           | status constant | reports raw status    |
|   [5]   | `sqlite3` / `sqlite3_backup` / `sqlite3_snapshot`                             | handle type     | owns native handle    |

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
