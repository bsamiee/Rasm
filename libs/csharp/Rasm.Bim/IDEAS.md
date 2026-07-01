# [RASM_BIM_IDEAS]

The forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

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

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CONNECTION]-[COMPLETE]: Semantics/connection.md authors ConnectionDetail + ConnectionRealization [Union] (Bolted/Welded/Bearing/Cast) over IfcRelConnectsWithRealizingElements.RealizingElements with BoltPattern/WeldSchedule/BearingSurface [ComplexValueObject] rows and the ConnectionProjection.Project/ProjectAll fold, members decompile-verified (fastener nominal scalars internal → IfcMaterialProfileSetUsage/IfcCircleProfileDef.Radius channel); Ripple: Rasm.Materials [JOINT_CONNECTION_FAMILY].
[PANEL_COVERING_EGRESS]-[COMPLETE]: the sheet-goods panel family's kind-determined IFC leaves round-trip through the EXISTING `Model/elements#IFC_CLASS` `IfcClass` `[SmartEnum<string>]` roster with NO row growth — the `Covering` row (`IfcCovering`, valid set carrying `CLADDING`/`CEILING`/`FLOORING`/`INSULATION`), the `Plate` row (`IfcPlate`, valid set carrying `SHEET`), and the `Slab` row (`IfcSlab`, valid set carrying `FLOOR`/`ROOF`), all `IfcSchema.Ifc2x3`-spanned, already admit every predefined token the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` panel arm stamps onto the minted Type `Object` (`CLADDING`/`CEILING` gypsum·cement covering, `INSULATION` rigid board, `SHEET` wood structural sheet + steel roof/form deck, `FLOOR` composite floor deck), so `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` and `Projection/egress#IFC_EGRESS` `Emit` gate a panel Type without an `UnmappedClass` fault ([C6][H8]) — the covering/plate/slab admission is complete for the panel family, no `IfcClass` roster growth required. A `ComponentClass.Panel` board is an `IfcBuiltElement` covering/plate/slab (never a realizing element), so its neutral `DetailSchema.Realization` bag (deck rib depth/pitch, field/edge fastener spacing, board thickness/edge-profile) round-trips through the GENERAL `Projection/egress#IFC_EGRESS` `ReauthorProperties` `IfcPropertySet` egress + `Projection/semantic#SEMANTIC_PROJECTOR` `Bags` ingest — NOT the `Semantics/connection#CONNECTION_DETAIL` realizing-element reader — the Bim egress mapping the neutral `DetailSchema` `SetName` onto the IFC `Pset_*` name at `Emit`. Ripple: Rasm.Materials [PANEL_SHEET_GOODS_FAMILY].
[COORDINATION]-[COMPLETE]: Review/coordination.md authors the CoordinationRule [Union] (Require/Prohibit/Recommend) over the ElementPredicate algebra, the ClashProposal fold over the Model/systems#INTERFERENCE Interference evidence, the ChangeImpact A/B ImpactReport, and the SignOff [SmartEnum] state-machine + IssueBoard domain over the BcfStatus lifecycle.
[PROPERTY_TEMPLATE_RESOLUTION]-[COMPLETE]: Semantics/properties.md PropertyKey.Resolve unions the live BsddClass.Properties dictionary rows over the static Pset anchors and QuantitySet.Derive folds per-IfcClass base quantities from the bound GeometryHandle through the BaseQuantityTable + UnitsNet SI-base coercion.
[SCHEDULE_NETWORK_DEPTH]-[COMPLETE]: Planning/schedule.md adds the CriticalPath forward/backward CPM fold + WorkCalendar working-time fold over the SequenceRel DAG, and Exchange/export.md adds the AnimateSchedule 4D-emit arm over the decompile-verified ModelRoot.CreateAnimation/Animation.CreateVisibilityChannel/CreateScaleChannel keyframe surface and KHR_node_visibility.
[VERSIONED]-[COMPLETE]: Review/versioning.md authors the BimCommit content-addressed commit-DAG (UInt128 CommitKey over the Review/diff#MODEL_DIFF ElementFingerprint set via XxHash128, Seq<UInt128> ParentKeys lineage), the BimBranch ref, the BimRepository Commit/History/CommonAncestor folds over the DAG, and Version.Merge — the three-way fold over the content-keyed fingerprint maps producing a merged graph plus the closed MergeConflict [Union] (BothModified/ModifiedAndRemoved/AddedTwiceDivergent) the Review/coordination#SIGN_OFF SignOff resolves, CommitMerge rejecting an unresolved outcome (never an auto-resolve); reuses ModelDiff.Fingerprint/Between verbatim, meets the Rasm.Persistence Version owner at the content-key wire; README router [27] + ARCHITECTURE codemap/seams aligned.
