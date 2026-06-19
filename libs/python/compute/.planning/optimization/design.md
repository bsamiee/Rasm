# [PY_COMPUTE_DESIGN]

The gradient-driven inverse-design apex the autodifferentiable solver chain enables and no other owner closes. `DesignProblem` discriminates a field problem over the `solvers/mesh.md#MESH_FIELD` assembled system, a parametric-mesh objective, and a material-distribution density objective, driving an Equinox-partitioned PyTree objective to a stationary point through `optimistix.minimise`/`least_squares` over the JAX floor. The descent reads the implicit-function-theorem adjoint through the converged solve — the default `ImplicitAdjoint` carried by every Optimistix entry — so the gradient `solvers/sensitivity.md#SENSITIVITY` pulls back is the gradient of the converged solution, never the iteration trace. The three cases share one table-driven Optimistix dispatch keyed on the tag, the `optax` first-order-descent axis threads through `optimistix.OptaxMinimiser` as a row on that table, and every route folds one content-keyed `OptimizationReceipt` over the final iterate, the objective trace, and the first-order/KKT residual that graduates outward on the existing `solver` axis at `graduation/handoff.md#GRADUATION`. A numpy central-difference floor reports the gradient-norm residual at `x0` for every case, reachable on cp315, so a run without the jaxlib wheel never returns `Error(Import)`. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

## [01]-[INDEX]

- [01]-[DESIGN]: field/mesh/density inverse-design over an Equinox-parameterized objective through Optimistix `minimise`/`least_squares` with the implicit-adjoint gradient, folding one `OptimizationReceipt` on one `DesignProblem` owner.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the ONE inverse-design owner; `DesignProblem` discriminates `Field(objective, params)` (an objective over a `solvers/mesh` `AssembledSystem` stiffness/load discretization), `Mesh(objective, params)` (a parametric-mesh objective over node coordinates), and `Density(objective, params)` (a material-distribution density objective over a SIMP-style design field), each carrying the objective thunk and the initial parameter PyTree. The provenance of the objective is the discriminant; the optimizer is one surface. The cases never become a per-problem optimizer family — `Field`, `Mesh`, and `Density` are rows on the same owner discriminated by the structure the objective integrates over.
- Parameterization: the objective is an Equinox PyTree partitioned through `equinox.partition`/`combine` into the inexact-array design leaves the optimizer threads and the static rest; `optimistix.minimise` carries the combined PyTree through iteration and the implicit adjoint differentiates the converged stationary point. A scalar objective routes `minimise`; a residual-vector inverse-identification objective routes `least_squares`; both read `optimistix.Solution.value` as the converged design and `Solution.stats["num_steps"]` as the iteration count. The `optax` first-order-descent axis is one row on the dispatch table: `OptaxMinimiser(optax.adam(lr), rtol, atol)` wraps an `optax` alias optimizer or a `chain` of transformations as the Optimistix solver, never a hand-rolled momentum loop and never a parallel descent surface.
- Adjoint: every Optimistix entry carries the default `ImplicitAdjoint`, so a sensitivity through a design that itself solves an inner system differentiates the converged solution through one linear solve per backward pass rather than backpropagating the iterations; `solvers/sensitivity.md#SENSITIVITY` reads that adjoint through `jax.vjp` over the objective, and `solvers/linear.md#LINEAR`/`solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` expose the autodifferentiable inner solves the `Field` objective composes (the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here).
- Entry: `DesignProblem.solve` enters one `boundary(f"design.{problem.tag}", ...)` returning `RuntimeRail[OptimizationReceipt]`; the dispatch matches the tag total over `match`/`assert_never` (a new case breaks every site), tries the Optimistix route table keyed on the tag behind the gated import, and falls to the numpy central-difference floor on `ImportError`. Each design keys through `ContentIdentity.of` over the canonical objective-and-parameter buffer, so a design re-run from the same objective and starting point keys identically across backends.
- Receipt: `OptimizationReceipt.contribute` emits one `Receipt.of("emitted", ...)` row carrying the problem tag, the converged objective value, the first-order/KKT residual, the iteration count, and the content key; the residual is the convergence evidence the C# graduation gate reads through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` to admit or reject a converged design for kernel handoff. No new handoff axis and no graduation edit — the converged design crosses on the `solver` axis already present.
- Packages: `optimistix` (`minimise`, `least_squares`, `OptaxMinimiser`, `BFGS`, `LevenbergMarquardt`, `ImplicitAdjoint`, `Solution`), `equinox` (`partition`, `combine`, `is_inexact_array`), `jax` (the autodiff floor the objective and the adjoint resolve over), `optax` (`adam`, `sgd`, `chain`, the first-order-descent axis threaded through `OptaxMinimiser`), `numpy` (`asarray`, `eye`, `linalg.norm` — the iterate buffer and the central-difference residual floor), `solvers/sensitivity.md#SENSITIVITY` (the implicit-adjoint gradient read through the converged solve), `solvers/{linear,nonlinear,differential}.md` (the autodifferentiable inner solves the `Field` objective composes), `solvers/mesh.md#MESH_FIELD` (`AssembledSystem` stiffness/load for the field objective), `graduation/handoff.md#GRADUATION` (the `solver` axis the converged design graduates on), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new design provenance is one `DesignProblem` case plus one row in the `_optimistix_design` route table; a new descent engine is one cell on the row (an `optax` alias or `chain` under `OptaxMinimiser`, or an Optimistix `BFGS`/`LevenbergMarquardt` solver); zero new surface, never a parallel field-design and density-design owner, never a per-case helper body, never a training loop.
- Boundary: offline inverse design only — PDE-constrained optimal design, parametric-mesh shape optimization, and material-distribution density optimization driven to a stationary point are in-scope; `jax`/`jaxlib`/`optimistix`/`equinox`/`optax` carry no cp315 wheel, so the Optimistix body is authored against the documented API behind one gated import, and the numpy central-difference floor runs unconditionally for every case so a cp315 run never returns `Error(Import)`. A separate optimizer owner per design provenance, a per-case `_*_receipt` helper body, a gradient-descent training loop, a production solver session, and a parallel optimizer surface beside the converged solve are the deleted forms; the inner solve stays on the solver owners and the assembly on `solvers/mesh`, so this owner composes the converged design rather than re-deriving it.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


@tagged_union(frozen=True)
class DesignProblem:
    tag: Literal["field", "mesh", "density"] = tag()
    field: tuple[object, object] = case()
    mesh: tuple[object, object] = case()
    density: tuple[object, object] = case()

    @staticmethod
    def Field(objective: Callable[[object], object], params: object) -> "DesignProblem":
        return DesignProblem(field=(objective, params))

    @staticmethod
    def Mesh(objective: Callable[[object], object], params: object) -> "DesignProblem":
        return DesignProblem(mesh=(objective, params))

    @staticmethod
    def Density(objective: Callable[[object], object], params: object) -> "DesignProblem":
        return DesignProblem(density=(objective, params))


class OptimizationReceipt(Struct, frozen=True):
    problem: str
    objective: float
    residual: float
    iterations: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        facts = {
            "problem": self.problem,
            "objective": f"{self.objective:.6e}",
            "residual": f"{self.residual:.3e}",
            "iterations": str(self.iterations),
            "key": str(self.content_key.value),
        }
        return Receipt.of("emitted", "compute.optimization.design", self.problem, facts)


_TOL = 1e-8
_LR = 1e-2


def solve(problem: DesignProblem, /, *, learning_rate: float = _LR) -> "RuntimeRail[OptimizationReceipt]":
    return boundary(f"design.{problem.tag}", lambda: _dispatch(problem, learning_rate))


def _dispatch(problem: DesignProblem, learning_rate: float) -> OptimizationReceipt:
    match problem:
        case DesignProblem(tag="field", field=(objective, params)):
            pass
        case DesignProblem(tag="mesh", mesh=(objective, params)):
            pass
        case DesignProblem(tag="density", density=(objective, params)):
            pass
        case unreachable:
            assert_never(unreachable)
    key = _design_key(problem.tag, params)
    try:
        return _optimistix_design(problem.tag, objective, params, learning_rate, key)
    except ImportError:
        return _floor_design(problem.tag, objective, params, key)


def _optimistix_design(tag: str, objective: Callable[..., object], params: object, learning_rate: float, key: ContentKey) -> OptimizationReceipt:
    import equinox as eqx
    import optimistix as optx
    import optax

    design, static = eqx.partition(params, eqx.is_inexact_array)

    def fn(y: object, _: object) -> object:
        return objective(eqx.combine(y, static))

    op, solver = {
        "field": (optx.minimise, optx.BFGS(rtol=_TOL, atol=_TOL)),
        "mesh": (optx.least_squares, optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL)),
        "density": (optx.minimise, optx.OptaxMinimiser(optax.adam(learning_rate), rtol=_TOL, atol=_TOL)),
    }[tag]
    solution = op(fn, solver, design, adjoint=optx.ImplicitAdjoint())
    converged = eqx.combine(solution.value, static)
    value = objective(converged)
    objective_scalar = float(np.asarray(value)) if tag != "mesh" else float(np.linalg.norm(np.asarray(value)))
    residual = _first_order_residual(objective, converged, tag)
    steps = int(solution.stats["num_steps"])
    return OptimizationReceipt(tag, objective_scalar, residual, steps, key)


def _floor_design(tag: str, objective: Callable[..., object], params: object, key: ContentKey) -> OptimizationReceipt:
    x0 = np.asarray(params, dtype=float)
    value = objective(x0)
    objective_scalar = float(np.asarray(value)) if tag != "mesh" else float(np.linalg.norm(np.asarray(value)))
    residual = _central_difference_norm(objective, x0, tag)
    return OptimizationReceipt(tag, objective_scalar, residual, 0, key)


def _first_order_residual(objective: Callable[..., object], converged: object, tag: str) -> float:
    import jax
    import jax.numpy as jnp

    cost = (lambda y: 0.5 * (jnp.asarray(objective(y)) ** 2).sum()) if tag == "mesh" else (lambda y: jnp.asarray(objective(y)).sum())
    gradient = jax.grad(cost)(converged)
    return float(jnp.linalg.norm(jnp.asarray(gradient).ravel(), jnp.inf))


def _central_difference_norm(objective: Callable[..., object], x0: np.ndarray, tag: str) -> float:
    cost = (lambda x: 0.5 * float(np.sum(np.asarray(objective(x)) ** 2))) if tag == "mesh" else (lambda x: float(np.asarray(objective(x))))
    basis = np.eye(x0.size)
    probe = [(cost(x0 + 1e-6 * e) - cost(x0 - 1e-6 * e)) / 2e-6 for e in basis]
    return float(np.linalg.norm(probe, np.inf))


def _design_key(tag: str, params: object) -> ContentKey:
    buffer = np.ascontiguousarray(np.asarray(params, dtype=float)).tobytes()
    return ContentIdentity.of(f"design-{tag}", buffer, IdentityPolicy())
```

## [03]-[RESEARCH]

- [OPTIMISTIX_DESIGN]: `optimistix` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `minimise`/`least_squares` entries, the `BFGS`/`LevenbergMarquardt` solvers, the `OptaxMinimiser` optax wrapper, the `ImplicitAdjoint` default adjoint, and `Solution.value`/`Solution.stats["num_steps"]` verify against `compute/.api/optimistix.md` under a uv-sync reflection pass on that band. Every Optimistix entry defaults to `ImplicitAdjoint`, so the descent differentiates the converged stationary point through one linear solve per backward pass consumed by `solvers/sensitivity.md#SENSITIVITY`, never the iteration trace; the numpy central-difference floor runs unconditionally on cp315 so every case stays reachable without the wheel.
- [EQUINOX_PARAM]: the `equinox.partition`/`combine` PyTree split over `equinox.is_inexact_array` separates the inexact-array design leaves the optimizer threads from the static rest, and the combined PyTree is the `y0` Optimistix carries through iteration; the spellings verify against `compute/.api/equinox.md` once the equinox wheel resolves on the gated band. The `optax.adam`/`sgd`/`chain` descent threaded through `optimistix.OptaxMinimiser` is the first-order-descent axis row, settled against `compute/.api/optax.md`, never a hand-rolled momentum accumulator.
- [JAX_ADJOINT]: the `jax.grad` first-order/KKT residual probe carries the `python_version<'3.15'` marker and verifies against `compute/.api/jax.md` once the jaxlib wheel resolves; the probe differentiates the scalar objective for the `field`/`density` minimise routes and the least-squares cost `½‖r‖²` for the `mesh` `least_squares` route, so the residual is the stationarity gradient `∇f` (respectively `Jᵀr`) that vanishes at the converged design the receipt folds and the `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads. The central-difference residual floor over an `np.eye` perturbation basis differentiates the same scalar cost and reproduces the same first-order quantity unconditionally on cp315.
