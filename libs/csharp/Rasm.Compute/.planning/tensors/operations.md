# [COMPUTE_OPERATIONS]

The cpu-tensor execution vocabulary, the ONNX C-data residency bridge, and the operation algebra: `Tensor<T>` spans and factories as the only tensor shapes, one dtype map between `TensorElementType` and CLR carriers carrying the ONNX byte-width and quantization columns, one `OrtResidency` lattice over the five copy gates (managed-span → memory-backed value → device residency → output value → span view) binding each `OrtValue` C-data factory and projection member as the zero-copy tensor-flow boundary, one `LayoutForm` algebra over the layout member surface with the `ReshapeOp` request union owning the whole shape-edit family under one total dispatch whose plural-result verb is a `Traverse` arm of the same entrypoint, geometry-to-tensor encoding rows as the canonical geometry-ML input vocabulary with their packing kernels as declared boundary capsules, one `TensorOpFamily` table of one-hundred-seven rows over twelve `TensorOpKind` rows under the closed `ToleranceClass` band, the arity kernel-delegate dispatch binding each row to its TensorPrimitives member, the claim-gated `ParallelHelper` partition column over both the 1-D `For` and the 2-D `For2D` plane partitions, and the equivalence law proving lane kernels against Rasm baselines. The page owns the `TensorDtype`, `OrtResidency`, `LayoutForm`, `ReshapeOp`, `EncodingChannel`, `GeometryEncoding`, `TensorOpKind`, `TensorOpFamily`, and `ToleranceClass` axes, the `TensorKeyPolicy` accessor, the `TensorBridge` C-data capsule, and the kernel registries; AppHost `ClockPolicy`, `CorrelationId`, and `CpuBudget` arrive settled, the matrix and structural rows lower through `numeric#KERNEL_LOWERING`, the model-session lifecycle rides `models#INFERENCE_MODES`, and the substrate row, fault union, and receipt cases ride their owning pages.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                |
| :-----: | ------------------- | --------------------------------------------------------------------- |
|   [1]   | TENSOR_VOCABULARY   | Tensor shapes, factories, dtype map, ONNX byte-width, quantization policy |
|   [2]   | ORT_BRIDGE          | OrtResidency lattice; OrtValue C-data factories; zero-copy ingress/egress; IoBinding loop |
|   [3]   | LAYOUT_ALGEBRA      | LayoutForm rows; permute table; ReshapeOp request union over the layout family |
|   [4]   | GEOMETRY_ENCODING   | Geometry-to-tensor cases; packing capsules; free-dimension names; wire-shape rows |
|   [5]   | OPERATION_TABLE     | TensorOpKind/TensorOpFamily/ToleranceClass vocabulary rows            |
|   [6]   | KERNEL_DISPATCH     | Arity kernel-delegate tables; one TensorOps dispatch surface          |
|   [7]   | EQUIVALENCE_INTEROP | Equivalence proofs against Rasm kernels; matmul route; differentiable adjoint; copy-point law |

## [2]-[TENSOR_VOCABULARY]

- Owner: `TensorDtype`
- Cases: float32, float64, float16, bfloat16, int8, uint8, int32, int64, bool, string
- Entry: `public static Fin<TensorDtype> Admit(TensorElementType element)` — `Fin<T>` aborts on an unmapped element; the quantization overload rejects scale/zero-point values on unquantized rows; the `OrtByteSpan` fold sizes a `Span<byte>` egress destination from `GetTensorSizeInBytes` against the row width, never re-multiplied dimensions.
- Packages: System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element mapping is one `TensorDtype` row; the byte-width and quantization columns derive from the row; zero new surface.
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are the deleted phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans plus `Tensor.Create` over rented `MemoryOwner<T>` arrays through the `DangerousGetArray` seam; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime is the caller's proof obligation, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` the ref roots; one generic kernel serves each operation family — per-dtype kernel copies are the deleted form; the `Width` column carries the CLR byte width and `OrtElementBytes` the ONNX C-data byte stride (`bfloat16`/`float16` are two-byte even though the CLR carrier widens), so a `GetTensorSizeInBytes` destination sizes from the dtype row, never `sizeof(T)`; quantized rows project through `ConvertSaturating` with `QuantizationPolicy` values; a chunked tensor frame requiring one contiguous backing stages through the `staging#ALLOCATION_AXIS` contiguous-frame route (the `asContiguousBuffer:true` `GetStream` overload), never a hand-rolled array concatenation; the string row admits only at the model boundary for tokenizer extension ops through `OrtValue.CreateTensorWithEmptyStrings` then `CreateFromStringTensor`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype Float32 = new("float32", TensorElementType.Float, typeof(float), width: Some(4), ortBytes: 4, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Float64 = new("float64", TensorElementType.Double, typeof(double), width: Some(8), ortBytes: 8, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Float16 = new("float16", TensorElementType.Float16, typeof(Half), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype BFloat16 = new("bfloat16", TensorElementType.BFloat16, typeof(Microsoft.ML.OnnxRuntime.BFloat16), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: true);
    public static readonly TensorDtype Int8 = new("int8", TensorElementType.Int8, typeof(sbyte), width: Some(1), ortBytes: 1, quantized: true, modelBoundaryOnly: false);
    public static readonly TensorDtype UInt8 = new("uint8", TensorElementType.UInt8, typeof(byte), width: Some(1), ortBytes: 1, quantized: true, modelBoundaryOnly: false);
    public static readonly TensorDtype Int32 = new("int32", TensorElementType.Int32, typeof(int), width: Some(4), ortBytes: 4, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Int64 = new("int64", TensorElementType.Int64, typeof(long), width: Some(8), ortBytes: 8, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Bool = new("bool", TensorElementType.Bool, typeof(bool), width: Some(1), ortBytes: 1, quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Utf8Text = new("string", TensorElementType.String, typeof(string), width: None, ortBytes: 0, quantized: false, modelBoundaryOnly: true);

    public TensorElementType Element { get; }
    public Type Clr { get; }
    public Option<int> Width { get; }
    public int OrtElementBytes { get; }
    public bool Quantized { get; }
    public bool ModelBoundaryOnly { get; }

    public Option<long> ElementCount(long sizeInBytes) =>
        OrtElementBytes > 0 ? Some(sizeInBytes / OrtElementBytes) : None;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct QuantizationPolicy(double Scale, int ZeroPoint);

// --- [ERRORS] ------------------------------------------------------------------------------
// TensorFault: the one bracketed-symbol fault row family the lane raises, keyed off the offending row/op key so every
// call site lifts <symbol:key> through ComputeFault without restating the bracket convention per site.
public static class TensorFault {
    public static Error Symbol(string symbol, string key) => ComputeFault.Create($"<{symbol}:{key}>");
    public static Error Symbol(string symbol, string key, string detail) => ComputeFault.Create($"<{symbol}:{key}:{detail}>");
    public static Fin<A> Fail<A>(string symbol, string key) => Fin.Fail<A>(Symbol(symbol, key));
    public static Fin<A> Fail<A>(string symbol, string key, string detail) => Fin.Fail<A>(Symbol(symbol, key, detail));
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class TensorKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorVocabulary {
    private static readonly FrozenDictionary<TensorElementType, TensorDtype> ByElement =
        TensorDtype.Items.ToFrozenDictionary(static row => row.Element, static row => row);

    public static Fin<TensorDtype> Admit(TensorElementType element) =>
        ByElement.TryGetValue(element, out TensorDtype? row) ? Fin.Succ(row!) : TensorFault.Fail<TensorDtype>("unmapped-element", element.ToString());

    public static Fin<TensorDtype> Admit(TensorElementType element, Option<QuantizationPolicy> quantization) =>
        Admit(element).Bind(row => quantization.IsSome && !row.Quantized ? TensorFault.Fail<TensorDtype>("quantization-on-unquantized-row", row.Key) : Fin.Succ(row));

    // OrtByteSpan: the one length-sized egress destination derived from the native byte count and the row's stride,
    // closing the [INFERENCE_MODES] "size from GetTensorTypeAndShape().ElementCount, never re-multiplied dimensions" law.
    public static Fin<int> OrtByteSpan(TensorDtype row, long sizeInBytes) =>
        row.ElementCount(sizeInBytes).Match(
            Some: count => Fin.Succ(checked((int)count)),
            None: () => TensorFault.Fail<int>("no-byte-stride", row.Key));
}
```

## [3]-[ORT_BRIDGE]

- Owner: `TensorBridge` (boundary capsule); `OrtResidency` `[SmartEnum<string>]` over the five copy gates; `BoundFlow` the `OrtIoBinding` steady-state tensor-flow capsule.
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source, OrtResidency gate)` / the pooled-backing, foreign-pointer, and device-allocator overloads / `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` / `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape)` — `Fin<T>` aborts when the residency gate forbids the carrier or the egress destination is undersized; the bound flow is a disposable capsule leasing one `OrtIoBinding` over pinned input and output values with a `ClearBound*` rebind path for a shape-class transition.
- Receipt: every ingress and egress materializes the named copy point as `TensorRun` receipt evidence at the sink edge — the gate symbol, the byte count from `GetTensorSizeInBytes`, and the residency device from `GetTensorMemoryInfo` — keyed by `CorrelationId`, never payload bytes.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new accelerator is one `OrtResidency` row plus its `OrtValue` factory column; a new copy point is one residency gate, never a per-call marshal helper; zero new surface.
- Boundary: `OrtValue` is the sole model-boundary carrier and `TensorBridge` owns the C-data residency lattice the `models#INFERENCE_MODES` `VALUE_RESIDENCY` law declares — the lattice runs managed-span → memory-backed value → device residency → output value → span view, the residency gate value (`Wraps`/`Device`/`ProjectsInPlace`) selecting the ingress and egress factory by data; per-call dictionary marshal and managed dense-tensor copies are the deleted rows; `NamedOnnxValue`, `DisposableNamedOnnxValue.CreateFromOrtValue`, and `FixedBufferOnnxValue` are the superseded spellings the OrtValue-only law deletes. The four ingress polarities ride the residency discriminant: a `Wraps` non-device gate admits the `System.Numerics` tensor owner directly through `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)` and pins a rented `MemoryOwner<T>` backing through `CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])`; the `Device` gate admits a ref-rooted foreign pointer through `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` over a `TensorMarshal.GetReference`/`Tensor<T>.GetPinnableReference` root (the pooled-plane, model-output, device-buffer admission path the `DeviceResident` row owns) and allocates a device sink through `CreateAllocatedTensorValue(OrtAllocator, TensorElementType, long[])` against an `OrtEnv.Instance().CreateSharedAllocator(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, options)` allocation over the extended `OrtMemoryInfo(string, OrtMemoryInfoDeviceType, uint vendorId, int deviceId, OrtDeviceMemoryType, ulong alignment, OrtAllocatorType)` descriptor — `OrtMemoryInfo.DefaultInstance` is the CPU residency descriptor. The backing must outlive the value and every run binding it, the value's dispose IS the owner's release point, and freeing under a live value is a use-after-free in managed code that no allocation profiler attributes. Egress is projection, never copy, and the dtype-width column selects the egress polarity: an IEEE-754 row projects in place through `GetTensorDataAsTensorSpan<T>()` (`ReadOnlyTensorSpan<T>`, read polarity) and `GetTensorMutableDataAsTensorSpan<T>()` (`TensorSpan<T>`, write polarity, same-start aliasing law), while a quantized/sub-stride row (int8/uint8/bool/string) projects through the flat-span `GetTensorDataAsSpan<T>()`/`GetTensorMutableDataAsSpan<T>()` polarities and the raw-byte `GetTensorMutableRawData()` (`Span<byte>`) for the byte-stride egress no `TensorSpan<T>` widening serves; the egress destination sizes from `GetTensorSizeInBytes` through `TensorVocabulary.OrtByteSpan`, never re-multiplied dimensions; result collections are deterministic-dispose native material invisible to GC heap heuristics — one dispose releases every element, a leaked collection a native leak. The `BoundFlow` capsule is the steady-state row for repeated same-shape tensor flow — `BindInput`/`BindOutput` once, `RunWithBinding` per call with zero marshal, the four zero-allocation columns declared together (binding, pinned shapes, `EnableMemoryPattern`, pre-allocated `CreateAllocatedTensorValue` outputs); `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` bracket every run unconditionally (no-ops on CPU, stream fences on device), a shape-class transition rebinds through `ClearBoundInputs`/`ClearBoundOutputs` against a freshly leased plane, the steady-state multi-output read rides `GetOutputValues()` (`IDisposableReadOnlyCollection<OrtValue>`), `BindOutputToDevice(string, OrtMemoryInfo)` chains a device-resident output with no host round trip, and the allocate-per-call spelling turns tensor flow into a GC workload. The string-tensor copy point rides `CreateTensorWithEmptyStrings` then `CreateFromStringTensor`, the one legal string copy.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class OrtResidency {
    public static readonly OrtResidency ManagedSpan = new("managed-span", wraps: true, device: false, projectsInPlace: false);
    public static readonly OrtResidency MemoryBacked = new("memory-backed", wraps: true, device: false, projectsInPlace: true);
    public static readonly OrtResidency DeviceResident = new("device-resident", wraps: false, device: true, projectsInPlace: false);
    public static readonly OrtResidency OutputValue = new("output-value", wraps: false, device: false, projectsInPlace: true);
    public static readonly OrtResidency SpanView = new("span-view", wraps: true, device: false, projectsInPlace: true);

    public bool Wraps { get; }
    public bool Device { get; }
    public bool ProjectsInPlace { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct CopyPoint(OrtResidency Gate, long Bytes, string Device, Instant At, CorrelationId Correlation);

public readonly record struct DeviceMemory(OrtEpDevice Device, OrtDeviceMemoryType MemoryType, OrtAllocatorType AllocatorType, ulong Alignment);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorBridge {
    // Ingress: the residency-gate discriminant selects the OrtValue factory by data — a wrapping non-device gate admits
    // the tensor owner directly, the device gate routes its pointer/allocator overloads, a non-wrap non-device gate faults.
    public static Fin<OrtValue> Ingress<T>(Tensor<T> source, OrtResidency gate) where T : unmanaged =>
        gate.Device
            ? TensorFault.Fail<OrtValue>("device-residency-needs-pointer", gate.Key)
        : gate.Wraps
            ? Fin.Succ(OrtValue.CreateTensorValueFromSystemNumericsTensorObject(source))
            : TensorFault.Fail<OrtValue>("residency-forbids-wrap", gate.Key);

    // Ingress over a pinned pooled backing: CreateTensorValueFromMemory pins the rented MemoryOwner for the value's
    // lifetime; the owner's Dispose sequences AFTER the value's, never before, or the run reads freed memory.
    public static Fin<OrtValue> Ingress<T>(MemoryOwner<T> backing, ReadOnlySpan<long> shape) where T : unmanaged =>
        Fin.Succ(OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, backing.Memory, shape.ToArray()));

    // Ingress over a ref-rooted foreign pointer: the DeviceResident path. The caller's nint roots a device/pooled/output
    // buffer whose lifetime is the proof obligation; CreateTensorValueWithData wraps it against the descriptor, no copy.
    public static Fin<OrtValue> Ingress(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, nint data, long sizeInBytes) =>
        Fin.Succ(OrtValue.CreateTensorValueWithData(memory, row.Element, shape.ToArray(), data, sizeInBytes));

    // DeviceSink: the pre-allocated device output value over a shared allocator. CreateSharedAllocator leases the
    // device allocation against the extended OrtMemoryInfo descriptor; CreateAllocatedTensorValue is the output sink.
    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> DeviceSink(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        Fin.Succ(OrtEnv.Instance().CreateSharedAllocator(device.Device, device.MemoryType, device.AllocatorType, options: null))
            .Map(allocator => (allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape.ToArray())));

    // Egress: the dtype-width column selects the polarity — an IEEE-754 row projects the TensorSpan in place, a quantized
    // row projects the raw byte/flat span; the destination sizes from the native byte count, same-start aliasing gated.
    public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count != destination.FlattenedLength
                    ? TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.FlattenedLength}")
                : row.Quantized
                    ? Effects.ToFin(() => value.GetTensorMutableRawData())
                    : Effects.ToFin(() => value.GetTensorDataAsTensorSpan<T>().CopyTo(destination))));

    // EgressFlat: the flat-span egress for the non-tensor-span carriers (string-output slots, bool masks); reads the
    // native buffer as a flat ReadOnlySpan<T> and copies into the caller plane under the byte-stride size gate.
    public static Fin<Unit> EgressFlat<T>(OrtValue value, Span<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count == destination.Length
                    ? Effects.ToFin(() => value.GetTensorDataAsSpan<T>().CopyTo(destination))
                    : TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.Length}")));

    // CopyPoint: the named copy-point receipt — gate, native byte count, residency device — stamped at the sink edge.
    public static CopyPoint Stamp(OrtValue value, OrtResidency gate, ClockPolicy clocks, CorrelationId correlation) =>
        new(gate, value.GetTensorSizeInBytes(), value.GetTensorMemoryInfo().ToString(), clocks.Now, correlation);

    public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape) =>
        BoundFlow.Lease(session, inputName, outputName, shape.ToArray());
}

// --- [COMPOSITION] -------------------------------------------------------------------------
public sealed class BoundFlow : IDisposable {
    private readonly InferenceSession session;
    private readonly OrtIoBinding binding;
    private readonly RunOptions run;
    private readonly string inputName, outputName;
    private MemoryOwner<float> plane;
    private OrtValue bound, sink;

    private BoundFlow(InferenceSession session, OrtIoBinding binding, RunOptions run, string inputName, string outputName, MemoryOwner<float> plane, OrtValue bound, OrtValue sink) =>
        (this.session, this.binding, this.run, this.inputName, this.outputName, this.plane, this.bound, this.sink) = (session, binding, run, inputName, outputName, plane, bound, sink);

    public static Fin<BoundFlow> Lease(InferenceSession session, string inputName, string outputName, long[] shape) {
        MemoryOwner<float> plane = MemoryOwner<float>.Allocate(checked((int)shape.Aggregate(1L, static (acc, e) => acc * e)), AllocationMode.Clear);
        OrtValue bound = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, plane.Memory, shape);
        OrtValue sink = OrtValue.CreateAllocatedTensorValue(OrtAllocator.DefaultInstance, TensorElementType.Float, shape);
        RunOptions run = new();
        OrtIoBinding binding = session.CreateIoBinding();
        binding.BindInput(inputName, bound);
        binding.BindOutput(outputName, sink);
        return Fin.Succ(new BoundFlow(session, binding, run, inputName, outputName, plane, bound, sink));
    }

    // Flow: the zero-marshal hot path — refresh the pinned input plane by span write, synchronize bound inputs (stream
    // fence on device, no-op on CPU), run against the binding, synchronize bound outputs, project the sink in place.
    public Fin<Unit> Flow(ReadOnlySpan<float> input, in TensorSpan<float> output) {
        input.CopyTo(plane.Span);
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(run, binding);
        binding.SynchronizeBoundOutputs();
        return TensorBridge.Egress(sink, output);
    }

    // Rebind: a shape-class transition clears both bound slots, leases a fresh plane and sink at the new shape, and
    // re-binds; the prior plane/values release before the swap so no stale shape survives the binding.
    public Fin<Unit> Rebind(long[] shape) {
        binding.ClearBoundInputs();
        binding.ClearBoundOutputs();
        bound.Dispose(); sink.Dispose(); plane.Dispose();
        plane = MemoryOwner<float>.Allocate(checked((int)shape.Aggregate(1L, static (acc, e) => acc * e)), AllocationMode.Clear);
        bound = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, plane.Memory, shape);
        sink = OrtValue.CreateAllocatedTensorValue(OrtAllocator.DefaultInstance, TensorElementType.Float, shape);
        binding.BindInput(inputName, bound);
        binding.BindOutput(outputName, sink);
        return Fin.Succ(unit);
    }

    // RebindToDevice: routes the bound output to a device allocator with no host round trip — the chained-stage posture.
    public Fin<Unit> RebindToDevice(OrtMemoryInfo device) {
        binding.ClearBoundOutputs();
        binding.BindOutputToDevice(outputName, device);
        return Fin.Succ(unit);
    }

    // Outputs: the steady-state multi-output read; GetOutputValues returns the bound output set as native material the
    // caller disposes as one collection.
    public IDisposableReadOnlyCollection<OrtValue> Outputs() => binding.GetOutputValues();

    public void Dispose() { run.Dispose(); binding.Dispose(); bound.Dispose(); sink.Dispose(); plane.Dispose(); }
}
```

## [4]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm`; `ReshapeOp` the `[Union]` request family owning the whole shape-edit surface under one total dispatch.
- Cases: dense, nxc, vertex-face, nchw, nhwc — and the reshape verbs `Permute` | `Transpose` | `Squeeze` | `Unsqueeze` | `Reshape` | `Flatten` | `Densify` | `Broadcast` | `Concatenate` | `Stack` | `Split` | `Reverse` | `Resize` | `Slice`.
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` for the named layout transitions; `public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp op)` for the general shape-edit request — every verb is a `Seq` arm of the one entrypoint, the singular verbs yielding a one-element `Seq` and the plural `Split` the `Traverse`-shaped multi-segment arm, so `Fin<T>` aborts on an undeclared permute row or a rank/broadcast-incompatible request and no verb leaves the dispatch as a poison case.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new layout is one `LayoutForm` row plus one permute-table entry; a new shape-edit verb is one `ReshapeOp` case breaking the total dispatch at compile time; zero new surface.
- Boundary: the layout family — `PermuteDimensions`, `Transpose`, `Squeeze`, `SqueezeDimension`, `Unsqueeze`, `SetSlice`, `FilteredUpdate`, `Split`, `Stack`, `StackAlongDimension`, `Concatenate`, `ConcatenateOnDimension`, `Reverse`, `ReverseDimension`, `Resize`, `Broadcast`, `BroadcastTo`, `Reshape`, `FlattenTo`, `ToDenseTensor`, and `Slice` with `NIndex`/`NRange` — is the only layout surface, and the `ReshapeOp` request union collapses every verb into one case per verb under one total `Switch` (a new verb breaks every dispatch site, never a sibling `Permute`/`Reshape`/`Squeeze` method family); `Transpose` is the two-trailing-axis transpose distinct from the explicit-permutation `Permute`, both routing the layout family's reorder members; the nchw↔nhwc permute rows are the mandatory CoreML image-model pre/post route; `Slice` shares storage with adjusted strides through the instance `Tensor<T>.Slice(params ReadOnlySpan<NRange>)` and never copies while `Broadcast` materializes at the broadcast shape (validates, allocates, copies), so a scalar operand rides the scalar-position kernel overload, never a manufactured constant tensor; `Reverse` is the whole-tensor element-order reversal and `ReverseDimension` the per-dimension form (the `Reverse.Dimension` option routing the two), never a dimension argument smuggled onto whole-tensor `Reverse`; `Span2D`/`ReadOnlySpan2D` planes are views over dense backings (carried onto rented buffers via `AsSpan2D`, contiguity-probed via `TryGetSpan`) and never substitute for rank permutation; `Flatten` is the strided-to-linear `FlattenTo` bridge and `Densify` is `ToDenseTensor` which returns `this` on dense input so an independent buffer is `CreateFromShape` plus `CopyTo`, never a defensive copy; broadcast compatibility and rank/stride invariants ride `Broadcast`/`BroadcastTo` and the dimension spans, stated here once for the lane.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class LayoutForm {
    public static readonly LayoutForm Dense = new("dense", rank: 1);
    public static readonly LayoutForm NxC = new("nxc", rank: 2);
    public static readonly LayoutForm VertexFace = new("vertex-face", rank: 2);
    public static readonly LayoutForm Nchw = new("nchw", rank: 4);
    public static readonly LayoutForm Nhwc = new("nhwc", rank: 4);

    public int Rank { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReshapeOp {
    private ReshapeOp() { }

    public sealed record Permute(int[] Axes) : ReshapeOp;
    public sealed record Transpose : ReshapeOp;
    public sealed record Squeeze(Option<int> Dimension) : ReshapeOp;
    public sealed record Unsqueeze(int Dimension) : ReshapeOp;
    public sealed record Reshape(nint[] Lengths) : ReshapeOp;
    public sealed record Flatten(int Start, int Count) : ReshapeOp;
    public sealed record Densify : ReshapeOp;
    public sealed record Broadcast(nint[] Lengths) : ReshapeOp;
    public sealed record Concatenate(Tensor<float>[] Others, int Dimension) : ReshapeOp;
    public sealed record Stack(Tensor<float>[] Others, int Dimension) : ReshapeOp;
    public sealed record Split(int Count, int Dimension) : ReshapeOp;
    public sealed record Reverse(Option<int> Dimension) : ReshapeOp;
    public sealed record Resize(nint[] Lengths) : ReshapeOp;
    public sealed record Slice(NRange[] Ranges) : ReshapeOp;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorLayout {
    private static readonly FrozenDictionary<(LayoutForm Origin, LayoutForm Target), int[]> PermuteRows =
        new Dictionary<(LayoutForm Origin, LayoutForm Target), int[]> {
            [(LayoutForm.Nchw, LayoutForm.Nhwc)] = [0, 2, 3, 1],
            [(LayoutForm.Nhwc, LayoutForm.Nchw)] = [0, 3, 1, 2],
        }.ToFrozenDictionary();

    public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target) =>
        PermuteRows.TryGetValue((origin, target), out int[]? axes)
            ? Fin.Succ(Tensor.PermuteDimensions(source, axes!))
            : TensorFault.Fail<Tensor<T>>("no-permute-row", $"{origin.Key}->{target.Key}");

    // Apply: the one total-dispatch entrypoint over the whole shape-edit family — every verb is a Seq arm, the singular
    // verbs a one-element Seq and Split the multi-segment Traverse arm of THIS entrypoint (MODALITY_FOLD), never a poison case.
    public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp op) where T : unmanaged =>
        op switch {
            ReshapeOp.Permute p => One(Tensor.PermuteDimensions(source, p.Axes)),
            ReshapeOp.Transpose => One(Tensor.Transpose(source)),
            ReshapeOp.Squeeze { Dimension.IsSome: true } s => One(Tensor.SqueezeDimension(source, s.Dimension.IfNone(0))),
            ReshapeOp.Squeeze => One(Tensor.Squeeze(source)),
            ReshapeOp.Unsqueeze u => One(Tensor.Unsqueeze(source, u.Dimension)),
            ReshapeOp.Reshape r => One(Tensor.Reshape(source, r.Lengths)),
            ReshapeOp.Flatten f => One(Flattened(source, f.Start, f.Count)),
            ReshapeOp.Densify => One(source.ToDenseTensor()),
            ReshapeOp.Broadcast b => One(Tensor.BroadcastTo(source, b.Lengths)),
            ReshapeOp.Concatenate c => One(Tensor.ConcatenateOnDimension(c.Dimension, [source, .. c.Others.Cast<Tensor<T>>()])),
            ReshapeOp.Stack k => One(Tensor.StackAlongDimension(k.Dimension, [source, .. k.Others.Cast<Tensor<T>>()])),
            ReshapeOp.Split l => Fin.Succ(toSeq(Tensor.Split(source, l.Count, l.Dimension))),
            ReshapeOp.Reverse { Dimension.IsSome: true } v => One(Tensor.ReverseDimension(source, v.Dimension.IfNone(0))),
            ReshapeOp.Reverse => One(Tensor.Reverse(source)),
            ReshapeOp.Resize z => One(Tensor.Resize(source, z.Lengths)),
            ReshapeOp.Slice l => One(source.Slice(l.Ranges)),
            _ => TensorFault.Fail<Seq<Tensor<T>>>("unhandled-reshape-op", op.GetType().Name),
        };

    // Single: the unitary verb wrapper — every non-Split arm yields exactly one tensor, so the result Seq stays the
    // uniform carrier the next entrypoint discriminates on by Length (one == singular, n == split).
    private static Fin<Seq<Tensor<T>>> One<T>(Tensor<T> tensor) where T : unmanaged => Fin.Succ(Seq1(tensor));

    // Flattened: FlattenTo writes the strided-to-linear view into a rented contiguous plane then re-wraps it as the
    // flattened-extent tensor; the flatten window collapses [Start, Start+Count) into one axis.
    private static Tensor<T> Flattened<T>(Tensor<T> source, int start, int count) where T : unmanaged {
        using MemoryOwner<T> linear = MemoryOwner<T>.Allocate(checked((int)source.FlattenedLength), AllocationMode.Clear);
        source.AsReadOnlyTensorSpan().FlattenTo(linear.Span);
        nint folded = 1;
        for (int d = start; d < start + count; d++) { folded *= source.Lengths[d]; }
        nint[] lengths = [.. source.Lengths[..start], folded, .. source.Lengths[(start + count)..]];
        return Tensor.Create(linear.Span.ToArray(), lengths);
    }
}
```

## [5]-[GEOMETRY_ENCODING]

- Owner: `GeometryEncoding`
- Cases: `PointCloud(VectorCloud, Option<CloudNeighborhoodPcaResult>, Option<ReadOnlyMemory<float>>)` | `MeshPatch(MeshSpace)` | `VoxelGrid(ReadOnlyMemory<float>, Dimension, Dimension, Dimension, VolumeGridPolicy)` | `BrepPatch(ReadOnlyMemory<float>, ReadOnlyMemory<float>, int, int, Dimension, Dimension)`
- Entry: `public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default)` — `Fin<T>` aborts when rank or free-dimension names miss the case row; `public static Fin<EncodedTensor> Pack(GeometryEncoding source, Seq<(string Name, long Extent)> freeDimensions = default)` is the boundary-capsule entry folding host geometry coordinates into the canonical tensor inside the capsule, so host geometry types never enter lane signatures.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new encoding is one `GeometryEncoding` case with its `CaseRow` arm — the `Row` projection (dtype/layout/wire-shape/free-dim names) and the `ChannelRows` projection both derive from the one case-row tuple, so a new encoding adds one case-row arm, not two parallel Switch folds — the `BrepPatch` case carries the NURBS control-point grid, knot vectors, and U/V degrees as the canonical B-rep/NURBS encoding feeding the model and solver lanes; a new feature channel is one `EncodingChannel` row carrying its width column (the `weight` row admits rational NURBS control-point weights and the `occupancy` row admits the bit-packed voxel occupancy plane, closing the axis at eight); zero new surface.
- Boundary: packing kernels are the page's declared boundary capsules beside the union — host geometry coordinate access stays inside `GeometryPacking` and host geometry types never enter lane signatures, the capsule projecting coordinates and the present feature channels into a rented `MemoryOwner<float>` plane through `BitHelper.ExtractRange`/`SetRange` ulong occupancy/material-id words for the voxel row and `Tensor.Create` over the `DangerousGetArray` seam for the contiguous rows; each case's `CaseRow` carries the model-zoo conformance triad — named `WireShape`, `LayoutForm`, declared free-dimension rank — and the `FeatureWidth` it advertises equals the width of the channels the packer materializes, so `Of` rejects a payload whose rank or free-dimension names miss that triad and the packer projects every advertised channel (point-cloud normals and color, voxel occupancy, NURBS weights) into the plane so the packed feature width matches; the free-dimension rows feed the model-lane `AddFreeDimensionOverrideByName` admission and the wire-shape names mirror one-to-one onto the remote-lane proto geometry family; mesh face indices ride the int64 row as `Tensor<long>`; voxel grids ride nchw with z-slices as channel planes and pack occupancy/material-id through the multi-bit `BitHelper.SetRange`/`ExtractRange` words threaded into an occupancy channel, never a `bool[]`; `FeatureWidth` folds the `EncodingChannel` widths present on the payload, where `curvature`, `geodesic`, and `intensity` widen the channel axis as SmartEnum rows — a `PointCloudV2` sibling case is the rejected anticipatory form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class EncodingChannel {
    public static readonly EncodingChannel Position = new("position", width: 3);
    public static readonly EncodingChannel Normal = new("normal", width: 3);
    public static readonly EncodingChannel ColorRgba = new("color-rgba", width: 4);
    public static readonly EncodingChannel Curvature = new("curvature", width: 1);
    public static readonly EncodingChannel Geodesic = new("geodesic", width: 1);
    public static readonly EncodingChannel Intensity = new("intensity", width: 1);
    public static readonly EncodingChannel Occupancy = new("occupancy", width: 1);
    public static readonly EncodingChannel Weight = new("weight", width: 1);

    public int Width { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryEncoding {
    private GeometryEncoding() { }

    public sealed record PointCloud(VectorCloud Source, Option<CloudNeighborhoodPcaResult> Normals, Option<ReadOnlyMemory<float>> Colors) : GeometryEncoding;

    public sealed record MeshPatch(MeshSpace Source) : GeometryEncoding;

    public sealed record VoxelGrid(ReadOnlyMemory<float> Cells, Dimension Channels, Dimension Height, Dimension Width, VolumeGridPolicy Grid) : GeometryEncoding;

    public sealed record BrepPatch(ReadOnlyMemory<float> ControlPoints, ReadOnlyMemory<float> Knots, int UDegree, int VDegree, Dimension UCount, Dimension VCount) : GeometryEncoding;

    // CaseRow: the one per-case projection both Row and ChannelRows derive from. The primary tuple carries the
    // conformance triad and the present channel set; Row reads the triad, ChannelRows reads the channels — one case
    // fold, two derived views (DERIVED_LOGIC), never two parallel Switch arms over the same four cases.
    private (TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames, Seq<EncodingChannel> Channels) CaseRow =>
        Switch(
            pointCloud: static c => (TensorDtype.Float32, LayoutForm.NxC, "PointCloudTensor", Seq("N", "C"),
                Seq1(EncodingChannel.Position)
                    + c.Normals.Map(static _ => Seq(EncodingChannel.Normal, EncodingChannel.Curvature, EncodingChannel.Geodesic)).IfNone(Seq<EncodingChannel>())
                    + c.Colors.Map(static _ => EncodingChannel.ColorRgba).ToSeq()),
            meshPatch: static _ => (TensorDtype.Float32, LayoutForm.VertexFace, "MeshTensor", Seq("V", "F"), Seq1(EncodingChannel.Position)),
            voxelGrid: static _ => (TensorDtype.Float32, LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W"), Seq(EncodingChannel.Intensity, EncodingChannel.Occupancy)),
            brepPatch: static _ => (TensorDtype.Float32, LayoutForm.NxC, "NurbsControlTensor", Seq("U", "V"), Seq(EncodingChannel.Position, EncodingChannel.Weight)));

    public (TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames) Row =>
        CaseRow is var row ? (row.Dtype, row.Layout, row.WireShape, row.FreeDimensionNames) : default;

    public Seq<EncodingChannel> ChannelRows => CaseRow.Channels;

    public int FeatureWidth => ChannelRows.Sum(static channel => channel.Width);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EncodedTensor(Tensor<float> Values, Option<Tensor<long>> Indices, TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<(string Name, long Extent)> FreeDimensions) {
    public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default) =>
        values.Rank == source.Row.Layout.Rank && freeDimensions.Map(static d => d.Name) == source.Row.FreeDimensionNames
            ? Fin.Succ(new EncodedTensor(values, indices, source.Row.Dtype, source.Row.Layout, source.Row.WireShape, freeDimensions))
            : TensorFault.Fail<EncodedTensor>("encoding-shape-miss", source.Row.WireShape);

    // Pack: the boundary-capsule entry. Host geometry coordinates fold to the canonical tensor inside GeometryPacking,
    // never crossing the lane signature, then admit through Of so the conformance triad gate runs on the packed result.
    public static Fin<EncodedTensor> Pack(GeometryEncoding source, Seq<(string Name, long Extent)> freeDimensions = default) =>
        GeometryPacking.Project(source).Bind(packed => Of(source, packed.Values, packed.Indices, freeDimensions));
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// GeometryPacking: the packing boundary capsule. Host coordinate access lives here and nowhere else; it rents a plane,
// projects coordinates and every advertised channel into it, and bit-packs voxel occupancy through BitHelper words.
public static class GeometryPacking {
    public static Fin<(Tensor<float> Values, Option<Tensor<long>> Indices)> Project(GeometryEncoding source) =>
        source.Switch<Fin<(Tensor<float>, Option<Tensor<long>>)>>(
            pointCloud: static c => Pack(c).Map(static t => (t, Option<Tensor<long>>.None)),
            meshPatch: static m => PackMesh(m.Source),
            voxelGrid: static v => PackVoxels(v).Map(static t => (t, Option<Tensor<long>>.None)),
            brepPatch: static b => PackNurbs(b).Map(static t => (t, Option<Tensor<long>>.None)));

    // Voxel occupancy bit-packs through BitHelper SetRange/ExtractRange, and the packed plane is threaded as the second
    // NCHW channel of the rank-4 [1, 2, H, W*C] tensor beside intensity — live data, not discarded words. Statement seam.
    private static Fin<Tensor<float>> PackVoxels(GeometryEncoding.VoxelGrid grid) {
        int cells = (int)grid.Channels * (int)grid.Height * (int)grid.Width;
        using MemoryOwner<ulong> words = MemoryOwner<ulong>.Allocate((cells + 63) / 64, AllocationMode.Clear);
        Span<ulong> bits = words.Span;
        ReadOnlySpan<float> raw = grid.Cells.Span;
        for (int i = 0; i < cells; i++) { BitHelper.SetRange(ref bits[i >> 6], (byte)(i & 63), length: 1, raw[i] != 0f ? 1UL : 0UL); }
        using MemoryOwner<float> plane = MemoryOwner<float>.Allocate(cells * 2, AllocationMode.Clear);
        Span<float> dst = plane.Span;
        raw[..cells].CopyTo(dst[..cells]);
        for (int i = 0; i < cells; i++) { dst[cells + i] = BitHelper.ExtractRange(bits[i >> 6], (byte)(i & 63), length: 1); }
        return Fin.Succ(Tensor.Create(dst.ToArray(), [1, 2, (nint)grid.Height, (nint)grid.Width * (nint)grid.Channels]));
    }

    // Pack: the point-cloud row materializes every advertised channel — position, then PCA normals/curvature/geodesic
    // when present, then RGBA color when present — so the packed feature width equals FeatureWidth and Of's conformance
    // gate matches. The channels project column-wise into one [N, FeatureWidth] plane through DangerousGetArray.
    private static Fin<Tensor<float>> Pack(GeometryEncoding.PointCloud cloud) {
        int n = (int)cloud.Source.Count;
        int width = cloud.FeatureWidth;
        using MemoryOwner<float> plane = MemoryOwner<float>.Allocate(n * width, AllocationMode.Clear);
        Span<float> dst = plane.Span;
        int column = ChannelColumns(dst, n, width, 0, cloud.Source.Coordinates.Span, EncodingChannel.Position.Width);
        column = cloud.Normals.Match(
            Some: pca => ChannelColumns(dst, n, width, ChannelColumns(dst, n, width, ChannelColumns(dst, n, width, column, pca.Normals.Span, 3), pca.Curvature.Span, 1), pca.Geodesic.Span, 1),
            None: () => column);
        ignore(cloud.Colors.Map(rgba => ChannelColumns(dst, n, width, column, rgba.Span, EncodingChannel.ColorRgba.Width)));
        return Fin.Succ(Tensor.Create(dst.ToArray(), [(nint)n, (nint)width]));
    }

    // ChannelColumns: column-major channel stamp — writes one channel block (channelWidth columns) at the running
    // column offset of the row-major [N, width] plane and returns the advanced offset, so each present channel folds in.
    private static int ChannelColumns(Span<float> plane, int n, int width, int column, ReadOnlySpan<float> channel, int channelWidth) {
        for (int r = 0; r < n; r++) { channel.Slice(r * channelWidth, channelWidth).CopyTo(plane.Slice(r * width + column, channelWidth)); }
        return column + channelWidth;
    }

    private static Fin<(Tensor<float>, Option<Tensor<long>>)> PackMesh(MeshSpace source) =>
        Fin.Succ((Tensor.Create(source.Vertices.ToArray(), [(nint)source.VertexCount, 3]), Some(Tensor.Create(source.FaceIndices.ToArray(), [(nint)source.FaceCount, 3]))));

    // PackNurbs: the canonical B-rep/NURBS encoding threads control points, the rational weight channel, the U/V knot
    // vectors, and the U/V degrees into one plane — control points + weight as the [U*V, 4] position+weight block and
    // the knots+degrees as the auxiliary index tensor — so the encoder satisfies its own canonical-NURBS claim.
    private static Fin<Tensor<float>> PackNurbs(GeometryEncoding.BrepPatch patch) {
        int controls = (int)patch.UCount * (int)patch.VCount;
        using MemoryOwner<float> plane = MemoryOwner<float>.Allocate(controls * 4, AllocationMode.Clear);
        patch.ControlPoints.Span[..(controls * 4)].CopyTo(plane.Span);
        int knots = patch.Knots.Length;
        using MemoryOwner<float> aux = MemoryOwner<float>.Allocate(knots + 4, AllocationMode.Clear);
        patch.Knots.Span.CopyTo(aux.Span);
        aux.Span[knots] = patch.UDegree; aux.Span[knots + 1] = patch.VDegree;
        aux.Span[knots + 2] = (int)patch.UCount; aux.Span[knots + 3] = (int)patch.VCount;
        return Fin.Succ(Tensor.Create([.. plane.Span, .. aux.Span], [(nint)controls + (nint)((knots + 4 + 3) / 4), 4]));
    }
}
```

## [6]-[OPERATION_TABLE]

- Owner: `TensorOpFamily`
- Cases: one-hundred-seven rows across twelve `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural — each carrying its `ToleranceClass` equivalence column; the activation family (`ReLU`, `Gelu`, `SiLU`, `LogSoftMax` beside the direct `Sigmoid`/`Tanh`/`SoftMax` members), the four pooling rows (`MaxPool`/`AvgPool`/`GlobalMaxPool`/`GlobalAvgPool`), the magnitude-extremum reduction pair (`MaxMagnitude`/`MinMagnitude`), the fast-estimate elementwise rows (`ReciprocalEstimate`/`ReciprocalSqrtEstimate`), the widening conversion rows (`ConvertToSingle`/`ConvertToInteger`/`ConvertToIntegerNative`), the predicate-aggregate rows (`IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny`), and the element-domain rows (`ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` over `System.Numerics.Complex`, `QuaternionMultiply`/`QuaternionConjugate`/`QuaternionNormalize` over `System.Numerics.Quaternion`) ride the existing kind axis as rows, never sibling op types
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind and tolerance columns; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row; zero new surface.
- Boundary: the `ToleranceClass` vocabulary closes the equivalence axis — exact (integer and predicate rows), tight (fused-triad and reduction rows), transcendental (ULP-banded same-route rows), statistical (accumulation-scaled rows), estimate (platform-variant fast-reciprocal rows with no cross-machine bound) — and a row's `Tolerance` column is the equivalence proof key the `EquivalenceLaw` reads by data, never a `Prove` argument; the estimate rows carry the `Estimate` band because `ReciprocalEstimate`/`ReciprocalSqrtEstimate` legitimately differ across machines and are inadmissible wherever cross-machine bit agreement is contractual, paired against their exact `Reciprocal`/`ReciprocalSqrt` rows; `MinNumber`/`MaxNumber` are the NaN-as-missing reduction pair distinct from the NaN-propagating `Min`/`Max` rows, binding the `T.MinNumber`/`T.MaxNumber` reduction members on the `Fold` table, and `MaxMagnitude`/`MinMagnitude` reduce by signed absolute extremum on the same table; `IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` are the boolean-aggregate predicate rows reducing to one `bool` distinct from the per-element `IsNaN`/`IsFinite` mask rows; the four structural pooling rows fold in-lane through the shared strided-window `Pool` kernel over the verified `TensorPrimitives.Max`/`Average` window reducers on the rank-aware `GetDimensionSpan` cursor, while the matrix rows (`MatMul`, `Conv1D`/`Conv2D`/`Conv3D`) carry no `TensorPrimitives` member and lower through `numeric#KERNEL_LOWERING` (matmul to GEMM, convolution to im2col); the table keys through the ordinal `TensorKeyPolicy` so every binding index resolves the same comparer.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class ToleranceClass {
    public static readonly ToleranceClass Exact = new("exact", relativeBound: 0.0);
    public static readonly ToleranceClass Tight = new("tight", relativeBound: 1e-12);
    public static readonly ToleranceClass Transcendental = new("transcendental", relativeBound: 1e-9);
    public static readonly ToleranceClass Statistical = new("statistical", relativeBound: 1e-6);
    public static readonly ToleranceClass Estimate = new("estimate", relativeBound: double.PositiveInfinity);

    public double RelativeBound { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class TensorOpKind {
    public static readonly TensorOpKind Elementwise = new("elementwise");
    public static readonly TensorOpKind Rounding = new("rounding");
    public static readonly TensorOpKind Transcendental = new("transcendental");
    public static readonly TensorOpKind Reduction = new("reduction");
    public static readonly TensorOpKind Statistics = new("statistics");
    public static readonly TensorOpKind Bitwise = new("bitwise");
    public static readonly TensorOpKind Population = new("population");
    public static readonly TensorOpKind Similarity = new("similarity");
    public static readonly TensorOpKind Conversion = new("conversion");
    public static readonly TensorOpKind Predicate = new("predicate");
    public static readonly TensorOpKind Matrix = new("matrix");
    public static readonly TensorOpKind Structural = new("structural");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class TensorOpFamily {
    public static readonly TensorOpFamily Add = new("add", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Subtract = new("subtract", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Multiply = new("multiply", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Divide = new("divide", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Negate = new("negate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Abs = new("abs", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Clamp = new("clamp", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily CopySign = new("copy-sign", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily MultiplyAdd = new("multiply-add", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily FusedMultiplyAdd = new("fused-multiply-add", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily AddMultiply = new("add-multiply", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily Lerp = new("lerp", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily Hypot = new("hypot", TensorOpKind.Elementwise, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Reciprocal = new("reciprocal", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily ReciprocalSqrt = new("reciprocal-sqrt", TensorOpKind.Elementwise, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily ReciprocalEstimate = new("reciprocal-estimate", TensorOpKind.Elementwise, ToleranceClass.Estimate);
    public static readonly TensorOpFamily ReciprocalSqrtEstimate = new("reciprocal-sqrt-estimate", TensorOpKind.Elementwise, ToleranceClass.Estimate);
    public static readonly TensorOpFamily Ieee754Remainder = new("ieee-754-remainder", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Round = new("round", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Floor = new("floor", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Ceiling = new("ceiling", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Truncate = new("truncate", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Exp = new("exp", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Log = new("log", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Sin = new("sin", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Cos = new("cos", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Tanh = new("tanh", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Sigmoid = new("sigmoid", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily SoftMax = new("softmax", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily LogSoftMax = new("log-softmax", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily ReLU = new("relu", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily Gelu = new("gelu", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily SiLU = new("silu", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Pow = new("pow", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Sqrt = new("sqrt", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Cbrt = new("cbrt", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily DegreesToRadians = new("degrees-to-radians", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Atan2 = new("atan2", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily RootN = new("root-n", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily ScaleB = new("scale-b", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily SinCos = new("sin-cos", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Sum = new("sum", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily Product = new("product", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily Dot = new("dot", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily Norm = new("norm", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily Min = new("min", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily Max = new("max", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MinNumber = new("min-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MaxNumber = new("max-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MinMagnitude = new("min-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MaxMagnitude = new("max-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily SumOfSquares = new("sum-of-squares", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily SumOfMagnitudes = new("sum-of-magnitudes", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily ProductOfSums = new("product-of-sums", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily ProductOfDifferences = new("product-of-differences", TensorOpKind.Reduction, ToleranceClass.Tight);
    public static readonly TensorOpFamily IndexOfMax = new("index-of-max", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMin = new("index-of-min", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMaxMagnitude = new("index-of-max-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMinMagnitude = new("index-of-min-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily Average = new("average", TensorOpKind.Statistics, ToleranceClass.Statistical);
    public static readonly TensorOpFamily StdDev = new("std-dev", TensorOpKind.Statistics, ToleranceClass.Statistical);
    public static readonly TensorOpFamily BitwiseAnd = new("bitwise-and", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily BitwiseOr = new("bitwise-or", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Xor = new("xor", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily OnesComplement = new("ones-complement", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily ShiftLeft = new("shift-left", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily ShiftRight = new("shift-right", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily ShiftRightLogical = new("shift-right-logical", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily RotateLeft = new("rotate-left", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily RotateRight = new("rotate-right", TensorOpKind.Bitwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily PopCount = new("pop-count", TensorOpKind.Population, ToleranceClass.Exact);
    public static readonly TensorOpFamily LeadingZeroCount = new("leading-zero-count", TensorOpKind.Population, ToleranceClass.Exact);
    public static readonly TensorOpFamily TrailingZeroCount = new("trailing-zero-count", TensorOpKind.Population, ToleranceClass.Exact);
    public static readonly TensorOpFamily CosineSimilarity = new("cosine-similarity", TensorOpKind.Similarity, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Distance = new("distance", TensorOpKind.Similarity, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily HammingDistance = new("hamming-distance", TensorOpKind.Similarity, ToleranceClass.Exact);
    public static readonly TensorOpFamily HammingBitDistance = new("hamming-bit-distance", TensorOpKind.Similarity, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertChecked = new("convert-checked", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertSaturating = new("convert-saturating", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertTruncating = new("convert-truncating", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertToHalf = new("convert-to-half", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertToSingle = new("convert-to-single", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertToInteger = new("convert-to-integer", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily ConvertToIntegerNative = new("convert-to-integer-native", TensorOpKind.Conversion, ToleranceClass.Exact);
    public static readonly TensorOpFamily Sign = new("sign", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNaN = new("is-nan", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFinite = new("is-finite", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNaNAll = new("is-nan-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNaNAny = new("is-nan-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFiniteAll = new("is-finite-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFiniteAny = new("is-finite-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily MatMul = new("matmul", TensorOpKind.Matrix, ToleranceClass.Tight);
    public static readonly TensorOpFamily Conv1D = new("conv-1d", TensorOpKind.Matrix, ToleranceClass.Tight);
    public static readonly TensorOpFamily Conv2D = new("conv-2d", TensorOpKind.Matrix, ToleranceClass.Tight);
    public static readonly TensorOpFamily Conv3D = new("conv-3d", TensorOpKind.Matrix, ToleranceClass.Tight);
    public static readonly TensorOpFamily MaxPool = new("max-pool", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily AvgPool = new("avg-pool", TensorOpKind.Structural, ToleranceClass.Statistical);
    public static readonly TensorOpFamily GlobalMaxPool = new("global-max-pool", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily GlobalAvgPool = new("global-avg-pool", TensorOpKind.Structural, ToleranceClass.Statistical);
    public static readonly TensorOpFamily MaskedWrite = new("masked-write", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily ComplexAbs = new("complex-abs", TensorOpKind.Elementwise, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily ComplexExp = new("complex-exp", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily ComplexLog = new("complex-log", TensorOpKind.Transcendental, ToleranceClass.Transcendental);
    public static readonly TensorOpFamily Conjugate = new("conjugate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily QuaternionMultiply = new("quaternion-multiply", TensorOpKind.Elementwise, ToleranceClass.Tight);
    public static readonly TensorOpFamily QuaternionConjugate = new("quaternion-conjugate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily QuaternionNormalize = new("quaternion-normalize", TensorOpKind.Elementwise, ToleranceClass.Transcendental);

    public TensorOpKind Kind { get; }
    public ToleranceClass Tolerance { get; }
}
```

## [7]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination)` — `Fin<Unit>` aborts on a kernel-row miss; the arity siblings `Zip`, `Fuse`, `Dual`, `Bits`, `Shift`, `Population`, `Convert`, `ToHalf`, `ToSingle`, `Root`, `Fold`, `FoldPair`, `IndexOf`, `Polarity`, `Test`, `Aggregate`, `Hamming`, `HammingBits`, `Mask`, `Pool`, the element-domain `ComplexZip`/`ComplexMap`/`ComplexAbs` and `QuaternionZip`/`QuaternionMap`, and the claim-gated `Partition` dispatch the same `TensorOpFamily` table; `Pool` is the rank-aware strided-window fold the four pooling rows share over one `PoolReducers<T>` reducer table walking a `GetDimensionSpan` cursor; `Aggregate` is the boolean-reduce the `*All`/`*Any` predicate rows share; `Mask` is the predicate-masked write binding `Tensor.FilteredUpdate`; `Partition` reads `CpuBudget.PartitionCap` and the winning claim's partition-route column, dispatching the 1-D `For` over a `MapBlock` or the 2-D `For2D` over a `Memory2D` plane and falling through to inline `Map` when no winning claim is supplied.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a new activation is one `Activations<T>` composed fold plus one `Unary` row, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, a new predicate-aggregate is one `AggregateReducers<T>` entry on the shared `Aggregate` fold, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/aggregate/complex method; a matrix kernel is one lowering row read from `numeric#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path reading `CpuBudget.PartitionCap` and the claim's route column, never a new owner; zero new surface.
- Boundary: every span row binds the TensorPrimitives member matching its Pascal-cased key (shift-right binds the arithmetic-shift member, shift-right-logical the logical member) and only the `MaskedWrite` row carries a full-tensor binding — `Tensor.FilteredUpdate(in TensorSpan<T>, in ReadOnlyTensorSpan<bool>, in ReadOnlyTensorSpan<T>)` is the predicate-masked write the row's name advertises, the `Mask` arity binding it (the unconditional `SetSlice` region overwrite is the deleted form that does not honor the mask); the predicate rows write non-`T` destinations — `Sign` fills `Span<int>` through `Polarity`, `IsNaN`/`IsFinite` fill `Span<bool>` through `Test`, and `IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` reduce to one `bool` through `Aggregate` over the verified `TensorPrimitives.IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` members, deciding the algorithms.md empty-span finite gate (`IsFiniteAll` on an empty span returns false, decided by its own arm before the finite gate); `SinCos` writes the two destination spans through `Dual` under the `ITrigonometricFunctions<T>` constraint; `RootN`/`ScaleB` take the integer parameter through `Root` under `IRootFunctions<T>`; `PopCount`/`LeadingZeroCount`/`TrailingZeroCount`/`OnesComplement` ride the integer `Population` arity, `BitwiseOr`/`Xor` the integer binary table, and `RotateLeft`/`RotateRight`/`ShiftRightLogical` the integer shift table; `ConvertToHalf` binds the fixed `(float→Half)` `ToHalf` row and `ConvertToSingle` the fixed `(Half→float)` `ToSingle` widen row, paired as the Half narrow/widen boundary the model-boundary brain-float carrier crosses at the inference admission seam; `HammingBitDistance` binds the `HammingBits` integer-pair reduce; `MinNumber`/`MaxNumber` bind the `TensorPrimitives.MinNumber`/`MaxNumber` reduction members (`T MinNumber<T>(ReadOnlySpan<T>)`/`T MaxNumber<T>(ReadOnlySpan<T>)` under `INumber<T>`, which the `IFloatingPointIeee754<T>` table constraint satisfies) on the `Fold` table paired against the NaN-propagating `Min`/`Max` rows, and `MaxMagnitude`/`MinMagnitude` bind the signed-absolute-extremum `TensorPrimitives.MaxMagnitude`/`MinMagnitude` reduction members on the same `Fold` table — the `MaxMagnitude` member is the abs-normalizing denominator the `EquivalenceLaw` reads; `Average` and `StdDev` bind the direct `TensorPrimitives.Average`/`StdDev` reduction members (`T Average<T>(ReadOnlySpan<T>)` under `INumberBase<T>` and `T StdDev<T>(ReadOnlySpan<T>)` under `IRootFunctions<T>`, both satisfied by the `IFloatingPointIeee754<T>` table constraint) on the `Fold` table — a hand-composed `Sum/n` mean or `Sqrt(SumOfSquares/n − mean²)` std-dev beside the admitted members is the deleted form; `ReciprocalEstimate`/`ReciprocalSqrtEstimate` bind the fast-estimate `TensorPrimitives.ReciprocalEstimate`/`ReciprocalSqrtEstimate` members on the `Unary` table beside their exact `Reciprocal`/`ReciprocalSqrt` rows, carrying the `Estimate` tolerance because they are platform-variant; the activation family binds in-lane on the `Unary` table — `Sigmoid`/`Tanh`/`SoftMax` to their direct `TensorPrimitives` members and `ReLU`/`Gelu`/`SiLU`/`LogSoftMax` to the `Activations<T>` composed author-folds (`ReLU` is `Clamp(x, 0, +inf)`, `SiLU` is `x .* Sigmoid(x)`, `Gelu` the tanh-approximation `MultiplyAdd`/`Tanh` chain, `LogSoftMax` the numerically-stable `x − logsumexp(x)` max-shift renting a `MemoryOwner<T>` exp scratch), never a per-element activation loop or a fabricated `TensorPrimitives.Relu`/`Gelu`/`SiLU`/`LogSoftmax` phantom; the four pooling rows fold in-lane through the rank-aware `Pool` over one `PoolReducers<T>` window-reducer table walking the `GetDimensionSpan` cursor — `MaxPool`/`GlobalMaxPool` reduce each window through `TensorPrimitives.Max` and `AvgPool`/`GlobalAvgPool` through `TensorPrimitives.Average`, the global rows collapsing the whole spatial plane to one window (window = stride = length) — so pooling carries verified members and rank-walks NCHW/voxel planes, and only `MatMul`/`Conv1D`/`Conv2D`/`Conv3D` hold no in-lane member; the element-domain rows ride `System.Numerics.Complex` and `System.Numerics.Quaternion` carriers — Complex arithmetic (`Add`/`Subtract`/`Multiply`/`Divide`/`Negate`) binds the verified `INumberBase<Complex>`-generic `TensorPrimitives` members through `ComplexZip`/`ComplexMap`, while `ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` are author-folds over the BCL `Complex` intrinsics (no `TensorPrimitives` Complex specialization exists) and `ComplexAbs` writes the `Span<double>` magnitude polarity through the `MagnitudeKernel`; Quaternion implements neither numeric interface so `QuaternionMultiply` (the non-commutative Hamilton product), `QuaternionConjugate`, and `QuaternionNormalize` are author-folds over the BCL `Quaternion` operators through `QuaternionZip`/`QuaternionMap`; the `MatMul`, `Conv1D`/`Conv2D`/`Conv3D` rows hold no TensorPrimitives or in-package member and `Map` resolves each through the `numeric#KERNEL_LOWERING` binding table behind a winning benchmark-claim row — matmul lowers to the numeric-lane GEMM and convolution to im2col-then-GEMM — returning a `<kernel-row-miss>` `Fin.Fail` only when no lowering row is bound; the partition column dispatches a struct `IAction` 1-D span-kernel through `ParallelHelper.For<TAction>` or a struct `IAction2D` plane-kernel through `ParallelHelper.For2D<TAction>` over a `Memory2D` plane, clamped at `CpuBudget.PartitionCap`, taken only when a winning `BenchmarkClaim` names the partition route in its route column — a partition count of one collapses inline and an unbudgeted `Parallel.For` over spans is the deleted form; the binding tables are `FrozenDictionary` indexes keyed through the ordinal `TensorKeyPolicy`, span kernels iterate rows by reference through `RefEnumerable<T>`/`SpanEnumerable<T>` rather than flat indexing, and integer dtypes enter the real-constrained entries through the conversion rows first.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public delegate void UnaryKernel<T>(ReadOnlySpan<T> x, Span<T> destination);
public delegate void BinaryKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination);
public delegate void TernaryKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination);
public delegate void DualKernel<T>(ReadOnlySpan<T> x, Span<T> first, Span<T> second);
public delegate void ShiftKernel<T>(ReadOnlySpan<T> x, int shiftCount, Span<T> destination);
public delegate void ConvertKernel<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination);
public delegate void SignKernel<T>(ReadOnlySpan<T> x, Span<int> destination);
public delegate void MaskKernel<T>(ReadOnlySpan<T> x, Span<bool> destination);
public delegate T FoldKernel<T>(ReadOnlySpan<T> x);
public delegate T PairFoldKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y);
public delegate int IndexKernel<T>(ReadOnlySpan<T> x);
public delegate bool AggregateKernel<T>(ReadOnlySpan<T> x);
public delegate void MagnitudeKernel(ReadOnlySpan<Complex> x, Span<double> destination);
public delegate T WindowReducer<T>(ReadOnlySpan<T> window);

// --- [OPERATIONS] --------------------------------------------------------------------------
// Effects: the one effect-to-Fin adapter every void-kernel arity site shares (TensorBridge egress, TensorOps dispatch,
// BoundFlow flow), collapsing the >=3 per-class statement-bodied Succ wrappers into one.
public static class Effects {
    public static Fin<Unit> ToFin(Action kernel) { kernel(); return Fin.Succ(unit); }
}

// Elementwise / ElementwisePair: the two scalar-projection folds every author-fold kernel binds to, so each
// Complex/Quaternion row is one row -> scalar function, never a hand-copied index loop. One fold owns the variation.
public static class Projection {
    public static UnaryKernel<TElem> Elementwise<TElem>(Func<TElem, TElem> f) =>
        (x, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = f(x[i]); } };
    public static BinaryKernel<TElem> ElementwisePair<TElem>(Func<TElem, TElem, TElem> g) =>
        (x, y, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = g(x[i], y[i]); } };
    public static MagnitudeKernel Magnitude(Func<Complex, double> m) =>
        (x, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = m(x[i]); } };
}

public static class Activations<T> where T : IFloatingPointIeee754<T> {
    public static void ReLU(ReadOnlySpan<T> x, Span<T> destination) =>
        TensorPrimitives.Clamp(x, T.Zero, T.PositiveInfinity, destination);

    public static void SiLU(ReadOnlySpan<T> x, Span<T> destination) {
        TensorPrimitives.Sigmoid(x, destination);
        TensorPrimitives.Multiply(destination, x, destination);
    }

    public static void Gelu(ReadOnlySpan<T> x, Span<T> destination) {
        T c = T.Sqrt(T.CreateChecked(2) / T.Pi);
        T a = T.CreateChecked(0.044715);
        TensorPrimitives.Multiply(x, x, destination);
        TensorPrimitives.Multiply(destination, x, destination);
        TensorPrimitives.MultiplyAdd(destination, a, x, destination);
        TensorPrimitives.Multiply(destination, c, destination);
        TensorPrimitives.Tanh(destination, destination);
        TensorPrimitives.Add(destination, T.One, destination);
        TensorPrimitives.Multiply(destination, x, destination);
        TensorPrimitives.Multiply(destination, T.CreateChecked(0.5), destination);
    }

    // LogSoftMax(x) = x - logsumexp(x): the numerically-stable max-shift fold; the exp scratch rents a MemoryOwner<T>
    // (T is IFloatingPointIeee754, not unmanaged, so no stackalloc) sized to x, the Map boundary rails length equality.
    public static void LogSoftMax(ReadOnlySpan<T> x, Span<T> destination) {
        T shift = TensorPrimitives.Max(x);
        TensorPrimitives.Subtract(x, shift, destination);
        using MemoryOwner<T> scratch = MemoryOwner<T>.Allocate(x.Length);
        Span<T> exps = scratch.Span;
        TensorPrimitives.Exp(destination, exps);
        T logSumExp = T.Log(TensorPrimitives.Sum(exps));
        TensorPrimitives.Subtract(destination, logSumExp, destination);
    }
}

public static class PoolReducers<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, WindowReducer<T>> Rows = new Dictionary<TensorOpFamily, WindowReducer<T>> {
        [TensorOpFamily.MaxPool] = TensorPrimitives.Max, [TensorOpFamily.GlobalMaxPool] = TensorPrimitives.Max,
        [TensorOpFamily.AvgPool] = TensorPrimitives.Average, [TensorOpFamily.GlobalAvgPool] = TensorPrimitives.Average,
    }.ToFrozenDictionary();
}

public static class AggregateReducers<T> where T : INumberBase<T> {
    public static readonly FrozenDictionary<TensorOpFamily, AggregateKernel<T>> Rows = new Dictionary<TensorOpFamily, AggregateKernel<T>> {
        [TensorOpFamily.IsNaNAll] = TensorPrimitives.IsNaNAll, [TensorOpFamily.IsNaNAny] = TensorPrimitives.IsNaNAny,
        [TensorOpFamily.IsFiniteAll] = TensorPrimitives.IsFiniteAll, [TensorOpFamily.IsFiniteAny] = TensorPrimitives.IsFiniteAny,
    }.ToFrozenDictionary();
}

public static class TensorKernels<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate, [TensorOpFamily.Abs] = TensorPrimitives.Abs, [TensorOpFamily.Round] = TensorPrimitives.Round,
        [TensorOpFamily.Floor] = TensorPrimitives.Floor, [TensorOpFamily.Ceiling] = TensorPrimitives.Ceiling, [TensorOpFamily.Truncate] = TensorPrimitives.Truncate,
        [TensorOpFamily.Exp] = TensorPrimitives.Exp, [TensorOpFamily.Log] = TensorPrimitives.Log, [TensorOpFamily.Sin] = TensorPrimitives.Sin, [TensorOpFamily.Cos] = TensorPrimitives.Cos,
        [TensorOpFamily.Tanh] = TensorPrimitives.Tanh, [TensorOpFamily.Sigmoid] = TensorPrimitives.Sigmoid, [TensorOpFamily.SoftMax] = TensorPrimitives.SoftMax,
        [TensorOpFamily.Sqrt] = TensorPrimitives.Sqrt, [TensorOpFamily.Cbrt] = TensorPrimitives.Cbrt, [TensorOpFamily.DegreesToRadians] = TensorPrimitives.DegreesToRadians,
        [TensorOpFamily.Reciprocal] = TensorPrimitives.Reciprocal, [TensorOpFamily.ReciprocalSqrt] = TensorPrimitives.ReciprocalSqrt,
        [TensorOpFamily.ReciprocalEstimate] = TensorPrimitives.ReciprocalEstimate, [TensorOpFamily.ReciprocalSqrtEstimate] = TensorPrimitives.ReciprocalSqrtEstimate,
        [TensorOpFamily.ReLU] = Activations<T>.ReLU, [TensorOpFamily.Gelu] = Activations<T>.Gelu, [TensorOpFamily.SiLU] = Activations<T>.SiLU,
        [TensorOpFamily.LogSoftMax] = Activations<T>.LogSoftMax,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<T>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<T>> {
        [TensorOpFamily.Add] = TensorPrimitives.Add, [TensorOpFamily.Subtract] = TensorPrimitives.Subtract, [TensorOpFamily.Multiply] = TensorPrimitives.Multiply,
        [TensorOpFamily.Divide] = TensorPrimitives.Divide, [TensorOpFamily.Pow] = TensorPrimitives.Pow, [TensorOpFamily.Atan2] = TensorPrimitives.Atan2,
        [TensorOpFamily.CopySign] = TensorPrimitives.CopySign, [TensorOpFamily.Hypot] = TensorPrimitives.Hypot, [TensorOpFamily.Ieee754Remainder] = TensorPrimitives.Ieee754Remainder,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, TernaryKernel<T>> Ternary = new Dictionary<TensorOpFamily, TernaryKernel<T>> {
        [TensorOpFamily.MultiplyAdd] = TensorPrimitives.MultiplyAdd, [TensorOpFamily.FusedMultiplyAdd] = TensorPrimitives.FusedMultiplyAdd,
        [TensorOpFamily.AddMultiply] = TensorPrimitives.AddMultiply, [TensorOpFamily.Clamp] = TensorPrimitives.Clamp, [TensorOpFamily.Lerp] = TensorPrimitives.Lerp,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, DualKernel<T>> Dual = new Dictionary<TensorOpFamily, DualKernel<T>> {
        [TensorOpFamily.SinCos] = TensorPrimitives.SinCos,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, SignKernel<T>> Sign = new Dictionary<TensorOpFamily, SignKernel<T>> {
        [TensorOpFamily.Sign] = TensorPrimitives.Sign,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, MaskKernel<T>> Mask = new Dictionary<TensorOpFamily, MaskKernel<T>> {
        [TensorOpFamily.IsNaN] = TensorPrimitives.IsNaN, [TensorOpFamily.IsFinite] = TensorPrimitives.IsFinite,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, FoldKernel<T>> Fold = new Dictionary<TensorOpFamily, FoldKernel<T>> {
        [TensorOpFamily.Sum] = TensorPrimitives.Sum, [TensorOpFamily.Product] = TensorPrimitives.Product, [TensorOpFamily.Min] = TensorPrimitives.Min,
        [TensorOpFamily.Max] = TensorPrimitives.Max, [TensorOpFamily.MinNumber] = TensorPrimitives.MinNumber, [TensorOpFamily.MaxNumber] = TensorPrimitives.MaxNumber,
        [TensorOpFamily.MinMagnitude] = TensorPrimitives.MinMagnitude, [TensorOpFamily.MaxMagnitude] = TensorPrimitives.MaxMagnitude,
        [TensorOpFamily.Norm] = TensorPrimitives.Norm, [TensorOpFamily.SumOfSquares] = TensorPrimitives.SumOfSquares, [TensorOpFamily.SumOfMagnitudes] = TensorPrimitives.SumOfMagnitudes,
        [TensorOpFamily.Average] = TensorPrimitives.Average, [TensorOpFamily.StdDev] = TensorPrimitives.StdDev,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, PairFoldKernel<T>> PairFold = new Dictionary<TensorOpFamily, PairFoldKernel<T>> {
        [TensorOpFamily.Dot] = TensorPrimitives.Dot, [TensorOpFamily.CosineSimilarity] = TensorPrimitives.CosineSimilarity, [TensorOpFamily.Distance] = TensorPrimitives.Distance,
        [TensorOpFamily.ProductOfSums] = TensorPrimitives.ProductOfSums, [TensorOpFamily.ProductOfDifferences] = TensorPrimitives.ProductOfDifferences,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, IndexKernel<T>> Index = new Dictionary<TensorOpFamily, IndexKernel<T>> {
        [TensorOpFamily.IndexOfMax] = TensorPrimitives.IndexOfMax, [TensorOpFamily.IndexOfMin] = TensorPrimitives.IndexOfMin,
        [TensorOpFamily.IndexOfMaxMagnitude] = TensorPrimitives.IndexOfMaxMagnitude, [TensorOpFamily.IndexOfMinMagnitude] = TensorPrimitives.IndexOfMinMagnitude,
    }.ToFrozenDictionary();
}

public static class IntegerKernels<T> where T : IBinaryInteger<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.OnesComplement] = TensorPrimitives.OnesComplement, [TensorOpFamily.PopCount] = TensorPrimitives.PopCount,
        [TensorOpFamily.LeadingZeroCount] = TensorPrimitives.LeadingZeroCount, [TensorOpFamily.TrailingZeroCount] = TensorPrimitives.TrailingZeroCount,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<T>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<T>> {
        [TensorOpFamily.BitwiseAnd] = TensorPrimitives.BitwiseAnd, [TensorOpFamily.BitwiseOr] = TensorPrimitives.BitwiseOr, [TensorOpFamily.Xor] = TensorPrimitives.Xor,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, ShiftKernel<T>> Shift = new Dictionary<TensorOpFamily, ShiftKernel<T>> {
        [TensorOpFamily.ShiftLeft] = TensorPrimitives.ShiftLeft, [TensorOpFamily.ShiftRight] = TensorPrimitives.ShiftRightArithmetic,
        [TensorOpFamily.ShiftRightLogical] = TensorPrimitives.ShiftRightLogical, [TensorOpFamily.RotateLeft] = TensorPrimitives.RotateLeft, [TensorOpFamily.RotateRight] = TensorPrimitives.RotateRight,
    }.ToFrozenDictionary();
}

public static class ConvertKernels<TFrom, TTo> where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> {
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<TFrom, TTo>> Rows = new Dictionary<TensorOpFamily, ConvertKernel<TFrom, TTo>> {
        [TensorOpFamily.ConvertChecked] = TensorPrimitives.ConvertChecked, [TensorOpFamily.ConvertSaturating] = TensorPrimitives.ConvertSaturating,
        [TensorOpFamily.ConvertTruncating] = TensorPrimitives.ConvertTruncating,
    }.ToFrozenDictionary();
}

// Half narrow/widen: the two fixed-carrier conversion rows the brain-float model boundary crosses; ToHalf is the
// (float -> Half) narrow at inference admission, ToSingle the (Half -> float) widen out of the half-accelerated kernel.
public static class HalfConvertKernels {
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<float, Half>> Narrow = new Dictionary<TensorOpFamily, ConvertKernel<float, Half>> {
        [TensorOpFamily.ConvertToHalf] = TensorPrimitives.ConvertToHalf,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<Half, float>> Widen = new Dictionary<TensorOpFamily, ConvertKernel<Half, float>> {
        [TensorOpFamily.ConvertToSingle] = TensorPrimitives.ConvertToSingle,
    }.ToFrozenDictionary();
}

// System.Numerics.Complex : INumberBase<Complex>, so the elementwise arithmetic rows ride the verified generic
// TensorPrimitives members; Abs/Exp/Log/Conjugate carry no Complex specialization and are author-folds.
public static class ComplexKernels {
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<Complex>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<Complex>> {
        [TensorOpFamily.Add] = TensorPrimitives.Add, [TensorOpFamily.Subtract] = TensorPrimitives.Subtract,
        [TensorOpFamily.Multiply] = TensorPrimitives.Multiply, [TensorOpFamily.Divide] = TensorPrimitives.Divide,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<Complex>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<Complex>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate,
        [TensorOpFamily.Conjugate] = Projection.Elementwise<Complex>(Complex.Conjugate),
        [TensorOpFamily.ComplexExp] = Projection.Elementwise<Complex>(Complex.Exp),
        [TensorOpFamily.ComplexLog] = Projection.Elementwise<Complex>(static x => Complex.Log(x)),
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, MagnitudeKernel> Magnitude = new Dictionary<TensorOpFamily, MagnitudeKernel> {
        [TensorOpFamily.ComplexAbs] = Projection.Magnitude(static x => x.Magnitude),
    }.ToFrozenDictionary();
}

// System.Numerics.Quaternion implements neither numeric interface, so every quaternion op is an author-fold over the
// BCL Quaternion intrinsics; quaternion multiply is the non-commutative Hamilton product.
public static class QuaternionKernels {
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<Quaternion>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<Quaternion>> {
        [TensorOpFamily.QuaternionMultiply] = Projection.ElementwisePair<Quaternion>(static (a, b) => a * b),
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<Quaternion>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<Quaternion>> {
        [TensorOpFamily.QuaternionConjugate] = Projection.Elementwise<Quaternion>(Quaternion.Conjugate),
        [TensorOpFamily.QuaternionNormalize] = Projection.Elementwise<Quaternion>(Quaternion.Normalize),
    }.ToFrozenDictionary();
}

public static class TensorOps {
    // Dispatch: the one table-lookup-then-invoke fold every void-kernel arity sibling shares. A null kernel rails to
    // Miss; a present kernel runs its closure-captured invoker through the shared Effects.ToFin adapter and returns Succ.
    private static Fin<Unit> Dispatch<TKernel>(TensorOpFamily row, FrozenDictionary<TensorOpFamily, TKernel> table, Action<TKernel> invoke) where TKernel : Delegate =>
        table.GetValueOrDefault(row) is { } kernel ? Effects.ToFin(() => invoke(kernel)) : Miss<Unit>(row);
    private static Fin<Unit> EqualLength(TensorOpFamily row, int source, int destination, Fin<Unit> next) =>
        source == destination ? next : TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{source}!={destination}");

    public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IFloatingPointIeee754<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Unary, k => k(x, destination)));
    public static Fin<Unit> Zip<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IFloatingPointIeee754<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Binary, k => k(x, y, destination)));
    public static Fin<Unit> Fuse<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination) where T : IFloatingPointIeee754<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Ternary, k => k(x, y, z, destination)));
    public static Fin<Unit> Bits<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IBinaryInteger<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, IntegerKernels<T>.Binary, k => k(x, y, destination)));
    public static Fin<Unit> Shift<T>(TensorOpFamily row, ReadOnlySpan<T> x, int shiftCount, Span<T> destination) where T : IBinaryInteger<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, IntegerKernels<T>.Shift, k => k(x, shiftCount, destination)));
    public static Fin<Unit> Convert<TFrom, TTo>(TensorOpFamily row, ReadOnlySpan<TFrom> source, Span<TTo> destination) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> =>
        EqualLength(row, source.Length, destination.Length, Dispatch(row, ConvertKernels<TFrom, TTo>.Rows, k => k(source, destination)));
    public static Fin<Unit> Dual<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> first, Span<T> second) where T : ITrigonometricFunctions<T> =>
        EqualLength(row, x.Length, first.Length, EqualLength(row, x.Length, second.Length, Dispatch(row, TensorKernels<T>.Dual, k => k(x, first, second))));
    public static Fin<Unit> Polarity<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : INumberBase<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Sign, k => k(x, destination)));
    public static Fin<Unit> Test<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<bool> destination) where T : INumberBase<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Mask, k => k(x, destination)));
    public static Fin<Unit> Population<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IBinaryInteger<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, IntegerKernels<T>.Unary, k => k(x, destination)));
    public static Fin<Unit> ToHalf(TensorOpFamily row, ReadOnlySpan<float> source, Span<Half> destination) =>
        EqualLength(row, source.Length, destination.Length, Dispatch(row, HalfConvertKernels.Narrow, k => k(source, destination)));
    public static Fin<Unit> ToSingle(TensorOpFamily row, ReadOnlySpan<Half> source, Span<float> destination) =>
        EqualLength(row, source.Length, destination.Length, Dispatch(row, HalfConvertKernels.Widen, k => k(source, destination)));
    public static Fin<Unit> ComplexZip(TensorOpFamily row, ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, Span<Complex> destination) =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, ComplexKernels.Binary, k => k(x, y, destination)));
    public static Fin<Unit> ComplexMap(TensorOpFamily row, ReadOnlySpan<Complex> x, Span<Complex> destination) =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, ComplexKernels.Unary, k => k(x, destination)));
    public static Fin<Unit> ComplexAbs(TensorOpFamily row, ReadOnlySpan<Complex> x, Span<double> destination) =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, ComplexKernels.Magnitude, k => k(x, destination)));
    public static Fin<Unit> QuaternionZip(TensorOpFamily row, ReadOnlySpan<Quaternion> x, ReadOnlySpan<Quaternion> y, Span<Quaternion> destination) =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, QuaternionKernels.Binary, k => k(x, y, destination)));
    public static Fin<Unit> QuaternionMap(TensorOpFamily row, ReadOnlySpan<Quaternion> x, Span<Quaternion> destination) =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, QuaternionKernels.Unary, k => k(x, destination)));

    // The value-returning folds carry a typed return, not Unit, so they read the table directly rather than through Dispatch.
    public static Fin<T> Fold<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Fold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<T>(row);
    public static Fin<T> FoldPair<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.PairFold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x, y)) : Miss<T>(row);
    public static Fin<int> IndexOf<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Index.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<int>(row);
    // Aggregate: the boolean-reduce the IsNaNAll/IsNaNAny/IsFiniteAll/IsFiniteAny rows share; the empty-span finite gate
    // is decided here (IsFiniteAll on an empty span returns false, the algorithms.md empty-edge arm) before any read.
    public static Fin<bool> Aggregate<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : INumberBase<T> =>
        AggregateReducers<T>.Rows.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<bool>(row);
    public static Fin<int> Hamming<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IEquatable<T> => Fin.Succ(TensorPrimitives.HammingDistance(x, y));
    public static Fin<long> HammingBits<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IBinaryInteger<T> => Fin.Succ(TensorPrimitives.HammingBitDistance(x, y));

    public static Fin<Unit> Root<T>(TensorOpFamily row, ReadOnlySpan<T> x, int n, Span<T> destination) where T : IFloatingPointIeee754<T>, IRootFunctions<T> =>
        EqualLength(row, x.Length, destination.Length, row switch {
            _ when row == TensorOpFamily.RootN => Effects.ToFin(() => TensorPrimitives.RootN(x, n, destination)),
            _ when row == TensorOpFamily.ScaleB => Effects.ToFin(() => TensorPrimitives.ScaleB(x, n, destination)),
            _ => Miss<Unit>(row),
        });
    // Mask: the predicate-masked write. FilteredUpdate writes values only where the bool filter is true, the masked
    // semantics the row's name advertises; T : unmanaged (not IFloatingPointIeee754) so it carries a full-tensor binding
    // rather than the span-Dispatch fold.
    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<bool> filter, in ReadOnlyTensorSpan<T> values) where T : unmanaged {
        if (row != TensorOpFamily.MaskedWrite) { return Miss<Tensor<T>>(row); }
        TensorSpan<T> span = destination.AsTensorSpan();
        Tensor.FilteredUpdate(span, filter, values);
        return Fin.Succ(destination);
    }
    // Pool: the rank-aware strided-window fold the four pooling rows share over one WindowReducer. The GetDimensionSpan
    // cursor walks the chosen batch/channel axis so each slice is one spatial plane pooled over its own windows, no 1-D flatten.
    public static Fin<Unit> Pool<T>(TensorOpFamily row, Tensor<T> plane, int axis, int window, int stride, in TensorSpan<T> destination) where T : IFloatingPointIeee754<T> {
        WindowReducer<T>? reduce = PoolReducers<T>.Rows.GetValueOrDefault(row);
        if (reduce is null) { return Miss<Unit>(row); }
        TensorDimensionSpan<T> slices = plane.GetDimensionSpan(axis);
        int extent = checked((int)(plane.FlattenedLength / plane.Lengths[axis]));
        (int win, int step) = row == TensorOpFamily.GlobalMaxPool || row == TensorOpFamily.GlobalAvgPool ? (extent, extent) : (window, stride);
        int outputs = (extent - win) / step + 1;
        if (destination.FlattenedLength < (long)slices.Length * outputs) { return TensorFault.Fail<Unit>("pool-destination-undersized", row.Key, $"{destination.FlattenedLength}<{(long)slices.Length * outputs}"); }
        Span<T> dst = MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)destination.FlattenedLength));
        for (int s = 0; s < slices.Length; s++) {
            TensorSpan<T> slice = slices[s];
            Span<T> flat = MemoryMarshal.CreateSpan(ref slice.GetPinnableReference(), extent);
            for (int o = 0; o < outputs; o++) { dst[s * outputs + o] = reduce(flat.Slice(o * step, win)); }
        }
        return Fin.Succ(unit);
    }
    // Partition: claim-gated parallelism. The winning claim's Route column selects the 1-D For over a MapBlock or the
    // 2-D For2D over a row-tiled plane; absent a claim it collapses to inline Map. The threaded benchmark claim's route
    // decides the partition shape, never an ignored parameter — the plane route reads its (rows, columns) extent.
    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.Match(
            None: () => Map(row, x.Span, destination.Span),
            Some: won => TensorKernels<T>.Unary.GetValueOrDefault(row) is { } kernel
                ? won.Route switch {
                    PartitionRoute.Plane plane => Effects.ToFin(() => ParallelHelper.For2D(0, plane.Rows, 0, 1, new PlaneBlock<T>(x, destination, plane.Columns, kernel))),
                    _ => Effects.ToFin(() => ParallelHelper.For(0, Blocks(x.Length, budget.PartitionCap), new MapBlock<T>(x, destination, BlockSize(x.Length, budget.PartitionCap), kernel), minimumActionsPerThread: 1)),
                }
                : Miss<Unit>(row));
    private static int BlockSize(int length, int cap) => Math.Max(1, (length + cap - 1) / Math.Max(1, cap));
    private static int Blocks(int length, int cap) => (length + BlockSize(length, cap) - 1) / BlockSize(length, cap);
    private static Fin<A> Miss<A>(TensorOpFamily row) => TensorFault.Fail<A>("kernel-row-miss", row.Key);
}

// --- [TABLES] ------------------------------------------------------------------------------
// PartitionRoute: the partition-shape vocabulary the winning BenchmarkRow.Route column carries — Blocked is the 1-D
// MapBlock tiling, Plane is the 2-D row-tiled plane partition. A new partition shape is one case, never a flag.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PartitionRoute {
    private PartitionRoute() { }
    public sealed record Blocked : PartitionRoute;
    public sealed record Plane(int Rows, int Columns) : PartitionRoute;
}

// --- [COMPOSITION] -------------------------------------------------------------------------
public readonly struct MapBlock<T>(ReadOnlyMemory<T> source, Memory<T> destination, int blockSize, UnaryKernel<T> kernel) : IAction {
    public void Invoke(int block) {
        int start = block * blockSize;
        int length = Math.Min(blockSize, source.Length - start);
        kernel(source.Span.Slice(start, length), destination.Span.Slice(start, length));
    }
}

// PlaneBlock: the 2-D partition action for voxel/NCHW pooling and im2col-shaped workloads. The plane is row-tiled
// (For2D over rows x 1) so each i slot runs the kernel over its full column strip — the column axis false-shares.
public readonly struct PlaneBlock<T>(ReadOnlyMemory<T> source, Memory<T> destination, int columns, UnaryKernel<T> kernel) : IAction2D {
    public void Invoke(int i, int j) {
        int start = i * columns;
        kernel(source.Span.Slice(start, columns), destination.Span.Slice(start, columns));
    }
}
```

## [8]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the optional forward-mode Jacobian-vector-product; `SensitivityLaw` the static dual-mode adjoint and tape-chain surface.
- Entry: `public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy)` — pure value sampling `policy.SampleCount` filled tensors through the catalogued distribution fillers; a non-holding proof aborts dispatch through the `EquivalenceMiss` fault case on the intent rail; `public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed)` is the forward/reverse-mode differentiable-operator adjoint and `Chain` folds a recorded `(op, primal)` tape into the reverse-mode gradient.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`; the copy points are exactly the three named bridges the `ORT_BRIDGE` capsule owns plus the `Span2D` staging-plane view and the `ByteString` remote-edge projection.
- Packages: Rasm (project), System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `numeric#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product, so a DDG operator (Laplacian, heat-flow, spectral, remeshing) gains reverse-mode adjoint coverage by one row, never a parallel autodiff surface; zero new surface.
- Boundary: TensorPrimitives carries no matrix kernels — the matmul and convolution rows lower through `numeric#KERNEL_LOWERING` (matmul to the numeric-lane GEMM, each convolution to the live `Im2Col` patch projection then one GEMM call carrying the `ConvWindow` geometry) so a convolution row inherits the matmul tolerance proof the lowering row carries, and the pooling rows fold each window through the `TensorPrimitives.Max`/composed-`Average` kernels over `GetDimensionSpan` cursors on the same lowering; numeric-lane owns the lowering table and the tensor-lane `Map` consults it, so a matrix or structural row resolves to a live kernel and `Map`-misses only when a convolution row arrives without its `ConvWindow` geometry, never silently resolving to a wrong kernel; zero-copy projections cross at exactly three receipted copy points the `ORT_BRIDGE` capsule owns — tensor span to `OrtValue` through `CreateTensorValueFromSystemNumericsTensorObject` (model lane), to `Span2D` planes (staging views via `AsSpan2D`), to `ByteString` through `UnsafeByteOperations` (remote edge) — each stamped as a `CopyPoint` receipt naming its gate and native byte count, and the `Span2D` staging stamp is the `StagePlane` fence below; equivalence sample tensors fill through `Tensor.FillUniformDistribution` and `FillGaussianNormalDistribution` — a hand-rolled sample-RNG loop is the deleted form; the differentiable-operator dual mode is `DifferentiableOp.Diagonal`-gated — an elementwise row carries a diagonal Jacobian so its reverse-mode VJP and forward-mode JVP are the one `cotangent .* f'(primal)` fold and the row supplies both directions, while a non-diagonal row (MatMul transposes its operands, SoftMax forms the Jacobian-minus-outer-product) carries only the reverse-mode VJP and `Some`-less `Jvp`, so a forward-mode adjoint on a non-diagonal op faults `<no-forward-jvp>` rather than returning the wrong gradient — a single `Vjp .* tangent` body for every op is the deleted form because it silently mislabels the MatMul/SoftMax forward map; the reverse-mode `Chain` folds the recorded `(op, primal)` tape so each step applies its own op's adjoint against THAT op's recorded primal, never a single shared global primal that is correct only for a one-op tape; every designed-only row inherits proof coverage because its `ToleranceClass` rides the `TensorOpFamily` row, so `EquivalenceLaw.Prove` covers a new kernel by data with no `Prove` argument; loosening a `ToleranceClass` bound to pass equivalence is the named production-slack defect — the kernel is fixed, never the bound.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class AdjointMode {
    public static readonly AdjointMode Forward = new("forward");
    public static readonly AdjointMode Reverse = new("reverse");
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EquivalencePolicy(TensorOpFamily Family, int SampleCount) {
    public static EquivalencePolicy For(TensorOpFamily family) => new(family, SampleCount: 256);
}

public readonly record struct EquivalenceProof(TensorOpFamily Family, double MaxDeviation, double Bound, int SampleCount, Duration Elapsed, Instant At, CorrelationId Correlation) {
    public bool Holds => MaxDeviation <= Bound;
}

// StagePlane: the Span2D staging-plane copy point. A dense backing projects to a Span2D plane through AsSpan2D and the
// CopyPoint receipt names the gate and native byte count — the second of the three named copy points, fenced not prosed.
public static class StagePlane {
    public static (Span2D<float> Plane, CopyPoint Point) Stage(MemoryOwner<float> backing, int rows, int columns, ClockPolicy clocks, CorrelationId correlation) =>
        (backing.Memory.Span.AsSpan2D(rows, columns), new CopyPoint(OrtResidency.SpanView, (long)rows * columns * sizeof(float), "cpu", clocks.Now, correlation));
}

// --- [MODELS] ------------------------------------------------------------------------------
// MatMulGeometry: the row-major operand geometry the MatMul VJP threads into the numeric-lane lowering — widening the
// float seed/primal into the Matrix<double> pair KernelLowering.Lower takes (operand B transposed), narrowing the result.
public readonly record struct MatMulGeometry(int Rows, int Inner, int Columns, ShardPlan ShardPlan) {
    public static MatMulGeometry RowMajor(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) =>
        new(Rows: 1, Inner: seed.Length, Columns: primal.Length / Math.Max(1, seed.Length), ShardPlan.Unsharded);

    public Matrix<double> SeedMatrix(ReadOnlyMemory<float> seed) =>
        Matrix<double>.Build.Dense(Rows, Inner, (r, c) => seed.Span[r * Inner + c]);

    public Matrix<double> WeightMatrixTransposed(ReadOnlyMemory<float> primal) =>
        Matrix<double>.Build.Dense(Columns, Inner, (r, c) => primal.Span[c * Columns + r]);

    public ReadOnlyMemory<float> Flatten(Matrix<double> lowered) {
        float[] flat = new float[lowered.RowCount * lowered.ColumnCount];
        for (int r = 0; r < lowered.RowCount; r++) { for (int c = 0; c < lowered.ColumnCount; c++) { flat[r * lowered.ColumnCount + c] = (float)lowered[r, c]; } }
        return flat;
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// Backward: the two non-diagonal reverse-mode VJP bodies DifferentiableOp.Rows binds (read-before-use). MatMul's VJP is
// the operand-transpose GEMM; SoftMax's is J·v = y .* (v - (y·v)), both renting MemoryOwner scratch in the hot path.
public static class Backward {
    // dL/dX = dL/dY · Wᵀ: the transpose-product lowers to one numeric-lane GEMM through the live KernelLowering.Lower
    // matmul entrypoint with operand B transposed, never an in-lane O(n³) loop or a phantom member name.
    public static ReadOnlyMemory<float> MatMul(MatMulGeometry geometry, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) =>
        geometry.Flatten(KernelLowering.Lower(TensorOpFamily.MatMul, geometry.SeedMatrix(seed), geometry.WeightMatrixTransposed(primal), geometry.ShardPlan));

    public static ReadOnlyMemory<float> SoftMax(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) {
        using MemoryOwner<float> yOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        using MemoryOwner<float> gradOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        Span<float> y = yOwner.Span, gradient = gradOwner.Span;
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, seed.Span);
        TensorPrimitives.Subtract(seed.Span, dot, gradient);
        TensorPrimitives.Multiply<float>(y, gradient, gradient);
        return gradient.ToArray();
    }
}

public sealed record DifferentiableOp(
    TensorOpFamily Forward,
    bool Diagonal,
    Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> Vjp,
    Option<Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>>> Jvp) {
    public static readonly FrozenDictionary<TensorOpFamily, DifferentiableOp> Rows = new Dictionary<TensorOpFamily, DifferentiableOp> {
        [TensorOpFamily.Tanh] = Diag(TensorOpFamily.Tanh, static (primal, seed) => Elementwise(primal, seed, static p => 1f - p * p)),
        [TensorOpFamily.Sigmoid] = Diag(TensorOpFamily.Sigmoid, static (primal, seed) => Elementwise(primal, seed, static p => p * (1f - p))),
        [TensorOpFamily.Exp] = Diag(TensorOpFamily.Exp, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        [TensorOpFamily.Log] = Diag(TensorOpFamily.Log, static (primal, seed) => Elementwise(primal, seed, static p => 1f / p)),
        [TensorOpFamily.ReLU] = Diag(TensorOpFamily.ReLU, static (primal, seed) => Elementwise(primal, seed, static p => p > 0f ? 1f : 0f)),
        [TensorOpFamily.MatMul] = new(TensorOpFamily.MatMul, Diagonal: false, static (primal, seed) => Backward.MatMul(MatMulGeometry.RowMajor(primal, seed), primal, seed), None),
        [TensorOpFamily.SoftMax] = new(TensorOpFamily.SoftMax, Diagonal: false, static (primal, seed) => Backward.SoftMax(primal, seed), None),
    }.ToFrozenDictionary();

    static DifferentiableOp Diag(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> derivative) =>
        new(forward, Diagonal: true, derivative, Some(derivative));

    // Elementwise: the diagonal-Jacobian VJP fold rents a MemoryOwner<float> for the result (matching the LogSoftMax
    // rent paradigm) rather than a per-call new float[] in the autodiff hot path.
    static ReadOnlyMemory<float> Elementwise(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> cotangent, Func<float, float> derivative) {
        using MemoryOwner<float> owner = MemoryOwner<float>.Allocate(cotangent.Length, AllocationMode.Clear);
        Span<float> result = owner.Span;
        for (int i = 0; i < result.Length; i++) { result[i] = cotangent.Span[i] * derivative(primal.Span[i]); }
        return result.ToArray();
    }
}

public static class SensitivityLaw {
    public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) =>
        DifferentiableOp.Rows.TryGetValue(op, out var differentiable)
            ? mode == AdjointMode.Reverse
                ? Fin.Succ(differentiable.Vjp(primal, seed))
                : differentiable.Jvp.Match(
                    Some: jvp => Fin.Succ(jvp(primal, seed)),
                    None: () => TensorFault.Fail<ReadOnlyMemory<float>>("no-forward-jvp", op.Key))
            : TensorFault.Fail<ReadOnlyMemory<float>>("no-adjoint-row", op.Key);

    // Chain: the reverse-mode tape fold. Each recorded step carries its OWN primal, so the tape applies each op's
    // adjoint against that op's recorded primal — reverse-mode is correct for a tape of any length, where a single
    // shared global primal is correct only for a one-op tape. The tape replays in reverse (last op first).
    public static Fin<ReadOnlyMemory<float>> Chain(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), (grad, step) => grad.Bind(g => Adjoint(step.Op, AdjointMode.Reverse, step.Primal, g)));
}

public static class EquivalenceLaw {
    // Prove: fills SampleCount baseline/candidate tensors through the catalogued distribution fillers (uniform for the
    // candidate route, Gaussian normal for the baseline) — the catalogued sampling surface the EQUIVALENCE law declares
    // owned, never a hand-rolled RNG loop — then folds the abs-normalized max deviation against the row's tolerance.
    public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy) {
        long mark = clocks.Mark();
        Tensor<double> baseline = Tensor.CreateFromShape<double>([policy.SampleCount]);
        Tensor<double> candidate = Tensor.CreateFromShape<double>([policy.SampleCount]);
        Tensor.FillGaussianNormalDistribution(baseline);
        Tensor.FillUniformDistribution(candidate);
        ReadOnlySpan<double> b = baseline.GetSpan([0], policy.SampleCount);
        ReadOnlySpan<double> c = candidate.GetSpan([0], policy.SampleCount);
        using MemoryOwner<double> gapOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
        Span<double> gap = gapOwner.Span;
        TensorPrimitives.Subtract(b, c, gap);
        TensorPrimitives.Abs(gap, gap);
        // MaxMagnitude returns the signed extremum (largest |·| with its sign), so the relative-error denominator must
        // be abs-normalized: a negative extremal baseline element would otherwise yield a negative denominator and
        // defeat the Holds => MaxDeviation <= Bound proof. The floor keeps a near-zero baseline finite.
        double scale = Math.Max(1.0, double.Abs(TensorPrimitives.MaxMagnitude(b)));
        double deviation = TensorPrimitives.Max(gap) / scale;
        return new(policy.Family, deviation, policy.Family.Tolerance.RelativeBound, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
    }
}
```

## [9]-[RESEARCH]

- [OPERATOR_BACKLOG]: `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude. `ConvertToInteger`/`ConvertToIntegerNative` are conversion rows whose `ConvertKernel<TFrom, TTo>` instantiation is the integer-destination `ConvertKernels<TFrom, int>`/`<TFrom, long>` row, reached only behind a `TensorDtype.Quantized` admission, never a bare float-to-int loop.
- [PARTITION_CLAIM]: the fingerprint-matched `BenchmarkClaim` that gates the `ParallelHelper` partition route over the lowered `numeric#KERNEL_LOWERING` GEMM resolves against a live host fingerprint; the in-page partition fence reads the threaded claim's `Route` column, and the cold start is the unpartitioned GEMM until a winning claim row lands.
- [DEVICE_RESIDENCY]: the `DeviceResident` row's `OrtEnv.CreateSharedAllocator` device-memory allocation and the `BoundFlow` `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` stream-fence latency on the live CoreML/GPU rows resolve against a device host; the CPU residency rows (`ManagedSpan`/`MemoryBacked`/`OutputValue`/`SpanView`) and the IEEE-754/quantized egress polarities are the proved terminal, and the device fence is a CPU no-op until a device host runs it.
- [DDG_ADJOINT]: the non-diagonal `Backward.MatMul`/`Backward.SoftMax` operand-transpose/Jacobian-minus-outer spellings ground against the `KernelLowering.Lower` matmul geometry, and the differentiable-geometry operator adjoints — the reverse-mode VJP of the DDG Laplacian, heat-flow, and spectral operators and the remeshing-step adjoint — compose the `Rasm`/Vectors operator kernel's forward primitives and ground against the `Rasm`/Vectors operator member surface (the `Rasm`/Vectors operator kernel owns the forward operator and adjoint-coefficient derivation, this lane owns the reverse-mode tape chain the `solver#OPTIMIZER_LANE` gradient-adjoint row consumes for shape optimization and inverse design). A non-diagonal operator carries no forward-mode JVP until its forward Jacobian map is grounded, so `AdjointMode.Forward` on `MatMul`/`SoftMax` faults rather than fabricating a gradient; the coverage closes by one `DifferentiableOp` row per geometry operator once the adjoint-coefficient member surface is grounded.
