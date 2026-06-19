# [PERSISTENCE_VERSION_COMMITS]

Rasm.Persistence content-addressed history: a content-addressed commit-DAG with named branches, lightweight commits, and merge-base computation; a convergent op-based/delta-state CRDT replacing the LWW `Adjudicate` scalar with RGA sequence, add-wins observed-remove set, multi-value register, PN-counter, and LWW-by-HLC register types over the parametric DAG; an HLC `Hlc` stamp that is the one causal-ordering primitive shared by the op-log, the CRDT merge, and the wire seam; a `CrdtOpWire` op/CRDT encoding (HLC cell, op kinds, causal metadata) that amends the one-wire-vocabulary law field-for-field across the version-control owner and the AppHost wire seam; and the version-control wire projection of commit, branch, version-vector, op, conflict, blame, and Merkle range shapes. The op-log (`OpLogEntry`, HLC stamp, `Closure` manifest), the content-addressed snapshot identity (`Snapshots.ContentAddress`, `XxHash128`), the MessagePack codec profile (`ThinktectureMessageFormatterResolver`, `GeneratedMessagePackResolver`, `Lz4BlockArray`, `UntrustedData` restore lane), and the merge receipts (`ConflictReceipt`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive from AppHost. The `ContentParityCorpus` on `#CRDT_WIRE` mints the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` — the frozen HLC-cell, commit-key, CRDT-op, element-set-receipt, and embedding-seed bytes every cross-runtime parity harness reconciles against the one `XxHash128` seed.

## [01]-[INDEX]

- [01]-[COMMIT_DAG]: content-addressed commit-DAG, named branches, merge-base, and version vectors.
- [02]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, and LWW convergent CRDT.
- [03]-[CRDT_WIRE]: HLC stamp, `CrdtOp` codec, and `CrdtOpWire` op-log payload amendment.
- [04]-[TS_PROJECTION]: commit, branch, version-vector, op, conflict, blame, and Merkle wire shapes.

## [02]-[COMMIT_DAG]

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

| [INDEX] | [POLICY]               | [VALUE]                  | [BINDING]                                     |
| :-----: | :--------------------- | :----------------------- | :-------------------------------------------- |
|  [01]   | commit column family   | `commit`                 | one `OpLogEntry` per commit on the changefeed |
|  [02]   | branch column family   | `branch`                 | one `OpLogEntry` per ref mutation             |
|  [03]   | merkle fan-out         | 16 children per range    | `CommitGraph.Fanout`                          |
|  [04]   | merge-commit parentage | two parents, vector join | `VersionVector.Join` is the per-slot max      |


## [03]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` — the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload an `OpLogEntry` carries; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, plus the version-vector-gated tombstone compaction.
- Cases: `LwwRegister` (last-write-wins-by-HLC scalar), `MvRegister` (concurrent-keep multi-value register), `OrSet` (add-wins observed-remove set with per-element unique tags), `PnCounter` (positive-negative per-origin counter), `RgaSequence` (replicated growable array with tombstone-stable ordering), `EphemeralMap` (the convergent presence type — an add-wins observed-remove map of `(Guid Origin) → (ReadOnlyMemory<byte> State, Hlc Cell)` whose entries self-evict at the liveness horizon so a live-multiplayer cursor/selection/camera/follow surface converges and a departed peer drops without a tombstone) on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` — the join-semilattice least-upper-bound over two field states, total over the six cases and idempotent so replaying the same op converges; `public static CrdtField Apply(CrdtField state, CrdtOp op)` folds one delta op into the state carrying its HLC cell; `public static CrdtField Compact(CrdtField state, VersionVector quiescent)` reclaims tombstones and evicts `EphemeralMap` entries the quiescence horizon proves stale.
- Auto: a CRDT mutation appends one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the existing changefeed and the existing `Closure` content-key manifest, and a peer's `SyncMerge.Apply` dispatches the `column-family=crdt` op-log row into `Crdt.Apply` rather than the LWW `Adjudicate` scalar — the op-based CRDT supersedes LWW so a concurrent edit converges by merge rather than discarding the loser; the OR-set tags are `(Guid Origin, ulong Logical)` HLC pairs so an add and a concurrent remove of the same element resolve add-wins by tag-set difference; the RGA insert position is the causal predecessor element id plus the inserting origin's HLC so two concurrent inserts after the same predecessor order deterministically by `(Logical, Origin)`; the PN-counter folds per-origin increment maps so a counter converges by per-origin max of monotone partial counts; the MV-register keeps every causally-concurrent write and collapses a dominated write so a later causal write supersedes but a concurrent pair survives for caller resolution; the `EphemeralMap` keeps one entry per origin, an incoming `Beat` supersedes the held entry for that origin only when its `Hlc` cell strictly dominates so a stale reorder never rewinds a peer's live state, a `Leave` evicts the origin's entry add-wins-loses against a strictly-later `Beat`, and the version-vector-gated `Compact` evicts every entry whose `Hlc` the quiescence horizon dominates so a crashed peer that stops beating drops at the liveness horizon without a durable tombstone — the presence type is the one CRDT arm that converges live-multiplayer state on the wire vocabulary rather than the lossy awareness lane.
- Receipt: a converged merge rides the settled `SyncApplyReceipt`; a tombstone count, a live-element count, a compacted-tombstone count, and a live-presence count fold into the `store.crdt.merge` fact on the interceptor stream.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm; zero new surface — a per-type merge service, a second convergence engine, or an op-transform (OT) rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — this is the strict superset of the `Sync/collaboration#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries tombstones so a deleted element's position stays stable for later concurrent inserts and `Compact` reclaims a tombstone only when the supplied `VersionVector` quiescence horizon dominates the tombstone's HLC for every peer — the [CRDT_COMPACTION] proof harness replays permuted op multisets and confirms the LUB holds under tombstone removal; the `OrSet` is add-wins so a concurrent add-remove keeps the element and a remove only erases observed add-tags, never a later add; the `MvRegister` is a causal anti-chain — a write supersedes only writes its `VersionVector` dominates and keeps every concurrent value, so the register never silently discards a divergent edit; the `PnCounter` is two grow-only per-origin maps whose value is positive-minus-negative, monotone under merge so no decrement is ever lost; the RGA element id is `(Guid Origin, ulong Logical)` so identity is HLC-stable across peers and never positional; the `EphemeralMap` is per-origin-LWW-by-HLC under add-wins liveness — `Merge` keeps the strictly-later `(State, Cell)` per origin so two peers observing each other's beats converge to identical live state, `Leave` carries the departing `Hlc` so a `Leave` erases an entry only when no strictly-later `Beat` survives, and `Compact` is the durable-presence distinction from the lossy awareness lane: an entry whose `Hlc` the quiescence horizon dominates is a peer the whole fleet has stopped observing, so eviction is convergence-correct, never a dropped beat — the lossy `Sync/collaboration#PRESENCE_AND_BLOB` `Awareness` lane stays the 60-Hz fire-and-forget cursor stream while this is the converging self-expiring presence map a late-joining peer reconstructs from the op-log prefix; `Crdt.Merge` reads no wall clock — the HLC `Hlc` cell from the op-log stamp is the only ordering input, so convergence is deterministic; the CROSS_PACKAGE_LAW amendment this carries is owned at `#CRDT_WIRE`.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public int CompareTo(ElementId other) =>
        Logical.CompareTo(other.Logical) is var byLogical && byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
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
    public sealed record Beat(string Field, Guid Origin, ReadOnlyMemory<byte> State, Hlc Cell) : CrdtOp;
    public sealed record Leave(string Field, Guid Origin, Hlc Cell) : CrdtOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtField {
    private CrdtField() { }

    public sealed record LwwRegister(ReadOnlyMemory<byte> Value, Hlc Cell, Guid Origin) : CrdtField;
    public sealed record MvRegister(Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> Values) : CrdtField;
    public sealed record OrSet(HashMap<UInt128, Set<ElementId>> Live, Set<ElementId> Tombstoned) : CrdtField;
    public sealed record PnCounter(HashMap<Guid, long> Positive, HashMap<Guid, long> Negative) : CrdtField;
    public sealed record RgaSequence(Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone, Hlc Cell)> Cells) : CrdtField;
    public sealed record EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte> State, Hlc Cell)> Live) : CrdtField;
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
            (CrdtField.EphemeralMap l, CrdtField.EphemeralMap r) => new CrdtField.EphemeralMap(
                r.Live.Fold(l.Live, static (acc, slot) =>
                    acc.AddOrUpdate(slot.Key, held => held.Cell.CompareTo(slot.Value.Cell) >= 0 ? held : slot.Value, slot.Value))),
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
            (CrdtField.EphemeralMap map, CrdtOp.Beat beat) => new CrdtField.EphemeralMap(
                map.Live.AddOrUpdate(beat.Origin, held => held.Cell.CompareTo(beat.Cell) >= 0 ? held : (beat.State, beat.Cell), (beat.State, beat.Cell))),
            (CrdtField.EphemeralMap map, CrdtOp.Leave leave) => new CrdtField.EphemeralMap(
                map.Live.Find(leave.Origin).Filter(held => held.Cell.CompareTo(leave.Cell) > 0).IsSome ? map.Live : map.Live.Remove(leave.Origin)),
            (CrdtField.EphemeralMap map, CrdtOp.Maintain m) => Compact(map, m.Quiescent),
            _ => state,
        };

    public static CrdtField Compact(CrdtField state, VersionVector quiescent) =>
        state switch {
            CrdtField.RgaSequence seq => new CrdtField.RgaSequence(seq.Cells.Filter(cell =>
                !cell.Tombstone || quiescent.At(cell.Id.Origin) < (long)cell.Id.Logical)),
            CrdtField.EphemeralMap map => new CrdtField.EphemeralMap(map.Live.Filter((origin, slot) =>
                quiescent.At(origin) < (long)slot.Cell.Logical)),
            _ => state,
        };

    public static long Value(CrdtField.PnCounter counter) =>
        counter.Positive.Values.Sum() - counter.Negative.Values.Sum();

    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence seq) =>
        seq.Cells.Filter(static cell => !cell.Tombstone).Map(static cell => cell.Value);

    public static Seq<(Guid Origin, ReadOnlyMemory<byte> State)> Live(CrdtField.EphemeralMap map) =>
        toSeq(map.Live.Map(static (origin, slot) => (origin, slot.State)));

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

| [INDEX] | [TYPE]       | [CRDT_CLASS]                          | [CONVERGENCE]                                                                                            |
| :-----: | :----------- | :------------------------------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | LwwRegister  | last-write-wins by (HLC, origin)      | total order on the stamp tuple; superset of `Adjudicate`                                                 |
|  [02]   | MvRegister   | multi-value concurrent-keep           | causal anti-chain; dominated writes collapse                                                             |
|  [03]   | OrSet        | add-wins observed-remove set          | per-element tag-set union minus observed removes                                                         |
|  [04]   | PnCounter    | positive-negative per-origin          | per-origin max of monotone partial counts                                                                |
|  [05]   | RgaSequence  | replicated growable array             | tombstone-stable causal order; `Compact` reclaims at quiescence                                          |
|  [06]   | EphemeralMap | add-wins observed-remove presence map | per-origin LWW-by-HLC; `Beat` supersedes, `Leave` evicts, `Compact` self-expires at the liveness horizon |


## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp value — the one causal-ordering primitive the op-log stamp, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op/CRDT encoding the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `CrdtWire` the static codec surface owning the byte-canonical content key, the `Encode`/`Decode` pair through the settled `ThinktectureMessageFormatterResolver`+`GeneratedMessagePackResolver` chain, and the `UntrustedData` restore-lane decode; `ContentParityCorpus` the frozen-golden-bytes fixture owner minting the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` — one `ParityVector` table keyed by the one `XxHash128` seed every cross-runtime parity harness reconciles against.
- Cases: 10 op rows — `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave` on `CrdtOpWire`; the `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned; the `beat`/`leave` arms carry the `EphemeralMap` presence delta so the TS projection version-vector and the UI presence surface decode live-multiplayer state on the one wire vocabulary, and the Python CRDT decode reconstructs the same self-expiring presence map.
- Entry: `public static UInt128 ContentKey(CrdtOpWire op)` — the byte-canonical content key over the `None`-compression companion encoding so an identical op on two peers shares one identity across runtimes (the LZ4 `Write` lane is not byte-reproducible against a wasm/cp315 consumer, so the identity hashes the uncompressed companion bytes); `public static ReadOnlyMemory<byte> Encode(CrdtOp op)` writes the durable delta through the version-control resolver under `Lz4BlockArray` (C#-internal at-rest framing); `public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op)` writes the same delta under `MessagePackCompression.None` for the Python and TS consumers that decode uncompressed MessagePack; `public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload)` reads the delta under `MessagePackSecurity.UntrustedData` with the depth ceiling because a synced payload crossed a rest boundary.
- Auto: `Hlc.Advance` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter — the same `ReceiptSinkPort.Advance` algebra the op-log stamp and every receipt envelope ride; `CrdtWire.Encode` rides the durability codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes that the restore ladder and the snapshot codec already verify, never a second framing; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row plus one `CrdtOp` arm plus one map case, zero schema fork.
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the codec, content key, and HLC cell); a decode failure folds into the `store.crdt.decode` fault on the interceptor stream as a typed contract-drift rejection.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag plus one `[Key]` member plus one `Map`/`Lift` arm; `Lift` over the owned `CrdtOp` `[Union]` is the generated total `Switch` so a new `CrdtOp` case breaks the build rather than throwing the `<crdt-op-unmapped>` default, while `Map` over the foreign `[MessagePack.Union]` `CrdtOpWire` stays a language `switch` whose `_ => throw` is the contract-drift guard at the wire-decode boundary, not a lossy owned-union default; a retired tag is never reassigned because reuse silently re-types history; zero new surface — a typeless payload, a JSON-array delta, or a per-type formatter beside the resolver chain is the deleted form.
- Boundary: this is the FLAGSHIP CrdtOpWire amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` now carries a `CrdtOpWire` discriminated union for `column-family=crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `AppHost/runtime-ports#WIRE_LAW` with the TS-web `wire-consumption` leg and the Python `runtime/ServerHost` companion decoding the amended payload — recorded as a seam-split at the suite CROSS_PACKAGE_LAWS, never an additive parallel surface; the `Hlc` is one packed `(Instant Physical, ulong Logical)` whose ordering is `Physical` then `Logical` so two peers compare causality without a wall clock, and `WriteTo` emits the canonical 16-byte cell the commit content key and the op content key both hash; the wire `[Key]` sequence and the `[MessagePack.Union]` tags obey the durability retirement law so contract drift is a build diagnostic through the `MessagePackAnalyzer`, never a first-restore discovery; the restore lane reads under `UntrustedData` plus the object-graph depth ceiling because a synced delta's provenance is unprovable, while the write lane keeps the trusted default; `ContentKey` hashes the `None`-companion canonical bytes (never the LZ4 at-rest framing) so the op content key the `OpLogEntry` carries is byte-reproducible across the C#, Python, and TS runtimes and is the same `XxHash128` identity the structural diff and the federation keys consume; `ContentParityCorpus` freezes the Persistence leg of the one content-addressed golden corpus — the `Hlc.WriteTo` 16-byte cell (`Int64LE` `Physical.ToUnixTimeTicks()` then `UInt64LE` `Logical`, the VERIFIED layout), the canonical `(SortedParents, SortedOpKeys, Hlc)` commit-key preimage (`CommitGraph.Commit` @ `#COMMIT_DAG` byte-for-byte), and the `CrdtOpWire` op-set `None`-companion MessagePack encoding (the uncompressed cross-runtime lane the Python and TS legs decode, never the C#-internal `Lz4BlockArray` at-rest lane) under the one seed convention (`SeedOrigin = Guid.Empty`, `SeedInstant = Instant.FromUnixTimeTicks(0)`, seed-zero content), plus the cross-page byte shapes cited by reference — the sorted `ElementSet` receipt bytes (`Query/federation#ELEMENT_SET_ALGEBRA` `ElementSetAlgebra.Receipt`, `UInt128` keys distinct-sorted then `UInt128LE`-packed) and the `EmbeddingIdentity` seed (`Query/lanes#SEARCH_LANES` `EmbeddingIdentity.Of`, `XxHash128` over content × model-id × arity) — so a Python-recomputed CRDT op-set, a TS-read content key, and a C#-minted commit key reconcile against one frozen corpus and an off-by-one-half HLC encoding or a `(SortedParents, SortedOpKeys)` sort-order drift fails a single corpus assertion rather than silently folding a fresh op as stale across runtimes; the byte SHAPE, the field order, and the seed convention are ground truth and authorable now, the literal `XxHash128` digest values stamping on the host-validation pass (the OBSERVED digest is frozen, never an un-run asserted value); the corpus is a fixture-byte fence on this page, never a `.cs` test source file, and the Geometry `CSHARP_CANONICAL_ADJACENCY_FIXTURE` (the 52-byte single-triangle golden documented @ `#RESEARCH`) is a SIBLING corpus sharing the one `XxHash128` seed convention, never cross-authored here.

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
[MessagePack.Union(8, typeof(Beat))]
[MessagePack.Union(9, typeof(Leave))]
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
    [MessagePackObject]
    public sealed record Beat([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] ReadOnlyMemory<byte> State, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Leave([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical) : CrdtOpWire;
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

    private static readonly MessagePackSerializerOptions Companion =
        Write.WithCompression(MessagePackCompression.None);

    public static CrdtOpWire Lift(CrdtOp op) =>
        op.Switch<CrdtOpWire>(
            set:         static s => new CrdtOpWire.Set(s.Field, s.Value, s.Cell.Physical.ToUnixTimeTicks(), s.Cell.Logical, s.Origin),
            write:       static w => new CrdtOpWire.Write(w.Field, w.Value, [.. w.Context.Slots.Map(static (k, v) => (k, v))], w.Cell.Physical.ToUnixTimeTicks(), w.Cell.Logical, w.Origin),
            add:         static a => new CrdtOpWire.Add(a.Field, a.Element, a.Tag.Origin, a.Tag.Logical),
            remove:      static r => new CrdtOpWire.Remove(r.Field, r.Element, [.. r.ObservedTags.Map(static t => (t.Origin, t.Logical))]),
            increment:   static i => new CrdtOpWire.Increment(i.Field, i.Origin, i.Delta),
            insertAfter: static ins => new CrdtOpWire.InsertAfter(ins.Field, ins.Predecessor.Origin, ins.Predecessor.Logical, ins.Id.Origin, ins.Id.Logical, ins.Value),
            delete:      static d => new CrdtOpWire.Delete(d.Field, d.Id.Origin, d.Id.Logical),
            maintain:    static m => new CrdtOpWire.Maintain(m.Field, [.. m.Quiescent.Slots.Map(static (k, v) => (k, v))]),
            beat:        static b => new CrdtOpWire.Beat(b.Field, b.Origin, b.State, b.Cell.Physical.ToUnixTimeTicks(), b.Cell.Logical),
            leave:       static l => new CrdtOpWire.Leave(l.Field, l.Origin, l.Cell.Physical.ToUnixTimeTicks(), l.Cell.Logical));

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
            CrdtOpWire.Beat b => new CrdtOp.Beat(b.Field, b.Origin, b.State, new Hlc(Instant.FromUnixTimeTicks(b.PhysicalTicks), b.Logical)),
            CrdtOpWire.Leave l => new CrdtOp.Leave(l.Field, l.Origin, new Hlc(Instant.FromUnixTimeTicks(l.PhysicalTicks), l.Logical)),
            _ => throw new MessagePack.MessagePackSerializationException("<crdt-wire-unmapped>"),
        };

    public static ReadOnlyMemory<byte> Encode(CrdtOp op) =>
        MessagePackSerializer.Serialize(Lift(op), Write);

    public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op) =>
        MessagePackSerializer.Serialize(Lift(op), Companion);

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        Try.lift(() => Map(MessagePackSerializer.Deserialize<CrdtOpWire>(payload, Restore)))
            .Run()
            .MapFail(static error => Error.New(8261, $"<crdt-decode-drift:{error.Message}>"));

    public static UInt128 ContentKey(CrdtOpWire op) =>
        XxHash128.HashToUInt128(MessagePackSerializer.Serialize(op, Companion).Span);
}

public readonly record struct ParityVector(string Name, byte[] CanonicalBytes, UInt128 ContentKey);

public static class ContentParityCorpus {
    public static readonly Guid SeedOrigin = Guid.Empty;

    public static readonly Instant SeedInstant = Instant.FromUnixTimeTicks(0L);

    public static readonly Hlc SeedCell = new(SeedInstant, 0UL);

    public static byte[] HlcCell(Hlc cell) {
        var sink = new ArrayBufferWriter<byte>();
        cell.WriteTo(sink);
        return sink.WrittenSpan.ToArray();
    }

    public static byte[] CommitKeyPreimage(Seq<UInt128> sortedParents, Seq<UInt128> sortedOpKeys, Hlc cell) {
        var canonical = new ArrayBufferWriter<byte>();
        foreach (var parent in sortedParents.OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], parent);
        canonical.Advance(sortedParents.Count * 16);
        foreach (var key in sortedOpKeys.OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], key);
        canonical.Advance(sortedOpKeys.Count * 16);
        cell.WriteTo(canonical);
        return canonical.WrittenSpan.ToArray();
    }

    public static ParityVector Of(string name, byte[] canonicalBytes) =>
        new(name, canonicalBytes, XxHash128.HashToUInt128(canonicalBytes));

    public static Seq<ParityVector> Vectors() =>
        Seq(
            Of("hlc.zero", HlcCell(SeedCell)),
            Of("commit.genesis", CommitKeyPreimage(Seq<UInt128>(), Seq<UInt128>(), SeedCell)),
            Of("crdt.set.seed", CrdtWire.EncodeCompanion(new CrdtOp.Set("f", new byte[] { 1 }, SeedCell, SeedOrigin)).ToArray()),
            Of("elementset.empty", ElementSetParity()),
            Of("embedding.seed", EmbeddingParity()));

    private static byte[] ElementSetParity() {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in Seq<UInt128>(UInt128.One).OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], key);
        return buffer.WrittenSpan.ToArray();
    }

    private static byte[] EmbeddingParity() {
        var identity = EmbeddingIdentity.Of("rasm-parity-seed"u8, "rasm-embed-0", EmbeddingArity.Dense);
        var buffer = new ArrayBufferWriter<byte>();
        BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], identity.ContentHash);
        buffer.Advance(16);
        buffer.Write(Encoding.UTF8.GetBytes(identity.ModelId));
        buffer.Write(Encoding.UTF8.GetBytes(identity.Arity.Key));
        return buffer.WrittenSpan.ToArray();
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                          | [BINDING]                                               |
| :-----: | :------------------ | :------------------------------- | :------------------------------------------------------ |
|  [01]   | crdt column family  | `crdt`                           | `OpLogEntry.Payload` carries `CrdtOpWire`               |
|  [02]   | wire-law owner      | `AppHost/runtime-ports#WIRE_LAW` | breaking descriptor change; LWW survives as `set`       |
|  [03]   | restore decode lane | `UntrustedData` depth 64         | synced delta crossed a rest boundary                    |
|  [04]   | op content key      | `XxHash128` over canonical bytes | reproducible across peers; one identity                 |
|  [05]   | presence union tags | `beat`=8, `leave`=9              | `EphemeralMap` delta; append-only `[MessagePack.Union]` |

| [INDEX] | [PARITY_VECTOR]    | [CANONICAL_SHAPE]                                                            | [SEED / CROSS-PAGE]                                                                        |
| :-----: | :----------------- | :--------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `hlc.zero`         | `Int64LE` ticks then `UInt64LE` logical (16 bytes)                           | `SeedCell = (Instant.FromUnixTimeTicks(0), 0)`; layout VERIFIED                            |
|  [02]   | `commit.genesis`   | sorted parents `UInt128LE` ‖ sorted op-keys `UInt128LE` ‖ HLC cell           | `CommitGraph.Commit` canonical preimage @ `#COMMIT_DAG`                                    |
|  [03]   | `crdt.set.seed`    | `CrdtWire.EncodeCompanion(CrdtOp.Set)` MessagePack under `None`              | `SeedOrigin = Guid.Empty`; uncompressed cross-runtime lane                                 |
|  [04]   | `elementset.empty` | distinct-sorted `UInt128LE`-packed keys                                      | `Query/federation#ELEMENT_SET_ALGEBRA` `ElementSetAlgebra.Receipt`                         |
|  [05]   | `embedding.seed`   | `EmbeddingIdentity.Of` `ContentHash` `UInt128LE` ‖ UTF8 model-id ‖ arity key | `Query/lanes#SEARCH_LANES` `EmbeddingIdentity.Of` (`XxHash128` content × model-id × arity) |

The `XxHash128` digest column of each `ParityVector` is host-frozen: the byte SHAPE, field order, and seed convention above are ground truth and authorable now, the OBSERVED literal digest bytes stamping on the host-validation pass. The Python (`runtime/evidence/identity`) and TS (`interchange/Codec/frame`) reconciliation legs land in their own units against this corpus.


## [05]-[TS_PROJECTION]

- Owner: `CommitNodeWire`, `BranchRefWire`, `VersionVectorWire`, `HlcWire`, `VectorOrderKind`, `CrdtOpWire`, `MerkleRangeWire`, `MergeConflictWire`, `BlameRowWire`, `RangeDiffWire` — the version-control wire surface the dashboard and peers decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: 64-bit fields decode as bigint under useBigInt64; content keys cross as 16-byte binary; instants cross as ISO-8601 extended strings; the `Hlc` crosses as `{ physical: string; logical: bigint }` so the TS leg compares causality without a wall clock; the `VersionVector` slots cross as a string-keyed bigint map (origin GUID string to sequence); the `CrdtOpWire` is the breaking wire-vocabulary amendment — it is a literal-discriminated union the TS-web and Python legs decode for `column-family=crdt` op-log rows, replacing the prior scalar payload assumption and adding the `write`/`increment`/`maintain` arms for the multi-value register, PN-counter, and tombstone compaction, plus the `beat`/`leave` arms for the `EphemeralMap` presence type so the TS projection version-vector reconstructs the live-peer set and the UI presence surface renders each origin's converged cursor/selection/camera/follow state, a beat superseded by a strictly-later `cell` and an entry self-expiring when the quiescence horizon dominates its `cell`, so a late-joining peer materializes presence from the op-log prefix rather than the lossy awareness lane; `MergeConflictWire` and `BlameRowWire` reconstruct as literal unions from the case names; `MerkleRangeWire` carries the anti-entropy descent digest.

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
  | { op: "maintain"; field: string; quiescent: Record<string, bigint> }
  | { op: "beat"; field: string; origin: string; state: Uint8Array; cell: HlcWire }
  | { op: "leave"; field: string; origin: string; cell: HlcWire };

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

