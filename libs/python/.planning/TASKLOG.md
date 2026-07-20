# [PYTHON_TASKLOG]

Cross-package Python work distilled from `IDEAS.md`: tasks coupling two or more packages or landing on a shared runtime owner the whole branch inherits. Per-folder work lives in the owning folder's pool; cross-language work lives in the cross-`libs/` pool. Each card uses an `[ID]-[STATUS]:` leader with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension`.

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

[WIRE_TRACE_RULING]-[QUEUED]: resolve the crossing-wire trace band and land the co-ship pair.
- Capability: verdict on the widened `GeometryHandoff.wire()` band against the compute decode owner and the cross-libs frozen-wire law; on admit, the geometry mint arm and the compute decode arm land and both folder `[EVIDENCE_TRACE_LINKS]` cards unblock.
- Shape: verdict recorded on both folder BLOCKED cards, mint and decode arms on `libs/python/geometry/.planning/graduation.md` and `libs/python/compute/.planning/graduation/handoff.md`.
- Unlocks: cross-producer trace links on every graduation crossing.
- Anchors: cross-libs `[UNIFIED_SIGNAL_FABRIC]` frozen-wire law; `SpanContext`/`Link` on `libs/python/.api/opentelemetry-api.md`; idea `[HANDOFF_TRACE_WIRE]`.

[PULSE_CONDUIT_DESIGN]-[QUEUED]: design the mailbox drain and its worker conduit on the lane spine.
- Capability: conduit member verdict — `pebble` map-iterator against pipe conduit — the per-lane `MailboxProcessor` drain fold posting hook facts, and the back-pressure law.
- Shape: drain-actor rows on `libs/python/runtime/.planning/execution/lanes.md`; the verdict answers geometry's `[PULSE_DRAIN_PROBE]`.
- Unlocks: mid-operation pulse taps for every folder's long-running kernels.
- Anchors: `libs/python/.api/expression.md` `MailboxProcessor`; `libs/python/.api/pebble.md`; runtime `observability/hooks#HOOKS`; idea `[PULSE_DRAIN_ACTOR]`.

[EXEMPLAR_FILTER_ROW]-[QUEUED]: land the trace-based exemplar row on the meter build.
- Capability: `TraceBasedExemplarFilter` at meter-provider construction on `libs/python/runtime/.planning/observability/telemetry.md`, beside the exponential-aggregation default row.
- Anchors: `libs/python/.api/opentelemetry-sdk.md`; idea `[EXEMPLAR_CLICKTHROUGH]`.
- Atomic: one exemplar-filter row on one existing page.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed as the `_tracer_provider`/`_log_attach` promotion pair on `runtime/observability/telemetry#TELEMETRY` — `PROMOTED_BAGGAGE` closed predicate, registration before batch attach and global set, research row resolved against the installed distribution.

[WORKER_TELEMETRY_CONTINUITY]-[DROPPED]: collapsed into `runtime` `[WORKER_INSTALL_SEAM]` (idea grain: `runtime` `[WORKER_TELEMETRY_PARENTING]`) — the per-arm install-seam task carries the same scope with exact landing pages.
