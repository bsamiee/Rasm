# [RASM_API_ARROW]

`Apache.Arrow` owns the columnar in-memory format and Arrow IPC file/stream serialisation, minting the `IArrowArrayStream` contract every analytical egress meets at. Ownership splits BY AXIS, never by package.

| [INDEX] | [AXIS]                                                             | [OWNER]                                                          |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | declaration → `(Schema, columns, builders, order, metadata)` FOLD  | `Rasm.Persistence` `Query/backend.md` `ArrowLanding.Build<TRow>` |
|  [02]   | dataset PROJECTION, zero-copy arena wrap, `PackSchema` conformance | `Rasm.Compute` `Runtime/codecs.md`                               |
|  [03]   | IPC, ADBC, and Flight egress behind one `IArrowArrayStream`        | `Rasm.Persistence`                                               |

Row `[01]` derives field list, field order, and every column's own builder from the one `AnalyticsSchema`/`ColumnType`/`ColumnShape` declaration; row `[02]` keeps the `Strided` gather, the `on_front` mask, and the arena borrow, which are dataset-shaped. Persistence-local ADBC, Flight, Flight-hosting, Flight-SQL, and IPC-compression packages catalogue at `Rasm.Persistence/.api/api-arrow-egress.md`.

## [01]-[PUBLIC_TYPES]

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
|  [08]   | `ChunkedArray`                     | chunked array    | list of same-type arrays                                                 |
|  [09]   | `Table`                            | table value      | schema plus chunked column list; `TableFromRecordBatches` factory        |
|  [10]   | `IArrowArray`                      | array contract   | the `RecordBatch` column element every lane erases to                    |
|  [11]   | `IArrowRecord`                     | record contract  | schema-plus-arrays capability                                            |
|  [12]   | `IArrowArrayBuilder<TArray>`       | builder contract | `Build(MemoryAllocator)` the `RecordBatch.Builder.Append` builder arm    |
|  [13]   | `MemoryAllocator`                  | buffer arena     | `abstract`; `Allocate(int) → IMemoryOwner<byte>`                         |
|  [14]   | `MemoryAllocator.Default`          | shared arena     | `Lazy<MemoryAllocator>`; process-global fallback                         |
|  [15]   | `ArrowBuffer`                      | buffer value     | `readonly struct`; wraps a `ReadOnlyMemory<byte>` with no copy           |
|  [16]   | `FixedSizeListArray`               | nested array     | fixed child count; states a channel's arity interleave                   |
|  [17]   | `FixedSizeListType`                | nested type      | `NestedType`; `ListSize` + `ValueDataType`, `listSize <= 0` throws       |

[PUBLIC_TYPE_SCOPE]: primitive array family — each with its nested `Builder`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                              |
| :-----: | :---------------- | :------------- | :---------------------------------------- |
|  [01]   | `BooleanArray`    | bool array     | validity-bitmap boolean values            |
|  [02]   | `Int8Array`       | integer array  | signed 8-bit values                       |
|  [03]   | `Int16Array`      | integer array  | signed 16-bit values                      |
|  [04]   | `Int32Array`      | integer array  | signed 32-bit values                      |
|  [05]   | `Int64Array`      | integer array  | signed 64-bit values                      |
|  [06]   | `UInt8Array`      | integer array  | unsigned 8-bit values; the unorm8 lane    |
|  [07]   | `UInt16Array`     | integer array  | unsigned 16-bit values                    |
|  [08]   | `UInt32Array`     | integer array  | unsigned 32-bit values                    |
|  [09]   | `UInt64Array`     | integer array  | unsigned 64-bit values                    |
|  [10]   | `FloatArray`      | float array    | `PrimitiveArray<float>`; the float32 lane |
|  [11]   | `HalfFloatArray`  | half array     | `PrimitiveArray<Half>`; the float16 lane  |
|  [12]   | `DoubleArray`     | float array    | 64-bit double values                      |
|  [13]   | `Decimal128Array` | decimal array  | 128-bit fixed-point values                |
|  [14]   | `Decimal256Array` | decimal array  | 256-bit fixed-point values                |
|  [15]   | `StringArray`     | binary array   | UTF-8 string values                       |
|  [16]   | `BinaryArray`     | binary array   | opaque byte sequences                     |
|  [17]   | `TimestampArray`  | temporal array | epoch timestamps under a `TimestampType`  |
|  [18]   | `Date32Array`     | temporal array | days-since-epoch dates                    |
|  [19]   | `DurationArray`   | temporal array | duration values                           |

[PUBLIC_TYPE_SCOPE]: nested container arrays — each assembles from CHILD ARRAYS, not from a typed child builder
- note: `ListArray.Builder` and `MapArray.Builder` expose their children only as `IArrowArrayBuilder<IArrowArray, …>`, which carries no typed append, and `DictionaryArray` and `FixedSizeBinaryArray` ship no concrete builder at all — so a composing owner builds each child array through its own element builder and binds the container's public constructor over it.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]    | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `ListArray`                                         | nested array     | variable-length runs; `ValueOffsets`/`Values` children        |
|  [02]   | `ListArray.Builder`                                 | builder          | `Builder(IArrowType)`; child face is UNTYPED `ValueBuilder`   |
|  [03]   | `MapArray`                                          | nested array     | `: ListArray`; `Keys`/`Values` off one `KeyValues` struct     |
|  [04]   | `MapArray.Builder`                                  | builder          | `Builder(MapType)` alone; `KeyBuilder`/`ValueBuilder` untyped |
|  [05]   | `DictionaryArray`                                   | encoded array    | `Indices` + `Dictionary`; ships NO builder — ctor is the path |
|  [06]   | `StructArray`                                       | nested array     | `: IArrowRecord`; `Fields` list; the map entries carrier      |
|  [07]   | `FixedSizeBinaryArray`                              | binary array     | `Apache.Arrow.Arrays`; `GetBytes(i)` at the type's width      |
|  [08]   | `FixedSizeBinaryArray.BuilderBase<TArray,TBuilder>` | abstract builder | the ONLY builder shape here; no concrete `Builder` ships      |
|  [09]   | `ArrayData`                                         | array payload    | `sealed`; type, length, nulls, offset, buffers, children      |
|  [10]   | `ArrowBuffer.Builder<T>`                            | buffer builder   | `where T : struct`; `bool` THROWS — use `BitmapBuilder`       |

[PUBLIC_TYPE_SCOPE]: type system, IPC, and stream contract

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                                |
| :-----: | :------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ArrowType`                | type base       | root type for all Arrow types                                               |
|  [02]   | `ArrowTypeId`              | type enum       | discriminates Arrow type identities                                         |
|  [03]   | `IArrowType`               | type contract   | `TypeId`/`Name`; the `Field.Builder.DataType` input                         |
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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metadata-free `RecordBatch.Builder` assembly, typed-column bulk-append, and metadata-bearing `Schema`/`RecordBatch`/`Table` construction
- note: `Append(ReadOnlySpan<T>)` copies a whole backing span in one call — the reduced-call path for the `DoeDataset` `ReadOnlyMemory<double>` columns via `.Span`, `Reserve(capacity)` pre-sizing the buffer first; `RecordBatch.Builder` carries no `Schema`/`Metadata` seat, so a metadata-bearing batch builds through the explicit `Schema` and the public `RecordBatch` constructor. Null `StringArray` appends land as a validity-bitmap null.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new RecordBatch.Builder(allocator = null)`                                   | ctor     | opens a batch builder under an arena        |
|  [02]   | `Append<TArray>(name, nullable, TArray array)`                                | instance | adds one built typed column by name         |
|  [03]   | `Append<TArray>(name, nullable, IArrowArrayBuilder<T>)`                       | instance | adds a column from an unbuilt builder       |
|  [04]   | `Append(RecordBatch batch)`                                                   | instance | merges schema and arrays from a batch       |
|  [05]   | `Build()` / `Clear()`                                                         | factory  | seals the immutable `RecordBatch` / resets  |
|  [06]   | `ArrayBuilder.Double(Action<DoubleArray.Builder>)`                            | factory  | builds a `DoubleArray` column inline        |
|  [07]   | `Append(ReadOnlySpan<T> span)`                                                | instance | copies one whole span, no scalar loop       |
|  [08]   | `AppendRange(IEnumerable<T> values)`                                          | instance | appends an enumerable column source         |
|  [09]   | `Append(T value)` / `Append(T? value)`                                        | instance | appends one value; nullable writes validity |
|  [10]   | `AppendNull()`                                                                | instance | appends a validity-bitmap null slot         |
|  [11]   | `Reserve(int capacity)` / `Resize(int)`                                       | instance | pre-allocates or resizes the backing buffer |
|  [12]   | `Set(int index, T value)` / `Swap(i, j)`                                      | instance | in-place value set / positional swap        |
|  [13]   | `Build(MemoryAllocator allocator = null)`                                     | factory  | seals the immutable typed array             |
|  [14]   | `new Schema.Builder()` / `.Build()`                                           | ctor     | opens and seals an immutable `Schema`       |
|  [15]   | `Schema.Builder.Field(Field)` / `.Field(Action<…>)`                           | instance | adds a field by value or inline builder     |
|  [16]   | `Schema.Builder.Metadata(key, value)`                                         | instance | attaches schema-level operation facts       |
|  [17]   | `Field.Builder.Name(s).DataType(t).Nullable(b).Build()`                       | factory  | assembles one field from parts              |
|  [18]   | `new Field(name, IArrowType, nullable, metadata?)`                            | ctor     | direct field construction                   |
|  [19]   | `new Schema(IEnumerable<Field>, IEnumerable<KVP>)`                            | ctor     | ordered field list, metadata nullable       |
|  [20]   | `new RecordBatch(Schema, IEnumerable<IArrowArray>, int)`                      | ctor     | binds metadata schema, arrays, and length   |
|  [21]   | `Table.TableFromRecordBatches(Schema, IList<batch>)`                          | static   | collects batches into one `Table`           |
|  [22]   | `MemoryAllocator.Default.Value` / `Allocate(int)`                             | property | shared default arena; `Allocate` a buffer   |
|  [23]   | `Schema.FieldsList -> IReadOnlyList<Field>`                                   | property | ordered field vocabulary; schema identity   |
|  [24]   | `RecordBatch.Schema` / `.Length` / `.Arrays`                                  | property | sealed batch reads back schema and length   |
|  [25]   | `RecordBatch.Slice(offset, length)` / `SliceShared`                           | instance | windows a batch without copying buffers     |
|  [26]   | `RecordBatch.Column(name)` / `Column(int)`                                    | property | reads one `IArrowArray` column              |
|  [27]   | `Field.Name` / `Field.DataType -> IArrowType`                                 | property | the `(name, TypeId)` pair a digest folds    |
|  [28]   | `new ArrowBuffer(ReadOnlyMemory<byte> data)`                                  | ctor     | borrows foreign memory; no copy, no arena   |
|  [29]   | `ArrowBuffer.Empty`                                                           | static   | absent validity bitmap on a dense array     |
|  [30]   | `new FloatArray(ArrowBuffer, ArrowBuffer, int, int, int)`                     | ctor     | value buffer, bitmap, length, nulls, offset |
|  [31]   | `new FixedSizeListType(IArrowType value, int listSize)`                       | ctor     | fixed child count; names the child `item`   |
|  [32]   | `new FixedSizeListArray(IArrowType, int, IArrowArray, ArrowBuffer, int, int)` | ctor     | flat child at a fixed stride                |
|  [33]   | `new TimestampArray.Builder(TimestampType)`                                   | ctor     | builds under the field's own unit and zone  |
|  [34]   | `TimestampArray.Builder.AppendRange(DateTimeOffset)`                          | instance | appends instants at the builder's unit      |

[ENTRYPOINT_SCOPE]: nested container assembly — the constructor path every container array publishes
- note: each container binds a child array and its own offsets or index run, so the child builds once through its element type's own builder and never re-appends through the untyped nested builder face; `ArrowBuffer.Builder<int>` carries the offsets and `ArrowBuffer.Empty` fills the absent validity bitmap. Trailing `nullCount`/`offset` parameters default to zero and are elided below.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `new ListArray(IArrowType, int, ArrowBuffer, IArrowArray, ArrowBuffer)`   | ctor     | binds offsets and one child values array    |
|  [02]   | `new ListArray.Builder(IArrowType)` / `(Field)`                           | ctor     | opens a run builder over an untyped child   |
|  [03]   | `ListArray.Builder.Append()` / `.AppendNull()` / `.Build(alloc)`          | instance | opens a run, a null run, or seals the array |
|  [04]   | `new MapArray(IArrowType, int, ArrowBuffer, IArrowArray, ArrowBuffer)`    | ctor     | binds offsets and one entries `StructArray` |
|  [05]   | `new MapArray.Builder(MapType)`                                           | ctor     | the only builder ctor; no `IArrowType` arm  |
|  [06]   | `new StructArray(IArrowType, int, IEnumerable<IArrowArray>, ArrowBuffer)` | ctor     | the map entries carrier over key and value  |
|  [07]   | `new DictionaryArray(DictionaryType, IArrowArray, IArrowArray)`           | ctor     | index run and value roster; the whole path  |
|  [08]   | `new ArrayData(IArrowType, int, int, int, IEnumerable<ArrowBuffer>)`      | ctor     | raw payload for a builderless array         |
|  [09]   | `new FixedSizeBinaryArray(ArrayData)` / `(ArrowTypeId, ArrayData)`        | ctor     | binds a packed run with no builder          |
|  [10]   | `FixedSizeBinaryArray.BuilderBase.Append(ReadOnlySpan<byte>)`             | instance | one fixed-width value; `Set`/`Swap` beside  |
|  [11]   | `new ArrowBuffer.Builder<T>(capacity)` / `.Append(T)` / `.Build(alloc)`   | ctor     | offsets and packed runs; `bool` throws      |
|  [12]   | `MapType.KeyValueType` / `.KeyField` / `.ValueField`                      | property | the entries struct the map ctor takes       |
|  [13]   | `Date32Array.Builder.Append(DateOnly)` / `(ReadOnlySpan<DateOnly>)`       | instance | `DateArrayBuilder` converts to the day unit |
|  [14]   | `StringArray.Builder.AppendRange(IEnumerable<string>, Encoding?)`         | instance | whole column append; null writes validity   |

[ENTRYPOINT_SCOPE]: type-system values (`Apache.Arrow.Types`) — a schema field takes an `IArrowType` instance, so the parameterless types expose one shared `Default` and the parameterized take their whole shape at construction

| [INDEX] | [SURFACE]                                          | [SHAPE]    | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------- | :--------- | :---------------------------------------------- |
|  [01]   | `StringType.Default`                               | type value | UTF-8 variable-length binary                    |
|  [02]   | `DoubleType.Default`                               | type value | IEEE 64-bit floating point                      |
|  [03]   | `FloatType.Default`                                | type value | IEEE 32-bit floating point                      |
|  [04]   | `HalfFloatType.Default`                            | type value | IEEE 16-bit floating point                      |
|  [05]   | `Int32Type.Default` / `Int64Type.Default`          | type value | signed 32/64-bit integer                        |
|  [06]   | `UInt8Type.Default`                                | type value | unsigned 8-bit integer                          |
|  [07]   | `UInt32Type.Default` / `UInt64Type.Default`        | type value | unsigned 32/64-bit integer                      |
|  [08]   | `BooleanType.Default`                              | type value | 1-bit boolean                                   |
|  [09]   | `Date32Type.Default`                               | type value | 32-bit day-unit date                            |
|  [10]   | `new TimestampType(TimeUnit, string)`              | ctor       | unit plus timezone; `Default` is millisecond    |
|  [11]   | `new FixedSizeBinaryType(int byteWidth)`           | ctor       | fixed-width binary; a non-positive width throws |
|  [12]   | `new ListType(IArrowType)`                         | ctor       | variable list; wraps an `item` field            |
|  [13]   | `new MapType(IArrowType, IArrowType, bool, bool)`  | ctor       | key + value; builds the entries struct itself   |
|  [14]   | `new DictionaryType(IArrowType, IArrowType, bool)` | ctor       | index must be IntegerType or the ctor throws    |

[ENTRYPOINT_SCOPE]: IPC read and write

| [INDEX] | [SURFACE]                                                   | [SHAPE]      | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :----------- | :--------------------------------------------------- |
|  [01]   | `new ArrowStreamReader(stream, leaveOpen?)`                 | ctor         | opens IPC stream reader (`IArrowReader`)             |
|  [02]   | `new ArrowStreamWriter(stream, schema, leaveOpen, options)` | ctor         | opens IPC stream writer; `MemoryAllocator` overload  |
|  [03]   | `new ArrowFileReader(stream, leaveOpen?)`                   | ctor         | opens IPC file reader (random-access)                |
|  [04]   | `new ArrowFileWriter(stream, schema, leaveOpen, options)`   | ctor         | opens IPC file writer                                |
|  [05]   | `WriteStart()` / `WriteStartAsync()`                        | schema write | emits the schema message before the first batch      |
|  [06]   | `ReadNextRecordBatch()` / `ReadNextRecordBatchAsync()`      | read         | reads the next `RecordBatch`                         |
|  [07]   | `WriteRecordBatch(batch)` / `WriteRecordBatchAsync(batch)`  | write        | writes one `RecordBatch`                             |
|  [08]   | `WriteEnd()` / `WriteEndAsync()`                            | finalize     | writes IPC EOS terminator (mandatory before dispose) |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Zero-copy construction is the second root: `new ArrowBuffer(ReadOnlyMemory<byte>)` borrows a caller's contiguous slice, a primitive array's `(valueBuffer, nullBitmapBuffer, length, nullCount, offset)` constructor binds it at the element width, `ArrowBuffer.Empty` fills the dense array's absent validity bitmap, and `FixedSizeListArray` states an interleave the borrowed bytes already carry — no builder, no allocator, no gather.
- Narrow lanes bind `HalfFloatArray`/`UInt8Array` at their stored width and never widen to `FloatArray`: widening at the wrap re-spells values its producer's tolerance proof certified narrow, and a reader needing floats widens on read.
- Metadata-bearing construction has one root: build typed arrays under the per-lane `MemoryAllocator`, build one `Schema` whose fields match the array order one-for-one and whose `Metadata` carries every operation fact, then call `new RecordBatch(schema, arrays, points)`. `RecordBatch.Builder` exposes neither `Schema` injection nor `Metadata`, so it discards `content_key`/`strategy`/`at`/`points`, and a batch whose metadata omits the content key is the drift defect.
- Each `DoeDataset` column bulk-appends its backing span — `Coordinates`/`Responses` through `DoubleArray.Builder.Append(span)`, `OnFront` through `BooleanArray.Builder.Append(span)`, `Reserve(points)` pre-sizing each allocator-owned buffer — never a scalar `Append(T)` loop.
- `MemoryAllocator` injects through typed-array `Build(allocator)` and `RecordBatch.Builder(allocator)`; `Schema.Builder.Build()` and `RecordBatch.Builder.Build()` take none, so a staging-bounded lane (`Tensor/memory#STREAM_POOL`) passes its allocator to every array build and buffers charge the lane budget.
- `IArrowType.Default` singletons type the scalar columns and `TimestampType.Default` (`TimeUnit.Millisecond`, UTC) types the `Instant At` column, so the Arrow wire carries the same NodaTime clock port operation results use; a bare `DateTime` column is the rejected form.
- `IArrowArrayStream` (`Schema` + `ReadNextRecordBatchAsync`) is the one async-enumerable egress boundary IPC, ADBC, and Flight all yield; the egress owner folds all three behind it and never forks a per-transport reader.
- `RecordBatch` implements `IArrowRecord` and `IArrowArray` and is `IDisposable`; `Slice`/`SliceShared` window a batch with zero buffer copy.
- `IpcOptions.CompressionCodec` (`CompressionCodecType?`, `Lz4Frame` \| `Zstd`) is inert unless `CompressionCodecFactory` is set; the concrete `ICompressionCodecFactory` ships in `Apache.Arrow.Compression`, never core Arrow, invoked per batch for the per-codec `ICompressionCodec`.
- `DictionaryType(indexType, valueType, ordered)` throws `ArgumentException` unless `indexType` is an `IntegerType`, and its `Default` is `[Obsolete]`, so the index width is fixed at the composing owner; `MapType(key, value, …)` builds its own `entries` struct field, so a composing schema hands two logical types and never assembles the key-value struct itself.
- Container assembly holds ONE discipline: build each child array through its element type's own builder, then bind the container constructor. `ListArray.Builder` and `MapArray.Builder` reach their children only through the untyped `IArrowArrayBuilder<IArrowArray, …>` face, so a typed append needs a cast; `DictionaryArray` ships no builder and `FixedSizeBinaryArray` ships only the abstract `BuilderBase<TArray, TBuilder>`, so a fixed-width column packs its own run and binds `ArrayData`.

[STACKING]:
- `NodaTime`(`api-nodatime.md`): an `Instant`/`ZonedDateTime` projects to the `TimestampArray` epoch column under its `TimestampType` at the builder edge — the one clock port the Arrow wire, operation results, and the Persistence store share; never a bare `DateTime`.
- `Arrow egress train`(`Rasm.Persistence/.api/api-arrow-egress.md`): the ADBC, Flight, Flight-hosting, Flight-SQL, and IPC-compression packages are Persistence-local and carry the solution's earned behavioural law for them — read its `[SUBSTRAIT_COMMAND_UNROUTED]` topology entry before writing any plan-carrying or Flight-facing member. Sealed `RecordBatch` values cross to the one lake custodian at `Rasm.Persistence/Query/lakehouse#FLAT_TABLE_EGRESS` `Land`, which owns writers, storage, hive generation, and index custody; the `#FLIGHT_RESULT_PLANE` Flight server is the READ end serving plans back, never a landing door, so a producer dialing Flight to write forks lake custody.
- `Query/lakehouse#FLAT_TABLE_EGRESS`(`Rasm.Persistence/.planning/Query/columnar.md`): `LandingArm` and `LakeGeneration` are the corpus types the landing projection composes — Compute names its arm row and readable segment, and the schema key derives off `Schema.FieldsList` on the two row-major arms so an additive column lands a compatible generation; the geometry arm alone keys off the kernel's own `PackSchema.SchemaId`, since re-digesting the Arrow projection splits the hive tree on a spelling the kernel never published. Every writer stays Persistence-side.
- Compute consumer anchor: `SweepLane.Dataset` folds a `SweepResult` into a content-keyed `DoeDataset` this build projects into one `RecordBatch` the Python graduation companion answers with a graduated ONNX surrogate over `GraduationEvidence`; `ChargebackDataset.Of` folds the identical builder path for billing; `GeometryDataset` wraps the kernel arena's descriptor-tiled slices as copy-free `FixedSizeList` columns — one construction owner, three dataset producers.
- Persistence consumer anchor: the egress owner folds IPC, ADBC, and Flight behind one `IArrowArrayStream` and reads one typed column via `RecordBatch.Column(name)` returning `IArrowArray` — one boundary materialisation, never a `PrimitiveArray<T>` batch accessor; `Query/backend#COLUMN_VOCABULARY`'s `ArrowLanding.Build<TRow>` is the declaration fold, projecting the storage column roster into `Field`/`Schema` values off each `ColumnType` row's own `IArrowType` and building each column through that row's own `Builder`, so a landing binds pre-built columns through the `RecordBatch(Schema, …, length)` ctor rather than re-declaring field order at a builder — the DDL, the batch, and every reader's ordinals derive from one declaration, and the `metadata` parameter is required at every producer arm so the operation facts reach the schema.

[LOCAL_ADMISSION]:
- Compute references core `Apache.Arrow` alone; the `Apache.Arrow.Adbc`, `Apache.Arrow.Flight`, `Apache.Arrow.Flight.AspNetCore`, `Apache.Arrow.Flight.Sql`, and `Apache.Arrow.Compression` egress packages are Persistence's and absent from the Compute closure.
- `WriteStart`/`WriteStartAsync` emits the schema message and `WriteEnd`/`WriteEndAsync` the mandatory EOS terminator; a writer disposed without `WriteEnd` leaves a truncated stream the reader rejects.
