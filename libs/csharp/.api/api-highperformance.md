# [RASM_COMPUTE_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies span planes, pooled ownership,
ref carriers, tokenizers, stream projection extensions, and parallel helpers
for measured staging payloads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`
- package: `CommunityToolkit.HighPerformance`
- assembly: `CommunityToolkit.HighPerformance`
- namespaces: `CommunityToolkit.HighPerformance`, `.Buffers`, `.Helpers` (the `.Streams` namespace types are package-internal)
- license: MIT (.NET Foundation)
- bound asset: `lib/net8.0/CommunityToolkit.HighPerformance.dll` (ships `net8.0`/`netstandard2.0`/`netstandard2.1`; no `net10.0` asset, so consumer `net10.0` binds `net8.0`)
- asset: runtime library
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: memory shapes
- rail: staging

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------------- |:-------------- |:----------------------- |
| [01] | `Span2D<T>` | span view | addresses dense planes |
| [02] | `ReadOnlySpan2D<T>` | span view | reads dense planes |
| [03] | `Memory2D<T>` | memory view | carries owned planes |
| [04] | `ReadOnlyMemory2D<T>` | memory view | carries read-only planes |
| [05] | `Ref<T>` | ref carrier | preserves ref access |
| [06] | `ReadOnlyRef<T>` | ref carrier | preserves read refs |
| [07] | `NullableRef<T>` | ref carrier | carries optional refs |
| [08] | `NullableReadOnlyRef<T>` | ref carrier | carries optional refs |
| [09] | `Box<T>` | box carrier | types boxed structs |
| [10] | `MemoryOwner<T>` | pooled owner | `IMemoryOwner<T>` over a rented array; heap-allocatable, async-safe; `.Memory`/`.Span`/`Slice` |
| [11] | `SpanOwner<T>` | pooled owner | `readonly ref struct` over a rented array; stack-only, never crosses an `await`/iterator; `.Span` only |
| [12] | `ArrayPoolBufferWriter<T>` | pooled writer | `IBuffer<T>`+`IBufferWriter<T>`+`IMemoryOwner<T>` over pooled storage; the codec-emit sink |
| [13] | `MemoryBufferWriter<T>` | memory writer | `IBuffer<T>`+`IBufferWriter<T>` over a caller-supplied `Memory<T>` (no pool, fixed capacity) |
| [14] | `IBuffer<T>` | buffer contract | extends `IBufferWriter<T>` with `WrittenMemory`/`WrittenSpan`/`WrittenCount`/`Capacity`/`FreeCapacity` |
| [15] | `AllocationMode` | pool policy | `Default` (no clear) / `Clear` (zero-on-rent); in `.Buffers` namespace |
| [16] | `StringPool` | text pool | interns staged text; `Shared` singleton + `GetOrAdd(ReadOnlySpan<byte>, Encoding)` |

[PUBLIC_TYPE_SCOPE]: enumeration and helper shapes
- rail: staging

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:-------------------------- |:-------------- |:-------------------------- |
| [01] | `ReadOnlySpanTokenizer<T>` | tokenizer | splits spans |
| [02] | `SpanTokenizer<T>` | tokenizer | splits mutable spans |
| [03] | `ReadOnlySpanEnumerable<T>` | item enumerable | enumerates with index |
| [04] | `SpanEnumerable<T>` | item enumerable | enumerates with index |
| [05] | `ReadOnlyRefEnumerable<T>` | ref enumerable | enumerates by ref |
| [06] | `RefEnumerable<T>` | ref enumerable | enumerates by ref |
| [07] | `ParallelHelper` | parallel root | partitions batch work |
| [08] | `IAction` / `IAction2D` | action contract | defines indexed work |
| [09] | `IInAction` / `IRefAction` | action contract | defines item work |
| [10] | `BitHelper` | bit helper | packs bit flags |
| [11] | `HashCode<T>` | hash helper | REJECTED — see `[REJECTED]` |
| [12] | `ObjectMarshal` | marshal helper | reads object internals |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: allocation and ownership
- rail: staging
- Both owners expose the same 4-overload `Allocate` ladder: `(int size)`, `(int size, ArrayPool<T> pool)`, `(int size, AllocationMode mode)`, `(int size, ArrayPool<T> pool, AllocationMode mode)`. The pool overload is the named-pool admission seam; the `AllocationMode` overload selects zero-on-rent. `ArrayPoolExtensions` lives in the ROOT `CommunityToolkit.HighPerformance` namespace (not `.Buffers`).

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `MemoryOwner<T>.Allocate(int[, ArrayPool<T>][, AllocationMode])` | factory call | rents an `IMemoryOwner<T>`; `.Empty` for the zero case |
| [02] | `SpanOwner<T>.Allocate(int[, ArrayPool<T>][, AllocationMode])` | factory call | rents a stack-only owner; `.Empty` for the zero case |
| [03] | `MemoryOwner<T>.Slice(int start, int length)` | instance call | projects a sub-window without re-renting |
| [04] | `MemoryOwner<T>.DangerousGetArray` / `SpanOwner<T>.DangerousGetArray` | instance call | exposes the rented `ArraySegment<T>` for an interop wrap |
| [05] | `DangerousGetReference()` | instance call | the `ref T` root of either owner for a pinned-free kernel |
| [06] | `ArrayPoolExtensions.Resize<T>(this ArrayPool<T>, ref T[], int newSize, bool clearArray = false)` | extension call | grows/shrinks a rented array in place, returning the old |
| [07] | `ArrayPoolExtensions.EnsureCapacity<T>(this ArrayPool<T>, ref T[], int capacity, bool clearArray = false)` | extension call | grows a rented array only if under capacity |
| [08] | `StringPool.GetOrAdd(string)` / `(ReadOnlySpan<char>)` / `(ReadOnlySpan<byte>, Encoding)` | pool call | interns staged text; the `(span, Encoding)` form decodes UTF-8 bytes without an intermediate string |
| [09] | `Dispose` | lifetime call | returns pooled storage; deterministic on every owner/writer |

[ENTRYPOINT_SCOPE]: buffer-writer emit (the `IBufferWriter<byte>` seam)
- rail: staging-and-streams#STREAM_POOL
- `ArrayPoolBufferWriter<T>` and `MemoryBufferWriter<T>` are the `IBufferWriter<T>` codec-emit sink that stacks ONTO the suite serializers: a `Utf8JsonWriter`, a protobuf `CodedOutputStream`, or `IHybridCacheSerializer<T>.Serialize(value, IBufferWriter<byte>)` writes directly into the pooled writer, and the result reads back as `WrittenMemory`/`WrittenSpan` (= a `ReadOnlySequence<byte>` for the cache deserialize hop) with zero intermediate array.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `GetSpan(int sizeHint = 0)` / `GetMemory(int sizeHint = 0)` | writer call | obtains the next writable region (the `IBufferWriter<T>` contract) |
| [02] | `Advance(int count)` | writer call | commits `count` written elements |
| [03] | `WrittenMemory` / `WrittenSpan` / `WrittenCount` | writer prop | reads the committed payload back for a zero-copy hand-off |
| [04] | `Capacity` / `FreeCapacity` | writer prop | sizing facts for an incremental emit |
| [05] | `ArrayPoolBufferWriter<T>.DangerousGetArray` | writer call | the rented `ArraySegment<T>` for `UnsafeByteOperations.UnsafeWrap` |
| [06] | `Clear()` | writer call | resets the write head for reuse without re-renting |

[ENTRYPOINT_SCOPE]: projections and transforms
- rail: staging
- `AsBytes`/`Cast` carry a `where T: unmanaged` (and `Cast` a `TFrom/TTo: unmanaged`) constraint — only blittable element types reinterpret, and a reinterpreted payload never crosses a process boundary uncoded. `AsMemory2D`/`AsSpan2D` over a 2D array have whole-array and windowed `(row, column, height, width)` forms; over `Memory<T>` they take `(height, width)` or the padded `(offset, height, width, pitch)` stride form; a 3D array has a `(depth)` slice form. `AsStream` covers FIVE `byte`-payload receivers (see the `AsStream` byte bridge scope below), never `Memory<byte>` alone; the `.Streams` implementation types are package-internal, only the extension surface is public.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `AsSpan2D<T>(this T[,])` / `(…, row, column, height, width)` | extension call | projects a whole or windowed plane view over a 2D array |
| [02] | `AsMemory2D<T>(this Memory<T>, height, width)` / `(…, offset, height, width, pitch)` | extension call | projects an owned memory plane; the `pitch` form carries padded image strides uncopied |
| [03] | `GetRow<T>(this T[,], int)` / `GetColumn<T>(this T[,], int)` | extension call | a `RefEnumerable<T>` over one plane axis (by ref, no copy) |
| [04] | `GetRowSpan<T>(this T[,], int)` / `GetRowMemory<T>(this T[,], int)` | extension call | the contiguous row as `Span<T>`/`Memory<T>` |
| [05] | `DangerousGetReference<T>` / `DangerousGetReferenceAt<T>(…, i[, j[, k]])` | extension call | the `ref T` root / a ref at flat or rank-2/3 index |
| [06] | `AsBytes<T>(this Span<T>) where T: unmanaged` | extension call | reinterprets a blittable span as raw `Span<byte>` |
| [07] | `Cast<TFrom, TTo>(…) where TFrom, TTo: unmanaged` | extension call | reinterprets a blittable `Span`/`Memory` to another width |
| [08] | `Tokenize` | extension call | splits a span on a separator into a `SpanTokenizer<T>` |
| [09] | `Enumerate` | extension call | exposes ref enumeration over a span |

[ENTRYPOINT_SCOPE]: `AsStream` byte bridge (zero-copy IO edge)
- rail: staging-and-streams#STREAM_POOL
- Five extension classes each project one already-materialized `byte` payload as a `System.IO.Stream` with NO intermediate `byte[]`, so a codec drain or a transport body wraps the existing buffer at the IO edge. The `IMemoryOwner<byte>` form disposes the owner with the stream.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `MemoryExtensions.AsStream(this Memory<byte>)` | extension call | a writable rented buffer as a read/write `Stream` |
| [02] | `ReadOnlyMemoryExtensions.AsStream(this ReadOnlyMemory<byte>)` | extension call | a read-only window as a read `Stream` |
| [03] | `ReadOnlySequenceExtensions.AsStream(this ReadOnlySequence<byte>)` | extension call | a multi-segment payload as one read `Stream`, no flatten |
| [04] | `IMemoryOwnerExtensions.AsStream(this IMemoryOwner<byte>)` | extension call | a pooled owner rental as a `Stream` (disposes the owner) |
| [05] | `IBufferWriterExtensions.AsStream(this IBufferWriter<byte>)` | extension call | a codec drain target as a write `Stream` |
| [06] | `IBufferWriterExtensions.Write<T>(this IBufferWriter<byte>, ReadOnlySpan<T>) where T: unmanaged` | extension call | blit a primitive/struct span into the writer (codec sink) |

[ENTRYPOINT_SCOPE]: parallel partition operations
- rail: staging

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:-------------------------------------- |:------------------ |:-------------------------- |
| [01] | `ParallelHelper.For<TAction>` | 1D index range | partitions index slots |
| [02] | `ParallelHelper.For<TAction>` | `Range` span | partitions a range span |
| [03] | `ParallelHelper.For2D<TAction>` | 2D rectangle bounds | partitions a 2D rectangle |
| [04] | `ParallelHelper.For2D<TAction>` | 2D region values | partitions a 2D region |
| [05] | `ParallelHelper.ForEach<TItem,TAction>` | `Memory<T>` | mutates items by reference |
| [06] | `ParallelHelper.ForEach<TItem,TAction>` | `ReadOnlyMemory<T>` | reads items by reference |
| [07] | `ParallelHelper.ForEach<TItem,TAction>` | memory plane | partitions 2D memory planes |

[ENTRYPOINT_SCOPE]: action contract invocation
- rail: staging

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------- |:------------- |:----------------------- |
| [01] | `IAction.Invoke` | 1D index | handles one index slot |
| [02] | `IAction2D.Invoke` | 2D index pair | handles one 2D slot |
| [03] | `IInAction<T>.Invoke` | read-only item | handles one input item |
| [04] | `IRefAction<T>.Invoke` | mutable item | handles one mutable item |

[ENTRYPOINT_SCOPE]: `BitHelper` member member signatures
- source: `CommunityToolkit.HighPerformance` — `CommunityToolkit.HighPerformance.Helpers.BitHelper` surface
- namespace: `CommunityToolkit.HighPerformance.Helpers`
- rail: staging-and-streams#STREAM_POOL
- consumer: `staging-and-streams#STREAM_POOL`

| [INDEX] | [MEMBER] | [SIGNATURE] |
|:-----: |:------------------------------------ |:---------------------------------------------------------------------------- |
| [01] | `HasFlag(uint,int)` | `static bool HasFlag(uint value, int n)` |
| [02] | `HasFlag(ulong,int)` | `static bool HasFlag(ulong value, int n)` |
| [03] | `SetFlag(ref uint,int,bool)` | `static void SetFlag(ref uint value, int n, bool flag)` |
| [04] | `SetFlag(uint,int,bool)` | `static uint SetFlag(uint value, int n, bool flag)` |
| [05] | `SetFlag(ref ulong,int,bool)` | `static void SetFlag(ref ulong value, int n, bool flag)` |
| [06] | `SetFlag(ulong,int,bool)` | `static ulong SetFlag(ulong value, int n, bool flag)` |
| [07] | `ExtractRange(uint,byte,byte)` | `static uint ExtractRange(uint value, byte start, byte length)` |
| [08] | `ExtractRange(ulong,byte,byte)` | `static ulong ExtractRange(ulong value, byte start, byte length)` |
| [09] | `SetRange(ref uint,byte,byte,uint)` | `static void SetRange(ref uint value, byte start, byte length, uint flags)` |
| [10] | `SetRange(uint,byte,byte,uint)` | `static uint SetRange(uint value, byte start, byte length, uint flags)` |
| [11] | `SetRange(ref ulong,byte,byte,ulong)` | `static void SetRange(ref ulong value, byte start, byte length, ulong flags)` |
| [12] | `SetRange(ulong,byte,byte,ulong)` | `static ulong SetRange(ulong value, byte start, byte length, ulong flags)` |
| [13] | `HasLookupFlag(uint,int,int)` | `static bool HasLookupFlag(uint table, int x, int min = 0)` |
| [14] | `HasLookupFlag(ulong,int,int)` | `static bool HasLookupFlag(ulong table, int x, int min = 0)` |

## [04]-[IMPLEMENTATION_LAW]

[STAGING_MEMORY]:
- namespaces: `CommunityToolkit.HighPerformance`, `CommunityToolkit.HighPerformance.Buffers`
- ownership: pooled owners are the staging lifetime boundary
- payload shape: spans and memory planes carry tensor, vector, codec, and remote buffers
- release rule: every owned buffer returns through deterministic disposal

[STAGING_STREAMS]:
- bridge: memory, memory owners, buffer writers, and read-only sequences become streams only through the `AsStream` extension family at IO edges
- implementation rule: stream implementation types (`CommunityToolkit.HighPerformance.Streams.*`) are package-internal and never appear in Compute vocabulary; only the `AsStream` extension surface is public
- text rule: pooled text belongs to staging receipts, not domain values

[SERIALIZER_STACKING]:
- the `IBufferWriter<byte>` writers are the convergence seam where this package stacks onto the codec and cache rails: `ArrayPoolBufferWriter<byte>` is the `IHybridCacheSerializer<T>.Serialize(value, IBufferWriter<byte>)` target and its `WrittenMemory` reads back as the `ReadOnlySequence<byte>` source `IHybridCacheSerializer<T>.Deserialize` consumes, so an L2 cache payload codec never allocates an intermediate array
- `MemoryOwner<byte>.DangerousGetArray` hands the rented `ArraySegment<byte>` to protobuf `UnsafeByteOperations.UnsafeWrap` for a zero-copy `ByteString`; the owner outlives the wrap and disposes after the send
- content identity over a written payload routes through the `System.IO.Hashing` `XxHash128.HashToUInt128(writer.WrittenSpan)` form, never a package-local digest

[PARALLEL_PARTITION]:
- partition root: `ParallelHelper` static methods `For`, `For2D`, `ForEach` (in `CommunityToolkit.HighPerformance.Helpers`)
- work shape: callers pass a `struct` action implementing `IAction` / `IAction2D` / `IInAction<T>` (read-only item) / `IRefAction<T>` (mutable item by ref); the `struct` constraint keeps the invoker allocation-free and inlinable
- action seeding: every overload comes in a no-seed form (the `TAction` is `default`-constructed per partition — for a stateless kernel) and an `in TAction action` form (a pre-populated struct carrying captured state, copied per partition); the `in` form is the route for an action holding a span root or a config field
- index forms: `For` takes `(int start, int end)` or `Range`; `For2D` takes `(top, bottom, left, right)`, `Rectangle`, or `(Range i, Range j)`
- item forms: `ForEach<TItem, TAction>` partitions `ReadOnlyMemory<T>` / `ReadOnlyMemory2D<T>` (over `IInAction<T>`) and `Memory<T>` / `Memory2D<T>` (over `IRefAction<T>`)
- granularity: `minimumActionsPerThread` lower-bounds work per thread; degree of parallelism clamps to `Environment.ProcessorCount`
- single-thread collapse: a partition count of one invokes the action inline on the calling thread

[BIT_FLAGS]:
- helper root: `BitHelper` static class in namespace `CommunityToolkit.HighPerformance.Helpers`
- flag forms: `bool HasFlag(value, int n)`; `void SetFlag(ref value, int n, bool flag)` in-place and `value SetFlag(value, int n, bool flag)` value-returning
- range forms: `value ExtractRange(value, byte start, byte length)`; `void SetRange(ref value, byte start, byte length, flags)` in-place and value-returning, `flags` typed to the word width
- word width: every method has a `uint` and a `ulong` overload; the staging allocation-axis word set/has-flag column binds the `ulong` overloads
- Compute staging uses package memory shapes before introducing package-local payload owners.
- Ref carriers remain internal implementation material and never become public domain vocabulary.
- Parallel helper entrypoints require benchmark receipts before becoming a default execution path.
- Byte projections require explicit codec and endianness ownership at the calling rail.

[REJECTED]:
- `HashCode<T>.Combine` is not admitted: the suite identity-hashing surface is a single monopoly held by `System.IO.Hashing` — `XxHash128` for whole-artifact and content-address identity and `Crc32` for per-frame integrity (the `FrameEdge`/`InterchangeIdentity` owners). A second hashing helper for value-span digests does mint a parallel hash owner that fragments the content-address identity, so the `HashCode<T>` span-combine path stays out of the package and every value digest routes through the XxHash/Crc32 monopoly.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: pooled memory, span planes, ref views, stream projection
- Accept: bounded execution payload staging
- Reject: package-local span wrappers; `HashCode<T>.Combine` value-span hashing (XxHash128/Crc32 monopoly via `System.IO.Hashing`)
