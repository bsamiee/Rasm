# [COMPUTE_VOCABULARY]

Cpu-tensor vocabulary uses `Tensor<T>` as its only tensor owner, `TensorDtype` as the `TensorElementType`↔CLR/ONNX map, and `TensorOpFamily` as the equivalence-keyed operation table. `TensorDtype`, `QuantizationPolicy`, `TensorReason`, `TensorVocabulary`, `TensorOpKind`, `TensorOpFamily`, `ToleranceClass`, and `ProofVerdict` live here; matrix lowering, kernel dispatch, residency, and layout consume those settled shapes.

## [01]-[INDEX]

- [02]-[TENSOR_VOCABULARY]: tensor shapes, factories, dtype map, ONNX byte-width, quantization policy.
- [03]-[OPERATION_TABLE]: `TensorOpKind`/`TensorOpFamily`/`ToleranceClass`/`ProofVerdict` vocabulary rows.

## [02]-[TENSOR_VOCABULARY]

- Owner: `TensorDtype`; `TensorReason` the closed refusal vocabulary and direct fault mint the `ComputeFault.TensorRejected` 2229 arm carries.
- Cases: float32, float64, float16, bfloat16, complex128, quaternion, int8, int16, int32, int64, uint8, uint16, uint32, uint64, bool, string — the quaternion row alone carries no `TensorElementType` and no ONNX stride, so the span lane admits it and the model boundary cannot.
- Entry: `Admit(TensorElementType)` aborts on an unmapped element; `Admit(IH5DataType)` is the archive admission arm — `Runtime/archive#HDF_ARCHIVE` reads bind a dtype row from `H5DataTypeClass` and byte size before any buffer sizes, `FloatingPoint`×`Size` and `FixedPoint`×`IsSigned`×`Size` projecting onto the landed rows, `String` binding the model-boundary text row, every other class refusing typed — the interface face carries no byte-order member, so endian divergence refuses at the archive read as `<hdf5-byte-order:…>`, never here; `Promote(TensorDtype, TensorDtype)` derives mixed arithmetic from each row's `NumericDomain` precision-exponent pair, integrality, signedness, and derived storage width, including signed/unsigned widening, float/complex escalation, and the range gate that promotes a bfloat16-float16 pair to float32 rather than truncating exponent range, without a named pair roster. `Promote` widens a mixed-sign integral pair demanding one bit past the widest integral row to float64 — the deliberate lossy widening numpy semantics fix, carrying both magnitudes at 53-bit precision rather than refusing a promotion every caller then works around. `AdmitSpan(TensorElementType)` is the span-lane arm refusing a boundary-only carrier by its `Reach` row. Quantization admission proves the axis extent first, then accumulates cardinality and zero-point invariants against the tensor shape. `TensorDtype.ElementCount` converts native bytes without negative, alignment, or width truncation, read directly by every consumer — the one-hop forwarder over it is deleted.
- Packages: System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, PureHDF (`IH5DataType.Class`/`Size`/`FixedPoint.IsSigned`, `H5DataTypeClass`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element mapping is one `TensorDtype` row carrying its optional ONNX element, byte width, optional ORT stride, quantization flag, reach, integrality, signedness, and optional numeric domain — storage width and numeracy DERIVE from those; element admission, archive admission, and mixed promotion all fold `Items`, so no pair table and no arm ladder grows.
- Boundary: `Tensor<T>`, `TensorSpan<T>`, `ReadOnlyTensorSpan<T>`, `TensorShape`, and `TensorDimensionSpan<T>` are the only tensor shapes — package-local tensor wrappers and a TensorService are the deleted forms; `Tensor.CreateFromArray`, `CreateFromMemory`, `CreateFromSequence`, and `CreateFromDiagonal` are phantom spellings — `Tensor.Create`, `CreateFromShape`, and `CreateFromShapeUninitialized` are the factory surface, and zero-copy admission rides `TensorSpan<T>` constructors over spans and `Tensor.Create` over rented `MemoryOwner<T>` arrays through `DangerousGetArray`; `TensorMarshal.CreateTensorSpan` is the write-polarity native bridge over ref-rooted foreign memory and `TensorMarshal.CreateReadOnlyTensorSpan` the read-polarity bridge admitting pooled-plane and model-output buffers whose lifetime the caller owns, with `TensorMarshal.GetReference` and `Tensor<T>.GetPinnableReference` as ref roots; one generic kernel serves each operation family. `Width` carries CLR byte width and `OrtElementBytes` the ONNX C-data stride, so `GetTensorSizeInBytes` converts through the dtype row, never `sizeof(T)`; `ElementCount` rejects negative, non-integral, and `int`-overflowing element counts before any destination slice, and answers `no-byte-stride` for the rows whose ORT stride is absent rather than zero. `Complex128` carries `System.Numerics.Complex`, while `complex64` has no BCL carrier and never admits to a span; native FP8, `Int4`/`UInt4`, and `Float4E2M1` types do not exist in managed `TensorElementType` and remain inadmissible. Quantized rows compose subtract-zero-point then multiply-scale dequantization and inverse round-add-`ConvertSaturating` quantization, broadcasting by per-tensor, per-axis, or blocked granularity. `QuantizationPolicy.Admit` receives the tensor shape, admits the axis extent it depends on, then accumulates the four independent structural gates through tuple `Apply` and exits once to `Fin`; a `Granularity` value carries whether a scale covers one axis element or a block of them, so one axial body serves both vector granularities; no kernel revalidates metadata. Chunked contiguous frames stage through `StreamGrant.ContiguousFrame`; the string row admits only at the model boundary through `OrtValue.CreateTensorWithEmptyStrings` then `CreateFromStringTensor`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Reach {
    public static readonly Reach Span = new("span");
    public static readonly Reach ModelBoundary = new("model-boundary");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype Float32 = new("float32", element: Some(TensorElementType.Float), typeof(float), width: Some(4), ortBytes: Some(4), quantized: false, reach: Reach.Span, integral: false, signed: true, numericDomain: Some((24, 8)));
    public static readonly TensorDtype Float64 = new("float64", element: Some(TensorElementType.Double), typeof(double), width: Some(8), ortBytes: Some(8), quantized: false, reach: Reach.Span, integral: false, signed: true, numericDomain: Some((53, 11)));
    public static readonly TensorDtype Float16 = new("float16", element: Some(TensorElementType.Float16), typeof(Half), width: Some(2), ortBytes: Some(2), quantized: false, reach: Reach.Span, integral: false, signed: true, numericDomain: Some((11, 5)));
    public static readonly TensorDtype BFloat16 = new("bfloat16", element: Some(TensorElementType.BFloat16), typeof(Microsoft.ML.OnnxRuntime.BFloat16), width: Some(2), ortBytes: Some(2), quantized: false, reach: Reach.ModelBoundary, integral: false, signed: true, numericDomain: Some((8, 8)));
    public static readonly TensorDtype Complex128 = new("complex128", element: Some(TensorElementType.Complex128), typeof(System.Numerics.Complex), width: Some(16), ortBytes: Some(16), quantized: false, reach: Reach.Span, integral: false, signed: true, numericDomain: Some((53, 11)));
    public static readonly TensorDtype Quaternion = new("quaternion", element: None, typeof(System.Numerics.Quaternion), width: Some(16), ortBytes: None, quantized: false, reach: Reach.Span, integral: false, signed: true, numericDomain: None);
    public static readonly TensorDtype Int8 = new("int8", element: Some(TensorElementType.Int8), typeof(sbyte), width: Some(1), ortBytes: Some(1), quantized: true, reach: Reach.Span, integral: true, signed: true, numericDomain: Some((7, 0)));
    public static readonly TensorDtype Int16 = new("int16", element: Some(TensorElementType.Int16), typeof(short), width: Some(2), ortBytes: Some(2), quantized: false, reach: Reach.Span, integral: true, signed: true, numericDomain: Some((15, 0)));
    public static readonly TensorDtype Int32 = new("int32", element: Some(TensorElementType.Int32), typeof(int), width: Some(4), ortBytes: Some(4), quantized: false, reach: Reach.Span, integral: true, signed: true, numericDomain: Some((31, 0)));
    public static readonly TensorDtype Int64 = new("int64", element: Some(TensorElementType.Int64), typeof(long), width: Some(8), ortBytes: Some(8), quantized: false, reach: Reach.Span, integral: true, signed: true, numericDomain: Some((63, 0)));
    public static readonly TensorDtype UInt8 = new("uint8", element: Some(TensorElementType.UInt8), typeof(byte), width: Some(1), ortBytes: Some(1), quantized: true, reach: Reach.Span, integral: true, signed: false, numericDomain: Some((8, 0)));
    public static readonly TensorDtype UInt16 = new("uint16", element: Some(TensorElementType.UInt16), typeof(ushort), width: Some(2), ortBytes: Some(2), quantized: false, reach: Reach.Span, integral: true, signed: false, numericDomain: Some((16, 0)));
    public static readonly TensorDtype UInt32 = new("uint32", element: Some(TensorElementType.UInt32), typeof(uint), width: Some(4), ortBytes: Some(4), quantized: false, reach: Reach.Span, integral: true, signed: false, numericDomain: Some((32, 0)));
    public static readonly TensorDtype UInt64 = new("uint64", element: Some(TensorElementType.UInt64), typeof(ulong), width: Some(8), ortBytes: Some(8), quantized: false, reach: Reach.Span, integral: true, signed: false, numericDomain: Some((64, 0)));
    public static readonly TensorDtype Bool = new("bool", element: Some(TensorElementType.Bool), typeof(bool), width: Some(1), ortBytes: Some(1), quantized: false, reach: Reach.Span, integral: false, signed: false, numericDomain: None);
    public static readonly TensorDtype Utf8Text = new("string", element: Some(TensorElementType.String), typeof(string), width: None, ortBytes: None, quantized: false, reach: Reach.ModelBoundary, integral: false, signed: false, numericDomain: None);

    public Option<TensorElementType> Element { get; }
    public Type Clr { get; }
    public Option<int> Width { get; }
    public Option<int> OrtElementBytes { get; }
    public bool Quantized { get; }
    public Reach Reach { get; }
    public bool Integral { get; }
    public bool Signed { get; }
    public Option<(int Precision, int Exponent)> NumericDomain { get; }

    public bool Numeric => NumericDomain.IsSome;
    public int StorageBits => Width.Map(static bytes => bytes * 8).IfNone(0);

    public OperandDomain Domain =>
        this == Quaternion ? OperandDomain.Quaternion
        : this == Complex128 ? OperandDomain.Complex | OperandDomain.Numeric
        : Integral ? OperandDomain.Integer | OperandDomain.Numeric | OperandDomain.BinaryNumeric
        : Numeric ? OperandDomain.Real | OperandDomain.Numeric | OperandDomain.BinaryNumeric
        : OperandDomain.None;

    public Option<(long Min, long Max)> ZeroPointDomain =>
        !Quantized ? None
        : Signed ? Some((-(1L << (StorageBits - 1)), (1L << (StorageBits - 1)) - 1))
        : Some((0L, (1L << StorageBits) - 1));

    public Fin<int> ElementCount(long sizeInBytes) =>
        OrtElementBytes.Match(
            None: () => TensorReason.ByteStrideAbsent.Fail<int>("no-byte-stride", Key),
            Some: stride =>
                sizeInBytes < 0 ? TensorReason.ByteSpanMisaligned.Fail<int>("negative-byte-span", Key, sizeInBytes.ToString(CultureInfo.InvariantCulture))
                : sizeInBytes % stride != 0 ? TensorReason.ByteSpanMisaligned.Fail<int>("misaligned-byte-span", Key, $"{sizeInBytes}%{stride}")
                : sizeInBytes / stride > int.MaxValue ? TensorReason.ExtentOverflow.Fail<int>("element-count-overflow", Key, sizeInBytes.ToString(CultureInfo.InvariantCulture))
                : Fin.Succ(checked((int)(sizeInBytes / stride))));
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct PositiveScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) {
            validationError = new ValidationError($"<quant-scale:{value.ToString(CultureInfo.InvariantCulture)}>");
        }
    }
}

[Union]
public abstract partial record Granularity {
    private Granularity() { }
    public sealed record Whole : Granularity;
    public sealed record Blocks(int Size) : Granularity;
}

[Union]
public abstract partial record QuantizationPolicy {
    private QuantizationPolicy() { }
    public sealed record PerTensor(PositiveScale Scale, int ZeroPoint) : QuantizationPolicy;
    [Equatable]
    public sealed partial record PerAxis(int Axis, [property: OrderedEquality] ImmutableArray<PositiveScale> Scales, [property: OrderedEquality] ImmutableArray<int> ZeroPoints) : QuantizationPolicy;
    [Equatable]
    public sealed partial record Blocked(int Axis, int BlockSize, [property: OrderedEquality] ImmutableArray<PositiveScale> Scales, [property: OrderedEquality] ImmutableArray<int> ZeroPoints) : QuantizationPolicy;

    public Fin<QuantizationPolicy> Admit(TensorDtype row, ReadOnlyMemory<long> shape) =>
        row.ZeroPointDomain.Match(
            None: () => TensorReason.QuantizationInvalid.Fail<QuantizationPolicy>("quantization-on-unquantized-row", row.Key),
            Some: domain => this.Switch<Fin<QuantizationPolicy>>(
                perTensor: p => ZeroGate(row, domain, p.ZeroPoint).As().ToFin().Map(_ => (QuantizationPolicy)p),
                perAxis: a => Axial(row, domain, shape, a.Axis, new Granularity.Whole(), a.Scales, a.ZeroPoints).Map(_ => (QuantizationPolicy)a),
                blocked: b => Axial(row, domain, shape, b.Axis, new Granularity.Blocks(b.BlockSize), b.Scales, b.ZeroPoints).Map(_ => (QuantizationPolicy)b)));

    private static Fin<Unit> Axial(
        TensorDtype row,
        (long Min, long Max) domain,
        ReadOnlyMemory<long> shape,
        int axis,
        Granularity granularity,
        ImmutableArray<PositiveScale> scales,
        ImmutableArray<int> zeroPoints) =>
        AxisExtent(row, axis, shape).Bind(extent =>
            (CardinalityGate(row, granularity, extent, scales.Length),
             NonEmptyGate(row, scales, zeroPoints),
             PairGate(row, scales, zeroPoints),
             ZeroVectorGate(row, domain, zeroPoints))
                .Apply(static (_, _, _, _) => unit).As().ToFin());

    private static Fin<long> AxisExtent(TensorDtype row, int axis, ReadOnlyMemory<long> shape) =>
        axis < 0 || axis >= shape.Length ? TensorReason.AxisOutOfRange.Fail<long>("quant-axis", row.Key, $"{axis}/{shape.Length}")
        : shape.Span[axis] is var extent && extent <= 0 ? TensorReason.ShapeMismatch.Fail<long>("quant-shape", row.Key, $"axis={axis}:extent={extent}")
        : Fin.Succ(extent);

    private static Validation<Error, Unit> CardinalityGate(TensorDtype row, Granularity granularity, long extent, int vectorLength) =>
        granularity.Switch(
            whole: _ => vectorLength == extent ? unit : TensorReason.QuantizationInvalid.Fault("quant-axis-cardinality", row.Key, $"{vectorLength}!={extent}"),
            blocks: b => b.Size <= 0 ? TensorReason.QuantizationInvalid.Fault("quant-block", row.Key, b.Size.ToString(CultureInfo.InvariantCulture))
                : vectorLength == 1 + ((extent - 1) / b.Size) ? unit
                : TensorReason.QuantizationInvalid.Fault("quant-block-cardinality", row.Key, $"{vectorLength}!={1 + ((extent - 1) / b.Size)}"));

    private static Validation<Error, Unit> ZeroGate(TensorDtype row, (long Min, long Max) domain, int zeroPoint) =>
        zeroPoint >= domain.Min && zeroPoint <= domain.Max ? unit : TensorReason.QuantizationInvalid.Fault("quant-zero-point", row.Key, $"{zeroPoint} outside [{domain.Min},{domain.Max}]");

    private static Validation<Error, Unit> NonEmptyGate(TensorDtype row, ImmutableArray<PositiveScale> scales, ImmutableArray<int> zeroPoints) =>
        scales.IsDefaultOrEmpty || zeroPoints.IsDefaultOrEmpty ? TensorReason.EmptyOperand.Fault("quant-empty", row.Key) : unit;

    private static Validation<Error, Unit> PairGate(TensorDtype row, ImmutableArray<PositiveScale> scales, ImmutableArray<int> zeroPoints) =>
        scales.Length != zeroPoints.Length ? TensorReason.QuantizationInvalid.Fault("quant-cardinality", row.Key, $"{scales.Length}!={zeroPoints.Length}") : unit;

    private static Validation<Error, Unit> ZeroVectorGate(TensorDtype row, (long Min, long Max) domain, ImmutableArray<int> zeroPoints) =>
        zeroPoints.Any(zero => zero < domain.Min || zero > domain.Max) ? TensorReason.QuantizationInvalid.Fault("quant-zero-point", row.Key) : unit;
}

// --- [ERRORS] --------------------------------------------------------------------------
public abstract partial record ComputeFault {
    [FaultCase(29)] public sealed partial record TensorRejected(TensorReason Reason, string Witness) : ComputeFault($"{Reason.Key}:{Witness}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorReason {
    // --- [SHAPE]
    public static readonly TensorReason ShapeMismatch       = new("shape-mismatch");
    public static readonly TensorReason AxisOutOfRange      = new("axis-out-of-range");
    public static readonly TensorReason EmptyOperand        = new("empty-operand");
    public static readonly TensorReason ExtentOverflow      = new("extent-overflow");
    public static readonly TensorReason PermutationInvalid  = new("permutation-invalid");
    // --- [DTYPE]
    public static readonly TensorReason DtypeMismatch       = new("dtype-mismatch");
    public static readonly TensorReason ByteStrideAbsent    = new("byte-stride-absent");
    public static readonly TensorReason ByteSpanMisaligned  = new("byte-span-misaligned");
    public static readonly TensorReason QuantizationInvalid = new("quantization-invalid");
    // --- [ROSTER]
    public static readonly TensorReason RowMissing          = new("row-missing");
    public static readonly TensorReason OperandDomainMiss   = new("operand-domain-miss");
    public static readonly TensorReason AxisUnderivable     = new("axis-underivable");
    // --- [RESIDENCY]
    public static readonly TensorReason ResidencyMismatch   = new("residency-mismatch");
    public static readonly TensorReason NativeRejected      = new("native-rejected");
    // --- [STAGING]
    public static readonly TensorReason StagingOverBound    = new("staging-over-bound");
    // --- [NUMERIC]
    public static readonly TensorReason PolicyInvalid       = new("policy-invalid");
    public static readonly TensorReason NonFinite           = new("non-finite");
    public static readonly TensorReason WitnessFail         = new("witness-fail");
    public static readonly TensorReason BudgetExhausted     = new("budget-exhausted");
    public static readonly TensorReason StructuralRank      = new("structural-rank");

    public Error Fault(string site, params ReadOnlySpan<string> payload) =>
        new ComputeFault.TensorRejected(this,
            payload.IsEmpty ? $"<{site}>" : $"<{site}:{string.Join(':', payload)}>");

    public Fin<A> Fail<A>(string site, params ReadOnlySpan<string> payload) =>
        Fin.Fail<A>(Fault(site, payload));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TensorVocabulary {
    private static readonly FrozenDictionary<TensorElementType, TensorDtype> ByElement =
        toSeq(TensorDtype.Items)
            .Choose(static row => row.Element.Map(element => (Element: element, Row: row)))
            .ToFrozenDictionary(static pair => pair.Element, static pair => pair.Row);

    public static Fin<TensorDtype> Admit(TensorElementType element) =>
        ByElement.TryGetValue(element, out TensorDtype? row) ? Fin.Succ(row!) : TensorReason.DtypeMismatch.Fail<TensorDtype>("unmapped-element", element.ToString());

    public static Fin<TensorDtype> AdmitSpan(TensorElementType element) =>
        Admit(element).Bind(static row => row.Reach == Reach.Span
            ? Fin.Succ(row)
            : TensorReason.DtypeMismatch.Fail<TensorDtype>("boundary-only-element", row.Key));

    public static Fin<TensorDtype> Promote(TensorDtype left, TensorDtype right) =>
        left == right ? Fin.Succ(left)
        : left == TensorDtype.Complex128 || right == TensorDtype.Complex128
            ? left.Numeric && right.Numeric ? Fin.Succ(TensorDtype.Complex128) : NonNumeric(left, right)
        : (from a in left.NumericDomain from b in right.NumericDomain select (Left: a, Right: b)).Match(
            None: () => NonNumeric(left, right),
            Some: pair => left.Integral && right.Integral
                ? Integral(left, right)
                : Floating(left, right, Math.Max(pair.Left.Precision, pair.Right.Precision), Math.Max(pair.Left.Exponent, pair.Right.Exponent)));

    private static Fin<TensorDtype> Floating(TensorDtype left, TensorDtype right, int precision, int exponent) =>
        toSeq(TensorDtype.Items)
            .Filter(static row => row.Numeric && !row.Integral && row != TensorDtype.Complex128 && row.Reach == Reach.Span)
            .OrderBy(static row => row.StorageBits)
            .Find(row => row.NumericDomain.Map(domain => domain.Precision >= precision && domain.Exponent >= exponent).IfNone(false))
            .Match(Some: Fin.Succ, None: () => TensorReason.DtypeMismatch.Fail<TensorDtype>("promotion-exhausted", $"{left.Key}+{right.Key}", $"precision={precision}:exponent={exponent}"));

    private static readonly TensorDtype MixedSignFloor = TensorDtype.Float64;

    private static Fin<TensorDtype> Integral(TensorDtype left, TensorDtype right) {
        bool signed = left.Signed || right.Signed;
        int demanded = Math.Max(left.StorageBits, right.StorageBits) + (left.Signed == right.Signed ? 0 : 1);
        return toSeq(TensorDtype.Items)
            .Filter(row => row.Integral && row.Signed == signed && row.StorageBits >= demanded)
            .OrderBy(static row => row.StorageBits)
            .Head
            .Match(Some: Fin.Succ, None: () => Fin.Succ(MixedSignFloor));
    }

    private static Fin<TensorDtype> NonNumeric(TensorDtype left, TensorDtype right) =>
        TensorReason.DtypeMismatch.Fail<TensorDtype>("non-numeric-promotion", $"{left.Key}+{right.Key}");

    public static Fin<TensorDtype> Admit(IH5DataType type) => type.Class switch {
        H5DataTypeClass.FloatingPoint => Derived(
            row => row.Numeric && !row.Integral && row != TensorDtype.Complex128 && row.Reach == Reach.Span && row.Width == Some(type.Size),
            "hdf5-float-width", type.Size),
        H5DataTypeClass.FixedPoint => Derived(
            row => row.Integral && row.Signed == type.FixedPoint.IsSigned && row.Width == Some(type.Size),
            "hdf5-integer-width", type.Size),
        H5DataTypeClass.String => Fin.Succ(TensorDtype.Utf8Text),
        _ => TensorReason.DtypeMismatch.Fail<TensorDtype>("hdf5-dtype", type.Class.ToString()),
    };

    private static Fin<TensorDtype> Derived(Func<TensorDtype, bool> admits, string slug, int size) =>
        toSeq(TensorDtype.Items).Find(admits)
            .Match(Some: Fin.Succ, None: () => slug.Fail<TensorDtype>(size.ToString(CultureInfo.InvariantCulture)));

    public static Fin<TensorDtype> Admit(TensorElementType element, Option<QuantizationPolicy> quantization, ReadOnlyMemory<long> shape) =>
        Admit(element).Bind(row => quantization.Match(
            Some: policy => policy.Admit(row, shape).Map(_ => row),
            None: () => Fin.Succ(row)));
}
```

## [03]-[OPERATION_TABLE]

- Owner: `TensorOpFamily` (114 operation rows over five columns) + `TensorOpKind` (proof-family selector carrying its `ProofOracle`) + `TensorArity` (the structural axis the kernel tables derive from) + `OperandDomain` (the generic-math admission column) + `Lowering` (row provenance) + the `[OP_FORMS]` axis rosters + `ToleranceClass`/`ProofVerdict` (the equivalence envelope and its fold).
- Cases: thirteen `TensorOpKind` rows — elementwise, rounding, transcendental, reduction, statistics, bitwise, population, similarity, conversion, predicate, matrix, structural, geometry — each carrying its `ProofOracle`; sixteen `TensorArity` rows naming the span entrypoint a row dispatches through; six `OperandDomain` flags naming the generic-math constraint a row's kernel binds; four `Lowering` rows — member, composed, lowered, authored. The 114 operation rows cover the same surface 203 rows covered, with every name-suffix discriminant moved onto an `[OP_FORMS]` axis: `Aggregation` folds the 54 predicate triples to 18 rows, `ExtremumMetric` crossed with `NanPolicy` folds the eight `Min`/`Max` names to two rows and the four `IndexOf*` names onto the same pair through the index entrypoint, the segment entrypoint folds `SegmentSum`/`SegmentMean`/`SegmentMax`/`SegmentMin` onto the un-prefixed reduction rows (`Count` alone is new), `AngleScaling` folds the eight `Pi` companions, `NumericBase` and `SeriesForm` fold the base-2/base-10 and `M1`/`P1` exp-log variants, `OverflowPolicy` crossed with the destination type folds seven conversion names to three rows, `ShiftForm` folds five shift and rotate names to one, `PoolWindow` folds the two `Global*` pools onto their windowed rows, `BitLogic`/`ElementMap`/`PairCombine`/`ZeroEnd`/`BitStepSense`/`AngleSense`/`RemainderForm` each fold one two-or-three-name family, and the convolution rank rides the request rather than three `Conv1D`/`Conv2D`/`Conv3D` names. Three collapses are REFUSED with cause: the estimate trio keeps its rows because the tolerance column is their discriminant and the proof rail reads that column off the family; the hyperbolic six keep theirs because a π-scaled sine is one function under an argument convention while a hyperbolic sine is a different function whose `Tanh` row keys a derivative table; and `Floor`/`Ceiling`/`Truncate` keep theirs because `MidpointRounding` decides ties alone.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation is one `TensorOpFamily` row carrying its kind, tolerance, arity, domain set, and lowering; a new VARIANT of an existing operation is one row on the axis roster it varies along, never a row here; a new tolerance band is one `ToleranceClass` row; a new operation kind is one `TensorOpKind` row carrying the `ProofOracle` it certifies through; a new proof oracle is one `ProofOracle` row breaking the consumer's total `Switch`; zero new surface.
- Boundary: a row's key IS its `TensorPrimitives` member under `nameof` wherever `Lowering.Member` holds, so a renamed or retired host member is a build break rather than a silent table miss, and the Pascal-casing string transform consumers used to perform against a kebab key is deleted. `OperandDomain` is the generic-math admission column stated ONCE: seventeen predicate rows carry `Numeric` because the host binds them at `INumberBase<T>`, `IsPow2` alone carries `BinaryNumeric` because the host narrows it to `IBinaryNumber<T>`, and `System.Numerics.Complex` implements the former and not the latter — a kernel table narrowing the whole predicate family to `IBinaryNumber<T>` made the three complex-classification rows uninstantiable for the domain they exist to classify. `TensorArity` is what makes the dispatch kernel tables derivable from `Items`: a `[SmartEnum]` static field cannot hold an open-generic `UnaryKernel<T>`, so the kernel stays in its per-`T` table, but which table a row belongs to is a column rather than a hand-kept parallel roster. `ToleranceClass.Bound(length, mass)` owns absolute equivalence bounds, and `Vacuous` rejects cancellation-dominated evidence. `TensorOpKind.Oracle` is the proof-family selector the `Tensor/dispatch#EQUIVALENCE_INTEROP` `EquivalenceLaw` switches on — elementwise, rounding, transcendental, bitwise, population, predicate, and conversion kinds against the element-by-element scalar tail; reduction, statistics, and similarity against the reassociated (reversed) order; matrix against the lowered GEMM reference; structural and geometry against the operator-transpose identity over a mesh fixture. It is a ROW rather than a delegate column because the oracle bodies are dispatch-owned and a substrate roster holding a consumer's method group inverts the strata. `ToleranceClass.Verdict` is the multi-state fold `ProofVerdict` names and the ONLY reader of the envelope: an infinite bound certifies nothing, so every estimate row lands `unprovable-estimate`, and the boolean shell that re-derived which non-pass is a violation is deleted because the comment above the fold forbids exactly that re-derivation. `TensorVocabulary.Promote(left, right)` generates result dtype from numeric domain, signedness, and storage width rather than an ordered-pair roster, and a float demand no row covers is a typed exhaustion fault rather than a silent widening to the widest row the estate happens to carry. Quantized admission sequences the axis extent it depends on, then accumulates the four independent cardinality and domain facts through `Validation`; scale positivity is unrepresentable-if-invalid at `PositiveScale` rather than re-proved per call.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProofVerdict {
    public static readonly ProofVerdict Holds = new("holds", certifies: true);
    public static readonly ProofVerdict Violated = new("violated", certifies: false);
    public static readonly ProofVerdict UnprovableEstimate = new("unprovable-estimate", certifies: false);
    public static readonly ProofVerdict UnprovableCancelling = new("unprovable-cancelling", certifies: false);
    public static readonly ProofVerdict UnprovableUnmeasured = new("unprovable-unmeasured", certifies: false);

    public bool Certifies { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToleranceClass {
    public static readonly ToleranceClass Exact = new("exact", static (_, _) => 0.0);
    public static readonly ToleranceClass UlpBanded = new("ulp-banded", static (_, mass) => Math.ScaleB(4.0, -52) * mass);
    public static readonly ToleranceClass AccumulationScaled = new("accumulation-scaled", static (length, mass) => length * Math.ScaleB(1.0, -52) * mass);
    public static readonly ToleranceClass CrossPlatformVariant = new("cross-platform-variant", static (_, mass) => Math.ScaleB(16.0, -52) * mass);
    public static readonly ToleranceClass PlatformVariant = new("platform-variant", static (_, _) => double.PositiveInfinity);

    [UseDelegateFromConstructor]
    public partial double Bound(int length, double mass);

    public const double CancellationFloor = 1e-8;

    public bool Vacuous(double cancellationRatio) => this != Exact && cancellationRatio < CancellationFloor;

    public bool Certifiable => double.IsFinite(Bound(1, 1.0));

    public ProofVerdict Verdict(double deviation, int length, double mass, double cancellationRatio) =>
        !Certifiable ? ProofVerdict.UnprovableEstimate
        : !double.IsFinite(deviation) ? ProofVerdict.UnprovableUnmeasured
        : Vacuous(cancellationRatio) ? ProofVerdict.UnprovableCancelling
        : deviation <= Bound(length, mass) ? ProofVerdict.Holds
        : ProofVerdict.Violated;
}

// --- [OP_FORMS] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Aggregation {
    public static readonly Aggregation PerElement = new("per-element");
    public static readonly Aggregation All = new("all");
    public static readonly Aggregation Any = new("any");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtremumMetric {
    public static readonly ExtremumMetric Value = new("value");
    public static readonly ExtremumMetric Magnitude = new("magnitude");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NanPolicy {
    public static readonly NanPolicy Propagate = new("propagate");
    public static readonly NanPolicy Missing = new("missing");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AngleScaling {
    public static readonly AngleScaling Radians = new("radians");
    public static readonly AngleScaling Pi = new("pi");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AngleSense {
    public static readonly AngleSense DegreesToRadians = new("degrees-to-radians");
    public static readonly AngleSense RadiansToDegrees = new("radians-to-degrees");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NumericBase {
    public static readonly NumericBase Natural = new("natural");
    public static readonly NumericBase Binary = new("binary");
    public static readonly NumericBase Decimal = new("decimal");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesForm {
    public static readonly SeriesForm Direct = new("direct");
    public static readonly SeriesForm NearUnit = new("near-unit");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RemainderForm {
    public static readonly RemainderForm Ieee754 = new("ieee-754");
    public static readonly RemainderForm Truncated = new("truncated");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BitStepSense {
    public static readonly BitStepSense Increment = new("increment");
    public static readonly BitStepSense Decrement = new("decrement");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ZeroEnd {
    public static readonly ZeroEnd Leading = new("leading");
    public static readonly ZeroEnd Trailing = new("trailing");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BitLogic {
    public static readonly BitLogic And = new("and");
    public static readonly BitLogic Or = new("or");
    public static readonly BitLogic Xor = new("xor");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShiftDirection {
    public static readonly ShiftDirection Left = new("left");
    public static readonly ShiftDirection Right = new("right");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShiftFill {
    public static readonly ShiftFill Arithmetic = new("arithmetic");
    public static readonly ShiftFill Logical = new("logical");
    public static readonly ShiftFill Rotate = new("rotate");
}

public readonly record struct ShiftForm(ShiftDirection Direction, ShiftFill Fill);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElementMap {
    public static readonly ElementMap Square = new("square");
    public static readonly ElementMap Magnitude = new("magnitude");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PairCombine {
    public static readonly PairCombine Sum = new("sum");
    public static readonly PairCombine Difference = new("difference");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OverflowPolicy {
    public static readonly OverflowPolicy Checked = new("checked");
    public static readonly OverflowPolicy Saturating = new("saturating");
    public static readonly OverflowPolicy Truncating = new("truncating");
}

public readonly record struct PoolWindow(Option<(int Window, int Stride)> Strided) {
    public static readonly PoolWindow Global = new(None);
}

// --- [OP_TABLE] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorArity {
    public static readonly TensorArity Unary = new("unary");
    public static readonly TensorArity Binary = new("binary");
    public static readonly TensorArity Ternary = new("ternary");
    public static readonly TensorArity Dual = new("dual");
    public static readonly TensorArity Shift = new("shift");
    public static readonly TensorArity Sign = new("sign");
    public static readonly TensorArity Mask = new("mask");
    public static readonly TensorArity Fold = new("fold");
    public static readonly TensorArity PairFold = new("pair-fold");
    public static readonly TensorArity Magnitude = new("magnitude");
    public static readonly TensorArity Convert = new("convert");
    public static readonly TensorArity Pool = new("pool");
    public static readonly TensorArity Segment = new("segment");
    public static readonly TensorArity Matrix = new("matrix");
    public static readonly TensorArity Geometry = new("geometry");
    public static readonly TensorArity Inline = new("inline");
}

[Flags]
public enum OperandDomain {
    None = 0,
    Real = 1,
    Integer = 2,
    Numeric = 4,
    BinaryNumeric = 8,
    Complex = 16,
    Quaternion = 32,
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Lowering {
    public static readonly Lowering Member = new("member");
    public static readonly Lowering Composed = new("composed");
    public static readonly Lowering Lowered = new("lowered");
    public static readonly Lowering Authored = new("authored");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProofOracle {
    public static readonly ProofOracle ScalarTail = new("scalar-tail");
    public static readonly ProofOracle Reassociated = new("reassociated");
    public static readonly ProofOracle Lowered = new("lowered");
    public static readonly ProofOracle Fixtured = new("fixtured");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorOpKind {
    public static readonly TensorOpKind Elementwise = new("elementwise", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Rounding = new("rounding", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Transcendental = new("transcendental", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Reduction = new("reduction", ProofOracle.Reassociated);
    public static readonly TensorOpKind Statistics = new("statistics", ProofOracle.Reassociated);
    public static readonly TensorOpKind Bitwise = new("bitwise", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Population = new("population", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Similarity = new("similarity", ProofOracle.Reassociated);
    public static readonly TensorOpKind Conversion = new("conversion", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Predicate = new("predicate", ProofOracle.ScalarTail);
    public static readonly TensorOpKind Matrix = new("matrix", ProofOracle.Lowered);
    public static readonly TensorOpKind Structural = new("structural", ProofOracle.Fixtured);
    public static readonly TensorOpKind Geometry = new("geometry", ProofOracle.Fixtured);

    public ProofOracle Oracle { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorOpFamily {
    public static readonly TensorOpFamily Add = new(nameof(TensorPrimitives.Add), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real | OperandDomain.Integer | OperandDomain.Complex, Lowering.Member);
    public static readonly TensorOpFamily Subtract = new(nameof(TensorPrimitives.Subtract), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real | OperandDomain.Integer | OperandDomain.Complex, Lowering.Member);
    public static readonly TensorOpFamily Multiply = new(nameof(TensorPrimitives.Multiply), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real | OperandDomain.Integer | OperandDomain.Complex, Lowering.Member);
    public static readonly TensorOpFamily Divide = new(nameof(TensorPrimitives.Divide), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real | OperandDomain.Complex, Lowering.Member);
    public static readonly TensorOpFamily Negate = new(nameof(TensorPrimitives.Negate), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real | OperandDomain.Integer | OperandDomain.Complex, Lowering.Member);
    public static readonly TensorOpFamily Abs = new(nameof(TensorPrimitives.Abs), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real | OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily Clamp = new(nameof(TensorPrimitives.Clamp), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily CopySign = new(nameof(TensorPrimitives.CopySign), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily MultiplyAdd = new(nameof(TensorPrimitives.MultiplyAdd), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily FusedMultiplyAdd = new(nameof(TensorPrimitives.FusedMultiplyAdd), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily MultiplyAddEstimate = new(nameof(TensorPrimitives.MultiplyAddEstimate), TensorOpKind.Elementwise, ToleranceClass.PlatformVariant, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily AddMultiply = new(nameof(TensorPrimitives.AddMultiply), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Lerp = new(nameof(TensorPrimitives.Lerp), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Ternary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Hypot = new(nameof(TensorPrimitives.Hypot), TensorOpKind.Elementwise, ToleranceClass.CrossPlatformVariant, TensorArity.Binary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Reciprocal = new(nameof(TensorPrimitives.Reciprocal), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ReciprocalSqrt = new(nameof(TensorPrimitives.ReciprocalSqrt), TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ReciprocalEstimate = new(nameof(TensorPrimitives.ReciprocalEstimate), TensorOpKind.Elementwise, ToleranceClass.PlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ReciprocalSqrtEstimate = new(nameof(TensorPrimitives.ReciprocalSqrtEstimate), TensorOpKind.Elementwise, ToleranceClass.PlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Remainder = new(nameof(TensorPrimitives.Remainder), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ILogB = new(nameof(TensorPrimitives.ILogB), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Sign, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily BitStep = new(nameof(TensorPrimitives.BitIncrement), TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Conjugate = new("Conjugate", TensorOpKind.Elementwise, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Complex | OperandDomain.Quaternion, Lowering.Authored);
    public static readonly TensorOpFamily QuaternionMultiply = new("QuaternionMultiply", TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Binary, OperandDomain.Quaternion, Lowering.Authored);
    public static readonly TensorOpFamily QuaternionNormalize = new("QuaternionNormalize", TensorOpKind.Elementwise, ToleranceClass.UlpBanded, TensorArity.Unary, OperandDomain.Quaternion, Lowering.Authored);
    public static readonly TensorOpFamily ComplexAbs = new("ComplexAbs", TensorOpKind.Elementwise, ToleranceClass.CrossPlatformVariant, TensorArity.Magnitude, OperandDomain.Complex, Lowering.Authored);
    public static readonly TensorOpFamily Round = new(nameof(TensorPrimitives.Round), TensorOpKind.Rounding, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Floor = new(nameof(TensorPrimitives.Floor), TensorOpKind.Rounding, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Ceiling = new(nameof(TensorPrimitives.Ceiling), TensorOpKind.Rounding, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Truncate = new(nameof(TensorPrimitives.Truncate), TensorOpKind.Rounding, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Exp = new(nameof(TensorPrimitives.Exp), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Log = new(nameof(TensorPrimitives.Log), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Sin = new(nameof(TensorPrimitives.Sin), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Cos = new(nameof(TensorPrimitives.Cos), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Tan = new(nameof(TensorPrimitives.Tan), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily SinCos = new(nameof(TensorPrimitives.SinCos), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Dual, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Asin = new(nameof(TensorPrimitives.Asin), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Acos = new(nameof(TensorPrimitives.Acos), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Atan = new(nameof(TensorPrimitives.Atan), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Atan2 = new(nameof(TensorPrimitives.Atan2), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Binary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Sinh = new(nameof(TensorPrimitives.Sinh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Cosh = new(nameof(TensorPrimitives.Cosh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Tanh = new(nameof(TensorPrimitives.Tanh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Asinh = new(nameof(TensorPrimitives.Asinh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Acosh = new(nameof(TensorPrimitives.Acosh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Atanh = new(nameof(TensorPrimitives.Atanh), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Sigmoid = new(nameof(TensorPrimitives.Sigmoid), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily SoftMax = new(nameof(TensorPrimitives.SoftMax), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily LogSoftMax = new("LogSoftMax", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Composed);
    public static readonly TensorOpFamily ReLU = new("ReLU", TensorOpKind.Transcendental, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Composed);
    public static readonly TensorOpFamily Gelu = new("Gelu", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Composed);
    public static readonly TensorOpFamily SiLU = new("SiLU", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Composed);
    public static readonly TensorOpFamily Pow = new(nameof(TensorPrimitives.Pow), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Binary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Sqrt = new(nameof(TensorPrimitives.Sqrt), TensorOpKind.Transcendental, ToleranceClass.UlpBanded, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Cbrt = new(nameof(TensorPrimitives.Cbrt), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily RootN = new(nameof(TensorPrimitives.RootN), TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Inline, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ScaleB = new(nameof(TensorPrimitives.ScaleB), TensorOpKind.Transcendental, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily AngleConvert = new(nameof(TensorPrimitives.DegreesToRadians), TensorOpKind.Transcendental, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ComplexExp = new("ComplexExp", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Complex, Lowering.Authored);
    public static readonly TensorOpFamily ComplexLog = new("ComplexLog", TensorOpKind.Transcendental, ToleranceClass.CrossPlatformVariant, TensorArity.Unary, OperandDomain.Complex, Lowering.Authored);
    public static readonly TensorOpFamily Sum = new(nameof(TensorPrimitives.Sum), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Product = new(nameof(TensorPrimitives.Product), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Dot = new(nameof(TensorPrimitives.Dot), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.PairFold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Norm = new(nameof(TensorPrimitives.Norm), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Min = new(nameof(TensorPrimitives.Min), TensorOpKind.Reduction, ToleranceClass.Exact, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Max = new(nameof(TensorPrimitives.Max), TensorOpKind.Reduction, ToleranceClass.Exact, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily SumOf = new(nameof(TensorPrimitives.SumOfSquares), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ProductOfPairs = new(nameof(TensorPrimitives.ProductOfSums), TensorOpKind.Reduction, ToleranceClass.AccumulationScaled, TensorArity.PairFold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Count = new("Count", TensorOpKind.Reduction, ToleranceClass.Exact, TensorArity.Segment, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily Average = new(nameof(TensorPrimitives.Average), TensorOpKind.Statistics, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily StdDev = new(nameof(TensorPrimitives.StdDev), TensorOpKind.Statistics, ToleranceClass.AccumulationScaled, TensorArity.Fold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Bitwise = new(nameof(TensorPrimitives.BitwiseAnd), TensorOpKind.Bitwise, ToleranceClass.Exact, TensorArity.Binary, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily OnesComplement = new(nameof(TensorPrimitives.OnesComplement), TensorOpKind.Bitwise, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily Shift = new(nameof(TensorPrimitives.ShiftLeft), TensorOpKind.Bitwise, ToleranceClass.Exact, TensorArity.Shift, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily PopCount = new(nameof(TensorPrimitives.PopCount), TensorOpKind.Population, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily ZeroCount = new(nameof(TensorPrimitives.LeadingZeroCount), TensorOpKind.Population, ToleranceClass.Exact, TensorArity.Unary, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily CosineSimilarity = new(nameof(TensorPrimitives.CosineSimilarity), TensorOpKind.Similarity, ToleranceClass.AccumulationScaled, TensorArity.PairFold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Distance = new(nameof(TensorPrimitives.Distance), TensorOpKind.Similarity, ToleranceClass.AccumulationScaled, TensorArity.PairFold, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily HammingDistance = new(nameof(TensorPrimitives.HammingDistance), TensorOpKind.Similarity, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily HammingBitDistance = new(nameof(TensorPrimitives.HammingBitDistance), TensorOpKind.Similarity, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Integer, Lowering.Member);
    public static readonly TensorOpFamily Convert = new(nameof(TensorPrimitives.ConvertChecked), TensorOpKind.Conversion, ToleranceClass.Exact, TensorArity.Convert, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily ConvertToInteger = new(nameof(TensorPrimitives.ConvertToInteger), TensorOpKind.Conversion, ToleranceClass.Exact, TensorArity.Convert, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily ConvertHalf = new(nameof(TensorPrimitives.ConvertToHalf), TensorOpKind.Conversion, ToleranceClass.Exact, TensorArity.Convert, OperandDomain.Real, Lowering.Member);
    public static readonly TensorOpFamily Sign = new(nameof(TensorPrimitives.Sign), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Sign, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsNaN = new(nameof(TensorPrimitives.IsNaN), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsFinite = new(nameof(TensorPrimitives.IsFinite), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsInfinity = new(nameof(TensorPrimitives.IsInfinity), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsPositiveInfinity = new(nameof(TensorPrimitives.IsPositiveInfinity), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsNegativeInfinity = new(nameof(TensorPrimitives.IsNegativeInfinity), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsInteger = new(nameof(TensorPrimitives.IsInteger), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsEvenInteger = new(nameof(TensorPrimitives.IsEvenInteger), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsOddInteger = new(nameof(TensorPrimitives.IsOddInteger), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsNegative = new(nameof(TensorPrimitives.IsNegative), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsPositive = new(nameof(TensorPrimitives.IsPositive), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsZero = new(nameof(TensorPrimitives.IsZero), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsNormal = new(nameof(TensorPrimitives.IsNormal), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsSubnormal = new(nameof(TensorPrimitives.IsSubnormal), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsCanonical = new(nameof(TensorPrimitives.IsCanonical), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsComplexNumber = new(nameof(TensorPrimitives.IsComplexNumber), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsImaginaryNumber = new(nameof(TensorPrimitives.IsImaginaryNumber), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsRealNumber = new(nameof(TensorPrimitives.IsRealNumber), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.Numeric, Lowering.Member);
    public static readonly TensorOpFamily IsPow2 = new(nameof(TensorPrimitives.IsPow2), TensorOpKind.Predicate, ToleranceClass.Exact, TensorArity.Mask, OperandDomain.BinaryNumeric, Lowering.Member);
    public static readonly TensorOpFamily MatMul = new("MatMul", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled, TensorArity.Matrix, OperandDomain.Real, Lowering.Lowered);
    public static readonly TensorOpFamily Conv = new("Conv", TensorOpKind.Matrix, ToleranceClass.AccumulationScaled, TensorArity.Matrix, OperandDomain.Real, Lowering.Lowered);
    public static readonly TensorOpFamily MaxPool = new("MaxPool", TensorOpKind.Structural, ToleranceClass.Exact, TensorArity.Pool, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily AvgPool = new("AvgPool", TensorOpKind.Structural, ToleranceClass.AccumulationScaled, TensorArity.Pool, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily MaskedWrite = new("MaskedWrite", TensorOpKind.Structural, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Numeric, Lowering.Authored);
    public static readonly TensorOpFamily Gather = new("Gather", TensorOpKind.Structural, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Numeric, Lowering.Authored);
    public static readonly TensorOpFamily Scatter = new("Scatter", TensorOpKind.Structural, ToleranceClass.Exact, TensorArity.Inline, OperandDomain.Numeric, Lowering.Authored);
    public static readonly TensorOpFamily Gradient = new("Gradient", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily Divergence = new("Divergence", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily Curl = new("Curl", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily CotangentLaplacian = new("CotangentLaplacian", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily HeatFlow = new("HeatFlow", TensorOpKind.Geometry, ToleranceClass.AccumulationScaled, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);
    public static readonly TensorOpFamily Spectral = new("Spectral", TensorOpKind.Geometry, ToleranceClass.CrossPlatformVariant, TensorArity.Geometry, OperandDomain.Real, Lowering.Authored);

    public TensorOpKind Kind { get; }
    public ToleranceClass Tolerance { get; }
    public TensorArity Arity { get; }
    public OperandDomain Domains { get; }
    public Lowering Lowering { get; }

    public bool Admits(OperandDomain domain) => (Domains & domain) != OperandDomain.None;
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
