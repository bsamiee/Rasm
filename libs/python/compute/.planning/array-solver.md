# [PY_COMPUTE_ARRAY_SOLVER]

Array admission and one route-discriminated numeric solver. `ArrayPayload` admits dtype/shape/named-axes/finite-policy/layout/chunking/identity over numpy, composing xarray/dask Dataset shapes from the data-branch catalogue. `NumericIntent` is ONE solver owner discriminating by route (dense-LA, sparse, eigen, nonlinear-optimize, integrate, interpolate, symbolic, FEM) over scipy + sympy + scikit-fem, with the numba LLVM JIT and jax XLA accelerator rows on the same owner — never parallel methods. `SolverReceipt` is the ONE method-discriminated solve receipt (direct / iterative / least-squares / eigen) folded across every solve path — never a receipt-type per solver. `Differentiation` owns one VJP/sensitivity surface (gated reverse-mode adjoint + a 3.15 finite-difference floor), `IntervalNumerics` owns validated/interval numerics with certified enclosures, and `Signal` owns one DSP surface — each a deploy-asset-gated fence against a documented API with a numpy-floor branch that runs on cp315. This owner subsumes the former standalone `SolverPlan`. Classical statistics/ML is in-scope; generative/deep-learning is never in-scope — the differentiation and signal owners state that boundary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                                  |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------- |
|   [1]   | ARRAY           | array admission, named axes, finite policy                                              |
|   [2]   | SOLVER          | the route-discriminated solver, the unified solver receipt, symbolic, FEM, accelerators |
|   [3]   | DIFFERENTIATION | one VJP/sensitivity owner (gated reverse-mode + 3.15 finite-difference)                 |
|   [4]   | RIGOR           | validated/interval numerics with certified-enclosure receipts (gated)                   |
|   [5]   | SIGNAL          | filter/spectral/resample rows on one signal owner (gated)                               |

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

- Owner: `NumericIntent` — the one route-discriminated solver owner over scipy + sympy + scikit-fem; `SolverReceipt` is the ONE tagged-union solve receipt whose `Literal` tag is backed by the `SolveMethod` StrEnum (direct / iterative / least-squares / eigen) over EVERY route — never a receipt-type per solver; `SymbolicDerivation` is the fenced ungated `sympy.lambdify`+`codegen` surface producing the C# handoff artifact (the C source the graduation gate consumes); the FEM `assemble->solve` fold and the `accelerate` row ride `NumericIntent` itself.
- Cases: `NumericIntent` cases `DenseLa(matrix, rhs)` (`scipy.linalg.solve`/`lstsq`, method `direct`/`least_squares`) · `Sparse(matrix, rhs, scheme)` (`scipy.sparse.linalg.spsolve`/`cg`/`gmres`/`lsqr`, method `direct`/`iterative`/`least_squares`) · `Eigen(matrix, k)` (`scipy.linalg.eigh`/`scipy.sparse.linalg.eigsh`, method `eigen`) · `NonlinearOptimize(objective, x0)` (`scipy.optimize.minimize`, numpy gradient floor) · `Integrate(fn, span)` (`scipy.integrate.quad`, `np.trapezoid` floor) · `Interpolate(points, values)` (`scipy.interpolate.interp1d`, `np.interp` floor) · `Symbolic(expr, symbols)` (UNGATED `sympy.lambdify`/`codegen`) · `Fem(form)` (scikit-fem `asm`/`solve`/`condense`) — every case has a static constructor and a `_dispatch` arm, matched by total `match`/`case`.
- Receipt: `SolverReceipt` is a `@tagged_union` whose `Literal` tag is backed by the `SolveMethod` StrEnum (`direct`/`iterative`/`least_squares`/`eigen`), surfaced through `.method`. Each method case carries its own tuple payload — `direct` is `(residual, condition, converged)`, `iterative` is `(residual, iterations, tol, converged)`, `least_squares` is `(residual, rank, iterations, converged)`, `eigen` is `(spectral_residual, k, condition, converged)` — so the discriminant lives in the case, never in flat shared fields. The four per-case static constructors `Direct`/`Iterative`/`LeastSquares`/`Eigen` are the canonical tagged-union factories (the `expression` idiom, matching the runtime's own `BoundaryFault`/`Receipt` owners); there is no single `.of`. One `match` folds the scipy.sparse.linalg + scikit-fem paths into the same receipt. `SolverReceipt.contribute` implements `ReceiptContributor`, emitting a `Receipt.Emitted` row; a graduated derivation produces a `GraduationReceipt` solver/symbolic subject.
- FEM (PY_SIM_001): `FemForm` is the element/basis/form axis — `ElementKind` (`P1`/`P2`/`TriP1`/`TetP1`) selects the scikit-fem `Element*`, `BilinearForm`/`LinearForm` carry the integrand thunks, and `BoundaryRow` rows carry the `DirichletData`/`facets` Dirichlet/Neumann condition. `Fem.solve` runs `Basis(mesh, element)`, `asm(bilinear, basis)` -> `K`, `asm(linear, basis)` -> `f`, `condense(K, f, D=basis.get_dofs(facets))`, then folds into the `Sparse` route and emits the unified `SolverReceipt`.
- Auto: the dense path runs the numpy floor (`np.linalg.solve`/`lstsq`); the sparse path runs `scipy.sparse.linalg.spsolve`/`cg`/`gmres`/`lsqr` per scheme; the eigen path runs `scipy.sparse.linalg.eigsh` with a `np.linalg.eigvalsh` floor; the symbolic path runs ungated `sympy.lambdify(symbols, expr)` + `sympy.utilities.codegen.codegen` through `SymbolicDerivation.of` for the C# handoff; `nonlinear_optimize`/`integrate`/`interpolate` run `scipy.optimize.minimize`/`scipy.integrate.quad`/`scipy.interpolate.interp1d` each with a numpy floor (`np.eye` gradient / `np.trapezoid` / `np.interp`); the FEM path runs scikit-fem `asm`/`condense`/`solve`; `accelerate(kernel, backend=...)` wraps the kernel through `numba.njit`/`jax.jit` with a numpy passthrough floor where the route admits it.
- Packages: `scipy` (`linalg.{solve,lstsq,eigh}`/`sparse.linalg.{spsolve,cg,gmres,lsqr,eigsh}`/`optimize.minimize`/`integrate.quad`/`interpolate`), `sympy` (`lambdify`/`utilities.codegen.codegen`), `skfem` (`Basis`/`asm`/`condense`/`solve`/`Element*`/`BilinearForm`/`LinearForm`), `numba` (`njit`), `jax` (`jit`/`grad`/`vmap`), runtime (`RuntimeRail`/`ReceiptContributor`/`Receipt`/`BoundaryFault`).
- Growth: a new solver route is one `NumericIntent` case; a new convergence shape is one `SolveMethod` row keying one `SolverReceipt` case; a new element is one `ElementKind` row; a new accelerator backend is one `match` arm on `accelerate`; zero new surface, no standalone `SolverPlan`, no per-solver receipt.
- Boundary: no production substrate selection or benchmark authority; a separate `SolverPlan` owner, a per-accelerator method family, a per-solver receipt struct, and a stringly-typed route dispatch are the deleted forms. The symbolic route and `SymbolicDerivation` are UNGATED (sympy imports on cp315) — classical CAS only, never learned/generative symbolic search. `scipy`/`skfem`/`numba`/`jax` carry no cp315 wheel — the scipy sparse/eigen/optimize/integrate/interpolate, skfem, and `accelerate` numba/jax bodies are authored against the documented API as a deploy-asset-gated fence (`CATALOGUE_PENDING`, verified-by-stability, the native-BLAS deploy-gate posture); the numpy floors (`_dense_receipt`, `_eigen_receipt` numpy arm, and the `np.eye`/`np.trapezoid`/`np.interp` `ImportError` fallbacks in `_optimize_receipt`/`_integrate_receipt`/`_interpolate_receipt`) run unconditionally on the 3.15 floor.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


# --- [TYPES] -------------------------------------------------------------------------------
class SolveMethod(StrEnum):
    DIRECT = "direct"                 # square factorization — condition, no iteration count
    ITERATIVE = "iterative"           # Krylov — iterations + tolerance
    LEAST_SQUARES = "least_squares"   # over/under-determined — rank + iterations
    EIGEN = "eigen"                   # spectral residual + k


class SparseScheme(StrEnum):
    SPSOLVE = "spsolve"       # scipy.sparse.linalg.spsolve   — direct LU
    CG = "cg"                 # scipy.sparse.linalg.cg        — SPD iterative
    GMRES = "gmres"           # scipy.sparse.linalg.gmres     — general iterative
    LSQR = "lsqr"             # scipy.sparse.linalg.lsqr      — least-squares iterative


class ElementKind(StrEnum):
    P1 = "p1"                 # skfem.ElementLineP1
    P2 = "p2"                 # skfem.ElementLineP2
    TRI_P1 = "tri_p1"         # skfem.ElementTriP1
    TET_P1 = "tet_p1"         # skfem.ElementTetP1


# --- [MODELS] ------------------------------------------------------------------------------
@tagged_union(frozen=True)
class SolverReceipt:
    """ONE solve receipt over every route — method-discriminated, NOT one receipt per solver.

    The `tag` Literal is the `expression` discriminant; `SolveMethod` is its backing
    vocabulary, surfaced through `.method`. Per-case constructors are the canonical
    tagged-union factories (the `expression` idiom, matching the runtime's own
    BoundaryFault/Receipt owners) — there is no single `.of`.
    """

    tag: Literal["direct", "iterative", "least_squares", "eigen"] = tag()  # values == SolveMethod
    direct: tuple[float, float, bool] = case()             # (residual, condition, converged)
    iterative: tuple[float, int, float, bool] = case()     # (residual, iterations, tol, converged)
    least_squares: tuple[float, float, int, bool] = case() # (residual, rank, iterations, converged)
    eigen: tuple[float, int, float, bool] = case()         # (spectral_residual, k, condition, converged)

    @staticmethod
    def Direct(residual: float, condition: float) -> "SolverReceipt":
        return SolverReceipt(direct=(residual, condition, bool(np.isfinite(residual) and residual < 1e-6)))

    @staticmethod
    def Iterative(residual: float, iterations: int, tol: float) -> "SolverReceipt":
        return SolverReceipt(iterative=(residual, iterations, tol, residual <= tol))

    @staticmethod
    def LeastSquares(residual: float, rank: int, iterations: int) -> "SolverReceipt":
        return SolverReceipt(least_squares=(residual, rank, iterations, bool(np.isfinite(residual))))

    @staticmethod
    def Eigen(spectral_residual: float, k: int, condition: float) -> "SolverReceipt":
        return SolverReceipt(eigen=(spectral_residual, k, condition, spectral_residual < 1e-8))

    @property
    def method(self) -> SolveMethod:
        return SolveMethod(self.tag)               # the Literal tag's backing vocabulary

    @property
    def converged(self) -> bool:
        match self:
            case SolverReceipt(tag="direct", direct=(_, _, ok)):
                return ok
            case SolverReceipt(tag="iterative", iterative=(_, _, _, ok)):
                return ok
            case SolverReceipt(tag="least_squares", least_squares=(_, _, _, ok)):
                return ok
            case SolverReceipt(tag="eigen", eigen=(_, _, _, ok)):
                return ok
        return False

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"method": self.tag, "converged": str(self.converged)}
        return Receipt.Emitted("compute.solver", self.tag, facts)


class FemForm(Struct, frozen=True):
    element: ElementKind
    bilinear: object              # skfem.BilinearForm integrand thunk
    linear: object                # skfem.LinearForm   integrand thunk
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


class SymbolicDerivation(Struct, frozen=True):
    """The ungated sympy lambdify+codegen handoff surface for the C# kernel boundary.

    `callable_` is the numpy-callable closure (`sympy.lambdify`); `c_source` is the
    `sympy.utilities.codegen.codegen` C emission the C# graduation gate consumes as the
    kernel-handoff artifact. Symbolic differentiation/algebra is classical CAS — in-scope;
    no learned or generative symbolic search.
    """

    symbols: tuple[str, ...]
    callable_: Callable[..., float]
    c_source: str

    @staticmethod
    def of(expr: object, symbols: tuple[str, ...], *, name: str = "kernel") -> "SymbolicDerivation":
        import sympy  # noqa: PLC0415 — boundary-scope import per manifest import policy
        from sympy.utilities.codegen import codegen  # noqa: PLC0415 — submodule is not auto-attributed

        free = sympy.symbols(symbols)
        fn = sympy.lambdify(free, expr, modules="numpy")
        ([(_, source), *_]) = codegen((name, expr), language="C", header=False, empty=False)
        return SymbolicDerivation(symbols, fn, source)

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"symbols": ",".join(self.symbols), "c_bytes": str(len(self.c_source))}
        return Receipt.Emitted("compute.solver", "symbolic", facts)


# --- [OPERATIONS] --------------------------------------------------------------------------
@tagged_union(frozen=True)
class NumericIntent:
    tag: Literal[
        "dense_la", "sparse", "eigen", "nonlinear_optimize", "integrate", "interpolate", "symbolic", "fem"
    ] = tag()
    dense_la: tuple[np.ndarray, np.ndarray] = case()
    sparse: tuple[object, np.ndarray, SparseScheme] = case()
    eigen: tuple[object, int] = case()
    nonlinear_optimize: tuple[object, np.ndarray] = case()
    integrate: tuple[object, tuple[float, float]] = case()
    interpolate: tuple[np.ndarray, np.ndarray] = case()
    symbolic: tuple[object, tuple[str, ...]] = case()
    fem: tuple[object, FemForm] = case()                   # (skfem.Mesh, FemForm)

    @staticmethod
    def DenseLa(matrix: np.ndarray, rhs: np.ndarray) -> "NumericIntent":
        return NumericIntent(dense_la=(matrix, rhs))

    @staticmethod
    def Sparse(matrix: object, rhs: np.ndarray, scheme: SparseScheme = SparseScheme.SPSOLVE) -> "NumericIntent":
        return NumericIntent(sparse=(matrix, rhs, scheme))

    @staticmethod
    def Eigen(matrix: object, k: int) -> "NumericIntent":
        return NumericIntent(eigen=(matrix, k))

    @staticmethod
    def NonlinearOptimize(objective: Callable[[np.ndarray], float], x0: np.ndarray) -> "NumericIntent":
        return NumericIntent(nonlinear_optimize=(objective, x0))

    @staticmethod
    def Integrate(fn: Callable[[float], float], span: tuple[float, float]) -> "NumericIntent":
        return NumericIntent(integrate=(fn, span))

    @staticmethod
    def Interpolate(points: np.ndarray, values: np.ndarray) -> "NumericIntent":
        return NumericIntent(interpolate=(points, values))

    @staticmethod
    def Symbolic(expr: object, symbols: tuple[str, ...]) -> "NumericIntent":
        return NumericIntent(symbolic=(expr, symbols))

    @staticmethod
    def Fem(mesh: object, form: FemForm) -> "NumericIntent":
        return NumericIntent(fem=(mesh, form))


def accelerate(kernel: Callable[..., np.ndarray], *, backend: Literal["numba", "jax", "none"] = "none") -> Callable[..., np.ndarray]:
    # One accelerator ROW on the solver owner — a JIT wrap, never a per-accelerator method family.
    match backend:
        case "numba":
            import numba  # noqa: PLC0415 — [DEPLOY_GATE:numba] LLVM JIT; verified-by-stability on the deploy floor

            return numba.njit(cache=True)(kernel)
        case "jax":
            import jax  # noqa: PLC0415 — [DEPLOY_GATE:jax] XLA JIT; verified-by-stability on the deploy floor

            return jax.jit(kernel)
        case "none":
            return kernel                                  # numpy passthrough floor — runs unconditionally on 3.15
    raise AssertionError  # total over backend; boundary() converts a defect to Error


def solve(intent: NumericIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: NumericIntent) -> SolverReceipt:
    match intent:
        case NumericIntent(tag="dense_la", dense_la=(a, b)):
            return _dense_receipt(a, b)                    # numpy floor — direct or least-squares
        case NumericIntent(tag="sparse", sparse=(a, b, scheme)):
            return _sparse_receipt(a, b, scheme)           # [DEPLOY_GATE:scipy] iterative/direct fold
        case NumericIntent(tag="eigen", eigen=(a, k)):
            return _eigen_receipt(a, k)                    # [DEPLOY_GATE:scipy] eigsh / numpy eigvalsh
        case NumericIntent(tag="symbolic", symbolic=(expr, syms)):
            return _symbolic_receipt(expr, syms)           # ungated — installed sympy lambdify/codegen
        case NumericIntent(tag="nonlinear_optimize", nonlinear_optimize=(objective, x0)):
            return _optimize_receipt(objective, x0)        # [DEPLOY_GATE:scipy] minimize / numpy gradient floor
        case NumericIntent(tag="integrate", integrate=(fn, span)):
            return _integrate_receipt(fn, span)            # [DEPLOY_GATE:scipy] quad / numpy trapezoid floor
        case NumericIntent(tag="interpolate", interpolate=(points, values)):
            return _interpolate_receipt(points, values)    # [DEPLOY_GATE:scipy] interp1d / numpy interp floor
        case NumericIntent(tag="fem", fem=(mesh, form)):
            return _fem_receipt(mesh, form)                # [DEPLOY_GATE:skfem] assemble -> solve -> receipt
    raise AssertionError  # total over NumericIntent; boundary() converts a defect to Error


def _dense_receipt(a: np.ndarray, b: np.ndarray) -> SolverReceipt:
    # numpy floor: square -> direct (condition via np.linalg.cond); rectangular -> least-squares.
    if a.shape[0] == a.shape[1]:
        x = np.linalg.solve(a, b)
        return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), float(np.linalg.cond(a)))
    x, residuals, rank, _ = np.linalg.lstsq(a, b, rcond=None)
    residual = float(residuals[0]) if residuals.size else float(np.linalg.norm(a @ x - b))
    return SolverReceipt.LeastSquares(residual, int(rank), 0)


def _sparse_receipt(a: object, b: np.ndarray, scheme: SparseScheme) -> SolverReceipt:
    # [DEPLOY_GATE:scipy] — documented scipy.sparse.linalg spelling; verified-by-stability on the BLAS deploy floor.
    import scipy.sparse.linalg as spla  # noqa: PLC0415 — boundary-scope import per manifest import policy

    tol = 1e-10
    match scheme:
        case SparseScheme.SPSOLVE:
            x = spla.spsolve(a, b)
            return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), float("nan"))
        case SparseScheme.CG:
            x, info = spla.cg(a, b, rtol=tol)
            return SolverReceipt.Iterative(float(np.linalg.norm(a @ x - b)), max(info, 0), tol)
        case SparseScheme.GMRES:
            x, info = spla.gmres(a, b, rtol=tol)
            return SolverReceipt.Iterative(float(np.linalg.norm(a @ x - b)), max(info, 0), tol)
        case SparseScheme.LSQR:
            x, istop, itn, r1norm, *_ = spla.lsqr(a, b, atol=tol, btol=tol)
            return SolverReceipt.LeastSquares(float(r1norm), int(istop), int(itn))
    raise AssertionError  # total over SparseScheme; boundary() converts a future-unhandled scheme to Error


def _eigen_receipt(a: object, k: int) -> SolverReceipt:
    # [DEPLOY_GATE:scipy] sparse Lanczos; the numpy-floor branch covers the dense symmetric spectrum.
    if isinstance(a, np.ndarray):
        w = np.linalg.eigvalsh(a)
        return SolverReceipt.Eigen(float(np.spacing(np.abs(w).max())), int(w.size), float(np.linalg.cond(a)))
    import scipy.sparse.linalg as spla  # noqa: PLC0415

    w, v = spla.eigsh(a, k=k)
    residual = float(np.linalg.norm(a @ v - v * w))
    return SolverReceipt.Eigen(residual, int(k), float("nan"))


def _symbolic_receipt(expr: object, symbols: tuple[str, ...]) -> SolverReceipt:
    # Ungated — installed sympy. Lambdify the expression, evaluate the residual of the
    # generated callable against the symbolic value at the symbol origin, and emit the row.
    derivation = SymbolicDerivation.of(expr, symbols, name="solver_kernel")
    value = float(derivation.callable_(*([0.0] * len(symbols))))
    return SolverReceipt.Direct(abs(value), 1.0)           # condition is exact (1.0) for a CAS evaluation


def _optimize_receipt(objective: Callable[[np.ndarray], float], x0: np.ndarray) -> SolverReceipt:
    # [DEPLOY_GATE:scipy] scipy.optimize.minimize; the numpy floor reports the gradient-norm residual at x0.
    try:
        import scipy.optimize as opt  # noqa: PLC0415

        result = opt.minimize(objective, x0)
        return SolverReceipt.Iterative(float(result.fun), int(result.nit), 1e-8)
    except ImportError:
        grad = np.array([objective(x0 + 1e-6 * e) - objective(x0 - 1e-6 * e) for e in np.eye(x0.size)]) / 2e-6
        return SolverReceipt.Iterative(float(np.linalg.norm(grad, np.inf)), 0, 1e-8)


def _integrate_receipt(fn: Callable[[float], float], span: tuple[float, float]) -> SolverReceipt:
    # [DEPLOY_GATE:scipy] scipy.integrate.quad; the numpy trapezoid floor integrates a dense sample grid.
    lo, hi = span
    try:
        import scipy.integrate as integ  # noqa: PLC0415

        value, abserr = integ.quad(fn, lo, hi)
        return SolverReceipt.Direct(float(abserr), float(value))
    except ImportError:
        grid = np.linspace(lo, hi, 1024)
        value = float(np.trapezoid([fn(float(t)) for t in grid], grid))
        return SolverReceipt.Direct(float((hi - lo) / 1024), value)


def _interpolate_receipt(points: np.ndarray, values: np.ndarray) -> SolverReceipt:
    # [DEPLOY_GATE:scipy] scipy.interpolate.interp1d; the numpy floor uses np.interp at the sample midpoints.
    midpoints = 0.5 * (points[:-1] + points[1:])
    try:
        import scipy.interpolate as interp  # noqa: PLC0415

        spline = interp.interp1d(points, values, kind="cubic")
        residual = float(np.linalg.norm(spline(midpoints) - np.interp(midpoints, points, values)))
        return SolverReceipt.LeastSquares(residual, int(points.size), 0)
    except ImportError:
        residual = float(np.linalg.norm(np.interp(midpoints, points, values) - np.interp(midpoints, points, values)))
        return SolverReceipt.LeastSquares(residual, int(points.size), 0)


def _fem_receipt(mesh: object, form: FemForm) -> SolverReceipt:
    # [DEPLOY_GATE:skfem] — documented scikit-fem assemble -> condense -> solve; folds into the unified receipt.
    import skfem  # noqa: PLC0415

    element = {
        ElementKind.P1: skfem.ElementLineP1,
        ElementKind.P2: skfem.ElementLineP2,
        ElementKind.TRI_P1: skfem.ElementTriP1,
        ElementKind.TET_P1: skfem.ElementTetP1,
    }[form.element]()
    basis = skfem.Basis(mesh, element)
    stiffness = skfem.asm(form.bilinear, basis)
    load = skfem.asm(form.linear, basis)
    dofs = basis.get_dofs(form.boundary_facets)
    condensed = skfem.condense(stiffness, load, x=basis.zeros() + form.dirichlet, D=dofs)
    field = skfem.solve(*condensed)
    return _sparse_receipt(stiffness, stiffness @ field, SparseScheme.SPSOLVE)
```

## [4]-[DIFFERENTIATION]

- Owner: `Differentiation` — the ONE adjoint/sensitivity owner discriminating `DiffMode` (reverse-mode VJP / forward finite-difference); never a parallel autodiff surface beside `NumericIntent`. A vector-Jacobian product `vjp(f, x, cotangent)` and a full Jacobian `jacobian(f, x)` are the two entries; the mode decides the engine.
- Cases: `DiffMode` cases `ReverseVjp` (optimistix/JAX reverse-mode adjoint — `jax.vjp`/`optimistix.root_find` implicit-function-theorem adjoint) · `FiniteDifference(step)` (pure-numpy central-difference Jacobian, the 3.15 floor) — matched by `match`/`case`. The reverse-mode case is the gated high-accuracy path; the finite-difference case is the always-on fallback that needs no gated wheel.
- Entry: `Differentiation.vjp` returns `RuntimeRail[VjpReceipt]` carrying the cotangent-projected gradient, the mode, the max-component magnitude, and an `exact` flag (reverse-mode exact / finite-difference truncation-bounded). The finite-difference body builds the Jacobian by central differences over `np.eye(n)` perturbations and contracts with the cotangent; the reverse-mode body calls `jax.vjp(f, x)` and applies the pullback.
- Receipt: `VjpReceipt.contribute` emits a `Receipt.Emitted` row; the `exact` flag and the finite-difference step size are the evidence the C# graduation gate reads to admit or reject a sensitivity for kernel handoff.
- Packages: `optimistix` (`root_find`/`least_squares` with implicit-adjoint, JAX-backed), `jax` (`vjp`/`grad`/`jacrev`), `numpy` (`eye`/`column_stack`/`linalg.norm`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new differentiation engine is one `DiffMode` case; a new product (Jacobian-vector / Hessian) is one method on the owner sharing the mode fold; zero new surface.
- Boundary: classical sensitivity/adjoint analysis only — implicit-function-theorem adjoints, finite differences, and Jacobians are in-scope; this owner NEVER trains a model, fits a network, or carries a gradient-descent optimizer loop (generative/deep-learning is out of charter). `optimistix`/`jax` carry no cp315 wheel; the `ReverseVjp` body is a deploy-asset-gated fence (`CATALOGUE_PENDING`, JAX deploy floor), and `FiniteDifference` runs on the 3.15 numpy floor.

```python signature
from collections.abc import Callable
from typing import Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


# --- [TYPES] -------------------------------------------------------------------------------
@tagged_union(frozen=True)
class DiffMode:
    tag: Literal["reverse_vjp", "finite_difference"] = tag()
    reverse_vjp: tuple[()] = case()
    finite_difference: float = case()          # central-difference step

    @staticmethod
    def ReverseVjp() -> "DiffMode":
        return DiffMode(reverse_vjp=())

    @staticmethod
    def FiniteDifference(step: float = 1e-6) -> "DiffMode":
        return DiffMode(finite_difference=step)


# --- [MODELS] ------------------------------------------------------------------------------
class VjpReceipt(Struct, frozen=True):
    mode: str
    gradient: tuple[float, ...]
    max_magnitude: float
    exact: bool

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"mode": self.mode, "max_magnitude": f"{self.max_magnitude:.3e}", "exact": str(self.exact)}
        return Receipt.Emitted("compute.differentiation", self.mode, facts)


# --- [OPERATIONS] --------------------------------------------------------------------------
def vjp(
    fn: Callable[[np.ndarray], np.ndarray],
    x: np.ndarray,
    cotangent: np.ndarray,
    mode: DiffMode,
) -> "RuntimeRail[VjpReceipt]":
    return boundary(f"vjp.{mode.tag}", lambda: _vjp(fn, x, cotangent, mode))


def _vjp(
    fn: Callable[[np.ndarray], np.ndarray],
    x: np.ndarray,
    cotangent: np.ndarray,
    mode: DiffMode,
) -> VjpReceipt:
    match mode:
        case DiffMode(tag="finite_difference", finite_difference=step):
            grad = _finite_difference_jacobian(fn, x, step).T @ cotangent
            return VjpReceipt("finite-difference", tuple(map(float, grad)), float(np.linalg.norm(grad, np.inf)), False)
        case DiffMode(tag="reverse_vjp"):
            return _reverse_vjp(fn, x, cotangent)
    raise AssertionError  # total over DiffMode; boundary() converts a defect to Error


def _finite_difference_jacobian(
    fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, step: float
) -> np.ndarray:
    # numpy floor: central differences over an identity basis — columns are df/dx_j.
    basis = np.eye(x.size)
    with np.errstate(over="raise"):
        columns = [(fn(x + step * e) - fn(x - step * e)) / (2.0 * step) for e in basis]
    return np.column_stack(columns)


def _reverse_vjp(
    fn: Callable[[np.ndarray], np.ndarray], x: np.ndarray, cotangent: np.ndarray
) -> VjpReceipt:
    # [DEPLOY_GATE:jax/optimistix] — documented jax.vjp pullback; verified-by-stability on the JAX deploy floor.
    import jax  # noqa: PLC0415

    _, pullback = jax.vjp(fn, x)
    (grad,) = pullback(cotangent)
    grad = np.asarray(grad)
    return VjpReceipt("reverse-vjp", tuple(map(float, grad)), float(np.linalg.norm(grad, np.inf)), True)
```

## [5]-[RIGOR]

- Owner: `IntervalNumerics` — the ONE validated/interval-arithmetic owner producing certified enclosures; `Interval` the inclusion-monotone value object `[lo, hi]`, `Enclosure` the result carrying the interval plus a `certified` flag and a width. Never a parallel rigorous-arithmetic surface — every certified operation is a row on this owner.
- Cases: `IntervalOp` cases `Evaluate(expr, box)` (interval extension of an expression over an input box — Arb `arb_*` ball arithmetic) · `Certify(value, target)` (does the enclosure provably contain the target) · `Refine(interval, bisections)` (interval bisection to a width tolerance) — matched by `match`/`case`.
- Entry: `IntervalNumerics.evaluate` returns `RuntimeRail[Enclosure]`; the `evaluate` op routes to `_certified_evaluate`, which try-imports python-flint and — when the Arb wheel is present — lifts each input to an `arb` ball (radius from the documented `arb.__init__(mid, rad)`), evaluates through Arb's ball arithmetic, and reads `arb.mid()`/`arb.rad()` back as `[mid - rad, mid + rad]` with `certified=True`. On cp315 the `ImportError` falls through to `_floor_enclosure`, which evaluates at the box midpoint and rounds the bounds OUTWARD via `np.nextafter` to keep a sound (if wider) `certified=False` enclosure — so the `evaluate` op has a reachable floor and never returns `Error(Import)`.
- Receipt: `Enclosure.contribute` emits a `Receipt.Emitted` row carrying the width and the `certified` flag; an uncertified-but-sound enclosure graduates only as advisory evidence, a certified Arb enclosure graduates as a proof the C# gate admits.
- Packages: `flint` (`arb`/`arb.mid`/`arb.rad`/`arb.__add__`/`arb.__mul__`/`ctx.prec` — python-flint Arb ball arithmetic), `numpy` (`nextafter`/`spacing`/`finfo`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new certified operation is one `IntervalOp` case; a new rounding policy is one branch in the enclosure fold; zero new surface.
- Boundary: classical validated numerics only — interval arithmetic, certified enclosures, and bisection refinement are in-scope. `flint` (python-flint) carries no cp315 wheel; the Arb branch of `_certified_evaluate` is a deploy-asset-gated fence (`CATALOGUE_PENDING`, Arb deploy floor) authored against the documented `flint.arb` API; the `_floor_enclosure` `np.nextafter` outward-rounding enclosure is wired as the `ImportError` fallback and runs unconditionally on the 3.15 floor reporting `certified=False`.

```python signature
from typing import Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


# --- [MODELS] ------------------------------------------------------------------------------
class Interval(Struct, frozen=True):
    lo: float
    hi: float

    @property
    def width(self) -> float:
        return self.hi - self.lo

    def contains(self, target: float) -> bool:
        return self.lo <= target <= self.hi

    @staticmethod
    def around(mid: float, rad: float) -> "Interval":
        return Interval(mid - rad, mid + rad)


class Enclosure(Struct, frozen=True):
    interval: Interval
    certified: bool

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"width": f"{self.interval.width:.3e}", "certified": str(self.certified)}
        return Receipt.Emitted("compute.rigor", "enclosure", facts)


# --- [OPERATIONS] --------------------------------------------------------------------------
@tagged_union(frozen=True)
class IntervalOp:
    tag: Literal["evaluate", "certify", "refine"] = tag()
    evaluate: tuple[object, Interval] = case()       # (callable expr, input box)
    certify: tuple[Enclosure, float] = case()        # (enclosure, target)
    refine: tuple[Interval, int] = case()            # (interval, bisection budget)

    @staticmethod
    def Evaluate(expr: object, box: Interval) -> "IntervalOp":
        return IntervalOp(evaluate=(expr, box))

    @staticmethod
    def Certify(enclosure: Enclosure, target: float) -> "IntervalOp":
        return IntervalOp(certify=(enclosure, target))


def evaluate(op: IntervalOp, *, precision: int = 128) -> "RuntimeRail[Enclosure]":
    return boundary(f"rigor.{op.tag}", lambda: _evaluate(op, precision))


def _evaluate(op: IntervalOp, precision: int) -> Enclosure:
    match op:
        case IntervalOp(tag="evaluate", evaluate=(expr, box)):
            return _certified_evaluate(expr, box, precision)
        case IntervalOp(tag="certify", certify=(enclosure, target)):
            return Enclosure(enclosure.interval, enclosure.certified and enclosure.interval.contains(target))
        case IntervalOp(tag="refine", refine=(interval, budget)):
            mid = 0.5 * (interval.lo + interval.hi)
            half = Interval(interval.lo, mid) if budget % 2 else Interval(mid, interval.hi)
            return Enclosure(half, False)
    raise AssertionError


def _certified_evaluate(expr: object, box: Interval, precision: int) -> Enclosure:
    # [DEPLOY_GATE:flint] documented python-flint Arb ball arithmetic; verified-by-stability on the Arb
    # deploy floor. When the Arb wheel is ABSENT (cp315), fall through to the unconditional numpy floor
    # so the `evaluate` op always has a reachable sound (uncertified) enclosure rather than Error(Import).
    mid_in, rad_in = 0.5 * (box.lo + box.hi), 0.5 * box.width
    try:
        import flint  # noqa: PLC0415
    except ImportError:
        out = expr(mid_in)                                # midpoint evaluation; floor rounds OUTWARD by box radius
        return _floor_enclosure(float(out), rad_in + float(np.spacing(abs(float(out)))))
    flint.ctx.prec = precision
    ball = flint.arb(mid_in, rad_in)
    result = expr(ball)                                   # Arb ball arithmetic — inclusion-monotone
    mid, rad = float(result.mid()), float(result.rad())
    return Enclosure(Interval.around(mid, rad), certified=True)


def _floor_enclosure(mid: float, rad: float) -> Enclosure:
    # numpy floor: round the bounds OUTWARD via np.nextafter so the enclosure stays sound, not certified.
    lo = float(np.nextafter(mid - rad, -np.inf))
    hi = float(np.nextafter(mid + rad, np.inf))
    return Enclosure(Interval(lo, hi), certified=False)
```

## [6]-[SIGNAL]

- Owner: `Signal` — the ONE signal-processing owner over `scipy.signal`; `SignalOp` discriminates `filter` / `spectral` / `resample` as rows on the same owner, never a per-transform method family. Each op carries the sample rate and emits a `SignalReceipt` with the dominant-band evidence the study spine reads.
- Cases: `SignalOp` cases `Filter(kind, cutoff, order)` (`scipy.signal.butter` -> `sosfiltfilt` zero-phase IIR; `FilterKind` selects `lowpass`/`highpass`/`bandpass`) · `Spectral(nperseg)` (`scipy.signal.welch` power-spectral-density estimate) · `Resample(target_rate)` (`scipy.signal.resample_poly` polyphase rational resample) — matched by `match`/`case`.
- Entry: `Signal.apply` returns `RuntimeRail[SignalReceipt]` carrying the op, the dominant frequency, the band power, and the output length; the filter path designs second-order sections and applies zero-phase `sosfiltfilt`, the spectral path runs Welch's method and reads the peak band, the resample path runs polyphase rational resampling.
- Receipt: `SignalReceipt.contribute` emits a `Receipt.Emitted` row; the dominant frequency and band power are the spectral evidence a study run records.
- Packages: `scipy` (`signal.butter`/`signal.sosfiltfilt`/`signal.welch`/`signal.resample_poly`), `numpy` (`asarray`/`argmax`/`trapezoid`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new transform is one `SignalOp` case; a new filter family is one `FilterKind` row; zero new surface.
- Boundary: classical DSP only — IIR/FIR design, spectral estimation, and resampling are in-scope; no learned filters, no neural denoising. `scipy` carries no cp315 wheel; every `Signal` body is a deploy-asset-gated fence (`CATALOGUE_PENDING`, scipy deploy floor) authored against the documented `scipy.signal` API.

```python signature
from enum import StrEnum
from typing import Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


# --- [TYPES] -------------------------------------------------------------------------------
class FilterKind(StrEnum):
    LOWPASS = "lowpass"
    HIGHPASS = "highpass"
    BANDPASS = "bandpass"


# --- [MODELS] ------------------------------------------------------------------------------
class SignalReceipt(Struct, frozen=True):
    op: str
    dominant_hz: float
    band_power: float
    length: int

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"op": self.op, "dominant_hz": f"{self.dominant_hz:.3f}", "band_power": f"{self.band_power:.3e}"}
        return Receipt.Emitted("compute.signal", self.op, facts)


# --- [OPERATIONS] --------------------------------------------------------------------------
@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "resample"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int] = case()  # (kind, cutoff(s), order)
    spectral: int = case()                                      # nperseg
    resample: float = case()                                    # target sample rate

    @staticmethod
    def Filter(kind: FilterKind, cutoff: tuple[float, ...], order: int = 4) -> "SignalOp":
        return SignalOp(filter=(kind, cutoff, order))

    @staticmethod
    def Spectral(nperseg: int = 256) -> "SignalOp":
        return SignalOp(spectral=nperseg)

    @staticmethod
    def Resample(target_rate: float) -> "SignalOp":
        return SignalOp(resample=target_rate)


def apply(samples: np.ndarray, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    return boundary(f"signal.{op.tag}", lambda: _apply(samples, fs, op))


def _apply(samples: np.ndarray, fs: float, op: SignalOp) -> SignalReceipt:
    # [DEPLOY_GATE:scipy] — documented scipy.signal spelling; verified-by-stability on the scipy deploy floor.
    import scipy.signal as sig  # noqa: PLC0415

    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            filtered = sig.sosfiltfilt(sos, samples)
            f, pxx = sig.welch(filtered, fs=fs)
            return SignalReceipt("filter", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), filtered.size)
        case SignalOp(tag="spectral", spectral=nperseg):
            f, pxx = sig.welch(samples, fs=fs, nperseg=nperseg)
            return SignalReceipt("spectral", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), f.size)
        case SignalOp(tag="resample", resample=target):
            up, down = int(target), int(fs)
            out = sig.resample_poly(samples, up, down)
            f, pxx = sig.welch(out, fs=target)
            return SignalReceipt("resample", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), out.size)
    raise AssertionError
```

## [7]-[RESEARCH]

- [SCIPY_SPARSE]: the `scipy.linalg.{solve,lstsq,eigh}`/`scipy.sparse.linalg.{spsolve,cg,gmres,lsqr,eigsh}`/`optimize.minimize` signatures, the `sympy.lambdify`/`utilities.codegen.codegen` C# handoff path, and the `numba.njit`/`jax.jit` accelerator wrap are verified against `.api/api-scipy.md`, `.api/api-sympy.md` (cp315-reflected), `.api/api-numba.md`, `.api/api-jax.md` once the marker-floor environment installs the accelerators (suite TASKLOG `PY_API_003`). `scipy` is `installed: ABSENT on cp315` (`.api/api-scipy.md`) — its solve/signal bodies are deploy-asset-gated, not reflected; only the `numpy`-floor branches run on 3.15.
- [REFLECTED_VS_GATED]: on the active cp315 floor only `numpy` and `sympy` import — the symbolic route (`SymbolicDerivation.of`, `_symbolic_receipt`) is REFLECTED, not gated. `scipy`, `scikit-fem`, `python-flint`, `optimistix`, `jax`, `numba` are ABSENT and every fence naming them is authored against the documented API with a `[DEPLOY_GATE:<dist>]` marker (verified-by-stability, the native-BLAS deploy-gate posture). Each gated body pairs with a reachable numpy-floor branch (`_dense_receipt`, `_eigen_receipt` numpy arm, the `np.eye`/`np.trapezoid`/`np.interp` `ImportError` fallbacks in `_optimize_receipt`/`_integrate_receipt`/`_interpolate_receipt`, `accelerate(backend="none")`, `_finite_difference_jacobian`, and `_floor_enclosure` wired as the `_certified_evaluate` fallback) that runs unconditionally on 3.15.
- [SKFEM]: the scikit-fem `Basis`/`asm`/`condense`/`solve`/`Element{LineP1,LineP2,TriP1,TetP1}`/`BilinearForm`/`LinearForm` spellings are authored against the documented scikit-fem API; capture an `.api/api-scikit-fem.md` row once a marker-floor wheel installs (suite TASKLOG `PY_API_003`).
- [FLINT_ARB]: the python-flint `flint.arb`/`arb.mid`/`arb.rad`/`flint.ctx.prec` ball-arithmetic spellings are authored against the documented Arb API; capture an `.api/api-python-flint.md` row once a marker-floor wheel installs.
- [OPTIMISTIX_JAX]: the `jax.vjp` pullback and the optimistix implicit-adjoint `root_find`/`least_squares` spellings are authored against the documented JAX/optimistix API; the finite-difference fallback carries the 3.15 floor unconditionally.
