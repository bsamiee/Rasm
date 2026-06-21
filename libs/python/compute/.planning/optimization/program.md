# [PY_COMPUTE_PROGRAM]

The constrained, global, and discrete counterpart of the gradient-driven design loop — the math-program routes the differentiable optimizer in `optimization/design.md#DESIGN` structurally cannot reach. `ProgramIntent` discriminates a linear program, a mixed-integer program, a derivative-free global minimum, a bounded/constrained smooth minimum, and an optimal assignment over `scipy.optimize`, every route folding the host termination verdict, the objective value, and the maximum constraint-violation residual into the `program` case of the shared content-keyed `OutcomeReceipt` (`optimization/design.md#DESIGN`), the one optimization-outcome owner whose sibling `design` case carries the first-order convergence verdict — the feasibility verdict and the convergence verdict are two cases of one `@tagged_union`, never two parallel structs. The `program` case carries the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns rather than a lone `bool success`: the `linprog`/`milp` `OptimizeResult.status` integer code and the `differential_evolution`/`minimize` `.success` flag fold into the same bounded `StrEnum` that adjudicates every solver route, so an infeasible LP, an unbounded LP, an iteration-limit MILP, and a numerically-failed SLSQP solve are distinct first-class verdicts the C# graduation gate and the sibling `convex`/`solver` status fields read by one vocabulary — never a boolean collapsing every non-success cause to `False`. The typed feasibility receipt is the `program` case projection: the success contract survives as the derived predicate `status is SolveStatus.SUCCESS`, while the receipt also carries *why* a program did not solve. The five routes share one tag-keyed `scipy.optimize` dispatch — the entrypoint, the constraint-assembly probe, the violation reduction, and the host-status adjudication are the row — and the program data admits through `numerics/array.md#PAYLOAD` keying on the same `ContentIdentity` seed. Unlike `design.md` and `solvers/nonlinear.md#NONLINEAR`, this owner carries no numpy floor: the math-program solve *is* `scipy.optimize`, so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate, mirroring the no-floor hull/Delaunay routes of `analysis/spatial.md#QUERY` where Qhull is the gated capability itself. The certified optimum graduates as the existing `solver` `HandoffAxis` case through `graduation/handoff.md#GRADUATION`, the discrete/constrained sibling of the differentiable design optimum on the one rail.

## [01]-[INDEX]

- [01]-[PROGRAM]: linear / integer / global / constrained / assignment math programs over `scipy.optimize` folding the `program` case of the shared content-keyed `OutcomeReceipt` on the one `ProgramIntent` owner.

## [02]-[PROGRAM]

- Owner: `ProgramIntent` — the math-program cases discriminated by constraint-and-integrality structure recoverable from the problem value itself, never a differentiable objective; `Linear(c, a_ub, b_ub, bounds, *, a_eq, b_eq)` over `scipy.optimize.linprog` on the HiGHS backend threading both the inequality `A_ub x ≤ b_ub` and the equality `A_eq x = b_eq` blocks (each block passed only when non-empty, so a pure-inequality, pure-equality, or mixed LP is one shape, never a parallel equality-LP owner), `Integer(c, integrality, bounds, constraints)` over `scipy.optimize.milp` threading the integrality vector and the `Bounds` box, `Global(objective, bounds)` (the `stochastic` case, the python-keyword-free tag for the derivative-free global search) over `scipy.optimize.differential_evolution` seeded for a reproducible content-keyed solve, `Constrained(objective, x0, bounds, constraints)` over `scipy.optimize.minimize(method="trust-constr")` threading the `LinearConstraint`/`NonlinearConstraint` carriers **directly** (the documented `minimize` constraint-carrier route, never lowered to legacy `{"type": "ineq", "fun": ...}` dicts scipy already accepts), and `Assignment(cost)` over `scipy.optimize.linear_sum_assignment`. The five routes share one tag-keyed dispatch in `_program_receipt` — the `scipy.optimize` entrypoint, the constraint carrier, and the one `_violation` fold are the row behind the gated import, never five parallel helper bodies. The discriminant is the program shape, so the gradient loop and the math-program loop are sibling cases on the one `optimization` sub-domain, never a duplicated optimizer surface beside the solve.
- Entry: `ProgramIntent.solve(intent, *, seed)` enters one `boundary(f"program.{intent.tag}", ...)`; every route reads the `OptimizeResult` (or the `linear_sum_assignment` row/column pair) into `OutcomeReceipt.Program` carrying the `SolveStatus` host verdict, the objective value, and the maximum constraint-violation residual, and keys by `ContentIdentity.of` over the canonical problem-data buffer. The `Linear`/`Integer` routes read `result.fun`/`result.x` and fold `result.status` — the documented `linprog`/`milp` integer termination code (`0` optimal, `1` iteration-limit, `2` infeasible, `3` unbounded, `4` numerical) — through `_program_status(result, coded=True)` into `SolveStatus`; the `Global`/`Constrained` routes (whose `differential_evolution`/`minimize` `OptimizeResult` carries the boolean `.success` rather than the uniform integer code) fold through `_program_status(result, coded=False)` reading `.success`; the per-route `coded` flag replaces a reflective `getattr(result, "status")` probe — the dispatch already knows which result shape each scipy entry returns. The `Assignment` route is feasible by construction, so it folds `SolveStatus.SUCCESS` and a zero violation. The residual reduces through the ONE `_violation` fold over `LinearConstraint`/`NonlinearConstraint` carriers — the `Linear` route reifies its `(A_ub, b_ub)` inequality block as `LinearConstraint(A_ub, -inf, b_ub)` and its `(A_eq, b_eq)` block as `LinearConstraint(A_eq, b_eq, b_eq)` so every route's violation flows through one `max(0, lb − Ax, Ax − ub)` reduction rather than a raw-row probe parallel to the carrier probe — and is `inf` on a non-`SUCCESS` LP/MILP where no feasible iterate exists.
- Receipt: `OutcomeReceipt.contribute` matches the `program` case to emit one `Receipt.of("emitted", "compute.optimization.program", ...)` row carrying the program tag, the objective value, the `SolveStatus` termination verdict, the derived `converged` success flag, the constraint violation, and the content key; a certified optimum (`status is SolveStatus.SUCCESS` with a within-tolerance violation) graduates outward through `graduation/handoff.md#GRADUATION` on the existing `solver` `HandoffAxis` case — no new literal, no graduation edit, the discrete/constrained counterpart of the `design.md` differentiable optimum on the one rail. The status vocabulary is the same one the graduation gate reads for the `solver`-axis convergence verdict, so an infeasible or unbounded program is an admission rejection carrying its termination reason rather than a bare `False`.
- Packages: `scipy` (`optimize.linprog` threading `A_ub`/`b_ub`/`A_eq`/`b_eq`, `optimize.milp`, `optimize.differential_evolution` with `seed`, `optimize.minimize` on `method="trust-constr"` consuming the constraint carriers directly, `optimize.linear_sum_assignment`, `optimize.Bounds`, `optimize.LinearConstraint`, `optimize.NonlinearConstraint`, `optimize.OptimizeResult` — the `.status` integer diagnostic for `linprog`/`milp` and the `.success`/`.fun`/`.x` fields all catalogued in `compute/.api/scipy.md`'s `scipy.optimize` entrypoint and public-type tables), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `empty`, `maximum`, `inf` — the canonical problem-data buffer, the empty-block sentinels, and the constraint-violation max-reduction), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, bounds, and integrality admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `solvers/receipt.md#RECEIPT` (`SolveStatus` — the ONE bounded termination vocabulary the host `OptimizeResult` verdict folds into, the same `StrEnum` every solver route adjudicates), `optimization/design.md#DESIGN` (`OutcomeReceipt` — the shared optimization-outcome receipt this owner folds its `program` case into, carrying the `Receipt`/`ReceiptContributor` contribution), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`).
- Growth: a new math-program route is one `ProgramIntent` case plus one row in the `_program_receipt` route table folding the shared `OutcomeReceipt.Program`; a new constraint block (equality, inequality) is one reified `LinearConstraint`/`NonlinearConstraint` carrier flowing through the existing `_violation` fold, never a new violation helper; a new host termination code is one `_PROGRAM_STATUS` row mapping the `OptimizeResult.status` integer into the existing `SolveStatus` vocabulary; zero new surface, never a per-program owner, never a parallel linear-program-and-assignment owner, never a per-route `_*_receipt` helper body, never a per-shape violation probe parallel to the carrier fold, never a legacy constraint-dict lowering parallel to the carrier the host already accepts, never a second optimization-outcome receipt struct beside `OutcomeReceipt`, never a boolean termination notion parallel to the shared `SolveStatus`.
- Boundary: constrained, global, and discrete optimization over `scipy.optimize` only — the linear program, the mixed-integer program, the derivative-free global minimum, the constrained smooth minimum, and the optimal assignment are in-scope; the differentiable inverse-design loop stays on `design.md` and never duplicates here. The termination vocabulary is consumed, not owned: `SolveStatus` lives on `solvers/receipt.md#RECEIPT` and this owner maps the host `OptimizeResult` verdict into it at the boundary, never re-declaring a parallel program-status enum and never folding the duality-gap certificate `convex.md#CONVEX` carries on its distinct `ConvexReceipt`. This owner carries **no numpy floor**: the math program *is* `scipy.optimize` (HiGHS LP, the MILP branch-and-bound, the DE population, the assignment Hungarian), so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate — the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR` (which carry a reachable numpy central-difference floor), matching the no-floor hull/Delaunay/Voronoi routes of `analysis/spatial.md#QUERY`. A training loop, a production solver session, a hand-rolled simplex or branch-and-bound kernel `scipy.optimize` owns, a legacy `{"type": "ineq", "fun": ...}` constraint-dict lowering parallel to the `LinearConstraint`/`NonlinearConstraint` carrier `minimize(method="trust-constr")` already consumes, a raw-row violation probe parallel to the carrier-keyed `_violation` fold, a parallel optimizer surface beside the differentiable solve, and a `bool success` field parallel to the shared `SolveStatus` are the deleted forms.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.compute.optimization.design import OutcomeReceipt
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary

_SEED = 0
_EMPTY_1D: np.ndarray = np.empty(0, dtype=float)
_EMPTY_2D: np.ndarray = np.empty((0, 0), dtype=float)


@tagged_union(frozen=True)
class ProgramIntent:
    tag: Literal["linear", "integer", "stochastic", "constrained", "assignment"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, tuple[tuple[float, float], ...]] = case()
    integer: tuple[np.ndarray, np.ndarray, tuple[tuple[float, float], ...], tuple[object, ...]] = case()
    stochastic: tuple[object, tuple[tuple[float, float], ...]] = case()
    constrained: tuple[object, np.ndarray, tuple[tuple[float, float], ...], tuple[object, ...]] = case()
    assignment: np.ndarray = case()

    @staticmethod
    def Linear(
        c: np.ndarray,
        a_ub: np.ndarray = _EMPTY_2D,
        b_ub: np.ndarray = _EMPTY_1D,
        bounds: tuple[tuple[float, float], ...] = (),
        *,
        a_eq: np.ndarray = _EMPTY_2D,
        b_eq: np.ndarray = _EMPTY_1D,
    ) -> "ProgramIntent":
        return ProgramIntent(linear=(c, a_ub, b_ub, a_eq, b_eq, bounds))

    @staticmethod
    def Integer(
        c: np.ndarray, integrality: np.ndarray, bounds: tuple[tuple[float, float], ...], constraints: tuple[object, ...] = ()
    ) -> "ProgramIntent":
        return ProgramIntent(integer=(c, integrality, bounds, constraints))

    @staticmethod
    def Global(objective: Callable[[np.ndarray], float], bounds: tuple[tuple[float, float], ...]) -> "ProgramIntent":
        return ProgramIntent(stochastic=(objective, bounds))

    @staticmethod
    def Constrained(
        objective: Callable[[np.ndarray], float], x0: np.ndarray, bounds: tuple[tuple[float, float], ...], constraints: tuple[object, ...] = ()
    ) -> "ProgramIntent":
        return ProgramIntent(constrained=(objective, x0, bounds, constraints))

    @staticmethod
    def Assignment(cost: np.ndarray) -> "ProgramIntent":
        return ProgramIntent(assignment=cost)


_PROGRAM_STATUS: FrozenDict[int, SolveStatus] = FrozenDict(
    {
        0: SolveStatus.SUCCESS,
        1: SolveStatus.MAX_STEPS,
        2: SolveStatus.INFEASIBLE,
        3: SolveStatus.UNBOUNDED,
        4: SolveStatus.ILL_CONDITIONED,
    }
)


def _program_status(result: object, *, coded: bool) -> SolveStatus:
    if coded:
        return _PROGRAM_STATUS.get(int(result.status), SolveStatus.OTHER)
    return SolveStatus.SUCCESS if bool(result.success) else SolveStatus.STAGNATION


def _program_key(intent: ProgramIntent, fields: tuple[np.ndarray, ...]) -> ContentKey:
    buffer = b"".join(np.ascontiguousarray(field).tobytes() for field in fields if field.size)
    return ContentIdentity.of(f"program.{intent.tag}", buffer, IdentityPolicy())


def solve(intent: ProgramIntent, *, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    return boundary(f"program.{intent.tag}", lambda: _program_receipt(intent, seed))


def _program_receipt(intent: ProgramIntent, seed: int) -> OutcomeReceipt:
    from scipy.optimize import LinearConstraint, differential_evolution, linear_sum_assignment, linprog, milp, minimize

    match intent:
        case ProgramIntent(tag="linear", linear=(c, a_ub, b_ub, a_eq, b_eq, bounds)):
            cost = np.asarray(c, dtype=float)
            ub_mat, ub_rhs = np.atleast_2d(np.asarray(a_ub, dtype=float)), np.asarray(b_ub, dtype=float)
            eq_mat, eq_rhs = np.atleast_2d(np.asarray(a_eq, dtype=float)), np.asarray(b_eq, dtype=float)
            result = linprog(
                cost,
                A_ub=ub_mat if ub_rhs.size else None,
                b_ub=ub_rhs if ub_rhs.size else None,
                A_eq=eq_mat if eq_rhs.size else None,
                b_eq=eq_rhs if eq_rhs.size else None,
                bounds=bounds or None,
                method="highs",
            )
            status = _program_status(result, coded=True)
            solved = status is SolveStatus.SUCCESS
            carriers = (
                *(LinearConstraint(ub_mat, -np.inf, ub_rhs),) * bool(ub_rhs.size),
                *(LinearConstraint(eq_mat, eq_rhs, eq_rhs),) * bool(eq_rhs.size),
            )
            x = np.asarray(result.x, dtype=float) if solved else _EMPTY_1D
            objective = float(result.fun) if solved else float("inf")
            violation = _violation(carriers, x) if solved else float("inf")
            return OutcomeReceipt.Program("linear", objective, status, violation, _program_key(intent, (cost, ub_mat, ub_rhs, eq_mat, eq_rhs)))
        case ProgramIntent(tag="integer", integer=(c, integrality, bounds, constraints)):
            cost, flags = np.asarray(c, dtype=float), np.asarray(integrality)
            result = milp(cost, integrality=flags, bounds=_bounds(bounds), constraints=list(constraints) or None)
            status = _program_status(result, coded=True)
            solved = status is SolveStatus.SUCCESS
            objective = float(result.fun) if solved else float("inf")
            violation = _violation(constraints, np.asarray(result.x, dtype=float)) if solved else float("inf")
            return OutcomeReceipt.Program("integer", objective, status, violation, _program_key(intent, (cost, flags)))
        case ProgramIntent(tag="stochastic", stochastic=(objective_fn, bounds)):
            box = np.asarray([[lo, hi] for lo, hi in bounds], dtype=float)
            result = differential_evolution(objective_fn, box, rng=seed)
            return OutcomeReceipt.Program("stochastic", float(result.fun), _program_status(result, coded=False), 0.0, _program_key(intent, (box,)))
        case ProgramIntent(tag="constrained", constrained=(objective_fn, x0, bounds, constraints)):
            start = np.asarray(x0, dtype=float)
            result = minimize(objective_fn, start, method="trust-constr", bounds=_bounds(bounds), constraints=list(constraints))
            status = _program_status(result, coded=False)
            objective = float(result.fun) if status is SolveStatus.SUCCESS else float("inf")
            violation = _violation(constraints, np.asarray(result.x, dtype=float))
            return OutcomeReceipt.Program("constrained", objective, status, violation, _program_key(intent, (start,)))
        case ProgramIntent(tag="assignment", assignment=cost):
            matrix = np.atleast_2d(np.asarray(cost, dtype=float))
            rows, cols = linear_sum_assignment(matrix)
            return OutcomeReceipt.Program("assignment", float(matrix[rows, cols].sum()), SolveStatus.SUCCESS, 0.0, _program_key(intent, (matrix,)))
        case unreachable:
            assert_never(unreachable)


def _bounds(bounds: tuple[tuple[float, float], ...]) -> object:
    from scipy.optimize import Bounds

    box = np.asarray(bounds, dtype=float).reshape(-1, 2)
    return Bounds(box[:, 0], box[:, 1])


def _violation(constraints: tuple[object, ...], x: np.ndarray) -> float:
    if not x.size:
        return float("inf")
    residual = (
        float(np.maximum(np.maximum(con.lb - (value := con.A @ x if hasattr(con, "A") else np.asarray(con.fun(x))), value - con.ub), 0.0).max(initial=0.0))
        for con in constraints
    )
    return float(max(residual, default=0.0))
```

## [03]-[RESEARCH]

- [SCIPY_OPTIMIZE]: the `scipy.optimize.linprog(c, A_ub, b_ub, A_eq, b_eq, bounds)` (the catalogued signature threading both the inequality and equality blocks, each passed only when non-empty), `milp(c, integrality, bounds, constraints)`, `differential_evolution(func, bounds, strategy)` (driven through the SPEC-007 `rng` keyword for a reproducible global solve so the content key maps to a fixed result — `seed` is the deprecated interim alias `compute/.api/scipy.md` flags), `minimize(fun, x0, method, bounds, constraints)` on `method="trust-constr"` consuming the `LinearConstraint`/`NonlinearConstraint` carriers directly (the catalogued constraint-carrier route — line 208's `constraints route through Bounds, LinearConstraint, and NonlinearConstraint` — never a legacy `{"type": "ineq", "fun": ...}` dict lowering scipy already owns), and `linear_sum_assignment(cost_matrix)` spellings — with the `Bounds`/`LinearConstraint`/`NonlinearConstraint` constraint carriers and the `OptimizeResult.status`/`.success`/`.fun`/`.x` result fields — are fully catalogued in `compute/.api/scipy.md`'s `scipy.optimize` entrypoint table (`linprog`/`milp`/`differential_evolution`/`linear_sum_assignment`) and public-type table (`OptimizeResult` carrying the solution, success flag, and diagnostics), so the body verifies against the present catalogue directly. The `linprog`/`milp` `OptimizeResult.status` integer code — `0` optimal, `1` iteration/time limit, `2` infeasible, `3` unbounded, `4` numerical difficulty — is the documented HiGHS/MILP termination diagnostic the `_PROGRAM_STATUS` table reads; `differential_evolution`/`minimize` surface the boolean `.success` rather than that uniform code, so those routes adjudicate through the `.success` branch of the same fold. The spellings carry the `python_version<'3.15'` scipy marker and settle against the installed wheel under a uv-sync reflection pass on that gated band; this owner carries no numpy floor because `scipy.optimize` is the gated capability itself, so a cp315 run without the scipy wheel returns `Error(Import)` for every route — the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR`.
- [PROGRAM_STATUS]: `_program_status` folds the host `OptimizeResult` termination into the `SolveStatus` `StrEnum` `solvers/receipt.md#RECEIPT` owns — the `linprog`/`milp` integer `.status` through the `_PROGRAM_STATUS` boundary table (`0`→`SUCCESS`, `1`→`MAX_STEPS` iteration/time limit, `2`→`INFEASIBLE`, `3`→`UNBOUNDED`, `4`→`ILL_CONDITIONED` numerical), reading the two feasibility-verdict members `solvers/receipt.md#RECEIPT` carries for exactly this discrete/constrained case rather than collapsing an infeasible LP onto an iterative `BREAKDOWN` or an unbounded LP onto a `DIVERGENCE`, and the `differential_evolution`/`minimize` `.success` flag through `SUCCESS`/`STAGNATION` — so the math-program feasibility verdict speaks the one vocabulary every solver route and the graduation gate read, never a parallel program-status enum. The `program` case of `OutcomeReceipt` carries this `SolveStatus`, and the `success` contract survives as the derived predicate `status is SolveStatus.SUCCESS` in `OutcomeReceipt.contribute` — the host receipt collapse that retires the lone `bool success` field while preserving the typed feasibility receipt as the `program` case projection. The fold lives at this boundary so `solvers/receipt.md#RECEIPT` stays the vocabulary owner and the `program` case stays one of the two `OutcomeReceipt` verdicts (`optimization/design.md#DESIGN`), whose `program` tuple is `(str, float, SolveStatus, float, ContentKey)` carrying the status in the third slot the prior `bool` held.
- [PROGRAM_CONTENT_KEY]: `_program_key` derives the `ContentKey` over the canonical contiguous problem-data buffer (the concatenated `tobytes()` of the cost vector, the inequality and equality constraint blocks, integrality, or cost matrix — empty blocks contribute nothing through the `field.size` guard so a pure-inequality and a mixed LP key distinctly only when the equality block is populated) through `ContentIdentity.of("program.<tag>", ...)` under the runtime `IdentityPolicy`, so a program whose data admits through `numerics/array.md#PAYLOAD` keys under the same `ContentIdentity` seed algebra and a repeated solve on identical data is a cache hit by reference. The objective callables of the `Global` and `Constrained` cases are excluded from the key buffer — only the array-shaped problem data seeds identity, matching the payload admission's host-transfer buffer; the `Global` route's `seed` makes the keyed `differential_evolution` solve reproducible so the cache hit returns the same global iterate.
