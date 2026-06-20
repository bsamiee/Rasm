# [FABRICATION_IDEAS]

The forward concept pool for `Rasm.Fabrication`. Each idea is a card — a bracketed slug leader plus a few bullets stating the capability, what it unlocks, and the gap or modern technique it draws on. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

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

[DRL_NEST_POLICY]-[QUEUED]: learned placement scoring becomes a policy column over the existing NFP primitive.
- Capability: a learned placement-score slot that deepens `Nesting/nfp#NESTING` without changing the no-fit-polygon geometry owner.
- Shape: `NestPolicy.Score` carries an injected `Func<NoFitPolygon, PartTransform, double>` delegate ranking candidate transforms inside the existing bottom-left/genetic folds.
- Unlocks: higher utilization on irregular sheet-metal nesting without a Fabrication-side `Rasm.Compute` reference, second runtime, or learned-vs-heuristic packer split.
- Anchors: `Rasm.Compute/Model/inference#INFERENCE_MODES`, `RunOps.Infer`, `Nesting/nfp#NESTING`, and the raw-scalar ingress pattern used by `RemovalBudget`.
- Tension: the trained placement-ranking model asset and consumer-side wiring must exist before the learned column joins the bottom-left/genetic folds.

[CSG_SILHOUETTE]-[QUEUED]: watertight boolean solids project through the managed arrangement substrate.
- Capability: a watertight-solid silhouette arm that composes the kernel arrangement output instead of authoring CSG inside Fabrication.
- Shape: `Posting/projection#PROJECTION_HIDDEN_LINE` reads the kept-cell boundary re-emitted by `Arrangement.Apply` / `ToMesh` for a boolean-combined solid while the per-facet HLR kernel remains the default.
- Unlocks: drafted outlines for true boolean profiles with no native CSG asset, no in-folder CSG kernel, and no coupling to `ArrangementStore` / `SimplexStore` internals.
- Anchors: `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT`, `Meshing/delaunay#TESSELLATION`, `Meshing/offset#STRAIGHT_SKELETON`, and the branch `ROBUST_ARRANGEMENT_SUBSTRATE`.
- Tension: `Posting/projection` can read exact kept-cell outlines only after the realized C# arrangement owner lands.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
