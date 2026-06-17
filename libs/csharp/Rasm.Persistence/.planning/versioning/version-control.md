# [PERSISTENCE_VERSION_CONTROL]

Rasm.Persistence owns Git-grade version control over the durable object graph: a content-addressed commit-DAG with named branches, lightweight commits, and merge-base computation; a convergent op-based/delta-state CRDT replacing the LWW `Adjudicate` scalar with RGA sequence, add-wins observed-remove set, multi-value register, PN-counter, and LWW-by-HLC register types over the parametric DAG; an HLC `Hlc` stamp that is the one causal-ordering primitive shared by the op-log, the CRDT merge, and the wire seam; a `CrdtOpWire` op/CRDT encoding (HLC cell, op kinds, causal metadata) that amends the one-wire-vocabulary law field-for-field across the version-control owner and the AppHost wire seam; a version-vector concurrency detector plus a Merkle-DAG range-reconciliation handshake for anti-entropy; an AS-OF time-travel engine reconstructing, diffing, blaming, scrubbing, checkpointing, and branching-from-past over the HLC op-log and the content-addressed snapshots; and a geometry-aware structural diff/merge engine doing tree-edit-distance node-identity matching, three-way merge, and typed conflict classification. The op-log (`OpLogEntry`, HLC stamp, `Closure` manifest), the content-addressed snapshot identity (`Snapshots.ContentAddress`, `XxHash128`), the MessagePack codec profile (`ThinktectureMessageFormatterResolver`, `GeneratedMessagePackResolver`, `Lz4BlockArray`, `UntrustedData` restore lane), and the merge receipts (`ConflictReceipt`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive from AppHost.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              |
| :-----: | :--------------- | :----------------------------------------------------------------- |
|   [1]   | COMMIT_DAG       | Content-addressed commit-DAG, named branches, merge-base, vectors  |
|   [2]   | CRDT_ALGEBRA     | RGA / OR-set / MV-register / PN-counter / LWW convergent CRDT       |
|   [3]   | CRDT_WIRE        | HLC stamp, `CrdtOp` codec, `CrdtOpWire` op-log payload amendment    |
|   [4]   | TIME_TRAVEL      | AS-OF reconstruction, checkpoint, range diff, blame, scrub, branch-from-past |
|   [5]   | STRUCTURAL_DIFF  | Tree-edit node-identity match, three-way merge, typed conflict classes |
|   [6]   | TS_PROJECTION    | Commit, branch, version-vector, op, conflict, blame, Merkle wire shapes |

## [2]-[COMMIT_DAG]

- Owner: `CommitNode` content-addressed commit record; `BranchRef` named-branch pointer with per-branch ACL; `VersionVector` per-origin sequence map; `MerkleRange` reconciliation node; `CommitGraph` static surface owning hash, parent-link, merge-base, vector-compare, Merkle range-fold, and the recursive anti-entropy descent.
- Cases: `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` folds a content-key range into one `XxHash128` digest over its sorted children so a peer compares one digest before descending, and `CommitGraph.Reconcile` recursively bisects only divergent subranges.
- Entry: `public static CommitNode Commit(VersionVector parents, Seq<UInt128> opKeys, BranchRef branch, string actor, Hlc cell)` — pure value; the commit content key is `XxHash128` over the canonical parent-set, op-key-set, and HLC cell, so an identical commit on two peers shares one identity; `public static Option<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right)` walks both parent closures to the lowest common ancestor; `public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote)` returns the divergent leaf ranges to transfer.
- Auto: a commit appends one `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload so the commit-DAG rides the one op-log changefeed, never a second store; the `VersionVector` advances the committing origin's slot by the committed op count so concurrency detection reads one vector per commit; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares digests top-down and transfers only the divergent subtree through the settled `SyncPump.GraphDiff` set-difference.
- Receipt: a commit rides `ReceiptSinkPort` under the `store.commit` kind; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides the settled `SyncApplyReceipt`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind (tag, remote-tracking) is one `RefKind` row; a new ACL grant is one `BranchAcl` flag; a new reconciliation fan-out is one policy value on `CommitGraph.Fanout`; zero new surface — a parallel commit store, a second DAG walker, or a `git`-shaped object database is the deleted form because the commit rides the op-log and the content address rides `Snapshots.ContentAddress`.
- Boundary: the commit content key derives from `XxHash128` over the canonical `(SortedParents, SortedOpKeys, Hlc)` tuple so two peers minting the same logical commit converge on one node — a wall-clock or random commit id is the deleted form; `MergeBase` walks the parent closure breadth-first over the `resolve` delegate and returns `None` for disjoint histories so a cross-document merge surfaces a typed absence rather than a false ancestor; the `VersionVector` is the one concurrency primitive — `Order` returns `Concurrent` exactly when neither vector dominates, and a merge commit carries both parents so the vector join is the per-slot max; `BranchAcl` is a per-branch capability flag set (`Read | Write | Merge | Rebase | ForcePush | Admin`) gated at the write path, never a second authz taxonomy — object-level grants ride the AppHost identity seam and this flag set scopes the branch ref; tags are immutable `BranchRef` rows whose `Kind` is `Tag` and whose `Acl` denies `Write`; `MerkleRange` is the anti-entropy accelerator — a peer exchanges one root digest, `Reconcile` descends only divergent ranges, and the leaf transfer rides `SyncPump.GraphDiff`, so a full op-log replay for reconciliation is the deleted form.

```csharp signature
public sealed class VersionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<VersionKeyPolicy, string>]
[KeyMemberComparer<VersionKeyPolicy, string>]
public sealed partial class RefKind {
    public static readonly RefKind Branch = new("branch");
    public static readonly RefKind Tag = new("tag");
    public static readonly RefKind RemoteTracking = new("remote-tracking");
}

[Flags]
public enum BranchAcl {
    None = 0,
    Read = 1,
    Write = 2,
    Merge = 4,
    Rebase = 8,
    ForcePush = 16,
    Admin = 32,
}

[SmartEnum]
public sealed partial class VectorOrder {
    public static readonly VectorOrder Before = new();
    public static readonly VectorOrder After = new();
    public static readonly VectorOrder Concurrent = new();
    public static readonly VectorOrder Equal = new();
}

public readonly record struct VersionVector(HashMap<Guid, long> Slots) {
    public static readonly VersionVector Empty = new(HashMap<Guid, long>());

    public VersionVector Advance(Guid origin, long count) =>
        new(Slots.AddOrUpdate(origin, existing => existing + count, count));

    public VersionVector Join(VersionVector other) =>
        new(other.Slots.Fold(Slots, static (acc, slot) =>
            acc.AddOrUpdate(slot.Key, existing => long.Max(existing, slot.Value), slot.Value)));

    public bool Dominates(VersionVector other) =>
        other.Slots.ForAll(slot => Slots.Find(slot.Key).IfNone(0L) >= slot.Value);

    public long At(Guid origin) => Slots.Find(origin).IfNone(0L);
}

public sealed record BranchRef(string Name, RefKind Kind, UInt128 Head, BranchAcl Acl, Guid Origin, Instant At) {
    public bool Permits(BranchAcl grant) => (Acl & grant) == grant;
}

public readonly record struct CommitNode(
    UInt128 ContentKey,
    Seq<UInt128> Parents,
    Seq<UInt128> OpKeys,
    string Branch,
    VersionVector Vector,
    string Actor,
    Hlc Cell);

public readonly record struct MerkleRange(UInt128 Low, UInt128 High, UInt128 Digest, int Count);

public static class CommitGraph {
    public const int Fanout = 16;

    public static CommitNode Commit(VersionVector parents, Seq<UInt128> opKeys, BranchRef branch, string actor, Hlc cell) {
        var advanced = parents.Advance(branch.Origin, opKeys.Count);
        var parentSet = branch.Head == default ? Seq<UInt128>() : Seq(branch.Head);
        var canonical = new ArrayBufferWriter<byte>();
        foreach (var parent in parentSet.OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], parent);
        canonical.Advance(parentSet.Count * 16);
        foreach (var key in opKeys.OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], key);
        canonical.Advance(opKeys.Count * 16);
        cell.WriteTo(canonical);
        return new CommitNode(
            XxHash128.HashToUInt128(canonical.WrittenSpan),
            parentSet, opKeys, branch.Name, advanced, actor, cell);
    }

    public static VectorOrder Order(VersionVector left, VersionVector right) =>
        (left.Slots.Equals(right.Slots), left.Dominates(right), right.Dominates(left)) switch {
            (true, _, _) => VectorOrder.Equal,
            (_, true, false) => VectorOrder.After,
            (_, false, true) => VectorOrder.Before,
            _ => VectorOrder.Concurrent,
        };

    public static Option<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right) {
        var leftClosure = Ancestry(resolve, left);
        return Bfs(resolve, right).Find(leftClosure.Contains);
    }

    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in sortedKeys)
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], key);
        buffer.Advance(sortedKeys.Count * 16);
        return new MerkleRange(
            sortedKeys.HeadOrNone().IfNone(UInt128.Zero),
            sortedKeys.LastOrNone().IfNone(UInt128.Zero),
            XxHash128.HashToUInt128(buffer.WrittenSpan),
            sortedKeys.Count);
    }

    public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        local.Digest == remote.Digest
            ? Seq<MerkleRange>()
            : remote.Count <= Fanout
                ? Seq(remote)
                : children(remote)
                    .Filter(child => Overlaps(local, child))
                    .Bind(child => Reconcile(children, Lookup(children, local, child), child))
                    .Concat(children(remote).Filter(child => !Overlaps(local, child)));

    private static bool Overlaps(MerkleRange a, MerkleRange b) => a.Low <= b.High && b.Low <= a.High;

    private static MerkleRange Lookup(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        children(local).Find(child => Overlaps(child, remote)).IfNone(new MerkleRange(remote.Low, remote.High, UInt128.Zero, 0));

    private static HashSet<UInt128> Ancestry(Func<UInt128, Option<CommitNode>> resolve, UInt128 root) =>
        Bfs(resolve, root).ToHashSet();

    private static Seq<UInt128> Bfs(Func<UInt128, Option<CommitNode>> resolve, UInt128 root) {
        var seen = new System.Collections.Generic.HashSet<UInt128>();
        var queue = new System.Collections.Generic.Queue<UInt128>([root]);
        var order = Seq<UInt128>();
        while (queue.TryDequeue(out var key))
            if (seen.Add(key)) {
                order = order.Add(key);
                resolve(key).Iter(node => node.Parents.Iter(queue.Enqueue));
            }
        return order;
    }
}
```

| [INDEX] | [POLICY]                | [VALUE]                          | [BINDING]                                         |
| :-----: | :---------------------- | :------------------------------- | :------------------------------------------------ |
|   [1]   | commit column family    | `commit`                         | one `OpLogEntry` per commit on the changefeed     |
|   [2]   | branch column family    | `branch`                         | one `OpLogEntry` per ref mutation                 |
|   [3]   | merkle fan-out          | 16 children per range            | `CommitGraph.Fanout`                              |
|   [4]   | merge-commit parentage  | two parents, vector join         | `VersionVector.Join` is the per-slot max          |

## [3]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` — the convergent op-based/delta-state field family carrying the five replicated data types; `CrdtOp` the delta payload an `OpLogEntry` carries; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, plus the version-vector-gated tombstone compaction.
- Cases: `LwwRegister` (last-write-wins-by-HLC scalar), `MvRegister` (concurrent-keep multi-value register), `OrSet` (add-wins observed-remove set with per-element unique tags), `PnCounter` (positive-negative per-origin counter), `RgaSequence` (replicated growable array with tombstone-stable ordering) on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` — the join-semilattice least-upper-bound over two field states, total over the five cases and idempotent so replaying the same op converges; `public static CrdtField Apply(CrdtField state, CrdtOp op)` folds one delta op into the state carrying its HLC cell; `public static CrdtField Compact(CrdtField state, VersionVector quiescent)` reclaims tombstones the quiescence horizon proves every peer has observed.
- Auto: a CRDT mutation appends one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the existing changefeed and the existing `Closure` content-key manifest, and a peer's `SyncMerge.Apply` dispatches the `column-family=crdt` op-log row into `Crdt.Apply` rather than the LWW `Adjudicate` scalar — the op-based CRDT supersedes LWW so a concurrent edit converges by merge rather than discarding the loser; the OR-set tags are `(Guid Origin, ulong Logical)` HLC pairs so an add and a concurrent remove of the same element resolve add-wins by tag-set difference; the RGA insert position is the causal predecessor element id plus the inserting origin's HLC so two concurrent inserts after the same predecessor order deterministically by `(Logical, Origin)`; the PN-counter folds per-origin increment maps so a counter converges by per-origin max of monotone partial counts; the MV-register keeps every causally-concurrent write and collapses a dominated write so a later causal write supersedes but a concurrent pair survives for caller resolution.
- Receipt: a converged merge rides the settled `SyncApplyReceipt`; a tombstone count, a live-element count, and a compacted-tombstone count fold into the `store.crdt.merge` fact on the interceptor stream.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm; zero new surface — a per-type merge service, a second convergence engine, or an op-transform (OT) rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — this is the strict superset of the `collaboration#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries tombstones so a deleted element's position stays stable for later concurrent inserts and `Compact` reclaims a tombstone only when the supplied `VersionVector` quiescence horizon dominates the tombstone's HLC for every peer — the [CRDT_COMPACTION] proof harness replays permuted op multisets and confirms the LUB holds under tombstone removal; the `OrSet` is add-wins so a concurrent add-remove keeps the element and a remove only erases observed add-tags, never a later add; the `MvRegister` is a causal anti-chain — a write supersedes only writes its `VersionVector` dominates and keeps every concurrent value, so the register never silently discards a divergent edit; the `PnCounter` is two grow-only per-origin maps whose value is positive-minus-negative, monotone under merge so no decrement is ever lost; the RGA element id is `(Guid Origin, ulong Logical)` so identity is HLC-stable across peers and never positional; `Crdt.Merge` reads no wall clock — the HLC `Hlc` cell from the op-log stamp is the only ordering input, so convergence is deterministic; the CROSS_PACKAGE_LAW amendment this carries is owned at `#CRDT_WIRE`.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public int CompareTo(ElementId other) =>
        Logical.CompareTo(other.Logical) is var byLogical && byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtOp {
    private CrdtOp() { }

    public sealed record Set(string Field, ReadOnlyMemory<byte> Value, Hlc Cell, Guid Origin) : CrdtOp;
    public sealed record Write(string Field, ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell, Guid Origin) : CrdtOp;
    public sealed record Add(string Field, UInt128 Element, ElementId Tag) : CrdtOp;
    public sealed record Remove(string Field, UInt128 Element, Seq<ElementId> ObservedTags) : CrdtOp;
    public sealed record Increment(string Field, Guid Origin, long Delta) : CrdtOp;
    public sealed record InsertAfter(string Field, ElementId Predecessor, ElementId Id, ReadOnlyMemory<byte> Value) : CrdtOp;
    public sealed record Delete(string Field, ElementId Id) : CrdtOp;
    public sealed record Maintain(string Field, VersionVector Quiescent) : CrdtOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtField {
    private CrdtField() { }

    public sealed record LwwRegister(ReadOnlyMemory<byte> Value, Hlc Cell, Guid Origin) : CrdtField;
    public sealed record MvRegister(Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> Values) : CrdtField;
    public sealed record OrSet(HashMap<UInt128, Set<ElementId>> Live, Set<ElementId> Tombstoned) : CrdtField;
    public sealed record PnCounter(HashMap<Guid, long> Positive, HashMap<Guid, long> Negative) : CrdtField;
    public sealed record RgaSequence(Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone, Hlc Cell)> Cells) : CrdtField;
}

public static class Crdt {
    public static CrdtField Merge(CrdtField left, CrdtField right) =>
        (left, right) switch {
            (CrdtField.LwwRegister l, CrdtField.LwwRegister r) =>
                (r.Cell, r.Origin).CompareTo((l.Cell, l.Origin)) > 0 ? r : l,
            (CrdtField.MvRegister l, CrdtField.MvRegister r) => new CrdtField.MvRegister(AntiChain(l.Values + r.Values)),
            (CrdtField.OrSet l, CrdtField.OrSet r) => new CrdtField.OrSet(
                r.Live.Fold(l.Live, static (acc, slot) =>
                    acc.AddOrUpdate(slot.Key, existing => existing.Union(slot.Value), slot.Value))
                    .Map(static (_, tags) => tags) is var union
                    ? union.Map((element, tags) => tags.Remove(l.Tombstoned.Union(r.Tombstoned))).Filter(static tags => tags.Count > 0)
                    : union,
                l.Tombstoned.Union(r.Tombstoned)),
            (CrdtField.PnCounter l, CrdtField.PnCounter r) => new CrdtField.PnCounter(
                MergeMax(l.Positive, r.Positive), MergeMax(l.Negative, r.Negative)),
            (CrdtField.RgaSequence l, CrdtField.RgaSequence r) => new CrdtField.RgaSequence(Weave(l.Cells, r.Cells)),
            _ => left,
        };

    public static CrdtField Apply(CrdtField state, CrdtOp op) =>
        (state, op) switch {
            (CrdtField.LwwRegister reg, CrdtOp.Set set) =>
                (set.Cell, set.Origin).CompareTo((reg.Cell, reg.Origin)) > 0
                    ? new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin) : reg,
            (CrdtField.MvRegister mv, CrdtOp.Write write) => new CrdtField.MvRegister(
                AntiChain(mv.Values.Filter(held => !write.Context.Dominates(held.Context)).Add((write.Value, write.Context, write.Cell)))),
            (CrdtField.OrSet s, CrdtOp.Add add) => new CrdtField.OrSet(
                s.Live.AddOrUpdate(add.Element, existing => existing.Add(add.Tag), Set(add.Tag)), s.Tombstoned),
            (CrdtField.OrSet s, CrdtOp.Remove rem) => new CrdtField.OrSet(
                s.Live.AddOrUpdate(rem.Element, existing => existing.Remove(toSet(rem.ObservedTags)), Set<ElementId>())
                    .Filter(static tags => tags.Count > 0),
                s.Tombstoned.Union(toSet(rem.ObservedTags))),
            (CrdtField.PnCounter c, CrdtOp.Increment inc) when inc.Delta >= 0 => new CrdtField.PnCounter(
                c.Positive.AddOrUpdate(inc.Origin, held => held + inc.Delta, inc.Delta), c.Negative),
            (CrdtField.PnCounter c, CrdtOp.Increment dec) => new CrdtField.PnCounter(
                c.Positive, c.Negative.AddOrUpdate(dec.Origin, held => held - dec.Delta, -dec.Delta)),
            (CrdtField.RgaSequence seq, CrdtOp.InsertAfter ins) => new CrdtField.RgaSequence(
                Weave(seq.Cells, Seq((ins.Id, ins.After, ins.Value, false, new Hlc(default, ins.Id.Logical))))),
            (CrdtField.RgaSequence seq, CrdtOp.Delete del) => new CrdtField.RgaSequence(
                seq.Cells.Map(cell => cell.Id == del.Id ? cell with { Tombstone = true } : cell)),
            (CrdtField.RgaSequence seq, CrdtOp.Maintain m) => Compact(seq, m.Quiescent),
            _ => state,
        };

    public static CrdtField Compact(CrdtField state, VersionVector quiescent) =>
        state is CrdtField.RgaSequence seq
            ? new CrdtField.RgaSequence(seq.Cells.Filter(cell =>
                !cell.Tombstone || quiescent.At(cell.Id.Origin) < (long)cell.Id.Logical))
            : state;

    public static long Value(CrdtField.PnCounter counter) =>
        counter.Positive.Values.Sum() - counter.Negative.Values.Sum();

    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence seq) =>
        seq.Cells.Filter(static cell => !cell.Tombstone).Map(static cell => cell.Value);

    private static HashMap<Guid, long> MergeMax(HashMap<Guid, long> left, HashMap<Guid, long> right) =>
        right.Fold(left, static (acc, slot) => acc.AddOrUpdate(slot.Key, held => long.Max(held, slot.Value), slot.Value));

    private static Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> AntiChain(
        Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> values) =>
        values.Filter(candidate => !values.Exists(other => other.Cell != candidate.Cell && other.Context.Dominates(candidate.Context)));

    private static Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone, Hlc Cell)> Weave(
        Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone, Hlc Cell)> left,
        Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone, Hlc Cell)> right) =>
        toSeq((left + right)
            .GroupBy(static cell => cell.Id)
            .Select(static group => group.Aggregate(static (a, b) => a with { Tombstone = a.Tombstone || b.Tombstone }))
            .OrderByDescending(static cell => cell.After)
            .ThenByDescending(static cell => cell.Id));
}
```

| [INDEX] | [TYPE]        | [CRDT_CLASS]                        | [CONVERGENCE]                                  |
| :-----: | :------------ | :---------------------------------- | :--------------------------------------------- |
|   [1]   | LwwRegister   | last-write-wins by (HLC, origin)    | total order on the stamp tuple; superset of `Adjudicate` |
|   [2]   | MvRegister    | multi-value concurrent-keep         | causal anti-chain; dominated writes collapse   |
|   [3]   | OrSet         | add-wins observed-remove set        | per-element tag-set union minus observed removes |
|   [4]   | PnCounter     | positive-negative per-origin        | per-origin max of monotone partial counts      |
|   [5]   | RgaSequence   | replicated growable array           | tombstone-stable causal order; `Compact` reclaims at quiescence |

## [4]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp value — the one causal-ordering primitive the op-log stamp, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op/CRDT encoding the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `CrdtWire` the static codec surface owning the byte-canonical content key, the `Encode`/`Decode` pair through the settled `ThinktectureMessageFormatterResolver`+`GeneratedMessagePackResolver` chain, and the `UntrustedData` restore-lane decode.
- Cases: 8 op rows — `set | write | add | remove | increment | insertAfter | delete | maintain` on `CrdtOpWire`; the `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned.
- Entry: `public static UInt128 ContentKey(CrdtOpWire op)` — the byte-canonical content key over the MessagePack-encoded delta so an identical op on two peers shares one identity; `public static ReadOnlyMemory<byte> Encode(CrdtOp op)` writes the delta through the version-control resolver under `Lz4BlockArray`; `public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload)` reads the delta under `MessagePackSecurity.UntrustedData` with the depth ceiling because a synced payload crossed a rest boundary.
- Auto: `Hlc.Advance` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter — the same `ReceiptSinkPort.Advance` algebra the op-log stamp and every receipt envelope ride; `CrdtWire.Encode` rides the durability codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes that the restore ladder and the snapshot codec already verify, never a second framing; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row plus one `CrdtOp` arm plus one map case, zero schema fork.
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the codec, content key, and HLC cell); a decode failure folds into the `store.crdt.decode` fault on the interceptor stream as a typed contract-drift rejection.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag plus one `[Key]` member plus one `Map`/`Lift` arm; a retired tag is never reassigned because reuse silently re-types history; zero new surface — a typeless payload, a JSON-array delta, or a per-type formatter beside the resolver chain is the deleted form.
- Boundary: this is the FLAGSHIP CrdtOpWire amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` now carries a `CrdtOpWire` discriminated union for `column-family=crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `AppHost/runtime-ports#WIRE_LAW` with the TS-web `wire-consumption` leg and the Python `runtime/ServerHost` companion decoding the amended payload — recorded as a seam-split at the suite CROSS_PACKAGE_LAWS, never an additive parallel surface; the `Hlc` is one packed `(Instant Physical, ulong Logical)` whose ordering is `Physical` then `Logical` so two peers compare causality without a wall clock, and `WriteTo` emits the canonical 16-byte cell the commit content key and the op content key both hash; the wire `[Key]` sequence and the `[MessagePack.Union]` tags obey the durability retirement law so contract drift is a build diagnostic through the `MessagePackAnalyzer`, never a first-restore discovery; the restore lane reads under `UntrustedData` plus the object-graph depth ceiling because a synced delta's provenance is unprovable, while the write lane keeps the trusted default; `ContentKey` hashes the encoded canonical bytes so the op content key the `OpLogEntry` carries is reproducible across peers and is the same `XxHash128` identity the structural diff and the federation keys consume.

```csharp signature
public readonly record struct Hlc(Instant Physical, ulong Logical) : IComparable<Hlc> {
    public static readonly Hlc Zero = new(Instant.MinValue, 0UL);

    public int CompareTo(Hlc other) =>
        Physical.CompareTo(other.Physical) is var byPhysical && byPhysical != 0 ? byPhysical : Logical.CompareTo(other.Logical);

    public Hlc Advance(Instant wall) =>
        wall > Physical ? new Hlc(wall, 0UL) : new Hlc(Physical, Logical + 1UL);

    public Hlc Observe(Hlc remote, Instant wall) =>
        (Instant.Max(Instant.Max(Physical, remote.Physical), wall)) is var lead
            ? new Hlc(lead, lead == Physical && lead == remote.Physical ? ulong.Max(Logical, remote.Logical) + 1UL
                : lead == Physical ? Logical + 1UL
                : lead == remote.Physical ? remote.Logical + 1UL : 0UL)
            : this;

    public void WriteTo(IBufferWriter<byte> sink) {
        var span = sink.GetSpan(16);
        BinaryPrimitives.WriteInt64LittleEndian(span, Physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(span[8..], Logical);
        sink.Advance(16);
    }
}

[MessagePack.Union(0, typeof(Set))]
[MessagePack.Union(1, typeof(Write))]
[MessagePack.Union(2, typeof(Add))]
[MessagePack.Union(3, typeof(Remove))]
[MessagePack.Union(4, typeof(Increment))]
[MessagePack.Union(5, typeof(InsertAfter))]
[MessagePack.Union(6, typeof(Delete))]
[MessagePack.Union(7, typeof(Maintain))]
public abstract record CrdtOpWire {
    [MessagePackObject]
    public sealed record Set([property: Key(0)] string Field, [property: Key(1)] ReadOnlyMemory<byte> Value, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical, [property: Key(4)] Guid Origin) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Write([property: Key(0)] string Field, [property: Key(1)] ReadOnlyMemory<byte> Value, [property: Key(2)] (Guid Origin, long Seq)[] Context, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical, [property: Key(5)] Guid Origin) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Add([property: Key(0)] string Field, [property: Key(1)] UInt128 Element, [property: Key(2)] Guid TagOrigin, [property: Key(3)] ulong TagLogical) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Remove([property: Key(0)] string Field, [property: Key(1)] UInt128 Element, [property: Key(2)] (Guid Origin, ulong Logical)[] ObservedTags) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Increment([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long Delta) : CrdtOpWire;
    [MessagePackObject]
    public sealed record InsertAfter([property: Key(0)] string Field, [property: Key(1)] Guid PredOrigin, [property: Key(2)] ulong PredLogical, [property: Key(3)] Guid IdOrigin, [property: Key(4)] ulong IdLogical, [property: Key(5)] ReadOnlyMemory<byte> Value) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Delete([property: Key(0)] string Field, [property: Key(1)] Guid IdOrigin, [property: Key(2)] ulong IdLogical) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Maintain([property: Key(0)] string Field, [property: Key(1)] (Guid Origin, long Seq)[] Quiescent) : CrdtOpWire;
}

[GeneratedMessagePackResolver]
public sealed partial class CrdtWireResolver;

public static class CrdtWire {
    private static readonly MessagePackSerializerOptions Write = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(
            ThinktectureMessageFormatterResolver.Instance,
            CrdtWireResolver.Instance,
            StandardResolver.Instance))
        .WithCompression(MessagePackCompression.Lz4BlockArray);

    private static readonly MessagePackSerializerOptions Restore =
        Write.WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(64));

    public static CrdtOpWire Lift(CrdtOp op) =>
        op switch {
            CrdtOp.Set s => new CrdtOpWire.Set(s.Field, s.Value, s.Cell.Physical.ToUnixTimeTicks(), s.Cell.Logical, s.Origin),
            CrdtOp.Write w => new CrdtOpWire.Write(w.Field, w.Value, [.. w.Context.Slots.Map(static (k, v) => (k, v))], w.Cell.Physical.ToUnixTimeTicks(), w.Cell.Logical, w.Origin),
            CrdtOp.Add a => new CrdtOpWire.Add(a.Field, a.Element, a.Tag.Origin, a.Tag.Logical),
            CrdtOp.Remove r => new CrdtOpWire.Remove(r.Field, r.Element, [.. r.ObservedTags.Map(static t => (t.Origin, t.Logical))]),
            CrdtOp.Increment i => new CrdtOpWire.Increment(i.Field, i.Origin, i.Delta),
            CrdtOp.InsertAfter ins => new CrdtOpWire.InsertAfter(ins.Field, ins.Predecessor.Origin, ins.Predecessor.Logical, ins.Id.Origin, ins.Id.Logical, ins.Value),
            CrdtOp.Delete d => new CrdtOpWire.Delete(d.Field, d.Id.Origin, d.Id.Logical),
            CrdtOp.Maintain m => new CrdtOpWire.Maintain(m.Field, [.. m.Quiescent.Slots.Map(static (k, v) => (k, v))]),
            _ => throw new MessagePack.MessagePackSerializationException("<crdt-op-unmapped>"),
        };

    public static CrdtOp Map(CrdtOpWire op) =>
        op switch {
            CrdtOpWire.Set s => new CrdtOp.Set(s.Field, s.Value, new Hlc(Instant.FromUnixTimeTicks(s.PhysicalTicks), s.Logical), s.Origin),
            CrdtOpWire.Write w => new CrdtOp.Write(w.Field, w.Value, new VersionVector(toHashMap(w.Context)), new Hlc(Instant.FromUnixTimeTicks(w.PhysicalTicks), w.Logical), w.Origin),
            CrdtOpWire.Add a => new CrdtOp.Add(a.Field, a.Element, new ElementId(a.TagOrigin, a.TagLogical)),
            CrdtOpWire.Remove r => new CrdtOp.Remove(r.Field, r.Element, toSeq(r.ObservedTags).Map(static t => new ElementId(t.Origin, t.Logical))),
            CrdtOpWire.Increment i => new CrdtOp.Increment(i.Field, i.Origin, i.Delta),
            CrdtOpWire.InsertAfter ins => new CrdtOp.InsertAfter(ins.Field, new ElementId(ins.PredOrigin, ins.PredLogical), new ElementId(ins.IdOrigin, ins.IdLogical), ins.Value),
            CrdtOpWire.Delete d => new CrdtOp.Delete(d.Field, new ElementId(d.IdOrigin, d.IdLogical)),
            CrdtOpWire.Maintain m => new CrdtOp.Maintain(m.Field, new VersionVector(toHashMap(m.Quiescent))),
            _ => throw new MessagePack.MessagePackSerializationException("<crdt-wire-unmapped>"),
        };

    public static ReadOnlyMemory<byte> Encode(CrdtOp op) =>
        MessagePackSerializer.Serialize(Lift(op), Write);

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        Try.lift(() => Map(MessagePackSerializer.Deserialize<CrdtOpWire>(payload, Restore)))
            .Run()
            .MapFail(static error => Error.New(8261, $"<crdt-decode-drift:{error.Message}>"));

    public static UInt128 ContentKey(CrdtOpWire op) =>
        XxHash128.HashToUInt128(MessagePackSerializer.Serialize(op, Write).Span);
}
```

| [INDEX] | [POLICY]               | [VALUE]                          | [BINDING]                                            |
| :-----: | :--------------------- | :------------------------------- | :--------------------------------------------------- |
|   [1]   | crdt column family     | `crdt`                           | `OpLogEntry.Payload` carries `CrdtOpWire`            |
|   [2]   | wire-law owner         | `AppHost/runtime-ports#WIRE_LAW` | breaking descriptor change; LWW survives as `set`    |
|   [3]   | restore decode lane    | `UntrustedData` depth 64         | synced delta crossed a rest boundary                 |
|   [4]   | op content key         | `XxHash128` over canonical bytes | reproducible across peers; one identity              |

## [5]-[TIME_TRAVEL]

- Owner: `AsOfQuery` reconstruction request; `Checkpoint` a sealed materialized-state fold anchor; `RangeDiff` two-instant delta record; `BlameRow` per-node authorship attribution; `ScrubFrame` a replay frame; `TimeTravel` the static surface owning AS-OF materialization, checkpoint sealing, range diff, blame fold, scrub iteration, and branch-from-past.
- Cases: `AsOfQuery` reconstructs at an `Instant` by folding every op whose HLC `Physical` precedes the cut; `Checkpoint` seals the materialized state content address at a commit boundary so a deep history folds from the nearest checkpoint; `RangeDiff` projects added/removed/changed entity keys between two cuts; `BlameRow` attributes the surviving value of one entity-key column-family to the winning `OpLogEntry`; `ScrubFrame` is one step in the ordered replay sequence.
- Entry: `public static IO<HashMap<string, CrdtField>> Reconstruct(AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest)` — `IO` carries the op-log read; the fold replays the prefix forward from the nearest checkpoint through `Crdt.Apply` into the as-of materialized state; `public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, BranchAcl acl, Guid origin, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks)` forks a new branch whose head is the reconstructed commit at the cut.
- Auto: AS-OF reconstruction reads the op-log prefix up to the cut and folds it through the same `Crdt.Apply` the live path uses so a historical materialization is bit-identical to the live state at that instant — no second materializer; range diff reconstructs both cuts and set-differences the materialized maps by content-key inequality; blame folds the winning op per `(EntityKey, ColumnFamily)` by the `(Hlc, origin)` stamp the merge selected so authorship is the same ordering the convergence uses; scrub iterates the ordered op prefix as `ScrubFrame` values so a debugger steps the history; branch-from-past mints a `CommitNode` whose parents are the commit at the cut and appends one `branch` op so a past state becomes a live branch head; a checkpoint seals the materialized-state content address at a commit boundary so reconstruction folds from the nearest checkpoint forward rather than from genesis.
- Receipt: a reconstruction rides `store.timetravel.asof` carrying the op count folded, the checkpoint hit, and the cut instant; a branch-from-past rides `store.branch`; a checkpoint seal rides `store.timetravel.checkpoint`.
- Packages: NodaTime, LanguageExt.Core, System.IO.Hashing, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one column on `BlameRow`; zero new surface — a temporal-table mirror, a second history store, or a snapshot-per-instant materialization is the deleted form because reconstruction folds the op-log prefix the changefeed already holds and pins the heavy cuts to the content-addressed snapshots (`Snapshots.ContentAddress`) as `Checkpoint` fold anchors.
- Boundary: reconstruction is a pure left-fold of the op-log prefix through `Crdt.Apply`, so the AS-OF state at any instant is reproducible from the changefeed and the `Checkpoint` anchors — a checkpoint is a sealed snapshot whose `Hash` is the materialized-state content address at a commit boundary, so a deep history folds from the nearest checkpoint forward; `RangeDiff` reconstructs both endpoints and set-differences the materialized maps so a range diff is two AS-OF folds, never a stored delta chain, and a `Changed` key is one whose live `CrdtField` content key differs at the two cuts; `BlameRow` reads the same `(Hlc, origin)` winner the convergence selected so blame never disagrees with the materialized value; scrub frames are read-only op projections and a scrub never mutates the live state; branch-from-past consumes `CommitGraph.Commit` so a forked head is a real commit node on the DAG, and the cut instant rides `ClockPolicy`, never `DateTime.UtcNow`.

```csharp signature
public readonly record struct AsOfQuery(Instant Cut, Option<string> Branch, Option<string> EntityKeyPrefix) {
    public Hlc Ceiling => new(Cut, ulong.MaxValue);
}

public readonly record struct Checkpoint(Instant At, UInt128 Hash, HashMap<string, CrdtField> State, long OpCount);

public readonly record struct RangeDiff(
    Instant From, Instant To,
    ImmutableArray<string> Added, ImmutableArray<string> Removed, ImmutableArray<string> Changed);

public readonly record struct BlameRow(
    string EntityKey, string ColumnFamily, string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey);

public readonly record struct ScrubFrame(long Index, OpLogEntry Entry, Hlc At);

public static class TimeTravel {
    public static IO<HashMap<string, CrdtField>> Reconstruct(
        AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest) =>
        from anchor in nearest(query)
        from entries in upTo(query.Ceiling)
        let seed = anchor.Map(static c => c.State).IfNone(HashMap<string, CrdtField>())
        let floor = anchor.Map(static c => new Hlc(c.At, 0UL)).IfNone(Hlc.Zero)
        select entries
            .Filter(entry => query.Branch.Map(b => entry.ColumnFamily == b).IfNone(true) && entry.Physical <= query.Cut && new Hlc(entry.Physical, entry.Logical).CompareTo(floor) > 0)
            .Filter(entry => query.EntityKeyPrefix.Map(p => entry.EntityKey.StartsWith(p, StringComparison.Ordinal)).IfNone(true))
            .Fold(seed, static (state, entry) =>
                state.AddOrUpdate(
                    $"{entry.EntityKey}:{entry.ColumnFamily}",
                    existing => Fold(existing, entry),
                    Fold(new CrdtField.LwwRegister(default, Hlc.Zero, Guid.Empty), entry)));

    public static Checkpoint Seal(Instant at, HashMap<string, CrdtField> state, long opCount) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var slot in state.OrderBy(static s => s.Key, StringComparer.Ordinal))
            buffer.Write(MessagePackSerializer.Serialize(slot.Value));
        return new Checkpoint(at, XxHash128.HashToUInt128(buffer.WrittenSpan), state, opCount);
    }

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest) =>
        from a in Reconstruct(from, upTo, nearest)
        from b in Reconstruct(to, upTo, nearest)
        select new RangeDiff(
            from.Cut, to.Cut,
            [.. b.Keys.Where(k => !a.ContainsKey(k))],
            [.. a.Keys.Where(k => !b.ContainsKey(k))],
            [.. b.Keys.Where(k => a.Find(k).Map(v => Identity(v) != Identity(b[k])).IfNone(false))]);

    public static Seq<BlameRow> Blame(Seq<OpLogEntry> entries) =>
        toSeq(entries
            .GroupBy(static entry => (entry.EntityKey, entry.ColumnFamily))
            .Select(static group => group
                .OrderByDescending(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
                .First())
            .Select(static winner => new BlameRow(
                winner.EntityKey, winner.ColumnFamily, winner.Actor, winner.OriginStoreId,
                new Hlc(winner.Physical, winner.Logical), winner.ContentKey)));

    public static IO<Seq<ScrubFrame>> Scrub(AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo) =>
        upTo(query.Ceiling).Map(entries => toSeq(entries
            .OrderBy(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
            .Select((entry, index) => new ScrubFrame(index, entry, new Hlc(entry.Physical, entry.Logical)))));

    public static IO<BranchRef> BranchFromPast(
        AsOfQuery query, string newBranch, BranchAcl acl, Guid origin,
        Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks) =>
        from entries in upTo(query.Ceiling)
        let opKeys = toSeq(entries.Where(e => e.Physical <= query.Cut).Select(static e => e.ContentKey))
        let cell = new Hlc(clocks.Now, 0UL)
        let seed = new BranchRef(newBranch, RefKind.Branch, default, acl, origin, clocks.Now)
        let commit = CommitGraph.Commit(VersionVector.Empty, opKeys, seed, "branch-from-past", cell)
        from _ in appendCommit(commit)
        select seed with { Head = commit.ContentKey };

    private static UInt128 Identity(CrdtField field) =>
        XxHash128.HashToUInt128(MessagePackSerializer.Serialize(field).AsSpan());

    private static CrdtField Fold(CrdtField state, OpLogEntry entry) =>
        CrdtWire.Decode(entry.Payload).Match(
            Succ: op => Crdt.Apply(state, op),
            Fail: _ => Crdt.Apply(state, new CrdtOp.Set(entry.ColumnFamily, entry.Payload, new Hlc(entry.Physical, entry.Logical), entry.OriginStoreId)));
}
```

| [INDEX] | [POLICY]               | [VALUE]                          | [BINDING]                                            |
| :-----: | :--------------------- | :------------------------------- | :--------------------------------------------------- |
|   [1]   | checkpoint anchor      | sealed snapshot content address  | `Snapshots.ContentAddress`; fold floor               |
|   [2]   | as-of cut ceiling      | `(Cut, ulong.MaxValue)` HLC      | every op at-or-before the instant folds              |
|   [3]   | change-key diff        | `CrdtField` content-key inequality | two AS-OF folds, never a stored delta chain        |

## [6]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` an identity-keyed node in the model graph; `EditOp` `[Union]` the tree-edit-distance operation family; `MergeConflict` `[Union]` the typed conflict-class family; `StructuralMerge` the static surface owning node-identity matching, the cheapest-edit-script tree diff, the three-way merge, and the topological brep/mesh delta classification.
- Cases: `Match | Insert | Delete | Update | Move` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | TypeChange | TopologyBreak` on `MergeConflict`.
- Entry: `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` — the minimal edit script matching nodes by stable identity then by structural signature; `public static (Seq<EditOp> Merged, Seq<MergeConflict> Conflicts) ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs)` computes the base-relative edit scripts on both sides and folds non-overlapping edits while classifying overlaps into typed conflicts.
- Auto: node identity matches first on the stable element id (GUID), then on the geometry-hash and property-hash signatures so a moved-but-unchanged node matches by signature and a transformed node matches by id with a `Update`/`Move` op carrying the transform delta — the tree-edit distance is computed over the matched forest with `Match` for identity-equal nodes, `Update` for same-id-different-signature, `Move` for same-signature-different-parent, and `Insert`/`Delete` for unmatched; the three-way merge diffs base→ours and base→theirs, applies every edit touching a disjoint node set, and folds every edit touching a shared node into a `MergeConflict` whose class names the structural cause; topology delta classification reads the brep face/edge/vertex adjacency or the mesh half-edge adjacency from the geometry-hash signature so a face split or an edge collapse surfaces as a `TopologyBreak` rather than a value change.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count by class, and each `MergeConflict` projects to the settled `ConflictReceipt` so the inspector surface reads one conflict vocabulary.
- Packages: System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new edit op is one `EditOp` case; a new conflict class is one `MergeConflict` case; a new identity signature is one column on `GraphNode`; zero new surface — a line-based text diff, a JSON-patch diff over serialized nodes, or a second merge resolver is the deleted form because the diff matches on geometry/property identity and the merge classifies by structural cause; the existing set-difference `GraphDiff` (closure-membership transfer) stays the transport-level diff while this owns the semantic node-level diff, so the two diffs are altitude-split, never duplicated.
- Boundary: node identity is the GUID stable id first, then the `(GeometryHash, PropertyHash)` content signature so a re-imported model whose ids changed still matches unchanged nodes by signature — a positional or ordinal match is the deleted form; the geometry hash is `XxHash128` over the canonical topology encoding (brep face-edge-vertex adjacency or mesh half-edge adjacency, the same canonical bytes the `indexes#ARTIFACT_BLOB_INDEX` `IfcSemantic` and the Compute interchange produce) so a morph (same topology, moved control points) hashes differently from a topology break (changed adjacency), and the diff distinguishes `Update` (signature change at stable id) from `Move` (signature stable, parent change) from `TopologyBreak` (adjacency change); the three-way merge is base-relative — it never compares ours to theirs directly, so a node both sides left untouched never enters a conflict, and a node one side deleted while the other updated is a `DeleteUpdate` conflict the inspector resolves; the tree-edit distance is bounded to the matched forest so the cost is linear in the changed-node count, never quadratic over the whole graph; the diff operands are `GraphNode` projections of the federated entity graph (`federation#ENTITY_GRAPH`) so a structural diff rides the same node identity the federation keys on, and the merge result re-projects as `EditOp` rows applied through the CRDT algebra so a merged structural change converges like any other op.

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

## [7]-[TS_PROJECTION]

- Owner: `CommitNodeWire`, `BranchRefWire`, `VersionVectorWire`, `HlcWire`, `VectorOrderKind`, `CrdtOpWire`, `MerkleRangeWire`, `MergeConflictWire`, `BlameRowWire`, `RangeDiffWire` — the version-control wire surface the dashboard and peers decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: 64-bit fields decode as bigint under useBigInt64; content keys cross as 16-byte binary; instants cross as ISO-8601 extended strings; the `Hlc` crosses as `{ physical: string; logical: bigint }` so the TS leg compares causality without a wall clock; the `VersionVector` slots cross as a string-keyed bigint map (origin GUID string to sequence); the `CrdtOpWire` is the breaking wire-vocabulary amendment — it is a literal-discriminated union the TS-web and Python legs decode for `column-family=crdt` op-log rows, replacing the prior scalar payload assumption and adding the `write`/`increment`/`maintain` arms for the multi-value register, PN-counter, and tombstone compaction; `MergeConflictWire` and `BlameRowWire` reconstruct as literal unions from the case names; `MerkleRangeWire` carries the anti-entropy descent digest.

```ts contract
type VectorOrderKind = "Before" | "After" | "Concurrent" | "Equal";

type RefKindWire = "branch" | "tag" | "remote-tracking";

interface HlcWire {
  physical: string;
  logical: bigint;
}

interface VersionVectorWire {
  slots: Record<string, bigint>;
}

interface CommitNodeWire {
  contentKey: Uint8Array;
  parents: Uint8Array[];
  opKeys: Uint8Array[];
  branch: string;
  vector: VersionVectorWire;
  actor: string;
  cell: HlcWire;
}

interface BranchRefWire {
  name: string;
  kind: RefKindWire;
  head: Uint8Array;
  acl: number;
  origin: string;
  at: string;
}

interface MerkleRangeWire {
  low: Uint8Array;
  high: Uint8Array;
  digest: Uint8Array;
  count: number;
}

type CrdtOpWire =
  | { op: "set"; field: string; value: Uint8Array; cell: HlcWire; origin: string }
  | { op: "write"; field: string; value: Uint8Array; context: Record<string, bigint>; cell: HlcWire; origin: string }
  | { op: "add"; field: string; element: Uint8Array; tagOrigin: string; tagLogical: bigint }
  | { op: "remove"; field: string; element: Uint8Array; observedTags: { origin: string; logical: bigint }[] }
  | { op: "increment"; field: string; origin: string; delta: bigint }
  | { op: "insertAfter"; field: string; predecessor: { origin: string; logical: bigint }; id: { origin: string; logical: bigint }; value: Uint8Array }
  | { op: "delete"; field: string; id: { origin: string; logical: bigint } }
  | { op: "maintain"; field: string; quiescent: Record<string, bigint> };

type MergeConflictWire =
  | { kind: "ParallelEdit"; id: string }
  | { kind: "DeleteUpdate"; id: string; deletedByOurs: boolean }
  | { kind: "MoveMove"; id: string; ourParent: string | null; theirParent: string | null }
  | { kind: "TypeChange"; id: string; ourKind: string; theirKind: string }
  | { kind: "TopologyBreak"; id: string };

interface BlameRowWire {
  entityKey: string;
  columnFamily: string;
  actor: string;
  origin: string;
  cell: HlcWire;
  contentKey: Uint8Array;
}

interface RangeDiffWire {
  from: string;
  to: string;
  added: readonly string[];
  removed: readonly string[];
  changed: readonly string[];
}
```
