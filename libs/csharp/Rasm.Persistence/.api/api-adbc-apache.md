# [RASM_PERSISTENCE_API_ADBC_APACHE]

`Apache.Arrow.Adbc.Drivers.Apache` supplies the concrete Thrift-over-Arrow ADBC drivers for the
HiveServer2 protocol family — `SparkDriver` (Databricks / Spark Thrift Server), `HiveServer2Driver`
(Apache Hive), and `ImpalaDriver` (Apache Impala / Apache Kudu) — each a concrete `AdbcDriver` whose
`Open(IReadOnlyDictionary<string,string>)` mints an `AdbcDatabase` that yields `AdbcConnection` /
`AdbcStatement` returning Arrow `RecordBatch` streams. The driver is a CONCRETE implementation of the
`Apache.Arrow.Adbc` abstraction cataloged in `api-arrow.md`; this catalog documents only the driver-
selection surface, the connection-string parameter vocabulary keyed into `Open`, and the integration
seams. The entire query / metadata / result-stream surface (`AdbcConnection.GetObjects`,
`AdbcStatement.SetSqlQuery` / `ExecuteQuery`, `IArrowArrayStream`) is the base-package surface and is
documented in `api-arrow.md`, not duplicated here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc.Drivers.Apache`
- package: `Apache.Arrow.Adbc.Drivers.Apache`
- license: Apache-2.0 (`licenses.nuget.org/Apache-2.0`)
- assembly: `Apache.Arrow.Adbc.Drivers.Apache`
- namespace: `Apache.Arrow.Adbc.Drivers.Apache`, `.Spark`, `.Hive2`, `.Impala`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset (Thrift over `System.Net.Http`/`System.Net.Sockets`). Multi-TFM `net8.0` / `netstandard2.0` / `net472`; the consumer `net10.0` binds the highest asset `lib/net8.0` — no `net10.0` or `net9.0` asset ships, so `net8.0` is the consumed surface. The whole protocol stack (`TCLIService`, `T*Req`/`T*Resp` Thrift messages, `TProtocolVersion`, `TStatus`) is `internal`; the only public surface is the three `AdbcDriver` subclasses, `HiveServer2Exception`, and the parameter/constant key holders (`const string` keys).
- rail: query egress
- ABI floor: `Apache.Arrow.Adbc` `0.23.0` is a PRE-1.0 contract — the abstract-method set of `AdbcDriver`/`AdbcConnection`/`AdbcStatement` this driver overrides can break across a minor bump. All three Arrow/ADBC packages move on coupled `0.23.0` (ADBC) / `23.0.0` (Arrow) lines; a driver bump that changes the abstract base requires re-verifying the override set. The `Apache.Arrow.Adbc.Drivers.Interop.*` (FlightSql, Snowflake) siblings are REJECTED — their native libs ship no `osx-arm64` asset; this pure-managed Thrift driver is the only `osx-arm64`-viable ADBC SQL-warehouse path beside `Drivers.BigQuery`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: driver entrypoints (concrete `AdbcDriver`)
- rail: query egress

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]  | [CAPABILITY]                                                                   |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `SparkDriver`          | `AdbcDriver`    | `.Open(parameters)` -> Databricks / Spark Thrift Server `AdbcDatabase`         |
|  [02]   | `HiveServer2Driver`    | `AdbcDriver`    | `.Open(parameters)` -> Apache Hive Thrift `AdbcDatabase`                       |
|  [03]   | `ImpalaDriver`         | `AdbcDriver`    | `.Open(parameters)` -> Apache Impala / Kudu Thrift `AdbcDatabase`              |
|  [04]   | `HiveServer2Exception` | `AdbcException` | typed driver fault carrying `SqlState` and `NativeError` over `AdbcStatusCode` |

[PUBLIC_TYPE_SCOPE]: connection-string key holders (`static`)
- rail: query egress

Every driver is configured ONLY by the `IReadOnlyDictionary<string,string>` passed to `Open`; these holders carry
the typed `adbc.*` `const string` keys that populate it (`ApacheParameters`/`SparkParameters` are non-static
`public class`; the Hive/Impala/value holders are `public static class`). The map IS the wire contract — there is
no strongly-typed options object.

| [INDEX] | [SYMBOL]                       | [KEYS]                                                                                         |
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
- rail: query egress
- `DataTypeConversionOptions` values key the per-driver `Spark/Hive/ImpalaParameters.DataTypeConv`, never `ApacheParameters`.

| [INDEX] | [SYMBOL]                            | [VALUES]                                              |
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
- rail: query egress

Every `*Driver.Open(IReadOnlyDictionary<string,string>)` mints the protocol `AdbcDatabase` from the map these
keys populate. `Type` selects the server transport, `DataTypeConv` the decimal-as-string policy,
`EscapePatternWildcards` the LIKE-escaping policy; the `ApacheParameters` metadata keys scope a
`GetObjects`/`GetTableSchema` pull through `adbc.get_metadata.target_*`, and a faulted statement raises
`HiveServer2Exception` (`SqlState`/`NativeError`).

| [INDEX] | [OWNER]                 | [KEYS]                                                                                    |
| :-----: | :---------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `SparkParameters`       | `HostName`/`Port`/`Path`/`Token`/`AccessToken`/`AuthType`/`Type` (endpoint + auth)        |
|  [02]   | `SparkParameters`       | `ConnectTimeoutMilliseconds`/`DataTypeConv`/`UserAgentEntry` (timeout, conversion, UA)    |
|  [03]   | `HiveServer2Parameters` | `HostName`/`Port`/`Path`/`AuthType`/`TransportType`/`ConnectTimeoutMilliseconds`          |
|  [04]   | `ImpalaParameters`      | `HostName`/`Port`/`Path`/`AuthType`/`Type`/`TLSOptions` (endpoint + TLS selector)         |
|  [05]   | `ApacheParameters`      | `PollTimeMilliseconds`/`BatchSize`/`BatchSizeStopCondition`/`QueryTimeoutSeconds` (fetch) |
|  [06]   | `ApacheParameters`      | `IsMetadataCommand`/`CatalogName`/`SchemaName`/`TableName`/`TableTypes`/`ColumnName`      |
|  [07]   | `ApacheParameters`      | `ForeignCatalogName`/`ForeignSchemaName`/`ForeignTableName`/`EscapePatternWildcards` (FK) |

[ENTRYPOINT_SCOPE]: inherited base surface (documented in `api-arrow.md`)
- rail: query egress

The result-execution path is the base `Apache.Arrow.Adbc` surface, NOT redefined here: `AdbcDatabase.Connect`
-> `AdbcConnection`; `AdbcConnection.CreateStatement` -> `AdbcStatement`; `AdbcStatement.SqlQuery` set +
`ExecuteQuery` / `ExecuteQueryAsync` (`ValueTask<QueryResult>`, decompile-verified at) ->
`QueryResult.Stream` (`IArrowArrayStream`); `AdbcStatement.Bind(RecordBatch, Schema)` / `BindStream(IArrowArrayStream)`
bind parameter batches ahead of execution (both real base virtuals at); `AdbcConnection.GetObjects` /
`GetTableSchema` / `GetInfo` / `GetTableTypes` for catalog metadata. The driver overrides those abstract
members; consumers compose them through the `api-arrow.md` contract.

## [04]-[IMPLEMENTATION_LAW]

[DRIVER_ALGEBRA]:
- driver root: `SparkDriver` / `HiveServer2Driver` / `ImpalaDriver` (one concrete `AdbcDriver` per protocol)
- config root: the `IReadOnlyDictionary<string,string>` parameter map keyed by the `static` `*Parameters` holders
- auth/transport discriminant root: the `*AuthTypeConstants` / `*ServerTypeConstants` / `*TransportTypeConstants` value holders
- result root: Arrow `RecordBatch` over `IArrowArrayStream` (the base `Apache.Arrow.Adbc` contract)
- error root: `HiveServer2Exception : AdbcException` carrying `SqlState` + `NativeError`

[PARAMETER_CONTRACT]:
- There is NO typed options class — the parameter dictionary IS the API. A consumer composes the connection
  by mapping its own typed config record onto these `const string` keys, never by passing a strongly-typed object.
- Auth-type and server-type are string discriminants (`SparkAuthTypeConstants.OAuth`, `SparkServerTypeConstants.Http`),
  not enums — a Persistence boundary maps them through a `[SmartEnum<string>]` so the canonical token survives projection.
- TLS is two parallel option sets selected by transport: `HttpTlsOptions` for the HTTP transport, `StandardTlsOptions`
  for the raw-socket transport; Impala routes them through its `ImpalaParameters.TLSOptions` selector key.

[LOCAL_ADMISSION]:
- The driver is admitted ONLY through the Persistence Query-federation boundary that owns the parameter-map
  construction; the `adbc.*` key strings never leak into an interior signature (they map to a typed connection record at the edge).
- A faulted statement surfaces as `HiveServer2Exception` -> the boundary lifts `SqlState`/`NativeError` into the
  canonical `Fin`/typed-failure rail; no `AdbcException` crosses into domain logic.
- Result `RecordBatch` streams are consumed through the base-package `IArrowArrayStream`, then projected to the
  canonical Arrow owner — the driver is a SOURCE adapter, not a data model.

[STACKING]:
- base-abstraction seam: the driver IS the concrete `AdbcDriver` for the `Apache.Arrow.Adbc` abstraction in
  `api-arrow.md`. The Persistence `Query/federation#FEDERATED_PLAN` rail selects the driver by protocol (`SparkDriver` for
  Databricks/Spark Thrift, `HiveServer2Driver` for Hive, `ImpalaDriver` for Impala), opens it with a parameter
  map, and reads the result through the base `QueryResult.Stream` `IArrowArrayStream` — one egress shape across
  every warehouse backend, distinct from the in-process DuckDB SQL path (`api-duckdb.md`).
- arrow-result seam: every statement returns Arrow `RecordBatch`es, so the egress folds directly into the
  `Apache.Arrow` columnar owner (`api-arrow.md`) — a Spark/Hive/Impala query result and an in-process Arrow
  batch are the SAME `RecordBatch` type, then optionally written to Parquet (`api-parquetsharp.md`) or a Delta
  table (`api-deltalake.md`) with zero re-materialization. The warehouse is just another `IArrowArrayStream` source.
- substrait seam: the base `AdbcStatement.SubstraitPlan` surface lets a `FlowtideDotNet.Substrait` plan
  (`api-flowtide-substrait.md`) execute against the Thrift warehouse where the backend supports it, so the same
  portable relational-algebra IR drives both the federation pipeline and the remote SQL endpoint.
- transport-security seam: the `HttpTlsOptions` / `StandardTlsOptions` / `HttpProxyOptions` key sets compose with
  the deployment's mTLS/proxy posture; the boundary populates them from the same redacted-credential source the
  KMS/KeyVault rows (`api-aws-kms.md`, `api-azure-keyvault.md`) feed, never inline literals — a self-signed cert
  in a private VPC sets `HttpTlsOptions.AllowSelfSigned`, never `DisableServerCertificateValidation`.

[RAIL_LAW]:
- Package: `Apache.Arrow.Adbc.Drivers.Apache` `0.23.0` (Apache-2.0, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 ADBC contract)
- Owns: the concrete HiveServer2-family ADBC drivers (`SparkDriver`/`HiveServer2Driver`/`ImpalaDriver`), their
  `adbc.*` connection-string parameter vocabulary, auth/transport/TLS/proxy key sets, and the `HiveServer2Exception` error rail
- Accept: a Spark/Hive/Impala Thrift SQL warehouse opened through the Query-federation boundary, configured by the
  typed parameter map, returning Arrow `RecordBatch` streams over the base `Apache.Arrow.Adbc` contract
- Reject: an `adbc.*` key string in an interior signature; an `AdbcException` crossing into domain logic; a
  re-materialization of the `RecordBatch` result away from the Arrow owner; use of an `Interop.*` driver (no `osx-arm64` native asset)
