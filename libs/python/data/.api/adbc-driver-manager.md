# [PY_DATA_API_ADBC_DRIVER_MANAGER]

`adbc_driver_manager` supplies the ADBC driver-loading core plus a PEP 249 DBAPI front end for the data query rail: `dbapi.connect`, `Connection`, and `Cursor` own the high-level surface returning Arrow tables and record-batch readers, while `AdbcDatabase`, `AdbcConnection`, and `AdbcStatement` own the low-level handle layer over the ADBC C API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc_driver_manager`
- package: `adbc-driver-manager`
- owner: `data`
- version: `1.11.0`
- module: `adbc_driver_manager`
- license: `Apache-2.0`
- rail: query

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: DBAPI surface family
- rail: query — `adbc_driver_manager.dbapi`; `apilevel='2.0'`, `paramstyle='qmark'`, `threadsafety=1`
- type family: PEP 249 ABC

| [INDEX] | [SYMBOL]     | [ROLE]                                               |
| :-----: | :----------- | :--------------------------------------------------- |
|  [01]   | `Connection` | autocommit/transaction control plus ADBC metadata    |
|  [02]   | `Cursor`     | execute, fetch tuples, fetch Arrow tables and frames |

[PUBLIC_TYPE_SCOPE]: low-level handle family
- rail: query — `adbc_driver_manager._lib`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [ROLE]                                      |
| :-----: | :----------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `AdbcDatabase`           | handle          | shared driver/database configuration handle |
|  [02]   | `AdbcConnection`         | handle          | connection handle over a database           |
|  [03]   | `AdbcStatement`          | handle          | query/statement execution handle            |
|  [04]   | `ArrowArrayHandle`       | capsule wrapper | exported Arrow array C-data pointer         |
|  [05]   | `ArrowArrayStreamHandle` | capsule wrapper | exported Arrow stream C-data pointer        |
|  [06]   | `ArrowSchemaHandle`      | capsule wrapper | exported Arrow schema C-data pointer        |

[PUBLIC_TYPE_SCOPE]: option and info enums
- rail: query
- `StatementOptions` keys: `INGEST_MODE`, `INGEST_TARGET_TABLE`, `INGEST_TARGET_CATALOG`, `INGEST_TARGET_DB_SCHEMA`, `INGEST_TEMPORARY`, `BIND_BY_NAME`, `INCREMENTAL`, `PROGRESS`
- `AdbcStatusCode` codes: `OK`, `UNKNOWN`, `NOT_IMPLEMENTED`, `NOT_FOUND`, `ALREADY_EXISTS`, `INVALID_ARGUMENT`, `INVALID_STATE`, `INVALID_DATA`, `INTEGRITY`, `INTERNAL`, `IO`, `CANCELLED`, `TIMEOUT`, `UNAUTHENTICATED`, `UNAUTHORIZED`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `DatabaseOptions`   | str enum      | `URI`, `USERNAME`, `PASSWORD`                             |
|  [02]   | `ConnectionOptions` | str enum      | `CURRENT_CATALOG`, `CURRENT_DB_SCHEMA`, `ISOLATION_LEVEL` |
|  [03]   | `StatementOptions`  | str enum      | ingest mode, target, and execution keys                   |
|  [04]   | `AdbcInfoCode`      | int enum      | vendor/driver name, version, Arrow version codes          |
|  [05]   | `AdbcStatusCode`    | int enum      | ADBC status codes                                         |
|  [06]   | `GetObjectsDepth`   | int enum      | `ALL`, `CATALOGS`, `DB_SCHEMAS`, `TABLES`, `COLUMNS`      |

[PUBLIC_TYPE_SCOPE]: error hierarchy
- rail: query — PEP 249 exception tree rooted at `Error`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                    |
| :-----: | :------------------ | :------------ | :---------------------------------------- |
|  [01]   | `Error`             | base          | root ADBC/DBAPI error with `status_code`  |
|  [02]   | `DatabaseError`     | error         | server-side error base                    |
|  [03]   | `OperationalError`  | error         | operational fault (connection, transient) |
|  [04]   | `ProgrammingError`  | error         | malformed query or API misuse             |
|  [05]   | `IntegrityError`    | error         | constraint violation                      |
|  [06]   | `NotSupportedError` | error         | unsupported feature or driver call        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DBAPI connection lifecycle
- rail: query — `adbc_driver_manager.dbapi`
- call: `connect(driver=None, uri=None, *, profile=None, entrypoint=None, db_kwargs=None, conn_kwargs=None, autocommit=False)`

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [ROLE]                                   |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `connect`                                     | open           | load a driver and open a `Connection`    |
|  [02]   | `Connection.cursor(*, adbc_stmt_kwargs=None)` | cursor         | create a `Cursor`                        |
|  [03]   | `Connection.commit()`                         | transaction    | commit the active transaction            |
|  [04]   | `Connection.rollback()`                       | transaction    | roll back the active transaction         |
|  [05]   | `Connection.close()`                          | lifecycle      | close the connection                     |
|  [06]   | `Connection.adbc_clone()`                     | lifecycle      | open a second connection on the database |

[ENTRYPOINT_SCOPE]: cursor execution and fetch
- rail: query — `adbc_driver_manager.dbapi.Cursor`
- call: `Cursor.adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False)`

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [ROLE]                                             |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Cursor.execute(operation, parameters=None)`                       | execute        | run one query with optional bind parameters        |
|  [02]   | `Cursor.executemany(operation, seq_of_parameters)`                 | execute        | run a query against a parameter batch              |
|  [03]   | `Cursor.fetchone()`                                                | fetch          | next row tuple or `None`                           |
|  [04]   | `Cursor.fetchmany(size=None)`                                      | fetch          | up to `size` row tuples                            |
|  [05]   | `Cursor.fetchall()`                                                | fetch          | all remaining row tuples                           |
|  [06]   | `Cursor.fetch_arrow_table()`                                       | fetch          | full result as `pyarrow.Table`                     |
|  [07]   | `Cursor.fetch_record_batch()`                                      | fetch          | streaming `pyarrow.RecordBatchReader`              |
|  [08]   | `Cursor.fetch_df()` / `Cursor.fetch_polars()`                      | fetch          | result as pandas or polars frame                   |
|  [09]   | `Cursor.adbc_ingest`                                               | bulk           | bulk-load Arrow data into a table                  |
|  [10]   | `Cursor.adbc_prepare(operation)`                                   | prepare        | pre-compile and return parameter schema            |
|  [11]   | `Cursor.adbc_execute_schema(operation, parameters=None)`           | plan           | result `pyarrow.Schema` without execution          |
|  [12]   | `Cursor.adbc_execute_partitions(operation, parameters=None)`       | partition      | return `(partitions, schema)` descriptors          |
|  [13]   | `Cursor.adbc_read_partition(partition)`                            | partition      | open one partition as a `RecordBatchReader`        |
|  [14]   | `Cursor.adbc_cancel()`                                             | control        | cancel the in-flight statement                     |
|  [15]   | `Cursor.adbc_statement` / `Cursor.rowcount` / `Cursor.description` | introspect     | low-level handle, affected rows, DBAPI description |

[ENTRYPOINT_SCOPE]: connection metadata
- rail: query — `adbc_driver_manager.dbapi.Connection`
- entry family: metadata
- call: `adbc_get_objects(*, depth='all', catalog_filter=None, db_schema_filter=None, table_name_filter=None, table_types_filter=None, column_name_filter=None)`
- call: `adbc_get_statistics(*, catalog_filter=None, db_schema_filter=None, table_name_filter=None, approximate=True)`

| [INDEX] | [SURFACE]                                                                          | [ROLE]                                |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `adbc_get_info()`                                                                  | driver/vendor info code mapping       |
|  [02]   | `adbc_get_objects`                                                                 | catalog/schema/table/column hierarchy |
|  [03]   | `adbc_get_table_schema(table_name, *, catalog_filter=None, db_schema_filter=None)` | `pyarrow.Schema` for one table        |
|  [04]   | `adbc_get_table_types()`                                                           | supported table type list             |
|  [05]   | `adbc_get_statistics`                                                              | table statistics reader               |

[ENTRYPOINT_SCOPE]: low-level handle operations
- rail: query — `adbc_driver_manager._lib`
- `AdbcConnection` setup: `AdbcConnection(database, **kwargs)`, `.set_autocommit(enabled)`, `.set_options(**kwargs)`, `.get_option(key)`
- `AdbcConnection` metadata: `.get_info()`, `.get_objects(...)`, `.get_table_schema(...)`, `.get_table_types()`, `.get_statistics(...)`, `.get_statistic_names()`

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [ROLE]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `AdbcDatabase(**kwargs)` / `AdbcDatabase.set_options(**kwargs)`    | configure      | open and set database driver options              |
|  [02]   | `AdbcConnection.commit()` / `.rollback()`                          | transaction    | commit or roll back on the handle                 |
|  [03]   | `AdbcStatement(connection)` / `.set_options(**kwargs)`             | prepare        | open a statement on a connection, set options     |
|  [04]   | `AdbcStatement.set_sql_query(query)` / `.set_substrait_plan(plan)` | prepare        | set SQL text or a Substrait plan                  |
|  [05]   | `AdbcStatement.prepare()` / `.get_parameter_schema()`              | prepare        | pre-compile; read the bind-parameter `Schema`     |
|  [06]   | `AdbcStatement.bind(data, schema=None)` / `.bind_stream(stream)`   | bind           | bind Arrow array data or a record-batch stream    |
|  [07]   | `AdbcStatement.execute_query()`                                    | execute        | run and return `(stream, rows_affected)`          |
|  [08]   | `AdbcStatement.execute_update()`                                   | execute        | run for affected-row count only                   |
|  [09]   | `AdbcStatement.execute_partitions()`                               | execute        | run into `(partitions, schema, rows_affected)`    |
|  [10]   | `AdbcStatement.execute_schema()` / `.cancel()`                     | execute        | result schema without execution; cancel in-flight |

## [04]-[IMPLEMENTATION_LAW]

[ADBC_TOPOLOGY]:
- two layers: the DBAPI front end (`dbapi.connect` -> `Connection` -> `Cursor`) wraps the handle layer (`AdbcDatabase` -> `AdbcConnection` -> `AdbcStatement`); the manager is driver-agnostic and is the shared core every concrete driver (e.g. `adbc_driver_flightsql`, `adbc_driver_postgresql`, `adbc_driver_sqlite`) loads through
- `connect` loads a shared driver library named by `driver` (or a registered `profile`), with `entrypoint` selecting the C init symbol; a concrete driver's own `connect` is the same call with the library path pre-bound
- result delivery is Arrow-native: `fetch_arrow_table`, `fetch_record_batch`, and zero-copy `ArrowArrayStreamHandle` export over the C Data Interface; every Arrow result implements `__arrow_c_stream__`/`__arrow_c_array__`
- ingest modes via `StatementOptions`/`adbc_ingest`: `append`, `create`, `replace`, `create_append` against a target table/catalog/schema
- the exception hierarchy follows PEP 249 with an ADBC `status_code` (`AdbcStatusCode`) attached to every `Error`; the boundary maps `INVALID_STATE`/`CANCELLED`/`TIMEOUT`/`UNAUTHENTICATED` to typed retry-vs-fail decisions rather than catching bare `Error`

[INTEGRATION_RAILS]:
- ADBC -> arro3: `Cursor.fetch_record_batch()` returns a `pyarrow.RecordBatchReader` whose `__arrow_c_stream__` feeds `arro3.core.RecordBatchReader.from_stream` with zero copy; `Cursor.fetch_arrow_table()` round-trips through `arro3.core.Table.from_arrow`.
- ADBC -> polars: `Cursor.fetch_polars()` is the native escape; equivalently `polars.from_arrow(cursor.fetch_record_batch())` consumes the capsule when a lazy frame is wanted.
- partition spine: `adbc_execute_partitions` returns opaque partition descriptors, each opened by `adbc_read_partition` as an independent `RecordBatchReader`, so a distributed driver (Flight SQL) fans result endpoints across workers; the manager owns the handoff, never a hand-stitched fetch loop.
- retry rail: wrap `execute`/`adbc_ingest` so an `OperationalError`/`status_code in {TIMEOUT, IO}` retries under the runtime backoff while `IntegrityError`/`ProgrammingError` fail fast; `Connection`/`Cursor` are context managers and close handles deterministically on exit.

[LOCAL_ADMISSION]:
- Open connections through `dbapi.connect` with `db_kwargs`/`conn_kwargs` carrying driver-specific option keys from `DatabaseOptions`/`ConnectionOptions`.
- Use `Connection` and `Cursor` as context managers so handles close deterministically.
- Pull results as Arrow tables or record-batch readers feeding arro3/polars; drop to tuple `fetchall` only for small scalar results.
- Bulk-load through `Cursor.adbc_ingest` with an explicit `mode`; never emit per-row `INSERT` loops.
- Reserve the `_lib` handle layer for driver authors, partition reads, and zero-copy stream export; ordinary query work stays on the DBAPI surface.

[RAIL_LAW]:
- Package: `adbc_driver_manager`
- Owns: ADBC driver loading, PEP 249 connectivity, Arrow-native query results, bulk ingest, partition reads, and database metadata introspection
- Accept: driver libraries via `connect`; bind and ingest data as Arrow; results as Arrow tables or streams feeding arro3/polars
- Reject: hand-rolled DBAPI shims; per-row insert loops where `adbc_ingest` applies; hand-stitched partition fetch loops where `adbc_read_partition` applies; wrapper-renames of `connect`/`Cursor`
