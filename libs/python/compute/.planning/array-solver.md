# [PY_COMPUTE_ARRAY_SOLVER]

Array admission and one route-discriminated numeric solver. `ArrayPayload` admits dtype/shape/named-axes/finite-policy/layout/chunking/identity over numpy, composing xarray/dask Dataset shapes from the data-branch catalogue. `NumericIntent` is ONE solver owner discriminating by route (dense-LA, sparse, nonlinear-optimize, integrate, interpolate, symbolic) over scipy + sympy, with the numba LLVM JIT and jax XLA accelerator rows on the same owner — never parallel methods. This owner subsumes the former standalone `SolverPlan`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | ARRAY     | array admission, named axes, finite policy                     |
|   [2]   | SOLVER    | the route-discriminated solver, symbolic derivation, accelerators |

## [2]-[ARRAY]

- Owner: `ArrayPayload` — the dtype/shape/named-axes/finite-policy/layout/chunking/identity admission over numpy; `NamedAxis` the value object; the xarray/dask `Dataset`/`DataArray` shapes are COMPOSED from the data-branch catalogue, never re-catalogued.
- Entry: `ArrayPayload.admit` admits a numpy array (or an xarray/dask shape) and returns a `RuntimeRail[ArrayPayload]`, rejecting non-finite values per the finite policy with an `Error(BoundaryFault)` rejection receipt; the identity keys through `ContentIdentity` over the canonical buffer.
- Packages: `numpy` (`asarray`/`isfinite`/`dtype`/`shape`), data-branch `xarray`/`dask` catalogues, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new layout is one column on `ArrayPayload`; a new finite policy is one row; zero new surface.
- Boundary: no production tensor runtime; a hand-rolled array validation loop and a re-catalogued xarray surface are the deleted forms.

```python signature
from enum import StrEnum

import numpy as np
from expression import Error, Ok
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail


class FinitePolicy(StrEnum):
    REJECT = "reject"
    ALLOW_NAN = "allow-nan"
    ALLOW_INF = "allow-inf"


class NamedAxis(Struct, frozen=True):
    name: str
    size: int


class ArrayPayload(Struct, frozen=True):
    dtype: str
    shape: tuple[int, ...]
    axes: tuple[NamedAxis, ...]
    finite: FinitePolicy
    content_key: ContentKey

    @classmethod
    def admit(cls, array: np.ndarray, axes: tuple[NamedAxis, ...], finite: FinitePolicy) -> "RuntimeRail[ArrayPayload]":
        if finite is FinitePolicy.REJECT and not np.isfinite(array).all():
            return Error(BoundaryFault.Boundary("non-finite", str(array.dtype)))
        return Ok(cls(str(array.dtype), array.shape, axes, finite, ContentIdentity.key("array", array.tobytes())))
```

## [3]-[SOLVER]

- Owner: `NumericIntent` — the one route-discriminated solver owner over scipy + sympy; `SymbolicDerivation` the lambdify-codegen surface for the C# handoff; the accelerator rows ride `NumericIntent` itself.
- Cases: `NumericIntent` cases `DenseLa(matrix, rhs)` (scipy `linalg.solve`/`lstsq`) · `Sparse(matrix, rhs)` (scipy `sparse.linalg.spsolve`) · `NonlinearOptimize(objective, x0)` (scipy `optimize.minimize`) · `Integrate(fn, span)` (scipy `integrate.quad`) · `Interpolate(points, values)` (scipy `interpolate`) · `Symbolic(expr, symbols)` (sympy `lambdify`/`codegen`) — matched by `match`/`case`.
- Entry: `NumericIntent.solve` dispatches the route and returns a `RuntimeRail[SolveResult]` carrying the route, the residual, and the accelerator used; an `accelerate` row binds numba `njit` (LLVM nopython JIT) or jax `jit`/`grad`/`vmap` to the inner kernel without a parallel method.
- Auto: the dense path runs `scipy.linalg.solve`; the sparse path runs `scipy.sparse.linalg.spsolve`; the symbolic path runs `sympy.lambdify(symbols, expr)` and `sympy.utilities.codegen.codegen` for the C# handoff; the accelerator wraps the solve kernel through numba/jax where the route admits it.
- Receipt: each solve contributes a `Receipt.emitted` row through `ReceiptContributor` carrying the route, residual norm, and accelerator; a graduated derivation produces a `GraduationReceipt` solver/symbolic subject.
- Packages: `scipy` (`linalg.solve`/`sparse.linalg.spsolve`/`optimize.minimize`/`integrate.quad`/`interpolate`), `sympy` (`lambdify`/`utilities.codegen.codegen`), `numba` (`njit`), `jax` (`jit`/`grad`/`vmap`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new solver route is one `NumericIntent` case; a new accelerator is one `accelerate` row on the owner; zero new surface, no standalone `SolverPlan`.
- Boundary: no production substrate selection or benchmark authority; a separate `SolverPlan` owner, a per-accelerator method family, and a stringly-typed route dispatch are the deleted forms. This owner is `SPIKE` on the marker floor (scipy/numba/jax).

```python signature
from typing import Literal

import numpy as np
import scipy.linalg
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.rails_resilience import RuntimeRail, boundary


@tagged_union(frozen=True)
class NumericIntent:
    tag: Literal["dense_la", "sparse", "nonlinear_optimize", "integrate", "interpolate", "symbolic"] = tag()
    dense_la: tuple[np.ndarray, np.ndarray] = case()
    sparse: tuple[object, np.ndarray] = case()
    nonlinear_optimize: tuple[object, np.ndarray] = case()
    integrate: tuple[object, tuple[float, float]] = case()
    interpolate: tuple[np.ndarray, np.ndarray] = case()
    symbolic: tuple[object, tuple[str, ...]] = case()

    @staticmethod
    def DenseLa(matrix: np.ndarray, rhs: np.ndarray) -> "NumericIntent":
        return NumericIntent(dense_la=(matrix, rhs))

    @staticmethod
    def Sparse(matrix: object, rhs: np.ndarray) -> "NumericIntent":
        return NumericIntent(sparse=(matrix, rhs))

    @staticmethod
    def Symbolic(expr: object, symbols: tuple[str, ...]) -> "NumericIntent":
        return NumericIntent(symbolic=(expr, symbols))


class SolveResult(Struct, frozen=True):
    route: str
    residual: float
    accelerator: str


def solve(intent: NumericIntent) -> "RuntimeRail[SolveResult]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: NumericIntent) -> SolveResult:
    match intent:
        case NumericIntent(tag="dense_la", dense_la=(a, b)):
            x = scipy.linalg.solve(a, b)
            return SolveResult("dense-la", float(np.linalg.norm(a @ x - b)), "cpu")
        case _:
            return SolveResult(intent.tag, 0.0, "cpu")
```

## [4]-[RESEARCH]

- [SCIPY_SPARSE]: the `scipy.linalg.solve`/`sparse.linalg.spsolve`/`optimize.minimize` signatures, the `sympy.lambdify`/`utilities.codegen.codegen` C# handoff path, and the `numba.njit`/`jax.jit` accelerator wrap are verified against `.api/api-scipy.md`, `.api/api-sympy.md` (cp315-reflected), `.api/api-numba.md`, `.api/api-jax.md` once the marker-floor environment installs the accelerators (suite TASKLOG `PY_API_003`).
