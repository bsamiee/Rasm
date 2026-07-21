# [RASM_COMPUTE_API_ARROW]

`Apache.Arrow` is the columnar in-memory format Compute builds its analytical result tables into: `RecordBatch.Builder` assembles typed columns, `Schema.Builder`/`Field.Builder` name and type them, `PrimitiveArrayBuilder<T,…>` families bulk-append a `ReadOnlySpan<T>` column in one call, and `MemoryAllocator` owns the buffer arena each builder draws from.

This overlay owns ONLY that construction-and-projection seam — the columnar table Compute *produces*; IPC stream/file serialisation (`ArrowStreamWriter`/`ArrowFileWriter`), the LZ4-frame/Zstandard `CompressionCodecFactory`, the ADBC query surface, and the Flight/Flight-SQL transport that *carries* the table are the Persistence `api-arrow` overlay's egress rails and are NOT restated here.

`Runtime/codecs#ARROW_BATCH` is the armed seam: `SweepLane.Dataset` projects a landed `DoeDataset` (`ReadOnlyMemory<double> Coordinates`/`Responses`, `ReadOnlyMemory<bool> OnFront`, `Instant At`) into one `RecordBatch` without per-row copies — each column bulk-appends its backing span through `DoubleArray.Builder.Append(ReadOnlySpan<double>)` / `BooleanArray.Builder.Append(ReadOnlySpan<bool>)`, `ContentKey`/`Strategy` ride `Schema.Builder.Metadata`, and the batch crosses to the Python graduation companion over the existing `Runtime/transport` wire plane; `ChargebackDataset` folds the same construction surface for the content-keyed billing egress.

Every builder is a freely-constructed per-instance object; `MemoryAllocator.Default` is the one process-global, and every builder and `Build` entry takes a per-instance `MemoryAllocator allocator = null` parameter, so a bounded per-lane arena injects at the seam and the default is the shared fallback — no host-global singleton, no single-owner admission demand.

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
|  [02]   | `RecordBatch.Builder`        | builder           | assembles typed columns into a batch under a `MemoryAllocator`           |
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

[ENTRYPOINT_SCOPE]: `RecordBatch.Builder` column assembly and build
- rail: columnar-egress
- note: `new RecordBatch.Builder(allocator?)` opens the batch under a per-instance arena; each `Append<TArray>(name, nullable, array)` adds one built column and `Build()` yields the immutable `RecordBatch`; the `ArrayBuilder` fluent arms (`.Double(b => …)`) construct a column inline against the batch's own allocator. A batch built without a matching `Schema` field order is the drift defect the `Schema.Builder` co-build guards.
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
- note: `Append(ReadOnlySpan<T>)` bulk-appends a whole backing span in one call — the zero-row-copy path for the `DoeDataset` `ReadOnlyMemory<double>` columns (`.Span`); `AppendNull` writes a validity-bitmap null; `Build(allocator?)` seals the column. `Reserve(capacity)` pre-sizes the buffer to the known point count before the span append.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<T> span)`         | bulk append    | appends the whole column span with no per-row iteration      |
|  [02]   | `AppendRange(IEnumerable<T> values)`   | range append   | appends an enumerable column source                          |
|  [03]   | `Append(T value)` / `Append(T? value)` | scalar append  | appends one value (nullable overload writes validity)        |
|  [04]   | `AppendNull()`                         | null append    | appends a validity-bitmap null slot                          |
|  [05]   | `Reserve(int capacity)` / `Resize(int)`| pre-size       | pre-allocates or resizes the backing buffer                  |
|  [06]   | `Set(int index, T value)` / `Swap(i,j)`| edit           | in-place value set / positional swap before build            |
|  [07]   | `Build(MemoryAllocator allocator = null)` | factory call | seals the immutable typed array under the arena              |

[ENTRYPOINT_SCOPE]: `Schema.Builder` / `Field.Builder` and `Table` assembly
- rail: columnar-egress
- note: the schema is co-built with the batch — one `Field` per column in append order, `Metadata(key, value)` carrying the `ContentKey`/`Strategy`/`At` receipt facts; `TableFromRecordBatches` collects streamed batches into one queryable `Table`.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `new Schema.Builder()` / `.Build()`                    | builder        | opens and seals an immutable `Schema`     |
|  [02]   | `Schema.Builder.Field(Field)` / `.Field(Action<…>)`    | field add      | adds a field by value or inline builder   |
|  [03]   | `Schema.Builder.Metadata(key, value)`                  | metadata       | attaches schema-level receipt facts       |
|  [04]   | `Field.Builder.Name(s).DataType(t).Nullable(b).Build()`| builder        | assembles one field from parts            |
|  [05]   | `new Field(name, IArrowType, nullable, metadata?)`     | ctor           | direct field construction                 |
|  [06]   | `Table.TableFromRecordBatches(Schema, IList<batch>)`   | table factory  | collects batches into one `Table`         |
|  [07]   | `MemoryAllocator.Default.Value` / `Allocate(int)`      | arena          | shared default arena; `Allocate` a buffer |

## [04]-[IMPLEMENTATION_LAW]

[BATCH_TOPOLOGY]:
- `RecordBatch.Builder(allocator)` is the one construction root: the sweep-egress owner opens a builder under a per-lane `MemoryAllocator`, co-builds a `Schema` whose fields match the column append order one-for-one, appends each typed column, and `Build()` seals the immutable batch — the schema field order and the batch column order are the same sequence, never two independently-ordered lists the reader must reconcile.
- each `DoeDataset` column bulk-appends its backing span: `Coordinates`/`Responses` (`ReadOnlyMemory<double>`) drive `DoubleArray.Builder.Append(coordinates.Span)`, `OnFront` (`ReadOnlyMemory<bool>`) drives `BooleanArray.Builder.Append(onFront.Span)`, and `Reserve(points)` pre-sizes each buffer to the known row count before the span append — the `Runtime/codecs#ARROW_BATCH` "without row copies" contract is the whole-span append, never a per-element `Append(T)` loop over the memory.
- `ContentKey`, `Strategy` (the `DoeDesign` key), and `At` receipt facts ride `Schema.Builder.Metadata(key, value)` string pairs, so the batch is self-describing across the wire — the reader recovers the content key from schema metadata rather than a side-channel, and a batch whose metadata omits the content key is the drift defect.
- `MemoryAllocator` is per-instance-injectable at every builder and `Build` entry (`Builder(allocator)`, `Build(allocator)`); `MemoryAllocator.Default` (a `Lazy<MemoryAllocator>`) is the process-global fallback the seam takes only when no bounded arena is supplied. A staging-bounded lane (`Tensor/memory#STAGING_POOL`) passes its own allocator so the batch buffers charge against the lane budget, never the shared default — the allocator is design policy, not an implicit global.

[TYPE_SEAM]:
- `Field.Builder.DataType` takes an `IArrowType`; the `.Default` singletons (`DoubleType.Default`, `BooleanType.Default`, `StringType.Default`) type the scalar columns, and `TimestampType.Default` (`TimeUnit.Millisecond`, UTC) types the `Instant At` column so the Arrow wire carries the same NodaTime clock seam (`api-nodatime`) the receipt stream uses — a bare `DateTime` column is the rejected form, symmetric to the Persistence egress rule.
- `RecordBatch.Builder.ArrayBuilder` exposes a fluent arm (`.Double(b => b.Append(span))`) that constructs a column inline against the batch's own allocator, an alternative to the explicit `Append<TArray>(name, nullable, builtArray)` add — the sweep owner picks the explicit form to keep the `Schema` field co-build visible, and the fluent form stays available where a column is trivially inline.

[SCOPE_SPLIT]:
- Compute owns the columnar table it *builds*; the Persistence `api-arrow` overlay owns everything that *carries* it — `ArrowStreamWriter`/`ArrowFileWriter` IPC serialisation, the `Apache.Arrow.Compression.CompressionCodecFactory` LZ4-frame/Zstandard codec, the ADBC `AdbcConnection`/`AdbcStatement` query surface, and the `FlightClient`/`FlightSqlClient` transport. Compute holds one `Apache.Arrow` reference and never references the four egress packages.
- Sealed `RecordBatch` crosses to Persistence over the existing `Runtime/transport` wire plane, not a second Arrow-owned transport: `Runtime/codecs#FLIGHT_SQL_PUSH` (the reciprocal Persistence port that redeems the content-keyed DOE batch over `Apache.Arrow.Flight.Sql`) is the Persistence-side obligation, so this overlay stops at the sealed `RecordBatch` and the wire push is the transport page's concern — Compute never opens a Flight listener.
- `GeoArrowRequest.ArrowIpc` (`Runtime/codecs#TWO_HOP_TESSELLATION`, `ReadOnlyMemory<byte>`) is Arrow IPC bytes the Python geospatial branch already encoded; Compute relays them opaque to the companion and never decodes or re-encodes them — that pass-through needs no `Apache.Arrow` member, so this overlay's build surface is distinct from the GeoArrow relay.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- surrogate-training egress is one rail: `SweepLane.Run` lands a `SweepResult` → `SweepLane.Dataset` folds it into a `DoeDataset` (content-keyed via `XxHash128.HashToUInt128`) → this Arrow build projects the labeled table into one `RecordBatch` (design-point columns + objective columns + `OnFront` mask, metadata carrying the content key) → the batch crosses to the Python graduation companion → a graduated ONNX surrogate returns over `GraduationEvidence` to `Solver/optimizer`, so labeled sweep data trains the surrogate refresh instead of dying in receipts — the identical wire plane the `DoeDataset` shape already commits to, with no new transport.
- billing egress folds the same construction surface: `ChargebackDataset.Of` aggregates the tenant-partitioned journal into `ChargebackRow`s, and the content-keyed chargeback batch builds through the same `RecordBatch.Builder`/`Schema.Builder` path — one Arrow construction owner, two dataset producers, never a per-dataset bespoke columnar encoder.
- NodaTime clock seam (`api-nodatime`) crosses at the `TimestampArray` column: an `Instant` projects to the epoch column under `TimestampType.Default` at the builder edge, the same instant seam the receipt stream and the Persistence relational store share, so the Arrow wire, the receipt fold, and the store agree on one clock.

[RAIL_LAW]:
- Package: `Apache.Arrow` (Compute references core `Apache.Arrow` alone)
- Owns: the columnar-table *construction* seam — `RecordBatch.Builder`/`Schema.Builder`/`Field.Builder`, the `PrimitiveArrayBuilder<T,…>` bulk-span append families, the `IArrowType` `.Default` descriptors, and `MemoryAllocator` — projecting `DoeDataset`/`ChargebackDataset` into a self-describing `RecordBatch`
- Accept: `new RecordBatch.Builder(perLaneAllocator)` with a co-built `Schema`, whole-span column appends (`DoubleArray.Builder.Append(span)`/`BooleanArray.Builder.Append(span)`) pre-sized by `Reserve`, `Schema.Builder.Metadata` carrying `ContentKey`/`Strategy`/`At`, and an `Instant` projected through `TimestampType.Default`
- Reject: a per-element `Append(T)` loop where a `ReadOnlySpan<T>` bulk append exists; a hand-rolled columnar byte layout `RecordBatch` already owns; a schema field order that diverges from the batch column order; a bare `DateTime` column where the NodaTime clock seam owns the instant; the shared `MemoryAllocator.Default` where a lane-bounded arena is available; any IPC/ADBC/Flight/compression member (owned by the Persistence `api-arrow` overlay); a Compute-side Flight listener or re-encode of the opaque `GeoArrowRequest.ArrowIpc` relay bytes
