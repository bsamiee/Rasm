# [PY_COMPUTE_NONLINEAR]

The nonlinear routes of the one numeric solver. `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over Optimistix on the JAX floor with a numpy central-difference floor reachable for every route, all four sharing one table-driven Optimistix dispatch and every route folding into the one `SolverReceipt`. Each route carries its solver as a `NonlinearSolver` policy value — never a hardcoded engine and never a boolean toggling two — so root-finding selects across `Newton`/`Chord`/`Bisection`, minimisation across `LBFGS`/`BFGS`/`DFP`/`NonlinearCG`/`NelderMead`/`GradientDescent`, fixed-point through `FixedPointIteration`, and least-squares across `GaussNewton`/`LevenbergMarquardt`/`IndirectLevenbergMarquardt`/`Dogleg`/`DampedNewtonDescent`, every member resolving through one `_SOLVER` factory table. Optimistix solves are implicit-function-theorem differentiable, so the route threads `optimistix.ImplicitAdjoint` by default (`RecursiveCheckpointAdjoint` for the ill-posed case) and a downstream sensitivity reads the adjoint through the solve. The residual probe, the objective evaluation, and the solve all run on JAX pytrees through `equinox.filter_jit`, and a batched `x0` stack vectorises through `equinox.filter_vmap` without leaving the compiled solve. Loop-kernel and XLA acceleration is owned by `numerics/jit.md#JIT`, not this route.

## [01]-[INDEX]

- [01]-[NONLINEAR]: root/minimise/fixed-point/least-squares routes over Optimistix + numpy central-difference floor, every solver a `NonlinearSolver` policy row

## [02]-[NONLINEAR]

- Owner: `NonlinearIntent` — the nonlinear-route cases on the one solver; `RootFind(residual_fn, x0, solver)`, `Minimise(objective, x0, solver)`, `FixedPoint(step_fn, x0, solver)`, and `NonlinearLeastSquares(residual_fn, x0, solver)` each discriminate the route, carry a `NonlinearSolver` policy whose default is the route-canonical engine, and carry the shared `NonlinearPolicy` (max-steps cap and adjoint mode). The Optimistix tier runs `optimistix.root_find`/`optimistix.minimise`/`optimistix.fixed_point`/`optimistix.least_squares` with the policy-selected solver and reads `optimistix.Solution.stats` (step count) and `optimistix.Solution.result.name` (the `RESULTS` member name) into `SolverReceipt`. The four routes share one Optimistix dispatch keyed on the tag — the entry function, the residual probe, and the receipt fold are the row; the solver is the orthogonal policy axis resolved through the one `_SOLVER` factory table — never four parallel helper bodies and never a per-route hardcoded engine. The numpy floor reports the gradient-norm or step residual at `x0` over a central-difference probe and is reachable for every route, so a cp315 run without the optimistix wheel never returns `Error(Import)`.
- Solver policy: `NonlinearSolver` is the ONE bounded solver vocabulary across every route; each member maps through `_SOLVER` to its Optimistix constructor under the shared `(rtol, atol)` tolerance, so a new solver is one enum member plus one `_SOLVER` row and the route stays a single dispatch. The `_ROUTE` table records, per tag, the Optimistix entry, the default solver member, and the residual contraction (`max_norm(fn(v))` for root and least-squares, the stationarity residual `max_norm(jax.grad(fn)(v))` for minimise, `max_norm(fn(v) - v)` for fixed-point), so the route reads one row and the solver reads one cell — two orthogonal tables, not a tag×solver matrix. `least_squares` upcasts a minimiser-family member, and `root_find`/`fixed_point` accept an upcast least-squares or minimiser solver when the problem class permits, exactly as the Optimistix entry points document.
- JAX-pytree discipline: `x0` and every solve value are JAX pytrees, so the gated body lifts the initial point with `jax.numpy.asarray`, contracts residual norms with `optimistix.max_norm`, and reads back the converged value with `jax.numpy.asarray` — never `numpy.asarray`, which would flatten a pytree and break a non-array leaf. The residual probe `lift` closes over the user function and runs inside the same compiled region, and `equinox.filter_jit` wraps the residual evaluation so the static (non-array) leaves of the closure skip tracing. When `NonlinearPolicy.batched` is set the leading axis of `x0` is a sweep of initial points and the whole solve maps through `equinox.filter_vmap(..., in_axes=0)` as one compiled solve over the stack, folding the per-row residual to its max component; the batched path adjudicates through the residual floor (`result=None`) because the per-row `RESULTS` member name is not reachable under a vmap trace, while the single-point path carries the backend's true `Solution.result.name`. One compiled solve over the whole sweep, never a Python loop over starts.
- Entry: `NonlinearIntent.solve` is a method on the union entering one `boundary(f"solve.{intent.tag}", ...)`; the minimise, root, and fixed-point routes fold the final residual, the iteration count, and the `RESULTS` member name into `SolverReceipt.Iterative`, and the least-squares route folds the rank, the step count, and the `RESULTS` member name into `SolverReceipt.LeastSquares`. The backend `Solution.result.name` flows to the receipt as the adjudication string the `_status` fold maps into `SolveStatus`, so a `max_steps_reached` or `nonlinear_divergence` solve carries its true verdict rather than collapsing to a residual-floor guess. The route passes `max_steps` and the `ImplicitAdjoint`/`RecursiveCheckpointAdjoint` mode from `NonlinearPolicy` into the Optimistix entry under `throw=False`, so `solvers/sensitivity.md#SENSITIVITY` differentiates through the converged solution rather than through the iteration trace, with the checkpoint adjoint reachable when the implicit form is not well-posed and a non-`successful` verdict recorded rather than raised.
- Packages: `optimistix` (`root_find`, `minimise`, `fixed_point`, `least_squares`, `Newton`, `Chord`, `Bisection`, `LBFGS`, `BFGS`, `DFP`, `NonlinearCG`, `NelderMead`, `GradientDescent`, `FixedPointIteration`, `GaussNewton`, `LevenbergMarquardt`, `IndirectLevenbergMarquardt`, `Dogleg`, `DampedNewtonDescent`, `max_norm`, `ImplicitAdjoint`, `RecursiveCheckpointAdjoint`, `Solution`), `equinox` (`filter_jit`, `filter_vmap`), `jax` (`grad` for the minimise stationarity residual, `numpy.asarray`, `numpy.max`), `numpy` (`eye`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new nonlinear route is one `NonlinearIntent` case plus one `_ROUTE` row; a new solver on any route is one `NonlinearSolver` member plus one `_SOLVER` row; a new adjoint mode is one `AdjointMode` member plus one `_ADJOINT` row; a study that sweeps many starts sets `NonlinearPolicy.batched` and vmaps through the same `solve`, never a second entry; zero new surface, never a parallel root-finder and minimiser owner, never a per-route helper body, never a boolean toggling two hardcoded solvers.
- Boundary: the numpy central-difference floor runs unconditionally on cp315; `optimistix`/`equinox`/`jaxlib` carry no cp315 wheel, so the Optimistix body is authored against the documented API behind one gated import with a reachable numpy floor for every route. A separate root-finding owner beside a minimisation owner, a per-engine method family, four parallel `_*_receipt` helper bodies, a `getattr`-on-tag dispatch in place of the total `match`/`assert_never`, an `except ImportError` wrapping the whole solve instead of the per-route import-floor arm, a `numpy.asarray` over a JAX pytree, a Python comprehension reading per-row values off a vmapped `Solution.value` instead of `filter_vmap(lift)`, the objective magnitude `abs(fn(v))` masquerading as a minimise residual where the stationarity gradient norm is the witness, the convergence tolerance reused as a `GradientDescent` learning rate, a hardcoded per-route solver, a boolean solver knob, a Python loop over a batched start stack, and a gradient-descent training loop (out of charter) are the deleted forms.

```python signature
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


type Route = Literal["root_find", "minimise", "fixed_point", "least_squares"]


class NonlinearSolver(StrEnum):
    NEWTON = "newton"
    CHORD = "chord"
    BISECTION = "bisection"
    LBFGS = "lbfgs"
    BFGS = "bfgs"
    DFP = "dfp"
    NONLINEAR_CG = "nonlinear_cg"
    NELDER_MEAD = "nelder_mead"
    GRADIENT_DESCENT = "gradient_descent"
    FIXED_POINT_ITERATION = "fixed_point_iteration"
    GAUSS_NEWTON = "gauss_newton"
    LEVENBERG_MARQUARDT = "levenberg_marquardt"
    INDIRECT_LEVENBERG_MARQUARDT = "indirect_levenberg_marquardt"
    DOGLEG = "dogleg"
    DAMPED_NEWTON = "damped_newton"


class AdjointMode(StrEnum):
    IMPLICIT = "implicit"
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"


_TOL = 1e-8
_LR = 1e-3  # GradientDescent step size; the convergence tolerance is never reused as a learning rate


@dataclass(frozen=True, slots=True)
class NonlinearPolicy:
    max_steps: int = 256
    adjoint: AdjointMode = AdjointMode.IMPLICIT
    batched: bool = False  # x0 carries a leading sweep axis vmapped through one compiled solve


@tagged_union(frozen=True)
class NonlinearIntent:
    tag: Route = tag()
    root_find: tuple[object, object, NonlinearSolver, NonlinearPolicy] = case()
    minimise: tuple[object, object, NonlinearSolver, NonlinearPolicy] = case()
    fixed_point: tuple[object, object, NonlinearSolver, NonlinearPolicy] = case()
    least_squares: tuple[object, object, NonlinearSolver, NonlinearPolicy] = case()

    @staticmethod
    def RootFind(
        residual_fn: Callable[[object], object], x0: object,
        solver: NonlinearSolver = NonlinearSolver.NEWTON, policy: NonlinearPolicy = NonlinearPolicy(),
    ) -> "NonlinearIntent":
        return NonlinearIntent(root_find=(residual_fn, x0, solver, policy))

    @staticmethod
    def Minimise(
        objective: Callable[[object], float], x0: object,
        solver: NonlinearSolver = NonlinearSolver.LBFGS, policy: NonlinearPolicy = NonlinearPolicy(),
    ) -> "NonlinearIntent":
        return NonlinearIntent(minimise=(objective, x0, solver, policy))

    @staticmethod
    def FixedPoint(
        step_fn: Callable[[object], object], x0: object,
        solver: NonlinearSolver = NonlinearSolver.FIXED_POINT_ITERATION, policy: NonlinearPolicy = NonlinearPolicy(),
    ) -> "NonlinearIntent":
        return NonlinearIntent(fixed_point=(step_fn, x0, solver, policy))

    @staticmethod
    def NonlinearLeastSquares(
        residual_fn: Callable[[object], object], x0: object,
        solver: NonlinearSolver = NonlinearSolver.LEVENBERG_MARQUARDT, policy: NonlinearPolicy = NonlinearPolicy(),
    ) -> "NonlinearIntent":
        return NonlinearIntent(least_squares=(residual_fn, x0, solver, policy))

    def solve(self) -> "RuntimeRail[SolverReceipt]":
        return boundary(f"solve.{self.tag}", lambda: _dispatch(self))


def _dispatch(intent: NonlinearIntent) -> SolverReceipt:
    match intent:
        case (
            NonlinearIntent(tag="root_find", root_find=(fn, x0, solver, policy))
            | NonlinearIntent(tag="minimise", minimise=(fn, x0, solver, policy))
            | NonlinearIntent(tag="fixed_point", fixed_point=(fn, x0, solver, policy))
            | NonlinearIntent(tag="least_squares", least_squares=(fn, x0, solver, policy))
        ):
            try:
                return _optimistix_receipt(intent.tag, fn, x0, solver, policy)
            except ImportError:
                return _floor_receipt(intent.tag, fn, np.asarray(x0))
        case unreachable:
            assert_never(unreachable)


def _optimistix_receipt(
    tag: Route, fn: Callable[..., object], x0: object, solver: NonlinearSolver, policy: NonlinearPolicy
) -> SolverReceipt:
    import equinox as eqx
    import jax
    import jax.numpy as jnp
    import optimistix as optx

    _SOLVER: FrozenDict[NonlinearSolver, Callable[[], object]] = FrozenDict(
        {
            NonlinearSolver.NEWTON: lambda: optx.Newton(rtol=_TOL, atol=_TOL),
            NonlinearSolver.CHORD: lambda: optx.Chord(rtol=_TOL, atol=_TOL),
            NonlinearSolver.BISECTION: lambda: optx.Bisection(rtol=_TOL, atol=_TOL),
            NonlinearSolver.LBFGS: lambda: optx.LBFGS(rtol=_TOL, atol=_TOL),
            NonlinearSolver.BFGS: lambda: optx.BFGS(rtol=_TOL, atol=_TOL),
            NonlinearSolver.DFP: lambda: optx.DFP(rtol=_TOL, atol=_TOL),
            NonlinearSolver.NONLINEAR_CG: lambda: optx.NonlinearCG(rtol=_TOL, atol=_TOL),
            NonlinearSolver.NELDER_MEAD: lambda: optx.NelderMead(rtol=_TOL, atol=_TOL),
            NonlinearSolver.GRADIENT_DESCENT: lambda: optx.GradientDescent(learning_rate=_LR, rtol=_TOL, atol=_TOL),
            NonlinearSolver.FIXED_POINT_ITERATION: lambda: optx.FixedPointIteration(rtol=_TOL, atol=_TOL),
            NonlinearSolver.GAUSS_NEWTON: lambda: optx.GaussNewton(rtol=_TOL, atol=_TOL),
            NonlinearSolver.LEVENBERG_MARQUARDT: lambda: optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL),
            NonlinearSolver.INDIRECT_LEVENBERG_MARQUARDT: lambda: optx.IndirectLevenbergMarquardt(rtol=_TOL, atol=_TOL),
            NonlinearSolver.DOGLEG: lambda: optx.Dogleg(rtol=_TOL, atol=_TOL),
            NonlinearSolver.DAMPED_NEWTON: lambda: optx.DampedNewtonDescent(rtol=_TOL, atol=_TOL),
        }
    )
    _ROUTE: FrozenDict[Route, tuple[Callable[..., object], Callable[[object], object]]] = FrozenDict(
        {
            "root_find": (optx.root_find, lambda v: optx.max_norm(fn(v))),
            "minimise": (optx.minimise, lambda v: optx.max_norm(jax.grad(fn)(v))),
            "fixed_point": (optx.fixed_point, lambda v: optx.max_norm(fn(v) - v)),
            "least_squares": (optx.least_squares, lambda v: optx.max_norm(fn(v))),
        }
    )
    _ADJOINT: FrozenDict[AdjointMode, Callable[[], object]] = FrozenDict(
        {AdjointMode.IMPLICIT: optx.ImplicitAdjoint, AdjointMode.RECURSIVE_CHECKPOINT: optx.RecursiveCheckpointAdjoint}
    )
    op, lift = _ROUTE[tag]
    instance, adjoint = _SOLVER[solver](), _ADJOINT[policy.adjoint]()
    compiled = eqx.filter_jit(lambda y, _: fn(y))

    if policy.batched:
        starts = jnp.asarray(x0)
        solve_one = eqx.filter_jit(lambda start: op(compiled, instance, start, max_steps=policy.max_steps, adjoint=adjoint, throw=False))
        solutions = eqx.filter_vmap(solve_one, in_axes=0)(starts)
        per_row = eqx.filter_vmap(lift, in_axes=0)(solutions.value)
        residual = float(jnp.max(jnp.asarray(per_row)))
        steps, result = int(jnp.max(jnp.asarray(solutions.stats["num_steps"]))), None
        rank = int(jnp.asarray(solutions.value).size // starts.shape[0])
    else:
        solution = op(compiled, instance, jnp.asarray(x0), max_steps=policy.max_steps, adjoint=adjoint, throw=False)
        value = jnp.asarray(solution.value)
        residual, steps, result = float(lift(value)), int(solution.stats["num_steps"]), solution.result.name
        rank = int(value.size)
    if tag == "least_squares":
        return SolverReceipt.LeastSquares(residual, rank, steps, _TOL, result)
    return SolverReceipt.Iterative(residual, steps, _TOL, result)


def _floor_receipt(tag: Route, fn: Callable[..., object], x0: np.ndarray) -> SolverReceipt:
    basis = np.eye(x0.size)
    probe = (
        np.linalg.norm([float(fn(x0 + 1e-6 * e)) - float(fn(x0 - 1e-6 * e)) for e in basis], np.inf) / 2e-6
        if tag == "minimise"
        else np.linalg.norm(np.asarray(fn(x0)) - (x0 if tag == "fixed_point" else 0.0), np.inf)
    )
    if tag == "least_squares":
        return SolverReceipt.LeastSquares(float(probe), int(x0.size), 0, _TOL, None)
    return SolverReceipt.Iterative(float(probe), 0, _TOL, None)
```

## [03]-[RESEARCH]

- [OPTIMISTIX_SOLVE]: `optimistix` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `root_find`/`minimise`/`fixed_point`/`least_squares` entries, the `Newton`/`Chord`/`Bisection`/`LBFGS`/`BFGS`/`DFP`/`NonlinearCG`/`NelderMead`/`GradientDescent`/`FixedPointIteration`/`GaussNewton`/`LevenbergMarquardt`/`IndirectLevenbergMarquardt`/`Dogleg`/`DampedNewtonDescent` solvers, `max_norm`, `ImplicitAdjoint`, `RecursiveCheckpointAdjoint`, `Solution.stats`, and `Solution.result` spellings verify against the `.api` catalogue under a uv-sync reflection pass on that band. Each entry's `solver=` is the documented algorithm axis, not the entry point, so the `NonlinearSolver` policy plus `_SOLVER` factory table is the canonical selection; `least_squares` upcasts a minimiser member and `root_find`/`fixed_point` upcast a least-squares or minimiser member when the problem class permits. `Newton` is the root default, `LBFGS` the minimise default, `LevenbergMarquardt` the trust-region least-squares default. `throw=False` returns the non-`successful` `Solution.result` so the receipt records the true `RESULTS` verdict instead of raising. `max_steps` and `adjoint` thread from `NonlinearPolicy`; the implicit-function-theorem adjoint is consumed by `solvers/sensitivity.md#SENSITIVITY`.
- [JAX_PYTREE]: `x0` is an arbitrary JAX pytree the Optimistix solver tracks through iteration, so the body lifts and reads back with `jax.numpy.asarray` and contracts residuals with `optimistix.max_norm` rather than `numpy.asarray`, which flattens a pytree and breaks a non-array leaf. `Solution.result` is a `RESULTS` enum whose `.name` is the documented member-name string the `solvers/receipt.md#RECEIPT` `_status` fold maps into `SolveStatus`; the gated route passes the string, never the imported `RESULTS` type, keeping the receipt owner cp315-clean.
- [EQUINOX_TRANSFORM]: `equinox.filter_jit` wraps the residual/objective evaluation so the static (non-array) leaves of the closure skip XLA tracing while array leaves compile, and `equinox.filter_vmap(solve_one, in_axes=0)` maps a batched initial-point stack over the leading axis when a study sets `NonlinearPolicy.batched` to sweep many starts through one compiled solve, folding the per-row residual to its max component and adjudicating through the residual floor since per-row `RESULTS` is not reachable under a vmap trace. Both verify against the `equinox` `.api` catalogue on the gated band; neither re-implements a JAX transform. `filter_vmap(fun, *, in_axes, out_axes, axis_name, axis_size)` is the documented signature.
