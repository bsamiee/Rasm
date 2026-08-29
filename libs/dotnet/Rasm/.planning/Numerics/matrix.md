# [RASM_NUMERICS_MATRIX]

`Rasm.Numerics` grounds every kernel solver on one linear-algebra substrate: the dense, sparse, and complex matrix owners, the solve/eigen/gauge route algebra, and `MatrixKernel`, the sole MathNet and CSparse path they compose. Every operation leaves as a typed solution carrying its route, stop, path evidence, and recomputed residual; a raw `Matrix<double>`, bare factorization, or untyped spectrum never crosses the surface.

Rebuilds compose the `Rasm.Domain` types as the solution validity floor and stay host-neutral: finiteness reads through `TensorPrimitives.IsFiniteAll` over flat spans, every threshold derives from a `ResidualCap` row over an `EpsilonPolicy` anchor or a `ToleranceLane` the caller's `Context` carries, and no absolute literal appears. `MatrixKernel` is `partial` across this page and `Numerics/transform`, so the one-funnel ruling binds a TYPE and not a file. MathNet and CSparse are the mined standard library.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `SolveTrait` capability rows, the route/stop/norm/sense vocabularies keyed on them, and the `GaugePolicy` union owning the singular-system algebra.
- [03]-[DENSE]: dense and packed-symmetric owners with decomposition carriers holding live factors for repeated right-hand sides.
- [04]-[SPARSE]: CSR and Hermitian owners with their structural invariants and the lock-guarded AMD factor cache.
- [05]-[SOLUTIONS]: typed solve, eigen, and gauge evidence carriers on the validity fold.
- [06]-[SOLVE_KERNEL]: `MatrixKernel`, the one MathNet and CSparse access path for every decomposition, solve, and eigen route.

## [02]-[VOCABULARY]

- Owner: `SolveTrait` is the `ICapability` vocabulary every route reads by set algebra — density, fallback status, transposability, element field, direct/iterative discipline, and squareness ride ONE `CapabilitySet<SolveTrait>` column, never parallel `bool` columns; `SolvePath` and `EigenSolvePath` are the route rosters keyed on it, `ResidualCap` the one threshold authority, `KrylovSolver` and `SparsePreconditioner` the two Krylov axes, `SolveStop`/`EigenSolveStop` the terminal partitions, and `GaugePolicy` the `[Union]` owning the singular-system gauge algebra.
- Cases: `GaugePolicy` is `Pin` | `MeanZeroDeflation` | `LagrangeKKT`; `SingularGaugeSolve` is the one generated-dispatch consumer of the mass, basis, shift, and pin payloads, reading each off the case through total `Switch` at the solve, so no projection member and no external roster mirrors the case set; `GaugeShift` rows carry their own post-solve normalization as the `Apply` delegate column, so the kernel invokes the row it was handed and re-dispatches nothing.
- Law: `SolveTrait`'s legal-corner table enumerates every LEGAL corner and `Admit` gates each row at static init — direct-XOR-iterative and complex-outside-eigen are genuinely illegal corners a bare flag pair cannot forbid. NAMED LOSS: per-trait compile-time exhaustiveness, bought back by the construction-time `CapabilityLaw.Admit` and by `AdmitsAll` at each consumer boundary.
- Law: `Conditioned()` is the route's own fallback column — the definite rows point at their rank-revealing or unsymmetric successor and a terminal row points at itself, so primary and conditioning routes are ONE vocabulary the kernel rebinds through one `BindFail`, dense and sparse alike. Deferring behind `static () => …` guards the forward reference: a field initializer reading a later item captures `null` before materialization.
- Law: `SparsePreconditioner` is PRIMARY over the Krylov axis and no route row mirrors it — the four former `SparseBiCgStab*` rows collapsed to one `SparseKrylov` route whose preconditioner and solver ride the solution's `PathEvidence.Iterative` case. NAMED LOSS: `solution.Path.Key` no longer names which preconditioner ran; the evidence case carries the exact `KrylovPlan` and every consumer reading `Stop` is untouched.
- Auto: mass diagonal rides one `Option<Arr<double>>` per gauge case, so one policy value selects Euclidean or M-weighted inner products throughout; `MeanZeroConstant` defaults its post-shift to `GaugeShift.MeanZero` because a deflated solve re-acquires the constant mode through rounding; `OperatorSense.Shape` reports the SENSED operator's extent, so one projection sizes the product's operands AND the solve's right-hand side and solution.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Arr`, `Seq`, `Option`), `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), CSparse (the sensed apply and solve columns bind its array overloads), MathNet.Numerics (the preconditioner and Krylov factories).
- Growth: a new solve substrate is one `SolvePath` row with its trait set, cap, and `Conditioned()` successor, the solution shape unchanged; a new gauge modality is one `GaugePolicy` case whose `Switch` arms break at compile time; a new Krylov engine is one `KrylovSolver` row and a new preconditioner one `SparsePreconditioner` row, neither touching the route roster.
- Boundary: capability reads off the trait set, so a parallel `FactorKind` enum re-declaring the route space never mints, and `Transposed` is a trait on the route because the transposed behaviour binds to the concrete CSparse factor while the route item is instance-free.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using DoubleDouble;
using MathNet.Numerics;
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

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class KrylovSolver {
    public static readonly KrylovSolver BiCgStab = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab());
    public static readonly KrylovSolver GpBiCg = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.GpBiCg());
    public static readonly KrylovSolver Tfqmr = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.TFQMR());
    public static readonly KrylovSolver MlkBiCgStab = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.MlkBiCgStab());
    [UseDelegateFromConstructor] public partial MathNet.Numerics.LinearAlgebra.Solvers.IIterativeSolver<double> Create();
}

[SmartEnum]
public sealed partial class SparsePreconditioner {
    public static readonly SparsePreconditioner None = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Solvers.UnitPreconditioner<double>());
    public static readonly SparsePreconditioner Diagonal = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.DiagonalPreconditioner());
    public static readonly SparsePreconditioner Milu0 = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.MILU0Preconditioner());
    public static readonly SparsePreconditioner Ilutp = new(
        create: static () => new MathNet.Numerics.LinearAlgebra.Double.Solvers.ILUTPPreconditioner());
    [UseDelegateFromConstructor] public partial MathNet.Numerics.LinearAlgebra.Solvers.IPreconditioner<double> Create();
}

public delegate bool KrylovStop(int iteration, double residual);

[ComplexValueObject]
public sealed partial class KrylovPolicy {
    public SparsePreconditioner Preconditioner { get; }
    public KrylovSolver Solver { get; }
    public PositiveMagnitude Tolerance { get; }
    public Dimension Budget { get; }
    public Option<KrylovStop> Stop { get; }
    public bool CanFallback { get; }
    public static Dimension AutoBudget(Dimension rows) =>
        Dimension.Create(value: Math.Max(val1: BudgetFloor, val2: rows.Value * BudgetPerRow));
    public static Dimension BlockBudget(Dimension order, int blocks) =>
        Dimension.Create(value: Math.Min(
            val1: Math.Max(val1: 1, val2: order.Value),
            val2: Math.Max(val1: blocks, val2: 1) * BlockSweeps * (int)Math.Ceiling(a: Math.Sqrt(d: Math.Max(val1: 1, val2: order.Value)))));
    private const int BlockSweeps = 16;
    private const int BudgetFloor = 64;
    private const int BudgetPerRow = 8;
    internal const int DivergenceWarmup = 8;
    internal const int ResidualConfirmations = 2;
}

[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath DenseCholesky = new(key: 1,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath DenseQr = new(key: 2,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath SparseCholesky = new(key: 3,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath SparseLdl = new(key: 4,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath SparseLu = new(key: 5,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath SparseQr = new(key: 6,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public static readonly SolvePath SparseKrylov = new(key: 7,
        defaultTolerance: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
    public static readonly SolvePath DenseFallback = new(key: 8,
        defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);

    public double DefaultTolerance { get; }
    public ToleranceLane Lane { get; }
    public double Tolerance(Option<Context> context) =>
        context.Map(model => model.For(Lane).Value).IfNone(DefaultTolerance);
}

[SmartEnum<int>]
public sealed partial class EigenSolvePath {
    public static readonly EigenSolvePath DenseSymmetric = new(key: 0);
    public static readonly EigenSolvePath DenseGeneral = new(key: 1);
    public static readonly EigenSolvePath SparseLobpcg = new(key: 2);
    public static readonly EigenSolvePath HermitianLobpcg = new(key: 3);
    public static readonly EigenSolvePath DenseCongruence = new(key: 4);
}

[SmartEnum]
public sealed partial class OperatorSense {
    public static readonly OperatorSense Forward = new(
        shape: static (rows, cols) => (rows, cols),
        apply: static (operand, x, y) => operand.Multiply(x, y),
        accumulate: static (operand, alpha, x, beta, y) => operand.Multiply(alpha, x, beta, y),
        solveLu: static (factor, b, x) => factor.Solve(b, x),
        solveQr: static (factor, b, x) => factor.Solve(b, x),
        flipped: static () => Transposed);
    public static readonly OperatorSense Transposed = new(
        shape: static (rows, cols) => (cols, rows),
        apply: static (operand, x, y) => operand.TransposeMultiply(x, y),
        accumulate: static (operand, alpha, x, beta, y) => operand.TransposeMultiply(alpha, x, beta, y),
        solveLu: static (factor, b, x) => factor.SolveTranspose(b, x),
        solveQr: static (factor, b, x) => factor.SolveTranspose(b, x),
        flipped: static () => Forward);

    [UseDelegateFromConstructor] internal partial (Dimension Rows, Dimension Cols) Shape(Dimension rows, Dimension cols);
    [UseDelegateFromConstructor] internal partial OperatorSense Flipped();
    [UseDelegateFromConstructor] internal partial void Apply(CSparse.Storage.CompressedColumnStorage<double> operand, double[] x, double[] y);
    [UseDelegateFromConstructor] internal partial void Accumulate(CSparse.Storage.CompressedColumnStorage<double> operand, double alpha, double[] x, double beta, double[] y);
    [UseDelegateFromConstructor] internal partial void SolveLu(CSparse.Double.Factorization.SparseLU factor, double[] rhs, double[] solution);
    [UseDelegateFromConstructor] internal partial void SolveQr(CSparse.Double.Factorization.SparseQR factor, double[] rhs, double[] solution);
}

[SmartEnum]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new(apply: static (_, x) => x);
    public static readonly GaugeShift MeanZero = new(apply: static (mass, x) => {
        LinearVector ones = DenseVectorD.Create(x.Count, static _ => 1.0);
        LinearVector massOnes = mass.Multiply(ones);
        return x - ((massOnes.DotProduct(x) / Math.Max(EpsilonPolicy.SqrtEpsilon, massOnes.DotProduct(ones))) * ones);
    });
    public static readonly GaugeShift MinZero = new(apply: static (_, x) => x - (x.Minimum() * DenseVectorD.Create(x.Count, static _ => 1.0)));
    [UseDelegateFromConstructor] internal partial LinearVector Apply(Matrix<double> mass, LinearVector x);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GaugePolicy {
    private GaugePolicy() { }
    public sealed record Pin(Arr<int> Indices, Arr<double> Values, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public sealed record MeanZeroDeflation(Arr<Arr<double>> Nullspace, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public sealed record LagrangeKKT(Arr<Arr<double>> Nullspace, Option<Arr<double>> Mass, GaugeShift PostShift) : GaugePolicy;
    public static GaugePolicy Pinned(ReadOnlySpan<int> indices, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
        new Pin(Indices: new Arr<int>([.. indices]), Values: new Arr<double>(new double[indices.Length]), Mass: mass, PostShift: shift.IfNone(noneValue: GaugeShift.None));
    public static GaugePolicy MeanZeroConstant(int dimension, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
        new MeanZeroDeflation(Nullspace: ConstantNullspace(dimension: dimension), Mass: mass, PostShift: shift.IfNone(noneValue: GaugeShift.MeanZero));
    public static GaugePolicy KktConstant(int dimension, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
        new LagrangeKKT(Nullspace: ConstantNullspace(dimension: dimension), Mass: mass, PostShift: shift.IfNone(noneValue: GaugeShift.None));
    private static Arr<Arr<double>> ConstantNullspace(int dimension) =>
        new([new Arr<double>([.. Enumerable.Repeat(element: 1.0, count: Math.Max(val1: 0, val2: dimension))])]);
}
```

## [03]-[DENSE]

- Owner: `Matrix` the dense row-major owner and `SymmetricMatrix` the packed-upper owner, both `Of`-gated on shape and span finiteness; `SvdResult`, `LuResult`, `QrResult`, and `CholeskyResult` are the decomposition carriers, each holding its live MathNet factor `internal` so repeated right-hand sides stream through ONE factorization (the held-handle law) with only typed solutions crossing the public surface.
- Entry: every fallible operation returns `Fin<T>`; `Of` admits through one vectorized span check, never a strided per-element loop; `Norm(kind)` is the one norm entry and `DecomposeSvd` the one evidence handle — a `Frobenius`, `Spectral`, or `Rank` one-hop shell over them is the deleted form, the reads living on the carrier that owns the data.
- Auto: `Matrix.At` reads through one internal `ReadOnlySpan2D<double>` plane, so `(i * Cols + j)` appears nowhere; `SymmetricMatrix.At` folds `(min, max)` into the triangular index so a written entry mirrors by construction; `SymmetricMatrix.FlatIndex` is the ONE packed-upper triangular-address mint — `SampleMoment`'s indexer and every `ILmModel` scatter read it directly, so the layout formula moves as one edit.
- Auto: `SymmetricMatrix.Definite` is the branch's one allocation-bounded positive-definiteness verdict — a packed-upper Cholesky sweep over pooled scratch answering in the result, so an unadmitted matrix, an indefinite pivot, and a definite operator are three verdicts and a consumer proving a metric tensor SPD reads it instead of hand-rolling leading principal minors.
- Packages: MathNet.Numerics (dense factorizations and norms), System.Numerics.Tensors (finiteness admission), CommunityToolkit.HighPerformance (`ReadOnlySpan2D` plane projection, `SpanOwner` scratch), LanguageExt.Core, Thinktecture.Runtime.Extensions (`Dimension`).
- Growth: a new dense decomposition adds one `Decompose*` member returning a typed carrier holding its factor with one `SolvePath` row, never a sibling matrix type; a norm is one `MatrixNormKind` row.
- Boundary: MathNet types never cross the public surface — `Matrix`/`Arr<double>` in, typed solutions out, the `internal` factor handles the held-handle exception. Symmetric consumers construct `SymmetricMatrix`, never a dense `Matrix` asserted symmetric: MathNet's `IsSymmetric()` compares with exact `!=`, which accumulation-built operators fail. `QrResult` holds its factor like its three siblings, so a least-squares stream re-solves rather than re-factorizing per right-hand side.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SymmetricMatrix : IValidityEvidence {
    private SymmetricMatrix(Dimension dimension, Arr<double> upper) => (Dimension, Upper) = (dimension, upper);
    public Dimension Dimension { get; }
    public Arr<double> Upper { get; }
    public static Fin<SymmetricMatrix> Of(Dimension dim, Arr<double> upper) =>
        from _ in guard(upper.Count == dim.Value * (dim.Value + 1) / 2, new KernelFault.InvalidInput()).ToFin()
        from finite in guard(TensorPrimitives.IsFiniteAll<double>(upper.AsSpan()), new KernelFault.InvalidInput())
        select new SymmetricMatrix(dimension: dim, upper: upper);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: Upper.Count, expected: Dimension.Value * (Dimension.Value + 1) / 2),
        ValidityClaim.Finite(Upper.AsSpan()));
    public Matrix<double> ToDense() => MatrixKernel.Expanded(matrix: this);
    public Fin<EigenSolution<double, Arr<double>>> DecomposeEigenDetailed() => MatrixKernel.SymmetricEigen(matrix: this);
    public Fin<MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double>> DecomposeCholesky() => MatrixKernel.Cholesky(matrix: this);
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        new(dimension: Dimension, upper: Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value));
    internal static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
}

```

## [04]-[SPARSE]

- Owner: `SparseMatrix` the CSR owner whose `IsValid` is its admission — factorizing invalid storage produces silently wrong factors; `SparseHermitian` the complex upper-store owner with conjugate reconstruction on multiply and a real-diagonal gate; `CholeskySparse` the SPD factor cache over CSparse `SparseCholesky` built under one AMD ordering, `Lock`-guarded because CSparse solves share scratch and a concurrent second solve corrupts both results silently, success-only so a broken factor never enters reuse, and the sole owner of the three factor-movement verbs and the pattern sweep the cached symbolic analysis makes cheap.
- Entry: `FromTriplets` admits any triplet stream — duplicates sum, zeros drop, out-of-range or non-finite fails typed — so consumers assemble by accumulation, never hand-build CSR; `Transpose()` is the ONE materialized CSR transpose — CSR(A)'s buffers ARE CSC(Aᵀ)'s, so the member wraps them zero-copy, runs the CSparse storage `Transpose()` (the package's counting re-index, sorted unique rows by its own law), and reads the result's CSC buffers back as CSR(Aᵀ) — a consumer's triplet re-assembly or hand CSR re-index is the deleted form, while an APPLIED transpose (`A'x`) stays `Multiply` under `OperatorSense`/`TransposeMultiply` and never materializes; `SolveDetailed(rhs, Option<KrylovPolicy>)` is the ONE square sparse entry, an absent policy taking the auto-routed Jacobi Krylov with the dense MathNet fallback admitted and a stated policy governing preconditioner, engine, budget, the optional `KrylovStop` halt callback, and the dense-fallback permission; `SolveIndefiniteDetailed` and `SolveLeastSquaresDetailed` are the two transposable direct routes, both taking an `OperatorSense` rather than minting a transposed twin.
- Law: every entry returns its whole `LinearSolution` and no `Map(r => r.Solution)` projection ships beside it — consuming a minted solution without reading its stop is this page's named consumer defect, and a shell publishing exactly that defect as API is the deleted form.
- Auto: `SparseHermitian.IsValid` gates the stored diagonal real within a scale-relative band, so a drifted assembly is caught at the owner, not inside LOBPCG; `Refactorize` re-runs the numeric phase against the cached `SymbolicFactorization`, so a parameter sweep over one pattern pays the AMD analysis once, and `Update`/`Downdate` move the standing factor by a rank-1 term so an added or removed constraint costs no refactorization — a `false` verdict means the partial tree walk already corrupted the factor and the caller re-mints rather than retries; `Sweep` brackets the process-global exact-fit trim flag once for a whole value sequence, so the factor buffers survive the loop the flag exists for.
- Packages: CSparse (the ordering-taking `Create` overloads under `ColumnOrdering.MinimumDegreeAtPlusA`, `SparseCholesky`/`SparseLDL`/`SparseLU`/`SparseQR` factorization, `Refactorize`/`Update`/`Downdate`, `CompressedColumnStorage.AutoTrimStorage`, the `IProgress<double>` phase report `Refactorize` deliberately carries none of), MathNet.Numerics (sparse storage and its duplicate-summing and diagonal-populating repairs, the Krylov engines and criterion stack), `Domain/validation` (`Admit.FiniteComplexSpan`/`HermitianDiagonalRealSpan`, the one complex-spectrum gate pair), LanguageExt.Core, BCL (`System.Threading.Lock`, `System.Numerics.Complex`, `IProgress<double>`).
- Growth: a new sparse capability adds one member and one column over the same owners; a second CSR/CSC representation beside `SparseMatrix` is the deleted form — format bridges live inside `MatrixKernel`.
- Boundary: mesh Laplacian memoization caches these factor objects, so their identity and `Lock` semantics compose from here; a transposed solve is linear-algebra vocabulary on the standing factor and nothing more — the adjoint sensitivity band that composes it stays `Rasm.Compute`'s, and a kernel differentiation owner beside `Lm`'s forward-mode dual floor never mints here.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian : IValidityEvidence {
    private SparseHermitian(Dimension order, CSparse.Storage.CompressedColumnStorage<Complex> upper) =>
        (Order, Upper) = (order, upper);
    public Dimension Order { get; }
    public CSparse.Storage.CompressedColumnStorage<Complex> Upper { get; }
    public static Fin<SparseHermitian> FromTriplets(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> upperTriplets) {
        return Optional(upperTriplets).ToFin(new KernelFault.InvalidInput()).Bind(active => MatrixKernel.AssembleHermitian(order: order, triplets: active));
    }
    internal static SparseHermitian Trusted(Dimension order, CSparse.Storage.CompressedColumnStorage<Complex> upper) =>
        new(order: order, upper: upper);
    public bool IsValid => ValidityClaim.All(
        Upper.RowCount == Order.Value && Upper.ColumnCount == Order.Value,
        CSparse.Helper.ValidateStorage(Upper, true),
        Admit.FiniteComplexSpan(Upper.Values.AsSpan(0, Upper.NonZerosCount)),
        Admit.HermitianDiagonalRealSpan(Enumerable.Range(0, Order.Value).Select(i => Upper.At(i, i)).ToArray()));
    public int NonZeros => Upper.NonZerosCount;
    public double FrobeniusScale {
        get {
            double diagonal = 0.0, offDiagonal = 0.0;
            for (int column = 0; column < Order.Value; column++)
                for (int p = Upper.ColumnPointers[column]; p < Upper.ColumnPointers[column + 1]; p++) {
                    double magnitude = (Upper.Values[p] * Complex.Conjugate(Upper.Values[p])).Real;
                    if (Upper.RowIndices[p] == column) diagonal += magnitude; else offDiagonal += magnitude;
                }
            return Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: Math.Sqrt(d: diagonal + (2.0 * offDiagonal)));
        }
    }
    public Fin<Arr<Complex>> Multiply(Arr<Complex> vector) =>
        vector.Count != Order.Value || !Admit.FiniteComplexSpan(vector.AsSpan())
            ? Fin.Fail<Arr<Complex>>(new KernelFault.InvalidInput())
            : MatrixKernel.HermitianProduct(self: this, x: vector);
    public Fin<EigenSolution<double, Arr<Complex>>> SmallestEigenpairsDetailed(int k, double tolerance, Dimension budget) =>
        MatrixKernel.LobpcgHermitian(matrix: this, k: k, tolerance: tolerance, budget: budget);
}

public sealed record CholeskySparse : IValidityEvidence {
    private CholeskySparse(CSparse.Storage.CompressedColumnStorage<double> source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) {
        Source = source; Factor = factor; Order = order;
    }
    private readonly Lock solveLock = new();
    internal void SolveGuarded(double[] b, double[] x) { lock (solveLock) { Factor.Solve(input: b.AsSpan(), result: x.AsSpan()); } }
    public static Fin<CholeskySparse> Of(CSparse.Storage.CompressedColumnStorage<double> symmetric, Option<IProgress<double>> progress = default) =>
        symmetric.RowCount != symmetric.ColumnCount
            ? Fin.Fail<CholeskySparse>(error: new KernelFault.InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric)
              from factor in Try.lift(() => progress.Match(
                  Some: report => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, progress: report),
                  None: () => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA))).Run()
              select new CholeskySparse(source: symmetric, factor: factor, order: Dimension.Create(symmetric.RowCount));
    public CSparse.Storage.CompressedColumnStorage<double> Source { get; }
    internal CSparse.Double.Factorization.SparseCholesky Factor { get; }
    public Dimension Order { get; }
    public int FactorNonZeros => Factor.NonZerosCount;
    public bool IsValid => ValidityClaim.All(CSparse.Helper.ValidateStorage(Source, true), Factor.NonZerosCount > 0, Order.Value >= 1);
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) =>
        MatrixKernel.CholeskySparseSolve(factor: this, rhs: rhs);
}
```

## [05]-[SOLUTIONS]

- Owner: `PathEvidence` the closed union carrying whatever the taken route MEASURED, `LinearSolution` the one linear-solve result, `EigenSolution<TEigen, TVector>` the one eigen result generic over real or complex spectra, and `GaugeFix` the gauge a singular solve applied — all spelling the validity fold `IsValid => ValidityClaim.All(…)` under `IValidityEvidence`.
- Cases: `PathEvidence` is `Direct` | `Factored(FactorNonZeros)` | `Ranked(Option<int> Rank, Columns, FactorNonZeros)` | `Iterative(Iterations, Budget, Tolerance, Plan)` — mutually exclusive by construction, so an iterative solution carrying a factor count is unrepresentable and the six vacuous per-column absents the two results once carried delete with them. ONE union serves both the linear and the eigen solution: it was already named for solve paths and read only by the eigen carrier.
- Law: a failure is a FAILED RESULT, never a success-shaped solution — no `NumericalBreakdown` column and no `PositiveInfinity` residual exist, because a magnitude sentinel is a measurement no producer took and every consumer that gates on magnitude reads it as one.
- Entry: solutions mint only at the `MatrixKernel` exits (`SolveSuccess`, `EigenSolutionOf`) under the two-tier evidence law — hard numerical garbage never mints and fails typed, a usable-stop solution is gated valid before release, and a non-usable-stop solution (`RankDeficient`, `IterativeExhausted`, `ResidualRejected`) is the witnessed refusal the caller reads off `Stop.IsUsable` before consuming the vector. Consuming a minted solution without reading its stop is the named consumer defect.
- Auto: each `IsValid` conjoins the mechanical field-shape gates with the semantic couplings — residual within the route's own recorded cap, iterations within budget, and the length pair read off the SENSED operator so a transposed solve is validated against `A'`'s extent — one claim row per invariant; the sense-versus-capability coupling refuses a transposed solution on a route whose trait set omits `Transposed`, so a mislabelled direction cannot mint.
- Packages: `Domain/validation` (`IValidityEvidence`), `Domain/results` (`ValidityClaim`, the validity floor), LanguageExt.Core (`Option`, `Seq`, `Arr`).
- Law: `GaugeFix` carries `PinIndices` and no separate `PinnedIndex` slot — NAMED LOSS of the former single-pin accessor, derivable as `PinIndices.Head` and never a second stored copy free to disagree with the roster.
- Growth: new evidence is one `PathEvidence` case or one field with at most one claim row; a new outcome family is one result type only when its evidence shape is disjoint (the eigen/solve/gauge split), never per-algorithm result clones.
- Boundary: `Option<T>` carries absence of evidence, never a sentinel; `InputNonZeros` is the one structurally-absent slot — a dense operator has no nonzero census to take — and the stored residual is always recomputed against the original operator, a preconditioned or factor-reconstructed residual being the named lying witness.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathEvidence {
    private PathEvidence() { }
    public sealed record Direct : PathEvidence;
    public sealed record Factored(int FactorNonZeros) : PathEvidence;
    public sealed record Ranked(Option<int> Rank, int Columns, int FactorNonZeros) : PathEvidence;
    public sealed record Iterative(int Iterations, Dimension Budget, double Tolerance, Option<(SparsePreconditioner Preconditioner, KrylovSolver Solver)> Plan) : PathEvidence;

    public ValidityClaim Holds(double residual) => Switch(
        state: residual,
        direct: static (_, _) => new ValidityClaim(Holds: true),
        factored: static (_, path) => new ValidityClaim(Holds: path.FactorNonZeros > 0),
        ranked: static (_, path) => new ValidityClaim(Holds: path.Rank.Map(rank => rank >= 0 && rank <= path.Columns).IfNone(noneValue: true) && path.FactorNonZeros >= 0),
        iterative: static (measured, path) => new ValidityClaim(
            Holds: path.Iterations >= 0 && path.Iterations <= path.Budget.Value && measured <= path.Tolerance));
}

public readonly record struct LinearSolution(
    Arr<double> Solution, SolvePath Path, PathEvidence Evidence,
    double Residual, double Tolerance, Option<GaugeFix> Gauge = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Solution.AsSpan()),
        Residual >= 0.0 && Residual <= Tolerance,
        Evidence.Holds(Residual),
        ValidityClaim.Evidence(Gauge));
}

public readonly record struct EigenSolution<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path,
    PathEvidence Evidence, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Pairs.Count > 0,
        ValidityClaim.Nonnegative(MaxResidual),
        Evidence.Holds(MaxResidual));
}

public readonly record struct GaugeFix(
    int Nullity, Option<int> BasisRank,
    double OperatorScale, double CompatibilityResidual, double GaugeResidual, double MassResidual,
    Arr<int> PinIndices, GaugeShift Shift, double RhsMutationNorm,
    Option<double> MultiplierNorm, double GaugeOrthogonality, double Regularization) : IValidityEvidence {
    public bool IsValid {
        get {
            return ValidityClaim.All(
                Nullity >= 0,
                PinIndices.ForAll(static index => index >= 0),
                ValidityClaim.Positive(value: OperatorScale),
                ValidityClaim.Nonnegative(value: CompatibilityResidual),
                ValidityClaim.Nonnegative(value: GaugeResidual),
                ValidityClaim.Nonnegative(value: MassResidual),
                ValidityClaim.Nonnegative(value: RhsMutationNorm),
                ValidityClaim.Nonnegative(value: GaugeOrthogonality)
                    && GaugeOrthogonality <= OperatorScale * Math.Sqrt(d: EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Nonnegative(value: Regularization) && Regularization <= OperatorScale * EpsilonPolicy.SqrtEpsilon,
                MultiplierNorm.Map(static norm => norm >= 0.0).IfNone(noneValue: true),
                BasisRank.Map(count => count <= Nullity).IfNone(noneValue: true));
        }
    }
}
```

## [06]-[SOLVE_KERNEL]

- Owner: `MatrixKernel` the `internal static partial` numeric kernel and the one MathNet and CSparse factorization, solve, and eigen path in the corpus, organized by operation family. `Numerics/transform` carries the `partial` half with the spectral and tap-fold bands; the funnel is a TYPE, so no consumer reaches a raw MathNet member on either side of the file cut.
- Entry: every public-facing operation enters through the owning model member; the kernel is reached only through them.
- Law: a route that misses its residual cap REBINDS through `SolvePath.Conditioned()` and re-solves, so the conditioning fallback is the vocabulary's own column and not a per-entry `if`; a route whose `Conditioned()` is itself is terminal and lowers a typed exhaustion fault naming the route it exhausted.
- Auto: `SingularGaugeSolve` derives every threshold from the operator's `MatrixNormKind.Frobenius` scale and the caller's `Context` lanes — relative gates, never absolute literals — and witnesses the true residual against the original un-shifted operator; `LobpcgCore` seeds its basis deterministically off the `Domain/identity` splitmix64 owner for bit-stable replay and terminates typed; CSR compression is the storage owner's — `OfIndexedEnumerable` sorts each row, `NormalizeDuplicates` sums the coincident run the admission APPENDS, and `PopulateExplicitZerosOnDiagonal` restores the structural diagonal the zero-drop erases before any preconditioner walks it.
- Exemption: statement loops inside `SolvePin`, `SolveKkt`, `SparseTripletsOf`, `SymmetricUpper` (its mutable coincident-entry fold), the definite sweep, the modified Gram-Schmidt pass, and the Ritz scatter are the named statement-kernel exemption — measured assembly, elimination, and packed-triangular hot paths; `BiCgStabDivergenceFactor` is the one named kernel policy constant, every other threshold reading a `ResidualCap` row.
- Packages: MathNet.Numerics (the managed provider path only — `UseManaged`, no `Control.UseNative*` call, no provider package — its CSR storage repairs, the Krylov engines, and the dense factorizations), CSparse (`SparseCholesky`, `SparseLDL`, `SparseLU`, `SparseQR` under `ColumnOrdering`, the transposed solves and the rank-1 factor moves, the raw-buffer `SparseMatrix` ctor and storage `Transpose()` the CSR transpose composes), `Rasm.Domain` (`Context`, `Deterministic` splitmix64), System.Numerics.Tensors, CommunityToolkit.HighPerformance (`SpanOwner`/`MemoryOwner` scratch), TYoshimura.DoubleDouble (`ddouble`, the 106-bit residual-witness lane), BCL (`System.Numerics.Complex`, `IProgress<double>`).
- Growth: a new route, gauge case, or eigen substrate adds one kernel arm over its vocabulary row and the existing solution shape.
- Boundary: `Matrix<T>.GramSchmidt()` is REFUSED for the LOBPCG basis pass and the refusal is named at the site — the survivor-deflation step requires rank-collapsed columns to remain EXACTLY zero, which a factorization contract promising an orthonormal `Q` neither publishes nor preserves. `SparseLDL` publishes no inertia, so the KKT route composes it for the halved work alone and the saddle's `(n, m)` signature stays unpublished rather than asserted.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<long>(KeyMemberName = nameof(IDrawLane<MatrixDrawLane>.Lane))]
public sealed partial class MatrixDrawLane : IDrawLane<MatrixDrawLane> {
    public static readonly MatrixDrawLane RealBasis = new(17L);
    public static readonly MatrixDrawLane HermitianBasis = new(19L);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class MatrixKernel {
    private const double BiCgStabDivergenceFactor = 1e3;

    // --- [BRIDGES] ---------------------------------------------------------------------
    internal static Fin<Matrix<double>> Dense(Dimension rows, Dimension cols, Arr<double> entries) =>
        from _ in guard(entries.Count == rows.Value * cols.Value, new KernelFault.InvalidInput()).ToFin()
        from _ in guard(TensorPrimitives.IsFiniteAll<double>(entries.AsSpan()), new KernelFault.InvalidInput())
        select DenseMatrixD.Build.Dense(rows.Value, cols.Value,
            (row, column) => entries[(row * cols.Value) + column]);
    internal static Matrix<double> Expanded(SymmetricMatrix matrix) {
        int n = matrix.Dimension.Value;
        double[] dense = new double[n * n];
        for (int i = 0; i < n; i++)
            for (int j = i; j < n; j++) {
                double value = matrix.Upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: j)];
                dense[(i * n) + j] = value;
                dense[(j * n) + i] = value;
            }
        return DenseMatrixD.Build.DenseOfRowMajor(n, n, dense);
    }
    internal static Fin<CSparse.Storage.CompressedColumnStorage<double>> ToCSparseSymmetric(CSparse.Storage.CompressedColumnStorage<double> s) =>
        SymmetricUpper(s: s).Map(upper =>
            CSparse.Double.SparseMatrix.OfIndexed(rows: s.RowCount, columns: s.RowCount, enumerable: upper));
    private static Fin<List<(int Row, int Col, double Value)>> SymmetricUpper(CSparse.Storage.CompressedColumnStorage<double> s) {
        if (s.RowCount != s.ColumnCount) return Fin.Fail<List<(int, int, double)>>(new KernelFault.InvalidInput());
        Dictionary<(int Row, int Col), double> folded = new(capacity: s.NonZerosCount);
        for (int column = 0; column < s.ColumnCount; column++)
            for (int k = s.ColumnPointers[column]; k < s.ColumnPointers[column + 1]; k++) {
                int row = s.RowIndices[k];
                (int Row, int Col) slot = (Math.Min(val1: row, val2: column), Math.Max(val1: row, val2: column));
                double value = s.Values[k];
                if (folded.TryGetValue(key: slot, value: out double held)
                    && Math.Abs(value: value - held) > EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value), val2: Math.Abs(held))))
                    return Fin.Fail<List<(int, int, double)>>(new KernelFault.InvalidInput());
                folded[slot] = value;
            }
        return Fin.Succ([.. folded.OrderBy(static e => e.Key.Row).ThenBy(static e => e.Key.Col).Select(static e => (e.Key.Row, e.Key.Col, e.Value))]);
    }
    private static Matrix<Complex> ToMathNetHermitian(SparseHermitian s) =>
        SparseMatrixC.OfIndexed(rows: s.Order.Value, columns: s.Order.Value, enumerable: s.Upper.EnumerateIndexedAsValueTuples()
            .SelectMany(entry => entry.Item1 == entry.Item2
                ? [(entry.Item1, entry.Item2, entry.Item3)]
                : new[] { (entry.Item1, entry.Item2, entry.Item3), (entry.Item2, entry.Item1, Complex.Conjugate(entry.Item3)) }));
    private static Matrix<double> ToMathNetSparse(CSparse.Storage.CompressedColumnStorage<double> s) {
        SparseCompressedRowMatrixStorage<double> storage = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(
            rows: s.RowCount, columns: s.ColumnCount, valueCount: s.NonZerosCount,
            rowIndices: s.RowIndices, columnPointers: s.ColumnPointers, values: s.Values);
        storage.PopulateExplicitZerosOnDiagonal();
        return DenseMatrixD.Build.Sparse(storage);
    }
    private static Matrix<double> ToMathNetSymmetric(CSparse.Storage.CompressedColumnStorage<double> matrix, IEnumerable<(int Row, int Col, double Value)> upper) =>
        SparseMatrixD.OfIndexed(rows: matrix.RowCount, columns: matrix.ColumnCount, enumerable: upper.SelectMany(static e => e.Row == e.Col
            ? [(e.Row, e.Col, e.Value)]
            : new[] { (e.Row, e.Col, e.Value), (e.Col, e.Row, e.Value) }));
    // --- [WITNESS] ---------------------------------------------------------------------
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        CompensatedNorm(v: b - a.Multiply(x)) / Math.Max(val1: 1.0, val2: CompensatedNorm(v: b));
    private static double BackwardError(Matrix<double> a, LinearVector x, LinearVector b, double operatorScale) =>
        CompensatedNorm(v: b - a.Multiply(x)) / ((operatorScale * x.L2Norm()) + Math.Max(val1: 1.0, val2: CompensatedNorm(v: b)));
    private static double CompensatedNorm(LinearVector vector) =>
        Math.Sqrt((double)vector.Enumerate()
            .Select(static value => (ddouble)value * value)
            .Sum());
    internal static bool RhsFits(int rows, Arr<double> rhs) =>
        rhs.Count == rows && TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan());
    internal static Fin<double> SparseSymmetricResidual(CSparse.Storage.CompressedColumnStorage<double> matrix, Arr<double> solution, Arr<double> rhs) =>
        SymmetricUpper(s: matrix).Bind(upper => Admit.Finite(value: RelativeResidual(
            a: ToMathNetSymmetric(matrix: matrix, upper: upper),
            x: DenseVectorD.OfArray([.. solution.AsIterable()]),
            b: DenseVectorD.OfArray([.. rhs.AsIterable()]))));
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
    internal static Fin<LinearSolution> SolveSuccess(Arr<double> solution, SolvePath path,
        Dimension rows, Dimension cols, int rhsLength, double residual, PathEvidence evidence,
        double tolerance, Option<OperatorSense> sense = default, Option<GaugeFix> gauge = default) {
        (Dimension sensedRows, Dimension sensedCols) = sense.IfNone(noneValue: OperatorSense.Forward).Shape(rows: rows, cols: cols);
        if (rhsLength != sensedRows.Value || solution.Count != sensedCols.Value
            || !TensorPrimitives.IsFiniteAll<double>(solution.AsSpan())
            || !double.IsFinite(residual) || residual < 0.0
            || !double.IsFinite(tolerance) || tolerance <= 0.0)
            return Fin.Fail<LinearSolution>(new KernelFault.InvalidResult());
        if (residual > tolerance)
            return Fin.Fail<LinearSolution>(new KernelFault.ResidualExceeded());
        LinearSolution solved = new(Solution: solution, Path: path, Evidence: evidence, Residual: residual, Tolerance: tolerance, Gauge: gauge);
        return solved.IsValid
            ? Fin.Succ(solved)
            : Fin.Fail<LinearSolution>(new KernelFault.InvalidResult());
    }
    private static Fin<EigenSolution<TEigen, TVector>> EigenSolutionOf<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, int requestedPairs, double maxResidual, PathEvidence evidence) {
        if (pairs.Count == 0) return Fin.Fail<EigenSolution<TEigen, TVector>>(new KernelFault.RankDeficient());
        if (pairs.Count > requestedPairs || !double.IsFinite(maxResidual))
            return Fin.Fail<EigenSolution<TEigen, TVector>>(new KernelFault.InvalidResult());
        EigenSolution<TEigen, TVector> solved = new(Pairs: pairs, Path: path, Evidence: evidence, MaxResidual: maxResidual);
        return solved.IsValid
            ? Fin.Succ(solved)
            : Fin.Fail<EigenSolution<TEigen, TVector>>(new KernelFault.InvalidResult());
    }
    // --- [DENSE_DECOMPOSITIONS] --------------------------------------------------------
    internal static Fin<MathNet.Numerics.LinearAlgebra.Factorization.Svd<double>> Svd(Matrix<double> matrix) =>
        Try.lift(() => matrix.Svd(computeVectors: true)).Run();
    internal static Fin<MathNet.Numerics.LinearAlgebra.Factorization.LU<double>> Lu(Matrix<double> matrix) =>
        matrix.RowCount != matrix.ColumnCount
            ? Fin.Fail<MathNet.Numerics.LinearAlgebra.Factorization.LU<double>>(new KernelFault.InvalidInput())
            : Try.lift(matrix.LU).Run();
    internal static Fin<MathNet.Numerics.LinearAlgebra.Factorization.QR<double>> Qr(Matrix<double> matrix) =>
        Try.lift(() => matrix.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin)).Run();
    internal static Fin<MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double>> Cholesky(SymmetricMatrix matrix) =>
        Try.lift(() => matrix.ToDense().Cholesky()).Run();
    internal static Fin<EigenSolution<double, Arr<double>>> SymmetricEigen(SymmetricMatrix matrix) =>
        Try.lift(() => {
                Matrix<double> mathNet = Expanded(matrix: matrix);
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = mathNet.Evd(Symmetricity.Symmetric);
                int n = matrix.Dimension.Value;
                Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: new Arr<double>(evd.EigenVectors.Column(i).ToArray())))
                    .OrderByDescending(static p => Math.Abs(p.Eigenvalue)));
                return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseSymmetric, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector), evidence: new PathEvidence.Direct());
            }).Run().Bind(static inner => inner);
    internal static Fin<EigenSolution<Complex, Arr<Complex>>> GeneralEigen(Matrix<double> matrix) =>
        matrix.RowCount != matrix.ColumnCount
            ? Fin.Fail<EigenSolution<Complex, Arr<Complex>>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                Matrix<Complex> mathNet = DenseMatrixC.Build.Dense(matrix.RowCount, matrix.ColumnCount, (i, j) => new Complex(matrix[i, j], 0.0));
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = mathNet.Evd(Symmetricity.Asymmetric);
                int n = matrix.RowCount;
                Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: new Arr<Complex>(evd.EigenVectors.Column(i).Normalize(p: 2.0).ToArray()))));
                return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseGeneral, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue), evidence: new PathEvidence.Direct());
            }).Run().Bind(static inner => inner);
    internal static Fin<double> Determinant(Matrix<double> matrix) =>
        matrix.RowCount != matrix.ColumnCount
            ? Fin.Fail<double>(error: new KernelFault.InvalidInput())
            : Try.lift(() => Admit.Finite(value: matrix.Determinant())).Run().Bind(static inner => inner);

    // --- [DENSE_SOLVES] ----------------------------------------------------------------
    internal static Fin<LinearSolution> Solve(Matrix<double> matrix, Arr<double> rhs) =>
        DenseGate(source: matrix, rhs: rhs, path: SolvePath.DenseLu)
            .Bind(_ => Lu(matrix: matrix))
            .Bind(lu => LuSolve(source: matrix, factor: lu, rhs: rhs))
            .BindFail(_ => LeastSquares(matrix: matrix, rhs: rhs));
    internal static Fin<LinearSolution> LeastSquares(Matrix<double> matrix, Arr<double> rhs) =>
        DenseGate(source: matrix, rhs: rhs, path: SolvePath.DenseQr)
            .Bind(_ => Qr(matrix: matrix))
            .Bind(qr => QrSolve(source: matrix, factor: qr, rhs: rhs));
    internal static Fin<LinearSolution> LuSolve(Matrix<double> source, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor, Arr<double> rhs) =>
        DenseGate(source: source, rhs: rhs, path: SolvePath.DenseLu)
            .Bind(_ => DenseSolve(source: source, rhs: rhs, path: SolvePath.DenseLu,
                solve: new Func<LinearVector, LinearVector>(factor.Solve), evidence: new PathEvidence.Direct()));
    internal static Fin<LinearSolution> QrSolve(Matrix<double> source, MathNet.Numerics.LinearAlgebra.Factorization.QR<double> factor, Arr<double> rhs) =>
        !factor.IsFullRank
            ? Fin.Fail<LinearSolution>(new KernelFault.RankDeficient())
            : DenseGate(source: source, rhs: rhs, path: SolvePath.DenseQr)
            .Bind(_ => DenseSolve(source: source, rhs: rhs, path: SolvePath.DenseQr,
                solve: new Func<LinearVector, LinearVector>(factor.Solve),
                evidence: new PathEvidence.Ranked(Rank: Some(source.ColumnCount), Columns: source.ColumnCount, FactorNonZeros: 0)));
    internal static Fin<LinearSolution> CholeskySolve(Matrix<double> source, MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor, Arr<double> rhs) =>
        DenseGate(source: source, rhs: rhs, path: SolvePath.DenseCholesky)
            .Bind(_ => DenseSolve(source: source, rhs: rhs, path: SolvePath.DenseCholesky,
                solve: new Func<LinearVector, LinearVector>(factor.Solve), evidence: new PathEvidence.Direct()));
    private static Fin<Unit> DenseGate(Matrix<double> source, Arr<double> rhs, SolvePath path) =>
        guard(RhsFits(rows: source.RowCount, rhs: rhs)
            && (!path.Equals(SolvePath.DenseLu) && !path.Equals(SolvePath.DenseCholesky)
                || source.RowCount == source.ColumnCount), new KernelFault.InvalidInput()).ToFin();
    private static Fin<LinearSolution> DenseSolve(Matrix<double> source, Arr<double> rhs, SolvePath path, Func<LinearVector, LinearVector> solve, PathEvidence evidence) =>
        Try.lift(() => {
            Matrix<double> a = source;
            LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
            LinearVector x = solve(arg: b);
            return SolveSuccess(solution: new Arr<double>(x.ToArray()), path: path,
                rows: Dimension.Create(source.RowCount), cols: Dimension.Create(source.ColumnCount), rhsLength: rhs.Count, residual: RelativeResidual(a: a, x: x, b: b),
                evidence: evidence, tolerance: path.DefaultTolerance);
        }).Run().Bind(static inner => inner);
    // --- [SPARSE_ASSEMBLY] -------------------------------------------------------------
    internal static Fin<CSparse.Storage.CompressedColumnStorage<double>> Sparse(
        Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets) =>
        Optional(triplets).ToFin(new KernelFault.InvalidInput()).Bind(active =>
            toSeq(active).Traverse(entry =>
                guard(entry.Row >= 0 && entry.Row < rows.Value
                    && entry.Col >= 0 && entry.Col < cols.Value
                    && double.IsFinite(entry.Value), new KernelFault.InvalidInput())
                .ToFin().Map(_ => entry)).As().Map(entries => {
                    CSparse.Storage.CoordinateStorage<double> coo = new(rows.Value, cols.Value, entries.Count);
                    entries.Iter(entry => coo.At(entry.Row, entry.Col, entry.Value));
                    return CSparse.Double.SparseMatrix.OfIndexed(coo, inplace: true);
                }));
    internal static Fin<SparseHermitian> AssembleHermitian(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> triplets) {
        List<(int Row, int Col, Complex Value)> raw = [.. triplets];
        if (raw.Exists(static t => !double.IsFinite(t.Value.Real) || !double.IsFinite(t.Value.Imaginary)) || raw.Exists(t => t.Row < 0 || t.Col < 0 || t.Row >= order.Value || t.Col >= order.Value || t.Row > t.Col)) return Fin.Fail<SparseHermitian>(new KernelFault.InvalidInput());
        List<(int Row, int Col, Complex Value)> upper = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Aggregate(Complex.Zero, static (acc, t) => acc + t.Value)))
            .OrderBy(static t => t.Row).ThenBy(static t => t.Col)];
        double diagonalScale = upper.Where(static t => t.Row == t.Col).Aggregate(seed: 0.0, func: static (max, t) => Math.Max(val1: max, val2: Math.Abs(value: t.Value.Real)));
        double diagonalBand = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: diagonalScale * EpsilonPolicy.SqrtEpsilon);
        if (upper.Exists(t => t.Row == t.Col && Math.Abs(value: t.Value.Imaginary) > diagonalBand)) return Fin.Fail<SparseHermitian>(new KernelFault.InvalidResult());
        CSparse.Storage.CoordinateStorage<Complex> coo = new(order.Value, order.Value, upper.Count);
        upper.ForEach(t => coo.At(t.Row, t.Col, t.Row == t.Col ? new Complex(t.Value.Real, 0.0) : t.Value));
        SparseHermitian result = SparseHermitian.Trusted(order, CSparse.Complex.SparseMatrix.OfIndexed(coo, inplace: true));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseHermitian>(new KernelFault.InvalidResult());
    }
    // --- [SPARSE_SOLVES] ---------------------------------------------------------------
    internal static Fin<Arr<double>> SparseProduct(CSparse.Storage.CompressedColumnStorage<double> self, Arr<double> x, OperatorSense sense) {
        (Dimension rows, Dimension cols) = sense.Shape(rows: Dimension.Create(self.RowCount), cols: Dimension.Create(self.ColumnCount));
        return x.Count != cols.Value || !TensorPrimitives.IsFiniteAll<double>(x.AsSpan())
            ? Fin.Fail<Arr<double>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                double[] y = new double[rows.Value];
                sense.Apply(operand: self, x: [.. x.AsIterable()], y: y);
                Arr<double> result = new(y);
                return TensorPrimitives.IsFiniteAll<double>(result.AsSpan()) ? Fin.Succ(result) : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult());
            }).Run().Bind(static inner => inner);
    }
    internal static Fin<Arr<Complex>> HermitianProduct(SparseHermitian self, Arr<Complex> x) =>
        Try.lift(() => new Arr<Complex>(ToMathNetHermitian(s: self).Multiply(DenseVectorC.OfArray([.. x.AsIterable()])).ToArray()) switch {
            Arr<Complex> result when Admit.FiniteComplexSpan(result.AsSpan()) => Fin.Succ(result),
            _ => Fin.Fail<Arr<Complex>>(new KernelFault.InvalidResult()),
        }).Run().Bind(static inner => inner);
    internal static Fin<LinearSolution> SparseSolve(CSparse.Storage.CompressedColumnStorage<double> matrix, Arr<double> rhs, Option<KrylovPolicy> policy) {
        if (matrix.RowCount != matrix.ColumnCount || !RhsFits(rows: matrix.RowCount, rhs: rhs)) return Fin.Fail<LinearSolution>(new KernelFault.InvalidInput());
        KrylovPolicy active = policy.IfNone(noneValue: KrylovPolicy.Create(
            SparsePreconditioner.Diagonal, KrylovSolver.BiCgStab,
            PositiveMagnitude.Create(SolvePath.SparseKrylov.DefaultTolerance),
            KrylovPolicy.AutoBudget(rows: Dimension.Create(matrix.RowCount)), None, true));
        return Try.lift(() => {
                Matrix<double> a = ToMathNetSparse(s: matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                int seen = 0;
                MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double> iterator = new([
                    new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, _) => {
                        seen = iteration;
                        return MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Continue;
                    }),
                    new MathNet.Numerics.LinearAlgebra.Solvers.FailureStopCriterion<double>(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.DivergenceStopCriterion<double>(maximumRelativeIncrease: BiCgStabDivergenceFactor, minimumIterations: KrylovPolicy.DivergenceWarmup),
                    new MathNet.Numerics.LinearAlgebra.Solvers.ResidualStopCriterion<double>(maximum: active.Tolerance.Value, minimumIterationsBelowMaximum: KrylovPolicy.ResidualConfirmations),
                    .. active.Stop.Map(static halt => (MathNet.Numerics.LinearAlgebra.Solvers.IIterationStopCriterion<double>)
                        new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, residual) =>
                            halt(iteration, residual.L2Norm())
                                ? MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.StoppedWithoutConvergence
                                : MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Continue)).ToSeq(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.IterationCountStopCriterion<double>(maximumNumberOfIterations: active.Budget.Value),
                ]);
                LinearVector iterate = DenseVectorD.Create(a.ColumnCount, 0.0);
                MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus status = a.TrySolveIterative(
                    b, iterate, active.Solver.Create(), iterator, active.Preconditioner.Create());
                double iterativeResidual = RelativeResidual(a: a, x: iterate, b: b);
                bool converged = status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged
                    && double.IsFinite(iterativeResidual) && iterativeResidual <= active.Tolerance.Value;
                PathEvidence evidence = new PathEvidence.Iterative(Iterations: seen, Budget: active.Budget, Tolerance: active.Tolerance.Value, Plan: Some((Preconditioner: active.Preconditioner, Solver: active.Solver)));
                return converged
                    ? SolveSuccess(solution: new Arr<double>(iterate.ToArray()), path: SolvePath.SparseKrylov,
                        rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount), rhsLength: rhs.Count,
                        residual: iterativeResidual, evidence: evidence, tolerance: active.Tolerance.Value)
                    : active.CanFallback
                        ? DenseFallback(matrix: matrix, a: a, b: b, rhs: rhs)
                        : Fin.Fail<LinearSolution>(new KernelFault.IterationLimit());
            }).Run().Bind(static inner => inner);
    }
    private static Fin<LinearSolution> DenseFallback(CSparse.Storage.CompressedColumnStorage<double> matrix, Matrix<double> a, LinearVector b, Arr<double> rhs) {
        LinearVector x = a.Solve(b);
        double residual = RelativeResidual(a: a, x: x, b: b);
        return !double.IsFinite(residual) || residual > SolvePath.DenseFallback.DefaultTolerance
            ? Fin.Fail<LinearSolution>(new KernelFault.ResidualExceeded())
            : SolveSuccess(solution: new Arr<double>(x.ToArray()), path: SolvePath.DenseFallback,
            rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount), rhsLength: rhs.Count, residual: residual,
            evidence: new PathEvidence.Direct(), tolerance: SolvePath.DenseFallback.DefaultTolerance);
    }
    internal static Fin<LinearSolution> CholeskySparseSolve(CholeskySparse factor, Arr<double> rhs) =>
        !factor.IsValid || !RhsFits(rows: factor.Order.Value, rhs: rhs)
            ? Fin.Fail<LinearSolution>(error: new KernelFault.InvalidInput())
            : Try.lift(() => {
                double[] b = [.. rhs.AsIterable()];
                double[] x = new double[factor.Order.Value];
                factor.SolveGuarded(b: b, x: x);
                Arr<double> solution = new(x);
                return SparseSymmetricResidual(matrix: factor.Source, solution: solution, rhs: rhs).Bind(residual =>
                    SolveSuccess(solution: solution, path: SolvePath.SparseCholesky,
                        rows: Dimension.Create(factor.Source.RowCount), cols: Dimension.Create(factor.Source.ColumnCount), rhsLength: rhs.Count,
                        residual: residual, evidence: new PathEvidence.Factored(FactorNonZeros: factor.FactorNonZeros), tolerance: SolvePath.SparseCholesky.DefaultTolerance));
            }).Run().Bind(static inner => inner);
    internal static Fin<LinearSolution> SparseLuSolve(CSparse.Storage.CompressedColumnStorage<double> matrix, Arr<double> rhs, OperatorSense sense, double pivotTolerance, Option<IProgress<double>> progress) =>
        matrix.RowCount != matrix.ColumnCount || !RhsFits(rows: matrix.RowCount, rhs: rhs)
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                int n = matrix.RowCount;
                CSparse.Storage.CompressedColumnStorage<double> csc = matrix;
                CSparse.Double.Factorization.SparseLU lu = progress.Match(
                    Some: report => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance, progress: report),
                    None: () => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance));
                double[] solution = new double[n];
                sense.SolveLu(factor: lu, rhs: [.. rhs.AsIterable()], solution: solution);
                double residual = SensedResidual(operand: csc, sense: sense, solution: solution, rhs: rhs);
                return !double.IsFinite(residual) || residual > SolvePath.SparseLu.DefaultTolerance
                    ? Fin.Fail<LinearSolution>(new KernelFault.ResidualExceeded())
                    : SolveSuccess(solution: new Arr<double>(solution), path: SolvePath.SparseLu,
                    rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount), rhsLength: rhs.Count, residual: residual,
                    evidence: new PathEvidence.Factored(FactorNonZeros: lu.NonZerosCount), tolerance: SolvePath.SparseLu.DefaultTolerance, sense: Some(sense));
            }).Run().Bind(static inner => inner);
    internal static Fin<LinearSolution> SparseQrSolve(CSparse.Storage.CompressedColumnStorage<double> matrix, Arr<double> rhs, OperatorSense sense, Option<IProgress<double>> progress) {
        (Dimension sensedRows, Dimension sensedCols) = sense.Shape(rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount));
        return rhs.Count != sensedRows.Value || !TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan())
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                CSparse.Storage.CompressedColumnStorage<double> csc = matrix;
                CSparse.Double.Factorization.SparseQR qr = progress.Match(
                    Some: report => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA, progress: report),
                    None: () => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA));
                using SpanOwner<double> work = SpanOwner<double>.Allocate(size: Math.Max(val1: matrix.RowCount, val2: matrix.ColumnCount), mode: AllocationMode.Clear);
                double[] staged = work.DangerousGetArray().Array!;
                sense.SolveQr(factor: qr, rhs: [.. rhs.AsIterable()], solution: staged);
                double[] solution = staged[..sensedCols.Value];
                double[] residualVector = [.. rhs.AsIterable()];
                sense.Accumulate(operand: csc, alpha: 1.0, x: solution, beta: -1.0, y: residualVector);
                using SpanOwner<double> normal = SpanOwner<double>.Allocate(size: sensedCols.Value, mode: AllocationMode.Clear);
                double[] projected = normal.DangerousGetArray().Array!;
                sense.Flipped().Apply(operand: csc, x: residualVector, y: projected);
                double operatorScale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: TensorPrimitives.Norm<double>(matrix.Values.AsSpan()));
                double residual = TensorPrimitives.Norm<double>(projected.AsSpan(start: 0, length: sensedCols.Value))
                    / (operatorScale * Math.Max(val1: TensorPrimitives.Norm<double>(rhs.AsSpan()), val2: EpsilonPolicy.SqrtEpsilon));
                bool admitted = double.IsFinite(residual) && residual <= SolvePath.SparseQr.DefaultTolerance;
                return !admitted
                    ? Fin.Fail<LinearSolution>(new KernelFault.RankDeficient())
                    : SolveSuccess(solution: new Arr<double>(solution), path: SolvePath.SparseQr,
                    rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount),
                    rhsLength: rhs.Count, residual: residual,
                    evidence: new PathEvidence.Ranked(Rank: admitted ? Some(sensedCols.Value) : None, Columns: sensedCols.Value, FactorNonZeros: qr.NonZerosCount),
                    tolerance: SolvePath.SparseQr.DefaultTolerance, sense: Some(sense));
            }).Run().Bind(static inner => inner);
    }

    // --- [SINGULAR_GAUGE] --------------------------------------------------------------
    internal static Fin<LinearSolution> SingularGaugeSolve(CSparse.Storage.CompressedColumnStorage<double> matrix, Arr<double> rhs, GaugePolicy gauge, Context context) =>
        matrix.RowCount != matrix.ColumnCount || !RhsFits(rows: matrix.RowCount, rhs: rhs) || !GaugeFits(gauge: gauge, dimension: matrix.RowCount)
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : from upper in SymmetricUpper(s: matrix)
              from result in Try.lift(() => {
                  int n = matrix.RowCount;
                  Matrix<double> aSym = ToMathNetSymmetric(matrix: matrix, upper: upper);
                  LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                  double operatorScale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: aSym.FrobeniusNorm());
                  var state = gauge.Switch(
                      state: (Dimension: n, Rhs: b),
                      pin: static (s, pin) => (
                          Mass: pin.Mass,
                          Nullspace: DenseMatrixD.OfColumnVectors([.. pin.Indices.AsIterable()
                              .Select(index => DenseVectorD.Create(s.Dimension, i => i == index ? 1.0 : 0.0))]),
                          Shift: pin.PostShift,
                          PinIndices: pin.Indices, Compatibility: 0.0, ProjectRhs: false),
                      meanZeroDeflation: static (s, policy) => {
                          Matrix<double> basis = BasisColumns(policy.Nullspace);
                          return (Mass: policy.Mass, Nullspace: basis, Shift: policy.PostShift,
                              PinIndices: new Arr<int>([]), Compatibility: basis.TransposeThisAndMultiply(s.Rhs).L2Norm(), ProjectRhs: true);
                      },
                      lagrangeKKT: static (s, policy) => {
                          Matrix<double> basis = BasisColumns(policy.Nullspace);
                          return (Mass: policy.Mass, Nullspace: basis, Shift: policy.PostShift,
                              PinIndices: new Arr<int>([]), Compatibility: basis.TransposeThisAndMultiply(s.Rhs).L2Norm(), ProjectRhs: false);
                      });
                  Matrix<double> mass = state.Mass.Match(
                      Some: diagonal => (Matrix<double>)DenseMatrixD.OfDiagonalVector(DenseVectorD.OfArray([.. diagonal.AsIterable()])),
                      None: () => DenseMatrixD.CreateIdentity(order: n));
                  int nullspaceDim = state.Nullspace.ColumnCount;
                  bool projectRhs = state.ProjectRhs
                      && state.Compatibility > context.For(lane: ToleranceLane.Kkt).Value * Math.Max(val1: 1.0, val2: b.InfinityNorm());
                  LinearVector rhsGauged = projectRhs ? DeflateRhs(nullspace: state.Nullspace, mass: mass, b: b) : b;
                  double rhsMutation = (rhsGauged - b).L2Norm();
                  return gauge.Switch(
                      state: (Matrix: matrix, Upper: upper, ASym: aSym, Mass: mass, Nullspace: state.Nullspace, Rhs: rhsGauged),
                      pin: static (s, p) => SolvePin(matrix: s.Matrix, upper: s.Upper, aSym: s.ASym, pin: p, b: s.Rhs),
                      meanZeroDeflation: static (s, _) => SolveDeflated(matrix: s.Matrix, aSym: s.ASym, mass: s.Mass, nullspace: s.Nullspace, b: s.Rhs),
                      lagrangeKKT: static (s, _) => SolveKkt(upper: s.Upper, aSym: s.ASym, massNullspace: s.Mass.Multiply(s.Nullspace), b: s.Rhs))
                  .Bind(stage => {
                      LinearVector shifted = state.Shift.Apply(mass: mass, x: stage.X);
                      double relative = BackwardError(a: aSym, x: shifted, b: b, operatorScale: operatorScale);
                      GaugeFix fix = new(
                          Nullity: nullspaceDim, BasisRank: stage.BasisRank,
                          OperatorScale: operatorScale, CompatibilityResidual: state.Compatibility, GaugeResidual: stage.Residual,
                          MassResidual: MassResidual(a: aSym, mass: mass, x: shifted, b: b),
                          PinIndices: state.PinIndices, Shift: state.Shift,
                          RhsMutationNorm: rhsMutation, MultiplierNorm: stage.MultiplierNorm,
                          GaugeOrthogonality: state.Nullspace.TransposeThisAndMultiply(mass.Multiply(shifted)).L2Norm() / Math.Max(val1: 1.0, val2: shifted.L2Norm()),
                          Regularization: stage.Regularization);
                      return relative > context.For(lane: ToleranceLane.Residual).Value
                          ? Fin.Fail<LinearSolution>(new KernelFault.ResidualExceeded())
                          : SolveSuccess(solution: new Arr<double>(shifted.ToArray()), path: stage.Path,
                          rows: Dimension.Create(matrix.RowCount), cols: Dimension.Create(matrix.ColumnCount), rhsLength: rhs.Count, residual: relative,
                          evidence: stage.Evidence, tolerance: context.For(lane: ToleranceLane.Residual).Value, gauge: Some(fix));
                  });
              }).Run().Bind(static inner => inner)
              select result;
    private static bool GaugeFits(GaugePolicy gauge, int dimension) =>
        gauge.Switch(
            state: dimension,
            pin: static (dim, p) => p.Indices.Count >= 1 && p.Indices.Count == p.Values.Count && p.Indices.ForAll(index => index >= 0 && index < dim) && MassFits(mass: p.Mass, dimension: dim),
            meanZeroDeflation: static (dim, d) => BasisFits(basis: d.Nullspace, dimension: dim) && MassFits(mass: d.Mass, dimension: dim),
            lagrangeKKT: static (dim, k) => BasisFits(basis: k.Nullspace, dimension: dim) && MassFits(mass: k.Mass, dimension: dim));
    private static bool BasisFits(Arr<Arr<double>> basis, int dimension) =>
        basis.Count >= 1 && basis.Count < dimension && basis.ForAll(column => column.Count == dimension && TensorPrimitives.IsFiniteAll<double>(column.AsSpan()));
    private static bool MassFits(Option<Arr<double>> mass, int dimension) =>
        mass.Map(diagonal => diagonal.Count == dimension && diagonal.ForAll(static value => double.IsFinite(value) && value > 0.0)).IfNone(noneValue: true);
    private static Matrix<double> BasisColumns(Arr<Arr<double>> basis) =>
        DenseMatrixD.OfColumnVectors([.. basis.AsIterable().Select(column => DenseVectorD.OfArray([.. column.AsIterable()]))]);
    private static (LinearVector Coords, double Shift, int NumericRank) RegularizedGramSolve(Matrix<double> gram, LinearVector rhs) {
        double floor = EpsilonPolicy.SqrtEpsilon * Math.Max(val1: EpsilonPolicy.SqrtEpsilon,
            val2: gram.Diagonal().Enumerate().Aggregate(0.0, static (acc, value) => Math.Max(acc, Math.Abs(value))));
        (MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor, double shift) =
            Try.lift(() => gram.Cholesky()).Run().ToOption().Match(
                Some: chol => (Factor: chol, Shift: 0.0),
                None: () => (Factor: (gram + (DenseMatrixD.CreateIdentity(order: gram.RowCount) * floor)).Cholesky(), Shift: floor));
        return (Coords: factor.Solve(rhs), shift, NumericRank: factor.Factor.Diagonal().Enumerate().Count(value => value * value > floor));
    }
    private static (LinearVector Projected, double Shift, int NumericRank) ProjectRange(Matrix<double> nullspace, Matrix<double> mass, LinearVector x) {
        Matrix<double> gram = nullspace.TransposeThisAndMultiply(mass.Multiply(nullspace));
        (LinearVector coords, double shift, int numericRank) = RegularizedGramSolve(gram: gram, rhs: nullspace.TransposeThisAndMultiply(mass.Multiply(x)));
        return (Projected: x - (nullspace * coords), Shift: shift, NumericRank: numericRank);
    }
    private static LinearVector DeflateRhs(Matrix<double> nullspace, Matrix<double> mass, LinearVector b) {
        Matrix<double> massNullspace = mass.Multiply(nullspace);
        (LinearVector coords, _, _) = RegularizedGramSolve(gram: nullspace.TransposeThisAndMultiply(massNullspace), rhs: nullspace.TransposeThisAndMultiply(b));
        return b - (massNullspace * coords);
    }
    private static double MassResidual(Matrix<double> a, Matrix<double> mass, LinearVector x, LinearVector b) {
        LinearVector residual = b - a.Multiply(x);
        return Math.Sqrt(residual.DotProduct(mass.Multiply(residual))) / Math.Max(val1: 1.0, val2: Math.Sqrt(b.DotProduct(mass.Multiply(b))));
    }
    private static Fin<GaugeStage> SolvePin(CSparse.Storage.CompressedColumnStorage<double> matrix, List<(int Row, int Col, double Value)> upper, Matrix<double> aSym, GaugePolicy.Pin pin, LinearVector b) {
        int n = matrix.RowCount;
        bool[] pinned = new bool[n];
        double[] pinValues = new double[n];
        for (int i = 0; i < pin.Indices.Count; i++) { pinned[pin.Indices[i]] = true; pinValues[pin.Indices[i]] = pin.Values[i]; }
        int[] remap = new int[n];
        int free = 0;
        for (int i = 0; i < n; i++) remap[i] = pinned[i] ? -1 : free++;
        if (free == 0) return Fin.Fail<GaugeStage>(new KernelFault.InvalidInput());
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
        return from reducedMatrix in Sparse(rows: dim, cols: dim, triplets: filtered)
               from factor in CholeskySparse.Of(symmetric: reducedMatrix)
               from solved in factor.SolveDetailed(rhs: new Arr<double>(reduced))
               let reassembled = DenseVectorD.Create(n, i => pinned[i] ? pinValues[i] : solved.Solution[remap[i]])
               select new GaugeStage(X: reassembled, Residual: RelativeResidual(a: aSym, x: reassembled, b: b),
                   Path: SolvePath.SparseCholesky, Evidence: new PathEvidence.Factored(FactorNonZeros: factor.FactorNonZeros),
                   MultiplierNorm: None, Regularization: 0.0);
    }
    private static Fin<GaugeStage> SolveDeflated(CSparse.Storage.CompressedColumnStorage<double> matrix, Matrix<double> aSym, Matrix<double> mass, Matrix<double> nullspace, LinearVector b) =>
        SparseSolve(matrix: matrix, rhs: new Arr<double>(b.ToArray()), policy: None).Map(solved => {
            (LinearVector projected, double shift, int numericRank) = ProjectRange(nullspace: nullspace, mass: mass,
                x: DenseVectorD.OfArray([.. solved.Solution.AsIterable()]));
            return new GaugeStage(X: projected, Residual: RelativeResidual(a: aSym, x: projected, b: b),
                Path: solved.Path, Evidence: solved.Evidence, MultiplierNorm: None, Regularization: shift,
                BasisRank: Some(numericRank));
        });
    private static Fin<GaugeStage> SolveKkt(List<(int Row, int Col, double Value)> upper, Matrix<double> aSym, Matrix<double> massNullspace, LinearVector b) {
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
        CSparse.Storage.CompressedColumnStorage<double> saddle = CSparse.Double.SparseMatrix.OfIndexed(rows: total, columns: total, enumerable: entries);
        return Saddle(saddle: saddle, rhs: rhs, aSym: aSym, b: b, n: n, path: SolvePath.SparseLdl)
            .BindFail(_ => Saddle(saddle: saddle, rhs: rhs, aSym: aSym, b: b, n: n, path: SolvePath.SparseLu));
    }
    private static Fin<GaugeStage> Saddle(CSparse.Storage.CompressedColumnStorage<double> saddle, double[] rhs, Matrix<double> aSym, LinearVector b, int n, SolvePath path) =>
        Try.lift(() => {
            double[] solution = new double[saddle.RowCount];
            CSparse.ISolver<double> factor = path.Equals(SolvePath.SparseLdl)
                ? CSparse.Double.Factorization.SparseLDL.Create(A: saddle, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA)
                : CSparse.Double.Factorization.SparseLU.Create(A: saddle, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: 1.0);
            factor.Solve(input: rhs.AsSpan(), result: solution.AsSpan());
            LinearVector x = DenseVectorD.OfArray([.. solution.Take(count: n)]);
            double residual = (b - aSym.Multiply(x)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
            return double.IsFinite(residual)
                ? Fin.Succ(new GaugeStage(X: x, Residual: residual, Path: path,
                    Evidence: new PathEvidence.Factored(FactorNonZeros: ((CSparse.Factorization.ISparseFactorization<double>)factor).NonZerosCount),
                    MultiplierNorm: Some(DenseVectorD.OfArray([.. solution.Skip(count: n)]).L2Norm()), Regularization: 0.0))
                : Fin.Fail<GaugeStage>(new KernelFault.InvalidResult(Detail: Some($"kkt residual non-finite on {path.Key}")));
        }).Run().Bind(static inner => inner);
    private readonly record struct GaugeStage(LinearVector X, double Residual, SolvePath Path,
        PathEvidence Evidence, Option<double> MultiplierNorm, double Regularization, Option<int> BasisRank = default);

    // --- [GENERALIZED_EIGEN] -----------------------------------------------------------
    internal static Fin<EigenSolution<double, Arr<double>>> GeneralizedEigenpairs(CSparse.Storage.CompressedColumnStorage<double> stiffness, CSparse.Storage.CompressedColumnStorage<double> mass, int k) =>
        stiffness.RowCount != stiffness.ColumnCount || mass.RowCount != mass.ColumnCount || stiffness.RowCount != mass.RowCount || k < 1 || k >= stiffness.RowCount
            ? Fin.Fail<EigenSolution<double, Arr<double>>>(new KernelFault.InvalidInput())
            : from stiffnessUpper in SymmetricUpper(s: stiffness)
              from massUpper in SymmetricUpper(s: mass)
              from solved in Try.lift(() => {
                  Matrix<double> stiffnessSource = ToMathNetSymmetric(matrix: stiffness, upper: stiffnessUpper);
                  Matrix<double> stiffnessM = DenseMatrixD.Build.DenseOfRowMajor(stiffnessSource.RowCount, stiffnessSource.ColumnCount, stiffnessSource.ToRowMajorArray());
                  Matrix<double> massSource = ToMathNetSymmetric(matrix: mass, upper: massUpper);
                  Matrix<double> massM = DenseMatrixD.Build.DenseOfRowMajor(massSource.RowCount, massSource.ColumnCount, massSource.ToRowMajorArray());
                  (LinearVector vals, Matrix<double> vecs, int factorNonZeros) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                  Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                      .OrderBy(i => vals[i]).Take(k)
                      .Select(i => (Eigenvalue: vals[i], Eigenvector: new Arr<double>(vecs.Column(i).ToArray()))));
                  return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseCongruence,
                      requestedPairs: k, maxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs),
                      evidence: new PathEvidence.Factored(FactorNonZeros: factorNonZeros));
              }).Run().Bind(static inner => inner)
              select solved;
    private static (LinearVector Vals, Matrix<double> Vecs, int FactorNonZeros) SolveGeneralised(Matrix<double> Ahat, Matrix<double> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> cholesky = Mhat.Cholesky();
        Matrix<double> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixD.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.Transpose());
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = ((reduced + reduced.Transpose()) * 0.5).Evd(Symmetricity.Symmetric);
        return (
            Vals: DenseVectorD.Create(evd.EigenValues.Count, i => evd.EigenValues[i].Real),
            Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.Transpose()),
            FactorNonZeros: cholesky.Factor.Enumerate(Zeros.AllowSkip).Count(static value => Math.Abs(value: value) > EpsilonPolicy.ZeroTolerance));
    }
    private static (ComplexVector Vals, Matrix<Complex> Vecs) SolveGeneralisedComplex(Matrix<Complex> Ahat, Matrix<Complex> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<Complex> cholesky = Mhat.Cholesky();
        Matrix<Complex> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixC.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.ConjugateTranspose());
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = ((reduced + reduced.ConjugateTranspose()) * 0.5).Evd(Symmetricity.Hermitian);
        return (Vals: evd.EigenValues, Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.ConjugateTranspose()));
    }
    private static Matrix<T> CongruentReduce<T>(Matrix<T> factor, Matrix<T> matrix, Matrix<T> identity, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        factor.Solve(matrix * adjoint(arg: factor).Solve(identity));
    private static Matrix<T> BackTransform<T>(Matrix<T> factor, Matrix<T> vectors, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        adjoint(arg: factor).Solve(vectors);

    // --- [LOBPCG] ----------------------------------------------------------------------
    private delegate T BasisSample<T>(ref ulong state);
    internal static Fin<EigenSolution<double, Arr<double>>> Lobpcg(CSparse.Storage.CompressedColumnStorage<double> matrix, int k, double tolerance, Dimension budget) =>
        matrix.RowCount != matrix.ColumnCount || k < 1 || k >= matrix.RowCount || !double.IsFinite(tolerance) || tolerance <= 0
            ? Fin.Fail<EigenSolution<double, Arr<double>>>(new KernelFault.InvalidInput())
            : SymmetricUpper(s: matrix).Bind(upper => {
                Matrix<double> a = ToMathNetSymmetric(matrix: matrix, upper: upper);
                return LobpcgCore(A: a, X: OrthonormalRandom(rows: matrix.RowCount, k: k, lane: MatrixDrawLane.RealBasis, sample: static (ref ulong s) => Deterministic.NextSignedUnit(state: ref s), orthonormalise: Orthonormalise), P: DenseMatrixD.Create(matrix.RowCount, k, 0.0), jacobi: DiagonalInverse(a), k: k, tolerance: tolerance, budget: budget, path: EigenSolvePath.SparseLobpcg, rayleigh: Rayleigh, diagonal: DenseMatrixD.OfDiagonalVector, adjoint: static m => m.Transpose(), orthonormalise: Orthonormalise, solveGeneralised: static (Ahat, Mhat) => { (LinearVector Vals, Matrix<double> Vecs, int _) = SolveGeneralised(Ahat: Ahat, Mhat: Mhat); return (Vals, Vecs); }, eigenvalue: static value => value, vector: static v => new Arr<double>(v.ToArray()), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector));
            });
    internal static Fin<EigenSolution<double, Arr<Complex>>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, Dimension budget) =>
        k < 1 || k >= matrix.Order.Value || !double.IsFinite(tolerance) || tolerance <= 0
            ? Fin.Fail<EigenSolution<double, Arr<Complex>>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                Matrix<Complex> a = ToMathNetHermitian(matrix);
                return LobpcgCore(A: a, X: OrthonormalRandom(rows: matrix.Order.Value, k: k, lane: MatrixDrawLane.HermitianBasis, sample: static (ref ulong s) => Deterministic.NextSignedComplexUnit(state: ref s), orthonormalise: OrthonormaliseComplex), P: DenseMatrixC.Create(matrix.Order.Value, k, Complex.Zero), jacobi: DiagonalInverseComplex(a), k: k, tolerance: tolerance, budget: budget, path: EigenSolvePath.HermitianLobpcg, rayleigh: RayleighComplex, diagonal: DenseMatrixC.OfDiagonalVector, adjoint: static m => m.ConjugateTranspose(), orthonormalise: OrthonormaliseComplex, solveGeneralised: static (Ahat, Mhat) => SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat), eigenvalue: static value => value.Real, vector: static v => new Arr<Complex>(v.ToArray()), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue));
            }).Run().Bind(static inner => inner);
    private static Fin<EigenSolution<double, TVector>> LobpcgCore<T, TVector>(Matrix<T> A, Matrix<T> X, Matrix<T> P, MathNet.Numerics.LinearAlgebra.Vector<T> jacobi, int k, double tolerance, Dimension budget, EigenSolvePath path, Func<Matrix<T>, Matrix<T>, MathNet.Numerics.LinearAlgebra.Vector<T>> rayleigh, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, Matrix<T>> diagonal, Func<Matrix<T>, Matrix<T>> adjoint, Func<Matrix<T>, Matrix<T>> orthonormalise, Func<Matrix<T>, Matrix<T>, (MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solveGeneralised, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector, Func<Matrix<T>, Seq<(double Eigenvalue, TVector Eigenvector)>, double> residual)
        where T : struct, IEquatable<T>, IFormattable {
        int n = A.RowCount;
        return Iterate(iter: 0, X: X, P: P);
        Fin<EigenSolution<double, TVector>> Iterate(int iter, Matrix<T> X, Matrix<T> P) =>
            iter >= budget.Value
                ? Fin.Fail<EigenSolution<double, TVector>>(new KernelFault.IterationLimit())
                : Step(iter: iter, X: X, P: P);
        Fin<EigenSolution<double, TVector>> Step(int iter, Matrix<T> X, Matrix<T> P) {
            Matrix<T> AX = A * X;
            MathNet.Numerics.LinearAlgebra.Vector<T> lambda = rayleigh(arg1: X, arg2: AX);
            Matrix<T> R = AX - (X * diagonal(arg: lambda));
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: lambda, X: X);
            return residual(arg1: A, arg2: pairs) < tolerance
                ? Solved(iter: iter, X: X)
                : Continue(iter: iter, X: X, P: P, R: R);
        }
        Fin<EigenSolution<double, TVector>> Continue(int iter, Matrix<T> X, Matrix<T> P, Matrix<T> R) {
            Matrix<T> W = ApplyJacobi(R: R, invDiag: jacobi);
            bool hasPrevious = iter > 0 && Enumerable.Range(0, P.ColumnCount).Any(j => P.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon);
            Matrix<T> S = orthonormalise(arg: hasPrevious ? X.Append(W).Append(P) : X.Append(W));
            int[] survivors = [.. Enumerable.Range(0, S.ColumnCount).Where(j => S.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon)];
            if (survivors.Length < k) return Fin.Fail<EigenSolution<double, TVector>>(new KernelFault.RankDeficient());
            Matrix<T> Sr = Matrix<T>.Build.DenseOfColumnVectors([.. survivors.Select(S.Column)]);
            Matrix<T> STr = adjoint(arg: Sr);
            return Try.lift(() => solveGeneralised(arg1: STr * (A * Sr), arg2: STr * Sr)).Run().Bind(solution => {
                Matrix<T> Z = ScatterRows(reduced: Matrix<T>.Build.DenseOfColumnVectors([
                    .. Enumerable.Range(start: 0, count: solution.Vals.Count)
                        .OrderBy(i => eigenvalue(arg: solution.Vals[i]))
                        .Take(count: k)
                        .Select(solution.Vecs.Column)]), rows: S.ColumnCount, sourceRows: survivors);
                Matrix<T> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : Matrix<T>.Build.Dense(n, k);
                return Iterate(iter: iter + 1, X: orthonormalise(arg: S * Z), P: (W * Z.SubMatrix(k, k, 0, k)) + previous);
            });
        }
        Fin<EigenSolution<double, TVector>> Solved(int iter, Matrix<T> X) {
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: rayleigh(arg1: X, arg2: A * X), X: X);
            return EigenSolutionOf(pairs: pairs, path: path, requestedPairs: k,
                maxResidual: residual(arg1: A, arg2: pairs),
                evidence: new PathEvidence.Iterative(Iterations: iter, Budget: budget, Tolerance: tolerance, Plan: None));
        }
        Seq<(double Eigenvalue, TVector Eigenvector)> Pairs(MathNet.Numerics.LinearAlgebra.Vector<T> lambda, Matrix<T> X) =>
            toSeq(Enumerable.Range(start: 0, count: k).Select(i => (Eigenvalue: eigenvalue(arg: lambda[i]), Eigenvector: vector(arg: X.Column(i)))).OrderBy(static p => p.Eigenvalue));
    }
    private static Matrix<T> OrthonormalRandom<T>(int rows, int k, MatrixDrawLane lane, BasisSample<T> sample, Func<Matrix<T>, Matrix<T>> orthonormalise)
        where T : struct, IEquatable<T>, IFormattable {
        ulong state = Deterministic.Of(seed: 0L, lane: lane).State;
        return orthonormalise(arg: Matrix<T>.Build.Dense(rows, k, (_, _) => sample(state: ref state)));
    }
    private static Matrix<double> Orthonormalise(Matrix<double> m) =>
        Orthonormalise(m: m, zero: 0.0, inner: static (basis, value) => basis.DotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<Complex> OrthonormaliseComplex(Matrix<Complex> m) =>
        Orthonormalise(m: m, zero: Complex.Zero, inner: static (basis, value) => basis.ConjugateDotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<T> Orthonormalise<T>(Matrix<T> m, T zero, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T> inner, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T, MathNet.Numerics.LinearAlgebra.Vector<T>> remove, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, double, MathNet.Numerics.LinearAlgebra.Vector<T>> normalise)
        where T : struct, IEquatable<T>, IFormattable {
        Matrix<T> q = Matrix<T>.Build.Dense(rows: m.RowCount, columns: m.ColumnCount, value: zero);
        for (int j = 0; j < m.ColumnCount; j++) {
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
    private static Matrix<T> ScatterRows<T>(Matrix<T> reduced, int rows, int[] sourceRows)
        where T : struct, IEquatable<T>, IFormattable {
        Matrix<T> full = Matrix<T>.Build.Dense(rows: rows, columns: reduced.ColumnCount);
        for (int i = 0; i < sourceRows.Length; i++) full.SetRow(rowIndex: sourceRows[i], row: reduced.Row(i));
        return full;
    }
    private static LinearVector DiagonalInverse(Matrix<double> a) =>
        DenseVectorD.Create(a.RowCount, i => Math.Abs(a[i, i]) > EpsilonPolicy.SqrtEpsilon ? 1.0 / a[i, i] : 1.0);
    private static ComplexVector DiagonalInverseComplex(Matrix<Complex> a) =>
        DenseVectorC.Create(a.RowCount, i => Complex.Abs(a[i, i]) > EpsilonPolicy.SqrtEpsilon ? Complex.One / a[i, i] : Complex.One);
    private static LinearVector Rayleigh(Matrix<double> X, Matrix<double> AX) =>
        DenseVectorD.Create(X.ColumnCount, j => X.Column(j).DotProduct(AX.Column(j)) / Math.Max(X.Column(j).DotProduct(X.Column(j)), EpsilonPolicy.ZeroTolerance));
    private static ComplexVector RayleighComplex(Matrix<Complex> X, Matrix<Complex> AX) =>
        DenseVectorC.Create(X.ColumnCount, j => X.Column(j).ConjugateDotProduct(AX.Column(j))
            / (X.Column(j).ConjugateDotProduct(X.Column(j)) switch {
                Complex den when Complex.Abs(den) > EpsilonPolicy.ZeroTolerance => den,
                _ => new Complex(EpsilonPolicy.ZeroTolerance, 0.0),
            }));
    private static Matrix<T> ApplyJacobi<T>(Matrix<T> R, MathNet.Numerics.LinearAlgebra.Vector<T> invDiag)
        where T : struct, IEquatable<T>, IFormattable {
        Matrix<T> scaled = R.Clone();
        for (int i = 0; i < R.RowCount; i++) scaled.SetRow(rowIndex: i, row: R.Row(i).Multiply(scalar: invDiag[i]));
        return scaled;
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
