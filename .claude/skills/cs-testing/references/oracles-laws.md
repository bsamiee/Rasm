# [H1][ORACLES_LAWS]
>**Dictum:** *The expected value must come from a different idea than the implementation.*

<br>

## [1][ORACLE_ORDER]

| [INDEX] | [ORACLE] | [EXAMPLE] |
| :-----: | -------- | --------- |
| [1] | Algebraic identity | Transpose involution, identity matrix, reflection length preservation. |
| [2] | External closed form | Percentile interpolation, SDF primitive formula, row-major dot product. |
| [3] | Smaller reference model | `HashSet<T>` for custom set, array loops for matrix multiply, scalar ODE for streamline. |
| [4] | Metamorphic relation | Translate/scale/permute input and assert invariant or transformed output. |
| [5] | Typed receipt invariant | Stop kind, unsupported output, residual budget, convergence metadata. |
| [6] | Bridge runtime observation | Rhino/GH native validity, UI marshaling, document/canvas side effects. |

[CRITICAL] Do not use production `Matrix.operator *`, `Matrix.Determinant`, `CloudKernel.*`, or the same projection method as the expected-value engine for the method under test.

---
## [2][ORACLE_GRADE]

| [GRADE] | [ORACLE] | [USE] |
| ------- | -------- | ----- |
| A | Independent closed form, smaller model, or metamorphic relation | Default target for core behavior. |
| B | External library fact or independent scalar/reference loop | Numeric and parsing rails. |
| C | Typed receipt/category/code invariant | Failure and unsupported-output rails. |
| D | Structural assertion | Supplemental shape proof only. |
| F | Implementation mirror or current-output snapshot | Reject. |

---
## [3][LAW_MATRIX]

| [AXIS] | [LAW] | [BUGS_CAUGHT] |
| ------ | ----- | ------------- |
| Construction | Valid accepted, invalid rejected, non-finite rejected, diagnostics stable by category/code. | Bad factory boundaries, accidental exception rails. |
| Projection | Accepted output succeeds, unsupported output fails, capability-gated output fails truthfully. | Wrong `typeof(TOut)` branch, capability drift. |
| Algebra | Identity, inverse, involution, associativity, commutativity, distributivity where true. | Operator and fold regressions. |
| Order/statistics | Monotonicity, extrema ties, percentile interpolation, permutation invariance. | Sort/index/tolerance bugs. |
| Numeric | Residual, reconstruction, rank, eigenpair, norm inequalities, conditioning guards. | Circular linear algebra tests. |
| Rail | `Fin` success/failure, `Validation` accumulation, `Option` some/none, `ManyErrors` shape. | Swallowed diagnostics. |
| Stateful | Attach/detach, LIFO/FIFO, idempotence or named non-idempotence, rollback double fault. | Lifecycle leaks and false monoids. |
| Runtime | Static-vs-bridge classification, native validity, host/UI thread behavior. | Tests that pass outside the only runtime that matters. |

---
## [4][ANTI_PATTERNS]

- Snapshotting the current implementation output and asserting it later.
- Using one test per branch when one generated law can traverse the axis.
- Adjusting a test after failure before proving whether product code is wrong.
- Testing only happy-path construction while projection/unsupported rails remain untouched.
- Using broad `Assert.True` labels that hide the generated input and law.
- Adding a shared helper for one file instead of inlining or promoting a universal testkit primitive.
