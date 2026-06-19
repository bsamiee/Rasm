# [PY_COMPUTE_CONVEX]

The dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval.md#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in — a linear/affine program, a quadratic program, a second-order-cone program, and a semidefinite program — each built from `cvxpy` `Variable`/`Parameter` leaves under `Minimize`/`Maximize` and the relational cone algebra (`==`, `<=`, `>=`, `>>`), compiled to standard conic form and solved through the Clarabel interior-point backend that returns the primal optimum and the per-constraint dual multipliers. The four cases share one tag-keyed `cvxpy` dispatch — the atom that assembles the objective, the constraint stack, and the cone membership are the row — folding one content-keyed `ConvexReceipt` over `problem.value`, every `Constraint.dual_value`, the complementary-slackness KKT gap `Σ|λᵢ · gᵢ(x)|` recovered from those duals and the constraint expression values, and `problem.status`. The KKT gap is the proof object: a vanishing complementary-slackness sum with feasible duals certifies the returned point is the global optimum, the convex sibling of the `Enclosure.certified` flag, so a returned point whose gap exceeds the tolerance is an admission rejection rather than a degraded answer. Like `optimization/program.md#PROGRAM`, this owner carries **no numpy floor** — the convex solve *is* `cvxpy` over the conic backend, so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate, both packages riding the companion `python_version<'3.15'` band. The certified optimum graduates outward on the dedicated `convex-program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`, a distinct admission from the first-order convergence verdict the `solver` axis carries for `optimization/design.md#DESIGN` and `program.md#PROGRAM`.

## [01]-[INDEX]

- [01]-[CONVEX]: linear / quadratic / second-order-cone / semidefinite disciplined-convex programs over `cvxpy` DCP atoms and the Clarabel conic backend, folding one content-keyed `ConvexReceipt` duality-gap optimality certificate on the one `ConvexProgram` owner.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the convex cases discriminated by the cone family the disciplined-convex model lands in, recoverable from the model structure itself, never a hand-rolled cone reduction; `Linear(c, a_ub, b_ub)` over an affine objective and elementwise inequality cone, `Quadratic(p, q, a_ub, b_ub)` carrying a PSD quadratic form through `cp.quad_form` under the same polyhedral cone, `SecondOrder(c, soc_terms, a_ub, b_ub)` adding `cp.SOC`/`cp.norm2` second-order-cone rows, and `Semidefinite(c_mat, a_ub, b_ub)` carrying a matrix `Variable` under the `>>` PSD-cone relation. The four routes share one tag-keyed dispatch in `_convex_receipt` — the `cvxpy` atom that builds the objective expression, the constraint list, and the cone membership are the row behind the gated import, never four parallel solver bodies. The discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on the one `optimization` sub-domain, never a duplicated optimizer surface beside the conic solve.
- Modeling: every case lifts its problem data into one `cvxpy` `Variable`/`Parameter` algebra and composes the objective from DCP atoms (`cp.sum`, `cp.quad_form`, `cp.norm2`, `cp.trace`) so curvature and sign are tracked by the DCP ruleset — a model that violates DCP raises at construction, never silently mis-solves. The objective sign is the `Minimize`/`Maximize` constructor row, the cone memberships are the relational operators and the explicit `cp.SOC`/`>>` rows, and the `Problem(objective, constraints)` compiles once; a parametrized family threads `cp.Parameter` under DPP so a sweep sets `Parameter.value` and re-`solve`s without rebuilding the `Problem`. No case ever assembles a slack reformulation or a manual cone partition the modeling layer owns.
- Backend: `Problem.solve(solver=cp.CLARABEL)` selects the Clarabel primal-dual interior-point backend — the default conic solver and the dual-certificate source — and writes the primal optimum to `Variable.value` and the dual multipliers to each `Constraint.dual_value`. The optimality gap the receipt folds is the complementary-slackness sum `Σ|λᵢ · gᵢ(x)|` recovered from those catalogued cvxpy `dual_value` multipliers and `expr.value` constraint values, never Clarabel's solver-internal `obj_val`/`obj_val_dual` objective pair (which cvxpy does not surface to the modeling layer outside the solver-specific `solver_stats` dict); the standalone `DefaultSolver` over a `get_problem_data` reduction is available when the problem is already in cone-standard form, but the admitted path is the `cvxpy` `solve` selector, never a direct sparse `P`/`q`/`A`/`b` assembly this owner re-derives.
- Entry: `ConvexProgram.solve` enters one `boundary("convex.solve", ...)` returning `RuntimeRail[ConvexReceipt]`; the dispatch matches the tag total over `match`/`assert_never` (a new cone family breaks every site), builds the `cvxpy` `Problem` behind the gated import, solves through Clarabel, and reduces the per-constraint duals and the constraint expression values into the complementary-slackness certificate. Each program keys through `ContentIdentity.of` over the canonical problem-data buffer, so a re-solve from identical data keys identically by reference; the certificate is the convex proof object, so the entry never returns an uncertified estimate the way the design floor would.
- Receipt: `ConvexReceipt.contribute` emits one `Receipt.of("emitted", ...)` row carrying the program tag, the optimal objective, the duality gap, the maximum dual-feasibility infeasibility, the solver status, and the content key; a certified optimum graduates outward through `graduation/handoff.md#GRADUATION` on the dedicated `convex-program` `HandoffAxis` case — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof, a distinct admission from the `solver` axis's first-order convergence verdict, so a `status` other than `optimal` or a gap above tolerance is an admission rejection, never a graduated handoff.
- Packages: `cvxpy` (`Variable`, `Parameter`, `Minimize`, `Maximize`, `Problem`, `Problem.solve`, `Problem.value`, `Problem.status`, `Variable.value`, `Constraint.dual_value`, the `==`/`<=`/`>=`/`>>` relational algebra, the `SOC`/`quad_form`/`norm2`/`trace`/`sum` atoms, the `CLARABEL` backend selector, `installed_solvers` — all catalogued in `compute/.api/cvxpy.md`), `clarabel` (the default conic interior-point backend whose `DefaultSolution.z` conic dual multipliers cvxpy surfaces through the catalogued `Constraint.dual_value`, from which the fence recovers the complementary-slackness KKT gap; its solver-internal `obj_val`/`obj_val_dual` objective pair stays inside Clarabel and is not read, admitted via the `solver=` selector — catalogued in `compute/.api/clarabel.md`), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `maximum` — the canonical problem-data buffer and the dual-infeasibility max-reduction), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, and quadratic form admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `graduation/handoff.md#GRADUATION` (the `convex-program` axis the certified optimum graduates on), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`).
- Growth: a new convex cone family is one `ConvexProgram` case plus one row in the `_convex_receipt` dispatch; a new DCP atom or cone membership is one cell on the row; a parameter sweep is a `cp.Parameter` warm re-solve, never a rebuilt `Problem`; zero new surface, never a per-cone owner, never a parallel linear-program-and-SDP owner, never a per-route `_*_receipt` helper body, never a hand-rolled interior-point or cone reduction `cvxpy` and Clarabel already own.
- Boundary: classical disciplined-convex programming over `cvxpy` and a conic backend only — the linear, quadratic, second-order-cone, and semidefinite programs with a dual-certificate proof of global optimality are in-scope; the differentiable inverse-design loop stays on `design.md` and the discrete/global math program on `program.md`, and neither duplicates here. This owner carries **no numpy floor**: the certified convex solve *is* `cvxpy` over Clarabel (the DCP compilation, the cone reduction, the interior-point iteration), so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate — the deliberate floor asymmetry against `design.md` and matching the no-floor posture of `program.md` and the no-floor Qhull routes of `analysis/spatial.md#QUERY`, because an uncertified convex answer is no certificate at all. A non-convex or neural relaxation, a hand-rolled cone slack reformulation, a mixed-integer branch the conic backend does not own, a production solver session, and a parallel optimizer surface beside the conic solve are the deleted forms; the modeling stays on `cvxpy` and the solve on Clarabel, so this owner composes the certified optimum rather than re-deriving it.

```python signature
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class ConvexReceipt(Struct, frozen=True):
    program: str
    objective: float
    duality_gap: float
    dual_infeasibility: float
    status: str
    content_key: ContentKey

    def contribute(self) -> Receipt:
        facts = {
            "program": self.program,
            "objective": f"{self.objective:.6g}",
            "duality_gap": f"{self.duality_gap:.3e}",
            "dual_infeasibility": f"{self.dual_infeasibility:.3e}",
            "status": self.status,
            "key": str(self.content_key.value),
        }
        return Receipt.of("emitted", "compute.optimization.convex", self.program, facts)


@tagged_union(frozen=True)
class ConvexProgram:
    tag: Literal["linear", "quadratic", "second_order", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray] = case()

    @staticmethod
    def Linear(c: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray) -> "ConvexProgram":
        return ConvexProgram(linear=(c, a_ub, b_ub))

    @staticmethod
    def Quadratic(p: np.ndarray, q: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray) -> "ConvexProgram":
        return ConvexProgram(quadratic=(p, q, a_ub, b_ub))

    @staticmethod
    def SecondOrder(c: np.ndarray, soc_terms: tuple[tuple[np.ndarray, float], ...], a_ub: np.ndarray, b_ub: np.ndarray) -> "ConvexProgram":
        return ConvexProgram(second_order=(c, soc_terms, a_ub, b_ub))

    @staticmethod
    def Semidefinite(c_mat: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray) -> "ConvexProgram":
        return ConvexProgram(semidefinite=(c_mat, a_ub, b_ub))


def _convex_key(program: ConvexProgram, fields: tuple[np.ndarray, ...]) -> ContentKey:
    buffer = b"".join(np.ascontiguousarray(np.asarray(field, dtype=float)).tobytes() for field in fields)
    return ContentIdentity.of(f"convex.{program.tag}", buffer, IdentityPolicy())


def solve(program: ConvexProgram) -> "RuntimeRail[ConvexReceipt]":
    return boundary("convex.solve", lambda: _convex_receipt(program))


def _convex_receipt(program: ConvexProgram) -> ConvexReceipt:
    import cvxpy as cp

    match program:
        case ConvexProgram(tag="linear", linear=(c, a_ub, b_ub)):
            cost, mat, rhs = _as_vec(c), _as_mat(a_ub), _as_vec(b_ub)
            x = cp.Variable(cost.size)
            constraints = [mat @ x <= rhs]
            problem = cp.Problem(cp.Minimize(cost @ x), constraints)
            key = _convex_key(program, (cost, mat, rhs))
        case ConvexProgram(tag="quadratic", quadratic=(p, q, a_ub, b_ub)):
            form, lin, mat, rhs = _as_mat(p), _as_vec(q), _as_mat(a_ub), _as_vec(b_ub)
            x = cp.Variable(lin.size)
            constraints = [mat @ x <= rhs]
            objective = 0.5 * cp.quad_form(x, cp.psd_wrap(form)) + lin @ x
            problem = cp.Problem(cp.Minimize(objective), constraints)
            key = _convex_key(program, (form, lin, mat, rhs))
        case ConvexProgram(tag="second_order", second_order=(c, soc_terms, a_ub, b_ub)):
            cost, mat, rhs = _as_vec(c), _as_mat(a_ub), _as_vec(b_ub)
            x = cp.Variable(cost.size)
            constraints = [mat @ x <= rhs]
            constraints += [cp.norm2(_as_mat(a) @ x) <= bound for a, bound in soc_terms]
            problem = cp.Problem(cp.Minimize(cost @ x), constraints)
            key = _convex_key(program, (cost, mat, rhs))
        case ConvexProgram(tag="semidefinite", semidefinite=(c_mat, a_ub, b_ub)):
            cost, mat, rhs = _as_mat(c_mat), _as_mat(a_ub), _as_vec(b_ub)
            n = cost.shape[0]
            x = cp.Variable((n, n), symmetric=True)
            constraints = [x >> 0, mat @ cp.reshape(x, (n * n,), order="C") <= rhs]
            problem = cp.Problem(cp.Minimize(cp.trace(cost @ x)), constraints)
            key = _convex_key(program, (cost, mat, rhs))
        case unreachable:
            assert_never(unreachable)

    problem.solve(solver=cp.CLARABEL)
    return _certificate(program.tag, problem, constraints, key)


def _certificate(tag: str, problem: object, constraints: list[object], key: ContentKey) -> ConvexReceipt:
    objective = float(problem.value) if problem.value is not None else float("inf")
    gap = _duality_gap(problem, constraints)
    infeasibility = _dual_infeasibility(constraints)
    return ConvexReceipt(tag, objective, gap, infeasibility, str(problem.status), key)


def _duality_gap(problem: object, constraints: list[object]) -> float:
    if problem.value is None:
        return float("inf")
    terms = [
        float(np.abs(np.asarray(con.dual_value, dtype=float) * np.asarray(con.expr.value, dtype=float)).sum())
        for con in constraints
        if con.dual_value is not None and con.expr.value is not None
    ]
    return float(sum(terms))


def _dual_infeasibility(constraints: list[object]) -> float:
    duals = [np.asarray(con.dual_value, dtype=float) for con in constraints if con.dual_value is not None]
    return float(max((np.maximum(-d, 0.0).max(initial=0.0) for d in duals), default=0.0))


def _as_vec(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.asarray(array, dtype=float).ravel())


def _as_mat(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.atleast_2d(np.asarray(array, dtype=float)))
```

## [03]-[RESEARCH]

- [CVXPY_SOLVE]: `cvxpy` resolves on the companion `python_version<'3.15'` band (cp311-cp314 wheels only; no CPython 3.15 wheel); the `Variable`/`Parameter`/`Minimize`/`Maximize`/`Problem` modeling algebra, the `==`/`<=`/`>=`/`>>` relational constraint operators, the `quad_form`/`psd_wrap`/`norm2`/`trace`/`reshape`/`sum` DCP atoms, `Problem.solve(solver=cp.CLARABEL)`, and `Problem.value`/`Problem.status`/`Variable.value`/`Constraint.dual_value`/`problem.solver_stats` verify against `compute/.api/cvxpy.md` under a uv-sync reflection pass on that band — the catalogue is documentation-authored RESEARCH-capture-pending-on-uv-sync, not yet reflection-verified against an installed wheel, so the spellings settle on uv sync into the companion interpreter band. DCP curvature tracking raises at construction on a non-convex model, so the certificate is never produced for a problem the modeling layer rejects; this owner carries no numpy floor because `cvxpy` over the conic backend *is* the certified capability, so a cp315 run without the wheel returns `Error(Import)` — the deliberate floor asymmetry against `optimization/design.md#DESIGN`.
- [CLARABEL_DUAL]: `clarabel` is the default conic interior-point backend `cvxpy` selects through `solver=cp.CLARABEL`; the primal-dual certificate — `DefaultSolution.z` conic dual multipliers, `DefaultSolution.s` primal slack, and the `obj_val`/`obj_val_dual` primal/dual objective whose difference is the backend duality gap — is surfaced to the modeling layer through the catalogued `Constraint.dual_value` multipliers and the primal `Variable.value`, so the fence reads the certificate from those catalogued cvxpy attributes (the complementary-slackness sum `Σ|λᵢ · gᵢ(x)|` over the recovered duals and constraint expression values is the KKT optimality gap, vanishing exactly at the global optimum), never the solver-specific `solver_stats.extra_stats` raw dict; the spellings verify against `compute/.api/clarabel.md` once the Rust-native cp39-abi3 wheel syncs into the active venv on the companion band. The effective convex gate is `python_version<'3.15'` on **both** packages: the abi3-capable Clarabel wheel transitively pulls `scipy` (gast/pythran lack 3.15 support) and `cvxpy` ships cp311-cp314 wheels only, so the path is companion-band on the pair and never authored as cp315-runnable. The vanishing duality gap with feasible duals is the global-optimality proof object the `convex-program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads to admit or reject the handoff — the convex analogue of the `Enclosure.certified` flag in `numerics/interval.md#ENCLOSURE`.
