# [RASM_PERSISTENCE_API_ADBC_BIGQUERY]

`Apache.Arrow.Adbc.Drivers.BigQuery` mints the concrete Google BigQuery ADBC driver — `BigQueryDriver`, `BigQueryDatabase`, and `BigQueryConnection` — opening a warehouse from an `IReadOnlyDictionary<string,string>` parameter map and returning Arrow `RecordBatch` streams over the BigQuery Storage Read API. This driver owns the `adbc.bigquery.*` connection vocabulary, the OAuth / service-account / Entra-ID auth discriminant, and the `UpdateToken` callback that heals token expiry without re-opening; the base query, metadata, and result-stream contract rides `api-arrow.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc.Drivers.BigQuery`
- package: `Apache.Arrow.Adbc.Drivers.BigQuery` (Apache-2.0)
- assembly: `Apache.Arrow.Adbc.Drivers.BigQuery`
- namespace: `Apache.Arrow.Adbc.Drivers.BigQuery`
- asset: pure-managed AnyCPU runtime library, no native RID asset; the `net10.0` consumer binds `lib/net8.0`, the highest shipped TFM
- rail: query egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: driver entrypoints (concrete `Apache.Arrow.Adbc`)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `BigQueryDriver`     | `AdbcDriver`     | `.Open(parameters)` -> BigQuery `AdbcDatabase`                                      |
|  [02]   | `BigQueryDatabase`   | `AdbcDatabase`   | `.Connect(options)` -> `BigQueryConnection`                                         |
|  [03]   | `BigQueryConnection` | `AdbcConnection` | `TracingConnection`+`ITokenProtectedResource`; metadata + statement + token refresh |

[PUBLIC_TYPE_SCOPE]: connection-string parameter vocabulary (`internal` key holders, public contract)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BigQueryParameters` | key holder    | the `adbc.bigquery.*` `const string` keys populating the `Open` map                 |
|  [02]   | `BigQueryConstants`  | value holder  | the value vocabulary — auth / Entra-STS / default literals / autodetect ([01]-[04]) |

- [01]-[AUTH]: `user`/`aad`/`service` auth-type discriminants.
- [02]-[ENTRA_STS]: `EntraStsTokenEndpoint`/`EntraGrantType`/`EntraSubjectTokenType`/`EntraRequestedTokenType`/`EntraIdScope` token-exchange endpoints.
- [03]-[LITERALS]: default-location `US`, temp-dataset `_bqadbc_temp_tables`, public-project `bigquery-public-data`.
- [04]-[AUTODETECT]: `DetectProjectId` (`*detect-project-id*`) project-autodetect sentinel.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: driver open + connection surface

`BigQueryDriver.Open` mints the `AdbcDatabase`; every surface below is a `BigQueryConnection` member, the constructor first, then instance members.

| [INDEX] | [SURFACE]                                                                                | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `BigQueryDriver.Open(IReadOnlyDictionary<string,string>)`                                | builds the BigQuery `AdbcDatabase`          |
|  [02]   | `new BigQueryConnection(IReadOnlyDictionary<string,string>)`                             | auth/project/billing/location resolved      |
|  [03]   | `SetOption(string key, string value)`                                                    | post-open `BigQueryParameters` override     |
|  [04]   | `UpdateToken { get; set; }` (`Func<Task>?`)                                              | token-refresh hook invoked on token expiry  |
|  [05]   | `TokenRequiresUpdate(Exception ex)`                                                      | tests whether a fault is an auth-expiry     |
|  [06]   | `CreateStatement()`                                                                      | the `AdbcStatement` for SQL execution       |
|  [07]   | `GetObjects(GetObjectsDepth, catalog?, dbSchema?, tableName?, tableTypes?, columnName?)` | hierarchical metadata (`IArrowArrayStream`) |
|  [08]   | `GetTableSchema(catalog?, dbSchema?, tableName)`                                         | the Arrow `Schema` of one table             |
|  [09]   | `GetInfo(IReadOnlyList<AdbcInfoCode>)` / `GetTableTypes()`                               | driver-info + table-type Arrow streams      |

[ENTRYPOINT_SCOPE]: connection-string parameter keys (`BigQueryParameters`)

`AuthenticationType` takes a `BigQueryConstants` discriminant; `BigQueryConstants.DetectProjectId` on `ProjectId` auto-detects the project from credentials.

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `ProjectId`/`BillingProjectId`/`DefaultClientLocation`                                        | query/billing project + `US` location    |
|  [02]   | `AuthenticationType`                                                                          | `user`/`aad`/`service` auth discriminant |
|  [03]   | `JsonCredential`/`ClientId`/`ClientSecret`/`RefreshToken`/`AccessToken`                       | service-account JSON, OAuth, tokens      |
|  [04]   | `Scopes`/`AudienceUri`                                                                        | OAuth scopes + Entra-ID WIF audience     |
|  [05]   | `EvaluationKind`/`StatementType`/`StatementIndex`                                             | multi-statement eval + selector          |
|  [06]   | `UseLegacySQL`/`AllowLargeResults`/`LargeResultsDataset`/`LargeResultsDestinationTable`       | SQL dialect + large-result spill         |
|  [07]   | `LargeDecimalsAsString`/`MaxFetchConcurrency`/`ClientTimeout`/`GetQueryResultsOptionsTimeout` | BIGNUMERIC-as-string, parallelism        |
|  [08]   | `MaximumRetryAttempts`/`RetryDelayMs`                                                         | retry budget over transient faults       |
|  [09]   | `IncludeConstraintsWithGetObjects`/`IncludePublicProjectId`                                   | metadata policy (FK, public-data)        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- driver chain: `BigQueryDriver` -> `BigQueryDatabase` -> `BigQueryConnection`, opened from the `IReadOnlyDictionary<string,string>` map keyed by `BigQueryParameters` constants — the `adbc.bigquery.*` dictionary IS the API, no typed options class
- auth discriminant: `BigQueryConstants.{UserAuthenticationType, EntraIdAuthenticationType, ServiceAccountAuthenticationType}` on `BigQueryParameters.AuthenticationType` selects `service` (a `JsonCredential` key), `user` (an OAuth `ClientId`/`ClientSecret`/`RefreshToken` triple), or `aad` (Entra-ID workload-identity-federation over the `BigQueryConstants` STS endpoints with an `AudienceUri`)
- token refresh: expiry heals through `UpdateToken` (`Func<Task>?`) when `TokenRequiresUpdate(Exception)` is true, never a re-open
- result: Arrow `RecordBatch` over `IArrowArrayStream`, the base `Apache.Arrow.Adbc` contract
- tracing: `BigQueryConnection : TracingConnection`, an `ActivitySource` integration

[STACKING]:
- `api-arrow.md`: `BigQueryDriver` IS the concrete `AdbcDriver` for the `Apache.Arrow.Adbc` abstraction — the federation rail selects it by backend, opens it with a parameter map, and reads results through the base `QueryResult.Stream` `IArrowArrayStream`, the same egress shape as the Spark/Hive/Impala drivers (`api-adbc-apache.md`) and distinct from in-process DuckDB (`api-duckdb.md`)
- `api-arrow.md`: BigQuery results arrive as Arrow `RecordBatch` over the Storage Read API, so a warehouse result and an in-process batch are one `Apache.Arrow` type — directly writable to Parquet (`api-parquetsharp.md`) or a Delta table (`api-deltalake.md`) with zero re-materialization
- `api-aws-kms.md`/`api-azure-keyvault.md`/`api-google-kms.md` + `api-redaction.md`: the auth parameter set (`JsonCredential`, `ClientSecret`, `RefreshToken`) and the `UpdateToken` refresh callback draw credentials from the secret store into the parameter map and refresh hook, never inline literals and never a log sink
- `api-npgsql-opentelemetry.md`: `BigQueryConnection : TracingConnection` emits `ActivitySource` spans, so a federated BigQuery query nests under the same OpenTelemetry trace the in-PG path opens

[LOCAL_ADMISSION]:
- this driver admits ONLY through the Persistence Query-federation boundary that owns parameter-map construction; its `adbc.bigquery.*` key strings and credential material never leak into an interior signature
- `UpdateToken` is the credential-refresh seam, set at the boundary to the redacted-credential source, so a token rotation never re-opens the connection and never logs the token
- result `RecordBatch` streams consume through the base `IArrowArrayStream` and project to the canonical Arrow owner — the driver is a SOURCE adapter, never a data model

[RAIL_LAW]:
- Package: `Apache.Arrow.Adbc.Drivers.BigQuery`
- Owns: the concrete BigQuery ADBC driver (`BigQueryDriver`/`BigQueryDatabase`/`BigQueryConnection`), the `adbc.bigquery.*` connection vocabulary, the OAuth/service-account/Entra-ID auth posture, the `UpdateToken` refresh callback, and the `TracingConnection` span integration
- Accept: a BigQuery warehouse opened through the Query-federation boundary, configured by the parameter map with credentials sourced from the secret store, returning Arrow `RecordBatch` streams over the base contract
- Reject: an `adbc.bigquery.*` key or credential string in an interior signature; a token re-open where `UpdateToken` heals; a `RecordBatch` re-materialization away from the Arrow owner; an `Interop.*` driver (no `osx-arm64` native asset)
