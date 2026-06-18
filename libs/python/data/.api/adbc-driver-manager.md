# [PY_DATA_API_ADBC_DRIVER_MANAGER]

`adbc_driver_manager` supplies the ADBC driver-loading core plus a PEP 249 DBAPI front end for the data query rail: `dbapi.connect`, `Connection`, and `Cursor` own the high-level surface returning Arrow tables and record-batch readers, while `AdbcDatabase`, `AdbcConnection`, and `AdbcStatement` own the low-level handle layer over the ADBC C API.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc_driver_manager`
- package: `adbc-driver-manager`
- module: `adbc_driver_manager`
- asset: C-extension runtime library (ADBC driver manager)
- rail: query

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: DBAPI surface family
- rail: query — `adbc_driver_manager.dbapi`; `apilevel='2.0'`, `paramstyle='qmark'`, `threadsafety=1`
- type family: PEP 249 ABC

| [INDEX] | [SYMBOL]     | [ROLE]                                               |
| :-----: | :----------- | :--------------------------------------------------- |
|   [1]   | `Connection` | autocommit/transaction control plus ADBC metadata    |
|   [2]   | `Cursor`     | execute, fetch tuples, fetch Arrow tables and frames |

[PUBLIC_TYPE_SCOPE]: low-level handle family
- rail: query — `adbc_driver_manager._lib`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [ROLE]                                      |
| :-----: | :----------------------- | :-------------- | :------------------------------------------ |
|   [1]   | `AdbcDatabase`           | handle          | shared driver/database configuration handle |
|   [2]   | `AdbcConnection`         | handle          | connection handle over a database           |
|   [3]   | `AdbcStatement`          | handle          | query/statement execution handle            |
|   [4]   | `ArrowArrayHandle`       | capsule wrapper | exported Arrow array C-data pointer         |
|   [5]   | `ArrowArrayStreamHandle` | capsule wrapper | exported Arrow stream C-data pointer        |
|   [6]   | `ArrowSchemaHandle`      | capsule wrapper | exported Arrow schema C-data pointer        |

[PUBLIC_TYPE_SCOPE]: option and info enums
- rail: query

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------- |
|   [1]   | `DatabaseOptions`   | str enum      | `URI`, `USERNAME`, `PASSWORD`                             |
|   [2]   | `ConnectionOptions` | str enum      | `CURRENT_CATALOG`, `CURRENT_DB_SCHEMA`, `ISOLATION_LEVEL` |
|   [3]   | `StatementOptions`  | str enum      | ingest mode/target, bind-by-name, incremental             |
|   [4]   | `AdbcInfoCode`      | int enum      | vendor/driver name, version, Arrow version codes          |
|   [5]   | `AdbcStatusCode`    | int enum      | `OK`, `NOT_FOUND`, `INVALID_STATE`, `CANCELLED`, ...      |
|   [6]   | `GetObjectsDepth`   | int enum      | `ALL`, `CATALOGS`, `DB_SCHEMAS`, `TABLES`                 |

[PUBLIC_TYPE_SCOPE]: error hierarchy
- rail: query — PEP 249 exception tree rooted at `Error`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                    |
| :-----: | :------------------ | :------------ | :---------------------------------------- |
|   [1]   | `Error`             | base          | root ADBC/DBAPI error with `status_code`  |
|   [2]   | `DatabaseError`     | error         | server-side error base                    |
|   [3]   | `OperationalError`  | error         | operational fault (connection, transient) |
|   [4]   | `ProgrammingError`  | error         | malformed query or API misuse             |
|   [5]   | `IntegrityError`    | error         | constraint violation                      |
|   [6]   | `NotSupportedError` | error         | unsupported feature or driver call        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DBAPI connection lifecycle
- rail: query — `adbc_driver_manager.dbapi`

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [ROLE]                                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|   [1]   | `connect(driver=None, uri=None, *, profile=None, entrypoint=None, db_kwargs=None, conn_kwargs=None, autocommit=False)` | open           | load a driver and open a `Connection`    |
|   [2]   | `Connection.cursor(*, adbc_stmt_kwargs=None)`                                                                          | cursor         | create a `Cursor`                        |
|   [3]   | `Connection.commit()`                                                                                                  | transaction    | commit the active transaction            |
|   [4]   | `Connection.rollback()`                                                                                                | transaction    | roll back the active transaction         |
|   [5]   | `Connection.close()`                                                                                                   | lifecycle      | close the connection                     |
|   [6]   | `Connection.adbc_clone()`                                                                                              | lifecycle      | open a second connection on the database |

[ENTRYPOINT_SCOPE]: cursor execution and fetch
- rail: query — `adbc_driver_manager.dbapi.Cursor`

| [INDEX] | [SURFACE]                                                                                                         | [ENTRY_FAMILY] | [ROLE]                                      |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `Cursor.execute(operation, parameters=None)`                                                                      | execute        | run one query with optional bind parameters |
|   [2]   | `Cursor.executemany(operation, seq_of_parameters)`                                                                | execute        | run a query against a parameter batch       |
|   [3]   | `Cursor.fetchone()`                                                                                               | fetch          | next row tuple or `None`                    |
|   [4]   | `Cursor.fetchmany(size=None)`                                                                                     | fetch          | up to `size` row tuples                     |
|   [5]   | `Cursor.fetchall()`                                                                                               | fetch          | all remaining row tuples                    |
|   [6]   | `Cursor.fetch_arrow_table()`                                                                                      | fetch          | full result as `pyarrow.Table`              |
|   [7]   | `Cursor.fetch_record_batch()`                                                                                     | fetch          | streaming `pyarrow.RecordBatchReader`       |
|   [8]   | `Cursor.fetch_df()` / `Cursor.fetch_polars()`                                                                     | fetch          | result as pandas or polars frame            |
|   [9]   | `Cursor.adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False)` | bulk           | bulk-load Arrow data into a table           |

[ENTRYPOINT_SCOPE]: connection metadata
- rail: query — `adbc_driver_manager.dbapi.Connection`
- entry family: metadata

| [INDEX] | [SURFACE]                                                                                                                                                | [ROLE]                                |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `adbc_get_info()`                                                                                                                                        | driver/vendor info code mapping       |
|   [2]   | `adbc_get_objects(*, depth='all', catalog_filter=None, db_schema_filter=None, table_name_filter=None, table_types_filter=None, column_name_filter=None)` | catalog/schema/table/column hierarchy |
|   [3]   | `adbc_get_table_schema(table_name, *, catalog_filter=None, db_schema_filter=None)`                                                                       | `pyarrow.Schema` for one table        |
|   [4]   | `adbc_get_table_types()`                                                                                                                                 | supported table type list             |
|   [5]   | `adbc_get_statistics(*, catalog_filter=None, db_schema_filter=None, table_name_filter=None, approximate=True)`                                           | table statistics reader               |

[ENTRYPOINT_SCOPE]: low-level handle operations
- rail: query — `adbc_driver_manager._lib`

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [ROLE]                                     |
| :-----: | :--------------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `AdbcDatabase.set_options(**kwargs)`     | configure      | set database driver options                |
|   [2]   | `AdbcConnection.set_autocommit(enabled)` | configure      | toggle autocommit on the handle            |
|   [3]   | `AdbcStatement.set_sql_query(query)`     | prepare        | set the SQL text on a statement            |
|   [4]   | `AdbcStatement.bind(data, schema=None)`  | bind           | bind Arrow parameter data                  |
|   [5]   | `AdbcStatement.execute_query()`          | execute        | run and return `(stream, rows_affected)`   |
|   [6]   | `AdbcStatement.execute_update()`         | execute        | run for affected-row count only            |
|   [7]   | `AdbcStatement.execute_partitions()`     | execute        | run into distributed partition descriptors |

## [4]-[IMPLEMENTATION_LAW]

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
