# [PY_COMPUTE_QUADRATURE]

`QuadratureIntent` is the one numeric solver over three routes — 1-D quadrature, 1-D interpolation, and the weak-form finite-element `condense -> solve` fold — every route folding into the one `SolverReceipt`. Variation rides bounded policy values and one catalog row per concept: `QuadKind` keys the `_QUAD` integration-family catalog, `InterpKind` keys the `_INTERP` interpolant catalog, the shared `Readout` axis parameterizes output shape across both numeric routes, and one `QuadPolicy` struct carries every per-call knob. The FEM route owns only the `condense -> solve` half of an already-assembled system and never re-runs a `Basis`/`asm`.

The reused `Readout` axis spans both numeric routes; the FEM route consumes the `AssembledSystem` that `solvers/mesh.md#EXCHANGE` `MeshExchange.assemble` lowers, condenses it through `skfem.condense`, and solves the condensed system through the `solvers/linear.md#LINEAR` public `sparse_receipt` under the caller's `SparseScheme`/`LinearPolicy` against the honest condensed-load residual, while the element axis `ElementKind`/`FemForm`/`CTOR` is owned by `solvers/mesh.md#MESH_FIELD` and imported downward at module top. Each numeric floor climbs the JAX-native `quadax`/`interpax` companion — woven once through the frozen `QuadEngine`, which floats the rail to float64 via `jax.config.update("jax_enable_x64", True)` since the `epsabs=1e-10`/`epsrel=1e-8` tolerances sit below float32 eps and the adaptive termination is otherwise unsatisfiable — then the host `scipy` body, then the unconditional `numpy` floor, the same x64 contract `solvers/linear.md#LINEAR`/`solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` float their rails to. The module-level `_dispatch` kernel crosses the process lane as spec data plus operands, `_MODALITY` pins the gated `integrate`/`interpolate` routes on PROCESS and the scipy-bound FEM on THREAD, and receipt egress rides the hub `evidence_run` weave — span, fence, `@receipted(REDACTION)` harvest — over the `solvers/receipt.md#RECEIPT` `status_of` residual floor every scipy/numpy path defers to with `result=None`.

## [01]-[INDEX]

- [01]-[QUADRATURE]: 1-D quadrature, 1-D interpolation, and the weak-form FEM condense fold on one `QuadratureIntent` owner over a three-floor quadax/scipy/numpy ladder into one `SolverReceipt`.

## [02]-[QUADRATURE]

- Owner: `QuadratureIntent` carries the integrate/interpolate/fem cases on one solver; the `fem` case carries the assembled stiffness/load/dof system itself, never the `MeshField`, so this route condenses and solves and never reaches into mesh assembly.
- Entry: `QuadratureIntent.solve(lane)` is the one union method matching `LinearIntent.solve`/`DifferentialIntent.solve`, composing the `_MODALITY`-routed `lane.offload` under the hub `evidence_run` weave — PROCESS for the gated quadax/interpax routes (x64 is process-global native state), THREAD for the scipy FEM — with `retry=RetryClass.OCCT` wrapping the process isolation leg only, never the deterministic solve.
- Output: the shared `Readout` axis carries output shape for both routes — scalar integral, running antiderivative, `nu`-th derivative, analytic antiderivative — never a `cumulative`/`derivative` boolean knob or parallel `IntegrateOutput`/`InterpOutput` enums.
- Receipt: the adaptive `QuadratureInfo` termination bitfield folds into the `Iterative` receipt's typed `SolveStatus` (estimated error in `residual`, evaluation count in `iterations`), exactly as the sparse Krylov `_info_status` fold does; a `VECTORIZED` per-component `err`/`status` reduces to the worst scalar (`np.max` error, `np.bitwise_or.reduce` flag union), `NO_CONVERGE` decodes to divergence rather than the distinct `MAX_NINTER` step-budget verdict, and every scipy/numpy floor passes `result=None` to the shared `solvers/receipt.md#RECEIPT` `status_of` residual floor.
- Packages: `quadax`/`interpax` the JAX-native differentiable adaptive/fixed/sampled and interpolant floor, `scipy` the host bodies (never the deprecated `interp1d`; no scipy node-derivative Hermite drop-in, so the `HERMITE` scipy floor is the degree-`k` `make_interp_spline` C2 cubic), `skfem` the `condense`/`solve` half only (`Basis`/`asm` stays on `solvers/mesh.md#EXCHANGE`), `jax` the x64 float64 promotion, `numpy` the unconditional floor owning `_prefix_trapezoid` locally (numpy exposes no `cumulative_trapezoid`; that spelling is SciPy-owned), otherwise per the fence imports.
- Growth: a new quadrature rule is one `QuadKind` member plus one `_QUAD` row folded through `QuadEngine.integrate`; a new interpolant family one `InterpKind` member plus one `_INTERP` row through `QuadEngine.interpolant`; a new output shape one `Readout` member; a new integrator knob one `QuadPolicy` field; a new termination code one severity-ranked `_QUAD_STATUS` token; a new element one mesh-owned `CTOR` row read downward; a new FEM sparse scheme zero new surface, since the caller passes any `SparseScheme`/`LinearPolicy` the linear route owns.
- Boundary: the FEM element axis (`ElementKind`/`FemForm`/`CTOR`) is mesh-owned on `solvers/mesh.md#MESH_FIELD` and imported downward at module top — no second element vocabulary, no `TYPE_CHECKING` cycle-dodge over `AssembledSystem`; 2-D/3-D interpolation lives on `solvers/field` (the `interpax` `interp2d`/`interp3d` family) and multidimensional ODE integration on `solvers/differential.md#DIFFERENTIAL`.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.numerics.jit import LoweredSpec
from rasm.compute.solvers.linear import LinearMap, LinearPolicy, MatrixStructure, SparseScheme, sparse_receipt
from rasm.compute.solvers.mesh import AssembledSystem, ElementKind, FemForm
from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass


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


# Output shape shared by both numeric routes: VALUE evaluates or scalar integral, DERIVATIVE the nu-th
# derivative, ANTIDERIVATIVE the analytic antiderivative, CUMULATIVE the running-antiderivative array.
class Readout(StrEnum):
    VALUE = "value"
    DERIVATIVE = "derivative"
    ANTIDERIVATIVE = "antiderivative"
    CUMULATIVE = "cumulative"


# --- [CONSTANTS] ---------------------------------------------------------------------------

# Decoded-message token -> `SolveStatus` member, SEVERITY-ORDERED: the `QuadratureInfo` status is a bitfield
# (NORMAL_EXIT/MAX_NINTER/ROUNDOFF/BAD_INTEGRAND/NO_CONVERGE combine), so the first match in this descending-severity tuple wins.
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


# family modality rows: gated quadax/interpax pin PROCESS (x64 flag is process-global native state), scipy FEM rides THREAD.
_MODALITY: Final[Map[str, Modality]] = Map.of_seq([
    ("integrate", Modality.PROCESS),
    ("interpolate", Modality.PROCESS),
    ("fem", Modality.THREAD),
])


# --- [MODELS] ------------------------------------------------------------------------------


# ONE policy struct across both numeric routes (the differential.md single-policy discipline): unused fields
# simply unread per route. `adaptive=False` selects the fixed_quad* constant-cost form, `readout=CUMULATIVE` the running-antiderivative path.
class QuadPolicy(Struct, frozen=True):
    epsabs: float = 1e-10
    epsrel: float = 1e-8
    order: int = 21  # adaptive Kronrod/Chebyshev/tanh-sinh node order
    max_ninter: int = 50  # adaptive subdivision cap
    divmax: int = 20  # Romberg extrapolation table depth
    fixed_nodes: int = 21  # constant-cost node count when `adaptive=False`
    floor_nodes: int = 1024  # numpy trapezoid floor grid count
    adaptive: bool = True
    nu: int = 1  # derivative order both interpax `__call__(x, nu=...)` and `<spline>.derivative(nu)` read
    extrapolate: bool = True
    bspline_k: int = 3  # B-spline degree for the make_interp_spline kind
    readout: Readout = Readout.VALUE


# one quadrature-family row: the adaptive quadax name, the fixed analogue, the flags, and the scipy floor callable.
class QuadRow(Struct, frozen=True):
    adaptive: str  # quadgk / quadcc / quadts / romberg / rombergts; the order/divmax kwarg the row carries
    fixed: str | None  # fixed_quadgk / fixed_quadcc / fixed_quadts; None where no constant-cost analogue
    extrapolated: bool  # True for romberg/rombergts: reads `divmax` not `order`/`max_ninter`
    sampled: bool  # True for SAMPLED_SIMPSON: pre-discretized data, bare-array return, no QuadratureInfo
    scipy: str  # scipy.integrate callable name the host floor reads (quad / quad_vec / tanhsinh / simpson)


# one interpolant-family row: the interpax one-shot method (None where a class exists), the interpax spline class (None for method-only / B-spline), and the scipy class/factory.
class InterpRow(Struct, frozen=True):
    interpax_method: str | None  # interp1d(method=...) kernel for method-only kinds; None where a class exists
    interpax_class: str | None  # JAX-differentiable interpax spline class; None for method-only / B-spline
    scipy_class: str  # modern scipy.interpolate class/factory; never the deprecated interp1d


# The gated quadax/interpax/jax modules folded into ONE frozen value object, behavior built once per `gated()`:
# `integrate` the SINGLE row-keyed (value, QuadratureInfo) driver, `sampled` the bare-array fold, `interpolant` the interpax constructor-and-readout fold.
@dataclass(frozen=True, slots=True)
class QuadEngine:
    quadax: object
    interpax: object

    @classmethod
    def gated(cls) -> Self:
        import interpax
        import jax
        import quadax
        import quadax.sampled  # bind the sampled submodule the SAMPLED_SIMPSON / CUMULATIVE rows read

        jax.config.update("jax_enable_x64", True)  # epsabs 1e-10 / epsrel 1e-8 are below float32 eps; JAX defaults to float32
        return cls(quadax=quadax, interpax=interpax)

    # The ONE callable-integrand driver: `row.adaptive` names the quadax specialization, so one row-keyed call site
    # spans every family — `not adaptive` swaps to fixed_quad* over scalar bounds, `extrapolated` passes `divmax`, the rest `order`+`max_ninter`.
    def integrate(self, row: QuadRow, fn: object, lo: float, hi: float, policy: "QuadPolicy") -> tuple[object, object]:
        qx, interval = self.quadax, np.asarray([lo, hi])
        if not policy.adaptive and row.fixed is not None:
            return getattr(qx, row.fixed)(fn, lo, hi, n=policy.fixed_nodes)
        if row.extrapolated:
            return getattr(qx, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, divmax=policy.divmax)
        return getattr(qx, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, max_ninter=policy.max_ninter, order=policy.order)

    # The ONE quadax.sampled fold: SAMPLED_SIMPSON integrates discretized data (bare array, no QuadratureInfo); CUMULATIVE selects the running antiderivative.
    def sampled(self, samples: np.ndarray, grid: np.ndarray, readout: "Readout") -> np.ndarray:
        fold = self.quadax.sampled.cumulative_simpson if readout is Readout.CUMULATIVE else self.quadax.sampled.simpson
        return np.asarray(fold(samples, x=grid))

    # The ONE interpax builder over the `_INTERP` row: a method-only kind takes interp1d(method=..., derivative=nu),
    # a spline kind the JAX-differentiable class (node_derivatives=True so interpax Hermite reads its dydx); `None` (BSPLINE) routes scipy.
    def interpolant(
        self,
        row: InterpRow,
        points: np.ndarray,
        values: np.ndarray,
        xq: np.ndarray,
        kind: "InterpKind",
        policy: "QuadPolicy",
        dydx: np.ndarray | None,
    ) -> np.ndarray | None:
        if row.interpax_method is not None:
            nu = policy.nu if policy.readout is Readout.DERIVATIVE else 0
            base = np.asarray(self.interpax.interp1d(xq, points, values, method=row.interpax_method, derivative=nu))
            return _cumulative_readout(base, xq, policy.readout)
        if row.interpax_class is None:
            return None
        spline = _construct(getattr(self.interpax, row.interpax_class), points, values, kind, policy, dydx, node_derivatives=True)
        return _read_spline(spline, xq, policy)


@tagged_union(frozen=True)
class QuadratureIntent:
    tag: Literal["integrate", "interpolate", "fem"] = tag()
    integrate: tuple[object, tuple[float, float], QuadKind, QuadPolicy] = case()
    interpolate: tuple[np.ndarray, np.ndarray, np.ndarray | None, InterpKind, QuadPolicy, np.ndarray | None] = case()
    fem: tuple[AssembledSystem, float, SparseScheme, LinearPolicy] = case()

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
        system: AssembledSystem, dirichlet: float = 0.0, scheme: SparseScheme = SparseScheme.Spsolve(), policy: LinearPolicy = LinearPolicy()
    ) -> "QuadratureIntent":
        return QuadratureIntent(fem=(system, dirichlet, scheme, policy))

    async def solve(self, lane: LanePolicy) -> "RuntimeRail[SolverReceipt]":
        # the gated routes pin PROCESS (x64 gate applies at worker import), the FEM arm rides THREAD;
        # `retry=RetryClass.OCCT` wraps the process isolation leg only. The weave owns span, fence, and harvest.
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            match _MODALITY[self.tag]:
                case Modality.PROCESS as modality:
                    return await lane.offload(_dispatch, self, modality=modality, retry=RetryClass.OCCT)
                case modality:
                    return await lane.offload(_dispatch, self, modality=modality)

        return await evidence_run(EvidenceScope.QUADRATURE, f"solve.{self.tag}", dispatch)


# --- [TABLES] ------------------------------------------------------------------------------

# QuadKind -> the one family row. VECTORIZED reuses quadgk over a vector integrand (quad_vec host floor);
# SAMPLED_SIMPSON's adaptive slot is unread — the sampled branch routes through quadax.sampled directly.
_QUAD: Final[Map[QuadKind, QuadRow]] = Map.of_seq([
    (QuadKind.GAUSS_KRONROD, QuadRow("quadgk", "fixed_quadgk", False, False, "quad")),
    (QuadKind.CLENSHAW_CURTIS, QuadRow("quadcc", "fixed_quadcc", False, False, "quad")),
    (QuadKind.ROMBERG, QuadRow("romberg", None, True, False, "quad")),
    (QuadKind.ROMBERG_TS, QuadRow("rombergts", None, True, False, "tanhsinh")),
    (QuadKind.TANH_SINH, QuadRow("quadts", "fixed_quadts", False, False, "tanhsinh")),
    (QuadKind.VECTORIZED, QuadRow("quadgk", "fixed_quadgk", False, False, "quad_vec")),
    (QuadKind.SAMPLED_SIMPSON, QuadRow("simpson", None, False, True, "simpson")),
])

# InterpKind -> the one family row: method-only kinds (LINEAR/CUBIC2/CATMULL_ROM) carry an interpax method and no
# class; spline kinds carry the interpax class; HERMITE's scipy floor is the degree-`k` make_interp_spline (no scipy
# node-derivative drop-in); BSPLINE carries neither interpax surface and routes to scipy or the np.interp floor.
_INTERP: Final[Map[InterpKind, InterpRow]] = Map.of_seq([
    (InterpKind.LINEAR, InterpRow("linear", None, "make_interp_spline")),
    (InterpKind.CUBIC2, InterpRow("cubic2", None, "make_interp_spline")),
    (InterpKind.CATMULL_ROM, InterpRow("catmull-rom", None, "make_interp_spline")),
    (InterpKind.CUBIC, InterpRow(None, "CubicSpline", "CubicSpline")),
    (InterpKind.PCHIP, InterpRow(None, "PchipInterpolator", "PchipInterpolator")),
    (InterpKind.AKIMA, InterpRow(None, "Akima1DInterpolator", "Akima1DInterpolator")),
    # interpax owns the node-derivative Hermite; the scipy floor is the C2-cubic `make_interp_spline`
    (InterpKind.HERMITE, InterpRow(None, "CubicHermiteSpline", "make_interp_spline")),
    (InterpKind.BSPLINE, InterpRow(None, None, "make_interp_spline")),
])


# --- [OPERATIONS] --------------------------------------------------------------------------


# Decodes the live `QuadratureInfo.status` bitfield off the gated `quadax.STATUS`: `code == 0` short-circuits to
# `"successful"`, `np.bitwise_or.reduce` unions a VECTORIZED per-component status array, then the severity-ordered `_QUAD_STATUS` walk resolves the co-set flag.
def _quad_status(quadax: object, status: int | np.ndarray) -> str:
    if (code := int(np.bitwise_or.reduce(np.asarray(status).ravel().astype(np.int64), initial=0))) == 0:
        return "successful"
    message = quadax.STATUS.get(code, "").lower()
    return next((member for token, member in _QUAD_STATUS if token in message), "other")


# the one measured kernel returning the `SolverReceipt` — module-level and import-resolvable, so it crosses the process lane as spec data plus operands.
def _dispatch(intent: QuadratureIntent) -> SolverReceipt:
    match intent:
        case QuadratureIntent(tag="integrate", integrate=(fn, span, kind, policy)):
            return _integrate_receipt(fn, span, kind, policy)
        case QuadratureIntent(tag="interpolate", interpolate=(points, values, query, kind, policy, dydx)):
            return _interpolate_receipt(points, values, query, kind, policy, dydx)
        case QuadratureIntent(tag="fem", fem=(system, dirichlet, scheme, policy)):
            return _fem_receipt(system, dirichlet, scheme, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _integrate_receipt(fn: object, span: tuple[float, float], kind: QuadKind, policy: QuadPolicy) -> SolverReceipt:
    lo, hi = span
    # the lowering bridge: a jit-minted `LoweredSpec` integrand compiles through its own route row before the
    # driver runs (symbolic->jit->quadrature, zero symbolic imports); a compile fault degrades to the host kernel.
    if isinstance(fn, LoweredSpec):
        fn = fn.compiled().map(lambda jitted: jitted.fn).default_value(fn.kernel)
    # Spline-integrand exact path: a fitted spline (recognized by the `.integrate` method) owns the exact definite
    # integral, so `.integrate(lo, hi)` replaces the quadrature call (CUMULATIVE reads `.antiderivative()`); no error estimate, so the verdict is the finiteness floor.
    if callable(fn) and hasattr(fn, "integrate"):
        out = (
            np.asarray(fn.antiderivative()(np.linspace(lo, hi, policy.floor_nodes)))
            if policy.readout is Readout.CUMULATIVE
            else np.asarray(fn.integrate(lo, hi))
        )
        residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
        return SolverReceipt.Iterative(residual, 0, policy.epsrel, result=None)
    row = _QUAD[kind]
    try:
        engine = QuadEngine.gated()  # floats the rail to float64 for the differentiable integral
        # a sampled rule returns a bare array with no QuadratureInfo; the honest residual is the finiteness floor, never the integral magnitude.
        if row.sampled:
            samples = np.asarray(fn)
            grid = np.linspace(lo, hi, samples.size)
            out = engine.sampled(samples, grid, policy.readout)
            residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
            return SolverReceipt.Iterative(residual, int(samples.size), policy.epsrel, result=None)
        # a VECTORIZED row carries per-component `err`/`neval`, so the receipt folds the WORST component (the scipy quad_vec `np.max(abserr)` discipline), scalar rows reducing to themselves.
        _, info = engine.integrate(row, fn, lo, hi, policy)
        err, neval = float(np.max(np.asarray(info.err))), int(np.max(np.asarray(info.neval)))
        return SolverReceipt.Iterative(err, neval, policy.epsrel, result=_quad_status(engine.quadax, info.status))
    except ImportError:
        return _integrate_scipy(fn, lo, hi, row, policy)


# The scipy host floor: the row's `scipy` field names the callable and one match folds the divergent call/return
# shapes into the one (err, neval) pair; result=None defers the verdict to the receipt residual floor.
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
        out = _prefix_trapezoid(samples, grid) if policy.readout is Readout.CUMULATIVE else np.trapezoid(samples, grid, axis=0)
        residual = float((hi - lo) / n) if np.all(np.isfinite(out)) else float("inf")
        return SolverReceipt.Iterative(residual, n, policy.epsrel, result=None)


def _interpolate_receipt(
    points: np.ndarray, values: np.ndarray, query: np.ndarray | None, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> SolverReceipt:
    xq = query if query is not None else 0.5 * (points[:-1] + points[1:])
    fitted = np.asarray(_evaluate_interpolant(points, values, xq, kind, policy, dydx))
    # VALUE residual measures the interpolant against the np.interp linear baseline; a non-VALUE readout has no
    # shared baseline, so its verdict is the finiteness floor, never the readout magnitude smuggled into the residual.
    residual = (
        float(np.linalg.norm(fitted - np.interp(xq, points, values)))
        if policy.readout is Readout.VALUE
        else (0.0 if np.all(np.isfinite(fitted)) else float("inf"))
    )
    return SolverReceipt.LeastSquares(residual, int(points.size), 0)


# The interpax floor reads the `_INTERP` row through `QuadEngine.interpolant`; a method-only ANTIDERIVATIVE/CUMULATIVE folds the running integral (no analytic `.antiderivative()`).
def _evaluate_interpolant(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> np.ndarray:
    row = _INTERP[kind]
    try:
        engine = QuadEngine.gated()  # floats the rail to float64 for the differentiable interpolant
        # `QuadEngine.interpolant` is the SINGLE interpax fold; a `None` return (BSPLINE) routes the scipy body.
        out = engine.interpolant(row, points, values, xq, kind, policy, dydx)
        return out if out is not None else _interpolate_scipy(points, values, xq, kind, policy, dydx)
    except ImportError:
        return _interpolate_scipy(points, values, xq, kind, policy, dydx)


# The numpy-floor running antiderivative: a vectorized prefix trapezoid with a leading zero row — numpy exposes no
# `cumulative_trapezoid`, so the no-scipy floor owns this fold locally.
def _prefix_trapezoid(y: np.ndarray, x: np.ndarray) -> np.ndarray:
    dx = np.diff(np.asarray(x, dtype=np.float64))
    widths = dx.reshape(-1, *([1] * (y.ndim - 1))) if y.ndim > 1 else dx
    steps = 0.5 * (y[1:] + y[:-1]) * widths
    return np.concatenate([np.zeros_like(y[:1]), np.cumsum(steps, axis=0)], axis=0)


# A one-shot/linear interpolant owns no analytic antiderivative; its running sample integral is the honest ANTIDERIVATIVE/CUMULATIVE, VALUE/DERIVATIVE passing through.
def _cumulative_readout(base: np.ndarray, xq: np.ndarray, readout: Readout) -> np.ndarray:
    if readout is Readout.ANTIDERIVATIVE or readout is Readout.CUMULATIVE:
        return _prefix_trapezoid(base, xq)
    return base


def _interpolate_scipy(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> np.ndarray:
    row = _INTERP[kind]
    if row.interpax_class is None and row.scipy_class == "make_interp_spline" and kind is not InterpKind.BSPLINE:
        # the method-only kinds have no dedicated scipy class: np.interp serves VALUE, np.gradient the DERIVATIVE,
        # the running cumulative-trapezoid the ANTIDERIVATIVE/CUMULATIVE — the Readout row never dropped.
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


# One constructor fold over the row back-end shape, NOT `kind` alone: interpax `CubicHermiteSpline` takes a third
# `dydx` array; scipy has no node-derivative Hermite, so HERMITE/BSPLINE degrades to degree-`k` `make_interp_spline` — passing `dydx` there would bind the `k` slot, a silent miscall.
def _construct(
    ctor: Callable[..., object],
    points: np.ndarray,
    values: np.ndarray,
    kind: InterpKind,
    policy: QuadPolicy,
    dydx: np.ndarray | None,
    *,
    node_derivatives: bool,
) -> object:
    if kind is InterpKind.HERMITE and node_derivatives:
        return ctor(points, values, dydx if dydx is not None else np.gradient(values, points))
    if kind is InterpKind.BSPLINE or kind is InterpKind.HERMITE:  # scipy degree-`k` `make_interp_spline` floor
        return ctor(points, values, k=policy.bspline_k)
    return ctor(points, values)


# Read a spline under the output axis: VALUE evaluates, DERIVATIVE the nu-th derivative, ANTIDERIVATIVE and CUMULATIVE the analytic `.antiderivative()`.
def _read_spline(spline: object, xq: np.ndarray, policy: QuadPolicy) -> np.ndarray:
    match policy.readout:
        case Readout.VALUE:
            return np.asarray(spline(xq))
        case Readout.DERIVATIVE:
            return np.asarray(spline.derivative(policy.nu)(xq))
        case Readout.ANTIDERIVATIVE | Readout.CUMULATIVE:
            return np.asarray(spline.antiderivative()(xq))
        case _ as unreachable:
            assert_never(unreachable)


# The FEM solve owns only the condense->solve half. `skfem.condense(..., expand=False)` returns the reduced
# `(cond_a, cond_b, *_restore)` bundle, then the caller's scheme solves it through `sparse_receipt` — honest `‖cond_a @ x - cond_b‖`, MatrixStructure.SYMMETRIC exposing SPD.
def _fem_receipt(system: AssembledSystem, dirichlet: float, scheme: SparseScheme, policy: LinearPolicy) -> SolverReceipt:
    import skfem

    seed = np.zeros(system.dof_count) + dirichlet
    cond_a, cond_b, *_restore = skfem.condense(system.stiffness, system.load, x=seed, D=system.dirichlet_dofs, expand=False)
    return sparse_receipt(LinearMap.SparseMat(cond_a, MatrixStructure.SYMMETRIC), np.asarray(cond_b), scheme, policy)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
