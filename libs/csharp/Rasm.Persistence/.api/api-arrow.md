# [RASM_PERSISTENCE_API_ARROW]

`Apache.Arrow` owns the in-memory columnar format and Arrow IPC file/stream serialisation, minting the `IArrowArrayStream` contract every analytical egress meets at. `Apache.Arrow.Adbc` drives driver-based query execution over Arrow streams; `Apache.Arrow.Flight` carries the Flight RPC `RecordBatch` transport over gRPC and `Apache.Arrow.Flight.Sql` folds a SQL dialect over that transport; `Apache.Arrow.Compression` binds the concrete IPC LZ4-frame and Zstandard codec factory. Persistence composes the five onto one analytical-egress rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow`
- package: `Apache.Arrow` (Apache-2.0)
- assembly: `Apache.Arrow`
- namespace: `Apache.Arrow`, `Apache.Arrow.Ipc`, `Apache.Arrow.Types`, `Apache.Arrow.Memory`
- rail: analytical-egress

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

[PACKAGE_SURFACE]: `Apache.Arrow.Compression`
- package: `Apache.Arrow.Compression` (Apache-2.0)
- assembly: `Apache.Arrow.Compression`
- namespace: `Apache.Arrow.Compression`
- asset: pure-managed AnyCPU, no native RID; the managed `K4os`/`ZstdSharp` transitives carry the codec bodies
- rail: analytical-egress (IPC compression)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record and schema family

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
|  [10]   | `CompressionCodecType`     | codec enum      | `Lz4Frame` \| `Zstd`                                                        |
|  [11]   | `ICompressionCodecFactory` | codec factory   | `CreateCodec(type[, level])`; concrete impl in `Apache.Arrow.Compression`   |
|  [12]   | `ICompressionCodec`        | codec contract  | `Decompress(ReadOnlyMemory<byte>, Memory<byte>)`; `Compress` default-throws |
|  [13]   | `IArrowArrayStream`        | stream contract | async enumerable of record batches; `Schema` + `ReadNextRecordBatchAsync`   |

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

[PUBLIC_TYPE_SCOPE]: IPC compression family (`Apache.Arrow.Compression`)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `CompressionCodecFactory` | codec factory | the only public type, `sealed : ICompressionCodecFactory`; `Lz4Frame`/`Zstd` codecs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: RecordBatch and Schema construction
- a null `StringArray` append lands as a validity-bitmap null.

| [INDEX] | [SURFACE]                                                | [SHAPE]        | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `StringArray.Builder().Append(string)`                   | array builder  | appends one UTF-8 string                     |
|  [02]   | `StringArray.Builder().AppendRange(IEnumerable<string>)` | array builder  | appends a UTF-8 string range                 |
|  [03]   | `StringArray.Builder().Build()`                          | factory call   | builds the UTF-8 `StringArray` column        |
|  [04]   | `RecordBatch.Builder(allocator?)`                        | ctor           | creates batch builder with allocator         |
|  [05]   | `RecordBatch.Builder.Append(name, nullable, array)`      | builder        | adds typed column to batch                   |
|  [06]   | `RecordBatch.Builder.Append(name, nullable, builder)`    | builder        | adds built column                            |
|  [07]   | `RecordBatch.Builder.Append(batch)`                      | builder        | merges schema and arrays from a batch        |
|  [08]   | `RecordBatch.Builder.Build()`                            | factory call   | yields immutable `RecordBatch`               |
|  [09]   | `RecordBatch.Builder.Clear()`                            | reset          | resets schema and arrays                     |
|  [10]   | `Schema.Builder.Field(field)`                            | builder        | adds field to schema                         |
|  [11]   | `Schema.Builder.Build()`                                 | factory call   | yields immutable `Schema`                    |
|  [12]   | `Field.Builder.Name(name)`                               | builder        | sets field name                              |
|  [13]   | `Field.Builder.DataType(type)`                           | builder        | sets Arrow type                              |
|  [14]   | `Field.Builder.Nullable(nullable)`                       | builder        | sets nullability                             |
|  [15]   | `Field.Builder.Build()`                                  | factory call   | yields immutable `Field`                     |
|  [16]   | `RecordBatch.Slice(offset, length)` / `SliceShared`      | zero-copy view | windows a batch without copying buffers      |
|  [17]   | `RecordBatch.Column(name)` / `Column(int)`               | column access  | reads one `IArrowArray` column by name/index |

[ENTRYPOINT_SCOPE]: IPC read and write

| [INDEX] | [SURFACE]                                                   | [SHAPE]      | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :----------- | :--------------------------------------------------- |
|  [01]   | `new ArrowStreamReader(stream, leaveOpen?)`                 | ctor         | opens IPC stream reader (`IArrowReader`)             |
|  [02]   | `new ArrowStreamWriter(stream, schema, leaveOpen, options)` | ctor         | opens IPC stream writer; `MemoryAllocator` overload  |
|  [03]   | `new ArrowFileReader(stream, leaveOpen?)`                   | ctor         | opens IPC file reader (random-access)                |
|  [04]   | `new ArrowFileWriter(stream, schema, leaveOpen, options)`   | ctor         | opens IPC file writer                                |
|  [05]   | `WriteStart()` / `WriteStartAsync()`                        | schema write | emits the schema message before the first batch      |
|  [06]   | `ReadNextRecordBatch()`                                     | sync read    | reads next `RecordBatch`                             |
|  [07]   | `ReadNextRecordBatchAsync()`                                | async read   | async reads next `RecordBatch`                       |
|  [08]   | `WriteRecordBatch(batch)`                                   | sync write   | writes one `RecordBatch`                             |
|  [09]   | `WriteRecordBatchAsync(batch)`                              | async write  | async writes one `RecordBatch`                       |
|  [10]   | `WriteEnd()` / `WriteEndAsync()`                            | finalize     | writes IPC EOS terminator (mandatory before dispose) |

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
- `IArrowArrayStream` (`Schema` + `ReadNextRecordBatchAsync`) is the one async-enumerable egress boundary IPC, ADBC, and Flight all yield; the egress owner folds all three behind it and never forks a per-transport reader.
- `RecordBatch` implements `IArrowRecord` and `IArrowArray` and is `IDisposable`; `Slice`/`SliceShared` window a batch with zero buffer copy.
- `IpcOptions.CompressionCodec` (`CompressionCodecType?`, `Lz4Frame` \| `Zstd`) is inert unless `CompressionCodecFactory` is set; the concrete `ICompressionCodecFactory` is `Apache.Arrow.Compression.CompressionCodecFactory`, never core Arrow, invoked per batch for the per-codec `ICompressionCodec`.
- `AdbcConnection.GetObjectsDepth` discriminates `All`/`Catalogs`/`DbSchemas`/`Tables`; `AdbcStatement.SqlQuery` and `SubstraitPlan` are mutually-exclusive query inputs.
- Flight SQL layers over the Flight transport, never a second listener: `FlightSqlServer` (`: FlightServer`) decodes the SQL command protobufs and reuses the `DoGet` ticket redemption.

[STACKING]:
- `api-duckdb`(`.api/api-duckdb.md`): the DuckDB ADBC driver is the in-process analytical engine reached through this `AdbcConnection`/`AdbcStatement` surface, so a federated rail dispatches SQL or a `SubstraitPlan` and reads back one `IArrowArrayStream` — `ExecutePartitioned` + `ReadPartition` fan a large scan, `BulkIngest` lands a `RecordBatch` stream.
- `api-parquetsharp`(`.api/api-parquetsharp.md`) + `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): the BIM analytics star schema (columnar tables in a `Parquet.Net` Brotli `.parquet`-zip) reads into `RecordBatch` streams through `ParquetSharp.Arrow` and queries over the same DuckDB-ADBC path, entering this egress as one `IArrowArrayStream` without re-encoding.
- `api-nodatime`(`.api/api-nodatime.md`): an `Instant`/`ZonedDateTime` projects to `TimestampArray` (epoch-nanosecond) at the builder edge, so the Arrow wire carries the relational store's clock seam, never a bare `DateTime`.
- `api-lz4`(`.api/api-lz4.md`): the Arrow-IPC buffer codec through `CompressionCodecFactory` is distinct from the snapshot-codec LZ4 rail driving `LZ4Pickler`/`CompressionPolicy` over `K4os.Compression.LZ4` for standalone snapshot/blob frames.
- within-lib: the Persistence egress owner folds IPC, ADBC, and Flight behind one `IArrowArrayStream`, composes each batch through `RecordBatch.Builder` typed `.Append` columns, and reads one typed column via `RecordBatch.Column(name)` returning `IArrowArray` — one boundary materialisation, never a `PrimitiveArray<T>` batch accessor.

[LOCAL_ADMISSION]:
- `WriteStart`/`WriteStartAsync` emits the schema message and `WriteEnd`/`WriteEndAsync` the mandatory EOS terminator; a writer disposed without `WriteEnd` leaves a truncated stream the reader rejects.
- IPC compression sets `IpcOptions.CompressionCodec` (`Lz4Frame` or `Zstd`) AND `IpcOptions.CompressionCodecFactory = new Apache.Arrow.Compression.CompressionCodecFactory()`, optionally `IpcOptions.CompressionLevel`; the codec enum alone is inert and the egress owner never hand-rolls an `ICompressionCodecFactory`.
- ADBC drivers load via `AdbcDriver.Open(parameters)` then `AdbcDatabase.Connect(options)`; direct `AdbcConnection` construction is not the public path.
- `FlightClient` constructs from a gRPC `ChannelBase`/`CallInvoker` (no static factory), connection lifetime/TLS/credentials caller-owned; a Flight read is `GetInfo` → pick a `FlightEndpoint` → `GetStream(endpoint.Ticket)`, a write is `StartPut(descriptor, schema)` then batches on the duplex `RequestStream`.
- Flight SQL rides that one served node over a single gRPC listener: `FlightSqlServer` reuses the `DoGet` ticket redemption, `PreparedStatement` binds a parameter `RecordBatch`, and a `Transaction` bounds a commit/rollback unit.

[RAIL_LAW]:
- Packages: `Apache.Arrow`, `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Sql`, `Apache.Arrow.Compression`
- Owns: the columnar in-memory format, Arrow IPC serialisation with LZ4-frame/Zstandard buffer compression, ADBC query execution (partitioned, transactional, Substrait), Flight `RecordBatch` transport, and the Flight SQL dialect over it
- Accept: `RecordBatch`/`Schema` construction for egress, IPC stream/file IO with the package `CompressionCodecFactory`, ADBC driver-level queries and bulk ingest, Flight `GetStream`/`StartPut`/`DoExchange`
- Reject: hand-rolled columnar byte layout; a custom `ICompressionCodecFactory` where `Apache.Arrow.Compression.CompressionCodecFactory` owns both codecs; `CompressionCodec` set without a factory; raw gRPC Flight Protobuf without `FlightClient`; a per-transport reader where `IArrowArrayStream` unifies IPC, ADBC, and Flight; a bare `DateTime` column where the NodaTime clock seam owns the instant
