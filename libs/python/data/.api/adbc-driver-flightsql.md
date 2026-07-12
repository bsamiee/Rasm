# [PY_DATA_API_ADBC_DRIVER_FLIGHTSQL]

`adbc-driver-flightsql` supplies the ADBC Arrow Flight SQL driver for the data partition rail: a `connect` factory that binds the native `libadbc_driver_flightsql.so` shared object to a Flight SQL endpoint as an `AdbcDatabase`, plus three `enum.Enum` option vocabularies (`DatabaseOptions`, `ConnectionOptions`, `StatementOptions`) and two OAuth axes (`OAuthFlowType`, `OAuthTokenType`) that key every database, connection, and statement setting by canonical `adbc.flight.sql.*` string. The package owner composes `dbapi.connect` into the `REMOTE_PARTITION_DEEPEN` path, feeding `execute_partitions` endpoints back as Arrow record batches over Flight RPC; it never re-implements the gRPC Flight transport or the partition handoff the native driver already owns, and it never mints a per-setting builder type where an option enum string already keys the value.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-flightsql`
- package: `adbc-driver-flightsql`
- version: `1.11.0`
- import: `adbc_driver_flightsql`
- owner: `data`
- rail: partition
- license: `Apache-2.0`
- entry points: library use is import-only; `connect` returns an `AdbcDatabase`, `dbapi.connect` returns a DBAPI 2.0 `Connection`
- capability: Flight SQL endpoint binding over gRPC, partitioned result retrieval via `execute_partitions`/`adbc_execute_partitions`, mTLS and TLS-override transport, arbitrary RPC header injection, per-call fetch/query/update timeouts, session-option get/set, OAuth 2.0 client-credentials and token-exchange (RFC 8693) authentication, Substrait plan version control, and DBAPI 2.0 cursor access yielding Arrow record batches

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory and option vocabularies
- rail: partition

`connect` is the single low-level factory; `DatabaseOptions`, `ConnectionOptions`, and `StatementOptions` are `enum.Enum` option keys whose values are the canonical `adbc.flight.sql.*` setting strings consumed by `db_kwargs`/`conn_kwargs`/statement options. `OAuthFlowType` and `OAuthTokenType` are the value vocabularies for the OAuth flow and token-exchange options on `DatabaseOptions`. The DBAPI 2.0 facade (`Connection`, `Cursor`, the typed error hierarchy) lives in the `dbapi` submodule.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                             |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `connect`           | factory       | bind a Flight SQL URI to an `AdbcDatabase`                         |
|  [02]   | `DatabaseOptions`   | option enum   | database-scoped `adbc.flight.sql.*` setting keys                   |
|  [03]   | `ConnectionOptions` | option enum   | connection-scoped setting keys, incl. session-option prefixes      |
|  [04]   | `StatementOptions`  | option enum   | statement-scoped setting keys, incl. partition queue and Substrait |
|  [05]   | `OAuthFlowType`     | value enum    | OAuth 2.0 flow values for `DatabaseOptions.OAUTH_FLOW`             |
|  [06]   | `OAuthTokenType`    | value enum    | RFC 8693 token-type URIs for OAuth token-exchange options          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories
- rail: partition

`connect` mints the low-level `AdbcDatabase` bound to the native driver path; `dbapi.connect` wraps that in a DBAPI 2.0 `Connection` and additionally accepts `conn_kwargs` because ADBC separates the shared database object from per-connection state. Both key `db_kwargs`/`conn_kwargs` by `DatabaseOptions`/`ConnectionOptions` enum values.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                             | [CAPABILITY]                         |
| :-----: | :-------------- | :----------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `connect`       | `connect(uri, db_kwargs=None) -> AdbcDatabase`                           | low-level Flight SQL database handle |
|  [02]   | `dbapi.connect` | `connect(uri, db_kwargs=None, conn_kwargs=None, **kwargs) -> Connection` | DBAPI 2.0 connection over Flight SQL |

[ENTRYPOINT_SCOPE]: `DatabaseOptions` keys
- rail: partition

Each member value is the canonical setting string passed in `db_kwargs`. Timeouts are floating-point seconds; `RPC_CALL_HEADER_PREFIX` is a key prefix suffixed with the header name; OAuth keys configure the flow selected by `OAUTH_FLOW`.

| [INDEX] | [MEMBER]                              | [VALUE]                                               | [CAPABILITY]                                |
| :-----: | :------------------------------------ | :---------------------------------------------------- | :------------------------------------------ |
|  [01]   | `AUTHORIZATION_HEADER`                | `adbc.flight.sql.authorization_header`                | authorization header for requests           |
|  [02]   | `AUTHORITY`                           | `adbc.flight.sql.client_option.authority`             | handshake server name                       |
|  [03]   | `MTLS_CERT_CHAIN`                     | `adbc.flight.sql.client_option.mtls_cert_chain`       | enable mTLS, PEM certificate chain          |
|  [04]   | `MTLS_PRIVATE_KEY`                    | `adbc.flight.sql.client_option.mtls_private_key`      | enable mTLS, PEM private key                |
|  [05]   | `RPC_CALL_HEADER_PREFIX`              | `adbc.flight.sql.rpc.call_header.`                    | per-header key prefix for outgoing requests |
|  [06]   | `TIMEOUT_FETCH`                       | `adbc.flight.sql.rpc.timeout_seconds.fetch`           | DoGet fetch timeout (seconds)               |
|  [07]   | `TIMEOUT_QUERY`                       | `adbc.flight.sql.rpc.timeout_seconds.query`           | GetFlightInfo query timeout (seconds)       |
|  [08]   | `TIMEOUT_UPDATE`                      | `adbc.flight.sql.rpc.timeout_seconds.update`          | upload/update timeout (seconds)             |
|  [09]   | `TLS_OVERRIDE_HOSTNAME`               | `adbc.flight.sql.client_option.tls_override_hostname` | TLS hostname override                       |
|  [10]   | `TLS_ROOT_CERTS`                      | `adbc.flight.sql.client_option.tls_root_certs`        | PEM root certificates for TLS               |
|  [11]   | `TLS_SKIP_VERIFY`                     | `adbc.flight.sql.client_option.tls_skip_verify`       | disable server TLS verification             |
|  [12]   | `WITH_BLOCK`                          | `adbc.flight.sql.client_option.with_block`            | block until connection established          |
|  [13]   | `WITH_COOKIE_MIDDLEWARE`              | `adbc.flight.sql.rpc.with_cookie_middleware`          | enable cookie middleware                    |
|  [14]   | `WITH_MAX_MSG_SIZE`                   | `adbc.flight.sql.client_option.with_max_msg_size`     | max gRPC message size in bytes              |
|  [15]   | `OAUTH_FLOW`                          | `adbc.flight.sql.oauth.flow`                          | OAuth flow selector (`OAuthFlowType`)       |
|  [16]   | `OAUTH_AUTH_URI`                      | `adbc.flight.sql.oauth.auth_uri`                      | OAuth authorization endpoint URL            |
|  [17]   | `OAUTH_TOKEN_URI`                     | `adbc.flight.sql.oauth.token_uri`                     | OAuth token endpoint URL                    |
|  [18]   | `OAUTH_REDIRECT_URI`                  | `adbc.flight.sql.oauth.redirect_uri`                  | OAuth redirect URI                          |
|  [19]   | `OAUTH_SCOPE`                         | `adbc.flight.sql.oauth.scope`                         | OAuth scope                                 |
|  [20]   | `OAUTH_CLIENT_ID`                     | `adbc.flight.sql.oauth.client_id`                     | OAuth client identifier                     |
|  [21]   | `OAUTH_CLIENT_SECRET`                 | `adbc.flight.sql.oauth.client_secret`                 | OAuth client secret                         |
|  [22]   | `OAUTH_EXCHANGE_SUBJECT_TOKEN`        | `adbc.flight.sql.oauth.exchange.subject_token`        | token-exchange subject token                |
|  [23]   | `OAUTH_EXCHANGE_SUBJECT_TOKEN_TYPE`   | `adbc.flight.sql.oauth.exchange.subject_token_type`   | subject token type (`OAuthTokenType`)       |
|  [24]   | `OAUTH_EXCHANGE_ACTOR_TOKEN`          | `adbc.flight.sql.oauth.exchange.actor_token`          | token-exchange actor token                  |
|  [25]   | `OAUTH_EXCHANGE_ACTOR_TOKEN_TYPE`     | `adbc.flight.sql.oauth.exchange.actor_token_type`     | actor token type (`OAuthTokenType`)         |
|  [26]   | `OAUTH_EXCHANGE_REQUESTED_TOKEN_TYPE` | `adbc.flight.sql.oauth.exchange.requested_token_type` | requested token type (`OAuthTokenType`)     |
|  [27]   | `OAUTH_EXCHANGE_SCOPE`                | `adbc.flight.sql.oauth.exchange.scope`                | token-exchange scope                        |
|  [28]   | `OAUTH_EXCHANGE_AUD`                  | `adbc.flight.sql.oauth.exchange.aud`                  | token-exchange audience                     |
|  [29]   | `OAUTH_EXCHANGE_RESOURCE`             | `adbc.flight.sql.oauth.exchange.resource`             | token-exchange resource                     |

[ENTRYPOINT_SCOPE]: `ConnectionOptions` and `StatementOptions` keys
- rail: partition

Connection options own session-option get/set and override database-scoped headers; statement options own the partition read-ahead queue, Substrait version, and per-statement timeout/header overrides. `RPC_CALL_HEADER_PREFIX`, `TIMEOUT_FETCH`, `TIMEOUT_QUERY`, and `TIMEOUT_UPDATE` alias the same `DatabaseOptions` value at narrower scope.

| [INDEX] | [MEMBER]                                                           | [VALUE]                                                    | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `ConnectionOptions.OPTION_SESSION_OPTIONS`                         | `adbc.flight.sql.session.options`                          | all session options as a JSON blob                 |
|  [02]   | `ConnectionOptions.OPTION_SESSION_OPTION_PREFIX`                   | `adbc.flight.sql.session.option.`                          | get/set one session option (key prefix)            |
|  [03]   | `ConnectionOptions.OPTION_ERASE_SESSION_OPTION_PREFIX`             | `adbc.flight.sql.session.optionerase.`                     | erase a session option (key prefix)                |
|  [04]   | `ConnectionOptions.OPTION_BOOL_SESSION_OPTION_PREFIX`              | `adbc.flight.sql.session.optionbool.`                      | get/set a boolean session option (key prefix)      |
|  [05]   | `ConnectionOptions.OPTION_STRING_LIST_SESSION_OPTION_PREFIX`       | `adbc.flight.sql.session.optionstringlist.`                | get/set a string-list session option (key prefix)  |
|  [06]   | `ConnectionOptions.RPC_CALL_HEADER_PREFIX`                         | `adbc.flight.sql.rpc.call_header.`                         | connection-scoped header prefix (overrides db)     |
|  [07]   | `ConnectionOptions.TIMEOUT_FETCH`/`TIMEOUT_QUERY`/`TIMEOUT_UPDATE` | `adbc.flight.sql.rpc.timeout_seconds.{fetch,query,update}` | connection-scoped timeout overrides (seconds)      |
|  [08]   | `StatementOptions.LAST_FLIGHT_INFO`                                | `adbc.flight.sql.statement.exec.last_flight_info`          | latest `FlightInfo` (incremental execution)        |
|  [09]   | `StatementOptions.QUEUE_SIZE`                                      | `adbc.rpc.result_queue_size`                               | batches queued per partition (default 5)           |
|  [10]   | `StatementOptions.RPC_CALL_HEADER_PREFIX`                          | `adbc.flight.sql.rpc.call_header.`                         | statement-scoped header prefix (overrides db/conn) |
|  [11]   | `StatementOptions.SUBSTRAIT_VERSION`                               | `adbc.flight.sql.substrait.version`                        | Substrait version on the Flight SQL request        |
|  [12]   | `StatementOptions.TIMEOUT_FETCH`                                   | `adbc.flight.sql.rpc.timeout_seconds.fetch`                | statement-scoped DoGet fetch timeout (seconds)     |
|  [13]   | `StatementOptions.TIMEOUT_QUERY`                                   | `adbc.flight.sql.rpc.timeout_seconds.query`                | statement-scoped query timeout (seconds)           |
|  [14]   | `StatementOptions.TIMEOUT_UPDATE`                                  | `adbc.flight.sql.rpc.timeout_seconds.update`               | statement-scoped update timeout (seconds)          |

[ENTRYPOINT_SCOPE]: OAuth value enums
- rail: partition

`OAuthFlowType` values key `DatabaseOptions.OAUTH_FLOW`; `OAuthTokenType` URIs key the token-type exchange options.

| [INDEX] | [MEMBER]                           | [VALUE]                                          | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :----------------------------------------------- | :----------------------------------- |
|  [01]   | `OAuthFlowType.CLIENT_CREDENTIALS` | `client_credentials`                             | RFC 6749 4.4 client-credentials flow |
|  [02]   | `OAuthFlowType.TOKEN_EXCHANGE`     | `token_exchange`                                 | RFC 8693 token-exchange flow         |
|  [03]   | `OAuthTokenType.ACCESS_TOKEN`      | `urn:ietf:params:oauth:token-type:access_token`  | OAuth 2.0 access token               |
|  [04]   | `OAuthTokenType.REFRESH_TOKEN`     | `urn:ietf:params:oauth:token-type:refresh_token` | OAuth 2.0 refresh token              |
|  [05]   | `OAuthTokenType.ID_TOKEN`          | `urn:ietf:params:oauth:token-type:id_token`      | OpenID Connect ID token              |
|  [06]   | `OAuthTokenType.SAML1`             | `urn:ietf:params:oauth:token-type:saml1`         | SAML 1.1 assertion                   |
|  [07]   | `OAuthTokenType.SAML2`             | `urn:ietf:params:oauth:token-type:saml2`         | SAML 2.0 assertion                   |
|  [08]   | `OAuthTokenType.JWT`               | `urn:ietf:params:oauth:token-type:jwt`           | JSON Web Token                       |

## [04]-[IMPLEMENTATION_LAW]

[PARTITION_FLIGHTSQL]:
- import: `import adbc_driver_flightsql` (and `adbc_driver_flightsql.dbapi`) at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `connect` owns endpoint binding to the native `libadbc_driver_flightsql.so`; `dbapi.connect` is the DBAPI 2.0 row that adds `conn_kwargs`, never a parallel client class — the database object is shared and connections are derived from it.
- option axis: `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum values are the canonical `adbc.flight.sql.*` keys; settings flow as `db_kwargs`/`conn_kwargs`/statement-option dictionaries keyed by enum value, never as ad hoc string literals or a per-setting builder type; connection and statement scope override database scope for shared keys.
- partition axis: `REMOTE_PARTITION_DEEPEN` runs `Cursor.adbc_execute_partitions` (or low-level `AdbcStatement.execute_partitions`) to receive Flight RPC endpoint descriptors, then opens each with `adbc_read_partition` as an independent `RecordBatchReader`; `StatementOptions.QUEUE_SIZE` tunes per-partition read-ahead and `StatementOptions.LAST_FLIGHT_INFO` inspects progress under incremental execution; partition handoff is the native driver's, never a hand-stitched gRPC loop.
- manager axis: the concrete driver delegates loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree), and Arrow result delivery to `adbc_driver_manager`; this catalog adds only the Flight SQL option vocabulary and OAuth axes — never a parallel DBAPI implementation. The typed error tree and `AdbcStatusCode` mapping are the manager's.
- arrow egress axis: each partition `RecordBatchReader` exposes `__arrow_c_stream__`, so a Flight SQL result feeds `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` with zero copy; fan partitions across workers and collapse them with one terminal `read_all`/`fetch_arrow_table`.
- transport axis: TLS, mTLS (`MTLS_CERT_CHAIN`/`MTLS_PRIVATE_KEY`), hostname override, root certs, message-size cap, cookie middleware, and per-call fetch/query/update timeouts are `DatabaseOptions` rows; arbitrary headers attach through the `RPC_CALL_HEADER_PREFIX` key prefix at the narrowest applicable scope.
- auth axis: `OAUTH_FLOW` selects an `OAuthFlowType` value; client-credentials uses the client-id/secret/scope rows while token-exchange (RFC 8693) keys subject/actor/requested token URIs by `OAuthTokenType`; the `AUTHORIZATION_HEADER` row carries a static bearer when no flow is configured.
- evidence: each connection captures the resolved URI, applied option keys, OAuth flow, partition count, per-partition batch count, and Arrow schema as a partition receipt.
- boundary: the driver owns the Flight SQL gRPC transport, option application, and partition retrieval, emitting Arrow record batches consumed by the data partition owner; result-set materialization and dataframe conversion route to `pyarrow`/`polars`, and credential identity minting stays with the runtime owner.

[RAIL_LAW]:
- Package: `adbc-driver-flightsql`
- Owns: Flight SQL endpoint binding, partitioned Arrow result retrieval, TLS/mTLS transport control, RPC header and timeout configuration, session-option get/set, and OAuth 2.0 client-credentials and token-exchange authentication
- Accept: remote Flight SQL partition deepening feeding Arrow record batches to the data partition, query, and dataframe owners
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a hand-rolled gRPC Flight client or partition loop the native driver owns; a per-setting builder type where an option enum value already keys the value; string-literal option keys bypassing `DatabaseOptions`/`ConnectionOptions`/`StatementOptions`; credential identity minting the runtime owns
