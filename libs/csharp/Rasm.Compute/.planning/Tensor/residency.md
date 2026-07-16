# [COMPUTE_RESIDENCY]

ONNX C-data residency classifies every `OrtValue` by backing location and ownership through one `OrtResidency` lattice. `TensorBridge` owns carrier-shaped ingress and dtype-shaped egress, `DeviceMemory` owns shared allocation, `BoundFlow` owns steady-state `OrtIoBinding`, and `EncodedTensor` owns model-lane geometry wrapping without repacking the kernel payload.

## [01]-[INDEX]

- [01]-[ORT_BRIDGE]: `OrtResidency` lattice; carrier-keyed C-data ingress and dtype-keyed egress; `DeviceMemory` shared-allocator descriptor and residency probe; `BoundFlow` gate-aware `OrtIoBinding` steady-state.
- [02]-[GEOMETRY_ENCODING]: the kernel `EncodedGeometry` wrap; `EncodedTensor` carrier; per-channel slice view; `PackKind` → wire-shape/layout/free-dimension model vocabulary; host-neutral, never a re-pack.

## [02]-[ORT_BRIDGE]

- Owner: `OrtResidency` `[SmartEnum<string>]` the five-gate residency lattice; `TensorBridge` the static `OrtValue` C-data factory surface (carrier-keyed ingress, dtype-keyed egress, the device descriptor, the residency probe); `BoundFlow` the ONE `OrtIoBinding` steady-state residency capsule the `Model/inference#INFERENCE_MODES` run-mode fold composes.
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source)` and its `MemoryOwner<T>`, foreign-pointer, and `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` overloads discriminate ingress by carrier shape; `public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape)` mints a device sink; `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` and its flat `Span<T>` overload project an output by the dtype row; `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena)` leases the steady-state capsule (the bound input and sink allocate from the supplied shared arena — the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` for the model lane — never a managed staging plane), with the `TensorDtype`-row overload binding any dtype the vocabulary admits so the capsule is dtype-polymorphic and the row-less form is the float32 convenience — `Fin<T>` aborts when the egress destination is undersized against the `GetTensorSizeInBytes` count, ingress shape volume fails to cover its payload (`ingress-cover-gap`), or a native mint rejects (`ingress-rejected` — every C-data factory call crosses `Try.lift` once); the leased flow is a disposable capsule whose `Dispose` is the bound backing's release point, and `Lease` releases every already-acquired native handle on its own failure path so a `lease-rejected` fault strands nothing.
- Receipt: `CopyPoint` stamps the `OrtResidency` gate, native byte count, device name, instant, and `CorrelationId`; `CopyPoint.Receipt` projects that evidence onto `ComputeReceipt.Copy`, and `ReceiptFolds.Crossings` aggregates it by gate.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new accelerator is one `DeviceMemory` descriptor over its `OrtEpDevice` plus the existing `Allocate`/device-pointer ingress, never a per-call marshal helper; a new carrier is one `TensorBridge.Ingress` overload discriminating by carrier shape (the `Model/inference#INFERENCE_MODES` `RunInput` cases compose these overloads, never re-spelling a factory); the `DeviceResident` row is the one residency gate the `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row and the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` both bind — a WGPU compute buffer and an ORT device value share this one residency row so device-ness is a residency discriminant, never a second tensor owner or a parallel device-residency lattice; the resolved shared `ONE_WGPU_DEVICE` adapter is what a composition root folds into the `device-wgpu` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` (present iff the adapter resolves), so the same device-presence fact the `DeviceResident` gate observes contributes the substrate key the `Substrate.DeviceWgpu` `!Providers.Contains(Key)` gate reads, never a raw `Device`/adapter handle pushed into `Providers`; zero new surface.
- Boundary: `OrtValue` is the sole model-boundary carrier. Every ingress shape proves non-negative extents, checked volume, payload coverage, and native construction on `Fin`; zero-sized tensors remain representable. Every egress proves dtype identity, native byte count, and destination density where raw-byte projection requires it. `BoundFlow.Write<T>` and framed-byte `Write` return `Fin<Unit>`, enforce exact dtype and length, and let `Flow` abort before `Drive`. Rebind operations allocate replacements before clearing current bindings, restore prior CPU bindings on failure, and transfer ownership only after successful binding. `Dispose` releases each owned native handle once.

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

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorBridge {
    public static Fin<OrtValue> Ingress<T>(Tensor<T> source) where T : unmanaged =>
        Minted(() => OrtValue.CreateTensorValueFromSystemNumericsTensorObject(source));

    public static Fin<OrtValue> Ingress<T>(T[] data, ReadOnlySpan<long> shape) where T : unmanaged =>
        Covered(shape, data.Length).Bind(admitted => Minted(() => OrtValue.CreateTensorValueFromMemory(data, admitted)));

    public static Fin<OrtValue> Ingress<T>(MemoryOwner<T> backing, ReadOnlySpan<long> shape) where T : unmanaged =>
        Covered(shape, backing.Length).Bind(admitted => Minted(() => OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, backing.Memory, admitted)));

    public static Fin<OrtValue> Ingress(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, nint data, long sizeInBytes) =>
        CoveredBytes(shape, row, sizeInBytes).Bind(admitted =>
            Minted(() => OrtValue.CreateTensorValueWithData(memory, row.Element, admitted, data, sizeInBytes)));

    public static Fin<OrtValue> Ingress(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> tokens) =>
        Minted(() => OrtValue.CreateFromStringTensor(tokens));

    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        Shape(shape).Bind(admitted => device.Allocate(row, admitted.Shape));

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

    // A ref-struct destination cannot cross a lambda, so the projection body is the named REF_SAFE statement
    // seam: admission stays on the rail, the copy runs in place, and a native rejection converts once.
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

    public static (Seq<string> Inputs, Seq<string> Outputs) Residency(InferenceSession session) {
        using IDisposableReadOnlyCollection<OrtMemoryInfo> inputs = session.GetMemoryInfosForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> outputs = session.GetMemoryInfosForOutputs();
        return (toSeq(inputs).Map(static info => info.Name), toSeq(outputs).Map(static info => info.Name));
    }

    public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena) =>
        BoundFlow.Lease(session, inputName, outputName, shape.ToArray(), arena, TensorDtype.Float32);

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

    public IDisposableReadOnlyCollection<OrtValue> Run(RunOptions options) {
        Drive(options);
        return binding.GetOutputValues();
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

    public IDisposableReadOnlyCollection<OrtValue> Outputs() => binding.GetOutputValues();

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

- Owner: `EncodedTensor` — the model-lane wrap of the kernel `Rasm.Drawing.EncodedGeometry`; the per-`PackKind` `Wire` mapping is the canonical geometry-ML input vocabulary.
- Entry: `Of(EncodedGeometry, PackKind)` derives only provable dimensions (`N`, point/mesh `V`, channel `C`, and indexed-face `F`), while `Of(EncodedGeometry, PackKind, Option<Seq<(string Name, long Extent)>>, Option<Tensor<long>>)` carries explicit spatial dimensions without default ghosts. `Fin<T>` rejects lossy witnesses, absent wire rows, non-positive or mismatched dimensions, underivable `U`/`V` and `H`/`W` grids, invalid channel ranges, and overflowed interleaving shapes.
- Receipt: the kernel `EncodedGeometry.Witness` is the lossless-round-trip proof keyed by the `Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` content hash; `Of` admits only a lossless payload, so the residency wrap carries no second witness and mints no second content key.
- Packages: Rasm (project), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new representation lands as one kernel `PackKind` row (the kernel `Rasm.Drawing` owner adds it with its active-channel column) plus one `Wire` row here carrying its `LayoutForm`/`WireShape`/free-dimension names — the `Field` (`geodesic`+`weight` lanes, positions omitted because the witness digest binds the source mesh) and `Toolpath` (`position`+`weight`, stored order is content) rows the `AppHost/Sandbox/solver#SOLVER_KIND` `EncodingKind` contract speaks are landed this way on `NxC`, closing the `Wire` table one-to-one over the kernel's six kinds, never a residency-side packer; a new feature channel is one kernel `EncodingChannel` row, read here through the descriptor set with zero residency edit; zero new surface.
- Boundary: geometry channel materialization remains in `Rasm.Drawing.Encode.Apply`; residency receives host-neutral `EncodedGeometry`. `EncodedTensor.Channel` returns an admitted zero-copy `ReadOnlyMemory<float>` slice, never a default ref-struct ghost. `ToTensor` validates each descriptor's `Offset`, `Floats`, `Count × Arity`, and aggregate shape before one array allocation interleaves channel-blocked SoA into point-major `[Count, FeatureWidth]`; `Tensor/layout#LAYOUT_ALGEBRA` owns later rank edits. `Wire` maps model shape names to the remote geometry family, and free-dimension rows feed `AddFreeDimensionOverrideByName`. Mesh face indices ride optional `Tensor<long>` topology. `U`/`V` and `H`/`W` never derive by assigning the same flat `Count` to both axes.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EncodedTensor(
    ReadOnlyMemory<float> Payload,
    Seq<EncodingChannelDescriptor> Descriptors,
    int Count,
    LayoutForm Layout,
    string WireShape,
    Seq<(string Name, long Extent)> FreeDimensions,
    Option<Tensor<long>> Indices) {

    private static readonly FrozenDictionary<PackKind, (LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames)> Wire =
        new Dictionary<PackKind, (LayoutForm, string, Seq<string>)> {
            [PackKind.PointCloud] = new(LayoutForm.NxC, "PointCloudTensor", Seq("N", "C")),
            [PackKind.MeshPatch] = new(LayoutForm.VertexFace, "MeshTensor", Seq("V", "F")),
            [PackKind.VoxelGrid] = new(LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W")),
            [PackKind.BrepPatch] = new(LayoutForm.NxC, "NurbsControlTensor", Seq("U", "V")),
            [PackKind.Field] = new(LayoutForm.NxC, "FieldTensor", Seq("N", "C")),
            [PackKind.Toolpath] = new(LayoutForm.NxC, "ToolpathTensor", Seq("N", "C")),
        }.ToFrozenDictionary();

    public int FeatureWidth => Descriptors.Sum(static descriptor => descriptor.Channel.Arity);

    public static Fin<EncodedTensor> Of(EncodedGeometry geometry, PackKind kind) => Of(geometry, kind, None, None);

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
                : Fin.Succ(new EncodedTensor(geometry.Payload, geometry.Descriptors, geometry.Count, row.Layout, row.WireShape, dims, indices)));

    public Fin<ReadOnlyMemory<float>> Channel(EncodingChannel channel) =>
        Descriptors.Find(descriptor => descriptor.Channel == channel).Match(
            Some: descriptor => descriptor.Offset < 0 || descriptor.Floats < 0 || descriptor.Offset > Payload.Length - descriptor.Floats
                ? TensorFault.Fail<ReadOnlyMemory<float>>("channel-range", channel.Key)
                : Fin.Succ(Payload.Slice(descriptor.Offset, descriptor.Floats)),
            None: () => TensorFault.Fail<ReadOnlyMemory<float>>("channel-miss", channel.Key));

    public Fin<Tensor<float>> ToTensor() {
        try {
            int width = FeatureWidth;
            if (Count <= 0 || width <= 0) { return TensorFault.Fail<Tensor<float>>("encoding-shape", $"{Count}x{width}"); }
            float[] data = new float[checked(Count * width)];
            Span<float> dst = data;
            ReadOnlySpan<float> src = Payload.Span;
            int column = 0;
            foreach (EncodingChannelDescriptor descriptor in Descriptors) {
                int arity = descriptor.Channel.Arity;
                int expected = checked(Count * arity);
                if (descriptor.Offset < 0 || descriptor.Floats != expected || descriptor.Offset > src.Length - descriptor.Floats) {
                    return TensorFault.Fail<Tensor<float>>("encoding-descriptor", descriptor.Channel.Key);
                }
                ReadOnlySpan<float> block = src.Slice(descriptor.Offset, descriptor.Floats);
                for (int element = 0; element < Count; element++) {
                    block.Slice(element * arity, arity).CopyTo(dst.Slice((element * width) + column, arity));
                }
                column += arity;
            }
            return Fin.Succ(Tensor.Create(data, [(nint)Count, (nint)width]));
        }
        catch (Exception ex) { return TensorFault.Fail<Tensor<float>>("encoding-projection", ex.Message); }
    }

    public Fin<OrtValue> Admit() => ToTensor().Bind(static tensor => TensorBridge.Ingress(tensor));

    // Per-axis derivation: `C` from the channel arity sum, `V`/`N`/`U` from the element count, and `F` from the
    // supplied face-index topology — a `VertexFace` layout with no indices faults `free-dimension-underivable`
    // instead of silently equating the face count to the vertex count.
    private static Fin<Seq<(string Name, long Extent)>> Derived(
        (LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames) row,
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

- [DEVICE_STEADY_STATE]: `BoundFlow.RebindDevice`, `RebindExternal`, and `RebindDevicePointer` form the `DeviceDispatch` host-free chaining surface; `TensorBridge.Residency` remains the open automatic gate-selection leaf.
