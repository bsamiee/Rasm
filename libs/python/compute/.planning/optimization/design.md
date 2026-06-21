# [PY_COMPUTE_DESIGN]

`DesignProblem` is the gradient-driven inverse-design apex the autodifferentiable solver chain enables and no other owner closes. It discriminates a `Field` problem over the `solvers/mesh.md#MESH_FIELD` assembled system, a parametric-`Mesh` objective, and a material-distribution `Density` objective, driving an Equinox-partitioned PyTree objective to a stationary point through `optimistix.minimise`/`least_squares` over the JAX floor. Every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so the gradient `solvers/sensitivity.md#SENSITIVITY` pulls back is the implicit-function-theorem gradient of the converged solution, never the iteration trace.

One dispatch threads all three cases: the `@functools.cache`-built `_objective()` shape table selects the `minimise`/`least_squares` entry from a typed `Shape`, the `Descent` `@tagged_union` projects to its Optimistix/optax solver in `Descent.solver` — defaulted per route, overridable per call, the `optax` first-order engine threading through `optimistix.OptaxMinimiser(optax.chain(...))` with the `density` route folding a chain-shaped feasibility projection through the cached `_feasible()` table. Each route folds one content-keyed `OutcomeReceipt.Design` over the final iterate, the converged objective, and the first-order/KKT residual that graduates outward on the existing `solver` axis at `graduation/handoff.md#GRADUATION`.

`OutcomeReceipt` is the ONE optimization-outcome owner: the `design` convergence verdict (objective, residual, iterations, `SolveStatus`) and the `program` feasibility verdict (objective, `SolveStatus`, violation) are two cases of one `@tagged_union`, folding `optimization/program.md#PROGRAM` onto the same receipt rather than a parallel struct. Both cases carry the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns rather than a lone `bool success`, so the Optimistix `RESULTS` verdict and the host `scipy.optimize` verdict speak one vocabulary and the success contract survives as the derived `status is SolveStatus.SUCCESS` predicate. `ConvexReceipt` stays distinct because it carries the duality-gap/dual-infeasibility/status certificate the first-order and feasibility verdicts have no field for.

A numpy central-difference floor reports the gradient-norm residual at `x0` for every case, reachable on cp315 behind the `_backend` import-guard, so a run without the jaxlib wheel never returns `Error(Import)`. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

## [01]-[INDEX]

- [01]-[DESIGN]: field/mesh/density inverse-design over an Equinox-parameterized `Objective` through one `_objective()` shape-keyed `optimistix.minimise`/`least_squares` dispatch with the implicit-adjoint gradient, the `Descent`-projected Optimistix/optax solver, and the `_backend` optimistix-vs-floor import-guard rail, folding the `design` case of the shared `OutcomeReceipt` on one `DesignProblem` owner whose `contribute` streams the `ReceiptContributor` port.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the ONE inverse-design owner; `DesignProblem` discriminates `Field(objective)` (an `Objective` over a `solvers/mesh` `AssembledSystem` stiffness/load discretization), `Mesh(objective)` (a parametric-mesh `Objective` over node coordinates), and `Density(objective)` (a material-distribution `Objective` over a SIMP-style design field), each case carrying one `Objective` value object. The provenance of the objective is the discriminant; the optimizer is one surface. The cases never become a per-problem optimizer family — `Field`, `Mesh`, and `Density` are rows on the same owner discriminated by the structure the objective integrates over, and the `carried` property folds the case to its `Objective` total over `match`/`assert_never` so a new provenance breaks the extractor rather than spawning a parallel dispatch arm.
- Objective value object: `Objective` is the frozen `Struct` carrying the cost thunk `fn`, the initial parameter PyTree `params`, and the `Shape` discriminant (`SCALAR` for the `field`/`density` minimise routes, `RESIDUAL` for the `mesh` inverse-identification least-squares route). The shape is a typed field on the `Objective`, never a stringy `frozenset({"mesh"})` membership test against the tag, so the `minimise`-vs-`least_squares` entry and the `½‖r‖²`-vs-scalar cost both read one `Shape` value. `Objective.cost` is the input-and-output-parameterized projection: it lowers the raw thunk output through `jax.numpy.asarray` and returns the `(reduced, reported)` pair the `Shape` selects — `(½‖r‖², ‖r‖)` on `RESIDUAL`, `(scalar, scalar)` on `SCALAR` — so the differentiated cost and the reported objective scalar fold from one definition rather than the prior `_converged_outcome`/`_objective_scalar` twin helpers each rebuilding the reduction.
- Parameterization: the `Objective.params` PyTree is partitioned through `equinox.partition`/`combine` over `equinox.is_inexact_array` into the inexact-array design leaves the optimizer threads and the static rest; `optimistix.minimise` carries the combined PyTree through iteration and the implicit adjoint differentiates the converged stationary point. The descent engine is not hardcoded per case: `Descent` is the `@tagged_union` vocabulary — `QuasiNewton` (`optimistix.BFGS`), `Levenberg` (`optimistix.LevenbergMarquardt`), and `FirstOrder` (`optimistix.OptaxMinimiser` over an `optax.chain`) — so the engine is a policy value carried on `DesignProblem.solve`, defaulting per route through the `_DEFAULT_DESCENT` table (`field`→`QuasiNewton`, `mesh`→`Levenberg`, `density`→`FirstOrder`) and overridable per call. The `Descent.admits(shape)` invariant gates an override — `Levenberg` requires the `RESIDUAL` least-squares route, the scalar minimisers require `SCALAR` — so a mismatched engine (an LM least-squares solver on a scalar objective) mints a typed `Error(BoundaryFault(boundary=...))` onto the rail rather than reaching the wrong solve entry. The `Descent.solver` projection builds the Optimistix solver from the policy and wraps it in the matching `optimistix.BestSoFarMinimiser`/`BestSoFarLeastSquares` so a non-monotone final iterate never poisons the converged design, so a new descent engine is a new `Descent` case mapping to its Optimistix/optax constructor, never a hand-rolled momentum loop and never a parallel descent surface.
- Feasibility: the `FirstOrder` descent carries an optional `Feasible` projection row (`BOX` for densities ∈ [0,1], `SIMPLEX` for a material-fraction simplex, `NONNEGATIVE` for a sign constraint, `FREE` for none) folded into the `optax.chain` through the cached `_feasible()` table as a chain-shaped `GradientTransformation`, never a bare `projection_box(x, lower, upper)` callable a `chain` cannot compose. The `_projected` lift wraps an `optax.projections` Euclidean projection in a stateless `GradientTransformationExtraArgs` whose `update` returns the corrected delta `projection(apply_updates(params, updates)) − params` over `optax.tree_utils.tree_add`/`tree_scale`, so the next iterate lands on the feasible set inside the chain rather than in a post-`apply_updates` body call the `OptaxMinimiser` solve has no seam for; `NONNEGATIVE` folds the catalogued stateful `optax.keep_params_nonnegative` transform directly. A SIMP density descent thus keeps its design vector feasible through projected gradient descent without a penalty term and without a parallel constrained optimizer — the `density` default is `FirstOrder(feasible=Feasible.BOX)`. A heterogeneous design vector descending under per-block policy routes through `optax.multi_transform` on the same `Descent.solver` projection, never a second optimizer beside the chain.
- Compilation: the partitioned objective `fn(y, args)` is wrapped once through `equinox.filter_jit` so the static rest skips tracing and the inexact-array design leaves are the only XLA-traced inputs — this is the same compile surface `solvers/nonlinear.md#NONLINEAR` threads through Optimistix, composed here, never re-derived. A `restarts>1` multi-start jitters `y0` across a leading restart axis through `jax.random.split`/`jax.random.normal` over the seeded PRNG key — each start is a distinct perturbed design, not a broadcast copy — and threads the partitioned solve through `equinox.filter_vmap` over that axis, returning the per-start converged values from which the route folds the best objective through one `jax.tree_util.tree_map` slice at `jax.numpy.argmin`; the ensemble is one `filter_vmap` row on the same `solve` owner, never a parallel multi-start optimizer surface.
- Adjoint: every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so a sensitivity through a design that itself solves an inner system differentiates the converged solution through one linear solve per backward pass rather than backpropagating the iterations; `solvers/sensitivity.md#SENSITIVITY` reads that adjoint through `jax.vjp` over the objective, and `solvers/linear.md#LINEAR`/`solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` expose the autodifferentiable inner solves the `Field` objective composes (the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here). The converged objective and the first-order/KKT residual fold from one `equinox.filter_value_and_grad(Objective.cost, has_aux=True)` pass over the partitioned cost — value and gradient w.r.t. the inexact-array design leaves in a single trace, the reported objective scalar carried as the differentiation aux so it is never re-evaluated, never a separate objective evaluation and a re-traced gradient — so the residual norm is the gradient PyTree's L∞ stationarity norm through `optimistix.max_norm` directly over the PyTree, never by routing the converged PyTree through `numpy.asarray`.
- Entry: `DesignProblem.solve` enters one `boundary(f"design.{problem.tag}", ...).bind(lambda r: r)`, so the solve fence and the `_design_key` `RuntimeRail[ContentKey]` digest rail join on one `RuntimeRail[OutcomeReceipt]` without double-wrapping — the same rail-join shape the sibling `optimization/program.md#PROGRAM` `solve` holds. The body resolves the `Descent` policy (caller override or the `_DEFAULT_DESCENT` per-tag default), gates it through `Descent.admits(problem.carried.shape)` so a mismatched override mints a typed `Error(BoundaryFault(boundary=...))` directly onto the rail — never a `raise` relying on the fence to re-classify, the ROP form the sibling `program.md` `Termination` adjudication holds — rather than reaching the wrong solve entry, dispatches the `_backend_outcome` import-guard (`_optimistix` tries the gated Optimistix route, falling to `_floor`, the numpy central-difference floor, on `ImportError`) into the deferred `Callable[[ContentKey], OutcomeReceipt]` receipt builder, and `Result.map`s the railed `_design_key` digest into it so a digest fault rides the one rail rather than collapsing to a phantom bare `ContentKey`. The `minimise`-vs-`least_squares` entry is the `_objective()[shape]` row the `Objective.shape` selects (the `@functools.cache`-built table deferring the gated `optimistix` import past module load), the route is the `Descent.solver()` projection plus that entry, the solve runs `throw=False` so a non-`successful` `Solution.result` reaches the receipt as its mapped `SolveStatus` rather than raising, and the converged objective and residual fold from one `filter_value_and_grad`. The `_design_key` digest over the canonical objective-parameter buffer keys a design re-run from the same starting point identically across backends.
- Receipt: `OutcomeReceipt` is the ONE optimization-outcome receipt this owner and `optimization/program.md#PROGRAM` share — `OutcomeReceipt.Design` folds the design convergence verdict and `OutcomeReceipt.Program` folds the math-program feasibility verdict on the same `@tagged_union`, both minted through the `@classmethod`-plus-`Self` form that binds the subtype once (never a `@staticmethod` over a `"OutcomeReceipt"` forward-ref string). The case payloads are owned by one `_OUTCOME_SLOTS` field-name table — the same data-driven projection the sibling `solvers/receipt.md#RECEIPT` `SolverReceipt._SLOTS` establishes — so `.facts` zips `_OUTCOME_SLOTS[self.tag]` against the case tuple under `zip(..., strict=True)`, `.status`/`.content_key` read named slots off that map, and `.contribute` spreads one fold rather than a per-case hand-spelled `dict`: a single `Receipt.of(f"compute.optimization.{self.tag}", ("emitted", subject, facts))` row whose facts carry the full per-case numeric evidence (the converged objective, the first-order/KKT residual, the iteration count, the `SolveStatus`, the constraint violation) with `key` lowered to `ContentKey.hex` and the derived `converged` injected. The `strict=True` zip makes the slot row and the case-tuple length structurally inseparable — a row that drifts from its payload raises rather than truncating evidence. Both cases carry `SolveStatus` (`solvers/receipt.md#RECEIPT`): the `design` case folds the `optimistix.Solution.result` `RESULTS` member name through the receipt owner's `_STATUS` vocabulary at the boundary, the `program` case carries the `scipy.optimize` verdict, so both speak one termination vocabulary the C# graduation gate reads. The facts ride as native scalars — the objective and residual as `float`, the iteration count as `int`, the status as the `SolveStatus` `StrEnum` member, the key through `ContentKey.hex` — into the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` its `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce, never a pre-`f""`-formatted `dict[str, str]`. The `Program` factory and the program-route body that maps the `OptimizeResult` into that vocabulary live on `optimization/program.md#PROGRAM`. The residual is the convergence evidence the C# graduation gate reads through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` to admit or reject a converged design for kernel handoff — no new handoff axis and no graduation edit. `ConvexReceipt` (`optimization/convex.md#CONVEX`) stays a distinct struct: its duality-gap, dual-infeasibility, and solver-status certificate is the global-optimality proof object the first-order and feasibility verdicts carry no field for, so folding it onto `OutcomeReceipt` would erase the spectral/convergence certificate.
- Egress: `OutcomeReceipt` implements the `observability/receipts#RECEIPT` `ReceiptContributor` port through `contribute`, so the converged outcome streams its `Receipt` sequence into the one egress fold the consumer drives — `Signals.emit`, or the cross-cutting `@receipted` aspect over a measured study kernel that returns the receipt as a `ReceiptContributor`. `DesignProblem.solve` returns the `RuntimeRail[OutcomeReceipt]` and threads no inline `emit`: the solve fence and the receipt egress stay orthogonal, `boundary` mints the rail, the receipt owns its projection, and a consumer drives graduation through `GraduationReceipt.graduates` on the `solver` axis.
- Packages: `optimistix` (`minimise`, `least_squares`, `OptaxMinimiser`, `BFGS`, `LevenbergMarquardt`, `BestSoFarMinimiser`, `BestSoFarLeastSquares`, `ImplicitAdjoint`, `Solution`, `RESULTS`/`RESULTS.promote` — the multi-start vmap code-combine reducing the per-start verdicts to the worst-case termination, `max_norm` — the Chebyshev stationarity norm over the gradient PyTree; the `Solution.result` `RESULTS` member name folds into `SolveStatus` and `throw=False` defers the non-success verdict to the receipt), `equinox` (`partition`, `combine`, `is_inexact_array`, `filter_jit`, `filter_vmap`, `filter_value_and_grad` — the partition/compile/vectorize/combined-grad filter transforms threading the inexact-array design leaves, the value-and-grad-with-aux fold the converged objective and residual share), `jax` (`numpy` the on-device reduction namespace; `random.key`/`random.split`/`random.normal` the seeded multi-start jitter; `tree_util.tree_map` the per-start value slice at `jax.numpy.argmin`; the autodiff floor the objective and the adjoint resolve over), `optax` (`adam`, `sgd`, `chain`, `multi_transform`, `clip_by_global_norm`, `GradientTransformationExtraArgs`/`EmptyState`/`apply_updates`/`keep_params_nonnegative`/`tree_utils.tree_add`/`tree_utils.tree_scale` — the chain-shaped `_projected` lift folding an `optax.projections.projection_box`/`projection_simplex`/`projection_non_negative` Euclidean projection into the `density` chain as a `GradientTransformation`, the first-order-descent engine threaded through `OptaxMinimiser`), `numpy` (`asarray`, `eye`, `fromiter`, `linalg.norm`, `concatenate`, `ascontiguousarray` — the host-side central-difference residual floor over real arrays only (the `Shape`-keyed `_floor_cost` reduction, the `np.eye` basis reshaped per-perturbation into `x0`'s structure, the `np.fromiter` probe fold), never over a JAX PyTree, and the canonical concatenated key buffer), `solvers/receipt.md#RECEIPT` (`SolveStatus` and its `_STATUS` `RESULTS`→`SolveStatus` boundary vocabulary the shared `design`/`program` cases carry), `solvers/sensitivity.md#SENSITIVITY` (the implicit-adjoint gradient read through the converged solve), `solvers/{linear,nonlinear,differential}.md` (the autodifferentiable inner solves the `Field` objective composes), `solvers/mesh.md#MESH_FIELD` (`AssembledSystem` stiffness/load for the field objective), `graduation/handoff.md#GRADUATION` (the `solver` axis the converged design graduates on), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor` the `OutcomeReceipt` contributes through, the `@receipted` aspect and `Signals.emit` available to the consuming study kernel).
- Growth: a new design provenance is one `DesignProblem` case plus one `_DEFAULT_DESCENT` row; a new objective shape is one `Shape` member plus one `_objective()` row, one `Objective.cost` arm, and its `_floor_cost` host-mirror arm (both `assert_never`-closed so the addition is compile-surfaced); a new descent engine is one `Descent` case mapping to its Optimistix/optax constructor in `Descent.solver` (a `chain` of `optax` transformations, an `optimistix.LBFGS`/`NonlinearCG`, or a trust-region least-squares solver), selectable per call without a body edit; a new feasibility constraint is one `Feasible` member plus one `_feasible()` projection row; a new outcome-receipt evidence field is one `_OUTCOME_SLOTS` slot plus its case-tuple position with no `contribute` edit, the slot table projecting it for free; a perturbed multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`; zero new surface, never a parallel field-design and density-design owner, never a per-case helper body, never a per-case fact dict beside the `_OUTCOME_SLOTS` projection, never a training loop.
- Boundary: offline inverse design only — PDE-constrained optimal design, parametric-mesh shape optimization, and material-distribution density optimization driven to a stationary point are in-scope; `jax`/`jaxlib`/`optimistix`/`equinox`/`optax` carry no cp315 wheel, so the Optimistix body is authored against the documented API behind one gated import, and the `_floor` numpy central-difference floor runs unconditionally for every case so a cp315 run never returns `Error(Import)`. The `_feasible()`/`_objective()` dispatch tables are `@functools.cache`-built rather than module-level `_FEASIBLE`/`_OBJECTIVE` assignments, so the gated `optax`/`optimistix` imports defer past module load and the module itself imports on the cp315 core where the floor must stay reachable — an eager table assignment importing the gated wheel at load is the deleted form that would render the whole `_floor` premise unreachable. A separate optimizer owner per design provenance, a module-level table eagerly importing the gated `optax`/`optimistix` at load against the cached lazy builder, a stringy `frozenset({"mesh"})` shape probe parallel to the typed `Shape`, a twin `_converged_outcome`/`_objective_scalar` reduction parallel to the one device `Objective.cost` (distinct from the sanctioned host-floor mirror `_floor_cost`, which folds the SAME `Shape`-keyed `(reduced, reported)` split over real numpy where importing the device `Objective.cost` would pull the gated jaxlib wheel onto cp315), a bare `optax.projections.projection_box(x, lower, upper)` callable spliced into an `optax.chain` that composes only `GradientTransformation`s, a `Levenberg` least-squares engine fed to a scalar `minimise` route (or a scalar minimiser to a `least_squares` route) where `Descent.admits` gates the engine/shape pairing, a `raise ValueError` for the engine/shape mismatch where `_mismatch` mints a typed `Error(BoundaryFault(boundary=...))` onto the rail, a `@staticmethod`-plus-`"OutcomeReceipt"`-forward-ref factory where the `@classmethod`-plus-`Self` form binds the subtype once, a per-case hand-spelled `contribute` fact dict where the one `_OUTCOME_SLOTS`-driven `.facts` zip projects both cases, a per-case `_*_receipt` helper body, a 4-positional `Receipt.of(phase, owner, subject, facts)` call against the two-argument `of(owner, evidence)` contract, a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port, a `str()`/`f""`-coerced `dict[str, str]` facts map where the renderer carries native scalars, a `str(key.value)` render where `ContentKey.hex` holds, a `@receipted` decoration on the `RuntimeRail`-returning `solve` where the aspect wraps a `ReceiptContributor`-returning kernel and the receipt owns its `contribute` projection, an inline `Signals.emit` threaded through the solve body where the consumer drives egress, a gradient-descent training loop, a production solver session, and a parallel optimizer surface beside the converged solve are the deleted forms; the inner solve stays on the solver owners and the assembly on `solvers/mesh`, so this owner composes the converged design rather than re-deriving it.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import functools
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import Error, case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.receipt import SolveStatus, _STATUS
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # gated `python_version<'3.15'` annotation carriers only; no wheel imports at runtime
    import jax
    import optax
    import optimistix as optx

    type PyTree = jax.Array | dict[str, "PyTree"] | list["PyTree"] | tuple["PyTree", ...]
    type DesignSolver = optx.AbstractMinimiser | optx.AbstractLeastSquaresSolver
    type DesignEntry = Callable[..., optx.Solution]

# --- [TYPES] ----------------------------------------------------------------------------


class Shape(StrEnum):
    SCALAR = "scalar"  # minimise: fn(y) -> scalar
    RESIDUAL = "residual"  # least_squares: fn(y) -> residual vector, cost ½‖r‖²


class Feasible(StrEnum):
    FREE = "free"
    BOX = "box"  # densities ∈ [0, 1]
    SIMPLEX = "simplex"  # material-fraction simplex
    NONNEGATIVE = "nonnegative"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TOL: float = 1e-8
_LR: float = 1e-2
_CLIP: float = 1e3  # global-norm step bound guarding a diverged inner solve
_FD: float = 1e-6
_SEED: int = 0
_JITTER: float = 1e-2
_MAX_STEPS: int = 256

# Per-case payload field names, one tuple per `OutcomeReceipt` tag, subject in slot 0 and the shared
# trailing `key` slot common to both rows; the `design` verdict sits in slot 4, the `program` verdict
# in slot 2. The ONE owner over the case shapes the sibling `SolverReceipt._SLOTS` establishes: the
# `.facts` zip packs by it and `contribute` spreads it, so a case's evidence is one row, never a
# per-case hand-spelled fact dict.
_OUTCOME_SLOTS: FrozenDict[str, tuple[str, ...]] = FrozenDict(
    {
        "design": ("problem", "objective", "residual", "iterations", "status", "key"),
        "program": ("program", "objective", "status", "violation", "key"),
    }
)

# --- [MODELS] ---------------------------------------------------------------------------


class Objective(Struct, frozen=True):
    fn: "Callable[[PyTree], PyTree]"  # the raw cost thunk over the design PyTree; container-holding, so GC-tracked
    params: "PyTree"
    shape: Shape = Shape.SCALAR

    def cost(self, y: "PyTree") -> "tuple[jax.Array, jax.Array]":
        # one input-and-output-parameterized projection: the differentiated reduction and the
        # reported objective scalar fold from one definition — ½‖r‖² for the gradient Jᵀr on the
        # residual route, the raw scalar otherwise — carried as the value-and-grad aux so the
        # objective is evaluated once, never a re-traced second pass.
        import jax.numpy as jnp

        value = jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                norm = jnp.linalg.norm(value)
                return 0.5 * (value**2).sum(), norm
            case Shape.SCALAR:
                scalar = value.reshape(())
                return scalar, scalar
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class OutcomeReceipt:
    tag: Literal["design", "program"] = tag()
    design: tuple[str, float, float, int, SolveStatus, ContentKey] = case()
    program: tuple[str, float, SolveStatus, float, ContentKey] = case()

    @classmethod
    def Design(
        cls, problem: str, objective: float, residual: float, iterations: int, status: SolveStatus, content_key: ContentKey
    ) -> Self:
        return cls(design=(problem, objective, residual, iterations, status, content_key))

    @classmethod
    def Program(cls, program: str, objective: float, status: SolveStatus, violation: float, content_key: ContentKey) -> Self:
        return cls(program=(program, objective, status, violation, content_key))

    @property
    def status(self) -> SolveStatus:
        # the trailing-but-one `design` verdict (slot 4) and the mid `program` verdict (slot 2) read
        # by structural position so the match narrows totally — the typed read stays union-total while
        # the egress projection rides the `_OUTCOME_SLOTS` zip.
        match self:
            case OutcomeReceipt(tag="design", design=(*_, status, _)):
                return status
            case OutcomeReceipt(tag="program", program=(_, _, status, _, _)):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    @property
    def content_key(self) -> ContentKey:
        match self:
            case OutcomeReceipt(tag="design", design=(*_, key)) | OutcomeReceipt(tag="program", program=(*_, key)):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def facts(self) -> "dict[str, str | float | int | SolveStatus | ContentKey]":
        # the one `_OUTCOME_SLOTS`-driven projection both cases share, the same data-driven shape the
        # sibling `solvers/receipt.md#RECEIPT` `SolverReceipt.facts` owns: `zip(..., strict=True)`
        # makes the slot row and the case-tuple length structurally inseparable, so a row that drifts
        # from its payload raises rather than truncating evidence — never a per-case hand-spelled dict.
        return dict(zip(_OUTCOME_SLOTS[self.tag], getattr(self, self.tag), strict=True))

    def contribute(self) -> Iterable[Receipt]:
        # the runtime `Receipt.of(owner, (phase, subject, facts))` contract: the `emitted`-phase triple
        # mints the `fact` case. The `_OUTCOME_SLOTS` projection carries native `float`/`int`/`StrEnum`
        # facts the `enc_hook=repr` renderer serializes without a coerce; `key` lowers to `ContentKey.hex`
        # and the derived `converged` rides alongside, both off the one fold rather than two dict arms.
        facts: dict[str, object] = {**self.facts, "key": self.content_key.hex, "converged": self.converged}
        subject = getattr(self, self.tag)[0]  # the problem/program tag is slot 0 of every case payload
        return (Receipt.of(f"compute.optimization.{self.tag}", ("emitted", subject, facts)),)


@tagged_union(frozen=True)
class Descent:
    tag: Literal["quasi_newton", "levenberg", "first_order"] = tag()
    quasi_newton: None = case()
    levenberg: None = case()
    first_order: tuple[float, Feasible] = case()

    @classmethod
    def QuasiNewton(cls) -> Self:
        return cls(quasi_newton=None)

    @classmethod
    def Levenberg(cls) -> Self:
        return cls(levenberg=None)

    @classmethod
    def FirstOrder(cls, learning_rate: float = _LR, feasible: Feasible = Feasible.FREE) -> Self:
        return cls(first_order=(learning_rate, feasible))

    def solver(self) -> "DesignSolver":
        # the `Descent` policy projected to its Optimistix/optax solver, wrapped in the `BestSoFar*`
        # guard matching the SOLVER class (least-squares LM under `BestSoFarLeastSquares`, the scalar
        # minimisers under `BestSoFarMinimiser`) so a non-monotone final iterate never poisons the
        # converged design; a new engine is a new case mapping to its constructor, never a
        # hand-rolled momentum loop. The `first_order` arm folds only chain-shaped
        # `GradientTransformation`s — the cached `_feasible()` constraint transform
        # (`optax.keep_params_nonnegative`/projection-as-transform), never a bare
        # `projection_box(x, lower, upper)` callable a `chain` cannot compose.
        import optax
        import optimistix as optx

        match self:
            case Descent(tag="quasi_newton"):
                return optx.BestSoFarMinimiser(optx.BFGS(rtol=_TOL, atol=_TOL))
            case Descent(tag="levenberg"):
                return optx.BestSoFarLeastSquares(optx.LevenbergMarquardt(rtol=_TOL, atol=_TOL))
            case Descent(tag="first_order", first_order=(learning_rate, feasible)):
                chain = optax.chain(optax.clip_by_global_norm(_CLIP), optax.adam(learning_rate), *_feasible()[feasible])
                return optx.BestSoFarMinimiser(optx.OptaxMinimiser(chain, rtol=_TOL, atol=_TOL))
            case _ as unreachable:
                assert_never(unreachable)

    def admits(self, shape: Shape) -> bool:
        # the engine/shape compatibility invariant: `Levenberg` requires the `RESIDUAL`
        # least-squares route, the scalar minimisers require `SCALAR`, so an override mismatched to
        # the objective shape is rejected at the boundary rather than fed to the wrong solve entry.
        match self:
            case Descent(tag="levenberg"):
                return shape is Shape.RESIDUAL
            case _:
                return shape is Shape.SCALAR


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_DESCENT: FrozenDict[str, Descent] = FrozenDict(
    {
        "field": Descent.QuasiNewton(),
        "mesh": Descent.Levenberg(),
        "density": Descent.FirstOrder(feasible=Feasible.BOX),
    }
)


def _projected(projection: "Callable[[PyTree], PyTree]") -> "optax.GradientTransformationExtraArgs":
    # lifts an `optax.projections` Euclidean projection into a chain-shaped stateless
    # `GradientTransformationExtraArgs` applied at `update` over the candidate `params + updates`,
    # so projected gradient descent folds into the `optax.chain` instead of a post-`apply_updates`
    # body call a `chain`/`OptaxMinimiser` cannot host. `update` returns the corrected delta
    # `projection(params + updates) - params` so the next iterate lands on the feasible set.
    import optax

    def init(_: "PyTree") -> "optax.EmptyState":
        return optax.EmptyState()

    def update(updates: "PyTree", state: "optax.OptState", params: "PyTree | None" = None, **_: object) -> "tuple[PyTree, optax.OptState]":
        import optax as _optax

        candidate = _optax.apply_updates(params, updates)
        corrected = _optax.tree_utils.tree_add(projection(candidate), _optax.tree_utils.tree_scale(-1.0, params))
        return corrected, state

    return optax.GradientTransformationExtraArgs(init, update)


@functools.cache
def _feasible() -> "FrozenDict[Feasible, tuple[optax.GradientTransformation, ...]]":
    # the gated `optax` import is deferred to first call so the module loads on the cp315 core where
    # no jaxlib/optax wheel resolves — the `_floor` path stays reachable. `@functools.cache` builds
    # the table once on the first gated `Descent.solver` call. Each `Feasible` row is a tuple of
    # chain-foldable `GradientTransformation`s, never a bare `projection_box(x, lower, upper)`
    # callable: `BOX`/`SIMPLEX` lift their `optax.projections` projection through `_projected`,
    # `NONNEGATIVE` folds the stateful `keep_params_nonnegative` transform the catalogue names for
    # in-chain box feasibility, `FREE` is the empty tuple.
    import optax

    return FrozenDict(
        {
            Feasible.FREE: (),
            Feasible.BOX: (_projected(functools.partial(optax.projections.projection_box, lower=0.0, upper=1.0)),),
            Feasible.SIMPLEX: (_projected(optax.projections.projection_simplex),),
            Feasible.NONNEGATIVE: (optax.keep_params_nonnegative(),),
        }
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def solve(problem: DesignProblem, /, *, descent: "Descent | None" = None, restarts: int = 1, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
    # the override resolves per-tag default or caller policy; `Descent.admits` rejects an
    # engine/shape mismatch (an LM least-squares solver on a scalar objective) as a typed
    # `Error(BoundaryFault(boundary=...))` minted directly onto the rail — never a `raise` relying on
    # the fence to re-classify — so the mismatch verdict is ROP, not exception control flow. The body
    # returns the railed `OutcomeReceipt` (the `_design_key` digest rides `RuntimeRail`), so `boundary`
    # over the rail join `.bind`-flattens the solve fence and the digest rail onto one
    # `RuntimeRail[OutcomeReceipt]` without double-wrapping, the sibling `program.md` join shape.
    chosen = descent if descent is not None else _DEFAULT_DESCENT[problem.tag]
    return boundary(
        f"design.{problem.tag}",
        lambda: _backend(problem, chosen, restarts, seed) if chosen.admits(problem.carried.shape) else _mismatch(problem, chosen),
    ).bind(lambda r: r)


def _mismatch(problem: "DesignProblem", descent: "Descent") -> "RuntimeRail[OutcomeReceipt]":
    detail = f"{descent.tag} does not admit {problem.tag} objective shape {problem.carried.shape}"
    return Error(BoundaryFault(boundary=(f"design.{problem.tag}", detail)))


def _backend(problem: "DesignProblem", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[OutcomeReceipt]":
    # the `_design_key` digest is `RuntimeRail[ContentKey]` (the runtime owner's `view="value"`
    # default), threaded into the converged receipt through `Result.map` so a digest fault rides the
    # one rail rather than collapsing to a phantom bare `ContentKey`; the import-guard chooses the
    # gated Optimistix route or the cp315 numpy floor before the railed fold.
    objective = problem.carried
    railed = _backend_outcome(problem.tag, objective, descent, restarts, seed)
    return _design_key(problem.tag, objective.params).map(railed)


def _backend_outcome(
    tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int
) -> "Callable[[ContentKey], OutcomeReceipt]":
    try:
        return _optimistix(tag, objective, descent, restarts, seed)
    except ImportError:
        return _floor(tag, objective)


def _optimistix(
    tag: str, objective: "Objective", descent: "Descent", restarts: int, seed: int
) -> "Callable[[ContentKey], OutcomeReceipt]":
    import equinox as eqx
    import jax
    import jax.numpy as jnp
    import optimistix as optx

    design, static = eqx.partition(objective.params, eqx.is_inexact_array)
    op = _objective()[objective.shape]
    solver = descent.solver()

    @eqx.filter_jit
    def fn(y: "PyTree", _: object) -> "jax.Array":
        reduced, _reported = objective.cost(eqx.combine(y, static))
        return reduced

    def run(y0: "PyTree") -> "optx.Solution":
        return op(fn, solver, y0, adjoint=optx.ImplicitAdjoint(), max_steps=_MAX_STEPS, throw=False)

    if restarts > 1:
        keys = jax.random.split(jax.random.key(seed), restarts)
        starts = eqx.filter_vmap(
            lambda k: jax.tree_util.tree_map(lambda leaf: leaf + _JITTER * jax.random.normal(k, leaf.shape), design)
        )(keys)
        solution = eqx.filter_vmap(run)(starts)
        scored = eqx.filter_vmap(lambda v: objective.cost(eqx.combine(v, static))[0])(solution.value)
        best = int(jnp.argmin(scored))
        converged = eqx.combine(jax.tree_util.tree_map(lambda leaf: leaf[best], solution.value), static)
        steps = int(jnp.asarray(solution.stats["num_steps"])[best])
        # the per-start `RESULTS` reduce to the single worst-case termination through
        # `optimistix.RESULTS.promote` — the catalogued vmap code-combine the sibling
        # `solvers/nonlinear.md#NONLINEAR` batched path folds, so the ensemble carries its true
        # aggregate verdict, never a per-leaf enumeration index assumption nor a `result=None` fiction.
        result = optx.RESULTS.promote(solution.result)
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])
        result = solution.result

    # the converged objective and the L∞ stationarity residual fold from one value-and-grad-with-aux
    # pass; the receipt builder is deferred over the railed `ContentKey` the `.map` in `_backend`
    # supplies, so the gated solve evaluates eagerly inside the import guard while the key stays railed.
    (_, reported), gradient = eqx.filter_value_and_grad(objective.cost, has_aux=True)(converged)
    objective_value, residual, status = float(reported), float(optx.max_norm(gradient)), _STATUS.get(_result_name(result), SolveStatus.OTHER)
    return lambda key: OutcomeReceipt.Design(tag, objective_value, residual, steps, status, key)


def _floor(tag: str, objective: "Objective") -> "Callable[[ContentKey], OutcomeReceipt]":
    # the floor reduces the same `Objective.shape`-keyed `(reduced, reported)` split as `Objective.cost`
    # over the real numpy buffer through the host mirror `_floor_cost`, so the floor verdict and the
    # gated verdict read one reduction: `cost` is the differentiated form the probe folds, `reported`
    # the reported scalar the receipt carries.
    x0 = np.asarray(objective.params, dtype=float)
    cost, reported = _floor_cost(objective)
    residual = _central_difference_norm(cost, x0)
    status = SolveStatus.SUCCESS if np.isfinite(residual) and residual <= _TOL else SolveStatus.STAGNATION
    return lambda key: OutcomeReceipt.Design(tag, reported(x0), residual, 0, status, key)


def _result_name(result: object) -> str:
    # `optimistix.RESULTS` is an equinox enumeration whose member name keys the receipt owner's
    # `_STATUS` vocabulary; reading `.name` keeps the gated `RESULTS` type off this owner's surface.
    return str(getattr(result, "name", result))


def _floor_cost(objective: "Objective") -> "tuple[Callable[[np.ndarray], float], Callable[[np.ndarray], float]]":
    # the `Shape`-keyed reduction the numpy floor differentiates and reports, the host mirror of
    # `Objective.cost`: `RESIDUAL` folds the differentiated `½‖r‖²` and the reported `‖r‖`, `SCALAR`
    # folds the raw scalar for both, each squeezed to a python float through `.item()` so a singleton
    # array reduces rather than crashing a `float(value)` on a non-0-d output, closed by `assert_never`.
    def raw(x: np.ndarray) -> np.ndarray:
        return np.asarray(objective.fn(x), dtype=float)

    match objective.shape:
        case Shape.RESIDUAL:
            return lambda x: 0.5 * float((raw(x) ** 2).sum()), lambda x: float(np.linalg.norm(raw(x)))
        case Shape.SCALAR:
            def scalar(x: np.ndarray) -> float:
                return float(raw(x).reshape(()).item())

            return scalar, scalar
        case _ as unreachable:
            assert_never(unreachable)


def _central_difference_norm(cost: "Callable[[np.ndarray], float]", x0: np.ndarray) -> float:
    # the L∞ central-difference stationarity probe over the raveled design buffer reshaped back into
    # `x0`'s structure per perturbation, so a non-flat node-coordinate or material-grid design (an
    # `(N, 3)`/`(H, W)` array) probes correctly rather than broadcasting a length-`x0.size` basis row
    # against a multidimensional `x0`. The basis rows fold to one `np.ndarray` probe, never a mutable
    # python accumulator, and `‖·‖∞` is the same Chebyshev stationarity norm the gated `max_norm` reads.
    steps = ((_FD * row).reshape(x0.shape) for row in np.eye(x0.size))
    probe = np.fromiter(((cost(x0 + e) - cost(x0 - e)) / (2.0 * _FD) for e in steps), dtype=float, count=x0.size)
    return float(np.linalg.norm(probe, np.inf))


def _design_key(tag: str, params: "PyTree") -> "RuntimeRail[ContentKey]":
    # backend-stable over the design leaves' raveled float64 buffer: a flat-array `params` ravels to
    # itself, a nested array-PyTree concatenates its leaves in deterministic structure order, so a
    # re-run from the same start keys identically on the gated Optimistix and the cp315 floor without
    # importing jax on the host. A non-array leaf coerces through `np.asarray` at the boundary.
    # `ContentIdentity.of` returns `RuntimeRail[ContentKey]` (the `view="value"` default), so the
    # digest fault rides the one rail the `_backend` `.map` threads into the receipt.
    leaves = params if isinstance(params, (tuple, list)) else (params,)
    buffer = np.concatenate([np.ascontiguousarray(np.asarray(leaf, dtype=float)).ravel() for leaf in leaves]).tobytes()
    return ContentIdentity.of(f"design-{tag}", buffer, IdentityPolicy())


# --- [COMPOSITION] ----------------------------------------------------------------------


@tagged_union(frozen=True)
class DesignProblem:
    tag: Literal["field", "mesh", "density"] = tag()
    field: Objective = case()
    mesh: Objective = case()
    density: Objective = case()

    @classmethod
    def Field(cls, objective: Objective) -> Self:
        return cls(field=objective)

    @classmethod
    def Mesh(cls, objective: Objective) -> Self:
        return cls(mesh=objective)

    @classmethod
    def Density(cls, objective: Objective) -> Self:
        return cls(density=objective)

    @property
    def carried(self) -> Objective:
        match self:
            case DesignProblem(tag="field", field=obj):
                return obj
            case DesignProblem(tag="mesh", mesh=obj):
                return obj
            case DesignProblem(tag="density", density=obj):
                return obj
            case _ as unreachable:
                assert_never(unreachable)


@functools.cache
def _objective() -> "FrozenDict[Shape, DesignEntry]":
    # the gated `optimistix` import is deferred to first call and the `minimise`/`least_squares`
    # entry table is cached once, so the module loads on the cp315 core without the jaxlib wheel and
    # the lookup only resolves inside the gated `_optimistix` route the import guard fences.
    import optimistix as optx

    return FrozenDict({Shape.SCALAR: optx.minimise, Shape.RESIDUAL: optx.least_squares})
```

## [03]-[RESEARCH]

- [OPTIMISTIX_DESIGN]: `optimistix` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `minimise`/`least_squares` entries, the `BFGS`/`LevenbergMarquardt` solvers, the `BestSoFarMinimiser`/`BestSoFarLeastSquares` wrappers guarding a non-monotone final iterate, the `OptaxMinimiser` optax wrapper, the `ImplicitAdjoint` default adjoint, `max_norm`, and `Solution.value`/`Solution.stats["num_steps"]`/`Solution.result` verify against `compute/.api/optimistix.md` under a uv-sync reflection pass on that band. Every Optimistix entry defaults to `ImplicitAdjoint`, so the descent differentiates the converged stationary point through one linear solve per backward pass consumed by `solvers/sensitivity.md#SENSITIVITY`, never the iteration trace; the solve runs `throw=False` so a non-`successful` `Solution.result` `RESULTS` member reaches the receipt as its mapped `SolveStatus` through the receipt owner's `_STATUS` boundary table (`successful`→`SUCCESS`, `max_steps_reached`/`nonlinear_max_steps_reached`→`MAX_STEPS`, `nonlinear_divergence`→`DIVERGENCE`, `singular`→`SINGULAR`, `breakdown`→`BREAKDOWN`, `stagnation`→`STAGNATION`, `nonfinite`→`NONFINITE`, an unmapped member→`OTHER`) rather than raising mid-fold, the design verdict thus speaking the one termination vocabulary every solver route and the graduation gate read. The `_MAX_STEPS=256` budget is the documented Optimistix default carried explicitly because a change recompiles under JIT. The `_floor` numpy central-difference fallback runs unconditionally on cp315 so every case stays reachable without the wheel, deriving the same first-order quantity through a finite-difference probe over the real `x0` buffer.
- [EQUINOX_PARAM]: the `equinox.partition`/`combine` PyTree split over `equinox.is_inexact_array` separates the inexact-array design leaves the optimizer threads from the static rest, and the combined PyTree is the `y0` Optimistix carries through iteration; the spellings verify against `compute/.api/equinox.md` once the equinox wheel resolves on the gated band. The partitioned objective compiles through `equinox.filter_jit` so the static rest skips tracing, and a `restarts>1` ensemble jitters the design leaves through `jax.random.split`/`jax.random.normal` over the seeded `jax.random.key(seed)`, threads the partitioned solve through `equinox.filter_vmap` over the leading restart axis, scores each converged start through `Objective.cost(...)[0]`, folds the best converged iterate and step count through one `jax.tree_util.tree_map` slice at `jax.numpy.argmin`, and reduces the per-start `Solution.result` to the worst-case termination through `optimistix.RESULTS.promote` (the catalogued vmap code-combine the sibling `solvers/nonlinear.md#NONLINEAR` batched path folds, never a per-leaf enumeration index nor a discarded-to-floor `result=None`) — the `key`/`split`/`normal` PRNG triple confirmed in `compute/.api/jax.md`, `RESULTS.promote` in `compute/.api/optimistix.md`, and both filter transforms in `compute/.api/equinox.md`, never a broadcast-copy restart and never a per-start optimizer surface. The descent engine is the `Descent` `@tagged_union` projected to its Optimistix/optax solver in `Descent.solver` — `optimistix.BFGS`/`LevenbergMarquardt` and `optimistix.OptaxMinimiser(optax.chain(optax.clip_by_global_norm, optax.adam(lr), *_feasible()[feasible]), ...)` all catalogued in `compute/.api/optimistix.md`/`compute/.api/optax.md`, defaulted per route through `_DEFAULT_DESCENT` and overridable per call — never a hardcoded per-case solver and never a hand-rolled momentum accumulator. The `density` route folds a feasibility projection into the chain through the `@functools.cache`-built `_feasible()` table as a chain-shaped `GradientTransformation` — the `_projected` lift wrapping an `optax.projections.projection_box`/`projection_simplex` Euclidean projection in a `GradientTransformationExtraArgs` whose `update` returns `projection(apply_updates(params, updates)) − params`, since a bare `projection_box(x, lower, upper)` callable is not a transformation an `optax.chain` composes (the catalogue documents projections as applied after `apply_updates`, with `keep_params_nonnegative` the lone in-chain box transform — line 137/84 of `compute/.api/optax.md`). The SIMP design vector thus stays feasible without a penalty term, and a heterogeneous design vector routes through `optax.multi_transform` on the same projection without a second optimizer.
- [JAX_ADJOINT]: the `equinox.filter_value_and_grad(Objective.cost, has_aux=True)` pass folds the converged objective and the first-order/KKT residual in one trace, carries the `python_version<'3.15'` marker, and verifies against `compute/.api/equinox.md`/`compute/.api/jax.md` once the jaxlib wheel resolves; it differentiates the scalar objective for the `field`/`density` `minimise` routes and the least-squares cost `½‖r‖²` for the `mesh` `least_squares` route (whose gradient is `Jᵀr`) w.r.t. the inexact-array design leaves only, returns the reported objective scalar (`‖r‖` on the `RESIDUAL` route, the raw scalar otherwise) as the differentiation aux so the objective is never re-evaluated, and the gradient PyTree's L∞ stationarity norm reads `optimistix.max_norm` directly over the PyTree (confirmed `(x: PyTree[Array]) → Shaped[Array,'']`), so the converged objective and residual are computed on-device through `jax.numpy`/filter transforms and `numpy.asarray` never touches a JAX PyTree. The single `Objective.cost` projection owns both the differentiated reduction and the reported aux, retiring the prior twin `_converged_outcome`/`_objective_scalar` helpers that each rebuilt the scalar reduction. The residual is the stationarity gradient `∇f` (respectively `Jᵀr`) that vanishes at the converged design the receipt folds and the `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads. The central-difference residual floor over an `np.eye` perturbation basis runs on the real numpy `x0` buffer only and folds the same `Shape`-keyed `(½‖r‖², ‖r‖)`-vs-`(scalar, scalar)` reduction `Objective.cost` owns through the host mirror `_floor_cost`, each `_FD`-scaled basis row reshaped back into `x0`'s structure so a non-flat `(N, 3)` node-coordinate or `(H, W)` material-grid design probes correctly rather than broadcasting a length-`x0.size` row against a multidimensional `x0`, so the floor reproduces the same first-order quantity through `np.fromiter`/`‖·‖∞` unconditionally on cp315.
