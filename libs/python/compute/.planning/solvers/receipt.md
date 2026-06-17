# [PY_COMPUTE_RECEIPT]

The one method-discriminated solve receipt folded across every solver route. `SolverReceipt` is a single tagged union whose `Literal` tag is backed by the `SolveMethod` vocabulary (direct, iterative, least-squares, eigen) and carries a per-case payload, so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case rather than in flat shared fields. The four per-case constructors are the canonical tagged-union factories; there is no single `.of`, and there is never a receipt type per solver. `SolverReceipt.contribute` emits one observability row, and a graduated solve produces a `GraduationReceipt` solver or symbolic subject.

## [1]-[INDEX]

[RECEIPT]: the unified method-discriminated solve receipt and the convergence fold.

## [2]-[RECEIPT]

- Owner: `SolverReceipt` — the ONE `@tagged_union` solve receipt over every route; `SolveMethod` is the backing vocabulary surfaced through `.method`. Each method case carries its own tuple payload, so `direct` carries `(residual, condition, converged)`, `iterative` carries `(residual, iterations, tol, converged)`, `least_squares` carries `(residual, rank, iterations, converged)`, and `eigen` carries `(spectral_residual, k, condition, converged)`. The discriminant lives in the case, never in a flat shared struct.
- Entry: the four static constructors `Direct`, `Iterative`, `LeastSquares`, and `Eigen` are the canonical factories every solver route folds into; `.converged` is a total `match` over the four cases reading the per-case convergence flag, and `.method` projects the tag back to `SolveMethod`. One `match` folds the scipy, lineax, optimistix, diffrax, and scikit-fem paths into the same receipt.
- Receipt: `SolverReceipt.contribute` implements `ReceiptContributor`, emitting one `Receipt.Emitted` row carrying the method tag and the convergence verdict; a solve graduating outward routes through `graduation/receipt.md#GRADUATION` on the solver or symbolic `HandoffAxis` case.
- Packages: `expression` (`tag`, `case`, `tagged_union`), `numpy` (`isfinite`, `linalg.norm` floors the converged predicates), runtime (`Receipt`, `ReceiptContributor`).
- Growth: a new convergence shape is one `SolveMethod` row keying one `SolverReceipt` case; zero new surface, no per-solver receipt struct, no flat shared-field receipt.
- Boundary: no benchmark authority and no substrate selection on the receipt — it carries the convergence evidence the C# graduation gate reads, never a runtime decision. A per-solver receipt struct, a flat receipt with a stringly-typed method field, and a single `.of` factory hiding the per-case payload are the deleted forms.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.runtime.observability.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------
class SolveMethod(StrEnum):
    DIRECT = "direct"
    ITERATIVE = "iterative"
    LEAST_SQUARES = "least_squares"
    EIGEN = "eigen"


# --- [MODELS] ------------------------------------------------------------------------------
@tagged_union(frozen=True)
class SolverReceipt:
    tag: Literal["direct", "iterative", "least_squares", "eigen"] = tag()
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
        return SolveMethod(self.tag)

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

    def contribute(self) -> Receipt:  # ReceiptContributor
        return Receipt.Emitted("compute.solver", self.tag, {"method": self.tag, "converged": str(self.converged)})
```
