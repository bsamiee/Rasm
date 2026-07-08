# [RASM_PERSISTENCE_API_ADBC_BIGQUERY]

`Apache.Arrow.Adbc.Drivers.BigQuery` supplies the concrete Google BigQuery ADBC driver — `BigQueryDriver`
(`AdbcDriver`), `BigQueryDatabase` (`AdbcDatabase`), and `BigQueryConnection` (`AdbcConnection`) — that
opens a BigQuery warehouse from an `IReadOnlyDictionary<string,string>` parameter map and returns query
results as Arrow `RecordBatch` streams over the `BigQuery Storage Read API`. It is a CONCRETE
implementation of the `Apache.Arrow.Adbc` abstraction: the driver-selection surface, the
`adbc.bigquery.*` connection-string vocabulary, the OAuth / service-account / Entra-ID
(`Azure AD` workload-identity-federation) auth posture, the token-refresh callback, and the
integration seams live here; `api-arrow.md` owns the base query/metadata/result-stream contract.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc.Drivers.BigQuery`
- package: `Apache.Arrow.Adbc.Drivers.BigQuery`
- license: Apache-2.0 (`licenses.nuget.org/Apache-2.0`)
- assembly: `Apache.Arrow.Adbc.Drivers.BigQuery`
- namespace: `Apache.Arrow.Adbc.Drivers.BigQuery`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset (over `Google.Cloud.BigQuery.V2` / `Google.Apis.Auth`). Multi-TFM `net8.0` / `netstandard2.0` / `net472`; the consumer `net10.0` binds the highest asset `lib/net8.0` — no `net10.0`/`net9.0` asset ships, so `net8.0` is the consumed surface. The public surface is `BigQueryDriver` / `BigQueryDatabase` / `BigQueryConnection`; the `BigQueryParameters` / `BigQueryConstants` key holders are `internal` but their `const string` values ARE the connection contract (they key the `Open` dictionary) — the wire vocabulary.
- rail: query egress
- ABI floor: `Apache.Arrow.Adbc` is a PRE-1.0 contract over the abstract `AdbcDriver`/`AdbcConnection` members this driver overrides. `BigQueryConnection` extends the driver framework's `TracingConnection` (a `System.Diagnostics.ActivitySource` integration) and implements `ITokenProtectedResource` (an `internal` interface) — the `internal` Thrift/tracing infrastructure is not consumer surface. The `Interop.Snowflake`/`Interop.FlightSql` siblings are REJECTED (no `osx-arm64` native asset).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: driver entrypoints (concrete `Apache.Arrow.Adbc`)
- rail: query egress

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]   | [CAPABILITY]                                                                       |
| :-----: | :-------------------- | :--------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `BigQueryDriver`      | `AdbcDriver`     | `.Open(parameters)` -> BigQuery `AdbcDatabase`                                      |
|  [02]   | `BigQueryDatabase`    | `AdbcDatabase`   | `.Connect(options)` -> `BigQueryConnection`                                         |
|  [03]   | `BigQueryConnection`  | `AdbcConnection` | `TracingConnection` + `ITokenProtectedResource`; owns metadata + statement creation + token refresh |

[PUBLIC_TYPE_SCOPE]: connection-string parameter vocabulary (`internal` key holders, public contract)
- rail: query egress

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE] | [CAPABILITY]                                                                          |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `BigQueryParameters`| key holder     | the `adbc.bigquery.*` `const string` keys that populate the `Open` parameter map      |
|  [02]   | `BigQueryConstants` | value holder   | the auth-type discriminants (`user`/`aad`/`service`), Entra/STS token-exchange endpoints (`EntraStsTokenEndpoint`, `EntraGrantType`, `EntraSubjectTokenType`, `EntraRequestedTokenType`, `EntraIdScope`), default-location (`US`) / temp-dataset (`_bqadbc_temp_tables`) / public-project (`bigquery-public-data`) literals, and the `DetectProjectId` (`*detect-project-id*`) project-autodetect sentinel |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: driver open + connection surface
- rail: query egress

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]     | [CAPABILITY]                                                                |
| :-----: | :-------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `BigQueryDriver.Open(IReadOnlyDictionary<string,string>)`       | factory call     | constructs the BigQuery `AdbcDatabase` from the parameter map               |
|  [02]   | `BigQueryConnection(IReadOnlyDictionary<string,string>)`        | constructor      | the configured connection (auth, project, billing, location resolved here) |
|  [03]   | `BigQueryConnection.SetOption(string key, string value)`        | mutator          | post-open option override keyed by a `BigQueryParameters` constant          |
|  [04]   | `BigQueryConnection.UpdateToken { get; set; }` (`Func<Task>?`)  | callback property| the token-refresh hook the driver invokes when an access token expires       |
|  [05]   | `BigQueryConnection.TokenRequiresUpdate(Exception ex)`          | predicate        | tests whether a faulted call is an auth-expiry that `UpdateToken` should heal |
|  [06]   | `BigQueryConnection.CreateStatement()`                          | factory call     | the `AdbcStatement` for SQL execution (base-contract result stream)         |
|  [07]   | `BigQueryConnection.GetObjects(GetObjectsDepth, catalog?, dbSchema?, tableName?, tableTypes?, columnName?)` | metadata call | the hierarchical catalog/schema/table/column metadata stream (`IArrowArrayStream`) |
|  [08]   | `BigQueryConnection.GetTableSchema(catalog?, dbSchema?, tableName)` | metadata call | the Arrow `Schema` of a single table                                       |
|  [09]   | `BigQueryConnection.GetInfo(IReadOnlyList<AdbcInfoCode>)` / `GetTableTypes()` | metadata call | driver-info + table-type-roster Arrow streams                              |

[ENTRYPOINT_SCOPE]: connection-string parameter keys (`BigQueryParameters`)
- rail: query egress

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]  | [CAPABILITY]                                                                |
| :-----: | :-------------------------------------------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `ProjectId` / `BillingProjectId` / `DefaultClientLocation`      | parameter key | the query project (set `BigQueryConstants.DetectProjectId` to auto-detect from credentials), the billing project, and the job location (`US` default) |
|  [02]   | `AuthenticationType` (`BigQueryConstants.UserAuthenticationType`/`EntraIdAuthenticationType`/`ServiceAccountAuthenticationType`) | parameter key | the `user` / `aad` / `service` auth-flow discriminant |
|  [03]   | `JsonCredential` / `ClientId` / `ClientSecret` / `RefreshToken` / `AccessToken` | parameter key | service-account JSON, OAuth client app, refresh + bearer tokens |
|  [04]   | `Scopes` / `AudienceUri`                                        | parameter key | OAuth scope list + the Entra-ID workload-identity-federation audience       |
|  [05]   | `EvaluationKind` / `StatementType` / `StatementIndex`           | parameter key | the multi-statement script evaluation policy + per-statement selector       |
|  [06]   | `UseLegacySQL` / `AllowLargeResults` / `LargeResultsDataset` / `LargeResultsDestinationTable` | parameter key | SQL dialect + large-result spill destination |
|  [07]   | `LargeDecimalsAsString` / `MaxFetchConcurrency` / `ClientTimeout` / `GetQueryResultsOptionsTimeout` | parameter key | BIGNUMERIC-as-string policy, Storage-Read parallelism, request timeouts |
|  [08]   | `MaximumRetryAttempts` / `RetryDelayMs`                         | parameter key | the driver-internal retry budget over transient BigQuery faults             |
|  [09]   | `IncludeConstraintsWithGetObjects` / `IncludePublicProjectId`   | parameter key | metadata-pull policy (FK constraints, `bigquery-public-data` visibility)     |

[ENTRYPOINT_SCOPE]: inherited base surface (`api-arrow.md`)
- rail: query egress

SQL execution is the base `Apache.Arrow.Adbc` surface: `AdbcStatement.SqlQuery` set + `ExecuteQuery()` ->
`QueryResult.Stream` (`IArrowArrayStream`); parameter binding + `ExecuteUpdate` for DML; transaction
properties on `AdbcConnection`. The driver overrides those abstract members; `api-arrow.md` owns that
contract.

## [04]-[IMPLEMENTATION_LAW]

[DRIVER_ALGEBRA]:
- driver root: `BigQueryDriver` -> `BigQueryDatabase` -> `BigQueryConnection`
- config root: the `IReadOnlyDictionary<string,string>` parameter map keyed by `BigQueryParameters` constants
- auth discriminant root: `BigQueryConstants.{UserAuthenticationType, EntraIdAuthenticationType, ServiceAccountAuthenticationType}` mapped onto `BigQueryParameters.AuthenticationType`
- token-refresh root: `BigQueryConnection.UpdateToken` (`Func<Task>?`) + `TokenRequiresUpdate(Exception)`
- result root: Arrow `RecordBatch` over `IArrowArrayStream` (the base `Apache.Arrow.Adbc` contract)
- tracing root: `BigQueryConnection : TracingConnection` (an `ActivitySource` integration)

[PARAMETER_CONTRACT]:
- There is NO typed options class — the `adbc.bigquery.*` parameter dictionary IS the API. A consumer maps its
  own typed config record onto the `BigQueryParameters` keys (which are `internal` but stable wire strings), never a strongly-typed object.
- Auth is a three-flow discriminant: `service` (a `JsonCredential` service-account key), `user` (an OAuth
  `ClientId`/`ClientSecret`/`RefreshToken` triple), and `aad` (Entra-ID workload-identity-federation through the
  `BigQueryConstants` STS token-exchange endpoints with an `AudienceUri`).
- Token expiry is HEALED, not failed: the driver calls `UpdateToken` (a `Func<Task>?` the consumer sets) when
  `TokenRequiresUpdate(ex)` is true — the boundary wires this to its KMS/KeyVault credential refresh, never a re-open.

[LOCAL_ADMISSION]:
- The driver is admitted ONLY through the Persistence Query-federation boundary that owns the parameter-map
  construction; the `adbc.bigquery.*` key strings and credential material never leak into an interior signature.
- The `UpdateToken` callback is the credential-refresh seam: it is set at the boundary to the redacted-credential
  source, so a token rotation never re-opens the connection and never logs the token.
- Result `RecordBatch` streams are consumed through the base-package `IArrowArrayStream` and projected to the
  canonical Arrow owner — the driver is a SOURCE adapter, not a data model.

[STACKING]:
- base-abstraction seam: the driver IS the concrete `AdbcDriver` for the `Apache.Arrow.Adbc` abstraction in
  `api-arrow.md`. The `Query/federation#FEDERATED_PLAN` rail selects `BigQueryDriver` by backend, opens it with a parameter map,
  and reads results through the base `QueryResult.Stream` `IArrowArrayStream` — the SAME egress shape as the
  Spark/Hive/Impala drivers (`api-adbc-apache.md`) and distinct from the in-process DuckDB path (`api-duckdb.md`).
- arrow-result seam: BigQuery results arrive as Arrow `RecordBatch`es over the Storage Read API, so a warehouse
  query result and an in-process Arrow batch are the SAME `Apache.Arrow` type (`api-arrow.md`) — directly
  writable to Parquet (`api-parquetsharp.md`) or a Delta table (`api-deltalake.md`) with zero re-materialization.
- credential seam: the auth parameter set (`JsonCredential`, `ClientSecret`, `RefreshToken`) and the `UpdateToken`
  refresh callback compose with the KMS/KeyVault/GCP-KMS credential owners (`api-aws-kms.md`, `api-azure-keyvault.md`,
  `api-google-kms.md`) and the redaction owner (`api-redaction.md`) — credentials flow from the secret store into
  the parameter map and the refresh callback, never inline literals, and never reach a log sink.
- tracing seam: `BigQueryConnection : TracingConnection` emits `ActivitySource` spans, so a federated BigQuery
  query nests under the same OpenTelemetry trace the Npgsql/OTel owner (`api-npgsql-otel.md`) opens — one
  distributed trace spans the in-PG path and the warehouse egress.

[RAIL_LAW]:
- Package: `Apache.Arrow.Adbc.Drivers.BigQuery` `0.23.0` (Apache-2.0, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 ADBC contract)
- Owns: the concrete BigQuery ADBC driver (`BigQueryDriver`/`BigQueryDatabase`/`BigQueryConnection`), the
  `adbc.bigquery.*` connection-string vocabulary, the OAuth/service-account/Entra-ID auth posture, the
  `UpdateToken` refresh callback, and the `TracingConnection` span integration
- Accept: a BigQuery warehouse opened through the Query-federation boundary, configured by the typed parameter
  map with credentials sourced from the secret store, returning Arrow `RecordBatch` streams over the base contract
- Reject: an `adbc.bigquery.*` key or credential string in an interior signature; a token re-open where
  `UpdateToken` heals; a `RecordBatch` re-materialization away from the Arrow owner; an `Interop.*` driver (no `osx-arm64` native asset)
