# [PY_COMPUTE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[HOOK_POINT_ROWS]-[QUEUED]: land the `[COMPUTE_HOOK_RAIL]` point vocabulary and its page `libs/python/compute/.planning/graduation/observability.md`.
- Capability: point-row table `rasm.compute.<domain>.<point>` — solve dispatch enter/exit, graduation admit and reject, study design evaluation, jit compile — each with its closed msgspec payload `Struct` and modality.
- Anchors: runtime `observability/hooks#HOOKS` `HookPoint` row grammar and composition-unique registry; `graduation/handoff#EVIDENCE_WEAVE` producer binding.
- Tension: libraries register points only; subscriber attachment stays at app composition.

[HOOK_TAP_PROJECTIONS]-[QUEUED]: pin the `[COMPUTE_HOOK_RAIL]` measure mapping from hook payloads to `rasm.compute.<domain>.<measure>` rows.
- Capability: per-payload numeric-measure mapping rows the `tap_metrics` projection reads — UCUM unit, kind, and domain per measure — on `libs/python/compute/.planning/graduation/observability.md`.
- Anchors: runtime `observability/hooks#HOOKS` `tap_metrics(measures, domain, kind)`; runtime `observability/metrics#METRIC` `Metrics.record`.
- Atomic: measure-mapping rows on the new observability page.

[RESOURCE_SAMPLE_FOLD]-[QUEUED]: pin the `[SOLVE_RESOURCE_LEDGER]` band and its sampling fold on `libs/python/compute/.planning/graduation/observability.md`.
- Capability: one sampling fold wrapping a measured kernel — a `Process.oneshot()` block reading `cpu_times()` and `memory_info()` before and after — folding user and system deltas with peak RSS beside wallclock into `ResourceUsage` facts on the weave.
- Anchors: branch `psutil` catalog `Process.oneshot()`, `Process.cpu_times() -> pcputimes`, `Process.memory_info() -> pmem`; the `_timed` fold precedent on `experiments/study#STUDY`.
- Tension: the fold reads the worker's own process handle inside the lane; cross-process aggregation stays the runtime lanes owner's.

[PROFILE_BAND_FIELDS]-[QUEUED]: pin the `[SOLVE_PROFILE_PARITY]` band fields on `libs/python/compute/.planning/numerics/jit.md` and `libs/python/compute/.planning/solvers/receipt.md`.
- Capability: numba dispatcher harvest — `signatures`, `parallel_diagnostics`, `inspect_types`/`inspect_asm` extents — beside XLA lowered-IR statistics off the captured IR, one `Struct` both pages share.
- Anchors: folder `numba` catalog `CPUDispatcher` inspect surface; `numerics/jit#JIT` `Capture` rows.

[JAX_PROFILER_SURFACE]-[BLOCKED]: which jax profiling members are cataloged truth for `[SOLVE_PROFILE_PARITY]`?
- Capability: verdict on whether `jax.profiler` trace and device-memory members and lowered `cost_analysis` exist on the installed distribution, then the XLA leg of the profile band admits on those exact spellings.
- Anchors: resolution route — repair `libs/python/compute/.api/jax.md` against the installed distribution through `tools/assay`, then pin the members on `libs/python/compute/.planning/numerics/jit.md`.

[STIFF_ROWS]-[QUEUED]: land the `[DIFFERENTIAL_STIFF_POLICY]` rows on `libs/python/compute/.planning/solvers/differential.md`.
- Capability: one `ImplicitEuler` solver-table row and one `ClipStepSizeController` controller policy row, both resolved off the gated `dfx` carrier at solve time.
- Anchors: folder `diffrax` catalog rows; the optimistix/lineax implicit-step seam the Kvaerno rows thread.
- Atomic: two table rows and their policy fields.

[CONE_BACKEND_ROWS]-[QUEUED]: land the `[CONVEX_BACKEND_FAMILY]` rows on `libs/python/compute/.planning/optimization/convex.md`.
- Capability: `PowCone3D` constraint row with its `args` dual-recovery read; SCS, HiGHS, and ProxSuite backend rows as policy values on the existing solve dispatch, each graded by the same KKT certificate.
- Anchors: folder `cvxpy` catalog `PowCone3D` and the `ARGS`/`DUAL_VALUE` laws; admission lane rows for `scs`, `highspy`, `proxsuite`.

[LOO_SUBSAMPLE_ROW]-[QUEUED]: land the `[INFERENCE_SCALE_SCORING]` row on `libs/python/compute/.planning/experiments/inference.md`.
- Capability: `loo_subsample` with `update_subsample` refinement as one scoring row, a draw-count policy selecting it over the full fold; `ELPDData.kind` discriminates on read.
- Anchors: folder `arviz` catalog LOO family; the residual-extractor table.
- Atomic: one scoring row and one policy threshold.

[GMSH_GENERATE_ARM]-[QUEUED]: land the `[MESH_GENERATION_ROUTE]` arm on `libs/python/compute/.planning/solvers/mesh.md`.
- Capability: generation route mapping gmsh physical groups onto named sets and gmsh element types onto `ElementKind`, emitting the same content-keyed `MeshField` the read arm mints.
- Anchors: `solvers/mesh#EXCHANGE` promote/recover round-trip; the `CTOR` table; admission lane row for `gmsh`.
- Tension: boundary input crosses as data; no geometry-branch kernel imports.

[DOE_DESIGN_ROWS]-[QUEUED]: land the `[STUDY_DESIGN_FAMILIES]` rows on `libs/python/compute/.planning/experiments/study.md`.
- Capability: factorial, fractional-factorial, Box-Behnken, central-composite, and Plackett-Burman rows whose `design` folds emit onto the sample-grid spine and whose `indices` return `{}`.
- Anchors: `experiments/study#STUDY` `StudyMethod` route table; admission lane row for `pyDOE3`.

[STUDY_BENCH_FOLD]-[QUEUED]: land the `Measured`-to-bench projection on `experiments/study.md`.
- Capability: one fold from `Measured` wallclock and speedup to the runtime bench contribution, subject-keyed per objective.
- Anchors: `experiments/study#STUDY`; runtime `observability/profiles#BENCH`; the `rasm.bench.*` rows.
- Tension: observability evidence only — no graduation verdict rides it; `RESULT` mode suppresses the contribution with its zero elapsed.
- Atomic: one projection fold and its card fields.

[TRACE_LINK_DECODE]-[BLOCKED]: does `GeometryHandoff.wire()` widen to carry a serialized `SpanContext` for `[EVIDENCE_TRACE_LINKS]`?
- Capability: once geometry lands the widened wire, decode the optional `SpanContext` beside the `ContentKey` and fold it as a `Link` at the `evidence_run` span open on `libs/python/compute/.planning/graduation/handoff.md`.
- Anchors: resolution route — the geometry origin card `[EVIDENCE_TRACE_LINKS]` on `libs/python/geometry/IDEAS.md`; `Link`/`Span.add_link` on the branch `opentelemetry-api` catalog.
- Ripple: `geometry` `[EVIDENCE_TRACE_LINKS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
