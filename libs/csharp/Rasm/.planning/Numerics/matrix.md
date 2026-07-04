# [RASM_NUMERICS_MATRIX]

The dense/sparse/complex linear-algebra owner of `Rasm.Vectors` — the one numeric substrate every solver in the kernel rides (DEC assembly, reconstruction FEM, registration, BNOT sampling, dense-output moment fits). The page owns the solve/eigen route vocabularies, the `GaugePolicy` singular-system algebra, the four matrix owners (`Matrix` dense row-major, `SymmetricMatrix` packed-upper, `SparseMatrix` CSR with structural invariants, `SparseHermitian` upper-store complex), the `CholeskySparse` lock-guarded AMD SPD factor cache, the typed solve/eigen/gauge receipts on the rails validity fold, and `MatrixKernel` — dense decompositions and solves over MathNet, sparse direct and iterative solves over CSparse and MathNet BiCgStab with a typed-receipt fallback, the three-mode `SingularGaugeSolve` (pin reduction, M-orthogonal deflation, Lagrange KKT saddle — with Tikhonov-regularized Gram solves and `GaugeShift` post-shifts), generalized eigenpairs by Cholesky congruence, and LOBPCG (Knyazev) for real and Hermitian operators with Rayleigh-Ritz on the `[X|W|P]` subspace, modified-Gram-Schmidt reorthonormalization, Jacobi preconditioning, deterministic basis seeding, and NO hidden dense fallback.

The floor is host-neutral-shaped: finiteness is `double.IsFinite` over flat spans (`TensorPrimitives.IsFiniteAll`), epsilons are `EpsilonPolicy` rows, and no `RhinoMath` member appears — the assembly stays RhinoCommon-aware, this page is portable by inspection. MathNet and CSparse are mined at full depth as the standard library; every result leaves as a typed receipt carrying its route, stop, and the recomputed true relative residual against the original operator — never a raw `Matrix<double>` or factorization instance. Sibling solvers compose these owners; a raw-MathNet reach beside them (the mature registration kernel's `Evd`/`Cholesky` bypass) is the named deleted form.

## [01]-[INDEX]

- [02]-[SOLVE_VOCABULARY]: `EigenSolvePath`/`EigenSolveStop`/`SolvePath`/`SolveStop`/`MatrixNormKind`/`GaugeSolverKind`/`GaugeShift` smart enums + `GaugePolicy` `[Union]` — the route, stop, norm, and gauge algebra every receipt discriminates on.
- [03]-[DENSE_OWNERS]: `Matrix` · `SymmetricMatrix` + `SvdResult`/`LuResult`/`QrResult`/`CholeskyResult` decomposition carriers.
- [04]-[SPARSE_OWNERS]: `SparseMatrix` CSR (+ structural invariants) · `SparseHermitian` upper-store · `CholeskySparse` lock-guarded AMD SPD factor cache.
- [05]-[RECEIPTS]: `SolveReceipt` · `EigenSolveReceipt<TEigen,TVector>` · `GaugeReceipt` — typed evidence on the rails `ValidityClaim.All` fold; the hand-rolled `IsValid` conjunction litany is the deleted form.
- [06]-[SOLVE_KERNEL]: `MatrixKernel` — bridges, dense decompositions/solves, sparse assembly, iterative + direct sparse solves, `SingularGaugeSolve`, generalized eigen by congruence, LOBPCG real + Hermitian.

## [02]-[SOLVE_VOCABULARY]

- Owner: seven smart enums carrying the solve/eigen route space as data — `EigenSolvePath` (5; `IsSparse`/`IsComplex` columns), `EigenSolveStop` (3; `IsUsable` column), `SolvePath` (7; `IsSparse`/`IsFallback`/`UsesDiagonalPreconditioner` columns), `SolveStop` (7; `IsUsable` column), `MatrixNormKind` (4; `[UseDelegateFromConstructor]` compute column so the norm IS the row), `GaugeSolverKind` (3; `IsDirect`/`IsIterative`/`Path` — the declared-route column on the gauge receipt, while the minted `SolveReceipt.Path` reports the stage's ACTUAL route, so neither the KKT SparseLU route nor a deflation solve that landed on the recorded direct fallback ever masquerades), `GaugeShift` (4 post-solve normalizations) — plus `GaugePolicy` the `[Union]` singular-system gauge algebra: `Pin(indices, values, mass, postShift)` reduces the system by eliminating pinned rows, `MeanZeroDeflation(nullspace, mass, postShift)` solves then M-orthogonally projects the nullspace out, `LagrangeKKT(nullspace, mass, postShift)` solves the saddle system with explicit multipliers.
- Cases: `EigenSolvePath` `DenseSymmetricEvd` · `DenseGeneralEvd` · `SparseLobpcg` · `SparseHermitianLobpcg` · `SparseGeneralizedCholeskyCongruence`; `SolvePath` `DenseLu` · `DenseQrLeastSquares` · `DenseCholesky` · `SparseBiCgStabDiagonal` · `SparseMathNetDirectFallback` · `SparseCholesky` · `SparseLuIndefinite`; `SolveStop` `DirectSolved` · `LeastSquaresSolved` · `ResidualConverged` · `DirectFallbackSolved` · `RankDeficient` · `IterativeExhausted` · `FallbackRejected`; `GaugeShift` `None` · `MeanZero` · `MinZero` · `PinZero`; `GaugePolicy` `Pin` · `MeanZeroDeflation` · `LagrangeKKT` (3).
- Entry: `GaugePolicy.PinConstant(index)` / `Pinned(indices)` / `MeanZeroConstant(dimension)` / `KktConstant(dimension)` — the constant-nullspace presets every Laplacian-shaped consumer reaches for; `NullspaceDim`/`Shift`/`SolverKind` are total `Switch` projections off the case.
- Auto: the mass diagonal rides `Option<Arr<double>>` on every case, so the SAME policy value selects Euclidean or M-weighted inner products throughout the gauge solve; `MeanZeroConstant` defaults its post-shift to `GaugeShift.MeanZero` because a deflated solve re-acquires the constant mode through rounding.
- Receipt: none here — the vocabulary rows ARE the receipt discriminants section [05] carries.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Arr`, `Seq`, `Option`).
- Growth: a new solve substrate (a new iterative method, a new factorization) is one `SolvePath`/`EigenSolvePath` row plus one kernel arm — the receipt shape does not change; a new gauge modality is one `GaugePolicy` case with its `Switch` arms breaking loudly at compile time.
- Boundary: routes and stops are never `bool` flags on receipts — the enum row carries `IsUsable`/`IsSparse` as columns so a consumer reads capability off the discriminant; a parallel `FactorKind` enum re-declaring the same space beside `SolvePath` is the deleted form.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using DoubleDouble;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using ComplexVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using DenseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorC = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using SparseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using SparseMatrixD = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EigenSolvePath {
    public static readonly EigenSolvePath DenseSymmetricEvd = new(key: 0, isSparse: false, isComplex: false);
    public static readonly EigenSolvePath DenseGeneralEvd = new(key: 1, isSparse: false, isComplex: true);
    public static readonly EigenSolvePath SparseLobpcg = new(key: 2, isSparse: true, isComplex: false);
    public static readonly EigenSolvePath SparseHermitianLobpcg = new(key: 3, isSparse: true, isComplex: true);
    public static readonly EigenSolvePath SparseGeneralizedCholeskyCongruence = new(key: 4, isSparse: true, isComplex: false);
    public bool IsSparse { get; }
    public bool IsComplex { get; }
}

[SmartEnum<int>]
public sealed partial class EigenSolveStop {
    public static readonly EigenSolveStop DirectSolved = new(key: 0, isUsable: true);
    public static readonly EigenSolveStop ResidualConverged = new(key: 1, isUsable: true);
    public static readonly EigenSolveStop MaxIterationsExhausted = new(key: 2, isUsable: false);
    public bool IsUsable { get; }
}

[SmartEnum<int>]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(key: 0, compute: static m => MatrixKernel.ToMathNet(m).FrobeniusNorm());
    public static readonly MatrixNormKind MaxAbs = new(key: 1, compute: static m => MatrixKernel.ToMathNet(m).Enumerate().Aggregate(0.0, static (acc, e) => Math.Max(acc, Math.Abs(e))));
    public static readonly MatrixNormKind L1 = new(key: 2, compute: static m => MatrixKernel.ToMathNet(m).L1Norm());
    public static readonly MatrixNormKind LInf = new(key: 3, compute: static m => MatrixKernel.ToMathNet(m).InfinityNorm());
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
}

[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseQrLeastSquares = new(key: 1, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseCholesky = new(key: 2, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseBiCgStabDiagonal = new(key: 3, isSparse: true, isFallback: false, usesDiagonalPreconditioner: true);
    public static readonly SolvePath SparseMathNetDirectFallback = new(key: 4, isSparse: true, isFallback: true, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseCholesky = new(key: 5, isSparse: true, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseLuIndefinite = new(key: 6, isSparse: true, isFallback: false, usesDiagonalPreconditioner: false);
    public bool IsSparse { get; }
    public bool IsFallback { get; }
    public bool UsesDiagonalPreconditioner { get; }
}

[SmartEnum<int>]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(key: 0, isUsable: true);
    public static readonly SolveStop LeastSquaresSolved = new(key: 1, isUsable: true);
    public static readonly SolveStop ResidualConverged = new(key: 2, isUsable: true);
    public static readonly SolveStop DirectFallbackSolved = new(key: 3, isUsable: true);
    public static readonly SolveStop RankDeficient = new(key: 4, isUsable: false);
    public static readonly SolveStop IterativeExhausted = new(key: 5, isUsable: false);
    public static readonly SolveStop FallbackRejected = new(key: 6, isUsable: false);
    public bool IsUsable { get; }
}

[SmartEnum<int>]
public sealed partial class GaugeSolverKind {
    public static readonly GaugeSolverKind SparseCholeskyReduced = new(key: 0, isDirect: true, isIterative: false, path: SolvePath.SparseCholesky);
    public static readonly GaugeSolverKind SparseBiCgStabDeflated = new(key: 1, isDirect: false, isIterative: true, path: SolvePath.SparseBiCgStabDiagonal);
    public static readonly GaugeSolverKind SparseLuKkt = new(key: 2, isDirect: true, isIterative: false, path: SolvePath.SparseLuIndefinite);
    public bool IsDirect { get; }
    public bool IsIterative { get; }
    public SolvePath Path { get; }
}

[SmartEnum<int>]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new(0);
    public static readonly GaugeShift MeanZero = new(1);
    public static readonly GaugeShift MinZero = new(2);
    public static readonly GaugeShift PinZero = new(3);
}

[Union]
public abstract partial record GaugePolicy {
    private GaugePolicy() { }
    public sealed record Pin(Arr<int> Indices, Arr<double> Values, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public sealed record MeanZeroDeflation(Arr<Arr<double>> Nullspace, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public sealed record LagrangeKKT(Arr<Arr<double>> Nullspace, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public static GaugePolicy PinConstant(int index, Option<Arr<double>> mass = default, GaugeShift? shift = null) =>
        new Pin(Indices: new Arr<int>([index]), Values: new Arr<double>([0.0]), Mass: mass, PostShift: shift ?? GaugeShift.None);
    public static GaugePolicy Pinned(Seq<int> indices, Option<Arr<double>> mass = default, GaugeShift? shift = null) =>
        new Pin(Indices: new Arr<int>([.. indices]), Values: new Arr<double>([.. indices.Map(static _ => 0.0)]), Mass: mass, PostShift: shift ?? GaugeShift.None);
    public static GaugePolicy MeanZeroConstant(int dimension, Option<Arr<double>> mass = default, GaugeShift? shift = null) =>
        new MeanZeroDeflation(Nullspace: ConstantNullspace(dimension: dimension), Mass: mass, PostShift: shift ?? GaugeShift.MeanZero);
    public static GaugePolicy KktConstant(int dimension, Option<Arr<double>> mass = default, GaugeShift? shift = null) =>
        new LagrangeKKT(Nullspace: ConstantNullspace(dimension: dimension), Mass: mass, PostShift: shift ?? GaugeShift.None);
    private static Arr<Arr<double>> ConstantNullspace(int dimension) =>
        new([new Arr<double>([.. Enumerable.Repeat(element: 1.0, count: Math.Max(val1: 0, val2: dimension))])]);
    internal int NullspaceDim => Switch(
        pin: static p => p.Indices.Count,
        meanZeroDeflation: static d => d.Nullspace.Count,
        lagrangeKKT: static k => k.Nullspace.Count);
    internal GaugeShift Shift => Switch(
        pin: static p => p.PostShift,
        meanZeroDeflation: static d => d.PostShift,
        lagrangeKKT: static k => k.PostShift);
    internal GaugeSolverKind SolverKind => Switch(
        pin: static _ => GaugeSolverKind.SparseCholeskyReduced,
        meanZeroDeflation: static _ => GaugeSolverKind.SparseBiCgStabDeflated,
        lagrangeKKT: static _ => GaugeSolverKind.SparseLuKkt);
}
```

## [03]-[DENSE_OWNERS]

- Owner: `Matrix` the dense row-major owner (`Dimension Rows` × `Dimension Cols` over `Arr<double>` — dimensions are admitted counts, entries an immutable flat span) and `SymmetricMatrix` the packed-upper symmetric owner (`n(n+1)/2` storage, `FlatIndex` triangular addressing) — both `Of`-gated on shape and one-pass span finiteness; `SvdResult`/`LuResult`/`QrResult`/`CholeskyResult` the decomposition carriers, with `LuResult`/`CholeskyResult` holding the live MathNet factorization `internal` so repeated right-hand sides stream through one factor (the held-handle law) while only typed receipts cross the public surface.
- Cases: `Matrix` operations `Of` · `Identity` · `Transpose` · `Multiply` · `Inverse` · `PseudoInverse` · `DecomposeEigenDetailed` (complex general) · `DecomposeLu`/`DecomposeQr`/`DecomposeSvd` · `Norm(MatrixNormKind)` · `Trace` · `Determinant` · `Spectral` (σ₀) · `SolveDetailed` (square LU) · `LeastSquaresDetailed` (QR) · `Rank` · `At`/`With`; `SymmetricMatrix` `Of` · `ToDense` · `DecomposeEigenDetailed`/`DecomposeEigen` · `DecomposeCholesky` · `At`/`With`.
- Entry: every fallible operation returns `Fin<T>` threading `Op? key = null`; `Matrix.Of(rows, cols, entries)` gates `entries.Count == rows·cols` then `TensorPrimitives.IsFiniteAll` over the flat span — the one-pass vectorized admission, never a strided per-element loop.
- Auto: `Norm` dispatches through the `MatrixNormKind` compute column; `Spectral` reads σ₀ off a fresh SVD; `SymmetricMatrix.At(i, j)` folds `(min, max)` into the triangular index so the packed store is symmetric by construction — a written entry mirrors automatically.
- Receipt: `SvdResult(U, Sigma, V, Rank)` — `Sigma` descending, `Rank` the MathNet conditioning rank; `LuResult(Source, Determinant, Factor)` with `SolveDetailed` streaming through the held factor; `QrResult(Q, R)`; `CholeskyResult(L, Source, Factor)` with `SolveDetailed`. All on the rails `ValidityClaim.All` fold.
- Packages: MathNet.Numerics (`DenseMatrix.Build.DenseOfRowMajor`, `.Svd(computeVectors:)`, `.LU()`, `.QR(QRMethod.Full)`, `.Cholesky()`, `.Evd(Symmetricity.Symmetric/Asymmetric)`, `.PseudoInverse()`, norms), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll` admission), LanguageExt.Core, Thinktecture.Runtime.Extensions (`Dimension` composition).
- Growth: a new dense decomposition is one `Decompose*` member returning a typed carrier plus one `SolvePath` row — never a sibling matrix type; a norm is one `MatrixNormKind` row.
- Boundary: MathNet types never cross the public surface — `Matrix`/`Arr<double>` in, typed receipts out, and the `internal` factorization handles are the held-handle exception that stays inside the assembly; `Inverse()` in a hot loop is the named anti-pattern — solve through the held `LuResult` factor instead; symmetric consumers construct `SymmetricMatrix`, never a dense `Matrix` asserted symmetric, because MathNet's `IsSymmetric()` compares entries with exact `!=` and accumulation-built operators fail it.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Matrix(Dimension Rows, Dimension Cols, Arr<double> Entries) {
    public static Fin<Matrix> Of(Dimension rows, Dimension cols, Arr<double> entries, Op? key = null) =>
        from _ in guard(entries.Count == rows.Value * cols.Value, key.OrDefault().InvalidInput()).ToFin()
        from finite in guard(TensorPrimitives.IsFiniteAll<double>(entries.AsSpan()), key.OrDefault().InvalidInput()).ToFin()
        select new Matrix(Rows: rows, Cols: cols, Entries: entries);
    public static Matrix Identity(Dimension dim) =>
        MatrixKernel.FromMathNet(m: DenseMatrixD.CreateIdentity(order: dim.Value), rows: dim, cols: dim);
    public bool IsValid => Entries.Count == Rows.Value * Cols.Value && TensorPrimitives.IsFiniteAll<double>(Entries.AsSpan());
    public double Frobenius => MatrixNormKind.Frobenius.Compute(matrix: this);
    public Matrix Transpose() => MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Transpose(), Cols, Rows);
    public Fin<Matrix> Multiply(Matrix other, Op? key = null) =>
        !IsValid || !other.IsValid || Cols.Value != other.Rows.Value
            ? Fin.Fail<Matrix>(error: key.OrDefault().InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: other.Cols, key: key.OrDefault(), project: left => left.Multiply(MatrixKernel.ToMathNet(other)));
    public Fin<Matrix> Inverse(Op? key = null) =>
        Rows.Value != Cols.Value
            ? Fin.Fail<Matrix>(error: key.OrDefault().InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: Cols, key: key.OrDefault(), project: static matrix => matrix.Inverse());
    public Fin<Matrix> PseudoInverse(Op? key = null) =>
        MatrixKernel.DenseResult(source: this, rows: Cols, cols: Rows, key: key.OrDefault(), project: static matrix => matrix.PseudoInverse());
    public Fin<EigenSolveReceipt<Complex, Arr<Complex>>> DecomposeEigenDetailed(Op? key = null) => MatrixKernel.GeneralEigen(matrix: this, key: key.OrDefault());
    public Fin<LuResult> DecomposeLu(Op? key = null) => MatrixKernel.Lu(matrix: this, key: key.OrDefault());
    public Fin<QrResult> DecomposeQr(Op? key = null) => MatrixKernel.Qr(matrix: this, key: key.OrDefault());
    public Fin<SvdResult> DecomposeSvd(Op? key = null) => MatrixKernel.Svd(matrix: this, key: key.OrDefault());
    public Fin<double> Norm(MatrixNormKind kind, Op? key = null) =>
        kind is null ? Fin.Fail<double>(error: key.OrDefault().InvalidInput()) : key.OrDefault().AcceptValue(value: kind.Compute(matrix: this));
    public Fin<double> Trace(Op? key = null) => Rows.Value != Cols.Value ? Fin.Fail<double>(key.OrDefault().InvalidInput()) : key.OrDefault().AcceptValue(value: MatrixKernel.ToMathNet(this).Trace());
    public Fin<double> Determinant(Op? key = null) => MatrixKernel.Determinant(matrix: this, key: key.OrDefault());
    public Fin<double> Spectral(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Bind(svd => key.OrDefault().AcceptValue(value: svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0]));
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.Solve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<SolveReceipt> LeastSquaresDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.LeastSquares(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<int> Rank(Op? key = null) => DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.Rank);
    internal double At(int i, int j) => Entries[(i * Cols.Value) + j];
    internal Matrix With(int i, int j, double value) => this with { Entries = Entries.SetItem((i * Cols.Value) + j, value) };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SymmetricMatrix(Dimension Dimension, Arr<double> Upper) : IValidityEvidence {
    public static Fin<SymmetricMatrix> Of(Dimension dim, Arr<double> upper, Op? key = null) =>
        from _ in guard(upper.Count == dim.Value * (dim.Value + 1) / 2, key.OrDefault().InvalidInput()).ToFin()
        from finite in guard(TensorPrimitives.IsFiniteAll<double>(upper.AsSpan()), key.OrDefault().InvalidInput()).ToFin()
        select new SymmetricMatrix(Dimension: dim, Upper: upper);
    public bool IsValid => Upper.Count == (Dimension.Value * (Dimension.Value + 1) / 2) && TensorPrimitives.IsFiniteAll<double>(Upper.AsSpan());
    public Matrix ToDense() {
        SymmetricMatrix self = this;
        int dim = Dimension.Value;
        return new(Rows: Dimension, Cols: Dimension, Entries: [.. toSeq(Enumerable.Range(start: 0, count: dim * dim)).Map(idx => self.At(i: idx / dim, j: idx % dim))]);
    }
    public Fin<EigenSolveReceipt<double, Arr<double>>> DecomposeEigenDetailed(Op? key = null) => MatrixKernel.SymmetricEigen(matrix: this, key: key.OrDefault());
    public Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        DecomposeEigenDetailed(key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<CholeskyResult> DecomposeCholesky(Op? key = null) => MatrixKernel.Cholesky(matrix: this, key: key.OrDefault());
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        this with { Upper = Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value) };
    private static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V, int Rank) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(U.IsValid && V.IsValid),
        ValidityClaim.Of(Sigma.All(static value => double.IsFinite(value) && value >= 0.0)),
        ValidityClaim.CountAtLeast(count: Rank, floor: 0));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult {
    internal LuResult(Matrix source, double determinant, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor) { Source = source; Determinant = determinant; Factor = factor; }
    public Matrix Source { get; }
    public double Determinant { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.LU<double> Factor { get; }
    public bool IsValid => Source.IsValid && double.IsFinite(Determinant);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.LuSolve(lu: this, rhs: rhs, key: key.OrDefault());
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct QrResult(Matrix Q, Matrix R) { public bool IsValid => Q.IsValid && R.IsValid; }

[StructLayout(LayoutKind.Auto)]
public readonly record struct CholeskyResult {
    internal CholeskyResult(Matrix l, Matrix source, MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor) { L = l; Source = source; Factor = factor; }
    public Matrix L { get; }
    public Matrix Source { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> Factor { get; }
    public bool IsValid => L.IsValid && Source.IsValid && L.Rows.Value == L.Cols.Value && Source.Rows.Value == Source.Cols.Value;
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.CholeskySolve(cholesky: this, rhs: rhs, key: key.OrDefault());
}
```

## [04]-[SPARSE_OWNERS]

- Owner: `SparseMatrix` the CSR owner (`RowPtr`/`ColInd`/`Values` over `Arr` — immutable compressed rows) whose `IsValid` enforces the FULL structural contract: pointer bounds, monotone row pointers (`RowPointersAreMonotone`), strictly-increasing in-bounds columns per row (`RowColumnsAreStrict`), and span finiteness — factorizing invalid storage produces silently wrong factors, so the invariant is the admission; `SparseHermitian` the complex upper-store owner (row ≤ col triplets only, conjugate reconstruction on multiply, real-diagonal gate); `CholeskySparse` the SPD factor cache — CSparse `SparseCholesky` under AMD (`ColumnOrdering.MinimumDegreeAtPlusA`) with a `Lock`-guarded `Solve` because the factorization's constructor-allocated scratch is non-reentrant, `FactorNonZeros` as the symbolic-fill witness, and success-only construction so a broken factor never enters reuse.
- Cases: `SparseMatrix` `FromTriplets` (duplicate-summing assembly) · `Multiply` · `ToDense` · `SolveDetailed`/`Solve` (BiCgStab + fallback) · `SingularSolveDetailed`/`SingularSolve` (gauge) · `SolveIndefiniteDetailed`/`SolveIndefinite` (SparseLU, pivot tol ∈ [0,1]) · `SmallestEigenpairsDetailed` (LOBPCG) · `GeneralizedEigenpairsDetailed` (A z = λ M z by Cholesky congruence — the `Meshing/dec` spectral-basis entry) · `NonZeros`; `SparseHermitian` `FromTriplets` · `Multiply` · `SmallestEigenpairsDetailed` (Hermitian LOBPCG); `CholeskySparse` `Of` · `SolveDetailed`/`Solve` · `FactorNonZeros`.
- Entry: `FromTriplets` admits ANY triplet stream — out-of-range or non-finite entries fail typed, duplicates sum, exact zeros drop, rows compress sorted — so consumers assemble by accumulation and never hand-build CSR; `CholeskySparse.Of` symmetrizes through the normalized upper-entry view and catches the CSparse bare-`Exception` pivot-loss throw into the typed rail (`key.Catch`).
- Auto: `SparseHermitian.IsValid` additionally gates the stored diagonal real within a scale-relative tolerance, so a drifted Hermitian assembly is caught at the owner, not inside LOBPCG; `AssembleHermitian` applies the SAME scale-relative band to the SUMMED diagonal before coercing it exactly real — conjugate-pair accumulation cancels instead of failing an absolute gate.
- Receipt: solve receipts per [05]; `CholeskySparse` itself is the cached evidence (source + factor + order + fill).
- Packages: CSparse (`CSparse.Double.SparseMatrix.OfIndexed`, `CSparse.Double.Factorization.SparseCholesky.Create`, `SparseLU.Create`, `ColumnOrdering.MinimumDegreeAtPlusA`), MathNet.Numerics (`SparseMatrix.OfIndexed`, `SparseCompressedRowMatrixStorage.OfCompressedSparseRowFormat` + `Build.Sparse(storage)`, BiCgStab + criterion stack), Rasm.Domain (`Admit.FiniteComplexSpan`/`HermitianDiagonalRealSpan` — the one complex-spectrum gate pair), LanguageExt.Core, BCL (`System.Threading.Lock`, `System.Numerics.Complex`).
- Growth: a new sparse capability (rank-1 update, transpose solve, LDL) is one member + one `SolvePath` row over the same owners; a second CSR/CSC representation beside `SparseMatrix` is the deleted form — format bridges live inside `MatrixKernel`.
- Boundary: the `Lock` on `CholeskySparse.Solve` is load-bearing concurrency law — CSparse solves share scratch and a concurrent second solve corrupts both results silently; deleting it on rebuild is the named correctness defect. The mesh Laplacian memoization (`Meshing/mesh`'s `LaplacianCache`) caches THESE factor objects — identity and locking semantics compose from here.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SparseMatrix(Dimension Rows, Dimension Cols, Arr<int> RowPtr, Arr<int> ColInd, Arr<double> Values) {
    public static Fin<SparseMatrix> FromTriplets(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(triplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleSparse(rows: rows, cols: cols, triplets: active, op: op));
    }
    public bool IsValid =>
        RowPtr.Count == Rows.Value + 1 && ColInd.Count == Values.Count && TensorPrimitives.IsFiniteAll<double>(Values.AsSpan())
        && RowPtr[0] == 0 && RowPtr[Rows.Value] == Values.Count
        && RowPointersAreMonotone(RowPtr) && RowColumnsAreStrict(rowPtr: RowPtr, colInd: ColInd, minCol: static _ => 0, maxCol: Cols.Value);
    public int NonZeros => Values.Count;
    public Fin<Arr<double>> Multiply(Arr<double> vector, Op? key = null) =>
        !IsValid || vector.Count != Cols.Value || !TensorPrimitives.IsFiniteAll<double>(vector.AsSpan())
            ? Fin.Fail<Arr<double>>(key.OrDefault().InvalidInput())
            : MatrixKernel.SparseMatVec(self: this, x: vector, key: key.OrDefault());
    public Matrix ToDense() => MatrixKernel.SparseToDense(self: this);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.SparseSolve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) => SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<SolveReceipt> SingularSolveDetailed(Arr<double> rhs, GaugePolicy gauge, Context context, Op? key = null) =>
        MatrixKernel.SingularGaugeSolve(matrix: this, rhs: rhs, gauge: gauge, context: context, key: key.OrDefault());
    public Fin<Arr<double>> SingularSolve(Arr<double> rhs, GaugePolicy gauge, Context context, Op? key = null) =>
        SingularSolveDetailed(rhs: rhs, gauge: gauge, context: context, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<SolveReceipt> SolveIndefiniteDetailed(Arr<double> rhs, double pivotTolerance = 1.0, Op? key = null) =>
        MatrixKernel.SparseLuSolve(matrix: this, rhs: rhs, pivotTolerance: pivotTolerance, key: key.OrDefault());
    public Fin<Arr<double>> SolveIndefinite(Arr<double> rhs, double pivotTolerance = 1.0, Op? key = null) =>
        SolveIndefiniteDetailed(rhs: rhs, pivotTolerance: pivotTolerance, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<EigenSolveReceipt<double, Arr<double>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.Lobpcg(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    public Fin<EigenSolveReceipt<double, Arr<double>>> GeneralizedEigenpairsDetailed(SparseMatrix mass, int k, Op? key = null) =>
        MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: this, mass: mass, k: k, key: key.OrDefault());
    internal static bool RowPointersAreMonotone(Arr<int> rowPtr) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1))).ForAll(i => rowPtr[i] <= rowPtr[i + 1]);
    internal static bool RowColumnsAreStrict(Arr<int> rowPtr, Arr<int> colInd, Func<int, int> minCol, int maxCol) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(row => toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row]))
                .Fold(initialState: (Ok: true, Prev: minCol(arg: row) - 1), f: (state, k) => (
                    Ok: state.Ok && colInd[k] >= minCol(arg: row) && colInd[k] < maxCol && colInd[k] > state.Prev,
                    Prev: colInd[k])).Ok);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian(Dimension Order, Arr<int> RowPtr, Arr<int> ColInd, Arr<Complex> Values) {
    public static Fin<SparseHermitian> FromTriplets(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> upperTriplets, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(upperTriplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleHermitian(order: order, triplets: active, op: op));
    }
    // Upper-triangular storage; Multiply reconstructs the lower triangle by conjugate transpose.
    public bool IsValid =>
        RowPtr.Count == Order.Value + 1 && ColInd.Count == Values.Count && Admit.FiniteComplexSpan(Values.AsSpan())
        && RowPtr[0] == 0 && RowPtr[Order.Value] == Values.Count
        && SparseMatrix.RowPointersAreMonotone(RowPtr)
        && SparseMatrix.RowColumnsAreStrict(rowPtr: RowPtr, colInd: ColInd, minCol: static row => row, maxCol: Order.Value)
        && Admit.HermitianDiagonalRealSpan(DiagonalEntries().AsSpan());
    public int NonZeros => Values.Count;
    public Fin<Arr<Complex>> Multiply(Arr<Complex> vector, Op? key = null) =>
        !IsValid || vector.Count != Order.Value || !Admit.FiniteComplexSpan(vector.AsSpan())
            ? Fin.Fail<Arr<Complex>>(key.OrDefault().InvalidInput())
            : MatrixKernel.HermitianMatVec(self: this, x: vector, key: key.OrDefault());
    public Fin<EigenSolveReceipt<double, Arr<Complex>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.LobpcgHermitian(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    private Complex[] DiagonalEntries() {
        Arr<int> rowPtr = RowPtr;
        Arr<int> colInd = ColInd;
        Arr<Complex> values = Values;
        return [.. Enumerable.Range(start: 0, count: Order.Value).SelectMany(row => Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row])
            .Where(k => colInd[k] == row)
            .Select(k => values[k]))];
    }
}

public sealed record CholeskySparse {
    private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) { Source = source; Factor = factor; Order = order; }
    private readonly Lock solveLock = new();
    public static Fin<CholeskySparse> Of(SparseMatrix symmetric, Op? key = null) =>
        symmetric.Rows.Value != symmetric.Cols.Value || !symmetric.IsValid
            ? Fin.Fail<CholeskySparse>(error: key.OrDefault().InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric, key: key.OrDefault())
              from factor in key.OrDefault().Catch(() =>
                  Fin.Succ(CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA)))
              select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
    public SparseMatrix Source { get; }
    internal CSparse.Double.Factorization.SparseCholesky Factor { get; }
    public Dimension Order { get; }
    public int FactorNonZeros => Factor.NonZerosCount;
    public bool IsValid => Source.IsValid && Factor.NonZerosCount > 0 && Order.Value > 0;
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        !IsValid || !MatrixKernel.SolveInputIsValid(rows: Order.Value, rhs: rhs)
            ? Fin.Fail<SolveReceipt>(error: key.OrDefault().InvalidInput())
            : key.OrDefault().Catch(() => {
                double[] b = [.. rhs.AsIterable()];
                double[] x = new double[Order.Value];
                lock (solveLock) {
                    Factor.Solve(input: b.AsSpan(), result: x.AsSpan());
                }
                Arr<double> solution = new(x);
                return MatrixKernel.SparseSymmetricResidual(matrix: Source, solution: solution, rhs: rhs, key: key.OrDefault()).Bind(residual =>
                    MatrixKernel.SolveSuccess(solution: solution, solutionLength: Order.Value, path: SolvePath.SparseCholesky, stop: SolveStop.DirectSolved, rows: Source.Rows, cols: Source.Cols, rhsLength: rhs.Count, residual: residual, key: key.OrDefault(), inputNonZeros: Some(Source.NonZeros), factorNonZeros: Some(Factor.NonZerosCount)));
            });
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
}
```

## [05]-[RECEIPTS]

- Owner: `SolveReceipt` the one linear-solve evidence carrier (solution + `SolvePath`/`SolveStop` + dimensions + iteration/tolerance witnesses + the recomputed true relative residual + optional rank/fill/gauge evidence), `EigenSolveReceipt<TEigen, TVector>` the one eigen carrier generic over real/complex eigenvalues and real/complex vectors (pairs + path/stop + requested/returned + `MaxResidual`), `GaugeReceipt` the singular-solve evidence (solver kind, declared and numeric nullspace dimensions, operator scale, compatibility/post-gauge/M-weighted/relative residuals, pin rows, post-shift applied, rhs mutation and multiplier norms, orthogonality check, regularization shift, breakdown flag) — all three spelling the rails fold `public bool IsValid => ValidityClaim.All(…)` with `IValidityEvidence` conformance.
- Entry: receipts are minted only by the `MatrixKernel` exits (`SolveSuccess`, `EigenReceiptOf`) under the two-tier evidence law — hard numerical garbage (non-finite residual, cap-exceeded residual, wrong-length or non-finite solution) never mints and fails typed; a usable-stop receipt is gated valid before release; a non-usable-stop receipt (`RankDeficient`, `IterativeExhausted`, KKT breakdown) is the WITNESSED refusal, returned as evidence the caller reads off `Stop.IsUsable`/`IsValid` before consuming the solution. Treating any minted receipt as a usable result without reading its stop is the named consumer defect.
- Auto: validity is the ONE `rails.md` fold spelled explicitly — each receipt's `IsValid` is `ValidityClaim.All(…)` over authored claim rows: the mechanical gates off field shape (every `double` witness `Finite`/`Nonnegative`, every count nonnegative, every nested evidence field valid-when-some, every vocabulary field non-null) conjoined with the SEMANTIC couplings (residual within tolerance, solution length matches columns, iterations within budget, returned ≤ requested). The mature ~12-to-18-term hand-rolled boolean conjunction litany is the deleted form; the claim rows state each invariant once, one row per witness.
- Receipt: these ARE the receipts — the typed evidence law: fields carry route, status, sampling, and solver evidence, so they stay typed records, never a generic ledger.
- Packages: Rasm.Domain (project — `IValidityEvidence` + `ValidityClaim`, the rails validity floor), LanguageExt.Core (`Option`, `Seq`, `Arr`).
- Growth: new evidence is one field plus at most one claim row; a new outcome family is one receipt type ONLY when its evidence shape is disjoint (the eigen/solve/gauge split), never per-algorithm receipt clones.
- Boundary: `Option<T>` carries absence of evidence (`Iterations` on direct solves), never sentinel values; the residual stored is ALWAYS recomputed against the original operator — a preconditioned or factor-reconstructed residual is the named lying witness.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SolveReceipt(
    Arr<double> Solution, SolvePath Path, SolveStop Stop, Dimension Rows, Dimension Cols, int RhsLength,
    Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, double Residual,
    Option<bool> FullRank, Option<int> InputNonZeros, Option<int> FactorNonZeros, Option<GaugeReceipt> Gauge = default) : IValidityEvidence {
    public bool IsValid {
        get {
            SolvePath path = Path;
            Option<int> maxIterations = MaxIterations;
            return ValidityClaim.All(
                ValidityClaim.Of(Path is not null && Stop is not null && Stop.IsUsable),
                ValidityClaim.CountExactly(count: RhsLength, expected: Rows.Value),
                ValidityClaim.Nonnegative(value: Residual),
                ValidityClaim.Of(Residual <= Tolerance.IfNone(double.PositiveInfinity)),
                ValidityClaim.CountExactly(count: Solution.Count, expected: Cols.Value),
                ValidityClaim.Finite(Solution.AsSpan()),
                ValidityClaim.Of(Iterations.Map(static iter => iter >= 0).IfNone(noneValue: true) && InputNonZeros.Map(static nz => nz >= 0).IfNone(noneValue: true) && FactorNonZeros.Map(static nz => nz >= 0).IfNone(noneValue: true)),
                ValidityClaim.Of(FullRank.Map(rank => !path.Equals(SolvePath.DenseQrLeastSquares) || rank).IfNone(noneValue: true)),
                ValidityClaim.Of(FactorNonZeros.Map(nz => !path.Equals(SolvePath.SparseCholesky) || nz > 0).IfNone(noneValue: true)),
                ValidityClaim.Of(Iterations.Map(iter => maxIterations.Map(max => max >= iter).IfNone(noneValue: true)).IfNone(noneValue: true)),
                ValidityClaim.Of(Gauge.Map(static gauge => gauge.IsValid).IfNone(noneValue: true)));
        }
    }
}

public readonly record struct EigenSolveReceipt<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path, EigenSolveStop Stop,
    int RequestedPairs, int ReturnedPairs, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance,
    double MaxResidual, Option<int> FactorNonZeros = default) : IValidityEvidence {
    public bool IsValid {
        get {
            Option<int> maxIterations = MaxIterations;
            return ValidityClaim.All(
                ValidityClaim.Of(Path is not null && Stop is not null && Stop.IsUsable),
                ValidityClaim.Of(RequestedPairs >= 1 && ReturnedPairs > 0 && ReturnedPairs <= RequestedPairs && Pairs.Count == ReturnedPairs),
                ValidityClaim.Nonnegative(value: MaxResidual),
                ValidityClaim.Of(MaxResidual <= Tolerance.IfNone(double.PositiveInfinity)),
                ValidityClaim.Of(Iterations.Map(iter => maxIterations.Map(max => max >= iter).IfNone(noneValue: true)).IfNone(noneValue: true)),
                ValidityClaim.Of(FactorNonZeros.Map(static count => count > 0).IfNone(noneValue: true)));
        }
    }
}

public readonly record struct GaugeReceipt(
    GaugeSolverKind Solver, int NullspaceDim, Option<int> NullspaceDimNumeric,
    double OperatorFrobeniusScale, double ResidualCompatibility, bool RhsProjected, double ResidualAfterGauge, double ResidualAfterGaugeM,
    double ResidualRelative, Option<int> PinnedIndex, Arr<int> PinIndices, int ConstraintRows, GaugeShift PostShiftApplied,
    double RhsMutationNorm, double MultiplierNorm, Option<int> Iterations, double GaugeOrthogonalityCheck, double RegularizationEpsUsed,
    bool NumericalBreakdown) : IValidityEvidence {
    public bool IsValid {
        get {
            int nullspaceDim = NullspaceDim;
            return ValidityClaim.All(
                ValidityClaim.Of(Solver is not null && PostShiftApplied is not null),
                ValidityClaim.Of(NullspaceDim >= 0 && ConstraintRows >= 0),
                ValidityClaim.Of(PinIndices.ForAll(static index => index >= 0)),
                ValidityClaim.Positive(value: OperatorFrobeniusScale),
                ValidityClaim.Nonnegative(value: ResidualCompatibility),
                ValidityClaim.Nonnegative(value: ResidualAfterGauge),
                ValidityClaim.Nonnegative(value: ResidualAfterGaugeM),
                ValidityClaim.Nonnegative(value: ResidualRelative),
                ValidityClaim.Nonnegative(value: RhsMutationNorm),
                ValidityClaim.Nonnegative(value: MultiplierNorm),
                ValidityClaim.Nonnegative(value: GaugeOrthogonalityCheck),
                ValidityClaim.Nonnegative(value: RegularizationEpsUsed),
                ValidityClaim.Of(PinnedIndex.Map(static index => index >= 0).IfNone(noneValue: true) && Iterations.Map(static iter => iter >= 0).IfNone(noneValue: true)),
                ValidityClaim.Of(NullspaceDimNumeric.Map(count => count <= nullspaceDim).IfNone(noneValue: true)));
        }
    }
}
```

## [06]-[SOLVE_KERNEL]

- Owner: `MatrixKernel` the `internal static` numeric kernel — the ONE MathNet + CSparse access path in the corpus. Members by family: bridges (`ToMathNet`/`FromMathNet` row-major, `ToMathNetComplex`, `ToMathNetHermitian` conjugate-expanding, `ToMathNetSparse` CSR-storage, `ToCSparseSymmetric` via `NormalizeSymmetricUpperEntries` — which rejects contradictory duplicate mirror entries beyond a scale-relative band), dense decompositions (`Svd`/`Lu`/`Qr`/`Cholesky`/`SymmetricEigen`/`GeneralEigen` — symmetric pairs ordered by |λ| descending), dense solves (`Solve` LU, `LeastSquares` full QR with rank-deficiency stop, `LuSolve`/`CholeskySolve` through held factors with `√SqrtEpsilon` residual caps), sparse assembly (`AssembleSparse` duplicate-summing + zero-dropping + `CompressRows`, `AssembleHermitian` upper-only with real-diagonal gate, `AddHermitianRealBlockTriplets` the 2n×2n real-block embedding rows the connection-Laplacian assembly composes), sparse solves (`SparseSolve` — diagonal-preconditioned BiCgStab under the explicit criterion stack Failure→Divergence→Residual→IterationCount, falling back to the MathNet direct solve with the fallback recorded as `SparseMathNetDirectFallback`/`DirectFallbackSolved` and gated at the looser `√SqrtEpsilon` cap; `SparseLuSolve` — CSparse SparseLU under AMD with column-relative pivot tolerance for symmetric-indefinite systems), the gauge family (`SingularGaugeSolve` + `SolvePin`/`SolveMeanZeroDeflation`/`SolveKkt` + the M-orthogonal primitive set `DeflateRhs`/`ProjectRange`/`MassResidual`/`GaugeOrthogonality`/`ApplyShift`/`RegularizedGramSolve`), `GeneralizedEigenpairsDetailed` (A z = λ M z via the symmetric Cholesky congruence `L⁻¹AL⁻ᵀ`, real and complex variants sharing `CongruentReduce`/`BackTransform`), and the LOBPCG family (`Lobpcg`/`LobpcgHermitian` over one generic `LobpcgCore`).
- Entry: every public-facing operation enters through the owning model member ([03]/[04]); the kernel is reached only through them.
- Auto: `SingularGaugeSolve` derives every threshold from `OperatorFrobeniusScale` and the rhs scale (`context.Fractional` relative gates — never absolute literals), projects the rhs onto range(A) only when the compatibility residual demands it, applies the case solver through one `Switch`, post-shifts through `GaugeShift`, witnesses BOTH the Euclidean and the M-weighted relative residuals against the original un-shifted operator, and reports the stage's ACTUAL `SolvePath` on the minted receipt (the KKT saddle assembles from the exact mirrored upper entries — a dense sweep or magnitude prune of operator entries is the deleted form); `RegularizedGramSolve` factors the SPD Gram, applying a diagonal-scaled Tikhonov shift ONLY on Cholesky breakdown (MathNet throws bare `Exception` on pivot loss — caught broadly at this one boundary, the algorithms-route exemption) and surfaces the applied shift plus the numeric rank read from the factor diagonal; `LobpcgCore` iterates span([X|W|P]) Rayleigh-Ritz: residual `R = AX − XΛ`, Jacobi-preconditioned `W`, one modified-Gram-Schmidt pass with rank-collapsed columns dropped (`SurvivingColumns`) and the reduced Ritz vectors scattered back (`ScatterRows`) so the block offsets survive — fewer survivors than `k` terminates typed as `MaxIterationsExhausted`, NEVER a hidden dense fallback; the initial basis is deterministic — `Deterministic.NextSignedUnit`/`NextSignedComplexUnit` (the `Domain/identity` splitmix64 owner; seeds 17 real / 19 Hermitian) orthonormalized, so eigen results replay bit-stable per provider.
- Receipt: every path exits through `SolveSuccess`/`EigenReceiptOf`/`LobpcgReceiptOf` which mint the [05] receipts gated on their own validity.
- Packages: MathNet.Numerics (factorizations, `SolveIterative` + `BiCgStab` + `DiagonalPreconditioner` + criterion stack, `Evd`, `Cholesky` — the managed provider path ONLY: no MathNet native provider ships an osx-arm64 asset, so the kernel references no provider package and no `Control.UseNative*` call exists; provider selection stays `UseManaged`), CSparse (`SparseCholesky`, `SparseLU`, AMD ordering), Rasm.Domain (project — `Op`, `Context`, `Deterministic` splitmix64 derivation), System.Numerics.Tensors, TYoshimura.DoubleDouble (`ddouble` — the 106-bit lane `CompensatedNorm` folds every recorded residual witness through, so cancellation in `b − Ax` cannot lie in the evidence), BCL (`System.Numerics.Complex`).
- Growth: a new solve route is one kernel member + one `SolvePath` row + receipt fields it already carries; a new gauge case is one `GaugePolicy` case + one `Solve*` arm; a new eigen substrate (shift-invert Lanczos, spectra-style transforms) is one `EigenSolvePath` row over the same receipt.
- Boundary: this kernel is the ONE linear-algebra access path — `Processing/register`'s GICP precision-field and spectrum-rebuild math route through `Matrix`/`SymmetricMatrix` owners, and a direct `DenseMatrix`/`Evd`/`Cholesky` reach in a sibling page is the named deleted form; statement loops inside `CompressRows`, `SolvePin`, `SolveKkt`, and the MGS pass are the named statement-kernel exemption (measured assembly and elimination hot paths); `BiCgStabDivergenceFactor = 1e3` and `KktPivotTolerance = 1.0` are named kernel policy constants — the divergence criterion's relative-increase ceiling and CSparse's full-partial-pivot column threshold.

```csharp
// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MatrixKernel {
    // Relative-increase ceiling for the BiCgStab divergence criterion; full-partial-pivot
    // column-relative threshold for the KKT saddle SparseLU (CSparse tol in [0,1]).
    private const double BiCgStabDivergenceFactor = 1e3;
    private const double KktPivotTolerance = 1.0;

    // --- [BRIDGES] ----------------------------------------------------------------------------
    internal static DenseMatrixD ToMathNet(Matrix m) =>
        (DenseMatrixD)DenseMatrixD.Build.DenseOfRowMajor(m.Rows.Value, m.Cols.Value, m.Entries.AsIterable());
    internal static Matrix FromMathNet(Matrix<double> m, Dimension rows, Dimension cols) =>
        new(Rows: rows, Cols: cols, Entries: new Arr<double>(m.ToRowMajorArray()));
    internal static Fin<Matrix> DenseResult(Matrix source, Dimension rows, Dimension cols, Op key, Func<Matrix<double>, Matrix<double>> project) =>
        !source.IsValid
            ? Fin.Fail<Matrix>(error: key.InvalidInput())
            : key.Catch(() => {
                Matrix result = FromMathNet(m: project(arg: ToMathNet(source)), rows: rows, cols: cols);
                return result.IsValid ? Fin.Succ(result) : Fin.Fail<Matrix>(key.InvalidResult());
            });
    internal static Fin<CSparse.Storage.CompressedColumnStorage<double>> ToCSparseSymmetric(SparseMatrix s, Op key) =>
        NormalizeSymmetricUpperEntries(s: s, key: key).Map(upper =>
            CSparse.Double.SparseMatrix.OfIndexed(rows: s.Rows.Value, columns: s.Rows.Value, enumerable: upper));
    // Rejects contradictory duplicate mirror entries beyond a scale-relative band, then yields the
    // canonical (row <= col) upper view every symmetric consumer shares.
    private static Fin<List<(int Row, int Col, double Value)>> NormalizeSymmetricUpperEntries(SparseMatrix s, Op key) {
        if (!s.IsValid || s.Rows.Value != s.Cols.Value) return Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput());
        List<(int Row, int Col, double[] Values)> grouped = [.. Enumerable.Range(start: 0, count: s.Rows.Value)
            .SelectMany(row => Enumerable.Range(start: s.RowPtr[row], count: s.RowPtr[row + 1] - s.RowPtr[row])
                .Select(k => (Row: Math.Min(val1: row, val2: s.ColInd[k]), Col: Math.Max(val1: row, val2: s.ColInd[k]), Value: s.Values[k])))
            .GroupBy(static e => (e.Row, e.Col))
            .Select(static group => (group.Key.Row, group.Key.Col, Values: group.Select(static e => e.Value).ToArray()))];
        return grouped.Exists(static group => group.Values.Any(value => Math.Abs(value - group.Values[0]) > EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value), val2: Math.Abs(group.Values[0])))))
            ? Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput())
            : Fin.Succ(grouped.Select(static group => (group.Row, group.Col, Value: group.Values[0])).OrderBy(static e => e.Row).ThenBy(static e => e.Col).ToList());
    }
    private static DenseMatrixC ToMathNetComplex(Matrix m) =>
        (DenseMatrixC)DenseMatrixC.Build.Dense(m.Rows.Value, m.Cols.Value, (i, j) => new Complex(m.At(i: i, j: j), 0.0));
    private static Matrix<Complex> ToMathNetHermitian(SparseHermitian s) =>
        SparseMatrixC.OfIndexed(rows: s.Order.Value, columns: s.Order.Value, enumerable: Enumerable.Range(start: 0, count: s.Order.Value)
            .SelectMany(row => Enumerable.Range(start: s.RowPtr[row], count: s.RowPtr[row + 1] - s.RowPtr[row])
                .SelectMany(k => row == s.ColInd[k]
                    ? [(row, row, s.Values[k])]
                    : new[] { (row, s.ColInd[k], s.Values[k]), (s.ColInd[k], row, Complex.Conjugate(s.Values[k])) })));
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) =>
        DenseMatrixD.Build.Sparse(SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            rows: s.Rows.Value, columns: s.Cols.Value, valueCount: s.Values.Count,
            rowPointers: [.. s.RowPtr.AsIterable()], columnIndices: [.. s.ColInd.AsIterable()], values: [.. s.Values.AsIterable()]));
    private static Matrix<double> ToMathNetSymmetric(SparseMatrix matrix, IEnumerable<(int Row, int Col, double Value)> upper) =>
        SparseMatrixD.OfIndexed(rows: matrix.Rows.Value, columns: matrix.Cols.Value, enumerable: upper.SelectMany(static e => e.Row == e.Col
            ? [(e.Row, e.Col, e.Value)]
            : new[] { (e.Row, e.Col, e.Value), (e.Col, e.Row, e.Value) }));
    internal static Matrix SparseToDense(SparseMatrix self) => FromMathNet(m: ToMathNetSparse(s: self), rows: self.Rows, cols: self.Cols);
    private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());
    private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());
    // Complex-spectrum admission is Rasm.Domain's: Admit.FiniteComplexSpan / Admit.HermitianDiagonalRealSpan
    // are the one span-gate pair — a kernel-local re-derivation is the named duplicate-kernel defect.

    // --- [WITNESS] ----------------------------------------------------------------------------
    // The recorded residual is the ONE truth witness — its norm folds accumulate in 106-bit ddouble
    // so ill-conditioned cancellation in b - Ax cannot inflate or deflate the evidence.
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        CompensatedNorm(v: b - a.Multiply(x)) / Math.Max(val1: 1.0, val2: CompensatedNorm(v: b));
    private static double CompensatedNorm(LinearVector v) {
        ddouble sum = 0.0;
        for (int i = 0; i < v.Count; i++) sum += (ddouble)v[i] * v[i];
        return Math.Sqrt(d: (double)sum);
    }
    internal static bool SolveInputIsValid(int rows, Arr<double> rhs) =>
        rhs.Count == rows && TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan());
    internal static double SparseResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs) =>
        RelativeResidual(a: ToMathNetSparse(s: matrix), x: DenseVectorD.OfArray([.. solution.AsIterable()]), b: DenseVectorD.OfArray([.. rhs.AsIterable()]));
    internal static Fin<double> SparseSymmetricResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs, Op key) =>
        NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => key.AcceptValue(value: RelativeResidual(
            a: ToMathNetSymmetric(matrix: matrix, upper: upper),
            x: DenseVectorD.OfArray([.. solution.AsIterable()]),
            b: DenseVectorD.OfArray([.. rhs.AsIterable()]))));
    private static double EigenResidual<T, TEigen, TVector>(Matrix<T> a, Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, Func<TVector, MathNet.Numerics.LinearAlgebra.Vector<T>> vector, Func<(MathNet.Numerics.LinearAlgebra.Vector<T> Vector, TEigen Eigenvalue), MathNet.Numerics.LinearAlgebra.Vector<T>> scale)
        where T : struct, IEquatable<T>, IFormattable =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            MathNet.Numerics.LinearAlgebra.Vector<T> v = vector(arg: pair.Eigenvector);
            return Math.Max(val1: max, val2: (a.Multiply(v) - scale(arg: (v, pair.Eigenvalue))).L2Norm() / Math.Max(val1: 1.0, val2: v.L2Norm()));
        });
    private static double GeneralizedEigenResidual(Matrix<double> stiffness, Matrix<double> mass, Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            LinearVector v = DenseVectorD.OfArray([.. pair.Eigenvector.AsIterable()]);
            LinearVector lhs = stiffness.Multiply(v);
            return Math.Max(val1: max, val2: (lhs - (mass.Multiply(v) * pair.Eigenvalue)).L2Norm() / Math.Max(val1: 1.0, val2: lhs.L2Norm()));
        });
    internal static Fin<SolveReceipt> SolveSuccess(Arr<double> solution, int solutionLength, SolvePath path, SolveStop stop, Dimension rows, Dimension cols, int rhsLength, double residual, Op key, double residualCap = double.PositiveInfinity, Option<int> iterations = default, Option<int> maxIterations = default, Option<double> tolerance = default, Option<bool> fullRank = default, Option<int> inputNonZeros = default, Option<int> factorNonZeros = default, Option<GaugeReceipt> gauge = default) {
        SolveReceipt receipt = new(Solution: solution, Path: path, Stop: stop, Rows: rows, Cols: cols, RhsLength: rhsLength, Iterations: iterations, MaxIterations: maxIterations, Tolerance: tolerance, Residual: residual, FullRank: fullRank, InputNonZeros: inputNonZeros, FactorNonZeros: factorNonZeros, Gauge: gauge);
        return solution.Count == solutionLength && TensorPrimitives.IsFiniteAll<double>(solution.AsSpan()) && double.IsFinite(residual) && residual <= residualCap && (!stop.IsUsable || receipt.IsValid)
            ? Fin.Succ(receipt)
            : Fin.Fail<SolveReceipt>(key.InvalidResult());
    }
    private static Fin<EigenSolveReceipt<TEigen, TVector>> EigenReceiptOf<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, EigenSolveStop stop, int requestedPairs, double maxResidual, Op key, Option<int> iterations = default, Option<int> maxIterations = default, Option<double> tolerance = default, Option<int> factorNonZeros = default) {
        EigenSolveReceipt<TEigen, TVector> receipt = new(Pairs: pairs, Path: path, Stop: stop, RequestedPairs: requestedPairs, ReturnedPairs: pairs.Count, Iterations: iterations, MaxIterations: maxIterations, Tolerance: tolerance, MaxResidual: maxResidual, FactorNonZeros: factorNonZeros);
        return double.IsFinite(maxResidual) && (!stop.IsUsable || receipt.IsValid)
            ? Fin.Succ(receipt)
            : Fin.Fail<EigenSolveReceipt<TEigen, TVector>>(key.InvalidResult());
    }

    // --- [DENSE_DECOMPOSITIONS] -----------------------------------------------------------------
    internal static Fin<SvdResult> Svd(Matrix matrix, Op key) => !matrix.IsValid ? Fin.Fail<SvdResult>(key.InvalidInput()) : key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svd = ToMathNet(matrix).Svd(computeVectors: true);
        SvdResult result = new(U: FromMathNet(svd.U, matrix.Rows, matrix.Rows), Sigma: ArrFromVector(svd.S), V: FromMathNet(svd.VT.Transpose(), matrix.Cols, matrix.Cols), Rank: svd.Rank);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SvdResult>(key.InvalidResult());
    });
    internal static Fin<LuResult> Lu(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<LuResult>(key.InvalidInput())
            : key.Catch(() => {
                MathNet.Numerics.LinearAlgebra.Factorization.LU<double> lu = ToMathNet(matrix).LU();
                LuResult result = new(source: matrix, determinant: lu.Determinant, factor: lu);
                return result.IsValid ? Fin.Succ(result) : Fin.Fail<LuResult>(key.InvalidResult());
            });
    internal static Fin<QrResult> Qr(Matrix matrix, Op key) => !matrix.IsValid ? Fin.Fail<QrResult>(key.InvalidInput()) : key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(matrix).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
        QrResult result = new(Q: FromMathNet(qr.Q, matrix.Rows, matrix.Rows), R: FromMathNet(qr.R, matrix.Rows, matrix.Cols));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<QrResult>(key.InvalidResult());
    });
    internal static Fin<CholeskyResult> Cholesky(SymmetricMatrix matrix, Op key) =>
        !matrix.IsValid ? Fin.Fail<CholeskyResult>(key.InvalidInput()) : key.Catch(() => {
            Matrix source = matrix.ToDense();
            MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor = ToMathNet(source).Cholesky();
            CholeskyResult result = new(l: FromMathNet(factor.Factor, matrix.Dimension, matrix.Dimension), source: source, factor: factor);
            return result.IsValid ? Fin.Succ(result) : Fin.Fail<CholeskyResult>(key.InvalidResult());
        });
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> SymmetricEigen(SymmetricMatrix matrix, Op key) =>
        !matrix.IsValid
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> mathNet = ToMathNet(matrix.ToDense());
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = mathNet.Evd(Symmetricity.Symmetric);
                int n = matrix.Dimension.Value;
                Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: ArrFromVector(evd.EigenVectors.Column(i))))
                    .OrderByDescending(static p => Math.Abs(p.Eigenvalue)));
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseSymmetricEvd, stop: EigenSolveStop.DirectSolved, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector), key: key);
            });
    internal static Fin<EigenSolveReceipt<Complex, Arr<Complex>>> GeneralEigen(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<EigenSolveReceipt<Complex, Arr<Complex>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<Complex> mathNet = ToMathNetComplex(matrix);
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = mathNet.Evd(Symmetricity.Asymmetric);
                int n = matrix.Rows.Value;
                Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: ArrFromComplexVector(evd.EigenVectors.Column(i)))));
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseGeneralEvd, stop: EigenSolveStop.DirectSolved, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue), key: key);
            });
    internal static Fin<double> Determinant(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<double>(error: key.InvalidInput())
            : key.Catch(() => key.AcceptValue(value: ToMathNet(matrix).Determinant()));

    // --- [DENSE_SOLVES] --------------------------------------------------------------------------
    internal static Fin<SolveReceipt> Solve(Matrix matrix, Arr<double> rhs, Op key) =>
        DenseSolveGated(source: matrix, rhs: rhs, key: key, square: true, solve: static (source, right, op) => Lu(matrix: source, key: op).Bind(lu => LuSolve(lu: lu, rhs: right, key: op)));
    internal static Fin<SolveReceipt> LeastSquares(Matrix matrix, Arr<double> rhs, Op key) =>
        DenseSolveGated(source: matrix, rhs: rhs, key: key, square: false, solve: static (source, right, op) => op.Catch(() => {
            MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(source).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
            return DenseSolve(source: source, rhs: right, key: op, path: SolvePath.DenseQrLeastSquares, stop: qr.IsFullRank ? SolveStop.LeastSquaresSolved : SolveStop.RankDeficient, solve: new Func<LinearVector, LinearVector>(qr.Solve), fullRank: Some(qr.IsFullRank));
        }));
    internal static Fin<SolveReceipt> LuSolve(LuResult lu, Arr<double> rhs, Op key) =>
        !lu.IsValid
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : DenseSolveGated(source: lu.Source, rhs: rhs, key: key, square: true, solve: (_, right, op) => DenseSolve(source: lu.Source, rhs: right, key: op, path: SolvePath.DenseLu, stop: SolveStop.DirectSolved, solve: new Func<LinearVector, LinearVector>(lu.Factor.Solve), residualCap: Math.Sqrt(EpsilonPolicy.SqrtEpsilon)));
    internal static Fin<SolveReceipt> CholeskySolve(CholeskyResult cholesky, Arr<double> rhs, Op key) =>
        !cholesky.IsValid
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : DenseSolveGated(source: cholesky.Source, rhs: rhs, key: key, square: true, solve: (_, right, op) => DenseSolve(source: cholesky.Source, rhs: right, key: op, path: SolvePath.DenseCholesky, stop: SolveStop.DirectSolved, solve: new Func<LinearVector, LinearVector>(cholesky.Factor.Solve), fullRank: Some(value: true), residualCap: Math.Sqrt(EpsilonPolicy.SqrtEpsilon)));
    private static Fin<SolveReceipt> DenseSolveGated(Matrix source, Arr<double> rhs, Op key, bool square, Func<Matrix, Arr<double>, Op, Fin<SolveReceipt>> solve) =>
        !source.IsValid || !SolveInputIsValid(rows: source.Rows.Value, rhs: rhs) || (square && source.Rows.Value != source.Cols.Value)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : solve(source, rhs, key);
    private static Fin<SolveReceipt> DenseSolve(Matrix source, Arr<double> rhs, Op key, SolvePath path, SolveStop stop, Func<LinearVector, LinearVector> solve, Option<bool> fullRank = default, double residualCap = double.PositiveInfinity) =>
        key.Catch(() => {
            Matrix<double> a = ToMathNet(source);
            LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
            LinearVector x = solve(arg: b);
            return SolveSuccess(solution: ArrFromVector(x), solutionLength: source.Cols.Value, path: path, stop: stop, rows: source.Rows, cols: source.Cols, rhsLength: rhs.Count, residual: RelativeResidual(a: a, x: x, b: b), key: key, residualCap: residualCap, fullRank: fullRank);
        });

    // --- [SPARSE_ASSEMBLY] -----------------------------------------------------------------------
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op op) {
        List<(int Row, int Col, double Value)> raw = [.. triplets];
        if (raw.Exists(t => !double.IsFinite(t.Value) || t.Row < 0 || t.Row >= rows.Value || t.Col < 0 || t.Col >= cols.Value)) return Fin.Fail<SparseMatrix>(op.InvalidInput());
        List<(int Row, int Col, double Value)> indexed = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Sum(static t => t.Value)))];
        if (indexed.Exists(static t => !double.IsFinite(t.Value))) return Fin.Fail<SparseMatrix>(op.InvalidResult());
        SparseMatrixD mathNet = SparseMatrixD.OfIndexed(rows: rows.Value, columns: cols.Value, enumerable: indexed);
        List<(int Row, int Col, double Value)> sorted = [.. mathNet.EnumerateIndexed(Zeros.AllowSkip)
            .Where(static t => double.IsFinite(t.Item3) && Math.Abs(value: t.Item3) > 0.0)
            .OrderBy(static t => t.Item1).ThenBy(static t => t.Item2)
            .Select(static t => (Row: t.Item1, Col: t.Item2, Value: t.Item3))];
        (Arr<int> rowPtr, Arr<int> colInd, Arr<double> values) = CompressRows(rowCount: rows.Value, sorted: sorted);
        SparseMatrix result = new(Rows: rows, Cols: cols, RowPtr: rowPtr, ColInd: colInd, Values: values);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseMatrix>(op.InvalidResult());
    }
    internal static Fin<SparseHermitian> AssembleHermitian(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> triplets, Op op) {
        List<(int Row, int Col, Complex Value)> raw = [.. triplets];
        if (raw.Exists(static t => !double.IsFinite(t.Value.Real) || !double.IsFinite(t.Value.Imaginary)) || raw.Exists(t => t.Row < 0 || t.Col < 0 || t.Row >= order.Value || t.Col >= order.Value || t.Row > t.Col)) return Fin.Fail<SparseHermitian>(op.InvalidInput());
        List<(int Row, int Col, Complex Value)> upper = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Aggregate(Complex.Zero, static (acc, t) => acc + t.Value)))
            .OrderBy(static t => t.Row).ThenBy(static t => t.Col)];
        // Diagonal realness gates SUMMED entries under the Admit.HermitianDiagonalRealSpan scale-relative
        // band — pre-sum imaginary parts legitimately cancel, and an absolute band rejects scale-large assemblies.
        double diagonalScale = upper.Where(static t => t.Row == t.Col).Aggregate(seed: 0.0, func: static (max, t) => Math.Max(val1: max, val2: Math.Abs(value: t.Value.Real)));
        double diagonalBand = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: diagonalScale * EpsilonPolicy.SqrtEpsilon);
        if (upper.Exists(t => !double.IsFinite(t.Value.Real) || !double.IsFinite(t.Value.Imaginary) || (t.Row == t.Col && Math.Abs(value: t.Value.Imaginary) > diagonalBand))) return Fin.Fail<SparseHermitian>(op.InvalidResult());
        (Arr<int> rowPtr, Arr<int> colInd, Arr<Complex> values) = CompressRows(rowCount: order.Value, sorted: [.. upper.Select(static t => (t.Row, t.Col, Value: t.Row == t.Col ? new Complex(t.Value.Real, 0.0) : t.Value))]);
        SparseHermitian result = new(Order: order, RowPtr: rowPtr, ColInd: colInd, Values: values);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseHermitian>(op.InvalidResult());
    }
    // 2n x 2n real-block embedding of a Hermitian pair — the connection-Laplacian assembly (Meshing/dec) composes these rows.
    internal static void AddHermitianRealBlockTriplets(List<(int Row, int Col, double Value)> triplets, int order, int i, int j, double real, double imaginary, double diagonal) =>
        triplets.AddRange([
            (i, i, diagonal), (j, j, diagonal), (i + order, i + order, diagonal), (j + order, j + order, diagonal),
            (i, j, real), (j, i, real), (i + order, j + order, real), (j + order, i + order, real),
            (i, j + order, -imaginary), (j + order, i, -imaginary), (i + order, j, imaginary), (j, i + order, imaginary),
        ]);
    private static (Arr<int> RowPtr, Arr<int> ColInd, Arr<T> Values) CompressRows<T>(int rowCount, List<(int Row, int Col, T Value)> sorted) {
        int[] rowPtr = new int[rowCount + 1];
        int[] colInd = new int[sorted.Count];
        T[] values = new T[sorted.Count];
        int cursor = 0;
        for (int row = 0; row < rowCount; row++) {
            rowPtr[row] = cursor;
            while (cursor < sorted.Count && sorted[cursor].Row == row) {
                colInd[cursor] = sorted[cursor].Col;
                values[cursor] = sorted[cursor].Value;
                cursor++;
            }
        }
        rowPtr[rowCount] = cursor;
        return (new Arr<int>(rowPtr), new Arr<int>(colInd), new Arr<T>(values));
    }

    // --- [SPARSE_SOLVES] --------------------------------------------------------------------------
    internal static Fin<Arr<double>> SparseMatVec(SparseMatrix self, Arr<double> x, Op key) =>
        ArrFromVector(ToMathNetSparse(s: self).Multiply(DenseVectorD.OfArray([.. x.AsIterable()]))) switch {
            Arr<double> result when TensorPrimitives.IsFiniteAll<double>(result.AsSpan()) => Fin.Succ(result),
            _ => Fin.Fail<Arr<double>>(key.InvalidResult()),
        };
    internal static Fin<Arr<Complex>> HermitianMatVec(SparseHermitian self, Arr<Complex> x, Op key) =>
        ArrFromComplexVector(ToMathNetHermitian(s: self).Multiply(DenseVectorC.OfArray([.. x.AsIterable()]))) switch {
            Arr<Complex> result when Admit.FiniteComplexSpan(result.AsSpan()) => Fin.Succ(result),
            _ => Fin.Fail<Arr<Complex>>(key.InvalidResult()),
        };
    internal static List<(int Row, int Col, double Value)> SparseTripletsOf(SparseMatrix matrix, int capacityBonus = 0, double scale = 1.0) {
        int n = matrix.Rows.Value;
        List<(int Row, int Col, double Value)> triplets = new(capacity: matrix.NonZeros + capacityBonus);
        for (int i = 0; i < n; i++)
            for (int k = matrix.RowPtr[index: i]; k < matrix.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, matrix.ColInd[index: k], scale * matrix.Values[index: k]));
        return triplets;
    }
    // Diagonal-preconditioned BiCgStab under the explicit criterion stack; the MathNet direct solve is the
    // RECORDED fallback (SparseMathNetDirectFallback / DirectFallbackSolved), gated at the looser cap.
    internal static Fin<SolveReceipt> SparseSolve(SparseMatrix matrix, Arr<double> rhs, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> A = ToMathNetSparse(s: matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                MathNet.Numerics.LinearAlgebra.Double.Solvers.DiagonalPreconditioner preconditioner = new();
                preconditioner.Initialize(matrix: A);
                int iterationCap = Math.Max(val1: 64, val2: matrix.Rows.Value * 8);
                MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double> iterator = new([
                    new MathNet.Numerics.LinearAlgebra.Solvers.FailureStopCriterion<double>(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.DivergenceStopCriterion<double>(maximumRelativeIncrease: BiCgStabDivergenceFactor, minimumIterations: 8),
                    new MathNet.Numerics.LinearAlgebra.Solvers.ResidualStopCriterion<double>(maximum: EpsilonPolicy.SqrtEpsilon, minimumIterationsBelowMaximum: 2),
                    new MathNet.Numerics.LinearAlgebra.Solvers.IterationCountStopCriterion<double>(maximumNumberOfIterations: iterationCap),
                ]);
                LinearVector iterative = A.SolveIterative(input: b, solver: new MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab(), iterator: iterator, preconditioner: preconditioner);
                double iterativeResidual = RelativeResidual(a: A, x: iterative, b: b);
                bool iterativeConverged = iterator.Status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged && double.IsFinite(iterativeResidual) && iterativeResidual <= EpsilonPolicy.SqrtEpsilon;
                double fallbackCap = Math.Sqrt(EpsilonPolicy.SqrtEpsilon);
                LinearVector x = iterativeConverged ? iterative : A.Solve(b);
                double residual = RelativeResidual(a: A, x: x, b: b);
                bool fallbackAccepted = double.IsFinite(residual) && residual <= fallbackCap;
                return SolveSuccess(solution: ArrFromVector(x), solutionLength: matrix.Cols.Value, path: iterativeConverged ? SolvePath.SparseBiCgStabDiagonal : SolvePath.SparseMathNetDirectFallback, stop: iterativeConverged ? SolveStop.ResidualConverged : fallbackAccepted ? SolveStop.DirectFallbackSolved : SolveStop.FallbackRejected, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, residualCap: iterativeConverged ? EpsilonPolicy.SqrtEpsilon : fallbackCap, maxIterations: Some(iterationCap), tolerance: Some(iterativeConverged ? EpsilonPolicy.SqrtEpsilon : fallbackCap), inputNonZeros: Some(matrix.NonZeros));
            });
    // Symmetric-indefinite (or nonsymmetric) sparse direct solve: CSparse SparseLU, A+At ordering,
    // column-relative pivot tol in [0,1]; SPD pivot loss throws bare Exception, caught into the typed rail.
    internal static Fin<SolveReceipt> SparseLuSolve(SparseMatrix matrix, Arr<double> rhs, double pivotTolerance, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs) || !double.IsFinite(pivotTolerance) || pivotTolerance is < 0.0 or > 1.0
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                int n = matrix.Rows.Value;
                CSparse.Storage.CompressedColumnStorage<double> csc = CSparse.Double.SparseMatrix.OfIndexed(rows: n, columns: n, enumerable: SparseTripletsOf(matrix: matrix));
                CSparse.Double.Factorization.SparseLU lu = CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance);
                double[] solution = new double[n];
                lu.Solve(input: [.. rhs.AsIterable()], result: solution.AsSpan());
                Arr<double> x = new(solution);
                double residual = SparseResidual(matrix: matrix, solution: x, rhs: rhs);
                double cap = Math.Sqrt(EpsilonPolicy.SqrtEpsilon);
                return SolveSuccess(solution: x, solutionLength: n, path: SolvePath.SparseLuIndefinite, stop: double.IsFinite(residual) && residual <= cap ? SolveStop.DirectSolved : SolveStop.FallbackRejected, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, residualCap: cap, tolerance: Some(cap), inputNonZeros: Some(matrix.NonZeros), factorNonZeros: Some(lu.NonZerosCount));
            });

    // --- [SINGULAR_GAUGE] --------------------------------------------------------------------------
    // Gauge dual-solve over a singular SPSD operator: admit once, derive every threshold from the
    // operator and rhs scales, witness the TRUE relative residual against the original un-shifted
    // operator, and leave a typed GaugeReceipt. Pin/KKT triplet and projection loops are the named
    // statement-kernel exemption.
    internal static Fin<SolveReceipt> SingularGaugeSolve(SparseMatrix matrix, Arr<double> rhs, GaugePolicy gauge, Context context, Op key) =>
        gauge is null || !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs) || !GaugeNullspaceFits(gauge: gauge, dimension: matrix.Rows.Value)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : from upper in NormalizeSymmetricUpperEntries(s: matrix, key: key)
              from result in key.Catch(() => {
                  int n = matrix.Rows.Value;
                  Matrix<double> aSym = ToMathNetSymmetric(matrix: matrix, upper: upper);
                  LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                  double operatorScale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: aSym.FrobeniusNorm());
                  Matrix<double> mass = GaugeMass(gauge: gauge, dimension: n);
                  Matrix<double> nullspace = GaugeNullspaceColumns(gauge: gauge, dimension: n);
                  double bScale = Math.Max(val1: 1.0, val2: b.InfinityNorm());
                  double compatibility = gauge.Switch(
                      state: (Nullspace: nullspace, B: b),
                      pin: static (_, _) => 0.0,
                      meanZeroDeflation: static (s, _) => CompatibilityResidual(nullspace: s.Nullspace, b: s.B),
                      lagrangeKKT: static (s, _) => CompatibilityResidual(nullspace: s.Nullspace, b: s.B));
                  bool projectRhs = gauge.Switch(
                      state: (Compat: compatibility, Tol: context.Fractional * bScale),
                      pin: static (_, _) => false,
                      meanZeroDeflation: static (s, _) => s.Compat > s.Tol,
                      lagrangeKKT: static (_, _) => false);
                  LinearVector rhsGauged = projectRhs ? DeflateRhs(nullspace: nullspace, mass: mass, b: b) : b;
                  double rhsMutation = (rhsGauged - b).L2Norm();
                  return gauge.Switch(
                      state: (Matrix: matrix, Upper: upper, ASym: aSym, Mass: mass, Nullspace: nullspace, Rhs: rhsGauged, Key: key),
                      pin: static (s, p) => SolvePin(matrix: s.Matrix, upper: s.Upper, aSym: s.ASym, pin: p, b: s.Rhs, key: s.Key),
                      meanZeroDeflation: static (s, _) => SolveMeanZeroDeflation(matrix: s.Matrix, aSym: s.ASym, mass: s.Mass, nullspace: s.Nullspace, b: s.Rhs, key: s.Key),
                      lagrangeKKT: static (s, _) => SolveKkt(upper: s.Upper, aSym: s.ASym, massNullspace: s.Mass.Multiply(s.Nullspace), b: s.Rhs, key: s.Key))
                  .Bind(stage => {
                      LinearVector shifted = ApplyShift(shift: gauge.Shift, mass: mass, x: stage.X, rows: n);
                      double relative = RelativeResidual(a: aSym, x: shifted, b: b);
                      double residualM = MassResidual(a: aSym, mass: mass, x: shifted, b: b);
                      double orthogonality = GaugeOrthogonality(nullspace: nullspace, mass: mass, x: shifted) / Math.Max(val1: 1.0, val2: shifted.L2Norm());
                      GaugeReceipt receipt = new(
                          Solver: gauge.SolverKind, NullspaceDim: gauge.NullspaceDim, NullspaceDimNumeric: stage.NullspaceDimNumeric,
                          OperatorFrobeniusScale: operatorScale, ResidualCompatibility: compatibility, RhsProjected: projectRhs, ResidualAfterGauge: stage.Residual, ResidualAfterGaugeM: residualM,
                          ResidualRelative: relative, PinnedIndex: GaugePinnedIndex(gauge: gauge), PinIndices: GaugePinIndices(gauge: gauge), ConstraintRows: gauge.NullspaceDim, PostShiftApplied: gauge.Shift,
                          RhsMutationNorm: rhsMutation, MultiplierNorm: stage.MultiplierNorm, Iterations: stage.Iterations, GaugeOrthogonalityCheck: orthogonality, RegularizationEpsUsed: stage.RegularizationEps,
                          NumericalBreakdown: stage.NumericalBreakdown);
                      SolveStop stop = stage.NumericalBreakdown || !double.IsFinite(relative)
                          ? SolveStop.FallbackRejected
                          : relative <= context.Fractional ? stage.Stop : SolveStop.IterativeExhausted;
                      return SolveSuccess(solution: ArrFromVector(shifted), solutionLength: n, path: stage.Path, stop: stop, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: relative, key: key, iterations: stage.Iterations, factorNonZeros: stage.FactorNonZeros, gauge: Some(receipt));
                  });
              })
              select result;
    private static bool GaugeNullspaceFits(GaugePolicy gauge, int dimension) =>
        gauge.Switch(
            state: dimension,
            pin: static (dim, p) => p.Indices.Count >= 1 && p.Indices.Count == p.Values.Count && p.Indices.ForAll(index => index >= 0 && index < dim) && GaugeMassFits(mass: p.Mass, dimension: dim),
            meanZeroDeflation: static (dim, d) => GaugeBasisFits(basis: d.Nullspace, dimension: dim) && GaugeMassFits(mass: d.Mass, dimension: dim),
            lagrangeKKT: static (dim, k) => GaugeBasisFits(basis: k.Nullspace, dimension: dim) && GaugeMassFits(mass: k.Mass, dimension: dim));
    private static bool GaugeBasisFits(Arr<Arr<double>> basis, int dimension) =>
        basis.Count >= 1 && basis.Count < dimension && basis.ForAll(column => column.Count == dimension && TensorPrimitives.IsFiniteAll<double>(column.AsSpan()));
    private static bool GaugeMassFits(Option<Arr<double>> mass, int dimension) =>
        mass.Map(diagonal => diagonal.Count == dimension && diagonal.ForAll(static value => double.IsFinite(value) && value > 0.0)).IfNone(noneValue: true);
    private static Matrix<double> GaugeMass(GaugePolicy gauge, int dimension) =>
        gauge.Switch(
            state: dimension,
            pin: static (dim, p) => MassDiagonal(mass: p.Mass, dimension: dim),
            meanZeroDeflation: static (dim, d) => MassDiagonal(mass: d.Mass, dimension: dim),
            lagrangeKKT: static (dim, k) => MassDiagonal(mass: k.Mass, dimension: dim));
    private static Matrix<double> MassDiagonal(Option<Arr<double>> mass, int dimension) =>
        mass.Match(
            Some: diagonal => (Matrix<double>)DenseMatrixD.OfDiagonalVector(DenseVectorD.OfArray([.. diagonal.AsIterable()])),
            None: () => DenseMatrixD.CreateIdentity(order: dimension));
    private static Matrix<double> GaugeNullspaceColumns(GaugePolicy gauge, int dimension) =>
        gauge.Switch(
            state: dimension,
            pin: static (dim, p) => DenseMatrixD.OfColumnVectors([.. p.Indices.AsIterable().Select(index => DenseVectorD.Create(dim, i => i == index ? 1.0 : 0.0))]),
            meanZeroDeflation: static (_, d) => BasisColumns(basis: d.Nullspace),
            lagrangeKKT: static (_, k) => BasisColumns(basis: k.Nullspace));
    private static Matrix<double> BasisColumns(Arr<Arr<double>> basis) =>
        DenseMatrixD.OfColumnVectors([.. basis.AsIterable().Select(column => DenseVectorD.OfArray([.. column.AsIterable()]))]);
    private static Option<int> GaugePinnedIndex(GaugePolicy gauge) =>
        gauge.Switch(pin: static p => p.Indices.Count > 0 ? Some(p.Indices[0]) : Option<int>.None, meanZeroDeflation: static _ => Option<int>.None, lagrangeKKT: static _ => Option<int>.None);
    private static Arr<int> GaugePinIndices(GaugePolicy gauge) =>
        gauge.Switch(pin: static p => p.Indices, meanZeroDeflation: static _ => new Arr<int>([]), lagrangeKKT: static _ => new Arr<int>([]));
    // M-orthogonal primitives: DeflateRhs, ProjectRange, MassResidual, GaugeOrthogonality share one mass inner product.
    private static double CompatibilityResidual(Matrix<double> nullspace, LinearVector b) => nullspace.TransposeThisAndMultiply(b).L2Norm();
    // Shared M-orthogonal Gram solve: factor the SPD Gram Nt M N, applying a diagonal-scaled Tikhonov
    // shift only when the plain Cholesky breaks down; surface the shift plus the numeric nullspace
    // dimension (factor diagonal entries above a scale-relative floor).
    private static (LinearVector Coords, double Shift, int NumericRank) RegularizedGramSolve(Matrix<double> gram, LinearVector rhs) {
        double scale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: gram.Diagonal().Enumerate().Aggregate(0.0, static (acc, value) => Math.Max(acc, Math.Abs(value))));
        (MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor, double shift) =
            TryGram(gram: gram).Match(
                Some: chol => (Factor: chol, Shift: 0.0),
                None: () => {
                    double tikhonov = EpsilonPolicy.SqrtEpsilon * scale;
                    return (Factor: (gram + (DenseMatrixD.CreateIdentity(order: gram.RowCount) * tikhonov)).Cholesky(), Shift: tikhonov);
                });
        double floor = EpsilonPolicy.SqrtEpsilon * scale;
        int numericRank = factor.Factor.Diagonal().Enumerate().Count(value => value * value > floor);
        return (Coords: factor.Solve(rhs), shift, NumericRank: numericRank);
    }
    // MathNet Cholesky throws a bare Exception on SPD pivot loss; the algorithms route catches broadly
    // at this one boundary and converts to None — the intended boundary form, not a swallowed error.
    private static Option<MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double>> TryGram(Matrix<double> gram) {
#pragma warning disable CA1031
        try { return Some(gram.Cholesky()); } catch (Exception) { return None; }
#pragma warning restore CA1031
    }
    private static (LinearVector Projected, double Shift, int NumericRank) ProjectRange(Matrix<double> nullspace, Matrix<double> mass, LinearVector x) {
        Matrix<double> massNullspace = mass.Multiply(nullspace);
        Matrix<double> gram = nullspace.TransposeThisAndMultiply(massNullspace);
        (LinearVector coords, double shift, int numericRank) = RegularizedGramSolve(gram: gram, rhs: nullspace.TransposeThisAndMultiply(mass.Multiply(x)));
        return (Projected: x - (nullspace * coords), Shift: shift, NumericRank: numericRank);
    }
    private static LinearVector DeflateRhs(Matrix<double> nullspace, Matrix<double> mass, LinearVector b) {
        Matrix<double> massNullspace = mass.Multiply(nullspace);
        Matrix<double> gram = nullspace.TransposeThisAndMultiply(massNullspace);
        (LinearVector coords, _, _) = RegularizedGramSolve(gram: gram, rhs: nullspace.TransposeThisAndMultiply(b));
        return b - (massNullspace * coords);
    }
    private static double GaugeOrthogonality(Matrix<double> nullspace, Matrix<double> mass, LinearVector x) =>
        nullspace.TransposeThisAndMultiply(mass.Multiply(x)).L2Norm();
    private static double MassResidual(Matrix<double> a, Matrix<double> mass, LinearVector x, LinearVector b) {
        LinearVector residual = b - a.Multiply(x);
        return Math.Sqrt(residual.DotProduct(mass.Multiply(residual))) / Math.Max(val1: 1.0, val2: Math.Sqrt(b.DotProduct(mass.Multiply(b))));
    }
    private static LinearVector ApplyShift(GaugeShift shift, Matrix<double> mass, LinearVector x, int rows) =>
        shift.Switch(
            state: (Mass: mass, X: x, Rows: rows),
            none: static s => s.X,
            meanZero: static s => s.X - (MassWeightedMean(mass: s.Mass, x: s.X) * DenseVectorD.Create(s.Rows, static _ => 1.0)),
            minZero: static s => s.X - (s.X.Minimum() * DenseVectorD.Create(s.X.Count, static _ => 1.0)),
            pinZero: static s => s.X);
    private static double MassWeightedMean(Matrix<double> mass, LinearVector x) {
        LinearVector ones = DenseVectorD.Create(x.Count, static _ => 1.0);
        LinearVector massOnes = mass.Multiply(ones);
        return massOnes.DotProduct(x) / Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: massOnes.DotProduct(ones));
    }
    private static Fin<GaugeStage> SolvePin(SparseMatrix matrix, List<(int Row, int Col, double Value)> upper, Matrix<double> aSym, GaugePolicy.Pin pin, LinearVector b, Op key) {
        int n = matrix.Rows.Value;
        bool[] pinned = new bool[n];
        double[] pinValues = new double[n];
        for (int i = 0; i < pin.Indices.Count; i++) { pinned[pin.Indices[i]] = true; pinValues[pin.Indices[i]] = pin.Values[i]; }
        int[] remap = new int[n];
        int free = 0;
        for (int i = 0; i < n; i++) remap[i] = pinned[i] ? -1 : free++;
        if (free == 0) return Fin.Fail<GaugeStage>(key.InvalidInput());
        List<(int Row, int Col, double Value)> filtered = new(capacity: upper.Count);
        double[] reduced = new double[free];
        foreach ((int row, int col, double value) in upper) {
            if (pinned[row] && pinned[col]) continue;
            if (pinned[row]) { reduced[remap[col]] -= value * pinValues[row]; continue; }
            if (pinned[col]) { reduced[remap[row]] -= value * pinValues[col]; continue; }
            filtered.Add(item: (remap[row], remap[col], value));
        }
        for (int i = 0; i < n; i++) if (!pinned[i]) reduced[remap[i]] += b[i];
        Dimension dim = Dimension.Create(value: free);
        return from reducedMatrix in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: filtered, key: key)
               from factor in CholeskySparse.Of(symmetric: reducedMatrix, key: key)
               from solved in factor.Solve(rhs: new Arr<double>(reduced), key: key)
               let reassembled = DenseVectorD.Create(n, i => pinned[i] ? pinValues[i] : solved[remap[i]])
               select new GaugeStage(X: reassembled, Residual: RelativeResidual(a: aSym, x: reassembled, b: b), Stop: SolveStop.DirectSolved, Path: SolvePath.SparseCholesky, MultiplierNorm: 0.0, Iterations: None, RegularizationEps: 0.0, NumericalBreakdown: false, FactorNonZeros: Some(factor.FactorNonZeros));
    }
    private static Fin<GaugeStage> SolveMeanZeroDeflation(SparseMatrix matrix, Matrix<double> aSym, Matrix<double> mass, Matrix<double> nullspace, LinearVector b, Op key) =>
        SparseSolve(matrix: matrix, rhs: new Arr<double>(b.ToArray()), key: key).Map(receipt => {
            LinearVector raw = DenseVectorD.OfArray([.. receipt.Solution.AsIterable()]);
            (LinearVector projected, double shift, int numericRank) = ProjectRange(nullspace: nullspace, mass: mass, x: raw);
            return new GaugeStage(X: projected, Residual: RelativeResidual(a: aSym, x: projected, b: b), Stop: receipt.Stop, Path: receipt.Path, MultiplierNorm: 0.0, Iterations: receipt.Iterations, RegularizationEps: shift, NumericalBreakdown: false, FactorNonZeros: receipt.FactorNonZeros, NullspaceDimNumeric: Some(numericRank));
        });
    // Saddle assembly rides the EXACT sparse upper entries mirrored — a dense n^2 sweep or a
    // magnitude prune of A entries mutates the operator and densifies against sparse consumers.
    private static Fin<GaugeStage> SolveKkt(List<(int Row, int Col, double Value)> upper, Matrix<double> aSym, Matrix<double> massNullspace, LinearVector b, Op key) {
        int n = aSym.RowCount, m = massNullspace.ColumnCount, total = n + m;
        List<(int Row, int Col, double Value)> entries = new(capacity: (2 * upper.Count) + (2 * n * m));
        foreach ((int row, int col, double value) in upper) {
            entries.Add(item: (row, col, value));
            if (row != col) entries.Add(item: (col, row, value));
        }
        for (int i = 0; i < n; i++)
            for (int c = 0; c < m; c++)
                if (massNullspace[i, c] != 0.0) { entries.Add(item: (i, n + c, massNullspace[i, c])); entries.Add(item: (n + c, i, massNullspace[i, c])); }
        double[] rhs = new double[total];
        for (int i = 0; i < n; i++) rhs[i] = b[i];
        return key.Catch(() => {
            CSparse.Storage.CompressedColumnStorage<double> saddle = CSparse.Double.SparseMatrix.OfIndexed(rows: total, columns: total, enumerable: entries);
            double[] solution = new double[total];
            CSparse.Double.Factorization.SparseLU lu = CSparse.Double.Factorization.SparseLU.Create(A: saddle, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: KktPivotTolerance);
            lu.Solve(input: rhs.AsSpan(), result: solution.AsSpan());
            LinearVector x = DenseVectorD.OfArray([.. solution.Take(count: n)]);
            LinearVector lambda = DenseVectorD.OfArray([.. solution.Skip(count: n)]);
            double residual = (b - aSym.Multiply(x)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
            return Fin.Succ(new GaugeStage(X: x, Residual: residual, Stop: SolveStop.DirectSolved, Path: SolvePath.SparseLuIndefinite, MultiplierNorm: lambda.L2Norm(), Iterations: None, RegularizationEps: 0.0, NumericalBreakdown: false, FactorNonZeros: Some(lu.NonZerosCount)));
        }).BindFail(_ => Fin.Succ(new GaugeStage(X: DenseVectorD.Create(n, static _ => 0.0), Residual: double.PositiveInfinity, Stop: SolveStop.FallbackRejected, Path: SolvePath.SparseLuIndefinite, MultiplierNorm: 0.0, Iterations: None, RegularizationEps: 0.0, NumericalBreakdown: true, FactorNonZeros: None)));
    }
    // Path is the stage's ACTUAL route — the minted SolveReceipt reports it, so a deflation solve that
    // landed on the recorded direct fallback never masquerades as the declared iterative route.
    private readonly record struct GaugeStage(LinearVector X, double Residual, SolveStop Stop, SolvePath Path, double MultiplierNorm, Option<int> Iterations, double RegularizationEps, bool NumericalBreakdown, Option<int> FactorNonZeros, Option<int> NullspaceDimNumeric = default);

    // --- [GENERALIZED_EIGEN] ------------------------------------------------------------------------
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> GeneralizedEigenpairsDetailed(SparseMatrix stiffness, SparseMatrix mass, int k, Op key) =>
        !stiffness.IsValid || !mass.IsValid || stiffness.Rows.Value != stiffness.Cols.Value || mass.Rows.Value != mass.Cols.Value || stiffness.Rows.Value != mass.Rows.Value || k < 1 || k >= stiffness.Rows.Value
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : from stiffnessUpper in NormalizeSymmetricUpperEntries(s: stiffness, key: key)
              from massUpper in NormalizeSymmetricUpperEntries(s: mass, key: key)
              from receipt in key.Catch(() => {
                  Matrix<double> stiffnessM = ToMathNetSymmetric(matrix: stiffness, upper: stiffnessUpper);
                  Matrix<double> massM = ToMathNetSymmetric(matrix: mass, upper: massUpper);
                  (LinearVector vals, Matrix<double> vecs, int factorNonZeros) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                  Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                      .OrderBy(i => vals[i]).Take(k)
                      .Select(i => (Eigenvalue: vals[i], Eigenvector: ArrFromVector(vecs.Column(i)))));
                  return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.SparseGeneralizedCholeskyCongruence, stop: EigenSolveStop.DirectSolved, requestedPairs: k, maxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs), key: key, factorNonZeros: Some(factorNonZeros));
              })
              select receipt;
    // Generalised eigenproblem A z = lambda M z via the symmetric Cholesky congruence L^-1 A L^-T.
    private static (LinearVector Vals, Matrix<double> Vecs, int FactorNonZeros) SolveGeneralised(Matrix<double> Ahat, Matrix<double> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> cholesky = Mhat.Cholesky();
        Matrix<double> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixD.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.Transpose());
        Matrix<double> sym = (reduced + reduced.Transpose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = sym.Evd(Symmetricity.Symmetric);
        return (
            Vals: DenseVectorD.Create(evd.EigenValues.Count, i => evd.EigenValues[i].Real),
            Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.Transpose()),
            FactorNonZeros: cholesky.Factor.Enumerate(Zeros.AllowSkip).Count(static value => Math.Abs(value: value) > EpsilonPolicy.ZeroTolerance));
    }
    private static (ComplexVector Vals, Matrix<Complex> Vecs) SolveGeneralisedComplex(Matrix<Complex> Ahat, Matrix<Complex> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<Complex> cholesky = Mhat.Cholesky();
        Matrix<Complex> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixC.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.ConjugateTranspose());
        Matrix<Complex> herm = (reduced + reduced.ConjugateTranspose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = herm.Evd(Symmetricity.Hermitian);
        return (Vals: evd.EigenValues, Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.ConjugateTranspose()));
    }
    private static Matrix<T> CongruentReduce<T>(Matrix<T> factor, Matrix<T> matrix, Matrix<T> identity, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        factor.Solve(matrix * adjoint(arg: factor).Solve(identity));
    private static Matrix<T> BackTransform<T>(Matrix<T> factor, Matrix<T> vectors, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        adjoint(arg: factor).Solve(vectors);

    // --- [LOBPCG] --------------------------------------------------------------------------------
    // Knyazev LOBPCG: span([X_i, R_i, P_i]) Rayleigh-Ritz; first iteration omits the zero previous direction.
    // Basis seeding is deterministic through the Domain/identity splitmix64 owner, so eigen results replay.
    private const int RealInitialBasisSeed = 17, HermitianInitialBasisSeed = 19;
    private delegate T BasisSample<T>(ref ulong state);
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> Lobpcg(SparseMatrix matrix, int k, double tolerance, int maxIterations, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || k < 1 || k >= matrix.Rows.Value || !double.IsFinite(tolerance) || tolerance <= 0 || maxIterations < 1
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => {
                Matrix<double> A = ToMathNetSymmetric(matrix: matrix, upper: upper);
                return LobpcgCore(A: A, X: OrthonormalRandom(rows: matrix.Rows.Value, k: k, seed: RealInitialBasisSeed, sample: static (ref ulong s) => Deterministic.NextSignedUnit(state: ref s), orthonormalise: OrthonormaliseColumns), P: DenseMatrixD.Create(matrix.Rows.Value, k, 0.0), jacobi: ExtractDiagonalInverse(A), k: k, tolerance: tolerance, maxIterations: maxIterations, key: key, path: EigenSolvePath.SparseLobpcg, rayleigh: RayleighQuotients, diagonal: DenseMatrixD.OfDiagonalVector, adjoint: static m => m.Transpose(), orthonormalise: OrthonormaliseColumns, solveGeneralised: static (Ahat, Mhat) => { (LinearVector Vals, Matrix<double> Vecs, int _) = SolveGeneralised(Ahat: Ahat, Mhat: Mhat); return (Vals, Vecs); }, eigenvalue: static value => value, vector: static v => ArrFromVector(v: v), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector));
            });
    internal static Fin<EigenSolveReceipt<double, Arr<Complex>>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, int maxIterations, Op key) =>
        !matrix.IsValid || k < 1 || k >= matrix.Order.Value || !double.IsFinite(tolerance) || tolerance <= 0 || maxIterations < 1
            ? Fin.Fail<EigenSolveReceipt<double, Arr<Complex>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<Complex> A = ToMathNetHermitian(matrix);
                return LobpcgCore(A: A, X: OrthonormalRandom(rows: matrix.Order.Value, k: k, seed: HermitianInitialBasisSeed, sample: static (ref ulong s) => Deterministic.NextSignedComplexUnit(state: ref s), orthonormalise: OrthonormaliseColumnsComplex), P: DenseMatrixC.Create(matrix.Order.Value, k, Complex.Zero), jacobi: ExtractDiagonalInverseComplex(A), k: k, tolerance: tolerance, maxIterations: maxIterations, key: key, path: EigenSolvePath.SparseHermitianLobpcg, rayleigh: RayleighQuotientsComplex, diagonal: DenseMatrixC.OfDiagonalVector, adjoint: static m => m.ConjugateTranspose(), orthonormalise: OrthonormaliseColumnsComplex, solveGeneralised: static (Ahat, Mhat) => SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat), eigenvalue: static value => value.Real, vector: static v => ArrFromComplexVector(v: v), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue));
            });
    private static Fin<EigenSolveReceipt<double, TVector>> LobpcgCore<T, TVector>(Matrix<T> A, Matrix<T> X, Matrix<T> P, MathNet.Numerics.LinearAlgebra.Vector<T> jacobi, int k, double tolerance, int maxIterations, Op key, EigenSolvePath path, Func<Matrix<T>, Matrix<T>, MathNet.Numerics.LinearAlgebra.Vector<T>> rayleigh, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, Matrix<T>> diagonal, Func<Matrix<T>, Matrix<T>> adjoint, Func<Matrix<T>, Matrix<T>> orthonormalise, Func<Matrix<T>, Matrix<T>, (MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solveGeneralised, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector, Func<Matrix<T>, Seq<(double Eigenvalue, TVector Eigenvector)>, double> residual)
        where T : struct, IEquatable<T>, IFormattable {
        int n = A.RowCount;
        return Iterate(iter: 0, X: X, P: P);
        Fin<EigenSolveReceipt<double, TVector>> Iterate(int iter, Matrix<T> X, Matrix<T> P) =>
            iter >= maxIterations
                ? Receipt(iter: iter, X: X, stop: EigenSolveStop.MaxIterationsExhausted)
                : Step(iter: iter, X: X, P: P);
        Fin<EigenSolveReceipt<double, TVector>> Step(int iter, Matrix<T> X, Matrix<T> P) {
            Matrix<T> AX = A * X;
            MathNet.Numerics.LinearAlgebra.Vector<T> lambda = rayleigh(arg1: X, arg2: AX);
            Matrix<T> R = AX - (X * diagonal(arg: lambda));
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: lambda, X: X);
            return residual(arg1: A, arg2: pairs) < tolerance
                ? Receipt(iter: iter, X: X, stop: EigenSolveStop.ResidualConverged)
                : Continue(iter: iter, X: X, P: P, R: R);
        }
        Fin<EigenSolveReceipt<double, TVector>> Continue(int iter, Matrix<T> X, Matrix<T> P, Matrix<T> R) {
            Matrix<T> W = ApplyJacobi(R: R, invDiag: jacobi);
            bool hasPrevious = iter > 0 && MaxColumnNorm(m: P) > EpsilonPolicy.SqrtEpsilon;
            Matrix<T> S = orthonormalise(arg: hasPrevious ? X.Append(W).Append(P) : X.Append(W));
            int[] survivors = SurvivingColumns(m: S);
            if (survivors.Length < k) return Receipt(iter: iter, X: X, stop: EigenSolveStop.MaxIterationsExhausted);
            Matrix<T> Sr = Matrix<T>.Build.DenseOfColumnVectors([.. survivors.Select(S.Column)]);
            Matrix<T> ASr = A * Sr;
            Matrix<T> STr = adjoint(arg: Sr);
            Fin<(MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solved = key.Catch(() => Fin.Succ(solveGeneralised(arg1: STr * ASr, arg2: STr * Sr)));
            return solved.Bind(solution => {
                Matrix<T> Z = ScatterRows(reduced: TakeSmallest(eigVals: solution.Vals, eigVecs: solution.Vecs, k: k, key: eigenvalue), rows: S.ColumnCount, sourceRows: survivors);
                Matrix<T> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : Matrix<T>.Build.Dense(n, k);
                return Iterate(iter: iter + 1, X: orthonormalise(arg: S * Z), P: (W * Z.SubMatrix(k, k, 0, k)) + previous);
            });
        }
        Fin<EigenSolveReceipt<double, TVector>> Receipt(int iter, Matrix<T> X, EigenSolveStop stop) {
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: rayleigh(arg1: X, arg2: A * X), X: X);
            return EigenReceiptOf(pairs: pairs, path: path, stop: stop, requestedPairs: k, maxResidual: residual(arg1: A, arg2: pairs), key: key, iterations: Some(iter), maxIterations: Some(maxIterations), tolerance: Some(tolerance));
        }
        Seq<(double Eigenvalue, TVector Eigenvector)> Pairs(MathNet.Numerics.LinearAlgebra.Vector<T> lambda, Matrix<T> X) =>
            toSeq(Enumerable.Range(start: 0, count: k).Select(i => (Eigenvalue: eigenvalue(arg: lambda[i]), Eigenvector: vector(arg: X.Column(i)))).OrderBy(static p => p.Eigenvalue));
    }
    private static Matrix<T> OrthonormalRandom<T>(int rows, int k, int seed, BasisSample<T> sample, Func<Matrix<T>, Matrix<T>> orthonormalise)
        where T : struct, IEquatable<T>, IFormattable {
        ulong state = unchecked((ulong)seed);
        return orthonormalise(arg: Matrix<T>.Build.Dense(rows, k, (_, _) => sample(state: ref state)));
    }
    // Modified Gram-Schmidt; rank-collapsed columns remain zero for the survivor-deflation pass.
    private static Matrix<double> OrthonormaliseColumns(Matrix<double> m) =>
        OrthonormaliseColumns(m: m, zero: 0.0, inner: static (basis, value) => basis.DotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<Complex> OrthonormaliseColumnsComplex(Matrix<Complex> m) =>
        OrthonormaliseColumns(m: m, zero: Complex.Zero, inner: static (basis, value) => basis.ConjugateDotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<T> OrthonormaliseColumns<T>(Matrix<T> m, T zero, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T> inner, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T, MathNet.Numerics.LinearAlgebra.Vector<T>> remove, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, double, MathNet.Numerics.LinearAlgebra.Vector<T>> normalise)
        where T : struct, IEquatable<T>, IFormattable {
        int n = m.RowCount;
        int k = m.ColumnCount;
        Matrix<T> q = Matrix<T>.Build.Dense(rows: n, columns: k, value: zero);
        for (int j = 0; j < k; j++) {
            MathNet.Numerics.LinearAlgebra.Vector<T> v = m.Column(j);
            for (int i = 0; i < j; i++) {
                MathNet.Numerics.LinearAlgebra.Vector<T> basis = q.Column(i);
                v = remove(arg1: v, arg2: basis, arg3: inner(arg1: basis, arg2: v));
            }
            double norm = v.L2Norm();
            if (norm > EpsilonPolicy.SqrtEpsilon) q.SetColumn(columnIndex: j, column: normalise(arg1: v, arg2: norm));
        }
        return q;
    }
    // Rank-collapsed (zero) columns make St S singular and the congruence throw; drop them before the
    // Rayleigh-Ritz solve and scatter the reduced Ritz vectors back so the [X|W|P] block offsets survive.
    private static int[] SurvivingColumns<T>(Matrix<T> m)
        where T : struct, IEquatable<T>, IFormattable =>
        [.. Enumerable.Range(start: 0, count: m.ColumnCount).Where(j => m.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon)];
    private static Matrix<T> ScatterRows<T>(Matrix<T> reduced, int rows, int[] sourceRows)
        where T : struct, IEquatable<T>, IFormattable {
        Matrix<T> full = Matrix<T>.Build.Dense(rows: rows, columns: reduced.ColumnCount);
        for (int i = 0; i < sourceRows.Length; i++) full.SetRow(rowIndex: sourceRows[i], row: reduced.Row(i));
        return full;
    }
    private static LinearVector ExtractDiagonalInverse(Matrix<double> A) =>
        DenseVectorD.Create(A.RowCount, i => Math.Abs(A[i, i]) > EpsilonPolicy.SqrtEpsilon ? 1.0 / A[i, i] : 1.0);
    private static ComplexVector ExtractDiagonalInverseComplex(Matrix<Complex> A) =>
        DenseVectorC.Create(A.RowCount, i => Complex.Abs(A[i, i]) > EpsilonPolicy.SqrtEpsilon ? Complex.One / A[i, i] : Complex.One);
    private static LinearVector RayleighQuotients(Matrix<double> X, Matrix<double> AX) =>
        DenseVectorD.Create(X.ColumnCount, j => X.Column(j).DotProduct(AX.Column(j)) / Math.Max(X.Column(j).DotProduct(X.Column(j)), EpsilonPolicy.ZeroTolerance));
    private static ComplexVector RayleighQuotientsComplex(Matrix<Complex> X, Matrix<Complex> AX) =>
        DenseVectorC.Create(X.ColumnCount, j => X.Column(j).ConjugateDotProduct(X.Column(j)) switch {
            Complex den when Complex.Abs(den) > EpsilonPolicy.ZeroTolerance => X.Column(j).ConjugateDotProduct(AX.Column(j)) / den,
            _ => Complex.Zero,
        });
    private static double MaxColumnNorm<T>(Matrix<T> m)
        where T : struct, IEquatable<T>, IFormattable =>
        Enumerable.Range(start: 0, count: m.ColumnCount).Aggregate(seed: 0.0, func: (max, j) => Math.Max(max, m.Column(j).L2Norm()));
    private static Matrix<T> ApplyJacobi<T>(Matrix<T> R, MathNet.Numerics.LinearAlgebra.Vector<T> invDiag)
        where T : struct, IEquatable<T>, IFormattable {
        Matrix<T> scaled = R.Clone();
        for (int i = 0; i < R.RowCount; i++) scaled.SetRow(rowIndex: i, row: R.Row(i).Multiply(scalar: invDiag[i]));
        return scaled;
    }
    private static Matrix<T> TakeSmallest<T>(MathNet.Numerics.LinearAlgebra.Vector<T> eigVals, Matrix<T> eigVecs, int k, Func<T, double> key)
        where T : struct, IEquatable<T>, IFormattable =>
        Matrix<T>.Build.DenseOfColumnVectors([.. Enumerable.Range(start: 0, count: eigVals.Count).OrderBy(i => key(arg: eigVals[i])).Take(count: k).Select(eigVecs.Column)]);
}
```

## [07]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]       | [OWNER]                                              | [KIND]                                                        | [CASES] |
| :-----: | :------------------- | :---------------------------------------------------- | :------------------------------------------------------------ | :-----: |
|  [01]   | Route/stop vocabulary | `EigenSolvePath` · `EigenSolveStop` · `SolvePath` · `SolveStop` · `MatrixNormKind` · `GaugeSolverKind` · `GaugeShift` | `[SmartEnum<int>]` with capability columns                    | 5·3·7·7·4·3·4 |
|  [02]   | Gauge algebra        | `GaugePolicy`                                        | `[Union]` Pin/MeanZeroDeflation/LagrangeKKT + presets          |    3    |
|  [03]   | Dense owners         | `Matrix` · `SymmetricMatrix` (+ 4 decomposition carriers) | admission-gated `record struct` over MathNet                  |    2    |
|  [04]   | Sparse owners        | `SparseMatrix` · `SparseHermitian` · `CholeskySparse` | CSR invariants + lock-guarded AMD factor cache over CSparse    |    3    |
|  [05]   | Evidence             | `SolveReceipt` · `EigenSolveReceipt<TEigen,TVector>` · `GaugeReceipt` | `ValidityClaim.All` fold + semantic claim rows                 |    3    |
|  [06]   | Kernel               | `MatrixKernel`                                       | the ONE MathNet+CSparse access path (dense/sparse/gauge/eigen/LOBPCG) |    1    |
