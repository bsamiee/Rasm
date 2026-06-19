# [PY_GEOMETRY_IDEAS]

The forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

## [01]-[OPEN]

[COMPANION_CPU_OFFLOAD]-[BLOCKED]: the heavy geometry kernels run on the runtime subinterpreter offload lane, not the companion event loop. (Trickle-down from `python:GEOMETRY_KERNEL_OFFLOAD_LANE`.) [BLOCKED] sequence-after the runtime `execution/lanes#LANES` `LanePolicy` CPU-offload variant.
- The registration ICP loops, the OCCT/IFC tessellation iteration, the `pdal` filter graph, the open3d reconstruction, and the `trimesh`/`manifold3d` boolean are all CPU-bound kernels that today block the daemon's `anyio` event loop or serialize through `to_process` pickle. The branch `runtime/execution/lanes` gains the `anyio.to_interpreter.run_sync` CPU-offload `LanePolicy` variant; every geometry owner that runs a heavy kernel hands it to that lane (the lane never imports the kernel) under one `CapacityLimiter` and one `DrainReceipt`, falling back to `to_process` where subinterpreters are unavailable.
- Unlocks true-parallel CPU geometry work without the companion stalling: a tessellation request, a registration fit, and a reconstruction run concurrently on subinterpreter-capable runtimes, and the daemon stays responsive during the 200-400 ms OCCT cold start and the multi-second registration loops.
- Draws on the cross-package `GEOMETRY_KERNEL_OFFLOAD_LANE` idea naming the geometry registration/tessellation loops as the exact heavy CPU kernels the lane must absorb; the geometry side is the consumer that hands its kernels to the lane, the lane owner staying branch-side — never a second concurrency surface minted in geometry.
- Close-condition: the runtime `execution/lanes#LANES` `LanePolicy` `anyio.to_interpreter.run_sync` CPU-offload variant lands on the branch runtime owner; the five geometry owner fences (daemon/registration/ingestion/reconstruction/repair) already carry the gated one-call hand-off seam (`daemon.md#DAEMON`, `scan/registration.md#REGISTRATION`, `mesh/repair.md#MESH`, plus the `ingestion.md`/`reconstruction.md` consumers), so the geometry-side realization is a uniform one-call growth across them once the lane exists.

## [02]-[CLOSED]

(none)
