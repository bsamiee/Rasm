# [RASM_PERSISTENCE_API_ARROW_EGRESS]

`Apache.Arrow.Adbc` drives driver-based query execution over Arrow streams; `Apache.Arrow.Flight` carries the Flight RPC `RecordBatch` transport over gRPC, `Apache.Arrow.Flight.Sql` folds a SQL dialect over it, `Apache.Arrow.Flight.AspNetCore` binds a served node onto an ASP.NET Core gRPC endpoint, and `Apache.Arrow.Compression` binds the IPC LZ4-frame and Zstandard codec factory. Persistence composes the five with the core columnar substrate whose member truth is the branch catalogue (`libs/dotnet/.api/api-arrow.md`) — `RecordBatch`, `Schema`, the IPC readers/writers, and `IArrowArrayStream`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc`
- package: `Apache.Arrow.Adbc` (Apache-2.0)
- assembly: `Apache.Arrow.Adbc`
- namespace: `Apache.Arrow.Adbc`
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Flight`
- package: `Apache.Arrow.Flight` (Apache-2.0)
- assembly: `Apache.Arrow.Flight`
- namespace: `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Client`, `Apache.Arrow.Flight.Server`
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Flight.Sql`
- package: `Apache.Arrow.Flight.Sql` (Apache-2.0)
- assembly: `Apache.Arrow.Flight.Sql`
- namespace: `Apache.Arrow.Flight.Sql`, `Apache.Arrow.Flight.Sql.Client`
- depends: `Apache.Arrow.Flight`
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Flight.AspNetCore`
- package: `Apache.Arrow.Flight.AspNetCore` (Apache-2.0)
- assembly: `Apache.Arrow.Flight.AspNetCore`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.AspNetCore.Builder`
- depends: `Apache.Arrow.Flight`, `Grpc.AspNetCore.Server`
- asset: two static extension classes and nothing else; it holds the SOLE `Apache.Arrow.Flight` `InternalsVisibleTo` grant, so no peer assembly reaches the server adapter
- rail: analytical-egress (host binding)

[PACKAGE_SURFACE]: `Apache.Arrow.Compression`
- package: `Apache.Arrow.Compression` (Apache-2.0)
- assembly: `Apache.Arrow.Compression`
- namespace: `Apache.Arrow.Compression`
- asset: pure-managed AnyCPU, no native RID; the managed `K4os`/`ZstdSharp` transitives carry the codec bodies
- rail: analytical-egress (IPC compression)

[REGISTRATION]: `Apache.Arrow` — branch substrate at `libs/dotnet/.api/api-arrow.md`; the columnar format, builders, `IArrowType` system, IPC readers/writers, `IpcOptions`, and `IArrowArrayStream` resolve there, and this file adds only the five egress packages above.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ADBC family

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :------------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `AdbcDriver`                     | driver root      | loads and creates databases                           |
|  [02]   | `AdbcDatabase`                   | database root    | opens connections                                     |
|  [03]   | `AdbcConnection`                 | connection root  | statements, schema ops, transactions, partitions      |
|  [04]   | `AdbcConnection.GetObjectsDepth` | nested enum      | `All` \| `Catalogs` \| `DbSchemas` \| `Tables` filter |
|  [05]   | `AdbcStatement`                  | statement root   | SQL or Substrait queries, updates, prepared bind      |
|  [06]   | `QueryResult`                    | result value     | `long RowCount` + `IArrowArrayStream? Stream`         |
|  [07]   | `UpdateResult`                   | result value     | `long AffectedRows` row-count receipt                 |
|  [08]   | `PartitionedResult`              | result value     | partition descriptors for distributed reads           |
|  [09]   | `PartitionDescriptor`            | partition handle | opaque partition token for `ReadPartition`            |
|  [10]   | `BulkIngestMode`                 | ingest enum      | `Create` \| `Append` \| `Replace` \| `CreateAppend`   |
|  [11]   | `AdbcException`                  | ADBC failure     | typed ADBC error with `AdbcStatusCode`                |
|  [12]   | `AdbcInfoCode`                   | info enum        | driver info code identifiers for `GetInfo`            |
|  [13]   | `AdbcOptions`                    | options value    | generic key-value option map                          |

[PUBLIC_TYPE_SCOPE]: Flight family

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `FlightClient`                         | client root     | gRPC client for Flight protocol                    |
|  [02]   | `FlightInfo`                           | info message    | schema, `Endpoints`, `TotalRecords`, `TotalBytes`  |
|  [03]   | `FlightDescriptor`                     | descriptor      | `CreatePathDescriptor` / `CreateCommandDescriptor` |
|  [04]   | `FlightDescriptorType`                 | descriptor enum | path vs. command discriminant                      |
|  [05]   | `FlightEndpoint`                       | endpoint        | carries the `Ticket` plus location list            |
|  [06]   | `FlightTicket`                         | ticket          | opaque token passed to `GetStream`                 |
|  [07]   | `FlightCriteria`                       | discovery input | optional `ListFlights` filter                      |
|  [08]   | `FlightData`                           | data message    | carries `RecordBatch` on the wire                  |
|  [09]   | `FlightAction`                         | action message  | opaque action request (`Type` + `Body`)            |
|  [10]   | `FlightActionType`                     | action type     | describes an available action                      |
|  [11]   | `FlightResult`                         | action result   | per-action `DoAction` result body                  |
|  [12]   | `FlightRecordBatchStreamingCall`       | call handle     | streaming read call handle (`ResponseStream`)      |
|  [13]   | `FlightRecordBatchDuplexStreamingCall` | call handle     | `StartPut` write call handle (`RequestStream`)     |
|  [14]   | `FlightRecordBatchExchangeCall`        | call handle     | bidirectional exchange call handle                 |

[PUBLIC_TYPE_SCOPE]: Flight server family (`Apache.Arrow.Flight.Server`)
- `FlightServer` is `abstract`; a served node overrides its `virtual` verbs (`GetFlightInfo`/`GetSchema`/`DoGet`/`DoPut`/`DoExchange`/`ListFlights`/`ListActions`/`DoAction`/`Handshake`), each throwing `NotImplementedException` until overridden.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `FlightServer`                        | server base   | the abstract serve root; a subclass overrides its `virtual` verbs              |
|  [02]   | `FlightServerRecordBatchStreamWriter` | server writer | `: IServerStreamWriter<RecordBatch>`; the `DoGet`/`DoExchange` response stream |
|  [03]   | `FlightServerRecordBatchStreamReader` | server reader | the `DoExchange`/`DoPut` request stream; `FlightDescriptor` resolves it        |
|  [04]   | `FlightRecordBatchStreamWriter`       | writer base   | `abstract : IAsyncStreamWriter<RecordBatch>`; the writer base                  |
|  [05]   | `FlightRecordBatchStreamReader`       | reader base   | `abstract : IAsyncStreamReader<RecordBatch>`; the reader base                  |
|  [06]   | `FlightLocation`                      | location      | `FlightLocation(string uri)`; `string Uri` — the `FlightEndpoint` address      |

[PUBLIC_TYPE_SCOPE]: Flight hosting family (`Apache.Arrow.Flight.AspNetCore`)
- both types are `static` extension holders seated in the host's own namespaces, so a `using` of the Arrow namespaces never surfaces them.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [CAPABILITY]                                   |
| :-----: | :-------------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `FlightIGrpcServerBuilderExtensions`    | DI extension      | binds a `FlightServer` subclass to the adapter |
|  [02]   | `FlightIEndpointRouteBuilderExtensions` | routing extension | maps the adapter as a gRPC service             |

[PUBLIC_TYPE_SCOPE]: Flight SQL family (`Apache.Arrow.Flight.Sql`, `Apache.Arrow.Flight.Sql.Client`)
- every `FlightSqlClient` verb takes a trailing `FlightCallOptions?` + `CancellationToken`, and each metadata verb pairs with a `*SchemaAsync` sibling returning the result `Schema`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :------------------ | :----------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `FlightSqlClient`   | client root        | SQL query/update/prepare, `DoGet`/`DoPut` stream, metadata, and transaction verbs        |
|  [02]   | `FlightSqlServer`   | server base        | `abstract : FlightServer`; decodes the SQL command protobufs and routes `DoGet`          |
|  [03]   | `PreparedStatement` | prepared handle    | `: IDisposable, IAsyncDisposable`; binds a parameter `RecordBatch` and executes          |
|  [04]   | `Transaction`       | transaction handle | `readonly struct : IEquatable<Transaction>`; `NoTransaction`, `IsValid`, `TransactionId` |
|  [05]   | `TableRef`          | table reference    | `Catalog?`/`DbSchema`/`Table` key the key-discovery verbs take                           |
|  [06]   | `FlightCallOptions` | call options       | per-call `Metadata Headers` + `TimeSpan Timeout`                                         |
|  [07]   | `DoPutResult`       | put result         | `Writer` + `Reader`; `ReadMetadataAsync`/`CompleteAsync` finalize an ingest              |

[FLIGHTSQLSERVER_SUBCLASS_COST]: `FlightSqlServer` declares 28 `protected abstract` handlers and no base implementation among them, so a subclass realizes every one whatever it serves — twelve `Get*FlightInfo` describe verbs, eleven `DoGet*` stream verbs, the `CreatePreparedStatement`/`ClosePreparedStatement` action pair, and the three `Put*` ingest verbs. It seals nothing and overrides `GetFlightInfo`, `DoGet`, `DoAction`, `DoPut`, and `ListActions` from `FlightServer`, each dispatching on a Flight SQL command protobuf it unpacks from the descriptor or ticket.
[SUBSTRAIT_COMMAND_UNROUTED]: `Arrow.Flight.Protocol.Sql` generates `CommandStatementSubstraitPlan { Plan = 1, TransactionId = 2 }` over `SubstraitPlan { Plan = 1, Version = 2 }`, and the `FlightSqlServer` dispatch matches NEITHER — its `GetCommand`, `GetFlightInfo`, and `DoGet` folds enumerate `CommandStatementQuery`, the catalog-metadata commands, and the prepared-statement pair alone, throwing `InvalidOperationException` on anything else. Plan-carrying servers override `GetFlightInfo` regardless, so `Query/federation#FLIGHT_RESULT_PLANE` serves a plain `FlightServer`.

[PUBLIC_TYPE_SCOPE]: IPC compression family (`Apache.Arrow.Compression`)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `CompressionCodecFactory` | codec factory | the only public type, `sealed : ICompressionCodecFactory`; `Lz4Frame`/`Zstd` codecs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IPC compression enable
- `CompressionLevel` (`int?`) forwards to `CreateCodec(type, level)`, called per batch.

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `new CompressionCodecFactory()`                                   | ctor    | assign to `IpcOptions.CompressionCodecFactory` to enable |
|  [02]   | `CompressionCodecFactory.CreateCodec(CompressionCodecType, int?)` | factory | `-> ICompressionCodec`, obtained per batch               |

[ENTRYPOINT_SCOPE]: ADBC statement execution
- rows [03]–[09] are `AdbcConnection` members; rows [10]–[15] are `AdbcStatement` members.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]        | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `AdbcDriver.Open(parameters)`                                               | driver open    | creates `AdbcDatabase`                    |
|  [02]   | `AdbcDatabase.Connect(options)`                                             | connect        | creates `AdbcConnection`                  |
|  [03]   | `CreateStatement()`                                                         | factory        | creates `AdbcStatement`                   |
|  [04]   | `BulkIngest(targetTable, BulkIngestMode)`                                   | ingest factory | ingest statement (+5-arg overload)        |
|  [05]   | `GetObjects(GetObjectsDepth, catalog?, dbSchema?, table?, types?, column?)` | schema query   | returns `IArrowArrayStream`               |
|  [06]   | `GetTableSchema(catalog?, dbSchema?, table)`                                | schema         | returns `Schema`                          |
|  [07]   | `GetTableTypes()` / `GetInfo(IReadOnlyList<AdbcInfoCode>)`                  | schema query   | returns `IArrowArrayStream`               |
|  [08]   | `AutoCommit` / `Commit()` / `Rollback()`                                    | transaction    | autocommit off; bounds a statement unit   |
|  [09]   | `ReadPartition(PartitionDescriptor)`                                        | partition read | reads one `PartitionedResult` partition   |
|  [10]   | `SqlQuery` / `SubstraitPlan`                                                | property       | SQL text or a Substrait `byte[]` plan     |
|  [11]   | `ExecuteQuery()` / `ExecuteQueryAsync()`                                    | execute        | `QueryResult` (`RowCount` + `Stream`)     |
|  [12]   | `ExecuteUpdate()` / `ExecuteUpdateAsync()`                                  | update         | `UpdateResult` (`AffectedRows`)           |
|  [13]   | `ExecutePartitioned()`                                                      | partitioned    | `PartitionedResult` for distributed reads |
|  [14]   | `Prepare()`                                                                 | prepare        | prepares statement server-side            |
|  [15]   | `Bind(batch, schema)` / `BindStream(IArrowArrayStream)`                     | bind           | binds one batch or a whole stream         |
|  [16]   | `PartitionedResult.Schema`                                                  | property       | the schema every partition shares         |
|  [17]   | `PartitionedResult.AffectedRows`                                            | property       | driver-reported count; `-1` when unknown  |
|  [18]   | `PartitionedResult.PartitionDescriptors`                                    | property       | `IReadOnlyList<PartitionDescriptor>`      |
|  [19]   | `PartitionDescriptor.Descriptor`                                            | property       | `ReadOnlySpan<byte>` opaque server token  |

[ENTRYPOINT_SCOPE]: Flight client operations
- rows [02]–[10] are `FlightClient` instance members.

| [INDEX] | [SURFACE]                                    | [SHAPE]        | [CAPABILITY]                                             |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new FlightClient(ChannelBase\|CallInvoker)` | ctor           | client from a gRPC channel or invoker                    |
|  [02]   | `GetInfo(descriptor)`                        | info query     | `AsyncUnaryCall<FlightInfo>` for a descriptor            |
|  [03]   | `GetSchema(descriptor)`                      | schema query   | `AsyncUnaryCall<Schema>` for a descriptor                |
|  [04]   | `GetStream(FlightTicket)`                    | stream read    | `FlightRecordBatchStreamingCall` (use `endpoint.Ticket`) |
|  [05]   | `StartPut(descriptor[, schema])`             | stream write   | `FlightRecordBatchDuplexStreamingCall` write path        |
|  [06]   | `DoExchange(descriptor)`                     | exchange       | `FlightRecordBatchExchangeCall` bidirectional call       |
|  [07]   | `DoAction(FlightAction)`                     | action call    | `AsyncServerStreamingCall<FlightResult>`                 |
|  [08]   | `ListActions()`                              | discovery      | `AsyncServerStreamingCall<FlightActionType>`             |
|  [09]   | `ListFlights(FlightCriteria?)`               | discovery      | `AsyncServerStreamingCall<FlightInfo>`                   |
|  [10]   | `Handshake()`                                | auth handshake | `AsyncDuplexStreamingCall` handshake exchange            |

[ENTRYPOINT_SCOPE]: Flight server verbs (`Apache.Arrow.Flight.Server`)
- every verb is `override` and takes a trailing `ServerCallContext`.

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `Task<FlightInfo> GetFlightInfo(FlightDescriptor)`                                          | one dataset per descriptor command bytes  |
|  [02]   | `Task<Schema> GetSchema(FlightDescriptor)`                                                  | dataset schema for a descriptor           |
|  [03]   | `Task DoGet(FlightTicket, FlightServerRecordBatchStreamWriter)`                             | streams `RecordBatch` per redeemed ticket |
|  [04]   | `Task DoExchange(FlightServerRecordBatchStreamReader, FlightServerRecordBatchStreamWriter)` | full-duplex incremental delta channel     |
|  [05]   | `Task DoPut(FlightServerRecordBatchStreamReader, IAsyncStreamWriter<FlightPutResult>)`      | client→server batch ingest                |

[ENTRYPOINT_SCOPE]: Flight server response/request streams
- members on `FlightServerRecordBatchStreamWriter` / `FlightServerRecordBatchStreamReader`.

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                       |
| :-----: | :--------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `WriteAsync(RecordBatch[, ByteString applicationMetadata])`      | writes one batch; auto-`SetupStream(batch.Schema)` on first write  |
|  [02]   | `SetupStream(Schema)` / `WriteOptions`                           | emits the schema message before the first batch; IPC write options |
|  [03]   | `await …StreamReader.FlightDescriptor`                           | `ValueTask<FlightDescriptor>` resolves the `DoExchange` descriptor |
|  [04]   | `MoveNextAsync()` / `Current` / `Schema` / `ApplicationMetadata` | reads the inbound `RecordBatch` request stream                     |

[ENTRYPOINT_SCOPE]: Flight server hosting (`Apache.Arrow.Flight.AspNetCore`)
- `AddFlightServer<T>` extends `IGrpcServerBuilder`, what `services.AddGrpc()` returns, never `IServiceCollection`; `MapFlightEndpoint` extends `IEndpointRouteBuilder` and takes NO type argument.

| [INDEX] | [SURFACE]                                     | [SHAPE]         | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `AddFlightServer<T>() where T : FlightServer` | DI registration | -> `IGrpcServerBuilder`; body `AddScoped<FlightServer, T>()` |
|  [02]   | `MapFlightEndpoint()`                         | endpoint map    | -> `GrpcServiceEndpointConventionBuilder`; maps the adapter  |

[ENTRYPOINT_SCOPE]: Flight message types
- `FlightInfo` ctor overloads add optional `long totalRecords`/`totalBytes` (default `-1`).

| [INDEX] | [SURFACE]                                                                     | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `new FlightInfo(Schema, FlightDescriptor, IReadOnlyList<FlightEndpoint>)`     | discovery payload; `TotalRecords`/`TotalBytes` props |
|  [02]   | `new FlightEndpoint(FlightTicket, IReadOnlyList<FlightLocation>)`             | `Ticket` + `Locations` for one endpoint              |
|  [03]   | `new FlightTicket(string)` / `(ByteString)` / `(byte[])`; `ByteString Ticket` | the opaque `DoGet` redemption token                  |
|  [04]   | `FlightDescriptor.Command` / `Paths` / `Type`                                 | command bytes, paths, `FlightDescriptorType`         |

[ENTRYPOINT_SCOPE]: Flight SQL client operations (`Apache.Arrow.Flight.Sql.Client`)
- `new FlightSqlClient(FlightClient)` wraps a constructed `FlightClient`; `PreparedStatement` carries `SetParameters(RecordBatch)`/`ExecuteAsync`/`ExecuteUpdateAsync(RecordBatch)`/`CloseAsync`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]       | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `ExecuteAsync(query, Transaction)`                                | query         | `Task<FlightInfo>` for a SQL query         |
|  [02]   | `ExecuteUpdateAsync(query, Transaction)`                          | update        | `Task<long>` affected-row count            |
|  [03]   | `PrepareAsync(query, Transaction)`                                | prepare       | `Task<PreparedStatement>` server handle    |
|  [04]   | `DoGetAsync(FlightTicket)`                                        | stream read   | `IAsyncEnumerable<RecordBatch>` per ticket |
|  [05]   | `DoPutAsync(FlightDescriptor, RecordBatch)`                       | stream write  | `Task<FlightPutResult>` ingest             |
|  [06]   | `GetCatalogsAsync()` / `GetDbSchemasAsync(catalog?, pat?)`        | metadata      | catalog and schema discovery               |
|  [07]   | `GetTablesAsync(catalog?, dbSchemaPat?, tablePat?)`               | metadata      | table discovery                            |
|  [08]   | `GetPrimaryKeysAsync` / exported / imported keys                  | metadata      | key discovery over a `TableRef`            |
|  [09]   | `GetCrossReferenceAsync(TableRef pk, TableRef fk)`                | metadata      | cross-reference discovery                  |
|  [10]   | `GetTableTypesAsync` / `GetXdbcTypeInfoAsync` / `GetSqlInfoAsync` | metadata      | type and driver-info discovery             |
|  [11]   | `BeginTransactionAsync()`                                         | transaction   | `Task<Transaction>` opens a unit           |
|  [12]   | `CommitAsync(Transaction)` / `RollbackAsync(Transaction)`         | transaction   | `AsyncServerStreamingCall<FlightResult>`   |
|  [13]   | `CancelFlightInfoAsync` / `CancelQueryAsync`                      | cancel        | `Task<FlightInfoCancelResult>`             |
|  [14]   | `DoActionAsync(FlightAction)` / `GetExecuteSchemaAsync`           | action/schema | action stream and result-schema probe      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AdbcConnection.GetObjectsDepth` discriminates `All`/`Catalogs`/`DbSchemas`/`Tables`; `AdbcStatement.SqlQuery` and `SubstraitPlan` are mutually-exclusive query inputs.
- `AdbcStatement` and `AdbcConnection` publish the WHOLE ADBC vocabulary as `virtual` bodies that throw `AdbcException.NotImplemented`, so member presence proves nothing about driver support — `ExecutePartitioned`, `ReadPartition`, `BulkIngest`, `Prepare`, `SubstraitPlan`, and `Cancel` each throw on a driver that declines them, and a composing rail lifts the call into its typed fault rather than reading the member as a capability.
- `PartitionDescriptor` is a struct whose `Descriptor` is a `ReadOnlySpan<byte>`, so the descriptor VALUE crosses a lambda or an await and its span never does.
- Flight SQL layers over the Flight transport, never a second listener: `FlightSqlServer` (`: FlightServer`) decodes the SQL command protobufs and reuses the `DoGet` ticket redemption. Its serve side is a SQL-catalog contract, so a plan-carrying result plane subclasses `FlightServer` directly and its client side stays fully composable — `FlightSqlClient` over a constructed `FlightClient` reaches any served node.
- `FlightServer` is a bare `abstract class` carrying NO `[BindServiceMethod]`; that attribute sits on `Apache.Arrow.Flight.Protocol.FlightService.FlightServiceBase`, which the subclass hierarchy never reaches. `MapGrpcService<TSubclass>()` therefore resolves no binder and fails at startup, and the served node reaches gRPC only indirectly — DI-resolved AS `FlightServer` into the `internal FlightServerImplementation : FlightService.FlightServiceBase` adapter that `Apache.Arrow.Flight.AspNetCore` alone constructs under the `InternalsVisibleTo` grant.

[STACKING]:
- `Apache.Arrow`(`libs/dotnet/.api/api-arrow.md`): the core columnar substrate every leg yields into — `QueryResult.Stream`, the Flight `RecordBatch` wire, and the compression factory all meet the branch-owned `IArrowArrayStream`/`IpcOptions` contracts.
- `api-duckdb`(`.api/api-duckdb.md`): the DuckDB ADBC driver is the in-process analytical engine reached through this `AdbcConnection`/`AdbcStatement` surface, so a federated rail dispatches SQL or a `SubstraitPlan` and reads back one `IArrowArrayStream` — `ExecutePartitioned` + `ReadPartition` fan a large scan, `BulkIngest` lands a `RecordBatch` stream.
- `api-parquetsharp`(`.api/api-parquetsharp.md`) + `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): the BIM analytics star schema (columnar tables in a `Parquet.Net` Brotli `.parquet`-zip) reads into `RecordBatch` streams through `ParquetSharp.Arrow` and queries over the same DuckDB-ADBC path, entering this egress as one `IArrowArrayStream` without re-encoding.
- `api-lz4`(`.api/api-lz4.md`): the Arrow-IPC buffer codec through `CompressionCodecFactory` is distinct from the snapshot-codec LZ4 rail driving `LZ4Pickler`/`CompressionPolicy` over `K4os.Compression.LZ4` for standalone snapshot/blob frames.
- within-lib: the Persistence egress owner folds IPC, ADBC, and Flight behind one `IArrowArrayStream`; `Query/federation#FLIGHT_RESULT_PLANE` serves a plain `FlightServer` as the READ end of the lake, never a landing door.

[LOCAL_ADMISSION]:
- IPC compression sets `IpcOptions.CompressionCodec` (`Lz4Frame` or `Zstd`) AND `IpcOptions.CompressionCodecFactory = new Apache.Arrow.Compression.CompressionCodecFactory()`, optionally `IpcOptions.CompressionLevel`; the codec enum alone is inert and the egress owner never hand-rolls an `ICompressionCodecFactory`.
- ADBC drivers load via `AdbcDriver.Open(parameters)` then `AdbcDatabase.Connect(options)`; direct `AdbcConnection` construction is not the public path.
- `FlightClient` constructs from a gRPC `ChannelBase`/`CallInvoker` (no static factory), connection lifetime/TLS/credentials caller-owned; a Flight read is `GetInfo` → pick a `FlightEndpoint` → `GetStream(endpoint.Ticket)`, a write is `StartPut(descriptor, schema)` then batches on the duplex `RequestStream`.
- Flight SQL rides that one served node over a single gRPC listener: `FlightSqlServer` reuses the `DoGet` ticket redemption, `PreparedStatement` binds a parameter `RecordBatch`, and a `Transaction` bounds a commit/rollback unit.
- Hosting a served node is exactly two host calls — `services.AddGrpc().AddFlightServer<TServer>()` binds the subclass scoped, then `app.MapFlightEndpoint()` maps the adapter; the subclass's own constructor dependencies resolve from the same container because the adapter takes `FlightServer` by injection.

[RAIL_LAW]:
- Packages: `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.AspNetCore`, `Apache.Arrow.Flight.Sql`, `Apache.Arrow.Compression`
- Owns: ADBC query execution (partitioned, transactional, Substrait), Flight `RecordBatch` transport, its ASP.NET Core host binding, the Flight SQL dialect over it, and the IPC LZ4-frame/Zstandard codec factory
- Accept: ADBC driver-level queries and bulk ingest read back as `IArrowArrayStream`, Flight `GetStream`/`StartPut`/`DoExchange`, `AddFlightServer<T>` beside `MapFlightEndpoint` for the served node, IPC compression through the package factory
- Reject: a custom `ICompressionCodecFactory` where `Apache.Arrow.Compression.CompressionCodecFactory` owns both codecs; `CompressionCodec` set without a factory; raw gRPC Flight Protobuf without `FlightClient`; `MapGrpcService<T>` over a `FlightServer` subclass, which carries no bind attribute; a hand-written `FlightService.FlightServiceBase` adapter where the grant-holding package owns the only reachable one; a per-transport reader where `IArrowArrayStream` unifies IPC, ADBC, and Flight; a core-Arrow member re-tabled here instead of the branch catalogue
