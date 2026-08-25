# [PY_COMPUTE_PROGRAM]

Constrained, global, and discrete counterpart of the gradient-driven design loop — the math-program reaches what the differentiable optimizer in `optimization/design#DESIGN` structurally cannot: `ProgramIntent` discriminates a linear program, a mixed-integer program, a derivative-free global minimum, a bounded constrained smooth minimum, and an optimal assignment. This owner carries no numpy floor — the math-program solve IS its backend, so a run without the package returns `Error(Import)` rather than a degraded estimate, the deliberate floor asymmetry against `design.md` and `solvers/nonlinear#NONLINEAR`, mirroring the no-floor Qhull routes of `analysis/spatial#SPATIAL`.

Two backends serve the LP and MIP arms and the VALUE selects between them: the `scipy.optimize` facade is a one-shot cold rebuild per call, while a `Warm` payload on the intent carries a retained `highspy.Highs` whose model is mutated in place so the simplex basis survives every re-solve — the design and study sweeps re-solve one program per perturbed right-hand side, where the facade pays a full rebuild each time. The retained solver also surfaces evidence the facade has no channel for: a `HighsRanging` stability band bounding how far a cost coefficient may move before the optimal basis fails, and a certificate size — an irreducible infeasible subsystem, or a dual/primal ray where presolve proved the refusal without isolating one — so `INFEASIBLE` and `UNBOUNDED` name a subsystem rather than standing as bare verdicts.

Every successful route folds its optimized vector or assignment pair, termination verdict, objective, and maximum constraint violation into the `program` case of the shared `Optimum` on `optimization/design#DESIGN`; a refusal retains absence for values the backend did not produce. Program data admits through `numerics/array#PAYLOAD` on the same `ContentIdentity` seed, and the successful optimum graduates on the existing `solver` `HandoffAxis` case through `Optimum.graduates`.

## [01]-[INDEX]

- [02]-[PROGRAM]: linear/integer/global/constrained/assignment programs over the scipy facade and the retained-HiGHS arm, one `_PROGRAM_ROUTES` row per route, folding the `program` case of the shared `Optimum` on one `ProgramIntent` owner.

## [02]-[PROGRAM]

- Owner: `ProgramIntent` — the discriminant is the program shape, so the gradient loop and the math program are sibling owners on one sub-domain; `Constrained` threads the `LinearConstraint`/`NonlinearConstraint` carriers DIRECTLY, never lowered to the legacy `{"type": "ineq", "fun": ...}` dicts scipy also accepts; a pure-inequality, pure-equality, or mixed LP is one `Linear` shape with each block passed only when non-empty, never a parallel equality-LP owner.
- Cases: `GlobalMethod` rides the `Global` factory's keyword-only `method` parameter — not the `solve` entry `design.md` carries `descent` on — because it discriminates the ONE stochastic route while `Descent` spans all three design routes, so an engine knob on `program.solve` is `None` for the four routes it cannot reach; `DE` carries its `workers=-1`/`polish=True`/`strategy` advanced surface so the population search runs process-parallel and L-BFGS-B-polished at full catalogued power, never the two-argument subset.
- Law: objective and violation are read on a CONVERGED carrier alone and spell absence on every refusal, because an `inf` in either slot enters the graduation ledger as a value and breaches the hub's finiteness refinement on every infeasible crossing — an absent slot leaves the ledger instead, and the hub's key-coverage gate refuses the crossing against the `violation` ceiling as the rejection it is. The stability band and the certificate size are likewise absent wherever the backend surfaces neither, so a facade solve never reports a zero indistinguishable from a measured margin of nothing, and an all-unbounded cost-range set reports no binding margin rather than an `inf`.
- Entry: `ProgramSolve` is one closed family over the three carrier shapes a solve produces — the scipy carrier bound to its own adjudicator shape, the assignment pair, the retained solver with its verdict and evidence — so the iterate read, the verdict fold, and the evidence read are each ONE total match and the route row sheds the `iterate` and `termination` columns it once carried in step. Nothing is derived at construction: an infeasible `linprog` whose `result.x`/`result.fun` are `None` folds its typed `INFEASIBLE` verdict, never a `float(None)` crash captured as a generic fault; the violation reduces through one typed carrier `match`, never a `hasattr(con, "A")` reflective probe.
- Auto: the retained-solver payload's PRESENCE is the discriminant, recoverable from the intent itself, so no `backend: str` knob rides beside a value that already answers; the `direct` route column declares which routes admit the arm at all, and a `None` there makes an unsupported warm solve unspellable rather than a runtime refusal. An integer program carrying a `NonlinearConstraint` declines the direct arm and keeps the `milp` facade, because HiGHS reads a two-sided linear row band and dropping the row silently would solve a different program. The model thunk assembles once — `getNumCol() == 0` is the not-yet-loaded probe — so a caller mutating columns, rows, or coefficients between solves pays back-substitution, never re-encoding. The retained handle rides the `RELEASING` thread band the LP and MIP routes already declare, which is what makes the arm expressible at all: a shared address space carries a live native handle where the `TERMINAL` process crossing the stochastic route declares would have to pickle it, so the one route that cannot hold a retained solver is also the one that declares no direct arm.
- Output: this owner mints `Optimum.Program` with the optimized vector or assignment pair when successful; `.facts`, telemetry, and graduation remain on the shared owner at `optimization/design#DESIGN`.
- Packages: exit code `4` diverges between `linprog` ("numerical") and `milp` ("other") and neither is the matrix-conditioning verdict `solvers/solve#SOLVE` reserves `ILL_CONDITIONED` for, so both fold the honest `OTHER`; `HighsModelStatus` is strictly richer than that five-code table, so `_HIGHS_STATUS` separates the declared-limit haltings from resource exhaustion, the stage errors, and an external interrupt; `getRanging`/`getIis`/`getDualRay`/`getPrimalRay` each answer a status-led tuple and each cost range is a `HighsRangingRecord` whose `value_` array spans columns and rows, so the read slices to the column count; `shgo` and `direct` are deterministic and take no `rng` keyword; the scipy carriers annotate through the `TYPE_CHECKING`-only `opt` alias while the live entrypoints ride module-scope `lazy` binds, so the SciPy and HiGHS trees load on the first route that solves rather than at page import.
- Faults: one `PROGRAM_SOLVE` fence row spans every route — the intent tag is a span fact, never a subject spelling — and `highspy` contributes no exception family to its `catch`, answering through status codes rather than raises.
- Growth: a new route is one `ProgramIntent` case, one `Carried` arm, one `_PROGRAM_ROUTES` row, and one `_project` arm; a new backend for an existing route is one `direct` column value and one entry closure, the carrier family absorbing its result shape as one case; a new global solver is one `GlobalMethod` case and one `solve` arm, never a new `ProgramIntent` tag; a new facade result shape is one `Termination` member and one `adjudicate` arm; a new host code is one `_PROGRAM_STATUS` or `_HIGHS_STATUS` row.

```python
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.optimization.design import Optimum
from rasm.compute.solvers.solve import SolveStatus
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Enforcement, Kernel, KernelTrait

lazy import highspy
lazy from scipy.optimize import (
    Bounds,
    LinearConstraint,
    NonlinearConstraint,
    differential_evolution,
    direct,
    dual_annealing,
    linear_sum_assignment,
    linprog,
    milp,
    minimize,
    shgo,
)
lazy from scipy.sparse import csc_matrix

if TYPE_CHECKING:
    import scipy.optimize as opt

# --- [TYPES] ----------------------------------------------------------------------------

type Bound = tuple[float, float]
type Objective = Callable[[np.ndarray], float]
type Constraints = "tuple[opt.LinearConstraint | opt.NonlinearConstraint, ...]"
type DirectEntry = Callable[[Carried, "Warm"], "ProgramSolve"]
type Carried = (
    tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, "Warm | None"]
    | tuple[np.ndarray, np.ndarray, np.ndarray, Constraints, "Warm | None"]
    | tuple[Objective, np.ndarray, "GlobalMethod"]
    | tuple[Objective, np.ndarray, np.ndarray, Constraints]
    | tuple[np.ndarray]
)


class Termination(StrEnum):
    CODED = "coded"
    FLAGGED = "flagged"

    def adjudicate(self, result: "opt.OptimizeResult") -> SolveStatus:
        match self:
            case Termination.CODED:
                return _PROGRAM_STATUS.try_find(int(result.status)).default_value(SolveStatus.OTHER)
            case Termination.FLAGGED:
                return SolveStatus.SUCCESS if bool(result.success) else SolveStatus.STAGNATION
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class GlobalMethod:
    tag: Literal["de", "annealing", "simplicial", "direct"] = tag()
    de: tuple[int, bool, str] = case()
    annealing: None = case()
    simplicial: None = case()
    direct: None = case()

    @classmethod
    def DE(cls, workers: int = -1, polish: bool = True, strategy: str = "best1bin") -> Self:
        return cls(de=(workers, polish, strategy))

    @classmethod
    def Annealing(cls) -> Self:
        return cls(annealing=None)

    @classmethod
    def Simplicial(cls) -> Self:
        return cls(simplicial=None)

    @classmethod
    def Direct(cls) -> Self:
        return cls(direct=None)

    def solve(self, func: Objective, box: np.ndarray, seed: int) -> "opt.OptimizeResult":
        pairs = box.reshape(-1, 2)
        match self:
            case GlobalMethod(tag="de", de=(workers, polish, strategy)):
                return differential_evolution(func, pairs, rng=seed, workers=workers, polish=polish, strategy=strategy)
            case GlobalMethod(tag="annealing"):
                return dual_annealing(func, pairs, rng=seed)
            case GlobalMethod(tag="simplicial"):
                return shgo(func, pairs)
            case GlobalMethod(tag="direct"):
                return direct(func, pairs)
            case _ as unreachable:
                assert_never(unreachable)


_EMPTY_1D: np.ndarray = np.empty(0, dtype=float)
_EMPTY_2D: np.ndarray = np.empty((0, 0), dtype=float)


_DEFAULT_GLOBAL: GlobalMethod = GlobalMethod.DE()


@tagged_union(frozen=True)
class ProgramIntent:
    tag: Literal["linear", "integer", "stochastic", "constrained", "assignment"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, tuple[Bound, ...], "Warm | None"] = case()
    integer: tuple[np.ndarray, np.ndarray, tuple[Bound, ...], Constraints, "Warm | None"] = case()
    stochastic: tuple[Objective, tuple[Bound, ...], GlobalMethod] = case()
    constrained: tuple[Objective, np.ndarray, tuple[Bound, ...], Constraints] = case()
    assignment: np.ndarray = case()

    @classmethod
    def Linear(
        cls,
        c: np.ndarray,
        a_ub: np.ndarray = _EMPTY_2D,
        b_ub: np.ndarray = _EMPTY_1D,
        bounds: tuple[Bound, ...] = (),
        *,
        a_eq: np.ndarray = _EMPTY_2D,
        b_eq: np.ndarray = _EMPTY_1D,
        warm: "Warm | None" = None,
    ) -> Self:
        return cls(linear=(c, a_ub, b_ub, a_eq, b_eq, bounds, warm))

    @classmethod
    def Integer(
        cls, c: np.ndarray, integrality: np.ndarray, bounds: tuple[Bound, ...], constraints: Constraints = (), *, warm: "Warm | None" = None
    ) -> Self:
        return cls(integer=(c, integrality, bounds, constraints, warm))

    @classmethod
    def Global(cls, objective: Objective, bounds: tuple[Bound, ...], *, method: GlobalMethod = _DEFAULT_GLOBAL) -> Self:
        return cls(stochastic=(objective, bounds, method))

    @classmethod
    def Constrained(cls, objective: Objective, x0: np.ndarray, bounds: tuple[Bound, ...], constraints: Constraints = ()) -> Self:
        return cls(constrained=(objective, x0, bounds, constraints))

    @classmethod
    def Assignment(cls, cost: np.ndarray) -> Self:
        return cls(assignment=cost)


# --- [CONSTANTS] ------------------------------------------------------------------------

_SEED = 0

_PROGRAM_STATUS: Map[int, SolveStatus] = Map.of_seq([
    (0, SolveStatus.SUCCESS),
    (1, SolveStatus.MAX_STEPS),
    (2, SolveStatus.INFEASIBLE),
    (3, SolveStatus.UNBOUNDED),
    (4, SolveStatus.OTHER),
])

_HIGHS_OK: Final[str] = "kOk"
_HIGHS_OPTIMAL: Final[str] = "kOptimal"

_HIGHS_STATUS: Map[str, SolveStatus] = Map.of_seq([
    ("kOptimal", SolveStatus.SUCCESS),
    ("kInfeasible", SolveStatus.INFEASIBLE),
    ("kUnbounded", SolveStatus.UNBOUNDED),
    ("kUnboundedOrInfeasible", SolveStatus.INFEASIBLE),
    ("kObjectiveBound", SolveStatus.MAX_STEPS),
    ("kObjectiveTarget", SolveStatus.MAX_STEPS),
    ("kTimeLimit", SolveStatus.MAX_STEPS),
    ("kIterationLimit", SolveStatus.MAX_STEPS),
    ("kSolutionLimit", SolveStatus.MAX_STEPS),
    ("kMemoryLimit", SolveStatus.BREAKDOWN),
    ("kModelError", SolveStatus.BREAKDOWN),
    ("kSolveError", SolveStatus.BREAKDOWN),
    ("kPresolveError", SolveStatus.BREAKDOWN),
    ("kPostsolveError", SolveStatus.BREAKDOWN),
    ("kLoadError", SolveStatus.BREAKDOWN),
])


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ProgramSolve:
    tag: Literal["host", "matched", "retained"] = tag()
    host: "tuple[opt.OptimizeResult, Termination]" = case()
    matched: tuple[np.ndarray, np.ndarray] = case()
    retained: tuple[object, str, float | None, int | None] = case()

    @classmethod
    def Host(cls, result: "opt.OptimizeResult", shape: Termination) -> Self:
        return cls(host=(result, shape))

    @classmethod
    def Matched(cls, pair: tuple[np.ndarray, np.ndarray]) -> Self:
        return cls(matched=pair)

    @classmethod
    def Retained(cls, solver: object, verdict: str, fragility: float | None, witness: int | None) -> Self:
        return cls(retained=(solver, verdict, fragility, witness))

    @property
    def status(self) -> SolveStatus:
        match self:
            case ProgramSolve(tag="matched"):
                return SolveStatus.SUCCESS
            case ProgramSolve(tag="retained", retained=(_, verdict, _, _)):
                return _HIGHS_STATUS.try_find(verdict).default_value(SolveStatus.OTHER)
            case ProgramSolve(tag="host", host=(result, shape)):
                return shape.adjudicate(result)
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def evidence(self) -> tuple[float | None, int | None]:
        match self:
            case ProgramSolve(tag="retained", retained=(_, _, fragility, witness)):
                return (fragility, witness)
            case _:
                return (None, None)


class Warm(Struct, frozen=True):
    handle: object
    ranging: bool = True
    witness: bool = True

    @classmethod
    def opened(cls, *, ranging: bool = True, witness: bool = True) -> Self:
        held = highspy.Highs()
        held.silent()
        return cls(held, ranging, witness)

    def solved(self, model: Callable[[], object]) -> ProgramSolve:
        if self.handle.getNumCol() == 0:
            self.handle.passModel(model())
        self.handle.run()
        verdict = self.handle.getModelStatus().name
        return ProgramSolve.Retained(self.handle, verdict, self._fragility(verdict), self._witness(verdict))

    def _fragility(self, verdict: str) -> float | None:
        if not self.ranging or verdict != _HIGHS_OPTIMAL:
            return None
        status, ranging = self.handle.getRanging()
        if status.name != _HIGHS_OK:
            return None
        columns = self.handle.getNumCol()
        widths = np.asarray(ranging.col_cost_up.value_[:columns], dtype=float) - np.asarray(ranging.col_cost_dn.value_[:columns], dtype=float)
        binding = widths[np.isfinite(widths)]
        return -float(binding.min()) if binding.size else None

    def _subsystem(self) -> int | None:
        status, iis = self.handle.getIis()
        return len(iis.col_index_) + len(iis.row_index_) if status.name == _HIGHS_OK else None

    def _witness(self, verdict: str) -> int | None:
        read = _CERTIFICATE.try_find(verdict).default_value(None) if self.witness else None
        return read(self) if read is not None else None


class ProgramRoute(Struct, frozen=True):
    entry: Callable[[Carried, int], ProgramSolve]
    direct: DirectEntry | None
    carriers: Callable[[Carried, ProgramSolve], Constraints]
    seeded: bool


# --- [OPERATIONS] -----------------------------------------------------------------------


async def solve(
    intent: ProgramIntent, lane: LanePolicy, *, seed: int = _SEED, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeRail[Optimum]":
    async def dispatch() -> "RuntimeRail[Optimum]":
        kernel = Kernel.of(
            _program_kernel,
            KernelTrait.RELEASING,
            enforcement=Enforcement.TERMINAL if intent.tag == "stochastic" else Enforcement.COOPERATIVE,
        )
        return (await lane.offload(kernel, intent, seed)).bind(lambda rail: rail)

    facts = {"program": intent.tag, "seed": seed, "backend": "direct" if _warmed(intent) is not None else "facade"}
    return await evidence_run(EvidenceScope.PROGRAM, f"program.{intent.tag}", dispatch, facts=facts, composition=composition)


PROGRAM_SOLVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.PROGRAM, point="solve", arm="boundary", defect="solve-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([PROGRAM_SOLVE]))


def _program_kernel(intent: ProgramIntent, seed: int) -> "RuntimeRail[Optimum]":
    return boundary(
        PROGRAM_SOLVE, lambda: _program_optimum(intent, seed), catch=(np.linalg.LinAlgError, ValueError, TypeError)
    ).bind(lambda r: r)


def _program_optimum(intent: ProgramIntent, seed: int) -> "RuntimeRail[Optimum]":
    route = _PROGRAM_ROUTES[intent.tag]
    fields = _project(intent)
    warm = _warmed(intent)
    outcome = route.direct(fields, warm) if warm is not None and route.direct is not None else route.entry(fields, seed)
    status = outcome.status
    graded = _graded(route, fields, outcome) if status is SolveStatus.SUCCESS else (None, None, None)
    fragility, witness = outcome.evidence
    return _program_key(intent, fields, seed if route.seeded else None).map(
        lambda key: Optimum.Program(graded[0], intent.tag, graded[1], status, graded[2], key, fragility=fragility, witness=witness)
    )


def _graded(
    route: ProgramRoute, fields: Carried, outcome: ProgramSolve
) -> tuple[np.ndarray | tuple[np.ndarray, np.ndarray], float, float]:
    value, objective = _iterate(fields, outcome)
    violation = 0.0 if isinstance(value, tuple) else _violation(route.carriers(fields, outcome), value)
    return value, objective, violation


def _warmed(intent: ProgramIntent) -> "Warm | None":
    match intent:
        case ProgramIntent(tag="linear", linear=(*_, warm)):
            return warm
        case ProgramIntent(tag="integer", integer=(_, _, _, constraints, warm)):
            return warm if all(_row_shaped(con) for con in constraints) else None
        case _:
            return None


def _row_shaped(constraint: object) -> bool:
    return isinstance(constraint, LinearConstraint)


def _ray_support(certificate: "tuple[object, bool, np.ndarray]") -> int | None:
    _status, present, ray = certificate
    return int(np.count_nonzero(np.asarray(ray, dtype=float))) if present else None


_CERTIFICATE: Map[str, Callable[["Warm"], int | None]] = Map.of_seq([
    ("kInfeasible", lambda warm: warm._subsystem() or _ray_support(warm.handle.getDualRay())),
    ("kUnbounded", lambda warm: _ray_support(warm.handle.getPrimalRay())),
])


def _program_key(intent: ProgramIntent, fields: Carried, seed: int | None) -> "RuntimeRail[ContentKey]":
    slots = [(i, f) for i, f in enumerate(fields) if isinstance(f, np.ndarray) and f.size]
    buffer = b"".join(np.ascontiguousarray(field).tobytes() for _, field in slots)
    shape_tag = "".join(f".{i}:{f.ndim}x{'x'.join(map(str, f.shape))}" for i, f in slots)
    seed_tag = f".{seed}{_engine_tag(intent)}" if seed is not None else ""
    return ContentIdentity.of(f"program.{intent.tag}{shape_tag}{seed_tag}", buffer)


def _engine_tag(intent: ProgramIntent) -> str:
    match intent:
        case ProgramIntent(tag="stochastic", stochastic=(_, _, GlobalMethod(tag="de", de=(_, _, strategy)))):
            return f".de.{strategy}"
        case ProgramIntent(tag="stochastic", stochastic=(_, _, method)):
            return f".{method.tag}"
        case _:
            return ""


def _bounds(box: np.ndarray) -> "opt.Bounds | None":
    pairs = box.reshape(-1, 2)
    return Bounds(pairs[:, 0], pairs[:, 1]) if pairs.size else None


def _violation(constraints: Constraints, x: np.ndarray) -> float:
    def residual(con: "opt.LinearConstraint | opt.NonlinearConstraint") -> float:
        match con:
            case LinearConstraint():
                value = np.asarray(con.A, dtype=float) @ x
            case NonlinearConstraint():
                value = np.asarray(con.fun(x), dtype=float)
            case _:
                return 0.0
        excess = np.maximum(np.maximum(np.asarray(con.lb) - value, value - np.asarray(con.ub)), 0.0)
        return float(excess.max(initial=0.0))

    return float(max((residual(con) for con in constraints), default=0.0))


# --- [COMPOSITION] ----------------------------------------------------------------------


def _entry_linear(fields: Carried, _: int) -> ProgramSolve:
    cost, ub_mat, ub_rhs, eq_mat, eq_rhs, box, _warm = fields
    return ProgramSolve.Host(
        linprog(
            cost,
            A_ub=ub_mat if ub_rhs.size else None,
            b_ub=ub_rhs if ub_rhs.size else None,
            A_eq=eq_mat if eq_rhs.size else None,
            b_eq=eq_rhs if eq_rhs.size else None,
            bounds=_bounds(box),
            method="highs",
        ),
        Termination.CODED,
    )


def _entry_integer(fields: Carried, _: int) -> ProgramSolve:
    cost, flags, box, constraints, _warm = fields
    return ProgramSolve.Host(milp(cost, integrality=flags, bounds=_bounds(box), constraints=list(constraints) or None), Termination.CODED)


def _entry_stochastic(fields: Carried, seed: int) -> ProgramSolve:
    objective_fn, box, method = fields
    return ProgramSolve.Host(method.solve(objective_fn, box, seed), Termination.FLAGGED)


def _entry_constrained(fields: Carried, _: int) -> ProgramSolve:
    objective_fn, start, box, constraints = fields
    return ProgramSolve.Host(
        minimize(objective_fn, start, method="trust-constr", bounds=_bounds(box), constraints=list(constraints)), Termination.FLAGGED
    )


def _entry_assignment(fields: Carried, _: int) -> ProgramSolve:
    (matrix,) = fields
    return ProgramSolve.Matched(linear_sum_assignment(matrix))


def _direct_linear(fields: Carried, warm: Warm) -> ProgramSolve:
    cost, ub_mat, ub_rhs, eq_mat, eq_rhs, box, _warm = fields
    mat, lower, upper = _stacked(_band(ub_mat, ub_rhs, equality=False), _band(eq_mat, eq_rhs, equality=True))
    return warm.solved(lambda: _highs_model(cost, mat, lower, upper, box, None))


def _direct_integer(fields: Carried, warm: Warm) -> ProgramSolve:
    cost, flags, box, constraints, _warm = fields
    mat, lower, upper = _stacked(*(_band(np.asarray(con.A, dtype=float), np.asarray(con.ub, dtype=float), lb=con.lb) for con in constraints))
    return warm.solved(lambda: _highs_model(cost, mat, lower, upper, box, flags))


def _iterate(fields: Carried, outcome: ProgramSolve) -> tuple[np.ndarray | tuple[np.ndarray, np.ndarray], float]:
    match outcome:
        case ProgramSolve(tag="host", host=(result, _)):
            return np.asarray(result.x, dtype=float), float(result.fun)
        case ProgramSolve(tag="retained", retained=(solver, *_)):
            return np.asarray(solver.getSolution().col_value, dtype=float), float(solver.getInfo().objective_function_value)
        case ProgramSolve(tag="matched", matched=(rows, cols)):
            (matrix,) = fields
            selected = matrix[rows, cols]
            return (np.asarray(rows), np.asarray(cols)), float(selected.sum())
        case _ as unreachable:
            assert_never(unreachable)


def _carriers_linear(fields: Carried, _: ProgramSolve) -> Constraints:
    _cost, ub_mat, ub_rhs, eq_mat, eq_rhs, _box, _warm = fields
    return (*(LinearConstraint(ub_mat, -np.inf, ub_rhs),) * bool(ub_rhs.size), *(LinearConstraint(eq_mat, eq_rhs, eq_rhs),) * bool(eq_rhs.size))


def _no_carriers(_: Carried, __: ProgramSolve) -> Constraints:
    return ()


def _carriers_integer(fields: Carried, _: ProgramSolve) -> Constraints:
    _cost, _flags, _box, constraints, _warm = fields
    return constraints


def _carriers_constrained(fields: Carried, _: ProgramSolve) -> Constraints:
    _objective, _x0, _box, constraints = fields
    return constraints


def _band(mat: np.ndarray, rhs: np.ndarray, *, equality: bool = False, lb: np.ndarray | None = None) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    lower = np.asarray(lb, dtype=float) if lb is not None else (rhs if equality else np.full(rhs.size, -np.inf))
    return (mat, lower, rhs)


def _stacked(*bands: tuple[np.ndarray, np.ndarray, np.ndarray]) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    live = tuple(band for band in bands if band[2].size)
    return (
        (np.vstack([mat for mat, _, _ in live]), np.concatenate([lo for _, lo, _ in live]), np.concatenate([hi for _, _, hi in live]))
        if live
        else (_EMPTY_2D, _EMPTY_1D, _EMPTY_1D)
    )


def _highs_model(cost: np.ndarray, mat: np.ndarray, lower: np.ndarray, upper: np.ndarray, box: np.ndarray, integrality: np.ndarray | None) -> object:
    lp = highspy.HighsLp()
    pairs = box.reshape(-1, 2)
    lp.num_col_, lp.num_row_ = int(cost.size), int(upper.size)
    lp.col_cost_ = cost
    lp.col_lower_ = pairs[:, 0] if pairs.size else np.full(cost.size, -highspy.kHighsInf)
    lp.col_upper_ = pairs[:, 1] if pairs.size else np.full(cost.size, highspy.kHighsInf)
    lp.row_lower_, lp.row_upper_ = lower, upper
    sparse = csc_matrix(mat) if lp.num_row_ else csc_matrix((0, lp.num_col_))
    lp.a_matrix_.format_ = highspy.MatrixFormat.kColwise
    lp.a_matrix_.start_, lp.a_matrix_.index_, lp.a_matrix_.value_ = sparse.indptr, sparse.indices, sparse.data
    if integrality is not None:
        lp.integrality_ = [highspy.HighsVarType.kInteger if flag else highspy.HighsVarType.kContinuous for flag in integrality]
    model = highspy.HighsModel()
    model.lp_ = lp
    return model


def _project(intent: ProgramIntent) -> Carried:
    match intent:
        case ProgramIntent(tag="linear", linear=(c, a_ub, b_ub, a_eq, b_eq, bounds, warm)):
            return (
                np.asarray(c, dtype=float),
                np.atleast_2d(np.asarray(a_ub, dtype=float)),
                np.asarray(b_ub, dtype=float),
                np.atleast_2d(np.asarray(a_eq, dtype=float)),
                np.asarray(b_eq, dtype=float),
                np.asarray(bounds, dtype=float),
                warm,
            )
        case ProgramIntent(tag="integer", integer=(c, integrality, bounds, constraints, warm)):
            return (np.asarray(c, dtype=float), np.asarray(integrality), np.asarray(bounds, dtype=float), constraints, warm)
        case ProgramIntent(tag="stochastic", stochastic=(objective_fn, bounds, method)):
            return (objective_fn, np.asarray(bounds, dtype=float), method)
        case ProgramIntent(tag="constrained", constrained=(objective_fn, x0, bounds, constraints)):
            return (objective_fn, np.asarray(x0, dtype=float), np.asarray(bounds, dtype=float), constraints)
        case ProgramIntent(tag="assignment", assignment=cost):
            return (np.atleast_2d(np.asarray(cost, dtype=float)),)
        case _ as unreachable:
            assert_never(unreachable)


_PROGRAM_ROUTES: Map[str, ProgramRoute] = Map.of_seq([
    ("linear", ProgramRoute(_entry_linear, _direct_linear, _carriers_linear, False)),
    ("integer", ProgramRoute(_entry_integer, _direct_integer, _carriers_integer, False)),
    ("stochastic", ProgramRoute(_entry_stochastic, None, _no_carriers, True)),
    ("constrained", ProgramRoute(_entry_constrained, None, _carriers_constrained, False)),
    ("assignment", ProgramRoute(_entry_assignment, None, _no_carriers, False)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
