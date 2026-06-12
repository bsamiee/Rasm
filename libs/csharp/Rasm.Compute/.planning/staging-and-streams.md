# [COMPUTE_STAGING_AND_STREAMS]

Staging memory for every payload that moves through Rasm.Compute between intent admission and the IO edges.
The page owns the `AllocationClass` axis with its admission predicate, the `AllocationEvidence` record every
staging receipt carries, the bare plane-projection law over CommunityToolkit.HighPerformance views, and the
one-per-process recyclable stream pool with its policy record and event-fold evidence. `CorrelationId` and
`ReceiptSinkPort` bind identity and emission as settled vocabulary; pooled owners are the lifetime boundary;
`System.IO.Pipelines` stays out — the axis owns staging.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          |
| :-----: | :-------------- | :-------------------------------------------------------------- |
|   [1]   | ALLOCATION_AXIS | Five-row staging axis; admission predicate; evidence record     |
|   [2]   | PLANE_VIEWS     | Bare plane projections; reinterpretation law; layout split      |
|   [3]   | STREAM_POOL     | One pooled stream manager; policy record; event-fold evidence   |

## [2]-[ALLOCATION_AXIS]

- Owner: `AllocationClass` `[SmartEnum<string>]` five rows under the `StagingKeyPolicy` ordinal accessor; `AllocationEvidence` is the evidence record every staging grant stamps.
- Cases: `SpanStack`, `PooledMemory`, `RecyclableStream`, `NativeOrt`, `EdgeCopy`.
- Entry: `bool Admits(long requestedBytes, long payloadBound, Option<string> copyReason = default)`.
- Auto: intent admission evaluates `Admits` once against the intent-declared payload bound; every grant stamps `AllocationEvidence` under the intent correlation with zero call-site accounting.
- Receipt: `AllocationEvidence` — correlation, class row, requested and granted bytes, copy reason, native allocator slots; it materializes at the receipt sink edge from hot-path structs.
- Packages: CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one `AllocationClass` row with its predicate values; a cap change is one policy value; zero new surface.
- Boundary: the class is intent-declared data, never a call-site choice; a false `Admits` folds to the `ComputeFault` rail at the admission edge as the allocation-over-class case; the sync-only row gates async lanes because a `SpanOwner<T>` never escapes its declaring scope; `MemoryOwner<T>`/`SpanOwner<T>` are the lifetime boundary composed bare while `Ref<T>` carriers and `DangerousGetReference` stay kernel-internal and `DangerousGetArray` exists solely for the tensor-lane rented-array factory seam; the native row's allocator slots receive their values from the model lane's allocator read; this axis deletes per-call-site pool choices, naked `ArrayPool<T>.Shared` rents, `System.IO.Pipelines` admission, and every unowned buffer type without a row.

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

| [INDEX] | [ROUTE]           | [RULING]                                                                                                                                       |
| :-----: | :---------------- | :--------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | stack rent        | `SpanOwner<T>.Allocate(int, AllocationMode)` inside one synchronous kernel scope; the owner never crosses an await or an iterator boundary     |
|   [2]   | pooled rent       | `MemoryOwner<T>.Allocate(int)` with `AllocationMode.Default`; `Slice` projects windows; `Dispose` returns deterministically                    |
|   [3]   | incremental build | `ArrayPoolBufferWriter<T>`/`MemoryBufferWriter<T>` own growing payloads; the writer is the `IBufferWriter<T>` seam for codec emit              |
|   [4]   | rent clearing     | `AllocationMode.Default` everywhere — upstream classification enforcement keeps secret payloads out of staging, so clearing buys nothing       |
|   [5]   | native evidence   | native grants happen inside the model lane; `NativeAllocator` and `NativeReservedBytes` slots carry the allocator name and reserved byte count |
|   [6]   | edge copy         | a grant on the copy-receipted row carries a reason; every array materialization and stream flatten routes through it                           |
|   [7]   | text interning    | `StringPool.GetOrAdd` interns receipt and diagnostic text at the sink edge only; `Tokenize` splits codec text spans without intermediate strings |

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

| [INDEX] | [LAW]            | [RULING]                                                                                                                        |
| :-----: | :--------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | projection       | `AsMemory2D`/`AsSpan2D` give height-by-width planes over rented memory; the pitch overload carries padded image strides uncopied |
|   [2]   | row kernel       | `GetRowSpan(int row)` on `Span2D<T>` addresses one contiguous row; kernels fold row spans instead of indexing a flat buffer      |
|   [3]   | reinterpretation | `Cast` and `AsBytes` reinterpret in place under the calling codec's endianness ownership; a foreign-endian payload decodes first  |
|   [4]   | layout split     | NCHW-to-NHWC and every rank permute ride the tensor-lane layout family; planes stage rows for kernels, never reorder dimensions  |

## [4]-[STREAM_POOL]

- Owner: `StreamPool` boundary capsule owning the one process `RecyclableMemoryStreamManager`; `StreamPoolPolicy` carries every pool policy value.
- Entry: `RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default)`.
- Auto: the constructor attaches all eleven manager events and every event projects into `AllocationEvidence` riding `ReceiptSinkPort` `Send` with zero call-site code; `Get` passes the correlation as the stream id and the `RecyclableStream` row key as the tag, so every later event rejoins its intent.
- Receipt: double-dispose, finalization, and discarded-buffer events are leak diagnostics, never log noise; an array-conversion event without a matching copy reason is the named call-site defect.
- Packages: Microsoft.IO.RecyclableMemoryStream, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one policy value on `StreamPoolPolicy`; a new evidence slot is one `AllocationEvidence` field; zero new surface.
- Boundary: `StreamPool` is the named boundary capsule for the statement carve-out — the constructor's manager creation, eleven-event wiring, and detacher collection carry language-owned statement forms while every other member stays expression-shaped; one capsule per process composed as a singleton contribution at the app root, and the `Diagnostic` policy row binds on debug and test-host profile rows; memory, owners, writers, and sequences become streams only through the `AsStream` extension family at IO edges — the package-internal stream classes never enter vocabulary; this capsule deletes per-call-site manager instances, raw `MemoryStream` construction, and unreceipted `ToArray` flattens.

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

    public RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default) =>
        requiredSize is { IsSome: true, Case: long size }
            ? manager.GetStream(correlation, AllocationClass.RecyclableStream.Key, size)
            : manager.GetStream(correlation, AllocationClass.RecyclableStream.Key);

    public void Dispose() => detachers.Rev().Iter(static detach => detach());
}
```

| [INDEX] | [EVENT]                | [EVIDENCE]                                                                                                          |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------------- |
|   [1]   | StreamCreated          | `RequestedSize`/`ActualSize` fill requested and granted bytes under the stream's correlation `Id`                    |
|   [2]   | StreamDisposed         | `Lifetime` closes the grant; the dispose pairs its create by `Id`                                                    |
|   [3]   | StreamLength           | `Length` at dispose fills the staged-payload size distribution corroborating granted-byte evidence                   |
|   [4]   | StreamDoubleDisposed   | leak diagnostic — `DisposeStack1`/`DisposeStack2` carry call sites on the `Diagnostic` policy row                    |
|   [5]   | StreamFinalized        | leak diagnostic — an undisposed stream reached the finalizer; `AllocationStack` names the renter when call stacks are on |
|   [6]   | StreamConvertedToArray | edge-copy corroboration — `Length` joins the receipted copy reason; a conversion without one is the named defect     |
|   [7]   | StreamOverCapacity     | `RequestedCapacity`/`MaximumCapacity` fill bound evidence                                                            |
|   [8]   | BlockCreated           | `SmallPoolInUse` tracks pool growth                                                                                  |
|   [9]   | LargeBufferCreated     | `Pooled`/`RequiredSize`/`LargePoolInUse` surface unpooled grants the moment they happen                              |
|  [10]   | BufferDiscarded        | `Reason` carries the discard taxonomy                                                                                |
|  [11]   | UsageReport            | the four pool byte gauges feed steady-state staging telemetry                                                        |

| [INDEX] | [LAW]           | [RULING]                                                                                                                            |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | fragmented read | `GetReadOnlySequence` is the default read of staged bytes; wire encode and decode ride its segments and never flatten the payload     |
|   [2]   | zero-copy edge  | the remote edge wraps sequence windows with `UnsafeByteOperations.UnsafeWrap` under the frame law the remote lane owns                |
|   [3]   | codec window    | `TryGetBuffer` exposes a contiguous window for codecs bounded by `MaximumBufferSize`; `WriteTo` is the array-free stream-to-stream copy |
|   [4]   | block alignment | `BlockSize` holds exactly two `ArtifactSync` frames, so a frame never straddles a pooled block                                        |
|   [5]   | payload cap     | `MaximumBufferSize` equals the wire payload cap from the canonical channel policy; large buffers step in 1 MiB multiples to that cap  |
|   [6]   | stream cap      | `MaximumStreamCapacity` zero is the package no-limit spelling; per-intent payload bounds own staging caps at admission                |
|   [7]   | pool retention  | the free-bytes caps pin retention to 128 pooled blocks and eight payload-cap buffers; returns beyond them release as discard events   |

## [5]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                       | [PROOF]                                                                                                              | [GATE]      |
| :-----: | :------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------------------------------------------------------------- | :---------- |
|   [1]   | Zero-allocation manager-event fold from handler through `AllocationEvidence` emission without per-event garbage | `uv run python -m tools.assay test run --target Rasm.Compute` running a CsCheck `Check.Faster` spec over the eleven-event fold | STREAM_POOL |
|   [2]   | `GetReadOnlySequence` segment granularity equal to pooled blocks behind the frame-window law                  | `uv run python -m tools.assay api query --key recyclable --symbol Microsoft.IO.RecyclableMemoryStream.GetReadOnlySequence` | STREAM_POOL |
