# [RASM_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies span planes, pooled ownership, ref carriers, tokenizers, stream projections, and parallel helpers for measured staging payloads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`

- package: `CommunityToolkit.HighPerformance`
- assembly: `CommunityToolkit.HighPerformance`
- namespaces: `CommunityToolkit.HighPerformance`, `.Buffers`, `.Enumerables`, `.Helpers` (the `.Streams` namespace types are package-internal)
- license: MIT (.NET Foundation)
- bound asset: `lib/net8.0/CommunityToolkit.HighPerformance.dll` (ships `net8.0`/`netstandard2.0`/`netstandard2.1`; no `net10.0` asset, so consumer `net10.0` binds `net8.0`)
- asset: runtime library
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: memory shapes

- rail: staging

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]  | [CAPABILITY]                    |
| :-----: | :------------------------- | :-------------- | :------------------------------ |
|  [01]   | `Span2D<T>`                | span view       | addresses dense planes          |
|  [02]   | `ReadOnlySpan2D<T>`        | span view       | reads dense planes              |
|  [03]   | `Memory2D<T>`              | memory view     | carries owned planes            |
|  [04]   | `ReadOnlyMemory2D<T>`      | memory view     | carries read-only planes        |
|  [05]   | `Ref<T>`                   | ref carrier     | preserves ref access            |
|  [06]   | `ReadOnlyRef<T>`           | ref carrier     | preserves read refs             |
|  [07]   | `NullableRef<T>`           | ref carrier     | carries optional refs           |
|  [08]   | `NullableReadOnlyRef<T>`   | ref carrier     | carries optional refs           |
|  [09]   | `Box<T>`                   | box carrier     | types boxed structs             |
|  [10]   | `MemoryOwner<T>`           | pooled owner    | owns heap-safe rented memory    |
|  [11]   | `SpanOwner<T>`             | pooled owner    | owns stack-only rented spans    |
|  [12]   | `ArrayPoolBufferWriter<T>` | pooled writer   | writes into pooled storage      |
|  [13]   | `MemoryBufferWriter<T>`    | memory writer   | writes into fixed caller memory |
|  [14]   | `IBuffer<T>`               | buffer contract | exposes committed writer state  |
|  [15]   | `AllocationMode`           | pool policy     | selects rental clearing         |
|  [16]   | `StringPool`               | text pool       | interns staged text             |

[PUBLIC_TYPE_SCOPE]: enumeration and helper shapes

- rail: staging

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]                |
| :-----: | :-------------------------- | :-------------- | :-------------------------- |
|  [01]   | `ReadOnlySpanTokenizer<T>`  | tokenizer       | splits spans                |
|  [02]   | `SpanTokenizer<T>`          | tokenizer       | splits mutable spans        |
|  [03]   | `ReadOnlySpanEnumerable<T>` | item enumerable | enumerates with index       |
|  [04]   | `SpanEnumerable<T>`         | item enumerable | enumerates with index       |
|  [05]   | `ReadOnlyRefEnumerable<T>`  | ref enumerable  | enumerates by ref           |
|  [06]   | `RefEnumerable<T>`          | ref enumerable  | enumerates by ref           |
|  [07]   | `ParallelHelper`            | parallel root   | partitions batch work       |
|  [08]   | `IAction`                   | action contract | defines 1D index work       |
|  [09]   | `IAction2D`                 | action contract | defines 2D index work       |
|  [10]   | `IInAction<T>`              | action contract | defines read-only item work |
|  [11]   | `IRefAction<T>`             | action contract | defines mutable item work   |
|  [12]   | `BitHelper`                 | bit helper      | packs bit flags             |
|  [13]   | `HashCode<T>`               | hash helper     | excluded value hashing      |
|  [14]   | `ObjectMarshal`             | marshal helper  | reads object internals      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: allocation and ownership

- rail: staging
- `MemoryOwner<T>` and `SpanOwner<T>` return `.Empty` for zero-sized allocation and expose the same overload family. The pool form admits a named pool, and `AllocationMode` selects zero-on-rent.

| [INDEX] | [SURFACE]                               | [KIND]    | [CAPABILITY]                 |
| :-----: | :-------------------------------------- | :-------- | :--------------------------- |
|  [01]   | `MemoryOwner<T>.Allocate`               | factory   | rents heap-safe memory       |
|  [02]   | `SpanOwner<T>.Allocate`                 | factory   | rents a stack-only span      |
|  [03]   | `MemoryOwner<T>.Slice`                  | instance  | projects a sub-window        |
|  [04]   | `MemoryOwner<T>.DangerousGetArray`      | instance  | returns rented array segment |
|  [05]   | `SpanOwner<T>.DangerousGetArray`        | instance  | returns rented array segment |
|  [06]   | `MemoryOwner<T>.DangerousGetReference`  | instance  | returns the root reference   |
|  [07]   | `SpanOwner<T>.DangerousGetReference`    | instance  | returns the root reference   |
|  [08]   | `ArrayPoolExtensions.Resize<T>`         | extension | resizes a rented array       |
|  [09]   | `ArrayPoolExtensions.EnsureCapacity<T>` | extension | grows an undersized rental   |
|  [10]   | `StringPool.GetOrAdd`                   | pool      | interns staged text          |
|  [11]   | `MemoryOwner<T>.Dispose`                | lifetime  | returns pooled storage       |
|  [12]   | `SpanOwner<T>.Dispose`                  | lifetime  | returns pooled storage       |
|  [13]   | `ArrayPoolBufferWriter<T>.Dispose`      | lifetime  | returns pooled storage       |

[ALLOCATION_OVERLOADS]:

- `(int size)`
- `(int size, ArrayPool<T> pool)`
- `(int size, AllocationMode mode)`
- `(int size, ArrayPool<T> pool, AllocationMode mode)`

[ARRAY_POOL_EXTENSIONS]:

- `Resize<T>`: `static void Resize<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int newSize, bool clearArray = false)`
- `EnsureCapacity<T>`: `static void EnsureCapacity<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int capacity, bool clearArray = false)`

[STRING_POOL_OVERLOADS]:

- `string GetOrAdd(string value)`
- `string GetOrAdd(ReadOnlySpan<char> span)`
- `string GetOrAdd(ReadOnlySpan<byte> span, Encoding encoding)`

[ENTRYPOINT_SCOPE]: buffer-writer emit (the `IBufferWriter<byte>` seam)

- rail: staging-and-streams#STREAM_POOL
- `ArrayPoolBufferWriter<T>` and `MemoryBufferWriter<T>` are the `IBufferWriter<T>` codec-emit sink that stacks ONTO the suite serializers: a `Utf8JsonWriter`, a protobuf `CodedOutputStream`, or `IHybridCacheSerializer<T>.Serialize(value, IBufferWriter<byte>)` writes directly into the pooled writer, and the result reads back as `WrittenMemory`/`WrittenSpan` (= a `ReadOnlySequence<byte>` for the cache deserialize hop) with zero intermediate array.

| [INDEX] | [SURFACE]                                      | [RESULT]            | [CAPABILITY]                  |
| :-----: | :--------------------------------------------- | :------------------ | :---------------------------- |
|  [01]   | `GetSpan(int sizeHint = 0)`                    | `Span<T>`           | opens a writable region       |
|  [02]   | `GetMemory(int sizeHint = 0)`                  | `Memory<T>`         | opens a writable region       |
|  [03]   | `Advance(int count)`                           | `void`              | commits written elements      |
|  [04]   | `WrittenMemory`                                | `ReadOnlyMemory<T>` | reads committed payload       |
|  [05]   | `WrittenSpan`                                  | `ReadOnlySpan<T>`   | reads committed payload       |
|  [06]   | `WrittenCount`                                 | `int`               | reads committed element count |
|  [07]   | `Capacity`                                     | `int`               | reads total capacity          |
|  [08]   | `FreeCapacity`                                 | `int`               | reads remaining capacity      |
|  [09]   | `ArrayPoolBufferWriter<T>.DangerousGetArray()` | `ArraySegment<T>`   | exposes the rented array      |
|  [10]   | `Clear()`                                      | `void`              | resets the write head         |

[ENTRYPOINT_SCOPE]: projections and transforms

- rail: staging
- `T[,]` exposes whole-array and windowed `(row, column, height, width)` plane forms, and `T[,,]` exposes a `(depth)` slice.
- `Memory<T>` and `ReadOnlyMemory<T>` expose `(height, width)` and padded `(offset, height, width, pitch)` plane forms.
- `Span<T>` and `ReadOnlySpan<T>` expose `(height, width)` and padded `(offset, height, width, pitch)` plane forms.
- `AsBytes` and `Cast` require unmanaged source and destination elements, and a reinterpreted payload crosses a process boundary only through its codec.

| [INDEX] | [SURFACE]                    | [CAPABILITY]                   |
| :-----: | :--------------------------- | :----------------------------- |
|  [01]   | `AsSpan2D<T>`                | projects a plane view          |
|  [02]   | `AsMemory2D<T>`              | projects an owned plane        |
|  [03]   | `GetRow<T>`                  | projects a row by reference    |
|  [04]   | `GetColumn<T>`               | projects a column by reference |
|  [05]   | `GetRowSpan<T>`              | projects a contiguous row span |
|  [06]   | `GetRowMemory<T>`            | projects contiguous row memory |
|  [07]   | `DangerousGetReference<T>`   | exposes the root reference     |
|  [08]   | `DangerousGetReferenceAt<T>` | exposes an indexed reference   |
|  [09]   | `AsBytes<T>`                 | reinterprets unmanaged storage |
|  [10]   | `Cast<TFrom, TTo>`           | reinterprets element widths    |
|  [11]   | `Tokenize`                   | splits spans                   |
|  [12]   | `Enumerate`                  | exposes reference enumeration  |

[ENTRYPOINT_SCOPE]: `AsStream` byte bridge (zero-copy IO edge)

- rail: staging-and-streams#STREAM_POOL
- `AsStream` extensions project already-materialized byte payloads without an intermediate array. The `IMemoryOwner<byte>` form transfers disposal to the stream, and the span-writing `Write<T>` overload admits only unmanaged elements.

| [INDEX] | [SURFACE]                                           | [RECEIVER]                    | [CAPABILITY]                |
| :-----: | :-------------------------------------------------- | :---------------------------- | :-------------------------- |
|  [01]   | `MemoryExtensions.AsStream`                         | `Memory<byte>`                | opens a duplex stream       |
|  [02]   | `ReadOnlyMemoryExtensions.AsStream`                 | `ReadOnlyMemory<byte>`        | opens a read stream         |
|  [03]   | `ReadOnlySequenceExtensions.AsStream`               | `ReadOnlySequence<byte>`      | streams segmented payloads  |
|  [04]   | `IMemoryOwnerExtensions.AsStream`                   | `IMemoryOwner<byte>`          | transfers owner disposal    |
|  [05]   | `ArrayPoolBufferWriterExtensions.AsStream`          | `ArrayPoolBufferWriter<byte>` | opens a write stream        |
|  [06]   | `IBufferWriterExtensions.AsStream`                  | `IBufferWriter<byte>`         | opens a write stream        |
|  [07]   | `IBufferWriterExtensions.Write<T>(ReadOnlySpan<T>)` | `IBufferWriter<byte>`         | copies unmanaged span bytes |

[ENTRYPOINT_SCOPE]: parallel partition operations

- rail: staging

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                 | [CAPABILITY]                |
| :-----: | :-------------------------------------- | :------------------------------------------- | :-------------------------- |
|  [01]   | `ParallelHelper.For<TAction>`           | `(int start, int end)`                       | partitions index slots      |
|  [02]   | `ParallelHelper.For<TAction>`           | `Range`                                      | partitions range indices    |
|  [03]   | `ParallelHelper.For2D<TAction>`         | `(int top, int bottom, int left, int right)` | partitions explicit bounds  |
|  [04]   | `ParallelHelper.For2D<TAction>`         | `Rectangle`                                  | partitions a rectangle      |
|  [05]   | `ParallelHelper.For2D<TAction>`         | `(Range i, Range j)`                         | partitions paired ranges    |
|  [06]   | `ParallelHelper.ForEach<TItem,TAction>` | `Memory<T>`                                  | mutates items by reference  |
|  [07]   | `ParallelHelper.ForEach<TItem,TAction>` | `ReadOnlyMemory<T>`                          | reads items by reference    |
|  [08]   | `ParallelHelper.ForEach<TItem,TAction>` | `Memory2D<T>`                                | mutates planes by reference |
|  [09]   | `ParallelHelper.ForEach<TItem,TAction>` | `ReadOnlyMemory2D<T>`                        | reads planes by reference   |

[ENTRYPOINT_SCOPE]: action contract invocation

- rail: staging

| [INDEX] | [SURFACE]              | [CALL_SHAPE]   | [CAPABILITY]             |
| :-----: | :--------------------- | :------------- | :----------------------- |
|  [01]   | `IAction.Invoke`       | 1D index       | handles one index slot   |
|  [02]   | `IAction2D.Invoke`     | 2D index pair  | handles one 2D slot      |
|  [03]   | `IInAction<T>.Invoke`  | read-only item | handles one input item   |
|  [04]   | `IRefAction<T>.Invoke` | mutable item   | handles one mutable item |

[ENTRYPOINT_SCOPE]: `BitHelper` member signatures

- source: `CommunityToolkit.HighPerformance` — `CommunityToolkit.HighPerformance.Helpers.BitHelper` surface
- namespace: `CommunityToolkit.HighPerformance.Helpers`
- rail: staging-and-streams#STREAM_POOL
- consumer: `staging-and-streams#STREAM_POOL`

| [INDEX] | [MEMBER]                              | [SIGNATURE]                                                                   |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `HasFlag(uint,int)`                   | `static bool HasFlag(uint value, int n)`                                      |
|  [02]   | `HasFlag(ulong,int)`                  | `static bool HasFlag(ulong value, int n)`                                     |
|  [03]   | `HasZeroByte(uint)`                   | `static bool HasZeroByte(uint value)`                                         |
|  [04]   | `HasZeroByte(ulong)`                  | `static bool HasZeroByte(ulong value)`                                        |
|  [05]   | `HasByteEqualTo(uint,byte)`           | `static bool HasByteEqualTo(uint value, byte target)`                         |
|  [06]   | `HasByteEqualTo(ulong,byte)`          | `static bool HasByteEqualTo(ulong value, byte target)`                        |
|  [07]   | `SetFlag(ref uint,int,bool)`          | `static void SetFlag(ref uint value, int n, bool flag)`                       |
|  [08]   | `SetFlag(uint,int,bool)`              | `static uint SetFlag(uint value, int n, bool flag)`                           |
|  [09]   | `SetFlag(ref ulong,int,bool)`         | `static void SetFlag(ref ulong value, int n, bool flag)`                      |
|  [10]   | `SetFlag(ulong,int,bool)`             | `static ulong SetFlag(ulong value, int n, bool flag)`                         |
|  [11]   | `ExtractRange(uint,byte,byte)`        | `static uint ExtractRange(uint value, byte start, byte length)`               |
|  [12]   | `ExtractRange(ulong,byte,byte)`       | `static ulong ExtractRange(ulong value, byte start, byte length)`             |
|  [13]   | `SetRange(ref uint,byte,byte,uint)`   | `static void SetRange(ref uint value, byte start, byte length, uint flags)`   |
|  [14]   | `SetRange(uint,byte,byte,uint)`       | `static uint SetRange(uint value, byte start, byte length, uint flags)`       |
|  [15]   | `SetRange(ref ulong,byte,byte,ulong)` | `static void SetRange(ref ulong value, byte start, byte length, ulong flags)` |
|  [16]   | `SetRange(ulong,byte,byte,ulong)`     | `static ulong SetRange(ulong value, byte start, byte length, ulong flags)`    |
|  [17]   | `HasLookupFlag(uint,int,int)`         | `static bool HasLookupFlag(uint table, int x, int min = 0)`                   |
|  [18]   | `HasLookupFlag(ulong,int,int)`        | `static bool HasLookupFlag(ulong table, int x, int min = 0)`                  |

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
