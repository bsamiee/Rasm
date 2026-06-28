# [RASM_PERSISTENCE_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies the zero-copy buffer/stream substrate the `Store/remote` object-store rail and the `Version/snapshots` codec compose: the `AsStream()` family that wraps a `Memory<byte>` / `ReadOnlyMemory<byte>` / `ReadOnlySequence<byte>` / `IMemoryOwner<byte>` / `IBufferWriter<byte>` as a `System.IO.Stream` with NO intermediate `byte[]` copy (the body handed to `AmazonS3Client`/`BlobContainerClient`/`StorageClient`/`Minio` PUT and returned from GET), the `IBufferWriterExtensions.Write` sink the snapshot codecs drain into, and the pooled `ArrayPoolBufferWriter<T>` / `MemoryOwner<T>` / `SpanOwner<T>` rentals that replace managed-heap drain buffers. It is the cross-cutting high-performance BCL substrate co-consumed with `Rasm.Compute`; the `Store/remote#OBJECT_IO` `Drain` already composes `IBufferWriterExtensions.AsStream` over an `ArrayBufferWriter<byte>` and the per-part slice legs ride the same bridge. The content-key digest is NOT a HighPerformance member — `Store/remote` keys identity off `System.IO.Hashing` `XxHash128` (`api-hashing.md`); the HighPerformance `HashCode<T>` / `GetDjb2HashCode` helpers are fast NON-cryptographic hashes for in-process maps only.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`
- package: `CommunityToolkit.HighPerformance`
- version: `8.4.2`
- license: MIT (`licenses.nuget.org/MIT` — .NET Foundation / Microsoft)
- assembly: `CommunityToolkit.HighPerformance`
- namespace: `CommunityToolkit.HighPerformance`, `.Buffers`, `.Enumerables`, `.Helpers`, `.Streams`
- target frameworks: `net8.0`, `netstandard2.1`, `netstandard2.0`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. The `net10.0` consumer binds the highest asset `lib/net8.0`; the `net8.0` surface lights up the `nint`-offset `DangerousGetReferenceAt`, `ref readonly` span overloads, and the `INumber`-free vectorized paths the netstandard fallbacks polyfill.
- rail: store-remote, snapshots (PERF substrate)
- ABI floor: a stable 8.x line — the `AsStream`/`IBufferWriter`/`MemoryOwner`/`Span2D` surface is settled across the 8.x minors. Centrally pinned (`Directory.Packages.props`) and co-consumed by `Rasm.Compute`; one pin, two folders.

The package is a SUBSTRATE row, not a domain owner: it carries no Persistence vocabulary. It supplies the byte-plumbing primitives the `Store/remote` `BlobRemote` placement and the `Version/snapshots` `SnapshotCodec` compose, and the 2D `Span2D<T>`/`Memory2D<T>` views `Rasm.Compute` frames ride. Every member below is the BCL-shaped primitive, mapped onto the canonical body/stream/buffer concept at the rail edge.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pooled buffers (`CommunityToolkit.HighPerformance.Buffers`)
- rail: store-remote

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                                                                                       |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `ArrayPoolBufferWriter<T>`     | buffer writer      | `: IBuffer<T>, IBufferWriter<T>, IMemoryOwner<T>, IDisposable` — `ArrayPool<T>`-backed growable writer; `WrittenMemory`/`WrittenSpan`/`WrittenCount`/`Capacity`/`FreeCapacity` |
|  [02]   | `MemoryOwner<T>`               | pooled memory      | `: IMemoryOwner<T>, IDisposable` — pooled `Memory<T>` rental with `Slice`; `Allocate(size[, pool][, mode])` |
|  [03]   | `SpanOwner<T>`                 | pooled span        | `readonly ref struct` — stack-scoped pooled `Span<T>` rental; `Allocate(size[, pool][, mode])`, `Dispose` returns the array |
|  [04]   | `MemoryBufferWriter<T>`        | buffer writer      | `IBufferWriter<T>` over a fixed pre-rented `Memory<T>` (no growth)                                  |
|  [05]   | `StringPool`                   | intern pool        | configurable thread-safe string interning; `Shared`, `GetOrAdd(ReadOnlySpan<char>|ReadOnlySpan<byte>+Encoding)` |
|  [06]   | `IBuffer<T>`                   | contract           | `: IBufferWriter<T>` — the writer+`WrittenMemory` marker the buffer types implement                |
|  [07]   | `AllocationMode`               | enum               | `Default` (uninitialized rental) / `Clear` (zeroed rental)                                         |

[PUBLIC_TYPE_SCOPE]: byte-view bridges and 2D views (root + `.Streams`)
- rail: store-remote, compute-frames

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                                                  |
| :-----: | :------------------------ | :-------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `StreamExtensions`        | static (root)   | `AsStream`/`Read`/`Write`/`ReadAsync`/`WriteAsync` over `Memory<byte>`-backed streams         |
|  [02]   | `IBufferWriterExtensions` | static (root)   | `AsStream(this IBufferWriter<byte>)` + `Write<T>` struct/span sink into a writer               |
|  [03]   | `IMemoryOwnerExtensions`  | static (root)   | `AsStream(this IMemoryOwner<byte>)`                                                            |
|  [04]   | `MemoryExtensions` / `ReadOnlyMemoryExtensions` | static (root) | `AsStream`, `AsBytes<T>`, `Cast<TFrom,TTo>`, `AsMemory2D<T>` over `Memory<T>`/`ReadOnlyMemory<T>` |
|  [05]   | `ReadOnlySequenceExtensions` | static (root) | `AsStream(this ReadOnlySequence<byte>)` — the multi-segment GET-body bridge                    |
|  [06]   | `Span2D<T>` / `ReadOnlySpan2D<T>` | ref struct | strided 2D view over contiguous memory (row/col/pitch); `GetRow`/`GetColumn`/`Slice`/`Fill`   |
|  [07]   | `Memory2D<T>` / `ReadOnlyMemory2D<T>` | readonly struct | heap-storable 2D view; `Span`/`Slice`/`TryGetMemory`                                       |
|  [08]   | `Ref<T>` / `ReadOnlyRef<T>` / `NullableRef<T>` / `NullableReadOnlyRef<T>` | ref struct | byref carriers for a single `T` (the `ref` field a method cannot return) |
|  [09]   | `Box<T>`                  | class           | typed view over a boxed value type; unbox-free mutation via `GetReference`                     |

[PUBLIC_TYPE_SCOPE]: span algebra and parallel kernels (root + `.Enumerables` + `.Helpers`)
- rail: compute-frames

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :---------------------------------------- | :----------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `SpanExtensions` / `ReadOnlySpanExtensions` | static (root)    | `AsBytes`/`Cast`/`AsSpan2D`/`DangerousGetReference[At]`/`Enumerate`/`Tokenize`/`Count`/`IndexOf`/`GetDjb2HashCode` |
|  [02]   | `RefEnumerable<T>` / `ReadOnlyRefEnumerable<T>` | ref struct    | strided ref enumeration (a column of a 2D buffer) with `CopyTo`/`TryCopyTo`/`Fill` |
|  [03]   | `SpanEnumerable<T>` / `ReadOnlySpanEnumerable<T>` | ref struct  | `(Index, Value)` index-paired span enumeration (the `foreach` with ordinal)        |
|  [04]   | `SpanTokenizer<T>` / `ReadOnlySpanTokenizer<T>` | ref struct   | allocation-free separator tokenization over a span                                 |
|  [05]   | `HashCode<T>`                             | struct (`.Helpers`)| `static int Combine(ReadOnlySpan<T>)` — vectorized non-crypto span hash (`T : notnull`) |
|  [06]   | `ParallelHelper`                          | static (`.Helpers`)| `For`/`For2D`/`ForEach` over `IAction`/`IAction2D`/`IInAction`/`IRefAction` value-type kernels |
|  [07]   | `BitHelper`                               | static (`.Helpers`)| `HasFlag`/`SetFlag`/`ExtractRange`/`SetRange` bit-flag primitives + SWAR byte search (`HasZeroByte`/`HasByteEqualTo`) |
|  [08]   | `ObjectMarshal`                           | static (`.Helpers`)| `DangerousGetObjectDataReferenceAt<T>` — raw object field access                     |
|  [09]   | `HashCodeExtensions` / `ArrayPoolExtensions` | static (root)  | `Add(ReadOnlySpan<T>)` HashCode fold; `EnsureCapacity`/`Resize` array-pool growth    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: zero-copy `AsStream` bridge — the object-store body
- rail: store-remote
- composition law: the body the cloud SDK PUT/GET moves is a `Stream`; `AsStream` wraps the already-materialized buffer with no copy, so the snapshot/chunk bytes flow to/from `Store/remote#BLOB_REMOTE` without a `new byte[]`.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                                          | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------- | :--------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `IBufferWriterExtensions.AsStream`                 | `Stream AsStream(this IBufferWriter<byte> writer)`    | the drain target (`ArrayBufferWriter<byte>`/`ArrayPoolBufferWriter<byte>`) as a write `Stream` — the `#OBJECT_IO` `Drain` bridge |
|  [02]   | `ReadOnlyMemoryExtensions.AsStream`                | `Stream AsStream(this ReadOnlyMemory<byte> memory)`   | a part-window slice as a read `Stream` for one multipart PUT part   |
|  [03]   | `ReadOnlySequenceExtensions.AsStream`              | `Stream AsStream(this ReadOnlySequence<byte> sequence)` | a multi-segment GET response as one read `Stream` with no flatten |
|  [04]   | `MemoryExtensions.AsStream`                        | `Stream AsStream(this Memory<byte> memory)`           | a writable rented buffer as a read/write `Stream`                  |
|  [05]   | `IMemoryOwnerExtensions.AsStream`                  | `Stream AsStream(this IMemoryOwner<byte> owner)`      | a `MemoryOwner<byte>` rental as a `Stream` (disposes the owner with the stream) |
|  [06]   | `StreamExtensions.AsStream`                        | `Stream AsStream(this ArrayPoolBufferWriter<byte>)`   | the pooled writer's written bytes as a read `Stream`               |

[ENTRYPOINT_SCOPE]: pooled drain buffers — the zero-alloc upgrade
- rail: store-remote
- composition law: `#OBJECT_IO` drains into a BCL `ArrayBufferWriter<byte>` today; `ArrayPoolBufferWriter<byte>` is the same `IBufferWriter<byte>`+`WrittenMemory` surface backed by `ArrayPool<T>`, so a large drain rents instead of reallocating the managed heap, and `MemoryOwner<byte>`/`SpanOwner<byte>` rent the per-chunk part slices.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]                                              | [CAPABILITY]                                                |
| :-----: | :---------------------------------------------- | :------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `new ArrayPoolBufferWriter<byte>(initialCapacity)` | constructor                                            | pooled growable drain target; `GetSpan`/`GetMemory`/`Advance` then `WrittenMemory`/`AsStream` |
|  [02]   | `.WrittenMemory` / `.WrittenSpan` / `.WrittenCount` | property                                              | the contiguous drained bytes (feeds the `FastCDC` chunker + content-key hash) |
|  [03]   | `MemoryOwner<byte>.Allocate(size, AllocationMode.Clear)` | static factory                                    | rent a pooled body; `.Memory`/`.Span`/`.Slice`; `Dispose` returns to pool |
|  [04]   | `SpanOwner<byte>.Allocate(size)`                | static factory                                           | stack-scoped pooled span for a transient part window (`using` returns it)   |
|  [05]   | `IBufferWriterExtensions.Write<T>`              | `void Write<T>(this IBufferWriter<byte>, ReadOnlySpan<T>) where T : unmanaged` | blit a primitive/struct span into the writer (the codec sink) |
|  [06]   | `StringPool.Shared.GetOrAdd`                    | `string GetOrAdd(ReadOnlySpan<char>)`                    | intern repeated parameter/tag keys without a per-row `string` alloc          |

[ENTRYPOINT_SCOPE]: stream read/write + async mirror
- rail: store-remote

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :----------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `StreamExtensions.Read`            | `int Read(this Stream stream, Span<byte> buffer)`                                           | span read (sync)                      |
|  [02]   | `StreamExtensions.Write`           | `void Write(this Stream stream, ReadOnlySpan<byte> buffer)`                                 | span write (sync)                     |
|  [03]   | `StreamExtensions.ReadAsync`       | `ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken = default)` | `ValueTask` async read mirror     |
|  [04]   | `StreamExtensions.WriteAsync`      | `ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken = default)` | `ValueTask` async write mirror   |

[ENTRYPOINT_SCOPE]: 2D views + parallel kernels — `Rasm.Compute` frames
- rail: compute-frames

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                                                 | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :-------------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `MemoryExtensions.AsMemory2D<T>`        | `Memory2D<T> AsMemory2D<T>(this Memory<T>, int offset, int height, int width, int pitch)` | a strided 2D view over a flat frame buffer  |
|  [02]   | `SpanExtensions.AsSpan2D<T>`            | `Span2D<T> AsSpan2D<T>(this Span<T>, int height, int width)`                 | the ref-struct 2D compute window                       |
|  [03]   | `MemoryExtensions.Cast<TFrom,TTo>`      | `Memory<TTo> Cast<TFrom,TTo>(this Memory<TFrom>) where … : unmanaged`        | reinterpret a `Memory<byte>` as `Memory<float>` (frame decode) |
|  [04]   | `ParallelHelper.For2D`                  | `For2D<TAction>(Rectangle area, in TAction action) where TAction : struct, IAction2D` | value-type parallel kernel over a 2D range (no delegate alloc) |
|  [05]   | `HashCode<T>.Combine`                   | `static int Combine(ReadOnlySpan<T> span) where T : notnull`                 | vectorized non-crypto span hash (in-process map key, NOT the content key) |

## [04]-[IMPLEMENTATION_LAW]

[BODY_RAIL]:
- The object-store body is a `Stream` at the SDK edge (`AmazonS3Client` `InitiateMultipartUploadRequest`/`UploadPartRequest` over a part `Stream`, `BlobContainerClient.StageBlockAsync(string, Stream, …)`, `StorageClient.UploadObjectAsync(…, Stream)`, `Minio` `PutObjectArgs.WithStreamData(Stream)`). HighPerformance's `AsStream` family wraps the ALREADY-materialized `ReadOnlyMemory<byte>` / `ReadOnlySequence<byte>` / `IBufferWriter<byte>` as that `Stream` with no copy, so the one `#OBJECT_IO` `Drain` buffer feeds the content-defined chunker, the content-key hash, AND the part PUTs off ONE allocation.
- The drain target is an `IBufferWriter<byte>` (`Store/remote#OBJECT_IO` uses the BCL `ArrayBufferWriter<byte>`; `ArrayPoolBufferWriter<byte>` is the pooled drop-in with the identical `IBufferWriter<byte>` + `WrittenMemory`/`WrittenSpan` surface). The snapshot codecs (`api-messagepack.md` `MessagePackSerializer.Serialize(IBufferWriter<byte>, …)`, `api-cbor.md` `CborWriter`, `api-zstd.md`) write into that writer; the same writer `.AsStream()` is the upload body. No `byte[]` between codec and socket.
- The download side: a multi-segment `ReadOnlySequence<byte>` (the SDK response body) `.AsStream()` decodes back through the snapshot codec; a ranged `Fetch` returns `IO<Stream>` directly.

[HASH_LANE_SEPARATION]:
- The content-address identity is `System.IO.Hashing` `XxHash128` -> `UInt128 ContentKey` (`Store/remote#OBJECT_IO`, `api-hashing.md`); the server-verified checksum is `ChecksumAlgorithm.XXHASH128`. HighPerformance's `HashCode<T>.Combine` and `SpanExtensions.GetDjb2HashCode` are FAST NON-CRYPTOGRAPHIC hashes for in-process dictionary keys and span bucketing ONLY — they are NEVER the content key, the dedup key, or a wire checksum.

[STACK]:
- remote-store seam: `Store/remote#OBJECT_IO.Drain(Stream)` = `new ArrayBufferWriter<byte>()` -> `stream.CopyTo(buffer.AsStream())` -> `buffer.WrittenMemory`; the part legs slice `WrittenMemory` windows and `.AsStream()` each for the multipart PUT, and `Fetch` returns the GET `ReadOnlySequence<byte>.AsStream()` — every byte-bridge a verified HighPerformance member (`api-highperformance` is the page's PERF spine alongside `AWSSDK.S3`/`Azure.Storage.Blobs`/`Google.Cloud.Storage.V1`).
- snapshot seam: the `Version/snapshots` `SnapshotCodec` serializes a `[ValueObject]`/`[SmartEnum]` graph into an `IBufferWriter<byte>` (`api-messagepack.md`/`api-cbor.md`) — the SAME writer the remote rail `.AsStream()`s for upload, so a committed snapshot is stored content-keyed with one buffer shared between codec and object store.
- compute-frame seam: `Span2D<T>`/`Memory2D<T>` + `ParallelHelper.For2D` give `Rasm.Compute` strided 2D views and delegate-free parallel kernels over flat frame buffers; `Cast<TFrom,TTo>` reinterprets a `Memory<byte>` artifact as `Memory<float>` without a copy — the same substrate pin, consumed by both folders.

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance` `8.4.2` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, stable 8.x ABI)
- Owns: the zero-copy `AsStream` body bridge, the `IBufferWriter<byte>` codec sink, the pooled `ArrayPoolBufferWriter<T>`/`MemoryOwner<T>`/`SpanOwner<T>` rentals, the 2D `Span2D<T>`/`Memory2D<T>` views, the `ParallelHelper` value-type kernels, and the fast non-crypto `HashCode<T>` helper
- Accept: a body/part/response wrapped as a `Stream` via `AsStream` at the `Store/remote` edge; a codec drain into an `IBufferWriter<byte>`; a pooled buffer rented for a transfer window; a 2D view over a `Rasm.Compute` frame
- Reject: a `new byte[]` copy where `AsStream`/`WrittenMemory` bridges the existing buffer; `HashCode<T>`/`GetDjb2HashCode` used as a content key, dedup key, or wire checksum (that is `XxHash128`, `api-hashing.md`); a managed-heap drain buffer where `ArrayPoolBufferWriter<byte>` rents; a delegate-allocating parallel loop where `ParallelHelper` takes a value-type `IAction`

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- This page carries `CommunityToolkit.HighPerformance` API facts only; the `BlobRemote` placement, the `ObjectClient` `[Union]`, the `MultipartTransfer` fold, the `ContentChunker`, and the `ContentKey` projection are owned at `Store/remote` and `Version/snapshots`.
- SUBSTRATE row: the package is centrally pinned and co-consumed by `Rasm.Compute`; this folder's `.api/` records the surface the Persistence rails compose, not a Persistence-only contract.
- Hash-lane law: identity/dedup/checksum is `XxHash128` (`api-hashing.md`); HighPerformance hashing is in-process-only. The two never substitute.
- Codec-lane law: HighPerformance supplies the `IBufferWriter<byte>` plumbing; the snapshot encoding is `api-messagepack.md`/`api-cbor.md`/`api-zstd.md`. This page never re-documents the codec surface.
