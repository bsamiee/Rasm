# [RASM_PERSISTENCE_API_ADBC_APACHE]

`Apache.Arrow.Adbc.Drivers.Apache` mints the concrete pure-managed Thrift-over-Arrow ADBC drivers for the HiveServer2 protocol family — Spark/Databricks, Hive, and Impala/Kudu — each a concrete `AdbcDriver` whose `Open` yields an `AdbcDatabase` over `AdbcConnection`/`AdbcStatement` returning Arrow `RecordBatch` streams. Every driver configures ONLY through the `IReadOnlyDictionary<string,string>` parameter map its `adbc.*` `const string` key holders populate — the map is the wire contract, never a typed options object.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc.Drivers.Apache`
- package: `Apache.Arrow.Adbc.Drivers.Apache` (Apache-2.0)
- assembly: `Apache.Arrow.Adbc.Drivers.Apache`
- namespace: `Apache.Arrow.Adbc.Drivers.Apache`, `.Spark`, `.Hive2`, `.Impala`
- asset: runtime library, pure-managed AnyCPU, no native RID asset (Thrift over `System.Net.Http`/`System.Net.Sockets`); multi-TFM `net8.0`/`netstandard2.0`/`net472`, the `net10.0` consumer binding `lib/net8.0`.
- rail: query egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: driver entrypoints (concrete `AdbcDriver`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                   |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `SparkDriver`          | `AdbcDriver`    | `.Open(parameters)` -> Databricks / Spark Thrift Server `AdbcDatabase`         |
|  [02]   | `HiveServer2Driver`    | `AdbcDriver`    | `.Open(parameters)` -> Apache Hive Thrift `AdbcDatabase`                       |
|  [03]   | `ImpalaDriver`         | `AdbcDriver`    | `.Open(parameters)` -> Apache Impala / Kudu Thrift `AdbcDatabase`              |
|  [04]   | `HiveServer2Exception` | `AdbcException` | typed driver fault carrying `SqlState` and `NativeError` over `AdbcStatusCode` |

[PUBLIC_TYPE_SCOPE]: connection-string key holders

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                                   |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `ApacheParameters`             | family-shared statement + metadata keys (`adbc.apache.*`, `adbc.get_metadata.*`)               |
|  [02]   | `SparkParameters`              | Spark endpoint + auth keys (`adbc.spark.*`)                                                    |
|  [03]   | `HiveServer2Parameters`        | Hive endpoint + auth keys (`adbc.hive.*`)                                                      |
|  [04]   | `ImpalaParameters`             | Impala endpoint + auth + TLS keys (`adbc.impala.*`)                                            |
|  [05]   | `HttpProxyOptions`             | `adbc.proxy_options.*` (use/host/port/ignore-list/auth/uid/pwd)                                |
|  [06]   | `HttpTlsOptions`               | `adbc.http_options.tls.*` (enabled/self-signed/hostname-mismatch/cert-path/disable-validation) |
|  [07]   | `StandardTlsOptions`           | `adbc.standard_options.tls.*` (non-HTTP transport TLS mirror)                                  |
|  [08]   | `ThriftTransportSizeConstants` | `MaxMessageSize`/`MaxFrameSize` caps (`adbc.apache.thrift.client.max.{message,frame}.size`)    |

[PUBLIC_TYPE_SCOPE]: auth / transport / conversion value holders (`static`)

`DataTypeConversionOptions` values key the per-driver `Spark`/`Hive`/`ImpalaParameters.DataTypeConv`, never `ApacheParameters`.

| [INDEX] | [SYMBOL]                            | [CAPABILITY]                                          |
| :-----: | :---------------------------------- | :---------------------------------------------------- |
|  [01]   | `SparkAuthTypeConstants`            | `none`/`username_only`/`basic`/`token`/`oauth` (auth) |
|  [02]   | `HiveServer2AuthTypeConstants`      | `none`/`username_only`/`basic` (auth)                 |
|  [03]   | `ImpalaAuthTypeConstants`           | `none`/`username_only`/`basic` (auth)                 |
|  [04]   | `SparkServerTypeConstants`          | `http`/`standard` (server-type)                       |
|  [05]   | `HiveServer2TransportTypeConstants` | `http`/`standard` (transport-type)                    |
|  [06]   | `ImpalaServerTypeConstants`         | `http`/`standard` (server-type)                       |
|  [07]   | `DataTypeConversionOptions`         | `None`=`"none"`/`Scalar`=`"scalar"`                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: driver open + parameter/statement keys

Every `*Driver.Open(IReadOnlyDictionary<string,string>)` mints the protocol `AdbcDatabase` from these keys; `Type` selects the server transport, `DataTypeConv` the decimal-as-string policy, `EscapePatternWildcards` the LIKE-escaping policy, and the `ApacheParameters` metadata keys scope a `GetObjects`/`GetTableSchema` pull through `adbc.get_metadata.target_*`.

| [INDEX] | [SURFACE]               | [CAPABILITY]                                                                              |
| :-----: | :---------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `SparkParameters`       | `HostName`/`Port`/`Path`/`Token`/`AccessToken`/`AuthType`/`Type` (endpoint + auth)        |
|  [02]   | `SparkParameters`       | `ConnectTimeoutMilliseconds`/`DataTypeConv`/`UserAgentEntry` (timeout, conversion, UA)    |
|  [03]   | `HiveServer2Parameters` | `HostName`/`Port`/`Path`/`AuthType`/`TransportType`/`ConnectTimeoutMilliseconds`          |
|  [04]   | `ImpalaParameters`      | `HostName`/`Port`/`Path`/`AuthType`/`Type`/`TLSOptions` (endpoint + TLS selector)         |
|  [05]   | `ApacheParameters`      | `PollTimeMilliseconds`/`BatchSize`/`BatchSizeStopCondition`/`QueryTimeoutSeconds` (fetch) |
|  [06]   | `ApacheParameters`      | `IsMetadataCommand`/`CatalogName`/`SchemaName`/`TableName`/`TableTypes`/`ColumnName`      |
|  [07]   | `ApacheParameters`      | `ForeignCatalogName`/`ForeignSchemaName`/`ForeignTableName`/`EscapePatternWildcards` (FK) |

[ENTRYPOINT_SCOPE]: inherited base surface (`api-arrow.md`)

`api-arrow.md` owns the base `Apache.Arrow.Adbc` result-execution contract this driver overrides — the `AdbcConnection`/`AdbcStatement` query, bind, and metadata members.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- driver root: `SparkDriver`/`HiveServer2Driver`/`ImpalaDriver` (`AdbcDriver`) `.Open` the parameter map -> `AdbcDatabase` -> `AdbcConnection`/`AdbcStatement`, one concrete driver per protocol
- config root: the `IReadOnlyDictionary<string,string>` map keyed by the `*Parameters` `const` holders, discriminated by the `*AuthTypeConstants`/`*ServerTypeConstants`/`*TransportTypeConstants` string value holders, with no typed options class
- result root: Arrow `RecordBatch` over `IArrowArrayStream` (the base `Apache.Arrow.Adbc` contract), faults raised as `HiveServer2Exception : AdbcException` carrying `SqlState` + `NativeError`

[STACKING]:
- base-abstraction seam: the driver IS the concrete `AdbcDriver` for the `Apache.Arrow.Adbc` abstraction (`api-arrow.md`); the Persistence Query-federation rail selects it by protocol, opens it with a parameter map, and reads the base `QueryResult.Stream` `IArrowArrayStream` — one egress shape across every warehouse backend, distinct from the in-process DuckDB path (`api-duckdb.md`)
- arrow-result seam: every statement returns Arrow `RecordBatch`, so a Spark/Hive/Impala result and an in-process batch are the SAME `Apache.Arrow` type (`api-arrow.md`), folding directly to Parquet (`api-parquetsharp.md`) or a Delta table (`api-deltalake.md`) with zero re-materialization
- substrait seam: the base `AdbcStatement.SubstraitPlan` executes a `FlowtideDotNet.Substrait` plan (`api-flowtide-substrait.md`) against the Thrift warehouse where the backend implements it, so one portable relational-algebra IR drives both the federation pipeline and the remote SQL endpoint
- transport-security seam: the `HttpTlsOptions`/`StandardTlsOptions`/`HttpProxyOptions` key sets compose with the deployment mTLS/proxy posture, populated from the same redacted-credential source the KMS/KeyVault owners (`api-aws-kms.md`, `api-azure-keyvault.md`) feed — a private-VPC self-signed cert sets `HttpTlsOptions.AllowSelfSigned`, never `DisableServerCertificateValidation`

[LOCAL_ADMISSION]:
- Persistence's Query-federation boundary owns parameter-map construction as the driver's sole admission path; the `adbc.*` key strings map to a typed connection record at the edge and never leak into an interior signature
- auth-type and server-type are string discriminants (`SparkAuthTypeConstants.OAuth`, `SparkServerTypeConstants.Http`), mapped at the boundary through a `[SmartEnum<string>]` so the canonical token survives projection
- a faulted statement surfaces as `HiveServer2Exception`; the boundary lifts `SqlState`/`NativeError` onto the canonical `Fin`/typed-failure rail, and no `AdbcException` crosses into domain logic
- result `RecordBatch` streams are consumed through the base `IArrowArrayStream`, then projected to the canonical Arrow owner — the driver is a SOURCE adapter, not a data model

[RAIL_LAW]:
- Package: `Apache.Arrow.Adbc.Drivers.Apache` (Apache-2.0, pure-managed AnyCPU, `net10.0` binds `net8.0`)
- Owns: the concrete HiveServer2-family ADBC drivers (`SparkDriver`/`HiveServer2Driver`/`ImpalaDriver`), their `adbc.*` connection-string parameter vocabulary, the auth/transport/TLS/proxy key sets, and the `HiveServer2Exception` error rail
- Accept: a Spark/Hive/Impala Thrift SQL warehouse opened through the Query-federation boundary, configured by the typed parameter map, returning Arrow `RecordBatch` streams over the base `Apache.Arrow.Adbc` contract
- Reject: an `adbc.*` key string in an interior signature; an `AdbcException` crossing into domain logic; a `RecordBatch` re-materialization away from the Arrow owner; an `Interop.*` driver (no `osx-arm64` native asset)
