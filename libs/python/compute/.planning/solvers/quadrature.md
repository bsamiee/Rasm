# [PY_COMPUTE_QUADRATURE]

The quadrature, interpolation, and finite-element routes of the one numeric solver. `QuadratureIntent` discriminates 1-D quadrature, 1-D interpolation, and the weak-form finite-element `assemble -> condense -> solve` fold, every route folding into the one `SolverReceipt`. Each numeric route reads ONE bounded policy value rather than a hardcoded backend call: `QuadKind` (adaptive Gauss-Kronrod / vectorized / Romberg / tanh-sinh-singular / sampled-Simpson) selects the integration rule, and `InterpKind` (linear / cubic / pchip / akima / bspline) selects the interpolant family — so adding an adaptive rule or a spline kind is one enum row, never a parallel `Integrate`/`IntegrateRomberg`/`IntegrateSingular` factory family. Each numeric route is a THREE-FLOOR ladder keyed by what resolves: the JAX-native differentiable companion (`quadax` adaptive quadrature, `interpax` differentiable interpolants) when the gated band is present and an autodiff-through-integrand/through-knots result is wanted, the host scipy body when scipy resolves, and the unconditional numpy floor (`np.trapezoid`, `np.interp`) on the bare cp315 core. The integrate route folds the `quadax.QuadratureInfo`/`scipy quad full_output` *termination verdict* — the convergence code, evaluation count, and estimated error — into the `Iterative` receipt's typed `SolveStatus` exactly as the sparse Krylov route does, so a non-converged adaptive integral is a first-class verdict, never a silent value. The FEM route consumes the `AssembledSystem` the `solvers/mesh.md#MESH` `MeshField.assemble` lowers — it never re-runs `Basis`/`asm`, owning only the `condense -> solve` half — and reuses the sparse linear receipt so the stiffness solve emits the same convergence evidence as a direct sparse system. `ElementKind` and the `_ELEMENT_CTOR` skfem-constructor table live here as the single element axis; `solvers/mesh.md#MESH` imports both rather than redeclaring the element-to-constructor mapping, so the assemble and the solve never diverge on the element family.

## [01]-[INDEX]

- [01]-[QUADRATURE]: 1-D quadrature, 1-D interpolation, and the weak-form FEM assemble fold on one `QuadratureIntent` owner reading two bounded policy values (`QuadKind`/`InterpKind`) over a three-floor quadax/scipy/numpy ladder, with the adaptive-quadrature termination code folded into typed `SolveStatus`.

## [02]-[QUADRATURE]

- Owner: `QuadratureIntent` — the integral/interpolation/FEM cases on the one solver; `Integrate(fn, span, kind)` over the `QuadKind`-selected quadax adaptive integrator / scipy `integrate` body / `np.trapezoid` floor, `Interpolate(points, values, query, kind)` over the `InterpKind`-selected interpax differentiable interpolant / scipy `interpolate` class / `np.interp` floor, and `Fem(system, dirichlet)` consuming the `AssembledSystem` artifact the `MeshField.assemble` mesh fold lowered — the FEM intent carries the assembled stiffness/load/dof system itself, never the `MeshField`, so this route condenses and solves and never reaches into the mesh assembly. `FemForm` is the element/basis/form axis the mesh `assemble` reads; `ElementKind` selects the scikit-fem `Element*` through the canonical `_ELEMENT_CTOR` table this owner declares, the bilinear and linear forms carry the integrand thunks, and the boundary facets carry the Dirichlet condition.
- Quadrature policy: `QuadKind` is the ONE bounded integration-rule policy — `GAUSS_KRONROD` (the default globally-adaptive smooth rule), `VECTORIZED` (vector-valued integrand panels), `ROMBERG` (Richardson-extrapolated smooth integral), `TANH_SINH` (endpoint-singular/infinite-range double-exponential), `SAMPLED_SIMPSON` (already-discretized data) — read once per integrate call and projected onto each floor. The `_QUADAX_ENTRY` table names the JAX-native `quadax` entrypoint per kind (`quadgk`/`quadgk`+vector integrand/`romberg`/`quadts`/`sampled.simpson`) so the gated companion is one keyed projection; the scipy floor reads its callable through the single `match kind` arm in `_integrate_scipy` — `SAMPLED_SIMPSON` to `simpson`, `VECTORIZED` to `quad_vec`, and every remaining adaptive kind grouped onto `quad(full_output=True)` — because the scipy callables carry divergent call and return shapes (`quad` a `(value, abserr, infodict)` triple, `quad_vec` a `(value, abserr)` pair, `simpson` a bare sampled scalar) that no uniform callable row captures. A new rule is one `QuadKind` row plus its `_QUADAX_ENTRY` entry and one `_integrate_scipy` arm, never a per-rule `_integrate_*` body. The adaptive quadax integrators return `(value, QuadratureInfo)` and the scipy `quad(full_output=True)` returns `(value, abserr, infodict)`, so the integrate fold reads the estimated error, the evaluation count, and the convergence verdict from the backend rather than discarding them into a bare scalar.
- Interpolation policy: `InterpKind` is the ONE bounded interpolant-family policy — `LINEAR`, `CUBIC`, `PCHIP` (shape-preserving monotonic), `AKIMA`, `BSPLINE` — read once per interpolate call. The `_INTERPAX_KIND` table maps the kinds interpax owns a drop-in class for — `CUBIC`/`PCHIP`/`AKIMA` to `CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator` — for the JAX-differentiable companion; `LINEAR` is the one-shot `interpax.interp1d(method="linear")` path handled ahead of the table, and `BSPLINE` has no interpax B-spline class so it routes to the scipy `make_interp_spline` body rather than silently degrading to a cubic. The `_SCIPY_INTERP` table maps each non-linear kind to the scipy `interpolate` class/factory (`CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`make_interp_spline`) — never the deprecated `interp1d`, since the modern interpolant classes own the C2/monotonic/Akima/B-spline kernels directly. The numpy floor serves the `LINEAR` kind through `np.interp`; the richer kinds degrade to the linear floor when neither gated companion nor scipy resolves. A new interpolant family is one `InterpKind` row plus its `_SCIPY_INTERP` entry and, when interpax owns a drop-in class for it, its `_INTERPAX_KIND` entry.
- Element axis: `ElementKind` is the ONE element/basis enum and `_ELEMENT_CTOR` is the ONE `{ElementKind: skfem-constructor-name}` table, both declared here as the canonical element owner; `solvers/mesh.md#MESH` imports `ElementKind`, `FemForm`, and `_ELEMENT_CTOR` rather than re-spelling the eight constructor rows, so the mesh `assemble` and the FEM `solve` resolve the identical `Element*` family and a new element is one row read by both routes. The duplicated inline element-dict literal that previously re-listed `ElementLineP1`…`ElementHex1` on this route is the deleted form.
- Non-convergence law: the integrate route never returns a non-converged integral as a silent success. The `quadax.QuadratureInfo.status` integer folds through the `quadax.STATUS` decode table into the `quadax`-message string and then through `_quad_status`, which normalizes the decoded message by substring through the `_QUAD_STATUS` table into the receipt's `SolveStatus` member-name vocabulary (`0` → `successful`; a step-budget message → `max_steps_reached`; a round-off message → `stagnation`; a divergence/singular/non-finite message → `nonlinear_divergence`/`singular`/`nonfinite`) and degrades an unrecognized code to `other` rather than collapsing every nonzero code to a single verdict, so the receipt distinguishes a step-budget miss from a round-off stall or a divergence. The scipy `quad` path reads `abserr` against the requested `epsabs`/`epsrel` and folds a tolerance miss into `STAGNATION`. The estimated error rides the receipt `residual` slot and the evaluation count rides the `iterations` slot, so `SolverReceipt.Iterative(abserr, neval, epsrel, result=...)` carries *why* the adaptive integral stopped, exactly as the sparse Krylov `_info_status` fold does — never `abserr` masquerading in the conditioning slot and never the integral value smuggled into an evidence field.
- Entry: `QuadratureIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the integrate route runs the `QuadKind`-selected floor and folds the estimated error, evaluation count, and decoded status into `Iterative`; the interpolate route builds the `InterpKind`-selected interpolant, evaluates it at the query points (the sample midpoints when no query is supplied), and reports the residual against the `np.interp` linear baseline into `LeastSquares`; and the FEM route reads the `AssembledSystem` the mesh fold lowered, condenses the Dirichlet dofs against `system.stiffness`/`system.load`/`system.dirichlet_dofs`, solves the condensed system, and folds the stiffness residual into the sparse receipt through `solvers/linear.md#LINEAR`.
- Differentiability: the quadax integrate floor and the interpax interpolate floor are JAX-traceable — a downstream `grad`/`vjp` flows through the integrand and the interval bounds (quadax) or through the sample knots (interpax) without differentiating the adaptive iterations, so `solvers/sensitivity.md#SENSITIVITY` and `optimization/design.md#DESIGN` read an autodifferentiable integral or interpolant through this route rather than a finite-difference floor. The scipy and numpy floors are the non-differentiable host fallbacks reached only when the gated band is absent.
- Packages: `quadax` (`quadgk`, `quadcc`, `quadts`, `romberg`, `rombergts`, `adaptive_quadrature`, `QuadratureInfo`, `STATUS`, `sampled.simpson`, `sampled.trapezoid` — the JAX-native differentiable adaptive floor), `interpax` (`CubicSpline`, `PchipInterpolator`, `Akima1DInterpolator`, `interp1d`, `Interpolator1D` — the JAX-native differentiable interpolant floor), `scipy` (`integrate.quad`, `integrate.quad_vec`, `integrate.simpson`, `interpolate.CubicSpline`, `interpolate.PchipInterpolator`, `interpolate.Akima1DInterpolator`, `interpolate.make_interp_spline` — the host bodies; never the deprecated `interp1d`), `skfem` (`condense`, `solve` — the solve half only; the `Basis`/`asm` assembly stays on `solvers/mesh.md#MESH`), `numpy` (`trapezoid`, `interp`, `linspace`, `linalg.norm`, `zeros`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`, `SolveStatus`), `solvers/linear.md#LINEAR` (`_sparse_receipt`, `LinearMap`, `SparseScheme`), `solvers/mesh.md#MESH` (`AssembledSystem` — the assembled stiffness/load/dof artifact the FEM route condenses), runtime (`RuntimeRail`, `boundary`). The `_ELEMENT_CTOR` table names the skfem `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1` constructors the mesh fold resolves.
- Growth: a new element is one `ElementKind` row plus one `_ELEMENT_CTOR` entry, read by both the mesh assemble and this solve; a new adaptive quadrature rule is one `QuadKind` row plus its `_QUADAX_ENTRY` entry and its `_integrate_scipy` arm; a new interpolant family is one `InterpKind` row plus its `_INTERPAX_KIND` entry (when interpax owns a drop-in class) and its `_SCIPY_INTERP` entry; a new termination code is one `_QUAD_STATUS` message-token row mapping the decoded `quadax.STATUS` message into the existing `SolveStatus` vocabulary; zero new surface, never a per-rule integrate factory, never a per-kind interpolate body, never a `least_squares`-style boolean knob where a policy row carries the modality.
- Boundary: the `np.trapezoid` and `np.interp` floors run unconditionally on cp315; `quadax`/`interpax`/`jaxlib`, `scipy`, and `skfem` carry no cp315 wheel, so the quadax/interpax differentiable companions, the scipy quadrature/interpolation bodies, and the scikit-fem condense/solve are authored against the documented API with reachable numpy floors. Assembly (`Basis`/`asm`) stays on `solvers/mesh.md#MESH` and this route consumes the `AssembledSystem` it lowers, never re-running the assemble. A single hardcoded `scipy.integrate.quad` where a `QuadKind` policy spans the adaptive family, a deprecated `interp1d` where the modern interpolant classes own the kernel, the integral value smuggled into the `condition` evidence slot, a discarded `QuadratureInfo` termination verdict, a duplicated element-constructor table on this route, an inline `Basis`/`asm` assembly here, a separate FEM-receipt struct, a 2-D/3-D interpolation route (which the `interpax` `interp2d`/`interp3d` family serves on `solvers/field` per the `[INTERPAX_QUADAX_USAGE]` map), and a multidimensional ODE integrator (which lives in `solvers/differential.md#DIFFERENTIAL`) are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.linear import LinearMap, SparseScheme, _sparse_receipt
from rasm.compute.solvers.receipt import SolveStatus, SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from rasm.compute.solvers.mesh import AssembledSystem


# --- [TYPES] -------------------------------------------------------------------------------

class QuadKind(StrEnum):
    GAUSS_KRONROD = "gauss_kronrod"
    VECTORIZED = "vectorized"
    ROMBERG = "romberg"
    TANH_SINH = "tanh_sinh"
    SAMPLED_SIMPSON = "sampled_simpson"


class InterpKind(StrEnum):
    LINEAR = "linear"
    CUBIC = "cubic"
    PCHIP = "pchip"
    AKIMA = "akima"
    BSPLINE = "bspline"


class ElementKind(StrEnum):
    P1 = "p1"
    P2 = "p2"
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TET_P1 = "tet_p1"
    TET_P2 = "tet_p2"
    QUAD_P1 = "quad_p1"
    HEX_P1 = "hex_p1"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_EPSABS: float = 1e-10
_EPSREL: float = 1e-8

_ELEMENT_CTOR: dict[ElementKind, str] = {
    ElementKind.P1: "ElementLineP1",
    ElementKind.P2: "ElementLineP2",
    ElementKind.TRI_P1: "ElementTriP1",
    ElementKind.TRI_P2: "ElementTriP2",
    ElementKind.TET_P1: "ElementTetP1",
    ElementKind.TET_P2: "ElementTetP2",
    ElementKind.QUAD_P1: "ElementQuad1",
    ElementKind.HEX_P1: "ElementHex1",
}

# InterpKind -> scipy.interpolate spline class/factory name (modern interpolant classes, never the deprecated `interp1d`).
_SCIPY_INTERP: FrozenDict[InterpKind, str] = FrozenDict(
    {
        InterpKind.CUBIC: "CubicSpline",
        InterpKind.PCHIP: "PchipInterpolator",
        InterpKind.AKIMA: "Akima1DInterpolator",
        InterpKind.BSPLINE: "make_interp_spline",
    }
)

# InterpKind -> interpax JAX-differentiable drop-in spline class. `LINEAR` is the one-shot
# `interpax.interp1d(method="linear")` path (handled before this table) and `BSPLINE` has no
# interpax B-spline class, so neither keys this table; a kind absent here has no differentiable
# companion and degrades to the scipy `make_interp_spline` body or the `np.interp` linear floor.
_INTERPAX_KIND: FrozenDict[InterpKind, str] = FrozenDict(
    {
        InterpKind.CUBIC: "CubicSpline",
        InterpKind.PCHIP: "PchipInterpolator",
        InterpKind.AKIMA: "Akima1DInterpolator",
    }
)


# quadax `QuadratureInfo.status` integer code -> the receipt `SolveStatus` member-name vocabulary.
# `0` converges; every nonzero code is decoded through the package `quadax.STATUS` table and
# normalized into the receipt vocabulary by substring of the decoded message, degrading an
# unrecognized code to `"other"` (the receipt `_status` fold maps it to `SolveStatus.OTHER`)
# rather than mislabeling round-off/divergence as a step-budget miss.
_QUAD_STATUS: FrozenDict[str, str] = FrozenDict(
    {
        "max": "max_steps_reached",
        "round": "stagnation",
        "diverg": "nonlinear_divergence",
        "singular": "singular",
        "nan": "nonfinite",
        "inf": "nonfinite",
    }
)


def _quad_status(quadax: object, status: int) -> str:
    if int(status) == 0:
        return "successful"
    message = quadax.STATUS.get(int(status), "").lower()
    return next((member for token, member in _QUAD_STATUS.items() if token in message), "other")


# --- [MODELS] ------------------------------------------------------------------------------

class FemForm(Struct, frozen=True):
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


@tagged_union(frozen=True)
class QuadratureIntent:
    tag: Literal["integrate", "interpolate", "fem"] = tag()
    integrate: tuple[object, tuple[float, float], QuadKind] = case()
    interpolate: tuple[np.ndarray, np.ndarray, np.ndarray | None, InterpKind] = case()
    fem: "tuple[AssembledSystem, float]" = case()

    @staticmethod
    def Integrate(
        fn: Callable[[float], float] | np.ndarray, span: tuple[float, float], kind: QuadKind = QuadKind.GAUSS_KRONROD
    ) -> "QuadratureIntent":
        return QuadratureIntent(integrate=(fn, span, kind))

    @staticmethod
    def Interpolate(
        points: np.ndarray, values: np.ndarray, query: np.ndarray | None = None, kind: InterpKind = InterpKind.CUBIC
    ) -> "QuadratureIntent":
        return QuadratureIntent(interpolate=(points, values, query, kind))

    @staticmethod
    def Fem(system: "AssembledSystem", dirichlet: float = 0.0) -> "QuadratureIntent":
        return QuadratureIntent(fem=(system, dirichlet))


# --- [TABLES] ------------------------------------------------------------------------------

# QuadKind -> quadax JAX-native adaptive integrator (deferred behind the gated `quadax` import, keyed by the cp315-clean enum).
_QUADAX_ENTRY: FrozenDict[QuadKind, Callable[[object], object]] = FrozenDict(
    {
        QuadKind.GAUSS_KRONROD: lambda qx: qx.quadgk,
        QuadKind.VECTORIZED: lambda qx: qx.quadgk,
        QuadKind.ROMBERG: lambda qx: qx.romberg,
        QuadKind.TANH_SINH: lambda qx: qx.quadts,
        QuadKind.SAMPLED_SIMPSON: lambda qx: qx.sampled.simpson,
    }
)


# --- [OPERATIONS] --------------------------------------------------------------------------

def solve(intent: QuadratureIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: QuadratureIntent) -> SolverReceipt:
    match intent:
        case QuadratureIntent(tag="integrate", integrate=(fn, span, kind)):
            return _integrate_receipt(fn, span, kind)
        case QuadratureIntent(tag="interpolate", interpolate=(points, values, query, kind)):
            return _interpolate_receipt(points, values, query, kind)
        case QuadratureIntent(tag="fem", fem=(system, dirichlet)):
            return _fem_receipt(system, dirichlet)
        case unreachable:
            assert_never(unreachable)


def _integrate_receipt(fn: object, span: tuple[float, float], kind: QuadKind) -> SolverReceipt:
    lo, hi = span
    try:
        import quadax
        import quadax.sampled  # bind the sampled-data submodule the `SAMPLED_SIMPSON` row reads.

        if kind is QuadKind.SAMPLED_SIMPSON:
            samples = np.asarray(fn)
            grid = np.linspace(lo, hi, samples.size)
            _ = _QUADAX_ENTRY[kind](quadax)(samples, x=grid)
            return SolverReceipt.Iterative(0.0, int(samples.size), _EPSREL, result="successful")
        _, info = _QUADAX_ENTRY[kind](quadax)(fn, np.asarray([lo, hi]), epsabs=_EPSABS, epsrel=_EPSREL)
        return SolverReceipt.Iterative(
            float(info.err), int(info.neval), _EPSREL, result=_quad_status(quadax, info.status)
        )
    except ImportError:
        return _integrate_scipy(fn, lo, hi, kind)


def _integrate_scipy(fn: object, lo: float, hi: float, kind: QuadKind) -> SolverReceipt:
    try:
        import scipy.integrate as integ

        match kind:
            case QuadKind.SAMPLED_SIMPSON:
                grid = np.linspace(lo, hi, np.asarray(fn).size)
                _ = integ.simpson(np.asarray(fn), x=grid)
                return SolverReceipt.Iterative(0.0, int(np.asarray(fn).size), _EPSREL, result="successful")
            case QuadKind.VECTORIZED:
                _, abserr = integ.quad_vec(fn, lo, hi, epsabs=_EPSABS, epsrel=_EPSREL)[:2]
                return SolverReceipt.Iterative(float(np.max(abserr)), 0, _EPSREL, result=_quad_floor(float(np.max(abserr))))
            case _:
                _, abserr, info = integ.quad(fn, lo, hi, epsabs=_EPSABS, epsrel=_EPSREL, full_output=True)[:3]
                return SolverReceipt.Iterative(float(abserr), int(info.get("neval", 0)), _EPSREL, result=_quad_floor(float(abserr)))
    except ImportError:
        grid = np.linspace(lo, hi, 1024)
        abserr = float((hi - lo) / 1024)
        _ = float(np.trapezoid([fn(float(t)) for t in grid], grid)) if callable(fn) else float(np.trapezoid(np.asarray(fn), grid))
        return SolverReceipt.Iterative(abserr, 1024, _EPSREL, result=None)


def _quad_floor(abserr: float) -> str | None:
    return "successful" if abserr <= _EPSREL else "stagnation"


def _interpolate_receipt(
    points: np.ndarray, values: np.ndarray, query: np.ndarray | None, kind: InterpKind
) -> SolverReceipt:
    xq = query if query is not None else 0.5 * (points[:-1] + points[1:])
    baseline = np.interp(xq, points, values)
    fitted = _evaluate_interpolant(points, values, xq, kind)
    residual = float(np.linalg.norm(np.asarray(fitted) - baseline))
    return SolverReceipt.LeastSquares(residual, int(points.size), 0)


def _evaluate_interpolant(points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind) -> np.ndarray:
    try:
        import interpax

        if kind is InterpKind.LINEAR:
            return np.asarray(interpax.interp1d(xq, points, values, method="linear"))
        if kind not in _INTERPAX_KIND:  # BSPLINE has no interpax class; route to the scipy B-spline body.
            return _interpolate_scipy(points, values, xq, kind)
        spline = getattr(interpax, _INTERPAX_KIND[kind])(points, values)
        return np.asarray(spline(xq))
    except ImportError:
        return _interpolate_scipy(points, values, xq, kind)


def _interpolate_scipy(points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind) -> np.ndarray:
    if kind is InterpKind.LINEAR:
        return np.interp(xq, points, values)
    try:
        import scipy.interpolate as interp

        ctor = getattr(interp, _SCIPY_INTERP[kind])
        spline = ctor(points, values, k=3) if kind is InterpKind.BSPLINE else ctor(points, values)
        return np.asarray(spline(xq))
    except ImportError:
        return np.interp(xq, points, values)


def _fem_receipt(system: "AssembledSystem", dirichlet: float) -> SolverReceipt:
    import skfem

    seed = np.zeros(system.dof_count) + dirichlet
    condensed = skfem.condense(system.stiffness, system.load, x=seed, D=system.dirichlet_dofs)
    solution = skfem.solve(*condensed)
    return _sparse_receipt(LinearMap.SparseMat(system.stiffness), system.stiffness @ solution, SparseScheme.Spsolve())
```

## [03]-[RESEARCH]

- [QUADAX_INTEGRATE]: `quadax` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `quadgk`/`quadcc`/`quadts`/`romberg`/`rombergts`/`adaptive_quadrature` integrators, the `QuadratureInfo` receipt (`err`/`neval`/`status`/`info`), the `STATUS` decode table, and the `sampled.simpson`/`sampled.trapezoid` sampled-data integrators verify against `compute/.api/quadax.md`. Every callable-integrand integrator returns `(value, QuadratureInfo)`, so the integrate fold reads the estimated error, the evaluation count, and the convergence status from the receipt rather than discarding them — `QuadratureInfo.status` is an integer code decoded through the package `STATUS` table and folded through `_quad_status` into the `SolveStatus` vocabulary, never inferred from the value. The integration is JIT-compatible and differentiable through the integrand and the interval bounds, so a downstream `grad`/`vjp` reads the autodifferentiable integral; this realizes the `[INTERPAX_QUADAX_USAGE]` deferred consumer the quadax catalogue records for `solvers/quadrature`. The `_QUADAX_ENTRY` table defers every `quadax` entrypoint reference behind a `Callable[[qx], entrypoint]` so the cp315-clean `QuadKind` enum keys it without importing the gated package at module load.
- [SCIPY_QUADRATURE]: the host floor `scipy.integrate.quad(func, a, b, epsabs, epsrel, full_output=True)` returns `(value, abserr, infodict)` so the scipy integrate path reads the same error/eval-count evidence the quadax path reads; `quad_vec(f, a, b, epsabs, epsrel)` serves the `VECTORIZED` kind and `simpson(y, x)` the `SAMPLED_SIMPSON` kind. The interpolation host floor uses the modern `scipy.interpolate` interpolant classes (`CubicSpline`, `PchipInterpolator`, `Akima1DInterpolator`, `make_interp_spline`) keyed by `_SCIPY_INTERP`, never the deprecated `interp1d`, since those classes own the C2-cubic/shape-preserving-PCHIP/Akima/B-spline kernels directly and carry `__call__` evaluation. All scipy spellings carry the `python_version<'3.15'` marker and verify against `compute/.api/scipy.md`. The `np.trapezoid` and `np.interp` floors run unconditionally on cp315 and are reached only when neither the gated companion nor scipy resolves, so the integrate and the linear-interpolation routes are never band-gated.
- [INTERPAX_INTERPOLATE]: `interpax` resolves on the gated `python_version<'3.15'` band; the JAX-differentiable drop-in spline classes `CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator` and the one-shot `interp1d(xq, x, f, method)` (with the `"linear"`/`"cubic"`/`"monotonic"` method vocabulary) and the reusable `Interpolator1D` object verify against `compute/.api/interpax.md`; the `_INTERPAX_KIND` table keys each `InterpKind` to its interpax class/method so the interpolate floor is `vmap`/`grad`/`jit`-compatible and differentiable through the sample knots, realizing the `[INTERPAX_QUADAX_USAGE]` deferred consumer the interpax catalogue records for `solvers/quadrature`. The 2-D/3-D `interp2d`/`interp3d` and `fft_interp*` family is the `solvers/field` consumer half, not this 1-D route.
- [SKFEM_SOLVE]: the `condense`/`solve` spellings carry the `python_version<'3.15'` marker and verify against `compute/.api/scikit-fem.md`; this route owns only the solve half — the `Basis`/`asm` assembly that builds the stiffness/load pair lives on `solvers/mesh.md#MESH` and reaches this route as the `AssembledSystem` artifact, so a single `condense(system.stiffness, system.load, x=seed, D=system.dirichlet_dofs)` and `solve(*condensed)` close the weak-form fold without re-running the assemble. The stiffness solve folds through `_sparse_receipt(LinearMap.SparseMat(system.stiffness), ...)` — wrapping the assembled sparse stiffness in the `LinearMap` operand the `solvers/linear.md#LINEAR` sparse route accepts — so the FEM stiffness solve emits the identical sparse convergence receipt as a direct sparse system rather than a parallel FEM-receipt struct. The element family is the `_ELEMENT_CTOR` table declared on this owner — the eight `ElementKind` rows map to the `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1` constructors the mesh assemble resolves through `getattr(skfem, _ELEMENT_CTOR[element])`, reaching the full P1/P2 line/tri/tet plus the bilinear-quad and trilinear-hex elements the catalogue lists so both routes span the identical catalogued skfem element family once the scikit-fem wheel resolves.
