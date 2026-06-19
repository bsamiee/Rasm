# [PERSISTENCE_STRUCTURAL_DIFF]

Rasm.Persistence geometry-aware structural diff/merge: a tree-edit-distance node-identity matching engine doing three-way merge and typed conflict classification over the durable object graph. `StructuralMerge` content-addresses canonical adjacency bytes through `GraphNode.GeometryHash` so a moved-control-point morph and a topology break are distinguished by hash; merge receipts (`ConflictReceipt`) arrive settled from the codec profile.

## [01]-[INDEX]

- [01]-[STRUCTURAL_DIFF]: tree-edit node-identity match, three-way merge, and typed conflict classes.

## [02]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` an identity-keyed node in the model graph; `EditOp` `[Union]` the tree-edit-distance operation family; `MergeConflict` `[Union]` the typed conflict-class family; `StructuralMerge` the static surface owning node-identity matching, the cheapest-edit-script tree diff, the three-way merge, and the topological brep/mesh delta classification.
- Cases: `Match | Insert | Delete | Update | Move` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | TypeChange | TopologyBreak` on `MergeConflict`.
- Entry: `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` — the minimal edit script matching nodes by stable identity then by structural signature; `public static (Seq<EditOp> Merged, Seq<MergeConflict> Conflicts) ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs)` computes the base-relative edit scripts on both sides and folds non-overlapping edits while classifying overlaps into typed conflicts.
- Auto: node identity matches first on the stable element id (GUID), then on the geometry-hash and property-hash signatures so a moved-but-unchanged node matches by signature and a transformed node matches by id with a `Update`/`Move` op carrying the transform delta — the tree-edit distance is computed over the matched forest with `Match` for identity-equal nodes, `Update` for same-id-different-signature, `Move` for same-signature-different-parent, and `Insert`/`Delete` for unmatched; the three-way merge diffs base→ours and base→theirs, applies every edit touching a disjoint node set, and folds every edit touching a shared node into a `MergeConflict` whose class names the structural cause; topology delta classification reads the brep face/edge/vertex adjacency or the mesh half-edge adjacency from the geometry-hash signature so a face split or an edge collapse surfaces as a `TopologyBreak` rather than a value change.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count by class, and each `MergeConflict` projects to the settled `ConflictReceipt` so the inspector surface reads one conflict vocabulary.
- Packages: System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new edit op is one `EditOp` case; a new conflict class is one `MergeConflict` case; a new identity signature is one column on `GraphNode`; zero new surface — a line-based text diff, a JSON-patch diff over serialized nodes, or a second merge resolver is the deleted form because the diff matches on geometry/property identity and the merge classifies by structural cause; the existing set-difference `GraphDiff` (closure-membership transfer) stays the transport-level diff while this owns the semantic node-level diff, so the two diffs are altitude-split, never duplicated.
- Boundary: node identity is the GUID stable id first, then the `(GeometryHash, PropertyHash)` content signature so a re-imported model whose ids changed still matches unchanged nodes by signature — a positional or ordinal match is the deleted form; the geometry hash is `XxHash128.HashToUInt128` over the canonical topology encoding read VERBATIM from `Rasm.Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` — the sole owner of the frozen byte layout (`int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding) — so `GeometryHash` content-addresses the IDENTICAL byte stream `NamingHashOps.Encode` emits and NEVER re-derives a second encoding; this is the same canonical adjacency the `Query/cache#ARTIFACT_BLOB_INDEX` `IfcSemantic` and the Compute interchange produce, so a morph (same topology, moved control points) hashes differently from a topology break (changed adjacency), and the diff distinguishes `Update` (signature change at stable id) from `Move` (signature stable, parent change) from `TopologyBreak` (adjacency change); the three-way merge is base-relative — it never compares ours to theirs directly, so a node both sides left untouched never enters a conflict, and a node one side deleted while the other updated is a `DeleteUpdate` conflict the inspector resolves; the tree-edit distance is bounded to the matched forest so the cost is linear in the changed-node count, never quadratic over the whole graph; the diff operands are `GraphNode` projections of the federated entity graph (`Query/federation#ENTITY_GRAPH`) so a structural diff rides the same node identity the federation keys on, and the merge result re-projects as `EditOp` rows applied through the CRDT algebra so a merged structural change converges like any other op.

```csharp signature
public readonly record struct GraphNode(
    Guid Id,
    string Kind,
    Option<Guid> Parent,
    UInt128 GeometryHash,
    UInt128 PropertyHash,
    Seq<Guid> Adjacency);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EditOp {
    private EditOp() { }

    public sealed record Match(Guid Id) : EditOp;
    public sealed record Insert(GraphNode Node) : EditOp;
    public sealed record Delete(Guid Id) : EditOp;
    public sealed record Update(Guid Id, UInt128 FromProperty, UInt128 ToProperty, UInt128 FromGeometry, UInt128 ToGeometry) : EditOp;
    public sealed record Move(Guid Id, Option<Guid> FromParent, Option<Guid> ToParent) : EditOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MergeConflict {
    private MergeConflict() { }

    public sealed record ParallelEdit(Guid Id, EditOp Ours, EditOp Theirs) : MergeConflict;
    public sealed record DeleteUpdate(Guid Id, bool DeletedByOurs) : MergeConflict;
    public sealed record MoveMove(Guid Id, Option<Guid> OurParent, Option<Guid> TheirParent) : MergeConflict;
    public sealed record TypeChange(Guid Id, string OurKind, string TheirKind) : MergeConflict;
    public sealed record TopologyBreak(Guid Id, UInt128 OurGeometry, UInt128 TheirGeometry) : MergeConflict;
}

public static class StructuralMerge {
    public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to) {
        var fromById = from.ToHashMap(static node => node.Id);
        var toById = to.ToHashMap(static node => node.Id);
        var matched = to.Map(node => fromById.Find(node.Id).Match(
            Some: prior => prior.GeometryHash == node.GeometryHash && prior.PropertyHash == node.PropertyHash
                ? prior.Parent == node.Parent ? new EditOp.Match(node.Id) : (EditOp)new EditOp.Move(node.Id, prior.Parent, node.Parent)
                : new EditOp.Update(node.Id, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash),
            None: () => new EditOp.Insert(node)));
        var deletes = from.Filter(node => !toById.ContainsKey(node.Id)).Map(static node => (EditOp)new EditOp.Delete(node.Id));
        return matched + deletes;
    }

    public static (Seq<EditOp> Merged, Seq<MergeConflict> Conflicts) ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs) {
        var ourEdits = Diff(baseGraph, ours).ToHashMap(TargetId);
        var theirEdits = Diff(baseGraph, theirs).ToHashMap(TargetId);
        var conflicts = toSeq(ourEdits.Keys
            .Where(theirEdits.ContainsKey)
            .Choose(id => Classify(id, ourEdits[id], theirEdits[id])));
        var conflicted = conflicts.Map(Subject).ToHashSet();
        var merged = ourEdits
            .Filter((id, _) => !conflicted.Contains(id))
            .Values
            .Append(theirEdits.Filter((id, _) => !ourEdits.ContainsKey(id) && !conflicted.Contains(id)).Values);
        return (toSeq(merged), conflicts);
    }

    public static ConflictReceipt Project(MergeConflict conflict, CorrelationId correlation, Instant at) =>
        new(conflict.GetType().Name, Subject(conflict).ToString(), "structural",
            Instant.MinValue, 0UL, "", at, 0UL, "", correlation, at);

    private static Guid TargetId(EditOp op) =>
        op switch {
            EditOp.Match m => m.Id, EditOp.Insert i => i.Node.Id, EditOp.Delete d => d.Id,
            EditOp.Update u => u.Id, EditOp.Move v => v.Id, _ => Guid.Empty,
        };

    private static Guid Subject(MergeConflict c) =>
        c switch {
            MergeConflict.ParallelEdit p => p.Id, MergeConflict.DeleteUpdate d => d.Id,
            MergeConflict.MoveMove m => m.Id, MergeConflict.TypeChange t => t.Id,
            MergeConflict.TopologyBreak b => b.Id, _ => Guid.Empty,
        };

    private static Option<MergeConflict> Classify(Guid id, EditOp ours, EditOp theirs) =>
        (ours, theirs) switch {
            (EditOp.Match, EditOp.Match) => None,
            (EditOp.Delete, EditOp.Update) => Some<MergeConflict>(new MergeConflict.DeleteUpdate(id, true)),
            (EditOp.Update, EditOp.Delete) => Some<MergeConflict>(new MergeConflict.DeleteUpdate(id, false)),
            (EditOp.Move m1, EditOp.Move m2) when m1.ToParent != m2.ToParent => Some<MergeConflict>(new MergeConflict.MoveMove(id, m1.ToParent, m2.ToParent)),
            (EditOp.Update u1, EditOp.Update u2) when u1.ToGeometry != u2.ToGeometry => Some<MergeConflict>(new MergeConflict.TopologyBreak(id, u1.ToGeometry, u2.ToGeometry)),
            (EditOp.Update u1, EditOp.Update u2) when u1.ToProperty != u2.ToProperty => Some<MergeConflict>(new MergeConflict.ParallelEdit(id, u1, u2)),
            _ => None,
        };
}
```


## [03]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] — `StructuralMerge` `GraphNode.GeometryHash` content-addresses the canonical adjacency bytes whose FROZEN field order is owned by `Rasm.Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` — `int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding — read VERBATIM before `XxHash128.HashToUInt128`, never a second divergent encoding. The shared FROZEN golden-bytes fixture both packages assert against is the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte canonical stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00` and whose `XxHash128` digest is `0x9462A71A5DD13DCFA3B1D6D225FCBE70` (16-byte LE `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`); a tier-2 cross-package byte-equality harness feeds this reference through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path and asserts the stream bytes AND the digest, with the morph case (moved control points, same adjacency → equal hash) and the topology-break case (changed adjacency → distinct hash) as the two discriminating laws.
