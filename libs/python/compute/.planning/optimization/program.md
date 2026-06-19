# [PY_COMPUTE_PROGRAM]

The constrained, global, and discrete counterpart of the gradient-driven design loop — the math-program routes the differentiable optimizer in `optimization/design.md#DESIGN` structurally cannot reach. `ProgramIntent` discriminates a linear program, a mixed-integer program, a derivative-free global minimum, a bounded/constrained smooth minimum, and an optimal assignment over `scipy.optimize`, every route folding the `OptimizeResult` success flag, the objective value, and the maximum constraint-violation residual into one content-keyed `ProgramReceipt`. The five routes share one tag-keyed `scipy.optimize` dispatch — the entrypoint, the constraint-assembly probe, and the violation reduction are the row — and the program data admits through `arrays/payload.md#PAYLOAD` keying on the same `ContentIdentity` seed. Unlike `design.md` and `solvers/nonlinear.md#NONLINEAR`, this owner carries no numpy floor: the math-program solve *is* `scipy.optimize`, so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate, mirroring the no-floor hull/Delaunay routes of `spatial/query.md#QUERY` where Qhull is the gated capability itself. The certified optimum graduates as the existing `solver` `HandoffAxis` case through `graduation/receipt.md#GRADUATION`, the discrete/constrained sibling of the differentiable design optimum on the one rail.

## [1]-[INDEX]

[PROGRAM]: linear / integer / global / constrained / assignment math programs over `scipy.optimize` folding one content-keyed `ProgramReceipt` on the one `ProgramIntent` owner.

## [2]-[PROGRAM]

- Owner: `ProgramIntent` — the math-program cases discriminated by constraint-and-integrality structure recoverable from the problem value itself, never a differentiable objective; `Linear(c, a_ub, b_ub, bounds)` over `scipy.optimize.linprog` on the HiGHS backend, `Integer(c, integrality, bounds, constraints)` over `scipy.optimize.milp` threading the integrality vector and `Bounds`, `Global(objective, bounds)` (the `stochastic` case, the python-keyword-free tag for the derivative-free global search) over `scipy.optimize.differential_evolution` on a box, `Constrained(objective, x0, bounds, constraints)` over `scipy.optimize.minimize` threading `Bounds`/`LinearConstraint`/`NonlinearConstraint`, and `Assignment(cost)` over `scipy.optimize.linear_sum_assignment`. The five routes share one tag-keyed dispatch in `_program_receipt` — the `scipy.optimize` entrypoint, the constraint carrier, and the violation probe are the row behind the gated import, never five parallel helper bodies. The discriminant is the program shape, so the gradient loop and the math-program loop are sibling cases on the one `optimization` sub-domain, never a duplicated optimizer surface beside the solve.
- Entry: `ProgramIntent.solve` enters one `boundary(f"program.{intent.tag}", ...)`; every route reads the `OptimizeResult` (or the `linear_sum_assignment` row/column pair) into `ProgramReceipt` carrying the success flag, the objective value, and the maximum constraint-violation residual, and keys by `ContentIdentity.of` over the canonical problem-data buffer. The `Linear`/`Integer`/`Global`/`Constrained` routes read `result.success`/`result.fun`/`result.x` and reduce the residual over the assembled inequality/equality rows; the `Assignment` route reads the optimal row/column pair, sums the selected cost, and reports a zero violation (the assignment is feasible by construction).
- Receipt: `ProgramReceipt.contribute` emits one `Receipt.of("emitted", ...)` row carrying the program tag, the objective value, the success flag, and the content key; a certified optimum graduates outward through `graduation/receipt.md#GRADUATION` on the existing `solver` `HandoffAxis` case — no new literal, no graduation edit, the discrete/constrained counterpart of the `design.md` differentiable optimum on the one rail.
- Packages: `scipy` (`optimize.linprog`, `optimize.milp`, `optimize.differential_evolution`, `optimize.minimize`, `optimize.linear_sum_assignment`, `optimize.Bounds`, `optimize.LinearConstraint`, `optimize.NonlinearConstraint`, `optimize.OptimizeResult` — all catalogued in `compute/.api/scipy.md`'s `scipy.optimize` entrypoint and public-type tables), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `maximum` — the canonical problem-data buffer and the constraint-violation max-reduction), `arrays/payload.md#PAYLOAD` (the cost vector, constraint matrix, bounds, and integrality admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`).
- Growth: a new math-program route is one `ProgramIntent` case plus one row in the `_program_receipt` route table; a new `scipy.optimize` constraint carrier is one cell on the row; zero new surface, never a per-program owner, never a parallel linear-program-and-assignment owner, never a per-route `_*_receipt` helper body.
- Boundary: constrained, global, and discrete optimization over `scipy.optimize` only — the linear program, the mixed-integer program, the derivative-free global minimum, the constrained smooth minimum, and the optimal assignment are in-scope; the differentiable inverse-design loop stays on `design.md` and never duplicates here. This owner carries **no numpy floor**: the math program *is* `scipy.optimize` (HiGHS LP, the MILP branch-and-bound, the DE population, the assignment Hungarian), so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate — the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR` (which carry a reachable numpy central-difference floor), matching the no-floor hull/Delaunay/Voronoi routes of `spatial/query.md#QUERY`. A training loop, a production solver session, a hand-rolled simplex or branch-and-bound kernel `scipy.optimize` owns, and a parallel optimizer surface beside the differentiable solve are the deleted forms.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class ProgramReceipt(Struct, frozen=True):
    program: str
    objective: float
    success: bool
    violation: float
    content_key: ContentKey

    def contribute(self) -> Receipt:
        facts = {
            "program": self.program,
            "objective": f"{self.objective:.6g}",
            "success": str(self.success),
            "violation": f"{self.violation:.3e}",
        }
        return Receipt.of("emitted", "compute.optimization.program", self.program, facts)


@tagged_union(frozen=True)
class ProgramIntent:
    tag: Literal["linear", "integer", "stochastic", "constrained", "assignment"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, tuple[tuple[float, float], ...]] = case()
    integer: tuple[np.ndarray, np.ndarray, tuple[tuple[float, float], ...], tuple[object, ...]] = case()
    stochastic: tuple[object, tuple[tuple[float, float], ...]] = case()
    constrained: tuple[object, np.ndarray, tuple[tuple[float, float], ...], tuple[object, ...]] = case()
    assignment: np.ndarray = case()

    @staticmethod
    def Linear(
        c: np.ndarray,
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        bounds: tuple[tuple[float, float], ...],
    ) -> "ProgramIntent":
        return ProgramIntent(linear=(c, a_ub, b_ub, bounds))

    @staticmethod
    def Integer(
        c: np.ndarray,
        integrality: np.ndarray,
        bounds: tuple[tuple[float, float], ...],
        constraints: tuple[object, ...] = (),
    ) -> "ProgramIntent":
        return ProgramIntent(integer=(c, integrality, bounds, constraints))

    @staticmethod
    def Global(
        objective: Callable[[np.ndarray], float], bounds: tuple[tuple[float, float], ...]
    ) -> "ProgramIntent":
        return ProgramIntent(stochastic=(objective, bounds))

    @staticmethod
    def Constrained(
        objective: Callable[[np.ndarray], float],
        x0: np.ndarray,
        bounds: tuple[tuple[float, float], ...],
        constraints: tuple[object, ...] = (),
    ) -> "ProgramIntent":
        return ProgramIntent(constrained=(objective, x0, bounds, constraints))

    @staticmethod
    def Assignment(cost: np.ndarray) -> "ProgramIntent":
        return ProgramIntent(assignment=cost)


def _program_key(intent: ProgramIntent, fields: tuple[object, ...]) -> ContentKey:
    buffer = b"".join(
        np.ascontiguousarray(field).tobytes() for field in fields if isinstance(field, np.ndarray)
    )
    return ContentIdentity.of(f"program.{intent.tag}", buffer, IdentityPolicy())


def solve(intent: ProgramIntent) -> "RuntimeRail[ProgramReceipt]":
    return boundary(f"program.{intent.tag}", lambda: _program_receipt(intent))


def _program_receipt(intent: ProgramIntent) -> ProgramReceipt:
    from scipy.optimize import (
        Bounds,
        differential_evolution,
        linear_sum_assignment,
        linprog,
        milp,
        minimize,
    )

    match intent:
        case ProgramIntent(tag="linear", linear=(c, a_ub, b_ub, bounds)):
            cost = np.asarray(c, dtype=float)
            mat, rhs = np.atleast_2d(np.asarray(a_ub, dtype=float)), np.asarray(b_ub, dtype=float)
            result = linprog(cost, A_ub=mat, b_ub=rhs, bounds=bounds, method="highs")
            objective = float(result.fun) if result.success else float("inf")
            violation = _row_violation(mat, np.asarray(result.x, dtype=float), rhs) if result.success else float("inf")
            return ProgramReceipt(
                "linear", objective, bool(result.success), violation, _program_key(intent, (cost, mat, rhs))
            )
        case ProgramIntent(tag="integer", integer=(c, integrality, bounds, constraints)):
            cost, flags = np.asarray(c, dtype=float), np.asarray(integrality)
            lower = np.asarray([lo for lo, _ in bounds], dtype=float)
            upper = np.asarray([hi for _, hi in bounds], dtype=float)
            result = milp(
                cost,
                integrality=flags,
                bounds=Bounds(lower, upper),
                constraints=list(constraints) or None,
            )
            objective = float(result.fun) if result.success else float("inf")
            violation = _constraint_violation(constraints, np.asarray(result.x, dtype=float)) if result.success else float("inf")
            return ProgramReceipt(
                "integer", objective, bool(result.success), violation, _program_key(intent, (cost, flags))
            )
        case ProgramIntent(tag="stochastic", stochastic=(objective_fn, bounds)):
            box = [tuple(float(b) for b in pair) for pair in bounds]
            result = differential_evolution(objective_fn, box)
            return ProgramReceipt(
                "stochastic",
                float(result.fun),
                bool(result.success),
                0.0,
                _program_key(intent, (np.asarray(box, dtype=float),)),
            )
        case ProgramIntent(tag="constrained", constrained=(objective_fn, x0, bounds, constraints)):
            start = np.asarray(x0, dtype=float)
            lower = np.asarray([lo for lo, _ in bounds], dtype=float)
            upper = np.asarray([hi for _, hi in bounds], dtype=float)
            result = minimize(
                objective_fn,
                start,
                method="SLSQP",
                bounds=Bounds(lower, upper),
                constraints=[row for con in constraints for row in _as_dicts(con)],
            )
            objective = float(result.fun) if result.success else float("inf")
            violation = _constraint_violation(constraints, np.asarray(result.x, dtype=float))
            return ProgramReceipt(
                "constrained", objective, bool(result.success), violation, _program_key(intent, (start,))
            )
        case ProgramIntent(tag="assignment", assignment=cost):
            matrix = np.atleast_2d(np.asarray(cost, dtype=float))
            rows, cols = linear_sum_assignment(matrix)
            return ProgramReceipt(
                "assignment",
                float(matrix[rows, cols].sum()),
                True,
                0.0,
                _program_key(intent, (matrix,)),
            )
        case unreachable:
            assert_never(unreachable)


def _row_violation(matrix: np.ndarray, x: np.ndarray, rhs: np.ndarray) -> float:
    return float(np.maximum(matrix @ x - rhs, 0.0).max(initial=0.0))


def _residual(constraint: object, x: np.ndarray) -> np.ndarray:
    value = constraint.A @ x if hasattr(constraint, "A") else constraint.fun(x)
    return np.maximum(np.maximum(constraint.lb - value, value - constraint.ub), 0.0)


def _constraint_violation(constraints: tuple[object, ...], x: np.ndarray) -> float:
    return float(
        max(
            (float(_residual(con, x).max(initial=0.0)) for con in constraints),
            default=0.0,
        )
    )


def _as_dicts(constraint: object) -> tuple[dict[str, object], ...]:
    matrix_form = hasattr(constraint, "A")
    value = (lambda x, con=constraint: con.A @ x) if matrix_form else (lambda x, con=constraint: con.fun(x))
    upper = np.asarray(constraint.ub, dtype=float)
    lower = np.asarray(constraint.lb, dtype=float)
    rows: list[dict[str, object]] = []
    if np.isfinite(upper).any():
        rows.append({"type": "ineq", "fun": lambda x, g=value, ub=upper: ub - np.asarray(g(x))})
    if np.isfinite(lower).any():
        rows.append({"type": "ineq", "fun": lambda x, g=value, lb=lower: np.asarray(g(x)) - lb})
    return tuple(rows)
```

## [3]-[RESEARCH]

- [SCIPY_OPTIMIZE]: the `scipy.optimize.linprog(c, A_ub, b_ub, bounds, method="highs")`, `milp(c, integrality, bounds, constraints)`, `differential_evolution(func, bounds)`, `minimize(fun, x0, method="SLSQP", bounds, constraints)`, and `linear_sum_assignment(cost_matrix)` spellings — with the `Bounds`/`LinearConstraint`/`NonlinearConstraint` constraint carriers and the `OptimizeResult.success`/`.fun`/`.x` result fields — are fully catalogued in `compute/.api/scipy.md`'s `scipy.optimize` entrypoint table (`linprog`/`milp`/`differential_evolution`/`linear_sum_assignment`) and public-type table (`OptimizeResult`/`Bounds`/`LinearConstraint`/`NonlinearConstraint`), so the body verifies against the present catalogue directly. The spellings carry the `python_version<'3.15'` scipy marker and settle against the installed wheel under a uv-sync reflection pass on that gated band; this owner carries no numpy floor because `scipy.optimize` is the gated capability itself, so a cp315 run without the scipy wheel returns `Error(Import)` for every route — the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR`.
- [PROGRAM_CONTENT_KEY]: `_program_key` derives the `ContentKey` over the canonical contiguous problem-data buffer (the concatenated `tobytes()` of the cost vector, constraint matrix, bounds box, integrality, or cost matrix) through `ContentIdentity.of("program.<tag>", ...)` under the runtime `IdentityPolicy`, so a program whose data admits through `arrays/payload.md#PAYLOAD` keys under the same `ContentIdentity` seed algebra and a repeated solve on identical data is a cache hit by reference. The objective callables of the `Global` and `Constrained` cases are excluded from the key buffer — only the array-shaped problem data seeds identity, matching the payload admission's host-transfer buffer.
