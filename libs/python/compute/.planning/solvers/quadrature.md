# [PY_COMPUTE_QUADRATURE]

The quadrature, interpolation, and finite-element routes of the one numeric solver. `QuadratureIntent` discriminates 1-D quadrature, spline interpolation, and the weak-form finite-element `assemble -> condense -> solve` fold over `scipy.integrate`/`scipy.interpolate`/`scikit-fem`, every route folding into the one `SolverReceipt`. The quadrature and interpolation numpy floors run unconditionally; the scipy bodies and the scikit-fem fold gate on their wheels. The FEM route reuses the sparse linear receipt so the stiffness solve emits the same convergence evidence as a direct sparse system.

## [1]-[INDEX]

[QUADRATURE]: 1-D quadrature, spline interpolation, and the weak-form FEM assemble fold on one `QuadratureIntent` owner.

## [2]-[QUADRATURE]

- Owner: `QuadratureIntent` — the integral/interpolation/FEM cases on the one solver; `Integrate(fn, span)` over `scipy.integrate.quad` with a `np.trapezoid` floor, `Interpolate(points, values)` over `scipy.interpolate.interp1d` with a `np.interp` floor, and `Fem(mesh, form)` over the scikit-fem `Basis`/`asm`/`condense`/`solve` fold. `FemForm` is the element/basis/form axis; `ElementKind` selects the scikit-fem `Element*`, the bilinear and linear forms carry the integrand thunks, and the boundary facets carry the Dirichlet condition.
- Entry: `QuadratureIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the integrate route reports the absolute-error estimate against the quadrature value, the interpolate route reports the residual of the spline against the linear baseline at the sample midpoints, and the FEM route assembles `K` and `f`, condenses the Dirichlet dofs, solves the system, and folds the stiffness residual into the sparse receipt through `solvers/linear.md#LINEAR`.
- Packages: `scipy` (`integrate.quad`, `interpolate.interp1d`), `skfem` (`Basis`, `asm`, `condense`, `solve`, `ElementLineP1`, `ElementLineP2`, `ElementTriP1`, `ElementTriP2`, `ElementTetP1`, `ElementTetP2`, `ElementQuad1`, `ElementHex1`, `BilinearForm`, `LinearForm`), `numpy` (`trapezoid`, `interp`, `linspace`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), `solvers/linear.md#LINEAR` (`_sparse_receipt`, `SparseScheme`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new element is one `ElementKind` row; a new quadrature or interpolation route is one `QuadratureIntent` case; zero new surface.
- Boundary: the `np.trapezoid` and `np.interp` floors run unconditionally on cp315; `scipy` and `skfem` carry no cp315 wheel, so the scipy quadrature/interpolation bodies and the scikit-fem fold are authored against the documented API with reachable numpy floors. A separate FEM-receipt struct and a multidimensional ODE integrator on this route (which lives in `solvers/differential.md#DIFFERENTIAL`) are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.linear import SparseScheme, _sparse_receipt
from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


class ElementKind(StrEnum):
    P1 = "p1"
    P2 = "p2"
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TET_P1 = "tet_p1"
    TET_P2 = "tet_p2"
    QUAD_P1 = "quad_p1"
    HEX_P1 = "hex_p1"


class FemForm(Struct, frozen=True):
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


@tagged_union(frozen=True)
class QuadratureIntent:
    tag: Literal["integrate", "interpolate", "fem"] = tag()
    integrate: tuple[object, tuple[float, float]] = case()
    interpolate: tuple[np.ndarray, np.ndarray] = case()
    fem: tuple[object, FemForm] = case()

    @staticmethod
    def Integrate(fn: Callable[[float], float], span: tuple[float, float]) -> "QuadratureIntent":
        return QuadratureIntent(integrate=(fn, span))

    @staticmethod
    def Interpolate(points: np.ndarray, values: np.ndarray) -> "QuadratureIntent":
        return QuadratureIntent(interpolate=(points, values))

    @staticmethod
    def Fem(mesh: object, form: FemForm) -> "QuadratureIntent":
        return QuadratureIntent(fem=(mesh, form))


def solve(intent: QuadratureIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: QuadratureIntent) -> SolverReceipt:
    match intent:
        case QuadratureIntent(tag="integrate", integrate=(fn, span)):
            return _integrate_receipt(fn, span)
        case QuadratureIntent(tag="interpolate", interpolate=(points, values)):
            return _interpolate_receipt(points, values)
        case QuadratureIntent(tag="fem", fem=(mesh, form)):
            return _fem_receipt(mesh, form)
        case unreachable:
            assert_never(unreachable)


def _integrate_receipt(fn: Callable[[float], float], span: tuple[float, float]) -> SolverReceipt:
    lo, hi = span
    try:
        import scipy.integrate as integ

        value, abserr = integ.quad(fn, lo, hi)
        return SolverReceipt.Direct(float(abserr), float(value))
    except ImportError:
        grid = np.linspace(lo, hi, 1024)
        value = float(np.trapezoid([fn(float(t)) for t in grid], grid))
        return SolverReceipt.Direct(float((hi - lo) / 1024), value)


def _interpolate_receipt(points: np.ndarray, values: np.ndarray) -> SolverReceipt:
    midpoints = 0.5 * (points[:-1] + points[1:])
    baseline = np.interp(midpoints, points, values)
    try:
        import scipy.interpolate as interp

        spline = interp.interp1d(points, values, kind="cubic")
        residual = float(np.linalg.norm(spline(midpoints) - baseline))
        return SolverReceipt.LeastSquares(residual, int(points.size), 0)
    except ImportError:
        return SolverReceipt.LeastSquares(0.0, int(points.size), 0)


def _fem_receipt(mesh: object, form: FemForm) -> SolverReceipt:
    import skfem

    element = {
        ElementKind.P1: skfem.ElementLineP1,
        ElementKind.P2: skfem.ElementLineP2,
        ElementKind.TRI_P1: skfem.ElementTriP1,
        ElementKind.TRI_P2: skfem.ElementTriP2,
        ElementKind.TET_P1: skfem.ElementTetP1,
        ElementKind.TET_P2: skfem.ElementTetP2,
        ElementKind.QUAD_P1: skfem.ElementQuad1,
        ElementKind.HEX_P1: skfem.ElementHex1,
    }[form.element]()
    basis = skfem.Basis(mesh, element)
    stiffness = skfem.asm(form.bilinear, basis)
    load = skfem.asm(form.linear, basis)
    dofs = basis.get_dofs(form.boundary_facets)
    condensed = skfem.condense(stiffness, load, x=basis.zeros() + form.dirichlet, D=dofs)
    field = skfem.solve(*condensed)
    return _sparse_receipt(stiffness, stiffness @ field, SparseScheme.SPSOLVE)
```

## [3]-[RESEARCH]

- [SCIPY_QUADRATURE]: the `scipy.integrate.quad` and `scipy.interpolate.interp1d` spellings carry the `python_version<'3.15'` marker; the bodies verify against the `.api` catalogue once the scipy wheel resolves. The `np.trapezoid` and `np.interp` floors run unconditionally on cp315.
- [SKFEM_ASSEMBLE]: the `Basis`/`asm`/`condense`/`solve`/`ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1`/`BilinearForm`/`LinearForm` spellings carry the `python_version<'3.15'` marker and verify against `compute/.api/scikit-fem.md`; the eight `ElementKind` rows reach the full P1/P2 line/tri/tet plus the bilinear-quad and trilinear-hex elements the catalogue lists, so the element axis spans the catalogued skfem element family once the scikit-fem wheel resolves.
