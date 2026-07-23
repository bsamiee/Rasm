# [RASM_PERSISTENCE_API_LIGHTNINGDB]

`LightningDB` binds LMDB — the memory-mapped single-writer/multi-reader B+tree — as the embedded read-optimized MVCC engine: a read transaction is a stable point-in-time snapshot and an `MDBValue` is a `ReadOnlySpan<byte>` window straight onto the mmap page, valid only inside that transaction. It owns the point-lookup, ordered range-scan, and dupsort secondary-index lane; `rocksdb` owns the write-amplified ingest and log lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LightningDB`
- package: `LightningDB` (MIT)
- assembly: `LightningDB`
- namespace: `LightningDB`, `LightningDB.Native`, `LightningDB.Comparers`
- asset: managed library carrying zero managed dependencies
- native: LMDB rides in-package per RID at `runtimes/<rid>/native/lmdb.dylib|.so|.dll`, resolved from the package
- rail: embedded-kv

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine roots, their configuration records, and the mmap value window

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]    | [CAPABILITY]                               |
| :-----: | :----------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `LightningEnvironment : IDisposable` | environment root | mmap file and reader table, one per path   |
|  [02]   | `EnvironmentConfiguration`           | config           | pre-`Open` map and slot caps               |
|  [03]   | `LightningTransaction : IDisposable` | transaction      | the MVCC isolation unit                    |
|  [04]   | `LightningDatabase : IDisposable`    | named database   | one B+tree keyspace inside the environment |
|  [05]   | `DatabaseConfiguration`              | config           | open flags and comparer wiring             |
|  [06]   | `LightningCursor : IDisposable`      | cursor           | positioned B+tree traversal                |
|  [07]   | `CursorEnumerable`                   | readonly struct  | allocation-free key/value walk             |
|  [08]   | `CursorDuplicateValuesEnumerable`    | readonly struct  | allocation-free dup-value walk             |
|  [09]   | `MDBValue`                           | readonly struct  | span window onto one mmap page             |
|  [10]   | `Stats`                              | introspection    | page size, B+tree depth, page/entry counts |
|  [11]   | `EnvironmentInfo`                    | introspection    | map size, last page number, last txn id    |
|  [12]   | `LightningVersionInfo`               | introspection    | native engine version                      |
|  [13]   | `LightningException : Exception`     | fault            | carries `StatusCode` from a failed call    |

- `EnvironmentConfiguration` carries `MapSize` `MaxDatabases` `MaxReaders` `AutoReduceMapSizeIn32BitProcess`, every field bound before `Open`.
- `DatabaseConfiguration` carries `Flags`, `CompareWith(IComparer<MDBValue>)` for key order, and `FindDuplicatesWith(IComparer<MDBValue>)` for dup order.

[VOCABULARY]: closed enums the call surface discriminates on.
- `MDBResultCode`: `Success` `KeyExist` `NotFound` `PageNotFound` `Corrupted` `Panic` `VersionMismatch` `Invalid` `MapFull` `DbsFull` `ReadersFull` `TLSFull` `TxnFull` `CursorFull` `PageFull` `MapResized` `Incompatible` `BadRSlot` `BadTxn` `BadValSize` `BadDBI` `Problem` `FileNotFound` `AccessDenied` `InvalidAccess` `InvalidData` `CurrentDirectory` `BadCommand` `OutOfPaper`
- `EnvironmentOpenFlags`: `None` `FixedMap` `NoSubDir` `NoSync` `ReadOnly` `NoMetaSync` `WriteMap` `MapAsync` `NoThreadLocalStorage` `NoLock` `NoReadAhead` `NoMemoryInitialization`
- `DatabaseOpenFlags`: `None` `ReverseKey` `DuplicatesSort` `IntegerKey` `DuplicatesFixed` `IntegerDuplicates` `ReverseDuplicates` `Create`
- `TransactionBeginFlags`: `None` `NoSync` `ReadOnly` `NoMetaSync`
- `PutOptions`: `None` `NoDuplicateData` `NoOverwrite` `ReserveSpace` `AppendData` `AppendDuplicateData`
- `CursorPutOptions`: `None` `Current` `NoDuplicateData` `NoOverwrite` `ReserveSpace` `AppendData` `AppendDuplicateData` `MultipleData`
- `CursorOperation`: `First` `FirstDuplicate` `GetBoth` `GetBothRange` `GetCurrent` `GetMultiple` `Last` `LastDuplicate` `Next` `NextDuplicate` `NextMultiple` `NextNoDuplicate` `Previous` `PreviousDuplicate` `PreviousNoDuplicate` `Set` `SetKey` `SetRange`
- `LightningTransactionState`: `Ready` `Reset` `Done` `Released`
- `UnixAccessMode`: `OwnerRead` `OwnerWrite` `OwnerExec` `GroupRead` `GroupWrite` `GroupExec` `OtherRead` `OtherWrite` `OtherExec` `Default`

[PUBLIC_TYPE_SCOPE]: `LightningDB.Comparers` — each strategy is a private-ctor singleton reached through its static `Instance`, fixed at create time and re-supplied identically on every open.

| [INDEX] | [FORWARD]                 | [REVERSE]                        | [ORDER]                            |
| :-----: | :------------------------ | :------------------------------- | :--------------------------------- |
|  [01]   | `BitwiseComparer`         | `ReverseBitwiseComparer`         | raw byte lexical, the LMDB default |
|  [02]   | `SignedIntegerComparer`   | `ReverseSignedIntegerComparer`   | 4/8-byte signed, negatives first   |
|  [03]   | `UnsignedIntegerComparer` | `ReverseUnsignedIntegerComparer` | 4/8-byte unsigned                  |
|  [04]   | `Utf8StringComparer`      | `ReverseUtf8StringComparer`      | ordinal UTF-8                      |
|  [05]   | `LengthComparer`          | `ReverseLengthComparer`          | length then content                |
|  [06]   | `GuidComparer`            | `ReverseGuidComparer`            | big-endian 16-byte pair            |
|  [07]   | `LengthOnlyComparer`      | —                                | length alone                       |
|  [08]   | `HashCodeComparer`        | —                                | hash digest of large values        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: environment lifecycle, caps, backup, and reader-table maintenance

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new LightningEnvironment(string, EnvironmentConfiguration)`            | ctor     | binds path, map size, DB/reader caps  |
|  [02]   | `LightningEnvironment.Open(EnvironmentOpenFlags, UnixAccessMode)`       | instance | maps the file and the reader table    |
|  [03]   | `LightningEnvironment.BeginTransaction(TransactionBeginFlags)`          | factory  | root txn; the `(parent, flags)` nests |
|  [04]   | `LightningEnvironment.CopyTo(string, bool) -> MDBResultCode`            | instance | online copy, `compact` reclaims pages |
|  [05]   | `LightningEnvironment.CopyToStream(FileStream, bool)`                   | instance | the same copy onto an open stream     |
|  [06]   | `LightningEnvironment.Flush(bool) -> MDBResultCode`                     | instance | forces msync of the map               |
|  [07]   | `LightningEnvironment.CheckStaleReaders() -> int`                       | instance | reclaims slots of dead readers        |
|  [08]   | `LightningEnvironment.GetFileStream() -> FileStream`                    | instance | the environment file handle           |
|  [09]   | `MapSize` / `MaxReaders` / `MaxDatabases` / `MaxKeySize`                | property | live caps; `MapSize` regrows the map  |
|  [10]   | `EnvironmentStats` / `Info` / `Flags` / `Path` / `IsOpened` / `Version` | property | engine state and open posture         |

[ENTRYPOINT_SCOPE]: transaction read/write, database lifecycle, and the value window. Every key and value parameter fans a `ReadOnlySpan<byte>` form beside its `byte[]` twin, and `LightningExtensions` owns every `tx.`-receiver row.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `LightningTransaction.OpenDatabase(string, DatabaseConfiguration, bool)` | instance | opens or creates a named sub-DB in the txn  |
|  [02]   | `LightningTransaction.CreateCursor(LightningDatabase)`                   | factory  | cursor bound to the db inside this txn      |
|  [03]   | `LightningTransaction.BeginTransaction(TransactionBeginFlags)`           | factory  | nested child transaction                    |
|  [04]   | `LightningTransaction.Get(db, ReadOnlySpan<byte>)`                       | instance | `(code, MDBValue, MDBValue)` zero-copy read |
|  [05]   | `LightningTransaction.Put(db, key, value, PutOptions)`                   | instance | writes one pair                             |
|  [06]   | `LightningTransaction.Delete(db, key)` / `Delete(db, key, value)`        | instance | removes a key or one dup value              |
|  [07]   | `LightningTransaction.Commit() -> MDBResultCode` / `Abort()`             | instance | atomic commit at fixed cost, or abort       |
|  [08]   | `LightningTransaction.Reset()` / `Renew() -> MDBResultCode`              | instance | parks then re-arms a read snapshot          |
|  [09]   | `LightningTransaction.CompareKeys(db, a, b)` / `CompareData(db, a, b)`   | instance | orders spans by the db's own comparer       |
|  [10]   | `LightningTransaction.TruncateDatabase(db)` / `DropDatabase(db)`         | instance | empties or deletes a sub-DB                 |
|  [11]   | `LightningTransaction.GetEntriesCount(db)` / `GetStats(db)`              | instance | entry count and B+tree stats                |
|  [12]   | `LightningDatabase.GetFlags(LightningTransaction)`                       | instance | the flags one keyspace was opened under     |
|  [13]   | `Name` / `Environment` / `IsOpened` / `DatabaseStats`                    | property | keyspace identity and live B+tree stats     |
|  [14]   | `MDBValue.AsSpan()` / `AsWritableSpan()`                                 | instance | the mmap window as a span                   |
|  [15]   | `MDBValue.Read<T>()` / `Cast<T>()` where `T : unmanaged`                 | instance | struct read or retype with no copy          |
|  [16]   | `MDBValue.CopyTo(Span<byte>)` / `CopyToNewArray()`                       | instance | lifts the value out of the txn lifetime     |
|  [17]   | `tx.TryGet(db, key, out byte[])` / `(…, Span<byte>, out int)`            | static   | try-read into caller storage                |
|  [18]   | `tx.TryGet(db, key, IBufferWriter<byte>)` / `(…, byte[] buffer)`         | static   | try-read into a pooled sink                 |
|  [19]   | `tx.ContainsKey(db, key)`                                                | static   | existence probe with no value copy          |
|  [20]   | `MDBResultCode.ThrowOnError()` / `ThrowOnReadError()`                    | static   | raises `LightningException` from a code     |

- `LightningTransaction`: `Dispose` without `Commit` aborts the transaction.

[ENTRYPOINT_SCOPE]: cursor seek, ordered walk, and dupsort traversal

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `LightningCursor.SetRange(ReadOnlySpan<byte>) -> MDBResultCode`    | instance | seeks the first key ≥ key                   |
|  [02]   | `LightningCursor.Set(key)` / `SetKey(key)`                         | instance | exact seek without or with the value read   |
|  [03]   | `LightningCursor.GetBoth(key, value)` / `GetBothRange(key, value)` | instance | exact or ≥ seek on a `(key, value)` pair    |
|  [04]   | `First()` / `Last()` / `Next()` / `Previous()`                     | instance | forward and backward B+tree walk            |
|  [05]   | `LightningCursor.GetCurrent()`                                     | instance | the entry under the cursor                  |
|  [06]   | `NextDuplicate()` / `PreviousDuplicate()`                          | instance | walks one key's dup set                     |
|  [07]   | `FirstDuplicate()` / `LastDuplicate()`                             | instance | ends of one key's dup set                   |
|  [08]   | `NextNoDuplicate()` / `PreviousNoDuplicate()`                      | instance | jumps to the adjacent distinct key          |
|  [09]   | `LightningCursor.Count(out long) -> MDBResultCode`                 | instance | dup values under the current key            |
|  [10]   | `LightningCursor.Put(key, value, CursorPutOptions)`                | instance | positioned write, `AppendData` bulk-loads   |
|  [11]   | `LightningCursor.Put(byte[] key, byte[][] values)`                 | instance | `DuplicatesFixed` batch of dup values       |
|  [12]   | `LightningCursor.Delete()` / `DeleteDuplicateData()`               | instance | drops the entry or the whole dup set        |
|  [13]   | `GetMultiple()` / `NextMultiple()`                                 | instance | page-sized `DuplicatesFixed` bulk read      |
|  [14]   | `LightningCursor.Renew()` / `Renew(LightningTransaction)`          | instance | rebinds the cursor to a renewed read txn    |
|  [15]   | `cursor.AsEnumerable()` / `cursor.AllValuesFor(key)`               | static   | struct-enumerator walk, dup set for one key |

- `LightningCursor.Put`: `CursorPutOptions` is positional, with no default; every walk member returns `(MDBResultCode, MDBValue key, MDBValue value)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One write transaction lives at a time and readers are unbounded, each a copy-on-write snapshot fixed at its start, so a reader never blocks a writer nor sees a later commit. That snapshot is the AS-OF primitive at the embedded tier, held only while a reconstruction reads it — a parked reader pins old pages and grows the file.
- `Reset` frees a read transaction's reader-table slot and keeps the handle; `Renew` re-arms it on the newest snapshot, amortizing setup across a read-heavy lane. `MaxReaders` caps live slots, `ReadersFull` reports exhaustion, `CheckStaleReaders` reclaims slots crashed readers orphaned.
- An `MDBValue` addresses mmap memory only its own transaction guarantees, so a decode, `CopyTo`, or `CopyToNewArray` lifts the bytes out before `Commit`, `Abort`, or `Dispose` releases the page; `Read<T>` and `Cast<T>` decode unmanaged shapes in place.
- `MapSize` binds the file ceiling before `Open`: a write past it returns `MapFull`, a foreign-process growth returns `MapResized`, and growth is a provisioning act, never a write-time realloc.
- `WriteMap`, `MapAsync`, `NoSync`, and `NoMetaSync` each trade fsync for throughput; omitting all four makes `Commit` fsync-durable, and durability posture is one environment row, never a per-write choice.
- `MaxDatabases` caps named sub-DBs, `Create` opens one B+tree per name, and a `null` name opens the root DB, each an independent keyspace in the one file. `IntegerKey` with an integer comparer yields a dense numeric index; `DuplicatesSort` with a dup comparer yields a sorted multi-value secondary index.

[STACKING]:
- `api-rocksdb`(`.api/api-rocksdb.md`): peer engine rows on the `Store/provisioning` backend axis — a read-heavy index or lookup lane selects LMDB, a write-amplified ingest or changefeed lane RocksDB, and a public `StoreOp` names neither.
- `api-messagepack`(`.api/api-messagepack.md`), `api-cbor`(`.api/api-cbor.md`), `api-thinktecture-serialization`(`.api/api-thinktecture-serialization.md`): the codec owns payload shape, LMDB the bytes — a read decodes off `MDBValue.AsSpan()` with no managed copy, and `TryGet(db, key, IBufferWriter<byte>)` feeds a pooled sink when the value outlives the transaction.
- `api-hashing`(`../../.api/api-hashing.md`): `XxHash128.HashToUInt128` mints the content key an `Element/codec` payload writes under, so the LMDB key is the content address and no second key vocabulary exists.
- `api-objectstore`(`.api/api-objectstore.md`): `CopyTo(path, compact: true)` yields the compacted point-in-time copy the `Version/recovery` leg ships to the object store, taken with writers running and folded into the `RecoveryFact` stream.
- within-lib: a `Query/columnar` keyset page lowers to `SetRange(afterKey)` then `Next()` over the order its comparer singleton fixed, never an offset skip; a dupsort walk serves the embedded secondary-index lookup and `GetMultiple`/`NextMultiple` drains fixed-width dup pages per read.

[LOCAL_ADMISSION]:
- One `LightningEnvironment` per store path lives for the process; every transaction, database handle, and cursor opens, works, and disposes inside it.
- Domain logic pins the `MDBResultCode`-returning core and lifts codes at one site: `NotFound` to the empty option, `KeyExist` to the write-once conflict, `MapFull`/`MapResized`/`ReadersFull` to typed engine faults.
- Key order enters through a `LightningDB.Comparers` singleton at `CompareWith`/`FindDuplicatesWith`, re-supplied identically on every open, because that comparer is the durable B+tree order.
- Keyspace partition rides named sub-DBs inside the one environment file.

[RAIL_LAW]:
- Package: `LightningDB`
- Owns: the embedded read-optimized MVCC B+tree — environment/database/transaction/cursor lifecycle, zero-copy span reads, dupsort secondary indexes, named keyspaces, and online compacting backup
- Accept: span-first `Get`/`Put`/`Delete`, the `MDBResultCode` rail, `MDBValue` bounded by its transaction, `Reset`/`Renew` snapshot reuse, cursor `SetRange` pagination, and `CopyTo(compact: true)` backup
- Reject: an `MDBValue` outliving its transaction, a throwing extension overload inside domain logic, a write-time map regrow, one file per keyspace, a hand-rolled byte comparer beside the comparer singletons, and LMDB on a write-amplified lane
