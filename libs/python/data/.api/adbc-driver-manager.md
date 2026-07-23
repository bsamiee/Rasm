# [PY_DATA_API_ADBC_DRIVER_MANAGER]

`adbc_driver_manager` owns ADBC driver loading and the PEP 249 DBAPI front end for the query rail: `dbapi.connect` opens a `Connection` that mints `Cursor` objects returning Arrow tables and record-batch readers over the low-level `AdbcDatabase`/`AdbcConnection`/`AdbcStatement` handle layer bound to the ADBC C Data Interface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc_driver_manager`
- package: `adbc-driver-manager` (`Apache-2.0`)
- module: `adbc_driver_manager`
- namespaces: `dbapi`
- rail: query

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: DBAPI class surface
- DBAPI attributes: `apilevel='2.0'`, `paramstyle='qmark'`, `threadsafety=1`

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Connection` | class         | autocommit and transaction control plus ADBC metadata |
|  [02]   | `Cursor`     | class         | execute, fetch tuples, fetch Arrow tables and frames  |

[PUBLIC_TYPE_SCOPE]: low-level handle family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :----------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `AdbcDatabase`           | handle          | shared driver/database configuration handle |
|  [02]   | `AdbcConnection`         | handle          | connection handle over a database           |
|  [03]   | `AdbcStatement`          | handle          | query/statement execution handle            |
|  [04]   | `ArrowArrayHandle`       | capsule wrapper | exported Arrow array C-data pointer         |
|  [05]   | `ArrowArrayStreamHandle` | capsule wrapper | exported Arrow stream C-data pointer        |
|  [06]   | `ArrowSchemaHandle`      | capsule wrapper | exported Arrow schema C-data pointer        |

[PUBLIC_TYPE_SCOPE]: option and info enums
- `DatabaseOptions`: `URI` `USERNAME` `PASSWORD`
- `ConnectionOptions`: `CURRENT_CATALOG` `CURRENT_DB_SCHEMA` `ISOLATION_LEVEL`
- `StatementOptions`: `INGEST_MODE` `INGEST_TARGET_TABLE` `INGEST_TARGET_CATALOG` `INGEST_TARGET_DB_SCHEMA` `INGEST_TEMPORARY` `BIND_BY_NAME` `INCREMENTAL` `PROGRESS`
- `GetObjectsDepth`: `ALL` `CATALOGS` `DB_SCHEMAS` `TABLES`
- `AdbcStatusCode`: `OK` `UNKNOWN` `NOT_IMPLEMENTED` `NOT_FOUND` `ALREADY_EXISTS` `INVALID_ARGUMENT` `INVALID_STATE` `INVALID_DATA` `INTEGRITY` `INTERNAL` `IO` `CANCELLED` `TIMEOUT` `UNAUTHENTICATED` `UNAUTHORIZED`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------ | :------------ | :----------------------------------------------- |
|  [01]   | `DatabaseOptions`   | str enum      | database identity and credential keys            |
|  [02]   | `ConnectionOptions` | str enum      | connection catalog, schema, isolation keys       |
|  [03]   | `StatementOptions`  | str enum      | ingest mode, target, and execution keys          |
|  [04]   | `AdbcInfoCode`      | int enum      | vendor/driver name, version, Arrow version codes |
|  [05]   | `AdbcStatusCode`    | int enum      | ADBC status codes                                |
|  [06]   | `GetObjectsDepth`   | int enum      | catalog-hierarchy traversal depth                |

[PUBLIC_TYPE_SCOPE]: error hierarchy
- PEP 249 exception tree rooted at `Error`, every instance carrying an ADBC `status_code`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------ | :------------ | :---------------------------------------- |
|  [01]   | `Error`             | base          | root ADBC/DBAPI error with `status_code`  |
|  [02]   | `DatabaseError`     | error         | server-side error base                    |
|  [03]   | `OperationalError`  | error         | operational fault (connection, transient) |
|  [04]   | `ProgrammingError`  | error         | malformed query or API misuse             |
|  [05]   | `IntegrityError`    | error         | constraint violation                      |
|  [06]   | `NotSupportedError` | error         | unsupported feature or driver call        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DBAPI connection lifecycle
- `connect` carry: `connect(driver=None, uri=None, *, profile=None, entrypoint=None, db_kwargs=None, conn_kwargs=None, autocommit=False)`

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `connect`                                     | factory  | load a driver and open a `Connection`    |
|  [02]   | `Connection.cursor(*, adbc_stmt_kwargs=None)` | factory  | create a `Cursor`                        |
|  [03]   | `Connection.commit()`                         | instance | commit the active transaction            |
|  [04]   | `Connection.rollback()`                       | instance | roll back the active transaction         |
|  [05]   | `Connection.close()`                          | instance | close the connection                     |
|  [06]   | `Connection.adbc_clone()`                     | factory  | open a second connection on the database |

[ENTRYPOINT_SCOPE]: cursor execution and fetch
- `adbc_ingest` carry: `adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False)`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Cursor.execute(operation, parameters=None)`             | instance | run one query with optional bind parameters |
|  [02]   | `Cursor.executemany(operation, seq_of_parameters)`       | instance | run a query against a parameter batch       |
|  [03]   | `Cursor.fetchone()`                                      | instance | next row tuple or `None`                    |
|  [04]   | `Cursor.fetchmany(size=None)`                            | instance | up to `size` row tuples                     |
|  [05]   | `Cursor.fetchall()`                                      | instance | all remaining row tuples                    |
|  [06]   | `Cursor.fetch_arrow_table()`                             | instance | full result as `pyarrow.Table`              |
|  [07]   | `Cursor.fetch_record_batch()`                            | instance | streaming `pyarrow.RecordBatchReader`       |
|  [08]   | `Cursor.fetch_df()`                                      | instance | result as a pandas frame                    |
|  [09]   | `Cursor.fetch_polars()`                                  | instance | result as a polars frame                    |
|  [10]   | `Cursor.adbc_ingest`                                     | instance | bulk-load Arrow data into a table           |
|  [11]   | `Cursor.adbc_prepare(operation)`                         | instance | pre-compile and return parameter schema     |
|  [12]   | `Cursor.adbc_execute_schema(operation, parameters=None)` | instance | result `pyarrow.Schema` without execution   |
|  [13]   | `Cursor.adbc_execute_partitions(operation, params=None)` | instance | return `(partitions, schema)` descriptors   |
|  [14]   | `Cursor.adbc_read_partition(partition)`                  | instance | open one partition as a `RecordBatchReader` |
|  [15]   | `Cursor.adbc_cancel()`                                   | instance | cancel the in-flight statement              |
|  [16]   | `Cursor.adbc_statement`                                  | property | low-level `AdbcStatement` handle            |
|  [17]   | `Cursor.rowcount`                                        | property | affected-row count                          |
|  [18]   | `Cursor.description`                                     | property | DBAPI column description                    |

[ENTRYPOINT_SCOPE]: connection metadata
- Every member is an instance method on `Connection`.
- `adbc_get_objects` carry: `adbc_get_objects(*, depth='all', catalog_filter=None, db_schema_filter=None, table_name_filter=None, table_types_filter=None, column_name_filter=None)`
- `adbc_get_statistics` carry: `adbc_get_statistics(*, catalog_filter=None, db_schema_filter=None, table_name_filter=None, approximate=True)`
- `adbc_get_table_schema` carry: `adbc_get_table_schema(table_name, *, catalog_filter=None, db_schema_filter=None)`

| [INDEX] | [SURFACE]                           | [CAPABILITY]                          |
| :-----: | :---------------------------------- | :------------------------------------ |
|  [01]   | `Connection.adbc_get_info()`        | driver/vendor info code mapping       |
|  [02]   | `Connection.adbc_get_objects`       | catalog/schema/table/column hierarchy |
|  [03]   | `Connection.adbc_get_table_schema`  | `pyarrow.Schema` for one table        |
|  [04]   | `Connection.adbc_get_table_types()` | supported table type list             |
|  [05]   | `Connection.adbc_get_statistics`    | table statistics reader               |

[ENTRYPOINT_SCOPE]: low-level handle operations
- `AdbcConnection` setup: `.set_autocommit(enabled)` `.set_options(**kwargs)` `.get_option(key)`
- `AdbcConnection` metadata: `.get_info()` `.get_objects(...)` `.get_table_schema(...)` `.get_table_types()` `.get_statistics(...)` `.get_statistic_names()`

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `AdbcDatabase(**kwargs)`                 | ctor     | open a database driver handle         |
|  [02]   | `AdbcDatabase.set_options(**kwargs)`     | instance | set database driver options           |
|  [03]   | `AdbcConnection(database, **kwargs)`     | ctor     | open a connection on a database       |
|  [04]   | `AdbcConnection.commit()`                | instance | commit on the handle                  |
|  [05]   | `AdbcConnection.rollback()`              | instance | roll back on the handle               |
|  [06]   | `AdbcStatement(connection)`              | ctor     | open a statement on a connection      |
|  [07]   | `AdbcStatement.set_options(**kwargs)`    | instance | set statement options                 |
|  [08]   | `AdbcStatement.set_sql_query(query)`     | instance | set SQL text                          |
|  [09]   | `AdbcStatement.set_substrait_plan(plan)` | instance | set a Substrait plan                  |
|  [10]   | `AdbcStatement.prepare()`                | instance | pre-compile the statement             |
|  [11]   | `AdbcStatement.get_parameter_schema()`   | instance | read the bind-parameter `Schema`      |
|  [12]   | `AdbcStatement.bind(data, schema=None)`  | instance | bind Arrow array data                 |
|  [13]   | `AdbcStatement.bind_stream(stream)`      | instance | bind a record-batch stream            |
|  [14]   | `AdbcStatement.execute_query()`          | instance | run, return `(stream, rows_affected)` |
|  [15]   | `AdbcStatement.execute_update()`         | instance | run for affected-row count            |
|  [16]   | `AdbcStatement.execute_partitions()`     | instance | run into `(partitions, schema, rows)` |
|  [17]   | `AdbcStatement.execute_schema()`         | instance | result schema without execution       |
|  [18]   | `AdbcStatement.cancel()`                 | instance | cancel the in-flight statement        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two layers stack: `dbapi.connect` -> `Connection` -> `Cursor` wraps the handle layer `AdbcDatabase` -> `AdbcConnection` -> `AdbcStatement`; this driver-agnostic manager is the shared core every concrete driver (`adbc_driver_flightsql`, `adbc_driver_postgresql`, `adbc_driver_sqlite`) loads through.
- `connect` loads a shared driver library named by `driver` (or a registered `profile`), `entrypoint` selecting the C init symbol; a concrete driver's `connect` pre-binds the library path.
- Result delivery is Arrow-native: `fetch_arrow_table`, `fetch_record_batch`, and zero-copy `ArrowArrayStreamHandle` export ride the C Data Interface, every result implementing `__arrow_c_stream__`/`__arrow_c_array__`.
- Ingest folds through `StatementOptions`/`adbc_ingest` in `append`, `create`, `replace`, and `create_append` modes against a target table/catalog/schema.
- Every `Error` carries an ADBC `status_code` (`AdbcStatusCode`); the boundary maps `INVALID_STATE`/`CANCELLED`/`TIMEOUT`/`UNAUTHENTICATED` to typed retry-vs-fail decisions.
- Every Go-backed concrete driver embeds its own OTel tracer through the `OTEL_TRACES_EXPORTER`/`OTEL_EXPORTER_OTLP_*`/`OTEL_SERVICE_NAME` env family, and the raw connection option `adbc.telemetry.trace_parent` accepts a W3C `traceparent` so driver spans join the caller's trace; no Python-side instrumentor covers a Go-backed driver.

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): `Cursor.fetch_record_batch()` yields a `pyarrow.RecordBatchReader` whose `__arrow_c_stream__` feeds `arro3.core.RecordBatchReader.from_stream` zero-copy, and `Cursor.fetch_arrow_table()` round-trips through `arro3.core.Table.from_arrow`.
- `polars`(`.api/polars.md`): `Cursor.fetch_polars()` is the native escape, and `polars.from_arrow(cursor.fetch_record_batch())` consumes the capsule for a lazy frame.
- partition spine: `adbc_execute_partitions` returns opaque partition descriptors, each opened by `adbc_read_partition` as an independent `RecordBatchReader`, so a distributed driver fans result endpoints across workers on the manager's handoff.
- retry rail: wrapping `execute`/`adbc_ingest` retries a `status_code in {TIMEOUT, IO}` under the runtime backoff while `IntegrityError`/`ProgrammingError` fail fast.

[LOCAL_ADMISSION]:
- Open connections through `dbapi.connect`, `db_kwargs`/`conn_kwargs` carrying driver option keys from `DatabaseOptions`/`ConnectionOptions`.
- Bind `Connection` and `Cursor` as context managers so handles close deterministically on exit.
- Pull results as Arrow tables or record-batch readers feeding arro3/polars; tuple `fetchall` serves only small scalar results.
- Bulk-load through `Cursor.adbc_ingest` with an explicit `mode`.
- Reserve the handle layer for driver authors, partition reads, and zero-copy stream export; ordinary query work stays on the DBAPI surface.

[RAIL_LAW]:
- Package: `adbc_driver_manager`
- Owns: ADBC driver loading, PEP 249 connectivity, Arrow-native query results, bulk ingest, partition reads, and database metadata introspection
- Accept: driver libraries via `connect`; Arrow bind and ingest; results as Arrow tables or streams feeding arro3/polars
- Reject: hand-rolled DBAPI shims; per-row insert loops where `adbc_ingest` applies; hand-stitched partition fetch loops where `adbc_read_partition` applies; wrapper-renames of `connect`/`Cursor`
