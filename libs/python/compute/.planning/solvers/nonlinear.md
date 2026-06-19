# [PY_COMPUTE_NONLINEAR]

The nonlinear routes of the one numeric solver and the loop-kernel accelerator. `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over Optimistix on the JAX floor with a numpy central-difference floor reachable for every route, all four sharing one table-driven Optimistix dispatch and every route folding into the one `SolverReceipt`. `accelerate` is the one numba LLVM JIT row that wraps a numpy loop kernel, distinct from the Array-API backend dispatch because numba is a loop-kernel compiler, not an array backend. Optimistix solves are implicit-function-theorem differentiable, so a downstream sensitivity reads the adjoint through the solve.

## [1]-[INDEX]

- [1]-[NONLINEAR]: root/minimise/fixed-point/least-squares routes over Optimistix + scipy
- [2]-[ACCELERATOR]: the one numba LLVM JIT loop-kernel row

## [2]-[NONLINEAR]

- Owner: `NonlinearIntent` — the nonlinear-route cases on the one solver; `RootFind(residual_fn, x0)`, `Minimise(objective, x0)`, `FixedPoint(step_fn, x0)`, and `NonlinearLeastSquares(residual_fn, x0)` each discriminate the engine. The Optimistix tier runs `optimistix.root_find`/`optimistix.minimise`/`optimistix.fixed_point`/`optimistix.least_squares` with a `Newton`/`BFGS`/`FixedPointIteration`/`LevenbergMarquardt` solver and reads `optimistix.Solution.stats` (step count, final residual) into `SolverReceipt`. The four routes share one Optimistix dispatch keyed on the tag inside `_optimistix_receipt` — the operation, the solver, and the residual probe are the row, built behind the gated import because the solver instances reference the resolved `optx` — never four parallel helper bodies. The numpy floor reports the gradient-norm or step residual at `x0` over a central-difference probe and is reachable for every route, so a cp315 run without the optimistix wheel never returns `Error(Import)`.
- Entry: `NonlinearIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the minimise, root, and fixed-point routes fold the final residual and iteration count into `SolverReceipt.Iterative`, and the least-squares route folds the rank and step count into `SolverReceipt.LeastSquares`. The Optimistix solve carries an implicit-function-theorem adjoint, so `solvers/sensitivity.md#SENSITIVITY` differentiates through the converged solution rather than through the iteration trace.
- Packages: `optimistix` (`root_find`, `minimise`, `fixed_point`, `least_squares`, `Newton`, `BFGS`, `FixedPointIteration`, `LevenbergMarquardt`, `Solution`), `numpy` (`eye`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new nonlinear route is one `NonlinearIntent` case plus one row in the `_optimistix_receipt` route table; a new Optimistix solver is one cell on the row; zero new surface, never a parallel root-finder and minimiser owner, never a per-route helper body.
- Boundary: the numpy central-difference floor runs unconditionally on cp315; `optimistix`/`jaxlib` carry no cp315 wheel, so the Optimistix body is authored against the documented API behind one gated import with a reachable numpy floor for every route. A separate root-finding owner beside a minimisation owner, a per-engine method family, four parallel `_*_receipt` helper bodies, and a gradient-descent training loop (out of charter) are the deleted forms.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


@tagged_union(frozen=True)
class NonlinearIntent:
    tag: Literal["root_find", "minimise", "fixed_point", "least_squares"] = tag()
    root_find: tuple[object, np.ndarray] = case()
    minimise: tuple[object, np.ndarray] = case()
    fixed_point: tuple[object, np.ndarray] = case()
    least_squares: tuple[object, np.ndarray] = case()

    @staticmethod
    def RootFind(residual_fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> "NonlinearIntent":
        return NonlinearIntent(root_find=(residual_fn, x0))

    @staticmethod
    def Minimise(objective: Callable[[np.ndarray], float], x0: np.ndarray) -> "NonlinearIntent":
        return NonlinearIntent(minimise=(objective, x0))

    @staticmethod
    def FixedPoint(step_fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> "NonlinearIntent":
        return NonlinearIntent(fixed_point=(step_fn, x0))

    @staticmethod
    def NonlinearLeastSquares(residual_fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> "NonlinearIntent":
        return NonlinearIntent(least_squares=(residual_fn, x0))


_TOL = 1e-8


def solve(intent: NonlinearIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: NonlinearIntent) -> SolverReceipt:
    match intent:
        case NonlinearIntent(tag="root_find", root_find=(fn, x0)):
            pass
        case NonlinearIntent(tag="minimise", minimise=(fn, x0)):
            pass
        case NonlinearIntent(tag="fixed_point", fixed_point=(fn, x0)):
            pass
        case NonlinearIntent(tag="least_squares", least_squares=(fn, x0)):
            pass
        case unreachable:
            assert_never(unreachable)
    try:
        return _optimistix_receipt(intent.tag, fn, np.asarray(x0))
    except ImportError:
        return _floor_receipt(intent.tag, fn, np.asarray(x0))


def _optimistix_receipt(tag: str, fn: Callable[..., object], x0: np.ndarray) -> SolverReceipt:
    import optimistix as optx

    op, solver, lift = {
        "root_find": (optx.root_find, optx.Newton(rtol=_TOL, atol=_TOL), lambda v: np.linalg.norm(np.asarray(fn(v)))),
        "minimise": (optx.minimise, optx.BFGS(rtol=_TOL, atol=_TOL), lambda v: abs(float(fn(v)))),
        "fixed_point": (optx.fixed_point, optx.FixedPointIteration(rtol=_TOL, atol=_TOL), lambda v: np.linalg.norm(np.asarray(fn(v)) - v)),
        "least_squares": (optx.least_squares, optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL), lambda v: np.linalg.norm(np.asarray(fn(v)))),
    }[tag]
    solution = op(lambda x, _: fn(x), solver, x0)
    value = np.asarray(solution.value)
    residual, steps = float(lift(value)), int(solution.stats["num_steps"])
    return SolverReceipt.LeastSquares(residual, int(x0.size), steps) if tag == "least_squares" else SolverReceipt.Iterative(residual, steps, _TOL)


def _floor_receipt(tag: str, fn: Callable[..., object], x0: np.ndarray) -> SolverReceipt:
    basis = np.eye(x0.size)
    probe = (
        np.linalg.norm([float(fn(x0 + 1e-6 * e)) - float(fn(x0 - 1e-6 * e)) for e in basis], np.inf) / 2e-6
        if tag == "minimise"
        else np.linalg.norm(np.asarray(fn(x0)) - (x0 if tag == "fixed_point" else 0.0), np.inf)
    )
    return SolverReceipt.LeastSquares(float(probe), int(x0.size), 0) if tag == "least_squares" else SolverReceipt.Iterative(float(probe), 0, _TOL)
```

## [3]-[ACCELERATOR]

- Owner: `accelerate` — the one numba LLVM JIT row that wraps a numpy loop kernel; it is distinct from the Array-API backend dispatch in `numerics/array.md#PAYLOAD` because numba compiles a Python loop to machine code, where JAX is an array backend resolved through `array_namespace`. The `backend="none"` passthrough is the unconditional floor; `backend="numba"` wraps the kernel through `numba.njit(cache=True)`.
- Packages: `numba` (`njit`), `numpy` (the loop-kernel floor).
- Growth: a new loop-kernel compiler is one `match` arm; zero new surface, never a per-accelerator method family.
- Boundary: `numba`/`llvmlite` carry no cp315 wheel, so the numba arm is authored against the documented API with the `none` passthrough as the reachable floor. A JAX accelerator arm here is the deleted form because JAX is admitted as an Array-API backend at array admission, not as a kernel wrap.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np


def accelerate(kernel: Callable[..., np.ndarray], *, backend: Literal["numba", "none"] = "none") -> Callable[..., np.ndarray]:
    match backend:
        case "numba":
            import numba

            return numba.njit(cache=True)(kernel)
        case "none":
            return kernel
        case unreachable:
            assert_never(unreachable)
```

## [4]-[RESEARCH]

- [OPTIMISTIX_SOLVE]: `optimistix` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `root_find`/`minimise`/`fixed_point`/`least_squares`/`Newton`/`BFGS`/`FixedPointIteration`/`LevenbergMarquardt`/`Solution.stats` spellings verify against the `.api` catalogue under a uv-sync reflection pass on that band. The Optimistix solve carries an implicit-function-theorem adjoint consumed by `solvers/sensitivity.md#SENSITIVITY`.
- [NUMBA_JIT]: `numba`/`llvmlite` carry the `python_version<'3.15'` marker; the `njit(cache=True)` spelling verifies against the `.api` catalogue once the numba wheel resolves. The `backend="none"` passthrough runs unconditionally on cp315.
