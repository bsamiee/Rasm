# [COMPUTE_TENSOR_LANE]

| [OWNER]            | [AXES]                                                                                                  | [STATE] | [DEPTH]                                          |
| ------------------ | ------------------------------------------------------------------------------------------------------ | :-----: | ----------------------------------------------- |
| `tensor-lane`      | `TensorDtype` · `LayoutForm` · `EncodingChannel` · `GeometryEncoding` · `TensorOpKind` · `TensorOpFamily` · `ToleranceClass` · `TensorOps` · `DifferentiableOp` | SPIKE   | 96 families / 12 kinds / 5 layouts; 5 fences     |

[STATE] is SPIKE because three tier-3 LIVE-HOST residuals remain unresolved against the running numeric-lane and `Rasm`/Vectors surfaces; each is a named [8]-[RESEARCH] cluster entry: (a) the fingerprint-matched partition `BenchmarkClaim` gating the partition route over the lowered GEMM on the live host (`[KERNEL_LOWERING]`), (b) the cross-folder `Backward.MatMul`/`Backward.SoftMax` non-diagonal VJP spellings (`[DDG_ADJOINT]`), (c) the DDG operator adjoints (Laplacian, heat-flow, spectral, remeshing) grounded against the `Rasm`/Vectors operator member surface (`[DDG_ADJOINT]`). The five fences are transcription-complete; FINALIZED is withheld until these probes land.

The cpu-tensor execution vocabulary and operation algebra: `Tensor<T>` spans and factories as the only tensor shapes, one dtype map between `TensorElementType` and CLR carriers, one `LayoutForm` algebra over the layout member surface, geometry-to-tensor encoding rows as the canonical geometry-ML input vocabulary, one `TensorOpFamily` table of ninety-six rows over twelve `TensorOpKind` rows under the closed `ToleranceClass` band, the arity kernel-delegate dispatch binding each row to its TensorPrimitives member, the claim-gated `ParallelHelper` partition column, and the equivalence law proving lane kernels against Rasm baselines. The page owns the `TensorDtype`, `LayoutForm`, `EncodingChannel`, `GeometryEncoding`, `TensorOpKind`, `TensorOpFamily`, and `ToleranceClass` axes, the `TensorKeyPolicy` accessor, and the kernel registries; AppHost `ClockPolicy`, `CorrelationId`, and `CpuBudget` arrive settled, the matrix and structural rows lower through `numeric-lane#KERNEL_LOWERING`, and the substrate row, fault union, and receipt cases ride their owning pages.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                |
| :-----: | ------------------- | --------------------------------------------------------------------- |
|   [1]   | TENSOR_VOCABULARY   | Tensor shapes, factories, dtype map, quantization policy              |
|   [2]   | LAYOUT_ALGEBRA      | LayoutForm rows; permute table; layout member surface                 |
|   [3]   | GEOMETRY_ENCODING   | Geometry-to-tensor cases; free-dimension names; wire-shape rows       |
|   [4]   | OPERATION_TABLE     | TensorOpKind/TensorOpFamily/ToleranceClass vocabulary rows            |
|   [5]   | KERNEL_DISPATCH     | Arity kernel-delegate tables; one TensorOps dispatch surface          |
|   [6]   | EQUIVALENCE_INTEROP | Equivalence proofs against Rasm kernels; matmul route; copy-point law |

## [2]-[TENSOR_VOCABULARY]

- Owner: `TensorDtype`
- Cases: float32, float64, float16, bfloat16, int8, uint8, int32, int64, bool, string
- Entry: `public static Fin<TensorDtype> Admit(TensorElementType element)` — `Fin<T>` aborts on an unmapped element; the quantization overload rejects scale/zero-point values on unquantized rows.
- Packages: System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element mapping is one `TensorDtype` row; zero new surface.
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are the deleted phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans plus `Tensor.Create` over rented `MemoryOwner<T>` arrays through the `DangerousGetArray` seam; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime is the caller's proof obligation, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` the ref roots; one generic kernel serves each operation family — per-dtype kernel copies are the deleted form; quantized rows project through `ConvertSaturating` with `QuantizationPolicy` values; a chunked tensor frame requiring one contiguous backing stages through the `staging-and-streams#ALLOCATION_AXIS` contiguous-frame route (the `asContiguousBuffer:true` `GetStream` overload), never a hand-rolled array concatenation; the string row admits only at the model boundary for tokenizer extension ops.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype Float32 = new("float32", TensorElementType.Float, typeof(float), width: Some(4), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Float64 = new("float64", TensorElementType.Double, typeof(double), width: Some(8), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Float16 = new("float16", TensorElementType.Float16, typeof(Half), width: Some(2), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype BFloat16 = new("bfloat16", TensorElementType.BFloat16, typeof(Microsoft.ML.OnnxRuntime.BFloat16), width: Some(2), quantized: false, modelBoundaryOnly: true);
    public static readonly TensorDtype Int8 = new("int8", TensorElementType.Int8, typeof(sbyte), width: Some(1), quantized: true, modelBoundaryOnly: false);
    public static readonly TensorDtype UInt8 = new("uint8", TensorElementType.UInt8, typeof(byte), width: Some(1), quantized: true, modelBoundaryOnly: false);
    public static readonly TensorDtype Int32 = new("int32", TensorElementType.Int32, typeof(int), width: Some(4), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Int64 = new("int64", TensorElementType.Int64, typeof(long), width: Some(8), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Bool = new("bool", TensorElementType.Bool, typeof(bool), width: Some(1), quantized: false, modelBoundaryOnly: false);
    public static readonly TensorDtype Utf8Text = new("string", TensorElementType.String, typeof(string), width: None, quantized: false, modelBoundaryOnly: true);

    public TensorElementType Element { get; }
    public Type Clr { get; }
    public Option<int> Width { get; }
    public bool Quantized { get; }
    public bool ModelBoundaryOnly { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct QuantizationPolicy(double Scale, int ZeroPoint);

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
        ByElement.TryGetValue(element, out TensorDtype? row) ? Fin.Succ(row!) : Fin.Fail<TensorDtype>(ComputeFault.Create($"<unmapped-element:{element}>"));

    public static Fin<TensorDtype> Admit(TensorElementType element, Option<QuantizationPolicy> quantization) =>
        Admit(element).Bind(row => quantization.IsSome && !row.Quantized ? Fin.Fail<TensorDtype>(ComputeFault.Create($"<quantization-on-unquantized-row:{row.Key}>")) : Fin.Succ(row));
}
```

## [3]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm`
- Cases: dense, nxc, vertex-face, nchw, nhwc
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` — `Fin<T>` aborts on an undeclared permute row.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new layout is one `LayoutForm` row plus one permute-table entry; zero new surface.
- Boundary: the layout family — `PermuteDimensions`, `Transpose`, `Squeeze`, `SqueezeDimension`, `Unsqueeze`, `SetSlice`, `Split`, `Stack`, `StackAlongDimension`, `Concatenate`, `ConcatenateOnDimension`, `Reverse`, `Resize`, `Broadcast`, `BroadcastTo`, `Reshape`, `FlattenTo`, `ToDenseTensor`, and `Slice` with `NIndex`/`NRange` — is the only layout surface, replacing the deleted phantom construction factories; the nchw↔nhwc permute rows are the mandatory CoreML image-model pre/post route; `Span2D` planes are views and never substitute for rank permutation; broadcast compatibility and rank/stride invariants ride `Broadcast`/`BroadcastTo` and the dimension spans, stated here once for the lane.

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
            : Fin.Fail<Tensor<T>>(ComputeFault.Create($"<no-permute-row:{origin.Key}->{target.Key}>"));
}
```

## [4]-[GEOMETRY_ENCODING]

- Owner: `GeometryEncoding`
- Cases: `PointCloud(VectorCloud, Option<CloudNeighborhoodPcaResult>, Option<ReadOnlyMemory<float>>)` | `MeshPatch(MeshSpace)` | `VoxelGrid(ReadOnlyMemory<float>, Dimension, Dimension, Dimension, VolumeGridPolicy)` | `BrepPatch(ReadOnlyMemory<float>, ReadOnlyMemory<float>, int, int, Dimension, Dimension)`
- Entry: `public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default)` — `Fin<T>` aborts when rank or free-dimension names miss the case row.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new encoding is one `GeometryEncoding` case with its `Row` and `ChannelRows` arms — the `BrepPatch` case carries the NURBS control-point grid, knot vectors, and U/V degrees as the canonical B-rep/NURBS encoding feeding the model and solver lanes; a new feature channel is one `EncodingChannel` row carrying its width column (the `weight` row admits rational NURBS control-point weights, closing the axis at seven); zero new surface.
- Boundary: packing kernels are the page's declared boundary capsules beside the union — host geometry coordinate access stays inside the capsule and host geometry types never enter lane signatures; each case's `Row` carries the model-zoo conformance triad — named `WireShape`, `LayoutForm`, declared free-dimension rank — and `Of` rejects a payload missing that triad, so the conformance gate is the case row, never an external architecture name; the free-dimension rows feed the model-lane `AddFreeDimensionOverrideByName` admission and the wire-shape names mirror one-to-one onto the remote-lane proto geometry family; mesh face indices ride the int64 row as `Tensor<long>`; voxel grids ride nchw with z-slices as channel planes and pack occupancy through `BitHelper` bit-flag words, never a `bool[]`; `FeatureWidth` folds the `EncodingChannel` widths present on the payload, where `curvature`, `geodesic`, and `intensity` widen the channel axis as SmartEnum rows — a `PointCloudV2` sibling case is the rejected anticipatory form.

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

    public (TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames) Row =>
        Switch(
            pointCloud: static _ => (TensorDtype.Float32, LayoutForm.NxC, "PointCloudTensor", Seq("N", "C")),
            meshPatch: static _ => (TensorDtype.Float32, LayoutForm.VertexFace, "MeshTensor", Seq("V", "F")),
            voxelGrid: static _ => (TensorDtype.Float32, LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W")),
            brepPatch: static _ => (TensorDtype.Float32, LayoutForm.NxC, "NurbsControlTensor", Seq("U", "V")));

    public Seq<EncodingChannel> ChannelRows =>
        Switch(
            pointCloud: static c => Seq1(EncodingChannel.Position) + c.Normals.Map(static _ => Seq(EncodingChannel.Normal, EncodingChannel.Curvature, EncodingChannel.Geodesic)).IfNone(Seq<EncodingChannel>()) + c.Colors.Map(static _ => EncodingChannel.ColorRgba).ToSeq(),
            meshPatch: static _ => Seq1(EncodingChannel.Position),
            voxelGrid: static _ => Seq1(EncodingChannel.Intensity),
            brepPatch: static _ => Seq(EncodingChannel.Position, EncodingChannel.Weight));

    public int FeatureWidth => ChannelRows.Sum(static channel => channel.Width);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EncodedTensor(Tensor<float> Values, Option<Tensor<long>> Indices, TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<(string Name, long Extent)> FreeDimensions) {
    public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default) =>
        values.Rank == source.Row.Layout.Rank && freeDimensions.Map(static d => d.Name) == source.Row.FreeDimensionNames
            ? Fin.Succ(new EncodedTensor(values, indices, source.Row.Dtype, source.Row.Layout, source.Row.WireShape, freeDimensions))
            : Fin.Fail<EncodedTensor>(ComputeFault.Create($"<encoding-shape-miss:{source.Row.WireShape}>"));
}
```

## [5]-[OPERATION_TABLE]

- Owner: `TensorOpFamily`
- Cases: ninety-six rows across twelve `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural — each carrying its `ToleranceClass` equivalence column; the activation family (`ReLU`, `Gelu`, `SiLU`, `LogSoftMax` beside the direct `Sigmoid`/`Tanh`/`SoftMax` members), the four pooling rows (`MaxPool`/`AvgPool`/`GlobalMaxPool`/`GlobalAvgPool`), and the element-domain rows (`ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` over `System.Numerics.Complex`, `QuaternionMultiply`/`QuaternionConjugate`/`QuaternionNormalize` over `System.Numerics.Quaternion`) ride the existing kind axis as rows, never sibling op types
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind and tolerance columns; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row; zero new surface.
- Boundary: the `ToleranceClass` vocabulary closes the equivalence axis — exact (integer and predicate rows), tight (fused-triad and reduction rows), transcendental (ULP-banded same-route rows), statistical (accumulation-scaled rows) — and a row's `Tolerance` column is the equivalence proof key the `EquivalenceLaw` reads by data, never a `Prove` argument; the ninety-six rows partition exactly across the kind axis; `MinNumber`/`MaxNumber` are the NaN-as-missing reduction pair distinct from the NaN-propagating `Min`/`Max` rows, binding the `T.MinNumber`/`T.MaxNumber` reduction members on the `Fold` table; the four structural pooling rows fold in-lane through the shared strided-window `Pool` kernel over the verified `TensorPrimitives.Max`/`Average` window reducers, while the matrix rows (`MatMul`, `Conv1D`/`Conv2D`/`Conv3D`) carry no `TensorPrimitives` member and lower through `numeric-lane#KERNEL_LOWERING` (matmul to GEMM, convolution to im2col); the table keys through the ordinal `TensorKeyPolicy` so every binding index resolves the same comparer.

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
    public static readonly TensorOpFamily Sign = new("sign", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNaN = new("is-nan", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFinite = new("is-finite", TensorOpKind.Predicate, ToleranceClass.Exact);
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

## [6]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination)` — `Fin<Unit>` aborts on a kernel-row miss; the arity siblings `Zip`, `Fuse`, `Dual`, `Bits`, `Shift`, `Population`, `Convert`, `ToHalf`, `Root`, `Fold`, `FoldPair`, `IndexOf`, `Polarity`, `Test`, `Hamming`, `HammingBits`, `Mask`, `Pool`, the element-domain `ComplexZip`/`ComplexMap`/`ComplexAbs` and `QuaternionZip`/`QuaternionMap`, and the claim-gated `Partition` dispatch the same `TensorOpFamily` table; `Pool` is the strided-window fold the four pooling rows share over one `PoolReducers<T>` reducer table; `Partition` reads `CpuBudget.PartitionCap` and falls through to inline `Map` when no winning claim is supplied.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a new activation is one `Activations<T>` composed fold plus one `Unary` row, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/complex method; a matrix kernel is one lowering row read from `numeric-lane#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path reading `CpuBudget.PartitionCap`, never a new owner; zero new surface.
- Boundary: every span row binds the TensorPrimitives member matching its Pascal-cased key (shift-right binds the arithmetic-shift member, shift-right-logical the logical member) and only the `MaskedWrite` row carries a full-tensor binding — `SetSlice`/`FilteredUpdate` over `NRange`/`NIndex` through the `Mask` arity; the predicate rows write non-`T` destinations — `Sign` fills `Span<int>` through `Polarity`, `IsNaN`/`IsFinite` fill `Span<bool>` through `Test`; `SinCos` writes the two destination spans through `Dual` under the `ITrigonometricFunctions<T>` constraint; `RootN`/`ScaleB` take the integer parameter through `Root` under `IRootFunctions<T>`; `PopCount`/`LeadingZeroCount`/`TrailingZeroCount`/`OnesComplement` ride the integer `Population` arity, `BitwiseOr`/`Xor` the integer binary table, and `RotateLeft`/`RotateRight`/`ShiftRightLogical` the integer shift table; `ConvertToHalf` binds the fixed `(float→Half)` `ToHalf` row and `HammingBitDistance` the `HammingBits` integer-pair reduce; `MinNumber`/`MaxNumber` bind the `TensorPrimitives.MinNumber`/`MaxNumber` reduction members (`T MinNumber<T>(ReadOnlySpan<T>)`/`T MaxNumber<T>(ReadOnlySpan<T>)` under `INumber<T>`, which the `IFloatingPointIeee754<T>` table constraint satisfies) on the `Fold` table, paired against the NaN-propagating `Min`/`Max` rows; `Average` and `StdDev` bind the direct `TensorPrimitives.Average`/`StdDev` reduction members (`T Average<T>(ReadOnlySpan<T>)` under `INumberBase<T>` and `T StdDev<T>(ReadOnlySpan<T>)` under `IRootFunctions<T>`, both satisfied by the `IFloatingPointIeee754<T>` table constraint) on the `Fold` table — a hand-composed `Sum/n` mean or `Sqrt(SumOfSquares/n − mean²)` std-dev beside the admitted members is the deleted form; the activation family binds in-lane on the `Unary` table — `Sigmoid`/`Tanh`/`SoftMax` to their direct `TensorPrimitives` members and `ReLU`/`Gelu`/`SiLU`/`LogSoftMax` to the `Activations<T>` composed author-folds (`ReLU` is `Clamp(x, 0, +inf)`, `SiLU` is `x .* Sigmoid(x)`, `Gelu` the tanh-approximation `MultiplyAdd`/`Tanh` chain, `LogSoftMax` the numerically-stable `x − logsumexp(x)` max-shift), never a per-element activation loop or a fabricated `TensorPrimitives.Relu`/`Gelu`/`SiLU`/`LogSoftmax` phantom; the four pooling rows fold in-lane through `Pool` over one `PoolReducers<T>` window-reducer table — `MaxPool`/`GlobalMaxPool` reduce each window through `TensorPrimitives.Max` and `AvgPool`/`GlobalAvgPool` through `TensorPrimitives.Average`, the global rows collapsing the whole spatial plane to one window (window = stride = length) — so pooling carries verified members and only `MatMul`/`Conv1D`/`Conv2D`/`Conv3D` hold no in-lane member; the element-domain rows ride `System.Numerics.Complex` and `System.Numerics.Quaternion` carriers — Complex arithmetic (`Add`/`Subtract`/`Multiply`/`Divide`/`Negate`) binds the verified `INumberBase<Complex>`-generic `TensorPrimitives` members through `ComplexZip`/`ComplexMap`, while `ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` are author-folds over the BCL `Complex` intrinsics (no `TensorPrimitives` Complex specialization exists) and `ComplexAbs` writes the `Span<double>` magnitude polarity through the `MagnitudeKernel`; Quaternion implements neither numeric interface so `QuaternionMultiply` (the non-commutative Hamilton product), `QuaternionConjugate`, and `QuaternionNormalize` are author-folds over the BCL `Quaternion` operators through `QuaternionZip`/`QuaternionMap`; the `MatMul`, `Conv1D`/`Conv2D`/`Conv3D` rows hold no TensorPrimitives or in-package member and `Map` resolves each through the `numeric-lane#KERNEL_LOWERING` binding table behind a winning benchmark-claim row — matmul lowers to the numeric-lane GEMM and convolution to im2col-then-GEMM — returning a `<kernel-row-miss>` `Fin.Fail` only when no lowering row is bound; the partition column dispatches a struct `IAction` span-kernel through `ParallelHelper.For<TAction>(int start, int end, in TAction action, int minimumActionsPerThread)` clamped at `CpuBudget.PartitionCap`, taken only when a winning `BenchmarkClaim` names the partition route — a partition count of one collapses inline and an unbudgeted `Parallel.For` over spans is the deleted form; the binding tables are `FrozenDictionary` indexes keyed through the ordinal `TensorKeyPolicy`, span kernels iterate rows by reference through `RefEnumerable<T>`/`SpanEnumerable<T>` rather than flat indexing, and integer dtypes enter the real-constrained entries through the conversion rows first.

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
public delegate void MagnitudeKernel(ReadOnlySpan<Complex> x, Span<double> destination);
public delegate T WindowReducer<T>(ReadOnlySpan<T> window);

// --- [OPERATIONS] --------------------------------------------------------------------------
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
    // ReLU = Clamp(x, 0, +inf): the single verified-member elementwise lower-bound, never a per-element branch loop.
    public static void ReLU(ReadOnlySpan<T> x, Span<T> destination) =>
        TensorPrimitives.Clamp(x, T.Zero, T.PositiveInfinity, destination);

    // SiLU(x) = x * Sigmoid(x): Sigmoid into the destination, then a fused in-place Multiply against the primal.
    public static void SiLU(ReadOnlySpan<T> x, Span<T> destination) {
        TensorPrimitives.Sigmoid(x, destination);
        TensorPrimitives.Multiply(destination, x, destination);
    }

    // GELU(x) = 0.5*x*(1 + Tanh(sqrt(2/pi)*(x + 0.044715*x^3))): the tanh approximation composed from verified members.
    public static void Gelu(ReadOnlySpan<T> x, Span<T> destination) {
        T c = T.Sqrt(T.CreateChecked(2) / T.Pi);
        T a = T.CreateChecked(0.044715);
        TensorPrimitives.Multiply(x, x, destination);          // x^2
        TensorPrimitives.Multiply(destination, x, destination); // x^3
        TensorPrimitives.MultiplyAdd(destination, a, x, destination); // x + a*x^3
        TensorPrimitives.Multiply(destination, c, destination); // c*(x + a*x^3)
        TensorPrimitives.Tanh(destination, destination);
        TensorPrimitives.Add(destination, T.One, destination);  // 1 + tanh(...)
        TensorPrimitives.Multiply(destination, x, destination); // x*(1 + tanh(...))
        TensorPrimitives.Multiply(destination, T.CreateChecked(0.5), destination);
    }

    // LogSoftMax(x) = x - logsumexp(x): the numerically-stable max-shift fold, never Log(SoftMax(x)). The exp scratch
    // rents from the pool (T is IFloatingPointIeee754, not unmanaged, so no stackalloc) and the rent is sized to x, not
    // destination; the Map boundary rails the x.Length == destination.Length equality before this kernel runs.
    public static void LogSoftMax(ReadOnlySpan<T> x, Span<T> destination) {
        T shift = TensorPrimitives.Max(x);
        TensorPrimitives.Subtract(x, shift, destination);         // x - max
        using MemoryOwner<T> scratch = MemoryOwner<T>.Allocate(x.Length);
        Span<T> exps = scratch.Span;
        TensorPrimitives.Exp(destination, exps);
        T logSumExp = T.Log(TensorPrimitives.Sum(exps));          // x - logsumexp = (x - max) - log(sum(exp(x - max)))
        TensorPrimitives.Subtract(destination, logSumExp, destination);
    }
}

// The four pooling rows collapse to one window-reducer table: Max for the max rows, Average for the avg rows, both
// verified TensorPrimitives reduction members; the strided-window fold in TensorOps.Pool reads this by data.
public static class PoolReducers<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, WindowReducer<T>> Rows = new Dictionary<TensorOpFamily, WindowReducer<T>> {
        [TensorOpFamily.MaxPool] = TensorPrimitives.Max, [TensorOpFamily.GlobalMaxPool] = TensorPrimitives.Max,
        [TensorOpFamily.AvgPool] = TensorPrimitives.Average, [TensorOpFamily.GlobalAvgPool] = TensorPrimitives.Average,
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

public static class HalfConvertKernels {
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<float, Half>> Rows = new Dictionary<TensorOpFamily, ConvertKernel<float, Half>> {
        [TensorOpFamily.ConvertToHalf] = TensorPrimitives.ConvertToHalf,
    }.ToFrozenDictionary();
}

// System.Numerics.Complex : INumberBase<Complex>, so the elementwise arithmetic rows ride the verified generic
// TensorPrimitives.Add/Subtract/Multiply/Divide/Negate members directly; Abs/Exp/Log/Conjugate carry no Complex
// TensorPrimitives specialization in the .api catalogue and are author-folds over System.Numerics.Complex.
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

// System.Numerics.Quaternion implements neither INumberBase nor IFloatingPointIeee754, so every quaternion op is an
// author-fold over the BCL Quaternion intrinsics; quaternion multiply is the non-commutative Hamilton product.
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
    // Miss; a present kernel runs its closure-captured invoker (the spans live in the closure, ref-struct-safe) and
    // returns Succ. Every Map/Zip/Fuse/Bits/Shift/Convert/Dual/Polarity/Test/Population/ToHalf/Complex*/Quaternion*
    // entry is one row binding its FrozenDictionary table + invoker, never a hand-copied body.
    private static Fin<Unit> Dispatch<TKernel>(TensorOpFamily row, FrozenDictionary<TensorOpFamily, TKernel> table, Action<TKernel> invoke) where TKernel : Delegate =>
        table.GetValueOrDefault(row) is { } kernel ? Tap(() => invoke(kernel)) : Miss<Unit>(row);
    // EqualLength: the one length-equality boundary rail the elementwise/destination-writing arities gate on before a
    // kernel touches a span indexer, so a mismatched destination rails <length-mismatch> rather than throwing IndexOutOfRange.
    private static Fin<Unit> EqualLength(TensorOpFamily row, int source, int destination, Fin<Unit> next) =>
        source == destination ? next : Fin.Fail<Unit>(ComputeFault.Create($"<length-mismatch:{row.Key}:{source}!={destination}>"));

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
        EqualLength(row, source.Length, destination.Length, Dispatch(row, HalfConvertKernels.Rows, k => k(source, destination)));
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
    public static Fin<int> Hamming<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IEquatable<T> => Fin.Succ(TensorPrimitives.HammingDistance(x, y));
    public static Fin<long> HammingBits<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IBinaryInteger<T> => Fin.Succ(TensorPrimitives.HammingBitDistance(x, y));

    // Root takes the integer parameter, so it dispatches over a two-row switch on the parameterised member, not a kernel table.
    public static Fin<Unit> Root<T>(TensorOpFamily row, ReadOnlySpan<T> x, int n, Span<T> destination) where T : IFloatingPointIeee754<T>, IRootFunctions<T> =>
        EqualLength(row, x.Length, destination.Length, row switch {
            _ when row == TensorOpFamily.RootN => Tap(() => TensorPrimitives.RootN(x, n, destination)),
            _ when row == TensorOpFamily.ScaleB => Tap(() => TensorPrimitives.ScaleB(x, n, destination)),
            _ => Miss<Unit>(row),
        });
    // Mask is the deliberate boundary exception: T : unmanaged (not IFloatingPointIeee754) and a full-tensor SetSlice
    // return, so it cannot ride the span-Dispatch fold and stays a single-row guard over Tensor.SetSlice.
    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<T> values, ReadOnlySpan<NRange> region) where T : unmanaged =>
        row == TensorOpFamily.MaskedWrite ? Fin.Succ(Tensor.SetSlice(destination, values, region)) : Miss<Tensor<T>>(row);
    // Pool: the strided-window fold shared by MaxPool/AvgPool/GlobalMaxPool/GlobalAvgPool. The pooling rows differ only
    // in their WindowReducer (Max vs Average over the window span) and in window extent — a global pool collapses the
    // whole spatial plane to one window (window = length, stride = length). One fold, four rows; no parallel kernels.
    public static Fin<Unit> Pool<T>(TensorOpFamily row, ReadOnlySpan<T> plane, int window, int stride, Span<T> destination) where T : IFloatingPointIeee754<T> {
        WindowReducer<T>? reduce = PoolReducers<T>.Rows.GetValueOrDefault(row); // ref-struct spans cannot close over Tap; the fold runs inline.
        if (reduce is null) { return Miss<Unit>(row); }
        (int win, int step) = row == TensorOpFamily.GlobalMaxPool || row == TensorOpFamily.GlobalAvgPool
            ? (plane.Length, plane.Length)
            : (window, stride);
        int outputs = (plane.Length - win) / step + 1;
        if (destination.Length < outputs) { return Fin.Fail<Unit>(ComputeFault.Create($"<pool-destination-undersized:{row.Key}:{destination.Length}<{outputs}>")); }
        for (int o = 0; o < outputs; o++) { destination[o] = reduce(plane.Slice(o * step, win)); }
        return Fin.Succ(unit);
    }
    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.IsNone
            ? Map(row, x.Span, destination.Span)
        : TensorKernels<T>.Unary.GetValueOrDefault(row) is { } kernel
            ? Tap(() => ParallelHelper.For(0, Blocks(x.Length, budget.PartitionCap), new MapBlock<T>(x, destination, BlockSize(x.Length, budget.PartitionCap), kernel), minimumActionsPerThread: 1))
            : Miss<Unit>(row);
    private static int BlockSize(int length, int cap) => Math.Max(1, (length + cap - 1) / Math.Max(1, cap));
    private static int Blocks(int length, int cap) => (length + BlockSize(length, cap) - 1) / BlockSize(length, cap);
    private static Fin<Unit> Tap(Action kernel) { kernel(); return Fin.Succ(unit); }
    private static Fin<A> Miss<A>(TensorOpFamily row) => Fin.Fail<A>(ComputeFault.Create($"<kernel-row-miss:{row.Key}>"));
}

// --- [COMPOSITION] -------------------------------------------------------------------------
public readonly struct MapBlock<T>(ReadOnlyMemory<T> source, Memory<T> destination, int blockSize, UnaryKernel<T> kernel) : IAction {
    public void Invoke(int block) {
        int start = block * blockSize;
        int length = Math.Min(blockSize, source.Length - start);
        kernel(source.Span.Slice(start, length), destination.Span.Slice(start, length));
    }
}
```

## [7]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the optional forward-mode Jacobian-vector-product; `SensitivityLaw` the static dual-mode adjoint and tape-chain surface.
- Entry: `public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy, ReadOnlySpan<double> baseline, ReadOnlySpan<double> candidate)` — pure value; a non-holding proof aborts dispatch through the `EquivalenceMiss` fault case on the intent rail; `public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed)` is the forward/reverse-mode differentiable-operator adjoint and `Chain` folds a recorded op tape into the reverse-mode gradient.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `numeric-lane#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product, so a DDG operator (Laplacian, heat-flow, spectral, remeshing) gains reverse-mode adjoint coverage by one row, never a parallel autodiff surface; zero new surface.
- Boundary: TensorPrimitives carries no matrix kernels — the matmul and convolution rows lower through `numeric-lane#KERNEL_LOWERING` (matmul to the numeric-lane GEMM, each convolution to the live `Im2Col` patch projection then one GEMM call carrying the `ConvWindow` geometry) so a convolution row inherits the matmul tolerance proof the lowering row carries, and the pooling rows fold each window through the `TensorPrimitives.Max`/composed-`Average` kernels over `GetDimensionSpan` cursors on the same lowering; numeric-lane owns the lowering table and the tensor-lane `Map` consults it, so a matrix or structural row resolves to a live kernel and `Map`-misses only when a convolution row arrives without its `ConvWindow` geometry, never silently resolving to a wrong kernel; zero-copy projections cross at three receipted copy points — tensor span to `OrtValue` through `CreateTensorValueFromSystemNumericsTensorObject` (model lane), to `Span2D` planes (staging views), to `ByteString` through `UnsafeByteOperations` (remote edge); equivalence sample tensors fill through `Tensor.FillUniformDistribution` and `FillGaussianNormalDistribution` — a hand-rolled sample-RNG loop is the deleted form; the differentiable-operator dual mode is `DifferentiableOp.Diagonal`-gated — an elementwise row carries a diagonal Jacobian so its reverse-mode VJP and forward-mode JVP are the one `cotangent .* f'(primal)` fold and the row supplies both directions, while a non-diagonal row (MatMul transposes its operands, SoftMax forms the Jacobian-minus-outer-product) carries only the reverse-mode VJP and `Some`-less `Jvp`, so a forward-mode adjoint on a non-diagonal op faults `<no-forward-jvp>` rather than returning the wrong gradient — a single `Vjp .* tangent` body for every op is the deleted form because it silently mislabels the MatMul/SoftMax forward map; every designed-only row inherits proof coverage because its `ToleranceClass` rides the `TensorOpFamily` row, so `EquivalenceLaw.Prove` covers a new kernel by data with no `Prove` argument; loosening a `ToleranceClass` bound to pass equivalence is the named production-slack defect — the kernel is fixed, never the bound.

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

// --- [OPERATIONS] --------------------------------------------------------------------------
// Backward: the two non-diagonal reverse-mode VJP bodies the DifferentiableOp.Rows table binds; declared ahead of the
// table that reads it (read-before-use dependency cluster). MatMul's VJP is the operand-transpose product the
// numeric-lane GEMM lowers; SoftMax's is the Jacobian-minus-outer-product J·v = y .* (v - (y·v)) over the softmaxed primal.
public static class Backward {
    // dL/dX = dL/dY · Wᵀ for the row-major operand pair carried in the lowering row's ConvWindow-free MatMul geometry;
    // the transpose-product lowers to one numeric-lane GEMM call, never an in-lane O(n³) loop.
    public static ReadOnlyMemory<float> MatMul(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) =>
        KernelLowering.MatMulTransposeProduct(primal, seed); // numeric-lane#KERNEL_LOWERING GEMM, operand-B transposed.

    // SoftMax VJP: with y = softmax(primal) the full Jacobian J = diag(y) - y·yᵀ collapses to J·v = y .* (v - <y,v>),
    // so the outer product never materializes — one Dot for the scalar projection, one Subtract, one Multiply.
    public static ReadOnlyMemory<float> SoftMax(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) {
        float[] y = new float[primal.Length];
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, seed.Span); // <y, v>
        float[] gradient = new float[primal.Length];
        TensorPrimitives.Subtract(seed.Span, dot, gradient);   // v - <y, v>
        TensorPrimitives.Multiply<float>(y, gradient, gradient); // y .* (v - <y, v>)
        return gradient;
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
        [TensorOpFamily.MatMul] = new(TensorOpFamily.MatMul, Diagonal: false, static (primal, seed) => Backward.MatMul(primal, seed), None),
        [TensorOpFamily.SoftMax] = new(TensorOpFamily.SoftMax, Diagonal: false, static (primal, seed) => Backward.SoftMax(primal, seed), None),
    }.ToFrozenDictionary();

    static DifferentiableOp Diag(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> derivative) =>
        new(forward, Diagonal: true, derivative, Some(derivative));

    static ReadOnlyMemory<float> Elementwise(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> cotangent, Func<float, float> derivative) {
        float[] result = new float[cotangent.Length];
        for (int i = 0; i < result.Length; i++) { result[i] = cotangent.Span[i] * derivative(primal.Span[i]); }
        return result;
    }
}

public static class SensitivityLaw {
    public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed) =>
        DifferentiableOp.Rows.TryGetValue(op, out var differentiable)
            ? mode == AdjointMode.Reverse
                ? Fin.Succ(differentiable.Vjp(primal, seed))
                : differentiable.Jvp.Match(
                    Some: jvp => Fin.Succ(jvp(primal, seed)),
                    None: () => Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create($"<no-forward-jvp:{op.Key}>")))
            : Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create($"<no-adjoint-row:{op.Key}>"));

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<TensorOpFamily> tape, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), (grad, op) => grad.Bind(g => Adjoint(op, AdjointMode.Reverse, primal, g)));
}

public static class EquivalenceLaw {
    public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy, ReadOnlySpan<double> baseline, ReadOnlySpan<double> candidate) {
        long mark = clocks.Mark();
        double[] gap = new double[baseline.Length];
        TensorPrimitives.Subtract(baseline, candidate, gap);
        TensorPrimitives.Abs(gap, gap);
        // MaxMagnitude returns the signed extremum (the element of largest |·|, with its sign), so the relative-error
        // denominator must be abs-normalized: a baseline whose extremal element is negative would otherwise yield a
        // negative denominator and defeat the Holds => MaxDeviation <= Bound proof. The floor keeps a near-zero baseline finite.
        double scale = Math.Max(1.0, double.Abs(TensorPrimitives.MaxMagnitude(baseline)));
        double deviation = TensorPrimitives.Max(gap) / scale;
        return new(policy.Family, deviation, policy.Family.Tolerance.RelativeBound, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
    }
}
```

## [8]-[RESEARCH]

- [OPERATOR_BACKLOG]: `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude.
- [KERNEL_LOWERING]: the matrix and structural rows resolve through the live `numeric-lane#KERNEL_LOWERING` GEMM, `Im2Col` patch-projection, and strided-window fold against the MathNet provider; the residual is the fingerprint-matched `BenchmarkClaim` that gates a partition route over the lowered GEMM on the live host.
- [DDG_ADJOINT]: the elementwise differentiable-operator rows (Tanh, Sigmoid, Exp, Log) carry their reverse-mode VJP and forward-mode JVP as transcription-complete diagonal-Jacobian folds, and the `MatMul` reverse-mode VJP is the operand-transpose product the `numeric-lane#KERNEL_LOWERING` GEMM lowers — these need no probe; the residual cross-folder leaves are the non-diagonal `Backward.MatMul`/`Backward.SoftMax` exact operand-transpose/Jacobian-minus-outer spellings and the differentiable-geometry operator adjoints — the reverse-mode VJP of the DDG Laplacian, heat-flow, and spectral operators and the remeshing-step adjoint — which compose the `Rasm`/Vectors operator kernel's forward primitives and ground against the `Rasm`/Vectors operator member surface at cross-folder alignment (the `Rasm`/Vectors operator kernel owns the forward operator and adjoint-coefficient derivation, this lane owns the reverse-mode tape chain the `solver-and-optimization#OPTIMIZER_LANE` gradient-adjoint row consumes for shape optimization and inverse design); a non-diagonal operator carries no forward-mode JVP until its forward Jacobian map is grounded, so `AdjointMode.Forward` on `MatMul`/`SoftMax` faults rather than fabricating a gradient.
