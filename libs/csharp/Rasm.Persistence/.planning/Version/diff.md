# [PERSISTENCE_STRUCTURAL_DIFF]

Rasm.Persistence geometry-aware structural diff/merge: a `FederatedEntity → GraphNode` forest projection, a Merkle-pruned forest diff matching nodes by stable composite identity then by content signature, a base-relative three-way merge that surfaces every typed conflict in one pass, and a per-node RFC 6902 patch egress the inspector and CDE consume. The diff operands are `GraphNode` projections of the `Query/federation#ENTITY_GRAPH` `FederatedEntity` — so a structural diff keys on the same `UInt128` composite identity the federation keys on, never a parallel node identity, and reads its forest topology off the `ContainmentPath` ltree rather than a second store. Node content is the one `(GeometryHash, PropertyHash)` identity the `ARCHITECTURE.md` `PROHIBITIONS` fixes as the sole node-identity axis: `GraphNode.GeometryHash` IS the federation `FederatedEntity.GeometryRef`, the adjacency-derived digest `Rasm.Geometry/Spatial/reconciliation#NAMING_HASH` `Encode` mints and the diff READS not re-derives, and `GraphNode.PropertyHash` IS the federation `EntityIdentity.PsetHash` — two federation-sourced content keys, never a third domain-minted axis. A geometry change at a stable key is an `Update`, a concurrent divergent geometry change is a `TopologyBreak`, and a property-only change is a `ParallelEdit` when both sides diverge — the diff distinguishes these by `GeometryHash`/`PropertyHash` inequality alone. The `SubtreeHash` Merkle digest prunes byte-identical subtrees over a deterministic LE preimage so the cost is linear in the changed-node count, never quadratic over the whole graph; merge conflicts project to the settled `Sync/collaboration#MERGE_LAW` `ConflictReceipt` carrying the real `(Hlc, actor)` evidence both sides stamp.

## [01]-[INDEX]

- [01]-[STRUCTURAL_DIFF]: Merkle-pruned forest match, base-relative three-way merge, typed conflict classes, and RFC 6902 patch egress.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` the identity-keyed forest node carrying the federation geometry-adjacency content key (`GeometryHash` ← `FederatedEntity.GeometryRef`), the property-set content key (`PropertyHash` ← `EntityIdentity.PsetHash`), the sibling ordinal, and the Merkle subtree digest; `NodeKind` `[SmartEnum<string>]` the IFC/federation classification vocabulary whose `From` projection lowers an `IfcSpace`/`IfcZone`/`IfcBuilding`/`IfcElementAssembly`/`IfcAnnotation` classification string to the node kind the retype conflict reads; `EditOp` `[Union]` the forest-edit operation family driven by a generated `Switch`/`Map`, carrying the `Target` key projection and the `KindName` discriminant; `MergeConflict` `[Union]` the typed conflict-class family carrying both sides' op-log stamp — conflict-as-data settling into the receipt, never a thrown fault, mirroring the `Sync/collaboration#MERGE_LAW` `ConflictResult` data modeling; `EntityEdit` `[Union]` the per-key egress kind (`Tombstone` whole-entity removal vs `Members` RFC 6902 member-patch) so a node delete is a typed tombstone the inspector applies by dropping the snapshot, never a field-level `remove` on the entity's own patch document; `TallySlot` `[SmartEnum]` the keyless edit-versus-conflict count axis and `TallyFact` the one slot/kind/count fact carrying both buckets; `MergeOutcome` the settled three-way receipt carrying the merged edit script, the conflict sequence, and the by-kind `TallyFact` stream; `StructuralMerge` the static surface owning the `FederatedEntity → GraphNode` forest projection, the Merkle-pruned forest diff, the base-relative three-way merge, the conflict classification fold, the per-node RFC 6902 patch projection, and the `ConflictReceipt` projection.
- Cases: `Match | Insert | Delete | Update | Move | Reorder | Retype` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | ReorderReorder | TypeChange | TopologyBreak | ContainmentCycle` on `MergeConflict`; `Tombstone | Members` on `EntityEdit`; the patch egress maps `Delete → Tombstone`, `Insert → Members` over the inserted node's full member set with `Add` ops, and `Update`/`Move`/`Reorder`/`Retype → Members` with `Replace` ops over the member the op targets, so the resolved script crosses as a `HashMap<UInt128, EntityEdit>` — one egress kind per touched entity key, never one flat document conflating N nodes — and the inspector applies break-on-first-error to each `Members` patch against the node's snapshot clone while a `Tombstone` drops the clone outright.
- Entry: `public static Seq<GraphNode> Forest(Seq<FederatedEntity> graph)` — the `FederatedEntity → GraphNode` projection deriving each node's `Parent`/`Ordinal`/`Children` forest topology from the `ContainmentPath` ltree, its `GeometryHash` from the federation `GeometryRef`, its `PropertyHash` from `EntityIdentity.PsetHash`, and its `NodeKind` from the IFC classification, then seals the `SubtreeHash` Merkle digest bottom-up; `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` — the minimal forest-edit script, matching by composite key then by `(GeometryHash, PropertyHash)` signature, descending only the subtrees whose `SubtreeHash` diverges; `public static MergeOutcome ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs, Func<UInt128, Option<OpLogEntry>> stampOurs, Func<UInt128, Option<OpLogEntry>> stampTheirs)` computes the two base-relative scripts, folds non-conflicting edits — including orthogonal both-sides edits like a `Move` beside an `Update` — into the merged script, and accumulates only true overlaps as typed conflicts each carrying both sides' op-log stamp; `public static HashMap<UInt128, EntityEdit> Patch(Seq<EditOp> script)` groups the resolved script by target key and projects each node's edits to its own `EntityEdit` egress kind; `public static ConflictReceipt Project(MergeConflict conflict, CorrelationId correlation, Instant at)` reuses the one conflict vocabulary.
- Auto: node matching is the federation composite key first, then the `(GeometryHash, PropertyHash)` content signature so a re-imported model whose source ids changed still matches unchanged nodes by signature and a transformed node matches by key with the op carrying the delta — the forest edit emits `Match` for signature-equal same-parent nodes, `Move` for signature-equal parent-changed nodes, `Reorder` for same-parent ordinal-changed nodes, `Retype` for `NodeKind`-changed nodes, `Update` for signature-changed nodes carrying both content axes, and `Insert`/`Delete` for unmatched; the `SubtreeHash` Merkle digest folds each node's own `(GeometryHash, PropertyHash, Ordinal, Kind.Key)` preimage with its sorted children's subtree digests through the `XxHash128.Append`/`GetCurrentHashAsUInt128` accumulator over a stable LE byte stream (no `GetHashCode`), so the diff compares one digest per node and descends only where it diverges — a byte-identical subtree under an unchanged root is pruned whole, making the matched-forest cost linear in the changed-node count; the three-way merge diffs base→ours and base→theirs, applies every edit on a key neither side conflicts on AND both orthogonal ops where the two sides edit one key on non-overlapping axes (parent vs content vs ordinal), and folds every genuine overlap into a `MergeConflict` whose class names the structural cause; a `GeometryHash` change at a stable key is an `Update` and two sides' divergent `GeometryHash` is a `TopologyBreak` — the adjacency-derived hash carries the only geometry-change signal the federation owns, so a control-point morph that the adjacency digest leaves invariant is convergence-neutral by construction.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count by class folded into the `MergeOutcome.Counts` `Seq<TallyFact>` whose `TallySlot`/`KindName`/`Count` rows derive from the generated `EditOp.KindName`/`MergeConflict.KindName` discriminant, never a reflected `GetType().Name`, and each `MergeConflict` projects to `ConflictReceipt` so the inspector surface reads one conflict vocabulary with the held/incoming `(Hlc, actor)` stamps populated from the op-log, never a zero-filled placeholder.
- Packages: System.IO.Hashing (`XxHash128.Append`/`GetCurrentHashAsUInt128`/`Clone`), Microsoft.AspNetCore.JsonPatch.SystemTextJson, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, `System.Buffers.Binary`/`System.Runtime.InteropServices`/`System.Globalization` (BCL inbox).
- Growth: a new edit op is one `EditOp` case plus one `Switch`/`Map` arm plus one `KindName`/`Target` arm the generated dispatch breaks the build on; a new conflict class is one `MergeConflict` case plus one `KindName`/`Subject` arm; a new identity signature is one column on `GraphNode`; a new egress kind is one `EntityEdit` case the total `Patch` switch breaks the build on; zero new surface — a line-based text diff, a JSON-patch diff over serialized nodes, a reflection-keyed count map, a third content axis beside the federation `(GeometryHash, PropertyHash)`, or a second merge resolver is the deleted form because the diff matches on geometry/property identity and the merge classifies by structural cause; the existing set-difference `SyncPump.GraphDiff` (closure-membership transfer) stays the transport-level diff while this owns the semantic node-level diff, and the `Version/timetravel#TIME_TRAVEL` `RangeDiff` owns the cell-level CRDT-field delta — three altitude-split diffs, never duplicated.
- Boundary: node identity is the `Query/federation#ENTITY_GRAPH` composite key (`UInt128` `XxHash128` over the five-axis `EntityIdentity` tuple) first, then the `(GeometryHash, PropertyHash)` content signature so a re-imported model whose ids changed still matches unchanged nodes by signature — a positional or ordinal-only match is the deleted form, and a `Guid`-keyed parallel node identity is the named defect because the federation already owns the one composite key the diff matches on. The `FederatedEntity → GraphNode` projection is `Forest`: forest topology (`Parent`, `Ordinal`, `Children`) reads VERBATIM from the federation `ContainmentPath` ltree — the parent is the `ltree` minus its last label resolved through the path→key map, the ordinal is the trailing numeric label, the sibling order is `Ordinal`-then-path stable — so a positional re-derivation or a second containment store is the deleted form. The node carries the one `ARCHITECTURE.md`-fixed `(GeometryHash, PropertyHash)` identity, two federation-sourced content keys and no third domain-minted axis. `GraphNode.GeometryHash` IS the federation `FederatedEntity.GeometryRef` — the adjacency-derived digest whose canonical byte layout is OWNED by `Rasm.Geometry/Spatial/reconciliation#NAMING_HASH` (the sole owner; the geometry domain names it `GeometryHash` and this page reads the identical value under the same name): `int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding, hashed by `XxHash128.HashToUInt128` — the diff READS this digest off the federation row and NEVER re-derives a second encoding. Because the federation owns exactly this one adjacency-derived geometry key, the diff carries no fabricated full-embedding axis: a topology break is `GeometryHash` inequality, and a control-point morph that leaves adjacency unchanged is convergence-neutral by construction (it changes no federation-held content key), so the diff distinguishes `Update` (signature change at stable key) from `Move` (signature stable, parent change) from `Reorder` (signature and parent stable, ordinal change) from `Retype` (`NodeKind` change) from `TopologyBreak` (concurrent divergent `GeometryHash`). `GraphNode.PropertyHash` reads the federation `EntityIdentity.PsetHash` (the one property-set content key the federation already mints), never a re-digest of the `HashMap<string, CrdtField> PropertySets`. The `SubtreeHash` is `XxHash128` over the node's own `(GeometryHash, PropertyHash, Ordinal)` LE preimage plus the `Kind.Key` UTF-16 bytes, folded with the sorted child `SubtreeHash` digests through the `XxHash128.Append`/`GetCurrentHashAsUInt128` accumulator — a stable deterministic byte stream with NO `GetHashCode`, so two peers seal the identical digest and two subtrees with identical content and child order collide so the forest fold prunes them in one digest compare; a full quadratic node-pair scan is the deleted form. The three-way merge is base-relative — it never compares ours to theirs directly, so a node both sides left untouched never enters a conflict, and a node one side deleted while the other updated is a `DeleteUpdate` conflict the inspector resolves; a structural merge is always-succeeds-with-annotations rather than fail-or-succeed, so `ThreeWay` returns `MergeOutcome` carrying BOTH the clean merged edit script AND the full conflict sequence in one pass — the `Classify` fold surfaces every overlap conflict at once rather than aborting on the first, exactly as `Sync/collaboration#MERGE_LAW` `SyncApplyReceipt` carries its conflict sequence beside the applied counts, never a `Validation` fail-XOR-succeed that would discard the partial merge. Two-sided edits on one key partition into genuine overlaps (`Classify` names the conflict) and orthogonal compatible edits (a `Move` on one side beside an `Update` on the other touch different axes), and the merged script applies BOTH compatible ops rather than silently dropping the second side. Each `MergeConflict` carries both sides' `(Hlc, actor)` resolved from the op-log stamp delegates so `Project` mints a `ConflictReceipt` with real held/incoming evidence — a structural conflict is a real conflict between two stamped op-log writes, never a synthetic zero-stamp, which is why the receipt reuses `Sync/collaboration#MERGE_LAW` `ConflictReceipt` rather than minting a second receipt taxonomy. The merge result re-projects two ways: as `EditOp` rows applied through the CRDT algebra (`Version/commits#CRDT_ALGEBRA` `RgaSequence` carries the sibling order a `Reorder` resolves) so a merged structural change converges like any other op, and as a `HashMap<UInt128, EntityEdit>` — ONE egress kind per touched entity key, never one flat document conflating N nodes — where a `Members` patch is an RFC 6902 document the inspector or CDE applies break-on-first-error to the node's snapshot clone (the typed `Expression`-path builder binding each op to the snapshot member so a renamed property breaks the build rather than producing a silent no-op `path`) and a `Tombstone` drops the clone outright, because a whole-entity delete cannot be a field-level `remove` on the entity's own document. A `ContainmentCycle` conflict (a `Move` whose target becomes its own descendant) is detected on BOTH sides — `Cycles` checks `ours` and `theirs` independently, each `ContainmentCycle` carrying the `ByOurs` side discriminant — and the cycle walk is visited-set-bounded so the detector terminates on the very cyclic input it rejects rather than recursing forever; the cycle-bearing key is excluded from the merged script before patch projection because an ltree containment path admits no cycle.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
public sealed class NodeKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NodeKeyPolicy, string>]
[KeyMemberComparer<NodeKeyPolicy, string>]
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
public readonly record struct GraphNode(
    UInt128 Key,
    NodeKind Kind,
    Option<UInt128> Parent,
    int Ordinal,
    UInt128 GeometryHash,
    UInt128 PropertyHash,
    UInt128 SubtreeHash,
    Seq<UInt128> Children) {
    public UInt128 Signature => SubtreeHash;

    public bool Matches(GraphNode other) =>
        GeometryHash == other.GeometryHash && PropertyHash == other.PropertyHash;
}

public sealed record EntityPatch(
    string Kind, Option<UInt128> Parent, int Ordinal, UInt128 GeometryHash, UInt128 PropertyHash);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EditOp {
    private EditOp() { }

    public sealed record Match(UInt128 Key) : EditOp;
    public sealed record Insert(GraphNode Node) : EditOp;
    public sealed record Delete(UInt128 Key) : EditOp;
    public sealed record Update(UInt128 Key, UInt128 FromProperty, UInt128 ToProperty, UInt128 FromGeometry, UInt128 ToGeometry) : EditOp;
    public sealed record Move(UInt128 Key, Option<UInt128> FromParent, Option<UInt128> ToParent) : EditOp;
    public sealed record Reorder(UInt128 Key, int FromOrdinal, int ToOrdinal) : EditOp;
    public sealed record Retype(UInt128 Key, NodeKind FromKind, NodeKind ToKind) : EditOp;

    public UInt128 Target => this.Map(
        match: m => m.Key, insert: i => i.Node.Key, delete: d => d.Key,
        update: u => u.Key, move: v => v.Key, reorder: r => r.Key, retype: t => t.Key);

    public string KindName => this.Map(
        match: static _ => "match", insert: static _ => "insert", delete: static _ => "delete",
        update: static _ => "update", move: static _ => "move", reorder: static _ => "reorder", retype: static _ => "retype");

    public string Axis => this.Map(
        match: static _ => "", insert: static _ => "insert", delete: static _ => "delete",
        update: static _ => "content", move: static _ => "parent", reorder: static _ => "ordinal", retype: static _ => "kind");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record MergeConflict {
    private MergeConflict() { }

    public sealed record ParallelEdit(UInt128 Key, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record DeleteUpdate(UInt128 Key, bool DeletedByOurs, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record MoveMove(UInt128 Key, Option<UInt128> OurParent, Option<UInt128> TheirParent, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ReorderReorder(UInt128 Key, int OurOrdinal, int TheirOrdinal, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TypeChange(UInt128 Key, NodeKind OurKind, NodeKind TheirKind, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TopologyBreak(UInt128 Key, UInt128 OurGeometry, UInt128 TheirGeometry, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ContainmentCycle(UInt128 Key, UInt128 Ancestor, bool ByOurs, Hlc OurCell, string OurActor) : MergeConflict;

    public UInt128 Subject => this.Map(
        parallelEdit: p => p.Key, deleteUpdate: d => d.Key, moveMove: m => m.Key,
        reorderReorder: r => r.Key, typeChange: t => t.Key, topologyBreak: b => b.Key, containmentCycle: y => y.Key);

    public string KindName => this.Map(
        parallelEdit: static _ => "parallelEdit", deleteUpdate: static _ => "deleteUpdate", moveMove: static _ => "moveMove",
        reorderReorder: static _ => "reorderReorder", typeChange: static _ => "typeChange", topologyBreak: static _ => "topologyBreak", containmentCycle: static _ => "containmentCycle");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EntityEdit {
    private EntityEdit() { }

    public sealed record Tombstone(UInt128 Key) : EntityEdit;
    public sealed record Members(UInt128 Key, JsonPatchDocument<EntityPatch> Patch) : EntityEdit;
}

public readonly record struct TallyFact(TallySlot Slot, string Kind, int Count);

public readonly record struct MergeOutcome(Seq<EditOp> Merged, Seq<MergeConflict> Conflicts, Seq<TallyFact> Counts) {
    public bool Clean => Conflicts.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StructuralMerge {
    public static Seq<GraphNode> Forest(Seq<FederatedEntity> graph) {
        HashMap<string, UInt128> keyByPath = graph.ToHashMap(static e => e.ContainmentPath, static e => e.Key);
        HashMap<UInt128, UInt128> parentByKey = graph.Choose(entity =>
            ParentPath(entity.ContainmentPath).Bind(keyByPath.Find).Map(parent => (entity.Key, parent))).ToHashMap();
        HashMap<UInt128, Seq<UInt128>> childrenByParent = toHashMap(graph
            .GroupBy(entity => parentByKey.Find(entity.Key))
            .Choose(group => group.Key.Map(parent =>
                (parent, toSeq(group.OrderBy(static e => e.ContainmentPath, StringComparer.Ordinal)).Map(static e => e.Key)))));
        HashMap<UInt128, GraphNode> nodes = graph.ToHashMap(static e => e.Key, entity => new GraphNode(
            entity.Key, NodeKind.From(entity.Identity.Classification), parentByKey.Find(entity.Key),
            Ordinal(entity.ContainmentPath), entity.GeometryRef, entity.Identity.PsetHash,
            UInt128.Zero, childrenByParent.Find(entity.Key).IfNone(Seq<UInt128>())));
        return nodes.Values.Filter(static node => node.Parent.IsNone).Bind(root => Seal(root, nodes));
    }

    public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to) {
        HashMap<UInt128, GraphNode> fromByKey = from.ToHashMap(static node => node.Key);
        HashMap<UInt128, GraphNode> toByKey = to.ToHashMap(static node => node.Key);
        Seq<GraphNode> roots = to.Filter(node => node.Parent.Map(p => !toByKey.ContainsKey(p)).IfNone(true));
        return Walk(roots.IsEmpty ? to : roots, fromByKey, toByKey)
             + from.Filter(node => !toByKey.ContainsKey(node.Key)).Map(static node => (EditOp)new EditOp.Delete(node.Key));
    }

    public static MergeOutcome ThreeWay(
        Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs,
        Func<UInt128, Option<OpLogEntry>> stampOurs, Func<UInt128, Option<OpLogEntry>> stampTheirs) {
        // One key may carry several axis-ops on a side (a node that both moved and changed content), so edits group
        // per key as a Seq and conflict detection runs per shared axis — a single-op-per-key HashMap would drop the second.
        HashMap<UInt128, HashMap<string, EditOp>> ourEdits = ByKeyAxis(Diff(baseGraph, ours));
        HashMap<UInt128, HashMap<string, EditOp>> theirEdits = ByKeyAxis(Diff(baseGraph, theirs));
        HashMap<UInt128, GraphNode> oursByKey = ours.ToHashMap(static node => node.Key);
        HashMap<UInt128, GraphNode> theirsByKey = theirs.ToHashMap(static node => node.Key);
        Seq<MergeConflict> conflicts = toSeq(ourEdits.Keys
            .Where(theirEdits.ContainsKey)
            .Bind(key => Conflicts(key, ourEdits[key], theirEdits[key], stampOurs(key), stampTheirs(key)))
            .Append(Cycles(ourEdits, oursByKey, ByOurs: true, stampOurs))
            .Append(Cycles(theirEdits, theirsByKey, ByOurs: false, stampTheirs)));
        HashSet<UInt128> conflicted = conflicts.Map(static c => c.Subject).ToHashSet();
        // Every non-conflicted axis-op applies — ours fully, theirs only where ours did not already write that axis —
        // so a Move beside an Update merges both and a node both sides moved to one parent is not double-applied.
        Seq<EditOp> merged = toSeq(ourEdits
            .Filter((key, _) => !conflicted.Contains(key))
            .Map(static (_, axes) => axes.Values).Values.Bind(static ops => ops)
            .Append(theirEdits
                .Filter((key, _) => !conflicted.Contains(key))
                .Map((key, axes) => axes.Filter((axis, _) => !ourEdits.Find(key).Map(a => a.ContainsKey(axis)).IfNone(false)).Values)
                .Values.Bind(static ops => ops)));
        return new MergeOutcome(merged, conflicts, Tally(merged, conflicts));
    }

    public static HashMap<UInt128, EntityEdit> Patch(Seq<EditOp> script) =>
        toHashMap(script.GroupBy(static op => op.Target).Map(static group =>
            (group.Key, group.Exists(static op => op is EditOp.Delete)
                ? (EntityEdit)new EntityEdit.Tombstone(group.Key)
                : new EntityEdit.Members(group.Key, group.Fold(new JsonPatchDocument<EntityPatch>(), static (doc, op) => Append(doc, op))))));

    public static ConflictReceipt Project(MergeConflict conflict, CorrelationId correlation, Instant at) =>
        conflict.Map(
            parallelEdit:    c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            deleteUpdate:    c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            moveMove:        c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            reorderReorder:  c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            typeChange:      c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            topologyBreak:   c => Receipt(c.Key, c.OurCell, c.OurActor, c.TheirCell, c.TheirActor, correlation, at),
            containmentCycle: c => Receipt(c.Key, c.OurCell, c.OurActor, c.OurCell, c.OurActor, correlation, at));

    // --- [FOREST_PROJECTION] -----------------------------------------------------------
    private static Option<string> ParentPath(string ltree) =>
        ltree.LastIndexOf('.') is var dot && dot > 0 ? Some(ltree[..dot]) : Option<string>.None;

    private static int Ordinal(string ltree) =>
        int.TryParse(ltree.AsSpan(ltree.LastIndexOf('.') + 1), CultureInfo.InvariantCulture, out int label) ? label : 0;

    private static Seq<GraphNode> Seal(GraphNode node, HashMap<UInt128, GraphNode> nodes) {
        Seq<Seq<GraphNode>> children = node.Children.Choose(nodes.Find).Map(child => Seal(child, nodes));
        var acc = new XxHash128();
        Span<byte> frame = stackalloc byte[36];
        BinaryPrimitives.WriteUInt128LittleEndian(frame[..16], node.GeometryHash);
        BinaryPrimitives.WriteUInt128LittleEndian(frame[16..32], node.PropertyHash);
        BinaryPrimitives.WriteInt32LittleEndian(frame[32..36], node.Ordinal);
        acc.Append(frame);
        acc.Append(MemoryMarshal.AsBytes(node.Kind.Key.AsSpan()));
        Span<byte> rollup = stackalloc byte[16];
        foreach (Seq<GraphNode> subtree in children) {
            BinaryPrimitives.WriteUInt128LittleEndian(rollup, subtree.Head.SubtreeHash);
            acc.Append(rollup);
        }
        return Seq1(node with { SubtreeHash = acc.GetCurrentHashAsUInt128() }) + children.Bind(static subtree => subtree);
    }

    // --- [FOREST_FOLD] -----------------------------------------------------------------
    private static Seq<EditOp> Walk(Seq<GraphNode> frontier, HashMap<UInt128, GraphNode> fromByKey, HashMap<UInt128, GraphNode> toByKey) =>
        toSeq(frontier.OrderBy(static node => node.Ordinal)).Bind(node => fromByKey.Find(node.Key).Match(
            Some: prior => prior.SubtreeHash == node.SubtreeHash
                ? Seq<EditOp>(new EditOp.Match(node.Key))
                : Edit(prior, node) + Descend(node, fromByKey, toByKey),
            None: () => Seq<EditOp>(new EditOp.Insert(node)) + Descend(node, fromByKey, toByKey)));

    private static Seq<EditOp> Descend(GraphNode node, HashMap<UInt128, GraphNode> fromByKey, HashMap<UInt128, GraphNode> toByKey) =>
        Walk(node.Children.Choose(toByKey.Find), fromByKey, toByKey);

    private static Seq<EditOp> Edit(GraphNode prior, GraphNode node) =>
        (prior.Kind.Key != node.Kind.Key
            ? Seq<EditOp>(new EditOp.Retype(node.Key, prior.Kind, node.Kind)) : Seq<EditOp>())
        + (prior.Parent != node.Parent
            ? Seq<EditOp>(new EditOp.Move(node.Key, prior.Parent, node.Parent))
            : prior.Ordinal != node.Ordinal ? Seq<EditOp>(new EditOp.Reorder(node.Key, prior.Ordinal, node.Ordinal)) : Seq<EditOp>())
        + (prior.Matches(node)
            ? Seq<EditOp>()
            : Seq<EditOp>(new EditOp.Update(node.Key, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash)));

    // --- [CLASSIFY] --------------------------------------------------------------------
    // A delete on one side conflicts with the other side's WHOLE edit set (DeleteUpdate); otherwise each shared axis
    // classifies its own divergence — Move/Reorder/Retype keep their axis, Update splits TopologyBreak vs ParallelEdit.
    private static Seq<MergeConflict> Conflicts(UInt128 key, HashMap<string, EditOp> ours, HashMap<string, EditOp> theirs, Option<OpLogEntry> o, Option<OpLogEntry> t) {
        (Hlc oc, string oa) = Stamp(o);
        (Hlc tc, string ta) = Stamp(t);
        return ours.ContainsKey("delete") && theirs.Keys.Exists(static a => a != "delete")
            ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: true, oc, oa, tc, ta))
            : theirs.ContainsKey("delete") && ours.Keys.Exists(static a => a != "delete")
                ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: false, oc, oa, tc, ta))
                : toSeq(ours.Keys.Filter(theirs.ContainsKey).Choose(axis => Diverge(key, ours[axis], theirs[axis], oc, oa, tc, ta)));
    }

    private static Option<MergeConflict> Diverge(UInt128 key, EditOp ours, EditOp theirs, Hlc oc, string oa, Hlc tc, string ta) =>
        (ours, theirs) switch {
            (EditOp.Retype r1, EditOp.Retype r2) when r1.ToKind.Key != r2.ToKind.Key => new MergeConflict.TypeChange(key, r1.ToKind, r2.ToKind, oc, oa, tc, ta),
            (EditOp.Move m1, EditOp.Move m2) when m1.ToParent != m2.ToParent => new MergeConflict.MoveMove(key, m1.ToParent, m2.ToParent, oc, oa, tc, ta),
            (EditOp.Reorder r1, EditOp.Reorder r2) when r1.ToOrdinal != r2.ToOrdinal => new MergeConflict.ReorderReorder(key, r1.ToOrdinal, r2.ToOrdinal, oc, oa, tc, ta),
            (EditOp.Update u1, EditOp.Update u2) when u1.ToGeometry != u2.ToGeometry => new MergeConflict.TopologyBreak(key, u1.ToGeometry, u2.ToGeometry, oc, oa, tc, ta),
            (EditOp.Update u1, EditOp.Update u2) when u1.ToProperty != u2.ToProperty => new MergeConflict.ParallelEdit(key, oc, oa, tc, ta),
            _ => Option<MergeConflict>.None,
        };

    private static Seq<MergeConflict> Cycles(HashMap<UInt128, HashMap<string, EditOp>> edits, HashMap<UInt128, GraphNode> byKey, bool ByOurs, Func<UInt128, Option<OpLogEntry>> stamp) =>
        toSeq(edits.Keys.Choose(key => ParentOf(key, edits, byKey)
            .Filter(parent => IsDescendant(parent, key, byKey, HashSet<UInt128>()))
            .Map(parent => { (Hlc cell, string actor) = Stamp(stamp(key)); return (MergeConflict)new MergeConflict.ContainmentCycle(key, parent, ByOurs, cell, actor); })));

    // --- [PROJECTIONS] -----------------------------------------------------------------
    private static JsonPatchDocument<EntityPatch> Append(JsonPatchDocument<EntityPatch> doc, EditOp op) {
        // BOUNDARY ADAPTER — JsonPatchDocument fluent ops mutate and return this; the builder is the RFC 6902 egress seam.
        // Delete never reaches here: Patch projects any Delete-bearing key to EntityEdit.Tombstone, so the arm stays a no-op.
        op.Switch(
            match:   static _ => { },
            insert:  i => doc.Add(static p => p.Kind, i.Node.Kind.Key)
                            .Add(static p => p.Parent, i.Node.Parent).Add(static p => p.Ordinal, i.Node.Ordinal)
                            .Add(static p => p.GeometryHash, i.Node.GeometryHash)
                            .Add(static p => p.PropertyHash, i.Node.PropertyHash),
            delete:  static _ => { },
            update:  u => doc.Replace(static p => p.PropertyHash, u.ToProperty).Replace(static p => p.GeometryHash, u.ToGeometry),
            move:    m => doc.Replace(static p => p.Parent, m.ToParent),
            reorder: r => doc.Replace(static p => p.Ordinal, r.ToOrdinal),
            retype:  r => doc.Replace(static p => p.Kind, r.ToKind.Key));
        return doc;
    }

    private static ConflictReceipt Receipt(UInt128 key, Hlc held, string heldActor, Hlc incoming, string incomingActor, CorrelationId correlation, Instant at) =>
        new("structural", key.ToString("x32"), "structural",
            held.Physical, held.Logical, heldActor,
            incoming.Physical, incoming.Logical, incomingActor,
            correlation, at);

    private static (Hlc Cell, string Actor) Stamp(Option<OpLogEntry> entry) =>
        entry.Match(Some: e => (new Hlc(e.Physical, e.Logical), e.Actor), None: () => (Hlc.Zero, ""));

    private static bool NonMatch(EditOp op) => op is not EditOp.Match;

    private static HashMap<UInt128, HashMap<string, EditOp>> ByKeyAxis(Seq<EditOp> script) =>
        toHashMap(script.Filter(NonMatch).GroupBy(static op => op.Target).Map(static group =>
            (group.Key, toHashMap(group.GroupBy(static op => op.Axis).Map(static axis => (axis.Key, axis.Last()))))));

    private static Option<UInt128> ParentOf(UInt128 key, HashMap<UInt128, HashMap<string, EditOp>> edits, HashMap<UInt128, GraphNode> byKey) =>
        edits.Find(key).Bind(static axes => axes.Find("parent")).Bind(static op => op is EditOp.Move m ? m.ToParent : Option<UInt128>.None)
            | byKey.Find(key).Bind(static node => node.Parent);

    private static bool IsDescendant(UInt128 candidate, UInt128 root, HashMap<UInt128, GraphNode> byKey, HashSet<UInt128> seen) =>
        candidate == root
            || (!seen.Contains(candidate)
                && byKey.Find(candidate).Bind(static node => node.Parent)
                    .Map(parent => IsDescendant(parent, root, byKey, seen.Add(candidate))).IfNone(false));

    private static Seq<TallyFact> Tally(Seq<EditOp> merged, Seq<MergeConflict> conflicts) =>
        toSeq(merged.GroupBy(static op => op.KindName).Map(static g => new TallyFact(TallySlot.Edit, g.Key, g.Count())))
            + toSeq(conflicts.GroupBy(static c => c.KindName).Map(static g => new TallyFact(TallySlot.Conflict, g.Key, g.Count())));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                    | [BINDING]                                                   |
| :-----: | :-------------------- | :----------------------------------------- | :--------------------------------------------------------- |
|  [01]   | node identity         | `FederatedEntity.Key` `UInt128`            | `Query/federation#ENTITY_GRAPH` composite key, never a GUID |
|  [02]   | forest topology       | `FederatedEntity.ContainmentPath` ltree    | `Forest` derives `Parent`/`Ordinal`/`Children`; no second store |
|  [03]   | geometry axis         | `FederatedEntity.GeometryRef` (`reconciliation#NAMING_HASH` `Encode`) | `GraphNode.GeometryHash`; the one `ARCHITECTURE.md` geometry key, read not re-minted |
|  [04]   | property axis         | `EntityIdentity.PsetHash`                  | `GraphNode.PropertyHash`; the federation pset key, no re-digest |
|  [05]   | subtree prune         | `XxHash128.Append` over child digests, stable LE preimage | `GraphNode.SubtreeHash`; linear in changed nodes, no `GetHashCode` |
|  [06]   | conflict accumulation | `MergeOutcome` carries merged + conflicts  | one-pass `Classify` fold, both carried, never first-abort   |
|  [07]   | orthogonal merge      | `EditOp.Axis` per touched key              | both compatible ops apply (Move beside Update), never drop theirs |
|  [08]   | count receipt         | `Seq<TallyFact>` slot/kind/count           | `EditOp.KindName`/`MergeConflict.KindName`, never `GetType().Name` |
|  [09]   | patch egress          | `HashMap<UInt128, EntityEdit>` (`Tombstone`/`Members`) | one egress kind per touched key; delete is a tombstone, never a field `remove` |
|  [10]   | conflict receipt      | `Sync/collaboration#MERGE_LAW` `ConflictReceipt` | held/incoming `(Hlc, actor)` from op-log, no zero stamp |

## [03]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] — `StructuralMerge` `GraphNode.GeometryHash` IS the federation `FederatedEntity.GeometryRef`, the adjacency-derived digest whose FROZEN field order is owned by `Rasm.Geometry/Spatial/reconciliation#NAMING_HASH` (`Encode`): `int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding, hashed by `XxHash128.HashToUInt128`. The diff READS this digest off the federation row and NEVER re-derives a second encoding. The geometry domain is the sole owner and names the digest `GeometryHash`; this page reads the identical value under the same axis name `GeometryHash`, so there is one cross-page name and the `ARCHITECTURE.md` `(GeometryHash, PropertyHash)` node-identity law holds with no rename residual. The digest is adjacency-derived: a topology break changes adjacency and re-hashes distinctly, while a pure control-point morph that preserves adjacency leaves the only federation-held geometry key unchanged and is therefore convergence-neutral by construction — the federation models no separate full-embedding key, so the diff carries no second geometry axis and fabricates no `shapeOf` producer. The shared FROZEN golden-bytes fixture both packages assert against is the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte canonical stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00` and whose `XxHash128.HashToUInt128` digest is `0x9462A71A5DD13DCFA3B1D6D225FCBE70` (16-byte LE `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`), pinned REAL in `Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY`; a tier-2 cross-package byte-equality harness feeds this reference through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path and asserts the stream bytes AND the digest, with the topology-break case (changed adjacency → distinct `GeometryHash`) the discriminating law and the morph case (moved control points, same adjacency → equal `GeometryHash`) the convergence-neutral law.
- [SUBTREE_PRUNING] — `GraphNode.SubtreeHash` is the Merkle digest folding a node's own `(GeometryHash, PropertyHash, Ordinal)` LE preimage plus the `Kind.Key` UTF-16 bytes with its sorted child `SubtreeHash` digests through the `XxHash128.Append`/`GetCurrentHashAsUInt128` accumulator, mirroring the `Version/commits#COMMIT_DAG` `MerkleRange` anti-entropy pattern at the forest level — the preimage carries NO `GetHashCode` so two peers seal an identical digest deterministically, the forest fold compares one digest per node and descends only where the root digests diverge, so a byte-identical subtree under an unchanged root is pruned whole and the matched-forest cost is linear in the changed-node count, never the quadratic Zhang-Shasha tree-edit cost over the whole graph. `Seal` builds one fresh `XxHash128` accumulator per node, ingesting the preimage then each sorted child's sealed `SubtreeHash`, and recurses children exactly once so the projection is single-pass over the forest; `XxHash128.Clone()` is the available accumulator fork for a sibling-prefix rollup where two child lists share a common head digest, never a re-ingest of the prefix.
