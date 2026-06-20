# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with `[ID]-[STATUS]: thesis` and carries `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[IFC_BYTES_CONSTRUCTOR]-[BLOCKED]: add the IFC-bytes non-manifold topology constructor.
- Capability: `TopologyAlgebra` admits IFC bytes into the existing non-manifold `TopologyOp.CONSTRUCT` case without minting a second topology owner.
- Shape: `ifcopenshell.open` over `guess_format` produces the in-memory IFC model; the arm then resolves through `Topology.ByIFCFile(file)`, the `IFC` facade's model-object entry, or `Topology.ByOCCTShape(shape)` in that priority order.
- Unlocks: IFC-derived cell, aperture, adjacency, and dual-graph analysis reaches the geometry graduation rail while the C# BIM space-graph remains the in-process semantic owner.
- Anchors: `graph/nonmanifold.md#3-IFC_BYTES_CONSTRUCTOR`, `.api/topologicpy.md`, `Topology.ByIFCFile`, `Topology.ByIFCPath`, `Topology.ByOCCTShape`, the `IFC` facade, `ifcopenshell.open`, and the `<'3.15'` companion-band admission.
- Tension: `Topology.ByIFCFile(file)` still needs companion-interpreter reflection to prove whether `file` accepts an `ifcopenshell.file` object or only a path string; the temp-file detour stays forbidden, and `topologicpy` remains manifest-absent until the companion row lands.

[GEOMETRY_CPU_OFFLOAD]-[BLOCKED]: hand geometry CPU kernels to the runtime offload lane.
- Capability: heavy geometry kernels run through the branch runtime offload lane instead of a geometry-local concurrency surface.
- Shape: `registration.py` ICP/global loops, `mesh/daemon.py` OCCT iteration, `ingestion.py` `pdal` filters, `reconstruction.py` open3d reconstruction, and `repair.py` `manifold3d` boolean work hand caller-supplied kernels to `LanePolicy.offload` under one `CapacityLimiter` and one `DrainReceipt`.
- Unlocks: the companion event loop stays responsive during multi-second geometry work, and every geometry owner adopts the same one-call handoff once the runtime lane lands.
- Anchors: `runtime/.planning/execution/lanes.md#LANES`, `anyio.to_interpreter.run_sync`, the `to_process.run_sync` fallback, `daemon.md#DAEMON`, `registration.md#REGISTRATION`, `repair.md#MESH`, `ingestion.md#OFFLOAD_LANE`, and `reconstruction.md#OFFLOAD_LANE`.
- Tension: runtime owns `LanePolicy`; geometry only consumes it, and every fallback kernel must stay picklable for the process path.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
