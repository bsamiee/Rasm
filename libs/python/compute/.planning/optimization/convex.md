# [PY_COMPUTE_CONVEX]

The dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval.md#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in (linear/affine, quadratic, second-order-cone, exponential-cone, semidefinite), each built from `cvxpy` `Variable`/`Parameter` leaves under a `Sense`-rowed `Minimize`/`Maximize` objective and the relational cone algebra (`==`, `<=`, `>=`, `>>`), compiled to standard conic form and solved through the Clarabel interior-point backend that returns the primal optimum and the per-constraint dual multipliers. The five cases share **one shared `_assemble` body plus a `_CONE_ROWS` data table** — the per-cone objective expression and the extra cone rows are a `ConeRow` cell, not five parallel solver bodies — so adding a cone family is one table row, never a new arm. The objective sense is a `Sense` policy row threaded through the `_SENSE` constructor table; a parametrized family threads `cp.Parameter` leaves so a sweep binds `Parameter.value` and warm-re-`solve`s the one compiled DPP `Problem` across the bind table rather than rebuilding it.

`ConvexReceipt` folds one content-keyed certificate over `Problem.value`, every `Constraint.dual_value`, the complementary-slackness KKT gap `Σᵢ slackᵢ` summed per constraint (the orthant's separable `Σ|λⱼ·gⱼ|`, the SOC/PSD non-separable `|⟨λ, g⟩|`) recovered from those duals and the per-cone primal value, the cone-aware dual-feasibility residual `dist(λᵢ, K*ᵢ)` scored per constraint against its own dual cone `K*ᵢ`, and the matching primal-feasibility residual `dist(gᵢ(x), Kᵢ)` scored against the constraint's own primal cone `Kᵢ` — all through the one `_CONE_KKT` table — collapsed into one `ConvexEvidence` value object that owns its own `facts()` projection. Every slot is recomputed from a catalogued cvxpy quantity through the cone table: the universal `Constraint.dual_value`/`Variable.value`, and the per-cone primal value the cell's `expr` closure reads off the universal `Constraint.args` (since `SOC(t, X)`/`ExpCone` carry no single `.expr`, only the relational `Inequality` does). The projection rides native `float`/`bool` scalars into the `observability/receipts#RECEIPT` `EventDict` the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes, never a pre-`f""`-formatted `dict[str, str]`, and the content key renders through `ContentKey.hex`. The `Problem.status` string folds into the shared `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns through the `_CONVEX_STATUS` boundary table. The full KKT triple is the proof object: a vanishing complementary-slackness gap with a primal-feasible iterate and dual-feasible multipliers certifies the returned point is the global optimum, the convex sibling of the `Enclosure.certified` flag, so a returned point whose gap, primal infeasibility, OR dual infeasibility exceeds the tolerance — `certified` gating `max(astuple(evidence)) <= _TOL`, not the gap alone — is an admission rejection rather than a degraded answer.

`ConvexReceipt` stays a **distinct** typed receipt and never folds into the `OutcomeReceipt` the `design`/`program` siblings share: the duality-gap, dual-infeasibility, and `SolveStatus` certificate is the global-optimality proof object the first-order convergence verdict and the feasibility verdict carry no field for, so collapsing it would erase the spectral certificate the route-owned-receipt law preserves. The coherence with `solvers/receipt.md#RECEIPT` is the **status vocabulary**, not the carrier — the convex status reads through the one `SolveStatus` enum the C# graduation gate and the sibling solve receipts read, mapped from the cvxpy status constants through the same boundary-table fold the solver routes use for the `RESULTS` enums. Like `optimization/program.md#PROGRAM`, the convex solve *is* `cvxpy` over the conic backend, so a runtime run without the package returns `Error(Import)` rather than an uncertified estimate. The certified optimum graduates outward on the dedicated `convex_program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`, a distinct admission from the first-order convergence verdict the `solver` axis carries for `optimization/design.md#DESIGN` and `program.md#PROGRAM`.

## [01]-[INDEX]

- [01]-[CONVEX]: linear / quadratic / second-order-cone / exponential-cone / semidefinite disciplined-convex programs over `cvxpy` DCP atoms and the Clarabel conic backend, under a `Sense` policy row and a DPP `Parameter` warm-re-solve sweep axis, assembled through one shared `_assemble` body plus the `_CONE_ROWS` data table, folding one content-keyed `ConvexReceipt` full-KKT optimality certificate per `ParamBind` row carrying the `ConvexEvidence` KKT-residual value object (the gap and the primal/dual feasibility residuals recomputed from the surfaced cvxpy KKT data through `_CONE_KKT`, `certified` gating all three within `_TOL`) — distinct from the shared `OutcomeReceipt`, each receipt implementing the `ReceiptContributor` port so the consumer drives egress, carrying the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns — on the one `ConvexProgram` owner.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the convex cases discriminated by the cone family the disciplined-convex model lands in, recoverable from the model structure itself, never a hand-rolled cone reduction; `Linear(c, a_ub, b_ub)` over an affine objective and elementwise inequality cone, `Quadratic(p, q, a_ub, b_ub)` carrying a symmetrized quadratic form through `cp.quad_form` under the same polyhedral cone, `SecondOrder(c, soc_terms, a_ub, b_ub)` adding `cp.SOC(t, X)` second-order-cone rows, `Exponential(c, exp_terms, a_ub, b_ub)` adding `cp.log_sum_exp` exponential-cone rows, and `Semidefinite(c_mat, a_ub, b_ub)` carrying a `Variable((n, n), symmetric=True)` matrix leaf under an explicit `X >> 0` cone row whose `Constraint.dual_value` surfaces the matrix dual the certificate folds. Each `@classmethod`-plus-`Self` factory (the subtype-binding form the sibling `program.md`/`solvers/receipt.md` hold, never a `@staticmethod` over a `"ConvexProgram"` forward-ref) packs the optional `sense: Sense` (`MIN` default, `MAX` for a concave objective) and the optional `params: ParamBind` warm-re-solve table into one `Policy` value object — the single uniform trailing slot every case carries, bound through the `ConvexProgram.policy` total `match self` or-pattern (the closed-union law `SolverReceipt.status` holds, closed by `assert_never`) rather than a `getattr(self, self.tag)[-1]` magic-negative-index reflection whose `object` residual escapes the exhaustive match — so the objective sign and the parameter sweep are one named row on the one owner, never two redundant positional tail slots repeated across five case tuples, never a parallel maximize-owner, never a per-sweep rebuilt `Problem`. The discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on the one `optimization` sub-domain, never a duplicated optimizer surface beside the conic solve.
- Modeling: every case lifts its problem data into one `cvxpy` `Variable`/`Parameter` algebra and composes the objective from DCP atoms (`cp.sum`, `cp.quad_form`, `cp.SOC`, `cp.log_sum_exp`, `cp.trace`, `cp.vec`) so curvature and sign are tracked by the DCP ruleset. The objective sign is the `Sense` policy row folded through the `_SENSE` table mapping `Sense.MIN`/`Sense.MAX` to the `cp.Minimize`/`cp.Maximize` constructor; the cone memberships are the relational operators and the explicit `cp.SOC`/`cp.log_sum_exp`/`X >> 0` cone rows. `Problem.is_dcp()` adjudicates curvature **before** the solve — a model that fails the DCP ruleset folds to a `SolveStatus.OTHER` certificate with an infinite gap rather than letting `solve` raise mid-fold, so the certificate is never produced for a problem the modeling layer rejects. The quadratic form is symmetrized at the numpy edge (`0.5·(P+Pᵀ)`) before `cp.quad_form` so the catalogued atom carries a DCP-legal symmetric form — a genuinely indefinite form fails `is_dcp()`, never a silent `cp.psd_wrap` coercion (the catalogued spectral atom this owner declines, so indefiniteness surfaces as a DCP rejection rather than a forced PSD lift). The semidefinite case carries the symmetric matrix domain on the `Variable((n, n), symmetric=True)` leaf attribute and the PSD membership as an explicit `X >> 0` cone row, because the dual-certificate is the owner's defining capability: a `PSD=True` leaf attribute hides the matrix dual `Z` behind the variable domain where `Constraint.dual_value` cannot reach it, so the explicit cone row is what surfaces `Z` for the `tr(Z·X)` complementary-slackness term and the eigenvalue dual-feasibility residual. Its polyhedral block reads `mat @ cp.vec(X) <= rhs` through the catalogued `cp.vec` vectorization rather than a hand-rolled `cp.reshape(..., order="C")`. A parametrized family declares one `cp.Parameter` leaf on the inequality `rhs` — the sole DPP-legal parametrizable buffer, since a `cp.Parameter` in the `cp.quad_form` form matrix or the constraint matrix `mat` breaks the DPP ruleset while an affine `rhs` leaf does not — so each `ParamBind` row binds `rhs` (a row omitting it reusing the prior `Parameter.value`) and warm-re-`solve`s the same compiled `Problem` across the sweep, folding one `ConvexReceipt` per bind; `_solve_bind` sets values by REGISTERED leaf name rather than by the bind's own keys, so a foreign key is structurally unreachable rather than a `parameters[name]` KeyError mid-solve. No case ever assembles a slack reformulation or a manual cone partition the modeling layer owns.
- Backend: `Problem.solve(solver=cp.CLARABEL, warm_start=True)` selects the Clarabel primal-dual interior-point backend — the default conic solver and the dual-certificate source — and writes the primal optimum to `Variable.value` and the dual multipliers to each `Constraint.dual_value`; `warm_start` reuses the prior factorization across a `Parameter` sweep. The entry preflights `cp.CLARABEL in cp.installed_solvers()` once and degrades a missing backend to `SolveStatus.OTHER` rather than letting the solve raise mid-fold. The optimality gap the receipt folds is the per-constraint complementary-slackness sum `Σᵢ slackᵢ` recovered through the `_CONE_KKT[_cone(c, cp)].slack` reduction that dispatches on the cvxpy `Constraint` cone family rather than the dual's matrix-vs-vector shape: the non-separable PSD AND SOC rows each contribute the SINGLE inner product `|⟨λ, g⟩|` (the PSD `|tr(Z·X)| = |np.sum(Z*X)|`, the SOC `|z·s|` over the stacked pair — the absolute value of the ONE sum, never the elementwise `Σ|λⱼ·gⱼ|` that cannot cancel the signed cross terms a non-separable cone carries at the optimum), while only the polyhedral/exponential `nonneg` rows contribute the orthant's genuinely separable `Σ|λⱼ·gⱼ|`, from those catalogued cvxpy `Constraint.dual_value` multipliers and the per-cone primal value. The companion dual-feasibility residual is genuinely cone-aware — the `_CONE_KKT[...].residual` distance from each dual to its own dual cone `K*ᵢ`: the polyhedral/exponential `nonneg` rows (the relational `log_sum_exp(...) <= b` inequality canonicalizes to a nonnegative-orthant scalar dual) score the self-dual orthant `max(−λ, 0)`, the `SOC` row scores the self-dual second-order cone `max(‖z₁:‖₂ − z₀, 0)` rather than an elementwise sign test the SOC dual does not satisfy, and the PSD row scores the self-dual PSD cone `max(−λ_min(Z), 0)` over the symmetrized matrix dual's spectrum through `np.linalg.eigvalsh` — so the cone membership is the constraint type the `_cone` discriminant reads, never a single elementwise sign test misapplied across the SOC or matrix cone. The matching primal-feasibility residual `dist(gᵢ(x), Kᵢ)` rides the same `_CONE_KKT[...].primal` cone-projection closure over the per-cone primal value the cell's `expr` closure reads off the universal `Constraint.args`, so the cone owns the primal-value read, slack, dual-feasibility, AND primal-feasibility as four closures on one cell, and the certificate folds exactly the catalogued cvxpy quantities — `Constraint.dual_value` and `Constraint.args[*].value`, the latter because `SOC(t, X)` and `ExpCone` carry NO single `.expr` the way the relational `Inequality` does, so a uniform `Constraint.expr.value` read would `AttributeError` on every SOC/SDP certificate — never a backend-internal residual the `cvxpy` `solve` path does not surface. The standalone `DefaultSolver` over a `get_problem_data(cp.CLARABEL)` reduction is available when the problem is already in cone-standard form, but the admitted path is the `cvxpy` `solve` selector, never a direct sparse `P`/`q`/`A`/`b` assembly this owner re-derives.
- Entry: `solve(program, lane)` is `async`, composing `lane.offload(_sweep, program, modality=Modality.THREAD)` (the Clarabel interior-point solve is native work; compute mints no limiter) under the hub `evidence_run` weave — span, fault fence, and the fenced `@receipted(REDACTION)` receipt harvest composed — and `.bind`-flattens the railed `_sweep`, returning `RuntimeRail[tuple[ConvexReceipt, ...]]` — one receipt for a single solve, one per `ParamBind` row for a sweep. The `.bind`-flatten joins the Clarabel solve fence and the railed `ContentIdentity.of` digest onto one rail exactly as `program.md` joins its `_program_key`, so a content-key fault propagates beside a solve fault rather than nesting `RuntimeRail[RuntimeRail[...]]`; the per-bind certificate rails thread through `traversed` under the default `Disposition.ABORT` so the sweep short-circuits to the first digest fault returning `RuntimeRail[Block[ConvexReceipt]]` the body `.map`s to the receipt tuple. The solve fence and the receipt egress stay orthogonal exactly as on `design.md` and `program.md`: `boundary` mints the rail, each `ConvexReceipt` owns its `contribute` projection, and the consumer drives egress over the receipt tuple through `Signals.emit` (polymorphic over the `Iterable[Receipt]` each `contribute` yields), never an inline `Signals.emit` threaded through the solve body and never a `@receipted` decoration on the `RuntimeRail`-returning entry where the aspect wraps a single `ReceiptContributor`-returning kernel. The shared `_assemble` body reads the tag total over the `_CONE_ROWS` table (a new cone family breaks the table lookup, not a body arm; the closed-union totality is owned by the one `_fields` `match`/`assert_never`), builds the `cvxpy` `Problem` behind the gated import, `is_dcp()`-gates the model, binds the registered `rhs` parameter per row, solves through Clarabel, and reduces the per-constraint duals and the per-cone primal values (read off `Constraint.args` through each cone cell's `expr` closure) into the complementary-slackness certificate. Each program-and-bind keys through the railed `ContentIdentity.of` over the canonical problem-data buffer seeded with the bound parameter values, the per-block ordinal-and-shape signature plus the sorted bind-key names folded into the `fmt` exactly as `program.md`'s `_program_key` folds its slot/shape discriminant — so two same-tag programs whose seed concatenations are byte-identical but whose block boundaries (or `terms` arity, or bound-key set) differ key DISTINCTLY rather than colliding on the boundary-erasing `b"".join`; the certificate is the convex proof object, so the entry never returns an uncertified estimate the way the design floor would.
- Receipt: `ConvexReceipt` is a **distinct** typed receipt, never the shared `OutcomeReceipt` `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` fold their convergence and feasibility verdicts into — it carries the duality-gap/dual-infeasibility/`SolveStatus` global-optimality certificate plus the `ConvexEvidence` KKT-residual value object the first-order and feasibility verdicts have no field for, so folding it onto `OutcomeReceipt` would erase the proof object. `ConvexEvidence` collapses the certificate residuals — `duality_gap`, `primal_residual`, `dual_infeasibility` — into one value object owning its own `facts()` projection, every slot recomputed from the surfaced KKT data through `_CONE_KKT`, so a new diagnostic is one slot on the evidence object rather than a new column on the receipt struct. Its coherence with `solvers/receipt.md#RECEIPT` is the termination vocabulary: the `status` field is the one `SolveStatus` `StrEnum` the solver routes carry, the cvxpy status string mapped through `_CONVEX_STATUS` (the convex sibling of the `_STATUS` `RESULTS` boundary table) — `optimal` to `SUCCESS`, `optimal_inaccurate` to `STAGNATION`, `infeasible`/`infeasible_inaccurate`/`infeasible_or_unbounded` to `INFEASIBLE`, `unbounded`/`unbounded_inaccurate` to `UNBOUNDED`, `solver_error` to `BREAKDOWN`, `user_limit` to `MAX_STEPS`, an unmapped constant degrading to `OTHER` rather than crashing. `ConvexReceipt.contribute` narrows the `ReceiptContributor` port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` the sibling `SolverReceipt`/`GraduationReceipt` return — `Receipt.of("compute.convex", ("emitted", self.program, facts))` over the canonical two-argument `(Phase, subject, facts)` `Evidence` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` the runtime owner deletes, spreading the program tag, the optimal objective as a native `float`, the `certified` flag as a native `bool`, the content key through `ContentKey.hex`, and the `ConvexEvidence.facts()` native-scalar diagnostic slots. A certified optimum graduates outward through `graduation/handoff.md#GRADUATION` on the dedicated `convex_program` `HandoffAxis` case — the dual multipliers and the full KKT triple (the complementary-slackness gap, the primal-feasibility residual, the dual-feasibility residual) are the global-optimality proof, a distinct admission from the `solver` axis's first-order convergence verdict, so a `status` other than `SolveStatus.SUCCESS` or any of the three residuals above tolerance is an admission rejection, never a graduated handoff.
- Packages: `cvxpy` (`Variable`, `Parameter`, `Minimize`, `Maximize`, `Problem`, `Problem.solve`, `Problem.value`, `Problem.status`, `Problem.is_dcp`, `installed_solvers`, `Variable.value`, `Constraint.dual_value`, `Constraint.args` (the universal expression-tree list whose per-element `.value` the cone cell's `expr` closure reads into the stacked primal — `[lhs, rhs]` for the relational `Inequality`, `[t, X]` for `SOC`, `[X]` for `PSD` — since `SOC`/`ExpCone` carry no single `.expr` and a uniform `Constraint.expr.value` read would `AttributeError` on every SOC/SDP certificate), the `Inequality.expr` (`lhs − rhs`) the relational `nonneg` extractor reads, the `==`/`<=`/`>=`/`>>` relational algebra, the `SOC` cone constructor and the `>>` PSD-cone relation over a `symmetric=True` matrix leaf, the `SOC`/`PSD` `Constraint` subclasses the `_cone` discriminant reads (`cp.SOC()`/`cp.PSD()` class patterns) to key the per-constraint dual-cone residual, the `quad_form`/`log_sum_exp`/`trace`/`vec`/`sum` DCP atoms, the `Constant` leaf, the `CLARABEL` backend selector — all catalogued in `compute/.api/cvxpy.md`), `clarabel` (the default conic interior-point backend whose `DefaultSolution.z` conic dual multipliers cvxpy surfaces through the catalogued `Constraint.dual_value`, from which the fence recovers the complementary-slackness KKT gap and the cone-feasibility residuals; admitted via the `solver=cp.CLARABEL` selector, never a direct `DefaultSolver` assembly — catalogued in `compute/.api/clarabel.md`), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `maximum`, `linalg.norm`, `linalg.eigvalsh` — the canonical problem-data buffer, the form symmetrization, the orthant dual-cone-residual max-reduction, the SOC dual-cone residual `‖z₁:‖₂ − z₀` through `linalg.norm`, and the PSD-cone dual-feasibility residual over the symmetrized matrix dual's minimum eigenvalue), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, and quadratic form admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `solvers/receipt.md#RECEIPT` (the one `SolveStatus` termination `StrEnum` this receipt's `status` field carries, the cvxpy status string folded into it through the local `_CONVEX_STATUS` boundary table — the convex sibling of the `_STATUS` `RESULTS` map, never a parallel convex-only status vocabulary), `graduation/handoff.md#GRADUATION` (the `convex_program` axis the certified optimum graduates on), `expression` (`tag`/`case`/`tagged_union` the `ConvexProgram` discriminated union, `Block.of_seq` the per-bind certificate-rail carrier `traversed` threads, `Result.map`/`Result.bind` the railed-key and sweep joins), `msgspec` (`Struct` the frozen `Policy`/`ConeRow`/`Fields`/`ConeKKT`/`ConvexEvidence`/`ConvexReceipt` value objects — `gc=False` only on the scalar-leaf carriers `ConvexEvidence` (three `float`s) and `ConvexReceipt` (whose transitive fields are all non-cyclic scalar leaves on the per-bind hot path), GC-tracked on the container/closure carriers `Policy` (the `binds` tuple), `Fields` (the `terms` tuple), and the closure-carrying `ConeRow`/`ConeKKT`, matching the sibling `program.md` rule that a container/closure-holding struct stays tracked), runtime (`RuntimeRail`/`boundary` the solve fence, `traversed` (default `Disposition.ABORT`) the per-bind rail fold short-circuiting to the first digest fault, `Receipt`/`ReceiptContributor` the `ConvexReceipt` contributes through and the consumer-driven `Signals.emit` egress, `ContentIdentity.of` returning the railed `RuntimeRail[ContentKey]` this owner threads through `Result.map` rather than a bare key over the `of` `CANONICAL_POLICY` default, `ContentKey`).
- Growth: a new convex cone family is one `ConvexProgram` case plus one `_CONE_ROWS` row carrying its `ConeRow` objective/extra closures; a new DCP atom or cone membership is one cell on the `ConeRow`; a concave objective is the `Sense.MAX` field on `Policy` through the `_SENSE` table, never a maximize-owner; a parameter sweep is the `Policy.binds` `ParamBind` table over the one DPP-legal `rhs` `cp.Parameter` leaf warm-re-solving the one compiled `Problem`, never a rebuilt `Problem`; a new solve-policy axis (a `clarabel.DefaultSettings` tolerance, a `gp`/`qcp` mode) is one `Policy` field rather than a sixth positional tail slot threaded through five factory signatures; a new solve diagnostic is one slot on `ConvexEvidence` reaching the facts map through the `__struct_fields__`/`astuple` zip with no second edit; a new cone family's KKT contribution is one `_CONE_KKT` row carrying its `(expr, slack, residual, primal)` cone-projection closures (the `expr` primal-value extractor off `Constraint.args`) plus one `_cone` arm reading the cvxpy `Constraint` subclass; a new cvxpy status constant is one `_CONVEX_STATUS` row; zero new surface, never a per-cone owner, never a parallel linear-program-and-SDP owner, never a per-route helper body parallel to the shared `_assemble`, never a parallel maximize-owner beside the minimize one, never a hand-rolled interior-point or cone reduction `cvxpy` and Clarabel already own.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from operator import attrgetter
from typing import Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from msgspec.structs import astuple

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt

# --- [TYPES] -------------------------------------------------------------------------------

type ParamBind = tuple[Map[str, np.ndarray], ...]
type ConeObjective = Callable[[object, "Fields", object], object]  # `(Variable, Fields, cp) -> Expression` cost
type ConeRows = Callable[[object, "Fields", object], tuple[object, ...]]  # `(Variable, Fields, cp)` -> extra cone-constraint rows
type ConeExpr = Callable[[object], np.ndarray | None]  # `(Constraint) -> stacked primal value` off `args`, None if unsolved
type ConeSlack = Callable[[np.ndarray, np.ndarray], float]  # `(dual, expr) -> |⟨λ, g⟩|` complementary slackness
type ConeResidual = Callable[[np.ndarray], float]  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
type ConePrimal = Callable[[np.ndarray], float]  # `(expr) -> dist(g, K)` primal-cone-membership violation


class Sense(StrEnum):
    MIN = "minimize"
    MAX = "maximize"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL = 1e-8
_NO_BIND: ParamBind = (Map.empty(),)

_CONVEX_STATUS: Map[str, SolveStatus] = Map.of_seq([
    ("optimal", SolveStatus.SUCCESS),
    ("optimal_inaccurate", SolveStatus.STAGNATION),
    ("infeasible", SolveStatus.INFEASIBLE),
    ("infeasible_inaccurate", SolveStatus.INFEASIBLE),
    ("unbounded", SolveStatus.UNBOUNDED),
    ("unbounded_inaccurate", SolveStatus.UNBOUNDED),
    ("infeasible_or_unbounded", SolveStatus.INFEASIBLE),
    ("solver_error", SolveStatus.BREAKDOWN),
    ("user_limit", SolveStatus.MAX_STEPS),
])


# --- [MODELS] ------------------------------------------------------------------------------


class Policy(Struct, frozen=True):  # GC-tracked: `binds` is a container (tuple of Map of ndarray)
    # the objective sign and the warm-re-solve sweep on one uniform value object, so a new
    # solve-policy axis (a `clarabel.DefaultSettings` tolerance) is one field rather than a sixth
    # tail slot threaded through five factory signatures and five case tuples.
    sense: Sense = Sense.MIN
    binds: ParamBind = _NO_BIND


class ConvexEvidence(Struct, frozen=True, gc=False):
    # the three KKT residuals recomputed from the surfaced `Constraint.dual_value`/`Constraint.args[*].value`/
    # `Variable.value` through `_CONE_KKT`: the complementary-slackness gap and the primal/dual cone
    # feasibility distances, each sourced from a catalogued cvxpy member (the primal value off `args`,
    # never `Constraint.expr` which SOC/SDP rows lack).
    duality_gap: float
    primal_infeasibility: float
    dual_infeasibility: float

    def facts(self) -> dict[str, object]:
        # zip the declared `Struct` fields against their values, so a new diagnostic slot reaches the
        # facts map by its declaration alone — never a second hand-spelled key drifting from the field.
        return dict(zip(self.__struct_fields__, astuple(self), strict=True))

    @classmethod
    def uncertified(cls) -> Self:
        inf = float("inf")
        return cls(inf, inf, inf)


class ConvexReceipt(Struct, frozen=True, gc=False):
    program: str
    objective: float
    status: SolveStatus
    evidence: ConvexEvidence
    content_key: ContentKey

    @property
    def certified(self) -> bool:
        # the optimality certificate is ALL THREE KKT conditions within tolerance, not the gap alone:
        # a vanishing complementary-slackness sum with a primal-feasible iterate AND dual-feasible
        # multipliers, so a tiny gap beside a positive `dual_infeasibility` is an admission rejection.
        return self.status is SolveStatus.SUCCESS and max(astuple(self.evidence)) <= _TOL

    def contribute(self) -> Iterable[Receipt]:
        # returns the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares (the concrete
        # one-element tuple the sibling `SolverReceipt`/`GraduationReceipt` return satisfying it), so a
        # multi-phase contributor stays representable on the one port type the corpus carries uniformly.
        facts: dict[str, object] = {
            "program": self.program,
            "objective": self.objective,
            "status": self.status,
            "certified": self.certified,
            "key": self.content_key.hex,
            **self.evidence.facts(),
        }
        return (Receipt.of("compute.convex", ("emitted", self.program, facts)),)


@tagged_union(frozen=True)
class ConvexProgram:
    # every case ends with one uniform `Policy` slot bound by the `policy` total `match`; the
    # cone-specific arrays are the leading positions `_fields` projects, so the discriminant stays the
    # cone family while `sense`/`binds` ride one named value object rather than two reflective tail reads.
    tag: Literal["linear", "quadratic", "second_order", "exponential", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    exponential: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()

    @classmethod
    def Linear(cls, c: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> Self:
        return cls(linear=(c, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Quadratic(
        cls, p: np.ndarray, q: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND
    ) -> Self:
        return cls(quadratic=(p, q, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def SecondOrder(
        cls,
        c: np.ndarray,
        soc_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
    ) -> Self:
        return cls(second_order=(c, soc_terms, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Exponential(
        cls,
        c: np.ndarray,
        exp_terms: tuple[tuple[np.ndarray, float], ...],
        a_ub: np.ndarray,
        b_ub: np.ndarray,
        sense: Sense = Sense.MIN,
        params: ParamBind = _NO_BIND,
    ) -> Self:
        return cls(exponential=(c, exp_terms, a_ub, b_ub, Policy(sense, params)))

    @classmethod
    def Semidefinite(cls, c_mat: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> Self:
        return cls(semidefinite=(c_mat, a_ub, b_ub, Policy(sense, params)))

    @property
    def policy(self) -> Policy:
        # the trailing `Policy` is bound by one total `match self` or-pattern over the closed union,
        # never the reflective `getattr(self, self.tag)[-1]` whose `object` residual escapes the
        # exhaustive match — the closed-union law the sibling `SolverReceipt.status` and `OutcomeReceipt`
        # folds hold; `assert_never` makes a sixth case a compile-surfaced gap.
        match self:
            case (
                ConvexProgram(tag="linear", linear=(*_, Policy() as policy))
                | ConvexProgram(tag="quadratic", quadratic=(*_, Policy() as policy))
                | ConvexProgram(tag="second_order", second_order=(*_, Policy() as policy))
                | ConvexProgram(tag="exponential", exponential=(*_, Policy() as policy))
                | ConvexProgram(tag="semidefinite", semidefinite=(*_, Policy() as policy))
            ):
                return policy
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] --------------------------------------------------------------------------


# the convex family's DEFAULT graduation ceiling — all three KKT conditions within tolerance;
# the governed policy row per the hub ceiling law, caller-overridable at the hub.
_CEILING: Final[Map[str, float]] = Map.of_seq([("duality_gap", 1e-8), ("primal_infeasibility", 1e-8), ("dual_infeasibility", 1e-8)])

# the family modality row: the Clarabel interior-point solve is native Rust riding the runtime
# THREAD band; policy DATA, never a per-page literal, never a compute-minted limiter.
_MODALITY: Final[Modality] = Modality.THREAD


async def solve(program: ConvexProgram, lane: LanePolicy) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    # `.bind`-flatten joins the Clarabel solve fence and the railed `ContentIdentity.of` digest
    # onto one rail so a content-key fault propagates beside a solve fault, never double-wrapped;
    # the weave owns span, fence, and the `@receipted(REDACTION)` receipt harvest.
    async def dispatch() -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
        return (await lane.offload(_sweep, program, modality=_MODALITY)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.CONVEX, f"convex.{program.tag}", dispatch)


def graduates(receipt: ConvexReceipt) -> "RuntimeRail[GraduationReceipt]":
    # the self-wired `convex_program` producer the Packages block long claimed without a prelude
    # import — the prose-vs-fence split closes fence-side: the KKT certificate triple is the ledger
    # the hub clears against the `_CEILING` family row, so a returned point whose gap exceeds
    # tolerance is an admission rejection, never a graduated handoff.
    ledger = {
        "duality_gap": receipt.evidence.duality_gap,
        "primal_infeasibility": receipt.evidence.primal_infeasibility,
        "dual_infeasibility": receipt.evidence.dual_infeasibility,
    }
    return GraduationReceipt.graduates("compute.convex", HandoffAxis(convex_program=receipt.program), receipt.content_key, ledger, dict(_CEILING.items()))
```

The cone-row table is the dispatch surface: each `ConeRow` carries the objective expression builder, the extra-cone-rows builder, and the `psd` matrix-scaffold flag over the shared `(Variable, Fields, cp)` scaffold, so the five families differ by two closures and one flag rather than five `match` bodies. `Fields` is the one typed problem-data value object the `_fields` `match` projects every case into — `cost`/`mat`/`rhs` present always, `lin` carrying the `quadratic` linear term and `None` elsewhere, `terms` carrying the SOC/exponential cone terms and empty elsewhere — so the closures and `_assemble` read fixed attributes rather than tag-keyed tuple positions; `_assemble` builds the common polyhedral scaffold once and folds the cell with zero `program.tag` literal branching.

```python signature
# --- [COMPOSITION] -------------------------------------------------------------------------


class ConeRow(Struct, frozen=True):  # GC-tracked: carries the two cone closures
    objective: ConeObjective
    extra: ConeRows
    psd: bool = False


def _affine_cost(x: object, fields: "Fields", cp: object) -> object:
    return fields.cost @ x


def _quadratic_cost(x: object, fields: "Fields", cp: object) -> object:
    return 0.5 * cp.quad_form(x, fields.cost) + fields.lin @ x


def _trace_cost(x: object, fields: "Fields", cp: object) -> object:
    return cp.trace(fields.cost @ x)


def _no_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return ()


def _soc_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return tuple(cp.SOC(cp.Constant(bound), _as_mat(a) @ x) for a, bound in fields.terms)


def _exp_rows(x: object, fields: "Fields", cp: object) -> tuple[object, ...]:
    return tuple(cp.log_sum_exp(_as_mat(a) @ x) <= bound for a, bound in fields.terms)


_CONE_ROWS: Map[str, ConeRow] = Map.of_seq([
    ("linear", ConeRow(_affine_cost, _no_rows)),
    ("quadratic", ConeRow(_quadratic_cost, _no_rows)),
    ("second_order", ConeRow(_affine_cost, _soc_rows)),
    ("exponential", ConeRow(_affine_cost, _exp_rows)),
    ("semidefinite", ConeRow(_trace_cost, _no_rows, psd=True)),
])


class Fields(Struct, frozen=True):  # GC-tracked: `terms` is a container (tuple of (ndarray, float) pairs)
    cost: np.ndarray  # cost vector, or the symmetrized form/`c_mat` matrix for `quadratic`/`semidefinite`
    mat: np.ndarray
    rhs: np.ndarray
    lin: np.ndarray | None = None  # the `quadratic` linear term `q`; absent on every other cone
    terms: tuple[tuple[np.ndarray, float], ...] = ()  # SOC/exponential cone terms; empty elsewhere


def _fields(program: ConvexProgram) -> Fields:
    match program:
        case ConvexProgram(tag="linear", linear=(c, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub))
        case ConvexProgram(tag="quadratic", quadratic=(p, q, a_ub, b_ub, _)):
            return Fields(_symm(p), _as_mat(a_ub), _as_vec(b_ub), lin=_as_vec(q))
        case ConvexProgram(tag="second_order", second_order=(c, terms, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub), terms=terms)
        case ConvexProgram(tag="exponential", exponential=(c, terms, a_ub, b_ub, _)):
            return Fields(_as_vec(c), _as_mat(a_ub), _as_vec(b_ub), terms=terms)
        case ConvexProgram(tag="semidefinite", semidefinite=(c_mat, a_ub, b_ub, _)):
            return Fields(_symm(c_mat), _as_mat(a_ub), _as_vec(b_ub))
        case _ as unreachable:
            assert_never(unreachable)


def _sweep(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    import cvxpy as cp

    # a missing backend or a DCP-rejected model folds ONE uncertified receipt per `ParamBind` row, so the
    # tuple cardinality matches the success fan (`one per bind`) the entry contract promises rather than
    # collapsing a sweep to a single receipt the consumer cannot align to its bind table.
    if cp.CLARABEL not in cp.installed_solvers():
        return _uncertified_sweep(program, None)
    objective, constraints, fields, parameters = _assemble(program, cp)
    problem = cp.Problem(_SENSE[program.policy.sense](cp)(objective), constraints)
    if not problem.is_dcp():
        return _uncertified_sweep(program, fields)
    rails = (_solve_bind(program, problem, constraints, parameters, fields, bind, cp) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _uncertified_sweep(program: ConvexProgram, fields: "Fields | None") -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    # one uncertified receipt per bind row keyed on its own bind, threaded through the same `traversed`
    # fold the certified fan uses so the missing-backend and DCP-rejection paths preserve sweep cardinality.
    rails = (_convex_key(program, fields, bind).map(lambda key: _uncertified(program, key)) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _assemble(program: ConvexProgram, cp: object) -> tuple[object, list[object], "Fields", dict[str, object]]:
    # the decision dimension is the COST extent `fields.cost.shape[0]` (the vector length, or the form/matrix
    # order for `quadratic`/`semidefinite`), NOT the constraint-matrix column count — an unconstrained or
    # box-only program carries an empty `mat`, so sizing `x` off `mat.shape[1]` would mis-size the variable
    # and break `cost @ x`; the polyhedral row is added ONLY when the inequality block is non-empty (`rhs.size`).
    parameters: dict[str, object] = {}
    row, fields = _CONE_ROWS[program.tag], _fields(program)
    rhs = _leaf("rhs", fields.rhs, program.policy.binds, cp, parameters)
    n = int(fields.cost.shape[0])
    if row.psd:
        x = cp.Variable((n, n), symmetric=True)
        # explicit `X >> 0` row so `Constraint.dual_value` surfaces the matrix dual `Z` the KKT
        # certificate folds; a `PSD=True` leaf would hide `Z` behind the variable domain.
        cone = [x >> 0]
        polyhedral = [fields.mat @ cp.vec(x) <= rhs] if fields.rhs.size else []
    else:
        x = cp.Variable(n)
        cone = []
        polyhedral = [fields.mat @ x <= rhs] if fields.rhs.size else []
    return row.objective(x, fields, cp), [*polyhedral, *cone, *row.extra(x, fields, cp)], fields, parameters


def _solve_bind(
    program: ConvexProgram,
    problem: object,
    constraints: list[object],
    parameters: dict[str, object],
    fields: "Fields",
    bind: Map[str, np.ndarray],
    cp: object,
) -> "RuntimeRail[ConvexReceipt]":
    # bind values are pulled by REGISTERED leaf name (only `rhs` is parametrizable under DPP), never by
    # iterating the bind's own keys: a foreign key (a `cost`/`mat` sweep DPP forbids in `quad_form`/the
    # constraint matrix) is structurally unreachable rather than a `parameters[name]` KeyError, and a row
    # omitting a registered leaf reuses its prior `value` the way a DPP warm re-solve updates only the
    # changed parameter, so a heterogeneous bind table never indexes a missing key.
    for name, leaf in parameters.items():
        leaf.value = np.asarray(bind.try_find(name).default_value(leaf.value), dtype=float)
    problem.solve(solver=cp.CLARABEL, warm_start=True)
    return _convex_key(program, fields, bind).map(lambda key: _certificate(program, problem, constraints, key, cp))


def _leaf(name: str, value: np.ndarray, binds: ParamBind, cp: object, parameters: dict[str, object]) -> object:
    # register a `cp.Parameter` leaf only when a bind references the key AND the buffer is non-empty, so an
    # empty `rhs` (an unconstrained/box-only program whose polyhedral row is skipped) never registers an inert
    # parameter `_solve_bind` would set against a constraint the assembly did not build.
    if not value.size or not any(name in bind for bind in binds):
        return value
    leaf = cp.Parameter(value.shape, name=name, value=value)
    parameters[name] = leaf
    return leaf
```

The certificate fold reads the KKT gap and the dual-feasibility residual from the catalogued cvxpy duals through the `_CONE_KKT` table keyed on the constraint's cone family — the `_cone` discriminant reads the cvxpy `Constraint` subclass (`cp.PSD`/`cp.SOC` class patterns, polyhedral and exponential `log_sum_exp(...) <= b` rows folding to `nonneg`), so the SOC dual scores against the self-dual second-order cone `max(‖z₁:‖₂ − z₀, 0)` and the PSD dual against the PSD cone `max(−λ_min(Z), 0)` rather than the elementwise `max(−λ, 0)` only the nonnegative orthant admits. Each `ConeKKT` cell carries the cone's `expr` primal-value extractor (off the universal `Constraint.args`, since `SOC(t, X)`/`ExpCone` carry no single `.expr`), its `slack` complementary-slackness pairing (the separable `Σ|λⱼ·gⱼ|` for the orthant, the single inner product `|⟨λ, g⟩|` for the non-separable SOC/PSD cones), its `residual` dual-cone-membership distance, and its `primal` primal-cone-membership distance as four closures, so the evidence fold reduces `kkt.expr`/`kkt.slack`/`kkt.residual`/`kkt.primal` over the constraint list into the `ConvexEvidence` value object rather than a shape-probe `if` — three diagnostic slots, each a catalogued cvxpy quantity. `_SENSE` is the data table folding the objective sign onto the `cp.Minimize`/`cp.Maximize` constructor.

```python signature
# --- [COMPOSITION] -------------------------------------------------------------------------

_SENSE: Map[Sense, Callable[[object], object]] = Map.of_seq([(Sense.MIN, attrgetter("Minimize")), (Sense.MAX, attrgetter("Maximize"))])


def _certificate(program: ConvexProgram, problem: object, constraints: list[object], key: ContentKey, cp: object) -> ConvexReceipt:
    if problem.value is None:
        return _uncertified(program, key)
    status = _CONVEX_STATUS.try_find(str(problem.status)).default_value(SolveStatus.OTHER)
    return ConvexReceipt(program.tag, float(problem.value), status, _evidence(constraints, cp), key)


def _uncertified(program: ConvexProgram, key: ContentKey) -> ConvexReceipt:
    return ConvexReceipt(program.tag, float("inf"), SolveStatus.OTHER, ConvexEvidence.uncertified(), key)


def _evidence(constraints: list[object], cp: object) -> ConvexEvidence:
    # one `_CONE_KKT` projection per cvxpy constraint cone family: the cone identity is the
    # constraint TYPE, never the dual array's matrix-vs-vector shape, so the `SOC` dual is scored
    # against the self-dual second-order cone and the `PSD` dual against the PSD cone rather than
    # an elementwise sign test the nonnegative orthant alone admits. The primal value rides the cone
    # cell's `expr` extractor off the universal `Constraint.args`, since `SOC(t, X)`/`ExpCone` carry
    # NO single `.expr` attribute (only the relational `Inequality` does) — reading `c.expr.value`
    # uniformly would `AttributeError` on every SOC/SDP row, the exact certificate-bearing cases.
    cells = ((_CONE_KKT[_cone(c, cp)], c) for c in constraints)
    rows = [(kkt, np.asarray(c.dual_value, dtype=float), kkt.expr(c)) for kkt, c in cells if c.dual_value is not None]
    solved = [(kkt, dual, expr) for kkt, dual, expr in rows if expr is not None]
    return ConvexEvidence(
        duality_gap=float(sum(kkt.slack(dual, expr) for kkt, dual, expr in solved)),
        primal_infeasibility=max((kkt.primal(expr) for kkt, _, expr in solved), default=0.0),
        dual_infeasibility=max((kkt.residual(dual) for kkt, dual, _ in solved), default=0.0),
    )


def _cone(constraint: object, cp: object) -> str:
    # the cone identity is the cvxpy `Constraint` subclass the KKT fold reads the dual cone from; the
    # `cp.PSD`/`cp.SOC` rows score against their own dual cones, every polyhedral inequality and the
    # `log_sum_exp(...) <= b` exponential row canonicalizing to a nonnegative-orthant scalar dual.
    match constraint:
        case cp.PSD():
            return "psd"
        case cp.SOC():
            return "soc"
        case _:
            return "nonneg"


def _slack_separable(dual: np.ndarray, expr: np.ndarray) -> float:
    return float(np.abs(dual * expr).sum())  # orthant `Σ|λᵢ·gᵢ|`: componentwise slackness, genuinely separable


def _slack_inner(dual: np.ndarray, expr: np.ndarray) -> float:
    # SOC/PSD slackness is the SINGLE inner product `|⟨λ, g⟩|` over the stacked/flattened pair (the SOC
    # `|z·s|`, the PSD `|tr(Z·X)| = |Σ Zᵢⱼ·Xᵢⱼ|`), never the orthant's `Σ|λᵢ·gᵢ|` — on a non-separable
    # cone the signed cross terms cancel at the optimum while `Σ|·|` over-counts them as a false gap.
    return float(np.abs(np.sum(dual.ravel() * expr.ravel())))


def _residual_nonneg(dual: np.ndarray) -> float:
    return float(np.maximum(-dual, 0.0).max(initial=0.0))  # self-dual orthant: `max(−λ, 0)`


def _residual_soc(dual: np.ndarray) -> float:
    z = dual.ravel()  # self-dual second-order cone: `max(‖z₁:‖₂ − z₀, 0)`, never an elementwise sign test
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _residual_psd(dual: np.ndarray) -> float:
    # self-dual PSD cone: `max(−λ_min(½(Z+Zᵀ)), 0)` over the symmetrized matrix dual's spectrum.
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (dual + dual.T)).min(initial=0.0), 0.0))


def _primal_nonneg(expr: np.ndarray) -> float:
    # cvxpy `Inequality` `g(x) <= 0` canonical form: the primal violation is `max(g, 0)`.
    return float(np.maximum(expr, 0.0).max(initial=0.0))


def _primal_soc(expr: np.ndarray) -> float:
    z = expr.ravel()  # second-order cone `‖z₁:‖₂ <= z₀`: violation `max(‖z₁:‖₂ − z₀, 0)`
    return float(np.maximum(float(np.linalg.norm(z[1:])) - float(z[0]), 0.0)) if z.size else 0.0


def _primal_psd(expr: np.ndarray) -> float:
    # PSD cone `X >> 0`: violation `max(−λ_min(½(X+Xᵀ)), 0)` over the symmetrized matrix's spectrum.
    return float(np.maximum(-np.linalg.eigvalsh(0.5 * (expr + expr.T)).min(initial=0.0), 0.0))


def _expr_nonneg(constraint: object) -> np.ndarray | None:
    # the relational `Inequality` carries `args = [lhs, rhs]` and `g = lhs − rhs` (cvxpy `Inequality.expr`,
    # `<= 0` at feasibility); read it off the universal `args` rather than the `.expr` only relationals own.
    lhs, rhs = constraint.args[0].value, constraint.args[1].value
    return None if lhs is None or rhs is None else np.asarray(lhs, dtype=float) - np.asarray(rhs, dtype=float)


def _expr_soc(constraint: object) -> np.ndarray | None:
    # `SOC(t, X)` carries NO `.expr`; its `args = [t, X]` stack into `[t, *X]` matching the dual layout
    # cvxpy reshapes to `[t_dual, X_dual...]`, so the slackness pairs and the `‖X‖₂ <= t` test align.
    t, x = constraint.args[0].value, constraint.args[1].value
    return None if t is None or x is None else np.append(np.ravel(np.asarray(t, dtype=float)), np.ravel(np.asarray(x, dtype=float)))


def _expr_psd(constraint: object) -> np.ndarray | None:
    # `X >> 0` carries `args = [X]`; the primal value is the optimal matrix the `tr(Z·X)` slackness and the
    # `λ_min(X)` membership read, the same `(n, n)` shape as the symmetric matrix dual `Constraint.dual_value`.
    matrix = constraint.args[0].value
    return None if matrix is None else np.asarray(matrix, dtype=float)


class ConeKKT(Struct, frozen=True):  # GC-tracked: carries the four cone-membership closures
    expr: ConeExpr  # `(Constraint) -> stacked primal value` off `args`, since SOC/PSD carry no `.expr`
    slack: ConeSlack  # `(dual, expr) -> |⟨λ, g⟩|` complementary-slackness contribution
    residual: ConeResidual  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
    primal: ConePrimal  # `(expr) -> dist(g, K)` primal-cone-membership violation


_CONE_KKT: Map[str, ConeKKT] = Map.of_seq([
    ("nonneg", ConeKKT(_expr_nonneg, _slack_separable, _residual_nonneg, _primal_nonneg)),
    ("soc", ConeKKT(_expr_soc, _slack_inner, _residual_soc, _primal_soc)),
    ("psd", ConeKKT(_expr_psd, _slack_inner, _residual_psd, _primal_psd)),
])


def _seed_arrays(fields: "Fields | None") -> tuple[np.ndarray, ...]:
    if fields is None:  # missing-backend key: program tag plus binds, no problem-data block
        return ()
    core = (fields.cost, *((fields.lin,) if fields.lin is not None else ()), fields.mat, fields.rhs)
    term_blocks = tuple(np.append(_as_mat(a).ravel(), bound) for a, bound in fields.terms)
    return (*core, *term_blocks)


def _convex_key(program: ConvexProgram, fields: "Fields | None", bind: Map[str, np.ndarray]) -> "RuntimeRail[ContentKey]":
    # the SAME slot-index/shape discriminant `program.md`'s `_program_key` adopts: the seed blocks are
    # variable-length arrays joined with NO boundary delimiter (`cost`/`lin`/`mat`/`rhs`, the flattened
    # SOC/exp `term_blocks`, then the bound `rhs` values), so a bare `b"".join(...)` over a
    # `convex.{tag}` fmt collides two same-tag programs whose concatenations are byte-identical but whose
    # block boundaries differ (a `cost=[1,2,3], mat=[[4,5,6]], rhs=[7]` linear program and a
    # `cost=[1,2], mat=[[3,4],[5,6]], rhs=[7]` one both flatten to `[1,2,3,4,5,6,7]`). Folding each
    # block's ordinal `i` and shape into the fmt — the digest seed `ContentIdentity.of` derives over
    # `fmt|policy.spec` — distinguishes the shifted boundaries and the differing `terms` arity/per-term
    # shapes, and the sorted bind-key names fold beside it so a bind dict keyed on a foreign parameter
    # never collides with the `rhs` bind, exactly as `program.md` folds its `_engine_tag` seed suffix.
    seed_blocks = _seed_arrays(fields)
    bind_blocks = tuple(np.asarray(bind[name], dtype=float) for name in sorted(bind))
    blocks = (*seed_blocks, *bind_blocks)
    buffer = b"".join(np.ascontiguousarray(field).tobytes() for field in blocks)
    shape_tag = "".join(f".{i}:{f.ndim}x{'x'.join(map(str, f.shape))}" for i, f in enumerate(blocks))
    bind_tag = f".binds:{'.'.join(sorted(bind))}" if bind else ""
    return ContentIdentity.of(f"convex.{program.tag}{shape_tag}{bind_tag}", buffer)


def _symm(array: np.ndarray) -> np.ndarray:
    form = np.atleast_2d(np.asarray(array, dtype=float))
    return np.ascontiguousarray(0.5 * (form + form.T))


def _as_vec(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.asarray(array, dtype=float).ravel())


def _as_mat(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.atleast_2d(np.asarray(array, dtype=float)))
```
