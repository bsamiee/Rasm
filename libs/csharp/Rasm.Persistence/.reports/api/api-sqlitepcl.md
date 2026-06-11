# [RASM_PERSISTENCE_API_SQLITEPCL]

`SQLitePCLRaw.bundle_e_sqlite3` supplies bundled native SQLite initialization and provider identity.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3`
- package: `SQLitePCLRaw.bundle_e_sqlite3`
- assembly: native/bootstrap assets
- namespace: `SQLitePCL`
- asset: native assets
- rail: native-store

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native SQLite family
- rail: native-store

| [INDEX] | [ASSET]                         | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :----------------- | :----------------------- |
|   [1]   | `SQLitePCLRaw.bundle_e_sqlite3` | bundle package     | admits SQLite bundle     |
|   [2]   | `SQLitePCLRaw.config.e_sqlite3` | provider config    | selects bundled provider |
|   [3]   | `SourceGear.sqlite3`            | native asset owner | supplies native library  |
|   [4]   | `e_sqlite3` native library      | native library     | loads bundled SQLite     |
|   [5]   | runtime native assets           | runtime assets     | declares native payload  |
|   [6]   | build-transitive targets        | build assets       | declares build input     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: native operations
- rail: native-store

| [INDEX] | [SURFACE]             | [CALL_SHAPE]        | [CAPABILITY]                |
| :-----: | :-------------------- | :------------------ | :-------------------------- |
|   [1]   | `Batteries_V2.Init`   | bootstrap call      | initializes native provider |
|   [2]   | `SQLitePCL.raw`       | transitive contract | anchors raw provider API    |
|   [3]   | provider registration | provider selection  | selects SQLite provider     |
|   [4]   | native library load   | native load         | loads bundled SQLite        |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3`
- Owns: SQLite native provider
- Accept: store open records native identity
- Reject: ambient system SQLite
