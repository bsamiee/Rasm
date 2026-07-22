# [RASM_PERSISTENCE_API_CLICKHOUSE]

`ClickHouse.Driver` owns the distributed columnar OLAP backend over the ClickHouse HTTP interface: the thread-safe connection-pooled `ClickHouseClient`, the full ADO.NET provider mirror, the parallel RowBinary bulk-ingest rail, the `QueryStats` throughput receipt, and an `ActivitySource` diagnostics surface emitting `db.*`/`db.clickhouse.*` OpenTelemetry tags. It is the billion-row scale-out backend class beyond the in-PG TimescaleDB hypertable tier and the embedded DuckDB analytical floor, admitted through the `Store/provisioning` store-profile algebra as a non-transactional append sink.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ClickHouse.Driver`
- package: `ClickHouse.Driver` (MIT)
- assembly: `ClickHouse.Driver`
- namespace: `ClickHouse.Driver` (`ClickHouseClient`, POCO/JSON attributes, `JsonReadMode`/`JsonWriteMode`), `ClickHouse.Driver.ADO` (settings, `QueryStats`, connection/command/reader/data-source), `ClickHouse.Driver.Copy` (`RowBinaryFormat`), `ClickHouse.Driver.Diagnostic`, `ClickHouse.Driver.Numerics` (`ClickHouseDecimal`)
- asset: pure-managed, no native payload; transport is `HttpClient` over the ClickHouse HTTP port (8123 plaintext, 8443 TLS)
- rail: store-backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: high-ingest primary client — `ClickHouseClient : IClickHouseClient, IDisposable`, one instance per cluster shared across threads, the primary entry over the ADO mirror

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `ClickHouseClient`               | primary client   | thread-safe pooled query + bulk-ingest owner       |
|  [02]   | `IClickHouseClient`              | client contract  | DI seam                                            |
|  [03]   | `ClickHouseClientSettings`       | settings record  | strongly-typed connection + ingest policy          |
|  [04]   | `QueryOptions`                   | per-query policy | query id, database, roles, custom settings, bearer |
|  [05]   | `InsertOptions`                  | ingest policy    | `QueryOptions` + batch, parallelism, format, types |
|  [06]   | `QueryResult`                    | internal result  | HTTP response + query id + stats carrier           |
|  [07]   | `ClickHouseRawResult`            | raw result       | un-decoded HTTP body (stream, bytes, string)       |
|  [08]   | `MemoryStreamManager` (property) | pool handle      | `RecyclableMemoryStreamManager` ingest buffer pool |

[PUBLIC_TYPE_SCOPE]: ADO.NET provider mirror — the `System.Data.Common` surface for tools binding `DbProviderFactory`; `ClickHouseDataSource : DbDataSource` is the pooled connection-factory root reaching the underlying client through `GetClient()`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CAPABILITY]                                                    |
| :-----: | :---------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `ClickHouseConnection`              | connection root  | `DbConnection`; no transactions (`BeginDbTransaction` throws)   |
|  [02]   | `ClickHouseCommand`                 | command surface  | `DbCommand`; `QueryStats`/`QueryId`/`ServerTimezone` after exec |
|  [03]   | `ClickHouseDataReader`              | reader surface   | `DbDataReader` + `IEnumerable<IDataReader>`; typed column reads |
|  [04]   | `ClickHouseDataSource`              | data source      | `DbDataSource`; pooled connection factory, `GetClient()`        |
|  [05]   | `ClickHouseConnectionFactory`       | provider factory | `DbProviderFactory`; `Instance` singleton                       |
|  [06]   | `ClickHouseConnectionStringBuilder` | conn string      | `DbConnectionStringBuilder`; typed keys, `ToSettings()`         |
|  [07]   | `ClickHouseDbParameter`             | parameter        | `DbParameter`; `ClickHouseType` server-type hint, `QueryForm`   |
|  [08]   | `ClickHouseParameterCollection`     | parameter set    | named `{name:Type}` placeholder substitution                    |
|  [09]   | `ClickHouseDataAdapter`             | data adapter     | `DbDataAdapter` fill surface                                    |

[PUBLIC_TYPE_SCOPE]: ingest format, receipt, POCO/JSON mapping, and diagnostics; the `ClickHouse.Driver.Types` column type-system is internal engine machinery, and `ClickHouseDecimal` (`readonly struct`, `IConvertible`, `IComparable<decimal>`) reads `Decimal128`/`Decimal256` past `System.Decimal` range under `ClickHouseClientSettings.UseCustomDecimals`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :------------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `QueryStats`                     | ingest receipt     | `record` of `ReadRows`/`WrittenRows`/`ResultRows`/`ElapsedNs` + byte counts |
|  [02]   | `RowBinaryFormat`                | codec enum         | `RowBinary` / `RowBinaryWithDefaults`                                       |
|  [03]   | `JsonReadMode` / `JsonWriteMode` | JSON codec enum    | `Binary` / `String` / `None` ClickHouse JSON column policy                  |
|  [04]   | `ClickHouseColumnAttribute`      | POCO mapping       | `[ClickHouseColumn(Name, Type)]` on a property                              |
|  [05]   | `ClickHouseNotMappedAttribute`   | POCO mapping       | `[ClickHouseNotMapped]` exclusion                                           |
|  [06]   | `ClickHouseJsonPathAttribute`    | JSON mapping       | maps a property to a ClickHouse JSON path                                   |
|  [07]   | `ClickHouseJsonIgnoreAttribute`  | JSON mapping       | excludes a property from the JSON column                                    |
|  [08]   | `ClickHouseServerException`      | server failure     | `DbException`; carries `Query` + numeric `ErrorCode`                        |
|  [09]   | `ClickHouseDiagnosticsOptions`   | diagnostics toggle | `ActivitySourceName`, `IncludeSqlInActivityTags`, `StatementMaxLength`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction, query, and registration

| [INDEX] | [SURFACE]                                                           | [SHAPE]      | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :----------- | :------------------------------------------ |
|  [01]   | `new ClickHouseClient(connectionString)`                            | ctor         | connection string; owns pooled `HttpClient` |
|  [02]   | `new ClickHouseClient(connectionString, IHttpClientFactory, name?)` | ctor         | rides an injected `IHttpClientFactory`      |
|  [03]   | `new ClickHouseClient(connectionString, HttpClient)`                | ctor         | rides a caller-supplied `HttpClient`        |
|  [04]   | `new ClickHouseClient(ClickHouseClientSettings)`                    | ctor         | from validated settings record              |
|  [05]   | `PingAsync(QueryOptions?, ct)`                                      | health       | `GET /ping`; `bool` reachability            |
|  [06]   | `ExecuteReaderAsync(sql, parameters?, QueryOptions?, ct)`           | query        | streams `ClickHouseDataReader` rows         |
|  [07]   | `QueryAsync<T>(sql, parameters?, QueryOptions?, ct)`                | query        | `IAsyncEnumerable<T>`; POCO-mapped stream   |
|  [08]   | `ExecuteScalarAsync(sql, parameters?, QueryOptions?, ct)`           | query        | first column of first row                   |
|  [09]   | `ExecuteNonQueryAsync(sql, parameters?, QueryOptions?, ct)`         | command      | DDL/DML row count                           |
|  [10]   | `ExecuteRawResultAsync(sql, QueryOptions?, ct)`                     | raw          | `ClickHouseRawResult` un-decoded body       |
|  [11]   | `CreateConnection()`                                                | ADO bridge   | mints `ClickHouseConnection` on this client |
|  [12]   | `RegisterBinaryInsertType<T>()`                                     | registration | registers a POCO for RowBinary insert       |
|  [13]   | `RegisterJsonSerializationType<T>()`                                | registration | registers a POCO for a JSON column          |
|  [14]   | `RegisterPocoType<T>()`                                             | registration | registers a POCO for read + insert mapping  |

[ENTRYPOINT_SCOPE]: bulk ingest — `InsertBinaryAsync` probes the table schema, batches at `InsertOptions.BatchSize`, fans batches through `Parallel.ForEachAsync` at `MaxDegreeOfParallelism`, serializes each as RowBinary into a pooled `RecyclableMemoryStream`, and POSTs gzip-compressed; the `<T>` overload requires a prior `RegisterBinaryInsertType<T>`, reads `[ClickHouseColumn]`/`[ClickHouseNotMapped]`, and returns the total rows written
- `InsertBinaryAsync` takes `InsertOptions?, ct`; `InsertRawStreamAsync`/`PostStreamAsync` take `QueryOptions?, ct`; `PostStreamAsync` body is a `Stream` or a `Func<Stream, CancellationToken, Task>` callback.
- `InsertOptions`: `BatchSize` (default 100000), `MaxDegreeOfParallelism` (default 1), `Format`, `ColumnTypes`, `UseSchemaCache`.

| [INDEX] | [SURFACE]                                                               | [SHAPE]       | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `InsertBinaryAsync(table, columns, IEnumerable<object[]> rows)`         | bulk insert   | parallel RowBinary insert of positional rows |
|  [02]   | `InsertBinaryAsync<T>(table, IEnumerable<T> rows)`                      | typed bulk    | attributed-POCO insert                       |
|  [03]   | `InsertRawStreamAsync(table, Stream, format, columns?, useCompression)` | raw stream    | streams a pre-framed body                    |
|  [04]   | `PostStreamAsync(sql, Stream, isCompressed)`                            | raw stream    | low-level POST of a stream                   |
|  [05]   | `PostStreamAsync(sql, Func<…> callback, isCompressed)`                  | callback sink | server-pull streaming via callback writer    |

[ENTRYPOINT_SCOPE]: ADO mirror and typed reads — `ClickHouseDataReader` extends `DbDataReader` with ClickHouse-native typed accessors and implements `IEnumerable<IDataReader>` for LINQ-style iteration; `GetBytes`/`GetChars` throw `NotImplementedException`, so read the value object directly
- `ClickHouseConnectionStringBuilder` keys: `Host`, `Port`, `Protocol`, `Database`, `Username`, `Password`, `Compression`, `UseSession`, `Roles`, `Timeout`, `JsonReadMode`, `JsonWriteMode`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]     | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :---------- | :----------------------------------------- |
|  [01]   | `new ClickHouseConnection(connectionString)/.Open()/.OpenAsync(ct)`       | ADO connect | opens the HTTP-backed connection           |
|  [02]   | `ClickHouseConnection.CreateCommand(commandText?)`                        | command     | typed `ClickHouseCommand`                  |
|  [03]   | `ExecuteReaderAsync/ExecuteScalarAsync/ExecuteNonQueryAsync`              | exec        | `ClickHouseCommand`; sets `QueryStats`     |
|  [04]   | `new ClickHouseDataSource(connectionString, ...)/OpenConnectionAsync(ct)` | data source | pooled ADO factory; `GetClient()`          |
|  [05]   | `GetUInt16/GetUInt32/GetUInt64`                                           | typed read  | unsigned-integer column reads              |
|  [06]   | `GetBigInteger/GetIPAddress/GetTuple/GetDateTimeOffset`                   | typed read  | `BigInteger`/`IPAddress`/`ITuple` reads    |
|  [07]   | `ClickHouseConnectionStringBuilder { … }`                                 | builder     | typed keys (above); `ToSettings()`         |
|  [08]   | `ClickHouseParameterCollection` + `{name:Type}` placeholders              | parameters  | named, server-typed parameter substitution |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- transport is the HTTP interface only; `HttpClient` and `HttpClientFactory` are mutually exclusive at `Validate()`, and the client owns a default pooled factory, an injected `IHttpClientFactory`, or a caller-supplied `HttpClient`.
- `ClickHouseClient` is thread-safe and reusable, one instance per cluster; the ADO `ClickHouseConnection` is the per-scope handle minted from it.
- `ClickHouseConnection.BeginDbTransaction` throws `NotSupportedException`; durability is per-insert-block, and the store-profile transaction rail treats this backend as a non-transactional sink.
- server version reads through `ExecuteScalarAsync("SELECT version()")`.
- `UseSession=true` mints a `SessionId` and serializes to one concurrent query, so `InsertOptions.MaxDegreeOfParallelism` must be 1 under a session.
- `UseCompression` (default) sets gzip request/response encoding; the reader requires the `HttpClient` `AutomaticDecompression` it does not itself decode.
- `RowBinaryFormat.RowBinary` is the dense insert format; `RowBinaryWithDefaults` lets the server fill omitted column defaults.
- `BearerToken` sets `Authorization: Bearer`, else HTTP Basic from `Username`/`Password`; `Roles` projects ClickHouse RBAC role names per query.
- `QueryStats` is the columnar-throughput receipt parsed from the `X-ClickHouse-Summary` response header; after any command it lands on `Command.QueryStats` with `QueryId` and `ServerTimezone`, and the client query path threads it through the diagnostics span as the `Store/provisioning` profile throughput receipt feeding a gauge.

[STACKING]:
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner crosses into a column through `api-thinktecture-serialization` — the factory projects the owner to its key, an attributed POCO field (`[ClickHouseColumn]`) carries the key into `InsertBinaryAsync<T>`, and the inverse decodes the `ClickHouseDataReader` column back through the same factory.
- temporal columns: `DateTime64`/`Date32` columns bind the substrate `NodaTime` (`api-nodatime`), an `Instant`/`ZonedDateTime` owner projecting to the column value.
- columnar interchange: a `ParquetSharp` (`api-parquetsharp`) Parquet file or an `api-arrow` batch ingests through `InsertRawStreamAsync(table, stream, "Parquet"/"ArrowStream", ...)` — ClickHouse decodes the format server-side, and the managed side streams the pre-framed body.
- telemetry: `ClickHouseDiagnosticsOptions.ActivitySourceName` (`"ClickHouse.Driver"`) names the source the AppHost tracer subscribes on the `telemetry` port; each query/insert opens an `ActivityKind.Client` span tagged `db.system=clickhouse` with `QueryStats` projected as `db.clickhouse.*`, and the static `IncludeSqlInActivityTags`/`StatementMaxLength` (default 300) gate SQL capture once at composition.
- fault rail: `ClickHouseServerException : DbException` lifts at the client edge discriminated on its numeric `ErrorCode` and carries the offending `Query`, joining the store-profile failure rail.

[LOCAL_ADMISSION]:
- ClickHouse enters behind the `Store/provisioning` store-profile vocabulary as a distinct scale-out columnar backend class, orthogonal to in-PG TimescaleDB and embedded DuckDB.
- `ClickHouseClient.InsertBinaryAsync` over POCO or `object[]` is the bulk-ingest rail; batch size and parallelism are profile policy.
- inserts record as idempotent append blocks keyed by `InsertOptions.QueryId`, never under a transaction scope.
- `ClickHouse.Driver.Types` column type-system stays internal; the profile declares column types in DDL or `[ClickHouseColumn(Type=...)]`, never a CLR type instance.

[RAIL_LAW]:
- Package: `ClickHouse.Driver`
- Owns: HTTP-interface columnar OLAP query, parallel RowBinary bulk ingest, the ADO.NET provider mirror, the `QueryStats` throughput receipt, and `ActivitySource` diagnostics
- Accept: thread-safe `ClickHouseClient` reuse, `InsertBinaryAsync` with profile-declared batch and parallelism, `QueryStats`-driven throughput receipts, `ActivitySource` telemetry through the AppHost tracer, `ClickHouseServerException` fault discrimination on `ErrorCode`
- Reject: per-call `HttpClient` construction, parallel bulk insert under an enabled session, transaction-scoped writes, and surfacing the internal `ClickHouse.Driver.Types` type-system as a consumer API
