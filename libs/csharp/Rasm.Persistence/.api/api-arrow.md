# [RASM_PERSISTENCE_API_ARROW]

`Apache.Arrow` supplies the in-memory columnar format, schema and field model,
typed array families, `RecordBatch` construction, IPC file and stream serialisation,
and the `IArrowArrayStream` contract for Persistence analytical egress. `Apache.Arrow.Adbc`
provides the Arrow Database Connectivity abstraction: `AdbcConnection`, `AdbcStatement`,
`AdbcDatabase`, and `AdbcDriver` for driver-based query execution over Arrow streams.
`Apache.Arrow.Flight` supplies the Flight RPC data-transfer protocol: descriptor, endpoint,
and info messages plus client-side `FlightClient` for reading and writing `RecordBatch`
streams over gRPC.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record and schema family
- rail: analytical-egress

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [CAPABILITY]                        |
| :-----: | :-------------------- | :--------------- | :---------------------------------- |
|   [1]   | `RecordBatch`         | record container | columnar batch with schema          |
|   [2]   | `RecordBatch.Builder` | builder          | assembles typed arrays into a batch |
|   [3]   | `Schema`              | schema value     | ordered field list with metadata    |
|   [4]   | `Schema.Builder`      | builder          | assembles fields into schema        |
|   [5]   | `Field`               | field value      | name, type, nullability, metadata   |
|   [6]   | `Field.Builder`       | builder          | assembles field from parts          |
|   [7]   | `ChunkedArray`        | chunked array    | list of same-type arrays            |
|   [8]   | `Table`               | table value      | schema plus chunked column list     |
|   [9]   | `IArrowArray`         | array contract   | columnar array capability           |
|  [10]   | `IArrowRecord`        | record contract  | schema-plus-arrays capability       |

[PUBLIC_TYPE_SCOPE]: primitive array family
- rail: analytical-egress

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                   |
| :-----: | :---------------- | :------------- | :----------------------------- |
|   [1]   | `BooleanArray`    | bool array     | validity-bitmap boolean values |
|   [2]   | `Int8Array`       | integer array  | signed 8-bit values            |
|   [3]   | `Int16Array`      | integer array  | signed 16-bit values           |
|   [4]   | `Int32Array`      | integer array  | signed 32-bit values           |
|   [5]   | `Int64Array`      | integer array  | signed 64-bit values           |
|   [6]   | `UInt8Array`      | integer array  | unsigned 8-bit values          |
|   [7]   | `UInt16Array`     | integer array  | unsigned 16-bit values         |
|   [8]   | `UInt32Array`     | integer array  | unsigned 32-bit values         |
|   [9]   | `UInt64Array`     | integer array  | unsigned 64-bit values         |
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

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                        |
| :-----: | :--------------------- | :-------------- | :---------------------------------- |
|   [1]   | `ArrowType`            | type base       | root type for all Arrow types       |
|   [2]   | `ArrowTypeId`          | type enum       | discriminates Arrow type identities |
|   [3]   | `IArrowType`           | type contract   | minimal type contract               |
|   [4]   | `ArrowStreamReader`    | IPC reader      | reads Arrow IPC stream format       |
|   [5]   | `ArrowStreamWriter`    | IPC writer      | writes Arrow IPC stream format      |
|   [6]   | `ArrowFileReader`      | IPC reader      | reads Arrow IPC file format         |
|   [7]   | `ArrowFileWriter`      | IPC writer      | writes Arrow IPC file format        |
|   [8]   | `IArrowReader`         | reader contract | shared sync/async reader contract   |
|   [9]   | `IpcOptions`           | IPC policy      | compression and alignment options   |
|  [10]   | `CompressionCodecType` | codec enum      | LZ4 or ZSTD codec selector          |
|  [11]   | `IArrowArrayStream`    | stream contract | async enumerable of record batches  |

[PUBLIC_TYPE_SCOPE]: ADBC family
- rail: analytical-egress

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [CAPABILITY]                       |
| :-----: | :--------------- | :-------------- | :--------------------------------- |
|   [1]   | `AdbcDriver`     | driver root     | loads and creates databases        |
|   [2]   | `AdbcDatabase`   | database root   | opens connections                  |
|   [3]   | `AdbcConnection` | connection root | executes statements and schema ops |
|   [4]   | `AdbcStatement`  | statement root  | executes queries and updates       |
|   [5]   | `AdbcException`  | ADBC failure    | typed ADBC error                   |
|   [6]   | `AdbcInfoCode`   | info enum       | driver info code identifiers       |
|   [7]   | `AdbcOptions`    | options value   | generic key-value option map       |

[PUBLIC_TYPE_SCOPE]: Flight family
- rail: analytical-egress

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `FlightClient`                   | client root     | gRPC client for Flight protocol      |
|   [2]   | `FlightInfo`                     | info message    | schema, endpoints, record counts     |
|   [3]   | `FlightDescriptor`               | descriptor      | path or command to identify a flight |
|   [4]   | `FlightDescriptorType`           | descriptor enum | path vs. command discriminant        |
|   [5]   | `FlightEndpoint`                 | endpoint        | location list for a partition        |
|   [6]   | `FlightData`                     | data message    | carries `RecordBatch` on the wire    |
|   [7]   | `FlightAction`                   | action message  | opaque action request                |
|   [8]   | `FlightActionType`               | action type     | describes an available action        |
|   [9]   | `FlightRecordBatchStreamingCall` | call handle     | streaming read call handle           |
|  [10]   | `FlightRecordBatchExchangeCall`  | call handle     | bidirectional exchange call handle   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: RecordBatch and Schema construction
- rail: analytical-egress

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `RecordBatch.Builder(allocator?)`                     | ctor           | creates batch builder with allocator  |
|   [2]   | `RecordBatch.Builder.Append(name, nullable, array)`   | builder        | adds typed column to batch            |
|   [3]   | `RecordBatch.Builder.Append(name, nullable, builder)` | builder        | adds built column                     |
|   [4]   | `RecordBatch.Builder.Append(batch)`                   | builder        | merges schema and arrays from a batch |
|   [5]   | `RecordBatch.Builder.Build()`                         | factory call   | yields immutable `RecordBatch`        |
|   [6]   | `RecordBatch.Builder.Clear()`                         | reset          | resets schema and arrays              |
|   [7]   | `Schema.Builder.Field(field)`                         | builder        | adds field to schema                  |
|   [8]   | `Schema.Builder.Build()`                              | factory call   | yields immutable `Schema`             |
|   [9]   | `Field.Builder.Name(name)`                            | builder        | sets field name                       |
|  [10]   | `Field.Builder.DataType(type)`                        | builder        | sets Arrow type                       |
|  [11]   | `Field.Builder.Nullable(nullable)`                    | builder        | sets nullability                      |
|  [12]   | `Field.Builder.Build()`                               | factory call   | yields immutable `Field`              |

[ENTRYPOINT_SCOPE]: IPC read and write
- rail: analytical-egress

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------- |
|   [1]   | `new ArrowStreamReader(stream)`                   | ctor           | opens IPC stream reader          |
|   [2]   | `new ArrowStreamWriter(stream, schema, options?)` | ctor           | opens IPC stream writer          |
|   [3]   | `new ArrowFileReader(stream)`                     | ctor           | opens IPC file reader            |
|   [4]   | `new ArrowFileWriter(stream, schema, options?)`   | ctor           | opens IPC file writer            |
|   [5]   | `ReadNextRecordBatch()`                           | sync read      | reads next `RecordBatch`         |
|   [6]   | `ReadNextRecordBatchAsync()`                      | async read     | async reads next `RecordBatch`   |
|   [7]   | `WriteRecordBatch(batch)`                         | sync write     | writes one `RecordBatch`         |
|   [8]   | `WriteRecordBatchAsync(batch)`                    | async write    | async writes one `RecordBatch`   |
|   [9]   | `WriteEnd()`                                      | finalize       | writes IPC terminator            |
|  [10]   | `WriteEndAsync()`                                 | async finalize | async writes IPC terminator      |
|  [11]   | `IpcOptions` with `CompressionCodecType`          | configuration  | selects LZ4\_FRAME or ZSTD codec |

[ENTRYPOINT_SCOPE]: ADBC statement execution
- rail: analytical-egress

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------ | :------------- | :---------------------------------------------- |
|   [1]   | `AdbcDriver.Open(options)`                              | driver open    | creates `AdbcDatabase`                          |
|   [2]   | `AdbcDatabase.Connect(options)`                         | connect        | creates `AdbcConnection`                        |
|   [3]   | `AdbcConnection.CreateStatement()`                      | factory        | creates `AdbcStatement`                         |
|   [4]   | `AdbcConnection.BulkIngest(table, mode)`                | ingest factory | creates ingest statement                        |
|   [5]   | `AdbcConnection.GetObjects(depth, patterns)`            | schema query   | returns `IArrowArrayStream`                     |
|   [6]   | `AdbcConnection.GetTableSchema(catalog, schema, table)` | schema         | returns `Schema`                                |
|   [7]   | `AdbcConnection.GetTableTypes()`                        | schema query   | returns `IArrowArrayStream`                     |
|   [8]   | `AdbcStatement.SqlQuery`                                | property       | sets SQL text for execution                     |
|   [9]   | `AdbcStatement.ExecuteQuery()`                          | sync execute   | returns `QueryResult` with stream               |
|  [10]   | `AdbcStatement.ExecuteQueryAsync()`                     | async execute  | async returns `QueryResult`                     |
|  [11]   | `AdbcStatement.ExecuteUpdate()`                         | sync update    | returns `UpdateResult` with row count           |
|  [12]   | `AdbcStatement.Prepare()`                               | prepare        | prepares statement server-side                  |
|  [13]   | `AdbcStatement.Bind(batch, schema)`                     | bind           | binds `RecordBatch` for parameterised execution |

[ENTRYPOINT_SCOPE]: Flight client operations
- rail: analytical-egress

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `FlightClient.Create(channel)`               | factory        | creates client from gRPC channel       |
|   [2]   | `FlightClient.GetFlightInfo(descriptor)`     | info query     | retrieves `FlightInfo` for descriptor  |
|   [3]   | `FlightClient.GetSchema(descriptor)`         | schema query   | retrieves `Schema` for descriptor      |
|   [4]   | `FlightClient.GetStream(endpoint, options?)` | stream read    | opens `FlightRecordBatchStreamingCall` |
|   [5]   | `FlightClient.DoPut(schema)`                 | stream write   | opens put streaming call               |
|   [6]   | `FlightClient.DoExchange(descriptor)`        | exchange       | opens bidirectional call               |
|   [7]   | `FlightClient.DoAction(action)`              | action call    | performs named action                  |
|   [8]   | `FlightClient.ListFlights(criteria?)`        | discovery      | enumerates available `FlightInfo`s     |

## [4]-[IMPLEMENTATION_LAW]

[ARROW_TOPOLOGY]:
- core namespace: `Apache.Arrow` — arrays, schema, field, `RecordBatch`, `Table`, `ChunkedArray`
- type namespace: `Apache.Arrow.Types` — `ArrowType`, `ArrowTypeId`, typed type descriptors
- IPC namespace: `Apache.Arrow.Ipc` — `ArrowStreamReader/Writer`, `ArrowFileReader/Writer`, `IpcOptions`
- ADBC namespace: `Apache.Arrow.Adbc` — `AdbcDriver`, `AdbcDatabase`, `AdbcConnection`, `AdbcStatement`
- Flight namespace: `Apache.Arrow.Flight` — messages; `Apache.Arrow.Flight.Client` — gRPC client
- `RecordBatch` implements both `IArrowRecord` and `IArrowArray`, and is `IDisposable`
- `IArrowArrayStream` is the async-enumerable contract across IPC, ADBC, and Flight
- `IpcOptions` carries `CompressionCodecType` (`LZ4_FRAME` or `ZSTD`) and buffer-alignment policy
- `AdbcConnection.GetObjectsDepth` discriminates `All`, `Catalogs`, `DbSchemas`, `Tables`

[LOCAL_ADMISSION]:
- Analytical egress enters through the `IArrowArrayStream` contract; callers compose `RecordBatch` via `RecordBatch.Builder` with typed `.Append` columns.
- IPC writers require explicit `WriteEnd`/`WriteEndAsync` to emit the schema message terminator; disposal without it produces an incomplete stream.
- ADBC drivers are loaded by `AdbcDriver` and enter through `AdbcDatabase.Connect`; direct `AdbcConnection` construction is not the public path.
- Flight `FlightClient` is created from a gRPC `Channel`; connection lifetime and SSL policy are the caller's responsibility.
- Schema projection (schema, field names, type ids) stays inside the egress owner; downstream code reads typed arrays via `PrimitiveArray<T>` accessors.

[RAIL_LAW]:
- Packages: `Apache.Arrow`, `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`
- Owns: columnar in-memory format, IPC serialisation, ADBC query execution, and Flight stream transport
- Accept: `RecordBatch` / `Schema` construction for egress, IPC stream/file IO, ADBC driver-level queries, Flight `GetStream`/`DoPut`
- Reject: hand-rolled columnar byte layout, raw gRPC Flight Protobuf without the Flight client wrapper
