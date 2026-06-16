# [RASM_COMPUTE_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies span planes, pooled ownership,
ref carriers, tokenizers, stream projection extensions, and parallel helpers
for measured staging payloads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`
- package: `CommunityToolkit.HighPerformance`
- assembly: `CommunityToolkit.HighPerformance`
- namespace: `CommunityToolkit.HighPerformance`
- asset: runtime library
- rail: staging

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: memory shapes
- rail: staging

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :------------------------- | :-------------- | :----------------------- |
|   [1]   | `Span2D<T>`                | span view       | addresses dense planes   |
|   [2]   | `ReadOnlySpan2D<T>`        | span view       | reads dense planes       |
|   [3]   | `Memory2D<T>`              | memory view     | carries owned planes     |
|   [4]   | `ReadOnlyMemory2D<T>`      | memory view     | carries read-only planes |
|   [5]   | `Ref<T>`                   | ref carrier     | preserves ref access     |
|   [6]   | `ReadOnlyRef<T>`           | ref carrier     | preserves read refs      |
|   [7]   | `NullableRef<T>`           | ref carrier     | carries optional refs    |
|   [8]   | `NullableReadOnlyRef<T>`   | ref carrier     | carries optional refs    |
|   [9]   | `Box<T>`                   | box carrier     | types boxed structs      |
|  [10]   | `MemoryOwner<T>`           | pooled owner    | owns rented memory       |
|  [11]   | `SpanOwner<T>`             | pooled owner    | owns rented span memory  |
|  [12]   | `ArrayPoolBufferWriter<T>` | pooled writer   | writes staged payloads   |
|  [13]   | `MemoryBufferWriter<T>`    | memory writer   | writes staged payloads   |
|  [14]   | `IBuffer<T>`               | buffer contract | defines writable buffer  |
|  [15]   | `AllocationMode`           | pool policy     | selects rent clearing    |
|  [16]   | `StringPool`               | text pool       | interns staged text      |

[PUBLIC_TYPE_SCOPE]: enumeration and helper shapes
- rail: staging

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]                |
| :-----: | :-------------------------- | :-------------- | :-------------------------- |
|   [1]   | `ReadOnlySpanTokenizer<T>`  | tokenizer       | splits spans                |
|   [2]   | `SpanTokenizer<T>`          | tokenizer       | splits mutable spans        |
|   [3]   | `ReadOnlySpanEnumerable<T>` | item enumerable | enumerates with index       |
|   [4]   | `SpanEnumerable<T>`         | item enumerable | enumerates with index       |
|   [5]   | `ReadOnlyRefEnumerable<T>`  | ref enumerable  | enumerates by ref           |
|   [6]   | `RefEnumerable<T>`          | ref enumerable  | enumerates by ref           |
|   [7]   | `ParallelHelper`            | parallel root   | partitions batch work       |
|   [8]   | `IAction` / `IAction2D`     | action contract | defines indexed work        |
|   [9]   | `IInAction` / `IRefAction`  | action contract | defines item work           |
|  [10]   | `BitHelper`                 | bit helper      | packs bit flags             |
|  [11]   | `HashCode<T>`               | hash helper     | REJECTED — see `[REJECTED]` |
|  [12]   | `ObjectMarshal`             | marshal helper  | reads object internals      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: allocation and ownership
- rail: staging

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]   | [CAPABILITY]                         |
| :-----: | :----------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `MemoryOwner<T>.Allocate`            | factory call   | rents owned memory                   |
|   [2]   | `SpanOwner<T>.Allocate`              | factory call   | rents owned span                     |
|   [3]   | `MemoryOwner<T>.Slice`               | instance call  | slices owned memory                  |
|   [4]   | `MemoryOwner<T>.DangerousGetArray`   | instance call  | exposes the rented `ArraySegment<T>` |
|   [5]   | `ArrayPoolExtensions.Resize`         | extension call | resizes rented array                 |
|   [6]   | `ArrayPoolExtensions.EnsureCapacity` | extension call | grows rented array                   |
|   [7]   | `StringPool.GetOrAdd`                | pool call      | interns text payload                 |
|   [8]   | `Dispose`                            | lifetime call  | returns pooled storage               |

[ENTRYPOINT_SCOPE]: projections and transforms
- rail: staging

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :-------------------------------- | :------------- | :---------------------- |
|   [1]   | `AsSpan2D`                        | extension call | projects plane view     |
|   [2]   | `AsMemory2D`                      | extension call | projects memory plane   |
|   [3]   | `GetRow` / `GetColumn`            | extension call | enumerates plane axis   |
|   [4]   | `GetRowSpan`                      | extension call | addresses plane row     |
|   [5]   | `DangerousGetReference`           | extension call | exposes ref root        |
|   [6]   | `AsBytes`                         | extension call | projects raw bytes      |
|   [7]   | `Cast`                            | extension call | reinterprets span bytes |
|   [8]   | `Tokenize`                        | extension call | splits span payloads    |
|   [9]   | `Enumerate`                       | extension call | exposes ref enumeration |
|  [10]   | `AsStream`                        | extension call | projects payload stream |
|  [11]   | `StreamExtensions.Read` / `Write` | extension call | moves span payloads     |

[ENTRYPOINT_SCOPE]: parallel partition operations
- rail: staging

Partition helpers carry the partition shape; action contracts carry invocation arity.

[PARTITION_OPERATIONS]:
| [INDEX] | [SURFACE]                               | [CALL_SHAPE]        | [CAPABILITY]                |
| :-----: | :-------------------------------------- | :------------------ | :-------------------------- |
|   [1]   | `ParallelHelper.For<TAction>`           | 1D index range      | partitions index slots      |
|   [2]   | `ParallelHelper.For<TAction>`           | `Range` span        | partitions a range span     |
|   [3]   | `ParallelHelper.For2D<TAction>`         | 2D rectangle bounds | partitions a 2D rectangle   |
|   [4]   | `ParallelHelper.For2D<TAction>`         | 2D region values    | partitions a 2D region      |
|   [5]   | `ParallelHelper.ForEach<TItem,TAction>` | `Memory<T>`         | mutates items by reference  |
|   [6]   | `ParallelHelper.ForEach<TItem,TAction>` | `ReadOnlyMemory<T>` | reads items by reference    |
|   [7]   | `ParallelHelper.ForEach<TItem,TAction>` | memory plane        | partitions 2D memory planes |

[ACTION_CONTRACTS]:
| [INDEX] | [SURFACE]              | [CALL_SHAPE]   | [CAPABILITY]             |
| :-----: | :--------------------- | :------------- | :----------------------- |
|   [1]   | `IAction.Invoke`       | 1D index       | handles one index slot   |
|   [2]   | `IAction2D.Invoke`     | 2D index pair  | handles one 2D slot      |
|   [3]   | `IInAction<T>.Invoke`  | read-only item | handles one input item   |
|   [4]   | `IRefAction<T>.Invoke` | mutable item   | handles one mutable item |

## [4]-[IMPLEMENTATION_LAW]

[STAGING_MEMORY]:
- namespaces: `CommunityToolkit.HighPerformance`, `CommunityToolkit.HighPerformance.Buffers`
- ownership: pooled owners are the staging lifetime boundary
- payload shape: spans and memory planes carry tensor, vector, codec, and remote buffers
- release rule: every owned buffer returns through deterministic disposal

[STAGING_STREAMS]:
- bridge: memory, memory owners, buffer writers, and read-only sequences become streams only through the `AsStream` extension family at IO edges
- implementation rule: stream implementation types are package-internal and never appear in Compute vocabulary
- text rule: pooled text belongs to staging receipts, not domain values

[PARALLEL_PARTITION]:
- partition root: `ParallelHelper` static methods `For`, `For2D`, `ForEach`
- work shape: callers pass a `struct` action implementing `IAction` / `IAction2D` / `IInAction<T>` / `IRefAction<T>`; the struct constraint keeps the invoker allocation-free and inlinable
- index forms: `For` takes `(int start, int end)` or `Range`; `For2D` takes `(top, bottom, left, right)`, `Rectangle`, or `(Range i, Range j)`
- item forms: `ForEach` partitions `Memory<T>` / `ReadOnlyMemory<T>` and the 2D plane variants `Memory2D<T>` / `ReadOnlyMemory2D<T>`
- granularity: `minimumActionsPerThread` lower-bounds work per thread; degree of parallelism clamps to `Environment.ProcessorCount`
- single-thread collapse: a partition count of one invokes the action inline on the calling thread

[BIT_FLAGS]:
- helper root: `BitHelper` static methods over `uint` and `ulong` words
- flag forms: `bool HasFlag(value, int n)`; `void SetFlag(ref value, int n, bool flag)` in-place and `value SetFlag(value, int n, bool flag)` value-returning
- range forms: `value ExtractRange(value, byte start, byte length)`; `void SetRange(ref value, byte start, byte length, flags)` in-place and value-returning, `flags` typed to the word width
- word width: every method has a `uint` and a `ulong` overload; the staging allocation-axis word set/has-flag column binds the `ulong` overloads
- Compute staging uses package memory shapes before introducing package-local payload owners.
- Ref carriers remain internal implementation material and never become public domain vocabulary.
- Parallel helper entrypoints require benchmark receipts before becoming a default execution path.
- Byte projections require explicit codec and endianness ownership at the calling rail.

[REJECTED]:
- `HashCode<T>.Combine` is not admitted: the suite identity-hashing surface is a single monopoly held by `System.IO.Hashing` — `XxHash128` for whole-artifact and content-address identity and `Crc32` for per-frame integrity (the `FrameEdge`/`InterchangeIdentity` owners). A second hashing helper for value-span digests would mint a parallel hash owner that fragments the content-address identity, so the `HashCode<T>` span-combine path stays out of the package and every value digest routes through the XxHash/Crc32 monopoly.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: pooled memory, span planes, ref views, stream projection
- Accept: bounded execution payload staging
- Reject: package-local span wrappers; `HashCode<T>.Combine` value-span hashing (XxHash128/Crc32 monopoly via `System.IO.Hashing`)
