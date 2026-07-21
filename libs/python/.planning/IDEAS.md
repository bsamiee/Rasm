# [PYTHON_IDEAS]

Cross-package Python concert: higher-order concepts coupling two or more branch packages or deepening a shared owner the whole branch inherits, distilled from the folder pools. A concept grounded in one folder stays in that folder's pool; a concept crossing a language boundary lives in the cross-`libs/` pool and is referenced as a wire seam, never restated. Each idea is a card — a bracketed slug leader with the capability, what it unlocks, and the gap it draws on.

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

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HANDOFF_TRACE_WIRE]-[COMPLETE]: landed as one optional W3C composite carrier — `GeometryHandoff.of`/`_trace`/`wire()` mint `traceparent`, `tracestate`, and baggage on `libs/python/geometry/.planning/graduation.md`; `_GeometryWire.trace`/`_linked` decode the same mapping on `libs/python/compute/.planning/graduation/handoff.md` and fold one `Link` under `rasm.link.kind: geometry-graduation`.
[PULSE_DRAIN_ACTOR]-[COMPLETE]: landed on `libs/python/runtime/.planning/execution/lanes.md` — `LanePolicy.of` opens `pulses`, `drain` lifecycle-binds its actor, the manager-queue pump re-enters through `anyio.from_thread.run_sync`, `_pulse_fold` posts `Hooks.fire`, and non-blocking close preserves the lossy telemetry law; `MailboxProcessor` and the `pebble` map iterator remain refuted.
[EXEMPLAR_CLICKTHROUGH]-[COMPLETE]: landed as `exemplar_filter=TraceBasedExemplarFilter()` in `_meter_provider` on `libs/python/runtime/.planning/observability/telemetry.md`, beside the `WIRE_AGGREGATION` base2-exponential histogram default; exemplars ride the standing OTLP egress.
[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed on the runtime telemetry install — `opentelemetry-processor-baggage` admitted, the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction, producer pages untouched.
[WORKER_TELEMETRY_CONTINUITY]-[DROPPED]: collapsed into `runtime` `[WORKER_TELEMETRY_PARENTING]` (task grain: `runtime` `[WORKER_INSTALL_SEAM]`) — the folder owner carries the worker-side install, context crossing, and flush law at per-arm grain with exact landing pages; the branch card held no work the folder owner lacks.
