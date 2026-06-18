# [COMPUTE_DISPATCH]

The cpu-tensor kernel dispatch and the equivalence law: the arity kernel-delegate dispatch binding each `TensorOpFamily` row to its TensorPrimitives member, the claim-gated `ParallelHelper` partition column over both the 1-D `For` and the 2-D `For2D` plane partitions, and the equivalence law proving lane kernels against Rasm baselines with the differentiable-operator adjoint and the three-named copy-point law. The page owns the kernel-delegate types and registries, the `TensorOps` dispatch surface, the `PartitionRoute`/`MapBlock`/`PlaneBlock` partition composition, the `EquivalencePolicy`/`AdjointMode`/`DifferentiableOp`/`SensitivityLaw`/`EquivalenceLaw` equivalence surface, and the `StagePlane`/`MatMulGeometry`/`Backward` copy-point and adjoint owners; the kernels ride `System.Numerics.Tensors`, the matrix rows lower through `numeric#KERNEL_LOWERING`, the partition cap reads AppHost `CpuBudget` and the winning `BenchmarkRow`, and the `TensorOpFamily`/`ToleranceClass`/`TensorDtype`/`TensorFault`/`TensorKeyPolicy` vocabulary, the `OrtResidency`/`CopyPoint` copy points, and the `Effects`/`Projection` scalar folds arrive settled from `vocabulary#TENSOR_VOCABULARY` and `residency#ORT_BRIDGE`. The reverse-mode `SensitivityLaw.Chain` is the tape the `optimizer#OPTIMIZER_LANE` gradient-adjoint row consumes.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                                         |
| :-----: | ------------------- | --------------------------------------------------------------------------------------------- |
|   [1]   | KERNEL_DISPATCH     | Arity kernel-delegate tables; one TensorOps dispatch surface                                   |
|   [2]   | EQUIVALENCE_INTEROP | Equivalence proofs against Rasm kernels; matmul route; differentiable adjoint; copy-point law |

## [2]-[KERNEL_DISPATCH]

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
public static class Effects {
    public static Fin<Unit> ToFin(Action kernel) { kernel(); return Fin.Succ(unit); }
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

    public static Fin<T> Fold<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Fold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<T>(row);
    public static Fin<T> FoldPair<T>(TensorOpFamily row, ReadOnlySpan<T> x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.PairFold.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x, y)) : Miss<T>(row);
    public static Fin<int> IndexOf<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T> =>
        TensorKernels<T>.Index.GetValueOrDefault(row) is { } kernel ? Fin.Succ(kernel(x)) : Miss<int>(row);
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
    public static Fin<Tensor<T>> Mask<T>(TensorOpFamily row, Tensor<T> destination, in ReadOnlyTensorSpan<bool> filter, in ReadOnlyTensorSpan<T> values) where T : unmanaged {
        if (row != TensorOpFamily.MaskedWrite) { return Miss<Tensor<T>>(row); }
        TensorSpan<T> span = destination.AsTensorSpan();
        Tensor.FilteredUpdate(span, filter, values);
        return Fin.Succ(destination);
    }
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
```

## [3]-[EQUIVALENCE_INTEROP]

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

public static class StagePlane {
    public static (Span2D<float> Plane, CopyPoint Point) Stage(MemoryOwner<float> backing, int rows, int columns, ClockPolicy clocks, CorrelationId correlation) =>
        (backing.Memory.Span.AsSpan2D(rows, columns), new CopyPoint(OrtResidency.SpanView, (long)rows * columns * sizeof(float), "cpu", clocks.Now, correlation));
}

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
public static class Backward {
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

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), (grad, step) => grad.Bind(g => Adjoint(step.Op, AdjointMode.Reverse, step.Primal, g)));
}

public static class EquivalenceLaw {
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
        double scale = Math.Max(1.0, double.Abs(TensorPrimitives.MaxMagnitude(b)));
        double deviation = TensorPrimitives.Max(gap) / scale;
        return new(policy.Family, deviation, policy.Family.Tolerance.RelativeBound, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
    }
}
```

## [4]-[RESEARCH]

- [OPERATOR_BACKLOG]: `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude. `ConvertToInteger`/`ConvertToIntegerNative` are conversion rows whose `ConvertKernel<TFrom, TTo>` instantiation is the integer-destination `ConvertKernels<TFrom, int>`/`<TFrom, long>` row, reached only behind a `TensorDtype.Quantized` admission, never a bare float-to-int loop.
- [PARTITION_CLAIM]: the fingerprint-matched `BenchmarkClaim` that gates the `ParallelHelper` partition route over the lowered `numeric#KERNEL_LOWERING` GEMM resolves against a live host fingerprint; the in-page partition fence reads the threaded claim's `Route` column, and the cold start is the unpartitioned GEMM until a winning claim row lands.
- [DEVICE_RESIDENCY]: the `DeviceResident` row's `OrtEnv.CreateSharedAllocator` device-memory allocation and the `BoundFlow` `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` stream-fence latency on the live CoreML/GPU rows resolve against a device host; the CPU residency rows (`ManagedSpan`/`MemoryBacked`/`OutputValue`/`SpanView`) and the IEEE-754/quantized egress polarities are the proved terminal, and the device fence is a CPU no-op until a device host runs it.
- [DDG_ADJOINT]: the non-diagonal `Backward.MatMul`/`Backward.SoftMax` operand-transpose/Jacobian-minus-outer spellings ground against the `KernelLowering.Lower` matmul geometry, and the differentiable-geometry operator adjoints — the reverse-mode VJP of the DDG Laplacian, heat-flow, and spectral operators and the remeshing-step adjoint — compose the `Rasm`/Vectors operator kernel's forward primitives and ground against the `Rasm`/Vectors operator member surface (the `Rasm`/Vectors operator kernel owns the forward operator and adjoint-coefficient derivation, this lane owns the reverse-mode tape chain the `optimizer#OPTIMIZER_LANE` gradient-adjoint row consumes for shape optimization and inverse design). A non-diagonal operator carries no forward-mode JVP until its forward Jacobian map is grounded, so `AdjointMode.Forward` on `MatMul`/`SoftMax` faults rather than fabricating a gradient; the coverage closes by one `DifferentiableOp` row per geometry operator once the adjoint-coefficient member surface is grounded.
```
