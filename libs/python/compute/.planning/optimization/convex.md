# [PY_COMPUTE_CONVEX]

Dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in, compiled to standard conic form and solved through the selected `Backend` row — Clarabel interior-point the default, SCS the first-order operator-splitting arm, HiGHS the LP/QP simplex arm — each returning the primal optimum and the per-constraint dual multipliers cvxpy normalizes, so backend choice never weakens the proof; the full KKT triple is the proof object, so `certified` gates the complementary-slackness gap AND both feasibility residuals within `_TOL`, never the gap alone. Like `optimization/program#PROGRAM`, the convex solve IS `cvxpy` over the conic backend, so a run without the package returns `Error(Import)` rather than an uncertified estimate.

`ConvexReceipt` stays a distinct typed receipt and never folds into the `OutcomeReceipt` the `design`/`program` siblings share — the KKT certificate is the proof object the first-order convergence and feasibility verdicts carry no field for. Its coherence with `solvers/receipt#RECEIPT` is the status vocabulary alone: the cvxpy status constants fold into the one `SolveStatus` enum through the `_CONVEX_STATUS` boundary table. The receipt settles on the ONE runtime spine, its `certified` verdict riding the warning band that names WHICH bar failed rather than a bool that erases it. Certified optimum graduates on the dedicated `convex_program` `HandoffAxis` case at `graduation/handoff#GRADUATION`, a distinct admission from the `solver` axis the design/program verdicts cross on.

## [01]-[INDEX]

- [02]-[CONVEX]: six cone families and three solve backends on one `ConvexProgram` owner over the `_CONE_ROWS`/`_CONE_KKT`/`_BACKEND` tables, folding one content-keyed `ConvexReceipt` KKT certificate per `ParamBind` row.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on one sub-domain, never a duplicated optimizer surface; every case ends with one uniform `Policy` slot bound through the `policy` total `match self` or-pattern, never a `getattr(self, self.tag)[-1]` reflection whose `object` residual escapes the exhaustive match; factories are `@classmethod`-plus-`Self`, never a `@staticmethod` over a forward-ref. Six cone families differ by two `ConeRow` closures and one `psd` flag, four `ConeKKT` closures keyed on the constraint's cone family — the residual and primal closures take the constraint beside the dual/expr because cone PARAMETERS (`PowCone3D.alpha`) live on the constraint object, never in the stacked value — and the one `Fields` typed projection every case lands in — table and closure rows, never parallel `match` bodies, so `_assemble` and the evidence fold read fixed attributes and reduce with no shape-probe `if`.
- Cases: `Problem.is_dcp()` adjudicates curvature BEFORE the solve, and a genuinely indefinite quadratic form fails it — never a silent `cp.psd_wrap` coercion that forces a PSD lift; the semidefinite case carries PSD membership as an explicit `X >> 0` cone row because a `PSD=True` leaf attribute hides the matrix dual `Z` behind the variable domain where `Constraint.dual_value` cannot reach it; the one `cp.Parameter` leaf sits on the inequality `rhs` — the sole DPP-legal parametrizable buffer, a `Parameter` in the form matrix or constraint matrix breaks the DPP ruleset — so a sweep warm-re-solves the one compiled `Problem`, never a rebuild. `power` rows `PowCone3D(aₓ@x, a_y@x, a_z@x, α)` membership per term — its dual is the `[u, v, w]` triple mirroring `args = [x, y, z]` (`np.asarray` stacks it `(3, n)`), inner-product slackness cancels per triple, and dual-cone membership scales `u/α`, `v/(1−α)` before the same weighted-geometric-mean gap the primal reads unscaled. `_BACKEND` rows declare each backend's covered cone families beside its cvxpy selector — HiGHS covers `linear`/`quadratic` alone and cvxpy refuses its conic hand-off with a `SolverError` at selection — so an uncovered `(backend, cone)` pair folds the uncertified sweep at admission, cardinality preserved, never a raise mid-sweep. Every solve pins `canon_backend=cp.SCIPY_CANON_BACKEND`: the floor's source-built CPP canon extension trips a fatal `ProblemData.hpp` assert on every canonicalization — a process abort no rail catches — and the SciPy path canonicalizes every family clean.
- Entry: a missing backend or a DCP-rejected model folds one uncertified receipt per `ParamBind` row, so the tuple cardinality always matches the bind table; the certificate folds exactly the catalogued cvxpy quantities — `Constraint.dual_value` and the per-cone primal read off `Constraint.args` — never a backend-internal residual the `solve` path does not surface.
- Packages: `clarabel`, `scs`, and `highspy` are admitted only through their `solver=` selectors off the `_BACKEND` row, never a direct `DefaultSolver`/`get_problem_data` assembly this owner re-derives; `gc=False` rides only the scalar-leaf carriers (`ConvexEvidence`, `ConvexReceipt`) while the container/closure carriers (`Policy`, `Fields`, `ConeRow`, `ConeKKT`) stay GC-tracked; problem data admits as `numerics/array#PAYLOAD` payloads keying through the same `ContentIdentity` seed.
- Growth: a new cone family is one `ConvexProgram` case with one `_CONE_ROWS` row, one `_CONE_KKT` row, and one `_cone` arm; a new solve backend is one `Backend` member and one `_BACKEND` row naming its selector and covered cone families; a new solve-policy axis is one `Policy` field rather than a positional slot threaded through six factories; a new diagnostic is one `ConvexEvidence` slot reaching the facts map with no second edit; a new cvxpy status constant is one `_CONVEX_STATUS` row.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from operator import attrgetter
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import astuple

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import cvxpy as cp

# --- [TYPES] ----------------------------------------------------------------------------

type ParamBind = tuple[Map[str, np.ndarray], ...]
type ConeObjective = Callable[[object, "Fields", object], object]
type ConeRows = Callable[[object, "Fields", object], tuple[object, ...]]
type ConeExpr = Callable[[object], np.ndarray | None]
type ConeSlack = Callable[[np.ndarray, np.ndarray], float]
type ConeResidual = Callable[[np.ndarray, object], float]
type ConePrimal = Callable[[np.ndarray, object], float]
type PowTerm = tuple[np.ndarray, np.ndarray, np.ndarray, float]


class Sense(StrEnum):
    MIN = "minimize"
    MAX = "maximize"


class Backend(StrEnum):
    CLARABEL = "clarabel"
    SCS = "scs"
    HIGHS = "highs"


# --- [CONSTANTS] ------------------------------------------------------------------------

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


# --- [MODELS] ---------------------------------------------------------------------------


class Policy(Struct, frozen=True):
    sense: Sense = Sense.MIN
    binds: ParamBind = _NO_BIND
    backend: Backend = Backend.CLARABEL


class ConvexEvidence(Struct, frozen=True, gc=False):
    duality_gap: float
    primal_infeasibility: float
    dual_infeasibility: float

    def facts(self) -> dict[str, object]:
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
        band = Block.of_seq((
            *((f"status:{self.status.value}",) if self.status is not SolveStatus.SUCCESS else ()),
            *(f"kkt:{name}={value}" for name, value in self.evidence.facts().items() if isinstance(value, float) and value > _TOL),
        ))
        facts: dict[str, object] = {
            "program": self.program,
            "objective": self.objective,
            "status": self.status,
            **self.evidence.facts(),
        }
        return (
            Receipt.of(
                EvidenceScope.CONVEX.value,
                ("emitted", self.program, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=band,
            ),
        )


@tagged_union(frozen=True)
class ConvexProgram:
    tag: Literal["linear", "quadratic", "second_order", "exponential", "power", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    exponential: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    power: tuple[np.ndarray, tuple[PowTerm, ...], np.ndarray, np.ndarray, Policy] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()

    @classmethod
    def Linear(
        cls,
        c: np.ndarray,
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(linear=(c, a_ub, b_ub, Policy(sense, params, backend)))

    @classmethod
    def Quadratic(
        cls,
        p: np.ndarray,
        q: np.ndarray,
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(quadratic=(p, q, a_ub, b_ub, Policy(sense, params, backend)))

    @classmethod
    def SecondOrder(
        cls,
        c: np.ndarray,
        soc_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(second_order=(c, soc_terms, a_ub, b_ub, Policy(sense, params, backend)))

    @classmethod
    def Exponential(
        cls,
        c: np.ndarray,
        exp_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(exponential=(c, exp_terms, a_ub, b_ub, Policy(sense, params, backend)))

    @classmethod
    def Power(
        cls,
        c: np.ndarray,
        pow_terms: tuple[PowTerm, ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(power=(c, pow_terms, a_ub, b_ub, Policy(sense, params, backend)))

    @classmethod
    def Semidefinite(
        cls,
        c_mat: np.ndarray,
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
        backend: Backend = Backend.CLARABEL,
    ) -> Self:
        return cls(semidefinite=(c_mat, a_ub, b_ub, Policy(sense, params, backend)))

    @property
    def policy(self) -> Policy:
        match self:
            case (
                ConvexProgram(tag="linear", linear=(*_, Policy() as policy))
                | ConvexProgram(tag="quadratic", quadratic=(*_, Policy() as policy))
                | ConvexProgram(tag="second_order", second_order=(*_, Policy() as policy))
                | ConvexProgram(tag="exponential", exponential=(*_, Policy() as policy))
                | ConvexProgram(tag="power", power=(*_, Policy() as policy))
                | ConvexProgram(tag="semidefinite", semidefinite=(*_, Policy() as policy))
            ):
                return policy
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


_CEILING: Final[Map[str, float]] = Map.of_seq([("duality_gap", 1e-8), ("primal_infeasibility", 1e-8), ("dual_infeasibility", 1e-8)])


async def solve(program: ConvexProgram, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    async def dispatch() -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
        return (await lane.offload(Kernel.of(_sweep, KernelTrait.RELEASING), program)).bind(lambda rail: rail)

    facts = {"program": program.tag, "binds": len(program.policy.binds)}
    return await evidence_run(EvidenceScope.CONVEX, f"convex.{program.tag}", dispatch, facts=facts, composition=composition)


def graduates(receipt: ConvexReceipt, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[GraduationReceipt]":
    ledger = {
        "duality_gap": receipt.evidence.duality_gap,
        "primal_infeasibility": receipt.evidence.primal_infeasibility,
        "dual_infeasibility": receipt.evidence.dual_infeasibility,
    }
    return GraduationReceipt.graduates(
        EvidenceScope.CONVEX.value,
        HandoffAxis(convex_program=receipt.program),
        receipt.content_key,
        ledger,
        dict(_CEILING.items()),
        composition=composition,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class ConeRow(Struct, frozen=True):
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


def _pow_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return tuple(cp.PowCone3D(_as_mat(ax) @ x, _as_mat(ay) @ x, _as_mat(az) @ x, alpha) for ax, ay, az, alpha in fields.pow_terms)


_CONE_ROWS: Map[str, ConeRow] = Map.of_seq([
    ("linear", ConeRow(_affine_cost, _no_rows)),
    ("quadratic", ConeRow(_quadratic_cost, _no_rows)),
    ("second_order", ConeRow(_affine_cost, _soc_rows)),
    ("exponential", ConeRow(_affine_cost, _exp_rows)),
    ("power", ConeRow(_affine_cost, _pow_rows)),
    ("semidefinite", ConeRow(_trace_cost, _no_rows, psd=True)),
])


class Fields(Struct, frozen=True):
    cost: np.ndarray
    mat: np.ndarray
    rhs: np.ndarray
    lin: np.ndarray | None = None
    terms: tuple[tuple[np.ndarray, float], ...] = ()
    pow_terms: tuple[PowTerm, ...] = ()


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
        case ConvexProgram(tag="power", power=(c, pow_terms, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub), pow_terms=pow_terms)
        case ConvexProgram(tag="semidefinite", semidefinite=(c_mat, a_ub, b_ub, _)):
            return Fields(_symm(c_mat), _as_mat(a_ub), _as_vec(b_ub))
        case _ as unreachable:
            assert_never(unreachable)


def _sweep(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    row = _BACKEND[program.policy.backend]
    if row.solver(cp) not in cp.installed_solvers() or program.tag not in row.cones:
        return _uncertified_sweep(program, None)
    objective, constraints, fields, parameters = _assemble(program, cp)
    problem = cp.Problem(_SENSE[program.policy.sense](cp)(objective), constraints)
    if not problem.is_dcp():
        return _uncertified_sweep(program, fields)
    rails = (_solve_bind(program, problem, constraints, parameters, fields, bind, cp) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _uncertified_sweep(program: ConvexProgram, fields: "Fields | None") -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    rails = (_convex_key(program, fields, bind).map(lambda key: _uncertified(program, key)) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _assemble(program: ConvexProgram, cp: object) -> tuple[object, list[object], "Fields", dict[str, object]]:
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


CONVEX_SOLVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.CONVEX, point="solve", arm="boundary", defect="solver-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([CONVEX_SOLVE]))


def _solve_bind(
    program: ConvexProgram,
    problem: object,
    constraints: list[object],
    parameters: dict[str, object],
    fields: "Fields",
    bind: Map[str, np.ndarray],
    cp: object,
) -> "RuntimeRail[ConvexReceipt]":
    for name, leaf in parameters.items():
        leaf.value = np.asarray(bind.try_find(name).default_value(leaf.value), dtype=float)
    return boundary(
        CONVEX_SOLVE,
        lambda: problem.solve(solver=_BACKEND[program.policy.backend].solver(cp), warm_start=True, canon_backend=cp.SCIPY_CANON_BACKEND),
        catch=(cp.error.SolverError, cp.error.DCPError, cp.error.DPPError, cp.error.ParameterError, ValueError, TypeError),
    ).bind(lambda _solved: _convex_key(program, fields, bind).map(lambda key: _certificate(program, problem, constraints, key, cp)))


def _leaf(name: str, value: np.ndarray, binds: ParamBind, cp: object, parameters: dict[str, object]) -> object:
    if not value.size or not any(name in bind for bind in binds):
        return value
    leaf = cp.Parameter(value.shape, name=name, value=value)
    parameters[name] = leaf
    return leaf


_SENSE: Map[Sense, Callable[[object], object]] = Map.of_seq([(Sense.MIN, attrgetter("Minimize")), (Sense.MAX, attrgetter("Maximize"))])


class BackendRow(Struct, frozen=True):
    solver: Callable[[object], str]
    cones: frozenset[str]


_BACKEND: Map[Backend, BackendRow] = Map.of_seq([
    (Backend.CLARABEL, BackendRow(attrgetter("CLARABEL"), frozenset({"linear", "quadratic", "second_order", "exponential", "power", "semidefinite"}))),
    (Backend.SCS, BackendRow(attrgetter("SCS"), frozenset({"linear", "quadratic", "second_order", "exponential", "power", "semidefinite"}))),
    (Backend.HIGHS, BackendRow(attrgetter("HIGHS"), frozenset({"linear", "quadratic"}))),
])


def _certificate(program: ConvexProgram, problem: object, constraints: list[object], key: ContentKey, cp: object) -> ConvexReceipt:
    if problem.value is None:
        return _uncertified(program, key)
    status = _CONVEX_STATUS.try_find(str(problem.status)).default_value(SolveStatus.OTHER)
    return ConvexReceipt(program.tag, float(problem.value), status, _evidence(constraints, cp), key)


def _uncertified(program: ConvexProgram, key: ContentKey) -> ConvexReceipt:
    return ConvexReceipt(program.tag, float("inf"), SolveStatus.OTHER, ConvexEvidence.uncertified(), key)


def _evidence(constraints: list[object], cp: object) -> ConvexEvidence:
    cells = ((_CONE_KKT[_cone(c, cp)], c) for c in constraints)
    rows = [(kkt, c, np.asarray(c.dual_value, dtype=float), kkt.expr(c)) for kkt, c in cells if c.dual_value is not None]
    solved = [(kkt, c, dual, expr) for kkt, c, dual, expr in rows if expr is not None]
    return ConvexEvidence(
        duality_gap=float(sum(kkt.slack(dual, expr) for kkt, _, dual, expr in solved)),
        primal_infeasibility=max((kkt.primal(expr, c) for kkt, c, _, expr in solved), default=0.0),
        dual_infeasibility=max((kkt.residual(dual, c) for kkt, c, dual, _ in solved), default=0.0),
    )


def _cone(constraint: object, cp: object) -> str:
    match constraint:
        case cp.PSD():
            return "psd"
        case cp.SOC():
            return "soc"
        case cp.PowCone3D():
            return "pow"
        case _:
            return "nonneg"


def _slack_separable(dual: np.ndarray, expr: np.ndarray) -> float:
    return float(np.abs(dual * expr).sum())


def _slack_inner(dual: np.ndarray, expr: np.ndarray) -> float:
    return float(np.abs(np.sum(dual.ravel() * expr.ravel())))


def _residual_nonneg(dual: np.ndarray, constraint: object) -> float:
    return float(np.maximum(-dual, 0.0).max(initial=0.0))


def _residual_soc(dual: np.ndarray, constraint: object) -> float:
    z = dual.ravel()
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _residual_psd(dual: np.ndarray, constraint: object) -> float:
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (dual + dual.T)).min(initial=0.0), 0.0))


def _residual_pow(dual: np.ndarray, constraint: object) -> float:
    u, v, w = dual.reshape(3, -1)
    alpha = np.ravel(np.asarray(constraint.alpha.value, dtype=float))
    return _pow_gap(u / alpha, v / (1.0 - alpha), w, alpha)


def _primal_nonneg(expr: np.ndarray, constraint: object) -> float:
    return float(np.maximum(expr, 0.0).max(initial=0.0))


def _primal_soc(expr: np.ndarray, constraint: object) -> float:
    z = expr.ravel()
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _primal_psd(expr: np.ndarray, constraint: object) -> float:
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (expr + expr.T)).min(initial=0.0), 0.0))


def _primal_pow(expr: np.ndarray, constraint: object) -> float:
    x, y, z = expr.reshape(3, -1)
    return _pow_gap(x, y, z, np.ravel(np.asarray(constraint.alpha.value, dtype=float)))


def _pow_gap(x: np.ndarray, y: np.ndarray, z: np.ndarray, alpha: np.ndarray) -> float:
    mean = np.clip(x, 0.0, None) ** alpha * np.clip(y, 0.0, None) ** (1.0 - alpha)
    violation = np.maximum(np.abs(z) - mean, 0.0)
    return float(max(violation.max(initial=0.0), np.maximum(-x, 0.0).max(initial=0.0), np.maximum(-y, 0.0).max(initial=0.0)))


def _expr_nonneg(constraint: object) -> np.ndarray | None:
    lhs, rhs = constraint.args[0].value, constraint.args[1].value
    return None if lhs is None or rhs is None else np.asarray(lhs, dtype=float) - np.asarray(rhs, dtype=float)


def _expr_soc(constraint: object) -> np.ndarray | None:
    t, x = constraint.args[0].value, constraint.args[1].value
    return None if t is None or x is None else np.append(np.ravel(np.asarray(t, dtype=float)), np.ravel(np.asarray(x, dtype=float)))


def _expr_pow(constraint: object) -> np.ndarray | None:
    values = [a.value for a in constraint.args]
    return None if any(v is None for v in values) else np.stack([np.ravel(np.asarray(v, dtype=float)) for v in values])


def _expr_psd(constraint: object) -> np.ndarray | None:
    matrix = constraint.args[0].value
    return None if matrix is None else np.asarray(matrix, dtype=float)


class ConeKKT(Struct, frozen=True):
    expr: ConeExpr
    slack: ConeSlack
    residual: ConeResidual
    primal: ConePrimal


_CONE_KKT: Map[str, ConeKKT] = Map.of_seq([
    ("nonneg", ConeKKT(_expr_nonneg, _slack_separable, _residual_nonneg, _primal_nonneg)),
    ("soc", ConeKKT(_expr_soc, _slack_inner, _residual_soc, _primal_soc)),
    ("psd", ConeKKT(_expr_psd, _slack_inner, _residual_psd, _primal_psd)),
    ("pow", ConeKKT(_expr_pow, _slack_inner, _residual_pow, _primal_pow)),
])


def _seed_arrays(fields: "Fields | None") -> tuple[np.ndarray, ...]:
    if fields is None:
        return ()
    core = (fields.cost, *((fields.lin,) if fields.lin is not None else ()), fields.mat, fields.rhs)
    term_blocks = tuple(np.append(_as_mat(a).ravel(), bound) for a, bound in fields.terms)
    pow_blocks = tuple(
        np.append(np.concatenate([_as_mat(ax).ravel(), _as_mat(ay).ravel(), _as_mat(az).ravel()]), alpha)
        for ax, ay, az, alpha in fields.pow_terms
    )
    return (*core, *term_blocks, *pow_blocks)


def _convex_key(program: ConvexProgram, fields: "Fields | None", bind: Map[str, np.ndarray]) -> "RuntimeRail[ContentKey]":
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
