# [PY_COMPUTE_NONLINEAR]

The nonlinear routes of the one numeric solver. `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over `optimistix` on the JAX floor, all four sharing one table-driven dispatch, a reachable numpy central-difference floor per route, and one `SolverReceipt` fold. The algorithm is never the entry point: each route carries a `NonlinearSolver` policy value resolved through the one `_SOLVER` profile table — `Newton`/`Chord`/`Bisection` for roots, `LBFGS`/`BFGS`/`DFP`/`NonlinearCG`/`NelderMead`/`GradientDescent`/`GoldenSearch` plus the three `OptaxMinimiser`-lifted members (`OPTAX_LBFGS`/`OPTAX_ADAM`/`OPTAX_SGD`) for minimise, `FixedPointIteration`, and `GaussNewton`/`LevenbergMarquardt`/`IndirectLevenbergMarquardt`/`Dogleg` for least-squares. Five orthogonal tuning axes ride one `SolverPolicy` value rather than per-solver `(rtol, atol)` literals: tolerance, the `NormKind` termination norm, the `InnerSolver` `lineax` `linear_solver=` cell, the `learning_rate` the `GradientDescent`/`OptaxMinimiser` step reads (`None` arming the `optax.lbfgs` Wolfe zoom line search rather than disabling it), and the `best_so_far` flag arming the route-matched `BestSoFar*` guard.

The rail composes four admitted libraries as one woven flow rather than four flat per-library calls: an `optimistix` solve over a `lineax` inner linear solve and an `optax`-lifted descent, on `equinox.filter_jit`/`filter_vmap` pytree transforms, under an `ImplicitAdjoint` (`RecursiveCheckpointAdjoint` for the ill-posed case) that `solvers/sensitivity.md#SENSITIVITY` differentiates through. The frozen `NonlinearEngine` value object folds the seven gated modules (`jax`/`jnp`/`jtu`/`eqx`/`optx`/`lx`/`optax`) so the solver build, the route entry, the adjoint, the `jax.grad` stationarity probe, and the per-leaf `jtu.tree_map` pytree read are carrier methods reading one populated handle rather than re-imported module handles threaded through every helper — the same `DiffEngine.gated()` discipline `solvers/sensitivity.md#SENSITIVITY` runs, each solver module owning a domain-named carrier rather than a second `SolveEngine` colliding with the `solvers/differential.md#DIFFERENTIAL` integration carrier. The gated body floats the rail to float64 with `jax.config.update("jax_enable_x64", True)` before the solve — the `1e-8` `(rtol, atol)` is below float32 eps (`~1.2e-7`) so without the flag the termination criterion is unsatisfiable and a downcast `x0` loses the precision the receipt's residual floor adjudicates against — the same x64 contract every sibling JAX solver route (`solvers/differential.md#DIFFERENTIAL`, `solvers/sensitivity.md#SENSITIVITY`) carries on the worker lane. Loop-kernel and XLA acceleration is owned by `numerics/jit.md#JIT`.

## [01]-[INDEX]

- [01]-[NONLINEAR]: root/minimise/fixed-point/least-squares routes over Optimistix plus a numpy central-difference floor, every solver a `NonlinearSolver` policy row, the five tuning axes (`(rtol, atol)`/`NormKind`/`InnerSolver`/`learning_rate`/`best_so_far`) on one `SolverPolicy` value object, the gated `jax`/`jnp`/`jtu`/`eqx`/`optx`/`lx`/`optax` modules folded into one `NonlinearEngine` carrier, `optax`-lifted minimisers and `BestSoFar*` guards stacked through the unified x64-floated solve, the measured `_dispatch` kernel wearing the runtime `@receipted` aspect that harvests `SolverReceipt.contribute` on exit

## [02]-[NONLINEAR]

- Solver policy: `NonlinearSolver` is the ONE bounded solver vocabulary across every route, and `_SOLVER` maps each member to a `_SolverSpec(attr, profile)` row. The `SolverProfile` enum records which keyword surface the constructor carries: `TOLERANCE` for the bare `(rtol, atol, norm)` quasi-Newton/derivative-free family, `LINEAR` for the `Newton`/`Chord`/`GaussNewton`/`LevenbergMarquardt`/`Dogleg` family that additionally threads `linear_solver=` from the `InnerSolver` cell, `LEARNING_RATE` for `GradientDescent` whose `learning_rate=policy.learning_rate` the convergence tolerance is never reused for, `TOL_ONLY` for the tolerance-only 1-D `Bisection`/`GoldenSearch` carrying neither `norm` nor `linear_solver`, and `OPTAX` for the three `OptaxMinimiser(optim, rtol, atol, norm)`-lifted minimisers. `NonlinearEngine.build_solver(tag, solver, policy)` reads the profile off the carrier's gated modules and assembles the constructor keywords once, so a new solver is one enum member plus one `_SOLVER` row — never a repeated `rtol=_TOL` literal, never a sixth solver body, and never a loose `(lx, optx, optax)` module triple threaded through a free builder. `_route_cells` records, per tag, the `optimistix` entry and the residual contraction (`norm(fn(v))` for root and least-squares, the stationarity residual `norm(jax.grad(fn)(v))` for minimise, `norm(fn(v) - v)` over the per-leaf `jax.tree_util.tree_map` difference for fixed-point), so the route reads one cell and the solver reads one orthogonal cell, not a tag×solver matrix; the `optimistix` norm is pytree-total over `Array` leaves (`(PyTree[Array]) → Shaped[Array, '']`), so a structured `x0` (a parameter `(weights, bias)` pair) contracts to one global scalar directly while the fixed-point residual `fn(v) - v` subtracts leaf-wise first because `-` on two pytrees is undefined. The single resolved `norm` is threaded into both `build_solver` and the route cell, so the termination norm and the receipt residual are the same callable by construction; `_BEST` wraps a converged solver in the route-matched `BestSoFarRootFinder`/`BestSoFarMinimiser`/`BestSoFarFixedPoint`/`BestSoFarLeastSquares` when `SolverPolicy.best_so_far` is set, the monotone-iterate guard as one aspect over any solver. `least_squares` upcasts a minimiser member and `root_find`/`fixed_point` accept an upcast least-squares or minimiser solver when the problem class permits, as the `optimistix` entries document.
- Step-size axis: `SolverPolicy.learning_rate: float | None` is the ONE first-order-step axis the convergence tolerance is never reused for. The `LEARNING_RATE`-profile `GradientDescent` reads it as its `learning_rate=` (defaulting `1e-3`), and the `OPTAX`-profile minimisers thread it into the lifted `optax` transformation — but `optax.lbfgs(learning_rate=None, ...)` is line-search-driven, so the `OPTAX_LBFGS` member passes `learning_rate=None` to keep its `scale_by_zoom_linesearch` Wolfe zoom rather than a fixed step that disables it, while `OPTAX_ADAM`/`OPTAX_SGD` take the policy scalar. The bracket axis is orthogonal and lives on `NonlinearPolicy`, not `SolverPolicy`: `NonlinearPolicy.bracket: tuple[float, float] | None` is the per-solve entry-call argument the `TOL_ONLY` profile (`Bisection`/`GoldenSearch`) forwards as the optimistix entry `options=dict(lower=, upper=)`, since the bracket is a runtime region passed to the entry function — not a solver-constructor kwarg the way the five `SolverPolicy` axes are (`Bisection(rtol, atol, flip, expand_if_necessary)`/`GoldenSearch(rtol, atol)` take no bracket) — exactly like `max_steps`/`adjoint`/`has_aux` which already ride `NonlinearPolicy` and thread through `run()`; `GoldenSearch` ignores `y0` so the bracket is its sole region specifier, and every non-`TOL_ONLY` solver forwards `options=None`. The `InnerSolver` axis spans the full `lineax` `linear_solver=` family the catalogued `INTEGRATION_LAW` admits — `AutoLinearSolver(well_posed=None)` tag-dispatched, `LU`/`QR`/`SVD` direct, and `GMRES`/`BiCGStab`/`Normal(CG(...))` iterative — so a Newton step on a general non-symmetric Jacobian factors through `GMRES` and an LM normal-equation through `Normal(CG)` rather than only the four cells a narrower vocabulary exposes; `_INNER` is one `Callable[[SolverPolicy, object], object]` row per member reading the policy tolerance and the gated `lx` module.
- JAX-pytree discipline: `x0` and every solve value are JAX pytrees, so the gated body first floats the rail to float64 with `jax.config.update("jax_enable_x64", True)` (the `1e-8` tolerance is below float32 eps, and JAX downcasts a float64 `x0` to float32 without it), lifts the initial point per-leaf with `jax.tree_util.tree_map(jnp.asarray, x0)`, contracts residuals with the pytree-total `NormKind`-selected `optimistix` norm, and reads back the converged value through the same per-leaf `tree_map` — never `numpy.asarray`, which flattens a pytree and breaks a non-array leaf. The solve objective `lambda y, _: fn(y)` wraps in `equinox.filter_jit` so the static (non-array) leaves of the closure skip XLA tracing while array leaves compile through the iteration; the receipt-residual `lift` closes over the same user function and contracts the converged value once at egress through the resolved `NormKind` norm. The least-squares `rank` slot is the pytree-total leaf-element count `sum(leaf.size for leaf in jax.tree_util.tree_leaves(value))` (`_tree_rank`), total over a structured `x0` where a bare `value.size` assumes a single array leaf, the batched path dividing the stacked count by the leading sweep width to recover the per-row rank. When `NonlinearPolicy.batched` is set the leading axis of `x0` is a sweep of initial points and the whole solve maps through `equinox.filter_vmap(..., in_axes=0)` as one compiled solve over the stack, folding the per-row residual scalar (itself a norm over that row's pytree) to its max component; the batched path reduces the per-row `RESULTS` to the single worst-case termination member through `NonlinearEngine.verdict` — `jnp.max` over the per-row `_value` codes (the zero `successful` making `max == 0` iff every start converged) plus the `RESULTS._name_to_item` name inversion, NEVER `RESULTS.promote` (which is inheritance-widening that raises on a same-class member, not a vmap combine, exactly as `optimization/design.md#DESIGN` resolves its multi-start ensemble) — so the sweep carries its true aggregate verdict rather than the `result=None` residual-floor fiction, while the single-point path inverts the one `int(solution.result._value)` to its member name. One compiled solve over the whole sweep, never a Python loop over starts.
- Entry: `NonlinearIntent.solve(lane)` is the one `async` method on the union, composing `lane.offload(_dispatch, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)` under the hub `evidence_run` weave — the x64-gated family pins the PROCESS modality (the flag is process-global native state), the retry wraps the isolation leg only, and the weave owns span, fence, and the `@receipted(REDACTION)` receipt harvest; the minimise, root, and fixed-point routes fold the final residual, the iteration count, and the `RESULTS` member name into `SolverReceipt.Iterative`, and the least-squares route folds the rank, the step count, and the `RESULTS` member name into `SolverReceipt.LeastSquares`. The backend `RESULTS` member name (`NonlinearEngine.verdict`, a one-row composition of the receipt-owned shared `verdict` fold, off the `Solution.result._value` code, since the `EnumerationItem` carries no `.name`) flows to the receipt as the adjudication string the `_status` fold maps into `SolveStatus`, so a `max_steps_reached` or `nonlinear_divergence` solve carries its true verdict rather than collapsing to a residual-floor guess. The route passes `max_steps`, the `ImplicitAdjoint`/`RecursiveCheckpointAdjoint` mode, and the profile-gated `options` (the `TOL_ONLY` `Bisection`/`GoldenSearch` bracket projected to `options=dict(lower=, upper=)`, `options=None` for every other solver) from `NonlinearPolicy` into the Optimistix entry under `throw=False`, so `solvers/sensitivity.md#SENSITIVITY` differentiates through the converged solution rather than through the iteration trace, with the checkpoint adjoint reachable when the implicit form is not well-posed and a non-`successful` verdict recorded rather than raised. A `TOL_ONLY` solver selected with `bracket=None` rails as a typed boundary fault on the `solve` fence rather than crashing inside optimistix on the missing options. Emission rides the runtime `@receipted(_REDACTION)` aspect the measured `_dispatch` kernel wears, so the harvested `SolverReceipt.contribute` stream emits on exit rather than an inline `Signals.emit` threaded through the route body — matching every sibling solver route (`solvers/linear.md#LINEAR`, `solvers/differential.md#DIFFERENTIAL`, `solvers/field.md#FIELD`, `solvers/mesh.md#EXCHANGE`), the `boundary` fence and the `@receipted` aspect firing on the same exit.
- Packages: `optimistix` (`root_find`, `minimise`, `fixed_point`, `least_squares`, `Newton`, `Chord`, `Bisection`, `GoldenSearch`, `LBFGS`, `BFGS`, `DFP`, `NonlinearCG`, `NelderMead`, `GradientDescent`, `FixedPointIteration`, `GaussNewton`, `LevenbergMarquardt`, `IndirectLevenbergMarquardt`, `Dogleg`, `OptaxMinimiser`, `BestSoFarMinimiser`, `BestSoFarLeastSquares`, `BestSoFarRootFinder`, `BestSoFarFixedPoint`, `max_norm`, `rms_norm`, `two_norm`, `ImplicitAdjoint`, `RecursiveCheckpointAdjoint`, `Solution`, `RESULTS`), `lineax` (`AutoLinearSolver`, `LU`, `QR`, `SVD`, `GMRES`, `BiCGStab`, `Normal`, `CG` — the `InnerSolver`-projected `linear_solver=` cell for the Newton/Gauss-Newton/LM family, spanning the catalogued tag-dispatched/direct/iterative solver surface), `optax` (`lbfgs`, `adam`, `sgd` — the first-order transformations `OptaxMinimiser` lifts into the minimise route; `optax.lbfgs(learning_rate=None)` keeps its `scale_by_zoom_linesearch` Wolfe zoom that a fixed `learning_rate` would disable), `equinox` (`filter_jit`, `filter_vmap`), `jax` (`config.update("jax_enable_x64", True)` floating the gated solve to float64 so the `1e-8` tolerance is reachable rather than silently clamped at float32 eps, `grad` for the minimise stationarity residual, `tree_util.tree_map` for the per-leaf lift/read total over a structured `x0`, `tree_util.tree_leaves` for the pytree-total least-squares rank), `numpy` (`eye`, `linalg.norm`), `expression` (`tag`, `case`, `tagged_union` for the `NonlinearIntent` union; `expression.collections.Map` for the empty `Redaction.classified` policy), `msgspec` (`Struct` for the `SolverPolicy`/`NonlinearPolicy`/`_SolverSpec` value objects), `solvers/receipt.md#RECEIPT` (`SolverReceipt` — `SolveStatus` is folded inside the receipt factories, never imported here), hub (`EvidenceScope`/`evidence_run` — the span/fence/harvest weave), `solvers/receipt.md#RECEIPT` (`verdict` the shared enum-verdict fold composed one-row on the gated carrier), runtime (`RuntimeRail`, `LanePolicy`/`Modality` the offload axis, `RetryClass.OCCT` the worker-death band on the process hop).
- Growth: a new nonlinear route is one `NonlinearIntent` case plus one `_route_cells` cell; a new solver on any route is one `NonlinearSolver` member plus one `_SOLVER` row naming its `SolverProfile`, reusing `NonlinearEngine.build_solver`; a new constructor keyword surface is one `SolverProfile` member plus one `build_solver` arm; a new termination norm is one `NormKind` member plus one arm in `NonlinearEngine.norm`; a new inner linear solver is one `InnerSolver` member plus one `_INNER` row reading the policy tolerance; a new adjoint mode is one `AdjointMode` member plus one arm in `NonlinearEngine.adjoint`; a first-order step-size change is one `SolverPolicy.learning_rate` value, never a second entry; a 1-D bracketing solve sets `NonlinearPolicy.bracket` and the `TOL_ONLY` profile forwards it as the entry `options`, never a second bracketing entry; a multi-start study sets `NonlinearPolicy.batched` and vmaps through the same `solve`; the receipt evidence and its emission grow on the `solvers/receipt.md#RECEIPT` owner (`SolverReceipt` slot rows) and the runtime `@receipted` aspect, never a per-route emit. Zero new surface, never a parallel root-finder and minimiser owner, never a per-route helper body, never a boolean toggling two hardcoded solvers.

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
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass

# --- [TYPES] -------------------------------------------------------------------------------

# x0 and every solve value are arbitrary JAX pytrees the Optimistix solver tracks through
# iteration; the floor narrows to np.ndarray at its scipy-free boundary. The route functions are
# pytree->pytree (root/fixed-point/least-squares) or pytree->scalar (minimise), where the minimise
# scalar is a traced rank-0 JAX Array (`Shaped[Array, '']`) the solver differentiates, never a Python float.
type Pytree = object
type Scalar = Pytree
type ResidualFn = Callable[[Pytree], Pytree]
type ObjectiveFn = Callable[[Pytree], Scalar]
type Route = Literal["root_find", "minimise", "fixed_point", "least_squares"]
# An inner-linear-solver cell reads the policy tolerance and the gated `lx` module and returns the
# lineax solver instance the LINEAR-profile Newton/GN/LM family threads as linear_solver=.
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


# Which keyword surface a solver constructor carries; NonlinearEngine.build_solver assembles the kwargs
# from SolverPolicy by profile rather than seventeen hardcoded (rtol, atol) literals.
class SolverProfile(StrEnum):
    TOLERANCE = "tolerance"  # (rtol, atol, norm) — quasi-Newton / derivative-free
    LINEAR = "linear"  # + linear_solver= from the InnerSolver cell — Newton/Chord/GN/LM/Dogleg
    LEARNING_RATE = "learning_rate"  # + learning_rate=policy.learning_rate — GradientDescent
    TOL_ONLY = "tol_only"  # (rtol, atol) only — 1-D Bisection/GoldenSearch carry no norm/linear_solver
    OPTAX = "optax"  # OptaxMinimiser(optim, rtol, atol, norm) — the optax-lifted minimisers


class NormKind(StrEnum):
    MAX = "max"  # optimistix.max_norm — Chebyshev (L-inf) termination
    RMS = "rms"  # optimistix.rms_norm
    TWO = "two"  # optimistix.two_norm — Euclidean (L2)


# Spans the catalogued lineax linear_solver= surface — tag-dispatched, direct, and iterative — so a
# Newton step factors through the inner solver its Jacobian structure wants, not only four cells.
class InnerSolver(StrEnum):
    AUTO = "auto"  # lineax.AutoLinearSolver(well_posed=None) — tag-dispatched
    LU = "lu"  # lineax.LU — square well-posed direct
    QR = "qr"  # lineax.QR — overdetermined inner system
    SVD = "svd"  # lineax.SVD — rank-deficient inner system
    GMRES = "gmres"  # lineax.GMRES — general square non-symmetric Krylov
    BICGSTAB = "bicgstab"  # lineax.BiCGStab — general square non-symmetric Krylov
    NORMAL_CG = "normal_cg"  # lineax.Normal(lineax.CG(...)) — SPD normal equations


class AdjointMode(StrEnum):
    IMPLICIT = "implicit"
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-3  # GradientDescent / optax step size; the convergence tolerance is never reused as a learning rate
_FD: float = 1e-6  # central-difference half-step for the numpy floor's gradient/residual probe

# the family modality row: every nonlinear route rides the x64-gated optimistix carrier, so the
# family pins PROCESS — the x64 flag is process-global native state concurrent in-process solves
# corrupt; policy DATA beside the route tables, never a per-page literal.
_MODALITY: Final[Modality] = Modality.PROCESS

# the nonlinear family's DEFAULT graduation ceiling — the governed policy row per the hub ceiling
# law; a caller's tighter row overrides at the `graduate` projection on `solvers/receipt`.
_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-8)])

# --- [MODELS] ------------------------------------------------------------------------------


# The five orthogonal solver-tuning axes on one value object, threaded into the profile-driven
# constructor — never a per-solver hardcoded (rtol, atol). `inner` reaches only the LINEAR-profile
# family; `learning_rate` reaches GradientDescent and the optax-lifted minimisers (None arms the
# optax.lbfgs Wolfe zoom line search rather than disabling it); `best_so_far` arms the route-matched
# BestSoFar* monotone-iterate guard.
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
    # The 1-D bracket [lower, upper] the TOL_ONLY profile forwards as the optimistix entry
    # options=dict(lower=, upper=); None for every non-bracket solver (the only profile that reads it
    # is TOL_ONLY, and a Bisection/GoldenSearch solve with bracket=None rails as a boundary fault).
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
        # the x64-gated solve crosses the process lane as spec data plus operands (the module-level
        # `_dispatch` kernel resolves by import in the worker); worker death rides `retry=
        # RetryClass.OCCT` on the isolation leg only — the deterministic solve is never retried.
        # The weave owns span, fence, and the `@receipted(REDACTION)` receipt harvest.
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            return await lane.offload(_dispatch, self, modality=_MODALITY, retry=RetryClass.OCCT)

        return await evidence_run(EvidenceScope.NONLINEAR, f"solve.{self.tag}", dispatch)


# The seven gated modules folded into one value object with behavior: `build_solver`, `norm`, `route`,
# and `adjoint` read `self.optx`/`self.lx`/`self.optax`/`self.jnp` off the carrier, the route cells read
# `self.jax.grad`/`self.jtu.tree_map`, and the receipt fold reads `self.jtu.tree_leaves` — rather than
# any helper re-importing or any builder threading a loose (lx, optx, optax) handle triple. `gated()`
# floats the rail to float64 and imports once behind the band — the DiffEngine.gated() discipline
# `solvers/sensitivity.md#SENSITIVITY` runs, so the JAX flow (build -> solve -> read) is one rail.
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
        # one-row composition of the receipt-owned shared enum-verdict fold: the `_name_to_item`
        # inversion and the batched worst-code `jnp.max` reduce live on `solvers/receipt.verdict`,
        # parameterized by this carrier's gated handle and the `optimistix.RESULTS` class — the
        # zero-code convention makes `max == 0` iff every start converged, and `RESULTS.promote`
        # is inheritance-widening, never a batch combine.
        return verdict(self.jnp, self.optx.RESULTS, result)

    def route(self, tag: Route, fn: Callable[..., object], policy: NonlinearPolicy) -> tuple[Callable[..., object], Callable[[object], object]]:
        return _route_cells(self, fn, policy)[tag]

    # Read the solver's profile and assemble the optimistix constructor keywords once: the OPTAX
    # profile lifts an optax transformation (lbfgs keeping its Wolfe zoom under learning_rate=None);
    # the LEARNING_RATE profile carries the policy step; the LINEAR profile threads the InnerSolver
    # linear_solver= cell; TOL_ONLY carries neither norm nor linear_solver. `best_so_far` wraps the
    # converged solver in the route-matched BestSoFar* guard. `spec.attr` names an optimistix member for
    # the four optimistix profiles and an optax member for OPTAX, so the attribute resolves per-arm off
    # the owning module — never one eager `getattr(self.optx, spec.attr)` that crashes on an optax name.
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


# NonlinearSolver -> (optimistix-attribute-name, construction profile). The attribute name defers
# behind the gated optimistix import; the profile decides which SolverPolicy kwargs build_solver
# assembles, so a new solver is one row and never a sixth construction body. DampedNewtonDescent is
# absent: it is a composable AbstractDescent taking linear_solver=, not an AbstractLeastSquaresSolver.
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


# InnerSolver -> the lineax linear_solver= the LINEAR-profile Newton/GN/LM family threads; one row
# per member reading the policy tolerance and the gated `lx` module, so a new inner solver is one row
# and the iterative CG/GMRES/BiCGStab cells carry the convergence tolerance the Krylov loop reads.
_INNER: Map[InnerSolver, InnerPick] = Map.of_seq([
    (InnerSolver.AUTO, lambda p, lx: lx.AutoLinearSolver(well_posed=None)),
    (InnerSolver.LU, lambda p, lx: lx.LU()),
    (InnerSolver.QR, lambda p, lx: lx.QR()),
    (InnerSolver.SVD, lambda p, lx: lx.SVD()),
    (InnerSolver.GMRES, lambda p, lx: lx.GMRES(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.BICGSTAB, lambda p, lx: lx.BiCGStab(rtol=p.rtol, atol=p.atol)),
    (InnerSolver.NORMAL_CG, lambda p, lx: lx.Normal(lx.CG(rtol=p.rtol, atol=p.atol))),
])


# Route -> the route-matched BestSoFar* monotone-iterate guard wrapping a converged solver when
# SolverPolicy.best_so_far is set; one aspect over any solver, keyed by tag.
_BEST: Map[Route, str] = Map.of_seq([
    ("root_find", "BestSoFarRootFinder"),
    ("minimise", "BestSoFarMinimiser"),
    ("fixed_point", "BestSoFarFixedPoint"),
    ("least_squares", "BestSoFarLeastSquares"),
])


# Route -> (optimistix entry, residual contraction). The optimistix norm is pytree-total over Array
# leaves, so it contracts a structured residual (a parameter (weights, bias) pair) to one global
# scalar directly; the same resolved norm threads into both the solver's norm= termination and the
# receipt residual. has_aux: fn returns (out, aux), the contraction reads only `out`; jax.grad(has_aux=
# ...) mirrors the shape returning (grad, aux), so the minimise stationarity lift unwraps identically.
# The fixed-point residual is the per-leaf fn(v) - v over the pytree, since `-` on two pytrees is
# undefined — jax.tree_util.tree_map subtracts leaf-wise before the norm contracts the difference.
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


# the one measured kernel returning the `SolverReceipt` — module-level and import-resolvable, so it
# crosses the process lane as spec data plus operands; the weave's `@receipted(REDACTION)` harvest
# streams the receipt. The single optimistix/floor `try/except ImportError` is the boundary-kernel
# import gate, not a domain rail — the four-parallel per-route form is the deleted spelling.
def _dispatch(intent: NonlinearIntent) -> SolverReceipt:
    match intent:
        case (
            NonlinearIntent(tag="root_find", root_find=(fn, x0, solver, policy))
            | NonlinearIntent(tag="minimise", minimise=(fn, x0, solver, policy))
            | NonlinearIntent(tag="fixed_point", fixed_point=(fn, x0, solver, policy))
            | NonlinearIntent(tag="least_squares", least_squares=(fn, x0, solver, policy))
        ):
            # the TOL_ONLY bracket invariant is a POLICY gate, not an optimistix option quirk: it
            # holds HERE before the import attempt so the gated path and the numpy floor rail the
            # same misconfiguration identically — optional-package availability never changes the
            # fault, and the floor's bracket read is established by construction past this gate.
            if _SOLVER[solver].profile is SolverProfile.TOL_ONLY and policy.bracket is None:
                raise ValueError(f"{solver} requires NonlinearPolicy.bracket=(lower, upper)")
            try:
                return _optimistix_receipt(intent.tag, fn, x0, solver, policy)
            except ImportError:
                return _floor_receipt(intent.tag, fn, np.asarray(x0), solver, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _optimistix_receipt(tag: Route, fn: Callable[..., object], x0: Pytree, solver: NonlinearSolver, policy: NonlinearPolicy) -> SolverReceipt:
    engine = NonlinearEngine.gated()  # imports the gated modules once and floats the rail to float64
    jtu = engine.jtu
    op, lift = engine.route(tag, fn, policy)
    instance, adjoint = engine.build_solver(tag, solver, policy.solver), engine.adjoint(policy.adjoint)
    # The TOL_ONLY pair (Bisection/GoldenSearch) carries the bracket through the entry options, not the
    # constructor, so the single edge maps the typed 2-tuple to optimistix's options=dict(lower=, upper=)
    # shape gated on the resolved profile; every other solver forwards options=None and a non-None bracket
    # on a non-TOL_ONLY solver is a no-op the entry never reads. The bracket invariant is `_dispatch`'s
    # shared policy gate, already established before this kernel runs.
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


# Pytree-total element count: sum the leaf sizes over jax.tree_util.tree_leaves so a structured
# converged value reports its true free-parameter rank where a flat value.size assumes one array leaf.
def _tree_rank(jtu: object, value: object) -> int:
    return int(sum(int(np.asarray(leaf).size) for leaf in jtu.tree_leaves(value)))


# The numpy central-difference floor, reachable for every route when the optimistix package is absent:
# the minimise probe is the gradient norm over a central difference of the scalar objective, the
# root/least-squares probe the residual norm at x0, the fixed-point probe ‖fn(x0) - x0‖. result=None
# lets the shared `solvers/receipt.md#RECEIPT` residual-floor adjudicate, the one verdict path every
# numpy floor in the corpus uses; the floor narrows to np.ndarray at its jaxlib-free boundary. `out`
# applies the same `has_aux` unwrap the route cells do — an aux-returning objective yields (value, aux)
# on the floor exactly as on the gated path, so the central difference reads only the value component.
# A TOL_ONLY solver (Bisection/GoldenSearch) probes at the bracket midpoint (lower+upper)/2 rather than
# x0, mirroring optimistix's bracket semantics — GoldenSearch ignores x0 entirely and Bisection is the
# region centre rather than an x0 probe optimistix would never compute; the bracket=None misconfiguration
# is railed by `_dispatch`'s shared policy gate BEFORE the import fork, so a TOL_ONLY solver reaching
# this floor carries a bracket by construction and no silent x0 fallback exists.
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
