# [COMPUTE_STAGING_AND_STREAMS]

Staging memory for every payload that moves through Rasm.Compute between intent admission and the IO edges.
The page owns the `AllocationClass` axis with its admission predicate and its bit-pack, in-place-growth, and
span-tokenizing routes, the `AllocationEvidence` record every staging receipt carries, the bare plane-projection
law over CommunityToolkit.HighPerformance views, and the one-per-process recyclable stream pool with its policy
record, zero-allocation event-fold evidence, and zero-copy contiguous and segment handoff to the remote frame
edge. `CorrelationId`/`ReceiptSinkPort` bind identity and emission; pooled owners are the lifetime boundary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                        |
| :-----: | :-------------- | :------------------------------------------------------------ |
|   [1]   | ALLOCATION_AXIS | Five-row staging axis; admission predicate; evidence record   |
|   [2]   | PLANE_VIEWS     | Bare plane projections; reinterpretation law; layout split    |
|   [3]   | STREAM_POOL     | One pooled stream manager; policy record; event-fold evidence |

## [2]-[ALLOCATION_AXIS]

- Owner: `AllocationClass` `[SmartEnum<string>]` five rows under the `StagingKeyPolicy` ordinal accessor; `AllocationEvidence` is the evidence record every staging grant stamps.
- Cases: `SpanStack`, `PooledMemory`, `RecyclableStream`, `NativeOrt`, `EdgeCopy`.
- Entry: `bool Admits(long requestedBytes, long payloadBound, Option<string> copyReason = default)`.
- Auto: intent admission evaluates `Admits` once against the intent-declared payload bound; every grant stamps `AllocationEvidence` under the intent correlation with zero call-site accounting.
- Receipt: `AllocationEvidence` — correlation, class row, requested and granted bytes, copy reason, native allocator slots; it materializes at the receipt sink edge from hot-path structs.
- Packages: CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one `AllocationClass` row with its predicate values; a cap change is one policy value; zero new surface.
- Boundary: the class is intent-declared data, never a call-site choice; a false `Admits` folds to the `ComputeFault` rail at the admission edge as the allocation-over-class case; the sync-only row gates async lanes because a `SpanOwner<T>` never escapes its declaring scope; `MemoryOwner<T>`/`SpanOwner<T>` are the lifetime boundary composed bare while `Ref<T>` carriers and `DangerousGetReference` stay kernel-internal and `DangerousGetArray` is the ArraySegment-handoff seam for the tensor-lane rented-array factory and the `StreamPool` zero-copy byte handoff; `BitHelper.HasFlag`/`SetFlag`/`ExtractRange`/`SetRange` pack voxel-occupancy and symbolic-dimension masks over the `PooledMemory` row so a `VoxelGrid` payload carries one bit per cell instead of one byte — the branchless members operate on a `ulong` word the `StagingViews` extension writes back through a `ref` slot, never a hand-rolled shift-and-test; `SpanTokenizer<T>`/`ReadOnlySpanTokenizer<T>` complete the codec-text split begun at `Tokenize` so receipt and codec spans partition without intermediate strings; `ArrayPoolExtensions.Resize`/`EnsureCapacity` grow a rented array in place during incremental codec emit so the `PooledMemory` and `RecyclableStream` rows never reallocate through a second rent; the native row's allocator slots receive their values from the model lane's allocator read; `HashCode<T>.Combine` over a flattened staging span is the rejected content-key route here — content hashing rides the suite XxHash owner (`XxHash3`/`XxHash128`) at the intent and model lanes, never a second staging-local hash; this axis deletes per-call-site pool choices, naked `ArrayPool<T>.Shared` rents, `byte`-per-cell occupancy buffers, intermediate split strings, `System.IO.Pipelines` admission, `HashCode<T>` content keys, and every unowned buffer type without a row.

```csharp signature
public sealed class StagingKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StagingKeyPolicy, string>]
[KeyMemberComparer<StagingKeyPolicy, string>]
public sealed partial class AllocationClass {
    public static readonly AllocationClass SpanStack = new("span-stack", syncOnly: true, copyReceipted: false);
    public static readonly AllocationClass PooledMemory = new("pooled-memory", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass RecyclableStream = new("recyclable-stream", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass NativeOrt = new("native-ort", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass EdgeCopy = new("edge-copy", syncOnly: false, copyReceipted: true);

    public bool SyncOnly { get; }

    public bool CopyReceipted { get; }

    public bool Admits(long requestedBytes, long payloadBound, Option<string> copyReason = default) =>
        requestedBytes <= payloadBound && (!CopyReceipted || copyReason.IsSome);
}

public readonly record struct AllocationEvidence(
    CorrelationId Correlation,
    AllocationClass Class,
    long RequestedBytes,
    long GrantedBytes,
    Option<string> CopyReason = default,
    Option<string> NativeAllocator = default,
    Option<long> NativeReservedBytes = default);
```

| [INDEX] | [ROUTE]           | [RULING]                                                                                                                                                                                                                                                                                                                        |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | stack rent        | `SpanOwner<T>.Allocate(int, AllocationMode)` inside one synchronous kernel scope; the owner never crosses an await or an iterator boundary                                                                                                                                                                                      |
|   [2]   | pooled rent       | `MemoryOwner<T>.Allocate(int)` with `AllocationMode.Default`; `Slice` projects windows; `Dispose` returns deterministically                                                                                                                                                                                                     |
|   [3]   | incremental build | `ArrayPoolBufferWriter<T>`/`MemoryBufferWriter<T>` own growing payloads; the writer is the `IBufferWriter<T>` seam for codec emit                                                                                                                                                                                               |
|   [4]   | rent clearing     | `AllocationMode.Default` everywhere — upstream classification enforcement keeps secret payloads out of staging, so clearing buys nothing                                                                                                                                                                                        |
|   [5]   | native evidence   | native grants happen inside the model lane; `NativeAllocator` and `NativeReservedBytes` slots carry the allocator name and reserved byte count                                                                                                                                                                                  |
|   [6]   | edge copy         | a grant on the copy-receipted row carries a reason; every array materialization and stream flatten routes through it                                                                                                                                                                                                            |
|   [7]   | text interning    | `StringPool.GetOrAdd` interns receipt and diagnostic text at the sink edge only; `Tokenize` splits codec text spans without intermediate strings                                                                                                                                                                                |
|   [8]   | bit packing       | `BitHelper.HasFlag(ulong, int)`/`SetFlag(ulong, int, bool)`/`ExtractRange(ulong, byte, byte)` own voxel-occupancy and symbolic-dimension bit-flag packing over a `Span<ulong>` window of `PooledMemory` (sixty-four cells per word); one bit per cell replaces a byte buffer through the branchless members the `StagingViews` fence carries and the encoded mask stages for the tensor-lane `VoxelGrid` encoding |
|   [9]   | in-place growth   | `ArrayPoolExtensions.Resize`/`EnsureCapacity` grow the rented backing array during incremental codec emit; the writer never reallocates through a second `MemoryOwner<T>.Allocate` and the granted-byte slot reflects the grown capacity                                                                                        |
|  [10]   | span tokenizing   | `ReadOnlySpanTokenizer<T>` partitions a codec or diagnostic span by separator without materializing intermediate strings; `SpanTokenizer<T>` rewrites a mutable staged span in the same pass
|  [11]   | contiguous frame  | `RecyclableMemoryStreamManager.GetStream(tag, requiredSize, asContiguousBuffer: true)` forces one large-buffer allocation for a chunked tensor frame the tensor lane requires contiguous; the grant rides the `RecyclableStream` row, the `requiredSize` fills the granted-byte slot, and the route replaces a hand-rolled array concatenation of chunk frames                                                                                                                                    |

```csharp signature
public static class StagingViews {
    extension(MemoryOwner<ulong> mask) {
        public bool Cell(int index) => BitHelper.HasFlag(mask.Span[index >> 6], index & 63);

        public void Mark(int index) {
            ref ulong word = ref mask.Span[index >> 6];
            word = BitHelper.SetFlag(word, index & 63, true);
        }

        public void Clear(int index) {
            ref ulong word = ref mask.Span[index >> 6];
            word = BitHelper.SetFlag(word, index & 63, false);
        }

        public ulong Range(int start, byte length) => BitHelper.ExtractRange(mask.Span[start >> 6], (byte)(start & 63), length);
    }

    public static Span<byte> Grow(this ArrayPool<byte> pool, ref byte[]? backing, int capacity) {
        pool.EnsureCapacity(ref backing, capacity);
        return backing.AsSpan(0, capacity);
    }

    extension(ReadOnlySpan<byte> codecText) {
        public ReadOnlySpanTokenizer<byte> Fields(byte separator) => codecText.Tokenize(separator);
    }
}
```

## [3]-[PLANE_VIEWS]

- Owner: `Span2D<T>`/`ReadOnlySpan2D<T>`/`Memory2D<T>`/`ReadOnlyMemory2D<T>` composed bare — the projection extensions are the whole surface and zero local plane owner exists.
- Entry: `Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int height, int width)`.
- Packages: CommunityToolkit.HighPerformance
- Growth: one projection row per new staged plane shape; a padded stride is one policy value; zero new surface.
- Boundary: planes are views, never layout — rank permutation stays tensor-lane layout algebra and a plane never substitutes for it; `Cast`/`AsBytes` reinterpretation is legal only inside the rail that owns the codec and its byte order, and reinterpreted bytes never cross a process boundary uncoded; model image pre/post stages through planes over `MemoryOwner<byte>` memory with zero intermediate arrays; this cluster deletes package-local plane wrappers, 1D index arithmetic over image rows, and copy-shaped pre/post buffers.

```csharp signature
public static Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int height, int width);
public static Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int offset, int height, int width, int pitch);
public static Span2D<T> AsSpan2D<T>(this Span<T> span, int height, int width);
public static Span<byte> AsBytes<T>(this Span<T> span) where T : unmanaged;
public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span) where TFrom : unmanaged where TTo : unmanaged;
```

| [INDEX] | [LAW]            | [RULING]                                                                                                                         |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | projection       | `AsMemory2D`/`AsSpan2D` give height-by-width planes over rented memory; the pitch overload carries padded image strides uncopied |
|   [2]   | row kernel       | `GetRowSpan(int row)` on `Span2D<T>` addresses one contiguous row; kernels fold row spans instead of indexing a flat buffer      |
|   [3]   | reinterpretation | `Cast` and `AsBytes` reinterpret in place under the calling codec's endianness ownership; a foreign-endian payload decodes first |
|   [4]   | layout split     | NCHW-to-NHWC and every rank permute ride the tensor-lane layout family; planes stage rows for kernels, never reorder dimensions  |

## [4]-[STREAM_POOL]

- Owner: `StreamPool` boundary capsule owning the one process `RecyclableMemoryStreamManager`; `StreamPoolPolicy` carries every pool policy value.
- Entry: `RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default, bool contiguous = false)`.
- Auto: the constructor attaches all eleven manager events and every event projects into `AllocationEvidence` riding `ReceiptSinkPort` `Send` with zero call-site code; `Get` passes the correlation as the stream id and the `RecyclableStream` row key as the tag, so every later event rejoins its intent.
- Receipt: double-dispose, finalization, and discarded-buffer events are leak diagnostics, never log noise; an array-conversion event without a matching copy reason is the named call-site defect.
- Packages: Microsoft.IO.RecyclableMemoryStream, CommunityToolkit.HighPerformance, Google.Protobuf, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one policy value on `StreamPoolPolicy`; a new evidence slot is one `AllocationEvidence` field; zero new surface.
- Boundary: `StreamPool` is the named boundary capsule for the statement carve-out — the constructor's manager creation, eleven-event wiring, and detacher collection carry language-owned statement forms, and the `PoolEvidence.Project` attach pair and `StagingViews.Grow` ref-array growth are the two further platform-forced statement seams (event add/remove and a `ref T[]` mutation the package returns through `void`) while every other member stays expression-shaped; `PoolEvidence.Project` is the foreign-receiver/local-behavior extension form — the AppHost `ReceiptSinkPort` is the foreign receiver read only to stamp evidence while the subscription-value detacher holding the exact handler identity is Compute-owned behavior, so the block adds no second disposer registry and mutates no port state; one capsule per process composed as a singleton contribution at the app root, and the `Diagnostic` policy row binds on debug and test-host profile rows; memory, owners, writers, and sequences become streams only through the `AsStream` extension family at IO edges — the package-internal stream classes never enter vocabulary; `GetBuffer` serves the contiguous-codec read past `MaximumBufferSize`, `DangerousGetArray` hands the rented segment to `UnsafeByteOperations.UnsafeWrap` for the zero-copy `ByteString`, and `BlockAndOffset`/`BlockSegment` stay on the `Diagnostic` policy row; `GetReadOnlySequence` is the zero-copy chunked read every tensor-frame and model-stream consumer takes — the model-lane `Chunked` and `GenerativeRun` token-stream paths read sequence segments through it without flattening, and `Get(correlation, requiredSize, contiguous: true)` forces one large pooled buffer through the `asContiguousBuffer:true` overload only for the tensor frame the tensor lane requires contiguous; this capsule deletes per-call-site manager instances, raw `MemoryStream` construction, copy-shaped `ByteString.CopyFrom`, and unreceipted `ToArray` flattens.

```csharp signature
public sealed record StreamPoolPolicy(
    int BlockSize,
    int LargeBufferMultiple,
    int MaximumBufferSize,
    long MaximumSmallPoolFreeBytes,
    long MaximumLargePoolFreeBytes,
    long MaximumStreamCapacity,
    bool UseExponentialLargeBuffer,
    bool AggressiveBufferReturn,
    bool ZeroOutBuffer,
    bool GenerateCallStacks,
    bool ThrowExceptionOnToArray) {
    public static readonly StreamPoolPolicy Canonical = new(
        BlockSize: 131072,
        LargeBufferMultiple: 1048576,
        MaximumBufferSize: 4194304,
        MaximumSmallPoolFreeBytes: 16777216,
        MaximumLargePoolFreeBytes: 33554432,
        MaximumStreamCapacity: 0,
        UseExponentialLargeBuffer: false,
        AggressiveBufferReturn: true,
        ZeroOutBuffer: false,
        GenerateCallStacks: false,
        ThrowExceptionOnToArray: false);

    public static readonly StreamPoolPolicy Diagnostic = Canonical with { GenerateCallStacks = true, ThrowExceptionOnToArray = true };

    public RecyclableMemoryStreamManager.Options Options => new() {
        BlockSize = BlockSize,
        LargeBufferMultiple = LargeBufferMultiple,
        MaximumBufferSize = MaximumBufferSize,
        MaximumSmallPoolFreeBytes = MaximumSmallPoolFreeBytes,
        MaximumLargePoolFreeBytes = MaximumLargePoolFreeBytes,
        MaximumStreamCapacity = MaximumStreamCapacity,
        UseExponentialLargeBuffer = UseExponentialLargeBuffer,
        AggressiveBufferReturn = AggressiveBufferReturn,
        ZeroOutBuffer = ZeroOutBuffer,
        GenerateCallStacks = GenerateCallStacks,
        ThrowExceptionOnToArray = ThrowExceptionOnToArray,
    };
}

public sealed class StreamPool : IDisposable {
    readonly RecyclableMemoryStreamManager manager;
    readonly Seq<Action> detachers;

    public StreamPool(StreamPoolPolicy policy, ReceiptSinkPort sink);

    public RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default, bool contiguous = false) =>
        requiredSize is { IsSome: true, Case: long size }
            ? contiguous
                ? manager.GetStream($"{AllocationClass.RecyclableStream.Key}/{correlation}", size, asContiguousBuffer: true)
                : manager.GetStream(correlation, AllocationClass.RecyclableStream.Key, size)
            : manager.GetStream(correlation, AllocationClass.RecyclableStream.Key);

    public void Dispose() => detachers.Rev().Iter(static detach => detach());
}
```

| [INDEX] | [EVENT]                | [EVIDENCE]                                                                                                               |
| :-----: | :--------------------- | :----------------------------------------------------------------------------------------------------------------------- |
|   [1]   | StreamCreated          | `RequestedSize`/`ActualSize` fill requested and granted bytes under the stream's correlation `Id`                        |
|   [2]   | StreamDisposed         | `Lifetime` closes the grant; the dispose pairs its create by `Id`                                                        |
|   [3]   | StreamLength           | `Length` at dispose fills the staged-payload size distribution corroborating granted-byte evidence                       |
|   [4]   | StreamDoubleDisposed   | leak diagnostic — `DisposeStack1`/`DisposeStack2` carry call sites on the `Diagnostic` policy row                        |
|   [5]   | StreamFinalized        | leak diagnostic — an undisposed stream reached the finalizer; `AllocationStack` names the renter when call stacks are on |
|   [6]   | StreamConvertedToArray | edge-copy corroboration — `Length` joins the receipted copy reason; a conversion without one is the named defect         |
|   [7]   | StreamOverCapacity     | `RequestedCapacity`/`MaximumCapacity` fill bound evidence                                                                |
|   [8]   | BlockCreated           | `SmallPoolInUse` tracks pool growth                                                                                      |
|   [9]   | LargeBufferCreated     | `Pooled`/`RequiredSize`/`LargePoolInUse` surface unpooled grants the moment they happen                                  |
|  [10]   | BufferDiscarded        | `Reason` carries the discard taxonomy                                                                                    |
|  [11]   | UsageReport            | the four pool byte gauges feed steady-state staging telemetry                                                            |

| [INDEX] | [LAW]            | [RULING]                                                                                                                                                                                                                        |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | fragmented read  | `GetReadOnlySequence` is the default read of staged bytes; segments map one-to-one onto pooled blocks (single-block and large-buffer streams collapse to one segment) and wire encode and decode never flatten the payload      |
|   [2]   | zero-copy edge   | the remote edge wraps sequence windows with `UnsafeByteOperations.UnsafeWrap` under the frame law the remote lane owns                                                                                                          |
|   [3]   | codec window     | `TryGetBuffer` exposes a contiguous window for codecs bounded by `MaximumBufferSize`; `WriteTo` is the array-free stream-to-stream copy                                                                                         |
|   [4]   | block alignment  | `BlockSize` holds exactly two `ArtifactSync` frames, so a frame never straddles a pooled block                                                                                                                                  |
|   [5]   | payload cap      | `MaximumBufferSize` equals the wire payload cap from the canonical channel policy; large buffers step in 1 MiB multiples to that cap                                                                                            |
|   [6]   | stream cap       | `MaximumStreamCapacity` zero is the package no-limit spelling; per-intent payload bounds own staging caps at admission                                                                                                          |
|   [7]   | pool retention   | the free-bytes caps pin retention to 128 pooled blocks and eight payload-cap buffers; returns beyond them release as discard events                                                                                             |
|   [8]   | contiguous view  | `GetBuffer` exposes the whole stream as one array when the codec needs a contiguous backing past `MaximumBufferSize`; the call is array-free against pooled blocks and never copies, where `TryGetBuffer` caps at one block     |
|   [9]   | segment handoff  | `MemoryOwner<byte>.DangerousGetArray` hands the rented `ArraySegment<byte>` to `UnsafeByteOperations.UnsafeWrap` so a pooled payload becomes a `ByteString` with zero copy; the owner outlives the wrap and disposes after send |
|  [10]   | block diagnostic | `BlockAndOffset`/`BlockSegment` address pooled-block boundaries on the `Diagnostic` policy row so a frame-straddle assertion reads exact block positions; production reads only `GetReadOnlySequence` segment counts            |

The eleven manager events fold to evidence through one `ReceiptSinkPort.Send` per event with no per-event allocation: each handler closure captures the sink once at construction, projects its `EventArgs` to an `AllocationEvidence` value, serializes that value to the `JsonElement` payload the 4-argument `Send` carries, and the detacher chain detaches LIFO at dispose. The drop-callback the channel rows pass for the `DropWrite`/`DropOldest` lanes routes a dropped item through the same projection rather than a per-drop allocation, so a dropped payload is an `AllocationEvidence` stamp under the renter correlation, never silent loss.

```csharp signature
public static class PoolEvidence {
    extension(ReceiptSinkPort sink) {
        public Action Project<TArgs>(
            Action<EventHandler<TArgs>> add,
            Action<EventHandler<TArgs>> remove,
            JsonSerializerOptions wire,
            Func<TArgs, AllocationEvidence> evidence) where TArgs : EventArgs {
            EventHandler<TArgs> handler = (_, args) => ignore(Stamp(sink, wire, evidence(args)).Run());
            add(handler);
            return () => remove(handler);
        }
    }

    static IO<ReceiptEnvelope> Stamp(ReceiptSinkPort sink, JsonSerializerOptions wire, AllocationEvidence e) =>
        sink.Send(e.Correlation, "Rasm.Compute", e.Class.Key, JsonSerializer.SerializeToElement(e, wire));
}
```

## [5]-[RESEARCH]

- [POOL_EVIDENCE]: bound `RecyclableMemoryStreamManager` event-arg field availability across single-block and large-buffer streams (`AllocationStack` populated only on the `GenerateCallStacks` policy row) for the zero-allocation projection.
