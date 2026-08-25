# [COMPUTE_MEMORY]

Every payload that crosses Rasm.Compute between intent admission and the IO edges is staged through one owned class on the `AllocationClass` axis and granted once against the intent-declared payload bound. `AllocationEvidence` is the manager-event payload discriminated by `StagingEventKind` and keyed by `CorrelationId`; `Grant` is the one admission edge and `Rent` the one staged rent it gates. `StagingViews` compose over rented memory beneath that edge, and one recyclable stream pool per composition attaches the manager events.

`CorrelationId` (`Rasm/Domain/frame#SOURCE`) arrives settled from the kernel frame, `Charges` from `Runtime/ledger#CHARGEBACK_EGRESS`, and `ComputeFault` from `Runtime/admission#DISPATCH_SPINE`; pooled `MemoryOwner<T>`/`SpanOwner<T>` owners are the lifetime boundary; the `DeviceWgpu` row classifies the `Tensor/dispatch#DEVICE_KERNELS` GPU buffer the renderer's `ONE_WGPU_DEVICE` owns. Plane projection is not this page's — `Span2D<T>`/`Memory2D<T>` compose bare at their consumers under the `Tensor/layout#LAYOUT_ALGEBRA` cession stated below.

## [01]-[INDEX]

- [02]-[ALLOCATION_AXIS]: staging axis with its stack cap and per-lane pool columns; `Admits` accumulating gate; `Grant`/`Rent` rail with the typed allocation fault; `StagingEventKind` taxonomy carrying its own manager attachment; `AllocationEvidence` event payload; bit-mask/grow/tokenize views.
- [03]-[STREAM_POOL]: one pooled stream manager per composition; policy record; the roster-driven evidence fold; the conservation ledger; zero-copy contiguous, segment, and text-frame handoff.

## [02]-[ALLOCATION_AXIS]

- Owner: `AllocationClass` `[SmartEnum<string>]` rows under the `ComparerAccessors.StringOrdinal` accessor, each carrying its synchronous-lane, copy-reason, stack-cap, and per-lane `ArrayPool<byte>` columns; `StagingEventKind` `[SmartEnum<string>]` carries the `Diagnostic` column and generated `Attach` manager-subscription column; `AllocationEvidence` is the event payload every grant and pool callback constructs.
- Cases: `SpanStack`, `PooledMemory`, `RecyclableStream`, `UnpooledBuffer` (a large buffer the manager declined to pool — the honest stamp for the one staging site that lawfully leaves the pool), `NativeOrt`, `EdgeCopy`, `DeviceWgpu` (the `Tensor/dispatch#DEVICE_KERNELS` GPU storage/staging buffer over the shared `ONE_WGPU_DEVICE`, `copyReasoned` because a device readback crosses the host boundary).
- Entry: `Fin<AllocationEvidence> Grant(AllocationRequest request)` consumes one request carrier holding correlation, byte bound, lane timing, reset mode, copy reason, and native reservation; `Validation<Error, Unit> Admits(AllocationRequest)` is the accumulating gate every refusal reads off, so a request breaching three columns names three. `Fin<(MemoryOwner<byte> Buffer, AllocationEvidence Evidence)> Rent(AllocationRequest)` is the granted BYTE rent over the row's own isolated lane pool and `Fin<(MemoryOwner<T> Buffer, AllocationEvidence Evidence)> Rent<T>(AllocationRequest, int elements)` the granted typed-element rent. Negative bytes, over-bound grants, over-cap stack rents, synchronous-only classes in async lanes, blank copy reasons, and invalid native reservations accumulate through `ComputeFault.AllocationOverClass`.
- Auto: intent admission calls `Grant` once against the intent-declared payload bound; every grant materializes one `AllocationEvidence` value under the intent correlation with zero call-site accounting, the holder settles it through the one `[03]-[STREAM_POOL]` `StreamPool.Stamp` door, and the manager events fold to the same record through each `StagingEventKind` row's own `Attach` column.
- Output: `AllocationEvidence` — correlation, class row, `StagingEventKind` discriminant, the requested/granted byte pair (reused per kind), the typed `Option<Duration> Lifetime` a dispose closes its grant with, the `Detail` string under the closed three-sense grammar the `Kind` row recovers (copy reason on a reasoned copy, discard taxonomy key on a discard, provenance stack on a leak), the native/device allocator slots, and the small/large free-pool gauges populated only on `UsageReport`; it is a `readonly record struct` built from hot-path values.
- Packages: CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule)
- Growth: one `AllocationClass` row with its four policy columns; a new evidence event is one `StagingEventKind` row carrying its own `Attach` column — the pool constructor is a fold over `Items` and takes no edit; a cap change is one policy value; a new evidence fact is one `AllocationEvidence` slot; zero new entrypoint.
- Boundary: the class is intent-declared data, never a call-site choice, so `Grant` is the one admission edge for STAGING — a pooled stream, a pinned plane, a device buffer, an edge copy, any rent crossing an `await` or a native API boundary — and a bare `ArrayPool<T>.Shared` rent standing in for one of those is the deleted form. Kernel-interior scratch is EXEMPT by the same law (its `Tensor/dispatch#KERNEL_DISPATCH` counterpart clause), because a rent sized by an already-admitted operand extent and released on its own frame stamps one evidence value per elementwise call that no instrument reads; the exemption NAMES its second member — `Tensor/layout#LAYOUT_ALGEBRA` shape-edit materialization, the five `StorageClass.Materialized` verbs `Concatenate`/`Stack`/`Pad`/`Write` and the plural `Split`, whose destination extent the same `ReshapeOp` admission just proved — so those five rent bare and every OTHER staged byte on the branch carries a `Grant`. `Admits` makes all four policy columns load-bearing: `SyncOnly` rejects a stack row requested for an async lane (the data-level complement of the `SpanOwner<T>` ref-struct that already cannot cross an `await`/iterator boundary), `StackCap` rejects a stack rent past the row's declared ceiling so the doctrine cap is a value rather than a review habit, `CopyReasoned` rejects a copy without a reason, and the bound rejects an over-class request — and a failing `Admits` folds `ComputeFault.AllocationOverClass` (the `Runtime/admission#DISPATCH_SPINE` 2209 case) carrying every breached column at once. `Lane` is the per-row `ArrayPool<byte>` doctrine names, so one lane's size-class churn cannot evict another's; the typed `Rent<T>` shares `ArrayPool<T>.Shared` because a per-`(row, T)` pool roster is unbounded in `T` — NAMED LOSS: element-typed staging keeps one process-wide size-class economy, and the byte lanes alone are isolated. `AllocationMode` is the rent-time reset column ON the request, not a fixed page default: a fully-overwritten payload keeps the bandwidth with `Default` while a partially-written one rents `Clear`, because a partial write leaks the pool's prior content in its unwritten tail whatever the payload's secrecy class. `MemoryOwner<T>`/`SpanOwner<T>` are the lifetime boundary composed bare while `Ref<T>` carriers and `DangerousGetReference` stay kernel-internal, and `DangerousGetArray` is the `ArraySegment<T>` handoff seam for the tensor-lane rented-array `Tensor.Create` factory and the `StreamPool` zero-copy `ByteString` wrap. Planes are views, never layout: rank permutation stays `Tensor/layout#LAYOUT_ALGEBRA` and a `Span2D<T>` projection never substitutes for it, so the layout lane's densify gate reads `Contiguity.Classify` over the `Tensor<T>` stride facts rather than a plane's `TryGetSpan` probe. Content hashing rides the suite `System.IO.Hashing` `XxHash3`/`XxHash128` owner, never a second staging-local `HashCode<T>` digest. This axis admits no `System.IO.Pipelines` route and no unowned buffer type without a row.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AllocationClass {
    public static readonly AllocationClass SpanStack = new("span-stack",
        syncOnly: true, copyReasoned: false, stackCap: Some(512L), lane: ArrayPool<byte>.Create(1 << 16, 32));
    public static readonly AllocationClass PooledMemory = new("pooled-memory",
        syncOnly: false, copyReasoned: false, stackCap: None, lane: ArrayPool<byte>.Create(1 << 20, 64));
    public static readonly AllocationClass RecyclableStream = new("recyclable-stream",
        syncOnly: false, copyReasoned: false, stackCap: None, lane: ArrayPool<byte>.Create(1 << 20, 64));
    public static readonly AllocationClass UnpooledBuffer = new("unpooled-buffer",
        syncOnly: false, copyReasoned: false, stackCap: None, lane: ArrayPool<byte>.Shared);
    public static readonly AllocationClass NativeOrt = new("native-ort",
        syncOnly: false, copyReasoned: false, stackCap: None, lane: ArrayPool<byte>.Create(1 << 22, 16));
    public static readonly AllocationClass EdgeCopy = new("edge-copy",
        syncOnly: false, copyReasoned: true, stackCap: None, lane: ArrayPool<byte>.Create(1 << 20, 16));
    public static readonly AllocationClass DeviceWgpu = new("device-wgpu",
        syncOnly: false, copyReasoned: true, stackCap: None, lane: ArrayPool<byte>.Create(1 << 22, 16));

    public bool SyncOnly { get; }

    public bool CopyReasoned { get; }

    public Option<long> StackCap { get; }

    public ArrayPool<byte> Lane { get; }

    public Validation<Error, Unit> Admits(AllocationRequest request) =>
        (Bound(request), Lane(request), Cap(request), Copy(request), Native(request))
            .Apply(static (_, _, _, _, _) => unit).As();

    public Fin<AllocationEvidence> Grant(AllocationRequest request) =>
        Admits(request).ToFin().Map(_ => new AllocationEvidence(
            request.Correlation, this, StagingEventKind.Grant, request.RequestedBytes, request.RequestedBytes,
            None, request.CopyReason, request.NativeAllocator, request.NativeReservedBytes, None, None));

    public Fin<(MemoryOwner<byte> Buffer, AllocationEvidence Evidence)> Rent(AllocationRequest request) =>
        request.RequestedBytes > int.MaxValue
            ? TensorReason.ExtentOverflow.Fail<(MemoryOwner<byte>, AllocationEvidence)>("grant-width", Key, request.RequestedBytes.ToString(CultureInfo.InvariantCulture))
            : Grant(request).Map(evidence => (MemoryOwner<byte>.Allocate(checked((int)request.RequestedBytes), Lane, request.Mode), evidence));

    public Fin<(MemoryOwner<T> Buffer, AllocationEvidence Evidence)> Rent<T>(AllocationRequest request, int elements) where T : unmanaged =>
        elements < 0 || (long)elements * Unsafe.SizeOf<T>() != request.RequestedBytes
            ? TensorReason.ShapeMismatch.Fail<(MemoryOwner<T>, AllocationEvidence)>("grant-elements", Key, $"{elements}x{Unsafe.SizeOf<T>()}!={request.RequestedBytes}")
            : Grant(request).Map(evidence => (MemoryOwner<T>.Allocate(elements, request.Mode), evidence));

    private Validation<Error, Unit> Bound(AllocationRequest request) =>
        request.RequestedBytes >= 0 && request.PayloadBound >= 0 && request.RequestedBytes <= request.PayloadBound
            ? unit
            : TensorReason.StagingOverBound.Fault("class-bound", Key, $"{request.RequestedBytes}/{request.PayloadBound}");

    private Validation<Error, Unit> Lane(AllocationRequest request) =>
        !(SyncOnly && request.Async) ? unit : TensorReason.PolicyInvalid.Fault("class-sync-only", Key);

    private Validation<Error, Unit> Cap(AllocationRequest request) =>
        StackCap.Match(None: static () => Validation<Error, Unit>.Success(unit),
            Some: cap => request.RequestedBytes <= cap
                ? unit
                : TensorReason.StagingOverBound.Fault("class-stack-cap", Key, $"{request.RequestedBytes}>{cap}"));

    private Validation<Error, Unit> Copy(AllocationRequest request) =>
        !CopyReasoned || request.CopyReason.Match(Some: static reason => !string.IsNullOrWhiteSpace(reason), None: static () => false)
            ? unit
            : TensorReason.PolicyInvalid.Fault("class-copy-reason", Key);

    private Validation<Error, Unit> Native(AllocationRequest request) =>
        request.NativeReservedBytes.Match(Some: static bytes => bytes >= 0, None: static () => true)
        && request.NativeAllocator.Match(Some: static allocator => !string.IsNullOrWhiteSpace(allocator), None: static () => true)
            ? unit
            : TensorReason.PolicyInvalid.Fault("class-native-reservation", Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StagingEventKind {
    public static readonly StagingEventKind Grant = new("grant", diagnostic: false,
        attach: static (_, _) => None);
    public static readonly StagingEventKind StreamCreated = new("stream-created", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamCreatedEventArgs>(
            h => m.StreamCreated += h, h => m.StreamCreated -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, StreamCreated, a.RequestedSize, a.ActualSize))));
    public static readonly StagingEventKind StreamDisposed = new("stream-disposed", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamDisposedEventArgs>(
            h => m.StreamDisposed += h, h => m.StreamDisposed -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, StreamDisposed, 0, 0) with { Lifetime = Some(Duration.FromTimeSpan(a.Lifetime)) })));
    public static readonly StagingEventKind StreamLength = new("stream-length", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamLengthEventArgs>(
            h => m.StreamLength += h, h => m.StreamLength -= h,
            a => AllocationEvidence.Pool(AllocationClass.RecyclableStream, StreamLength, a.Length, a.Length))));
    public static readonly StagingEventKind BlockCreated = new("block-created", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.BlockCreatedEventArgs>(
            h => m.BlockCreated += h, h => m.BlockCreated -= h,
            a => AllocationEvidence.Pool(AllocationClass.RecyclableStream, BlockCreated, 0, a.SmallPoolInUse))));
    public static readonly StagingEventKind LargeBufferCreated = new("large-buffer-created", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.LargeBufferCreatedEventArgs>(
            h => m.LargeBufferCreated += h, h => m.LargeBufferCreated -= h,
            a => AllocationEvidence.Bytes(a.Id, a.Pooled ? AllocationClass.RecyclableStream : AllocationClass.UnpooledBuffer,
                LargeBufferCreated, a.RequiredSize, a.LargePoolInUse))));
    public static readonly StagingEventKind UsageReport = new("usage-report", diagnostic: false,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.UsageReportEventArgs>(
            h => m.UsageReport += h, h => m.UsageReport -= h,
            a => AllocationEvidence.Pool(AllocationClass.RecyclableStream, UsageReport, a.SmallPoolInUseBytes, a.LargePoolInUseBytes) with {
                SmallPoolFreeBytes = Some(a.SmallPoolFreeBytes), LargePoolFreeBytes = Some(a.LargePoolFreeBytes),
            })));
    public static readonly StagingEventKind StreamConvertedToArray = new("stream-converted-to-array", diagnostic: true,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamConvertedToArrayEventArgs>(
            h => m.StreamConvertedToArray += h, h => m.StreamConvertedToArray -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.EdgeCopy, StreamConvertedToArray, a.Length, 0) with { Detail = Optional(a.Stack) })));
    public static readonly StagingEventKind StreamOverCapacity = new("stream-over-capacity", diagnostic: true,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamOverCapacityEventArgs>(
            h => m.StreamOverCapacity += h, h => m.StreamOverCapacity -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, StreamOverCapacity, a.RequestedCapacity, a.MaximumCapacity) with { Detail = Optional(a.AllocationStack) })));
    public static readonly StagingEventKind StreamDoubleDisposed = new("stream-double-disposed", diagnostic: true,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamDoubleDisposedEventArgs>(
            h => m.StreamDoubleDisposed += h, h => m.StreamDoubleDisposed -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, StreamDoubleDisposed, 0, 0) with { Detail = Optional(a.DisposeStack2) })));
    public static readonly StagingEventKind StreamFinalized = new("stream-finalized", diagnostic: true,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.StreamFinalizedEventArgs>(
            h => m.StreamFinalized += h, h => m.StreamFinalized -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, StreamFinalized, 0, 0) with { Detail = Optional(a.AllocationStack) })));
    public static readonly StagingEventKind BufferDiscarded = new("buffer-discarded", diagnostic: true,
        attach: static (m, pool) => Some(pool.Project<RecyclableMemoryStreamManager.BufferDiscardedEventArgs>(
            h => m.BufferDiscarded += h, h => m.BufferDiscarded -= h,
            a => AllocationEvidence.Bytes(a.Id, AllocationClass.RecyclableStream, BufferDiscarded, 0, 0) with { Detail = Some($"{a.BufferType}:{a.Reason}") })));

    public bool Diagnostic { get; }

    [UseDelegateFromConstructor]
    public partial Option<Action> Attach(RecyclableMemoryStreamManager manager, StreamPool pool);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AllocationRequest(
    CorrelationId Correlation,
    long RequestedBytes,
    long PayloadBound,
    bool Async,
    AllocationMode Mode,
    Option<string> CopyReason,
    Option<string> NativeAllocator,
    Option<long> NativeReservedBytes);

public readonly record struct AllocationEvidence(
    CorrelationId Correlation,
    AllocationClass Class,
    StagingEventKind Kind,
    long RequestedBytes,
    long GrantedBytes,
    Option<Duration> Lifetime,
    Option<string> Detail,
    Option<string> NativeAllocator,
    Option<long> NativeReservedBytes,
    Option<long> SmallPoolFreeBytes,
    Option<long> LargePoolFreeBytes) {
    public static AllocationEvidence Bytes(Guid id, AllocationClass row, StagingEventKind kind, long requested, long granted) =>
        new(CorrelationId.Create(id), row, kind, requested, granted, None, None, None, None, None, None);

    public static AllocationEvidence Pool(AllocationClass row, StagingEventKind kind, long requested, long granted) =>
        new(CorrelationId.None, row, kind, requested, granted, None, None, None, None, None, None);
}

public readonly record struct PoolLedger(long Created, long Disposed, long Finalized, long Discarded) {
    public static readonly PoolLedger Empty = new(0, 0, 0, 0);

    public long Live => Created - Disposed - Finalized;

    public bool Conserved => Live >= 0;

    public PoolLedger Fold(StagingEventKind kind) =>
        kind == StagingEventKind.StreamCreated ? this with { Created = Created + 1 }
        : kind == StagingEventKind.StreamDisposed ? this with { Disposed = Disposed + 1 }
        : kind == StagingEventKind.StreamFinalized ? this with { Finalized = Finalized + 1 }
        : kind == StagingEventKind.BufferDiscarded ? this with { Discarded = Discarded + 1 }
        : this;
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class StagingViews {
    extension(MemoryOwner<ulong> mask) {
        public bool Cell(int index) => BitHelper.HasFlag(mask.Span[index >> 6], index & 63);

        public void Mark(int index) => BitHelper.SetFlag(ref mask.Span[index >> 6], index & 63, true);

        public void Clear(int index) => BitHelper.SetFlag(ref mask.Span[index >> 6], index & 63, false);

        public ulong Read(int start, byte length) => BitHelper.ExtractRange(mask.Span[start >> 6], (byte)(start & 63), length);

        public void Pack(int start, byte length, ulong value) => BitHelper.SetRange(ref mask.Span[start >> 6], (byte)(start & 63), length, value);
    }

    extension(ArrayPool<byte> pool) {
        public Span<byte> Grow(ref byte[]? backing, int capacity) {
            pool.EnsureCapacity(ref backing, capacity);
            return backing.AsSpan(0, capacity);
        }
    }

    extension(ReadOnlySpan<byte> codecText) {
        public ReadOnlySpanTokenizer<byte> Fields(byte separator) => codecText.Tokenize(separator);
    }
}
```

Each staging route carries one allocation ruling:

- [01]-[ADMISSION]: `AllocationClass.Grant` is the one staging edge and `Rent`/`Rent<T>` the only rents it gates — `Admits` accumulates every breached column, stamps `AllocationEvidence` on success, and folds `ComputeFault.AllocationOverClass` carrying all of them on rejection; a call-site pool choice is the deleted form
- [02]-[STACK_RENT]: `SpanOwner<T>.Allocate(int, AllocationMode)` stays inside one synchronous kernel scope on `SpanStack` under the row's own `StackCap`; an `AllocationRequest` with `Async = true` or a byte count past the cap rejects the row before rent
- [03]-[POOLED_RENT]: `AllocationClass.PooledMemory.Rent` hands a `MemoryOwner<byte>` off the row's own isolated lane; `Slice` projects windows; `Dispose` returns deterministically
- [04]-[INCREMENTAL_BUILD]: `ArrayPoolBufferWriter<T>`/`MemoryBufferWriter<T>` own growing payloads as the `IBufferWriter<T>` codec-emit sink on the `PooledMemory` row; `WrittenMemory`/`WrittenSpan` read the committed payload back zero-copy
- [05]-[RENT_CLEARING]: `AllocationMode` is a request column — a fully-overwritten payload rents `Default` and a partially-written or trust-seam payload rents `Clear`, because the unwritten tail of a partial write carries the pool's prior content whatever the payload's secrecy class; `ZeroOutBuffer` stays a `Diagnostic`-row policy for the manager's own blocks
- [06]-[FOREIGN_EVIDENCE]: native and device grants pass `nativeAllocator`/`nativeReservedBytes` into `Grant` — the `NativeOrt` row carries the model-lane allocator name and reserved bytes and the `DeviceWgpu` row the `wgpu:<deviceId>` descriptor through the same slot pair
- [07]-[EDGE_COPY]: every `Grant` on the `EdgeCopy` row carries a copy reason or `Admits` rejects it; every array materialization and stream flatten routes through it, and the realized copy surfaces as the `StreamConvertedToArray` diagnostic event whose stack rides `Detail` as provenance beside the reason its own grant already carried
- [08]-[TEXT_INTERNING]: `StringPool.GetOrAdd` interns diagnostic text at the evidence edge only; `ReadOnlySpan<byte>.Fields`/`Tokenize` split codec text spans without intermediate strings
- [09]-[BIT_PACKING]: `StagingViews.Mark`/`Clear`/`Cell` set/test one occupancy bit and `Pack`/`Read` pack/extract a multi-bit material-id field over a `Span<ulong>` window of `PooledMemory` (sixty-four cells per word) through the branchless `BitHelper` `ref`-overloads — one bit per cell replaces a `byte` buffer, and `Tensor/residency#GEOMETRY_ENCODING` stages the `PackKind.VoxelGrid` occupancy lane through it
- [10]-[IN_PLACE_GROWTH]: `ArrayPool<byte>.Grow` (over `ArrayPoolExtensions.EnsureCapacity`) grows the rented backing during incremental codec emit; the writer never reallocates through a second rent and the granted-byte slot reflects the grown capacity
- [11]-[CONTIGUOUS_FRAME]: `StreamPool.Frame(correlation, requiredSize, payloadBound)` is the ONE contiguous-frame entry — under `MaximumBufferSize` it forces one large-buffer stream on the `RecyclableStream` row whose `RequiredSize` fills the granted-byte slot, and above it a `PooledMemory` byte rent carrying its own `Grant` evidence; `Tensor/factor#SPARSE_SOLVE`'s `.mtx` and archive exchange legs are its standing consumers

## [03]-[STREAM_POOL]

- Owner: `StreamPool` boundary capsule owning its composition's `RecyclableMemoryStreamManager`, its detacher chain, its `PoolLedger` conservation cell, its refusal cell, and the ONE `Stamp` door every `AllocationEvidence` in the package settles through; `StreamPoolPolicy` carries every pool policy value; `PoolEvidence` the foreign-receiver projection each `StagingEventKind` row's `Attach` column composes.
- Entry: `Fin<RecyclableMemoryStream> Get(CorrelationId correlation, StreamGrant grant)` admits positive sizes and contiguous-buffer capacity before trapping the manager rent; `Fin<FrameRent> Frame(CorrelationId, long requiredSize, long payloadBound)` owns the contiguous-frame route across the cap, returning a pooled stream below `MaximumBufferSize` and a granted `PooledMemory` byte rent above it. `IO<Fin<RecyclableMemoryStream>> Write(CorrelationId, IMessage)` sizes the rent off `CalculateSize()` and emits the UNPREFIXED body through `WriteTo(IBufferWriter<byte>)` under a bracket that releases the rent on EVERY failure arm; `Read<T>(RecyclableMemoryStream, MessageParser<T>, WireLimits)` parses the pooled stream through `CodedInputStream.CreateWithLimits(stream, limits.SizeLimit, limits.RecursionLimit)` from position zero, the stream walking its pooled blocks sequentially without flattening. `Unit Stamp(AllocationEvidence evidence)` folds the conservation cell and settles the process-scoped byte charge on a `Grant`; `PoolLedger Ledger` and `Seq<Error> Refusals` read the two held cells. `StreamGrant` remains the closed `Open | Sized | ContiguousFrame` acquisition discriminant.
- Auto: the constructor creates the manager from `policy.Options` and folds `StagingEventKind.Items` into its detacher chain — each row attaching its own manager event through `PoolEvidence.Project`, projecting its `EventArgs` to an `AllocationEvidence` value and handing it to `Stamp` with zero call-site code; `Get` passes the correlation as the `Guid` stream id on every path so every later event rejoins its intent by id, and the `RecyclableStream` row key is the tag.
- Law: double-dispose, finalization, and discarded-buffer events are leak diagnostics (the `StagingEventKind.Diagnostic` column) counted under their own event kind, never log noise; an array-conversion event is the `StreamConvertedToArray` diagnostic corroborating an edge copy; `StreamDisposed` carries a typed `Duration` so the lifetime-percentile early warning reads a measurement rather than parsing one out of a string.
- Packages: Microsoft.IO.RecyclableMemoryStream, CommunityToolkit.HighPerformance, Google.Protobuf (`MessageExtensions.WriteTo(IBufferWriter<byte>)`, `CalculateSize`, `CodedInputStream.CreateWithLimits`, `MessageParser<T>.ParseFrom(CodedInputStream)`), LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Compute `Runtime/channels#ARTIFACT_FRAMES` `WireLimits`, `Runtime/ledger#CHARGEBACK_EGRESS` `Charges`
- Growth: one policy value on `StreamPoolPolicy`; a new evidence slot is one `AllocationEvidence` field; a new manager event is one `StagingEventKind` row carrying its own `Attach` column, and the constructor takes no edit; zero new surface.
- Boundary: `StreamPool` is the named boundary capsule for the statement carve-out — the constructor's manager creation and detacher collection carry language-owned statement forms while every other member stays expression-shaped, and `StagingViews.Grow`'s `ref byte[]?` growth is the one further platform-forced statement seam. `PoolEvidence.Project` is the foreign-receiver/local-behavior extension form: the manager event is the foreign receiver read only to mint evidence while the subscription detacher holding the exact handler identity is Compute-owned behavior, so the block adds no second disposer registry. A charge that refuses inside the manager's own event dispatch must not tear the manager's invocation list, so `Stamp` parks it on the capsule's `Atom<Seq<Error>>` cell. The conservation fold rides an `Atom<PoolLedger>` for the same reason: an emitted stream nothing accumulates cannot answer the question at any instant. Per-event `JsonSerializer.SerializeToElement` materializes one `JsonElement` per event and that cost is the price of a self-describing evidence, which is why the diagnostic-heavy rows bind on the `Diagnostic` policy row alone; a `Channel<AllocationEvidence>` drain is NOT taken because the only full-mode that preserves every fact blocks the manager's dispatch and the ones that do not drop evidence a leak audit needs. One capsule per COMPOSITION — the composing root supplies the policy and owns the capsule's lifetime and disposal, so this tier holds no static, no ambient locator, and no process-wide claim a plugin host loading the assembly into a second load context already breaks, each composition owning its own manager, at the accepted cost doctrine names: two managers fork the block economy and roughly double steady-state pooled residency, which the composition root answers by holding exactly one composition per process. Memory, owners, writers, and sequences become streams only through the `AsStream` extension family at IO edges, and the package-internal stream classes never enter vocabulary. One rent carries binary and text payloads alike — the protobuf pair crosses through `Write`/`Read` as one unprefixed body per rent (the stream's own length IS the frame, so `WriteDelimitedTo`/`ParseDelimitedFrom` and a hand length prefix never enter — a prefix the sequence parser cannot consume was the round-trip defect this pair once carried), a text payload through `StreamReader`/`StreamWriter` over that same rent — so no lane opens a `FileStream` of its own for a large operator or a codec listing; the one carve is the `Runtime/archive#HDF_ARCHIVE` capsule, whose `HdfSource.Path`/`Mapped` opens are the parallel-read law's own requirement (`ThreadLocal` read position lives on the file-handle and mapped drivers alone) and whose handles stay job-scoped — a lane still never opens its own, it opens the archive's. This capsule deletes per-call-site manager instances, raw `MemoryStream` construction, copy-shaped `ByteString.CopyFrom`, unsettled `ToArray` flattens, and unpooled file-backed text reads.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StreamGrant {
    private StreamGrant() { }
    public sealed record Open : StreamGrant;
    public sealed record Sized(long RequiredSize) : StreamGrant;
    public sealed record ContiguousFrame(long RequiredSize) : StreamGrant;

    public (Option<long> Size, bool Contiguous) Shape => Switch(
        open: static _ => (Option<long>.None, false),
        sized: static s => (Some(s.RequiredSize), false),
        contiguousFrame: static f => (Some(f.RequiredSize), true));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameRent {
    private FrameRent() { }
    public sealed record Streamed(RecyclableMemoryStream Stream) : FrameRent;
    public sealed record Rented(MemoryOwner<byte> Buffer, AllocationEvidence Evidence) : FrameRent;
}

// --- [MODELS] --------------------------------------------------------------------------
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
    public static readonly StreamPoolPolicy Canonical = Derived(GrpcChannelPolicy.Canonical.MaxSendBytes, ArtifactCeiling);

    public const long ArtifactCeiling = 1024L * 1024 * 1024;

    public static StreamPoolPolicy Derived(int payloadCap, long artifactCap) => new(
        BlockSize: payloadCap >> 5,
        LargeBufferMultiple: payloadCap >> 2,
        MaximumBufferSize: payloadCap,
        MaximumSmallPoolFreeBytes: 16777216,
        MaximumLargePoolFreeBytes: 33554432,
        MaximumStreamCapacity: artifactCap,
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

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class StreamPool : IDisposable {
    readonly RecyclableMemoryStreamManager manager;
    readonly StreamPoolPolicy policy;
    readonly Seq<Action> detachers;
    readonly Charges charges;
    readonly Atom<PoolLedger> ledger = Atom(PoolLedger.Empty);
    readonly Atom<Seq<Error>> refusals = Atom(Seq<Error>());
    bool disposed;

    public StreamPool(StreamPoolPolicy policy, Charges charges) {
        this.policy = policy;
        this.charges = charges;
        manager = new RecyclableMemoryStreamManager(policy.Options);
        detachers = toSeq(StagingEventKind.Items)
            .Map(row => row.Attach(manager, this))
            .Somes();
    }

    public PoolLedger Ledger => ledger.Value;

    public Seq<Error> Refusals => refusals.Value;

    public Unit Stamp(AllocationEvidence evidence) {
        ledger.Swap(held => held.Fold(evidence.Kind));
        if (evidence.Kind == StagingEventKind.Grant) {
            Park(charges.Settle(new Charge(None, charges.Rates.Staged(evidence.GrantedBytes)), Op.Of(name: "compute.charge.staging")).Map(static _ => unit));
        }
        return unit;
    }

    Unit Park(Fin<Unit> written) =>
        written.Match(Succ: static _ => unit, Fail: error => ignore(refusals.Swap(held => held.Add(error))));

    public Fin<RecyclableMemoryStream> Get(CorrelationId correlation, StreamGrant grant) =>
        Rent(correlation, grant.Shape);

    public Fin<FrameRent> Frame(CorrelationId correlation, long requiredSize, long payloadBound) =>
        requiredSize <= 0 ? TensorReason.StagingOverBound.Fail<FrameRent>("stream-size", requiredSize.ToString(CultureInfo.InvariantCulture))
        : requiredSize <= policy.MaximumBufferSize ? Get(correlation, new StreamGrant.ContiguousFrame(requiredSize)).Map(static stream => (FrameRent)new FrameRent.Streamed(stream))
        : AllocationClass.PooledMemory
            .Rent(new AllocationRequest(correlation, requiredSize, payloadBound, Async: false, AllocationMode.Default, None, None, None))
            .Map(static rent => (FrameRent)new FrameRent.Rented(rent.Buffer, rent.Evidence));

    public IO<Fin<RecyclableMemoryStream>> Write(CorrelationId correlation, IMessage message) =>
        IO.pure(Framed(message).Bind(required => Get(correlation, new StreamGrant.Sized(required))))
            .Bind(opened => opened.Match(
                Succ: stream => IO.lift(() => stream).Bracket(
                    Use: held => IO.lift(() => {
                        message.WriteTo((IBufferWriter<byte>)held);
                        held.Position = 0;
                        return Fin.Succ(held);
                    }),
                    Catch: static error => IO.pure(Fin<RecyclableMemoryStream>.Fail(error)),
                    Fin: static held => IO.lift(() => { held.Dispose(); return unit; })),
                Fail: static error => IO.pure(Fin<RecyclableMemoryStream>.Fail(error))));

    public Fin<T> Read<T>(RecyclableMemoryStream stream, MessageParser<T> parser, WireLimits limits) where T : IMessage<T> =>
        Op.Of(name: "stream-read").Catch(() => Fin.Succ(parser.ParseFrom(
            CodedInputStream.CreateWithLimits(stream, limits.SizeLimit, limits.RecursionLimit))));

    static Fin<long> Framed(IMessage message) =>
        Op.Of(name: "stream-size").Catch(() => Fin.Succ((long)message.CalculateSize()));

    Fin<RecyclableMemoryStream> Rent(CorrelationId correlation, (Option<long> Size, bool Contiguous) shape) =>
        shape.Size.Match(
            None: () => Op.Of(name: "stream-rent").Catch(() => Fin.Succ(manager.GetStream(correlation, AllocationClass.RecyclableStream.Key))),
            Some: size =>
                size <= 0 ? TensorReason.StagingOverBound.Fail<RecyclableMemoryStream>("stream-size", size.ToString(CultureInfo.InvariantCulture))
                : shape.Contiguous && size > policy.MaximumBufferSize ? TensorReason.StagingOverBound.Fail<RecyclableMemoryStream>("stream-contiguous-cap", $"{size}>{policy.MaximumBufferSize}")
                : size > policy.MaximumStreamCapacity ? TensorReason.StagingOverBound.Fail<RecyclableMemoryStream>("stream-cap", $"{size}>{policy.MaximumStreamCapacity}")
                : Op.Of(name: "stream-rent").Catch(() => Fin.Succ(manager.GetStream(correlation, AllocationClass.RecyclableStream.Key, size, shape.Contiguous))));

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        detachers.Rev().Iter(static detach => detach());
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class PoolEvidence {
    extension(StreamPool pool) {
        public Action Project<TArgs>(
            Action<EventHandler<TArgs>> add,
            Action<EventHandler<TArgs>> remove,
            Func<TArgs, AllocationEvidence> evidence) where TArgs : EventArgs {
            EventHandler<TArgs> handler = (_, args) => pool.Stamp(evidence(args));
            add(handler);
            return () => remove(handler);
        }
    }
}
```

Each entry carries one ruling:

- [01]-[FRAGMENTED_READ]: `GetReadOnlySequence` is the default VIEW of staged bytes — segments map one-to-one onto pooled blocks (single-block and large-buffer streams collapse to one segment) and the frame law's `UnsafeWrap` reads them with no flatten — while the protobuf decode reads the stream itself under `CreateWithLimits`, which walks the same blocks sequentially and flattens nothing
- [02]-[ZERO_COPY_EDGE]: `UnsafeByteOperations.UnsafeWrap` wraps sequence windows at the remote edge under the frame law the remote lane owns
- [03]-[CODEC_WINDOW]: `TryGetBuffer` exposes a contiguous window for codecs bounded by `MaximumBufferSize`; the stream's `WriteTo(Stream)` is the array-free stream-to-stream copy and the message's `WriteTo(IBufferWriter<byte>)` the array-free encode into the rent
- [04]-[DERIVED_GEOMETRY]: `BlockSize`, `LargeBufferMultiple`, and `MaximumBufferSize` derive from the ONE channel payload cap `Runtime/admission#DISPATCH_SPINE` `GrpcChannelPolicy.Canonical.MaxSendBytes` owns, so a frame never straddles a pooled block, every large-buffer step lands on a cap boundary, and a cap change moves one value
- [05]-[STREAM_CAP]: `MaximumStreamCapacity` is the artifact ceiling `ArtifactCeiling` rather than the package's zero no-limit spelling — a per-stream cap distinct from the channel payload cap the geometry strides on — so an unbounded stream is refused by the manager as well as by `AllocationClass.Grant` at admission
- [06]-[POOL_RETENTION]: free-bytes caps bound RETAINED (never in-use) memory and a return past a cap releases as a `BufferDiscarded` event; the large cap applies per size class, so real retention is the floor times the touched size-class count
- [07]-[CONTIGUOUS_VIEW]: `GetBuffer` exposes the whole stream as one array when the codec needs a contiguous backing past `MaximumBufferSize`; the call is array-free against pooled blocks and never copies, where `TryGetBuffer` caps at one block
- [08]-[SEGMENT_HANDOFF]: `MemoryOwner<byte>.DangerousGetArray` hands the rented `ArraySegment<byte>` to `UnsafeByteOperations.UnsafeWrap` so a pooled payload becomes a `ByteString` with zero copy; the owner outlives the wrap and disposes after send
- [09]-[BLOCK_DIAGNOSTIC]: `BlockAndOffset`/`BlockSegment` address pooled-block boundaries on the `Diagnostic` policy row so a frame-straddle assertion reads exact block positions; production reads only `GetReadOnlySequence` segment counts
- [10]-[TEXT_FRAME]: `StreamReader`/`StreamWriter` frame text over a rented stream under `Encoding.ASCII` — `StreamGrant.Sized` on a known length, `StreamGrant.Open` on an emitted one — and both ride the sequential `Read`/`Write` path across chained blocks, so a frame of any size demands neither `GetBuffer`'s contiguity cliff nor an `EdgeCopy` flatten; the `Tensor/factor#SPARSE_SOLVE` `.mtx` exchange is the standing consumer
- [11]-[TEXT_EGRESS]: `StreamWriter` takes `leaveOpen: true` and flushes before the rent leaves the capsule, so its consumer reads from position zero with the rent intact and disposal caller-owned
- [12]-[EVIDENCE_FOLDS]: the event stream answers conservation through `PoolLedger.Live`/`Conserved` and preserves typed lifetime through `AllocationEvidence.Lifetime`
