# [PY_COMPUTE_DESIGN]

`DesignProblem` is the gradient-driven inverse-design apex the autodifferentiable solver chain enables and no other owner closes. It discriminates a `Field` problem over the `solvers/mesh.md#MESH_FIELD` assembled system, a parametric-`Mesh` objective, and a material-distribution `Density` objective, driving an Equinox-partitioned PyTree objective to a stationary point through `optimistix.minimise`/`least_squares` over the JAX floor. Every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so the gradient `solvers/sensitivity.md#SENSITIVITY` pulls back is the implicit-function-theorem gradient of the converged solution, never the iteration trace.

One dispatch threads all three cases: the `@functools.cache`-built `_objective()` shape table selects the `minimise`/`least_squares` entry from a typed `Shape`, the `Descent` `@tagged_union` projects to its Optimistix/optax solver in `Descent.solver` — defaulted per route, overridable per call, the `optax` first-order engine threading through `optimistix.OptaxMinimiser(optax.chain(...))` with the `density` route folding a chain-shaped feasibility projection through the cached `_feasible()` table. Each route folds one content-keyed `OutcomeReceipt.Design` over the final iterate, the converged objective, and the first-order/KKT residual that graduates outward on the existing `solver` axis at `graduation/handoff.md#GRADUATION`.

`OutcomeReceipt` is the ONE optimization-outcome owner: the `design` convergence verdict (objective, residual, iterations, `SolveStatus`) and the `program` feasibility verdict (objective, `SolveStatus`, violation) are two cases of one `@tagged_union`, folding `optimization/program.md#PROGRAM` onto the same receipt rather than a parallel struct. Both cases carry the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns rather than a lone `bool success`, so the Optimistix `RESULTS` verdict and the host `scipy.optimize` verdict speak one vocabulary and the success contract survives as the derived `status is SolveStatus.SUCCESS` predicate. `ConvexReceipt` stays distinct because it carries the duality-gap/dual-infeasibility/status certificate the first-order and feasibility verdicts have no field for.

A numpy central-difference floor reports the gradient-norm residual at `x0` for every case, reachable on runtime behind the `_backend` import-guard, so a run without the jaxlib package never returns `Error(Import)`. This owner composes the solver, sensitivity, and assembly owners; it never re-owns a solve, never runs a training loop, and never stands a parallel optimizer surface beside the converged solve.

## [01]-[INDEX]

- [01]-[DESIGN]: field/mesh/density inverse-design over an Equinox-parameterized `Objective` through one `_objective()` shape-keyed `optimistix.minimise`/`least_squares` dispatch with the implicit-adjoint gradient, the `Descent`-projected Optimistix/optax solver, and the `_backend` optimistix-vs-floor import-guard rail, folding the `design` case of the shared `OutcomeReceipt` on one `DesignProblem` owner whose `contribute` streams the `ReceiptContributor` port.

## [02]-[DESIGN]

- Owner: `DesignProblem` — the ONE inverse-design owner; `DesignProblem` discriminates `Field(objective)` (an `Objective` over a `solvers/mesh` `AssembledSystem` stiffness/load discretization), `Mesh(objective)` (a parametric-mesh `Objective` over node coordinates), and `Density(objective)` (a material-distribution `Objective` over a SIMP-style design field), each case carrying one `Objective` value object. The provenance of the objective is the discriminant; the optimizer is one surface. The cases never become a per-problem optimizer family — `Field`, `Mesh`, and `Density` are rows on the same owner discriminated by the structure the objective integrates over, and the `carried` property folds the case to its `Objective` total over `match`/`assert_never` so a new provenance breaks the extractor rather than spawning a parallel dispatch arm.
- Objective value object: `Objective` is the frozen `Struct` carrying the cost thunk `fn`, the initial parameter PyTree `params`, and the `Shape` discriminant (`SCALAR` for the `field`/`density` minimise routes, `RESIDUAL` for the `mesh` inverse-identification least-squares route). The shape is a typed field on the `Objective`, never a stringy `frozenset({"mesh"})` membership test against the tag, so the `minimise`-vs-`least_squares` entry and the two `Shape`-keyed projections all read one `Shape` value. `Objective` owns TWO shape-keyed projections of the one `fn`, distinct because the solver and the receipt consume different reductions: `Objective.target` is the SOLVE input optimistix consumes — the raw residual VECTOR `r` on `RESIDUAL` so `least_squares`/LM owns the `½‖r‖²` reduction and the `Jᵀr` Jacobian internally, the scalar on `SCALAR` for `minimise` — never the pre-reduced `½‖r‖²` scalar that would collapse the LM Jacobian to a degenerate 1-element solve; `Objective.cost` is the RECEIPT objective the `filter_value_and_grad` folds — the `(reduced, reported)` pair the `Shape` selects, `(½‖r‖², ‖r‖)` on `RESIDUAL` (so `∇(½‖r‖²) = Jᵀr` is the converged-design stationarity gradient) and `(scalar, scalar)` on `SCALAR`, the reported scalar riding as the value-and-grad aux so it is never a re-traced second pass. Both retire the prior `_converged_outcome`/`_objective_scalar` twin helpers that each rebuilt a reduction; conflating the two projections into one (feeding the receipt's `½‖r‖²` to `least_squares`) is the deleted form the split forecloses.
- Parameterization: the `Objective.params` PyTree is partitioned through `equinox.partition`/`combine` over `equinox.is_inexact_array` into the inexact-array design leaves the optimizer threads and the static rest; `optimistix.minimise` carries the combined PyTree through iteration and the implicit adjoint differentiates the converged stationary point. The descent engine is not hardcoded per case: `Descent` is the `@tagged_union` vocabulary — `QuasiNewton` (`optimistix.BFGS`), `Levenberg` (`optimistix.LevenbergMarquardt`), and `FirstOrder` (`optimistix.OptaxMinimiser` over an `optax.chain`) — so the engine is a policy value carried on `DesignProblem.solve`, defaulting per route through the `_DEFAULT_DESCENT` table (`field`→`QuasiNewton`, `mesh`→`Levenberg`, `density`→`FirstOrder`) and overridable per call. The `Descent.admits(shape)` invariant gates an override — `Levenberg` requires the `RESIDUAL` least-squares route, the scalar minimisers require `SCALAR` — so a mismatched engine (an LM least-squares solver on a scalar objective) mints a typed `Error(BoundaryFault(boundary=...))` onto the rail rather than reaching the wrong solve entry. The `Descent.solver` projection builds the Optimistix solver from the policy and wraps it in the matching `optimistix.BestSoFarMinimiser`/`BestSoFarLeastSquares` so a non-monotone final iterate never poisons the converged design, so a new descent engine is a new `Descent` case mapping to its Optimistix/optax constructor, never a hand-rolled momentum loop and never a parallel descent surface.
- Feasibility: the `FirstOrder` descent carries an optional `Feasible` projection row (`BOX` for densities ∈ [0,1], `SIMPLEX` for a material-fraction simplex, `NONNEGATIVE` for a sign constraint, `FREE` for none) folded into the `optax.chain` through the cached `_feasible()` table as a chain-shaped `GradientTransformation`, never a bare `projection_box(x, lower, upper)` callable a `chain` cannot compose. The `_projected` lift wraps an `optax.projections` Euclidean projection in a stateless `GradientTransformationExtraArgs` whose `update` returns the corrected delta `projection(apply_updates(params, updates)) − params` over `optax.tree_utils.tree_add`/`tree_scale`, so the next iterate lands on the feasible set inside the chain rather than in a post-`apply_updates` body call the `OptaxMinimiser` solve has no seam for; `NONNEGATIVE` folds the catalogued stateful `optax.keep_params_nonnegative` transform directly. A SIMP density descent thus keeps its design vector feasible through projected gradient descent without a penalty term and without a parallel constrained optimizer — the `density` default is `FirstOrder(feasible=Feasible.BOX)`. The chain leads with the `optax.zero_nans()` + `clip_by_global_norm(_CLIP)` diverged-inner-solve guard the `optax` robustness law names — `zero_nans` neutralizing a NaN gradient the clip cannot bound and the global-norm clip bounding the step — so a non-finite gradient from a diverged inner `optimistix`/`diffrax` solve never poisons the design step. A heterogeneous design vector descending under per-block policy routes through `optax.multi_transform` on the same `Descent.solver` projection, never a second optimizer beside the chain.
- Compilation: the partitioned objective `fn(y, args)` is wrapped once through `equinox.filter_jit` so the static rest skips tracing and the inexact-array design leaves are the only XLA-traced inputs — this is the same compile surface `solvers/nonlinear.md#NONLINEAR` threads through Optimistix, composed here, never re-derived. A `restarts>1` multi-start jitters `y0` across a leading restart axis through `jax.random.split`/`jax.random.normal` over the seeded PRNG key — each start is a distinct perturbed design, not a broadcast copy — and threads the partitioned solve through `equinox.filter_vmap` over that axis, returning the per-start converged values from which the route folds the best objective through one `jax.tree_util.tree_map` slice at `jax.numpy.argmin`; the ensemble is one `filter_vmap` row on the same `solve` owner, never a parallel multi-start optimizer surface.
- Adjoint: every Optimistix entry carries the default `optimistix.ImplicitAdjoint`, so a sensitivity through a design that itself solves an inner system differentiates the converged solution through one linear solve per backward pass rather than backpropagating the iterations; `solvers/sensitivity.md#SENSITIVITY` reads that adjoint through `jax.vjp` over the objective, and `solvers/linear.md#LINEAR`/`solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` expose the autodifferentiable inner solves the `Field` objective composes (the quadrature weak-form assembly enters transitively through `solvers/mesh`, never as a direct dependency here). The converged objective and the first-order/KKT residual fold from one `equinox.filter_value_and_grad(Objective.cost, has_aux=True)` pass over the partitioned cost — value and gradient w.r.t. the inexact-array design leaves in a single trace, the reported objective scalar carried as the differentiation aux so it is never re-evaluated, never a separate objective evaluation and a re-traced gradient — so the residual norm is the gradient PyTree's L∞ stationarity norm through `optimistix.max_norm` directly over the PyTree, never by routing the converged PyTree through `numpy.asarray`.
- Entry: `DesignProblem.solve` enters one `boundary(f"design.{problem.tag}", ...).bind(lambda r: r)`, so the solve fence and the `_design_key` `RuntimeRail[ContentKey]` digest rail join on one `RuntimeRail[OutcomeReceipt]` without double-wrapping — the same rail-join shape the sibling `optimization/program.md#PROGRAM` `solve` holds. The body resolves the `Descent` policy (caller override or the `_DEFAULT_DESCENT` per-tag default), gates it through `Descent.admits(problem.carried.shape)` so a mismatched override mints a typed `Error(BoundaryFault(boundary=...))` directly onto the rail — never a `raise` relying on the fence to re-classify, the ROP form the sibling `program.md` `Termination` adjudication holds — rather than reaching the wrong solve entry, dispatches the `_backend_outcome` import-guard (`_optimistix` tries the gated Optimistix route, falling to `_floor`, the numpy central-difference floor, on `ImportError`) into the deferred `Callable[[ContentKey], OutcomeReceipt]` receipt builder, and `Result.map`s the railed `_design_key` digest into it so a digest fault rides the one rail rather than collapsing to a phantom bare `ContentKey`. The `minimise`-vs-`least_squares` entry is the `_objective()[shape]` row the `Objective.shape` selects (the `@functools.cache`-built table deferring the gated `optimistix` import past module load), the route is the `Descent.solver()` projection plus that entry, the solve runs `throw=False` so a non-`successful` `Solution.result` reaches the receipt as its mapped `SolveStatus` rather than raising, and the converged objective and residual fold from one `filter_value_and_grad`. The `_design_key` digest over the canonical objective-parameter buffer — the per-leaf ordinal-and-shape signature and the iterate-determining `descent`/`restarts`/`seed` policy folded into the `fmt` exactly as `program.md`'s `_program_key` folds its slot/shape plus seeded `_engine_tag` discriminant — keys a design re-run from the same starting point under the same engine and ensemble identically across backends, so a single raveled buffer shared by structurally distinct PyTrees (a `(4,)` array vs a tuple of two `(2,)` leaves) or a re-solve under a different engine/restart/seed never collides on the boundary-erasing flatten.
- Receipt: `OutcomeReceipt` is the ONE optimization-outcome receipt this owner and `optimization/program.md#PROGRAM` share — `OutcomeReceipt.Design` folds the design convergence verdict and `OutcomeReceipt.Program` folds the math-program feasibility verdict on the same `@tagged_union`, both minted through the `@classmethod`-plus-`Self` form that binds the subtype once (never a `@staticmethod` over a `"OutcomeReceipt"` forward-ref string). The case payloads are owned by one `_OUTCOME_SLOTS` field-name table — the same data-driven projection the sibling `solvers/receipt.md#RECEIPT` `SolverReceipt._SLOTS` establishes — so `.facts`, `.status`, and `.content_key` are each one total `match self` fold closed by `assert_never`, the `.facts` arm zipping `_OUTCOME_SLOTS[tag]` against the matched case's destructured payload under `zip(..., strict=True)` rather than a reflective `getattr(self, self.tag)` whose `object` residual makes the `assert_never` tail a lie — the closed-union law the sibling `SolverReceipt.facts` and `reliability/faults#FAULT` `BoundaryFault.facts` both hold against the getattr escape. `.contribute` spreads that one fold into a single `Receipt.of(f"compute.optimization.{self.tag}", ("emitted", self.tag, facts))` row rather than a per-case hand-spelled `dict`, the tag riding as the receipt `subject` the runtime `project` binds; its facts carry the full per-case numeric evidence (the problem/program name, the converged objective, the first-order/KKT residual, the iteration count, the `SolveStatus`, the constraint violation) with `key` lowered to `ContentKey.hex` and the derived `converged` injected. The `strict=True` zip makes the slot row and the case-tuple length structurally inseparable — a row that drifts from its payload raises rather than truncating evidence. Both cases carry `SolveStatus` (`solvers/receipt.md#RECEIPT`): the `design` case folds the `optimistix.Solution.result` `RESULTS` member name through the sibling `_status(adjudicated, residual, tol)` polymorphic verdict — the one fold every solver route shares, mapping a backend member name through `_STATUS` and grading the no-adjudicator floor against tolerance — the `program` case carries the `scipy.optimize` verdict, so both speak one termination vocabulary the C# graduation gate reads. The facts ride as native scalars — the objective and residual as `float`, the iteration count as `int`, the status as the `SolveStatus` `StrEnum` member, the key through `ContentKey.hex` — into the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` its `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce, never a pre-`f""`-formatted `dict[str, str]`. The `Program` factory and the program-route body that maps the `OptimizeResult` into that vocabulary live on `optimization/program.md#PROGRAM`. The residual is the convergence evidence the C# graduation gate reads through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` to admit or reject a converged design for kernel handoff — no new handoff axis and no graduation edit. `ConvexReceipt` (`optimization/convex.md#CONVEX`) stays a distinct struct: its duality-gap, dual-infeasibility, and solver-status certificate is the global-optimality proof object the first-order and feasibility verdicts carry no field for, so folding it onto `OutcomeReceipt` would erase the spectral/convergence certificate.
- Egress: `OutcomeReceipt` implements the `observability/receipts#RECEIPT` `ReceiptContributor` port through `contribute`, so the converged outcome streams its `Receipt` sequence into the one egress fold the consumer drives — `Signals.emit`, or the cross-cutting `@receipted` aspect over a measured study kernel that returns the receipt as a `ReceiptContributor`. `DesignProblem.solve` returns the `RuntimeRail[OutcomeReceipt]` and threads no inline `emit`: the solve fence and the receipt egress stay orthogonal, `boundary` mints the rail, the receipt owns its projection, and a consumer drives graduation through `GraduationReceipt.graduates` on the `solver` axis.
- Packages: `optimistix` (`minimise`, `least_squares`, `OptaxMinimiser`, `BFGS`, `LevenbergMarquardt`, `BestSoFarMinimiser`, `BestSoFarLeastSquares`, `ImplicitAdjoint`, `Solution`, `RESULTS` — the `equinox.Enumeration` whose `EnumerationItem._value` integer code the multi-start ensemble reduces by `jnp.max` (zero `successful` code iff every start converged) and whose code→member-name is recovered by inverting the class `RESULTS._name_to_item` in `_result_names()`, since an item carries no `.name` and `RESULTS[item]` yields the human message not the name; `RESULTS.promote` is NOT a batch combine and is deliberately unused, `max_norm` — the Chebyshev stationarity norm over the gradient PyTree; the resolved member name folds into `SolveStatus` and `throw=False` defers the non-success verdict to the receipt), `equinox` (`partition`, `combine`, `is_inexact_array`, `filter_jit`, `filter_vmap`, `filter_value_and_grad` — the partition/compile/vectorize/combined-grad filter transforms threading the inexact-array design leaves, the value-and-grad-with-aux fold the converged objective and residual share), `jax` (`numpy` the on-device reduction namespace; `random.key`/`random.split`/`random.normal` the seeded multi-start jitter; `tree_util.tree_map` the per-start value slice at `jax.numpy.argmin`; the autodiff floor the objective and the adjoint resolve over), `optax` (`adam`, `sgd`, `chain`, `multi_transform`, `zero_nans`/`clip_by_global_norm` — the diverged-inner-solve guard at the chain head neutralizing a NaN gradient then bounding the step, `GradientTransformationExtraArgs`/`EmptyState`/`apply_updates`/`keep_params_nonnegative`/`tree_utils.tree_add`/`tree_utils.tree_scale` — the chain-shaped `_projected` lift folding an `optax.projections.projection_box`/`projection_simplex`/`projection_non_negative` Euclidean projection into the `density` chain as a `GradientTransformation`, the first-order-descent engine threaded through `OptaxMinimiser`), `numpy` (`asarray`, `ascontiguousarray`, `concatenate`, `split`, `cumsum`, `zeros`, `fromiter`, `linalg.norm` — the host-side central-difference residual floor over real arrays only: the `_ravel` pure-numpy flatten/restore (`concatenate`/`split`/`cumsum` the leaf flatten and shape-restore the host mirror of `jax.flatten_util.ravel_pytree`), the `Shape`-keyed `_floor_cost` reduction over the structured `unravel`, the `np.zeros` one-hot directional perturbation (never a dense `np.eye(x0.size)` basis), and the `np.fromiter` `‖·‖∞` probe fold, never over a JAX PyTree, and the canonical raveled key buffer), `solvers/receipt.md#RECEIPT` (`SolveStatus` the shared `design`/`program` cases carry, and `_status` the one polymorphic verdict fold the gated route and the numpy floor both adjudicate through — mapping a backend `RESULTS` member name through the owner's `_STATUS` table or grading the floor residual against tolerance to `NONFINITE`/`SUCCESS`/`STAGNATION`), `solvers/sensitivity.md#SENSITIVITY` (the implicit-adjoint gradient read through the converged solve), `solvers/{linear,nonlinear,differential}.md` (the autodifferentiable inner solves the `Field` objective composes), `solvers/mesh.md#MESH_FIELD` (`AssembledSystem` stiffness/load for the field objective), `graduation/handoff.md#GRADUATION` (the `solver` axis the converged design graduates on), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor` the `OutcomeReceipt` contributes through, the `@receipted` aspect and `Signals.emit` available to the consuming study kernel).
- Growth: a new design provenance is one `DesignProblem` case plus one `_DEFAULT_DESCENT` row; a new objective shape is one `Shape` member plus one `_objective()` row, one `Objective.target` solve-input arm, one `Objective.cost` receipt-objective arm, and its `_floor_cost` host-mirror arm (all `assert_never`-closed so the addition is compile-surfaced); a new descent engine is one `Descent` case mapping to its Optimistix/optax constructor in `Descent.solver` (a `chain` of `optax` transformations, an `optimistix.LBFGS`/`NonlinearCG`, or a trust-region least-squares solver), selectable per call without a body edit; a new feasibility constraint is one `Feasible` member plus one `_feasible()` projection row; a new outcome-receipt evidence field is one `_OUTCOME_SLOTS` slot plus its case-tuple position with no `contribute` edit, the slot table projecting it for free; a perturbed multi-start ensemble is the seeded `filter_vmap` restart axis already on `solve`; zero new surface, never a parallel field-design and density-design owner, never a per-case helper body, never a per-case fact dict beside the `_OUTCOME_SLOTS` projection, never a training loop.

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

from rasm.compute.solvers.receipt import SolveStatus, _status
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # worker annotation carriers only; no package imports at runtime
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

# Per-case payload field names, one tuple per `OutcomeReceipt` tag, the problem/program name in slot 0
# and the shared trailing `key` slot common to both rows; the `design` verdict sits in slot 4, the
# `program` verdict in slot 2. The ONE owner over the case shapes the sibling `SolverReceipt._SLOTS`
# establishes: the `.facts` total `match`-zip packs each case's destructured payload by its row under
# `strict=True`, so a case's evidence is one row, never a per-case hand-spelled fact dict.
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

    def target(self, y: "PyTree") -> "jax.Array":
        # the SOLVE target optimistix consumes, shape-correct per entry: `least_squares` owns the
        # ½‖r‖² reduction and the Jᵀr Jacobian INTERNALLY, so the `RESIDUAL` route must feed it the raw
        # residual VECTOR `r` — feeding the pre-reduced scalar ½‖r‖² would collapse LM/Gauss-Newton to
        # a 1-element problem minimising ½(½‖r‖²)², the wrong solve; the `SCALAR` route feeds `minimise`
        # the scalar objective. This is distinct from `cost` (the receipt objective the value-and-grad
        # folds) because the solver and the receipt consume different reductions of the same `fn`.
        import jax.numpy as jnp

        value = jnp.asarray(self.fn(y))
        match self.shape:
            case Shape.RESIDUAL:
                return value
            case Shape.SCALAR:
                return value.reshape(())
            case _ as unreachable:
                assert_never(unreachable)

    def cost(self, y: "PyTree") -> "tuple[jax.Array, jax.Array]":
        # the RECEIPT objective projection, distinct from the solve `target`: the differentiated
        # reduction and the reported objective scalar fold from one definition — ½‖r‖² for the
        # stationarity gradient Jᵀr on the residual route (`max_norm(∇(½‖r‖²)) = max_norm(Jᵀr)` the
        # converged-design residual), the raw scalar otherwise — carried as the `filter_value_and_grad`
        # aux so the reported scalar is never a re-traced second pass.
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
    def facts(self) -> "dict[str, str | float | int | SolveStatus]":
        # total `match self` zip (the sibling `SolverReceipt.facts` shape), never a `getattr(self,
        # self.tag)` escape whose `object` residual makes `assert_never` a lie; `key` lowers to native
        # `ContentKey.hex` at the source so the projection carries only renderer-native scalars.
        match self:
            case OutcomeReceipt(tag="design", design=(*lead, key)):
                return dict(zip(_OUTCOME_SLOTS["design"], (*lead, key.hex), strict=True))
            case OutcomeReceipt(tag="program", program=(*lead, key)):
                return dict(zip(_OUTCOME_SLOTS["program"], (*lead, key.hex), strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        # the two-argument `Receipt.of(owner, (Phase, subject, facts))` contract; the one-element
        # `Iterable[Receipt]` the port declares (a concrete one-element tuple satisfies it), tag as `subject`.
        facts: dict[str, object] = {"converged": self.converged, **self.facts}
        return (Receipt.of(f"compute.optimization.{self.tag}", ("emitted", self.tag, facts)),)


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
                # `zero_nans` -> `clip_by_global_norm` -> `adam` -> feasibility projection: the complete
                # diverged-inner-solve guard the `optax` robustness law names — `zero_nans` neutralizes a
                # NaN gradient a clip cannot bound, the global-norm clip bounds the step magnitude, and the
                # `_feasible()` projection lands the next iterate on the feasible set as the chain tail.
                chain = optax.chain(optax.zero_nans(), optax.clip_by_global_norm(_CLIP), optax.adam(learning_rate), *_feasible()[feasible])
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
        candidate = optax.apply_updates(params, updates)
        corrected = optax.tree_utils.tree_add(projection(candidate), optax.tree_utils.tree_scale(-1.0, params))
        return corrected, state

    return optax.GradientTransformationExtraArgs(init, update)


@functools.cache
def _feasible() -> "FrozenDict[Feasible, tuple[optax.GradientTransformation, ...]]":
    # no jaxlib/optax admission resolves — the `_floor` path stays reachable. `@functools.cache` builds
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


@functools.cache
def _result_names() -> FrozenDict[int, str]:
    # the integer-code -> member-name inversion of the gated `optimistix.RESULTS` `Enumeration`,
    # exposes only `_value` (the code) and `_enumeration` (the class); the member NAME the sibling
    # `_status` `_STATUS` table keys on is recoverable ONLY by inverting the class `_name_to_item`
    # name->item map (`RESULTS[item]` yields the human message, not the name), so the gated route maps
    # `int(solution.result._value)` to `successful`/`max_steps_reached`/`nonlinear_divergence`/... here
    # rather than reading a non-existent `.name` off the item.
    import optimistix as optx

    return FrozenDict({int(item._value): name for name, item in optx.RESULTS._name_to_item.items()})


# --- [OPERATIONS] -----------------------------------------------------------------------


def solve(problem: "DesignProblem", /, *, descent: "Descent | None" = None, restarts: int = 1, seed: int = _SEED) -> "RuntimeRail[OutcomeReceipt]":
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
    objective = problem.carried
    railed = _backend_outcome(problem.tag, objective, descent, restarts, seed)
    return _design_key(problem.tag, objective.params, descent, restarts, seed).map(railed)


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
        # the SOLVE target, shape-correct per entry: the residual VECTOR for `least_squares` (which owns
        # the ½‖r‖²/Jᵀr reduction), the scalar for `minimise` — never `cost(...)[0]`, whose pre-reduced
        # ½‖r‖² scalar would degenerate the LM least-squares Jacobian.
        return objective.target(eqx.combine(y, static))

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
        # the ensemble verdict folds the batched `EnumerationItem._value` codes by `jnp.max`: the sole
        # `successful = 0` zero code makes `max == 0` iff EVERY start converged, so a partial-failure
        # ensemble surfaces a non-success code rather than masking a diverged start as `SUCCESS` (the
        # load-bearing invariant — the specific non-zero code is a representative failure, the codes
        # carry no severity total-order). `RESULTS.promote` is NOT a batch combine — it widens a member
        # from a parent `Enumeration` to a subclass and raises `ValueError` on a same-class member — so
        # the reduction is the code max, never `promote(solution.result)`, the code mapped to its name
        # through the one `_result_names()` inversion.
        code = int(jnp.max(solution.result._value))
    else:
        solution = run(design)
        converged = eqx.combine(solution.value, static)
        steps = int(solution.stats["num_steps"])
        code = int(solution.result._value)

    # the converged objective and the L∞ stationarity residual fold from one value-and-grad-with-aux
    # pass; the receipt builder is deferred over the railed `ContentKey` the `.map` in `_backend`
    # supplies, so the gated solve evaluates eagerly inside the import guard while the key stays railed.
    # The verdict folds through the sibling `_status` (the backend `RESULTS` member NAME as the
    # adjudicator), so the gated design route and the numpy floor read the one polymorphic verdict
    # every solver route folds rather than re-inlining the `_STATUS.get(..., OTHER)` lookup here.
    (_, reported), gradient = eqx.filter_value_and_grad(objective.cost, has_aux=True)(converged)
    objective_value, residual = float(reported), float(optx.max_norm(gradient))
    # `solution.result` is an `optimistix.RESULTS` `EnumerationItem` carrying ONLY an integer-code
    # array and the enum class — no `.name`/`.value` member-name attribute, and `RESULTS[item]` yields
    # the human MESSAGE, not the member name the `_STATUS` table keys. `_result_names()` inverts the
    # `RESULTS._name_to_item` code→name map once so the converged code resolves to its member name
    # (`successful`/`max_steps_reached`/`nonlinear_divergence`/...) the sibling `_status` adjudicates;
    # an unmapped code degrades to "" (a present `str`, so `_status` lands `OTHER` through the
    # `case str()` arm rather than the `case None` residual floor a `None` would wrongly trip).
    status = _status(_result_names().get(code, ""), residual, _TOL)
    return lambda key: OutcomeReceipt.Design(tag, objective_value, residual, steps, status, key)


def _floor(tag: str, objective: "Objective") -> "Callable[[ContentKey], OutcomeReceipt]":
    # the floor reduces the same `Objective.shape`-keyed `(reduced, reported)` split as `Objective.cost`
    # over the real numpy buffer through the host mirror `_floor_cost`, so the floor verdict and the
    # gated verdict read one reduction: `cost` is the differentiated form the probe folds, `reported`
    # the reported scalar the receipt carries. `params` is the SAME general design PyTree the gated
    # `eqx.partition` splits — a single array, OR a tuple/list of array leaves — so the floor ravels it
    # to one flat host buffer through `_ravel` (never `np.asarray(params)`, which silently stacks a
    # tuple of equal-shaped leaves into one wrong-rank array and crashes on a ragged/dict PyTree exactly
    # the captured `unravel` before each `objective.fn` call.
    x0, unravel = _ravel(objective.params)
    cost, reported = _floor_cost(objective, unravel)
    residual = _central_difference_norm(cost, x0)
    status = _status(None, residual, _TOL)  # the no-adjudicator floor: `_status` grades NONFINITE/SUCCESS/STAGNATION
    return lambda key: OutcomeReceipt.Design(tag, reported(x0), residual, 0, status, key)


def _floor_cost(
    objective: "Objective", unravel: "Callable[[np.ndarray], PyTree]"
) -> "tuple[Callable[[np.ndarray], float], Callable[[np.ndarray], float]]":
    # the `Shape`-keyed reduction the numpy floor differentiates and reports, the host mirror of
    # `Objective.cost`: `RESIDUAL` folds the differentiated `½‖r‖²` and the reported `‖r‖`, `SCALAR`
    # folds the raw scalar for both, each squeezed to a python float through `.item()` so a singleton
    # array reduces rather than crashing a `float(value)` on a non-0-d output, closed by `assert_never`.
    # `raw` runs the flat probe buffer back through `unravel` so `objective.fn` receives the structured
    # design PyTree it is typed over, never the raw flat buffer that would mis-shape a multi-leaf design.
    def raw(flat: np.ndarray) -> np.ndarray:
        return np.asarray(objective.fn(unravel(flat)), dtype=float)

    match objective.shape:
        case Shape.RESIDUAL:
            return lambda flat: 0.5 * float((raw(flat) ** 2).sum()), lambda flat: float(np.linalg.norm(raw(flat)))
        case Shape.SCALAR:
            def scalar(flat: np.ndarray) -> float:
                return float(raw(flat).reshape(()).item())

            return scalar, scalar
        case _ as unreachable:
            assert_never(unreachable)


def _central_difference_norm(cost: "Callable[[np.ndarray], float]", x0: np.ndarray) -> float:
    # the L∞ central-difference stationarity probe over the FLAT raveled design buffer: each coordinate
    # is perturbed by a `_FD` one-hot through `x0.at`-style indexed copies (`np.zeros` + single-index
    # write), never a materialized dense `np.eye(x0.size)` basis whose O(n²) allocation a realistic SIMP
    # density field (10⁴–10⁶ cells) cannot afford. The structured PyTree is restored inside `cost` via
    # the captured `unravel`, so the probe stays one flat `np.ndarray` fold — never a mutable python
    # accumulator — and `‖·‖∞` is the same Chebyshev stationarity norm the gated `max_norm` reads.
    def directional(i: int) -> float:
        e = np.zeros(x0.size, dtype=float)
        e[i] = _FD
        return (cost(x0 + e) - cost(x0 - e)) / (2.0 * _FD)

    probe = np.fromiter((directional(i) for i in range(x0.size)), dtype=float, count=x0.size)
    return float(np.linalg.norm(probe, np.inf))


def _ravel(params: "PyTree") -> "tuple[np.ndarray, Callable[[np.ndarray], PyTree]]":
    # `jax.flatten_util.ravel_pytree` (which would pull the gated jaxlib package): a single-array `params`
    # ravels to its flat float64 buffer and `unravel` reshapes it back; a tuple/list PyTree concatenates
    # its leaves in deterministic structure order (the SAME order `_design_key` keys over) and `unravel`
    # splits by the captured leaf sizes and reshapes each, rebuilding the original container — so
    # `objective.fn` receives the structured design it is typed over and the probe runs over one flat
    # buffer. A non-array leaf coerces through `np.asarray` at the boundary.
    single = not isinstance(params, (tuple, list))
    leaves = [np.ascontiguousarray(np.asarray(leaf, dtype=float)) for leaf in ((params,) if single else params)]
    shapes = [leaf.shape for leaf in leaves]
    splits = np.cumsum([leaf.size for leaf in leaves])[:-1]

    def unravel(flat: np.ndarray) -> "PyTree":
        parts = [chunk.reshape(shape) for chunk, shape in zip(np.split(flat, splits), shapes, strict=True)]
        return parts[0] if single else type(params)(parts)

    return np.concatenate([leaf.ravel() for leaf in leaves]) if leaves else np.zeros(0), unravel


def _design_key(tag: str, params: "PyTree", descent: "Descent", restarts: int, seed: int) -> "RuntimeRail[ContentKey]":
    # backend-stable over the design leaves' raveled float64 buffer: the SAME `_ravel` flatten the
    # the host floor without importing jax. `ContentIdentity.of` returns `RuntimeRail[ContentKey]` (the
    # `view="value"` default), so the digest fault rides the one rail the `_backend` `.map` threads.
    #
    # the SAME slot/shape discriminant `program.md`'s `_program_key` adopts: `_ravel` concatenates the
    # design leaves with NO boundary delimiter, so a single `(4,)` array and a tuple of two `(2,)` leaves
    # both flatten to one byte-identical buffer and a bare `design-{tag}` fmt would key them identically
    # for STRUCTURALLY distinct designs; folding each leaf's ordinal and shape into the fmt distinguishes
    # the PyTree structure the bare flatten erases. The iterate-determining policy folds beside it exactly
    # as `program.md` folds its seeded `rng`/`_engine_tag` suffix: a multi-start ensemble (`restarts>1`)
    # jitters `y0` off `seed` and selects the best converged start, so the converged design is BOTH
    # `seed`- and `restarts`-dependent, and the `Descent` engine selects the converged stationary point,
    # so two solves on identical `params` under a different engine/restart/seed key DISTINCTLY rather than
    # a cache hit returning the wrong converged design (a single deterministic solve folds `restarts=1`).
    leaves = [np.asarray(leaf, dtype=float) for leaf in ((params,) if not isinstance(params, (tuple, list)) else params)]
    shape_tag = "".join(f".{i}:{leaf.ndim}x{'x'.join(map(str, leaf.shape))}" for i, leaf in enumerate(leaves))
    policy_tag = f".{descent.tag}.r{restarts}" + (f".s{seed}" if restarts > 1 else "")
    buffer = _ravel(params)[0].tobytes()
    return ContentIdentity.of(f"design-{tag}{shape_tag}{policy_tag}", buffer, IdentityPolicy())


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
    # the lookup only resolves inside the gated `_optimistix` route the import guard fences.
    import optimistix as optx

    return FrozenDict({Shape.SCALAR: optx.minimise, Shape.RESIDUAL: optx.least_squares})
```

## [03]-[RESEARCH]

- [EQUINOX_PARAM]: the `equinox.partition`/`combine` PyTree split over `equinox.is_inexact_array` separates the inexact-array design leaves the optimizer threads from the static rest, and the combined PyTree is the `y0` Optimistix carries through iteration; the spellings verify against `compute/.api/equinox.md` once the equinox package resolves on the worker lane. The partitioned objective compiles through `equinox.filter_jit` so the static rest skips tracing, and a `restarts>1` ensemble jitters the design leaves through `jax.random.split`/`jax.random.normal` over the seeded `jax.random.key(seed)`, threads the partitioned solve through `equinox.filter_vmap` over the leading restart axis, scores each converged start through `Objective.cost(...)[0]`, folds the best converged iterate and step count through one `jax.tree_util.tree_map` slice at `jax.numpy.argmin`, and reduces the per-start verdicts by `jnp.max` over the batched `optimistix.RESULTS` `EnumerationItem._value` codes — the zero `successful` code makes `max == 0` iff every start converged, so a partial-failure ensemble never masks a diverged start as `SUCCESS`, the specific non-zero code a representative failure since the codes carry no severity total-order. `optimistix.RESULTS.promote` is NOT a batch code-combine: it widens a member from a parent `Enumeration` to a subclass and raises `ValueError` on a same-class member, so the documented vmap reduction is the integer-code max plus the code→member-name inversion of `RESULTS._name_to_item` (an `EnumerationItem` carries only `_value`/`_enumeration`, no `.name`, and `RESULTS[item]` yields the human message not the member-name key the `_STATUS` table reads), never `promote(solution.result)` and never a discarded-to-floor `result=None` — the `key`/`split`/`normal` PRNG triple confirmed in `compute/.api/jax.md`, the `RESULTS` enumeration and `max_norm` in `compute/.api/optimistix.md`, and both filter transforms in `compute/.api/equinox.md`, never a broadcast-copy restart and never a per-start optimizer surface. The descent engine is the `Descent` `@tagged_union` projected to its Optimistix/optax solver in `Descent.solver` — `optimistix.BFGS`/`LevenbergMarquardt` and `optimistix.OptaxMinimiser(optax.chain(optax.zero_nans(), optax.clip_by_global_norm(_CLIP), optax.adam(lr), *_feasible()[feasible]), ...)` all catalogued in `compute/.api/optimistix.md`/`compute/.api/optax.md`, defaulted per route through `_DEFAULT_DESCENT` and overridable per call — never a hardcoded per-case solver and never a hand-rolled momentum accumulator. The chain head folds the catalogued `zero_nans`+`clip_by_global_norm` diverged-inner-solve robustness guard (`compute/.api/optax.md` `[INTEGRATION_LAW]` robustness row) so a non-finite gradient from a diverged inner solve is neutralized then bounded before the moment rescaling. The `density` route folds a feasibility projection into the chain through the `@functools.cache`-built `_feasible()` table as a chain-shaped `GradientTransformation` — the `_projected` lift wrapping an `optax.projections.projection_box`/`projection_simplex` Euclidean projection in a `GradientTransformationExtraArgs` whose `update` returns `projection(apply_updates(params, updates)) − params`, since a bare `projection_box(x, lower, upper)` callable is not a transformation an `optax.chain` composes (the catalogue documents projections as applied after `apply_updates`, with `keep_params_nonnegative` the lone in-chain box transform — line 137/84 of `compute/.api/optax.md`). The SIMP design vector thus stays feasible without a penalty term, and a heterogeneous design vector routes through `optax.multi_transform` on the same projection without a second optimizer.
