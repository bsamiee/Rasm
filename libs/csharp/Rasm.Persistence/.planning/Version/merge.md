# [PERSISTENCE_VERSION_MERGE]

`StructuralMerge` aligns re-ingested rooted objects, projects the `ElementGraph` into a Merkle-pruned object forest, compares non-topologized content nodes directly, and classifies both axes in one base-relative merge. `ElementGraph.EqualityComparer.Default.Inequalities` localizes member changes for `EntityEdit.Members`; whole-node absence becomes `EntityEdit.Tombstone`. `GraphNode` remains a detection projection only: every RFC 6902 value resolves from a real seam `Node`, and every conflict projects the stored `(Hlc, actor)` evidence into `ConflictReceipt`.

## [01]-[INDEX]

- [01]-[STRUCTURAL_DIFF]: the re-ingest `Reconcile` GlobalId alignment, the `Inequalities` change-set, the Merkle-pruned `Object`-forest match, the non-topologized content-node axis, the base-relative three-way merge, the typed conflict classes, and the RFC 6902 patch egress.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` the identity-keyed forest node carrying the geometry content axis (`GeometryHash` = the digest of the `Object`'s WHOLE `Representations.ByIdentifier` map), the node content-axis hash (`PropertyHash` = the seam `Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` over the `Projection/address#CANONICAL_WRITER` `Node.ToCanonicalBytes`), the sibling ordinal, and the Merkle subtree digest; `NodeRole` the NEUTRAL structural-role vocabulary (`Occurrence`/`Type`/`Container`/`Annotation`) a node's `ObjectKind` plus its containment-`Whole` role derive — never an IFC entity-class string the neutral seam classification does not carry; `EditOp` the forest-edit operation family driven by a generated `Switch`/`Map`; `MergeConflict` the typed conflict-class family carrying both sides' op-log stamp; `EntityEdit` the per-key egress kind (`Tombstone` whole-node removal vs `Members` RFC 6902 member-patch); `TallySlot`/`TallyFact` the edit-versus-conflict count axis; `MergeOutcome` the settled three-way receipt; `StructuralMerge` the static surface owning the re-ingest `Reconcile` GlobalId alignment, the `ElementGraph → GraphNode` forest projection, the content-node projection, the Merkle-pruned classification, the base-relative three-way merge over both axes, the conflict classification fold, the `Inequalities`-driven member-granular RFC 6902 patch projection, and the `ConflictReceipt` projection.
- Cases: `Match | Insert | Delete | Update | Move | Reorder | Retype` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | ReorderReorder | TypeChange | TopologyBreak | ContainmentCycle` on `MergeConflict`; `Tombstone | Members` on `EntityEdit`; the patch egress maps `Delete → Tombstone`, `Insert → Members` carrying the REAL `Node` payload resolved from the `target` graph (the full Material/PropertySet/Object a CDE materializes, never the hash-only `GraphNode` diff projection), and `Update`/`Move`/`Reorder`/`Retype → Members` over the targeted member; an `Update` whose member rows did not localize replaces the WHOLE real target `Node` at the `""` root pointer, and a key neither graph still holds is the `Tombstone` — a `GraphNode` hash field never enters a patch.
- Entry: `public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested)` aligns a re-ingest's freshly-minted `NodeId`s to the durable persisted ids by correlating rooted nodes on `Node.Object.ExternalId` (the stable 1:1 GlobalId) plus GlobalId-less `Type` objects on the classification-independent `TypeKey` natural key (an ambiguous natural key drops from correlation — a 1:1 aligner never guesses) and rewriting the ingest's node ids + edge endpoints onto the durable ids; `public static Seq<GraphNode> Forest(ElementGraph graph)` projects each `Object` node's `Parent`/`Ordinal`/`Children` topology from the seam `Relationship.Compose` containment edges, its `GeometryHash` from the full `Object` `Representations.ByIdentifier` map, its `PropertyHash` from the seam `ContentAddress.Of` over `Node.ToCanonicalBytes`, and its `NodeRole` from `(ObjectKind, containment-whole)`, then seals the `SubtreeHash` bottom-up; `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` is the minimal forest-edit script; `public static MergeOutcome ThreeWay(ElementGraph @base, ElementGraph ours, ElementGraph theirs, Func<NodeId, Option<OpLogEntry>> stampOurs, Func<NodeId, Option<OpLogEntry>> stampTheirs)` folds non-conflicting `Object`-forest AND content-node edits and accumulates true overlaps as typed conflicts; `public static HashMap<NodeId, EntityEdit> Patch(Seq<EditOp> script, ElementGraph @base, ElementGraph target)` groups the resolved script by target key and projects each `Update` into the MEMBER-GRANULAR RFC 6902 ops the `@base`→`target` `Inequalities` member diff localizes (a changed `Layers[2].Thickness` is one `Replace` at that path, never a whole-node replace); `public static ConflictReceipt Project(MergeConflict conflict, ModelId model, Guid correlation, Instant at)` reuses the one conflict vocabulary, carrying the merged model and the conflict's real `ColumnFamily` (the geometry axis for a `TopologyBreak`, the CRDT axis for every property/structural class).
- Auto: a re-ingest first runs `Reconcile` so the freshly-minted ingest `NodeId`s align to the durable persisted ids on the stable `Node.Object.ExternalId` GlobalId — and a GlobalId-less `Type` on its `TypeKey` natural key — (the neutral `NodeId` is minted afresh each `Project`, so a raw `NodeId`-keyed diff reads the whole re-import as delete-all + insert-all); with the graphs aligned, node matching is the durable `NodeId` and the `(GeometryHash, PropertyHash)` content signature drives change DETECTION — the forest edit emits `Match` for signature-equal same-parent nodes, `Move` for signature-equal parent-changed, `Reorder` for same-parent ordinal-changed, `Retype` for `NodeRole`-changed, `Update` for signature-changed carrying both content axes, and `Insert`/`Delete` for unmatched; the `GeometryHash` folds EVERY representation the `Object` carries (the heavy `Body` mesh AND the analytical `Axis`/`Box`/`FootPrint` content keys, sorted by identifier), so a structural-line or space-boundary geometry edit surfaces as a geometry divergence the same as a display-mesh edit; the `SubtreeHash` folds each node's own `(GeometryHash, PropertyHash, Ordinal, Role.Key)` preimage with its sorted children's digests through `XxHash128.Append`, clones the completed running state before sealing it, and compares one digest per node so the walk descends only where it diverges; the three-way merge runs TWO axes — the `Object`-forest axis (`Move`/`Reorder`/`Retype`/`TopologyBreak`/content `ParallelEdit` per key-axis) AND the content-node axis (the `Material`/`PropertySet`/`QuantitySet`/`Coverage`/`Appearance`/`Assessment` nodes the forest does not topologize, diffed directly off the node map so a single-side content edit APPLIES as a `Members` patch and a both-side content edit is a `ParallelEdit`) — and the `Inequalities` change-set localizes the changed MEMBER each `EntityEdit.Members` patch targets.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count folded into `MergeOutcome.Counts`, and each `MergeConflict` projects to `ConflictReceipt` with the held/incoming `(Hlc, actor)` from the changefeed; each projected `ConflictReceipt` fires the `rasm.persistence.merge.conflict` observe point (`Store/observability#HOOK_RAIL`) at the composition root.
- Packages: Rasm.Element (`ElementGraph`/`Node`/`NodeId`/`Relationship` + `ContentAddress.Of` the seam node-content key the `PropertyHash` axis composes), Generator.Equals (`[Equatable]` deep equality + `Inequalities` member diff for member-localization), System.IO.Hashing (the `SubtreeHash` Merkle `XxHash128.Append`/`Clone` accumulator only — the node-content axis composes the seam `ContentAddress`, never a raw hasher), Microsoft.AspNetCore.JsonPatch.SystemTextJson (the untyped `JsonPatchDocument` JSON-pointer `Add`/`Replace` member-granular egress), System.Text.Json (`JsonSerializer.SerializeToElement` under the `Element/codec` `ElementJson.Options` — every patched value lowers through the ONE codec converter graph), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Buffers.Binary, System.Runtime.InteropServices, System.Collections.Frozen, System.Collections.Immutable, BCL inbox.
- Growth: a new edit op is one `EditOp` case plus one `Switch`/`Map` arm; a new conflict class is one `MergeConflict` case; a new egress kind is one `EntityEdit` case; zero new surface — a line-based text diff, a JSON-patch diff over serialized whole nodes, a third content axis, or a second merge resolver is the deleted form; the `Version/ledger#SYNC_TRANSPORTS` `GraphDiff` stays the transport-level set-difference and the `Version/timetravel#TIME_TRAVEL` `RangeDiff` the AS-OF member delta between two settled snapshots — three altitude-split diffs, never duplicated; `Reconcile` is the re-ingest identity ALIGNER that feeds the diff, never a fourth diff.
- Boundary: node identity is the durable `NodeId` the re-ingest `Reconcile` aligns BEFORE the diff — a rooted node on the 1:1 IFC GlobalId (`Node.Object.ExternalId`), a GlobalId-less `Type` on the classification-excluded `TypeKey` natural key (an ambiguous key drops; the kernel V8a seed replaces the interim on landing) — the neutral `NodeId` is freshly minted each `Project`, so a re-imported model is correlated on the stable keys, never the minted id; a `NodeId`-only re-ingest diff that reads the whole import as delete-all + insert-all, a content-signature "match" that silently drops a changed element's identity continuity, a positional or ordinal-only match, or a `Guid`-keyed parallel node identity beside the graph's own key is the named defect because after `Reconcile` the graph owns the one durable key the diff matches on; the `ElementGraph → GraphNode` forest reads `Object` topology VERBATIM from the seam `Relationship.Compose` containment edges — the parent is the `Whole`, the child the `Part`, the sibling order the index among one `Whole`'s `Compose` edges — so a positional re-derivation or a second containment store is the deleted form; `NodeRole` derives from the NEUTRAL `(ObjectKind, containment-whole)` signal the graph already encodes — an `Object` matched as a `Compose.Contain` `Whole` is a `Container`, an occurrence/type flip is an `Occurrence`/`Type` retype — never an `['I','f','c',…]` string match against `Classification.Code` (the seam classification is a neutral `(system, code)`, the `Code` a code-within-the-system NOT an IFC entity class, so an IFC-class literal match is the phantom deleted form, the IFC roster living wholly in the `Rasm.Bim` projector); `GraphNode.GeometryHash` folds the `Object`'s WHOLE `Representations.ByIdentifier` keyed map (every `RepresentationIdentifier` the kernel content-keyed, sorted, the `Body` mesh AND the analytical `Axis`/`Box`/`FootPrint` — reading only `Body` silently misses a structural-line or space-boundary change, the deleted thin slice) of kernel-minted digests read not re-minted — the digests are `Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE` `GeometryHash` values minted over the kernel-FROZEN `EncodeForm` byte layouts, this page the seam's RE-TARGETED consumer, and the fold preimage pairs each identifier (the form lane) WITH its digest so a bare digest never crosses a form boundary — and `GraphNode.PropertyHash` composes the seam `ContentAddress.Of` over `Node.ToCanonicalBytes` — the ONE seam hasher, never a raw second `XxHash128` instance and never a re-digest; the `SubtreeHash` is the Persistence-local Merkle digest `XxHash128` over a stable deterministic byte stream (the `(GeometryHash, PropertyHash, Ordinal, Role, child digests)` rollup, NOT a node-content identity) with NO `GetHashCode` so two peers seal the identical digest and a byte-identical subtree under an unchanged root prunes whole; the content-node axis is diffed DIRECTLY off the two node maps (the `Material`/`PropertySet`/`QuantitySet`/`Coverage`/`Appearance`/`Assessment` nodes the `Object`-forest never topologizes), NOT recovered by `MemberPath`-segment archaeology — the change-set keys on the durable `NodeId` the map already holds, the `Inequalities` member diff used only to localize WHICH member an `EntityEdit.Members` patch touches, so a single-side content edit APPLIES (an `Object`-only forest silently drops it, the deleted form) and a both-side content edit is a `ParallelEdit`; the three-way merge is base-relative so a node both sides left untouched never enters a conflict, and a structural merge is always-succeeds-with-annotations — `ThreeWay` returns `MergeOutcome` carrying BOTH the clean merged edit script AND the full conflict sequence in one pass, never a `Validation` fail-XOR-succeed that discards the partial merge; each `MergeConflict` carries both sides' `(Hlc, actor)` resolved from the changefeed stamp delegates so `Project` mints a `ConflictReceipt` with real evidence; the merge result re-projects two ways — as `EditOp` rows applied through the CRDT algebra (a `Reorder` resolves against the `Version/commits#CRDT_ALGEBRA` `RgaSequence` sibling order) and as a `HashMap<NodeId, EntityEdit>` (one egress kind per touched key, a `Members` patch a CDE/inspector consumer applies break-on-first-error to the node snapshot clone, a `Tombstone` dropping it); a `ContainmentCycle` (a `Move` whose target becomes its own descendant) is detected on BOTH sides, the cycle walk visited-set-bounded so the detector terminates on the cyclic input, and the cycle-bearing key is excluded from the merged script before patch projection because a containment graph admits no cycle.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using Generator.Equals;
using LanguageExt;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
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
// The NEUTRAL structural-role vocabulary the forest retype axis routes on — derived from the seam's OWN
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

    // The neutral role projection: a Type object is Type; a containment Whole is a Container; an occurrence with
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
// The forest node. GeometryHash is the digest of the Object's WHOLE Representations.ByIdentifier map (Body + the
// analytical Axis/Box/FootPrint), PropertyHash the seam ContentAddress over Node.ToCanonicalBytes. Matches tests
// the content signature; the SubtreeHash is the Merkle prune key. A content node (non-Object) carries its
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

    // The merge-lane the conflict adjudicates on — only a TopologyBreak is the geometry axis; every property/
    // structural class is the CRDT axis — derived once so the receipt projection reads one accessor, never a
    // seven-arm Map that repeats `ColumnFamily.Crdt` six times.
    public ColumnFamily Family => this is TopologyBreak ? ColumnFamily.Geometry : ColumnFamily.Crdt;

    // The conflicted AXIS the merged script excludes — a per-(key, axis) mask, so one content conflict never
    // suppresses a clean move/reorder/retype on the same node; DeleteUpdate and ContainmentCycle poison the
    // WHOLE key (None = every axis) because no orthogonal edit survives a contested existence or a cycle.
    public Option<string> ConflictAxis => this.Map(
        parallelEdit: static _ => Some("content"), deleteUpdate: static _ => Option<string>.None,
        moveMove: static _ => Some("parent"), reorderReorder: static _ => Some("ordinal"),
        typeChange: static _ => Some("role"), topologyBreak: static _ => Some("content"),
        containmentCycle: static _ => Option<string>.None);

    // The two-sided (Hlc, actor) evidence both sides stamp — derived through the generated Map so the seven
    // near-identical Receipt arms collapse to ONE Project expression; the single-author ContainmentCycle (the
    // cycle is detected on one side) reuses its own cell/actor for both held and incoming, the only case where
    // the two stamps coincide. A TheirCell/TheirActor pair is load-bearing on the six two-sided cases and absent
    // on the cycle case, so the union owns the projection, not seven copy-pasted Receipt calls.
    public (Hlc Held, string HeldActor, Hlc Incoming, string IncomingActor) Evidence => this.Map(
        parallelEdit: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        deleteUpdate: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        moveMove: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        reorderReorder: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        typeChange: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        topologyBreak: static c => (c.OurCell, c.OurActor, c.TheirCell, c.TheirActor),
        containmentCycle: static c => (c.OurCell, c.OurActor, c.OurCell, c.OurActor));
}

// The per-key egress: a Tombstone drops the node, a Members carries the UNTYPED RFC 6902 document whose ops are
// JSON-pointer-pathed — the forest deltas at fixed pointers (`/parent`, `/ordinal`, `/role`), the content deltas
// at the MEMBER-GRANULAR pointers the Generator.Equals Inequalities member diff localizes (a changed
// `Layers/2/Thickness` is one Replace at that pointer, never a whole-node replace). The untyped JsonPatchDocument
// is the primary SystemTextJson surface a CDE/inspector deserializes and ApplyTo-folds break-on-first-error.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record EntityEdit {
    private EntityEdit() { }
    public sealed record Tombstone(NodeId Key) : EntityEdit;
    public sealed record Members(NodeId Key, JsonPatchDocument Patch) : EntityEdit;
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
        HashMap<NodeId, GraphNode> nodes = toHashMap(graph.Nodes.Values.Choose(static n => Optional(n as Node.Object)).Map(o => (o.Id, new GraphNode(
            o.Id,
            NodeRole.Of(o.Kind, containerWholes.Contains(o.Id), !o.Representations.ByIdentifier.IsEmpty),
            parentByKey.Find(o.Id), ordinalByKey.Find(o.Id).IfNone(0),
            GeometryDigest(o.Representations), ContentAddress.Of(o.ToCanonicalBytes(graph.Header.Tolerance).Span).Value, UInt128.Zero,
            childrenByParent.Find(o.Id).IfNone(Seq<NodeId>())))));
        return nodes.Values.Filter(static node => node.Parent.IsNone).Bind(root => Seal(root, nodes));
    }

    // The content axis: the non-Object nodes (Material/PropertySet/QuantitySet/Coverage/Appearance/Assessment) the
    // Object-forest never topologizes, projected as Parent-less GraphNodes carrying only the PropertyHash content
    // signature (GeometryHash 0). The three-way merge diffs these DIRECTLY off the node map so a single-side content
    // edit APPLIES as a Members patch and a both-side content edit is a ParallelEdit — an Object-only forest would
    // silently drop a changed property set / material, the deleted form.
    public static HashMap<NodeId, GraphNode> ContentNodes(ElementGraph graph) =>
        toHashMap(graph.Nodes.Values.Filter(static n => n is not Node.Object).Map(n => (n.Id, new GraphNode(
            n.Id, ContentRole(n), Option<NodeId>.None, 0, UInt128.Zero,
            ContentAddress.Of(n.ToCanonicalBytes(graph.Header.Tolerance).Span).Value, UInt128.Zero, Seq<NodeId>()))));

    public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to) {
        HashMap<NodeId, GraphNode> fromByKey = from.ToHashMap(static n => n.Key);
        HashMap<NodeId, GraphNode> toByKey = to.ToHashMap(static n => n.Key);
        Seq<GraphNode> roots = to.Filter(n => n.Parent.Map(p => !toByKey.ContainsKey(p)).IfNone(true));
        return Walk(roots.IsEmpty ? to : roots, fromByKey, toByKey)
             + from.Filter(n => !toByKey.ContainsKey(n.Key)).Map(static n => (EditOp)new EditOp.Delete(n.Key));
    }

    // The content-axis diff: a flat map-to-map compare keyed on the durable NodeId (NOT the Object forest), emitting
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
        // Two axes per side keyed on the durable NodeId the node map already holds: the Object-forest edit script AND
        // the content-node script, concatenated and grouped by (key, axis). Identity is the NodeId, NEVER recovered by
        // MemberPath-segment archaeology (which depends on the generated [UnorderedEquality] dictionary path-segment
        // shape and is the deleted fragile form) — Patch alone reads the graph Inequalities, and only to localize the
        // member an EntityEdit.Members pointer targets.
        HashMap<NodeId, HashMap<string, EditOp>> ourEdits = ByKeyAxis(Diff(baseForest, ourForest) + DiffContent(baseContent, ourContent));
        HashMap<NodeId, HashMap<string, EditOp>> theirEdits = ByKeyAxis(Diff(baseForest, theirForest) + DiffContent(baseContent, theirContent));
        HashMap<NodeId, GraphNode> oursByKey = ourForest.ToHashMap(static n => n.Key);
        HashMap<NodeId, GraphNode> theirsByKey = theirForest.ToHashMap(static n => n.Key);
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

    // Group the resolved script by target key and project each into one EntityEdit. A group with a Delete is a
    // Tombstone; otherwise a Members carrying the untyped RFC 6902 document — the forest deltas at fixed pointers,
    // a content Update at the MEMBER-GRANULAR pointers the @base→target diff localizes (so the egress replaces a
    // changed `Layers/2/Thickness`, never the whole node). The member pointers come from the ONE GRAPH-level
    // Generator.Equals Inequalities call the page advertises — ElementGraph.EqualityComparer.Default.Inequalities
    // reports `Nodes[id].Layers[2].Thickness`-shaped paths whose leading `Nodes[id]` segment groups the rows by
    // owning NodeId, so the patch reads the VERIFIED ElementGraph [Equatable] surface, never a Node-level
    // EqualityComparer the union root does not declare.
    public static HashMap<NodeId, EntityEdit> Patch(Seq<EditOp> script, ElementGraph @base, ElementGraph target) {
        HashMap<NodeId, Seq<Inequality>> byNode = MemberDiff(@base, target);
        // A group with a Delete — or a key NEITHER graph still holds (a delete-after-insert race) — is the
        // Tombstone: net absence is the truthful egress, never a fabricated payload. Every other group builds
        // a Members document whose Append arms always resolve a REAL seam Node.
        return toHashMap(script.GroupBy(static op => op.Target).Map(group => {
            Option<Node> payload = target.Find(group.Key) | @base.Find(group.Key);
            EntityEdit edit = group.Exists(static op => op is EditOp.Delete)
                ? new EntityEdit.Tombstone(group.Key)
                : payload.Match<EntityEdit>(
                    Some: node => new EntityEdit.Members(group.Key, group.Fold(new JsonPatchDocument(), (doc, op) => Append(doc, op, byNode.Find(group.Key).IfNone(Seq<Inequality>()), node))),
                    None: () => new EntityEdit.Tombstone(group.Key));
            return (group.Key, edit);
        }));
    }

    // The ONE graph-level member diff grouped by owning NodeId: ElementGraph.EqualityComparer.Default.Inequalities
    // reports each changed member at a `Nodes[<NodeId>].<member-path>` path, so the leading `Nodes` dictionary-key
    // segment carries the NodeId a content Update's member-granular pointers belong to (the edge `Edges[i]` rows are
    // a separate diff axis this content-patch grouping ignores).
    static HashMap<NodeId, Seq<Inequality>> MemberDiff(ElementGraph @base, ElementGraph target) =>
        toHashMap(ElementGraph.EqualityComparer.Default.Inequalities(@base, target)
            .Choose(static ineq => OwningNode(ineq.Path).Map(key => (key, ineq)))
            .GroupBy(static row => row.key)
            .Select(static g => (g.Key, toSeq(g.Select(static row => row.ineq)))));

    // The owning NodeId of an Inequality whose path threads the `Nodes` dictionary — the segment immediately after
    // the `Nodes` property segment is the dictionary Key carrying the NodeId; a non-`Nodes` path (an `Edges[i]` or
    // `Header` change) yields None and is not a content-node member patch. A bounded array-pair scan (the
    // EXPRESSION_SPINE named-kernel exemption — a fixed-segment-count path walk, never a domain fold).
    static Option<NodeId> OwningNode(MemberPath path) {
        MemberPathSegment[] segments = path.Segments;
        for (int i = 1; i < segments.Length; i++) {
            if (segments[i - 1].Value is "Nodes" && segments[i].Value is NodeId key) { return Some(key); }
        }
        return None;
    }

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
    // the same remap applies to a freshly-PROJECTED GraphDelta before it commits so the durable stream never
    // forks, and the content signature drives change DETECTION, never cross-ingest identity.
    public static (ElementGraph Aligned, HashMap<NodeId, NodeId> Remap) Reconcile(ElementGraph persisted, ElementGraph ingested) {
        HashMap<string, NodeId> durable = Correlation(persisted);
        HashMap<NodeId, NodeId> remap = toHashMap(toSeq(Correlation(ingested))
            .Choose(pair => durable.Find(pair.Key).Map(id => (Ingest: pair.Value, Durable: id)))
            .Filter(static move => move.Ingest != move.Durable));
        return remap.IsEmpty ? (ingested, remap) : (Reindex(ingested, remap), remap);
    }

    // One correlation map per graph: every rooted GlobalId row plus every UNAMBIGUOUS Type natural-key row — a
    // natural key two Types share on one side drops from correlation (a 1:1 aligner never guesses; the GlobalId
    // row wins a collision via TryAdd).
    static HashMap<string, NodeId> Correlation(ElementGraph graph) {
        HashMap<string, NodeId> rooted = toHashMap(graph.Nodes.Values.Choose(ExternalKey));
        return toSeq(graph.Nodes.Values.Choose(TypeKey).GroupBy(static pair => pair.External).Where(static g => g.Count() == 1))
            .Fold(rooted, static (acc, g) => acc.TryAdd(g.Key, g.First().Id));
    }

    // The 1:1 GlobalId correlation key off a ROOTED node — Some only when the node carries an ExternalId (a rooted
    // IFC node); a from-scratch node carries None and stays NodeId-identified (it lives in one authoring lineage,
    // matched by the durable NodeId, never re-minted by a foreign ingest).
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

    // Rewrite the ingested graph onto the durable ids: re-stamp every node id and re-endpoint every edge through
    // the ingest->durable map (an unmapped id passes through unchanged), then re-freeze the snapshot. Node identity
    // rewrite and edge-endpoint rewrite are the seam-owned `Node.Relabel`/`Relationship.Remap` operations (the
    // Graph/element#NODE_MODEL + Relations/relation#EDGE_ALGEBRA owners) — the seam `Node`/`Relationship` are
    // class-root [Equatable] [Union]s (the GRAPH_FAMILY form, so ElementGraph.Inequalities drills to member
    // granularity) and a class-root union case has NO `with`, so the id/endpoint rewrite is the union's own total-
    // Map reconstruction, NOT a per-case `with` re-spelled in this consumer. Reconcile composes them, never re-deriving them.
    static ElementGraph Reindex(ElementGraph graph, HashMap<NodeId, NodeId> remap) {
        NodeId Resolve(NodeId id) => remap.Find(id).IfNone(id);
        FrozenDictionary<NodeId, Node> nodes = graph.Nodes.Values.Select(node => node.Relabel(Resolve(node.Id))).ToFrozenDictionary(static node => node.Id);
        ImmutableArray<Relationship> edges = [.. graph.Edges.Select(edge => edge.Remap(Resolve))];
        return ElementGraph.Of(graph.Header, nodes, edges);
    }

    // The Object's geometry signature is the FULL Representations.ByIdentifier keyed map (M2: every RepresentationIdentifier
    // the kernel content-keyed — the heavy Body mesh AND the analytical Axis/Box/FootPrint) folded into one digest over a
    // deterministic identifier-sorted preimage, so a structural-line (Axis) or space-boundary (FootPrint) geometry edit
    // surfaces as a geometry divergence the same as a Body edit. Reading only Body would silently miss an analytical change
    // (the deleted thin slice). The digests are kernel `GeometryHash` values minted over the kernel-FROZEN `EncodeForm`
    // byte layouts (`Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE` — IEEE-754-LE, `-0.0`→`+0.0`; this page is that
    // seam's RE-TARGETED consumer), READ not re-minted, and the preimage interleaves each identifier WITH its digest —
    // the identifier keys the representation's EncodeForm lane, so the fold pairs (form lane, digest) and a bare digest
    // never crosses a form boundary. The empty map digests to 0.
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

    // The neutral content-node role — a leaf classification of the non-Object node kinds the content axis carries.
    static NodeRole ContentRole(Node node) => node switch {
        Node.Material => NodeRole.Occurrence,
        Node.Coverage => NodeRole.Occurrence,
        Node.Appearance => NodeRole.Annotation,
        _ => NodeRole.Annotation, // PropertySet/QuantitySet/Assessment — definition facets, never topologized
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

    // The RFC 6902 patch egress — the UNTYPED JsonPatchDocument a CDE/inspector deserializes and ApplyTo-folds. The
    // forest deltas write fixed JSON pointers (`/parent`, `/ordinal`, `/role`); a content Update writes the
    // MEMBER-GRANULAR pointers the graph-level Inequalities rows (already grouped to this node, the leading
    // `Nodes[id]` prefix stripped by Pointer) localize — a changed `/Layers/2/Thickness` is one Replace at that
    // pointer, never a whole-node replace. Every payload the document carries is the REAL seam `Node` resolved
    // from the `target` graph (else `@base` — the last real state): an Insert seeds it at the node-key pointer,
    // and an Update whose member rows did NOT localize (the diff reported the whole node) replaces the WHOLE
    // TARGET NODE at the `""` root pointer — a real, applyable target-state change. The hash-only `GraphNode`
    // diff projection NEVER enters a patch (a `/propertyHash`/`/geometryHash` pointer addresses no member of the
    // target `Node` — the deleted illusory form). Patch resolves the payload before dispatch; absence becomes the
    // group's Tombstone, so this fold has no swallowed `None` arm. A Delete is the separate Tombstone.
    static JsonPatchDocument Append(JsonPatchDocument doc, EditOp op, Seq<Inequality> members, Node payload) =>
        op.Switch<(JsonPatchDocument Doc, Seq<Inequality> Members, Node Payload), JsonPatchDocument>(
            (doc, members, payload),
            match: static (s, _) => s.Doc,
            insert: static (s, i) => s.Doc.Add($"/{i.Node.Key.Value}", Codec(s.Payload)),
            delete: static (s, _) => s.Doc,
            update: static (s, u) => s.Members.IsEmpty
                ? s.Doc.Replace(string.Empty, Codec(s.Payload))
                : s.Members.Fold(s.Doc, static (d, ineq) => d.Replace(Pointer(ineq.Path), Codec(ineq.Right))),
            move: static (s, m) => s.Doc.Replace("/parent", Codec(m.ToParent)),
            reorder: static (s, r) => s.Doc.Replace("/ordinal", Codec(r.ToOrdinal)),
            retype: static (s, r) => s.Doc.Replace("/role", Codec(r.ToRole.Key)));

    // Every patched VALUE lowers through the ONE codec STJ converter graph (`ElementJson.Options` — the same
    // Thinktecture/LanguageExt/NodaTime converter set the codec owner registers once), so an inserted seam
    // `Node` or a replaced `Option`/`Seq`/NodaTime member round-trips; a default-options serialization of a
    // seam value inside the patch document is the deleted form that cannot re-hydrate.
    static JsonElement Codec<T>(T value) => JsonSerializer.SerializeToElement(value, ElementJson.Options);

    // Lower a grouped graph-level MemberPath (`Nodes[<NodeId>].Layers[2].Thickness`) to a node-LOCAL RFC 6902 JSON
    // pointer (`/Layers/2/Thickness`) — skip the leading `Nodes`-property segment and the `NodeId` key segment, then
    // each residual segment is a pointer step, so a member change replaces exactly its pointer within the node.
    static string Pointer(MemberPath path) =>
        toSeq(path.Segments).SkipWhile(static seg => seg.Value is "Nodes" or NodeId).Fold(string.Empty, static (acc, seg) => $"{acc}/{seg.Value}");

    static ConflictReceipt Receipt(NodeId key, ModelId model, ColumnFamily family, Hlc held, string heldActor, Hlc incoming, string incomingActor, Guid correlation, Instant at) =>
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
|  [09]   | patch egress          | untyped `JsonPatchDocument` JSON pointers                                     |
|  [10]   | conflict receipt      | `Version/ledger#MERGE_LAW` `ConflictReceipt`                                  |
|  [11]   | reconciliation seam   | `Rasm/Spatial/reconciliation` `GeometryHash` over frozen `EncodeForm` layouts |
|  [12]   | type correlation      | `TypeKey` classification-excluded `Name`/`Tag` natural key                    |
|  [13]   | patch value codec     | `SerializeToElement` under `ElementJson.Options`                              |

Each row's binding invariant, keyed to its policy:

- [01]-[re-ingest align]: freshly-minted rooted `NodeId` aligned to the durable id on the 1:1 GlobalId; never a `NodeId`-keyed re-ingest diff.
- [02]-[FOREST_TOPOLOGY]: `Forest` derives `Parent`/`Ordinal`/`Children`; no second store.
- [03]-[NODE_ROLE]: never an IFC-class string scan of `Classification.Code`.
- [04]-[GEOMETRY_AXIS]: `Body` + analytical `Axis`/`Box`/`FootPrint`, kernel digests read not re-minted.
- [05]-[CONTENT_AXIS]: a single-side content edit applies; never dropped by the `Object`-only forest.
- [06]-[CONTENT_KEY]: the ONE seam hasher; never a raw second `XxHash128` instance.
- [07]-[SUBTREE_PRUNE]: linear in changed nodes, no `GetHashCode`.
- [08]-[CONFLICT_ACCUMULATION]: one-pass classify, both carried, never first-abort.
- [09]-[PATCH_EGRESS]: member-granular per the graph `Inequalities`; a CDE/inspector reads it; delete is a tombstone.
- [10]-[CONFLICT_RECEIPT]: held/incoming `(Hlc, actor)` from the changefeed.
- [11]-[RECONCILIATION_SEAM]: `GraphNode.GeometryHash` is the RE-TARGETED consumer; the preimage pairs (form lane, digest) — a bare digest never crosses a form boundary.
- [12]-[TYPE_CORRELATION]: a re-keyed GlobalId-less `Type` diffs as RENAME; kernel V8a seed replaces the interim on landing.
- [13]-[PATCH_VALUE_CODEC]: every patched value rides the ONE codec converter graph; default-options serialization deleted.
