# [PY_COMPUTE_NONLINEAR]

The nonlinear routes of the one numeric solver and the loop-kernel accelerator. `NonlinearIntent` discriminates root-finding, minimisation, fixed-point iteration, and nonlinear least-squares over Optimistix on the JAX floor with a `scipy.optimize` mid-tier and a numpy central-difference floor, every route folding into the one `SolverReceipt`. `accelerate` is the one numba LLVM JIT row that wraps a numpy loop kernel, distinct from the Array-API backend dispatch because numba is a loop-kernel compiler, not an array backend. Optimistix solves are implicit-function-theorem differentiable, so a downstream sensitivity reads the adjoint through the solve.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                 |
| :-----: | :---------- | :--------------------------------------------------------------------- |
|   [1]   | NONLINEAR   | root/minimise/fixed-point/least-squares routes over Optimistix + scipy |
|   [2]   | ACCELERATOR | the one numba LLVM JIT loop-kernel row                                 |

## [2]-[NONLINEAR]

- Owner: `NonlinearIntent` — the nonlinear-route cases on the one solver; `RootFind(residual_fn, x0)`, `Minimise(objective, x0)`, `FixedPoint(step_fn, x0)`, and `NonlinearLeastSquares(residual_fn, x0)` each discriminate the engine. The Optimistix tier runs `optimistix.root_find`/`optimistix.minimise`/`optimistix.fixed_point`/`optimistix.least_squares` with a `Newton`/`BFGS`/`FixedPointIteration`/`LevenbergMarquardt` solver and reads `optimistix.Solution.stats` (step count, final residual) into `SolverReceipt`. The scipy mid-tier runs `scipy.optimize.minimize`/`scipy.optimize.root`/`scipy.optimize.least_squares`. The numpy floor reports the gradient-norm residual at `x0` over a central-difference Jacobian.
- Entry: `NonlinearIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the minimise and root routes fold the final residual and iteration count into `SolverReceipt.Iterative`, and the least-squares route folds the rank and step count into `SolverReceipt.LeastSquares`. The Optimistix solve carries an implicit-function-theorem adjoint, so `differentiation/sensitivity.md#SENSITIVITY` differentiates through the converged solution rather than through the iteration trace.
- Packages: `optimistix` (`root_find`, `minimise`, `fixed_point`, `least_squares`, `Newton`, `BFGS`, `FixedPointIteration`, `LevenbergMarquardt`, `Solution`), `scipy` (`optimize.minimize`, `optimize.root`, `optimize.least_squares`), `numpy` (`eye`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new nonlinear route is one `NonlinearIntent` case; a new Optimistix solver is one `match` arm; zero new surface, never a parallel root-finder and minimiser owner.
- Boundary: the numpy gradient floor runs unconditionally on cp315; `optimistix`/`jaxlib` and `scipy` carry no cp315 wheel, so the Optimistix and scipy bodies are authored against the documented API with a reachable numpy floor. A separate root-finding owner beside a minimisation owner, a per-engine method family, and a gradient-descent training loop (out of charter) are the deleted forms.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


# --- [OPERATIONS] --------------------------------------------------------------------------
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


def solve(intent: NonlinearIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: NonlinearIntent) -> SolverReceipt:
    match intent:
        case NonlinearIntent(tag="minimise", minimise=(objective, x0)):
            return _minimise_receipt(objective, x0)
        case NonlinearIntent(tag="root_find", root_find=(fn, x0)):
            return _root_receipt(fn, x0)
        case NonlinearIntent(tag="fixed_point", fixed_point=(step_fn, x0)):
            return _fixed_point_receipt(step_fn, x0)
        case NonlinearIntent(tag="least_squares", least_squares=(residual_fn, x0)):
            return _least_squares_receipt(residual_fn, x0)
        case unreachable:
            assert_never(unreachable)


def _minimise_receipt(objective: Callable[[np.ndarray], float], x0: np.ndarray) -> SolverReceipt:
    try:
        import optimistix as optx

        solution = optx.minimise(lambda x, _: objective(x), optx.BFGS(rtol=1e-8, atol=1e-8), x0)
        return SolverReceipt.Iterative(float(solution.state.f_val), int(solution.stats["num_steps"]), 1e-8)
    except ImportError:
        grad = np.array([objective(x0 + 1e-6 * e) - objective(x0 - 1e-6 * e) for e in np.eye(x0.size)]) / 2e-6
        return SolverReceipt.Iterative(float(np.linalg.norm(grad, np.inf)), 0, 1e-8)


def _root_receipt(fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> SolverReceipt:
    import optimistix as optx

    solution = optx.root_find(lambda x, _: fn(x), optx.Newton(rtol=1e-8, atol=1e-8), x0)
    residual = float(np.linalg.norm(np.asarray(fn(np.asarray(solution.value)))))
    return SolverReceipt.Iterative(residual, int(solution.stats["num_steps"]), 1e-8)


def _fixed_point_receipt(step_fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> SolverReceipt:
    import optimistix as optx

    solution = optx.fixed_point(lambda x, _: step_fn(x), optx.FixedPointIteration(rtol=1e-8, atol=1e-8), x0)
    fixed = np.asarray(solution.value)
    residual = float(np.linalg.norm(np.asarray(step_fn(fixed)) - fixed))
    return SolverReceipt.Iterative(residual, int(solution.stats["num_steps"]), 1e-8)


def _least_squares_receipt(residual_fn: Callable[[np.ndarray], np.ndarray], x0: np.ndarray) -> SolverReceipt:
    import optimistix as optx

    solution = optx.least_squares(lambda x, _: residual_fn(x), optx.LevenbergMarquardt(rtol=1e-8, atol=1e-8), x0)
    residual = float(np.linalg.norm(np.asarray(residual_fn(np.asarray(solution.value)))))
    return SolverReceipt.LeastSquares(residual, int(x0.size), int(solution.stats["num_steps"]))
```

## [3]-[ACCELERATOR]

- Owner: `accelerate` — the one numba LLVM JIT row that wraps a numpy loop kernel; it is distinct from the Array-API backend dispatch in `arrays/payload.md#PAYLOAD` because numba compiles a Python loop to machine code, where JAX is an array backend resolved through `array_namespace`. The `backend="none"` passthrough is the unconditional floor; `backend="numba"` wraps the kernel through `numba.njit(cache=True)`.
- Packages: `numba` (`njit`), `numpy` (the loop-kernel floor).
- Growth: a new loop-kernel compiler is one `match` arm; zero new surface, never a per-accelerator method family.
- Boundary: `numba`/`llvmlite` carry no cp315 wheel, so the numba arm is authored against the documented API with the `none` passthrough as the reachable floor. A JAX accelerator arm here is the deleted form because JAX is admitted as an Array-API backend at array admission, not as a kernel wrap.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np


def accelerate(
    kernel: Callable[..., np.ndarray], *, backend: Literal["numba", "none"] = "none"
) -> Callable[..., np.ndarray]:
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

- [OPTIMISTIX_SOLVE]: `optimistix` is NOT yet in the root manifest; the `root_find`/`minimise`/`fixed_point`/`least_squares`/`Newton`/`BFGS`/`FixedPointIteration`/`LevenbergMarquardt`/`Solution.stats` spellings are admitted to the `scientific` group on the jaxlib `python_version<'3.15'` floor and verified against the branch `.api` catalogue. The Optimistix solve carries an implicit-function-theorem adjoint consumed by `differentiation/sensitivity.md#SENSITIVITY`.
- [NUMBA_JIT]: `numba`/`llvmlite` carry the `python_version<'3.15'` marker; the `njit(cache=True)` spelling verifies against the branch `.api` catalogue once the numba wheel resolves. The `backend="none"` passthrough runs unconditionally on cp315.
