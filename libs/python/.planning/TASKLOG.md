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

[WORKER_TELEMETRY_CONTINUITY]-[QUEUED]: Design the worker-process telemetry seam on the runtime observability owners.
- Capability: Trace-context inject/extract across the task-submit pickle seam and a per-process provider lifecycle for the worker fabric.
- Shape: Rows on the runtime `observability/telemetry` install (worker bootstrap, flush-at-retirement law) and the `lanes`/`workers` pages (submit-side context capture); flame tags follow the profiles owner.
- Unlocks: Compute and geometry kernel spans join the parent trace; a worker crash leaves flushed evidence.
- Anchors: runtime telemetry install; workers/lanes owners; `propagate.inject`/`propagate.extract` over the `cloudpickle`/`tblib` submit seam; `TracerProvider.force_flush` at retirement; profiles push owner.
- Tension: HOSTILE kernels bar in-process registry reach.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TENANT_SPAN_PROMOTION]-[COMPLETE]: landed as the `_tracer_provider`/`_log_attach` promotion pair on `runtime/observability/telemetry#TELEMETRY` — `PROMOTED_BAGGAGE` closed predicate, registration before batch attach and global set, research row resolved against the installed distribution.
