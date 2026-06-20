# [FABRICATION_TASKLOG]

The open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — plus `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

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

[NFP_DRL_POLICY]-[BLOCKED]: learned nesting remains a policy score column, not a second packer.
- Capability: learned placement ranking for `Nesting/nfp#NESTING` over the existing no-fit-polygon placement primitive.
- Shape: `NestPolicy.Score` carries the injected `Func<NoFitPolygon, PartTransform, double>` delegate; the app-platform consumer runs `Rasm.Compute/Model/inference#INFERENCE_MODES` `RunOps.Infer` and passes the scalar score while Fabrication keeps the bottom-left/genetic folds, `NoFitPolygon.Of`, and the `Stock`/`Remnant` feasibility set.
- Unlocks: higher utilization on irregular sheets and remnants after a trained ranking model exists, with phase 1 still shipping the Geometry2D-routed NFP and deterministic/genetic heuristics.
- Anchors: `Nesting/nfp#NESTING`, `NestPolicy`, `NoFitPolygon`, `PartTransform`, `Polygon/clipper#POLYGON_ALGEBRA`, `Rasm.Compute/Model/inference#INFERENCE_MODES`, and `RunOps.Infer`.
- Tension: the trained placement-ranking model asset and consumer-side inference wiring are outside this folder's scope, and the AEC-domain strata boundary forbids a Fabrication reference to `Rasm.Compute`.

[CSG_WATERTIGHT_SILHOUETTE]-[BLOCKED]: watertight-solid silhouettes compose the kernel arrangement owner, not in-folder CSG.
- Capability: exact boolean-solid outline production for `Posting/projection#PROJECTION_HIDDEN_LINE`.
- Shape: the silhouette arm reads `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT` through the realized `Arrangement.Apply` / `ToMesh` seam over `MeshBoolean`/`PlanarOverlay`/`CellComplex`, with `Meshing/delaunay#TESSELLATION` and `Meshing/offset#STRAIGHT_SKELETON` supplying the managed exact arrangement substrate.
- Unlocks: drafted outlines for boolean-combined watertight solids while the existing per-facet HLR kernel remains the pure-managed default for ordinary projection.
- Anchors: `Posting/projection#PROJECTION_HIDDEN_LINE`, `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT`, `Arrangement.Apply`, `ToMesh`, `Meshing/delaunay#TESSELLATION`, `Meshing/offset#STRAIGHT_SKELETON`, and `ROBUST_ARRANGEMENT_SUBSTRATE`.
- Tension: the realized C# arrangement owner must land before this arm opens; Fabrication never authors a CSG kernel, admits a native CSG asset, or couples to `ArrangementStore` / `SimplexStore` internals.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
