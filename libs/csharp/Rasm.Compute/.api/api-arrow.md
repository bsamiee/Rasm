# [RASM_COMPUTE_API_ARROW]

`Apache.Arrow` is the columnar in-memory format Compute builds its analytical result tables into: `RecordBatch.Builder` and the public `RecordBatch` constructor co-order typed columns under an explicit `Schema`, the `PrimitiveArrayBuilder<T,…>` families bulk-append a `ReadOnlySpan<T>` per column, and `MemoryAllocator` owns the buffer arena each builder draws from. This overlay owns only the construction seam — the columnar table Compute produces — folding a landed `DoeDataset` or `ChargebackDataset` into one self-describing `RecordBatch` on the columnar-egress rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Apache.Arrow`
- package: `Apache.Arrow` (Apache-2.0)
- assembly: `Apache.Arrow`
- namespace: `Apache.Arrow`, `Apache.Arrow.Types`, `Apache.Arrow.Memory`
- asset: pure-managed runtime library, AnyCPU, no native RID; the `net10.0` consumer binds `lib/net8.0`
- abi: `RecordBatch`/`Schema`/`Field` are reference types; `MemoryAllocator` is `abstract` with a `Lazy<MemoryAllocator> Default`
- rail: columnar-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: record, schema, field, array-builder, and type-descriptor construction (`Apache.Arrow`, `.Types`, `.Memory`)
- note: every symbol is per-instance constructed, never a static handle; each primitive builder derives `PrimitiveArrayBuilder<T, TArray, TBuilder>` with a uniform append/build set (`StringArray.Builder` adds `.Append(string)`), and each `IArrowType.Default` singleton feeds `Field.Builder.DataType`.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :--------------------------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `RecordBatch`                      | record container | columnar batch with schema; `: IArrowRecord, IArrowArray`, `IDisposable` |
|  [02]   | `RecordBatch.Builder`              | builder          | co-orders fields and arrays; carries no schema metadata seat             |
|  [03]   | `RecordBatch.Builder.ArrayBuilder` | fluent factory   | per-column typed builder (`.Double`/`.Boolean`/`.Int64` `Action` arms)   |
|  [04]   | `Schema`                           | schema value     | ordered field list plus metadata; `this[int]`/`this[string]` field index |
|  [05]   | `Schema.Builder`                   | builder          | assembles fields and metadata into an immutable `Schema`                 |
|  [06]   | `Field`                            | field value      | name, `IArrowType`, nullability, metadata                                |
|  [07]   | `Field.Builder`                    | builder          | assembles a field from name/type/nullable/metadata parts                 |
|  [08]   | `Table`                            | table value      | schema plus chunked column list; `TableFromRecordBatches` factory        |
|  [09]   | `IArrowArrayBuilder<TArray>`       | builder contract | `Build(MemoryAllocator)` the `RecordBatch.Builder.Append` builder arm    |
|  [10]   | `DoubleArray.Builder`              | float builder    | `Coordinates`/`Responses` double columns                                 |
|  [11]   | `BooleanArray.Builder`             | bool builder     | `OnFront` validity-bitmap bool column                                    |
|  [12]   | `Int64Array.Builder`               | int builder      | 64-bit counts, cardinalities                                             |
|  [13]   | `Int32Array.Builder`               | int builder      | 32-bit integer columns                                                   |
|  [14]   | `FloatArray.Builder`               | float builder    | 32-bit float columns                                                     |
|  [15]   | `TimestampArray.Builder`           | temporal         | `Instant At` epoch column via NodaTime                                   |
|  [16]   | `StringArray.Builder`              | binary builder   | UTF-8 axis/objective label columns                                       |
|  [17]   | `DoubleType.Default`               | type singleton   | `IArrowType` for a `double` field                                        |
|  [18]   | `BooleanType.Default`              | type singleton   | `IArrowType` for a `bool` field                                          |
|  [19]   | `TimestampType.Default`            | type value       | `(TimeUnit, timezone)`; the epoch-column type                            |
|  [20]   | `StringType.Default`               | type singleton   | `IArrowType` for a UTF-8 string field                                    |
|  [21]   | `IArrowType`                       | type contract    | `TypeId`/`Name`; the `Field.Builder.DataType` input                      |
|  [22]   | `MemoryAllocator`                  | buffer arena     | `abstract`; `Allocate(int) → IMemoryOwner<byte>`                         |
|  [23]   | `MemoryAllocator.Default`          | shared arena     | `Lazy<MemoryAllocator>`; process-global fallback                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metadata-free `RecordBatch.Builder` assembly, typed-column bulk-append, and metadata-bearing `Schema`/`RecordBatch`/`Table` construction
- note: `Append(ReadOnlySpan<T>)` copies a whole backing span in one call — the reduced-call path for the `DoeDataset` `ReadOnlyMemory<double>` columns via `.Span`, `Reserve(capacity)` pre-sizing the buffer first; `RecordBatch.Builder` carries no `Schema`/`Metadata` seat, so a receipt-bearing batch builds through the explicit `Schema` and the public `RecordBatch` constructor.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new RecordBatch.Builder(allocator = null)`              | ctor     | opens a batch builder under an arena        |
|  [02]   | `Append<TArray>(name, nullable, TArray array)`           | instance | adds one built typed column by name         |
|  [03]   | `Append<TArray>(name, nullable, IArrowArrayBuilder<T>)`  | instance | adds a column from an unbuilt builder       |
|  [04]   | `Append(RecordBatch batch)`                              | instance | merges schema and arrays from a batch       |
|  [05]   | `Build()` / `Clear()`                                    | factory  | seals the immutable `RecordBatch` / resets  |
|  [06]   | `ArrayBuilder.Double(Action<DoubleArray.Builder>)`       | factory  | builds a `DoubleArray` column inline        |
|  [07]   | `Append(ReadOnlySpan<T> span)`                           | instance | copies one whole span, no scalar loop       |
|  [08]   | `AppendRange(IEnumerable<T> values)`                     | instance | appends an enumerable column source         |
|  [09]   | `Append(T value)` / `Append(T? value)`                   | instance | appends one value; nullable writes validity |
|  [10]   | `AppendNull()`                                           | instance | appends a validity-bitmap null slot         |
|  [11]   | `Reserve(int capacity)` / `Resize(int)`                  | instance | pre-allocates or resizes the backing buffer |
|  [12]   | `Set(int index, T value)` / `Swap(i, j)`                 | instance | in-place value set / positional swap        |
|  [13]   | `Build(MemoryAllocator allocator = null)`                | factory  | seals the immutable typed array             |
|  [14]   | `new Schema.Builder()` / `.Build()`                      | ctor     | opens and seals an immutable `Schema`       |
|  [15]   | `Schema.Builder.Field(Field)` / `.Field(Action<…>)`      | instance | adds a field by value or inline builder     |
|  [16]   | `Schema.Builder.Metadata(key, value)`                    | instance | attaches schema-level receipt facts         |
|  [17]   | `Field.Builder.Name(s).DataType(t).Nullable(b).Build()`  | factory  | assembles one field from parts              |
|  [18]   | `new Field(name, IArrowType, nullable, metadata?)`       | ctor     | direct field construction                   |
|  [19]   | `new RecordBatch(Schema, IEnumerable<IArrowArray>, int)` | ctor     | binds metadata schema, arrays, and length   |
|  [20]   | `Table.TableFromRecordBatches(Schema, IList<batch>)`     | static   | collects batches into one `Table`           |
|  [21]   | `MemoryAllocator.Default.Value` / `Allocate(int)`        | property | shared default arena; `Allocate` a buffer   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Metadata-bearing construction has one root: build typed arrays under the per-lane `MemoryAllocator`, build one `Schema` whose fields match the array order one-for-one and whose `Metadata` carries every receipt fact, then call `new RecordBatch(schema, arrays, points)`. `RecordBatch.Builder` exposes neither `Schema` injection nor `Metadata`, so it discards `content_key`/`strategy`/`at`/`points`, and a batch whose metadata omits the content key is the drift defect.
- Each `DoeDataset` column bulk-appends its backing span — `Coordinates`/`Responses` through `DoubleArray.Builder.Append(span)`, `OnFront` through `BooleanArray.Builder.Append(span)`, `Reserve(points)` pre-sizing each allocator-owned buffer — never a scalar `Append(T)` loop.
- `MemoryAllocator` injects through typed-array `Build(allocator)` and `RecordBatch.Builder(allocator)`; `Schema.Builder.Build()` and `RecordBatch.Builder.Build()` take none, so a staging-bounded lane (`Tensor/memory#STAGING_POOL`) passes its allocator to every array build and buffers charge the lane budget.
- `IArrowType.Default` singletons type the scalar columns and `TimestampType.Default` (`TimeUnit.Millisecond`, UTC) types the `Instant At` column, so the Arrow wire carries the same NodaTime clock seam the receipt stream uses; a bare `DateTime` column is the rejected form.

[STACKING]:
- `api-nodatime`(`libs/csharp/.api/api-nodatime.md`): an `Instant` projects to the `TimestampArray` epoch column under `TimestampType.Default` at the builder edge — the one clock seam the Arrow wire, the receipt fold, and the Persistence store share.
- `api-arrow`(`libs/csharp/Rasm.Persistence/.api/api-arrow.md`): a sealed `RecordBatch` crosses to the Persistence egress overlay over the `Runtime/transport` wire plane, redeemed at `Runtime/codecs#FLIGHT_SQL_PUSH`; Compute stops at the sealed batch and opens no Arrow-owned transport.
- within-lib: `SweepLane.Run` lands a `SweepResult`, `SweepLane.Dataset` folds it into a content-keyed `DoeDataset`, this build projects the labeled table into one `RecordBatch` crossing to the Python graduation companion, which returns a graduated ONNX surrogate over `GraduationEvidence` to `Solver/optimizer`; `ChargebackDataset.Of` folds the identical typed-array/`Schema.Builder`/`RecordBatch` path for billing egress — one construction owner, two dataset producers.

[LOCAL_ADMISSION]:
- Compute references core `Apache.Arrow` alone; the `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.Sql`, and `Apache.Arrow.Compression` egress packages are the Persistence overlay's and absent from the Compute closure.
- `GeoArrowRequest.ArrowIpc` (`Runtime/codecs#TWO_HOP_TESSELLATION`) carries Arrow IPC bytes the Python geospatial branch already encoded; Compute relays them opaque and decodes nothing, so no `Apache.Arrow` member touches that path.

[RAIL_LAW]:
- Package: `Apache.Arrow`
- Owns: the columnar-table construction seam — typed-array builders, `Schema.Builder`/`Field.Builder`, the public `RecordBatch` constructor, metadata-free `RecordBatch.Builder`, the `IArrowType.Default` descriptors, and `MemoryAllocator` — projecting `DoeDataset`/`ChargebackDataset` into a self-describing `RecordBatch`
- Accept: whole-span column appends pre-sized by `Reserve`, `Schema.Builder.Metadata` carrying receipt facts, `new RecordBatch(schema, arrays, points)`, metadata-free `RecordBatch.Builder` use, and an `Instant` through `TimestampType.Default`
- Reject: `RecordBatch.Builder` for a metadata-bearing batch; a per-element `Append(T)` loop where a span append exists; a hand-rolled columnar layout `RecordBatch` owns; divergent schema-field and array order; a bare `DateTime` column; the shared `MemoryAllocator.Default` where a lane arena exists; any IPC/ADBC/Flight/compression member; a Compute-side Flight listener or re-encode of the opaque `GeoArrowRequest.ArrowIpc` relay
