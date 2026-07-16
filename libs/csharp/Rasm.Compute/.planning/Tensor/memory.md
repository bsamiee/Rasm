# [COMPUTE_MEMORY]

Every payload that crosses Rasm.Compute between intent admission and the IO edges is staged through one owned class on the `AllocationClass` axis, granted once against the intent-declared payload bound, and folded onto one `AllocationEvidence` fact stream discriminated by `StagingEventKind` and keyed by `CorrelationId`. `Grant` is the one admission edge — success stamps evidence, failure folds the typed `ComputeFault.AllocationOverClass`; beneath it compose the `StagingViews` bit-mask/grow/tokenize views over rented memory, the bare plane projections over `CommunityToolkit.HighPerformance`, and the one-per-process recyclable stream pool whose constructor wires every manager event into the evidence fold.

`CorrelationId`/`ReceiptSinkPort`/`TenantContext`/`ComputeFault` arrive settled from the AppHost ports and `Runtime/admission#DISPATCH_SPINE`; pooled `MemoryOwner<T>`/`SpanOwner<T>` owners are the lifetime boundary; the `DeviceWgpu` row classifies the `Tensor/dispatch#DEVICE_KERNELS` GPU buffer the renderer's `ONE_WGPU_DEVICE` owns.

## [01]-[INDEX]

- [01]-[ALLOCATION_AXIS]: staging axis (incl. device-wgpu GPU buffer); `Admits` predicate; `Grant` rail with the typed allocation fault; `StagingEventKind` evidence taxonomy; `AllocationEvidence` slot/kind fact; bit-mask/grow/tokenize views.
- [02]-[PLANE_VIEWS]: bare plane projections; row/axis kernels; contiguity probe; reinterpretation law; layout split.
- [03]-[STREAM_POOL]: one pooled stream manager; policy record; eleven-event evidence fold; zero-copy contiguous and segment handoff.

## [02]-[ALLOCATION_AXIS]

- Owner: `AllocationClass` `[SmartEnum<string>]` rows under the `ComparerAccessors.StringOrdinal` accessor; `StagingEventKind` `[SmartEnum<string>]` the evidence taxonomy with the `Diagnostic` column; `AllocationEvidence` the slot/kind fact record every grant and every pool event stamps.
- Cases: `SpanStack`, `PooledMemory`, `RecyclableStream`, `NativeOrt`, `EdgeCopy`, `DeviceWgpu` (the `Tensor/dispatch#DEVICE_KERNELS` GPU storage/staging buffer over the shared `ONE_WGPU_DEVICE`, `copyReceipted` because a device readback crosses the host boundary).
- Entry: `Fin<AllocationEvidence> Grant(CorrelationId correlation, long requestedBytes, long payloadBound, bool async = false, Option<string> copyReason = default, Option<string> nativeAllocator = default, Option<long> nativeReservedBytes = default)` — `Grant` evaluates `Admits` once and either stamps the `StagingEventKind.Grant` evidence or folds the typed `ComputeFault.AllocationOverClass` carrying the precise rejection (`over-bound` | `sync-only-in-async-lane` | `copy-reason-missing`); `bool Admits(long requestedBytes, long payloadBound, bool async = false, Option<string> copyReason = default)` is the pure structural predicate the entry composes and a caller may pre-check.
- Auto: intent admission calls `Grant` once against the intent-declared payload bound; every grant materializes one `AllocationEvidence` value under the intent correlation with zero call-site accounting, and the eleven manager events fold to the same record through the `[03]-[STREAM_POOL]` `PoolEvidence` projection.
- Receipt: `AllocationEvidence` — correlation, class row, `StagingEventKind` discriminant, the requested/granted byte pair (reused per kind), the polymorphic `Detail` string (copy reason on a copy-receipted grant, discard reason on a discard, lifetime on a dispose, allocation stack on a leak), the native/device allocator slots, and the small/large free-pool gauges populated only on `UsageReport`; it is a `readonly record struct` that materializes at the receipt sink edge from hot-path values.
- Packages: CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one `AllocationClass` row with its predicate columns; a new evidence event is one `StagingEventKind` row plus its projection arm; a cap change is one policy value; a new evidence fact is one `AllocationEvidence` slot; zero new entrypoint.
- Boundary: the class is intent-declared data, never a call-site choice, so `Grant` is the one admission edge and a bare `ArrayPool<T>.Shared` rent beside it is the deleted form; `Admits` makes all three policy columns load-bearing — `SyncOnly` rejects a stack row requested for an async lane (the data-level complement of the `SpanOwner<T>` ref-struct that already cannot cross an `await`/iterator boundary), `CopyReceipted` rejects a copy without a reason, and the bound rejects an over-class request — and a false `Admits` folds `ComputeFault.AllocationOverClass` (the `Runtime/admission#DISPATCH_SPINE` 2210 band, never a stringly `ComputeFault.Create` text fault) with the discriminated detail. `MemoryOwner<T>`/`SpanOwner<T>` are the lifetime boundary composed bare while `Ref<T>` carriers and `DangerousGetReference` stay kernel-internal, and `DangerousGetArray` is the `ArraySegment<T>` handoff seam for the tensor-lane rented-array `Tensor.Create` factory and the `StreamPool` zero-copy `ByteString` wrap. Content hashing rides the suite `System.IO.Hashing` `XxHash3`/`XxHash128` owner, never a second staging-local `HashCode<T>` digest. This axis admits no `System.IO.Pipelines` route and no unowned buffer type without a row.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AllocationClass {
    public static readonly AllocationClass SpanStack = new("span-stack", syncOnly: true, copyReceipted: false);
    public static readonly AllocationClass PooledMemory = new("pooled-memory", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass RecyclableStream = new("recyclable-stream", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass NativeOrt = new("native-ort", syncOnly: false, copyReceipted: false);
    public static readonly AllocationClass EdgeCopy = new("edge-copy", syncOnly: false, copyReceipted: true);
    public static readonly AllocationClass DeviceWgpu = new("device-wgpu", syncOnly: false, copyReceipted: true);

    public bool SyncOnly { get; }

    public bool CopyReceipted { get; }

    public bool Admits(long requestedBytes, long payloadBound, bool async = false, Option<string> copyReason = default) =>
        requestedBytes <= payloadBound && !(SyncOnly && async) && (!CopyReceipted || copyReason.IsSome);

    public Fin<AllocationEvidence> Grant(
        CorrelationId correlation,
        long requestedBytes,
        long payloadBound,
        bool async = false,
        Option<string> copyReason = default,
        Option<string> nativeAllocator = default,
        Option<long> nativeReservedBytes = default) =>
        Admits(requestedBytes, payloadBound, async, copyReason)
            ? Fin.Succ(new AllocationEvidence(
                correlation, this, StagingEventKind.Grant, requestedBytes, requestedBytes,
                copyReason, nativeAllocator, nativeReservedBytes))
            : Fin.Fail<AllocationEvidence>(new ComputeFault.AllocationOverClass(Reject(requestedBytes, payloadBound, async, copyReason)));

    string Reject(long requested, long bound, bool async, Option<string> copyReason) =>
        requested > bound ? $"{Key}:over-bound:{requested}>{bound}"
        : SyncOnly && async ? $"{Key}:sync-only-in-async-lane"
        : $"{Key}:copy-reason-missing";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StagingEventKind {
    public static readonly StagingEventKind Grant = new("grant", diagnostic: false);
    public static readonly StagingEventKind StreamCreated = new("stream-created", diagnostic: false);
    public static readonly StagingEventKind StreamDisposed = new("stream-disposed", diagnostic: false);
    public static readonly StagingEventKind StreamLength = new("stream-length", diagnostic: false);
    public static readonly StagingEventKind BlockCreated = new("block-created", diagnostic: false);
    public static readonly StagingEventKind LargeBufferCreated = new("large-buffer-created", diagnostic: false);
    public static readonly StagingEventKind UsageReport = new("usage-report", diagnostic: false);
    public static readonly StagingEventKind StreamConvertedToArray = new("stream-converted-to-array", diagnostic: true);
    public static readonly StagingEventKind StreamOverCapacity = new("stream-over-capacity", diagnostic: true);
    public static readonly StagingEventKind StreamDoubleDisposed = new("stream-double-disposed", diagnostic: true);
    public static readonly StagingEventKind StreamFinalized = new("stream-finalized", diagnostic: true);
    public static readonly StagingEventKind BufferDiscarded = new("buffer-discarded", diagnostic: true);

    public bool Diagnostic { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct AllocationEvidence(
    CorrelationId Correlation,
    AllocationClass Class,
    StagingEventKind Kind,
    long RequestedBytes,
    long GrantedBytes,
    Option<string> Detail = default,
    Option<string> NativeAllocator = default,
    Option<long> NativeReservedBytes = default,
    Option<long> SmallPoolFreeBytes = default,
    Option<long> LargePoolFreeBytes = default);

// --- [BOUNDARIES] --------------------------------------------------------------------------
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

- [01]-[ADMISSION]: `AllocationClass.Grant` is the one staging edge — it evaluates `Admits` once against the intent-declared bound, stamps `AllocationEvidence` on success, and folds `ComputeFault.AllocationOverClass` with the discriminated reason on rejection; a call-site pool choice is the deleted form
- [02]-[STACK_RENT]: `SpanOwner<T>.Allocate(int, AllocationMode)` inside one synchronous kernel scope on the `SpanStack` row; the owner never crosses an await or iterator boundary, and `Admits(async: true)` rejects the row at admission
- [03]-[POOLED_RENT]: `MemoryOwner<T>.Allocate(int)` with `AllocationMode.Default` on the `PooledMemory` row; `Slice` projects windows; `Dispose` returns deterministically
- [04]-[INCREMENTAL_BUILD]: `ArrayPoolBufferWriter<T>`/`MemoryBufferWriter<T>` own growing payloads as the `IBufferWriter<T>` codec-emit sink on the `PooledMemory` row; `WrittenMemory`/`WrittenSpan` read the committed payload back zero-copy
- [05]-[RENT_CLEARING]: `AllocationMode.Default` everywhere — upstream classification keeps secret payloads out of staging, so clearing buys nothing; `ZeroOutBuffer` stays a `Diagnostic`-row policy
- [06]-[FOREIGN_EVIDENCE]: native and device grants pass `nativeAllocator`/`nativeReservedBytes` into `Grant` — the `NativeOrt` row carries the model-lane allocator name and reserved bytes and the `DeviceWgpu` row the `wgpu:<deviceId>` descriptor through the same slot pair
- [07]-[EDGE_COPY]: a `Grant` on the `EdgeCopy` row carries a copy reason or `Admits` rejects it; every array materialization and stream flatten routes through it, and the realized copy surfaces as the `StreamConvertedToArray` diagnostic event
- [08]-[TEXT_INTERNING]: `StringPool.GetOrAdd` interns receipt and diagnostic text at the sink edge only; `ReadOnlySpan<byte>.Fields`/`Tokenize` split codec text spans without intermediate strings
- [09]-[BIT_PACKING]: `StagingViews.Mark`/`Clear`/`Cell` set/test one occupancy bit and `Pack`/`Read` pack/extract a multi-bit material-id field over a `Span<ulong>` window of `PooledMemory` (sixty-four cells per word) through the branchless `BitHelper` `ref`-overloads — one bit per cell replaces a `byte` buffer and the tensor-lane `VoxelGrid` encoding stages the mask
- [10]-[IN_PLACE_GROWTH]: `ArrayPool<byte>.Grow` (over `ArrayPoolExtensions.EnsureCapacity`) grows the rented backing during incremental codec emit; the writer never reallocates through a second `MemoryOwner<T>.Allocate` and the granted-byte slot reflects the grown capacity
- [11]-[CONTIGUOUS_FRAME]: `StreamPool.Get(correlation, requiredSize, contiguous: true)` forces one large-buffer allocation for a chunked tensor frame the tensor lane requires contiguous; the `RecyclableStream` row carries it, `requiredSize` fills the granted-byte slot, and the route replaces a hand-rolled array concatenation of chunk frames

## [03]-[PLANE_VIEWS]

- Owner: `Span2D<T>`/`ReadOnlySpan2D<T>`/`Memory2D<T>`/`ReadOnlyMemory2D<T>` composed bare — the projection extensions and the plane's own row/axis members are the whole surface and zero local plane owner exists.
- Entry: `Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int height, int width)` and the padded `(offset, height, width, pitch)` stride overload; `Span2D<T> AsSpan2D<T>(this Span<T> span, int height, int width)`.
- Packages: CommunityToolkit.HighPerformance
- Growth: one projection overload per new staged plane shape; a padded stride is one policy value; a new row/axis kernel is one verified `Span2D<T>` member, never a local wrapper; zero new surface.
- Boundary: planes are views, never layout — rank permutation stays `Tensor/layout#LAYOUT_ALGEBRA` and a plane never substitutes for it, so the layout lane's densify gate reads `Contiguity.Classify` over the `Tensor<T>` stride facts, never a plane's `TryGetSpan` contiguity probe. `Cast`/`AsBytes` reinterpretation is legal only inside the rail owning the codec and its byte order; a reinterpreted payload never crosses a process boundary uncoded and a foreign-endian payload decodes first. Partitioned plane execution rides the `Tensor/dispatch#KERNEL_DISPATCH` `ParallelHelper.For2D` over a `Memory2D` plane and is never re-owned here. This cluster deletes package-local plane wrappers, 1-D index arithmetic over image rows, and copy-shaped pre/post buffers.

```csharp signature
public static Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int height, int width);
public static Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int offset, int height, int width, int pitch);
public static Span2D<T> AsSpan2D<T>(this Span<T> span, int height, int width);
public static Span<byte> AsBytes<T>(this Span<T> span) where T : unmanaged;
public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span) where TFrom : unmanaged where TTo : unmanaged;

// Span2D<T> own surface, composed bare (no local wrapper):
//   Span<T> GetRowSpan(int row);            ref T DangerousGetReference();
//   RefEnumerable<T> GetRow(int row);       RefEnumerable<T> GetColumn(int column);
//   bool TryGetSpan(out Span<T> span);      Span2D<T> Slice(int row, int column, int height, int width);
```

Each entry carries one ruling:

- [01]-[PROJECTION]: `AsMemory2D`/`AsSpan2D` give height-by-width planes over rented memory; the pitch overload carries padded image strides uncopied
- [02]-[ROW_KERNEL]: `GetRowSpan(int)` addresses one contiguous row; kernels fold row spans instead of indexing a flat buffer
- [03]-[AXIS_BY_REF]: `GetRow(int)`/`GetColumn(int)` give a `RefEnumerable<T>` over one plane axis (by ref, no copy) for a column-strided fold
- [04]-[CONTIGUITY_PROBE]: `TryGetSpan(out Span<T>)` reports whether a plane is contiguous; a true result takes the flat-span kernel, a false one the strided row walk
- [05]-[REINTERPRETATION]: `Cast` and `AsBytes` reinterpret in place under the calling codec's endianness ownership; a foreign-endian payload decodes first
- [06]-[LAYOUT_SPLIT]: NCHW-to-NHWC and every rank permute ride the tensor-lane layout family; planes stage rows for kernels, never reorder dimensions

## [04]-[STREAM_POOL]

- Owner: `StreamPool` boundary capsule owning the one process `RecyclableMemoryStreamManager`; `StreamPoolPolicy` carries every pool policy value; `PoolEvidence` the foreign-receiver projection folding the eleven manager events to `AllocationEvidence`.
- Entry: `RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default, bool contiguous = false)`.
- Auto: the constructor creates the manager from `policy.Options` and attaches all eleven manager events through `PoolEvidence.Project`, each event projecting its `EventArgs` to an `AllocationEvidence` value riding `ReceiptSinkPort.Send` with zero call-site code; `Get` passes the correlation as the `Guid` stream id on every path so every later event rejoins its intent by id, and the `RecyclableStream` row key is the tag.
- Receipt: double-dispose, finalization, and discarded-buffer events are leak diagnostics (the `StagingEventKind.Diagnostic` column), never log noise; an array-conversion event is the `StreamConvertedToArray` diagnostic corroborating an edge copy.
- Packages: Microsoft.IO.RecyclableMemoryStream, CommunityToolkit.HighPerformance, Google.Protobuf, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one policy value on `StreamPoolPolicy`; a new evidence slot is one `AllocationEvidence` field; a new manager event is one `PoolEvidence.Project` row in the constructor; zero new surface.
- Boundary: `StreamPool` is the named boundary capsule for the statement carve-out — the constructor's manager creation, event wiring, and detacher collection carry language-owned statement forms while every other member stays expression-shaped, and `StagingViews.Grow`'s `ref byte[]?` growth is the one further platform-forced statement seam. `PoolEvidence.Project` is the foreign-receiver/local-behavior extension form: the AppHost `ReceiptSinkPort` is the foreign receiver read only to stamp evidence while the subscription detacher holding the exact handler identity is Compute-owned behavior, so the block adds no second disposer registry and mutates no port state. One capsule per process composes as a singleton at the app root, the `Diagnostic` policy row binding on the debug and test-host profiles; memory, owners, writers, and sequences become streams only through the `AsStream` extension family at IO edges, and the package-internal stream classes never enter vocabulary. This capsule deletes per-call-site manager instances, raw `MemoryStream` construction, copy-shaped `ByteString.CopyFrom`, and unreceipted `ToArray` flattens.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
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

// --- [COMPOSITION] -------------------------------------------------------------------------
public sealed class StreamPool : IDisposable {
    readonly RecyclableMemoryStreamManager manager;
    readonly Seq<Action> detachers;

    public StreamPool(StreamPoolPolicy policy, ReceiptSinkPort sink, JsonSerializerOptions wire) {
        manager = new RecyclableMemoryStreamManager(policy.Options);
        detachers = Seq(
            sink.Project<RecyclableMemoryStreamManager.StreamCreatedEventArgs>(
                h => manager.StreamCreated += h, h => manager.StreamCreated -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.StreamCreated, a.RequestedSize, a.ActualSize)),
            sink.Project<RecyclableMemoryStreamManager.StreamDisposedEventArgs>(
                h => manager.StreamDisposed += h, h => manager.StreamDisposed -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.StreamDisposed, 0, 0, Detail: Some($"lifetime:{a.Lifetime}"))),
            sink.Project<RecyclableMemoryStreamManager.StreamLengthEventArgs>(
                h => manager.StreamLength += h, h => manager.StreamLength -= h, wire,
                a => new AllocationEvidence(CorrelationId.None, AllocationClass.RecyclableStream, StagingEventKind.StreamLength, a.Length, a.Length)),
            sink.Project<RecyclableMemoryStreamManager.StreamConvertedToArrayEventArgs>(
                h => manager.StreamConvertedToArray += h, h => manager.StreamConvertedToArray -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.EdgeCopy, StagingEventKind.StreamConvertedToArray, a.Length, 0, Detail: Optional(a.Stack))),
            sink.Project<RecyclableMemoryStreamManager.StreamOverCapacityEventArgs>(
                h => manager.StreamOverCapacity += h, h => manager.StreamOverCapacity -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.StreamOverCapacity, a.RequestedCapacity, a.MaximumCapacity, Detail: Optional(a.AllocationStack))),
            sink.Project<RecyclableMemoryStreamManager.BlockCreatedEventArgs>(
                h => manager.BlockCreated += h, h => manager.BlockCreated -= h, wire,
                a => new AllocationEvidence(CorrelationId.None, AllocationClass.RecyclableStream, StagingEventKind.BlockCreated, 0, a.SmallPoolInUse)),
            sink.Project<RecyclableMemoryStreamManager.LargeBufferCreatedEventArgs>(
                h => manager.LargeBufferCreated += h, h => manager.LargeBufferCreated -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.LargeBufferCreated, a.RequiredSize, a.LargePoolInUse, Detail: Some(a.Pooled ? "pooled" : "unpooled"))),
            sink.Project<RecyclableMemoryStreamManager.BufferDiscardedEventArgs>(
                h => manager.BufferDiscarded += h, h => manager.BufferDiscarded -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.BufferDiscarded, 0, 0, Detail: Some($"{a.BufferType}:{a.Reason}"))),
            sink.Project<RecyclableMemoryStreamManager.UsageReportEventArgs>(
                h => manager.UsageReport += h, h => manager.UsageReport -= h, wire,
                a => new AllocationEvidence(CorrelationId.None, AllocationClass.RecyclableStream, StagingEventKind.UsageReport, a.SmallPoolInUseBytes, a.LargePoolInUseBytes,
                    SmallPoolFreeBytes: Some(a.SmallPoolFreeBytes), LargePoolFreeBytes: Some(a.LargePoolFreeBytes))),
            sink.Project<RecyclableMemoryStreamManager.StreamDoubleDisposedEventArgs>(
                h => manager.StreamDoubleDisposed += h, h => manager.StreamDoubleDisposed -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.StreamDoubleDisposed, 0, 0, Detail: Optional(a.DisposeStack2))),
            sink.Project<RecyclableMemoryStreamManager.StreamFinalizedEventArgs>(
                h => manager.StreamFinalized += h, h => manager.StreamFinalized -= h, wire,
                a => new AllocationEvidence(CorrelationId.Create(a.Id), AllocationClass.RecyclableStream, StagingEventKind.StreamFinalized, 0, 0, Detail: Optional(a.AllocationStack))));
    }

    public RecyclableMemoryStream Get(CorrelationId correlation, Option<long> requiredSize = default, bool contiguous = false) =>
        requiredSize is { IsSome: true, Case: long size }
            ? manager.GetStream(correlation, AllocationClass.RecyclableStream.Key, size, asContiguousBuffer: contiguous)
            : manager.GetStream(correlation, AllocationClass.RecyclableStream.Key);

    public void Dispose() => detachers.Rev().Iter(static detach => detach());
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
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
        sink.Send(e.Correlation, TenantContext.Current, TelemetrySource.Compute.Key, e.Kind.Key, JsonSerializer.SerializeToElement(e, wire));
}
```

Each manager event projects one evidence ruling:

- [01]-[STREAMCREATED]: `RequestedSize`/`ActualSize` fill the requested/granted byte pair under the stream's correlation `Id` (the `Get`-set `Guid`)
- [02]-[STREAMDISPOSED]: `Lifetime` rides `Detail` and closes the grant; the dispose pairs its create by `Id`
- [03]-[STREAMLENGTH]: `Length` fills both byte slots as the staged-payload size; the event carries no `Id`, so it stamps under `CorrelationId.None`
- [04]-[STREAMCONVERTEDTOARRAY]: edge-copy diagnostic on the `EdgeCopy` row — `Length` and the optional `Stack` corroborate the edge copy `StagingViews` cannot intercept
- [05]-[STREAMOVERCAPACITY]: `RequestedCapacity`/`MaximumCapacity` fill the byte pair; the over-cap diagnostic is the bounded-payload guard the admission rail reads
- [06]-[BLOCKCREATED]: `SmallPoolInUse` fills the granted slot tracking small-pool growth; no `Id`, so `CorrelationId.None`
- [07]-[LARGEBUFFERCREATED]: `RequiredSize`/`LargePoolInUse` fill the byte pair and `Pooled` rides `Detail` — surfaces an unpooled large grant the moment it happens
- [08]-[BUFFERDISCARDED]: `BufferType:Reason` rides `Detail` as the discard taxonomy diagnostic
- [09]-[USAGEREPORT]: the four pool byte gauges feed steady-state telemetry — in-use fills the byte pair, free fills the `SmallPoolFreeBytes`/`LargePoolFreeBytes` slots
- [10]-[STREAMDOUBLEDISPOSED]: leak diagnostic — `DisposeStack2` rides `Detail` on the `Diagnostic` policy row
- [11]-[STREAMFINALIZED]: leak diagnostic — an undisposed stream reached the finalizer; `AllocationStack` rides `Detail` when call stacks are on

Each entry carries one ruling:

- [01]-[FRAGMENTED_READ]: `GetReadOnlySequence` is the default read of staged bytes; segments map one-to-one onto pooled blocks (single-block and large-buffer streams collapse to one segment) and wire encode and decode never flatten the payload
- [02]-[ZERO_COPY_EDGE]: the remote edge wraps sequence windows with `UnsafeByteOperations.UnsafeWrap` under the frame law the remote lane owns
- [03]-[CODEC_WINDOW]: `TryGetBuffer` exposes a contiguous window for codecs bounded by `MaximumBufferSize`; `WriteTo` is the array-free stream-to-stream copy
- [04]-[BLOCK_ALIGNMENT]: `BlockSize` holds exactly two 64 KiB `ArtifactSync` frames, so a frame never straddles a pooled block
- [05]-[PAYLOAD_CAP]: `MaximumBufferSize` equals the wire payload cap from the canonical channel policy; large buffers step in 1 MiB multiples to that cap
- [06]-[STREAM_CAP]: `MaximumStreamCapacity` zero is the package no-limit spelling; per-intent payload bounds own staging caps at admission through `AllocationClass.Grant`
- [07]-[POOL_RETENTION]: the free-bytes caps pin retention to 128 pooled blocks and eight payload-cap buffers; returns beyond them release as `BufferDiscarded` events
- [08]-[CONTIGUOUS_VIEW]: `GetBuffer` exposes the whole stream as one array when the codec needs a contiguous backing past `MaximumBufferSize`; the call is array-free against pooled blocks and never copies, where `TryGetBuffer` caps at one block
- [09]-[SEGMENT_HANDOFF]: `MemoryOwner<byte>.DangerousGetArray` hands the rented `ArraySegment<byte>` to `UnsafeByteOperations.UnsafeWrap` so a pooled payload becomes a `ByteString` with zero copy; the owner outlives the wrap and disposes after send
- [10]-[BLOCK_DIAGNOSTIC]: `BlockAndOffset`/`BlockSegment` address pooled-block boundaries on the `Diagnostic` policy row so a frame-straddle assertion reads exact block positions; production reads only `GetReadOnlySequence` segment counts

Every manager event folds to evidence through one `ReceiptSinkPort.Send` with no per-event allocation: the handler closure captures the sink once, projects its `EventArgs` to an `AllocationEvidence` value, threads the ambient `TenantContext.Current` beside the renter correlation, and stamps `TelemetrySource.Compute.Key` and the `StagingEventKind.Key`; the detacher chain detaches LIFO at dispose. Per-stream events carry the stream's `Guid Id` back through `CorrelationId.Create` (the `[ValueObject<Guid>]` factory — no `Guid`-to-`CorrelationId` operator exists), while the pool-scoped `BlockCreated`/`StreamLength`/`UsageReport` events carry no stream id and stamp under `CorrelationId.None`, so a pool-pressure gauge is process-scoped and a per-stream leak renter-attributable.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [RETENTION_UNDER_CHURN]-[OPEN]: under sustained model-lane token-stream churn, does eager `AggressiveBufferReturn` trade a higher `LargeBufferCreated`/`BufferDiscarded` rate for a lower resident set, or does the `MaximumLargePoolFreeBytes` retention floor absorb it; resolve against a live generative workload profile through the `UsageReport` free-pool gauges.
