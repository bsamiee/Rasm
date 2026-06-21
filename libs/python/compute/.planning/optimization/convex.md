# [PY_COMPUTE_CONVEX]

The dual-certificate proof of global optimality the first-order design loop and the discrete math program structurally cannot furnish — the convex analogue of the certified-enclosure ladder in `numerics/interval.md#ENCLOSURE`. `ConvexProgram` discriminates the cone family a disciplined-convex model lands in (linear/affine, quadratic, second-order-cone, exponential-cone, semidefinite), each built from `cvxpy` `Variable`/`Parameter` leaves under a `Sense`-rowed `Minimize`/`Maximize` objective and the relational cone algebra (`==`, `<=`, `>=`, `>>`), compiled to standard conic form and solved through the Clarabel interior-point backend that returns the primal optimum and the per-constraint dual multipliers. The five cases share **one shared `_assemble` body plus a `_CONE_ROWS` data table** — the per-cone objective expression and the extra cone rows are a `ConeRow` cell, not five parallel solver bodies — so adding a cone family is one table row, never a new arm. The objective sense is a `Sense` policy row threaded through the `_SENSE` constructor table; a parametrized family threads `cp.Parameter` leaves so a sweep binds `Parameter.value` and warm-re-`solve`s the one compiled DPP `Problem` across the bind table rather than rebuilding it.

`ConvexReceipt` folds one content-keyed certificate over `problem.value`, every `Constraint.dual_value`, the complementary-slackness KKT gap `Σ|⟨λᵢ, gᵢ(x)⟩|` recovered from those duals and the constraint-expression values, the cone-aware dual-feasibility residual `dist(λᵢ, K*ᵢ)` scored per constraint against its own dual cone `K*ᵢ`, the matching primal-feasibility residual `dist(gᵢ(x), Kᵢ)` scored against the constraint's own primal cone `Kᵢ` — both through the one `_CONE_KKT` table — and the solve diagnostics `cvxpy` genuinely surfaces, `solver_stats.num_iters`/`solver_stats.solve_time`, collapsed into one `ConvexEvidence` value object that owns its own `facts()` projection. Every slot is recovered from a cvxpy-surfaced quantity: cvxpy's Clarabel reduction leaves `SolverStats.extra_stats` unset and persists no raw `DefaultSolution`, so the primal/dual feasibility residuals are recomputed from the surfaced `Constraint.dual_value`/`Constraint.expr.value`/`Variable.value` KKT data through the cone table rather than read off the Clarabel-internal `r_prim`/`r_dual` no cvxpy path exposes. The projection rides native `float`/`int`/`bool` scalars into the `observability/receipts#RECEIPT` `EventDict` the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes, never a pre-`f""`-formatted `dict[str, str]`, and the content key renders through `ContentKey.hex`. The cvxpy `problem.status` string folds into the shared `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns through the `_CONVEX_STATUS` boundary table. The KKT gap is the proof object: a vanishing complementary-slackness sum with feasible duals certifies the returned point is the global optimum, the convex sibling of the `Enclosure.certified` flag, so a returned point whose gap exceeds the tolerance is an admission rejection rather than a degraded answer.

`ConvexReceipt` stays a **distinct** typed receipt and never folds into the `OutcomeReceipt` the `design`/`program` siblings share: the duality-gap, dual-infeasibility, and `SolveStatus` certificate is the global-optimality proof object the first-order convergence verdict and the feasibility verdict carry no field for, so collapsing it would erase the spectral certificate the route-owned-receipt law preserves. The coherence with `solvers/receipt.md#RECEIPT` is the **status vocabulary**, not the carrier — the convex status reads through the one `SolveStatus` enum the C# graduation gate and the sibling solve receipts read, mapped from the cvxpy status constants through the same boundary-table fold the solver routes use for the `RESULTS` enums. Like `optimization/program.md#PROGRAM`, this owner carries **no numpy floor** — the convex solve *is* `cvxpy` over the conic backend, so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate, both packages riding the companion `python_version<'3.15'` band. The certified optimum graduates outward on the dedicated `convex_program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`, a distinct admission from the first-order convergence verdict the `solver` axis carries for `optimization/design.md#DESIGN` and `program.md#PROGRAM`.

## [01]-[INDEX]

- [01]-[CONVEX]: linear / quadratic / second-order-cone / exponential-cone / semidefinite disciplined-convex programs over `cvxpy` DCP atoms and the Clarabel conic backend, under a `Sense` policy row and a DPP `Parameter` warm-re-solve sweep axis, assembled through one shared `_assemble` body plus the `_CONE_ROWS` data table, folding one content-keyed `ConvexReceipt` duality-gap optimality certificate per `ParamBind` row carrying the `ConvexEvidence` KKT-residual value object (the gap and the primal/dual feasibility residuals recomputed from the surfaced cvxpy KKT data through `_CONE_KKT`, the iteration count and wall time from `solver_stats`) — distinct from the shared `OutcomeReceipt`, each receipt implementing the `ReceiptContributor` port so the consumer drives egress, carrying the `SolveStatus` termination vocabulary `solvers/receipt.md#RECEIPT` owns — on the one `ConvexProgram` owner.

## [02]-[CONVEX]

- Owner: `ConvexProgram` — the convex cases discriminated by the cone family the disciplined-convex model lands in, recoverable from the model structure itself, never a hand-rolled cone reduction; `Linear(c, a_ub, b_ub)` over an affine objective and elementwise inequality cone, `Quadratic(p, q, a_ub, b_ub)` carrying a symmetrized quadratic form through `cp.quad_form` under the same polyhedral cone, `SecondOrder(c, soc_terms, a_ub, b_ub)` adding `cp.SOC(t, X)` second-order-cone rows, `Exponential(c, exp_terms, a_ub, b_ub)` adding `cp.log_sum_exp` exponential-cone rows, and `Semidefinite(c_mat, a_ub, b_ub)` carrying a `Variable((n, n), symmetric=True)` matrix leaf under an explicit `X >> 0` cone row whose `Constraint.dual_value` surfaces the matrix dual the certificate folds. Each factory packs the optional `sense: Sense` (`MIN` default, `MAX` for a concave objective) and the optional `params: ParamBind` warm-re-solve table into one `Policy` value object — the single uniform trailing slot every case carries, read through the `ConvexProgram.policy` projection rather than a `getattr(self, self.tag)[-2]` magic-negative-index tail reflection — so the objective sign and the parameter sweep are one named row on the one owner, never two redundant positional tail slots repeated across five case tuples, never a parallel maximize-owner, never a per-sweep rebuilt `Problem`. The discriminant is the cone structure, so the differentiable design loop, the discrete math program, and the certified convex program are sibling owners on the one `optimization` sub-domain, never a duplicated optimizer surface beside the conic solve.
- Assembly: the five cone families share **one `_assemble` body** that builds the common scaffold — `cp.Variable` sized off the polyhedral column count `fields.mat.shape[1]`, the polyhedral inequality row `fields.mat @ x <= fields.rhs`, and the optional `rhs` `cp.Parameter` leaf — then folds the `_CONE_ROWS[tag]` `ConeRow` cell whose `objective(x, fields, cp)` builds the case objective expression and whose `extra(x, fields, cp)` builds the case-specific cone rows (`SecondOrder`'s `cp.SOC`, `Exponential`'s `cp.log_sum_exp`, `Semidefinite`'s explicit `X >> 0` cone row and trace objective). The per-case problem data rides one typed `Fields` value object the `_fields` `match` projects — `cost`/`mat`/`rhs` present always, `lin` the `quadratic` linear term and `None` elsewhere, `terms` the SOC/exponential cone terms and empty elsewhere — so the closures and `_assemble` index named attributes rather than tag-keyed tuple positions and the shared body carries **zero `program.tag` literal branch**. The `row.psd` flag flips the matrix-variable scaffold — a `symmetric=True` square leaf sized off `fields.cost.shape[0]`, the `cp.vec` polyhedral block, and the `X >> 0` cone row whose dual the certificate reads — while the polyhedral cone, the rhs leaf, and the content-key seed are built once in the shared body rather than re-spelled per arm, so a new cone family is one `ConeRow` row plus one `Fields` projection arm plus one `tag` literal, never a sixth near-identical `match` arm in the body. The `_CONE_ROWS` table is the row behind the gated import; the per-cell closures defer the `cp.<atom>` references so the cp315-clean enum keys the table without importing the gated package at module load.
- Modeling: every case lifts its problem data into one `cvxpy` `Variable`/`Parameter` algebra and composes the objective from DCP atoms (`cp.sum`, `cp.quad_form`, `cp.SOC`, `cp.log_sum_exp`, `cp.trace`, `cp.vec`) so curvature and sign are tracked by the DCP ruleset. The objective sign is the `Sense` policy row folded through the `_SENSE` table mapping `Sense.MIN`/`Sense.MAX` to the `cp.Minimize`/`cp.Maximize` constructor; the cone memberships are the relational operators and the explicit `cp.SOC`/`cp.log_sum_exp`/`X >> 0` cone rows. `Problem.is_dcp()` adjudicates curvature **before** the solve — a model that fails the DCP ruleset folds to a `SolveStatus.OTHER` certificate with an infinite gap rather than letting `solve` raise mid-fold, so the certificate is never produced for a problem the modeling layer rejects. The quadratic form is symmetrized at the numpy edge (`0.5·(P+Pᵀ)`) before `cp.quad_form` so the catalogued atom carries a DCP-legal symmetric form — a genuinely indefinite form fails `is_dcp()`, never a silent `psd_wrap` coercion the catalogue does not own. The semidefinite case carries the symmetric matrix domain on the `Variable((n, n), symmetric=True)` leaf attribute and the PSD membership as an explicit `X >> 0` cone row, because the dual-certificate is the owner's defining capability: a `PSD=True` leaf attribute hides the matrix dual `Z` behind the variable domain where `Constraint.dual_value` cannot reach it, so the explicit cone row is what surfaces `Z` for the `tr(Z·X)` complementary-slackness term and the eigenvalue dual-feasibility residual. Its polyhedral block reads `mat @ cp.vec(X) <= rhs` through the catalogued `cp.vec` vectorization rather than a hand-rolled `cp.reshape(..., order="C")`. A parametrized family declares `cp.Parameter` leaves over the named bind keys under DPP, so each `ParamBind` row sets `Parameter.value` and warm-re-`solve`s the same compiled `Problem` across the sweep, folding one `ConvexReceipt` per bind. No case ever assembles a slack reformulation or a manual cone partition the modeling layer owns.
- Backend: `Problem.solve(solver=cp.CLARABEL, warm_start=True)` selects the Clarabel primal-dual interior-point backend — the default conic solver and the dual-certificate source — and writes the primal optimum to `Variable.value`, the dual multipliers to each `Constraint.dual_value`, and the solve diagnostics to `Problem.solver_stats`; `warm_start` reuses the prior factorization across a `Parameter` sweep. The entry preflights `cp.CLARABEL in cp.installed_solvers()` once and degrades a missing backend to `SolveStatus.OTHER` rather than letting the solve raise mid-fold. The optimality gap the receipt folds is the per-constraint complementary-slackness sum `Σ|⟨λᵢ, gᵢ(x)⟩|` recovered through the `_CONE_KKT[_cone(c, cp)].slack` reduction that dispatches on the cvxpy `Constraint` cone family rather than the dual's matrix-vs-vector shape: the PSD row contributes `|tr(Z·X)|` (the Frobenius inner product `|np.sum(Z*X)|`, the absolute value of the one sum, never the elementwise `Σ|Zᵢⱼ·Xᵢⱼ|` that cannot cancel cross terms) and the separable polyhedral/SOC/exponential rows contribute `Σ|λᵢ·gᵢ|` (the independent slackness pairs) from those catalogued cvxpy `dual_value` multipliers and `Constraint.expr.value` constraint-expression values. The companion dual-feasibility residual is genuinely cone-aware — the `_CONE_KKT[...].residual` distance from each dual to its own dual cone `K*ᵢ`: the polyhedral/exponential `nonneg` rows (the relational `log_sum_exp(...) <= b` inequality canonicalizes to a nonnegative-orthant scalar dual) score the self-dual orthant `max(−λ, 0)`, the `SOC` row scores the self-dual second-order cone `max(‖z₁:‖₂ − z₀, 0)` rather than an elementwise sign test the SOC dual does not satisfy, and the PSD row scores the self-dual PSD cone `max(−λ_min(Z), 0)` over the symmetrized matrix dual's spectrum through `np.linalg.eigvalsh` — so the cone membership is the constraint type the `_cone` discriminant reads, never a single elementwise sign test misapplied across the SOC or matrix cone. The matching primal-feasibility residual `dist(gᵢ(x), Kᵢ)` rides the same `_CONE_KKT[...].primal` cone-projection closure over the surfaced `Constraint.expr.value`, so the cone owns slack, dual-feasibility, AND primal-feasibility as three closures on one cell. The solve diagnostics cvxpy genuinely surfaces — `solver_stats.num_iters` and `solver_stats.solve_time` — fold beside the gap into `ConvexEvidence`; cvxpy's Clarabel reduction extracts only `obj_val`/`solve_time`/`iterations`/`x`/`z`/`status` and leaves `SolverStats.extra_stats` unset, persisting no raw `DefaultSolution`, so the Clarabel-internal `r_prim`/`r_dual` and the Clarabel `DefaultSolution.obj_val_dual` field are unreachable through the cvxpy path and the gap is read as the KKT complementary-slackness sum rather than a primal/dual objective difference. The standalone `DefaultSolver` over a `get_problem_data(cp.CLARABEL)` reduction is available when the problem is already in cone-standard form, but the admitted path is the `cvxpy` `solve` selector, never a direct sparse `P`/`q`/`A`/`b` assembly this owner re-derives.
- Entry: `solve` enters one `boundary("convex.solve", ...)` and `.bind`-flattens the railed `_sweep`, returning `RuntimeRail[tuple[ConvexReceipt, ...]]` — one receipt for a single solve, one per `ParamBind` row for a sweep. The `.bind`-flatten joins the Clarabel solve fence and the railed `ContentIdentity.of` digest onto one rail exactly as `program.md` joins its `_program_key`, so a content-key fault propagates beside a solve fault rather than nesting `RuntimeRail[RuntimeRail[...]]`; the per-bind certificate rails thread through `traversed` under the default `Disposition.ABORT` so the sweep short-circuits to the first digest fault returning `RuntimeRail[Block[ConvexReceipt]]` the body `.map`s to the receipt tuple. The solve fence and the receipt egress stay orthogonal exactly as on `design.md` and `program.md`: `boundary` mints the rail, each `ConvexReceipt` owns its `contribute` projection, and the consumer drives egress over the receipt tuple through `Signals.emit` (polymorphic over the `Iterable[Receipt]` each `contribute` yields), never an inline `Signals.emit` threaded through the solve body and never a `@receipted` decoration on the `RuntimeRail`-returning entry where the aspect wraps a single `ReceiptContributor`-returning kernel. The shared `_assemble` body reads the tag total over the `_CONE_ROWS` table (a new cone family breaks the table lookup, not a body arm; the closed-union totality is owned by the one `_fields` `match`/`assert_never`), builds the `cvxpy` `Problem` behind the gated import, `is_dcp()`-gates the model, binds each parameter row, solves through Clarabel, and reduces the per-constraint duals and the constraint expression values into the complementary-slackness certificate. Each program-and-bind keys through the railed `ContentIdentity.of` over the canonical problem-data buffer seeded with the bound parameter values, so a re-solve from identical data and bind keys identically by reference; the certificate is the convex proof object, so the entry never returns an uncertified estimate the way the design floor would.
- Receipt: `ConvexReceipt` is a **distinct** typed receipt, never the shared `OutcomeReceipt` `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` fold their convergence and feasibility verdicts into — it carries the duality-gap/dual-infeasibility/`SolveStatus` global-optimality certificate plus the `ConvexEvidence` KKT-residual value object the first-order and feasibility verdicts have no field for, so folding it onto `OutcomeReceipt` would erase the proof object. `ConvexEvidence` collapses the solve diagnostics — `duality_gap`, `primal_residual`, `dual_infeasibility`, `solve_time`, `iterations` — into one value object owning its own `facts()` projection, every slot a cvxpy-surfaced quantity (the gap and the primal/dual feasibility residuals from the recovered KKT data through `_CONE_KKT`, the wall time and iteration count from `solver_stats`), so a new diagnostic is one slot on the evidence object rather than a new column on the receipt struct. Its coherence with `solvers/receipt.md#RECEIPT` is the termination vocabulary: the `status` field is the one `SolveStatus` `StrEnum` the solver routes carry, the cvxpy status string mapped through `_CONVEX_STATUS` (the convex sibling of the `_STATUS` `RESULTS` boundary table) — `optimal` to `SUCCESS`, `optimal_inaccurate` to `STAGNATION`, `infeasible`/`infeasible_inaccurate`/`infeasible_or_unbounded` to `INFEASIBLE`, `unbounded`/`unbounded_inaccurate` to `UNBOUNDED`, `solver_error` to `BREAKDOWN`, `user_limit` to `MAX_STEPS`, an unmapped constant degrading to `OTHER` rather than crashing. `ConvexReceipt.contribute` yields the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares — `Receipt.of("compute.optimization.convex", ("emitted", self.program, facts))` over the canonical two-argument `(Phase, subject, facts)` `Evidence` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` the runtime owner deletes, spreading the program tag, the optimal objective as a native `float`, the `certified` flag as a native `bool`, the content key through `ContentKey.hex`, and the `ConvexEvidence.facts()` native-scalar diagnostic slots. A certified optimum graduates outward through `graduation/handoff.md#GRADUATION` on the dedicated `convex_program` `HandoffAxis` case — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof, a distinct admission from the `solver` axis's first-order convergence verdict, so a `status` other than `SolveStatus.SUCCESS` or a gap above tolerance is an admission rejection, never a graduated handoff.
- Packages: `cvxpy` (`Variable`, `Parameter`, `Minimize`, `Maximize`, `Problem`, `Problem.solve`, `Problem.value`, `Problem.status`, `Problem.is_dcp`, `Problem.solver_stats` (the `SolverStats.num_iters`/`solve_time` the receipt folds; its `extra_stats` is `None` under cvxpy's Clarabel reduction, so no backend-residual pair is read through it), `Variable.value`, `Constraint.dual_value`, `Constraint.expr` (whose `.value` recovers the constraint-expression values the complementary-slackness sum AND the primal-feasibility residual fold), the `==`/`<=`/`>=`/`>>` relational algebra, the `SOC` cone constructor and the `>>` PSD-cone relation over a `symmetric=True` matrix leaf, the `SOC`/`PSD` `Constraint` subclasses the `_cone` discriminant reads (`cp.SOC()`/`cp.PSD()` class patterns) to key the per-constraint dual-cone residual, the `quad_form`/`log_sum_exp`/`trace`/`vec`/`sum` DCP atoms, the `CLARABEL` backend selector, `installed_solvers` — all catalogued in `compute/.api/cvxpy.md`), `clarabel` (the default conic interior-point backend whose `DefaultSolution.z` conic dual multipliers cvxpy surfaces through the catalogued `Constraint.dual_value`, from which the fence recovers the complementary-slackness KKT gap, and whose `iterations`/`solve_time` cvxpy surfaces through `Problem.solver_stats.num_iters`/`solver_stats.solve_time`; cvxpy's Clarabel reduction extracts only `obj_val`/`solve_time`/`iterations`/`x`/`z`/`status` and leaves `SolverStats.extra_stats` unset, so the Clarabel-internal `r_prim`/`r_dual` residual pair and the `DefaultSolution.obj_val_dual` field — both present on the native `DefaultSolution` — are unreachable through the cvxpy path, and the gap is read as the KKT complementary-slackness sum rather than a primal/dual objective difference while the primal/dual feasibility residuals are recomputed from the surfaced KKT data; admitted via the `solver=` selector — catalogued in `compute/.api/clarabel.md`), `numpy` (`asarray`, `ascontiguousarray`, `atleast_2d`, `maximum`, `linalg.norm`, `linalg.eigvalsh` — the canonical problem-data buffer, the form symmetrization, the orthant dual-cone-residual max-reduction, the SOC dual-cone residual `‖z₁:‖₂ − z₀` through `linalg.norm`, and the PSD-cone dual-feasibility residual over the symmetrized matrix dual's minimum eigenvalue), `numerics/array.md#PAYLOAD` (the cost vector, constraint matrix, and quadratic form admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), `solvers/receipt.md#RECEIPT` (the one `SolveStatus` termination `StrEnum` this receipt's `status` field carries, the cvxpy status string folded into it through the local `_CONVEX_STATUS` boundary table — the convex sibling of the `_STATUS` `RESULTS` map, never a parallel convex-only status vocabulary), `graduation/handoff.md#GRADUATION` (the `convex_program` axis the certified optimum graduates on), `expression` (`tag`/`case`/`tagged_union` the `ConvexProgram` discriminated union, `Block.of_seq` the per-bind certificate-rail carrier `traversed` threads, `Result.map`/`Result.bind` the railed-key and sweep joins), `msgspec` (`Struct` the frozen `Policy`/`ConeRow`/`Fields`/`ConeKKT`/`ConvexEvidence`/`ConvexReceipt` value objects — `gc=False` on the pure-data carriers, GC-tracked on the closure-carrying `ConeRow`/`ConeKKT`), runtime (`RuntimeRail`/`boundary` the solve fence, `traversed` (default `Disposition.ABORT`) the per-bind rail fold short-circuiting to the first digest fault, `Receipt`/`ReceiptContributor` the `ConvexReceipt` contributes through and the consumer-driven `Signals.emit` egress, `ContentIdentity.of` returning the railed `RuntimeRail[ContentKey]` this owner threads through `Result.map` rather than a bare key, `ContentKey`/`IdentityPolicy`).
- Growth: a new convex cone family is one `ConvexProgram` case plus one `_CONE_ROWS` row carrying its `ConeRow` objective/extra closures; a new DCP atom or cone membership is one cell on the `ConeRow`; a concave objective is the `Sense.MAX` field on `Policy` through the `_SENSE` table, never a maximize-owner; a parameter sweep is the `Policy.binds` `ParamBind` table over `cp.Parameter` leaves warm-re-solving the one compiled `Problem`, never a rebuilt `Problem`; a new solve-policy axis (a `clarabel.DefaultSettings` tolerance, a `gp`/`qcp` mode) is one `Policy` field rather than a sixth positional tail slot threaded through five factory signatures; a new solve diagnostic is one slot on `ConvexEvidence` reaching the facts map through the `__struct_fields__`/`astuple` zip with no second edit; a new cone family's KKT contribution is one `_CONE_KKT` row carrying its `(slack, residual, primal)` cone-projection closures plus one `_cone` arm reading the cvxpy `Constraint` subclass; a new cvxpy status constant is one `_CONVEX_STATUS` row; zero new surface, never a per-cone owner, never a parallel linear-program-and-SDP owner, never a per-route helper body parallel to the shared `_assemble`, never a parallel maximize-owner beside the minimize one, never a hand-rolled interior-point or cone reduction `cvxpy` and Clarabel already own.
- Boundary: classical disciplined-convex programming over `cvxpy` and a conic backend only — the linear, quadratic, second-order-cone, exponential-cone, and semidefinite programs with a dual-certificate proof of global optimality are in-scope; the differentiable inverse-design loop stays on `design.md` and the discrete/global math program on `program.md`, and neither duplicates here. This owner carries **no numpy floor**: the certified convex solve *is* `cvxpy` over Clarabel (the DCP compilation, the cone reduction, the interior-point iteration), so a cp315 run without the wheel returns `Error(Import)` rather than an uncertified estimate — the deliberate floor asymmetry against `design.md` and matching the no-floor posture of `program.md` and the no-floor Qhull routes of `analysis/spatial.md#SPATIAL`, because an uncertified convex answer is no certificate at all. A non-convex or neural relaxation, a hand-rolled cone slack reformulation, an uncatalogued `psd_wrap` coercion of an indefinite form, a primal/dual objective difference `obj_val − obj_val_dual` gap where the cvxpy reduction surfaces only `obj_val` and the KKT complementary-slackness sum is the gap, a `PSD=True` leaf attribute carrying the SDP cone where it hides the matrix dual `Z` the certificate folds and the explicit `X >> 0` cone row surfaces, a `cp.reshape(..., order="C")` matrix-flatten where `cp.vec` vectorizes, a per-cone `match` arm parallel to the shared `_assemble` body, an elementwise `max(−λ, 0)` dual-feasibility residual applied across the `SOC` or PSD cone where the `_CONE_KKT[_cone(c, cp)].residual` scores each dual against its own dual cone (`max(‖z₁:‖₂ − z₀, 0)` for the SOC dual, `max(−λ_min(Z), 0)` for the PSD dual), a `_is_matrix_cone(dual)` matrix-vs-vector array-shape probe where the cone identity is the cvxpy `Constraint` subclass the `_cone` discriminant reads, a hand-spelled per-field `ConvexEvidence.facts()` dict re-spelling the declared `Struct` fields where the `__struct_fields__`/`astuple` zip projects them drift-proof, a ragged `tuple[object, ...]` problem-data carrier indexed by `program.tag`-keyed magic positions where the typed `Fields` value object names `cost`/`lin`/`mat`/`rhs`/`terms`, a `program.tag == "quadratic"` literal branch in the shared body where the uniform `Fields` projection erases the per-case offset, a `Sense, ParamBind` pair repeated as two positional tail slots across five case tuples read through a `getattr(self, self.tag)[-2]`/`[-1]` magic-negative-index reflection where the one uniform `Policy` value object names `sense`/`binds`, a bare `ContentKey` return from `_convex_key` dropping the railed digest fault where `ContentIdentity.of` returns `RuntimeRail[ContentKey]` the fold threads through `Result.map`, a `solver_stats.extra_stats["solution"].r_prim`/`r_dual` read off a raw Clarabel `DefaultSolution` cvxpy's reduction never persists (it leaves `extra_stats` unset) where the feasibility residuals recompute from the surfaced KKT data through `_CONE_KKT[...].primal`/`.residual`, a four-positional `Receipt.of("emitted", owner, subject, facts)` against the two-argument `of(owner, evidence)` contract, a `contribute` returning one bare `Receipt` against the `Iterable[Receipt]` port, a `str()`/`f""`-coerced `dict[str, str]` facts map where the renderer carries native scalars, a `str(key.value)` render where `ContentKey.hex` holds, a parallel maximize-owner beside the `Sense.MIN` one, a per-sweep rebuilt `Problem` where DPP warm re-solve applies, a mixed-integer branch the conic backend does not own, a `@receipted` decoration on the `RuntimeRail`-returning `solve` where the aspect wraps a single `ReceiptContributor`-returning kernel and the consumer drives egress over the receipt tuple, an inline `Signals.emit` threaded through the solve body, and a parallel optimizer surface beside the conic solve are the deleted forms; the modeling stays on `cvxpy` and the solve on Clarabel, so this owner composes the certified optimum rather than re-deriving it.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from operator import attrgetter
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import Block, case, tag, tagged_union
from msgspec import Struct
from msgspec.structs import astuple

from rasm.compute.solvers.receipt import SolveStatus
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt


type ParamBind = tuple[FrozenDict[str, np.ndarray], ...]
type ConeObjective = Callable[[object, "Fields", object], object]  # `(Variable, Fields, cp) -> Expression` cost
type ConeRows = Callable[[object, "Fields", object], tuple[object, ...]]  # `(Variable, Fields, cp)` -> extra cone-constraint rows
type ConeSlack = Callable[[np.ndarray, np.ndarray], float]  # `(dual, expr) -> |⟨λ, g⟩|` complementary slackness
type ConeResidual = Callable[[np.ndarray], float]  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
type ConePrimal = Callable[[np.ndarray], float]  # `(expr) -> dist(g, K)` primal-cone-membership violation


class Sense(StrEnum):
    MIN = "minimize"
    MAX = "maximize"


_TOL = 1e-8
_NO_BIND: ParamBind = (FrozenDict({}),)

_CONVEX_STATUS: FrozenDict[str, SolveStatus] = FrozenDict(
    {
        "optimal": SolveStatus.SUCCESS,
        "optimal_inaccurate": SolveStatus.STAGNATION,
        "infeasible": SolveStatus.INFEASIBLE,
        "infeasible_inaccurate": SolveStatus.INFEASIBLE,
        "unbounded": SolveStatus.UNBOUNDED,
        "unbounded_inaccurate": SolveStatus.UNBOUNDED,
        "infeasible_or_unbounded": SolveStatus.INFEASIBLE,
        "solver_error": SolveStatus.BREAKDOWN,
        "user_limit": SolveStatus.MAX_STEPS,
    }
)


class Policy(Struct, frozen=True, gc=False):
    # the objective sign and the warm-re-solve sweep on one uniform value object, so a new
    # solve-policy axis (a `clarabel.DefaultSettings` tolerance) is one field rather than a sixth
    # tail slot threaded through five factory signatures and five case tuples.
    sense: Sense = Sense.MIN
    binds: ParamBind = _NO_BIND


class ConvexEvidence(Struct, frozen=True, gc=False):
    # every slot is a cvxpy-surfaced quantity: feasibility residuals recomputed from the recovered
    # KKT data via `_CONE_KKT`, time/iterations from `solver_stats` — never the Clarabel-internal
    # `r_prim`/`r_dual`, which cvxpy's reduction leaves unreachable (`extra_stats` unset).
    duality_gap: float
    primal_residual: float
    dual_infeasibility: float
    solve_time: float
    iterations: int

    def facts(self) -> dict[str, object]:
        # zip the declared `Struct` fields against their values, so a new diagnostic slot reaches the
        # facts map by its declaration alone — never a second hand-spelled key drifting from the field.
        return dict(zip(self.__struct_fields__, astuple(self), strict=True))

    @staticmethod
    def uncertified() -> "ConvexEvidence":
        inf = float("inf")
        return ConvexEvidence(inf, inf, inf, 0.0, 0)


class ConvexReceipt(Struct, frozen=True, gc=False):
    program: str
    objective: float
    status: SolveStatus
    evidence: ConvexEvidence
    content_key: ContentKey

    @property
    def certified(self) -> bool:
        return self.status is SolveStatus.SUCCESS and self.evidence.duality_gap <= _TOL

    def contribute(self) -> Iterable[Receipt]:
        facts = {
            "program": self.program,
            "objective": self.objective,
            "status": self.status,
            "certified": self.certified,
            "key": self.content_key.hex,
            **self.evidence.facts(),
        }
        return (Receipt.of("compute.optimization.convex", ("emitted", self.program, facts)),)


@tagged_union(frozen=True)
class ConvexProgram:
    # every case ends with one uniform `Policy` slot; the cone-specific arrays are the leading
    # positions `_fields` projects, so the discriminant stays the cone family while `sense`/`binds`
    # ride one named value object rather than two reflective `[-2]`/`[-1]` tail reads.
    tag: Literal["linear", "quadratic", "second_order", "exponential", "semidefinite"] = tag()
    linear: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    quadratic: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, Policy] = case()
    second_order: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    exponential: tuple[np.ndarray, tuple[tuple[np.ndarray, float], ...], np.ndarray, np.ndarray, Policy] = case()
    semidefinite: tuple[np.ndarray, np.ndarray, np.ndarray, Policy] = case()

    @staticmethod
    def Linear(c: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(linear=(c, a_ub, b_ub, Policy(sense, params)))

    @staticmethod
    def Quadratic(p: np.ndarray, q: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(quadratic=(p, q, a_ub, b_ub, Policy(sense, params)))

    @staticmethod
    def SecondOrder(c: np.ndarray, soc_terms: tuple[tuple[np.ndarray, float], ...], a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(second_order=(c, soc_terms, a_ub, b_ub, Policy(sense, params)))

    @staticmethod
    def Exponential(c: np.ndarray, exp_terms: tuple[tuple[np.ndarray, float], ...], a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(exponential=(c, exp_terms, a_ub, b_ub, Policy(sense, params)))

    @staticmethod
    def Semidefinite(c_mat: np.ndarray, a_ub: np.ndarray, b_ub: np.ndarray, sense: Sense = Sense.MIN, params: ParamBind = _NO_BIND) -> "ConvexProgram":
        return ConvexProgram(semidefinite=(c_mat, a_ub, b_ub, Policy(sense, params)))

    @property
    def policy(self) -> Policy:
        return getattr(self, self.tag)[-1]


def solve(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    # `.bind`-flatten joins the Clarabel solve fence and the railed `ContentIdentity.of` digest
    # onto one rail so a content-key fault propagates beside a solve fault, never double-wrapped.
    return boundary("convex.solve", lambda: _sweep(program)).bind(lambda rail: rail)
```

The cone-row table is the dispatch surface: each `ConeRow` carries the objective expression builder, the extra-cone-rows builder, and the `psd` matrix-scaffold flag over the shared `(Variable, Fields, cp)` scaffold, so the five families differ by two closures and one flag rather than five `match` bodies. `Fields` is the one typed problem-data value object the `_fields` `match` projects every case into — `cost`/`mat`/`rhs` present always, `lin` carrying the `quadratic` linear term and `None` elsewhere, `terms` carrying the SOC/exponential cone terms and empty elsewhere — so the closures and `_assemble` read fixed attributes rather than tag-keyed tuple positions; `_assemble` builds the common polyhedral scaffold once and folds the cell with zero `program.tag` literal branching.

```python signature
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


_CONE_ROWS: FrozenDict[str, ConeRow] = FrozenDict(
    {
        "linear": ConeRow(_affine_cost, _no_rows),
        "quadratic": ConeRow(_quadratic_cost, _no_rows),
        "second_order": ConeRow(_affine_cost, _soc_rows),
        "exponential": ConeRow(_affine_cost, _exp_rows),
        "semidefinite": ConeRow(_trace_cost, _no_rows, psd=True),
    }
)


class Fields(Struct, frozen=True, gc=False):
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
        case unreachable:
            assert_never(unreachable)


def _sweep(program: ConvexProgram) -> "RuntimeRail[tuple[ConvexReceipt, ...]]":
    import cvxpy as cp

    if cp.CLARABEL not in cp.installed_solvers():
        return _convex_key(program, None, FrozenDict({})).map(lambda key: (_uncertified(program, key),))
    objective, constraints, fields, parameters = _assemble(program, cp)
    problem = cp.Problem(_SENSE[program.policy.sense](cp)(objective), constraints)
    if not problem.is_dcp():
        return _convex_key(program, fields, FrozenDict({})).map(lambda key: (_uncertified(program, key),))
    rails = (_solve_bind(program, problem, constraints, parameters, fields, bind, cp) for bind in program.policy.binds)
    return traversed(Block.of_seq(rails)).map(lambda block: tuple(block))


def _assemble(program: ConvexProgram, cp: object) -> tuple[object, list[object], "Fields", dict[str, object]]:
    parameters: dict[str, object] = {}
    row, fields = _CONE_ROWS[program.tag], _fields(program)
    rhs = _leaf("rhs", fields.rhs, program.policy.binds, cp, parameters)
    if row.psd:
        n = fields.cost.shape[0]
        x = cp.Variable((n, n), symmetric=True)
        # explicit `X >> 0` row so `Constraint.dual_value` surfaces the matrix dual `Z` the KKT
        # certificate folds; a `PSD=True` leaf would hide `Z` behind the variable domain.
        cone = [x >> 0]
        polyhedral = [fields.mat @ cp.vec(x) <= rhs]
    else:
        x = cp.Variable(int(fields.mat.shape[1]))  # decision dimension is the polyhedral column count
        cone = []
        polyhedral = [fields.mat @ x <= rhs]
    return row.objective(x, fields, cp), [*polyhedral, *cone, *row.extra(x, fields, cp)], fields, parameters


def _solve_bind(
    program: ConvexProgram,
    problem: object,
    constraints: list[object],
    parameters: dict[str, object],
    fields: "Fields",
    bind: FrozenDict[str, np.ndarray],
    cp: object,
) -> "RuntimeRail[ConvexReceipt]":
    for name, value in bind.items():
        parameters[name].value = np.asarray(value, dtype=float)
    problem.solve(solver=cp.CLARABEL, warm_start=True)
    return _convex_key(program, fields, bind).map(lambda key: _certificate(program, problem, constraints, key, cp))


def _leaf(name: str, value: np.ndarray, binds: ParamBind, cp: object, parameters: dict[str, object]) -> object:
    if not any(name in bind for bind in binds):
        return value
    leaf = cp.Parameter(value.shape, name=name, value=value)
    parameters[name] = leaf
    return leaf
```

The certificate fold reads the KKT gap and the dual-feasibility residual from the catalogued cvxpy duals through the `_CONE_KKT` table keyed on the constraint's cone family — the `_cone` discriminant reads the cvxpy `Constraint` subclass (`cp.PSD`/`cp.SOC` class patterns, polyhedral and exponential `log_sum_exp(...) <= b` rows folding to `nonneg`), so the SOC dual scores against the self-dual second-order cone `max(‖z₁:‖₂ − z₀, 0)` and the PSD dual against the PSD cone `max(−λ_min(Z), 0)` rather than the elementwise `max(−λ, 0)` only the nonnegative orthant admits. Each `ConeKKT` cell carries the cone's `slack` complementary-slackness pairing, its `residual` dual-cone-membership distance, and its `primal` primal-cone-membership distance as three closures, so the evidence fold reduces `kkt.slack`/`kkt.residual`/`kkt.primal` over the constraint list rather than a shape-probe `if`. It then folds the iteration count and wall time cvxpy genuinely surfaces — `solver_stats.num_iters`/`solver_stats.solve_time` — beside those KKT residuals into the `ConvexEvidence` value object, one diagnostic owner the receipt carries with no read off a Clarabel `DefaultSolution` cvxpy's reduction never persists. `_SENSE` is the data table folding the objective sign onto the `cp.Minimize`/`cp.Maximize` constructor.

```python signature
_SENSE: FrozenDict[Sense, Callable[[object], object]] = FrozenDict(
    {
        Sense.MIN: attrgetter("Minimize"),
        Sense.MAX: attrgetter("Maximize"),
    }
)


def _certificate(program: ConvexProgram, problem: object, constraints: list[object], key: ContentKey, cp: object) -> ConvexReceipt:
    if problem.value is None:
        return _uncertified(program, key)
    status = _CONVEX_STATUS.get(str(problem.status), SolveStatus.OTHER)
    return ConvexReceipt(program.tag, float(problem.value), status, _evidence(constraints, problem.solver_stats, cp), key)


def _uncertified(program: ConvexProgram, key: ContentKey) -> ConvexReceipt:
    return ConvexReceipt(program.tag, float("inf"), SolveStatus.OTHER, ConvexEvidence.uncertified(), key)


def _evidence(constraints: list[object], stats: object, cp: object) -> ConvexEvidence:
    # one `_CONE_KKT` projection per cvxpy constraint cone family: the cone identity is the
    # constraint TYPE, never the dual array's matrix-vs-vector shape, so the `SOC` dual is scored
    # against the self-dual second-order cone and the `PSD` dual against the PSD cone rather than
    # an elementwise sign test the nonnegative orthant alone admits. Every fold reads SURFACED cvxpy
    # data only — duals, constraint-expression values, and `solver_stats` — never a phantom Clarabel
    # `DefaultSolution` cvxpy's reduction never persists.
    rows = [
        (_CONE_KKT[_cone(c, cp)], np.asarray(c.dual_value, dtype=float), np.asarray(c.expr.value, dtype=float))
        for c in constraints
        if c.dual_value is not None and c.expr.value is not None
    ]
    return ConvexEvidence(
        duality_gap=float(sum(kkt.slack(dual, expr) for kkt, dual, expr in rows)),
        primal_residual=max((kkt.primal(expr) for kkt, _, expr in rows), default=0.0),
        dual_infeasibility=max((kkt.residual(dual) for kkt, dual, _ in rows), default=0.0),
        solve_time=float(getattr(stats, "solve_time", 0.0) or 0.0),
        iterations=int(getattr(stats, "num_iters", 0) or 0),
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
    return float(np.abs(dual * expr).sum())  # `Σ|λᵢ·gᵢ|` independent complementary-slackness pairs


def _slack_frobenius(dual: np.ndarray, expr: np.ndarray) -> float:
    return float(np.abs(np.sum(dual * expr)))  # PSD `|tr(Z·X)|`, abs of the one Frobenius sum


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


class ConeKKT(Struct, frozen=True):  # GC-tracked: carries the three cone-membership closures
    slack: ConeSlack        # `(dual, expr) -> |⟨λ, g⟩|` complementary-slackness contribution
    residual: ConeResidual  # `(dual) -> dist(λ, K*)` dual-cone-membership violation
    primal: ConePrimal      # `(expr) -> dist(g, K)` primal-cone-membership violation


_CONE_KKT: FrozenDict[str, ConeKKT] = FrozenDict(
    {
        "nonneg": ConeKKT(_slack_separable, _residual_nonneg, _primal_nonneg),
        "soc": ConeKKT(_slack_separable, _residual_soc, _primal_soc),
        "psd": ConeKKT(_slack_frobenius, _residual_psd, _primal_psd),
    }
)


def _seed_arrays(fields: "Fields | None") -> tuple[np.ndarray, ...]:
    if fields is None:  # missing-backend key: program tag plus binds, no problem-data block
        return ()
    core = (fields.cost, *((fields.lin,) if fields.lin is not None else ()), fields.mat, fields.rhs)
    term_blocks = tuple(np.append(_as_mat(a).ravel(), bound) for a, bound in fields.terms)
    return (*core, *term_blocks)


def _convex_key(program: ConvexProgram, fields: "Fields | None", bind: FrozenDict[str, np.ndarray]) -> "RuntimeRail[ContentKey]":
    seed = (*_seed_arrays(fields), *(bind[name] for name in sorted(bind)))
    buffer = b"".join(np.ascontiguousarray(np.asarray(field, dtype=float)).tobytes() for field in seed)
    return ContentIdentity.of(f"convex.{program.tag}", buffer, IdentityPolicy())


def _symm(array: np.ndarray) -> np.ndarray:
    form = np.atleast_2d(np.asarray(array, dtype=float))
    return np.ascontiguousarray(0.5 * (form + form.T))


def _as_vec(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.asarray(array, dtype=float).ravel())


def _as_mat(array: np.ndarray) -> np.ndarray:
    return np.ascontiguousarray(np.atleast_2d(np.asarray(array, dtype=float)))
```

## [03]-[RESEARCH]

- [CVXPY_SOLVE]: `cvxpy` resolves on the companion `python_version<'3.15'` band (cp311-cp314 wheels only; no CPython 3.15 wheel); the `Variable`/`Parameter`/`Minimize`/`Maximize`/`Problem` modeling algebra, the `==`/`<=`/`>=`/`>>` relational constraint operators, the `cp.SOC` cone constructor and the `>>` PSD-cone relation over a `Variable(..., symmetric=True)` matrix leaf, the `quad_form`/`log_sum_exp`/`trace`/`vec`/`sum` DCP atoms, `Problem.is_dcp()`, `Problem.solve(solver=cp.CLARABEL, warm_start=True)`, `installed_solvers()`, `Problem.value`/`Problem.status`/`Problem.solver_stats`/`Variable.value`/`Constraint.dual_value` verify against `compute/.api/cvxpy.md` under a uv-sync reflection pass on that band — the catalogue is documentation-authored RESEARCH-capture-pending-on-uv-sync, not yet reflection-verified against an installed wheel, so the spellings settle on uv sync into the companion interpreter band. The semidefinite case carries the symmetric domain on the `Variable((n, n), symmetric=True)` leaf and the PSD membership as an explicit `X >> 0` cone row rather than a `PSD=True` leaf attribute, because `Constraint.dual_value` recovers the matrix dual `Z` only from an explicit cone row, never from a leaf-attribute domain — the dual-certificate requirement outranks the leaf-attribute style here; its polyhedral block reads `mat @ cp.vec(X) <= rhs` through the catalogued `cp.vec` vectorization. `Problem.is_dcp()` is the catalogued curvature-ruleset classification run **before** the solve, so a non-convex model folds to a `SolveStatus.OTHER` certificate rather than raising at `solve` time; the quadratic and semidefinite forms are symmetrized at the numpy edge (`0.5·(P+Pᵀ)`) before `cp.quad_form`/`cp.trace`, so a genuinely indefinite cost fails `is_dcp()` and `psd_wrap` is not in the catalogued atom tables and is never used. This owner carries no numpy floor because `cvxpy` over the conic backend *is* the certified capability, so a cp315 run without the wheel returns `Error(Import)` — the deliberate floor asymmetry against `optimization/design.md#DESIGN`.
- [CONVEX_ASSEMBLY_AND_SENSE]: the five cone families share one `_assemble` body plus the `_CONE_ROWS` `FrozenDict[str, ConeRow]` table, each `ConeRow` carrying the `objective(x, fields, cp)` expression builder, the `extra(x, fields, cp)` cone-rows builder, and the `psd` matrix-scaffold flag — so `linear`/`quadratic`/`second_order`/`exponential` share the polyhedral scaffold and differ by their objective closure and `cp.SOC`/`cp.log_sum_exp` extra rows, and `semidefinite` flips the `psd` flag to a `Variable((n, n), symmetric=True)` leaf, an explicit `X >> 0` cone row, and a `cp.vec` polyhedral block; the `_fields` `match` is the one place per-case problem data is read into the typed `Fields` value object (`cost`/`mat`/`rhs` always, `lin` for `quadratic`, `terms` for SOC/exponential) the closures and `_assemble` index by named attribute, so the shared body carries zero `program.tag` literal branch. The objective sign is the `Sense` `StrEnum` policy row folded through the `_SENSE` `FrozenDict[Sense, Callable]` table mapping each member to the catalogued `cp.Minimize`/`cp.Maximize` constructor via `operator.attrgetter`, so a concave-maximize program is a row on the one owner, never a parallel maximize-owner; DCP curvature still adjudicates convexity through `is_dcp()`, so a `Sense.MAX` over a convex objective is the construction rejection. The parameter axis declares a catalogued `cp.Parameter` leaf for each named problem-data slot referenced by the `ParamBind` rows (the inequality `rhs` is the canonical parametrizable surface), compiles the DPP `Problem` once, and warm-re-`solve`s across the bind table with `warm_start=True` reusing the prior factorization, folding one `ConvexReceipt` per bind keyed on the bound values through `ContentIdentity.of`; the no-bind default `_NO_BIND` declares no parameter and solves once. The exponential-cone case carries the `cp.log_sum_exp` atom (the catalogued exponential-cone-representable family) Clarabel solves through its `ExponentialConeT`, the fifth DCP cone family beside linear/quadratic/SOC/SDP. The entry preflights `cp.CLARABEL in cp.installed_solvers()` and degrades a missing backend to a `SolveStatus.OTHER` receipt rather than raising mid-fold.
- [CONVEX_STATUS_FOLD]: the `Problem.status` string `compute/.api/cvxpy.md` catalogues (`optimal`/`infeasible`/`unbounded`/inaccurate) is the cvxpy status-constant family — `optimal`, `optimal_inaccurate`, `infeasible`, `infeasible_inaccurate`, `unbounded`, `unbounded_inaccurate`, `infeasible_or_unbounded`, `solver_error`, `user_limit` — folded into the shared `SolveStatus` `StrEnum` `solvers/receipt.md#RECEIPT` owns through the local `_CONVEX_STATUS` boundary table, the convex sibling of the `_STATUS` `RESULTS` map: `optimal`->`SUCCESS`, `optimal_inaccurate`->`STAGNATION`, `infeasible`/`infeasible_inaccurate`/`infeasible_or_unbounded`->`INFEASIBLE`, `unbounded`/`unbounded_inaccurate`->`UNBOUNDED`, `solver_error`->`BREAKDOWN`, `user_limit`->`MAX_STEPS`, every unmapped constant degrading to `OTHER` rather than crashing. The `INFEASIBLE`/`UNBOUNDED` members are the feasibility-verdict termination classes this convex consumer adds to the shared vocabulary (the genuinely-new-termination-class growth path `solvers/receipt.md#RECEIPT` records), so `ConvexReceipt.status` reads the one `SolveStatus` enum the C# graduation gate and the sibling solve receipts read, never a parallel convex-only status string. The `ConvexReceipt.certified` predicate admits only on `SolveStatus.SUCCESS` with a duality gap within `_TOL`; `STAGNATION` (an inaccurate optimum), `INFEASIBLE`, `UNBOUNDED`, `BREAKDOWN`, and `MAX_STEPS` are admission rejections the `convex_program` `HandoffAxis` reads, never graduated handoffs.
- [CLARABEL_DUAL]: `clarabel` is the default conic interior-point backend `cvxpy` selects through `solver=cp.CLARABEL`; the primal-dual certificate — `DefaultSolution.z` conic dual multipliers and `DefaultSolution.s` primal slack — is surfaced to the modeling layer through the catalogued `Constraint.dual_value` multipliers and the primal `Variable.value`, and only the `iterations`/`solve_time` solve diagnostics reach the receipt through the catalogued `Problem.solver_stats.num_iters`/`solver_stats.solve_time`. The native Clarabel `DefaultSolution` carries `obj_val`, `obj_val_dual`, `r_prim`, and `r_dual` (verified on the upstream struct), but cvxpy's Clarabel reduction extracts only `obj_val`/`solve_time`/`iterations`/`x`/`z`/`status` and leaves `SolverStats.extra_stats` unset (`None`), persisting no raw `DefaultSolution` — so the Clarabel-internal `r_prim`/`r_dual` and `obj_val_dual` are unreachable through the cvxpy `solve` path this owner admits. The global-optimality gap the certificate folds is therefore the KKT complementary-slackness sum `Σ|⟨λᵢ, gᵢ(x)⟩|` over the recovered duals and `Constraint.expr.value` expression values (vanishing exactly at the global optimum), never an `obj_val − obj_val_dual` primal/dual objective difference cvxpy does not surface. The companion feasibility residuals are recomputed from the same surfaced KKT data through the `_CONE_KKT` table the `_cone` discriminant keys (`cp.SOC`/`cp.PSD` class patterns, polyhedral and exponential `log_sum_exp(...) <= b` inequality rows folding to `nonneg`): the dual-cone-membership distance scores the self-dual orthant `max(−λ, 0)` over a `nonneg` dual, the self-dual second-order cone `max(‖z₁:‖₂ − z₀, 0)` over an `SOC` dual through `np.linalg.norm` (a self-dual cone the elementwise sign test does not certify), and the self-dual PSD cone `max(−λ_min(½(Z+Zᵀ)), 0)` over the symmetrized PSD matrix dual through `np.linalg.eigvalsh`; the matching primal-cone-membership distance scores the same three cones over `Constraint.expr.value`, so the cone owns slack, dual feasibility, and primal feasibility on one cell — never a single elementwise sign test misread across the SOC or matrix cone. The `num_iters`/`solve_time` solve evidence folds beside those residuals into the `ConvexEvidence` value object. The spellings verify against `compute/.api/cvxpy.md`/`compute/.api/clarabel.md` once the Rust-native cp39-abi3 wheel syncs into the active venv on the companion band. The effective convex gate is `python_version<'3.15'` on **both** packages: the abi3-capable Clarabel wheel transitively pulls `scipy` (gast/pythran lack 3.15 support) and `cvxpy` ships cp311-cp314 wheels only, so the path is companion-band on the pair and never authored as cp315-runnable. The vanishing duality gap with feasible duals is the global-optimality proof object the `convex_program` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` reads to admit or reject the handoff — the convex analogue of the `Enclosure.certified` flag in `numerics/interval.md#ENCLOSURE`.
