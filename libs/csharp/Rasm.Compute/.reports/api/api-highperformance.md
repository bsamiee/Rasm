# [RASM_COMPUTE_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies span, memory, pooling, stream, ref,
and buffer primitives for measured staging payloads.

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

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :------------------------- | :------------- | :----------------------- |
|   [1]   | `Span2D<T>`                | span view      | addresses dense planes   |
|   [2]   | `ReadOnlySpan2D<T>`        | span view      | reads dense planes       |
|   [3]   | `Memory2D<T>`              | memory view    | carries owned planes     |
|   [4]   | `ReadOnlyMemory2D<T>`      | memory view    | carries read-only planes |
|   [5]   | `Ref<T>`                   | ref carrier    | preserves ref access     |
|   [6]   | `ReadOnlyRef<T>`           | ref carrier    | preserves read refs      |
|   [7]   | `NullableRef<T>`           | ref carrier    | carries optional refs    |
|   [8]   | `NullableReadOnlyRef<T>`   | ref carrier    | carries optional refs    |
|   [9]   | `MemoryOwner<T>`           | pooled owner   | owns rented memory       |
|  [10]   | `SpanOwner<T>`             | pooled owner   | owns rented span memory  |
|  [11]   | `ArrayPoolBufferWriter<T>` | pooled writer  | writes staged payloads   |
|  [12]   | `MemoryBufferWriter<T>`    | memory writer  | writes staged payloads   |

[PUBLIC_TYPE_SCOPE]: stream and enumerable shapes
- rail: staging

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]               |
| :-----: | :-------------------------- | :------------- | :------------------------- |
|   [1]   | `IBufferWriterStream`       | stream bridge  | exposes writer as stream   |
|   [2]   | `IMemoryOwnerStream`        | stream bridge  | exposes owner as stream    |
|   [3]   | `ReadOnlySequenceStream`    | stream bridge  | exposes sequence as stream |
|   [4]   | `ArrayOwner<T>`             | buffer owner   | owns rented arrays         |
|   [5]   | `ArrayBufferWriterOwner<T>` | buffer owner   | owns writer payload        |
|   [6]   | `ReadOnlySpanTokenizer<T>`  | tokenizer      | splits spans               |
|   [7]   | `SpanTokenizer<T>`          | tokenizer      | splits mutable spans       |
|   [8]   | `ReadOnlyRefEnumerable<T>`  | ref enumerable | enumerates by ref          |
|   [9]   | `RefEnumerable<T>`          | ref enumerable | enumerates by ref          |
|  [10]   | `StringPool`                | text pool      | interns staged text        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: allocation and ownership
- rail: staging

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]   | [CAPABILITY]           |
| :-----: | :------------------------ | :------------- | :--------------------- |
|   [1]   | `MemoryOwner<T>.Allocate` | factory call   | rents owned memory     |
|   [2]   | `SpanOwner<T>.Allocate`   | factory call   | rents owned span       |
|   [3]   | `MemoryOwner<T>.Wrap`     | factory call   | wraps existing memory  |
|   [4]   | `ArrayPool<T>.Rent`       | extension call | rents array payload    |
|   [5]   | `IMemoryOwner<T>.Slice`   | extension call | slices owned memory    |
|   [6]   | `Dispose`                 | lifetime call  | returns pooled storage |

[ENTRYPOINT_SCOPE]: projections and transforms
- rail: staging

| [INDEX] | [SURFACE]               | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :---------------------- | :------------- | :---------------------- |
|   [1]   | `AsSpan2D`              | extension call | projects plane view     |
|   [2]   | `AsMemory2D`            | extension call | projects memory plane   |
|   [3]   | `GetRowSpan`            | instance call  | addresses plane row     |
|   [4]   | `DangerousGetReference` | extension call | exposes ref root        |
|   [5]   | `AsBytes`               | extension call | projects raw bytes      |
|   [6]   | `Cast<TFrom,TTo>`       | extension call | reinterprets span bytes |
|   [7]   | `Tokenize`              | extension call | splits span payloads    |
|   [8]   | `Enumerate`             | extension call | exposes ref enumeration |
|   [9]   | `CopyToAsync`           | stream call    | copies staged stream    |
|  [10]   | `ParallelHelper.For`    | helper call    | partitions span work    |

## [4]-[IMPLEMENTATION_LAW]

[STAGING_MEMORY]:
- namespaces: `CommunityToolkit.HighPerformance`, `CommunityToolkit.HighPerformance.Buffers`
- ownership: pooled owners are the staging lifetime boundary
- payload shape: spans and memory planes carry tensor, vector, codec, and remote buffers
- release rule: every owned buffer returns through deterministic disposal

[STAGING_STREAMS]:
- namespaces: `CommunityToolkit.HighPerformance.Streams`
- bridge: buffer writers and memory owners become stream-compatible only at IO edges
- sequence rule: `ReadOnlySequenceStream` carries fragmented payloads without flattening
- text rule: pooled text belongs to staging receipts, not domain values

[LOCAL_ADMISSION]:
- Compute staging uses package memory shapes before introducing package-local payload owners.
- Ref carriers remain internal implementation material and never become public domain vocabulary.
- Parallel helper entrypoints require benchmark receipts before becoming a default execution path.
- Byte projections require explicit codec and endianness ownership at the calling rail.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: pooled memory, span planes, ref views, stream bridges
- Accept: bounded execution payload staging
- Reject: package-local span wrappers
