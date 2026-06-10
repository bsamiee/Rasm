# Solve Phase And Factored-Operator Reuse

[SOLVE_PIPELINE_DECOMPOSITION]:
- A completed direct factorization is not a matrix; it is a four-stage permute-solve-permute pipeline composed of pure span kernels and integer permutation gathers. `Solve` never touches the factorization input matrix again — it reads only the stored factor `L`/`U`/`R`, the diagonal `D`, and the symbolic permutation vectors. The factorization object is the closure; `Solve` is its application.
- Cholesky drives `ApplyInverse(pinv, b) → SolveLower(L) → SolveLowerTranspose(L) → Apply(pinv, x)`: a single symmetric permutation wraps a forward-then-back-transpose sweep over the one stored factor. LDL inlines its own three-stage scan over `L`/`D` between the same `ApplyInverse(pinv)`/`Apply(pinv)` bracket — forward unit-lower, elementwise divide by `D`, then back unit-lower-transpose — and reuses `pinv` on both ends. LU runs `ApplyInverse(pinv) → SolveLower(L) → SolveUpper(U) → ApplyInverse(q)`: row permutation on entry, column permutation on exit, two distinct vectors. QR composes Householder reflector application around `SolveUpper(R)` and branches on shape.
- The transpose solve is a literal reversal of the forward composition, not a refactorization: LU's `SolveTranspose` runs `Apply(q) → SolveUpperTranspose(U) → SolveLowerTranspose(L) → Apply(pinv)`, exactly mirroring the forward stage order and swapping each permutation for its dual. One factorization answers both `A x = b` and `Aᵀ x = b` at solve cost.
- The triangular kernels are column-walking span operations with no allocation and no bounds-check seam beyond the span itself. Forward elimination divides by the column-pointer-indexed pivot then scatters the scaled column into the suffix; the transpose forms run the columns in reverse and accumulate a gathered dot product before the final pivot divide. Each is one tight loop pair over `ColumnPointers`/`RowIndices`/`Values` — the entire numeric solve surface is four such kernels (`SolveLower`, `SolveLowerTranspose`, `SolveUpper`, `SolveUpperTranspose`).

[SHARED_SCRATCH_NON_REENTRANCY]:
- The non-rectangular factorizations carry one instance-field scratch array `temp` of length `n`, allocated once in the constructor and reused by every `Solve` call. `Solve` permutes the right-hand side into `temp`, runs the in-place triangular sweeps on `temp`, and permutes the result out. Two threads sharing one factorization instance race on this single buffer; a second `Solve` entered before the first returns corrupts both results silently — there is no guard, no clone, no thread-local.
- This makes a cached factorization a single-flight resource per instance, not a free-threaded operator. The correct reuse shape is a capsule that owns the factorization and serializes solves, or a pool of factorization instances keyed by the same symbolic pattern when concurrent multi-RHS throughput is required. The factorization is cheap to share for sequential many-RHS reuse and unsafe to share for parallel solves.
- QR breaks the pattern: both its forward and transpose solves allocate a fresh work array of the augmented length on every call rather than reusing a field. QR solves are therefore reentrant across threads but allocate per solve; the dense direct factorizations are allocation-free per solve but non-reentrant. The reuse policy is opposite for the two families and is a property of the factorization kind, not a caller knob.

[SOLVE_CONTRACT_AND_POLYMORPHISM_BOUNDARY]:
- The shared interface across every direct factorization exposes only the forward `Solve` in both array and `ReadOnlySpan`/`Span` forms; the array overload merely forwards to the span overload. Code that holds the factorization through its interface can solve `A x = b` polymorphically but cannot reach `SolveTranspose` — transpose solving is declared only on the two concrete nonsymmetric types and is invisible to the abstraction. A request surface that must dispatch forward-or-transpose across factorization kinds cannot rest on the shared interface; it must close over the concrete capability, and only LU and QR carry it.
- The span overload is the real entrypoint and admits the empty span as its only argument check, throwing on an empty input or result span and otherwise trusting the caller's lengths. The square solvers require `result` length `n`; QR requires `result` length equal to the solution dimension, which differs from the RHS dimension for rectangular systems. There is no length validation beyond emptiness — a too-short `result` throws an index fault deep inside a permutation gather, and a too-long one silently leaves a tail untouched. The boundary owner sizes the output span from the factorization's solution dimension, never from the RHS.
- The span contract permits solving directly into a slice of a larger buffer and reading the RHS from a slice without copy, so multi-RHS reuse can stream columns through one factorization with zero intermediate allocation on the square solvers. The factorization's own `temp` is the only scratch consumed; the caller's per-RHS cost is two permutation passes plus the triangular sweeps over the factor nonzeros.

[RECTANGULAR_SHAPE_BRANCH]:
- QR's solve is two algorithms behind one entrypoint, selected by the stored row-versus-column count. The overdetermined branch applies the inverse row permutation, sweeps the Householder reflectors forward to form `Qᵀ b`, back-solves `R`, then inverts the column permutation into the first `n` result slots — least-squares minimization. The underdetermined branch applies the column permutation, solves `Rᵀ` by forward elimination, sweeps the reflectors in reverse, then inverts the row permutation — minimum-norm. The transpose solve swaps the two branch bodies, so `Solve`/`SolveTranspose` on a rectangular factorization gives the four corners of the least-squares/minimum-norm cross product from one stored `QR`.
- The work buffer and reflector count are sized by the symbolic augmented dimension, which exceeds the matrix row count for rank-deficient or structurally-singular systems because structural singletons inflate the Householder vector allocation. The solution-dimension/RHS-dimension asymmetry is therefore not simply `min`/`max` of the matrix shape; the output length is the trailing column or row count and the internal buffer is the augmented count. Sizing the result span from the matrix shape rather than the factorization's solution dimension is the latent off-by-augmentation fault.

[SYMBOLIC_NUMERIC_SEPARATION_FOR_REUSE]:
- `Create` runs symbolic analysis then numeric factorization in one call, but the two phases consume disjoint inputs: symbolic analysis reads only the sparsity pattern (column pointers and row indices) to build the inverse permutation, elimination tree, postorder, and column counts into the per-instance symbolic record; numeric factorization then reads the values into the stored factor. When a family of systems shares one pattern and varies only in values, the entire symbolic record is invariant and only the value sweep must rerun — yet the public `Create` re-derives the symbolic record every call, because there is no public entrypoint that accepts a prebuilt symbolic record and a fresh value array.
- The permutation-accepting `Create` overload is the partial amortization the public surface allows: precompute the ordering once, then pass the same permutation vector to every refactorization. This skips the ordering search but still reruns the elimination tree, column counts, and cumulative-sum symbolic build on each call. The ordering is the expensive graph search; the symbolic build that follows is linear in the pattern and unavoidable through the public surface. Full symbolic reuse requires holding the factorization instance itself and re-driving its numeric phase, which the type does not expose — so the durable reuse unit is the whole factorization object, refactored by reconstruction with a cached permutation, not a reusable symbolic handle.
- The symbolic record carries the factor nonzero counts as integer fields populated before any numeric work, so fill-in is knowable from the symbolic phase alone. A reuse policy reads these to decide direct-versus-iterative before committing to the numeric factorization, and stores them beside the cached factorization so the fill ratio is auditable without re-running anything. The factorization's own nonzero count is the count of the stored factor and is computed differently per family — single factor for the symmetric solvers, a fill-corrected sum of two factors for the asymmetric ones — so the cached count is meaningful only paired with the factorization kind.

[FACTORED_OPERATOR_AS_ONE_VALUE]:
- The reuse surface collapses to a single typed value: a factored operator that owns the factorization instance, the cached permutation, the symbolic fill counts, the solution dimension, and the kind discriminant that decides whether transpose-solve and reentrancy are available. Construction admits the matrix once through the chosen factorization; thereafter the value answers many right-hand sides without re-admission, and its kind drives the solve dispatch rather than a parallel family of per-kind solve functions.
- Because the internal residual of a direct solve is exact only up to the factorization's own pivoting tolerance and the matrix it actually read — Cholesky and LDL read only the upper triangle, so an asymmetric input is silently symmetrized — the factored-operator value owns a post-solve residual recomputation against the original operator as its correctness witness. The solve returns a vector with no status; the witness is the only signal that the stored factor answers the system the caller posed, and it belongs to the operator value, not the call site.

```csharp
// A factored operator owns admission-once and answers many right-hand sides.
// Kind decides which solve directions exist and whether the temp buffer is shared.
[Union]
public partial interface IFactoredOp
{
    Solver<T>      Forward();                 // every kind has the forward direction
    Option<Solver<T>> Transpose();            // only the nonsymmetric kinds expose Aᵀx=b
    int            SolutionDim();             // sizes the result span; ≠ rhsDim for rectangular
    bool           SharesScratch();           // true ⇒ serialize solves on one instance
}

public sealed record FactoredOp<T>(
    ISolver<T> Inner, int[] Permutation, int FactorNnz, int SolutionDim, FactorKind Kind);

// One reuse entrypoint folds many right-hand sides through the shared factorization,
// streaming each result column into a pre-sized destination span — zero per-solve alloc
// on the square solvers — and recomputing the true residual as the correctness witness.
static Fin<Seq<double[]>> SolveMany<T>(FactoredOp<T> op, Seq<ReadOnlyMemory<T>> rhs) =>
    rhs.Traverse(b =>
            Span(new double[op.SolutionDim], x =>
                op.Inner.Solve(b.Span, x)) is var x
                && Residual(op, b.Span, x) is var r && double.IsFinite(r) && r <= Tol
                    ? FinSucc(x)
                    : FinFail<double[]>(Error.New("residual witness failed")))
       .As();
```
