# [PY_COMPUTE_RECEIPT]

The one method-discriminated solve receipt folded across every solver route. `SolverReceipt` is a single tagged union whose `Literal` tag is the solve method (direct, iterative, least-squares, eigen) and carries a per-case payload, so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case rather than in flat shared fields. The four per-case constructors are the canonical tagged-union factories; there is no single `.of`, and there is never a receipt type per solver. `SolverReceipt.contribute` emits one observability row, and a graduated solve produces a `GraduationReceipt` solver or symbolic subject.

## [01]-[INDEX]

- [01]-[RECEIPT]: the unified method-discriminated solve receipt and the convergence fold.

## [02]-[RECEIPT]

- Owner: `SolverReceipt` — the ONE `@tagged_union` solve receipt over every route; the `Literal` tag is the solve method, read directly through `.method`. Each method case carries its own tuple payload, so `direct` carries `(residual, condition, converged)`, `iterative` carries `(residual, iterations, tol, converged)`, `least_squares` carries `(residual, rank, iterations, converged)`, and `eigen` carries `(spectral_residual, k, condition, converged)`. The discriminant lives in the case, never in a flat shared struct and never in a parallel vocabulary enum beside the tag.
- Entry: the four static constructors `Direct`, `Iterative`, `LeastSquares`, and `Eigen` are the canonical factories every solver route folds into; `.converged` is a total `match` over the four cases reading the per-case convergence flag, and `.method` reads the tag as the method literal. One `match` folds the scipy, lineax, optimistix, diffrax, and scikit-fem paths into the same receipt.
- Receipt: `SolverReceipt.contribute` implements `ReceiptContributor`, emitting one `Receipt.of("emitted", ...)` row carrying the method tag and the convergence verdict; a solve graduating outward routes through `graduation/handoff.md#GRADUATION` on the solver or symbolic `HandoffAxis` case.
- Packages: `expression` (`tag`, `case`, `tagged_union`), `numpy` (`isfinite`, `linalg.norm` floors the converged predicates), runtime (`Receipt`, `ReceiptContributor`).
- Growth: a new convergence shape is one `SolverReceipt` case keyed by one tag literal; zero new surface, no per-solver receipt struct, no flat shared-field receipt, no parallel method-vocabulary enum.
- Boundary: no benchmark authority and no substrate selection on the receipt — it carries the convergence evidence the C# graduation gate reads, never a runtime decision. A per-solver receipt struct, a flat receipt with a stringly-typed method field, a `SolveMethod` enum duplicating the tag, and a single per-case factory collapse hiding the method payload are the deleted forms; the runtime `Receipt.of(phase, owner, subject, facts)` carries the contribution.

```python signature
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.runtime.receipts import Receipt


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]


@tagged_union(frozen=True)
class SolverReceipt:
    tag: SolveMethod = tag()
    direct: tuple[float, float, bool] = case()
    iterative: tuple[float, int, float, bool] = case()
    least_squares: tuple[float, int, int, bool] = case()
    eigen: tuple[float, int, float, bool] = case()

    @staticmethod
    def Direct(residual: float, condition: float) -> "SolverReceipt":
        return SolverReceipt(direct=(residual, condition, bool(np.isfinite(residual) and residual < 1e-6)))

    @staticmethod
    def Iterative(residual: float, iterations: int, tol: float) -> "SolverReceipt":
        return SolverReceipt(iterative=(residual, iterations, tol, residual <= tol))

    @staticmethod
    def LeastSquares(residual: float, rank: int, iterations: int) -> "SolverReceipt":
        return SolverReceipt(least_squares=(residual, rank, iterations, bool(np.isfinite(residual))))

    @staticmethod
    def Eigen(spectral_residual: float, k: int, condition: float) -> "SolverReceipt":
        return SolverReceipt(eigen=(spectral_residual, k, condition, spectral_residual < 1e-8))

    @property
    def method(self) -> SolveMethod:
        return self.tag

    @property
    def converged(self) -> bool:
        match self:
            case SolverReceipt(tag="direct", direct=(_, _, ok)):
                return ok
            case SolverReceipt(tag="iterative", iterative=(_, _, _, ok)):
                return ok
            case SolverReceipt(tag="least_squares", least_squares=(_, _, _, ok)):
                return ok
            case SolverReceipt(tag="eigen", eigen=(_, _, _, ok)):
                return ok
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "compute.solver", self.tag, {"method": self.tag, "converged": str(self.converged)})
```
