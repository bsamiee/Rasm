# [RASM_PERSISTENCE_API_PARQUETSHARP]

`ParquetSharp` owns the native libparquet-cpp Parquet read/write codec the managed `Apache.Arrow` C# stack lacks, layering three surfaces over one Arrow C++ core: the low-level `ColumnWriter`/`ColumnReader` chunk API and its typed `LogicalColumnWriter<TValue>` mirror, the `RowOriented.ParquetFile` tuple mapper, and the `ParquetSharp.Arrow` `RecordBatch` bridge over the Arrow C Data Interface.

It reads and writes Parquet from a managed `Stream` or Arrow batch with no SQL engine — the direct columnar-file lane distinct from the DuckDB `COPY ... TO` path, with `ParquetSharp.Dataset` layering a Hive-partitioned lake scanner over the same core under `Col`/`IFilter` predicate and column pushdown.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ParquetSharp`
- package: `ParquetSharp` (Apache-2.0)
- assembly: `ParquetSharp`
- namespace: `ParquetSharp`, `ParquetSharp.Schema`, `ParquetSharp.Arrow`, `ParquetSharp.RowOriented`, `ParquetSharp.Encryption`, `ParquetSharp.IO`
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
|  [07]   | `Statistics` / `Statistics<TValue>`                  | column statistics | typed min/max/null-count per column chunk              |
|  [08]   | `ByteArray` / `FixedLenByteArray`                    | physical value    | variable/fixed binary cell (decimal materializes here) |
|  [09]   | `Int96` / `Date` / `DateTimeNanos` / `TimeSpanNanos` | physical value    | legacy/temporal physical cells                         |

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

A `DatasetReader` ctor takes either a concrete `IPartitioning` or an `IPartitioningFactory` that infers one from the directory tree; each scheme carries a nested `Factory : IPartitioningFactory`.

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

[ENTRYPOINT_SCOPE]: writer tuning and Parquet Modular Encryption

`WriterPropertiesBuilder` is the full tuning surface; every `[SURFACE]` below is a `.` builder call, and every column-targeting method carries a global, `string path`, and `ColumnPath` overload (the `path?` slot). `Encryption.CryptoFactory` derives `FileEncryptionProperties`/`FileDecryptionProperties` from a `KmsConnectionConfig` and an `EncryptionConfiguration`, wrapping data-encryption keys with KEKs from a customer `IKmsClient`.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `.Compression(path?, codec)` / `.CompressionLevel(path?, n)`            | builder | per-column codec and level                 |
|  [02]   | `.EnableDictionary(path?)` / `.DisableDictionary(path?)`                | builder | per-column dictionary encoding             |
|  [03]   | `.Encoding(path?, encoding)`                                            | builder | per-column physical encoding               |
|  [04]   | `.EnableWritePageIndex(path?)` / `.EnablePageChecksum()`                | builder | column/offset index + CRC page checksums   |
|  [05]   | `.SortingColumns(WriterProperties.SortingColumn[])`                     | builder | declares sorted-column metadata            |
|  [06]   | `.EnableStatistics(path?)` / `.SetMaxStatisticsSize(n)`                 | builder | per-column statistics policy               |
|  [07]   | `.MaxRowGroupLength(n)` / `.DataPagesize(n)` / `.WriteBatchSize(n)`     | builder | row-group / page / batch sizing            |
|  [08]   | `.Version(ParquetVersion)` / `.DataPageVersion(ParquetDataPageVersion)` | builder | format and data-page version               |
|  [09]   | `.EnableStoreDecimalAsInteger()` / `.CreatedBy(s)`                      | builder | decimal storage + writer signature         |
|  [10]   | `.Encryption(FileEncryptionProperties?)` / `.Build()`                   | builder | binds PME, materializes `WriterProperties` |
|  [11]   | `new CryptoFactory(kmsClientFactory)`                                   | ctor    | binds a customer `IKmsClient` factory      |
|  [12]   | `.GetFileEncryptionProperties(kmsConfig, encConfig, filePath?)`         | crypto  | derives PME file encryption properties     |
|  [13]   | `.GetFileDecryptionProperties(kmsConfig, decConfig, filePath?)`         | crypto  | derives PME file decryption properties     |
|  [14]   | `.RotateMasterKeys(kmsConfig, path, doubleWrapping, cacheLifetime)`     | crypto  | re-wraps keys for an existing file         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- three layers terminate at one `ParquetSharpNative.dylib` handle: low-level (`ParquetFileWriter`→`RowGroupWriter`→`ColumnWriter`→`LogicalColumnWriter<TValue>`), row-oriented (`RowOriented.ParquetFile`→`ParquetRowWriter<TTuple>`), and Arrow (`Arrow.FileWriter`/`FileReader` ↔ `Apache.Arrow`).
- `ParquetSharpNative` wraps the Apache Arrow/Parquet C++ build; codec selection is C++-side, never a managed re-implementation.
- `WriterProperties` is immutable, built by `WriterPropertiesBuilder`; the bare-`Compression` writer ctors are sugar over a default builder.
- `Column[]` ctors derive a `GroupNode` schema automatically; the `GroupNode` ctors take a hand-built tree for nested/repeated columns the flat `Column[]` shape cannot express.
- handles are `IDisposable` and own native memory; `Close()` flushes the footer, and a reader/writer over a managed `Stream` honors `leaveOpen`.
- `Arrow.FileReader.GetRecordBatchReader` returns `Apache.Arrow.Ipc.IArrowArrayStream` and `Arrow.FileWriter.WriteRecordBatch`/`WriteTable` consume `RecordBatch`/`Table`; `api-arrow` owns the in-memory Arrow model, and the two compose at the `RecordBatch`/`Schema` boundary — this file codec never re-declares that model.

[STACKING]:
- `api-arrow`(`.api/api-arrow.md`): Parquet ↔ `RecordBatch` is the load-bearing stack — the analytical lane reads a Parquet file as an `IArrowArrayStream`, feeds it to a DuckDB query or an ADBC consumer (`Apache.Arrow.Adbc`), and writes the result back through `Arrow.FileWriter`, one Arrow batch type crossing all three codecs.
- `api-duckdb`(`.api/api-duckdb.md`): the symmetric counterpart to its `ARROW_BOUNDARY` — DuckDB exposes no Arrow type so its Arrow path is a native ADBC bridge, while `ParquetSharp.Arrow` exposes `RecordBatch` as a first-class managed call; a Parquet file is the durable form DuckDB queries and this codec writes/reads.
- `api-thinktecture-serialization`(`.api/api-thinktecture-serialization.md`): the per-column CLR→physical mapping for a `[ValueObject]`/`[SmartEnum]` owner reuses its key projection; the projected key writes through `LogicalColumnWriter<TValue>.WriteBatch`, and a custom `LogicalWriteConverterFactory`/`LogicalReadConverterFactory` is the seam for a non-default physical encoding.
- `api-aws-kms`/`api-azure-keyvault`/`api-google-kms`: `CryptoFactory(KmsClientFactory)` binds an `IKmsClient` whose `WrapKey`/`UnwrapKey` delegate to the admitted KMS clients; `KmsConnectionConfig.RefreshKeyAccessToken` rotates the access token in place, and the tenant KEK id binds the file to the `Element/identity#KEY_ENVELOPE` row.
- `api-zstd`/`api-lz4`: `Compression.Zstd`/`Lz4` are C++-core codecs Parquet applies internally, orthogonal to the standalone `ZstdSharp.Port`/`K4os.Compression.LZ4` blob snapshot codecs, so a Parquet extract is compressed once by the writer, never double-compressed.
- `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): its managed `Parquet.Net` writer (`WriteToParquetZip`) emits one Brotli-compressed `.parquet` per BIM table inside a zip; this native reader consumes those standard-format files at the format boundary (managed writer / native libparquet-cpp reader interoperate at the format, never the assembly) and streams them as `RecordBatch` through `Arrow.FileReader` into the columnar query rail.
- statistics/page-index pushdown: `EnableWritePageIndex` + `SortingColumns` write the column/offset index that lets the DuckDB/Arrow read path skip row groups by predicate; `ParquetSharp.Dataset.DatasetReader.ToBatches` yields the same `RecordBatch` `IArrowArrayStream`, so a Hive-partitioned directory is the lake-scan counterpart to a single-file `Arrow.FileReader` read, `Col`/`IFilter` skipping partitions and those emitted row groups.

[LOCAL_ADMISSION]:
- Parquet file write enters behind the `Query/columnar#ARTIFACT_EGRESS` columnar-extract receipt: a `[ValueObject]`/`[SmartEnum]` owner projects to its physical key through the snapshot codec, and `LogicalColumnWriter<TValue>.WriteBatch` (or Arrow `WriteRecordBatch`) writes the column.
- managed `Stream` ctors are the admitted sink/source for object-store residence: a Parquet extract writes into an S3/MinIO upload stream and reads from a download stream with `leaveOpen`, so the store owns the stream lifecycle.
- `ParquetRowWriter<TTuple>` is the admitted path for fact-record extracts of fixed tuple/POCO shape; the low-level `LogicalColumnWriter<TValue>` path is admitted where the column type is computed at runtime through the visitor.
- PME is the admitted at-rest encryption for sensitive extracts: `CryptoFactory` wraps DEKs with a tenant KEK from an `IKmsClient` adapter.

[RAIL_LAW]:
- Packages: `ParquetSharp`, `ParquetSharp.Dataset`
- Owns: native Parquet file read/write — low-level column chunks, typed logical batches, row-oriented tuple mapping, the `Apache.Arrow` C-Data bridge, Parquet Modular Encryption, and the partitioned multi-file dataset scan over that native core
- Accept: `ParquetFileWriter`/`ParquetFileReader` over a managed `Stream`, typed `LogicalColumnWriter<TValue>.WriteBatch`/`ParquetRowWriter<TTuple>`, the `Arrow.FileReader`/`FileWriter` `RecordBatch` bridge, `CryptoFactory` PME over an `IKmsClient` adapter, and `DatasetReader.ToBatches`/`ToTable` with `Col`/`IFilter` pushdown over a partitioned directory
- Reject: hand-rolled Parquet byte framing, a per-cell write loop where a typed batch or Arrow `RecordBatch` exists, a managed re-implementation of a codec the native core owns, a hand-rolled directory walk where `DatasetReader` owns partitioned scan, and re-declaring the `Apache.Arrow` model `api-arrow` owns
