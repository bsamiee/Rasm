# [PERSISTENCE_VERSION_MERGE]

Rasm.Persistence structural diff/merge over the seam `ElementGraph`: the `Generator.Equals` `Inequalities` member diff over the `[Equatable]` `ElementGraph` as the authoritative change-set, an `ElementGraph → GraphNode` forest classification keyed on the durable `NodeId` the re-ingest `Reconcile` first aligns on the 1:1 IFC GlobalId (`Node.Object.ExternalId`), a base-relative three-way merge that surfaces every typed conflict in one pass, and a per-node RFC 6902 patch egress the inspector and CDE consume. The change-set is `ElementGraph.EqualityComparer.Default.Inequalities(before, after)` — it enumerates exactly which `Nodes[id]`/`Edges[i]` members each side moved off the base (each `Inequality` carrying its `MemberPath` plus the `Left`/`Right` old→new values), so a node neither side touched never enters the merge and a node BOTH sides moved is the conflict candidate. The forest CLASSIFIES that candidate: it keys on the same `NodeId` the graph keys on (never a parallel node identity) and reads its containment topology off the seam `Relations/relation#EDGE_ALGEBRA` `Relationship.Compose` edges (`Whole`→`Part`, ordered by sibling index) rather than a second store. Each `GraphNode` carries two seam-sourced content axes: the geometry axis is the `Object` node's `Representations.Body` representation content hash (the kernel-minted digest the diff READS, never re-derives) and the content axis is `XxHash128` over the seam `Node.ToCanonicalBytes(tolerance)` canonical projection — never a third hasher. A geometry-divergent concurrent change is a `TopologyBreak`, a content-only divergent change a `ParallelEdit`, and a content node (`PropertySet`/`Material`/`Coverage`) the Object-forest does not topologize but both sides moved is a content `ParallelEdit` the Inequalities change-set alone surfaces. The `SubtreeHash` Merkle digest prunes byte-identical subtrees over a deterministic LE preimage so the forest classification stays linear in the changed-node count; merge conflicts project to the settled `Version/ledger#MERGE_LAW` `ConflictReceipt` carrying the real `(Hlc, actor)` evidence both sides stamp (resolved from the changefeed). A re-ingest is correlated BEFORE the diff by `Reconcile`: the seam mints a FRESH neutral rooted `NodeId` on every `Project`, so a re-imported model shares no `NodeId` with the persisted graph and a raw `NodeId`-keyed diff would read it as delete-all + insert-all — `Reconcile` matches rooted nodes on the stable `Node.Object.ExternalId` (the 1:1 IFC GlobalId, `H6`) and rewrites the ingest onto the durable ids, so the durable `NodeId` survives re-import and the forest diff then keys on it while the `(GeometryHash, PropertyHash)` content signature drives change DETECTION, never cross-ingest identity. `NodeId`, `Node`, `Relationship`, `ElementGraph` arrive from `Rasm.Element`; `Hlc`, `Crdt`, `RgaSequence` arrive from `Version/commits`; `OpLogEntry`, `ConflictReceipt` arrive from `Version/ledger`.

## [01]-[INDEX]

- [01]-[STRUCTURAL_DIFF]: the re-ingest `Reconcile` GlobalId alignment, Merkle-pruned forest match, base-relative three-way merge, typed conflict classes, and RFC 6902 patch egress.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` the identity-keyed forest node carrying the geometry content key (`GeometryHash` = the `Object` `Representations.Body`), the node content-axis hash (`PropertyHash` = `XxHash128` over `Node.ToCanonicalBytes`), the sibling ordinal, and the Merkle subtree digest; `NodeKind` the IFC/seam classification vocabulary whose `From` projection lowers a node's `Classification` to the kind the retype conflict reads; `EditOp` the forest-edit operation family driven by a generated `Switch`/`Map`; `MergeConflict` the typed conflict-class family carrying both sides' op-log stamp; `EntityEdit` the per-key egress kind (`Tombstone` whole-node removal vs `Members` RFC 6902 member-patch); `TallySlot`/`TallyFact` the edit-versus-conflict count axis; `MergeOutcome` the settled three-way receipt; `StructuralMerge` the static surface owning the re-ingest `Reconcile` GlobalId alignment, the `Generator.Equals` `Inequalities` change-set, the `ElementGraph → GraphNode` forest projection, the Merkle-pruned forest classification, the base-relative three-way merge, the conflict classification fold, the per-node RFC 6902 patch projection, and the `ConflictReceipt` projection.
- Cases: `Match | Insert | Delete | Update | Move | Reorder | Retype` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | ReorderReorder | TypeChange | TopologyBreak | ContainmentCycle` on `MergeConflict`; `Tombstone | Members` on `EntityEdit`; the patch egress maps `Delete → Tombstone`, `Insert → Members` over the inserted node's full member set, and `Update`/`Move`/`Reorder`/`Retype → Members` over the targeted member.
- Entry: `public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested)` aligns a re-ingest's freshly-minted rooted `NodeId`s to the durable persisted ids by correlating rooted nodes on `Node.Object.ExternalId` (the stable 1:1 GlobalId) and rewriting the ingest's node ids + edge endpoints onto the durable ids; `public static Seq<GraphNode> Forest(ElementGraph graph)` projects each `Object` node's `Parent`/`Ordinal`/`Children` topology from the seam `Relationship.Compose` containment edges, its `GeometryHash` from the `Object` `Representations.Body`, its `PropertyHash` from `XxHash128` over `Node.ToCanonicalBytes`, and its `NodeKind` from the `Object` `Classification`, then seals the `SubtreeHash` bottom-up; `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` is the minimal forest-edit script; `public static MergeOutcome ThreeWay(ElementGraph @base, ElementGraph ours, ElementGraph theirs, Func<NodeId, Option<OpLogEntry>> stampOurs, Func<NodeId, Option<OpLogEntry>> stampTheirs)` gates conflict candidates on the `Generator.Equals` `Inequalities` change-set, folds non-conflicting edits, and accumulates true overlaps as typed conflicts; `public static HashMap<NodeId, EntityEdit> Patch(Seq<EditOp> script)` groups the resolved script by target key; `public static ConflictReceipt Project(MergeConflict conflict, ModelId model, CorrelationId correlation, Instant at)` reuses the one conflict vocabulary, carrying the merged model and the conflict's real `ColumnFamily` (the geometry axis for a `TopologyBreak`, the CRDT axis for every property/structural class).
- Auto: a re-ingest first runs `Reconcile` so the freshly-minted ingest `NodeId`s align to the durable persisted ids on the stable `Node.Object.ExternalId` GlobalId (the neutral `NodeId` is minted afresh each `Project`, so a raw `NodeId`-keyed diff would read the whole re-import as delete-all + insert-all); with the graphs aligned, node matching is the durable `NodeId` and the `(GeometryHash, PropertyHash)` content signature drives change DETECTION — the forest edit emits `Match` for signature-equal same-parent nodes, `Move` for signature-equal parent-changed, `Reorder` for same-parent ordinal-changed, `Retype` for `NodeKind`-changed, `Update` for signature-changed carrying both content axes, and `Insert`/`Delete` for unmatched; the `SubtreeHash` folds each node's own `(GeometryHash, PropertyHash, Ordinal, Kind.Key)` preimage with its sorted children's digests through the `XxHash128.Append` accumulator so the diff compares one digest per node and descends only where it diverges; the three-way merge reads the `Inequalities` change-set as the authoritative per-side moved-member set, diffs base→ours and base→theirs for the forest axis classification, applies every edit on a key neither side conflicts on AND both orthogonal ops where the sides edit one key on non-overlapping axes, and folds every genuine overlap into a `MergeConflict`.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count folded into `MergeOutcome.Counts`, and each `MergeConflict` projects to `ConflictReceipt` with the held/incoming `(Hlc, actor)` from the changefeed.
- Packages: Generator.Equals (`[Equatable]` deep equality + `Inequalities` member diff), System.IO.Hashing, Microsoft.AspNetCore.JsonPatch.SystemTextJson, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Runtime.InteropServices, System.Collections.Frozen, System.Collections.Immutable, BCL inbox.
- Growth: a new edit op is one `EditOp` case plus one `Switch`/`Map` arm; a new conflict class is one `MergeConflict` case; a new egress kind is one `EntityEdit` case; zero new surface — a line-based text diff, a JSON-patch diff over serialized nodes, a third content axis, or a second merge resolver is the deleted form; the `Version/ledger#SYNC_TRANSPORTS` `GraphDiff` stays the transport-level set-difference and the `Version/timetravel#TIME_TRAVEL` `RangeDiff` the cell-level CRDT delta — three altitude-split diffs, never duplicated; `Reconcile` is the re-ingest identity ALIGNER that feeds the forest diff, never a fourth diff.
- Boundary: node identity is the durable `NodeId` the re-ingest `Reconcile` aligns on the 1:1 IFC GlobalId (`Node.Object.ExternalId`) BEFORE the diff — the neutral `NodeId` is freshly minted each `Project`, so a re-imported model is correlated on the stable GlobalId, never the minted id; a `NodeId`-only re-ingest diff that reads the whole import as delete-all + insert-all, a content-signature "match" that silently drops a changed element's identity continuity, a positional or ordinal-only match, or a `Guid`-keyed parallel node identity beside the graph's own key is the named defect because after `Reconcile` the graph owns the one durable key the diff matches on; the `ElementGraph → GraphNode` projection reads forest topology VERBATIM from the seam `Relationship.Compose` containment edges — the parent is the `Whole`, the child the `Part`, the sibling order the index among one `Whole`'s `Compose` edges — so a positional re-derivation or a second containment store is the deleted form; `GraphNode.GeometryHash` IS the `Object` node's `Representations.Body` representation content hash (the kernel-minted digest the geometry domain owns, read not re-minted) and `GraphNode.PropertyHash` is `XxHash128` over the seam `Node.ToCanonicalBytes`, never a re-digest; the `SubtreeHash` is `XxHash128` over a stable deterministic byte stream with NO `GetHashCode` so two peers seal the identical digest and a byte-identical subtree under an unchanged root prunes whole; the three-way merge is base-relative so a node both sides left untouched never enters a conflict, and a structural merge is always-succeeds-with-annotations — `ThreeWay` returns `MergeOutcome` carrying BOTH the clean merged edit script AND the full conflict sequence in one pass, never a `Validation` fail-XOR-succeed that would discard the partial merge; each `MergeConflict` carries both sides' `(Hlc, actor)` resolved from the changefeed stamp delegates so `Project` mints a `ConflictReceipt` with real evidence; the merge result re-projects two ways — as `EditOp` rows applied through the CRDT algebra (a `Reorder` resolves against the `Version/commits#CRDT_ALGEBRA` `RgaSequence` sibling order) and as a `HashMap<NodeId, EntityEdit>` (one egress kind per touched key, a `Members` patch the inspector applies break-on-first-error to the node snapshot clone, a `Tombstone` dropping it); a `ContainmentCycle` (a `Move` whose target becomes its own descendant) is detected on BOTH sides, the cycle walk visited-set-bounded so the detector terminates on the cyclic input, and the cycle-bearing key is excluded from the merged script before patch projection because a containment graph admits no cycle.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<VersionKeyPolicy, string>]
[KeyMemberComparer<VersionKeyPolicy, string>]
public sealed partial class NodeKind {
    public static readonly NodeKind Element = new("element");
    public static readonly NodeKind Space = new("space");
    public static readonly NodeKind Container = new("container");
    public static readonly NodeKind Assembly = new("assembly");
    public static readonly NodeKind Annotation = new("annotation");

    public static NodeKind From(string classification) => classification switch {
        ['I', 'f', 'c', 'S', 'p', 'a', 'c', 'e', ..] or ['I', 'f', 'c', 'Z', 'o', 'n', 'e', ..] => Space,
        ['I', 'f', 'c', 'B', 'u', 'i', 'l', 'd', 'i', 'n', 'g', ..] or ['I', 'f', 'c', 'S', 'i', 't', 'e', ..] or ['I', 'f', 'c', 'S', 't', 'o', 'r', 'e', 'y', ..] => Container,
        ['I', 'f', 'c', 'E', 'l', 'e', 'm', 'e', 'n', 't', 'A', 's', 's', 'e', 'm', 'b', 'l', 'y', ..] => Assembly,
        ['I', 'f', 'c', 'A', 'n', 'n', 'o', 't', 'a', 't', 'i', 'o', 'n', ..] => Annotation,
        _ => Element,
    };
}

[SmartEnum]
public sealed partial class TallySlot {
    public static readonly TallySlot Edit = new();
    public static readonly TallySlot Conflict = new();
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GraphNode(NodeId Key, NodeKind Kind, Option<NodeId> Parent, int Ordinal, UInt128 GeometryHash, UInt128 PropertyHash, UInt128 SubtreeHash, Seq<NodeId> Children) {
    public UInt128 Signature => SubtreeHash;
    public bool Matches(GraphNode other) => GeometryHash == other.GeometryHash && PropertyHash == other.PropertyHash;
}

public sealed record EntityPatch(string Kind, Option<NodeId> Parent, int Ordinal, UInt128 GeometryHash, UInt128 PropertyHash);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EditOp {
    private EditOp() { }
    public sealed record Match(NodeId Key) : EditOp;
    public sealed record Insert(GraphNode Node) : EditOp;
    public sealed record Delete(NodeId Key) : EditOp;
    public sealed record Update(NodeId Key, UInt128 FromProperty, UInt128 ToProperty, UInt128 FromGeometry, UInt128 ToGeometry) : EditOp;
    public sealed record Move(NodeId Key, Option<NodeId> FromParent, Option<NodeId> ToParent) : EditOp;
    public sealed record Reorder(NodeId Key, int FromOrdinal, int ToOrdinal) : EditOp;
    public sealed record Retype(NodeId Key, NodeKind FromKind, NodeKind ToKind) : EditOp;

    public NodeId Target => this.Map(match: m => m.Key, insert: i => i.Node.Key, delete: d => d.Key, update: u => u.Key, move: v => v.Key, reorder: r => r.Key, retype: t => t.Key);
    public string KindName => this.Map(match: static _ => "match", insert: static _ => "insert", delete: static _ => "delete", update: static _ => "update", move: static _ => "move", reorder: static _ => "reorder", retype: static _ => "retype");
    public string Axis => this.Map(match: static _ => "", insert: static _ => "insert", delete: static _ => "delete", update: static _ => "content", move: static _ => "parent", reorder: static _ => "ordinal", retype: static _ => "kind");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record MergeConflict {
    private MergeConflict() { }
    public sealed record ParallelEdit(NodeId Key, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record DeleteUpdate(NodeId Key, bool DeletedByOurs, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record MoveMove(NodeId Key, Option<NodeId> OurParent, Option<NodeId> TheirParent, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ReorderReorder(NodeId Key, int OurOrdinal, int TheirOrdinal, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TypeChange(NodeId Key, NodeKind OurKind, NodeKind TheirKind, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TopologyBreak(NodeId Key, UInt128 OurGeometry, UInt128 TheirGeometry, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ContainmentCycle(NodeId Key, NodeId Ancestor, bool ByOurs, Hlc OurCell, string OurActor) : MergeConflict;

    public NodeId Subject => this.Map(parallelEdit: p => p.Key, deleteUpdate: d => d.Key, moveMove: m => m.Key, reorderReorder: r => r.Key, typeChange: t => t.Key, topologyBreak: b => b.Key, containmentCycle: y => y.Key);
    public string KindName => this.Map(parallelEdit: static _ => "parallelEdit", deleteUpdate: static _ => "deleteUpdate", moveMove: static _ => "moveMove", reorderReorder: static _ => "reorderReorder", typeChange: static _ => "typeChange", topologyBreak: static _ => "topologyBreak", containmentCycle: static _ => "containmentCycle");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EntityEdit {
    private EntityEdit() { }
    public sealed record Tombstone(NodeId Key) : EntityEdit;
    public sealed record Members(NodeId Key, JsonPatchDocument<EntityPatch> Patch) : EntityEdit;
}

public readonly record struct TallyFact(TallySlot Slot, string Kind, int Count);
public readonly record struct MergeOutcome(Seq<EditOp> Merged, Seq<MergeConflict> Conflicts, Seq<TallyFact> Counts) {
    public bool Clean => Conflicts.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StructuralMerge {
    public static Seq<GraphNode> Forest(ElementGraph graph) {
        // Forest topology rides the seam Relationship.Compose containment edges (Whole→Part), siblings
        // ordered by their index among one Whole's Compose edges — never a phantom unified edge kind.
        Seq<Relationship.Compose> contain = toSeq(graph.Edges.Choose(static e => Optional(e as Relationship.Compose)));
        HashMap<NodeId, NodeId> parentByKey = toHashMap(contain.Map(static c => (c.Part, c.Whole)));
        HashMap<NodeId, Seq<NodeId>> childrenByParent = toHashMap(contain.GroupBy(static c => c.Whole).Select(static g => (g.Key, toSeq(g.Select(static c => c.Part)))));
        HashMap<NodeId, int> ordinalByKey = toHashMap(contain.GroupBy(static c => c.Whole).SelectMany(static g => g.Select(static (c, ordinal) => (c.Part, ordinal))));
        HashMap<NodeId, GraphNode> nodes = toHashMap(graph.Nodes.Values.Choose(static n => Optional(n as Node.Object)).Map(o => (o.Id, new GraphNode(
            o.Id, NodeKind.From(o.Classification.Code), parentByKey.Find(o.Id), ordinalByKey.Find(o.Id).IfNone(0),
            o.Representations.Body.IfNone(UInt128.Zero), XxHash128.HashToUInt128(o.ToCanonicalBytes(graph.Header.Tolerance).Span), UInt128.Zero,
            childrenByParent.Find(o.Id).IfNone(Seq<NodeId>())))));
        return nodes.Values.Filter(static node => node.Parent.IsNone).Bind(root => Seal(root, nodes));
    }

    public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to) {
        HashMap<NodeId, GraphNode> fromByKey = from.ToHashMap(static n => n.Key);
        HashMap<NodeId, GraphNode> toByKey = to.ToHashMap(static n => n.Key);
        Seq<GraphNode> roots = to.Filter(n => n.Parent.Map(p => !toByKey.ContainsKey(p)).IfNone(true));
        return Walk(roots.IsEmpty ? to : roots, fromByKey, toByKey)
             + from.Filter(n => !toByKey.ContainsKey(n.Key)).Map(static n => (EditOp)new EditOp.Delete(n.Key));
    }

    public static MergeOutcome ThreeWay(ElementGraph @base, ElementGraph ours, ElementGraph theirs, Func<NodeId, Option<OpLogEntry>> stampOurs, Func<NodeId, Option<OpLogEntry>> stampTheirs) {
        Seq<GraphNode> baseForest = Forest(@base), ourForest = Forest(ours), theirForest = Forest(theirs);
        // The Generator.Equals Inequalities member diff over the [Equatable] ElementGraph is the AUTHORITATIVE
        // change-set: exactly which Nodes[id]/Edges[i] members each side moved off the base, with old→new
        // evidence — so a node neither side touched never enters the merge and a node BOTH moved is the
        // conflict candidate the Object-forest axis edits then classify (Move/Reorder/Retype/Topology/Parallel).
        HashMap<NodeId, Seq<Inequality>> ourChanges = ChangedNodes(@base, ours);
        HashMap<NodeId, Seq<Inequality>> theirChanges = ChangedNodes(@base, theirs);
        HashMap<NodeId, HashMap<string, EditOp>> ourEdits = ByKeyAxis(Diff(baseForest, ourForest));
        HashMap<NodeId, HashMap<string, EditOp>> theirEdits = ByKeyAxis(Diff(baseForest, theirForest));
        HashMap<NodeId, GraphNode> oursByKey = ourForest.ToHashMap(static n => n.Key);
        HashMap<NodeId, GraphNode> theirsByKey = theirForest.ToHashMap(static n => n.Key);
        Seq<MergeConflict> conflicts = toSeq(ourChanges.Keys.Where(theirChanges.ContainsKey)
            .Bind(key => Conflicts(key, ourEdits.Find(key).IfNone(HashMap<string, EditOp>()), theirEdits.Find(key).IfNone(HashMap<string, EditOp>()), ourChanges[key], theirChanges[key], stampOurs(key), stampTheirs(key)))
            .Append(Cycles(ourEdits, oursByKey, ByOurs: true, stampOurs))
            .Append(Cycles(theirEdits, theirsByKey, ByOurs: false, stampTheirs)));
        HashSet<NodeId> conflicted = conflicts.Map(static c => c.Subject).ToHashSet();
        Seq<EditOp> merged = toSeq(ourEdits.Filter((key, _) => !conflicted.Contains(key)).Map(static (_, axes) => axes.Values).Values.Bind(static ops => ops)
            .Append(theirEdits.Filter((key, _) => !conflicted.Contains(key)).Map((key, axes) => axes.Filter((axis, _) => !ourEdits.Find(key).Map(a => a.ContainsKey(axis)).IfNone(false)).Values).Values.Bind(static ops => ops)));
        return new MergeOutcome(merged, conflicts, Tally(merged, conflicts));
    }

    public static HashMap<NodeId, EntityEdit> Patch(Seq<EditOp> script) =>
        toHashMap(script.GroupBy(static op => op.Target).Map(static group =>
            (group.Key, group.Exists(static op => op is EditOp.Delete)
                ? (EntityEdit)new EntityEdit.Tombstone(group.Key)
                : new EntityEdit.Members(group.Key, group.Fold(new JsonPatchDocument<EntityPatch>(), static (doc, op) => Append(doc, op))))));

    public static ConflictReceipt Project(MergeConflict conflict, ModelId model, CorrelationId correlation, Instant at) => conflict.Map(
        parallelEdit: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        deleteUpdate: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        moveMove: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        reorderReorder: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        typeChange: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        topologyBreak: c => Receipt(c.Key, model, ColumnFamily.Geometry, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
        containmentCycle: c => Receipt(c.Key, model, ColumnFamily.Crdt, c.OurCell, c.OurActor, c.OurCell, c.OurActor, correlation, at));

    // Re-ingest correlation [H6]: the seam mints a FRESH neutral rooted NodeId on every Project, so a re-imported
    // model shares NO NodeId with the persisted graph and a raw NodeId-keyed diff reads the whole import as
    // delete-all + insert-all. Reconcile aligns the re-ingested graph to the DURABLE persisted identities BEFORE
    // the forest diff: it correlates rooted nodes on the stable 1:1 IFC GlobalId (Node.Object.ExternalId — NEVER
    // the minted NodeId), builds the ingest->durable NodeId remap, and rewrites the ingested nodes + every edge
    // endpoint onto the durable ids (an ExternalId absent from the persisted graph keeps its fresh id as a genuine
    // insert; a persisted ExternalId absent from the ingest surfaces as a delete in the diff). So the durable
    // NodeId survives re-import (Element/identity#ELEMENT_IDENTITY) and the NodeId-keyed Diff/ThreeWay below operate
    // on aligned graphs; the same remap applies to a freshly-PROJECTED GraphDelta before it commits so the durable
    // stream never forks, and the content signature drives change DETECTION, never cross-ingest identity.
    public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested) {
        HashMap<string, NodeId> durableByExternal = toHashMap(persisted.Nodes.Values.Choose(ExternalKey));
        HashMap<NodeId, NodeId> remap = toHashMap(ingested.Nodes.Values.Choose(ExternalKey)
            .Choose(pair => durableByExternal.Find(pair.External).Map(durable => (Ingest: pair.Id, Durable: durable)))
            .Filter(static move => move.Ingest != move.Durable));
        return remap.IsEmpty ? (ingested, remap) : (Reindex(ingested, remap), remap);
    }

    // The 1:1 GlobalId correlation key off a ROOTED node — Some only when the node carries an ExternalId (a rooted
    // IFC node); a from-scratch node carries None and stays NodeId-identified (it lives in one authoring lineage,
    // matched by the durable NodeId, never re-minted by a foreign ingest).
    static Option<(string External, NodeId Id)> ExternalKey(Node node) =>
        node is Node.Object { ExternalId: var external } obj ? external.Map(ext => (External: ext, Id: obj.Id)) : None;

    // Rewrite the ingested graph onto the durable ids: remap every node id and every edge endpoint through the
    // ingest->durable map (an unmapped id passes through unchanged), then re-freeze the snapshot.
    static ElementGraph Reindex(ElementGraph graph, HashMap<NodeId, NodeId> remap) {
        NodeId Resolve(NodeId id) => remap.Find(id).IfNone(id);
        FrozenDictionary<NodeId, Node> nodes = graph.Nodes.Values.Select(node => WithId(node, Resolve(node.Id))).ToFrozenDictionary(static node => node.Id);
        ImmutableArray<Relationship> edges = [.. graph.Edges.Select(edge => Reendpoint(edge, Resolve))];
        return ElementGraph.Of(graph.Header, nodes, edges);
    }

    // One polymorphic node-id rewrite — each union case re-stamps its Id, the rest of the payload intact.
    static Node WithId(Node node, NodeId id) => node switch {
        Node.Object o => o with { Id = id },
        Node.Material m => m with { Id = id },
        Node.PropertySet p => p with { Id = id },
        Node.QuantitySet q => q with { Id = id },
        Node.Assessment a => a with { Id = id },
        Node.Appearance ap => ap with { Id = id },
        Node.Coverage c => c with { Id = id },
        _ => node,
    };

    // One polymorphic edge-endpoint rewrite — each neutral case re-maps its typed endpoints (a Connect also re-maps
    // its optional realizing node), the sub-kind and payload intact.
    static Relationship Reendpoint(Relationship edge, Func<NodeId, NodeId> map) => edge switch {
        Relationship.Compose c => c with { Whole = map(c.Whole), Part = map(c.Part) },
        Relationship.Assign a => a with { Subject = map(a.Subject), Definition = map(a.Definition) },
        Relationship.Associate r => r with { Subject = map(r.Subject), Resource = map(r.Resource) },
        Relationship.Connect n => n with { From = map(n.From), To = map(n.To), Realizing = n.Realizing.Map(map) },
        Relationship.Void v => v with { Host = map(v.Host), Feature = map(v.Feature) },
        Relationship.Generic g => g with { Relating = map(g.Relating), Related = map(g.Related) },
        _ => edge,
    };

    static Seq<GraphNode> Seal(GraphNode node, HashMap<NodeId, GraphNode> nodes) {
        Seq<Seq<GraphNode>> children = node.Children.Choose(nodes.Find).Map(child => Seal(child, nodes));
        var acc = new XxHash128();
        Span<byte> frame = stackalloc byte[36];
        BinaryPrimitives.WriteUInt128LittleEndian(frame[..16], node.GeometryHash);
        BinaryPrimitives.WriteUInt128LittleEndian(frame[16..32], node.PropertyHash);
        BinaryPrimitives.WriteInt32LittleEndian(frame[32..36], node.Ordinal);
        acc.Append(frame);
        acc.Append(MemoryMarshal.AsBytes(node.Kind.Key.AsSpan()));
        Span<byte> rollup = stackalloc byte[16];
        foreach (Seq<GraphNode> subtree in children) { BinaryPrimitives.WriteUInt128LittleEndian(rollup, subtree.Head.SubtreeHash); acc.Append(rollup); }
        return Seq1(node with { SubtreeHash = acc.GetCurrentHashAsUInt128() }) + children.Bind(static subtree => subtree);
    }

    static Seq<EditOp> Walk(Seq<GraphNode> frontier, HashMap<NodeId, GraphNode> fromByKey, HashMap<NodeId, GraphNode> toByKey) =>
        toSeq(frontier.OrderBy(static n => n.Ordinal)).Bind(node => fromByKey.Find(node.Key).Match(
            Some: prior => prior.SubtreeHash == node.SubtreeHash ? Seq<EditOp>(new EditOp.Match(node.Key)) : Edit(prior, node) + Descend(node, fromByKey, toByKey),
            None: () => Seq<EditOp>(new EditOp.Insert(node)) + Descend(node, fromByKey, toByKey)));

    static Seq<EditOp> Descend(GraphNode node, HashMap<NodeId, GraphNode> fromByKey, HashMap<NodeId, GraphNode> toByKey) => Walk(node.Children.Choose(toByKey.Find), fromByKey, toByKey);

    static Seq<EditOp> Edit(GraphNode prior, GraphNode node) =>
        (prior.Kind.Key != node.Kind.Key ? Seq<EditOp>(new EditOp.Retype(node.Key, prior.Kind, node.Kind)) : Seq<EditOp>())
        + (prior.Parent != node.Parent ? Seq<EditOp>(new EditOp.Move(node.Key, prior.Parent, node.Parent))
            : prior.Ordinal != node.Ordinal ? Seq<EditOp>(new EditOp.Reorder(node.Key, prior.Ordinal, node.Ordinal)) : Seq<EditOp>())
        + (prior.Matches(node) ? Seq<EditOp>() : Seq<EditOp>(new EditOp.Update(node.Key, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash)));

    static Seq<MergeConflict> Conflicts(NodeId key, HashMap<string, EditOp> ours, HashMap<string, EditOp> theirs, Seq<Inequality> ourDelta, Seq<Inequality> theirDelta, Option<OpLogEntry> o, Option<OpLogEntry> t) {
        (Hlc oc, string oa) = Stamp(o);
        (Hlc tc, string ta) = Stamp(t);
        Seq<MergeConflict> axisConflicts = ours.ContainsKey("delete") && theirs.Keys.Exists(static a => a != "delete")
            ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: true, oc, oa, tc, ta))
            : theirs.ContainsKey("delete") && ours.Keys.Exists(static a => a != "delete")
                ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: false, oc, oa, tc, ta))
                : toSeq(ours.Keys.Filter(theirs.ContainsKey).Choose(axis => Diverge(key, ours[axis], theirs[axis], oc, oa, tc, ta)));
        // A non-Object content node (PropertySet/QuantitySet/Material/Coverage/Assessment) the Object-forest
        // does not topologize, moved off the base by BOTH sides per the Inequalities change-set, is a content
        // ParallelEdit — surfacing a conflict the Object-only forest alone would silently drop.
        return axisConflicts.IsEmpty && ours.IsEmpty && theirs.IsEmpty && !ourDelta.IsEmpty && !theirDelta.IsEmpty
            ? Seq<MergeConflict>(new MergeConflict.ParallelEdit(key, oc, oa, tc, ta))
            : axisConflicts;
    }

    // The Generator.Equals member diff over the [Equatable] ElementGraph, grouped by the changed node key:
    // the authoritative change-set the three-way merge gates conflicts on (which Nodes[id]/Edges[i] moved,
    // with each Inequality's MemberPath + Left/Right old→new evidence) — never a re-hashed whole-node scan.
    static HashMap<NodeId, Seq<Inequality>> ChangedNodes(ElementGraph @base, ElementGraph side) =>
        toHashMap(ElementGraph.EqualityComparer.Default.Inequalities(@base, side)
            .Choose(static ineq => NodeKeyOf(ineq.Path).Map(key => (key, ineq)))
            .GroupBy(static p => p.key)
            .Select(static g => (g.Key, toSeq(g.Select(static p => p.ineq)))));

    static Option<NodeId> NodeKeyOf(MemberPath path) =>
        toSeq(path.Segments).Choose(static seg => Optional(seg.Value as NodeId)).HeadOrNone();

    static Option<MergeConflict> Diverge(NodeId key, EditOp ours, EditOp theirs, Hlc oc, string oa, Hlc tc, string ta) => (ours, theirs) switch {
        (EditOp.Retype r1, EditOp.Retype r2) when r1.ToKind.Key != r2.ToKind.Key => new MergeConflict.TypeChange(key, r1.ToKind, r2.ToKind, oc, oa, tc, ta),
        (EditOp.Move m1, EditOp.Move m2) when m1.ToParent != m2.ToParent => new MergeConflict.MoveMove(key, m1.ToParent, m2.ToParent, oc, oa, tc, ta),
        (EditOp.Reorder r1, EditOp.Reorder r2) when r1.ToOrdinal != r2.ToOrdinal => new MergeConflict.ReorderReorder(key, r1.ToOrdinal, r2.ToOrdinal, oc, oa, tc, ta),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToGeometry != u2.ToGeometry => new MergeConflict.TopologyBreak(key, u1.ToGeometry, u2.ToGeometry, oc, oa, tc, ta),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToProperty != u2.ToProperty => new MergeConflict.ParallelEdit(key, oc, oa, tc, ta),
        _ => Option<MergeConflict>.None,
    };

    static Seq<MergeConflict> Cycles(HashMap<NodeId, HashMap<string, EditOp>> edits, HashMap<NodeId, GraphNode> byKey, bool ByOurs, Func<NodeId, Option<OpLogEntry>> stamp) =>
        toSeq(edits.Keys.Choose(key => ParentOf(key, edits, byKey).Filter(parent => IsDescendant(parent, key, byKey, HashSet<NodeId>()))
            .Map(parent => { (Hlc cell, string actor) = Stamp(stamp(key)); return (MergeConflict)new MergeConflict.ContainmentCycle(key, parent, ByOurs, cell, actor); })));

    static JsonPatchDocument<EntityPatch> Append(JsonPatchDocument<EntityPatch> doc, EditOp op) {
        op.Switch(
            match: static _ => { },
            insert: i => doc.Add(static p => p.Kind, i.Node.Kind.Key).Add(static p => p.Parent, i.Node.Parent).Add(static p => p.Ordinal, i.Node.Ordinal).Add(static p => p.GeometryHash, i.Node.GeometryHash).Add(static p => p.PropertyHash, i.Node.PropertyHash),
            delete: static _ => { },
            update: u => doc.Replace(static p => p.PropertyHash, u.ToProperty).Replace(static p => p.GeometryHash, u.ToGeometry),
            move: m => doc.Replace(static p => p.Parent, m.ToParent),
            reorder: r => doc.Replace(static p => p.Ordinal, r.ToOrdinal),
            retype: r => doc.Replace(static p => p.Kind, r.ToKind.Key));
        return doc;
    }

    static ConflictReceipt Receipt(NodeId key, ModelId model, ColumnFamily family, Hlc held, string heldActor, Hlc incoming, string incomingActor, CorrelationId correlation, Instant at) =>
        new(model, key.Value, family, held, heldActor, incoming, incomingActor, correlation, at);

    static (Hlc Cell, string Actor) Stamp(Option<OpLogEntry> entry) => entry.Match(Some: e => (new Hlc(e.Physical, e.Logical), e.Actor), None: () => (Hlc.Zero, ""));

    static HashMap<NodeId, HashMap<string, EditOp>> ByKeyAxis(Seq<EditOp> script) =>
        toHashMap(script.Filter(static op => op is not EditOp.Match).GroupBy(static op => op.Target).Map(static group => (group.Key, toHashMap(group.GroupBy(static op => op.Axis).Map(static axis => (axis.Key, axis.Last()))))));

    static Option<NodeId> ParentOf(NodeId key, HashMap<NodeId, HashMap<string, EditOp>> edits, HashMap<NodeId, GraphNode> byKey) =>
        edits.Find(key).Bind(static axes => axes.Find("parent")).Bind(static op => op is EditOp.Move m ? m.ToParent : Option<NodeId>.None) | byKey.Find(key).Bind(static node => node.Parent);

    static bool IsDescendant(NodeId candidate, NodeId root, HashMap<NodeId, GraphNode> byKey, HashSet<NodeId> seen) =>
        candidate == root || (!seen.Contains(candidate) && byKey.Find(candidate).Bind(static node => node.Parent).Map(parent => IsDescendant(parent, root, byKey, seen.Add(candidate))).IfNone(false));

    static Seq<TallyFact> Tally(Seq<EditOp> merged, Seq<MergeConflict> conflicts) =>
        toSeq(merged.GroupBy(static op => op.KindName).Map(static g => new TallyFact(TallySlot.Edit, g.Key, g.Count())))
            + toSeq(conflicts.GroupBy(static c => c.KindName).Map(static g => new TallyFact(TallySlot.Conflict, g.Key, g.Count())));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                    | [BINDING]                                                   |
| :-----: | :-------------------- | :----------------------------------------- | :--------------------------------------------------------- |
|  [01]   | change-set            | `Generator.Equals` `Inequalities` over `[Equatable]` `ElementGraph` | the authoritative moved-member set both sides gate on; node identity is `NodeId`, never a GUID |
|  [02]   | forest topology       | `Relationship.Compose` containment edges   | `Forest` derives `Parent`/`Ordinal`/`Children`; no second store |
|  [03]   | geometry axis         | `Object.Representations.Body`              | the kernel content digest, read not re-minted             |
|  [04]   | subtree prune         | `XxHash128.Append`, stable LE preimage     | linear in changed nodes, no `GetHashCode`                  |
|  [05]   | conflict accumulation | `MergeOutcome` carries merged + conflicts  | one-pass `Classify`, both carried, never first-abort       |
|  [06]   | patch egress          | `HashMap<NodeId, EntityEdit>`              | one egress kind per key; delete is a tombstone            |
|  [07]   | conflict receipt      | `Version/ledger#MERGE_LAW` `ConflictReceipt` | held/incoming `(Hlc, actor)` from the changefeed         |
|  [08]   | re-ingest align       | `Reconcile` on `Node.Object.ExternalId`    | freshly-minted rooted `NodeId` aligned to the durable id on the 1:1 GlobalId; never a `NodeId`-keyed re-ingest diff |
