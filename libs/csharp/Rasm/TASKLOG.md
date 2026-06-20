# [RASM_TASKLOG]

The kernel's open and closed work is distilled from ideas and design-page RESEARCH residuals. Each task is a card whose leader carries `[ID]-[STATUS]: thesis`, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` bullets.

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

[T-BOOLEAN-NATIVE-ASSET]-[BLOCKED]: native arrangement asset remains a scale-only gate after the managed exact boolean path lands.
- Capability: Admit a robust exact-arithmetic mesh-arrangement native asset only for boolean workloads beyond the managed arrangement scale ceiling.
- Shape: `HealOp.Boolean` composes `Arrangement.Apply(ArrangementKind.MeshBoolean, ...)` for common managed cases; `NativeAssetMissing` stays reserved for the future tier-3 scale route.
- Unlocks: Large boolean and CSG repair workloads can gain a chartered native acceleration path without blocking managed correctness, planar overlay, or ordinary mesh-boolean repair.
- Anchors: `Processing/repair#BOOLEAN_NATIVE_ASSET`, `Meshing/arrangement#MESH_BOOLEAN`, `Arrangement.Apply`, exact `Predicate` crossings, constrained `Tessellation`, `SpatialQuery.Winding`, and `DuplicateWeld`.
- Tension: Native admission is still an external dependency with RID burden and post-condition proof; it is a charter amendment for scale, not a second correctness owner.

[ARRANGEMENT_REALIZED_OWNER_LANDING]-[QUEUED]: State the Rasm.Geometry Meshing arrangement REALIZED-OWNER landing as the gate the Fabrication CSG watertight-silhouette arm waits on — the arrangement DESIGN page is authored, so the blocker is the .cs implementation of Arrangement.Apply/ToMesh, not a missing page, and the ripple records the consume-when-realized contract on the kernel side.
- Capability: The Fabrication CSG_WATERTIGHT_SILHOUETTE arm composes the kernel Rasm.Geometry/Meshing/arrangement#ARRANGEMENT exact-boolean owner (the authored Arrangement [Union] over MeshBoolean/PlanarOverlay/CellComplex, with Apply/ToMesh and the BooleanReceipt — design page LIVE at libs/csharp/Rasm/Geometry/.planning/Meshing/arrangement.md) reading the kept-cell boundary Arrangement re-emits. The page IS authored; the blocker is the realized C# owner landing. This ripple states the kernel-side contract: when the arrangement .cs owner lands, Apply/ToMesh emit the watertight-solid outline the Fabrication projection silhouette consumes, no Fabrication-side CSG kernel and no coupling to ArrangementStore/SimplexStore internals.
- Shape: A Rasm.Geometry Meshing/arrangement page note (the page exists) stating the Apply/ToMesh kept-cell-boundary re-emission is the consume-when-realized seam the Fabrication Posting/projection silhouette arm reads, and that the silhouette arm is forward-blocked on the realized owner, not on the design page — reconciling the Fabrication TASKLOG BLOCKED status against the authored-page truth.
- Unlocks: A correctly-scoped CSG silhouette block — held on the realized arrangement .cs owner (real kernel implementation work), not on an unauthored design page — and the kept-cell-boundary seam the Fabrication silhouette and any Materials/Bim CSG-layer composition read.
- Anchors: csharp:Rasm.Geometry/Meshing/arrangement#ARRANGEMENT (the AUTHORED page, Arrangement [Union] MeshBoolean/PlanarOverlay/CellComplex, Apply/ToMesh, BooleanReceipt, ROBUST_ARRANGEMENT_SUBSTRATE); Fabrication/Posting/projection#PROJECTION_HIDDEN_LINE the CSG silhouette consumer; csharp:Rasm.Geometry/Meshing/delaunay#TESSELLATION substrate
- Tension: The arrangement design page is fully authored (MeshBoolean/PlanarOverlay/CellComplex, exact predicate floor, generalized-winding cell classifier); the only outstanding work is the C# implementation, so this ripple records the consume-when-realized contract and corrects any 'page unauthored' framing the Fabrication side or the prior synth carried.
- Ripple: counterpart of `Rasm.Fabrication` `[CSG_SILHOUETTE]` idea / `[CSG_WATERTIGHT_SILHOUETTE]` task (the realized `Meshing/arrangement` owner they wait on).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
