# [PY_DATA_API_ADBC_DRIVER_POSTGRESQL]

`adbc-driver-postgresql` supplies the native ADBC PostgreSQL driver for the data partition rail: a `connect` factory binding the native `libadbc_driver_postgresql.so` to a libpq connection string as an `AdbcDatabase`, and two postgres-specific `enum.Enum` option vocabularies (`ConnectionOptions`, `StatementOptions`) keying the driver's `adbc.postgresql.*` settings. Its distinctive capability is `COPY`-path bulk ingest: `Cursor.adbc_ingest` streams Arrow record batches into a table through PostgreSQL binary `COPY` under the `StatementOptions.USE_COPY` toggle, and partitioned reads split a query across libpq server-side cursors returning Arrow batches. Consumption rides `dbapi.connect` on the `REMOTE_PARTITION_DEEPEN` path and the ingest egress path — never a hand-stitched `COPY` protocol encoder, a per-row prepare-bind-insert loop where `adbc_ingest` streams the whole Arrow table, or a parallel DBAPI surface the manager already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-postgresql`
- package: `adbc-driver-postgresql`
- import: `adbc_driver_postgresql`
- owner: `data`
- rail: partition
- license: `Apache-2.0`
- entry points: library use is import-only; `connect` returns an `AdbcDatabase`, `dbapi.connect` returns a DBAPI `Connection`
- capability: libpq connection-string binding, Arrow `COPY`-path bulk ingest with create/append/replace modes, partitioned result retrieval via `adbc_execute_partitions`/`adbc_read_partition` over server-side cursors, catalog and statistics introspection through the manager's `adbc_get_objects`/`adbc_get_table_schema`/`adbc_get_statistics`, transaction-status inspection, per-statement batch-size tuning, and DBAPI cursor access yielding Arrow record batches

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory and option vocabularies
- rail: partition

`connect` is the single low-level factory; `ConnectionOptions` and `StatementOptions` are `enum.Enum` option keys whose values are the driver's `adbc.postgresql.*` setting strings. Unlike the Flight SQL driver, the postgres driver exposes no `DatabaseOptions` vocabulary and no OAuth axes — database identity is the libpq connection URI, and transport security rides libpq `sslmode`/`sslrootcert` keywords inside that URI. `dbapi` carries the DBAPI facade — `Connection`, `Cursor`, the typed error hierarchy — shared with every ADBC driver through the manager.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `connect`           | factory       | bind a libpq connection string to an `AdbcDatabase`       |
|  [02]   | `ConnectionOptions` | option enum   | connection-scoped `adbc.postgresql.*` setting keys        |
|  [03]   | `StatementOptions`  | option enum   | statement-scoped keys — `COPY` toggle and batch-size hint |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories
- rail: partition

`connect` mints the low-level `AdbcDatabase` bound to the native driver path with `db_kwargs` carrying the libpq URI; `dbapi.connect` wraps that in a DBAPI `Connection`, adds `conn_kwargs`, and exposes an `autocommit` toggle because ADBC separates the shared database object from per-connection transaction state. This postgres driver has no `DatabaseOptions` enum, so the URI itself carries host/port/dbname/user/password and libpq TLS keywords.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                     | [CAPABILITY]                    |
| :-----: | :-------------- | :------------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `connect`       | `connect(uri, db_kwargs=) -> AdbcDatabase`                                       | low-level libpq database handle |
|  [02]   | `dbapi.connect` | `connect(uri, db_kwargs=, conn_kwargs=, *, autocommit=, **kwargs) -> Connection` | DBAPI connection over libpq     |

[ENTRYPOINT_SCOPE]: `ConnectionOptions` keys
- rail: partition
- Postgres-specific connection settings carry the `ConnectionOptions.` prefix and apply at connection scope. Transaction status is read-only, reporting the libpq backend transaction state.

| [INDEX] | [MEMBER]             | [VALUE]                              | [CAPABILITY]                                             |
| :-----: | :------------------- | :----------------------------------- | :------------------------------------------------------- |
|  [01]   | `TRANSACTION_STATUS` | `adbc.postgresql.transaction_status` | read the libpq backend transaction state (idle/in/error) |

[ENTRYPOINT_SCOPE]: `StatementOptions` keys
- rail: partition
- Postgres-specific statement settings carry the `StatementOptions.` prefix. `USE_COPY` toggles the binary `COPY` fast path for reads and ingest; `BATCH_SIZE_HINT_BYTES` bounds the Arrow batch the driver assembles per fetch.

| [INDEX] | [MEMBER]                | [VALUE]                                 | [CAPABILITY]                                       |
| :-----: | :---------------------- | :-------------------------------------- | :------------------------------------------------- |
|  [01]   | `BATCH_SIZE_HINT_BYTES` | `adbc.postgresql.batch_size_hint_bytes` | target bytes per Arrow result batch                |
|  [02]   | `USE_COPY`              | `adbc.postgresql.use_copy`              | toggle the binary `COPY` path for reads and ingest |

[ENTRYPOINT_SCOPE]: bulk ingest and partition surface (via the manager `Cursor`)
- rail: partition
- These `adbc_*` methods live on the manager's DBAPI `Cursor`/`Connection`; the postgres driver backs them with libpq `COPY` and server-side cursors. Listed here because they are the postgres arm's load-bearing consumption path, not a parallel surface.

```python signature
Cursor.adbc_ingest(table_name, data, mode='create', *, catalog_name=None, db_schema_name=None, temporary=False) -> int
Cursor.adbc_execute_partitions(operation, parameters=None) -> (list[bytes], pyarrow.Schema)
Cursor.adbc_read_partition(partition: bytes)
Connection.adbc_get_objects(*, depth='all', catalog_filter=None, db_schema_filter=None, table_name_filter=None, table_types_filter=None, column_name_filter=None) -> pyarrow.RecordBatchReader
Connection.adbc_get_table_schema(table_name, *, catalog_filter=None, db_schema_filter=None) -> pyarrow.Schema
Connection.adbc_get_statistics(*, catalog_filter=None, db_schema_filter=None, table_name_filter=None, approximate=True) -> pyarrow.RecordBatchReader
```

| [INDEX] | [SURFACE]                          | [CAPABILITY]                                           |
| :-----: | :--------------------------------- | :----------------------------------------------------- |
|  [01]   | `Cursor.adbc_ingest`               | stream an Arrow table into postgres over binary `COPY` |
|  [02]   | `Cursor.adbc_execute_partitions`   | split a query into serialized partition descriptors    |
|  [03]   | `Cursor.adbc_read_partition`       | open one partition descriptor as an Arrow batch stream |
|  [04]   | `Connection.adbc_get_objects`      | catalog/schema/table/column introspection as Arrow     |
|  [05]   | `Connection.adbc_get_table_schema` | one table's Arrow schema                               |
|  [06]   | `Connection.adbc_get_statistics`   | table row-count/size statistics as Arrow               |

## [04]-[IMPLEMENTATION_LAW]

[PARTITION_POSTGRESQL]:
- import: `import adbc_driver_postgresql` (and `adbc_driver_postgresql.dbapi`) at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `connect` owns binding to the native `libadbc_driver_postgresql.so` with the libpq URI in `db_kwargs`; `dbapi.connect` is the DBAPI row that adds `conn_kwargs` and the `autocommit` toggle, never a parallel client class — the database object is shared and connections are derived from it.
- option axis: `ConnectionOptions`/`StatementOptions` enum values are the canonical `adbc.postgresql.*` keys; settings flow as `conn_kwargs`/statement-option dictionaries keyed by enum value, never as ad hoc string literals. This postgres driver exposes no `DatabaseOptions` — database identity and TLS keywords live inside the libpq URI, so no per-setting builder type or database-option table is minted where the URI already carries them.
- ingest axis: `Cursor.adbc_ingest` is the postgres-distinctive egress — it streams a `pyarrow.Table`/`RecordBatch`/`RecordBatchReader` (or any `__arrow_c_stream__` producer) into a table over PostgreSQL binary `COPY`, keyed by `mode` (`append`/`create`/`create_append`/`replace`), returning the row count; `StatementOptions.USE_COPY` gates the `COPY` fast path and `StatementOptions.BATCH_SIZE_HINT_BYTES` bounds the assembled batch. A per-row prepare-bind-insert loop where `adbc_ingest` streams the whole table is rejected.
- partition axis: `REMOTE_PARTITION_DEEPEN` runs `Cursor.adbc_execute_partitions` to receive serialized partition descriptors, then opens each with `adbc_read_partition` as an independent Arrow batch stream over libpq server-side cursors; partition handoff is the native driver's, never a hand-stitched cursor loop.
- introspection axis: catalog, schema, table, and column discovery route through `Connection.adbc_get_objects` at the requested `depth`; `adbc_get_table_schema` returns one table's Arrow schema and `adbc_get_statistics` returns row-count/size estimates — each an Arrow surface, never a hand-written `information_schema`/`pg_catalog` query.
- manager axis: the concrete driver delegates loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree), transaction control, and Arrow result delivery to `adbc_driver_manager`; this catalog adds only the postgres option vocabulary and the `COPY` ingest law — never a parallel DBAPI implementation. Typed error tree and `AdbcStatusCode` mapping stay the manager's.
- arrow egress axis: each result `RecordBatchReader` exposes `__arrow_c_stream__`, so a postgres result feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` with zero copy; fan partitions across workers and collapse them with one terminal `read_all`/`fetch_arrow_table`.
- transport axis: TLS rides libpq keywords (`sslmode`, `sslrootcert`, `sslcert`, `sslkey`) inside the connection URI — no driver option table duplicates them; connection pooling and identity minting stay outside the driver.
- telemetry axis: the Go driver embeds its own OTel tracer configured through the standard `OTEL_TRACES_EXPORTER`/`OTEL_EXPORTER_OTLP_*`/`OTEL_SERVICE_NAME` env family, and the raw `adbc.telemetry.trace_parent` connection option — spelled by no Python option enum, verified against the shipped `libadbc_driver_postgresql.so` — accepts a W3C `traceparent` so driver spans join the caller's trace; no Python-side instrumentor covers this driver.
- evidence: each connection captures the resolved URI (credentials redacted), applied option keys, ingest mode and row count, partition count, and Arrow schema as a partition receipt.
- boundary: the driver owns the libpq transport, `COPY` ingest, option application, and partition retrieval, emitting Arrow record batches consumed by the data partition owner; result-set materialization and dataframe conversion route to `pyarrow`/`polars`, and credential identity minting stays with the runtime owner.

[RAIL_LAW]:
- Package: `adbc-driver-postgresql`
- Owns: libpq endpoint binding, Arrow `COPY`-path bulk ingest, partitioned Arrow result retrieval over server-side cursors, catalog/schema/statistics introspection, transaction-status inspection, and per-statement batch-size and `COPY` tuning
- Accept: remote postgres partition deepening feeding Arrow record batches to the data partition, query, and dataframe owners; Arrow-table bulk load into postgres over binary `COPY`
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a hand-stitched `COPY` encoder or per-row insert loop where `adbc_ingest` streams the whole Arrow table; a hand-written `pg_catalog` query where `adbc_get_objects` returns Arrow; a `DatabaseOptions`-style table duplicating libpq URI keywords; string-literal option keys bypassing `ConnectionOptions`/`StatementOptions`; credential identity minting the runtime owns
