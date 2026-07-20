# [PY_DATA_API_ADBC_DRIVER_SNOWFLAKE]

`adbc-driver-snowflake` supplies the ADBC Snowflake warehouse driver for the data partition rail: a `connect` factory binding the native `libadbc_driver_snowflake.so` to a Snowflake account as an `AdbcDatabase`, two `enum.Enum` option vocabularies (`DatabaseOptions`, `StatementOptions`) keying every account, connection, and statement setting by canonical `adbc.snowflake.*` string, and one `AuthType` value vocabulary selecting the authentication mode. Consumption rides `dbapi.connect` on the `REMOTE_PARTITION_DEEPEN` path — Snowflake returns each result set as Arrow chunks staged in cloud object storage, fetched concurrently under `PREFETCH_CONCURRENCY`, never a re-implemented Snowflake REST client, a hand-stitched chunk downloader, or a per-setting builder type where an option enum string already keys the value.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-snowflake`
- package: `adbc-driver-snowflake`
- import: `adbc_driver_snowflake`
- owner: `data`
- rail: partition
- license: `Apache-2.0`
- entry points: library use is import-only; `connect` returns an `AdbcDatabase`, `dbapi.connect` returns a DBAPI `Connection`
- capability: Snowflake account binding over the Go gosnowflake driver, concurrent Arrow result-chunk retrieval under a prefetch queue, staged bulk `adbc_ingest` with writer/upload/copy concurrency axes, authentication selection, high-precision decimal control, per-connection warehouse/role/database/schema routing, query tagging, and DBAPI cursor access yielding Arrow record batches

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory and option vocabularies
- rail: partition

`connect` is the single low-level factory; `DatabaseOptions` and `StatementOptions` are `enum.Enum` option keys whose values are the canonical `adbc.snowflake.*` setting strings consumed by `db_kwargs`/statement options. `AuthType` is the `str` value vocabulary for `DatabaseOptions.AUTH_TYPE`, selecting the credential flow. `dbapi` carries the DBAPI facade — `Connection`, `Cursor`, the typed error hierarchy.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `connect`          | factory       | bind a Snowflake account to an `AdbcDatabase`                |
|  [02]   | `DatabaseOptions`  | option enum   | account- and connection-scoped `adbc.snowflake.*` keys       |
|  [03]   | `StatementOptions` | option enum   | statement-scoped keys, incl. prefetch and ingest axes        |
|  [04]   | `AuthType`         | value enum    | `str` credential-mode values for `DatabaseOptions.AUTH_TYPE` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories
- rail: partition

`connect` mints the low-level `AdbcDatabase` bound to the native driver path; `dbapi.connect` wraps that in a DBAPI `Connection` and adds `conn_kwargs` because ADBC separates the shared database object from per-connection state. Both key `db_kwargs`/`conn_kwargs` by `DatabaseOptions` enum values.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                  | [CAPABILITY]                        |
| :-----: | :-------------- | :---------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `connect`       | `connect(uri=None, db_kwargs=None) -> AdbcDatabase`                           | low-level Snowflake database handle |
|  [02]   | `dbapi.connect` | `connect(uri=None, db_kwargs=None, conn_kwargs=None, **kwargs) -> Connection` | DBAPI connection over Snowflake |

[ENTRYPOINT_SCOPE]: `DatabaseOptions` keys
- rail: partition

Each member value is the canonical setting string passed in `db_kwargs`. Timeouts are duration strings; `AUTH_TYPE` selects an `AuthType` value and gates which credential rows apply; account routing sets warehouse, role, database, and schema at session scope.

| [INDEX] | [MEMBER]                   | [VALUE]                                                           | [CAPABILITY]                          |
| :-----: | :------------------------- | :---------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `ACCOUNT`                  | `adbc.snowflake.sql.account`                                      | Snowflake account identifier          |
|  [02]   | `REGION`                   | `adbc.snowflake.sql.region`                                       | account region                        |
|  [03]   | `HOST`                     | `adbc.snowflake.sql.uri.host`                                     | endpoint host override                |
|  [04]   | `PORT`                     | `adbc.snowflake.sql.uri.port`                                     | endpoint port override                |
|  [05]   | `PROTOCOL`                 | `adbc.snowflake.sql.uri.protocol`                                 | endpoint protocol override            |
|  [06]   | `DATABASE`                 | `adbc.snowflake.sql.db`                                           | session database                      |
|  [07]   | `SCHEMA`                   | `adbc.snowflake.sql.schema`                                       | session schema                        |
|  [08]   | `WAREHOUSE`                | `adbc.snowflake.sql.warehouse`                                    | compute warehouse                     |
|  [09]   | `ROLE`                     | `adbc.snowflake.sql.role`                                         | session role                          |
|  [10]   | `AUTH_TYPE`                | `adbc.snowflake.sql.auth_type`                                    | credential mode selector (`AuthType`) |
|  [11]   | `AUTH_TOKEN`               | `adbc.snowflake.sql.client_option.auth_token`                     | OAuth/PAT bearer token                |
|  [12]   | `AUTH_OKTA_URL`            | `adbc.snowflake.sql.client_option.okta_url`                       | Okta authenticator URL                |
|  [13]   | `CLIENT_IDENTITY_PROVIDER` | `adbc.snowflake.sql.client_option.identity_provider`              | workload-identity provider            |
|  [14]   | `JWT_PRIVATE_KEY`          | `adbc.snowflake.sql.client_option.jwt_private_key`                | key-pair auth private-key path        |
|  [15]   | `JWT_PRIVATE_KEY_VALUE`    | `adbc.snowflake.sql.client_option.jwt_private_key_pkcs8_value`    | inline PKCS#8 private-key bytes       |
|  [16]   | `JWT_PRIVATE_KEY_PASSWORD` | `adbc.snowflake.sql.client_option.jwt_private_key_pkcs8_password` | PKCS#8 private-key passphrase         |
|  [17]   | `JWT_EXPIRE_TIMEOUT`       | `adbc.snowflake.sql.client_option.jwt_expire_timeout`             | key-pair JWT expiry window            |
|  [18]   | `CLIENT_REQUEST_MFA_TOKEN` | `adbc.snowflake.sql.client_option.cache_mfa_token`                | cache the MFA token                   |
|  [19]   | `CLIENT_STORE_TEMP_CRED`   | `adbc.snowflake.sql.client_option.store_temp_creds`               | cache temporary credentials           |
|  [20]   | `APPLICATION_NAME`         | `adbc.snowflake.sql.client_option.app_name`                       | reported application name             |
|  [21]   | `LOGIN_TIMEOUT`            | `adbc.snowflake.sql.client_option.login_timeout`                  | login handshake timeout               |
|  [22]   | `REQUEST_TIMEOUT`          | `adbc.snowflake.sql.client_option.request_timeout`                | per-request timeout                   |
|  [23]   | `CLIENT_TIMEOUT`           | `adbc.snowflake.sql.client_option.client_timeout`                 | overall client timeout                |
|  [24]   | `KEEP_SESSION_ALIVE`       | `adbc.snowflake.sql.client_option.keep_session_alive`             | keepalive against session expiry      |
|  [25]   | `OCSP_FAIL_OPEN_MODE`      | `adbc.snowflake.sql.client_option.ocsp_fail_open_mode`            | trusted-environment fail-open only    |
|  [26]   | `SSL_SKIP_VERIFY`          | `adbc.snowflake.sql.client_option.tls_skip_verify`                | trusted-environment bypass only       |
|  [27]   | `DISABLE_TELEMETRY`        | `adbc.snowflake.sql.client_option.disable_telemetry`              | disable in-band client telemetry      |
|  [28]   | `USE_HIGH_PRECISION`       | `adbc.snowflake.sql.client_option.use_high_precision`             | exact `NUMBER` as Arrow `Decimal128`  |

[ENTRYPOINT_SCOPE]: `StatementOptions` keys
- rail: partition
- Every key is prefixed `StatementOptions.` and owns the result-chunk prefetch queue, the query tag, and the staged-ingest concurrency axes; `RESULT_QUEUE_SIZE` shares the transport-neutral `adbc.rpc.result_queue_size` key with the other ADBC drivers.

| [INDEX] | [MEMBER]                    | [VALUE]                                              | [CAPABILITY]                                 |
| :-----: | :-------------------------- | :--------------------------------------------------- | :------------------------------------------- |
|  [01]   | `RESULT_QUEUE_SIZE`         | `adbc.rpc.result_queue_size`                         | Arrow batches queued ahead per reader        |
|  [02]   | `PREFETCH_CONCURRENCY`      | `adbc.snowflake.rpc.prefetch_concurrency`            | parallel result-chunk downloads (default 10) |
|  [03]   | `QUERY_TAG`                 | `adbc.snowflake.statement.query_tag`                 | Snowflake `QUERY_TAG` session parameter      |
|  [04]   | `INGEST_WRITER_CONCURRENCY` | `adbc.snowflake.statement.ingest_writer_concurrency` | Parquet writer threads for bulk ingest       |
|  [05]   | `INGEST_UPLOAD_CONCURRENCY` | `adbc.snowflake.statement.ingest_upload_concurrency` | stage-upload threads for bulk ingest         |
|  [06]   | `INGEST_COPY_CONCURRENCY`   | `adbc.snowflake.statement.ingest_copy_concurrency`   | `COPY INTO` parallelism for bulk ingest      |
|  [07]   | `INGEST_TARGET_FILE_SIZE`   | `adbc.snowflake.statement.ingest_target_file_size`   | target staged-file byte size per split       |

[ENTRYPOINT_SCOPE]: `AuthType` values
- rail: partition

Each value keys `DatabaseOptions.AUTH_TYPE` and gates which credential rows apply; `AuthType` subclasses `str`, so the enum member passes directly as the option value.

| [INDEX] | [MEMBER]                    | [VALUE]            | [CAPABILITY]                               |
| :-----: | :-------------------------- | :----------------- | :----------------------------------------- |
|  [01]   | `AuthType.SNOWFLAKE`        | `auth_snowflake`   | username/password default flow             |
|  [02]   | `AuthType.OAUTH`            | `auth_oauth`       | OAuth bearer via `AUTH_TOKEN`              |
|  [03]   | `AuthType.EXTERNAL_BROWSER` | `auth_ext_browser` | browser-based SSO                          |
|  [04]   | `AuthType.OKTA`             | `auth_okta`        | native Okta via `AUTH_OKTA_URL`            |
|  [05]   | `AuthType.JWT`              | `auth_jwt`         | key-pair JWT via `JWT_PRIVATE_KEY*`        |
|  [06]   | `AuthType.MFA`              | `auth_mfa`         | password with multi-factor prompt          |
|  [07]   | `AuthType.PAT`              | `auth_pat`         | programmatic access token via `AUTH_TOKEN` |
|  [08]   | `AuthType.WIF`              | `auth_wif`         | workload-identity federation               |

## [04]-[IMPLEMENTATION_LAW]

[PARTITION_SNOWFLAKE]:
- import: `import adbc_driver_snowflake` (and `adbc_driver_snowflake.dbapi`) at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `connect` owns account binding to the native `libadbc_driver_snowflake.so`; `dbapi.connect` is the DBAPI row that adds `conn_kwargs`, never a parallel client class — the database object is shared and connections are derived from it.
- option axis: `DatabaseOptions`/`StatementOptions` enum values are the canonical `adbc.snowflake.*` keys; settings flow as `db_kwargs`/statement-option dictionaries keyed by enum value, never as ad hoc string literals or a per-setting builder type; account routing (warehouse, role, database, schema) sets session context at database scope.
- auth axis: `AUTH_TYPE` selects an `AuthType` value; password and MFA use the account's login rows, key-pair JWT keys the private-key rows, OAuth and PAT carry `AUTH_TOKEN`, Okta keys `AUTH_OKTA_URL`, and workload-identity keys `CLIENT_IDENTITY_PROVIDER` — one enum discriminates the flow, never a per-mode connect variant; credential material stays runtime-owned.
- transport security axis: production policy refuses `SSL_SKIP_VERIFY` and `OCSP_FAIL_OPEN_MODE`; a declared trusted-environment profile is the sole admission row for either bypass, and connection evidence records the admitted key.
- partition axis: `REMOTE_PARTITION_DEEPEN` runs `Cursor.adbc_execute_partitions` to receive the Snowflake result-chunk descriptors, then opens each with `adbc_read_partition` as an independent `RecordBatchReader`; the driver downloads chunks from cloud staging concurrently under `StatementOptions.PREFETCH_CONCURRENCY`, and `RESULT_QUEUE_SIZE` tunes per-reader read-ahead — chunk retrieval is the native driver's, never a hand-stitched REST download loop.
- ingest axis: bulk load runs `Cursor.adbc_ingest` writing Arrow to internal-stage Parquet then `COPY INTO`; `INGEST_WRITER_CONCURRENCY`, `INGEST_UPLOAD_CONCURRENCY`, `INGEST_COPY_CONCURRENCY`, and `INGEST_TARGET_FILE_SIZE` tune the write/upload/copy pipeline and split size — never a hand-rolled `PUT`/`COPY` sequence.
- manager axis: the concrete driver delegates loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree), and Arrow result delivery to `adbc_driver_manager`; this catalog adds only the Snowflake option vocabulary and `AuthType` axis — never a parallel DBAPI implementation. Typed error tree and `AdbcStatusCode` mapping stay the manager's.
- arrow egress axis: each result-chunk `RecordBatchReader` exposes `__arrow_c_stream__`, so a Snowflake result feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` with zero copy; `USE_HIGH_PRECISION` keeps exact `NUMBER` columns as Arrow `Decimal128` rather than lossy float; fan chunks across workers and collapse them with one terminal `read_all`/`fetch_arrow_table`.
- precision axis: `USE_HIGH_PRECISION` governs whether `NUMBER`/`NUMERIC` land as `Decimal128` (exact) or `Float64` (compact); the impact and quantity planes take the high-precision row so carbon and quantity sums never drift on a float round-trip.
- telemetry axis: the Go driver embeds its own OTel tracer configured through the standard `OTEL_TRACES_EXPORTER`/`OTEL_EXPORTER_OTLP_*`/`OTEL_SERVICE_NAME` env family, and `DISABLE_TELEMETRY` suppresses Snowflake's in-band client telemetry; the raw option `adbc.telemetry.trace_parent` — spelled by no Python enum, verified against the shipped `libadbc_driver_snowflake.so` — accepts a W3C `traceparent` so driver spans join the caller's trace; no Python-side instrumentor covers this driver.
- evidence: each connection captures the resolved account, warehouse/role/database/schema, auth type, applied option keys, result-chunk count, per-chunk batch count, and Arrow schema as a partition receipt.
- boundary: the driver owns the Snowflake session transport, option application, result-chunk retrieval, and staged ingest, emitting Arrow record batches consumed by the data partition owner; result-set materialization and dataframe conversion route to `pyarrow`/`polars`, and credential identity minting stays with the runtime owner.

[RAIL_LAW]:
- Package: `adbc-driver-snowflake`
- Owns: Snowflake account binding, concurrent Arrow result-chunk retrieval, staged bulk Arrow ingest, high-precision decimal control, per-session warehouse/role/database/schema routing, query tagging, and authentication selection
- Accept: remote Snowflake partition deepening feeding Arrow record batches to the data partition, query, and dataframe owners, and staged Arrow bulk ingest to warehouse tables
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a hand-rolled Snowflake REST client, chunk downloader, or `PUT`/`COPY` ingest loop the native driver owns; a per-setting builder type or per-auth-mode connect variant where an option enum value already keys the value; string-literal option keys bypassing `DatabaseOptions`/`StatementOptions`; credential identity minting the runtime owner owns
