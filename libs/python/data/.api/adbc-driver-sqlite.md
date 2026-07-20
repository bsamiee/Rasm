# [PY_DATA_API_ADBC_DRIVER_SQLITE]

`adbc-driver-sqlite` supplies the native ADBC SQLite driver for the data partition rail: a `connect` factory binding the native `libadbc_driver_sqlite.so` to a SQLite URI as an `AdbcDatabase`, one connection-scoped `enum.Enum` option vocabulary (`ConnectionOptions`) keying the driver's `adbc.sqlite.load_extension.*` settings, one statement-scoped vocabulary (`StatementOptions`) carrying the `BATCH_ROWS` fetch hint, and a driver-specific `AdbcSqliteConnection` whose `enable_load_extension`/`load_extension` pair is the sqlite-distinctive capability — a loadable-extension arm no other ADBC driver exposes. Consumption rides `dbapi.connect` on the local-file federation path: a `:memory:` URI opens an in-memory database, a file path or `file:` URI opens a local file, and `Cursor.adbc_ingest` streams an Arrow table into it. This driver is the local counterpart to the remote `adbc-driver-flightsql`/`adbc-driver-postgresql` arms — never a re-implemented `sqlite3` DBAPI shim, a hand-stitched extension-loading `PRAGMA`, or a per-setting builder type where an option enum string already keys the value.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-sqlite`
- package: `adbc-driver-sqlite`
- import: `adbc_driver_sqlite`
- owner: `data`
- rail: partition
- license: `Apache-2.0`
- entry points: library use is import-only; `connect` returns an `AdbcDatabase`, `dbapi.connect` returns an `AdbcSqliteConnection` (a DBAPI `Connection`)
- capability: SQLite URI binding for in-memory and local-file databases, loadable-extension support through `enable_load_extension`/`load_extension`, Arrow-native bulk ingest via `Cursor.adbc_ingest` with create/append/replace modes, catalog and statistics introspection through the manager's `adbc_get_objects`/`adbc_get_table_schema`/`adbc_get_statistics`, per-statement fetch-batch sizing, and DBAPI cursor access yielding Arrow record batches

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory, option vocabularies, and the sqlite connection subtype
- rail: partition

`connect` is the single low-level factory; `ConnectionOptions` and `StatementOptions` are `enum.Enum` option keys whose values are the driver's `adbc.sqlite.*` setting strings. `AdbcSqliteConnection` is the concrete `dbapi.Connection` subclass `dbapi.connect` returns — it adds the loadable-extension pair over the manager's shared DBAPI connection. Like the postgres driver and unlike Flight SQL, the sqlite driver exposes no `DatabaseOptions` vocabulary and no OAuth axes — database identity is the URI (`:memory:`, a filesystem path, or a `file:` URI), and there is no network transport to secure. `dbapi` carries the DBAPI facade — `Connection`, `Cursor`, the typed error hierarchy — shared with every ADBC driver through the manager.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [RAIL]                                                |
| :-----: | :--------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `connect`                    | factory            | bind a SQLite URI to an `AdbcDatabase`                |
|  [02]   | `ConnectionOptions`          | option enum        | connection-scoped `adbc.sqlite.load_extension.*` keys |
|  [03]   | `StatementOptions`           | option enum        | statement-scoped key — result-batch row hint          |
|  [04]   | `dbapi.AdbcSqliteConnection` | connection subtype | `dbapi.Connection` plus the loadable-extension pair   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories
- rail: partition

`connect` mints the low-level `AdbcDatabase` bound to the native driver path with the SQLite URI; `dbapi.connect` wraps that in an `AdbcSqliteConnection`, forwarding `**kwargs` to the manager's `dbapi.connect` for `db_kwargs`/`conn_kwargs` and the `autocommit` toggle. This driver has no `DatabaseOptions` enum — a URI carries database identity, and a `None` URI opens a private in-memory database.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                          | [CAPABILITY]                     |
| :-----: | :-------------- | :---------------------------------------------------- | :------------------------------- |
|  [01]   | `connect`       | `connect(uri=None) -> AdbcDatabase`                   | low-level SQLite database handle |
|  [02]   | `dbapi.connect` | `connect(uri=None, **kwargs) -> AdbcSqliteConnection` | DBAPI connection over SQLite |

[ENTRYPOINT_SCOPE]: `ConnectionOptions` keys
- rail: partition
- Sqlite-specific connection settings carry the `ConnectionOptions.` prefix and apply at connection scope. Together they gate and drive loadable-extension registration; `enable` must be set before a `path`/`entrypoint` load is applied.

| [INDEX] | [MEMBER]                    | [VALUE]                                 | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :-------------------------------------- | :---------------------------------------------------- |
|  [01]   | `LOAD_EXTENSION_ENABLED`    | `adbc.sqlite.load_extension.enabled`    | arm loadable-extension registration on the connection |
|  [02]   | `LOAD_EXTENSION_ENTRYPOINT` | `adbc.sqlite.load_extension.entrypoint` | override the extension's C init entrypoint symbol     |
|  [03]   | `LOAD_EXTENSION_PATH`       | `adbc.sqlite.load_extension.path`       | filesystem path of the extension to load              |

[ENTRYPOINT_SCOPE]: `StatementOptions` keys
- rail: partition
- Sqlite-specific statement settings carry the `StatementOptions.` prefix and bound each Arrow batch the driver assembles per fetch.

| [INDEX] | [MEMBER]     | [VALUE]                        | [CAPABILITY]                            |
| :-----: | :----------- | :----------------------------- | :-------------------------------------- |
|  [01]   | `BATCH_ROWS` | `adbc.sqlite.query.batch_rows` | target row count per Arrow result batch |

[ENTRYPOINT_SCOPE]: loadable-extension pair (on `AdbcSqliteConnection`)
- rail: partition
- Loadable-extension registration is the sqlite-distinctive surface — `enable_load_extension` sets `ConnectionOptions.LOAD_EXTENSION_ENABLED`, and `load_extension` sets `LOAD_EXTENSION_PATH` (with an optional `LOAD_EXTENSION_ENTRYPOINT`) to register a SQLite loadable extension. Extension policy admits only trusted, handle-relative paths beneath an admitted root and refuses caller-controlled paths.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                       | [CAPABILITY]                                 |
| :-----: | :---------------------- | :------------------------------------------------- | :------------------------------------------- |
|  [01]   | `enable_load_extension` | `enable_load_extension(enabled: bool) -> None`     | arm or disarm extension registration         |
|  [02]   | `load_extension`        | `load_extension(path, *, entrypoint=None) -> None` | register a loadable extension by path/symbol |

[ENTRYPOINT_SCOPE]: bulk ingest surface (via the manager `Cursor`)
- rail: partition
- call: `Cursor.adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False) -> int`
- `adbc_ingest` lives on the manager's DBAPI `Cursor`; the sqlite driver backs it with a native table load. Listed because it is the sqlite arm's load-bearing egress path, not a parallel surface — the query, metadata, and partition surface is the manager's.

| [INDEX] | [SURFACE]            | [CAPABILITY]                              |
| :-----: | :------------------- | :---------------------------------------- |
|  [01]   | `Cursor.adbc_ingest` | stream an Arrow table into a SQLite table |

## [04]-[IMPLEMENTATION_LAW]

[PARTITION_SQLITE]:
- import: `import adbc_driver_sqlite` (and `adbc_driver_sqlite.dbapi`) at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `connect` owns binding to the native `libadbc_driver_sqlite.so` with the SQLite URI; `dbapi.connect` is the DBAPI row returning `AdbcSqliteConnection` and forwarding `**kwargs` to the manager, never a parallel client class — the database object is shared and connections are derived from it. A `None`/`:memory:` URI opens an in-memory database; a filesystem path or `file:` URI opens a local file.
- option axis: `ConnectionOptions`/`StatementOptions` enum values are the canonical `adbc.sqlite.*` keys; settings flow as `conn_kwargs`/statement-option dictionaries keyed by enum value, never as ad hoc string literals. This driver exposes no `DatabaseOptions` — database identity lives in the URI, so no per-setting builder type or database-option table is minted.
- extension axis: `AdbcSqliteConnection.enable_load_extension` then `load_extension` is the sqlite-distinctive capability — it registers a native SQLite loadable extension by setting `LOAD_EXTENSION_ENABLED`, `LOAD_EXTENSION_PATH`, and optionally `LOAD_EXTENSION_ENTRYPOINT`. A hand-stitched `SELECT load_extension(...)` or a raw `PRAGMA` where these methods apply is rejected; extension provisioning is the driver's, never a shell-out.
- extension policy: policy admits only trusted, handle-relative paths beneath each extension handle's root, gates the entrypoint before `load_extension`, and refuses caller-controlled paths.
- ingest axis: `Cursor.adbc_ingest` streams a `pyarrow.Table`/`RecordBatch`/`RecordBatchReader` (or any `__arrow_c_stream__` producer) into a table, keyed by `mode` (`append`/`create`/`create_append`/`replace`), returning the row count. A per-row prepare-bind-insert loop where `adbc_ingest` streams the whole table is rejected.
- fetch axis: `StatementOptions.BATCH_ROWS` bounds the row count per assembled Arrow batch; every result `RecordBatchReader` exposes `__arrow_c_stream__`, so a sqlite result feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` with zero copy.
- manager axis: the concrete driver delegates loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree), transaction control, metadata introspection, and Arrow result delivery to `adbc_driver_manager`; this catalog adds only the sqlite option vocabulary and the loadable-extension law — never a parallel DBAPI implementation. Typed error tree and `AdbcStatusCode` mapping stay the manager's.
- federation axis: the local-file arm complements the remote `adbc-driver-flightsql`/`adbc-driver-postgresql` arms on one shared manager surface — an in-memory or on-disk SQLite database is queried, ingested, and introspected through the identical `dbapi` API, so cross-store federation swaps the driver, never the call shape.
- telemetry axis: the Go driver embeds its own OTel tracer configured through the standard `OTEL_TRACES_EXPORTER`/`OTEL_EXPORTER_OTLP_*`/`OTEL_SERVICE_NAME` env family; no Python-side instrumentor covers this driver.
- evidence: each connection captures the resolved URI, whether extension loading was armed and each loaded extension path, ingest mode and row count, and Arrow schema as a partition receipt.
- boundary: the driver owns SQLite binding, extension registration, Arrow ingest, and option application, emitting Arrow record batches consumed by the data partition owner; result-set materialization and dataframe conversion route to `pyarrow`/`polars`.

[RAIL_LAW]:
- Package: `adbc-driver-sqlite`
- Owns: in-memory and local-file SQLite binding, loadable-extension registration, Arrow-native bulk ingest, and per-statement fetch-batch sizing
- Accept: local-file federation feeding Arrow record batches to the data partition, query, and dataframe owners; Arrow-table bulk load into a SQLite database; native SQLite extension registration
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a raw `SELECT load_extension`/`PRAGMA` where `enable_load_extension`/`load_extension` apply; a per-row insert loop where `adbc_ingest` streams the whole Arrow table; a hand-written `sqlite_master` query where the manager's `adbc_get_objects` returns Arrow; a `DatabaseOptions`-style table where the URI carries identity; string-literal option keys bypassing `ConnectionOptions`/`StatementOptions`; a `sqlite3`-module DBAPI shim the manager already supersedes
