# [COMPUTE_DISPATCH]

The cpu-tensor kernel dispatch and the equivalence law: the arity kernel-delegate dispatch binding each `TensorOpFamily` row to its TensorPrimitives member, the claim-gated `ParallelHelper` partition column over both the 1-D `For` and the 2-D `For2D` plane partitions, and the equivalence law proving each lane kernel's vector body against its scalar-path reference with the differentiable-operator adjoint and the three-named copy-point law. The page owns the kernel-delegate types and registries, the `TensorOps` dispatch surface, the `PartitionRoute`/`MapBlock`/`PlaneBlock` partition composition, the `EquivalencePolicy`/`AdjointMode`/`DifferentiableOp`/`SensitivityLaw`/`EquivalenceLaw` equivalence surface, the `StagePlane`/`MatMulGeometry`/`Sensitivity` copy-point and adjoint owners, and the `Effects`/`Projection` scalar-fold projectors the arity kernels compose; the kernels ride `System.Numerics.Tensors`, the matrix rows lower through `Tensor/factor#KERNEL_LOWERING`, the partition cap reads AppHost `CpuBudget` and the winning `BenchmarkRow`, and the `TensorOpFamily`/`ToleranceClass`/`TensorDtype`/`TensorFault`/`ComparerAccessors.StringOrdinal` vocabulary arrives settled from `Tensor/vocabulary#TENSOR_VOCABULARY`, the `OrtResidency`/`CopyPoint` copy points from `Tensor/residency#ORT_BRIDGE`. The reverse-mode `SensitivityLaw.Chain` is the tape the `Solver/optimizer#OPTIMIZER_LANE` gradient-adjoint row consumes.

## [01]-[INDEX]

- [01]-[KERNEL_DISPATCH]: arity kernel-delegate tables; one `TensorOps` dispatch surface.
- [02]-[EQUIVALENCE_INTEROP]: equivalence proofs of the vector lane against its scalar-path reference; matmul route; dual-mode (forward+reverse) differentiable adjoint; generalized Gauss-Newton `JᵀJ` product; sparse-Jacobian coloring; copy-point law.
- [03]-[DEVICE_KERNELS]: WGSL compute-pipeline registry lowering matrix/structural/sparse op-family rows to `ONE_WGPU_DEVICE` workgroup dispatch behind the residency gate and a winning benchmark claim.

## [02]-[KERNEL_DISPATCH]

- Owner: `TensorOps`
- Entry: `public static Fin<Unit> Map<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> destination)` — `Fin<Unit>` aborts on a kernel-row miss; the arity siblings `Zip`, `Fuse`, `Dual`, `Bits`, `Shift`, `Population`, `Convert`, `ToHalf`, `ToSingle`, `Root`, `Fold`, `FoldPair`, `IndexOf`, `Polarity`, `Test`, `Aggregate`, `Hamming`, `HammingBits`, `Mask`, `Pool`, the element-domain `ComplexZip`/`ComplexMap`/`ComplexAbs` and `QuaternionZip`/`QuaternionMap`, and the claim-gated `Partition` dispatch the same `TensorOpFamily` table; `Pool` is the rank-aware strided-window fold the four pooling rows share over one `PoolReducers<T>` reducer table walking a `GetDimensionSpan` cursor over a `ToDenseTensor()` plane (which returns `this` on an already-dense backing, so a permuted/sliced input is densified once before the flat-window walk instead of reading across stride gaps) and faulting `pool-window-out-of-range` when the window exceeds the extent; `Aggregate` is the boolean-reduce the `*All`/`*Any` predicate rows share; `Mask` is the predicate-masked write binding `Tensor.FilteredUpdate`; `Partition` reads `CpuBudget.PartitionCap` and the winning claim's partition-route column, dispatching the 1-D `For` over a `MapBlock` or the 2-D `For2D` over a `Memory2D` plane and falling through to inline `Map` when no winning claim is supplied.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operation binds one entry on its arity kernel table; a new activation is one `Activations<T>` composed fold plus one `Unary` row, a new pooling row is one `PoolReducers<T>` window-reducer entry on the shared `Pool` fold, a new predicate-aggregate is one `AggregateReducers<T>` entry on the shared `Aggregate` fold, and a new element-domain op is one `ComplexKernels`/`QuaternionKernels` entry — never a sibling activation/pooling/aggregate/complex method; a matrix kernel is one lowering row read from `Tensor/factor#KERNEL_LOWERING`, never a span-kernel entry; the partition column is one claim-gated execution path reading `CpuBudget.PartitionCap` and the claim's route column, never a new owner; zero new surface.
- Boundary: every span row binds the TensorPrimitives member matching its Pascal-cased key (shift-right binds the arithmetic-shift member, shift-right-logical the logical member) and only the `MaskedWrite` row carries a full-tensor binding — `Tensor.FilteredUpdate(in TensorSpan<T>, in ReadOnlyTensorSpan<bool>, in ReadOnlyTensorSpan<T>)` is the predicate-masked write the row's name advertises, the `Mask` arity binding it (the unconditional `SetSlice` region overwrite is the deleted form that does not honor the mask); the predicate rows write non-`T` destinations on constraint-faithful owners — `Sign` fills `Span<int>` through `Polarity` on the `TensorKernels<T>` `Sign` table co-located with the float-only `ILogB`, so `Polarity` carries that owner's `IFloatingPointIeee754<T>` constraint (`TensorPrimitives.Sign` needs only `INumber<T>`, subsumed), while `IsNaN`/`IsFinite`/`IsPow2` fill `Span<bool>` through `Test` over the separate `IBinaryNumber<T>`-constrained `MaskKernels<T>` predicate owner — `IBinaryNumber<T>` is the constraint `TensorPrimitives.IsPow2` demands (`where T : IBinaryNumber<T>`), a strict superset of the `INumberBase<T>` the other predicate members need and satisfied by every floating and integer carrier, so the predicate table is split out of the `IFloatingPointIeee754<T>` `TensorKernels<T>` rather than placed where `IsPow2` cannot bind, and `IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny`/`IsPow2All`/`IsPow2Any` reduce to one `bool` through `Aggregate` over the verified `TensorPrimitives.IsNaNAll`/`IsNaNAny`/`IsFiniteAll`/`IsFiniteAny`/`IsPow2All`/`IsPow2Any` members on the matching `IBinaryNumber<T>` `AggregateReducers<T>` owner, deciding the algorithms.md empty-span finite gate (`IsFiniteAll` on an empty span returns false, decided by its own arm before the finite gate); `SinCos` writes the two destination spans through `Dual` under the `IFloatingPointIeee754<T>` constraint of the `TensorKernels<T>` owner that holds the `Dual` table (`TensorPrimitives.SinCos` needs only `ITrigonometricFunctions<T>`, which that constraint subsumes); `RootN`/`ScaleB` take the integer parameter through `Root` under `IRootFunctions<T>`; `PopCount`/`LeadingZeroCount`/`TrailingZeroCount`/`OnesComplement` ride the integer `Population` arity, `BitwiseOr`/`Xor` the integer binary table, and `RotateLeft`/`RotateRight`/`ShiftRightLogical` the integer shift table; `ConvertToHalf` binds the fixed `(float→Half)` `ToHalf` row and `ConvertToSingle` the fixed `(Half→float)` `ToSingle` widen row, paired as the Half narrow/widen boundary the model-boundary brain-float carrier crosses at the inference admission seam; `HammingBitDistance` binds the `HammingBits` integer-pair reduce; `MinNumber`/`MaxNumber` bind the `TensorPrimitives.MinNumber`/`MaxNumber` reduction members (`T MinNumber<T>(ReadOnlySpan<T>)`/`T MaxNumber<T>(ReadOnlySpan<T>)` under `INumber<T>`, which the `IFloatingPointIeee754<T>` table constraint satisfies) on the `Fold` table paired against the NaN-propagating `Min`/`Max` rows, and `MaxMagnitude`/`MinMagnitude` bind the signed-absolute-extremum `TensorPrimitives.MaxMagnitude`/`MinMagnitude` reduction members on the same `Fold` table — the `MaxMagnitude` member is the abs-normalizing denominator the `EquivalenceLaw` reads; `Average` and `StdDev` bind the direct `TensorPrimitives.Average`/`StdDev` reduction members (`T Average<T>(ReadOnlySpan<T>)` under `INumberBase<T>` and `T StdDev<T>(ReadOnlySpan<T>)` under `IRootFunctions<T>`, both satisfied by the `IFloatingPointIeee754<T>` table constraint) on the `Fold` table — a hand-composed `Sum/n` mean or `Sqrt(SumOfSquares/n − mean²)` std-dev beside the admitted members is the deleted form; `ReciprocalEstimate`/`ReciprocalSqrtEstimate` bind the fast-estimate `TensorPrimitives.ReciprocalEstimate`/`ReciprocalSqrtEstimate` members on the `Unary` table beside their exact `Reciprocal`/`ReciprocalSqrt` rows, carrying the `PlatformVariant` tolerance because they have no cross-machine bound; the activation family binds in-lane on the `Unary` table — `Sigmoid`/`Tanh`/`SoftMax` to their direct `TensorPrimitives` members and `ReLU`/`Gelu`/`SiLU`/`LogSoftMax` to the `Activations<T>` composed author-folds (`ReLU` is `Clamp(x, 0, +inf)`, `SiLU` is `x .* Sigmoid(x)`, `Gelu` the tanh-approximation `MultiplyAdd`/`Tanh` chain, `LogSoftMax` the numerically-stable `x − logsumexp(x)` max-shift renting a `MemoryOwner<T>` exp scratch), never a per-element activation loop or a fabricated `TensorPrimitives.Relu`/`Gelu`/`SiLU`/`LogSoftmax` phantom; the four pooling rows fold in-lane through the rank-aware `Pool` over one `PoolReducers<T>` window-reducer table walking the `GetDimensionSpan` cursor — `MaxPool`/`GlobalMaxPool` reduce each window through `TensorPrimitives.Max` and `AvgPool`/`GlobalAvgPool` through `TensorPrimitives.Average`, the global rows collapsing the whole spatial plane to one window (window = stride = length) — so pooling carries verified members and rank-walks NCHW/voxel planes, and only `MatMul`/`Conv1D`/`Conv2D`/`Conv3D` hold no in-lane member; the element-domain rows ride `System.Numerics.Complex` and `System.Numerics.Quaternion` carriers — Complex arithmetic (`Add`/`Subtract`/`Multiply`/`Divide`/`Negate`) binds the verified `INumberBase<Complex>`-generic `TensorPrimitives` members through `ComplexZip`/`ComplexMap`, while `ComplexAbs`/`ComplexExp`/`ComplexLog`/`Conjugate` are author-folds over the BCL `Complex` intrinsics (no `TensorPrimitives` Complex specialization exists) and `ComplexAbs` writes the `Span<double>` magnitude polarity through the `MagnitudeKernel`; Quaternion implements neither numeric interface so `QuaternionMultiply` (the non-commutative Hamilton product), `QuaternionConjugate`, and `QuaternionNormalize` are author-folds over the BCL `Quaternion` operators through `QuaternionZip`/`QuaternionMap`; the `MatMul`, `Conv1D`/`Conv2D`/`Conv3D` rows hold no TensorPrimitives or in-package member and `Map` resolves each through the `Tensor/factor#KERNEL_LOWERING` binding table behind a winning benchmark-claim row — matmul lowers to the numeric-lane GEMM and convolution to im2col-then-GEMM — returning a `<kernel-row-miss>` `Fin.Fail` only when no lowering row is bound; the partition column dispatches a struct `IAction` 1-D span-kernel through `ParallelHelper.For<TAction>` or a struct `IAction2D` plane-kernel through `ParallelHelper.For2D<TAction>` over a `Memory2D` plane, clamped at `CpuBudget.PartitionCap`, taken only when a winning `BenchmarkRow` (the persisted form of the live `BenchmarkClaim`) names the partition route in its `Route` column through the generated `PartitionRoute.Switch` — a partition count of one collapses inline, an unbudgeted `Parallel.For` over spans is the deleted form, and a runtime-silent `_` arm over the closed `PartitionRoute` union is the deleted dispatch; the binding tables are `FrozenDictionary` indexes keyed through the ordinal `ComparerAccessors.StringOrdinal`, span kernels iterate rows by reference through `RefEnumerable<T>`/`SpanEnumerable<T>` rather than flat indexing, and integer dtypes enter the real-constrained entries through the conversion rows first.

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
    public static Fin<Unit> Dual<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<T> first, Span<T> second) where T : IFloatingPointIeee754<T> =>
        EqualLength(row, x.Length, first.Length, EqualLength(row, x.Length, second.Length, Dispatch(row, TensorKernels<T>.Dual, k => k(x, first, second))));
    public static Fin<Unit> Polarity<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<int> destination) where T : IFloatingPointIeee754<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, TensorKernels<T>.Sign, k => k(x, destination)));
    public static Fin<Unit> Test<T>(TensorOpFamily row, ReadOnlySpan<T> x, Span<bool> destination) where T : IBinaryNumber<T> =>
        EqualLength(row, x.Length, destination.Length, Dispatch(row, MaskKernels<T>.Rows, k => k(x, destination)));
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
    public static Fin<bool> Aggregate<T>(TensorOpFamily row, ReadOnlySpan<T> x) where T : IBinaryNumber<T> =>
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
                ? won.Route.Switch(
                    state: (X: x, Dest: destination, Kernel: kernel, Budget: budget),
                    blocked: static s => Effects.ToFin(() => ParallelHelper.For(0, Blocks(s.X.Length, s.Budget.PartitionCap), new MapBlock<T>(s.X, s.Dest, BlockSize(s.X.Length, s.Budget.PartitionCap), s.Kernel), minimumActionsPerThread: 1)),
                    plane: static (s, plane) => Effects.ToFin(() => ParallelHelper.For2D(0, plane.Rows, 0, 1, new PlaneBlock<T>(s.X, s.Dest, plane.Columns, s.Kernel))))
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

- Owner: `EquivalencePolicy`; `AdjointMode` `[SmartEnum<string>]` forward/reverse rows; `DifferentiableOp` the per-`TensorOpFamily` binding table carrying the reverse-mode vector-Jacobian-product, the `Diagonal` flag, and the forward-mode Jacobian-vector-product as a TOTAL (non-optional) `Func` column on every bound row; `Sensitivity` the ONE directional-derivative owner carrying each non-elementwise op's reverse VJP and forward JVP — sharing one body wherever the two directions coincide so a `Forward`/`Backward` class pair with copy-pasted SoftMax/MatMul bodies is the deleted illusory-dual form — with the MatMul weight projection selected by `AdjointMode` (`Wᵀ` reverse for `ȳ·Wᵀ`, `W` forward for `ẋ·W`), the symmetric SoftMax Jacobian shared across both directions, and the `Operator` DDG geometry apply selecting the `Rasm.Numerics` `Spectral.cs` `OperatorRow.Adjoint` (reverse transpose `Aᵀ·ȳ`) or `OperatorRow.Apply` (forward pushforward `A·ṫ`); `SensitivityLaw` the static dual-mode adjoint, forward and reverse tape sweeps over BOTH the `(op, primal)` and `GeometryTape` tapes, and the generalized Gauss-Newton `JᵀJ·v` (reverse-over-forward) surface; `JacobianColoring` the graph-coloring sparse-Jacobian assembler over the AD tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage.
- Entry: `public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy)` — it fills the `policy.SampleCount` sample inputs through the catalogued distribution fillers, then runs an OP-AGNOSTIC self-contained proof comparing the ABSOLUTE max abs gap against the family's `ToleranceClass.Bound(length, mass)` envelope: an elementwise unary/binary row against its element-by-element scalar-tail reference, a reduction row against its reversed-order reassociation; the matrix family has no scalar-tail kernel so it routes to the `Tensor/factor#KERNEL_LOWERING` `KernelLowering.ProveGemm` GEMM-vs-naive-reference proof (which OWNS MatMul/Conv admission, since the span lane never runs them) and the geometry family proves through `ProveOperator(family, snapshot)` over a `MeshAdjointSnapshot` fixture composing the verified `Rasm.Numerics` `OperatorRow.Apply`/`Adjoint` transpose-pair — so the data-only `Prove` yields the `Unprovable` evidence (deviation `+inf`) for a geometry row it has no mesh for, the verdict is the `ToleranceClass.Bound(length, mass)` envelope under the cancellation gate (never a stored relative scalar), and a non-holding proof aborts dispatch through the `EquivalenceMiss` fault case on the intent rail; `public static Fin<ReadOnlyMemory<float>> Adjoint(TensorOpFamily op, AdjointMode mode, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> seed)` is the forward/reverse-mode differentiable-operator adjoint (forward-mode total — every bound `DifferentiableOp` row supplies a real `Jvp`, so the surface carries no `<no-forward-jvp>` fault), `Chain` folds a recorded `(op, primal)` tape into the reverse-mode gradient, `Pushforward` threads a forward tangent inputs-to-outputs through the same tape for a tall-Jacobian problem, and `GaussNewton(tape, vector) = Chain(tape, Pushforward(tape, vector))` is the matrix-free generalized Gauss-Newton product `JᵀJ·v` (reverse-over-forward of the first-order tape: `Jᵀ` applied to the forward `J·v`), the SPD curvature operator the optimizer Newton-CG/trust-region/Levenberg-Marquardt step consumes — it equals the true Hessian only at a zero-residual or affine point, so the exact stored-energy/loss Hessian-vector product (the FEM consistent tangent) needs the second-order flowing-activation-plus-`f''`-curvature tape the first-order `(op, primal)` tape deliberately omits, never this fold; `JacobianColoring.Of(rows, columns, pattern).Assemble(probeColor)` recovers the full sparse Jacobian in (#colors) directional sweeps into CSR storage.
- Receipt: equivalence runs and explicit copy points materialize as TensorRun receipt evidence at the sink edge, stamped through `ClockPolicy` and keyed by `CorrelationId`; the copy points are exactly the three named bridges the `ORT_BRIDGE` capsule owns plus the `Span2D` staging-plane view and the `ByteString` remote-edge projection.
- Packages: Rasm (project), System.Numerics.Tensors, MathNet.Numerics, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, NodaTime, LanguageExt.Core
- Growth: a new kernel route is one `TensorOpFamily` row with one `EquivalencePolicy` row; convolution lands as one matrix-kind row lowered through `Tensor/factor#KERNEL_LOWERING` im2col and pooling as one structural-kind row lowered to the strided-window route; a new differentiable operator is one `DifferentiableOp` row binding its vector-Jacobian-product and (for a non-elementwise op) its Jacobian-vector-product to one `Sensitivity` directional body, so the six DDG geometry rows each gain reverse-mode adjoint coverage by one `DifferentiableOp` row routing to `Sensitivity.Operator` under `AdjointMode.Reverse` and forward coverage under `AdjointMode.Forward`, a new geometry operator (remeshing-step, connection-Laplacian) lands as one `Tensor/vocabulary#OPERATION_TABLE` geometry row plus one `GeometryAdjoint.Rows` binding, a generalized Gauss-Newton curvature operator is one `SensitivityLaw.GaussNewton` composition over the existing forward+reverse primitives, while the EXACT Hessian-vector product is a distinct second-order capability that grows an `f''` curvature column on `DifferentiableOp` plus a flowing-activation tape (never a free composition of first-order primitives), and a large sparse Jacobian is one `JacobianColoring` over the same tape into the `Tensor/factor#SPARSE_SOLVE` CSR storage — never a parallel autodiff surface; zero new surface.
- Boundary: TensorPrimitives carries no matrix kernels — the matmul and convolution rows lower through `Tensor/factor#KERNEL_LOWERING` (matmul to the numeric-lane GEMM, each convolution to the live `Im2Col` patch projection then one GEMM call carrying the `ConvWindow` geometry) so a convolution row inherits the matmul tolerance proof the lowering row carries, and the pooling rows fold each window through the `TensorPrimitives.Max`/composed-`Average` kernels over `GetDimensionSpan` cursors on the same lowering; numeric-lane owns the lowering table and the tensor-lane `Map` consults it, so a matrix or structural row resolves to a live kernel and `Map`-misses only when a convolution row arrives without its `ConvWindow` geometry, never silently resolving to a wrong kernel; zero-copy projections cross at exactly three receipted copy points the `ORT_BRIDGE` capsule owns — tensor span to `OrtValue` through `CreateTensorValueFromSystemNumericsTensorObject` (model lane), to `Span2D` planes (staging views via `AsSpan2D`), to `ByteString` through `UnsafeByteOperations` (remote edge) — each stamped as a `CopyPoint` receipt naming its gate and native byte count, and the `Span2D` staging stamp is the `StagePlane` fence below; the equivalence sample inputs fill through `Tensor.FillGaussianNormalDistribution` and `FillUniformDistribution` and the op-agnostic proof runs the family's vector body against its element-by-element scalar-tail reference (elementwise) or its reversed-order reassociation (reduction) — a hand-rolled sample-RNG loop, or a proof that diffs two unrelated random fills without ever running the kernel, is the deleted hollow form; the matrix family has no scalar-tail reference so it routes to the `Tensor/factor#KERNEL_LOWERING` `KernelLowering.ProveGemm` GEMM-vs-naive-reference proof (the proof that OWNS MatMul/Conv admission), and the geometry family proves the adjoint identity `⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩` through `GeometryAdjoint.ProveAdjoint`/`EquivalenceLaw.ProveOperator` over a `MeshAdjointSnapshot` fixture — the deviation ABSOLUTE against the `ToleranceClass.Bound(length, mass)` envelope, the `EquivalenceProof` reading `Bound`/`Holds` from the `ToleranceClass` owner alone (never a stored relative scalar, the de-sync the vocabulary owner forecloses); the differentiable-operator dual mode is `DifferentiableOp.Diagonal`-gated for the SHAPE of the JVP, not for its presence — an elementwise row carries a diagonal Jacobian so its reverse-mode VJP and forward-mode JVP are the one `cotangent .* f'(primal)` fold — the bound elementwise rows are the activations PLUS the chain ring `Add`/`Subtract`/`Multiply`/`Divide`/`Pow`, each carrying the local derivative w.r.t. the flowing operand with the held operand (or the precomputed `Pow` power-rule diagonal `y·x^(y−1)`) recorded as the primal, the constitutive return-map / multi-term stored-energy tape vocabulary `Solver/contract#CONSTITUTIVE` rides — while a non-diagonal row carries both directions through the one `Sensitivity` owner — MatMul the genuinely-distinct weight maps (`ẋ·W` forward through `WeightMatrix`, `ȳ·Wᵀ` reverse through `WeightMatrixTransposed`, both rectangular-total because both project the weight to `Inner×Columns`), SoftMax the symmetric `J·t = y .* (t − ⟨y,t⟩)` map shared across both directions because `J = diag(y) − y·yᵀ` is its own transpose, and the reductions `Sum`/`Dot` the dimension-changing maps — `Sum` broadcasts the scalar cotangent over the recorded extent on reverse and contracts the tangent to `Σẋ` on forward, `Dot` scales the held operand by the scalar cotangent on reverse and contracts the tangent with the held operand on forward — so forward-mode is total over every bound row, the `DifferentiableOp.Jvp` column is a non-optional `Func`, and the surface carries no `<no-forward-jvp>` fault (the deleted vestigial Option); a single reverse `Vjp` body reused for the forward direction is the deleted form for MatMul because it applies the reverse transpose to a forward tangent, and a copy-pasted `Forward`/`Backward` class pair whose SoftMax and MatMul bodies are byte-identical is the deleted illusory-dual form; the generalized Gauss-Newton product is reverse-over-forward composition — `GaussNewton` runs `Pushforward` (the forward JVP seeded by `vector`) to get `J·v` THEN the reverse `Chain` to get `Jᵀ·(J·v)`, so `JᵀJ·v` computes without materializing the dense matrix and the Newton-CG inner step and the SPD trust-region model read the matrix-free curvature operator; this is NOT the exact Hessian — the `Σ x̄ₖ·f''(primalₖ)·ẋₖ` curvature is absent because the first-order `(op, primal)` tape carries neither the flowing activations nor an `f''` column, so the FEM consistent tangent `K_T` that needs the true stored-energy Hessian is the documented second-order open leaf, not this fold (mistaking `JᵀJ·v` for `Hv` is the named defect: for a scalar stored energy `JᵀJ` is the rank-1 `stress⊗stress`, never `∂²W/∂ε²`); the sparse-Jacobian coloring is greedy distance-1 degree-ordered over the detected sparsity pattern (distance-2 coloring is NP-hard, so the owner uses the greedy heuristic) and the pattern must be known or probed first because coloring over an unknown pattern silently under-recovers, so pattern detection precedes the color partition, the per-color seed vector probes the structurally-orthogonal column group through the forward `Pushforward` or the reverse `Chain` and scatters the compressed columns as COO triplets handed once to the `Tensor/factor#SPARSE_SOLVE` `SparseOps.Ingest(SparseFormat.Coo, …)` CSR ingestion — never a raw `CoordinateStorage` RowIndices/Values surgery beside the owned ingestion, and the assembled Jacobian then factors through the `Tensor/factor#SPARSE_ALGEBRA` `Qr` least-squares route, and below the direct-AD column threshold the owner falls through to per-column AD because coloring graph-construction cost dominates a small dense Jacobian; the six DDG geometry rows are non-diagonal and their reverse-mode VJP is the linear-operator transpose law `x̄ = Aᵀ·ȳ` — `Sensitivity.Operator(step, cotangent, AdjointMode.Reverse)` routes to the `Rasm.Numerics` `OperatorRow.Adjoint` over the recorded `MeshAdjointSnapshot` (the public Vectors handle wrapping the internal mesh snapshot and its cached `DiscreteCalculus`, so the Compute lane never names the internal `IntrinsicMesh`), the self-adjoint rows (`CotangentLaplacian`/`HeatFlow`/`Spectral` and the diagonal stars) aliasing `Apply` (the symmetric operator and its cached Cholesky back-substitution are their own adjoint) and the incidence rows (`Gradient`→`Divergence` transpose pair, `Curl`→`d1ᵀ`) routing to the paired transpose apply, so the geometry adjoint re-uses the live `DiscreteCalculus` assembly and the `LaplacianCache` factor with no re-assembled second matrix and no autodiff over the assembly entry — a fabricated dense Jacobian of a sparse DEC operator is the deleted form; the geometry rows gain forward coverage through `Sensitivity.Operator(step, tangent, AdjointMode.Forward)` because a linear DEC operator is its own pushforward (the JVP equals the forward `OperatorRow.Apply` over the recorded `MeshAdjointSnapshot`, reusing the live `DiscreteCalculus` assembly with no second matrix) so the `GeometryTape` forward sweep `SensitivityLaw.Pushforward(Seq<GeometryTape>)` and reverse sweep `SensitivityLaw.Chain(Seq<GeometryTape>)` both ride the one `OperatorRow`; the reverse-mode `Chain` folds the recorded geometry tape so each step applies its own op's adjoint against THAT step's recorded snapshot — the geometry primal IS the `MeshAdjointSnapshot` the operator was assembled over, never a single shared global primal that is correct only for a one-op tape; every designed-only row inherits proof coverage because its `ToleranceClass` rides the `TensorOpFamily` row, so `EquivalenceLaw.Prove` covers a new kernel by data with no `Prove` argument; loosening a `ToleranceClass` bound to pass equivalence is the named production-slack defect — the kernel is fixed, never the bound.

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
    // The bound and the verdict are the `ToleranceClass` envelope ALONE — the owner's `Bound(length, mass)` error
    // envelope under the cancellation-ratio gate, never a stored relative scalar (the de-sync the vocabulary owner
    // forecloses): a catastrophically-cancelling reduction is `Vacuous` and certifies nothing. The deviation is
    // ABSOLUTE (the envelope is N·ε·Σ|x|, never relative), so the proof carries `Length`/`Mass`/`CancellationRatio`.
    public double Bound => Family.Tolerance.Bound(Length, Mass);
    public bool Holds => Family.Tolerance.Holds(MaxDeviation, Length, Mass, CancellationRatio);

    public static EquivalenceProof Of(TensorOpFamily family, ProofEvidence evidence, int sampleCount, Duration elapsed, Instant at, CorrelationId correlation) =>
        new(family, evidence.Deviation, evidence.Length, evidence.Mass, evidence.CancellationRatio, sampleCount, elapsed, at, correlation);
}

// The op-agnostic proof EVIDENCE every `Prove` arm funnels into the `EquivalenceProof`: the absolute max-abs
// deviation, the accumulation length and operand mass the `ToleranceClass` envelope keys, and the cancellation
// ratio the `Vacuous` gate reads. The matrix/geometry families have no scalar-tail span reference, so the
// data-only `Prove` yields `Unprovable` (deviation `+inf`, ratio 1.0 — an honest non-proof, not vacuousness): the
// matrix family routes to `Tensor/factor#KERNEL_LOWERING` `KernelLowering.ProveGemm` and the geometry family to
// `EquivalenceLaw.ProveOperator` over a mesh fixture, each returning real evidence.
public readonly record struct ProofEvidence(double Deviation, int Length, double Mass, double CancellationRatio) {
    public static readonly ProofEvidence Unprovable = new(double.PositiveInfinity, 0, 0.0, 1.0);
}

public static class StagePlane {
    public static (Span2D<float> Plane, CopyPoint Point) Stage(MemoryOwner<float> backing, int rows, int columns, ClockPolicy clocks, CorrelationId correlation) =>
        (backing.Memory.Span.AsSpan2D(rows, columns), new CopyPoint(OrtResidency.SpanView, (long)rows * columns * sizeof(float), "cpu", clocks.Now, correlation));
}

public readonly record struct MatMulGeometry(int Rows, int Inner, int Columns, ShardPlan ShardPlan) {
    // Row-vector (M=1) activation convention: the differentiated input is one activation row, the recorded
    // primal is the step's weight, and the direction length names the contracted dimension — Inner from the
    // direction, Columns from the residual weight extent. BOTH directional weight projections are Inner×
    // Columns so SeedMatrix(1×Inner)·weight(Inner×Columns) is dimensionally total for a RECTANGULAR K≠N
    // weight, not only the square case; the M>1 batched activation is one direction sweep per row.
    public static MatMulGeometry RowMajor(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction) =>
        new(Rows: 1, Inner: direction.Length, Columns: primal.Length / Math.Max(1, direction.Length), new ShardPlan.Single());

    public Matrix<double> SeedMatrix(ReadOnlyMemory<float> direction) =>
        Matrix<double>.Build.Dense(Rows, Inner, (r, c) => direction.Span[r * Inner + c]);

    // Forward JVP weight W as Inner×Columns: the pushforward ẋ·W applies the UN-transposed weight.
    public Matrix<double> WeightMatrix(ReadOnlyMemory<float> primal) =>
        Matrix<double>.Build.Dense(Inner, Columns, (r, c) => primal.Span[r * Columns + c]);

    // Reverse VJP weight Wᵀ as Inner×Columns: the transpose ȳ·Wᵀ flows the cotangent back to the input.
    public Matrix<double> WeightMatrixTransposed(ReadOnlyMemory<float> primal) =>
        Matrix<double>.Build.Dense(Inner, Columns, (r, c) => primal.Span[c * Inner + r]);

    // Consumes the lowering Fin at the one boundary where the non-Fin Vjp/Jvp Func contract terminates: a
    // MatMul row on a ShardPlan.Single plan is total (the lowering's single arm is Fin.Succ(left.Multiply
    // (right)) over the compatible Inner×Columns operands), so the Fail arm is structurally unreachable and
    // named, never a silent .IfFail that feeds an empty tangent into the Gauss-Newton product and Newton solve.
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
// One directional-derivative owner for the non-elementwise ops: it carries BOTH the reverse-mode VJP and the
// forward-mode JVP of each op, sharing one body wherever the two directions coincide, so the prior
// Forward/Backward class pair — whose SoftMax and MatMul bodies were byte-identical, the illusory-dual form
// that reads as a rich dual surface yet is copy-paste — is deleted. MatMul picks the weight projection by
// AdjointMode (Wᵀ reverse for ȳ·Wᵀ, W forward for ẋ·W); SoftMax is direction-blind because its Jacobian
// diag(y)−y·yᵀ is symmetric so Jᵀ=J and the VJP equals the JVP; the DEC geometry Operator picks
// OperatorRow.Adjoint (the transpose Aᵀ·ȳ) for reverse and OperatorRow.Apply (a linear operator is its own
// pushforward A·ṫ) for forward over the recorded mesh snapshot.
public static class Sensitivity {
    public static ReadOnlyMemory<float> MatMul(MatMulGeometry geometry, ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction, AdjointMode mode) =>
        geometry.Flatten(KernelLowering.Lower(TensorOpFamily.MatMul, geometry.SeedMatrix(direction),
            mode == AdjointMode.Reverse ? geometry.WeightMatrixTransposed(primal) : geometry.WeightMatrix(primal), geometry.ShardPlan));

    public static ReadOnlyMemory<float> SoftMax(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction) {
        using MemoryOwner<float> yOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        using MemoryOwner<float> jacobianOwner = MemoryOwner<float>.Allocate(primal.Length, AllocationMode.Clear);
        Span<float> y = yOwner.Span, jacobian = jacobianOwner.Span;
        TensorPrimitives.SoftMax(primal.Span, y);
        float dot = TensorPrimitives.Dot<float>(y, direction.Span);
        TensorPrimitives.Subtract(direction.Span, dot, jacobian);
        TensorPrimitives.Multiply<float>(y, jacobian, jacobian);
        return jacobian.ToArray();
    }

    // The reduction directional derivatives — non-diagonal vector→scalar maps. Sum's reverse VJP broadcasts the
    // scalar cotangent over the recorded input extent (x̄ᵢ = ȳ) and its forward JVP contracts the tangent to the
    // scalar Σẋ; Dot's reverse VJP scales the held operand (the recorded primal y) by the scalar cotangent
    // (x̄ᵢ = ȳ·yᵢ) and its forward JVP contracts the tangent with the held operand (ẏ = ẋ·y). These are the
    // dimension-changing reduction adjoints the constitutive strain-energy norm / quadratic-form tapes ride.
    public static ReadOnlyMemory<float> Sum(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (mode == AdjointMode.Forward) { return new[] { TensorPrimitives.Sum(direction.Span) }; }
        float[] broadcast = new float[primal.Length];
        Array.Fill(broadcast, direction.Length > 0 ? direction.Span[0] : 0f);
        return broadcast;
    }

    public static ReadOnlyMemory<float> Dot(ReadOnlyMemory<float> primal, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (mode == AdjointMode.Forward) {
            int n = Math.Min(primal.Length, direction.Length);
            return new[] { TensorPrimitives.Dot<float>(direction.Span[..n], primal.Span[..n]) };
        }
        float[] scaled = new float[primal.Length];
        TensorPrimitives.Multiply(primal.Span, direction.Length > 0 ? direction.Span[0] : 0f, scaled);
        return scaled;
    }

    // The DEC operator works in float64 (`Arr<double>` the Rasm.Numerics carrier) while the autodiff tape is
    // float32; the impedance converts through `TensorPrimitives.ConvertChecked` at the seam, never the phantom
    // `Arr.fromSpan`/`Arr.AsSpan` spelling — the verified factory is `Arr.create<T>(ReadOnlySpan<T>)` and the
    // read-back is `.ToArray()`. A row outside the geometry table passes the direction through as identity.
    public static ReadOnlyMemory<float> Operator(GeometryTape step, ReadOnlyMemory<float> direction, AdjointMode mode) {
        if (!GeometryAdjoint.Rows.TryGetValue(step.Op, out OperatorRow? row)) { return direction; }
        Func<MeshAdjointSnapshot, Arr<double>, Fin<Arr<double>>> apply = mode == AdjointMode.Reverse ? row.Adjoint : row.Apply;
        using MemoryOwner<double> wide = MemoryOwner<double>.Allocate(direction.Length, AllocationMode.Clear);
        TensorPrimitives.ConvertChecked(direction.Span, wide.Span);
        return apply(step.Snapshot, Arr.create<double>(wide.Span)).Match(
            Succ: static result => { float[] narrow = new float[result.Count]; TensorPrimitives.ConvertChecked<double, float>(result.ToArray(), narrow); return (ReadOnlyMemory<float>)narrow; },
            Fail: _ => direction);
    }
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

    // The geometry analogue of the span-kernel scalar-tail proof: the linear-operator transpose identity
    // ⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩ over a `MeshAdjointSnapshot` FIXTURE (a small canonical mesh, so the accumulation
    // residual stays inside the family band), composing the verified `Rasm.Numerics` `OperatorRow.Apply`/`Adjoint`
    // contract — the self-adjoint rows alias `Adjoint` to `Apply`, the incidence rows route the paired transpose
    // — so the assembled DEC operator pair is certified WITHOUT a per-op reference and WITHOUT re-assembling the
    // operator (the live `DiscreteCalculus` factor the snapshot already holds). A random domain vector x feeds
    // the forward `Apply`; a random codomain vector y (sized from `Apply`'s output) feeds `Adjoint`; the absolute
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
    Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> Vjp,
    Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> Jvp) {
    public static readonly FrozenDictionary<TensorOpFamily, DifferentiableOp> Rows = new Dictionary<TensorOpFamily, DifferentiableOp> {
        [TensorOpFamily.Tanh] = Diag(TensorOpFamily.Tanh, static (primal, seed) => Elementwise(primal, seed, static p => 1f - p * p)),
        [TensorOpFamily.Sigmoid] = Diag(TensorOpFamily.Sigmoid, static (primal, seed) => Elementwise(primal, seed, static p => p * (1f - p))),
        [TensorOpFamily.Exp] = Diag(TensorOpFamily.Exp, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        [TensorOpFamily.Log] = Diag(TensorOpFamily.Log, static (primal, seed) => Elementwise(primal, seed, static p => 1f / p)),
        [TensorOpFamily.ReLU] = Diag(TensorOpFamily.ReLU, static (primal, seed) => Elementwise(primal, seed, static p => p > 0f ? 1f : 0f)),
        // The elementwise-ring chain rows: each is diagonal in the FLOWING (primary) operand the (op, primal)
        // tape threads, the held operand (or a precomputed local coefficient) recorded as the primal — the MatMul
        // `primal=weight` convention extended to the scalar ring. Add/Subtract differentiate the addend/minuend
        // (∂=1, the held operand carried but unread); Multiply scales by the held factor (∂=y, primal=y); Divide
        // by the reciprocal denominator (∂=1/y, primal=y); Pow records the power-rule diagonal y·x^(y−1) as its
        // primal because the local derivative needs BOTH base and exponent (neither snapshot alone suffices) —
        // the stored pullback the cotangent multiplies. These supply the constitutive return-map / multi-term
        // stored-energy tape vocabulary the `Solver/contract#CONSTITUTIVE` stress-update/contact tapes compose.
        [TensorOpFamily.Add] = Diag(TensorOpFamily.Add, static (primal, seed) => Elementwise(primal, seed, static _ => 1f)),
        [TensorOpFamily.Subtract] = Diag(TensorOpFamily.Subtract, static (primal, seed) => Elementwise(primal, seed, static _ => 1f)),
        [TensorOpFamily.Multiply] = Diag(TensorOpFamily.Multiply, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        [TensorOpFamily.Divide] = Diag(TensorOpFamily.Divide, static (primal, seed) => Elementwise(primal, seed, static p => 1f / p)),
        [TensorOpFamily.Pow] = Diag(TensorOpFamily.Pow, static (primal, seed) => Elementwise(primal, seed, static p => p)),
        // MatMul is genuinely bilinear: the reverse VJP applies Wᵀ, the forward JVP applies W, so the two
        // arms are distinct directional maps, not the deleted copy-paste-identical body.
        [TensorOpFamily.MatMul] = Bilinear(TensorOpFamily.MatMul,
            static (primal, seed) => Sensitivity.MatMul(MatMulGeometry.RowMajor(primal, seed), primal, seed, AdjointMode.Reverse),
            static (primal, tangent) => Sensitivity.MatMul(MatMulGeometry.RowMajor(primal, tangent), primal, tangent, AdjointMode.Forward)),
        // SoftMax's Jacobian is symmetric, so one body serves both directions — Bilinear with the same map
        // names the shared identity instead of duplicating it.
        [TensorOpFamily.SoftMax] = Bilinear(TensorOpFamily.SoftMax,
            static (primal, seed) => Sensitivity.SoftMax(primal, seed),
            static (primal, tangent) => Sensitivity.SoftMax(primal, tangent)),
        // The reduction rows are non-diagonal (vector→scalar), so the reverse VJP and forward JVP are GENUINELY
        // distinct directional maps routed to the one `Sensitivity` owner, not a shared diagonal fold: Sum
        // broadcasts the scalar cotangent over the recorded extent (reverse) and contracts the tangent to Σẋ
        // (forward); Dot scales the held operand by the scalar cotangent (reverse) and contracts the tangent
        // with it (forward). These supply the strain-energy norm / quadratic-form tape vocabulary the
        // `Solver/contract#CONSTITUTIVE` exact return-map (algorithmic) tangent composes.
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
        new(forward, Diagonal: true, derivative, derivative);

    // Forward is total over every bound row: each carries a real JVP, so the prior Option<Jvp> + the dead
    // <no-forward-jvp> fault are deleted — a row either resolves both directions or is absent (no-adjoint-row).
    static DifferentiableOp Bilinear(TensorOpFamily forward, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> vjp, Func<ReadOnlyMemory<float>, ReadOnlyMemory<float>, ReadOnlyMemory<float>> jvp) =>
        new(forward, Diagonal: false, vjp, jvp);

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
            ? Fin.Succ((mode == AdjointMode.Reverse ? differentiable.Vjp : differentiable.Jvp)(primal, seed))
            : TensorFault.Fail<ReadOnlyMemory<float>>("no-adjoint-row", op.Key);

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), (grad, step) => grad.Bind(g => Adjoint(step.Op, AdjointMode.Reverse, step.Primal, g)));

    public static Fin<ReadOnlyMemory<float>> Chain(Seq<GeometryTape> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(Fin.Succ(upstream), static (grad, step) => grad.Map(g => Sensitivity.Operator(step, g, AdjointMode.Reverse)));

    // Forward-mode pushforward through the (op, primal) tape: the dual of the reverse Chain, threading a
    // forward tangent inputs-to-outputs so a tall-Jacobian problem (few inputs, many outputs) costs one
    // forward sweep.
    public static Fin<ReadOnlyMemory<float>> Pushforward(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> tangent) =>
        tape.Fold(Fin.Succ(tangent), (dot, step) => dot.Bind(t => Adjoint(step.Op, AdjointMode.Forward, step.Primal, t)));

    // Forward geometry sweep, the dual of the reverse Chain(Seq<GeometryTape>): each step applies its own DEC
    // operator's pushforward (the operator is its own forward map) against THAT step's recorded mesh snapshot,
    // so the forward and reverse geometry sweeps both ride the one OperatorRow with no re-assembled matrix.
    public static Fin<ReadOnlyMemory<float>> Pushforward(Seq<GeometryTape> tape, ReadOnlyMemory<float> tangent) =>
        tape.Fold(Fin.Succ(tangent), static (dot, step) => dot.Map(t => Sensitivity.Operator(step, t, AdjointMode.Forward)));

    // Generalized Gauss-Newton matrix-free product JᵀJ·v: run the forward JVP seeded by `vector` to get J·v,
    // then the reverse Chain to get Jᵀ·(J·v) — reverse-over-forward of the ONE first-order tape, no dense
    // matrix materialized. This is the SPD curvature operator Newton-CG / trust-region / Levenberg-Marquardt
    // consume (SPD by construction, so CG never breaks on an indefinite step). It is NOT the exact Hessian:
    // the Σ x̄ₖ·f''(primalₖ)·ẋₖ curvature is absent because the first-order (op, primal) tape carries neither the
    // flowing activations nor a second-derivative column — the true Hessian-vector product is a separate
    // second-order capability (an f'' row on DifferentiableOp plus a forward-over-reverse sweep), not this fold.
    public static Fin<ReadOnlyMemory<float>> GaussNewton(Seq<(TensorOpFamily Op, ReadOnlyMemory<float> Primal)> tape, ReadOnlyMemory<float> vector) =>
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
    // law), a reduction row runs the candidate against the SAME reduction over the reversed input (the
    // reassociation-stability the `AccumulationScaled` bound certifies). The deviation is the ABSOLUTE max abs
    // gap and the verdict is the `ToleranceClass.Bound(length, mass)` envelope under the cancellation gate — a
    // stored relative scalar is the de-sync the vocabulary owner forecloses. Diffing two unrelated random fills
    // without ever running the kernel is the deleted hollow form. The matrix family has no scalar-tail kernel,
    // so it routes to the `Tensor/factor#KERNEL_LOWERING` GEMM-vs-naive-reference proof (`KernelLowering.ProveGemm`,
    // which OWNS MatMul/Conv admission); the geometry family has no data fixture here, so the data-only `Prove`
    // yields `Unprovable` and the geometry gate is `ProveOperator` over a `MeshAdjointSnapshot`.
    public static EquivalenceProof Prove(ClockPolicy clocks, CorrelationId correlation, EquivalencePolicy policy) {
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
            TensorKernels<double>.Unary.GetValueOrDefault(policy.Family) is { } unary ? SpanEvidence(UnaryGap(unary, a), a, policy.SampleCount)
            : TensorKernels<double>.Binary.GetValueOrDefault(policy.Family) is { } binary ? SpanEvidence(BinaryGap(binary, a, b), a, policy.SampleCount)
            : TensorKernels<double>.Fold.GetValueOrDefault(policy.Family) is { } fold ? SpanEvidence(FoldGap(fold, a), a, policy.SampleCount)
            : KernelLowering.IsMatrix(policy.Family) ? KernelLowering.ProveGemm(policy.SampleCount)
            : ProofEvidence.Unprovable;
        return EquivalenceProof.Of(policy.Family, evidence, policy.SampleCount, clocks.Elapsed(mark), clocks.Now, correlation);
    }

    // The geometry-family gate the data-only `Prove` cannot reach (no mesh): a geometry row proves through the
    // adjoint-identity ⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩ over a `MeshAdjointSnapshot` fixture, composing the verified Vectors
    // `OperatorRow.Apply`/`Adjoint` transpose-pair via `GeometryAdjoint.ProveAdjoint`. A row outside the geometry
    // table is `no-adjoint-row` (never silently admitted), and the verdict reads the same `ToleranceClass`
    // envelope the span/matrix proofs read — the geometry band keys the proof, never a loosened bound.
    public static Fin<EquivalenceProof> ProveOperator(ClockPolicy clocks, CorrelationId correlation, TensorOpFamily family, MeshAdjointSnapshot snapshot) {
        long mark = clocks.Mark();
        return GeometryAdjoint.Rows.TryGetValue(family, out OperatorRow? row)
            ? GeometryAdjoint.ProveAdjoint(row, snapshot).Map(evidence => EquivalenceProof.Of(family, evidence, evidence.Length, clocks.Elapsed(mark), clocks.Now, correlation))
            : TensorFault.Fail<EquivalenceProof>("no-adjoint-row", family.Key);
    }

    // The span deviation is absolute (the envelope is N·ε·Σ|x|, never a relative scalar), so the evidence carries
    // the operand mass Σ|xᵢ| and the cancellation ratio |Σxᵢ|/Σ|xᵢ| the `Vacuous` gate reads alongside the gap.
    static ProofEvidence SpanEvidence(double deviation, ReadOnlySpan<double> input, int length) {
        double mass = double.Abs(TensorPrimitives.SumOfMagnitudes<double>(input));
        double ratio = mass > 0.0 ? double.Abs(TensorPrimitives.Sum<double>(input)) / mass : 1.0;
        return new ProofEvidence(deviation, length, mass, ratio);
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

    // The ABSOLUTE max-abs gap between the vector body and its element-by-element scalar tail; the operand mass
    // and the envelope bound live with the `ProofEvidence` the caller folds, never a relative pre-division here.
    static double SpanGap(Span<double> vectorized, ReadOnlySpan<double> scalar) {
        TensorPrimitives.Subtract<double>(vectorized, scalar, vectorized);
        TensorPrimitives.Abs<double>(vectorized, vectorized);
        return TensorPrimitives.Max<double>(vectorized);
    }
}
```

## [04]-[DEVICE_KERNELS]

- Owner: `DeviceKernels` — the WGSL compute-pipeline registry parallel in shape to the `Tensor/factor#KERNEL_LOWERING` binding table (a `FrozenDictionary<TensorOpFamily, DeviceKernel>` keyed by op-family, NOT an extension of the CPU `TensorKernels<T>` delegate tables) mapping each matrix/structural/sparse op-family row to its compiled `Silk.NET.WebGPU` `ComputePipeline` plus `BindGroupLayout`; `DeviceKernel` the compiled-pipeline-and-layout value (a WGSL `ShaderModule`, a `ComputePipeline`, a `BindGroupLayout`, and the launch geometry, never a span delegate); `WgpuDevice` the boundary capsule over the shared `ONE_WGPU_DEVICE` `Device`/`Queue` the AppUi renderer owns; `DeviceDispatch` the static record-and-submit fold building a compute pass, binding the tensor storage `Buffer`s, and issuing `DispatchWorkgroups` over the op's tile/workgroup decomposition; the WGPU↔ORT device-residency bridge is NOT a dispatch.md owner — it is the `Tensor/residency#ORT_BRIDGE` `OrtResidency.DeviceResident` row (a WGPU buffer and an ORT device value share that one residency without a host round-trip), grounded at the live-device leaf.
- Cases: the grounded `DeviceKernels.Wgsl` device op rows — `MatMul` (tiled GEMM over `WgslSource.TiledGemm`), `Conv2D` (`WgslSource.Im2Col` gather then the TiledGemm pipeline, the two-dispatch convolution mirroring the CPU im2col-then-GEMM), `MaxPool`/`AvgPool` (strided-window reduce) — each a real WGSL compute pipeline compiled and cached on the registry; `Conv1D`/`Conv3D` and the `Tensor/factor#SPARSE_ALGEBRA` `Spmv`/`Spmm` rows stay CPU-lowered through factor.md until their device shaders ground (the device path is never a phantom mapping), the elementwise `TensorKernels<T>` rows stay CPU `TensorPrimitives`, and a device elementwise map is a future row, not a fork of the dispatch surface.
- Entry: `public static Fin<DeviceKernel> Compile(WgpuDevice device, TensorOpFamily row, ReadOnlySpan<long> launch)` compiles one op-family's WGSL `ShaderModule` into a `ComputePipeline` + `BindGroupLayout` once and caches it on the registry; `public static Fin<TensorRun> Dispatch(WgpuDevice device, DeviceKernel kernel, ReadOnlySpan<DeviceBuffer> bindings, (uint X, uint Y, uint Z) workgroups, OrtResidency residency, ClockPolicy clocks, CorrelationId correlation)` records the compute pass, binds the storage buffers, issues `DispatchWorkgroups`, resolves the timestamp `QuerySet`, and stamps the `TensorRun` receipt with the device residency through `TensorBridge.Stamp` — `Fin<T>` aborts when the residency gate forbids the device carrier or the launch geometry exceeds the device `SupportedLimits`.
- Auto: `KernelLowering.Lower` (and the sparse SpMV/SpMM entry of `Tensor/factor#SPARSE_ALGEBRA`) consult `DeviceKernels` instead of the CPU GEMM ONLY when the active `Runtime/admission#SUBSTRATE_AXIS` `Substrate.DeviceWgpu` row is selected AND the `OrtResidency.DeviceResident` gate holds AND a winning `BenchmarkRow` names the device route in its `Route` column — otherwise the CPU `Matrix<double>.Multiply` GEMM is the terminal, so the CPU/device split rides residency and a benchmark claim, never a fork of the `Map`/`Lower` dispatch contract; a device GEMM output feeding the render lane crosses the existing `Rasm.AppUi/Render` `ResidencyManifest.Mint` seam (the same physical `Buffer`, no host copy) rather than a new device-to-render path, and the one shared device descriptor that this row resolves also gates the ONNX Runtime Mac execution-provider residency so a model-lane device tensor and a tensor-lane device kernel resolve the same allocator on the same physical device.
- Receipt: a device dispatch emits the `TensorRun` `ComputeReceipt` carrying the op family, the resolved per-pass GPU nanosecond duration from the `QuerySet` timestamp (never a busy-wait fence), the `device-wgpu` SIMD-width tag and the workgroup count as the partition count, the `DeterminismTag` extended with the device identity, and the `Tensor/memory#ALLOCATION_AXIS` `AllocationClass.DeviceWgpu`; the device GEMM is a new `LinearProvider.DeterminismTag` because a device result is bit-divergent from the managed/native CPU GEMM, so the `SolveDedupKey` folds the device identity exactly as it folds the managed/native provider or a cross-substrate cache hit returns bit-divergent numbers.
- Packages: Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU (the `Wgpu` table for `DevicePoll`/`QueueSubmitForIndex` device-tick readback), Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new device op is one `DeviceKernels.Rows` row binding its WGSL pipeline; a new launch geometry is one column on `DeviceKernel`; zero new surface — a `DeviceTensor`/`GpuTensor` parallel tensor type is the rejected form (device-ness is the `OrtResidency.DeviceResident` residency discriminant, never a second tensor owner), a second device-state machine is the rejected form (the `Runtime/admission#SUBSTRATE_AXIS` `DeviceWgpu` row and `SubstrateSelection` own admission), and a CPU-side fallback math where a row lowers to a device dispatch is the rejected form.
- Boundary: WebGPU kernels are WGSL compute-pipeline bodies over storage buffers, so the registry value is a compiled `ComputePipeline` + `BindGroupLayout`, NOT a span delegate like `UnaryKernel<T>` — the device entry mirrors `KernelLowering`'s table shape and never extends the CPU `TensorKernels<T>` delegate tables, so the CPU/device split rides residency, not a fork of the dispatch surface; the Compute lane holds no second device — `WgpuDevice` composes the renderer's already-acquired `Device`/`Queue` (the `Rasm.AppUi/Render` `ONE_WGPU_DEVICE` boundary capsule mints them, Metal-backed on macOS) and the adapter/device-request entrypoints stay in the AppUi boundary, so the Compute capsule never `CreateInstance`/`AdapterRequestDevice` a second time and the shared `Device`/`Queue` are released by the AppUi owner, not the Compute lane; a `BufferUsage.Storage | CopySrc` storage buffer holds the tensor data and a `MapRead | CopyDst` staging buffer retires the result through the two-phase readback (`CommandEncoderCopyBufferToBuffer` then `BufferMapAsync(MapMode.Read)` polled through `BufferGetMapState` to `BufferGetMappedRange`), the map request advanced by `DevicePoll` from the admitted companion `Silk.NET.WebGPU.Extensions.WGPU` `Wgpu` table — `DevicePoll(device, wait: false, …)` the non-blocking device-tick that advances the readback map without `QueueOnSubmittedWorkDone` polling latency, with `QueueSubmitForIndex` + `DevicePoll(device, wait: true, WrappedSubmissionIndex)` the deterministic non-busy-wait completion when an exact submission index is needed; the WGSL `DispatchWorkgroups(x, y, z)` count derives from the op's tile decomposition against the device `SupportedLimits` `maxComputeWorkgroupSizeX`, never an unbounded launch, and the GEMM tiles the `[M×K]·[K×N]` product into workgroup-sized blocks reading the shared tile into workgroup memory; the device residency bridge is `OrtResidency.DeviceResident` — a WGPU `Buffer` admits to an ORT device value through `CreateTensorValueWithData(OrtMemoryInfo, …, nint, …)` over the buffer's mapped pointer and an ORT device output binds back to a WGPU buffer through `BoundFlow.BindOutputToDevice`, so a model-lane device tensor and a tensor-lane device kernel share one allocation with no host round-trip; the device determinism collides with the `Tensor/blas#DENSE_ALGEBRA` `LinearProvider.DeterminismTag` bit-divergence law — a device GEMM is a new determinism tag and the `SolveDedupKey` must fold the device identity exactly as it folds the managed/native provider, or a cross-substrate cache hit returns bit-divergent numbers (the named correctness defect); all device handles release through their matching `XxxRelease`/`XxxDestroy` native call in a `using`-equivalent scoped fold (not `IDisposable`), the compute-only resources (`Buffer`, `ShaderModule`, `BindGroupLayout`, `ComputePipeline`, `CommandEncoder`, `QuerySet`) owned by this capsule and the shared `Device`/`Queue` owned by AppUi.

```csharp signature
// --- [CONSTANTS] ---------------------------------------------------------------------------
// The WGSL compute-shader bodies the device pipelines compile, one entry per grounded op. The base binding
// is host-language-neutral GPGPU: a storage buffer per tensor operand, a uniform block per geometry, the
// `main` entry the ProgrammableStageDescriptor names. TiledGemm is the [M×K]·[K×N] product over 16×16
// workgroup tiles reading each shared block into workgroup memory; Im2Col is the 2-D NCHW patch projection
// the convolution lowering gathers before reusing TiledGemm (device convolution is the two-dispatch im2col-
// then-GEMM, mirroring the CPU `KernelLowering` path); StridedWindowMax/Avg fold one window per output over
// the flattened (slice × extent) plane.
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

public sealed record DeviceKernel(TensorOpFamily Op, nuint Pipeline, nuint BindGroupLayout, nuint ShaderModule, ImmutableArray<long> Launch);

// --- [SERVICES] ----------------------------------------------------------------------------
// The Compute-lane boundary capsule over the shared ONE_WGPU_DEVICE the AppUi renderer mints (Metal-backed
// on macOS); it NEVER acquires a second `Device`/`Queue` (no CreateInstance/AdapterRequestDevice here). Build
// compiles one op's WGSL into a ComputePipeline through the AUTO-LAYOUT path — a null pipeline `Layout` makes
// the runtime derive the `BindGroupLayout` the WGSL `@group` declarations imply, collapsing the layout-
// descriptor authoring. RecordAndSubmit records the compute pass, issues the workgroup dispatch, reads the
// GPU-side duration from a timestamp `QuerySet` (never a busy-wait fence), and retires the result through the
// two-phase `CommandEncoderCopyBufferToBuffer` -> `MapRead` readback advanced by the wgpu-native `DevicePoll`
// device-tick. Every compute-only handle releases through its native `XxxRelease`; the shared `Device`/`Queue`
// are released by the AppUi owner, not here.
public sealed unsafe class WgpuDevice(WebGPU api, Wgpu ext, Device* device, Queue* queue, string identity) {
    static readonly PfnBufferMapCallback MapNoop = new(static (BufferMapAsyncStatus status, void* data) => { });

    public string Identity => identity;

    public DeviceKernel Build(TensorOpFamily op, string wgsl, long[] launch) {
        nint code = SilkMarshal.StringToPtr(wgsl, NativeStringEncoding.UTF8);
        nint entry = SilkMarshal.StringToPtr("main", NativeStringEncoding.UTF8);
        try {
            ShaderModuleWGSLDescriptor wgslDesc = new() { Chain = new ChainedStruct { SType = SType.ShaderModuleWgslDescriptor }, Code = (byte*)code };
            ShaderModuleDescriptor moduleDesc = new() { NextInChain = (ChainedStruct*)&wgslDesc };
            ShaderModule* module = api.DeviceCreateShaderModule(device, &moduleDesc);
            ComputePipelineDescriptor pipelineDesc = new() { Layout = null, Compute = new ProgrammableStageDescriptor { Module = module, EntryPoint = (byte*)entry } };
            ComputePipeline* pipeline = api.DeviceCreateComputePipeline(device, &pipelineDesc);
            return new DeviceKernel(op, (nuint)pipeline, (nuint)api.ComputePipelineGetBindGroupLayout(pipeline, 0), (nuint)module, [.. launch]);
        }
        finally { SilkMarshal.Free(code); SilkMarshal.Free(entry); }
    }

    public Fin<Duration> RecordAndSubmit(DeviceKernel kernel, ReadOnlySpan<DeviceBuffer> bindings, (uint X, uint Y, uint Z) workgroups) {
        Span<BindGroupEntry> entries = stackalloc BindGroupEntry[bindings.Length];
        for (int i = 0; i < bindings.Length; i++) {
            entries[i] = new BindGroupEntry { Binding = (uint)i, Buffer = (Buffer*)bindings[i].Handle, Offset = 0, Size = (ulong)bindings[i].ByteLength };
        }
        QuerySetDescriptor querySetDesc = new() { Type = QueryType.Timestamp, Count = 2 };
        QuerySet* timestamps = api.DeviceCreateQuerySet(device, &querySetDesc);
        BufferDescriptor resolveDesc = new() { Size = 2 * sizeof(ulong), Usage = BufferUsage.QueryResolve | BufferUsage.CopySrc };
        BufferDescriptor readbackDesc = new() { Size = 2 * sizeof(ulong), Usage = BufferUsage.MapRead | BufferUsage.CopyDst };
        Buffer* resolve = api.DeviceCreateBuffer(device, &resolveDesc);
        Buffer* readback = api.DeviceCreateBuffer(device, &readbackDesc);
        BindGroup* group;
        fixed (BindGroupEntry* entryRoot = entries) {
            BindGroupDescriptor groupDesc = new() { Layout = (BindGroupLayout*)kernel.BindGroupLayout, EntryCount = (nuint)bindings.Length, Entries = entryRoot };
            group = api.DeviceCreateBindGroup(device, &groupDesc);
        }
        CommandEncoder* encoder = api.DeviceCreateCommandEncoder(device, null);
        ComputePassTimestampWrites timestampWrites = new() { QuerySet = timestamps, BeginningOfPassWriteIndex = 0, EndOfPassWriteIndex = 1 };
        ComputePassDescriptor passDesc = new() { TimestampWrites = &timestampWrites };
        ComputePassEncoder* pass = api.CommandEncoderBeginComputePass(encoder, &passDesc);
        api.ComputePassEncoderSetPipeline(pass, (ComputePipeline*)kernel.Pipeline);
        api.ComputePassEncoderSetBindGroup(pass, 0, group, 0, null);
        api.ComputePassEncoderDispatchWorkgroups(pass, workgroups.X, workgroups.Y, workgroups.Z);
        api.ComputePassEncoderEnd(pass);
        api.CommandEncoderResolveQuerySet(encoder, timestamps, 0, 2, resolve, 0);
        api.CommandEncoderCopyBufferToBuffer(encoder, resolve, 0, readback, 0, 2 * sizeof(ulong));
        CommandBuffer* commands = api.CommandEncoderFinish(encoder, null);
        ulong submission = ext.QueueSubmitForIndex(queue, 1, &commands);
        WrappedSubmissionIndex wait = new() { Queue = queue, SubmissionIndex = submission };
        ext.DevicePoll(device, true, &wait);
        api.BufferMapAsync(readback, MapMode.Read, 0, (nuint)(2 * sizeof(ulong)), MapNoop, null);
        while (api.BufferGetMapState(readback) != BufferMapState.Mapped) { ext.DevicePoll(device, false, null); }
        ulong* ticks = (ulong*)api.BufferGetMappedRange(readback, 0, (nuint)(2 * sizeof(ulong)));
        Duration elapsed = Duration.FromNanoseconds(checked((long)(ticks[1] - ticks[0])));
        api.BufferUnmap(readback);
        api.BindGroupRelease(group); api.QuerySetRelease(timestamps); api.BufferRelease(resolve); api.BufferRelease(readback);
        api.ComputePassEncoderRelease(pass); api.CommandBufferRelease(commands); api.CommandEncoderRelease(encoder);
        return Fin.Succ(elapsed);
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DeviceKernels {
    // One grounded WGSL body per device op, compiled once into a ComputePipeline and cached on the registry.
    // The CPU TensorKernels<T> delegate tables are never extended — the device table mirrors KernelLowering's
    // shape. Conv2D maps to the Im2Col gather (device convolution chains the gather pipeline then the TiledGemm
    // pipeline); Conv1D/Conv3D stay CPU-lowered through factor.md until their device shaders ground.
    static readonly FrozenDictionary<TensorOpFamily, string> Wgsl = new Dictionary<TensorOpFamily, string> {
        [TensorOpFamily.MatMul] = WgslSource.TiledGemm,
        [TensorOpFamily.Conv2D] = WgslSource.Im2Col,
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
- [PARTITION_CLAIM]: the fingerprint-matched `BenchmarkRow` (the `Runtime/receipts` persisted form of the winning `BenchmarkClaim`) that gates the `ParallelHelper` partition route over the lowered `Tensor/factor#KERNEL_LOWERING` GEMM resolves against a live host fingerprint; the in-page partition fence reads the threaded row's `Route` column through the generated `PartitionRoute.Switch`, and the cold start is the unpartitioned GEMM until a winning row lands.
- [DEVICE_RESIDENCY]: the `DeviceResident` row's `OrtEnv.CreateSharedAllocator` device-memory allocation, the `BoundFlow` `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` stream-fence latency on the live CoreML/GPU rows, and the `[04]-[DEVICE_KERNELS]` WGSL compute-pipeline dispatch over the shared `ONE_WGPU_DEVICE` resolve against a device host that exposes the shared adapter; the CPU residency rows (`ManagedSpan`/`MemoryBacked`/`OutputValue`/`SpanView`) and the IEEE-754/quantized egress polarities are the proved terminal, the `device-wgpu` substrate row vetoes itself and degrades to `cpu-tensor` when the shared device is absent, and the device fence/pipeline is a CPU no-op until a device host runs it. The WGSL shader bodies (`WgslSource.TiledGemm`/`Im2Col`/`StridedWindowMax`/`StridedWindowAvg`) and the `WgpuDevice` compile/record/readback capsule are grounded IN-PAGE; the open leaf is their LIVE execution against the running AppUi-owned `Device`/`Queue` — the `timestamp-query` feature enablement and the workgroup-size tiling against the device `SupportedLimits` `maxComputeWorkgroupSizeX`, the uncaptured-error scope over a malformed shader compile, the device `Spmv`/`Conv1D`/`Conv3D` shader rows still CPU-lowered, and the WGPU↔ORT `OrtResidency.DeviceResident` buffer-pointer bridge.
- [DDG_ADJOINT]: the differentiable-geometry operator adjoints are settled in BOTH modes — the six `Tensor/vocabulary#OPERATION_TABLE` geometry rows (`Gradient`/`Divergence`/`Curl`/`CotangentLaplacian`/`HeatFlow`/`Spectral`) carry the linear DEC operators the `Rasm.Numerics` `Spectral.cs` `DiscreteCalculus` assembly already builds, the reverse-mode VJP is the linear-operator transpose law `x̄ = Aᵀ·ȳ` (`Sensitivity.Operator(step, seed, AdjointMode.Reverse)` routing to `OperatorRow.Adjoint`), and the forward-mode JVP is the operator's own forward `Apply` (`Sensitivity.Operator(step, tangent, AdjointMode.Forward)` — a linear DEC operator is its own pushforward) over the recorded `MeshAdjointSnapshot` (the public `Rasm.Meshing` handle over the internal mesh snapshot and its cached `DiscreteCalculus`), the symmetric `CotangentLaplacian`/`HeatFlow`/`Spectral` rows and the diagonal Hodge stars aliasing their forward `Apply` (self-adjoint, reusing the `LaplacianCache` Cholesky factor with no re-factorisation) and the incidence `Gradient`/`Curl` rows routing to their `Divergence`/`d1ᵀ` transpose pair, so no second matrix is assembled and no autodiff runs over the DEC assembly. The `GeometryTape` step records the operator and the `MeshAdjointSnapshot` it was assembled over so the reverse `SensitivityLaw.Chain(Seq<GeometryTape>, upstream)` and the forward `SensitivityLaw.Pushforward(Seq<GeometryTape>, tangent)` both apply each step against THAT step's mesh, and the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` registry lowers the shape/topology design fields to these rows so `DescendAdjoint` reads a non-degenerate gradient. A fabricated dense Jacobian of a sparse DEC operator and a `TensorOpFamily` geometry member the closed vocabulary table does not declare are the deleted forms; a new geometry operator (connection-Laplacian, remeshing-step) closes by one `Tensor/vocabulary#OPERATION_TABLE` row plus one `GeometryAdjoint.Rows` binding against the live `DiscreteCalculus` operator. The adjoint pair is equivalence-PROVEN, not asserted: `GeometryAdjoint.ProveAdjoint` (the `⟨A·x, y⟩ == ⟨x, Aᵀ·y⟩` transpose-law residual over a `MeshAdjointSnapshot` fixture, wrapped by `EquivalenceLaw.ProveOperator` into the `ToleranceClass`-bounded `EquivalenceProof`) is the geometry gate the matrix-lane `Tensor/factor#KERNEL_LOWERING` `KernelLowering.ProveGemm` mirrors — neither family reaches the data-only span `Prove`, which returns `ProofEvidence.Unprovable` for them by design.
- [GAUSS_NEWTON_AND_COLORING]: the reverse-over-forward `SensitivityLaw.GaussNewton` generalized Gauss-Newton `JᵀJ·v` product and the `JacobianColoring` greedy distance-1 partition are settled in shape over the recorded tape, and `DifferentiableOp.Rows` now binds the elementwise-ring (`Add`/`Subtract`/`Multiply`/`Divide`/`Pow`) and reduction (`Sum`/`Dot`) adjoint rows beside the activations/`MatMul`/`SoftMax` — the ring as diagonal `cotangent .* f'(primal)` folds, the dimension-changing `Sum`/`Dot` through the one `Sensitivity` owner — so the FEM constitutive/contact strain-energy/gap-potential tapes ride `Chain` for the gradient and `GaussNewton` for the SPD curvature model on the closed adjoint vocabulary; the open leaves are (1) the EXACT stored-energy Hessian-vector product the consistent tangent needs beyond the SPD Gauss-Newton model — a second-order tape carrying the flowing activations plus an `f''` curvature column on `DifferentiableOp`, since `JᵀJ` is the rank-1 `stress⊗stress` for a scalar energy and not `∂²W/∂ε²` — and (2) unrolling the EXACT multi-iteration return-map (algorithmic) tangent into the linear `(op, primal)` tape at the `Solver/contract#CONSTITUTIVE` stress-update/contact-enforcement call site (the chain tape composes the single-step closed-form return directly), both grounded against the live `DiscreteCalculus` and the seam `MaterialPropertySet.Mechanical` the `Solver/contract#CONSTITUTIVE` stress-update reads once off the `Rasm.Element` `ElementGraph` (the canonical material the `Rasm.Materials` projector lowered, content-keyed by `NodeId`), never a downward `Rasm.Materials` reference.
