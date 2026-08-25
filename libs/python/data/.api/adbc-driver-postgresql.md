# [PY_DATA_API_ADBC_DRIVER_POSTGRESQL]

`adbc-driver-postgresql` supplies the native ADBC PostgreSQL driver for the data partition rail: a `connect` factory binding `libadbc_driver_postgresql.so` to a libpq URI as an `AdbcDatabase`, and two `enum.Enum` option vocabularies `ConnectionOptions`/`StatementOptions` keying `adbc.postgresql.*` settings. Its distinctive capability is `COPY`-path bulk ingest under `StatementOptions.USE_COPY`; partitioned execution refuses outright. `adbc_driver_manager` owns the DBAPI, ingest, partition, and metadata surface; this catalog adds the postgres option vocabulary and `COPY` law.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory and option vocabularies

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `connect`           | factory       | bind a libpq connection string to an `AdbcDatabase`       |
|  [02]   | `ConnectionOptions` | option enum   | connection-scoped `adbc.postgresql.*` setting keys        |
|  [03]   | `StatementOptions`  | option enum   | statement-scoped keys — `COPY` toggle and batch-size hint |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `connect(uri, db_kwargs=) -> AdbcDatabase`                                   | factory | low-level libpq database handle |
|  [02]   | `dbapi.connect(uri, db_kwargs=, conn_kwargs=, *, autocommit=) -> Connection` | factory | DBAPI connection over libpq     |

[CONSUMER]: `tabular/query#QUERY` `_DRIVER` rows `RemoteDriver.POSTGRESQL` at `DriverKind.NATIVE` — the kind whose connect projection threads `uri`/`db_kwargs`/`conn_kwargs` — with `db.system.name` `postgresql` on its `DbapiSeam` row.

[ENTRYPOINT_SCOPE]: `ConnectionOptions` keys

| [INDEX] | [MEMBER]             | [VALUE]                              | [CAPABILITY]                                             |
| :-----: | :------------------- | :----------------------------------- | :------------------------------------------------------- |
|  [01]   | `TRANSACTION_STATUS` | `adbc.postgresql.transaction_status` | read the libpq backend transaction state (idle/in/error) |

[ENTRYPOINT_SCOPE]: `StatementOptions` keys

| [INDEX] | [MEMBER]                | [VALUE]                                 | [CAPABILITY]                                                    |
| :-----: | :---------------------- | :-------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `BATCH_SIZE_HINT_BYTES` | `adbc.postgresql.batch_size_hint_bytes` | target bytes per Arrow result batch                             |
|  [02]   | `USE_COPY`              | `adbc.postgresql.use_copy`              | toggle the binary `COPY` path for reads and ingest (default on) |

[ENTRYPOINT_SCOPE]: bulk ingest (manager `Cursor`, postgres `COPY` backing)
- `Cursor.adbc_ingest` is the manager's; the postgres driver backs it with libpq binary `COPY`. Metadata (`adbc_get_objects`/`adbc_get_table_schema`/`adbc_get_statistics`) stays the manager's surface, while partitioned execution refuses at the driver.

| [INDEX] | [SURFACE]            | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Cursor.adbc_ingest` | instance | stream an Arrow table into postgres over binary `COPY` |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- concrete ADBC driver: `connect` pre-binds `libadbc_driver_postgresql.so`; this catalog owns the postgres `adbc.postgresql.*` option vocabulary and the binary `COPY` ingest/read law, delegating driver loading, the DBAPI surface, transaction control, partition reads, metadata, and Arrow delivery to `adbc_driver_manager`.
- factory axis: one `connect` binds the native driver with the libpq URI in `db_kwargs`; `dbapi.connect` derives the DBAPI `Connection` adding `conn_kwargs` and `autocommit` — the database object is shared, connections derive from it, never a parallel client class.
- option axis: `ConnectionOptions`/`StatementOptions` values are the canonical `adbc.postgresql.*` keys, flowing as `conn_kwargs`/statement-option dicts keyed by enum value, never ad hoc string literals; no `DatabaseOptions`, since identity and TLS keywords ride the libpq URI.
- ingest axis: `Cursor.adbc_ingest` streams a `pyarrow.Table`/`RecordBatch`/`RecordBatchReader` (or any `__arrow_c_stream__` producer) into a table over binary `COPY`, keyed by `mode` (`append`/`create`/`create_append`/`replace`), returning the row count; `USE_COPY` gates the fast path and `BATCH_SIZE_HINT_BYTES` bounds the assembled batch.
- partition axis: postgres REFUSES partitioned execution — its `AdbcStatementExecutePartitions` is a 21-instruction stub calling nothing, answering `NOT_IMPLEMENTED` (status 2) once initialized and `INVALID_STATE` (6) otherwise and writing no error message — so a partitioned read plan collapses to one streamed cursor or a caller-side key-range fan-out.
- refusal law: `Cursor.adbc_execute_partitions(operation, parameters=None) -> tuple[list[bytes], pyarrow.Schema]` raises `adbc_driver_manager.NotSupportedError` on a refusing driver, its `str()` reading exactly `NOT_IMPLEMENTED` (gaining `: <driver msg>` only where the driver wrote one) and its `status_code` reading `AdbcStatusCode.NOT_IMPLEMENTED` (value 2); exported symbol presence proves nothing, since every driver ships the flat-C wrapper and only the body decides.
- `AdbcStatusCode` carries NO `NOT_SUPPORTED` member: the enum is `OK`/`UNKNOWN`/`NOT_IMPLEMENTED`/`NOT_FOUND`/`ALREADY_EXISTS`/`INVALID_ARGUMENT`/`INVALID_STATE`/`INVALID_DATA`/`INTEGRITY`/`INTERNAL`/`IO`/`CANCELLED`/`TIMEOUT`/`UNAUTHENTICATED`/`UNAUTHORIZED`.
- transport axis: TLS rides libpq keywords (`sslmode`, `sslrootcert`, `sslcert`, `sslkey`) inside the connection URI; pooling and identity minting stay outside the driver.
- telemetry axis: inherits the manager's ADBC Go-driver OTel contract (`adbc-driver-manager.md` `[04]-[IMPLEMENTATION_LAW]`) with no postgres delta.

[STACKING]:
- `adbc-driver-manager`(`.api/adbc-driver-manager.md`): postgres `connect` pre-binds the driver library; `dbapi.connect`/`Connection`/`Cursor`, `Cursor.adbc_ingest`, `adbc_execute_partitions`/`adbc_read_partition`, `adbc_get_objects`/`adbc_get_table_schema`/`adbc_get_statistics`, and the PEP 249 error tree with `AdbcStatusCode` are the manager's, postgres backing ingest with binary `COPY` and refusing `adbc_execute_partitions` outright.
- `arro3-core`(`.api/arro3-core.md`): each result `RecordBatchReader` exposes `__arrow_c_stream__`, feeding `arro3.core.RecordBatchReader.from_stream` with zero copy; fan partitions across workers and collapse with one terminal `read_all`/`fetch_arrow_table`.
- `polars`(`.api/polars.md`): `polars.from_arrow(cursor.fetch_record_batch())` consumes the same capsule with zero copy when a frame is wanted.

[LOCAL_ADMISSION]:
- import `adbc_driver_postgresql` (and `.dbapi`) at boundary scope only; the manifest import policy bans module-level import.
- catalog, schema, table, and column discovery route through the manager's `adbc_get_objects` at the requested `depth`, with `adbc_get_table_schema` and `adbc_get_statistics` returning Arrow — never a hand-written `information_schema`/`pg_catalog` query.
- each connection exposes partition metadata: resolved URI with credentials redacted, applied option keys, ingest mode and row count, and Arrow schema.
- `adbc_driver_postgresql` owns libpq transport, `COPY` ingest, option application, and streamed Arrow result delivery, emitting Arrow record batches to the data partition owner; result materialization and dataframe conversion route to `pyarrow`/`polars`, and credential identity minting stays with the runtime owner.
