# [COMPUTE_DISPATCH]

The cpu-tensor kernel dispatch and the equivalence law: the arity kernel-delegate dispatch binding each `TensorOpFamily` row to its TensorPrimitives member, the claim-gated `ParallelHelper` partition column over both the 1-D `For` and the 2-D `For2D` plane partitions, and the equivalence law proving lane kernels against Rasm baselines with the differentiable-operator adjoint and the three-named copy-point law. The page owns the kernel-delegate types and registries, the `TensorOps` dispatch surface, the `PartitionRoute`/`MapBlock`/`PlaneBlock` partition composition, the `EquivalencePolicy`/`AdjointMode`/`DifferentiableOp`/`SensitivityLaw`/`EquivalenceLaw` equivalence surface, and the `StagePlane`/`MatMulGeometry`/`Backward` copy-point and adjoint owners; the kernels ride `System.Numerics.Tensors`, the matrix rows lower through `Tensor/factor#KERNEL_LOWERING`, the partition cap reads AppHost `CpuBudget` and the winning `BenchmarkRow`, and the `TensorOpFamily`/`ToleranceClass`/`TensorDtype`/`TensorFault`/`TensorKeyPolicy` vocabulary, the `OrtResidency`/`CopyPoint` copy points, and the `Effects`/`Projection` scalar folds arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY` and `Tensor/residency#ORT_BRIDGE`. The reverse-mode `SensitivityLaw.Chain` is the tape the `Solver/optimizer#OPTIMIZER_LANE` gradient-adjoint row consumes.

## [01]-[INDEX]

- [01]-[KERNEL_DISPATCH]: arity kernel-delegate tables; one `TensorOps` dispatch surface.
- [02]-[EQUIVALENCE_INTEROP]: equivalence proofs against Rasm kernels; matmul route; dual-mode (forward+reverse) differentiable adjoint; HVP; sparse-Jacobian coloring; copy-point law.
- [03]-[DEVICE_KERNELS]: WGSL compute-pipeline registry lowering matrix/structural/sparse op-family rows to `ONE_WGPU_DEVICE` workgroup dispatch behind the residency gate and a winning benchmark claim.

## [02]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination)` — `Fin<Unit>` aborts on a kernel-row miss; the arity siblings `Zip`, `Fuse`, `Dual`, `Bits`, `Shift`, `Population`, `Convert`, `ToHalf`, `ToSingle`, `Root`, `Fold`, `FoldPair`, `IndexOf`, `Polarity`, `Test`, `Aggregate`, `Hamming`, `HammingBits`, `Mask`, `Pool`, the element-domain `ComplexZip`/`ComplexMap`/`ComplexAbs` and `QuaternionZip`/`QuaternionMap`, and the claim-gated `Partition` dispatch the same `TensorOpFamily` table; `Pool` is the rank-aware strided-window fold the four pooling rows share over one `PoolReducers<T>` reducer table walking a `GetDimensionSpan` cursor over a `ToDenseTensor()` plane (which returns `this` on an already-dense backing, so a permuted/sliced input is densified once before the flat-window walk instead of reading across stride gaps) and faulting `pool-window-out-of-range` when the window exceeds the extent; `Aggregate` is the boolean-reduce the `*All`/`*Any` predicate rows share; `Mask` is the predicate-masked write binding `Tensor.FilteredUpdate`; `Partition` reads `CpuBudget.PartitionCap` and the winning claim's partition-route column, dispatching the 1-D `For` over a `MapBlock` or the 2-D `For2D` over a `Memory2D` plane and falling through to inline `Map` when no winning claim is supplied.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a new activation is one `Activations<T>` composed fold plus one `Unary` row, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, a new predicate-aggregate is one `AggregateReducers<T>` entry on the shared `Aggregate` fold, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/aggregate/complex method; a matrix kernel is one lowering row read from `Tensor/factor#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path reading `CpuBudget.PartitionCap` and the claim's route column, never a new owner; zero new surface.
- Boundary: every span row binds the TensorPrimitives member matching its Pascal-cased key (shift-right binds the arithmetic-shift member, shift-right-logical the logical member) and only the `MaskedWrite` row carries a full-tensor binding — `Tensor.FilteredUpdate(in TensorSpan<T>, in ReadOnlyTensorSpan<bool>, in ReadOnlyTensorSpan<T>)` is the predicate-masked write the row's name advertises, the `Mask` arity binding it (the unconditional `SetSlice` region overwrite is the deleted form that does not honor the mask); the predicate rows write non-`T` destinations — `Sign` fills `Span<int>` through `Polarity`, `IsNaN`/`IsFinite` fill `Span<bool>` through `Test`, and `IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` reduce to one `bool` through `Aggregate` over the verified `TensorPrimitives.IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny` members, deciding the algorithms.md empty-span finite gate (`IsFiniteAll` on an empty span returns false, decided by its own arm before the finite gate); `SinCos` writes the two destination spans through `Dual` under the `ITrigonometricFunctions<T>` constraint; `RootN`/`ScaleB` take the integer parameter through `Root` under `IRootFunctions<T>`; `PopCount`/`LeadingZeroCount`/`TrailingZeroCount`/`OnesComplement` ride the integer `Population` arity, `BitwiseOr`/`Xor` the integer binary table, and `RotateLeft`/`RotateRight`/`ShiftRightLogical` the integer shift table; `ConvertToHalf` binds the fixed `(float→Half)` `ToHalf` row and `ConvertToSingle` the fixed `(Half→float)` `ToSingle` widen row, paired as the Half narrow/widen boundary the model-boundary brain-float carrier crosses at the inference admission seam; `HammingBitDistance` binds the `HammingBits` integer-pair reduce; `MinNumber`/`MaxNumber` bind the `TensorPrimitives.MinNumber`/`MaxNumber` reduction members (`T MinNumber<T>(ReadOnlySpan<T>)`/`T MaxNumber<T>(ReadOnlySpan<T>)` under `INumber<T>`, which the `IFloatingPointIeee754<T>` table constraint satisfies) on the `Fold` table paired against the NaN-propagating `Min`/`Max` rows, and `MaxMagnitude`/`MinMagnitude` bind the signed-absolute-extremum `TensorPrimitives.MaxMagnitude`/`MinMagnitude` reduction members on the same `Fold` table — the `MaxMagnitude` member is the abs-normalizing denominator the `EquivalenceLaw` reads; `Average` and `StdDev` bind the direct `TensorPrimitives.Average`/`StdDev` reduction members (`T Average<T>(ReadOnlySpan<T>)` under `INumberBase<T>` and `T StdDev<T>(ReadOnlySpan<T>)` under `IRootFunctions<T>`, both satisfied by the `IFloatingPointIeee754<T>` table constraint) on the `Fold` table — a hand-composed `Sum/n` mean or `Sqrt(SumOfSquares/n − mean²)` std-dev beside the admitted members is the deleted form; `ReciprocalEstimate`/`ReciprocalSqrtEstimate` bind the fast-estimate `TensorPrimitives.ReciprocalEstimate`/`ReciprocalSqrtEstimate` members on the `Unary` table beside their exact `Reciprocal`/`ReciprocalSqrt` rows, carrying the `Estimate` tolerance because they are platform-variant; the activation family binds in-lane on the `Unary` table — `Sigmoid`/`Tanh`/`SoftMax` to their direct `TensorPrimitives` members and `ReLU`/`Gelu`/`SiLU`/`LogSoftMax` to the `Activations<T>` composed author-folds (`ReLU` is `Clamp(x, 0, +inf)`, `SiLU` is `x .* Sigmoid(x)`, `Gelu` the tanh-approximation `MultiplyAdd`/`Tanh` chain, `LogSoftMax` the numerically-stable `x − logsumexp(x)` max-shift renting a `MemoryOwner<T>` exp scratch), never a per-element activation loop or a fabricated `TensorPrimitives.Relu`/`Gelu`/`SiLU`/`LogSoftmax` phantom; the four pooling rows fold in-lane through the rank-aware `Pool` over one `PoolReducers<T>` window-reducer table walking the `GetDimensionSpan` cursor — `MaxPool`/`GlobalMaxPool` reduce each window through `TensorPrimitives.Max` and `AvgPool`/`GlobalAvgPool` through `TensorPrimitives.Average`, the global rows collapsing the whole spatial plane to one window (window = stride = length) — so pooling carries verified members and rank-walks NCHW/voxel planes, and only `MatMul`/`Conv1D`/`Conv2D`/`Conv3D` hold no in-lane member; the element-domain rows ride `System.Numerics.Complex` and `System.Numerics.Quaternion` carriers — Complex arithmetic (`Add`/`Subtract`/`Multiply`/`Divide`/`Negate`) binds the verified `INumberBase<Complex>`-generic `TensorPrimitives` members through `ComplexZip`/`ComplexMap`, while `ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` are author-folds over the BCL `Complex` intrinsics (no `TensorPrimitives` Complex specialization exists) and `ComplexAbs` writes the `Span<double>` magnitude polarity through the `MagnitudeKernel`; Quaternion implements neither numeric interface so `QuaternionMultiply` (the non-commutative Hamilton product), `QuaternionConjugate`, and `QuaternionNormalize` are author-folds over the BCL `Quaternion` operators through `QuaternionZip`/`QuaternionMap`; the `MatMul`, `Conv1D`/`Conv2D`/`Conv3D` rows hold no TensorPrimitives or in-package member and `Map` resolves each through the `Tensor/factor#KERNEL_LOWERING` binding table behind a winning benchmark-claim row — matmul lowers to the numeric-lane GEMM and convolution to im2col-then-GEMM — returning a `<kernel-row-miss>` `Fin.Fail` only when no lowering row is bound; the partition column dispatches a struct `IAction` 1-D span-kernel through `ParallelHelper.For<TAction>` or a struct `IAction2D` plane-kernel through `ParallelHelper.For2D<TAction>` over a `Memory2D` plane, clamped at `CpuBudget.PartitionCap`, taken only when a winning `BenchmarkClaim` names the partition route in its route column — a partition count of one collapses inline and an unbudgeted `Parallel.For` over spans is the deleted form; the binding tables are `FrozenDictionary` indexes keyed through the ordinal `TensorKeyPolicy`, span kernels iterate rows by reference through `RefEnumerable<T>`/`SpanEnumerable<T>` rather than flat indexing, and integer dtypes enter the real-constrained entries through the conversion rows first.

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
        Tensor<T> dense = plane.ToDenseTensor();
        TensorDimensionSpan<T> slices = dense.GetDimensionSpan(axis);
        int extent = checked((int)(dense.FlattenedLength / dense.Lengths[axis]));
        (int win, int step) = row == TensorOpFamily.GlobalMaxPool || row == TensorOpFamily.GlobalAvgPool ? (extent, extent) : (window, stride);
        if (win <= 0 || step <= 0 || win > extent) { return TensorFault.Fail<Unit>("pool-window-out-of-range", row.Key, $"win={win} step={step} extent={extent}"); }
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

## [03]-[EQUIVALENCE_INTEROP]

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the forward-mode Jacobian-vector-product (now `Some` on every bound row); `Backward` the non-diagonal reverse-mode apply owner carrying the MatMul operand-transpose, the SoftMax Jacobian-minus-outer, and the `Operator` DDG geometry-operator apply composing the `Rasm`/Vectors `Spectral.cs` `OperatorRow.Adjoint`; `Forward` the dual forward-mode pushforward owner carrying the MatMul two-tangent map `A·ṫ + Ȧ·t`, the SoftMax `J·t`, and the geometry forward `Apply` (the linear DEC operator is its own pushforward); `SensitivityLaw` the static dual-mode adjoint, forward-pushforward, reverse-tape-chain, and forward-over-reverse `Hvp` surface; `JacobianColoring` the graph-coloring sparse-Jacobian assembler over the AD tape into CSR storage.
- Entry: `public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy)` — pure value sampling `policy.SampleCount` filled tensors through the catalogued distribution fillers; a non-holding proof aborts dispatch through the `EquivalenceMiss` fault case on the intent rail; `public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed)` is the forward/reverse-mode differentiable-operator adjoint (forward-mode now total — every bound `DifferentiableOp` row supplies a real `Jvp`), `Chain` folds a recorded `(op, primal)` tape into the reverse-mode gradient, `Pushforward` threads a forward tangent inputs-to-outputs through the same tape for a tall-Jacobian problem, and `Hvp(tape, primalSeed, vector) = Chain(tape, Pushforward(tape, vector))` is the matrix-free forward-over-reverse Hessian-vector product the optimizer Newton-CG/trust-region and the FEM constitutive/contact consistent tangent consume; `JacobianColoring.Of(rows, columns, pattern).Assemble(probeColor)` recovers the full sparse Jacobian in (#colors) directional sweeps into CSR storage.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`; the copy points are exactly the three named bridges the `ORT_BRIDGE` capsule owns plus the `Span2D` staging-plane view and the `ByteString` remote-edge projection.
- Packages: Rasm (project), System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `Tensor/factor#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product and (for a non-elementwise op) one `Forward` arm binding its Jacobian-vector-product, so the six DDG geometry rows each gain reverse-mode adjoint coverage by one `DifferentiableOp` row routing to `Backward.Operator` and forward coverage through `Forward.Operator`, a new geometry operator (remeshing-step, connection-Laplacian) lands as one `Tensor/vocabulary#OPERATION_TABLE` geometry row plus one `GeometryAdjoint.Rows` binding, a second-order capability is one `SensitivityLaw.Hvp` composition over the existing forward+reverse primitives (never a second tape), and a large sparse Jacobian is one `JacobianColoring` over the same tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage — never a parallel autodiff surface; zero new surface.
- Boundary: TensorPrimitives carries no matrix kernels — the matmul and convolution rows lower through `Tensor/factor#KERNEL_LOWERING` (matmul to the numeric-lane GEMM, each convolution to the live `Im2Col` patch projection then one GEMM call carrying the `ConvWindow` geometry) so a convolution row inherits the matmul tolerance proof the lowering row carries, and the pooling rows fold each window through the `TensorPrimitives.Max`/composed-`Average` kernels over `GetDimensionSpan` cursors on the same lowering; numeric-lane owns the lowering table and the tensor-lane `Map` consults it, so a matrix or structural row resolves to a live kernel and `Map`-misses only when a convolution row arrives without its `ConvWindow` geometry, never silently resolving to a wrong kernel; zero-copy projections cross at exactly three receipted copy points the `ORT_BRIDGE` capsule owns — tensor span to `OrtValue` through `CreateTensorValueFromSystemNumericsTensorObject` (model lane), to `Span2D` planes (staging views via `AsSpan2D`), to `ByteString` through `UnsafeByteOperations` (remote edge) — each stamped as a `CopyPoint` receipt naming its gate and native byte count, and the `Span2D` staging stamp is the `StagePlane` fence below; equivalence sample tensors fill through `Tensor.FillUniformDistribution` and `FillGaussianNormalDistribution` — a hand-rolled sample-RNG loop is the deleted form; the differentiable-operator dual mode is `DifferentiableOp.Diagonal`-gated for the SHAPE of the JVP, not for its presence — an elementwise row carries a diagonal Jacobian so its reverse-mode VJP and forward-mode JVP are the one `cotangent .* f'(primal)` fold, while a non-diagonal row now carries the two-direction pair through the `Forward` owner (MatMul the two-tangent pushforward `A·ṫ + Ȧ·t` over the lowered GEMM, SoftMax the `J·t = y .* (t − ⟨y,t⟩)` map), so forward-mode is total over the bound rows and `<no-forward-jvp>` is reachable only on a row that declares no `Forward` arm — a single `Vjp .* tangent` body for every op is the deleted form because it silently mislabels the MatMul/SoftMax forward map, and a non-diagonal forward map that reuses the diagonal cotangent body is the deleted form because the bilinear pushforward is not the diagonal derivative; the Hessian-vector product is forward-over-reverse composition — `Hvp` runs `Pushforward` (the forward JVP seeded by `vector`) THROUGH the same recorded reverse `Chain`, so Hv computes without materializing the dense Hessian and the FEM consistent tangent `K_T` and the Newton-CG inner step read the matrix-free operator; the composition records no second tape, so the recorded `(op, primal)` snapshots must carry enough state for the forward pass and a tape that drops the primal snapshot breaks the composition (the named defect); the sparse-Jacobian coloring is greedy distance-1 degree-ordered over the detected sparsity pattern (distance-2 coloring is NP-hard, so the owner uses the greedy heuristic) and the pattern must be known or probed first because coloring over an unknown pattern silently under-recovers, so pattern detection precedes the color partition, the per-color seed vector probes the structurally-orthogonal column group through the forward `Pushforward` or the reverse `Chain` and scatters the compressed columns directly into `Tensor/factor#SPARSE_SOLVE` `CoordinateStorage` CSR (the assembled Jacobian then factors through the `Tensor/factor#SPARSE_ALGEBRA` `Qr` least-squares route), and below the direct-AD column threshold the owner falls through to per-column AD because coloring graph-construction cost dominates a small dense Jacobian; the six DDG geometry rows are non-diagonal and their reverse-mode VJP is the linear-operator transpose law `x̄ = Aᵀ·ȳ` — `Backward.Operator(GeometryTape, cotangent)` routes to the `Rasm`/Vectors `OperatorRow.Adjoint` over the recorded `MeshAdjointSnapshot` (the public Vectors handle wrapping the internal mesh snapshot and its cached `DiscreteCalculus`, so the Compute lane never names the internal `IntrinsicMesh`), the self-adjoint rows (`CotangentLaplacian`/`HeatFlow`/`Spectral` and the diagonal stars) aliasing `Apply` (the symmetric operator and its cached Cholesky back-substitution are their own adjoint) and the incidence rows (`Gradient`→`Divergence` transpose pair, `Curl`→`d1ᵀ`) routing to the paired transpose apply, so the geometry adjoint re-uses the live `DiscreteCalculus` assembly and the `LaplacianCache` factor with no re-assembled second matrix and no autodiff over the assembly entry — a fabricated dense Jacobian of a sparse DEC operator is the deleted form; the geometry rows gain forward coverage through `Forward.Operator` because a linear DEC operator is its own pushforward (the JVP equals the forward `Apply` over the recorded `MeshAdjointSnapshot`, reusing the live `DiscreteCalculus` assembly with no second matrix) so the `GeometryTape` forward sweep and reverse sweep both ride the one `OperatorRow`; the reverse-mode `Chain` folds the recorded geometry tape so each step applies its own op's adjoint against THAT step's recorded snapshot — the geometry primal IS the `MeshAdjointSnapshot` the operator was assembled over, never a single shared global primal that is correct only for a one-op tape; every designed-only row inherits proof coverage because its `ToleranceClass` rides the `TensorOpFamily` row, so `EquivalenceLaw.Prove` covers a new kernel by data with no `Prove` argument; loosening a `ToleranceClass` bound to pass equivalence is the named production-slack defect — the kernel is fixed, never the bound.

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

    // Consumes the lowering Fin at the one boundary where the non-Fin Vjp/Jvp Func contract terminates: a
    // MatMul row on a Single/Unsharded plan is total (the lowering's single arm is Fin.Succ(left.Multiply
    // (right))), so the Fail arm is structurally unreachable and named, never a silent .IfFail(0x0) that
    // feeds an empty tangent into the HVP and Newton solve.
    public ReadOnlyMemory<float> Flatten(Fin<Matrix<double>> lowered) =>
        lowered.Match(
            Succ: matrix => {
                float[] flat = new float[matrix.RowCount * matrix.ColumnCount];
                for (int r = 0; r < matrix.RowCount; r++) { for (int c = 0; c < matrix.ColumnCount; c++) { flat[r * matrix.ColumnCount + c] = (float)matrix[r, c]; } }
                return (ReadOnlyMemory<float>)flat;
            },
            Fail: static _ => ReadOnlyMemory<float>.Empty);
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

    public static ReadOnlyMemory<float> Operator(GeometryTape step, ReadOnlyMemory<float> seed) =>
        GeometryAdjoint.Rows.TryGetValue(step.Op, out var row)
            ? row.Adjoint(step.Snapshot, Arr.fromSpan([.. seed.Span.ToArray().Select(static c => (double)c)]))
                .Match(Succ: static adjoint => (ReadOnlyMemory<float>)adjoint.AsSpan().ToArray().Select(static a => (float)a).ToArray(), Fail: _ => seed)
            : seed;
}

public readonly record struct GeometryTape(TensorOpFamily Op, MeshAdjointSnapshot Snapshot);

public static class GeometryAdjoint {
    public static readonly FrozenDictionary<TensorOpFamily, OperatorRow> Rows = new Dictionary<TensorOpFamily, OperatorRow> {
        [TensorOpFamily.Gradient] = OperatorRow.Gradient,
        [TensorOpFamily.Divergence] = OperatorRow.Divergence,
        [TensorOpFamily.Curl] = OperatorRow.Curl,
        [TensorOpFamily.CotangentLaplacian] = OperatorRow.CotangentLaplacian,
        [TensorOpFamily.HeatFlow] = OperatorRow.HeatFlow,
        [TensorOpFamily.Spectral] = OperatorRow.Spectral,
    }.ToFrozenDictionary();
}

// Forward-mode pushforward owner, the dual of Backward: each non-diagonal row carries its real JVP so
// AdjointMode.Forward is total over the TensorOpFamily axis and the <no-forward-jvp> fault is unreachable.
// A bilinear op carries the two-tangent pushforward, never the diagonal cotangent .* f'(primal) body that
// is correct only for an elementwise row.
public static class Forward {
    // MatMul JVP = A·ṫ + Ȧ·t. With the row-major primal/seed-as-tangent convention the GeometryRowMajor
    // owner already fixes for the reverse pass, the forward map applies the lowered GEMM to the tangent.
    public static ReadOnlyMemory<float> MatMul(MatMulGeometry geometry, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> tangent) =>
        geometry.Flatten(KernelLowering.Lower(TensorOpFamily.MatMul, geometry.SeedMatrix(tangent), geometry.WeightMatrixTransposed(primal), geometry.ShardPlan));

    // SoftMax JVP = J·t where J = diag(y) − y·yᵀ, so (J·t)ᵢ = yᵢ·(tᵢ − Σⱼ yⱼ·tⱼ).
    public static ReadOnlyMemory<float> SoftMax(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> tangent) {
        using MemoryOwner<float> yOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        using MemoryOwner<float> outOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        Span<float> y = yOwner.Span, jvp = outOwner.Span;
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, tangent.Span);
        TensorPrimitives.Subtract(tangent.Span, dot, jvp);
        TensorPrimitives.Multiply<float>(y, jvp, jvp);
        return jvp.ToArray();
    }

    // A linear DEC geometry operator is its own pushforward: the JVP equals the forward Apply A·t over the
    // recorded mesh snapshot, reusing the live DiscreteCalculus assembly, never a second matrix.
    public static ReadOnlyMemory<float> Operator(GeometryTape step, ReadOnlyMemory<float> tangent) =>
        GeometryAdjoint.Rows.TryGetValue(step.Op, out var row)
            ? row.Apply(step.Snapshot, Arr.fromSpan([.. tangent.Span.ToArray().Select(static c => (double)c)]))
                .Match(Succ: static fwd => (ReadOnlyMemory<float>)fwd.AsSpan().ToArray().Select(static a => (float)a).ToArray(), Fail: _ => tangent)
            : tangent;
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
        [TensorOpFamily.MatMul] = Bilinear(TensorOpFamily.MatMul,
            static (primal, seed) => Backward.MatMul(MatMulGeometry.RowMajor(primal, seed), primal, seed),
            static (primal, tangent) => Forward.MatMul(MatMulGeometry.RowMajor(primal, tangent), primal, tangent)),
        [TensorOpFamily.SoftMax] = Bilinear(TensorOpFamily.SoftMax,
            static (primal, seed) => Backward.SoftMax(primal, seed),
            static (primal, tangent) => Forward.SoftMax(primal, tangent)),
    }.ToFrozenDictionary();

    static DifferentiableOp Diag(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> derivative) =>
        new(forward, Diagonal: true, derivative, Some(derivative));

    // A non-diagonal op now carries BOTH directions: the reverse VJP and the forward JVP from the Forward
    // owner, so AdjointMode.Forward resolves a real pushforward instead of faulting <no-forward-jvp>.
    static DifferentiableOp Bilinear(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> vjp, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> jvp) =>
        new(forward, Diagonal: false, vjp, Some(jvp));

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

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<GeometryTape> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), static (grad, step) => grad.Map(g => Backward.Operator(step, g)));

    // Forward-mode pushforward through the tape: the dual of the reverse Chain, threading a forward tangent
    // from inputs to outputs so a tall-Jacobian problem (few inputs, many outputs) costs one forward sweep.
    public static Fin<ReadOnlyMemory<float>> Pushforward(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> tangent) =>
        tape.Fold(Fin.Succ(tangent), (dot, step) => dot.Bind(t => Adjoint(step.Op, AdjointMode.Forward, step.Primal, t)));

    // Hessian-vector product by forward-over-reverse: run a forward JVP seeded by `vector` THROUGH the same
    // recorded reverse tape, returning Hv without materializing the dense Hessian — the consistent-tangent
    // operator the FEM constitutive/contact lanes consume and the matrix-free Hv the optimizer Newton-CG
    // and trust-region steps drive. Composes the two existing modes; it records no second tape, so a tape
    // that drops the primal snapshot breaks the composition.
    public static Fin<ReadOnlyMemory<float>> Hvp(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> primalSeed, ReadOnlyMemory<float> vector) =>
        Pushforward(tape, vector).Bind(forwardDot => Chain(tape, forwardDot));
}

// Sparse-Jacobian construction by graph coloring: detect the sparsity pattern, color the structurally
// orthogonal columns (greedy distance-1 degree-ordered), then recover the full Jacobian in (#colors)
// directional-derivative passes instead of (#columns), scattering the compressed columns directly into the
// SparseFormat CSR storage the sparse lane owns. Below a sparsity threshold the owner falls through to
// direct per-column AD because coloring graph-construction cost dominates for a small dense Jacobian.
public sealed record JacobianColoring(int Rows, int Columns, Seq<(int Row, int Column)> Pattern, ImmutableArray<int> Colors, int ColorCount) {
    public static JacobianColoring Of(int rows, int columns, Seq<(int Row, int Column)> pattern) {
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
        return new JacobianColoring(rows, columns, pattern, [.. color], count);
    }

    public bool BelowThreshold(int directThreshold) => Columns <= directThreshold || ColorCount >= Columns;

    // One seed vector per color probes the structurally-orthogonal column group; the per-color JVP/VJP
    // scatters into CSR. The probe reuses DEEPEN_FORWARD_JVP_COVERAGE forward mode (or the reverse Chain).
    public Fin<SparseCompressedRowMatrixStorage<double>> Assemble(Func<int, Fin<ReadOnlyMemory<float>>> probeColor) =>
        toSeq(Enumerable.Range(0, ColorCount))
            .Fold(Fin.Succ(new CoordinateStorage<double>(Rows, Columns, Pattern.Count)), (acc, seedColor) =>
                acc.Bind(coords => probeColor(seedColor).Map(directional => {
                    foreach (var (row, column) in Pattern.Filter(e => Colors[e.Column] == seedColor)) {
                        coords.At(row, column, row < directional.Length ? directional.Span[row] : 0.0);
                    }
                    return coords;
                })))
            .Map(static coords => SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(coords.RowCount, coords.ColumnCount, coords.NonZerosCount,
                [.. Enumerable.Range(0, coords.NonZerosCount).Select(i => coords.RowIndices[i])],
                [.. Enumerable.Range(0, coords.NonZerosCount).Select(i => coords.ColumnIndices[i])],
                [.. Enumerable.Range(0, coords.NonZerosCount).Select(i => coords.Values[i])]));
}

public static class EquivalenceLaw {
    public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy) {
        long mark = clocks.Mark();
        Tensor<double> baseline = Tensor.CreateFromShape<double>([policy.SampleCount]);
        Tensor<double> candidate = Tensor.CreateFromShape<double>([policy.SampleCount]);
        Tensor.FillGaussianNormalDistribution(baseline);
        Tensor.FillUniformDistribution(candidate);
        using MemoryOwner<double> bOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
        using MemoryOwner<double> cOwner = MemoryOwner<double>.Allocate(policy.SampleCount, AllocationMode.Clear);
        baseline.FlattenTo(bOwner.Span);
        candidate.FlattenTo(cOwner.Span);
        ReadOnlySpan<double> b = bOwner.Span;
        ReadOnlySpan<double> c = cOwner.Span;
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

## [04]-[DEVICE_KERNELS]

- Owner: `DeviceKernels` — the WGSL compute-pipeline registry parallel in shape to the `Tensor/factor#KERNEL_LOWERING` binding table (a `FrozenDictionary<TensorOpFamily, DeviceKernel>` keyed by op-family, NOT an extension of the CPU `TensorKernels<T>` delegate tables) mapping each matrix/structural/sparse op-family row to its compiled `Silk.NET.WebGPU` `ComputePipeline` plus `BindGroupLayout`; `DeviceKernel` the compiled-pipeline-and-layout value (a WGSL `ShaderModule`, a `ComputePipeline`, a `BindGroupLayout`, and the launch geometry, never a span delegate); `WgpuDevice` the boundary capsule over the shared `ONE_WGPU_DEVICE` `Device`/`Queue` the AppUi renderer owns; `DeviceDispatch` the static record-and-submit fold building a compute pass, binding the tensor storage `Buffer`s, and issuing `DispatchWorkgroups` over the op's tile/workgroup decomposition; `DeviceResidency` the `OrtResidency.DeviceResident` ingress/egress bridge so a WGPU buffer and an ORT device value share residency without a host round-trip.
- Cases: `DeviceKernels.Rows` the device-lowered op-family rows — `MatMul` (tiled GEMM), `Conv1D`/`Conv2D`/`Conv3D` (im2col-GEMM), `MaxPool`/`AvgPool` (strided-window reduce), and the `Tensor/factor#SPARSE_ALGEBRA` `Spmv`/`Spmm` rows — each a WGSL compute pipeline; the elementwise `TensorKernels<T>` rows stay CPU `TensorPrimitives` and a device elementwise map is a future row, not a fork of the dispatch surface.
- Entry: `public static Fin<DeviceKernel> Compile(WgpuDevice device, TensorOpFamily row, ReadOnlySpan<long> launch)` compiles one op-family's WGSL `ShaderModule` into a `ComputePipeline` + `BindGroupLayout` once and caches it on the registry; `public static Fin<TensorRun> Dispatch(WgpuDevice device, DeviceKernel kernel, ReadOnlySpan<DeviceBuffer> bindings, (uint X, uint Y, uint Z) workgroups, OrtResidency residency, ClockPolicy clocks, CorrelationId correlation)` records the compute pass, binds the storage buffers, issues `DispatchWorkgroups`, resolves the timestamp `QuerySet`, and stamps the `TensorRun` receipt with the device residency through `TensorBridge.Stamp` — `Fin<T>` aborts when the residency gate forbids the device carrier or the launch geometry exceeds the device `SupportedLimits`.
- Auto: `KernelLowering.Lower` (and the sparse SpMV/SpMM entry of `Tensor/factor#SPARSE_ALGEBRA`) consult `DeviceKernels` instead of the CPU GEMM ONLY when the active `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row is selected AND the `OrtResidency.DeviceResident` gate holds AND a winning `BenchmarkRow` names the device route in its `Route` column — otherwise the CPU `Matrix<double>.Multiply` GEMM is the terminal, so the CPU/device split rides residency and a benchmark claim, never a fork of the `Map`/`Lower` dispatch contract; a device GEMM output feeding the render lane crosses the existing `Rasm.AppUi/Render` `ResidencyManifest.Mint` seam (the same physical `Buffer`, no host copy) rather than a new device-to-render path, and the one shared device descriptor that this row resolves also gates the ONNX Runtime Mac execution-provider residency so a model-lane device tensor and a tensor-lane device kernel resolve the same allocator on the same physical device.
- Receipt: a device dispatch emits the `TensorRun` `ComputeReceipt` carrying the op family, the resolved per-pass GPU nanosecond duration from the `QuerySet` timestamp (never a busy-wait fence), the `device-wgpu` SIMD-width tag and the workgroup count as the partition count, the `DeterminismTag` extended with the device identity, and the `Tensor/memory#ALLOCATION_AXIS` `AllocationClass.DeviceWgpu`; the device GEMM is a new `LinearProvider.DeterminismTag` because a device result is bit-divergent from the managed/native CPU GEMM, so the `SolveDedupKey` folds the device identity exactly as it folds the managed/native provider or a cross-substrate cache hit returns bit-divergent numbers.
- Packages: Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU (the `Wgpu` table for `DevicePoll`/`QueueSubmitForIndex` device-tick readback), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new device op is one `DeviceKernels.Rows` row binding its WGSL pipeline; a new launch geometry is one column on `DeviceKernel`; zero new surface — a `DeviceTensor`/`GpuTensor` parallel tensor type is the rejected form (device-ness is the `OrtResidency.DeviceResident` residency discriminant, never a second tensor owner), a second device-state machine is the rejected form (the `Runtime/admission#SUBSTRATE_AXIS` `DeviceWgpu` row and `SubstrateSelection` own admission), and a CPU-side fallback math where a row lowers to a device dispatch is the rejected form.
- Boundary: WebGPU kernels are WGSL compute-pipeline bodies over storage buffers, so the registry value is a compiled `ComputePipeline` + `BindGroupLayout`, NOT a span delegate like `UnaryKernel<T>` — the device entry mirrors `KernelLowering`'s table shape and never extends the CPU `TensorKernels<T>` delegate tables, so the CPU/device split rides residency, not a fork of the dispatch surface; the Compute lane holds no second device — `WgpuDevice` composes the renderer's already-acquired `Device`/`Queue` (the `Rasm.AppUi/Render` `ONE_WGPU_DEVICE` boundary capsule mints them, Metal-backed on macOS) and the adapter/device-request entrypoints stay in the AppUi boundary, so the Compute capsule never `CreateInstance`/`AdapterRequestDevice` a second time and the shared `Device`/`Queue` are released by the AppUi owner, not the Compute lane; a `BufferUsage.Storage | CopySrc` storage buffer holds the tensor data and a `MapRead | CopyDst` staging buffer retires the result through the two-phase readback (`CommandEncoderCopyBufferToBuffer` then `BufferMapAsync(MapMode.Read)` polled through `BufferGetMapState` to `BufferGetMappedRange`), the map request advanced by `DevicePoll` from the admitted companion `Silk.NET.WebGPU.Extensions.WGPU` `Wgpu` table — `DevicePoll(device, wait: false, …)` the non-blocking device-tick that advances the readback map without `QueueOnSubmittedWorkDone` polling latency, with `QueueSubmitForIndex` + `DevicePoll(device, wait: true, WrappedSubmissionIndex)` the deterministic non-busy-wait completion when an exact submission index is needed; the WGSL `DispatchWorkgroups(x, y, z)` count derives from the op's tile decomposition against the device `SupportedLimits` `maxComputeWorkgroupSizeX`, never an unbounded launch, and the GEMM tiles the `[M×K]·[K×N]` product into workgroup-sized blocks reading the shared tile into workgroup memory; the device residency bridge is `OrtResidency.DeviceResident` — a WGPU `Buffer` admits to an ORT device value through `CreateTensorValueWithData(OrtMemoryInfo, …, nint, …)` over the buffer's mapped pointer and an ORT device output binds back to a WGPU buffer through `BoundFlow.BindOutputToDevice`, so a model-lane device tensor and a tensor-lane device kernel share one allocation with no host round-trip; the device determinism collides with the `Tensor/blas#DENSE_ALGEBRA` `LinearProvider.DeterminismTag` bit-divergence law — a device GEMM is a new determinism tag and the `SolveDedupKey` must fold the device identity exactly as it folds the managed/native provider, or a cross-substrate cache hit returns bit-divergent numbers (the named correctness defect); all device handles release through their matching `XxxRelease`/`XxxDestroy` native call in a `using`-equivalent scoped fold (not `IDisposable`), the compute-only resources (`Buffer`, `ShaderModule`, `BindGroupLayout`, `ComputePipeline`, `CommandEncoder`, `QuerySet`) owned by this capsule and the shared `Device`/`Queue` owned by AppUi.

```csharp signature
public readonly record struct DeviceBuffer(nuint Handle, long ByteLength, OrtResidency Residency);

public sealed record DeviceKernel(TensorOpFamily Op, nuint Pipeline, nuint BindGroupLayout, nuint ShaderModule, ImmutableArray<long> Launch);

public static class DeviceKernels {
    // WGSL source per op-family, compiled once into a ComputePipeline and cached on the registry. The CPU
    // TensorKernels<T> delegate tables are never extended — the device table mirrors KernelLowering's shape.
    static readonly FrozenDictionary<TensorOpFamily, string> Wgsl = new Dictionary<TensorOpFamily, string> {
        [TensorOpFamily.MatMul] = WgslSource.TiledGemm,
        [TensorOpFamily.Conv1D] = WgslSource.Im2ColGemm, [TensorOpFamily.Conv2D] = WgslSource.Im2ColGemm, [TensorOpFamily.Conv3D] = WgslSource.Im2ColGemm,
        [TensorOpFamily.MaxPool] = WgslSource.StridedWindowMax, [TensorOpFamily.AvgPool] = WgslSource.StridedWindowAvg,
    }.ToFrozenDictionary();

    static readonly ConcurrentDictionary<TensorOpFamily, DeviceKernel> Compiled = new();

    public static Fin<DeviceKernel> Compile(WgpuDevice device, TensorOpFamily row, ReadOnlySpan<long> launch) =>
        Wgsl.TryGetValue(row, out string? source)
            ? Fin.Succ(Compiled.GetOrAdd(row, key => device.Build(key, source!, launch.ToArray())))
            : Fin.Fail<DeviceKernel>(ComputeFault.Create($"<device-kernel-miss:{row.Key}>"));
}

public static class DeviceDispatch {
    public static Fin<ComputeReceipt.TensorRun> Dispatch(WgpuDevice device, DeviceKernel kernel, ReadOnlySpan<DeviceBuffer> bindings, (uint X, uint Y, uint Z) workgroups, OrtResidency residency, ClockPolicy clocks, CorrelationId correlation) =>
        residency.Device
            ? device.RecordAndSubmit(kernel, bindings, workgroups)
                .Map(elapsed => new ComputeReceipt.TensorRun(kernel.Op, "float32", Elements(bindings), SimdWidth: "device-wgpu", Partitions: (int)(workgroups.X * workgroups.Y * workgroups.Z)) {
                    Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.DeviceWgpu, AllocationClass = AllocationClass.DeviceWgpu,
                    Elapsed = elapsed, DeterminismTag = $"device-wgpu:{device.Identity}",
                })
            : TensorFault.Fail<ComputeReceipt.TensorRun>("device-residency-required", kernel.Op.Key);

    static long Elements(ReadOnlySpan<DeviceBuffer> bindings) {
        long total = 0;
        foreach (DeviceBuffer buffer in bindings) { total += buffer.ByteLength / sizeof(float); }
        return total;
    }
}
```

## [05]-[RESEARCH]

- [OPERATOR_BACKLOG]: `Normalize` has no `TensorPrimitives` member and never becomes a single-call row — vector normalization composes `Norm` then `Divide` against the reduced magnitude. `ConvertToInteger`/`ConvertToIntegerNative` are conversion rows whose `ConvertKernel<TFrom, TTo>` instantiation is the integer-destination `ConvertKernels<TFrom, int>`/`<TFrom, long>` row, reached only behind a `TensorDtype.Quantized` admission, never a bare float-to-int loop.
- [PARTITION_CLAIM]: the fingerprint-matched `BenchmarkClaim` that gates the `ParallelHelper` partition route over the lowered `Tensor/factor#KERNEL_LOWERING` GEMM resolves against a live host fingerprint; the in-page partition fence reads the threaded claim's `Route` column, and the cold start is the unpartitioned GEMM until a winning claim row lands.
- [DEVICE_RESIDENCY]: the `DeviceResident` row's `OrtEnv.CreateSharedAllocator` device-memory allocation, the `BoundFlow` `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` stream-fence latency on the live CoreML/GPU rows, and the `[04]-[DEVICE_KERNELS]` WGSL compute-pipeline dispatch over the shared `ONE_WGPU_DEVICE` resolve against a device host that exposes the shared adapter; the CPU residency rows (`ManagedSpan`/`MemoryBacked`/`OutputValue`/`SpanView`) and the IEEE-754/quantized egress polarities are the proved terminal, the `device-wgpu` substrate row vetoes itself and degrades to `cpu-tensor` when the shared device is absent, and the device fence/pipeline is a CPU no-op until a device host runs it. The open leaf is the live WGSL kernel-body grounding (the tiled-GEMM/im2col-GEMM/strided-window/SpMV shader sources and their workgroup-size tiling against the device `SupportedLimits`) and the WGPU↔ORT `OrtResidency.DeviceResident` buffer-pointer bridge, both grounded against the running AppUi-owned device.
- [DDG_ADJOINT]: the differentiable-geometry operator adjoints are settled in BOTH modes — the six `Tensor/vocabulary#OPERATION_TABLE` geometry rows (`Gradient`/`Divergence`/`Curl`/`CotangentLaplacian`/`HeatFlow`/`Spectral`) carry the linear DEC operators the `Rasm`/Vectors `Spectral.cs` `DiscreteCalculus` assembly already builds, the reverse-mode VJP is the linear-operator transpose law `x̄ = Aᵀ·ȳ` (`Backward.Operator(GeometryTape, seed)` routing to `OperatorRow.Adjoint`), and the forward-mode JVP is the operator's own forward `Apply` (`Forward.Operator(GeometryTape, tangent)` — a linear DEC operator is its own pushforward) over the recorded `MeshAdjointSnapshot` (the public Vectors handle over the internal mesh snapshot and its cached `DiscreteCalculus`), the symmetric `CotangentLaplacian`/`HeatFlow`/`Spectral` rows and the diagonal Hodge stars aliasing their forward `Apply` (self-adjoint, reusing the `LaplacianCache` Cholesky factor with no re-factorisation) and the incidence `Gradient`/`Curl` rows routing to their `Divergence`/`d1ᵀ` transpose pair, so no second matrix is assembled and no autodiff runs over the DEC assembly. The `GeometryTape` step records the operator and the `MeshAdjointSnapshot` it was assembled over so the reverse `SensitivityLaw.Chain(Seq<GeometryTape>, upstream)` and the forward sweep both apply each step against THAT step's mesh, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` registry lowers the shape/topology design fields to these rows so `DescendAdjoint` reads a non-degenerate gradient. A fabricated dense Jacobian of a sparse DEC operator and a `TensorOpFamily` geometry member the closed vocabulary table does not declare are the deleted forms; a new geometry operator (connection-Laplacian, remeshing-step) closes by one `Tensor/vocabulary#OPERATION_TABLE` row plus one `GeometryAdjoint.Rows` binding against the live `DiscreteCalculus` operator.
- [HVP_AND_COLORING]: the forward-over-reverse `SensitivityLaw.Hvp` and the `JacobianColoring` greedy distance-1 partition are settled in shape over the recorded tape; the open leaf is the live re-traversable tape state (the `(op, primal)` snapshots carrying enough state for the forward pass through the reverse tape) and the FEM constitutive/contact consumption at the `Solver/contract#CONSTITUTIVE` stress-update/contact-enforcement call site that supplies the strain-energy/gap-potential tape, grounded against the live `DiscreteCalculus` and the Rasm.Materials physical-properties source.
