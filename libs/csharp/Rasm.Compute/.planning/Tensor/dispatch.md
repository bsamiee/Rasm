# [COMPUTE_DISPATCH]

CPU tensor dispatch binds each `TensorOpFamily` row to one arity kernel, claim-gated partition route, equivalence proof, sensitivity law, and device lowering. `TensorOps`, `EquivalenceLaw`, `SensitivityLaw`, and `DeviceDispatch` own those execution algebras; vocabulary, residency, numeric lowering, runtime receipts, and solver consumers compose their typed seams.

## [01]-[INDEX]

- [01]-[KERNEL_DISPATCH]: arity kernel-delegate tables; one `TensorOps` dispatch surface.
- [02]-[EQUIVALENCE_INTEROP]: equivalence proofs of the vector lane against its scalar-path reference; matmul route; dual-mode (forward+reverse) differentiable adjoint; generalized Gauss-Newton `JᵀJ` product; sparse-Jacobian coloring; copy-point law.
- [03]-[DEVICE_KERNELS]: WGSL compute-pipeline registry lowering matrix/structural/sparse op-family rows to `ONE_WGPU_DEVICE` workgroup dispatch behind the residency gate and a winning benchmark claim.

## [02]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `TensorOps.Map` and its arity-shaped siblings validate common extents before selecting one closed row. `Segment` validates every segment id before grouped reduction, `Gather`/`Scatter` prove every index against the addressed extent before element movement, `Pool` validates rank, axis, window, stride, and exact destination shape before arbitrary-axis reduction, and `Partition` selects inline, block, or plane execution from the admitted claim. Every span-shaped method catches at its statement seam because ref-struct operands never cross an effect closure.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a new activation is one `Activations<T>` composed fold plus one `Unary` row, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, a new predicate-aggregate is one `AggregateReducers<T>` entry on the shared `Aggregate` fold, a new segmented reduction is one `SegmentReducers<T>` seed/combine/finalize row on the shared `Segment` fold, an index-driven structural op is one row-gated span arity beside `Gather`/`Scatter`, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/aggregate/segment/complex method; a matrix kernel is one lowering row read from `Tensor/factor#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path reading `CpuBudget.PartitionCap` and the claim's route column, never a new owner; zero new surface.
- Boundary: arity tables bind only verified `TensorPrimitives` members at compatible generic constraints. Author folds cover activation, complex, and quaternion operations that have no direct member; matrix operations lower through the numeric lane; pooling reduces arbitrary axes through tuple policy rows; predicates, reductions, masks, segments, index gathers/scatters, partitions, and conversions retain their distinct destination and admission shapes. Frozen indexes use ordinal comparison, and ref-struct kernels remain statement-shaped.

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

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Effects {
    // One exception-to-rail bridge captures every throwing kernel, native call, or
    // partition body lands as a typed fault — never an escaping exception under an already-announced Succ.
    public static Fin<Unit> ToFin(Action kernel) =>
        Try.lift(fun(kernel)).Run().MapFail(static error => TensorFault.Symbol("kernel-threw", error.Message));
}

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
    public static readonly FrozenDictionary<TensorOpFamily, (T Seed, Func<T, T, T> Combine, Func<T, int, T> Final)> Rows =
        new Dictionary<TensorOpFamily, (T, Func<T, T, T>, Func<T, int, T>)> {
        [TensorOpFamily.MaxPool] = (T.NegativeInfinity, static (acc, value) => T.Max(acc, value), static (acc, _) => acc),
        [TensorOpFamily.GlobalMaxPool] = (T.NegativeInfinity, static (acc, value) => T.Max(acc, value), static (acc, _) => acc),
        [TensorOpFamily.AvgPool] = (T.Zero, static (acc, value) => acc + value, static (acc, count) => acc / T.CreateChecked(count)),
        [TensorOpFamily.GlobalAvgPool] = (T.Zero, static (acc, value) => acc + value, static (acc, count) => acc / T.CreateChecked(count)),
    }.ToFrozenDictionary();
}

public static class SegmentReducers<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, (T Seed, Func<T, T, T> Combine, Func<T, int, T> Final)> Rows =
        new Dictionary<TensorOpFamily, (T, Func<T, T, T>, Func<T, int, T>)> {
        [TensorOpFamily.SegmentSum] = new(T.Zero, static (acc, value) => acc + value, static (acc, _) => acc),
        [TensorOpFamily.SegmentMean] = new(T.Zero, static (acc, value) => acc + value, static (acc, count) => count > 0 ? acc / T.CreateChecked(count) : T.NaN),
        [TensorOpFamily.SegmentMax] = new(T.NegativeInfinity, static (acc, value) => T.Max(acc, value), static (acc, _) => acc),
        [TensorOpFamily.SegmentMin] = new(T.PositiveInfinity, static (acc, value) => T.Min(acc, value), static (acc, _) => acc),
        [TensorOpFamily.SegmentCount] = new(T.Zero, static (acc, _) => acc + T.One, static (acc, _) => acc),
    }.ToFrozenDictionary();
}

public static class AggregateReducers<T> where T : IBinaryNumber<T> {
    public static readonly FrozenDictionary<TensorOpFamily, AggregateKernel<T>> Rows = new Dictionary<TensorOpFamily, AggregateKernel<T>> {
        [TensorOpFamily.IsNaNAll] = TensorPrimitives.IsNaNAll, [TensorOpFamily.IsNaNAny] = TensorPrimitives.IsNaNAny,
        [TensorOpFamily.IsFiniteAll] = TensorPrimitives.IsFiniteAll, [TensorOpFamily.IsFiniteAny] = TensorPrimitives.IsFiniteAny,
        [TensorOpFamily.IsInfinityAll] = TensorPrimitives.IsInfinityAll, [TensorOpFamily.IsInfinityAny] = TensorPrimitives.IsInfinityAny,
        [TensorOpFamily.IsPositiveInfinityAll] = TensorPrimitives.IsPositiveInfinityAll, [TensorOpFamily.IsPositiveInfinityAny] = TensorPrimitives.IsPositiveInfinityAny,
        [TensorOpFamily.IsNegativeInfinityAll] = TensorPrimitives.IsNegativeInfinityAll, [TensorOpFamily.IsNegativeInfinityAny] = TensorPrimitives.IsNegativeInfinityAny,
        [TensorOpFamily.IsIntegerAll] = TensorPrimitives.IsIntegerAll, [TensorOpFamily.IsIntegerAny] = TensorPrimitives.IsIntegerAny,
        [TensorOpFamily.IsEvenIntegerAll] = TensorPrimitives.IsEvenIntegerAll, [TensorOpFamily.IsEvenIntegerAny] = TensorPrimitives.IsEvenIntegerAny,
        [TensorOpFamily.IsOddIntegerAll] = TensorPrimitives.IsOddIntegerAll, [TensorOpFamily.IsOddIntegerAny] = TensorPrimitives.IsOddIntegerAny,
        [TensorOpFamily.IsNegativeAll] = TensorPrimitives.IsNegativeAll, [TensorOpFamily.IsNegativeAny] = TensorPrimitives.IsNegativeAny,
        [TensorOpFamily.IsPositiveAll] = TensorPrimitives.IsPositiveAll, [TensorOpFamily.IsPositiveAny] = TensorPrimitives.IsPositiveAny,
        [TensorOpFamily.IsZeroAll] = TensorPrimitives.IsZeroAll, [TensorOpFamily.IsZeroAny] = TensorPrimitives.IsZeroAny,
        [TensorOpFamily.IsNormalAll] = TensorPrimitives.IsNormalAll, [TensorOpFamily.IsNormalAny] = TensorPrimitives.IsNormalAny,
        [TensorOpFamily.IsSubnormalAll] = TensorPrimitives.IsSubnormalAll, [TensorOpFamily.IsSubnormalAny] = TensorPrimitives.IsSubnormalAny,
        [TensorOpFamily.IsPow2All] = TensorPrimitives.IsPow2All, [TensorOpFamily.IsPow2Any] = TensorPrimitives.IsPow2Any,
        [TensorOpFamily.IsCanonicalAll] = TensorPrimitives.IsCanonicalAll, [TensorOpFamily.IsCanonicalAny] = TensorPrimitives.IsCanonicalAny,
        [TensorOpFamily.IsComplexNumberAll] = TensorPrimitives.IsComplexNumberAll, [TensorOpFamily.IsComplexNumberAny] = TensorPrimitives.IsComplexNumberAny,
        [TensorOpFamily.IsImaginaryNumberAll] = TensorPrimitives.IsImaginaryNumberAll, [TensorOpFamily.IsImaginaryNumberAny] = TensorPrimitives.IsImaginaryNumberAny,
        [TensorOpFamily.IsRealNumberAll] = TensorPrimitives.IsRealNumberAll, [TensorOpFamily.IsRealNumberAny] = TensorPrimitives.IsRealNumberAny,
    }.ToFrozenDictionary();
}

public static class MaskKernels<T> where T : IBinaryNumber<T> {
    public static readonly FrozenDictionary<TensorOpFamily, MaskKernel<T>> Rows = new Dictionary<TensorOpFamily, MaskKernel<T>> {
        [TensorOpFamily.IsNaN] = TensorPrimitives.IsNaN, [TensorOpFamily.IsFinite] = TensorPrimitives.IsFinite,
        [TensorOpFamily.IsInfinity] = TensorPrimitives.IsInfinity, [TensorOpFamily.IsPositiveInfinity] = TensorPrimitives.IsPositiveInfinity, [TensorOpFamily.IsNegativeInfinity] = TensorPrimitives.IsNegativeInfinity,
        [TensorOpFamily.IsInteger] = TensorPrimitives.IsInteger, [TensorOpFamily.IsEvenInteger] = TensorPrimitives.IsEvenInteger, [TensorOpFamily.IsOddInteger] = TensorPrimitives.IsOddInteger,
        [TensorOpFamily.IsNegative] = TensorPrimitives.IsNegative, [TensorOpFamily.IsPositive] = TensorPrimitives.IsPositive, [TensorOpFamily.IsZero] = TensorPrimitives.IsZero,
        [TensorOpFamily.IsNormal] = TensorPrimitives.IsNormal, [TensorOpFamily.IsSubnormal] = TensorPrimitives.IsSubnormal, [TensorOpFamily.IsPow2] = TensorPrimitives.IsPow2,
        [TensorOpFamily.IsCanonical] = TensorPrimitives.IsCanonical, [TensorOpFamily.IsComplexNumber] = TensorPrimitives.IsComplexNumber,
        [TensorOpFamily.IsImaginaryNumber] = TensorPrimitives.IsImaginaryNumber, [TensorOpFamily.IsRealNumber] = TensorPrimitives.IsRealNumber,
    }.ToFrozenDictionary();
}

public static class TensorKernels<T> where T : IFloatingPointIeee754<T> {
    public static readonly FrozenDictionary<TensorOpFamily, UnaryKernel<T>> Unary = new Dictionary<TensorOpFamily, UnaryKernel<T>> {
        [TensorOpFamily.Negate] = TensorPrimitives.Negate, [TensorOpFamily.Abs] = TensorPrimitives.Abs, [TensorOpFamily.Round] = TensorPrimitives.Round,
        [TensorOpFamily.Floor] = TensorPrimitives.Floor, [TensorOpFamily.Ceiling] = TensorPrimitives.Ceiling, [TensorOpFamily.Truncate] = TensorPrimitives.Truncate,
        [TensorOpFamily.BitIncrement] = TensorPrimitives.BitIncrement, [TensorOpFamily.BitDecrement] = TensorPrimitives.BitDecrement,
        [TensorOpFamily.Exp] = TensorPrimitives.Exp, [TensorOpFamily.Exp2] = TensorPrimitives.Exp2, [TensorOpFamily.Exp10] = TensorPrimitives.Exp10,
        [TensorOpFamily.ExpM1] = TensorPrimitives.ExpM1, [TensorOpFamily.Exp2M1] = TensorPrimitives.Exp2M1, [TensorOpFamily.Exp10M1] = TensorPrimitives.Exp10M1,
        [TensorOpFamily.Log] = TensorPrimitives.Log, [TensorOpFamily.Log2] = TensorPrimitives.Log2, [TensorOpFamily.Log10] = TensorPrimitives.Log10,
        [TensorOpFamily.LogP1] = TensorPrimitives.LogP1, [TensorOpFamily.Log2P1] = TensorPrimitives.Log2P1, [TensorOpFamily.Log10P1] = TensorPrimitives.Log10P1,
        [TensorOpFamily.Sin] = TensorPrimitives.Sin, [TensorOpFamily.Cos] = TensorPrimitives.Cos, [TensorOpFamily.Tan] = TensorPrimitives.Tan,
        [TensorOpFamily.SinPi] = TensorPrimitives.SinPi, [TensorOpFamily.CosPi] = TensorPrimitives.CosPi, [TensorOpFamily.TanPi] = TensorPrimitives.TanPi,
        [TensorOpFamily.Asin] = TensorPrimitives.Asin, [TensorOpFamily.Acos] = TensorPrimitives.Acos, [TensorOpFamily.Atan] = TensorPrimitives.Atan,
        [TensorOpFamily.AsinPi] = TensorPrimitives.AsinPi, [TensorOpFamily.AcosPi] = TensorPrimitives.AcosPi, [TensorOpFamily.AtanPi] = TensorPrimitives.AtanPi,
        [TensorOpFamily.Sinh] = TensorPrimitives.Sinh, [TensorOpFamily.Cosh] = TensorPrimitives.Cosh, [TensorOpFamily.Tanh] = TensorPrimitives.Tanh,
        [TensorOpFamily.Asinh] = TensorPrimitives.Asinh, [TensorOpFamily.Acosh] = TensorPrimitives.Acosh, [TensorOpFamily.Atanh] = TensorPrimitives.Atanh,
        [TensorOpFamily.Sigmoid] = TensorPrimitives.Sigmoid, [TensorOpFamily.SoftMax] = TensorPrimitives.SoftMax,
        [TensorOpFamily.Sqrt] = TensorPrimitives.Sqrt, [TensorOpFamily.Cbrt] = TensorPrimitives.Cbrt,
        [TensorOpFamily.DegreesToRadians] = TensorPrimitives.DegreesToRadians, [TensorOpFamily.RadiansToDegrees] = TensorPrimitives.RadiansToDegrees,
        [TensorOpFamily.Reciprocal] = TensorPrimitives.Reciprocal, [TensorOpFamily.ReciprocalSqrt] = TensorPrimitives.ReciprocalSqrt,
        [TensorOpFamily.ReciprocalEstimate] = TensorPrimitives.ReciprocalEstimate, [TensorOpFamily.ReciprocalSqrtEstimate] = TensorPrimitives.ReciprocalSqrtEstimate,
        [TensorOpFamily.ReLU] = Activations<T>.ReLU, [TensorOpFamily.Gelu] = Activations<T>.Gelu, [TensorOpFamily.SiLU] = Activations<T>.SiLU,
        [TensorOpFamily.LogSoftMax] = Activations<T>.LogSoftMax,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, BinaryKernel<T>> Binary = new Dictionary<TensorOpFamily, BinaryKernel<T>> {
        [TensorOpFamily.Add] = TensorPrimitives.Add, [TensorOpFamily.Subtract] = TensorPrimitives.Subtract, [TensorOpFamily.Multiply] = TensorPrimitives.Multiply,
        [TensorOpFamily.Divide] = TensorPrimitives.Divide, [TensorOpFamily.Pow] = TensorPrimitives.Pow, [TensorOpFamily.Atan2] = TensorPrimitives.Atan2, [TensorOpFamily.Atan2Pi] = TensorPrimitives.Atan2Pi,
        [TensorOpFamily.CopySign] = TensorPrimitives.CopySign, [TensorOpFamily.Hypot] = TensorPrimitives.Hypot,
        [TensorOpFamily.Ieee754Remainder] = TensorPrimitives.Ieee754Remainder, [TensorOpFamily.Remainder] = TensorPrimitives.Remainder,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, TernaryKernel<T>> Ternary = new Dictionary<TensorOpFamily, TernaryKernel<T>> {
        [TensorOpFamily.MultiplyAdd] = TensorPrimitives.MultiplyAdd, [TensorOpFamily.FusedMultiplyAdd] = TensorPrimitives.FusedMultiplyAdd, [TensorOpFamily.MultiplyAddEstimate] = TensorPrimitives.MultiplyAddEstimate,
        [TensorOpFamily.AddMultiply] = TensorPrimitives.AddMultiply, [TensorOpFamily.Clamp] = TensorPrimitives.Clamp, [TensorOpFamily.Lerp] = TensorPrimitives.Lerp,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, DualKernel<T>> Dual = new Dictionary<TensorOpFamily, DualKernel<T>> {
        [TensorOpFamily.SinCos] = TensorPrimitives.SinCos, [TensorOpFamily.SinCosPi] = TensorPrimitives.SinCosPi,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, SignKernel<T>> Sign = new Dictionary<TensorOpFamily, SignKernel<T>> {
        [TensorOpFamily.Sign] = TensorPrimitives.Sign, [TensorOpFamily.ILogB] = TensorPrimitives.ILogB,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, FoldKernel<T>> Fold = new Dictionary<TensorOpFamily, FoldKernel<T>> {
        [TensorOpFamily.Sum] = TensorPrimitives.Sum, [TensorOpFamily.Product] = TensorPrimitives.Product, [TensorOpFamily.Min] = TensorPrimitives.Min,
        [TensorOpFamily.Max] = TensorPrimitives.Max, [TensorOpFamily.MinNumber] = TensorPrimitives.MinNumber, [TensorOpFamily.MaxNumber] = TensorPrimitives.MaxNumber,
        [TensorOpFamily.MinMagnitude] = TensorPrimitives.MinMagnitude, [TensorOpFamily.MaxMagnitude] = TensorPrimitives.MaxMagnitude,
        [TensorOpFamily.MinMagnitudeNumber] = TensorPrimitives.MinMagnitudeNumber, [TensorOpFamily.MaxMagnitudeNumber] = TensorPrimitives.MaxMagnitudeNumber,
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
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<float, Half>> Narrow = new Dictionary<TensorOpFamily, ConvertKernel<float, Half>> {
        [TensorOpFamily.ConvertToHalf] = TensorPrimitives.ConvertToHalf,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, ConvertKernel<Half, float>> Widen = new Dictionary<TensorOpFamily, ConvertKernel<Half, float>> {
        [TensorOpFamily.ConvertToSingle] = TensorPrimitives.ConvertToSingle,
    }.ToFrozenDictionary();
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
        [TensorOpFamily.ComplexLog] = Projection.Elementwise<Complex>(static x => Complex.Log(x)),
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<TensorOpFamily, MagnitudeKernel> Magnitude = new Dictionary<TensorOpFamily, MagnitudeKernel> {
        [TensorOpFamily.ComplexAbs] = Projection.Magnitude(static x => x.Magnitude),
    }.ToFrozenDictionary();
}

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
    // A span operand is a ref struct and cannot cross a closure, so every span entry is the named kernel
    // statement seam: admit the common extent, resolve the table row, invoke in place, convert a throw once.
    private static Fin<Unit> Mismatch(TensorOpFamily row, int expected, int actual) =>
        TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{expected}!={actual}");
    private static Fin<A> Threw<A>(TensorOpFamily row, Exception ex) => TensorFault.Fail<A>("kernel-threw", row.Key, ex.Message);

    public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Zip<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Binary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Fuse<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> z, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length || y.Length != destination.Length || z.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Ternary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, z, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Bits<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IBinaryInteger<T> {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (IntegerKernels<T>.Binary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Shift<T>(TensorOpFamily row, ReadOnlySpan<T> x, int shiftCount, Span<T> destination) where T : IBinaryInteger<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (IntegerKernels<T>.Shift.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, shiftCount, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Convert<TFrom, TTo>(TensorOpFamily row, ReadOnlySpan<TFrom> source, Span<TTo> destination) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo> {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        if (ConvertKernels<TFrom, TTo>.Rows.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(source, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Dual<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> first, Span<T> second) where T : IFloatingPointIeee754<T> {
        if (x.Length != first.Length || x.Length != second.Length) { return Mismatch(row, x.Length, first.Length); }
        if (TensorKernels<T>.Dual.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, first, second); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Polarity<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : IFloatingPointIeee754<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (TensorKernels<T>.Sign.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Test<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<bool> destination) where T : IBinaryNumber<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (MaskKernels<T>.Rows.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Population<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination) where T : IBinaryInteger<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (IntegerKernels<T>.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> ToHalf(TensorOpFamily row, ReadOnlySpan<float> source, Span<Half> destination) {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        if (HalfConvertKernels.Narrow.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(source, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> ToSingle(TensorOpFamily row, ReadOnlySpan<Half> source, Span<float> destination) {
        if (source.Length != destination.Length) { return Mismatch(row, source.Length, destination.Length); }
        if (HalfConvertKernels.Widen.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(source, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> ComplexZip(TensorOpFamily row, ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, Span<Complex> destination) {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (ComplexKernels.Binary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> ComplexMap(TensorOpFamily row, ReadOnlySpan<Complex> x, Span<Complex> destination) {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (ComplexKernels.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> ComplexAbs(TensorOpFamily row, ReadOnlySpan<Complex> x, Span<double> destination) {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (ComplexKernels.Magnitude.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> QuaternionZip(TensorOpFamily row, ReadOnlySpan<Quaternion> x, ReadOnlySpan<Quaternion> y, Span<Quaternion> destination) {
        if (x.Length != destination.Length || y.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (QuaternionKernels.Binary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, y, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> QuaternionMap(TensorOpFamily row, ReadOnlySpan<Quaternion> x, Span<Quaternion> destination) {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        if (QuaternionKernels.Unary.GetValueOrDefault(row) is not { } kernel) { return Miss<Unit>(row); }
        try { kernel(x, destination); return Fin.Succ(unit); } catch (Exception ex) { return Threw<Unit>(row, ex); }
    }

    public static Fin<T> Fold<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorFault.Fail<T>("empty-operand", row.Key); }
        if (TensorKernels<T>.Fold.GetValueOrDefault(row) is not { } kernel) { return Miss<T>(row); }
        try { return Fin.Succ(kernel(x)); } catch (Exception ex) { return Threw<T>(row, ex); }
    }
    public static Fin<T> FoldPair<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorFault.Fail<T>("empty-operand", row.Key); }
        if (x.Length != y.Length) { return TensorFault.Fail<T>("length-mismatch", row.Key, $"{x.Length}!={y.Length}"); }
        if (TensorKernels<T>.PairFold.GetValueOrDefault(row) is not { } kernel) { return Miss<T>(row); }
        try { return Fin.Succ(kernel(x, y)); } catch (Exception ex) { return Threw<T>(row, ex); }
    }
    public static Fin<int> IndexOf<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> {
        if (x.IsEmpty) { return TensorFault.Fail<int>("empty-operand", row.Key); }
        if (TensorKernels<T>.Index.GetValueOrDefault(row) is not { } kernel) { return Miss<int>(row); }
        try { return Fin.Succ(kernel(x)); } catch (Exception ex) { return Threw<int>(row, ex); }
    }
    public static Fin<bool> Aggregate<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IBinaryNumber<T> {
        if (AggregateReducers<T>.Rows.GetValueOrDefault(row) is not { } kernel) { return Miss<bool>(row); }
        try { return Fin.Succ(kernel(x)); } catch (Exception ex) { return Threw<bool>(row, ex); }
    }
    public static Fin<int> Hamming<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IEquatable<T> {
        if (x.Length != y.Length) { return TensorFault.Fail<int>("length-mismatch", TensorOpFamily.HammingDistance.Key, $"{x.Length}!={y.Length}"); }
        try { return Fin.Succ(TensorPrimitives.HammingDistance(x, y)); } catch (Exception ex) { return Threw<int>(TensorOpFamily.HammingDistance, ex); }
    }
    public static Fin<long> HammingBits<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IBinaryInteger<T> {
        if (x.Length != y.Length) { return TensorFault.Fail<long>("length-mismatch", TensorOpFamily.HammingBitDistance.Key, $"{x.Length}!={y.Length}"); }
        try { return Fin.Succ(TensorPrimitives.HammingBitDistance(x, y)); } catch (Exception ex) { return Threw<long>(TensorOpFamily.HammingBitDistance, ex); }
    }
    // Index-driven structural movement: every index admits against the addressed extent before any element
    // moves; a colliding scatter resolves last-write-wins in index order, so the result is deterministic.
    public static Fin<Unit> Gather<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> indices, Span<T> destination) {
        if (row != TensorOpFamily.Gather) { return Miss<Unit>(row); }
        if (indices.Length != destination.Length) { return TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{indices.Length}!={destination.Length}"); }
        for (int i = 0; i < indices.Length; i++) {
            if (indices[i] < 0 || indices[i] >= values.Length) { return TensorFault.Fail<Unit>("index-range", row.Key, $"{indices[i]}/{values.Length}"); }
        }
        for (int i = 0; i < indices.Length; i++) { destination[i] = values[indices[i]]; }
        return Fin.Succ(unit);
    }
    public static Fin<Unit> Scatter<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> indices, Span<T> destination) {
        if (row != TensorOpFamily.Scatter) { return Miss<Unit>(row); }
        if (values.Length != indices.Length) { return TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{values.Length}!={indices.Length}"); }
        for (int i = 0; i < indices.Length; i++) {
            if (indices[i] < 0 || indices[i] >= destination.Length) { return TensorFault.Fail<Unit>("index-range", row.Key, $"{indices[i]}/{destination.Length}"); }
        }
        for (int i = 0; i < indices.Length; i++) { destination[indices[i]] = values[i]; }
        return Fin.Succ(unit);
    }
    public static Fin<Unit> Segment<T>(TensorOpFamily row, ReadOnlySpan<T> values, ReadOnlySpan<int> segments, Span<T> destination) where T : IFloatingPointIeee754<T> {
        if (SegmentReducers<T>.Rows.GetValueOrDefault(row) is not { } reducer) { return Miss<Unit>(row); }
        if (values.Length != segments.Length) { return TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{values.Length}!={segments.Length}"); }
        for (int i = 0; i < segments.Length; i++) {
            if (segments[i] < 0 || segments[i] >= destination.Length) { return TensorFault.Fail<Unit>("segment-id-range", row.Key, $"{segments[i]}/{destination.Length}"); }
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

    public static Fin<Unit> Root<T>(TensorOpFamily row, ReadOnlySpan<T> x, int n, Span<T> destination) where T : IFloatingPointIeee754<T>, IRootFunctions<T> {
        if (x.Length != destination.Length) { return Mismatch(row, x.Length, destination.Length); }
        try {
            if (row == TensorOpFamily.RootN) { TensorPrimitives.RootN(x, n, destination); return Fin.Succ(unit); }
            if (row == TensorOpFamily.ScaleB) { TensorPrimitives.ScaleB(x, n, destination); return Fin.Succ(unit); }
            return Miss<Unit>(row);
        }
        catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<bool> filter, in ReadOnlyTensorSpan<T> values) where T : unmanaged {
        if (row != TensorOpFamily.MaskedWrite) { return Miss<Tensor<T>>(row); }
        if (!destination.Lengths.SequenceEqual(filter.Lengths) || !destination.Lengths.SequenceEqual(values.Lengths)) {
            return TensorFault.Fail<Tensor<T>>("mask-shape", row.Key);
        }
        try {
            Tensor.FilteredUpdate(destination.AsTensorSpan(), filter, values);
            return Fin.Succ(destination);
        }
        catch (Exception ex) { return Threw<Tensor<T>>(row, ex); }
    }
    public static Fin<Unit> Pool<T>(TensorOpFamily row, Tensor<T> plane, int axis, int window, int stride, in TensorSpan<T> destination) where T : IFloatingPointIeee754<T> {
        if (PoolReducers<T>.Rows.GetValueOrDefault(row) is not { } reducer) { return Miss<Unit>(row); }
        if (axis < 0 || axis >= plane.Rank) { return TensorFault.Fail<Unit>("pool-axis", row.Key, $"{axis}/{plane.Rank}"); }
        Tensor<T> dense = plane.ToDenseTensor();
        if (dense.Lengths[axis] <= 0) { return TensorFault.Fail<Unit>("pool-empty-axis", row.Key, axis.ToString()); }
        try {
            int extent = checked((int)dense.Lengths[axis]);
            int outer = checked((int)TensorPrimitives.Product<nint>(dense.Lengths[..axis]));
            int inner = checked((int)TensorPrimitives.Product<nint>(dense.Lengths[(axis + 1)..]));
            (int win, int step) = row == TensorOpFamily.GlobalMaxPool || row == TensorOpFamily.GlobalAvgPool ? (extent, extent) : (window, stride);
            if (win <= 0 || step <= 0 || win > extent) { return TensorFault.Fail<Unit>("pool-window-out-of-range", row.Key, $"win={win} step={step} extent={extent}"); }
            int outputs = (extent - win) / step + 1;
            long expected = (long)outer * outputs * inner;
            if (destination.FlattenedLength != expected) { return TensorFault.Fail<Unit>("pool-destination-shape", row.Key, $"{destination.FlattenedLength}!={expected}"); }
            ReadOnlySpan<T> src = MemoryMarshal.CreateReadOnlySpan(ref dense.GetPinnableReference(), checked((int)dense.FlattenedLength));
            Span<T> dst = MemoryMarshal.CreateSpan(ref destination.GetPinnableReference(), checked((int)destination.FlattenedLength));
            for (int prefix = 0; prefix < outer; prefix++) {
                for (int output = 0; output < outputs; output++) {
                    for (int lane = 0; lane < inner; lane++) {
                        T acc = reducer.Seed;
                        for (int offset = 0; offset < win; offset++) {
                            int index = ((prefix * extent) + (output * step) + offset) * inner + lane;
                            acc = reducer.Combine(acc, src[index]);
                        }
                        dst[((prefix * outputs) + output) * inner + lane] = reducer.Final(acc, win);
                    }
                }
            }
            return Fin.Succ(unit);
        }
        catch (Exception ex) { return Threw<Unit>(row, ex); }
    }
    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.Match(
            None: () => Map(row, x.Span, destination.Span),
            Some: won => x.Length != destination.Length ? TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{x.Length}!={destination.Length}")
                : TensorKernels<T>.Unary.GetValueOrDefault(row) is { } kernel
                ? won.Route.Switch(
                    state: (Row: row, X: x, Dest: destination, Kernel: kernel, Budget: budget),
                    blocked: static s => Effects.ToFin(() => ParallelHelper.For(0, Blocks(s.X.Length, s.Budget.PartitionCap), new MapBlock<T>(s.X, s.Dest, BlockSize(s.X.Length, s.Budget.PartitionCap), s.Kernel), minimumActionsPerThread: 1)),
                    plane: static (s, plane) => plane.Rows <= 0 || plane.Columns <= 0 || (long)plane.Rows * plane.Columns != s.X.Length
                        ? TensorFault.Fail<Unit>("partition-plane", s.Row.Key, $"{plane.Rows}x{plane.Columns}!={s.X.Length}")
                        : Effects.ToFin(() => ParallelHelper.For2D(0, plane.Rows, 0, 1, new PlaneBlock<T>(s.X, s.Dest, plane.Columns, s.Kernel))))
                : Miss<Unit>(row));
    public static Fin<Unit> Partition<T>(TensorOpFamily row, ReadOnlyMemory<T> x, ReadOnlyMemory<T> y, Memory<T> destination, CpuBudget budget, Option<BenchmarkRow> claim) where T : IFloatingPointIeee754<T> =>
        claim.Match(
            None: () => Zip(row, x.Span, y.Span, destination.Span),
            Some: won => x.Length != destination.Length || y.Length != destination.Length ? TensorFault.Fail<Unit>("length-mismatch", row.Key, $"{x.Length}/{y.Length}!={destination.Length}")
                : TensorKernels<T>.Binary.GetValueOrDefault(row) is { } kernel
                ? won.Route.Switch(
                    state: (Row: row, X: x, Y: y, Dest: destination, Kernel: kernel, Budget: budget),
                    blocked: static s => Effects.ToFin(() => ParallelHelper.For(0, Blocks(s.X.Length, s.Budget.PartitionCap), new ZipBlock<T>(s.X, s.Y, s.Dest, BlockSize(s.X.Length, s.Budget.PartitionCap), s.Kernel), minimumActionsPerThread: 1)),
                    plane: static (s, plane) => plane.Rows <= 0 || plane.Columns <= 0 || (long)plane.Rows * plane.Columns != s.X.Length
                        ? TensorFault.Fail<Unit>("partition-plane", s.Row.Key, $"{plane.Rows}x{plane.Columns}!={s.X.Length}")
                        : Effects.ToFin(() => ParallelHelper.For2D(0, plane.Rows, 0, 1, new PlaneZipBlock<T>(s.X, s.Y, s.Dest, plane.Columns, s.Kernel))))
                : Miss<Unit>(row));
    private static int BlockSize(int length, int cap) => Math.Max(1, checked((int)((length + (long)Math.Max(1, cap) - 1) / Math.Max(1, cap))));
    private static int Blocks(int length, int cap) => (length + BlockSize(length, cap) - 1) / BlockSize(length, cap);
    private static Fin<A> Miss<A>(TensorOpFamily row) => TensorFault.Fail<A>("kernel-row-miss", row.Key);
}

// --- [TABLES] ------------------------------------------------------------------------------
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

public readonly struct PlaneBlock<T>(ReadOnlyMemory<T> source, Memory<T> destination, int columns, UnaryKernel<T> kernel) : IAction2D {
    public void Invoke(int i, int j) {
        int start = i * columns;
        kernel(source.Span.Slice(start, columns), destination.Span.Slice(start, columns));
    }
}

public readonly struct ZipBlock<T>(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y, Memory<T> destination, int blockSize, BinaryKernel<T> kernel) : IAction {
    public void Invoke(int block) {
        int start = block * blockSize;
        int length = Math.Min(blockSize, x.Length - start);
        kernel(x.Span.Slice(start, length), y.Span.Slice(start, length), destination.Span.Slice(start, length));
    }
}

public readonly struct PlaneZipBlock<T>(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y, Memory<T> destination, int columns, BinaryKernel<T> kernel) : IAction2D {
    public void Invoke(int i, int j) {
        int start = i * columns;
        kernel(x.Span.Slice(start, columns), y.Span.Slice(start, columns), destination.Span.Slice(start, columns));
    }
}
```

## [03]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the forward-mode Jacobian-vector-product as a TOTAL (non-optional) `Func` column on every bound row; `Sensitivity` the ONE directional-derivative owner carrying each non-elementwise op's reverse VJP and forward JVP — sharing one body wherever the two directions coincide so a `Forward`/`Backward` class pair with copy-pasted SoftMax/MatMul bodies is the deleted illusory-dual form — with the MatMul weight projection selected by `AdjointMode` (`Wᵀ` reverse for `ȳ·Wᵀ`, `W` forward for `ẋ·W`), the symmetric SoftMax Jacobian shared across both directions, and the `Operator` DDG geometry apply selecting the page-owned `OperatorRow.Adjoint` (reverse transpose `Aᵀ·ȳ`) or `OperatorRow.Apply` (forward pushforward `A·ṫ`) — the row table composing the kernel `Rasm.Numerics` `DiscreteCalculus`; `SensitivityLaw` the static dual-mode adjoint, forward and reverse tape sweeps over BOTH the `(op, primal)` and `GeometryTape` tapes, the generalized Gauss-Newton `JᵀJ·v` (reverse-over-forward) surface, AND the hyper-dual scalar leg — the THIRD leg of the ONE `Sensitivity` family beside the geometry tape and the `Symbolic/lowering` symbolic tape: a general smooth scalar objective authored once over the `HyperJet` hyper-dual scalar yields the EXACT gradient (order 1) and the EXACT gradient+Hessian (order 2) in one evaluation through `DDScalar.Variables`/`GetGradient()`/`GetHessian()`, deleting the finite-difference fall its consumers carried (a fourth parallel gradient mechanism is the deleted form); `JacobianColoring` the graph-coloring sparse-Jacobian assembler over the AD tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage.
- Entry: `EquivalenceLaw.Prove` admits a positive sample count, captures distribution and kernel boundaries, and applies `ToleranceClass.Bound`. `SensitivityLaw.Adjoint`, `Chain`, `Pushforward`, and `GaussNewton` keep derivative shape and operator failures on `Fin`; `Gradient` and `Hessian` trap hyper-dual evaluation. `JacobianColoring.Of` admits matrix extents and every sparsity coordinate before `Assemble` recovers colored derivatives into CSR storage.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`; the copy points are exactly the three named bridges the `ORT_BRIDGE` capsule owns plus the `Span2D` staging-plane view and the `ByteString` remote-edge projection.
- Packages: Rasm (project), System.Numerics.Tensors, MathNet.Numerics, HyperJet (the hyper-dual scalar-AD leg — `DDScalar`/`DDScalar1..15`/`DDScalarSpan`, `GetGradient()`/`GetHessian()` MathNet export), Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `Tensor/factor#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product and (for a non-elementwise op) its Jacobian-vector-product to one `Sensitivity` directional body, so the six DDG geometry rows each gain reverse-mode adjoint coverage by one `DifferentiableOp` row routing to `Sensitivity.Operator` under `AdjointMode.Reverse` and forward coverage under `AdjointMode.Forward`, a new geometry operator (remeshing-step, connection-Laplacian) lands as one `Tensor/vocabulary#OPERATION_TABLE` geometry row plus one `GeometryAdjoint.Rows` binding, a generalized Gauss-Newton curvature operator is one `SensitivityLaw.GaussNewton` composition over the existing forward+reverse primitives, while the EXACT Hessian-vector product is a distinct second-order capability that grows an `f''` curvature column on `DifferentiableOp` plus a flowing-activation tape (never a free composition of first-order primitives), and a large sparse Jacobian is one `JacobianColoring` over the same tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage — never a parallel autodiff surface; a new gradient SOURCE is one leg on the `Sensitivity` family (the hyperdual scalar leg is the proof — one pair of entries, no fourth mechanism); zero new surface.
- Boundary: `TensorOps` binds verified span members directly, routes matrix rows through `KernelLowering`, folds arbitrary-axis pooling over dense outer×axis×inner coordinates, and rejects missing geometry or arity before mutation. `EquivalenceLaw` compares span kernels against scalar or reassociated references, matrix kernels against `KernelLowering.ProveGemm`, and geometry kernels against the recorded `OperatorRow` transpose identity. `SensitivityLaw` composes total forward and reverse maps, matrix-free `JᵀJ·v`, sparse coloring, and hyper-dual scalar derivatives without parallel gradient owners.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdjointMode {
    public static readonly AdjointMode Forward = new("forward");
    public static readonly AdjointMode Reverse = new("reverse");
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EquivalencePolicy(TensorOpFamily Family, int SampleCount) {
    public static EquivalencePolicy For(TensorOpFamily family) => new(family, SampleCount: 256);
}

public readonly record struct EquivalenceProof(TensorOpFamily Family, double MaxDeviation, int Length, double Mass, double CancellationRatio, int SampleCount, Duration Elapsed, Instant At, CorrelationId Correlation) {
    // `ToleranceClass.Bound(length, mass)` alone owns the error envelope and verdict under the cancellation-ratio gate,
    // never a stored relative scalar (the de-sync the vocabulary owner
    // forecloses): a catastrophically-cancelling reduction is `Vacuous` and certifies nothing. The deviation is
    // ABSOLUTE (the envelope is N·ε·Σ|x|, never relative), so the proof carries `Length`/`Mass`/`CancellationRatio`.
    public double Bound => Family.Tolerance.Bound(Length, Mass);
    public bool Holds => Family.Tolerance.Holds(MaxDeviation, Length, Mass, CancellationRatio);

    public static EquivalenceProof Of(TensorOpFamily family, ProofEvidence evidence, int sampleCount, Duration elapsed, Instant at, CorrelationId correlation) =>
        new(family, evidence.Deviation, evidence.Length, evidence.Mass, evidence.CancellationRatio, sampleCount, elapsed, at, correlation);
}

// Op-agnostic proof evidence funnels every `Prove` arm into `EquivalenceProof`: the absolute max-abs
// deviation, the accumulation length and operand mass the `ToleranceClass` envelope keys, and the cancellation
// ratio the `Vacuous` gate reads. The matrix/geometry families have no scalar-tail span reference, so the
// data-only `Prove` yields `Unprovable` (deviation `+inf`, ratio 1.0 — an honest non-proof, not vacuousness): the
// matrix family routes to `Tensor/factor#KERNEL_LOWERING` `KernelLowering.ProveGemm` and the geometry family to
// `EquivalenceLaw.ProveOperator` over a mesh fixture, each returning real evidence.
public readonly record struct ProofEvidence(double Deviation, int Length, double Mass, double CancellationRatio) {
    public static readonly ProofEvidence Unprovable = new(double.PositiveInfinity, 0, 0.0, 1.0);
}

public static class StagePlane {
    // A Span2D is a ref struct and rides neither a tuple nor a Fin, so the plane leaves through `out` while
    // admission and the copy-point receipt stay on the rail; rows·columns must exactly cover the rented backing.
    public static Fin<CopyPoint> Stage(MemoryOwner<float> backing, int rows, int columns, ClockPolicy clocks, CorrelationId correlation, out Span2D<float> plane) {
        plane = default;
        if (rows <= 0 || columns <= 0 || (long)rows * columns != backing.Length) {
            return TensorFault.Fail<CopyPoint>("stage-plane-shape", $"{rows}x{columns}!={backing.Length}");
        }
        plane = backing.Memory.Span.AsSpan2D(rows, columns);
        return Fin.Succ(new CopyPoint(OrtResidency.SpanView, (long)rows * columns * sizeof(float), "cpu", clocks.Now, correlation));
    }
}

public readonly record struct MatMulGeometry(int Rows, int Inner, int Columns, ShardPlan ShardPlan) {
    public static Fin<MatMulGeometry> Admit(ReadOnlyMemory<float> weights, ReadOnlyMemory<float> direction, AdjointMode mode) {
        int known = direction.Length;
        if (known <= 0 || weights.Length == 0 || weights.Length % known != 0) {
            return TensorFault.Fail<MatMulGeometry>("matmul-adjoint-shape", $"{weights.Length}%{known}");
        }
        int other = weights.Length / known;
        return Fin.Succ(mode == AdjointMode.Forward
            ? new MatMulGeometry(Rows: 1, Inner: known, Columns: other, new ShardPlan.Single())
            : new MatMulGeometry(Rows: 1, Inner: other, Columns: known, new ShardPlan.Single()));
    }

    public Matrix<double> DirectionMatrix(ReadOnlyMemory<float> direction, AdjointMode mode) {
        int width = mode == AdjointMode.Forward ? Inner : Columns;
        return Matrix<double>.Build.Dense(Rows, width, (r, c) => direction.Span[(r * width) + c]);
    }

    public Matrix<double> WeightMatrix(ReadOnlyMemory<float> weights, AdjointMode mode) =>
        mode == AdjointMode.Forward
            ? Matrix<double>.Build.Dense(Inner, Columns, (r, c) => weights.Span[(r * Columns) + c])
            : Matrix<double>.Build.Dense(Columns, Inner, (r, c) => weights.Span[(c * Columns) + r]);

    public ReadOnlyMemory<float> Flatten(Matrix<double> matrix) {
        float[] flat = new float[matrix.RowCount * matrix.ColumnCount];
        for (int r = 0; r < matrix.RowCount; r++) {
            for (int c = 0; c < matrix.ColumnCount; c++) { flat[(r * matrix.ColumnCount) + c] = (float)matrix[r, c]; }
        }
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
// pushforward A·ṫ) for forward over the recorded mesh snapshot.
public static class Sensitivity {
    public static Fin<ReadOnlyMemory<float>> MatMul(ReadOnlyMemory<float> weights, ReadOnlyMemory<float> direction, AdjointMode mode) =>
        MatMulGeometry.Admit(weights, direction, mode).Bind(geometry =>
            KernelLowering.Lower(TensorOpFamily.MatMul, geometry.DirectionMatrix(direction, mode), geometry.WeightMatrix(weights, mode), geometry.ShardPlan)
                .Map(geometry.Flatten));

    public static Fin<ReadOnlyMemory<float>> SoftMax(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction) {
        if (primal.Length != direction.Length) { return TensorFault.Fail<ReadOnlyMemory<float>>("adjoint-length", TensorOpFamily.SoftMax.Key, $"{primal.Length}!={direction.Length}"); }
        using MemoryOwner<float> yOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        using MemoryOwner<float> jacobianOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        Span<float> y = yOwner.Span, jacobian = jacobianOwner.Span;
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, direction.Span);
        TensorPrimitives.Subtract(direction.Span, dot, jacobian);
        TensorPrimitives.Multiply<float>(y, jacobian, jacobian);
        return Fin.Succ<ReadOnlyMemory<float>>(jacobian.ToArray());
    }

    // Reduction directional derivatives are non-diagonal vector→scalar maps. Sum's reverse VJP broadcasts the
    // scalar cotangent over the recorded input extent (x̄ᵢ = ȳ) and its forward JVP contracts the tangent to the
    // scalar Σẋ; Dot's reverse VJP scales the held operand (the recorded primal y) by the scalar cotangent
    // (x̄ᵢ = ȳ·yᵢ) and its forward JVP contracts the tangent with the held operand (ẏ = ẋ·y). These are the
    // dimension-changing reduction adjoints the constitutive strain-energy norm / quadratic-form tapes ride.
    public static Fin<ReadOnlyMemory<float>> Sum(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (mode == AdjointMode.Forward && primal.Length != direction.Length) { return TensorFault.Fail<ReadOnlyMemory<float>>("adjoint-length", TensorOpFamily.Sum.Key, $"{primal.Length}!={direction.Length}"); }
        if (mode == AdjointMode.Reverse && direction.Length != 1) { return TensorFault.Fail<ReadOnlyMemory<float>>("adjoint-seed", TensorOpFamily.Sum.Key, direction.Length.ToString()); }
        if (mode == AdjointMode.Forward) { return Fin.Succ<ReadOnlyMemory<float>>(new[] { TensorPrimitives.Sum(direction.Span) }); }
        float[] broadcast = new float[primal.Length];
        Array.Fill(broadcast, direction.Span[0]);
        return Fin.Succ<ReadOnlyMemory<float>>(broadcast);
    }

    public static Fin<ReadOnlyMemory<float>> Dot(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (mode == AdjointMode.Forward && primal.Length != direction.Length) { return TensorFault.Fail<ReadOnlyMemory<float>>("adjoint-length", TensorOpFamily.Dot.Key, $"{primal.Length}!={direction.Length}"); }
        if (mode == AdjointMode.Reverse && direction.Length != 1) { return TensorFault.Fail<ReadOnlyMemory<float>>("adjoint-seed", TensorOpFamily.Dot.Key, direction.Length.ToString()); }
        if (mode == AdjointMode.Forward) {
            return Fin.Succ<ReadOnlyMemory<float>>(new[] { TensorPrimitives.Dot<float>(direction.Span, primal.Span) });
        }
        float[] scaled = new float[primal.Length];
        TensorPrimitives.Multiply(primal.Span, direction.Span[0], scaled);
        return Fin.Succ<ReadOnlyMemory<float>>(scaled);
    }

    // DEC operators use float64 (`Arr<double>` the Rasm.Numerics carrier) while the autodiff tape is
    // float32; the impedance converts through `TensorPrimitives.ConvertChecked` at the seam, never the phantom
    // `Arr.fromSpan`/`Arr.AsSpan` spelling — the verified factory is `Arr.create<T>(ReadOnlySpan<T>)` and the
    // read-back is `.ToArray()`. A row outside the geometry table returns `no-operator-row`.
    public static Fin<ReadOnlyMemory<float>> Operator(GeometryTape step, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (!GeometryAdjoint.Rows.TryGetValue(step.Op, out OperatorRow? row)) { return TensorFault.Fail<ReadOnlyMemory<float>>("no-operator-row", step.Op.Key); }
        Func<MeshAdjointSnapshot, Arr<double>, Fin<Arr<double>>> apply = mode == AdjointMode.Reverse ? row.Adjoint : row.Apply;
        try {
            using MemoryOwner<double> wide = MemoryOwner<double>.Allocate(direction.Length, AllocationMode.Clear);
            TensorPrimitives.ConvertChecked(direction.Span, wide.Span);
            return apply(step.Snapshot, Arr.create<double>(wide.Span)).Map(static result => {
                float[] narrow = new float[result.Count];
                TensorPrimitives.ConvertChecked<double, float>(result.ToArray(), narrow);
                return (ReadOnlyMemory<float>)narrow;
            });
        }
        catch (Exception ex) { return TensorFault.Fail<ReadOnlyMemory<float>>("operator-adjoint", step.Op.Key, ex.Message); }
    }
}

public readonly record struct GeometryTape(TensorOpFamily Op, MeshAdjointSnapshot Snapshot);

// Six-row DDG operator table stays Compute-owned; the kernel declares only DiscreteCalculus (Numerics/Spectral.cs)
// and the MeshAdjointSnapshot handle. Apply is the forward map, Adjoint the plain transpose the ⟨A·x,y⟩ == ⟨x,Aᵀ·y⟩
// proof reads: incidence rows pair by transpose (Gradient ↔ Divergence over D0, Curl over D1), symmetric weak rows
// alias Adjoint to Apply; transposes re-index the live CSR fields through FromTriplets, never a re-assembled operator.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OperatorRow {
    public static readonly OperatorRow Gradient = new("gradient", D0Apply, D0TransposeApply);
    public static readonly OperatorRow Divergence = new("divergence", D0TransposeApply, D0Apply);
    public static readonly OperatorRow Curl = new("curl", D1Apply, D1TransposeApply);
    public static readonly OperatorRow CotangentLaplacian = new("cotangent-laplacian", WeakLaplacian, WeakLaplacian);
    public static readonly OperatorRow HeatFlow = new("heat-flow", HeatOperator, HeatOperator);
    public static readonly OperatorRow Spectral = new("spectral", NormalizedLaplacian, NormalizedLaplacian);

    [UseDelegateFromConstructor]
    public partial Fin<Arr<double>> Apply(MeshAdjointSnapshot snapshot, Arr<double> field);

    [UseDelegateFromConstructor]
    public partial Fin<Arr<double>> Adjoint(MeshAdjointSnapshot snapshot, Arr<double> field);

    private static Fin<Arr<double>> D0Apply(MeshAdjointSnapshot snapshot, Arr<double> field) => snapshot.Calculus.D0.Multiply(field);

    private static Fin<Arr<double>> D1Apply(MeshAdjointSnapshot snapshot, Arr<double> field) => snapshot.Calculus.D1.Multiply(field);

    private static Fin<Arr<double>> D0TransposeApply(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        Transposed(snapshot.Calculus.D0).Bind(transpose => transpose.Multiply(field));

    private static Fin<Arr<double>> D1TransposeApply(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        Transposed(snapshot.Calculus.D1).Bind(transpose => transpose.Multiply(field));

    // Weak cotangent Laplacian L = D0ᵀ·diag(Star1)·D0 — symmetric by construction, its own transpose.
    private static Fin<Arr<double>> WeakLaplacian(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        D0Apply(snapshot, field).Map(edge => Scaled(edge, snapshot.Calculus.Star1)).Bind(weighted => D0TransposeApply(snapshot, weighted));

    // Unit-step implicit heat operator diag(Star0) + L — SPD, self-adjoint.
    private static Fin<Arr<double>> HeatOperator(MeshAdjointSnapshot snapshot, Arr<double> field) =>
        WeakLaplacian(snapshot, field).Map(stiff => {
            double[] summed = new double[stiff.Count];
            for (int i = 0; i < summed.Length; i++) { summed[i] = stiff[i] + (snapshot.Calculus.Star0[i] * field[i]); }
            return Arr.create<double>(summed);
        });

    // Mass-symmetrized pencil diag(Star0)^-1/2 · L · diag(Star0)^-1/2 — the spectral-basis operator form;
    // Star0 entries are strictly positive under DiscreteCalculus.IsValid, so the half-inverse is total.
    private static Fin<Arr<double>> NormalizedLaplacian(MeshAdjointSnapshot snapshot, Arr<double> field) {
        double[] halfInverse = new double[snapshot.Calculus.Star0.Count];
        for (int i = 0; i < halfInverse.Length; i++) { halfInverse[i] = 1.0 / Math.Sqrt(snapshot.Calculus.Star0[i]); }
        Arr<double> weights = Arr.create<double>(halfInverse);
        return WeakLaplacian(snapshot, Scaled(field, weights)).Map(stiff => Scaled(stiff, weights));
    }

    private static Fin<SparseMatrix> Transposed(SparseMatrix source) {
        IEnumerable<(int Row, int Col, double Value)> Swapped() {
            for (int row = 0; row < source.RowPtr.Count - 1; row++) {
                for (int index = source.RowPtr[row]; index < source.RowPtr[row + 1]; index++) {
                    yield return (source.ColInd[index], row, source.Values[index]);
                }
            }
        }
        return SparseMatrix.FromTriplets(source.Cols, source.Rows, Swapped());
    }

    private static Arr<double> Scaled(Arr<double> values, Arr<double> weights) {
        double[] scaled = new double[values.Count];
        for (int i = 0; i < scaled.Length; i++) { scaled[i] = values[i] * weights[i]; }
        return Arr.create<double>(scaled);
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
    // domain vector x; `Adjoint` consumes a random codomain vector y; the absolute
    // inner-product residual |⟨A·x, y⟩ − ⟨x, Aᵀ·y⟩| is the deviation the envelope bounds. A fabricated dense
    // Jacobian of the sparse operator, or a proof that never applies the row, is the deleted hollow form.
    public static Fin<ProofEvidence> ProveAdjoint(OperatorRow row, MeshAdjointSnapshot snapshot) {
        Arr<double> x = Gaussian(Domain(row, snapshot));
        return row.Apply(snapshot, x).Bind(ax => {
            Arr<double> y = Gaussian(ax.Count);
            return row.Adjoint(snapshot, y).Map(aty => {
                double forward = TensorPrimitives.Dot<double>(ax.ToArray(), y.ToArray());
                double reverse = TensorPrimitives.Dot<double>(x.ToArray(), aty.ToArray());
                return new ProofEvidence(double.Abs(forward - reverse), x.Count + ax.Count, double.Abs(forward) + double.Abs(reverse), 1.0);
            });
        });
    }

    // Gradient/CotangentLaplacian/HeatFlow/Spectral act on the vertex field (|V|), Divergence/Curl on the edge
    // field (|E|); the codomain length falls out of `Apply`, so only the domain length is keyed here.
    static int Domain(OperatorRow row, MeshAdjointSnapshot snapshot) =>
        row == OperatorRow.Divergence || row == OperatorRow.Curl ? snapshot.EdgeCount : snapshot.VertexCount;

    static Arr<double> Gaussian(int length) {
        Tensor<double> flat = Tensor.CreateFromShape<double>([length]);
        Tensor.FillGaussianNormalDistribution(flat);
        double[] values = new double[length];
        flat.FlattenTo(values);
        return Arr.create<double>(values);
    }
}

public sealed record DifferentiableOp(
    TensorOpFamily Forward,
    bool Diagonal,
    Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, Fin<ReadOnlyMemory<float>>> Vjp,
    Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, Fin<ReadOnlyMemory<float>>> Jvp) {
    public static readonly FrozenDictionary<TensorOpFamily, DifferentiableOp> Rows = new Dictionary<TensorOpFamily, DifferentiableOp> {
        [TensorOpFamily.Tanh] = Diag(TensorOpFamily.Tanh, static (primal, seed) => Elementwise(primal, seed, static p => 1f - p * p)),
        [TensorOpFamily.Sigmoid] = Diag(TensorOpFamily.Sigmoid, static (primal, seed) => Elementwise(primal, seed, static p => p * (1f - p))),
        [TensorOpFamily.Exp] = Diag(TensorOpFamily.Exp, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        [TensorOpFamily.Log] = Diag(TensorOpFamily.Log, static (primal, seed) => Elementwise(primal, seed, static p => 1f / p)),
        [TensorOpFamily.ReLU] = Diag(TensorOpFamily.ReLU, static (primal, seed) => Elementwise(primal, seed, static p => p > 0f ? 1f : 0f)),
        // Elementwise-ring chain rows stay diagonal in the flowing operand the (op, primal)
        // tape threads, the held operand (or a precomputed local coefficient) recorded as the primal — the MatMul
        // `primal=weight` convention extended to the scalar ring. Add/Subtract differentiate the addend/minuend
        // (∂=1, the held operand carried but unread); Multiply scales by the held factor (∂=y, primal=y); Divide
        // by the reciprocal denominator (∂=1/y, primal=y); Pow records the power-rule diagonal y·x^(y−1) as its
        // primal because the local derivative needs BOTH base and exponent (neither snapshot alone suffices) —
        // Stored pullback supplies the cotangent multiplier and the constitutive return-map / multi-term
        // stored-energy tape vocabulary the `Solver/constitutive#CONSTITUTIVE` stress-update/contact tapes compose.
        [TensorOpFamily.Add] = Diag(TensorOpFamily.Add, static (primal, seed) => Elementwise(primal, seed, static _ => 1f)),
        [TensorOpFamily.Subtract] = Diag(TensorOpFamily.Subtract, static (primal, seed) => Elementwise(primal, seed, static _ => 1f)),
        [TensorOpFamily.Multiply] = Diag(TensorOpFamily.Multiply, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        [TensorOpFamily.Divide] = Diag(TensorOpFamily.Divide, static (primal, seed) => Elementwise(primal, seed, static p => 1f / p)),
        [TensorOpFamily.Pow] = Diag(TensorOpFamily.Pow, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        // MatMul is genuinely bilinear: the reverse VJP applies Wᵀ, the forward JVP applies W, so the two
        // arms are distinct directional maps, not the deleted copy-paste-identical body.
        [TensorOpFamily.MatMul] = Bilinear(TensorOpFamily.MatMul,
            static (weights, seed) => Sensitivity.MatMul(weights, seed, AdjointMode.Reverse),
            static (weights, tangent) => Sensitivity.MatMul(weights, tangent, AdjointMode.Forward)),
        // SoftMax's Jacobian is symmetric, so one body serves both directions — Bilinear with the same map
        // names the shared identity instead of duplicating it.
        [TensorOpFamily.SoftMax] = Bilinear(TensorOpFamily.SoftMax,
            static (primal, seed) => Sensitivity.SoftMax(primal, seed),
            static (primal, tangent) => Sensitivity.SoftMax(primal, tangent)),
        // Reduction rows are non-diagonal (vector→scalar), so reverse VJP and forward JVP are genuinely
        // distinct directional maps routed to the one `Sensitivity` owner, not a shared diagonal fold: Sum
        // broadcasts the scalar cotangent over the recorded extent (reverse) and contracts the tangent to Σẋ
        // (forward); Dot scales the held operand by the scalar cotangent (reverse) and contracts the tangent
        // with it (forward). These supply the strain-energy norm / quadratic-form tape vocabulary the
        // `Solver/constitutive#CONSTITUTIVE` exact return-map (algorithmic) tangent composes.
        [TensorOpFamily.Sum] = Bilinear(TensorOpFamily.Sum,
            static (primal, seed) => Sensitivity.Sum(primal, seed, AdjointMode.Reverse),
            static (primal, tangent) => Sensitivity.Sum(primal, tangent, AdjointMode.Forward)),
        [TensorOpFamily.Dot] = Bilinear(TensorOpFamily.Dot,
            static (primal, seed) => Sensitivity.Dot(primal, seed, AdjointMode.Reverse),
            static (primal, tangent) => Sensitivity.Dot(primal, tangent, AdjointMode.Forward)),
    }.ToFrozenDictionary();

    // An elementwise op's diagonal Jacobian makes the VJP and JVP the one `direction .* f'(primal)` fold, so
    // both directions bind the same derivative body.
    static DifferentiableOp Diag(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> derivative) =>
        new(forward, Diagonal: true,
            (primal, seed) => Capture(forward, () => Fin.Succ(derivative(primal, seed))),
            (primal, tangent) => Capture(forward, () => Fin.Succ(derivative(primal, tangent))));

    // Forward is total over every bound row: each carries a real JVP, so the prior Option<Jvp> + the dead
    // <no-forward-jvp> fault are deleted — a row either resolves both directions or is absent (no-adjoint-row).
    static DifferentiableOp Bilinear(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, Fin<ReadOnlyMemory<float>>> vjp, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, Fin<ReadOnlyMemory<float>>> jvp) =>
        new(forward, Diagonal: false,
            (primal, seed) => Capture(forward, () => vjp(primal, seed)),
            (primal, tangent) => Capture(forward, () => jvp(primal, tangent)));

    static Fin<ReadOnlyMemory<float>> Capture(TensorOpFamily row, Func<Fin<ReadOnlyMemory<float>>> run) =>
        Try.lift(run).Run().MapFail(error => TensorFault.Symbol("adjoint-threw", row.Key, error.Message)).Bind(static result => result);

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
            ? (mode == AdjointMode.Reverse ? differentiable.Vjp : differentiable.Jvp)(primal, seed)
            : TensorFault.Fail<ReadOnlyMemory<float>>("no-adjoint-row", op.Key);

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), (grad, step) => grad.Bind(g => Adjoint(step.Op, AdjointMode.Reverse, step.Primal, g)));

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<GeometryTape> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), static (grad, step) => grad.Bind(g => Sensitivity.Operator(step, g, AdjointMode.Reverse)));

    // Forward-mode pushforward through the (op, primal) tape: the dual of the reverse Chain, threading a
    // forward tangent inputs-to-outputs so a tall-Jacobian problem (few inputs, many outputs) costs one
    // forward sweep.
    public static Fin<ReadOnlyMemory<float>> Pushforward(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> tangent) =>
        tape.Fold(Fin.Succ(tangent), (dot, step) => dot.Bind(t => Adjoint(step.Op, AdjointMode.Forward, step.Primal, t)));

    // Forward geometry sweep, the dual of the reverse Chain(Seq<GeometryTape>): each step applies its own DEC
    // operator's pushforward (the operator is its own forward map) against THAT step's recorded mesh snapshot,
    // so the forward and reverse geometry sweeps both ride the one OperatorRow with no re-assembled matrix.
    public static Fin<ReadOnlyMemory<float>> Pushforward(Seq<GeometryTape> tape, ReadOnlyMemory<float> tangent) =>
        tape.Fold(Fin.Succ(tangent), static (dot, step) => dot.Bind(t => Sensitivity.Operator(step, t, AdjointMode.Forward)));

    // Generalized Gauss-Newton matrix-free product JᵀJ·v: run the forward JVP seeded by `vector` to get J·v,
    // then the reverse Chain to get Jᵀ·(J·v) — reverse-over-forward of the ONE first-order tape, no dense
    // matrix materialized. This is the SPD curvature operator Newton-CG / trust-region / Levenberg-Marquardt
    // consume (SPD by construction, so CG never breaks on an indefinite step). It is NOT the exact Hessian:
    // Curvature Σ x̄ₖ·f''(primalₖ)·ẋₖ is absent because the first-order (op, primal) tape carries neither the
    // flowing activations nor a second-derivative column — the true Hessian-vector product is a separate
    // second-order capability (an f'' row on DifferentiableOp plus a forward-over-reverse sweep), not this fold.
    public static Fin<ReadOnlyMemory<float>> GaussNewton(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> vector) =>
        Pushforward(tape, vector).Bind(forwardDot => Chain(tape, forwardDot));

    // Hyper-dual scalars form the third `Sensitivity` leg beside geometry and symbolic tapes; a smooth scalar
    // objective authored once over the HyperJet scalar yields the
    // exact gradient (order 1) or exact gradient + Hessian (order 2) in ONE evaluation; the primal Value
    // rides free and a non-finite result is a typed fault, never a NaN sentinel downstream.
    public static Fin<(double Value, double[] Gradient)> Gradient(Func<DDScalar[], DDScalar> objective, double[] at) {
        try {
            DDScalar f = objective(DDScalar.Variables(at, order: 1));
            double[] gradient = f.GetGradient();
            return double.IsFinite(f.Value) && gradient.All(double.IsFinite)
                ? Fin.Succ((f.Value, gradient))
                : TensorFault.Fail<(double, double[])>("hyperdual-nonfinite", $"n={at.Length}");
        }
        catch (Exception ex) { return TensorFault.Fail<(double, double[])>("hyperdual-evaluation", ex.Message); }
    }

    public static Fin<(double Value, double[] Gradient, double[,] Hessian)> Hessian(Func<DDScalar[], DDScalar> objective, double[] at) {
        try {
            DDScalar f = objective(DDScalar.Variables(at, order: 2));
            double[] gradient = f.GetGradient();
            double[,] hessian = f.GetHessian();
            return double.IsFinite(f.Value) && gradient.All(double.IsFinite) && hessian.Cast<double>().All(double.IsFinite)
                ? Fin.Succ((f.Value, gradient, hessian))
                : TensorFault.Fail<(double, double[], double[,])>("hyperdual-nonfinite", $"n={at.Length}");
        }
        catch (Exception ex) { return TensorFault.Fail<(double, double[], double[,])>("hyperdual-evaluation", ex.Message); }
    }
}

// Sparse-Jacobian construction by graph coloring: detect the sparsity pattern, color the structurally
// orthogonal columns (greedy distance-1 degree-ordered), then recover the full Jacobian in (#colors)
// directional-derivative passes instead of (#columns), scattering the compressed columns directly into the
// SparseFormat CSR storage the sparse lane owns. Below a sparsity threshold the owner falls through to
// direct per-column AD because coloring graph-construction cost dominates for a small dense Jacobian.
public sealed record JacobianColoring(int Rows, int Columns, Seq<(int Row, int Column)> Pattern, ImmutableArray<int> Colors, int ColorCount) {
    public static Fin<JacobianColoring> Of(int rows, int columns, Seq<(int Row, int Column)> pattern) {
        if (rows < 0 || columns < 0) { return TensorFault.Fail<JacobianColoring>("jacobian-shape", $"{rows}x{columns}"); }
        if (pattern.Exists(entry => entry.Row < 0 || entry.Row >= rows || entry.Column < 0 || entry.Column >= columns)) {
            return TensorFault.Fail<JacobianColoring>("jacobian-pattern", $"{rows}x{columns}");
        }
        var adjacency = new HashSet<int>[columns];
        for (int c = 0; c < columns; c++) { adjacency[c] = []; }
        var byRow = pattern.GroupBy(static e => e.Row);
        foreach (var group in byRow) {
            int[] cols = group.Select(static e => e.Column).ToArray();
            for (int a = 0; a < cols.Length; a++)
                for (int b = a + 1; b < cols.Length; b++) { adjacency[cols[a]].Add(cols[b]); adjacency[cols[b]].Add(cols[a]); }
        }
        int[] color = new int[columns];
        Array.Fill(color, -1);
        int[] order = Enumerable.Range(0, columns).OrderByDescending(c => adjacency[c].Count).ToArray();
        foreach (int col in order) {
            var used = adjacency[col].Where(n => color[n] >= 0).Select(n => color[n]).ToHashSet();
            int chosen = 0;
            while (used.Contains(chosen)) { chosen++; }
            color[col] = chosen;
        }
        int count = columns == 0 ? 0 : color.Max() + 1;
        return Fin.Succ(new JacobianColoring(rows, columns, pattern, [.. color], count));
    }

    public bool BelowThreshold(int directThreshold) => Columns <= directThreshold || ColorCount >= Columns;

    // One seed vector per color probes the structurally-orthogonal column group through the forward
    // Pushforward (or the reverse Chain); each color's directional derivative scatters its pattern entries as
    // COO triplets accumulated in color order, then handed ONCE to the `Tensor/factor#SPARSE_SOLVE`-owned
    // `SparseOps.Ingest(Coo)` CSR conversion — never a raw `CoordinateStorage` RowIndices/Values surgery the
    // sparse owner forbids, and never a second ingestion path beside the one factor.md owns.
    public Fin<SparseCompressedRowMatrixStorage<double>> Assemble(Func<int, Fin<ReadOnlyMemory<float>>> probeColor) =>
        toSeq(Enumerable.Range(0, ColorCount))
            .Fold(Fin.Succ(Seq<(int Row, int Column, double Value)>()), (acc, seedColor) =>
                acc.Bind(triplets => probeColor(seedColor).Map(directional =>
                    triplets + Pattern
                        .Filter(entry => Colors[entry.Column] == seedColor)
                        .Map(entry => (entry.Row, entry.Column, entry.Row < directional.Length ? (double)directional.Span[entry.Row] : 0.0)))))
            .Bind(triplets => SparseOps.Ingest(SparseFormat.Coo, Rows, Columns,
                [.. triplets.Map(static t => t.Row)], [.. triplets.Map(static t => t.Column)], [.. triplets.Map(static t => t.Value)]));
}

public static class EquivalenceLaw {
    // Proves the span-kernel lane against its scalar-path reference over sampled inputs — OP-AGNOSTIC, needing
    // no per-op reference: an elementwise row runs the candidate VECTOR body against the SAME kernel applied
    // element-by-element (the length-1 scalar/tail path the SIMD body must match per the compute length-class
    // law), a span-coupled row (`SoftMax`/`LogSoftMax`) runs the shift-invariance identity f(x+c) == f(x)
    // because its length-1 slice is a constant, a reduction row runs the candidate against the SAME reduction
    // over the reversed input (the reassociation-stability the `AccumulationScaled` bound certifies). The deviation is the ABSOLUTE max abs
    // gap and the verdict is the `ToleranceClass.Bound(length, mass)` envelope under the cancellation gate — a
    // stored relative scalar is the de-sync the vocabulary owner forecloses. Diffing two unrelated random fills
    // without ever running the kernel is the deleted hollow form. The matrix family has no scalar-tail kernel,
    // so it routes to the `Tensor/factor#KERNEL_LOWERING` GEMM-vs-naive-reference proof (`KernelLowering.ProveGemm`,
    // which OWNS MatMul/Conv admission); the geometry family has no data fixture here, so the data-only `Prove`
    // yields `Unprovable` and the geometry gate is `ProveOperator` over a `MeshAdjointSnapshot`.
    public static Fin<EquivalenceProof> Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy) =>
        policy.SampleCount <= 0
            ? TensorFault.Fail<EquivalenceProof>("equivalence-sample-count", policy.Family.Key, policy.SampleCount.ToString())
            : Try.lift(() => {
                  long mark = clocks.Mark();
                  Tensor<double> lhs = Tensor.CreateFromShape<double>([policy.SampleCount]);
                  Tensor<double> rhs = Tensor.CreateFromShape<double>([policy.SampleCount]);
                  Tensor.FillGaussianNormalDistribution(lhs);
                  Tensor.FillUniformDistribution(rhs);
                  using MemoryOwner<double> aOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
                  using MemoryOwner<double> bOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
                  lhs.FlattenTo(aOwner.Span);
                  rhs.FlattenTo(bOwner.Span);
                  ReadOnlySpan<double> a = aOwner.Span, b = bOwner.Span;
                  ProofEvidence evidence =
                      Coupled.Contains(policy.Family) && TensorKernels<double>.Unary.GetValueOrDefault(policy.Family) is { } whole ? SpanEvidence(CoupledGap(whole, a), a, policy.SampleCount)
                      : TensorKernels<double>.Unary.GetValueOrDefault(policy.Family) is { } unary ? SpanEvidence(UnaryGap(unary, a), a, policy.SampleCount)
                      : TensorKernels<double>.Binary.GetValueOrDefault(policy.Family) is { } binary ? SpanEvidence(BinaryGap(binary, a, b), a, policy.SampleCount)
                      : TensorKernels<double>.Fold.GetValueOrDefault(policy.Family) is { } fold ? SpanEvidence(FoldGap(fold, a), a, policy.SampleCount)
                      : KernelLowering.IsMatrix(policy.Family) ? KernelLowering.ProveGemm(policy.SampleCount)
                      : ProofEvidence.Unprovable;
                  return EquivalenceProof.Of(policy.Family, evidence, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
              }).Run().MapFail(error => TensorFault.Symbol("equivalence-threw", policy.Family.Key, error.Message));

    // Geometry rows unreachable by data-only `Prove` certify through the
    // adjoint-identity ⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩ over a `MeshAdjointSnapshot` fixture, composing the page-owned
    // `OperatorRow.Apply`/`Adjoint` transpose-pair via `GeometryAdjoint.ProveAdjoint`. A row outside the geometry
    // table is `no-adjoint-row` (never silently admitted), and the verdict reads the same `ToleranceClass`
    // envelope the span/matrix proofs read — the geometry band keys the proof, never a loosened bound.
    public static Fin<EquivalenceProof> ProveOperator(ClockPolicy clocks, CorrelationId correlation, TensorOpFamily family, MeshAdjointSnapshot snapshot) {
        long mark = clocks.Mark();
        return GeometryAdjoint.Rows.TryGetValue(family, out OperatorRow? row)
            ? GeometryAdjoint.ProveAdjoint(row, snapshot).Map(evidence => EquivalenceProof.Of(family, evidence, evidence.Length, clocks.Elapsed(mark), clocks.Now, correlation))
            : TensorFault.Fail<EquivalenceProof>("no-adjoint-row", family.Key);
    }

    // Span deviation is absolute (N·ε·Σ|x|), so evidence carries operand mass Σ|xᵢ| and cancellation ratio.
    static ProofEvidence SpanEvidence(double deviation, ReadOnlySpan<double> input, int length) {
        double mass = double.Abs(TensorPrimitives.SumOfMagnitudes<double>(input));
        double ratio = mass > 0.0 ? double.Abs(TensorPrimitives.Sum<double>(input)) / mass : 1.0;
        return new ProofEvidence(deviation, length, mass, ratio);
    }

    // A span-coupled row has no scalar-tail identity — softmax over a length-1 slice is the constant 1 — so
    // its reference is the shift invariance f(x + c) == f(x), a real metamorphic oracle at the same envelope.
    static readonly FrozenSet<TensorOpFamily> Coupled = new[] { TensorOpFamily.SoftMax, TensorOpFamily.LogSoftMax }.ToFrozenSet();

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
- Cases: the grounded `DeviceKernels.Wgsl` device op rows — `MatMul` (tiled GEMM over `WgslSource.TiledGemm`), `Conv2D` (`WgslSource.Im2Col` gather then the TiledGemm pipeline, the two-dispatch convolution mirroring the CPU im2col-then-GEMM), `MaxPool`/`AvgPool` (strided-window reduce) — each a real WGSL compute pipeline compiled and cached on the registry; `Conv1D`/`Conv3D` and the `Tensor/factor#SPARSE_ALGEBRA` `Spmv`/`Spmm` rows stay CPU-lowered through factor.md until their device shaders ground (the device path is never a phantom mapping), the elementwise `TensorKernels<T>` rows stay CPU `TensorPrimitives`, and a device elementwise map is a future row, not a fork of the dispatch surface.
- Entry: `Compile(WgpuDevice, TensorOpFamily)` compiles once per `(device identity, op family)` through a thread-safe `Lazy<Fin<DeviceKernel>>`; `Release(WgpuDevice)` retires every cached module, pipeline, and layout for that device. `Dispatch(WgpuDevice, DevicePlan, ReadOnlySpan<DeviceBuffer>, OrtResidency, CorrelationId)` admits every roster index, device-resident buffer, positive workgroup component, and receipt-width conversion before recording all steps on one encoder and retiring one submission.
- Auto: `KernelLowering.Lower` (and the sparse SpMV/SpMM entry of `Tensor/factor#SPARSE_ALGEBRA`) consult `DeviceKernels` instead of the CPU GEMM ONLY when the active `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row is selected AND the `OrtResidency.DeviceResident` gate holds AND a winning `BenchmarkRow` names the device route in its `Route` column — otherwise the CPU `Matrix<double>.Multiply` GEMM is the terminal, so the CPU/device split rides residency and a benchmark claim, never a fork of the `Map`/`Lower` dispatch contract; a device GEMM output feeding the render lane crosses the existing `Rasm.AppUi/Render` `ResidencyManifest.Mint` seam (the same physical `Buffer`, no host copy) rather than a new device-to-render path, and the one shared device descriptor that this row resolves also gates the ONNX Runtime Mac execution-provider residency so a model-lane device tensor and a tensor-lane device kernel resolve the same allocator on the same physical device.
- Receipt: a device dispatch emits the `TensorRun` `ComputeReceipt` carrying the op family, the resolved per-pass GPU nanosecond duration from the `QuerySet` timestamp (never a busy-wait fence), the `device-wgpu` SIMD-width tag and the workgroup count as the partition count, the `DeterminismTag` extended with the device identity, and the `Tensor/memory#ALLOCATION_AXIS` `AllocationClass.DeviceWgpu`; the device GEMM is a new `LinearProvider.DeterminismTag` because a device result is bit-divergent from the managed/native CPU GEMM, so the `SolveDedupKey` folds the device identity exactly as it folds the managed/native provider or a cross-substrate cache hit returns bit-divergent numbers.
- Packages: Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU (the `Wgpu` table for `DevicePoll`/`QueueSubmitForIndex` device-tick readback), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new device operation is one WGSL row; a launch shape is one `DeviceStep.Workgroups` value; a multi-kernel chain is one `DevicePlan` value with roster-indexed intermediates and one submission. Device residency remains `OrtResidency.DeviceResident`, never a parallel tensor type.
- Boundary: `DeviceKernels.Compile` caches typed compile results by device identity and operation, rejects null native handles, releases partial construction, and exposes device-scoped cache retirement. `DevicePlan` carries ordered kernels, binding slots, and workgroups. `DeviceDispatch.Dispatch` proves non-empty bindings, device residency, binding indexes, workgroup arithmetic, terminal output byte alignment, and one common submission. `WgpuDevice.RecordAndSubmit` admits plan and binding counts against fixed caps before any stack staging, then owns one encoder, timestamped passes, one submit, blocking poll, one mapped readback, and deterministic transient-handle release; shared `Device` and `Queue` remain AppUi-owned. Device-limit negotiation and WGPU↔ORT pointer interop remain research leaves because no verified body claims them.

```csharp signature
// --- [CONSTANTS] ---------------------------------------------------------------------------
// WGSL rows compile one pipeline per grounded op: tiled GEMM, im2col projection, and strided-window pooling.
public static class WgslSource {
    public const string TiledGemm = """
        @group(0) @binding(0) var<storage, read> a : array<f32>;
        @group(0) @binding(1) var<storage, read> b : array<f32>;
        @group(0) @binding(2) var<storage, read_write> c : array<f32>;
        @group(0) @binding(3) var<uniform> dims : vec3<u32>;
        const TILE : u32 = 16u;
        var<workgroup> a_tile : array<f32, 256>;
        var<workgroup> b_tile : array<f32, 256>;
        @compute @workgroup_size(16, 16, 1)
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

    public const string Im2Col = """
        struct ConvGeom {
            channels : u32, in_h : u32, in_w : u32, kernel_h : u32,
            kernel_w : u32, stride_h : u32, stride_w : u32, pad_h : u32,
            pad_w : u32, dil_h : u32, dil_w : u32, out_w : u32,
        };
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> patch : array<f32>;
        @group(0) @binding(2) var<uniform> g : ConvGeom;
        @compute @workgroup_size(8, 8, 1)
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

    public const string StridedWindowMax = """
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> output : array<f32>;
        @group(0) @binding(2) var<uniform> p : vec4<u32>;
        @compute @workgroup_size(64, 1, 1)
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

    public const string StridedWindowAvg = """
        @group(0) @binding(0) var<storage, read> input : array<f32>;
        @group(0) @binding(1) var<storage, read_write> output : array<f32>;
        @group(0) @binding(2) var<uniform> p : vec4<u32>;
        @compute @workgroup_size(64, 1, 1)
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
public readonly record struct DeviceBuffer(nuint Handle, long ByteLength, OrtResidency Residency);

public sealed record DeviceKernel(TensorOpFamily Op, nuint Pipeline, nuint BindGroupLayout, nuint ShaderModule);

public readonly record struct DeviceStep(DeviceKernel Kernel, ImmutableArray<int> Bindings, (uint X, uint Y, uint Z) Workgroups);

// One command submission carries roster-indexed steps, device-resident intermediates, timestamps, and one readback.
public sealed record DevicePlan(Seq<DeviceStep> Steps) {
    public static DevicePlan Of(DeviceKernel kernel, ImmutableArray<int> bindings, (uint X, uint Y, uint Z) workgroups) =>
        new(Seq(new DeviceStep(kernel, bindings, workgroups)));
}

// --- [SERVICES] ----------------------------------------------------------------------------
// Compute-lane capsule composes AppUi's shared `Device`/`Queue`; compute-only handles release through native calls.
public sealed unsafe class WgpuDevice(WebGPU api, Wgpu ext, Device* device, Queue* queue, string identity) {
    // Stackalloc admission caps: a plan is a short kernel chain and a WGSL row binds a handful of buffers, so
    // caller-controlled counts never size an unbounded stack frame.
    const int MaxPlanSteps = 64;
    const int MaxStepBindings = 8;

    static readonly PfnBufferMapCallback MapNoop = new(static (BufferMapAsyncStatus status, void* data) => { });

    public string Identity => identity;

    internal Fin<DeviceKernel> Build(TensorOpFamily op, string wgsl) {
        nint code = 0;
        nint entry = 0;
        ShaderModule* module = null;
        ComputePipeline* pipeline = null;
        BindGroupLayout* layout = null;
        bool transferred = false;
        try {
            code = SilkMarshal.StringToPtr(wgsl, NativeStringEncoding.UTF8);
            entry = SilkMarshal.StringToPtr("main", NativeStringEncoding.UTF8);
            ShaderModuleWGSLDescriptor wgslDesc = new() { Chain = new ChainedStruct { SType = SType.ShaderModuleWgslDescriptor }, Code = (byte*)code };
            ShaderModuleDescriptor moduleDesc = new() { NextInChain = (ChainedStruct*)&wgslDesc };
            module = api.DeviceCreateShaderModule(device, &moduleDesc);
            if (module == null) { return TensorFault.Fail<DeviceKernel>("device-shader", op.Key); }
            ComputePipelineDescriptor pipelineDesc = new() { Layout = null, Compute = new ProgrammableStageDescriptor { Module = module, EntryPoint = (byte*)entry } };
            pipeline = api.DeviceCreateComputePipeline(device, &pipelineDesc);
            if (pipeline == null) { return TensorFault.Fail<DeviceKernel>("device-pipeline", op.Key); }
            layout = api.ComputePipelineGetBindGroupLayout(pipeline, 0);
            if (layout == null) { return TensorFault.Fail<DeviceKernel>("device-layout", op.Key); }
            transferred = true;
            return Fin.Succ(new DeviceKernel(op, (nuint)pipeline, (nuint)layout, (nuint)module));
        }
        catch (Exception ex) { return TensorFault.Fail<DeviceKernel>("device-compile", op.Key, ex.Message); }
        finally {
            if (!transferred) {
                if (layout != null) { api.BindGroupLayoutRelease(layout); }
                if (pipeline != null) { api.ComputePipelineRelease(pipeline); }
                if (module != null) { api.ShaderModuleRelease(module); }
            }
            if (code != 0) { SilkMarshal.Free(code); }
            if (entry != 0) { SilkMarshal.Free(entry); }
        }
    }

    internal void Release(DeviceKernel kernel) {
        api.BindGroupLayoutRelease((BindGroupLayout*)kernel.BindGroupLayout);
        api.ComputePipelineRelease((ComputePipeline*)kernel.Pipeline);
        api.ShaderModuleRelease((ShaderModule*)kernel.ShaderModule);
    }

    internal Fin<Duration> RecordAndSubmit(DevicePlan plan, ReadOnlySpan<DeviceBuffer> roster) {
        int steps = plan.Steps.Count;
        if (steps == 0) { return TensorFault.Fail<Duration>("empty-plan", "device"); }
        if (steps > MaxPlanSteps) { return TensorFault.Fail<Duration>("device-plan-bounds", $"steps={steps}>{MaxPlanSteps}"); }
        int maxBindings = plan.Steps.Fold(0, static (peak, step) => Math.Max(peak, step.Bindings.Length));
        if (maxBindings > MaxStepBindings) { return TensorFault.Fail<Duration>("device-plan-bounds", $"bindings={maxBindings}>{MaxStepBindings}"); }
        Span<BindGroupEntry> entries = stackalloc BindGroupEntry[maxBindings];
        Span<nint> groups = stackalloc nint[steps];
        groups.Clear();
        QuerySet* timestamps = null;
        Buffer* resolve = null;
        Buffer* readback = null;
        CommandEncoder* encoder = null;
        CommandBuffer* commands = null;
        bool mapped = false;
        try {
            QuerySetDescriptor querySetDesc = new() { Type = QueryType.Timestamp, Count = checked((uint)(2 * steps)) };
            timestamps = api.DeviceCreateQuerySet(device, &querySetDesc);
            BufferDescriptor resolveDesc = new() { Size = checked((ulong)(2 * steps * sizeof(ulong))), Usage = BufferUsage.QueryResolve | BufferUsage.CopySrc };
            BufferDescriptor readbackDesc = new() { Size = checked((ulong)(2 * steps * sizeof(ulong))), Usage = BufferUsage.MapRead | BufferUsage.CopyDst };
            resolve = api.DeviceCreateBuffer(device, &resolveDesc);
            readback = api.DeviceCreateBuffer(device, &readbackDesc);
            encoder = api.DeviceCreateCommandEncoder(device, null);
            if (timestamps == null || resolve == null || readback == null || encoder == null) { return TensorFault.Fail<Duration>("device-resource", "timestamp-readback"); }
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
                if (group == null) { return TensorFault.Fail<Duration>("device-resource", step.Kernel.Op.Key); }
                groups[index] = (nint)group;
                ComputePassTimestampWrites timestampWrites = new() { QuerySet = timestamps, BeginningOfPassWriteIndex = (uint)(2 * index), EndOfPassWriteIndex = (uint)(2 * index + 1) };
                ComputePassDescriptor passDesc = new() { TimestampWrites = &timestampWrites };
                ComputePassEncoder* pass = api.CommandEncoderBeginComputePass(encoder, &passDesc);
                if (pass == null) { return TensorFault.Fail<Duration>("device-resource", "compute-pass"); }
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
            if (commands == null) { return TensorFault.Fail<Duration>("device-resource", "command-buffer"); }
            ulong submission = ext.QueueSubmitForIndex(queue, 1, &commands);
            WrappedSubmissionIndex wait = new() { Queue = queue, SubmissionIndex = submission };
            ext.DevicePoll(device, true, &wait);
            api.BufferMapAsync(readback, MapMode.Read, 0, (nuint)byteCount, MapNoop, null);
            ext.DevicePoll(device, true, null);
            if (api.BufferGetMapState(readback) != BufferMapState.Mapped) { return TensorFault.Fail<Duration>("device-map", "readback"); }
            mapped = true;
            ulong* ticks = (ulong*)api.BufferGetMappedRange(readback, 0, (nuint)byteCount);
            if (ticks == null) { return TensorFault.Fail<Duration>("device-map", "range"); }
            return Fin.Succ(Duration.FromNanoseconds(checked((long)(ticks[(2 * steps) - 1] - ticks[0]))));
        }
        catch (Exception ex) { return TensorFault.Fail<Duration>("device-submit", "native", ex.Message); }
        finally {
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
    // Device rows cache one grounded WGSL pipeline per `(device identity, op)`; CPU tables stay closed.
    static readonly FrozenDictionary<TensorOpFamily, string> Wgsl = new Dictionary<TensorOpFamily, string> {
        [TensorOpFamily.MatMul] = WgslSource.TiledGemm,
        [TensorOpFamily.Conv2D] = WgslSource.Im2Col,
        [TensorOpFamily.MaxPool] = WgslSource.StridedWindowMax, [TensorOpFamily.AvgPool] = WgslSource.StridedWindowAvg,
    }.ToFrozenDictionary();

    static readonly ConcurrentDictionary<(string Device, TensorOpFamily Op), Lazy<Fin<DeviceKernel>>> Compiled = new();

    public static Fin<DeviceKernel> Compile(WgpuDevice device, TensorOpFamily row) =>
        Wgsl.TryGetValue(row, out string? source)
            ? Compiled.GetOrAdd((device.Identity, row), key => new Lazy<Fin<DeviceKernel>>(
                () => device.Build(key.Op, source!), LazyThreadSafetyMode.ExecutionAndPublication)).Value
            : Fin.Fail<DeviceKernel>(ComputeFault.Create($"<device-kernel-miss:{row.Key}>"));

    public static void Release(WgpuDevice device) {
        foreach (var pair in Compiled.Where(pair => pair.Key.Device == device.Identity).ToArray()) {
            if (Compiled.TryRemove(pair.Key, out Lazy<Fin<DeviceKernel>>? compiled) && compiled.IsValueCreated && compiled.Value.Case is DeviceKernel kernel) {
                device.Release(kernel);
            }
        }
    }
}

public static class DeviceDispatch {
    // A span operand cannot cross into the receipt lambda, so element and workgroup facts precompute; the
    // singular dispatch is a one-step plan through DevicePlan.Of — one entrypoint owns both modalities.
    public static Fin<ComputeReceipt.TensorRun> Dispatch(WgpuDevice device, DevicePlan plan, ReadOnlySpan<DeviceBuffer> roster, OrtResidency residency, CorrelationId correlation) {
        if (plan.Steps.IsEmpty) { return TensorFault.Fail<ComputeReceipt.TensorRun>("empty-plan", "device"); }
        if (!residency.Device) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-residency-required", plan.Steps[0].Kernel.Op.Key); }
        long workgroups = 0;
        foreach (DeviceStep step in plan.Steps) {
            if (step.Bindings.IsDefaultOrEmpty) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-bindings-empty", step.Kernel.Op.Key); }
            if (step.Workgroups.X == 0 || step.Workgroups.Y == 0 || step.Workgroups.Z == 0) {
                return TensorFault.Fail<ComputeReceipt.TensorRun>("device-workgroups", step.Kernel.Op.Key, $"{step.Workgroups.X}x{step.Workgroups.Y}x{step.Workgroups.Z}");
            }
            foreach (int binding in step.Bindings) {
                if (binding < 0 || binding >= roster.Length) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-binding-range", step.Kernel.Op.Key, $"{binding}/{roster.Length}"); }
                if (roster[binding].ByteLength <= 0 || !roster[binding].Residency.Device) {
                    return TensorFault.Fail<ComputeReceipt.TensorRun>("device-buffer-residency", step.Kernel.Op.Key, binding.ToString());
                }
            }
            try { workgroups = checked(workgroups + checked((long)step.Workgroups.X * step.Workgroups.Y * step.Workgroups.Z)); }
            catch (OverflowException) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-workgroup-overflow", step.Kernel.Op.Key); }
        }
        if (workgroups > int.MaxValue) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-workgroup-overflow", plan.Steps[0].Kernel.Op.Key); }
        DeviceStep terminalStep = plan.Steps[plan.Steps.Count - 1];
        TensorOpFamily terminal = terminalStep.Kernel.Op;
        long outputBytes = roster[terminalStep.Bindings[^1]].ByteLength;
        if (outputBytes % sizeof(float) != 0) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-output-alignment", terminal.Key, outputBytes.ToString()); }
        long elements = outputBytes / sizeof(float);
        int partitions = (int)workgroups;
        Fin<Duration> run;
        try { run = device.RecordAndSubmit(plan, roster); }
        catch (Exception ex) { return TensorFault.Fail<ComputeReceipt.TensorRun>("device-submit", terminal.Key, ex.Message); }
        return run
            .Map(elapsed => new ComputeReceipt.TensorRun(terminal, "float32", elements, SimdWidth: "device-wgpu", Partitions: partitions) {
                Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.DeviceWgpu, AllocationClass = AllocationClass.DeviceWgpu,
                Elapsed = elapsed, DeterminismTag = $"device-wgpu:{device.Identity}",
            });
    }

}
```

## [05]-[RESEARCH]

- [OPERATOR_BACKLOG]: `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude. `ConvertToInteger`/`ConvertToIntegerNative` are conversion rows whose `ConvertKernel<TFrom, TTo>` instantiation is the integer-destination `ConvertKernels<TFrom, int>`/`<TFrom, long>` row, reached only behind a `TensorDtype.Quantized` admission, never a bare float-to-int loop.
- [PARTITION_CLAIM]: `BenchmarkRow.Route` remains the live-fingerprint gate for `ParallelHelper` partitioning; an absent winning row selects inline execution.
- [DEVICE_RESIDENCY]: `DeviceDispatch` is grounded through pipeline compilation, plan recording, timestamp readback, and native cleanup. Live device limits, error scopes, remaining shader rows, and WGPU↔ORT buffer-pointer interop remain open host-bound leaves.
- [DDG_ADJOINT]: `GeometryAdjoint.ProveAdjoint` certifies each recorded DEC operator through `⟨A·x,y⟩ = ⟨x,Aᵀ·y⟩`; new operators extend the closed vocabulary and `GeometryAdjoint.Rows` together.
- [GAUSS_NEWTON_AND_COLORING]: `SensitivityLaw.GaussNewton` and `JacobianColoring` settle first-order matrix-free curvature and sparse recovery; exact tensor-tape Hessian-vector products still require flowing activations and an `f''` column.
