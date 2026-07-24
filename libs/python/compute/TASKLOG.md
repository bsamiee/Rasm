# [PY_COMPUTE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[JAX_PROFILER_SURFACE]-[BLOCKED]: which jax profiling members are cataloged truth for the `[SOLVE_PROFILE_PARITY]` XLA leg?
- Capability: verdict on whether `jax.profiler.trace`, `jax.profiler.device_memory_profile`, `jax.profiler.save_device_memory_profile`, and `jax.stages.Lowered.cost_analysis` exist on the installed distribution, then the XLA leg of the `EngineProfile` band admits on those exact spellings beside the captured-jaxpr statistics the `xla` case already carries.
- Shape: member verdict landing as the repaired XLA-profiler roster on `libs/python/compute/.api/jax.md` and the pinned member row on the `xla` case of `libs/python/compute/.planning/numerics/jit.md`, admitting `jax.profiler.trace`, `device_memory_profile`, `save_device_memory_profile`, and `jax.stages.Lowered.cost_analysis` beside the captured-jaxpr statistics.
- Unlocks: IDEAS.md [SOLVE_PROFILE_PARITY] — the XLA leg of the `EngineProfile` band reaches trace-file and device-memory profiling parity with the llvm case, closing the deferred profiler gate.
- Anchors: resolution route — repair `libs/python/compute/.api/jax.md` against the installed distribution through `tools/assay`, then pin the members on `libs/python/compute/.planning/numerics/jit.md`; the folder `[PACKAGES]` ruling holding the gate intended.
- Arms: the upstream `jaxlib` wheel for the manifest's interpreter floor publishes and the interpreter marker lifts — an assay `unsupported` resolve is the intended gated state, never this card's trigger; each named member query must then return member evidence.

[RESOURCE_BRACKET_COMPOSE]-[QUEUED]: graduation resource sampling composes the substrate bracket.
- Capability: compute samples process cost through the one runtime owner instead of a folder-local psutil bracket.
- Shape: `_opened`/`_closed` and `ResourceUsage` on `libs/python/compute/.planning/graduation/observability.md` compose runtime `Cost.sampled`/`Cost.delta`; the `rasm.compute.evidence.*` names stay folder-local.
- Unlocks: one honest-RSS band and one sampling fix point across the branch.
- Anchors: runtime `Cost` on `libs/python/runtime/.planning/observability/receipts.md`; the substrate-bracket ruling at `libs/python/.planning/RULINGS.md`.
- Tension: `ResourceUsage.user_s`/`system_s` splits what `Cost.cpu_ms` sums — a consumer reading the split grows the substrate owner one user/system pair; an unread split dies with the bracket.
- Atomic: one bracket substitution on one page.

[EVIDENCE_SCOPE_GRAMMAR]-[QUEUED]: evidence scopes join the branch telemetry grammar.
- Capability: compute's span-scope members spell the one `rasm.`-rooted grammar every branch scope shares, so the handoff weave never forks the telemetry namespace.
- Shape: `EvidenceScope` members on `libs/python/compute/.planning/graduation/handoff.md` respell `rasm.compute.array`/`rasm.compute.mesh`, matching the `rasm.compute.*` ids the observability page already spells.
- Unlocks: one grep-true telemetry namespace across compute spans, hooks, and instruments.
- Anchors: the runtime scope-grammar ruling at `libs/python/runtime/RULINGS.md`; the id grammar on `libs/python/compute/.planning/graduation/observability.md`.
- Ripple: mirrors `runtime` `[SCOPE_GRAMMAR_GUARD]`.
- Atomic: two enum member respells and their consumers.

[SUBJECT_MIRROR_GATE]-[QUEUED]: the geometry subject mirror gains a drift gate.
- Capability: the decode-end subject roster stays provably aligned with geometry's owning union — a geometry member edit surfaces as a gate failure, never silent decode drift.
- Shape: a drift-gate row beside `GEOMETRY_SUBJECTS` on `libs/python/compute/.planning/graduation/handoff.md` — a seam-ledger row or generated projection over the subject wire literals geometry exports, per the branch descriptor-drift-gate pattern; the proven set also carries the `_GeometryWire` field roster (closing the rename-and-drop hole `forbid_unknown_fields` leaves) and the shared `rasm.link.kind` literal.
- Unlocks: the inherited contract block stops being standing ripple debt at the decode end.
- Anchors: `GEOMETRY_SUBJECTS` on the handoff page; the `GeometrySubject` union and `SUBJECTS` export on `libs/python/geometry/.planning/graduation.md`; the descriptor drift gate on `libs/python/runtime/.planning/transport/shapes.md`.
- Atomic: one gate row on one page.

[STIFF_ROWS]-[QUEUED]: land the `[DIFFERENTIAL_STIFF_POLICY]` rows on `libs/python/compute/.planning/solvers/differential.md`.
- Capability: one `ImplicitEuler` solver-table row and one `ClipStepSizeController` controller policy row, both resolved off the gated `dfx` carrier at solve time.
- Shape: one `ImplicitEuler` solver-table row and one `ClipStepSizeController` controller-policy row on `libs/python/compute/.planning/solvers/differential.md`, both resolved off the gated `dfx` carrier like every sibling row.
- Unlocks: IDEAS.md [DIFFERENTIAL_STIFF_POLICY] — robust integration of severely stiff problems where high-order SDIRK stages fail, and step clamping across event-adjacent integration windows.
- Anchors: folder `diffrax` catalog rows; the optimistix/lineax implicit-step seam the Kvaerno rows thread.
- Atomic: two table rows and their policy fields.

[CONE_BACKEND_ROWS]-[QUEUED]: land the `[CONVEX_BACKEND_FAMILY]` rows on `libs/python/compute/.planning/optimization/convex.md`.
- Capability: `PowCone3D` constraint row with its `args` dual-recovery read; SCS, HiGHS, and ProxSuite backend rows as policy values on the existing solve dispatch, each graded by the same KKT certificate.
- Shape: one `PowCone3D` constraint row and the SCS/HiGHS/ProxSuite backend rows on `libs/python/compute/.planning/optimization/convex.md`, backend choice one policy value on the existing solve dispatch, each arm graded by the same KKT certificate.
- Unlocks: IDEAS.md [CONVEX_BACKEND_FAMILY] — geometric-mean and power-utility programs, LP-heavy conic programs on HiGHS, and real-time QP re-solves on ProxSuite under DPP warm starts.
- Anchors: folder `cvxpy` catalog `PowCone3D` and the `ARGS`/`DUAL_VALUE` laws; the admitted `scs`/`highspy` manifest rows; `proxsuite` through the admission lane.

[LOO_SUBSAMPLE_ROW]-[QUEUED]: land the `[INFERENCE_SCALE_SCORING]` row on `libs/python/compute/.planning/experiments/inference.md`.
- Capability: `loo_subsample` with `update_subsample` refinement as one scoring row, a draw-count policy selecting it over the full fold; `ELPDData.kind` discriminates on read.
- Shape: one `loo_subsample`/`update_subsample` scoring row and its draw-count policy threshold on `libs/python/compute/.planning/experiments/inference.md`, `ELPDData.kind` discriminating the subsampled result on read.
- Unlocks: IDEAS.md [INFERENCE_SCALE_SCORING] — convergence-plus-fit graduation on posteriors whose full pointwise LOO cost is prohibitive.
- Anchors: folder `arviz` catalog LOO family; the residual-extractor table.
- Atomic: one scoring row and one policy threshold.

[GMSH_GENERATE_ARM]-[QUEUED]: land the `[MESH_GENERATION_ROUTE]` arm on `libs/python/compute/.planning/solvers/mesh.md`.
- Capability: generation route mapping gmsh physical groups onto named sets and gmsh element types onto `ElementKind`, emitting the same content-keyed `MeshField` the read arm mints.
- Shape: one `generate` route on `MeshExchange` beside read and write on `libs/python/compute/.planning/solvers/mesh.md`, mapping gmsh physical groups onto the named-set round-trip and gmsh element types onto `ElementKind`, emitting the same content-keyed `MeshField` the read arm mints.
- Unlocks: IDEAS.md [MESH_GENERATION_ROUTE] — end-to-end FEM studies from boundary description to graduated solve evidence with no external meshing step, and design loops that re-mesh per iteration.
- Anchors: `solvers/mesh#EXCHANGE` promote/recover round-trip; the `CTOR` table; the admitted root-manifest `gmsh` row.
- Tension: boundary input crosses as data; no geometry-branch kernel imports.
- Ripple: `geometry` `[GMSH_REGISTRY_ALIGN]`.

[DOE_DESIGN_ROWS]-[QUEUED]: land the `[STUDY_DESIGN_FAMILIES]` rows on `libs/python/compute/.planning/experiments/study.md`.
- Capability: factorial, fractional-factorial, Box-Behnken, central-composite, and Plackett-Burman rows whose `design` folds emit onto the sample-grid spine and whose `indices` return `{}`.
- Shape: factorial, fractional-factorial, Box-Behnken, central-composite, and Plackett-Burman `StudyMethod` rows and their design folds on `libs/python/compute/.planning/experiments/study.md`, each folding onto the sample-grid spine with `indices` returning `{}`.
- Unlocks: IDEAS.md [STUDY_DESIGN_FAMILIES] — response-surface and screening studies over solver routes, and `RunHistory` comparison across design families under provably equal content keys.
- Anchors: `experiments/study#STUDY` `StudyMethod` route table; the admitted root-manifest `pyDOE3` row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOK_POINT_ROWS]-[COMPLETE]: landed as the derived `COMPUTE_POINTS` table over `SCOPE_DOMAIN` with the closed payload family and the `registered` composition gate on `libs/python/compute/.planning/graduation/observability.md`.
[HOOK_TAP_PROJECTIONS]-[COMPLETE]: landed as the `_measures` polymorphic projection and the `tapped` per-point receipt/metric subscription fold on `libs/python/compute/.planning/graduation/observability.md`; the `rasm.compute.*` instrument rows remain a runtime `observability/metrics#METRIC` ripple.
[RESOURCE_SAMPLE_FOLD]-[COMPLETE]: landed as the two-block `oneshot` bracket (`_opened`/`_closed`) and `resource_sampled` on `libs/python/compute/.planning/graduation/observability.md`; the settled band records before/after RSS — cross-platform `pmem` carries no peak field — superseding this card's peak-RSS claim.
[PROFILE_BAND_FIELDS]-[COMPLETE]: landed as `EngineProfile` with the `_profiled` dispatcher harvest and `_printed_lines` tally on `libs/python/compute/.planning/numerics/jit.md`, mounted as the optional per-case `profile` slot with the `profile.`-namespaced spread on `libs/python/compute/.planning/solvers/receipt.md`.
[STUDY_BENCH_FOLD]-[COMPLETE]: landed as `StudyReceipt.benched` feeding `BenchmarkReceipt.of` from held measurements — `.serial` baseline series under SPEEDUP, zero-elapsed RESULT suppression — composed into the contributor harvest on `libs/python/compute/.planning/experiments/study.md`.
[TRACE_LINK_DECODE]-[COMPLETE]: wire widened and decoded — `_GeometryWire.trace` carries the optional `traceparent`/`tracestate`/baggage mapping, `GraduationReceipt.geometry` threads it into `graduates`, and `_linked` folds the valid extracted context as one `Link` on `libs/python/compute/.planning/graduation/handoff.md`.
