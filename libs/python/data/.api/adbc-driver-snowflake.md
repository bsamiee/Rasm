# [PY_DATA_API_ADBC_DRIVER_SNOWFLAKE]

`adbc-driver-snowflake` binds a Snowflake warehouse to the data partition rail: one `connect` factory loads the native `libadbc_driver_snowflake.so` into an `AdbcDatabase`, and typed `enum.Enum` vocabularies key every setting by its canonical `adbc.snowflake.*` string. Consumption rides `dbapi.connect` on the `REMOTE_PARTITION_DEEPEN` path — Snowflake stages each result set as Arrow chunks in cloud storage, fetched concurrently under `PREFETCH_CONCURRENCY` and delivered zero-copy to the partition, query, and dataframe owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-snowflake`
- package: `adbc-driver-snowflake` (Apache-2.0)
- module: `adbc_driver_snowflake`
- owner: `data`
- rail: partition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: option and credential vocabularies

`DatabaseOptions` and `StatementOptions` members carry the canonical `adbc.snowflake.*` setting strings; `AuthType` subclasses `str`, so a member passes directly as the `AUTH_TYPE` value.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `DatabaseOptions`  | enum          | account- and connection-scoped `adbc.snowflake.*` keys |
|  [02]   | `StatementOptions` | enum          | statement keys, including prefetch and ingest axes     |
|  [03]   | `AuthType`         | enum          | `str` credential-mode values for `AUTH_TYPE`           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `connect(uri, db_kwargs) -> AdbcDatabase`                            | factory | low-level Snowflake database handle |
|  [02]   | `dbapi.connect(uri, db_kwargs, conn_kwargs, **kwargs) -> Connection` | factory | DBAPI connection over Snowflake     |

[ENTRYPOINT_SCOPE]: `DatabaseOptions` keys

Timeouts are duration strings; `AUTH_TYPE` gates which credential rows apply.

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

`RESULT_QUEUE_SIZE` carries the transport-neutral `adbc.rpc.result_queue_size` key shared with the other ADBC drivers.

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

[TOPOLOGY]:
- one `connect` binds the account to the native `libadbc_driver_snowflake.so`; `dbapi.connect` is the DBAPI row adding `conn_kwargs` over the shared database object, never a parallel client class.
- `DatabaseOptions`/`StatementOptions` values are the canonical `adbc.snowflake.*` keys carried as `db_kwargs`/statement dictionaries, never string literals or a per-setting builder type; account routing sets warehouse, role, database, and schema at session scope.
- `AUTH_TYPE` selects one `AuthType` value discriminating the credential flow, never a per-mode connect variant; credential material stays runtime-owned.

[STACKING]:
- `adbc-driver-manager`(`.api/adbc-driver-manager.md`): delegates driver loading, the DBAPI tree (`Connection`/`Cursor`/`Error`, `AdbcStatusCode` mapping), and Arrow delivery; this catalog adds only the Snowflake option vocabulary and `AuthType`, and inherits the manager's ADBC Go-driver OTel telemetry contract.
- `pyarrow`(`.api/pyarrow.md`) / `arro3-core`(`.api/arro3-core.md`) / `polars`(`.api/polars.md`): each result-chunk `RecordBatchReader.__arrow_c_stream__` feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` zero-copy; fan chunks across workers and collapse with one terminal `fetch_arrow_table`/`read_all`.
- partition deepen: `REMOTE_PARTITION_DEEPEN` runs `Cursor.adbc_execute_partitions` for the chunk descriptors, opens each with `adbc_read_partition` as an independent `RecordBatchReader`, and downloads from cloud staging concurrently under `PREFETCH_CONCURRENCY` with `RESULT_QUEUE_SIZE` read-ahead.
- staged ingest: `Cursor.adbc_ingest` writes Arrow to internal-stage Parquet then `COPY INTO`, tuned by `INGEST_WRITER_CONCURRENCY`, `INGEST_UPLOAD_CONCURRENCY`, `INGEST_COPY_CONCURRENCY`, and `INGEST_TARGET_FILE_SIZE`.

[LOCAL_ADMISSION]:
- import `adbc_driver_snowflake` and `adbc_driver_snowflake.dbapi` at boundary scope only.
- `USE_HIGH_PRECISION` lands `NUMBER`/`NUMERIC` as Arrow `Decimal128` rather than `Float64`; the impact and quantity planes take it so carbon and quantity sums never drift on a float round-trip.
- production refuses `SSL_SKIP_VERIFY` and `OCSP_FAIL_OPEN_MODE`; a declared trusted-environment profile is the sole bypass admission, and connection evidence records the admitted key.
- `DISABLE_TELEMETRY` suppresses Snowflake in-band client telemetry.
- each connection captures the resolved account, warehouse/role/database/schema, auth type, applied option keys, result-chunk count, per-chunk batch count, and Arrow schema as a partition receipt.

[RAIL_LAW]:
- Package: `adbc-driver-snowflake`
- Owns: Snowflake account binding, concurrent Arrow result-chunk retrieval, staged bulk Arrow ingest, high-precision decimal control, per-session warehouse/role/database/schema routing, query tagging, and authentication selection
- Accept: remote Snowflake partition deepening feeding Arrow record batches to the data partition, query, and dataframe owners, and staged Arrow bulk ingest to warehouse tables
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a hand-rolled Snowflake REST client, chunk downloader, or `PUT`/`COPY` ingest loop the native driver owns; a per-setting builder type or per-auth-mode connect variant where an option enum value already keys the value; string-literal option keys bypassing `DatabaseOptions`/`StatementOptions`; credential identity minting the runtime owner owns
