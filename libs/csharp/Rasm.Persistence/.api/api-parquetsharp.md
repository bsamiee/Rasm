# [RASM_PERSISTENCE_API_PARQUETSHARP]

`ParquetSharp` owns the native libparquet-cpp Parquet read/write codec the managed `Apache.Arrow` C# stack lacks, layering three surfaces over one Arrow C++ core: the low-level `ColumnWriter`/`ColumnReader` chunk API and its typed `LogicalColumnWriter<TValue>` mirror, the `RowOriented.ParquetFile` tuple mapper, and the `ParquetSharp.Arrow` `RecordBatch` bridge over the Arrow C Data Interface.

It reads and writes Parquet from a managed `Stream` or Arrow batch with no SQL engine — the direct columnar-file lane distinct from the DuckDB `COPY ... TO` path, with `ParquetSharp.Dataset` layering a Hive-partitioned lake scanner over the same core under `Col`/`IFilter` predicate and column pushdown.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ParquetSharp`
- package: `ParquetSharp` (Apache-2.0)
- assembly: `ParquetSharp`
- namespace: `ParquetSharp`, `ParquetSharp.Schema`, `ParquetSharp.Arrow`, `ParquetSharp.RowOriented`, `ParquetSharp.Encryption`, `ParquetSharp.IO`
- admission: Parquet Modular Encryption ships INSIDE this package as the `ParquetSharp.Encryption` namespace — `CryptoFactory`, `KmsConnectionConfig`, `EncryptionConfiguration`, and `DecryptionConfiguration` all decompile out of `ParquetSharp.dll` — so no separate `PackageVersion` row and no second catalogue exist to mint; a manifest row named `ParquetSharp.Encryption` is a phantom package.
- target: multi-target (`net8.0`, `netstandard2.1`, `net471`); the `net10.0` consumer binds `lib/net8.0`
- native: `runtimes/<rid>/native/ParquetSharpNative.dylib` (`osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64`, `win-x64`, `win-arm64`) — the wrapped Apache Arrow/Parquet C++ core, P/Invoke-loaded at `ParquetFileWriter`/`ParquetFileReader` handle construction, RID-resolved at load
- rail: columnar-file-codec

[PACKAGE_SURFACE]: `ParquetSharp.Dataset`
- package: `ParquetSharp.Dataset` (Apache-2.0)
- assembly: `ParquetSharp.Dataset`
- namespace: `ParquetSharp.Dataset`, `ParquetSharp.Dataset.Filter`, `ParquetSharp.Dataset.Partitioning`
- target: `net6.0`; the `net10.0` consumer binds `lib/net6.0`
- native: none — pure-managed over the `ParquetSharp` native core and `Apache.Arrow`
- rail: columnar-file-codec (partitioned lake scan)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file reader/writer roots

`ParquetFileWriter` ctors fan three sink shapes (`string`, `OutputStream`, managed `Stream` with `leaveOpen`) across two schema shapes (`Column[]` or a `GroupNode` tree) and two property shapes (bare `Compression` or a full `WriterProperties`), with an optional `LogicalTypeFactory` and `IReadOnlyDictionary<string,string>` key-value metadata; `ParquetFileReader` takes `string`/`RandomAccessFile`/`Stream` with an optional `ReaderProperties`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                |
| :-----: | :--------------------- | :----------------- | :------------------------------------------ |
|  [01]   | `ParquetFileWriter`    | writer root        | owns native writer handle, row groups       |
|  [02]   | `ParquetFileReader`    | reader root        | owns native reader handle, file metadata    |
|  [03]   | `RowGroupWriter`       | row-group writer   | `Column(i)`/`NextColumn()`, byte counters   |
|  [04]   | `RowGroupReader`       | row-group reader   | `Column(i)`, `RowGroupMetaData`             |
|  [05]   | `ColumnWriter`         | column writer      | low-level physical column write             |
|  [06]   | `ColumnReader`         | column reader      | low-level physical column read              |
|  [07]   | `Column` / `Column<T>` | schema column      | maps a CLR type to a Parquet schema node    |
|  [08]   | `FileMetaData`         | file metadata      | row counts, schema, key-value metadata      |
|  [09]   | `RowGroupMetaData`     | row-group metadata | per-group row/byte/column statistics        |
|  [10]   | `ColumnChunkMetaData`  | chunk metadata     | per-chunk encoding, compression, statistics |

[PUBLIC_TYPE_SCOPE]: typed logical column family

`LogicalColumnWriter<TElement>`/`LogicalColumnReader<TElement>` are the typed batch mirror over the physical `ColumnWriter`/`ColumnReader`, resolving repetition/definition levels for nullables and nesting.

For a runtime-only element type, `ColumnDescriptor.Apply<TReturn>(LogicalTypeFactory, IColumnDescriptorVisitor<TReturn>)` dispatches into `OnColumnDescriptor<TPhysical,TLogical,TElement>()` by reflection-free generic dispatch; `LogicalWriterOverride<TElement>()`/`LogicalReaderOverride<TElement>()` override the schema-inferred element type, and `LogicalReader(useNesting: true)` opts an untyped reader into `Nested<T>` reassembly.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :------------------------------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `LogicalColumnWriter<TValue>`          | typed writer       | `WriteBatch(ReadOnlySpan<TValue>)`                               |
|  [02]   | `LogicalColumnReader<TValue>`          | typed reader       | `ReadBatch(Span<TValue>)`, `ReadAll`, `GetEnumerator`            |
|  [03]   | `LogicalColumnStream`                  | stream base        | shared buffered batch state                                      |
|  [04]   | `ILogicalColumnWriterVisitor<TReturn>` | writer visitor     | runtime-typed logical-writer dispatch                            |
|  [05]   | `ILogicalColumnReaderVisitor<TReturn>` | reader visitor     | runtime-typed logical-reader dispatch                            |
|  [06]   | `IColumnDescriptorVisitor<TReturn>`    | descriptor visitor | `OnColumnDescriptor<TPhysical,TLogical,TElement>()` continuation |
|  [07]   | `LogicalTypeFactory`                   | type factory       | maps CLR types to logical Parquet types                          |
|  [08]   | `LogicalReadConverterFactory`          | read converter     | custom physical→CLR conversion                                   |
|  [09]   | `LogicalWriteConverterFactory`         | write converter    | custom CLR→physical conversion                                   |
|  [10]   | `Nested<T>`                            | nesting wrapper    | public struct-nesting wrapper for repeated/struct schemas        |

[PUBLIC_TYPE_SCOPE]: schema and physical value family

`Schema.GroupNode`/`Schema.PrimitiveNode` build the column tree; `LogicalType` is the annotated-type base reached through its `.Decimal(...)`/`.Timestamp(...)`/`.String()` static factories. Physical value structs are the wire-level representations converters target — a Parquet `Decimal` cell materializes through `FixedLenByteArray`. `Statistics<TValue>` exposes the per-column typed min/max/null-count the page-index pushdown reads.

[LOGICALTYPE_SUBTYPES]: `StringLogicalType` `DecimalLogicalType` `DateLogicalType` `TimestampLogicalType` `TimeLogicalType` `IntLogicalType` `JsonLogicalType` `BsonLogicalType` `UuidLogicalType` `Float16LogicalType` `ListLogicalType` `MapLogicalType` `EnumLogicalType` `IntervalLogicalType` `NullLogicalType` `NoneLogicalType`

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]     | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `Schema.GroupNode`                                   | schema node       | struct/group schema node                               |
|  [02]   | `Schema.PrimitiveNode`                               | schema node       | leaf primitive schema node                             |
|  [03]   | `Schema.Node`                                        | schema node base  | shared node identity/repetition                        |
|  [04]   | `LogicalType`                                        | logical type      | base; `Decimal`/`Timestamp`/`String` factory subtypes  |
|  [05]   | `SchemaDescriptor`                                   | schema descriptor | flattened leaf-column descriptor                       |
|  [06]   | `ColumnDescriptor`                                   | column descriptor | one leaf column's type/levels                          |
|  [07]   | `Schema.ColumnPath`                                  | column path       | the dotted leaf address every per-column knob targets  |
|  [08]   | `Statistics` / `Statistics<TValue>`                  | column statistics | typed min/max/null-count per column chunk              |
|  [09]   | `ByteArray` / `FixedLenByteArray`                    | physical value    | variable/fixed binary cell (decimal materializes here) |
|  [10]   | `Int96` / `Date` / `DateTimeNanos` / `TimeSpanNanos` | physical value    | legacy/temporal physical cells                         |

- `ColumnPath` ctors take a `string[]` dot-vector, a dotted `string`, or a `Schema.Node`, and `Extend` appends one name, so a nested leaf addresses structurally rather than through a hand-joined name a dot inside a field name silently splits.

[PUBLIC_TYPE_SCOPE]: enum and policy family

[COMPRESSION_CODECS]: `Uncompressed` `Snappy` `Gzip` `Brotli` `Zstd` `Lz4` `Lz4Frame` `Lzo` `Bz2` `Lz4Hadoop`

[ENCODINGS]: `Plain` `PlainDictionary` `Rle` `DeltaBinaryPacked` `DeltaLengthByteArray` `DeltaByteArray` `RleDictionary` `ByteStreamSplit`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                   |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Compression`               | codec enum      | per-column compression codec                                   |
|  [02]   | `Encoding`                  | encoding enum   | per-column physical encoding                                   |
|  [03]   | `ParquetVersion`            | format enum     | physical format version selector                               |
|  [04]   | `ParquetDataPageVersion`    | page enum       | `V1`/`V2` data-page layout                                     |
|  [05]   | `LogicalTypeEnum`           | logical enum    | logical type discriminant                                      |
|  [06]   | `PhysicalType`              | physical enum   | `Boolean`/`Int32`/`Int64`/`Float`/`Double`/`ByteArray`/`Fixed` |
|  [07]   | `Repetition`                | level enum      | `Required`/`Optional`/`Repeated`                               |
|  [08]   | `SortOrder` / `ColumnOrder` | order enum      | column sort-order metadata                                     |
|  [09]   | `ParquetCipher`             | crypto enum     | `AesGcmV1`/`AesGcmCtrV1` PME cipher                            |
|  [10]   | `SizeStatisticsLevel`       | statistics enum | none/chunk/page size-statistics level                          |

[PUBLIC_TYPE_SCOPE]: writer, reader, and Arrow-bridge property family

Every reader and writer ctor takes its policy as a constructed object, and the two sides mint oppositely: the write side builds an immutable `WriterProperties` through a builder, the read side mutates a live `ReaderProperties` in place. `ParquetSharp.Arrow` layers a second policy per direction over the same handles.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `WriterPropertiesBuilder`                   | writer builder      | the whole write-side tuning surface, `Build()` materializing it  |
|  [02]   | `WriterProperties`                          | writer policy       | resolved immutable policy, per-column readback by `ColumnPath`   |
|  [03]   | `WriterProperties.SortingColumn`            | sort row            | `ColumnIndex`, `IsDescending`, `NullsFirst`                      |
|  [04]   | `DefaultWriterProperties`                   | static defaults     | nullable process-wide overrides seeding every builder            |
|  [05]   | `ReaderProperties`                          | reader policy       | buffering, checksums, thrift bounds, footer read size, PME mount |
|  [06]   | `ArrowReaderProperties`                     | arrow read policy   | batch grain, threading, range coalescing, Arrow type steering    |
|  [07]   | `ArrowWriterPropertiesBuilder`              | arrow write builder | timestamp coercion, stored schema, nesting, engine selection     |
|  [08]   | `ArrowWriterProperties`                     | arrow write policy  | the resolved read-only Arrow write policy                        |
|  [09]   | `ArrowWriterProperties.WriterEngineVersion` | engine enum         | `V1` / `V2` Arrow write path the builder selects                 |
|  [10]   | `CacheOptions`                              | range-read policy   | `hole_size_limit` `range_size_limit` `lazy` `prefetch_limit`     |
|  [11]   | `MemoryPool`                                | allocator handle    | named Arrow allocator with live allocation counters              |

- `DefaultWriterProperties` carries mutable STATIC nullable fields the `WriterPropertiesBuilder` ctor applies through its own `ApplyDefaults`, so it is ambient policy no per-file builder can scope: a set field re-tunes every writer later minted in that load context, including one another composition owns.
- `ReaderProperties` exposes no builder and no immutable twin, so a read policy is a live object each reader ctor captures by reference: one instance reused across readers propagates every later mutation, and `WithMemoryPool` is the only mint that differs from the default.
- `ArrowWriterProperties` exposes getters alone, so `ArrowWriterPropertiesBuilder` is its one mint — the same builder/immutable split the core write side carries, while `ArrowReaderProperties` mutates in place like `ReaderProperties`.
- `CacheOptions` is a mutable struct behind a property, so it assigns whole; a field write through the property getter does not compile and a per-field tune reads back, mutates the local, and assigns the whole struct.

[PUBLIC_TYPE_SCOPE]: Parquet Modular Encryption family (`ParquetSharp.Encryption` beside the root property types)

Two legs derive one pair of property types over the same native core: `CryptoFactory` resolves them from a KMS-wrapped key hierarchy, and `FileEncryptionPropertiesBuilder`/`FileDecryptionPropertiesBuilder` build them from explicit keys the caller already holds, taking per-column rows from `ColumnEncryptionPropertiesBuilder`/`ColumnDecryptionPropertiesBuilder`.

`WriterPropertiesBuilder.Encryption` mounts the write leg and `ReaderProperties.FileDecryptionProperties` the read leg, so both derivations terminate at the two property types the writer and reader ctors already take. Every type below holds a native handle and disposes, and `CryptoFactory` alone must outlive what it derives.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [CAPABILITY]                                                    |
| :-----: | :------------------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `CryptoFactory`                  | KMS crypto factory | derives both property sets from a KMS-wrapped key hierarchy     |
|  [02]   | `CryptoFactory.KmsClientFactory` | client delegate    | `IKmsClient(ReadonlyKmsConnectionConfig)` — the one client mint |
|  [03]   | `IKmsClient`                     | KMS contract       | `WrapKey(byte[], string)` / `UnwrapKey(string, string)`         |
|  [04]   | `KmsConnectionConfig`            | KMS coordinates    | instance id, url, access token, custom conf; token refresh      |
|  [05]   | `ReadonlyKmsConnectionConfig`    | KMS coordinates    | the immutable view handed to the client delegate                |
|  [06]   | `EncryptionConfiguration`        | write-side policy  | footer key, column key map, cipher, wrapping, cache window      |
|  [07]   | `DecryptionConfiguration`        | read-side policy   | `CacheLifetimeSeconds` — the whole read-side surface            |
|  [08]   | `FileEncryptionProperties`       | write properties   | derived footer key, key metadata, file AAD, column lookup       |
|  [09]   | `FileDecryptionProperties`       | read properties    | derived footer key, AAD prefix, retriever and verifier seats    |
|  [10]   | `ColumnEncryptionProperties`     | column properties  | one column's key, metadata, encrypted-with-footer-key flag      |
|  [11]   | `ColumnDecryptionProperties`     | column properties  | one column's key by column path                                 |
|  [12]   | `DecryptionKeyRetriever`         | retriever seat     | `GetKey(string keyMetadata) -> byte[]` on the read leg          |
|  [13]   | `AadPrefixVerifier`              | verifier seat      | `Verify(string aadPrefix)` at footer read                       |

[PUBLIC_TYPE_SCOPE]: dataset scan family (`ParquetSharp.Dataset`)

`DatasetReader.ToBatches`/`ToTable` emit `Apache.Arrow` output; the filter DSL roots at `Col.Named(x)`, whose `ColExtensions` comparands cover `long`/`string`/`DateOnly`/`DateTime` with `IsInRange`/`IsIn`, folding through `And`/`Or` into an `IFilter` the scan pushes down to partition, row-group statistics, and row grain.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                                    |
| :-----: | :--------------------- | :----------------- | :------------------------------------------------------------------------------ |
|  [01]   | `DatasetReader`        | scan root          | `sealed`; `ToBatches` → `IArrowArrayStream`, `ToTable` → `Table`, `Schema` prop |
|  [02]   | `DatasetOptions`       | scan policy        | `Default`; `IgnorePrefixes` init skips `.`/`_` hidden files                     |
|  [03]   | `PartitionInformation` | partition values   | `sealed`; `Batch` `RecordBatch` of partition field values, `Empty`              |
|  [04]   | `Col`                  | filter column      | `sealed`; `Col.Named(name)` roots the predicate DSL                             |
|  [05]   | `ColExtensions`        | filter DSL         | typed `IsEqualTo`/`IsGreaterThan`/`IsInRange`/`IsIn` → `IFilter`                |
|  [06]   | `FilterExtensions`     | filter combinators | `And`/`Or` fold two `IFilter`s                                                  |
|  [07]   | `IFilter`              | filter contract    | partition + row-group + row predicate pushed into the scan                      |

[PUBLIC_TYPE_SCOPE]: partitioning family (`ParquetSharp.Dataset.Partitioning`)

`DatasetReader` ctors take either a concrete `IPartitioning` or an `IPartitioningFactory` inferring one from the directory tree; each scheme carries a nested `Factory : IPartitioningFactory`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                      |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `IPartitioning`        | scheme contract | `Schema`, `Parse`, `SortDirectories` over a directory layout                      |
|  [02]   | `IPartitioningFactory` | scheme factory  | infers an `IPartitioning` from the directory tree                                 |
|  [03]   | `HivePartitioning`     | hive scheme     | `sealed : IPartitioning`; `key=value` dirs; ctor takes `Schema`; nested `Factory` |
|  [04]   | `NoPartitioning`       | flat scheme     | `sealed : IPartitioning`; single-directory scan; nested `Factory`                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: low-level column-chunk write/read

`AppendRowGroup()` writes a fully-buffered row group; `AppendBufferedRowGroup()` streams columns of unequal length. `LogicalColumnWriter<TValue>.WriteBatch` accepts a `TElement[]`, an array slice `(values, start, length)`, or a `ReadOnlySpan<TElement>`; the reader's `ReadBatch(Span<TElement>)`/`ReadAll(rows)`/`GetEnumerator()` mirror it.

| [INDEX] | [SURFACE]                                                     | [SHAPE]     | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------ | :---------- | :--------------------------------------------- |
|  [01]   | `new ParquetFileWriter(stream, columns, props, …, leaveOpen)` | ctor        | opens a Parquet writer over a managed stream   |
|  [02]   | `ParquetFileWriter.AppendRowGroup()`                          | writer call | opens a buffered row group                     |
|  [03]   | `ParquetFileWriter.AppendBufferedRowGroup()`                  | writer call | opens an unequal-length streaming group        |
|  [04]   | `RowGroupWriter.NextColumn()` / `.Column(i)`                  | group call  | advances to / selects a column writer          |
|  [05]   | `ColumnWriter.LogicalWriter<TValue>()`                        | column call | yields the typed `LogicalColumnWriter<TValue>` |
|  [06]   | `LogicalColumnWriter<TValue>.WriteBatch(span)`                | typed write | writes a typed value batch                     |
|  [07]   | `new ParquetFileReader(stream, props, leaveOpen)`             | ctor        | opens a Parquet reader over a stream           |
|  [08]   | `ParquetFileReader.RowGroup(i)`                               | reader call | selects a `RowGroupReader`                     |
|  [09]   | `ColumnReader.LogicalReader<TValue>()`                        | column call | yields the typed `LogicalColumnReader<TValue>` |
|  [10]   | `LogicalColumnReader<TValue>.ReadBatch(span)` / `.Skip(n)`    | typed read  | reads / skips a typed value batch              |
|  [11]   | `ParquetFileWriter.Close()` / `Dispose()`                     | finalize    | flushes footer and closes the file             |

[ENTRYPOINT_SCOPE]: row-oriented POCO/tuple mapping — `RowOriented.ParquetFile`

`CreateRowWriter<TTuple>`/`CreateRowReader<TTuple>` map a `TTuple` (a `ValueTuple`, or a POCO whose columns bind by `[MapToColumn]`) to and from the column layout, so a fact record round-trips without manual column indexing; both fan the same sink/source shapes as the low-level writer (path/`OutputStream`, `Compression` or `WriterProperties`, `string[] columnNames` or `Column[]`, key-value metadata, optional `LogicalTypeFactory`/converter factories).

| [INDEX] | [SURFACE]                                                                         | [SHAPE]        | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `ParquetFile.CreateRowWriter<TTuple>(path, columnNames, compression, kvMetadata)` | static factory | opens a typed row writer           |
|  [02]   | `ParquetFile.CreateRowWriter<TTuple>(outputStream, writerProperties, columns, …)` | static factory | opens a tuned typed row writer     |
|  [03]   | `ParquetRowWriter<TTuple>.WriteRow(row)`                                          | row write      | writes one mapped record           |
|  [04]   | `ParquetRowWriter<TTuple>.WriteRows(IEnumerable<TTuple>)` / `.WriteRowSpan(span)` | row write      | bulk-writes a record sequence/span |
|  [05]   | `ParquetRowWriter<TTuple>.StartNewRowGroup()`                                     | row write      | begins a new row group             |
|  [06]   | `ParquetFile.CreateRowReader<TTuple>(path, …)`                                    | static factory | opens a typed row reader           |
|  [07]   | `ParquetRowReader<TTuple>.ReadRows(rowGroup)`                                     | row read       | reads a row group as `TTuple[]`    |

[ENTRYPOINT_SCOPE]: Arrow C-Data bridge — `ParquetSharp.Arrow`

`Arrow.FileWriter` writes `Apache.Arrow` `RecordBatch`/`Table` (and `ChunkedArray`/`IArrowArray` column chunks) straight to Parquet; `Arrow.FileReader.GetRecordBatchReader(rowGroups?, columns?)` returns an `IArrowArrayStream` streaming selected row groups and columns back as Arrow batches over the C Data Interface — the zero-managed-copy path, Parquet bytes ↔ `RecordBatch` with no per-cell CLR boxing.

| [INDEX] | [SURFACE]                                                            | [SHAPE]     | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :---------- | :------------------------------------------- |
|  [01]   | `new Arrow.FileWriter(stream, schema, props, arrowProps, leaveOpen)` | ctor        | opens an Arrow-schema Parquet writer         |
|  [02]   | `Arrow.FileWriter.WriteRecordBatch(recordBatch, chunkSize)`          | arrow write | writes an `Apache.Arrow` record batch        |
|  [03]   | `Arrow.FileWriter.WriteTable(table, chunkSize)`                      | arrow write | writes an `Apache.Arrow` table               |
|  [04]   | `Arrow.FileWriter.WriteBufferedRecordBatch(batch)`                   | arrow write | buffered unequal-length batch write          |
|  [05]   | `Arrow.FileWriter.NewBufferedRowGroup()`                             | arrow write | opens a new buffered row group               |
|  [06]   | `Arrow.FileWriter.WriteColumnChunk(IArrowArray \| ChunkedArray)`     | arrow write | writes one Arrow column chunk                |
|  [07]   | `new Arrow.FileReader(stream, props, arrowProps, leaveOpen)`         | ctor        | opens an Arrow-projecting Parquet reader     |
|  [08]   | `Arrow.FileReader.GetRecordBatchReader(rowGroups, columns)`          | arrow read  | streams `IArrowArrayStream` of Arrow batches |
|  [09]   | `Arrow.FileReader.ParquetReader`                                     | accessor    | drops to the low-level reader                |
|  [10]   | `Arrow.FileReader.SchemaManifest`                                    | accessor    | the Arrow schema map                         |

[ENTRYPOINT_SCOPE]: writer tuning — `WriterPropertiesBuilder`

`WriterPropertiesBuilder` is the full tuning surface; every `[SURFACE]` below is a `.` builder call, and every column-targeting method carries a global, `string path`, and `ColumnPath` overload (the `path?` slot).

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `.Compression(path?, codec)` / `.CompressionLevel(path?, n)`                | builder | per-column codec and level                 |
|  [02]   | `.EnableDictionary(path?)` / `.DisableDictionary(path?)`                    | builder | per-column dictionary encoding             |
|  [03]   | `.Encoding(path?, encoding)`                                                | builder | per-column physical encoding               |
|  [04]   | `.EnableWritePageIndex(path?)` / `.DisableWritePageIndex(path?)`            | builder | per-column column and offset index         |
|  [05]   | `.EnablePageChecksum()` / `.DisablePageChecksum()`                          | builder | CRC page checksums                         |
|  [06]   | `.SortingColumns(WriterProperties.SortingColumn[])`                         | builder | declares sorted-column metadata            |
|  [07]   | `.EnableStatistics(path?)` / `.DisableStatistics(path?)`                    | builder | per-column statistics policy               |
|  [08]   | `.SetMaxStatisticsSize(n)` / `.SetSizeStatisticsLevel(SizeStatisticsLevel)` | builder | statistics byte cap and size-stats grain   |
|  [09]   | `.MaxRowGroupLength(n)` / `.DataPagesize(n)` / `.WriteBatchSize(n)`         | builder | row-group / page / batch sizing            |
|  [10]   | `.DictionaryPagesizeLimit(n)`                                               | builder | dictionary-page byte ceiling               |
|  [11]   | `.Version(ParquetVersion)` / `.DataPageVersion(ParquetDataPageVersion)`     | builder | format and data-page version               |
|  [12]   | `.EnableStoreDecimalAsInteger()` / `.DisableStoreDecimalAsInteger()`        | builder | decimal physical storage                   |
|  [13]   | `.CreatedBy(s)` / `.MemoryPool(MemoryPool)`                                 | builder | writer signature and allocator             |
|  [14]   | `.Encryption(FileEncryptionProperties?)` / `.Build()`                       | builder | binds PME, materializes `WriterProperties` |

- `SetSizeStatisticsLevel(PageAndColumnChunk)` writes NO page-level size statistics while the page index stays disabled, degrading silently to column-chunk grain — the level and `EnableWritePageIndex` arm together or the finer level buys nothing.
- Every knob here is builder-global except the `path?` slot's own overloads, so `DictionaryPagesizeLimit`, the two page-version rows, `SetMaxStatisticsSize`, and `SetSizeStatisticsLevel` apply file-wide with no per-column form.

[ENTRYPOINT_SCOPE]: read tuning — `ReaderProperties`, `ArrowReaderProperties`, and `ArrowWriterPropertiesBuilder`

`ReaderProperties` mints from a static factory and mutates in place; `ArrowReaderProperties` does the same for the Arrow decode side, and `ArrowWriterPropertiesBuilder` mints the Arrow encode policy. Each bare `.` row continues the receiver its scope block names, and all three objects pass into the reader and writer ctors above.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `ReaderProperties.GetDefaultReaderProperties()`                            | factory  | the read policy every reader ctor takes    |
|  [02]   | `ReaderProperties.WithMemoryPool(MemoryPool)`                              | factory  | the same policy over a named allocator     |
|  [03]   | `.EnableBufferedStream()` / `.DisableBufferedStream()` / `.BufferSize`     | instance | stream buffering mode and its window       |
|  [04]   | `.EnablePageChecksumVerification()` / `.DisablePageChecksumVerification()` | instance | verify written page CRCs on read           |
|  [05]   | `.SetThriftStringSizeLimit(int)` / `.SetThriftContainerSizeLimit(int)`     | instance | footer-parse ceilings on a hostile file    |
|  [06]   | `.SetFooterReadSize(long)`                                                 | instance | bytes the first footer read pulls          |
|  [07]   | `.FileDecryptionProperties`                                                | property | mounts PME read properties onto the reader |
|  [08]   | `.MemoryPool`                                                              | property | the allocator this policy resolves         |
|  [09]   | `ArrowReaderProperties.GetDefault()`                                       | factory  | the Arrow decode policy                    |
|  [10]   | `.BatchSize` / `.UseThreads`                                               | property | rows per `RecordBatch`, parallel decode    |
|  [11]   | `.PreBuffer` / `.CacheOptions`                                             | property | range coalescing for a remote read         |
|  [12]   | `.BinaryType` / `.ListType`                                                | property | Arrow binary and list type on decode       |
|  [13]   | `.CoerceInt96TimestampUnit` / `.ArrowExtensionEnabled`                     | property | INT96 unit and Arrow extension admission   |
|  [14]   | `.GetReadDictionary(int)` / `.SetReadDictionary(int, bool)`                | instance | per-column dictionary-array decode         |
|  [15]   | `new ArrowWriterPropertiesBuilder()` / `.Build()`                          | ctor     | the Arrow encode policy                    |
|  [16]   | `.CoerceTimestamps(TimeUnit)` / `.AllowTruncatedTimestamps()`              | builder  | timestamp unit and its truncation stance   |
|  [17]   | `.DisallowTruncatedTimestamps()` / `.StoreSchema()`                        | builder  | refuse lossy coercion, embed the schema    |
|  [18]   | `.EnableCompliantNestedTypes()` / `.DisableCompliantNestedTypes()`         | builder  | spec-compliant list and map element naming |
|  [19]   | `.EngineVersion(WriterEngineVersion)` / `.UseThreads(bool)`                | builder  | write engine and parallel column encode    |

- `MemoryPool` mints from four static seats — `GetDefaultMemoryPool`, `SystemMemoryPool`, `JemallocMemoryPool`, `MimallocMemoryPool` — and reports `BytesAllocated`, `MaxMemory`, and `BackendName`, so allocator choice and its live counters ride one handle across `ReaderProperties.WithMemoryPool` and `WriterPropertiesBuilder.MemoryPool`.

- `PreBuffer` defaults ON and exists for high-latency filesystems, so an object-store read already coalesces; `CacheOptions` tunes that coalescing to the filesystem — `hole_size_limit` bounds the gap two ranges merge across, `range_size_limit` caps a merged range, and `lazy` defers each fetch to first touch instead of issuing at open.
- `SetThriftStringSizeLimit` and `SetThriftContainerSizeLimit` bound footer deserialization, so a corrupt or hostile footer refuses at a declared ceiling rather than allocating against a length the file itself declares — the two knobs an untrusted-Parquet ingress sets before any other.
- `StoreSchema` writes the binary Arrow schema into Parquet key-value metadata, and a reader finding it IGNORES `BinaryType` and `ListType` whole: the embedded schema wins, so a write-side `StoreSchema` silently disarms the read-side type steering and the two sides pick one owner of Arrow typing.
- `EnableCompliantNestedTypes` defaults ON and renames a list's values field from Arrow's `item` to the spec's `element`, so an Arrow list round-trips under a changed field name unless the write disables it or `StoreSchema` carries the original naming.
- `CoerceTimestamps` casts nanoseconds to microseconds under Parquet `V1_0` and `V2_4` whatever unit the call names, and truncation loss FAULTS by default — `AllowTruncatedTimestamps` is the opt-in that turns that refusal into a silent narrowing, `DisallowTruncatedTimestamps` restoring it.

[ENTRYPOINT_SCOPE]: KMS-managed PME derivation — `ParquetSharp.Encryption`

`CryptoFactory` wraps each data-encryption key with a master key the customer `IKmsClient` holds, so key material never leaves the KMS and the file carries wrapped keys alone. `EncryptionConfiguration` is the whole write-side policy and `DecryptionConfiguration` the whole read-side one; a bare `.` row below is a `CryptoFactory` call.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new CryptoFactory(kmsClientFactory)`                                       | ctor     | binds the customer `IKmsClient` mint         |
|  [02]   | `.GetFileEncryptionProperties(kmsConfig, encConfig, filePath?)`             | crypto   | derives KMS-wrapped write properties         |
|  [03]   | `.GetFileDecryptionProperties(kmsConfig, decConfig, filePath?)`             | crypto   | derives KMS-wrapped read properties          |
|  [04]   | `.RotateMasterKeys(kmsConfig, path, doubleWrapping, cacheSeconds)`          | crypto   | re-wraps an existing file's data keys        |
|  [05]   | `new KmsConnectionConfig()` / `.RefreshKeyAccessToken(token)`               | ctor     | mints coordinates, swaps the token in place  |
|  [06]   | `KmsConnectionConfig.KmsInstanceId` / `.KmsInstanceUrl` / `.KeyAccessToken` | property | KMS instance identity and access token       |
|  [07]   | `KmsConnectionConfig.CustomKmsConf`                                         | property | extra provider-specific KMS settings         |
|  [08]   | `new EncryptionConfiguration(footerKey)`                                    | ctor     | seats the footer master-key identifier       |
|  [09]   | `EncryptionConfiguration.ColumnKeys` / `.UniformEncryption`                 | property | per-column key map, or one key for all       |
|  [10]   | `EncryptionConfiguration.EncryptionAlgorithm` / `.PlaintextFooter`          | property | `ParquetCipher` and footer encryption stance |
|  [11]   | `EncryptionConfiguration.DoubleWrapping` / `.CacheLifetimeSeconds`          | property | KEK double-wrap and key-cache window         |
|  [12]   | `EncryptionConfiguration.InternalKeyMaterial` / `.DataKeyLengthBits`        | property | key-material residence and DEK bit width     |
|  [13]   | `new DecryptionConfiguration()` / `.CacheLifetimeSeconds`                   | ctor     | read-side key-cache window                   |

- `CryptoFactory` OUTLIVES every reader armed from its `GetFileDecryptionProperties` result: the derived properties hold native references into the factory that the package cannot manage, so disposing it while a generation still streams faults in native memory where no managed catch reaches. Custody is a composition-held value, never a per-call `using`.
- `EncryptionConfiguration.ColumnKeys` is `IReadOnlyDictionary<string, IReadOnlyList<string>>` marshalling through one joined string of `masterKeyId:col,col` groups separated by `;`, so a master-key id or column path carrying `:`, `,`, or `;` corrupts the grouping silently rather than faulting at assignment.
- `InternalKeyMaterial` decides where wrapped keys rest: true stores them in the Parquet footer and false writes a sidecar key file beside the data, which is why `filePath` is optional on both derivations and required the moment key material goes external.
- `CacheLifetimeSeconds` is a `double` on both configurations and `RotateMasterKeys` defaults its own to 600, so a rotation run inherits a ten-minute KEK and client cache unless the call names its own window.
- `KmsClientFactory` and both `IKmsClient` verbs run behind a marshalling shim catching every escape into `ex.ToString()` beside an empty wrapped key from a throwing `WrapKey`, so a KMS outage crosses the boundary as a flattened message carrying no exception type and no stack — the adapter states its own typed diagnosis or a credential fault reads as a crypto fault.

[ENTRYPOINT_SCOPE]: explicit-key PME derivation — the root properties builders

Callers already holding raw AES keys build the same `FileEncryptionProperties`/`FileDecryptionProperties` `CryptoFactory` derives, so the two legs are alternatives at one seam rather than two encryption models. Each bare `.` row continues the builder the `new` row above it opened.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new FileEncryptionPropertiesBuilder(footerKey)`                     | ctor     | explicit-key write build over a footer key  |
|  [02]   | `.Algorithm(ParquetCipher)` / `.SetPlaintextFooter()`                | builder  | cipher and footer encryption stance         |
|  [03]   | `.FooterKeyId(id)` / `.FooterKeyMetadata(metadata)`                  | builder  | footer key identity carried in the file     |
|  [04]   | `.AadPrefix(prefix)` / `.DisableAadPrefixStorage()`                  | builder  | AAD prefix and whether the file stores it   |
|  [05]   | `.EncryptedColumns(ColumnEncryptionProperties[])` / `.Build()`       | builder  | binds column rows, materializes properties  |
|  [06]   | `new FileDecryptionPropertiesBuilder()` / `.FooterKey(bytes)`        | ctor     | explicit-key read build over a footer key   |
|  [07]   | `.ColumnKeys(ColumnDecryptionProperties[])` / `.Build()`             | builder  | per-column keys, materializes properties    |
|  [08]   | `.KeyRetriever(DecryptionKeyRetriever)`                              | builder  | resolves a column key from its key metadata |
|  [09]   | `.AadPrefixVerifier(AadPrefixVerifier)` / `.AadPrefix(prefix)`       | builder  | AAD prefix and the verifier that checks it  |
|  [10]   | `.DisableFooterSignatureVerification()` / `.PlaintextFilesAllowed()` | builder  | signature and mixed-plaintext read stance   |
|  [11]   | `new ColumnEncryptionPropertiesBuilder(columnName)`                  | ctor     | one encrypted column                        |
|  [12]   | `.Key(bytes)` / `.KeyId(id)` / `.KeyMetadata(metadata)` / `.Build()` | builder  | its key, key id, key metadata, build        |
|  [13]   | `new ColumnDecryptionPropertiesBuilder(columnName)` / `.Key(bytes)`  | ctor     | one decrypted column and its key            |
|  [14]   | `ReaderProperties.FileDecryptionProperties`                          | property | mounts read properties onto reader ctors    |

- Both column builders overload their ctor on `string` column name and `ColumnPath`, so a nested leaf targets by path rather than by a hand-joined dotted name.
- `DecryptionKeyRetriever.GetKey(keyMetadata)` and `AadPrefixVerifier.Verify(aadPrefix)` are abstract seats a composition subclasses, and the native core holds the GC handle on each instance — reverse of every other handle here — so the seat must outlive the reader that mounted it.
- Both seats run behind a marshalling shim that catches every escape and hands the native side `ex.ToString()` alone, so exception type and stack are lost at the boundary and a retriever throw yields a zero key beside that message; the seat carries its own typed diagnosis or the failure reads as a corrupt-key decrypt.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- three layers terminate at one `ParquetSharpNative.dylib` handle: low-level (`ParquetFileWriter`→`RowGroupWriter`→`ColumnWriter`→`LogicalColumnWriter<TValue>`), row-oriented (`RowOriented.ParquetFile`→`ParquetRowWriter<TTuple>`), and Arrow (`Arrow.FileWriter`/`FileReader` ↔ `Apache.Arrow`).
- `ParquetSharpNative` wraps the Apache Arrow/Parquet C++ build; codec selection is C++-side, never a managed re-implementation.
- `WriterProperties` is immutable, built by `WriterPropertiesBuilder`; the bare-`Compression` writer ctors are sugar over a default builder.
- Four policies govern one file — core write, core read, Arrow encode, Arrow decode — and each reader and writer ctor takes its pair, so tuning is constructed state a composition owns rather than a call-site argument.
- Read policy carries no builder and no immutable form, so `ReaderProperties` and `ArrowReaderProperties` are live objects a ctor captures; a policy shared across readers propagates every later mutation and one per reader is the isolating shape.
- `Column[]` ctors derive a `GroupNode` schema automatically; the `GroupNode` ctors take a hand-built tree for nested/repeated columns the flat `Column[]` shape cannot express.
- handles are `IDisposable` and own native memory; `Close()` flushes the footer, and a reader/writer over a managed `Stream` honors `leaveOpen`.
- `Arrow.FileReader.GetRecordBatchReader` returns `Apache.Arrow.Ipc.IArrowArrayStream` and `Arrow.FileWriter.WriteRecordBatch`/`WriteTable` consume `RecordBatch`/`Table`; `api-arrow` owns the in-memory Arrow model, and the two compose at the `RecordBatch`/`Schema` boundary — this file codec never re-declares that model.

[STACKING]:
- `api-arrow`(`.api/api-arrow.md`): Parquet ↔ `RecordBatch` is the load-bearing stack — the analytical lane reads a Parquet file as an `IArrowArrayStream`, feeds it to a DuckDB query or an ADBC consumer (`Apache.Arrow.Adbc`), and writes the result back through `Arrow.FileWriter`, one Arrow batch type crossing all three codecs.
- `api-duckdb`(`.api/api-duckdb.md`): the symmetric counterpart to its `ARROW_BOUNDARY` — DuckDB exposes no Arrow type so its Arrow path is a native ADBC bridge, while `ParquetSharp.Arrow` exposes `RecordBatch` as a first-class managed call; a Parquet file is the durable form DuckDB queries and this codec writes/reads.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the per-column CLR→physical mapping for a `[ValueObject]`/`[SmartEnum]` owner reuses its generated key projection; the projected key writes through `LogicalColumnWriter<TValue>.WriteBatch`, and a custom `LogicalWriteConverterFactory`/`LogicalReadConverterFactory` is the seam for a non-default physical encoding.
- `api-aws-kms`/`api-azure-keyvault`/`api-google-kms`: `CryptoFactory(KmsClientFactory)` binds an `IKmsClient` whose `WrapKey`/`UnwrapKey` delegate to the admitted KMS clients; `KmsConnectionConfig.RefreshKeyAccessToken` rotates the access token in place, and the tenant KEK id binds the file to the `Element/identity#KMS_CUSTODY` row.
- `api-zstd`/`api-lz4`: `Compression.Zstd`/`Lz4` are C++-core codecs Parquet applies internally, orthogonal to the standalone `ZstdSharp.Port`/`K4os.Compression.LZ4` blob snapshot codecs, so a Parquet extract is compressed once by the writer, never double-compressed.
- `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): its managed `Parquet.Net` writer (`WriteToParquetZip`) emits one Brotli-compressed `.parquet` per BIM table inside a zip; this native reader consumes those standard-format files at the format boundary (managed writer / native libparquet-cpp reader interoperate at the format, never the assembly) and streams them as `RecordBatch` through `Arrow.FileReader` into the columnar query rail.
- statistics/page-index pushdown: `EnableWritePageIndex` + `SortingColumns` write the column/offset index that lets the DuckDB/Arrow read path skip row groups by predicate; `ParquetSharp.Dataset.DatasetReader.ToBatches` yields the same `RecordBatch` `IArrowArrayStream`, so a Hive-partitioned directory is the lake-scan counterpart to a single-file `Arrow.FileReader` read, `Col`/`IFilter` skipping partitions and those emitted row groups.

[LOCAL_ADMISSION]:
- Parquet file write enters behind the `Query/columnar#ARTIFACT_EGRESS` columnar-extract receipt: a `[ValueObject]`/`[SmartEnum]` owner projects to its physical key through the snapshot codec, and `LogicalColumnWriter<TValue>.WriteBatch` (or Arrow `WriteRecordBatch`) writes the column.
- managed `Stream` ctors are the admitted sink/source for object-store residence: a Parquet extract writes into an S3/MinIO upload stream and reads from a download stream with `leaveOpen`, so the store owns the stream lifecycle.
- Object-store reads tune on the policy objects rather than around them: `PreBuffer` with a `CacheOptions` row sized to the store, `SetFooterReadSize` past the typical footer, and `BatchSize` at the downstream batch grain — a hand-rolled range-request layer re-implements coalescing the native core already owns.
- Untrusted Parquet enters behind `SetThriftStringSizeLimit` and `SetThriftContainerSizeLimit` set at the ingress, since footer parse allocates against lengths the file declares.
- `DefaultWriterProperties` stays unset: process-wide static policy crosses plugin load contexts and silently re-tunes an extract another composition owns, so every knob rides its own `WriterPropertiesBuilder`.
- `ParquetRowWriter<TTuple>` is the admitted path for fact-record extracts of fixed tuple/POCO shape; the low-level `LogicalColumnWriter<TValue>` path is admitted where the column type is computed at runtime through the visitor.
- PME is the admitted at-rest encryption for sensitive extracts and the KMS leg is the admitted derivation: `CryptoFactory` wraps DEKs with a tenant KEK an `IKmsClient` adapter reaches, so no process holds a master key and rotation is `RotateMasterKeys` over the published file.
- Explicit-key builders enter for a fixture or a recovery read whose key is already in hand; a published extract deriving from them re-homes master-key custody into process memory, which is the custody the KMS leg exists to remove.

[RAIL_LAW]:
- Packages: `ParquetSharp`, `ParquetSharp.Dataset`
- Owns: native Parquet file read/write — low-level column chunks, typed logical batches, row-oriented tuple mapping, the `Apache.Arrow` C-Data bridge, the four-policy read and write tuning plane, Parquet Modular Encryption, and the partitioned multi-file dataset scan over that native core
- Accept: `ParquetFileWriter`/`ParquetFileReader` over a managed `Stream`, typed `LogicalColumnWriter<TValue>.WriteBatch`/`ParquetRowWriter<TTuple>`, the `Arrow.FileReader`/`FileWriter` `RecordBatch` bridge, `CryptoFactory` PME over an `IKmsClient` adapter mounted through `WriterPropertiesBuilder.Encryption` and `ReaderProperties.FileDecryptionProperties`, and `DatasetReader.ToBatches`/`ToTable` with `Col`/`IFilter` pushdown over a partitioned directory
- Reject: hand-rolled Parquet byte framing, a per-cell write loop where a typed batch or Arrow `RecordBatch` exists, a managed re-implementation of a codec the native core owns, an envelope-encryption pass over finished Parquet bytes where PME encrypts pages and footer in place under per-column keys, a hand-rolled directory walk where `DatasetReader` owns partitioned scan, a caller-built range-request layer where `PreBuffer` and `CacheOptions` own coalescing, a `DefaultWriterProperties` write standing in for a builder row, a `using` bounding a `CryptoFactory` around a derivation, and re-declaring the `Apache.Arrow` model `api-arrow` owns
