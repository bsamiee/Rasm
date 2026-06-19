# [PY_DATA_API_ADBC_DRIVER_MANAGER]

`adbc_driver_manager` supplies the ADBC driver-loading core plus a PEP 249 DBAPI front end for the data query rail: `dbapi.connect`, `Connection`, and `Cursor` own the high-level surface returning Arrow tables and record-batch readers, while `AdbcDatabase`, `AdbcConnection`, and `AdbcStatement` own the low-level handle layer over the ADBC C API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc_driver_manager`
- package: `adbc-driver-manager`
- module: `adbc_driver_manager`
- asset: C-extension runtime library (ADBC driver manager)
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

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `DatabaseOptions`   | str enum      | `URI`, `USERNAME`, `PASSWORD`                             |
|  [02]   | `ConnectionOptions` | str enum      | `CURRENT_CATALOG`, `CURRENT_DB_SCHEMA`, `ISOLATION_LEVEL` |
|  [03]   | `StatementOptions`  | str enum      | ingest mode/target, bind-by-name, incremental             |
|  [04]   | `AdbcInfoCode`      | int enum      | vendor/driver name, version, Arrow version codes          |
|  [05]   | `AdbcStatusCode`    | int enum      | `OK`, `NOT_FOUND`, `INVALID_STATE`, `CANCELLED`, ...      |
|  [06]   | `GetObjectsDepth`   | int enum      | `ALL`, `CATALOGS`, `DB_SCHEMAS`, `TABLES`                 |

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

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [ROLE]                                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `connect(driver=None, uri=None, *, profile=None, entrypoint=None, db_kwargs=None, conn_kwargs=None, autocommit=False)` | open           | load a driver and open a `Connection`    |
|  [02]   | `Connection.cursor(*, adbc_stmt_kwargs=None)`                                                                          | cursor         | create a `Cursor`                        |
|  [03]   | `Connection.commit()`                                                                                                  | transaction    | commit the active transaction            |
|  [04]   | `Connection.rollback()`                                                                                                | transaction    | roll back the active transaction         |
|  [05]   | `Connection.close()`                                                                                                   | lifecycle      | close the connection                     |
|  [06]   | `Connection.adbc_clone()`                                                                                              | lifecycle      | open a second connection on the database |

[ENTRYPOINT_SCOPE]: cursor execution and fetch
- rail: query — `adbc_driver_manager.dbapi.Cursor`

| [INDEX] | [SURFACE]                                                                                                         | [ENTRY_FAMILY] | [ROLE]                                      |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Cursor.execute(operation, parameters=None)`                                                                      | execute        | run one query with optional bind parameters |
|  [02]   | `Cursor.executemany(operation, seq_of_parameters)`                                                                | execute        | run a query against a parameter batch       |
|  [03]   | `Cursor.fetchone()`                                                                                               | fetch          | next row tuple or `None`                    |
|  [04]   | `Cursor.fetchmany(size=None)`                                                                                     | fetch          | up to `size` row tuples                     |
|  [05]   | `Cursor.fetchall()`                                                                                               | fetch          | all remaining row tuples                    |
|  [06]   | `Cursor.fetch_arrow_table()`                                                                                      | fetch          | full result as `pyarrow.Table`              |
|  [07]   | `Cursor.fetch_record_batch()`                                                                                     | fetch          | streaming `pyarrow.RecordBatchReader`       |
|  [08]   | `Cursor.fetch_df()` / `Cursor.fetch_polars()`                                                                     | fetch          | result as pandas or polars frame            |
|  [09]   | `Cursor.adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False)` | bulk           | bulk-load Arrow data into a table           |

[ENTRYPOINT_SCOPE]: connection metadata
- rail: query — `adbc_driver_manager.dbapi.Connection`
- entry family: metadata

| [INDEX] | [SURFACE]                                                                                                                                                | [ROLE]                                |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `adbc_get_info()`                                                                                                                                        | driver/vendor info code mapping       |
|  [02]   | `adbc_get_objects(*, depth='all', catalog_filter=None, db_schema_filter=None, table_name_filter=None, table_types_filter=None, column_name_filter=None)` | catalog/schema/table/column hierarchy |
|  [03]   | `adbc_get_table_schema(table_name, *, catalog_filter=None, db_schema_filter=None)`                                                                       | `pyarrow.Schema` for one table        |
|  [04]   | `adbc_get_table_types()`                                                                                                                                 | supported table type list             |
|  [05]   | `adbc_get_statistics(*, catalog_filter=None, db_schema_filter=None, table_name_filter=None, approximate=True)`                                           | table statistics reader               |

[ENTRYPOINT_SCOPE]: low-level handle operations
- rail: query — `adbc_driver_manager._lib`

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [ROLE]                                     |
| :-----: | :--------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `AdbcDatabase.set_options(**kwargs)`     | configure      | set database driver options                |
|  [02]   | `AdbcConnection.set_autocommit(enabled)` | configure      | toggle autocommit on the handle            |
|  [03]   | `AdbcStatement.set_sql_query(query)`     | prepare        | set the SQL text on a statement            |
|  [04]   | `AdbcStatement.bind(data, schema=None)`  | bind           | bind Arrow parameter data                  |
|  [05]   | `AdbcStatement.execute_query()`          | execute        | run and return `(stream, rows_affected)`   |
|  [06]   | `AdbcStatement.execute_update()`         | execute        | run for affected-row count only            |
|  [07]   | `AdbcStatement.execute_partitions()`     | execute        | run into distributed partition descriptors |

## [04]-[IMPLEMENTATION_LAW]

[ADBC_TOPOLOGY]:
- two layers: the DBAPI front end (`dbapi.connect` -> `Connection` -> `Cursor`) wraps the handle layer (`AdbcDatabase` -> `AdbcConnection` -> `AdbcStatement`)
- `connect` loads a shared driver library named by `driver` (or a registered `profile`), with `entrypoint` selecting the C init symbol
- result delivery is Arrow-native: `fetch_arrow_table`, `fetch_record_batch`, and zero-copy `ArrowArrayStreamHandle` export over the C Data Interface
- ingest modes via `StatementOptions`/`adbc_ingest`: `append`, `create`, `replace`, `create_append` against a target table/catalog/schema
- the exception hierarchy follows PEP 249 with an ADBC `status_code` (`AdbcStatusCode`) attached to every `Error`

[LOCAL_ADMISSION]:
- Open connections through `dbapi.connect` with `db_kwargs`/`conn_kwargs` carrying driver-specific option keys from `DatabaseOptions`/`ConnectionOptions`.
- Use `Connection` and `Cursor` as context managers so handles close deterministically.
- Pull results as Arrow tables or record-batch readers; drop to tuple `fetchall` only for small scalar results.
- Bulk-load through `Cursor.adbc_ingest` with an explicit `mode`; never emit per-row `INSERT` loops.
- Reserve the `_lib` handle layer for driver authors and zero-copy stream export; ordinary query work stays on the DBAPI surface.

[RAIL_LAW]:
- Package: `adbc_driver_manager`
- Owns: ADBC driver loading, PEP 249 connectivity, Arrow-native query results, bulk ingest, and database metadata introspection
- Accept: driver libraries via `connect`; bind and ingest data as Arrow; results as Arrow tables or streams
- Reject: hand-rolled DBAPI shims; per-row insert loops where `adbc_ingest` applies; wrapper-renames of `connect`/`Cursor`
