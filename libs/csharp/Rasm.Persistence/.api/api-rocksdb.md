# [RASM_PERSISTENCE_API_ROCKSDB]

`rocksdb` (curiosity-ai line, assembly `RocksDbSharp`) is the embedded LSM-tree write-optimized KV/log engine beyond the SQLite relational B-tree floor (`api-sqlite`) and beside the read-optimized LMDB B+tree engine (`LightningDB`). It owns the full canonical RocksDB surface over the bundled native `librocksdb`: `RocksDb.Open` and its read-only/secondary/TTL variants, the polymorphic `Get`/`Put`/`Merge`/`Remove` family (every key/value as `byte[]`, `ReadOnlySpan<byte>`, or `string`, with a generic deserializer overload), `MultiGet`, atomic `WriteBatch`/`WriteBatchWithIndex` writes, prefix-seekable `Iterator`s, `Snapshot`/`Checkpoint` point-in-time consistency, column families with per-family `ColumnFamilyOptions`, custom `MergeOperator`/`Comparator`/`SliceTransform`, bulk `SstFileWriter`+`IngestExternalFiles`, and the `GetUpdatesSince`/`TransactionLogIterator` WAL changefeed. The `Options<T>` fluent base carries the full self-returning tuner surface (compression per level, compaction style, write-buffer sizing, bloom filters, block-based table factory, WAL recovery mode). This is the write-amplification-optimized log/KV lane; the relational floor stays SQLite, the read-optimized MVCC lane stays LMDB.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rocksdb`
- package: `rocksdb` (NuGet id `rocksdb`; the curiosity-ai maintained line)
- license: BSD-2-Clause (Apache-2.0 / GPLv2 dual on the underlying RocksDB core)
- assembly: `RocksDbSharp`
- namespace: `RocksDbSharp`, `NativeImport`, `Transitional`
- target: multi-target (`net10.0`, `net9.0`, `net8.0` … `netstandard2.0`); the `net10.0` consumer binds `lib/net10.0` directly
- native: `runtimes/osx-arm64/native/librocksdb.dylib` (plus `osx-x64`, `linux-x64`/`linux-arm64` incl. `-musl`/`-jemalloc` variants, `win-x64`); the RocksDB C++ core, P/Invoke-loaded through `NativeImport` at first call — RID-resolved at load, never AnyCPU
- xml docs: none ships (`RocksDbSharp.xml` absent); member intent is decompile-sourced, not doc-comment-sourced
- rail: embedded-lsm-kv

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: database root and consistency family
- rail: embedded-lsm-kv

`RocksDb` is the open-database handle and the canonical surface. `Snapshot` is a point-in-time read view (passed via `ReadOptions`); `Checkpoint.Save` materializes a consistent hard-linked on-disk copy (the backup/clone primitive); `TransactionLogIterator` (from `GetUpdatesSince`) streams the WAL as a sequence-numbered changefeed. `ColumnFamilies`/`ColumnFamilies.Descriptor` declare the per-family layout opened with `Open`.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `RocksDb`                 | database root   | open handle, all KV/CF/iterate/compact ops |
|  [02]   | `Snapshot`                | consistency view | point-in-time read snapshot            |
|  [03]   | `Checkpoint`              | backup primitive | `Save(dir, logSizeForFlush)` hard-linked clone |
|  [04]   | `TransactionLogIterator`  | WAL changefeed  | sequence-numbered WAL update stream     |
|  [05]   | `ColumnFamilies`          | CF declaration  | named-family + options descriptor set   |
|  [06]   | `ColumnFamilyHandle`      | CF handle       | per-family operation target             |
|  [07]   | `Iterator`                | cursor          | ordered/prefix seek + forward/backward scan |
|  [08]   | `WriteBatch`              | atomic batch    | buffered atomic multi-op write          |
|  [09]   | `WriteBatchWithIndex`     | indexed batch   | atomic batch with read-your-writes index |
|  [10]   | `SstFileWriter`           | bulk writer     | writes an external SST for ingest       |
|  [11]   | `LiveFileMetadata`        | file metadata   | per-SST level/size/key-range metadata   |

[PUBLIC_TYPE_SCOPE]: options family
- rail: embedded-lsm-kv

`DbOptions : Options<DbOptions>` and `ColumnFamilyOptions : Options<ColumnFamilyOptions>` both inherit the fluent self-returning tuner base `Options<T>` (the `Set*`/`Optimize*` method surface). Per-operation `ReadOptions`/`WriteOptions` control snapshot binding, sync-on-write, fill-cache, and prefix-seek. `BlockBasedTableOptions` + `BloomFilterPolicy` + `Cache` tune the read path; `IngestExternalFileOptions`/`FlushOptions`/`EnvOptions` tune ingest/flush/IO.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `Options<T>`              | fluent base     | self-returning tuner base for all options |
|  [02]   | `DbOptions`               | db options      | open-time + cross-CF database options   |
|  [03]   | `ColumnFamilyOptions`     | CF options      | per-family compaction/compression/memtable |
|  [04]   | `ReadOptions`             | read options    | snapshot, fill-cache, prefix-same-as-start |
|  [05]   | `WriteOptions`            | write options   | sync, disable-WAL, no-slowdown          |
|  [06]   | `BlockBasedTableOptions`  | table options   | block size, index type, bloom filter    |
|  [07]   | `BloomFilterPolicy`       | filter policy   | bits-per-key bloom filter               |
|  [08]   | `Cache`                   | block cache     | LRU block cache instance                |
|  [09]   | `IngestExternalFileOptions` / `FlushOptions` / `EnvOptions` | op options | bulk ingest / flush / IO env policy |

[PUBLIC_TYPE_SCOPE]: customization hooks
- rail: embedded-lsm-kv

`MergeOperator` (built by `MergeOperators.Create(name, PartialMergeFunc, FullMergeFunc)`) implements read-modify-write merge semantics natively — counters, append-sets, CRDT-style registers — so the engine merges operands at compaction without a read-modify-write round trip. `Comparator`/`StringComparator` install a custom key ordering; `SliceTransform` installs a prefix extractor for prefix-seek bloom filters; `CompactionFilter` drops/rewrites keys during compaction (TTL/GC).

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `MergeOperator`           | merge hook      | native read-modify-write merge          |
|  [02]   | `MergeOperators`          | merge factory   | `Create(name, partial, full)` + built-ins |
|  [03]   | `MergeOperators.FullMergeFunc` / `PartialMergeFunc` | merge delegate | full/partial operand fold over a span |
|  [04]   | `Comparator` / `StringComparator` | comparator | custom key ordering                  |
|  [05]   | `SliceTransform`          | prefix extractor | prefix-seek + prefix bloom            |
|  [06]   | `CompactionFilter`        | compaction hook | per-key drop/rewrite during compaction  |
|  [07]   | `ISpanDeserializer<T>`    | value codec     | zero-copy span→`T` value decode         |
|  [08]   | `IWriteBatch`             | batch contract  | shared `WriteBatch`/`WriteBatchWithIndex` surface |

[PUBLIC_TYPE_SCOPE]: enum and policy family
- rail: embedded-lsm-kv

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `Compression`             | codec enum      | `No`/`Snappy`/`Zlib`/`Bz2`/`Lz4`/`Lz4hc`/`Xpress`/`Zstd` |
|  [02]   | `Compaction`              | style enum      | `Level`/`Universal`/`Fifo` compaction style |
|  [03]   | `CompactionPri`           | priority enum   | compaction file-pick priority           |
|  [04]   | `Recovery`                | WAL recovery enum | WAL recovery mode at open             |
|  [05]   | `BlockBasedTableIndexType` | index enum     | binary/hash-search/two-level index      |
|  [06]   | `StatisticsLevel`         | stats enum      | statistics collection verbosity         |
|  [07]   | `InfoLogLevel`            | log enum        | native log verbosity                    |
|  [08]   | `PrepopulateBlob`         | blob enum       | blob-cache prepopulation policy         |
|  [09]   | `PerfLevel` / `PerfMetric` | perf enum      | per-thread perf-context collection      |

[PUBLIC_TYPE_SCOPE]: fork-noncanonical surface (NOT admitted)
- rail: embedded-lsm-kv

The curiosity-ai fork ships additional non-canonical RocksDB types — `RaftClusterNode`/`RaftConfig`/`PeerState`/`IPeerTransport` (a Raft replication cluster), `ReplicationSession`/`ReplicationConsumer`/`ReplicationDelta`/`ReplicationSource` (a delta-replication protocol), `RocksDbWalInspector`, and `AdaptiveCommitDelayController`/`CommitDelaySnapshot`. These are fork-specific noncanonical surface, not part of the upstream RocksDB API, and are NOT admitted: durable replication is owned by the PostgreSQL tier and the messaging-protocol changefeed (`api-kafka`/`api-rabbitmq`), and `Store` consistency is owned by the canonical `Snapshot`/`Checkpoint`/`TransactionLogIterator` primitives above.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and configure
- rail: embedded-lsm-kv

`RocksDb.Open` takes `(DbOptions, path, ColumnFamilies)` for a multi-family database or `(OptionsHandle, path)` for the default family; `OpenReadOnly`/`OpenAsSecondary`/`OpenWithTtl` open the read-only, follower, and TTL-GC variants. `Options<T>` tuners are fluent and self-returning.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `RocksDb.Open(DbOptions, path, ColumnFamilies)`    | static open    | opens a multi-column-family database       |
|  [02]   | `RocksDb.Open(OptionsHandle, path)`                | static open    | opens the default-family database          |
|  [03]   | `RocksDb.OpenReadOnly(DbOptions, path, ColumnFamilies, errIfLogFileExists)` | static open | read-only handle |
|  [04]   | `RocksDb.OpenAsSecondary(DbOptions, path, secondaryPath, ColumnFamilies)` | static open | follower/secondary handle |
|  [05]   | `RocksDb.OpenWithTtl(OptionsHandle, path, ttlSeconds)` | static open | TTL-expiring database               |
|  [06]   | `new DbOptions().SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true).IncreaseParallelism(n)` | fluent | open-time db tuning |
|  [07]   | `new ColumnFamilyOptions().SetCompression(Compression.Zstd).SetCompactionStyle(Compaction.Level).OptimizeLevelStyleCompaction(budget)` | fluent | per-family tuning |
|  [08]   | `.SetMergeOperator(op)` / `.SetComparator(cmp)` / `.SetPrefixExtractor(transform)` / `.SetCompactionFilter(filter)` | fluent | install custom hooks |
|  [09]   | `.SetBlockBasedTableFactory(BlockBasedTableOptions)` / `.SetWriteBufferSize(n)` / `.SetWalRecoveryMode(Recovery)` | fluent | read-path / memtable / WAL tuning |
|  [10]   | `.PrepareForBulkLoad()` / `.OptimizeForPointLookup(cacheMB)` | fluent | bulk-load / point-lookup presets       |
|  [11]   | `new ColumnFamilies(defaultOptions)` then `.Add(name, ColumnFamilyOptions)` | builder | declares the family set to open |

[ENTRYPOINT_SCOPE]: point and batch read/write
- rail: embedded-lsm-kv

`Get`/`Put`/`Merge`/`Remove` each fan `byte[]`, `ReadOnlySpan<byte>`, and `string` key/value forms with optional `ColumnFamilyHandle` + `ReadOptions`/`WriteOptions`. `Get<T>(ReadOnlySpan<byte>, ISpanDeserializer<T> | Func<Stream,T>, …)` decodes the value zero-copy; `GetFixedSizeValue` reads into a caller `Span<byte>`; `MultiGet` batches reads.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `RocksDb.Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, cf?, writeOptions?)` | span write | writes one key (span, no alloc) |
|  [02]   | `RocksDb.Get(ReadOnlySpan<byte> key, cf?, readOptions?)` | span read | reads one value (`byte[]?`)        |
|  [03]   | `RocksDb.Get<T>(key, ISpanDeserializer<T>, cf?, readOptions?)` | typed read | zero-copy span→`T` decode        |
|  [04]   | `RocksDb.GetFixedSizeValue(key, Span<byte> output, cf?, readOptions?)` | span read | reads into a caller buffer       |
|  [05]   | `RocksDb.MultiGet(byte[][] keys, cf[]?, readOptions?)`  | batch read  | batched multi-key read                 |
|  [06]   | `RocksDb.Merge(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, cf?, writeOptions?)` | span merge | enqueues a merge operand |
|  [07]   | `RocksDb.Remove(ReadOnlySpan<byte> key, cf?, writeOptions?)` | span delete | deletes one key                    |
|  [08]   | `RocksDb.HasKey(ReadOnlySpan<byte> key, cf?, readOptions?)` | probe    | key-presence probe (bloom-aware)        |
|  [09]   | `RocksDb.Write(WriteBatch, writeOptions?)` / `Write(WriteBatchWithIndex, writeOptions?)` | atomic apply | applies a batch atomically |
|  [10]   | `WriteBatch.Put(k,v).Merge(k,v).Delete(k).DeleteRange(start,end)` | fluent batch | builds an atomic multi-op batch |
|  [11]   | `WriteBatch.SetSavePoint()` / `.RollbackToSavePoint()` / `.PopSavePoint()` | savepoint | nested rollback within a batch |
|  [12]   | `WriteBatchWithIndex.Get(k, db)` / `.NewIterator(…)` / `.CreateIteratorWithBase(baseIterator)` | read-your-writes | reads pending batch writes |

[ENTRYPOINT_SCOPE]: iterate, snapshot, compact, ingest
- rail: embedded-lsm-kv

`NewIterator(cf?, readOptions?)` opens a cursor; `Seek`/`SeekForPrev`/`SeekToFirst`/`SeekToLast` position it and `Next`/`Prev` walk it; `GetKeySpan()`/`GetValueSpan()` read the current cell zero-copy. `CreateSnapshot` + a `ReadOptions.Snapshot` binding gives a consistent multi-read view. `IngestExternalFiles` bulk-loads pre-built SSTs from `SstFileWriter`.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `RocksDb.NewIterator(cf?, readOptions?)`           | cursor open    | opens an ordered iterator                  |
|  [02]   | `Iterator.Seek(ReadOnlySpan<byte>)` / `.SeekForPrev(span)` | seek    | positions at ≥/≤ key (prefix-seek)        |
|  [03]   | `Iterator.SeekToFirst()` / `.SeekToLast()` / `.Next()` / `.Prev()` | walk | full/range ordered scan          |
|  [04]   | `Iterator.Valid()` / `.GetKeySpan()` / `.GetValueSpan()` | cursor read | zero-copy current-cell access      |
|  [05]   | `Iterator.Value<T>(ISpanDeserializer<T>)`          | typed read     | decodes the current value to `T`           |
|  [06]   | `RocksDb.CreateSnapshot()` (+ `ReadOptions.Snapshot`) | snapshot    | consistent point-in-time read view         |
|  [07]   | `RocksDb.Checkpoint().Save(dir, logSizeForFlush)`  | backup         | hard-linked consistent on-disk clone       |
|  [08]   | `RocksDb.CompactRange(start, limit, cf?)`          | maintenance    | forces compaction of a key range           |
|  [09]   | `new SstFileWriter(envOptions, ioOptions)` → `.Open(path)` → `.Put(k,v)` → `.Finish()` | bulk build | writes an external SST |
|  [10]   | `RocksDb.IngestExternalFiles(files, IngestExternalFileOptions, cf?)` | bulk load | atomically ingests pre-built SSTs |
|  [11]   | `RocksDb.GetLiveFilesMetadata()` / `GetProperty(name, cf?)` | introspection | per-SST metadata / engine stats |

[ENTRYPOINT_SCOPE]: column family and WAL changefeed
- rail: embedded-lsm-kv

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `RocksDb.CreateColumnFamily(ColumnFamilyOptions, name)` | CF lifecycle | creates a column family at runtime    |
|  [02]   | `RocksDb.GetColumnFamily(name)` / `TryGetColumnFamily(name, out h)` / `GetDefaultColumnFamily()` | CF lookup | resolves a family handle |
|  [03]   | `RocksDb.DropColumnFamily(name)`                   | CF lifecycle   | drops a column family                     |
|  [04]   | `RocksDb.GetLatestSequenceNumber()`                | WAL position   | latest WAL sequence number                |
|  [05]   | `RocksDb.GetUpdatesSince(sequenceNumber)`          | WAL changefeed | opens a `TransactionLogIterator` from a seq |
|  [06]   | `MergeOperators.Create(name, partialMergeFunc, fullMergeFunc)` | merge build | builds a custom merge operator |

## [04]-[IMPLEMENTATION_LAW]

[ROCKSDB_TOPOLOGY]:
- one native `librocksdb` core under `RocksDbSharp`; `NativeImport` P/Invoke-loads the RID-resolved dylib at first call. Every `RocksDb`/options/iterator type is a managed handle over a native pointer and is `IDisposable`.
- the options tuners live on the generic CRTP base `Options<T>` (`DbOptions : Options<DbOptions>`, `ColumnFamilyOptions : Options<ColumnFamilyOptions>`), so every `Set*`/`Optimize*` method returns the concrete options type for fluent chaining. The non-generic legacy `Options`/`OptionsHandle` types are the base handle, not the tuner surface.
- the key/value API is span-first: the `ReadOnlySpan<byte>` overloads of `Get`/`Put`/`Merge`/`Remove` and `Iterator.GetKeySpan()`/`GetValueSpan()` are zero-allocation; the `byte[]`/`string` overloads are convenience. The `string` overloads carry an optional `Encoding` (default UTF-8).
- `WriteBatch` is the atomic unit: all `Put`/`Merge`/`Delete`/`DeleteRange` in one batch apply atomically under one `Write` call. `WriteBatchWithIndex` adds a queryable index so a read inside the transaction sees its own pending writes (read-your-writes); savepoints give nested rollback.
- `Merge` is the LSM read-modify-write primitive: it enqueues an operand resolved by the installed `MergeOperator` at read/compaction time, avoiding a get-modify-put round trip for counters/sets/registers.
- consistency primitives are `Snapshot` (in-memory read view, cheap), `Checkpoint` (durable hard-linked clone, the backup form), and `GetUpdatesSince`→`TransactionLogIterator` (the WAL changefeed for CDC).

[LOCAL_ADMISSION]:
- RocksDB enters behind the same `Store/provisioning` store-backend vocabulary as every backend, as the embedded write-optimized log/KV class; the relational floor stays SQLite (`api-sqlite`) and the read-optimized MVCC engine stays LMDB (`LightningDB`). The three are distinct backend rows, never collapsed.
- a `[ValueObject]`/`[SmartEnum]` owner crosses into a RocksDB cell through the snapshot codec key projection and the span `Put`; the read decodes through `Get<T>(key, ISpanDeserializer<T>)` — no per-cell boxing, no hand-rolled byte framing.
- multi-entity layouts use column families (one family per logical stream/index), opened together via `ColumnFamilies`; the family handle is the operation target, never a key-prefix hack where a family is the right tool.
- atomic multi-key state transitions use `WriteBatch` (or `WriteBatchWithIndex` where read-your-writes is needed) under one `Write` — never a sequence of single `Put`s on the durable path.
- the WAL changefeed (`GetUpdatesSince`/`TransactionLogIterator`) is the admitted CDC tap for an embedded store; durable cross-process replication stays on the PostgreSQL tier and the messaging-protocol egress, NOT the fork's Raft/replication types.

[STACKING]:
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner projects to its physical key through `api-thinktecture-serialization`; the projected bytes write through the span `Put`/`Merge` and decode through `Get<T>(ISpanDeserializer<T>)`. The deserializer is the seam where the MessagePack/CBOR codec (`api-messagepack`/`api-cbor`) decodes the value span without a managed copy.
- compression alignment: `Compression.Zstd`/`Lz4` are the native-core block codecs applied per level via `SetCompressionPerLevel`/`SetMinLevelToCompress`; the standalone `ZstdSharp.Port`/`K4os.Compression.LZ4` codecs are orthogonal (snapshot/blob compression), so a RocksDB value is block-compressed once by the engine.
- merge-operator CRDT: a CRDT counter/register/set (the `Version` CRDT lane) installs a custom `MergeOperator` via `MergeOperators.Create` whose `FullMergeFunc`/`PartialMergeFunc` fold operands over a `ReadOnlySpan<byte>` — the merge resolution is native at compaction, the canonical form for high-write-rate counters rather than a read-modify-write loop.
- WAL→egress: `GetUpdatesSince` taps the WAL as a sequence-numbered changefeed; each batch is framed by the redaction codec (`api-redaction`) and published to the Kafka/RabbitMQ egress (`api-kafka`/`api-rabbitmq`) — the embedded store's CDC reuses the same egress lane as the relational tier, keyed by the WAL sequence number.
- backup residence: `Checkpoint.Save` produces a hard-linked consistent clone whose files (or a `SstFileWriter` export) land in the object-store residence (`api-objectstore`/`Minio`) via the `Store/blobstore` lane; `IngestExternalFiles` is the symmetric bulk-restore/bulk-load path.
- telemetry: `EnableStatistics()` + `GetProperty(name)` expose engine counters (memtable/compaction/cache stats) that feed the AppHost `telemetry` port as a metric stream, and `SetInfoLogLevel(InfoLogLevel)` routes native logs — not a bespoke logger.

[RAIL_LAW]:
- Package: `rocksdb` (`RocksDbSharp`)
- Owns: embedded LSM-tree write-optimized KV/log storage — column families, atomic `WriteBatch`/`WriteBatchWithIndex`, prefix-seekable iterators, `Snapshot`/`Checkpoint` consistency, custom merge/comparator/prefix/compaction hooks, bulk SST ingest, and the WAL changefeed
- Accept: `RocksDb.Open` with typed `ColumnFamilies`, span-first `Get`/`Put`/`Merge`/`Remove`, `WriteBatch` atomicity, `MergeOperators.Create` for read-modify-write, `Checkpoint`/`Snapshot` consistency, and `GetUpdatesSince` CDC
- Reject: per-cell byte framing where the span codec exists, single-`Put` sequences where a `WriteBatch` is atomic, a key-prefix hack where a column family is the tool, a read-modify-write loop where a merge operator is native, and the fork-only Raft/replication/WAL-inspector surface as if it were canonical RocksDB
