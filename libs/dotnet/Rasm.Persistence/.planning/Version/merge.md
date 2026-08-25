# [PERSISTENCE_VERSION_MERGE]

`StructuralMerge` aligns re-ingested roots and classifies topology and content in one base-relative merge. `EntityEdit` emits base-addressed tombstones or `FieldMask` member patches over the binary `NodeWire`, lowered onto the generated `Element.EntityEditWire` whose members arm carries one `PatchOp` per mask path, while insertions stay on the `EditOp.Insert` and `GraphDelta` mutation rail. `GraphNode` drives detection alone, and conflicts project available `(Hlc, actor)` evidence into `Conflict` without manufacturing missing authorship.

## [01]-[INDEX]

- [02]-[STRUCTURAL_DIFF]: Re-ingest alignment, Merkle-pruned matching, three-way merge, conflicts, and node edit egress.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` carries forest identity, content axes, sibling position, and Merkle subtree digest.
- Owner: `NodeRole`, `EditOp`, and `MergeConflict` close structural roles, operations, and conflicts.
- Owner: `EntityEdit` carries base-addressed tombstone and member-patch egress; `MemberPatch` is the mask beside its prior and successor `NodeWire`, and `Apply` is the substrate `Merge`.
- Owner: `PatchPolicy` admits the caller-supplied mask-path ceiling shared with the crossing.
- Owner: `TallyFact`, `MergeOutcome`, and `StructuralMerge` own merge evidence and the full merge fold.
- Cases: `EntityEdit` is `Tombstone | Members`; both arms carry the addressed current-node base.
- Cases: `Delete` maps to `Tombstone`; held-node material edits map to `Members`.
- Cases: `EditOp.Insert` remains on the graph mutation rail and does not fabricate a held-node EntityEdit.
- Entry: `Reconcile` aligns imported ids from GlobalId and unambiguous GlobalId-less type keys.
- Entry: `Forest` and `DiffContent` project object topology and non-object content onto separate detection axes.
- Entry: `ThreeWay` returns clean edits, typed conflicts, and tally evidence in one result.
- Entry: `Patch(script, base, target, policy, key)` returns `Fin<HashMap<NodeId, EntityEdit>>` on the element fault rail; `EditWire.Wire(edit, key)` lowers one edit onto the generated message.
- Auto: Every base is `ContentAddress.Of(baseNode, base.Header.Tolerance)`.
- Auto: `Members` diffs the binary `NodeWire` pair field by field off `NodeWire.Descriptor.Fields.InFieldNumberOrder()` through `IFieldAccessor` equality — message fields recurse, repeated and map fields compare whole, a presence flip names the path, a changed oneof arm names both arms.
- Auto: `FieldMask.IsValid` gates the path set; `Merge` under `ReplaceMessageFields`/`ReplaceRepeatedFields`/`ReplacePrimitiveFields` applies it, so a primitive returning to its default crosses as a change — the member a ProtoJSON diff elided.
- Auto: The wire pointer re-spells each mask segment through the field's `JsonName`, and the op kind derives from which side renders the member — `Add`, `Replace`, or `Remove` — so the peer's ProtoJSON document and the binary mask name one change.
- Auto: `Patch` collapses an over-ceiling path set to the top-level field set both sides render, so the successor replaces whole and the op count stays under the ceiling by construction.
- Output: `MergeOutcome.Counts` carries the conflict count, and each `MergeConflict` projects the held/incoming changefeed evidence it actually has to `Conflict`; each projected conflict fires the `rasm.persistence.merge.conflict` observe point (`Store/observability#HOOK_RAIL`) at the composition root.
- Packages: Google.Protobuf owns `FieldMask`/`Merge`/`IsValid`, the descriptor walk (`Fields.InFieldNumberOrder`, `FieldDescriptor.Accessor`/`JsonName`/`HasPresence`/`FieldType`/`IsRepeated`, `MessageDescriptor.Parser`), and `Value.Parser.ParseJson`; Rasm.AppHost `WireJson.Formatter.WriteValue` is the one ProtoJSON leaf render.
- Packages: Rasm `ContentHash.Of` + `CanonicalWriter` own every local digest (`GeometryDigest`, `Seal`); LanguageExt owns immutable carriers and `Fin`.
- Packages: Thinktecture owns closed unions; NodaTime owns merge evidence time.
- Growth: detection absorbs each new edit operation; edit egress stays the closed lifecycle union.
- Boundary: Re-ingest alignment precedes every durable-`NodeId` diff.
- Boundary: `Forest` reads `Relationship.Compose` containment and the complete representation map.
- Boundary: Non-object content nodes diff directly from the node map and never disappear behind the object forest.
- Boundary: `GraphNode` hashes drive detection only; patch masks target the binary `NodeWire` the seam encoded, and ProtoJSON is a `PatchOp` leaf render, never a patch target or a relay.
- Boundary: TypeScript decodes the generated `EntityEditWire`, applies the `PatchOp` run to its retained ProtoJSON, and decodes the successor through the existing node landing.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.AppHost.Runtime;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
// Contracts are retired from this logic.
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Version;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeRole {
    public static readonly NodeRole Occurrence = new("occurrence");
    public static readonly NodeRole Type = new("type");
    public static readonly NodeRole Container = new("container");
    public static readonly NodeRole Annotation = new("annotation");

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
    public sealed record ParallelEdit(NodeId Key, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record DeleteUpdate(NodeId Key, bool DeletedByOurs, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record MoveMove(NodeId Key, Option<NodeId> OurParent, Option<NodeId> TheirParent, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record ReorderReorder(NodeId Key, int OurOrdinal, int TheirOrdinal, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record TypeChange(NodeId Key, NodeRole OurRole, NodeRole TheirRole, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record TopologyBreak(NodeId Key, UInt128 OurGeometry, UInt128 TheirGeometry, Option<ConflictSide> Ours, Option<ConflictSide> Theirs) : MergeConflict;
    public sealed record ContainmentCycle(NodeId Key, NodeId Ancestor, bool ByOurs, Option<ConflictSide> Side) : MergeConflict;

    public NodeId Subject => this.Map(parallelEdit: p => p.Key, deleteUpdate: d => d.Key, moveMove: m => m.Key, reorderReorder: r => r.Key, typeChange: t => t.Key, topologyBreak: b => b.Key, containmentCycle: y => y.Key);
    public string KindName => this.Map(parallelEdit: static _ => "parallelEdit", deleteUpdate: static _ => "deleteUpdate", moveMove: static _ => "moveMove", reorderReorder: static _ => "reorderReorder", typeChange: static _ => "typeChange", topologyBreak: static _ => "topologyBreak", containmentCycle: static _ => "containmentCycle");

    public ColumnFamily Family => this is TopologyBreak ? ColumnFamily.Geometry : ColumnFamily.Crdt;

    public Option<string> ConflictAxis => this.Map(
        parallelEdit: static _ => Some("content"), deleteUpdate: static _ => Option<string>.None,
        moveMove: static _ => Some("parent"), reorderReorder: static _ => Some("ordinal"),
        typeChange: static _ => Some("role"), topologyBreak: static _ => Some("content"),
        containmentCycle: static _ => Option<string>.None);

    public (Option<ConflictSide> Held, Option<ConflictSide> Incoming) Evidence => this.Map(
        parallelEdit: static c => (c.Ours, c.Theirs),
        deleteUpdate: static c => (c.Ours, c.Theirs),
        moveMove: static c => (c.Ours, c.Theirs),
        reorderReorder: static c => (c.Ours, c.Theirs),
        typeChange: static c => (c.Ours, c.Theirs),
        topologyBreak: static c => (c.Ours, c.Theirs),
        containmentCycle: static c => (Option<ConflictSide>.None, c.Side));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EntityEdit {
    private EntityEdit() { }
    public sealed record Tombstone(NodeId Key, ContentAddress Base) : EntityEdit;
    public sealed record Members(NodeId Key, ContentAddress Base, MemberPatch Patch) : EntityEdit;
}

public sealed record MemberPatch(FieldMask Mask, NodeWire Prior, NodeWire Successor) {
    public static readonly FieldMask.MergeOptions Replace = new() { ReplaceMessageFields = true, ReplaceRepeatedFields = true, ReplacePrimitiveFields = true };

    public int Paths => Mask.Paths.Count;

    public NodeWire Apply(NodeWire target) {
        NodeWire patched = target.Clone();
        Mask.Merge(Successor, patched, Replace);
        return patched;
    }

    public static Fin<Option<MemberPatch>> Between(NodeWire prior, NodeWire successor, PatchPolicy policy, Op key) =>
        MemberDiff.Paths(prior, successor) switch {
            { IsEmpty: true } => Fin.Succ(Option<MemberPatch>.None),
            var paths => Masked(paths.Count <= policy.OperationCeiling ? paths : MemberDiff.TopLevel(prior, successor), prior, successor, key),
        };

    static Fin<Option<MemberPatch>> Masked(Seq<string> paths, NodeWire prior, NodeWire successor, Op key) =>
        new FieldMask { Paths = { paths } } switch {
            var mask when FieldMask.IsValid(NodeWire.Descriptor, mask) => Fin.Succ(Some(new MemberPatch(mask, prior, successor))),
            var mask => ElementFault.ValueRejected(key, $"<entity-edit-mask:{string.Join(',', mask.Paths)}>"),
        };
}

public sealed record PatchPolicy {
    private PatchPolicy(int operationCeiling) => OperationCeiling = operationCeiling;
    public int OperationCeiling { get; }

    public static Fin<PatchPolicy> Of(int operationCeiling, Op key) => operationCeiling > 0
        ? Fin.Succ(new PatchPolicy(operationCeiling))
        : ElementFault.ValueRejected(key, $"<entity-edit-operation-ceiling:{operationCeiling}>");
}

public static class MemberDiff {
    public static Seq<string> Paths(IMessage before, IMessage after) => Paths(before, after, Seq<string>());

    static Seq<string> Paths(IMessage before, IMessage after, Seq<string> prefix) =>
        toSeq(before.Descriptor.Fields.InFieldNumberOrder()).Bind(field => {
            Seq<string> path = prefix.Add(field.Name);
            (object held, object next) = (field.Accessor.GetValue(before), field.Accessor.GetValue(after));
            return field switch {
                { HasPresence: true } when field.Accessor.HasValue(before) != field.Accessor.HasValue(after) => Seq(Spelled(path)),
                { FieldType: FieldType.Message, IsRepeated: false } when held is IMessage prior && next is IMessage successor => Paths(prior, successor, path),
                _ => Equals(held, next) ? Seq<string>() : Seq(Spelled(path)),
            };
        });

    public static Seq<string> TopLevel(IMessage prior, IMessage successor) =>
        toSeq(prior.Descriptor.Fields.InFieldNumberOrder())
            .Filter(field => Rendered(field, prior) || Rendered(field, successor))
            .Map(static field => field.Name);

    public static bool Rendered(FieldDescriptor field, IMessage message) =>
        field.HasPresence
            ? field.Accessor.HasValue(message)
            : !Equals(field.Accessor.GetValue(message), field.Accessor.GetValue(message.Descriptor.Parser.ParseFrom(ByteString.Empty)));

    static string Spelled(Seq<string> path) => string.Join('.', path);
}

public static class EditWire {
    public static Fin<Host.EntityEditWire> Wire(EntityEdit edit, Op key) => edit.Switch(
        tombstone: row => Address(row.Key, key).Map(id => new Host.EntityEditWire {
            Tombstone = new Host.EditTombstone { Key = id, Base = ContentHash.Wire(row.Base.Value) },
        }),
        members: row => Address(row.Key, key).Map(id => new Host.EntityEditWire {
            Members = new Host.EditMembers { Key = id, Base = ContentHash.Wire(row.Base.Value), Patch = { Ops(row.Patch) } },
        }));

    static Fin<ByteString> Address(NodeId id, Op key) => ContentHash.Admit(id.Value, key).Map(ContentHash.Wire);

    static Seq<Control.PatchOp> Ops(MemberPatch patch) =>
        toSeq(patch.Mask.Paths).Map(path => Op(patch, toSeq(path.Split('.'))));

    static Control.PatchOp Op(MemberPatch patch, Seq<string> segments) {
        string pointer = Pointer(NodeWire.Descriptor, segments);
        return (Leaf(patch.Prior, segments), Leaf(patch.Successor, segments)) switch {
            (_, { IsNone: true }) => new Control.PatchOp { Remove = new Control.PatchRemove { Path = pointer } },
            ({ IsNone: true }, { Case: object next }) => new Control.PatchOp { Add = new Control.PatchAdd { Path = pointer, Value = Json(next) } },
            (_, { Case: object next }) => new Control.PatchOp { Replace = new Control.PatchReplace { Path = pointer, Value = Json(next) } },
        };
    }

    static string Pointer(MessageDescriptor root, Seq<string> segments) =>
        segments.Fold((Owner: root, Path: string.Empty), static (at, name) => {
            FieldDescriptor field = at.Owner.FindFieldByName(name);
            return (field.FieldType == FieldType.Message && !field.IsRepeated ? field.MessageType : at.Owner, $"{at.Path}/{field.JsonName}");
        }).Path;

    static Option<object> Leaf(IMessage message, Seq<string> segments) {
        FieldDescriptor field = message.Descriptor.FindFieldByName(segments[0]);
        return segments.Count == 1
            ? MemberDiff.Rendered(field, message) ? Some(field.Accessor.GetValue(message)) : None
            : field.Accessor.HasValue(message) && field.Accessor.GetValue(message) is IMessage inner ? Leaf(inner, segments.Tail) : None;
    }

    static Value Json(object value) {
        StringWriter writer = new(CultureInfo.InvariantCulture);
        WireJson.Formatter.WriteValue(writer, value);
        return Value.Parser.ParseJson(writer.ToString());
    }
}

public readonly record struct TallyFact(TallySlot Slot, string Kind, int Count);
public readonly record struct MergeOutcome(Seq<EditOp> Merged, Seq<MergeConflict> Conflicts, Seq<TallyFact> Counts) {
    public bool Clean => Conflicts.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StructuralMerge {
    public static Seq<GraphNode> Forest(ElementGraph graph) {
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

    public static Seq<EditOp> DiffContent(HashMap<NodeId, GraphNode> from, HashMap<NodeId, GraphNode> to) =>
        toSeq(to.Map((key, node) => from.Find(key).Match(
            Some: prior => prior.PropertyHash == node.PropertyHash ? (EditOp)new EditOp.Match(key) : new EditOp.Update(key, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash),
            None: () => new EditOp.Insert(node))).Values)
        + toSeq(from.Filter((key, _) => !to.ContainsKey(key)).Map(static (key, _) => (EditOp)new EditOp.Delete(key)).Values);

    public static MergeOutcome ThreeWay(ElementGraph @base, ElementGraph ours, ElementGraph theirs, Func<NodeId, Option<OpLogEntry>> stampOurs, Func<NodeId, Option<OpLogEntry>> stampTheirs) {
        Seq<GraphNode> baseForest = Forest(@base), ourForest = Forest(ours), theirForest = Forest(theirs);
        HashMap<NodeId, GraphNode> baseContent = ContentNodes(@base), ourContent = ContentNodes(ours), theirContent = ContentNodes(theirs);
        HashMap<NodeId, HashMap<string, EditOp>> ourEdits = ByKeyAxis(Diff(baseForest, ourForest) + DiffContent(baseContent, ourContent));
        HashMap<NodeId, HashMap<string, EditOp>> theirEdits = ByKeyAxis(Diff(baseForest, theirForest) + DiffContent(baseContent, theirContent));
        HashMap<NodeId, GraphNode> oursByKey = toHashMap(ourForest.Map(static n => (n.Key, n)));
        HashMap<NodeId, GraphNode> theirsByKey = toHashMap(theirForest.Map(static n => (n.Key, n)));
        Seq<MergeConflict> conflicts = toSeq(ourEdits.Keys.Where(theirEdits.ContainsKey)
            .Bind(key => Conflicts(key, ourEdits.Find(key).IfNone(HashMap<string, EditOp>()), theirEdits.Find(key).IfNone(HashMap<string, EditOp>()), stampOurs(key), stampTheirs(key)))
            .Append(Cycles(ourEdits, oursByKey, ByOurs: true, stampOurs))
            .Append(Cycles(theirEdits, theirsByKey, ByOurs: false, stampTheirs)));
        HashSet<NodeId> poisoned = conflicts.Filter(static c => c.ConflictAxis.IsNone).Map(static c => c.Subject).ToHashSet();
        HashSet<(NodeId Key, string Axis)> conflictedAxes = conflicts.Bind(c => c.ConflictAxis.Map(axis => (c.Subject, axis)).ToSeq()).ToHashSet();
        bool Excluded(NodeId key, string axis) => poisoned.Contains(key) || conflictedAxes.Contains((key, axis));
        Seq<EditOp> merged = toSeq(ourEdits.Map((key, axes) => toSeq(axes.Filter((axis, _) => !Excluded(key, axis)).Values)).Values.Bind(static ops => ops)
            .Append(theirEdits.Map((key, axes) => toSeq(axes.Filter((axis, _) => !Excluded(key, axis) && !ourEdits.Find(key).Map(a => a.ContainsKey(axis)).IfNone(false)).Values)).Values.Bind(static ops => ops)));
        return new MergeOutcome(merged, conflicts, Tally(merged, conflicts));
    }

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
                    .Bind(after => ElementWire.Encode(before, @base.Header.Tolerance, key).Bind(beforeWire =>
                        ElementWire.Encode(after, target.Header.Tolerance, key).Bind(afterWire =>
                            MemberPatch.Between(beforeWire, afterWire, policy, key)
                                .Map(patch => patch.Map(held => (EntityEdit)new EntityEdit.Members(
                                    subject, ContentAddress.Of(before, @base.Header.Tolerance), held))))))),
        };

    public static Conflict Project(MergeConflict conflict, ModelId model) {
        (Option<ConflictSide> held, Option<ConflictSide> incoming) = conflict.Evidence;
        return new(model, conflict.Subject.Value, conflict.Family, held, incoming);
    }

    public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested) {
        HashMap<string, NodeId> durable = Correlation(persisted);
        HashMap<NodeId, NodeId> remap = toHashMap(toSeq(Correlation(ingested).AsIterable())
            .Choose(pair => durable.Find(pair.Key).Map(id => (Ingest: pair.Value, Durable: id)))
            .Filter(static move => move.Ingest != move.Durable));
        return remap.IsEmpty ? (ingested, remap) : (Reindex(ingested, remap), remap);
    }

    static HashMap<string, NodeId> Correlation(ElementGraph graph) {
        HashMap<string, NodeId> rooted = toHashMap(toSeq(graph.Nodes.Values).Choose(ExternalKey));
        return toSeq(graph.Nodes.Values.Select(TypeKey).Somes().GroupBy(static pair => pair.External).Where(static g => g.Count() == 1))
            .Fold(rooted, static (acc, g) => acc.TryAdd(g.Key, g.First().Id));
    }

    static Option<(string External, NodeId Id)> ExternalKey(Node node) =>
        node is Node.Object { ExternalId: var external } obj ? external.Map(ext => (External: ext, Id: obj.Id)) : None;

    static Option<(string External, NodeId Id)> TypeKey(Node node) =>
        node is Node.Object { ExternalId.IsNone: true } obj && obj.Kind == ObjectKind.Type
            ? Some((External: $"type:{obj.Name}\u001f{obj.Tag}", Id: obj.Id))
            : None;

    static ElementGraph Reindex(ElementGraph graph, HashMap<NodeId, NodeId> remap) {
        NodeId Resolve(NodeId id) => remap.Find(id).IfNone(id);
        FrozenDictionary<NodeId, Node> nodes = graph.Nodes.Values.Select(node => node.Relabel(Resolve(node.Id))).ToFrozenDictionary(static node => node.Id);
        ImmutableArray<Relationship> edges = [.. graph.Edges.Select(edge => edge.Remap(Resolve))];
        return ElementGraph.Of(graph.Header, nodes, edges);
    }

    static UInt128 GeometryDigest(RepresentationContentHash representations) =>
        representations.ByIdentifier.IsEmpty
            ? UInt128.Zero
            : ContentHash.Of(representations, static (r, w) => {
                w.Sorted(r.ByIdentifier.ToSeq(), static pair => pair.Key.Key, Comparer<int>.Default,
                    static (pair, x) => { x.Ordinal(pair.Key.Key).U128(pair.Value); });
            });

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
        Seq<UInt128> rollups = children.Map(static subtree => subtree.Head.Map(static r => r.SubtreeHash).IfNone(UInt128.Zero));
        UInt128 sealedHash = ContentHash.Of((Node: node, Rollups: rollups), static (s, w) => {
            w.U128(s.Node.GeometryHash).U128(s.Node.PropertyHash).Ordinal(s.Node.Ordinal).String(s.Node.Role.Key)
             .Rows(s.Rollups, static (hash, x) => { x.U128(hash); });
        });
        return Seq(node with { SubtreeHash = sealedHash }) + children.Bind(static subtree => subtree);
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
        Option<ConflictSide> ourSide = Stamp(o);
        Option<ConflictSide> theirSide = Stamp(t);
        return ours.ContainsKey("delete") && theirs.Keys.Exists(static a => a != "delete")
            ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: true, ourSide, theirSide))
            : theirs.ContainsKey("delete") && ours.Keys.Exists(static a => a != "delete")
                ? Seq<MergeConflict>(new MergeConflict.DeleteUpdate(key, DeletedByOurs: false, ourSide, theirSide))
                : toSeq(ours.Keys.Filter(theirs.ContainsKey).Choose(axis => Diverge(key, ours[axis], theirs[axis], ourSide, theirSide)));
    }

    static Option<MergeConflict> Diverge(NodeId key, EditOp ours, EditOp theirs, Option<ConflictSide> ourSide, Option<ConflictSide> theirSide) => (ours, theirs) switch {
        (EditOp.Retype r1, EditOp.Retype r2) when r1.ToRole.Key != r2.ToRole.Key => new MergeConflict.TypeChange(key, r1.ToRole, r2.ToRole, ourSide, theirSide),
        (EditOp.Move m1, EditOp.Move m2) when m1.ToParent != m2.ToParent => new MergeConflict.MoveMove(key, m1.ToParent, m2.ToParent, ourSide, theirSide),
        (EditOp.Reorder r1, EditOp.Reorder r2) when r1.ToOrdinal != r2.ToOrdinal => new MergeConflict.ReorderReorder(key, r1.ToOrdinal, r2.ToOrdinal, ourSide, theirSide),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToGeometry != u2.ToGeometry => new MergeConflict.TopologyBreak(key, u1.ToGeometry, u2.ToGeometry, ourSide, theirSide),
        (EditOp.Update u1, EditOp.Update u2) when u1.ToProperty != u2.ToProperty => new MergeConflict.ParallelEdit(key, ourSide, theirSide),
        _ => Option<MergeConflict>.None,
    };

    static Seq<MergeConflict> Cycles(HashMap<NodeId, HashMap<string, EditOp>> edits, HashMap<NodeId, GraphNode> byKey, bool ByOurs, Func<NodeId, Option<OpLogEntry>> stamp) =>
        toSeq(edits.Keys.Choose(key => ParentOf(key, edits, byKey).Filter(parent => IsDescendant(parent, key, byKey, HashSet<NodeId>()))
            .Map(parent => (MergeConflict)new MergeConflict.ContainmentCycle(key, parent, ByOurs, Stamp(stamp(key))))));

    static Option<ConflictSide> Stamp(Option<OpLogEntry> entry) =>
        entry.Map(static e => new ConflictSide(new Hlc(e.Physical, e.Logical), e.Actor));

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
|  [07]   | subtree prune         | kernel `ContentHash.Of` over `U128`/`Ordinal`/`String`/`Rows`                 |
|  [08]   | conflict accumulation | `MergeOutcome` carries merged + conflicts                                     |
|  [09]   | edit egress           | `Tombstone \| Members` lowered onto `Element.EntityEditWire` by `EditWire`    |
|  [10]   | conflict              | `Version/ledger#MERGE_LAW` `Conflict`                                         |
|  [11]   | reconciliation seam   | `Rasm/Spatial/reconciliation` `GeometryHash` over frozen `EncodeForm` layouts |
|  [12]   | type correlation      | `TypeKey` classification-excluded `Name`/`Tag` natural key                    |
|  [13]   | patch target          | binary `NodeWire`: `FieldMask` diff, `IsValid` gate, `Merge` apply            |

Each row's binding invariant, keyed to its policy:

- [01]-[RE_INGEST_ALIGN]: `Reconcile` aligns each freshly-minted rooted `NodeId` to the durable id on the 1:1 GlobalId, so no diff keys on a re-ingest `NodeId`.
- [02]-[FOREST_TOPOLOGY]: `Forest` derives `Parent`/`Ordinal`/`Children`; no second store.
- [03]-[NODE_ROLE]: never an IFC-class string scan of `Classification.Code`.
- [04]-[GEOMETRY_AXIS]: the complete `Representations.ByIdentifier` map, kernel digests read not re-minted.
- [05]-[CONTENT_AXIS]: `DiffContent` carries every single-side content edit the `Object`-only forest never reaches.
- [06]-[CONTENT_KEY]: `ContentAddress.Of` mints every content key as the ONE seam hasher.
- [07]-[SUBTREE_PRUNE]: linear in changed nodes, no `GetHashCode`, no second alphabet beside the kernel writer.
- [08]-[CONFLICT_ACCUMULATION]: one-pass classify, both carried, never first-abort.
- [09]-[EDIT_EGRESS]: both arms compare the held node's producer-carried base address.
- [10]-[CONFLICT]: held/incoming `(Hlc, actor)` from the changefeed.
- [11]-[RECONCILIATION_SEAM]: `GraphNode.GeometryHash` is the RE-TARGETED consumer; the preimage pairs (form lane, digest) — a bare digest never crosses a form boundary.
- [12]-[TYPE_CORRELATION]: `TypeKey` diffs a re-keyed GlobalId-less `Type` as RENAME; the kernel V8a seed replaces the interim on landing.
- [13]-[PATCH_TARGET]: `Members` diffs the binary `NodeWire` and applies through `Merge`; ProtoJSON is the `PatchOp` leaf render alone; insertion remains on `GraphDelta`.

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
