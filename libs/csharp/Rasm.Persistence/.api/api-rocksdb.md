# [RASM_PERSISTENCE_API_ROCKSDB]

`rocksdb` (assembly `RocksDbSharp`) owns the embedded LSM-tree write-optimized KV/log engine over the bundled native `librocksdb` — the write-amplification lane where ingest, atomic batches, and merge-resolved registers dominate. It is the write half of the `[EMBEDDED_KV]` pair the `[STORE_BACKENDS]` cluster admits beside `LightningDB` (the read-optimized LMDB lane), above the SQLite relational floor (`api-sqlite`). Span-first `Get`/`Put`/`Merge`/`Remove` carries the snapshot-codec boundary, and `Snapshot`/`Checkpoint`/`GetUpdatesSince` own consistency and the CDC changefeed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rocksdb`
- package: `rocksdb` (curiosity-ai line)
- license: BSD-2-Clause binding; Apache-2.0/GPLv2 dual on the native RocksDB core
- assembly: `RocksDbSharp`
- namespace: `RocksDbSharp`, `NativeImport`, `Transitional`
- native: `librocksdb` P/Invoke-loaded through `NativeImport` at first call, RID-resolved at load (`runtimes/osx-arm64/native/librocksdb.dylib`), never AnyCPU
- rail: embedded-lsm-kv

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: database root and consistency family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                   |
| :-----: | :----------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `RocksDb`                | database root    | open handle, all KV/CF/iterate/compact ops     |
|  [02]   | `Snapshot`               | consistency view | point-in-time read snapshot                    |
|  [03]   | `Checkpoint`             | backup primitive | `Save(dir, logSizeForFlush)` hard-linked clone |
|  [04]   | `TransactionLogIterator` | WAL changefeed   | sequence-numbered WAL update stream            |
|  [05]   | `ColumnFamilies`         | CF declaration   | named-family + options descriptor set          |
|  [06]   | `ColumnFamilyHandle`     | CF handle        | per-family operation target                    |
|  [07]   | `Iterator`               | cursor           | ordered/prefix seek + forward/backward scan    |
|  [08]   | `WriteBatch`             | atomic batch     | buffered atomic multi-op write                 |
|  [09]   | `WriteBatchWithIndex`    | indexed batch    | atomic batch with read-your-writes index       |
|  [10]   | `SstFileWriter`          | bulk writer      | writes an external SST for ingest              |
|  [11]   | `LiveFileMetadata`       | file metadata    | per-SST level/size/key-range metadata          |

[PUBLIC_TYPE_SCOPE]: options family

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `Options<T>`                                                | fluent base   | self-returning tuner base for all options  |
|  [02]   | `DbOptions`                                                 | db options    | open-time + cross-CF database options      |
|  [03]   | `ColumnFamilyOptions`                                       | CF options    | per-family compaction/compression/memtable |
|  [04]   | `ReadOptions`                                               | read options  | snapshot, fill-cache, prefix-same-as-start |
|  [05]   | `WriteOptions`                                              | write options | sync, disable-WAL, no-slowdown             |
|  [06]   | `BlockBasedTableOptions`                                    | table options | block size, index type, bloom filter       |
|  [07]   | `BloomFilterPolicy`                                         | filter policy | bits-per-key bloom filter                  |
|  [08]   | `Cache`                                                     | block cache   | LRU block cache instance                   |
|  [09]   | `IngestExternalFileOptions` / `FlushOptions` / `EnvOptions` | op options    | bulk ingest / flush / IO env policy        |

[PUBLIC_TYPE_SCOPE]: customization hooks

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]    | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `MergeOperator`                                     | merge hook       | native read-modify-write merge                    |
|  [02]   | `MergeOperators`                                    | merge factory    | `Create(name, partial, full)` + built-ins         |
|  [03]   | `MergeOperators.FullMergeFunc` / `PartialMergeFunc` | merge delegate   | full/partial operand fold over a span             |
|  [04]   | `Comparator` / `StringComparator`                   | comparator       | custom key ordering                               |
|  [05]   | `SliceTransform`                                    | prefix extractor | prefix-seek + prefix bloom                        |
|  [06]   | `CompactionFilter`                                  | compaction hook  | per-key drop/rewrite during compaction            |
|  [07]   | `ISpanDeserializer<T>`                              | value codec      | zero-copy span→`T` value decode                   |
|  [08]   | `IWriteBatch`                                       | batch contract   | shared `WriteBatch`/`WriteBatchWithIndex` surface |

[PUBLIC_TYPE_SCOPE]: enum and policy family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                             |
| :-----: | :------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `Compression`              | codec enum        | `No`/`Snappy`/`Zlib`/`Bz2`/`Lz4`/`Lz4hc`/`Xpress`/`Zstd` |
|  [02]   | `Compaction`               | style enum        | `Level`/`Universal`/`Fifo` compaction style              |
|  [03]   | `CompactionPri`            | priority enum     | compaction file-pick priority                            |
|  [04]   | `Recovery`                 | WAL recovery enum | WAL recovery mode at open                                |
|  [05]   | `BlockBasedTableIndexType` | index enum        | binary/hash-search/two-level index                       |
|  [06]   | `StatisticsLevel`          | stats enum        | statistics collection verbosity                          |
|  [07]   | `InfoLogLevel`             | log enum          | native log verbosity                                     |
|  [08]   | `PrepopulateBlob`          | blob enum         | blob-cache prepopulation policy                          |
|  [09]   | `PerfLevel` / `PerfMetric` | perf enum         | per-thread perf-context collection                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and configure

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `RocksDb.Open(DbOptions, path, ColumnFamilies)`                                 | static   | opens a multi-column-family database |
|  [02]   | `RocksDb.Open(OptionsHandle, path)`                                             | static   | opens the default-family database    |
|  [03]   | `RocksDb.OpenReadOnly(DbOptions, path, ColumnFamilies, errIfLogFileExists)`     | static   | read-only handle                     |
|  [04]   | `RocksDb.OpenAsSecondary(DbOptions, path, secondaryPath, ColumnFamilies)`       | static   | follower/secondary handle            |
|  [05]   | `RocksDb.OpenWithTtl(OptionsHandle, path, ttlSeconds)`                          | static   | TTL-expiring database                |
|  [06]   | `SetCreateIfMissing`/`SetCreateMissingColumnFamilies`/`IncreaseParallelism`     | instance | open-time db tuning                  |
|  [07]   | `SetCompression(Compression)`/`SetCompactionStyle(Compaction)`                  | instance | per-family codec + compaction style  |
|  [08]   | `SetMergeOperator`/`SetComparator`/`SetPrefixExtractor`/`SetCompactionFilter`   | instance | install custom hooks                 |
|  [09]   | `SetBlockBasedTableFactory`/`SetWriteBufferSize`/`SetWalRecoveryMode(Recovery)` | instance | read-path / memtable / WAL tuning    |
|  [10]   | `PrepareForBulkLoad`/`OptimizeForPointLookup`/`OptimizeLevelStyleCompaction`    | instance | bulk-load / point-lookup / presets   |
|  [11]   | `new ColumnFamilies(defaultOptions).Add(name, ColumnFamilyOptions)`             | ctor     | declares the family set to open      |

[ENTRYPOINT_SCOPE]: point and batch read/write

`Get`/`Put`/`Merge`/`Remove` each fan `byte[]`/`ReadOnlySpan<byte>`/`string` forms and take an optional trailing `ColumnFamilyHandle` + `ReadOptions`/`WriteOptions`.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `RocksDb.Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)`            | instance | writes one key (span, no alloc)  |
|  [02]   | `RocksDb.Get(ReadOnlySpan<byte> key)`                                      | instance | reads one value (`byte[]?`)      |
|  [03]   | `RocksDb.Get<T>(key, ISpanDeserializer<T>)`                                | instance | zero-copy span→`T` decode        |
|  [04]   | `RocksDb.GetFixedSizeValue(key, Span<byte> output)`                        | instance | reads into a caller buffer       |
|  [05]   | `RocksDb.MultiGet(byte[][] keys)`                                          | instance | batched multi-key read           |
|  [06]   | `RocksDb.Merge(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)`          | instance | enqueues a merge operand         |
|  [07]   | `RocksDb.Remove(ReadOnlySpan<byte> key)`                                   | instance | deletes one key                  |
|  [08]   | `RocksDb.HasKey(ReadOnlySpan<byte> key)`                                   | instance | key-presence probe (bloom-aware) |
|  [09]   | `RocksDb.Write(WriteBatch)` / `Write(WriteBatchWithIndex)`                 | instance | applies a batch atomically       |
|  [10]   | `WriteBatch.Put(k,v).Merge(k,v).Delete(k).DeleteRange(start,end)`          | fold     | builds an atomic multi-op batch  |
|  [11]   | `WriteBatch.SetSavePoint()` / `.RollbackToSavePoint()` / `.PopSavePoint()` | instance | nested rollback within a batch   |
|  [12]   | `WriteBatchWithIndex.Get(k, db)` / `.NewIterator(…)`                       | instance | reads pending batch writes       |
|  [13]   | `WriteBatchWithIndex.CreateIteratorWithBase(baseIterator)`                 | instance | merges base + pending iterator   |

[ENTRYPOINT_SCOPE]: iterate, snapshot, compact, ingest

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `RocksDb.NewIterator(cf?, readOptions?)`                                | instance | opens an ordered iterator            |
|  [02]   | `Iterator.Seek(ReadOnlySpan<byte>)` / `.SeekForPrev(span)`              | instance | positions at ≥/≤ key (prefix-seek)   |
|  [03]   | `Iterator.SeekToFirst()` / `.SeekToLast()` / `.Next()` / `.Prev()`      | instance | full/range ordered scan              |
|  [04]   | `Iterator.Valid()` / `.GetKeySpan()` / `.GetValueSpan()`                | instance | zero-copy current-cell access        |
|  [05]   | `Iterator.Value<T>(ISpanDeserializer<T>)`                               | instance | decodes the current value to `T`     |
|  [06]   | `RocksDb.CreateSnapshot()` (+ `ReadOptions.Snapshot`)                   | instance | consistent point-in-time read view   |
|  [07]   | `RocksDb.Checkpoint().Save(dir, logSizeForFlush)`                       | instance | hard-linked consistent on-disk clone |
|  [08]   | `RocksDb.CompactRange(start, limit, cf?)`                               | instance | forces compaction of a key range     |
|  [09]   | `new SstFileWriter(envOptions, ioOptions).Open(path).Put(k,v).Finish()` | ctor     | writes an external SST               |
|  [10]   | `RocksDb.IngestExternalFiles(files, IngestExternalFileOptions, cf?)`    | instance | atomically ingests pre-built SSTs    |
|  [11]   | `RocksDb.GetLiveFilesMetadata()` / `GetProperty(name, cf?)`             | instance | per-SST metadata / engine stats      |

[ENTRYPOINT_SCOPE]: column family and WAL changefeed

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `RocksDb.CreateColumnFamily(ColumnFamilyOptions, name)`             | instance | creates a column family at runtime          |
|  [02]   | `RocksDb.GetColumnFamily(name)` / `TryGetColumnFamily(name, out h)` | instance | resolves a family handle                    |
|  [03]   | `RocksDb.GetDefaultColumnFamily()`                                  | instance | the default family handle                   |
|  [04]   | `RocksDb.DropColumnFamily(name)`                                    | instance | drops a column family                       |
|  [05]   | `RocksDb.GetLatestSequenceNumber()`                                 | instance | latest WAL sequence number                  |
|  [06]   | `RocksDb.GetUpdatesSince(sequenceNumber)`                           | instance | opens a `TransactionLogIterator` from a seq |
|  [07]   | `MergeOperators.Create(name, partialMergeFunc, fullMergeFunc)`      | static   | builds a custom merge operator              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `RocksDb`/options/iterator/batch type is a managed handle over a native pointer, `IDisposable`; `NativeImport` P/Invoke-loads the RID-resolved `librocksdb` at first call.
- Option tuners live on the CRTP base `Options<T>` (`DbOptions : Options<DbOptions>`, `ColumnFamilyOptions : Options<ColumnFamilyOptions>`), so every `Set*`/`Optimize*` returns the concrete options type for fluent chaining; the non-generic `Options`/`OptionsHandle` is the base handle, not the tuner surface.
- `Get`/`Put`/`Merge`/`Remove` span the key/value API: their `ReadOnlySpan<byte>` overloads and `Iterator.GetKeySpan()`/`GetValueSpan()` are zero-allocation, the `byte[]`/`string` overloads convenience, the `string` form carrying an optional `Encoding` (default UTF-8).
- `WriteBatch` is the atomic unit: every `Put`/`Merge`/`Delete`/`DeleteRange` in one batch applies under one `Write`; `WriteBatchWithIndex` adds a queryable index for read-your-writes, and savepoints give nested rollback.
- `Merge` is the LSM read-modify-write primitive: it enqueues an operand the installed `MergeOperator` resolves at read/compaction, never a get-modify-put round trip.
- Consistency rides `Snapshot` (cheap in-memory read view), `Checkpoint` (durable hard-linked clone, the backup form), and `GetUpdatesSince`→`TransactionLogIterator` (the WAL changefeed).

[STACKING]:
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner projects to its physical key through `api-thinktecture-serialization`; the bytes write via span `Put`/`Merge` and decode via `Get<T>(ISpanDeserializer<T>)`, the deserializer the seam where `api-messagepack`/`api-cbor` decode the value span with no managed copy.
- compression: `Compression.Zstd`/`Lz4` are the native block codecs applied per level via `SetCompressionPerLevel`/`SetMinLevelToCompress`; the standalone `ZstdSharp.Port`/`K4os.Compression.LZ4` codecs stay orthogonal (snapshot/blob), so an engine value is block-compressed once.
- merge-operator CRDT: a counter/register/set installs a custom `MergeOperator` via `MergeOperators.Create` whose `FullMergeFunc`/`PartialMergeFunc` fold operands over a `ReadOnlySpan<byte>` — native resolution at compaction, the canonical form for high-write-rate counters over a read-modify-write loop.
- WAL→egress: `GetUpdatesSince` taps the WAL as a sequence-numbered changefeed; each batch frames through `api-redaction` and publishes to the `api-kafka`/`api-rabbitmq` egress, keyed by the WAL sequence number over the same lane as the relational tier.
- backup residence: `Checkpoint.Save` produces a hard-linked clone whose files (or an `SstFileWriter` export) land in `api-objectstore`/`Minio` via the `Store/blobstore` lane; `IngestExternalFiles` is the symmetric bulk-restore path.
- telemetry: `EnableStatistics()` + `GetProperty(name)` expose engine counters to the AppHost `telemetry` port as a metric stream, and `SetInfoLogLevel(InfoLogLevel)` routes native logs.

[LOCAL_ADMISSION]:
- RocksDB enters behind the shared `Store/provisioning` store-backend vocabulary as the embedded write-optimized log/KV class; SQLite (`api-sqlite`) stays the relational floor and LMDB (`LightningDB`) the read-optimized MVCC lane — three distinct backend rows, never collapsed.
- a `[ValueObject]`/`[SmartEnum]` owner crosses into a cell through the span `Put`/`Get<T>(ISpanDeserializer<T>)` codec seam — no per-cell boxing, no hand-rolled byte framing.
- multi-entity layouts use column families (one family per logical stream/index) opened together via `ColumnFamilies`; the family handle is the operation target, never a key-prefix hack.
- atomic multi-key transitions use `WriteBatch` (or `WriteBatchWithIndex` for read-your-writes) under one `Write`, never a sequence of single `Put`s on the durable path.
- `GetUpdatesSince`/`TransactionLogIterator` is the admitted CDC tap; durable cross-process replication stays on the PostgreSQL tier and the messaging-protocol egress.

[RAIL_LAW]:
- Package: `rocksdb` (`RocksDbSharp`)
- Owns: embedded LSM-tree write-optimized KV/log storage — column families, atomic `WriteBatch`/`WriteBatchWithIndex`, prefix-seekable iterators, `Snapshot`/`Checkpoint` consistency, custom merge/comparator/prefix/compaction hooks, bulk SST ingest, WAL changefeed
- Accept: `RocksDb.Open` with typed `ColumnFamilies`, span-first `Get`/`Put`/`Merge`/`Remove`, `WriteBatch` atomicity, `MergeOperators.Create` read-modify-write, `Checkpoint`/`Snapshot` consistency, `GetUpdatesSince` CDC
- Reject: per-cell byte framing where the span codec exists, single-`Put` sequences where a `WriteBatch` is atomic, a key-prefix hack where a column family is the tool, a read-modify-write loop where a merge operator is native, and the fork-only Raft/replication/WAL-inspector surface as canonical RocksDB
