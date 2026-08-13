# [COMPUTE_RESIDENCY]

ONNX C-data residency classifies every `OrtValue` by backing location and ownership through one `OrtResidency` lattice. `TensorBridge` owns carrier-shaped ingress and dtype-shaped egress, `DeviceMemory` owns shared allocation, `BoundFlow` owns steady-state `OrtIoBinding`, and `EncodedTensor` owns model-lane geometry wrapping without repacking the kernel payload.

## [01]-[INDEX]

- [02]-[ORT_BRIDGE]: `OrtResidency` lattice; carrier-keyed C-data ingress and dtype-keyed egress; `PinnedPlane<T>` handle-rooted pin for the crossings outliving their statement; `DeviceMemory` shared-allocator descriptor and residency probe; `BoundFlow` gate-aware `OrtIoBinding` steady-state.
- [03]-[GEOMETRY_ENCODING]: `EncodedGeometry` wraps the kernel payload host-neutral; `EncodedTensor` slices per channel and `PackKind` fixes wire shape, layout, and free-dimension names.

## [02]-[ORT_BRIDGE]

- Owner: `OrtResidency` `[SmartEnum<string>]` the five-gate residency lattice; `TensorBridge` the static `OrtValue` C-data factory surface (carrier-keyed ingress, dtype-keyed egress, the device descriptor, the residency probe); `PinnedPlane<T>` the ONE handle-rooted pin capsule every crossing that outlives its own statement takes; `BoundFlow` the ONE `OrtIoBinding` steady-state residency capsule the `Model/inference#INFERENCE_MODES` run-mode fold composes.
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source)` and its `MemoryOwner<T>`, foreign-pointer, and `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` overloads discriminate ingress by carrier shape; `public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape)` mints a device sink; `public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed)` moves a device-resident pair whole on the producing device's own sync stream; `public static Fin<PinnedPlane<T>> Pin<T>(Tensor<T> source, TensorDtype row)` roots a managed plane on a `MemoryHandle` for a crossing outliving its statement and the paired `Ingress(OrtMemoryInfo, TensorDtype, ReadOnlySpan<long>, PinnedPlane<T>)` overload hands that rooted pointer to the C-data factory; `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` and its flat `Span<T>` overload project an output by the dtype row; `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena, TensorDtype row)` leases the steady-state capsule (the bound input and sink allocate from the supplied shared arena — the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` for the model lane — never a managed staging plane), the dtype row a required argument so the capsule is dtype-polymorphic with no defaulted sibling to disagree with the session — `Fin<T>` aborts when the egress destination is undersized against the `GetTensorSizeInBytes` count, ingress shape volume fails to cover its payload (`ingress-cover-gap`), or a native mint rejects (`ingress-rejected` — every C-data factory call crosses `Try.lift` once); the leased flow is a disposable capsule whose `Dispose` is the bound backing's release point, and `Lease` releases every already-acquired native handle on its own failure path so a `lease-rejected` fault strands nothing.
- Receipt: `CopyPoint` stamps the `OrtResidency` gate, native byte count, device name, instant, and `CorrelationId`; `CopyPoint.Receipt` projects that evidence onto `ComputeReceipt.Copy`, and `ReceiptFolds.Crossings` aggregates it by gate.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule)
- Growth: a new accelerator is one `DeviceMemory` descriptor over its `OrtEpDevice` reaching the existing `Allocate`/device-pointer ingress, never a per-call marshal helper; a new carrier is one `TensorBridge.Ingress` overload discriminating by carrier shape (the `Model/inference#INFERENCE_MODES` `RunInput` cases compose these overloads, never re-spelling a factory); the `DeviceResident` row is the one residency gate the `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row and the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` both bind — a WGPU compute buffer and an ORT device value share this one residency row so device-ness is a residency discriminant, never a second tensor owner or a parallel device-residency lattice; the resolved shared `ONE_WGPU_DEVICE` adapter is what a composition root folds into the `device-wgpu` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` (present iff the adapter resolves), so the same device-presence fact the `DeviceResident` gate observes contributes the substrate key the `Substrate.DeviceWgpu` `!Providers.Contains(Key)` gate reads, never a raw `Device`/adapter handle pushed into `Providers`; a device-resident chain link is one `Relay` call over the pair the residency lattice already classifies, never a second copy owner; zero new surface.
- Boundary: `OrtValue` is the sole model-boundary carrier. Every ingress shape proves non-negative extents, checked volume, payload coverage, and native construction on `Fin`; zero-sized tensors remain representable. Buffer ROOTING splits by ownership: `Tensor<T>.GetPinnableReference` roots a `fixed` region and serves the in-statement copy alone, so a managed plane whose pointer an `OrtValue` or a device submission holds past that region roots on `PinnedPlane<T>` instead, and the raw-`nint` ingress overload is reserved for genuinely FOREIGN memory (a device allocation, an ORT arena block) the caller can neither own nor pin — a managed buffer reaching that overload is the deleted unrooted form. A strided plane hands no contiguous pointer, so the pin repacks once through the non-throwing `TryFlattenTo` into a rental the capsule releases with the handle; a plane with no dense dimension flattens element-wise, so that walk carries a stated ceiling and refuses `pin-strided-oversize` past it rather than paying an unannounced traversal. Every egress proves dtype identity, native byte count, and destination density where raw-byte projection requires it. `BoundFlow.Write<T>` and framed-byte `Write` return `Fin<Unit>`, enforce exact dtype and length, and let `Flow` abort before `Drive`. Rebind operations allocate replacements before clearing current bindings, restore prior CPU bindings on failure, and transfer ownership only after successful binding. Foreign custody is ONE clause: `RebindDevice` and `RebindExternal` bind memory the CALLER owns — an `OrtMemoryAllocation` or an `OrtExternalAllocation` whose lifetime must outlive both the binding and this capsule — so the capsule releases neither and only clears its own `foreignInput` slot, while the `OrtValue` that `RebindDevicePointer` mints over a raw device pointer is capsule-owned and releases at the next rebind or at `Dispose`. `Dispose` releases each owned native handle once. Gate selection derives from the session's own `OrtMemoryInfo` through `OrtResidency.Classify` — a caller-declared gate the model contradicts is the deleted form, and the allocator name stays receipt evidence rather than a discriminant. A relay proves BOTH ends device-resident through `OrtResidency.Classify` before any native copy — a caller-declared residency the model contradicts is the deleted form the gate already names.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OrtResidency {
    public static readonly OrtResidency ManagedSpan = new("managed-span", wraps: true, device: false, projectsInPlace: false, foreign: false);
    public static readonly OrtResidency MemoryBacked = new("memory-backed", wraps: true, device: false, projectsInPlace: true, foreign: false);
    public static readonly OrtResidency DeviceResident = new("device-resident", wraps: false, device: true, projectsInPlace: true, foreign: false);
    public static readonly OrtResidency OutputValue = new("output-value", wraps: false, device: false, projectsInPlace: true, foreign: false);
    public static readonly OrtResidency SpanView = new("span-view", wraps: true, device: false, projectsInPlace: true, foreign: true);

    public bool Wraps { get; }
    public bool Device { get; }
    public bool ProjectsInPlace { get; }
    public bool Foreign { get; }

    // OrtResidency owns its own native discriminant: OrtMemType.CpuOutput marks a session-minted output whatever
    // its allocator, an arena over HOST_ACCESSIBLE memory stays host-side, and every other device memory class is
    // device-resident. Reading the allocator NAME instead reads a vendor string, never a class.
    public static OrtResidency Classify(OrtMemoryInfo info) =>
        info.GetMemoryType() is OrtMemType.CpuOutput ? OutputValue
        : info.GetDeviceMemoryType() is OrtDeviceMemoryType.HOST_ACCESSIBLE
          || info.GetAllocatorType() is OrtAllocatorType.ArenaAllocator && info.GetMemoryType() is OrtMemType.Cpu or OrtMemType.CpuInput ? MemoryBacked
        : DeviceResident;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct CopyPoint(OrtResidency Gate, long Bytes, string Device, Instant At, CorrelationId Correlation) {
    public ComputeReceipt.Copy Receipt(WorkLane lane, Duration elapsed) =>
        new(Gate, Bytes, Device) { Scope = new ReceiptScope.Execution(Correlation, lane, Substrate.Onnx, AllocationClass.NativeOrt, elapsed) };
}

// Shared ORT allocators are ModelSessions-owned: `ModelSessions.SharedAllocator` mints and maps the per-(device,
// memory) arena and its drain lifecycle releases it — a residency-local `CreateSharedAllocator` would mint a second
// unmapped arena the drain never releases, the deleted double-owner form.
public readonly record struct DeviceMemory(OrtEpDevice Device, OrtDeviceMemoryType MemoryType, OrtAllocatorType AllocatorType) {
    public OrtMemoryInfo Info => Device.GetMemoryInfo(MemoryType);

    public Fin<OrtAllocator> Shared() =>
        Try.lift(() => ModelSessions.SharedAllocator(Device, MemoryType))
            .Run().MapFail(static error => TensorFault.Symbol("allocator-rejected", error.Message));

    public Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(TensorDtype row, long[] shape) {
        try {
            OrtAllocator allocator = ModelSessions.SharedAllocator(Device, MemoryType);
            return Fin.Succ((allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape)));
        }
        catch (Exception ex) {
            return TensorFault.Fail<(OrtAllocator, OrtValue)>("allocator-rejected", row.Key, ex.Message);
        }
    }
}

// Handle-rooted pin for a native crossing that OUTLIVES its statement. `GetPinnableReference` roots a `fixed`
// region alone, so an `OrtValue` holding the pointer past that region, or a device submit draining after the
// frame, reads freed memory; `GetPinnedHandle` roots the same buffer on a `MemoryHandle` this capsule releases
// at its own end. A strided plane hands no contiguous pointer at all, so it repacks once through the
// non-throwing `TryFlattenTo` into a pooled rental the capsule also owns — never a stride-ignoring reinterpret.
public sealed class PinnedPlane<T> : IDisposable where T : unmanaged {
    // Flattening a fully strided plane walks element-wise, so the repack ceiling bounds that walk rather than paying it
    // silently over a plane sized in gigabytes.
    private const long StridedRepackCeiling = 1L << 26;

    private readonly MemoryHandle handle;
    private readonly MemoryOwner<T>? rental;
    private bool disposed;

    private PinnedPlane(MemoryHandle handle, MemoryOwner<T>? rental, long elements, long bytes, bool repacked) =>
        (this.handle, this.rental, Elements, Bytes, Repacked) = (handle, rental, elements, bytes, repacked);

    public long Elements { get; }
    public long Bytes { get; }
    public bool Repacked { get; }

    public unsafe nint Pointer => (nint)handle.Pointer;

    public static Fin<PinnedPlane<T>> Of(Tensor<T> plane, TensorDtype row) {
        if (plane is null || row is null) { return TensorFault.Fail<PinnedPlane<T>>("pin-null"); }
        if (row.Clr != typeof(T)) { return TensorFault.Fail<PinnedPlane<T>>("pin-dtype", row.Key, typeof(T).Name); }
        if (row.OrtElementBytes <= 0) { return TensorFault.Fail<PinnedPlane<T>>("pin-byte-stride", row.Key); }
        long elements = plane.FlattenedLength;
        if (elements > long.MaxValue / row.OrtElementBytes) { return TensorFault.Fail<PinnedPlane<T>>("pin-volume-overflow", row.Key); }
        long bytes = elements * row.OrtElementBytes;
        return plane.IsDense
            ? Rooted(() => new PinnedPlane<T>(plane.GetPinnedHandle(), null, elements, bytes, repacked: false), row)
            : Repack(plane, row, elements, bytes);
    }

    // Dense-run screening decides the repack COST: a plane with no dense dimension walks scalar strides, and
    // that walk is bounded rather than discovered at the copy.
    static Fin<PinnedPlane<T>> Repack(Tensor<T> plane, TensorDtype row, long elements, long bytes) {
        if (elements > int.MaxValue) { return TensorFault.Fail<PinnedPlane<T>>("pin-strided-width", row.Key, elements.ToString(CultureInfo.InvariantCulture)); }
        if (!plane.HasAnyDenseDimensions && elements > StridedRepackCeiling) {
            return TensorFault.Fail<PinnedPlane<T>>("pin-strided-oversize", row.Key, $"{elements}>{StridedRepackCeiling}");
        }

        MemoryOwner<T> rental = MemoryOwner<T>.Allocate((int)elements);
        if (!plane.TryFlattenTo(rental.Span)) {
            rental.Dispose();
            return TensorFault.Fail<PinnedPlane<T>>("pin-flatten", row.Key, $"rank={plane.Rank}");
        }

        return Rooted(() => new PinnedPlane<T>(rental.Memory.Pin(), rental, elements, bytes, repacked: true), row)
            .MapFail(fault => { rental.Dispose(); return fault; });
    }

    static Fin<PinnedPlane<T>> Rooted(Func<PinnedPlane<T>> root, TensorDtype row) =>
        Try.lift(root).Run().MapFail(error => TensorFault.Symbol("pin-rejected", $"{row.Key}:{error.Message}"));

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        handle.Dispose();
        rental?.Dispose();
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorBridge {
    public static Fin<OrtValue> Ingress<T>(Tensor<T> source) where T : unmanaged =>
        Minted(() => OrtValue.CreateTensorValueFromSystemNumericsTensorObject(source));

    public static Fin<OrtValue> Ingress<T>(T[] data, ReadOnlySpan<long> shape) where T : unmanaged =>
        Covered(shape, data.Length).Bind(admitted => Minted(() => OrtValue.CreateTensorValueFromMemory(data, admitted)));

    public static Fin<OrtValue> Ingress<T>(MemoryOwner<T> backing, ReadOnlySpan<long> shape) where T : unmanaged =>
        Covered(shape, backing.Length).Bind(admitted => Minted(() => OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, backing.Memory, admitted)));

    // Foreign pointer: memory the caller does not own and cannot pin — a device allocation or an ORT arena
    // block. A MANAGED buffer reaches the same factory only through `Pin`, whose handle outlives the value.
    public static Fin<OrtValue> Ingress(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, nint data, long sizeInBytes) =>
        CoveredBytes(shape, row, sizeInBytes).Bind(admitted =>
            Minted(() => OrtValue.CreateTensorValueWithData(memory, row.Element, admitted, data, sizeInBytes)));

    public static Fin<PinnedPlane<T>> Pin<T>(Tensor<T> source, TensorDtype row) where T : unmanaged =>
        PinnedPlane<T>.Of(source, row);

    public static Fin<OrtValue> Ingress<T>(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, PinnedPlane<T> pinned) where T : unmanaged =>
        pinned is null
            ? TensorFault.Fail<OrtValue>("ingress-unpinned", row.Key)
            : CoveredBytes(shape, row, pinned.Bytes).Bind(admitted =>
                Minted(() => OrtValue.CreateTensorValueWithData(memory, row.Element, admitted, pinned.Pointer, pinned.Bytes)));

    public static Fin<OrtValue> Ingress(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> tokens) =>
        Minted(() => OrtValue.CreateFromStringTensor(tokens));

    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        Shape(shape).Bind(admitted => device.Allocate(row, admitted.Shape));

    // Device-to-device handoff for a chain whose links stay RESIDENT. `CopyTensors` moves whole values on the
    // device under one sync stream the producing device itself mints, so a producer sink and a consumer bound input
    // sharing an `OrtMemoryInfo` never cross device→host→device between links. Residency is PROVED, never declared:
    // both ends classify through `OrtResidency.Classify`, and a pair whose memory descriptors disagree is a
    // `relay-residency` refusal rather than a silent host round trip. The stream disposes with the copy, because a
    // stream outliving its transfer is a device handle no capsule owns.
    public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed) =>
        (OrtResidency.Classify(produced.GetTensorMemoryInfo()), OrtResidency.Classify(consumed.GetTensorMemoryInfo())) switch {
            (OrtResidency source, OrtResidency sink) when source.Device && sink.Device =>
                Try.lift(() => {
                    using OrtSyncStream stream = device.Device.CreateSyncStream(FrozenDictionary<string, string>.Empty);
                    OrtEnv.Instance().CopyTensors([produced], [consumed], stream);
                    return unit;
                }).Run().MapFail(static error => TensorFault.Symbol("relay-rejected", error.Message)),
            (OrtResidency source, OrtResidency sink) =>
                TensorFault.Fail<Unit>("relay-residency", source.Key, sink.Key),
        };

    // Shape covers the payload before any native mint, and every C-data factory call crosses once into the
    // rail — a native rejection lands as a typed fault, never an exception under an announced Succ.
    private static Fin<long[]> Covered(ReadOnlySpan<long> shape, long payload) =>
        Shape(shape).Bind(admitted => admitted.Volume == payload
            ? Fin.Succ(admitted.Shape)
            : TensorFault.Fail<long[]>("ingress-cover-gap", $"{payload}!={admitted.Volume}"));

    private static Fin<long[]> CoveredBytes(ReadOnlySpan<long> shape, TensorDtype row, long payloadBytes) =>
        Shape(shape).Bind(admitted =>
            row.OrtElementBytes <= 0 ? TensorFault.Fail<long[]>("ingress-byte-stride", row.Key)
            : admitted.Volume > long.MaxValue / row.OrtElementBytes ? TensorFault.Fail<long[]>("ingress-volume-overflow", row.Key)
            : admitted.Volume * row.OrtElementBytes != payloadBytes
                ? TensorFault.Fail<long[]>("ingress-cover-gap", row.Key, $"{payloadBytes}!={admitted.Volume}x{row.OrtElementBytes}")
                : Fin.Succ(admitted.Shape));

    private static Fin<(long[] Shape, long Volume)> Shape(ReadOnlySpan<long> shape) {
        long[] admitted = shape.ToArray();
        long volume = 1;
        try {
            foreach (long extent in admitted) {
                if (extent < 0) { return TensorFault.Fail<(long[], long)>("ingress-shape", extent.ToString(CultureInfo.InvariantCulture)); }
                volume = checked(volume * extent);
            }
            return Fin.Succ((admitted, volume));
        }
        catch (OverflowException) { return TensorFault.Fail<(long[], long)>("ingress-volume-overflow", $"rank={admitted.Length}"); }
    }

    private static Fin<OrtValue> Minted(Func<OrtValue> mint) =>
        Try.lift(mint).Run().MapFail(static error => TensorFault.Symbol("ingress-rejected", error.Message));

    // Ref-struct destinations cross no lambda, so the projection body is the named REF_SAFE statement seam:
    // admission stays on the rail, the copy runs in place, and a native rejection converts once.
    public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination) where T : unmanaged {
        long flattened = destination.FlattenedLength;
        Fin<TensorDtype> admitted = TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count != flattened ? TensorFault.Fail<TensorDtype>("egress-undersized", row.Key, $"{count}!={flattened}") : Fin.Succ(row)));
        if (admitted.Case is not TensorDtype row) { return admitted.Map(static _ => unit); }
        if (row.Clr != typeof(T)) { return TensorFault.Fail<Unit>("egress-dtype", row.Key, typeof(T).Name); }
        if (row.Quantized && !destination.IsDense) { return TensorFault.Fail<Unit>("egress-strided-quantized", row.Key); }
        try {
            if (row.Quantized) { value.GetTensorDataAsSpan<T>().CopyTo(MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)flattened))); }
            else { value.GetTensorDataAsTensorSpan<T>().CopyTo(destination); }
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return TensorFault.Fail<Unit>("egress-rejected", row.Key, ex.Message); }
    }

    public static Fin<Unit> Egress<T>(OrtValue value, Span<T> destination) where T : unmanaged {
        int length = destination.Length;
        Fin<TensorDtype> admitted = TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count == length ? Fin.Succ(row) : TensorFault.Fail<TensorDtype>("egress-undersized", row.Key, $"{count}!={length}")));
        if (admitted.Case is not TensorDtype row) { return admitted.Map(static _ => unit); }
        if (row.Clr != typeof(T)) { return TensorFault.Fail<Unit>("egress-dtype", row.Key, typeof(T).Name); }
        try { value.GetTensorDataAsSpan<T>().CopyTo(destination); return Fin.Succ(unit); }
        catch (Exception ex) { return TensorFault.Fail<Unit>("egress-rejected", row.Key, ex.Message); }
    }

    public static CopyPoint Stamp(OrtValue value, OrtResidency gate, IClock clock, CorrelationId correlation) =>
        new(gate, value.GetTensorSizeInBytes(), value.GetTensorMemoryInfo().Name, clock.GetCurrentInstant(), correlation);

    // Residency reports the CLASSIFIED gate per I/O, so a caller binds off the session's own descriptors rather
    // than declaring a gate the model then contradicts; Name rides along as the arena the Copy receipt stamps.
    public static (Seq<(string Name, OrtResidency Gate)> Inputs, Seq<(string Name, OrtResidency Gate)> Outputs) Residency(InferenceSession session) {
        using IDisposableReadOnlyCollection<OrtMemoryInfo> inputs = session.GetMemoryInfosForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> outputs = session.GetMemoryInfosForOutputs();
        return (toSeq(inputs).Map(static info => (info.Name, OrtResidency.Classify(info))),
                toSeq(outputs).Map(static info => (info.Name, OrtResidency.Classify(info))));
    }

    // ONE bind: the dtype row is a required argument, never a defaulted float32 sibling. A convenience overload
    // that picks the row for the caller binds a session's real element type to whatever the sibling assumed, and
    // the mismatch surfaces as a byte-count refusal far from the call that chose it.
    public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena, TensorDtype row) =>
        BoundFlow.Lease(session, inputName, outputName, shape.ToArray(), arena, row);
}

// --- [COMPOSITION] -------------------------------------------------------------------------
public sealed class BoundFlow : IDisposable {
    private readonly InferenceSession session;
    private readonly OrtIoBinding binding;
    private readonly RunOptions run;
    private readonly OrtAllocator arena;
    private readonly TensorDtype row;
    private readonly string inputName, outputName;
    private OrtValue bound, sink;
    private OrtValue? foreignInput;
    private bool disposed;

    private BoundFlow(InferenceSession session, OrtIoBinding binding, RunOptions run, OrtAllocator arena, TensorDtype row, string inputName, string outputName, OrtValue bound, OrtValue sink) =>
        (this.session, this.binding, this.run, this.arena, this.row, this.inputName, this.outputName, this.bound, this.sink) = (session, binding, run, arena, row, inputName, outputName, bound, sink);

    // Leak-safe acquisition: every native handle acquired before the capsule exists releases on the failure
    // path, so a returned BoundFlow is the sole owner and a failed lease strands nothing.
    public static Fin<BoundFlow> Lease(InferenceSession session, string inputName, string outputName, long[] shape, OrtAllocator arena, TensorDtype row) {
        OrtValue? bound = null;
        OrtValue? sink = null;
        RunOptions? run = null;
        OrtIoBinding? binding = null;
        try {
            bound = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
            sink = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
            run = new RunOptions();
            binding = session.CreateIoBinding();
            binding.BindInput(inputName, bound);
            binding.BindOutput(outputName, sink);
            return Fin.Succ(new BoundFlow(session, binding, run, arena, row, inputName, outputName, bound, sink));
        }
        catch (Exception ex) {
            binding?.Dispose(); run?.Dispose(); sink?.Dispose(); bound?.Dispose();
            return TensorFault.Fail<BoundFlow>("lease-rejected", row.Key, ex.Message);
        }
    }

    public Fin<Unit> Write<T>(ReadOnlySpan<T> payload) where T : unmanaged {
        if (row.Clr != typeof(T)) { return TensorFault.Fail<Unit>("bound-dtype", row.Key, typeof(T).Name); }
        try {
            Span<T> destination = bound.GetTensorMutableDataAsSpan<T>();
            if (payload.Length != destination.Length) { return TensorFault.Fail<Unit>("bound-length", row.Key, $"{payload.Length}!={destination.Length}"); }
            payload.CopyTo(destination);
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return TensorFault.Fail<Unit>("bound-write", row.Key, ex.Message); }
    }

    public Fin<Unit> Write(ReadOnlySequence<byte> window) {
        try {
            Span<byte> destination = bound.GetTensorMutableRawData();
            if (window.Length != destination.Length) { return TensorFault.Fail<Unit>("bound-length", row.Key, $"{window.Length}!={destination.Length}"); }
            window.CopyTo(destination);
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return TensorFault.Fail<Unit>("bound-write", row.Key, ex.Message); }
    }

    private void Drive(RunOptions options) {
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(options, binding);
        binding.SynchronizeBoundOutputs();
    }

    // Native drive and output collection throw, so both cross the rail exactly like `Flow` does: an announced
    // `Succ` holding an escaping ORT exception is the one shape this capsule never publishes.
    public Fin<IDisposableReadOnlyCollection<OrtValue>> Run(RunOptions options) {
        try { Drive(options); return Fin.Succ(binding.GetOutputValues()); }
        catch (Exception ex) { return TensorFault.Fail<IDisposableReadOnlyCollection<OrtValue>>("bound-run", row.Key, ex.Message); }
    }

    public Fin<Unit> Flow<T>(ReadOnlySpan<T> input, in TensorSpan<T> output) where T : unmanaged {
        Fin<Unit> written = Write(input);
        if (written.Case is not Unit) { return written; }
        try { Drive(run); }
        catch (Exception ex) { return TensorFault.Fail<Unit>("bound-run", row.Key, ex.Message); }
        return TensorBridge.Egress(sink, output);
    }

    public Fin<Unit> Rebind(long[] shape) {
        OrtValue? nextBound = null;
        OrtValue? nextSink = null;
        try {
            nextBound = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
            nextSink = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, nextBound);
            binding.BindOutput(outputName, nextSink);
            OrtValue priorBound = bound;
            OrtValue priorSink = sink;
            bound = nextBound;
            sink = nextSink;
            nextBound = null;
            nextSink = null;
            foreignInput?.Dispose();
            foreignInput = null;
            priorBound.Dispose();
            priorSink.Dispose();
            return Fin.Succ(unit);
        }
        catch (Exception ex) {
            Fin<Unit> restored = Restore("rebind-rejected", ex);
            nextSink?.Dispose();
            nextBound?.Dispose();
            return restored;
        }
    }

    public Fin<Unit> RebindDevice(TensorElementType dtype, long[] shape, OrtMemoryAllocation deviceInput, OrtMemoryInfo deviceOutput) {
        try {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, dtype, shape, deviceInput);
            binding.BindOutputToDevice(outputName, deviceOutput);
            foreignInput?.Dispose();
            foreignInput = null;
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Restore("rebind-device", ex); }
    }

    public Fin<Unit> RebindDevicePointer(TensorElementType dtype, long[] shape, OrtMemoryInfo deviceInfo, nint pointer, long sizeInBytes) {
        OrtValue? next = null;
        try {
            next = OrtValue.CreateTensorValueWithData(deviceInfo, dtype, shape, pointer, sizeInBytes);
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, next);
            binding.BindOutputToDevice(outputName, deviceInfo);
            foreignInput?.Dispose();
            foreignInput = next;
            next = null;
            return Fin.Succ(unit);
        }
        catch (Exception ex) {
            Fin<Unit> restored = Restore("rebind-device-pointer", ex);
            next?.Dispose();
            return restored;
        }
    }

    public Fin<Unit> RebindExternal(OrtExternalAllocation input, OrtExternalAllocation output) {
        try {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, input);
            binding.BindOutput(outputName, output);
            foreignInput?.Dispose();
            foreignInput = null;
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Restore("rebind-external", ex); }
    }

    public Fin<IDisposableReadOnlyCollection<OrtValue>> Outputs() {
        try { return Fin.Succ(binding.GetOutputValues()); }
        catch (Exception ex) { return TensorFault.Fail<IDisposableReadOnlyCollection<OrtValue>>("bound-outputs", row.Key, ex.Message); }
    }

    private Fin<Unit> Restore(string symbol, Exception failure) {
        try {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, bound);
            binding.BindOutput(outputName, sink);
            return TensorFault.Fail<Unit>(symbol, row.Key, failure.Message);
        }
        catch (Exception restore) { return TensorFault.Fail<Unit>("rebind-restore", row.Key, restore.Message); }
    }

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        foreignInput?.Dispose();
        run.Dispose();
        binding.Dispose();
        bound.Dispose();
        sink.Dispose();
    }
}
```

## [03]-[GEOMETRY_ENCODING]

- Owner: `EncodedTensor` — the model-lane wrap holding the kernel `Rasm.Drawing.EncodedGeometry` WHOLE beside its layout row; the per-`PackKind` `Wire` mapping is the canonical geometry-ML input vocabulary.
- Entry: `Of(EncodedGeometry, PackKind)` derives only provable dimensions (`N`, point/mesh `V`, channel `C`, and indexed-face `F`) and refuses a row the `Wire` `Derivable` column marks unreachable, naming `Of(EncodedGeometry, PackKind, Option<Seq<(string Name, long Extent)>>, Option<Tensor<long>>)` — the entry carrying explicit spatial dimensions without default ghosts. `Fin<T>` rejects lossy witnesses, absent wire rows, non-positive or mismatched dimensions, underivable `U`/`V` and `H`/`W` grids, invalid channel ranges, and overflowed interleaving shapes.
- Receipt: the kernel `EncodedGeometry.Witness` is the lossless-round-trip proof, its `ContentHash` keyed by the witness's own `DigestRoot` — the `Spatial/reconciliation#RECONCILIATION_BRIDGE` source digest on an `Encode.Apply` mint, the packed-payload digest on an `Encode.Of` raw-lane mint (the interchange arenas), so a consumer keying dedup on the hash reads `Root` before comparing; `Of` admits only a lossless payload, so the residency wrap carries no second witness and mints no second content key.
- Packages: Rasm (project), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new representation lands as one kernel `PackKind` row (the kernel `Rasm.Drawing` owner adds it with its active-channel column) and one `Wire` row here carrying its `LayoutForm`/`WireShape`/free-dimension names and its `Derivable` truth — the `Field` (`geodesic`+`weight` lanes, positions omitted because the witness digest binds the source mesh) and `Toolpath` (`position`+`arc-center`+`arc-sense`+`weight`, so an analytic arc survives packing as content rather than sampled chords) rows the `Rasm.AppHost/Sandbox/solver#SOLVER_KIND` `EncodingKind` contract speaks are landed this way on `NxC`, closing the `Wire` table one-to-one over the kernel's six kinds, never a residency-side packer; a new feature channel is one kernel `EncodingChannel` row, read here through the descriptor set with zero residency edit; zero new surface.
- Boundary: geometry channel materialization remains in `Rasm.Drawing.Encode.Apply`; residency receives host-neutral `EncodedGeometry` and holds it whole. `EncodedTensor.Channel` returns the admitted zero-copy `ReadOnlyMemory<byte>` slice at the channel's STORED width, never a default ref-struct ghost and never a float re-typing of a dtype-strided arena, whose float16 and unorm8 lanes such a reinterpretation reads as garbage; descriptor tiling, extent, and offset are proved once by the kernel's own `EncodedGeometry.IsValid` claim set, so this wrap re-derives no range check and `ToTensor` widens through `ChannelDtype.Unpack` before one array allocation interleaves channel-blocked SoA into point-major `[Count, FeatureWidth]`; `Tensor/layout#LAYOUT_ALGEBRA` owns later rank edits. `Wire` maps model shape names to the remote geometry family, and free-dimension rows feed `AddFreeDimensionOverrideByName`. Mesh face indices ride optional `Tensor<long>` topology. `U`/`V` and `H`/`W` never derive by assigning the same flat `Count` to both axes. The `BrepPatch` `NurbsControlTensor` row carries a control net whose semantic authority is the kernel `Rasm/Parametric/nurbs#NURBS_ENGINE` `Nurbs.Of` admission — homogeneous SoA columns, strictly positive weights, normalized knots — so any quantization of that lane must re-admit through that gate, never a residency-local judgement.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Source is the WHOLE kernel carrier, never a destructured payload triple: the arena is dtype-STRIDED, so a
// payload re-typed to float reads a float16 curvature lane or a unorm8 colour lane as garbage, and the byte
// offsets a descriptor carries only address that arena. Every read here therefore composes the kernel's own
// dtype-dispatched `Channel`/`View<T>` readers, which gate width against the descriptor's own row.
public sealed record EncodedTensor(
    EncodedGeometry Source,
    LayoutForm Layout,
    string WireShape,
    Seq<(string Name, long Extent)> FreeDimensions,
    Option<Tensor<long>> Indices) {

    public Seq<EncodingChannelDescriptor> Descriptors => Source.Descriptors;

    public int Count => Source.Count;

    // `Derivable` is the truth column of what the geometry payload can ANSWER: `N`/`V` come from the element
    // count, `C` from the channel arity sum, `F` from supplied face topology — while the voxel `H`/`W` grid and
    // the NURBS `U`/`V` control net are extents no channel arena carries, so those rows are reachable through the
    // explicit-extents entry alone and say so at admission rather than at a later underivable-axis fault.
    private static readonly FrozenDictionary<PackKind, (LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames, bool Derivable)> Wire =
        new Dictionary<PackKind, (LayoutForm, string, Seq<string>, bool)> {
            [PackKind.PointCloud] = new(LayoutForm.NxC, "PointCloudTensor", Seq("N", "C"), true),
            [PackKind.MeshPatch] = new(LayoutForm.VertexFace, "MeshTensor", Seq("V", "F"), true),
            [PackKind.VoxelGrid] = new(LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W"), false),
            [PackKind.BrepPatch] = new(LayoutForm.NxC, "NurbsControlTensor", Seq("U", "V"), false),
            [PackKind.Field] = new(LayoutForm.NxC, "FieldTensor", Seq("N", "C"), true),
            [PackKind.Toolpath] = new(LayoutForm.NxC, "ToolpathTensor", Seq("N", "C"), true),
        }.ToFrozenDictionary();

    public int FeatureWidth => Descriptors.Sum(static descriptor => descriptor.Channel.Arity);

    // The derive-only entry refuses a non-derivable row BY NAME and points at the four-argument entry that takes
    // the extents explicitly; falling through would refuse anyway, one axis at a time, as if the caller had
    // supplied a bad grid rather than reached for an entry the row cannot serve.
    public static Fin<EncodedTensor> Of(EncodedGeometry geometry, PackKind kind) =>
        Wire.TryGetValue(kind, out var row) && !row.Derivable
            ? TensorFault.Fail<EncodedTensor>("free-dimension-explicit", kind.Key, string.Join(',', row.FreeDimensionNames))
            : Of(geometry, kind, None, None);

    public static Fin<EncodedTensor> Of(
        EncodedGeometry geometry,
        PackKind kind,
        Option<Seq<(string Name, long Extent)>> freeDimensions,
        Option<Tensor<long>> indices) =>
        !geometry.Witness.Lossless
            ? TensorFault.Fail<EncodedTensor>("encoding-lossy", kind.Key)
        : !Wire.TryGetValue(kind, out var row)
            ? TensorFault.Fail<EncodedTensor>("no-wire-row", kind.Key)
        : freeDimensions.Match(
            Some: dims => Fin.Succ(dims),
            None: () => Derived(row, geometry, indices)).Bind(dims =>
            dims.Exists(static d => d.Extent <= 0) || dims.Map(static d => d.Name) != row.FreeDimensionNames
                ? TensorFault.Fail<EncodedTensor>("free-dimension-miss", row.WireShape)
                : Fin.Succ(new EncodedTensor(geometry, row.Layout, row.WireShape, dims, indices)));

    // Raw stored bytes at the channel's own width — an empty span answers an inactive channel, exactly as the
    // kernel reader defines. A model lane wanting floats takes `ToTensor`, which widens through the dtype row.
    public Fin<ReadOnlyMemory<byte>> Channel(EncodingChannel channel) =>
        Source.Channel(channel) is { IsEmpty: false } stored
            ? Fin.Succ(stored)
            : TensorFault.Fail<ReadOnlyMemory<byte>>("channel-miss", channel.Key);

    // Model ingress is the ONE widening seam. Every channel admits through this row's OWN `Channel` gate before a
    // byte moves: an inactive channel answers an EMPTY span, `Unpack` then writes nothing, and the interleave copies
    // whatever the previous descriptor left in the shared staging lane under this channel's columns — one channel's
    // values silently transcribed onto another. The whole descriptor set therefore admits first, so a miss rails
    // channel-miss ahead of the staging rent rather than landing as plausible feature data.
    public Fin<Tensor<float>> ToTensor() =>
        Count <= 0 || FeatureWidth <= 0
            ? TensorFault.Fail<Tensor<float>>("encoding-shape", $"{Count}x{FeatureWidth}")
            : Descriptors.TraverseM(descriptor => Channel(descriptor.Channel)).As().Map(Interleaved);

    // `ChannelDtype.Unpack` is the kernel's own quantization inverse, so float16 and unorm8 lanes restore through
    // the row that packed them rather than a second conversion policy here. One scoped rent serves every channel
    // because the widest lane bounds them all, and the fold writes channel-blocked SoA into the point-major AoS the
    // wire shape declares, descriptor order carrying the column offset and the admitted-byte index together.
    private Tensor<float> Interleaved(Seq<ReadOnlyMemory<byte>> admitted) {
        int width = FeatureWidth;
        float[] data = new float[checked(Count * width)];
        Span<float> dst = data;
        using SpanOwner<float> staging = SpanOwner<float>.Allocate(checked(Count * Descriptors.Max(static d => d.Channel.Arity)));
        int column = 0;
        int index = 0;
        foreach (EncodingChannelDescriptor descriptor in Descriptors) {
            int arity = descriptor.Channel.Arity;
            Span<float> lane = staging.Span[..checked(Count * arity)];
            descriptor.Dtype.Unpack(admitted[index].Span, lane);
            for (int element = 0; element < Count; element++) {
                lane.Slice(element * arity, arity).CopyTo(dst.Slice((element * width) + column, arity));
            }
            column += arity;
            index++;
        }
        return Tensor.Create(data, [(nint)Count, (nint)width]);
    }

    public Fin<OrtValue> Admit() => ToTensor().Bind(static tensor => TensorBridge.Ingress(tensor));

    // Per-axis derivation: `C` from the channel arity sum, `V`/`N`/`U` from the element count, and `F` from the
    // supplied face-index topology — a `VertexFace` layout with no indices faults `free-dimension-underivable`
    // instead of silently equating the face count to the vertex count.
    private static Fin<Seq<(string Name, long Extent)>> Derived(
        (LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames, bool Derivable) row,
        EncodedGeometry geometry,
        Option<Tensor<long>> indices) =>
        row.FreeDimensionNames.Map<Fin<(string Name, long Extent)>>(name =>
            name == "C" ? Fin.Succ<(string Name, long Extent)>((name, geometry.Descriptors.Sum(static d => (long)d.Channel.Arity)))
            : name == "F" ? indices.Match(
                Some: topology => Fin.Succ<(string Name, long Extent)>((name, (long)topology.Lengths[0])),
                None: () => TensorFault.Fail<(string Name, long Extent)>("free-dimension-underivable", name))
            : name == "N" || (name == "V" && row.Layout == LayoutForm.VertexFace)
                ? Fin.Succ<(string Name, long Extent)>((name, geometry.Count))
                : TensorFault.Fail<(string Name, long Extent)>("free-dimension-underivable", name))
        .TraverseM(identity).As();
}
```

## [04]-[RESEARCH]

(none)
