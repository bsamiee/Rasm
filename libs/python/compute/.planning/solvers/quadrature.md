# [PY_COMPUTE_QUADRATURE]

The quadrature, interpolation, and finite-element routes of the one numeric solver. `QuadratureIntent` discriminates 1-D quadrature, 1-D interpolation, and the weak-form finite-element `condense -> solve` fold, every route folding into the one `SolverReceipt`.

Variation rides bounded policy values and one data catalog per concept, never parallel entrypoints or parallel tables. `QuadKind` keys one `_QUAD` row catalog selecting the integration family; `InterpKind` keys one `_INTERP` row catalog selecting the interpolant family; the shared `Readout` axis (`VALUE`/`DERIVATIVE`/`ANTIDERIVATIVE`/`CUMULATIVE`) parameterizes the output shape across both routes. A scalar definite integral, a running antiderivative array, an interpolant's `nu`-th derivative, and its analytic antiderivative are `Readout` rows, never an `IntegrateCumulative`/`InterpolateDerivative` factory family.

One `QuadPolicy` struct carries every per-call knob across both numeric routes — the `differential.md#DIFFERENTIAL` `IntegratePolicy` single-policy discipline, one struct whose unused fields stay unread per route rather than two parallel `QuadPolicy`/`InterpPolicy` siblings sharing a `readout`. `epsabs`/`epsrel`/`order`/`max_ninter`/`divmax`, the `adaptive`-versus-`fixed_quad*` constant-cost bit, the derivative order `nu`, `extrapolate`, `bspline_k`, and the shared `readout` are struct fields, never buried module literals or boolean knobs.

One per-kind catalog row — a `QuadRow`/`InterpRow` value object — replaces the prior parallel `_QUADAX_ENTRY`/`_INTERPAX_METHOD`/`_INTERPAX_KIND`/`_SCIPY_INTERP` tables. The `QuadRow` carries the quadax adaptive-integrator name, the `fixed`/`extrapolated`/`sampled` flags, and the scipy-adapter callable; the `InterpRow` carries the interpax method/class plus the scipy class. A new family is one catalog row, never a triple-table edit plus a fresh `match` arm.

Each numeric route is a three-floor ladder keyed by what resolves: the JAX-native differentiable companion when the gated band is present (`quadax` adaptive/fixed/sampled rules through the polymorphic `adaptive_quadrature(rule, fun, interval, ...)` driver, `interpax` differentiable interpolants and spline calculus), the host scipy body when scipy resolves, and the unconditional numpy floor (`np.trapezoid`, `np.interp`) on the bare cp315 core.

The integrate route folds the `quadax.utils.QuadratureInfo`/`scipy quad full_output` termination verdict — the convergence bitfield, evaluation count, and estimated error — into the `Iterative` receipt's typed `SolveStatus`, exactly as the sparse Krylov route does, so a non-converged adaptive integral is a first-class verdict rather than a silent value. The scipy and numpy floors carry no backend adjudicator: they pass `result=None` to the shared `solvers/receipt.md#RECEIPT` `_status` residual floor, the one verdict path every numpy floor in the corpus uses.

The FEM route consumes the `AssembledSystem` the `solvers/mesh.md#MESH` `MeshExchange.assemble` lowers, owning only the `condense -> solve` half and never re-running `Basis`/`asm`. `skfem.condense` eliminates the Dirichlet dofs, then the caller's `SparseScheme` and `LinearPolicy` solve the condensed system through the `solvers/linear.md#LINEAR` `_sparse_receipt` against `MatrixStructure.SYMMETRIC` stiffness, so a large SPD stiffness picks `Krylov(CG)` and a small one `Spsolve` while emitting the same convergence evidence as a direct sparse system. The receipt residual is the honest `‖cond_a @ x - cond_b‖` against the condensed load, never a circular `A x = A @ x` re-solve. `ElementKind` and `FemForm` are the element/basis/form axis this owner declares; `solvers/mesh.md#MESH` imports both and keys them onto its own `_CTOR` `(Mesh*, Element*, cell-type)` triple — the constructor-spelling table is the mesh owner's, since only the assemble fold constructs an `Element*` and the FEM solve here never touches a basis.

## [01]-[INDEX]

- [01]-[QUADRATURE]: 1-D quadrature, 1-D interpolation, and the weak-form FEM condense fold on one `QuadratureIntent` owner reading two family enums (`QuadKind`/`InterpKind`) keyed onto one `_QUAD`/`_INTERP` catalog row each, one shared `Readout` output axis, and one `QuadPolicy` struct over a three-floor quadax/scipy/numpy ladder, the adaptive-quadrature termination bitfield folded into typed `SolveStatus` and the condensed FEM solve into a caller-chosen `SparseScheme`/`LinearPolicy`.

## [02]-[QUADRATURE]

- Owner: `QuadratureIntent` carries the integral, interpolation, and FEM cases on the one solver. `Integrate(fn, span, kind, policy)` runs the `QuadKind`-selected `adaptive_quadrature` driver / scipy `integrate` body / `np.trapezoid` floor; `Interpolate(points, values, query, kind, policy, dydx)` the `InterpKind`-selected interpax interpolant / scipy `interpolate` class / `np.interp` floor; `Fem(system, dirichlet, scheme, policy)` the `condense -> solve` half over the `AssembledSystem` the `MeshExchange.assemble` fold lowered. The FEM case carries the assembled stiffness/load/dof system itself, never the `MeshField`, so this route condenses and solves and never reaches into the mesh assembly. `QuadratureIntent.solve` is the one method on the union, matching `LinearIntent.solve`/`DifferentialIntent.solve`/`FieldQuery.evaluate`: it enters `boundary(f"solve.{self.tag}", ...)` and the inner `match self` dispatches the three routes total through `assert_never`, never a free `solve(intent)` beside a free `_dispatch`. `FemForm` is the element/basis/form axis the mesh `assemble` reads; `ElementKind` is the shared element enum, the bilinear and linear forms carry the integrand thunks, and the boundary facets carry the Dirichlet condition. The skfem `Element*` constructor spelling lives on the mesh owner's `_CTOR` triple, not here, because only the assemble fold instantiates a basis.
- Quadrature catalog: `QuadKind` is the integration-family enum and `_QUAD` the row catalog it keys, one row per family. The members are `GAUSS_KRONROD` (default globally-adaptive smooth), `CLENSHAW_CURTIS` (Chebyshev-node oscillatory-friendly), `ROMBERG` (Richardson-extrapolated smooth), `ROMBERG_TS` (Romberg over tanh-sinh nodes for singular extrapolated integrals), `TANH_SINH` (endpoint-singular/infinite-range double-exponential), `VECTORIZED` (vector-valued integrand panels), and `SAMPLED_SIMPSON` (already-discretized data). Each `QuadRow` carries the `adaptive` integrator name (the documented `quadgk`/`quadcc`/`quadts` specialization that builds its `AbstractQuadratureRule` from `order` and delegates to the polymorphic `adaptive_quadrature(rule, fun, interval, ...)` driver, or `romberg`/`rombergts` for the extrapolated rows), the `fixed` constant-cost analogue, the `extrapolated` flag (reads `divmax` not `order`/`max_ninter`), the `sampled` flag, and the `scipy` adapter folding the divergent scipy callable into the one `(err, neval)` evidence pair. The prior `_QUADAX_ENTRY` lambda-triple, the per-family kwarg-shape split, and the five-arm `_integrate_scipy` match collapse into this one catalog whose row is the family.
- Adaptive call site: the adaptive entry is one row-keyed call `getattr(quadax, row.adaptive)(fn, interval, ...)`. The named integrators each delegate to the `adaptive_quadrature` rule driver internally, so the row's name discriminates Gauss-Kronrod/Clenshaw-Curtis/tanh-sinh/Romberg without three hand-spelled wrapper sites — the `extrapolated` rows pass `divmax`, the rest pass `order`+`max_ninter`, and the constant-cost path swaps to `getattr(quadax, row.fixed)(fn, lo, hi, n=fixed_nodes)` over scalar bounds. A new rule is one `QuadKind` member plus one `_QUAD` row, never a per-rule `_integrate_*` body and never three parallel `quadgk`/`quadcc`/`quadts` call sites. `QuadPolicy` carries the scalar integrator keyword arguments — `epsabs`/`epsrel`, `order` (adaptive node count), `max_ninter` (subdivision cap), `divmax` (Romberg table depth), `fixed_nodes` (constant-cost node count), `floor_nodes` (the numpy trapezoid floor grid count), and `adaptive` (the `adaptive_quadrature`-versus-`fixed_quad*` bit) — so a tolerance, an order bump, a floor resolution, or a `jax.vmap`-friendly constant-cost integral is a struct field, never a buried `1e-10` or `1024` literal or a parallel factory.
- Interpolation catalog: `InterpKind` is the interpolant-family enum and `_INTERP` the row catalog it keys — `LINEAR`, `CUBIC` (C2 not-a-knot), `CUBIC2` and `CATMULL_ROM` (interpax-native cubic variants), `PCHIP` (shape-preserving monotonic), `AKIMA`, `HERMITE` (values plus node derivatives), and `BSPLINE`. Each `InterpRow` carries the `interpax_method` (the one-shot `interp1d(method=...)` kernel for the method-only kinds — `"linear"`/`"cubic2"`/`"catmull-rom"`, `None` otherwise), the `interpax_class` (the JAX-differentiable drop-in spline class `CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`CubicHermiteSpline`, `None` for the method-only and B-spline kinds), and the `scipy_class` (the modern `scipy.interpolate` class/factory, never the deprecated `interp1d`; the admitted catalog carries `CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`make_interp_spline` but no node-derivative Hermite drop-in, so `HERMITE` carries the interpax `CubicHermiteSpline` and the degree-`k` `make_interp_spline` scipy floor). The prior `_INTERPAX_METHOD`/`_INTERPAX_KIND`/`_SCIPY_INTERP` triple collapses into this one catalog whose row is the family. A kind whose `interpax_class` and `interpax_method` are both `None` (`BSPLINE`) has no differentiable interpax companion and routes to the scipy `make_interp_spline` body or the `np.interp` linear floor. A new interpolant family is one `InterpKind` member plus one `_INTERP` row.
- Interpolation policy: `QuadPolicy` carries the interp output knobs alongside the integrate knobs — `nu` (the derivative order both interpax `__call__(x, nu=...)` and `<spline>.derivative(nu)` accept), `extrapolate`, `bspline_k` (the B-spline degree), and the shared `readout`. `HERMITE` carries its `dydx` derivative array on the `Interpolate` request itself, since `CubicHermiteSpline(x, y, dydx)` takes a third array no `(points, values)` row supplies; `_construct` defaults it through `np.gradient(values, points)` when the caller passes none.
- Output axis: `Readout` is the bounded output-shape policy shared by both numeric routes, the output parameterization the corpus mandates. The integrate route reads `VALUE`/`ANTIDERIVATIVE` as the scalar integral and `CUMULATIVE` as the running-antiderivative array (`quadax.sampled.cumulative_simpson`, `scipy.integrate.cumulative_simpson`, or the spline `.antiderivative()`); the interpolate route reads `VALUE`/`DERIVATIVE`/`ANTIDERIVATIVE`/`CUMULATIVE` off the interpolant's `nu`-evaluation and calculus methods. The `CUMULATIVE`/`ANTIDERIVATIVE` readout on a spline kind evaluates the analytic `.antiderivative()` across the query array; a method-only kind (`LINEAR`/`CUBIC2`/`CATMULL_ROM`) owns no analytic antiderivative, so it folds the running cumulative-trapezoid of its evaluated samples through `_cumulative_readout` — the running integral, distinct from `VALUE`, never silently conflated into the evaluate arm where a one-shot `interp1d` carries no calculus surface. One enum spans both routes rather than parallel `IntegrateOutput`/`InterpOutput` enums, so a derivative-of-interpolant, an exact spline definite integral, and a cumulative quadrature share one vocabulary the policy struct carries.
- Element axis: `ElementKind` is the element/basis enum and `FemForm` the weak-form value object, both declared here as the canonical element-family axis the FEM cases on both owners read. `solvers/mesh.md#MESH` imports `ElementKind` and `FemForm` and keys `ElementKind` onto its own `_CTOR` `(Mesh*-constructor, Element*-constructor, meshio-cell-type)` triple, so the constructor-spelling table is the mesh owner's — only the `assemble` fold instantiates an `Element*`, and this FEM solve condenses an already-assembled system without ever resolving a basis. A new element is one `ElementKind` member read by both routes plus one `_CTOR` row on the mesh owner. A `_ELEMENT_CTOR` constructor table on this route is the deleted form: it would duplicate the mesh owner's `_CTOR` and stay unread here, since the solve half never constructs a basis.
- Spline-integrand exact path: when the integrand IS a fitted interpax/scipy spline (the `quadax`↔`interpax` stack the catalog documents — "`<spline>.integrate(a, b)` gives the exact piecewise-polynomial definite integral when the integrand IS the spline, avoiding a quadrature call"), the integrate route reads `spline.integrate(lo, hi)` directly rather than re-quadraturing a closed-form polynomial, and a `CUMULATIVE` readout reads the analytic `spline.antiderivative()` running integral across the grid. Exact integration carries no error estimate, so the verdict is the finiteness floor on the computed value (`0.0` finite → `SUCCESS`, non-finite → `NONFINITE`) with `result=None`, never an unconditional converged verdict that would smuggle a divergent spline integrand past the floor. A callable integrand that is not a spline takes the `_QUAD`-selected adaptive/fixed driver; the spline-integrand fast path is recognized structurally (the integrand carries the `.integrate` calculus method), never a parallel `IntegrateSpline` case.
- Non-convergence law: the integrate route never returns a non-converged integral as a silent success. The `quadax.utils.QuadratureInfo.status` integer is a bitfield combining `NORMAL_EXIT`/`MAX_NINTER`/`ROUNDOFF`/`BAD_INTEGRAND`/`NO_CONVERGE`, so a combined code's decoded message may carry several tokens. `_quad_status` decodes it through `quadax.STATUS` and walks the severity-ordered `_QUAD_STATUS` token tuple, so the most-severe verdict wins deterministically — a divergence or non-finite token beats a co-set round-off token rather than resolving by dict-iteration order. It folds `0` to `successful`, a divergence/singular/non-finite message to `nonlinear_divergence`/`singular`/`nonfinite`, a step-budget message to `max_steps_reached`, a round-off message to `stagnation`, and an unrecognized code to `other`. The `NO_CONVERGE` flag decodes to `nonlinear_divergence` (the adaptive driver failed to reach tolerance), not `max_steps_reached` (the distinct step-budget exhaustion `MAX_NINTER` carries), so the two non-success causes stay separate verdicts.
- Verdict slots: the estimated error rides the `residual` slot and the evaluation count the `iterations` slot, so `SolverReceipt.Iterative(err, neval, epsrel, result=_quad_status(...))` carries why the adaptive integral stopped, exactly as the sparse Krylov `_info_status` fold does. The scipy `quad`/`tanhsinh` and the numpy `trapezoid` floors hold no decoded bitfield: they pass `result=None`, and the shared `solvers/receipt.md#RECEIPT` `_status(None, residual, tol)` floor adjudicates `SUCCESS`/`STAGNATION`/`NONFINITE` from the estimated error against `epsrel`. That is the residual-floor verdict every numpy floor in `linear.md`/`field.md`/`differential.md` uses, collapsing the former bespoke `_quad_floor` threshold into the one corpus floor.
- Entry: `QuadratureIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`. The integrate route runs the `QuadKind`/`policy`-selected floor and folds the estimated error, evaluation count, and decoded status into `Iterative`, or the cumulative-array finiteness verdict for a `CUMULATIVE` readout, never the integral magnitude smuggled into the residual slot. The interpolate route builds the `InterpKind`-selected interpolant and reads it under the `Readout` at the query points (the sample midpoints when no query is supplied): a `VALUE` readout reports the honest residual against the `np.interp` linear baseline into `LeastSquares`, while a `DERIVATIVE`/`ANTIDERIVATIVE`/`CUMULATIVE` readout has no shared baseline and reports the finiteness floor (`0.0` finite, `inf` non-finite) rather than the readout magnitude smuggled into the residual. The FEM route condenses the Dirichlet dofs against `system.stiffness`/`system.load`/`system.dirichlet_dofs` through `skfem.condense` and folds the condensed solve through `solvers/linear.md#LINEAR` `_sparse_receipt` over the caller's `SparseScheme`/`LinearPolicy`, against the honest condensed-load residual rather than a circular re-solve.
- Differentiability: the quadax integrate floor and the interpax interpolate floor are JAX-traceable. A downstream `grad`/`vjp` flows through the integrand and interval bounds (quadax) or through the sample knots and the read-out derivative (interpax) without differentiating the adaptive iterations, so `solvers/sensitivity.md#SENSITIVITY` and `optimization/design.md#DESIGN` read an autodifferentiable integral, antiderivative, or interpolant-derivative through this route rather than a finite-difference floor. A `fixed_quad*` integral (`policy.adaptive=False`) is the `jax.vmap`-friendly constant-cost form: a parameter-swept batch of integrals vectorizes through the leading axis without data-dependent subdivision, the batched-integral case `experiments/study.md#STUDY` reads. The scipy and numpy floors are the non-differentiable host fallbacks reached only when the gated band is absent.
- Packages: `quadax` (`adaptive_quadrature` the polymorphic rule-parameterized driver, `GaussKronrodRule`/`ClenshawCurtisRule`/`TanhSinhRule` the `AbstractQuadratureRule` subclasses the driver dispatches on, `romberg`/`rombergts` the extrapolated integrators, `fixed_quadgk`/`fixed_quadcc`/`fixed_quadts` the constant-cost `vmap`-friendly fixed-order forms, `utils.QuadratureInfo` the `err`/`neval`/`status`/`info` receipt imported from `quadax.utils`, `STATUS` the top-level decode table, `sampled.simpson`/`sampled.cumulative_simpson`/`sampled.trapezoid`/`sampled.cumulative_trapezoid` the sampled-data floor — the JAX-native differentiable adaptive/fixed/sampled surface), `interpax` (`CubicSpline`, `PchipInterpolator`, `Akima1DInterpolator`, `CubicHermiteSpline`, `interp1d`, `Interpolator1D` — the JAX-native differentiable interpolant floor with `.derivative`/`.antiderivative`/`.integrate`/`.roots` calculus), `scipy` (`integrate.quad`, `integrate.quad_vec`, `integrate.tanhsinh`, `integrate.simpson`, `integrate.cumulative_simpson`, `interpolate.CubicSpline`, `interpolate.PchipInterpolator`, `interpolate.Akima1DInterpolator`, `interpolate.make_interp_spline` — the host bodies; never the deprecated `interp1d`, and no scipy `CubicHermiteSpline` since the admitted catalog carries no node-derivative Hermite drop-in: the HERMITE scipy floor is the degree-`k` `make_interp_spline` C2 cubic), `skfem` (`condense`, `solve` — the solve half only; the `Basis`/`asm` assembly stays on `solvers/mesh.md#MESH`), `numpy` (`trapezoid`, `cumulative_trapezoid`, `interp`, `gradient`, `linspace`, `linalg.norm`, `isfinite`, `asarray`, `zeros`), `solvers/receipt.md#RECEIPT` (`SolverReceipt` — `SolveStatus` and the `_status` residual floor are folded inside the receipt factories, never imported here; the scipy/numpy quadrature paths pass `result=None` so the factory's floor adjudicates), `solvers/linear.md#LINEAR` (`_sparse_receipt`, `LinearMap`, `LinearPolicy`, `MatrixStructure`, `SparseScheme`), `solvers/mesh.md#MESH` (`AssembledSystem` — the assembled stiffness/load/dof artifact the FEM route condenses; the mesh owner's `_CTOR` triple names the `ElementLineP1`…`ElementHex1` constructors keyed off the shared `ElementKind`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new element is one `ElementKind` member read by both routes plus one `_CTOR` row on the mesh owner; a new quadrature rule is one `QuadKind` member plus one `_QUAD` row (the adaptive integrator name, the `fixed`/`extrapolated`/`sampled` flags, and the scipy adapter), never a per-rule integrate body; a new interpolant family is one `InterpKind` member plus one `_INTERP` row (the interpax method/class and scipy class); a new output shape is one `Readout` member read by both routes' fold; a new integrator scalar is one `QuadPolicy` field threaded into the entry; a new termination code is one `_QUAD_STATUS` severity-ranked token row mapping the decoded `quadax.STATUS` message into the existing `SolveStatus` vocabulary; a new FEM sparse scheme or tuning axis is zero new surface because the caller passes any `SparseScheme`/`LinearPolicy` the linear route owns; zero new surface, never a per-rule integrate factory, never a per-kind interpolate body, never a `cumulative`/`derivative` boolean knob where the `Readout` row carries the modality, never a triple parallel table where one catalog row carries the family.
- Boundary: the `np.trapezoid` and `np.interp` floors run unconditionally on cp315. `quadax`/`interpax`/`jaxlib`, `scipy`, and `skfem` carry no cp315 wheel, so the quadax/interpax differentiable companions, the scipy quadrature/interpolation bodies, and the scikit-fem condense/solve are authored against the documented API with reachable numpy floors. Assembly (`Basis`/`asm`) stays on `solvers/mesh.md#MESH` and this route consumes the `AssembledSystem` it lowers, never re-running the assemble.
- Deleted forms: a hardcoded `scipy.integrate.quad` where a `QuadKind` row spans the family; three parallel `_QUADAX_ENTRY`/`_INTERPAX_KIND`/`_SCIPY_INTERP` tables where one `_QUAD`/`_INTERP` row carries the family; two parallel `QuadPolicy`/`InterpPolicy` structs sharing a `readout`; three hand-spelled `quadgk`/`quadcc`/`quadts` call sites where one row-keyed `getattr(quadax, row.adaptive)(fn, interval, ...)` discriminates the family; a deprecated `interp1d` where the modern interpolant classes own the kernel; a `1e-10` tolerance or `1024` floor-grid literal where `QuadPolicy` carries it; a `cumulative`/`derivative` boolean where `Readout` carries the output shape; a `CUMULATIVE` readout conflated into the spline `VALUE` evaluate arm; an integral or interpolant-readout magnitude smuggled into the residual (`np.ptp`/`np.linalg.norm(fitted)`) or condition slot where the finiteness floor or linear baseline is the honest verdict; a circular `_sparse_receipt(stiffness, stiffness @ x, scheme, policy)` re-solve where the residual is the honest condensed-load residual; a `_sparse_receipt` call dropping the `LinearPolicy` or the `MatrixStructure` the linear route reads; a `NO_CONVERGE` decoded to `max_steps_reached` where it is a divergence; a discarded `QuadratureInfo` termination bitfield; a bespoke `_quad_floor` threshold where the shared `_status` floor adjudicates; a re-quadrature of a spline integrand where `<spline>.integrate(a, b)` is exact; a duplicated element-constructor table on this route; an inline `Basis`/`asm` assembly here; a separate FEM-receipt struct; a 2-D/3-D interpolation route (the `interpax` `interp2d`/`interp3d` family serves it on `solvers/field` per the `[INTERPAX_QUADAX_USAGE]` map); and a multidimensional ODE integrator (it lives in `solvers/differential.md#DIFFERENTIAL`).

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.linear import LinearMap, LinearPolicy, MatrixStructure, SparseScheme, _sparse_receipt
from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from rasm.compute.solvers.mesh import AssembledSystem


# --- [TYPES] -------------------------------------------------------------------------------

class QuadKind(StrEnum):
    GAUSS_KRONROD = "gauss_kronrod"
    CLENSHAW_CURTIS = "clenshaw_curtis"
    ROMBERG = "romberg"
    ROMBERG_TS = "romberg_ts"  # Romberg over tanh-sinh nodes; singular/infinite-range extrapolated integral
    TANH_SINH = "tanh_sinh"
    VECTORIZED = "vectorized"
    SAMPLED_SIMPSON = "sampled_simpson"


class InterpKind(StrEnum):
    LINEAR = "linear"
    CUBIC = "cubic"
    CUBIC2 = "cubic2"
    CATMULL_ROM = "catmull_rom"
    PCHIP = "pchip"
    AKIMA = "akima"
    HERMITE = "hermite"  # values + node derivatives; CubicHermiteSpline takes a third `dydx` array
    BSPLINE = "bspline"


# Output shape shared by both numeric routes: VALUE evaluates / scalar integral, DERIVATIVE reads
# the nu-th derivative, ANTIDERIVATIVE reads the analytic antiderivative spline / scalar integral,
# CUMULATIVE reads the running-antiderivative array. One axis spans integrate and interpolate.
class Readout(StrEnum):
    VALUE = "value"
    DERIVATIVE = "derivative"
    ANTIDERIVATIVE = "antiderivative"
    CUMULATIVE = "cumulative"


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

# Decoded-message token -> receipt `SolveStatus` member name, SEVERITY-ORDERED: the `QuadratureInfo`
# status is a bitfield (NORMAL_EXIT/MAX_NINTER/ROUNDOFF/BAD_INTEGRAND/NO_CONVERGE combine), so a
# combined code's decoded message may carry several tokens; the first match in this descending-
# severity tuple wins, so a divergence/non-finite flag dominates a co-set round-off flag rather
# than resolving by dict-iteration order. `NO_CONVERGE` ("did not converge") is a divergence verdict
# distinct from `MAX_NINTER` ("max subdivisions" -> step budget), so the divergence tokens precede
# the step-budget tokens and an unrecognized code degrades to `"other"`.
_QUAD_STATUS: tuple[tuple[str, str], ...] = (
    ("nan", "nonfinite"),
    ("inf", "nonfinite"),
    ("singular", "singular"),
    ("diverg", "nonlinear_divergence"),
    ("converge", "nonlinear_divergence"),  # NO_CONVERGE: adaptive driver failed tolerance, a divergence
    ("bad", "nonlinear_divergence"),
    ("subdivision", "max_steps_reached"),  # MAX_NINTER: step-budget exhaustion, distinct from divergence
    ("max", "max_steps_reached"),
    ("ninter", "max_steps_reached"),
    ("round", "stagnation"),
)


# `quadax` rides the gated band, so the live module threads in from the integrate body; STATUS is the
# top-level decode table mapping the bitfield to a message walked through the severity tuple.
def _quad_status(quadax: object, status: int) -> str:
    if (code := int(status)) == 0:
        return "successful"
    message = quadax.STATUS.get(code, "").lower()
    return next((member for token, member in _QUAD_STATUS if token in message), "other")


# --- [MODELS] ------------------------------------------------------------------------------

# ONE policy struct across both numeric routes (the differential.md `IntegratePolicy` single-policy
# discipline): the integrate fields (`epsabs`..`adaptive`) are read by the integrate route, the interp
# fields (`nu`/`extrapolate`/`bspline_k`) by the interpolate route, the shared `readout` by both —
# unused fields simply unread per route, never two parallel structs sharing a `readout`. `adaptive=False`
# selects the constant-cost `fixed_quad*` form (the jax.vmap-friendly batched integral);
# `readout=CUMULATIVE` selects the running-antiderivative path.
class QuadPolicy(Struct, frozen=True):
    epsabs: float = 1e-10
    epsrel: float = 1e-8
    order: int = 21  # adaptive Kronrod/Chebyshev/tanh-sinh node order
    max_ninter: int = 50  # adaptive subdivision cap
    divmax: int = 20  # Romberg extrapolation table depth
    fixed_nodes: int = 21  # constant-cost node count when `adaptive=False`
    floor_nodes: int = 1024  # numpy trapezoid floor grid count, never a buried literal in the floor body
    adaptive: bool = True
    nu: int = 1  # derivative order both interpax `__call__(x, nu=...)` and `<spline>.derivative(nu)` read
    extrapolate: bool = True
    bspline_k: int = 3  # B-spline degree for the make_interp_spline kind
    readout: Readout = Readout.VALUE


# One quadrature-family catalog row: the adaptive `quadax` callable name (the documented named
# specialization quadgk/quadcc/quadts that constructs its AbstractQuadratureRule internally from `order`
# and delegates to the `adaptive_quadrature` driver; romberg/rombergts for the extrapolated rows; None
# for the sampled row), the constant-cost fixed-order analogue, the sampled-data flag, and the
# scipy-callable name the host floor reads. Replaces the prior parallel `_QUADAX_ENTRY` lambda triple +
# the five-arm `_integrate_scipy` match: the row IS the family, and one row-keyed call site drives every
# adaptive integral rather than three hand-spelled wrapper sites.
class QuadRow(Struct, frozen=True):
    adaptive: str  # quadgk / quadcc / quadts / romberg / rombergts; the order/divmax kwarg the row carries
    fixed: str | None  # fixed_quadgk / fixed_quadcc / fixed_quadts; None where no constant-cost analogue
    extrapolated: bool  # True for romberg/rombergts: reads `divmax` not `order`/`max_ninter`
    sampled: bool  # True for SAMPLED_SIMPSON: pre-discretized data, bare-array return, no QuadratureInfo
    scipy: str  # scipy.integrate callable name the host floor reads (quad / quad_vec / tanhsinh / simpson)


# One interpolant-family catalog row: the interpax one-shot method (None where a spline class exists),
# the interpax differentiable spline class (None for method-only and B-spline kinds), and the scipy
# class/factory. Replaces the prior parallel `_INTERPAX_METHOD`/`_INTERPAX_KIND`/`_SCIPY_INTERP` triple.
class InterpRow(Struct, frozen=True):
    interpax_method: str | None  # interp1d(method=...) kernel for method-only kinds; None where a class exists
    interpax_class: str | None  # JAX-differentiable interpax spline class; None for method-only / B-spline
    scipy_class: str  # modern scipy.interpolate class/factory; never the deprecated interp1d


class FemForm(Struct, frozen=True):
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


@tagged_union(frozen=True)
class QuadratureIntent:
    tag: Literal["integrate", "interpolate", "fem"] = tag()
    integrate: tuple[object, tuple[float, float], QuadKind, QuadPolicy] = case()
    interpolate: tuple[np.ndarray, np.ndarray, np.ndarray | None, InterpKind, QuadPolicy, np.ndarray | None] = case()
    fem: "tuple[AssembledSystem, float, SparseScheme, LinearPolicy]" = case()

    @staticmethod
    def Integrate(
        fn: Callable[[float], float] | np.ndarray,
        span: tuple[float, float],
        kind: QuadKind = QuadKind.GAUSS_KRONROD,
        policy: QuadPolicy = QuadPolicy(),
    ) -> "QuadratureIntent":
        return QuadratureIntent(integrate=(fn, span, kind, policy))

    @staticmethod
    def Interpolate(
        points: np.ndarray,
        values: np.ndarray,
        query: np.ndarray | None = None,
        kind: InterpKind = InterpKind.CUBIC,
        policy: QuadPolicy = QuadPolicy(),
        dydx: np.ndarray | None = None,  # node derivatives for the HERMITE kind; np.gradient-defaulted when None
    ) -> "QuadratureIntent":
        return QuadratureIntent(interpolate=(points, values, query, kind, policy, dydx))

    @staticmethod
    def Fem(
        system: "AssembledSystem",
        dirichlet: float = 0.0,
        scheme: SparseScheme = SparseScheme.Spsolve(),
        policy: LinearPolicy = LinearPolicy(),
    ) -> "QuadratureIntent":
        return QuadratureIntent(fem=(system, dirichlet, scheme, policy))

    def solve(self) -> "RuntimeRail[SolverReceipt]":
        return boundary(f"solve.{self.tag}", lambda: _dispatch(self))


# --- [TABLES] ------------------------------------------------------------------------------

# QuadKind -> the one family row. The adaptive rows carry the documented named quadax integrator
# (quadgk/quadcc/quadts, each a specialization that builds its rule from `order` and delegates to the
# `adaptive_quadrature` driver) and their constant-cost `fixed_quad*` analogue; the Romberg rows carry
# romberg/rombergts (extrapolated=True reads `divmax`, no fixed form); the sampled row carries
# sampled.simpson/cumulative_simpson. The `scipy` field is the host floor callable. VECTORIZED reuses
# quadgk over a vector integrand (quadgk with norm=inf handles arrays); SAMPLED_SIMPSON's adaptive slot
# is unread (the sampled branch routes through quadax.sampled directly).
_QUAD: FrozenDict[QuadKind, QuadRow] = FrozenDict(
    {
        QuadKind.GAUSS_KRONROD: QuadRow("quadgk", "fixed_quadgk", False, False, "quad"),
        QuadKind.CLENSHAW_CURTIS: QuadRow("quadcc", "fixed_quadcc", False, False, "quad"),
        QuadKind.ROMBERG: QuadRow("romberg", None, True, False, "quad"),
        QuadKind.ROMBERG_TS: QuadRow("rombergts", None, True, False, "tanhsinh"),
        QuadKind.TANH_SINH: QuadRow("quadts", "fixed_quadts", False, False, "tanhsinh"),
        QuadKind.VECTORIZED: QuadRow("quadgk", "fixed_quadgk", False, False, "quad_vec"),
        QuadKind.SAMPLED_SIMPSON: QuadRow("simpson", None, False, True, "simpson"),
    }
)

# InterpKind -> the one family row. The method-only kinds (LINEAR/CUBIC2/CATMULL_ROM) carry an
# interpax one-shot method and no class; the spline kinds (CUBIC/PCHIP/AKIMA/HERMITE) carry the
# JAX-differentiable interpax class and a scipy class — HERMITE's scipy class is the degree-`k`
# make_interp_spline FLOOR, since the admitted scipy catalog carries no node-derivative CubicHermiteSpline
# drop-in; BSPLINE carries neither interpax surface and routes to scipy make_interp_spline or the np.interp
# floor. A row whose interpax_method and interpax_class are both None has no differentiable companion.
_INTERP: FrozenDict[InterpKind, InterpRow] = FrozenDict(
    {
        InterpKind.LINEAR: InterpRow("linear", None, "make_interp_spline"),
        InterpKind.CUBIC2: InterpRow("cubic2", None, "make_interp_spline"),
        InterpKind.CATMULL_ROM: InterpRow("catmull-rom", None, "make_interp_spline"),
        InterpKind.CUBIC: InterpRow(None, "CubicSpline", "CubicSpline"),
        InterpKind.PCHIP: InterpRow(None, "PchipInterpolator", "PchipInterpolator"),
        InterpKind.AKIMA: InterpRow(None, "Akima1DInterpolator", "Akima1DInterpolator"),
        InterpKind.HERMITE: InterpRow(None, "CubicHermiteSpline", "make_interp_spline"),  # interpax owns the node-derivative Hermite; the scipy floor is the C2-cubic `make_interp_spline`
        InterpKind.BSPLINE: InterpRow(None, None, "make_interp_spline"),
    }
)


# --- [OPERATIONS] --------------------------------------------------------------------------

def _dispatch(intent: QuadratureIntent) -> SolverReceipt:
    match intent:
        case QuadratureIntent(tag="integrate", integrate=(fn, span, kind, policy)):
            return _integrate_receipt(fn, span, kind, policy)
        case QuadratureIntent(tag="interpolate", interpolate=(points, values, query, kind, policy, dydx)):
            return _interpolate_receipt(points, values, query, kind, policy, dydx)
        case QuadratureIntent(tag="fem", fem=(system, dirichlet, scheme, policy)):
            return _fem_receipt(system, dirichlet, scheme, policy)
        case unreachable:
            assert_never(unreachable)


def _integrate_receipt(fn: object, span: tuple[float, float], kind: QuadKind, policy: QuadPolicy) -> SolverReceipt:
    lo, hi = span
    # Spline-integrand exact path (the quadax<->interpax stack): a fitted spline integrand owns the exact
    # piecewise-polynomial definite integral, so `.integrate(lo, hi)` replaces a quadrature call entirely
    # — a CUMULATIVE readout reads the analytic `.antiderivative()` running integral instead. Exact
    # integration carries no error estimate, so the verdict is the finiteness floor on the computed value
    # (0.0 -> SUCCESS, non-finite -> NONFINITE), never an unconditional success on a divergent integrand.
    if callable(fn) and hasattr(fn, "integrate"):
        out = np.asarray(fn.antiderivative()(np.linspace(lo, hi, policy.floor_nodes))) if policy.readout is Readout.CUMULATIVE else np.asarray(fn.integrate(lo, hi))
        residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
        return SolverReceipt.Iterative(residual, 0, policy.epsrel, result=None)
    row = _QUAD[kind]
    try:
        import quadax
        import quadax.sampled  # bind the sampled submodule the SAMPLED_SIMPSON / CUMULATIVE rows read

        if row.sampled:
            samples = np.asarray(fn)
            grid = np.linspace(lo, hi, samples.size)
            entry = quadax.sampled.cumulative_simpson if policy.readout is Readout.CUMULATIVE else quadax.sampled.simpson
            out = np.asarray(entry(samples, x=grid))
            # A sampled rule returns a bare array with no error estimate; the honest residual is the
            # finiteness floor (0.0 -> SUCCESS, non-finite -> NONFINITE), NEVER the integral magnitude.
            residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
            return SolverReceipt.Iterative(residual, int(samples.size), policy.epsrel, result=None)
        interval = np.asarray([lo, hi])
        # One row-keyed call site over the documented quadax integrators — the named quadgk/quadcc/quadts
        # specializations delegate to the `adaptive_quadrature` rule driver internally, so the row's
        # `adaptive` name discriminates the family without three hand-spelled wrapper sites. romberg/
        # rombergts read `divmax`; the Kronrod/Chebyshev/tanh-sinh family reads `order`+`max_ninter`.
        if not policy.adaptive and row.fixed is not None:
            _, info = getattr(quadax, row.fixed)(fn, lo, hi, n=policy.fixed_nodes)
        elif row.extrapolated:
            _, info = getattr(quadax, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, divmax=policy.divmax)
        else:
            _, info = getattr(quadax, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, max_ninter=policy.max_ninter, order=policy.order)
        return SolverReceipt.Iterative(float(info.err), int(info.neval), policy.epsrel, result=_quad_status(quadax, info.status))
    except ImportError:
        return _integrate_scipy(fn, lo, hi, row, policy)


# The scipy host floor: the row's `scipy` field names the callable and one match folds the divergent
# call/return shapes (quad's (value, abserr, infodict) triple, quad_vec's (value, abserr) pair,
# tanhsinh's result object, simpson's bare scalar) into the one (err, neval) evidence pair. The scipy
# floors hold no decoded bitfield, so they pass result=None and the receipt residual floor adjudicates.
def _integrate_scipy(fn: object, lo: float, hi: float, row: QuadRow, policy: QuadPolicy) -> SolverReceipt:
    try:
        import scipy.integrate as integ

        match row.scipy:
            case "simpson":
                samples = np.asarray(fn)
                grid = np.linspace(lo, hi, samples.size)
                fold = integ.cumulative_simpson if policy.readout is Readout.CUMULATIVE else integ.simpson
                out = np.asarray(fold(samples, x=grid))
                residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
                return SolverReceipt.Iterative(residual, int(samples.size), policy.epsrel, result=None)
            case "quad_vec":
                _, abserr = integ.quad_vec(fn, lo, hi, epsabs=policy.epsabs, epsrel=policy.epsrel)[:2]
                return SolverReceipt.Iterative(float(np.max(abserr)), 0, policy.epsrel, result=None)
            case "tanhsinh":
                res = integ.tanhsinh(fn, lo, hi)  # endpoint-singular host floor; result carries .error/.nfev
                return SolverReceipt.Iterative(float(res.error), int(res.nfev), policy.epsrel, result=None)
            case _:
                _, abserr, info = integ.quad(fn, lo, hi, epsabs=policy.epsabs, epsrel=policy.epsrel, full_output=True)[:3]
                return SolverReceipt.Iterative(float(abserr), int(info.get("neval", 0)), policy.epsrel, result=None)
    except ImportError:
        n = policy.floor_nodes
        grid = np.linspace(lo, hi, n)
        # Sample on the grid axis (axis 0), so a VECTORIZED `(n, d)` integrand integrates over the grid,
        # never over the value dimension a default axis=-1 would collapse.
        samples = np.asarray([fn(float(t)) for t in grid]) if callable(fn) else np.asarray(fn)
        out = (
            np.cumulative_trapezoid(samples, grid, axis=0)
            if policy.readout is Readout.CUMULATIVE
            else np.trapezoid(samples, grid, axis=0)
        )
        residual = float((hi - lo) / n) if np.all(np.isfinite(out)) else float("inf")
        return SolverReceipt.Iterative(residual, n, policy.epsrel, result=None)


def _interpolate_receipt(
    points: np.ndarray, values: np.ndarray, query: np.ndarray | None, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> SolverReceipt:
    xq = query if query is not None else 0.5 * (points[:-1] + points[1:])
    fitted = np.asarray(_evaluate_interpolant(points, values, xq, kind, policy, dydx))
    # VALUE residual measures the interpolant against the np.interp linear baseline; a non-VALUE readout
    # (derivative/antiderivative/cumulative) has no shared baseline, so its verdict is the finiteness floor
    # (0.0 finite -> SUCCESS, inf -> NONFINITE), NEVER the readout magnitude smuggled into the residual.
    residual = (
        float(np.linalg.norm(fitted - np.interp(xq, points, values)))
        if policy.readout is Readout.VALUE
        else (0.0 if np.all(np.isfinite(fitted)) else float("inf"))
    )
    return SolverReceipt.LeastSquares(residual, int(points.size), 0)


# The interpax floor reads the `_INTERP` row: a method-only kind takes the one-shot interp1d(method=...)
# under VALUE/DERIVATIVE (derivative=nu); a spline kind constructs the JAX-differentiable class and reads
# it under the full Readout; a kind with neither interpax surface (BSPLINE) routes to the scipy body. A
# one-shot interp1d owns no `.antiderivative()`, so an ANTIDERIVATIVE/CUMULATIVE readout on a method-only
# kind folds the running integral of the evaluated samples through `_cumulative_readout` rather than
# silently returning the VALUE — the distinct Readout row is never conflated into the evaluate arm.
def _evaluate_interpolant(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> np.ndarray:
    row = _INTERP[kind]
    try:
        import interpax

        if row.interpax_method is not None:
            nu = policy.nu if policy.readout is Readout.DERIVATIVE else 0
            base = np.asarray(interpax.interp1d(xq, points, values, method=row.interpax_method, derivative=nu))
            return _cumulative_readout(base, xq, policy.readout)
        if row.interpax_class is None:  # BSPLINE: no interpax class; route to the scipy B-spline body
            return _interpolate_scipy(points, values, xq, kind, policy, dydx)
        spline = _construct(getattr(interpax, row.interpax_class), points, values, kind, policy, dydx, node_derivatives=True)
        return _read_spline(spline, xq, policy)
    except ImportError:
        return _interpolate_scipy(points, values, xq, kind, policy, dydx)


# A one-shot/linear interpolant owns no analytic antiderivative, so the running integral of its sampled
# values is the honest ANTIDERIVATIVE/CUMULATIVE readout; VALUE/DERIVATIVE pass the evaluated base through.
def _cumulative_readout(base: np.ndarray, xq: np.ndarray, readout: Readout) -> np.ndarray:
    if readout is Readout.ANTIDERIVATIVE or readout is Readout.CUMULATIVE:
        return np.asarray(np.cumulative_trapezoid(base, xq, axis=0, initial=0.0))
    return base


def _interpolate_scipy(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> np.ndarray:
    row = _INTERP[kind]
    if row.interpax_class is None and row.scipy_class == "make_interp_spline" and kind is not InterpKind.BSPLINE:
        # The interpax method-only kinds (LINEAR/CUBIC2/CATMULL_ROM) have no dedicated scipy class; the
        # np.interp linear floor serves the value, np.gradient the derivative, and the running
        # cumulative-trapezoid the antiderivative/cumulative readout — the Readout row is never dropped.
        base = np.asarray(np.interp(xq, points, values))
        if policy.readout is Readout.DERIVATIVE:
            return np.asarray(np.gradient(base, xq))
        return _cumulative_readout(base, xq, policy.readout)
    try:
        import scipy.interpolate as interp

        ctor = getattr(interp, row.scipy_class)
        spline = _construct(ctor, points, values, kind, policy, dydx, node_derivatives=False)
        return _read_spline(spline, xq, policy)
    except ImportError:
        return np.interp(xq, points, values)


# One constructor fold over the row back-end shape, NOT `kind` alone: the interpax `CubicHermiteSpline`
# takes a third `dydx` node-derivative array (np.gradient-defaulted), but scipy carries no node-derivative
# Hermite drop-in in the admitted catalog, so the scipy HERMITE floor degrades to the degree-`k`
# `make_interp_spline` C2 cubic over `(points, values)` — `node_derivatives` is true ONLY for the interpax
# Hermite class. BSPLINE and the scipy Hermite floor both build the degree-`k` `make_interp_spline`; the
# remaining spline kinds take `(points, values)`. Passing the derivative array to `make_interp_spline`
# would bind it to the `k` degree slot, a silent miscall the back-end discriminant forbids.
def _construct(
    ctor: Callable[..., object], points: np.ndarray, values: np.ndarray, kind: InterpKind, policy: QuadPolicy,
    dydx: np.ndarray | None, *, node_derivatives: bool,
) -> object:
    if kind is InterpKind.HERMITE and node_derivatives:
        return ctor(points, values, dydx if dydx is not None else np.gradient(values, points))
    if kind is InterpKind.BSPLINE or kind is InterpKind.HERMITE:  # scipy degree-`k` `make_interp_spline` floor
        return ctor(points, values, k=policy.bspline_k)
    return ctor(points, values)


# Read an interpax/scipy spline under the output axis: VALUE evaluates, DERIVATIVE reads the nu-th
# derivative (both back-ends accept `.derivative(nu)`), ANTIDERIVATIVE and CUMULATIVE both evaluate the
# analytic antiderivative spline `.antiderivative()` across the query array — CUMULATIVE is the running
# integral, distinct from VALUE, never folded into the evaluate arm.
def _read_spline(spline: object, xq: np.ndarray, policy: QuadPolicy) -> np.ndarray:
    match policy.readout:
        case Readout.VALUE:
            return np.asarray(spline(xq))
        case Readout.DERIVATIVE:
            return np.asarray(spline.derivative(policy.nu)(xq))
        case Readout.ANTIDERIVATIVE | Readout.CUMULATIVE:
            return np.asarray(spline.antiderivative()(xq))
        case unreachable:
            assert_never(unreachable)


# The FEM solve owns only the condense->solve half. `skfem.condense(..., expand=False)` eliminates the
# Dirichlet dofs against the seed and returns the reduced `(cond_a, cond_b, *_restore)` bundle (the
# trailing restore map and kept-dof index set are unused here), then the caller's `SparseScheme`/
# `LinearPolicy` solve that CONDENSED system through `_sparse_receipt` over the linear route, so the
# residual is the honest `‖cond_a @ x - cond_b‖` rather than a circular `A x = A @ x` re-solve. The
# stiffness carries `MatrixStructure.SYMMETRIC` so a `Krylov(CG)` scheme reads the SPD structure axis.
def _fem_receipt(system: "AssembledSystem", dirichlet: float, scheme: SparseScheme, policy: LinearPolicy) -> SolverReceipt:
    import skfem

    seed = np.zeros(system.dof_count) + dirichlet
    cond_a, cond_b, *_restore = skfem.condense(system.stiffness, system.load, x=seed, D=system.dirichlet_dofs, expand=False)
    return _sparse_receipt(LinearMap.SparseMat(cond_a, MatrixStructure.SYMMETRIC), np.asarray(cond_b), scheme, policy)
```

## [03]-[RESEARCH]

- [QUADAX_INTEGRATE]: `quadax` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the polymorphic `adaptive_quadrature(rule, fun, interval, args, full_output, epsabs, epsrel, max_ninter, norm)` driver, the `AbstractQuadratureRule` subclasses `GaussKronrodRule`/`ClenshawCurtisRule`/`TanhSinhRule` it dispatches on, the `romberg`/`rombergts` extrapolated integrators, the fixed-order `fixed_quadgk`/`fixed_quadcc`/`fixed_quadts` (scalar bounds `a, b` and node count `n`), the `utils.QuadratureInfo` receipt (`err`/`neval`/`status`/`info`) imported from `quadax.utils` (NOT a top-level export), the top-level `STATUS` decode table, and the `sampled.simpson`/`sampled.cumulative_simpson`/`sampled.trapezoid`/`sampled.cumulative_trapezoid` integrators verify against `compute/.api/quadax.md`. The `_QUAD` catalog keys each `QuadKind` to one `QuadRow` carrying the adaptive integrator name, the `fixed_quad*` analogue, the `extrapolated`/`sampled` flags, and the scipy floor callable — collapsing the prior parallel `_QUADAX_ENTRY` lambda triple, the per-family kwarg-shape split, and the five-arm `_integrate_scipy` match into one catalog whose row IS the family. The adaptive entry is one row-keyed call site `getattr(quadax, row.adaptive)(fn, interval, ...)` over the documented `quadgk`/`quadcc`/`quadts`/`romberg`/`rombergts` integrators — the named integrators are the catalog's convenience specializations that build their `AbstractQuadratureRule` from `order` and delegate to the polymorphic `adaptive_quadrature(rule, fun, interval, ...)` driver, so the row's name discriminates the family through one call site rather than three hand-spelled wrappers. `policy.adaptive=False` swaps to the constant-cost `fixed_quad*` form (the catalogue's `jax.vmap`-friendly batched integral, since adaptive subdivision introduces data-dependent control flow) and the `extrapolated` rows pass `divmax` while the rest pass `order`+`max_ninter`. Every callable-integrand integrator returns `(value, QuadratureInfo)`, so the fold reads the estimated error, the evaluation count, and the convergence status from the receipt rather than the value; `QuadratureInfo.status` is a bitfield (`NORMAL_EXIT`/`MAX_NINTER`/`ROUNDOFF`/`BAD_INTEGRAND`/`NO_CONVERGE` combine across codes 0-31), decoded through `STATUS` into a message and walked through the severity-ordered `_QUAD_STATUS` token tuple so `NO_CONVERGE` resolves to `nonlinear_divergence` (a tolerance-failure divergence) distinct from `MAX_NINTER` -> `max_steps_reached` (a step-budget exhaustion). The `quadax`<->`interpax` stack the catalog documents drives the spline-integrand fast path: when the integrand carries the `.integrate` calculus method, `spline.integrate(lo, hi)` reads the exact piecewise-polynomial definite integral rather than re-quadraturing a closed form. The integration is JIT-compatible and differentiable through the integrand and interval bounds, realizing the `[INTERPAX_QUADAX_USAGE]` deferred consumer the catalogue records for `solvers/quadrature`.
- [SCIPY_QUADRATURE]: the host floor `scipy.integrate.quad(func, a, b, epsabs, epsrel, full_output=True)` returns `(value, abserr, infodict)` so the scipy integrate path reads the same error/eval-count evidence; `quad_vec(f, a, b, epsabs, epsrel)` serves the `VECTORIZED` row, the modern `integrate.tanhsinh(f, a, b)` (result object carrying `.integral`/`.error`/`.nfev`/`.success`) serves the `TANH_SINH` and `ROMBERG_TS` rows, `simpson(y, x)` and `cumulative_simpson(y, x)` serve the `SAMPLED_SIMPSON` value/cumulative readouts, and `np.cumulative_trapezoid` floors the cumulative readout on the bare core. The `QuadRow.scipy` field names the callable and one `match row.scipy` arm folds the divergent scipy call/return shapes into the one `(err, neval)` evidence pair. The sampled and cumulative floors report a finiteness verdict (`0.0` -> `SUCCESS`, non-finite -> `NONFINITE`) rather than the integral magnitude via `np.ptp`, which would smuggle the value into the residual slot. The scipy floors hold no decoded bitfield, so they pass `result=None` and the receipt's internal `_status` residual floor adjudicates the verdict against `epsrel`, the same numpy-floor verdict every sibling route uses — collapsing the former bespoke `_quad_floor` threshold. The interpolation host floor uses the modern `scipy.interpolate` classes (`CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`make_interp_spline`) keyed by the `InterpRow.scipy_class`, never the deprecated `interp1d`, since those classes own the C2-cubic/PCHIP/Akima/B-spline kernels and the `.derivative(nu)`/`.antiderivative()` calculus the `Readout` axis reads; the admitted catalog carries no node-derivative `CubicHermiteSpline`, so the interpax class owns the true Hermite and the scipy HERMITE floor degrades to the degree-`k` `make_interp_spline`. All scipy spellings carry the `python_version<'3.15'` marker and verify against `compute/.api/scipy.md`; the `np.trapezoid`/`np.cumulative_trapezoid`/`np.interp`/`np.gradient` floors run unconditionally on cp315.
- [INTERPAX_INTERPOLATE]: `interpax` resolves on the gated `python_version<'3.15'` band; the JAX-differentiable drop-in spline classes `CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`CubicHermiteSpline` (each a pytree module whose `__call__(x, nu)` evaluates the `nu`-th derivative and whose `.derivative(nu)`/`.antiderivative()`/`.integrate(a, b)`/`.roots()` return new differentiable spline/array objects), the one-shot `interp1d(xq, x, f, method, derivative)` (the `"linear"`/`"cubic"`/`"cubic2"`/`"catmull-rom"`/`"monotonic"` method vocabulary, `derivative` evaluating the requested order), and the reusable `Interpolator1D` object verify against `compute/.api/interpax.md`. The `_INTERP` catalog keys each `InterpKind` to one `InterpRow` carrying the interpax one-shot method (method-only kinds), the interpax differentiable class (spline kinds), and the scipy class — collapsing the prior parallel `_INTERPAX_METHOD`/`_INTERPAX_KIND`/`_SCIPY_INTERP` triple. `_construct` is the one constructor fold both back-ends share, dispatching on the back-end shape rather than `kind` alone: the interpax `CubicHermiteSpline(x, y, dydx)` takes a third node-derivative array (`np.gradient`-defaulted) under `node_derivatives=True`, but scipy carries no node-derivative Hermite drop-in in the admitted catalog, so the scipy HERMITE floor (`node_derivatives=False`) and `BSPLINE` both build the degree-`k` `make_interp_spline`, and the rest take `(points, values)` — passing the derivative array to `make_interp_spline` would bind it to the `k` slot, the silent miscall the discriminant forbids. `_read_spline` folds the `Readout` axis over the spline-class calculus (`VALUE` evaluates, `DERIVATIVE` reads `.derivative(nu)`, `ANTIDERIVATIVE`/`CUMULATIVE` both read `.antiderivative()` across the query array — `CUMULATIVE` being the running-integral array distinct from `VALUE`, never folded into the evaluate arm) so the spline-kind interpolate floor is `vmap`/`grad`/`jit`-compatible and differentiable through both the sample knots and the read-out derivative; the method-only kinds (`LINEAR`/`CUBIC2`/`CATMULL_ROM`) carry no `.antiderivative()`, so `_cumulative_readout` folds the running cumulative-trapezoid of their evaluated samples for the `ANTIDERIVATIVE`/`CUMULATIVE` readout rather than dropping it to `VALUE`, realizing the `[INTERPAX_QUADAX_USAGE]` deferred consumer. The 2-D/3-D `interp2d`/`interp3d` and `fft_interp*` family is the `solvers/field` consumer half, not this 1-D route.
- [SKFEM_SOLVE]: the `condense`/`solve` spellings carry the `python_version<'3.15'` marker and verify against `compute/.api/scikit-fem.md`; this route owns only the solve half — the `Basis`/`asm` assembly lives on `solvers/mesh.md#MESH` and reaches this route as the `AssembledSystem` artifact. `condense(A, b, x=seed, D=dirichlet_dofs, expand=False)` eliminates the constrained dofs and returns the reduced `(cond_a, cond_b)` system (the catalogue's eliminate-and-restore route, `expand=False` returning the reduced system for the caller's own solver rather than restoring the full-length solution from a skfem solve), and the caller's `SparseScheme`/`LinearPolicy` solve that condensed system through `_sparse_receipt(LinearMap.SparseMat(cond_a, MatrixStructure.SYMMETRIC), cond_b, scheme, policy)` over the `solvers/linear.md#LINEAR` owner — a small stiffness picking the default `Spsolve`, a large SPD one picking `Krylov(KrylovKind.CG)` reading the `SYMMETRIC` structure axis or a reusable `Splu` factor — so the FEM solve reaches the full sparse-route vocabulary and emits the identical sparse convergence receipt (including the Krylov `_info_status` verdict and the honest `‖cond_a @ x - cond_b‖` residual) as a direct sparse system, never a parallel FEM-receipt struct, never a hardcoded scheme, and never a circular `_sparse_receipt(stiffness, stiffness @ solution, scheme, policy)` re-solve that recovers the solution and reports a meaningless residual. The element family is the `ElementKind` enum declared on this owner; `solvers/mesh.md#MESH` keys it onto its own `_CTOR` `(Mesh*, Element*, cell-type)` triple and the `assemble` fold resolves the `Element*` constructor through `getattr(skfem, _CTOR[element][1])`, spanning the full P1/P2 line/tri/tet plus the bilinear-quad and trilinear-hex elements once the scikit-fem wheel resolves. This FEM solve never constructs a basis — it condenses an already-assembled `AssembledSystem` — so no constructor table lives here.
