# [COMPUTE_DISPATCH]

CPU tensor dispatch binds each `TensorOpFamily` row to one arity kernel, claim-gated partition route, equivalence proof, sensitivity law, and device lowering. `TensorOps`, `EquivalenceLaw`, `SensitivityLaw`, and `DeviceDispatch` own those execution algebras; vocabulary, residency, numeric lowering, runtime receipts, and solver consumers compose their typed seams.

## [01]-[INDEX]

- [02]-[KERNEL_DISPATCH]: arity kernel-delegate tables; one `TensorOps` dispatch surface.
- [03]-[EQUIVALENCE_INTEROP]: equivalence proofs of the vector lane against its scalar-path reference; matmul route; dual-mode (forward+reverse) differentiable adjoint; generalized Gauss-Newton `JᵀJ` product; sparse-Jacobian coloring; copy-point law.
- [04]-[DEVICE_KERNELS]: WGSL compute-pipeline registry lowering matrix/structural/sparse op-family rows to `ONE_WGPU_DEVICE` workgroup dispatch behind the residency gate and a winning benchmark claim.

## [02]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `TensorOps.Map` and its arity-shaped siblings validate common extents before resolving one closed row and its FORM. `Segment` validates every segment id before grouped reduction, `Gather`/`Scatter` prove every index against the addressed extent before element movement, `Pool` validates rank, axis, window, stride, and exact destination shape before arbitrary-axis reduction, and `Partition` selects inline or blocked execution from the admitted claim. Every span-shaped method catches at its statement seam because ref-struct operands never cross an effect closure, and the filter is NARROWED to the exceptions a kernel body can actually raise past its length guard — a blanket catch turned an out-of-memory or a cancellation into a rail verdict the caller could no longer tell from a bad argument.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on the table its `TensorArity` column names; a new VARIANT of an existing operation is one corner on that table under the axis value the `[OP_FORMS]` roster already carries, never a new row and never a new entrypoint; a new activation is one `Activations<T>` composed fold with one `Plain` entry, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, a new predicate is one `PredicateKernels<T>` mask entry beside its `All`/`Any` pair on the same key, a new segmented reduction is one `SegmentReducers<T>` seed/combine/finalize row on the shared `Segment` fold, an index-driven structural op is one row-gated span arity beside `Gather`/`Scatter`, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/aggregate/segment/complex method; a matrix kernel is one lowering row read from `Tensor/factor#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path whose shape reads the winning claim's own partition count and falls back to `CpuBudget.PartitionCap`, never a new owner; zero new surface.
- Boundary: arity tables bind only verified `TensorPrimitives` members at compatible generic constraints. Author folds cover activation, complex, and quaternion operations that have no direct member, and vector normalization composes `Norm` then `Divide` against the reduced magnitude rather than binding a row of its own; matrix operations lower through the numeric lane; pooling reduces arbitrary axes through tuple policy rows; predicates, reductions, masks, segments, index gathers/scatters, partitions, and conversions retain their distinct destination and admission shapes. Frozen indexes use ordinal comparison, and ref-struct kernels remain statement-shaped. Kernel-interior `SpanOwner`/`MemoryOwner` scratch is EXEMPT from the `Tensor/memory#ALLOCATION_AXIS` `AllocationClass.Grant` edge by declared law: a rent whose entire life is one kernel body — sized by the operand extent the caller already admitted, released on the same frame, visible to no other lane — produces evidence no receipt reader acts on, and granting it would stamp one `AllocationEvidence` value per elementwise call. `Grant` admits STAGING allocations alone, so an interior rent neither re-grants nor picks a class at the call site. The row/kernel pairing is proved by the `TensorArity` COLUMN the vocabulary row carries, not by a boot-time census: a census re-proving a mapping no type held needed a seven-row identity set to patch its own false gaps, a thirteen-arm owner probe restating which table each kind lives in, and a ten-way disjunction under it — and no composition root ever called it, so the whole scaffold was the cost of the mirror it re-proved. With arity on the row, a family minted under an arity whose table it never entered surfaces as `kernel-row-miss` at its one resolution site with the arity named.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public delegate void UnaryKernel<T>(ReadOnlySpan<T> x, Span<T> destination);
public delegate void BinaryKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination);
public delegate void TernaryKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination);
public delegate void DualKernel<T>(ReadOnlySpan<T> x, Span<T> first, Span<T> second);
public delegate void CountedKernel<T>(ReadOnlySpan<T> x, int count, Span<T> destination);
public delegate void ConvertKernel<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination);
public delegate void SignKernel<T>(ReadOnlySpan<T> x, Span<int> destination);
public delegate void MaskKernel<T>(ReadOnlySpan<T> x, Span<bool> destination);
public delegate T FoldKernel<T>(ReadOnlySpan<T> x);
public delegate T PairFoldKernel<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y);
public delegate int IndexKernel<T>(ReadOnlySpan<T> x);
public delegate bool AggregateKernel<T>(ReadOnlySpan<T> x);
public delegate void MagnitudeKernel(ReadOnlySpan<Complex> x, Span<double> destination);

// The request SHAPE a span entrypoint discriminates on. A row that admits an axis carries its value here rather
// than in its key, so `Map` stays ONE entrypoint over the whole unary family instead of growing a `Series`, a
// `Scaled`, and a `Stepped` sibling that differ only by which suffix the old key string spelled.
[Union]
public abstract partial record UnaryForm {
    private UnaryForm() { }

    public sealed record Plain : UnaryForm;
    public sealed record Scaled(AngleScaling Scaling) : UnaryForm;
    public sealed record Series(NumericBase Base, SeriesForm Precision) : UnaryForm;
    public sealed record Stepped(BitStepSense Sense) : UnaryForm;
    public sealed record Angular(AngleSense Sense) : UnaryForm;
    public sealed record Counted(ZeroEnd End) : UnaryForm;
    public sealed record Rounded(MidpointRounding Mode) : UnaryForm;
}

[Union]
public abstract partial record BinaryForm {
    private BinaryForm() { }

    public sealed record Plain : BinaryForm;
    public sealed record Scaled(AngleScaling Scaling) : BinaryForm;
    public sealed record Remainder(RemainderForm Convention) : BinaryForm;
    public sealed record Logic(BitLogic Operation) : BinaryForm;
}

[Union]
public abstract partial record FoldForm {
    private FoldForm() { }

    public sealed record Plain : FoldForm;
    public sealed record Extremum(ExtremumMetric Metric, NanPolicy Nan) : FoldForm;
    public sealed record Mapped(ElementMap Element) : FoldForm;
}

[Union]
public abstract partial record PairFoldForm {
    private PairFoldForm() { }

    public sealed record Plain : PairFoldForm;
    public sealed record Combined(PairCombine Combine) : PairFoldForm;
}

// --- [KERNEL_TABLES] -----------------------------------------------------------------------
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

// Window and segment folds bind the two POOL rows and the reduction rows a segment lane admits; a global pool is
// the windowed row at `PoolWindow.Global`, never a fourth reducer, and a segmented sum is the `Sum` row under
// segment entrypoint, never a `SegmentSum` roster entry.
public static class PoolReducers<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, (T Seed, Func<T, T, T> Combine, Func<T, int, T> Final)> Rows =
        new Dictionary<TensorOpFamily, (T, Func<T, T, T>, Func<T, int, T>)> {
        [TensorOpFamily.MaxPool] = (T.NegativeInfinity, static (acc, value) => T.Max(acc, value), static (acc, _) => acc),
        [TensorOpFamily.AvgPool] = (T.Zero, static (acc, value) => acc + value, static (acc, count) => acc / T.CreateChecked(count)),
    }.ToFrozenDictionary();
}

public static class SegmentReducers<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, (T Seed, Func<T, T, T> Combine, Func<T, int, T> Final)> Rows =
        new Dictionary<TensorOpFamily, (T, Func<T, T, T>, Func<T, int, T>)> {
        [TensorOpFamily.Sum] = new(T.Zero, static (acc, value) => acc + value, static (acc, _) => acc),
        [TensorOpFamily.Average] = new(T.Zero, static (acc, value) => acc + value, static (acc, count) => count > 0 ? acc / T.CreateChecked(count) : T.NaN),
        [TensorOpFamily.Max] = new(T.NegativeInfinity, static (acc, value) => T.Max(acc, value), static (acc, _) => acc),
        [TensorOpFamily.Min] = new(T.PositiveInfinity, static (acc, value) => T.Min(acc, value), static (acc, _) => acc),
        [TensorOpFamily.Count] = new(T.Zero, static (acc, _) => acc + T.One, static (acc, _) => acc),
    }.ToFrozenDictionary();
}

// Predicates bind `INumberBase<T>` because that is what the host binds them at — verified on the installed
// surface, where every predicate but the `IsPow2` triad takes `INumberBase<T>`. The prior `IBinaryNumber<T>`
// clause on this table made `PredicateKernels<Complex>` uninstantiable and left nine complex-classification rows
// unreachable for the very domain they classify, because `Complex` implements `INumberBase<Complex>` and not
// `IBinaryNumber<Complex>`. `IsPow2` carves onto its own table at the constraint the host actually demands.
public static class PredicateKernels<T> where T : INumberBase<T> {
    public static readonly FrozenDictionary<TensorOpFamily, MaskKernel<T>> Mask = new Dictionary<TensorOpFamily, MaskKernel<T>> {
        [TensorOpFamily.IsNaN] = TensorPrimitives.IsNaN, [TensorOpFamily.IsFinite] = TensorPrimitives.IsFinite,
        [TensorOpFamily.IsInfinity] = TensorPrimitives.IsInfinity, [TensorOpFamily.IsPositiveInfinity] = TensorPrimitives.IsPositiveInfinity,
        [TensorOpFamily.IsNegativeInfinity] = TensorPrimitives.IsNegativeInfinity, [TensorOpFamily.IsInteger] = TensorPrimitives.IsInteger,
        [TensorOpFamily.IsEvenInteger] = TensorPrimitives.IsEvenInteger, [TensorOpFamily.IsOddInteger] = TensorPrimitives.IsOddInteger,
        [TensorOpFamily.IsNegative] = TensorPrimitives.IsNegative, [TensorOpFamily.IsPositive] = TensorPrimitives.IsPositive,
        [TensorOpFamily.IsZero] = TensorPrimitives.IsZero, [TensorOpFamily.IsNormal] = TensorPrimitives.IsNormal,
        [TensorOpFamily.IsSubnormal] = TensorPrimitives.IsSubnormal, [TensorOpFamily.IsCanonical] = TensorPrimitives.IsCanonical,
        [TensorOpFamily.IsComplexNumber] = TensorPrimitives.IsComplexNumber, [TensorOpFamily.IsImaginaryNumber] = TensorPrimitives.IsImaginaryNumber,
        [TensorOpFamily.IsRealNumber] = TensorPrimitives.IsRealNumber,
    }.ToFrozenDictionary();

    // The `All`/`Any` reducers ride the SAME row as their mask under an `Aggregation` value, so the eighteen
    // triples the roster used to spell as `is-nan`/`is-nan-all`/`is-nan-any` are eighteen rows and one axis.
    public static readonly FrozenDictionary<TensorOpFamily, (AggregateKernel<T> All, AggregateKernel<T> Any)> Aggregate =
        new Dictionary<TensorOpFamily, (AggregateKernel<T>, AggregateKernel<T>)> {
        [TensorOpFamily.IsNaN] = (TensorPrimitives.IsNaNAll, TensorPrimitives.IsNaNAny),
        [TensorOpFamily.IsFinite] = (TensorPrimitives.IsFiniteAll, TensorPrimitives.IsFiniteAny),
        [TensorOpFamily.IsInfinity] = (TensorPrimitives.IsInfinityAll, TensorPrimitives.IsInfinityAny),
        [TensorOpFamily.IsPositiveInfinity] = (TensorPrimitives.IsPositiveInfinityAll, TensorPrimitives.IsPositiveInfinityAny),
        [TensorOpFamily.IsNegativeInfinity] = (TensorPrimitives.IsNegativeInfinityAll, TensorPrimitives.IsNegativeInfinityAny),
        [TensorOpFamily.IsInteger] = (TensorPrimitives.IsIntegerAll, TensorPrimitives.IsIntegerAny),
        [TensorOpFamily.IsEvenInteger] = (TensorPrimitives.IsEvenIntegerAll, TensorPrimitives.IsEvenIntegerAny),
        [TensorOpFamily.IsOddInteger] = (TensorPrimitives.IsOddIntegerAll, TensorPrimitives.IsOddIntegerAny),
        [TensorOpFamily.IsNegative] = (TensorPrimitives.IsNegativeAll, TensorPrimitives.IsNegativeAny),
        [TensorOpFamily.IsPositive] = (TensorPrimitives.IsPositiveAll, TensorPrimitives.IsPositiveAny),
        [TensorOpFamily.IsZero] = (TensorPrimitives.IsZeroAll, TensorPrimitives.IsZeroAny),
        [TensorOpFamily.IsNormal] = (TensorPrimitives.IsNormalAll, TensorPrimitives.IsNormalAny),
        [TensorOpFamily.IsSubnormal] = (TensorPrimitives.IsSubnormalAll, TensorPrimitives.IsSubnormalAny),
        [TensorOpFamily.IsCanonical] = (TensorPrimitives.IsCanonicalAll, TensorPrimitives.IsCanonicalAny),
        [TensorOpFamily.IsComplexNumber] = (TensorPrimitives.IsComplexNumberAll, TensorPrimitives.IsComplexNumberAny),
        [TensorOpFamily.IsImaginaryNumber] = (TensorPrimitives.IsImaginaryNumberAll, TensorPrimitives.IsImaginaryNumberAny),
        [TensorOpFamily.IsRealNumber] = (TensorPrimitives.IsRealNumberAll, TensorPrimitives.IsRealNumberAny),
    }.ToFrozenDictionary();
}

public static class Pow2Kernels<T> where T : IBinaryNumber<T> {
    public static readonly MaskKernel<T> Mask = TensorPrimitives.IsPow2;
    public static readonly (AggregateKernel<T> All, AggregateKernel<T> Any) Aggregate = (TensorPrimitives.IsPow2All, TensorPrimitives.IsPow2Any);
}

public static class TensorKernels<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Plain = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate, [TensorOpFamily.Abs] = TensorPrimitives.Abs,
        [TensorOpFamily.Floor] = TensorPrimitives.Floor, [TensorOpFamily.Ceiling] = TensorPrimitives.Ceiling, [TensorOpFamily.Truncate] = TensorPrimitives.Truncate,
        [TensorOpFamily.Sigmoid] = TensorPrimitives.Sigmoid, [TensorOpFamily.SoftMax] = TensorPrimitives.SoftMax,
        [TensorOpFamily.Sqrt] = TensorPrimitives.Sqrt, [TensorOpFamily.Cbrt] = TensorPrimitives.Cbrt,
        [TensorOpFamily.Reciprocal] = TensorPrimitives.Reciprocal, [TensorOpFamily.ReciprocalSqrt] = TensorPrimitives.ReciprocalSqrt,
        [TensorOpFamily.ReciprocalEstimate] = TensorPrimitives.ReciprocalEstimate, [TensorOpFamily.ReciprocalSqrtEstimate] = TensorPrimitives.ReciprocalSqrtEstimate,
        [TensorOpFamily.Sinh] = TensorPrimitives.Sinh, [TensorOpFamily.Cosh] = TensorPrimitives.Cosh, [TensorOpFamily.Tanh] = TensorPrimitives.Tanh,
        [TensorOpFamily.Asinh] = TensorPrimitives.Asinh, [TensorOpFamily.Acosh] = TensorPrimitives.Acosh, [TensorOpFamily.Atanh] = TensorPrimitives.Atanh,
        [TensorOpFamily.ReLU] = Activations<T>.ReLU, [TensorOpFamily.Gelu] = Activations<T>.Gelu, [TensorOpFamily.SiLU] = Activations<T>.SiLU,
        [TensorOpFamily.LogSoftMax] = Activations<T>.LogSoftMax,
    }.ToFrozenDictionary();

    // The argument-scaling axis: one trig function per row, two conventions per key. The eight `Pi` companions
    // the roster used to carry as separate rows are these eight entries under a typed axis value.
    public static readonly FrozenDictionary<(TensorOpFamily Row, AngleScaling Scaling), UnaryKernel<T>> Scaled =
        new Dictionary<(TensorOpFamily, AngleScaling), UnaryKernel<T>> {
        [(TensorOpFamily.Sin, AngleScaling.Radians)] = TensorPrimitives.Sin, [(TensorOpFamily.Sin, AngleScaling.Pi)] = TensorPrimitives.SinPi,
        [(TensorOpFamily.Cos, AngleScaling.Radians)] = TensorPrimitives.Cos, [(TensorOpFamily.Cos, AngleScaling.Pi)] = TensorPrimitives.CosPi,
        [(TensorOpFamily.Tan, AngleScaling.Radians)] = TensorPrimitives.Tan, [(TensorOpFamily.Tan, AngleScaling.Pi)] = TensorPrimitives.TanPi,
        [(TensorOpFamily.Asin, AngleScaling.Radians)] = TensorPrimitives.Asin, [(TensorOpFamily.Asin, AngleScaling.Pi)] = TensorPrimitives.AsinPi,
        [(TensorOpFamily.Acos, AngleScaling.Radians)] = TensorPrimitives.Acos, [(TensorOpFamily.Acos, AngleScaling.Pi)] = TensorPrimitives.AcosPi,
        [(TensorOpFamily.Atan, AngleScaling.Radians)] = TensorPrimitives.Atan, [(TensorOpFamily.Atan, AngleScaling.Pi)] = TensorPrimitives.AtanPi,
    }.ToFrozenDictionary();

    // The exponential-base and near-unit axes crossed: six exp members and six log members under two rows.
    public static readonly FrozenDictionary<(TensorOpFamily Row, NumericBase Base, SeriesForm Precision), UnaryKernel<T>> Series =
        new Dictionary<(TensorOpFamily, NumericBase, SeriesForm), UnaryKernel<T>> {
        [(TensorOpFamily.Exp, NumericBase.Natural, SeriesForm.Direct)] = TensorPrimitives.Exp,
        [(TensorOpFamily.Exp, NumericBase.Binary, SeriesForm.Direct)] = TensorPrimitives.Exp2,
        [(TensorOpFamily.Exp, NumericBase.Decimal, SeriesForm.Direct)] = TensorPrimitives.Exp10,
        [(TensorOpFamily.Exp, NumericBase.Natural, SeriesForm.NearUnit)] = TensorPrimitives.ExpM1,
        [(TensorOpFamily.Exp, NumericBase.Binary, SeriesForm.NearUnit)] = TensorPrimitives.Exp2M1,
        [(TensorOpFamily.Exp, NumericBase.Decimal, SeriesForm.NearUnit)] = TensorPrimitives.Exp10M1,
        [(TensorOpFamily.Log, NumericBase.Natural, SeriesForm.Direct)] = TensorPrimitives.Log,
        [(TensorOpFamily.Log, NumericBase.Binary, SeriesForm.Direct)] = TensorPrimitives.Log2,
        [(TensorOpFamily.Log, NumericBase.Decimal, SeriesForm.Direct)] = TensorPrimitives.Log10,
        [(TensorOpFamily.Log, NumericBase.Natural, SeriesForm.NearUnit)] = TensorPrimitives.LogP1,
        [(TensorOpFamily.Log, NumericBase.Binary, SeriesForm.NearUnit)] = TensorPrimitives.Log2P1,
        [(TensorOpFamily.Log, NumericBase.Decimal, SeriesForm.NearUnit)] = TensorPrimitives.Log10P1,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<BitStepSense, UnaryKernel<T>> Stepped = new Dictionary<BitStepSense, UnaryKernel<T>> {
        [BitStepSense.Increment] = TensorPrimitives.BitIncrement, [BitStepSense.Decrement] = TensorPrimitives.BitDecrement,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<AngleSense, UnaryKernel<T>> Angular = new Dictionary<AngleSense, UnaryKernel<T>> {
        [AngleSense.DegreesToRadians] = TensorPrimitives.DegreesToRadians, [AngleSense.RadiansToDegrees] = TensorPrimitives.RadiansToDegrees,
    }.ToFrozenDictionary();

    // `Round` is the ONE row whose midpoint convention is a request value; `Floor`/`Ceiling`/`Truncate` are
    // separate rows because the mode decides ties alone and never the direction of an ordinary value.
    public static void Rounded(ReadOnlySpan<T> x, MidpointRounding mode, Span<T> destination) => TensorPrimitives.Round(x, mode, destination);

    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<T>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<T>> {
        [TensorOpFamily.Add] = TensorPrimitives.Add, [TensorOpFamily.Subtract] = TensorPrimitives.Subtract, [TensorOpFamily.Multiply] = TensorPrimitives.Multiply,
        [TensorOpFamily.Divide] = TensorPrimitives.Divide, [TensorOpFamily.Pow] = TensorPrimitives.Pow,
        [TensorOpFamily.CopySign] = TensorPrimitives.CopySign, [TensorOpFamily.Hypot] = TensorPrimitives.Hypot,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<AngleScaling, BinaryKernel<T>> ScaledBinary = new Dictionary<AngleScaling, BinaryKernel<T>> {
        [AngleScaling.Radians] = TensorPrimitives.Atan2, [AngleScaling.Pi] = TensorPrimitives.Atan2Pi,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<RemainderForm, BinaryKernel<T>> Remainders = new Dictionary<RemainderForm, BinaryKernel<T>> {
        [RemainderForm.Ieee754] = TensorPrimitives.Ieee754Remainder, [RemainderForm.Truncated] = TensorPrimitives.Remainder,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<TensorOpFamily, TernaryKernel<T>> Ternary = new Dictionary<TensorOpFamily, TernaryKernel<T>> {
        [TensorOpFamily.MultiplyAdd] = TensorPrimitives.MultiplyAdd, [TensorOpFamily.FusedMultiplyAdd] = TensorPrimitives.FusedMultiplyAdd, [TensorOpFamily.MultiplyAddEstimate] = TensorPrimitives.MultiplyAddEstimate,
        [TensorOpFamily.AddMultiply] = TensorPrimitives.AddMultiply, [TensorOpFamily.Clamp] = TensorPrimitives.Clamp, [TensorOpFamily.Lerp] = TensorPrimitives.Lerp,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<AngleScaling, DualKernel<T>> Dual = new Dictionary<AngleScaling, DualKernel<T>> {
        [AngleScaling.Radians] = TensorPrimitives.SinCos, [AngleScaling.Pi] = TensorPrimitives.SinCosPi,
    }.ToFrozenDictionary();

    // `ILogB` binds `IFloatingPointIeee754<T>` while `Sign` binds `INumber<T>`; the two shared one table under
    // the narrower clause, so a signed integer span could not reach the sign kernel the host publishes for it.
    public static readonly SignKernel<T> ExponentOf = TensorPrimitives.ILogB;

    public static readonly FrozenDictionary<TensorOpFamily, FoldKernel<T>> Fold = new Dictionary<TensorOpFamily, FoldKernel<T>> {
        [TensorOpFamily.Sum] = TensorPrimitives.Sum, [TensorOpFamily.Product] = TensorPrimitives.Product,
        [TensorOpFamily.Norm] = TensorPrimitives.Norm, [TensorOpFamily.Average] = TensorPrimitives.Average, [TensorOpFamily.StdDev] = TensorPrimitives.StdDev,
    }.ToFrozenDictionary();

    // The extremum corner: two rows crossed with the metric and NaN axes the eight `Min`/`Max` keys used to
    // concatenate. `Index` pairs with the value and magnitude metrics alone because the host publishes no
    // NaN-missing index search — the corner refuses by name rather than resolving to a neighbouring member.
    public static readonly FrozenDictionary<(TensorOpFamily Row, ExtremumMetric Metric, NanPolicy Nan), FoldKernel<T>> Extremum =
        new Dictionary<(TensorOpFamily, ExtremumMetric, NanPolicy), FoldKernel<T>> {
        [(TensorOpFamily.Min, ExtremumMetric.Value, NanPolicy.Propagate)] = TensorPrimitives.Min,
        [(TensorOpFamily.Min, ExtremumMetric.Value, NanPolicy.Missing)] = TensorPrimitives.MinNumber,
        [(TensorOpFamily.Min, ExtremumMetric.Magnitude, NanPolicy.Propagate)] = TensorPrimitives.MinMagnitude,
        [(TensorOpFamily.Min, ExtremumMetric.Magnitude, NanPolicy.Missing)] = TensorPrimitives.MinMagnitudeNumber,
        [(TensorOpFamily.Max, ExtremumMetric.Value, NanPolicy.Propagate)] = TensorPrimitives.Max,
        [(TensorOpFamily.Max, ExtremumMetric.Value, NanPolicy.Missing)] = TensorPrimitives.MaxNumber,
        [(TensorOpFamily.Max, ExtremumMetric.Magnitude, NanPolicy.Propagate)] = TensorPrimitives.MaxMagnitude,
        [(TensorOpFamily.Max, ExtremumMetric.Magnitude, NanPolicy.Missing)] = TensorPrimitives.MaxMagnitudeNumber,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<(TensorOpFamily Row, ExtremumMetric Metric), IndexKernel<T>> Index =
        new Dictionary<(TensorOpFamily, ExtremumMetric), IndexKernel<T>> {
        [(TensorOpFamily.Min, ExtremumMetric.Value)] = TensorPrimitives.IndexOfMin,
        [(TensorOpFamily.Min, ExtremumMetric.Magnitude)] = TensorPrimitives.IndexOfMinMagnitude,
        [(TensorOpFamily.Max, ExtremumMetric.Value)] = TensorPrimitives.IndexOfMax,
        [(TensorOpFamily.Max, ExtremumMetric.Magnitude)] = TensorPrimitives.IndexOfMaxMagnitude,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<ElementMap, FoldKernel<T>> Mapped = new Dictionary<ElementMap, FoldKernel<T>> {
        [ElementMap.Square] = TensorPrimitives.SumOfSquares, [ElementMap.Magnitude] = TensorPrimitives.SumOfMagnitudes,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<TensorOpFamily, PairFoldKernel<T>> PairFold = new Dictionary<TensorOpFamily, PairFoldKernel<T>> {
        [TensorOpFamily.Dot] = TensorPrimitives.Dot, [TensorOpFamily.CosineSimilarity] = TensorPrimitives.CosineSimilarity, [TensorOpFamily.Distance] = TensorPrimitives.Distance,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<PairCombine, PairFoldKernel<T>> Combined = new Dictionary<PairCombine, PairFoldKernel<T>> {
        [PairCombine.Sum] = TensorPrimitives.ProductOfSums, [PairCombine.Difference] = TensorPrimitives.ProductOfDifferences,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<TensorOpFamily, CountedKernel<T>> Counted = new Dictionary<TensorOpFamily, CountedKernel<T>> {
        [TensorOpFamily.RootN] = TensorPrimitives.RootN, [TensorOpFamily.ScaleB] = TensorPrimitives.ScaleB,
    }.ToFrozenDictionary();
}

public static class SignKernels<T> where T : INumber<T> {
    public static readonly SignKernel<T> Sign = TensorPrimitives.Sign;
}

public static class IntegerKernels<T> where T : IBinaryInteger<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.OnesComplement] = TensorPrimitives.OnesComplement, [TensorOpFamily.PopCount] = TensorPrimitives.PopCount,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<ZeroEnd, UnaryKernel<T>> Counted = new Dictionary<ZeroEnd, UnaryKernel<T>> {
        [ZeroEnd.Leading] = TensorPrimitives.LeadingZeroCount, [ZeroEnd.Trailing] = TensorPrimitives.TrailingZeroCount,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<BitLogic, BinaryKernel<T>> Logic = new Dictionary<BitLogic, BinaryKernel<T>> {
        [BitLogic.And] = TensorPrimitives.BitwiseAnd, [BitLogic.Or] = TensorPrimitives.BitwiseOr, [BitLogic.Xor] = TensorPrimitives.Xor,
    }.ToFrozenDictionary();
    // A left shift has no arithmetic fill, so that corner is absent from the table and refuses by name rather
    // than resolving to whichever neighbouring member a five-name roster happened to place beside it.
    public static readonly FrozenDictionary<ShiftForm, CountedKernel<T>> Shift = new Dictionary<ShiftForm, CountedKernel<T>> {
        [new ShiftForm(ShiftDirection.Left, ShiftFill.Logical)] = TensorPrimitives.ShiftLeft,
        [new ShiftForm(ShiftDirection.Right, ShiftFill.Arithmetic)] = TensorPrimitives.ShiftRightArithmetic,
        [new ShiftForm(ShiftDirection.Right, ShiftFill.Logical)] = TensorPrimitives.ShiftRightLogical,
        [new ShiftForm(ShiftDirection.Left, ShiftFill.Rotate)] = TensorPrimitives.RotateLeft,
        [new ShiftForm(ShiftDirection.Right, ShiftFill.Rotate)] = TensorPrimitives.RotateRight,
    }.ToFrozenDictionary();
}

public static class ConvertKernels<TFrom, TTo> where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> {
    public static readonly FrozenDictionary<OverflowPolicy, ConvertKernel<TFrom, TTo>> Rows = new Dictionary<OverflowPolicy, ConvertKernel<TFrom, TTo>> {
        [OverflowPolicy.Checked] = TensorPrimitives.ConvertChecked, [OverflowPolicy.Saturating] = TensorPrimitives.ConvertSaturating,
        [OverflowPolicy.Truncating] = TensorPrimitives.ConvertTruncating,
    }.ToFrozenDictionary();
}

public static class IntegerConvertKernels<TFrom, TTo> where TFrom : IFloatingPoint<TFrom> where TTo : IBinaryInteger<TTo> {
    public static readonly ConvertKernel<TFrom, TTo> Managed = TensorPrimitives.ConvertToInteger;
    public static readonly ConvertKernel<TFrom, TTo> Native = TensorPrimitives.ConvertToIntegerNative;
}

public static class HalfConvertKernels {
    public static readonly ConvertKernel<float, Half> Narrow = TensorPrimitives.ConvertToHalf;
    public static readonly ConvertKernel<Half, float> Widen = TensorPrimitives.ConvertToSingle;
}

public static class ComplexKernels {
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<Complex>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<Complex>> {
        [TensorOpFamily.Add] = TensorPrimitives.Add, [TensorOpFamily.Subtract] = TensorPrimitives.Subtract,
        [TensorOpFamily.Multiply] = TensorPrimitives.Multiply, [TensorOpFamily.Divide] = TensorPrimitives.Divide,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<Complex>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<Complex>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate,
        [TensorOpFamily.Conjugate] = Projection.Elementwise<Complex>(Complex.Conjugate),
        [TensorOpFamily.ComplexExp] = Projection.Elementwise<Complex>(Complex.Exp),
        [TensorOpFamily.ComplexLog] = Projection.Elementwise<Complex>(Complex.Log),
    }.ToFrozenDictionary();
    public static readonly MagnitudeKernel Magnitude = Projection.Magnitude(static x => x.Magnitude);
}

public static class QuaternionKernels {
    public static readonly BinaryKernel<Quaternion> Multiply = Projection.ElementwisePair<Quaternion>(static (a, b) => a * b);
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<Quaternion>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<Quaternion>> {
        [TensorOpFamily.Conjugate] = Projection.Elementwise<Quaternion>(Quaternion.Conjugate),
        [TensorOpFamily.QuaternionNormalize] = Projection.Elementwise<Quaternion>(Quaternion.Normalize),
    }.ToFrozenDictionary();
}

public static class Projection {
    public static UnaryKernel<TElem> Elementwise<TElem>(Func<TElem, TElem> f) =>
        (x, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = f(x[i]); } };
    public static BinaryKernel<TElem> ElementwisePair<TElem>(Func<TElem, TElem, TElem> g) =>
        (x, y, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = g(x[i], y[i]); } };
    public static MagnitudeKernel Magnitude(Func<Complex, double> m) =>
        (x, dst) => { for (int i = 0; i < x.Length; i++) { dst[i] = m(x[i]); } };
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorOps {
    // Span operands are ref structs and cross no closure, so every span entry is the named kernel statement
    // seam: admit the common extent, resolve the row and its form, invoke in place, convert a throw once. The
    // filter is NARROW by row family: after the length guard the only host throws are an argument-range refusal
    // and a checked-conversion overflow, and a blanket `catch (Exception)` turned an `OutOfMemoryException` or a
    // cancellation into a `kernel-threw` rail verdict — a success-shaped classification of an unrecoverable
    // condition the caller can no longer distinguish.
    private static Fin<Unit> Mismatch(TensorOpFamily row, int expected, int actual) =>
        TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{expected}!={actual}");
    private static Fin<A> Threw<A>(Exception ex) =>
        Fin.Fail<A>(Error.New(ex.Message, ex));
    private static Fin<A> Miss<A>(TensorOpFamily row) => TensorReason.RowMissing.Fail<A>("kernel-row-miss", row.Key);
    private static Fin<A> Corner<A>(TensorOpFamily row, string form) => TensorReason.RowMissing.Fail<A>("kernel-form-miss", row.Key, form);

    // The one RUN-TIME admission for a request that did not come from the interior. A generic-math constraint
    // gates the (row, element) pair at compile time for every call the interior makes, so no dispatch table can
    // hold an invalid row — but a request crossing the model boundary carries an op key and a dtype both chosen
    // at run time, and this is where the row's own arity and domain columns refuse a mismatched entrypoint or a
    // carrier its kernel cannot bind, rather than the caller discovering either as a missing table entry.
    public static Fin<TensorOpFamily> Admit(TensorOpFamily row, TensorArity arity, TensorDtype dtype) =>
        row.Arity != arity ? TensorReason.OperandDomainMiss.Fail<TensorOpFamily>("op-arity", row.Key, $"{row.Arity.Key}!={arity.Key}")
        : row.Admits(dtype.Domain) ? Fin.Succ(row)
        : TensorReason.OperandDomainMiss.Fail<TensorOpFamily>("op-domain", row.Key, $"{dtype.Key}:{dtype.Domain}");

    // ONE unary entrypoint over the whole family: the form discriminates which table holds the row's kernel, so
    // a scaled trig call, a near-unit logarithm, and a plain negate are one signature and one prologue.
    public static Fin<Unit> Map<T>(TensorOpFamily row, UnaryForm form, ReadOnlySpan<T> x, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        Fin<UnaryKernel<T>> resolved = form.Switch<Fin<UnaryKernel<T>>>(
            plain: _ => TensorKernels<T>.Plain.GetValueOrDefault(row) is { } k ? Fin.Succ(k) : Miss<UnaryKernel<T>>(row),
            scaled: s => TensorKernels<T>.Scaled.GetValueOrDefault((row, s.Scaling)) is { } k ? Fin.Succ(k) : Corner<UnaryKernel<T>>(row, s.Scaling.Key),
            series: s => TensorKernels<T>.Series.GetValueOrDefault((row, s.Base, s.Precision)) is { } k ? Fin.Succ(k) : Corner<UnaryKernel<T>>(row, $"{s.Base.Key}:{s.Precision.Key}"),
            stepped: s => TensorKernels<T>.Stepped.GetValueOrDefault(s.Sense) is { } k ? Fin.Succ(k) : Corner<UnaryKernel<T>>(row, s.Sense.Key),
            angular: a => TensorKernels<T>.Angular.GetValueOrDefault(a.Sense) is { } k ? Fin.Succ(k) : Corner<UnaryKernel<T>>(row, a.Sense.Key),
            counted: c => Corner<UnaryKernel<T>>(row, c.End.Key),
            rounded: r => Fin.Succ<UnaryKernel<T>>((source, target) => TensorKernels<T>.Rounded(source, r.Mode, target)));
        return resolved.Bind(kernel => Invoke(row, kernel, x, destination));
    }

    private static Fin<Unit> Invoke<T>(TensorOpFamily row, UnaryKernel<T> kernel, ReadOnlySpan<T> x, Span<T> destination) {
        try { kernel(x, destination); return Fin.Succ(unit); }
        catch (Exception ex) when (ex is ArgumentException or OverflowException or NotSupportedException) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Zip<T>(TensorOpFamily row, BinaryForm form, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        Fin<BinaryKernel<T>> resolved = form.Switch<Fin<BinaryKernel<T>>>(
            plain: _ => TensorKernels<T>.Binary.GetValueOrDefault(row) is { } k ? Fin.Succ(k) : Miss<BinaryKernel<T>>(row),
            scaled: s => TensorKernels<T>.ScaledBinary.GetValueOrDefault(s.Scaling) is { } k ? Fin.Succ(k) : Corner<BinaryKernel<T>>(row, s.Scaling.Key),
            remainder: r => TensorKernels<T>.Remainders.GetValueOrDefault(r.Convention) is { } k ? Fin.Succ(k) : Corner<BinaryKernel<T>>(row, r.Convention.Key),
            logic: l => Corner<BinaryKernel<T>>(row, l.Operation.Key));
        return resolved.Bind(kernel => {
            try { kernel(x, y, destination); return Fin.Succ(unit); }
            catch (Exception ex) when (ex is ArgumentException or OverflowException) { return Threw<Unit>(ex); }
        });
    }

    public static Fin<Unit> Fuse<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length || y.Length != destination.Length || z.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Ternary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, z, destination); return Fin.Succ(unit); }
        catch (Exception ex) when (ex is ArgumentException or OverflowException) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Bits<T>(TensorOpFamily row, BitLogic operation, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IBinaryInteger<T> {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (IntegerKernels<T>.Logic.GetValueOrDefault(operation) is not { } kernel) { return Corner<Unit>(row, operation.Key); }
        try { kernel(x, y, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Shift<T>(TensorOpFamily row, ShiftForm form, ReadOnlySpan<T> x, int shiftCount, Span<T> destination) where T : IBinaryInteger<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (IntegerKernels<T>.Shift.GetValueOrDefault(form) is not { } kernel) { return Corner<Unit>(row, $"{form.Direction.Key}:{form.Fill.Key}"); }
        try { kernel(x, shiftCount, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    // `RootN` and `ScaleB` take an integer beside the span; the two-row identity ladder that tested each row by
    // name inside a `try` is a table like every other arity.
    public static Fin<Unit> Root<T>(TensorOpFamily row, ReadOnlySpan<T> x, int n, Span<T> destination) where T : IFloatingPointIeee754<T>, IRootFunctions<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Counted.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, n, destination); return Fin.Succ(unit); }
        catch (Exception ex) when (ex is ArgumentException or OverflowException) { return Threw<Unit>(ex); }
    }

    // The conversion request is a POLICY and a TARGET; the seven conversion rows the vocabulary used to carry are
    // three rows, one overflow axis, and the dtype the caller already holds.
    public static Fin<Unit> Convert<TFrom, TTo>(TensorOpFamily row, OverflowPolicy policy, ReadOnlySpan<TFrom> source, Span<TTo> destination) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        if (ConvertKernels<TFrom, TTo>.Rows.GetValueOrDefault(policy) is not { } kernel) { return Corner<Unit>(row, policy.Key); }
        try { kernel(source, destination); return Fin.Succ(unit); }
        catch (OverflowException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> ConvertToInteger<TFrom, TTo>(TensorOpFamily row, bool native, ReadOnlySpan<TFrom> source, Span<TTo> destination) where TFrom : IFloatingPoint<TFrom> where TTo : IBinaryInteger<TTo> {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        ConvertKernel<TFrom, TTo> kernel = native ? IntegerConvertKernels<TFrom, TTo>.Native : IntegerConvertKernels<TFrom, TTo>.Managed;
        try { kernel(source, destination); return Fin.Succ(unit); }
        catch (OverflowException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> ToHalf(TensorOpFamily row, ReadOnlySpan<float> source, Span<Half> destination) {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        try { HalfConvertKernels.Narrow(source, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> ToSingle(TensorOpFamily row, ReadOnlySpan<Half> source, Span<float> destination) {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        try { HalfConvertKernels.Widen(source, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Dual<T>(TensorOpFamily row, AngleScaling scaling, ReadOnlySpan<T> x, Span<T> first, Span<T> second) where T : IFloatingPointIeee754<T> {
        if (x.Length != first.Length || x.Length != second.Length) { return Mismatch(row, x.Length, first.Length); }
        if (TensorKernels<T>.Dual.GetValueOrDefault(scaling) is not { } kernel) { return Corner<Unit>(row, scaling.Key); }
        try { kernel(x, first, second); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Polarity<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : INumber<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        try { SignKernels<T>.Sign(x, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> Exponent<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        try { TensorKernels<T>.ExponentOf(x, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    // Predicate masks bind `INumberBase<T>`, so the complex lane reaches the three rows that classify it; the
    // one row the host narrows to `IBinaryNumber<T>` has its own entrypoint at that constraint.
    public static Fin<Unit> Test<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<bool> destination) where T : INumberBase<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (PredicateKernels<T>.Mask.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> TestPow2<T>(ReadOnlySpan<T> x, Span<bool> destination) where T : IBinaryNumber<T> {
        if (x.Length != destination.Length) { return Mismatch(TensorOpFamily.IsPow2, x.Length, destination.Length); }
        try { Pow2Kernels<T>.Mask(x, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    // `PerElement` is the mask entrypoint's own aggregation and has no scalar answer, so it refuses BY NAME here
    // rather than resolving to whichever reducer sits beside it.
    public static Fin<bool> Aggregate<T>(TensorOpFamily row, Aggregation aggregation, ReadOnlySpan<T> x) where T : INumberBase<T> {
        if (PredicateKernels<T>.Aggregate.GetValueOrDefault(row) is not { } pair) { return Miss<bool>(row); }
        Fin<AggregateKernel<T>> kernel = aggregation.Switch<Fin<AggregateKernel<T>>>(
            perElement: _ => Corner<AggregateKernel<T>>(row, Aggregation.PerElement.Key),
            all: _ => Fin.Succ(pair.All),
            any: _ => Fin.Succ(pair.Any));
        return kernel.Bind(reduce => {
            try { return Fin.Succ(reduce(x)); }
            catch (ArgumentException ex) { return Threw<bool>(ex); }
        });
    }

    public static Fin<Unit> ComplexZip(TensorOpFamily row, ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, Span<Complex> destination) {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (ComplexKernels.Binary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> ComplexMap(TensorOpFamily row, ReadOnlySpan<Complex> x, Span<Complex> destination) {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (ComplexKernels.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); }
        catch (ArgumentException ex) { return Threw<Unit>(ex); }
    }

    public static Fin<Unit> ComplexAbs(ReadOnlySpan<Complex> x, Span<double> destination) {
        if (x.Length != destination.Length) { return Mismatch(TensorOpFamily.ComplexAbs, x.Length, destination.Length); }
        ComplexKernels.Magnitude(x, destination);
        return Fin.Succ(unit);
    }

    public static Fin<Unit> QuaternionZip(ReadOnlySpan<Quaternion> x, ReadOnlySpan<Quaternion> y, Span<Quaternion> destination) {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(TensorOpFamily.QuaternionMultiply, x.Length, destination.Length); }
        QuaternionKernels.Multiply(x, y, destination);
        return Fin.Succ(unit);
    }

    public static Fin<Unit> QuaternionMap(TensorOpFamily row, ReadOnlySpan<Quaternion> x, Span<Quaternion> destination) {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (QuaternionKernels.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        kernel(x, destination);
        return Fin.Succ(unit);
    }

    public static Fin<T> Fold<T>(TensorOpFamily row, FoldForm form, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorReason.EmptyOperand.Fail<T>("empty-operand", row.Key); }
        Fin<FoldKernel<T>> resolved = form.Switch<Fin<FoldKernel<T>>>(
            plain: _ => TensorKernels<T>.Fold.GetValueOrDefault(row) is { } k ? Fin.Succ(k) : Miss<FoldKernel<T>>(row),
            extremum: e => TensorKernels<T>.Extremum.GetValueOrDefault((row, e.Metric, e.Nan)) is { } k ? Fin.Succ(k) : Corner<FoldKernel<T>>(row, $"{e.Metric.Key}:{e.Nan.Key}"),
            mapped: m => TensorKernels<T>.Mapped.GetValueOrDefault(m.Element) is { } k ? Fin.Succ(k) : Corner<FoldKernel<T>>(row, m.Element.Key));
        return resolved.Bind(kernel => {
            try { return Fin.Succ(kernel(x)); }
            catch (ArgumentException ex) { return Threw<T>(ex); }
        });
    }

    public static Fin<T> FoldPair<T>(TensorOpFamily row, PairFoldForm form, ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorReason.EmptyOperand.Fail<T>("empty-operand", row.Key); }
        if (x.Length != y.Length) { return TensorReason.ShapeMismatch.Fail<T>("length-mismatch", row.Key, $"{x.Length}!={y.Length}"); }
        Fin<PairFoldKernel<T>> resolved = form.Switch<Fin<PairFoldKernel<T>>>(
            plain: _ => TensorKernels<T>.PairFold.GetValueOrDefault(row) is { } k ? Fin.Succ(k) : Miss<PairFoldKernel<T>>(row),
            combined: c => TensorKernels<T>.Combined.GetValueOrDefault(c.Combine) is { } k ? Fin.Succ(k) : Corner<PairFoldKernel<T>>(row, c.Combine.Key));
        return resolved.Bind(kernel => {
            try { return Fin.Succ(kernel(x, y)); }
            catch (ArgumentException ex) { return Threw<T>(ex); }
        });
    }

    // The index search reads the SAME extremum row through the entrypoint whose RETURN states it; the four `IndexOf*` roster
    // entries are these four table corners.
    public static Fin<int> IndexOf<T>(TensorOpFamily row, ExtremumMetric metric, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorReason.EmptyOperand.Fail<int>("empty-operand", row.Key); }
        if (TensorKernels<T>.Index.GetValueOrDefault((row, metric)) is not { } kernel) { return Corner<int>(row, metric.Key); }
        try { return Fin.Succ(kernel(x)); }
        catch (ArgumentException ex) { return Threw<int>(ex); }
    }

    public static Fin<int> Hamming<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) {
        if (x.Length != y.Length) { return TensorReason.ShapeMismatch.Fail<int>("length-mismatch", TensorOpFamily.HammingDistance.Key, $"{x.Length}!={y.Length}"); }
        return Fin.Succ(TensorPrimitives.HammingDistance(x, y));
    }

    public static Fin<long> HammingBits<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IBinaryInteger<T> {
        if (x.Length != y.Length) { return TensorReason.ShapeMismatch.Fail<long>("length-mismatch", TensorOpFamily.HammingBitDistance.Key, $"{x.Length}!={y.Length}"); }
        return Fin.Succ(TensorPrimitives.HammingBitDistance(x, y));
    }

    // Index-driven structural movement: every index admits against the addressed extent before any element
    // moves; a colliding scatter resolves last-write-wins in index order, so the result is deterministic.
    public static Fin<Unit> Gather<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> indices, Span<T> destination) {
        if (row != TensorOpFamily.Gather) { return Miss<Unit>(row); }
        if (indices.Length != destination.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{indices.Length}!={destination.Length}"); }
        for (int i = 0; i < indices.Length; i++) {
            if (indices[i] < 0 || indices[i] >= values.Length) { return TensorReason.AxisOutOfRange.Fail<Unit>("index-range", row.Key, $"{indices[i]}/{values.Length}"); }
        }
        for (int i = 0; i < indices.Length; i++) { destination[i] = values[indices[i]]; }
        return Fin.Succ(unit);
    }

    public static Fin<Unit> Scatter<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> indices, Span<T> destination) {
        if (row != TensorOpFamily.Scatter) { return Miss<Unit>(row); }
        if (values.Length != indices.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{values.Length}!={indices.Length}"); }
        for (int i = 0; i < indices.Length; i++) {
            if (indices[i] < 0 || indices[i] >= destination.Length) { return TensorReason.AxisOutOfRange.Fail<Unit>("index-range", row.Key, $"{indices[i]}/{destination.Length}"); }
        }
        for (int i = 0; i < indices.Length; i++) { destination[indices[i]] = values[i]; }
        return Fin.Succ(unit);
    }

    // The segmented lane is the REDUCTION row under this entrypoint: a segmented sum is `Sum` with a
    // segment-id span, not a `SegmentSum` roster entry, and `Count` is the one row that exists here alone.
    public static Fin<Unit> Segment<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> segments, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (SegmentReducers<T>.Rows.GetValueOrDefault(row) is not { } reducer) { return Miss<Unit>(row); }
        if (values.Length != segments.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{values.Length}!={segments.Length}"); }
        for (int i = 0; i < segments.Length; i++) {
            if (segments[i] < 0 || segments[i] >= destination.Length) { return TensorReason.AxisOutOfRange.Fail<Unit>("segment-id-range", row.Key, $"{segments[i]}/{destination.Length}"); }
        }
        destination.Fill(reducer.Seed);
        using MemoryOwner<int> countsOwner = MemoryOwner<int>.Allocate(destination.Length, AllocationMode.Clear);
        Span<int> counts = countsOwner.Span;
        for (int i = 0; i < values.Length; i++) {
            int id = segments[i];
            destination[id] = reducer.Combine(destination[id], values[i]);
            counts[id]++;
        }
        for (int group = 0; group < destination.Length; group++) { destination[group] = reducer.Final(destination[group], counts[group]); }
        return Fin.Succ(unit);
    }

    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<bool> filter, in ReadOnlyTensorSpan<T> values) where T : unmanaged {
        if (row != TensorOpFamily.MaskedWrite) { return Miss<Tensor<T>>(row); }
        if (!destination.Lengths.SequenceEqual(filter.Lengths) || !destination.Lengths.SequenceEqual(values.Lengths)) {
            return TensorReason.ShapeMismatch.Fail<Tensor<T>>("mask-shape", row.Key);
        }
        try {
            Tensor.FilteredUpdate(destination.AsTensorSpan(), filter, values);
            return Fin.Succ(destination);
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException) { return Threw<Tensor<T>>(ex); }
    }

    // A global pool is the WINDOW at the axis extent — the two `Global*` roster rows are this one form value.
    public static Fin<Unit> Pool<T>(TensorOpFamily row, Tensor<T> plane, int axis, PoolWindow window, in TensorSpan<T> destination) where T : IFloatingPointIeee754<T> {
        if (PoolReducers<T>.Rows.GetValueOrDefault(row) is not { } reducer) { return Miss<Unit>(row); }
        if (axis < 0 || axis >= plane.Rank) { return TensorReason.AxisOutOfRange.Fail<Unit>("pool-axis", row.Key, $"{axis}/{plane.Rank}"); }
        Tensor<T> dense = plane.ToDenseTensor();
        if (dense.Lengths[axis] <= 0) { return TensorReason.EmptyOperand.Fail<Unit>("pool-empty-axis", row.Key, axis.ToString(CultureInfo.InvariantCulture)); }
        int extent = checked((int)dense.Lengths[axis]);
        (int win, int step) = window.Strided.IfNone((Window: extent, Stride: extent));
        if (win <= 0 || step <= 0 || win > extent) { return TensorReason.AxisOutOfRange.Fail<Unit>("pool-window-out-of-range", row.Key, $"win={win} step={step} extent={extent}"); }
        int outer = checked((int)TensorPrimitives.Product<nint>(dense.Lengths[..axis]));
        int inner = checked((int)TensorPrimitives.Product<nint>(dense.Lengths[(axis + 1)..]));
        int outputs = ((extent - win) / step) + 1;
        long expected = (long)outer * outputs * inner;
        if (destination.FlattenedLength != expected) { return TensorReason.ShapeMismatch.Fail<Unit>("pool-destination-shape", row.Key, $"{destination.FlattenedLength}!={expected}"); }
        ReadOnlySpan<T> src = MemoryMarshal.CreateReadOnlySpan(ref dense.GetPinnableReference(), checked((int)dense.FlattenedLength));
        Span<T> dst = MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)destination.FlattenedLength));
        for (int prefix = 0; prefix < outer; prefix++) {
            for (int output = 0; output < outputs; output++) {
                for (int lane = 0; lane < inner; lane++) {
                    T acc = reducer.Seed;
                    for (int offset = 0; offset < win; offset++) {
                        int index = (((prefix * extent) + (output * step) + offset) * inner) + lane;
                        acc = reducer.Combine(acc, src[index]);
                    }
                    dst[(((prefix * outputs) + output) * inner) + lane] = reducer.Final(acc, win);
                }
            }
        }
        return Fin.Succ(unit);
    }

    // The winning claim is the WHOLE partition gate and the arm READS it: absent, the span kernel runs inline;
    // present, the claim's own partition column sizes the split it was measured at, so a demotion and a
    // promotion both name the row that decided them. Testing the claim for PRESENCE and then sizing the split
    // off an ambient budget published a fast path no measurement covered. A second "plane" route spelling its
    // launch as `For2D(0, rows, 0, 1)` walked one block per row — this same blocked walk at a caller-supplied
    // block size — so it published two shapes for one execution and is deleted along with its 2-D action structs.
    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.Match(
            None: () => Map(row, new UnaryForm.Plain(), x.Span, destination.Span),
            Some: measured => x.Length != destination.Length ? TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{x.Length}!={destination.Length}")
                : TensorKernels<T>.Plain.GetValueOrDefault(row) is { } kernel
                ? PartitionShape.Of(x.Length, budget, measured).Bind(shape =>
                    Op.Of(name: "partition-threw").Catch(() => ParallelHelper.For(0, shape.Blocks, new MapBlock<T>(x, destination, shape.BlockSize, kernel), minimumActionsPerThread: 1)))
                : Miss<Unit>(row));

    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, ReadOnlyMemory<T> y, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.Match(
            None: () => Zip(row, new BinaryForm.Plain(), x.Span, y.Span, destination.Span),
            Some: measured => x.Length != destination.Length || y.Length != destination.Length ? TensorReason.ShapeMismatch.Fail<Unit>("length-mismatch", row.Key, $"{x.Length}/{y.Length}!={destination.Length}")
                : TensorKernels<T>.Binary.GetValueOrDefault(row) is { } kernel
                ? PartitionShape.Of(x.Length, budget, measured).Bind(shape =>
                    Op.Of(name: "partition-threw").Catch(() => ParallelHelper.For(0, shape.Blocks, new ZipBlock<T>(x, y, destination, shape.BlockSize, kernel), minimumActionsPerThread: 1)))
                : Miss<Unit>(row));
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// ONE partition derivation: block size and block count are two readings of the same division, and computing
// them at two call sites let a rounding change land on one and not the other. The cap comes from the CLAIM's
// own partition column where the row carries one and from the lane budget otherwise, so a measured fast path is
// launched at the shape it was measured at.
public readonly record struct PartitionShape(int BlockSize, int Blocks) {
    public static Fin<PartitionShape> Of(int length, CpuBudget budget, BenchmarkRow claim) {
        int cap = Math.Max(1, claim.Partitions.IfNone(budget.PartitionCap));
        if (length <= 0) { return TensorReason.EmptyOperand.Fail<PartitionShape>("partition-empty", claim.Route); }
        int blockSize = Math.Max(1, checked((int)((length + (long)cap - 1) / cap)));
        return Fin.Succ(new PartitionShape(blockSize, ((length + blockSize - 1) / blockSize)));
    }
}

public readonly struct MapBlock<T>(ReadOnlyMemory<T> source, Memory<T> destination, int blockSize, UnaryKernel<T> kernel) : IAction {
    public void Invoke(int block) {
        int start = block * blockSize;
        int length = Math.Min(blockSize, source.Length - start);
        kernel(source.Span.Slice(start, length), destination.Span.Slice(start, length));
    }
}

public readonly struct ZipBlock<T>(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y, Memory<T> destination, int blockSize, BinaryKernel<T> kernel) : IAction {
    public void Invoke(int block) {
        int start = block * blockSize;
        int length = Math.Min(blockSize, x.Length - start);
        kernel(x.Span.Slice(start, length), y.Span.Slice(start, length), destination.Span.Slice(start, length));
    }
}
```

## [03]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the forward-mode Jacobian-vector-product as a TOTAL (non-optional) `Func` column on every bound row; `Directional` the ONE directional-derivative owner carrying each non-elementwise op's reverse VJP and forward JVP — sharing one body wherever the two directions coincide so a `Forward`/`Backward` class pair with copy-pasted SoftMax/MatMul bodies is the deleted illusory-dual form — with the MatMul weight projection selected by `AdjointMode` (`Wᵀ` reverse for `ȳ·Wᵀ`, `W` forward for `ẋ·W`), the symmetric SoftMax Jacobian shared across both directions, and the `Operator` DDG geometry apply selecting the page-owned `OperatorRow.Adjoint` (reverse transpose `Aᵀ·ȳ`) or `OperatorRow.Apply` (forward pushforward `A·ṫ`) — the row table composing the kernel `Rasm.Numerics` `DiscreteCalculus`; `TapeStep` the ONE tape entry — op, forward input, and the row's held-operand payload — under the convention that `Primal` is the forward input on every row and extra recorded data rides `Payload`; `Tape` the `[Union]` closing the tape family over the dense, geometry, and SPILLED forms so one sweep serves all three; `SensitivityLaw` the static `Sweep` folding either direction over that one family, the generalized Gauss-Newton `JᵀJ·v` (reverse-over-forward) surface, AND the hyper-dual scalar leg — the THIRD leg of the ONE `Directional` family beside the geometry tape and the `Symbolic/lowering` symbolic tape: a general smooth scalar objective authored once over the `HyperJet` hyper-dual scalar yields the EXACT gradient (order 1) and the EXACT gradient+Hessian (order 2) in one evaluation through `DDScalar.Variables`/`GetGradient()`/`GetHessian()`, deleting the finite-difference fall its consumers carried (a fourth parallel gradient mechanism is the deleted form); `JacobianColoring` the graph-coloring sparse-Jacobian assembler over the AD tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage; `TapeSpill` the fixed-length step-chunked primal spill over the `Runtime/archive#HDF_ARCHIVE` capsule — the forward sweep pushes step ordinals in index order, the reverse sweep replays per-step hyperslabs with ONE chunk resident, `fileDims` declare the step count (the fixed-step PDE march declares `[steps, width]` outright), and an undeclarable step count segments one create-only session per segment because the unlimited-dimension write faults at encode.
- Entry: `EquivalenceLaw.Prove` admits a positive sample count, captures distribution and kernel boundaries, and applies `ToleranceClass.Bound`, passing the `EquivalencePolicy.Seed` draw key to the one oracle that mints its own fixture. `SensitivityLaw.Sweep` is the ONE fold over the ONE `Tape` family in either direction — dense, geometry, and spilled alike — and `GaussNewton` composes it twice; both keep derivative shape and operator failures on `Fin`, every sweep two-argument because the `Tensor/factor#KERNEL_LOWERING` `ShardDispatch` the MatMul row lowers against rides the geometry it derives from the operand shape; `Gradient` and `Hessian` trap hyper-dual evaluation. `JacobianColoring.Of` admits matrix extents and every sparsity coordinate before `Assemble` recovers colored derivatives into CSR storage. `TapeSpill.Begin` seats the `[steps, width]` extent as one declared slot on the archive session and takes its cursor once, `Push` refuses a mis-sized primal and a step that is not the cursor's own next ordinal typed, and `Replay` reads one step-chunk into a caller-owned span.
- Receipt: equivalence runs materialize as TensorRun receipt evidence at the sink edge, while an explicit copy point rides the `ComputeReceipt.Copy` case its own owner projects (`Tensor/residency#ORT_BRIDGE` `TensorBridge.Crossing`) — this lane performs no host-device tensor copy of its own, its only readback being the `QuerySet` timestamp ticks, so claiming copy points here forked one census across two receipt cases and left `ReceiptFolds.Crossings` reading the one nobody produced; stamped through ONE kernel `MonotonicTimeline` — built once off the host `TimeProvider` at the app root and threaded in — whose `MonotonicBeat` carries the ordinal, the semantic stamp, and the elapsed span together, and keyed by `CorrelationId` (the app-stratum `ClockPolicy` never crosses down here); a raw timestamp/elapsed pair beside a second `IClock` re-minted temporal identity at four sites and threaded two clocks where the branch ruling wants one. The copy points are the three named bridges the `ORT_BRIDGE` capsule owns and the `ByteString` remote-edge projection; the `Span2D` staging-plane view lands with the consumer that stages one, never as an unreached helper on this page.
- Packages: Rasm (project), System.Numerics.Tensors, MathNet.Numerics, HyperJet (the hyper-dual scalar-AD leg — `DDScalar`/`DDScalar1..15`/`DDScalarSpan`, `GetGradient()`/`GetHessian()` MathNet export), Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, PureHDF (`HyperslabSelection`, `NativeDataset` — the replay read alone; every dataset declaration, open, session, filter, and cursor mechanic the `Runtime/archive#HDF_ARCHIVE` capsule owns), NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `Tensor/factor#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product and (for a non-elementwise op) its Jacobian-vector-product to one `Directional` directional body, so the six DDG geometry rows each gain reverse-mode adjoint coverage by one `DifferentiableOp` row routing to `Directional.Operator` under `AdjointMode.Reverse` and forward coverage under `AdjointMode.Forward`, a new geometry operator (remeshing-step, connection-Laplacian) lands as one `Tensor/vocabulary#OPERATION_TABLE` geometry row with one `GeometryAdjoint.Rows` binding, a generalized Gauss-Newton curvature operator is one `SensitivityLaw.GaussNewton` composition over the existing forward+reverse primitives, while the EXACT Hessian-vector product is a distinct second-order capability that grows an `f''` curvature column on `DifferentiableOp` and a flowing-activation tape (never a free composition of first-order primitives), and a large sparse Jacobian is one `JacobianColoring` over the same tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage — never a parallel autodiff surface; a longer-than-resident tape is one `Tape.Spilled` case the same `Sweep` folds — never a second archive surface, an appendable container, or a spill-aware fork of the sweep; a new gradient SOURCE is one leg on the `Directional` family (the hyperdual scalar leg is the proof — one pair of entries, no fourth mechanism); zero new surface.
- Boundary: `TensorOps` binds verified span members directly, routes matrix rows through `KernelLowering`, folds arbitrary-axis pooling over dense outer×axis×inner coordinates, and rejects missing geometry or arity before mutation. `EquivalenceLaw` selects its oracle by `TensorOpKind` — pointwise kinds against the scalar tail, reducing kinds against the reassociated order, matrix against `KernelLowering.ProveGemm`, fixture kinds against the recorded `OperatorRow` transpose identity — and shifts the right-operand fill away from zero on the rows whose gap a zero divisor or base would dominate. `SensitivityLaw` composes total forward and reverse maps, matrix-free `JᵀJ·v`, sparse coloring, and hyper-dual scalar derivatives without parallel gradient owners. A reverse sweep over a spilled tape reads each step's primal through `TapeSpill.Replay` in descending ordinal order, rebuilding the `TapeStep` at the span boundary, so the resident set is one step-chunk plus the flowing adjoint whatever the tape length; the spill composes the archive capsule's cursor, filter, and slug law whole and mints no session mechanics of its own.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The direction a sweep threads. Consumers read it through the generated `Switch` rather than eight `==` tests
// against a two-row roster with no columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdjointMode {
    public static readonly AdjointMode Forward = new("forward");
    public static readonly AdjointMode Reverse = new("reverse");
}

// ONE tape family. `TapeStep` and `GeometryTape` were two record structs a caller had to know in order to pick
// between two `Chain` overloads and two `Pushforward` overloads — four entrypoints for one fold over a mode the
// page already carried as a value. The `Spilled` case is what finally lets a reverse sweep read a tape too long
// to hold: the capsule and the sweep meet here instead of the Boundary claiming a sweep no entrypoint provided.
[Union]
public abstract partial record Tape {
    private Tape() { }

    public sealed record Dense(Seq<TapeStep> Steps) : Tape;
    public sealed record Geometry(Seq<GeometryTape> Steps) : Tape;
    public sealed record Spilled(SpillCursor Cursor) : Tape;
}

// ONE tape convention: `Primal` is the forward INPUT on every row without exception, and a derivative body
// recomputes whatever local value it needs from it. A row recording its OUTPUT (tanh's y, sigmoid's y), its HELD
// operand (multiply's y), or a precomputed local coefficient under that same slot reads identically at the call
// site and mis-differentiates the moment its algebra moves. `Payload` is the row's SECOND recorded operand — the
// held factor, the exponent, the weight block — empty on a row that needs none, so the extra data has its own
// slot instead of colonizing the primal's.
public readonly record struct TapeStep(TensorOpFamily Op, ReadOnlyMemory<float> Primal, ReadOnlyMemory<float> Payload) {
    public static TapeStep Of(TensorOpFamily op, ReadOnlyMemory<float> primal) => new(op, primal, ReadOnlyMemory<float>.Empty);

    public static TapeStep Of(TensorOpFamily op, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> payload) => new(op, primal, payload);
}

public readonly record struct GeometryTape(TensorOpFamily Op, MeshAdjointSnapshot Snapshot);

// The ordinal-indexed reader over a sealed spill container: the op roster is recorded at spill time because a
// primal alone cannot say which derivative to apply to it, and the width is the chunk extent every replay reads.
public readonly record struct SpillCursor(HdfHandle Archive, string Dataset, ImmutableArray<TensorOpFamily> Ops, int Width);

// --- [MODELS] ------------------------------------------------------------------------------
// `SampleCount` sizes the probe, `Seed` keys the draw, and `Lane` separates the streams one policy draws so two
// operands of one proof never share a stream. Every oracle that mints its own fixture draws through the kernel
// `Deterministic.Source(seed, lane)` — an unseeded process RNG made deviation, mass, and cancellation ratio
// unreplayable, so two runs of one row compared nothing while this record declared the opposite.
public sealed record EquivalencePolicy(TensorOpFamily Family, int SampleCount, long Seed) {
    public static EquivalencePolicy For(TensorOpFamily family) => new(family, SampleCount: 256, Seed: 0L);

    public Random Draw(long lane) => Deterministic.Source(Seed, lane);
}

public readonly record struct EquivalenceProof(TensorOpFamily Family, ProofEvidence Evidence, int SampleCount, MonotonicBeat Beat, CorrelationId Correlation) {
    // `ToleranceClass.Bound(length, mass)` alone owns the error envelope and verdict under the cancellation-ratio
    // gate, never a stored relative scalar. The deviation is ABSOLUTE (the envelope is N·ε·Σ|x|), so the evidence
    // carries its own length, mass, and cancellation ratio and an unmeasured run carries none of them.
    public ProofVerdict Verdict => Evidence.Switch(
        measured: m => Family.Tolerance.Verdict(m.Deviation, m.Length, m.Mass, m.CancellationRatio),
        unmeasured: static _ => ProofVerdict.UnprovableUnmeasured);

    public bool Holds => Verdict.Certifies;
}

// Op-agnostic proof evidence funnels every `Prove` arm into `EquivalenceProof`. The non-measurement is a CASE,
// not a sentinel instance: the prior `Unprovable` was `(+inf, 0, 0.0, 1.0)` — a required deviation slot holding
// infinity, a required length holding a zero nobody counted — and a consumer reading those fields could not tell
// them from a measurement that happened to produce them.
[Union]
public abstract partial record ProofEvidence {
    private ProofEvidence() { }

    public sealed record Measured(double Deviation, int Length, double Mass, double CancellationRatio) : ProofEvidence;
    public sealed record Unmeasured(string Reason) : ProofEvidence;
}

// The geometry carries the `ShardDispatch` WHOLE, because the dispatch is the lowering's one argument and an
// adjoint sweep holds no transport of its own: an operand-shape-derived geometry spells `Local`, and a
// farm-planned one is constructed by whoever holds the stub, arriving here as a `Farm` already paired with its
// context. A bare plan riding this slot beside a context threaded down the sweep is the deleted form.
public readonly record struct MatMulGeometry(int Rows, int Inner, int Columns, ShardDispatch Dispatch) {
    public static Fin<MatMulGeometry> Admit(ReadOnlyMemory<float> weights, ReadOnlyMemory<float> direction, AdjointMode mode) {
        int known = direction.Length;
        if (known <= 0 || weights.Length == 0 || weights.Length % known != 0) {
            return TensorReason.ShapeMismatch.Fail<MatMulGeometry>("matmul-adjoint-shape", $"{weights.Length}%{known}");
        }
        int other = weights.Length / known;
        return Fin.Succ(mode.Switch(
            forward: _ => new MatMulGeometry(Rows: 1, Inner: known, Columns: other, new ShardDispatch.Local()),
            reverse: _ => new MatMulGeometry(Rows: 1, Inner: other, Columns: known, new ShardDispatch.Local())));
    }

    // `DenseOfRowMajor` takes the flat sequence directly; the prior `Build.Dense(r, c, (r, c) => span[…])` paid a
    // delegate invocation per CELL to read a contiguous span it already had.
    public Matrix<double> DirectionMatrix(ReadOnlyMemory<float> direction, AdjointMode mode) {
        int width = mode.Switch(forward: _ => Inner, reverse: _ => Columns);
        return Matrix<double>.Build.DenseOfRowMajor(Rows, width, Widened(direction.Span[..(Rows * width)]));
    }

    // The reverse projection is the weight block TRANSPOSED, which `DenseOfRowMajor` states by reading the same
    // row-major buffer at the transposed extents rather than by a per-cell index inversion.
    public Matrix<double> WeightMatrix(ReadOnlyMemory<float> weights, AdjointMode mode) =>
        mode.Switch(
            forward: _ => Matrix<double>.Build.DenseOfRowMajor(Inner, Columns, Widened(weights.Span)),
            reverse: _ => Matrix<double>.Build.DenseOfRowMajor(Inner, Columns, Widened(weights.Span)).Transpose());

    // The widened buffer is CONSUMED by `DenseOfRowMajor`, which copies it into MathNet's own storage, so it is
    // an egress array rather than kernel scratch and a pooled rent would be released before the copy completes.
    static IEnumerable<double> Widened(ReadOnlySpan<float> narrow) {
        double[] wide = GC.AllocateUninitializedArray<double>(narrow.Length);
        TensorPrimitives.ConvertChecked<float, double>(narrow, wide);
        return wide;
    }

    // `ToRowMajorArray` hands the whole buffer at once and `ConvertChecked` narrows it span-wise; the nested
    // per-cell copy this replaces walked a `Matrix<double>` indexer twice per element.
    public MemoryOwner<float> Flatten(Matrix<double> matrix) {
        MemoryOwner<float> flat = MemoryOwner<float>.Allocate(matrix.RowCount * matrix.ColumnCount);
        TensorPrimitives.ConvertChecked<double, float>(matrix.ToRowMajorArray(), flat.Span);
        return flat;
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// One directional-derivative owner for the non-elementwise ops: it carries BOTH the reverse-mode VJP and the
// forward-mode JVP of each op, sharing one body wherever the two directions coincide, so the prior
// Forward/Backward class pair — whose SoftMax and MatMul bodies were byte-identical, the illusory-dual form
// that reads as a rich dual surface yet is copy-paste — is deleted. MatMul picks the weight projection by
// AdjointMode (Wᵀ reverse for ȳ·Wᵀ, W forward for ẋ·W); SoftMax is direction-blind because its Jacobian
// diag(y)−y·yᵀ is symmetric so Jᵀ=J and the VJP equals the JVP; the DEC geometry Operator picks
// OperatorRow.Adjoint (the transpose Aᵀ·ȳ) for reverse and OperatorRow.Apply (a linear operator is its own
// pushforward A·ṫ) for forward over the recorded mesh snapshot. Every arm answers a POOLED rent the sweep owns.
public static class Directional {
    // `Directional` is the synchronous Fin family; the lowering IO (shard-dispatch effects) runs exactly once at
    // this boundary so the directional-derivative rail stays uniform with SoftMax/Sum/Dot/Operator. The geometry
    // hands the lowering its one `ShardDispatch`, so a local adjoint names no transport and no derivative
    // signature widens for an ambient it never reads. The lowered result reads its SOLUTION alone — a local
    // lowering decomposes nothing and carries an empty receipt roster, so an adjoint folding it would publish a
    // factorization no route ran.
    public static Fin<MemoryOwner<float>> MatMul(ReadOnlyMemory<float> weights, ReadOnlySpan<float> direction, AdjointMode mode) {
        ReadOnlyMemory<float> held = direction.ToArray();
        return MatMulGeometry.Admit(weights, held, mode).Bind(geometry =>
            KernelLowering.Lower(TensorOpFamily.MatMul, geometry.DirectionMatrix(held, mode), geometry.WeightMatrix(weights, mode), geometry.Dispatch)
                .Run()
                .Map(outcome => geometry.Flatten(outcome.Solution)));
    }

    public static Fin<MemoryOwner<float>> SoftMax(ReadOnlyMemory<float> primal, ReadOnlySpan<float> direction) {
        if (primal.Length != direction.Length) { return TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-length", TensorOpFamily.SoftMax.Key, $"{primal.Length}!={direction.Length}"); }
        using MemoryOwner<float> yOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        MemoryOwner<float> jacobian = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        Span<float> y = yOwner.Span;
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, direction);
        TensorPrimitives.Subtract(direction, dot, jacobian.Span);
        TensorPrimitives.Multiply<float>(y, jacobian.Span, jacobian.Span);
        return Fin.Succ(jacobian);
    }

    // Reduction directional derivatives are non-diagonal vector→scalar maps. Sum reads the forward INPUT for its
    // extent alone: the reverse VJP broadcasts the scalar cotangent over it (x̄ᵢ = ȳ) and the forward JVP contracts
    // the tangent to the scalar Σẋ. Dot reads the HELD operand the tape carries as payload: the reverse VJP scales
    // it by the scalar cotangent (x̄ᵢ = ȳ·yᵢ) and the forward JVP contracts the tangent with it (ẏ = ẋ·y). These are
    // the dimension-changing reduction adjoints the constitutive strain-energy norm / quadratic-form tapes ride.
    public static Fin<MemoryOwner<float>> Sum(ReadOnlyMemory<float> input, ReadOnlySpan<float> direction, AdjointMode mode) =>
        mode.Switch(
            forward: _ => input.Length != direction.Length
                ? TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-length", TensorOpFamily.Sum.Key, $"{input.Length}!={direction.Length}")
                : Scalar(TensorPrimitives.Sum(direction)),
            reverse: _ => direction.Length != 1
                ? TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-seed", TensorOpFamily.Sum.Key, direction.Length.ToString(CultureInfo.InvariantCulture))
                : Broadcast(input.Length, direction[0]));

    public static Fin<MemoryOwner<float>> Dot(ReadOnlyMemory<float> held, ReadOnlySpan<float> direction, AdjointMode mode) {
        if (held.IsEmpty) { return TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-payload", TensorOpFamily.Dot.Key, "held-operand"); }
        return mode.Switch(
            forward: _ => held.Length != direction.Length
                ? TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-length", TensorOpFamily.Dot.Key, $"{held.Length}!={direction.Length}")
                : Scalar(TensorPrimitives.Dot<float>(direction, held.Span)),
            reverse: _ => direction.Length != 1
                ? TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-seed", TensorOpFamily.Dot.Key, direction.Length.ToString(CultureInfo.InvariantCulture))
                : Scaled(held.Span, direction[0]));
    }

    static Fin<MemoryOwner<float>> Scalar(float value) {
        MemoryOwner<float> rent = MemoryOwner<float>.Allocate(1);
        rent.Span[0] = value;
        return Fin.Succ(rent);
    }

    static Fin<MemoryOwner<float>> Broadcast(int length, float value) {
        MemoryOwner<float> rent = MemoryOwner<float>.Allocate(length);
        rent.Span.Fill(value);
        return Fin.Succ(rent);
    }

    static Fin<MemoryOwner<float>> Scaled(ReadOnlySpan<float> held, float factor) {
        MemoryOwner<float> rent = MemoryOwner<float>.Allocate(held.Length);
        TensorPrimitives.Multiply(held, factor, rent.Span);
        return Fin.Succ(rent);
    }

    // DEC operators use float64 (`Arr<double>` the Rasm.Numerics carrier) while the autodiff tape is
    // float32; the impedance converts through `TensorPrimitives.ConvertChecked` at the seam, never the phantom
    // `Arr.fromSpan`/`Arr.AsSpan` spelling — the verified factory is `Arr.create<T>(ReadOnlySpan<T>)` and the
    // read-back is `.ToArray()`. A row outside the geometry table returns `no-operator-row`.
    public static Fin<MemoryOwner<float>> Operator(GeometryTape step, ReadOnlySpan<float> direction, AdjointMode mode) {
        if (!GeometryAdjoint.Rows.TryGetValue(step.Op, out OperatorRow? row)) { return TensorReason.RowMissing.Fail<MemoryOwner<float>>("no-operator-row", step.Op.Key); }
        Func<MeshAdjointSnapshot, Arr<double>, Fin<Arr<double>>> apply = mode.Switch(reverse: _ => row.Adjoint, forward: _ => row.Apply);
        using MemoryOwner<double> wide = MemoryOwner<double>.Allocate(direction.Length, AllocationMode.Clear);
        TensorPrimitives.ConvertChecked(direction, wide.Span);
        Arr<double> field = Arr.create<double>(wide.Span);
        return Op.Of(name: "operator-adjoint").Catch(() => apply(step.Snapshot, field).Map(static result => {
            MemoryOwner<float> narrow = MemoryOwner<float>.Allocate(result.Count);
            TensorPrimitives.ConvertChecked<double, float>(result.ToArray(), narrow.Span);
            return narrow;
        }));
    }
}

// Fixed-length spill law — probe-proven: `H5Constants.Unlimited` in a `H5Dataset<T>` fileDims faults the
// `BeginWrite` encode itself before any chunk write, so an unknown-length reverse-mode tape has NO lawful
// unbounded container. fileDims declare the step count up front — the fixed-step PDE march declares
// [steps, width] outright — and a sweep that cannot declare one SEGMENTS: one create-only session per segment
// at the sweep's own cadence edge, the `Runtime/archive#HDF_ARCHIVE` sanctioned accumulation form, each segment
// declaring its own step count at `Begin`.
public sealed class TapeSpill : IDisposable {
    readonly ArchiveSession session;
    readonly ChunkCursor<float> cursor;
    readonly int width;

    TapeSpill(ArchiveSession session, ChunkCursor<float> cursor, int width) =>
        (this.session, this.cursor, this.width) = (session, cursor, width);

    // One step-chunk per accepted step: chunks are [1, width], so the resident primal is ONE chunk — the spill's
    // whole memory bound. The extent is SEATED, never re-derived: the step count and width are already declared
    // here, and `Runtime/archive#CHUNK_CURSOR` owns the ordinal↔hyperslab correspondence they address. The
    // capsule folds the slot mint, the graph, the typed attribute stamp, and the writer open into one act, and
    // the cursor comes out ONCE — the forward sweep pushes ordinals in index order through that single cursor,
    // which holds the ordinal no caller re-passes. A refused extent leaves before the session truncates its sink.
    public static Fin<TapeSpill> Begin(int steps, int width, Stream sink, HdfArchivePolicy policy) =>
        ChunkGrid.Seat(fileDims: [(ulong)steps, (ulong)width], chunks: [1u, (uint)width]).ToFin()
            .Map(static grid => new ArchiveSlot<float>("tape", grid))
            .Bind(slot => ArchiveSession.Open(
                    sink, policy, Seq<IArchiveSlot>(slot),
                    Seq(("steps", (ArchiveAttribute)new ArchiveAttribute.Whole(steps)),
                        ("width", (ArchiveAttribute)new ArchiveAttribute.Whole(width))))
                // Release binds to BOTH arms: a session that opened and then failed to hand back its cursor has
                // already truncated the sink, so dropping the handle on the fault arm strands an open writer.
                .Bind(opened => opened.Cursor(slot).Match(
                    Succ: held => Fin.Succ(new TapeSpill(opened, held, width)),
                    Fail: error => { opened.Dispose(); return Fin.Fail<TapeSpill>(error); })));

    // Two admissions, both plain guards on the rail rather than throws raised so an enclosing lift can convert
    // them back: an exception minted to be caught two lines later is exception-style control flow wearing a rail.
    // The width mismatch is a shape refusal; the ordinal mismatch is the ORDER refusal this spill declares — the
    // sweep's own step index must agree with the ordinal the cursor holds, and disagreement is caught at
    // admission ahead of the library's mid-encode `Chunks can only be written once.` fault, which throws after
    // the producing work is already spent. The write passes no ordinal at all, so the chunk address cannot fork
    // and the failure rides the capsule's own `<hdf5-…>` slug.
    public Fin<Unit> Push(int step, ReadOnlyMemory<float> primal) =>
        primal.Length != width
            ? TensorReason.ShapeMismatch.Fail<Unit>("hdf5-tape-width", primal.Length.ToString(CultureInfo.InvariantCulture), width.ToString(CultureInfo.InvariantCulture))
            : step != cursor.Next
            ? TensorReason.PolicyInvalid.Fail<Unit>("hdf5-tape-order", step.ToString(CultureInfo.InvariantCulture), cursor.Next.ToString(CultureInfo.InvariantCulture))
            : cursor.Write(primal.ToArray());

    // Reverse sweep reads the SEALED container per step by hyperslab — [step, 0]/[1, width] — into a caller-owned
    // span, so the backward pass holds one step-chunk resident whatever the tape length; the statement-form catch
    // preserves the exact PureHDF exception because no documented provider refusal earns a local remint here.
    public static Fin<Unit> Replay(HdfHandle handle, string dataset, int step, Span<float> primal) {
        try {
            NativeDataset source = handle.Dataset(dataset);
            HyperslabSelection slab = new(rank: 2, starts: [(ulong)step, 0UL], blocks: [1UL, (ulong)primal.Length]);
            source.Read<float>(handle.Access, primal, fileSelection: slab);
            return Fin.Succ(Unit.Default);
        }
        catch (Exception ex) {
            return Fin.Fail<Unit>(Error.New(ex.Message, ex));
        }
    }

    public void Dispose() => session.Dispose();
}

// Six-row DDG operator table stays Compute-owned; the kernel declares only `DiscreteCalculus` at
// `Rasm/Numerics/spectral#DEC_CARRIERS` and the `MeshAdjointSnapshot` handle. `Apply` is the forward map,
// `Adjoint` the plain transpose the ⟨A·x,y⟩ == ⟨x,Aᵀ·y⟩ proof reads: incidence rows pair by transpose (Gradient ↔
// Divergence over D0, Curl over D1), symmetric weak rows alias `Adjoint` to `Apply`. The kernel `SparseMatrix`
// now publishes the total `Transpose()` (CSparse raw-buffer wrap over the CSR/CSC duality), so the incidence
// adjoints compose it and this lane materializes no triplet round trip.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OperatorRow {
    public static readonly OperatorRow Gradient = new("gradient", D0Apply, D0TransposeApply, FieldDomain.Vertex);
    public static readonly OperatorRow Divergence = new("divergence", D0TransposeApply, D0Apply, FieldDomain.Edge);
    public static readonly OperatorRow Curl = new("curl", D1Apply, D1TransposeApply, FieldDomain.Edge);
    public static readonly OperatorRow CotangentLaplacian = new("cotangent-laplacian", WeakLaplacian, WeakLaplacian, FieldDomain.Vertex);
    public static readonly OperatorRow HeatFlow = new("heat-flow", HeatOperator, HeatOperator, FieldDomain.Vertex);
    public static readonly OperatorRow Spectral = new("spectral", NormalizedLaplacian, NormalizedLaplacian, FieldDomain.Vertex);

    [UseDelegateFromConstructor]
    public partial Fin<Arr<double>> Apply(MeshAdjointSnapshot snapshot, Arr<double> field);

    [UseDelegateFromConstructor]
    public partial Fin<Arr<double>> Adjoint(MeshAdjointSnapshot snapshot, Arr<double> field);

    // Which field a row consumes is ROW DATA: Gradient/CotangentLaplacian/HeatFlow/Spectral act on the vertex
    // field (|V|), Divergence/Curl on the edge field (|E|). A helper testing two rows by identity re-derived a
    // column the roster already had room for, and every later row would have defaulted to the wrong domain.
    public FieldDomain Domain { get; }

    private static Fin<Arr<double>> D0Apply(MeshAdjointSnapshot snapshot, Arr<double> field) => snapshot.Calculus.D0.Multiply(field);

    private static Fin<Arr<double>> D1Apply(MeshAdjointSnapshot snapshot, Arr<double> field) => snapshot.Calculus.D1.Multiply(field);

    private static Fin<Arr<double>> D0TransposeApply(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        snapshot.Calculus.D0.Transpose().Multiply(field);

    private static Fin<Arr<double>> D1TransposeApply(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        snapshot.Calculus.D1.Transpose().Multiply(field);

    // Weak cotangent Laplacian L = D0ᵀ·diag(Star1)·D0 — symmetric by construction, its own transpose.
    private static Fin<Arr<double>> WeakLaplacian(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        D0Apply(snapshot, field).Map(edge => Scaled(edge, snapshot.Calculus.Star1)).Bind(weighted => D0TransposeApply(snapshot, weighted));

    // Unit-step implicit heat operator diag(Star0) + L — SPD, self-adjoint. The fused multiply-add walks the
    // whole vector once through the span primitive, where the element loop it replaces read three arrays by index.
    private static Fin<Arr<double>> HeatOperator(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        WeakLaplacian(snapshot, field).Map(stiff => {
            using MemoryOwner<double> summed = MemoryOwner<double>.Allocate(stiff.Count);
            TensorPrimitives.MultiplyAdd<double>(snapshot.Calculus.Star0.ToArray(), field.ToArray(), stiff.ToArray(), summed.Span);
            return Arr.create<double>(summed.Span);
        });

    // Mass-symmetrized pencil diag(Star0)^-1/2 · L · diag(Star0)^-1/2 — the spectral-basis operator form;
    // Star0 entries are strictly positive under `DiscreteCalculus.IsValid`, so the half-inverse is total.
    private static Fin<Arr<double>> NormalizedLaplacian(MeshAdjointSnapshot snapshot, Arr<double> field) {
        using MemoryOwner<double> halfInverse = MemoryOwner<double>.Allocate(snapshot.Calculus.Star0.Count);
        TensorPrimitives.Sqrt<double>(snapshot.Calculus.Star0.ToArray(), halfInverse.Span);
        TensorPrimitives.Reciprocal<double>(halfInverse.Span, halfInverse.Span);
        Arr<double> weights = Arr.create<double>(halfInverse.Span);
        return WeakLaplacian(snapshot, Scaled(field, weights)).Map(stiff => Scaled(stiff, weights));
    }

    private static Arr<double> Scaled(Arr<double> values, Arr<double> weights) {
        using MemoryOwner<double> scaled = MemoryOwner<double>.Allocate(values.Count);
        TensorPrimitives.Multiply<double>(values.ToArray(), weights.ToArray(), scaled.Span);
        return Arr.create<double>(scaled.Span);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldDomain {
    public static readonly FieldDomain Vertex = new("vertex");
    public static readonly FieldDomain Edge = new("edge");
}

// The one seeded draw for every oracle that mints its own fixture. The unseeded process RNG this replaces made
// the deviation, mass, and cancellation ratio of a proof unreplayable, and the same helper existed twice under
// one name across two pages — one seeded, one not — so a reader could not tell which stream a run drew from.
public static class ProofDraw {
    public static Arr<double> Gaussian(int length, long seed, long lane) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(length);
        Tensor<double> flat = Tensor.CreateFromShape<double>([length]);
        Tensor.FillGaussianNormalDistribution(flat.AsTensorSpan(), Deterministic.Source(seed, lane));
        flat.FlattenTo(values.Span);
        return Arr.create<double>(values.Span);
    }

    public static void Fill(Span<double> destination, long seed, long lane, bool gaussian) {
        Tensor<double> flat = Tensor.CreateFromShape<double>([destination.Length]);
        Random stream = Deterministic.Source(seed, lane);
        ignore(gaussian
            ? Tensor.FillGaussianNormalDistribution(flat.AsTensorSpan(), stream)
            : Tensor.FillUniformDistribution(flat.AsTensorSpan(), stream));
        flat.FlattenTo(destination);
    }
}

public static class GeometryAdjoint {
    public static readonly FrozenDictionary<TensorOpFamily, OperatorRow> Rows = new Dictionary<TensorOpFamily, OperatorRow> {
        [TensorOpFamily.Gradient] = OperatorRow.Gradient,
        [TensorOpFamily.Divergence] = OperatorRow.Divergence,
        [TensorOpFamily.Curl] = OperatorRow.Curl,
        [TensorOpFamily.CotangentLaplacian] = OperatorRow.CotangentLaplacian,
        [TensorOpFamily.HeatFlow] = OperatorRow.HeatFlow,
        [TensorOpFamily.Spectral] = OperatorRow.Spectral,
    }.ToFrozenDictionary();

    // Geometry's span-kernel analogue proves the linear-operator transpose identity
    // ⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩ over a `MeshAdjointSnapshot` FIXTURE (a small canonical mesh, so the accumulation
    // residual stays inside the family band), composing the page-owned `OperatorRow.Apply`/`Adjoint`
    // contract — the self-adjoint rows alias `Adjoint` to `Apply`, the incidence rows route the paired transpose
    // — so the assembled DEC operator pair is certified WITHOUT a per-op reference and WITHOUT re-assembling the
    // operator (the live `DiscreteCalculus` factor the snapshot already holds). Forward `Apply` consumes a random
    // domain vector x on ITS OWN lane; `Adjoint` consumes a random codomain vector y on a second lane, so one
    // seed replays the whole proof and the two operands never share a stream. A fabricated dense Jacobian of the
    // sparse operator, or a proof that never applies the row, is the deleted hollow form.
    public static Fin<ProofEvidence> ProveAdjoint(OperatorRow row, MeshAdjointSnapshot snapshot, long seed) {
        Arr<double> x = ProofDraw.Gaussian(Domain(row, snapshot), seed, lane: 0L);
        return row.Apply(snapshot, x).Bind(ax => {
            Arr<double> y = ProofDraw.Gaussian(ax.Count, seed, lane: 1L);
            return row.Adjoint(snapshot, y).Map(aty => {
                double forward = TensorPrimitives.Dot<double>(ax.ToArray(), y.ToArray());
                double reverse = TensorPrimitives.Dot<double>(x.ToArray(), aty.ToArray());
                return (ProofEvidence)new ProofEvidence.Measured(double.Abs(forward - reverse), x.Count + ax.Count, double.Abs(forward) + double.Abs(reverse), 1.0);
            });
        });
    }

    // The codomain length falls out of `Apply`, so only the domain length is keyed — off the row's own column.
    static int Domain(OperatorRow row, MeshAdjointSnapshot snapshot) =>
        row.Domain == FieldDomain.Edge ? snapshot.EdgeCount : snapshot.VertexCount;
}

// Both directional columns are `(step, direction) → Fin`: a derivative reads the tape step and the direction and
// nothing else, and the MatMul row's `Tensor/factor#KERNEL_LOWERING` GEMM lowers against the `ShardDispatch` its
// own geometry derives. No row here names transport, so no row carries a column it never reads.
public sealed record DifferentiableOp(
    TensorOpFamily Forward,
    bool Diagonal,
    Func<TapeStep, ReadOnlySpan<float>, Fin<MemoryOwner<float>>> Vjp,
    Func<TapeStep, ReadOnlySpan<float>, Fin<MemoryOwner<float>>> Jvp) {
    public static readonly FrozenDictionary<TensorOpFamily, DifferentiableOp> Rows = new Dictionary<TensorOpFamily, DifferentiableOp> {
        // Every diagonal derivative reads the INPUT x and recomputes its own local value: tanh recomputes t=tanh(x)
        // for 1−t², sigmoid recomputes s for s(1−s), exp recomputes eˣ. Recording those outputs instead would save
        // one transcendental per element and cost the tape its one readable convention — the trade this owner refuses.
        [TensorOpFamily.Tanh] = Diag(TensorOpFamily.Tanh, static (x, d) => { TensorPrimitives.Tanh(x, d); TensorPrimitives.Multiply(d, d, d); TensorPrimitives.Subtract(1f, d, d); }),
        [TensorOpFamily.Sigmoid] = Diag(TensorOpFamily.Sigmoid, static (x, d) => { TensorPrimitives.Sigmoid(x, d); Complement(d); }),
        [TensorOpFamily.Exp] = Diag(TensorOpFamily.Exp, static (x, d) => TensorPrimitives.Exp(x, d)),
        [TensorOpFamily.Log] = Diag(TensorOpFamily.Log, static (x, d) => TensorPrimitives.Reciprocal(x, d)),
        [TensorOpFamily.ReLU] = Diag(TensorOpFamily.ReLU, static (x, d) => { TensorPrimitives.Max(x, 0f, d); TensorPrimitives.Sign(d, d); }),
        // Elementwise-ring rows stay diagonal in the flowing operand. Add/Subtract differentiate the addend/minuend
        // (∂=1); Multiply scales by the held factor (∂=y); Divide by the reciprocal denominator (∂=1/y); Pow needs
        // BOTH base and exponent (∂=y·x^(y−1)), which is exactly why the exponent rides `Payload` rather than
        // displacing the input under `Primal`. These supply the constitutive return-map / multi-term stored-energy
        // tape vocabulary the `Solver/constitutive#CONSTITUTIVE` stress-update and contact tapes compose.
        [TensorOpFamily.Add] = Diag(TensorOpFamily.Add, static (_, d) => d.Fill(1f)),
        [TensorOpFamily.Subtract] = Diag(TensorOpFamily.Subtract, static (_, d) => d.Fill(1f)),
        // A held-operand row reads BOTH recorded operands, so an absent or mis-sized payload is a MALFORMED tape
        // and rails `adjoint-payload` — an empty payload silently read as zero returns a derivative of zero, which
        // the sweep then propagates as a legitimate vanishing gradient. `Diag` and `Held` differed only by the
        // PRESENCE of that payload, which the step already carries, so one factory reads an empty payload as the
        // unary case and the pair is one.
        [TensorOpFamily.Multiply] = Diag(TensorOpFamily.Multiply, static (_, y, d) => y.CopyTo(d)),
        [TensorOpFamily.Divide] = Diag(TensorOpFamily.Divide, static (_, y, d) => TensorPrimitives.Reciprocal(y, d)),
        [TensorOpFamily.Pow] = Diag(TensorOpFamily.Pow, static (x, y, d) => { TensorPrimitives.Subtract(y, 1f, d); TensorPrimitives.Pow(x, d, d); TensorPrimitives.Multiply(d, y, d); }),
        // MatMul is genuinely bilinear: the reverse VJP applies Wᵀ, the forward JVP applies W, so the two
        // arms are distinct directional maps, not the deleted copy-paste-identical body. The weight block is the
        // recorded second operand, so it reads from `Payload` like every other held operand.
        [TensorOpFamily.MatMul] = Bilinear(TensorOpFamily.MatMul,
            static (step, seed) => Directional.MatMul(step.Payload, seed, AdjointMode.Reverse),
            static (step, tangent) => Directional.MatMul(step.Payload, tangent, AdjointMode.Forward)),
        // SoftMax's Jacobian is symmetric, so one body serves both directions — Bilinear with the same map
        // names the shared identity instead of duplicating it — and it recomputes y from the recorded input.
        [TensorOpFamily.SoftMax] = Bilinear(TensorOpFamily.SoftMax,
            static (step, seed) => Directional.SoftMax(step.Primal, seed),
            static (step, tangent) => Directional.SoftMax(step.Primal, tangent)),
        // Reduction rows are non-diagonal (vector→scalar), so reverse VJP and forward JVP are genuinely
        // distinct directional maps routed to the one `Directional` owner: Sum reads the input for its extent,
        // Dot the held operand from `Payload`.
        [TensorOpFamily.Sum] = Bilinear(TensorOpFamily.Sum,
            static (step, seed) => Directional.Sum(step.Primal, seed, AdjointMode.Reverse),
            static (step, tangent) => Directional.Sum(step.Primal, tangent, AdjointMode.Forward)),
        [TensorOpFamily.Dot] = Bilinear(TensorOpFamily.Dot,
            static (step, seed) => Directional.Dot(step.Payload, seed, AdjointMode.Reverse),
            static (step, tangent) => Directional.Dot(step.Payload, tangent, AdjointMode.Forward)),
    }.ToFrozenDictionary();

    // Diagonal Jacobians collapse an elementwise op's VJP and JVP into the one `direction .* f'(x)` fold, and the
    // derivative itself is a SPAN kernel: a `Func<float, float>` invoked once per element on the hottest AD path
    // paid a delegate call for work every one of these rows expresses as a `TensorPrimitives` composition.
    static DifferentiableOp Diag(TensorOpFamily forward, SpanDerivative derivative) =>
        Diag(forward, (x, _, d) => derivative(x, d));

    static DifferentiableOp Diag(TensorOpFamily forward, PairedDerivative derivative) =>
        new(forward, Diagonal: true,
            (step, seed) => Pointwise(step, seed, derivative),
            (step, tangent) => Pointwise(step, tangent, derivative));

    // Forward is total over every bound row: each carries a real JVP, so a row either resolves both directions
    // or is absent (`no-adjoint-row`).
    static DifferentiableOp Bilinear(TensorOpFamily forward, Func<TapeStep, ReadOnlySpan<float>, Fin<MemoryOwner<float>>> vjp, Func<TapeStep, ReadOnlySpan<float>, Fin<MemoryOwner<float>>> jvp) =>
        new(forward, Diagonal: false, vjp, jvp);

    static Fin<MemoryOwner<float>> Pointwise(TapeStep step, ReadOnlySpan<float> direction, PairedDerivative derivative) {
        if (step.Primal.Length != direction.Length) { return TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-length", step.Op.Key, $"{step.Primal.Length}!={direction.Length}"); }
        if (!step.Payload.IsEmpty && step.Payload.Length != step.Primal.Length) { return TensorReason.ShapeMismatch.Fail<MemoryOwner<float>>("adjoint-payload", step.Op.Key, $"{step.Payload.Length}!={step.Primal.Length}"); }
        MemoryOwner<float> local = MemoryOwner<float>.Allocate(direction.Length);
        return Op.Of(name: "adjoint-threw").Catch(() => {
            derivative(step.Primal.Span, step.Payload.Span, local.Span);
            TensorPrimitives.Multiply(local.Span, direction, local.Span);
            return Fin.Succ(local);
        });
    }

    static void Complement(Span<float> s) {
        using MemoryOwner<float> one = MemoryOwner<float>.Allocate(s.Length);
        TensorPrimitives.Subtract(1f, s, one.Span);
        TensorPrimitives.Multiply(s, one.Span, s);
    }
}

public delegate void SpanDerivative(ReadOnlySpan<float> primal, Span<float> destination);
public delegate void PairedDerivative(ReadOnlySpan<float> primal, ReadOnlySpan<float> payload, Span<float> destination);

public static class SensitivityLaw {
    // ONE sweep over one tape family in one direction: `Chain` and `Pushforward` were four entrypoints folding
    // over a mode the page already carried as a value, and the caller had to know which tape type it held to
    // pick between them. The fold OWNS every rent it produces and releases each as the flow moves past it —
    // on the refusal arm as well as the success arm — while the caller keeps the seed it brought.
    public static Fin<MemoryOwner<float>> Sweep(Tape tape, AdjointMode mode, ReadOnlySpan<float> seed) {
        MemoryOwner<float> flow = MemoryOwner<float>.Allocate(seed.Length);
        seed.CopyTo(flow.Span);
        return tape.Switch(
            dense: t => Fold(Ordered(t.Steps, mode), flow, (step, carried) => Adjoint(step, mode, carried.Span)),
            geometry: t => Fold(Ordered(t.Steps, mode), flow, (step, carried) => Directional.Operator(step, carried.Span, mode)),
            spilled: t => Fold(Ordered(toSeq(Enumerable.Range(0, t.Cursor.Ops.Length)), mode), flow,
                (ordinal, carried) => Replayed(t.Cursor, ordinal, carried.Span, mode)));
    }

    public static Fin<MemoryOwner<float>> Adjoint(TapeStep step, AdjointMode mode, ReadOnlySpan<float> seed) =>
        DifferentiableOp.Rows.TryGetValue(step.Op, out DifferentiableOp? differentiable)
            ? mode.Switch(reverse: _ => differentiable.Vjp, forward: _ => differentiable.Jvp)(step, seed)
            : TensorReason.RowMissing.Fail<MemoryOwner<float>>("no-adjoint-row", step.Op.Key);

    // A spilled step rebuilds at the span boundary: ONE chunk resident whatever the tape length, which is the
    // memory bound the spill exists for and the sweep the Boundary described but no entrypoint provided.
    static Fin<MemoryOwner<float>> Replayed(SpillCursor cursor, int ordinal, ReadOnlySpan<float> carried, AdjointMode mode) {
        using MemoryOwner<float> primal = MemoryOwner<float>.Allocate(cursor.Width);
        return TapeSpill.Replay(cursor.Archive, cursor.Dataset, ordinal, primal.Span)
            .Bind(_ => Adjoint(TapeStep.Of(cursor.Ops[ordinal], primal.Memory), mode, carried));
    }

    static Seq<A> Ordered<A>(Seq<A> steps, AdjointMode mode) => mode.Switch(reverse: _ => steps.Rev(), forward: _ => steps);

    static Fin<MemoryOwner<float>> Fold<A>(Seq<A> steps, MemoryOwner<float> seed, Func<A, MemoryOwner<float>, Fin<MemoryOwner<float>>> step) =>
        steps.Fold(Fin.Succ(seed), (held, item) => held.Bind(carried => step(item, carried).Match(
            Succ: next => { carried.Dispose(); return Fin.Succ(next); },
            Fail: error => { carried.Dispose(); return Fin.Fail<MemoryOwner<float>>(error); })));

    // Generalized Gauss-Newton matrix-free product JᵀJ·v: run the forward sweep seeded by `vector` to get J·v,
    // then the reverse sweep to get Jᵀ·(J·v) — reverse-over-forward of the ONE first-order tape, no dense
    // matrix materialized. This is the SPD curvature operator Newton-CG / trust-region / Levenberg-Marquardt
    // consume (SPD by construction, so CG never breaks on an indefinite step). It is NOT the exact Hessian:
    // Curvature Σ x̄ₖ·f''(xₖ)·ẋₖ is absent because the first-order tape carries neither the flowing activations
    // nor a second-derivative column — the true Hessian-vector product is a separate second-order capability (an
    // f'' row on `DifferentiableOp` plus a forward-over-reverse sweep), not this fold.
    public static Fin<MemoryOwner<float>> GaussNewton(Tape tape, ReadOnlySpan<float> vector) {
        ReadOnlyMemory<float> held = vector.ToArray();
        return Sweep(tape, AdjointMode.Forward, held.Span).Bind(forwardDot => {
            Fin<MemoryOwner<float>> reverse = Sweep(tape, AdjointMode.Reverse, forwardDot.Span);
            forwardDot.Dispose();
            return reverse;
        });
    }

    // Hyper-dual scalars form the third `Directional` leg beside geometry and symbolic tapes; a smooth scalar
    // objective authored once over the HyperJet scalar yields the exact gradient (order 1) or exact gradient +
    // Hessian (order 2) in ONE evaluation. HyperJet returns PLAIN .NET arrays — `GetGradient()` answers
    // `double[]` and `GetHessian()` answers `double[,]` — so the MathNet carriers are LIFTED at this seam; the
    // prior assignment claimed a MathNet export the package does not make and then called MathNet-only members
    // on the result.
    public static Fin<(double Value, Vector<double> Gradient)> Gradient(Func<DDScalar[], DDScalar> objective, double[] at) =>
        Op.Of(name: "hyperdual-evaluation").Catch(() => {
            DDScalar f = objective(DDScalar.Variables(at, order: 1));
            double[] gradient = f.GetGradient();
            return double.IsFinite(f.Value) && TensorPrimitives.IsFiniteAll<double>(gradient)
                ? Fin.Succ((f.Value, Vector<double>.Build.DenseOfArray(gradient)))
                : TensorReason.NonFinite.Fail<(double, Vector<double>)>("hyperdual-nonfinite", $"n={at.Length}");
        });

    public static Fin<(double Value, Vector<double> Gradient, Matrix<double> Hessian)> Hessian(Func<DDScalar[], DDScalar> objective, double[] at) =>
        Op.Of(name: "hyperdual-evaluation").Catch(() => {
            DDScalar f = objective(DDScalar.Variables(at, order: 2));
            double[] gradient = f.GetGradient();
            double[,] hessian = f.GetHessian();
            return double.IsFinite(f.Value) && TensorPrimitives.IsFiniteAll<double>(gradient) && Finite(hessian)
                ? Fin.Succ((f.Value, Vector<double>.Build.DenseOfArray(gradient), Matrix<double>.Build.DenseOfArray(hessian)))
                : TensorReason.NonFinite.Fail<(double, Vector<double>, Matrix<double>)>("hyperdual-nonfinite", $"n={at.Length}");
        });

    static bool Finite(double[,] block) =>
        TensorPrimitives.IsFiniteAll<double>(MemoryMarshal.CreateReadOnlySpan(ref block[0, 0], block.Length));
}

// Sparse-Jacobian construction by graph coloring: detect the sparsity pattern, color the structurally
// orthogonal columns (greedy distance-1 degree-ordered) over a QuikGraph undirected container, then recover the
// full Jacobian in (#colors) directional-derivative passes instead of (#columns), scattering the compressed
// columns directly into the `SparseFormat` CSR storage the sparse lane owns. `[Equatable]`: a coloring is
// deterministic from its sparsity pattern, so caching it across Newton iterations keys on value identity —
// `Colors` otherwise reference-compares (`Pattern` is a Seq, already structural).
[Equatable]
public sealed partial record JacobianColoring(int Rows, int Columns, Seq<(int Row, int Column)> Pattern, [property: OrderedEquality] ImmutableArray<int> Colors, int ColorCount) {
    // The adjacency is a GRAPH, so it is built by the graph library's own conversion rather than by a
    // `HashSet<int>[]` beside a `GroupBy` beside a nested pair loop; the assignment is a fold over the
    // degree-ordered vertices with no `-1` sentinel standing in for an unassigned colour, and the colour count
    // is the distinct count rather than a maximum that assumed a contiguity nothing stated.
    public static Fin<JacobianColoring> Of(int rows, int columns, Seq<(int Row, int Column)> pattern) {
        if (rows < 0 || columns < 0) { return TensorReason.ShapeMismatch.Fail<JacobianColoring>("jacobian-shape", $"{rows}x{columns}"); }
        if (pattern.Exists(entry => entry.Row < 0 || entry.Row >= rows || entry.Column < 0 || entry.Column >= columns)) {
            return TensorReason.ShapeMismatch.Fail<JacobianColoring>("jacobian-pattern", $"{rows}x{columns}");
        }
        UndirectedGraph<int, SEquatableEdge<int>> graph = pattern
            .GroupBy(static entry => entry.Row)
            .SelectMany(static group => group.Select(static e => e.Column).ToArray() is var cols
                ? cols.SelectMany((a, i) => cols.Skip(i + 1).Select(b => new SEquatableEdge<int>(a, b)))
                : [])
            .ToUndirectedGraph<int, SEquatableEdge<int>>();
        foreach (int column in Enumerable.Range(0, columns)) { graph.AddVertex(column); }
        HashMap<int, int> assigned = toSeq(Enumerable.Range(0, columns).OrderByDescending(graph.AdjacentDegree))
            .Fold(HashMap<int, int>(), (held, column) => held.Add(column, Lowest(toSeq(graph.AdjacentVertices(column)).Choose(held.Find))));
        return Fin.Succ(new JacobianColoring(rows, columns, pattern,
            [.. Enumerable.Range(0, columns).Map(column => assigned.Find(column).IfNone(0))],
            assigned.Values.Distinct().Count()));
    }

    static int Lowest(Seq<int> used) => Enumerable.Range(0, used.Count + 1).First(candidate => !used.Contains(candidate));

    // One seed vector per color probes the structurally-orthogonal column group through the forward sweep (or
    // the reverse one); each color's directional derivative scatters its pattern entries as COO triplets
    // accumulated in color order, then handed ONCE to the `Tensor/factor#SPARSE_SOLVE`-owned
    // `SparseOps.Ingest(Coo)` CSR conversion — never a raw `CoordinateStorage` RowIndices/Values surgery the
    // sparse owner forbids, and never a second ingestion path beside the one factor.md owns. A probe shorter than
    // the row count means the tape produced a derivative of another shape, so it refuses: zero-filling the missing
    // rows publishes structural zeros the pattern declared nonzero, and the solve that reads them cannot tell.
    public Fin<SparseCompressedRowMatrixStorage<double>> Assemble(Func<int, Fin<MemoryOwner<float>>> probeColor) =>
        toSeq(Enumerable.Range(0, ColorCount))
            .Fold(Fin.Succ(Seq<(int Row, int Column, double Value)>()), (acc, seedColor) =>
                acc.Bind(triplets => probeColor(seedColor).Bind(directional => {
                    Fin<Seq<(int, int, double)>> folded = directional.Length < Rows
                        ? TensorReason.ShapeMismatch.Fail<Seq<(int, int, double)>>("jacobian-probe-short", $"color={seedColor}:{directional.Length}<{Rows}")
                        : Fin.Succ(triplets + Pattern
                            .Filter(entry => Colors[entry.Column] == seedColor)
                            .Map(entry => (entry.Row, entry.Column, (double)directional.Span[entry.Row])));
                    directional.Dispose();
                    return folded;
                })))
            .Bind(triplets => SparseOps.Ingest(SparseFormat.Coo, Rows, Columns,
                [.. triplets.Map(static t => t.Row)], [.. triplets.Map(static t => t.Column)], [.. triplets.Map(static t => t.Value)]));
}

public static class EquivalenceLaw {
    // `TensorOpKind.Oracle` is the proof-family selector — the reader the vocabulary owner declares its column
    // for — so the gap kernel comes from the row's KIND through a TOTAL four-arm switch, never from a
    // thirteen-row map keyed on the same roster and never from probing which arity table happens to hold it: a
    // scalar-tail kind runs the candidate VECTOR body against the SAME kernel applied element-by-element (the
    // length-1 tail path the SIMD body must match per the compute length-class law), a reassociated kind runs the
    // candidate against the SAME reduction over the reversed operands (the reassociation-stability the
    // `AccumulationScaled` bound certifies), the lowered kind routes to the `Tensor/factor#KERNEL_LOWERING`
    // GEMM-vs-naive proof (`KernelLowering.ProveGemm`, which OWNS MatMul/Conv admission), and the fixtured kinds
    // carry no data oracle here so the data-only `Prove` reports `Unmeasured` and their gate is `ProveOperator`
    // over a `MeshAdjointSnapshot`. Diffing two unrelated random fills without ever running the kernel is the
    // deleted hollow form.

    // Right-operand fill is a PER-FAMILY PROOF POLICY, not a roster mirror: a divisor, a modulus, or a power base
    // drawn from the plain uniform [0, 1) reaches zero, where the kernel's own gap explodes and the run grades the
    // sample rather than the kernel. These rows shift that draw to [0.5, 1.5) — bounded away from zero on both
    // the value and the derivative — so the measured deviation is the vectorization gap the envelope is
    // calibrated against. The set lives with the law that reads it, never as a column on the vocabulary row.
    static readonly FrozenSet<TensorOpFamily> BoundedAwayFromZero = new[] {
        TensorOpFamily.Divide, TensorOpFamily.Pow, TensorOpFamily.Remainder,
    }.ToFrozenSet();

    // Span-coupled rows carry no scalar-tail identity — softmax over a length-1 slice is the constant 1 — so
    // each references the shift invariance f(x + c) == f(x), a real metamorphic oracle at the same envelope.
    static readonly FrozenSet<TensorOpFamily> Coupled = new[] { TensorOpFamily.SoftMax, TensorOpFamily.LogSoftMax }.ToFrozenSet();

    // ONE clock: the kernel `MonotonicTimeline` is built once off the host `TimeProvider` at the app root and
    // threaded here, so its beat carries the ordinal, the stamp, and the elapsed span together. The raw
    // timestamp/elapsed pair beside a second `IClock` re-minted temporal identity at four sites and threaded two
    // clocks where the branch ruling wants one.
    public static Fin<EquivalenceProof> Prove(MonotonicTimeline timeline, CorrelationId correlation, EquivalencePolicy policy) {
        if (policy.SampleCount <= 0) { return TensorReason.BudgetExhausted.Fail<EquivalenceProof>("equivalence-sample-count", policy.Family.Key, policy.SampleCount.ToString(CultureInfo.InvariantCulture)); }
        MonotonicBeat mark = timeline.Capture();
        return Op.Of(name: "equivalence-threw").Catch(() => {
            using MemoryOwner<double> aOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
            using MemoryOwner<double> bOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
            ProofDraw.Fill(aOwner.Span, policy.Seed, lane: 0L, gaussian: true);
            ProofDraw.Fill(bOwner.Span, policy.Seed, lane: 1L, gaussian: false);
            if (BoundedAwayFromZero.Contains(policy.Family)) { TensorPrimitives.Add<double>(bOwner.Span, 0.5, bOwner.Span); }
            ReadOnlySpan<double> a = aOwner.Span, b = bOwner.Span;
            // An unbounded envelope certifies nothing, so an estimate row never runs a gap at all — it reports
            // `Unmeasured`, which the verdict fold reads through `ToleranceClass` as `unprovable-estimate`.
            ProofEvidence evidence = !policy.Family.Tolerance.Certifiable
                ? new ProofEvidence.Unmeasured("estimate-row")
                : policy.Family.Kind.Oracle.Switch(
                    scalarTail: _ => ScalarTail(policy.Family, a, b),
                    reassociated: _ => Reassociated(policy.Family, a, b),
                    lowered: _ => KernelLowering.ProveGemm(a.Length, policy.Seed),
                    fixtured: _ => new ProofEvidence.Unmeasured($"fixture-kind:{policy.Family.Lowering.Key}"));
            return Fin.Succ(new EquivalenceProof(policy.Family, evidence, policy.SampleCount, timeline.Elapsed(mark), correlation));
        });
    }

    // A kind resolves the FAMILY of oracle; the row still resolves its own arity inside that family, and a row
    // whose element domain this double-typed run cannot instantiate (the integer and complex lanes) reports
    // `Unmeasured` rather than a gap it never measured. Every FORM of a row is probed and the WORST gap is the
    // row's evidence: the tolerance band is a per-row column, so a row certified at one form and unmeasured at
    // another would publish a band its π-scaled or near-unit corner never met — which is exactly what a roster
    // carrying each corner as its own key hid, by proving twelve rows and grading none of them against the row
    // whose envelope they share.
    static ProofEvidence ScalarTail(TensorOpFamily row, ReadOnlySpan<double> a, ReadOnlySpan<double> b) {
        Seq<UnaryKernel<double>> unary = UnaryForms(row);
        Seq<BinaryKernel<double>> binary = BinaryForms(row);
        if (unary.IsEmpty && binary.IsEmpty) { return new ProofEvidence.Unmeasured($"no-double-kernel:{row.Arity.Key}:{row.Lowering.Key}"); }
        double worst = Coupled.Contains(row)
            ? Worst(unary, kernel => CoupledGap(kernel, a))
            : Math.Max(Worst(unary, kernel => UnaryGap(kernel, a)), Worst(binary, kernel => BinaryGap(kernel, a, b)));
        return SpanEvidence(worst, a);
    }

    static ProofEvidence Reassociated(TensorOpFamily row, ReadOnlySpan<double> a, ReadOnlySpan<double> b) {
        Seq<FoldKernel<double>> folds = FoldForms(row);
        Seq<PairFoldKernel<double>> pairs = PairFoldForms(row);
        if (folds.IsEmpty && pairs.IsEmpty) { return new ProofEvidence.Unmeasured($"no-double-kernel:{row.Arity.Key}:{row.Lowering.Key}"); }
        return SpanEvidence(Math.Max(Worst(folds, kernel => FoldGap(kernel, a)), Worst(pairs, kernel => PairFoldGap(kernel, a, b))), a);
    }

    // An empty form set contributes NEGATIVE INFINITY, never a zero: a zero is a measured perfect agreement and
    // an absent arity measured nothing, and folding the two together would certify a row on an oracle that
    // never ran. The caller has already refused the case where both sets are empty.
    static double Worst<TKernel>(Seq<TKernel> kernels, Func<TKernel, double> gap) =>
        kernels.Fold(double.NegativeInfinity, (peak, kernel) => Math.Max(peak, gap(kernel)));

    static Seq<UnaryKernel<double>> UnaryForms(TensorOpFamily row) =>
        toSeq(TensorKernels<double>.Plain.Where(pair => pair.Key == row).Select(static pair => pair.Value))
        + toSeq(TensorKernels<double>.Scaled.Where(pair => pair.Key.Row == row).Select(static pair => pair.Value))
        + toSeq(TensorKernels<double>.Series.Where(pair => pair.Key.Row == row).Select(static pair => pair.Value));

    static Seq<BinaryKernel<double>> BinaryForms(TensorOpFamily row) =>
        toSeq(TensorKernels<double>.Binary.Where(pair => pair.Key == row).Select(static pair => pair.Value))
        + (row == TensorOpFamily.Atan2 ? toSeq(TensorKernels<double>.ScaledBinary.Values) : Seq<BinaryKernel<double>>())
        + (row == TensorOpFamily.Remainder ? toSeq(TensorKernels<double>.Remainders.Values) : Seq<BinaryKernel<double>>());

    static Seq<FoldKernel<double>> FoldForms(TensorOpFamily row) =>
        toSeq(TensorKernels<double>.Fold.Where(pair => pair.Key == row).Select(static pair => pair.Value))
        + toSeq(TensorKernels<double>.Extremum.Where(pair => pair.Key.Row == row).Select(static pair => pair.Value))
        + (row == TensorOpFamily.SumOf ? toSeq(TensorKernels<double>.Mapped.Values) : Seq<FoldKernel<double>>());

    static Seq<PairFoldKernel<double>> PairFoldForms(TensorOpFamily row) =>
        toSeq(TensorKernels<double>.PairFold.Where(pair => pair.Key == row).Select(static pair => pair.Value))
        + (row == TensorOpFamily.ProductOfPairs ? toSeq(TensorKernels<double>.Combined.Values) : Seq<PairFoldKernel<double>>());

    // Geometry and structural rows unreachable by data-only `Prove` certify through the adjoint identity
    // ⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩ over a `MeshAdjointSnapshot` fixture, composing the page-owned
    // `OperatorRow.Apply`/`Adjoint` transpose-pair via `GeometryAdjoint.ProveAdjoint` under the SAME policy seed
    // the data oracles draw from. A row outside the geometry table is `no-adjoint-row` (never silently
    // admitted), and the verdict reads the same `ToleranceClass` envelope the span and matrix proofs read.
    public static Fin<EquivalenceProof> ProveOperator(MonotonicTimeline timeline, CorrelationId correlation, EquivalencePolicy policy, MeshAdjointSnapshot snapshot) {
        MonotonicBeat mark = timeline.Capture();
        return GeometryAdjoint.Rows.TryGetValue(policy.Family, out OperatorRow? row)
            ? GeometryAdjoint.ProveAdjoint(row, snapshot, policy.Seed)
                .Map(evidence => new EquivalenceProof(policy.Family, evidence, policy.SampleCount, timeline.Elapsed(mark), correlation))
            : TensorReason.RowMissing.Fail<EquivalenceProof>("no-adjoint-row", policy.Family.Key);
    }

    // Span deviation is absolute (N·ε·Σ|x|), so evidence carries operand mass Σ|xᵢ| and cancellation ratio.
    // The accumulation length is the probed operand's own extent, never a separately passed count that drifts.
    static ProofEvidence SpanEvidence(double deviation, ReadOnlySpan<double> input) {
        double mass = double.Abs(TensorPrimitives.SumOfMagnitudes<double>(input));
        double ratio = mass > 0.0 ? double.Abs(TensorPrimitives.Sum<double>(input)) / mass : 1.0;
        return new ProofEvidence.Measured(deviation, input.Length, mass, ratio);
    }

    static double CoupledGap(UnaryKernel<double> kernel, ReadOnlySpan<double> input) {
        using MemoryOwner<double> referenceOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        using MemoryOwner<double> shiftedOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        using MemoryOwner<double> movedOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        Span<double> reference = referenceOwner.Span, shifted = shiftedOwner.Span, moved = movedOwner.Span;
        TensorPrimitives.Add(input, 1.0, moved);
        kernel(input, reference);
        kernel(moved, shifted);
        return SpanGap(shifted, reference);
    }

    static double UnaryGap(UnaryKernel<double> kernel, ReadOnlySpan<double> input) {
        using MemoryOwner<double> vectorOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        using MemoryOwner<double> scalarOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        Span<double> vectorized = vectorOwner.Span, scalar = scalarOwner.Span;
        kernel(input, vectorized);
        // The scalar tail MUST be element-by-element: it is the oracle the vector body is graded against.
        for (int i = 0; i < input.Length; i++) { kernel(input.Slice(i, 1), scalar.Slice(i, 1)); }
        return SpanGap(vectorized, scalar);
    }

    static double BinaryGap(BinaryKernel<double> kernel, ReadOnlySpan<double> a, ReadOnlySpan<double> b) {
        using MemoryOwner<double> vectorOwner = MemoryOwner<double>.Allocate(a.Length, AllocationMode.Clear);
        using MemoryOwner<double> scalarOwner = MemoryOwner<double>.Allocate(a.Length, AllocationMode.Clear);
        Span<double> vectorized = vectorOwner.Span, scalar = scalarOwner.Span;
        kernel(a, b, vectorized);
        for (int i = 0; i < a.Length; i++) { kernel(a.Slice(i, 1), b.Slice(i, 1), scalar.Slice(i, 1)); }
        return SpanGap(vectorized, scalar);
    }

    // Reassociation-stability: the vector reduction over the forward order versus the reversed order — the ABSOLUTE
    // gap the `AccumulationScaled` envelope (N·ε·Σ|x|) bounds; an order-invariant reduction (`Min`/`Max`) gaps to 0.
    static double FoldGap(FoldKernel<double> kernel, ReadOnlySpan<double> input) {
        using MemoryOwner<double> reverseOwner = MemoryOwner<double>.Allocate(input.Length, AllocationMode.Clear);
        Span<double> reversed = reverseOwner.Span;
        input.CopyTo(reversed);
        reversed.Reverse();
        return double.Abs(kernel(input) - kernel(reversed));
    }

    // A pair reduction reassociates identically, so its reference reverses BOTH operands together — reversing one
    // alone compares a different mathematical quantity and grades the misalignment as a vectorization gap.
    static double PairFoldGap(PairFoldKernel<double> kernel, ReadOnlySpan<double> a, ReadOnlySpan<double> b) {
        using MemoryOwner<double> leftOwner = MemoryOwner<double>.Allocate(a.Length, AllocationMode.Clear);
        using MemoryOwner<double> rightOwner = MemoryOwner<double>.Allocate(b.Length, AllocationMode.Clear);
        Span<double> left = leftOwner.Span, right = rightOwner.Span;
        a.CopyTo(left);
        b.CopyTo(right);
        left.Reverse();
        right.Reverse();
        return double.Abs(kernel(a, b) - kernel(left, right));
    }

    // Absolute max gap compares the vector body with its element-by-element scalar tail; operand mass
    // and the envelope bound live with the `ProofEvidence` the caller folds, never a relative pre-division here.
    static double SpanGap(Span<double> vectorized, ReadOnlySpan<double> scalar) {
        TensorPrimitives.Subtract<double>(vectorized, scalar, vectorized);
        TensorPrimitives.Abs<double>(vectorized, vectorized);
        return TensorPrimitives.Max<double>(vectorized);
    }
}
```

## [04]-[DEVICE_KERNELS]

- Owner: `DeviceKernels` owns WGSL source rows, per-device typed compilation, and cache retirement; `DeviceKernel` carries compiled module/pipeline/layout handles; `DeviceStep` carries binding slots and launch geometry; `DevicePlan` carries ordered steps; `WgpuDevice` owns native construction, submission, readback, and compute-handle release over AppUi's shared device; `DeviceDispatch` owns admission and receipts.
- Cases: the grounded `DeviceKernels.Wgsl` device op rows — `MatMul` (tiled GEMM over `WgslSource.TiledGemm`), `Conv` at rank 2 (`WgslSource.Im2Col` gather then the TiledGemm pipeline, the two-dispatch convolution mirroring the CPU im2col-then-GEMM), `MaxPool`/`AvgPool` (strided-window reduce) — each a real WGSL compute pipeline compiled and cached on the registry; the rank-1 and rank-3 convolution forms and the `Tensor/factor#SPARSE_ALGEBRA` `Spmv`/`Spmm` rows stay CPU-lowered through factor.md until their device shaders ground (the device path is never a phantom mapping), the elementwise `TensorKernels<T>` rows stay CPU `TensorPrimitives`, and a device elementwise map is a future row, not a fork of the dispatch surface.
- Entry: `Compile(WgpuDevice, TensorOpFamily)` compiles once per `(device identity, op family)` through a thread-safe `Lazy<Fin<DeviceKernel>>`; `Release(WgpuDevice)` retires every cached module, pipeline, and layout for that device, forcing an in-flight compile so its handles release instead of leaking behind the removed entry. `Convolution(WgpuDevice, ConvLaunch, gather, gemm)` builds the two-step rank-2 convolution plan from the registry's own kernels and their declared tiles. `Dispatch(WgpuDevice, DevicePlan, ReadOnlySpan<DeviceBuffer>, TensorDtype, OrtResidency, CorrelationId)` admits every roster index, device-resident buffer, positive workgroup component, and the run's dtype row against the terminal shader's declared element before recording all steps on one encoder and retiring one submission.
- Auto: the CPU/device split lives at the `Runtime/admission#SUBSTRATE_AXIS` spine, never inside `KernelLowering.Lower` — `Lower` stays the CPU `Matrix<double>` terminal with no device consult, and a matmul/conv/pool intent routes to `DeviceDispatch.Dispatch` ONLY when the selected substrate row is `Substrate.DeviceWgpu` AND the `OrtResidency.DeviceResident` gate holds AND a winning `BenchmarkRow` names the device route in its `Route` column — so the split rides substrate selection, residency, and a benchmark claim, never a fork of the `Map`/`Lower` dispatch contract; `Convolution` composes the registry's own `Conv` (Im2Col) and `MatMul` (TiledGemm) kernels, each step's workgroup count derived from that shader's own tile constant — the two-dispatch convolution over one submission; a device result enters rendering only after the owning geometry codec admits it as a Compute `ResidencyPayload`, which `Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyMap.Mint` projects into generated `Render.V1.GeometryResidency` rather than opening a tensor-to-render shortcut; the one shared device descriptor that this row resolves also gates the ONNX Runtime Mac execution-provider residency so a model-lane device tensor and a tensor-lane device kernel resolve the same allocator on the same physical device.
- Receipt: a device dispatch emits the `TensorRun` `ComputeReceipt` carrying the op family, the resolved per-pass GPU nanosecond duration from the `QuerySet` timestamp (never a busy-wait fence), the `device-wgpu` SIMD-width tag and the workgroup count as the partition count, the `DeterminismTag` extended with the device identity, and the `Tensor/memory#ALLOCATION_AXIS` `AllocationClass.DeviceWgpu`; the device GEMM is a new `LinearProvider.DeterminismTag` because a device result is bit-divergent from the managed/native CPU GEMM, so the `SolveDedupKey` folds the device identity exactly as it folds the managed/native provider or a cross-substrate cache hit returns bit-divergent numbers.
- Packages: Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU (the `Wgpu` table for `DevicePoll`/`QueueSubmitForIndex` device-tick readback), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new device operation is one WGSL row; a launch shape is one `DeviceStep.Workgroups` value; a multi-kernel chain is one `DevicePlan` value with roster-indexed intermediates and one submission. Device residency remains `OrtResidency.DeviceResident`, never a parallel tensor type.
- Boundary: `DeviceKernels.Compile` caches typed compile results by device identity and operation, rejects null native handles, releases partial construction, and exposes device-scoped cache retirement. `DevicePlan` carries ordered kernels, binding slots, and workgroups. `DeviceDispatch.Dispatch` proves non-empty bindings, device residency, binding indexes, workgroup arithmetic, terminal output byte alignment, and one common submission. `WgpuDevice.RecordAndSubmit` admits plan and binding counts against fixed caps before any stack staging, then owns one encoder, timestamped passes, one submit, blocking poll, one mapped readback, and deterministic transient-handle release; shared `Device` and `Queue` remain AppUi-owned. Every submission records inside one `ErrorFilter.Validation` scope drained through `DevicePopErrorScope` before its duration becomes a receipt, so a driver-rejected dispatch rails `device-validation` instead of returning timing for work that never ran. Device-limit negotiation reads `DeviceGetLimits` at admission. Model lane and device lane share one physical device but never one allocation: `OrtValue` imports a foreign pointer and exports none, and the WebGPU binding mints through `DeviceCreateBuffer` with no import entrypoint and no buffer-import `NativeSType` chain tag, so every ORT↔WGPU handoff crosses as the host round trip under an `AllocationClass.EdgeCopy` grant — a zero-copy device-to-device claim between the two is unrepresentable at the admitted binding, and asserting one fabricates a residency neither surface reports.

```csharp signature
// --- [CONSTANTS] ---------------------------------------------------------------------------
// WGSL rows compile one pipeline per grounded op: tiled GEMM, im2col projection, and strided-window pooling.
public static class WgslSource {
    // The tile is ONE constant the shader source INTERPOLATES: a C# literal beside a `@workgroup_size` literal
    // beside a WGSL `const` spelled the same 16 three times in one file, and a launch computed against a tile the
    // shader no longer used under-dispatches silently, leaving the output tail at whatever the device buffer
    // already held. The rows are `static readonly` rather than `const` because a compile-time constant cannot
    // carry an interpolation hole, and the source is read at pipeline build, never at type load.
    public const uint GemmTile = 16;
    public const uint GatherTile = 8;
    public const uint WindowLane = 64;

    public static readonly string TiledGemm = $$"""
        @group(0) @binding(0) var<storage, read> a : array<f32>;
        @group(0) @binding(1) var<storage, read> b : array<f32>;
        @group(0) @binding(2) var<storage, read_write> c : array<f32>;
        @group(0) @binding(3) var<uniform> dims : vec3<u32>;
        const TILE : u32 = {{GemmTile}}u;
        var<workgroup> a_tile : array<f32, {{GemmTile * GemmTile}}>;
        var<workgroup> b_tile : array<f32, {{GemmTile * GemmTile}}>;
        @compute @workgroup_size({{GemmTile}}, {{GemmTile}}, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>,
                @builtin(local_invocation_id) lid : vec3<u32>) {
            let m = dims.x;
            let k = dims.y;
            let n = dims.z;
            var acc : f32 = 0.0;
            let steps = (k + TILE - 1u) / TILE;
            for (var s : u32 = 0u; s < steps; s = s + 1u) {
                let a_col = s * TILE + lid.x;
                let b_row = s * TILE + lid.y;
                a_tile[lid.y * TILE + lid.x] = select(0.0, a[gid.y * k + a_col], gid.y < m && a_col < k);
                b_tile[lid.y * TILE + lid.x] = select(0.0, b[b_row * n + gid.x], b_row < k && gid.x < n);
                workgroupBarrier();
                for (var e : u32 = 0u; e < TILE; e = e + 1u) {
                    acc = acc + a_tile[lid.y * TILE + e] * b_tile[e * TILE + lid.x];
                }
                workgroupBarrier();
            }
            if (gid.y < m && gid.x < n) {
                c[gid.y * n + gid.x] = acc;
            }
        }
        """;

    public static readonly string Im2Col = $$"""
        struct ConvGeom {
            channels : u32, in_h : u32, in_w : u32, kernel_h : u32,
            kernel_w : u32, stride_h : u32, stride_w : u32, pad_h : u32,
            pad_w : u32, dil_h : u32, dil_w : u32, out_w : u32,
        };
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> patch : array<f32>;
        @group(0) @binding(2) var<uniform> g : ConvGeom;
        @compute @workgroup_size({{GatherTile}}, {{GatherTile}}, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            let out_h = (g.in_h + 2u * g.pad_h - g.dil_h * (g.kernel_h - 1u) - 1u) / g.stride_h + 1u;
            let position = gid.x;
            let channel = gid.y;
            if (position >= out_h * g.out_w || channel >= g.channels) {
                return;
            }
            let oy = position / g.out_w;
            let ox = position % g.out_w;
            let kernel_vol = g.kernel_h * g.kernel_w;
            let patch_width = g.channels * kernel_vol;
            for (var ky : u32 = 0u; ky < g.kernel_h; ky = ky + 1u) {
                for (var kx : u32 = 0u; kx < g.kernel_w; kx = kx + 1u) {
                    let iy = i32(oy * g.stride_h + ky * g.dil_h) - i32(g.pad_h);
                    let ix = i32(ox * g.stride_w + kx * g.dil_w) - i32(g.pad_w);
                    let col = channel * kernel_vol + ky * g.kernel_w + kx;
                    var value : f32 = 0.0;
                    if (iy >= 0 && iy < i32(g.in_h) && ix >= 0 && ix < i32(g.in_w)) {
                        value = input[channel * g.in_h * g.in_w + u32(iy) * g.in_w + u32(ix)];
                    }
                    patch[position * patch_width + col] = value;
                }
            }
        }
        """;

    public static readonly string StridedWindowMax = $$"""
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> output : array<f32>;
        @group(0) @binding(2) var<uniform> p : vec4<u32>;
        @compute @workgroup_size({{WindowLane}}, 1, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            let window = p.x;
            let stride = p.y;
            let extent = p.z;
            let slices = p.w;
            let outputs = (extent - window) / stride + 1u;
            let idx = gid.x;
            if (idx >= outputs * slices) {
                return;
            }
            let base = (idx / outputs) * extent + (idx % outputs) * stride;
            var acc : f32 = input[base];
            for (var w : u32 = 1u; w < window; w = w + 1u) {
                acc = max(acc, input[base + w]);
            }
            output[idx] = acc;
        }
        """;

    public static readonly string StridedWindowAvg = $$"""
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> output : array<f32>;
        @group(0) @binding(2) var<uniform> p : vec4<u32>;
        @compute @workgroup_size({{WindowLane}}, 1, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            let window = p.x;
            let stride = p.y;
            let extent = p.z;
            let slices = p.w;
            let outputs = (extent - window) / stride + 1u;
            let idx = gid.x;
            if (idx >= outputs * slices) {
                return;
            }
            let base = (idx / outputs) * extent + (idx % outputs) * stride;
            var acc : f32 = 0.0;
            for (var w : u32 = 0u; w < window; w = w + 1u) {
                acc = acc + input[base + w];
            }
            output[idx] = acc / f32(window);
        }
        """;
}

// --- [MODELS] ------------------------------------------------------------------------------
// Roster entry is a wgpu `Buffer*` the device owns, and the ORT↔WGPU crossing is DIRECTIONAL by member truth:
// `OrtValue` only IMPORTS foreign memory — `CreateTensorValueWithData` takes a trailing `(nint dataPtr, long
// sizeBytes)` under an `OrtMemoryInfo` naming the foreign device — and hands no pointer OUT, its egress being the
// managed views `GetTensorMutableRawData()`/`GetTensorDataAsSpan<T>()` sized by `GetTensorSizeInBytes()`. The
// WebGPU binding mints buffers through `DeviceCreateBuffer` alone: neither `WebGPU` nor the `Wgpu` native table
// carries an import, adopt, or shared-handle entrypoint, and none of the ten `NativeSType` `next`-chain extras
// tags a buffer import, so a wgpu allocation cannot enter ORT and a device-resident `OrtValue` cannot bind here.
// ONE crossing survives — the host round trip this page already owns: `GetTensorMutableRawData()` into
// `QueueWriteBuffer` inbound, `CommandEncoderCopyBufferToBuffer` into the `MapRead` staging buffer then
// `BufferGetMappedRange` outbound — each an `AllocationClass.EdgeCopy` grant carrying its copy reason, never a
// zero-copy claim the two surfaces cannot honour.
public readonly record struct DeviceBuffer(nuint Handle, long ByteLength, OrtResidency Residency);

// `Element` is the shader's OWN declared storage element, carried out of the WGSL row so admission compares the
// run's dtype against what the pipeline actually indexes rather than trusting a caller's word.
public sealed record DeviceKernel(TensorOpFamily Op, TensorDtype Element, nuint Pipeline, nuint BindGroupLayout, nuint ShaderModule);

public readonly record struct DeviceStep(DeviceKernel Kernel, ImmutableArray<int> Bindings, (uint X, uint Y, uint Z) Workgroups);

// Launch geometry of the two-dispatch convolution: `Positions` is the output pixel count (out_h·out_w) the Im2Col
// kernel walks on x with `Channels` on y, and the patch matrix it writes is [Positions × PatchWidth] against a
// [PatchWidth × Filters] filter block, so the GEMM walks filters on x and positions on y at its own tile.
public readonly record struct ConvLaunch(uint Positions, uint Channels, uint PatchWidth, uint Filters);

// One command submission carries roster-indexed steps, device-resident intermediates, timestamps, and one readback.
public sealed record DevicePlan(Seq<DeviceStep> Steps) {
    public static DevicePlan Of(DeviceKernel kernel, ImmutableArray<int> bindings, (uint X, uint Y, uint Z) workgroups) =>
        new(Seq(new DeviceStep(kernel, bindings, workgroups)));

    public static DevicePlan Of(params ReadOnlySpan<DeviceStep> steps) => new(toSeq(steps.ToArray()));
}

// --- [SERVICES] ----------------------------------------------------------------------------
// Compute-lane capsule composes AppUi's shared `Device`/`Queue`; compute-only handles release through native calls.
public sealed unsafe class WgpuDevice(WebGPU api, Wgpu ext, Device* device, Queue* queue, string identity) {
    // Stackalloc admission caps: a plan is a short kernel chain and a WGSL row binds a handful of buffers, so
    // caller-controlled counts never size an unbounded stack frame.
    const int MaxPlanSteps = 64;
    const int MaxStepBindings = 8;

    static readonly PfnBufferMapCallback MapNoop = new(static (BufferMapAsyncStatus status, void* data) => { });

    // WGPU reports shader, binding, and dispatch validation faults asynchronously, so an unmanaged sink closing over
    // nothing writes the drained message into a [ThreadStatic] slot the owning submission reads back on its own thread.
    [ThreadStatic] static string? scopeFault;

    static readonly PfnErrorCallback ScopeSink = new(static (ErrorType type, byte* message, void* data) => {
        if (type != ErrorType.NoError) { scopeFault = $"{type}:{Marshal.PtrToStringUTF8((nint)message)}"; }
    });

    // Pop drains asynchronously, so one blocking poll settles the scope before the slot is read.
    Fin<Unit> DrainScope(string key) {
        api.DevicePopErrorScope(device, ScopeSink, null);
        ext.DevicePoll(device, true, null);
        return scopeFault is { } captured ? TensorReason.NativeRejected.Fail<Unit>("device-validation", key, captured) : Fin.Succ(unit);
    }

    public string Identity => identity;

    internal Fin<DeviceKernel> Build(TensorOpFamily op, TensorDtype element, string wgsl) {
        nint code = 0;
        nint entry = 0;
        ShaderModule* module = null;
        ComputePipeline* pipeline = null;
        BindGroupLayout* layout = null;
        bool transferred = false;
        try {
            code = Marshal.StringToCoTaskMemUTF8(wgsl);
            entry = Marshal.StringToCoTaskMemUTF8("main");
            ShaderModuleWGSLDescriptor wgslDesc = new() { Chain = new ChainedStruct { SType = SType.ShaderModuleWgslDescriptor }, Code = (byte*)code };
            ShaderModuleDescriptor moduleDesc = new() { NextInChain = (ChainedStruct*)&wgslDesc };
            module = api.DeviceCreateShaderModule(device, &moduleDesc);
            if (module == null) { return TensorReason.NativeRejected.Fail<DeviceKernel>("device-shader", op.Key); }
            ComputePipelineDescriptor pipelineDesc = new() { Layout = null, Compute = new ProgrammableStageDescriptor { Module = module, EntryPoint = (byte*)entry } };
            pipeline = api.DeviceCreateComputePipeline(device, &pipelineDesc);
            if (pipeline == null) { return TensorReason.NativeRejected.Fail<DeviceKernel>("device-pipeline", op.Key); }
            layout = api.ComputePipelineGetBindGroupLayout(pipeline, 0);
            if (layout == null) { return TensorReason.ShapeMismatch.Fail<DeviceKernel>("device-layout", op.Key); }
            transferred = true;
            return Fin.Succ(new DeviceKernel(op, element, (nuint)pipeline, (nuint)layout, (nuint)module));
        }
        catch (Exception ex) {
            return Fin.Fail<DeviceKernel>(Error.New(ex.Message, ex));
        }
        finally {
            if (!transferred) {
                if (layout != null) { api.BindGroupLayoutRelease(layout); }
                if (pipeline != null) { api.ComputePipelineRelease(pipeline); }
                if (module != null) { api.ShaderModuleRelease(module); }
            }
            if (code != 0) { Marshal.FreeCoTaskMem(code); }
            if (entry != 0) { Marshal.FreeCoTaskMem(entry); }
        }
    }

    internal void Release(DeviceKernel kernel) {
        api.BindGroupLayoutRelease((BindGroupLayout*)kernel.BindGroupLayout);
        api.ComputePipelineRelease((ComputePipeline*)kernel.Pipeline);
        api.ShaderModuleRelease((ShaderModule*)kernel.ShaderModule);
    }

    internal Fin<Duration> RecordAndSubmit(DevicePlan plan, ReadOnlySpan<DeviceBuffer> roster) {
        int steps = plan.Steps.Count;
        if (steps == 0) { return TensorReason.EmptyOperand.Fail<Duration>("empty-plan", "device"); }
        if (steps > MaxPlanSteps) { return TensorReason.BudgetExhausted.Fail<Duration>("device-plan-bounds", $"steps={steps}>{MaxPlanSteps}"); }
        int maxBindings = plan.Steps.Fold(0, static (peak, step) => Math.Max(peak, step.Bindings.Length));
        if (maxBindings > MaxStepBindings) { return TensorReason.BudgetExhausted.Fail<Duration>("device-plan-bounds", $"bindings={maxBindings}>{MaxStepBindings}"); }
        Span<BindGroupEntry> entries = stackalloc BindGroupEntry[maxBindings];
        Span<nint> groups = stackalloc nint[steps];
        groups.Clear();
        QuerySet* timestamps = null;
        Buffer* resolve = null;
        Buffer* readback = null;
        CommandEncoder* encoder = null;
        CommandBuffer* commands = null;
        bool mapped = false;
        bool scoped = false;
        try {
            scopeFault = null;
            api.DevicePushErrorScope(device, ErrorFilter.Validation);
            scoped = true;
            QuerySetDescriptor querySetDesc = new() { Type = QueryType.Timestamp, Count = checked((uint)(2 * steps)) };
            timestamps = api.DeviceCreateQuerySet(device, &querySetDesc);
            BufferDescriptor resolveDesc = new() { Size = checked((ulong)(2 * steps * sizeof(ulong))), Usage = BufferUsage.QueryResolve | BufferUsage.CopySrc };
            BufferDescriptor readbackDesc = new() { Size = checked((ulong)(2 * steps * sizeof(ulong))), Usage = BufferUsage.MapRead | BufferUsage.CopyDst };
            resolve = api.DeviceCreateBuffer(device, &resolveDesc);
            readback = api.DeviceCreateBuffer(device, &readbackDesc);
            encoder = api.DeviceCreateCommandEncoder(device, null);
            if (timestamps == null || resolve == null || readback == null || encoder == null) { return TensorReason.BudgetExhausted.Fail<Duration>("device-resource", "timestamp-readback"); }
            int index = 0;
            foreach (DeviceStep step in plan.Steps) {
                for (int i = 0; i < step.Bindings.Length; i++) {
                    DeviceBuffer buffer = roster[step.Bindings[i]];
                    entries[i] = new BindGroupEntry { Binding = (uint)i, Buffer = (Buffer*)buffer.Handle, Offset = 0, Size = (ulong)buffer.ByteLength };
                }
                BindGroup* group;
                fixed (BindGroupEntry* entryRoot = entries) {
                    BindGroupDescriptor groupDesc = new() { Layout = (BindGroupLayout*)step.Kernel.BindGroupLayout, EntryCount = (nuint)step.Bindings.Length, Entries = entryRoot };
                    group = api.DeviceCreateBindGroup(device, &groupDesc);
                }
                if (group == null) { return TensorReason.BudgetExhausted.Fail<Duration>("device-resource", step.Kernel.Op.Key); }
                groups[index] = (nint)group;
                ComputePassTimestampWrites timestampWrites = new() { QuerySet = timestamps, BeginningOfPassWriteIndex = (uint)(2 * index), EndOfPassWriteIndex = (uint)(2 * index + 1) };
                ComputePassDescriptor passDesc = new() { TimestampWrites = &timestampWrites };
                ComputePassEncoder* pass = api.CommandEncoderBeginComputePass(encoder, &passDesc);
                if (pass == null) { return TensorReason.BudgetExhausted.Fail<Duration>("device-resource", "compute-pass"); }
                try {
                    api.ComputePassEncoderSetPipeline(pass, (ComputePipeline*)step.Kernel.Pipeline);
                    api.ComputePassEncoderSetBindGroup(pass, 0, group, 0, null);
                    api.ComputePassEncoderDispatchWorkgroups(pass, step.Workgroups.X, step.Workgroups.Y, step.Workgroups.Z);
                    api.ComputePassEncoderEnd(pass);
                }
                finally { api.ComputePassEncoderRelease(pass); }
                index++;
            }
            ulong byteCount = checked((ulong)(2 * steps * sizeof(ulong)));
            api.CommandEncoderResolveQuerySet(encoder, timestamps, 0, (uint)(2 * steps), resolve, 0);
            api.CommandEncoderCopyBufferToBuffer(encoder, resolve, 0, readback, 0, byteCount);
            commands = api.CommandEncoderFinish(encoder, null);
            if (commands == null) { return TensorReason.BudgetExhausted.Fail<Duration>("device-resource", "command-buffer"); }
            ulong submission = ext.QueueSubmitForIndex(queue, 1, &commands);
            WrappedSubmissionIndex wait = new() { Queue = queue, SubmissionIndex = submission };
            ext.DevicePoll(device, true, &wait);
            api.BufferMapAsync(readback, MapMode.Read, 0, (nuint)byteCount, MapNoop, null);
            ext.DevicePoll(device, true, null);
            if (api.BufferGetMapState(readback) != BufferMapState.Mapped) { return TensorReason.NativeRejected.Fail<Duration>("device-map", "readback"); }
            mapped = true;
            ulong* ticks = (ulong*)api.BufferGetMappedRange(readback, 0, (nuint)byteCount);
            if (ticks == null) { return TensorReason.NativeRejected.Fail<Duration>("device-map", "range"); }
            Duration elapsed = Duration.FromNanoseconds(checked((long)(ticks[(2 * steps) - 1] - ticks[0])));
            // Captured validation faults mean the readback ticks describe a dispatch the driver rejected, so the
            // scope drains BEFORE the duration becomes a receipt — and the flag clears only once that drain
            // RETURNS, so a throwing drain still meets the finally's retry rather than skipping the cleanup on the
            // strength of an intent that never completed.
            Fin<Duration> drained = DrainScope(plan.Steps[0].Kernel.Op.Key).Map(_ => elapsed);
            scoped = false;
            return drained;
        }
        catch (Exception ex) {
            return Fin.Fail<Duration>(Error.New(ex.Message, ex));
        }
        finally {
            if (scoped) { _ = DrainScope(plan.Steps[0].Kernel.Op.Key); }
            if (mapped) { api.BufferUnmap(readback); }
            foreach (nint group in groups) { if (group != 0) { api.BindGroupRelease((BindGroup*)group); } }
            if (timestamps != null) { api.QuerySetRelease(timestamps); }
            if (resolve != null) { api.BufferRelease(resolve); }
            if (readback != null) { api.BufferRelease(readback); }
            if (commands != null) { api.CommandBufferRelease(commands); }
            if (encoder != null) { api.CommandEncoderRelease(encoder); }
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DeviceKernels {
    // Device rows cache one grounded WGSL pipeline per `(device identity, op)`; CPU tables stay closed. Every
    // grounded row declares `array<f32>` storage, so the element column carries that fact into admission instead
    // of a dtype literal at the receipt, and a widened shader is one row edit here.
    static readonly FrozenDictionary<TensorOpFamily, (string Source, TensorDtype Element)> Wgsl = new Dictionary<TensorOpFamily, (string, TensorDtype)> {
        [TensorOpFamily.MatMul] = (WgslSource.TiledGemm, TensorDtype.Float32),
        [TensorOpFamily.Conv] = (WgslSource.Im2Col, TensorDtype.Float32),
        [TensorOpFamily.MaxPool] = (WgslSource.StridedWindowMax, TensorDtype.Float32),
        [TensorOpFamily.AvgPool] = (WgslSource.StridedWindowAvg, TensorDtype.Float32),
    }.ToFrozenDictionary();

    static readonly ConcurrentDictionary<(string Device, TensorOpFamily Op), Lazy<Fin<DeviceKernel>>> Compiled = new();

    public static Fin<DeviceKernel> Compile(WgpuDevice device, TensorOpFamily row) =>
        Wgsl.TryGetValue(row, out (string Source, TensorDtype Element) wgsl)
            ? Compiled.GetOrAdd((device.Identity, row), key => new Lazy<Fin<DeviceKernel>>(
                () => device.Build(key.Op, wgsl.Element, wgsl.Source), LazyThreadSafetyMode.ExecutionAndPublication)).Value
            : TensorReason.RowMissing.Fail<DeviceKernel>("device-kernel-miss", row.Key);

    // The rank-2 convolution is the canonical multi-step plan and it homes with the registry, because building
    // it resolves two
    // compiled rows: the Im2Col kernel gathers the patch matrix into a roster-indexed device-resident intermediate
    // and the TiledGemm kernel multiplies it by the filter block — one encoder, one submission, no host round trip
    // between the steps. The intermediate is a roster index like any other binding, so the caller allocates it once
    // beside the operands and both steps address it by index. Each launch derives from its OWN shader's tile.
    public static Fin<DevicePlan> Convolution(WgpuDevice device, ConvLaunch launch, ImmutableArray<int> gather, ImmutableArray<int> gemm) =>
        launch.Positions == 0 || launch.Channels == 0 || launch.PatchWidth == 0 || launch.Filters == 0
            ? TensorReason.NativeRejected.Fail<DevicePlan>("device-conv-launch", $"{launch.Positions}x{launch.Channels}x{launch.PatchWidth}x{launch.Filters}")
            : Compile(device, TensorOpFamily.Conv).Bind(im2col =>
              Compile(device, TensorOpFamily.MatMul).Map(tiledGemm =>
                  DevicePlan.Of(
                      new DeviceStep(im2col, gather, (Tiles(launch.Positions, WgslSource.GatherTile), Tiles(launch.Channels, WgslSource.GatherTile), 1)),
                      new DeviceStep(tiledGemm, gemm, (Tiles(launch.Filters, WgslSource.GemmTile), Tiles(launch.Positions, WgslSource.GemmTile), 1)))));

    static uint Tiles(uint extent, uint tile) => (extent + tile - 1) / tile;

    // Retirement FORCES each removed entry rather than skipping an uncreated one: a `Lazy` removed while its
    // compile is in flight still publishes a module, pipeline, and layout to the thread that forced it, and once
    // the entry has left the cache nothing else holds a handle to release — the drain blocks on the compile,
    // then releases exactly what it produced.
    public static void Release(WgpuDevice device) {
        foreach (var pair in Compiled.Where(pair => pair.Key.Device == device.Identity).ToArray()) {
            if (Compiled.TryRemove(pair.Key, out Lazy<Fin<DeviceKernel>>? compiled) && compiled.Value.Case is DeviceKernel kernel) {
                device.Release(kernel);
            }
        }
    }
}

public static class DeviceDispatch {
    // Span operands cross no receipt lambda, so element and workgroup facts precompute; singular dispatch is a
    // one-step plan through DevicePlan.Of — one entrypoint owns both modalities.
    public static Fin<ComputeReceipt.TensorRun> Dispatch(WgpuDevice device, DevicePlan plan, ReadOnlySpan<DeviceBuffer> roster, TensorDtype row, OrtResidency residency, CorrelationId correlation) {
        if (plan.Steps.IsEmpty) { return TensorReason.EmptyOperand.Fail<ComputeReceipt.TensorRun>("empty-plan", "device"); }
        if (!residency.Device) { return TensorReason.ResidencyMismatch.Fail<ComputeReceipt.TensorRun>("device-residency-required", plan.Steps[0].Kernel.Op.Key); }
        long workgroups = 0;
        foreach (DeviceStep step in plan.Steps) {
            if (step.Bindings.IsDefaultOrEmpty) { return TensorReason.EmptyOperand.Fail<ComputeReceipt.TensorRun>("device-bindings-empty", step.Kernel.Op.Key); }
            if (step.Workgroups.X == 0 || step.Workgroups.Y == 0 || step.Workgroups.Z == 0) {
                return TensorReason.BudgetExhausted.Fail<ComputeReceipt.TensorRun>("device-workgroups", step.Kernel.Op.Key, $"{step.Workgroups.X}x{step.Workgroups.Y}x{step.Workgroups.Z}");
            }
            foreach (int binding in step.Bindings) {
                if (binding < 0 || binding >= roster.Length) { return TensorReason.AxisOutOfRange.Fail<ComputeReceipt.TensorRun>("device-binding-range", step.Kernel.Op.Key, $"{binding}/{roster.Length}"); }
                if (roster[binding].ByteLength <= 0 || !roster[binding].Residency.Device) {
                    return TensorReason.ResidencyMismatch.Fail<ComputeReceipt.TensorRun>("device-buffer-residency", step.Kernel.Op.Key, binding.ToString());
                }
            }
            // The launch bound is a COMPARISON at the accumulation site, never a checked multiply raising an
            // exception a bound test two lines later would have caught anyway: three `uint` components multiply
            // into a `long` without overflow, so the only reachable breach is the running sum, and testing it
            // here retires both the `checked` block and the trailing re-test that duplicated its verdict.
            long launch = (long)step.Workgroups.X * step.Workgroups.Y * step.Workgroups.Z;
            if (launch > int.MaxValue - workgroups) { return TensorReason.ExtentOverflow.Fail<ComputeReceipt.TensorRun>("device-workgroup-overflow", step.Kernel.Op.Key); }
            workgroups += launch;
        }
        DeviceStep terminalStep = plan.Steps[plan.Steps.Count - 1];
        TensorOpFamily terminal = terminalStep.Kernel.Op;
        // Element width comes from the RUN's dtype row and must equal the shader's own declared element: a
        // pipeline indexing `array<f32>` reads a float64 buffer as twice as many wrong values, and a receipt
        // naming a dtype the pipeline never indexed is worse evidence than the refusal.
        if (row != terminalStep.Kernel.Element) { return TensorReason.DtypeMismatch.Fail<ComputeReceipt.TensorRun>("device-dtype", terminal.Key, $"{row.Key}!={terminalStep.Kernel.Element.Key}"); }
        if (row.Width.Case is not int width || width <= 0) { return TensorReason.DtypeMismatch.Fail<ComputeReceipt.TensorRun>("device-dtype-width", row.Key); }
        long outputBytes = roster[terminalStep.Bindings[^1]].ByteLength;
        if (outputBytes % width != 0) { return TensorReason.ByteSpanMisaligned.Fail<ComputeReceipt.TensorRun>("device-output-alignment", terminal.Key, $"{outputBytes}%{width}"); }
        long elements = outputBytes / width;
        int partitions = (int)workgroups;
        Fin<Duration> run = Op.Of(name: "device-submit").Catch(() => device.RecordAndSubmit(plan, roster));
        return run
            .Map(elapsed => new ComputeReceipt.TensorRun(terminal, row.Key, elements, SimdWidth: Substrate.DeviceWgpu.Key, Partitions: partitions) {
                Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.DeviceWgpu, AllocationClass.DeviceWgpu, elapsed),
                DeterminismTag = $"{Substrate.DeviceWgpu.Key}:{device.Identity}",
            });
    }

}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
