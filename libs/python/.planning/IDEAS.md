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

[WORKER_TELEMETRY_CONTINUITY]-[QUEUED]: One telemetry fabric across the process-pool seam — worker kernels emit into the parent's trace, metric, and profile streams.
- Capability: Per-worker-process provider bootstrap with trace context crossing the task-submit pickle seam, closing the branch's one observability dark zone.
- Shape: A worker-side install in the runtime telemetry owner — context injected at submit, extracted at kernel entry, providers flushed at worker retirement — with flame tags riding the profiler push.
- Unlocks: End-to-end traces and profiles over compute graduation kernels and geometry daemon loops that today report only through receipts.
- Anchors: runtime telemetry install and workers/lanes fabric; `propagate.inject`/`propagate.extract` over the `cloudpickle`/`tblib` seam already crossing tracebacks; `TracerProvider.force_flush` and `shutdown` at retirement; geometry's graduation worker-seam boundary law.
- Tension: HOSTILE kernels bar in-process registry reach; the tap grain waits on a parent-side streaming drain.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed on the runtime telemetry install — `opentelemetry-processor-baggage` admitted, the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction, producer pages untouched.
