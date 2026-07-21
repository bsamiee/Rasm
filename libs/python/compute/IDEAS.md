# [PY_COMPUTE_IDEAS]

Forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[MESH_GENERATION_ROUTE]-[QUEUED]: compute generates its own simulation meshes instead of only reading them.
- Capability: gmsh-backed unstructured 1/2/3-D mesh generation with physical groups, size fields, and boundary input, landing as a generation arm on `MeshExchange` beside read and write — generated meshes flow straight into the `CTOR` element table, weak-form assembly, and the meshio round-trip.
- Shape: one `generate` route on `libs/python/compute/.planning/solvers/mesh.md` mapping gmsh physical groups onto the named-set round-trip and gmsh element types onto `ElementKind`.
- Unlocks: end-to-end FEM studies from boundary description to graduated solve evidence with no external meshing step; design loops that re-mesh per iteration.
- Anchors: `solvers/mesh#EXCHANGE` promote/recover round-trip; the `CTOR` table; the admitted root-manifest `gmsh` row.
- Tension: geometry tessellation, registration, and topology stay the geometry branch's; FEM discretization of a simulation domain is compute's charter — the arm consumes boundary input as data and imports no geometry kernel.

[STUDY_DESIGN_FAMILIES]-[QUEUED]: classical experiment designs join the study spine as method rows.
- Capability: factorial, fractional-factorial, Box-Behnken, central-composite, and Plackett-Burman design generation as `StudyMethod` rows whose `indices` fold returns `{}` — sampling-only members on the existing axis admitting the multi-output objective shape.
- Shape: new method rows and their design folds on `libs/python/compute/.planning/experiments/study.md`, marginals resolved through the same per-axis parameterization the SALib channel reads.
- Unlocks: response-surface and screening studies over solver routes; `RunHistory` comparison across design families under provably equal content keys.
- Anchors: `experiments/study#STUDY` `StudyMethod` axis and sample-grid spine; the admitted root-manifest `pyDOE3` row.

[CONVEX_BACKEND_FAMILY]-[QUEUED]: the conic backend axis completes and the power cone becomes a first-class constraint row.
- Capability: `PowCone3D` membership joins the cone-constraint family, and SCS, HiGHS, and ProxSuite land as selectable solve-backend rows beside the Clarabel arm — every backend recovering the same primal/dual pair the KKT certificate grades, so backend choice never weakens the proof.
- Shape: one cone row and the backend rows on `libs/python/compute/.planning/optimization/convex.md`, backend selection one policy value on the existing solve dispatch.
- Unlocks: geometric-mean and power-utility programs; LP-heavy conic programs on HiGHS; real-time QP re-solves on ProxSuite under DPP warm starts.
- Anchors: folder `cvxpy` catalog `PowCone3D`, multi-backend `solve`, and the `ARGS`/`DUAL_VALUE` recovery laws; `optimization/convex#CONVEX` dual-certificate fold; the admitted `scs` and `highspy` manifest rows; `proxsuite` through the admission lane.

[DIFFERENTIAL_STIFF_POLICY]-[QUEUED]: the stiff route gains its implicit floor and bounded step control.
- Capability: `ImplicitEuler` joins the solver table as the order-1 DIRK floor beneath the Kvaerno family, and `ClipStepSizeController` lands as a wrapping-controller policy row bounding any adaptive controller's steps to a range.
- Shape: one solver row and one controller policy row on `libs/python/compute/.planning/solvers/differential.md`, both resolved off the gated `dfx` carrier like every sibling row.
- Unlocks: robust integration of severely stiff problems where high-order SDIRK stages fail; step clamping across event-adjacent integration windows.
- Anchors: folder `diffrax` catalog `ImplicitEuler` (implicit DIRK) and `ClipStepSizeController` (wrapping controller); the optimistix/lineax implicit-step seam the Kvaerno rows already thread.

[INFERENCE_SCALE_SCORING]-[QUEUED]: posterior scoring stays tractable at large draw counts.
- Capability: subsampled PSIS-LOO — `loo_subsample` with `update_subsample` refinement — as a scoring row beside the full `loo` fold, selected by draw-count policy so a large posterior grades predictive fit without the full pointwise matrix.
- Shape: one scoring row and its policy threshold on `libs/python/compute/.planning/experiments/inference.md`; `ELPDData.kind` discriminates the subsampled result on read.
- Unlocks: convergence-plus-fit graduation on posteriors whose full LOO cost is prohibitive.
- Anchors: folder `arviz` catalog LOO family (`loo_subsample`/`update_subsample`, `ELPDData.kind`); `experiments/inference#BAYESIAN` residual table.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[COMPUTE_HOOK_RAIL]-[COMPLETE]: landed as `COMPUTE_POINTS`/`SCOPE_DOMAIN` with the `registered`/`tapped` legs and the `_measures` projection on `libs/python/compute/.planning/graduation/observability.md`; the `rasm.compute.*` instrument rows remain a runtime `observability/metrics#METRIC` ripple.
[SOLVE_RESOURCE_LEDGER]-[COMPLETE]: landed as `ResourceUsage` with the two-block `oneshot` bracket and the `ledgered` hub weave on `libs/python/compute/.planning/graduation/observability.md`; the settled band records the before/after RSS pair — cross-platform `pmem` carries no peak field — superseding this card's peak-RSS claim.
[SOLVE_PROFILE_PARITY]-[COMPLETE]: landed as the `EngineProfile` band on the `JitEvidence` llvm case with the `_profiled` dispatcher harvest on `libs/python/compute/.planning/numerics/jit.md` and the optional per-case `profile` slot on `libs/python/compute/.planning/solvers/receipt.md`; the XLA leg carries captured-jaxpr statistics, its profiler members gated behind `TASKLOG` `[JAX_PROFILER_SURFACE]`.
[STUDY_BENCH_PROJECTION]-[COMPLETE]: landed as `StudyReceipt.benched` — `BenchmarkReceipt.of` fed from held measurements with the `.serial` baseline series under SPEEDUP — riding the contributor harvest on `libs/python/compute/.planning/experiments/study.md`.
[EVIDENCE_TRACE_LINKS]-[COMPLETE]: landed as `_GeometryWire.trace` decoding the optional W3C composite mapping and `_linked` folding its valid `SpanContext` under `rasm.link.kind: geometry-graduation` on `libs/python/compute/.planning/graduation/handoff.md`, co-shipped with the geometry mint on `libs/python/geometry/.planning/graduation.md`.
