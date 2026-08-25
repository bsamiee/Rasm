# [PY_COMPUTE_QUADRATURE]

`QuadratureIntent` is the one numeric solver over three routes — 1-D quadrature, 1-D interpolation, and the weak-form finite-element `condense -> solve` fold — every route folding into the one `Solve`. Variation rides bounded policy values and one catalog row per concept: `QuadKind` keys the `_QUAD` integration-family catalog, `InterpKind` keys the `_INTERP` interpolant catalog, the shared `Readout` axis parameterizes output shape across both numeric routes, and one `QuadPolicy` struct carries every per-call knob. This FEM route owns only the `condense -> solve` half of an already-assembled system and never re-runs a `Basis`/`asm`.

Reused `Readout` axis spans both numeric routes; the FEM route consumes the `AssembledSystem` `solvers/mesh#EXCHANGE` lowers, condenses it through `skfem.condense`, and solves the condensed system through the `solvers/linear#LINEAR` public `sparse_solve` under the caller's `SparseScheme`/`LinearPolicy` against the honest condensed-load residual; the element axis `ElementKind`/`FemForm`/`CTOR` stays `solvers/mesh#MESH_FIELD`-owned and never crosses — the `fem` case carries the lowered system alone. The lowering bridge is the folder's ONE seam where a compiled kernel meets a `Solve`, so it threads the compile's `EngineProfile` off `Jitted.evidence.profile` onto every integrate arm — the mount `solvers/solve#SOLVE` reserves, filled here rather than left absent on every solve. Each numeric floor climbs the JAX-native `quadax`/`interpax` companion — woven once through the frozen `QuadEngine`, floating the rail to float64 because the `epsabs=1e-10`/`epsrel=1e-8` tolerances sit below float32 eps — then the host `scipy` body, then the unconditional `numpy` floor. Module-level `_dispatch` crosses the process lane as spec data and operands, `_TRAIT` declares the gated `integrate`/`interpolate` routes `HOSTILE` and the scipy-bound FEM `RELEASING`, and every `Solve` stamps its `attributes` on the hub `evidence_run` span — span and fence the weave's — over the `solvers/solve#SOLVE` `status_of` residual floor every scipy/numpy path defers to with `result=None`. `graduates` is the sibling-shaped solver-axis crossing over the `_CEILING` family row, so the weak-form result reaches the hub on the one projection its linear, nonlinear, and differential peers cross on.

## [01]-[INDEX]

- [02]-[QUADRATURE]: 1-D quadrature, 1-D interpolation, and the weak-form FEM condense fold on one `QuadratureIntent` owner over a three-floor quadax/scipy/numpy ladder into one `Solve`.

## [02]-[QUADRATURE]

- Owner: `QuadratureIntent` carries the integrate/interpolate/fem cases on one solver; the `fem` case carries the assembled stiffness/load/dof system itself, never the `MeshField`, so this route condenses and solves and never reaches into mesh assembly.
- Law: `QuadratureIntent.graduates` is the sibling-shaped solver-axis crossing over the `_CEILING` family row — the weak-form `condense -> solve` result is the terminal evidence of the whole mesh-assemble-solve chain, so withholding it would leave that chain unable to reach the hub while its linear, nonlinear, and differential peers cross on the same axis with the same projection. Ceiling default is governed policy beside the trait rows and a caller's tighter row overrides at the projection.
- Entry: `QuadratureIntent.solve(lane)` is the one union method matching `LinearIntent.solve`/`DifferentialIntent.solve`, composing the `_TRAIT`-routed `lane.offload` under the hub `evidence_run` weave — `HOSTILE` for the gated quadax/interpax routes (x64 is process-global native state), `RELEASING` for the scipy FEM — isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing owner from the trait row; the caller's composition `ScopeKey` threads onto the weave and the crossing alike.
- Output: the shared `Readout` axis carries output shape for both routes — scalar integral, running antiderivative, `nu`-th derivative, analytic antiderivative — never a `cumulative`/`derivative` boolean knob or parallel `IntegrateOutput`/`InterpOutput` enums.
- Output: each route retains its integral, interpolation readout, or FEM solution array in `Solve`; adaptive `QuadratureInfo` contributes estimated error, evaluation count, and typed status. A vectorized error/status reduces to the worst scalar, while scipy/numpy floors use the shared residual adjudication.
- Packages: `quadax`/`interpax` the JAX-native differentiable adaptive/fixed/sampled and interpolant floor, `scipy` the host bodies (never the deprecated `interp1d`; no scipy node-derivative Hermite drop-in, so the `HERMITE` scipy floor is the degree-`k` `make_interp_spline` C2 cubic), `skfem` the `condense`/`solve` half only (`Basis`/`asm` stays on `solvers/mesh#EXCHANGE`), `jax` the x64 float64 promotion, `numpy` the unconditional floor owning `_prefix_trapezoid` locally (numpy exposes no `cumulative_trapezoid`; that spelling is SciPy-owned), otherwise per the fence imports.
- Growth: a new quadrature rule is one `QuadKind` member and one `_QUAD` row folded through `QuadEngine.integrate`; a new interpolant family one `InterpKind` member and one `_INTERP` row through `QuadEngine.interpolant`; a new output shape one `Readout` member; a new integrator knob one `QuadPolicy` field; a new termination code one severity-ranked `_QUAD_STATUS` token; a new admission bar one `_CEILING` row; a new element one mesh-owned `CTOR` row, zero surface here; a new FEM sparse scheme zero new surface, since the caller passes any `SparseScheme`/`LinearPolicy` the linear route owns.
- Boundary: the FEM element axis (`ElementKind`/`FemForm`/`CTOR`) is mesh-owned on `solvers/mesh#MESH_FIELD` — this route consumes only the `AssembledSystem` lowering, so no element vocabulary crosses and no `TYPE_CHECKING` cycle-dodge exists; 2-D/3-D interpolation lives on `solvers/field` (the `interpax` `interp2d`/`interp3d` family) and multidimensional ODE integration on `solvers/differential#DIFFERENTIAL`.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, Graduation, evidence_run
from rasm.compute.numerics.jit import EngineProfile, LoweredSpec
from rasm.compute.solvers.linear import LinearMap, LinearPolicy, MatrixStructure, SparseScheme, sparse_solve
from rasm.compute.solvers.mesh import AssembledSystem
from rasm.compute.solvers.solve import Provider, Solve, graduate
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import scipy.integrate as integ
lazy import scipy.interpolate as interp
lazy import skfem

# --- [TYPES] ----------------------------------------------------------------------------


class QuadKind(StrEnum):
    GAUSS_KRONROD = "gauss_kronrod"
    CLENSHAW_CURTIS = "clenshaw_curtis"
    ROMBERG = "romberg"
    ROMBERG_TS = "romberg_ts"
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
    HERMITE = "hermite"
    BSPLINE = "bspline"


class Readout(StrEnum):
    VALUE = "value"
    DERIVATIVE = "derivative"
    ANTIDERIVATIVE = "antiderivative"
    CUMULATIVE = "cumulative"


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUAD_STATUS: tuple[tuple[str, str], ...] = (
    ("nan", "nonfinite"),
    ("inf", "nonfinite"),
    ("singular", "singular"),
    ("diverg", "nonlinear_divergence"),
    ("converge", "nonlinear_divergence"),
    ("bad", "nonlinear_divergence"),
    ("subdivision", "max_steps_reached"),
    ("max", "max_steps_reached"),
    ("ninter", "max_steps_reached"),
    ("round", "stagnation"),
)


_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("integrate", KernelTrait.HOSTILE),
    ("interpolate", KernelTrait.HOSTILE),
    ("fem", KernelTrait.RELEASING),
])

_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-6)])


# --- [MODELS] ---------------------------------------------------------------------------


class QuadPolicy(Struct, frozen=True):
    epsabs: float = 1e-10
    epsrel: float = 1e-8
    order: int = 21
    max_ninter: int = 50
    divmax: int = 20
    fixed_nodes: int = 21
    floor_nodes: int = 1024
    adaptive: bool = True
    nu: int = 1
    extrapolate: bool = True
    bspline_k: int = 3
    readout: Readout = Readout.VALUE


class QuadRow(Struct, frozen=True):
    adaptive: str
    fixed: str | None
    extrapolated: bool
    sampled: bool
    scipy: str


class InterpRow(Struct, frozen=True):
    interpax_method: str | None
    interpax_class: str | None
    scipy_class: str


@dataclass(frozen=True, slots=True)
class QuadEngine:
    quadax: object
    interpax: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)

        import interpax
        import quadax
        import quadax.sampled

        return cls(quadax=quadax, interpax=interpax)

    def integrate(self, row: QuadRow, fn: object, lo: float, hi: float, policy: "QuadPolicy") -> tuple[object, object]:
        qx, interval = self.quadax, np.asarray([lo, hi])
        if not policy.adaptive and row.fixed is not None:
            return getattr(qx, row.fixed)(fn, lo, hi, n=policy.fixed_nodes)
        if row.extrapolated:
            return getattr(qx, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, divmax=policy.divmax)
        return getattr(qx, row.adaptive)(fn, interval, epsabs=policy.epsabs, epsrel=policy.epsrel, max_ninter=policy.max_ninter, order=policy.order)

    def sampled(self, samples: np.ndarray, grid: np.ndarray, readout: "Readout") -> np.ndarray:
        fold = self.quadax.sampled.cumulative_simpson if readout is Readout.CUMULATIVE else self.quadax.sampled.simpson
        return np.asarray(fold(samples, x=grid))

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
        dydx: np.ndarray | None = None,
    ) -> "QuadratureIntent":
        return QuadratureIntent(interpolate=(points, values, query, kind, policy, dydx))

    @staticmethod
    def Fem(
        system: AssembledSystem, dirichlet: float = 0.0, scheme: SparseScheme = SparseScheme.Spsolve(), policy: LinearPolicy = LinearPolicy()
    ) -> "QuadratureIntent":
        return QuadratureIntent(fem=(system, dirichlet, scheme, policy))

    async def solve(self, lane: LanePolicy, key: ContentKey, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[Solve[np.ndarray]]":
        async def dispatch() -> "RuntimeRail[Solve[np.ndarray]]":
            return await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self, key)

        return await evidence_run(EvidenceScope.QUADRATURE, f"solve.{self.tag}", dispatch, facts={"route": self.tag}, composition=composition)

    def graduates(
        self, solve: "Solve[np.ndarray]", ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Graduation]":
        return graduate(
            EvidenceScope.QUADRATURE.value, f"solve.{self.tag}", solve.content_key, solve, ceiling or dict(_CEILING.items()),
            composition=composition,
        )


# --- [TABLES] ---------------------------------------------------------------------------

_QUAD: Final[Map[QuadKind, QuadRow]] = Map.of_seq([
    (QuadKind.GAUSS_KRONROD, QuadRow("quadgk", "fixed_quadgk", False, False, "quad")),
    (QuadKind.CLENSHAW_CURTIS, QuadRow("quadcc", "fixed_quadcc", False, False, "quad")),
    (QuadKind.ROMBERG, QuadRow("romberg", None, True, False, "quad")),
    (QuadKind.ROMBERG_TS, QuadRow("rombergts", None, True, False, "tanhsinh")),
    (QuadKind.TANH_SINH, QuadRow("quadts", "fixed_quadts", False, False, "tanhsinh")),
    (QuadKind.VECTORIZED, QuadRow("quadgk", "fixed_quadgk", False, False, "quad_vec")),
    (QuadKind.SAMPLED_SIMPSON, QuadRow("simpson", None, False, True, "simpson")),
])

_INTERP: Final[Map[InterpKind, InterpRow]] = Map.of_seq([
    (InterpKind.LINEAR, InterpRow("linear", None, "make_interp_spline")),
    (InterpKind.CUBIC2, InterpRow("cubic2", None, "make_interp_spline")),
    (InterpKind.CATMULL_ROM, InterpRow("catmull-rom", None, "make_interp_spline")),
    (InterpKind.CUBIC, InterpRow(None, "CubicSpline", "CubicSpline")),
    (InterpKind.PCHIP, InterpRow(None, "PchipInterpolator", "PchipInterpolator")),
    (InterpKind.AKIMA, InterpRow(None, "Akima1DInterpolator", "Akima1DInterpolator")),
    (InterpKind.HERMITE, InterpRow(None, "CubicHermiteSpline", "make_interp_spline")),
    (InterpKind.BSPLINE, InterpRow(None, None, "make_interp_spline")),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _quad_status(quadax: object, status: int | np.ndarray) -> str:
    if (code := int(np.bitwise_or.reduce(np.asarray(status).ravel().astype(np.int64), initial=0))) == 0:
        return "successful"
    message = quadax.STATUS.get(code, "").lower()
    return next((member for token, member in _QUAD_STATUS if token in message), "other")


def _dispatch(intent: QuadratureIntent, key: ContentKey) -> "Solve[np.ndarray]":
    match intent:
        case QuadratureIntent(tag="integrate", integrate=(fn, span, kind, policy)):
            return _integrate_solve(key, fn, span, kind, policy)
        case QuadratureIntent(tag="interpolate", interpolate=(points, values, query, kind, policy, dydx)):
            return _interpolate_readout(key, points, values, query, kind, policy, dydx)
        case QuadratureIntent(tag="fem", fem=(system, dirichlet, scheme, policy)):
            return _fem_solve(key, system, dirichlet, scheme, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _integrate_solve(key: ContentKey, fn: object, span: tuple[float, float], kind: QuadKind, policy: QuadPolicy) -> "Solve[np.ndarray]":
    lo, hi = span
    profile: EngineProfile | None = None
    if isinstance(fn, LoweredSpec):
        compiled = fn.compiled()
        profile = compiled.map(lambda jitted: jitted.evidence.profile).default_value(None)
        fn = compiled.map(lambda jitted: jitted.fn).default_value(fn.kernel)
    if callable(fn) and hasattr(fn, "integrate"):
        out = (
            np.asarray(fn.antiderivative()(np.linspace(lo, hi, policy.floor_nodes)))
            if policy.readout is Readout.CUMULATIVE
            else np.asarray(fn.integrate(lo, hi))
        )
        residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
        return Solve.Iterative(out, key, residual, 0, Provider.GATED, policy.epsrel, result=None, profile=profile)
    row = _QUAD[kind]
    try:
        engine = QuadEngine.gated()
        if row.sampled:
            samples = np.asarray(fn)
            grid = np.linspace(lo, hi, samples.size)
            out = engine.sampled(samples, grid, policy.readout)
            residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
            return Solve.Iterative(out, key, residual, int(samples.size), Provider.GATED, policy.epsrel, result=None, profile=profile)
        out, info = engine.integrate(row, fn, lo, hi, policy)
        err, neval = float(np.max(np.asarray(info.err))), int(np.max(np.asarray(info.neval)))
        return Solve.Iterative(
            np.asarray(out), key, err, neval, Provider.GATED, policy.epsrel,
            result=_quad_status(engine.quadax, info.status), profile=profile,
        )
    except ImportError:
        return _integrate_scipy(key, fn, lo, hi, row, policy, profile)


def _integrate_scipy(
    key: ContentKey, fn: object, lo: float, hi: float, row: QuadRow, policy: QuadPolicy, profile: EngineProfile | None = None
) -> "Solve[np.ndarray]":
    try:
        match row.scipy:
            case "simpson":
                samples = np.asarray(fn)
                grid = np.linspace(lo, hi, samples.size)
                fold = integ.cumulative_simpson if policy.readout is Readout.CUMULATIVE else integ.simpson
                out = np.asarray(fold(samples, x=grid))
                residual = 0.0 if np.all(np.isfinite(out)) else float("inf")
                return Solve.Iterative(out, key, residual, int(samples.size), Provider.FLOOR, policy.epsrel, result=None, profile=profile)
            case "quad_vec":
                out, abserr = integ.quad_vec(fn, lo, hi, epsabs=policy.epsabs, epsrel=policy.epsrel)[:2]
                return Solve.Iterative(
                    np.asarray(out), key, float(np.max(abserr)), 0, Provider.FLOOR, policy.epsrel, result=None, profile=profile
                )
            case "tanhsinh":
                res = integ.tanhsinh(fn, lo, hi)
                return Solve.Iterative(
                    np.asarray(res.integral), key, float(res.error), int(res.nfev), Provider.FLOOR, policy.epsrel, result=None,
                    profile=profile,
                )
            case _:
                out, abserr, info = integ.quad(fn, lo, hi, epsabs=policy.epsabs, epsrel=policy.epsrel, full_output=True)[:3]
                return Solve.Iterative(
                    np.asarray(out), key, float(abserr), int(info.get("neval", 0)), Provider.FLOOR, policy.epsrel, result=None,
                    profile=profile,
                )
    except ImportError:
        n = policy.floor_nodes
        grid = np.linspace(lo, hi, n)
        samples = np.asarray([fn(float(t)) for t in grid]) if callable(fn) else np.asarray(fn)
        out = _prefix_trapezoid(samples, grid) if policy.readout is Readout.CUMULATIVE else np.trapezoid(samples, grid, axis=0)
        residual = float((hi - lo) / n) if np.all(np.isfinite(out)) else float("inf")
        return Solve.Iterative(np.asarray(out), key, residual, n, Provider.FLOOR, policy.epsrel, result=None, profile=profile)


def _interpolate_readout(
    key: ContentKey, points: np.ndarray, values: np.ndarray, query: np.ndarray | None, kind: InterpKind, policy: QuadPolicy,
    dydx: np.ndarray | None,
) -> "Solve[np.ndarray]":
    xq = query if query is not None else 0.5 * (points[:-1] + points[1:])
    provider, fitted = _evaluate_interpolant(points, values, xq, kind, policy, dydx)
    residual = (
        float(np.linalg.norm(fitted - np.interp(xq, points, values)))
        if policy.readout is Readout.VALUE
        else (0.0 if np.all(np.isfinite(fitted)) else float("inf"))
    )
    return Solve.LeastSquares(fitted, key, residual, int(points.size), 0, provider)


def _evaluate_interpolant(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> tuple[Provider, np.ndarray]:
    row = _INTERP[kind]
    try:
        engine = QuadEngine.gated()
        out = engine.interpolant(row, points, values, xq, kind, policy, dydx)
        if out is not None:
            return (Provider.GATED, np.asarray(out))
        return (Provider.FLOOR, np.asarray(_interpolate_scipy(points, values, xq, kind, policy, dydx)))
    except ImportError:
        return (Provider.FLOOR, np.asarray(_interpolate_scipy(points, values, xq, kind, policy, dydx)))


def _prefix_trapezoid(y: np.ndarray, x: np.ndarray) -> np.ndarray:
    dx = np.diff(np.asarray(x, dtype=np.float64))
    widths = dx.reshape(-1, *([1] * (y.ndim - 1))) if y.ndim > 1 else dx
    steps = 0.5 * (y[1:] + y[:-1]) * widths
    return np.concatenate([np.zeros_like(y[:1]), np.cumsum(steps, axis=0)], axis=0)


def _cumulative_readout(base: np.ndarray, xq: np.ndarray, readout: Readout) -> np.ndarray:
    if readout is Readout.ANTIDERIVATIVE or readout is Readout.CUMULATIVE:
        return _prefix_trapezoid(base, xq)
    return base


def _interpolate_scipy(
    points: np.ndarray, values: np.ndarray, xq: np.ndarray, kind: InterpKind, policy: QuadPolicy, dydx: np.ndarray | None
) -> np.ndarray:
    row = _INTERP[kind]
    if row.interpax_class is None and row.scipy_class == "make_interp_spline" and kind is not InterpKind.BSPLINE:
        base = np.asarray(np.interp(xq, points, values))
        if policy.readout is Readout.DERIVATIVE:
            return np.asarray(np.gradient(base, xq))
        return _cumulative_readout(base, xq, policy.readout)
    try:
        ctor = getattr(interp, row.scipy_class)
        spline = _construct(ctor, points, values, kind, policy, dydx, node_derivatives=False)
        return _read_spline(spline, xq, policy)
    except ImportError:
        return np.interp(xq, points, values)


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
    if kind is InterpKind.BSPLINE or kind is InterpKind.HERMITE:
        return ctor(points, values, k=policy.bspline_k)
    return ctor(points, values)


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


def _fem_solve(key: ContentKey, system: AssembledSystem, dirichlet: float, scheme: SparseScheme, policy: LinearPolicy) -> "Solve[np.ndarray]":
    seed = np.zeros(system.dof_count) + dirichlet
    cond_a, cond_b, *_restore = skfem.condense(system.stiffness, system.load, x=seed, D=system.dirichlet_dofs, expand=False)
    return sparse_solve(key, LinearMap.SparseMat(cond_a, MatrixStructure.SYMMETRIC), np.asarray(cond_b), scheme, policy)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
