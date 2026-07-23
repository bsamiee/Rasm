# [RASM_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` owns the staging rail's payload substrate: pooled rentals released deterministically, dense plane views over contiguous storage, and struct-action partitioning that keeps a parallel kernel allocation-free. Every shape it mints stops at the staging boundary — codec, cache, and IO rails consume its buffers, and domain vocabulary names its own value types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`
- package: `CommunityToolkit.HighPerformance` (MIT)
- assembly: `CommunityToolkit.HighPerformance`
- namespaces: `CommunityToolkit.HighPerformance`, `.Buffers`, `.Enumerables`, `.Helpers`
- asset: runtime library binding the `net8.0` ABI
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pooled ownership, plane views, and ref carriers

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]       | [CAPABILITY]                           |
| :-----: | :------------------------- | :------------------ | :------------------------------------- |
|  [01]   | `Span2D<T>`                | readonly ref struct | writes a dense plane in place          |
|  [02]   | `ReadOnlySpan2D<T>`        | readonly ref struct | reads a dense plane                    |
|  [03]   | `Memory2D<T>`              | readonly struct     | carries an owned plane across an await |
|  [04]   | `ReadOnlyMemory2D<T>`      | readonly struct     | carries a read-only owned plane        |
|  [05]   | `Ref<T>`                   | readonly ref struct | preserves a mutable reference          |
|  [06]   | `ReadOnlyRef<T>`           | readonly ref struct | preserves a read-only reference        |
|  [07]   | `NullableRef<T>`           | readonly ref struct | carries a present-or-absent ref        |
|  [08]   | `NullableReadOnlyRef<T>`   | readonly ref struct | carries a present-or-absent read ref   |
|  [09]   | `Box<T>`                   | sealed class        | types a boxed struct without re-boxing |
|  [10]   | `MemoryOwner<T>`           | sealed class        | owns a heap-safe pooled rental         |
|  [11]   | `SpanOwner<T>`             | readonly ref struct | owns a stack-scoped pooled rental      |
|  [12]   | `ArrayPoolBufferWriter<T>` | sealed class        | grows a pooled write buffer            |
|  [13]   | `MemoryBufferWriter<T>`    | sealed class        | writes into fixed caller memory        |
|  [14]   | `IBuffer<T>`               | interface           | reads committed writer state           |
|  [15]   | `AllocationMode`           | enum                | selects rental clearing                |
|  [16]   | `StringPool`               | sealed class        | interns staged text by span            |

[PUBLIC_TYPE_SCOPE]: enumeration, partition, and helper roots

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [CAPABILITY]                            |
| :-----: | :-------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `SpanTokenizer<T>`          | ref struct          | splits a mutable span on a separator    |
|  [02]   | `ReadOnlySpanTokenizer<T>`  | ref struct          | splits a read-only span on a separator  |
|  [03]   | `SpanEnumerable<T>`         | ref struct          | enumerates a span with its index        |
|  [04]   | `ReadOnlySpanEnumerable<T>` | ref struct          | enumerates a read span with its index   |
|  [05]   | `RefEnumerable<T>`          | readonly ref struct | walks a strided row or column by ref    |
|  [06]   | `ReadOnlyRefEnumerable<T>`  | readonly ref struct | walks a strided view by read ref        |
|  [07]   | `ParallelHelper`            | static class        | partitions index and item work          |
|  [08]   | `IAction`                   | interface           | defines one 1D index slot               |
|  [09]   | `IAction2D`                 | interface           | defines one 2D index slot               |
|  [10]   | `IInAction<T>`              | interface           | defines read-only item work             |
|  [11]   | `IRefAction<T>`             | interface           | defines mutable item work               |
|  [12]   | `BitHelper`                 | static class        | packs and reads word-level bit flags    |
|  [13]   | `ObjectMarshal`             | static class        | addresses object payload by byte offset |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pooled rentals and interned text

`MemoryOwner<T>` and `SpanOwner<T>` share one `Allocate(int[, ArrayPool<T>][, AllocationMode])` family, mint a zero-length owner through the static `.Empty`, return the rental on `Dispose`, and each exposes `DangerousGetArray() -> ArraySegment<T>` beside `DangerousGetReference() -> ref T`. `AllocationMode.Clear` zeroes on rent where `AllocationMode.Default` hands back pool-dirty storage, and `StringPool.GetOrAdd` interns from a `string`, a `ReadOnlySpan<char>`, or a `ReadOnlySpan<byte>` with its `Encoding`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------ | :-------- | :-------------------------------- |
|  [01]   | `MemoryOwner<T>.Allocate(int, ArrayPool<T>, AllocationMode)`              | factory   | rent heap-safe pooled memory      |
|  [02]   | `SpanOwner<T>.Allocate(int, ArrayPool<T>, AllocationMode)`                | factory   | rent a stack-scoped pooled span   |
|  [03]   | `MemoryOwner<T>.Memory`                                                   | property  | owned `Memory<T>` over the window |
|  [04]   | `MemoryOwner<T>.Span`                                                     | property  | owned `Span<T>` over the window   |
|  [05]   | `MemoryOwner<T>.Slice(int, int)`                                          | instance  | re-window without a second rental |
|  [06]   | `ArrayPoolExtensions.Resize<T>(ArrayPool<T>, ref T[], int, bool)`         | extension | resize a rented array in place    |
|  [07]   | `ArrayPoolExtensions.EnsureCapacity<T>(ArrayPool<T>, ref T[], int, bool)` | extension | grow an undersized rental         |
|  [08]   | `StringPool.Shared`                                                       | property  | process-wide default pool         |
|  [09]   | `StringPool(int)`                                                         | ctor      | pool sized to a staging budget    |
|  [10]   | `StringPool.GetOrAdd(ReadOnlySpan<char>)`                                 | instance  | intern text without allocating    |
|  [11]   | `StringPool.TryGet(ReadOnlySpan<char>, out string)`                       | instance  | probe without interning           |
|  [12]   | `StringPool.Add(string)`                                                  | instance  | seed a known value                |
|  [13]   | `StringPool.Reset()`                                                      | instance  | drop every interned entry         |

[ENTRYPOINT_SCOPE]: buffer-writer emit

`ArrayPoolBufferWriter<T>` grows its pooled rental on demand where `MemoryBufferWriter<T>` binds fixed caller memory; both implement `IBuffer<T> : IBufferWriter<T>`, so one loop opens a region, `Advance` commits it, and the committed prefix reads back with no copy.

| [INDEX] | [SURFACE]                                                                | [SHAPE]   | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------- | :-------- | :------------------------------- |
|  [01]   | `ArrayPoolBufferWriter<T>(ArrayPool<T>, int)`                            | ctor      | bind a pool and initial capacity |
|  [02]   | `MemoryBufferWriter<T>(Memory<T>)`                                       | ctor      | bind fixed caller memory         |
|  [03]   | `IBuffer<T>.GetSpan(int) -> Span<T>`                                     | instance  | open a writable region           |
|  [04]   | `IBuffer<T>.GetMemory(int) -> Memory<T>`                                 | instance  | open a writable region           |
|  [05]   | `IBuffer<T>.Advance(int)`                                                | instance  | commit written elements          |
|  [06]   | `IBuffer<T>.WrittenMemory -> ReadOnlyMemory<T>`                          | property  | read the committed payload       |
|  [07]   | `IBuffer<T>.WrittenSpan -> ReadOnlySpan<T>`                              | property  | read the committed payload       |
|  [08]   | `IBuffer<T>.WrittenCount`                                                | property  | read the committed count         |
|  [09]   | `IBuffer<T>.Capacity`                                                    | property  | read total capacity              |
|  [10]   | `IBuffer<T>.FreeCapacity`                                                | property  | read remaining capacity          |
|  [11]   | `IBuffer<T>.Clear()`                                                     | instance  | reset the write head             |
|  [12]   | `ArrayPoolBufferWriter<T>.DangerousGetArray() -> ArraySegment<T>`        | instance  | expose the rented array          |
|  [13]   | `IBufferWriterExtensions.Write<T>(IBufferWriter<byte>, ReadOnlySpan<T>)` | extension | copy unmanaged span bytes        |
|  [14]   | `IBufferWriterExtensions.Write<T>(IBufferWriter<byte>, T)`               | extension | copy one unmanaged value         |

[ENTRYPOINT_SCOPE]: plane projection and span reinterpretation

`T[,]` projects whole or windowed `(row, column, height, width)` planes, `T[,,]` adds a leading `(depth)` selector, and `Memory<T>` with `Span<T>` project `(height, width)` or padded `(offset, height, width, pitch)` planes. `Memory2D<T>` mirrors the `Span2D<T>` window, copy, and contiguity-probe family over owned storage and hands the plane over as `.Span`; `AsBytes` and `Cast` admit unmanaged elements only, so a reinterpreted payload crosses a process boundary through its codec.

| [INDEX] | [SURFACE]                                    | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :------------------------------------------- | :-------- | :--------------------------------- |
|  [01]   | `AsSpan2D<T>`                                | extension | project a plane view               |
|  [02]   | `AsMemory2D<T>`                              | extension | project an owned plane             |
|  [03]   | `ArrayExtensions.GetRow<T>(T[,], int)`       | extension | walk one row as `RefEnumerable<T>` |
|  [04]   | `ArrayExtensions.GetColumn<T>(T[,], int)`    | extension | walk one column by ref             |
|  [05]   | `ArrayExtensions.GetRowSpan<T>(T[,], int)`   | extension | contiguous row span                |
|  [06]   | `ArrayExtensions.GetRowMemory<T>(T[,], int)` | extension | contiguous row memory              |
|  [07]   | `Span2D<T>.Slice(int, int, int, int)`        | instance  | window a sub-plane                 |
|  [08]   | `Span2D<T>.GetRowSpan(int)`                  | instance  | contiguous row of a plane          |
|  [09]   | `Span2D<T>.TryGetSpan(out Span<T>)`          | instance  | probe plane contiguity             |
|  [10]   | `Span2D<T>.CopyTo(Span2D<T>)`                | instance  | blit plane to plane                |
|  [11]   | `Span2D<T>.Fill(T)`                          | instance  | write one value across the plane   |
|  [12]   | `Span2D<T>.Clear()`                          | instance  | zero the plane                     |
|  [13]   | `Memory2D<T>.Span -> Span2D<T>`              | property  | address the owned plane            |
|  [14]   | `DangerousGetReference<T>`                   | extension | root reference of a span or array  |
|  [15]   | `DangerousGetReferenceAt<T>`                 | extension | indexed reference, no bounds check |
|  [16]   | `AsBytes<T>`                                 | extension | reinterpret unmanaged storage      |
|  [17]   | `Cast<TFrom, TTo>`                           | extension | reinterpret element width          |
|  [18]   | `Tokenize`                                   | extension | split on a separator value         |
|  [19]   | `Enumerate`                                  | extension | index-paired or by-ref enumeration |

[ENTRYPOINT_SCOPE]: `AsStream` byte bridge

`AsStream` projects an already-materialized byte payload with no intermediate array, and the `IMemoryOwner<byte>` form transfers disposal to the returned stream.

| [INDEX] | [SURFACE]                                                               | [SHAPE]   | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------------------- | :-------- | :--------------------------- |
|  [01]   | `MemoryExtensions.AsStream(Memory<byte>)`                               | extension | duplex stream over memory    |
|  [02]   | `ReadOnlyMemoryExtensions.AsStream(ReadOnlyMemory<byte>)`               | extension | read stream over memory      |
|  [03]   | `ReadOnlySequenceExtensions.AsStream(ReadOnlySequence<byte>)`           | extension | stream a segmented payload   |
|  [04]   | `IMemoryOwnerExtensions.AsStream(IMemoryOwner<byte>)`                   | extension | stream that owns disposal    |
|  [05]   | `ArrayPoolBufferWriterExtensions.AsStream(ArrayPoolBufferWriter<byte>)` | extension | write stream over a writer   |
|  [06]   | `IBufferWriterExtensions.AsStream(IBufferWriter<byte>)`                 | extension | write stream over any writer |
|  [07]   | `StreamExtensions.Read<T>(Stream) -> T`                                 | extension | read one unmanaged value     |
|  [08]   | `StreamExtensions.Write<T>(Stream, in T)`                               | extension | write one unmanaged value    |

[ENTRYPOINT_SCOPE]: parallel partition and contention

Every `ParallelHelper` entry takes a `struct TAction` — `default`-constructed per partition in the no-seed form, or copied from an `in TAction` carrying captured state — so the invoker allocates nothing and inlines. `minimumActionsPerThread` lower-bounds per-thread work, parallelism clamps to `Environment.ProcessorCount`, and a single partition invokes inline on the calling thread.

| [INDEX] | [SURFACE]                                                     | [SHAPE]   | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------ | :-------- | :------------------------ |
|  [01]   | `ParallelHelper.For<TAction>(int, int)`                       | static    | partition an index range  |
|  [02]   | `ParallelHelper.For<TAction>(Range)`                          | static    | partition a `Range`       |
|  [03]   | `ParallelHelper.For2D<TAction>(int, int, int, int)`           | static    | partition explicit bounds |
|  [04]   | `ParallelHelper.For2D<TAction>(Rectangle)`                    | static    | partition a rectangle     |
|  [05]   | `ParallelHelper.For2D<TAction>(Range, Range)`                 | static    | partition paired ranges   |
|  [06]   | `ParallelHelper.ForEach<TItem, TAction>(Memory<T>)`           | static    | mutate items by ref       |
|  [07]   | `ParallelHelper.ForEach<TItem, TAction>(ReadOnlyMemory<T>)`   | static    | read items by ref         |
|  [08]   | `ParallelHelper.ForEach<TItem, TAction>(Memory2D<T>)`         | static    | mutate a plane by ref     |
|  [09]   | `ParallelHelper.ForEach<TItem, TAction>(ReadOnlyMemory2D<T>)` | static    | read a plane by ref       |
|  [10]   | `SpinLockExtensions.Enter(ref SpinLock)`                      | extension | `using`-scoped spin lock  |

[ENTRYPOINT_SCOPE]: word-level bit and mask operations

Every `BitHelper` operation carries a `uint` and a `ulong` overload; the `ref` form mutates the word in place and the value form returns the updated word.

| [INDEX] | [SURFACE]                                            | [SHAPE]   | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------- | :-------- | :---------------------------- |
|  [01]   | `BitHelper.HasFlag(uint, int)`                       | static    | read one bit                  |
|  [02]   | `BitHelper.HasZeroByte(uint)`                        | static    | detect a zero byte in a word  |
|  [03]   | `BitHelper.HasByteEqualTo(uint, byte)`               | static    | detect a byte value in a word |
|  [04]   | `BitHelper.HasLookupFlag(uint, int, int)`            | static    | read a bit from a lookup word |
|  [05]   | `BitHelper.SetFlag(ref uint, int, bool)`             | static    | set one bit in place          |
|  [06]   | `BitHelper.SetFlag(uint, int, bool) -> uint`         | static    | set one bit into a new word   |
|  [07]   | `BitHelper.ExtractRange(uint, byte, byte)`           | static    | read a bit range              |
|  [08]   | `BitHelper.SetRange(ref uint, byte, byte, uint)`     | static    | write a bit range in place    |
|  [09]   | `BitHelper.SetRange(uint, byte, byte, uint) -> uint` | static    | write a range into a new word |
|  [10]   | `BoolExtensions.ToBitwiseMask32(bool) -> int`        | extension | branchless all-ones mask      |
|  [11]   | `BoolExtensions.ToByte(bool) -> byte`                | extension | branchless zero-or-one byte   |

[ENTRYPOINT_SCOPE]: reference access into boxed, nullable, and object storage

| [INDEX] | [SURFACE]                                                           | [SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------ | :-------- | :---------------------------------------- |
|  [01]   | `Box<T>.GetFrom(object) -> Box<T>`                                  | static    | type a boxed struct, throwing on mismatch |
|  [02]   | `Box<T>.DangerousGetFrom(object) -> Box<T>`                         | static    | type a boxed struct unchecked             |
|  [03]   | `Box<T>.TryGetFrom(object, out Box<T>)`                             | static    | probe a boxed struct                      |
|  [04]   | `BoxExtensions.GetReference<T>(Box<T>) -> ref T`                    | extension | mutate the boxed value in place           |
|  [05]   | `NullableRef<T>.Value -> ref T`                                     | property  | address the carried reference             |
|  [06]   | `NullableRef<T>.HasValue`                                           | property  | test presence before deref                |
|  [07]   | `ObjectMarshal.TryUnbox<T>(object, out T)`                          | extension | probe an unboxed value                    |
|  [08]   | `ObjectMarshal.DangerousUnbox<T>(object) -> ref T`                  | static    | address a boxed value unchecked           |
|  [09]   | `ObjectMarshal.DangerousGetObjectDataByteOffset<T>(object, ref T)`  | static    | compute a payload byte offset             |
|  [10]   | `ObjectMarshal.DangerousGetObjectDataReferenceAt<T>(object, nint)`  | static    | address payload at an offset              |
|  [11]   | `NullableExtensions.DangerousGetValueOrDefaultReference<T>(ref T?)` | extension | reference into a `Nullable<T>`            |
|  [12]   | `ListExtensions.AsSpan<T>(List<T>)`                                 | extension | address list backing storage              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Pooled owners bound the staging lifetime: a rental enters through `Allocate`, projects as span, memory, plane, or array segment while live, and returns to its pool on `Dispose`.
- `AsStream` is the whole public stream surface over memory, owners, writers, and sequences; its implementations stay assembly-internal.
- A `struct TAction` carries the parallel kernel and its captured state rides the `in` overload, so partitioning allocates nothing and the invoker inlines.
- Each `BitHelper` operation binds one machine width per call site, so a flag axis picks `uint` or `ulong` once and its accumulators stay that width.
- Reinterpreting projections — `AsBytes`, `Cast`, `DangerousGetReferenceAt` — admit unmanaged elements and hand endianness ownership to the calling rail.

[STACKING]:
- `Microsoft.Extensions.Caching.Hybrid`(`.api/api-hybrid-cache.md`): `ArrayPoolBufferWriter<byte>` is the `IHybridCacheSerializer<T>` serialize target, and its `WrittenMemory` feeds the paired deserialize read, so an L2 payload never materializes an intermediate array.
- `Google.Protobuf`(`.api/api-protobuf.md`): `MemoryOwner<byte>.DangerousGetArray` hands its rented `ArraySegment<byte>` to `UnsafeByteOperations.UnsafeWrap`, and the owner disposes after the send it backs.
- `System.IO.Hashing`(`.api/api-hashing.md`): `XxHash128.HashToUInt128(writer.WrittenSpan)` fingerprints a committed payload straight off the pooled writer.
- Staging rail: one `MemoryOwner<T>` rental backs a `Memory2D<T>` plane, `ParallelHelper.ForEach` partitions that plane over a state-carrying `IRefAction<T>`, and the same rental emits through `ArrayPoolBufferWriter<byte>` into the codec, so one allocation spans compute and emit.

[LOCAL_ADMISSION]:
- Compute staging binds these memory shapes before minting a package-local payload owner.
- Ref carriers and plane views are implementation material inside a staging owner; domain types carry their own value vocabulary.
- A parallel entrypoint becomes a default execution path on a benchmark receipt.
- Pooled text is a staging receipt value; a domain value carries its own string.
- A byte projection declares codec and endianness ownership at the calling rail.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: pooled rental lifetime, dense plane views, ref carriers, stream projection over materialized bytes, struct-action partitioning
- Accept: bounded execution-payload staging composed from pooled owners, planes, writers, and partition kernels
- Reject: a package-local span wrapper over these shapes; `HashCode<T>.Combine` value-span digests, which `System.IO.Hashing` owns
