# [PY_COMPUTE_NONLINEAR]

Nonlinear routes of the one numeric solver: `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over `optimistix` on the JAX floor, all four sharing one table-driven dispatch, a numpy central-difference floor reachable per route when the package is absent, and one `SolverReceipt` fold. Algorithm is never the entry point — each route carries a `NonlinearSolver` policy value resolved through the one `_SOLVER` profile table, and five orthogonal tuning axes ride one `SolverPolicy` value rather than per-solver `(rtol, atol)` literals.

One rail composes `optimistix` over a `lineax` inner linear solve and an `optax`-lifted descent on `equinox.filter_jit`/`filter_vmap` pytree transforms, under an `ImplicitAdjoint` (`RecursiveCheckpointAdjoint` for the ill-posed case) that `solvers/sensitivity.md#SENSITIVITY` differentiates through. Frozen `NonlinearEngine` folds the seven gated JAX modules so the solver build, route entry, adjoint, stationarity probe, and pytree read are carrier methods over one populated handle — a domain-named carrier, never a `SolveEngine` colliding with the `solvers/differential.md#DIFFERENTIAL` integration carrier. Its gated body floats the rail to float64 before the solve — the x64 contract the sibling JAX routes share — and loop-kernel/XLA acceleration is owned by `numerics/jit.md#JIT`.

## [01]-[INDEX]

- [01]-[NONLINEAR]: the four nonlinear routes over Optimistix with a numpy central-difference floor, every solver a `NonlinearSolver` policy row on the profile-driven builder, the gated JAX modules folded into one `NonlinearEngine` carrier.

## [02]-[NONLINEAR]

- Owner: `NonlinearSolver` is the one bounded solver vocabulary across every route; `_SOLVER` maps each member to a `_SolverSpec(attr, profile)` and `NonlinearEngine.build_solver` assembles the optimistix constructor keywords once by `SolverProfile`, so a new solver adds no construction body. Single resolved `norm` threads into both the `build_solver` termination and the route cell, so the termination norm and the receipt residual are one callable by construction. `_route_cells` keys one `(entry, residual contraction)` cell per route and `build_solver` one solver per member — never a tag×solver matrix. `best_so_far` wraps the converged solver in the route-matched `BestSoFar*` guard, one aspect over any solver.
- Cases: `least_squares` upcasts a minimiser member and `root_find`/`fixed_point` accept an upcast least-squares or minimiser solver where the problem class permits, per the `optimistix` entries. Batched path reduces the per-row `RESULTS` to the single worst-case termination member through `NonlinearEngine.verdict` — `jnp.max` over the per-row codes, the zero `successful` making `max == 0` iff every start converged — never `RESULTS.promote` (inheritance-widening that raises on a same-class member), the same multi-start resolution `optimization/design.md#DESIGN` runs.
- Entry: `NonlinearIntent.solve` composes `lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self)` under the `evidence_run` weave; the family declares `HOSTILE` because the x64 flag is process-global native state, and isolation, band, and worker-death retry derive at the runtime `Kernel` crossing owner from the trait row. Route forwards `max_steps`, the adjoint mode, and the profile-gated `options` under `throw=False`, so `solvers/sensitivity.md#SENSITIVITY` differentiates through the converged solution rather than the iteration trace and a non-`successful` verdict is recorded rather than raised.
- Output: the minimise/root/fixed-point routes fold residual, step count, and the `RESULTS` member name into `SolverReceipt.Iterative`; least-squares folds rank, step count, and the name into `SolverReceipt.LeastSquares`. Verdict is the true backend `RESULTS` member name off `Solution.result._value` (the `EnumerationItem` carries no `.name`), so a `max_steps_reached` or divergent solve carries its verdict rather than a residual-floor guess. Emission rides the weave's fenced contributor harvest, streaming `SolverReceipt.contribute` on the clean exit — `_dispatch` wears no emit decorator, so the receipt emits exactly once.
- Packages: `optimistix` (the solver, entry, and norm surface), `lineax` (the `InnerSolver`-projected `linear_solver=` family spanning tag-dispatched, direct, and iterative solvers), `optax` (the first-order transformations `OptaxMinimiser` lifts into minimise), `equinox` (`filter_jit`/`filter_vmap`), `jax`/`numpy` per the fence imports; `expression`/`msgspec` own the `NonlinearIntent` union and the value objects. `solvers/receipt.md#RECEIPT` owns `SolverReceipt` and the shared `verdict` fold (`SolveStatus` folds inside the receipt factories, never imported here); the hub `evidence_run` weave and the runtime offload crossing (`Kernel`/`KernelTrait`) compose silently.
- Growth: a new route is one `NonlinearIntent` case and one `_route_cells` cell; a new solver is one `NonlinearSolver` member and one `_SOLVER` row naming its `SolverProfile`; a new constructor surface is one `SolverProfile` member and one `build_solver` arm; a new termination norm, inner linear solver, or adjoint mode is one enum member and one arm or row on the matching carrier. A first-order step change is one `SolverPolicy.learning_rate` value, a 1-D bracketing solve one `NonlinearPolicy.bracket`, a multi-start study one `NonlinearPolicy.batched` vmapped through the same `solve` — never a second entry, never a per-route helper or emit.
- Boundary: the `TOL_ONLY` bracket is the per-solve entry argument (`options=dict(lower=, upper=)`), not a constructor kwarg like the five `SolverPolicy` axes, and rides `NonlinearPolicy` beside `max_steps`/`adjoint`/`has_aux`; a `TOL_ONLY` solve with an absent, non-finite, or unordered bracket rails as a typed boundary fault, gated in `_dispatch` before the import fork so the gated path and the numpy floor rail the misconfiguration identically. Numpy central-difference floor is reachable per route when `optimistix` is absent and narrows to `np.ndarray` at its jaxlib-free edge.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.receipt import SolverReceipt, verdict
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] -------------------------------------------------------------------------------

# x0 and every solve value are JAX pytrees Optimistix tracks through iteration; the floor narrows to
# np.ndarray at its jaxlib-free edge. The minimise scalar is a traced rank-0 Array (`Shaped[Array, '']`), never a Python float.
type Pytree = object
type Scalar = Pytree
type ResidualFn = Callable[[Pytree], Pytree]
type ObjectiveFn = Callable[[Pytree], Scalar]
type Route = Literal["root_find", "minimise", "fixed_point", "least_squares"]
# An inner cell reads the policy tolerance and gated `lx` module, returning the lineax `linear_solver=` the LINEAR profile threads.
type InnerPick = Callable[["SolverPolicy", object], object]


class NonlinearSolver(StrEnum):
    NEWTON = "newton"
    CHORD = "chord"
    BISECTION = "bisection"
    GOLDEN_SEARCH = "golden_search"  # 1-D golden-section minimiser; tolerance-only like Bisection
    LBFGS = "lbfgs"
    BFGS = "bfgs"
    DFP = "dfp"
    NONLINEAR_CG = "nonlinear_cg"
    NELDER_MEAD = "nelder_mead"
    GRADIENT_DESCENT = "gradient_descent"
    OPTAX_LBFGS = "optax_lbfgs"  # optax.lbfgs lifted by OptaxMinimiser into the minimise route
    OPTAX_ADAM = "optax_adam"
    OPTAX_SGD = "optax_sgd"
    FIXED_POINT_ITERATION = "fixed_point_iteration"
    GAUSS_NEWTON = "gauss_newton"
    LEVENBERG_MARQUARDT = "levenberg_marquardt"
    INDIRECT_LEVENBERG_MARQUARDT = "indirect_levenberg_marquardt"
    DOGLEG = "dogleg"


# Which keyword surface a solver constructor carries; build_solver assembles the kwargs by profile, not per-solver literals.
class SolverProfile(StrEnum):
    TOLERANCE = "tolerance"  # (rtol, atol, norm) — quasi-Newton / derivative-free
    LINEAR = "linear"  # + linear_solver= from the InnerSolver cell — Newton/Chord/GN/LM/Dogleg
    LEARNING_RATE = "learning_rate"  # + learning_rate=policy.learning_rate — GradientDescent
    TOL_ONLY = "tol_only"  # (rtol, atol) only — 1-D Bisection/GoldenSearch, no norm/linear_solver
    OPTAX = "optax"  # OptaxMinimiser(optim, rtol, atol, norm) — the optax-lifted minimisers


class NormKind(StrEnum):
    MAX = "max"  # optimistix.max_norm — Chebyshev (L-inf) termination
    RMS = "rms"  # optimistix.rms_norm
    TWO = "two"  # optimistix.two_norm — Euclidean (L2)


# Spans the lineax linear_solver= surface — tag-dispatched, direct, iterative.
class InnerSolver(StrEnum):
    AUTO = "auto"  # lineax.AutoLinearSolver(well_posed=None) — tag-dispatched
    LU = "lu"  # lineax.LU — square well-posed direct
    QR = "qr"  # lineax.QR — overdetermined inner system
    SVD = "svd"  # lineax.SVD — rank-deficient inner system
    GMRES = "gmres"  # lineax.GMRES — general square non-symmetric Krylov
    BICGSTAB = "bicgstab"  # lineax.BiCGStab — same Krylov band
    NORMAL_CG = "normal_cg"  # lineax.Normal(lineax.CG(...)) — SPD normal equations


class AdjointMode(StrEnum):
    IMPLICIT = "implicit"
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-3  # GradientDescent / optax step; the convergence tolerance is never reused as a learning rate
_FD: float = 1e-6  # central-difference half-step for the numpy floor's gradient/residual probe

# default graduation ceiling; a caller's tighter row overrides at the `graduate` projection on `solvers/receipt.md#RECEIPT`.
_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-8)])

# --- [MODELS] ------------------------------------------------------------------------------


# Five orthogonal tuning axes on one value object; `inner` reaches only the LINEAR family.
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
    has_aux: bool = False  # objective returns (value, aux); aux rides through the solve
    batched: bool = False  # x0 carries a leading sweep axis vmapped through one compiled solve
    # 1-D bracket the TOL_ONLY profile forwards as the entry options=dict(lower=, upper=); None elsewhere.
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

    async def solve(self, lane: LanePolicy) -> "RuntimeRail[SolverReceipt]":
        # one HOSTILE-trait Kernel carries the solve — spec data and operands (`_dispatch` resolves by import in the worker); isolation,
        # band, and worker-death retry derive at the runtime Kernel crossing. The weave owns span, fence, and the contributor harvest.
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            return await lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self)

        return await evidence_run(EvidenceScope.NONLINEAR, f"solve.{self.tag}", dispatch, facts={"route": self.tag})


# Seven gated JAX modules folded into one value object; carrier methods read the handles off `self`, never a helper re-import or a
# loose (lx, optx, optax) triple. `gated()` floats to float64 and imports once.
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
        import equinox as eqx
        import jax
        import jax.numpy as jnp
        import jax.tree_util as jtu
        import lineax as lx
        import optax
        import optimistix as optx

        jax.config.update("jax_enable_x64", True)  # 1e-8 (rtol, atol) is below float32 eps; JAX defaults to float32
        return cls(jax=jax, jnp=jnp, jtu=jtu, eqx=eqx, optx=optx, lx=lx, optax=optax)

    def norm(self, kind: NormKind) -> Callable[[object], object]:
        return {NormKind.MAX: self.optx.max_norm, NormKind.RMS: self.optx.rms_norm, NormKind.TWO: self.optx.two_norm}[kind]

    def adjoint(self, mode: AdjointMode) -> object:
        return {AdjointMode.IMPLICIT: self.optx.ImplicitAdjoint, AdjointMode.RECURSIVE_CHECKPOINT: self.optx.RecursiveCheckpointAdjoint}[mode]()

    def verdict(self, result: object) -> str:
        # one-row composition of the receipt-owned enum-verdict fold, parameterized by this carrier's gated handle and optimistix.RESULTS.
        return verdict(self.jnp, self.optx.RESULTS, result)

    def route(self, tag: Route, fn: Callable[..., object], policy: NonlinearPolicy) -> tuple[Callable[..., object], Callable[[object], object]]:
        return _route_cells(self, fn, policy)[tag]

    # Assemble the optimistix constructor keywords once by profile. `spec.attr` names an optimistix member for four profiles and an
    # optax member for OPTAX, resolved per-arm off the owning module — never one eager getattr(self.optx, attr) crashing on an optax name.
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
                # None keeps optax.lbfgs's zoom line search; adam/sgd carry a required scalar, never None.
                lr = None if solver is NonlinearSolver.OPTAX_LBFGS else (policy.learning_rate if policy.learning_rate is not None else _LR)
                instance = self.optx.OptaxMinimiser(getattr(self.optax, spec.attr)(learning_rate=lr), **base, norm=norm)
            case _ as unreachable:
                assert_never(unreachable)
        return getattr(self.optx, _BEST[tag])(instance) if policy.best_so_far else instance


# --- [TABLES] ------------------------------------------------------------------------------


# NonlinearSolver -> (optimistix attr, profile); the attr defers behind the gated import, the profile decides which kwargs build.
# DampedNewtonDescent is absent — a composable AbstractDescent taking linear_solver=, not an AbstractLeastSquaresSolver.
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


# InnerSolver -> the lineax linear_solver= the LINEAR profile threads; the iterative cells carry the policy tolerance the Krylov loop reads.
_INNER: Map[InnerSolver, InnerPick] = Map.of_seq([
    (InnerSolver.AUTO, lambda p, lx: lx.AutoLinearSolver(well_posed=None)),
    (InnerSolver.LU, lambda p, lx: lx.LU()),
    (InnerSolver.QR, lambda p, lx: lx.QR()),
    (InnerSolver.SVD, lambda p, lx: lx.SVD()),
    (InnerSolver.GMRES, lambda p, lx: lx.GMRES(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.BICGSTAB, lambda p, lx: lx.BiCGStab(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.NORMAL_CG, lambda p, lx: lx.Normal(lx.CG(rtol=p.rtol, atol=p.atol))),
])


# Route -> the route-matched BestSoFar* guard applied when SolverPolicy.best_so_far is set.
_BEST: Map[Route, str] = Map.of_seq([
    ("root_find", "BestSoFarRootFinder"),
    ("minimise", "BestSoFarMinimiser"),
    ("fixed_point", "BestSoFarFixedPoint"),
    ("least_squares", "BestSoFarLeastSquares"),
])


# Route -> (optimistix entry, residual contraction). The optimistix norm is pytree-total over Array leaves, contracting a structured
# residual to one scalar directly. has_aux: fn/grad return (out, aux), the cell reads only out. The fixed-point residual subtracts
# leaf-wise via tree_map before the norm, since `-` on two pytrees is undefined.
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


# --- [OPERATIONS] --------------------------------------------------------------------------


# One measured kernel returning SolverReceipt — module-level so it crosses the process lane. The try/except ImportError is the boundary import gate.
def _dispatch(intent: NonlinearIntent) -> SolverReceipt:
    match intent:
        case (
            NonlinearIntent(tag="root_find", root_find=(fn, x0, solver, policy))
            | NonlinearIntent(tag="minimise", minimise=(fn, x0, solver, policy))
            | NonlinearIntent(tag="fixed_point", fixed_point=(fn, x0, solver, policy))
            | NonlinearIntent(tag="least_squares", least_squares=(fn, x0, solver, policy))
        ):
            # Bracket invariant is a policy gate held before the import attempt so both paths rail it identically:
            # finiteness precedes the order check — an infinite bound satisfies a bare inequality, poisons the entry
            # options and the floor's midpoint probe alike — and a degenerate or inverted bracket brackets no root.
            if _SOLVER[solver].profile is SolverProfile.TOL_ONLY and (
                policy.bracket is None or not all(np.isfinite(policy.bracket)) or policy.bracket[0] >= policy.bracket[1]
            ):
                raise ValueError(f"{solver} requires NonlinearPolicy.bracket=(lower, upper) finite with lower < upper")
            try:
                return _optimistix_receipt(intent.tag, fn, x0, solver, policy)
            except ImportError:
                return _floor_receipt(intent.tag, fn, np.asarray(x0), solver, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _optimistix_receipt(tag: Route, fn: Callable[..., object], x0: Pytree, solver: NonlinearSolver, policy: NonlinearPolicy) -> SolverReceipt:
    engine = NonlinearEngine.gated()
    jtu = engine.jtu
    op, lift = engine.route(tag, fn, policy)
    instance, adjoint = engine.build_solver(tag, solver, policy.solver), engine.adjoint(policy.adjoint)
    # TOL_ONLY maps the bracket 2-tuple to the entry options=dict(lower=, upper=); every other solver forwards options=None.
    profile = _SOLVER[solver].profile
    options = {"lower": policy.bracket[0], "upper": policy.bracket[1]} if profile is SolverProfile.TOL_ONLY and policy.bracket is not None else None
    compiled = engine.eqx.filter_jit(lambda y, _: fn(y))
    lift_x0 = jtu.tree_map(engine.jnp.asarray, x0)  # per-leaf lift, total over a structured x0; a bare jnp.asarray flattens the pytree

    def run(start: object) -> object:
        return op(compiled, instance, start, options=options, max_steps=policy.max_steps, adjoint=adjoint, has_aux=policy.has_aux, throw=False)

    if policy.batched:
        solutions = engine.eqx.filter_vmap(engine.eqx.filter_jit(run), in_axes=0)(lift_x0)
        per_row = engine.eqx.filter_vmap(lift, in_axes=0)(solutions.value)
        residual = float(engine.jnp.max(engine.jnp.asarray(per_row)))  # worst per-row residual; each is itself a norm over that row's pytree
        steps = int(engine.jnp.max(engine.jnp.asarray(solutions.stats["num_steps"])))
        result = engine.verdict(solutions.result)  # worst-case verdict: the shared receipt fold reduces the per-row codes
        width = int(engine.jnp.asarray(jtu.tree_leaves(lift_x0)[0]).shape[0])  # leading sweep width
        rank = _tree_rank(jtu, solutions.value) // width  # batched leaf count divided back to per-row rank
    else:
        solution = run(lift_x0)
        residual, steps, result = float(lift(solution.value)), int(solution.stats["num_steps"]), engine.verdict(solution.result)
        rank = _tree_rank(jtu, solution.value)
    if tag == "least_squares":
        return SolverReceipt.LeastSquares(residual, rank, steps, policy.solver.rtol, result)
    return SolverReceipt.Iterative(residual, steps, policy.solver.rtol, result)


# Pytree-total element count over tree_leaves, so a structured value reports its true rank where a flat value.size assumes one leaf.
def _tree_rank(jtu: object, value: object) -> int:
    return int(sum(int(np.asarray(leaf).size) for leaf in jtu.tree_leaves(value)))


# Numpy central-difference floor, reachable per route when optimistix is absent. `out` applies the same has_aux unwrap the route
# cells do; result=None lets the shared receipt residual-floor adjudicate. A TOL_ONLY solver probes at the bracket midpoint (GoldenSearch ignores x0).
def _floor_receipt(tag: Route, fn: Callable[..., object], x0: np.ndarray, solver: NonlinearSolver, policy: NonlinearPolicy) -> SolverReceipt:
    rtol, out = policy.solver.rtol, (lambda v: fn(v)[0]) if policy.has_aux else fn
    bracketed = _SOLVER[solver].profile is SolverProfile.TOL_ONLY and policy.bracket is not None
    probe_at = np.asarray((policy.bracket[0] + policy.bracket[1]) / 2) if bracketed else x0  # TOL_ONLY: region centre, x0 carries no meaning
    basis = np.eye(probe_at.size).reshape((
        probe_at.size,
        *probe_at.shape,
    ))  # per-component unit steps reshaped to the single dense leaf the floor narrows to
    probe = (
        np.linalg.norm([float(out(probe_at + _FD * e)) - float(out(probe_at - _FD * e)) for e in basis], np.inf) / (2 * _FD)
        if tag == "minimise"
        else float(np.linalg.norm(np.asarray(out(probe_at)) - (probe_at if tag == "fixed_point" else 0.0), np.inf))
    )
    if tag == "least_squares":
        return SolverReceipt.LeastSquares(float(probe), int(x0.size), 0, rtol, None)
    return SolverReceipt.Iterative(float(probe), 0, rtol, None)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
