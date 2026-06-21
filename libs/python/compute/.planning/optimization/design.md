# [PY_COMPUTE_DESIGN]

The gradient-driven inverse-design apex the autodifferentiable solver chain enables and no other owner closes. `DesignProblem` discriminates a field problem over the `solvers/mesh.md#MESH_FIELD` assembled system, a parametric-mesh objective, and a material-distribution density objective, driving an Equinox-partitioned PyTree objective to a stationary point through `optimistix.minimise`/`least_squares` over the JAX floor. The descent reads the implicit-function-theorem adjoint through the converged solve — the default `ImplicitAdjoint` carried by every Optimistix entry — so the gradient `solvers/sensitivity.md#SENSITIVITY` pulls back is the gradient of the converged solution, never the iteration trace. The three cases share one Optimistix `minimise`/`least_squares` dispatch selected by the residual-vs-scalar objective shape, the descent engine is the `Descent` policy vocabulary (`quasi_newton`/`levenberg`/`first_order`) projected to its Optimistix/optax solver — defaulted per route, overridable per call, the `optax` first-order engine threading through `optimistix.OptaxMinimiser` — and every route folds one content-keyed `OutcomeReceipt` `design` case over the final iterate, the converged objective, and the first-order/KKT residual that graduates outward on the existing `solver` axis at `graduation/handoff.md#GRADUATION`. `OutcomeReceipt` is the ONE optimization-outcome owner — the `design` convergence verdict (objective, residual, iterations) and the `program` feasibility verdict (objective, `SolveStatus`, violation) are the two cases of one `@tagged_union`, folding `optimization/program.md#PROGRAM` onto the same receipt rather than a parallel struct; the `program` case carries the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns rather than a lone `bool success`, so the host `scipy.optimize` verdict speaks the one vocabulary every solve adjudicates and the success contract survives as the derived `status is SolveStatus.SUCCESS` predicate; `ConvexReceipt` stays distinct because it carries the duality-gap/dual-infeasibility/status certificate the first-order and feasibility verdicts have no field for. A numpy central-difference floor reports the gradient-norm residual at `x0` for every case, reachable on cp315, so a run without the jaxlib wheel never returns `Error(Import)`. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

## [01]-[INDEX]

- [01]-[DESIGN]: field/mesh/density inverse-design over an Equinox-parameterized objective through Optimistix `minimise`/`least_squares` with the implicit-adjoint gradient, folding the `design` case of the shared `OutcomeReceipt` on one `DesignProblem` owner.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the ONE inverse-design owner; `DesignProblem` discriminates `Field(objective, params)` (an objective over a `solvers/mesh` `AssembledSystem` stiffness/load discretization), `Mesh(objective, params)` (a parametric-mesh objective over node coordinates), and `Density(objective, params)` (a material-distribution density objective over a SIMP-style design field), each carrying the objective thunk and the initial parameter PyTree. The provenance of the objective is the discriminant; the optimizer is one surface. The cases never become a per-problem optimizer family — `Field`, `Mesh`, and `Density` are rows on the same owner discriminated by the structure the objective integrates over.
- Parameterization: the objective is an Equinox PyTree partitioned through `equinox.partition`/`combine` into the inexact-array design leaves the optimizer threads and the static rest; `optimistix.minimise` carries the combined PyTree through iteration and the implicit adjoint differentiates the converged stationary point. A scalar objective routes `minimise`; a residual-vector inverse-identification objective routes `least_squares`; both read `optimistix.Solution.value` as the converged design and `Solution.stats["num_steps"]` as the iteration count. The descent engine is not hardcoded per case: `Descent` is the bounded vocabulary — `quasi_newton` (`optimistix.BFGS`), `levenberg` (`optimistix.LevenbergMarquardt`), and `first_order` (`optimistix.OptaxMinimiser` over an `optax` alias or `chain`) — so the engine is a policy value carried on `DesignProblem.solve`, defaulting per route through the `_DEFAULT_DESCENT` table (`field`→`quasi_newton`, `mesh`→`levenberg`, `density`→`first_order`) and overridable to any admissible engine without a body edit. The `Descent.solver` projection builds the Optimistix solver from the policy, so a new descent engine is a new `Descent` case mapping to its Optimistix/optax constructor, never a hand-rolled momentum loop and never a parallel descent surface.
- Compilation: the partitioned objective `fn(y, args)` is wrapped once through `equinox.filter_jit` so the static rest skips tracing and the inexact-array design leaves are the only XLA-traced inputs — this is the same compile surface `solvers/nonlinear.md#NONLINEAR` threads through Optimistix, composed here, never re-derived. A `restarts>1` multi-start jitters `y0` across a leading restart axis through `jax.random.split`/`jax.random.normal` over the seeded PRNG key — each start is a distinct perturbed design, not a broadcast copy — and threads the partitioned solve through `equinox.filter_vmap` over that axis, returning the per-start converged values from which the route folds the best objective; the ensemble is one `filter_vmap` row on the same `solve` owner, never a parallel multi-start optimizer surface.
- Adjoint: every Optimistix entry carries the default `ImplicitAdjoint`, so a sensitivity through a design that itself solves an inner system differentiates the converged solution through one linear solve per backward pass rather than backpropagating the iterations; `solvers/sensitivity.md#SENSITIVITY` reads that adjoint through `jax.vjp` over the objective, and `solvers/linear.md#LINEAR`/`solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` expose the autodifferentiable inner solves the `Field` objective composes (the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here). The converged objective and the first-order/KKT residual fold from one `equinox.filter_value_and_grad` pass over the partitioned cost — value and gradient w.r.t. the inexact-array design leaves in a single trace, never a separate objective evaluation and a re-traced gradient — so the residual norm is computed on a JAX PyTree gradient through the package's own combined-grad transform and `optimistix.max_norm`, never by routing the converged PyTree through `numpy.asarray`.
- Entry: `DesignProblem.solve` enters one `boundary(f"design.{problem.tag}", ...)` returning `RuntimeRail[OutcomeReceipt]`; the `DesignProblem.carried` property extracts the `(objective, params)` payload total over `match`/`assert_never` (a new case breaks the extractor, not three parallel dispatch arms), `_dispatch` resolves the `Descent` policy (caller override or the `_DEFAULT_DESCENT` per-tag default), keys the design, and tries the Optimistix route behind the gated import, falling to the numpy central-difference floor on `ImportError`. The objective-vs-residual shape is the `_RESIDUAL_VECTOR` membership test, the route is the `Descent.solver` projection plus the `minimise`/`least_squares` entry the shape selects, and the converged objective and residual fold from one `filter_value_and_grad`. Each design keys through `ContentIdentity.of` over the canonical objective-and-parameter buffer, so a design re-run from the same objective and starting point keys identically across backends.
- Receipt: `OutcomeReceipt` is the ONE optimization-outcome receipt this owner and `optimization/program.md#PROGRAM` share — `OutcomeReceipt.Design` folds the design convergence verdict and `OutcomeReceipt.Program` folds the math-program feasibility verdict on the same `@tagged_union`, and `.contribute` matches the kind tag total, emitting the `compute.optimization.design` row carrying the problem tag, the converged objective, the first-order/KKT residual, the iteration count, and the content key, or the `compute.optimization.program` row carrying the program tag, objective, the `SolveStatus` termination verdict, the derived `converged` flag, and constraint violation. The `program` case carries `SolveStatus` (`solvers/receipt.md#RECEIPT`) in the slot a bare `bool success` formerly held, so the math-program host verdict and the solver routes share one termination vocabulary the C# graduation gate reads; the `Program` factory and the program-route body that maps the `scipy.optimize` `OptimizeResult` into that vocabulary live on `optimization/program.md#PROGRAM`. The residual is the convergence evidence the C# graduation gate reads through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` to admit or reject a converged design for kernel handoff. No new handoff axis and no graduation edit — the converged design crosses on the `solver` axis already present. `ConvexReceipt` (`optimization/convex.md#CONVEX`) stays a distinct struct: its duality-gap, dual-infeasibility, and solver-status certificate is the global-optimality proof object the first-order and feasibility verdicts carry no field for, so folding it onto `OutcomeReceipt` would erase the spectral/convergence certificate.
- Packages: `optimistix` (`minimise`, `least_squares`, `OptaxMinimiser`, `BFGS`, `LevenbergMarquardt`, `ImplicitAdjoint`, `Solution`, `max_norm` — the Chebyshev stationarity norm over the gradient PyTree), `equinox` (`partition`, `combine`, `is_inexact_array`, `filter_jit`, `filter_vmap`, `filter_value_and_grad` — the partition/compile/vectorize/combined-grad filter transforms threading the inexact-array design leaves, the value-and-grad fold the converged objective and residual share), `jax` (`numpy` the on-device reduction namespace; `random.key`/`random.split`/`random.normal` the seeded multi-start jitter; `tree_util.tree_map` the per-start slice; the autodiff floor the objective and the adjoint resolve over), `optax` (`adam`, `sgd`, `chain`, the first-order-descent engine threaded through `OptaxMinimiser`), `numpy` (`asarray`, `eye`, `linalg.norm` — the host-side central-difference residual floor over real arrays only, never over a JAX PyTree), `solvers/receipt.md#RECEIPT` (`SolveStatus` — the bounded termination vocabulary the shared `program` case carries so the math-program feasibility verdict and the solver routes speak one status enum), `solvers/sensitivity.md#SENSITIVITY` (the implicit-adjoint gradient read through the converged solve), `solvers/{linear,nonlinear,differential}.md` (the autodifferentiable inner solves the `Field` objective composes), `solvers/mesh.md#MESH_FIELD` (`AssembledSystem` stiffness/load for the field objective), `graduation/handoff.md#GRADUATION` (the `solver` axis the converged design graduates on), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new design provenance is one `DesignProblem` case plus one `_DEFAULT_DESCENT` row; a new descent engine is one `Descent` case mapping to its Optimistix/optax constructor in `Descent.solver` (a `chain` of `optax` transformations, an `optimistix.LBFGS`/`NonlinearCG`, or a trust-region least-squares solver), selectable per call without a body edit; a perturbed multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`; zero new surface, never a parallel field-design and density-design owner, never a per-case helper body, never a training loop.
- Boundary: offline inverse design only — PDE-constrained optimal design, parametric-mesh shape optimization, and material-distribution density optimization driven to a stationary point are in-scope; `jax`/`jaxlib`/`optimistix`/`equinox`/`optax` carry no cp315 wheel, so the Optimistix body is authored against the documented API behind one gated import, and the numpy central-difference floor runs unconditionally for every case so a cp315 run never returns `Error(Import)`. A separate optimizer owner per design provenance, a per-case `_*_receipt` helper body, a gradient-descent training loop, a production solver session, and a parallel optimizer surface beside the converged solve are the deleted forms; the inner solve stays on the solver owners and the assembly on `solvers/mesh`, so this owner composes the converged design rather than re-deriving it.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolveStatus
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

    @property
    def carried(self) -> tuple[Callable[[object], object], object]:
        match self:
            case DesignProblem(tag="field", field=payload):
                return payload
            case DesignProblem(tag="mesh", mesh=payload):
                return payload
            case DesignProblem(tag="density", density=payload):
                return payload
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class OutcomeReceipt:
    tag: Literal["design", "program"] = tag()
    design: tuple[str, float, float, int, ContentKey] = case()
    program: tuple[str, float, SolveStatus, float, ContentKey] = case()

    @staticmethod
    def Design(problem: str, objective: float, residual: float, iterations: int, content_key: ContentKey) -> "OutcomeReceipt":
        return OutcomeReceipt(design=(problem, objective, residual, iterations, content_key))

    @staticmethod
    def Program(program: str, objective: float, status: SolveStatus, violation: float, content_key: ContentKey) -> "OutcomeReceipt":
        return OutcomeReceipt(program=(program, objective, status, violation, content_key))

    @property
    def content_key(self) -> ContentKey:
        match self:
            case OutcomeReceipt(tag="design", design=(_, _, _, _, key)):
                return key
            case OutcomeReceipt(tag="program", program=(_, _, _, _, key)):
                return key
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Receipt:
        match self:
            case OutcomeReceipt(tag="design", design=(problem, objective, residual, iterations, key)):
                facts = {
                    "problem": problem,
                    "objective": f"{objective:.6e}",
                    "residual": f"{residual:.3e}",
                    "iterations": str(iterations),
                    "key": str(key.value),
                }
                return Receipt.of("emitted", "compute.optimization.design", problem, facts)
            case OutcomeReceipt(tag="program", program=(program_tag, objective, status, violation, key)):
                facts = {
                    "program": program_tag,
                    "objective": f"{objective:.6g}",
                    "status": status,
                    "converged": str(status is SolveStatus.SUCCESS),
                    "violation": f"{violation:.3e}",
                    "key": str(key.value),
                }
                return Receipt.of("emitted", "compute.optimization.program", program_tag, facts)
            case unreachable:
                assert_never(unreachable)


_TOL = 1e-8
_LR = 1e-2
_FD = 1e-6
_SEED = 0
_JITTER = 1e-2

_RESIDUAL_VECTOR: frozenset[str] = frozenset({"mesh"})


@tagged_union(frozen=True)
class Descent:
    tag: Literal["quasi_newton", "levenberg", "first_order"] = tag()
    quasi_newton: None = case()
    levenberg: None = case()
    first_order: float = case()

    @staticmethod
    def QuasiNewton() -> "Descent":
        return Descent(quasi_newton=None)

    @staticmethod
    def Levenberg() -> "Descent":
        return Descent(levenberg=None)

    @staticmethod
    def FirstOrder(learning_rate: float = _LR) -> "Descent":
        return Descent(first_order=learning_rate)

    def solver(self) -> object:
        import optimistix as optx
        import optax

        match self:
            case Descent(tag="quasi_newton"):
                return optx.BFGS(rtol=_TOL, atol=_TOL)
            case Descent(tag="levenberg"):
                return optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL)
            case Descent(tag="first_order", first_order=learning_rate):
                return optx.OptaxMinimiser(optax.adam(learning_rate), rtol=_TOL, atol=_TOL)
            case unreachable:
                assert_never(unreachable)


_DEFAULT_DESCENT: dict[str, Descent] = {
    "field": Descent.QuasiNewton(),
    "mesh": Descent.Levenberg(),
    "density": Descent.FirstOrder(),
}


def solve(problem: DesignProblem, /, *, descent: "Descent | None" = None, restarts: int = 1, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    return boundary(f"design.{problem.tag}", lambda: _dispatch(problem, descent or _DEFAULT_DESCENT[problem.tag], restarts, seed))


def _dispatch(problem: DesignProblem, descent: "Descent", restarts: int, seed: int) -> OutcomeReceipt:
    objective, params = problem.carried
    key = _design_key(problem.tag, params)
    try:
        return _optimistix_design(problem.tag, objective, params, descent, restarts, seed, key)
    except ImportError:
        return _floor_design(problem.tag, objective, params, key)


def _optimistix_design(
    tag: str, objective: Callable[..., object], params: object, descent: "Descent", restarts: int, seed: int, key: ContentKey
) -> OutcomeReceipt:
    import equinox as eqx
    import jax
    import jax.numpy as jnp
    import optimistix as optx

    design, static = eqx.partition(params, eqx.is_inexact_array)
    residual_vector = tag in _RESIDUAL_VECTOR
    op = optx.least_squares if residual_vector else optx.minimise
    solver = descent.solver()

    @eqx.filter_jit
    def fn(y: object, _: object) -> object:
        return objective(eqx.combine(y, static))

    def run(y0: object) -> optx.Solution:
        return op(fn, solver, y0, adjoint=optx.ImplicitAdjoint())

    if restarts > 1:
        keys = jax.random.split(jax.random.key(seed), restarts)
        starts = eqx.filter_vmap(lambda k: jax.tree_util.tree_map(lambda leaf: leaf + _JITTER * jax.random.normal(k, leaf.shape), design))(keys)
        solution = eqx.filter_vmap(run)(starts)
        objectives = eqx.filter_vmap(lambda v: _objective_scalar(objective, eqx.combine(v, static), residual_vector))(solution.value)
        best = int(jnp.argmin(objectives))
        converged = eqx.combine(jax.tree_util.tree_map(lambda leaf: leaf[best], solution.value), static)
        steps = int(jnp.asarray(solution.stats["num_steps"])[best])
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])

    objective_scalar, residual = _converged_outcome(objective, converged, residual_vector)
    return OutcomeReceipt.Design(tag, objective_scalar, residual, steps, key)


def _floor_design(tag: str, objective: Callable[..., object], params: object, key: ContentKey) -> OutcomeReceipt:
    x0 = np.asarray(params, dtype=float)
    residual_vector = tag in _RESIDUAL_VECTOR
    value = np.asarray(objective(x0))
    objective_scalar = float(np.linalg.norm(value)) if residual_vector else float(value)
    residual = _central_difference_norm(objective, x0, residual_vector)
    return OutcomeReceipt.Design(tag, objective_scalar, residual, 0, key)


def _converged_outcome(objective: Callable[..., object], converged: object, residual_vector: bool) -> tuple[float, float]:
    import equinox as eqx
    import jax.numpy as jnp
    from optimistix import max_norm

    def cost(y: object) -> tuple[object, object]:
        value = jnp.asarray(objective(y))
        scalar = jnp.linalg.norm(value) if residual_vector else value.reshape(())
        return (0.5 * (value**2).sum() if residual_vector else scalar), scalar

    (_, reported), gradient = eqx.filter_value_and_grad(cost, has_aux=True)(converged)
    return float(reported), float(max_norm(gradient))


def _objective_scalar(objective: Callable[..., object], converged: object, residual_vector: bool) -> object:
    import jax.numpy as jnp

    value = jnp.asarray(objective(converged))
    return jnp.linalg.norm(value) if residual_vector else value.reshape(())


def _central_difference_norm(objective: Callable[..., object], x0: np.ndarray, residual_vector: bool) -> float:
    cost = (lambda x: 0.5 * float(np.sum(np.asarray(objective(x)) ** 2))) if residual_vector else (lambda x: float(np.asarray(objective(x))))
    basis = np.eye(x0.size)
    probe = [(cost(x0 + _FD * e) - cost(x0 - _FD * e)) / (2.0 * _FD) for e in basis]
    return float(np.linalg.norm(probe, np.inf))


def _design_key(tag: str, params: object) -> ContentKey:
    buffer = np.ascontiguousarray(np.asarray(params, dtype=float)).tobytes()
    return ContentIdentity.of(f"design-{tag}", buffer, IdentityPolicy())
```

## [03]-[RESEARCH]

- [OPTIMISTIX_DESIGN]: `optimistix` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `minimise`/`least_squares` entries, the `BFGS`/`LevenbergMarquardt` solvers, the `OptaxMinimiser` optax wrapper, the `ImplicitAdjoint` default adjoint, and `Solution.value`/`Solution.stats["num_steps"]` verify against `compute/.api/optimistix.md` under a uv-sync reflection pass on that band. Every Optimistix entry defaults to `ImplicitAdjoint`, so the descent differentiates the converged stationary point through one linear solve per backward pass consumed by `solvers/sensitivity.md#SENSITIVITY`, never the iteration trace; the numpy central-difference floor runs unconditionally on cp315 so every case stays reachable without the wheel.
- [EQUINOX_PARAM]: the `equinox.partition`/`combine` PyTree split over `equinox.is_inexact_array` separates the inexact-array design leaves the optimizer threads from the static rest, and the combined PyTree is the `y0` Optimistix carries through iteration; the spellings verify against `compute/.api/equinox.md` once the equinox wheel resolves on the gated band. The partitioned objective compiles through `equinox.filter_jit` so the static rest skips tracing, and a `restarts>1` ensemble jitters the design leaves through `jax.random.split`/`jax.random.normal` over the seeded `jax.random.key(seed)`, threads the partitioned solve through `equinox.filter_vmap` over the leading restart axis, and folds the best converged objective through one `jax.tree_util.tree_map` slice — the `key`/`split`/`normal` PRNG triple confirmed in `compute/.api/jax.md` and both filter transforms in `compute/.api/equinox.md`, never a broadcast-copy restart and never a per-start optimizer surface. The descent engine is the `Descent` policy vocabulary projected to its Optimistix/optax solver in `Descent.solver` — `optimistix.BFGS`/`LevenbergMarquardt` and `optimistix.OptaxMinimiser(optax.adam(lr), ...)` all catalogued in `compute/.api/optimistix.md`/`compute/.api/optax.md`, defaulted per route through `_DEFAULT_DESCENT` and overridable per call — never a hardcoded per-case solver and never a hand-rolled momentum accumulator.
- [JAX_ADJOINT]: the `equinox.filter_value_and_grad(cost, has_aux=True)` pass folds the converged objective and the first-order/KKT residual in one trace, carries the `python_version<'3.15'` marker, and verifies against `compute/.api/equinox.md`/`compute/.api/jax.md` once the jaxlib wheel resolves; it differentiates the scalar objective for the `field`/`density` minimise routes and the least-squares cost `½‖r‖²` for the `mesh` `least_squares` route (whose gradient is `Jᵀr`) w.r.t. the inexact-array design leaves only, returns the reported objective scalar (`‖r‖` on the residual route, the raw scalar otherwise) as the differentiation aux so the objective is never re-evaluated, and the gradient PyTree's L∞ stationarity norm reads `optimistix.max_norm` directly over the PyTree (confirmed `(x: PyTree[Array]) → Shaped[Array,'']`), so the converged objective and residual are computed on-device through `jax.numpy`/filter transforms and `numpy.asarray` never touches a JAX PyTree. The residual is the stationarity gradient `∇f` (respectively `Jᵀr`) that vanishes at the converged design the receipt folds and the `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads. The central-difference residual floor over an `np.eye` perturbation basis runs on the real numpy `x0` buffer only, differentiates the same scalar cost, and reproduces the same first-order quantity unconditionally on cp315.
