# [RASM_PERSISTENCE_API_ARROW]

`Apache.Arrow` supplies the in-memory columnar format, schema and field model,
typed array families, `RecordBatch` construction, IPC file and stream serialisation,
and the `IArrowArrayStream` contract for Persistence analytical egress. `Apache.Arrow.Adbc`
provides the Arrow Database Connectivity abstraction: `AdbcConnection`, `AdbcStatement`,
`AdbcDatabase`, and `AdbcDriver` for driver-based query execution over Arrow streams,
including the partitioned-result, Substrait-plan, and transaction surfaces. `Apache.Arrow.Flight`
supplies the Flight RPC data-transfer protocol: descriptor, ticket, endpoint, and info
messages plus the client-side `FlightClient` for reading and writing `RecordBatch` streams
over gRPC. `Apache.Arrow.Flight.Sql` layers the Flight SQL dialect over that transport via `FlightSqlClient` SQL/metadata/transaction verbs and the `FlightSqlServer` (`: FlightServer`) served-node base. `Apache.Arrow.Compression` supplies the concrete `Apache.Arrow.Ipc.ICompressionCodecFactory`
(`CompressionCodecFactory`) that backs IPC-stream compression for the `Lz4Frame` and `Zstd` codecs.

ABI floor (consumer `net10.0`): `Apache.Arrow`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Sql`, and
`Apache.Arrow.Compression` ride one release line; `Apache.Arrow.Adbc` versions independently as a pre-1.0
contract. The bound asset is `lib/net8.0`, the highest TFM shipped, and every one of these packages is Apache-2.0. The
`Apache.Arrow.Compression` closure is pure-managed AnyCPU with no native RID asset: its transitives
`K4os.Compression.LZ4.Streams` (over `K4os.Compression.LZ4` + `K4os.Hash.xxHash`) and `ZstdSharp.Port`
(a managed Zstandard port) carry the codec bodies. The `ICompressionCodecFactory` interface ships in
core `Apache.Arrow` (`Apache.Arrow.Ipc`); the concrete LZ4-frame/ZSTD codec implementation ships in
`Apache.Arrow.Compression` (admitted), so enabling IPC compression sets
`IpcOptions.CompressionCodec = CompressionCodecType.Lz4Frame | Zstd` together with
`IpcOptions.CompressionCodecFactory = new Apache.Arrow.Compression.CompressionCodecFactory()`. Setting
`IpcOptions.CompressionCodec` without `CompressionCodecFactory` throws at write.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow`
- package: `Apache.Arrow`
- assembly: `Apache.Arrow`
- namespace: `Apache.Arrow`, `Apache.Arrow.Ipc`, `Apache.Arrow.Types`, `Apache.Arrow.Memory`
- asset: runtime library
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Adbc`
- package: `Apache.Arrow.Adbc`
- assembly: `Apache.Arrow.Adbc`
- namespace: `Apache.Arrow.Adbc`
- asset: runtime library
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Flight`
- package: `Apache.Arrow.Flight`
- assembly: `Apache.Arrow.Flight`
- namespace: `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Client`
- asset: runtime library
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Flight.Sql`
- package: `Apache.Arrow.Flight.Sql`
- assembly: `Apache.Arrow.Flight.Sql`
- namespace: `Apache.Arrow.Flight.Sql`, `Apache.Arrow.Flight.Sql.Client`
- asset: runtime library over `Apache.Arrow.Flight`
- rail: analytical-egress

[PACKAGE_SURFACE]: `Apache.Arrow.Compression`
- package: `Apache.Arrow.Compression`
- license: Apache-2.0
- assembly: `Apache.Arrow.Compression`
- namespace: `Apache.Arrow.Compression`
- target: multi-target (`net462`, `netstandard2.0`, `net8.0`); the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed runtime library, AnyCPU, no native RID asset (transitives `K4os.Compression.LZ4.Streams` `1.3.8`, `ZstdSharp.Port` `0.8.8`)
- rail: analytical-egress (IPC compression)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record and schema family
- rail: analytical-egress

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [CAPABILITY]                        |
| :-----: | :-------------------- | :--------------- | :---------------------------------- |
|  [01]   | `RecordBatch`         | record container | columnar batch with schema          |
|  [02]   | `RecordBatch.Builder` | builder          | assembles typed arrays into a batch |
|  [03]   | `Schema`              | schema value     | ordered field list with metadata    |
|  [04]   | `Schema.Builder`      | builder          | assembles fields into schema        |
|  [05]   | `Field`               | field value      | name, type, nullability, metadata   |
|  [06]   | `Field.Builder`       | builder          | assembles field from parts          |
|  [07]   | `ChunkedArray`        | chunked array    | list of same-type arrays            |
|  [08]   | `Table`               | table value      | schema plus chunked column list     |
|  [09]   | `IArrowArray`         | array contract   | columnar array capability           |
|  [10]   | `IArrowRecord`        | record contract  | schema-plus-arrays capability       |

[PUBLIC_TYPE_SCOPE]: primitive array family
- rail: analytical-egress

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                   |
| :-----: | :---------------- | :------------- | :----------------------------- |
|  [01]   | `BooleanArray`    | bool array     | validity-bitmap boolean values |
|  [02]   | `Int8Array`       | integer array  | signed 8-bit values            |
|  [03]   | `Int16Array`      | integer array  | signed 16-bit values           |
|  [04]   | `Int32Array`      | integer array  | signed 32-bit values           |
|  [05]   | `Int64Array`      | integer array  | signed 64-bit values           |
|  [06]   | `UInt8Array`      | integer array  | unsigned 8-bit values          |
|  [07]   | `UInt16Array`     | integer array  | unsigned 16-bit values         |
|  [08]   | `UInt32Array`     | integer array  | unsigned 32-bit values         |
|  [09]   | `UInt64Array`     | integer array  | unsigned 64-bit values         |
|  [10]   | `FloatArray`      | float array    | 32-bit float values            |
|  [11]   | `DoubleArray`     | float array    | 64-bit double values           |
|  [12]   | `Decimal128Array` | decimal array  | 128-bit fixed-point values     |
|  [13]   | `Decimal256Array` | decimal array  | 256-bit fixed-point values     |
|  [14]   | `StringArray`     | binary array   | UTF-8 string values            |
|  [15]   | `BinaryArray`     | binary array   | opaque byte sequences          |
|  [16]   | `TimestampArray`  | temporal array | epoch-nanosecond timestamps    |
|  [17]   | `Date32Array`     | temporal array | days-since-epoch dates         |
|  [18]   | `DurationArray`   | temporal array | duration values                |

[PUBLIC_TYPE_SCOPE]: type system and IPC family
- rail: analytical-egress

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                |
| :-----: | :------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ArrowType`                | type base       | root type for all Arrow types                                               |
|  [02]   | `ArrowTypeId`              | type enum       | discriminates Arrow type identities                                         |
|  [03]   | `IArrowType`               | type contract   | minimal type contract                                                       |
|  [04]   | `ArrowStreamReader`        | IPC reader      | reads Arrow IPC stream format                                               |
|  [05]   | `ArrowStreamWriter`        | IPC writer      | writes Arrow IPC stream format                                              |
|  [06]   | `ArrowFileReader`          | IPC reader      | reads Arrow IPC file format (random-access footer)                          |
|  [07]   | `ArrowFileWriter`          | IPC writer      | writes Arrow IPC file format                                                |
|  [08]   | `IArrowReader`             | reader contract | shared sync/async reader contract                                           |
|  [09]   | `IpcOptions`               | IPC policy      | codec + level + legacy-format flags                                         |
|  [10]   | `CompressionCodecType`     | codec enum      | `Lz4Frame` \| `Zstd` (the only two members)                                 |
|  [11]   | `ICompressionCodecFactory` | codec factory   | `CreateCodec(type[, level])`; concrete impl in `Apache.Arrow.Compression`   |
|  [12]   | `ICompressionCodec`        | codec contract  | `Decompress(ReadOnlyMemory<byte>, Memory<byte>)`; `Compress` default-throws |
|  [13]   | `IArrowArrayStream`        | stream contract | async enumerable of record batches; `Schema` + `ReadNextRecordBatchAsync`   |

[PUBLIC_TYPE_SCOPE]: ADBC family
- rail: analytical-egress

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
- rail: analytical-egress

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
- rail: analytical-egress
- `FlightServer` is `abstract`; the served node subclasses it and overrides the nine `virtual` verbs (`GetFlightInfo`/`GetSchema`/`DoGet`/`DoPut`/`DoExchange`/`ListFlights`/`ListActions`/`DoAction`/`Handshake`), each throwing `NotImplementedException` until overridden.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `FlightServer`                        | server base   | the abstract serve root; subclass overrides the nine `virtual` verbs (above)   |
|  [02]   | `FlightServerRecordBatchStreamWriter` | server writer | `: IServerStreamWriter<RecordBatch>`; the `DoGet`/`DoExchange` response stream |
|  [03]   | `FlightServerRecordBatchStreamReader` | server reader | the `DoExchange`/`DoPut` request stream; `FlightDescriptor` resolves it        |
|  [04]   | `FlightRecordBatchStreamWriter`       | writer base   | `abstract : IAsyncStreamWriter<RecordBatch>` — the writer base ([03] members)  |
|  [05]   | `FlightRecordBatchStreamReader`       | reader base   | `abstract : IAsyncStreamReader<RecordBatch>` — the reader base ([03] members)  |
|  [06]   | `FlightLocation`                      | location      | `FlightLocation(string uri)`; `string Uri` — the `FlightEndpoint` address      |

[PUBLIC_TYPE_SCOPE]: Flight SQL family (`Apache.Arrow.Flight.Sql`, `Apache.Arrow.Flight.Sql.Client`)
- rail: analytical-egress
- every `FlightSqlClient` verb takes a trailing `FlightCallOptions?` + `CancellationToken`, and each metadata verb pairs with a `*SchemaAsync` sibling returning the result `Schema`; the verbs enumerate in the entrypoint scope below.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                             |
| :-----: | :------------------ | :----------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `FlightSqlClient`   | client root        | SQL query/update/prepare, `DoGet`/`DoPut` stream, metadata, and transaction verbs        |
|  [02]   | `FlightSqlServer`   | server base        | `abstract : FlightServer`; decodes the SQL command protobufs and routes `DoGet`          |
|  [03]   | `PreparedStatement` | prepared handle    | `: IDisposable, IAsyncDisposable`; binds a parameter `RecordBatch` and executes          |
|  [04]   | `Transaction`       | transaction handle | `readonly struct : IEquatable<Transaction>`; `NoTransaction`, `IsValid`, `TransactionId` |
|  [05]   | `TableRef`          | table reference    | `Catalog?`/`DbSchema`/`Table` key the key-discovery verbs take                           |
|  [06]   | `FlightCallOptions` | call options       | per-call `Metadata Headers` + `TimeSpan Timeout`                                         |
|  [07]   | `DoPutResult`       | put result         | `Writer` + `Reader`; `ReadMetadataAsync`/`CompleteAsync` finalize an ingest              |

[PUBLIC_TYPE_SCOPE]: IPC compression family (`Apache.Arrow.Compression`)
- rail: analytical-egress (IPC compression)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `CompressionCodecFactory` | codec factory | the only public type, `sealed : ICompressionCodecFactory`; `Lz4Frame`/`Zstd` codecs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: RecordBatch and Schema construction
- rail: analytical-egress

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [00]   | `StringArray.Builder().Append(string)/.AppendRange(IEnumerable<string>)/.Build()` | array builder  | builds a UTF-8 `StringArray` column (null appends as validity-bitmap null) |
|  [01]   | `RecordBatch.Builder(allocator?)`                                                 | ctor           | creates batch builder with allocator                                       |
|  [02]   | `RecordBatch.Builder.Append(name, nullable, array)`                               | builder        | adds typed column to batch                                                 |
|  [03]   | `RecordBatch.Builder.Append(name, nullable, builder)`                             | builder        | adds built column                                                          |
|  [04]   | `RecordBatch.Builder.Append(batch)`                                               | builder        | merges schema and arrays from a batch                                      |
|  [05]   | `RecordBatch.Builder.Build()`                                                     | factory call   | yields immutable `RecordBatch`                                             |
|  [06]   | `RecordBatch.Builder.Clear()`                                                     | reset          | resets schema and arrays                                                   |
|  [07]   | `Schema.Builder.Field(field)`                                                     | builder        | adds field to schema                                                       |
|  [08]   | `Schema.Builder.Build()`                                                          | factory call   | yields immutable `Schema`                                                  |
|  [09]   | `Field.Builder.Name(name)`                                                        | builder        | sets field name                                                            |
|  [10]   | `Field.Builder.DataType(type)`                                                    | builder        | sets Arrow type                                                            |
|  [11]   | `Field.Builder.Nullable(nullable)`                                                | builder        | sets nullability                                                           |
|  [12]   | `Field.Builder.Build()`                                                           | factory call   | yields immutable `Field`                                                   |
|  [13]   | `RecordBatch.Slice(offset, length)` / `SliceShared`                               | zero-copy view | windows a batch without copying buffers                                    |
|  [14]   | `RecordBatch.Column(name)` / `Column(int)`                                        | column access  | reads one `IArrowArray` column by name/index                               |

[ENTRYPOINT_SCOPE]: IPC read and write
- rail: analytical-egress

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `new ArrowStreamReader(stream, leaveOpen?)`                 | ctor           | opens IPC stream reader (`IArrowReader`)             |
|  [02]   | `new ArrowStreamWriter(stream, schema, leaveOpen, options)` | ctor           | opens IPC stream writer; `MemoryAllocator` overload  |
|  [03]   | `new ArrowFileReader(stream, leaveOpen?)`                   | ctor           | opens IPC file reader (random-access)                |
|  [04]   | `new ArrowFileWriter(stream, schema, leaveOpen, options)`   | ctor           | opens IPC file writer                                |
|  [05]   | `WriteStart()` / `WriteStartAsync()`                        | schema write   | emits the schema message before the first batch      |
|  [06]   | `ReadNextRecordBatch()`                                     | sync read      | reads next `RecordBatch`                             |
|  [07]   | `ReadNextRecordBatchAsync()`                                | async read     | async reads next `RecordBatch`                       |
|  [08]   | `WriteRecordBatch(batch)`                                   | sync write     | writes one `RecordBatch`                             |
|  [09]   | `WriteRecordBatchAsync(batch)`                              | async write    | async writes one `RecordBatch`                       |
|  [10]   | `WriteEnd()` / `WriteEndAsync()`                            | finalize       | writes IPC EOS terminator (mandatory before dispose) |

[ENTRYPOINT_SCOPE]: IPC compression enable
- rail: analytical-egress (IPC compression)
- `IpcOptions` carries `CompressionCodec`/`CompressionCodecFactory`/`CompressionLevel`/`WriteLegacyIpcFormat`; `CompressionCodec` is inert unless `CompressionCodecFactory` is set, and `CompressionLevel` (`int?`) forwards to `CreateCodec(type, level)`, called per batch.

| [INDEX] | [SURFACE]     | [SIGNATURE]                                                                                     |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | factory ctor  | `new Apache.Arrow.Compression.CompressionCodecFactory()` — assign to enable `Lz4Frame`/`Zstd`   |
|  [02]   | `CreateCodec` | `CompressionCodecFactory.CreateCodec(CompressionCodecType[, int? level])` → `ICompressionCodec` |

[ENTRYPOINT_SCOPE]: ADBC statement execution
- rail: analytical-egress
- the open→connect chain (`AdbcDriver.Open`→`AdbcDatabase.Connect`), then `AdbcConnection` members ([03]-[09]) and `AdbcStatement` members ([10]-[15]).

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                              |
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

[ENTRYPOINT_SCOPE]: Flight client operations
- rail: analytical-egress
- constructed from a gRPC `ChannelBase\|CallInvoker` (no static factory); rows [02]-[10] are `FlightClient` members.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                                             |
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
- rail: analytical-egress
- every verb is `override` and takes a trailing `ServerCallContext`; the remaining base verbs throw until overridden.

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `Task<FlightInfo> GetFlightInfo(FlightDescriptor)`                                          | one dataset per descriptor command bytes  |
|  [02]   | `Task<Schema> GetSchema(FlightDescriptor)`                                                  | dataset schema for a descriptor           |
|  [03]   | `Task DoGet(FlightTicket, FlightServerRecordBatchStreamWriter)`                             | streams `RecordBatch` per redeemed ticket |
|  [04]   | `Task DoExchange(FlightServerRecordBatchStreamReader, FlightServerRecordBatchStreamWriter)` | full-duplex incremental delta channel     |
|  [05]   | `Task DoPut(FlightServerRecordBatchStreamReader, IAsyncStreamWriter<FlightPutResult>)`      | client→server batch ingest                |

[ENTRYPOINT_SCOPE]: Flight server response/request streams
- rail: analytical-egress
- members are on `FlightServerRecordBatchStreamWriter` / `FlightServerRecordBatchStreamReader`.

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                       |
| :-----: | :--------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `WriteAsync(RecordBatch[, ByteString applicationMetadata])`      | writes one batch; auto-`SetupStream(batch.Schema)` on first write  |
|  [02]   | `SetupStream(Schema)` / `WriteOptions`                           | emits the schema message before the first batch; IPC write options |
|  [03]   | `await …StreamReader.FlightDescriptor`                           | `ValueTask<FlightDescriptor>` resolves the `DoExchange` descriptor |
|  [04]   | `MoveNextAsync()` / `Current` / `Schema` / `ApplicationMetadata` | reads the inbound `RecordBatch` request stream                     |

[ENTRYPOINT_SCOPE]: Flight message types
- rail: analytical-egress
- the `FlightInfo` ctor also takes optional `long totalRecords`/`totalBytes` (default `-1`).

| [INDEX] | [SURFACE]                                                                     | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `new FlightInfo(Schema, FlightDescriptor, IReadOnlyList<FlightEndpoint>)`     | discovery payload; `TotalRecords`/`TotalBytes` props |
|  [02]   | `new FlightEndpoint(FlightTicket, IReadOnlyList<FlightLocation>)`             | `Ticket` + `Locations` for one endpoint              |
|  [03]   | `new FlightTicket(string)` / `(ByteString)` / `(byte[])`; `ByteString Ticket` | the opaque `DoGet` redemption token                  |
|  [04]   | `FlightDescriptor.Command` / `Paths` / `Type`                                 | command bytes, paths, `FlightDescriptorType`         |

[ENTRYPOINT_SCOPE]: Flight SQL client operations (`Apache.Arrow.Flight.Sql.Client`)
- rail: analytical-egress
- `new FlightSqlClient(FlightClient)` wraps a constructed `FlightClient`; `PreparedStatement` carries `SetParameters(RecordBatch)`/`ExecuteAsync`/`ExecuteUpdateAsync(RecordBatch)`/`CloseAsync`, and the served `FlightSqlServer` overrides `GetFlightInfo`/`DoGet`/`DoAction`/`DoPut`/`ListActions` with static `GetCommand`/`GetTableSchema`/`SupportsAction` decoders.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `ExecuteAsync(query, Transaction)`                                | query          | `Task<FlightInfo>` for a SQL query         |
|  [02]   | `ExecuteUpdateAsync(query, Transaction)`                          | update         | `Task<long>` affected-row count            |
|  [03]   | `PrepareAsync(query, Transaction)`                                | prepare        | `Task<PreparedStatement>` server handle    |
|  [04]   | `DoGetAsync(FlightTicket)`                                        | stream read    | `IAsyncEnumerable<RecordBatch>` per ticket |
|  [05]   | `DoPutAsync(FlightDescriptor, RecordBatch)`                       | stream write   | `Task<FlightPutResult>` ingest             |
|  [06]   | `GetCatalogsAsync()` / `GetDbSchemasAsync(catalog?, pat?)`        | metadata       | catalog and schema discovery               |
|  [07]   | `GetTablesAsync(catalog?, dbSchemaPat?, tablePat?)`               | metadata       | table discovery                            |
|  [08]   | `GetPrimaryKeysAsync` / exported / imported keys                  | metadata       | key discovery over a `TableRef`            |
|  [09]   | `GetCrossReferenceAsync(TableRef pk, TableRef fk)`                | metadata       | cross-reference discovery                  |
|  [10]   | `GetTableTypesAsync` / `GetXdbcTypeInfoAsync` / `GetSqlInfoAsync` | metadata       | type and driver-info discovery             |
|  [11]   | `BeginTransactionAsync()`                                         | transaction    | `Task<Transaction>` opens a unit           |
|  [12]   | `CommitAsync(Transaction)` / `RollbackAsync(Transaction)`         | transaction    | `AsyncServerStreamingCall<FlightResult>`   |
|  [13]   | `CancelFlightInfoAsync` / `CancelQueryAsync`                      | cancel         | `Task<FlightInfoCancelResult>`             |
|  [14]   | `DoActionAsync(FlightAction)` / `GetExecuteSchemaAsync`           | action/schema  | action stream and result-schema probe      |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_TOPOLOGY]:
- core namespace: `Apache.Arrow` — arrays, schema, field, `RecordBatch`, `Table`, `ChunkedArray`
- type namespace: `Apache.Arrow.Types` — `ArrowType`, `ArrowTypeId`, typed type descriptors
- IPC namespace: `Apache.Arrow.Ipc` — `ArrowStreamReader/Writer`, `ArrowFileReader/Writer`, `IpcOptions`, `ICompressionCodecFactory`, `ICompressionCodec`
- compression namespace: `Apache.Arrow.Compression` — `CompressionCodecFactory`, the admitted concrete `ICompressionCodecFactory` over LZ4-frame + Zstandard (internal `Lz4CompressionCodec`/`ZstdCompressionCodec` codecs reached only through the factory)
- ADBC namespace: `Apache.Arrow.Adbc` — `AdbcDriver`, `AdbcDatabase`, `AdbcConnection`, `AdbcStatement`, `QueryResult`/`UpdateResult`/`PartitionedResult`
- Flight namespace: `Apache.Arrow.Flight` — messages, tickets, `FlightClient` (the client lives in this assembly; `Apache.Arrow.Flight.Client` is the namespace not a separate package)
- Flight server namespace: `Apache.Arrow.Flight.Server` (same assembly) — `FlightServer` (the abstract serve base whose `virtual` verbs throw until overridden), `FlightServerRecordBatchStreamWriter`/`FlightServerRecordBatchStreamReader` (the server response/request streams over the `FlightRecordBatchStreamWriter`/`FlightRecordBatchStreamReader` bases carrying `WriteAsync`/`SetupStream` and `MoveNextAsync`/`Schema`), and `FlightLocation` (the endpoint serving URI). A served node subclasses `FlightServer` and overrides `GetFlightInfo`/`GetSchema`/`DoGet`/`DoExchange`; the gRPC service binding enters at the app root that hosts the listener (`ServerCallContext` is the gRPC per-call context), never an interior dependency
- Flight SQL namespace: `Apache.Arrow.Flight.Sql` (client in `Apache.Arrow.Flight.Sql.Client`) — `FlightSqlClient` over a `FlightClient`, `FlightSqlServer` (`abstract : FlightServer`) decoding the SQL command protobufs, `PreparedStatement` (`: IDisposable, IAsyncDisposable`), and the `Transaction`/`TableRef`/`FlightCallOptions`/`DoPutResult` value types. Flight SQL layers over the Flight transport, never a separate listener
- `RecordBatch` implements both `IArrowRecord` and `IArrowArray`, and is `IDisposable`; `Slice`/`SliceShared` window a batch with zero buffer copy
- `IArrowArrayStream` is the async-enumerable contract across IPC, ADBC, and Flight — `Schema` plus `ReadNextRecordBatchAsync`; it is the one egress boundary every analytical surface meets at
- `IpcOptions.CompressionCodec` is `CompressionCodecType?` (`Lz4Frame` \| `Zstd`); the codec is a no-op unless `CompressionCodecFactory` (`ICompressionCodecFactory`) is also set. The concrete factory is NOT in core Arrow — it is `Apache.Arrow.Compression.CompressionCodecFactory` (admitted), which the writer/reader invokes per batch to obtain the per-codec `ICompressionCodec`
- `AdbcConnection.GetObjectsDepth` is the nested enum discriminating `All`, `Catalogs`, `DbSchemas`, `Tables`; `AdbcStatement.SqlQuery` and `SubstraitPlan` are mutually-exclusive query inputs

[LOCAL_ADMISSION]:
- One boundary, three transports: `IArrowArrayStream` (`Schema` + `ReadNextRecordBatchAsync`) is the single shape ADBC `QueryResult.Stream`, the IPC `ArrowStreamReader`, and the Flight `GetStream` call all yield, so the egress owner folds all three behind one async-enumerable and never forks a per-transport reader.
- Callers compose `RecordBatch` via `RecordBatch.Builder` with typed `.Append<TArray>(name, nullable, …)` columns; `WriteStart`/`WriteStartAsync` emits the schema message and `WriteEnd`/`WriteEndAsync` emits the mandatory EOS terminator — a writer disposed without `WriteEnd` leaves a truncated stream the reader rejects.
- IPC compression rides the admitted `Apache.Arrow.Compression` package: set `IpcOptions.CompressionCodec = CompressionCodecType.Lz4Frame` (or `Zstd`) AND `IpcOptions.CompressionCodecFactory = new Apache.Arrow.Compression.CompressionCodecFactory()`, optionally with `IpcOptions.CompressionLevel`. The package-owned `CompressionCodecFactory` is the concrete `ICompressionCodecFactory` for both codecs — LZ4-frame over the transitive `K4os.Compression.LZ4.Streams` and Zstandard over the transitive `ZstdSharp.Port` — so the egress owner NEVER hand-rolls a custom `ICompressionCodecFactory`; the codec enum alone is inert. This Arrow-IPC buffer codec is DISTINCT from the snapshot-codec LZ4 rail (`#api-lz4`), which drives `LZ4Pickler`/`CompressionPolicy` over `K4os.Compression.LZ4` directly for standalone snapshot/blob frames.
- ADBC drivers load via `AdbcDriver.Open(parameters)` then `AdbcDatabase.Connect(options)`; direct `AdbcConnection` construction is not the public path. The DuckDB ADBC driver (`#api-duckdb`) is the in-process analytical engine reached through this same `AdbcConnection`/`AdbcStatement` surface, so a federated query rail dispatches SQL or a `SubstraitPlan` and reads back one `IArrowArrayStream` — `ExecutePartitioned` + `ReadPartition` fan a large scan, `BulkIngest` lands a `RecordBatch` stream.
- Flight `FlightClient` is constructed from a gRPC `ChannelBase`/`CallInvoker` (no static factory); connection lifetime, TLS, and credentials are caller-owned. The `Query/federation#FLIGHT_RESULT_PLANE` `FederationFlight` producer is the served node (AppHost binds the gRPC service) — a Flight read is `GetInfo` → pick a `FlightEndpoint` → `GetStream(endpoint.Ticket)`; a Flight write is `StartPut(descriptor, schema)` then push batches on the duplex `RequestStream`.
- Flight SQL rides that same served node: `FlightSqlServer` (`: FlightServer`) decodes the SQL command protobufs and reuses the existing `DoGet` ticket redemption, so an ADBC FlightSQL consumer — the Python data branch's `adbc-driver-flightsql` — issues `FlightSqlClient` verbs and redeems `FlightTicket`s over one gRPC listener, never a second transport. `PreparedStatement` binds a parameter `RecordBatch`; a `Transaction` bounds a commit/rollback unit.
- Temporal columns map through NodaTime (`#api-nodatime`): an `Instant`/`ZonedDateTime` projects to `TimestampArray` (epoch-nanosecond) at the builder edge so the Arrow wire carries the same clock-seam the relational store uses, never a bare `DateTime`.
- Schema projection (field names, type ids) stays inside the egress owner; downstream reads one typed column via `RecordBatch.Column(name)` returning `IArrowArray` (cast to the concrete `Int64Array`/`DoubleArray`/… or visit), never a `PrimitiveArray<T>` accessor on the batch.
- BIM analytics frames: `Ara3D.BimOpenSchema.IO` (`#api-ara3d-bimopenschema`) is the BIM analytics-frame producer whose managed `Parquet.Net` Brotli `.parquet`-zip (eleven columnar BIM tables) is read into `RecordBatch` streams through `ParquetSharp.Arrow` (`#api-parquetsharp`) and queried over the same `AdbcConnection`/`AdbcStatement` DuckDB-ADBC path (`#api-duckdb`), so the BIM star schema enters this Arrow egress as one `IArrowArrayStream` without re-encoding.

[RAIL_LAW]:
- Packages: `Apache.Arrow`, `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Sql`, `Apache.Arrow.Compression`
- Owns: columnar in-memory format, IPC serialisation (incl. LZ4-frame/Zstandard buffer compression via the package factory), ADBC query execution (incl. partitioned/transactional/Substrait), Flight stream transport, and the Flight SQL dialect over that transport
- Accept: `RecordBatch`/`Schema` construction for egress, IPC stream/file IO with `CompressionCodecFactory = new Apache.Arrow.Compression.CompressionCodecFactory()`, ADBC driver-level queries and bulk ingest, Flight `GetStream`/`StartPut`/`DoExchange`
- Reject: hand-rolled columnar byte layout; a hand-rolled custom `ICompressionCodecFactory` where the admitted `Apache.Arrow.Compression.CompressionCodecFactory` owns LZ4-frame + Zstandard; `IpcOptions.CompressionCodec` set without a `CompressionCodecFactory`; raw gRPC Flight Protobuf without the `FlightClient` wrapper; a per-transport reader where `IArrowArrayStream` already unifies IPC, ADBC, and Flight; a bare `DateTime` column where the NodaTime clock seam owns the instant; conflating the Arrow-IPC compression rail with the `#api-lz4` snapshot-codec rail
