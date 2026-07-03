# [PY_COMPUTE_PROGRAM]

The constrained, global, and discrete counterpart of the gradient-driven design loop — the math-program reaches what the differentiable optimizer in `optimization/design.md#DESIGN` structurally cannot. `ProgramIntent` discriminates a linear program, a mixed-integer program, a derivative-free global minimum, a bounded constrained smooth minimum, and an optimal assignment over `scipy.optimize`. Every route folds the host termination verdict, the objective value, and the maximum constraint-violation residual into the `program` case of the shared content-keyed `OutcomeReceipt` (`optimization/design.md#DESIGN`); the `program` feasibility verdict and the sibling `design` first-order convergence verdict are two cases of one `@tagged_union`, never two structs.

The `program` case carries the `SolveStatus` `StrEnum` `solvers/receipt.md#RECEIPT` owns rather than a lone `bool success`. The `linprog`/`milp` `OptimizeResult.status` integer code and the `differential_evolution`/`dual_annealing`/`shgo`/`direct`/`minimize` `.success` flag fold into that one vocabulary through the `Termination` adjudicator, so the `CODED` integer path resolves an infeasible LP (`status == 2`), an unbounded LP (`status == 3`), and an iteration-limit MILP (`status == 1`) to distinct first-class verdicts the C# graduation gate reads, and the `FLAGGED` boolean path carries the success-versus-`STAGNATION` distinction the global-search and `trust-constr` engines surface, never a boolean collapsing every non-success cause to `False`. The success contract survives as the derived `status is SolveStatus.SUCCESS` predicate while the receipt also carries why a program did not solve.

The five routes are five `_PROGRAM_ROUTES` rows over one fold: each `ProgramRoute` carries the static `Termination` policy, the `seeded` flag, and the bound `scipy.optimize` `entry` callable plus the `iterate`/`carriers` projections as data, the per-tag buffer-and-key projection owned once by the `_project` `match` rather than re-carried as a constant route column. The result-shape adjudication is the `Termination` `StrEnum` carried per route — `CODED`, `FLAGGED`, `FEASIBLE` — reading a typed `OptimizeResult | None`, never a `coded` boolean knob over a bare-`object` result. The `entry` callable returns ONLY the raw carrier so a non-success `linprog` whose `result.x`/`result.fun` are `None` adjudicates to its typed verdict before the `iterate` reader ever touches the carrier, never a `float(None)` crash folded into a generic fault. The program data admits through `numerics/array.md#PAYLOAD` on the same `ContentIdentity` seed, and `_program_key` returns the railed `RuntimeRail[ContentKey]` the receipt fold threads through `Result.map`, never a bare key dropping the digest fault.

The derivative-free `Global` case carries the full catalogued global-search family as a `GlobalMethod` policy, not one hardcoded solver: `differential_evolution`, `dual_annealing`, `shgo`, and `direct` (scipy.md row `[12]`) are arms of one `GlobalMethod.solve(func, box, seed)` projection threading the SPEC-007 `rng` keyword, defaulted to `DE` through the `_DEFAULT_GLOBAL` anchor and overridable per call — the same engine-as-policy shape `design.md`'s `Descent` `@tagged_union` and `_DEFAULT_DESCENT` table own, so a differential-evolution population search, a simulated-annealing escape, a simplicial-homology global search, and a Lipschitz `DIRECT` partition are one owner discriminated by a policy value, never four parallel global-optimizer cases beside the LP/MILP solve. `DE` threads its advanced surface — `workers=-1` process-parallel population evaluation, `polish=True` L-BFGS-B refinement of the incumbent, and a `strategy` mutation policy — so the global route runs at its full power, never the single-feature `differential_evolution(func, bounds)` subset.

This owner carries no numpy floor: the math-program solve *is* `scipy.optimize`, so a runtime run without the scipy package returns `Error(Import)` rather than a degraded estimate — the deliberate floor asymmetry against `design.md` and `solvers/nonlinear.md#NONLINEAR`, mirroring the no-floor hull/Delaunay routes of `analysis/spatial.md#SPATIAL` where Qhull is the gated capability itself. The certified optimum graduates on the existing `solver` `HandoffAxis` case through `graduation/handoff.md#GRADUATION`, the discrete/constrained sibling of the differentiable design optimum on the one rail.

## [01]-[INDEX]

- [01]-[PROGRAM]: linear, integer, global, constrained, and assignment math programs over `scipy.optimize` driven by the one `_PROGRAM_ROUTES` data table (each row a static `termination`/`seeded` policy plus the bound `entry` callable and the `iterate` / `carriers` projections, the iterate read only past a `SUCCESS` adjudication, the buffer projection owned once by `_project`), the `Global` case threading the catalogued `differential_evolution`/`dual_annealing`/`shgo`/`direct` family as a `GlobalMethod` policy defaulted through `_DEFAULT_GLOBAL`, folding the `program` case of the shared content-keyed `OutcomeReceipt` on the one `ProgramIntent` owner.

## [02]-[PROGRAM]

- Owner: `ProgramIntent` — the math-program cases discriminated by constraint-and-integrality structure recoverable from the problem value itself, never a differentiable objective; `Linear(c, a_ub, b_ub, bounds, *, a_eq, b_eq)` over `scipy.optimize.linprog` on the HiGHS backend threading both the inequality `A_ub x ≤ b_ub` and the equality `A_eq x = b_eq` blocks (each block passed only when non-empty, so a pure-inequality, pure-equality, or mixed LP is one shape, never a parallel equality-LP owner), `Integer(c, integrality, bounds, constraints)` over `scipy.optimize.milp` threading the integrality vector and the `Bounds` box, `Global(objective, bounds, *, method)` (the `stochastic` case, the python-keyword-free tag for the derivative-free global search) carrying its `GlobalMethod` engine policy and dispatching it over `scipy.optimize.differential_evolution`/`dual_annealing`/`shgo`/`direct` seeded through the SPEC-007 `rng` keyword for a reproducible content-keyed solve, `Constrained(objective, x0, bounds, constraints)` over `scipy.optimize.minimize(method="trust-constr")` threading the `LinearConstraint`/`NonlinearConstraint` carriers **directly** (the documented `minimize` constraint-carrier route, never lowered to legacy `{"type": "ineq", "fun": ...}` dicts scipy already accepts), and `Assignment(cost)` over `scipy.optimize.linear_sum_assignment`. The discriminant is the program shape, so the gradient loop and the math-program loop are sibling cases on the one `optimization` sub-domain, never a duplicated optimizer surface beside the solve.
- Global engine: `GlobalMethod` is the `@tagged_union` global-search vocabulary — `DE` (`differential_evolution`, carrying the `(workers, polish, strategy)` advanced surface), `Annealing` (`dual_annealing`), `Simplicial` (`shgo`), and `Direct` (`direct`) — projected to its `scipy.optimize` entrypoint through one `GlobalMethod.solve(func, box, seed) -> OptimizeResult` total `match`/`assert_never`, the engine-as-policy union `design.md`'s `Descent` `@tagged_union` also holds. The policy rides the `Global` factory's keyword-only `method` parameter — not the `solve` entry the sibling `design.md` carries `descent` on — because `GlobalMethod` discriminates the ONE `stochastic` route while `Descent` spans all three design routes, so an engine knob on `program.solve` would be `None` for the LP/MILP/`trust-constr`/assignment routes it cannot reach; carrying it on the only case it affects is the denser placement, defaulted to `DE` through the `_DEFAULT_GLOBAL` anchor. The four derivative-free global optimizers are one owner discriminated by a policy value carried on the case, never four parallel `ProgramIntent` cases; `DE`'s `workers=-1`/`polish=True`/`strategy` cell rides on the case so a population search runs process-parallel and L-BFGS-B-polished at its full catalogued power. A new global solver is one `GlobalMethod` case plus one `solve` arm, never a new `ProgramIntent` tag and never a new `_PROGRAM_ROUTES` row.
- Route table: each `ProgramRoute` `Struct` carries the static `Termination` policy, the `seeded` flag, the bound `entry` callable (`(Carried, int) -> ProgramSolve`, closing over the route's `scipy.optimize` entrypoint and returning the RAW carrier), and two orthogonal projections — `iterate` (`(Carried, ProgramSolve) -> (np.ndarray, float)`, reading the optimal iterate and objective off the carrier read ONLY past a `SUCCESS` adjudication) and `carriers` (`(Carried, ProgramSolve) -> Constraints`, reifying the `_violation` fold inputs). `Carried` is the precise five-arm union of the route payload shapes (`_project` projects each `ProgramIntent` case into its arm), never an erased `tuple[object, ...]` a closure indexes by magic position; `Constraints` is the one named `LinearConstraint`/`NonlinearConstraint` carrier-tuple alias the `_violation` fold and the `carriers` reifiers share. The buffer-and-key projection is NOT a route column: `_project` is the one `match` owning every per-tag `Carried` shape, so the route table never re-carries `_project` as a constant fifth field the way a closure-quad would. The single `_program_receipt` body resolves the row, projects buffers through `_project`, evaluates `entry`, adjudicates the row's `termination`, reads `iterate` on `SUCCESS` (else folds `inf`), reifies `carriers`, and reduces `_violation`, so a new route is one row plus one `_project` arm, never an arm in a five-way solve fold. `ProgramSolve` carries ONLY the raw `OptimizeResult | None` and the assignment `tuple | None` — never a derived `objective`/`x`, since reading `result.fun`/`result.x` eagerly inside the `entry` body would coerce the `None` an infeasible `linprog` returns into a `float(None)` crash captured as a generic `boundary` fault rather than the typed `INFEASIBLE` verdict the page advertises; the lazy `iterate` read past adjudication is what keeps the feasibility verdict first-class.
- Termination: `Termination` is the ONE result-shape adjudicator carried per route as a static `StrEnum` field on `ProgramRoute`, never reconstructed inside an `entry` body, never a three-`None`-case union, nor the prior `_program_status(result, *, coded)` boolean knob. `CODED` reads the `linprog`/`milp` integer `OptimizeResult.status` (`0` optimal, `1` iteration/time limit, `2` infeasible, `3` unbounded, `4` `linprog`-numerical / `milp`-other, both folding `OTHER` since neither is the matrix-conditioning `conlim` verdict) through the `_PROGRAM_STATUS` boundary table; `FLAGGED` reads the boolean `OptimizeResult.success` every global-search engine (`differential_evolution`/`dual_annealing`/`shgo`/`direct`) and `minimize(method="trust-constr")` surface; `FEASIBLE` folds `SolveStatus.SUCCESS` for the `linear_sum_assignment` route optimal by construction. `Termination.adjudicate(result)` is one total `match`/`assert_never` whose leading `FEASIBLE` arm folds `SUCCESS` over the `None` assignment carrier and whose `case _ if result is None` guard narrows the declared `OptimizeResult | None` to `OTHER` before the `CODED`/`FLAGGED` arms read `.status`/`.success`, so each field is named on the catalogued result type with the `| None` narrowed away rather than accessed off a phantom bare `object` — and the match dispatches on `self` so it never names the `TYPE_CHECKING`-only `opt.OptimizeResult` as a runtime class pattern. A new termination-source shape is one `Termination` member plus one `adjudicate` arm plus the row's `termination` cell.
- Entry: `solve(intent, *, seed)` enters one `boundary(f"program.{intent.tag}", ...)` and `.bind`-flattens the railed `_program_receipt`, so the scipy solve fence and the `RuntimeRail[ContentKey]` digest rail join on one `RuntimeRail[OutcomeReceipt]` without double-wrapping. The body projects the `Carried` buffers through `_project`, evaluates the row's `entry` into a `ProgramSolve` carrying ONLY the host `OptimizeResult | None` and the `linear_sum_assignment` row/column `tuple | None`, adjudicates the row's static `termination` into the `SolveStatus` verdict, reads the `(x, objective)` pair through the route's `iterate` projection ONLY on a `SUCCESS` adjudication (else `(_EMPTY_1D, inf)` so the `None` carrier is never coerced), and `.map`s the railed `_program_key` into `OutcomeReceipt.Program`. The residual reduces through the ONE `_violation` fold over the reified carriers against that iterate — the `Linear` route reifies its `(A_ub, b_ub)` inequality block as `LinearConstraint(A_ub, -inf, b_ub)` and its `(A_eq, b_eq)` block as `LinearConstraint(A_eq, b_eq, b_eq)`, the `Integer`/`Constrained` routes forward their caller carriers, and the `Global`/`Assignment` routes carry none — so every violation flows through one typed `max(0, lb − Ax, Ax − ub)` reduction (a `match` on the carrier class, never a `hasattr(con, "A")` reflective probe) and is `inf` where the iterate is the empty non-success sentinel.
- Receipt: this owner mints only the `OutcomeReceipt.Program(program_tag, objective, status, violation, key)` factory case; the receipt projection is the shared `OutcomeReceipt` owner's one `_OUTCOME_SLOTS`-driven `.facts` zip and `contribute` fold on `optimization/design.md#DESIGN`, never a program-specific `contribute` body. That shared fold emits one `Receipt.of(f"compute.optimization.{self.tag}", ("emitted", self.tag, facts))` row carrying the program tag, the objective value, the `SolveStatus` termination verdict, the derived `converged` flag, the constraint violation, and the content key through `ContentKey.hex`, the facts riding as native scalars the `enc_hook=repr` renderer serializes without a coerce. The solve fence and the receipt egress stay orthogonal exactly as on `design.md`/`convex.md`: `boundary` mints the rail, the shared `OutcomeReceipt` owns its `contribute` projection, and the consumer drives egress through `Signals.emit` — never an inline `Signals.emit` threaded through the `solve` body and never a `@receipted` decoration on the `RuntimeRail`-returning `solve` where the aspect wraps a `ReceiptContributor`-returning kernel. A certified optimum (`status is SolveStatus.SUCCESS` with a within-tolerance violation) graduates through `graduation/handoff.md#GRADUATION` on the existing `solver` `HandoffAxis` case — no new literal, no graduation edit — so an infeasible or unbounded program is an admission rejection carrying its termination reason rather than a bare `False`.
- Packages: `scipy` (`optimize.linprog` threading `A_ub`/`b_ub`/`A_eq`/`b_eq`, `optimize.milp`, the global-search family `optimize.differential_evolution`/`optimize.dual_annealing`/`optimize.shgo`/`optimize.direct` (scipy.md row `[12]`, all four the `GlobalMethod` arms) with the `differential_evolution` `rng`/`workers`/`polish`/`strategy` advanced keywords beyond the row-`[12]` 2-argument entry settling under the reflection pass, `optimize.minimize` on `method="trust-constr"` consuming the constraint carriers directly, `optimize.linear_sum_assignment`, `optimize.Bounds`, `optimize.LinearConstraint`, `optimize.NonlinearConstraint`, `optimize.OptimizeResult` — the `.status` integer diagnostic for `linprog`/`milp` and the `.success`/`.fun`/`.x` fields all catalogued in `compute/.api/scipy.md`'s `scipy.optimize` entrypoint and public-type tables, imported under `TYPE_CHECKING` so `ProgramSolve.result`, `Termination.adjudicate`, `GlobalMethod.solve`, and the `_violation` `residual` annotate the real `OptimizeResult`/constraint carriers rather than a bare `object` while the gated package never imports at runtime, entrypoints staying boundary-scoped per the manifest import policy), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `empty`, `maximum`, `inf` — the canonical problem-data buffer, the empty-block sentinels, and the constraint-violation max-reduction), `expression` (`tag`/`case`/`tagged_union` — the `ProgramIntent` and `GlobalMethod` discriminated unions; `Result.bind`/`Result.map` — the rail join flattening `boundary` over the railed key and the `OutcomeReceipt.Program` mapping), `msgspec` (`Struct` — the frozen `ProgramRoute` row and `ProgramSolve` result carrier, both GC-tracked rather than `gc=False` because each holds container/closure fields (`ProgramRoute` the entry/iterate/carriers closures, `ProgramSolve` the host `OptimizeResult` and assignment tuple), the closure-carrying sibling of the convex `ConeRow`/`ConeKKT`), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, bounds, and integrality admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `solvers/receipt.md#RECEIPT` (`SolveStatus` — the ONE bounded termination vocabulary the host `OptimizeResult` verdict folds into, the same `StrEnum` every solver route adjudicates), `optimization/design.md#DESIGN` (`OutcomeReceipt` — the shared optimization-outcome receipt this owner folds its `program` case into, carrying the `Receipt`/`ReceiptContributor` contribution), runtime (`RuntimeRail`/`boundary` the solve fence and rail carrier, `ContentIdentity.of` the railed key over an `IdentityPolicy`, `ContentKey` the resolved key).
- Growth: a new math-program route is one `ProgramIntent` case, one `Carried` payload arm, one `_PROGRAM_ROUTES` row, and one `_project` arm folding the shared `OutcomeReceipt.Program`; a new derivative-free global solver is one `GlobalMethod` case plus one `GlobalMethod.solve` arm on the existing `Global` route, never a new `ProgramIntent` tag; a new constraint block is one reified `LinearConstraint`/`NonlinearConstraint` carrier the row's `carriers` reifier emits into the existing `_violation` fold; a new termination-source shape is one `Termination` member plus one `adjudicate` arm; a new host termination code is one `_PROGRAM_STATUS` row mapping the `OptimizeResult.status` integer into the existing `SolveStatus` vocabulary. Zero new surface: never a per-program owner, never a parallel linear-program-and-assignment owner, never a parallel global-optimizer case beside the `GlobalMethod` policy, never a per-route `_*_receipt` body, never a per-arm solve fold parallel to the route table, never a per-shape violation probe parallel to the carrier fold, never a second optimization-outcome receipt struct beside `OutcomeReceipt`.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.optimization.design import OutcomeReceipt
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary

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
                return _PROGRAM_STATUS.get(int(result.status), SolveStatus.OTHER)
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

# `0/1/2/3` agree across the `linprog` and `milp` exit-code tables; code `4` diverges — `linprog`
# "numerical difficulties", `milp` "other" — and neither is the matrix-conditioning `conlim` verdict
# `solvers/receipt.md#RECEIPT` reserves `ILL_CONDITIONED` for, so both fold the honest `OTHER`.
_PROGRAM_STATUS: FrozenDict[int, SolveStatus] = FrozenDict({
    0: SolveStatus.SUCCESS,
    1: SolveStatus.MAX_STEPS,
    2: SolveStatus.INFEASIBLE,
    3: SolveStatus.UNBOUNDED,
    4: SolveStatus.OTHER,
})


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


def solve(intent: ProgramIntent, *, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    # `boundary` fences the scipy solve (raising routes, the gated `ImportError`); the railed
    # `ContentIdentity.of` key threads through `.bind` so a digest fault propagates on the one rail
    # rather than collapsing to a phantom bare `ContentKey`.
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
    return ContentIdentity.of(f"program.{intent.tag}{shape_tag}{seed_tag}", buffer, IdentityPolicy())


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


_PROGRAM_ROUTES: FrozenDict[str, ProgramRoute] = FrozenDict({
    "linear": ProgramRoute(_entry_linear, _iterate_host, _carriers_linear, Termination.CODED, False),
    "integer": ProgramRoute(_entry_integer, _iterate_host, _fwd_carriers, Termination.CODED, False),
    "stochastic": ProgramRoute(_entry_stochastic, _iterate_host, _no_carriers, Termination.FLAGGED, True),
    "constrained": ProgramRoute(_entry_constrained, _iterate_host, _fwd_carriers, Termination.FLAGGED, False),
    "assignment": ProgramRoute(_entry_assignment, _iterate_assignment, _no_carriers, Termination.FEASIBLE, False),
})
```

## [03]-[RESEARCH]

- [PROGRAM_STATUS]: `Termination.adjudicate(result)` folds the host `OptimizeResult` termination into the `SolveStatus` `StrEnum` `solvers/receipt.md#RECEIPT` owns through one total `match`/`assert_never` over a typed `OptimizeResult | None`. `CODED` maps the `linprog`/`milp` integer `.status` through `_PROGRAM_STATUS` (`0`→`SUCCESS`, `1`→`MAX_STEPS`, `2`→`INFEASIBLE`, `3`→`UNBOUNDED`, `4`→`OTHER` because the divergent code-`4` exits — `linprog` "numerical difficulties", `milp` "other; see message" — are neither the matrix-conditioning `conlim` verdict `ILL_CONDITIONED` reserves nor a feasibility class), reading the two feasibility-verdict members `solvers/receipt.md#RECEIPT` carries for exactly this discrete/constrained case rather than collapsing an infeasible LP onto an iterative `BREAKDOWN` or an unbounded LP onto a `DIVERGENCE`; `FLAGGED` maps the boolean `.success` flag every global-search engine (`differential_evolution`/`dual_annealing`/`shgo`/`direct`) and `minimize` surface through `SUCCESS`/`STAGNATION`; `FEASIBLE` folds `SUCCESS` for the assignment route. The `Termination` `StrEnum` is a static `ProgramRoute.termination` cell adjudicated by `_program_receipt`, never reconstructed inside a `solve` body — and never the prior `_program_status(result: object, *, coded: bool)` whose boolean knob branched the body and whose bare-`object` parameter read a phantom `.status`/`.success`, and never a three-`None`-case union spending a tag declaration on a member-free dispatch a `StrEnum` carries denser. The `program` case of `OutcomeReceipt` carries this `SolveStatus`, the success contract surviving as the derived `status is SolveStatus.SUCCESS` predicate. The `program` tuple is `(str, float, SolveStatus, float, ContentKey)` carrying the status in the third slot the prior `bool` held.
- [PROGRAM_VIOLATION]: `_violation` reduces the maximum constraint residual through one typed `match` over the scipy carrier classes — a `LinearConstraint()` arm contracting `con.A @ x`, a `NonlinearConstraint()` arm evaluating `con.fun(x)`, both folding the `max(0, lb − Ax, Ax − ub)` excess through `np.maximum`, never a `hasattr(con, "A")` reflective probe. Every route feeds the fold through its `ProgramRoute.carriers` reifier: the `linear` row reifies its `(A_ub, b_ub)`/`(A_eq, b_eq)` blocks into `LinearConstraint` carriers, the `integer`/`constrained` rows forward their caller carrier tuple, and the `stochastic`/`assignment` rows carry none — so a route's violation inputs are a declarative cell on its row, and an infeasible solve where `x.size` is zero folds to `inf`.
- [PROGRAM_CONTENT_KEY]: `_program_key` derives the railed `RuntimeRail[ContentKey]` over the canonical contiguous problem-data buffer (the concatenated `tobytes()` of the cost vector, the inequality and equality constraint blocks, integrality, or cost matrix — non-array `Carried` slots and empty blocks contribute nothing through the `isinstance(f, np.ndarray) and f.size` guard) through `ContentIdentity.of(fmt, buffer, IdentityPolicy())`, whose `view="value"` default returns `RuntimeRail[ContentKey]`. Because the empty-block filter drops zero-size blocks from the buffer, the per-slot populated-shape signature rides the `fmt` (`.{ndim}x{shape}` per surviving array) so a pure-inequality LP and a pure-equality LP whose `A`/`b` blocks are byte-identical key DISTINCTLY rather than colliding on the bare concatenation that erases each block's role and shape. The `fmt` discriminant additionally folds the `rng` seed plus the `_engine_tag` `GlobalMethod` discriminant (`f"program.{tag}{shape}.{seed}.{engine}"`) for a `ProgramRoute.seeded` route and stays seed-free (`f"program.{tag}{shape}"`) otherwise: the `Global` route's iterate is BOTH seed- and engine-dependent, so two `differential_evolution` runs at distinct seeds, and a DE-vs-`dual_annealing`-vs-`direct` solve on identical data and seed, MUST key distinctly or a cache hit returns the wrong global iterate — `_engine_tag` folds the engine tag and the DE `strategy` (which alters the mutation walk) into the seed suffix and returns `""` off the deterministic LP/MILP/`trust-constr`/assignment routes that ignore both arguments, so a re-solve on identical data is a cache hit regardless. The fold threads that rail through `Result.map` into `OutcomeReceipt.Program` rather than treating the result as a bare `ContentKey`, so a digest fault propagates on the one rail and `solve` joins it under `.bind`. A program whose data admits through `numerics/array.md#PAYLOAD` keys under the same `ContentIdentity` seed algebra; the objective callable and the `GlobalMethod` policy ride the `Carried` projection but never seed the data buffer (the engine folds into `fmt`, not the bytes), and the seeded global cache key reproduces the same global iterate per `(data, seed, engine)` triple.
