# [PYTHON_TASKLOG]

Cross-package Python work distilled from `IDEAS.md`: tasks coupling two or more packages or landing on a shared runtime owner the whole branch inherits. Per-folder work lives in the owning folder's pool; cross-language work lives in the cross-`libs/` pool. Each card uses an `[ID]-[STATUS]:` leader with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension`.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[LAZY_IMPORT_DIALECT]-[QUEUED]: import deferral converges on the lazy proxy dialect.
- Capability: one deferral idiom across the branch — a function-local import survives only where module-scope import is banned.
- Shape: unearned `# ruff:ignore[import-outside-top-level]` function-local imports reclassify to the `lazy` module-top proxy across the data, geometry, and artifacts pages; each survivor names its module-scope ban.
- Unlocks: the branch deferral ruling realized; floor audits read one module-top roster.
- Anchors: the deferral ruling at `libs/python/.planning/RULINGS.md`; the `lazy` DSL owner; the manifest ban comments.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WIRE_TRACE_RULING]-[COMPLETE]: carrier admitted and co-shipped — `GeometryHandoff.of`/`_trace`/`wire()` mint the full W3C composite mapping on `libs/python/geometry/.planning/graduation.md`; `_GeometryWire.trace`/`GraduationReceipt.geometry`/`_linked` decode it on `libs/python/compute/.planning/graduation/handoff.md`; both folder `[EVIDENCE_TRACE_LINKS]` cards close against those fences.
[PULSE_CONDUIT_DESIGN]-[COMPLETE]: conduit and actor lifecycle landed on `libs/python/runtime/.planning/execution/lanes.md` — `LanePolicy.pulses`, structured `drain` custody, spawn-context manager queue, `anyio.from_thread.run_sync` relay, serialized `Hooks.fire` fold, and non-blocking close enforce the lossy law; `pebble` map iterator and `MailboxProcessor` remain refuted, geometry `[PULSE_DRAIN_PROBE]` answered.
[EXEMPLAR_FILTER_ROW]-[COMPLETE]: landed as the `exemplar_filter=TraceBasedExemplarFilter()` argument in `_meter_provider` on `libs/python/runtime/.planning/observability/telemetry.md`, beside the `WIRE_AGGREGATION` exponential-aggregation default row.
[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed as the `_tracer_provider`/`_log_attach` promotion pair on `runtime/observability/telemetry#TELEMETRY` — `PROMOTED_BAGGAGE` closed predicate, registration before batch attach and global set, research row resolved against the installed distribution.
[WORKER_TELEMETRY_CONTINUITY]-[DROPPED]: collapsed into `runtime` `[WORKER_INSTALL_SEAM]` (idea grain: `runtime` `[WORKER_TELEMETRY_PARENTING]`) — the per-arm install-seam task carries the same scope with exact landing pages.
