# [COMPUTE_VOCABULARY]

The cpu-tensor vocabulary and the operation table: `Tensor<T>` spans and factories as the only tensor shapes, one `TensorDtype` map between `TensorElementType` and CLR carriers carrying the ONNX byte-width and quantization columns, and one `TensorOpFamily` table of one-hundred-thirteen rows over thirteen `TensorOpKind` rows under the closed `ToleranceClass` band as the equivalence-keyed operation catalogue. The page owns the `TensorDtype`/`QuantizationPolicy` vocabulary, the `TensorFault` bracketed-symbol fault family, the `TensorKeyPolicy` ordinal accessor, the `TensorVocabulary` admission fold with the `OrtByteSpan` egress-size law, and the `TensorOpKind`/`TensorOpFamily`/`ToleranceClass` operation axes; the dtype carriers ride `System.Numerics.Tensors` and `Microsoft.ML.OnnxRuntime`, the matrix rows lower through `Tensor/factor#KERNEL_LOWERING`, and the kernel-delegate bindings are owned by `Tensor/dispatch#KERNEL_DISPATCH`. The `TensorDtype`/`TensorFault`/`TensorKeyPolicy`/`TensorOpFamily` shapes cross to `Tensor/residency#ORT_BRIDGE`, `Tensor/layout#LAYOUT_ALGEBRA`, and `Tensor/dispatch#KERNEL_DISPATCH` as settled vocabulary.

## [01]-[INDEX]

- [01]-[TENSOR_VOCABULARY]: tensor shapes, factories, dtype map, ONNX byte-width, quantization policy.
- [02]-[OPERATION_TABLE]: `TensorOpKind`/`TensorOpFamily`/`ToleranceClass` vocabulary rows.

## [02]-[TENSOR_VOCABULARY]

- Owner: `TensorDtype`
- Cases: float32, float64, float16, bfloat16, int8, uint8, int32, int64, bool, string
- Entry: `public static Fin<TensorDtype> Admit(TensorElementType element)` — `Fin<T>` aborts on an unmapped element; the quantization overload rejects scale/zero-point values on unquantized rows; the `OrtByteSpan` fold sizes a `Span<byte>` egress destination from `GetTensorSizeInBytes` against the row width, never re-multiplied dimensions.
- Packages: System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element mapping is one `TensorDtype` row; the byte-width and quantization columns derive from the row; zero new surface.
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are the deleted phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans plus `Tensor.Create` over rented `MemoryOwner<T>` arrays through the `DangerousGetArray` seam; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime is the caller's proof obligation, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` the ref roots; one generic kernel serves each operation family — per-dtype kernel copies are the deleted form; the `Width` column carries the CLR byte width and `OrtElementBytes` the ONNX C-data byte stride (`bfloat16`/`float16` are two-byte even though the CLR carrier widens), so a `GetTensorSizeInBytes` destination sizes from the dtype row, never `sizeof(T)`; quantized rows project through `ConvertSaturating` with `QuantizationPolicy` values; a chunked tensor frame requiring one contiguous backing stages through the `Tensor/memory#ALLOCATION_AXIS` contiguous-frame route (the `asContiguousBuffer:true` `GetStream` overload), never a hand-rolled array concatenation; the string row admits only at the model boundary for tokenizer extension ops through `OrtValue.CreateTensorWithEmptyStrings` then `CreateFromStringTensor`.

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

    public static Fin<int> OrtByteSpan(TensorDtype row, long sizeInBytes) =>
        row.ElementCount(sizeInBytes).Match(
            Some: count => Fin.Succ(checked((int)count)),
            None: () => TensorFault.Fail<int>("no-byte-stride", row.Key));
}
```

## [03]-[OPERATION_TABLE]

- Owner: `TensorOpFamily`
- Cases: one-hundred-thirteen rows across thirteen `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural, geometry — each carrying its `ToleranceClass` equivalence column; the activation family (`ReLU`, `Gelu`, `SiLU`, `LogSoftMax` beside the direct `Sigmoid`/`Tanh`/`SoftMax` members), the four pooling rows (`MaxPool`/`AvgPool`/`GlobalMaxPool`/`GlobalAvgPool`), the magnitude-extremum reduction pair (`MaxMagnitude`/`MinMagnitude`), the fast-estimate elementwise rows (`ReciprocalEstimate`/`ReciprocalSqrtEstimate`), the widening conversion rows (`ConvertToSingle`/`ConvertToInteger`/`ConvertToIntegerNative`), the predicate-aggregate rows (`IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny`), the element-domain rows (`ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` over `System.Numerics.Complex`, `QuaternionMultiply`/`QuaternionConjugate`/`QuaternionNormalize` over `System.Numerics.Quaternion`), and the six DDG geometry-operator rows (`Gradient`/`Divergence`/`Curl`/`CotangentLaplacian`/`HeatFlow`/`Spectral`) carrying the linear DEC operators whose reverse-mode adjoint is the operator transpose over the `Tensor/dispatch#EQUIVALENCE_INTEROP` `Backward.Operator` apply, ride the existing kind axis as rows, never sibling op types
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind and tolerance columns; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row; zero new surface.
- Boundary: the `ToleranceClass` vocabulary closes the equivalence axis — exact (integer and predicate rows), tight (fused-triad and reduction rows), transcendental (ULP-banded same-route rows), statistical (accumulation-scaled rows), estimate (platform-variant fast-reciprocal rows with no cross-machine bound) — and a row's `Tolerance` column is the equivalence proof key the `EquivalenceLaw` reads by data, never a `Prove` argument; the estimate rows carry the `Estimate` band because `ReciprocalEstimate`/`ReciprocalSqrtEstimate` legitimately differ across machines and are inadmissible wherever cross-machine bit agreement is contractual, paired against their exact `Reciprocal`/`ReciprocalSqrt` rows; `MinNumber`/`MaxNumber` are the NaN-as-missing reduction pair distinct from the NaN-propagating `Min`/`Max` rows, binding the `T.MinNumber`/`T.MaxNumber` reduction members on the `Fold` table, and `MaxMagnitude`/`MinMagnitude` reduce by signed absolute extremum on the same table; `IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` are the boolean-aggregate predicate rows reducing to one `bool` distinct from the per-element `IsNaN`/`IsFinite` mask rows; the four structural pooling rows fold in-lane through the shared strided-window `Pool` kernel over the verified `TensorPrimitives.Max`/`Average` window reducers on the rank-aware `GetDimensionSpan` cursor, while the matrix rows (`MatMul`, `Conv1D`/`Conv2D`/`Conv3D`) carry no `TensorPrimitives` member and lower through `Tensor/factor#KERNEL_LOWERING` (matmul to GEMM, convolution to im2col); the six geometry rows (`Gradient`/`Divergence`/`Curl`/`CotangentLaplacian`/`HeatFlow`/`Spectral`) carry no `TensorPrimitives` member and no `Tensor/factor#KERNEL_LOWERING` route — they lower to the `Rasm`/Vectors `Spectral.cs` `OperatorRow` forward apply over a mesh snapshot, so a geometry row resolves only inside the `Tensor/dispatch#EQUIVALENCE_INTEROP` differentiable-operator path where the recorded primal carries the snapshot, never as a bare span kernel on the `Map` dispatch table, and the geometry `ToleranceClass.Tight`/`Transcendental` bands key the reverse-mode adjoint proof against the self-adjoint/transpose identity rather than a kernel-versus-baseline span comparison; the table keys through the ordinal `TensorKeyPolicy` so every binding index resolves the same comparer.

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
    public static readonly TensorOpKind Geometry = new("geometry");
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
    public static readonly TensorOpFamily Gradient = new("gradient", TensorOpKind.Geometry, ToleranceClass.Tight);
    public static readonly TensorOpFamily Divergence = new("divergence", TensorOpKind.Geometry, ToleranceClass.Tight);
    public static readonly TensorOpFamily Curl = new("curl", TensorOpKind.Geometry, ToleranceClass.Tight);
    public static readonly TensorOpFamily CotangentLaplacian = new("cotangent-laplacian", TensorOpKind.Geometry, ToleranceClass.Tight);
    public static readonly TensorOpFamily HeatFlow = new("heat-flow", TensorOpKind.Geometry, ToleranceClass.Tight);
    public static readonly TensorOpFamily Spectral = new("spectral", TensorOpKind.Geometry, ToleranceClass.Transcendental);

    public TensorOpKind Kind { get; }
    public ToleranceClass Tolerance { get; }
}
```
