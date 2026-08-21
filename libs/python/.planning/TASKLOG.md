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
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CONTENT_KEY_PAYLOAD_FAMILY]-[COMPLETE]: the fold ruled WRONG and both ends landed — `BareValue` on `libs/python/runtime/.planning/evidence/identity.md` declares one union arm per `IdentitySource` case as page `- Law:`, `lift` decides every tuple at its own arms so a mixed one reaches `assert_never` instead of mis-keying as a chunk stream, and the two geometry producers hand `IdentitySource(parts=...)` with each key lowered through `ContentKey.memory` (`scan/registration` `keyed()`, `scan/deviation` `scanned`). No arm ever keyed a mixed `(ContentKey, bytes)` tuple: `stream` means buffer chunks of ONE payload, so `digest.update` took a tuple and raised a bare `TypeError` outside every rail, and a `canonical` `Struct` preimage escapes nothing — msgspec raises `OverflowError` on the u128 `ContentKey.value` before `enc_hook`, outside the `EncodeError` the rail catches.
[LAZY_IMPORT_DIALECT]-[COMPLETE]: swept branch-wide — data 96→2 survivors, geometry 81→42, compute 129 indented→29 carrier sites under the `Engine.gated()` x64 seam law at `libs/python/compute/RULINGS.md` `[04]`, runtime recipe 7 AGPL sites — every survivor naming its module-scope ban under the two rows the card's citation lacked, landed at `libs/python/.planning/RULINGS.md` `[04]` (the manifest roster bans the EAGER form alone; survivors are AGPL/network-copyleft lexical isolation and call-seam side effects) with the doctrine carve at `docs/stacks/python`; live 3.15 probes refuted the `lazy import pkg.sub` sibling-collision claim and proved TID253 reads `lazy` as module-level, recorded at the manifest roster comment.
[WIRE_TRACE_RULING]-[COMPLETE]: carrier admitted and co-shipped — `GeometryHandoff.of`/`_trace`/`wire()` mint the full W3C composite mapping on `libs/python/geometry/.planning/graduation.md`; `_GeometryWire.trace`/`GraduationReceipt.geometry`/`_linked` decode it on `libs/python/compute/.planning/graduation/handoff.md`; both folder `[EVIDENCE_TRACE_LINKS]` cards close against those fences.
[PULSE_CONDUIT_DESIGN]-[COMPLETE]: conduit and actor lifecycle landed on `libs/python/runtime/.planning/execution/lanes.md` — `LanePolicy.pulses`, structured `drain` custody, spawn-context manager queue, `anyio.from_thread.run_sync` relay, serialized `Hooks.fire` fold, and non-blocking close enforce the lossy law; `pebble` map iterator and `MailboxProcessor` remain refuted, geometry `[PULSE_DRAIN_PROBE]` answered.
[EXEMPLAR_FILTER_ROW]-[COMPLETE]: landed as the `exemplar_filter=TraceBasedExemplarFilter()` argument in `_meter_provider` on `libs/python/runtime/.planning/observability/telemetry.md`, beside the `WIRE_AGGREGATION` exponential-aggregation default row.
[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed as the `_tracer_provider`/`_log_attach` promotion pair on `runtime/observability/telemetry#TELEMETRY` — `PROMOTED_BAGGAGE` closed predicate, registration before batch attach and global set, research row resolved against the installed distribution.
[WORKER_TELEMETRY_CONTINUITY]-[DROPPED]: collapsed into `runtime` `[WORKER_INSTALL_SEAM]` (idea grain: `runtime` `[WORKER_TELEMETRY_PARENTING]`) — the per-arm install-seam task carries the same scope with exact landing pages.
