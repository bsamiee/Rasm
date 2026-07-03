# [FABRICATION_TASKLOG]

The open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — plus `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

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

[NFP_DRL_POLICY]-[BLOCKED]: learned nesting remains a policy score column, not a second packer.
- Capability: learned placement ranking for `Nesting/nfp#NESTING` over the existing no-fit-polygon placement primitive.
- Shape: `NestPolicy.Score` carries the injected `Func<NoFitPolygon, PartTransform, double>` delegate; the app-platform consumer runs `Rasm.Compute/Model/inference#INFERENCE_MODES` `RunOps.Infer` and passes the scalar score while Fabrication keeps the bottom-left/genetic folds, `NoFitPolygon.Of`, and the `Stock`/`Remnant` feasibility set.
- Unlocks: higher utilization on irregular sheets and remnants after a trained ranking model exists, with phase 1 still shipping the Geometry2D-routed NFP and deterministic/genetic heuristics.
- Anchors: `Nesting/nfp#NESTING`, `NestPolicy`, `NoFitPolygon`, `PartTransform`, `Polygon/clipper#POLYGON_ALGEBRA`, `Rasm.Compute/Model/inference#INFERENCE_MODES`, and `RunOps.Infer`.
- Tension: the trained placement-ranking model asset and consumer-side inference wiring are outside this folder's scope, and the AEC-domain strata boundary forbids a Fabrication reference to `Rasm.Compute`.
- Ripple: realizes the `[DRL_NEST_POLICY]` idea in this folder's `IDEAS.md`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CSG_WATERTIGHT_SILHOUETTE]-[COMPLETE]: projection.md Hlr.Solve sources the watertight mesh through Arrangement.Apply(MeshBoolean)/ToMesh off the HiddenLine.Watertight Option<BooleanSolid> operand, the per-facet HLR kernel projecting the re-emitted kept-cell mesh unchanged; Ripple: Rasm/Geometry [CSG_SILHOUETTE].
[TOOLPATH_STRATEGY_MODALITY_FACTOR]-[COMPLETE]: motion.md Cam.Generate dispatches the (RemovalModality, CutStrategy) cross-product through one generated total Switch over the family.md CutStrategy axis + RemovalModality.Admits relation, the flat 11-row ToolpathKind retired, an inadmissible pair routing FabricationFault.InadmissiblePair.
[POST_DIALECT_OVERRIDE_SEAM]-[COMPLETE]: FabricationInput.Dialect Option<PostDialect> override resolved against Process.Dialect fallback at Posting.Resolve, dialect.Admits(modality) gating compatibility, ProcessFamily.AdmitDialect the span-keyed boundary; one resolution fold, no second dialect enum.
[TROCHOIDAL_ENGAGEMENT_LIMIT]-[COMPLETE]: motion.md Engage/Walk size the variable radial step off ClearanceAt + EngagementPolicy with the MaxAxialDepth cap, each curving chord tagged Option<ArcCenter> the program.md Biarc fold refits to G2/G3 — both the constant-engagement controller and the arc-emission arm landed.
[NEST_MULTISHEET_SCHEDULE]-[COMPLETE]: nfp.md Nest.Solve folds FabricationInput.Inventory Seq<Stock> sheet-by-sheet through Schedule/Consume, stamping SheetIndex, spilling unplaced parts forward, re-injecting the consumed-stock remnant, routing StockOverflow on inventory exhaustion.
[NEST_REMNANT_LINEAGE]-[COMPLETE]: nfp.md Remnant.From mints each kerf-inflated Boolean-difference leftover region as a content-keyed XxHash128 child stamped with the consumed stock's Parent lineage, the remnant re-entering the multi-sheet inventory.
[RECTPACK_FASTPATH_ADMISSION]-[COMPLETE]: RectpackSharp 1.2.0 admitted (csproj/README/.api-rectpacksharp), nfp.md PlacementMode rect-fastpath arm runs RectanglePacker.Pack over the planar Sheet/Plate/Billet bounds with Guillotine/GrainDirection columns, the Pack infeasibility throw lowered through Try; Ripple: Directory.Packages.props central pin (reconcile-owned).
[BIARC_ARC_EMISSION_ADMISSION]-[COMPLETE]: geometry3Sharp reused scoped to g3.BiArcFit2/Arc2d/Segment2d/Vector2d (csproj/README/.api-geometry3sharp), program.md Biarc/FitRun/ArcWord refits each cutting span to G2/G3 ArcCw/ArcCcw GWords with SampleT(0)-to-Center I/J, a sub-MinRunLength run staying Feed.
[ACADSHARP_SPLINE_INSERT]-[COMPLETE]: import.md Admit grows the Spline arm (Spline.TryPolygonalVertexes native tessellator) and the Insert arm (Flatten over Block.Entities composing the OCS-to-WCS Placement transform), both ratified by .api-acadsharp [SPLINE_SAMPLER]/[BLOCK_TRAVERSAL]; Ripple: Rasm.Bim [ACADSHARP_MANAGED_DWG].
[CLIPPER_INNER_FIT]-[COMPLETE]: clipper.md MinkowskiDiff primitive over the precision-bearing Minkowski.Diff facade plus the nfp.md consumer — NoFitPolygon.InnerFit per-rotation IFP factory and Stock.Contains FromRemnant IFP-inside arm via Remnant.Holds/InnerFeasible.
[FABRICATION_FAULT_BAND_DEEPENING]-[COMPLETE]: faults.md FabricationFault widened to 9 arms — InadmissiblePair 2505/Gouge 2506/Collision 2507/NonManifoldSlice 2508/StockOverflow 2509 added beside the original four, Code/Message Switch and Cases extended, producers named in Auto.
[TOOL_CUTTING_DATA_TABLE]-[COMPLETE]: physics.md Tool gains Coating[SmartEnum]/CornerRadius/HelixAngle/Stickout/Runout columns and a frozen (Material,Tool,Operation) CuttingData SFM/chip-load table SubtractiveBudget reads before the formula, defeating the inline 90.0 fallback.
[CLIPPER_VARIABLE_KERF]-[COMPLETE]: clipper.md PolygonAlgebra gains OffsetVariable driving the stateful ClipperOffset.DeltaCallback64 per-vertex delta on the int64 Paths64 rail beside the uniform InflatePaths PathD facade.
[FAULT_REBAND_2700]-[COMPLETE]: faults.md re-banded onto the federation FaultBand registry — Code derives FaultBand.Fabrication + n (2701-2709 offsets preserved, message strings unchanged) off the live 2501-2509 collision inside Element's 2500 band; the persisted-boundary probe confirmed no 25xx Fabrication code crosses a persisted or wire boundary; the re-homed Nest case (a malformed cutting-stock job, ex-ConstructionFault.Nest) lands at + 10.
[STOCK_NEST_MOVE]-[COMPLETE]: the rectangular cutting-stock engine moved whole from Rasm.Materials Construction/nesting.md into NEW Nesting/stock.md (namespace Rasm.Fabrication.Nesting) — re-railed onto FabricationFault.Nest/GeometryFault.DegenerateInput, CutPart/NestPlacement gain the PartId profile-alignment column; nfp.md's CuttingPlan/PlannedPlacement wire mirror DELETED, FabricationInput.Plan retyped Option<NestPlan>, Nest.Honor consumes the sibling NestPlan directly; RectangleBinPack.CSharp admitted (csproj/README/.api moved from Materials); Materials Construction/ folder removed with its governance rows. Ripple: the NestYield.WasteAreaMm2 -> Rasm.Compute AggregateEnvironmental/AggregateCost rollup is the recorded next-campaign counterpart.
