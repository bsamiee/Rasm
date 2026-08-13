# [RASM_NUMERICS_MATRIX]

`Rasm.Numerics` grounds every kernel solver on one linear-algebra substrate: the dense, sparse, and complex matrix owners, the solve/eigen/gauge route algebra, the lattice-addressed transform band, and `MatrixKernel`, the sole MathNet and CSparse path they compose. Every operation leaves as a typed receipt carrying its route, stop, and recomputed residual; a raw `Matrix<double>`, bare factorization, or untyped spectrum never crosses the surface.

Rebuilds compose the `Rasm.Domain` rails as the receipt validity floor and stays host-neutral: finiteness reads through `TensorPrimitives.IsFiniteAll` over flat spans, every tolerance draws from an `EpsilonPolicy` row, and no `RhinoMath` member or absolute literal appears. MathNet and CSparse are the mined standard library.

## [01]-[INDEX]

- [02]-[SOLVE_VOCABULARY]: `Solve`, `eigen`, `norm`, `sense`, and `gauge` route the algebra every receipt discriminates on.
- [03]-[DENSE_OWNERS]: dense and packed-symmetric owners with the decomposition carriers holding live factors for repeated right-hand sides.
- [04]-[SPARSE_OWNERS]: CSR and Hermitian owners with their structural invariants and the lock-guarded AMD factor cache.
- [05]-[RECEIPTS]: typed solve, eigen, and gauge evidence carriers on the rails validity fold.
- [06]-[TRANSFORM_BAND]: `SpectralArena`'s caller-arena spectral transform, the sample-domain tap fold closing the convolution correspondence beside it, the taper roster, and the fitted-interpolant carrier over one sampled axis.
- [07]-[SOLVE_KERNEL]: `MatrixKernel`, the one MathNet and CSparse access path for every decomposition, solve, eigen, and transform route.

## [02]-[SOLVE_VOCABULARY]

- Owner: route, stop, norm, sense, and gauge-solver capability live as `[SmartEnum<int>]` vocabularies, never `bool` flags — each rides as a discriminant column a consumer reads off the row; `GaugePolicy` is the `[Union]` owning the singular-system gauge algebra.
- Entry: constant-nullspace presets are the Laplacian consumer's entry; `NullspaceDim`, `Shift`, and `SolverKind` project off the case as total `Switch`; `OperatorSense` enters every sparse product and every transposable solve, defaulting to the canonical `Forward` row.
- Auto: mass diagonal rides one `Option<Arr<double>>` per case, so the same policy value selects Euclidean or M-weighted inner products throughout the gauge solve; `MeanZeroConstant` defaults its post-shift to `GaugeShift.MeanZero` because a deflated solve re-acquires the constant mode through rounding; `OperatorSense.Shape` reports the SENSED operator's own extent, so one projection sizes the product's operands AND the solve's right-hand side and solution.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Arr`, `Seq`, `Option`), CSparse (the sensed apply and solve columns bind its array overloads).
- Growth: a new solve substrate is one `SolvePath`/`EigenSolvePath` row, the receipt shape unchanged; a new gauge modality is one `GaugePolicy` case whose `Switch` arms break at compile time; a direction is a `SolvePath` capability column plus the existing `OperatorSense` row, never a transposed twin of every route.
- Boundary: capability reads off the discriminant column, so a parallel `FactorKind` enum re-declaring the route space beside `SolvePath` never mints, and `TransposeSolve` is a column on the route because the transposed behavior binds to the concrete CSparse factor while the route itself is instance-free.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using DoubleDouble;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using Rasm.Domain;
using ComplexVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using DenseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorC = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using SparseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using SparseMatrixD = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EigenSolvePath {
    public static readonly EigenSolvePath DenseSymmetricEvd = new(key: 0, isSparse: false, isComplex: false);
    public static readonly EigenSolvePath DenseGeneralEvd = new(key: 1, isSparse: false, isComplex: true);
    public static readonly EigenSolvePath SparseLobpcg = new(key: 2, isSparse: true, isComplex: false);
    public static readonly EigenSolvePath SparseHermitianLobpcg = new(key: 3, isSparse: true, isComplex: true);
    // The congruence route DENSIFIES by construction and its row says so: the reduction needs M's triangular factor,
    // which the CSparse peer does not publish, and it solves against an n-by-n identity — a MathNet sparse operator
    // handed to Cholesky() would densify silently with no fill-reducing ordering, so the density rides the row honestly.
    public static readonly EigenSolvePath DenseGeneralizedCholeskyCongruence = new(key: 4, isSparse: false, isComplex: false);
    public bool IsSparse { get; }
    public bool IsComplex { get; }
}

// The receipt's ORDERING discriminant: three producers sort Pairs three ways, and a consumer reading Pairs[0]
// without this row silently assumes whichever mint it last saw — the dominant-mode reader wants DescendingMagnitude,
// the smallest-mode reader Ascending, and the dense general EVD emits factorization order unsorted.
[SmartEnum<int>]
public sealed partial class EigenOrder {
    public static readonly EigenOrder DescendingMagnitude = new(key: 0);
    public static readonly EigenOrder Ascending = new(key: 1);
    public static readonly EigenOrder Factorization = new(key: 2);
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

// The operator-application direction: ONE vocabulary serving the sparse product, the LU route, and the QR route, so a
// transposed product and a transposed solve are the same owner's second direction rather than direction-named siblings.
// Shape reports the SENSED operator's extent — A is (rows x cols) and A' is (cols x rows) — so one projection sizes the
// product's operands, the solve's right-hand side and solution, and the receipt's length claims, with no per-route flip.
// The columns bind the CSparse ARRAY overloads because the page's solve buffers are already arrays; the span twins carry
// the same semantics and would force a hand-written delegate type on a generated column.
[SmartEnum<int>]
public sealed partial class OperatorSense {
    public static readonly OperatorSense Forward = new(key: 0,
        shape: static (rows, cols) => (rows, cols),
        apply: static (operand, x, y) => operand.Multiply(x, y),
        accumulate: static (operand, alpha, x, beta, y) => operand.Multiply(alpha, x, beta, y),
        solveLu: static (factor, b, x) => factor.Solve(b, x),
        solveQr: static (factor, b, x) => factor.Solve(b, x),
        flipped: static () => Transposed);
    public static readonly OperatorSense Transposed = new(key: 1,
        shape: static (rows, cols) => (cols, rows),
        apply: static (operand, x, y) => operand.TransposeMultiply(x, y),
        accumulate: static (operand, alpha, x, beta, y) => operand.TransposeMultiply(alpha, x, beta, y),
        solveLu: static (factor, b, x) => factor.SolveTranspose(b, x),
        solveQr: static (factor, b, x) => factor.SolveTranspose(b, x),
        flipped: static () => Forward);

    [UseDelegateFromConstructor] internal partial (Dimension Rows, Dimension Cols) Shape(Dimension rows, Dimension cols);
    // The opposite direction as a row-to-row column, deferred behind a lambda because a field initializer reading a
    // later item captures null before materialization. The normal-equation projection is the one consumer: it applies
    // the sense the solve did NOT take, so neither direction spells its own transposed member.
    [UseDelegateFromConstructor] internal partial OperatorSense Flipped();
    [UseDelegateFromConstructor] internal partial void Apply(CSparse.Storage.CompressedColumnStorage<double> operand, double[] x, double[] y);
    [UseDelegateFromConstructor] internal partial void Accumulate(CSparse.Storage.CompressedColumnStorage<double> operand, double alpha, double[] x, double beta, double[] y);
    [UseDelegateFromConstructor] internal partial void SolveLu(CSparse.Double.Factorization.SparseLU factor, double[] rhs, double[] solution);
    [UseDelegateFromConstructor] internal partial void SolveQr(CSparse.Double.Factorization.SparseQR factor, double[] rhs, double[] solution);
}

// SparsePreconditioner is the Krylov axis — a ROW family, never a boolean beside the path: Diagonal is the Jacobi
// default, Milu0 the modified incomplete-LU row elliptic operators (a Neumann Laplacian, any SPD grid) want,
// row-sum-preserving and CSR-native, Ilutp the threshold-and-pivot ILU lane for the near-singular or
// indefinite grid where row-sum preservation breaks down — fill/drop/pivot spell the package defaults, and
// ILU0 stays unadmitted because its Approximate materializes a dense row per call. Each row carries its
// factory AND its honest receipt path, so the receipt never labels a unit-preconditioned run as Jacobi.
// Solve calls Initialize itself, so the factor re-pays per call by that package's own contract.
[SmartEnum<int>]
public sealed partial class SparsePreconditioner {
    public static readonly SparsePreconditioner None = new(key: 0,
        create: static () => new MathNet.Numerics.LinearAlgebra.Solvers.UnitPreconditioner<double>(),
        krylovPath: static () => SolvePath.SparseBiCgStabUnit);
    public static readonly SparsePreconditioner Diagonal = new(key: 1,
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.DiagonalPreconditioner(),
        krylovPath: static () => SolvePath.SparseBiCgStabDiagonal);
    public static readonly SparsePreconditioner Milu0 = new(key: 2,
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.MILU0Preconditioner(),
        krylovPath: static () => SolvePath.SparseBiCgStabMilu0);
    public static readonly SparsePreconditioner Ilutp = new(key: 3,
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.ILUTPPreconditioner(fillLevel: 200.0, dropTolerance: 1e-4, pivotTolerance: 0.0),
        krylovPath: static () => SolvePath.SparseBiCgStabIlutp);

    [UseDelegateFromConstructor] public partial MathNet.Numerics.LinearAlgebra.Solvers.IPreconditioner<double> Create();
    [UseDelegateFromConstructor] public partial SolvePath KrylovPath();
}

// TransposeSolve is the CAPABILITY column, never a transposed twin per route: CSparse publishes SolveTranspose on the
// LU and QR factors alone, so the column states which route admits an OperatorSense.Transposed request and the concrete
// factor resolves the direction through the sense row's own solve column.
[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0, isSparse: false, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath DenseQrLeastSquares = new(key: 1, isSparse: false, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath DenseCholesky = new(key: 2, isSparse: false, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath SparseBiCgStabDiagonal = new(key: 3, isSparse: true, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.Diagonal);
    public static readonly SolvePath SparseMathNetDirectFallback = new(key: 4, isSparse: true, isFallback: true, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath SparseCholesky = new(key: 5, isSparse: true, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath SparseLuIndefinite = new(key: 6, isSparse: true, isFallback: false, transposeSolve: true, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath SparseBiCgStabMilu0 = new(key: 7, isSparse: true, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.Milu0);
    public static readonly SolvePath SparseBiCgStabUnit = new(key: 8, isSparse: true, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.None);
    public static readonly SolvePath SparseBiCgStabIlutp = new(key: 9, isSparse: true, isFallback: false, transposeSolve: false, preconditioner: SparsePreconditioner.Ilutp);
    public static readonly SolvePath SparseQrLeastSquares = new(key: 10, isSparse: true, isFallback: false, transposeSolve: true, preconditioner: SparsePreconditioner.None);
    public bool IsSparse { get; }
    public bool IsFallback { get; }
    public bool TransposeSolve { get; }
    public SparsePreconditioner Preconditioner { get; }
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

- Owner: `Matrix` the dense row-major owner and `SymmetricMatrix` the packed-upper owner, both `Of`-gated on shape and span finiteness; the decomposition carriers hold their live MathNet factor `internal` so repeated right-hand sides stream through one factor (the held-handle law), only typed receipts crossing the public surface.
- Entry: every fallible operation returns `Fin<T>`; `Of` admits through one vectorized span check, never a strided per-element loop.
- Auto: `Norm` dispatches through the `MatrixNormKind` compute column, and `SymmetricMatrix.At` folds `(min, max)` into the triangular index so a written entry mirrors by construction; `SymmetricMatrix.FlatIndex` is the ONE packed-upper triangular-address mint — `SampleMoment`'s indexer and `Lm.PackedIndex` delegate to it, so the layout formula moves as one edit; `Matrix.AsPlane` projects the row-major buffer as the admitted `ReadOnlySpan2D<double>` plane substrate, so a 2D-structured consumer windows, row-rails, and blits through the toolkit family instead of hand-indexing `(i * Cols + j)`.
- Packages: MathNet.Numerics (dense factorizations and norms), System.Numerics.Tensors (finiteness admission), CommunityToolkit.HighPerformance (`ReadOnlySpan2D` plane projection), LanguageExt.Core, Thinktecture.Runtime.Extensions (`Dimension`).
- Growth: a new dense decomposition adds one `Decompose*` member returning a typed carrier and one `SolvePath` row, never a sibling matrix type; a norm is one `MatrixNormKind` row.
- Boundary: MathNet types never cross the public surface — `Matrix`/`Arr<double>` in, typed receipts out, the `internal` factor handles the held-handle exception. Symmetric consumers construct `SymmetricMatrix`, never a dense `Matrix` asserted symmetric — MathNet's `IsSymmetric()` compares with exact `!=`, which accumulation-built operators fail.

```csharp signature
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
    // AsPlane views the row-major buffer as the admitted 2D plane substrate: Slice windows a sub-plane, GetRowSpan is the
    // row rail, CopyTo blits — a consumer hand-indexing (i * Cols + j) over Entries is the deleted form.
    public ReadOnlySpan2D<double> AsPlane() => Entries.AsSpan().AsSpan2D(height: Rows.Value, width: Cols.Value);
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
    public Fin<CholeskyResult> DecomposeCholesky(Op? key = null) => MatrixKernel.Cholesky(matrix: this, key: key.OrDefault());
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        this with { Upper = Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value) };
    // THE packed-upper triangular-address mint: SampleMoment's indexer and Lm.PackedIndex delegate here, so
    // one edit moves the layout formula everywhere and drift across the three former hand-kept copies is unrepresentable.
    internal static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
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

- Owner: `SparseMatrix` the CSR owner whose `IsValid` is its admission — factorizing invalid storage produces silently wrong factors; `SparseHermitian` the complex upper-store owner with conjugate reconstruction on multiply and a real-diagonal gate; `CholeskySparse` the SPD factor cache over CSparse `SparseCholesky` under AMD, `Lock`-guarded because CSparse solves share scratch and a concurrent second solve corrupts both results silently, success-only so a broken factor never enters reuse, and the sole owner of the three factor-movement verbs the cached symbolic analysis makes cheap.
- Entry: `FromTriplets` admits any triplet stream — duplicates sum, zeros drop, out-of-range or non-finite fails typed — so consumers assemble by accumulation, never hand-build CSR; `CholeskySparse.Of` catches the CSparse bare-`Exception` pivot loss into the typed rail and takes the caller's `IProgress<double>` so a long factorization reports its symbolic and numeric phases on the same governance surface the arrangement band carries; `SolveLeastSquaresDetailed` is the ONE rectangular route — CSparse `SparseQR` under the `A'A` ordering, minimizer at `m >= n` and minimum-norm below, witnessed on the normal-equation residual — and both transposable routes take an `OperatorSense` rather than mint a transposed twin; the Krylov entry takes an optional `KrylovStop` that becomes a criterion ROW on the ladder rather than a check the caller runs after the budget is already spent.
- Auto: `SparseHermitian.IsValid` gates the stored diagonal real within a scale-relative band, so a drifted assembly is caught at the owner, not inside LOBPCG; `Refactorize` re-runs the numeric phase against the cached `SymbolicFactorization`, so a parameter sweep over one pattern pays the AMD analysis once, and `Update`/`Downdate` move the standing factor by a rank-1 term so an added or removed constraint costs no refactorization — a `false` verdict means the partial tree walk already corrupted the factor and the caller re-mints rather than retries.
- Packages: CSparse (SPD, LU, and QR factorization, AMD ordering, `Refactorize`/`Update`/`Downdate`, the `IProgress<double>` phase report `Refactorize` deliberately carries none of), MathNet.Numerics (sparse storage and its duplicate-summing and diagonal-populating repairs, BiCgStab and criterion stack), Rasm.Domain (`Admit.FiniteComplexSpan`/`HermitianDiagonalRealSpan`, the one complex-spectrum gate pair), LanguageExt.Core, BCL (`System.Threading.Lock`, `System.Numerics.Complex`, `IProgress<double>`).
- Growth: a new sparse capability adds one member and one column over the same owners — an LDL' inertia read and a second fill-reducing ordering are the open axes; a second CSR/CSC representation beside `SparseMatrix` is the deleted form — format bridges live inside `MatrixKernel`.
- Boundary: mesh Laplacian memoization caches these factor objects, so their identity and `Lock` semantics compose from here; a transposed solve is linear-algebra vocabulary on the standing factor and nothing more — the adjoint sensitivity band that composes it stays `Rasm.Compute`'s, and a kernel differentiation rail beside `Lm`'s forward-mode dual floor never mints here.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// A domain halt condition as a ROW on the criterion ladder rather than a post-loop check: the delegate criterion mirrors
// the iterator's own status contract, so the rule runs INSIDE the iteration and stops the solver where a post-loop test
// would only observe the budget already spent. It reads the iteration index and the residual norm — kernel currency —
// so no MathNet vector crosses the public surface, and a criterion reset clears only the held status, so a rule closing
// over an absolute start instant survives the reset a composite ladder performs per rung.
public readonly record struct KrylovStop(Func<int, double, bool> Halt);

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
    // ONE product entry carrying both directions: the sense row's Shape names the SENSED operator, so the operand gate
    // reads its column count and the result its row count with no second member and no per-direction length arithmetic.
    public Fin<Arr<double>> Multiply(Arr<double> vector, OperatorSense? sense = null, Op? key = null) =>
        MatrixKernel.SparseProduct(self: this, x: vector, sense: sense ?? OperatorSense.Forward, key: key.OrDefault());
    public Matrix ToDense() => MatrixKernel.SparseToDense(self: this);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) => MatrixKernel.SparseSolve(matrix: this, rhs: rhs, key: key.OrDefault());

    public Fin<SolveReceipt> SolveIterativeDetailed(Arr<double> rhs, SparsePreconditioner preconditioner, double tolerance, int maxIterations, Option<KrylovStop> stop = default, Op? key = null) =>
        MatrixKernel.SparseSolveIterative(matrix: this, rhs: rhs, preconditioner: preconditioner, tolerance: tolerance, maxIterations: maxIterations, stop: stop, key: key.OrDefault());
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) => SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<SolveReceipt> SingularSolveDetailed(Arr<double> rhs, GaugePolicy gauge, Context context, Op? key = null) =>
        MatrixKernel.SingularGaugeSolve(matrix: this, rhs: rhs, gauge: gauge, context: context, key: key.OrDefault());
    public Fin<Arr<double>> SingularSolve(Arr<double> rhs, GaugePolicy gauge, Context context, Op? key = null) =>
        SingularSolveDetailed(rhs: rhs, gauge: gauge, context: context, key: key.OrDefault()).Map(static result => result.Solution);
    // Both direct routes are transposable on ONE numeric factorization: CSparse runs the standing L/U and Q/R factors in
    // reverse order for A'x = b, so a transposed request assembles no explicit transpose and analyses no second pattern.
    public Fin<SolveReceipt> SolveIndefiniteDetailed(Arr<double> rhs, OperatorSense? sense = null, double pivotTolerance = 1.0, IProgress<double>? progress = null, Op? key = null) =>
        MatrixKernel.SparseLuSolve(matrix: this, rhs: rhs, sense: sense ?? OperatorSense.Forward, pivotTolerance: pivotTolerance, progress: progress, key: key.OrDefault());
    public Fin<Arr<double>> SolveIndefinite(Arr<double> rhs, OperatorSense? sense = null, double pivotTolerance = 1.0, Op? key = null) =>
        SolveIndefiniteDetailed(rhs: rhs, sense: sense, pivotTolerance: pivotTolerance, key: key.OrDefault()).Map(static result => result.Solution);
    // SolveLeastSquaresDetailed is the ONE rectangular sparse route: Householder QR over the CSC bridge. At m >= n the least-squares
    // minimizer, below the minimum-norm solution — CSparse transposes internally, so one member serves both.
    public Fin<SolveReceipt> SolveLeastSquaresDetailed(Arr<double> rhs, OperatorSense? sense = null, IProgress<double>? progress = null, Op? key = null) =>
        MatrixKernel.SparseQrSolve(matrix: this, rhs: rhs, sense: sense ?? OperatorSense.Forward, progress: progress, key: key.OrDefault());
    public Fin<Arr<double>> SolveLeastSquares(Arr<double> rhs, OperatorSense? sense = null, Op? key = null) =>
        SolveLeastSquaresDetailed(rhs: rhs, sense: sense, key: key.OrDefault()).Map(static result => result.Solution);
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
    // The progress report is the caller's governance surface, not a knob: an absent reporter selects the two-argument
    // Create overload rather than handing CSparse a null, and Refactorize deliberately carries none — its symbolic
    // phase is already cached, so the only phase left to report is the numeric sweep the caller already bounded.
    public static Fin<CholeskySparse> Of(SparseMatrix symmetric, IProgress<double>? progress = null, Op? key = null) =>
        symmetric.Rows.Value != symmetric.Cols.Value || !symmetric.IsValid
            ? Fin.Fail<CholeskySparse>(error: key.OrDefault().InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric, key: key.OrDefault())
              from factor in key.OrDefault().Catch(() => Fin.Succ(Optional(progress).Match(
                  Some: report => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, progress: report),
                  None: () => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA))))
              select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
    // Source moves WITH the factor under the one solve lock: the CSparse factor is mutable by construction, so a
    // get-only Source would report the values a refactorized or rank-1-moved factor no longer holds and every residual
    // witness minted afterwards would measure against an operator the factor abandoned.
    public SparseMatrix Source { get; private set; }
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

    // Numeric re-run on the cached AMD pattern: a parameter sweep re-forming values alone pays the symbolic analysis
    // once. CSparse binds Refactorize to the cached SymbolicFactorization and reads the incoming pointers AS that
    // pattern, so a drifted pattern yields a silently wrong factor rather than a throw — the congruence gate is the
    // only thing standing between a sweep and that silence.
    public Fin<CholeskySparse> Refactorize(SparseMatrix values, Op? key = null) {
        Op op = key.OrDefault();
        return !IsValid || !values.IsValid || !SharesPattern(values: values)
            ? Fin.Fail<CholeskySparse>(error: op.InvalidInput())
            : MatrixKernel.ToCSparseSymmetric(s: values, key: op).Bind(csc => op.Catch(() => {
                lock (solveLock) {
                    Factor.Refactorize(A: csc);
                    Source = values;
                }
                return Fin.Succ(this);
            }));
    }
    // Rank-1 factor movement: one added or removed constraint moves L·L' by ±w·w' with no refactorization. The two
    // entries differ by a data pair — the outer-product sign the source re-assembly folds and the CSparse verb — never
    // by a branch on a sign literal. A false verdict means the partial tree walk already corrupted the factor, so the
    // rail fails and the caller re-mints; a retry compounds the corruption it inherited.
    public Fin<CholeskySparse> Update(SparseMatrix column, Op? key = null) =>
        Move(column: column, scale: 1.0, move: static (factor, w) => factor.Update(w: w), key: key.OrDefault());
    public Fin<CholeskySparse> Downdate(SparseMatrix column, Op? key = null) =>
        Move(column: column, scale: -1.0, move: static (factor, w) => factor.Downdate(w: w), key: key.OrDefault());
    private Fin<CholeskySparse> Move(SparseMatrix column, double scale, Func<CSparse.Double.Factorization.SparseCholesky, CSparse.Storage.CompressedColumnStorage<double>, bool> move, Op key) =>
        !IsValid || !column.IsValid || column.Rows.Value != Order.Value || column.Cols.Value != 1
            ? Fin.Fail<CholeskySparse>(error: key.InvalidInput())
            : from moved in MatrixKernel.RankOneMoved(source: Source, column: column, scale: scale, key: key)
              from carrier in key.Catch(() => Fin.Succ(CSparse.Double.SparseMatrix.OfIndexed(
                  rows: Order.Value, columns: 1, enumerable: MatrixKernel.SparseTripletsOf(matrix: column))))
              from committed in key.Catch(() => {
                  lock (solveLock) {
                      if (!move(arg1: Factor, arg2: carrier)) return Fin.Fail<CholeskySparse>(key.InvalidResult());
                      Source = moved;
                      return Fin.Succ(this);
                  }
              })
              select committed;
    // Pattern congruence over the stored index arrays — a span comparison, cheap beside the numeric sweep it guards.
    private bool SharesPattern(SparseMatrix values) =>
        values.Rows.Value == Source.Rows.Value && values.Cols.Value == Source.Cols.Value && values.NonZeros == Source.NonZeros
        && values.RowPtr.AsSpan().SequenceEqual(Source.RowPtr.AsSpan()) && values.ColInd.AsSpan().SequenceEqual(Source.ColInd.AsSpan());
}
```

## [05]-[RECEIPTS]

- Owner: `SolveReceipt` the one linear-solve evidence carrier, `EigenSolveReceipt<TEigen, TVector>` the one eigen carrier generic over real or complex spectra, and `GaugeReceipt` the singular-solve evidence — all three spelling the rails fold `IsValid => ValidityClaim.All(…)` under `IValidityEvidence`.
- Entry: receipts mint only at the `MatrixKernel` exits (`SolveSuccess`, `EigenReceiptOf`) under the two-tier evidence law — hard numerical garbage never mints and fails typed, a usable-stop receipt is gated valid before release, and a non-usable-stop receipt (`RankDeficient`, `IterativeExhausted`, KKT breakdown) is the witnessed refusal the caller reads off `Stop.IsUsable` before consuming the solution. Consuming a minted receipt without reading its stop is the named consumer defect.
- Auto: each `IsValid` conjoins the mechanical field-shape gates with the semantic couplings — residual within tolerance, iterations within budget, and the length pair read off the SENSED operator so a transposed solve is validated against `A'`'s own extent rather than the stored one — one claim row per invariant; the sense-versus-capability coupling refuses a transposed receipt on a route whose `TransposeSolve` column denies it, so a mislabelled direction cannot mint.
- Packages: Rasm.Domain (`IValidityEvidence` and `ValidityClaim`, the rails validity floor), LanguageExt.Core (`Option`, `Seq`, `Arr`).
- Growth: new evidence is one field and at most one claim row; a new outcome family is one receipt type only when its evidence shape is disjoint (the eigen/solve/gauge split), never per-algorithm receipt clones.
- Boundary: `Option<T>` carries absence of evidence (`Iterations` on direct solves), never a sentinel; the stored residual is always recomputed against the original operator — a preconditioned or factor-reconstructed residual is the named lying witness.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Rows and Cols are the STORED operator's extent and Sense names the direction the route ran, so the length claims
// read the sensed pair rather than the stored one — a transposed solve over an m-by-n operator carries an n-length
// right-hand side and an m-length solution, and validating it against the stored extent would reject every correct one.
public readonly record struct SolveReceipt(
    Arr<double> Solution, SolvePath Path, SolveStop Stop, OperatorSense Sense, Dimension Rows, Dimension Cols, int RhsLength,
    Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, double Residual,
    Option<bool> FullRank, Option<int> InputNonZeros, Option<int> FactorNonZeros, Option<GaugeReceipt> Gauge = default) : IValidityEvidence {
    public bool IsValid {
        get {
            SolvePath path = Path;
            Option<int> maxIterations = MaxIterations;
            (Dimension sensedRows, Dimension sensedCols) = Sense.Shape(rows: Rows, cols: Cols);
            return ValidityClaim.All(
                ValidityClaim.Of(Path is not null && Stop is not null && Sense is not null && Stop.IsUsable),
                ValidityClaim.Of(Sense.Equals(OperatorSense.Forward) || path.TransposeSolve),
                ValidityClaim.CountExactly(count: RhsLength, expected: sensedRows.Value),
                ValidityClaim.Nonnegative(value: Residual),
                ValidityClaim.Of(Residual <= Tolerance.IfNone(double.PositiveInfinity)),
                ValidityClaim.CountExactly(count: Solution.Count, expected: sensedCols.Value),
                ValidityClaim.Finite(Solution.AsSpan()),
                ValidityClaim.Of(Iterations.Map(static iter => iter >= 0).IfNone(noneValue: true) && InputNonZeros.Map(static nz => nz >= 0).IfNone(noneValue: true) && FactorNonZeros.Map(static nz => nz >= 0).IfNone(noneValue: true)),
                ValidityClaim.Of(FullRank.Map(rank => !path.Equals(SolvePath.DenseQrLeastSquares) || rank).IfNone(noneValue: true)),
                ValidityClaim.Of(FactorNonZeros.Map(nz => !path.Equals(SolvePath.SparseCholesky) || nz > 0).IfNone(noneValue: true)),
                ValidityClaim.Of(Iterations.Map(iter => maxIterations.Map(max => max >= iter).IfNone(noneValue: true)).IfNone(noneValue: true)),
                ValidityClaim.Of(Gauge.Map(static gauge => gauge.IsValid).IfNone(noneValue: true)));
        }
    }
}

// Solve-path evidence is a closed union — the iterative triple, the factor count, and the direct route are mutually
// exclusive by construction, so an iterative receipt carrying a factor count is unrepresentable and the receipt's
// IsValid adjudicates one total Switch instead of vacuous per-column absents. The Option projections serve
// order-independent consumers that fold whichever axis their own receipt re-publishes.
[Union]
public abstract partial record EigenPathEvidence {
    public sealed record Direct : EigenPathEvidence;
    public sealed record Iterative(int Iterations, int MaxIterations, double Tolerance) : EigenPathEvidence;
    public sealed record Factored(int FactorNonZeros) : EigenPathEvidence;

    public Option<int> Iterations => Switch(
        direct: static _ => Option<int>.None,
        iterative: static path => Some(path.Iterations),
        factored: static _ => Option<int>.None);
    public Option<int> FactorNonZeros => Switch(
        direct: static _ => Option<int>.None,
        iterative: static _ => Option<int>.None,
        factored: static path => Some(path.FactorNonZeros));
}

public readonly record struct EigenSolveReceipt<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path, EigenSolveStop Stop, EigenOrder Order,
    int RequestedPairs, int ReturnedPairs, EigenPathEvidence Evidence, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Path is not null && Stop is not null && Stop.IsUsable && Order is not null && Evidence is not null),
        ValidityClaim.Of(RequestedPairs >= 1 && ReturnedPairs > 0 && ReturnedPairs <= RequestedPairs && Pairs.Count == ReturnedPairs),
        ValidityClaim.Nonnegative(value: MaxResidual),
        ValidityClaim.Of(Evidence.Switch(
            state: MaxResidual,
            direct: static (_, _) => true,
            iterative: static (residual, path) => path.Iterations >= 0 && path.MaxIterations >= path.Iterations && residual <= path.Tolerance,
            factored: static (_, path) => path.FactorNonZeros > 0)));

    // The ordering contract is DEMANDED, never assumed: a positional consumer names the order its reads rely on and
    // the rail breaks if the mint's order ever changes. No re-sort is served — DescendingMagnitude and Ascending are
    // reverse orders only on a nonnegative spectrum, a fact this receipt cannot witness; an order-independent fold
    // reads Pairs directly.
    public Fin<Seq<(TEigen Eigenvalue, TVector Eigenvector)>> PairsIn(EigenOrder expected, Op? key = null) =>
        IsValid && Order.Equals(expected)
            ? Fin.Succ(Pairs)
            : Fin.Fail<Seq<(TEigen Eigenvalue, TVector Eigenvector)>>(key.OrDefault().InvalidResult());
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

## [06]-[TRANSFORM_BAND]

- Owner: `SpectralArena` is the ONE transform carrier — four cases, each holding the buffer layout its MathNet entrypoint owns and exactly the extent its arm consumes; `SpectralReceipt` the evidence a transform leaves; `TapSeries` the admitted sample-domain convolution kernel, `TapBorder` its closed out-of-extent vocabulary, and `TapWindow` the staged-window geometry a banded caller states; `WindowTaper` the taper roster carrying FFT framing as a column on the row rather than a second roster; `InterpolationRoute` the twelve-scheme fit route and `Interpolant` the fitted carrier whose case states what the fitted instance will serve.
- Entry: `arena.Transform(sense, scaling, key)` is the one transform entry and the arena case is its discriminant, so no per-carrier entrypoint family and no mode flag exist; `series.Convolve(source, folded, window, border, key)` folds one strided axis in the sample domain and `TapSeries.Convolve(values, lattice, axes, border, key)` is its separable lattice form — one series per axis, arity in the request shape, never an entrypoint sibling; `WindowTaper.Of(width, framing, shape, key)` samples a taper; `InterpolationRoute.Fit(points, values, slopes, key)` mints a carrier, and `Interpolant.OfSegments`/`OfTransformed` reach the two schemes the factory roster omits — the quadratic spline through its own coefficient constructor, the transformed fit through its own static.
- Auto: rank 2 and rank 3 ARE the row-column fold over the managed-complete 1D pair, and symmetric scaling composes per axis (`1/sqrt(w) · 1/sqrt(h) = 1/sqrt(w·h)`), so the folded transform carries the convention the 1D row declares and `RoundTripFactor` reads the cell count once; the tap fold divides every output by its RESOLVED-weight sum, so partition of unity holds at every border by construction — no caller pre-normalizes a table, a series and its scalar multiple fold identically, a `Zero`-dropped tap leaves the divisor rather than darkening the rim, and a rim record whose resolved sum cancels refuses typed rather than certifying a fabricated zero sample — and its lattice form is the SAME per-axis line fold the rank-2/3 transform law declares, walking the lattice's own linearization strides; `Power` reads `MagnitudeSquared` on the interleaved bins and the vectorized `Multiply`-then-`MultiplyAdd` pair on the split spans — the reason the split case exists — and never a square root it only squares back; `Axis` derives the bin count from the lattice census and the sampling rate from the affine's own per-axis column norm, so a spectrum reads its own axis instead of a caller-passed rate that can disagree with the grid.
- Packages: MathNet.Numerics (`IntegralTransforms.Fourier` interleaved, split, and packed pairs, `FrequencyScale`, `Hartley.NaiveForward`/`NaiveInverse`, `FourierOptions`/`HartleyOptions`, the `Window` roster, `Interpolate`, `Interpolation.IInterpolation`/`QuadraticSpline`/`TransformedInterpolation`, `ComplexExtensions.MagnitudeSquared`), System.Numerics.Tensors (`TensorPrimitives.Multiply`/`MultiplyAdd`/`Sum`/`IsFiniteAll`), `Numerics/atoms` (`CellLattice` the addressing carrier, `Dimension`, `PositiveMagnitude`, `UnitInterval`, `SignedAxis`, `EpsilonPolicy`), Rasm.Domain (`Op`, `Admit.FiniteComplexSpan`, the validity fold), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL (`System.Numerics.Complex`).
- Growth: a taper is one roster row and its FFT-framing twin a column on that row; a border law is one `TapBorder` row every tap fold reads with no kernel edit; an interpolation scheme is one route row; a scaling convention is one `SpectralScaling` row governing both transform owners at once, its `FourierConvention`/`HartleyConvention` columns published so a package binding MathNet's transform entrypoints directly reads this row instead of forking a second convention vocabulary; a buffer layout is one `SpectralArena` case whose arms break every fold at compile time.
- Boundary: `Fourier.Forward2D`/`Inverse2D`/`ForwardMultiDim`/`InverseMultiDim` never spell in a fence — all four route to the multidim provider seam whose managed realization throws `NotSupportedException`, and the admitted native adapters ship no arm64 asset, so the page's own managed-provider pin makes them unservable by construction and the separable fold IS the platform-total N-dimensional transform. Every transform overwrites the caller's arena, so an immutable spectrum value is unrepresentable and the receipt names the arena the result lives in — the same instance for the three in-place cases, a fresh one for the Hartley case, the sole entrypoint that allocates its output. Separable convolution has NO package primitive — `System.Numerics.Tensors` carries no `Conv1D`, `Conv2D`, `Conv3D`, or `MatMul` on `TensorPrimitives` or on the `Tensor` static owner — so this band owns BOTH routes of the one convolution correspondence itself: the pointwise spectral product between the transform legs (`SpectralReceipt.Modulate`) and the sample-domain tap fold (`TapSeries.Convolve`); a consumer composes one of the two and spells no fold of its own, while its tap GENERATION — which weights fill the series — stays the consumer's domain policy. Zero-sum series are DIFFERENCE stencils and refuse at the mint: `Numerics/calculus#NABLA` owns those, so the two owners partition on the tap sum rather than overlapping. `CellLattice` is the addressing carrier for a lattice-backed plane and the band mints no second linearization, no sibling 2D arena, and no strided-view owner beside it — the `Tensor<T>` plane stays refused on four structural grounds: array-only static entrypoints at the mint, `ref struct` span views that cannot cross the `Fin` rail, an allocating `PermuteDimensions` on every transpose, and this carrier's one-linearization law; `MathNet.Numerics.Interpolation` is one-dimensional whole, so a bicubic or scattered-surface reconstruction is the regression route's and never a row here.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The transform convention is a DECLARED policy row, never an assumed default and never a literal at a call site:
// Default scales symmetrically so a forward-inverse pair is the identity, AsymmetricScaling scales the inverse alone to
// the same identity, and NoScaling omits both so the round trip carries a factor of N. FourierOptions and HartleyOptions
// spell those three conventions at the same ordinals, so one row governs both transform owners. RoundTrip reads the CELL
// count, so a separable fold composes per axis and the rank-3 factor is the axis-length product with no call-site arithmetic.
[SmartEnum<int>]
public sealed partial class SpectralScaling {
    public static readonly SpectralScaling Symmetric = new(key: 0,
        fourierConvention: FourierOptions.Default, hartleyConvention: HartleyOptions.Default, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling AsymmetricInverse = new(key: 1,
        fourierConvention: FourierOptions.AsymmetricScaling, hartleyConvention: HartleyOptions.AsymmetricScaling, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling Unscaled = new(key: 2,
        fourierConvention: FourierOptions.NoScaling, hartleyConvention: HartleyOptions.NoScaling, roundTrip: static cells => (double)cells);
    // Both convention columns are PUBLIC: a consumer binding MathNet's own transform entrypoints — Compute's
    // `Stats/signal` spectra and `Tensor/quadrature` folds — reads the declared row rather than re-spelling a
    // second `FourierOptions` vocabulary no compiler joins to this one.
    public FourierOptions FourierConvention { get; }
    public HartleyOptions HartleyConvention { get; }
    [UseDelegateFromConstructor] public partial double RoundTrip(long cells);
}

// Direction as a row carrying the four entrypoint pairs it names, so an arena's arm invokes the column and no arm ever
// branches on a direction. The packed pair refuses FourierOptions.InverseExponent outright; none of the three admitted
// convention rows sets it, so the packed arm reaches the provider unconditionally.
[SmartEnum<int>]
public sealed partial class SpectralSense {
    public static readonly SpectralSense Forward = new(key: 0,
        interleaved: Fourier.Forward, split: Fourier.Forward, packed: Fourier.ForwardReal, realValued: Hartley.NaiveForward);
    public static readonly SpectralSense Inverse = new(key: 1,
        interleaved: Fourier.Inverse, split: Fourier.Inverse, packed: Fourier.InverseReal, realValued: Hartley.NaiveInverse);
    [UseDelegateFromConstructor] internal partial void Interleaved(Complex[] arena, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Split(double[] real, double[] imaginary, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Packed(double[] arena, int samples, FourierOptions options);
    [UseDelegateFromConstructor] internal partial double[] RealValued(double[] samples, HartleyOptions options);
}

// Filter-design versus FFT framing is a COLUMN on the taper roster: the bare factory is the symmetric filter-design form
// and the *Periodic twin the FFT-framing one, and only four of the fifteen rows carry both. A row asked for a framing it
// does not serve refuses typed rather than substitute the form it happens to have.
[SmartEnum<int>]
public sealed partial class TaperFraming {
    public static readonly TaperFraming FilterDesign = new(key: 0);
    public static readonly TaperFraming FftFrame = new(key: 1);
}

// The two shaped rows' parameter, each case carrying exactly the admission its factory consumes: a Gauss sigma is a
// positive magnitude relative to the half-width and a Tukey fraction is unit-bounded, so one shared scalar slot would
// admit a sigma of any size and a fraction above one.
[Union]
public abstract partial record TaperShape {
    private TaperShape() { }
    public sealed record Spread(PositiveMagnitude Sigma) : TaperShape;
    public sealed record Tapered(UnitInterval Fraction) : TaperShape;
}

internal delegate Fin<Arr<double>> TaperKernel(int width, Option<TaperShape> shape, TaperFraming framing, Op key);

// The taper roster as ROWS: a taper is a row binding the package factory, never a hand-authored coefficient loop. Three
// shared bodies serve every row — Fixed carries the symmetric design plus the optional FFT-framing twin, the two shaped
// bodies bind the parameterized factories — so a row is one line and its framing reach is readable in the row itself.
[SmartEnum<int>]
public sealed partial class WindowTaper {
    public static readonly WindowTaper Hann = new(key: 0, sample: Fixed(Window.Hann, Some<Func<int, double[]>>(Window.HannPeriodic)));
    public static readonly WindowTaper Hamming = new(key: 1, sample: Fixed(Window.Hamming, Some<Func<int, double[]>>(Window.HammingPeriodic)));
    public static readonly WindowTaper Cosine = new(key: 2, sample: Fixed(Window.Cosine, Some<Func<int, double[]>>(Window.CosinePeriodic)));
    public static readonly WindowTaper Lanczos = new(key: 3, sample: Fixed(Window.Lanczos, Some<Func<int, double[]>>(Window.LanczosPeriodic)));
    public static readonly WindowTaper Blackman = new(key: 4, sample: Fixed(Window.Blackman, None));
    public static readonly WindowTaper BlackmanHarris = new(key: 5, sample: Fixed(Window.BlackmanHarris, None));
    public static readonly WindowTaper BlackmanNuttall = new(key: 6, sample: Fixed(Window.BlackmanNuttall, None));
    public static readonly WindowTaper Nuttall = new(key: 7, sample: Fixed(Window.Nuttall, None));
    public static readonly WindowTaper FlatTop = new(key: 8, sample: Fixed(Window.FlatTop, None));
    public static readonly WindowTaper Bartlett = new(key: 9, sample: Fixed(Window.Bartlett, None));
    public static readonly WindowTaper BartlettHann = new(key: 10, sample: Fixed(Window.BartlettHann, None));
    public static readonly WindowTaper Triangular = new(key: 11, sample: Fixed(Window.Triangular, None));
    public static readonly WindowTaper Dirichlet = new(key: 12, sample: Fixed(Window.Dirichlet, None));
    public static readonly WindowTaper Gauss = new(key: 13, sample: SigmaShaped(Window.Gauss));
    public static readonly WindowTaper Tukey = new(key: 14, sample: FractionShaped(Window.Tukey));

    internal TaperKernel Sample { get; }
    public Fin<Arr<double>> Of(Dimension width, TaperFraming framing, Option<TaperShape> shape = default, Op? key = null) =>
        framing is null ? Fin.Fail<Arr<double>>(error: key.OrDefault().InvalidInput()) : Sample(width.Value, shape, framing, key.OrDefault());

    private static TaperKernel Fixed(Func<int, double[]> design, Option<Func<int, double[]>> framed) =>
        (width, shape, framing, key) => shape.IsSome
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : framing.Switch(
                filterDesign: () => Fin.Succ(new Arr<double>(design(arg: width))),
                fftFrame: () => framed.Match(
                    Some: twin => Fin.Succ(new Arr<double>(twin(arg: width))),
                    None: () => Fin.Fail<Arr<double>>(key.InvalidInput())));
    private static TaperKernel SigmaShaped(Func<int, double, double[]> design) =>
        (width, shape, framing, key) => framing.Switch(
            filterDesign: () => shape.Match(
                Some: value => value.Switch(
                    spread: spread => Fin.Succ(new Arr<double>(design(arg1: width, arg2: spread.Sigma.Value))),
                    tapered: _ => Fin.Fail<Arr<double>>(key.InvalidInput())),
                None: () => Fin.Fail<Arr<double>>(key.InvalidInput())),
            fftFrame: () => Fin.Fail<Arr<double>>(key.InvalidInput()));
    private static TaperKernel FractionShaped(Func<int, double, double[]> design) =>
        (width, shape, framing, key) => framing.Switch(
            filterDesign: () => shape.Match(
                Some: value => value.Switch(
                    spread: _ => Fin.Fail<Arr<double>>(key.InvalidInput()),
                    tapered: tapered => Fin.Succ(new Arr<double>(design(arg1: width, arg2: tapered.Fraction.Value)))),
                None: () => Fin.Fail<Arr<double>>(key.InvalidInput())),
            fftFrame: () => Fin.Fail<Arr<double>>(key.InvalidInput()));
}

// TapBorder closes the tap fold's out-of-extent law at four rows, each carrying its address fold as the
// column the kernel reads — so no fold body re-derives an addressing rule per tap. Clamp repeats the border
// sample, Wrap reaches the opposite edge, Mirror reflects about the border sample without repeating it, and
// Zero resolves NEGATIVE: an absent tap the fold drops from its weight sum, never a fabricated sample —
// which is what a caller whose staging already resolved its own edge law by address composes.
[SmartEnum<int>]
public sealed partial class TapBorder {
    public static readonly TapBorder Clamp = new(key: 0, resolve: static (index, extent) => Math.Clamp(value: index, min: 0, max: extent - 1));
    public static readonly TapBorder Wrap = new(key: 1, resolve: static (index, extent) => ((index % extent) + extent) % extent);
    public static readonly TapBorder Mirror = new(key: 2, resolve: static (index, extent) => Reflected(index: index, extent: extent));
    public static readonly TapBorder Zero = new(key: 3, resolve: static (_, _) => -1);

    [UseDelegateFromConstructor] internal partial int Resolve(int index, int extent);

    // Reflection about the border sample — period (extent−1)·2 — so the edge never doubles the way a repeated
    // border tap would brighten a rim under a weighting kernel.
    private static int Reflected(int index, int extent) {
        int period = Math.Max(val1: 1, val2: (extent - 1) * 2);
        int folded = ((index % period) + period) % period;
        return folded < extent ? folded : period - folded;
    }
}

internal delegate Fin<IInterpolation> FitKernel(Arr<double> points, Arr<double> values, Option<Arr<double>> slopes, Op key);

// Twelve factory schemes as rows on ONE route. The Hermite row is the only one consuming prescribed slopes, so the slope
// series rides an Option the row admits or refuses rather than a dead third argument eleven rows would ignore.
[SmartEnum<int>]
public sealed partial class InterpolationRoute {
    public static readonly InterpolationRoute CubicSpline = new(key: 0, kernel: Sampled(Interpolate.CubicSpline));
    public static readonly InterpolationRoute CubicSplineRobust = new(key: 1, kernel: Sampled(Interpolate.CubicSplineRobust));
    public static readonly InterpolationRoute CubicSplineMonotone = new(key: 2, kernel: Sampled(Interpolate.CubicSplineMonotone));
    public static readonly InterpolationRoute CubicSplineWithDerivatives = new(key: 3, kernel: Sloped(Interpolate.CubicSplineWithDerivatives));
    public static readonly InterpolationRoute Common = new(key: 4, kernel: Sampled(Interpolate.Common));
    public static readonly InterpolationRoute RationalWithoutPoles = new(key: 5, kernel: Sampled(Interpolate.RationalWithoutPoles));
    public static readonly InterpolationRoute RationalWithPoles = new(key: 6, kernel: Sampled(Interpolate.RationalWithPoles));
    public static readonly InterpolationRoute Polynomial = new(key: 7, kernel: Sampled(Interpolate.Polynomial));
    public static readonly InterpolationRoute PolynomialEquidistant = new(key: 8, kernel: Sampled(Interpolate.PolynomialEquidistant));
    public static readonly InterpolationRoute Linear = new(key: 9, kernel: Sampled(Interpolate.Linear));
    public static readonly InterpolationRoute LogLinear = new(key: 10, kernel: Sampled(Interpolate.LogLinear));
    public static readonly InterpolationRoute Step = new(key: 11, kernel: Sampled(Interpolate.Step));

    internal FitKernel Kernel { get; }
    public Fin<Interpolant> Fit(Arr<double> points, Arr<double> values, Option<Arr<double>> slopes = default, Op? key = null) {
        Op op = key.OrDefault();
        return points.Count < 2 || points.Count != values.Count
            || !TensorPrimitives.IsFiniteAll<double>(points.AsSpan()) || !TensorPrimitives.IsFiniteAll<double>(values.AsSpan())
            ? Fin.Fail<Interpolant>(error: op.InvalidInput())
            : Kernel(points, values, slopes, op).Bind(fitted => Interpolant.Of(fitted: fitted, key: op));
    }

    private static FitKernel Sampled(Func<IEnumerable<double>, IEnumerable<double>, IInterpolation> factory) =>
        (points, values, slopes, key) => slopes.IsSome
            ? Fin.Fail<IInterpolation>(key.InvalidInput())
            : key.Catch(() => Fin.Succ(factory(arg1: points.AsIterable(), arg2: values.AsIterable())));
    private static FitKernel Sloped(Func<IEnumerable<double>, IEnumerable<double>, IEnumerable<double>, IInterpolation> factory) =>
        (points, values, slopes, key) => slopes.Match(
            Some: prescribed => prescribed.Count != points.Count
                ? Fin.Fail<IInterpolation>(key.InvalidInput())
                : key.Catch(() => Fin.Succ(factory(arg1: points.AsIterable(), arg2: values.AsIterable(), arg3: prescribed.AsIterable()))),
            None: () => Fin.Fail<IInterpolation>(key.InvalidInput()));
}

// --- [MODELS] -----------------------------------------------------------------------------
// The fitted-curve carrier. The package advertises differentiation and integration as runtime booleans and THROWS on an
// unsupported call, so capability is lifted into the case the mint lands in and an unsupported member is unspellable.
// The two flags genuinely diverge across the roster — the splines and the step scheme answer both, the polynomial and
// log-linear rows answer differentiation alone, the two rational rows answer neither — so the pairing is READ off the
// fitted instance and never kept as a per-row column that drifts. Every read is gated finite: the step scheme returns
// NaN at a sample point and the rational schemes return NaN below ULP, and an ungated read poisons a gradient silently.
[Union]
public abstract partial record Interpolant {
    private Interpolant() { }
    public sealed record Sampled(IInterpolation Curve) : Interpolant;
    public sealed record Sloped(IInterpolation Curve) : Interpolant {
        public Fin<double> Slope(double t, Op? key = null) => Finite(value: Curve.Differentiate(t: t), key: key.OrDefault());
        public Fin<double> Curvature(double t, Op? key = null) => Finite(value: Curve.Differentiate2(t: t), key: key.OrDefault());
    }
    public sealed record Analytic(IInterpolation Curve) : Interpolant {
        public Fin<double> Slope(double t, Op? key = null) => Finite(value: Curve.Differentiate(t: t), key: key.OrDefault());
        public Fin<double> Curvature(double t, Op? key = null) => Finite(value: Curve.Differentiate2(t: t), key: key.OrDefault());
        public Fin<double> Integral(double t, Op? key = null) => Finite(value: Curve.Integrate(t: t), key: key.OrDefault());
        public Fin<double> Integral(double a, double b, Op? key = null) => Finite(value: Curve.Integrate(a: a, b: b), key: key.OrDefault());
    }

    public Fin<double> Value(double t, Op? key = null) => Finite(value: Fitted().Interpolate(t: t), key: key.OrDefault());
    // The two schemes the Interpolate factory roster omits, reached at their OWN owners and therefore not rows: the
    // quadratic spline admits per-segment coefficients through its CONSTRUCTOR — N+1 ascending knots against N
    // coefficient triples, never a sample fit — and the transformed fit composes a domain transform with its inverse
    // around the fit instead of re-fitting a log or reciprocal domain by hand.
    public static Fin<Interpolant> OfSegments(Arr<double> knots, Arr<double> constant, Arr<double> linear, Arr<double> quadratic, Op? key = null) {
        Op op = key.OrDefault();
        return knots.Count < 2 || constant.Count != knots.Count - 1 || linear.Count != constant.Count || quadratic.Count != constant.Count
            ? Fin.Fail<Interpolant>(error: op.InvalidInput())
            : op.Catch(() => Of(fitted: new QuadraticSpline(
                x: [.. knots.AsIterable()], c0: [.. constant.AsIterable()], c1: [.. linear.AsIterable()], c2: [.. quadratic.AsIterable()]), key: op));
    }
    public static Fin<Interpolant> OfTransformed(Func<double, double> transform, Func<double, double> inverse, Arr<double> points, Arr<double> values, Op? key = null) {
        Op op = key.OrDefault();
        return points.Count < 2 || points.Count != values.Count
            ? Fin.Fail<Interpolant>(error: op.InvalidInput())
            : op.Catch(() => Of(fitted: TransformedInterpolation.Interpolate(
                transform: transform, transformInverse: inverse, x: points.AsIterable(), y: values.AsIterable()), key: op));
    }
    internal static Fin<Interpolant> Of(IInterpolation fitted, Op key) =>
        (fitted.SupportsDifferentiation, fitted.SupportsIntegration) switch {
            (true, true) => Fin.Succ<Interpolant>(new Analytic(Curve: fitted)),
            (true, false) => Fin.Succ<Interpolant>(new Sloped(Curve: fitted)),
            (false, false) => Fin.Succ<Interpolant>(new Sampled(Curve: fitted)),
            // Integration without differentiation inhabits no shipped scheme; minting a case whose slope member the
            // instance would throw on is the fabrication this arm refuses.
            (false, true) => Fin.Fail<Interpolant>(key.InvalidResult()),
        };
    internal IInterpolation Fitted() => Switch(sampled: static s => s.Curve, sloped: static s => s.Curve, analytic: static a => a.Curve);
    private static Fin<double> Finite(double value, Op key) =>
        double.IsFinite(value) ? key.AcceptValue(value: value) : Fin.Fail<double>(key.InvalidResult());
}

// TapWindow states the staged-window geometry a tap fold addresses: a logical axis of Extent records, Stride
// scalars each, staged from record Origin, folding records [From, From+Run). Interleaved lanes RIDE the
// stride — the fold moves every scalar of a record together, which is exactly the layout a lane-interleaved
// plane row or column presents and the reason no per-lane entrypoint exists. Whole is the unwindowed common
// case: the full axis staged, the full axis folded. Coverage against the series' own radius is the kernel's
// admission, because the geometry cannot know a reach it does not carry.
public readonly record struct TapWindow(int Extent, int Origin, int From, int Run, int Stride) {
    public static TapWindow Whole(int extent, int stride) => new(Extent: extent, Origin: 0, From: 0, Run: extent, Stride: stride);
    public bool IsValid => Extent >= 1 && Stride >= 1 && Run >= 1 && From >= 0 && Origin >= 0 && Origin <= From && From + Run <= Extent;
}

// TapSeries carries the admitted sample-domain convolution kernel: an odd tap count so the centre tap is
// structural, every weight finite, and a total sum bounded away from zero because the fold normalizes by its
// resolved portion — a zero-sum series is a DIFFERENCE stencil and `Numerics/calculus#NABLA` owns those,
// so the refusal is the seam between the two owners rather than a silent divide-by-nothing. Admission is the
// ONE gate; every fold below runs total on its taps. Convolve is the one entry over both input shapes: the
// instance form folds one strided axis, the static form is the separable rank-2/3 fold — one series per
// lattice axis, walking the lattice's own linearization strides exactly as the spectral row-column fold
// does — and arity lives in the request shape, never in a name.
public readonly record struct TapSeries {
    private TapSeries(Arr<double> taps) => Taps = taps;

    public Arr<double> Taps { get; }
    public int Radius => Taps.Count / 2;
    // Default-ghost read for the fold seams: an array- or default-minted TapSeries carries an empty tap
    // roster no admission saw, and the fold entrypoints refuse it on this one member — Of already proved
    // the rest (odd count, finite weights, non-cancelling sum) once, so no seam re-validates.
    public bool IsValid => Taps.Count >= 1;

    public static Fin<TapSeries> Of(Arr<double> taps, Op? key = null) =>
        taps.Count >= 1 && int.IsOddInteger(taps.Count) && TensorPrimitives.IsFiniteAll<double>(taps.AsSpan())
            && Math.Abs(value: TensorPrimitives.Sum<double>(taps.AsSpan())) > EpsilonPolicy.ZeroTolerance
            ? Fin.Succ(new TapSeries(taps: taps))
            : Fin.Fail<TapSeries>(error: key.OrDefault().InvalidInput());

    public Fin<Unit> Convolve(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op? key = null) =>
        MatrixKernel.TapFold(series: this, source: source, folded: folded, window: window, border: border, key: key.OrDefault());
    public static Fin<Unit> Convolve(Span<double> values, CellLattice lattice, Arr<TapSeries> axes, TapBorder border, Op? key = null) =>
        MatrixKernel.TapFoldLattice(values: values, lattice: lattice, axes: axes, border: border, key: key.OrDefault());
}

// Every transform OVERWRITES the caller's buffer, so the arena is the unit of work and an immutable spectrum value is
// unrepresentable. Each case carries exactly the extent its arm consumes: the interleaved case rides the CellLattice
// (Layers == 1 IS the plane, so no sibling 2D arena exists) and the three one-dimensional cases carry their own sampling
// rate, so the frequency axis is total over the family. The interleaved buffer is int-indexed while a lattice census is
// long, so a census above the array bound refuses at the length gate rather than truncating.
[Union]
public abstract partial record SpectralArena {
    private SpectralArena() { }
    public sealed record Interleaved(Complex[] Values, CellLattice Lattice) : SpectralArena;
    public sealed record Split(double[] Real, double[] Imaginary, PositiveMagnitude Rate) : SpectralArena;
    public sealed record HalfSpectrum(double[] Values, Dimension Samples, PositiveMagnitude Rate) : SpectralArena;
    public sealed record RealValued(Arr<double> Samples, PositiveMagnitude Rate) : SpectralArena;

    // ONE transform entry: the arena case is the discriminant, so no per-carrier entrypoint family and no mode flag.
    public Fin<SpectralReceipt> Transform(SpectralSense sense, SpectralScaling scaling, Op? key = null) =>
        MatrixKernel.SpectralTransform(arena: this, sense: sense, scaling: scaling, key: key.OrDefault());
    public bool IsValid => Switch(
        interleaved: static a => a.Values.Length == a.Lattice.CellCount && Admit.FiniteComplexSpan(a.Values.AsSpan()),
        split: static s => s.Real.Length >= 1 && s.Real.Length == s.Imaginary.Length
            && TensorPrimitives.IsFiniteAll<double>(s.Real) && TensorPrimitives.IsFiniteAll<double>(s.Imaginary),
        halfSpectrum: static h => h.Values.Length >= PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
        realValued: static r => r.Samples.Count >= 1 && TensorPrimitives.IsFiniteAll<double>(r.Samples.AsSpan()));
    public int Rank => Switch(interleaved: static a => a.Lattice.Rank, split: static _ => 1, halfSpectrum: static _ => 1, realValued: static _ => 1);
    public long Cells => Switch(
        interleaved: static a => a.Lattice.CellCount,
        split: static s => (long)s.Real.Length,
        halfSpectrum: static h => (long)h.Samples.Value,
        realValued: static r => (long)r.Samples.Count);
    // Packed conjugate-even extent: N+2 doubles for even N, N+1 for odd, holding interleaved (real, imaginary) bin pairs
    // — bin zero's imaginary slot included — which is exactly the layout the inverse packed transform unpacks.
    internal static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
}

// The transform's evidence. Rank, cell count, and the round-trip factor derive from the arena and the convention rather
// than riding as fields a producer could fill with something else; Energy is the one MEASURED value — the summed bin
// power, which under symmetric scaling is the Parseval-invariant a round trip must preserve.
public readonly record struct SpectralReceipt(SpectralArena Arena, SpectralSense Sense, SpectralScaling Scaling, double Energy) : IValidityEvidence {
    public int Rank => Arena.Rank;
    public long Cells => Arena.Cells;
    public double RoundTripFactor => Scaling.RoundTrip(cells: Cells);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Arena is not null && Sense is not null && Scaling is not null && Arena.IsValid),
        ValidityClaim.Of(double.IsFinite(Energy)),
        ValidityClaim.Nonnegative(value: Energy),
        ValidityClaim.Of(Cells >= 1L && Rank >= 1));
    // The per-bin power the Energy field sums, folded per arena layout.
    public Fin<Arr<double>> Power(Op? key = null) => MatrixKernel.SpectralPower(arena: Arena, key: key.OrDefault());
    // The spectrum's OWN axis: bin count from the census, spacing from the lattice affine's per-axis column norm or the
    // one-dimensional case's carried rate, so no consumer derives a frequency axis beside the grid that produced it.
    public Fin<Arr<double>> Axis(SignedAxis axis, Op? key = null) => MatrixKernel.SpectralAxis(arena: Arena, axis: axis, key: key.OrDefault());
    // The pointwise spectral product re-mints the receipt because it moves the energy the previous one measured.
    public Fin<SpectralReceipt> Modulate(ReadOnlySpan<Complex> symbol, Op? key = null) =>
        MatrixKernel.SpectralModulate(receipt: this, symbol: symbol, key: key.OrDefault());
}
```

## [07]-[SOLVE_KERNEL]

- Owner: `MatrixKernel` the `internal static` numeric kernel and the one MathNet and CSparse factorization, solve, and transform path in the corpus, organized by operation family.
- Entry: every public-facing operation enters through the owning model member; the kernel is reached only through them.
- Auto: `SingularGaugeSolve` derives every threshold from `OperatorFrobeniusScale` and the rhs scale — relative gates, never absolute literals — and witnesses the true residual against the original un-shifted operator; `LobpcgCore` seeds its basis deterministically off the `Domain/identity` splitmix64 owner for bit-stable replay and terminates typed, never a hidden dense fallback; CSR compression is the storage owner's — `OfIndexedEnumerable` sorts each row, `NormalizeDuplicates` sums the coincident run the admission APPENDS, and `PopulateExplicitZerosOnDiagonal` restores the structural diagonal the zero-drop erases before any preconditioner walks it.
- Packages: MathNet.Numerics (the managed provider path only — `UseManaged`, no `Control.UseNative*` call, no provider package — its CSR storage repairs, the 1D transform pairs, and the taper and interpolation rosters), CSparse (`SparseCholesky`, `SparseLU`, `SparseQR`, AMD ordering, the transposed solves and the rank-1 factor moves), Rasm.Domain (`Op`, `Context`, `Deterministic` splitmix64), System.Numerics.Tensors, TYoshimura.DoubleDouble (`ddouble`, the 106-bit residual-witness lane), BCL (`System.Numerics.Complex`, `IProgress<double>`).
- Growth: a new route, gauge case, eigen substrate, or arena layout adds one kernel arm over its vocabulary row and the existing receipt shape.
- Boundary: statement loops inside `SolvePin`, `SolveKkt`, the MGS pass, the separable axis gather-scatter, and the tap-fold record walk are the named statement-kernel exemption — measured assembly, elimination, and strided-line hot paths; `BiCgStabDivergenceFactor` (divergence ceiling) and `KktPivotTolerance` (CSparse full-partial-pivot column threshold) are the named kernel policy constants.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MatrixKernel {
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
    // The ONE plain CSR-to-CSC bridge every sensed product and every direct sparse route enters through; the symmetric
    // twin below is its mirror-normalizing sibling for the routes that demand a canonical upper view.
    internal static CSparse.Storage.CompressedColumnStorage<double> ToCSparse(SparseMatrix s) =>
        CSparse.Double.SparseMatrix.OfIndexed(rows: s.Rows.Value, columns: s.Cols.Value, enumerable: SparseTripletsOf(matrix: s));
    internal static Fin<CSparse.Storage.CompressedColumnStorage<double>> ToCSparseSymmetric(SparseMatrix s, Op key) =>
        NormalizeSymmetricUpperEntries(s: s, key: key).Map(upper =>
            CSparse.Double.SparseMatrix.OfIndexed(rows: s.Rows.Value, columns: s.Rows.Value, enumerable: upper));
    // Rejects contradictory duplicate mirror entries beyond a scale-relative band, yielding the canonical row<=col upper view.
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
    // The Krylov lane's storage carries an EXPLICIT diagonal: MILU0 walks the CSR diagonal position directly and the
    // Jacobi row reads it, while assembly drops a cancelled diagonal entry structurally, so the slot is restored here
    // rather than left absent for an incomplete factor to index past. The repair is square-only by construction — the
    // member writes column index == row index for every diagonal-less row, which on a tall operator would emit columns
    // beyond the column count.
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) =>
        DenseMatrixD.Build.Sparse(DiagonalPopulated(
            storage: SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
                rows: s.Rows.Value, columns: s.Cols.Value, valueCount: s.Values.Count,
                rowPointers: [.. s.RowPtr.AsIterable()], columnIndices: [.. s.ColInd.AsIterable()], values: [.. s.Values.AsIterable()]),
            square: s.Rows.Value == s.Cols.Value));
    private static SparseCompressedRowMatrixStorage<double> DiagonalPopulated(SparseCompressedRowMatrixStorage<double> storage, bool square) =>
        square ? (fun(storage.PopulateExplicitZerosOnDiagonal)(), storage).Item2 : storage;
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
    // RelativeResidual folds its norm in 106-bit ddouble, so cancellation in b - Ax cannot inflate or deflate the residual witness.
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        CompensatedNorm(v: b - a.Multiply(x)) / Math.Max(val1: 1.0, val2: CompensatedNorm(v: b));
    private static double CompensatedNorm(LinearVector v) {
        ddouble sum = 0.0;
        for (int i = 0; i < v.Count; i++) sum += (ddouble)v[i] * v[i];
        return Math.Sqrt(d: (double)sum);
    }
    internal static bool SolveInputIsValid(int rows, Arr<double> rhs) =>
        rhs.Count == rows && TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan());
    internal static Fin<double> SparseSymmetricResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs, Op key) =>
        NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => key.AcceptValue(value: RelativeResidual(
            a: ToMathNetSymmetric(matrix: matrix, upper: upper),
            x: DenseVectorD.OfArray([.. solution.AsIterable()]),
            b: DenseVectorD.OfArray([.. rhs.AsIterable()]))));
    // The sensed residual witness: the result buffer starts as b and the accumulate column folds it to Op·x − b in ONE
    // pass over the operator — the subtract loop deletes — while the difference norm still folds in 106-bit ddouble, so
    // the fused pass buys the loop's removal without surrendering the compensation the witness law demands.
    private static double SensedResidual(CSparse.Storage.CompressedColumnStorage<double> operand, OperatorSense sense, double[] solution, Arr<double> rhs) {
        double[] residual = [.. rhs.AsIterable()];
        sense.Accumulate(operand: operand, alpha: 1.0, x: solution, beta: -1.0, y: residual);
        return CompensatedNorm(v: DenseVectorD.OfArray(residual)) / Math.Max(val1: 1.0, val2: CompensatedNorm(v: DenseVectorD.OfArray([.. rhs.AsIterable()])));
    }
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
    internal static Fin<SolveReceipt> SolveSuccess(Arr<double> solution, int solutionLength, SolvePath path, SolveStop stop, Dimension rows, Dimension cols, int rhsLength, double residual, Op key, OperatorSense? sense = null, double residualCap = double.PositiveInfinity, Option<int> iterations = default, Option<int> maxIterations = default, Option<double> tolerance = default, Option<bool> fullRank = default, Option<int> inputNonZeros = default, Option<int> factorNonZeros = default, Option<GaugeReceipt> gauge = default) {
        SolveReceipt receipt = new(Solution: solution, Path: path, Stop: stop, Sense: sense ?? OperatorSense.Forward, Rows: rows, Cols: cols, RhsLength: rhsLength, Iterations: iterations, MaxIterations: maxIterations, Tolerance: tolerance, Residual: residual, FullRank: fullRank, InputNonZeros: inputNonZeros, FactorNonZeros: factorNonZeros, Gauge: gauge);
        return solution.Count == solutionLength && TensorPrimitives.IsFiniteAll<double>(solution.AsSpan()) && double.IsFinite(residual) && residual <= residualCap && (!stop.IsUsable || receipt.IsValid)
            ? Fin.Succ(receipt)
            : Fin.Fail<SolveReceipt>(key.InvalidResult());
    }
    private static Fin<EigenSolveReceipt<TEigen, TVector>> EigenReceiptOf<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, EigenSolveStop stop, EigenOrder order, int requestedPairs, double maxResidual, EigenPathEvidence evidence, Op key) {
        EigenSolveReceipt<TEigen, TVector> receipt = new(Pairs: pairs, Path: path, Stop: stop, Order: order, RequestedPairs: requestedPairs, ReturnedPairs: pairs.Count, Evidence: evidence, MaxResidual: maxResidual);
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
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseSymmetricEvd, stop: EigenSolveStop.DirectSolved, order: EigenOrder.DescendingMagnitude, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector), evidence: new EigenPathEvidence.Direct(), key: key);
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
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseGeneralEvd, stop: EigenSolveStop.DirectSolved, order: EigenOrder.Factorization, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue), evidence: new EigenPathEvidence.Direct(), key: key);
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
    // CSR compression belongs to the storage owner, never to a hand-written compressor beside it: OfIndexedEnumerable
    // sorts each row and APPENDS coincident entries (and drops exact-zero triplets at admission), NormalizeDuplicates
    // sums the appended run — the one member that adds coincident entries — and the residue pass zeroes then compacts
    // through the storage's own map-and-normalize pair, so the whole fold is three package calls over one buffer set.
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op op) {
        List<(int Row, int Col, double Value)> raw = [.. triplets];
        if (raw.Exists(t => !double.IsFinite(t.Value) || t.Row < 0 || t.Row >= rows.Value || t.Col < 0 || t.Col >= cols.Value)) return Fin.Fail<SparseMatrix>(op.InvalidInput());
        SparseCompressedRowMatrixStorage<double> storage = SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(
            rows: rows.Value, columns: cols.Value, data: raw.Select(static t => (t.Row, t.Col, t.Value)));
        storage.NormalizeDuplicates();
        // Assembly residue drops at a STRUCTURAL band scaled to the operator, never at binary zero: a summed
        // triplet cancelling to 1e-300 is fill the factorization pays for and the pattern fingerprint keys on.
        double residue = EpsilonPolicy.SqrtEpsilon * TensorPrimitives.Norm<double>(storage.Values.AsSpan(start: 0, length: storage.ValueCount));
        storage.MapInplace(f: value => Math.Abs(value: value) > residue ? value : 0.0, zeros: Zeros.AllowSkip);
        storage.NormalizeZeros();
        return CompressedOf(storage: storage, rows: rows, cols: cols, op: op);
    }
    // The storage's own three buffers ARE the CSR: RowPointers runs rows+1, and the value and index arrays are exactly
    // ValueCount long after NormalizeDuplicates resizes them, so the read is a slice rather than a re-walk.
    private static Fin<SparseMatrix> CompressedOf(SparseCompressedRowMatrixStorage<double> storage, Dimension rows, Dimension cols, Op op) {
        SparseMatrix result = new(Rows: rows, Cols: cols,
            RowPtr: new Arr<int>(storage.RowPointers), ColInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), Values: new Arr<double>(storage.Values[..storage.ValueCount]));
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
        // The complex leg pre-sums BEFORE admission and keeps doing so: the diagonal-realness gate must read SUMMED
        // entries, and the storage admission's per-row `Tuple<int, T>` sort falls through to the VALUE comparison on a
        // duplicate column, which `Complex` — carrying no `IComparable` — cannot answer. Pre-summed keys are unique, so
        // that comparison is never reached and the storage owns the compression on this leg too.
        SparseCompressedRowMatrixStorage<Complex> storage = SparseCompressedRowMatrixStorage<Complex>.OfIndexedEnumerable(
            rows: order.Value, columns: order.Value,
            data: upper.Select(static t => (t.Row, t.Col, t.Row == t.Col ? new Complex(t.Value.Real, 0.0) : t.Value)));
        SparseHermitian result = new(Order: order, RowPtr: new Arr<int>(storage.RowPointers),
            ColInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), Values: new Arr<Complex>(storage.Values[..storage.ValueCount]));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseHermitian>(op.InvalidResult());
    }
    // 2n x 2n real-block embedding of a Hermitian pair — the connection-Laplacian assembly (Meshing/dec) composes these rows.
    internal static void AddHermitianRealBlockTriplets(List<(int Row, int Col, double Value)> triplets, int order, int i, int j, double real, double imaginary, double diagonal) =>
        triplets.AddRange([
            (i, i, diagonal), (j, j, diagonal), (i + order, i + order, diagonal), (j + order, j + order, diagonal),
            (i, j, real), (j, i, real), (i + order, j + order, real), (j + order, i + order, real),
            (i, j + order, -imaginary), (j + order, i, -imaginary), (i + order, j, imaginary), (j, i + order, imaginary),
        ]);
    // Rank-1 source movement: the factor moves by ±w·w', so the held source moves by the same outer product or the
    // residual witness would measure against the operator the factor abandoned. The column is sparse, so the product is
    // its nonzero pairs and the re-assembly folds them through the same accumulating admission every consumer uses.
    internal static Fin<SparseMatrix> RankOneMoved(SparseMatrix source, SparseMatrix column, double scale, Op key) {
        List<(int Row, int Col, double Value)> stored = SparseTripletsOf(matrix: column);
        List<(int Row, int Col, double Value)> entries = SparseTripletsOf(matrix: source, capacityBonus: stored.Count * stored.Count);
        entries.AddRange(collection: from left in stored
                                     from right in stored
                                     select (Row: left.Row, Col: right.Row, Value: scale * left.Value * right.Value));
        return AssembleSparse(rows: source.Rows, cols: source.Cols, triplets: entries, op: key);
    }

    // --- [SPARSE_SOLVES] --------------------------------------------------------------------------
    // The sensed product: the sense row's own column runs the CSC operator forward or transposed, so a transposed
    // product is the same entry rather than a second member, and both lengths come off the sensed shape.
    internal static Fin<Arr<double>> SparseProduct(SparseMatrix self, Arr<double> x, OperatorSense sense, Op key) {
        if (sense is null || !self.IsValid) return Fin.Fail<Arr<double>>(key.InvalidInput());
        (Dimension rows, Dimension cols) = sense.Shape(rows: self.Rows, cols: self.Cols);
        return x.Count != cols.Value || !TensorPrimitives.IsFiniteAll<double>(x.AsSpan())
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => {
                double[] y = new double[rows.Value];
                sense.Apply(operand: ToCSparse(s: self), x: [.. x.AsIterable()], y: y);
                Arr<double> result = new(y);
                return TensorPrimitives.IsFiniteAll<double>(result.AsSpan()) ? Fin.Succ(result) : Fin.Fail<Arr<double>>(key.InvalidResult());
            });
    }
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
    // Diagonal-preconditioned BiCgStab; the MathNet direct solve is the RECORDED fallback (SparseMathNetDirectFallback), gated at the looser cap.
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
    // Preconditioned Krylov with NO direct fallback — the large-grid arm where SparseSolve's MathNet direct
    // rescue (A.Solve densifies a sparse operator) IS the failure mode: a caller above its direct ceiling
    // states the preconditioner ROW, the tolerance, the cap, and — where its own condition governs — one
    // stop row, and a non-converged run reports
    // IterativeExhausted with the true residual on the receipt rather than silently densifying millions of
    // unknowns. ToMathNetSparse converts the kernel's OWN duplicate-summed CSR, so the MathNet OfIndexed
    // append-duplicates trap never enters this rail.
    internal static Fin<SolveReceipt> SparseSolveIterative(SparseMatrix matrix, Arr<double> rhs, SparsePreconditioner preconditioner, double tolerance, int maxIterations, Option<KrylovStop> stop, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs) || !double.IsFinite(tolerance) || tolerance <= 0.0 || maxIterations < 1
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> A = ToMathNetSparse(s: matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                // Criterion ORDER is precedence and the status read short-circuits on the first non-continuing verdict:
                // failure stays terminal ahead of everything, divergence before the residual test, the residual test
                // ahead of the domain rule so a converged solve is never reported as domain-halted, and the count cap
                // last so it only fires on a run nothing else settled.
                MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double> iterator = new([
                    new MathNet.Numerics.LinearAlgebra.Solvers.FailureStopCriterion<double>(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.DivergenceStopCriterion<double>(maximumRelativeIncrease: BiCgStabDivergenceFactor, minimumIterations: 8),
                    new MathNet.Numerics.LinearAlgebra.Solvers.ResidualStopCriterion<double>(maximum: tolerance, minimumIterationsBelowMaximum: 2),
                    .. stop.Map(static rule => (MathNet.Numerics.LinearAlgebra.Solvers.IIterationStopCriterion<double>)
                        new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, residual) =>
                            rule.Halt(arg1: iteration, arg2: residual.L2Norm())
                                ? MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.StoppedWithoutConvergence
                                : MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Continue)).ToSeq(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.IterationCountStopCriterion<double>(maximumNumberOfIterations: maxIterations),
                ]);
                LinearVector x = A.SolveIterative(input: b, solver: new MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab(), iterator: iterator, preconditioner: preconditioner.Create());
                double residual = RelativeResidual(a: A, x: x, b: b);
                bool converged = iterator.Status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged && double.IsFinite(residual) && residual <= tolerance;
                return SolveSuccess(solution: ArrFromVector(x), solutionLength: matrix.Cols.Value, path: preconditioner.KrylovPath(), stop: converged ? SolveStop.ResidualConverged : SolveStop.IterativeExhausted, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, residualCap: tolerance, maxIterations: Some(maxIterations), tolerance: Some(tolerance), inputNonZeros: Some(matrix.NonZeros));
            });

    // Symmetric-indefinite (or nonsymmetric) sparse direct solve: CSparse SparseLU, A+At ordering,
    // column-relative pivot tol in [0,1]; SPD pivot loss throws bare Exception, caught into the typed rail. The sense
    // row picks the standing factor's forward or transposed triangular sweep, so A'x = b runs on the SAME numeric
    // factorization — no explicit transpose is assembled and no second symbolic analysis is paid for.
    internal static Fin<SolveReceipt> SparseLuSolve(SparseMatrix matrix, Arr<double> rhs, OperatorSense sense, double pivotTolerance, IProgress<double>? progress, Op key) =>
        sense is null || !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs) || !double.IsFinite(pivotTolerance) || pivotTolerance is < 0.0 or > 1.0
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                int n = matrix.Rows.Value;
                CSparse.Storage.CompressedColumnStorage<double> csc = ToCSparse(s: matrix);
                CSparse.Double.Factorization.SparseLU lu = Optional(progress).Match(
                    Some: report => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance, progress: report),
                    None: () => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance));
                double[] solution = new double[n];
                sense.SolveLu(factor: lu, rhs: [.. rhs.AsIterable()], solution: solution);
                Arr<double> x = new(solution);
                double residual = SensedResidual(operand: csc, sense: sense, solution: solution, rhs: rhs);
                double cap = Math.Sqrt(EpsilonPolicy.SqrtEpsilon);
                return SolveSuccess(solution: x, solutionLength: n, path: SolvePath.SparseLuIndefinite, stop: double.IsFinite(residual) && residual <= cap ? SolveStop.DirectSolved : SolveStop.FallbackRejected, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, sense: sense, residualCap: cap, tolerance: Some(cap), inputNonZeros: Some(matrix.NonZeros), factorNonZeros: Some(lu.NonZerosCount));
            });

    // Rectangular sparse least-squares: CSparse SparseQR under the A'A minimum-degree mint. The residual
    // witnessed is the NORMAL-equation residual ‖Opᵀ(Op·x − b)‖ / (‖A‖_F·‖b‖) — the least-squares optimality
    // signal a raw ‖Op·x − b‖ cannot carry — and the work buffer sizes from max(m, n), the solution dimension
    // a structurally singular system exceeds its row count by, which is also the sizing both senses share. The residual
    // leg is two accumulate passes: the sense folds Op·x − b in place, its FLIPPED row applies the opposite direction
    // for the normal projection, so neither direction spells its own subtraction loop or its own transposed member.
    internal static Fin<SolveReceipt> SparseQrSolve(SparseMatrix matrix, Arr<double> rhs, OperatorSense sense, IProgress<double>? progress, Op key) {
        if (sense is null || !matrix.IsValid) return Fin.Fail<SolveReceipt>(key.InvalidInput());
        (Dimension sensedRows, Dimension sensedCols) = sense.Shape(rows: matrix.Rows, cols: matrix.Cols);
        return rhs.Count != sensedRows.Value || !TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan())
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                CSparse.Storage.CompressedColumnStorage<double> csc = ToCSparse(s: matrix);
                CSparse.Double.Factorization.SparseQR qr = Optional(progress).Match(
                    Some: report => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA, progress: report),
                    None: () => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA));
                double[] work = new double[Math.Max(val1: matrix.Rows.Value, val2: matrix.Cols.Value)];
                sense.SolveQr(factor: qr, rhs: [.. rhs.AsIterable()], solution: work);
                double[] solution = work[..sensedCols.Value];
                Arr<double> x = new(solution);
                double[] residualVector = [.. rhs.AsIterable()];
                sense.Accumulate(operand: csc, alpha: 1.0, x: solution, beta: -1.0, y: residualVector);
                double[] normal = new double[sensedCols.Value];
                sense.Flipped().Apply(operand: csc, x: residualVector, y: normal);
                double operatorScale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: TensorPrimitives.Norm<double>(matrix.Values.AsSpan()));
                double residual = TensorPrimitives.Norm<double>(normal) / (operatorScale * Math.Max(val1: TensorPrimitives.Norm<double>(rhs.AsSpan()), val2: EpsilonPolicy.SqrtEpsilon));
                double cap = Math.Sqrt(EpsilonPolicy.SqrtEpsilon);
                return SolveSuccess(solution: x, solutionLength: sensedCols.Value, path: SolvePath.SparseQrLeastSquares, stop: double.IsFinite(residual) && residual <= cap ? SolveStop.LeastSquaresSolved : SolveStop.RankDeficient, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, sense: sense, residualCap: cap, inputNonZeros: Some(matrix.NonZeros), factorNonZeros: Some(qr.NonZerosCount));
            });
    }

    // --- [SINGULAR_GAUGE] --------------------------------------------------------------------------
    // Gauge dual-solve over a singular SPSD operator: derive every threshold from the operator and rhs
    // scales, witness the TRUE residual against the original un-shifted operator, leave a typed GaugeReceipt.
    // Pin/KKT triplet and projection loops are the named statement-kernel exemption.
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
    // Shared M-orthogonal Gram solve: factor the SPD Gram Nt M N, applying a diagonal-scaled Tikhonov shift only on Cholesky breakdown,
    // surfacing the shift and the numeric nullspace dimension (factor diagonal entries above a scale-relative floor).
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
                  Matrix<double> stiffnessM = Densified(operand: ToMathNetSymmetric(matrix: stiffness, upper: stiffnessUpper));
                  Matrix<double> massM = Densified(operand: ToMathNetSymmetric(matrix: mass, upper: massUpper));
                  (LinearVector vals, Matrix<double> vecs, int factorNonZeros) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                  Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                      .OrderBy(i => vals[i]).Take(k)
                      .Select(i => (Eigenvalue: vals[i], Eigenvector: ArrFromVector(vecs.Column(i)))));
                  return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseGeneralizedCholeskyCongruence, stop: EigenSolveStop.DirectSolved, order: EigenOrder.Ascending, requestedPairs: k, maxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs), evidence: new EigenPathEvidence.Factored(FactorNonZeros: factorNonZeros), key: key);
              })
              select receipt;
    // The densification is SPELLED at the boundary rather than left to happen inside Cholesky(): a MathNet sparse
    // operator declares no Cholesky override, so the factorization would run through the dense user path with no
    // fill-reducing ordering and the route would read sparse while paying dense cost.
    private static Matrix<double> Densified(Matrix<double> operand) =>
        DenseMatrixD.Build.DenseOfRowMajor(operand.RowCount, operand.ColumnCount, operand.ToRowMajorArray());
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
            return EigenReceiptOf(pairs: pairs, path: path, stop: stop, order: EigenOrder.Ascending, requestedPairs: k, maxResidual: residual(arg1: A, arg2: pairs), evidence: new EigenPathEvidence.Iterative(Iterations: iter, MaxIterations: maxIterations, Tolerance: tolerance), key: key);
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
    // The degenerate denominator FLOORS exactly as the real twin's Math.Max does — a fabricated Complex.Zero
    // quotient sorts as the SMALLEST eigenvalue through Pairs' ascending order and is reported as a converged
    // eigenpair, so the two twins share one degeneracy law rather than one flooring and one manufacturing.
    private static ComplexVector RayleighQuotientsComplex(Matrix<Complex> X, Matrix<Complex> AX) =>
        DenseVectorC.Create(X.ColumnCount, j => X.Column(j).ConjugateDotProduct(AX.Column(j))
            / (X.Column(j).ConjugateDotProduct(X.Column(j)) switch {
                Complex den when Complex.Abs(den) > EpsilonPolicy.ZeroTolerance => den,
                _ => new Complex(EpsilonPolicy.ZeroTolerance, 0.0),
            }));
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

    // --- [SPECTRAL] ------------------------------------------------------------------------------
    // Rank 2 and rank 3 ARE the row-column fold over the managed-complete 1D pair (Radix-2 at a power of two, Bluestein
    // otherwise): Forward2D and ForwardMultiDim route to the multidim provider seam whose managed realization throws
    // NotSupportedException, and the admitted native adapters ship no arm64 asset, so the page's managed-provider pin
    // makes those four entrypoints unreachable by construction and they never spell here. Symmetric scaling composes per
    // axis, so the folded transform carries exactly the convention the 1D row declares.
    internal static Fin<SpectralReceipt> SpectralTransform(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
        arena is null || sense is null || scaling is null || !arena.IsValid
            ? Fin.Fail<SpectralReceipt>(key.InvalidInput())
            : key.Catch(() => SpectralReceiptOf(arena: Transformed(arena: arena, sense: sense, scaling: scaling), sense: sense, scaling: scaling, key: key));
    private static SpectralArena Transformed(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
        arena.Switch(
            state: (Sense: sense, Scaling: scaling),
            interleaved: static (s, a) => FoldSeparable(arena: a, sense: s.Sense, options: s.Scaling.FourierConvention),
            split: static (s, a) => {
                s.Sense.Split(real: a.Real, imaginary: a.Imaginary, options: s.Scaling.FourierConvention);
                return (SpectralArena)a;
            },
            halfSpectrum: static (s, a) => {
                s.Sense.Packed(arena: a.Values, samples: a.Samples.Value, options: s.Scaling.FourierConvention);
                return (SpectralArena)a;
            },
            // The ONE arm that allocates: the Hartley pair returns a fresh coefficient vector rather than overwriting
            // its input, so this case hands back a new arena while the three Fourier cases hand back the caller's own.
            realValued: static (s, a) => new SpectralArena.RealValued(
                Samples: new Arr<double>(s.Sense.RealValued(samples: [.. a.Samples.AsIterable()], options: s.Scaling.HartleyConvention)), Rate: a.Rate));
    // The separable axis fold — the named statement-kernel exemption on this rail. Per axis, each line gathers at that
    // axis's stride into one contiguous buffer, takes the 1D transform, and scatters back. The strides ARE the lattice's
    // own linearization read as per-axis steps (1, Columns, Columns·Rows), so the fold addresses through the declared
    // linearization instead of re-deriving an index expression the lattice already owns.
    private static SpectralArena FoldSeparable(SpectralArena.Interleaved arena, SpectralSense sense, FourierOptions options) {
        int columns = arena.Lattice.Columns.Value, rows = arena.Lattice.Rows.Value, layers = arena.Lattice.Layers.Value;
        int cells = arena.Values.Length;
        for (int axis = 0; axis < arena.Lattice.Rank; axis++) {
            (int count, int stride) = axis switch {
                0 => (columns, 1),
                1 => (rows, columns),
                _ => (layers, columns * rows),
            };
            Complex[] line = new Complex[count];
            for (int origin = 0; origin < cells; origin++) {
                if (origin / stride % count != 0) continue;
                for (int k = 0; k < count; k++) line[k] = arena.Values[origin + (k * stride)];
                sense.Interleaved(arena: line, options: options);
                for (int k = 0; k < count; k++) arena.Values[origin + (k * stride)] = line[k];
            }
        }
        return arena;
    }
    // One power fold serves both the receipt's measured energy and the consumer's per-bin read, so the spectrum is never
    // walked twice and the receipt's Energy is the summed power the caller can re-derive from Power itself.
    private static Fin<SpectralReceipt> SpectralReceiptOf(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
        SpectralPower(arena: arena, key: key).Bind(power => {
            SpectralReceipt receipt = new(Arena: arena, Sense: sense, Scaling: scaling, Energy: TensorPrimitives.Sum<double>(power.AsSpan()));
            return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<SpectralReceipt>(key.InvalidResult());
        });
    // Per-bin power with no square root anywhere: MagnitudeSquared over the interleaved bins, the vectorized
    // square-then-fused-multiply-add over the split spans (the reason that arena exists), the interleaved pair fold over
    // the packed half-spectrum, and the reflection pairing the real Hartley spectrum's power identity demands.
    internal static Fin<Arr<double>> SpectralPower(SpectralArena arena, Op key) =>
        arena is null || !arena.IsValid
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => Fin.Succ(arena.Switch(
                interleaved: static a => new Arr<double>([.. a.Values.Select(static bin => bin.MagnitudeSquared())]),
                split: static s => SplitPower(real: s.Real, imaginary: s.Imaginary),
                halfSpectrum: static h => PackedPower(values: h.Values, samples: h.Samples.Value),
                realValued: static r => HartleyPower(samples: r.Samples))));
    private static Arr<double> SplitPower(double[] real, double[] imaginary) {
        double[] power = new double[real.Length];
        TensorPrimitives.Multiply<double>(real, real, power);
        TensorPrimitives.MultiplyAdd<double>(imaginary, imaginary, power, power);
        return new Arr<double>(power);
    }
    private static Arr<double> PackedPower(double[] values, int samples) =>
        new Arr<double>([.. Enumerable.Range(start: 0, count: SpectralArena.PackedLength(samples: samples) / 2)
            .Select(bin => (values[2 * bin] * values[2 * bin]) + (values[(2 * bin) + 1] * values[(2 * bin) + 1]))]);
    // A Hartley spectrum is real and its power pairs a bin with its reflection — |F(k)|² = (H(k)² + H(N−k)²)/2, bin zero
    // pairing itself — so reading H(k)² alone would report the Fourier power of a symmetric spectrum only.
    private static Arr<double> HartleyPower(Arr<double> samples) {
        int n = samples.Count;
        return new Arr<double>([.. Enumerable.Range(start: 0, count: n)
            .Select(k => ((samples[index: k] * samples[index: k]) + (samples[index: (n - k) % n] * samples[index: (n - k) % n])) * 0.5)]);
    }
    // The spectrum reads its OWN axis: the bin count and the sample rate come from the arena that produced it — the
    // lattice census with the affine's per-axis column norm inverted, or the one-dimensional case's carried rate — so no
    // consumer derives a frequency axis beside the grid it sampled.
    internal static Fin<Arr<double>> SpectralAxis(SpectralArena arena, SignedAxis axis, Op key) =>
        arena is null || axis is null || !arena.IsValid
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : arena.Switch(
                state: (Ordinal: Math.Abs(value: axis.Key) - 1, Key: key),
                interleaved: static (s, a) => s.Ordinal >= a.Lattice.Rank
                    ? Fin.Fail<Arr<double>>(s.Key.InvalidInput())
                    : AxisOf(metric: LatticeMetric(lattice: a.Lattice, ordinal: s.Ordinal), key: s.Key),
                split: static (s, a) => s.Ordinal != 0
                    ? Fin.Fail<Arr<double>>(s.Key.InvalidInput())
                    : AxisOf(metric: (Count: a.Real.Length, SampleRate: a.Rate.Value), key: s.Key),
                halfSpectrum: static (s, a) => s.Ordinal != 0
                    ? Fin.Fail<Arr<double>>(s.Key.InvalidInput())
                    : AxisOf(metric: (Count: a.Samples.Value, SampleRate: a.Rate.Value), key: s.Key),
                realValued: static (s, a) => s.Ordinal != 0
                    ? Fin.Fail<Arr<double>>(s.Key.InvalidInput())
                    : AxisOf(metric: (Count: a.Samples.Count, SampleRate: a.Rate.Value), key: s.Key));
    private static Fin<Arr<double>> AxisOf((int Count, double SampleRate) metric, Op key) =>
        metric.Count < 1 || !double.IsFinite(metric.SampleRate) || metric.SampleRate <= 0.0
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => {
                Arr<double> bins = new(Fourier.FrequencyScale(length: metric.Count, sampleRate: metric.SampleRate));
                return TensorPrimitives.IsFiniteAll<double>(bins.AsSpan()) ? Fin.Succ(bins) : Fin.Fail<Arr<double>>(key.InvalidResult());
            });
    // The lattice admits only an invertible affine, so no column norm is zero and the reciprocal needs no floor; a
    // degenerate spacing that somehow reached here divides to infinity and the axis gate refuses it as non-finite.
    private static (int Count, double SampleRate) LatticeMetric(CellLattice lattice, int ordinal) =>
        ordinal switch {
            0 => (Count: lattice.Columns.Value, SampleRate: 1.0 / lattice.CellSize.X),
            1 => (Count: lattice.Rows.Value, SampleRate: 1.0 / lattice.CellSize.Y),
            _ => (Count: lattice.Layers.Value, SampleRate: 1.0 / lattice.CellSize.Z),
        };
    // SpectralModulate carries the convolution theorem's whole content — the SPECTRAL half of the one
    // convolution correspondence, whose sample-domain half is the tap fold below. The symbol addresses
    // interleaved bins, so the packed and Hartley layouts refuse rather than have the band re-derive the
    // package's own bin packing; the destination aliases its operand legally, so the product runs over the
    // caller's arena with no staging copy.
    internal static Fin<SpectralReceipt> SpectralModulate(SpectralReceipt receipt, ReadOnlySpan<Complex> symbol, Op key) =>
        receipt.Arena is SpectralArena.Interleaved plane && plane.Values.Length == symbol.Length && Admit.FiniteComplexSpan(symbol)
            ? Modulated(plane: plane, symbol: symbol, receipt: receipt, key: key)
            : Fin.Fail<SpectralReceipt>(key.InvalidInput());
    private static Fin<SpectralReceipt> Modulated(SpectralArena.Interleaved plane, ReadOnlySpan<Complex> symbol, SpectralReceipt receipt, Op key) {
        TensorPrimitives.Multiply<Complex>(plane.Values, symbol, plane.Values);
        return SpectralReceiptOf(arena: plane, sense: receipt.Sense, scaling: receipt.Scaling, key: key);
    }

    // --- [TAP_FOLD] ------------------------------------------------------------------------------
    // TapFold folds the sample-domain half of the convolution correspondence. Admission proves the window's
    // staging covers every in-extent tap the run reaches, and the RESOLVING border rows — Clamp, Wrap, Mirror —
    // demand the whole axis staged, because a wrapped or clamped index resolves anywhere on the axis and a
    // partial window cannot answer it; a partial window therefore rides `TapBorder.Zero` alone, the caller
    // having already resolved its own edge law by address at its fill. Each output divides by the
    // RESOLVED-weight sum — partition of unity at every border, no pre-normalized table, no rim a dropped
    // tap darkens — and a resolved sum the drops cancel refuses typed rather than scaling by zero.
    internal static Fin<Unit> TapFold(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op key) {
        int stride = window.Stride, staged = stride >= 1 ? source.Length / stride : 0;
        bool whole = window.Origin == 0 && staged == window.Extent;
        return !series.IsValid || !window.IsValid || border is null
            || source.Length != staged * stride || folded.Length != window.Run * stride
            || window.Origin > Math.Max(val1: 0, val2: window.From - series.Radius)
            || window.Origin + staged <= Math.Min(val1: window.Extent - 1, val2: window.From + window.Run - 1 + series.Radius)
            || (!whole && border != TapBorder.Zero)
            ? Fin.Fail<Unit>(key.InvalidInput())
            : TapFoldCore(series: series, source: source, folded: folded, window: window, border: border, key: key);
    }

    // TapFoldLattice runs the separable lattice fold: per axis, every line at that axis's stride gathers into
    // one contiguous run, takes the SAME 1D fold, and scatters back — the identical walk FoldSeparable takes
    // for the transform, reading the lattice's own linearization as per-axis steps (1, Columns, Columns·Rows)
    // rather than re-deriving an index expression the lattice already owns. In place over the caller's values,
    // mirroring the transform's arena law.
    internal static Fin<Unit> TapFoldLattice(Span<double> values, CellLattice lattice, Arr<TapSeries> axes, TapBorder border, Op key) {
        // Axes arrive caller-shaped, so the default-ghost gate reads every series' key member before any
        // lattice value moves — an array-minted default(TapSeries) otherwise reaches the core and throws
        // off the rail instead of routing InvalidInput.
        if (lattice is null || border is null || axes.Count != lattice.Rank || values.Length != lattice.CellCount
            || axes.Exists(static series => !series.IsValid)) {
            return Fin.Fail<Unit>(key.InvalidInput());
        }
        int columns = lattice.Columns.Value, rows = lattice.Rows.Value, layers = lattice.Layers.Value;
        int cells = values.Length, longest = Math.Max(val1: columns, val2: Math.Max(val1: rows, val2: layers));
        double[] line = new double[longest], result = new double[longest];
        for (int axis = 0; axis < axes.Count; axis++) {
            (int count, int stride) = axis switch { 0 => (columns, 1), 1 => (rows, columns), _ => (layers, columns * rows) };
            TapWindow window = TapWindow.Whole(extent: count, stride: 1);
            for (int origin = 0; origin < cells; origin++) {
                if (origin / stride % count != 0) { continue; }
                for (int k = 0; k < count; k++) { line[k] = values[origin + (k * stride)]; }
                Fin<Unit> lineFold = TapFoldCore(series: axes[axis], source: line.AsSpan(0, count), folded: result.AsSpan(0, count), window: window, border: border, key: key);
                if (lineFold.IsFail) { return lineFold; }
                for (int k = 0; k < count; k++) { values[origin + (k * stride)] = result[k]; }
            }
        }
        return Fin.Succ(unit);
    }

    // TapFoldCore owns the fold body — a fixed-extent record walk on the named statement-kernel exemption. An
    // in-extent tap reads its staged record directly; an out-of-extent tap routes through the border row's own
    // address column, and a negative resolution drops the tap AND its weight. All Stride lanes of a record move
    // together, so one call folds an interleaved plane row or column whole. A resolved-weight sum the drops
    // cancel below the epsilon floor refuses TYPED naming the record — mixed-sign taps under a Zero border can
    // cancel at a rim record, and a 0.0-scale fall-through would certify a fabricated zero sample as folded.
    private static Fin<Unit> TapFoldCore(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op key) {
        ReadOnlySpan<double> taps = series.Taps.AsSpan();
        int radius = series.Radius, stride = window.Stride;
        for (int at = 0; at < window.Run; at++) {
            int record = window.From + at, seat = at * stride;
            folded.Slice(seat, stride).Clear();
            double admitted = 0.0;
            for (int tap = -radius; tap <= radius; tap++) {
                int logical = record + tap;
                int resolved = logical >= 0 && logical < window.Extent ? logical : border.Resolve(index: logical, extent: window.Extent);
                if (resolved < 0) { continue; }
                double weight = taps[tap + radius];
                admitted += weight;
                int from = (resolved - window.Origin) * stride;
                for (int lane = 0; lane < stride; lane++) { folded[seat + lane] += weight * source[from + lane]; }
            }
            if (Math.Abs(value: admitted) <= EpsilonPolicy.ZeroTolerance) {
                return Fin.Fail<Unit>(key.InvalidResult(detail: $"resolved tap-weight sum cancelled at record {record}"));
            }
            double scale = 1.0 / admitted;
            for (int lane = 0; lane < stride; lane++) { folded[seat + lane] *= scale; }
        }
        return Fin.Succ(unit);
    }
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
