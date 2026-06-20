# [PY_GEOMETRY_IDEAS]

The forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[COMPANION_CPU_OFFLOAD]-[QUEUED]: geometry hands heavy CPU kernels to the runtime offload lane.
- Capability: companion geometry CPU kernels leave the event loop through one runtime-owned offload seam.
- Shape: registration, OCCT-backed IFC tessellation, `pdal` filtering, `open3d` reconstruction, and `trimesh`/`manifold3d` boolean work all use one caller-supplied `LanePolicy` handoff.
- Unlocks: true-parallel geometry work while the companion event loop stays responsive during OCCT cold starts and multi-second registration loops.
- Anchors: `python:GEOMETRY_KERNEL_OFFLOAD_LANE`, `execution/lanes#LANES`, `anyio.to_interpreter.run_sync`, one `CapacityLimiter`, one `DrainReceipt`, and `to_process` fallback.
- Tension: runtime must land `execution/lanes#LANES`; geometry then applies the same one-call seam in `daemon.md#DAEMON`, `scan/registration.md#REGISTRATION`, `mesh/repair.md#MESH`, `ingestion.md`, and `reconstruction.md`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
