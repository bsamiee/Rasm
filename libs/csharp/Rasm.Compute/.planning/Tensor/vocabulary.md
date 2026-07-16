# [COMPUTE_VOCABULARY]

Cpu-tensor vocabulary uses `Tensor<T>` as its only tensor owner, `TensorDtype` as the `TensorElementType`↔CLR/ONNX map, and `TensorOpFamily` as the equivalence-keyed operation table. `TensorDtype`, `QuantizationPolicy`, `TensorFault`, `TensorVocabulary`, `TensorOpKind`, `TensorOpFamily`, and `ToleranceClass` live here; matrix lowering, kernel dispatch, residency, and layout consume those settled shapes.

## [01]-[INDEX]

- [01]-[TENSOR_VOCABULARY]: tensor shapes, factories, dtype map, ONNX byte-width, quantization policy.
- [02]-[OPERATION_TABLE]: `TensorOpKind`/`TensorOpFamily`/`ToleranceClass` vocabulary rows.

## [02]-[TENSOR_VOCABULARY]

- Owner: `TensorDtype`
- Cases: float32, float64, float16, bfloat16, complex128, int8, int16, int32, int64, uint8, uint16, uint32, uint64, bool, string
- Entry: `Admit(TensorElementType)` aborts on an unmapped element; `Promote(TensorDtype, TensorDtype)` derives mixed arithmetic from each row's numeric, integral, signedness, storage, precision, and exponent-range columns, including signed/unsigned widening, float/complex escalation, and the range gate that promotes a bfloat16-float16 pair to float32 rather than truncating exponent range, without a named pair roster. Quantization admission proves scalar, axis, block, vector-cardinality, and zero-point invariants against the tensor shape. `OrtByteSpan` converts native bytes without negative, alignment, or width truncation.
- Packages: System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element mapping is one `TensorDtype` row carrying byte-width, quantization, numeric-domain, storage, precision, and exponent-range columns; admission and mixed promotion derive from `Items`, so no pair table grows.
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans plus `Tensor.Create` over rented `MemoryOwner<T>` arrays through `DangerousGetArray`; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime the caller owns, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` as ref roots; one generic kernel serves each operation family. `Width` carries CLR byte width and `OrtElementBytes` the ONNX C-data stride, so `GetTensorSizeInBytes` converts through the dtype row, never `sizeof(T)`; `OrtByteSpan` rejects negative, non-integral, and `int`-overflowing element counts before any destination slice. `Complex128` carries `System.Numerics.Complex`, while `complex64` has no BCL carrier and never admits to a span; native FP8, `Int4`/`UInt4`, and `Float4E2M1` types do not exist in managed `TensorElementType` and remain inadmissible. Quantized rows compose subtract-zero-point then multiply-scale dequantization and inverse round-add-`ConvertSaturating` quantization, broadcasting by per-tensor, per-axis, or blocked granularity. `QuantizationPolicy.Admit` receives the tensor shape, accumulates independent scalar and structural gates through tuple `Apply`, proves the axis exists, requires vector count equal to the axis extent or blocked group count, and exits once to `Fin`; no kernel revalidates metadata. Chunked contiguous frames stage through `StreamGrant.ContiguousFrame`; the string row admits only at the model boundary through `OrtValue.CreateTensorWithEmptyStrings` then `CreateFromStringTensor`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype Float32 = new("float32", TensorElementType.Float, typeof(float), width: Some(4), ortBytes: 4, quantized: false, modelBoundaryOnly: false, numeric: true, integral: false, signed: true, storageBits: 32, precisionBits: 24, exponentBits: 8);
    public static readonly TensorDtype Float64 = new("float64", TensorElementType.Double, typeof(double), width: Some(8), ortBytes: 8, quantized: false, modelBoundaryOnly: false, numeric: true, integral: false, signed: true, storageBits: 64, precisionBits: 53, exponentBits: 11);
    public static readonly TensorDtype Float16 = new("float16", TensorElementType.Float16, typeof(Half), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: false, numeric: true, integral: false, signed: true, storageBits: 16, precisionBits: 11, exponentBits: 5);
    public static readonly TensorDtype BFloat16 = new("bfloat16", TensorElementType.BFloat16, typeof(Microsoft.ML.OnnxRuntime.BFloat16), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: true, numeric: true, integral: false, signed: true, storageBits: 16, precisionBits: 8, exponentBits: 8);
    public static readonly TensorDtype Complex128 = new("complex128", TensorElementType.Complex128, typeof(System.Numerics.Complex), width: Some(16), ortBytes: 16, quantized: false, modelBoundaryOnly: false, numeric: true, integral: false, signed: true, storageBits: 128, precisionBits: 53, exponentBits: 11);
    public static readonly TensorDtype Int8 = new("int8", TensorElementType.Int8, typeof(sbyte), width: Some(1), ortBytes: 1, quantized: true, modelBoundaryOnly: false, numeric: true, integral: true, signed: true, storageBits: 8, precisionBits: 7, exponentBits: 0);
    public static readonly TensorDtype Int16 = new("int16", TensorElementType.Int16, typeof(short), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: true, storageBits: 16, precisionBits: 15, exponentBits: 0);
    public static readonly TensorDtype Int32 = new("int32", TensorElementType.Int32, typeof(int), width: Some(4), ortBytes: 4, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: true, storageBits: 32, precisionBits: 31, exponentBits: 0);
    public static readonly TensorDtype Int64 = new("int64", TensorElementType.Int64, typeof(long), width: Some(8), ortBytes: 8, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: true, storageBits: 64, precisionBits: 63, exponentBits: 0);
    public static readonly TensorDtype UInt8 = new("uint8", TensorElementType.UInt8, typeof(byte), width: Some(1), ortBytes: 1, quantized: true, modelBoundaryOnly: false, numeric: true, integral: true, signed: false, storageBits: 8, precisionBits: 8, exponentBits: 0);
    public static readonly TensorDtype UInt16 = new("uint16", TensorElementType.UInt16, typeof(ushort), width: Some(2), ortBytes: 2, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: false, storageBits: 16, precisionBits: 16, exponentBits: 0);
    public static readonly TensorDtype UInt32 = new("uint32", TensorElementType.UInt32, typeof(uint), width: Some(4), ortBytes: 4, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: false, storageBits: 32, precisionBits: 32, exponentBits: 0);
    public static readonly TensorDtype UInt64 = new("uint64", TensorElementType.UInt64, typeof(ulong), width: Some(8), ortBytes: 8, quantized: false, modelBoundaryOnly: false, numeric: true, integral: true, signed: false, storageBits: 64, precisionBits: 64, exponentBits: 0);
    public static readonly TensorDtype Bool = new("bool", TensorElementType.Bool, typeof(bool), width: Some(1), ortBytes: 1, quantized: false, modelBoundaryOnly: false, numeric: false, integral: false, signed: false, storageBits: 1, precisionBits: 1, exponentBits: 0);
    public static readonly TensorDtype Utf8Text = new("string", TensorElementType.String, typeof(string), width: None, ortBytes: 0, quantized: false, modelBoundaryOnly: true, numeric: false, integral: false, signed: false, storageBits: 0, precisionBits: 0, exponentBits: 0);

    public TensorElementType Element { get; }
    public Type Clr { get; }
    public Option<int> Width { get; }
    public int OrtElementBytes { get; }
    public bool Quantized { get; }
    public bool ModelBoundaryOnly { get; }
    public bool Numeric { get; }
    public bool Integral { get; }
    public bool Signed { get; }
    public int StorageBits { get; }
    public int PrecisionBits { get; }
    public int ExponentBits { get; }

    // Zero-point range shares the quantized row's integer domain.
    public Option<(long Min, long Max)> ZeroPointDomain =>
        !Quantized ? None
        : Element == TensorElementType.Int8 ? Some(((long)sbyte.MinValue, (long)sbyte.MaxValue))
        : Some(((long)byte.MinValue, (long)byte.MaxValue));

    public Fin<int> ElementCount(long sizeInBytes) =>
        OrtElementBytes <= 0 ? TensorFault.Fail<int>("no-byte-stride", Key)
        : sizeInBytes < 0 ? TensorFault.Fail<int>("negative-byte-span", Key, sizeInBytes.ToString(CultureInfo.InvariantCulture))
        : sizeInBytes % OrtElementBytes != 0 ? TensorFault.Fail<int>("misaligned-byte-span", Key, $"{sizeInBytes}%{OrtElementBytes}")
        : sizeInBytes / OrtElementBytes > int.MaxValue ? TensorFault.Fail<int>("element-count-overflow", Key, sizeInBytes.ToString(CultureInfo.InvariantCulture))
        : Fin.Succ(checked((int)(sizeInBytes / OrtElementBytes)));
}

// --- [MODELS] ------------------------------------------------------------------------------
// ONNX affine quantization owns scalar, per-axis, and blocked granularities; dequant(x) = (x - zeroPoint) * scale.
[Union]
public abstract partial record QuantizationPolicy {
    private QuantizationPolicy() { }
    public sealed record PerTensor(double Scale, int ZeroPoint) : QuantizationPolicy;
    public sealed record PerAxis(int Axis, ImmutableArray<double> Scales, ImmutableArray<int> ZeroPoints) : QuantizationPolicy;
    public sealed record Blocked(int Axis, int BlockSize, ImmutableArray<double> Scales, ImmutableArray<int> ZeroPoints) : QuantizationPolicy;

    public Fin<QuantizationPolicy> Admit(TensorDtype row, ReadOnlyMemory<long> shape) =>
        row.ZeroPointDomain.Match(
            None: () => TensorFault.Fail<QuantizationPolicy>("quantization-on-unquantized-row", row.Key),
            Some: domain => this.Switch<Fin<QuantizationPolicy>>(
                perTensor: p => (ScaleGate(row, p.Scale), ZeroGate(row, domain, p.ZeroPoint))
                    .Apply((_, _) => (QuantizationPolicy)p).As().ToFin(),
                perAxis: a => (
                    AxisGate(row, a.Axis, shape),
                    ShapeVectorGate(row, a.Axis, null, a.Scales.Length, shape),
                    NonEmptyGate(row, a.Scales, a.ZeroPoints),
                    PairGate(row, a.Scales, a.ZeroPoints),
                    ScaleVectorGate(row, a.Scales),
                    ZeroVectorGate(row, domain, a.ZeroPoints))
                    .Apply((_, _, _, _, _, _) => (QuantizationPolicy)a).As().ToFin(),
                blocked: b => (
                    AxisGate(row, b.Axis, shape),
                    BlockGate(row, b.BlockSize),
                    ShapeVectorGate(row, b.Axis, b.BlockSize, b.Scales.Length, shape),
                    NonEmptyGate(row, b.Scales, b.ZeroPoints),
                    PairGate(row, b.Scales, b.ZeroPoints),
                    ScaleVectorGate(row, b.Scales),
                    ZeroVectorGate(row, domain, b.ZeroPoints))
                    .Apply((_, _, _, _, _, _, _) => (QuantizationPolicy)b).As().ToFin()));

    private static Validation<Error, Unit> ScaleGate(TensorDtype row, double scale) =>
        double.IsFinite(scale) && scale > 0.0 ? unit : TensorFault.Symbol("quant-scale", row.Key, scale.ToString(CultureInfo.InvariantCulture));

    private static Validation<Error, Unit> ZeroGate(TensorDtype row, (long Min, long Max) domain, int zeroPoint) =>
        zeroPoint >= domain.Min && zeroPoint <= domain.Max ? unit : TensorFault.Symbol("quant-zero-point", row.Key, $"{zeroPoint} outside [{domain.Min},{domain.Max}]");

    private static Validation<Error, Unit> AxisGate(TensorDtype row, int axis, ReadOnlyMemory<long> shape) =>
        axis >= 0 && axis < shape.Length ? unit : TensorFault.Symbol("quant-axis", row.Key, $"{axis}/{shape.Length}");

    private static Validation<Error, Unit> BlockGate(TensorDtype row, int blockSize) =>
        blockSize > 0 ? unit : TensorFault.Symbol("quant-block", row.Key, blockSize.ToString(CultureInfo.InvariantCulture));

    private static Validation<Error, Unit> ShapeVectorGate(
        TensorDtype row,
        int axis,
        int? blockSize,
        int vectorLength,
        ReadOnlyMemory<long> shape) {
        if (axis < 0 || axis >= shape.Length || blockSize is <= 0) { return unit; }
        long extent = shape.Span[axis];
        if (extent <= 0) { return TensorFault.Symbol("quant-shape", row.Key, $"axis={axis}:extent={extent}"); }
        if (blockSize is null && vectorLength != extent) { return TensorFault.Symbol("quant-axis-cardinality", row.Key, $"{vectorLength}!={extent}"); }
        if (blockSize is int block && vectorLength != 1 + (extent - 1) / block) {
            return TensorFault.Symbol("quant-block-cardinality", row.Key, $"{vectorLength}!={1 + (extent - 1) / block}");
        }
        return unit;
    }

    private static Validation<Error, Unit> NonEmptyGate(TensorDtype row, ImmutableArray<double> scales, ImmutableArray<int> zeroPoints) =>
        scales.IsDefaultOrEmpty || zeroPoints.IsDefaultOrEmpty ? TensorFault.Symbol("quant-empty", row.Key) : unit;

    private static Validation<Error, Unit> PairGate(TensorDtype row, ImmutableArray<double> scales, ImmutableArray<int> zeroPoints) =>
        scales.Length != zeroPoints.Length ? TensorFault.Symbol("quant-cardinality", row.Key, $"{scales.Length}!={zeroPoints.Length}") : unit;

    private static Validation<Error, Unit> ScaleVectorGate(TensorDtype row, ImmutableArray<double> scales) =>
        scales.Any(static scale => !double.IsFinite(scale) || scale <= 0.0) ? TensorFault.Symbol("quant-scale", row.Key) : unit;

    private static Validation<Error, Unit> ZeroVectorGate(TensorDtype row, (long Min, long Max) domain, ImmutableArray<int> zeroPoints) =>
        zeroPoints.Any(zero => zero < domain.Min || zero > domain.Max) ? TensorFault.Symbol("quant-zero-point", row.Key) : unit;
}

// --- [ERRORS] ------------------------------------------------------------------------------
public static class TensorFault {
    public static Error Symbol(string symbol, string key) => ComputeFault.Create($"<{symbol}:{key}>");
    public static Error Symbol(string symbol, string key, string detail) => ComputeFault.Create($"<{symbol}:{key}:{detail}>");
    public static Fin<A> Fail<A>(string symbol, string key) => Fin.Fail<A>(Symbol(symbol, key));
    public static Fin<A> Fail<A>(string symbol, string key, string detail) => Fin.Fail<A>(Symbol(symbol, key, detail));
}

// --- [SERVICES] ----------------------------------------------------------------------------

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorVocabulary {
    private static readonly FrozenDictionary<TensorElementType, TensorDtype> ByElement =
        TensorDtype.Items.ToFrozenDictionary(static row => row.Element, static row => row);

    public static Fin<TensorDtype> Admit(TensorElementType element) =>
        ByElement.TryGetValue(element, out TensorDtype? row) ? Fin.Succ(row!) : TensorFault.Fail<TensorDtype>("unmapped-element", element.ToString());

    public static Fin<TensorDtype> Promote(TensorDtype left, TensorDtype right) {
        if (left == right) { return Fin.Succ(left); }
        if (!left.Numeric || !right.Numeric) { return TensorFault.Fail<TensorDtype>("non-numeric-promotion", $"{left.Key}+{right.Key}"); }
        if (left == TensorDtype.Complex128 || right == TensorDtype.Complex128) { return Fin.Succ(TensorDtype.Complex128); }
        if (!left.Integral || !right.Integral) {
            // Range dominates beside precision: bfloat16 + float16 promotes to float32 because neither operand's
            // exponent domain covers the other, never to the precision-minimal float16 whose range truncates.
            int requiredPrecision = Math.Max(left.PrecisionBits, right.PrecisionBits);
            int requiredExponent = Math.Max(left.ExponentBits, right.ExponentBits);
            TensorDtype promoted = TensorDtype.Items
                .Where(static row => row.Numeric && !row.Integral && row != TensorDtype.Complex128 && !row.ModelBoundaryOnly)
                .OrderBy(static row => row.PrecisionBits)
                .FirstOrDefault(row => row.PrecisionBits >= requiredPrecision && row.ExponentBits >= requiredExponent) ?? TensorDtype.Float64;
            return Fin.Succ(promoted);
        }
        bool signed = left.Signed || right.Signed;
        int requiredBits = Math.Max(left.StorageBits, right.StorageBits) + (left.Signed == right.Signed ? 0 : 1);
        TensorDtype? integer = TensorDtype.Items
            .Where(row => row.Integral && row.Signed == signed && row.StorageBits >= requiredBits)
            .OrderBy(static row => row.StorageBits)
            .FirstOrDefault();
        return integer is not null ? Fin.Succ(integer) : Fin.Succ(TensorDtype.Float64);
    }

    public static Fin<TensorDtype> Admit(TensorElementType element, Option<QuantizationPolicy> quantization, ReadOnlyMemory<long> shape) =>
        Admit(element).Bind(row => quantization.Match(
            Some: policy => policy.Admit(row, shape).Map(_ => row),
            None: () => Fin.Succ(row)));

    public static Fin<int> OrtByteSpan(TensorDtype row, long sizeInBytes) => row.ElementCount(sizeInBytes);
}
```

## [03]-[OPERATION_TABLE]

- Owner: `TensorOpFamily`
- Cases: thirteen `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural, geometry — each carrying its `ToleranceClass` envelope column; the transcendental family spans the full `TensorPrimitives` surface — the forward and inverse trig (`Sin`/`Cos`/`Tan`/`Asin`/`Acos`/`Atan`/`Atan2`) and their `Pi` companions (`SinPi`/`CosPi`/`TanPi`/`AsinPi`/`AcosPi`/`AtanPi`/`Atan2Pi`/`SinCos`/`SinCosPi`), the hyperbolic and inverse-hyperbolic pairs (`Sinh`/`Cosh`/`Tanh`/`Asinh`/`Acosh`/`Atanh`), the base-2 and base-10 and `M1`/`P1` precision variants (`Exp2`/`Exp10`/`ExpM1`/`Exp2M1`/`Exp10M1`/`Log2`/`Log10`/`LogP1`/`Log2P1`/`Log10P1`), and the `Pow`/`Sqrt`/`Cbrt`/`RootN`/`ScaleB`/`Hypot` powers beside the exact-scale `DegreesToRadians`/`RadiansToDegrees`; the activation family — `Sigmoid`/`Tanh`/`SoftMax` bind direct `TensorPrimitives` members while `ReLU` (a `Max` clamp at zero), `SiLU` (`x · Sigmoid(x)`), `Gelu` (the tanh-approximation form), and `LogSoftMax` (`SoftMax` then `Log`) carry NO direct `TensorPrimitives` member and lower as the composed forms `Tensor/dispatch#KERNEL_DISPATCH` binds; the four pooling rows (`MaxPool`/`AvgPool`/`GlobalMaxPool`/`GlobalAvgPool`); the index-driven structural pair (`Gather`/`Scatter`) reading or writing elements by index value at span rate — take-along-axis composes `Gather` over layout-derived flat indices and a colliding scatter resolves last-write-wins in index order; the magnitude-extremum reduction quartet (`Max`/`Min` crossed with value/magnitude and NaN-propagating/NaN-missing: `MaxMagnitude`/`MinMagnitude`/`MaxMagnitudeNumber`/`MinMagnitudeNumber`); the five segmented-reduction rows (`SegmentSum`/`SegmentMean`/`SegmentMax`/`SegmentMin`/`SegmentCount`) folding per-group aggregates over a segment-id lane in one pass — per-zone, per-member, and per-cell rollups ride the `Tensor/dispatch#KERNEL_DISPATCH` `Segment` arity at span rate instead of a caller loop, and the rows carry no `TensorPrimitives` member; the estimate rows (`ReciprocalEstimate`/`ReciprocalSqrtEstimate`/`MultiplyAddEstimate`); the bit-adjacency and exponent-extraction elementwise rows (`BitIncrement`/`BitDecrement`/`ILogB`/`Remainder`); the widening conversion rows (`ConvertToSingle`/`ConvertToInteger`/`ConvertToIntegerNative`); the complete IEEE-754 predicate-classification family — `IsNaN`/`IsFinite`/`IsInfinity`/`IsPositiveInfinity`/`IsNegativeInfinity`/`IsInteger`/`IsEvenInteger`/`IsOddInteger`/`IsNegative`/`IsPositive`/`IsZero`/`IsNormal`/`IsSubnormal`/`IsPow2`/`IsCanonical`/`IsComplexNumber`/`IsImaginaryNumber`/`IsRealNumber`, each carrying its per-element mask plus its fused `All`/`Any` boolean-aggregate rows; the element-domain rows (`ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` over `System.Numerics.Complex`, `QuaternionMultiply`/`QuaternionConjugate`/`QuaternionNormalize` over `System.Numerics.Quaternion`); and the six DDG geometry-operator rows (`Gradient`/`Divergence`/`Curl`/`CotangentLaplacian`/`HeatFlow`/`Spectral`) carrying the linear DEC operators whose reverse-mode adjoint is the operator transpose over the `Tensor/dispatch#EQUIVALENCE_INTEROP` `Sensitivity.Operator` reverse-mode apply (routing the `Tensor/dispatch#EQUIVALENCE_INTEROP` `OperatorRow.Adjoint` over the kernel `Rasm.Numerics` `DiscreteCalculus`) — all ride the existing kind axis as rows, never sibling op types
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind and tolerance columns; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row; zero new surface.
- Boundary: `ToleranceClass.Bound(length, mass)` owns absolute equivalence envelopes, and `Vacuous` rejects cancellation-dominated evidence. `TensorVocabulary.Promote(left, right)` generates result dtype from numeric kind, signedness, storage width, precision width, and exponent range rather than maintaining an ordered-pair roster. Quantized admission composes independent scale, zero-point, axis, block, vector, and shape gates through accumulated `Validation`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToleranceClass {
    // Equivalence bound is the error ENVELOPE Bound(N, mass) over the reduction length N and the operand
    // mass Σ|xᵢ|, never a flat relative scalar: a vectorized reduction reassociates so its bound scales N·ε·Σ|xᵢ|;
    // a same-route fused/iterative op bands by a few ε·Σ|xᵢ| (ULP); a C-runtime transcendental is golden-vector
    // banded and never bit-exact across platforms; an estimate row has no cross-machine bound. ε = ScaleB(1, -52)
    // is the double machine epsilon. The envelope IS the single source of truth — a stored relative scalar beside
    // it is the de-sync defect this owner forecloses.
    public static readonly ToleranceClass Exact = new("exact", static (_, _) => 0.0);
    public static readonly ToleranceClass UlpBanded = new("ulp-banded", static (_, mass) => Math.ScaleB(4.0, -52) * mass);
    public static readonly ToleranceClass AccumulationScaled = new("accumulation-scaled", static (length, mass) => length * Math.ScaleB(1.0, -52) * mass);
    public static readonly ToleranceClass CrossPlatformVariant = new("cross-platform-variant", static (_, mass) => Math.ScaleB(16.0, -52) * mass);
    public static readonly ToleranceClass PlatformVariant = new("platform-variant", static (_, _) => double.PositiveInfinity);

    [UseDelegateFromConstructor]
    public partial double Bound(int length, double mass);

    // Cancellation ratio |Σxᵢ|/Σ|xᵢ| decides when even the scaled envelope is vacuous: a reduction whose
    // catastrophic cancellation drives the ratio below the floor cannot certify equivalence, so the proof rail
    // records the ratio class beside the bound rather than passing a meaningless tight bound — Exact alone is
    // ratio-invariant because bit-equality holds regardless of cancellation.
    public const double CancellationFloor = 1e-8;

    public bool Vacuous(double cancellationRatio) => this != Exact && cancellationRatio < CancellationFloor;

    public bool Holds(double deviation, int length, double mass, double cancellationRatio) =>
        !Vacuous(cancellationRatio) && deviation <= Bound(length, mass);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorOpFamily {
    public static readonly TensorOpFamily Add = new("add", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Subtract = new("subtract", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Multiply = new("multiply", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Divide = new("divide", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Negate = new("negate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Abs = new("abs", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Clamp = new("clamp", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily CopySign = new("copy-sign", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily MultiplyAdd = new("multiply-add", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily FusedMultiplyAdd = new("fused-multiply-add", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily MultiplyAddEstimate = new("multiply-add-estimate", TensorOpKind.Elementwise, ToleranceClass.PlatformVariant);
    public static readonly TensorOpFamily AddMultiply = new("add-multiply", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily Lerp = new("lerp", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily Hypot = new("hypot", TensorOpKind.Elementwise, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Reciprocal = new("reciprocal", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily ReciprocalSqrt = new("reciprocal-sqrt", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily ReciprocalEstimate = new("reciprocal-estimate", TensorOpKind.Elementwise, ToleranceClass.PlatformVariant);
    public static readonly TensorOpFamily ReciprocalSqrtEstimate = new("reciprocal-sqrt-estimate", TensorOpKind.Elementwise, ToleranceClass.PlatformVariant);
    public static readonly TensorOpFamily Ieee754Remainder = new("ieee-754-remainder", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Remainder = new("remainder", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily ILogB = new("ilogb", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily BitIncrement = new("bit-increment", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily BitDecrement = new("bit-decrement", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily Round = new("round", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Floor = new("floor", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Ceiling = new("ceiling", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Truncate = new("truncate", TensorOpKind.Rounding, ToleranceClass.Exact);
    public static readonly TensorOpFamily Exp = new("exp", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Exp2 = new("exp2", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Exp10 = new("exp10", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily ExpM1 = new("expm1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Exp2M1 = new("exp2m1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Exp10M1 = new("exp10m1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Log = new("log", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Log2 = new("log2", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Log10 = new("log10", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily LogP1 = new("logp1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Log2P1 = new("log2p1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Log10P1 = new("log10p1", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Sin = new("sin", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Cos = new("cos", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Tan = new("tan", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily SinPi = new("sin-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily CosPi = new("cos-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily TanPi = new("tan-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily SinCos = new("sin-cos", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily SinCosPi = new("sin-cos-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Asin = new("asin", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Acos = new("acos", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Atan = new("atan", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Atan2 = new("atan2", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily AsinPi = new("asin-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily AcosPi = new("acos-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily AtanPi = new("atan-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Atan2Pi = new("atan2-pi", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Sinh = new("sinh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Cosh = new("cosh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Tanh = new("tanh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Asinh = new("asinh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Acosh = new("acosh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Atanh = new("atanh", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Sigmoid = new("sigmoid", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily SoftMax = new("softmax", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily LogSoftMax = new("log-softmax", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily ReLU = new("relu", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily Gelu = new("gelu", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily SiLU = new("silu", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Pow = new("pow", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Sqrt = new("sqrt", TensorOpKind.Transcendental, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily Cbrt = new("cbrt", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily RootN = new("root-n", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily ScaleB = new("scale-b", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily DegreesToRadians = new("degrees-to-radians", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily RadiansToDegrees = new("radians-to-degrees", TensorOpKind.Transcendental, ToleranceClass.Exact);
    public static readonly TensorOpFamily Sum = new("sum", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Product = new("product", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Dot = new("dot", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Norm = new("norm", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Min = new("min", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily Max = new("max", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MinNumber = new("min-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MaxNumber = new("max-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MinMagnitude = new("min-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MaxMagnitude = new("max-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MinMagnitudeNumber = new("min-magnitude-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily MaxMagnitudeNumber = new("max-magnitude-number", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily SumOfSquares = new("sum-of-squares", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily SumOfMagnitudes = new("sum-of-magnitudes", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily ProductOfSums = new("product-of-sums", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily ProductOfDifferences = new("product-of-differences", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily IndexOfMax = new("index-of-max", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMin = new("index-of-min", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMaxMagnitude = new("index-of-max-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily IndexOfMinMagnitude = new("index-of-min-magnitude", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily SegmentSum = new("segment-sum", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily SegmentMean = new("segment-mean", TensorOpKind.Reduction, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily SegmentMax = new("segment-max", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily SegmentMin = new("segment-min", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily SegmentCount = new("segment-count", TensorOpKind.Reduction, ToleranceClass.Exact);
    public static readonly TensorOpFamily Average = new("average", TensorOpKind.Statistics, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily StdDev = new("std-dev", TensorOpKind.Statistics, ToleranceClass.AccumulationScaled);
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
    public static readonly TensorOpFamily CosineSimilarity = new("cosine-similarity", TensorOpKind.Similarity, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Distance = new("distance", TensorOpKind.Similarity, ToleranceClass.AccumulationScaled);
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
    public static readonly TensorOpFamily IsNaNAll = new("is-nan-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNaNAny = new("is-nan-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFinite = new("is-finite", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFiniteAll = new("is-finite-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsFiniteAny = new("is-finite-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsInfinity = new("is-infinity", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsInfinityAll = new("is-infinity-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsInfinityAny = new("is-infinity-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositiveInfinity = new("is-positive-infinity", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositiveInfinityAll = new("is-positive-infinity-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositiveInfinityAny = new("is-positive-infinity-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegativeInfinity = new("is-negative-infinity", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegativeInfinityAll = new("is-negative-infinity-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegativeInfinityAny = new("is-negative-infinity-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsInteger = new("is-integer", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsIntegerAll = new("is-integer-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsIntegerAny = new("is-integer-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsEvenInteger = new("is-even-integer", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsEvenIntegerAll = new("is-even-integer-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsEvenIntegerAny = new("is-even-integer-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsOddInteger = new("is-odd-integer", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsOddIntegerAll = new("is-odd-integer-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsOddIntegerAny = new("is-odd-integer-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegative = new("is-negative", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegativeAll = new("is-negative-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNegativeAny = new("is-negative-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositive = new("is-positive", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositiveAll = new("is-positive-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPositiveAny = new("is-positive-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsZero = new("is-zero", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsZeroAll = new("is-zero-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsZeroAny = new("is-zero-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNormal = new("is-normal", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNormalAll = new("is-normal-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsNormalAny = new("is-normal-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsSubnormal = new("is-subnormal", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsSubnormalAll = new("is-subnormal-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsSubnormalAny = new("is-subnormal-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPow2 = new("is-pow2", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPow2All = new("is-pow2-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsPow2Any = new("is-pow2-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsCanonical = new("is-canonical", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsCanonicalAll = new("is-canonical-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsCanonicalAny = new("is-canonical-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsComplexNumber = new("is-complex-number", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsComplexNumberAll = new("is-complex-number-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsComplexNumberAny = new("is-complex-number-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsImaginaryNumber = new("is-imaginary-number", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsImaginaryNumberAll = new("is-imaginary-number-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsImaginaryNumberAny = new("is-imaginary-number-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsRealNumber = new("is-real-number", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsRealNumberAll = new("is-real-number-all", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily IsRealNumberAny = new("is-real-number-any", TensorOpKind.Predicate, ToleranceClass.Exact);
    public static readonly TensorOpFamily MatMul = new("matmul", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Conv1D = new("conv-1d", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Conv2D = new("conv-2d", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Conv3D = new("conv-3d", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily MaxPool = new("max-pool", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily AvgPool = new("avg-pool", TensorOpKind.Structural, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily GlobalMaxPool = new("global-max-pool", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily GlobalAvgPool = new("global-avg-pool", TensorOpKind.Structural, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily MaskedWrite = new("masked-write", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily Gather = new("gather", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily Scatter = new("scatter", TensorOpKind.Structural, ToleranceClass.Exact);
    public static readonly TensorOpFamily ComplexAbs = new("complex-abs", TensorOpKind.Elementwise, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily ComplexExp = new("complex-exp", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily ComplexLog = new("complex-log", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant);
    public static readonly TensorOpFamily Conjugate = new("conjugate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily QuaternionMultiply = new("quaternion-multiply", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily QuaternionConjugate = new("quaternion-conjugate", TensorOpKind.Elementwise, ToleranceClass.Exact);
    public static readonly TensorOpFamily QuaternionNormalize = new("quaternion-normalize", TensorOpKind.Elementwise, ToleranceClass.UlpBanded);
    public static readonly TensorOpFamily Gradient = new("gradient", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Divergence = new("divergence", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Curl = new("curl", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily CotangentLaplacian = new("cotangent-laplacian", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily HeatFlow = new("heat-flow", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled);
    public static readonly TensorOpFamily Spectral = new("spectral", TensorOpKind.Geometry, ToleranceClass.CrossPlatformVariant);

    public TensorOpKind Kind { get; }
    public ToleranceClass Tolerance { get; }
}
```
