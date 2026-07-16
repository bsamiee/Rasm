# [PY_COMPUTE_DESIGN]

`DesignProblem` is the gradient-driven inverse-design apex the autodifferentiable solver chain enables and no other owner closes: a `Field` objective over the `solvers/mesh.md#MESH_FIELD` assembled system, a parametric-`Mesh` objective, and a material-distribution `Density` objective, each driven to a stationary point through `optimistix.minimise`/`least_squares` over the Equinox-partitioned JAX floor. Every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so the gradient `solvers/sensitivity.md#SENSITIVITY` pulls back is the implicit-function-theorem gradient of the converged solution, never the iteration trace. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

`OutcomeReceipt` is the one optimization-outcome owner this page and `optimization/program.md#PROGRAM` share — the `design` convergence verdict and the `program` feasibility verdict are two cases of one union, both carrying the `SolveStatus` vocabulary `solvers/receipt.md#RECEIPT` owns — while `ConvexReceipt` stays distinct because the KKT certificate has no field here. A numpy central-difference floor reports the gradient-norm residual behind the `_backend` import guard, so a run without the jaxlib package never returns `Error(Import)`; the converged design graduates on the existing `solver` axis at `graduation/handoff.md#GRADUATION`.

## [01]-[INDEX]

- [01]-[DESIGN]: field/mesh/density inverse-design through one shape-keyed `optimistix` dispatch with the implicit-adjoint gradient, folding the `design` case of the shared `OutcomeReceipt` on one `DesignProblem` owner.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the provenance of the objective is the discriminant and the optimizer is one surface; `carried` folds the case to its `Objective` total over `match`/`assert_never`, so a new provenance breaks the extractor rather than spawning a parallel dispatch arm.
- Cases: `Objective` owns TWO shape-keyed projections of one `fn` because the solver and the receipt consume different reductions — `target` feeds `least_squares` the raw residual VECTOR (a pre-reduced `½‖r‖²` scalar collapses the LM Jacobian to a degenerate 1-element solve) while `cost` folds the `(reduced, reported)` receipt pair as the value-and-grad aux, never a re-traced second pass; `Descent.admits` gates an engine override — `Levenberg` requires the `RESIDUAL` route, the scalar minimisers require `SCALAR` — as a typed `Error(BoundaryFault)` on the rail before the wrong solve entry; the `FirstOrder` chain leads `optax.zero_nans()` before `clip_by_global_norm` because a NaN gradient from a diverged inner solve is not boundable by a clip.
- Entry: the solve runs `throw=False` so a non-`successful` `Solution.result` reaches the receipt as its mapped `SolveStatus` rather than raising; `_design_key` folds each leaf's ordinal and shape plus the iterate-determining `descent`/`restarts`/`seed` policy, so structurally distinct PyTrees or a re-solve under a different engine never collide on the boundary-erasing flatten; the x64-gated descent pins the PROCESS lane with the module-level `_solve_kernel` crossing as spec data — no closure crosses the process lane.
- Receipt: `_OUTCOME_SLOTS` owns the case payloads so `.facts` is one total strict zip — a slot row that drifts from its payload raises rather than truncating evidence, and never a reflective `getattr(self, self.tag)` whose `object` residual makes the `assert_never` tail a lie; the verdict folds through the receipt-owned shared `status_of`/`verdict` folds, never a page-local `RESULTS` inversion.
- Packages: `RESULTS.promote` is deliberately unused — it widens a member across `Enumeration` classes and raises on a same-class member, so the multi-start reduction is the `jnp.max` code fold; the numpy floor runs over real arrays only, never a JAX PyTree, and its one-hot perturbation never materializes a dense `np.eye(x0.size)` basis a realistic SIMP density field cannot afford; the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here.
- Growth: a new provenance is one `DesignProblem` case plus one `_DEFAULT_DESCENT` row; a new objective shape is one `Shape` member plus its `_objective()`/`target`/`cost`/`_floor_cost` arms, all `assert_never`-closed; a new descent engine is one `Descent` case mapping to its constructor in `Descent.solver`; a new feasibility constraint is one `Feasible` member plus one `_feasible()` row; a new evidence field is one `_OUTCOME_SLOTS` slot plus its case-tuple position with no `contribute` edit; a multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`.

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

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.receipt import SolveStatus, status_of, verdict
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.receipts import Receipt

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
# by its row, so a case's evidence is one row, never a per-case hand-spelled fact dict.
_OUTCOME_SLOTS: Map[str, tuple[str, ...]] = Map.of_seq([
    ("design", ("problem", "objective", "residual", "iterations", "status", "key")),
    ("program", ("program", "objective", "status", "violation", "key")),
])

# --- [MODELS] ---------------------------------------------------------------------------


class Objective(Struct, frozen=True):
    fn: "Callable[[PyTree], PyTree]"  # the raw cost thunk over the design PyTree; container-holding, so GC-tracked
    params: "PyTree"
    shape: Shape = Shape.SCALAR

    def target(self, y: "PyTree") -> "jax.Array":
        # the SOLVE input: `least_squares` owns the ½‖r‖² reduction and the Jᵀr Jacobian internally, so `RESIDUAL` feeds the raw vector.
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
        # the RECEIPT projection: the differentiated reduction plus the reported scalar as the value-and-grad aux — ∇(½‖r‖²) = Jᵀr
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
    program: tuple[str, float, SolveStatus, float, ContentKey] = case()

    @classmethod
    def Design(cls, problem: str, objective: float, residual: float, iterations: int, status: SolveStatus, content_key: ContentKey) -> Self:
        return cls(design=(problem, objective, residual, iterations, status, content_key))

    @classmethod
    def Program(cls, program: str, objective: float, status: SolveStatus, violation: float, content_key: ContentKey) -> Self:
        return cls(program=(program, objective, status, violation, content_key))

    @property
    def status(self) -> SolveStatus:
        match self:
            case OutcomeReceipt(tag="design", design=(*_, status, _)):
                return status
            case OutcomeReceipt(tag="program", program=(_, _, status, _, _)):
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
    def facts(self) -> "dict[str, str | float | int | SolveStatus]":
        # `key` lowers to `ContentKey.hex` at the source so the projection carries only renderer-native scalars.
        match self:
            case OutcomeReceipt(tag="design", design=(*lead, key)):
                return dict(zip(_OUTCOME_SLOTS["design"], (*lead, key.hex), strict=True))
            case OutcomeReceipt(tag="program", program=(*lead, key)):
                return dict(zip(_OUTCOME_SLOTS["program"], (*lead, key.hex), strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {"converged": self.converged, **self.facts}
        return (Receipt.of(f"compute.optimization.{self.tag}", ("emitted", self.tag, facts)),)


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

_DEFAULT_DESCENT: Map[str, Descent] = Map.of_seq([("field", Descent.QuasiNewton()), ("mesh", Descent.Levenberg()), ("density", Descent.FirstOrder(feasible=Feasible.BOX))])


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


async def solve(problem: "DesignProblem", lane: LanePolicy, /, *, descent: "Descent | None" = None, restarts: int = 1, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    chosen = descent if descent is not None else _DEFAULT_DESCENT[problem.tag]

    async def dispatch() -> "RuntimeRail[OutcomeReceipt]":
        # worker death rides `retry=RetryClass.OCCT` on the isolation leg only.
        return (await lane.offload(_solve_kernel, problem, chosen, restarts, seed, modality=Modality.PROCESS, retry=RetryClass.OCCT)).bind(
            lambda rail: rail
        )

    return await evidence_run(EvidenceScope.DESIGN, f"design.{problem.tag}", dispatch)


def _solve_kernel(problem: "DesignProblem", chosen: "Descent", restarts: int, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    return boundary(
        f"design.{problem.tag}",
        lambda: _backend(problem, chosen, restarts, seed) if chosen.admits(problem.carried.shape) else _mismatch(problem, chosen),
    ).bind(lambda r: r)


def _mismatch(problem: "DesignProblem", descent: "Descent") -> "RuntimeRail[OutcomeReceipt]":
    detail = f"{descent.tag} does not admit {problem.tag} objective shape {problem.carried.shape}"
    return Error(BoundaryFault(boundary=(f"design.{problem.tag}", detail)))


def _backend(problem: "DesignProblem", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    # the railed digest threads into the deferred receipt builder through `Result.map`, so a digest fault rides the one rail.
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
        # the ensemble verdict folds the batched codes by `jnp.max`: `successful = 0`, so `max == 0` iff EVERY start converged —
        # a partial-failure ensemble surfaces a non-success code rather than masking a diverged start as `SUCCESS`.
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])

    # the converged objective and the L∞ stationarity residual fold from one value-and-grad-with-aux pass; the residual norm rides
    # `optx.max_norm` directly over the gradient PyTree, never a `numpy.asarray` detour.
    (_, reported), gradient = eqx.filter_value_and_grad(objective.cost, has_aux=True)(converged)
    objective_value, residual = float(reported), float(optx.max_norm(gradient))
    status = status_of(verdict(jnp, optx.RESULTS, solution.result), residual, _TOL)
    return lambda key: OutcomeReceipt.Design(tag, objective_value, residual, steps, status, key)


def _floor(tag: str, objective: "Objective") -> "Callable[[ContentKey], OutcomeReceipt]":
    # the floor ravels the general design PyTree through `_ravel` — never `np.asarray(params)`, which silently stacks a tuple of
    # equal-shaped leaves into one wrong-rank array and crashes on a ragged PyTree — and restores structure via the captured `unravel`.
    x0, unravel = _ravel(objective.params)
    cost, reported = _floor_cost(objective, unravel)
    residual = _central_difference_norm(cost, x0)
    status = status_of(None, residual, _TOL)  # the no-adjudicator floor: `status_of` grades NONFINITE/SUCCESS/STAGNATION
    return lambda key: OutcomeReceipt.Design(tag, reported(x0), residual, 0, status, key)


def _floor_cost(
    objective: "Objective", unravel: "Callable[[np.ndarray], PyTree]"
) -> "tuple[Callable[[np.ndarray], float], Callable[[np.ndarray], float]]":
    # the host mirror of `Objective.cost`; `.item()` squeezes a singleton array where a bare `float(value)` crashes on non-0-d output,
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
    # the pure-numpy host mirror of `jax.flatten_util.ravel_pytree` (which pulls the gated jaxlib package): leaves concatenate in
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
    # the gated import defers to first call; the lookup resolves only inside the `_optimistix` route the import guard fences.
    import optimistix as optx

    return Map.of_seq([(Shape.SCALAR, optx.minimise), (Shape.RESIDUAL, optx.least_squares)])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
