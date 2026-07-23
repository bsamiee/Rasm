# [PY_DATA_API_ADBC_DRIVER_FLIGHTSQL]

`adbc-driver-flightsql` binds an Arrow Flight SQL endpoint as an ADBC database on the data partition rail: `connect` mints the native-driver-backed `AdbcDatabase`, three option-enum vocabularies key every database, connection, and statement setting by canonical `adbc.flight.sql.*` string, and two OAuth axes drive client-credentials and RFC 8693 token-exchange auth. Partition deepening rides `dbapi.connect`, fanning `execute_partitions` endpoints back as Arrow record batches over Flight RPC.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-flightsql`
- package: `adbc-driver-flightsql` (Apache-2.0, Apache Arrow)
- module: `adbc_driver_flightsql`, `adbc_driver_flightsql.dbapi`
- native: `libadbc_driver_flightsql.so` — the Go-built ADBC driver over the C ABI
- rail: partition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: factory and option vocabularies

`DatabaseOptions`, `ConnectionOptions`, and `StatementOptions` are `enum.Enum` keys resolving to canonical `adbc.flight.sql.*` setting strings; `OAuthFlowType` and `OAuthTokenType` value the OAuth flow and token-exchange options. `dbapi` carries the DBAPI 2.0 facade — `Connection`, `Cursor`, the typed error tree.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `connect`           | factory       | bind a Flight SQL URI to an `AdbcDatabase`                         |
|  [02]   | `DatabaseOptions`   | option enum   | database-scoped `adbc.flight.sql.*` setting keys                   |
|  [03]   | `ConnectionOptions` | option enum   | connection-scoped setting keys, incl. session-option prefixes      |
|  [04]   | `StatementOptions`  | option enum   | statement-scoped setting keys, incl. partition queue and Substrait |
|  [05]   | `OAuthFlowType`     | value enum    | OAuth 2.0 flow values for `DatabaseOptions.OAUTH_FLOW`             |
|  [06]   | `OAuthTokenType`    | value enum    | RFC 8693 token-type URIs for OAuth token-exchange options          |

[PUBLIC_TYPE_SCOPE]: `DatabaseOptions` keys

Each member resolves to its `db_kwargs` setting string; `RPC_CALL_HEADER_PREFIX` suffixes the header name, timeouts are floating-point seconds, and the OAuth keys configure the flow selected by `OAUTH_FLOW`.

| [INDEX] | [MEMBER]                              | [VALUE]                                               | [CAPABILITY]                            |
| :-----: | :------------------------------------ | :---------------------------------------------------- | :-------------------------------------- |
|  [01]   | `AUTHORIZATION_HEADER`                | `adbc.flight.sql.authorization_header`                | authorization header for requests       |
|  [02]   | `AUTHORITY`                           | `adbc.flight.sql.client_option.authority`             | handshake server name                   |
|  [03]   | `MTLS_CERT_CHAIN`                     | `adbc.flight.sql.client_option.mtls_cert_chain`       | enable mTLS, PEM certificate chain      |
|  [04]   | `MTLS_PRIVATE_KEY`                    | `adbc.flight.sql.client_option.mtls_private_key`      | enable mTLS, PEM private key            |
|  [05]   | `RPC_CALL_HEADER_PREFIX`              | `adbc.flight.sql.rpc.call_header.`                    | per-request header key prefix           |
|  [06]   | `TIMEOUT_FETCH`                       | `adbc.flight.sql.rpc.timeout_seconds.fetch`           | DoGet fetch timeout (seconds)           |
|  [07]   | `TIMEOUT_QUERY`                       | `adbc.flight.sql.rpc.timeout_seconds.query`           | GetFlightInfo query timeout (seconds)   |
|  [08]   | `TIMEOUT_UPDATE`                      | `adbc.flight.sql.rpc.timeout_seconds.update`          | upload/update timeout (seconds)         |
|  [09]   | `TLS_OVERRIDE_HOSTNAME`               | `adbc.flight.sql.client_option.tls_override_hostname` | TLS hostname override                   |
|  [10]   | `TLS_ROOT_CERTS`                      | `adbc.flight.sql.client_option.tls_root_certs`        | PEM root certificates for TLS           |
|  [11]   | `TLS_SKIP_VERIFY`                     | `adbc.flight.sql.client_option.tls_skip_verify`       | disable server TLS verification         |
|  [12]   | `WITH_BLOCK`                          | `adbc.flight.sql.client_option.with_block`            | block until connection established      |
|  [13]   | `WITH_COOKIE_MIDDLEWARE`              | `adbc.flight.sql.rpc.with_cookie_middleware`          | enable cookie middleware                |
|  [14]   | `WITH_MAX_MSG_SIZE`                   | `adbc.flight.sql.client_option.with_max_msg_size`     | max gRPC message size in bytes          |
|  [15]   | `OAUTH_FLOW`                          | `adbc.flight.sql.oauth.flow`                          | OAuth flow selector (`OAuthFlowType`)   |
|  [16]   | `OAUTH_AUTH_URI`                      | `adbc.flight.sql.oauth.auth_uri`                      | OAuth authorization endpoint URL        |
|  [17]   | `OAUTH_TOKEN_URI`                     | `adbc.flight.sql.oauth.token_uri`                     | OAuth token endpoint URL                |
|  [18]   | `OAUTH_REDIRECT_URI`                  | `adbc.flight.sql.oauth.redirect_uri`                  | OAuth redirect URI                      |
|  [19]   | `OAUTH_SCOPE`                         | `adbc.flight.sql.oauth.scope`                         | OAuth scope                             |
|  [20]   | `OAUTH_CLIENT_ID`                     | `adbc.flight.sql.oauth.client_id`                     | OAuth client identifier                 |
|  [21]   | `OAUTH_CLIENT_SECRET`                 | `adbc.flight.sql.oauth.client_secret`                 | OAuth client secret                     |
|  [22]   | `OAUTH_EXCHANGE_SUBJECT_TOKEN`        | `adbc.flight.sql.oauth.exchange.subject_token`        | token-exchange subject token            |
|  [23]   | `OAUTH_EXCHANGE_SUBJECT_TOKEN_TYPE`   | `adbc.flight.sql.oauth.exchange.subject_token_type`   | subject token type (`OAuthTokenType`)   |
|  [24]   | `OAUTH_EXCHANGE_ACTOR_TOKEN`          | `adbc.flight.sql.oauth.exchange.actor_token`          | token-exchange actor token              |
|  [25]   | `OAUTH_EXCHANGE_ACTOR_TOKEN_TYPE`     | `adbc.flight.sql.oauth.exchange.actor_token_type`     | actor token type (`OAuthTokenType`)     |
|  [26]   | `OAUTH_EXCHANGE_REQUESTED_TOKEN_TYPE` | `adbc.flight.sql.oauth.exchange.requested_token_type` | requested token type (`OAuthTokenType`) |
|  [27]   | `OAUTH_EXCHANGE_SCOPE`                | `adbc.flight.sql.oauth.exchange.scope`                | token-exchange scope                    |
|  [28]   | `OAUTH_EXCHANGE_AUD`                  | `adbc.flight.sql.oauth.exchange.aud`                  | token-exchange audience                 |
|  [29]   | `OAUTH_EXCHANGE_RESOURCE`             | `adbc.flight.sql.oauth.exchange.resource`             | token-exchange resource                 |

[PUBLIC_TYPE_SCOPE]: `ConnectionOptions` keys

Session-option rows get and set through the `adbc.flight.sql.session.*` prefixes; `RPC_CALL_HEADER_PREFIX`/`TIMEOUT_*` alias the database keys and override database scope.

| [INDEX] | [MEMBER]                                   | [VALUE]                                      | [CAPABILITY]                            |
| :-----: | :----------------------------------------- | :------------------------------------------- | :-------------------------------------- |
|  [01]   | `OPTION_SESSION_OPTIONS`                   | `adbc.flight.sql.session.options`            | all session options as a JSON blob      |
|  [02]   | `OPTION_SESSION_OPTION_PREFIX`             | `adbc.flight.sql.session.option.`            | one session option (key prefix)         |
|  [03]   | `OPTION_ERASE_SESSION_OPTION_PREFIX`       | `adbc.flight.sql.session.optionerase.`       | erase a session option (key prefix)     |
|  [04]   | `OPTION_BOOL_SESSION_OPTION_PREFIX`        | `adbc.flight.sql.session.optionbool.`        | boolean session option (key prefix)     |
|  [05]   | `OPTION_STRING_LIST_SESSION_OPTION_PREFIX` | `adbc.flight.sql.session.optionstringlist.`  | string-list session option (key prefix) |
|  [06]   | `RPC_CALL_HEADER_PREFIX`                   | `adbc.flight.sql.rpc.call_header.`           | header prefix, overrides db scope       |
|  [07]   | `TIMEOUT_FETCH`                            | `adbc.flight.sql.rpc.timeout_seconds.fetch`  | fetch timeout (seconds)                 |
|  [08]   | `TIMEOUT_QUERY`                            | `adbc.flight.sql.rpc.timeout_seconds.query`  | query timeout (seconds)                 |
|  [09]   | `TIMEOUT_UPDATE`                           | `adbc.flight.sql.rpc.timeout_seconds.update` | update timeout (seconds)                |

[PUBLIC_TYPE_SCOPE]: `StatementOptions` keys

Owns the partition read-ahead queue, Substrait version, and per-statement overrides; `RPC_CALL_HEADER_PREFIX`/`TIMEOUT_*` alias the database keys and override database and connection scope.

| [INDEX] | [MEMBER]                 | [VALUE]                                           | [CAPABILITY]                                       |
| :-----: | :----------------------- | :------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `LAST_FLIGHT_INFO`       | `adbc.flight.sql.statement.exec.last_flight_info` | latest `FlightInfo` (incremental execution)        |
|  [02]   | `QUEUE_SIZE`             | `adbc.rpc.result_queue_size`                      | batches queued per partition (default 5)           |
|  [03]   | `RPC_CALL_HEADER_PREFIX` | `adbc.flight.sql.rpc.call_header.`                | statement-scoped header prefix (overrides db/conn) |
|  [04]   | `SUBSTRAIT_VERSION`      | `adbc.flight.sql.substrait.version`               | Substrait version on the Flight SQL request        |
|  [05]   | `TIMEOUT_FETCH`          | `adbc.flight.sql.rpc.timeout_seconds.fetch`       | statement-scoped DoGet fetch timeout (seconds)     |
|  [06]   | `TIMEOUT_QUERY`          | `adbc.flight.sql.rpc.timeout_seconds.query`       | statement-scoped query timeout (seconds)           |
|  [07]   | `TIMEOUT_UPDATE`         | `adbc.flight.sql.rpc.timeout_seconds.update`      | statement-scoped update timeout (seconds)          |

[PUBLIC_TYPE_SCOPE]: OAuth value enums

`OAuthFlowType` values key `DatabaseOptions.OAUTH_FLOW`; `OAuthTokenType` URIs key the token-exchange type options.

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection factories

`connect` mints the native-driver `AdbcDatabase`; `dbapi.connect` derives a DBAPI 2.0 `Connection`, adding `conn_kwargs` for per-connection state over the shared database object. Both key `db_kwargs`/`conn_kwargs` by option-enum value.

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `connect(uri, db_kwargs=None) -> AdbcDatabase`                                 | factory | low-level Flight SQL database handle |
|  [02]   | `dbapi.connect(uri, db_kwargs=None, conn_kwargs=None, **kwargs) -> Connection` | factory | DBAPI 2.0 connection over Flight SQL |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- factory: one `connect` binds the endpoint to `libadbc_driver_flightsql.so`; `dbapi.connect` derives the DBAPI 2.0 connection and adds `conn_kwargs` — the database object is shared and connections derive from it.
- option: `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` values are the canonical `adbc.flight.sql.*` keys applied as `db_kwargs`/`conn_kwargs`/statement dictionaries; connection and statement scope override database scope for shared keys.
- partition: `REMOTE_PARTITION_DEEPEN` runs `Cursor.adbc_execute_partitions` (low-level `AdbcStatement.execute_partitions`) for Flight RPC endpoint descriptors, opens each with `adbc_read_partition` as an independent `RecordBatchReader`, tunes read-ahead with `StatementOptions.QUEUE_SIZE`, and inspects incremental progress with `StatementOptions.LAST_FLIGHT_INFO`.
- transport: TLS, mTLS (`MTLS_CERT_CHAIN`/`MTLS_PRIVATE_KEY`), hostname override, root certs, message-size cap, cookie middleware, and per-call fetch/query/update timeouts are `DatabaseOptions` rows; arbitrary headers attach through `RPC_CALL_HEADER_PREFIX` at the narrowest applicable scope.
- auth: `OAUTH_FLOW` selects an `OAuthFlowType`; client-credentials keys client-id/secret/scope, token-exchange keys subject/actor/requested token URIs by `OAuthTokenType`, and `AUTHORIZATION_HEADER` carries a static bearer without a flow.
- telemetry: inherits the manager's ADBC Go-driver OTel contract (`adbc-driver-manager.md` `[04]-[IMPLEMENTATION_LAW]`); the flightsql delta is dual OTLP exporters, gRPC and HTTP.
- receipt: each connection captures the resolved URI, applied option keys, OAuth flow, partition and per-partition batch counts, and Arrow schema.

[STACKING]:
- `adbc-driver-manager`(`.api/adbc-driver-manager.md`): the concrete driver delegates loading, the DBAPI surface (`Connection`/`Cursor`/`Error` tree, `AdbcStatusCode`), and Arrow delivery to the manager; this catalog adds only the Flight SQL option vocabulary and OAuth axes.
- `arro3-core`(`.api/arro3-core.md`), `polars`: each partition `RecordBatchReader` exposes `__arrow_c_stream__`, feeding `arro3.core.RecordBatchReader.from_stream` or `polars.from_arrow` zero-copy; fan partitions across workers and collapse with one terminal `read_all`/`fetch_arrow_table`.
- data partition owner: partition endpoints fan across workers and Arrow batches collapse to the partition receipt; dataframe materialization routes to `pyarrow`/`polars` and credential minting to the runtime owner.

[LOCAL_ADMISSION]:
- Import `adbc_driver_flightsql`/`.dbapi` at boundary scope only.
- Key every setting by option-enum value through `db_kwargs`/`conn_kwargs`/statement options.
- Deepen remote partitions through the native `execute_partitions`/`adbc_read_partition` handoff.

[RAIL_LAW]:
- Package: `adbc-driver-flightsql`
- Owns: Flight SQL endpoint binding, partitioned Arrow result retrieval, TLS/mTLS transport control, RPC header and timeout configuration, session-option get/set, and OAuth 2.0 client-credentials and token-exchange authentication
- Accept: remote Flight SQL partition deepening feeding Arrow record batches to the data partition, query, and dataframe owners
- Reject: wrapper-renames of `connect`/`dbapi.connect`; a hand-rolled gRPC Flight client or partition loop the native driver owns; a per-setting builder type where an option-enum value keys the value; string-literal option keys bypassing `DatabaseOptions`/`ConnectionOptions`/`StatementOptions`; credential identity minting the runtime owns
