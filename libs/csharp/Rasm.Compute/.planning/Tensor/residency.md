# [COMPUTE_RESIDENCY]

ONNX C-data residency classifies every `OrtValue` by backing location and ownership through one `OrtResidency` lattice carrying its `Locale` and `ReleaseOwner` columns. `TensorBridge` owns carrier-shaped ingress and dtype-shaped egress, `DeviceMemory` owns shared allocation, `BoundFlow` owns steady-state `OrtIoBinding` under one `BindingSource` rebind axis and one `FlowState` lifecycle, and `EncodedTensor` owns model-lane geometry wrapping without repacking the kernel payload.

## [01]-[INDEX]

- [02]-[ORT_BRIDGE]: `OrtResidency` lattice with its locale and release-ownership columns; carrier-keyed C-data ingress and dtype-keyed egress; `PinnedPlane<T>` handle-rooted pin for the crossings outliving their statement; `DeviceMemory` shared-allocator descriptor and residency probe; `BoundFlow` gate-aware `OrtIoBinding` steady-state over one `BindingSource` rebind union.
- [03]-[GEOMETRY_ENCODING]: `EncodedGeometry` wraps the kernel payload host-neutral; `EncodedTensor` slices per channel and the generated `PackKind` wire projection fixes wire shape, layout, and the `FreeAxis` rows each kind derives.

## [02]-[ORT_BRIDGE]

- Owner: `OrtResidency` `[SmartEnum<string>]` the four-gate residency lattice over its `Locale` and `ReleaseOwner` columns; `TensorBridge` the static `OrtValue` C-data factory surface (carrier-keyed ingress, dtype-keyed egress, the device sink mint, the device-to-device relay, the residency probe); `PinnedPlane<T>` the ONE handle-rooted pin capsule every crossing that outlives its own statement takes; `DeviceMemory` the shared-allocator descriptor; `BindingSource` the `[Union]` naming what memory backs the next binding and who owns it; `FlowState` the `Live`/`Poisoned` capsule lifecycle; `BoundFlow` the ONE `OrtIoBinding` steady-state residency capsule the `Model/run#RUN_MODES` run-mode fold composes.
- Cases: `OrtResidency` rows memory-backed · device-resident · output-value · span-view (4); `Locale` rows host · device (2); `ReleaseOwner` rows capsule · caller · session (3); `BindingSource` cases `Arena(long[] Shape)` · `Pinned(PinnedPlane<T>)` · `DeviceArena(DeviceMemory, long[] Shape)` · `DevicePointer(OrtMemoryInfo, long[] Shape, nint, long)` · `External(OrtExternalAllocation In, OrtExternalAllocation Out)` · `Encoded(EncodedTensor)` (6, each carrying its own `ReleaseOwner`); `FlowState` cases `Live` · `Poisoned(Error Cause)` (2).
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source)` and its `MemoryOwner<T>`, array, foreign-pointer, pinned-plane, and `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` overloads discriminate ingress by carrier SHAPE — the one axis a value's own type already decides; `public static Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape)` mints a device sink; `public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed, Op key)` moves a device-resident pair whole on the producing device's own sync stream; `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` and its flat `Span<T>` overload project an output by the dtype row; `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape, OrtAllocator arena, TensorDtype row)` leases the steady-state capsule and `flow.Rebind(BindingSource)` is its ONE re-binding entry — the four name-suffix rebind siblings are the deleted form, because what backs the next binding is a VALUE the caller hands over, never a method name the caller picks. `flow.Chain(BoundFlow next, Op key)` relays this capsule's device-resident sink into the next capsule's bound input under `Relay`. `Fin<T>` aborts when the egress destination is undersized against the `GetTensorSizeInBytes` count, ingress shape volume fails to cover its payload, a native mint rejects, or the capsule is `Poisoned`.
- Receipt: `TensorBridge.Crossing` is the ONE receipted crossing — it brackets a copy that genuinely moves bytes, classifies the gate off the value's own `OrtMemoryInfo`, measures the elapsed span rather than accepting one, and projects `CopyPoint` onto `ComputeReceipt.Copy`, which `Runtime/receipts#FOLD_PROJECTIONS` `ReceiptFolds.Crossings` aggregates by gate. `Egress` and `Relay` are the copies that stamp; a zero-copy `Ingress` wrap stamps nothing, because a zero-byte row inflates the census the fold reads.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule)
- Growth: a new accelerator is one `DeviceMemory` descriptor over its `OrtEpDevice` reaching the existing `Allocate`/device-pointer ingress, never a per-call marshal helper; a new carrier is one `TensorBridge.Ingress` overload discriminating by carrier shape; a new BACKING is one `BindingSource` case carrying its own `ReleaseOwner` column, and the generated total `Switch` in `Rebind` breaks until its arm exists — where four sibling entrypoints let a fifth backing land as a fifth method nothing forced anyone to notice; the `DeviceResident` row is the one residency gate the `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row and the `Tensor/dispatch#DEVICE_KERNELS` `DeviceDispatch` both bind — a WGPU compute buffer and an ORT device value share this one residency row so device-ness is a residency discriminant, never a second tensor owner or a parallel device-residency lattice; the resolved shared `ONE_WGPU_DEVICE` adapter is what a composition root folds into the `device-wgpu` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` (present iff the adapter resolves), so the same device-presence fact the `DeviceResident` gate observes contributes the substrate key the `Substrate.DeviceWgpu` `!Providers.Contains(Key)` gate reads, never a raw `Device`/adapter handle pushed into `Providers`; zero new surface.
- Boundary — carriers and release ownership: `OrtValue` is the sole model-boundary carrier. Every ingress shape proves non-negative extents, checked volume, payload coverage, and native construction on `Fin`; zero-sized tensors remain representable. Buffer ROOTING splits by ownership: `Tensor<T>.GetPinnableReference` roots a `fixed` region and serves the in-statement copy alone, so a managed plane whose pointer an `OrtValue` or a device submission holds past that region roots on `PinnedPlane<T>` instead, and the raw-`nint` ingress overload is reserved for genuinely FOREIGN memory (a device allocation, an ORT arena block) the caller can neither own nor pin — a managed buffer reaching that overload is the deleted unrooted form. A strided plane hands no contiguous pointer, so the pin repacks once through the non-throwing `TryFlattenTo` into a GRANTED rental the capsule releases with the handle; a plane with no dense dimension flattens element-wise, so that walk carries a stated ceiling and refuses past it rather than paying an unannounced traversal. Release ownership is a COLUMN on the `BindingSource` case, never a nulled field a reader reconstructs from call history: `Arena`, `Pinned`, `DevicePointer`, and `Encoded` mint capsule-owned values the capsule releases at the next rebind or at `Dispose`, while `DeviceArena` and `External` bind memory the CALLER owns — an `OrtMemoryAllocation` or an `OrtExternalAllocation` whose lifetime must outlive both the binding and this capsule — so the capsule releases neither. `Dispose` releases each owned native handle once.
- Boundary — lifecycle: rebinding allocates its replacement before clearing the current bindings and restores the prior CPU bindings on failure; when that restore ITSELF fails the capsule's binding table is in a state no code can name, so the capsule transitions to `FlowState.Poisoned` carrying the restore cause and every later `Write`, `Run`, `Flow`, `Rebind`, and `Chain` refuses against it. A capsule that reported a restore failure and then accepted a write published results from an unknown binding — the one shape this capsule never publishes — and `Dispose` still releases, because a poisoned capsule owns handles exactly as a live one does.
- Boundary — gates: every egress proves dtype identity, native byte count, and destination density where raw-byte projection requires it, through ONE admitted-dtype gate both overloads take, so the copy arm is the only thing that differs between them. `BoundFlow.Write<T>` and framed-byte `Write` return `Fin<Unit>`, enforce exact dtype and length, and let `Flow` abort before `Drive`. Gate selection for a NATIVE value derives from the session's own `OrtMemoryInfo` through `OrtResidency.Classify` — a caller-declared gate the model contradicts is the deleted form, and the allocator name stays receipt evidence rather than a discriminant; `SpanView` is the one row `Classify` never answers because it classifies no native value at all — it is the Compute-minted wrap of a managed span the `Tensor/dispatch#KERNEL_DISPATCH` copy point owns, and it carries `ReleaseOwner.Caller` for exactly that reason. A relay proves BOTH ends device-resident through `Classify` before any native copy — a caller-declared residency the model contradicts is the deleted form the gate already names — and the `Locale` column is what that proof reads, so a sixth residency row breaks the relay gate at compile time rather than routing silently to refusal. Every staged byte this page rents carries a `Tensor/memory#ALLOCATION_AXIS` `Grant`: the repack rental and the interleave plane are sized by a CALLER's strided plane or channel arity, not by an already-admitted kernel operand extent, so the kernel-interior-scratch exemption does not reach them.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Two typed columns replace four decorative bools: `Locale` is what the relay gate proves and `ReleaseOwner` is what
// the release law reads. A `Wraps`/`ProjectsInPlace` pair no member ever read is deleted — NAMED LOSS: nothing
// now records whether a gate's value aliases its source buffer, which no consumer asked and `ReleaseOwner` answers
// for the only question that had a caller (who frees it).
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
    // The one row `Classify` never answers: a Compute-minted wrap over a managed span, not a native value.
    public static readonly OrtResidency SpanView = new("span-view", locale: Locale.Host, releaseOwner: ReleaseOwner.Caller);

    public Locale Locale { get; }

    public ReleaseOwner ReleaseOwner { get; }

    // OrtResidency owns its own native discriminant: OrtMemType.CpuOutput marks a session-minted output whatever
    // its allocator, an arena over HOST_ACCESSIBLE memory stays host-side, and every other device memory class is
    // device-resident. Reading the allocator NAME instead reads a vendor string, never a class.
    public static OrtResidency Classify(OrtMemoryInfo info) =>
        info.GetMemoryType() is OrtMemType.CpuOutput ? OutputValue
        : info.GetDeviceMemoryType() is OrtDeviceMemoryType.HOST_ACCESSIBLE
          || info.GetAllocatorType() is OrtAllocatorType.ArenaAllocator && info.GetMemoryType() is OrtMemType.Cpu or OrtMemType.CpuInput ? MemoryBacked
        : DeviceResident;
}

// What memory backs the NEXT binding, as a value. Four name-suffix entrypoints carried this axis before, so a
// caller picked a backing by choosing a method and the ownership rule lived in prose beside a field that was
// nulled on three paths and set on a fourth. Each case carries its own `ReleaseOwner`, so the release law reads a
// column and a fifth backing breaks the generated `Switch` rather than landing as a fifth method.
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

// A restore that itself fails leaves the binding table in a state no code can name, so the capsule takes a state
// no later call can ignore. Without it a `rebind-restore` report was the only signal, and a caller that read the
// `Fin` and kept driving published results from an unknown binding.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FlowState {
    private FlowState() { }

    public sealed record Live : FlowState;
    public sealed record Poisoned(Error Cause) : FlowState;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct CopyPoint(OrtResidency Gate, long Bytes, string Device, Instant At, CorrelationId Correlation) {
    public ComputeReceipt.Copy Receipt(WorkLane lane, Duration elapsed) =>
        new(Gate, Bytes, Device) { Scope = new ReceiptScope.Execution(Correlation, lane, Substrate.Onnx, AllocationClass.NativeOrt, elapsed) };
}

// Shared ORT allocators are ModelSessions-owned: `ModelSessions.SharedAllocator` mints and maps the per-(device,
// memory) arena and its drain lifecycle releases it — a residency-local `CreateSharedAllocator` would mint a second
// unmapped arena the drain never releases, the deleted double-owner form. `Allocate` COMPOSES `Shared` rather
// than re-spelling the same native call five lines away under a second exception posture.
public readonly record struct DeviceMemory(OrtEpDevice Device, OrtDeviceMemoryType MemoryType, OrtAllocatorType AllocatorType) {
    public OrtMemoryInfo Info => Device.GetMemoryInfo(MemoryType);

    public Fin<OrtAllocator> Shared() =>
        Op.Of(name: "allocator-rejected").Catch(() => Fin.Succ(ModelSessions.SharedAllocator(Device, MemoryType)));

    public Fin<(OrtAllocator Allocator, OrtValue Sink)> Allocate(TensorDtype row, long[] shape) =>
        Shared().Bind(allocator => Op.Of(name: "device-sink").Catch(() => Fin.Succ((allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape)))));
}

// Handle-rooted pin for a native crossing that OUTLIVES its statement. `GetPinnableReference` roots a `fixed`
// region alone, so an `OrtValue` holding the pointer past that region, or a device submit draining after the
// frame, reads freed memory; `GetPinnedHandle` roots the same buffer on a `MemoryHandle` this capsule releases
// at its own end. A strided plane hands no contiguous pointer at all, so it repacks once through the
// non-throwing `TryFlattenTo` into a granted rental the capsule also owns — never a stride-ignoring reinterpret.
public sealed class PinnedPlane<T> : IDisposable where T : unmanaged {
    // Flattening a fully strided plane walks element-wise, so the repack ceiling bounds that walk rather than paying it
    // silently over a plane sized in gigabytes.
    private const long StridedRepackCeiling = 1L << 26;

    private readonly MemoryHandle handle;
    private readonly Option<MemoryOwner<T>> rental;
    private bool disposed;

    private PinnedPlane(MemoryHandle handle, Option<MemoryOwner<T>> rental, long elements, long bytes, Option<AllocationEvidence> evidence) =>
        (this.handle, this.rental, Elements, Bytes, Evidence) = (handle, rental, elements, bytes, evidence);

    public long Elements { get; }
    public long Bytes { get; }

    // Some on a repacked plane alone, so `Repacked` is a fact the evidence carries rather than a bool beside it.
    public Option<AllocationEvidence> Evidence { get; }

    public unsafe nint Pointer => (nint)handle.Pointer;

    // The four independent admissions ACCUMULATE, so a caller handed a mismatched dtype over an oversized plane
    // learns both; the sequential gate ladder reported whichever came first and hid the rest.
    public static Fin<PinnedPlane<T>> Of(Tensor<T> plane, TensorDtype row, AllocationRequest staging) =>
        (Element(row), Stride(row), Volume(plane, row))
            .Apply(static (_, _, bytes) => bytes).As().ToFin()
            .Bind(bytes => plane.IsDense
                ? Rooted(row, () => new PinnedPlane<T>(plane.GetPinnedHandle(), None, plane.FlattenedLength, bytes, None))
                : Repack(plane, row, plane.FlattenedLength, bytes, staging));

    private static Validation<Error, Unit> Element(TensorDtype row) =>
        row.Clr == typeof(T) ? unit : TensorReason.DtypeMismatch.Fault("pin-dtype", row.Key, typeof(T).Name);

    private static Validation<Error, Unit> Stride(TensorDtype row) =>
        row.OrtElementBytes.IsSome ? unit : TensorReason.ByteStrideAbsent.Fault("pin-byte-stride", row.Key);

    private static Validation<Error, long> Volume(Tensor<T> plane, TensorDtype row) =>
        TensorBridge.NativeBytes(row, plane.FlattenedLength).Match(
            Succ: static bytes => Validation<Error, long>.Success(bytes),
            Fail: static error => Validation<Error, long>.Fail(error));

    // Dense-run screening decides the repack COST: a plane with no dense dimension walks scalar strides, and
    // that walk is bounded rather than discovered at the copy. The rental is a GRANTED staging rent, because its
    // size comes from a caller's strided plane and not from an operand extent the kernel already admitted.
    static Fin<PinnedPlane<T>> Repack(Tensor<T> plane, TensorDtype row, long elements, long bytes, AllocationRequest staging) =>
        elements > int.MaxValue
            ? TensorReason.ExtentOverflow.Fail<PinnedPlane<T>>("pin-strided-width", row.Key, elements.ToString(CultureInfo.InvariantCulture))
        : !plane.HasAnyDenseDimensions && elements > StridedRepackCeiling
            ? TensorReason.StagingOverBound.Fail<PinnedPlane<T>>("pin-strided-oversize", row.Key, $"{elements}>{StridedRepackCeiling}")
        // Custody TRANSFERS on success — the rental becomes the pin's, released with its handle — and rolls back
        // on failure, which is exactly the kernel acquire-chain discriminant rather than a conditional dispose
        // reading the outcome it sits inside.
        : AllocationClass.PooledMemory.Rent<T>(staging with { RequestedBytes = bytes, Mode = AllocationMode.Clear }, checked((int)elements))
            .Bind(rent => (plane.TryFlattenTo(rent.Buffer.Span)
                    ? Rooted(row, () => new PinnedPlane<T>(rent.Buffer.Memory.Pin(), Some(rent.Buffer), elements, bytes, Some(rent.Evidence)))
                    : TensorReason.ShapeMismatch.Fail<PinnedPlane<T>>("pin-flatten", row.Key, $"rank={plane.Rank}"))
                .Rollback(rent.Buffer));

    static Fin<PinnedPlane<T>> Rooted(TensorDtype row, Func<PinnedPlane<T>> root) =>
        Op.Of(name: "pin-rejected").Catch(() => Fin.Succ(root()));

    public void Dispose() {
        if (disposed) { return; }
        disposed = true;
        handle.Dispose();
        rental.Iter(static owner => owner.Dispose());
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
    // block. A MANAGED buffer reaches the same factory only through `PinnedPlane<T>.Of`, whose handle outlives the value.
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

    // Device-to-device handoff for a chain whose links stay RESIDENT. `CopyTensors` moves whole values on the
    // device under one sync stream the producing device itself mints, so a producer sink and a consumer bound input
    // sharing an `OrtMemoryInfo` never cross device→host→device between links. Residency is PROVED, never declared:
    // both ends read the `Locale` column off `Classify`, and a pair whose memory descriptors disagree refuses
    // rather than taking a silent host round trip. The stream disposes with the copy, because a stream outliving
    // its transfer is a device handle no capsule owns.
    public static Fin<Unit> Relay(DeviceMemory device, OrtValue produced, OrtValue consumed, Op key) =>
        (Source: OrtResidency.Classify(produced.GetTensorMemoryInfo()), Sink: OrtResidency.Classify(consumed.GetTensorMemoryInfo())) is var pair
        && pair.Source.Locale == Locale.Device && pair.Sink.Locale == Locale.Device
            // Custody never TRANSFERS — the stream exists only for this copy — so the unconditional-disposal
            // member is the one the kernel rail names for exactly this shape.
            ? Custody.Bracket(
                acquire: () => device.Device.CreateSyncStream(FrozenDictionary<string, string>.Empty),
                project: stream => Op.Of(name: "relay-rejected").Catch(() => { OrtEnv.Instance().CopyTensors([produced], [consumed], stream); return Fin.Succ(unit); }),
                key: key)
            : TensorReason.ResidencyMismatch.Fail<Unit>("relay-residency", pair.Source.Key, pair.Sink.Key);

    // Shape covers the payload before any native mint, and every C-data factory call crosses once into the
    // rail — a native rejection lands as a typed fault, never an exception under an announced Succ.
    private static Fin<long[]> Covered(ReadOnlySpan<long> shape, long payload) =>
        Shape(shape).Bind(admitted => admitted.Volume == payload
            ? Fin.Succ(admitted.Shape)
            : TensorReason.ShapeMismatch.Fail<long[]>("ingress-cover-gap", $"{payload}!={admitted.Volume}"));

    private static Fin<long[]> CoveredBytes(ReadOnlySpan<long> shape, TensorDtype row, long payloadBytes) =>
        Shape(shape).Bind(admitted => NativeBytes(row, admitted.Volume).Bind(bytes =>
            bytes != payloadBytes
                ? TensorReason.ShapeMismatch.Fail<long[]>("ingress-cover-gap", row.Key, $"{payloadBytes}!={bytes}")
                : Fin.Succ(admitted.Shape)));

    // The forward direction of the vocabulary's own `ElementCount` correspondence: `OrtElementBytes` is the ORT
    // C-data stride column and the product is range-gated, so an element count whose byte width passes `long`
    // refuses typed rather than wrapping into a size the native factory would accept. One owner serves the pin
    // volume, the pointer ingress cover, and the pinned-plane cover alike.
    internal static Fin<long> NativeBytes(TensorDtype row, long elements) =>
        row.OrtElementBytes.Match(
            None: () => TensorReason.ByteStrideAbsent.Fail<long>("ingress-byte-stride", row.Key),
            Some: stride => elements > long.MaxValue / stride
                ? TensorReason.ExtentOverflow.Fail<long>("ingress-volume-overflow", row.Key)
                : Fin.Succ(elements * stride));

    // Extent non-negativity and the checked volume product are the SAME sweep the host already vectorizes:
    // `TensorPrimitives.Product<long>` folds the extents and `IsFiniteAll`-style range gating rides the min, so
    // the hand `foreach` under a `catch (OverflowException)` — an exception used as an arithmetic branch — is gone.
    private static Fin<(long[] Shape, long Volume)> Shape(ReadOnlySpan<long> shape) {
        long[] admitted = shape.ToArray();
        return admitted.Length > 0 && TensorPrimitives.Min<long>(admitted) < 0
            ? TensorReason.ShapeMismatch.Fail<(long[], long)>("ingress-shape", TensorPrimitives.Min<long>(admitted).ToString(CultureInfo.InvariantCulture))
            : Op.Of(name: "ingress-volume-overflow").Catch(() => Fin.Succ((admitted, admitted.Length == 0 ? 1L : checked(TensorPrimitives.Product<long>(admitted)))));
    }

    private static Fin<OrtValue> Minted(Func<OrtValue> mint) =>
        Op.Of(name: "ingress-rejected").Catch(() => Fin.Succ(mint()));

    // ONE admitted-dtype gate both projections take: element identity, native byte count against the
    // destination, and the strided-quantized refusal are one decision spelled once, so the two overloads differ
    // only in the copy arm the destination's own shape forces.
    private static Fin<TensorDtype> Projected<T>(OrtValue value, long destinationElements, bool dense) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            row.ElementCount(value.GetTensorSizeInBytes()).Bind(count =>
                count != destinationElements ? TensorReason.ShapeMismatch.Fail<TensorDtype>("egress-undersized", row.Key, $"{count}!={destinationElements}")
                : row.Clr != typeof(T) ? TensorReason.DtypeMismatch.Fail<TensorDtype>("egress-dtype", row.Key, typeof(T).Name)
                : row.Quantized && !dense ? TensorReason.QuantizationInvalid.Fail<TensorDtype>("egress-strided-quantized", row.Key)
                : Fin.Succ(row)));

    // Ref-struct destinations cross no lambda, so the projection body is the named REF_SAFE statement seam:
    // admission stays on the rail, the copy runs in place, and a native rejection converts once.
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
        return Op.Of(name: "egress-rejected").Catch(() => { value.GetTensorDataAsSpan<T>().CopyTo(sink); return Fin.Succ(unit); });
    }

    // The ONE receipted crossing, and the producer `Runtime/receipts#FOLD_PROJECTIONS` `ReceiptFolds.Crossings`
    // folds: it brackets a copy that genuinely MOVES bytes, classifies the gate off the value's own descriptor,
    // measures the span, and projects one `ComputeReceipt.Copy`. A bare stamp that minted a `CopyPoint` nobody
    // projected left that fold reading an empty stream while every crossing on this page ran unrecorded — the
    // census existed, correct and unfed. A crossing that WRAPS without copying stamps nothing, because a
    // zero-byte copy row inflates the very census the fold exists to read.
    public static Fin<(A Result, ComputeReceipt.Copy Receipt)> Crossing<A>(
        OrtValue value, WorkLane lane, IClock clock, CorrelationId correlation, Func<Fin<A>> copy) {
        Instant opened = clock.GetCurrentInstant();
        OrtMemoryInfo memory = value.GetTensorMemoryInfo();
        return copy().Map(result => (result,
            new CopyPoint(OrtResidency.Classify(memory), value.GetTensorSizeInBytes(), memory.Name, opened, correlation)
                .Receipt(lane, clock.GetCurrentInstant() - opened)));
    }

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
    private Option<OrtValue> owned;
    private FlowState state = new FlowState.Live();
    private bool disposed;

    private BoundFlow(InferenceSession session, OrtIoBinding binding, RunOptions run, OrtAllocator arena, TensorDtype row, string inputName, string outputName, OrtValue bound, OrtValue sink) =>
        (this.session, this.binding, this.run, this.arena, this.row, this.inputName, this.outputName, this.bound, this.sink, owned) =
            (session, binding, run, arena, row, inputName, outputName, bound, sink, None);

    public FlowState State => state;

    // Leak-safe acquisition composes the kernel `Rasm/Domain/rails#RESOURCE_RAIL` acquire-chain algebra: the
    // four native handles acquire in order into locals the failure arm hands to `Custody.Rollback`, which
    // releases them LIFO and skips the nulls a half-built chain leaves for the handles it never reached, while
    // the success arm TRANSFERS custody into the returned capsule that owns disposal from then on. That is
    // exactly the discriminant a hand-written reverse-order catch block re-derived per site, and the kernel's
    // version also aggregates a disposer fault into the primary rather than losing it behind the lease refusal.
    public static Fin<BoundFlow> Lease(InferenceSession session, string inputName, string outputName, long[] shape, OrtAllocator arena, TensorDtype row) {
        OrtValue? bound = null, sink = null;
        RunOptions? options = null;
        OrtIoBinding? binding = null;
        return Op.Of(name: "lease-rejected").Catch(() => {
                bound = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
                sink = OrtValue.CreateAllocatedTensorValue(arena, row.Element, shape);
                options = new RunOptions();
                binding = session.CreateIoBinding();
                binding.BindInput(inputName, bound);
                binding.BindOutput(outputName, sink);
                return Fin.Succ(new BoundFlow(session, binding, options, arena, row, inputName, outputName, bound, sink));
            })
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

    // Native drive and output collection throw, so both cross the rail exactly like `Flow` does: an announced
    // `Succ` holding an escaping ORT exception is the one shape this capsule never publishes. `Outputs()` is
    // deleted — it duplicated this member's native call under a second slug and no caller reached it.
    public Fin<IDisposableReadOnlyCollection<OrtValue>> Run(RunOptions options) =>
        Refused<IDisposableReadOnlyCollection<OrtValue>>().Bind(_ =>
            Op.Of(name: "bound-run").Catch(() => { Drive(options); return Fin.Succ(binding.GetOutputValues()); }));

    public Fin<Unit> Flow<T>(ReadOnlySpan<T> input, in TensorSpan<T> output) where T : unmanaged {
        Fin<Unit> written = Write(input);
        if (written.Case is not Unit) { return written; }
        Fin<Unit> driven = Op.Of(name: "bound-run").Catch(() => { Drive(run); return Fin.Succ(unit); });
        return driven.Case is not Unit ? driven : TensorBridge.Egress(sink, output);
    }

    // The steady-state chain: this capsule's device-resident sink relays whole into the next capsule's bound
    // input, so a two-model pipeline never crosses device→host→device between links. `Relay` proves both ends.
    public Fin<Unit> Chain(DeviceMemory device, BoundFlow next, Op key) =>
        Refused().Bind(_ => next.Refused()).Bind(_ => TensorBridge.Relay(device, sink, next.bound, key));

    // ONE rebind over the backing the caller HANDS IN. Replacements allocate before the current bindings clear,
    // the swap publishes only after every native call succeeded, and the capsule releases the prior value only
    // when the case it replaces was capsule-owned.
    public Fin<Unit> Rebind(BindingSource source) =>
        Refused().Bind(_ => Next(source).Bind(next => Op.Of(name: "rebind-rejected").Catch(() => {
                binding.ClearBoundInputs();
                binding.ClearBoundOutputs();
                next.Bind(binding, inputName, outputName);
                return Fin.Succ(unit);
            })
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
        Op.Of(name: "rebind-mint").Catch(() => Fin.Succ((
            OrtValue.CreateAllocatedTensorValue(s.Arena, s.Row.Element, shape),
            OrtValue.CreateAllocatedTensorValue(s.Arena, s.Row.Element, shape))));

    private void Adopt(BindingSource source, Bound next) {
        OrtValue priorBound = bound, priorSink = sink;
        Option<OrtValue> priorOwned = owned;
        (bound, sink) = (next.Input, next.Output);
        owned = source.ReleaseOwner == ReleaseOwner.Capsule ? Some(next.Input) : None;
        priorOwned.Iter(static value => value.Dispose());
        if (source.ReleaseOwner == ReleaseOwner.Capsule) { priorBound.Dispose(); priorSink.Dispose(); }
    }

    // A restore that itself fails poisons the capsule, because the binding table then holds a state no code can
    // name; every later call refuses against that state instead of publishing a result from an unknown binding.
    private Fin<Unit> Restore(Error cause) =>
        Op.Of(name: "rebind-restore").Catch(() => {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(inputName, bound);
            binding.BindOutput(outputName, sink);
            return Fin<Unit>.Fail(cause);
        }).MapFail(restore => { state = new FlowState.Poisoned(restore); return restore; });

    private Fin<Unit> Refused() => Refused<Unit>().Map(static _ => unit);

    private Fin<A> Refused<A>() => state.Switch(
        live: static _ => Fin.Succ<A>(default!),
        poisoned: static p => Fin.Fail<A>(p.Cause));

    private void Drive(RunOptions options) {
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(options, binding);
        binding.SynchronizeBoundOutputs();
    }

    // The three binding shapes the ORT binder itself distinguishes, so the rebind body binds through one value
    // rather than four bodies each re-spelling the clear-then-bind protocol.
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
- Receipt: the kernel `EncodedGeometry.Witness` is the lossless-round-trip proof, its `ContentHash` keyed by the witness's own `DigestRoot` — the `Spatial/reconciliation#RECONCILIATION_BRIDGE` source digest on an `Encode.Apply` mint, the packed-payload digest on an `Encode.Of` raw-lane mint (the interchange arenas), so a consumer keying dedup on the hash reads `Root` before comparing; `Of` admits only a lossless payload, so the residency wrap carries no second witness and mints no second content key.
- Packages: Rasm (project), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new representation is one kernel `PackKind` row at the `Rasm/Drawing/pack#ENCODING_BAND` owner, and the generated total `Switch` in `Wire` BREAKS until this page answers it — where a `FrozenDictionary` mirror let `gaussian-splat` sit unrepresented while a prose line asserted the table was complete and a splat geometry faulted at runtime instead of at build; a new feature channel is one kernel `EncodingChannel` row, read here through the descriptor set with zero residency edit; a new free dimension is one `FreeAxis` row carrying its own derivation; zero new surface.
- Boundary: geometry channel materialization remains in `Rasm.Drawing.Encode.Apply`; residency receives host-neutral `EncodedGeometry` and holds it whole. The wire projection is the kernel roster's own generated `Switch` rather than a dictionary keyed on that roster, so representation identity is TYPE identity — the same law `Rasm.AppHost/Sandbox/solver#SOLVER_KIND` states from the other end, where `SolverKind` rows carry `Input`/`Output` columns speaking `PackKind` directly and the mirrored `EncodingKind` roster that once sat beside them is the deleted form. The `Field` (`geodesic`+`weight` lanes, positions omitted because the witness digest binds the source mesh), `Toolpath` (`position`+`arc-center`+`arc-sense`+`weight`, so an analytic arc survives packing as content rather than sampled chords), and `GaussianSplat` (`scale`+`rotation`+`harmonic` beside position and colour) rows are landed this way on `NxC`, never a residency-side packer. Splat residency has TWO carriers by concern, not by drift: `Runtime/payload#RESIDENCY_PAYLOAD` `ResidencyPayload` is the render-streaming carrier `Rasm.AppUi/Render/reality` consumes, and `EncodedTensor` is the model-input carrier — a splat reaching an ONNX model crosses here, a splat reaching a rasteriser crosses there, and neither wraps the other. `EncodedTensor.Channel` returns the admitted zero-copy `ReadOnlyMemory<byte>` slice at the channel's STORED width, never a default ref-struct ghost and never a float re-typing of a dtype-strided arena, whose float16 and unorm8 lanes such a reinterpretation reads as garbage; descriptor tiling, extent, and offset are proved once by the kernel's own `EncodedGeometry.IsValid` claim set, so this wrap re-derives no range check, and geometry reads resolve BY DESCRIPTOR through that arena rather than by a named column or an assumed stride. `ToTensor` widens through `ChannelDtype.Unpack` into ONE granted plane and interleaves channel-blocked SoA into point-major `[Count, FeatureWidth]` through `Span2D<float>` row addressing — a jagged index walk over a flat buffer computed the same offsets by hand on every element; `Tensor/layout#LAYOUT_ALGEBRA` owns later rank edits. Free-dimension rows feed `AddFreeDimensionOverrideByName`. `U`/`V` and `H`/`W` never derive by assigning the same flat `Count` to both axes — the `FreeAxis` row for each answers absence, and derivability is therefore a fact the roster computes rather than a `bool` column a caller trusts. The `BrepPatch` `NurbsControlTensor` row carries a control net whose semantic authority is the kernel `Rasm/Parametric/nurbs#NURBS_ENGINE` `Nurbs.Of` admission — homogeneous SoA columns, strictly positive weights, normalized knots — so any quantization of that lane must re-admit through that gate, never a residency-local judgement.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The free-dimension vocabulary carries its OWN derivation, so the axis names are spelled once instead of twice
// — once as a `Seq<string>` on each wire row and again as a chain of `name == "C"` string tests — and a row's
// derivability is COMPUTED from whether its axes answer rather than asserted by a `bool` beside them.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FreeAxis {
    public static readonly FreeAxis N = new("N", derive: static (g, _) => Some((long)g.Count));
    public static readonly FreeAxis V = new("V", derive: static (g, _) => Some((long)g.Count));
    public static readonly FreeAxis C = new("C", derive: static (g, _) => Some(g.Descriptors.Sum(static d => (long)d.Channel.Arity)));
    public static readonly FreeAxis F = new("F", derive: static (_, indices) => indices.Map(static topology => (long)topology.Lengths[0]));
    // A control net's `U`/`V` and a voxel grid's `H`/`W` are extents no channel arena carries, so these rows
    // answer absence and the explicit-extents entry is the only route that serves them.
    public static readonly FreeAxis U = new("U", derive: static (_, _) => None);
    public static readonly FreeAxis W = new("W", derive: static (_, _) => None);
    public static readonly FreeAxis H = new("H", derive: static (_, _) => None);

    [UseDelegateFromConstructor]
    public partial Option<long> Derive(EncodedGeometry geometry, Option<Tensor<long>> indices);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct WireRow(LayoutForm Layout, string WireShape, Seq<FreeAxis> Axes) {
    // Derivability is a CONSEQUENCE of the axes, never a column: a row every axis answers derives, and a row
    // carrying one silent axis does not. The bool that stated it separately could disagree with its own rows.
    public bool Derivable(EncodedGeometry geometry, Option<Tensor<long>> indices) =>
        Axes.ForAll(axis => axis.Derive(geometry, indices).IsSome);

    // The kernel roster's own generated total `Switch`: an eighth `PackKind` row breaks THIS expression at
    // compile time, where a `FrozenDictionary` over the same roster answered a runtime miss and let the seventh
    // row sit unmirrored behind a prose claim that the table was one-to-one.
    public static WireRow Of(PackKind kind) => kind.Switch(
        pointCloud:    static _ => new WireRow(LayoutForm.NxC, "PointCloudTensor", Seq(FreeAxis.N, FreeAxis.C)),
        meshPatch:     static _ => new WireRow(LayoutForm.VertexFace, "MeshTensor", Seq(FreeAxis.V, FreeAxis.F)),
        voxelGrid:     static _ => new WireRow(LayoutForm.Nchw, "VoxelGridTensor", Seq(FreeAxis.C, FreeAxis.H, FreeAxis.W)),
        brepPatch:     static _ => new WireRow(LayoutForm.NxC, "NurbsControlTensor", Seq(FreeAxis.U, FreeAxis.V)),
        field:         static _ => new WireRow(LayoutForm.NxC, "FieldTensor", Seq(FreeAxis.N, FreeAxis.C)),
        toolpath:      static _ => new WireRow(LayoutForm.NxC, "ToolpathTensor", Seq(FreeAxis.N, FreeAxis.C)),
        gaussianSplat: static _ => new WireRow(LayoutForm.NxC, "GaussianSplatTensor", Seq(FreeAxis.N, FreeAxis.C)));
}

// Source is the WHOLE kernel carrier, never a destructured payload triple: the arena is dtype-STRIDED, so a
// payload re-typed to float reads a float16 curvature lane or a unorm8 colour lane as garbage, and the byte
// offsets a descriptor carries only address that arena. Every read here therefore composes the kernel's own
// dtype-dispatched `Channel`/`View<T>` readers, which gate width against the descriptor's own row.
public sealed record EncodedTensor(
    EncodedGeometry Source,
    LayoutForm Layout,
    string WireShape,
    Seq<(FreeAxis Axis, long Extent)> FreeDimensions,
    Option<Tensor<long>> Indices) {

    public Seq<EncodingChannelDescriptor> Descriptors => Source.Descriptors;

    public int Count => Source.Count;

    public int FeatureWidth => Descriptors.Sum(static descriptor => descriptor.Channel.Arity);

    // The shape a rebind binds this payload at, so the model-lane consumer reads one value rather than
    // re-projecting the free-dimension pairs at every call site that needs extents.
    public long[] WireExtents => FreeDimensions.Map(static pair => pair.Extent).ToArray();

    // The derive-only entry refuses a row carrying a silent axis BY NAME and points at the four-argument entry
    // that takes the extents explicitly; falling through would refuse anyway, one axis at a time, as if the
    // caller had supplied a bad grid rather than reached for an entry the row cannot serve.
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

    // Raw stored bytes at the channel's own width — an empty span answers an inactive channel, exactly as the
    // kernel reader defines. A model lane wanting floats takes `ToTensor`, which widens through the dtype row.
    public Fin<ReadOnlyMemory<byte>> Channel(EncodingChannel channel) =>
        Source.Channel(channel) is { IsEmpty: false } stored
            ? Fin.Succ(stored)
            : TensorReason.RowMissing.Fail<ReadOnlyMemory<byte>>("channel-miss", channel.Key);

    // Model ingress is the ONE widening seam. Every channel admits through this row's OWN `Channel` gate before a
    // byte moves: an inactive channel answers an EMPTY span, `Unpack` then writes nothing, and the interleave copies
    // whatever the previous descriptor left in the shared staging lane under this channel's columns — one channel's
    // values silently transcribed onto another. The whole descriptor set therefore admits first, so a miss rails
    // ahead of the staging rent rather than landing as plausible feature data.
    public Fin<Tensor<float>> ToTensor(AllocationRequest staging) =>
        Count <= 0 || FeatureWidth <= 0
            ? TensorReason.ShapeMismatch.Fail<Tensor<float>>("encoding-shape", $"{Count}x{FeatureWidth}")
            : Descriptors.TraverseM(descriptor => Channel(descriptor.Channel)).As().Bind(admitted => Interleaved(admitted, staging));

    // `ChannelDtype.Unpack` is the kernel's own quantization inverse, so float16 and unorm8 lanes restore through
    // the row that packed them rather than a second conversion policy here. BOTH planes are granted rents on one
    // admission edge — the destination sized by the caller's own channel arity is staging, not kernel scratch —
    // and the point-major write addresses `Span2D<float>` rows, so the AoS offset is the plane's own row span
    // rather than an index expression re-derived per element beside a raw array the same method also rented.
    private Fin<Tensor<float>> Interleaved(Seq<ReadOnlyMemory<byte>> admitted, AllocationRequest staging) {
        int width = FeatureWidth, widest = Descriptors.Max(static d => d.Channel.Arity);
        return Op.Of(name: "encoding-volume").Catch(() => Fin.Succ((
                Plane: checked(Count * width), Lane: checked(Count * widest))))
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
                                descriptor.Dtype.Unpack(admitted[index].Span, lane);
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

    // Per-axis derivation reads each row's OWN delegate, so `C` from the channel arity sum, `V`/`N` from the
    // element count, and `F` from the supplied face-index topology are one fold — a `VertexFace` layout with no
    // indices answers absence at the `F` row rather than silently equating the face count to the vertex count.
    private static Fin<Seq<(FreeAxis Axis, long Extent)>> Derived(WireRow row, EncodedGeometry geometry, Option<Tensor<long>> indices) =>
        row.Axes.Map<Fin<(FreeAxis Axis, long Extent)>>(axis => axis.Derive(geometry, indices).Match(
                Some: extent => Fin.Succ((axis, extent)),
                None: () => TensorReason.AxisUnderivable.Fail<(FreeAxis, long)>("free-dimension-underivable", axis.Key)))
            .TraverseM(identity).As();
}
```

## [04]-[RESEARCH]

(none)
