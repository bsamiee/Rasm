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
[SmartEnum<string>]
public sealed partial class SolveTrait : ICapability<SolveTrait> {
    public static readonly SolveTrait Direct = new(key: "direct", rank: 0);
    public static readonly SolveTrait Iterative = new(key: "iterative", rank: 1);
    public static readonly SolveTrait Sparse = new(key: "sparse", rank: 2);
    public static readonly SolveTrait Square = new(key: "square", rank: 3);
    public static readonly SolveTrait Transposed = new(key: "transposed", rank: 4);
    public static readonly SolveTrait Complex = new(key: "complex", rank: 5);
    public static readonly SolveTrait Fallback = new(key: "fallback", rank: 6);
    public int Rank { get; }
    private static readonly CapabilityLaw<SolveTrait> Routes = new(Legal: Seq(
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Square),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Transposed),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Transposed),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Fallback),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Square),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct, SolveTrait.Complex),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Iterative, SolveTrait.Sparse),
        CapabilitySet<SolveTrait>.Of(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Complex)));
    internal static CapabilitySet<SolveTrait> Admit(params ReadOnlySpan<SolveTrait> held) =>
        Routes.Admit(held: CapabilitySet<SolveTrait>.Of(held)).ThrowIfFail();
}

[SmartEnum]
public sealed partial class ResidualCap {
    public static readonly ResidualCap Converged = new(floor: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
    public static readonly ResidualCap Relaxed = new(floor: Math.Sqrt(d: EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public double Floor { get; }
    public ToleranceLane Lane { get; }
    public double In(Option<Context> context) => context.Map(model => model.For(lane: Lane).Value).IfNone(noneValue: Floor);
}

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

public readonly record struct KrylovPolicy(
    SparsePreconditioner Preconditioner, KrylovSolver Solver, double Tolerance, Dimension Budget,
    Option<KrylovStop> Stop, bool CanFallback) {
    public static Fin<KrylovPolicy> Of(SparsePreconditioner preconditioner, double tolerance, Dimension budget,
        Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, bool canFallback = false) =>
        from _ in Admit.Finite(value: tolerance)
        from gated in guard(tolerance > 0.0, new KernelFault.InvalidInput())
        select new KrylovPolicy(Preconditioner: preconditioner, Solver: solver.IfNone(noneValue: KrylovSolver.BiCgStab),
            Tolerance: tolerance, Budget: budget, Stop: stop, CanFallback: canFallback);
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

public readonly record struct KrylovPlan(SparsePreconditioner Preconditioner, KrylovSolver Solver);

[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Square), cap: ResidualCap.Relaxed, conditioned: static () => DenseQr);
    public static readonly SolvePath DenseCholesky = new(key: 1,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Square), cap: ResidualCap.Relaxed, conditioned: static () => DenseLu);
    public static readonly SolvePath DenseQr = new(key: 2,
        traits: SolveTrait.Admit(SolveTrait.Direct), cap: ResidualCap.Relaxed, conditioned: static () => DenseQr);
    public static readonly SolvePath SparseCholesky = new(key: 3,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Relaxed, conditioned: static () => SparseLdl);
    public static readonly SolvePath SparseLdl = new(key: 4,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Relaxed, conditioned: static () => SparseLu);
    public static readonly SolvePath SparseLu = new(key: 5,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Transposed), cap: ResidualCap.Relaxed, conditioned: static () => SparseLu);
    public static readonly SolvePath SparseQr = new(key: 6,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Transposed), cap: ResidualCap.Relaxed, conditioned: static () => SparseQr);
    public static readonly SolvePath SparseKrylov = new(key: 7,
        traits: SolveTrait.Admit(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Converged, conditioned: static () => DenseFallback);
    public static readonly SolvePath DenseFallback = new(key: 8,
        traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Fallback), cap: ResidualCap.Relaxed, conditioned: static () => DenseFallback);

    public CapabilitySet<SolveTrait> Traits { get; }
    public ResidualCap Cap { get; }
    [UseDelegateFromConstructor] public partial SolvePath Conditioned();
}

[SmartEnum<int>]
public sealed partial class EigenSolvePath {
    public static readonly EigenSolvePath DenseSymmetric = new(key: 0, traits: SolveTrait.Admit(SolveTrait.Direct));
    public static readonly EigenSolvePath DenseGeneral = new(key: 1, traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Complex));
    public static readonly EigenSolvePath SparseLobpcg = new(key: 2, traits: SolveTrait.Admit(SolveTrait.Iterative, SolveTrait.Sparse));
    public static readonly EigenSolvePath HermitianLobpcg = new(key: 3, traits: SolveTrait.Admit(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Complex));
    public static readonly EigenSolvePath DenseCongruence = new(key: 4, traits: SolveTrait.Admit(SolveTrait.Direct));
    public CapabilitySet<SolveTrait> Traits { get; }
}

[SmartEnum]
public sealed partial class EigenOrder {
    public static readonly EigenOrder DescendingMagnitude = new();
    public static readonly EigenOrder Ascending = new();
    public static readonly EigenOrder Factorization = new();
}

[SmartEnum]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(isUsable: true);
    public static readonly SolveStop LeastSquaresSolved = new(isUsable: true);
    public static readonly SolveStop ResidualConverged = new(isUsable: true);
    public static readonly SolveStop RankDeficient = new(isUsable: false);
    public static readonly SolveStop IterativeExhausted = new(isUsable: false);
    public static readonly SolveStop ResidualRejected = new(isUsable: false);
    public bool IsUsable { get; }
}

[SmartEnum]
public sealed partial class EigenSolveStop {
    public static readonly EigenSolveStop DirectSolved = new(isUsable: true);
    public static readonly EigenSolveStop ResidualConverged = new(isUsable: true);
    public static readonly EigenSolveStop IterativeExhausted = new(isUsable: false);
    public bool IsUsable { get; }
}

[SmartEnum]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(compute: static m => TensorPrimitives.Norm<double>(m.Entries.AsSpan()));
    public static readonly MatrixNormKind MaxAbs = new(compute: static m => m.Entries.Count == 0 ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(m.Entries.AsSpan())));
    public static readonly MatrixNormKind L1 = new(compute: static m => MatrixKernel.ToMathNet(m).L1Norm());
    public static readonly MatrixNormKind LInf = new(compute: static m => MatrixKernel.ToMathNet(m).InfinityNorm());
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
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
public readonly record struct Matrix : IValidityEvidence {
    private Matrix(Dimension rows, Dimension cols, Arr<double> entries) => (Rows, Cols, Entries) = (rows, cols, entries);
    public Dimension Rows { get; }
    public Dimension Cols { get; }
    public Arr<double> Entries { get; }
    public static Fin<Matrix> Of(Dimension rows, Dimension cols, Arr<double> entries) =>
        from _ in guard(entries.Count == rows.Value * cols.Value, new KernelFault.InvalidInput()).ToFin()
        from finite in guard(TensorPrimitives.IsFiniteAll<double>(entries.AsSpan()), new KernelFault.InvalidInput())
        select new Matrix(rows: rows, cols: cols, entries: entries);
    internal static Matrix Trusted(Dimension rows, Dimension cols, Arr<double> entries) => new(rows: rows, cols: cols, entries: entries);
    public static Matrix Identity(Dimension dim) =>
        MatrixKernel.FromMathNet(m: DenseMatrixD.CreateIdentity(order: dim.Value), rows: dim, cols: dim);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: Entries.Count, expected: Rows.Value * Cols.Value),
        ValidityClaim.Finite(Entries.AsSpan()));
    public Matrix Transpose() => MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Transpose(), Cols, Rows);
    public Fin<Matrix> Multiply(Matrix other) =>
        Cols.Value != other.Rows.Value
            ? Fin.Fail<Matrix>(error: new KernelFault.InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: other.Cols, project: left => left.Multiply(MatrixKernel.ToMathNet(other)));
    public Fin<Matrix> Inverse() =>
        Rows.Value != Cols.Value
            ? Fin.Fail<Matrix>(error: new KernelFault.InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: Cols, project: static matrix => matrix.Inverse());
    public Fin<Matrix> PseudoInverse() =>
        MatrixKernel.DenseResult(source: this, rows: Cols, cols: Rows, project: static matrix => matrix.PseudoInverse());
    public Fin<EigenSolution<Complex, Arr<Complex>>> DecomposeEigenDetailed() => MatrixKernel.GeneralEigen(matrix: this);
    public Fin<LuResult> DecomposeLu() => MatrixKernel.Lu(matrix: this);
    public Fin<QrResult> DecomposeQr() => MatrixKernel.Qr(matrix: this);
    public Fin<SvdResult> DecomposeSvd() => MatrixKernel.Svd(matrix: this);
    public Fin<double> Norm(MatrixNormKind kind) => Admit.Finite(value: kind.Compute(matrix: this));
    public Fin<double> Trace() =>
        Rows.Value != Cols.Value ? Fin.Fail<double>(new KernelFault.InvalidInput()) : Admit.Finite(value: MatrixKernel.ToMathNet(this).Trace());
    public Fin<double> Determinant() => MatrixKernel.Determinant(matrix: this);
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.Solve(matrix: this, rhs: rhs);
    public Fin<LinearSolution> LeastSquaresDetailed(Arr<double> rhs) => MatrixKernel.LeastSquares(matrix: this, rhs: rhs);
    internal ReadOnlySpan2D<double> AsPlane() => Entries.AsSpan().AsSpan2D(height: Rows.Value, width: Cols.Value);
    internal double At(int i, int j) => AsPlane()[i, j];
    internal Matrix With(int i, int j, double value) => Trusted(rows: Rows, cols: Cols, entries: Entries.SetItem((i * Cols.Value) + j, value));
}

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
    public Matrix ToDense() => MatrixKernel.Expanded(matrix: this);
    public Fin<EigenSolution<double, Arr<double>>> DecomposeEigenDetailed() => MatrixKernel.SymmetricEigen(matrix: this);
    public Fin<CholeskyResult> DecomposeCholesky() => MatrixKernel.Cholesky(matrix: this);
    public Fin<Unit> Definite() => MatrixKernel.DefiniteSweep(matrix: this);
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        new(dimension: Dimension, upper: Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value));
    internal static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V, int Rank) : IValidityEvidence {
    public Fin<double> Spectral() =>
        Sigma.IsEmpty ? Fin.Fail<double>(new KernelFault.InvalidResult()) : Admit.Finite(value: Sigma[0]);
    public Fin<double> Condition() =>
        Sigma.IsEmpty || Sigma[Sigma.Count - 1] <= EpsilonPolicy.ZeroTolerance
            ? Fin.Fail<double>(new KernelFault.InvalidResult())
            : Admit.Finite(value: Sigma[0] / Sigma[Sigma.Count - 1]);
    public bool IsValid => ValidityClaim.All(
        U.IsValid && V.IsValid,
        Sigma.All(static value => double.IsFinite(value) && value >= 0.0),
        toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: Sigma.Count - 1))).ForAll(i => Sigma[i - 1] >= Sigma[i]),
        ValidityClaim.CountAtLeast(count: Rank, floor: 0));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult : IValidityEvidence {
    internal LuResult(Matrix source, double determinant, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor) { Source = source; Determinant = determinant; Factor = factor; }
    public Matrix Source { get; }
    public double Determinant { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.LU<double> Factor { get; }
    public bool IsValid => ValidityClaim.All(Source.IsValid, ValidityClaim.Finite(value: Determinant));
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.LuSolve(lu: this, rhs: rhs);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct QrResult : IValidityEvidence {
    internal QrResult(Matrix source, Matrix q, Matrix r, bool fullRank, MathNet.Numerics.LinearAlgebra.Factorization.QR<double> factor) { Source = source; Q = q; R = r; FullRank = fullRank; Factor = factor; }
    public Matrix Source { get; }
    public Matrix Q { get; }
    public Matrix R { get; }
    public bool FullRank { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.QR<double> Factor { get; }
    public bool IsValid => ValidityClaim.All(Source.IsValid, Q.IsValid, R.IsValid);
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.QrSolve(qr: this, rhs: rhs);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CholeskyResult : IValidityEvidence {
    internal CholeskyResult(Matrix l, Matrix source, MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor) { L = l; Source = source; Factor = factor; }
    public Matrix L { get; }
    public Matrix Source { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> Factor { get; }
    public bool IsValid => ValidityClaim.All(
        L.IsValid, Source.IsValid,
        ValidityClaim.CountExactly(count: L.Rows.Value, expected: L.Cols.Value),
        ValidityClaim.CountExactly(count: Source.Rows.Value, expected: Source.Cols.Value));
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.CholeskySolve(cholesky: this, rhs: rhs);
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
public readonly record struct SparseMatrix : IValidityEvidence {
    private SparseMatrix(Dimension rows, Dimension cols, Arr<int> rowPtr, Arr<int> colInd, Arr<double> values) =>
        (Rows, Cols, RowPtr, ColInd, Values) = (rows, cols, rowPtr, colInd, values);
    public Dimension Rows { get; }
    public Dimension Cols { get; }
    public Arr<int> RowPtr { get; }
    public Arr<int> ColInd { get; }
    public Arr<double> Values { get; }
    public static Fin<SparseMatrix> FromTriplets(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets) {
        return Optional(triplets).ToFin(new KernelFault.InvalidInput()).Bind(active => MatrixKernel.AssembleSparse(rows: rows, cols: cols, triplets: active));
    }
    internal static SparseMatrix Trusted(Dimension rows, Dimension cols, Arr<int> rowPtr, Arr<int> colInd, Arr<double> values) =>
        new(rows: rows, cols: cols, rowPtr: rowPtr, colInd: colInd, values: values);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: RowPtr.Count, expected: Rows.Value + 1),
        ValidityClaim.CountExactly(count: ColInd.Count, expected: Values.Count),
        ValidityClaim.Finite(Values.AsSpan()),
        RowPtr[0] == 0 && RowPtr[Rows.Value] == Values.Count,
        Monotone(rowPtr: RowPtr),
        StrictColumns(rowPtr: RowPtr, colInd: ColInd, minCol: static _ => 0, maxCol: Cols.Value));
    public int NonZeros => Values.Count;
    public Fin<Arr<double>> Multiply(Arr<double> vector, Option<OperatorSense> sense = default) =>
        MatrixKernel.SparseProduct(self: this, x: vector, sense: sense.IfNone(noneValue: OperatorSense.Forward));
    public Matrix ToDense() => MatrixKernel.SparseToDense(self: this);
    public SparseMatrix Transpose() => MatrixKernel.SparseTranspose(self: this);
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs, Option<KrylovPolicy> policy = default) =>
        MatrixKernel.SparseSolve(matrix: this, rhs: rhs, policy: policy);
    public Fin<LinearSolution> SingularSolveDetailed(Arr<double> rhs, GaugePolicy gauge, Context context) =>
        MatrixKernel.SingularGaugeSolve(matrix: this, rhs: rhs, gauge: gauge, context: context);
    public Fin<LinearSolution> SolveIndefiniteDetailed(Arr<double> rhs, Option<OperatorSense> sense = default, Option<UnitInterval> pivotTolerance = default, Option<IProgress<double>> progress = default) =>
        MatrixKernel.SparseLuSolve(matrix: this, rhs: rhs, sense: sense.IfNone(noneValue: OperatorSense.Forward),
            pivotTolerance: pivotTolerance.Map(static p => p.Value).IfNone(noneValue: 1.0), progress: progress);
    public Fin<LinearSolution> SolveLeastSquaresDetailed(Arr<double> rhs, Option<OperatorSense> sense = default, Option<IProgress<double>> progress = default) =>
        MatrixKernel.SparseQrSolve(matrix: this, rhs: rhs, sense: sense.IfNone(noneValue: OperatorSense.Forward), progress: progress);
    public Fin<EigenSolution<double, Arr<double>>> SmallestEigenpairsDetailed(int k, double tolerance, Dimension budget) =>
        MatrixKernel.Lobpcg(matrix: this, k: k, tolerance: tolerance, budget: budget);
    public Fin<EigenSolution<double, Arr<double>>> GeneralizedEigenpairsDetailed(SparseMatrix mass, int k) =>
        MatrixKernel.GeneralizedEigenpairs(stiffness: this, mass: mass, k: k);
    internal static bool Monotone(Arr<int> rowPtr) =>
        rowPtr.AsIterable().Zip(rowPtr.AsIterable().Skip(1)).All(static pair => pair.First <= pair.Second);
    internal static bool StrictColumns(Arr<int> rowPtr, Arr<int> colInd, Func<int, int> minCol, int maxCol) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1))).ForAll(row =>
            toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row])) is var span
            && span.ForAll(k => colInd[k] >= minCol(arg: row) && colInd[k] < maxCol)
            && span.AsIterable().Zip(span.AsIterable().Skip(1)).All(pair => colInd[pair.First] < colInd[pair.Second]));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian : IValidityEvidence {
    private SparseHermitian(Dimension order, Arr<int> rowPtr, Arr<int> colInd, Arr<Complex> values) =>
        (Order, RowPtr, ColInd, Values) = (order, rowPtr, colInd, values);
    public Dimension Order { get; }
    public Arr<int> RowPtr { get; }
    public Arr<int> ColInd { get; }
    public Arr<Complex> Values { get; }
    public static Fin<SparseHermitian> FromTriplets(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> upperTriplets) {
        return Optional(upperTriplets).ToFin(new KernelFault.InvalidInput()).Bind(active => MatrixKernel.AssembleHermitian(order: order, triplets: active));
    }
    internal static SparseHermitian Trusted(Dimension order, Arr<int> rowPtr, Arr<int> colInd, Arr<Complex> values) =>
        new(order: order, rowPtr: rowPtr, colInd: colInd, values: values);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: RowPtr.Count, expected: Order.Value + 1),
        ValidityClaim.CountExactly(count: ColInd.Count, expected: Values.Count),
        Admit.FiniteComplexSpan(Values.AsSpan()),
        RowPtr[0] == 0 && RowPtr[Order.Value] == Values.Count,
        SparseMatrix.Monotone(RowPtr),
        SparseMatrix.StrictColumns(rowPtr: RowPtr, colInd: ColInd, minCol: static row => row, maxCol: Order.Value),
        Admit.HermitianDiagonalRealSpan(DiagonalEntries().AsSpan()));
    public int NonZeros => Values.Count;
    public double FrobeniusScale {
        get {
            double diagonal = 0.0, offDiagonal = 0.0;
            for (int row = 0; row < Order.Value; row++)
                for (int p = RowPtr[index: row]; p < RowPtr[index: row + 1]; p++) {
                    double magnitude = (Values[index: p] * Complex.Conjugate(value: Values[index: p])).Real;
                    if (ColInd[index: p] == row) diagonal += magnitude; else offDiagonal += magnitude;
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
    private Complex[] DiagonalEntries() {
        Arr<int> rowPtr = RowPtr;
        Arr<int> colInd = ColInd;
        Arr<Complex> values = Values;
        return [.. Enumerable.Range(start: 0, count: Order.Value).SelectMany(row => Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row])
            .Where(k => colInd[k] == row)
            .Select(k => values[k]))];
    }
}

public sealed record CholeskySparse : IValidityEvidence {
    private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) {
        Source = source; Factor = factor; Order = order;
    }
    private readonly Lock solveLock = new();
    internal void SolveGuarded(double[] b, double[] x) { lock (solveLock) { Factor.Solve(input: b.AsSpan(), result: x.AsSpan()); } }
    public static Fin<CholeskySparse> Of(SparseMatrix symmetric, Option<IProgress<double>> progress = default) =>
        symmetric.Rows.Value != symmetric.Cols.Value
            ? Fin.Fail<CholeskySparse>(error: new KernelFault.InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric)
              from factor in Try.lift(() => progress.Match(
                  Some: report => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, progress: report),
                  None: () => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA))).Run()
              select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
    public SparseMatrix Source { get; private set; }
    internal CSparse.Double.Factorization.SparseCholesky Factor { get; }
    public Dimension Order { get; }
    public int FactorNonZeros => Factor.NonZerosCount;
    public bool IsValid => ValidityClaim.All(Source.IsValid, Factor.NonZerosCount > 0, ValidityClaim.CountAtLeast(count: Order.Value, floor: 1));
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) =>
        MatrixKernel.CholeskySparseSolve(factor: this, rhs: rhs);
    public Fin<CholeskySparse> Refactorize(SparseMatrix values) {
        return !IsValid || !SharesPattern(values: values)
            ? Fin.Fail<CholeskySparse>(error: new KernelFault.InvalidInput())
            : MatrixKernel.ToCSparseSymmetric(s: values).Bind(csc => Try.lift(() => {
                lock (solveLock) {
                    Factor.Refactorize(A: csc);
                    Source = values;
                }
                return Fin.Succ(this);
            }).Run().Bind(static inner => inner));
    }
    public Fin<Seq<LinearSolution>> Sweep(Seq<SparseMatrix> values, Arr<double> rhs) =>
        MatrixKernel.RefactorSweep(factor: this, values: values, rhs: rhs);
    public Fin<CholeskySparse> Update(SparseMatrix column) =>
        Move(column: column, scale: 1.0, move: static (factor, w) => factor.Update(w: w));
    public Fin<CholeskySparse> Downdate(SparseMatrix column) =>
        Move(column: column, scale: -1.0, move: static (factor, w) => factor.Downdate(w: w));
    private Fin<CholeskySparse> Move(SparseMatrix column, double scale, Func<CSparse.Double.Factorization.SparseCholesky, CSparse.Storage.CompressedColumnStorage<double>, bool> move) =>
        !IsValid || column.Rows.Value != Order.Value || column.Cols.Value != 1
            ? Fin.Fail<CholeskySparse>(error: new KernelFault.InvalidInput())
            : from moved in MatrixKernel.RankOneMoved(source: Source, column: column, scale: scale)
              from carrier in Try.lift(() => CSparse.Double.SparseMatrix.OfIndexed(
                  rows: Order.Value, columns: 1, enumerable: MatrixKernel.SparseTripletsOf(matrix: column))).Run()
              from committed in Try.lift(() => {
                  lock (solveLock) {
                      if (!move(arg1: Factor, arg2: carrier)) return Fin.Fail<CholeskySparse>(new KernelFault.InvalidResult());
                      Source = moved;
                      return Fin.Succ(this);
                  }
              }).Run().Bind(static inner => inner)
              select committed;
    private bool SharesPattern(SparseMatrix values) =>
        values.Rows.Value == Source.Rows.Value && values.Cols.Value == Source.Cols.Value && values.NonZeros == Source.NonZeros
        && values.RowPtr.AsSpan().SequenceEqual(Source.RowPtr.AsSpan()) && values.ColInd.AsSpan().SequenceEqual(Source.ColInd.AsSpan());
}
```

## [05]-[SOLUTIONS]

- Owner: `PathEvidence` the closed union carrying whatever the taken route MEASURED, `LinearSolution` the one linear-solve result, `EigenSolution<TEigen, TVector>` the one eigen result generic over real or complex spectra, and `GaugeFix` the gauge a singular solve applied — all spelling the validity fold `IsValid => ValidityClaim.All(…)` under `IValidityEvidence`.
- Cases: `PathEvidence` is `Direct` | `Factored(FactorNonZeros)` | `Ranked(Option<int> Rank, Columns, FactorNonZeros)` | `Iterative(Iterations, Budget, Tolerance, Plan)` — mutually exclusive by construction, so an iterative solution carrying a factor count is unrepresentable and the six vacuous per-column absents the two results once carried delete with them. ONE union serves both the linear and the eigen solution: it was already named for solve paths and read only by the eigen carrier.
- Law: a failure is a FAILED RESULT, never a success-shaped solution — no `NumericalBreakdown` column and no `PositiveInfinity` residual exist, because a magnitude sentinel is a measurement no producer took and every consumer that gates on magnitude reads it as one.
- Entry: solutions mint only at the `MatrixKernel` exits (`SolveSuccess`, `EigenSolutionOf`) under the two-tier evidence law — hard numerical garbage never mints and fails typed, a usable-stop solution is gated valid before release, and a non-usable-stop solution (`RankDeficient`, `IterativeExhausted`, `ResidualRejected`) is the witnessed refusal the caller reads off `Stop.IsUsable` before consuming the vector. Consuming a minted solution without reading its stop is the named consumer defect.
- Auto: each `IsValid` conjoins the mechanical field-shape gates with the semantic couplings — residual within the route's own recorded cap, iterations within budget, and the length pair read off the SENSED operator so a transposed solve is validated against `A'`'s extent — one claim row per invariant; the sense-versus-capability coupling refuses a transposed solution on a route whose trait set omits `Transposed`, so a mislabelled direction cannot mint.
- Packages: `Domain/results` (`IValidityEvidence` and `ValidityClaim`, the validity floor), LanguageExt.Core (`Option`, `Seq`, `Arr`).
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
    public sealed record Iterative(int Iterations, Dimension Budget, double Tolerance, Option<KrylovPlan> Plan) : PathEvidence;

    public Option<int> Iterations => Switch(
        direct: static _ => Option<int>.None, factored: static _ => Option<int>.None,
        ranked: static _ => Option<int>.None, iterative: static path => Some(path.Iterations));
    public Option<int> FactorNonZeros => Switch(
        direct: static _ => Option<int>.None, factored: static path => Some(path.FactorNonZeros),
        ranked: static path => Some(path.FactorNonZeros), iterative: static _ => Option<int>.None);
    public ValidityClaim Holds(double residual) => Switch(
        state: residual,
        direct: static (_, _) => new ValidityClaim(Holds: true),
        factored: static (_, path) => new ValidityClaim(Holds: path.FactorNonZeros > 0),
        ranked: static (_, path) => new ValidityClaim(Holds: path.Rank.Map(rank => rank >= 0 && rank <= path.Columns).IfNone(noneValue: true) && path.FactorNonZeros >= 0),
        iterative: static (measured, path) => new ValidityClaim(
            Holds: path.Iterations >= 0 && path.Iterations <= path.Budget.Value && measured <= path.Tolerance));
}

public readonly record struct LinearSolution(
    Arr<double> Solution, SolvePath Path, SolveStop Stop, OperatorSense Sense, Dimension Rows, Dimension Cols,
    int RhsLength, PathEvidence Evidence, double Residual, double ResidualCap,
    Option<int> InputNonZeros = default, Option<GaugeFix> Gauge = default) : IValidityEvidence {
    public bool IsValid {
        get {
            (Dimension sensedRows, Dimension sensedCols) = Sense.Shape(rows: Rows, cols: Cols);
            return ValidityClaim.All(
                Stop.IsUsable,
                Sense.Equals(OperatorSense.Forward) || Path.Traits.Admits(SolveTrait.Transposed),
                ValidityClaim.CountExactly(count: RhsLength, expected: sensedRows.Value),
                ValidityClaim.CountExactly(count: Solution.Count, expected: sensedCols.Value),
                ValidityClaim.Finite(Solution.AsSpan()),
                ValidityClaim.Nonnegative(value: Residual),
                ValidityClaim.Positive(value: ResidualCap),
                Residual <= ResidualCap,
                Evidence.Holds(residual: Residual),
                InputNonZeros.Map(static nz => nz >= 0).IfNone(noneValue: true),
                ValidityClaim.Evidence(Gauge));
        }
    }
}

public readonly record struct EigenSolution<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path, EigenSolveStop Stop, EigenOrder Order,
    int RequestedPairs, int ReturnedPairs, PathEvidence Evidence, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Stop.IsUsable,
        RequestedPairs >= 1 && ReturnedPairs > 0 && ReturnedPairs <= RequestedPairs,
        ValidityClaim.CountExactly(count: Pairs.Count, expected: ReturnedPairs),
        ValidityClaim.Nonnegative(value: MaxResidual),
        Evidence.Holds(residual: MaxResidual));

    public Fin<Seq<(TEigen Eigenvalue, TVector Eigenvector)>> PairsIn(EigenOrder expected) =>
        IsValid && Order.Equals(expected)
            ? Fin.Succ(Pairs)
            : Fin.Fail<Seq<(TEigen Eigenvalue, TVector Eigenvector)>>(new KernelFault.InvalidResult());
}

public readonly record struct GaugeFix(
    SolvePath Path, int NullspaceDim, Option<int> NullspaceDimNumeric,
    double OperatorScale, double ResidualCompatibility, double ResidualAfterGauge, double ResidualAfterGaugeM,
    double ResidualRelative, Arr<int> PinIndices, int ConstraintRows, GaugeShift PostShiftApplied,
    double RhsMutationNorm, Option<double> MultiplierNorm, double GaugeOrthogonality, double RegularizationEps) : IValidityEvidence {
    public bool IsValid {
        get {
            int nullspaceDim = NullspaceDim;
            return ValidityClaim.All(
                NullspaceDim >= 0 && ConstraintRows >= 0,
                PinIndices.ForAll(static index => index >= 0),
                ValidityClaim.Positive(value: OperatorScale),
                ValidityClaim.Nonnegative(value: ResidualCompatibility),
                ValidityClaim.Nonnegative(value: ResidualAfterGauge),
                ValidityClaim.Nonnegative(value: ResidualAfterGaugeM),
                ValidityClaim.Nonnegative(value: ResidualRelative),
                ValidityClaim.Nonnegative(value: RhsMutationNorm),
                ValidityClaim.Nonnegative(value: GaugeOrthogonality)
                    && GaugeOrthogonality <= OperatorScale * Math.Sqrt(d: EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Nonnegative(value: RegularizationEps) && RegularizationEps <= OperatorScale * EpsilonPolicy.SqrtEpsilon,
                MultiplierNorm.Map(static norm => norm >= 0.0).IfNone(noneValue: true),
                NullspaceDimNumeric.Map(count => count <= nullspaceDim).IfNone(noneValue: true));
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
    private const int StackScratchCells = 512;

    // --- [BRIDGES] ---------------------------------------------------------------------
    internal static DenseMatrixD ToMathNet(Matrix m) =>
        (DenseMatrixD)DenseMatrixD.Build.DenseOfRowMajor(m.Rows.Value, m.Cols.Value, m.Entries.AsIterable());
    internal static Matrix FromMathNet(Matrix<double> m, Dimension rows, Dimension cols) =>
        Matrix.Trusted(rows: rows, cols: cols, entries: new Arr<double>(m.ToRowMajorArray()));
    internal static Fin<Matrix> DenseResult(Matrix source, Dimension rows, Dimension cols, Func<Matrix<double>, Matrix<double>> project) =>
        Try.lift(() => {
            Matrix result = FromMathNet(m: project(arg: ToMathNet(source)), rows: rows, cols: cols);
            return result.IsValid ? Fin.Succ(result) : Fin.Fail<Matrix>(new KernelFault.InvalidResult());
        }).Run().Bind(static inner => inner);
    internal static Matrix Expanded(SymmetricMatrix matrix) {
        int n = matrix.Dimension.Value;
        double[] dense = new double[n * n];
        for (int i = 0; i < n; i++)
            for (int j = i; j < n; j++) {
                double value = matrix.Upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: j)];
                dense[(i * n) + j] = value;
                dense[(j * n) + i] = value;
            }
        return Matrix.Trusted(rows: matrix.Dimension, cols: matrix.Dimension, entries: new Arr<double>(dense));
    }
    internal static Fin<Unit> DefiniteSweep(SymmetricMatrix matrix) {
        if (matrix.Dimension.Value < 1) return Fin.Fail<Unit>(new KernelFault.InvalidInput());
        int cells = matrix.Upper.Count;
        if (cells > StackScratchCells) {
            using SpanOwner<double> rented = SpanOwner<double>.Allocate(size: cells);
            matrix.Upper.AsSpan().CopyTo(rented.Span);
            return DefiniteCore(upper: rented.Span, n: matrix.Dimension.Value);
        }
        Span<double> scratch = stackalloc double[cells];
        matrix.Upper.AsSpan().CopyTo(scratch);
        return DefiniteCore(upper: scratch, n: matrix.Dimension.Value);
    }
    private static Fin<Unit> DefiniteCore(Span<double> upper, int n) {
        for (int i = 0; i < n; i++) {
            double pivot = upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: i)];
            if (!(pivot > EpsilonPolicy.ZeroTolerance)) return Fin.Fail<Unit>(new KernelFault.InvalidResult(Detail: Some($"pivot {i} = {pivot}")));
            double root = Math.Sqrt(d: pivot);
            for (int j = i; j < n; j++) upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: j)] /= root;
            for (int k = i + 1; k < n; k++) {
                double factor = upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: k)];
                for (int j = k; j < n; j++)
                    upper[SymmetricMatrix.FlatIndex(n: n, i: k, j: j)] -= factor * upper[SymmetricMatrix.FlatIndex(n: n, i: i, j: j)];
            }
        }
        return Fin.Succ(unit);
    }
    internal static SparseMatrix SparseTranspose(SparseMatrix self) {
        CSparse.Storage.CompressedColumnStorage<double> transposedCsc = new CSparse.Double.SparseMatrix(
            self.Cols.Value, self.Rows.Value, [.. self.Values], [.. self.ColInd], [.. self.RowPtr]).Transpose();
        return SparseMatrix.Trusted(
            rows: self.Cols, cols: self.Rows,
            rowPtr: new Arr<int>(transposedCsc.ColumnPointers), colInd: new Arr<int>(transposedCsc.RowIndices),
            values: new Arr<double>(transposedCsc.Values));
    }
    internal static CSparse.Storage.CompressedColumnStorage<double> ToCSparse(SparseMatrix s) =>
        CSparse.Double.SparseMatrix.OfIndexed(rows: s.Rows.Value, columns: s.Cols.Value, enumerable: SparseTripletsOf(matrix: s));
    internal static Fin<CSparse.Storage.CompressedColumnStorage<double>> ToCSparseSymmetric(SparseMatrix s) =>
        SymmetricUpper(s: s).Map(upper =>
            CSparse.Double.SparseMatrix.OfIndexed(rows: s.Rows.Value, columns: s.Rows.Value, enumerable: upper));
    private static Fin<List<(int Row, int Col, double Value)>> SymmetricUpper(SparseMatrix s) {
        if (s.Rows.Value != s.Cols.Value) return Fin.Fail<List<(int, int, double)>>(new KernelFault.InvalidInput());
        Dictionary<(int Row, int Col), double> folded = new(capacity: s.NonZeros);
        for (int row = 0; row < s.Rows.Value; row++)
            for (int k = s.RowPtr[row]; k < s.RowPtr[row + 1]; k++) {
                (int Row, int Col) slot = (Math.Min(val1: row, val2: s.ColInd[k]), Math.Max(val1: row, val2: s.ColInd[k]));
                double value = s.Values[k];
                if (folded.TryGetValue(key: slot, value: out double held)
                    && Math.Abs(value: value - held) > EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value), val2: Math.Abs(held))))
                    return Fin.Fail<List<(int, int, double)>>(new KernelFault.InvalidInput());
                folded[slot] = value;
            }
        return Fin.Succ([.. folded.OrderBy(static e => e.Key.Row).ThenBy(static e => e.Key.Col).Select(static e => (e.Key.Row, e.Key.Col, e.Value))]);
    }
    private static Matrix<Complex> ToMathNetHermitian(SparseHermitian s) =>
        SparseMatrixC.OfIndexed(rows: s.Order.Value, columns: s.Order.Value, enumerable: Enumerable.Range(start: 0, count: s.Order.Value)
            .SelectMany(row => Enumerable.Range(start: s.RowPtr[row], count: s.RowPtr[row + 1] - s.RowPtr[row])
                .SelectMany(k => row == s.ColInd[k]
                    ? [(row, row, s.Values[k])]
                    : new[] { (row, s.ColInd[k], s.Values[k]), (s.ColInd[k], row, Complex.Conjugate(s.Values[k])) })));
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) {
        SparseCompressedRowMatrixStorage<double> storage = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            rows: s.Rows.Value, columns: s.Cols.Value, valueCount: s.Values.Count,
            rowPointers: [.. s.RowPtr.AsIterable()], columnIndices: [.. s.ColInd.AsIterable()], values: [.. s.Values.AsIterable()]);
        storage.PopulateExplicitZerosOnDiagonal();
        return DenseMatrixD.Build.Sparse(storage);
    }
    private static Matrix<double> ToMathNetSymmetric(SparseMatrix matrix, IEnumerable<(int Row, int Col, double Value)> upper) =>
        SparseMatrixD.OfIndexed(rows: matrix.Rows.Value, columns: matrix.Cols.Value, enumerable: upper.SelectMany(static e => e.Row == e.Col
            ? [(e.Row, e.Col, e.Value)]
            : new[] { (e.Row, e.Col, e.Value), (e.Col, e.Row, e.Value) }));
    internal static Matrix SparseToDense(SparseMatrix self) {
        SparseCompressedRowMatrixStorage<double> storage = SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            rows: self.Rows.Value, columns: self.Cols.Value, valueCount: self.Values.Count,
            rowPointers: [.. self.RowPtr.AsIterable()], columnIndices: [.. self.ColInd.AsIterable()], values: [.. self.Values.AsIterable()]);
        return FromMathNet(m: DenseMatrixD.Build.Sparse(storage), rows: self.Rows, cols: self.Cols);
    }
    private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());
    private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());

    // --- [WITNESS] ---------------------------------------------------------------------
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        CompensatedNorm(v: b - a.Multiply(x)) / Math.Max(val1: 1.0, val2: CompensatedNorm(v: b));
    private static double BackwardError(Matrix<double> a, LinearVector x, LinearVector b, double operatorScale) =>
        CompensatedNorm(v: b - a.Multiply(x)) / ((operatorScale * x.L2Norm()) + Math.Max(val1: 1.0, val2: CompensatedNorm(v: b)));
    private static double CompensatedNorm(LinearVector v) {
        ddouble sum = 0.0;
        for (int i = 0; i < v.Count; i++) sum += (ddouble)v[i] * v[i];
        return Math.Sqrt(d: (double)sum);
    }
    internal static bool RhsFits(int rows, Arr<double> rhs) =>
        rhs.Count == rows && TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan());
    internal static Fin<double> SparseSymmetricResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs) =>
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
    internal static Fin<LinearSolution> SolveSuccess(Arr<double> solution, int solutionLength, SolvePath path, SolveStop stop,
        Dimension rows, Dimension cols, int rhsLength, double residual, PathEvidence evidence,
        Option<OperatorSense> sense = default, Option<Context> context = default, Option<int> inputNonZeros = default, Option<GaugeFix> gauge = default) {
        LinearSolution solved = new(Solution: solution, Path: path, Stop: stop, Sense: sense.IfNone(noneValue: OperatorSense.Forward),
            Rows: rows, Cols: cols, RhsLength: rhsLength, Evidence: evidence, Residual: residual,
            ResidualCap: path.Cap.In(context: context), InputNonZeros: inputNonZeros, Gauge: gauge);
        return solution.Count == solutionLength && TensorPrimitives.IsFiniteAll<double>(solution.AsSpan())
            && double.IsFinite(residual) && (!stop.IsUsable || solved.IsValid)
            ? Fin.Succ(solved)
            : Fin.Fail<LinearSolution>(new KernelFault.InvalidResult());
    }
    private static Fin<EigenSolution<TEigen, TVector>> EigenSolutionOf<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, EigenSolveStop stop, EigenOrder order, int requestedPairs, double maxResidual, PathEvidence evidence) {
        EigenSolution<TEigen, TVector> solved = new(Pairs: pairs, Path: path, Stop: stop, Order: order, RequestedPairs: requestedPairs, ReturnedPairs: pairs.Count, Evidence: evidence, MaxResidual: maxResidual);
        return double.IsFinite(maxResidual) && (!stop.IsUsable || solved.IsValid)
            ? Fin.Succ(solved)
            : Fin.Fail<EigenSolution<TEigen, TVector>>(new KernelFault.InvalidResult());
    }
    private static Fin<LinearSolution> Conditioned(SolvePath path, Func<SolvePath, Fin<LinearSolution>> solve) =>
        path.Conditioned() is var next && !next.Equals(path)
            ? solve(arg: next)
            : Fin.Fail<LinearSolution>(new KernelFault.OutOfRange(Label: "solve-route", Scalar: path.Key, Requirement: "a conditioning successor"));

    // --- [DENSE_DECOMPOSITIONS] --------------------------------------------------------
    internal static Fin<SvdResult> Svd(Matrix matrix) => Try.lift(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svd = ToMathNet(matrix).Svd(computeVectors: true);
        SvdResult result = new(U: FromMathNet(svd.U, matrix.Rows, matrix.Rows), Sigma: ArrFromVector(svd.S), V: FromMathNet(svd.VT.Transpose(), matrix.Cols, matrix.Cols), Rank: svd.Rank);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SvdResult>(new KernelFault.InvalidResult());
    }).Run().Bind(static inner => inner);
    internal static Fin<LuResult> Lu(Matrix matrix) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<LuResult>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                MathNet.Numerics.LinearAlgebra.Factorization.LU<double> lu = ToMathNet(matrix).LU();
                LuResult result = new(source: matrix, determinant: lu.Determinant, factor: lu);
                return result.IsValid ? Fin.Succ(result) : Fin.Fail<LuResult>(new KernelFault.InvalidResult());
            }).Run().Bind(static inner => inner);
    internal static Fin<QrResult> Qr(Matrix matrix) => Try.lift(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(matrix).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin);
        QrResult result = new(source: matrix, q: FromMathNet(qr.Q, matrix.Rows, matrix.Cols), r: FromMathNet(qr.R, matrix.Cols, matrix.Cols), fullRank: qr.IsFullRank, factor: qr);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<QrResult>(new KernelFault.InvalidResult());
    }).Run().Bind(static inner => inner);
    internal static Fin<CholeskyResult> Cholesky(SymmetricMatrix matrix) =>
        Try.lift(() => {
            Matrix source = Expanded(matrix: matrix);
            MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor = ToMathNet(source).Cholesky();
            CholeskyResult result = new(l: FromMathNet(factor.Factor, matrix.Dimension, matrix.Dimension), source: source, factor: factor);
            return result.IsValid ? Fin.Succ(result) : Fin.Fail<CholeskyResult>(new KernelFault.InvalidResult());
        }).Run().Bind(static inner => inner);
    internal static Fin<EigenSolution<double, Arr<double>>> SymmetricEigen(SymmetricMatrix matrix) =>
        Try.lift(() => {
                Matrix<double> mathNet = ToMathNet(Expanded(matrix: matrix));
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = mathNet.Evd(Symmetricity.Symmetric);
                int n = matrix.Dimension.Value;
                Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: ArrFromVector(evd.EigenVectors.Column(i))))
                    .OrderByDescending(static p => Math.Abs(p.Eigenvalue)));
                return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseSymmetric, stop: EigenSolveStop.DirectSolved, order: EigenOrder.DescendingMagnitude, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector), evidence: new PathEvidence.Direct());
            }).Run().Bind(static inner => inner);
    internal static Fin<EigenSolution<Complex, Arr<Complex>>> GeneralEigen(Matrix matrix) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<EigenSolution<Complex, Arr<Complex>>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                Matrix<Complex> mathNet = DenseMatrixC.Build.Dense(matrix.Rows.Value, matrix.Cols.Value, (i, j) => new Complex(matrix.At(i, j), 0.0));
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = mathNet.Evd(Symmetricity.Asymmetric);
                int n = matrix.Rows.Value;
                Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: ArrFromComplexVector(evd.EigenVectors.Column(i).Normalize(p: 2.0)))));
                return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseGeneral, stop: EigenSolveStop.DirectSolved, order: EigenOrder.Factorization, requestedPairs: n, maxResidual: EigenResidual(a: mathNet, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue), evidence: new PathEvidence.Direct());
            }).Run().Bind(static inner => inner);
    internal static Fin<double> Determinant(Matrix matrix) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<double>(error: new KernelFault.InvalidInput())
            : Try.lift(() => Admit.Finite(value: ToMathNet(matrix).Determinant())).Run().Bind(static inner => inner);

    // --- [DENSE_SOLVES] ----------------------------------------------------------------
    internal static Fin<LinearSolution> Solve(Matrix matrix, Arr<double> rhs) =>
        DenseGate(source: matrix, rhs: rhs, path: SolvePath.DenseLu)
            .Bind(_ => Lu(matrix: matrix))
            .Bind(lu => LuSolve(lu: lu, rhs: rhs))
            .BindFail(_ => Conditioned(path: SolvePath.DenseLu, solve: _ => LeastSquares(matrix: matrix, rhs: rhs)));
    internal static Fin<LinearSolution> LeastSquares(Matrix matrix, Arr<double> rhs) =>
        DenseGate(source: matrix, rhs: rhs, path: SolvePath.DenseQr)
            .Bind(_ => Qr(matrix: matrix))
            .Bind(qr => QrSolve(qr: qr, rhs: rhs));
    internal static Fin<LinearSolution> LuSolve(LuResult lu, Arr<double> rhs) =>
        DenseGate(source: lu.Source, rhs: rhs, path: SolvePath.DenseLu)
            .Bind(_ => DenseSolve(source: lu.Source, rhs: rhs, path: SolvePath.DenseLu, stop: SolveStop.DirectSolved,
                solve: new Func<LinearVector, LinearVector>(lu.Factor.Solve), evidence: new PathEvidence.Direct()));
    internal static Fin<LinearSolution> QrSolve(QrResult qr, Arr<double> rhs) =>
        DenseGate(source: qr.Source, rhs: rhs, path: SolvePath.DenseQr)
            .Bind(_ => DenseSolve(source: qr.Source, rhs: rhs, path: SolvePath.DenseQr,
                stop: qr.FullRank ? SolveStop.LeastSquaresSolved : SolveStop.RankDeficient,
                solve: new Func<LinearVector, LinearVector>(qr.Factor.Solve),
                evidence: new PathEvidence.Ranked(Rank: qr.FullRank ? Some(qr.Source.Cols.Value) : None, Columns: qr.Source.Cols.Value, FactorNonZeros: 0)));
    internal static Fin<LinearSolution> CholeskySolve(CholeskyResult cholesky, Arr<double> rhs) =>
        DenseGate(source: cholesky.Source, rhs: rhs, path: SolvePath.DenseCholesky)
            .Bind(_ => DenseSolve(source: cholesky.Source, rhs: rhs, path: SolvePath.DenseCholesky, stop: SolveStop.DirectSolved,
                solve: new Func<LinearVector, LinearVector>(cholesky.Factor.Solve), evidence: new PathEvidence.Direct()));
    private static Fin<Unit> DenseGate(Matrix source, Arr<double> rhs, SolvePath path) =>
        guard(RhsFits(rows: source.Rows.Value, rhs: rhs)
            && (!path.Traits.Admits(SolveTrait.Square) || source.Rows.Value == source.Cols.Value), new KernelFault.InvalidInput()).ToFin();
    private static Fin<LinearSolution> DenseSolve(Matrix source, Arr<double> rhs, SolvePath path, SolveStop stop, Func<LinearVector, LinearVector> solve, PathEvidence evidence) =>
        Try.lift(() => {
            Matrix<double> a = ToMathNet(source);
            LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
            LinearVector x = solve(arg: b);
            return SolveSuccess(solution: ArrFromVector(x), solutionLength: source.Cols.Value, path: path, stop: stop,
                rows: source.Rows, cols: source.Cols, rhsLength: rhs.Count, residual: RelativeResidual(a: a, x: x, b: b),
                evidence: evidence);
        }).Run().Bind(static inner => inner);
    // --- [SPARSE_ASSEMBLY] -------------------------------------------------------------
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets) {
        List<(int Row, int Col, double Value)> raw = [.. triplets];
        if (raw.Exists(t => !double.IsFinite(t.Value) || t.Row < 0 || t.Row >= rows.Value || t.Col < 0 || t.Col >= cols.Value)) return Fin.Fail<SparseMatrix>(new KernelFault.InvalidInput());
        SparseCompressedRowMatrixStorage<double> storage = SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(
            rows: rows.Value, columns: cols.Value, data: raw.Select(static t => (t.Row, t.Col, t.Value)));
        storage.NormalizeDuplicates();
        double residue = EpsilonPolicy.SqrtEpsilon * TensorPrimitives.Norm<double>(storage.Values.AsSpan(start: 0, length: storage.ValueCount));
        storage.MapInplace(f: value => Math.Abs(value: value) > residue ? value : 0.0, zeros: Zeros.AllowSkip);
        storage.NormalizeZeros();
        SparseMatrix result = SparseMatrix.Trusted(rows: rows, cols: cols,
            rowPtr: new Arr<int>(storage.RowPointers), colInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), values: new Arr<double>(storage.Values[..storage.ValueCount]));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseMatrix>(new KernelFault.InvalidResult());
    }
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
        SparseCompressedRowMatrixStorage<Complex> storage = SparseCompressedRowMatrixStorage<Complex>.OfIndexedEnumerable(
            rows: order.Value, columns: order.Value,
            data: upper.Select(static t => (t.Row, t.Col, t.Row == t.Col ? new Complex(t.Value.Real, 0.0) : t.Value)));
        SparseHermitian result = SparseHermitian.Trusted(order: order, rowPtr: new Arr<int>(storage.RowPointers),
            colInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), values: new Arr<Complex>(storage.Values[..storage.ValueCount]));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseHermitian>(new KernelFault.InvalidResult());
    }
    internal static void AddHermitianRealBlockTriplets(Action<int, int, double> add, int order, int i, int j, double real, double imaginary, double diagonal) {
        add(i, i, diagonal); add(j, j, diagonal); add(i + order, i + order, diagonal); add(j + order, j + order, diagonal);
        add(i, j, real); add(j, i, real); add(i + order, j + order, real); add(j + order, i + order, real);
        add(i, j + order, -imaginary); add(j + order, i, -imaginary); add(i + order, j, imaginary); add(j, i + order, imaginary);
    }
    internal static void AddHermitianRealBlockTriplets(List<(int Row, int Col, double Value)> triplets, int order, int i, int j, double real, double imaginary, double diagonal) =>
        triplets.AddRange([
            (i, i, diagonal), (j, j, diagonal), (i + order, i + order, diagonal), (j + order, j + order, diagonal),
            (i, j, real), (j, i, real), (i + order, j + order, real), (j + order, i + order, real),
            (i, j + order, -imaginary), (j + order, i, -imaginary), (i + order, j, imaginary), (j, i + order, imaginary),
        ]);
    internal static Fin<SparseMatrix> RankOneMoved(SparseMatrix source, SparseMatrix column, double scale) {
        List<(int Row, int Col, double Value)> stored = SparseTripletsOf(matrix: column);
        List<(int Row, int Col, double Value)> entries = SparseTripletsOf(matrix: source, capacityBonus: stored.Count * stored.Count);
        entries.AddRange(collection: from left in stored
                                     from right in stored
                                     select (Row: left.Row, Col: right.Row, Value: scale * left.Value * right.Value));
        return AssembleSparse(rows: source.Rows, cols: source.Cols, triplets: entries);
    }
    internal static List<(int Row, int Col, double Value)> SparseTripletsOf(SparseMatrix matrix, int capacityBonus = 0, double scale = 1.0) {
        int n = matrix.Rows.Value;
        List<(int Row, int Col, double Value)> triplets = new(capacity: matrix.NonZeros + capacityBonus);
        for (int i = 0; i < n; i++)
            for (int k = matrix.RowPtr[index: i]; k < matrix.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, matrix.ColInd[index: k], scale * matrix.Values[index: k]));
        return triplets;
    }

    // --- [SPARSE_SOLVES] ---------------------------------------------------------------
    internal static Fin<Arr<double>> SparseProduct(SparseMatrix self, Arr<double> x, OperatorSense sense) {
        (Dimension rows, Dimension cols) = sense.Shape(rows: self.Rows, cols: self.Cols);
        return x.Count != cols.Value || !TensorPrimitives.IsFiniteAll<double>(x.AsSpan())
            ? Fin.Fail<Arr<double>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                double[] y = new double[rows.Value];
                sense.Apply(operand: ToCSparse(s: self), x: [.. x.AsIterable()], y: y);
                Arr<double> result = new(y);
                return TensorPrimitives.IsFiniteAll<double>(result.AsSpan()) ? Fin.Succ(result) : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult());
            }).Run().Bind(static inner => inner);
    }
    internal static Fin<Arr<Complex>> HermitianProduct(SparseHermitian self, Arr<Complex> x) =>
        Try.lift(() => ArrFromComplexVector(ToMathNetHermitian(s: self).Multiply(DenseVectorC.OfArray([.. x.AsIterable()]))) switch {
            Arr<Complex> result when Admit.FiniteComplexSpan(result.AsSpan()) => Fin.Succ(result),
            _ => Fin.Fail<Arr<Complex>>(new KernelFault.InvalidResult()),
        }).Run().Bind(static inner => inner);
    internal static Fin<LinearSolution> SparseSolve(SparseMatrix matrix, Arr<double> rhs, Option<KrylovPolicy> policy) {
        if (matrix.Rows.Value != matrix.Cols.Value || !RhsFits(rows: matrix.Rows.Value, rhs: rhs)) return Fin.Fail<LinearSolution>(new KernelFault.InvalidInput());
        return KrylovPolicy.AutoBudget(rows: matrix.Rows) is var autoBudget
            && policy.IfNone(noneValue: new KrylovPolicy(Preconditioner: SparsePreconditioner.Diagonal, Solver: KrylovSolver.BiCgStab,
                Tolerance: SolvePath.SparseKrylov.Cap.Floor, Budget: autoBudget, Stop: None, CanFallback: true)) is var active
            ? Try.lift(() => {
                Matrix<double> a = ToMathNetSparse(s: matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                MathNet.Numerics.LinearAlgebra.Solvers.IPreconditioner<double> preconditioner = active.Preconditioner.Create();
                preconditioner.Initialize(matrix: a);
                int seen = 0;
                MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double> iterator = new([
                    new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, _) => {
                        seen = iteration;
                        return MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Continue;
                    }),
                    new MathNet.Numerics.LinearAlgebra.Solvers.FailureStopCriterion<double>(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.DivergenceStopCriterion<double>(maximumRelativeIncrease: BiCgStabDivergenceFactor, minimumIterations: KrylovPolicy.DivergenceWarmup),
                    new MathNet.Numerics.LinearAlgebra.Solvers.ResidualStopCriterion<double>(maximum: active.Tolerance, minimumIterationsBelowMaximum: KrylovPolicy.ResidualConfirmations),
                    .. active.Stop.Map(static halt => (MathNet.Numerics.LinearAlgebra.Solvers.IIterationStopCriterion<double>)
                        new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, residual) =>
                            halt(iteration, residual.L2Norm())
                                ? MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.StoppedWithoutConvergence
                                : MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Continue)).ToSeq(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.IterationCountStopCriterion<double>(maximumNumberOfIterations: active.Budget.Value),
                ]);
                LinearVector iterate = a.SolveIterative(input: b, solver: active.Solver.Create(), iterator: iterator, preconditioner: preconditioner);
                double iterativeResidual = RelativeResidual(a: a, x: iterate, b: b);
                bool converged = iterator.Status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged
                    && double.IsFinite(iterativeResidual) && iterativeResidual <= active.Tolerance;
                PathEvidence evidence = new PathEvidence.Iterative(Iterations: seen, Budget: active.Budget, Tolerance: active.Tolerance, Plan: Some(new KrylovPlan(Preconditioner: active.Preconditioner, Solver: active.Solver)));
                return converged
                    ? SolveSuccess(solution: ArrFromVector(iterate), solutionLength: matrix.Cols.Value, path: SolvePath.SparseKrylov,
                        stop: SolveStop.ResidualConverged, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count,
                        residual: iterativeResidual, evidence: evidence, inputNonZeros: Some(matrix.NonZeros))
                    : active.CanFallback
                        ? DenseFallback(matrix: matrix, a: a, b: b, rhs: rhs)
                        : SolveSuccess(solution: ArrFromVector(iterate), solutionLength: matrix.Cols.Value, path: SolvePath.SparseKrylov,
                            stop: SolveStop.IterativeExhausted, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count,
                            residual: iterativeResidual, evidence: evidence, inputNonZeros: Some(matrix.NonZeros));
            }).Run().Bind(static inner => inner)
            : Fin.Fail<LinearSolution>(new KernelFault.InvalidInput());
    }
    private static Fin<LinearSolution> DenseFallback(SparseMatrix matrix, Matrix<double> a, LinearVector b, Arr<double> rhs) {
        LinearVector x = a.Solve(b);
        double residual = RelativeResidual(a: a, x: x, b: b);
        return SolveSuccess(solution: ArrFromVector(x), solutionLength: matrix.Cols.Value, path: SolvePath.DenseFallback,
            stop: double.IsFinite(residual) && residual <= SolvePath.DenseFallback.Cap.Floor ? SolveStop.DirectSolved : SolveStop.ResidualRejected,
            rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual,
            evidence: new PathEvidence.Direct(), inputNonZeros: Some(matrix.NonZeros));
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
                    SolveSuccess(solution: solution, solutionLength: factor.Order.Value, path: SolvePath.SparseCholesky,
                        stop: SolveStop.DirectSolved, rows: factor.Source.Rows, cols: factor.Source.Cols, rhsLength: rhs.Count,
                        residual: residual, evidence: new PathEvidence.Factored(FactorNonZeros: factor.FactorNonZeros), inputNonZeros: Some(factor.Source.NonZeros)));
            }).Run().Bind(static inner => inner);
    internal static Fin<Seq<LinearSolution>> RefactorSweep(CholeskySparse factor, Seq<SparseMatrix> values, Arr<double> rhs) {
        bool prior = CSparse.Storage.CompressedColumnStorage<double>.AutoTrimStorage;
        CSparse.Storage.CompressedColumnStorage<double>.AutoTrimStorage = false;
        try {
            return values.TraverseM(step => factor.Refactorize(values: step).Bind(_ => factor.SolveDetailed(rhs: rhs))).As();
        }
        finally {
            CSparse.Storage.CompressedColumnStorage<double>.AutoTrimStorage = prior;
        }
    }
    internal static Fin<LinearSolution> SparseLuSolve(SparseMatrix matrix, Arr<double> rhs, OperatorSense sense, double pivotTolerance, Option<IProgress<double>> progress) =>
        matrix.Rows.Value != matrix.Cols.Value || !RhsFits(rows: matrix.Rows.Value, rhs: rhs)
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                int n = matrix.Rows.Value;
                CSparse.Storage.CompressedColumnStorage<double> csc = ToCSparse(s: matrix);
                CSparse.Double.Factorization.SparseLU lu = progress.Match(
                    Some: report => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance, progress: report),
                    None: () => CSparse.Double.Factorization.SparseLU.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, tol: pivotTolerance));
                double[] solution = new double[n];
                sense.SolveLu(factor: lu, rhs: [.. rhs.AsIterable()], solution: solution);
                double residual = SensedResidual(operand: csc, sense: sense, solution: solution, rhs: rhs);
                return SolveSuccess(solution: new Arr<double>(solution), solutionLength: n, path: SolvePath.SparseLu,
                    stop: double.IsFinite(residual) && residual <= SolvePath.SparseLu.Cap.Floor ? SolveStop.DirectSolved : SolveStop.ResidualRejected,
                    rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual,
                    evidence: new PathEvidence.Factored(FactorNonZeros: lu.NonZerosCount), sense: Some(sense),
                    inputNonZeros: Some(matrix.NonZeros));
            }).Run().Bind(static inner => inner);
    internal static Fin<LinearSolution> SparseQrSolve(SparseMatrix matrix, Arr<double> rhs, OperatorSense sense, Option<IProgress<double>> progress) {
        (Dimension sensedRows, Dimension sensedCols) = sense.Shape(rows: matrix.Rows, cols: matrix.Cols);
        return rhs.Count != sensedRows.Value || !TensorPrimitives.IsFiniteAll<double>(rhs.AsSpan())
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                CSparse.Storage.CompressedColumnStorage<double> csc = ToCSparse(s: matrix);
                CSparse.Double.Factorization.SparseQR qr = progress.Match(
                    Some: report => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA, progress: report),
                    None: () => CSparse.Double.Factorization.SparseQR.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtA));
                using SpanOwner<double> work = SpanOwner<double>.Allocate(size: Math.Max(val1: matrix.Rows.Value, val2: matrix.Cols.Value), mode: AllocationMode.Clear);
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
                bool admitted = double.IsFinite(residual) && residual <= SolvePath.SparseQr.Cap.Floor;
                return SolveSuccess(solution: new Arr<double>(solution), solutionLength: sensedCols.Value, path: SolvePath.SparseQr,
                    stop: admitted ? SolveStop.LeastSquaresSolved : SolveStop.RankDeficient, rows: matrix.Rows, cols: matrix.Cols,
                    rhsLength: rhs.Count, residual: residual,
                    evidence: new PathEvidence.Ranked(Rank: admitted ? Some(sensedCols.Value) : None, Columns: sensedCols.Value, FactorNonZeros: qr.NonZerosCount), sense: Some(sense), inputNonZeros: Some(matrix.NonZeros));
            }).Run().Bind(static inner => inner);
    }

    // --- [SINGULAR_GAUGE] --------------------------------------------------------------
    internal static Fin<LinearSolution> SingularGaugeSolve(SparseMatrix matrix, Arr<double> rhs, GaugePolicy gauge, Context context) =>
        matrix.Rows.Value != matrix.Cols.Value || !RhsFits(rows: matrix.Rows.Value, rhs: rhs) || !GaugeFits(gauge: gauge, dimension: matrix.Rows.Value)
            ? Fin.Fail<LinearSolution>(new KernelFault.InvalidInput())
            : from upper in SymmetricUpper(s: matrix)
              from result in Try.lift(() => {
                  int n = matrix.Rows.Value;
                  Matrix<double> aSym = ToMathNetSymmetric(matrix: matrix, upper: upper);
                  LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                  double operatorScale = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: MatrixNormKind.Frobenius.Compute(matrix: matrix));
                  Option<Arr<double>> weights = gauge.Switch(
                      pin: static p => p.Mass, meanZeroDeflation: static d => d.Mass, lagrangeKKT: static k => k.Mass);
                  Matrix<double> mass = weights.Match(
                      Some: diagonal => (Matrix<double>)DenseMatrixD.OfDiagonalVector(DenseVectorD.OfArray([.. diagonal.AsIterable()])),
                      None: () => DenseMatrixD.CreateIdentity(order: n));
                  Matrix<double> nullspace = gauge.Switch(
                      state: n,
                      pin: static (dim, p) => DenseMatrixD.OfColumnVectors([.. p.Indices.AsIterable().Select(index => DenseVectorD.Create(dim, i => i == index ? 1.0 : 0.0))]),
                      meanZeroDeflation: static (_, d) => BasisColumns(basis: d.Nullspace), lagrangeKKT: static (_, k) => BasisColumns(basis: k.Nullspace));
                  GaugeShift shift = gauge.Switch(
                      pin: static p => p.PostShift, meanZeroDeflation: static d => d.PostShift, lagrangeKKT: static k => k.PostShift);
                  int nullspaceDim = nullspace.ColumnCount;
                  Arr<int> pinIndices = gauge.Switch(
                      pin: static p => p.Indices, meanZeroDeflation: static _ => new Arr<int>([]), lagrangeKKT: static _ => new Arr<int>([]));
                  double compatibility = gauge.Switch(
                      state: (Nullspace: nullspace, Rhs: b),
                      pin: static (_, _) => 0.0,
                      meanZeroDeflation: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm(),
                      lagrangeKKT: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm());
                  bool projectRhs = gauge.Switch(
                      state: compatibility > context.For(lane: ToleranceLane.Kkt).Value * Math.Max(val1: 1.0, val2: b.InfinityNorm()),
                      pin: static (_, _) => false, meanZeroDeflation: static (project, _) => project, lagrangeKKT: static (_, _) => false);
                  LinearVector rhsGauged = projectRhs ? DeflateRhs(nullspace: nullspace, mass: mass, b: b) : b;
                  double rhsMutation = (rhsGauged - b).L2Norm();
                  return gauge.Switch(
                      state: (Matrix: matrix, Upper: upper, ASym: aSym, Mass: mass, Nullspace: nullspace, Rhs: rhsGauged),
                      pin: static (s, p) => SolvePin(matrix: s.Matrix, upper: s.Upper, aSym: s.ASym, pin: p, b: s.Rhs),
                      meanZeroDeflation: static (s, _) => SolveDeflated(matrix: s.Matrix, aSym: s.ASym, mass: s.Mass, nullspace: s.Nullspace, b: s.Rhs),
                      lagrangeKKT: static (s, _) => SolveKkt(upper: s.Upper, aSym: s.ASym, massNullspace: s.Mass.Multiply(s.Nullspace), b: s.Rhs))
                  .Bind(stage => {
                      LinearVector shifted = shift.Apply(mass: mass, x: stage.X);
                      double relative = BackwardError(a: aSym, x: shifted, b: b, operatorScale: operatorScale);
                      GaugeFix fix = new(
                          Path: stage.Path, NullspaceDim: nullspaceDim, NullspaceDimNumeric: stage.NullspaceDimNumeric,
                          OperatorScale: operatorScale, ResidualCompatibility: compatibility, ResidualAfterGauge: stage.Residual,
                          ResidualAfterGaugeM: MassResidual(a: aSym, mass: mass, x: shifted, b: b), ResidualRelative: relative,
                          PinIndices: pinIndices, ConstraintRows: nullspaceDim, PostShiftApplied: shift,
                          RhsMutationNorm: rhsMutation, MultiplierNorm: stage.MultiplierNorm,
                          GaugeOrthogonality: nullspace.TransposeThisAndMultiply(mass.Multiply(shifted)).L2Norm() / Math.Max(val1: 1.0, val2: shifted.L2Norm()),
                          RegularizationEps: stage.RegularizationEps);
                      return SolveSuccess(solution: ArrFromVector(shifted), solutionLength: n, path: stage.Path,
                          stop: relative <= context.For(lane: ToleranceLane.Residual).Value ? stage.Stop : SolveStop.IterativeExhausted,
                          rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: relative,
                          evidence: stage.Evidence, context: Some(context), inputNonZeros: Some(matrix.NonZeros), gauge: Some(fix));
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
    private static Fin<GaugeStage> SolvePin(SparseMatrix matrix, List<(int Row, int Col, double Value)> upper, Matrix<double> aSym, GaugePolicy.Pin pin, LinearVector b) {
        int n = matrix.Rows.Value;
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
        return from reducedMatrix in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: filtered)
               from factor in CholeskySparse.Of(symmetric: reducedMatrix)
               from solved in factor.SolveDetailed(rhs: new Arr<double>(reduced))
               let reassembled = DenseVectorD.Create(n, i => pinned[i] ? pinValues[i] : solved.Solution[remap[i]])
               select new GaugeStage(X: reassembled, Residual: RelativeResidual(a: aSym, x: reassembled, b: b), Stop: SolveStop.DirectSolved,
                   Path: SolvePath.SparseCholesky, Evidence: new PathEvidence.Factored(FactorNonZeros: factor.FactorNonZeros),
                   MultiplierNorm: None, RegularizationEps: 0.0);
    }
    private static Fin<GaugeStage> SolveDeflated(SparseMatrix matrix, Matrix<double> aSym, Matrix<double> mass, Matrix<double> nullspace, LinearVector b) =>
        SparseSolve(matrix: matrix, rhs: new Arr<double>(b.ToArray()), policy: None).Map(solved => {
            (LinearVector projected, double shift, int numericRank) = ProjectRange(nullspace: nullspace, mass: mass,
                x: DenseVectorD.OfArray([.. solved.Solution.AsIterable()]));
            return new GaugeStage(X: projected, Residual: RelativeResidual(a: aSym, x: projected, b: b), Stop: solved.Stop,
                Path: solved.Path, Evidence: solved.Evidence, MultiplierNorm: None, RegularizationEps: shift,
                NullspaceDimNumeric: Some(numericRank));
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
            .BindFail(_ => Saddle(saddle: saddle, rhs: rhs, aSym: aSym, b: b, n: n, path: SolvePath.SparseLdl.Conditioned()));
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
                ? Fin.Succ(new GaugeStage(X: x, Residual: residual, Stop: SolveStop.DirectSolved, Path: path,
                    Evidence: new PathEvidence.Factored(FactorNonZeros: ((CSparse.Factorization.ISparseFactorization<double>)factor).NonZerosCount),
                    MultiplierNorm: Some(DenseVectorD.OfArray([.. solution.Skip(count: n)]).L2Norm()), RegularizationEps: 0.0))
                : Fin.Fail<GaugeStage>(new KernelFault.InvalidResult(Detail: Some($"kkt residual non-finite on {path.Key}")));
        }).Run().Bind(static inner => inner);
    private readonly record struct GaugeStage(LinearVector X, double Residual, SolveStop Stop, SolvePath Path,
        PathEvidence Evidence, Option<double> MultiplierNorm, double RegularizationEps, Option<int> NullspaceDimNumeric = default);

    // --- [GENERALIZED_EIGEN] -----------------------------------------------------------
    internal static Fin<EigenSolution<double, Arr<double>>> GeneralizedEigenpairs(SparseMatrix stiffness, SparseMatrix mass, int k) =>
        stiffness.Rows.Value != stiffness.Cols.Value || mass.Rows.Value != mass.Cols.Value || stiffness.Rows.Value != mass.Rows.Value || k < 1 || k >= stiffness.Rows.Value
            ? Fin.Fail<EigenSolution<double, Arr<double>>>(new KernelFault.InvalidInput())
            : from stiffnessUpper in SymmetricUpper(s: stiffness)
              from massUpper in SymmetricUpper(s: mass)
              from solved in Try.lift(() => {
                  Matrix<double> stiffnessM = Densified(operand: ToMathNetSymmetric(matrix: stiffness, upper: stiffnessUpper));
                  Matrix<double> massM = Densified(operand: ToMathNetSymmetric(matrix: mass, upper: massUpper));
                  (LinearVector vals, Matrix<double> vecs, int factorNonZeros) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                  Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                      .OrderBy(i => vals[i]).Take(k)
                      .Select(i => (Eigenvalue: vals[i], Eigenvector: ArrFromVector(vecs.Column(i)))));
                  return EigenSolutionOf(pairs: pairs, path: EigenSolvePath.DenseCongruence, stop: EigenSolveStop.DirectSolved,
                      order: EigenOrder.Ascending, requestedPairs: k, maxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs),
                      evidence: new PathEvidence.Factored(FactorNonZeros: factorNonZeros));
              }).Run().Bind(static inner => inner)
              select solved;
    private static Matrix<double> Densified(Matrix<double> operand) =>
        DenseMatrixD.Build.DenseOfRowMajor(operand.RowCount, operand.ColumnCount, operand.ToRowMajorArray());
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
    internal static Fin<EigenSolution<double, Arr<double>>> Lobpcg(SparseMatrix matrix, int k, double tolerance, Dimension budget) =>
        matrix.Rows.Value != matrix.Cols.Value || k < 1 || k >= matrix.Rows.Value || !double.IsFinite(tolerance) || tolerance <= 0
            ? Fin.Fail<EigenSolution<double, Arr<double>>>(new KernelFault.InvalidInput())
            : SymmetricUpper(s: matrix).Bind(upper => {
                Matrix<double> a = ToMathNetSymmetric(matrix: matrix, upper: upper);
                return LobpcgCore(A: a, X: OrthonormalRandom(rows: matrix.Rows.Value, k: k, lane: MatrixDrawLane.RealBasis, sample: static (ref ulong s) => Deterministic.NextSignedUnit(state: ref s), orthonormalise: Orthonormalise), P: DenseMatrixD.Create(matrix.Rows.Value, k, 0.0), jacobi: DiagonalInverse(a), k: k, tolerance: tolerance, budget: budget, path: EigenSolvePath.SparseLobpcg, rayleigh: Rayleigh, diagonal: DenseMatrixD.OfDiagonalVector, adjoint: static m => m.Transpose(), orthonormalise: Orthonormalise, solveGeneralised: static (Ahat, Mhat) => { (LinearVector Vals, Matrix<double> Vecs, int _) = SolveGeneralised(Ahat: Ahat, Mhat: Mhat); return (Vals, Vecs); }, eigenvalue: static value => value, vector: static v => ArrFromVector(v: v), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector));
            });
    internal static Fin<EigenSolution<double, Arr<Complex>>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, Dimension budget) =>
        k < 1 || k >= matrix.Order.Value || !double.IsFinite(tolerance) || tolerance <= 0
            ? Fin.Fail<EigenSolution<double, Arr<Complex>>>(new KernelFault.InvalidInput())
            : Try.lift(() => {
                Matrix<Complex> a = ToMathNetHermitian(matrix);
                return LobpcgCore(A: a, X: OrthonormalRandom(rows: matrix.Order.Value, k: k, lane: MatrixDrawLane.HermitianBasis, sample: static (ref ulong s) => Deterministic.NextSignedComplexUnit(state: ref s), orthonormalise: OrthonormaliseComplex), P: DenseMatrixC.Create(matrix.Order.Value, k, Complex.Zero), jacobi: DiagonalInverseComplex(a), k: k, tolerance: tolerance, budget: budget, path: EigenSolvePath.HermitianLobpcg, rayleigh: RayleighComplex, diagonal: DenseMatrixC.OfDiagonalVector, adjoint: static m => m.ConjugateTranspose(), orthonormalise: OrthonormaliseComplex, solveGeneralised: static (Ahat, Mhat) => SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat), eigenvalue: static value => value.Real, vector: static v => ArrFromComplexVector(v: v), residual: static (a, pairs) => EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue));
            }).Run().Bind(static inner => inner);
    private static Fin<EigenSolution<double, TVector>> LobpcgCore<T, TVector>(Matrix<T> A, Matrix<T> X, Matrix<T> P, MathNet.Numerics.LinearAlgebra.Vector<T> jacobi, int k, double tolerance, Dimension budget, EigenSolvePath path, Func<Matrix<T>, Matrix<T>, MathNet.Numerics.LinearAlgebra.Vector<T>> rayleigh, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, Matrix<T>> diagonal, Func<Matrix<T>, Matrix<T>> adjoint, Func<Matrix<T>, Matrix<T>> orthonormalise, Func<Matrix<T>, Matrix<T>, (MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solveGeneralised, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector, Func<Matrix<T>, Seq<(double Eigenvalue, TVector Eigenvector)>, double> residual)
        where T : struct, IEquatable<T>, IFormattable {
        int n = A.RowCount;
        return Iterate(iter: 0, X: X, P: P);
        Fin<EigenSolution<double, TVector>> Iterate(int iter, Matrix<T> X, Matrix<T> P) =>
            iter >= budget.Value
                ? Solved(iter: iter, X: X, stop: EigenSolveStop.IterativeExhausted)
                : Step(iter: iter, X: X, P: P);
        Fin<EigenSolution<double, TVector>> Step(int iter, Matrix<T> X, Matrix<T> P) {
            Matrix<T> AX = A * X;
            MathNet.Numerics.LinearAlgebra.Vector<T> lambda = rayleigh(arg1: X, arg2: AX);
            Matrix<T> R = AX - (X * diagonal(arg: lambda));
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: lambda, X: X);
            return residual(arg1: A, arg2: pairs) < tolerance
                ? Solved(iter: iter, X: X, stop: EigenSolveStop.ResidualConverged)
                : Continue(iter: iter, X: X, P: P, R: R);
        }
        Fin<EigenSolution<double, TVector>> Continue(int iter, Matrix<T> X, Matrix<T> P, Matrix<T> R) {
            Matrix<T> W = ApplyJacobi(R: R, invDiag: jacobi);
            bool hasPrevious = iter > 0 && Enumerable.Range(0, P.ColumnCount).Any(j => P.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon);
            Matrix<T> S = orthonormalise(arg: hasPrevious ? X.Append(W).Append(P) : X.Append(W));
            int[] survivors = [.. Enumerable.Range(0, S.ColumnCount).Where(j => S.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon)];
            if (survivors.Length < k) return Solved(iter: iter, X: X, stop: EigenSolveStop.IterativeExhausted);
            Matrix<T> Sr = Matrix<T>.Build.DenseOfColumnVectors([.. survivors.Select(S.Column)]);
            Matrix<T> STr = adjoint(arg: Sr);
            return Try.lift(() => solveGeneralised(arg1: STr * (A * Sr), arg2: STr * Sr)).Run().Bind(solution => {
                Matrix<T> Z = ScatterRows(reduced: TakeSmallest(eigVals: solution.Vals, eigVecs: solution.Vecs, k: k, key: eigenvalue), rows: S.ColumnCount, sourceRows: survivors);
                Matrix<T> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : Matrix<T>.Build.Dense(n, k);
                return Iterate(iter: iter + 1, X: orthonormalise(arg: S * Z), P: (W * Z.SubMatrix(k, k, 0, k)) + previous);
            });
        }
        Fin<EigenSolution<double, TVector>> Solved(int iter, Matrix<T> X, EigenSolveStop stop) {
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = Pairs(lambda: rayleigh(arg1: X, arg2: A * X), X: X);
            return EigenSolutionOf(pairs: pairs, path: path, stop: stop, order: EigenOrder.Ascending, requestedPairs: k,
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
        for (int i = 0; i < sourceRows.Length; i++) full.SetRow(rowIndex: sourceRows[i], row: FactoryBridge.Row(i));
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
        for (int i = 0; i < R.RowCount; i++) scaled.SetRow(rowIndex: i, row: FactoryBridge.Row(i).Multiply(scalar: invDiag[i]));
        return scaled;
    }
    private static Matrix<T> TakeSmallest<T>(MathNet.Numerics.LinearAlgebra.Vector<T> eigVals, Matrix<T> eigVecs, int k, Func<T, double> key)
        where T : struct, IEquatable<T>, IFormattable =>
        Matrix<T>.Build.DenseOfColumnVectors([.. Enumerable.Range(start: 0, count: eigVals.Count).OrderBy(i => key(arg: eigVals[i])).Take(count: k).Select(eigVecs.Column)]);
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
