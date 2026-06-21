# [PY_COMPUTE_RECEIPT]

The one method-discriminated solve receipt folded across every solver route. `SolverReceipt` is a single tagged union whose `Literal` tag is the solve method (direct, iterative, least-squares, eigen) and carries a per-case payload terminating in one `SolveStatus`, so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case rather than in flat shared fields. `SolveStatus` is the ONE termination vocabulary every backend folds into: it unifies the `lineax`/`optimistix`/`diffrax` `RESULTS` enums, the `scipy` `OptimizeResult` success flag, and the numpy-floor residual verdict into one bounded `StrEnum`, so a converged solve, a max-steps solve, a singular system, and a stagnated iteration are distinct first-class verdicts rather than one Boolean collapsing every non-success cause to `False`. The four per-case constructors are the canonical tagged-union factories; there is no single `.of`, and there is never a receipt type per solver. `SolverReceipt.contribute` emits one observability row carrying the termination reason the C# graduation gate and the sibling `convex`/`program` status fields read by the same vocabulary, and a graduated solve produces a `GraduationReceipt` solver or symbolic subject.

## [01]-[INDEX]

- [01]-[RECEIPT]: the unified method-discriminated solve receipt, the termination-status vocabulary, and the status fold.

## [02]-[RECEIPT]

- Owner: `SolverReceipt` — the ONE `@tagged_union` solve receipt over every route; the `Literal` tag is the solve method, read directly through `.method`. Each method case carries its own tuple payload terminating in one `SolveStatus`, so `direct` carries `(residual, condition, status)`, `iterative` carries `(residual, iterations, tol, status)`, `least_squares` carries `(residual, rank, iterations, tol, status)`, and `eigen` carries `(spectral_residual, k, condition, status)`. The status is the LAST slot of every case by construction, so `.status` reads it through one structural `case (*_, status)` pattern rather than four shape-specific arms; the discriminant lives in the case, never in a flat shared struct and never in a parallel vocabulary enum beside the tag.
- Status vocabulary: `SolveStatus` is the ONE bounded termination `StrEnum` — `SUCCESS`, `MAX_STEPS`, `SINGULAR`, `BREAKDOWN`, `STAGNATION`, `DIVERGENCE`, `NONFINITE`, `ILL_CONDITIONED`, `INFEASIBLE`, `UNBOUNDED`, `OTHER` — folding every backend's native result enum into one verdict the receipt carries. A backend that already adjudicates termination (`lineax.Solution.result`, `optimistix.Solution.result`, `diffrax.Solution.result`, all `RESULTS` members) maps into `SolveStatus` through the one `_STATUS` boundary table keyed on the documented `RESULTS` member-name strings; `scipy.OptimizeResult.success` maps to `SUCCESS`/`STAGNATION`; the numpy floor with no library adjudicator derives its verdict from the residual against the per-method tolerance. The `INFEASIBLE`/`UNBOUNDED` members are the feasibility-verdict termination classes the convex consumer (`optimization/convex.md#CONVEX`) folds the cvxpy `infeasible`/`unbounded` status constants into through its own `_CONVEX_STATUS` boundary table; they live on this shared vocabulary so the convex receipt's `status` reads the same enum rather than a parallel convex-only status type, the iterative/least-squares solver routes simply never emit them. `converged` is the derived predicate `status is SolveStatus.SUCCESS`, so the Boolean contract survives while the receipt also carries *why* a solve did not converge.
- Status fold: `_status(adjudicated, residual, tol)` is the ONE polymorphic verdict every factory folds — when a backend supplies its `RESULTS` member name the fold reads `_STATUS.get(name, SolveStatus.OTHER)` so an unmapped member degrades to `OTHER` rather than crashing; when no adjudicator exists (`adjudicated is None`, the numpy-floor path) it returns `SUCCESS` exactly when `np.isfinite(residual) and residual <= tol`, else `STAGNATION` for a finite-but-stalled residual and `NONFINITE` for a non-finite one. The four method tolerances are one frozen `_TOL` table keyed by tag (`direct`/`least_squares` at `1e-6`, `iterative` carrying its caller-supplied `tol`, `eigen` at `1e-8`), so a new method tolerance is one row, never a fifth inlined `np.isfinite`/comparison expression, and the LM least-squares and the iterative floor read the same parameterized verdict.
- Entry: the four static constructors `Direct`, `Iterative`, `LeastSquares`, and `Eigen` are the canonical factories every solver route folds into, each terminating its payload through `_status` — a route that holds a backend `RESULTS` member passes its name, a numpy-floor route passes `None` and lets the residual floor adjudicate. `.status` reads the trailing status slot through one structural pattern, `.converged` derives the Boolean, and `.method` reads the tag as the method literal. One mapping folds the scipy, lineax, optimistix, diffrax, and scikit-fem termination reasons into the same vocabulary.
- Receipt: `SolverReceipt.contribute` implements `ReceiptContributor`, emitting one `Receipt.of("emitted", ...)` row carrying the method tag, the termination `status`, and the derived `converged` flag; a solve graduating outward routes through `graduation/handoff.md#GRADUATION` on the solver or symbolic `HandoffAxis` case, the residual ledger the graduation fold reads being the per-case residual this receipt already carries.
- Packages: `expression` (`tag`, `case`, `tagged_union`), `numpy` (`isfinite` floors the no-adjudicator verdict), runtime (`Receipt`, `ReceiptContributor`). The `lineax`/`optimistix`/`diffrax` `RESULTS` member names this owner maps are supplied by the gated solver routes at the boundary, never imported here, so the receipt owner carries no gated-band dependency.
- Growth: a new convergence shape is one `SolverReceipt` case keyed by one tag literal plus one `_TOL` row; a new backend termination reason is one `_STATUS` row mapping its `RESULTS` member name into the existing `SolveStatus` vocabulary, or one new `SolveStatus` member when a genuinely new termination class appears; the status fold is reused, never re-inlined; zero new surface, no per-solver receipt struct, no flat shared-field receipt, no parallel method-vocabulary enum, no Boolean-per-case convergence flag discarding the backend's termination reason.
- Boundary: no benchmark authority and no substrate selection on the receipt — it carries the termination evidence the C# graduation gate reads, never a runtime decision. A per-solver receipt struct, a flat receipt with a stringly-typed method field, a `SolveMethod` enum duplicating the tag, a single per-case factory collapse hiding the method payload, and a lone `bool converged` discarding the `RESULTS` enum the backend already computed are the deleted forms; the runtime `Receipt.of(phase, owner, subject, facts)` carries the contribution.

```python signature
from enum import StrEnum
from typing import Literal

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.runtime.receipts import Receipt


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]


class SolveStatus(StrEnum):
    SUCCESS = "success"
    MAX_STEPS = "max_steps"
    SINGULAR = "singular"
    BREAKDOWN = "breakdown"
    STAGNATION = "stagnation"
    DIVERGENCE = "divergence"
    NONFINITE = "nonfinite"
    ILL_CONDITIONED = "ill_conditioned"
    INFEASIBLE = "infeasible"
    UNBOUNDED = "unbounded"
    OTHER = "other"


_TOL: FrozenDict[SolveMethod, float] = FrozenDict(
    {"direct": 1e-6, "iterative": 1e-6, "least_squares": 1e-6, "eigen": 1e-8}
)

_STATUS: FrozenDict[str, SolveStatus] = FrozenDict(
    {
        "successful": SolveStatus.SUCCESS,
        "max_steps_reached": SolveStatus.MAX_STEPS,
        "nonlinear_max_steps_reached": SolveStatus.MAX_STEPS,
        "singular": SolveStatus.SINGULAR,
        "breakdown": SolveStatus.BREAKDOWN,
        "stagnation": SolveStatus.STAGNATION,
        "nonlinear_divergence": SolveStatus.DIVERGENCE,
        "nonfinite": SolveStatus.NONFINITE,
        "nonfinite_input": SolveStatus.NONFINITE,
        "conlim": SolveStatus.ILL_CONDITIONED,
    }
)


def _status(adjudicated: str | None, residual: float, tol: float) -> SolveStatus:
    if adjudicated is not None:
        return _STATUS.get(adjudicated, SolveStatus.OTHER)
    if not np.isfinite(residual):
        return SolveStatus.NONFINITE
    return SolveStatus.SUCCESS if residual <= tol else SolveStatus.STAGNATION


@tagged_union(frozen=True)
class SolverReceipt:
    tag: SolveMethod = tag()
    direct: tuple[float, float, SolveStatus] = case()
    iterative: tuple[float, int, float, SolveStatus] = case()
    least_squares: tuple[float, int, int, float, SolveStatus] = case()
    eigen: tuple[float, int, float, SolveStatus] = case()

    @staticmethod
    def Direct(residual: float, condition: float, result: str | None = None) -> "SolverReceipt":
        return SolverReceipt(direct=(residual, condition, _status(result, residual, _TOL["direct"])))

    @staticmethod
    def Iterative(residual: float, iterations: int, tol: float, result: str | None = None) -> "SolverReceipt":
        return SolverReceipt(iterative=(residual, iterations, tol, _status(result, residual, tol)))

    @staticmethod
    def LeastSquares(
        residual: float, rank: int, iterations: int, tol: float = _TOL["least_squares"], result: str | None = None
    ) -> "SolverReceipt":
        return SolverReceipt(least_squares=(residual, rank, iterations, tol, _status(result, residual, tol)))

    @staticmethod
    def Eigen(spectral_residual: float, k: int, condition: float, result: str | None = None) -> "SolverReceipt":
        return SolverReceipt(eigen=(spectral_residual, k, condition, _status(result, spectral_residual, _TOL["eigen"])))

    @property
    def method(self) -> SolveMethod:
        return self.tag

    @property
    def status(self) -> SolveStatus:
        match getattr(self, self.tag):
            case (*_, SolveStatus() as status):
                return status

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", "compute.solver", self.tag, {"method": self.tag, "status": self.status, "converged": str(self.converged)}
        )
```

## [03]-[RESEARCH]

- [STATUS_VOCABULARY]: the `_STATUS` boundary table keys map the documented `RESULTS` member names every gated solver backend carries — `lineax` (`successful`, `max_steps_reached`, `singular`, `breakdown`, `stagnation`, `conlim`, `nonfinite_input`), `optimistix` (the same set plus `nonlinear_max_steps_reached`, `nonlinear_divergence`, `nonfinite`), and `diffrax` (interrogated through its `is_successful`/`is_okay` checks) — into the one `SolveStatus` vocabulary. The mapping lives on this owner so the gated routes pass only the member-name string, never an imported `RESULTS` type, keeping the receipt owner cp315-clean while the gated routes stay the only band-dependent code. An unmapped member degrades to `OTHER` rather than raising, so a `RESULTS` member added by a future backend release reaches the receipt as a recorded fact, not a crash.
- [STATUS_FLOOR]: the numpy-floor routes (`solvers/linear.md#LINEAR` dense, `solvers/nonlinear.md#NONLINEAR` central-difference, `solvers/quadrature.md#QUADRATURE` trapezoid/interp) hold no backend adjudicator, so they pass `result=None` and the residual-against-tolerance floor adjudicates `SUCCESS`/`STAGNATION`/`NONFINITE`; the gated routes pass the backend `Solution.result` member name and the floor is bypassed. This keeps one verdict path with the backend status as the authority where it exists and the residual floor as the fallback where it does not, never two parallel convergence notions.
