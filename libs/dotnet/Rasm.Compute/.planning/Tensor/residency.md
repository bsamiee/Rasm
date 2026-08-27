# [COMPUTE_RESIDENCY]

ONNX C-data residency classifies every `OrtValue` by backing location and ownership through one `OrtResidency` matrix carrying its `Locale` and `ReleaseOwner` columns. `TensorBridge` owns carrier-shaped ingress and dtype-shaped egress, `DeviceMemory` owns shared allocation, `BoundFlow` owns steady-state `OrtIoBinding` under one `BindingSource` rebind axis and one `FlowState` lifecycle, and `EncodedTensor` owns model-lane geometry wrapping without repacking the kernel payload.

## [01]-[INDEX]

- [02]-[ORT_BRIDGE]: `OrtResidency` matrix with its locale and release-ownership columns; carrier-keyed C-data ingress and dtype-keyed egress; `PinnedPlane<T>` handle-rooted pin for the crossings outliving their statement; `DeviceMemory` shared-allocator descriptor and residency probe; `BoundFlow` gate-aware `OrtIoBinding` steady-state over one `BindingSource` rebind union.
- [03]-[GEOMETRY_ENCODING]: `EncodedGeometry` wraps the kernel payload host-neutral; `EncodedTensor` slices per channel and the generated `PackKind` wire projection fixes wire shape, layout, and the `FreeAxis` rows each kind derives.

## [02]-[ORT_BRIDGE]

- Owner: `OrtResidency` `[SmartEnum<string>]` the four-gate residency matrix over its `Locale` and `ReleaseOwner` columns; `TensorBridge` the static `OrtValue` C-data factory surface (carrier-keyed ingress, dtype-keyed egress, the device sink mint, the device-to-device relay, the residency probe); `PinnedPlane<T>` the ONE handle-rooted pin capsule every crossing that outlives its own statement takes; `DeviceMemory` the shared-allocator descriptor; `BindingSource` the `[Union]` naming what memory backs the next binding and who owns it; `FlowState` the `Live`/`Poisoned` capsule lifecycle; `BoundFlow` the ONE `OrtIoBinding` steady-state residency capsule the `Model/run#RUN_MODES` run-mode fold composes.
- Cases: `OrtResidency` rows memory-backed · device-resident · output-value · span-view (4); `Locale` rows host · device (2); `ReleaseOwner` rows capsule · caller · session (3); `BindingSource` cases `Arena(long[] Shape)` · `Pinned(PinnedPlane<T>)` · `DeviceArena(DeviceMemory, long[] Shape)` · `DevicePointer(OrtMemoryInfo, long[] Shape, nint, long)` · `External(OrtExternalAllocation In, OrtExternalAllocation Out)` · `Encoded(EncodedTensor)` (6, each carrying its own `ReleaseOwner`); `FlowState` cases `Live` · `Poisoned(Error Cause)` (2).
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source)` and its `MemoryOwner<T>`, array, foreign-pointer, pinned-plane, and `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` overloads discriminate ingress by carrier SHAPE — the one axis a value's own type already decides; `public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape)` mints a device sink; `public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed)` moves a device-resident pair whole on the producing device's own sync stream; `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` and its flat `Span<T>` overload project an output by the dtype row; `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena, TensorDtype row)` leases the steady-state capsule and `flow.Rebind(BindingSource)` is its ONE re-binding entry — the four name-suffix rebind siblings are the deleted form, because what backs the next binding is a VALUE the caller hands over, never a method name the caller picks. `flow.Chain(BoundFlow next)` relays this capsule's device-resident sink into the next capsule's bound input under `Relay`. `Fin<T>` aborts when the egress destination is undersized against the `GetTensorSizeInBytes` count, ingress shape volume fails to cover its payload, a native mint rejects, or the capsule is `Poisoned`.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule)
- Growth: a new accelerator is one `DeviceMemory` descriptor over its `OrtEpDevice` reaching the existing `Allocate`/device-pointer ingress, never a per-call marshal helper; a new carrier is one `TensorBridge.Ingress` overload discriminating by carrier shape; a new BACKING is one `BindingSource` case carrying its own `ReleaseOwner` column, and the generated total `Switch` in `Rebind` breaks until its arm exists — where four sibling entrypoints let a fifth backing land as a fifth method nothing forced anyone to notice; the `DeviceResident` row is the one residency gate the `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row and the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` both bind — a WGPU compute buffer and an ORT device value share this one residency row so device-ness is a residency discriminant, never a second tensor owner or a parallel device-residency matrix; the resolved shared `ONE_WGPU_DEVICE` adapter is what a composition root folds into the `device-wgpu` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` (present iff the adapter resolves), so the same device-presence fact the `DeviceResident` gate observes contributes the substrate key the `Substrate.DeviceWgpu` `!Providers.Contains(Key)` gate reads, never a raw `Device`/adapter handle pushed into `Providers`; zero new surface.
- Boundary — carriers and release ownership: `OrtValue` is the sole model-boundary carrier. Every ingress shape proves non-negative extents, checked volume, payload coverage, and native construction on `Fin`; zero-sized tensors remain representable. Buffer ROOTING splits by ownership: `Tensor<T>.GetPinnableReference` roots a `fixed` region and serves the in-statement copy alone, so a managed plane whose pointer an `OrtValue` or a device submission holds past that region roots on `PinnedPlane<T>` instead, and the raw-`nint` ingress overload is reserved for genuinely FOREIGN memory (a device allocation, an ORT arena block) the caller can neither own nor pin — a managed buffer reaching that overload is the deleted unrooted form. A strided plane hands no contiguous pointer, so the pin repacks once through the non-throwing `TryFlattenTo` into a GRANTED rental the capsule releases with the handle; a plane with no dense dimension flattens element-wise, so that walk carries a stated ceiling and refuses past it rather than paying an unannounced traversal. Release ownership is a COLUMN on the `BindingSource` case, never a nulled field a reader reconstructs from call history: `Arena`, `Pinned`, `DevicePointer`, and `Encoded` mint capsule-owned values the capsule releases at the next rebind or at `Dispose`, while `DeviceArena` and `External` bind memory the CALLER owns — an `OrtMemoryAllocation` or an `OrtExternalAllocation` whose lifetime must outlive both the binding and this capsule — so the capsule releases neither. `Dispose` releases each owned native handle once.
- Boundary — lifecycle: rebinding allocates its replacement before clearing the current bindings and restores the prior CPU bindings on failure; when that restore ITSELF fails the capsule's binding table is in a state no code can name, so the capsule transitions to `FlowState.Poisoned` carrying the restore cause and every later `Write`, `Run`, `Flow`, `Rebind`, and `Chain` refuses against it. A capsule that reported a restore failure and then accepted a write published results from an unknown binding — the one shape this capsule never publishes — and `Dispose` still releases, because a poisoned capsule owns handles exactly as a live one does.
- Boundary — gates: every egress proves dtype identity, native byte count, and destination density where raw-byte projection requires it, through ONE admitted-dtype gate both overloads take, so the copy arm is the only thing that differs between them. `BoundFlow.Write<T>` and framed-byte `Write` return `Fin<Unit>`, enforce exact dtype and length, and let `Flow` abort before `Drive`. Gate selection for a NATIVE value derives from the session's own `OrtMemoryInfo` through `OrtResidency.Classify` — a caller-declared gate the model contradicts is the deleted form, and the allocator name stays result evidence rather than a discriminant; `SpanView` is the one row `Classify` never answers because it classifies no native value at all — it is the Compute-minted wrap of a managed span the `Tensor/dispatch#KERNEL_DISPATCH` copy point owns, and it carries `ReleaseOwner.Caller` for exactly that reason. A relay proves BOTH ends device-resident through `Classify` before any native copy — a caller-declared residency the model contradicts is the deleted form the gate already names — and the `Locale` column is what that proof reads, so a sixth residency row breaks the relay gate at compile time rather than routing silently to refusal. Every staged byte this page rents carries a `Tensor/memory#ALLOCATION_AXIS` `Grant`: the repack rental and the interleave plane are sized by a CALLER's strided plane or channel arity, not by an already-admitted kernel operand extent, so the kernel-interior-scratch exemption does not reach them.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Locale {
    public static readonly Locale Host = new("host");
    public static readonly Locale Device = new("device");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReleaseOwner {
    public static readonly ReleaseOwner Capsule = new("capsule");
    public static readonly ReleaseOwner Caller = new("caller");
    public static readonly ReleaseOwner Session = new("session");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OrtResidency {
    public static readonly OrtResidency MemoryBacked = new("memory-backed", locale: Locale.Host, releaseOwner: ReleaseOwner.Capsule);
    public static readonly OrtResidency DeviceResident = new("device-resident", locale: Locale.Device, releaseOwner: ReleaseOwner.Caller);
    public static readonly OrtResidency OutputValue = new("output-value", locale: Locale.Host, releaseOwner: ReleaseOwner.Session);
    public static readonly OrtResidency SpanView = new("span-view", locale: Locale.Host, releaseOwner: ReleaseOwner.Caller);

    public Locale Locale { get; }

    public ReleaseOwner ReleaseOwner { get; }

    public static OrtResidency Classify(OrtMemoryInfo info) =>
        info.GetMemoryType() is OrtMemType.CpuOutput ? OutputValue
        : info.GetDeviceMemoryType() is OrtDeviceMemoryType.HOST_ACCESSIBLE
          || info.GetAllocatorType() is OrtAllocatorType.ArenaAllocator && info.GetMemoryType() is OrtMemType.Cpu or OrtMemType.CpuInput ? MemoryBacked
        : DeviceResident;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindingSource {
    private BindingSource() { }

    public sealed record Arena(long[] Shape) : BindingSource;
    public sealed record Pinned<T>(PinnedPlane<T> Plane, long[] Shape) : BindingSource where T : unmanaged;
    public sealed record DeviceArena(DeviceMemory Device, long[] Shape) : BindingSource;
    public sealed record DevicePointer(OrtMemoryInfo Info, long[] Shape, nint Pointer, long Bytes) : BindingSource;
    public sealed record External(OrtExternalAllocation In, OrtExternalAllocation Out) : BindingSource;
    public sealed record Encoded(EncodedTensor Tensor) : BindingSource;

    public ReleaseOwner ReleaseOwner => Switch(
        arena: static _ => ReleaseOwner.Capsule,
        pinned: static _ => ReleaseOwner.Capsule,
        deviceArena: static _ => ReleaseOwner.Caller,
        devicePointer: static _ => ReleaseOwner.Capsule,
        external: static _ => ReleaseOwner.Caller,
        encoded: static _ => ReleaseOwner.Capsule);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FlowState {
    private FlowState() { }

    public sealed record Live : FlowState;
    public sealed record Poisoned(Error Cause) : FlowState;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DeviceMemory(OrtEpDevice Device, OrtDeviceMemoryType MemoryType, OrtAllocatorType AllocatorType) {
    public OrtMemoryInfo Info => Device.GetMemoryInfo(MemoryType);

    public Fin<OrtAllocator> Shared() =>
        Try.lift(() => Fin.Succ(ModelSessions.SharedAllocator(Device, MemoryType))).Run().Bind(static inner => inner);

    public Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(TensorDtype row, long[] shape) =>
        Shared().Bind(allocator => Try.lift(() => Fin.Succ((allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape)))).Run().Bind(static inner => inner));
}

public sealed class PinnedPlane<T> : IDisposable where T : unmanaged {
    private const long StridedRepackCeiling = 1L << 26;

    private readonly MemoryHandle handle;
    private readonly Option<MemoryOwner<T>> rental;
    private bool disposed;

    private PinnedPlane(MemoryHandle handle, Option<MemoryOwner<T>> rental, long elements, long bytes, Option<AllocationEvidence> evidence) =>
        (this.handle, this.rental, Elements, Bytes, Evidence) = (handle, rental, elements, bytes, evidence);

    public long Elements { get; }
    public long Bytes { get; }

    public Option<AllocationEvidence> Evidence { get; }

    public unsafe nint Pointer => (nint)handle.Pointer;

    public static Fin<PinnedPlane<T>> Of(Tensor<T> plane, TensorDtype row, AllocationRequest staging) =>
        (AdmissionSlots.Gate(
             row.Clr == typeof(T),
             TensorReason.DtypeMismatch.Fault("pin-dtype", row.Key, typeof(T).Name)),
         AdmissionSlots.Gate(
             row.OrtElementBytes.IsSome,
             TensorReason.ByteStrideAbsent.Fault("pin-byte-stride", row.Key)),
         Volume(plane, row))
            .Apply(static (_, _, bytes) => bytes).As().ToFin()
            .Bind(bytes => plane.IsDense
                ? Rooted(row, () => new PinnedPlane<T>(plane.GetPinnedHandle(), None, plane.FlattenedLength, bytes, None))
                : Repack(plane, row, plane.FlattenedLength, bytes, staging));

    private static Validation<Error, long> Volume(Tensor<T> plane, TensorDtype row) =>
        TensorBridge.NativeBytes(row, plane.FlattenedLength).ToValidation();

    static Fin<PinnedPlane<T>> Repack(Tensor<T> plane, TensorDtype row, long elements, long bytes, AllocationRequest staging) =>
        elements > int.MaxValue
            ? TensorReason.ExtentOverflow.Fail<PinnedPlane<T>>("pin-strided-width", row.Key, elements.ToString(CultureInfo.InvariantCulture))
        : !plane.HasAnyDenseDimensions && elements > StridedRepackCeiling
            ? TensorReason.StagingOverBound.Fail<PinnedPlane<T>>("pin-strided-oversize", row.Key, $"{elements}>{StridedRepackCeiling}")
        : AllocationClass.PooledMemory.Rent<T>(staging with { RequestedBytes = bytes, Mode = AllocationMode.Clear }, checked((int)elements))
            .Bind(rent => (plane.TryFlattenTo(rent.Buffer.Span)
                    ? Rooted(row, () => new PinnedPlane<T>(rent.Buffer.Memory.Pin(), Some(rent.Buffer), elements, bytes, Some(rent.Evidence)))
                    : TensorReason.ShapeMismatch.Fail<PinnedPlane<T>>("pin-flatten", row.Key, $"rank={plane.Rank}"))
                .Rollback(rent.Buffer));

    static Fin<PinnedPlane<T>> Rooted(TensorDtype row, Func<PinnedPlane<T>> root) =>
        Try.lift(() => Fin.Succ(root())).Run().Bind(static inner => inner);

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        handle.Dispose();
        rental.Iter(static owner => owner.Dispose());
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

    public static Fin<OrtValue> Ingress<T>(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, PinnedPlane<T> pinned) where T : unmanaged =>
        CoveredBytes(shape, row, pinned.Bytes).Bind(admitted =>
            Minted(() => OrtValue.CreateTensorValueWithData(memory, row.Element, admitted, pinned.Pointer, pinned.Bytes)));

    public static Fin<OrtValue> Ingress(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> tokens) =>
        Minted(() => OrtValue.CreateFromStringTensor(tokens));

    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        Shape(shape).Bind(admitted => device.Allocate(row, admitted.Shape));

    public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed) =>
        (Source: OrtResidency.Classify(produced.GetTensorMemoryInfo()), Sink: OrtResidency.Classify(consumed.GetTensorMemoryInfo())) is var pair
        && pair.Source.Locale == Locale.Device && pair.Sink.Locale == Locale.Device
            ? Custody.Bracket(
                acquire: () => device.Device.CreateSyncStream(FrozenDictionary<string, string>.Empty),
                project: stream => Try.lift(() => { OrtEnv.Instance().CopyTensors([produced], [consumed], stream); return Fin.Succ(unit); }).Run().Bind(static inner => inner))
            : TensorReason.ResidencyMismatch.Fail<Unit>("relay-residency", pair.Source.Key, pair.Sink.Key);

    private static Fin<long[]> Covered(ReadOnlySpan<long> shape, long payload) =>
        Shape(shape).Bind(admitted => admitted.Volume == payload
            ? Fin.Succ(admitted.Shape)
            : TensorReason.ShapeMismatch.Fail<long[]>("ingress-cover-gap", $"{payload}!={admitted.Volume}"));

    private static Fin<long[]> CoveredBytes(ReadOnlySpan<long> shape, TensorDtype row, long payloadBytes) =>
        Shape(shape).Bind(admitted => NativeBytes(row, admitted.Volume).Bind(bytes =>
            bytes != payloadBytes
                ? TensorReason.ShapeMismatch.Fail<long[]>("ingress-cover-gap", row.Key, $"{payloadBytes}!={bytes}")
                : Fin.Succ(admitted.Shape)));

    internal static Fin<long> NativeBytes(TensorDtype row, long elements) =>
        row.OrtElementBytes
            .ToFin(TensorReason.ByteStrideAbsent.Fault("ingress-byte-stride", row.Key))
            .Bind(stride => elements > long.MaxValue / stride
                ? TensorReason.ExtentOverflow.Fail<long>("ingress-volume-overflow", row.Key)
                : Fin.Succ(elements * stride));

    private static Fin<(long[] Shape, long Volume)> Shape(ReadOnlySpan<long> shape) {
        long[] admitted = shape.ToArray();
        return admitted.Length > 0 && TensorPrimitives.Min<long>(admitted) < 0
            ? TensorReason.ShapeMismatch.Fail<(long[], long)>("ingress-shape", TensorPrimitives.Min<long>(admitted).ToString(CultureInfo.InvariantCulture))
            : Try.lift(() => Fin.Succ((admitted, admitted.Length == 0 ? 1L : checked(TensorPrimitives.Product<long>(admitted))))).Run().Bind(static inner => inner);
    }

    private static Fin<OrtValue> Minted(Func<OrtValue> mint) =>
        Try.lift(() => Fin.Succ(mint())).Run().Bind(static inner => inner);

    private static Fin<TensorDtype> Projected<T>(OrtValue value, long destinationElements, bool dense) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            row.ElementCount(value.GetTensorSizeInBytes()).Bind(count =>
                count != destinationElements ? TensorReason.ShapeMismatch.Fail<TensorDtype>("egress-undersized", row.Key, $"{count}!={destinationElements}")
                : row.Clr != typeof(T) ? TensorReason.DtypeMismatch.Fail<TensorDtype>("egress-dtype", row.Key, typeof(T).Name)
                : row.Quantized && !dense ? TensorReason.QuantizationInvalid.Fail<TensorDtype>("egress-strided-quantized", row.Key)
                : Fin.Succ(row)));

    public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination) where T : unmanaged {
        long flattened = destination.FlattenedLength;
        Fin<TensorDtype> admitted = Projected<T>(value, flattened, destination.IsDense);
        if (admitted.Case is not TensorDtype row) { return admitted.Map(static _ => unit); }
        try {
            if (row.Quantized) { value.GetTensorDataAsSpan<T>().CopyTo(MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)flattened))); }
            else { value.GetTensorDataAsTensorSpan<T>().CopyTo(destination); }
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Fin.Fail<Unit>(Error.New(ex.Message, ex)); }
    }

    public static Fin<Unit> Egress<T>(OrtValue value, Span<T> destination) where T : unmanaged {
        Fin<TensorDtype> admitted = Projected<T>(value, destination.Length, dense: true);
        if (admitted.Case is not TensorDtype row) { return admitted.Map(static _ => unit); }
        Span<T> sink = destination;
        return Try.lift(() => { value.GetTensorDataAsSpan<T>().CopyTo(sink); return Fin.Succ(unit); }).Run().Bind(static inner => inner);
    }

    public static (Seq<(string Name, OrtResidency Gate)> Inputs, Seq<(string Name, OrtResidency Gate)> Outputs) Residency(InferenceSession session) {
        using IDisposableReadOnlyCollection<OrtMemoryInfo> inputs = session.GetMemoryInfosForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> outputs = session.GetMemoryInfosForOutputs();
        return (toSeq(inputs).Map(static info => (info.Name, OrtResidency.Classify(info))),
                toSeq(outputs).Map(static info => (info.Name, OrtResidency.Classify(info))));
    }

    public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena, TensorDtype row) =>
        BoundFlow.Lease(session, inputName, outputName, shape.ToArray(), arena, row);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class BoundFlow : IDisposable {
    private readonly InferenceSession session;
    private readonly OrtIoBinding binding;
    private readonly RunOptions run;
    private readonly OrtAllocator arena;
    private readonly TensorDtype row;
    private readonly string inputName, outputName;
    private OrtValue bound, sink;
    private Option<OrtValue> owned;
    private FlowState state = new FlowState.Live();
    private bool disposed;

    private BoundFlow(InferenceSession session, OrtIoBinding binding, RunOptions run, OrtAllocator arena, TensorDtype row, string inputName, string outputName, OrtValue bound, OrtValue sink) =>
        (this.session, this.binding, this.run, this.arena, this.row, this.inputName, this.outputName, this.bound, this.sink, owned) =
            (session, binding, run, arena, row, inputName, outputName, bound, sink, None);

    public FlowState State => state;

    public static Fin<BoundFlow> Lease(InferenceSession session, string inputName, string outputName, long[] shape, OrtAllocator arena, TensorDtype row) {
        OrtValue? bound = null, sink = null;
        RunOptions? options = null;
        OrtIoBinding? binding = null;
        return Try.lift(() => {
                bound = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
                sink = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
                options = new RunOptions();
                binding = session.CreateIoBinding();
                binding.BindInput(inputName, bound);
                binding.BindOutput(outputName, sink);
                return Fin.Succ(new BoundFlow(session, binding, options, arena, row, inputName, outputName, bound, sink));
            }).Run().Bind(static inner => inner)
            .Rollback(bound, sink, options, binding);
    }

    public Fin<Unit> Write<T>(ReadOnlySpan<T> payload) where T : unmanaged {
        if (Refused() is { IsFail: true } refused) { return refused; }
        if (row.Clr != typeof(T)) { return TensorReason.DtypeMismatch.Fail<Unit>("bound-dtype", row.Key, typeof(T).Name); }
        try {
            Span<T> destination = bound.GetTensorMutableDataAsSpan<T>();
            if (payload.Length != destination.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("bound-length", row.Key, $"{payload.Length}!={destination.Length}"); }
            payload.CopyTo(destination);
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Fin.Fail<Unit>(Error.New(ex.Message, ex)); }
    }

    public Fin<Unit> Write(ReadOnlySequence<byte> window) {
        if (Refused() is { IsFail: true } refused) { return refused; }
        try {
            Span<byte> destination = bound.GetTensorMutableRawData();
            if (window.Length != destination.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("bound-length", row.Key, $"{window.Length}!={destination.Length}"); }
            window.CopyTo(destination);
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Fin.Fail<Unit>(Error.New(ex.Message, ex)); }
    }

    public Fin<IDisposableReadOnlyCollection<OrtValue>> Run(RunOptions options) =>
        Refused<IDisposableReadOnlyCollection<OrtValue>>().Bind(_ =>
            Try.lift(() => { Drive(options); return Fin.Succ(binding.GetOutputValues()); }).Run().Bind(static inner => inner));

    public Fin<Unit> Flow<T>(ReadOnlySpan<T> input, in TensorSpan<T> output) where T : unmanaged {
        Fin<Unit> written = Write(input);
        if (written.Case is not Unit) { return written; }
        Fin<Unit> driven = Try.lift(() => { Drive(run); return Fin.Succ(unit); }).Run().Bind(static inner => inner);
        return driven.Case is not Unit ? driven : TensorBridge.Egress(sink, output);
    }

    public Fin<Unit> Chain(DeviceMemory device, BoundFlow next) =>
        Refused().Bind(_ => next.Refused()).Bind(_ => TensorBridge.Relay(device, sink, next.bound, key));

    public Fin<Unit> Rebind(BindingSource source) =>
        Refused().Bind(_ => Next(source).Bind(next => Try.lift(() => {
                binding.ClearBoundInputs();
                binding.ClearBoundOutputs();
                next.Bind(binding, inputName, outputName);
                return Fin.Succ(unit);
            }).Run().Bind(static inner => inner)
            .Match(
                Succ: _ => { Adopt(source, next); return Fin.Succ(unit); },
                Fail: error => Restore(error))));

    private Fin<Bound> Next(BindingSource source) => source.Switch(
        state: (Arena: arena, Row: row),
        arena: static (s, a) => Minted(s, a.Shape).Map(static pair => Bound.Values(pair.Input, pair.Output)),
        pinned: (s, p) => TensorBridge.Ingress(OrtMemoryInfo.DefaultInstance, s.Row, p.Shape, p.Plane)
            .Bind(input => Minted(s, p.Shape).Map(pair => Bound.Values(input, pair.Output))),
        deviceArena: (s, d) => TensorBridge.Allocate(d.Device, s.Row, d.Shape)
            .Map(sink => Bound.Device(sink.Sink, d.Device.Info)),
        devicePointer: (s, d) => TensorBridge.Ingress(d.Info, s.Row, d.Shape, d.Pointer, d.Bytes)
            .Map(input => Bound.Device(input, d.Info)),
        external: static (_, e) => Fin.Succ(Bound.External(e.In, e.Out)),
        encoded: (s, e) => e.Tensor.Admit()
            .Bind(input => Minted(s, e.Tensor.WireExtents).Map(pair => Bound.Values(input, pair.Output))));

    private static Fin<(OrtValue Input, OrtValue Output)> Minted((OrtAllocator Arena, TensorDtype Row) s, long[] shape) =>
        Try.lift(() => Fin.Succ((
            OrtValue.CreateAllocatedTensorValue(s.Arena, s.Row.Element, shape),
            OrtValue.CreateAllocatedTensorValue(s.Arena, s.Row.Element, shape)))).Run().Bind(static inner => inner);

    private void Adopt(BindingSource source, Bound next) {
        OrtValue priorBound = bound, priorSink = sink;
        Option<OrtValue> priorOwned = owned;
        (bound, sink) = (next.Input, next.Output);
        owned = source.ReleaseOwner == ReleaseOwner.Capsule ? Some(next.Input) : None;
        priorOwned.Iter(static value => value.Dispose());
        if (source.ReleaseOwner == ReleaseOwner.Capsule) { priorBound.Dispose(); priorSink.Dispose(); }
    }

    private Fin<Unit> Restore(Error cause) =>
        Try.lift(() => {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, bound);
            binding.BindOutput(outputName, sink);
            return Fin<Unit>.Fail(cause);
        }).Run().Bind(static inner => inner).MapFail(restore => { state = new FlowState.Poisoned(restore); return restore; });

    private Fin<Unit> Refused() => Refused<Unit>().Map(static _ => unit);

    private Fin<A> Refused<A>() => state.Switch(
        live: static _ => Fin.Succ<A>(default!),
        poisoned: static p => Fin.Fail<A>(p.Cause));

    private void Drive(RunOptions options) {
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(options, binding);
        binding.SynchronizeBoundOutputs();
    }

    private readonly record struct Bound(OrtValue Input, OrtValue Output, Option<OrtMemoryInfo> Device, Option<OrtExternalAllocation> ExternalOut) {
        public static Bound Values(OrtValue input, OrtValue output) => new(input, output, None, None);
        public static Bound Device(OrtValue input, OrtMemoryInfo info) => new(input, input, Some(info), None);
        public static Bound External(OrtExternalAllocation input, OrtExternalAllocation output) => new(default!, default!, None, Some(output)) { };

        public void Bind(OrtIoBinding binding, string inputName, string outputName) {
            ExternalOut.Match(
                Some: external => { binding.BindInput(inputName, external); binding.BindOutput(outputName, external); },
                None: () => {
                    binding.BindInput(inputName, Input);
                    Device.Match(
                        Some: info => binding.BindOutputToDevice(outputName, info),
                        None: () => binding.BindOutput(outputName, Output));
                });
        }
    }

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        owned.Iter(static value => value.Dispose());
        run.Dispose();
        binding.Dispose();
        bound.Dispose();
        sink.Dispose();
    }
}
```

## [03]-[GEOMETRY_ENCODING]

- Owner: `EncodedTensor` — the model-lane wrap holding the kernel `Rasm.Drawing.EncodedGeometry` WHOLE beside its layout row; `FreeAxis` the `[SmartEnum<string>]` free-dimension vocabulary carrying its own derivation delegate; `WireRow` the per-`PackKind` wire projection, answered by the kernel roster's own generated total `Switch`.
- Cases: `FreeAxis` rows `N` · `V` · `C` · `F` · `U` · `W` · `H` (7), each carrying the `Derive` column that answers its extent from the payload or answers absence; `WireRow` is a value, not a roster — the seven kernel `PackKind` rows each answer one.
- Entry: `Of(EncodedGeometry, PackKind)` derives every axis the kind's row can answer and refuses by name the kind whose row carries an underivable axis, pointing at `Of(EncodedGeometry, PackKind, Option<Seq<(string Name, long Extent)>>, Option<Tensor<long>>)` — the entry carrying explicit spatial dimensions without default ghosts. `Fin<T>` rejects lossy witnesses, non-positive or mismatched dimensions, underivable `U`/`V` and `H`/`W` grids, invalid channel ranges, and overflowed interleaving shapes.
- Result: the kernel `EncodedGeometry.Witness` is the lossless-round-trip proof, its `ContentHash` keyed by the witness's own `DigestRoot` — the `Spatial/reconciliation#RECONCILIATION_BRIDGE` source digest on an `Encode.Apply` mint, the packed-payload digest on an `Encode.Of` raw-lane mint (the interchange arenas), so a consumer keying dedup on the hash reads `Root` before comparing; `Of` admits only a lossless payload, so the residency wrap carries no second witness and mints no second content key.
- Packages: Rasm (project), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new representation is one kernel `PackKind` row at the `Rasm/Drawing/pack#ENCODING_BAND` owner, and the generated total `Switch` in `Wire` BREAKS until this page answers it — where a `FrozenDictionary` mirror let `gaussian-splat` sit unrepresented while a prose line asserted the table was complete and a splat geometry faulted at runtime instead of at build; a new feature channel is one kernel `EncodingChannel` row, read here through the descriptor set with zero residency edit; a new free dimension is one `FreeAxis` row carrying its own derivation; zero new surface.
- Boundary: geometry channel materialization remains in `Rasm.Drawing.Encode.Apply`; residency receives host-neutral `EncodedGeometry` and holds it whole. The wire projection is the kernel roster's own generated `Switch` rather than a dictionary keyed on that roster, so representation identity is TYPE identity — the same law `Rasm.AppHost/Sandbox/solver#SOLVER_KIND` states from the other end, where `SolverKind` rows carry `Input`/`Output` columns speaking `PackKind` directly and the mirrored `EncodingKind` roster that once sat beside them is the deleted form. The `Field` (`geodesic`+`weight` lanes, positions omitted because the witness digest binds the source mesh), `Toolpath` (`position`+`arc-center`+`arc-sense`+`weight`, so an analytic arc survives packing as content rather than sampled chords), and `GaussianSplat` (`scale`+`rotation`+`harmonic` beside position and colour) rows are landed this way on `NxC`, never a residency-side packer. Splat residency has TWO carriers by concern, not by drift: `Runtime/payload#RESIDENCY_PAYLOAD` `ResidencyPayload` is the render-streaming carrier `Rasm.AppUi/Render/reality` consumes, and `EncodedTensor` is the model-input carrier — a splat reaching an ONNX model crosses here, a splat reaching a rasteriser crosses there, and neither wraps the other. `EncodedTensor.Channel` returns the admitted zero-copy `ReadOnlyMemory<byte>` slice at the channel's STORED width, never a default ref-struct ghost and never a float re-typing of a dtype-strided arena, whose float16 and unorm8 lanes such a reinterpretation reads as garbage; descriptor tiling, extent, and offset are proved once by the kernel's own `EncodedGeometry.IsValid` claim set, so this wrap re-derives no range check, and geometry reads resolve BY DESCRIPTOR through that arena rather than by a named column or an assumed stride. `ToTensor` widens through `ChannelDtype.Unpack` into ONE granted plane and interleaves channel-blocked SoA into point-major `[Count, FeatureWidth]` through `Span2D<float>` row addressing — a jagged index walk over a flat buffer computed the same offsets by hand on every element; `Tensor/layout#LAYOUT_ALGEBRA` owns later rank edits. Free-dimension rows feed `AddFreeDimensionOverrideByName`. `U`/`V` and `H`/`W` never derive by assigning the same flat `Count` to both axes — the `FreeAxis` row for each answers absence, and derivability is therefore a fact the roster computes rather than a `bool` column a caller trusts. The `BrepPatch` `NurbsControlTensor` row carries a control net whose semantic authority is the kernel `Rasm/Parametric/nurbs#NURBS_ENGINE` `Nurbs.Of` admission — homogeneous SoA columns, strictly positive weights, normalized knots — so any quantization of that lane must re-admit through that gate, never a residency-local judgement.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FreeAxis {
    public static readonly FreeAxis N = new("N", derive: static (g, _) => Some((long)g.Count));
    public static readonly FreeAxis V = new("V", derive: static (g, _) => Some((long)g.Count));
    public static readonly FreeAxis C = new("C", derive: static (g, _) => Some(g.Descriptors.Sum(static d => (long)d.Channel.Arity)));
    public static readonly FreeAxis F = new("F", derive: static (_, indices) => indices.Map(static topology => (long)topology.Lengths[0]));
    public static readonly FreeAxis U = new("U", derive: static (_, _) => None);
    public static readonly FreeAxis W = new("W", derive: static (_, _) => None);
    public static readonly FreeAxis H = new("H", derive: static (_, _) => None);

    [UseDelegateFromConstructor]
    public partial Option<long> Derive(EncodedGeometry geometry, Option<Tensor<long>> indices);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WireRow(LayoutForm Layout, string WireShape, Seq<FreeAxis> Axes) {
    public bool Derivable(EncodedGeometry geometry, Option<Tensor<long>> indices) =>
        Axes.ForAll(axis => axis.Derive(geometry, indices).IsSome);

    public static WireRow Of(PackKind kind) => kind.Switch(
        pointCloud:    static _ => new WireRow(LayoutForm.NxC, "PointCloudTensor", Seq(FreeAxis.N, FreeAxis.C)),
        meshPatch:     static _ => new WireRow(LayoutForm.VertexFace, "MeshTensor", Seq(FreeAxis.V, FreeAxis.F)),
        voxelGrid:     static _ => new WireRow(LayoutForm.Nchw, "VoxelGridTensor", Seq(FreeAxis.C, FreeAxis.H, FreeAxis.W)),
        brepPatch:     static _ => new WireRow(LayoutForm.NxC, "NurbsControlTensor", Seq(FreeAxis.U, FreeAxis.V)),
        field:         static _ => new WireRow(LayoutForm.NxC, "FieldTensor", Seq(FreeAxis.N, FreeAxis.C)),
        toolpath:      static _ => new WireRow(LayoutForm.NxC, "ToolpathTensor", Seq(FreeAxis.N, FreeAxis.C)),
        gaussianSplat: static _ => new WireRow(LayoutForm.NxC, "GaussianSplatTensor", Seq(FreeAxis.N, FreeAxis.C)));
}

public sealed record EncodedTensor(
    EncodedGeometry Source,
    LayoutForm Layout,
    string WireShape,
    Seq<(FreeAxis Axis, long Extent)> FreeDimensions,
    Option<Tensor<long>> Indices) {

    public Seq<EncodingChannelDescriptor> Descriptors => Source.Descriptors;

    public int Count => Source.Count;

    public int FeatureWidth => Descriptors.Sum(static descriptor => descriptor.Channel.Arity);

    public long[] WireExtents => FreeDimensions.Map(static pair => pair.Extent).ToArray();

    public static Fin<EncodedTensor> Of(EncodedGeometry geometry, PackKind kind) =>
        WireRow.Of(kind) is var row && !row.Derivable(geometry, None)
            ? TensorReason.AxisUnderivable.Fail<EncodedTensor>("free-dimension-explicit", kind.Key, string.Join(',', row.Axes.Map(static a => a.Key)))
            : Of(geometry, kind, None, None);

    public static Fin<EncodedTensor> Of(
        EncodedGeometry geometry,
        PackKind kind,
        Option<Seq<(FreeAxis Axis, long Extent)>> freeDimensions,
        Option<Tensor<long>> indices) =>
        !geometry.Witness.Lossless
            ? TensorReason.WitnessFail.Fail<EncodedTensor>("encoding-lossy", kind.Key)
            : WireRow.Of(kind) is var row && freeDimensions.Match(
                Some: Fin.Succ,
                None: () => Derived(row, geometry, indices)).Bind(dims =>
                dims.Exists(static d => d.Extent <= 0) || dims.Map(static d => d.Axis) != row.Axes
                    ? TensorReason.ShapeMismatch.Fail<EncodedTensor>("free-dimension-miss", row.WireShape)
                    : Fin.Succ(new EncodedTensor(geometry, row.Layout, row.WireShape, dims, indices)));

    public Fin<ReadOnlyMemory<byte>> Channel(EncodingChannel channel) =>
        Source.Channel(channel) is { IsEmpty: false } stored
            ? Fin.Succ(stored)
            : TensorReason.RowMissing.Fail<ReadOnlyMemory<byte>>("channel-miss", channel.Key);

    public Fin<Tensor<float>> ToTensor(AllocationRequest staging) =>
        Count <= 0 || FeatureWidth <= 0
            ? TensorReason.ShapeMismatch.Fail<Tensor<float>>("encoding-shape", $"{Count}x{FeatureWidth}")
            : Descriptors.TraverseM(descriptor => Channel(descriptor.Channel)).As().Bind(admitted => Interleaved(admitted, staging));

    private Fin<Tensor<float>> Interleaved(Seq<ReadOnlyMemory<byte>> admitted, AllocationRequest staging) {
        int width = FeatureWidth, widest = Descriptors.Max(static d => d.Channel.Arity);
        return Try.lift(() => Fin.Succ((
                Plane: checked(Count * width), Lane: checked(Count * widest)))).Run().Bind(static inner => inner)
            .Bind(sizes => AllocationClass.PooledMemory
                .Rent<float>(staging with { RequestedBytes = (long)sizes.Plane * sizeof(float), Mode = AllocationMode.Default }, sizes.Plane)
                .Bind(plane => AllocationClass.PooledMemory
                    .Rent<float>(staging with { RequestedBytes = (long)sizes.Lane * sizeof(float), Mode = AllocationMode.Default }, sizes.Lane)
                    .Map(scratch => {
                        using (scratch.Buffer) {
                            Span2D<float> destination = plane.Buffer.Span.AsSpan2D(Count, width);
                            int column = 0, index = 0;
                            foreach (EncodingChannelDescriptor descriptor in Descriptors) {
                                int arity = descriptor.Channel.Arity;
                                Span<float> lane = scratch.Buffer.Span[..(Count * arity)];
                                descriptor.Channel.Dtype.Unpack(admitted[index].Span, lane);
                                for (int element = 0; element < Count; element++) {
                                    lane.Slice(element * arity, arity).CopyTo(destination.GetRowSpan(element).Slice(column, arity));
                                }

                                column += arity;
                                index++;
                            }
                        }

                        return Tensor.Create(plane.Buffer.DangerousGetArray().Array!, [(nint)Count, (nint)width]);
                    })));
    }

    public Fin<OrtValue> Admit(AllocationRequest staging) => ToTensor(staging).Bind(static tensor => TensorBridge.Ingress(tensor));

    private static Fin<Seq<(FreeAxis Axis, long Extent)>> Derived(WireRow row, EncodedGeometry geometry, Option<Tensor<long>> indices) =>
        row.Axes.Map<Fin<(FreeAxis Axis, long Extent)>>(axis => axis.Derive(geometry, indices)
                .Map(extent => (axis, extent))
                .ToFin(TensorReason.AxisUnderivable.Fault("free-dimension-underivable", axis.Key)))
            .TraverseM(identity).As();
}
```
