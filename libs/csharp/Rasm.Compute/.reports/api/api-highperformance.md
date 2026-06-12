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

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :-------------------------- | :-------------- | :--------------------- |
|   [1]   | `ReadOnlySpanTokenizer<T>`  | tokenizer       | splits spans           |
|   [2]   | `SpanTokenizer<T>`          | tokenizer       | splits mutable spans   |
|   [3]   | `ReadOnlySpanEnumerable<T>` | item enumerable | enumerates with index  |
|   [4]   | `SpanEnumerable<T>`         | item enumerable | enumerates with index  |
|   [5]   | `ReadOnlyRefEnumerable<T>`  | ref enumerable  | enumerates by ref      |
|   [6]   | `RefEnumerable<T>`          | ref enumerable  | enumerates by ref      |
|   [7]   | `ParallelHelper`            | parallel root   | partitions batch work  |
|   [8]   | `IAction` / `IAction2D`     | action contract | defines indexed work   |
|   [9]   | `IInAction` / `IRefAction`  | action contract | defines item work      |
|  [10]   | `BitHelper`                 | bit helper      | packs bit flags        |
|  [11]   | `HashCode<T>`               | hash helper     | hashes value spans     |
|  [12]   | `ObjectMarshal`             | marshal helper  | reads object internals |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: allocation and ownership
- rail: staging

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]   | [CAPABILITY]           |
| :-----: | :----------------------------------- | :------------- | :--------------------- |
|   [1]   | `MemoryOwner<T>.Allocate`            | factory call   | rents owned memory     |
|   [2]   | `SpanOwner<T>.Allocate`              | factory call   | rents owned span       |
|   [3]   | `MemoryOwner<T>.Slice`               | instance call  | slices owned memory    |
|   [4]   | `ArrayPoolExtensions.Resize`         | extension call | resizes rented array   |
|   [5]   | `ArrayPoolExtensions.EnsureCapacity` | extension call | grows rented array     |
|   [6]   | `StringPool.GetOrAdd`                | pool call      | interns text payload   |
|   [7]   | `Dispose`                            | lifetime call  | returns pooled storage |

[ENTRYPOINT_SCOPE]: projections and transforms
- rail: staging

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :----------------------------------------- | :------------- | :---------------------- |
|   [1]   | `AsSpan2D`                                 | extension call | projects plane view     |
|   [2]   | `AsMemory2D`                               | extension call | projects memory plane   |
|   [3]   | `GetRow` / `GetColumn`                     | extension call | enumerates plane axis   |
|   [4]   | `GetRowSpan`                               | extension call | addresses plane row     |
|   [5]   | `DangerousGetReference`                    | extension call | exposes ref root        |
|   [6]   | `AsBytes`                                  | extension call | projects raw bytes      |
|   [7]   | `Cast`                                     | extension call | reinterprets span bytes |
|   [8]   | `Tokenize`                                 | extension call | splits span payloads    |
|   [9]   | `Enumerate`                                | extension call | exposes ref enumeration |
|  [10]   | `AsStream`                                 | extension call | projects payload stream |
|  [11]   | `StreamExtensions.Read` / `Write`          | extension call | moves span payloads     |
|  [12]   | `ParallelHelper.For` / `For2D` / `ForEach` | helper call    | partitions span work    |

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

[LOCAL_ADMISSION]:
- Compute staging uses package memory shapes before introducing package-local payload owners.
- Ref carriers remain internal implementation material and never become public domain vocabulary.
- Parallel helper entrypoints require benchmark receipts before becoming a default execution path.
- Byte projections require explicit codec and endianness ownership at the calling rail.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: pooled memory, span planes, ref views, stream projection
- Accept: bounded execution payload staging
- Reject: package-local span wrappers
