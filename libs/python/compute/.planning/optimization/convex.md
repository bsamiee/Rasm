# [PY_COMPUTE_CONVEX]

The dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval.md#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in, compiled to standard conic form and solved through the Clarabel interior-point backend that returns the primal optimum and the per-constraint dual multipliers; the full KKT triple is the proof object, so `certified` gates the complementary-slackness gap AND both feasibility residuals within `_TOL`, never the gap alone. Like `optimization/program.md#PROGRAM`, the convex solve IS `cvxpy` over the conic backend, so a run without the package returns `Error(Import)` rather than an uncertified estimate.

`ConvexReceipt` stays a distinct typed receipt and never folds into the `OutcomeReceipt` the `design`/`program` siblings share — the KKT certificate is the proof object the first-order convergence and feasibility verdicts carry no field for. Its coherence with `solvers/receipt.md#RECEIPT` is the status vocabulary alone: the cvxpy status constants fold into the one `SolveStatus` enum through the `_CONVEX_STATUS` boundary table. The certified optimum graduates on the dedicated `convex_program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`, a distinct admission from the `solver` axis the design/program verdicts cross on.

## [01]-[INDEX]

- [01]-[CONVEX]: five cone families on one `ConvexProgram` owner over the `_CONE_ROWS`/`_CONE_KKT` tables, folding one content-keyed `ConvexReceipt` KKT certificate per `ParamBind` row.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on one sub-domain, never a duplicated optimizer surface; every case ends with one uniform `Policy` slot bound through the `policy` total `match self` or-pattern, never a `getattr(self, self.tag)[-1]` reflection whose `object` residual escapes the exhaustive match; factories are `@classmethod`-plus-`Self`, never a `@staticmethod` over a forward-ref. The five cone families differ by two `ConeRow` closures and one `psd` flag, four `ConeKKT` closures keyed on the constraint's cone family, and the one `Fields` typed projection every case lands in — table and closure rows, never parallel `match` bodies, so `_assemble` and the evidence fold read fixed attributes and reduce with no shape-probe `if`.
- Cases: `Problem.is_dcp()` adjudicates curvature BEFORE the solve, and a genuinely indefinite quadratic form fails it — never a silent `cp.psd_wrap` coercion that forces a PSD lift; the semidefinite case carries PSD membership as an explicit `X >> 0` cone row because a `PSD=True` leaf attribute hides the matrix dual `Z` behind the variable domain where `Constraint.dual_value` cannot reach it; the one `cp.Parameter` leaf sits on the inequality `rhs` — the sole DPP-legal parametrizable buffer, a `Parameter` in the form matrix or constraint matrix breaks the DPP ruleset — so a sweep warm-re-solves the one compiled `Problem`, never a rebuild.
- Entry: a missing backend or a DCP-rejected model folds one uncertified receipt per `ParamBind` row, so the tuple cardinality always matches the bind table; the certificate folds exactly the catalogued cvxpy quantities — `Constraint.dual_value` and the per-cone primal read off `Constraint.args` — never a backend-internal residual the `solve` path does not surface.
- Packages: `clarabel` is admitted only through the `solver=cp.CLARABEL` selector, never a direct `DefaultSolver`/`get_problem_data` assembly this owner re-derives; `gc=False` rides only the scalar-leaf carriers (`ConvexEvidence`, `ConvexReceipt`) while the container/closure carriers (`Policy`, `Fields`, `ConeRow`, `ConeKKT`) stay GC-tracked; problem data admits as `numerics/array.md#PAYLOAD` payloads keying through the same `ContentIdentity` seed.
- Growth: a new cone family is one `ConvexProgram` case plus one `_CONE_ROWS` row, one `_CONE_KKT` row, and one `_cone` arm; a new solve-policy axis is one `Policy` field rather than a sixth positional slot threaded through five factories; a new diagnostic is one `ConvexEvidence` slot reaching the facts map with no second edit; a new cvxpy status constant is one `_CONVEX_STATUS` row.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from operator import attrgetter
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import astuple

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt

# --- [TYPES] -------------------------------------------------------------------------------

type ParamBind = tuple[Map[str, np.ndarray], ...]
type ConeObjective = Callable[[object, "Fields", object], object]  # `(Variable, Fields, cp) -> Expression` cost
type ConeRows = Callable[[object, "Fields", object], tuple[object, ...]]  # `(Variable, Fields, cp)` -> extra cone-constraint rows
type ConeExpr = Callable[[object], np.ndarray | None]  # `(Constraint) -> stacked primal value` off `args`, None if unsolved
type ConeSlack = Callable[[np.ndarray, np.ndarray], float]  # `(dual, expr) -> |⟨λ, g⟩|` complementary slackness
type ConeResidual = Callable[[np.ndarray], float]  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
type ConePrimal = Callable[[np.ndarray], float]  # `(expr) -> dist(g, K)` primal-cone-membership violation


class Sense(StrEnum):
    MIN = "minimize"
    MAX = "maximize"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL = 1e-8
_NO_BIND: ParamBind = (Map.empty(),)

_CONVEX_STATUS: Map[str, SolveStatus] = Map.of_seq([
    ("optimal", SolveStatus.SUCCESS),
    ("optimal_inaccurate", SolveStatus.STAGNATION),
    ("infeasible", SolveStatus.INFEASIBLE),
    ("infeasible_inaccurate", SolveStatus.INFEASIBLE),
    ("unbounded", SolveStatus.UNBOUNDED),
    ("unbounded_inaccurate", SolveStatus.UNBOUNDED),
    ("infeasible_or_unbounded", SolveStatus.INFEASIBLE),
    ("solver_error", SolveStatus.BREAKDOWN),
    ("user_limit", SolveStatus.MAX_STEPS),
])


# --- [MODELS] ------------------------------------------------------------------------------


class Policy(Struct, frozen=True):  # GC-tracked: `binds` is a container (tuple of Map of ndarray)
    sense: Sense = Sense.MIN
    binds: ParamBind = _NO_BIND


class ConvexEvidence(Struct, frozen=True, gc=False):
    duality_gap: float
    primal_infeasibility: float
    dual_infeasibility: float

    def facts(self) -> dict[str, object]:
        # zip the declared `Struct` fields against their values, so a new diagnostic slot reaches the
        # facts map by its declaration alone — never a second hand-spelled key drifting from the field.
        return dict(zip(self.__struct_fields__, astuple(self), strict=True))

    @classmethod
    def uncertified(cls) -> Self:
        inf = float("inf")
        return cls(inf, inf, inf)


class ConvexReceipt(Struct, frozen=True, gc=False):
    program: str
    objective: float
    status: SolveStatus
    evidence: ConvexEvidence
    content_key: ContentKey

    @property
    def certified(self) -> bool:
        return self.status is SolveStatus.SUCCESS and max(astuple(self.evidence)) <= _TOL

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "program": self.program,
            "objective": self.objective,
            "status": self.status,
            "certified": self.certified,
            "key": self.content_key.hex,
            **self.evidence.facts(),
        }
        return (Receipt.of("compute.convex", ("emitted", self.program, facts)),)


@tagged_union(frozen=True)
class ConvexProgram:
    tag: Literal["linear", "quadratic", "second_order", "exponential", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    exponential: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()

    @classmethod
    def Linear(cls, c: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> Self:
        return cls(linear=(c, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Quadratic(
        cls, p: np.ndarray, q: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND
    ) -> Self:
        return cls(quadratic=(p, q, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def SecondOrder(
        cls,
        c: np.ndarray,
        soc_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
    ) -> Self:
        return cls(second_order=(c, soc_terms, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Exponential(
        cls,
        c: np.ndarray,
        exp_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
    ) -> Self:
        return cls(exponential=(c, exp_terms, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Semidefinite(cls, c_mat: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> Self:
        return cls(semidefinite=(c_mat, a_ub, b_ub, Policy(sense, params)))

    @property
    def policy(self) -> Policy:
        match self:
            case (
                ConvexProgram(tag="linear", linear=(*_, Policy() as policy))
                | ConvexProgram(tag="quadratic", quadratic=(*_, Policy() as policy))
                | ConvexProgram(tag="second_order", second_order=(*_, Policy() as policy))
                | ConvexProgram(tag="exponential", exponential=(*_, Policy() as policy))
                | ConvexProgram(tag="semidefinite", semidefinite=(*_, Policy() as policy))
            ):
                return policy
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] --------------------------------------------------------------------------


# the convex family's default graduation ceiling — all three KKT conditions within tolerance; caller-overridable at the hub.
_CEILING: Final[Map[str, float]] = Map.of_seq([("duality_gap", 1e-8), ("primal_infeasibility", 1e-8), ("dual_infeasibility", 1e-8)])

# the family modality row: policy DATA, never a per-page literal, never a compute-minted limiter.
_MODALITY: Final[Modality] = Modality.THREAD


async def solve(program: ConvexProgram, lane: LanePolicy) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    # `.bind`-flatten joins the solve fence and the railed digest onto one rail; the weave owns span, fence, and receipt harvest.
    async def dispatch() -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
        return (await lane.offload(_sweep, program, modality=_MODALITY)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.CONVEX, f"convex.{program.tag}", dispatch)


def graduates(receipt: ConvexReceipt) -> "RuntimeRail[GraduationReceipt]":
    # the KKT triple is the ledger the hub clears against the family ceiling — a gap above tolerance is an admission rejection.
    ledger = {
        "duality_gap": receipt.evidence.duality_gap,
        "primal_infeasibility": receipt.evidence.primal_infeasibility,
        "dual_infeasibility": receipt.evidence.dual_infeasibility,
    }
    return GraduationReceipt.graduates("compute.convex", HandoffAxis(convex_program=receipt.program), receipt.content_key, ledger, dict(_CEILING.items()))


# --- [COMPOSITION] -------------------------------------------------------------------------


class ConeRow(Struct, frozen=True):  # GC-tracked: carries the two cone closures
    objective: ConeObjective
    extra: ConeRows
    psd: bool = False


def _affine_cost(x: object, fields: "Fields", cp: object) -> object:
    return fields.cost @ x


def _quadratic_cost(x: object, fields: "Fields", cp: object) -> object:
    return 0.5 * cp.quad_form(x, fields.cost) + fields.lin @ x


def _trace_cost(x: object, fields: "Fields", cp: object) -> object:
    return cp.trace(fields.cost @ x)


def _no_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return ()


def _soc_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return tuple(cp.SOC(cp.Constant(bound), _as_mat(a) @ x) for a, bound in fields.terms)


def _exp_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return tuple(cp.log_sum_exp(_as_mat(a) @ x) <= bound for a, bound in fields.terms)


_CONE_ROWS: Map[str, ConeRow] = Map.of_seq([
    ("linear", ConeRow(_affine_cost, _no_rows)),
    ("quadratic", ConeRow(_quadratic_cost, _no_rows)),
    ("second_order", ConeRow(_affine_cost, _soc_rows)),
    ("exponential", ConeRow(_affine_cost, _exp_rows)),
    ("semidefinite", ConeRow(_trace_cost, _no_rows, psd=True)),
])


class Fields(Struct, frozen=True):  # GC-tracked: `terms` is a container (tuple of (ndarray, float) pairs)
    cost: np.ndarray  # cost vector, or the symmetrized form/`c_mat` matrix for `quadratic`/`semidefinite`
    mat: np.ndarray
    rhs: np.ndarray
    lin: np.ndarray | None = None  # the `quadratic` linear term `q`; absent on every other cone
    terms: tuple[tuple[np.ndarray, float], ...] = ()  # SOC/exponential cone terms; empty elsewhere


def _fields(program: ConvexProgram) -> Fields:
    match program:
        case ConvexProgram(tag="linear", linear=(c, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub))
        case ConvexProgram(tag="quadratic", quadratic=(p, q, a_ub, b_ub, _)):
            return Fields(_symm(p), _as_mat(a_ub), _as_vec(b_ub), lin=_as_vec(q))
        case ConvexProgram(tag="second_order", second_order=(c, terms, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub), terms=terms)
        case ConvexProgram(tag="exponential", exponential=(c, terms, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub), terms=terms)
        case ConvexProgram(tag="semidefinite", semidefinite=(c_mat, a_ub, b_ub, _)):
            return Fields(_symm(c_mat), _as_mat(a_ub), _as_vec(b_ub))
        case _ as unreachable:
            assert_never(unreachable)


def _sweep(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    import cvxpy as cp

    if cp.CLARABEL not in cp.installed_solvers():
        return _uncertified_sweep(program, None)
    objective, constraints, fields, parameters = _assemble(program, cp)
    problem = cp.Problem(_SENSE[program.policy.sense](cp)(objective), constraints)
    if not problem.is_dcp():
        return _uncertified_sweep(program, fields)
    rails = (_solve_bind(program, problem, constraints, parameters, fields, bind, cp) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _uncertified_sweep(program: ConvexProgram, fields: "Fields | None") -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    # one uncertified receipt per bind row, so the failure paths preserve the sweep cardinality the entry contract promises.
    rails = (_convex_key(program, fields, bind).map(lambda key: _uncertified(program, key)) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _assemble(program: ConvexProgram, cp: object) -> tuple[object, list[object], "Fields", dict[str, object]]:
    # the decision dimension is the COST extent, NOT the constraint-matrix column count — an unconstrained program carries an empty
    # `mat`, so sizing `x` off `mat.shape[1]` mis-sizes the variable; the polyhedral row is added only when `rhs.size` is non-empty.
    parameters: dict[str, object] = {}
    row, fields = _CONE_ROWS[program.tag], _fields(program)
    rhs = _leaf("rhs", fields.rhs, program.policy.binds, cp, parameters)
    n = int(fields.cost.shape[0])
    if row.psd:
        x = cp.Variable((n, n), symmetric=True)
        cone = [x >> 0]
        polyhedral = [fields.mat @ cp.vec(x) <= rhs] if fields.rhs.size else []
    else:
        x = cp.Variable(n)
        cone = []
        polyhedral = [fields.mat @ x <= rhs] if fields.rhs.size else []
    return row.objective(x, fields, cp), [*polyhedral, *cone, *row.extra(x, fields, cp)], fields, parameters


def _solve_bind(
    program: ConvexProgram,
    problem: object,
    constraints: list[object],
    parameters: dict[str, object],
    fields: "Fields",
    bind: Map[str, np.ndarray],
    cp: object,
) -> "RuntimeRail[ConvexReceipt]":
    # bind values pull by REGISTERED leaf name, never by iterating the bind's own keys: a foreign key is structurally unreachable
    # rather than a `parameters[name]` KeyError, and a row omitting a registered leaf reuses its prior `value`.
    for name, leaf in parameters.items():
        leaf.value = np.asarray(bind.try_find(name).default_value(leaf.value), dtype=float)
    problem.solve(solver=cp.CLARABEL, warm_start=True)
    return _convex_key(program, fields, bind).map(lambda key: _certificate(program, problem, constraints, key, cp))


def _leaf(name: str, value: np.ndarray, binds: ParamBind, cp: object, parameters: dict[str, object]) -> object:
    # register a `cp.Parameter` only when a bind references the key AND the buffer is non-empty — an empty `rhs` (polyhedral row
    # skipped) never registers an inert parameter against a constraint the assembly did not build.
    if not value.size or not any(name in bind for bind in binds):
        return value
    leaf = cp.Parameter(value.shape, name=name, value=value)
    parameters[name] = leaf
    return leaf


_SENSE: Map[Sense, Callable[[object], object]] = Map.of_seq([(Sense.MIN, attrgetter("Minimize")), (Sense.MAX, attrgetter("Maximize"))])


def _certificate(program: ConvexProgram, problem: object, constraints: list[object], key: ContentKey, cp: object) -> ConvexReceipt:
    if problem.value is None:
        return _uncertified(program, key)
    status = _CONVEX_STATUS.try_find(str(problem.status)).default_value(SolveStatus.OTHER)
    return ConvexReceipt(program.tag, float(problem.value), status, _evidence(constraints, cp), key)


def _uncertified(program: ConvexProgram, key: ContentKey) -> ConvexReceipt:
    return ConvexReceipt(program.tag, float("inf"), SolveStatus.OTHER, ConvexEvidence.uncertified(), key)


def _evidence(constraints: list[object], cp: object) -> ConvexEvidence:
    # the cone identity is the constraint TYPE, never the dual's matrix-vs-vector shape; the primal rides the cell's `expr` extractor
    # off `Constraint.args` because `SOC(t, X)`/`ExpCone` carry NO single `.expr` — a uniform `c.expr.value` read raises
    # `AttributeError` on exactly the certificate-bearing SOC/SDP rows.
    cells = ((_CONE_KKT[_cone(c, cp)], c) for c in constraints)
    rows = [(kkt, np.asarray(c.dual_value, dtype=float), kkt.expr(c)) for kkt, c in cells if c.dual_value is not None]
    solved = [(kkt, dual, expr) for kkt, dual, expr in rows if expr is not None]
    return ConvexEvidence(
        duality_gap=float(sum(kkt.slack(dual, expr) for kkt, dual, expr in solved)),
        primal_infeasibility=max((kkt.primal(expr) for kkt, _, expr in solved), default=0.0),
        dual_infeasibility=max((kkt.residual(dual) for kkt, dual, _ in solved), default=0.0),
    )


def _cone(constraint: object, cp: object) -> str:
    # every polyhedral inequality and the `log_sum_exp(...) <= b` exponential row canonicalize to a nonnegative-orthant scalar dual.
    match constraint:
        case cp.PSD():
            return "psd"
        case cp.SOC():
            return "soc"
        case _:
            return "nonneg"


def _slack_separable(dual: np.ndarray, expr: np.ndarray) -> float:
    return float(np.abs(dual * expr).sum())  # orthant `Σ|λᵢ·gᵢ|`: componentwise slackness, genuinely separable


def _slack_inner(dual: np.ndarray, expr: np.ndarray) -> float:
    # SOC/PSD slackness is the SINGLE inner product `|⟨λ, g⟩|` over the stacked/flattened pair (the SOC
    # `|z·s|`, the PSD `|tr(Z·X)| = |Σ Zᵢⱼ·Xᵢⱼ|`), never the orthant's `Σ|λᵢ·gᵢ|` — on a non-separable
    # cone the signed cross terms cancel at the optimum while `Σ|·|` over-counts them as a false gap.
    return float(np.abs(np.sum(dual.ravel() * expr.ravel())))


def _residual_nonneg(dual: np.ndarray) -> float:
    return float(np.maximum(-dual, 0.0).max(initial=0.0))  # self-dual orthant: `max(−λ, 0)`


def _residual_soc(dual: np.ndarray) -> float:
    z = dual.ravel()  # self-dual second-order cone: `max(‖z₁:‖₂ − z₀, 0)`, never an elementwise sign test
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _residual_psd(dual: np.ndarray) -> float:
    # self-dual PSD cone: `max(−λ_min(½(Z+Zᵀ)), 0)` over the symmetrized matrix dual's spectrum.
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (dual + dual.T)).min(initial=0.0), 0.0))


def _primal_nonneg(expr: np.ndarray) -> float:
    # cvxpy `Inequality` `g(x) <= 0` canonical form: the primal violation is `max(g, 0)`.
    return float(np.maximum(expr, 0.0).max(initial=0.0))


def _primal_soc(expr: np.ndarray) -> float:
    z = expr.ravel()  # second-order cone `‖z₁:‖₂ <= z₀`: violation `max(‖z₁:‖₂ − z₀, 0)`
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _primal_psd(expr: np.ndarray) -> float:
    # PSD cone `X >> 0`: violation `max(−λ_min(½(X+Xᵀ)), 0)` over the symmetrized matrix's spectrum.
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (expr + expr.T)).min(initial=0.0), 0.0))


def _expr_nonneg(constraint: object) -> np.ndarray | None:
    # the relational `Inequality` carries `args = [lhs, rhs]` and `g = lhs − rhs` (cvxpy `Inequality.expr`,
    # `<= 0` at feasibility); read it off the universal `args` rather than the `.expr` only relationals own.
    lhs, rhs = constraint.args[0].value, constraint.args[1].value
    return None if lhs is None or rhs is None else np.asarray(lhs, dtype=float) - np.asarray(rhs, dtype=float)


def _expr_soc(constraint: object) -> np.ndarray | None:
    # `SOC(t, X)` carries NO `.expr`; its `args = [t, X]` stack into `[t, *X]` matching the dual layout
    # cvxpy reshapes to `[t_dual, X_dual...]`, so the slackness pairs and the `‖X‖₂ <= t` test align.
    t, x = constraint.args[0].value, constraint.args[1].value
    return None if t is None or x is None else np.append(np.ravel(np.asarray(t, dtype=float)), np.ravel(np.asarray(x, dtype=float)))


def _expr_psd(constraint: object) -> np.ndarray | None:
    # `X >> 0` carries `args = [X]`; the primal value is the optimal matrix the `tr(Z·X)` slackness and the
    # `λ_min(X)` membership read, the same `(n, n)` shape as the symmetric matrix dual `Constraint.dual_value`.
    matrix = constraint.args[0].value
    return None if matrix is None else np.asarray(matrix, dtype=float)


class ConeKKT(Struct, frozen=True):  # GC-tracked: carries the four cone-membership closures
    expr: ConeExpr  # `(Constraint) -> stacked primal value` off `args`, since SOC/PSD carry no `.expr`
    slack: ConeSlack  # `(dual, expr) -> |⟨λ, g⟩|` complementary-slackness contribution
    residual: ConeResidual  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
    primal: ConePrimal  # `(expr) -> dist(g, K)` primal-cone-membership violation


_CONE_KKT: Map[str, ConeKKT] = Map.of_seq([
    ("nonneg", ConeKKT(_expr_nonneg, _slack_separable, _residual_nonneg, _primal_nonneg)),
    ("soc", ConeKKT(_expr_soc, _slack_inner, _residual_soc, _primal_soc)),
    ("psd", ConeKKT(_expr_psd, _slack_inner, _residual_psd, _primal_psd)),
])


def _seed_arrays(fields: "Fields | None") -> tuple[np.ndarray, ...]:
    if fields is None:  # missing-backend key: program tag plus binds, no problem-data block
        return ()
    core = (fields.cost, *((fields.lin,) if fields.lin is not None else ()), fields.mat, fields.rhs)
    term_blocks = tuple(np.append(_as_mat(a).ravel(), bound) for a, bound in fields.terms)
    return (*core, *term_blocks)


def _convex_key(program: ConvexProgram, fields: "Fields | None", bind: Map[str, np.ndarray]) -> "RuntimeRail[ContentKey]":
    # variable-length blocks joined with no delimiter collide two same-tag programs whose concatenations are byte-identical but whose
    # block boundaries differ; folding each block's ordinal and shape into the fmt (plus the sorted bind-key names) distinguishes
    # shifted boundaries, differing `terms` arity, and foreign-keyed binds — the same discriminant `program.md`'s `_program_key` folds.
    seed_blocks = _seed_arrays(fields)
    bind_blocks = tuple(np.asarray(bind[name], dtype=float) for name in sorted(bind))
    blocks = (*seed_blocks, *bind_blocks)
    buffer = b"".join(np.ascontiguousarray(field).tobytes() for field in blocks)
    shape_tag = "".join(f".{i}:{f.ndim}x{'x'.join(map(str, f.shape))}" for i, f in enumerate(blocks))
    bind_tag = f".binds:{'.'.join(sorted(bind))}" if bind else ""
    return ContentIdentity.of(f"convex.{program.tag}{shape_tag}{bind_tag}", buffer)


def _symm(array: np.ndarray) -> np.ndarray:
    form = np.atleast_2d(np.asarray(array, dtype=float))
    return np.ascontiguousarray(0.5 * (form + form.T))


def _as_vec(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.asarray(array, dtype=float).ravel())


def _as_mat(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.atleast_2d(np.asarray(array, dtype=float)))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
