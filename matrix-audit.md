# 1. Replace the dense matrix wrapper with the MathNet carrier

From:
`matrix.md:271-281`
```csharp
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
```
To:
```csharp
internal static Fin<Matrix<double>> Dense(Dimension rows, Dimension cols, Arr<double> entries) =>
    from _ in guard(entries.Count == rows.Value * cols.Value, new KernelFault.InvalidInput()).ToFin()
    from _ in guard(TensorPrimitives.IsFiniteAll<double>(entries.AsSpan()), new KernelFault.InvalidInput())
    select DenseMatrixD.Build.Dense(rows.Value, cols.Value,
        (row, column) => entries[(row * cols.Value) + column]);
```
Why: The local type duplicates MathNet's matrix identity, dimensions, storage, transpose, multiplication, inverse, pseudoinverse, trace, determinant, and factorization surface. The MathNet catalog requires `Matrix<double>` to compose directly and identifies a package-local matrix wrapper as the rejected form; only finite row-major admission remains Rasm-owned.

Change: Delete `Rasm.Numerics.Matrix`, retain `MatrixKernel.Dense` as the carrier-typed admission boundary, use MathNet members for package-owned algebra, and keep only Rasm operations that add typed solve or eigen evidence.

Delta: -35 target LOC, -1 module-level type, and -23 module-level members net after adding `MatrixKernel.Dense`.

Ripples: Replace the `Rasm.Numerics.Matrix` alias, `Matrix.Of`, and wrapper algebra with `Matrix<double>`, `MatrixKernel.Dense`, and direct MathNet members in `libs/dotnet/Rasm/.planning/Numerics/integrate.md`, `Parametric/projections.md`, `Parametric/subdivide.md`, `Processing/flow.md`, `Processing/register.md`, `Solving/fit.md`, `Solving/solver.md`, `Spatial/fields.md`, `Spatial/neighbors.md`, `Spatial/transport.md`, and `Meshing/reconstruct.md`; remove the obsolete collision note from `Interaction/paint.md`.

# 2. Return MathNet factorization handles directly

From:
`matrix.md:350-369`
```csharp
public readonly record struct LuResult : IValidityEvidence {
    internal LuResult(Matrix source, double determinant, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor) { Source = source; Determinant = determinant; Factor = factor; }
    public Matrix Source { get; }
    public double Determinant { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.LU<double> Factor { get; }
    public bool IsValid => ValidityClaim.All(Source.IsValid, ValidityClaim.Finite(value: Determinant));
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.LuSolve(lu: this, rhs: rhs);
}

public readonly record struct QrResult : IValidityEvidence {
    internal MathNet.Numerics.LinearAlgebra.Factorization.QR<double> Factor { get; }
    public Fin<LinearSolution> SolveDetailed(Arr<double> rhs) => MatrixKernel.QrSolve(qr: this, rhs: rhs);
}
```
To:
```csharp
// LuResult, QrResult, CholeskyResult, and SvdResult DELETED
```
Why: `LuResult`, `QrResult`, `CholeskyResult`, and `SvdResult` rename package factor types and mirror their determinant, rank, singular-value, and factor-matrix projections. MathNet already owns reusable factorization handles; Rasm owns only admission and residual-bearing solve evidence.

Change: Delete all four result wrappers. Consumers retain `LU<double>`, `QR<double>`, `Cholesky<double>`, or `Svd<double>` directly and pass the factor with its original matrix to the typed solve exit when a `LinearSolution` is required.

Delta: -42 target LOC, -4 module-level types, and -27 module-level members net after the factor-accepting solve entry.

Ripples: Store `LU<double>` with its source matrix in `libs/dotnet/Rasm/.planning/Parametric/subdivide.md`; use `Svd<double>` directly in `Processing/register.md` and `Solving/solver.md`; use `Cholesky<double>` directly in `Solving/solver.md`.

# 3. Replace the general sparse wrapper with CSparse compressed-column storage

From:
`matrix.md:398-411`
```csharp
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
```
To:
```csharp
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
```
Why: The wrapper re-declares dimensions, buffers, transpose, GEMV, densification, and nonzero count already owned by `CompressedColumnStorage<double>`, then reconstructs CSC before every direct factorization. CSparse's admitted entry shape is CSC and its `CoordinateStorage<double>` already owns duplicate accumulation.

Change: Delete `SparseMatrix`; make admitted CSC the sparse carrier; overload the typed solve, gauge, and eigen exits on that carrier. Convert `SparseHermitian`'s stored upper plane to `CompressedColumnStorage<Complex>` so `CSparse.Helper.ValidateStorage` replaces the deleted CSR helper pair.

Delta: -34 target LOC, -1 module-level type, and -20 module-level members net after adding `MatrixKernel.Sparse`.

Ripples: Replace `SparseMatrix.FromTriplets`, wrapper fields, and wrapper algebra with `MatrixKernel.Sparse` and CSC members in `libs/dotnet/Rasm/.planning/Meshing/dec.md`, `Meshing/mesh.md`, `Meshing/reconstruct.md`, `Meshing/skeleton.md`, `Numerics/spectral.md`, `Parametric/nurbs.md`, `Parametric/subdivide.md`, `Processing/flatten.md`, `Processing/geodesics.md`, `Processing/sample.md`, and `Processing/segment.md`.

# 4. Delete the derived solve-trait lattice

From:
`matrix.md:48-70`
```csharp
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
        CapabilitySet<SolveTrait>.Of(SolveTrait.Direct)));
    internal static CapabilitySet<SolveTrait> Admit(params ReadOnlySpan<SolveTrait> held) =>
        Routes.Admit(held: CapabilitySet<SolveTrait>.Of(held)).ThrowIfFail();
}
```
To:
```csharp
// SolveTrait DELETED
```
Why: Density, scalar field, squareness, transposition, and iterative discipline are already determined by the operator type and selected algorithm. The trait sets duplicate the route roster, while the only behavioral reads re-check input shape and whether a transposed label was attached.

Change: Delete `SolveTrait`, both `Traits` properties, every `traits:` constructor argument, the `DenseGate` trait read, and the `LinearSolution` transpose-trait check. Admit shape and direction at the operation that owns them.

Delta: -28 target LOC, -1 module-level type, and -12 module-level members.

# 5. Put residual policy on the solve path

From:
`matrix.md:73-79`
```csharp
[SmartEnum]
public sealed partial class ResidualCap {
    public static readonly ResidualCap Converged = new(floor: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
    public static readonly ResidualCap Relaxed = new(floor: Math.Sqrt(d: EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
    public double Floor { get; }
    public ToleranceLane Lane { get; }
    public double In(Option<Context> context) => context.Map(model => model.For(lane: Lane).Value).IfNone(noneValue: Floor);
}
```
To:
```csharp
public double DefaultTolerance { get; }
public ToleranceLane Lane { get; }
public double Tolerance(Option<Context> context) =>
    context.Map(model => model.For(Lane).Value).IfNone(DefaultTolerance);
```
Why: `ResidualCap` has only two rows and no identity outside `SolvePath`; it is a parallel vocabulary for two route columns. Moving the values to the consuming path makes the threshold one hop from the selected algorithm.

Change: Delete `ResidualCap`; replace each `cap:` with `defaultTolerance:` and `lane:`, `.Cap.Floor` with `.DefaultTolerance`, and `.Cap.In(context)` with `.Tolerance(context)`.

Delta: -4 target LOC, -1 module-level type, and -2 module-level members net.

Ripples: Replace `SolvePath.SparseLdl.Cap.In(context)` with `SolvePath.SparseLdl.Tolerance(Some(context))` in `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md`.

# 6. Delete the conditioning delegate column

From:
`matrix.md:150-157`
```csharp
public static readonly SolvePath SparseKrylov = new(key: 7,
    traits: SolveTrait.Admit(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Converged, conditioned: static () => DenseFallback);
public static readonly SolvePath DenseFallback = new(key: 8,
    traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Fallback), cap: ResidualCap.Relaxed, conditioned: static () => DenseFallback);

public CapabilitySet<SolveTrait> Traits { get; }
public ResidualCap Cap { get; }
[UseDelegateFromConstructor] public partial SolvePath Conditioned();
```
To:
```csharp
public static readonly SolvePath SparseKrylov = new(
    key: 7, defaultTolerance: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
public static readonly SolvePath DenseFallback = new(
    key: 8, defaultTolerance: Math.Sqrt(EpsilonPolicy.SqrtEpsilon), lane: ToleranceLane.Krylov);
```
Why: Only two fallback sites exist: dense LU to QR and KKT LDL to LU. The other successor values are never read, while terminal rows point to themselves solely to satisfy the delegate column.

Change: Delete every `conditioned:` argument, `SolvePath.Conditioned`, and `MatrixKernel.Conditioned`; bind QR and LU recovery directly at their two owning call sites.

Delta: -10 target LOC and -2 module-level members.

# 7. Generate Krylov policy admission

From:
`matrix.md:110-118`
```csharp
public readonly record struct KrylovPolicy(
    SparsePreconditioner Preconditioner, KrylovSolver Solver, double Tolerance, Dimension Budget,
    Option<KrylovStop> Stop, bool CanFallback) {
    public static Fin<KrylovPolicy> Of(SparsePreconditioner preconditioner, double tolerance, Dimension budget,
        Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, bool canFallback = false) =>
        from _ in Admit.Finite(value: tolerance)
        from gated in guard(tolerance > 0.0, new KernelFault.InvalidInput())
        select new KrylovPolicy(Preconditioner: preconditioner, Solver: solver.IfNone(noneValue: KrylovSolver.BiCgStab),
            Tolerance: tolerance, Budget: budget, Stop: stop, CanFallback: canFallback);
```
To:
```csharp
[ComplexValueObject]
public sealed partial class KrylovPolicy {
    public SparsePreconditioner Preconditioner { get; }
    public KrylovSolver Solver { get; }
    public PositiveMagnitude Tolerance { get; }
    public Dimension Budget { get; }
    public Option<KrylovStop> Stop { get; }
    public bool CanFallback { get; }
```
Why: The positional constructor bypasses `Of`, so an invalid tolerance is representable. `PositiveMagnitude` already owns the finite-positive invariant, and `[ComplexValueObject]` generates one constructor/factory plane without obscuring the genuine solver and preconditioner case families.

Change: Convert `KrylovPolicy` to a generated complex value object, use `PositiveMagnitude`, delete `Of`, preserve the budget derivations inside the type, and read `Tolerance.Value` in stop criteria and residual gates.

Delta: -2 handwritten target LOC, -1 module-level factory, and -1 bypassing public constructor.

# 8. Return numerical refusal on the result failure branch

From:
`matrix.md:177-193`
```csharp
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
```
To:
```csharp
// SolveStop and EigenSolveStop DELETED
```
Why: The mint currently returns `Fin.Succ` for non-usable stops while the value's own `IsValid` is false. Rank deficiency, iteration exhaustion, and residual rejection are expected, matchable failures; successful stop labels are derivable from the path and add no evidence.

Change: Delete both stop vocabularies and result fields. Add `KernelFault.RankDeficient`, `KernelFault.IterationLimit`, and `KernelFault.ResidualExceeded` cases and return them before constructing a solution.

Delta: -22 target LOC, -2 module-level types, and -15 module-level members including the two result fields.

Ripples: Add the three fault cases in `libs/dotnet/Rasm/.planning/Domain/results.md`; remove stop handling from `Parametric/nurbs.md`, `Spatial/neighbors.md`, and `Processing/segment.md`, whose existing `Bind` then receives only admitted solutions.

# 9. Keep only measured linear-solution evidence

From:
`matrix.md:590-610`
```csharp
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
```
To:
```csharp
public readonly record struct LinearSolution(
    Arr<double> Solution, SolvePath Path, PathEvidence Evidence,
    double Residual, double Tolerance, Option<GaugeFix> Gauge = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Solution.AsSpan()),
        Residual >= 0.0 && Residual <= Tolerance,
        Evidence.Holds(Residual),
        ValidityClaim.Evidence(Gauge));
}
```
Why: Sense, extents, and right-hand-side length are admitted inputs, not persistent solve evidence. `InputNonZeros` has no consumer, and the actual accepted tolerance must be stored instead of recomputing a route default that can disagree with a caller-supplied Krylov tolerance.

Change: Gate sensed input and output lengths in the solve exit; remove `Stop`, `Sense`, `Rows`, `Cols`, `RhsLength`, and `InputNonZeros`; store the actual `Tolerance` used by the operation.

Delta: -10 target LOC and -6 module-level stored members.

# 10. Delete redundant eigen order and pair counts

From:
`matrix.md:613-626`
```csharp
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
```
To:
```csharp
public readonly record struct EigenSolution<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path,
    PathEvidence Evidence, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Pairs.Count > 0,
        ValidityClaim.Nonnegative(MaxResidual),
        Evidence.Holds(MaxResidual));
}
```
Why: `ReturnedPairs` is always `Pairs.Count`; `RequestedPairs` repeats an admitted input; and `Order` is only a producer-written label, not a proof that the sequence is sorted. Every entry already defines its output order, so `PairsIn` merely checks a redundant label.

Change: Delete `EigenOrder`, the three result fields, and `PairsIn`; gate `pairs.Count <= requestedPairs` before minting and let each entry's documented output order be its contract.

Delta: -12 target LOC, -1 module-level type, and -7 module-level members.

Ripples: Read `Pairs` directly in `libs/dotnet/Rasm/.planning/Meshing/dec.md`, `Processing/flatten.md`, `Processing/flow.md`, `Processing/segment.md`, `Solving/fit.md`, `Spatial/cloud.md`, `Spatial/fields.md`, and `Spatial/neighbors.md`; retain any consumer-required sorting at the producing eigen entry.

# 11. Replace the trivial Krylov plan record with a named tuple

From:
`matrix.md:132,573`
```csharp
public readonly record struct KrylovPlan(SparsePreconditioner Preconditioner, KrylovSolver Solver);
public sealed record Iterative(int Iterations, Dimension Budget, double Tolerance, Option<KrylovPlan> Plan) : PathEvidence;
```
To:
```csharp
public sealed record Iterative(int Iterations, Dimension Budget, double Tolerance, Option<(SparsePreconditioner Preconditioner, KrylovSolver Solver)> Plan) : PathEvidence;
```
Why: `KrylovPlan` carries no invariant or behavior and exists only as the payload of one `Option`. The named tuple states the same atomic pair at the one owning case.

Change: Delete `KrylovPlan`; replace `Option<KrylovPlan>` with the named tuple and construct the tuple directly at the iterative solve exit.

Delta: -1 target LOC, -1 module-level type, and -3 module-level generated record members.

# 12. Delete generated-union projection wrappers

From:
`matrix.md:575-580`
```csharp
public Option<int> Iterations => Switch(
    direct: static _ => Option<int>.None, factored: static _ => Option<int>.None,
    ranked: static _ => Option<int>.None, iterative: static path => Some(path.Iterations));
public Option<int> FactorNonZeros => Switch(
    direct: static _ => Option<int>.None, factored: static path => Some(path.FactorNonZeros),
    ranked: static path => Some(path.FactorNonZeros), iterative: static _ => Option<int>.None);
```
To:
```csharp
// PathEvidence.Iterations and PathEvidence.FactorNonZeros DELETED
```
Why: Both properties are direct projections of public union cases, and `FactorNonZeros` has no consumer. Thinktecture already generates the exhaustive dispatch needed by the one remaining iteration read.

Change: Delete both properties and use the generated exhaustive `Switch` at the sole iteration consumer.

Delta: -6 target LOC and -2 module-level members.

Ripples: Extract the iterative count through `PathEvidence.Switch` in `libs/dotnet/Rasm/.planning/Processing/flatten.md`.

# 13. Delete the matrix-norm dispatch wrapper

From:
`matrix.md:196-202`
```csharp
[SmartEnum]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(compute: static m => TensorPrimitives.Norm<double>(m.Entries.AsSpan()));
    public static readonly MatrixNormKind MaxAbs = new(compute: static m => m.Entries.Count == 0 ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(m.Entries.AsSpan())));
    public static readonly MatrixNormKind L1 = new(compute: static m => MatrixKernel.ToMathNet(m).L1Norm());
    public static readonly MatrixNormKind LInf = new(compute: static m => MatrixKernel.ToMathNet(m).InfinityNorm());
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
}
```
To:
```csharp
// MatrixNormKind DELETED
```
Why: The four rows rename `TensorPrimitives` and MathNet norm members. The only internal read is Frobenius scale, which both MathNet and CSparse publish directly.

Change: Delete `MatrixNormKind`; call `FrobeniusNorm`, `L1Norm`, `InfinityNorm`, or the required `TensorPrimitives` reduction at the numerical site.

Delta: -8 target LOC, -1 module-level type, and -6 module-level members.

# 14. Replace the handwritten definiteness sweep with MathNet Cholesky

From:
`matrix.md:701-711`
```csharp
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
```
To:
```csharp
// SymmetricMatrix.Definite, MatrixKernel.DefiniteSweep, and MatrixKernel.DefiniteCore DELETED
```
Why: The two-method sweep reimplements Cholesky only to discard the factor. MathNet already owns Cholesky, and `Try.lift` converts its expected factorization refusal into the ambient result carrier.

Change: Delete `SymmetricMatrix.Definite`, both sweep members, and `StackScratchCells`; bind `Try.lift(() => symmetric.ToDense().Cholesky()).Run().Map(_ => unit)` at the sole definiteness consumer.

Delta: -31 target LOC and -4 module-level members.

Ripples: Replace `tensor.Definite()` with the MathNet Cholesky gate in `libs/dotnet/Rasm/.planning/Numerics/calculus.md`.

# 15. Derive gauge case state in one dispatch

From:
`matrix.md:1131-1149`
```csharp
Option<Arr<double>> weights = gauge.Switch(
    pin: static p => p.Mass, meanZeroDeflation: static d => d.Mass, lagrangeKKT: static k => k.Mass);
Matrix<double> nullspace = gauge.Switch(
    state: n,
    pin: static (dim, p) => DenseMatrixD.OfColumnVectors([.. p.Indices.AsIterable().Select(index => DenseVectorD.Create(dim, i => i == index ? 1.0 : 0.0))]),
    meanZeroDeflation: static (_, d) => BasisColumns(basis: d.Nullspace), lagrangeKKT: static (_, k) => BasisColumns(basis: k.Nullspace));
GaugeShift shift = gauge.Switch(
    pin: static p => p.PostShift, meanZeroDeflation: static d => d.PostShift, lagrangeKKT: static k => k.PostShift);
Arr<int> pinIndices = gauge.Switch(
    pin: static p => p.Indices, meanZeroDeflation: static _ => new Arr<int>([]), lagrangeKKT: static _ => new Arr<int>([]));
double compatibility = gauge.Switch(
    state: (Nullspace: nullspace, Rhs: b),
    pin: static (_, _) => 0.0,
    meanZeroDeflation: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm(),
    lagrangeKKT: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm());
```
To:
```csharp
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
```
Why: Six exhaustive dispatches over the same discriminant reconstruct one operation state and repeat identical deflation and KKT arms. One generated `Switch` should derive all case-dependent inputs once.

Change: Derive mass input, nullspace, shift, pin indices, compatibility, and projection posture in one `GaugePolicy.Switch`; retain only the later dispatch selecting the solve algorithm.

Delta: -12 target LOC, 0 module-level symbols, and -5 generated-dispatch call sites net.

# 16. Remove duplicate gauge evidence and use numerical terms

From:
`matrix.md:629-633`
```csharp
public readonly record struct GaugeFix(
    SolvePath Path, int NullspaceDim, Option<int> NullspaceDimNumeric,
    double OperatorScale, double ResidualCompatibility, double ResidualAfterGauge, double ResidualAfterGaugeM,
    double ResidualRelative, Arr<int> PinIndices, int ConstraintRows, GaugeShift PostShiftApplied,
    double RhsMutationNorm, Option<double> MultiplierNorm, double GaugeOrthogonality, double RegularizationEps) : IValidityEvidence {
```
To:
```csharp
public readonly record struct GaugeFix(
    int Nullity, Option<int> BasisRank,
    double OperatorScale, double CompatibilityResidual, double GaugeResidual, double MassResidual,
    Arr<int> PinIndices, GaugeShift Shift, double RhsMutationNorm,
    Option<double> MultiplierNorm, double GaugeOrthogonality, double Regularization) : IValidityEvidence {
```
Why: `Path` duplicates `LinearSolution.Path`, `ResidualRelative` duplicates `LinearSolution.Residual`, and `ConstraintRows` always equals `NullspaceDim`. `Nullity`, `BasisRank`, and `Regularization` are the canonical numerical terms; the `AfterGauge`, `Applied`, and `Eps` suffixes add no distinction.

Change: Delete the three duplicate fields, rename the retained evidence as shown, and update construction and validity clauses.

Delta: -4 target LOC and -3 module-level stored members.

Ripples: Replace `PostShiftApplied` with `Shift` in `libs/dotnet/Rasm/.planning/Processing/sample.md`.

# 17. Delete the unused destructive factor-movement surface

From:
`matrix.md:516-533`
```csharp
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
```
To:
```csharp
// CholeskySparse.Refactorize, Sweep, Update, Downdate, Move, and SharesPattern DELETED
```
Why: No consumer uses refactorization or rank-one movement. A failed CSparse update or downdate has already mutated the standing factor, while this wrapper returns the same apparently valid object; the unused surface cannot represent that destructive failure safely.

Change: Delete the six `CholeskySparse` members plus `MatrixKernel.RefactorSweep` and `RankOneMoved`; make `Source` immutable and retain the lock-guarded repeated-right-hand-side solve.

Delta: -59 target LOC and -8 module-level members.

# 18. Use the iterative solve overload that returns its verdict

From:
`matrix.md:1008-1009,1026-1029`
```csharp
MathNet.Numerics.LinearAlgebra.Solvers.IPreconditioner<double> preconditioner = active.Preconditioner.Create();
preconditioner.Initialize(matrix: a);
LinearVector iterate = a.SolveIterative(input: b, solver: active.Solver.Create(), iterator: iterator, preconditioner: preconditioner);
double iterativeResidual = RelativeResidual(a: a, x: iterate, b: b);
bool converged = iterator.Status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged
    && double.IsFinite(iterativeResidual) && iterativeResidual <= active.Tolerance;
```
To:
```csharp
LinearVector iterate = DenseVectorD.Create(a.ColumnCount, 0.0);
MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus status = a.TrySolveIterative(
    b, iterate, active.Solver.Create(), iterator, active.Preconditioner.Create());
double iterativeResidual = RelativeResidual(a, iterate, b);
bool converged = status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged
    && double.IsFinite(iterativeResidual) && iterativeResidual <= active.Tolerance.Value;
```
Why: MathNet initializes the preconditioner inside iterative solve, so the explicit initialization duplicates work. `TrySolveIterative` returns the verdict directly; reading it again from the supplied iterator duplicates one fact.

Change: Reuse one result vector, call `TrySolveIterative`, branch on its returned status, and retain the iterator only for the criterion fold and measured iteration count.

Delta: 0 target LOC, 0 module-level symbols, and -1 duplicate preconditioner initialization.

# 19. Use DoubleDouble's aggregation surface

From:
`matrix.md:785-789`
```csharp
private static double CompensatedNorm(LinearVector v) {
    ddouble sum = 0.0;
    for (int i = 0; i < v.Count; i++) sum += (ddouble)v[i] * v[i];
    return Math.Sqrt(d: (double)sum);
}
```
To:
```csharp
private static double CompensatedNorm(LinearVector vector) =>
    Math.Sqrt((double)vector.Enumerate()
        .Select(static value => (ddouble)value * value)
        .Sum());
```
Why: `DoubleDoubleEnumerableExpand.Sum` is the admitted package's 106-bit aggregation. The manual accumulator is not a measured span kernel and duplicates the package fold.

Change: Replace the loop with the package aggregation over MathNet's lazy vector enumeration.

Delta: -1 target LOC, 0 module-level symbols, and -1 imperative loop.

# 20. Inline the sole Hermitian real-block expansion

From:
`matrix.md:955-965`
```csharp
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
```
To:
```csharp
// MatrixKernel.AddHermitianRealBlockTriplets DELETED
```
Why: The list overload is unused and the action overload has one consumer. The twelve-entry real representation is directly expressible at that consumer's assembly boundary and does not justify two kernel members.

Change: Delete both overloads and write the twelve `CoordinateStorage<double>.At` calls at the sole assembly site.

Delta: -11 target LOC and -2 module-level members; the consumer gains 3 LOC, for -8 project LOC net.

Ripples: Inline the block entries at `libs/dotnet/Rasm/.planning/Meshing/dec.md:284` and remove the helper from that page's package roster.

# 21. Delete package-conversion aliases

From:
`matrix.md:777-778`
```csharp
private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());
private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());
```
To:
```csharp
// MatrixKernel.ArrFromVector and MatrixKernel.ArrFromComplexVector DELETED
```
Why: Both members only rename `Vector<T>.ToArray`; `Densified` and `TakeSmallest` likewise each hide one package expression without owning an invariant, policy, or repeated expensive work.

Change: Inline the two `Arr` constructions, the two generalized-eigen dense copies, and the sole `DenseOfColumnVectors` selection; delete `ArrFromVector`, `ArrFromComplexVector`, `Densified`, and `TakeSmallest`.

Delta: -7 target LOC and -4 module-level members.
