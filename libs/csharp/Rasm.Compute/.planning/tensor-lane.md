# [COMPUTE_TENSOR_LANE]

The cpu-tensor execution vocabulary and operation algebra: `Tensor<T>` spans and factories as the only tensor shapes, one dtype map between `TensorElementType` and CLR carriers, one `LayoutForm` algebra over the layout member surface, geometry-to-tensor encoding rows as the canonical geometry-ML input vocabulary, one `TensorOpFamily` table of eighty-two rows over twelve `TensorOpKind` rows under the closed `ToleranceClass` band, the arity kernel-delegate dispatch binding each row to its TensorPrimitives member, and the equivalence law proving lane kernels against Rasm baselines. The page owns the `TensorDtype`, `LayoutForm`, `EncodingChannel`, `GeometryEncoding`, `TensorOpKind`, `TensorOpFamily`, and `ToleranceClass` axes, the `TensorKeyPolicy` accessor, and the kernel registries; AppHost `ClockPolicy` and `CorrelationId` arrive settled, and the substrate row, fault union, and receipt cases ride their owning pages.

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
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are the deleted phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans plus `Tensor.Create` over rented `MemoryOwner<T>` arrays through the `DangerousGetArray` seam; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime is the caller's proof obligation, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` the ref roots; one generic kernel serves each operation family — per-dtype kernel copies are the deleted form; quantized rows project through `ConvertSaturating` with `QuantizationPolicy` values; the string row admits only at the model boundary for tokenizer extension ops.

```csharp signature
public sealed class TensorKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

public readonly record struct QuantizationPolicy(double Scale, int ZeroPoint);

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
- Cases: `PointCloud(VectorCloud, Option<CloudNeighborhoodPcaResult>, Option<ReadOnlyMemory<float>>)` | `MeshPatch(MeshSpace)` | `VoxelGrid(ReadOnlyMemory<float>, Dimension, Dimension, Dimension, VolumeGridPolicy)`
- Entry: `public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default)` — `Fin<T>` aborts when rank or free-dimension names miss the case row.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new encoding is one `GeometryEncoding` case with its `Row` and `ChannelRows` arms; a new feature channel is one `EncodingChannel` row carrying its width column (six rows close the axis); zero new surface.
- Boundary: packing kernels are the page's declared boundary capsules beside the union — host geometry coordinate access stays inside the capsule and host geometry types never enter lane signatures; each case's `Row` carries the model-zoo conformance triad — named `WireShape`, `LayoutForm`, declared free-dimension rank — and `Of` rejects a payload missing that triad, so the conformance gate is the case row, never an external architecture name; the free-dimension rows feed the model-lane `AddFreeDimensionOverrideByName` admission and the wire-shape names mirror one-to-one onto the remote-lane proto geometry family; mesh face indices ride the int64 row as `Tensor<long>`; voxel grids ride nchw with z-slices as channel planes and pack occupancy through `BitHelper` bit-flag words, never a `bool[]`; `FeatureWidth` folds the `EncodingChannel` widths present on the payload, where `curvature`, `geodesic`, and `intensity` widen the channel axis as SmartEnum rows — a `PointCloudV2` sibling case is the rejected anticipatory form.

```csharp signature
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

    public int Width { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryEncoding {
    private GeometryEncoding() { }

    public sealed record PointCloud(VectorCloud Source, Option<CloudNeighborhoodPcaResult> Normals, Option<ReadOnlyMemory<float>> Colors) : GeometryEncoding;

    public sealed record MeshPatch(MeshSpace Source) : GeometryEncoding;

    public sealed record VoxelGrid(ReadOnlyMemory<float> Cells, Dimension Channels, Dimension Height, Dimension Width, VolumeGridPolicy Grid) : GeometryEncoding;

    public (TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<string> FreeDimensionNames) Row =>
        Switch(
            pointCloud: static _ => (TensorDtype.Float32, LayoutForm.NxC, "PointCloudTensor", Seq("N", "C")),
            meshPatch: static _ => (TensorDtype.Float32, LayoutForm.VertexFace, "MeshTensor", Seq("V", "F")),
            voxelGrid: static _ => (TensorDtype.Float32, LayoutForm.Nchw, "VoxelGridTensor", Seq("C", "H", "W")));

    public Seq<EncodingChannel> ChannelRows =>
        Switch(
            pointCloud: static c => Seq1(EncodingChannel.Position) + c.Normals.Map(static _ => Seq(EncodingChannel.Normal, EncodingChannel.Curvature, EncodingChannel.Geodesic)).IfNone(Seq<EncodingChannel>()) + c.Colors.Map(static _ => EncodingChannel.ColorRgba).ToSeq(),
            meshPatch: static _ => Seq1(EncodingChannel.Position),
            voxelGrid: static _ => Seq1(EncodingChannel.Intensity));

    public int FeatureWidth => ChannelRows.Sum(static channel => channel.Width);
}

public sealed record EncodedTensor(Tensor<float> Values, Option<Tensor<long>> Indices, TensorDtype Dtype, LayoutForm Layout, string WireShape, Seq<(string Name, long Extent)> FreeDimensions) {
    public static Fin<EncodedTensor> Of(GeometryEncoding source, Tensor<float> values, Option<Tensor<long>> indices = default, Seq<(string Name, long Extent)> freeDimensions = default) =>
        values.Rank == source.Row.Layout.Rank && freeDimensions.Map(static d => d.Name) == source.Row.FreeDimensionNames
            ? Fin.Succ(new EncodedTensor(values, indices, source.Row.Dtype, source.Row.Layout, source.Row.WireShape, freeDimensions))
            : Fin.Fail<EncodedTensor>(ComputeFault.Create($"<encoding-shape-miss:{source.Row.WireShape}>"));
}
```

## [5]-[OPERATION_TABLE]

- Owner: `TensorOpFamily`
- Cases: eighty-two rows across twelve `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural — each carrying its `ToleranceClass` equivalence column
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind and tolerance columns; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row; zero new surface.
- Boundary: the `ToleranceClass` vocabulary closes the equivalence axis — exact (integer and predicate rows), tight (fused-triad and reduction rows), transcendental (ULP-banded same-route rows), statistical (accumulation-scaled rows) — and a row's `Tolerance` column is the equivalence proof key the `EquivalenceLaw` reads by data, never a `Prove` argument; the eighty-two rows partition exactly across the kind axis and the matrix/structural designed-only rows hold no member until their Rasm route lands; the table keys through the ordinal `TensorKeyPolicy` so every binding index resolves the same comparer.

```csharp signature
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
    public static readonly TensorOpFamily GlobalAvgPool = new("global-avg-pool", TensorOpKind.Structural, ToleranceClass.Statistical);
    public static readonly TensorOpFamily MaskedWrite = new("masked-write", TensorOpKind.Structural, ToleranceClass.Exact);

    public TensorOpKind Kind { get; }
    public ToleranceClass Tolerance { get; }
}
```

## [6]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination)` — `Fin<Unit>` aborts on a kernel-row miss; the arity siblings `Zip`, `Fuse`, `Dual`, `Bits`, `Shift`, `Population`, `Convert`, `ToHalf`, `Root`, `Fold`, `FoldPair`, `IndexOf`, `Polarity`, `Test`, `Hamming`, `HammingBits`, and `Mask` dispatch the same `TensorOpFamily` table.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a windowed convolution or pooling kernel is one row bound to its designed Rasm route, never a span-kernel entry; zero new surface.
- Boundary: every span row binds the TensorPrimitives member matching its Pascal-cased key (shift-right binds the arithmetic-shift member, shift-right-logical the logical member) and only the `MaskedWrite` row carries a full-tensor binding — `SetSlice`/`FilteredUpdate` over `NRange`/`NIndex` through the `Mask` arity; the predicate rows write non-`T` destinations — `Sign` fills `Span<int>` through `Polarity`, `IsNaN`/`IsFinite` fill `Span<bool>` through `Test`; `SinCos` writes the two destination spans through `Dual` under the `ITrigonometricFunctions<T>` constraint; `RootN`/`ScaleB` take the integer parameter through `Root` under `IRootFunctions<T>`; `PopCount`/`LeadingZeroCount`/`TrailingZeroCount`/`OnesComplement` ride the integer `Population` arity, `BitwiseOr`/`Xor` the integer binary table, and `RotateLeft`/`RotateRight`/`ShiftRightLogical` the integer shift table; `ConvertToHalf` binds the fixed `(float→Half)` `ToHalf` row and `HammingBitDistance` the `HammingBits` integer-pair reduce; the `MatMul`, `Conv1D`/`Conv2D`/`Conv3D`, and `MaxPool`/`AvgPool`/`GlobalAvgPool` rows hold no TensorPrimitives or in-package member, carry no binding-table entry, and `Map` returns a `<kernel-row-miss>` `Fin.Fail` for each until its designed Rasm route lands behind a winning benchmark-claim row — the matrix rows route to the Rasm `Matrix.Multiply` im2col lowering and the pooling rows to the strided-window fold over `GetDimensionSpan` cursors; the binding tables are `FrozenDictionary` indexes keyed through the ordinal `TensorKeyPolicy`, span kernels iterate rows by reference through `RefEnumerable<T>`/`SpanEnumerable<T>` rather than flat indexing, and integer dtypes enter the real-constrained entries through the conversion rows first.

```csharp signature
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

public static class TensorKernels<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate, [TensorOpFamily.Abs] = TensorPrimitives.Abs, [TensorOpFamily.Round] = TensorPrimitives.Round,
        [TensorOpFamily.Floor] = TensorPrimitives.Floor, [TensorOpFamily.Ceiling] = TensorPrimitives.Ceiling, [TensorOpFamily.Truncate] = TensorPrimitives.Truncate,
        [TensorOpFamily.Exp] = TensorPrimitives.Exp, [TensorOpFamily.Log] = TensorPrimitives.Log, [TensorOpFamily.Sin] = TensorPrimitives.Sin, [TensorOpFamily.Cos] = TensorPrimitives.Cos,
        [TensorOpFamily.Tanh] = TensorPrimitives.Tanh, [TensorOpFamily.Sigmoid] = TensorPrimitives.Sigmoid, [TensorOpFamily.SoftMax] = TensorPrimitives.SoftMax,
        [TensorOpFamily.Sqrt] = TensorPrimitives.Sqrt, [TensorOpFamily.Cbrt] = TensorPrimitives.Cbrt, [TensorOpFamily.DegreesToRadians] = TensorPrimitives.DegreesToRadians,
        [TensorOpFamily.Reciprocal] = TensorPrimitives.Reciprocal, [TensorOpFamily.ReciprocalSqrt] = TensorPrimitives.ReciprocalSqrt,
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
        [TensorOpFamily.Max] = TensorPrimitives.Max, [TensorOpFamily.Norm] = TensorPrimitives.Norm, [TensorOpFamily.Average] = TensorPrimitives.Average,
        [TensorOpFamily.StdDev] = TensorPrimitives.StdDev, [TensorOpFamily.SumOfSquares] = TensorPrimitives.SumOfSquares, [TensorOpFamily.SumOfMagnitudes] = TensorPrimitives.SumOfMagnitudes,
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

public static class TensorOps {
    public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IFloatingPointIeee754<T> {
        UnaryKernel<T>? kernel = TensorKernels<T>.Unary.GetValueOrDefault(row);
        kernel?.Invoke(x, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Zip<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IFloatingPointIeee754<T> {
        BinaryKernel<T>? kernel = TensorKernels<T>.Binary.GetValueOrDefault(row);
        kernel?.Invoke(x, y, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Fuse<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination) where T : IFloatingPointIeee754<T> {
        TernaryKernel<T>? kernel = TensorKernels<T>.Ternary.GetValueOrDefault(row);
        kernel?.Invoke(x, y, z, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Bits<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IBinaryInteger<T> {
        BinaryKernel<T>? kernel = IntegerKernels<T>.Binary.GetValueOrDefault(row);
        kernel?.Invoke(x, y, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Shift<T>(TensorOpFamily row, ReadOnlySpan<T> x, int shiftCount, Span<T> destination) where T : IBinaryInteger<T> {
        ShiftKernel<T>? kernel = IntegerKernels<T>.Shift.GetValueOrDefault(row);
        kernel?.Invoke(x, shiftCount, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Convert<TFrom, TTo>(TensorOpFamily row, ReadOnlySpan<TFrom> source, Span<TTo> destination) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> {
        ConvertKernel<TFrom, TTo>? kernel = ConvertKernels<TFrom, TTo>.Rows.GetValueOrDefault(row);
        kernel?.Invoke(source, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<T> Fold<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Fold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<T>(row);
    public static Fin<T> FoldPair<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.PairFold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x, y)) : Miss<T>(row);
    public static Fin<int> IndexOf<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Index.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<int>(row);
    public static Fin<Unit> Dual<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> first, Span<T> second) where T : ITrigonometricFunctions<T> {
        DualKernel<T>? kernel = TensorKernels<T>.Dual.GetValueOrDefault(row);
        kernel?.Invoke(x, first, second);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Polarity<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : INumberBase<T> {
        SignKernel<T>? kernel = TensorKernels<T>.Sign.GetValueOrDefault(row);
        kernel?.Invoke(x, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Test<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<bool> destination) where T : INumberBase<T> {
        MaskKernel<T>? kernel = TensorKernels<T>.Mask.GetValueOrDefault(row);
        kernel?.Invoke(x, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Population<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IBinaryInteger<T> {
        UnaryKernel<T>? kernel = IntegerKernels<T>.Unary.GetValueOrDefault(row);
        kernel?.Invoke(x, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Unit> Root<T>(TensorOpFamily row, ReadOnlySpan<T> x, int n, Span<T> destination) where T : IFloatingPointIeee754<T>, IRootFunctions<T> =>
        row switch {
            _ when row == TensorOpFamily.RootN => Tap(() => TensorPrimitives.RootN(x, n, destination)),
            _ when row == TensorOpFamily.ScaleB => Tap(() => TensorPrimitives.ScaleB(x, n, destination)),
            _ => Miss<Unit>(row),
        };
    public static Fin<int> Hamming<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IEquatable<T> => Fin.Succ(TensorPrimitives.HammingDistance(x, y));
    public static Fin<long> HammingBits<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IBinaryInteger<T> => Fin.Succ(TensorPrimitives.HammingBitDistance(x, y));
    public static Fin<Unit> ToHalf(TensorOpFamily row, ReadOnlySpan<float> source, Span<Half> destination) {
        ConvertKernel<float, Half>? kernel = HalfConvertKernels.Rows.GetValueOrDefault(row);
        kernel?.Invoke(source, destination);
        return kernel is null ? Miss<Unit>(row) : Fin.Succ(unit);
    }
    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<T> values, ReadOnlySpan<NRange> region) where T : unmanaged =>
        row == TensorOpFamily.MaskedWrite ? Fin.Succ(Tensor.SetSlice(destination, values, region)) : Miss<Tensor<T>>(row);
    private static Fin<Unit> Tap(Action kernel) { kernel(); return Fin.Succ(unit); }
    private static Fin<A> Miss<A>(TensorOpFamily row) => Fin.Fail<A>(ComputeFault.Create($"<kernel-row-miss:{row.Key}>"));
}
```

## [7]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`
- Entry: `public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy, ReadOnlySpan<double> baseline, ReadOnlySpan<double> candidate)` — pure value; a non-holding proof aborts dispatch through the `EquivalenceMiss` fault case on the intent rail.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`.
- Packages: Rasm (project), System.Numerics.Tensors, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row bound to its Rasm im2col route and pooling as one structural-kind row bound to the strided-window route; zero new surface.
- Boundary: TensorPrimitives carries no matrix kernels — `Matrix.Multiply` (Rasm) is the designed route behind the matmul row and the convolution rows, an im2col patch projection lowering each convolution to one `Matrix.Multiply` call so a convolution row inherits the matmul tolerance proof once its route lands, and the pooling rows fold each window through the `TensorPrimitives.Max`/`Average` kernels over `GetDimensionSpan` cursors on the same designed route; until those routes land each matrix and pooling row `Map`-misses, never silently resolving to a wrong kernel; zero-copy projections cross at three receipted copy points — tensor span to `OrtValue` through `CreateTensorValueFromSystemNumericsTensorObject` (model lane), to `Span2D` planes (staging views), to `ByteString` through `UnsafeByteOperations` (remote edge); equivalence sample tensors fill through `Tensor.FillUniformDistribution` and `FillGaussianNormalDistribution` — a hand-rolled sample-RNG loop is the deleted form; every designed-only row inherits proof coverage because its `ToleranceClass` rides the `TensorOpFamily` row, so `EquivalenceLaw.Prove` covers a new kernel by data with no `Prove` argument; loosening a `ToleranceClass` bound to pass equivalence is the named production-slack defect — the kernel is fixed, never the bound.

```csharp signature
public sealed record EquivalencePolicy(TensorOpFamily Family, int SampleCount) {
    public static EquivalencePolicy For(TensorOpFamily family) => new(family, SampleCount: 256);
}

public readonly record struct EquivalenceProof(TensorOpFamily Family, double MaxDeviation, double Bound, int SampleCount, Duration Elapsed, Instant At, CorrelationId Correlation) {
    public bool Holds => MaxDeviation <= Bound;
}

public static class EquivalenceLaw {
    public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy, ReadOnlySpan<double> baseline, ReadOnlySpan<double> candidate) {
        long mark = clocks.Mark();
        double[] gap = new double[baseline.Length];
        TensorPrimitives.Subtract(baseline, candidate, gap);
        TensorPrimitives.Abs(gap, gap);
        double deviation = TensorPrimitives.Max(gap) / Math.Max(1.0, TensorPrimitives.MaxMagnitude(baseline));
        return new(policy.Family, deviation, policy.Family.Tolerance.RelativeBound, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
    }
}
```

## [8]-[RESEARCH]

- [OPERATOR_BACKLOG]: the `MinNumber`/`MaxNumber` NaN-as-missing reduction pair stays uncatalogued against the package surface and lands as two reduction `TensorOpFamily` rows on the `Fold` table once the member spellings confirm in the tensor catalogue; `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude.
- [PARALLEL_PARTITION]: the `ParallelHelper.For` span-partition surface for kernel fan-out becomes a default execution path only behind a winning benchmark-claim row inside the shared processor-budget record, after the surface is confirmed as a rail entry rather than an unbudgeted partition helper.
