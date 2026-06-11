# Refinement and Conditioning on a Held Factorization

[CONDITION_EVIDENCE_HIDES_A_FULL_SVD]:
- The three induced-norm and rank queries on the base matrix surface are each a full singular-value decomposition in disguise: the rank query, the spectral two-norm, and the condition number all resolve to `Svd(computeVectors: false).Rank`, `.L2Norm`, and `.ConditionNumber` respectively — three independent calls construct three separate decompositions of the same operand, with no shared handle and no memoization. A conditioning gate that reads rank, then spectral norm, then condition factors the matrix three times at cubic cost each, when one retained decomposition answers all three from its singular-value vector.
- The cheap induced norms route to the provider as flat-array reductions and never touch a decomposition: the one-norm, the infinity-norm, and the Frobenius norm each call the provider's matrix-norm reduction over the contiguous value buffer at quadratic cost. The expensive surface is exactly the family that needs the spectrum — two-norm, rank, condition — and the cheap surface is exactly the family that needs only the entries. A gate that mistakes the spectral two-norm for a free aggregate pays a decomposition it never reads the vectors of.
- The correct condition estimate for a square operator avoids the spectral decomposition entirely by pairing a cheap induced norm of the operator with the same induced norm of its action on a probe through a retained factorization. The one-norm condition is `‖A‖₁ · ‖A⁻¹‖₁`, and the inverse-norm factor is estimated by a power-iteration probe — solve against a sign-vector right-hand side on the held factorization, renormalize, iterate a fixed small number of times — so the estimate costs one cheap norm plus a handful of triangular solves on a factorization already in hand, never a fresh cubic decomposition. The probe reuses the very handle the solve will use, so the conditioning evidence and the solution share one factorization.

```csharp
// κ₁ estimate: cheap ‖A‖₁ × power-iteration probe of ‖A⁻¹‖₁ on a held factorization — no SVD
static double ConditionOneEstimate(Matrix<double> a, ISolver<double> fac, int probes) =>
    a.L1Norm() * (Schedule.recurs(probes) | Schedule.Forever).Run()
        .Fold((norm: 0.0, x: Vector<double>.Build.Dense(a.RowCount, 1.0 / a.RowCount)),
              (s, _) =>
              {
                  var z = fac.Solve(fac.Solve(s.x).PointwiseSign());
                  return (z.L1Norm(), z.Normalize(1.0));
              }).norm;
```

[REFINEMENT_IS_A_RESIDUAL_FOLD_OVER_A_HELD_HANDLE]:
- Single-precision-quality solutions earn full precision by residual correction without ever refactoring: factor once, solve once for the initial estimate, then loop computing the residual against the original operator, solving the correction on the same held factorization, and accumulating it into the estimate. The factorization is read on every iteration and written on none, so each refinement step is one matrix-vector product plus one triangular back-substitution — sub-cubic — against a cubic decomposition paid exactly once. The loop body never reconstructs evidence; it consumes the handle's solve arrow repeatedly.
- The residual must be formed against the original operator in the working precision, never against the reconstructed factors, because the factors already carry the rounding error the refinement exists to cancel. The residual is `b − A·x` with the unfactored operator; reconstructing `A` from the factors to form the residual defeats the entire correction.
- The fold carries a typed iteration state, not a mutable accumulator: the running estimate, the previous relative-residual norm, and the held factorization travel as one immutable record through a state-threaded reduction. Each step projects a new state from the prior; convergence is the predicate `relative residual below the working-precision floor`, stagnation is `relative residual fails to shrink by a fixed contraction ratio`, and divergence is `relative residual grows`. The three outcomes are cases of one status discriminant the fold returns, so the loop's termination reason is recoverable from the final state, never from an exception or a sentinel return.
- The whole capped refinement runs on exactly four persistent buffers regardless of iteration count — the estimate, the action scratch the in-place matrix-vector product writes, the residual scratch the subtraction overwrites, and the correction scratch the held solve fills — with the estimate absorbing each correction by an in-place vector add. The naive phrasing allocates a fresh vector at every solve and subtraction; the four-buffer discipline is the allocation contract that makes per-iteration cost purely arithmetic.

```csharp
// x ← x + A⁻¹(b − A·x) on one held factorization; cap as schedule, exit as fold predicate
static (Vector<double> X, IterationStatus Status) Refine(
    Matrix<double> a, ISolver<double> fac, Vector<double> b, double floor, int cap)
{
    var ax = Vector<double>.Build.Dense(b.Count);
    var bn = b.L2Norm();                                            // loop invariant: hoisted once
    var end = (Schedule.recurs(cap) | Schedule.Forever).Run().FoldWhile(
        (x: fac.Solve(b), prev: double.PositiveInfinity, st: IterationStatus.Continue),
        (s, _) =>
        {
            a.Multiply(s.x, ax);                                    // in-place A·x, reused buffer
            var rel = b.Subtract(ax).L2Norm() / bn;
            return rel <= floor  ? (s.x, rel, IterationStatus.Converged)
                 : rel >= s.prev ? (s.x, rel, IterationStatus.Diverged)
                 : (s.x.Add(fac.Solve(b.Subtract(ax))), rel, IterationStatus.Continue);
        },
        s => s.st == IterationStatus.Continue);
    return (end.x, end.st == IterationStatus.Continue
        ? IterationStatus.StoppedWithoutConvergence : end.st);      // cap reached is its own status
}
```

[ITERATION_POLICY_IS_ONE_VALUE_AND_DELAY_IS_INERT]:
- The refinement's iteration policy is a recurrence-capped schedule value, and for a compute-bound numerical loop the temporal combinators — spaced, exponential, fibonacci backoff — are inert: the loop has no I/O to pace, so a backoff delay is policy that decides nothing and is the rejected form. The only load-bearing components are the recurrence cap and the convergence predicate that short-circuits it.
- The schedule owns the upper bound on steps and the residual predicate owns the early exit; the two compose without either knowing the other. Reaching the cap is itself a terminal status — stopped-without-convergence — distinct from converged, diverged, and the failure status that marks a degenerate factorization, so the statuses partition every exit and the caller's recovery dispatches on the status case, never on inspecting the residual after the fact.
- The working-precision floor, the recurrence cap, and the stagnation contraction ratio are three rows of one policy record, never magic constants in the loop body. A refinement that hard-codes any of the three has smuggled policy into control flow; lifting all three makes the loop body a pure transition function over operand, factorization, and policy, and the same body serves single-precision-promoted and full-precision-tightened refinements by swapping the policy row.
[OVERFLOW_SAFE_NORMS_COST_A_SCALAR_PASS]:
- The vector two-norm that drives every convergence test is overflow-safe at the cost of being scalar: it aggregates the elements through a hypotenuse reduction that rescales to avoid intermediate overflow and underflow, so a residual whose squared sum would overflow a naive accumulation still norms correctly, but the reduction is one managed call per element with no vectorization. For a refinement loop whose residual norm is recomputed every iteration over a large residual, this scalar reduction is the per-iteration floor cost, and the vectorized sum-of-squares primitive over the residual's contiguous buffer replaces it with one wide pass — at the price of forfeiting the overflow rescaling, which is safe only when the residual magnitude is known bounded.
- The relative-residual denominator is computed once and reused, never per iteration: the right-hand-side norm is invariant across the refinement, so a loop that recomputes it each step pays a redundant scalar reduction. Hoisting the denominator into the initial fold state and threading it unchanged makes each iteration cost exactly one residual norm, and the relative residual is a division by the held constant. The same hoist applies to the operator's cheap induced norm when the loop also tracks a running condition estimate — both invariants live in the initial state, computed once.
