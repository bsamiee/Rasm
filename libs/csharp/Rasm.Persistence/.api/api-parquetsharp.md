# [RASM_PERSISTENCE_API_PARQUETSHARP]

`ParquetSharp` is the native libparquet-cpp Parquet read/write codec the managed `Apache.Arrow` C# stack lacks (no Parquet reader/writer in `Apache.Arrow`). It owns three layered surfaces over one Arrow C++ core: the low-level `ParquetFileWriter`/`RowGroupWriter`/`ColumnWriter` column chunk API and its typed `LogicalColumnWriter<TValue>.WriteBatch` mirror, the row-oriented `RowOriented.ParquetFile.CreateRowWriter<TTuple>`/`CreateRowReader<TTuple>` POCO/tuple mapper, and the `ParquetSharp.Arrow.FileReader`/`FileWriter` bridge that produces and consumes `Apache.Arrow` `RecordBatch`/`Table` over the Arrow C Data Interface. The `WriterPropertiesBuilder` carries the full Parquet tuning surface (per-column compression, dictionary, encoding, page index, page checksum, sorting columns, statistics, data-page version), and `ParquetSharp.Encryption.CryptoFactory` carries Parquet Modular Encryption (PME) with an `IKmsClient` factory that stacks onto the admitted KMS catalogs. This is the direct columnar-file lane distinct from the DuckDB SQL `COPY ... TO 'x.parquet'` path (`api-duckdb`): ParquetSharp reads and writes Parquet from a managed `Stream`/Arrow batch without a SQL engine in the loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ParquetSharp`
- package: `ParquetSharp`
- version: `23.0.0.2`
- license: Apache-2.0
- assembly: `ParquetSharp`
- namespace: `ParquetSharp`, `ParquetSharp.Schema`, `ParquetSharp.Arrow`, `ParquetSharp.RowOriented`, `ParquetSharp.Encryption`, `ParquetSharp.IO`
- target: multi-target (`net8.0`, `netstandard2.1`, `net471`); the `net10.0` consumer binds `lib/net8.0` (no `net10.0` asset ships)
- native: `runtimes/osx-arm64/native/ParquetSharpNative.dylib` (plus `osx-x64`, `linux-x64`, `linux-arm64`, `win-x64`, `win-arm64`); the wrapped Apache Arrow/Parquet C++ core, P/Invoke-loaded by `ParquetFileWriter`/`ParquetFileReader` handle construction — RID-resolved at load, never AnyCPU
- xml docs: `ParquetSharp.xml` ships beside the assembly; member intent is doc-comment-sourced
- rail: columnar-file-codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file reader/writer roots
- rail: columnar-file-codec

`ParquetFileWriter` ctors fan three sink shapes (`string path`, `OutputStream`, managed `Stream` with `leaveOpen`) crossed with two schema shapes (a `Column[]` array or a `GroupNode` schema tree) and two property shapes (a bare `Compression` enum or a full `WriterProperties`), plus an optional `LogicalTypeFactory` and `IReadOnlyDictionary<string,string>` key-value file metadata. `ParquetFileReader` ctors take `string path`, `RandomAccessFile`, or `Stream` with an optional `ReaderProperties`. The managed `Stream` ctors are the load-bearing seam into the object-store residence path (`api-objectstore` / `AWSSDK.S3` / `Minio`): a Parquet file is written straight into an upload stream and read back from a download stream without a temp file.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                  |
| :-----: | :------------------ | :--------------- | :-------------------------------------- |
|  [01]   | `ParquetFileWriter` | writer root      | owns native writer handle, row groups   |
|  [02]   | `ParquetFileReader` | reader root      | owns native reader handle, file metadata |
|  [03]   | `RowGroupWriter`    | row-group writer | `Column(i)`/`NextColumn()`, byte counters |
|  [04]   | `RowGroupReader`    | row-group reader | `Column(i)`, `RowGroupMetaData`         |
|  [05]   | `ColumnWriter`      | column writer    | low-level physical column write         |
|  [06]   | `ColumnReader`      | column reader    | low-level physical column read          |
|  [07]   | `Column` / `Column<T>` | schema column | maps a CLR type to a Parquet schema node |
|  [08]   | `FileMetaData`      | file metadata    | row counts, schema, key-value metadata  |
|  [09]   | `RowGroupMetaData`  | row-group metadata | per-group row/byte/column statistics  |
|  [10]   | `ColumnChunkMetaData` | chunk metadata | per-chunk encoding, compression, statistics |

[PUBLIC_TYPE_SCOPE]: typed logical column family
- rail: columnar-file-codec

The logical layer maps CLR values to Parquet physical types with repetition/definition-level handling for nullables and nesting. `LogicalColumnWriter<TElement>` and `LogicalColumnReader<TElement>` are the typed batch mirror over the physical `ColumnWriter`/`ColumnReader`. For a column whose element type is only known at runtime, `ColumnDescriptor.Apply<TReturn>(LogicalTypeFactory, IColumnDescriptorVisitor<TReturn>)` dispatches into the visitor's `OnColumnDescriptor<TPhysical, TLogical, TElement>()` (the strongly-typed continuation reached by reflection-free generic dispatch); `ILogicalColumnWriterVisitor<TReturn>`/`ILogicalColumnReaderVisitor<TReturn>` are the same pattern over an opened logical stream. `ColumnWriter.LogicalWriterOverride<TElement>()`/`ColumnReader.LogicalReaderOverride<TElement>()` force an element type that overrides the schema-inferred default. `Nested<T>` is the public struct-nesting wrapper for repeated/struct columns; `ColumnReader.LogicalReader(useNesting: true)` opts an untyped reader into `Nested<T>` reassembly.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                  |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `LogicalColumnWriter<TValue>`  | typed writer     | `WriteBatch(ReadOnlySpan<TValue>)`      |
|  [02]   | `LogicalColumnReader<TValue>`  | typed reader     | `ReadBatch(Span<TValue>)`, `ReadAll`, `GetEnumerator` |
|  [03]   | `LogicalColumnStream`          | stream base      | shared buffered batch state             |
|  [04]   | `ILogicalColumnWriterVisitor<TReturn>` | writer visitor | runtime-typed logical-writer dispatch |
|  [05]   | `ILogicalColumnReaderVisitor<TReturn>` | reader visitor | runtime-typed logical-reader dispatch |
|  [06]   | `IColumnDescriptorVisitor<TReturn>` | descriptor visitor | `OnColumnDescriptor<TPhysical,TLogical,TElement>()` continuation reached by `ColumnDescriptor.Apply` |
|  [07]   | `LogicalTypeFactory`           | type factory     | maps CLR types to logical Parquet types |
|  [08]   | `LogicalReadConverterFactory`  | read converter   | custom physical→CLR conversion          |
|  [09]   | `LogicalWriteConverterFactory` | write converter  | custom CLR→physical conversion          |
|  [10]   | `Nested<T>`                    | nesting wrapper  | public struct-nesting wrapper for repeated/struct schemas |

[PUBLIC_TYPE_SCOPE]: schema and physical value family
- rail: columnar-file-codec

`Schema.GroupNode`/`Schema.PrimitiveNode` build the column tree; `LogicalType` (root `ParquetSharp` namespace) is the annotated-logical-type base, its sealed subtypes (`StringLogicalType`/`DecimalLogicalType`/`DateLogicalType`/`TimestampLogicalType`/`TimeLogicalType`/`IntLogicalType`/`JsonLogicalType`/`BsonLogicalType`/`UuidLogicalType`/`Float16LogicalType`/`ListLogicalType`/`MapLogicalType`/`EnumLogicalType`/`IntervalLogicalType`/`NullLogicalType`/`NoneLogicalType`) reached by the `LogicalType.Decimal(...)`/`.Timestamp(...)`/`.String()` static factories. The physical value structs (`ByteArray`, `FixedLenByteArray`, `Int96`, `Date`, `DateTimeNanos`, `TimeSpanNanos`) are the public wire-level representations the converters target; a Parquet `Decimal` cell physically materializes through `FixedLenByteArray` (`DecimalConverter`/`Decimal128` are internal). `Statistics<TValue>` exposes the per-column typed min/max/null-count the page-index pushdown reads.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [RAIL]                                |
| :-----: | :------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `Schema.GroupNode`              | schema node     | struct/group schema node              |
|  [02]   | `Schema.PrimitiveNode`          | schema node     | leaf primitive schema node            |
|  [03]   | `Schema.Node`                   | schema node base | shared node identity/repetition      |
|  [04]   | `LogicalType` (+ sealed subtypes) | logical type  | annotated logical type base + `Decimal`/`Timestamp`/`String` factory subtypes |
|  [05]   | `SchemaDescriptor`              | schema descriptor | flattened leaf-column descriptor    |
|  [06]   | `ColumnDescriptor`              | column descriptor | one leaf column's type/levels       |
|  [07]   | `Statistics` / `Statistics<TValue>` | column statistics | typed min/max/null-count per column chunk |
|  [08]   | `ByteArray` / `FixedLenByteArray` | physical value | variable/fixed binary cell (decimal materializes here) |
|  [09]   | `Int96` / `Date` / `DateTimeNanos` / `TimeSpanNanos` | physical value | legacy/temporal physical cells |

[PUBLIC_TYPE_SCOPE]: enum and policy family
- rail: columnar-file-codec

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `Compression`             | codec enum      | `Uncompressed`/`Snappy`/`Gzip`/`Brotli`/`Zstd`/`Lz4`/`Lz4Frame`/`Lzo`/`Bz2`/`Lz4Hadoop` |
|  [02]   | `Encoding`                | encoding enum   | `Plain`/`PlainDictionary`/`Rle`/`DeltaBinaryPacked`/`DeltaLengthByteArray`/`DeltaByteArray`/`RleDictionary`/`ByteStreamSplit` |
|  [03]   | `ParquetVersion`          | format enum     | physical format version selector       |
|  [04]   | `ParquetDataPageVersion`  | page enum       | `V1`/`V2` data-page layout              |
|  [05]   | `LogicalTypeEnum`         | logical enum    | logical type discriminant               |
|  [06]   | `PhysicalType`            | physical enum   | `Boolean`/`Int32`/`Int64`/`Float`/`Double`/`ByteArray`/`Fixed` |
|  [07]   | `Repetition`              | level enum      | `Required`/`Optional`/`Repeated`        |
|  [08]   | `SortOrder` / `ColumnOrder` | order enum    | column sort-order metadata              |
|  [09]   | `ParquetCipher`           | crypto enum     | `AesGcmV1`/`AesGcmCtrV1` PME cipher     |
|  [10]   | `SizeStatisticsLevel`     | statistics enum | none/chunk/page size-statistics level   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: low-level column-chunk write/read
- rail: columnar-file-codec

`AppendRowGroup()` writes a fully-buffered row group; `AppendBufferedRowGroup()` streams columns of unequal length (the column-major incremental path). `LogicalColumnWriter<TValue>.WriteBatch` accepts a `TElement[]`, an array slice `(values, start, length)`, or a `ReadOnlySpan<TElement>`; the reader's `ReadBatch(Span<TElement>)`/`ReadAll(rows)`/`GetEnumerator()` mirror it.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]    | [CAPABILITY]                              |
| :-----: | :--------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `new ParquetFileWriter(stream, columns, props, …, leaveOpen)` | ctor | opens a Parquet writer over a managed stream |
|  [02]   | `ParquetFileWriter.AppendRowGroup()`           | writer call     | opens a buffered row group                |
|  [03]   | `ParquetFileWriter.AppendBufferedRowGroup()`   | writer call     | opens an unequal-length streaming group   |
|  [04]   | `RowGroupWriter.NextColumn()` / `.Column(i)`   | group call      | advances to / selects a column writer     |
|  [05]   | `ColumnWriter.LogicalWriter<TValue>()`         | column call     | yields the typed `LogicalColumnWriter<TValue>` |
|  [06]   | `LogicalColumnWriter<TValue>.WriteBatch(span)` | typed write     | writes a typed value batch                |
|  [07]   | `new ParquetFileReader(stream, props, leaveOpen)` | ctor         | opens a Parquet reader over a stream       |
|  [08]   | `ParquetFileReader.RowGroup(i)`                | reader call     | selects a `RowGroupReader`                |
|  [09]   | `ColumnReader.LogicalReader<TValue>()`         | column call     | yields the typed `LogicalColumnReader<TValue>` |
|  [10]   | `LogicalColumnReader<TValue>.ReadBatch(span)` / `.Skip(n)` | typed read | reads / skips a typed value batch    |
|  [11]   | `ParquetFileWriter.Close()` / `Dispose()`      | finalize        | flushes footer and closes the file        |

[ENTRYPOINT_SCOPE]: row-oriented POCO/tuple mapping — `RowOriented.ParquetFile`
- rail: columnar-file-codec

The row-oriented layer maps a `TTuple` (a `ValueTuple`, or a POCO whose columns are bound by `[MapToColumn]`) to/from the column layout, so a fact record round-trips without manual column indexing. `CreateRowWriter<TTuple>`/`CreateRowReader<TTuple>` fan the same sink/source shapes as the low-level writer (path/`OutputStream`, `Compression` or `WriterProperties`, explicit `string[] columnNames` or a `Column[]`, key-value metadata, optional `LogicalTypeFactory`/converter factories).

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `ParquetFile.CreateRowWriter<TTuple>(path, columnNames, compression, kvMetadata)` | static factory | opens a typed row writer |
|  [02]   | `ParquetFile.CreateRowWriter<TTuple>(outputStream, writerProperties, columns, …)` | static factory | opens a tuned typed row writer |
|  [03]   | `ParquetRowWriter<TTuple>.WriteRow(row)`           | row write      | writes one mapped record                  |
|  [04]   | `ParquetRowWriter<TTuple>.WriteRows(IEnumerable<TTuple>)` / `.WriteRowSpan(span)` | row write | bulk-writes a record sequence/span |
|  [05]   | `ParquetRowWriter<TTuple>.StartNewRowGroup()`      | row write      | begins a new row group                    |
|  [06]   | `ParquetFile.CreateRowReader<TTuple>(path, …)`     | static factory | opens a typed row reader                  |
|  [07]   | `ParquetRowReader<TTuple>.ReadRows(rowGroup)`      | row read       | reads a row group as `TTuple[]`           |

[ENTRYPOINT_SCOPE]: Arrow C-Data bridge — `ParquetSharp.Arrow`
- rail: columnar-file-codec

`Arrow.FileWriter` writes `Apache.Arrow` `RecordBatch`/`Table` (and `ChunkedArray`/`IArrowArray` column chunks) straight to Parquet; `Arrow.FileReader.GetRecordBatchReader(rowGroups?, columns?)` returns an `IArrowArrayStream` that streams selected row groups and columns back as Arrow batches over the C Data Interface. This is the zero-managed-copy column path: Parquet bytes ↔ `Apache.Arrow` `RecordBatch` with no per-cell CLR boxing.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `new Arrow.FileWriter(stream, schema, props, arrowProps, leaveOpen)` | ctor | opens an Arrow-schema Parquet writer |
|  [02]   | `Arrow.FileWriter.WriteRecordBatch(recordBatch, chunkSize)` | arrow write | writes an `Apache.Arrow` record batch |
|  [03]   | `Arrow.FileWriter.WriteTable(table, chunkSize)` | arrow write    | writes an `Apache.Arrow` table            |
|  [04]   | `Arrow.FileWriter.WriteBufferedRecordBatch(batch)` / `.NewBufferedRowGroup()` | arrow write | buffered unequal-length batch path |
|  [05]   | `Arrow.FileWriter.WriteColumnChunk(IArrowArray \| ChunkedArray)` | arrow write | writes one Arrow column chunk |
|  [06]   | `new Arrow.FileReader(stream, props, arrowProps, leaveOpen)` | ctor | opens an Arrow-projecting Parquet reader |
|  [07]   | `Arrow.FileReader.GetRecordBatchReader(rowGroups, columns)` | arrow read | streams `IArrowArrayStream` of Arrow batches |
|  [08]   | `Arrow.FileReader.ParquetReader` / `.SchemaManifest` | accessor | drops to the low-level reader / Arrow schema map |

[ENTRYPOINT_SCOPE]: writer tuning and Parquet Modular Encryption
- rail: columnar-file-codec

`WriterPropertiesBuilder` is the full tuning surface; every column-targeting method has a global, `string path`, and `ColumnPath` overload. `Encryption.CryptoFactory` derives `FileEncryptionProperties`/`FileDecryptionProperties` from a `KmsConnectionConfig` plus an `EncryptionConfiguration`, wrapping data-encryption keys with KEKs from a customer `IKmsClient`; `RotateMasterKeys` re-wraps an existing file's keys.

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `WriterPropertiesBuilder.Compression(path?, codec)` / `.CompressionLevel(path?, n)` | builder | per-column codec and level |
|  [02]   | `.EnableDictionary(path?)` / `.DisableDictionary(path?)` | builder | per-column dictionary encoding |
|  [03]   | `.Encoding(path?, encoding)`                      | builder        | per-column physical encoding              |
|  [04]   | `.EnableWritePageIndex(path?)` / `.EnablePageChecksum()` | builder | column/offset index + CRC page checksums |
|  [05]   | `.SortingColumns(WriterProperties.SortingColumn[])` | builder      | declares sorted-column metadata           |
|  [06]   | `.EnableStatistics(path?)` / `.SetMaxStatisticsSize(n)` | builder  | per-column statistics policy              |
|  [07]   | `.MaxRowGroupLength(n)` / `.DataPagesize(n)` / `.WriteBatchSize(n)` | builder | row-group / page / batch sizing |
|  [08]   | `.Version(ParquetVersion)` / `.DataPageVersion(ParquetDataPageVersion)` | builder | format and data-page version |
|  [09]   | `.EnableStoreDecimalAsInteger()` / `.CreatedBy(s)` | builder       | decimal storage + writer signature        |
|  [10]   | `.Encryption(FileEncryptionProperties?)` / `.Build()` | builder     | binds PME and materializes `WriterProperties` |
|  [11]   | `new CryptoFactory(kmsClientFactory)`             | ctor           | binds a customer `IKmsClient` factory     |
|  [12]   | `CryptoFactory.GetFileEncryptionProperties(kmsConfig, encConfig, filePath?)` | crypto call | derives PME file encryption properties |
|  [13]   | `CryptoFactory.GetFileDecryptionProperties(kmsConfig, decConfig, filePath?)` | crypto call | derives PME file decryption properties |
|  [14]   | `CryptoFactory.RotateMasterKeys(kmsConfig, parquetFilePath, doubleWrapping, cacheLifetimeSeconds)` | crypto call | re-wraps keys for an existing file |

## [04]-[IMPLEMENTATION_LAW]

[PARQUETSHARP_TOPOLOGY]:
- three layers over one native core: low-level (`ParquetFileWriter`→`RowGroupWriter`→`ColumnWriter`→`LogicalColumnWriter<TValue>`), row-oriented (`RowOriented.ParquetFile`→`ParquetRowWriter<TTuple>`), Arrow (`Arrow.FileWriter`/`FileReader` ↔ `Apache.Arrow`). All three terminate at the same `ParquetSharpNative.dylib` handle.
- the native library is the wrapped Apache Arrow/Parquet C++ build; `Compression` codecs and `Encoding`s are whatever the bundled C++ core supports (Snappy/Gzip/Brotli/Zstd/Lz4/Lz4Frame compression; Plain/Rle/Delta*/ByteStreamSplit encodings). Codec selection is C++-side, never a managed re-implementation.
- `WriterProperties` is immutable and built by `WriterPropertiesBuilder`; the bare-`Compression` writer ctors are sugar over a default builder.
- `Column[]` ctors derive a `GroupNode` schema automatically; the `GroupNode` ctors take a hand-built schema tree for nested/repeated columns where the flat `Column[]` shape cannot express the structure.
- handles are `IDisposable` and own native memory; `Close()` flushes the footer. A reader/writer over a managed `Stream` honors `leaveOpen`.

[ARROW_BRIDGE_LAW]:
- `Arrow.FileReader.GetRecordBatchReader` returns `Apache.Arrow.Ipc.IArrowArrayStream`; `Arrow.FileWriter.WriteRecordBatch`/`WriteTable` consume `Apache.Arrow` `RecordBatch`/`Table`. The Arrow CLR model is owned by `api-arrow`, NOT re-declared here — ParquetSharp.Arrow is the file codec, `Apache.Arrow` is the in-memory columnar model. The two compose at the `RecordBatch`/`Schema` boundary.
- this is the symmetric counterpart to `api-duckdb`'s `ARROW_BOUNDARY`: DuckDB's managed surface exposes no Arrow type, so the DuckDB→Arrow path is a native ADBC bridge; ParquetSharp.Arrow exposes the Arrow type directly, so Parquet↔Arrow is a first-class managed call. A Parquet file is the durable form of an Arrow `RecordBatch`; DuckDB queries it; ParquetSharp writes/reads it.

[LOCAL_ADMISSION]:
- Parquet file write enters behind the `Store/profiles` columnar-extract receipt: a `[ValueObject]`/`[SmartEnum]` owner projects to its physical key through the snapshot codec, and the typed `LogicalColumnWriter<TValue>.WriteBatch` (or the Arrow `WriteRecordBatch`) writes the column — never a hand-rolled cell loop.
- the managed `Stream` ctor is the only admitted sink/source for object-store residence: a Parquet extract is written into an S3/MinIO upload stream and read from a download stream with `leaveOpen` so the store owns the stream lifecycle.
- the row-oriented `ParquetRowWriter<TTuple>` is the admitted path for fact-record extracts whose shape is a fixed tuple/POCO; the low-level `LogicalColumnWriter<TValue>` path is admitted where the column type is computed at runtime through the visitor.
- PME is the admitted at-rest encryption for sensitive Parquet extracts: `CryptoFactory` wraps DEKs with a tenant KEK from an `IKmsClient` adapter — see `[STACKING]`.

[STACKING]:
- Arrow columnar model: Parquet ↔ `Apache.Arrow` `RecordBatch` is the load-bearing stack with `api-arrow`. The analytical lane reads a Parquet file as an `IArrowArrayStream`, feeds it to a DuckDB query or an ADBC consumer (`Apache.Arrow.Adbc`), and writes the result back through `Arrow.FileWriter` — one Arrow batch type crosses all three codecs.
- snapshot codec: the per-column CLR→physical mapping for a `[ValueObject]`/`[SmartEnum]` owner reuses the `api-thinktecture-serialization` key projection; the projected key is written through `LogicalColumnWriter<TValue>.WriteBatch`, and a custom `LogicalWriteConverterFactory`/`LogicalReadConverterFactory` is the seam where the owner needs a non-default physical encoding.
- KMS encryption: `CryptoFactory(KmsClientFactory)` binds an `IKmsClient` whose `WrapKey`/`UnwrapKey` delegate to the admitted KMS clients — AWS KMS (`api-aws-kms`), Azure Key Vault (`api-azure-keyvault`), or Google Cloud KMS (`api-google-kms`). `KmsConnectionConfig.RefreshKeyAccessToken` rotates the access token in place, mirroring the credential-rotation seam the Kafka/RabbitMQ transports use; the tenant KEK id binds the file to the `Store/tenancy` row.
- compression alignment: ParquetSharp's `Compression.Zstd`/`Lz4` are the C++-core codecs; the standalone `ZstdSharp.Port`/`K4os.Compression.LZ4` snapshot codecs are orthogonal (blob compression, not Parquet-internal), so a Parquet extract never double-compresses — the file is Zstd-compressed once by the writer.
- statistics/page-index push-down: `EnableWritePageIndex` + `SortingColumns` write the column/offset index that lets the DuckDB/Arrow read path skip row groups by predicate — the Parquet extract is written to be predicate-pushdown-friendly for the federation lane, not as an opaque blob.

[RAIL_LAW]:
- Package: `ParquetSharp`
- Owns: native Parquet file read/write — low-level column chunks, typed logical batches, row-oriented tuple mapping, the `Apache.Arrow` C-Data bridge, and Parquet Modular Encryption
- Accept: `ParquetFileWriter`/`ParquetFileReader` over a managed `Stream`, typed `LogicalColumnWriter<TValue>.WriteBatch`/`ParquetRowWriter<TTuple>`, the `Arrow.FileReader`/`FileWriter` `RecordBatch` bridge, and `CryptoFactory` PME over an `IKmsClient` adapter
- Reject: hand-rolled Parquet byte framing, a per-cell write loop where a typed batch or Arrow `RecordBatch` exists, a managed re-implementation of a codec the native core owns, and re-declaring the `Apache.Arrow` model that `api-arrow` owns
