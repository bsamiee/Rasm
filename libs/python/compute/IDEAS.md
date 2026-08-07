# [PY_COMPUTE_IDEAS]

Forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. Ideas drive one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[STUDY_DESIGN_FAMILIES]-[COMPLETE]: landed via TASKLOG [DOE_DESIGN_ROWS] — four coded `StudyMethod` rows (`fractional`/`box_behnken`/`central_composite`/`plackett_burman`) on the shared `_unit`/`_box` bounds map with `indices` `{}`; the two-level factorial member folded into the existing `factorial` grid rather than a duplicate row, and the linear-onto-bounds law for every coded design landed at `RULINGS.md` `[02]`.
[CONVEX_BACKEND_FAMILY]-[COMPLETE]: landed via TASKLOG [CONE_BACKEND_ROWS] — the `power` cone case and the CLARABEL/SCS/HIGHS `_BACKEND` rows under the one KKT certificate, plus the live-proved `SCIPY_CANON_BACKEND` pin; the ProxSuite arm is REFUTED at the interpreter floor (no cp315 wheel, `cmeel` build backend dies on py3.15), recorded at `RULINGS.md` `[01]` with its wheel re-open condition.
[DIFFERENTIAL_STIFF_POLICY]-[COMPLETE]: landed via TASKLOG [STIFF_ROWS] — `IMPLICIT_EULER` as the adaptive order-1 A-B-L-stable DIRK floor and the clip triple; this card's `ClipStepSizeController` steps-to-a-range claim is REFUTED against source — the wrap owns forced step times, jump discontinuities, and SDE rejected-step revisits, while range bounding stays `dtmin`/`dtmax` on the PID row.
[INFERENCE_SCALE_SCORING]-[COMPLETE]: landed via TASKLOG [LOO_SUBSAMPLE_ROW] — the `_score` fold behind the `loo_cells` budget with one `update_subsample` refinement; this card's `ELPDData.kind` discrimination claim is REFUTED live (`loo_subsample` keeps `kind == "loo"`), the typed `subsample_size`/`subsampling_se` pair discriminating instead.
[MESH_GENERATION_ROUTE]-[COMPLETE]: landed as the `generate` arm on `MeshExchange` over the `GmshSource` boundary-input axis and the `SizeField` density axis on `libs/python/compute/.planning/solvers/mesh.md`, minting its `MeshField` through the SAME `_read` fold every inbound mesh crosses so groups, cell block, and content key derive identically; the card's "gmsh element types map onto `ElementKind`" clause is REFUTED and its opposite landed — routing extraction through the `.msh`/meshio round-trip is what keeps `CTOR` the one element vocabulary, where an element-type-integer table would mint a second, and the high-order promotion the card implied is unbuildable because `setOrder(2)` writes `triangle6`/`tetra10` blocks neither `cells_dict[CTOR[element].cell]` nor the affine `Mesh*1` constructors can take.
[COMPUTE_HOOK_RAIL]-[COMPLETE]: landed as `COMPUTE_POINTS`/`SCOPE_DOMAIN` with the composition-scoped `registered`/`tapped` legs and the `_measures` projection on `libs/python/compute/.planning/graduation/observability.md`; the `rasm.compute.*` instrument rows and the `domain="compute"` roster row remain a runtime `runtime/observability/metrics#METRIC` ripple, proven at composition by the `registered` census gate.
[SOLVE_RESOURCE_LEDGER]-[COMPLETE]: landed as `ResourceUsage` over the runtime `Cost` substrate bracket inside the `ledgered` hub weave on `libs/python/compute/.planning/graduation/observability.md`, banding cpu, rss, io, and switches with a settled-or-raised outcome; the band records a signed RSS delta — cross-platform `pmem` carries no peak field — superseding this card's peak-RSS claim.
[SOLVE_PROFILE_PARITY]-[COMPLETE]: parity reached on both engines — `EngineProfile` is the engine-neutral compile-extent band on `libs/python/compute/.planning/numerics/jit.md` that the `llvm` case fills off the `_profiled` dispatcher harvest and the `xla` case fills off the `_xla_profiled` staging-ladder harvest, mounted through the optional per-case `profile` slot on `libs/python/compute/.planning/solvers/receipt.md`; `TraceEvidence` carries the device-timeline evidence a host compiler cannot answer, riding the `xla` case alone rather than a band column every engine zero-fills.
[STUDY_BENCH_PROJECTION]-[COMPLETE]: landed as `StudyReceipt.benched` — `BenchmarkReceipt.of` fed from held measurements with the `.serial` baseline series under SPEEDUP — riding the contributor harvest on `libs/python/compute/.planning/experiments/study.md`.
[EVIDENCE_TRACE_LINKS]-[COMPLETE]: landed as `_GeometryWire.trace` decoding the optional W3C composite mapping and `_linked` folding its valid `SpanContext` under `rasm.link.kind: geometry-graduation` on `libs/python/compute/.planning/graduation/handoff.md`, co-shipped with the geometry mint on `libs/python/geometry/.planning/graduation.md`.
