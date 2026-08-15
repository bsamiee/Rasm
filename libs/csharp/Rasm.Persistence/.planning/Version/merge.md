# [PERSISTENCE_VERSION_MERGE]

`StructuralMerge` aligns re-ingested roots and classifies topology and content in one base-relative merge. `EntityEdit` emits base-addressed tombstones or closed RFC 6902 documents over exact `NodeWire` ProtoJSON, while insertions stay on the `EditOp.Insert` and `GraphDelta` mutation rail. `GraphNode` drives detection alone, and conflicts project stored `(Hlc, actor)` evidence into `ConflictReceipt`.

## [01]-[INDEX]

- [02]-[STRUCTURAL_DIFF]: Re-ingest alignment, Merkle-pruned matching, three-way merge, conflicts, and node edit egress.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` carries forest identity, content axes, sibling position, and Merkle subtree digest.
- Owner: `NodeRole`, `EditOp`, and `MergeConflict` close structural roles, operations, and conflicts.
- Owner: `EntityEdit` carries base-addressed tombstone and member-patch egress.
- Owner: `EntityEditWire` projects `EntityEdit` into the discriminated JSON contract.
- Owner: `PatchPolicy` admits the caller-supplied operation ceiling shared with the crossing.
- Owner: `TallyFact`, `MergeOutcome`, and `StructuralMerge` own merge evidence and the full merge fold.
- Cases: `EntityEdit` is `Tombstone | Members`; both arms carry the addressed current-node base.
- Cases: `Delete` maps to `Tombstone`; held-node material edits map to `Members`.
- Cases: `EditOp.Insert` remains on the graph mutation rail and does not fabricate a held-node EntityEdit.
- Entry: `Reconcile` aligns imported ids from GlobalId and unambiguous GlobalId-less type keys.
- Entry: `Forest` and `DiffContent` project object topology and non-object content onto separate detection axes.
- Entry: `ThreeWay` returns clean edits, typed conflicts, and tally evidence in one result.
- Entry: `Patch(script, base, target, policy, key)` returns `Fin<HashMap<NodeId, EntityEdit>>` on the element fault rail.
- Auto: Every base is `ContentAddress.Of(baseNode, base.Header.Tolerance)`.
- Auto: `Members` diffs exact before/after `NodeWire` ProtoJSON; object keys sort ordinally.
- Auto: Objects recurse; arrays, scalars, and changed roots replace whole; missing members add or remove.
- Auto: Changed prototype members replace their containing object, so emitted pointers remain safe.
- Auto: `Patch` collapses an over-ceiling member diff to one root replacement carrying exact successor ProtoJSON.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count folded into `MergeOutcome.Counts`, and each `MergeConflict` projects to `ConflictReceipt` with the held/incoming `(Hlc, actor)` from the changefeed; each projected `ConflictReceipt` fires the `rasm.persistence.merge.conflict` observe point (`Store/observability#HOOK_RAIL`) at the composition root.
- Packages: Rasm.Element owns graphs, node addressing, `NodeWire`, and its ProtoJSON projection.
- Packages: JsonPatch owns the RFC 6902 document; System.Text.Json owns exact wire values and outer edit JSON.
- Packages: System.IO.Hashing owns local Merkle accumulation; LanguageExt owns immutable carriers and `Fin`.
- Packages: Thinktecture owns closed unions; NodaTime owns merge evidence time.
- Growth: detection absorbs each new edit operation; edit egress stays the closed lifecycle union.
- Boundary: Re-ingest alignment precedes every durable-`NodeId` diff.
- Boundary: `Forest` reads `Relationship.Compose` containment and the complete representation map.
- Boundary: Non-object content nodes diff directly from the node map and never disappear behind the object forest.
- Boundary: `GraphNode` hashes drive detection only; patch paths target exact producer `NodeWire` ProtoJSON.
- Boundary: TypeScript applies the patch to retained ProtoJSON and decodes the successor through the existing node landing.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;
using NodaTime;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Rasm.Persistence.Element;                    // ElementJson.Options — the ONE codec STJ converter graph
using System.IO.Hashing;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Version;

// --- [TYPES] ---------------------------------------------------------------------------
// `NodeRole` closes the NEUTRAL structural-role vocabulary the forest retype axis routes on — derived from the seam's OWN
// neutral signal (ObjectKind occurrence/type + the containment-Whole role the Compose edges already encode),
// NEVER an IFC entity-class string. The seam Classification is a neutral (system, code) pair whose Code is a
// code-within-the-system (a Uniclass "Ss_25_10", an OmniClass code), NOT an IFC class — matching IFC-class
// literals against it is the phantom deleted form, the IfcClass roster living wholly in the Rasm.Bim projector.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeRole {
    public static readonly NodeRole Occurrence = new("occurrence"); // an ObjectKind.Occurrence with no containment-whole role
    public static readonly NodeRole Type = new("type");             // an ObjectKind.Type definition object
    public static readonly NodeRole Container = new("container");   // a node that is the Whole of a Compose.Contain edge (spatial structure)
    public static readonly NodeRole Annotation = new("annotation"); // an occurrence carrying no Body/Axis/FootPrint geometry

    // `Of` projects the neutral role: a Type object is Type; a containment Whole is a Container; an occurrence with
    // no geometry is an Annotation; everything else is a plain Occurrence — read off ObjectKind + the structural
    // role the graph encodes, never a foreign-schema string scan.
    public static NodeRole Of(ObjectKind kind, bool containerWhole, bool hasGeometry) =>
        kind == ObjectKind.Type ? Type
        : containerWhole ? Container
        : hasGeometry ? Occurrence
        : Annotation;
}

[SmartEnum]
public sealed partial class TallySlot {
    public static readonly TallySlot Edit = new();
    public static readonly TallySlot Conflict = new();
}

// --- [MODELS] --------------------------------------------------------------------------
// `GraphNode` carries one forest node. GeometryHash digests the Object's WHOLE Representations.ByIdentifier map (Body
// with the analytical Axis/Box/FootPrint), PropertyHash the seam ContentAddress over Node.ToCanonicalBytes. Matches
// tests the content signature, SubtreeHash is the Merkle prune key, and a content node (non-Object) carries its
// PropertyHash with GeometryHash == 0 and Parent == None — it lives on the content axis, never the Object forest.
public readonly record struct GraphNode(NodeId Key, NodeRole Role, Option<NodeId> Parent, int Ordinal, UInt128 GeometryHash, UInt128 PropertyHash, UInt128 SubtreeHash, Seq<NodeId> Children) {
    public UInt128 Signature => SubtreeHash;
    public bool Matches(GraphNode other) => GeometryHash == other.GeometryHash && PropertyHash == other.PropertyHash;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EditOp {
    private EditOp() { }
    public sealed record Match(NodeId Key) : EditOp;
    public sealed record Insert(GraphNode Node) : EditOp;
    public sealed record Delete(NodeId Key) : EditOp;
    public sealed record Update(NodeId Key, UInt128 FromProperty, UInt128 ToProperty, UInt128 FromGeometry, UInt128 ToGeometry) : EditOp;
    public sealed record Move(NodeId Key, Option<NodeId> FromParent, Option<NodeId> ToParent) : EditOp;
    public sealed record Reorder(NodeId Key, int FromOrdinal, int ToOrdinal) : EditOp;
    public sealed record Retype(NodeId Key, NodeRole FromRole, NodeRole ToRole) : EditOp;

    public NodeId Target => this.Map(match: m => m.Key, insert: i => i.Node.Key, delete: d => d.Key, update: u => u.Key, move: v => v.Key, reorder: r => r.Key, retype: t => t.Key);
    public string KindName => this.Map(match: static _ => "match", insert: static _ => "insert", delete: static _ => "delete", update: static _ => "update", move: static _ => "move", reorder: static _ => "reorder", retype: static _ => "retype");
    public string Axis => this.Map(match: static _ => "", insert: static _ => "insert", delete: static _ => "delete", update: static _ => "content", move: static _ => "parent", reorder: static _ => "ordinal", retype: static _ => "role");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record MergeConflict {
    private MergeConflict() { }
    public sealed record ParallelEdit(NodeId Key, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record DeleteUpdate(NodeId Key, bool DeletedByOurs, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record MoveMove(NodeId Key, Option<NodeId> OurParent, Option<NodeId> TheirParent, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ReorderReorder(NodeId Key, int OurOrdinal, int TheirOrdinal, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TypeChange(NodeId Key, NodeRole OurRole, NodeRole TheirRole, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record TopologyBreak(NodeId Key, UInt128 OurGeometry, UInt128 TheirGeometry, Hlc OurCell, string OurActor, Hlc TheirCell, string TheirActor) : MergeConflict;
    public sealed record ContainmentCycle(NodeId Key, NodeId Ancestor, bool ByOurs, Hlc OurCell, string OurActor) : MergeConflict;

    public NodeId Subject => this.Map(parallelEdit: p => p.Key, deleteUpdate: d => d.Key, moveMove: m => m.Key, reorderReorder: r => r.Key, typeChange: t => t.Key, topologyBreak: b => b.Key, containmentCycle: y => y.Key);
    public string KindName => this.Map(parallelEdit: static _ => "parallelEdit", deleteUpdate: static _ => "deleteUpdate", moveMove: static _ => "moveMove", reorderReorder: static _ => "reorderReorder", typeChange: static _ => "typeChange", topologyBreak: static _ => "topologyBreak", containmentCycle: static _ => "containmentCycle");

    // `Family` names the merge-lane a conflict adjudicates on — only a TopologyBreak rides the geometry axis, every
    // property/structural class the CRDT axis — derived once so the receipt projection reads one accessor, never a
    // seven-arm Map that repeats `ColumnFamily.Crdt` six times.
    public ColumnFamily Family => this is TopologyBreak ? ColumnFamily.Geometry : ColumnFamily.Crdt;

    // `ConflictAxis` names the axis the merged script excludes — a per-(key, axis) mask, so one content conflict never
    // suppresses a clean move/reorder/retype on the same node; DeleteUpdate and ContainmentCycle poison the
    // WHOLE key (None = every axis) because no orthogonal edit survives a contested existence or a cycle.
    public Option<string> ConflictAxis => this.Map(
        parallelEdit: static _ => Some("content"), deleteUpdate: static _ => Option<string>.None,
        moveMove: static _ => Some("parent"), reorderReorder: static _ => Some("ordinal"),
        typeChange: static _ => Some("role"), topologyBreak: static _ => Some("content"),
        containmentCycle: static _ => Option<string>.None);

    // `Evidence` carries the two-sided (Hlc, actor) stamp — derived through the generated Map so the seven
    // near-identical Receipt arms collapse to ONE Project expression; the single-author ContainmentCycle (the cycle
    // is detected on one side) reuses its own cell/actor for both held and incoming, the only case where the two
    // stamps coincide. A TheirCell/TheirActor pair is load-bearing on the six two-sided cases and absent on the
    // cycle case, so the union owns the projection, not seven copy-pasted Receipt calls.
    public (Hlc Held, string HeldActor, Hlc Incoming, string IncomingActor) Evidence => this.Map(
        parallelEdit: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        deleteUpdate: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        moveMove: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        reorderReorder: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        typeChange: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        topologyBreak: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        containmentCycle: static c => (c.OurCell, c.OurActor, c.OurCell, c.OurActor));
}

// Both egress arms carry authoritative held-node OCC. Members carries a closed RFC 6902 document whose paths target
// exact NodeWire ProtoJSON; insertion stays on EditOp.Insert and the GraphDelta rail because no held node can supply Base.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EntityEdit {
    private EntityEdit() { }
    public sealed record Tombstone(NodeId Key, ContentAddress Base) : EntityEdit;
    public sealed record Members(NodeId Key, ContentAddress Base, JsonPatchDocument Patch) : EntityEdit;
}

public sealed record PatchPolicy {
    private PatchPolicy(int operationCeiling) => OperationCeiling = operationCeiling;
    public int OperationCeiling { get; }

    public static Fin<PatchPolicy> Of(int operationCeiling, Op key) => operationCeiling > 0
        ? Fin.Succ(new PatchPolicy(operationCeiling))
        : ElementFault.ValueRejected(key, $"<entity-edit-operation-ceiling:{operationCeiling}>");
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "op")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Add), "add")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Remove), "remove")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Replace), "replace")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Move), "move")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Copy), "copy")]
[JsonDerivedType(typeof(JsonPatchOperationWire.Test), "test")]
public abstract record JsonPatchOperationWire {
    static readonly FrozenSet<string> UnsafeMembers = new[] { "__proto__", "prototype", "constructor" }
        .ToFrozenSet(StringComparer.Ordinal);

    private JsonPatchOperationWire() { }
    public sealed record Add(string Path, JsonElement Value) : JsonPatchOperationWire;
    public sealed record Remove(string Path) : JsonPatchOperationWire;
    public sealed record Replace(string Path, JsonElement Value) : JsonPatchOperationWire;
    public sealed record Move(string From, string Path) : JsonPatchOperationWire;
    public sealed record Copy(string From, string Path) : JsonPatchOperationWire;
    public sealed record Test(string Path, JsonElement Value) : JsonPatchOperationWire;

    public static Fin<JsonPatchOperationWire> Of(Operation operation, Op key) =>
        key.Catch(() => Project(operation, key)).MapFail(error => error.IsExceptional
            ? (Error)ElementFault.ValueRejected(key, $"<entity-edit-operation-value:{operation.op}>")
            : error);

    static Fin<JsonPatchOperationWire> Project(Operation operation, Op key) => operation.OperationType switch {
        OperationType.Add => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Add(path, Value(operation.value))),
        OperationType.Remove => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Remove(path)),
        OperationType.Replace => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Replace(path, Value(operation.value))),
        OperationType.Move => Pointer(operation.from, key).Bind(from => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Move(from, path))),
        OperationType.Copy => Pointer(operation.from, key).Bind(from => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Copy(from, path))),
        OperationType.Test => Pointer(operation.path, key)
            .Map(path => (JsonPatchOperationWire)new Test(path, Value(operation.value))),
        _ => ElementFault.ValueRejected(key, $"<entity-edit-operation:{operation.op}>")
    };

    static JsonElement Value(object? value) => JsonSerializer.SerializeToElement(value, ElementJson.Options);

    internal static bool IsUnsafe(string member) => UnsafeMembers.Contains(member);

    static Fin<string> Pointer(string? value, Op key) =>
        value is null || value.Length > 0 && value[0] != '/'
            ? ElementFault.ValueRejected(key, $"<entity-edit-pointer:{value}>")
            : toSeq(value.Split('/').Skip(1)).Exists(segment =>
                !ValidEscapes(segment) || UnsafeMembers.Contains(segment.Replace("~1", "/").Replace("~0", "~")))
                ? ElementFault.ValueRejected(key, $"<entity-edit-pointer:{value}>")
                : Fin.Succ(value);

    static bool ValidEscapes(string token) {
        for (int i = 0; i < token.Length; i++) {
            if (token[i] != '~') { continue; }
            if (++i == token.Length || token[i] is not ('0' or '1')) { return false; }
        }
        return true;
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(EntityEditWire.Tombstone), "tombstone")]
[JsonDerivedType(typeof(EntityEditWire.Members), "members")]
public abstract record EntityEditWire {
    private EntityEditWire() { }
    public sealed record Tombstone(string Key, string Base) : EntityEditWire;
    public sealed record Members(string Key, string Base, JsonPatchOperationWire[] Patch) : EntityEditWire;

    public static Fin<EntityEditWire> Of(EntityEdit edit, PatchPolicy policy, Op key) => edit.Switch<Fin<EntityEditWire>>(
        tombstone: static row => Fin.Succ((EntityEditWire)new Tombstone(row.Key.Value, row.Base.ToValue())),
        members: row => row.Patch.Operations.Count > policy.OperationCeiling
            ? ElementFault.ValueRejected(key, $"<entity-edit-operation-overrun:{row.Patch.Operations.Count}>")
            : toSeq(row.Patch.Operations)
            .Traverse(operation => JsonPatchOperationWire.Of(operation, key).ToValidation()).As().ToFin()
            .Map(operations => (EntityEditWire)new Members(row.Key.Value, row.Base.ToValue(), operations.ToArray())));

    public static Fin<byte[]> Encode(EntityEdit edit, PatchPolicy policy, Op key) =>
        Of(edit, policy, key).Map(row => JsonSerializer.SerializeToUtf8Bytes(row, ElementJson.Options));
}

public readonly record struct TallyFact(TallySlot Slot, string Kind, int Count);
public readonly record struct MergeOutcome(Seq<EditOp> Merged, Seq<MergeConflict> Conflicts, Seq<TallyFact> Counts) {
    public bool Clean => Conflicts.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StructuralMerge {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.diff.structural"), StoreSlot.Create("store.merge.threeway"));

    public static Seq<GraphNode> Forest(ElementGraph graph) {
        // Forest topology rides the seam Relationship.Compose containment edges (Whole→Part), siblings ordered
        // by their index among one Whole's Compose edges — never a phantom unified edge kind. ONLY Object nodes
        // enter the forest; the non-Object content nodes ride the content axis (ContentNodes below).
        Seq<Relationship.Compose> contain = toSeq(graph.Edges.Choose(static e => Optional(e as Relationship.Compose)));
        HashMap<NodeId, NodeId> parentByKey = toHashMap(contain.Map(static c => (c.Part, c.Whole)));
        HashMap<NodeId, Seq<NodeId>> childrenByParent = toHashMap(contain.GroupBy(static c => c.Whole).Select(static g => (g.Key, toSeq(g.Select(static c => c.Part)))));
        HashMap<NodeId, int> ordinalByKey = toHashMap(contain.GroupBy(static c => c.Whole).SelectMany(static g => g.Select(static (c, ordinal) => (c.Part, ordinal))));
        HashSet<NodeId> containerWholes = toHashSet(contain.Filter(static c => c.SubKind == ComposeKind.Contain).Map(static c => c.Whole));
        HashMap<NodeId, GraphNode> nodes = toHashMap(toSeq(graph.Nodes.Values).Choose(static n => Optional(n as Node.Object)).Map(o => (o.Id, new GraphNode(
            o.Id,
            NodeRole.Of(o.Kind, containerWholes.Contains(o.Id), !o.Representations.ByIdentifier.IsEmpty),
            parentByKey.Find(o.Id), ordinalByKey.Find(o.Id).IfNone(0),
            GeometryDigest(o.Representations), ContentAddress.Of(o.ToCanonicalBytes(graph.Header.Tolerance).Span).Value, UInt128.Zero,
            childrenByParent.Find(o.Id).IfNone(Seq<NodeId>())))));
        return nodes.Values.Filter(static node => node.Parent.IsNone).Bind(root => Seal(root, nodes));
    }

    // Content axis: every non-Object node the Object-forest never topologizes — the ContentRole switch names
    // that roster and breaks on the next case — projects as Parent-less GraphNodes carrying only the PropertyHash content
    // signature (GeometryHash 0). The three-way merge diffs these DIRECTLY off the node map so a single-side content
    // edit materializes as a Members patch and a both-side content edit is a ParallelEdit — an Object-only forest would
    // silently drop a changed property set / material, the deleted form.
    public static HashMap<NodeId, GraphNode> ContentNodes(ElementGraph graph) =>
        toHashMap(toSeq(graph.Nodes.Values).Filter(static n => n is not Node.Object).Map(n => (n.Id, new GraphNode(
            n.Id, ContentRole(n), Option<NodeId>.None, 0, UInt128.Zero,
            ContentAddress.Of(n.ToCanonicalBytes(graph.Header.Tolerance).Span).Value, UInt128.Zero, Seq<NodeId>()))));

    public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to) {
        HashMap<NodeId, GraphNode> fromByKey = toHashMap(from.Map(static n => (n.Key, n)));
        HashMap<NodeId, GraphNode> toByKey = toHashMap(to.Map(static n => (n.Key, n)));
        Seq<GraphNode> roots = to.Filter(n => n.Parent.Map(p => !toByKey.ContainsKey(p)).IfNone(true));
        return Walk(roots.IsEmpty ? to : roots, fromByKey, toByKey)
             + from.Filter(n => !toByKey.ContainsKey(n.Key)).Map(static n => (EditOp)new EditOp.Delete(n.Key));
    }

    // `DiffContent` compares the content axis map-to-map on the durable NodeId (NOT the Object forest), emitting
    // Update for a signature change, Insert/Delete for an unmatched key. The content node never moves/reorders/retypes
    // (it has no containment role), so the only axes are content and insert/delete.
    public static Seq<EditOp> DiffContent(HashMap<NodeId, GraphNode> from, HashMap<NodeId, GraphNode> to) =>
        toSeq(to.Map((key, node) => from.Find(key).Match(
            Some: prior => prior.PropertyHash == node.PropertyHash ? (EditOp)new EditOp.Match(key) : new EditOp.Update(key, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash),
            None: () => new EditOp.Insert(node))).Values)
        + toSeq(from.Filter((key, _) => !to.ContainsKey(key)).Map(static (key, _) => (EditOp)new EditOp.Delete(key)).Values);

    public static MergeOutcome ThreeWay(ElementGraph @base, ElementGraph ours, ElementGraph theirs, Func<NodeId, Option<OpLogEntry>> stampOurs, Func<NodeId, Option<OpLogEntry>> stampTheirs) {
        Seq<GraphNode> baseForest = Forest(@base), ourForest = Forest(ours), theirForest = Forest(theirs);
        HashMap<NodeId, GraphNode> baseContent = ContentNodes(@base), ourContent = ContentNodes(ours), theirContent = ContentNodes(theirs);
        // Two axes per side key on the durable NodeId the node map already holds. The object-forest and content-node
        // scripts concatenate and group by (key, axis); no member-path projection participates in edit egress.
        HashMap<NodeId, HashMap<string, EditOp>> ourEdits = ByKeyAxis(Diff(baseForest, ourForest) + DiffContent(baseContent, ourContent));
        HashMap<NodeId, HashMap<string, EditOp>> theirEdits = ByKeyAxis(Diff(baseForest, theirForest) + DiffContent(baseContent, theirContent));
        HashMap<NodeId, GraphNode> oursByKey = toHashMap(ourForest.Map(static n => (n.Key, n)));
        HashMap<NodeId, GraphNode> theirsByKey = toHashMap(theirForest.Map(static n => (n.Key, n)));
        Seq<MergeConflict> conflicts = toSeq(ourEdits.Keys.Where(theirEdits.ContainsKey)
            .Bind(key => Conflicts(key, ourEdits.Find(key).IfNone(HashMap<string, EditOp>()), theirEdits.Find(key).IfNone(HashMap<string, EditOp>()), stampOurs(key), stampTheirs(key)))
            .Append(Cycles(ourEdits, oursByKey, ByOurs: true, stampOurs))
            .Append(Cycles(theirEdits, theirsByKey, ByOurs: false, stampTheirs)));
        // Exclusion is PER (key, axis): a node with one conflicting axis retains every clean orthogonal edit —
        // only DeleteUpdate/ContainmentCycle (ConflictAxis None) poison the whole key. Ours' clean axes first,
        // then theirs' axes ours did not touch (last-write-wins per axis).
        HashSet<NodeId> poisoned = conflicts.Filter(static c => c.ConflictAxis.IsNone).Map(static c => c.Subject).ToHashSet();
        HashSet<(NodeId Key, string Axis)> conflictedAxes = conflicts.Bind(c => c.ConflictAxis.Map(axis => (c.Subject, axis)).ToSeq()).ToHashSet();
        bool Excluded(NodeId key, string axis) => poisoned.Contains(key) || conflictedAxes.Contains((key, axis));
        Seq<EditOp> merged = toSeq(ourEdits.Map((key, axes) => toSeq(axes.Filter((axis, _) => !Excluded(key, axis)).Values)).Values.Bind(static ops => ops)
            .Append(theirEdits.Map((key, axes) => toSeq(axes.Filter((axis, _) => !Excluded(key, axis) && !ourEdits.Find(key).Map(a => a.ContainsKey(axis)).IfNone(false)).Values)).Values.Bind(static ops => ops)));
        return new MergeOutcome(merged, conflicts, Tally(merged, conflicts));
    }

    // Project held-node groups only. Insert remains on EditOp.Insert/GraphDelta because no current node can supply Base.
    // Members diffs exact NodeWire ProtoJSON; TypeScript patches that same representation before node admission.
    public static Fin<HashMap<NodeId, EntityEdit>> Patch(
        Seq<EditOp> script, ElementGraph @base, ElementGraph target, PatchPolicy policy, Op key) =>
        toSeq(script.GroupBy(static op => op.Target)).Fold(
            Fin.Succ(HashMap<NodeId, EntityEdit>()),
            (state, group) => state.Bind(edits => Edit(group.Key, toSeq(group), @base, target, policy, key)
                .Map(edit => edit.Map(row => edits.AddOrUpdate(group.Key, row)).IfNone(edits))));

    static Fin<Option<EntityEdit>> Edit(
        NodeId subject, Seq<EditOp> ops, ElementGraph @base, ElementGraph target, PatchPolicy policy, Op key) =>
        (ops.Exists(static op => op is EditOp.Delete), ops.Exists(static op => op is EditOp.Insert)) switch {
            (true, true) => ElementFault.DeltaConflict(key, $"<merge-edit-existence-conflict:{subject.Value}>"),
            (true, false) when target.Find(subject).IsSome =>
                ElementFault.DeltaConflict(key, $"<merge-tombstone-target-present:{subject.Value}>"),
            (true, false) => @base.Find(subject)
                .ToFin(ElementFault.NodeAbsent(key, $"<merge-tombstone-base-absent:{subject.Value}>"))
                .Map(node => Some<EntityEdit>(new EntityEdit.Tombstone(
                    subject, ContentAddress.Of(node, @base.Header.Tolerance)))),
            (false, true) when @base.Find(subject).IsSome =>
                ElementFault.DeltaConflict(key, $"<merge-insert-base-present:{subject.Value}>"),
            (false, true) when ops.Exists(static op => op is not EditOp.Insert and not EditOp.Match) =>
                ElementFault.DeltaConflict(key, $"<merge-insert-mixed-edit:{subject.Value}>"),
            (false, true) => target.Find(subject)
                .ToFin(ElementFault.NodeAbsent(key, $"<merge-insert-target-absent:{subject.Value}>"))
                .Map(static _ => Option<EntityEdit>.None),
            _ => @base.Find(subject)
                .ToFin(ElementFault.NodeAbsent(key, $"<merge-members-base-absent:{subject.Value}>"))
                .Bind(before => target.Find(subject)
                    .ToFin(ElementFault.NodeAbsent(key, $"<merge-members-target-absent:{subject.Value}>"))
                    .Map(after => PatchDocument(
                        NodeJson(before, @base.Header.Tolerance), NodeJson(after, target.Header.Tolerance), policy) switch {
                        { Operations.Count: 0 } => Option<EntityEdit>.None,
                        var patch => Some<EntityEdit>(new EntityEdit.Members(
                            subject, ContentAddress.Of(before, @base.Header.Tolerance), patch)),
                    })),
        };

    static JsonElement NodeJson(Node node, double tolerance) {
        using JsonDocument document = JsonDocument.Parse(JsonFormatter.Default.Format(ElementWire.Encode(node, tolerance)));
        return document.RootElement.Clone();
    }

    static JsonPatchDocument PatchDocument(JsonElement before, JsonElement after, PatchPolicy policy) {
        Seq<Operation> operations = Diff(before, after, "");
        Seq<Operation> admitted = operations.Count <= policy.OperationCeiling
            ? operations
            : Seq(new Operation("replace", string.Empty, string.Empty, after.Clone()));
        return new JsonPatchDocument(admitted.ToList(), ElementJson.Options);
    }

    static Seq<Operation> Diff(JsonElement before, JsonElement after, string path) {
        if (JsonElement.DeepEquals(before, after)) { return Seq<Operation>(); }
        if (before.ValueKind is not JsonValueKind.Object || after.ValueKind is not JsonValueKind.Object) {
            return Seq(new Operation("replace", path, string.Empty, after.Clone()));
        }

        HashMap<string, JsonElement> left = toHashMap(before.EnumerateObject()
            .Select(static property => (property.Name, property.Value)));
        HashMap<string, JsonElement> right = toHashMap(after.EnumerateObject()
            .Select(static property => (property.Name, property.Value)));
        Seq<string> names = toSeq(left.Keys.Concat(right.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        if (names.Exists(name => JsonPatchOperationWire.IsUnsafe(name) && Changed(left.Find(name), right.Find(name)))) {
            return Seq(new Operation("replace", path, string.Empty, after.Clone()));
        }
        return names.Bind(name =>
            left.Find(name).Match(
                Some: prior => right.Find(name).Match(
                    Some: next => Diff(prior, next, Child(path, name)),
                    None: () => Seq(new Operation("remove", Child(path, name), string.Empty, null!))),
                None: () => right.Find(name).Match(
                    Some: added => Seq(new Operation("add", Child(path, name), string.Empty, added.Clone())),
                    None: Seq<Operation>)));
    }

    static bool Changed(Option<JsonElement> before, Option<JsonElement> after) => before.Match(
        Some: prior => after.Match(Some: next => !JsonElement.DeepEquals(prior, next), None: static () => true),
        None: () => after.IsSome);

    static string Child(string path, string member) => $"{path}/{member.Replace("~", "~0").Replace("/", "~1")}";

    // ONE projection over the derived Family + Evidence accessors — the seven near-identical Receipt arms are the
    // DERIVED_LOGIC collapse: the lane is `conflict.Family`, the two-sided stamps `conflict.Evidence`, so a new
    // conflict class adds one union case (and its Family/Evidence arm) without a new Project arm.
    public static ConflictReceipt Project(MergeConflict conflict, ModelId model, Guid correlation, Instant at) {
        (Hlc held, string heldActor, Hlc incoming, string incomingActor) = conflict.Evidence;
        return Receipt(conflict.Subject, model, conflict.Family, held, heldActor, incoming, incomingActor, correlation, at);
    }

    // Re-ingest correlation [H6]+[V8b]: the seam mints a FRESH neutral rooted NodeId on every Project, so a
    // re-imported model shares NO NodeId with the persisted graph and a raw NodeId-keyed diff reads the whole
    // import as delete-all + insert-all. Reconcile aligns the re-ingested graph to the DURABLE persisted
    // identities BEFORE the forest/content diff on TWO key rows: a rooted node on the stable 1:1 IFC GlobalId
    // (Node.Object.ExternalId — NEVER the minted NodeId) and a GlobalId-less Type on the TypeKey natural key;
    // it builds the ingest->durable NodeId remap and rewrites the ingested nodes + every edge endpoint onto the
    // durable ids (a correlation key absent from the persisted graph keeps its fresh id as a genuine insert; a
    // persisted key absent from the ingest surfaces as a delete in the diff). So the durable NodeId survives
    // re-import (Graph/element#NODE_MODEL) and the NodeId-keyed Diff/ThreeWay below operate on aligned graphs;
    // Reconcile applies that same remap to a freshly-PROJECTED GraphDelta before it commits so the durable stream
    // never forks, and the content signature drives change DETECTION, never cross-ingest identity.
    public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested) {
        HashMap<string, NodeId> durable = Correlation(persisted);
        HashMap<NodeId, NodeId> remap = toHashMap(toSeq(Correlation(ingested).AsIterable())
            .Choose(pair => durable.Find(pair.Key).Map(id => (Ingest: pair.Value, Durable: id)))
            .Filter(static move => move.Ingest != move.Durable));
        return remap.IsEmpty ? (ingested, remap) : (Reindex(ingested, remap), remap);
    }

    // One correlation map per graph: every rooted GlobalId row plus every UNAMBIGUOUS Type natural-key row — a
    // natural key two Types share on one side drops from correlation (a 1:1 aligner never guesses; the GlobalId
    // row wins a collision via TryAdd).
    static HashMap<string, NodeId> Correlation(ElementGraph graph) {
        HashMap<string, NodeId> rooted = toHashMap(toSeq(graph.Nodes.Values).Choose(ExternalKey));
        return toSeq(graph.Nodes.Values.Select(TypeKey).Somes().GroupBy(static pair => pair.External).Where(static g => g.Count() == 1))
            .Fold(rooted, static (acc, g) => acc.TryAdd(g.Key, g.First().Id));
    }

    // `ExternalKey` reads the 1:1 GlobalId correlation key off a ROOTED node — Some only when the node carries an
    // ExternalId (a rooted IFC node); a from-scratch node carries None and stays NodeId-identified (it lives in one
    // authoring lineage, matched by the durable NodeId, never re-minted by a foreign ingest).
    static Option<(string External, NodeId Id)> ExternalKey(Node node) =>
        node is Node.Object { ExternalId: var external } obj ? external.Map(ext => (External: ext, Id: obj.Id)) : None;

    // [V8b] The Type correlation key: a GlobalId-less Type definition object correlates on the classification-
    // EXCLUDED `type:Name\u001fTag` natural key (stable across a re-key, so a re-keyed Type diffs as RENAME,
    // never delete+insert) — the interim seed the kernel V8a Type natural-key seed replaces on landing; the
    // `type:` prefix keeps the row disjoint from the GlobalId key space, and Classification NEVER enters the key.
    static Option<(string External, NodeId Id)> TypeKey(Node node) =>
        node is Node.Object { ExternalId.IsNone: true } obj && obj.Kind == ObjectKind.Type
            ? Some((External: $"type:{obj.Name}\u001f{obj.Tag}", Id: obj.Id))
            : None;

    // Rewrite the ingested graph onto the durable ids: re-stamp every node id and re-endpoint every edge through the
    // ingest->durable map (an unmapped id passes through unchanged), then re-freeze the snapshot. Node identity
    // rewrite and edge-endpoint rewrite are the seam-owned `Node.Relabel`/`Relationship.Remap` operations (the
    // Graph/element#NODE_MODEL + Relations/relation#EDGE_ALGEBRA owners) — the seam `Node`/`Relationship` are
    // class-root unions and a class-root union case has NO `with`, so the id/endpoint rewrite is the union's own
    // total-Map reconstruction, NOT a per-case `with` re-spelled in this consumer. Reconcile composes them, never
    // re-deriving them.
    static ElementGraph Reindex(ElementGraph graph, HashMap<NodeId, NodeId> remap) {
        NodeId Resolve(NodeId id) => remap.Find(id).IfNone(id);
        FrozenDictionary<NodeId, Node> nodes = graph.Nodes.Values.Select(node => node.Relabel(Resolve(node.Id))).ToFrozenDictionary(static node => node.Id);
        ImmutableArray<Relationship> edges = [.. graph.Edges.Select(edge => edge.Remap(Resolve))];
        return ElementGraph.Of(graph.Header, nodes, edges);
    }

    // `GeometryDigest` folds the Object's geometry signature over the FULL Representations.ByIdentifier keyed map
    // (M2: every RepresentationIdentifier the kernel content-keyed — the heavy Body mesh AND the analytical
    // Axis/Box/FootPrint) into one digest over a deterministic identifier-sorted preimage, so a structural-line (Axis)
    // or space-boundary (FootPrint) geometry edit surfaces as a geometry divergence the same as a Body edit. Reading
    // only Body silently misses an analytical change (the deleted thin slice). The digests are kernel `GeometryHash`
    // values minted over the kernel-FROZEN `EncodeForm` byte layouts (`Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE`
    // — IEEE-754-LE, `-0.0`→`+0.0`; this page is that seam's RE-TARGETED consumer), READ not re-minted, and the
    // preimage interleaves each identifier WITH its digest — each identifier keys the representation's EncodeForm
    // lane, so the fold pairs (form lane, digest) and a bare digest never crosses a form boundary. The empty map
    // digests to 0.
    static UInt128 GeometryDigest(RepresentationContentHash representations) {
        if (representations.ByIdentifier.IsEmpty) { return UInt128.Zero; }
        using XxHash128 acc = new();
        Span<byte> cell = stackalloc byte[16];
        foreach ((string identifier, UInt128 hash) in representations.ByIdentifier.OrderBy(static p => p.Key, StringComparer.Ordinal)) {
            acc.Append(MemoryMarshal.AsBytes(identifier.AsSpan()));
            BinaryPrimitives.WriteUInt128LittleEndian(cell, hash);
            acc.Append(cell);
        }
        return acc.GetCurrentHashAsUInt128();
    }

    // Neutral content-node role, one arm per non-Object case and NO catch-all: a `_` arm files the next case the
    // seam mints under whichever role it happens to sit beside, so a ninth node kind lands misclassified and the
    // diff never says so. `Node.Object` reaches this fold from nowhere — `ContentNodes` filters it out and the
    // forest arm owns it — yet its arm is spelled, because an unlisted case in a switch expression is a
    // `SwitchExpressionException` out of a pure projection rather than a compiler complaint.
    // `Observation` classifies Occurrence beside Material and Coverage: a deployed sensor's measured run is
    // resident evidence a commissioning comparison reads, so a `Retype` off that row reports a real instrument
    // remount, where an Annotation reading buries it among presentation facets.
    static NodeRole ContentRole(Node node) => node switch {
        Node.Material => NodeRole.Occurrence,
        Node.Coverage => NodeRole.Occurrence,
        Node.Observation => NodeRole.Occurrence,
        Node.Assessment => NodeRole.Annotation,
        Node.Appearance => NodeRole.Annotation,
        Node.PropertySet => NodeRole.Annotation,
        Node.QuantitySet => NodeRole.Annotation,
        Node.Object o => NodeRole.Of(o.Kind, containerWhole: false, hasGeometry: !o.Representations.ByIdentifier.IsEmpty),
    };

    static Seq<GraphNode> Seal(GraphNode node, HashMap<NodeId, GraphNode> nodes) {
        Seq<Seq<GraphNode>> children = node.Children.Choose(nodes.Find).Map(child => Seal(child, nodes));
        using XxHash128 acc = new();
        Span<byte> frame = stackalloc byte[36];
        BinaryPrimitives.WriteUInt128LittleEndian(frame[..16], node.GeometryHash);
        BinaryPrimitives.WriteUInt128LittleEndian(frame[16..32], node.PropertyHash);
        BinaryPrimitives.WriteInt32LittleEndian(frame[32..36], node.Ordinal);
        acc.Append(frame);
        acc.Append(MemoryMarshal.AsBytes(node.Role.Key.AsSpan()));
        Span<byte> rollup = stackalloc byte[16];
        foreach (Seq<GraphNode> subtree in children) { BinaryPrimitives.WriteUInt128LittleEndian(rollup, subtree.Head.Map(static r => r.SubtreeHash).IfNone(UInt128.Zero)); acc.Append(rollup); }
        using XxHash128 sealedState = acc.Clone();
        return Seq(node with { SubtreeHash = sealedState.GetCurrentHashAsUInt128() }) + children.Bind(static subtree => subtree);
    }

    static Seq<EditOp> Walk(Seq<GraphNode> frontier, HashMap<NodeId, GraphNode> fromByKey, HashMap<NodeId, GraphNode> toByKey) =>
        toSeq(frontier.OrderBy(static n => n.Ordinal)).Bind(node => fromByKey.Find(node.Key).Match(
            Some: prior => prior.SubtreeHash == node.SubtreeHash ? Seq<EditOp>(new EditOp.Match(node.Key)) : Edit(prior, node) + Descend(node, fromByKey, toByKey),
            None: () => Seq<EditOp>(new EditOp.Insert(node)) + Descend(node, fromByKey, toByKey)));

    static Seq<EditOp> Descend(GraphNode node, HashMap<NodeId, GraphNode> fromByKey, HashMap<NodeId, GraphNode> toByKey) => Walk(node.Children.Choose(toByKey.Find), fromByKey, toByKey);

    static Seq<EditOp> Edit(GraphNode prior, GraphNode node) =>
        (prior.Role.Key != node.Role.Key ? Seq<EditOp>(new EditOp.Retype(node.Key, prior.Role, node.Role)) : Seq<EditOp>())
        + (prior.Parent != node.Parent ? Seq<EditOp>(new EditOp.Move(node.Key, prior.Parent, node.Parent))
            : prior.Ordinal != node.Ordinal ? Seq<EditOp>(new EditOp.Reorder(node.Key, prior.Ordinal, node.Ordinal)) : Seq<EditOp>())
        + (prior.Matches(node) ? Seq<EditOp>() : Seq<EditOp>(new EditOp.Update(node.Key, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash)));

    static Seq<MergeConflict> Conflicts(NodeId key, HashMap<string, EditOp> ours, HashMap<string, EditOp> theirs, Option<OpLogEntry> o, Option<OpLogEntry> t) {
        (Hlc oc, string oa) = Stamp(o);
        (Hlc tc, string ta) = Stamp(t);
        return ours.ContainsKey("delete") && theirs.Keys.Exists(static a => a != "delete")
            ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: true, oc, oa, tc, ta))
            : theirs.ContainsKey("delete") && ours.Keys.Exists(static a => a != "delete")
                ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: false, oc, oa, tc, ta))
                : toSeq(ours.Keys.Filter(theirs.ContainsKey).Choose(axis => Diverge(key, ours[axis], theirs[axis], oc, oa, tc, ta)));
    }

    static Option<MergeConflict> Diverge(NodeId key, EditOp ours, EditOp theirs, Hlc oc, string oa, Hlc tc, string ta) => (ours, theirs) switch {
        (EditOp.Retype r1, EditOp.Retype r2) when r1.ToRole.Key != r2.ToRole.Key => new MergeConflict.TypeChange(key, r1.ToRole, r2.ToRole, oc, oa, tc, ta),
        (EditOp.Move m1, EditOp.Move m2) when m1.ToParent != m2.ToParent => new MergeConflict.MoveMove(key, m1.ToParent, m2.ToParent, oc, oa, tc, ta),
        (EditOp.Reorder r1, EditOp.Reorder r2) when r1.ToOrdinal != r2.ToOrdinal => new MergeConflict.ReorderReorder(key, r1.ToOrdinal, r2.ToOrdinal, oc, oa, tc, ta),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToGeometry != u2.ToGeometry => new MergeConflict.TopologyBreak(key, u1.ToGeometry, u2.ToGeometry, oc, oa, tc, ta),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToProperty != u2.ToProperty => new MergeConflict.ParallelEdit(key, oc, oa, tc, ta),
        _ => Option<MergeConflict>.None,
    };

    static Seq<MergeConflict> Cycles(HashMap<NodeId, HashMap<string, EditOp>> edits, HashMap<NodeId, GraphNode> byKey, bool ByOurs, Func<NodeId, Option<OpLogEntry>> stamp) =>
        toSeq(edits.Keys.Choose(key => ParentOf(key, edits, byKey).Filter(parent => IsDescendant(parent, key, byKey, HashSet<NodeId>()))
            .Map(parent => { (Hlc cell, string actor) = Stamp(stamp(key)); return (MergeConflict)new MergeConflict.ContainmentCycle(key, parent, ByOurs, cell, actor); })));

    static ConflictReceipt Receipt(NodeId key, ModelId model, ColumnFamily family, Hlc held, string heldActor, Hlc incoming, string incomingActor, Guid correlation, Instant at) =>
        new(model, key.Value, family, held, heldActor, incoming, incomingActor, correlation, at);

    static (Hlc Cell, string Actor) Stamp(Option<OpLogEntry> entry) => entry.Match(Some: e => (new Hlc(e.Physical, e.Logical), e.Actor), None: () => (Hlc.Zero, ""));

    static HashMap<NodeId, HashMap<string, EditOp>> ByKeyAxis(Seq<EditOp> script) =>
        toHashMap(toSeq(script.Filter(static op => op is not EditOp.Match).GroupBy(static op => op.Target)).Map(static group => (group.Key, toHashMap(toSeq(group.GroupBy(static op => op.Axis)).Map(static axis => (axis.Key, axis.Last()))))));

    static Option<NodeId> ParentOf(NodeId key, HashMap<NodeId, HashMap<string, EditOp>> edits, HashMap<NodeId, GraphNode> byKey) =>
        edits.Find(key).Bind(static axes => axes.Find("parent")).Bind(static op => op is EditOp.Move m ? m.ToParent : Option<NodeId>.None) | byKey.Find(key).Bind(static node => node.Parent);

    static bool IsDescendant(NodeId candidate, NodeId root, HashMap<NodeId, GraphNode> byKey, HashSet<NodeId> seen) =>
        candidate == root || (!seen.Contains(candidate) && byKey.Find(candidate).Bind(static node => node.Parent).Map(parent => IsDescendant(parent, root, byKey, seen.Add(candidate))).IfNone(false));

    static Seq<TallyFact> Tally(Seq<EditOp> merged, Seq<MergeConflict> conflicts) =>
        toSeq(merged.GroupBy(static op => op.KindName)).Map(static g => new TallyFact(TallySlot.Edit, g.Key, g.Count()))
            + toSeq(conflicts.GroupBy(static c => c.KindName)).Map(static g => new TallyFact(TallySlot.Conflict, g.Key, g.Count()));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                                                       |
| :-----: | :-------------------- | :---------------------------------------------------------------------------- |
|  [01]   | re-ingest align       | `Reconcile` on `Node.Object.ExternalId`                                       |
|  [02]   | forest topology       | `Relationship.Compose` containment edges                                      |
|  [03]   | node role             | `(ObjectKind, containment-whole)` neutral signal                              |
|  [04]   | geometry axis         | the FULL `Representations.ByIdentifier` map                                   |
|  [05]   | content axis          | the non-`Object` nodes diffed off the node map                                |
|  [06]   | content key           | seam `ContentAddress.Of` over `ToCanonicalBytes`                              |
|  [07]   | subtree prune         | domain accumulator `Clone` + `Append`, stable LE preimage                     |
|  [08]   | conflict accumulation | `MergeOutcome` carries merged + conflicts                                     |
|  [09]   | edit egress           | `Tombstone \| Members` over authoritative held-node addresses                 |
|  [10]   | conflict receipt      | `Version/ledger#MERGE_LAW` `ConflictReceipt`                                  |
|  [11]   | reconciliation seam   | `Rasm/Spatial/reconciliation` `GeometryHash` over frozen `EncodeForm` layouts |
|  [12]   | type correlation      | `TypeKey` classification-excluded `Name`/`Tag` natural key                    |
|  [13]   | patch target          | exact `NodeWire` ProtoJSON through `JsonFormatter`                            |

Each row's binding invariant, keyed to its policy:

- [01]-[RE_INGEST_ALIGN]: `Reconcile` aligns each freshly-minted rooted `NodeId` to the durable id on the 1:1 GlobalId, so no diff keys on a re-ingest `NodeId`.
- [02]-[FOREST_TOPOLOGY]: `Forest` derives `Parent`/`Ordinal`/`Children`; no second store.
- [03]-[NODE_ROLE]: never an IFC-class string scan of `Classification.Code`.
- [04]-[GEOMETRY_AXIS]: `Body` + analytical `Axis`/`Box`/`FootPrint`, kernel digests read not re-minted.
- [05]-[CONTENT_AXIS]: `DiffContent` carries every single-side content edit the `Object`-only forest never reaches.
- [06]-[CONTENT_KEY]: `ContentAddress.Of` mints every content key as the ONE seam hasher.
- [07]-[SUBTREE_PRUNE]: linear in changed nodes, no `GetHashCode`.
- [08]-[CONFLICT_ACCUMULATION]: one-pass classify, both carried, never first-abort.
- [09]-[EDIT_EGRESS]: both arms compare the held node's producer-carried base address.
- [10]-[CONFLICT_RECEIPT]: held/incoming `(Hlc, actor)` from the changefeed.
- [11]-[RECONCILIATION_SEAM]: `GraphNode.GeometryHash` is the RE-TARGETED consumer; the preimage pairs (form lane, digest) — a bare digest never crosses a form boundary.
- [12]-[TYPE_CORRELATION]: `TypeKey` diffs a re-keyed GlobalId-less `Type` as RENAME; the kernel V8a seed replaces the interim on landing.
- [13]-[PATCH_TARGET]: `Members` diffs and patches exact `NodeWire` ProtoJSON; insertion remains on `GraphDelta`.

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
