# [RASM_PERSISTENCE_API_LIGHTNINGDB]

`LightningDB` is the managed binding over LMDB — the embedded memory-mapped B+tree read-optimized MVCC engine: a single-writer/multi-reader ACID store with zero-copy reads straight out of the mmap, named sub-databases, cursors with dupsort multi-value keys, and a fixed-cost commit. It is the read-optimized half of the `[EMBEDDED_KV]` pair the `[STORE_BACKENDS]` cluster admits beside `rocksdb` (the write-optimized LSM half): LightningDB owns the point-lookup / range-scan / index lane where `rocksdb` owns the write-amplified ingest/log lane. The `MDBValue` `ReadOnlySpan<byte>` zero-copy read is the snapshot-codec boundary — the bytes a `Element/codec` content-addressed payload or a `Query/cache` L-tier entry deserializes from the mmap with no managed copy, under one read transaction that is a stable point-in-time MVCC snapshot.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LightningDB`
- package: `LightningDB`
- version: `0.22.0`
- assembly: `LightningDB`
- namespace: `LightningDB`, `LightningDB.Native`, `LightningDB.Comparers`
- license: MIT (`<license type="file">LICENSE</license>`)
- target framework: `net10.0` asset on the `net10.0` floor (package ships `net10.0`/`net9.0`/`net8.0`/`netstandard2.0`; the `net10.0` lib binds directly and carries zero managed dependencies)
- native: LMDB ships per-RID under `runtimes/<rid>/native/lmdb.dylib|.so|.dll`; `runtimes/osx-arm64/native/lmdb.dylib` is present (also linux-arm64/x64, win-x64/arm64, android, ios, browser-wasm) — the embedded engine is in-package, no system LMDB
- asset: runtime library + native dylib
- rail: embedded-kv

## [02]-[PUBLIC_TYPES]

[ENGINE_TYPES]: environment, database, transaction, cursor
- rail: embedded-kv

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------- | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `LightningEnvironment : IDisposable`      | environment root   | the mmap file + reader table; long-lived, one per store path           |
|  [02]   | `EnvironmentConfiguration`                | config             | `MapSize` (long), `MaxDatabases` (int), `MaxReaders` (int) before `Open` |
|  [03]   | `LightningDatabase : IDisposable`         | named database     | one B+tree (named sub-DB) opened inside a transaction                   |
|  [04]   | `DatabaseConfiguration`                   | config             | `Flags` (`DatabaseOpenFlags`), `CompareWith`/`FindDuplicatesWith` (`IComparer<MDBValue>`) |
|  [05]   | `LightningTransaction : IDisposable`      | transaction        | the unit of MVCC isolation; read txns are snapshots, write txns serialize |
|  [06]   | `LightningCursor : IDisposable`           | cursor             | positioned B+tree traversal — range scan, dupsort multi-value walk      |
|  [07]   | `MDBValue` (readonly struct)              | zero-copy span     | wraps a key/value as `ReadOnlySpan<byte>` straight from the mmap        |
|  [08]   | `LightningVersionInfo` / `EnvironmentInfo` / `Stats` | introspection | engine version, map/reader usage, B+tree depth & page counts          |
|  [09]   | `LightningException : Exception`          | fault              | carries the `MDBResultCode` of a failed native call                    |

[VOCABULARY_TYPES]: the closed flag / result / state enums
- rail: embedded-kv

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [CAPABILITY]                                                          |
| :-----: | :--------------------------- | :----------------- | :-------------------------------------------------------------------- |
|  [01]   | `MDBResultCode`              | result enum        | the native return code — `Success`, `NotFound`, `KeyExist`, `MapFull`, `MapResized`, … |
|  [02]   | `EnvironmentOpenFlags`       | flags enum         | `NoSync`/`NoMetaSync`/`WriteMap`/`MapAsync`/`NoSubDir`/`ReadOnly`/`NoLock` durability & layout |
|  [03]   | `DatabaseOpenFlags`          | flags enum         | `Create`/`DuplicatesSort`/`IntegerKey`/`ReverseKey`/`DuplicatesFixed`/`IntegerDuplicates` |
|  [04]   | `TransactionBeginFlags`      | flags enum         | `ReadOnly`/`NoSync`/`NoMetaSync` per-transaction posture             |
|  [05]   | `PutOptions`                 | put flags          | `NoOverwrite`/`NoDuplicateData`/`AppendData`/`AppendDuplicateData`/`ReserveSpace` |
|  [06]   | `CursorOperation`            | cursor op enum     | `First`/`Last`/`Next`/`Prev`/`Set`/`SetRange`/`GetCurrent`/`NextDuplicate`/`NextNoDuplicate` |
|  [07]   | `CursorPutOptions` / `CursorDeleteOption` | cursor flags | cursor-positioned write/delete variants (`NoDupData`, `MultipleData`) |
|  [08]   | `LightningTransactionState`  | state enum         | `Ready`/`Done`/`Released` lifecycle of a transaction                  |
|  [09]   | `EnvironmentCopyFlags`       | copy flags         | `Compact` — the page-reclaiming variant the `CopyTo(path, compact: true)` backup maps to |
|  [10]   | `UnixAccessMode : uint`      | mode               | POSIX file mode for the created mmap file                            |

[COMPARERS]: `LightningDB.Comparers` — the key-ordering and dupsort strategies
- rail: embedded-kv

`IComparer<MDBValue>` implementations wired through `DatabaseConfiguration.CompareWith` (key order) and `FindDuplicatesWith` (dupsort value order): `BitwiseComparer`/`ReverseBitwiseComparer` (raw byte lexical), `SignedIntegerComparer`/`UnsignedIntegerComparer` (+ `Reverse*` — native integer-key order pairing `DatabaseOpenFlags.IntegerKey`), `Utf8StringComparer`/`ReverseUtf8StringComparer`, `LengthComparer`/`LengthOnlyComparer`/`ReverseLengthComparer`, `GuidComparer`/`ReverseGuidComparer`, `HashCodeComparer`. The comparer is the durable key-order contract — it is fixed at create time and re-supplied identically on every open, never inferred.

## [03]-[ENTRYPOINTS]

[LIFECYCLE]: open the environment, open a named database in a txn
- rail: embedded-kv

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `new LightningEnvironment(path, EnvironmentConfiguration?)`        | construct      | binds the mmap path + map size + max DBs/readers      |
|  [02]   | `LightningEnvironment.Open(EnvironmentOpenFlags = None, UnixAccessMode = Default)` | open | maps the file and the reader table                    |
|  [03]   | `LightningEnvironment.BeginTransaction(TransactionBeginFlags = None)` / `BeginTransaction(parent, flags)` | txn factory | starts a read (snapshot) or write (serialized) txn; nested via parent |
|  [04]   | `LightningTransaction.OpenDatabase(string name, DatabaseConfiguration configuration, bool closeOnDispose = false)` | db open | opens/creates a named sub-DB inside the txn (overloads drop the name and/or config; `OpenDatabase(bool)` = the unnamed root DB) |
|  [05]   | `LightningTransaction.CreateCursor(LightningDatabase)`            | cursor factory | a cursor bound to the db within the txn               |
|  [06]   | `LightningEnvironment.CopyTo(string path, bool compact = false)` / `CopyToStream(FileStream, compact)` | hot backup | consistent online copy (`compact: true` reclaims free pages) — the `Version/recovery` backup leg |
|  [07]   | `LightningEnvironment.Flush(bool force)` / `MapSize` / `MaxReaders` / `MaxDatabases` / `IsOpened` | maintenance | force msync; grow the map; reader-slot cap; sub-DB cap |

[READ_WRITE]: transaction get/put/delete and the result-code rail
- rail: embedded-kv

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `LightningTransaction.Get(db, ReadOnlySpan<byte> key)` (also `byte[]`) | read | returns `(MDBResultCode resultCode, MDBValue key, MDBValue value)` — zero-copy; `NotFound` code = absent |
|  [02]   | `LightningTransaction.Put(db, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, PutOptions = None)` | write | `NoOverwrite` = write-once, returns `MDBResultCode.KeyExist` on conflict |
|  [03]   | `LightningTransaction.Delete(db, key)` / `Delete(db, key, value)` | delete       | removes a key (the `(key,value)` overload removes one dup value)  |
|  [04]   | `tx.ContainsKey(db, key)` / `tx.TryGet(db, key, out byte[]? value)` (`LightningExtensions`) | read | existence probe / try-pattern materializing the value — the boundary lift over `Get`'s `NotFound` code |
|  [05]   | `LightningTransaction.Commit()` → `MDBResultCode` / `Abort()` (void) | finalize | atomic commit (fixed cost) / explicit abort; `Dispose` without `Commit` also aborts |
|  [06]   | `LightningTransaction.Reset()` (void) / `Renew()` → `MDBResultCode` | read reuse | park then re-arm a read txn to amortize snapshot setup across reads (`mdb_txn_reset`/`mdb_txn_renew`) |
|  [07]   | `LightningTransaction.TruncateDatabase(db)` / `DropDatabase(db)` / `GetEntriesCount(db)` / `GetStats(db)` | bulk/introspect | empty / delete a sub-DB; entry count; B+tree stats |

[CURSOR_SCAN]: positioned range scan and dupsort multi-value traversal — `LightningCursor`
- rail: embedded-kv

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `cursor.SetRange(ReadOnlySpan<byte> key)` → `MDBResultCode`  | seek           | positions at the first key ≥ key — the keyset-pagination seek      |
|  [02]   | `cursor.Set(key)` → `MDBResultCode` / `cursor.SetKey(key)` → `(code, key, value)` | seek | positions exactly at key (without / with the value read)     |
|  [03]   | `cursor.GetBoth(key, value)` / `GetBothRange(key, value)`  | dupsort seek   | positions at an exact / ≥ `(key,value)` pair within a dup set      |
|  [04]   | `cursor.First()` / `Last()` / `Next()` / `Previous()` → `(code, key, value)` | walk | forward/backward B+tree traversal                          |
|  [05]   | `cursor.GetCurrent()` → `(code, MDBValue key, MDBValue value)` | read        | the entry at the cursor                                            |
|  [06]   | `cursor.NextDuplicate()` / `PreviousDuplicate()` / `FirstDuplicate()` / `LastDuplicate()` | dupsort walk | iterate the multi-value set under one key (`DuplicatesSort`) |
|  [07]   | `cursor.NextNoDuplicate()` / `PreviousNoDuplicate()`       | dupsort skip   | jump to the next/prev distinct key, skipping remaining dups        |
|  [08]   | `cursor.Count(out int value)` → `MDBResultCode`            | dupsort count  | number of dup values under the current key                         |
|  [09]   | `cursor.Put(key, value, CursorPutOptions)` / `Put(key, byte[][] values)` | write | cursor-positioned write (`AppendData` for sorted bulk load; the `byte[][]` form for `DuplicatesFixed` batch) |
|  [10]   | `cursor.Delete()` / `cursor.DeleteDuplicateData()`         | delete         | delete the current entry / all dup values under the current key    |
|  [11]   | `cursor.GetMultiple()` / `cursor.NextMultiple()` → `(code, key, value)` | dupsort bulk read | the `DuplicatesFixed` bulk read — many fixed-width dup values in one page-sized `MDBValue` (the read counterpart of `Put(key, byte[][])`) |
|  [12]   | `cursor.AsEnumerable()` / `cursor.AllValuesFor(key)` (`LightningExtensions`) | enumerate | the cursor as `IEnumerable<(MDBValue, MDBValue)>` / the dup-value set for a key |

`LightningExtensions` also folds the raw `(MDBResultCode, …)` tuples into throw-on-error helpers (`ThrowOnError`/`ThrowOnReadError`) and the `TryGet`/`ContainsKey` boundary lifts — pin the `MDBResultCode`-returning core and lift it into the typed rail, never the throwing overload in domain logic.

## [04]-[IMPLEMENTATION_LAW]

[MVCC_SNAPSHOT]:
- LMDB is single-writer / multi-reader: at most one write transaction is live at a time (writers serialize), while read transactions are unbounded and each is a stable copy-on-write snapshot of the B+tree as of its start — a long read never blocks a write and never sees a later commit. This IS the `Version/timetravel` AS-OF primitive at the embedded tier: a read txn is a consistent point-in-time view, held open exactly as long as the reconstruction needs and no longer (a stale read txn pins old pages and grows the file).
- `Reset()` parks a read transaction (releasing its reader-table slot while keeping the handle) and `Renew()` re-arms it against the latest snapshot, so a read-heavy lane amortizes the snapshot-handle cost across many gets instead of begin/commit per read (the LMDB `mdb_txn_reset`/`mdb_txn_renew` reader-slot reuse, surfaced here as the `Reset`/`Renew` pair). `Abort()` is the explicit non-committing close (a bare `Dispose` aborts too).
- `MaxReaders` caps concurrent read slots; exceeding it returns `MDBResultCode.ReadersFull`, and a crashed reader leaves a stale slot until `LightningEnvironment` clears it — the reader table is engine state the `Store/provisioning` rule set probes via `EnvironmentInfo`/`Stats`.

[ZERO_COPY_AND_MAP]:
- `MDBValue` is a `ReadOnlySpan<byte>` directly over the mmap page — valid ONLY for the lifetime of its transaction. A value read in a read txn must be deserialized (the snapshot codec) or copied before the txn ends; escaping a `MDBValue` past `Commit`/`Abort`/`Dispose` is a use-after-free. The zero-copy read is the whole performance argument and the whole discipline.
- `MapSize` is the maximum file size set before `Open`; a write that exceeds it returns `MDBResultCode.MapFull`. Growth is an explicit `MapSize` increase (or `MapResized` re-open after another process grew it) — never an automatic realloc. Size the map at provisioning, not at write time.
- `WriteMap` + `MapAsync` trade durability for write throughput (the page cache is written async); `NoSync`/`NoMetaSync` skip the fsync entirely. The durable default omits all three so a `Commit` is fsync-durable — the durability posture is an `EnvironmentOpenFlags` row on the `Store/provisioning` engine axis, never a per-write decision.

[NAMED_DATABASES]:
- `MaxDatabases` (set before `Open`) caps the number of named sub-DBs; `OpenDatabase(name, …)` with `DatabaseOpenFlags.Create` opens one B+tree, and `OpenDatabase(null, …)` opens the unnamed root DB. Each named DB is an independent keyspace within the one environment/file — the multi-keyspace layout a `Store/provisioning` embedded-KV row partitions by concern, never one DB per file.
- `IntegerKey` + `(Un)SignedIntegerComparer` gives native-endian integer key order (dense numeric ids); `DuplicatesSort` + `FindDuplicatesWith` makes a key carry a sorted multi-value set — the secondary-index shape (one indexed value → many primary keys) without a second table.

[FAULT_RAIL]:
- Every native call returns an `MDBResultCode`; the typed core methods return it (or a tuple carrying it) rather than throwing. `MDBResultCode.Success` is the only non-fault; `NotFound` → `Option.None` at the boundary, `KeyExist` → the write-once/`NoOverwrite` conflict, `MapFull`/`MapResized`/`ReadersFull` → typed engine faults the `StoreFault` rail lifts at one site. `LightningException` carries the code only for the throwing extension overloads — pin the code-returning core.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- the read-optimized half of `[EMBEDDED_KV]`: `LightningDB` (LMDB B+tree, point-lookup & range-scan & MVCC-snapshot read) and `rocksdb` (LSM, write-amplified ingest/log/merge) are two engine rows on the `Store/provisioning` axis, not a choice — a profile selects LMDB for the read-heavy index/lookup lane and RocksDB for the write-heavy ingest/changefeed lane, and a public `StoreOp` never names either package.
- snapshot codec at the mmap boundary: a `Element/codec` content-addressed payload or a `Query/cache` entry is the `MessagePack`/`api-thinktecture-serialization`/`System.Formats.Cbor` bytes written as a `MDBValue` and deserialized zero-copy from the read-txn span — LMDB stores the bytes, the codec owns the shape, the content key (`XxHash128` via `System.IO.Hashing`) is the LMDB key.
- keyset pagination at the cursor: a `Query/columnar` keyset page lowers to `cursor.SetRange(afterKey)` + `Next()` over the B+tree order the `IComparer<MDBValue>` fixed — the engine-native ordered scan, never an offset skip; the dupsort cursor walk is the embedded secondary-index lookup.
- AS-OF read at the embedded tier: a read transaction is the `Version/timetravel` point-in-time snapshot for an embedded store — held for the reconstruction, released immediately after, so the MVCC reader table stays shallow.
- hot backup into recovery: `CopyTo(path, compact: true)` (or `CopyToStream(stream, compact: true)`) is the consistent online backup the `Version/recovery` per-engine backup leg invokes, producing a compacted point-in-time copy without stopping writers, folded into the `RecoveryFact` stream proving RPO.

[RAIL_LAW]:
- Packages: `LightningDB` (in-package native LMDB)
- Owns: the embedded read-optimized MVCC B+tree KV/index engine — environment/db/txn/cursor lifecycle, zero-copy reads, dupsort secondary indexes, named keyspaces, online compacting backup
- Accept: one long-lived `LightningEnvironment` per store path, the `MDBResultCode` rail, `MDBValue` spans bounded by their transaction, the `IComparer<MDBValue>` as the fixed durable key order, `MapSize`/durability flags as `Store/provisioning` rows
- Reject: a `MDBValue` escaping its transaction (use-after-free), a throwing extension overload in domain logic, an automatic map regrow at write time, one named DB per file, a second snapshot/content-key vocabulary beside the settled codec/`XxHash128` owners, choosing LMDB where the lane is write-amplified (that is the `rocksdb` row)
