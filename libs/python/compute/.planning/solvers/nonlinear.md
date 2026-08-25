# [PY_COMPUTE_NONLINEAR]

Nonlinear routes of the one numeric solver: `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over `optimistix` on the JAX floor, all four sharing one table-driven dispatch, a numpy central-difference floor reachable per route when the package is absent, and one `Solve` fold carrying the `Provider` that answered. Algorithm is never the entry point — each route carries a `NonlinearSolver` policy value resolved through the one `_SOLVER` profile table, and five orthogonal tuning axes ride one `SolverPolicy` value rather than per-solver `(rtol, atol)` literals.

One rail composes `optimistix` over a `lineax` inner linear solve and an `optax`-lifted descent on `equinox.filter_jit`/`filter_vmap` pytree transforms, under an `ImplicitAdjoint` (`RecursiveCheckpointAdjoint` for the ill-posed case) that `solvers/sensitivity#SENSITIVITY` differentiates through. Frozen `NonlinearEngine` folds the seven gated JAX modules so the solver build, route entry, adjoint, stationarity probe, and pytree read are carrier methods over one populated handle — a domain-named carrier, never a `SolveEngine` colliding with the `solvers/differential#DIFFERENTIAL` integration carrier. Its gated body floats the rail to float64 before the solve — the x64 contract the sibling JAX routes share — and loop-kernel/XLA acceleration is owned by `numerics/jit#JIT`.

## [01]-[INDEX]

- [02]-[NONLINEAR]: the four nonlinear routes over Optimistix with a numpy central-difference floor, every solver a `NonlinearSolver` policy row on the profile-driven builder, the gated JAX modules folded into one `NonlinearEngine` carrier.

## [02]-[NONLINEAR]

- Owner: `NonlinearSolver` is the one bounded solver vocabulary across every route; `_SOLVER` maps each member to a `_SolverSpec(attr, profile)` and `NonlinearEngine.build_solver` assembles the optimistix constructor keywords once by `SolverProfile`, so a new solver adds no construction body. Single resolved `norm` threads into both the `build_solver` termination and the route cell, so the termination norm and the `Solve` residual are one callable by construction. `_route_cells` keys one `(entry, residual contraction)` cell per route and `build_solver` one solver per member — never a tag×solver matrix. `best_so_far` wraps the converged solver in the route-matched `BestSoFar*` guard, one aspect over any solver.
- Cases: `least_squares` upcasts a minimiser member and `root_find`/`fixed_point` accept an upcast least-squares or minimiser solver where the problem class permits, per the `optimistix` entries. Batched path reduces the per-row `RESULTS` to the single worst-case termination member through `NonlinearEngine.verdict` — `jnp.max` over the per-row codes, the zero `successful` making `max == 0` iff every start converged — never `RESULTS.promote` (inheritance-widening that raises on a same-class member), the same multi-start resolution `optimization/design#DESIGN` runs.
- Entry: `NonlinearIntent.solve` composes `lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self, key)` under the `evidence_run` weave, the key naming the solved system so the `Solve` settles carrying its own coordinate and `graduates` restates none; the family declares `HOSTILE` because the x64 flag is process-global native state, and isolation, band, and worker-death retry derive at the runtime `Kernel` crossing owner from the trait row. Route forwards `max_steps`, the adjoint mode, and the profile-gated `options` under `throw=False`, so `solvers/sensitivity#SENSITIVITY` differentiates through the converged solution rather than the iteration trace and a non-`successful` verdict is recorded rather than raised.
- Output: each route returns its solved pytree in `Solve` beside residual, step count, provider, and the true `Solution.result` verdict; least-squares also carries rank. The numpy floor returns its admitted iterate with the measured probe rather than a metadata-only substitute. Every `Solve` factory stamps only scalar attributes on the weave span.
- Packages: `optimistix` (the solver, entry, and norm surface), `lineax` (the `InnerSolver`-projected `linear_solver=` family spanning tag-dispatched, direct, and iterative solvers), `optax` (the first-order transformations `OptaxMinimiser` lifts into minimise), `equinox` (`filter_jit`/`filter_vmap`), `jax`/`numpy` per the fence imports; `expression`/`msgspec` own the `NonlinearIntent` union and the value objects. `solvers/solve#SOLVE` owns `Solve` and the shared `verdict` fold (`SolveStatus` folds inside the `Solve` factories, never imported here); the hub `evidence_run` weave and the runtime offload crossing (`Kernel`/`KernelTrait`) compose silently.
- Growth: a new refusal is one `NonlinearFault` case with one `__str__` arm, its kwargs crossing the lane whole on the `BoundaryFault.domain` case; a new route is one `NonlinearIntent` case and one `_route_cells` cell; a new solver is one `NonlinearSolver` member and one `_SOLVER` row naming its `SolverProfile`; a new constructor surface is one `SolverProfile` member and one `build_solver` arm; a new termination norm, inner linear solver, or adjoint mode is one enum member and one arm or row on the matching carrier. A first-order step change is one `SolverPolicy.learning_rate` value, a 1-D bracketing solve one `NonlinearPolicy.bracket`, a multi-start study one `NonlinearPolicy.batched` vmapped through the same `solve` — never a second entry, never a per-route helper or emit.
- Boundary: the `TOL_ONLY` bracket is the per-solve entry argument (`options=dict(lower=, upper=)`), not a constructor kwarg like the five `SolverPolicy` axes, and rides `NonlinearPolicy` beside `max_steps`/`adjoint`/`has_aux`; a `TOL_ONLY` solve with an absent, non-finite, or unordered bracket raises `NonlinearFault.bracket`, gated in `_dispatch` before the import fork so the gated path and the numpy floor refuse the misconfiguration identically. The numpy central-difference floor is reachable per route when `optimistix` is absent, narrows to `np.ndarray` at its jaxlib-free edge, and REFUSES the two requests it cannot take — a bracketing `TOL_ONLY` search and a `batched` multi-start sweep, neither of which one central-difference probe performs.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from jaxtyping import Array, Float, PyTree
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, Graduation, evidence_run
from rasm.compute.solvers.solve import Provider, Solve, graduate, verdict
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type Pytree = PyTree[Float[Array, "..."]]
type Scalar = Pytree
type ResidualFn = Callable[[Pytree], Pytree]
type ObjectiveFn = Callable[[Pytree], Scalar]
type Route = Literal["root_find", "minimise", "fixed_point", "least_squares"]
type InnerPick = Callable[["SolverPolicy", object], object]


class NonlinearSolver(StrEnum):
    NEWTON = "newton"
    CHORD = "chord"
    BISECTION = "bisection"
    GOLDEN_SEARCH = "golden_search"
    LBFGS = "lbfgs"
    BFGS = "bfgs"
    DFP = "dfp"
    NONLINEAR_CG = "nonlinear_cg"
    NELDER_MEAD = "nelder_mead"
    GRADIENT_DESCENT = "gradient_descent"
    OPTAX_LBFGS = "optax_lbfgs"
    OPTAX_ADAM = "optax_adam"
    OPTAX_SGD = "optax_sgd"
    FIXED_POINT_ITERATION = "fixed_point_iteration"
    GAUSS_NEWTON = "gauss_newton"
    LEVENBERG_MARQUARDT = "levenberg_marquardt"
    INDIRECT_LEVENBERG_MARQUARDT = "indirect_levenberg_marquardt"
    DOGLEG = "dogleg"


class SolverProfile(StrEnum):
    TOLERANCE = "tolerance"
    LINEAR = "linear"
    LEARNING_RATE = "learning_rate"
    TOL_ONLY = "tol_only"
    OPTAX = "optax"


class NormKind(StrEnum):
    MAX = "max"
    RMS = "rms"
    TWO = "two"


class InnerSolver(StrEnum):
    AUTO = "auto"
    LU = "lu"
    QR = "qr"
    SVD = "svd"
    GMRES = "gmres"
    BICGSTAB = "bicgstab"
    NORMAL_CG = "normal_cg"


class AdjointMode(StrEnum):
    IMPLICIT = "implicit"
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class NonlinearFault(BaseException):
    tag: Literal["bracket", "unserved"] = tag()
    bracket: tuple[str, str] = case()
    unserved: tuple[str, str] = case()

    def __str__(self) -> str:
        match self:
            case NonlinearFault(tag="bracket", bracket=(solver, render)):
                return f"bracket:{solver}:{render}"
            case NonlinearFault(tag="unserved", unserved=(route, request)):
                return f"unserved:{route}:{request}"
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-3
_FD: float = 1e-6

_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-8)])

# --- [MODELS] ---------------------------------------------------------------------------


class SolverPolicy(Struct, frozen=True):
    rtol: float = _TOL
    atol: float = _TOL
    norm: NormKind = NormKind.MAX
    inner: InnerSolver = InnerSolver.AUTO
    learning_rate: float | None = _LR
    best_so_far: bool = False


class NonlinearPolicy(Struct, frozen=True):
    max_steps: int = 256
    adjoint: AdjointMode = AdjointMode.IMPLICIT
    has_aux: bool = False
    batched: bool = False
    bracket: tuple[float, float] | None = None
    solver: SolverPolicy = SolverPolicy()


@tagged_union(frozen=True)
class NonlinearIntent:
    tag: Route = tag()
    root_find: tuple[ResidualFn, Pytree, NonlinearSolver, NonlinearPolicy] = case()
    minimise: tuple[ObjectiveFn, Pytree, NonlinearSolver, NonlinearPolicy] = case()
    fixed_point: tuple[ResidualFn, Pytree, NonlinearSolver, NonlinearPolicy] = case()
    least_squares: tuple[ResidualFn, Pytree, NonlinearSolver, NonlinearPolicy] = case()

    @staticmethod
    def RootFind(
        residual_fn: ResidualFn, x0: Pytree, solver: NonlinearSolver = NonlinearSolver.NEWTON, policy: NonlinearPolicy = NonlinearPolicy()
    ) -> "NonlinearIntent":
        return NonlinearIntent(root_find=(residual_fn, x0, solver, policy))

    @staticmethod
    def Minimise(
        objective: ObjectiveFn, x0: Pytree, solver: NonlinearSolver = NonlinearSolver.LBFGS, policy: NonlinearPolicy = NonlinearPolicy()
    ) -> "NonlinearIntent":
        return NonlinearIntent(minimise=(objective, x0, solver, policy))

    @staticmethod
    def FixedPoint(
        step_fn: ResidualFn, x0: Pytree, solver: NonlinearSolver = NonlinearSolver.FIXED_POINT_ITERATION, policy: NonlinearPolicy = NonlinearPolicy()
    ) -> "NonlinearIntent":
        return NonlinearIntent(fixed_point=(step_fn, x0, solver, policy))

    @staticmethod
    def NonlinearLeastSquares(
        residual_fn: ResidualFn,
        x0: Pytree,
        solver: NonlinearSolver = NonlinearSolver.LEVENBERG_MARQUARDT,
        policy: NonlinearPolicy = NonlinearPolicy(),
    ) -> "NonlinearIntent":
        return NonlinearIntent(least_squares=(residual_fn, x0, solver, policy))

    async def solve(self, lane: LanePolicy, key: ContentKey, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[Solve[Pytree]]":
        async def dispatch() -> "RuntimeRail[Solve[Pytree]]":
            return await lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self, key)

        return await evidence_run(EvidenceScope.NONLINEAR, f"solve.{self.tag}", dispatch, facts={"route": self.tag}, composition=composition)

    def graduates(
        self, solve: "Solve[Pytree]", ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Graduation]":
        return graduate(
            EvidenceScope.NONLINEAR.value, f"solve.{self.tag}", solve.content_key, solve, ceiling or dict(_CEILING.items()),
            composition=composition,
        )


@dataclass(frozen=True, slots=True)
class NonlinearEngine:
    jax: object
    jnp: object
    jtu: object
    eqx: object
    optx: object
    lx: object
    optax: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)

        import equinox as eqx
        import jax.numpy as jnp
        import jax.tree_util as jtu
        import lineax as lx
        import optax
        import optimistix as optx

        return cls(jax=jax, jnp=jnp, jtu=jtu, eqx=eqx, optx=optx, lx=lx, optax=optax)

    def norm(self, kind: NormKind) -> Callable[[object], object]:
        return {NormKind.MAX: self.optx.max_norm, NormKind.RMS: self.optx.rms_norm, NormKind.TWO: self.optx.two_norm}[kind]

    def adjoint(self, mode: AdjointMode) -> object:
        return {AdjointMode.IMPLICIT: self.optx.ImplicitAdjoint, AdjointMode.RECURSIVE_CHECKPOINT: self.optx.RecursiveCheckpointAdjoint}[mode]()

    def verdict(self, result: object) -> str:
        return verdict(self.jnp, self.optx.RESULTS, result)

    def route(self, tag: Route, fn: Callable[..., object], policy: NonlinearPolicy) -> tuple[Callable[..., object], Callable[[object], object]]:
        return _route_cells(self, fn, policy)[tag]

    def build_solver(self, tag: Route, solver: NonlinearSolver, policy: SolverPolicy) -> object:
        spec = _SOLVER[solver]
        base, norm = {"rtol": policy.rtol, "atol": policy.atol}, self.norm(policy.norm)
        match spec.profile:
            case SolverProfile.TOLERANCE:
                instance = getattr(self.optx, spec.attr)(**base, norm=norm)
            case SolverProfile.LINEAR:
                instance = getattr(self.optx, spec.attr)(**base, norm=norm, linear_solver=_INNER[policy.inner](policy, self.lx))
            case SolverProfile.LEARNING_RATE:
                instance = getattr(self.optx, spec.attr)(
                    learning_rate=policy.learning_rate if policy.learning_rate is not None else _LR, **base, norm=norm
                )
            case SolverProfile.TOL_ONLY:
                instance = getattr(self.optx, spec.attr)(**base)
            case SolverProfile.OPTAX:
                lr = None if solver is NonlinearSolver.OPTAX_LBFGS else (policy.learning_rate if policy.learning_rate is not None else _LR)
                instance = self.optx.OptaxMinimiser(getattr(self.optax, spec.attr)(learning_rate=lr), **base, norm=norm)
            case _ as unreachable:
                assert_never(unreachable)
        return getattr(self.optx, _BEST[tag])(instance) if policy.best_so_far else instance


# --- [TABLES] ---------------------------------------------------------------------------


class _SolverSpec(Struct, frozen=True):
    attr: str
    profile: SolverProfile


_SOLVER: Map[NonlinearSolver, _SolverSpec] = Map.of_seq([
    (NonlinearSolver.NEWTON, _SolverSpec("Newton", SolverProfile.LINEAR)),
    (NonlinearSolver.CHORD, _SolverSpec("Chord", SolverProfile.LINEAR)),
    (NonlinearSolver.BISECTION, _SolverSpec("Bisection", SolverProfile.TOL_ONLY)),
    (NonlinearSolver.GOLDEN_SEARCH, _SolverSpec("GoldenSearch", SolverProfile.TOL_ONLY)),
    (NonlinearSolver.LBFGS, _SolverSpec("LBFGS", SolverProfile.TOLERANCE)),
    (NonlinearSolver.BFGS, _SolverSpec("BFGS", SolverProfile.TOLERANCE)),
    (NonlinearSolver.DFP, _SolverSpec("DFP", SolverProfile.TOLERANCE)),
    (NonlinearSolver.NONLINEAR_CG, _SolverSpec("NonlinearCG", SolverProfile.TOLERANCE)),
    (NonlinearSolver.NELDER_MEAD, _SolverSpec("NelderMead", SolverProfile.TOLERANCE)),
    (NonlinearSolver.GRADIENT_DESCENT, _SolverSpec("GradientDescent", SolverProfile.LEARNING_RATE)),
    (NonlinearSolver.OPTAX_LBFGS, _SolverSpec("lbfgs", SolverProfile.OPTAX)),
    (NonlinearSolver.OPTAX_ADAM, _SolverSpec("adam", SolverProfile.OPTAX)),
    (NonlinearSolver.OPTAX_SGD, _SolverSpec("sgd", SolverProfile.OPTAX)),
    (NonlinearSolver.FIXED_POINT_ITERATION, _SolverSpec("FixedPointIteration", SolverProfile.TOLERANCE)),
    (NonlinearSolver.GAUSS_NEWTON, _SolverSpec("GaussNewton", SolverProfile.LINEAR)),
    (NonlinearSolver.LEVENBERG_MARQUARDT, _SolverSpec("LevenbergMarquardt", SolverProfile.LINEAR)),
    (NonlinearSolver.INDIRECT_LEVENBERG_MARQUARDT, _SolverSpec("IndirectLevenbergMarquardt", SolverProfile.LINEAR)),
    (NonlinearSolver.DOGLEG, _SolverSpec("Dogleg", SolverProfile.LINEAR)),
])


_INNER: Map[InnerSolver, InnerPick] = Map.of_seq([
    (InnerSolver.AUTO, lambda p, lx: lx.AutoLinearSolver(well_posed=None)),
    (InnerSolver.LU, lambda p, lx: lx.LU()),
    (InnerSolver.QR, lambda p, lx: lx.QR()),
    (InnerSolver.SVD, lambda p, lx: lx.SVD()),
    (InnerSolver.GMRES, lambda p, lx: lx.GMRES(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.BICGSTAB, lambda p, lx: lx.BiCGStab(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.NORMAL_CG, lambda p, lx: lx.Normal(lx.CG(rtol=p.rtol, atol=p.atol))),
])


_BEST: Map[Route, str] = Map.of_seq([
    ("root_find", "BestSoFarRootFinder"),
    ("minimise", "BestSoFarMinimiser"),
    ("fixed_point", "BestSoFarFixedPoint"),
    ("least_squares", "BestSoFarLeastSquares"),
])


def _route_cells(
    e: "NonlinearEngine", fn: Callable[..., object], policy: NonlinearPolicy
) -> Map[Route, tuple[Callable[..., object], Callable[[object], object]]]:
    norm, out = e.norm(policy.solver.norm), (lambda v: fn(v)[0]) if policy.has_aux else fn
    grad_fn = e.jax.grad(fn, has_aux=policy.has_aux)
    minim = (lambda v: grad_fn(v)[0]) if policy.has_aux else grad_fn
    return Map.of_seq([
        ("root_find", (e.optx.root_find, lambda v: norm(out(v)))),
        ("minimise", (e.optx.minimise, lambda v: norm(minim(v)))),
        ("fixed_point", (e.optx.fixed_point, lambda v: norm(e.jtu.tree_map(lambda a, b: e.jnp.asarray(a) - e.jnp.asarray(b), out(v), v)))),
        ("least_squares", (e.optx.least_squares, lambda v: norm(out(v)))),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dispatch(intent: NonlinearIntent, key: ContentKey) -> "Solve[Pytree]":
    match intent:
        case (
            NonlinearIntent(tag="root_find", root_find=(fn, x0, solver, policy))
            | NonlinearIntent(tag="minimise", minimise=(fn, x0, solver, policy))
            | NonlinearIntent(tag="fixed_point", fixed_point=(fn, x0, solver, policy))
            | NonlinearIntent(tag="least_squares", least_squares=(fn, x0, solver, policy))
        ):
            if _SOLVER[solver].profile is SolverProfile.TOL_ONLY and (
                policy.bracket is None or not all(np.isfinite(policy.bracket)) or policy.bracket[0] >= policy.bracket[1]
            ):
                raise NonlinearFault(bracket=(solver.value, repr(policy.bracket)))
            try:
                engine = NonlinearEngine.gated()
            except ImportError:
                return _floor_solve(key, intent.tag, fn, np.asarray(x0), solver, policy)
            return _optimistix_solve(engine, key, intent.tag, fn, x0, solver, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _optimistix_solve(
    engine: "NonlinearEngine", key: ContentKey, tag: Route, fn: Callable[..., object], x0: Pytree, solver: NonlinearSolver,
    policy: NonlinearPolicy,
) -> "Solve[Pytree]":
    jtu = engine.jtu
    op, lift = engine.route(tag, fn, policy)
    instance, adjoint = engine.build_solver(tag, solver, policy.solver), engine.adjoint(policy.adjoint)
    profile = _SOLVER[solver].profile
    options = {"lower": policy.bracket[0], "upper": policy.bracket[1]} if profile is SolverProfile.TOL_ONLY and policy.bracket is not None else None
    compiled = engine.eqx.filter_jit(lambda y, _: fn(y))
    lift_x0 = jtu.tree_map(engine.jnp.asarray, x0)

    def run(start: object) -> object:
        return op(compiled, instance, start, options=options, max_steps=policy.max_steps, adjoint=adjoint, has_aux=policy.has_aux, throw=False)

    if policy.batched:
        solutions = engine.eqx.filter_vmap(engine.eqx.filter_jit(run), in_axes=0)(lift_x0)
        per_row = engine.eqx.filter_vmap(lift, in_axes=0)(solutions.value)
        residual = float(engine.jnp.max(engine.jnp.asarray(per_row)))
        steps = int(engine.jnp.max(engine.jnp.asarray(solutions.stats["num_steps"])))
        result = engine.verdict(solutions.result)
        value = solutions.value
        width = int(engine.jnp.asarray(jtu.tree_leaves(lift_x0)[0]).shape[0])
        rank = _tree_rank(jtu, solutions.value) // width
    else:
        solution = run(lift_x0)
        residual, steps, result = float(lift(solution.value)), int(solution.stats["num_steps"]), engine.verdict(solution.result)
        rank = _tree_rank(jtu, solution.value)
        value = solution.value
    if tag == "least_squares":
        return Solve.LeastSquares(value, key, residual, rank, steps, Provider.GATED, tol=policy.solver.rtol, result=result)
    return Solve.Iterative(value, key, residual, steps, Provider.GATED, tol=policy.solver.rtol, result=result)


def _tree_rank(jtu: object, value: object) -> int:
    return int(sum(int(np.asarray(leaf).size) for leaf in jtu.tree_leaves(value)))


def _floor_solve(
    key: ContentKey, tag: Route, fn: Callable[..., object], x0: np.ndarray, solver: NonlinearSolver, policy: NonlinearPolicy
) -> "Solve[np.ndarray]":
    if _SOLVER[solver].profile is SolverProfile.TOL_ONLY:
        raise NonlinearFault(unserved=(tag, solver.value))
    if policy.batched:
        raise NonlinearFault(unserved=(tag, "batched"))
    rtol, out = policy.solver.rtol, (lambda v: fn(v)[0]) if policy.has_aux else fn
    basis = np.eye(x0.size).reshape((x0.size, *x0.shape))
    probe = (
        np.linalg.norm([float(out(x0 + _FD * e)) - float(out(x0 - _FD * e)) for e in basis], np.inf) / (2 * _FD)
        if tag == "minimise"
        else float(np.linalg.norm(np.asarray(out(x0)) - (x0 if tag == "fixed_point" else 0.0), np.inf))
    )
    if tag == "least_squares":
        return Solve.LeastSquares(x0, key, float(probe), int(x0.size), 0, Provider.FLOOR, tol=rtol)
    return Solve.Iterative(x0, key, float(probe), 0, Provider.FLOOR, tol=rtol)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
