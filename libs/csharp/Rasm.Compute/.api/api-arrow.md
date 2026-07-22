# [RASM_COMPUTE_API_ARROW]

`Apache.Arrow` is the columnar in-memory format Compute builds its analytical result tables into: `RecordBatch.Builder` assembles typed columns, `Schema.Builder`/`Field.Builder` name and type them, `PrimitiveArrayBuilder<T,…>` families bulk-append a `ReadOnlySpan<T>` column in one call, and `MemoryAllocator` owns the buffer arena each builder draws from.

This overlay owns ONLY that construction-and-projection seam — the columnar table Compute *produces*; IPC stream/file serialisation (`ArrowStreamWriter`/`ArrowFileWriter`), the LZ4-frame/Zstandard `CompressionCodecFactory`, the ADBC query surface, and the Flight/Flight-SQL transport that *carries* the table are the Persistence `api-arrow` overlay's egress rails and are NOT restated here.

Compute's Arrow batch seam projects a landed `DoeDataset` into one `RecordBatch` with one bulk append call per column. Each builder copies the `Coordinates`, `Responses`, or `OnFront` span into its allocator-owned buffer through the typed `Append(ReadOnlySpan<T>)` family.

`ContentKey`, `Strategy`, `At`, and `Points` ride an explicitly built `Schema` through `Schema.Builder.Metadata`, and the public `RecordBatch(Schema, IEnumerable<IArrowArray>, int)` constructor binds the schema to the arrays. `ChargebackDataset` folds the same construction surface for content-keyed billing egress.

Every builder is a freely constructed per-instance object; `MemoryAllocator.Default` is the one process-global. Typed-array `Build` entries and `RecordBatch.Builder` construction admit a per-instance `MemoryAllocator allocator = null`; `Schema.Builder.Build()` and `RecordBatch.Builder.Build()` do not. A bounded per-lane arena therefore injects while arrays are built, and the direct `RecordBatch` constructor receives the completed schema and arrays.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow`
- package: `Apache.Arrow`
- license: `Apache-2.0` (`apache/arrow`)
- assembly: `Apache.Arrow`
- namespace: `Apache.Arrow` (arrays, builders, `RecordBatch`, `Schema`, `Field`, `Table`), `Apache.Arrow.Types` (the `IArrowType` descriptors), `Apache.Arrow.Memory` (`MemoryAllocator`)
- target: multi-target (`net462`, `netstandard2.0`, `net8.0`); the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed runtime library, AnyCPU, no native RID asset
- depends: `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Sql`, and `Apache.Arrow.Compression` are the Persistence overlay's egress packages and are absent from the Compute closure — Compute references core `Apache.Arrow` alone
- abi: `RecordBatch`/`Schema`/`Field` are reference types; `MemoryAllocator` is `abstract` with a `Lazy<MemoryAllocator> Default`; resolve builder members against the restored `Apache.Arrow` public surface
- rail: columnar-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: record, schema, and field construction (`Apache.Arrow`)
- rail: columnar-egress
- note: the IPC reader/writer, `IpcOptions`, `IArrowArrayStream`, ADBC, and Flight types are the Persistence overlay's and are absent here; every type below is per-instance constructed, never a static handle.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]     | [CAPABILITY]                                                              |
| :-----: | :--------------------------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `RecordBatch`                | record container  | columnar batch with schema; `: IArrowRecord, IArrowArray`, `IDisposable` |
|  [02]   | `RecordBatch.Builder`        | builder           | co-orders fields and arrays; carries no schema metadata seat             |
|  [03]   | `RecordBatch.Builder.ArrayBuilder` | fluent factory | per-column typed builder (`.Double`/`.Boolean`/`.Int64` `Action` arms)  |
|  [04]   | `Schema`                     | schema value      | ordered field list plus metadata; `this[int]`/`this[string]` field index |
|  [05]   | `Schema.Builder`             | builder           | assembles fields and metadata into an immutable `Schema`                 |
|  [06]   | `Field`                      | field value       | name, `IArrowType`, nullability, metadata                                |
|  [07]   | `Field.Builder`              | builder           | assembles a field from name/type/nullable/metadata parts                 |
|  [08]   | `Table`                      | table value       | schema plus chunked column list; `TableFromRecordBatches` factory        |
|  [09]   | `IArrowArrayBuilder<TArray>` | builder contract  | `Build(MemoryAllocator)` the `RecordBatch.Builder.Append` builder arm    |

[PUBLIC_TYPE_SCOPE]: typed array builders (`Apache.Arrow`) — the `DoeDataset`/`ChargebackDataset` columns
- rail: columnar-egress
- note: every primitive builder derives `PrimitiveArrayBuilder<T, TArray, TBuilder>` (e.g. `Int64Array.Builder : PrimitiveArrayBuilder<long, Int64Array, Int64Array.Builder>`), so the append/build member set below is uniform across the family; `StringArray.Builder` adds `.Append(string)`/`.AppendRange(IEnumerable<string>)`.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `DoubleArray.Builder`    | float builder | `Coordinates`/`Responses` double columns      |
|  [02]   | `BooleanArray.Builder`   | bool builder  | `OnFront` validity-bitmap bool column         |
|  [03]   | `Int64Array.Builder`     | int builder   | 64-bit counts, cardinalities                  |
|  [04]   | `Int32Array.Builder`     | int builder   | 32-bit integer columns                        |
|  [05]   | `FloatArray.Builder`     | float builder | 32-bit float columns                          |
|  [06]   | `TimestampArray.Builder` | temporal      | `Instant At` epoch column via NodaTime        |
|  [07]   | `StringArray.Builder`    | binary builder| UTF-8 axis/objective label columns            |

[PUBLIC_TYPE_SCOPE]: type descriptors and allocator (`Apache.Arrow.Types`, `Apache.Arrow.Memory`)
- rail: columnar-egress
- note: each `IArrowType` exposes a `.Default` singleton the `Field.Builder.DataType` arm consumes; `TimestampType` takes a `TimeUnit`+timezone so its `.Default` is `(TimeUnit.Millisecond, "+00:00")`.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :------------------------ | :------------- | :------------------------------------------------- |
|  [01]   | `DoubleType.Default`      | type singleton | `IArrowType` for a `double` field                  |
|  [02]   | `BooleanType.Default`     | type singleton | `IArrowType` for a `bool` field                    |
|  [03]   | `TimestampType.Default`   | type value     | `(TimeUnit, timezone)`; the epoch-column type      |
|  [04]   | `StringType.Default`      | type singleton | `IArrowType` for a UTF-8 string field              |
|  [05]   | `IArrowType`              | type contract  | `TypeId`/`Name`; the `Field.Builder.DataType` input|
|  [06]   | `MemoryAllocator`         | buffer arena   | `abstract`; `Allocate(int) → IMemoryOwner<byte>`   |
|  [07]   | `MemoryAllocator.Default` | shared arena   | `Lazy<MemoryAllocator>`; process-global fallback   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `RecordBatch.Builder` metadata-free column assembly and build
- rail: columnar-egress
- note: `new RecordBatch.Builder(allocator?)` opens a metadata-free batch builder under a per-instance arena; each `Append<TArray>(name, nullable, array)` co-orders one generated field and built column, and `Build()` yields the immutable `RecordBatch`. Builder exposes no `Schema` injection or metadata entrypoint, so it never constructs `DoeDataset` or `ChargebackDataset` batches whose receipt facts must survive.
- returns: `Build()` → `RecordBatch`; each typed `Build(allocator?)` → the concrete `TArray`.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new RecordBatch.Builder(allocator = null)`            | ctor           | opens a batch builder under an arena         |
|  [02]   | `Append<TArray>(name, nullable, TArray array)`         | column add     | adds one built typed column by name          |
|  [03]   | `Append<TArray>(name, nullable, IArrowArrayBuilder<T>)`| column add     | adds a column from an unbuilt builder        |
|  [04]   | `Append(RecordBatch batch)`                            | merge          | merges schema and arrays from a batch        |
|  [05]   | `Build()` / `Clear()`                                  | factory/reset  | seals the immutable `RecordBatch` / resets   |
|  [06]   | `ArrayBuilder.Double(Action<DoubleArray.Builder>)`     | inline column  | builds a `DoubleArray` column inline         |

[ENTRYPOINT_SCOPE]: typed column bulk-append (`PrimitiveArrayBuilder<T,…>`)
- rail: columnar-egress
- note: `Append(ReadOnlySpan<T>)` bulk-appends a whole backing span in one call — the reduced append-call path for the `DoeDataset` `ReadOnlyMemory<double>` columns (`.Span`), copying each span into the builder's allocator-owned buffer; `AppendNull` writes a validity-bitmap null; `Build(allocator?)` seals the column. `Reserve(capacity)` pre-sizes the buffer to the known point count before the span append.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<T> span)`         | bulk append    | copies one whole span without a caller-side scalar loop      |
|  [02]   | `AppendRange(IEnumerable<T> values)`   | range append   | appends an enumerable column source                          |
|  [03]   | `Append(T value)` / `Append(T? value)` | scalar append  | appends one value (nullable overload writes validity)        |
|  [04]   | `AppendNull()`                         | null append    | appends a validity-bitmap null slot                          |
|  [05]   | `Reserve(int capacity)` / `Resize(int)`| pre-size       | pre-allocates or resizes the backing buffer                  |
|  [06]   | `Set(int index, T value)` / `Swap(i,j)`| edit           | in-place value set / positional swap before build            |
|  [07]   | `Build(MemoryAllocator allocator = null)` | factory call | seals the immutable typed array under the arena              |

[ENTRYPOINT_SCOPE]: metadata-bearing `Schema` / `RecordBatch` and `Table` assembly
- rail: columnar-egress
- note: the schema is built explicitly — one `Field` per array in matching order, with `Metadata(key, value)` carrying the `ContentKey`/`Strategy`/`At`/`Points` receipt facts. `new RecordBatch(schema, arrays, length)` binds that schema to the arrays; `TableFromRecordBatches` collects streamed batches into one queryable `Table`.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `new Schema.Builder()` / `.Build()`                    | builder        | opens and seals an immutable `Schema`     |
|  [02]   | `Schema.Builder.Field(Field)` / `.Field(Action<…>)`    | field add      | adds a field by value or inline builder   |
|  [03]   | `Schema.Builder.Metadata(key, value)`                  | metadata       | attaches schema-level receipt facts       |
|  [04]   | `Field.Builder.Name(s).DataType(t).Nullable(b).Build()`| builder        | assembles one field from parts            |
|  [05]   | `new Field(name, IArrowType, nullable, metadata?)`     | ctor           | direct field construction                 |
|  [06]   | `new RecordBatch(Schema, IEnumerable<IArrowArray>, int)` | ctor         | binds metadata schema, arrays, and length |
|  [07]   | `Table.TableFromRecordBatches(Schema, IList<batch>)`   | table factory  | collects batches into one `Table`         |
|  [08]   | `MemoryAllocator.Default.Value` / `Allocate(int)`      | arena          | shared default arena; `Allocate` a buffer |

## [04]-[IMPLEMENTATION_LAW]

[BATCH_TOPOLOGY]:
- metadata-bearing construction has one root: the sweep-egress owner builds typed arrays under the per-lane `MemoryAllocator`, builds one `Schema` whose fields match the array order one-for-one and whose metadata carries every receipt fact, then calls `new RecordBatch(schema, arrays, points)`. Substituting `RecordBatch.Builder` discards `content_key`, `strategy`, `at`, and `points` because it exposes neither `Schema` injection nor `Metadata`.
- each `DoeDataset` column bulk-appends its backing span: `Coordinates`/`Responses` drive `DoubleArray.Builder.Append(span)`, `OnFront` drives `BooleanArray.Builder.Append(span)`, and `Reserve(points)` pre-sizes each allocator-owned buffer. Builders copy spans into owned storage with one caller-side append per column, never a scalar `Append(T)` loop.
- `ContentKey`, `Strategy` (the `DoeDesign` key), `At`, and `Points` receipt facts ride `Schema.Builder.Metadata(key, value)` string pairs, so the batch is self-describing across the wire — the reader recovers the content key from schema metadata rather than a side-channel, and a batch whose metadata omits the content key is the drift defect.
- `ChargebackDataset` projects one row per tenant-route partition: `tenant` and nullable `route` build through `StringArray`, elapsed/token/byte/remote cost lanes through `DoubleArray`, and `facts` through `Int64Array`; `content_key`, `window_start`, and `window_end` ride schema metadata, so billing egress consumes the same allocator, field-order, and metadata-bearing constructor policy as DOE egress.
- `MemoryAllocator` injects through typed-array builds and `RecordBatch.Builder(allocator)`; `Schema.Builder.Build()` and `RecordBatch.Builder.Build()` take no allocator. A staging-bounded lane (`Tensor/memory#STAGING_POOL`) passes its allocator to every array build so buffers charge against the lane budget, and the public `RecordBatch` constructor binds those arrays without reallocating them.

[TYPE_SEAM]:
- `Field.Builder.DataType` takes an `IArrowType`; the `.Default` singletons (`DoubleType.Default`, `BooleanType.Default`, `StringType.Default`) type the scalar columns, and `TimestampType.Default` (`TimeUnit.Millisecond`, UTC) types the `Instant At` column so the Arrow wire carries the same NodaTime clock seam (`api-nodatime`) the receipt stream uses — a bare `DateTime` column is the rejected form, symmetric to the Persistence egress rule.
- `RecordBatch.Builder.ArrayBuilder` exposes a fluent arm (`.Double(b => b.Append(span))`) that constructs a column inline against the batch's own allocator. Metadata-free batches may use that convenience; the sweep owner uses explicit typed-array builders so the metadata-bearing `Schema` remains visible and reaches the public `RecordBatch` constructor.

[SCOPE_SPLIT]:
- Compute owns the columnar table it *builds*; the Persistence `api-arrow` overlay owns everything that *carries* it — `ArrowStreamWriter`/`ArrowFileWriter` IPC serialisation, the `Apache.Arrow.Compression.CompressionCodecFactory` LZ4-frame/Zstandard codec, the ADBC `AdbcConnection`/`AdbcStatement` query surface, and the `FlightClient`/`FlightSqlClient` transport. Compute holds one `Apache.Arrow` reference and never references the four egress packages.
- Sealed `RecordBatch` crosses to Persistence over the existing `Runtime/transport` wire plane, not a second Arrow-owned transport: `Runtime/codecs#FLIGHT_SQL_PUSH` (the reciprocal Persistence port that redeems the content-keyed DOE batch over `Apache.Arrow.Flight.Sql`) is the Persistence-side obligation, so this overlay stops at the sealed `RecordBatch` and the wire push is the transport page's concern — Compute never opens a Flight listener.
- `GeoArrowRequest.ArrowIpc` (`Runtime/codecs#TWO_HOP_TESSELLATION`, `ReadOnlyMemory<byte>`) is Arrow IPC bytes the Python geospatial branch already encoded; Compute relays them opaque to the companion and never decodes or re-encodes them — that pass-through needs no `Apache.Arrow` member, so this overlay's build surface is distinct from the GeoArrow relay.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- surrogate-training egress is one rail: `SweepLane.Run` lands a `SweepResult` → `SweepLane.Dataset` folds it into a `DoeDataset` (content-keyed via `XxHash128.HashToUInt128`) → this Arrow build projects the labeled table into one `RecordBatch` (design-point columns + objective columns + `OnFront` mask, metadata carrying the content key) → the batch crosses to the Python graduation companion → a graduated ONNX surrogate returns over `GraduationEvidence` to `Solver/optimizer`, so labeled sweep data trains the surrogate refresh instead of dying in receipts — the identical wire plane the `DoeDataset` shape already commits to, with no new transport.
- billing egress folds the same construction surface: `ChargebackDataset.Of` aggregates the tenant-partitioned journal into `ChargebackRow`s, and the content-keyed chargeback batch builds through the same typed-array/`Schema.Builder`/`RecordBatch` constructor path — one Arrow construction owner, two dataset producers, never a per-dataset bespoke columnar encoder.
- NodaTime clock seam (`api-nodatime`) crosses at the `TimestampArray` column: an `Instant` projects to the epoch column under `TimestampType.Default` at the builder edge, the same instant seam the receipt stream and the Persistence relational store share, so the Arrow wire, the receipt fold, and the store agree on one clock.

[RAIL_LAW]:
- Package: `Apache.Arrow` (Compute references core `Apache.Arrow` alone)
- Owns: the columnar-table *construction* seam — typed-array builders, `Schema.Builder`/`Field.Builder`, the public `RecordBatch` constructor, metadata-free `RecordBatch.Builder`, the `IArrowType` `.Default` descriptors, and `MemoryAllocator` — projecting `DoeDataset`/`ChargebackDataset` into a self-describing `RecordBatch`
- Accept: whole-span column appends pre-sized by `Reserve`, `Schema.Builder.Metadata` carrying DOE content/strategy/instant/point facts or chargeback content/window facts, `new RecordBatch(schema, arrays, points)`, metadata-free `RecordBatch.Builder` use, and an `Instant` projected through `TimestampType.Default`
- Reject: `RecordBatch.Builder` for a metadata-bearing batch; a per-element `Append(T)` loop where a `ReadOnlySpan<T>` bulk append exists; a hand-rolled columnar byte layout `RecordBatch` already owns; divergent schema-field and array order; a bare `DateTime` column where the NodaTime clock seam owns the instant; the shared `MemoryAllocator.Default` where a lane-bounded arena is available; any IPC/ADBC/Flight/compression member; a Compute-side Flight listener or re-encode of the opaque `GeoArrowRequest.ArrowIpc` relay bytes
