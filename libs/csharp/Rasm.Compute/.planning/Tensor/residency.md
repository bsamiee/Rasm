# [COMPUTE_RESIDENCY]

The ONNX C-data residency bridge and the geometry-to-tensor encoding: one `OrtResidency` lattice over the five copy gates (managed-span → memory-backed value → device residency → output value → span view) binding each `OrtValue` C-data factory and projection member as the zero-copy tensor-flow boundary, the `BoundFlow` `OrtIoBinding` steady-state capsule, and the `GeometryEncoding` cases as the canonical geometry-ML input vocabulary with their packing kernels as declared boundary capsules. The page owns the `OrtResidency`/`CopyPoint`/`DeviceMemory` residency vocabulary, the `TensorBridge` C-data capsule, the `BoundFlow` steady-state composition, the `EncodingChannel`/`GeometryEncoding` encoding axes, the `EncodedTensor` carrier, and the `GeometryPacking` boundary capsule; the `OrtValue` carrier rides `Microsoft.ML.OnnxRuntime`, host geometry enters through `Rasm` and folds inside `GeometryPacking`, and the `TensorDtype`/`TensorFault`/`TensorKeyPolicy` and the `TensorVocabulary.OrtByteSpan` egress-size law arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY`. The `OrtResidency`/`CopyPoint` residency lattice serves the `Model/inference#INFERENCE_MODES` OrtValue-only run law and the `EncodedTensor` free-dimension rows feed the model-lane admission.

## [01]-[INDEX]

- [01]-[ORT_BRIDGE]: `OrtResidency` lattice; `OrtValue` C-data factories; zero-copy ingress/egress; IoBinding loop.
- [02]-[GEOMETRY_ENCODING]: geometry-to-tensor cases; packing capsules; free-dimension names; wire-shape rows.

## [02]-[ORT_BRIDGE]

- Owner: `TensorBridge` (boundary capsule); `OrtResidency` `[SmartEnum<string>]` over the five copy gates; `BoundFlow` the `OrtIoBinding` steady-state tensor-flow capsule.
- Entry: `public static Fin<OrtValue> Ingress<T>(Tensor<T> source, OrtResidency gate)` / the pooled-backing, foreign-pointer, and device-allocator overloads / `public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination)` / `public static Fin<BoundFlow> Bind(InferenceSession session, string inputName, string outputName, ReadOnlySpan<long> shape)` — `Fin<T>` aborts when the residency gate forbids the carrier or the egress destination is undersized; the bound flow is a disposable capsule leasing one `OrtIoBinding` over pinned input and output values with a `ClearBound*` rebind path for a shape-class transition.
- Receipt: every ingress and egress materializes the named copy point as `TensorRun` receipt evidence at the sink edge — the gate symbol, the byte count from `GetTensorSizeInBytes`, and the residency device from `GetTensorMemoryInfo` — keyed by `CorrelationId`, never payload bytes.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new accelerator is one `OrtResidency` row plus its `OrtValue` factory column; a new copy point is one residency gate, never a per-call marshal helper; zero new surface.
- Boundary: `OrtValue` is the sole model-boundary carrier and `TensorBridge` owns the C-data residency lattice the `Model/inference#INFERENCE_MODES` OrtValue-only run law consumes — the lattice runs managed-span → memory-backed value → device residency → output value → span view, the residency gate value (`Wraps`/`Device`/`ProjectsInPlace`) selecting the ingress and egress factory by data; per-call dictionary marshal and managed dense-tensor copies are the deleted rows; `NamedOnnxValue`, `DisposableNamedOnnxValue.CreateFromOrtValue`, and `FixedBufferOnnxValue` are the superseded spellings the OrtValue-only law deletes. The four ingress polarities ride the residency discriminant: a `Wraps` non-device gate admits the `System.Numerics` tensor owner directly through `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)` and pins a rented `MemoryOwner<T>` backing through `CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])`; the `Device` gate admits a ref-rooted foreign pointer through `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` over a `TensorMarshal.GetReference`/`Tensor<T>.GetPinnableReference` root (the pooled-plane, model-output, device-buffer admission path the `DeviceResident` row owns) and allocates a device sink through `CreateAllocatedTensorValue(OrtAllocator, TensorElementType, long[])` against an `OrtEnv.Instance().CreateSharedAllocator(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, options)` allocation over the extended `OrtMemoryInfo(string, OrtMemoryInfoDeviceType, uint vendorId, int deviceId, OrtDeviceMemoryType, ulong alignment, OrtAllocatorType)` descriptor — `OrtMemoryInfo.DefaultInstance` is the CPU residency descriptor. The backing must outlive the value and every run binding it, the value's dispose IS the owner's release point, and freeing under a live value is a use-after-free in managed code that no allocation profiler attributes. Egress is projection, never copy, and the dtype-width column selects the egress polarity: an IEEE-754 row projects in place through `GetTensorDataAsTensorSpan<T>()` (`ReadOnlyTensorSpan<T>`, read polarity) and `GetTensorMutableDataAsTensorSpan<T>()` (`TensorSpan<T>`, write polarity, same-start aliasing law), while a quantized/sub-stride row (int8/uint8/bool/string) projects through the flat-span `GetTensorDataAsSpan<T>()`/`GetTensorMutableDataAsSpan<T>()` polarities and the raw-byte `GetTensorMutableRawData()` (`Span<byte>`) for the byte-stride egress no `TensorSpan<T>` widening serves; the egress destination sizes from `GetTensorSizeInBytes` through `TensorVocabulary.OrtByteSpan`, never re-multiplied dimensions; result collections are deterministic-dispose native material invisible to GC heap heuristics — one dispose releases every element, a leaked collection a native leak. The `BoundFlow` capsule is the steady-state row for repeated same-shape tensor flow — `BindInput`/`BindOutput` once, `RunWithBinding` per call with zero marshal, the four zero-allocation columns declared together (binding, pinned shapes, `EnableMemoryPattern`, pre-allocated `CreateAllocatedTensorValue` outputs); `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` bracket every run unconditionally (no-ops on CPU, stream fences on device), a shape-class transition rebinds through `ClearBoundInputs`/`ClearBoundOutputs` against a freshly leased plane, the steady-state multi-output read rides `GetOutputValues()` (`IDisposableReadOnlyCollection<OrtValue>`), `BindOutputToDevice(string, OrtMemoryInfo)` chains a device-resident output with no host round trip, and the allocate-per-call spelling turns tensor flow into a GC workload. The string-tensor copy point rides `CreateTensorWithEmptyStrings` then `CreateFromStringTensor`, the one legal string copy.

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
    public static Fin<OrtValue> Ingress<T>(Tensor<T> source, OrtResidency gate) where T : unmanaged =>
        gate.Device
            ? TensorFault.Fail<OrtValue>("device-residency-needs-pointer", gate.Key)
        : gate.Wraps
            ? Fin.Succ(OrtValue.CreateTensorValueFromSystemNumericsTensorObject(source))
            : TensorFault.Fail<OrtValue>("residency-forbids-wrap", gate.Key);

    public static Fin<OrtValue> Ingress<T>(MemoryOwner<T> backing, ReadOnlySpan<long> shape) where T : unmanaged =>
        Fin.Succ(OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, backing.Memory, shape.ToArray()));

    public static Fin<OrtValue> Ingress(OrtMemoryInfo memory, TensorDtype row, ReadOnlySpan<long> shape, nint data, long sizeInBytes) =>
        Fin.Succ(OrtValue.CreateTensorValueWithData(memory, row.Element, shape.ToArray(), data, sizeInBytes));

    public static Fin<(OrtAllocator Allocator, OrtValue Sink)> DeviceSink(DeviceMemory device, TensorDtype row, ReadOnlySpan<long> shape) =>
        Fin.Succ(OrtEnv.Instance().CreateSharedAllocator(device.Device, device.MemoryType, device.AllocatorType, options: null))
            .Map(allocator => (allocator, OrtValue.CreateAllocatedTensorValue(allocator, row.Element, shape.ToArray())));

    public static Fin<Unit> Egress<T>(OrtValue value, in TensorSpan<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count != destination.FlattenedLength
                    ? TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.FlattenedLength}")
                : row.Quantized
                    ? Effects.ToFin(() => value.GetTensorDataAsSpan<T>().CopyTo(MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)destination.FlattenedLength))))
                    : Effects.ToFin(() => value.GetTensorDataAsTensorSpan<T>().CopyTo(destination))));

    public static Fin<Unit> EgressFlat<T>(OrtValue value, Span<T> destination) where T : unmanaged =>
        TensorVocabulary.Admit(value.GetTensorTypeAndShape().ElementDataType).Bind(row =>
            TensorVocabulary.OrtByteSpan(row, value.GetTensorSizeInBytes()).Bind(count =>
                count == destination.Length
                    ? Effects.ToFin(() => value.GetTensorDataAsSpan<T>().CopyTo(destination))
                    : TensorFault.Fail<Unit>("egress-undersized", row.Key, $"{count}!={destination.Length}")));

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

    public Fin<Unit> Flow(ReadOnlySpan<float> input, in TensorSpan<float> output) {
        input.CopyTo(plane.Span);
        binding.SynchronizeBoundInputs();
        session.RunWithBinding(run, binding);
        binding.SynchronizeBoundOutputs();
        return TensorBridge.Egress(sink, output);
    }

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

    public Fin<Unit> RebindToDevice(OrtMemoryInfo device) {
        binding.ClearBoundOutputs();
        binding.BindOutputToDevice(outputName, device);
        return Fin.Succ(unit);
    }

    public IDisposableReadOnlyCollection<OrtValue> Outputs() => binding.GetOutputValues();

    public void Dispose() { run.Dispose(); binding.Dispose(); bound.Dispose(); sink.Dispose(); plane.Dispose(); }
}
```

## [03]-[GEOMETRY_ENCODING]

- Owner: `GeometryEncoding`
- Cases: `PointCloud(VectorCloud, Option<CloudNeighborhoodPcaResult>, Option<ReadOnlyMemory<float>>)` | `MeshPatch(MeshSpace)` | `VoxelGrid(ReadOnlyMemory<float>, Dimension, Dimension, Dimension, VolumeGridPolicy)` | `BrepPatch(ReadOnlyMemory<float>, ReadOnlyMemory<float>, int, int, Dimension, Dimension)`
- Entry: `public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default)` — `Fin<T>` aborts when rank or free-dimension names miss the case row; `public static Fin<EncodedTensor> Pack(GeometryEncoding source, Seq<(string Name, long Extent)> freeDimensions = default)` is the boundary-capsule entry folding host geometry coordinates into the canonical tensor inside the capsule, so host geometry types never enter lane signatures.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new encoding is one `GeometryEncoding` case with its `CaseRow` arm — the `Row` projection (dtype/layout/wire-shape/free-dim names) and the `ChannelRows` projection both derive from the one case-row tuple, so a new encoding adds one case-row arm, not two parallel Switch folds — the `BrepPatch` case carries the NURBS control-point grid, knot vectors, and U/V degrees as the canonical B-rep/NURBS encoding feeding the model and solver lanes; a new feature channel is one `EncodingChannel` row carrying its width column (the `weight` row admits rational NURBS control-point weights and the `occupancy` row admits the bit-packed voxel occupancy plane, closing the axis at eight); zero new surface.
- Boundary: packing kernels are the page's declared boundary capsules beside the union — host geometry coordinate access stays inside `GeometryPacking` and host geometry types never enter lane signatures, the capsule projecting coordinates and the present feature channels into a rented `MemoryOwner<float>` plane through `BitHelper.ExtractRange`/`SetRange` ulong occupancy/material-id words for the voxel row and `Tensor.Create` over the `DangerousGetArray` seam for the contiguous rows; each case's `CaseRow` carries the model-zoo conformance triad — named `WireShape`, `LayoutForm`, declared free-dimension rank — and the `FeatureWidth` it advertises equals the width of the channels the packer materializes, so `Of` rejects a payload whose rank or free-dimension names miss that triad and the packer projects every advertised channel (point-cloud normals and color, voxel occupancy, NURBS weights) into the plane so the packed feature width matches; the free-dimension rows feed the model-lane `AddFreeDimensionOverrideByName` admission and the wire-shape names mirror one-to-one onto the remote-lane proto geometry family; mesh face indices ride the int64 row as `Tensor<long>`; voxel grids ride nchw with z-slices as channel planes and pack occupancy/material-id through the multi-bit `BitHelper.SetRange`/`ExtractRange` words threaded into an occupancy channel, never a `bool[]`; `FeatureWidth` folds the `EncodingChannel` widths present on the payload, where `curvature`, `geodesic`, and `intensity` widen the channel axis as SmartEnum rows — a `PointCloudV2` sibling case is the rejected anticipatory form; the voxel occupancy bit-pack and the NURBS control-point/knot/degree thread carry the named boundary-capsule statement-form exemption inside `GeometryPacking`.

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

    public static Fin<EncodedTensor> Pack(GeometryEncoding source, Seq<(string Name, long Extent)> freeDimensions = default) =>
        GeometryPacking.Project(source).Bind(packed => Of(source, packed.Values, packed.Indices, freeDimensions));
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
public static class GeometryPacking {
    public static Fin<(Tensor<float> Values, Option<Tensor<long>> Indices)> Project(GeometryEncoding source) =>
        source.Switch<Fin<(Tensor<float>, Option<Tensor<long>>)>>(
            pointCloud: static c => Pack(c).Map(static t => (t, Option<Tensor<long>>.None)),
            meshPatch: static m => PackMesh(m.Source),
            voxelGrid: static v => PackVoxels(v).Map(static t => (t, Option<Tensor<long>>.None)),
            brepPatch: static b => PackNurbs(b).Map(static t => (t, Option<Tensor<long>>.None)));

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

    private static int ChannelColumns(Span<float> plane, int n, int width, int column, ReadOnlySpan<float> channel, int channelWidth) {
        for (int r = 0; r < n; r++) { channel.Slice(r * channelWidth, channelWidth).CopyTo(plane.Slice(r * width + column, channelWidth)); }
        return column + channelWidth;
    }

    private static Fin<(Tensor<float>, Option<Tensor<long>>)> PackMesh(MeshSpace source) =>
        Fin.Succ((Tensor.Create(source.Vertices.ToArray(), [(nint)source.VertexCount, 3]), Some(Tensor.Create(source.FaceIndices.ToArray(), [(nint)source.FaceCount, 3]))));

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

## [04]-[RESEARCH]

- [SOLVER_FIELD_TOOLPATH_ENCODING]: the `Field` and `Toolpath` representations the `AppHost/Sandbox/solver#SOLVER_KIND` `EncodingKind` solver and CAM-post kind contracts speak land as two `GeometryEncoding` case extensions on this owner — a `Field` case carrying the discretization `FieldSpace` sample grid and a `Toolpath` case carrying the ordered cut-segment stream — each a single `CaseRow` arm with its `WireShape`/`LayoutForm`/free-dimension triad and its `EncodingChannel` set, never a solver-page literal and never a fifth feature axis. The four geometry rows (`PointCloud`/`MeshPatch`/`VoxelGrid`/`BrepPatch`) the `EncodingKind` axis already projects onto are finalized; the residual is the two case-row arms whose packing kernels finalize against their source owners — the `Field` arm against the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` station grid, the `Toolpath` arm against the `Rasm.Fabrication/Toolpath/motion#CAM_MOTION` `Motion` cut-segment stream the `Rasm.Fabrication/Posting/program#CUT_PROGRAM` emitter consumes — read as settled vocabulary and folded inside `GeometryPacking`, never re-minted; once both arms resolve the `EncodingKind.Field`/`.Toolpath` working rows bind to settled cases and the negotiation reads the case-level encoding for all seven plugin kinds.
