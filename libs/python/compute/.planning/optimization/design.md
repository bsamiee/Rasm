# [PY_COMPUTE_DESIGN]

`DesignProblem` is the gradient-driven inverse-design apex built on the autodifferentiable solver chain and closed by no other owner: a `Field` objective over the `solvers/mesh#MESH_FIELD` assembled system, a parametric-`Mesh` objective, and a material-distribution `Density` objective, each driven to a stationary point through `optimistix.minimise`/`least_squares` over the Equinox-partitioned JAX floor. Every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so the gradient `solvers/sensitivity#SENSITIVITY` pulls back is the implicit-function-theorem gradient of the converged solution, never the iteration trace. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

`OutcomeReceipt` is the one optimization-outcome owner this page and `optimization/program#PROGRAM` share — the `design` convergence verdict and the `program` feasibility verdict are two cases of one union, both carrying the `SolveStatus` vocabulary `solvers/receipt#RECEIPT` owns — while `ConvexReceipt` stays distinct because the KKT certificate has no field here. A numpy central-difference floor reports the gradient-norm residual behind the `_backend` import guard, so a run without the jaxlib package never returns `Error(Import)`; the converged design graduates on the existing `solver` axis through `OutcomeReceipt.graduates`, the shared projection clearing each case's numeric facts against its `_OUTCOME_CEILING` row.

## [01]-[INDEX]

- [02]-[DESIGN]: field/mesh/density inverse-design through one shape-keyed `optimistix` dispatch with the implicit-adjoint gradient, folding the `design` case of the shared `OutcomeReceipt` on one `DesignProblem` owner.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the provenance of the objective is the discriminant and the optimizer is one surface; `carried` folds the case to its `Objective` total over `match`/`assert_never`, so a new provenance breaks the extractor rather than spawning a parallel dispatch arm.
- Cases: `Objective` owns TWO shape-keyed projections of one `fn` because the solver and the receipt consume different reductions — `target` feeds `least_squares` the raw residual VECTOR (a pre-reduced `½‖r‖²` scalar collapses the LM Jacobian to a degenerate 1-element solve) while `cost` folds the `(reduced, reported)` receipt pair as the value-and-grad aux, never a re-traced second pass; `Descent.admits` gates an engine override — `Levenberg` requires the `RESIDUAL` route, the scalar minimisers require `SCALAR` — as a typed `Error(BoundaryFault)` on the rail before the wrong solve entry; the `FirstOrder` chain leads `optax.zero_nans()` before `clip_by_global_norm` because a NaN gradient from a diverged inner solve is not boundable by a clip.
- Entry: the solve runs `throw=False` so a non-`successful` `Solution.result` reaches the receipt as its mapped `SolveStatus` rather than raising; `_design_key` folds each leaf's ordinal and shape with the iterate-determining `descent`/`restarts`/`seed` policy, so structurally distinct PyTrees or a re-solve under a different engine never collide on the boundary-erasing flatten; the x64-gated descent declares the HOSTILE trait with the module-level `_solve_kernel` crossing by reference, a closure shipping by value at the crossing owner.
- Receipt: `_OUTCOME_SLOTS` owns the case payloads so `.facts` is one total strict zip — a slot row that drifts from its payload raises rather than truncating evidence, and never a reflective `getattr(self, self.tag)` whose `object` residual makes the `assert_never` tail a lie; the verdict folds through the receipt-owned shared `status_of`/`verdict` folds, never a page-local `RESULTS` inversion. `graduates` is the one solver-axis crossing on the shared owner — the case's MEASURED numeric facts project as the ledger, `_OUTCOME_CEILING` supplies the governed default bar a caller's tighter row overrides, and the `_OUTCOME_SCOPE` row names the owner off the case tag rather than reconstructing a scope value the vocabulary owns.
- Optional slots: the `program` case carries a stability band and a certificate size the retained-solver backend alone fills and the facade leaves absent — the settled per-case optional-slot precedent, where the `xla` case alone carries its `TraceEvidence` band — so a backend's extra evidence lands as slots on the one shared receipt rather than a second receipt beside it. Absence is the honest state and the ledger drops it: an unmeasured slot never floats, so the hub's key-coverage gate refuses a crossing whose ceiling names a quantity that backend never measured. The `program` objective and violation follow the same rule on a refusal, while a `design` solve's non-finite objective is a MEASURED non-finite value and still rails at the hub's finiteness admission — a measurement that came out non-finite and a measurement nobody took are two states, and only the second spells absence.
- Packages: `RESULTS.promote` is deliberately unused — it widens a member across `Enumeration` classes and raises on a same-class member, so the multi-start reduction is the `jnp.max` code fold; the numpy floor runs over real arrays only, never a JAX PyTree, and its one-hot perturbation never materializes a dense `np.eye(x0.size)` basis a realistic SIMP density field cannot afford; the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here.
- Growth: a new provenance is one `DesignProblem` case and one `_DEFAULT_DESCENT` row; a new objective shape is one `Shape` member with its `_objective()`/`target`/`cost`/`_floor_cost` arms, all `assert_never`-closed; a new descent engine is one `Descent` case mapping to its constructor in `Descent.solver`; a new feasibility constraint is one `Feasible` member and one `_feasible()` row; a new evidence field is one `_OUTCOME_SLOTS` slot with its case-tuple position and no `contribute` edit, a backend-specific one landing optional so every other backend leaves it absent; a new outcome case is one `_OUTCOME_SLOTS`, `_OUTCOME_CEILING`, and `_OUTCOME_SCOPE` row; a tighter graduation bar is one `_OUTCOME_CEILING` row; a multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import functools
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, Self, assert_never

import numpy as np
from expression import Error, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, evidence_run
from rasm.compute.solvers.receipt import SolveStatus, graduate, status_of, verdict
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:  # worker annotation carriers only; no package imports at runtime
    import jax
    import optax
    import optimistix as optx

    type PyTree = jax.Array | dict[str, "PyTree"] | list["PyTree"] | tuple["PyTree", ...]
    type DesignSolver = optx.AbstractMinimiser | optx.AbstractLeastSquaresSolver
    type DesignEntry = Callable[..., optx.Solution]

# --- [TYPES] ----------------------------------------------------------------------------


class Shape(StrEnum):
    SCALAR = "scalar"  # minimise: fn(y) -> scalar
    RESIDUAL = "residual"  # least_squares: fn(y) -> residual vector, cost ½‖r‖²


class Feasible(StrEnum):
    FREE = "free"
    BOX = "box"  # densities ∈ [0, 1]
    SIMPLEX = "simplex"  # material-fraction simplex
    NONNEGATIVE = "nonnegative"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-2
_CLIP: float = 1e3  # global-norm step bound guarding a diverged inner solve
_FD: float = 1e-6
_SEED: int = 0
_JITTER: float = 1e-2
_MAX_STEPS: int = 256

# per-case payload field names, one tuple per `OutcomeReceipt` tag; the `.facts` strict zip packs each case's destructured payload
# by its row, so a case's evidence is one row, never a per-case hand-spelled fact dict. The `program` row's two trailing
# evidence slots are filled by the retained-solver backend alone and left absent by the facade, exactly as the `xla`
# case alone carries its `TraceEvidence` band — a per-case optional slot, never a second receipt beside this one.
_OUTCOME_SLOTS: Map[str, tuple[str, ...]] = Map.of_seq([
    ("design", ("problem", "objective", "residual", "iterations", "status", "key")),
    ("program", ("program", "objective", "status", "violation", "fragility", "witness", "key")),
])

# family DEFAULT graduation ceilings, one row per `OutcomeReceipt` tag beside the slot table; a caller's
# tighter row overrides at `graduates`. `objective` carries no bar — the ceiling fold checks only its own keys.
_OUTCOME_CEILING: Map[str, dict[str, float]] = Map.of_seq([("design", {"residual": _TOL}), ("program", {"violation": 0.0})])

# tag -> owning scope, beside the slot and ceiling rows. The enum member is the ONE handle: reconstructing a scope by
# feeding its VALUE back through `EvidenceScope(...)` re-spells a string the vocabulary already owns and breaks the
# moment its root moves, so the correspondence is a row the two consumers read.
_OUTCOME_SCOPE: Map[str, EvidenceScope] = Map.of_seq([("design", EvidenceScope.DESIGN), ("program", EvidenceScope.PROGRAM)])

# --- [MODELS] ---------------------------------------------------------------------------


class Objective(Struct, frozen=True):
    fn: "Callable[[PyTree], PyTree]"  # the raw cost thunk over the design PyTree; container-holding, so GC-tracked
    params: "PyTree"
    shape: Shape = Shape.SCALAR

    def target(self, y: "PyTree") -> "jax.Array":
        # SOLVE input: `least_squares` owns the ½‖r‖² reduction and the Jᵀr Jacobian internally, so `RESIDUAL` feeds the raw vector.
        import jax.numpy as jnp

        value = jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                return value
            case Shape.SCALAR:
                return value.reshape(())
            case _ as unreachable:
                assert_never(unreachable)

    def cost(self, y: "PyTree") -> "tuple[jax.Array, jax.Array]":
        # RECEIPT projection: the differentiated reduction plus the reported scalar as the value-and-grad aux — ∇(½‖r‖²) = Jᵀr
        # is the converged-design stationarity gradient on the residual route.
        import jax.numpy as jnp

        value = jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                norm = jnp.linalg.norm(value)
                return 0.5 * (value**2).sum(), norm
            case Shape.SCALAR:
                scalar = value.reshape(())
                return scalar, scalar
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class OutcomeReceipt:
    tag: Literal["design", "program"] = tag()
    design: tuple[str, float, float, int, SolveStatus, ContentKey] = case()
    program: tuple[str, float | None, SolveStatus, float | None, float | None, int | None, ContentKey] = case()

    @classmethod
    def Design(cls, problem: str, objective: float, residual: float, iterations: int, status: SolveStatus, content_key: ContentKey) -> Self:
        return cls(design=(problem, objective, residual, iterations, status, content_key))

    @classmethod
    def Program(
        cls,
        program: str,
        objective: float | None,
        status: SolveStatus,
        violation: float | None,
        content_key: ContentKey,
        *,
        fragility: float | None = None,
        witness: int | None = None,
    ) -> Self:
        # objective and violation are MEASURED on a converged program alone and absent on every refusal, so a rejected
        # crossing leaves the hub's key-coverage gate to refuse it rather than an `inf` breaching the finiteness
        # refinement one fence earlier. `fragility` and `witness` are the retained backend's evidence, absent under the
        # facade — the diagnosis a bare `INFEASIBLE` and a bare optimum each carry no field for.
        return cls(program=(program, objective, status, violation, fragility, witness, content_key))

    @property
    def status(self) -> SolveStatus:
        match self:
            case OutcomeReceipt(tag="design", design=(*_, status, _)):
                return status
            case OutcomeReceipt(tag="program", program=(_, _, status, *_)):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    @property
    def content_key(self) -> ContentKey:
        match self:
            case OutcomeReceipt(tag="design", design=(*_, key)) | OutcomeReceipt(tag="program", program=(*_, key)):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def facts(self) -> "dict[str, str | float | int | None | SolveStatus]":
        # `key` lowers to `ContentKey.hex` at the source so the projection carries only renderer-native scalars; an
        # absent slot rides as `None` and the ledger projection drops it, so an unmeasured quantity is never floated.
        match self:
            case OutcomeReceipt(tag="design", design=(*lead, key)) | OutcomeReceipt(tag="program", program=(*lead, key)):
                return dict(zip(_OUTCOME_SLOTS[self.tag], (*lead, key.hex), strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        # owner spelling resolves through the `_OUTCOME_SCOPE` row off the case tag, so the shared union never mints a
        # third spelling and never reconstructs a scope value the vocabulary already owns.
        facts: dict[str, object] = {"converged": self.converged, **self.facts}
        return (Receipt.of(_OUTCOME_SCOPE[self.tag].value, ("emitted", self.tag, facts)),)

    def graduates(self, ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[GraduationReceipt]":
        # ONE solver-axis crossing for both cases: measured numeric facts project as the ledger — an absent slot is
        # excluded, so the hub's key-coverage gate refuses a crossing whose ceiling names a quantity the solve never
        # took — the `_OUTCOME_CEILING` tag row is the governed default bar, and the leading slot (problem/program
        # name) is the subject.
        facts = self.facts
        ledger = {name: float(value) for name, value in facts.items() if isinstance(value, (int, float))}
        bar = ceiling if ceiling is not None else _OUTCOME_CEILING[self.tag]
        return graduate(
            _OUTCOME_SCOPE[self.tag].value, str(facts[_OUTCOME_SLOTS[self.tag][0]]), self.content_key, ledger, bar, composition=composition
        )


@tagged_union(frozen=True)
class Descent:
    tag: Literal["quasi_newton", "levenberg", "first_order"] = tag()
    quasi_newton: None = case()
    levenberg: None = case()
    first_order: tuple[float, Feasible] = case()

    @classmethod
    def QuasiNewton(cls) -> Self:
        return cls(quasi_newton=None)

    @classmethod
    def Levenberg(cls) -> Self:
        return cls(levenberg=None)

    @classmethod
    def FirstOrder(cls, learning_rate: float = _LR, feasible: Feasible = Feasible.FREE) -> Self:
        return cls(first_order=(learning_rate, feasible))

    def solver(self) -> "DesignSolver":
        # wrapped in the class-matching `BestSoFar*` guard so a non-monotone final iterate never poisons the converged design; the
        # `first_order` arm folds only chain-shaped `GradientTransformation`s — never a bare projection callable a `chain` cannot compose.
        import optax
        import optimistix as optx

        match self:
            case Descent(tag="quasi_newton"):
                return optx.BestSoFarMinimiser(optx.BFGS(rtol=_TOL, atol=_TOL))
            case Descent(tag="levenberg"):
                return optx.BestSoFarLeastSquares(optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL))
            case Descent(tag="first_order", first_order=(learning_rate, feasible)):
                # zero_nans -> clip -> adam -> feasibility projection: guard order is load-bearing, the projection is the chain tail.
                chain = optax.chain(optax.zero_nans(), optax.clip_by_global_norm(_CLIP), optax.adam(learning_rate), *_feasible()[feasible])
                return optx.BestSoFarMinimiser(optx.OptaxMinimiser(chain, rtol=_TOL, atol=_TOL))
            case _ as unreachable:
                assert_never(unreachable)

    def admits(self, shape: Shape) -> bool:
        match self:
            case Descent(tag="levenberg"):
                return shape is Shape.RESIDUAL
            case _:
                return shape is Shape.SCALAR


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_DESCENT: Map[str, Descent] = Map.of_seq([
    ("field", Descent.QuasiNewton()),
    ("mesh", Descent.Levenberg()),
    ("density", Descent.FirstOrder(feasible=Feasible.BOX)),
])


def _projected(projection: "Callable[[PyTree], PyTree]") -> "optax.GradientTransformationExtraArgs":
    # `update` returns the corrected delta `projection(params + updates) - params`, so the next iterate lands on the feasible set
    # INSIDE the chain — an `OptaxMinimiser` solve has no seam for a post-`apply_updates` body call.
    import optax

    def init(_: "PyTree") -> "optax.EmptyState":
        return optax.EmptyState()

    def update(updates: "PyTree", state: "optax.OptState", params: "PyTree | None" = None, **_: object) -> "tuple[PyTree, optax.OptState]":
        candidate = optax.apply_updates(params, updates)
        corrected = optax.tree_utils.tree_add(projection(candidate), optax.tree_utils.tree_scale(-1.0, params))
        return corrected, state

    return optax.GradientTransformationExtraArgs(init, update)


@functools.cache
def _feasible() -> "Map[Feasible, tuple[optax.GradientTransformation, ...]]":
    # `@functools.cache` defers the gated optax import to the first `Descent.solver` call, so the `_floor` path stays reachable;
    # `BOX`/`SIMPLEX` lift their projections through `_projected`, `NONNEGATIVE` folds the stateful catalogued transform, `FREE` is ().
    import optax

    return Map.of_seq([
        (Feasible.FREE, ()),
        (Feasible.BOX, (_projected(functools.partial(optax.projections.projection_box, lower=0.0, upper=1.0)),)),
        (Feasible.SIMPLEX, (_projected(optax.projections.projection_simplex),)),
        (Feasible.NONNEGATIVE, (optax.keep_params_nonnegative(),)),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


async def solve(
    problem: "DesignProblem",
    lane: LanePolicy,
    /,
    *,
    descent: "Descent | None" = None,
    restarts: int = 1,
    seed: int = _SEED,
    composition: ScopeKey = DEFAULT_SCOPE,
) -> "RuntimeRail[OutcomeReceipt]":
    chosen = descent if descent is not None else _DEFAULT_DESCENT[problem.tag]

    async def dispatch() -> "RuntimeRail[OutcomeReceipt]":
        # worker death rides the HOSTILE trait row on the isolation leg only.
        return (await lane.offload(Kernel.of(_solve_kernel, KernelTrait.HOSTILE), problem, chosen, restarts, seed)).bind(
            lambda rail: rail
        )

    facts = {"problem": problem.tag, "descent": chosen.tag, "restarts": restarts}
    return await evidence_run(EvidenceScope.DESIGN, f"design.{problem.tag}", dispatch, facts=facts, composition=composition)


def _solve_kernel(problem: "DesignProblem", chosen: "Descent", restarts: int, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    return boundary(
        f"design.{problem.tag}",
        lambda: _backend(problem, chosen, restarts, seed) if chosen.admits(problem.carried.shape) else _mismatch(problem, chosen),
    ).bind(lambda r: r)


def _mismatch(problem: "DesignProblem", descent: "Descent") -> "RuntimeRail[OutcomeReceipt]":
    detail = f"{descent.tag} does not admit {problem.tag} objective shape {problem.carried.shape}"
    return Error(BoundaryFault(boundary=(f"design.{problem.tag}", detail)))


def _backend(problem: "DesignProblem", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    # railed digest threads into the deferred receipt builder through `Result.map`, so a digest fault rides the one rail.
    objective = problem.carried
    railed = _backend_outcome(problem.tag, objective, descent, restarts, seed)
    return _design_key(problem.tag, objective.params, descent, restarts, seed).map(railed)


def _backend_outcome(tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int) -> "Callable[[ContentKey], OutcomeReceipt]":
    try:
        return _optimistix(tag, objective, descent, restarts, seed)
    except ImportError:
        return _floor(tag, objective)


def _optimistix(tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int) -> "Callable[[ContentKey], OutcomeReceipt]":
    import equinox as eqx
    import jax
    import jax.numpy as jnp
    import optimistix as optx

    design, static = eqx.partition(objective.params, eqx.is_inexact_array)
    op = _objective()[objective.shape]
    solver = descent.solver()

    @eqx.filter_jit
    def fn(y: "PyTree", _: object) -> "jax.Array":
        # never `cost(...)[0]` — the pre-reduced ½‖r‖² scalar degenerates the LM least-squares Jacobian.
        return objective.target(eqx.combine(y, static))

    def run(y0: "PyTree") -> "optx.Solution":
        return op(fn, solver, y0, adjoint=optx.ImplicitAdjoint(), max_steps=_MAX_STEPS, throw=False)

    if restarts > 1:
        keys = jax.random.split(jax.random.key(seed), restarts)
        starts = eqx.filter_vmap(lambda k: jax.tree_util.tree_map(lambda leaf: leaf + _JITTER * jax.random.normal(k, leaf.shape), design))(keys)
        solution = eqx.filter_vmap(run)(starts)
        scored = eqx.filter_vmap(lambda v: objective.cost(eqx.combine(v, static))[0])(solution.value)
        best = int(jnp.argmin(scored))
        converged = eqx.combine(jax.tree_util.tree_map(lambda leaf: leaf[best], solution.value), static)
        steps = int(jnp.asarray(solution.stats["num_steps"])[best])
        # ensemble verdict folds the batched codes by `jnp.max`: `successful = 0`, so `max == 0` iff EVERY start converged —
        # a partial-failure ensemble surfaces a non-success code rather than masking a diverged start as `SUCCESS`.
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])

    # converged objective and the L∞ stationarity residual fold from one value-and-grad-with-aux pass; the residual norm rides
    # `optx.max_norm` directly over the gradient PyTree, never a `numpy.asarray` detour.
    (_, reported), gradient = eqx.filter_value_and_grad(objective.cost, has_aux=True)(converged)
    objective_value, residual = float(reported), float(optx.max_norm(gradient))
    status = status_of(verdict(jnp, optx.RESULTS, solution.result), residual, _TOL)
    return lambda key: OutcomeReceipt.Design(tag, objective_value, residual, steps, status, key)


def _floor(tag: str, objective: "Objective") -> "Callable[[ContentKey], OutcomeReceipt]":
    # floor ravels the general design PyTree through `_ravel` — never `np.asarray(params)`, which silently stacks a tuple of
    # equal-shaped leaves into one wrong-rank array and crashes on a ragged PyTree — and restores structure via the captured `unravel`.
    x0, unravel = _ravel(objective.params)
    cost, reported = _floor_cost(objective, unravel)
    residual = _central_difference_norm(cost, x0)
    status = status_of(None, residual, _TOL)  # the no-adjudicator floor: `status_of` grades NONFINITE/SUCCESS/STAGNATION
    return lambda key: OutcomeReceipt.Design(tag, reported(x0), residual, 0, status, key)


def _floor_cost(
    objective: "Objective", unravel: "Callable[[np.ndarray], PyTree]"
) -> "tuple[Callable[[np.ndarray], float], Callable[[np.ndarray], float]]":
    # host mirror of `Objective.cost`; `.item()` squeezes a singleton array where a bare `float(value)` crashes on non-0-d output,
    # and `raw` runs the flat probe buffer back through `unravel` so `objective.fn` receives the structured design it is typed over.
    def raw(flat: np.ndarray) -> np.ndarray:
        return np.asarray(objective.fn(unravel(flat)), dtype=float)

    match objective.shape:
        case Shape.RESIDUAL:
            return lambda flat: 0.5 * float((raw(flat) ** 2).sum()), lambda flat: float(np.linalg.norm(raw(flat)))
        case Shape.SCALAR:

            def scalar(flat: np.ndarray) -> float:
                return float(raw(flat).reshape(()).item())

            return scalar, scalar
        case _ as unreachable:
            assert_never(unreachable)


def _central_difference_norm(cost: "Callable[[np.ndarray], float]", x0: np.ndarray) -> float:
    # one-hot perturbations, never a materialized dense `np.eye(x0.size)` basis whose O(n²) allocation a realistic SIMP density
    # field cannot afford; `‖·‖∞` is the same Chebyshev stationarity norm the gated `max_norm` reads.
    def directional(i: int) -> float:
        e = np.zeros(x0.size, dtype=float)
        e[i] = _FD
        return (cost(x0 + e) - cost(x0 - e)) / (2.0 * _FD)

    probe = np.fromiter((directional(i) for i in range(x0.size)), dtype=float, count=x0.size)
    return float(np.linalg.norm(probe, np.inf))


def _ravel(params: "PyTree") -> "tuple[np.ndarray, Callable[[np.ndarray], PyTree]]":
    # pure-numpy host mirror of `jax.flatten_util.ravel_pytree` (which pulls the gated jaxlib package): leaves concatenate in
    # deterministic structure order — the SAME order `_design_key` keys over — and `unravel` rebuilds the original container.
    single = not isinstance(params, (tuple, list))
    leaves = [np.ascontiguousarray(np.asarray(leaf, dtype=float)) for leaf in ((params,) if single else params)]
    shapes = [leaf.shape for leaf in leaves]
    splits = np.cumsum([leaf.size for leaf in leaves])[:-1]

    def unravel(flat: np.ndarray) -> "PyTree":
        parts = [chunk.reshape(shape) for chunk, shape in zip(np.split(flat, splits), shapes, strict=True)]
        return parts[0] if single else type(params)(parts)

    return np.concatenate([leaf.ravel() for leaf in leaves]) if leaves else np.zeros(0), unravel


def _design_key(tag: str, params: "PyTree", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[ContentKey]":
    # `_ravel` concatenates leaves with no boundary delimiter, so a `(4,)` array and a tuple of two `(2,)` leaves flatten byte-identically;
    # folding each leaf's ordinal and shape into the fmt distinguishes the structure, and the `descent`/`restarts`/`seed` policy folds
    # beside it because the converged design depends on all three — a cache hit must never return the wrong converged design.
    leaves = [np.asarray(leaf, dtype=float) for leaf in ((params,) if not isinstance(params, (tuple, list)) else params)]
    shape_tag = "".join(f".{i}:{leaf.ndim}x{'x'.join(map(str, leaf.shape))}" for i, leaf in enumerate(leaves))
    policy_tag = f".{descent.tag}.r{restarts}" + (f".s{seed}" if restarts > 1 else "")
    buffer = _ravel(params)[0].tobytes()
    return ContentIdentity.of(f"design-{tag}{shape_tag}{policy_tag}", buffer)


# --- [COMPOSITION] ----------------------------------------------------------------------


@tagged_union(frozen=True)
class DesignProblem:
    tag: Literal["field", "mesh", "density"] = tag()
    field: Objective = case()
    mesh: Objective = case()
    density: Objective = case()

    @classmethod
    def Field(cls, objective: Objective) -> Self:
        return cls(field=objective)

    @classmethod
    def Mesh(cls, objective: Objective) -> Self:
        return cls(mesh=objective)

    @classmethod
    def Density(cls, objective: Objective) -> Self:
        return cls(density=objective)

    @property
    def carried(self) -> Objective:
        match self:
            case DesignProblem(tag="field", field=obj):
                return obj
            case DesignProblem(tag="mesh", mesh=obj):
                return obj
            case DesignProblem(tag="density", density=obj):
                return obj
            case _ as unreachable:
                assert_never(unreachable)


@functools.cache
def _objective() -> "Map[Shape, DesignEntry]":
    # gated import defers to first call; the lookup resolves only inside the `_optimistix` route the import guard fences.
    import optimistix as optx

    return Map.of_seq([(Shape.SCALAR, optx.minimise), (Shape.RESIDUAL, optx.least_squares)])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
