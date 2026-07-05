# [COMPUTE_RESIDENCY]

The ONNX C-data residency bridge: one `OrtResidency` lattice over the five copy gates (`ManagedSpan` → `MemoryBacked` → `DeviceResident` → `OutputValue` → `SpanView`) classifying every `OrtValue` by where its bytes live and who owns the backing, the `TensorBridge` carrier-keyed ingress and dtype-keyed egress as the sole `OrtValue` C-data factory owner (the `Model/inference#INFERENCE_MODES` `RunInput` admission composes it, never re-spelling a factory), the `DeviceMemory` shared-allocator descriptor over `OrtEpDevice`, and the `BoundFlow` arena-allocated `OrtIoBinding` steady-state residency capsule — the ONE such capsule, composed by the `Model/inference#INFERENCE_MODES` run-mode fold — with its device and external-allocation rebind paths. The page owns the `OrtResidency` residency lattice, the `CopyPoint` copy-gate witness, the `DeviceMemory` device descriptor, the `TensorBridge` C-data factory surface, the `BoundFlow` steady-state composition, and the `EncodedTensor` carrier; the `OrtValue` carrier rides `Microsoft.ML.OnnxRuntime` and the `TensorDtype`/`TensorFault`/`ComparerAccessors.StringOrdinal` and the `TensorVocabulary.OrtByteSpan` egress-size law arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY`, the `LayoutForm` rows from `Tensor/layout#LAYOUT_ALGEBRA`, and the `Effects.ToFin` fold the `Egress` projection lifts through from `Tensor/dispatch#KERNEL_DISPATCH`. Geometry channel materialization is NOT this page's concern — the kernel `Rasm.Drawing` owner produces the `EncodedGeometry` (`Descriptors` + contiguous `ReadOnlyMemory<float>` `Payload` + lossless `RoundTripWitness`) and the `Rasm.AppHost` `GeometryPacking` sandbox capsule marshals host geometry across the plugin boundary; `EncodedTensor` wraps that host-neutral float payload as the model-lane tensor with its wire-shape and free-dimension vocabulary, never a re-pack. The `OrtResidency`/`TensorBridge` lattice serves the `Model/inference#INFERENCE_MODES` `OrtValue`-only run law, the `DeviceResident` gate is the residency row the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` binds, and the `EncodedTensor` free-dimension rows feed the model-lane `AddFreeDimensionOverrideByName` admission.

## [01]-[INDEX]

- [01]-[ORT_BRIDGE]: `OrtResidency` lattice; carrier-keyed C-data ingress and dtype-keyed egress; `DeviceMemory` shared-allocator descriptor and residency probe; `BoundFlow` gate-aware `OrtIoBinding` steady-state.
- [02]-[GEOMETRY_ENCODING]: the kernel `EncodedGeometry` wrap; `EncodedTensor` carrier; per-channel slice view; `PackKind` → wire-shape/layout/free-dimension model vocabulary; host-neutral, never a re-pack.

## [02]-[ORT_BRIDGE]

- Owner: `OrtResidency` `[SmartEnum<string>]` the five-gate residency lattice; `TensorBridge` the static `OrtValue` C-data factory surface (carrier-keyed ingress, dtype-keyed egress, the device descriptor, the residency probe); `BoundFlow` the ONE `OrtIoBinding` steady-state residency capsule the `Model/inference#INFERENCE_MODES` run-mode fold composes.
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source)` and its `MemoryOwner<T>`, foreign-pointer, and `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` overloads discriminate ingress by carrier shape; `public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape)` mints a device sink; `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` and its flat `Span<T>` overload project an output by the dtype row; `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena)` leases the steady-state capsule (the bound input and sink allocate from the supplied shared arena — the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` for the model lane — never a managed staging plane) — `Fin<T>` aborts when the egress destination is undersized against the `GetTensorSizeInBytes` count, and the leased flow is a disposable capsule whose `Dispose` is the bound backing's release point.
- Receipt: `CopyPoint` is the copy-gate witness `TensorBridge.Stamp` mints at a crossing — the `OrtResidency` gate, the byte count from `GetTensorSizeInBytes`, the residency-device name from `GetTensorMemoryInfo().Name`, the instant, and the `CorrelationId` — and `CopyPoint.Receipt(lane, elapsed)` projects it onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Copy` case (`Gate`/`Bytes`/`Device`) the receipts owner registers, so the gate the bytes-and-device-only `ModelRun` could not attribute now rides the one union under the crossing's correlation and the `ReceiptFolds.Crossings` fold tallies crossings by gate; the witness mints no standalone receipt type and keys by `CorrelationId`, never payload bytes.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new accelerator is one `DeviceMemory` descriptor over its `OrtEpDevice` plus the existing `Allocate`/device-pointer ingress, never a per-call marshal helper; a new carrier is one `TensorBridge.Ingress` overload discriminating by carrier shape (the `Model/inference#INFERENCE_MODES` `RunInput` cases compose these overloads, never re-spelling a factory); the `DeviceResident` row is the one residency gate the `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row and the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` both bind — a WGPU compute buffer and an ORT device value share this one residency row so device-ness is a residency discriminant, never a second tensor owner or a parallel device-residency lattice; the resolved shared `ONE_WGPU_DEVICE` adapter is what a composition root folds into the `device-wgpu` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` (present iff the adapter resolves), so the same device-presence fact the `DeviceResident` gate observes contributes the substrate key the `Substrate.DeviceWgpu` `!Providers.Contains(Key)` gate reads, never a raw `Device`/adapter handle pushed into `Providers`; zero new surface.
- Boundary: `OrtValue` is the sole model-boundary carrier and `TensorBridge` owns the C-data residency lattice the `Model/inference#INFERENCE_MODES` `OrtValue`-only run law consumes — `NamedOnnxValue`, `DisposableNamedOnnxValue.CreateFromOrtValue`, and `FixedBufferOnnxValue` are the superseded spellings the `OrtValue`-only law deletes, and per-call dictionary marshal and managed dense-tensor copies are the deleted rows. The `OrtResidency` lattice is a classification, not a dispatch knob: each gate is uniquely keyed by the `(Wraps, Device, ProjectsInPlace, Foreign)` column tuple — `Wraps` borrows caller memory with no ingress copy, `Device` is accelerator-resident, `ProjectsInPlace` writes the value's buffer in place on egress or steady-state refresh, and `Foreign` is a ref-rooted span whose lifetime is the caller's proof obligation distinct from a managed `Tensor<T>`/`MemoryOwner<T>` owner; the `Foreign` column is the discriminant that separates `MemoryBacked` (`true, false, true, false`) from the otherwise-identical `SpanView` (`true, false, true, true`), so no two gates are behavioural twins and the gate value is never the ingress dispatch input — ingress discriminates on the carrier value itself. The ingress carriers ride distinct overloads the `Model/inference#INFERENCE_MODES` `RunInput` `[Union]` cases (`Managed<T>`/`Carrier<T>`/`Strings`) compose, never re-spelling a factory: a managed `System.Numerics` `Tensor<T>` admits directly through `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)` (the `ManagedSpan` gate); a managed `T[]` admits through `CreateTensorValueFromMemory<T>(T[], long[])` and a rented `MemoryOwner<T>` pins through `CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])` (both the `MemoryBacked` gate, refreshable in place for steady-state); a ref-rooted foreign pointer over a `TensorMarshal.GetReference`/`Tensor<T>.GetPinnableReference` root admits through `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` (the `SpanView` and `DeviceResident` gates, the pooled-plane, model-output, and device-buffer path); and the `DeviceResident` gate also allocates an ORT-owned device sink through `CreateAllocatedTensorValue(OrtAllocator, TensorElementType, long[])` against an `OrtEnv.Instance().CreateSharedAllocator(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, allocatorOptions)` allocation over the `OrtEpDevice.GetMemoryInfo(OrtDeviceMemoryType)` descriptor — `OrtMemoryInfo.DefaultInstance` is the CPU residency descriptor and the extended `OrtMemoryInfo(string, OrtMemoryInfoDeviceType, uint, int, OrtDeviceMemoryType, ulong, OrtAllocatorType)` constructor builds a manually-aligned device descriptor where `GetMemoryInfo` does not suffice. The backing must outlive the value and every run binding it, the value's dispose IS the owner's release point, and freeing under a live value is a use-after-free in managed code that no allocation profiler attributes. Egress is projection, never copy, and the dtype-width column selects the polarity: an IEEE-754 row projects through `GetTensorDataAsTensorSpan<T>()` (`ReadOnlyTensorSpan<T>`) into the strided `in TensorSpan<T>` destination under the same-start aliasing law, while a quantized/sub-stride row (int8/uint8/bool) projects through the flat `GetTensorDataAsSpan<T>()` and the raw-byte `GetTensorMutableRawData()` (`Span<byte>`) no `TensorSpan<T>` widening serves; the egress destination sizes from `GetTensorSizeInBytes` through `TensorVocabulary.OrtByteSpan`, never re-multiplied dimensions, and the `IDisposableReadOnlyCollection<OrtValue>` result set is deterministic-dispose native material invisible to GC heap heuristics — one dispose releases every element, a leaked collection a native leak. `TensorBridge.Residency` reads `InferenceSession.GetMemoryInfosForInputs`/`GetMemoryInfosForOutputs` to report where each I/O buffer must live, the device-residency binding decision the `BoundFlow` device rebind consumes rather than assumes. The `BoundFlow` capsule is the ONE steady-state row for repeated same-shape tensor flow the model and tensor lanes share — `Lease` arena-allocates the bound input and sink through `CreateAllocatedTensorValue(OrtAllocator, TensorElementType, long[])` from the supplied shared arena, `BindInput`/`BindOutput` once, the per-`Flow` `Write` straight into the bound value through `GetTensorMutableDataAsSpan<float>()` (no managed staging copy beside the bound backing) or the framed `ReadOnlySequence<byte>` `Write` through `GetTensorMutableRawData()`, `RunWithBinding` per `Drive` with zero marshal, and `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` bracketing every run unconditionally (no-ops on CPU, stream fences on device); a shape-class transition rebinds through `ClearBoundInputs`/`ClearBoundOutputs` — `Rebind` resizes from the arena, `RebindDevice` binds a shared-arena device input through `BindInput(string, TensorElementType, long[], OrtMemoryAllocation)` and routes the output through `BindOutputToDevice(string, OrtMemoryInfo)` with no host round trip, `RebindDevicePointer` binds a raw device pointer through `CreateTensorValueWithData` to the same device-output route, and `RebindExternal` binds a caller-owned device buffer through `BindInput`/`BindOutput(string, OrtExternalAllocation)` — the steady-state multi-output read rides `GetOutputValues()`, and the allocate-per-call spelling turns tensor flow into a GC workload. The capsule stays cancellation-agnostic (its `Write`/`Run`/`Flow` primitives); the `Model/inference#INFERENCE_MODES` `RunOps` run-mode fold composes it, wrapping the deadline-classifying `CancelScope` `Pulse` (run + project) and the `Chunked` windowed streaming over the one capsule, so no second `OrtIoBinding` steady-state type exists. The string-tensor copy point rides `CreateFromStringTensor` over the ONNX `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` carrier — a distinct type from the `System.Numerics` `Tensor<T>` the generic ingress rides, so the two `Tensor<...>` spellings never unify in the one bridge — the one legal string copy.

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
        new(Gate, Bytes, Device) { Correlation = Correlation, Lane = lane, Substrate = Substrate.Onnx, AllocationClass = AllocationClass.NativeOrt, Elapsed = elapsed };
}

public readonly record struct DeviceMemory(OrtEpDevice Device, OrtDeviceMemoryType MemoryType, OrtAllocatorType AllocatorType) {
    public OrtMemoryInfo Info => Device.GetMemoryInfo(MemoryType);

    public Fin<OrtAllocator> Shared() =>
        Fin.Succ(OrtEnv.Instance().CreateSharedAllocator(Device, MemoryType, AllocatorType, allocatorOptions: null));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorBridge {
    public static Fin<OrtValue> Ingress<T>(Tensor<T> source) where T : unmanaged =>
        Fin.Succ(OrtValue.CreateTensorValueFromSystemNumericsTensorObject(source));

    public static Fin<OrtValue> Ingress<T>(T[] data, ReadOnlySpan<long> shape) where T : unmanaged =>
        Fin.Succ(OrtValue.CreateTensorValueFromMemory(data, shape.ToArray()));

    public static Fin<OrtValue> Ingress<T>(MemoryOwner<T> backing, ReadOnlySpan<long> shape) where T : unmanaged =>
        Fin.Succ(OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, backing.Memory, shape.ToArray()));

    public static Fin<OrtValue> Ingress(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, nint data, long sizeInBytes) =>
        Fin.Succ(OrtValue.CreateTensorValueWithData(memory, row.Element, shape.ToArray(), data, sizeInBytes));

    public static Fin<OrtValue> Ingress(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> tokens) =>
        Fin.Succ(OrtValue.CreateFromStringTensor(tokens));

    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        device.Shared().Map(allocator => (allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape.ToArray())));

    public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count != destination.FlattenedLength
                    ? TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.FlattenedLength}")
                : row.Quantized
                    ? Effects.ToFin(() => value.GetTensorDataAsSpan<T>().CopyTo(MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)destination.FlattenedLength))))
                    : Effects.ToFin(() => value.GetTensorDataAsTensorSpan<T>().CopyTo(destination))));

    public static Fin<Unit> Egress<T>(OrtValue value, Span<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count == destination.Length
                    ? Effects.ToFin(() => value.GetTensorDataAsSpan<T>().CopyTo(destination))
                    : TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.Length}")));

    public static CopyPoint Stamp(OrtValue value, OrtResidency gate, ClockPolicy clocks, CorrelationId correlation) =>
        new(gate, value.GetTensorSizeInBytes(), value.GetTensorMemoryInfo().Name, clocks.Now, correlation);

    public static (Seq<string> Inputs, Seq<string> Outputs) Residency(InferenceSession session) {
        using IDisposableReadOnlyCollection<OrtMemoryInfo> inputs = session.GetMemoryInfosForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> outputs = session.GetMemoryInfosForOutputs();
        return (toSeq(inputs).Map(static info => info.Name), toSeq(outputs).Map(static info => info.Name));
    }

    public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena) =>
        BoundFlow.Lease(session, inputName, outputName, shape.ToArray(), arena);
}

// --- [COMPOSITION] -------------------------------------------------------------------------
public sealed class BoundFlow : IDisposable {
    private readonly InferenceSession session;
    private readonly OrtIoBinding binding;
    private readonly RunOptions run;
    private readonly OrtAllocator arena;
    private readonly string inputName, outputName;
    private OrtValue bound, sink;

    private BoundFlow(InferenceSession session, OrtIoBinding binding, RunOptions run, OrtAllocator arena, string inputName, string outputName, OrtValue bound, OrtValue sink) =>
        (this.session, this.binding, this.run, this.arena, this.inputName, this.outputName, this.bound, this.sink) = (session, binding, run, arena, inputName, outputName, bound, sink);

    public static Fin<BoundFlow> Lease(InferenceSession session, string inputName, string outputName, long[] shape, OrtAllocator arena) {
        OrtValue bound = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
        OrtValue sink = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
        RunOptions run = new();
        OrtIoBinding binding = session.CreateIoBinding();
        binding.BindInput(inputName, bound);
        binding.BindOutput(outputName, sink);
        return Fin.Succ(new BoundFlow(session, binding, run, arena, inputName, outputName, bound, sink));
    }

    public void Write(ReadOnlySpan<float> payload) => payload.CopyTo(bound.GetTensorMutableDataAsSpan<float>());

    public void Write(ReadOnlySequence<byte> window) => window.CopyTo(bound.GetTensorMutableRawData());

    private void Drive(RunOptions options) {
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(options, binding);
        binding.SynchronizeBoundOutputs();
    }

    public IDisposableReadOnlyCollection<OrtValue> Run(RunOptions options) {
        Drive(options);
        return binding.GetOutputValues();
    }

    public Fin<Unit> Flow(ReadOnlySpan<float> input, in TensorSpan<float> output) {
        Write(input);
        Drive(run);
        return TensorBridge.Egress(sink, output);
    }

    public Fin<Unit> Rebind(long[] shape) {
        binding.ClearBoundInputs();
        binding.ClearBoundOutputs();
        bound.Dispose(); sink.Dispose();
        bound = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
        sink = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
        binding.BindInput(inputName, bound);
        binding.BindOutput(outputName, sink);
        return Fin.Succ(unit);
    }

    public Fin<Unit> RebindDevice(TensorElementType dtype, long[] shape, OrtMemoryAllocation deviceInput, OrtMemoryInfo deviceOutput) {
        binding.ClearBoundInputs();
        binding.ClearBoundOutputs();
        binding.BindInput(inputName, dtype, shape, deviceInput);
        binding.BindOutputToDevice(outputName, deviceOutput);
        return Fin.Succ(unit);
    }

    public Fin<Unit> RebindDevicePointer(TensorElementType dtype, long[] shape, OrtMemoryInfo deviceInfo, nint pointer, long sizeInBytes) {
        binding.ClearBoundInputs();
        binding.ClearBoundOutputs();
        binding.BindInput(inputName, OrtValue.CreateTensorValueWithData(deviceInfo, dtype, shape, pointer, sizeInBytes));
        binding.BindOutputToDevice(outputName, deviceInfo);
        return Fin.Succ(unit);
    }

    public Fin<Unit> RebindExternal(OrtExternalAllocation input, OrtExternalAllocation output) {
        binding.ClearBoundInputs();
        binding.ClearBoundOutputs();
        binding.BindInput(inputName, input);
        binding.BindOutput(outputName, output);
        return Fin.Succ(unit);
    }

    public IDisposableReadOnlyCollection<OrtValue> Outputs() => binding.GetOutputValues();

    public void Dispose() { run.Dispose(); binding.Dispose(); bound.Dispose(); sink.Dispose(); }
}
```

## [03]-[GEOMETRY_ENCODING]

- Owner: `EncodedTensor` — the model-lane wrap of the kernel `Rasm.Drawing.EncodedGeometry`; the per-`PackKind` `Wire` mapping is the canonical geometry-ML input vocabulary.
- Entry: `public static Fin<EncodedTensor> Of(EncodedGeometry geometry, PackKind kind, Seq<(string Name, long Extent)> freeDimensions = default, Option<Tensor<long>> indices = default)` — `Fin<T>` aborts when the kernel `RoundTripWitness` is not lossless, the `PackKind` has no `Wire` row, or the supplied free-dimension names miss the row's names; with no free dimensions supplied the row names derive their extents from the encoded `Count` and channel arity, so the common `NxC` case needs no caller bookkeeping while a spatial layout overrides per axis.
- Receipt: the kernel `EncodedGeometry.Witness` is the lossless-round-trip proof keyed by the `Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` content hash; `Of` admits only a lossless payload, so the residency wrap carries no second witness and mints no second content key.
- Packages: Rasm (project), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new representation lands as one kernel `PackKind` row (the kernel `Rasm.Drawing` owner adds it with its active-channel column) plus one `Wire` row here carrying its `LayoutForm`/`WireShape`/free-dimension names — the `Field` and `Toolpath` representations the `AppHost/Sandbox/solver#SOLVER_KIND` `EncodingKind` contract speaks arrive this way, never a residency-side packer; a new feature channel is one kernel `EncodingChannel` row, read here through the descriptor set with zero residency edit; zero new surface.
- Boundary: geometry channel materialization is the kernel `Rasm.Drawing` owner's concern and a residency-side `GeometryEncoding`/`GeometryPacking` re-packer over host `MeshSpace`/`VectorCloud` coordinates is the deleted strata violation — host geometry folds inside the kernel `Encode.Apply` (and the `Rasm.AppHost` `GeometryPacking` sandbox capsule at the plugin boundary), so this page receives only the host-neutral `EncodedGeometry` float payload and never names a host geometry type. `EncodedTensor` slices the contiguous channel-blocked `EncodedGeometry.Payload` per `EncodingChannelDescriptor` as a zero-copy `ReadOnlyTensorSpan<float>` view through `TensorMarshal.CreateReadOnlyTensorSpan` and re-projects per use — a cached span is the foreclosed defect; `ToTensor` is the one residency assembly that interleaves the channel-blocked SoA into a point-major `[Count, FeatureWidth]` `Tensor<float>` the model lane consumes — its element-major fill loop is the named statement-form seam beside the page's expression surface (the `Tensor/layout#LAYOUT_ALGEBRA` `TensorLayout` owner permutes and reshapes onto `Nchw` or any rank target, never a residency-side rank edit), and the spread is residency, never the re-materialization of a channel from geometry the kernel forbids; the `Wire` table keys the model wire-shape names one-to-one onto the remote-lane proto geometry family and the free-dimension rows feed the `Model/inference#INFERENCE_MODES` `AddFreeDimensionOverrideByName` admission; mesh face indices ride the optional caller-supplied `Tensor<long>` on the int64 row (the topology the channel encoding omits), host-neutral at the residency seam; the `EncodingChannel`/`EncodingChannelDescriptor`/`PackKind`/`EncodedGeometry` vocabulary arrives settled from the kernel and is composed, never re-declared.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct WireRow(LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames);

public sealed record EncodedTensor(
    ReadOnlyMemory<float> Payload,
    Seq<EncodingChannelDescriptor> Descriptors,
    int Count,
    LayoutForm Layout,
    string WireShape,
    Seq<(string Name, long Extent)> FreeDimensions,
    Option<Tensor<long>> Indices) {

    private static readonly FrozenDictionary<PackKind, WireRow> Wire =
        new Dictionary<PackKind, WireRow> {
            [PackKind.PointCloud] = new(LayoutForm.NxC, "PointCloudTensor", Seq("N", "C")),
            [PackKind.MeshPatch] = new(LayoutForm.VertexFace, "MeshTensor", Seq("V", "F")),
            [PackKind.VoxelGrid] = new(LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W")),
            [PackKind.BrepPatch] = new(LayoutForm.NxC, "NurbsControlTensor", Seq("U", "V")),
        }.ToFrozenDictionary();

    public int FeatureWidth => Descriptors.Sum(static descriptor => descriptor.Channel.Arity);

    public static Fin<EncodedTensor> Of(EncodedGeometry geometry, PackKind kind, Seq<(string Name, long Extent)> freeDimensions = default, Option<Tensor<long>> indices = default) =>
        !geometry.Witness.Lossless
            ? TensorFault.Fail<EncodedTensor>("encoding-lossy", kind.Key)
        : !Wire.TryGetValue(kind, out WireRow row)
            ? TensorFault.Fail<EncodedTensor>("no-wire-row", kind.Key)
        : (freeDimensions.IsEmpty ? Derived(row, geometry) : freeDimensions) is var dims && dims.Map(static d => d.Name) != row.FreeDimensionNames
            ? TensorFault.Fail<EncodedTensor>("free-dimension-miss", row.WireShape)
            : Fin.Succ(new EncodedTensor(geometry.Payload, geometry.Descriptors, geometry.Count, row.Layout, row.WireShape, dims, indices));

    public ReadOnlyTensorSpan<float> Channel(EncodingChannel channel) =>
        Descriptors.Find(descriptor => descriptor.Channel == channel).Match(
            Some: descriptor => TensorMarshal.CreateReadOnlyTensorSpan(
                ref MemoryMarshal.GetReference(Payload.Slice(descriptor.Offset, descriptor.Floats).Span),
                descriptor.Floats, [(nint)Count, (nint)channel.Arity], [], pinned: false),
            None: static () => default);

    public Fin<Tensor<float>> ToTensor() {
        int width = FeatureWidth;
        using MemoryOwner<float> owner = MemoryOwner<float>.Allocate(Count * width, AllocationMode.Clear);
        Span<float> dst = owner.Span;
        ReadOnlySpan<float> src = Payload.Span;
        int column = 0;
        foreach (EncodingChannelDescriptor descriptor in Descriptors) {
            int arity = descriptor.Channel.Arity;
            ReadOnlySpan<float> block = src.Slice(descriptor.Offset, descriptor.Floats);
            for (int element = 0; element < Count; element++) {
                block.Slice(element * arity, arity).CopyTo(dst.Slice(element * width + column, arity));
            }
            column += arity;
        }
        return Fin.Succ(Tensor.Create(dst.ToArray(), [(nint)Count, (nint)width]));
    }

    public Fin<OrtValue> Admit() => ToTensor().Bind(static tensor => TensorBridge.Ingress(tensor));

    private static Seq<(string Name, long Extent)> Derived(WireRow row, EncodedGeometry geometry) =>
        row.FreeDimensionNames.Map(name => (Name: name, Extent: name == "C" ? (long)geometry.Descriptors.Sum(static d => d.Channel.Arity) : (long)geometry.Count));
}
```

## [04]-[RESEARCH]

- [DEVICE_STEADY_STATE]: the `BoundFlow` device residency path (`RebindDevice` over `OrtMemoryAllocation`, `RebindExternal` over `OrtExternalAllocation`, `RebindDevicePointer` over a raw device pointer) is the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` host-free chaining surface — the open leaf is the `TensorBridge.Residency` `GetMemoryInfosForInputs`/`GetMemoryInfosForOutputs` probe driving the gate choice automatically against the shared `ONE_WGPU_DEVICE` adapter so a device-eligible flow rebinds onto the device output without a host round trip and a device-absent flow degrades to the arena-allocated CPU steady-state through the same capsule, never a parallel device-flow type.
