# [PY_DATA_API_ADBC_DRIVER_SQLITE]

`adbc-driver-sqlite` binds the native ADBC SQLite driver to the data partition rail: `connect` maps a SQLite URI (`:memory:`, a filesystem path, or a `file:` URI) onto an `AdbcDatabase`, and `dbapi.connect` derives an `AdbcSqliteConnection` over the shared driver-manager DBAPI core. `ConnectionOptions` keys `adbc.sqlite.load_extension.*` and `StatementOptions` carries `BATCH_ROWS` as canonical wire keys; `AdbcSqliteConnection` adds the sqlite-distinctive `enable_load_extension`/`load_extension` pair, while every query, ingest, and metadata concern rides the driver-manager rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-sqlite`
- package: `adbc-driver-sqlite` (Apache-2.0)
- module: `adbc_driver_sqlite`, `adbc_driver_sqlite.dbapi`
- driver: native `libadbc_driver_sqlite.so`, bound by `connect` to a SQLite URI
- owner: `data`
- rail: partition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory, option vocabularies, and the sqlite connection subtype

`connect` is the sole low-level factory; `ConnectionOptions`/`StatementOptions` are `enum.Enum` keys whose values are the driver's `adbc.sqlite.*` setting strings; `AdbcSqliteConnection` is the `dbapi.Connection` subclass adding the loadable-extension pair. Database identity is the URI — no `DatabaseOptions` vocabulary and no transport axis.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `connect`                    | factory            | bind a SQLite URI to an `AdbcDatabase`                |
|  [02]   | `ConnectionOptions`          | option enum        | connection-scoped `adbc.sqlite.load_extension.*` keys |
|  [03]   | `StatementOptions`           | option enum        | statement-scoped result-batch row hint                |
|  [04]   | `dbapi.AdbcSqliteConnection` | connection subtype | `dbapi.Connection` plus the loadable-extension pair   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `connect(uri=None) -> AdbcDatabase`                         | factory | low-level SQLite database handle                    |
|  [02]   | `dbapi.connect(uri=None, **kwargs) -> AdbcSqliteConnection` | factory | DBAPI connection; `**kwargs` forward to the manager |

- `None`/`:memory:` opens a private in-memory database; a path or `file:` URI opens a local file.

[ENTRYPOINT_SCOPE]: `ConnectionOptions` keys — connection-scoped, gate and drive loadable-extension registration

| [INDEX] | [MEMBER]                    | [VALUE]                                 | [CAPABILITY]                                    |
| :-----: | :-------------------------- | :-------------------------------------- | :---------------------------------------------- |
|  [01]   | `LOAD_EXTENSION_ENABLED`    | `adbc.sqlite.load_extension.enabled`    | arm loadable-extension registration             |
|  [02]   | `LOAD_EXTENSION_ENTRYPOINT` | `adbc.sqlite.load_extension.entrypoint` | override the extension C init entrypoint symbol |
|  [03]   | `LOAD_EXTENSION_PATH`       | `adbc.sqlite.load_extension.path`       | filesystem path of the extension to load        |

[ENTRYPOINT_SCOPE]: `StatementOptions` keys — statement-scoped, bound each assembled Arrow batch

| [INDEX] | [MEMBER]     | [VALUE]                        | [CAPABILITY]                            |
| :-----: | :----------- | :----------------------------- | :-------------------------------------- |
|  [01]   | `BATCH_ROWS` | `adbc.sqlite.query.batch_rows` | target row count per Arrow result batch |

[ENTRYPOINT_SCOPE]: loadable-extension pair on `AdbcSqliteConnection`

`enable_load_extension` sets `LOAD_EXTENSION_ENABLED`; `load_extension` then sets `LOAD_EXTENSION_PATH` and optional `LOAD_EXTENSION_ENTRYPOINT`. Provisioning admits only trusted, handle-relative paths beneath an admitted root.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `enable_load_extension(enabled) -> None`           | instance | arm or disarm extension registration         |
|  [02]   | `load_extension(path, *, entrypoint=None) -> None` | instance | register a loadable extension by path/symbol |

[ENTRYPOINT_SCOPE]: bulk ingest via the manager `Cursor` — the arm's load-bearing egress, not a parallel surface
- call: `adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False) -> int`

| [INDEX] | [SURFACE]            | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------- | :------- | :---------------------------------------- |
|  [01]   | `Cursor.adbc_ingest` | instance | stream an Arrow table into a SQLite table |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `connect` binds `libadbc_driver_sqlite.so` to a SQLite URI; `dbapi.connect` derives `AdbcSqliteConnection` from the shared database object, never a parallel client class. A `None`/`:memory:` URI is in-memory; a path or `file:` URI is a local file.
- `ConnectionOptions`/`StatementOptions` enum values are the canonical `adbc.sqlite.*` keys; settings flow as `conn_kwargs`/statement dictionaries keyed by enum value. Database identity lives in the URI, so no `DatabaseOptions` type or database-option table exists.
- `enable_load_extension` then `load_extension` registers a native SQLite loadable extension by setting `LOAD_EXTENSION_ENABLED`, `LOAD_EXTENSION_PATH`, and optional `LOAD_EXTENSION_ENTRYPOINT`; extension policy admits only trusted, handle-relative paths beneath each handle's root and gates the entrypoint before the load.
- `Cursor.adbc_ingest` streams a `pyarrow.Table`/`RecordBatch`/`RecordBatchReader` (any `__arrow_c_stream__` producer) into a table keyed by `mode` (`append`/`create`/`create_append`/`replace`), returning the row count.
- `StatementOptions.BATCH_ROWS` bounds each assembled Arrow batch; every result `RecordBatchReader` exposes `__arrow_c_stream__` for zero-copy handoff.
- each connection captures the resolved URI, extension-load arming and loaded paths, ingest mode and row count, and Arrow schema as a partition receipt.

[STACKING]:
- `adbc-driver-manager`(`adbc-driver-manager.md`): loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree), transaction control, metadata (`adbc_get_objects`/`adbc_get_table_schema`/`adbc_get_statistics`), `Cursor.adbc_ingest`, and `AdbcStatusCode` mapping are the manager's; this catalog adds only the sqlite option vocabulary and the loadable-extension pair.
- `arro3-core`(`arro3-core.md`), `polars`(`polars.md`): a result `RecordBatchReader.__arrow_c_stream__` feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` with zero copy.
- `adbc-driver-flightsql`(`adbc-driver-flightsql.md`), `adbc-driver-postgresql`(`adbc-driver-postgresql.md`): the local-file arm complements the remote arms on one shared manager surface, so cross-store federation swaps the driver, never the `dbapi` call shape.
- data partition owner: an in-memory or on-disk SQLite database is queried, ingested, and introspected through the identical `dbapi` API, emitting Arrow record batches to the partition rail.

[LOCAL_ADMISSION]:
- import `adbc_driver_sqlite` (and `.dbapi`) at boundary scope only.
- bind through `connect`/`dbapi.connect`; carry option keys from `ConnectionOptions`/`StatementOptions` by enum value.
- register extensions through `enable_load_extension`/`load_extension`, provisioned from an admitted root.
- bulk-load through `Cursor.adbc_ingest` with an explicit `mode`.

[RAIL_LAW]:
- Package: `adbc-driver-sqlite`
- Owns: in-memory and local-file SQLite binding, loadable-extension registration, Arrow-native bulk ingest, and per-statement fetch-batch sizing
- Accept: local-file federation feeding Arrow record batches to the partition, query, and dataframe owners; Arrow-table bulk load; native SQLite extension registration
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a raw `SELECT load_extension`/`PRAGMA` where `enable_load_extension`/`load_extension` apply; a per-row insert loop where `adbc_ingest` streams the whole Arrow table; a hand-written `sqlite_master` query where the manager's `adbc_get_objects` returns Arrow; a `DatabaseOptions`-style table where the URI carries identity; string-literal option keys bypassing `ConnectionOptions`/`StatementOptions`; a `sqlite3`-module DBAPI shim the manager supersedes
