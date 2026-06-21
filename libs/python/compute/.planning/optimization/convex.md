# [PY_COMPUTE_CONVEX]

The dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval.md#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in — a linear/affine program, a quadratic program, a second-order-cone program, an exponential-cone program, and a semidefinite program — each built from `cvxpy` `Variable`/`Parameter` leaves under a `Sense`-rowed `Minimize`/`Maximize` objective and the relational cone algebra (`==`, `<=`, `>=`, `>>`), compiled to standard conic form and solved through the Clarabel interior-point backend that returns the primal optimum and the per-constraint dual multipliers. The objective sense is a `Minimize`/`Maximize` policy row on the program, never a parallel maximize-owner; a parametrized family threads `cp.Parameter` leaves so a sweep binds `Parameter.value` and warm-re-`solve`s the one compiled DPP `Problem` across the bind table rather than rebuilding it. The five cases share one tag-keyed `cvxpy` dispatch — the atom that assembles the objective, the constraint stack, and the cone membership are the row — folding one content-keyed `ConvexReceipt` over `problem.value`, every `Constraint.dual_value`, the complementary-slackness KKT gap `Σ|⟨λᵢ, gᵢ(x)⟩|` recovered from those duals and the constraint-expression values, and the cvxpy `problem.status` string folded into the shared `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns. The KKT gap is the proof object: a vanishing complementary-slackness sum with feasible duals certifies the returned point is the global optimum, the convex sibling of the `Enclosure.certified` flag, so a returned point whose gap exceeds the tolerance is an admission rejection rather than a degraded answer. `ConvexReceipt` stays a **distinct** typed receipt and never folds into the `OutcomeReceipt` the `design`/`program` siblings share: the duality-gap, dual-infeasibility, and `SolveStatus` certificate is the global-optimality proof object the first-order convergence verdict and the feasibility verdict carry no field for, so collapsing it would erase the spectral certificate the route-owned-receipt law preserves; the coherence with `solvers/receipt.md#RECEIPT` is the **status vocabulary**, not the carrier — the convex status reads through the one `SolveStatus` enum the C# graduation gate and the sibling solve receipts read, mapped from the cvxpy status constants through the same boundary-table fold the solver routes use for the `RESULTS` enums. Like `optimization/program.md#PROGRAM`, this owner carries **no numpy floor** — the convex solve *is* `cvxpy` over the conic backend, so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate, both packages riding the companion `python_version<'3.15'` band. The certified optimum graduates outward on the dedicated `convex-program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`, a distinct admission from the first-order convergence verdict the `solver` axis carries for `optimization/design.md#DESIGN` and `program.md#PROGRAM`.

## [01]-[INDEX]

- [01]-[CONVEX]: linear / quadratic / second-order-cone / exponential-cone / semidefinite disciplined-convex programs over `cvxpy` DCP atoms and the Clarabel conic backend, under a `Minimize`/`Maximize` `Sense` policy row and a DPP `Parameter` warm-re-solve sweep axis, folding one content-keyed `ConvexReceipt` duality-gap optimality certificate — distinct from the shared `OutcomeReceipt`, carrying the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns — on the one `ConvexProgram` owner.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the convex cases discriminated by the cone family the disciplined-convex model lands in, recoverable from the model structure itself, never a hand-rolled cone reduction; `Linear(c, a_ub, b_ub)` over an affine objective and elementwise inequality cone, `Quadratic(p, q, a_ub, b_ub)` carrying a symmetrized quadratic form through `cp.quad_form` under the same polyhedral cone, `SecondOrder(c, soc_terms, a_ub, b_ub)` adding `cp.norm2` second-order-cone rows, `Exponential(c, exp_terms, a_ub, b_ub)` adding `cp.log_sum_exp`/`cp.rel_entr` exponential-cone rows, and `Semidefinite(c_mat, a_ub, b_ub)` carrying a matrix `Variable` under the `>>` PSD-cone relation. Each factory carries the optional `sense: Sense` policy row (`MIN` default, `MAX` for a concave objective) and an optional `params: ParamBind` warm-re-solve table, so the objective sign and the parameter sweep are rows on the one owner, never a parallel maximize-owner or a per-sweep rebuilt `Problem`. The five routes share one tag-keyed dispatch in `_assemble` returning the `(Problem, Variable, constraints, parameters)` quad — the `cvxpy` atom that builds the objective expression, the constraint list, and the cone membership are the row behind the gated import, never five parallel solver bodies. The discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on the one `optimization` sub-domain, never a duplicated optimizer surface beside the conic solve.
- Modeling: every case lifts its problem data into one `cvxpy` `Variable`/`Parameter` algebra and composes the objective from DCP atoms (`cp.sum`, `cp.quad_form`, `cp.norm2`, `cp.log_sum_exp`, `cp.rel_entr`, `cp.trace`) so curvature and sign are tracked by the DCP ruleset — a model that violates DCP raises at construction, never silently mis-solves. The objective sign is the `Sense` policy row folded into the `cp.Minimize`/`cp.Maximize` constructor through the `_objective` sense fold, the cone memberships are the relational operators and the explicit `>>`/`cp.norm2`/`cp.log_sum_exp` rows, and the `Problem(objective, constraints)` compiles once. The quadratic form is symmetrized at the numpy edge (`0.5·(P+Pᵀ)`) before `cp.quad_form` so the catalogued atom carries a DCP-legal symmetric form — a genuinely indefinite form is the DCP rejection, never a silent `psd_wrap` coercion the catalogue does not own. A parametrized family declares `cp.Parameter` leaves over the named bind keys under DPP, so each `ParamBind` row sets `Parameter.value` and warm-re-`solve`s the same compiled `Problem` across the sweep, folding one `ConvexReceipt` per bind. No case ever assembles a slack reformulation or a manual cone partition the modeling layer owns.
- Backend: `Problem.solve(solver=cp.CLARABEL, warm_start=True)` selects the Clarabel primal-dual interior-point backend — the default conic solver and the dual-certificate source — and writes the primal optimum to `Variable.value` and the dual multipliers to each `Constraint.dual_value`; `warm_start` reuses the prior factorization across a `Parameter` sweep. The entry preflights `cp.CLARABEL in cp.installed_solvers()` once and degrades a missing backend to `SolveStatus.OTHER` rather than letting the solve raise mid-fold. The optimality gap the receipt folds is the complementary-slackness sum `Σ|⟨λᵢ, gᵢ(x)⟩|` (the elementwise dual-by-expression Frobenius inner product, so the PSD row contributes `|tr(Z·X)|` and the polyhedral rows contribute `|λ·(rhs−Ax)|` under one elementwise-product-then-sum) recovered from those catalogued cvxpy `dual_value` multipliers and `Constraint.expr.value` constraint-expression values; the companion dual-feasibility residual is cone-aware — an elementwise `max(−λ, 0)` for the polyhedral/SOC/exponential inequality duals and the negative minimum eigenvalue `max(−λ_min(Z), 0)` for the PSD-cone matrix dual, never a single elementwise sign test misapplied across the matrix cone — and neither reads Clarabel's solver-internal `obj_val`/`obj_val_dual` objective pair (which cvxpy does not surface to the modeling layer outside the solver-specific `solver_stats` dict); the standalone `DefaultSolver` over a `get_problem_data` reduction is available when the problem is already in cone-standard form, but the admitted path is the `cvxpy` `solve` selector, never a direct sparse `P`/`q`/`A`/`b` assembly this owner re-derives.
- Entry: `ConvexProgram.solve` enters one `boundary("convex.solve", ...)` returning `RuntimeRail[tuple[ConvexReceipt, ...]]` — one receipt for a single solve, one per `ParamBind` row for a sweep; the dispatch matches the tag total over `match`/`assert_never` (a new cone family breaks every site), builds the `cvxpy` `Problem` behind the gated import, binds each parameter row, solves through Clarabel, and reduces the per-constraint duals and the constraint expression values into the complementary-slackness certificate. Each program-and-bind keys through `ContentIdentity.of` over the canonical problem-data buffer seeded with the bound parameter values, so a re-solve from identical data and bind keys identically by reference; the certificate is the convex proof object, so the entry never returns an uncertified estimate the way the design floor would.
- Receipt: `ConvexReceipt` is a **distinct** typed receipt, never the shared `OutcomeReceipt` `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` fold their convergence and feasibility verdicts into — it carries the duality-gap/dual-infeasibility/`SolveStatus` global-optimality certificate the first-order and feasibility verdicts have no field for, so folding it onto `OutcomeReceipt` would erase the proof object. Its coherence with `solvers/receipt.md#RECEIPT` is the termination vocabulary: the `status` field is the one `SolveStatus` `StrEnum` the solver routes carry, the cvxpy status string mapped through `_CONVEX_STATUS` (the convex sibling of the `_STATUS` `RESULTS` boundary table) — `optimal`/`optimal_inaccurate` fold to `SUCCESS`/`STAGNATION`, `infeasible`/`unbounded` to `INFEASIBLE`/`UNBOUNDED`, the `*_inaccurate` and `solver_error`/`user_limit` variants to the corresponding degraded verdict, an unmapped constant degrading to `OTHER` rather than crashing. `ConvexReceipt.contribute` emits one `Receipt.of("emitted", ...)` row carrying the program tag, the optimal objective, the duality gap, the cone-aware maximum dual-feasibility residual, the `SolveStatus`, and the content key; a certified optimum graduates outward through `graduation/handoff.md#GRADUATION` on the dedicated `convex-program` `HandoffAxis` case — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof, a distinct admission from the `solver` axis's first-order convergence verdict, so a `status` other than `SolveStatus.SUCCESS` or a gap above tolerance is an admission rejection, never a graduated handoff.
- Packages: `cvxpy` (`Variable`, `Parameter`, `Minimize`, `Maximize`, `Problem`, `Problem.solve`, `Problem.value`, `Problem.status`, `Variable.value`, `Constraint.dual_value`, `Constraint.expr` (whose `.value` recovers the constraint-expression values the complementary-slackness sum folds), the `==`/`<=`/`>=`/`>>` relational algebra, the `quad_form`/`norm2`/`log_sum_exp`/`rel_entr`/`trace`/`reshape`/`sum` DCP atoms, the `CLARABEL` backend selector, `installed_solvers` — all catalogued in `compute/.api/cvxpy.md`), `clarabel` (the default conic interior-point backend whose `DefaultSolution.z` conic dual multipliers cvxpy surfaces through the catalogued `Constraint.dual_value`, from which the fence recovers the complementary-slackness KKT gap; its solver-internal `obj_val`/`obj_val_dual` objective pair stays inside Clarabel and is not read, admitted via the `solver=` selector — catalogued in `compute/.api/clarabel.md`), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `maximum`, `linalg.eigvalsh` — the canonical problem-data buffer, the form symmetrization, the elementwise dual-cone-residual max-reduction, and the PSD-cone dual-feasibility residual over the dual matrix's minimum eigenvalue), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, and quadratic form admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `solvers/receipt.md#RECEIPT` (the one `SolveStatus` termination `StrEnum` this receipt's `status` field carries, the cvxpy status string folded into it through the local `_CONVEX_STATUS` boundary table — the convex sibling of the `_STATUS` `RESULTS` map, never a parallel convex-only status vocabulary), `graduation/handoff.md#GRADUATION` (the `convex-program` axis the certified optimum graduates on), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`).
- Growth: a new convex cone family is one `ConvexProgram` case plus one arm in the `_assemble` dispatch; a new DCP atom or cone membership is one cell on the arm; a concave objective is the `Sense.MAX` policy row, never a maximize-owner; a parameter sweep is a `ParamBind` table over `cp.Parameter` leaves warm-re-solving the one compiled `Problem`, never a rebuilt `Problem`; a new cvxpy status constant is one `_CONVEX_STATUS` row; zero new surface, never a per-cone owner, never a parallel linear-program-and-SDP owner, never a per-route `_*_receipt` helper body, never a parallel maximize-owner beside the minimize one, never a hand-rolled interior-point or cone reduction `cvxpy` and Clarabel already own.
- Boundary: classical disciplined-convex programming over `cvxpy` and a conic backend only — the linear, quadratic, second-order-cone, exponential-cone, and semidefinite programs with a dual-certificate proof of global optimality are in-scope; the differentiable inverse-design loop stays on `design.md` and the discrete/global math program on `program.md`, and neither duplicates here. This owner carries **no numpy floor**: the certified convex solve *is* `cvxpy` over Clarabel (the DCP compilation, the cone reduction, the interior-point iteration), so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate — the deliberate floor asymmetry against `design.md` and matching the no-floor posture of `program.md` and the no-floor Qhull routes of `analysis/spatial.md#QUERY`, because an uncertified convex answer is no certificate at all. A non-convex or neural relaxation, a hand-rolled cone slack reformulation, an uncatalogued `psd_wrap` coercion of an indefinite form, a parallel maximize-owner beside the `Sense.MIN` one, a per-sweep rebuilt `Problem` where DPP warm re-solve applies, a mixed-integer branch the conic backend does not own, a production solver session, and a parallel optimizer surface beside the conic solve are the deleted forms; the modeling stays on `cvxpy` and the solve on Clarabel, so this owner composes the certified optimum rather than re-deriving it.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


type ParamBind = tuple[FrozenDict[str, np.ndarray], ...]


class Sense(StrEnum):
    MIN = "minimize"
    MAX = "maximize"


_CONVEX_STATUS: FrozenDict[str, SolveStatus] = FrozenDict(
    {
        "optimal": SolveStatus.SUCCESS,
        "optimal_inaccurate": SolveStatus.STAGNATION,
        "infeasible": SolveStatus.INFEASIBLE,
        "infeasible_inaccurate": SolveStatus.INFEASIBLE,
        "unbounded": SolveStatus.UNBOUNDED,
        "unbounded_inaccurate": SolveStatus.UNBOUNDED,
        "infeasible_or_unbounded": SolveStatus.INFEASIBLE,
        "solver_error": SolveStatus.BREAKDOWN,
        "user_limit": SolveStatus.MAX_STEPS,
    }
)

_NO_BIND: ParamBind = (FrozenDict({}),)


class ConvexReceipt(Struct, frozen=True):
    program: str
    objective: float
    duality_gap: float
    dual_infeasibility: float
    status: SolveStatus
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
    tag: Literal["linear", "quadratic", "second_order", "exponential", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, Sense, ParamBind] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, Sense, ParamBind] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Sense, ParamBind] = case()
    exponential: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Sense, ParamBind] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray, Sense, ParamBind] = case()

    @staticmethod
    def Linear(c: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(linear=(c, a_ub, b_ub, sense, params))

    @staticmethod
    def Quadratic(p: np.ndarray, q: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(quadratic=(p, q, a_ub, b_ub, sense, params))

    @staticmethod
    def SecondOrder(c: np.ndarray, soc_terms: tuple[tuple[np.ndarray, float], ...], a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(second_order=(c, soc_terms, a_ub, b_ub, sense, params))

    @staticmethod
    def Exponential(c: np.ndarray, exp_terms: tuple[tuple[np.ndarray, float], ...], a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(exponential=(c, exp_terms, a_ub, b_ub, sense, params))

    @staticmethod
    def Semidefinite(c_mat: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(semidefinite=(c_mat, a_ub, b_ub, sense, params))

    @property
    def sense(self) -> Sense:
        return getattr(self, self.tag)[-2]

    @property
    def binds(self) -> ParamBind:
        return getattr(self, self.tag)[-1]


def _convex_key(program: ConvexProgram, fields: tuple[np.ndarray, ...], bind: FrozenDict[str, np.ndarray]) -> ContentKey:
    seed = (*fields, *(bind[name] for name in sorted(bind)))
    buffer = b"".join(np.ascontiguousarray(np.asarray(field, dtype=float)).tobytes() for field in seed)
    return ContentIdentity.of(f"convex.{program.tag}", buffer, IdentityPolicy())


def solve(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    return boundary("convex.solve", lambda: _sweep(program))


def _objective(sense: Sense, expr: object, cp: object) -> object:
    return (cp.Minimize if sense is Sense.MIN else cp.Maximize)(expr)


def _sweep(program: ConvexProgram) -> tuple[ConvexReceipt, ...]:
    import cvxpy as cp

    if cp.CLARABEL not in cp.installed_solvers():
        return (_certificate(program, None, [], _convex_key(program, (), FrozenDict({}))),)
    objective, constraints, fields, parameters = _assemble(program, cp)
    problem = cp.Problem(_objective(program.sense, objective, cp), constraints)
    return tuple(
        _solve_bind(program, problem, constraints, parameters, fields, bind, cp)
        for bind in program.binds
    )


def _solve_bind(
    program: ConvexProgram,
    problem: object,
    constraints: list[object],
    parameters: dict[str, object],
    fields: tuple[np.ndarray, ...],
    bind: FrozenDict[str, np.ndarray],
    cp: object,
) -> ConvexReceipt:
    for name, value in bind.items():
        parameters[name].value = np.asarray(value, dtype=float)
    problem.solve(solver=cp.CLARABEL, warm_start=True)
    return _certificate(program, problem, constraints, _convex_key(program, fields, bind))


def _leaf(name: str, value: np.ndarray, binds: ParamBind, cp: object, parameters: dict[str, object]) -> object:
    if not any(name in bind for bind in binds):
        return value
    leaf = cp.Parameter(value.shape, name=name, value=value)
    parameters[name] = leaf
    return leaf


def _assemble(program: ConvexProgram, cp: object) -> tuple[object, list[object], tuple[np.ndarray, ...], dict[str, object]]:
    parameters: dict[str, object] = {}
    binds = program.binds
    match program:
        case ConvexProgram(tag="linear", linear=(c, a_ub, b_ub, _, _)):
            cost, mat = _as_vec(c), _as_mat(a_ub)
            rhs = _leaf("rhs", _as_vec(b_ub), binds, cp, parameters)
            x = cp.Variable(cost.size)
            return cost @ x, [mat @ x <= rhs], (cost, mat, _as_vec(b_ub)), parameters
        case ConvexProgram(tag="quadratic", quadratic=(p, q, a_ub, b_ub, _, _)):
            form, lin, mat = _symm(p), _as_vec(q), _as_mat(a_ub)
            rhs = _leaf("rhs", _as_vec(b_ub), binds, cp, parameters)
            x = cp.Variable(lin.size)
            return 0.5 * cp.quad_form(x, form) + lin @ x, [mat @ x <= rhs], (form, lin, mat, _as_vec(b_ub)), parameters
        case ConvexProgram(tag="second_order", second_order=(c, soc_terms, a_ub, b_ub, _, _)):
            cost, mat = _as_vec(c), _as_mat(a_ub)
            rhs = _leaf("rhs", _as_vec(b_ub), binds, cp, parameters)
            x = cp.Variable(cost.size)
            rows = [mat @ x <= rhs] + [cp.norm2(_as_mat(a) @ x) <= bound for a, bound in soc_terms]
            return cost @ x, rows, (cost, mat, _as_vec(b_ub)), parameters
        case ConvexProgram(tag="exponential", exponential=(c, exp_terms, a_ub, b_ub, _, _)):
            cost, mat = _as_vec(c), _as_mat(a_ub)
            rhs = _leaf("rhs", _as_vec(b_ub), binds, cp, parameters)
            x = cp.Variable(cost.size)
            rows = [mat @ x <= rhs] + [cp.log_sum_exp(_as_mat(a) @ x) <= bound for a, bound in exp_terms]
            return cost @ x, rows, (cost, mat, _as_vec(b_ub)), parameters
        case ConvexProgram(tag="semidefinite", semidefinite=(c_mat, a_ub, b_ub, _, _)):
            cost, mat = _symm(c_mat), _as_mat(a_ub)
            rhs = _leaf("rhs", _as_vec(b_ub), binds, cp, parameters)
            n = cost.shape[0]
            x = cp.Variable((n, n), symmetric=True)
            rows = [x >> 0, mat @ cp.reshape(x, (n * n,), order="C") <= rhs]
            return cp.trace(cost @ x), rows, (cost, mat, _as_vec(b_ub)), parameters
        case unreachable:
            assert_never(unreachable)


def _certificate(program: ConvexProgram, problem: object | None, constraints: list[object], key: ContentKey) -> ConvexReceipt:
    if problem is None or problem.value is None:
        return ConvexReceipt(program.tag, float("inf"), float("inf"), float("inf"), SolveStatus.OTHER, key)
    gap, infeasibility = _kkt(constraints)
    status = _CONVEX_STATUS.get(str(problem.status), SolveStatus.OTHER)
    return ConvexReceipt(program.tag, float(problem.value), gap, infeasibility, status, key)


def _kkt(constraints: list[object]) -> tuple[float, float]:
    duals = [np.asarray(con.dual_value, dtype=float) for con in constraints if con.dual_value is not None]
    gap = sum(
        float(np.abs(np.asarray(con.dual_value, dtype=float) * np.asarray(con.expr.value, dtype=float)).sum())
        for con in constraints
        if con.dual_value is not None and con.expr.value is not None
    )
    infeasibility = max((_dual_residual(d) for d in duals), default=0.0)
    return float(gap), float(infeasibility)


def _dual_residual(dual: np.ndarray) -> float:
    if dual.ndim == 2 and dual.shape[0] == dual.shape[1]:
        return float(np.maximum(-np.linalg.eigvalsh(0.5 * (dual + dual.T)).min(initial=0.0), 0.0))
    return float(np.maximum(-dual, 0.0).max(initial=0.0))


def _symm(array: np.ndarray) -> np.ndarray:
    form = np.atleast_2d(np.asarray(array, dtype=float))
    return np.ascontiguousarray(0.5 * (form + form.T))


def _as_vec(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.asarray(array, dtype=float).ravel())


def _as_mat(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.atleast_2d(np.asarray(array, dtype=float)))
```

## [03]-[RESEARCH]

- [CVXPY_SOLVE]: `cvxpy` resolves on the companion `python_version<'3.15'` band (cp311-cp314 wheels only; no CPython 3.15 wheel); the `Variable`/`Parameter`/`Minimize`/`Maximize`/`Problem` modeling algebra, the `==`/`<=`/`>=`/`>>` relational constraint operators, the `quad_form`/`norm2`/`log_sum_exp`/`rel_entr`/`trace`/`reshape`/`sum` DCP atoms, `Problem.solve(solver=cp.CLARABEL, warm_start=True)`, `installed_solvers()`, and `Problem.value`/`Problem.status`/`Variable.value`/`Constraint.dual_value` verify against `compute/.api/cvxpy.md` under a uv-sync reflection pass on that band — the catalogue is documentation-authored RESEARCH-capture-pending-on-uv-sync, not yet reflection-verified against an installed wheel, so the spellings settle on uv sync into the companion interpreter band. The quadratic and semidefinite forms are symmetrized at the numpy edge (`0.5·(P+Pᵀ)`) before `cp.quad_form`/`cp.trace`, so the catalogued atom receives a DCP-legal symmetric form and a genuinely indefinite cost is the DCP construction rejection — `psd_wrap` is not in the catalogued atom tables and is never used. DCP curvature tracking raises at construction on a non-convex model, so the certificate is never produced for a problem the modeling layer rejects; this owner carries no numpy floor because `cvxpy` over the conic backend *is* the certified capability, so a cp315 run without the wheel returns `Error(Import)` — the deliberate floor asymmetry against `optimization/design.md#DESIGN`.
- [CONVEX_SENSE_AND_SWEEP]: the objective sign is the `Sense` `StrEnum` policy row folded into the catalogued `cp.Minimize`/`cp.Maximize` constructor through the `{Sense.MIN: cp.Minimize, Sense.MAX: cp.Maximize}` table, so a concave-maximize program is a row on the one owner, never a parallel maximize-owner; DCP curvature still adjudicates convexity, so a `Sense.MAX` over a convex objective is the construction rejection. The parameter axis declares a catalogued `cp.Parameter` leaf for each named problem-data slot referenced by the `ParamBind` rows (the inequality `rhs` is the canonical parametrizable surface), compiles the DPP `Problem` once, and warm-re-`solve`s across the bind table with `warm_start=True` reusing the prior factorization, folding one `ConvexReceipt` per bind keyed on the bound values through `ContentIdentity.of`; the no-bind default `_NO_BIND` declares no parameter and solves once. The exponential-cone case carries the `cp.log_sum_exp`/`cp.rel_entr` atoms (the catalogued exponential-cone-representable family) Clarabel solves through its `ExponentialConeT`, the fifth DCP cone family beside linear/quadratic/SOC/SDP. The entry preflights `cp.CLARABEL in cp.installed_solvers()` and degrades a missing backend to a `SolveStatus.OTHER` receipt rather than raising mid-fold.
- [CONVEX_STATUS_FOLD]: the `Problem.status` string `compute/.api/cvxpy.md` catalogues (`optimal`/`infeasible`/`unbounded`/inaccurate) is the cvxpy status-constant family — `optimal`, `optimal_inaccurate`, `infeasible`, `infeasible_inaccurate`, `unbounded`, `unbounded_inaccurate`, `infeasible_or_unbounded`, `solver_error`, `user_limit` — folded into the shared `SolveStatus` `StrEnum` `solvers/receipt.md#RECEIPT` owns through the local `_CONVEX_STATUS` boundary table, the convex sibling of the `_STATUS` `RESULTS` map: `optimal`->`SUCCESS`, `optimal_inaccurate`->`STAGNATION`, `infeasible`/`infeasible_inaccurate`/`infeasible_or_unbounded`->`INFEASIBLE`, `unbounded`/`unbounded_inaccurate`->`UNBOUNDED`, `solver_error`->`BREAKDOWN`, `user_limit`->`MAX_STEPS`, every unmapped constant degrading to `OTHER` rather than crashing. The `INFEASIBLE`/`UNBOUNDED` members are the feasibility-verdict termination classes this convex consumer adds to the shared vocabulary (the genuinely-new-termination-class growth path `solvers/receipt.md#RECEIPT` records), so `ConvexReceipt.status` reads the one `SolveStatus` enum the C# graduation gate and the sibling solve receipts read, never a parallel convex-only status string. The certified handoff admits only on `SolveStatus.SUCCESS` with a vanishing duality gap; `STAGNATION` (an inaccurate optimum), `INFEASIBLE`, `UNBOUNDED`, `BREAKDOWN`, and `MAX_STEPS` are admission rejections the `convex-program` `HandoffAxis` reads, never graduated handoffs.
- [CLARABEL_DUAL]: `clarabel` is the default conic interior-point backend `cvxpy` selects through `solver=cp.CLARABEL`; the primal-dual certificate — `DefaultSolution.z` conic dual multipliers, `DefaultSolution.s` primal slack, and the `obj_val`/`obj_val_dual` primal/dual objective whose difference is the backend duality gap — is surfaced to the modeling layer through the catalogued `Constraint.dual_value` multipliers and the primal `Variable.value`, so the fence reads the certificate from those catalogued cvxpy attributes (the complementary-slackness sum `Σ|⟨λᵢ, gᵢ(x)⟩|` over the recovered duals and `Constraint.expr.value` expression values is the KKT optimality gap, vanishing exactly at the global optimum, and the companion dual-feasibility residual is the cone-aware `_dual_residual` — elementwise `max(−λ, 0)` over a vector/scalar inequality dual, `max(−λ_min(Z), 0)` over the symmetrized PSD matrix dual through `np.linalg.eigvalsh`, never a single elementwise sign test misread across the matrix cone), never the solver-specific `solver_stats.extra_stats` raw dict; the spellings verify against `compute/.api/clarabel.md` once the Rust-native cp39-abi3 wheel syncs into the active venv on the companion band. The effective convex gate is `python_version<'3.15'` on **both** packages: the abi3-capable Clarabel wheel transitively pulls `scipy` (gast/pythran lack 3.15 support) and `cvxpy` ships cp311-cp314 wheels only, so the path is companion-band on the pair and never authored as cp315-runnable. The vanishing duality gap with feasible duals is the global-optimality proof object the `convex-program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads to admit or reject the handoff — the convex analogue of the `Enclosure.certified` flag in `numerics/interval.md#ENCLOSURE`.
