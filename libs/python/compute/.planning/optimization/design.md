# [PY_COMPUTE_DESIGN]

`DesignProblem` is the gradient-driven inverse-design apex built on the autodifferentiable solver chain and closed by no other owner: a `Field` objective over the `solvers/mesh#MESH_FIELD` assembled system, a parametric-`Mesh` objective, and a material-distribution `Density` objective, each driven to a stationary point through `optimistix.minimise`/`least_squares` over the Equinox-partitioned JAX floor. Every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so the gradient `solvers/sensitivity#SENSITIVITY` pulls back is the implicit-function-theorem gradient of the converged solution, never the iteration trace. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

`Optimum` is the one optimization-outcome owner this page and `optimization/program#PROGRAM` share — the `design` convergence verdict and the `program` feasibility verdict are two cases of one union, both carrying the `SolveStatus` vocabulary `solvers/solve#SOLVE` owns — while `ConvexOptimum` stays distinct because the KKT certificate has no field here. A numpy central-difference floor reports the gradient-norm residual behind an import guard scoped to the `DesignEngine.gated()` dereference ALONE, so a run without the jaxlib package never returns `Error(Import)` while an `ImportError` out of the gated solve itself stays the defect it is; the converged design graduates on the existing `solver` axis through `Optimum.graduates`, the shared projection clearing each case's numeric facts against its `_OUTCOME_CEILING` row.

## [01]-[INDEX]

- [02]-[DESIGN]: field/mesh/density inverse-design through one shape-keyed `optimistix` dispatch with the implicit-adjoint gradient, folding the `design` case of the shared `Optimum` on one `DesignProblem` owner.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the provenance of the objective is the discriminant and the optimizer is one surface; `carried` folds the case to its `Objective` total over `match`/`assert_never`, so a new provenance breaks the extractor rather than spawning a parallel dispatch arm.
- Cases: `Objective` owns TWO shape-keyed projections of one `fn` because the solver and the `Optimum` consume different reductions — `target` feeds `least_squares` the raw residual VECTOR (a pre-reduced `½‖r‖²` scalar collapses the LM Jacobian to a degenerate 1-element solve) while `cost` folds the `(reduced, reported)` pair as the value-and-grad aux, never a re-traced second pass; `Descent.admits` gates an engine override — `Levenberg` requires the `RESIDUAL` route, the scalar minimisers require `SCALAR` — as a typed `Error(BoundaryFault)` on the rail before the wrong solve entry; the `FirstOrder` chain leads `optax.zero_nans()` before `clip_by_global_norm` because a NaN gradient from a diverged inner solve is not boundable by a clip.
- Law: the `program` case carries a stability band and a certificate size the retained-solver backend alone fills and the facade leaves absent — the settled per-case optional-slot precedent, where the `xla` case alone carries its `TraceEvidence` band — so a backend's extra evidence lands as slots on the one shared `Optimum` rather than a second owner beside it. Absence is the honest state and the ledger drops it: an unmeasured slot never floats, so the hub's key-coverage gate refuses a crossing whose ceiling names a quantity that backend never measured. The `program` objective and violation follow the same rule on a refusal, while a `design` solve's non-finite objective is a MEASURED non-finite value and still rails at the hub's finiteness admission — a measurement that came out non-finite and a measurement nobody took are two states, and only the second spells absence.
- Entry: the solve runs `throw=False` so a non-`successful` `Solution.result` reaches the `Optimum` as its mapped `SolveStatus` rather than raising; `_design_key` folds each leaf's ordinal and shape with the iterate-determining `descent`/`restarts`/`seed` policy, so structurally distinct PyTrees or a re-solve under a different engine never collide on the boundary-erasing flatten; the x64-gated descent declares the HOSTILE trait because `DesignEngine.gated()` mutates process-global x64 state, with the module-level `_solve_kernel` crossing by reference, a closure shipping by value at the crossing owner.
- Output: `Optimum` retains the optimized design pytree or program decision beside its measured facts; `.facts` skips that typed product and feeds telemetry and graduation scalars only. Both factories close through `_noted`, and `graduates` clears the measured numeric facts against the case's `_OUTCOME_CEILING` row.
- Packages: `RESULTS.promote` is deliberately unused — it widens a member across `Enumeration` classes and raises on a same-class member, so the multi-start reduction is the `jnp.max` code fold; the numpy floor runs over real arrays only, never a JAX PyTree, and its one-hot perturbation never materializes a dense `np.eye(x0.size)` basis a realistic SIMP density field cannot afford; the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here.
- Growth: a new provenance is one `DesignProblem` case and one `_DEFAULT_DESCENT` row; a new objective shape is one `Shape` member with its `_objective`/`target`/`cost`/`_floor_cost` arms, all `assert_never`-closed; a new descent engine is one `Descent` case mapping to its constructor in `Descent.solver`; a new feasibility constraint is one `Feasible` member and one `_feasible` row; a new gated module is one `DesignEngine` field and one `gated()` import line; a new evidence field is one `_OUTCOME_SLOTS` slot with its case-tuple position and no `attributes` edit; the answering engine is the `Provider` column the `Solve` owner seats, never a page-local vocabulary, a backend-specific one landing optional so every other backend leaves it absent; a new outcome case is one `_OUTCOME_SLOTS`, `_OUTCOME_CEILING`, and `_OUTCOME_SCOPE` row; a tighter graduation bar is one `_OUTCOME_CEILING` row; a multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import functools
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import numpy as np
from expression import Error, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from opentelemetry import trace

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, Graduation, evidence_run
from rasm.compute.solvers.solve import Provider, SolveStatus, graduate, status_of, verdict
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

if TYPE_CHECKING:
    import jax
    import optax
    import optimistix as optx

    type PyTree = jax.Array | dict[str, "PyTree"] | list["PyTree"] | tuple["PyTree", ...]
    type DesignSolver = optx.AbstractMinimiser | optx.AbstractLeastSquaresSolver
    type DesignEntry = Callable[..., optx.Solution]

# --- [TYPES] ----------------------------------------------------------------------------


class Shape(StrEnum):
    SCALAR = "scalar"
    RESIDUAL = "residual"


class Feasible(StrEnum):
    FREE = "free"
    BOX = "box"
    SIMPLEX = "simplex"
    NONNEGATIVE = "nonnegative"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-2
_CLIP: float = 1e3
_FD: float = 1e-6
_SEED: int = 0
_JITTER: float = 1e-2
_MAX_STEPS: int = 256

_OUTCOME_SLOTS: Map[str, tuple[str, ...]] = Map.of_seq([
    ("design", ("problem", "objective", "residual", "iterations", "provider", "status", "key")),
    ("program", ("program", "objective", "status", "violation", "fragility", "witness", "key")),
])

_OUTCOME_CEILING: Map[str, dict[str, float]] = Map.of_seq([("design", {"residual": _TOL}), ("program", {"violation": 0.0})])

_OUTCOME_SCOPE: Map[str, EvidenceScope] = Map.of_seq([("design", EvidenceScope.DESIGN), ("program", EvidenceScope.PROGRAM)])

# --- [MODELS] ---------------------------------------------------------------------------


class Objective(Struct, frozen=True):
    fn: "Callable[[PyTree], PyTree]"
    params: "PyTree"
    shape: Shape = Shape.SCALAR

    def target(self, engine: "DesignEngine", y: "PyTree") -> "jax.Array":
        value = engine.jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                return value
            case Shape.SCALAR:
                return value.reshape(())
            case _ as unreachable:
                assert_never(unreachable)

    def cost(self, engine: "DesignEngine", y: "PyTree") -> "tuple[jax.Array, jax.Array]":
        value = engine.jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                norm = engine.jnp.linalg.norm(value)
                return 0.5 * (value**2).sum(), norm
            case Shape.SCALAR:
                scalar = value.reshape(())
                return scalar, scalar
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Optimum:
    tag: Literal["design", "program"] = tag()
    design: "tuple[PyTree, str, float, float, int, Provider, SolveStatus, ContentKey]" = case()
    program: tuple[np.ndarray | tuple[np.ndarray, np.ndarray] | None, str, float | None, SolveStatus, float | None, float | None, int | None, ContentKey] = case()

    @classmethod
    def Design(
        cls, value: "PyTree", problem: str, objective: float, residual: float, iterations: int, provider: Provider, status: SolveStatus,
        content_key: ContentKey,
    ) -> Self:
        return cls(design=(value, problem, objective, residual, iterations, provider, status, content_key))._noted()

    @classmethod
    def Program(
        cls,
        value: np.ndarray | tuple[np.ndarray, np.ndarray] | None,
        program: str,
        objective: float | None,
        status: SolveStatus,
        violation: float | None,
        content_key: ContentKey,
        *,
        fragility: float | None = None,
        witness: int | None = None,
    ) -> Self:
        return cls(program=(value, program, objective, status, violation, fragility, witness, content_key))._noted()

    @property
    def status(self) -> SolveStatus:
        match self:
            case Optimum(tag="design", design=(*_, status, _)):
                return status
            case Optimum(tag="program", program=(_, _, _, status, *_)):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    @property
    def content_key(self) -> ContentKey:
        match self:
            case Optimum(tag="design", design=(*_, key)) | Optimum(tag="program", program=(*_, key)):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def facts(self) -> "dict[str, str | float | int | None | SolveStatus]":
        match self:
            case Optimum(tag="design", design=(_, *lead, key)) | Optimum(tag="program", program=(_, *lead, key)):
                return dict(zip(_OUTCOME_SLOTS[self.tag], (*lead, key.hex), strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def value(self) -> "PyTree | np.ndarray | tuple[np.ndarray, np.ndarray] | None":
        match self:
            case Optimum(tag="design", design=(value, *_)) | Optimum(tag="program", program=(value, *_)):
                return value
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        scalars = {name: value for name, value in self.facts.items() if isinstance(value, str | bool | int | float)}
        return {"outcome": self.tag, "converged": self.converged, **scalars}

    def _noted(self) -> Self:
        trace.get_current_span().set_attributes(self.attributes)
        return self

    def graduates(self, ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[Graduation]":
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

    def solver(self, engine: "DesignEngine") -> "DesignSolver":
        optx, optax = engine.optx, engine.optax
        match self:
            case Descent(tag="quasi_newton"):
                return optx.BestSoFarMinimiser(optx.BFGS(rtol=_TOL, atol=_TOL))
            case Descent(tag="levenberg"):
                return optx.BestSoFarLeastSquares(optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL))
            case Descent(tag="first_order", first_order=(learning_rate, feasible)):
                chain = optax.chain(optax.zero_nans(), optax.clip_by_global_norm(_CLIP), optax.adam(learning_rate), *_feasible(engine)[feasible])
                return optx.BestSoFarMinimiser(optx.OptaxMinimiser(chain, rtol=_TOL, atol=_TOL))
            case _ as unreachable:
                assert_never(unreachable)

    def admits(self, shape: Shape) -> bool:
        match self:
            case Descent(tag="levenberg"):
                return shape is Shape.RESIDUAL
            case _:
                return shape is Shape.SCALAR


@dataclass(frozen=True, slots=True)
class DesignEngine:
    jax: object
    jnp: object
    eqx: object
    optx: object
    optax: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)
        import equinox as eqx
        import jax.numpy as jnp
        import optax
        import optimistix as optx

        return cls(jax=jax, jnp=jnp, eqx=eqx, optx=optx, optax=optax)


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_DESCENT: Map[str, Descent] = Map.of_seq([
    ("field", Descent.QuasiNewton()),
    ("mesh", Descent.Levenberg()),
    ("density", Descent.FirstOrder(feasible=Feasible.BOX)),
])


def _projected(engine: "DesignEngine", projection: "Callable[[PyTree], PyTree]") -> "optax.GradientTransformationExtraArgs":
    optax = engine.optax

    def init(_: "PyTree") -> "optax.EmptyState":
        return optax.EmptyState()

    def update(updates: "PyTree", state: "optax.OptState", params: "PyTree | None" = None, **_: object) -> "tuple[PyTree, optax.OptState]":
        candidate = optax.apply_updates(params, updates)
        corrected = optax.tree_utils.tree_add(projection(candidate), optax.tree_utils.tree_scale(-1.0, params))
        return corrected, state

    return optax.GradientTransformationExtraArgs(init, update)


def _feasible(engine: "DesignEngine") -> "Map[Feasible, tuple[optax.GradientTransformation, ...]]":
    optax = engine.optax
    return Map.of_seq([
        (Feasible.FREE, ()),
        (Feasible.BOX, (_projected(engine, functools.partial(optax.projections.projection_box, lower=0.0, upper=1.0)),)),
        (Feasible.SIMPLEX, (_projected(engine, optax.projections.projection_simplex),)),
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
) -> "RuntimeRail[Optimum]":
    chosen = descent if descent is not None else _DEFAULT_DESCENT[problem.tag]

    async def dispatch() -> "RuntimeRail[Optimum]":
        return (await lane.offload(Kernel.of(_solve_kernel, KernelTrait.HOSTILE), problem, chosen, restarts, seed)).bind(
            lambda rail: rail
        )

    facts = {"problem": problem.tag, "descent": chosen.tag, "restarts": restarts}
    return await evidence_run(EvidenceScope.DESIGN, f"design.{problem.tag}", dispatch, facts=facts, composition=composition)


DESIGN_SOLVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.DESIGN, point="solve", arm="boundary", defect="solve-refused", retriability=TERMINAL
)
DESIGN_SHAPE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.DESIGN, point="descent", arm="config", defect="shape-mismatch", retriability=TERMINAL,
    slots=("descent", "problem", "shape"),
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([DESIGN_SOLVE, DESIGN_SHAPE]))


def _solve_kernel(problem: "DesignProblem", chosen: "Descent", restarts: int, seed: int) -> "RuntimeRail[Optimum]":
    return boundary(
        DESIGN_SOLVE,
        lambda: _backend(problem, chosen, restarts, seed) if chosen.admits(problem.carried.shape) else _mismatch(problem, chosen),
        catch=(np.linalg.LinAlgError, ValueError, TypeError, RuntimeError),
    ).bind(lambda r: r)


def _mismatch(problem: "DesignProblem", descent: "Descent") -> "RuntimeRail[Optimum]":
    return Error(DESIGN_SHAPE.raised(descent.tag, problem.tag, str(problem.carried.shape)))


def _backend(problem: "DesignProblem", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[Optimum]":
    objective = problem.carried
    railed = _backend_outcome(problem.tag, objective, descent, restarts, seed)
    return _design_key(problem.tag, objective.params, descent, restarts, seed).map(railed)


def _backend_outcome(tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int) -> "Callable[[ContentKey], Optimum]":
    try:
        engine = DesignEngine.gated()
    except ImportError:
        return _floor(tag, objective)
    return _optimistix(engine, tag, objective, descent, restarts, seed)


def _optimistix(
    engine: "DesignEngine", tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int
) -> "Callable[[ContentKey], Optimum]":
    eqx, jnp, optx = engine.eqx, engine.jnp, engine.optx
    design, static = eqx.partition(objective.params, eqx.is_inexact_array)
    op = _objective(engine)[objective.shape]
    solver = descent.solver(engine)

    @eqx.filter_jit
    def fn(y: "PyTree", _: object) -> "jax.Array":
        return objective.target(engine, eqx.combine(y, static))

    def run(y0: "PyTree") -> "optx.Solution":
        return op(fn, solver, y0, adjoint=optx.ImplicitAdjoint(), max_steps=_MAX_STEPS, throw=False)

    if restarts > 1:
        keys = engine.jax.random.split(engine.jax.random.key(seed), restarts)
        starts = eqx.filter_vmap(
            lambda k: engine.jax.tree_util.tree_map(lambda leaf: leaf + _JITTER * engine.jax.random.normal(k, leaf.shape), design)
        )(keys)
        solution = eqx.filter_vmap(run)(starts)
        scored = eqx.filter_vmap(lambda v: objective.cost(engine, eqx.combine(v, static))[0])(solution.value)
        best = int(jnp.argmin(scored))
        converged = eqx.combine(engine.jax.tree_util.tree_map(lambda leaf: leaf[best], solution.value), static)
        steps = int(jnp.asarray(solution.stats["num_steps"])[best])
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])

    (_, reported), gradient = eqx.filter_value_and_grad(functools.partial(objective.cost, engine), has_aux=True)(converged)
    objective_value, residual = float(reported), float(optx.max_norm(gradient))
    status = status_of(verdict(jnp, optx.RESULTS, solution.result), residual, _TOL)
    return lambda key: Optimum.Design(converged, tag, objective_value, residual, steps, Provider.GATED, status, key)


def _floor(tag: str, objective: "Objective") -> "Callable[[ContentKey], Optimum]":
    x0, unravel = _ravel(objective.params)
    cost, reported = _floor_cost(objective, unravel)
    residual = _central_difference_norm(cost, x0)
    status = status_of(None, residual, _TOL)
    return lambda key: Optimum.Design(unravel(x0), tag, reported(x0), residual, 0, Provider.FLOOR, status, key)


def _floor_cost(
    objective: "Objective", unravel: "Callable[[np.ndarray], PyTree]"
) -> "tuple[Callable[[np.ndarray], float], Callable[[np.ndarray], float]]":
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
    def directional(i: int) -> float:
        e = np.zeros(x0.size, dtype=float)
        e[i] = _FD
        return (cost(x0 + e) - cost(x0 - e)) / (2.0 * _FD)

    probe = np.fromiter((directional(i) for i in range(x0.size)), dtype=float, count=x0.size)
    return float(np.linalg.norm(probe, np.inf))


def _ravel(params: "PyTree") -> "tuple[np.ndarray, Callable[[np.ndarray], PyTree]]":
    single = not isinstance(params, (tuple, list))
    leaves = [np.ascontiguousarray(np.asarray(leaf, dtype=float)) for leaf in ((params,) if single else params)]
    shapes = [leaf.shape for leaf in leaves]
    splits = np.cumsum([leaf.size for leaf in leaves])[:-1]

    def unravel(flat: np.ndarray) -> "PyTree":
        parts = [chunk.reshape(shape) for chunk, shape in zip(np.split(flat, splits), shapes, strict=True)]
        return parts[0] if single else type(params)(parts)

    return np.concatenate([leaf.ravel() for leaf in leaves]) if leaves else np.zeros(0), unravel


def _design_key(tag: str, params: "PyTree", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[ContentKey]":
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


def _objective(engine: "DesignEngine") -> "Map[Shape, DesignEntry]":
    return Map.of_seq([(Shape.SCALAR, engine.optx.minimise), (Shape.RESIDUAL, engine.optx.least_squares)])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
