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

[VERSIONED]-[QUEUED]: Introduce a Versioned sub-domain folding the content-keyed ElementFingerprint and the ModelDiff change-set into a content-addressed commit-DAG: a BimCommit referencing parent commit(s) by content-key, a BimBranch ref, and a three-way Merge fold reconciling two divergent revisions through the existing ElementChange union against a common ancestor, so model history is a first-class Bim concept meeting the Persistence Version owner at the content-key wire rather than living only as a pairwise Diff.
- Capability: A higher-order versioning algebra over the settled diff: the per-element content-key fingerprint is already the commit-object identity, so a commit is the fingerprint set, a diff is two commits, and a merge folds two ElementChange streams against a common ancestor producing a merged graph plus a MergeConflict [Union] (both-modified / modified-and-removed / added-twice-divergent) the coordination sign-off resolves.
- Shape: A new Versioned sub-domain (one node: Version.cs) carrying BimCommit (UInt128 CommitKey, Seq<UInt128> ParentKeys, Map<string,ElementFingerprint> Fingerprints, Instant At), BimBranch ref, and Version.Merge folding two revisions through ElementChange against a common ancestor; the commit-DAG meets the Persistence Version owner at the content-key wire and reuses Diff/Between verbatim.
- Unlocks: Federation version control (branch/merge/resolve), model history lineage, three-way merge governed by coordination sign-off, the Persistence Version owner storing the DAG by the same content-key the wire and diff carry, and the OpLog wire face the Persistence CRDT sync converges over.
- Anchors: Review/diff#MODEL_DIFF ModelDiff/ElementFingerprint/ElementChange; Exchange/wire#WIRE_PROJECTION OpLog face (ONE_MODEL_THREE_FACES Snapshot/OpLog/Grpc confirmed); Model/elements#ELEMENT_MODEL BimModel; csharp:Rasm.Persistence Version commit-DAG; csharp:Rasm.Compute CONTENT_ADDRESSING.
- Tension: The commit-DAG storage is the Persistence Version owner's concern and Bim depends strictly upward: the Bim Versioned owner produces the content-addressed commit objects and the merge algebra, the durable DAG riding the Persistence ripple; the version lineage reuses the diff content-key, never a second identity scheme; conflict resolution is the coordination sign-off's concern, not an auto-resolve. The audit-log half of this idea (the AuditTrail chained mutation log) is realized at Review/diff#AUDIT; the commit-DAG/BimBranch/three-way Merge algebra here is NOT yet authored — no Review/versioning.md page nor any BimCommit/Merge owner exists — so the idea's core Capability/Shape stays open pending a new Versioning design page.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CONNECTION]-[COMPLETE]: Semantics/connection.md authors ConnectionDetail + ConnectionRealization [Union] (Bolted/Welded/Bearing/Cast) over IfcRelConnectsWithRealizingElements.RealizingElements with BoltPattern/WeldSchedule/BearingSurface [ComplexValueObject] rows and the ConnectionProjection.Project/ProjectAll fold, members decompile-verified (fastener nominal scalars internal → IfcMaterialProfileSetUsage/IfcCircleProfileDef.Radius channel); Ripple: Rasm.Materials [JOINT_CONNECTION_FAMILY].
[COORDINATION]-[COMPLETE]: Review/coordination.md authors the CoordinationRule [Union] (Require/Prohibit/Recommend) over the ElementPredicate algebra, the ClashProposal fold over the Model/systems#INTERFERENCE Interference evidence, the ChangeImpact A/B ImpactReport, and the SignOff [SmartEnum] state-machine + IssueBoard domain over the BcfStatus lifecycle.
[PROPERTY_TEMPLATE_RESOLUTION]-[COMPLETE]: Semantics/properties.md PropertyKey.Resolve unions the live BsddClass.Properties dictionary rows over the static Pset anchors and QuantitySet.Derive folds per-IfcClass base quantities from the bound GeometryHandle through the BaseQuantityTable + UnitsNet SI-base coercion.
[SCHEDULE_NETWORK_DEPTH]-[COMPLETE]: Planning/schedule.md adds the CriticalPath forward/backward CPM fold + WorkCalendar working-time fold over the SequenceRel DAG, and Exchange/export.md adds the AnimateSchedule 4D-emit arm over the decompile-verified ModelRoot.CreateAnimation/Animation.CreateVisibilityChannel/CreateScaleChannel keyframe surface and KHR_node_visibility.
