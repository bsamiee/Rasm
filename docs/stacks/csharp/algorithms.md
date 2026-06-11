# [ALGORITHMS]

Numeric work is admitted once and routed by shape: raw operands cross into a finite-checked owner at one boundary, the interior is total over admitted values, and the operand's structure — definite, square, overdetermined, symmetric, sparse-pattern, periodic-grid — selects the owning factorization, never the call site and never a knob riding beside the matrix. Every solve route — dense direct, sparse direct, iterative, every conditioning fallback — is one composed lifecycle admit → route → solve → witness → receipt written once; a per-module pipeline re-deriving the chain is the rejected form. MathNet owns dense factorization, the spectral decompositions, `Fourier`, and quadrature; CSparse owns sparse direct factorization, factor reuse, and fill-reducing ordering once a matrix is admitted; the owned-build lane — low-discrepancy sampling, ODE tableaux, spectral multipliers, block eigeniteration, scattered reconstruction — has no library surface and is composed from the rails. Every library refuses its own gates — no constructor checks finiteness, symmetry is tested by exact `!=`, singularity is never asserted, a zero-norm `QR` fills `NaN` while `IsFullRank` returns `true` — so admission re-imposes each refused gate as an explicit predicate. Every result leaves as a domain receipt carrying its route variant, its scale-derived tolerance policy, and the recomputed true relative residual against the original operator — the one correctness signal that survives preconditioning, breakdown substitution, cancellation, and provider divergence — never a `Matrix<T>`, `Vector<T>`, or factorization instance. A hand-rolled kernel is admitted only after a benchmark defeats both libraries.

## [1]-[ROUTE_SPINE]

The operand shape selects the route before any solve; the most specific shape wins, and the table is one lifecycle's route step, never thirteen pipelines.

| [INDEX] | [OPERAND_SHAPE]                          | [OWNING_ROUTE]                       | [CONDITIONING_FALLBACK]                |
| :-----: | :--------------------------------------- | :----------------------------------- | :------------------------------------- |
|   [1]   | dense symmetric positive-definite        | `Cholesky()`                         | `ε·I` shift, then `Evd`                |
|   [2]   | dense square general                     | `LU()`                               | rank-revealing `Svd(true)`             |
|   [3]   | dense overdetermined                     | thin `QR()`                          | `Svd(true)` truncated pseudo-inverse   |
|   [4]   | tiny system below sparse crossover       | dense factorization-as-probe         | managed path beats provider switch     |
|   [5]   | symmetric or Hermitian spectrum          | `Evd(Symmetricity.Symmetric)`        | congruence-reduced generalized `Evd`   |
|   [6]   | nonsymmetric spectrum                    | `Evd(Symmetricity.Unknown)`          | block residual witness, no resort      |
|   [7]   | rank, norm, or condition evidence only   | retained `Svd(false)` handle         | one handle answers all three           |
|   [8]   | sparse SPD, fixed pattern                | `SparseCholesky` + `AMD`             | rank-1 downdate fails → reconstruct    |
|   [9]   | sparse symmetric indefinite              | `SparseLDL`                          | `BiCgStab` while structure unproven    |
|  [10]   | sparse nonsymmetric or uncertain         | `SparseLU` (column-relative `tol`)   | iterative solve + witness gate         |
|  [11]   | sparse rectangular least-squares         | `SparseQR`                           | augmented-dimension solution sizing    |
|  [12]   | huge sparse or changing pattern          | `TrySolveIterative` + criterion stack | verdict-routed direct solve            |
|  [13]   | constant-coefficient periodic grid       | owned spectral multiplier            | regularization at the vanishing symbol |

Recover the fallback from the route value and record it in the receipt, never a caller-named entrypoint: a fill ratio — symbolic factor nonzeros over input nonzeros, read before the numeric sweep — routes direct versus iterative, and the augmented regression case (the design matrix stacked over a `√λ`-scaled identity under thin `QR`) is the derived consequence of the conditioning budget exceeding the inverse cap. MathNet exposes no CSparse surface, so hybrid routing is integrator-authored and carries its own residual validation. The fallback is a rebind onto the same lifecycle: primary and conditioning routes converge on the one witness gate, and the receipt records the taken path.

[ROUTE_UNION]:
- Law: discriminate the factorization on the operand through a `FactorRoute` `[Union]` carrying its mode, symmetricity, vector demand, and rank-tolerance convention as case data recoverable by matching the value.
- Reject: a `bool computeVectors`, `QRMethod`, or `Symmetricity` parameter riding beside the matrix; a parallel knob re-describing the input is arity smuggled back.
- Law: seat modified Gram-Schmidt and the one rank-tolerance convention inside the `Orthonormal` case — orthogonalization method is a QR discriminant and the constructor collapses the five built-in absolute, magnitude-squared, and scale-relative thresholds into one — never a sixth sibling factory.
- Boundary: the element carrier is monomorphic `double`; the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative.

```csharp conceptual
[Union]
public abstract partial record FactorRoute
{
    public sealed partial record DefinitePsd(Matrix<double> A) : FactorRoute;
    public sealed partial record SquarePivoting(Matrix<double> A) : FactorRoute;
    public sealed partial record Orthonormal(Matrix<double> A, QRMethod Mode, bool Modified) : FactorRoute;
    public sealed partial record Spectral(Matrix<double> A, Symmetricity Sym) : FactorRoute;
    public sealed partial record RankRevealing(Matrix<double> A, bool Vectors) : FactorRoute;
}

public static class DenseRoute
{
    public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> b, double cap) =>
        route.Switch(
            definitePsd:    c => Admit(c.A).Map(a => (A: a, S: (ISolver<double>)a.Cholesky())),
            squarePivoting: c => Admit(c.A).Map(a => (A: a, S: (ISolver<double>)a.LU())),
            orthonormal:    c => Admit(c.A).Map(a => (A: a, S: (ISolver<double>)a.QR(c.Mode))),
            spectral:       c => Admit(c.A).Map(a => (A: a, S: (ISolver<double>)a.Evd(c.Sym))),
            rankRevealing:  c => Admit(c.A).Map(a => (A: a, S: (ISolver<double>)a.Svd(true))))
        .Bind(t => Capped(t.A, t.S.Solve(b), b, cap));
}
```

## [2]-[ADMISSION_GATES]

[FINITE_ADMISSION]:
- Law: gate every operand on one all-finite predicate over the flat column-major `Values` span before factoring, with NaN-any and Inf-any early-exit variants when the typed rejection must name its cause; no constructor rejects `NaN` or `Inf`, a non-finite entry propagates silently into corrupted factor state, and a strided per-element loop forfeits the one-pass vectorized admission the layout grants.

[SYMMETRY_FORCING]:
- Law: force symmetry with `(A + A.Transpose()) * 0.5` before the call (`ConjugateTranspose` for Hermitian); `IsSymmetric()` compares entries with exact `!=`, so accumulation-built matrices fail it and `Symmetricity.Unknown` falls to the asymmetric spectral path whose solve throws.
- Reject: `MapIndexedInplace` self-averaging; it mutates the backing array sequentially, so a mirror entry is already modified when read.
- Boundary: a one-ULP mirror difference silently acquires the full nonsymmetric contract — block-diagonal `D`, non-orthonormal columns, spurious complex pairs.
- Boundary: the definite kernel reads only the upper triangle, certifying solely the upper-symmetrized matrix; symmetrize first, never substitute a separate eigenvalue probe.

[SINGULAR_AND_ZERO_NORM]:
- Law: probe the factor diagonal for near-zero entries and read the streaming log-determinant as `2·Σ log L[i,i]`; `LU()` on a singular input yields `Determinant == 0.0` and never throws, the determinant product alone underflows to zero with no signal, and the plain determinant's logarithm is `−Inf`.
- Law: test reflection as `det < 0.0`, never `det != 1.0`, since a rotation's determinant is only approximately ±1.
- Law: check the factor buffers all-finite after a `QR` construction; a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`, the throw firing only at a bit-exact-zero norm.
- Use: thin Householder `QR` over single-pass Gram-Schmidt in near-dependent columns; the probe `qr.R.Diagonal().Any(v => Math.Abs(v) < floor)` lazily materializes `R`.

## [3]-[DENSE_FACTOR_LAW]

[RANK_AND_TOLERANCE]:
- Law: derive every threshold from operator and right-hand-side scale (`σ_max`, `‖A‖_F`, `‖b‖∞`) and travel it as one named policy record on the receipt; a bare per-module absolute literal in `1e-4..1e-8` is the rejected form, unreplayable and uncomparable across operators.
- Law: carry the `Svd<T>.Rank` (`σ_max.EpsilonOf() · max(m, n)`) as the conditioning rank; never share its slot with `Evd<T>.Rank` (`AlmostEqual` at `DefaultDoubleAccuracy`), and treat any spectral rank as a definiteness signature only.
- Law: recompute domain rank as `n − count(|λᵢ| < ε_rank · max|λⱼ|)` with a caller-supplied spectral-radius-relative `ε_rank`; the built-in absolute `|λ| < ~3.33e-8` zero test misclassifies significant eigenvalues for `‖A‖₂ ~ 10⁶`.
- Law: build validity predicates on `EpsilonOf(value)` and store the pre-computed residual diff beside the threshold; `value * DoublePrecision` diverges at binade boundaries (`EpsilonOf(2.5) = 2⁻⁵¹` versus `6.95e-16`), and `AlmostEqualNormRelative` hides relative, absolute-on-tiny, and absolute-on-zero behind `IsRelative: true`.
- Boundary: guard `ConditionNumber` against `+Inf` before gating; it is `+Inf` for rank-deficient operators.

[HELD_HANDLE]:
- Law: retain one `Svd(false)` handle for rank, `L2Norm`, and `ConditionNumber`; each base-matrix query otherwise builds a fresh cubic decomposition, while the cheap induced norms stay quadratic provider reductions touching no decomposition.
- Law: stream N right-hand sides through one held factorization as N triangular solves into the in-place overload, the hot-route allocation contract; a loop of fresh factorizations is the rejected form, and the allocating overload appears only where a value leaves the boundary.
- Use: the definite handle's in-place block-copy privilege to refactor a streaming operator; every other handle demands a fresh construction per operand, and the per-construction clone is the controlling cost at small `n`.
- Reject: `Inverse()` in a hot loop — it clones the factors plus an `n²` identity, crossing the large-object threshold at `n ≥ 104`; solve against an identity through the retained pivoting handle with reused result and right-hand-side buffers.
- Boundary: form an iterative-refinement residual against the original operator in working precision, never against reconstructed factors — the factors carry exactly the rounding error the correction exists to cancel.

[GRAM_WEIGHTING]:
- Law: apply weighting as one policy row through the typed diagonal operand — its fast path scales each row with no `m×n` intermediate — never per-module `√`-weight row scaling re-derived before each Gram; a raw-array weight has no fast path and forfeits the structural route.
- Law: select the damped normal form and the augmented `√λ`-stacked form as two cases of one route on a conditioning-budget field; the Gram-plus-ridge form squares `κ` through the densifying transpose-multiply, and the stacked-identity thin `QR` avoids the squaring when condition exceeds the inverse cap.

## [4]-[SPARSE_DIRECT_LAW]

[CSC_OWNER]:
- Law: own one `CompressedColumnStorage<T>`; convert triplet exactly once and derive the row orientation by one `Transpose` at construction, never reconverting among the three formats.
- Reject: `inplace: true` conversion when the triplet must survive a structural-edit increment; it calls `Invalidate()`, nulling arrays and dangling references.
- Boundary: the row-format bridge factory accepts column-format arrays without type rejection but with transposed semantics, yielding silently wrong preconditioner factors.
- Boundary: run strict storage validation before factoring; it returns `bool` and never throws, and factorizing invalid storage produces silently incorrect factors.

[FILL_AND_ORDERING]:
- Law: read the symbolic factor nonzeros before the numeric sweep to route direct versus iterative; the count is per-kind — one factor for the symmetric kinds, an `L + U − n` sum for `SparseLU`, a `Q + R − m` sum for `SparseQR` — so a bare fill integer compared across kinds is meaningless.
- Law: cache the `AMD.Generate` permutation `int[]` as the value-only refactor key over an invariant pattern; no entrypoint accepts a prebuilt symbolic record plus fresh values, so the durable reuse unit is the whole factorization reconstructed with the cached permutation.
- Boundary: drop assembly residue with a structural tolerance near `machineEps · ‖A‖_F`; `DropZeros()` defaults to `0.0` and removes only binary zeros, while `CoordinateStorage.At` skips exact-zero contributions so the post-assembly count is the nonzero count.

[KIND_CAPABILITY]:
- Law: recover transpose-solve, rank-1 edit, inertia, and reentrancy from the factor kind alone; the shared solver interface exposes only the forward solve, so a request needing edit plus transpose closes over the concrete kind.
- Reject: a typed-only catch at the factorization boundary; SPD pivot loss and the zero-diagonal break throw bare `Exception`, not the numerical-breakdown type, so the boundary catches broadly and converts.
- Boundary: an asymmetric input to a symmetric kind factors as its symmetrization and returns a correct answer to the wrong system — the post-solve true residual is the only structural signal.
- Boundary: set the `SparseLU` pivot `tol` in `[0, 1]` as a relative column threshold — `1` full partial pivoting, `0` disabled — never an absolute floor.

| [FACTOR_KIND]            | [RANK1_EDIT] | [TRANSPOSE_SOLVE] | [INERTIA] | [REENTRANT] |
| :----------------------- | :----------: | :---------------: | :-------: | :---------: |
| `SparseCholesky` SPD     |     yes      |        no         |     —     |     no      |
| `SparseLDL` symmetric    |      no      |        no         |  private  |     no      |
| `SparseLU` unsymmetric   |      no      |        yes        |     —     |     no      |
| `SparseQR` rectangular   |      no      |        yes        |     —     |     yes     |

[FACTOR_CACHE]:
- Law: collapse a completed factorization to one typed operator value owning the factorization instance, cached permutation, symbolic fill counts, solution dimension, and kind discriminant.
- Law: the solve returns a bare vector with no status, so the post-solve residual witness against the original operator belongs to the operator value, never the call site.
- Law: key the cache on dimensions, sparsity fingerprint (hash of the pointer and index arrays), and ordering identity, populating it success-only so only residual-witnessed factorizations enter and a diverged solve never poisons reuse.
- Boundary: serialize solves on a cached square factorization — its one constructor-allocated scratch is non-reentrant and a concurrent second solve corrupts both results with no guard — through a capsule owning the factorization or a pattern-keyed instance pool.
- Boundary: size the rectangular kind's work buffer from the factorization's solution dimension, which exceeds the row count for structurally singular systems; sizing from the matrix shape is the off-by-augmentation fault.

[STRUCTURAL_EDIT]:
- Law: express every structural-edit dialect — pin, prune, rank-1 bump, revalue — as one `Edit` `[Union]` over three primitives: predicate compaction for removal, marker-array scatter for additive merge, tree-path walk along `parent[]` for symmetric rank-1 change.
- Law: discard and reconstruct on a `false` rank-1 result; the partial walk has already corrupted the factor, so a retry compounds corruption and a non-symmetric change routes to reconstruction regardless of magnitude.
- Boundary: batch more than one structural insertion through triplet accumulation plus a single re-conversion — each in-place insertion shifts the suffix at `O(nnz)` — and suppress the process-global exact-fit trim flag for the whole sequence, restoring it once.

```csharp conceptual
[Union]
public abstract partial record Edit
{
    public sealed partial record Pin(int Node) : Edit;
    public sealed partial record Prune(double Tolerance) : Edit;
    public sealed partial record Bump(int Sign, double[] Column) : Edit;
    public sealed partial record Revalue(double[] Values) : Edit;
}

public static class StructuralEdit
{
    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit) => edit switch
    {
        Edit.Bump b when op.Kind is FactorKind.Spd =>
            op.RankOne(b.Sign, b.Column)
                ? Fin.Succ(op)
                : Rebuild(op, edit),
        Edit.Revalue r => Fin.Succ(op with { Inner = Refactor(op.Permutation, r.Values) }),
        var structural => Rebuild(op, structural),
    };
}
```

## [5]-[ITERATIVE_AND_WITNESS]

[CRITERION_PRECEDENCE]:
- Law: construct the criterion stack explicitly in order `FailureStopCriterion`, `DivergenceStopCriterion`, `ResidualStopCriterion`, `IterationCountStopCriterion`; insertion order is precedence, so `Failure` first keeps `NaN` terminal and the default — residual after the cap — suppresses convergence on the final iteration.
- Law: size the count ceiling as a multiple of the expected Krylov dimension and the residual threshold relative to `‖b‖∞`; a `null` iterator silently substitutes a hardcoded `1000`-iteration cap, and an absolute residual floor misreads operator scale.
- Boundary: key the divergence window length on the operator's symmetry class; short windows fire spuriously through the non-symmetric oscillatory warm-up.

[BREAKDOWN_AND_WITNESS]:
- Law: treat recovery as a solver-kind property; the same near-zero inner-product denominator throws in one nonsymmetric solver, cancels the iterator in a second, and substitutes `1` and continues in a third, so swapping solvers swaps failure semantics with no call-site difference.
- Reject: the substitution path as the most dangerous — it certifies an arbitrary iterate under a normal verdict, and the ULP guard fails open on `NaN`.
- Law: admit an iterate only on the independently recomputed true relative residual against the original operator, the one gate both direct and iterative routes converge on; the converged verdict certifies only that the preconditioned residual fell below tolerance, and left preconditioning distorts the norm.
- Reject: the composite solver's externally passed preconditioner — its constructor resolves each setup's preconditioner internally, making the argument dead code.
- Boundary: initialize each preconditioner outside the solve and catch its throw there; the init throw otherwise escapes the verdict-returning entrypoint and leaves stale buffer content reading as an answer.
- Boundary: route divergence to a regularized retry or direct solve and count-exhaustion to a direct solve seeded with the partial iterate — the two non-convergence verdicts demand opposite responses.

```csharp conceptual
public static class Iterative
{
    static Iterator<double> Stack(int dim, double tol, bool symmetric) =>
        new(new IIterationStopCriterion<double>[]
        {
            new FailureStopCriterion<double>(),
            new DivergenceStopCriterion<double>(0.08, minimumIterations: symmetric ? 3 : 10),
            new ResidualStopCriterion<double>(tol),
            new IterationCountStopCriterion<double>(8 * dim),
        });

    public static Fin<Vector<double>> Solve(
        Matrix<double> a, Vector<double> b, IIterativeSolver<double> solver,
        IPreconditioner<double> pre, double tol, bool symmetric) =>
        Try.lift<(IterationStatus Verdict, Vector<double> X)>(() =>
            {
                var x = Vector<double>.Build.Dense(b.Count);
                return (a.TrySolveIterative(b, x, solver, Stack(b.Count, tol, symmetric), pre), x);
            }).Run()
            .Bind(t => Residual(a, b, t.X) is var r && double.IsFinite(r) && r <= tol
                ? Fin.Succ(t.X)
                : Fin.Fail<Vector<double>>(Error.New($"witness failed: verdict={t.Verdict} r={r}")));
}
```

```csharp conceptual
public static class Conditioned
{
    public static Fin<Vector<double>> Solve(FactorRoute primary, FactorRoute secondary, Vector<double> b, double cap) =>
        DenseRoute.Solve(primary, b, cap)
            .Bind(x => Gate(nameof(primary), primary, x, b, cap))
            .BindFail(_ => DenseRoute.Solve(secondary, b, cap).Bind(x => Gate(nameof(secondary), secondary, x, b, cap)));

    static Fin<Vector<double>> Gate(string path, FactorRoute route, Vector<double> x, Vector<double> b, double cap) =>
        Witness(route, x, b) is var r && double.IsFinite(r) && r <= cap
            ? Fin.Succ(x)
            : Fin.Fail<Vector<double>>(Error.New($"witness failed: path={path} r={r}"));
}
```

## [6]-[SPECTRAL_LAW]

[SYMMETRY_CONTRACT]:
- Law: carry a result `[Union]` with distinct dense-symmetric and dense-general cases; the `Symmetricity` flag selects five output axes together — eigenvector norm, real versus block-diagonal `D`, single-column versus column-pair encoding, ascending versus Schur-deflation order, and a working versus norm-gated solve.
- Law: decode a real conjugate pair from two adjacent columns dispatched on `Math.Sign(values[j].Imaginary)`; reading `Column(j)` whole discards the imaginary half, and the result must carry `EigenValues` to interpret `EigenVectors` because no parallel pairing array exists.
- Boundary: census conjugate pairs by counting nonzero `D` super-diagonals — `n − 2·pairs` real eigenvalues — the cheapest spectral-type probe before any eigenvector work.

```csharp conceptual
public static class SchurDecode
{
    public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values) =>
        Matrix<Complex>.Build.DenseOfColumns(
            Enumerable.Range(0, values.Count).Select(j =>
                Math.Sign(values[j].Imaginary) switch
                {
                    > 0 => packed.Column(j).Enumerate().Zip(packed.Column(j + 1).Enumerate(),
                               (re, im) => new Complex(re, im)),
                    < 0 => packed.Column(j - 1).Enumerate().Zip(packed.Column(j).Enumerate(),
                               (re, im) => new Complex(re, -im)),
                    _   => packed.Column(j).Enumerate().Select(re => new Complex(re, 0.0)),
                }));
}
```

[EIGENVECTOR_QUALITY]:
- Law: `Normalize(2)` each nonsymmetric column before any modal weight; recovered columns are raw triangular solutions with arbitrary per-column norms, and Hermitian eigenvectors are genuinely complex, so projecting them to real parts is incorrect.
- Law: detect a structurally meaningless eigenvector block from the operator norm itself; a zero accumulated Hessenberg-band norm returns the bare similarity transform with valid eigenvalues and a meaningless eigenvector block.
- Boundary: detect defectiveness by a rank or condition probe on `EigenVectors`; a Jordan block never throws and manifests as near-parallel columns.
- Boundary: never assert eigenvalue equality tighter than the convergence band; the exceptional-shift escape bakes the literal `0.964` into the last bits.

[EIGEN_RESIDUAL_AND_FILTER]:
- Law: compute the block defect `(A.Multiply(V) - V.Multiply(D)).FrobeniusNorm()`; no built-in eigen residual exists, and it is the one signal both the managed throw rail and the native in-band info-code rail surface identically.
- Law: retain the full eigenvector matrix from the always-all-`n` dense solve as the cached basis; each filter then pays one `SubMatrix` allocation, never the O(n³) resolve, and a cache hit requires matching `n` and `Symmetricity`.
- Law: own the eigenbasis filter family as a closed weight vocabulary — the eigen-space analog of the Fourier symbol row — excluding the zero mode (`λ < ε_zero ? 0.0 : f(λ)`, excluded never clamped) and carrying the weight sum as evidence so a fully-excluded spectrum fails rather than reads as a zero signal.
- Reject: the library `Determinant`, `Rank`, and `IsFullRank` in domain logic; `Determinant` short-circuits to `0.0` the moment any eigenvalue crosses the absolute zero test.
- Boundary: pin the managed provider once before first provider touch and treat provider identity as a result discriminant; the slot is process-static with no per-call override, only `DenseMatrix` reaches the native `EigenDecomp`, the managed `Evd` kernels are serial regardless of degree, and sign, ordering, and last bits differ across the seam.

## [7]-[RECEIPT_LAW]

[PROVENANCE_SNAPSHOT]:
- Law: snapshot the provider `ToString` tag, parallelism degree, and the two parallelization thresholds at solve construction; every kernel reads this ambient static at execution instant, so a receipt without it captures the question and the answer while discarding the machine that produced them.
- Law: derive bit-versus-envelope equality from the provider, degree, and threshold triple; the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering.
- Boundary: short-circuit provider-mismatched eigenvector comparison to span equivalence; backends agree only up to sign and span and choose different bases within degenerate eigenspaces.

[HONESTY_MARKERS]:
- Law: store every implicit algorithm choice that shaped a result as a named policy field; a consumer that switch-dispatches on it re-derives behavior and breaks re-admission.
- Law: record `Symmetricity` as the case supplied (the caller's verified precondition), not the library's inferred result; the receipt `IsSymmetric` bool merges `Symmetric` and `Hermitian` and loses the distinction.
- Reject: a `QRMethod.Thin` receipt where full-column-space reconstruction is required, and a `computeVectors: false` receipt read for vectors that leaves `U`/`VT` as identity-shaped stubs.
- Law: assert singular values descending at construction and re-admission, never restoring the order, since a re-sort desyncs every index-0-derived fact.

[ONLINE_STATISTICS]:
- Law: record the accumulator as a fact — running over the whole stream versus moving over a fixed window are distinct semantics, never one knob.
- Law: carry the Bessel-versus-population normalizer as a policy enum beside every stored deviation; unmarked mixing silently corrupts every downstream confidence computation.
- Law: merge partitions by the pure static `Combine` as a CAS-safe reduction, asserting the identity `combined.Count == a.Count + b.Count`.
- Boundary: parallel online moments accumulate to fourth order and serialize for distributed aggregation; the merge identity holds only to the floating-point merge envelope.
- Boundary: guard the stream at admission — one pushed `NaN` permanently poisons extrema and every moment with no reset — through the same all-finite predicate the operands cross.

[TERMINAL_PARTITION]:
- Law: partition the terminal status as `Converged | StoppedWithoutConvergence | (Diverged | Failure | Cancelled)`; mapping budget-exhausted to `Fin.Fail` destroys the caller's relaxed-criterion or different-preconditioner retry.
- Law: catch the numerical-breakdown child before its non-convergence parent; a parent-first ladder swallows the more specific signal.
- Law: map the singular-`U` index (message-string only) and the zero-pivot sparse status to a distinct singular-matrix domain error, not a blanket solver failure.
- Boundary: only `Expected` errors are wire-faithful — equality is by `Code` alone — so every exceptional error converts to a domain-coded expected error before serialization, and `HasCode` dispatch on a round-tripped error is sound where message-substring matching is not.

## [8]-[OWNED_BUILDS]

[QUADRATURE]:
- Law: choose the quadrature route as the accuracy decision — double-exponential, fixed Gauss-Legendre, adaptive Gauss-Kronrod, tensor-product cubature are four distinct kernels, not wrappers — and order is the secondary knob.
- Law: admit the integrand as a guarded delegate lifting each evaluation into an absence carrier; no route inspects returns for non-finiteness, a pole poisons the weighted sum silently, and the skipped evaluation counts in the receipt as coverage.
- Boundary: substitute infinite bounds only on the facade entry; the direct double-exponential kernel feeds infinity into abscissa evaluation and yields `NaN` weights.
- Use: the `L1` value-to-ratio cancellation channel as the free conditioning diagnostic; the short overload discards it.
- Boundary: record the terminated-at-budget case with its binding budget and residual; the three exhaustion mechanisms return best-so-far indistinguishable from convergence.

[INTEGRATOR_TABLEAU]:
- Law: validate the tableau at construction; `Create` returns an order-carrying `StepTableau` or a typed structural fault, row-sum consistency and the order conditions are definition-time facts, and verified order is the largest integer for which every condition holds — derived, never asserted.
- Law: write one step function over the minimal additive-module operations (addition, scalar scaling, step-scaled increment); it admits scalar, complex, fixed-rank vector, and grid-slab carriers, collapsing the scalar-versus-vector transcription-error class.
- Law: freeze adaptive control as one policy row — safety factor, step-ratio clamps, error-norm choice, and reject budget travel together — and read the next step off the receipt.
- Boundary: use the scaled two-pass error norm for large-magnitude state; the naive squared-sum-then-root overflows, and an infinity in the error channel is then attributable to norm policy.

[SAMPLING]:
- Law: draw stochastic samples seed-explicit over a state-serializable generator, the checkpoint-resume route for resumable sampling; the default thread-entropy source and the length-2048 parallel block fill are non-deterministic regardless of seeding.
- Law: build the low-discrepancy family in the owned lane — no surface exists — and carry the independent-versus-equidistributed discriminant as a type, since variance law, error bars, and convergence rate all fork on it and the state shapes do not unify.
- Law: accept the block exponent at the draw entrypoint, never a free count; non-power prefixes, dropped origins, and strided thinning degrade discrepancy with no diagnostic, and equidistribution holds only at power-of-base counts.
- Boundary: return a replicate family with cross-replicate variance and a Student bound; a single equidistributed estimate carries no recoverable spread.
- Law: fix the scramble as a variance-law policy row, never a boolean — only the digital-shift randomizer survives progressive extension across exponents.
- Law: carry the net quality parameter and per-coordinate-pair projection figure as structural evidence so gates reject on quality, not on slow convergence.

[SPECTRAL_OPERATOR]:
- Law: collapse every constant-coefficient periodic operator to one symbol applied pointwise to the forward transform; symbols are policy values, composition is pointwise multiplication before a single inverse, and a new operator is a new symbol row, never a new code path.
- Law: derive the split-spectrum wavenumber once at grid construction — ascending positives through Nyquist, then descending negatives, scaled by `2π/extent`; hand-indexing the bin number applies an aliased symbol past the half length silently.
- Boundary: fix the asymmetric scaling convention in the owner; the symmetric default cancels only on round trips and is the most common silent error.
- Boundary: zero the Nyquist bin for odd-order symbols, and route B-orthonormalization through `chol.Factor.LU()`, since `chol.Factor.Transpose()` yields the silently-wrong half-factor.
- Boundary: mark the result inadmissible on excess imaginary residual; a real-symbol operator owes a machine-zero imaginary part, and excess diagnoses broken Hermitian symmetry.

```csharp conceptual
public readonly record struct WaveAxis(int Length, double Extent)
{
    public double[] K() =>
        Generate.LinearRangeMap(0.0, 1.0, Length - 1.0,
            i => (i < (Length >> 1) + 1 ? i : i - Length) * (2.0 * Math.PI / Extent));

    public int Nyquist => (Length & 1) == 0 ? Length >> 1 : -1;
}

public static class SpectralOperator
{
    public static Complex[] Apply(Complex[] field, WaveAxis axis, Func<double, Complex> symbol, bool oddOrder)
    {
        var (k, n) = (axis.K(), axis.Nyquist);
        Fourier.Forward(field, FourierOptions.AsymmetricScaling);
        var driven = field.Select((c, i) => c * (oddOrder && i == n ? Complex.Zero : symbol(k[i]))).ToArray();
        Fourier.Inverse(driven, FourierOptions.AsymmetricScaling);
        return driven;
    }
}
```

[INTERPOLANT_AND_SCATTER]:
- Law: lift interpolant capability to a phantom type parameter or marker case so the unsupported call is unrepresentable; the contract advertises differentiation and integration through runtime booleans and throws on unsupported calls.
- Law: reconstruct scattered multi-dimensional fields as a radial-basis or polynomial design matrix into the rank-revealing regression route — no library surface exists, and a matrix-valued response reconstructs gradient and flux fields in one solve.
- Boundary: wrap interpolant evaluation in an absence carrier; the step interpolant returns `NaN` at sample points and the rational interpolant returns `NaN` below ULP, poisoning a gradient accumulator silently.
- Use: the serial kernel-density route for reproducibility; it parallelizes uncapped and is non-reproducible across schedules even when seeded.
