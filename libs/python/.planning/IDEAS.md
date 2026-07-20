# [PYTHON_IDEAS]

Cross-package Python concert: higher-order concepts coupling two or more branch packages or deepening a shared owner the whole branch inherits, distilled from the folder pools. A concept grounded in one folder stays in that folder's pool; a concept crossing a language boundary lives in the cross-`libs/` pool and is referenced as a wire seam, never restated. Each idea is a card — a bracketed slug leader with the capability, what it unlocks, and the gap it draws on.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[HANDOFF_TRACE_WIRE]-[QUEUED]: content-keyed crossing wires gain an optional W3C trace band, ruled once and co-shipped across producers and consumers.
- Capability: one branch ruling widens `GeometryHandoff.wire()` with ONE byte-level representation — an optional serialized W3C `traceparent` string beside the `ContentKey`, absent meaning no link, decoded at the consumer into a `SpanContext` and folded as a `Link` — so a consumer span joins its upstream producer trace and cross-producer click-through exists on every graduation crossing; counterpart cards spell this exact shape, never a raw `SpanContext` field on the wire.
- Shape: one wire verdict resolved against the compute handoff decode owner and the cross-libs `[UNIFIED_SIGNAL_FABRIC]` frozen-wire law, then a geometry mint arm and a compute decode arm co-ship; landing files `libs/python/geometry/.planning/graduation.md` and `libs/python/compute/.planning/graduation/handoff.md`.
- Unlocks: unblocks `geometry` `[EVIDENCE_TRACE_LINKS]` and `compute` `[EVIDENCE_TRACE_LINKS]`; backend metric-to-trace-to-upstream-trace navigation across the producer chain.
- Anchors: `Link`/`Span.add_link`/`SpanContext` on `libs/python/.api/opentelemetry-api.md`; both folder BLOCKED cards naming this ruling as their resolution route.
- Tension: data's landed observability ruling keeps `add_link` off the data planes — co-ship scope is geometry and compute only, and the wire stays frozen until the cross-libs ruling admits the widened band.
- Ripple: `geometry` `[EVIDENCE_TRACE_LINKS]`.

[PULSE_DRAIN_ACTOR]-[QUEUED]: parent-side serialized pulse drain on the lane spine — long-running kernels stream mid-operation facts through one mailbox actor.
- Capability: worker kernels pulse progress, convergence, and phase facts through a lane conduit a parent-side `MailboxProcessor` drains serially into `Hooks.fire` on the emitter's registry, so mid-operation taps exist for every folder's HOSTILE kernels without in-worker registry reach.
- Shape: one drain actor per lane on the runtime lane spine — conduit selection resolved against the `pebble` map-iterator and pipe members, mailbox fold posting each pulse as a hook fact — landing on `libs/python/runtime/.planning/execution/lanes.md`; folder pulse vocabularies stay folder-owned rows.
- Unlocks: unblocks `geometry` `[MID_OPERATION_PULSE]`; live tessellation, solve, and query progress for the C# rail and the TS viewer without polling receipts.
- Anchors: `MailboxProcessor` on `libs/python/.api/expression.md`; `libs/python/.api/pebble.md` member truth; runtime `observability/hooks#HOOKS` registry and taps.
- Tension: drain stays parent-side and serialized — a worker kernel writes to the conduit only, and the geometry blocked pair holds until the drain verdict lands.
- Ripple: `geometry` `[MID_OPERATION_PULSE]`.

[EXEMPLAR_CLICKTHROUGH]-[QUEUED]: every branch histogram point carries a trace exemplar, so a dashboard spike clicks through to its exact trace.
- Capability: trace-based exemplar sampling on the shared meter build attaches span context to every recorded `rasm.<domain>.<measure>` measurement across the five folder domains with zero producer edits, realizing the estate wire law's trace-based-exemplar ruling on the python leg.
- Shape: one `TraceBasedExemplarFilter` row at meter-provider construction beside the histogram-aggregation default, landing on `libs/python/runtime/.planning/observability/telemetry.md`; exemplars ride the standing OTLP egress.
- Unlocks: metric-to-trace click-through on every dashboard panel the branch feeds; exemplar parity with the C# and TS legs.
- Anchors: `sdk.metrics.TraceBasedExemplarFilter` on `libs/python/.api/opentelemetry-sdk.md`; the meter build on runtime `observability/telemetry#TELEMETRY`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed on the runtime telemetry install — `opentelemetry-processor-baggage` admitted, the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction, producer pages untouched.

[WORKER_TELEMETRY_CONTINUITY]-[DROPPED]: collapsed into `runtime` `[WORKER_TELEMETRY_PARENTING]` (task grain: `runtime` `[WORKER_INSTALL_SEAM]`) — the folder owner carries the worker-side install, context crossing, and flush law at per-arm grain with exact landing pages; the branch card held no work the folder owner lacks.
