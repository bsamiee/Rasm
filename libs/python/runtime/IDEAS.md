# [PY_RUNTIME_IDEAS]

`runtime`'s forward pool of higher-order folder concepts, grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or modern technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

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

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[REMOTE_WORKER_DISPATCH]-[COMPLETE]: landed as `WorkerKind.REMOTE` on `execution/workers` — `KIND_POLICY(fidelity=True, restart=RetryClass.SSH)`, the `WorkerPool` remote arm over one `transport/roots` `RemoteEndpoint` channel with `remote_floor` far-side, shm-wire refusal, channel-liveness supervision; roots scope law widened one seam.

[SHARED_MEMORY_CHANNEL]-[COMPLETE]: landed as the `Wire.SHARED_MEMORY` span channel on `execution/workers#FABRIC` — `ShmSpan` named blocks, exporter-owned unlink, worker-side `numpy.frombuffer` reconstruction, ingress-only law.
