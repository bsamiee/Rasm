# [RASM_PERSISTENCE_API_CLICKHOUSE]

`ClickHouse.Driver` supplies the official high-ingest distributed columnar OLAP client over the ClickHouse HTTP interface: the thread-safe primary `ClickHouseClient` (reusable, connection-pooled, the primary entry), the full ADO.NET mirror (`ClickHouseConnection`/`ClickHouseCommand`/`ClickHouseDataReader`/`ClickHouseDataSource`/`ClickHouseConnectionFactory`), the parallel RowBinary bulk-ingest rail (`InsertBinaryAsync` over `object[]` rows or attributed POCOs, plus `InsertRawStreamAsync`/`PostStreamAsync` for pre-framed streams), strongly-typed connection settings, the `QueryStats` ingest receipt, and a first-class `System.Diagnostics.ActivitySource` diagnostics surface emitting OpenTelemetry `db.*`/`db.clickhouse.*` semantic-convention tags. This is the scale-out billion-row aggregation store-backend beyond the in-PG TimescaleDB hypertable tier and the embedded DuckDB analytical floor (`api-duckdb`); it composes the `Store/provisioning` store-profile algebra as a distinct backend class, reuses the admitted `NodaTime` transitive for temporal columns, and folds its `ActivitySource` into the AppHost `telemetry` port rather than carrying a bespoke logger.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ClickHouse.Driver`
- package: `ClickHouse.Driver`
- license: MIT
- assembly: `ClickHouse.Driver`
- namespace: `ClickHouse.Driver` (`ClickHouseClient`, the POCO/JSON attributes), `ClickHouse.Driver.ADO` (`ClickHouseClientSettings`, `QueryStats`, the connection/command/reader/data-source/conn-string-builder), `ClickHouse.Driver.ADO.Adapters`, `ClickHouse.Driver.ADO.Parameters`, `ClickHouse.Driver.ADO.Readers`, `ClickHouse.Driver.Copy` (`RowBinaryFormat`, the `[Obsolete]` `ClickHouseBulkCopy`), `ClickHouse.Driver.Diagnostic` (`ClickHouseDiagnosticsOptions`), `ClickHouse.Driver.Json` (`ClickHouseJsonPathAttribute`/`ClickHouseJsonIgnoreAttribute`/`ClickHouseJsonSerializationException`), `ClickHouse.Driver.Numerics` (the public `ClickHouseDecimal` value type); `ClickHouse.Driver.Types`/`ClickHouse.Driver.Utility` are internal engine machinery
- target: multi-target (`net10.0`, `net9.0`, `net8.0`, `net6.0`); the `net10.0` consumer binds `lib/net10.0` (the bound asset, highest-precedence — not a fallback TFM)
- native: pure-managed (no `runtimes/<rid>/native` payload); transport is `HttpClient` over the ClickHouse HTTP port (8123 plaintext / 8443 TLS)
- transitive: `Microsoft.Extensions.Http@10.0.5`, `Microsoft.Extensions.Logging.Abstractions@10.0.5`, `Microsoft.IO.RecyclableMemoryStream@3.0.1` (the pooled `RecyclableMemoryStreamManager` backing bulk serialization), `NodaTime` (nuspec floor `3.2.3`; the substrate `NodaTime` row pins `3.3.2` higher and is what resolves — `api-nodatime`), `System.Text.Json@10.0.3`
- xml docs: absent (member intent is decompile-sourced)
- rail: store-backend

The transport is exclusively the HTTP interface — there is no native ClickHouse binary-protocol P/Invoke. `ClickHouseConnection` does NOT support transactions: `BeginDbTransaction` throws `NotSupportedException`. `ClickHouseConnection.ServerVersion` is `[Obsolete]` and throws; the server version is read through `ExecuteScalarAsync("SELECT version()")`. The legacy `ClickHouse.Driver.Copy.ClickHouseBulkCopy` is `[Obsolete]` — its functionality moved to `ClickHouseClient.InsertBinaryAsync`, which is the admitted bulk-ingest entry.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: high-ingest primary client
- rail: store-backend

`ClickHouseClient : IClickHouseClient, IDisposable` is the thread-safe, reusable, connection-pooled primary API (one instance per cluster, shared across threads) and the primary entry over the ADO mirror. It owns query execution (`ExecuteReaderAsync`/`ExecuteScalarAsync`/`ExecuteNonQueryAsync`/`ExecuteRawResultAsync`/`PingAsync`), the parallel RowBinary bulk-insert rail (`InsertBinaryAsync`, `InsertRawStreamAsync`, `PostStreamAsync`), POCO/JSON type registration (`RegisterBinaryInsertType<T>`/`RegisterJsonSerializationType<T>`), and the `RecyclableMemoryStreamManager` pool slot. `CreateConnection()` mints an ADO `ClickHouseConnection` riding the same client.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]    | [RAIL]                                                 |
| :-----: | :------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `ClickHouseClient`               | primary client   | thread-safe pooled query + bulk-ingest owner           |
|  [02]   | `IClickHouseClient`              | client contract  | the client interface (DI seam)                         |
|  [03]   | `ClickHouseClientSettings`       | settings record  | strongly-typed connection + ingest policy              |
|  [04]   | `QueryOptions`                   | per-query policy | query id, database, roles, custom settings, bearer     |
|  [05]   | `InsertOptions`                  | ingest policy    | `QueryOptions` + batch/parallelism/format/column-types |
|  [06]   | `QueryResult`                    | internal result  | (internal) HTTP response + query id + stats carrier    |
|  [07]   | `ClickHouseRawResult`            | raw result       | un-decoded HTTP body (stream/bytes/string)             |
|  [08]   | `MemoryStreamManager` (property) | pool handle      | `RecyclableMemoryStreamManager` ingest buffer pool     |

[PUBLIC_TYPE_SCOPE]: ADO.NET provider mirror
- rail: store-backend

The full `System.Data.Common` surface for tools and ORMs that bind `DbProviderFactory`. `ClickHouseDataSource : DbDataSource` is the modern connection-factory root (`CreateConnection`/`OpenConnection`/`OpenConnectionAsync`, `GetClient()` reaches the underlying `IClickHouseClient`); `ClickHouseConnectionFactory.Instance` registers under `DbProviderFactories`.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [RAIL]                                                          |
| :-----: | :---------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `ClickHouseConnection`              | connection root  | `DbConnection`; no transactions (`BeginDbTransaction` throws)   |
|  [02]   | `ClickHouseCommand`                 | command surface  | `DbCommand`; `QueryStats`/`QueryId`/`ServerTimezone` after exec |
|  [03]   | `ClickHouseDataReader`              | reader surface   | `DbDataReader` + `IEnumerable<IDataReader>`; typed column reads |
|  [04]   | `ClickHouseDataSource`              | data source      | `DbDataSource`; pooled connection factory                       |
|  [05]   | `ClickHouseConnectionFactory`       | provider factory | `DbProviderFactory`; `Instance` singleton                       |
|  [06]   | `ClickHouseConnectionStringBuilder` | conn string      | `DbConnectionStringBuilder`; typed keys + `ToSettings()`        |
|  [07]   | `ClickHouseDbParameter`             | parameter        | `DbParameter`; `ClickHouseType` server-type hint, `QueryForm`   |
|  [08]   | `ClickHouseParameterCollection`     | parameter set    | named `{name:Type}` placeholder substitution                    |
|  [09]   | `ClickHouseDataAdapter`             | data adapter     | `DbDataAdapter` fill surface                                    |

[PUBLIC_TYPE_SCOPE]: ingest format, receipt, mapping, and diagnostics
- rail: store-backend

`QueryStats` is the columnar-throughput receipt; `RowBinaryFormat` selects the bulk wire codec; the POCO attributes drive attributed bulk/JSON mapping; the `static` `ClickHouseDiagnosticsOptions` global toggles + the package `ActivitySource` own observability. `ClickHouse.Driver.Types.*` (the `ArrayType`/`MapType`/`Decimal*Type`/`DateTime64Type`/`Enum*Type`/`TupleType`/`PointType`/`PolygonType` column type-system, `ClickHouseType` base, `Grammar.Parser`/`Tokenizer`) and `TypeConverter` are `internal` engine machinery — NOT a consumer surface; the column type is expressed through the SQL DDL or the `[ClickHouseColumn(Type=...)]` attribute string, never a CLR `ClickHouseType` instance. The one public value type in the numeric family is `ClickHouse.Driver.Numerics.ClickHouseDecimal` (a `readonly struct` implementing `IConvertible`/`IComparable<decimal>`) — surfaced when `ClickHouseClientSettings.UseCustomDecimals` (default true) reads `Decimal128`/`Decimal256` columns at full precision past `System.Decimal` range.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [RAIL]                                                                                                                                                                |
| :-----: | :------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `QueryStats`                     | ingest receipt     | `record (ReadRows, ReadBytes, WrittenRows, WrittenBytes, TotalRowsToRead, ResultRows, ResultBytes, ElapsedNs)`                                                        |
|  [02]   | `RowBinaryFormat`                | codec enum         | `RowBinary` / `RowBinaryWithDefaults`                                                                                                                                 |
|  [03]   | `JsonReadMode` / `JsonWriteMode` | JSON codec enum    | `Binary` / `String` / `None` (ClickHouse JSON column policy)                                                                                                          |
|  [04]   | `ClickHouseColumnAttribute`      | POCO mapping       | `[ClickHouseColumn(Name, Type)]` on a property                                                                                                                        |
|  [05]   | `ClickHouseNotMappedAttribute`   | POCO mapping       | `[ClickHouseNotMapped]` exclusion                                                                                                                                     |
|  [06]   | `ClickHouseJsonPathAttribute`    | JSON mapping       | maps a property to a ClickHouse JSON path                                                                                                                             |
|  [07]   | `ClickHouseJsonIgnoreAttribute`  | JSON mapping       | excludes a property from the JSON column                                                                                                                              |
|  [08]   | `ClickHouseServerException`      | server failure     | `DbException`; carries `Query` + numeric `ErrorCode`                                                                                                                  |
|  [09]   | `ClickHouseDiagnosticsOptions`   | diagnostics toggle | `static class`: `const ActivitySourceName="ClickHouse.Driver"`, static `IncludeSqlInActivityTags`, static `StatementMaxLength=300` (process-global, not per-instance) |
|  [10]   | `QueryStats` (in `Activity`)     | telemetry tags     | projected as `db.clickhouse.read_rows`/`written_rows`/`elapsed_ns`                                                                                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and query
- rail: store-backend

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                                    |
| :-----: | :--------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `new ClickHouseClient(connectionString)`                               | ctor           | from connection string (owns default pooled `HttpClient`) |
|  [02]   | `new ClickHouseClient(connectionString, IHttpClientFactory, name?)`    | ctor           | rides an injected `IHttpClientFactory`                    |
|  [03]   | `new ClickHouseClient(ClickHouseClientSettings)`                       | ctor           | from validated settings record                            |
|  [04]   | `PingAsync(QueryOptions?, ct)`                                         | health         | `GET /ping`; `bool` reachability                          |
|  [05]   | `ExecuteReaderAsync(sql, parameters?, QueryOptions?, ct)`              | query          | streams `ClickHouseDataReader` rows                       |
|  [06]   | `ExecuteScalarAsync(sql, parameters?, QueryOptions?, ct)`              | query          | first column of first row                                 |
|  [07]   | `ExecuteNonQueryAsync(sql, parameters?, QueryOptions?, ct)`            | command        | DDL/DML row count                                         |
|  [08]   | `ExecuteRawResultAsync(sql, QueryOptions?, ct)`                        | raw            | `ClickHouseRawResult` (un-decoded body)                   |
|  [09]   | `CreateConnection()`                                                   | ADO bridge     | mints `ClickHouseConnection` on this client               |
|  [10]   | `RegisterJsonSerializationType<T>()` / `RegisterBinaryInsertType<T>()` | registration   | registers POCO type for JSON column / RowBinary insert    |

[ENTRYPOINT_SCOPE]: bulk ingest (the high-throughput rail)
- rail: store-backend

`InsertBinaryAsync` is the admitted high-ingest path (the `[Obsolete]` `ClickHouseBulkCopy` forwards to it). It probes the table schema, batches by `InsertOptions.BatchSize` (default 100000), and fans batches through `Parallel.ForEachAsync` at `MaxDegreeOfParallelism` (default 1; must be 1 when sessions are enabled — ClickHouse allows one query per session), serializing each batch as RowBinary into a pooled `RecyclableMemoryStream` and POSTing gzip-compressed. The POCO overload requires a prior `RegisterBinaryInsertType<T>` and reads `[ClickHouseColumn]`/`[ClickHouseNotMapped]`. Returns the total rows written.

| [INDEX] | [SURFACE]                                                                                             | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `InsertBinaryAsync(table, columns, IEnumerable<object[]> rows, InsertOptions?, ct)`                   | bulk insert    | parallel RowBinary insert of positional rows                 |
|  [02]   | `InsertBinaryAsync<T>(table, IEnumerable<T> rows, InsertOptions?, ct)`                                | typed bulk     | attributed-POCO insert (needs `RegisterBinaryInsertType<T>`) |
|  [03]   | `InsertRawStreamAsync(table, Stream, format, columns?, useCompression, QueryOptions?, ct)`            | raw stream     | streams a pre-framed body (`FORMAT <format>`)                |
|  [04]   | `PostStreamAsync(sql, Stream, isCompressed, ct, QueryOptions?)`                                       | raw stream     | low-level POST of a stream                                   |
|  [05]   | `PostStreamAsync(sql, Func<Stream,CancellationToken,Task> callback, isCompressed, ct, QueryOptions?)` | callback sink  | server-pull streaming via callback writer                    |
|  [06]   | `InsertOptions { BatchSize, MaxDegreeOfParallelism, Format, ColumnTypes, UseSchemaCache }`            | object init    | tunes batch size, parallelism, RowBinary format              |

[ENTRYPOINT_SCOPE]: ADO mirror and typed reads
- rail: store-backend

`ClickHouseDataReader` extends `DbDataReader` with ClickHouse-native typed accessors beyond the base contract; it implements `IEnumerable<IDataReader>` for LINQ-style row iteration. `GetBytes`/`GetChars` are `NotImplementedException` — read the value object directly.

| [INDEX] | [SURFACE]                                                                                                                                                        | [ENTRY_FAMILY] | [RAIL]                                                               |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new ClickHouseConnection(connectionString)` / `.Open()` / `.OpenAsync(ct)`                                                                                      | ADO connect    | opens the HTTP-backed connection                                     |
|  [02]   | `ClickHouseConnection.CreateCommand(commandText?)`                                                                                                               | command        | typed `ClickHouseCommand`                                            |
|  [03]   | `ClickHouseCommand.ExecuteReaderAsync` / `ExecuteScalarAsync` / `ExecuteNonQueryAsync`                                                                           | exec           | ADO async execution; sets `QueryStats`/`QueryId`                     |
|  [04]   | `new ClickHouseDataSource(connectionString, ...)` / `OpenConnectionAsync(ct)`                                                                                    | data source    | pooled ADO factory; `GetClient()` reaches `IClickHouseClient`        |
|  [05]   | `ClickHouseDataReader.GetUInt16/GetUInt32/GetUInt64`                                                                                                             | typed read     | unsigned-integer column reads                                        |
|  [06]   | `ClickHouseDataReader.GetBigInteger` / `GetIPAddress` / `GetTuple` / `GetDateTimeOffset`                                                                         | typed read     | `BigInteger`/`IPAddress`/`ITuple`/tz-aware reads                     |
|  [07]   | `ClickHouseConnectionStringBuilder { Host, Port, Protocol, Database, Username, Password, Compression, UseSession, Roles, Timeout, JsonReadMode, JsonWriteMode }` | builder        | typed keys; `set_<k>` rows carry ClickHouse settings; `ToSettings()` |
|  [08]   | `ClickHouseParameterCollection` + `{name:Type}` placeholders                                                                                                     | parameters     | named, server-typed parameter substitution                           |

## [04]-[IMPLEMENTATION_LAW]

[CLICKHOUSE_TOPOLOGY]:
- transport: HTTP interface only (`HttpClient`); the client owns either a default pooled `IHttpClientFactory` (`DefaultPoolHttpClientFactory`), an injected `IHttpClientFactory`, or a caller-supplied `HttpClient` — `HttpClient` and `HttpClientFactory` are mutually exclusive at `Validate()`.
- threading: `ClickHouseClient` is thread-safe and reusable; one instance per cluster, shared. The ADO `ClickHouseConnection` is the per-scope handle minted from it.
- no transactions: `ClickHouseConnection.BeginDbTransaction` throws `NotSupportedException` — ClickHouse has no client-side transaction; durability is per-insert-block. The store-profile transaction rail (`Element/graph`) treats this backend as a non-transactional sink.
- sessions: `UseSession=true` mints a `SessionId` (GUID) and serializes to one concurrent query — `InsertOptions.MaxDegreeOfParallelism` must be 1 under a session or the insert throws.
- compression: `UseCompression` (default true) sets gzip request/response encoding; the reader rejects compressed bytes the `HttpClient` did not auto-decompress (set `AutomaticDecompression`).
- bulk codec: `RowBinaryFormat.RowBinary` is the dense binary insert format; `RowBinaryWithDefaults` lets the server fill column defaults for omitted values.
- auth: `BearerToken` sets `Authorization: Bearer`; otherwise HTTP Basic from `Username`/`Password`. `Roles` projects ClickHouse RBAC role names per query.

[INGEST_RECEIPT]:
- `QueryStats` is the canonical columnar-throughput receipt, parsed from the `X-ClickHouse-Summary` response header: `ReadRows`/`ReadBytes`/`WrittenRows`/`WrittenBytes`/`TotalRowsToRead`/`ResultRows`/`ResultBytes`/`ElapsedNs`. After any `ClickHouseCommand` execution it lands on `Command.QueryStats` alongside `QueryId` and `ServerTimezone`; the `ClickHouseClient` query path threads it through the diagnostics span.
- this is the `Store/provisioning` profile receipt for the ClickHouse backend — the rows/bytes counts feed a throughput gauge, never a bespoke counter.

[LOCAL_ADMISSION]:
- ClickHouse enters behind the same store-profile vocabulary as every backend (`Store/provisioning`); it is a distinct backend class (scale-out columnar OLAP), orthogonal to in-PG TimescaleDB and embedded DuckDB.
- the bulk-ingest rail is `ClickHouseClient.InsertBinaryAsync` (POCO or `object[]`), never the `[Obsolete]` `ClickHouseBulkCopy`; batch size and parallelism are profile policy, not per-call magic numbers.
- a non-transactional backend: the store profile records ClickHouse inserts as idempotent append blocks keyed by `InsertOptions.QueryId`, not under a transaction scope.
- the column type-system (`ClickHouse.Driver.Types.*`) is internal — the profile declares column types in DDL or `[ClickHouseColumn(Type=...)]`, never a CLR type instance.

[STACKING]:
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner crosses into a ClickHouse column through one rail — the `api-thinktecture-serialization` factory projects the owner to its key, and an attributed POCO field (`[ClickHouseColumn]`) carries that key into `InsertBinaryAsync<T>`; the inverse decodes the `ClickHouseDataReader` column back through the same factory. No hand-rolled column mapping.
- temporal columns: ClickHouse `DateTime64`/`Date32` columns bind through the transitive `NodaTime@3.2.3` (the substrate `NodaTime` row) — an `Instant`/`ZonedDateTime` owner projects to the column value, consistent with the rest of the time-identity seam (`api-nodatime`).
- columnar interchange: a Parquet file written by `ParquetSharp` (`api-parquetsharp`) or an Arrow batch (`api-arrow`) ingests through `InsertRawStreamAsync(table, stream, "Parquet"/"ArrowStream", ...)` — ClickHouse decodes the columnar format server-side; the managed side streams the pre-framed body, never re-serializes row-by-row.
- telemetry: the `const ClickHouseDiagnosticsOptions.ActivitySourceName = "ClickHouse.Driver"` names the source the AppHost OpenTelemetry tracer subscribes (`telemetry` port); each query/insert opens an `ActivityKind.Client` span tagged with `db.system=clickhouse`, the redacted connection string, and the `QueryStats` projected as `db.clickhouse.*` metrics. The process-global static toggles `ClickHouseDiagnosticsOptions.IncludeSqlInActivityTags` + `StatementMaxLength` (default 300) gate SQL-statement capture once at composition, not per-call. No bespoke logger competes with the span.
- fault rail: `ClickHouseServerException : DbException` lifts at the client edge discriminated on its numeric `ErrorCode` (parsed from the server error text) and carries the offending `Query`, joining the store-profile failure rail rather than surfacing as a raw `HttpRequestException`.

[RAIL_LAW]:
- Package: `ClickHouse.Driver`
- Owns: HTTP-interface columnar OLAP query, parallel RowBinary bulk ingest, ADO.NET provider mirror, `QueryStats` throughput receipt, and `ActivitySource` diagnostics
- Accept: thread-safe `ClickHouseClient` reuse, `InsertBinaryAsync` bulk ingest with profile-declared batch/parallelism, `QueryStats`-driven throughput receipts, `ActivitySource` telemetry through the AppHost tracer, and `ClickHouseServerException` fault discrimination on `ErrorCode`
- Reject: the `[Obsolete]` `ClickHouseBulkCopy`, per-call `HttpClient` construction (use a pooled factory), parallel bulk insert under an enabled session, transaction-scoped writes (the backend has none), and surfacing the internal `ClickHouse.Driver.Types` type-system as a consumer API
