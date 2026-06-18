# [PY_COMPUTE_SENSITIVITY]

The one vector-Jacobian-product and sensitivity owner. `Differentiation` discriminates reverse-mode adjoint over the JAX family and forward central-difference over a numpy floor, computing a cotangent-projected gradient or a full Jacobian. Reverse mode reads the implicit-function-theorem adjoint through a Lineax linear solve or an Optimistix nonlinear solve, so a sensitivity through a solved system differentiates the converged solution rather than the iteration trace; the numpy central-difference floor runs unconditionally on cp315. This owner never trains a model, fits a network, or carries a gradient-descent loop.

## [1]-[INDEX]

[SENSITIVITY]: reverse-mode VJP/Jacobian over JAX with an implicit-adjoint solver loop and a finite-difference floor on one `Differentiation` owner.

## [2]-[SENSITIVITY]

- Owner: `Differentiation` — the ONE adjoint/sensitivity owner discriminating `DiffMode` (reverse-mode VJP over JAX / forward finite-difference over numpy); never a parallel autodiff surface beside the solver. A vector-Jacobian product `vjp(fn, x, cotangent, mode)` and a full Jacobian `jacobian(fn, x, mode)` are the two entries, and the mode decides the engine.
- Implicit-adjoint loop: when the differentiated function is itself a solve, reverse mode reads the adjoint through the solver rather than through the iterations. A Lineax `linear_solve` and an Optimistix `root_find`/`least_squares` carry implicit-function-theorem adjoints, so `jax.vjp` over a function that calls them pulls back through the converged solution; `solvers/linear.md#LINEAR` and `solvers/nonlinear.md#NONLINEAR` expose those autodifferentiable solves, and this owner consumes the adjoint they carry.
- Entry: `Differentiation.vjp` returns `RuntimeRail[VjpReceipt]` carrying the cotangent-projected gradient, the mode, the max-component magnitude, and an `exact` flag (reverse-mode exact, finite-difference truncation-bounded). The reverse-mode body calls `jax.vjp(fn, x)` and applies the pullback; the finite-difference body builds the Jacobian by central differences over an `np.eye` perturbation basis and contracts with the cotangent.
- Receipt: `VjpReceipt.contribute` emits one `Receipt.of("emitted", ...)` row; the `exact` flag and the finite-difference step size are the evidence the C# graduation gate reads through the solver `HandoffAxis` case to admit or reject a sensitivity for kernel handoff.
- Packages: `jax` (`vjp`, `jacrev`, `grad`), `optimistix` (the implicit-adjoint `root_find`/`least_squares` consumed through the solver), `lineax` (the implicit-adjoint `linear_solve` consumed through the solver), `numpy` (`eye`, `column_stack`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (the solver evidence the gradient annotates), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new differentiation engine is one `DiffMode` case; a new product (Jacobian-vector, Hessian-vector) is one method on the owner sharing the mode fold; zero new surface.
- Boundary: classical sensitivity and adjoint analysis only — implicit-function-theorem adjoints, finite differences, and Jacobians are in-scope; this owner never trains a model, fits a network, or carries a gradient-descent optimizer loop. `jax`/`jaxlib` carry no cp315 wheel, so the `ReverseVjp` body is authored against the documented API on the JAX floor, and `FiniteDifference` runs unconditionally on the numpy floor.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


@tagged_union(frozen=True)
class DiffMode:
    tag: Literal["reverse_vjp", "finite_difference"] = tag()
    reverse_vjp: tuple[()] = case()
    finite_difference: float = case()

    @staticmethod
    def ReverseVjp() -> "DiffMode":
        return DiffMode(reverse_vjp=())

    @staticmethod
    def FiniteDifference(step: float = 1e-6) -> "DiffMode":
        return DiffMode(finite_difference=step)


class VjpReceipt(Struct, frozen=True):
    mode: str
    gradient: tuple[float, ...]
    max_magnitude: float
    exact: bool

    def contribute(self) -> Receipt:
        facts = {"mode": self.mode, "max_magnitude": f"{self.max_magnitude:.3e}", "exact": str(self.exact)}
        return Receipt.of("emitted", "compute.differentiation", self.mode, facts)


def vjp(
    fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, cotangent: np.ndarray, mode: DiffMode
) -> "RuntimeRail[VjpReceipt]":
    return boundary(f"vjp.{mode.tag}", lambda: _vjp(fn, x, cotangent, mode))


def _vjp(
    fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, cotangent: np.ndarray, mode: DiffMode
) -> VjpReceipt:
    match mode:
        case DiffMode(tag="finite_difference", finite_difference=step):
            grad = _finite_difference_jacobian(fn, x, step).T @ cotangent
            return VjpReceipt("finite-difference", tuple(map(float, grad)), float(np.linalg.norm(grad, np.inf)), False)
        case DiffMode(tag="reverse_vjp"):
            return _reverse_vjp(fn, x, cotangent)
        case unreachable:
            assert_never(unreachable)


def _finite_difference_jacobian(fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, step: float) -> np.ndarray:
    basis = np.eye(x.size)
    with np.errstate(over="raise"):
        columns = [(fn(x + step * e) - fn(x - step * e)) / (2.0 * step) for e in basis]
    return np.column_stack(columns)


def _reverse_vjp(fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, cotangent: np.ndarray) -> VjpReceipt:
    import jax

    _, pullback = jax.vjp(fn, x)
    (grad,) = pullback(cotangent)
    grad = np.asarray(grad)
    return VjpReceipt("reverse-vjp", tuple(map(float, grad)), float(np.linalg.norm(grad, np.inf)), True)
```

## [3]-[RESEARCH]

- [JAX_VJP]: the `jax.vjp`/`jacrev`/`grad` spellings carry the `python_version<'3.15'` marker (no jaxlib cp315 wheel); the reverse-mode body verifies against the `.api` catalogue once the jaxlib wheel resolves. The finite-difference floor runs unconditionally on cp315.
- [IMPLICIT_ADJOINT]: the implicit-function-theorem adjoint is carried by the Lineax `linear_solve` and the Optimistix `root_find`/`least_squares` exposed through `solvers/linear.md#LINEAR` and `solvers/nonlinear.md#NONLINEAR`; this owner reads the adjoint through `jax.vjp` over those solves rather than re-deriving it.
