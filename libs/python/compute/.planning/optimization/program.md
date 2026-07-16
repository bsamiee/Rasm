# [PY_COMPUTE_PROGRAM]

The constrained, global, and discrete counterpart of the gradient-driven design loop — the math-program reaches what the differentiable optimizer in `optimization/design.md#DESIGN` structurally cannot: `ProgramIntent` discriminates a linear program, a mixed-integer program, a derivative-free global minimum, a bounded constrained smooth minimum, and an optimal assignment over `scipy.optimize`. This owner carries no numpy floor — the math-program solve IS `scipy.optimize`, so a run without the package returns `Error(Import)` rather than a degraded estimate, the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR`, mirroring the no-floor Qhull routes of `analysis/spatial.md#SPATIAL`.

Every route folds the host termination verdict, the objective, and the maximum constraint-violation residual into the `program` case of the shared `OutcomeReceipt` on `optimization/design.md#DESIGN`, carrying the `SolveStatus` vocabulary `solvers/receipt.md#RECEIPT` owns — so an infeasible, unbounded, or iteration-limited program is a distinct first-class verdict the C# graduation gate reads, never a boolean collapsing every non-success cause to `False`. Program data admits through `numerics/array.md#PAYLOAD` on the same `ContentIdentity` seed, and the certified optimum graduates on the existing `solver` `HandoffAxis` case through `graduation/handoff.md#GRADUATION`.

## [01]-[INDEX]

- [01]-[PROGRAM]: linear/integer/global/constrained/assignment programs over `scipy.optimize`, one `_PROGRAM_ROUTES` row per route, folding the `program` case of the shared `OutcomeReceipt` on one `ProgramIntent` owner.

## [02]-[PROGRAM]

- Owner: `ProgramIntent` — the discriminant is the program shape, so the gradient loop and the math program are sibling owners on one sub-domain; `Constrained` threads the `LinearConstraint`/`NonlinearConstraint` carriers DIRECTLY, never lowered to the legacy `{"type": "ineq", "fun": ...}` dicts scipy also accepts; a pure-inequality, pure-equality, or mixed LP is one `Linear` shape with each block passed only when non-empty, never a parallel equality-LP owner.
- Cases: `GlobalMethod` rides the `Global` factory's keyword-only `method` parameter — not the `solve` entry `design.md` carries `descent` on — because it discriminates the ONE stochastic route while `Descent` spans all three design routes, so an engine knob on `program.solve` is `None` for the four routes it cannot reach; `DE` carries its `workers=-1`/`polish=True`/`strategy` advanced surface so the population search runs process-parallel and L-BFGS-B-polished at full catalogued power, never the two-argument subset.
- Entry: `ProgramSolve` carries ONLY the raw host carrier, and the `iterate` read is LAZY past adjudication — an infeasible `linprog` whose `result.x`/`result.fun` are `None` folds its typed `INFEASIBLE` verdict, never a `float(None)` crash captured as a generic fault; `Termination.adjudicate` dispatches on `self`, so it never names the `TYPE_CHECKING`-only `opt.OptimizeResult` as a runtime class pattern; the violation reduces through one typed carrier `match`, never a `hasattr(con, "A")` reflective probe.
- Receipt: this owner mints only the `OutcomeReceipt.Program` factory case — the `.facts` projection and `contribute` fold live on the shared owner at `optimization/design.md#DESIGN`, never a program-specific body.
- Packages: exit code `4` diverges between `linprog` ("numerical") and `milp` ("other") and neither is the matrix-conditioning verdict `solvers/receipt.md#RECEIPT` reserves `ILL_CONDITIONED` for, so both fold the honest `OTHER`; `shgo` and `direct` are deterministic and take no `rng` keyword; the scipy carriers annotate under `TYPE_CHECKING` and the gated package never imports at runtime.
- Growth: a new route is one `ProgramIntent` case, one `Carried` arm, one `_PROGRAM_ROUTES` row, and one `_project` arm; a new global solver is one `GlobalMethod` case plus one `solve` arm, never a new `ProgramIntent` tag; a new termination shape is one `Termination` member plus one `adjudicate` arm; a new host code is one `_PROGRAM_STATUS` row.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Final, TYPE_CHECKING, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.optimization.design import OutcomeReceipt
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy, Modality

if TYPE_CHECKING:
    import scipy.optimize as opt  # `OptimizeResult`/`Bounds` annotation carriers only; the gated package never imports at runtime

# --- [TYPES] -------------------------------------------------------------------------------

type Bound = tuple[float, float]
type Objective = Callable[[np.ndarray], float]
type Constraints = "tuple[opt.LinearConstraint | opt.NonlinearConstraint, ...]"  # the scipy carrier tuple the `_violation` `match` reads
# the prepared per-route buffers as the precise union of the five route payload shapes, never an
# erased `tuple[object, ...]`: each arm is the normalized buffer set the route's `_entry`/`iterate`/
# `carriers` closures consume and `_program_key` digests, so a closure narrows its shape by route
# construction rather than indexing a phantom `object` tuple by magic position.
type Carried = (
    tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray]  # linear: cost, A_ub, b_ub, A_eq, b_eq, box
    | tuple[np.ndarray, np.ndarray, np.ndarray, Constraints]  # integer: cost, integrality, box, constraints
    | tuple[Objective, np.ndarray, "GlobalMethod"]  # stochastic: objective, box, engine
    | tuple[Objective, np.ndarray, np.ndarray, Constraints]  # constrained: objective, x0, box, constraints
    | tuple[np.ndarray]  # assignment: cost matrix
)


class Termination(StrEnum):
    # the result-shape adjudicator carried as a route policy value, one table-driven `adjudicate` over
    # the three host-result shapes: `CODED` reads the integer `.status`, `FLAGGED` the boolean `.success`,
    # `FEASIBLE` the assignment route optimal-by-construction (no host result).
    CODED = "coded"
    FLAGGED = "flagged"
    FEASIBLE = "feasible"

    def adjudicate(self, result: "opt.OptimizeResult | None") -> SolveStatus:
        # the `FEASIBLE` arm folds `SUCCESS` over the `None` assignment carrier; `CODED`/`FLAGGED` ride a
        # non-`None` host carrier by route construction, so the `result is None` arm narrows the `| None`
        # to `OTHER` and the `.status`/`.success` reads stay type-checker-total without naming the
        # `TYPE_CHECKING`-only `opt.OptimizeResult` as a runtime match class.
        match self:
            case Termination.FEASIBLE:
                return SolveStatus.SUCCESS
            case _ if result is None:
                return SolveStatus.OTHER
            case Termination.CODED:
                return _PROGRAM_STATUS.try_find(int(result.status)).default_value(SolveStatus.OTHER)
            case Termination.FLAGGED:
                return SolveStatus.SUCCESS if bool(result.success) else SolveStatus.STAGNATION
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class GlobalMethod:
    # the derivative-free global-search family as ONE engine-as-policy union (the `design.md` `Descent`
    # shape): `DE` carries its `(workers, polish, strategy)` advanced surface, the rest are bare cases.
    # `solve` is the one total projection binding each arm's `scipy.optimize` entrypoint behind the
    # gated import, threading the SPEC-007 `rng` seed so a reproducible content-keyed global solve maps
    # to a fixed iterate — never four parallel `ProgramIntent` cases beside the LP/MILP solve.
    tag: Literal["de", "annealing", "simplicial", "direct"] = tag()
    de: tuple[int, bool, str] = case()  # (workers, polish, strategy)
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
        from scipy.optimize import differential_evolution, direct, dual_annealing, shgo

        pairs = box.reshape(-1, 2)
        match self:
            case GlobalMethod(tag="de", de=(workers, polish, strategy)):
                return differential_evolution(func, pairs, rng=seed, workers=workers, polish=polish, strategy=strategy)
            case GlobalMethod(tag="annealing"):
                return dual_annealing(func, pairs, rng=seed)
            case GlobalMethod(tag="simplicial"):
                return shgo(func, pairs)  # deterministic simplicial-homology global search; no `rng` keyword
            case GlobalMethod(tag="direct"):
                return direct(func, pairs)  # deterministic Lipschitz partition; no `rng` keyword
            case _ as unreachable:
                assert_never(unreachable)


_EMPTY_1D: np.ndarray = np.empty(0, dtype=float)  # `ProgramIntent.Linear` default-arg anchors, read at class definition
_EMPTY_2D: np.ndarray = np.empty((0, 0), dtype=float)


_DEFAULT_GLOBAL: GlobalMethod = GlobalMethod.DE()  # the global-route default engine; per-call `method` overrides it


@tagged_union(frozen=True)
class ProgramIntent:
    tag: Literal["linear", "integer", "stochastic", "constrained", "assignment"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray, tuple[Bound, ...]] = case()
    integer: tuple[np.ndarray, np.ndarray, tuple[Bound, ...], Constraints] = case()
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
    ) -> Self:
        return cls(linear=(c, a_ub, b_ub, a_eq, b_eq, bounds))

    @classmethod
    def Integer(cls, c: np.ndarray, integrality: np.ndarray, bounds: tuple[Bound, ...], constraints: Constraints = ()) -> Self:
        return cls(integer=(c, integrality, bounds, constraints))

    @classmethod
    def Global(cls, objective: Objective, bounds: tuple[Bound, ...], *, method: GlobalMethod = _DEFAULT_GLOBAL) -> Self:
        return cls(stochastic=(objective, bounds, method))

    @classmethod
    def Constrained(cls, objective: Objective, x0: np.ndarray, bounds: tuple[Bound, ...], constraints: Constraints = ()) -> Self:
        return cls(constrained=(objective, x0, bounds, constraints))

    @classmethod
    def Assignment(cls, cost: np.ndarray) -> Self:
        return cls(assignment=cost)


# --- [CONSTANTS] ---------------------------------------------------------------------------

_SEED = 0

# the program family modality row: the scipy LP/MILP/global/assignment bodies are native work
# riding the runtime THREAD band; policy DATA, never a per-page literal.
_MODALITY: Final[Modality] = Modality.THREAD

# `0/1/2/3` agree across the `linprog` and `milp` exit-code tables; code `4` diverges — `linprog`
# "numerical difficulties", `milp` "other" — and neither is the matrix-conditioning `conlim` verdict
# `solvers/receipt.md#RECEIPT` reserves `ILL_CONDITIONED` for, so both fold the honest `OTHER`.
_PROGRAM_STATUS: Map[int, SolveStatus] = Map.of_seq([(0, SolveStatus.SUCCESS), (1, SolveStatus.MAX_STEPS), (2, SolveStatus.INFEASIBLE), (3, SolveStatus.UNBOUNDED), (4, SolveStatus.OTHER)])


# --- [MODELS] ------------------------------------------------------------------------------


class ProgramSolve(Struct, frozen=True):  # GC-tracked: carries the host `OptimizeResult` and assignment tuple
    # the raw host carrier and nothing derived: the iterate and objective are read LAZILY off the
    # carrier only past `Termination.adjudicate`, so an infeasible `linprog` whose `result.x`/`result.fun`
    # are `None` adjudicates to `SolveStatus.INFEASIBLE` rather than crashing a `float(None)` into the fence.
    result: "opt.OptimizeResult | None"  # host result, or `None` for the closed-form assignment route
    assignment: "tuple[np.ndarray, np.ndarray] | None"  # the `linear_sum_assignment` row/column pair, else `None`


class ProgramRoute(Struct, frozen=True):  # GC-tracked: carries the entry/iterate/carriers closures
    # the buffer-and-key projection is NOT a route column: `_project` owns every per-tag `Carried`
    # shape in one `match`, so the row carries only what genuinely varies per route.
    entry: Callable[[Carried, int], ProgramSolve]  # binds the route's `scipy.optimize` entrypoint, returning the RAW carrier
    iterate: Callable[[Carried, ProgramSolve], tuple[np.ndarray, float]]  # reads (x, objective) off the SUCCESS carrier
    carriers: Callable[[Carried, ProgramSolve], Constraints]  # reifies the `LinearConstraint`/`NonlinearConstraint` fold inputs
    termination: Termination  # the static result-shape adjudicator policy for this route
    seeded: bool  # whether the `rng` seed alters the iterate, so it folds into the content key


# --- [OPERATIONS] --------------------------------------------------------------------------


async def solve(intent: ProgramIntent, lane: LanePolicy, *, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    # `boundary` fences the scipy solve (raising routes, the gated `ImportError`); the railed
    # `ContentIdentity.of` key threads through `.bind` so a digest fault propagates on the one rail
    # rather than collapsing to a phantom bare `ContentKey`. The scipy global/discrete bodies are
    # native work riding the runtime THREAD band under the hub weave — span, fence, and the
    # `@receipted(REDACTION)` receipt harvest composed, with the MODULE-LEVEL `_program_kernel`
    # crossing the lane; compute mints no limiter and the deterministic solve takes no retry.
    async def dispatch() -> "RuntimeRail[OutcomeReceipt]":
        return (await lane.offload(_program_kernel, intent, seed, modality=_MODALITY)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.PROGRAM, f"program.{intent.tag}", dispatch)


def _program_kernel(intent: ProgramIntent, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    return boundary(f"program.{intent.tag}", lambda: _program_receipt(intent, seed)).bind(lambda r: r)


def _program_receipt(intent: ProgramIntent, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    route = _PROGRAM_ROUTES[intent.tag]
    fields = _project(intent)
    outcome = route.entry(fields, seed)
    status = route.termination.adjudicate(outcome.result)
    # the iterate and objective are read only on SUCCESS, so a non-success carrier's `None` fields are
    # never coerced; an infeasible/unbounded/iteration-limited program folds `inf`/`inf` with its verdict.
    x, objective = route.iterate(fields, outcome) if status is SolveStatus.SUCCESS else (_EMPTY_1D, float("inf"))
    violation = _violation(route.carriers(fields, outcome), x)
    return _program_key(intent, fields, seed if route.seeded else None).map(
        lambda key: OutcomeReceipt.Program(intent.tag, objective, status, violation, key)
    )


def _program_key(intent: ProgramIntent, fields: Carried, seed: int | None) -> "RuntimeRail[ContentKey]":
    slots = [(i, f) for i, f in enumerate(fields) if isinstance(f, np.ndarray) and f.size]  # callables/carrier tuples never seed identity
    buffer = b"".join(np.ascontiguousarray(field).tobytes() for _, field in slots)
    # each surviving block's `Carried` SLOT INDEX and shape ride the fmt so a pure-inequality LP and a
    # pure-equality LP whose `A`/`b` blocks are byte-identical AND same-shaped still key DISTINCTLY: the
    # empty-block filter drops zero-size blocks, so the slot index is what distinguishes the `A_ub`/`b_ub`
    # role (positions 1/2) from the `A_eq`/`b_eq` role (positions 3/4) the bare concatenation would erase.
    shape_tag = "".join(f".{i}:{f.ndim}x{'x'.join(map(str, f.shape))}" for i, f in slots)
    # a seeded route folds its `rng` seed AND the `GlobalMethod` engine discriminant into the fmt so two
    # `differential_evolution` runs at distinct seeds, and a DE-vs-DIRECT solve on identical data and
    # seed, key DISTINCTLY (the iterate is both seed- and engine-dependent); the deterministic
    # LP/MILP/assignment routes pass `None` so a re-solve on identical data is a cache hit regardless of
    # the ignored seed argument (`_engine_tag` contributes only on the seeded stochastic route).
    seed_tag = f".{seed}{_engine_tag(intent)}" if seed is not None else ""
    return ContentIdentity.of(f"program.{intent.tag}{shape_tag}{seed_tag}", buffer)


def _engine_tag(intent: ProgramIntent) -> str:
    # the `Global` route's iterate is engine-dependent, so the chosen `GlobalMethod` (and the DE strategy,
    # which alters the mutation walk) folds into the content key; every other route returns "".
    match intent:
        case ProgramIntent(tag="stochastic", stochastic=(_, _, GlobalMethod(tag="de", de=(_, _, strategy)))):
            return f".de.{strategy}"
        case ProgramIntent(tag="stochastic", stochastic=(_, _, method)):
            return f".{method.tag}"
        case _:
            return ""


def _bounds(box: np.ndarray) -> "opt.Bounds | None":
    from scipy.optimize import Bounds

    pairs = box.reshape(-1, 2)
    return Bounds(pairs[:, 0], pairs[:, 1]) if pairs.size else None  # the one `Bounds` carrier linprog/milp/minimize all accept


def _violation(constraints: Constraints, x: np.ndarray) -> float:
    from scipy.optimize import LinearConstraint, NonlinearConstraint

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

    return float(max((residual(con) for con in constraints), default=0.0)) if x.size else float("inf")


# --- [COMPOSITION] -------------------------------------------------------------------------


def _entry_linear(fields: Carried, _: int) -> ProgramSolve:
    from scipy.optimize import linprog

    cost, ub_mat, ub_rhs, eq_mat, eq_rhs, box = fields
    return ProgramSolve(
        linprog(
            cost,
            A_ub=ub_mat if ub_rhs.size else None,
            b_ub=ub_rhs if ub_rhs.size else None,
            A_eq=eq_mat if eq_rhs.size else None,
            b_eq=eq_rhs if eq_rhs.size else None,
            bounds=_bounds(box),
            method="highs",
        ),
        None,
    )


def _entry_integer(fields: Carried, _: int) -> ProgramSolve:
    from scipy.optimize import milp

    cost, flags, box, constraints = fields
    return ProgramSolve(milp(cost, integrality=flags, bounds=_bounds(box), constraints=list(constraints) or None), None)


def _entry_stochastic(fields: Carried, seed: int) -> ProgramSolve:
    objective_fn, box, method = fields  # the `GlobalMethod` policy carried on the case dispatches the engine
    return ProgramSolve(method.solve(objective_fn, box, seed), None)


def _entry_constrained(fields: Carried, _: int) -> ProgramSolve:
    from scipy.optimize import minimize

    objective_fn, start, box, constraints = fields
    return ProgramSolve(minimize(objective_fn, start, method="trust-constr", bounds=_bounds(box), constraints=list(constraints)), None)


def _entry_assignment(fields: Carried, _: int) -> ProgramSolve:
    from scipy.optimize import linear_sum_assignment

    (matrix,) = fields
    return ProgramSolve(None, linear_sum_assignment(matrix))


def _iterate_host(_: Carried, outcome: ProgramSolve) -> tuple[np.ndarray, float]:
    return np.asarray(outcome.result.x, dtype=float), float(outcome.result.fun)  # read only past SUCCESS


def _iterate_assignment(fields: Carried, outcome: ProgramSolve) -> tuple[np.ndarray, float]:
    (matrix,) = fields
    rows, cols = outcome.assignment
    selected = matrix[rows, cols]
    return np.asarray(selected, dtype=float), float(selected.sum())


def _carriers_linear(fields: Carried, _: ProgramSolve) -> Constraints:
    from scipy.optimize import LinearConstraint

    _cost, ub_mat, ub_rhs, eq_mat, eq_rhs, _box = fields
    return (*(LinearConstraint(ub_mat, -np.inf, ub_rhs),) * bool(ub_rhs.size), *(LinearConstraint(eq_mat, eq_rhs, eq_rhs),) * bool(eq_rhs.size))


def _no_carriers(_: Carried, __: ProgramSolve) -> Constraints:
    return ()  # the global and assignment routes are unconstrained / feasible-by-construction


def _fwd_carriers(fields: Carried, _: ProgramSolve) -> Constraints:
    *_, constraints = fields  # integer/constrained forward their trailing caller-carrier tuple; starred-unpack, never a `[-1]` index
    return constraints


def _project(intent: ProgramIntent) -> Carried:
    match intent:
        case ProgramIntent(tag="linear", linear=(c, a_ub, b_ub, a_eq, b_eq, bounds)):
            return (
                np.asarray(c, dtype=float),
                np.atleast_2d(np.asarray(a_ub, dtype=float)),
                np.asarray(b_ub, dtype=float),
                np.atleast_2d(np.asarray(a_eq, dtype=float)),
                np.asarray(b_eq, dtype=float),
                np.asarray(bounds, dtype=float),
            )
        case ProgramIntent(tag="integer", integer=(c, integrality, bounds, constraints)):
            return (np.asarray(c, dtype=float), np.asarray(integrality), np.asarray(bounds, dtype=float), constraints)
        case ProgramIntent(tag="stochastic", stochastic=(objective_fn, bounds, method)):
            return (objective_fn, np.asarray(bounds, dtype=float), method)
        case ProgramIntent(tag="constrained", constrained=(objective_fn, x0, bounds, constraints)):
            return (objective_fn, np.asarray(x0, dtype=float), np.asarray(bounds, dtype=float), constraints)
        case ProgramIntent(tag="assignment", assignment=cost):
            return (np.atleast_2d(np.asarray(cost, dtype=float)),)
        case _ as unreachable:
            assert_never(unreachable)


_PROGRAM_ROUTES: Map[str, ProgramRoute] = Map.of_seq([
    ("linear", ProgramRoute(_entry_linear, _iterate_host, _carriers_linear, Termination.CODED, False)),
    ("integer", ProgramRoute(_entry_integer, _iterate_host, _fwd_carriers, Termination.CODED, False)),
    ("stochastic", ProgramRoute(_entry_stochastic, _iterate_host, _no_carriers, Termination.FLAGGED, True)),
    ("constrained", ProgramRoute(_entry_constrained, _iterate_host, _fwd_carriers, Termination.FLAGGED, False)),
    ("assignment", ProgramRoute(_entry_assignment, _iterate_assignment, _no_carriers, Termination.FEASIBLE, False)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
